using System;
using System.Linq;
using Kapman.World;

namespace Kapman.Game.Sim
{
    public enum RaidStage { None, Threat, Running, Caught }

    /// <summary>
    /// RAIDERS AT SEA, AGAINST THE MERCHANT (Plans/ai_fleets.md, phase 4) - a band made out by the lookout that has room
    /// for more and daylight to work in comes for him (the graph "Encounter - raiders"): they close at their best - a
    /// rowing sprint, then a steady pull, or their sail if the wind serves better - while he chooses. Running, his ship
    /// sails away from them at her best (VoyagePlanner); the chase ends when they're alongside (caught), when night
    /// falls, when the air thickens past the gap, or when they give up. His answers: turn and fight (the chance a^2 over
    /// a^2 + b^2, a and b the men each side), pay tribute, parley (in a shared tongue), or submit. Every outcome is a
    /// plain fact applied to the hold, the purse and the men; the merchant always lives (the user's decision for now).
    /// </summary>
    public static class RaiderEncounter
    {
        public static SimShip Band { get; private set; }
        public static RaidStage Stage { get; private set; }
        /// <summary>Game hours since they came on; why it ended ("" while it goes on).</summary>
        public static double ChaseHours { get; private set; }
        public static string Ended { get; private set; } = "";
        public static bool ParleyTried { get; private set; }
        /// <summary>Game metres apart at which they're alongside.</summary>
        public static float AlongsideGameM = 40f;
        static double lastGap = double.NaN, closing;
        static Random dice = new Random(793);

        static RaiderRules Rules => KapmanGame.Current?.World?.raiders;

        /// <summary>A band made out by the lookout comes for the merchant now (the graphs' "On raiders made out").</summary>
        public static event Action<SimShip> Coming;
        static double nextLook;
        /// <summary>What the looks are spaced by: game minutes where the game steps its clock (the chart), else real seconds.</summary>
        static double LookClock => GameSpeed.Stepped && KapmanGame.Current != null ? KapmanGame.Current.Hours * 60.0 : UnityEngine.Time.unscaledTime;

        /// <summary>Every frame (the voyage planner): a band the lookout has made out comes on as soon as the moment
        /// allows - nothing else on, no panel open - not only at the instant it's made out.</summary>
        public static void Look()
        {
            var w = KapmanGame.Current != null ? KapmanGame.Current.World : null;
            if (nextLook - LookClock > 10.0)
                nextLook = 0.0;                                              // (the clock went back: a loaded game)
            if (w == null || w.raiders == null || Stage != RaidStage.None || SeaWatch.Current == null || LookClock < nextLook)
                return;
            nextLook = LookClock + 0.5;
            foreach (var s in w.ships)
                if (s.raider && SeaWatch.Current.Of(s) == SeaWatch.Seen.MadeOut && CanThreaten(s))
                {
                    if (MapView.AwayFromSea)
                    {
                        // at the map: an urgent alert stops the game; they come on once you're back at sea
                        var game = KapmanGame.Current;
                        var me = SimStage.ToWorld(game.ShipPosition);          // ported 4 Oct: was FloatingOrigin.ToWorld
                        double km = Math.Sqrt((s.pos.x - me.x) * (s.pos.x - me.x) + (s.pos.z - me.z) * (s.pos.z - me.z)) / 1000.0;
                        Alerts.RaidersComing(s, km, SeaWatch.Airt(me.x, me.z, s.pos.x, s.pos.z));
                        return;
                    }
                    nextLook = LookClock + 5.0;                              // (the graph answers by calling Threaten)
                    Coming?.Invoke(s);
                    return;
                }
        }

        /// <summary>A band that has had its answer (fought, paid, given way to, or lost) does not come on again for so
        /// many game hours. A floor under the game, not its design: the looks are spaced by half a game minute, and with
        /// nothing here the same band came on again as soon as the window that told what came of it was shut.</summary>
        public const double LeaveBeHours = 12.0;
        static string leftBy = "";
        static double leftAt = double.NegativeInfinity;

