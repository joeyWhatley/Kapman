using System;
using System.Collections.Generic;
using System.Text;
using Kapman.World;

namespace Kapman.Game.Sim
{
    public enum EncounterStage { None, Offered, Closing, Alongside }

    /// <summary>
    /// MEETING A SHIP AT SEA (Plans/ai_fleets.md, phase 3) - a trader made out by the lookout can be met: she is offered
    /// (the graph "Encounter - a trader" shows her facts and the choice); "close and hail" has her heave to and the
    /// merchant's ship make for her (VoyagePlanner); alongside, the game's clock is held (the sea keeps moving) while
    /// they talk: the weather she met on her way (the weather model along her track), and the prices she knows (into
    /// the ledger as heard, where newer than what's in it); then they part, she sails on and the merchant's course
    /// resumes. Facts only; your writing for each key goes in writing.json.
    /// </summary>
    public static class Encounter
    {
        public static SimShip Ship { get; private set; }
        public static EncounterStage Stage { get; private set; }
        /// <summary>What she last told (the weather, the prices), shown on the hail's menu.</summary>
        public static string LastNews = "";
        /// <summary>Game metres apart at which the two ships are alongside.</summary>
        public static float AlongsideGameM = 40f;
        public static event Action Changed;

        /// <summary>Can a meeting with her be offered now: nothing else on, she's under way, the merchant at sea (not lying
        /// in a port, no panel open).</summary>
        public static bool CanOffer(SimShip s) => CanOffer(s, false);

        /// <summary>As <see cref="CanOffer(SimShip)"/>; `ignoreMap`: as if the map weren't open (the map's alert asks).</summary>
        public static bool CanOffer(SimShip s, bool ignoreMap)
        {
            var game = KapmanGame.Current;
            if (Stage != EncounterStage.None || s == null || s.raider || s.state != ShipState.Sailing || game == null || game.ship == null
                || RaiderEncounter.Stage != RaidStage.None)
                return false;
            if (TextPanel.Current != null && TextPanel.Current.Open)
                return false;
            if (!ignoreMap && MapView.AwayFromSea)
                return false;
            if (Patrols.Stage != PatrolStage.None)
                return false;
            var port = Port.Nearest(game.ShipPosition);
            return port == null || port.DistanceTo(game.ShipPosition) > port.arrivalRadius * 2f;
        }

        /// <summary>THE PACE ON THE CHART (7 Oct 2026): game hours between two meetings offered by themselves. On the
        /// Rhine a sail is made out every few minutes, and each offer is a window that holds the clock. A sail the
        /// player hails (<see cref="Hail"/>) is offered at any time. Not ruled by the user: the builder's number.</summary>
        public static double ChartOfferGapHours = 12.0;
        static double lastOffered = double.NegativeInfinity;
        static SimShip hailed;

        /// <summary>Game hours since a meeting was last offered (a great many before the first).</summary>
        public static double HoursSinceOffer => KapmanGame.Current != null ? KapmanGame.Current.Hours - lastOffered : double.PositiveInfinity;

        /// <summary>The player hails a sail made out (the frame's Hail button): the lookout tells of her again, and the
        /// meeting is offered whatever the pace. False if she cannot be met now.</summary>
        public static bool Hail(SimShip s)
        {
            if (!CanOffer(s))
                return false;
            hailed = s;
            SeaWatch.Retell(s);                                             // (the graphs offer the meeting as usual)
            hailed = null;
            return Ship == s && Stage == EncounterStage.Offered;
        }

        public static bool Offer(SimShip s)
        {
            if (!CanOffer(s))
                return false;
            var game = KapmanGame.Current;
            bool chart = MapView.Current != null && MapView.Current.chartMode;
            if (chart && s != hailed && game != null && game.Hours - lastOffered < ChartOfferGapHours)
                return false;                                               // (too soon after the last: she passes unhailed)
            if (game != null)
                lastOffered = game.Hours;
            Ship = s;
            Stage = EncounterStage.Offered;
            Changed?.Invoke();
            return true;
        }

        /// <summary>A saved game was loaded: the pace begins again (the hours of the game before mean nothing).</summary>
        public static void Loaded()
        {
            lastOffered = double.NegativeInfinity;
            hailed = null;
        }

        /// <summary>She heaves to; the merchant's ship makes for her.</summary>
        public static void CloseAndHail()
        {
            if (Stage != EncounterStage.Offered || Ship == null)
                return;
            Ship.heaveTo = true;
            Stage = EncounterStage.Closing;
            Changed?.Invoke();
        }

        /// <summary>Alongside: the clock is held while they talk.</summary>
        public static void ComeAlongside()
        {
            if (Stage != EncounterStage.Closing)
                return;
            Stage = EncounterStage.Alongside;
            var game = KapmanGame.Current;
            SimStage.HoldClock(true);                                       // ported 4 Oct: was game.clock.held = true
            if (game != null)
                Known.Spoke(Ship, game.Day);                                    // (where she's bound and what she carries: known now)
            Changed?.Invoke();
        }

