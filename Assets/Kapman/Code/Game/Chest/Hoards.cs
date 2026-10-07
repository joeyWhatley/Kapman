using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    // ported 4 Oct: the hunts' logic is unchanged (it's all in the world frame: lat/lon, pinned notes, paces). Two
    // touches of the runtime were adapted: the dig skips the level's clock (LevelClock, was KapmanGame.clock.Skip), and
    // WhyNotDig turns the ship's scene position into world metres through FloatingOrigin - the 3D hand-off, which needs
    // the level scene to carry a FloatingOrigin placed at its world position (see VoyagePlanner); without one the ship is
    // never "near" a mark, and WhyNotDig says so.

    /// <summary>A landmark a hoard's clue names (placed from open data), and whether it stood then.</summary>
    public sealed class LandmarkDef
    {
        public string id, name, then;
        public double lat, lon;
    }

    /// <summary>A real hoard of the period (Data/hoards_793.json): where it lay, when it was buried, what it held, the
    /// landmarks round it and its clue as measures (from one landmark toward another, so many paces) that land on it.</summary>
    public sealed class HoardDef
    {
        public string id, name, kind, held, port, foundStory, seaRoute, seaDirection, seaPort;
        public double lat, lon, silverG, seaDays;
        public int buried;
        public bool churchGround;
        public readonly List<LandmarkDef> landmarks = new List<LandmarkDef>();
        /// <summary>The clue's measures, as plain facts (the user writes the clue's own words).</summary>
        public readonly List<string> measures = new List<string>();

        public LandmarkDef Landmark(string id) => landmarks.Find(l => l.id == id);
    }

    /// <summary>
    /// HOARD HUNTS (Plans/map_play.md: "a string from the landmarks the clue names, paced out; dig at your mark") - a hunt
    /// opens when a clue to a hoard is won (a riddle set's reward, a verse, a stone). The clue gives the sea leg and the
    /// measures; its landmarks show on the map, close up; the merchant lays the string and paces with the map's tools and
    /// sets a dig mark. Lying near the mark, the crew dig (half a day): within <see cref="Within"/> metres of where it lay,
    /// the silver is lifted - a share of the real hoard (<see cref="Share"/>: real hoards dwarf the game's money; the share is
    /// a placeholder to tune). Church ground costs standing.
    /// OFF FOR NOW: no reward opens a hunt and the chest has no hoards tab. The famous hoards as treasure to find is a game
    /// trope, not period practice; the hunt is to be rebuilt as fetching an owner's buried silver for a share (the pacing,
    /// dig mark and dig here carry over; the real hoards stay lost, shown as history only).
    /// </summary>
    public static class Hoards
    {
        /// <summary>A pace, metres (the Roman passus of five feet, as Isidore defines it).</summary>
        public static double PaceM = 1.48;
        /// <summary>How near the dig mark must be (metres).</summary>
        public static double Within = 40.0;
        /// <summary>The share of a real hoard's silver the game pays (tune it).</summary>
        public static double Share = 0.05;
        /// <summary>How near the ship must lie to the mark for the crew to dig (metres).</summary>
        public static double DigReach = 3000.0;
        /// <summary>Game hours a dig takes.</summary>
        public static double DigHours = 6.0;

        public static readonly Dictionary<string, HoardDef> All = new Dictionary<string, HoardDef>();

        static readonly HashSet<string> open = new HashSet<string>();        // hunts opened (clues won)
        static readonly HashSet<string> lifted = new HashSet<string>();      // hoards lifted

        public static IEnumerable<HoardDef> Open => open.Select(Get).Where(h => h != null);
        public static bool IsOpen(string id) => id != null && open.Contains(id);
        public static bool Lifted(string id) => id != null && lifted.Contains(id);
        public static HoardDef Get(string id) => id != null && All.TryGetValue(id, out var h) ? h : null;

        public static void Load(TextAsset json)
        {
            All.Clear();
            if (json == null)
                return;
            var root = Json.Parse(json.text);
            PaceM = Json.Num(root, "pace_m", PaceM);
            foreach (var o in Json.Arr(root, "hoards") ?? new List<object>())
            {
                var h = new HoardDef
                {
                    id = Json.Str(o, "id"), name = Json.Str(o, "name"), kind = Json.Str(o, "kind"), held = Json.Str(o, "held"), port = Json.Str(o, "port", null),
                    foundStory = Json.Str(o, "found_story"), lat = Json.Num(o, "lat"), lon = Json.Num(o, "lon"), buried = (int)Json.Num(o, "buried"),
                    silverG = Json.Num(o, "silver_g"), churchGround = Json.Bool(o, "church_ground"),
                };
                foreach (var l in Json.Arr(o, "landmarks") ?? new List<object>())
                    h.landmarks.Add(new LandmarkDef { id = Json.Str(l, "id"), name = Json.Str(l, "name"), then = Json.Str(l, "then"), lat = Json.Num(l, "lat"), lon = Json.Num(l, "lon") });
                var sea = Json.Obj(o, "sea_leg");
                if (sea != null)
                {
                    h.seaPort = Json.Str(sea, "port", null);
                    h.seaDirection = Json.Str(sea, "direction");
                    h.seaRoute = Json.Str(sea, "route");
                    h.seaDays = Json.Num(sea, "days");
                }
                foreach (var m in Json.Arr(o, "measures") ?? new List<object>())
                {
                    string line = Describe(h, m);
                    if (!string.IsNullOrEmpty(line))
                        h.measures.Add(line);
                }
                if (!string.IsNullOrEmpty(h.id))
                    All[h.id] = h;
            }
        }

        /// <summary>A measure as a plain fact: "From where the Darwen meets the Ribble, toward Samlesbury: 1,181 paces."</summary>
        static string Describe(HoardDef h, object m)
        {
            string Name(string id) => id == null ? "" : id.StartsWith("pin:") ? "your pin" : h.Landmark(id)?.name ?? id.Replace('_', ' ');
            string type = Json.Str(m, "type");
            string then = Json.Str(m, "then", null);
            switch (type)
            {
                case "line_paces":
                {
                    string s = $"From {Name(Json.Str(m, "from", null))}, toward {Name(Json.Str(m, "toward", null))}: {Json.Num(m, "paces"):#,0} paces.";
                    var turn = Json.Obj(m, "turn");
                    if (turn != null)
                        s += $" Then turn {Json.Str(turn, "hand")}, square to the string: {Json.Num(turn, "paces"):#,0} paces.";
                    if (Json.Str(m, "pin", null) != null)
                        s += " Set a pin there.";
                    return s + (then != null && !then.StartsWith("set a pin") ? " " + Cap(then) + "." : "");
                }
                case "between":
                    return $"Between {Name(Json.Str(m, "from", Json.Str(m, "a", null)))} and {Name(Json.Str(m, "to", Json.Str(m, "b", null)))}: " +
                           $"{Json.Num(m, "fraction"):0.###} of the way." + (then != null ? " " + Cap(then) + "." : "");
                case "cross_lines":
                {
                    var lines = Json.Arr(m, "lines");
                    if (lines != null && lines.Count >= 2)
                        return "Where the line " + string.Join(" crosses the line ", lines.Select(l => $"from {Name(Json.Str(l, "from", null))} toward {Name(Json.Str(l, "toward", null))}")) + "." + (then != null ? " " + Cap(then) + "." : "");
                    return Json.Str(m, "words", "Where two lines cross.");
                }
                case "dig_depth":
                    return "Dig: " + Json.Str(m, "words", "");
                default:
                    return Cap(Json.Str(m, "words", null) ?? then ?? Json.Str(m, "how", null) ?? Json.Str(m, "where", null) ?? type);
            }
        }

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);

        /// <summary>A clue to a hoard is won: its hunt opens (the nearest hoard to a reward''s place, if given a place).</summary>
        public static HoardDef OpenNear(double lat, double lon)
        {
            HoardDef best = null;
            double bestKm = 20.0;
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            foreach (var h in All.Values)
            {
                var (hx, hz) = WorldFrame.ToWorld(h.lat, h.lon);
                double km = Math.Sqrt((hx - x) * (hx - x) + (hz - z) * (hz - z)) / 1000.0;
                if (km < bestKm)
                {
                    bestKm = km;
                    best = h;
                }
            }
            if (best != null)
                OpenHunt(best.id);
            return best;
        }

        public static void OpenHunt(string id)
        {
            if (Get(id) != null && open.Add(id))
                Voyage.Say("You have a clue to a buried hoard in your chest now.");
        }

        /// <summary>The dig mark set for a hunt (a pinned note joined "dig:ID"), or null.</summary>
        public static PlayerNote MarkOf(string id) => Chest.Notes.FirstOrDefault(n => n.links.Contains("dig:" + id) && !double.IsNaN(n.x));

        /// <summary>Why the crew can't dig at the mark now (null: they can).</summary>
        public static string WhyNotDig(HoardDef h)
        {
            var game = KapmanGame.Current;
            if (h == null || game == null || game.ship == null)
                return "Nothing to dig for.";
            if (Lifted(h.id))
                return "It's lifted.";
            var mark = MarkOf(h.id);
            if (mark == null)
                return "Set your dig mark on the map first.";
            if (!game.ship.anchored)
                return "Lie to near your mark first (anchor).";
            if (UnityEngine.Object.FindAnyObjectByType<FloatingOrigin>() == null)
                return "This level isn't placed in the world frame yet: the crew can't find your mark from here.";   // (ported 4 Oct: see the note above)
            var me = FloatingOrigin.ToWorld(game.ShipPosition);              // 3D HAND-OFF: scene -> world metres
            double d = Math.Sqrt((mark.x - me.x) * (mark.x - me.x) + (mark.z - me.z) * (mark.z - me.z));
            if (d > DigReach)
                return $"Your mark is {d / 1000.0:0.#} km off: lie nearer (within {DigReach / 1000.0:0} km).";
            return null;
        }

        /// <summary>The crew dig at the mark (half a day): the silver if the mark is on it, else earth. Returns the fact.</summary>
        public static string Dig(HoardDef h)
        {
            string why = WhyNotDig(h);
            if (why != null)
                return why;
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;                                  // ported 4 Oct: was game.clock?.Skip(DigHours)
            if (clock != null)
                clock.GameHours = Math.Max(0.0, clock.GameHours + DigHours);
            var mark = MarkOf(h.id);
            var (x, z) = WorldFrame.ToWorld(h.lat, h.lon);
            double off = Math.Sqrt((mark.x - x) * (mark.x - x) + (mark.z - z) * (mark.z - z));
            if (off > Within || game.Year < h.buried)
            {
                Chest.Touched();
                return $"The crew dig for half a day: nothing but earth{(off < 200 ? " (you feel it's near)" : "")}.";
            }
            lifted.Add(h.id);
            double grams = Math.Max(0.0, h.silverG) * Share;
            if (Trade.Started && grams > 0.0)
                Trade.Purse.Add("hacksilver", grams);
            if (h.churchGround)
                Sim.Deeds.Record("dug_church_ground", "dug in church ground", Sim.Deeds.NearestPlace());
            var found = Chest.AddNote($"Lifted here: {h.name}.", "dig:" + h.id);
            Chest.Pin(found, x, z);
            return $"The spades strike it: {h.name}. {Cap(h.held ?? "")} You take {grams / 1.7:#,0} deniers' worth of silver aboard.";
        }

        /// <summary>The hunts as plain data (a saved game).</summary>
        public static Dictionary<string, object> Capture() => new Dictionary<string, object>
        {
            ["open"] = open.Select(o => (object)o).ToList(), ["lifted"] = lifted.Select(o => (object)o).ToList(),
        };

        public static void Restore(Dictionary<string, object> d)
        {
            Clear();
            foreach (var x in Json.Arr(d, "open") ?? new List<object>())
                if (x is string s) open.Add(s);
            foreach (var x in Json.Arr(d, "lifted") ?? new List<object>())
                if (x is string s) lifted.Add(s);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            open.Clear();
            lifted.Clear();
        }
    }
}
