using System;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE BOAT IN THE LIVE GAME - each game hour (KapmanGame's tick ladder: every hour once, in order, at every speed of
    /// the clock) it reads the weather where she is and for the hours ahead (the world's own model, the one the other
    /// ships sail by), hands the hour to <see cref="Boat.Hour"/>, and does what the boat cannot do itself: says the
    /// facts in the log, drops the clock and opens the card when a sign of a gale comes, lets the water take its room
    /// in the hold and spoil what stands in it, reckons the men's share at Martinmas, and ends the game when she is lost.
    /// </summary>
    public static class BoatWatch
    {
        /// <summary>The card the second sign opens (the swell: shorten sail, turn for a lee, or hold on).</summary>
        public const string GaleCard = "sea.swell.long";
        /// <summary>The card the last sign opens (the gale: lie to, run under shortened sail, or carry it).</summary>
        public const string OrderCard = "sea.gale.breaks";
        /// <summary>The game is over (how she was lost): the frame shows the last page.</summary>
        public static event Action<string> GameOver;

        static bool hooked;

        static void Hook()
        {
            if (hooked)
                return;
            hooked = true;
            Boat.Said += Voyage.Say;
            Boat.SignCame += OnSign;
            Boat.Lost += OnLost;
            KapmanGame.Jumped += (from, to) => Boat.Grace((long)Math.Ceiling(to) + 1);   // (a jump of the clock is not play)
        }

        /// <summary>Hours to the first hour of wind over the gale line at a place, looking `look` hours ahead (infinity:
        /// none). The weather model answers for any hour, so this is no guess: it is what will blow there.</summary>
        public static double GaleIn(WorldSim world, WorldXZ at, double look)
        {
            if (world == null || world.weather == null)
                return double.PositiveInfinity;
            var (lat, lon) = WorldFrame.ToLatLon(at.x, at.z);
            for (int k = 1; k <= (int)look; k++)
                if (world.weather.Sample(lat, lon, world.WeatherHours + k, true).windSpeed > GaleDice.GaleWind)
                    return k;
            return double.PositiveInfinity;
        }

        /// <summary>
        /// Hours to the first hour of gale WHERE SHE WILL BE: the weather is asked for each hour ahead at the point of her
        /// charted course she reaches by then at the way she is making (her own place when she has no course or makes
        /// none). The play-through of 9 Oct 2026: asked only where she lay, the first sign of a gale she was sailing
        /// into came "within the hour".
        /// </summary>
        public static double GaleAhead(WorldSim world, WorldXZ at, double metresAnHour, double look)
        {
            if (world == null || world.weather == null)
                return double.PositiveInfinity;
            bool along = Course.IsSet && metresAnHour > 1.0;
            for (int k = 1; k <= (int)look; k++)
            {
                var p = along ? Course.Aim(at, metresAnHour * k, out _) : at;
                var (lat, lon) = WorldFrame.ToLatLon(p.x, p.z);
                if (world.weather.Sample(lat, lon, world.WeatherHours + k, true).windSpeed > GaleDice.GaleWind)
                    return k;
            }
            return double.PositiveInfinity;
        }

        /// <summary>The cards of her wounds, offered when they befall her: the split sail, and the water over the floor timbers.</summary>
        public const string SailCard = "sea.sail.splits", LeakCard = "sea.leak.gains";

        /// <summary>The house's worth now, deniers: silver and the cargo at the table's values, less the debt.</summary>
        public static double Worth()
        {
            if (!Trade.Started)
                return 0.0;
            double v = Trade.Purse.Silver(TradeRules.Current) - Trade.Debt;
            foreach (var (good, count) in Trade.Hold.Contents)
                v += count * (double)good.value;
            return v;
        }

        /// <summary>A game hour has passed (KapmanGame.Ticks.Hour, after the world's ships have had theirs).</summary>
        public static void Hour(long hour)
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            var body = ChartBody.Current;
            if (game == null || world == null || game.ship == null || body == null || !Voyage.Started || Boat.IsLost)
                return;
            Hook();
            var cls = world.ClassOf(body.classId);
            if (cls == null)
                return;
            var at = ChartWorld.ShipWorld();
            var w = world.WeatherAt(at);
            bool moored = Port.LyingId() != null;
            double hourOfDay = ((game.Hours % 24.0) + 24.0) % 24.0;
            double off = Math.Abs(((game.ship.Heading - w.windFrom) % 360.0 + 540.0) % 360.0 - 180.0);
            Boat.Hour(new BoatHour
            {
                hour = hour, wind = w.windSpeed, offWind = off,
                underWay = !moored && !game.ship.anchored && Course.IsSet, moored = moored, river = ChartWorld.OnRiver(),
                night = hourOfDay < 5.0 || hourOfDay >= 21.0, hardOars = Oars.Out,
                galeIn = moored ? double.PositiveInfinity : GaleAhead(world, at, body.Knots * 1852.0, Boat.R.lookAhead),
                men = Voyage.Men, hands = Mathf.Max(1, Mathf.RoundToInt(cls.crew)),
                length = cls.length, beam = cls.beam, fragility = cls.galeFragility, lossPerHour = world.rules.galeLossPerHour,
                seed = world.seed,
            });
            // WHAT HAS BEFALLEN HER IS TOLD AND ASKED AT ONCE, as the gale's signs are (the play-through of 9 Oct 2026: her
            // sail split on 1 August and its card, left to the deck's own chance, had not come by the 13th: she crawled
            // into Quentovic on half a sail with nothing to be done about it). The card comes first in line when it can
            // (its own "when" and its days of rest hold), and again after those days while the wound stands.
            var cards = EncounterRunner.Director;
            if (cards != null && !moored)
            {
                if (Boat.Cloth == SailCloth.Split)
                    cards.Offer(SailCard);
                if (Boat.Water >= Boat.R.spoilFrom)
                    cards.Offer(LeakCard);
            }
            // the water in her and the men's bundles take their room in the hold
            if (Trade.Started)
                Trade.Hold.taken = (float)Boat.TakenKg;
            // weary men lose heart
            if (Boat.Weary >= Boat.R.wearyHeartFrom)
                Voyage.Affect(0.0, -Boat.R.wearyHeartPerHour);
        }

        /// <summary>A game day has passed: the hull's wear, what the water spoils, and Martinmas.</summary>
        public static void Day(int day)
        {
            var game = KapmanGame.Current;
            if (game == null || !Voyage.Started || Boat.IsLost)
                return;
            Hook();
            Boat.Day(game.Day);
            if (Trade.Started && Boat.Water >= Boat.R.spoilFrom)
                foreach (var (good, count) in Trade.Hold.Contents)
                    if (count > 0 && WetSpoils(good))
                    {
                        Trade.Hold.Remove(good, 1);
                        Voyage.Say($"Standing in the water: {Trade.UnitsNamed(good, 1)} spoiled.");
                        break;
                    }
            if (game.clock == null)
                return;
            var date = game.clock.Date;
            double owed = Boat.Martinmas(date.year, date.month, date.day, Worth(), out bool due);
            if (!due)
                return;
            if (owed > 0.0)
            {
                Voyage.Owe(owed);
                Voyage.Affect(0.0, Boat.R.gainHeart);
                Voyage.Say($"Martinmas: a tenth of the season's gain is the men's. {owed:0} d is owed them.");
            }
            else
            {
                Voyage.Affect(0.0, Boat.R.noGainHeart);
                Voyage.Say("Martinmas: the season made no gain, and the men's share of nothing is nothing.");
            }
        }

        /// <summary>The goods water spoils when the lowest of the cargo stands in it.</summary>
        public static bool WetSpoils(GoodDef g) =>
            g != null && (g.keeps > 0 || g.cat == "Food" || g.cat == "Provisions" || g.cat == "Spices" || g.cat == "Textiles"
                          || g.cat == "Fibres" || g.cat == "Furs" || g.cat == "Dyes" || g.cat == "Books and church goods");

        static void OnSign(int sign, double hours)
        {
            // the words are placeholders, the user's to change
            string text = sign == 1 ? "A long swell from a quarter the wind is not in. Weather is coming, half a day off or less."
                        : sign == 2 ? "The sky is lowering and the gulls are going inland. It will blow hard before the watch is out."
                        : "White water to windward. It is on her within the hour.";
            Voyage.Say(text);
            // (the clock drops back by itself for it, with its own switch in the settings: the first sign to speed 2,
            // the later ones to speed 1. A card still holds the clock when it opens, so a gale is never met unasked.)
            ClockDrops.Raise(sign == 1 ? ClockReason.WeatherFar : ClockReason.WeatherNear);
            var cards = EncounterRunner.Director;
            if (cards != null && sign >= 2)
                cards.Offer(sign == 2 ? GaleCard : OrderCard);             // (first in line; it holds the clock when it opens)
        }

        /// <summary>
        /// "Turn for the nearest lee": her course is laid to the nearest port she can reach by water (a made-fast boat
        /// is out of the gale: no dice, no seas). The fact for the card's outcome; null when there is nowhere to run.
        /// </summary>
        public static string RunForShelter()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
                return null;
            var at = ChartWorld.ShipWorld();
            PortDef best = null;
            double bestKm = double.MaxValue;
            var line = new System.Collections.Generic.List<WorldXZ>();
            foreach (var def in GameData.Ports)
            {
                if (def == null || string.IsNullOrEmpty(def.id))
                    continue;
                // (a port she has found: she does not run for a harbour she has only heard of, 9 Oct 2026)
                if (!Hearsay.Found(def))
                    continue;
                var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
                if (WorldXZ.Distance(at, new WorldXZ(x, z)) > 150000.0)     // (a day's run at the most: no need to plan to them all)
                    continue;
                if (Course.Plan(def.id, at, null, line, out double km) && km < bestKm)
                {
                    best = def;
                    bestKm = km;
                }
            }
            if (best == null || !Course.SetTo(best.id, at, null))
                return null;
            return $"She turns for {best.name}, {bestKm:0} km off.";
        }

        static void OnLost(string how)
        {
            GameSpeed.Hold("lost");                                       // (the clock stops; SaveGame.Busy refuses a save from here)
            GameOver?.Invoke(how);
        }

        /// <summary>A saved game was loaded: the clock's hold for a lost boat is let go if this one swims.</summary>
        public static void Loaded()
        {
            if (!Boat.IsLost)
                GameSpeed.Release("lost");
            if (Trade.Started)
                Trade.Hold.taken = (float)Boat.TakenKg;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            hooked = false;
            GameOver = null;
        }
    }
}
