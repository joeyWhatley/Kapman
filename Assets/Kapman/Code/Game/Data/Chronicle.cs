using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One real, dated event (Data/chronicle_793.json).</summary>
    public sealed class ChronicleEntry
    {
        public string id, kind, title, place, effect, note, certainty, realm, precision, season, date;
        public string[] ports = new string[0], regions = new string[0], sources = new string[0], who = new string[0];
        /// <summary>The day the game shows it (Julian): year, month, day.</summary>
        public int year, month, day;
        /// <summary>Where it happened, if the table gives a place off the ports (lat, lon); NaN if not.</summary>
        public double lat = double.NaN, lon = double.NaN;

        /// <summary>A number that orders the days (for "has it happened by today").</summary>
        public int Key => year * 400 + month * 32 + day;
        /// <summary>When the state it sets ends ("794-08": the end of that month), as a key like Key; 0 when it gives none.</summary>
        public int EndKey;

        /// <summary>Its date as the sources allow: "8 June 793", "June 793", "summer 793", "793".</summary>
        public string When
        {
            get
            {
                switch (precision)
                {
                    case "day": return $"{day} {Calendar.MonthNames[month - 1]} {year}";
                    case "month": return $"{Calendar.MonthNames[month - 1]} {year}";
                    case "season": return $"{(string.IsNullOrEmpty(season) ? "" : char.ToUpper(season[0]) + season.Substring(1) + " ")}{year}";
                    default: return year.ToString();
                }
            }
        }

        /// <summary>Where it happened on the map (real metres), if the table says.</summary>
        public bool Where(out double x, out double z)
        {
            x = z = 0.0;
            foreach (var id in ports)
            {
                var p = GameData.Port(id);
                if (p != null)
                {
                    (x, z) = WorldFrame.ToWorld(p.lat, p.lon);
                    return true;
                }
            }
            if (!double.IsNaN(lat))
            {
                (x, z) = WorldFrame.ToWorld(lat, lon);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// THE CHRONICLE - real, dated events of the time (Data/chronicle_793.json: raids, wars, laws, deaths of kings,
    /// famines, hard winters, eclipses... each with its sources), in date order. As the game's date passes an entry's
    /// day, it has happened: the map's log shows it, and the graphs hear of it (Chronicle: On event) to make it bite -
    /// a raid on its day, a law from its year. Entries before the game's start are the backstory, there from the start.
    /// The user: "a day by day month by month year by year event log for historical events pulled from real history".
    /// </summary>
    public static class Chronicle
    {
        static readonly List<ChronicleEntry> entries = new List<ChronicleEntry>();
        static int next;                                             // the first entry not yet happened
        static readonly Dictionary<string, string> kinds = new Dictionary<string, string>();
        static readonly Dictionary<string, ChronicleEntry> byId = new Dictionary<string, ChronicleEntry>();

        public static IReadOnlyList<ChronicleEntry> Entries => entries;
        /// <summary>An entry by its id (null when the table has none such).</summary>
        public static ChronicleEntry Entry(string id) => id != null && byId.TryGetValue(id, out var e) ? e : null;
        /// <summary>Kind id -> what it means ("raid": "a raid or piracy from the sea or by land...").</summary>
        public static IReadOnlyDictionary<string, string> Kinds => kinds;
        /// <summary>How many have happened by the last <see cref="Advance"/>.</summary>
        public static int Happened => next;

        /// <summary>An entry's day came (the graphs' "Chronicle: On event").</summary>
        public static event Action<ChronicleEntry> Came;

        public static void Load(TextAsset json)
        {
            entries.Clear();
            kinds.Clear();
            byId.Clear();
            next = 0;
            if (json == null)
                return;
            var root = Json.Parse(json.text);
            foreach (var kv in Json.Obj(root, "kinds") ?? new Dictionary<string, object>())
                kinds[kv.Key] = kv.Value as string ?? "";
            foreach (var o in Json.Arr(root, "entries") ?? new List<object>())
            {
                var e = new ChronicleEntry
                {
                    id = Json.Str(o, "id"), kind = Json.Str(o, "kind"), title = Json.Str(o, "title"), place = Json.Str(o, "place"),
                    effect = Json.Str(o, "effect"), note = Json.Str(o, "note"), certainty = Json.Str(o, "certainty"),
                    realm = Json.Str(o, "realm"), precision = Json.Str(o, "precision", "year"), season = Json.Str(o, "season"),
                    date = Json.Str(o, "date"), EndKey = EndOf(Json.Str(o, "end")),
                    ports = Strings(Json.Arr(o, "ports")), regions = Strings(Json.Arr(o, "regions")),
                    sources = Strings(Json.Arr(o, "sources")), who = Strings(Json.Arr(o, "who")),
                };
                var fire = Json.Str(o, "fire").Split('-');
                if (fire.Length < 3 || !int.TryParse(fire[0], out e.year) || !int.TryParse(fire[1], out e.month) || !int.TryParse(fire[2], out e.day))
                    continue;
                if (Json.Arr(o, "at") is List<object> at && at.Count == 2 && at[0] is double la && at[1] is double lo)
                {
                    e.lat = la;
                    e.lon = lo;
                }
                entries.Add(e);
                if (!string.IsNullOrEmpty(e.id))
                    byId[e.id] = e;
            }
            entries.Sort((a, b) => a.Key.CompareTo(b.Key));
        }

        /// <summary>"794", "794-08" or "794-08-15" as a key (a year or month given alone means its last day).</summary>
        static int EndOf(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;
            var p = s.Split('-');
            if (!int.TryParse(p[0], out int y))
                return 0;
            int m = p.Length > 1 && int.TryParse(p[1], out int mm) ? mm : 12;
            int d = p.Length > 2 && int.TryParse(p[2], out int dd) ? dd : Calendar.DaysIn(y, m);
            return y * 400 + m * 32 + d;
        }

        /// <summary>The entries before the last days days whose state still holds today (an end after today): the great
        /// famine of 793 is still on at the start in June, though it began in January. Oldest first.</summary>
        public static List<ChronicleEntry> Ongoing(GameDate today, int days)
        {
            var list = new List<ChronicleEntry>();
            int k = Key(today);
            int k0 = Key(Calendar.At(today.year, today.dayOfYear, -days * 24.0));
            foreach (var e in entries)
                if (e.Key <= k0 && e.EndKey >= k)
                    list.Add(e);
            return list;
        }

        static string[] Strings(List<object> list) => list == null ? new string[0] : list.ConvertAll(s => s as string ?? "").ToArray();

        static int Key(GameDate d) => d.year * 400 + d.month * 32 + d.day;

        /// <summary>Sets the chronicle to a date without telling anyone (the start: what's before it is the backstory).</summary>
        public static void StartAt(GameDate today)
        {
            next = 0;
            int k = Key(today);
            while (next < entries.Count && entries[next].Key <= k)
                next++;
        }

        /// <summary>Moves the chronicle on to today: every entry whose day has come is told (Came), in order.</summary>
        public static void Advance(GameDate today)
        {
            int k = Key(today);
            while (next < entries.Count && entries[next].Key <= k)
                Came?.Invoke(entries[next++]);
        }

        /// <summary>The entries of the last days days before today (still felt at the start: Lindisfarne's market is bare in
        /// June 793), oldest first. <see cref="Replay"/> tells them again.</summary>
        public static List<ChronicleEntry> Recent(GameDate today, int days)
        {
            var list = new List<ChronicleEntry>();
            int k = Key(today);
            var from = Calendar.At(today.year, today.dayOfYear, -days * 24.0);
            int k0 = Key(from);
            foreach (var e in entries)
                if (e.Key > k0 && e.Key <= k)
                    list.Add(e);
            return list;
        }

        /// <summary>Tells an entry again (the graphs make it bite): for the recent past at the start.</summary>
        public static void Replay(ChronicleEntry e) => Came?.Invoke(e);

        /// <summary>The entries that have happened by now, newest first.</summary>
        public static IEnumerable<ChronicleEntry> SoFar()
        {
            for (int i = next - 1; i >= 0; i--)
                yield return entries[i];
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => next = 0;
    }
}