        /// <summary>Will this band come for the merchant now: nothing else on (no meeting, no panel open), they have room
        /// and daylight, he's at sea, and they have not just had their answer from him.</summary>
        public static bool CanThreaten(SimShip s)
        {
            var game = KapmanGame.Current;
            if (Stage != RaidStage.None || Encounter.Stage != EncounterStage.None || s == null || !s.raider || s.state != ShipState.Sailing
                || game == null || game.ship == null || Rules == null)
                return false;
            if (s.id == leftBy && game.Hours >= leftAt && game.Hours - leftAt < LeaveBeHours)
                return false;
            if (TextPanel.Current != null && TextPanel.Current.Open)
                return false;
            if (s.hold.Free < s.hold.capacity * 0.2f)
                return false;                                                  // laden: making for a market
            var port = Port.Nearest(game.ShipPosition);
            if (port != null && port.DistanceTo(game.ShipPosition) < port.arrivalRadius * 2f)
                return false;
            return SunNow() > -6.0;
        }

        /// <summary>They come on: the sim leaves them to the chase.</summary>
        public static bool Threaten(SimShip s)
        {
            if (!CanThreaten(s))
                return false;
            Band = s;
            Band.chasing = true;
            Stage = RaidStage.Threat;
            ChaseHours = 0.0;
            Ended = "";
            ParleyTried = false;
            lastGap = double.NaN;
            closing = 0.0;
            dice = new Random(KapmanGame.Current.World.seed ^ (int)(KapmanGame.Current.Hours * 60.0));
            if (MapView.AwayFromSea)
                MapView.Current.Close();                                       // (urgent: back to the sea)
            return true;
        }

        public static void Run()
        {
            if (Stage == RaidStage.Threat)
                Stage = RaidStage.Running;
        }

        /// <summary>The chase is called off from outside (it has gone on past all sense: SeaHolds): they give up, and the
        /// graph's chase tells it as it tells any chase that ended.</summary>
        public static void GiveUp()
        {
            if (Stage == RaidStage.Running && string.IsNullOrEmpty(Ended))
                Ended = "They gave up the chase.";
        }

        /// <summary>Each frame of the chase: they close on her; it may end. `gameHours`: game time passed; `me`: the
        /// merchant's ship in the world (real metres: SimStage.ToWorld of her scene position).</summary>
        public static void Tick(double gameHours, WorldPos me)
        {
            if ((Stage != RaidStage.Threat && Stage != RaidStage.Running) || Band == null || gameHours <= 0.0)
                return;
            var r = Rules;
            ChaseHours += gameHours;
            double dx = me.x - Band.pos.x, dz = me.z - Band.pos.z, gap = Math.Sqrt(dx * dx + dz * dz);
            // their best: oars (a sprint, then a steady pull) or sail, if the wind serves better
            // ported 4 Oct: the wind and the air come from the stage (the level's weather), not LocalWeather
            SimStage.Wind(out float windFrom, out float windSpeed);
            double bearing = (Math.Atan2(dx, dz) * 180.0 / Math.PI + 360.0) % 360.0;
            double oars = (ChaseHours < r.sprintHours ? r.sprintKn : r.pullKn) * SimUnits.KnotMs;
            double sail = Travel.Speed(Band.cls, bearing, windFrom, windSpeed, out _);
            double v = Math.Max(oars, sail);
            double step = Math.Min(gap, SimUnits.RealMetresPerGameHour(v) * gameHours);
            if (gap > 1.0)
            {
                Band.pos = new WorldXZ(Band.pos.x + dx / gap * step, Band.pos.z + dz / gap * step);
                Band.prevPos = Band.pos;
                Band.heading = bearing;
            }
            gap -= step;
            if (!double.IsNaN(lastGap) && gameHours > 0.0)
                closing += ((lastGap - gap) / gameHours - closing) * Math.Min(1.0, gameHours * 6.0);   // m per game hour, eased
            lastGap = gap;
            if (gap < AlongsideGameM * SimUnits.Across)
            {
                Stage = RaidStage.Caught;
                return;
            }
            if (Stage != RaidStage.Running)
                return;
            double vis = SimStage.VisibilityKmTo(Band.pos) * 1000.0;         // (thick air between hides her)
            if (SunNow() <= -6.0)
                Ended = "Night fell and you lost them in the dark.";
            else if (vis < gap)
                Ended = "The air thickened and you lost them in it.";
            else if (ChaseHours >= r.giveUpHours)
                Ended = "They gave up the chase.";
            else if (gap > r.sightKm * 1000.0 * 1.2)
                Ended = "You drew away and lost them over the horizon.";
        }

