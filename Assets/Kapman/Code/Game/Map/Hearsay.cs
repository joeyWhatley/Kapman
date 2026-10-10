using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// A PLACE ONLY HEARD OF IS A WIDE RING ON THE CHART, NOT A DOT (the user, 9 Oct 2026: "Maybe don't even show dots for
    /// unfound locations, needs to be more vague"; and by the picker the same day: "A wide hazy ring"). A port is FOUND
    /// when she has seen where it lies (the fog has lifted there): then it has its dot. A port she has heard of and
    /// not found has a ring some 24 km across in the dark: "somewhere in here". The place lies inside the ring, between
    /// a quarter and two thirds of the way out from its middle, never in the middle: which way and how far is the
    /// same in every game (by the port's name), so a ring is not a puzzle that changes when the game is loaded.
    /// </summary>
    public static class Hearsay
    {
        /// <summary>Real metres from a ring's middle to its edge. 12 km since the user saw the rings of 40 km on the chart
        /// (9 Oct 2026: "make the rings much smaller pls"): a ring is some 24 km across, 200 pixels of the chart as it
        /// opens, where it took most of the chart's height.</summary>
        public const double RingRadius = 12000.0;
        const double NearestToMiddle = 0.25, FarthestFromMiddle = 0.65;

        /// <summary>Has she seen where the port lies? (With no fog at all: yes.)</summary>
        public static bool Found(PortDef def)
        {
            if (def == null)
                return false;
            if (!Exploration.Ready)
                return true;
            var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
            return Exploration.Seen(x, z) > 0.35f;
        }

        /// <summary>Heard of and not found: it has a ring on the chart.</summary>
        public static bool Ringed(PortDef def) => def != null && !Found(def) && Known.IsHeardOf(def);

        /// <summary>The middle of a port's ring (real metres, the world frame).</summary>
        public static WorldXZ RingMiddle(PortDef def)
        {
            var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
            uint h = Hash(def.id);
            double angle = (h & 0xFFFFu) / 65536.0 * Math.PI * 2.0;
            double off = (NearestToMiddle + ((h >> 16) & 0xFFFFu) / 65536.0 * (FarthestFromMiddle - NearestToMiddle)) * RingRadius;
            return new WorldXZ(x - Math.Cos(angle) * off, z - Math.Sin(angle) * off);
        }

        /// <summary>The same number for the same name on every machine and in every run (FNV-1a; a string's own hash is not).</summary>
        static uint Hash(string s)
        {
            uint h = 2166136261u;
            foreach (char c in s ?? "")
            {
                h ^= c;
                h *= 16777619u;
            }
            // (mixed once more: names that differ in their last letter should not point the same way)
            h ^= h >> 15;
            h *= 2246822519u;
            h ^= h >> 13;
            return h;
        }

        /// <summary>The ports whose rings a place lies in, the nearest middle first (none: an empty list).</summary>
        public static List<PortDef> RingsAt(WorldXZ place, int year, List<PortDef> into = null)
        {
            var list = into ?? new List<PortDef>();
            list.Clear();
            foreach (var def in GameData.Ports)
                if (def.Exists(year) && Ringed(def) && WorldXZ.Distance(RingMiddle(def), place) <= RingRadius)
                    list.Add(def);
            if (list.Count > 1)
                list.Sort((a, b) => WorldXZ.Distance(RingMiddle(a), place).CompareTo(WorldXZ.Distance(RingMiddle(b), place)));
            return list;
        }

        /// <summary>What is said under the pointer inside a ring or rings. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string Words(IReadOnlyList<PortDef> rings)
        {
            if (rings == null || rings.Count == 0)
                return "";
            if (rings.Count == 1)
                return rings[0].name + ", by hearsay: somewhere in this ring";
            var names = new List<string>();
            for (int i = 0; i < rings.Count && i < 4; i++)
                names.Add(rings[i].name);
            return "By hearsay, somewhere here: " + string.Join(", ", names) + (rings.Count > 4 ? $" and {rings.Count - 4} more" : "");
        }
    }

    /// <summary>
    /// A HARBOUR COMES IN SIGHT (the user, 9 Oct 2026, by the picker: "Click and sail ... A port or landmark appears when
    /// it comes in sight, and the clock slows so you can decide"). Each game minute the ports whose place the fog has
    /// just lifted from are told, once each: the log says so and the clock drops back. The ports in what she knows on
    /// the first morning are known without a word. Which ports have been told is saved with the game.
    /// </summary>
    public static class Sightings
    {
        static readonly HashSet<string> told = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<PortDef> None = new List<PortDef>();
        static bool older;

        /// <summary>Counts up when a port comes in sight, a game is loaded or begun.</summary>
        public static int Version { get; private set; }

        public static bool Told(string portId) => portId != null && told.Contains(portId);

        /// <summary>The first morning: the ports where the fog is lifted already are known without a word.</summary>
        public static void KnowAtStart(int year)
        {
            foreach (var def in GameData.Ports)
                if (def.Exists(year) && Hearsay.Found(def))
                    told.Add(def.id);
            Version++;
        }

        /// <summary>The ports that have come in sight since the last look, in the table's order (none, nearly always).</summary>
        public static List<PortDef> Step(int year)
        {
            if (older)
            {
                // (a game saved before sightings were kept: what she has seen is not told as new)
                older = false;
                KnowAtStart(year);
            }
            List<PortDef> now = null;
            foreach (var def in GameData.Ports)
                if (def.Exists(year) && !told.Contains(def.id) && Hearsay.Found(def))
                {
                    told.Add(def.id);
                    (now ??= new List<PortDef>()).Add(def);
                }
            if (now == null)
                return None;
            Version++;
            return now;
        }

        /// <summary>What the log says of a harbour that comes in sight. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string Line(PortDef port, WorldXZ ship)
        {
            if (port == null)
                return "";
            var (x, z) = WorldFrame.ToWorld(port.lat, port.lon);
            bool heard = Known.IsHeardOf(port);
            return $"A harbour in sight to the {SeaWatch.Airt(ship.x, ship.z, x, z)}: {port.name}" + (heard ? ", that you had heard of." : ".");
        }

        /// <summary>The same in the log's short form (10 Oct 2026): "Witla in sight, to the south". PLACEHOLDER WORDS.</summary>
        public static string Short(PortDef port, WorldXZ ship)
        {
            if (port == null)
                return "";
            var (x, z) = WorldFrame.ToWorld(port.lat, port.lon);
            return $"{port.name} in sight, to the {SeaWatch.Airt(ship.x, ship.z, x, z)}";
        }

        /// <summary>And its box: what it is, where it lies, and whether she had heard of it.</summary>
        public static LogBox Box(PortDef port, WorldXZ ship)
        {
            if (port == null)
                return null;
            var (x, z) = WorldFrame.ToWorld(port.lat, port.lon);
            var box = new LogBox(port.name).Row("A harbour in sight", "to the " + SeaWatch.Airt(ship.x, ship.z, x, z));
            if (Known.IsHeardOf(port))
                box.Say("You had heard of it.");
            return box;
        }

        /// <summary>Nothing has been told (a new game).</summary>
        public static void Forget()
        {
            told.Clear();
            older = false;
            Version++;
        }

        static object Capture()
        {
            var list = new List<object>();
            foreach (string id in told)
                list.Add(id);
            list.Sort((a, b) => string.CompareOrdinal((string)a, (string)b));   // (the same save for the same game)
            return new Dictionary<string, object> { ["told"] = list };
        }

        static void Restore(object saved)
        {
            told.Clear();
            older = saved == null;
            foreach (object o in Json.Arr(saved, "told") ?? new List<object>())
                if (o is string id && id.Length > 0)
                    told.Add(id);
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            told.Clear();
            older = false;
            Version++;
            ContentSave.Register("sighted", Capture, Restore);
        }
    }
}