        /// <summary>They part (or the offer is let go): she sails on, the clock runs, the course resumes.</summary>
        public static void Part()
        {
            if (Ship != null)
                Ship.heaveTo = false;
            Ship = null;
            Stage = EncounterStage.None;
            LastNews = "";
            ForSale = null;
            SimStage.HoldClock(false);                                      // ported 4 Oct: was game.clock.held = false
            Changed?.Invoke();
        }

        /// <summary>Real metres from the merchant's ship to her (where the sim has her now).</summary>
        public static double DistanceM()
        {
            var game = KapmanGame.Current;
            if (Ship == null || game == null)
                return double.MaxValue;
            var me = SimStage.ToWorld(game.ShipPosition);                  // ported 4 Oct: was FloatingOrigin.ToWorld
            return Math.Sqrt((Ship.pos.x - me.x) * (Ship.pos.x - me.x) + (Ship.pos.z - me.z) * (Ship.pos.z - me.z));
        }

        /// <summary>Her facts, as far as they're known: her kind and home (made out), how far and which way, how she sits
        /// in the water; once spoken to, where she's bound and from where, and what she carries.</summary>
        public static string Facts(SimShip s)
        {
            if (s == null)
                return "";
            var game = KapmanGame.Current;
            var me = game != null ? SimStage.ToWorld(game.ShipPosition) : default;
            double km = Math.Sqrt((s.pos.x - me.x) * (s.pos.x - me.x) + (s.pos.z - me.z) * (s.pos.z - me.z)) / 1000.0;
            var b = new StringBuilder();
            b.Append(Cap(SeaWatch.Label(s))).Append('.');
            bool spoken = Known.HasSpoken(s.id) || (s == Ship && Stage == EncounterStage.Alongside);
            if (spoken && s.dest >= 0)
                b.Append(" Bound for ").Append(GameData.Ports[s.dest].name).Append(", from ").Append(GameData.Ports[s.at].name).Append('.');
            b.Append($" {km:0.#} km to the {SeaWatch.Airt(me.x, me.z, s.pos.x, s.pos.z)}.");
            if (spoken)
                b.Append(string.IsNullOrEmpty(s.cargoNote) ? " No cargo aboard." : " Laden: " + s.cargoNote.Replace("_", " ") + ".");
            else
                b.Append(s.hold.Used > s.hold.capacity * 0.4f ? " She sits deep: laden." : " She rides high: light, or in ballast.");
            return b.ToString();
        }

        /// <summary>The weather she met on her way, every 6 game hours back to 48 (the weather model where she was then),
        /// as facts; where she was is told from the merchant's ship now.</summary>
        public static string WeatherNews(WorldSim w, SimShip s, int everyH = 6, int backH = 48)
        {
            if (w == null || s == null || s.path.Count < 2)
                return "She has no news of the weather.";
            var game = KapmanGame.Current;
            var me = game != null ? SimStage.ToWorld(game.ShipPosition) : default;
            double hoursOut = Math.Max(1.0, (w.Day - s.departedDay) * 24.0);
            double perHour = s.done / hoursOut;                          // real metres an hour, on average this passage
            var b = new StringBuilder();
            for (int h = everyH; h <= backH; h += everyH)
            {
                double along = s.done - perHour * h;
                string where;
                WorldXZ p;
                if (along <= 0.0)
                {
                    p = s.path[0];
                    where = "in port at " + GameData.Ports[s.at].name;
                }
                else
                {
                    p = w.PointAlong(s, along);
                    double km = Math.Sqrt((p.x - me.x) * (p.x - me.x) + (p.z - me.z) * (p.z - me.z)) / 1000.0;
                    where = $"{km:0} km to the {SeaWatch.Airt(me.x, me.z, p.x, p.z)} of you";
                }
                var (lat, lon) = WorldFrame.ToLatLon(p.x, p.z);
                var ws = w.weather.Sample(lat, lon, w.WeatherHours - h, along > 0.0);
                b.Append($"{h} hours ago, {where}: wind from the {Compass(ws.windFrom)}, {ws.windSpeed / 0.514444:0} knots; {ws.Describe()}.\n");
                if (along <= 0.0)
                    break;
            }
            return b.ToString().TrimEnd();
        }