        /// <summary>The chase in numbers, for the panel's status line.</summary>
        public static string Status()
        {
            var game = KapmanGame.Current;
            if (Band == null || game == null)
                return "";
            var me = SimStage.ToWorld(game.ShipPosition);
            double gap = Math.Sqrt((me.x - Band.pos.x) * (me.x - Band.pos.x) + (me.z - Band.pos.z) * (me.z - Band.pos.z)) / 1000.0;
            double kn = closing / 1920.0 / SimUnits.KnotMs;                     // (real m a game hour -> knots)
            string sun = SunsetIn(game, out double h) ? $" Sunset in {(int)h} h {(int)((h - (int)h) * 60):00} min." : "";
            string who = $"{Cap(SeaWatch.Label(Band))}: {Band.bandShips} {(Band.bandShips == 1 ? "ship" : "ships")}, about {Band.men} men.";
            if (Stage == RaidStage.Caught)
                return who + $" They're alongside. You have {Voyage.Men} men.";
            string trend = Math.Abs(kn) < 0.05 ? "holding the gap" : kn > 0 ? $"closing at {kn:0.0} knots" : $"falling back at {-kn:0.0} knots";
            return who + $" {gap:0.0} km off, {trend}.{sun} {ChaseHours:0.0} hours since they came on.";
        }

        // ------------------------------------------------------------------------------------------ the answers

        /// <summary>The chance that a men beat b (Lanchester's square law: a^2 / (a^2 + b^2)).</summary>
        public static double FightOdds(double a, double b) => a * a + b * b > 0.0 ? a * a / (a * a + b * b) : 0.5;

        /// <summary>Turn and fight: the chance a^2/(a^2+b^2). Won: they sheer off, a man or none lost. Lost: as if
        /// submitting, and men lost besides.</summary>
        public static string Fight()
        {
            var r = Rules;
            if (Band == null || r == null)
                return End("");
            double p = FightOdds(Voyage.Men, Band.men);
            Deeds.Record("fought_raiders", "turned and fought raiders", Deeds.NearestPlace());
            if (dice.NextDouble() < p)
            {
                int lost = Voyage.LoseMen(dice.Next(r.winFightMenMin, r.winFightMenMax + 1));
                Band.men = Math.Max(1, Band.men - (int)Math.Ceiling(Band.men * 0.2));
                return End($"You fought them off (the odds were {p * 100:0}%). {Men(lost)} lost. They sheer away.");
            }
            int dead = Voyage.LoseMen(dice.Next(r.loseFightMenMin, r.loseFightMenMax + 1));
            string taken = Plunder(r.submitCargoShare, r.submitSilverShare);
            return End($"You fought and lost (the odds were {p * 100:0}%). {Men(dead)} lost. {taken}");
        }

        /// <summary>Pay them to go: a share of the silver, at least the table's least; if the purse can't, they take
        /// what they want.</summary>
        public static string PayTribute()
        {
            var r = Rules;
            if (Band == null || r == null || !Trade.Started)
                return End("");
            double silver = Trade.Purse.Silver(TradeRules.Current);
            double want = Math.Max(r.tributeMinD, silver * r.tributeShare);
            if (silver < want)
                return End("You hadn't the silver they asked. " + Plunder(r.submitCargoShare, r.submitSilverShare));
            double paid = Trade.Purse.PayUpTo(want, TradeRules.Current);
            Band.purse.Add("hacksilver", paid * 1.7);
            Deeds.Record("paid_raiders", "paid raiders to go", Deeds.NearestPlace());
            return End($"You paid them {paid:0} deniers' worth of silver to go. They let you go.");
        }

