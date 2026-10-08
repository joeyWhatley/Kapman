using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One water or landmark of the chart that is there to be found (Game/Data/discoveries_793.json).</summary>
    public sealed class DiscoveryDef
    {
        public string id = "", kind = "", name = "", note = "";
        /// <summary>What its kind is called in a word ("water", "shoal", "holy place").</summary>
        public string kindName = "";
        public double lat, lon;
        /// <summary>Its place in the world frame, and how near she must come to find it (real metres).</summary>
        public double x, z, reach;
        public int from, to;

        public bool There(int year) => ContentJson.InYears(from, to, year);
    }

    /// <summary>The table of them, as read.</summary>
    public sealed class DiscoveryBook
    {
        public readonly List<DiscoveryDef> things = new List<DiscoveryDef>();
        readonly Dictionary<string, DiscoveryDef> byId = new Dictionary<string, DiscoveryDef>(StringComparer.Ordinal);

        public DiscoveryDef Get(string id) => id != null && byId.TryGetValue(id, out var d) ? d : null;

        /// <summary>The table from its text's root (null: an empty book). A row with no id, no name or no place is left
        /// out; so is a second row of the same id.</summary>
        public static DiscoveryBook Read(Dictionary<string, object> root)
        {
            var book = new DiscoveryBook();
            if (root == null)
                return book;
            var kinds = Json.Obj(root, "kinds");
            foreach (var row in ContentJson.Rows(root, "things"))
            {
                var d = new DiscoveryDef
                {
                    id = ContentJson.Text(row, "id"), kind = ContentJson.Text(row, "kind"), name = ContentJson.Text(row, "name"),
                    note = ContentJson.Text(row, "note"), lat = ContentJson.Num(row, "lat", double.NaN), lon = ContentJson.Num(row, "lon", double.NaN),
                    from = ContentJson.Int(row, "from"), to = ContentJson.Int(row, "to"),
                };
                if (d.id.Length == 0 || d.name.Length == 0 || double.IsNaN(d.lat) || double.IsNaN(d.lon) || book.byId.ContainsKey(d.id))
                    continue;
                var kind = kinds != null ? Json.Obj(kinds, d.kind) : null;
                d.kindName = kind != null ? ContentJson.Text(kind, "name", d.kind) : d.kind;
                double km = ContentJson.Num(row, "reach_km", kind != null ? ContentJson.Num(kind, "reach_km", 16.0) : 16.0);
                d.reach = Math.Max(1.0, km) * 1000.0;
                (d.x, d.z) = WorldFrame.ToWorld(d.lat, d.lon);
                book.things.Add(d);
                book.byId[d.id] = d;
            }
            return book;
        }
    }

    /// <summary>
    /// WATERS AND LANDMARKS TO FIND (the user, 9 Oct 2026: "add waters to discover and land marks ect points of
    /// interest"). The chart has seas, river mouths, islands, headlands, shoals, old lights and churches on it that are
    /// not there until she has come to them. Each has a place and a reach: when she is within its reach it is FOUND,
    /// once and for good. The log says so, its dot stands on the chart from then on (MapView; its name shows under the
    /// pointer, as a port's does), and Journey lists it. What lies in the waters she knows on the first morning is known
    /// to her already, and nothing is said of it (<see cref="KnowAtStart"/>). Saved with the game.
    /// The words said of a thing are the table's (PLACEHOLDERS, the user's to write).
    /// </summary>
    public static class Discoveries
    {
        public const string File = "discoveries_793.json";

        static readonly ContentCache<DiscoveryBook> cache = new ContentCache<DiscoveryBook>("the waters and landmarks", () => DiscoveryBook.Read(ContentFiles.ParseObject(File)));
        static DiscoveryBook own;

        /// <summary>The table (the game's file; or one a test has put in its place with <see cref="Use"/>).</summary>
        public static DiscoveryBook Book => own ?? cache.Value;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(DiscoveryBook book) => own = book;

        static readonly List<string> found = new List<string>();                 // in the order found
        static readonly Dictionary<string, double> foundDay = new Dictionary<string, double>(StringComparer.Ordinal);

        /// <summary>Counts up whenever what is found changes (a find, a loaded game, a new one).</summary>
        public static int Version { get; private set; }
        /// <summary>A thing was found by sailing to it (not said of what is known from the start).</summary>
        public static event Action<DiscoveryDef> Found;

        public static IReadOnlyList<string> FoundIds => found;
        public static int Count => found.Count;
        public static bool IsFound(string id) => id != null && foundDay.ContainsKey(id);
        /// <summary>The game day it was found on (under 0: known from the start; NaN: not found).</summary>
        public static double DayFound(string id) => id != null && foundDay.TryGetValue(id, out double day) ? day : double.NaN;

        /// <summary>How many there are to find in a year.</summary>
        public static int Total(int year)
        {
            int n = 0;
            foreach (var d in Book.things)
                if (d.There(year))
                    n++;
            return n;
        }

        /// <summary>
        /// She is here (real metres, the world frame): everything within its own reach of her is found. What was found
        /// by this step, in the table's order (none, nearly always).
        /// </summary>
        public static List<DiscoveryDef> Step(WorldXZ ship, double day, int year)
        {
            if (olderSave)
            {
                // (a game saved before there was anything to find: what lies where she has been is known to her, and
                // what is round her now too, and nothing is said of either)
                olderSave = false;
                KnowAtStart(year, ship);
            }
            List<DiscoveryDef> now = null;
            foreach (var d in Book.things)
            {
                if (foundDay.ContainsKey(d.id) || !d.There(year))
                    continue;
                double dx = d.x - ship.x, dz = d.z - ship.z;
                if (dx * dx + dz * dz > d.reach * d.reach)
                    continue;
                Add(d.id, day);
                (now ??= new List<DiscoveryDef>()).Add(d);
            }
            if (now == null)
                return None;
            Version++;
            foreach (var d in now)
                Found?.Invoke(d);
            return now;
        }

        static readonly List<DiscoveryDef> None = new List<DiscoveryDef>();

        /// <summary>
        /// The first morning: what lies in the waters she knows already (the fog is lifted there: Exploration), and what
        /// is within its own reach of where she lies, is known to her, and nothing is said of it. Her own river is no
        /// discovery.
        /// </summary>
        public static int KnowAtStart(int year, WorldXZ at)
        {
            int n = 0;
            foreach (var d in Book.things)
            {
                if (foundDay.ContainsKey(d.id) || !d.There(year))
                    continue;
                double dx = d.x - at.x, dz = d.z - at.z;
                if (dx * dx + dz * dz <= d.reach * d.reach || (Exploration.Ready && Exploration.Seen(d.x, d.z) > 0.35f))
                {
                    Add(d.id, -1.0);
                    n++;
                }
            }
            if (n > 0)
                Version++;
            return n;
        }

        static bool olderSave;

        static void Add(string id, double day)
        {
            foundDay[id] = day;
            found.Add(id);
        }

        /// <summary>What the log says of a find. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string Line(DiscoveryDef d) => d == null ? "" : $"New to you: {d.name}. {d.note}".Trim();

        /// <summary>Nothing is found (a new game).</summary>
        public static void Forget()
        {
            olderSave = false;
            if (found.Count == 0)
                return;
            found.Clear();
            foundDay.Clear();
            Version++;
        }

        // ------------------------------------------------------------------ the saved game

        static object Capture()
        {
            var list = new List<object>();
            foreach (string id in found)
                list.Add(new List<object> { id, foundDay[id] });
            return new Dictionary<string, object> { ["found"] = list };
        }

        static void Restore(object saved)
        {
            found.Clear();
            foundDay.Clear();
            olderSave = saved == null;                                    // (a save of before 9 Oct 2026 has no such part: see Step)
            foreach (object o in Json.Arr(saved, "found") ?? new List<object>())
                if (o is List<object> pair && pair.Count >= 1 && pair[0] is string id && id.Length > 0 && !foundDay.ContainsKey(id))
                    Add(id, pair.Count > 1 && pair[1] is double day ? day : -1.0);
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            found.Clear();
            foundDay.Clear();
            olderSave = false;
            own = null;
            Found = null;
            Version++;
            ContentSave.Register("found", Capture, Restore);
        }
    }
}