        /// <summary>The prices she knows go into the merchant's price notes, as told by her: as a witness where she saw them
        /// herself, else as talk; and her news, as hers. Returns the facts: which ports, how old.</summary>
        public static string PricesHeard(WorldSim w, SimShip s)
        {
            if (w == null || s == null || !Trade.Started)
                return "She has no prices to tell.";
            var told = new List<string>();
            string who = SeaWatch.Label(s);
            foreach (var snap in s.knows.All)
            {
                bool witness = snap.seenBy == s.id;
                if (Trade.Ledger.Hear(snap, (witness ? "told by " : "heard from ") + who, witness ? How.Told : How.Talk, witness ? 0 : 1, w.Day,
                                      "skipper", s.home >= 0 && s.home < GameData.Ports.Count ? GameData.Ports[s.home].id : null))
                {
                    told.Add($"{GameData.Ports[snap.port].name}, {Old(w.Day - snap.day)}");
                    Known.HearOf(GameData.Ports[snap.port]);
                }
            }
            foreach (var (item, learned, from) in s.knows.News)
                Known.HearNews(item, w.Day, "heard from " + who, How.Talk, from == "her own crew" ? 1 : 2);
            string riddle = Chest.GivenAtSea(s, w.Day);                    // (now and then a master has a riddle to give)
            return (told.Count == 0 ? "She knows nothing newer than what's in your ledger."
                                    : $"She tells you what {told.Count} {(told.Count == 1 ? "port was" : "ports were")} paying: {string.Join("; ", told)}.")
                   + (riddle.Length > 0 ? " " + riddle : "");
        }

        /// <summary>How old a thing told is, in plain words: "today's", "1 day old", "14 days old".</summary>
        static string Old(double days)
        {
            int n = (int)Math.Round(Math.Max(0.0, days));
            return n == 0 ? "today's" : n == 1 ? "1 day old" : n + " days old";
        }

        /// <summary>What she'd sell from her hold now: (good, units, deniers each), or null.</summary>
        public static (GoodDef good, int count, double each)? ForSale { get; private set; }

        /// <summary>She offers a little from her hold: up to a tenth of her biggest lot (the merchant's room and silver
        /// allowing), at what it fetches where she's bound less 15% (no toll at sea). The facts of the offer.</summary>
        public static string MakeOffer(WorldSim w, SimShip s, double share = 0.1, double below = 0.15)
        {
            ForSale = null;
            if (w == null || s == null || !Trade.Started)
                return "She has nothing to sell.";
            GoodDef best = null;
            int most = 0;
            foreach (var (good, count) in s.hold.Contents)
                if (count > most)
                {
                    most = count;
                    best = good;
                }
            int n = best == null ? 0 : Math.Min((int)Math.Floor(most * share), Trade.Hold.RoomFor(best));
            if (n <= 0)
                return best == null ? "She has nothing to sell." : "You've no room aboard for what she has.";
            // at what she believes it fetches where she's bound (what she's seen there, fading to what everyone knows), less
            // a share: her belief, not the market's truth (asking the market would tell what nobody at sea could know)
            var dest = s.dest >= 0 ? GameData.Ports[s.dest] : GameData.Ports[s.at];
            int month = w.Date.month;
            float usual = Lore.UsualPays(dest, best, month), believed = usual;
            var snap = s.knows.Of(dest.index);
            if (snap != null && !float.IsNaN(snap.pays[best.index]))
            {
                float wt = Known.Weight(w.Day - snap.day, w.rules.beliefHalfLifeDays);
                believed = wt * snap.pays[best.index] + (1f - wt) * usual;
            }
            double each = Math.Round(Math.Max(0.05f, believed) * (1.0 - below), 2);
            double silver = Trade.Purse.Silver(TradeRules.Current);
            n = Math.Min(n, (int)Math.Floor(silver / Math.Max(0.01, each)));
            if (n <= 0)
                return $"She'd sell {best.name.ToLowerInvariant()}, but you haven't the silver.";
            ForSale = (best, n, each);
            return $"She'll sell {Trade.Units(best, n)} at {each:0.##} deniers each, {n * each:0.##} in all.";
        }

        /// <summary>Takes her offer: the goods into your hold, the silver to her purse. The fact of it.</summary>
        public static string TakeOffer()
        {
            if (ForSale == null || Ship == null || !Trade.Started)
                return "There's no offer to take.";
            var (good, n, each) = ForSale.Value;
            ForSale = null;
            n = Math.Min(n, Ship.hold.Count(good.id));
            if (n <= 0)
                return "She has none left.";
            double total = Math.Round(n * each, 2);
            if (Trade.Purse.Silver(TradeRules.Current) < total)
                return "You haven't the silver.";
            Trade.Purse.PayUpTo(total, TradeRules.Current);
            Ship.hold.Remove(good, n);
            Ship.purse.Add("hacksilver", total * 1.7);
            Trade.Hold.Add(good, n, total, KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0);
            return $"You bought {Trade.Units(good, n)} from her for {total:0.##} deniers' worth of silver.";
        }

        static readonly string[] Points16 = { "north", "north-north-east", "north-east", "east-north-east", "east", "east-south-east",
            "south-east", "south-south-east", "south", "south-south-west", "south-west", "west-south-west", "west", "west-north-west",
            "north-west", "north-north-west" };

        static string Compass(double deg) => Points16[(int)Math.Round(((deg % 360.0) + 360.0) % 360.0 / 22.5) % 16];

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Ship = null;
            Stage = EncounterStage.None;
            Changed = null;
            lastOffered = double.NegativeInfinity;
            hailed = null;
        }
    }
}