        /// <summary>Talk: in a shared tongue there's a chance they take a small tribute and go; if not, they won't talk
        /// again.</summary>
        public static string Parley(out bool settled)
        {
            var r = Rules;
            settled = false;
            if (Band == null || r == null)
            {
                settled = true;
                return End("");
            }
            ParleyTried = true;
            string tongue = r.Origin(Band.origin)?.tongue ?? "other";
            double chance = r.parley.TryGetValue(tongue, out double c) ? c : r.parley.TryGetValue("other", out double o) ? o : 0.15;
            if (dice.NextDouble() < chance)
            {
                settled = true;
                double paid = Trade.Started ? Trade.Purse.TakeShare(r.parleyTributeShare, TradeRules.Current) : 0.0;
                Band.purse.Add("hacksilver", paid * 1.7);
                Deeds.Record("paid_raiders", "paid raiders to go", Deeds.NearestPlace());
                return End($"You talked them round, in their own tongue. They took {paid:0} deniers' worth of silver and let you go.");
            }
            return "They won't talk.";
        }

        /// <summary>Give up without a fight: they take the cargo and most of the silver; the men are spared.</summary>
        public static string Submit()
        {
            var r = Rules;
            if (Band == null || r == null)
                return End("");
            return End("You gave up without a fight. " + Plunder(r.submitCargoShare, r.submitSilverShare));
        }

        /// <summary>The chase ended without them (night, the murk, they gave up, you drew away).</summary>
        public static string Escaped() => End(string.IsNullOrEmpty(Ended) ? "They're gone." : Ended);

        /// <summary>The cargo (a share of each good, their room allowing) and a share of the silver go to the band.</summary>
        static string Plunder(double cargoShare, double silverShare)
        {
            if (!Trade.Started)
                return "";
            int units = 0;
            var gone = new System.Collections.Generic.List<string>();
            foreach (var (good, count) in Trade.Hold.Contents.ToList())
            {
                int n = Math.Min((int)Math.Ceiling(count * cargoShare), Band.hold.RoomFor(good));
                if (n <= 0)
                    continue;
                Trade.Hold.Remove(good, n);
                Band.hold.Add(good, n, 0.0, KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0);
                units += n;
                gone.Add(Trade.UnitsNamed(good, n));
            }
            double silver = Trade.Purse.TakeShare(silverShare, TradeRules.Current);
            Band.purse.Add("hacksilver", silver * 1.7);
            return (units > 0 ? "They took " + string.Join(", ", gone) : "They found no cargo worth taking") +
                   $", and {silver:0} deniers' worth of silver.";
        }

        static string End(string fact)
        {
            if (Band != null)
            {
                Band.chasing = false;
                KapmanGame.Current?.World?.Rejoin(Band);
                leftBy = Band.id ?? "";
                leftAt = KapmanGame.Current != null ? KapmanGame.Current.Hours : double.NegativeInfinity;
            }
            Band = null;
            Stage = RaidStage.None;
            if (!string.IsNullOrEmpty(fact))
                Voyage.Say(fact);
            return fact;
        }

        static string Men(int n) => n == 0 ? "No men" : n == 1 ? "One man" : $"{n} men";

        // ported 4 Oct: the sun by the stage's clock (the level's), where the merchant's ship is in the world
        static double SunNow()
        {
            var game = KapmanGame.Current;
            if (game == null)
                return 30.0;
            return SimStage.SunElevationAt(SimStage.ToWorld(game.ShipPosition));
        }

        /// <summary>Game hours till the sun is 6 degrees below the horizon today (false if it's down or doesn't set).</summary>
        static bool SunsetIn(KapmanGame game, out double hours)
        {
            hours = 0.0;
            var me = SimStage.ToWorld(game.ShipPosition);
            var (lat, _) = WorldFrame.ToLatLon(me.x, me.z);
            double h0 = SimStage.HourOfDay;
            int dayOfYear = SimStage.DayOfYear;
            for (double h = h0; h < 24.0; h += 0.1)
                if (Sight.SunElevation(lat, dayOfYear, h) <= -6.0)
                {
                    hours = h - h0;
                    return hours > 0.0;
                }
            return false;
        }

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Band = null;
            Stage = RaidStage.None;
            Ended = "";
            leftBy = "";
            leftAt = double.NegativeInfinity;
        }
    }
}
