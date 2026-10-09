using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One rough water of Game/Data/rough_waters_793.json.</summary>
    public sealed class RoughWater
    {
        public string id = "", kind = "", find = "", today = "";
        public double lat, lon, reachM, rough = 1.0;
        /// <summary>The compass bearing a bar lies open to (NaN: none given).</summary>
        public double openTo = double.NaN;
        /// <summary>Its place, real metres in the world frame.</summary>
        public WorldXZ at;
        /// <summary>The hardest the tide model runs here in a fortnight, m/s (worked out when first asked).</summary>
        public double streamMost = double.NaN;
    }

    /// <summary>
    /// ROUGH WATERS (the user, 9 Oct 2026: "... and rough ocean areas"). Real places where the sea is worse than the
    /// wind alone makes it, from Game/Data/rough_waters_793.json:
    ///   a RACE is rough while the tide's stream runs (as the tide model has it there, against its own hardest),
    ///     worst with the wind against the stream, and quiet at slack water;
    ///   a BAR, the bar of a river mouth, is rough with the wind blowing on to it;
    ///   a SHOAL breaks in any fresh wind;
    ///   a CAPE with no shelter is rough in a fresh wind, worse while the tide runs.
    /// How rough it is now runs from 0 to the water's own `rough` (1: a man bailing, the hull working as in a gale).
    /// In it she takes water over the gunwale, her hull works, and she makes less way (Boat.Hour, ChartBody). The
    /// log says so when she comes into it and the clock drops back. Each is tied to a thing to find (Discoveries):
    /// its name is hers when she has found that.
    /// </summary>
    public static class RoughWaters
    {
        public const string File = "rough_waters_793.json";
        /// <summary>How rough it must be before the log says so and the clock drops; and under which it is over.</summary>
        public const double ToldFrom = 0.3, OverUnder = 0.15;

        sealed class Book
        {
            public readonly List<RoughWater> waters = new List<RoughWater>();
        }

        static readonly ContentCache<Book> cache = new ContentCache<Book>("the rough waters", () => Read(ContentFiles.Exists(File) ? ContentFiles.ParseObject(File) : null));
        static Book own;

        /// <summary>The rough waters as the game has them (or a test's: <see cref="Use"/>).</summary>
        public static IReadOnlyList<RoughWater> All => (own ?? cache.Value).waters;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(IEnumerable<RoughWater> waters)
        {
            if (waters == null)
            {
                own = null;
                return;
            }
            own = new Book();
            foreach (var w in waters)
            {
                var (x, z) = WorldFrame.ToWorld(w.lat, w.lon);
                w.at = new WorldXZ(x, z);
                own.waters.Add(w);
            }
        }

        /// <summary>A test's sea: how rough a water is, in place of the reckoning (null: the real one). Never set by the game.</summary>
        public static Func<RoughWater, double> Fixed;

        static Book Read(Dictionary<string, object> root)
        {
            var book = new Book();
            foreach (object o in Json.Arr(root, "waters") ?? new List<object>())
            {
                if (!(o is Dictionary<string, object>))
                    continue;
                var w = new RoughWater
                {
                    id = Json.Str(o, "id"), kind = Json.Str(o, "kind"), find = Json.Str(o, "find"), today = Json.Str(o, "today"),
                    lat = Json.Num(o, "lat"), lon = Json.Num(o, "lon"), reachM = Json.Num(o, "reach_km", 8.0) * 1000.0,
                    rough = Json.Num(o, "rough", 1.0), openTo = Json.Num(o, "open_to", double.NaN),
                };
                if (w.id.Length == 0)
                    continue;
                var (x, z) = WorldFrame.ToWorld(w.lat, w.lon);
                w.at = new WorldXZ(x, z);
                book.waters.Add(w);
            }
            return book;
        }

        /// <summary>The rough water a place lies in: of those whose reach it is within, the one that is roughest at
        /// its worst. Null: none.</summary>
        public static RoughWater At(WorldXZ at)
        {
            RoughWater best = null;
            foreach (var w in All)
                if (WorldXZ.Distance(at, w.at) <= w.reachM && (best == null || w.rough > best.rough))
                    best = w;
            return best;
        }

        /// <summary>The hardest the tide model runs at a water in a fortnight, m/s: found once, by asking every half hour.</summary>
        static double StreamMost(RoughWater w)
        {
            if (!double.IsNaN(w.streamMost))
                return w.streamMost;
            double most = 0.0, depth = SeaPush.Depth(w.at);
            for (int k = 0; k < 15 * 48; k++)
            {
                var (e, n) = SeaPush.Stream(w.at, 2010700.5 + k / 48.0, depth);   // (a fortnight of the year 793)
                most = Math.Max(most, Math.Sqrt(e * e + n * n));
            }
            return w.streamMost = most;
        }

        /// <summary>
        /// HOW ROUGH A WATER IS NOW: 0 quiet, up to its own `rough` at its worst (a little over, in a race with a hard
        /// wind against the tide). `wind`: m/s where she is, and the compass point it blows from. `clockJulianDay`:
        /// the clock's own.
        /// </summary>
        public static double Roughness(RoughWater w, double wind, double windFrom, double clockJulianDay)
        {
            if (w == null)
                return 0.0;
            if (Fixed != null)
                return Math.Max(0.0, Fixed(w));
            var (east, north) = SeaPush.Stream(w.at, clockJulianDay);
            double speed = Math.Sqrt(east * east + north * north), most = StreamMost(w);
            double run = most > 0.02 ? Math.Min(1.0, speed / most) : 0.0;      // (0 at slack water, 1 at the hardest of a spring tide)
            switch (w.kind)
            {
                case "race":
                {
                    // the wind against the stream: where the wind blows TO, against where the stream sets
                    double against = 0.0;
                    if (speed > 1e-6 && wind > 0.0)
                    {
                        double to = (windFrom + 180.0) * Math.PI / 180.0;
                        against = Math.Max(0.0, -(Math.Sin(to) * east + Math.Cos(to) * north) / speed);
                    }
                    return GameRules.RoughRace(w.rough, run, wind, against);         // (the graph "Rule - how rough a tide race is")
                }
                case "bar":
                {
                    // the wind blowing on to it
                    double onshore = double.IsNaN(w.openTo) ? 0.6 : Math.Max(0.0, Math.Cos((windFrom - w.openTo) * Math.PI / 180.0));
                    return GameRules.RoughBar(w.rough, run, wind, onshore);          // (the graph "Rule - how rough a river's bar is")
                }
                case "shoal":
                    return GameRules.RoughShoal(w.rough, run, wind);                 // (the graph "Rule - how rough a shoal is")
                default:                                                     // a cape
                    return GameRules.RoughCape(w.rough, run, wind);                  // (the graph "Rule - how rough an open cape is")
            }
        }

        // ------------------------------------------------------------------ the live game

        /// <summary>The rough water she is in now (null: none), and how rough it is.</summary>
        public static RoughWater Now { get; private set; }
        public static double RoughNow { get; private set; }
        /// <summary>Counts up when what the windows show of this has changed.</summary>
        public static int Version { get; private set; }

        static string toldOf;
        static double quietHours;

        /// <summary>The share of her speed she keeps in the water she is in.</summary>
        public static double Slow => RoughNow <= 0.0 ? 1.0 : GameRules.RoughSlow(RoughNow, SeaPush.Rules.roughSlow);   // (the graph "Rule - her speed in rough water")

        /// <summary>The name she has for a water: the name of the thing found that it is, else what it plainly is.
        /// PLACEHOLDER WORDS, the user's to write.</summary>
        public static string NameOf(RoughWater w)
        {
            if (w == null)
                return "";
            var known = w.find.Length > 0 && Discoveries.IsFound(w.find) ? Discoveries.Book.Get(w.find) : null;
            if (known != null)
                return known.name;
            return w.kind == "race" ? "A tide race" : w.kind == "bar" ? "The bar of a river" : w.kind == "shoal" ? "Shoal water" : "An open headland";
        }

        /// <summary>What the log says when she comes into it. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string Words(RoughWater w, bool windAgainst)
        {
            string name = NameOf(w);
            switch (w.kind)
            {
                case "race": return name + (windAgainst ? ": the tide runs against the wind here, and the sea stands up." : ": the tide runs hard here, and the water boils.");
                case "bar": return name + ": white water on the bar.";
                case "shoal": return name + ": the sea breaks on the shoals.";
                default: return name + ": a steep confused sea off the head.";
            }
        }

        /// <summary>One game minute: the water she is in and how rough it is; said when she comes into rough water, and
        /// the clock drops back. `atSea`: she is neither made fast nor on a river.</summary>
        public static void Step(WorldXZ at, double wind, double windFrom, double clockJulianDay, bool atSea, double hours)
        {
            var was = Now;
            double roughWas = RoughNow;
            Now = atSea && SeaPush.Rules.on ? At(at) : null;
            RoughNow = Now != null ? Roughness(Now, wind, windFrom, clockJulianDay) : 0.0;
            if (RoughNow >= ToldFrom)
            {
                quietHours = 0.0;
                if (toldOf != Now.id)
                {
                    toldOf = Now.id;
                    bool against = false;
                    if (Now.kind == "race")
                    {
                        var (east, north) = SeaPush.Stream(Now.at, clockJulianDay);
                        double to = (windFrom + 180.0) * Math.PI / 180.0;
                        against = wind > 4.0 && Math.Sin(to) * east + Math.Cos(to) * north < 0.0;
                    }
                    SeaMoments.Raise("rough", Words(Now, against), ClockReason.RoughWater);   // (a moment of the flow: "On the sea moves her")
                }
            }
            else if (toldOf != null && (Now == null || RoughNow < OverUnder))
            {
                // (out of it, or it has gone quiet: after an hour of that it is news again)
                quietHours += Math.Max(0.0, hours);
                if (quietHours >= 1.0)
                    toldOf = null;
            }
            if (!ReferenceEquals(was, Now) || (int)(roughWas * 5.0) != (int)(RoughNow * 5.0))
                Version++;
        }

        /// <summary>A test's: as a new game has it.</summary>
        public static void Forget()
        {
            Now = null;
            RoughNow = 0.0;
            toldOf = null;
            quietHours = 0.0;
            Version++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            own = null;
            Fixed = null;
            Forget();
        }
    }
}
