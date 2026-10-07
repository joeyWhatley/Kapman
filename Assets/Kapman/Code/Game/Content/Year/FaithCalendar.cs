// 7 Oct 2026: THE DAYS A FAITH KEEPS (Design/fate_faith_and_self.md section 3). Reads Game/Data/story/calendar_793.json
// through ContentFiles: the feasts and fasts with a day of their own, the days that move with Easter, the days of the
// week a faith keeps, the days of the Jewish year, and the heathen feasts the data gives a day for. It answers one
// question for now: which of them lie ahead of a date for a faith. What a day forbids is carried as the data words it;
// nothing here enforces it yet.
using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.World;
using Calendar = Kapman.World.Calendar;

namespace Kapman.Game
{
    /// <summary>A day a faith keeps that lies ahead: its name, its kind (feast, fast, rule, custom, season), what may
    /// not be done on it as the data says it, the date, and how many days off it is (0: today).</summary>
    public sealed class FaithDay
    {
        public string id = "", name = "", kind = "", note = "";
        public string[] mayNot = Array.Empty<string>();
        public int year, month, day, daysAhead;
        /// <summary>It comes every week (the Lord's day, the Friday fast, the Sabbath).</summary>
        public bool weekly;

        /// <summary>"work or trade; bring a suit", or "" where the data forbids nothing.</summary>
        public string MayNot => string.Join("; ", mayNot);
    }

    /// <summary>
    /// THE YEAR BY FAITH: what calendar_793.json holds, as days that can be asked for. If the file is missing or does not
    /// parse the calendar is empty and FromFile is false (ContentFiles has said why).
    /// </summary>
    public sealed class FaithCalendar
    {
        public const string File = "story/calendar_793.json";

        sealed class Fixed
        {
            public string id, name, kind, note;
            public string[] faiths, mayNot;
            public int month, day, easterOffset;
            public bool movable;
        }

        sealed class Weekly
        {
            public string id, name, kind;
            public string[] faiths, mayNot;
            public int weekday;                                           // 0 Sunday .. 6 Saturday
        }

        readonly List<Fixed> days = new List<Fixed>();
        readonly List<Weekly> weekly = new List<Weekly>();
        readonly Dictionary<int, (int month, int day)> easter = new Dictionary<int, (int, int)>();
        // the Jewish year: by year, each day's key with its date; and each key's length in days and what it forbids
        readonly Dictionary<int, List<(string key, int month, int day)>> jewish = new Dictionary<int, List<(string, int, int)>>();
        readonly Dictionary<string, (int days, string[] mayNot)> jewishRules = new Dictionary<string, (int, string[])>(StringComparer.Ordinal);
        /// <summary>Feasts the data gives no day for, by faith: their name and when, in the data's words.</summary>
        readonly List<(string name, string when, string[] faiths)> undated = new List<(string, string, string[])>();

        public bool FromFile { get; private set; }

        static readonly ContentCache<FaithCalendar> cache = new ContentCache<FaithCalendar>("the calendar", Load);
        public static FaithCalendar Current => cache.Value;

        static readonly string[] WeekdayNames = { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
        public static readonly string[] WeekdayWords = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        // ------------------------------------------------------------------ the days of the Julian calendar

        /// <summary>Days from 1 January 700 to a date of the Julian calendar.</summary>
        public static int DayNumber(int year, int month, int day)
        {
            int n = 0;
            for (int y = 700; y < year; y++)
                n += Calendar.DaysIn(y);
            for (int m = 1; m < month; m++)
                n += Calendar.DaysIn(year, m);
            return n + day - 1;
        }

        /// <summary>The day of the week of a Julian date, 0 Sunday to 6 Saturday. 1 May 793 was a Wednesday and 8 June 793
        /// a Saturday (the data's own reckoning).</summary>
        public static int Weekday(int year, int month, int day)
        {
            int n = (DayNumber(year, month, day) - DayNumber(793, 5, 1) + 3) % 7;
            return n < 0 ? n + 7 : n;
        }

        static (int year, int month, int day) AddDays(int year, int month, int day, int add)
        {
            var d = Calendar.At(year, DayNumber(year, month, day) - DayNumber(year, 1, 1) + 1 + add, 0.0);
            return (d.year, d.month, d.day);
        }

        // ------------------------------------------------------------------ asking

        static bool Keeps(string[] faiths, string faith, string countsAs)
        {
            if (faiths == null)
                return false;
            foreach (string f in faiths)
                if (f == faith)
                    return true;
            // (a faith the list does not name follows its family: a Greek Christian keeps the Christian days)
            if (!string.IsNullOrEmpty(countsAs) && countsAs != faith && !Named(faith))
                foreach (string f in faiths)
                    if (f == countsAs)
                        return true;
            return false;
        }

        // the faiths the calendar names outright: these are never given their family's days as well
        static bool Named(string faith) =>
            faith == "christian" || faith == "christian_irish" || faith == "old_gods" || faith == "prime_signed" || faith == "jewish";

        /// <summary>
        /// The days a faith keeps from a date on, nearest first: those with a date within `within` days, and the next
        /// coming of each day of the week it keeps. `countsAs` is the faith's family (FaithDef.countsAs), for a faith
        /// the calendar does not name. At most `most` are given.
        /// </summary>
        public List<FaithDay> Ahead(string faith, string countsAs, int year, int month, int day, int within = 60, int most = 8)
        {
            var list = new List<FaithDay>();
            int today = DayNumber(year, month, day);
            for (int y = year; y <= year + 1; y++)
            {
                foreach (var f in days)
                {
                    if (!Keeps(f.faiths, faith, countsAs))
                        continue;
                    int m = f.month, d = f.day;
                    if (f.movable)
                    {
                        if (!easter.TryGetValue(y, out var e))
                            continue;
                        (_, m, d) = AddDays(y, e.month, e.day, f.easterOffset);
                    }
                    if (m < 1 || m > 12 || d < 1 || d > Calendar.DaysIn(y, m))
                        continue;
                    int ahead = DayNumber(y, m, d) - today;
                    if (ahead < 0 || ahead > within)
                        continue;
                    list.Add(new FaithDay { id = f.id, name = f.name, kind = f.kind, note = f.note, mayNot = f.mayNot, year = y, month = m, day = d, daysAhead = ahead });
                }
                if (faith == "jewish" && jewish.TryGetValue(y, out var jy))
                    foreach (var (key, m, d) in jy)
                    {
                        int ahead = DayNumber(y, m, d) - today;
                        if (ahead < 0 || ahead > within)
                            continue;
                        jewishRules.TryGetValue(key, out var rule);
                        list.Add(new FaithDay
                        {
                            id = key, name = JewishName(key) + (rule.days > 1 ? $" ({rule.days} days)" : ""), kind = "feast",
                            mayNot = rule.mayNot ?? Array.Empty<string>(), year = y, month = m, day = d, daysAhead = ahead,
                        });
                    }
            }
            int wd = Weekday(year, month, day);
            foreach (var w in weekly)
            {
                if (w.weekday < 0 || !Keeps(w.faiths, faith, countsAs))
                    continue;
                int ahead = (w.weekday - wd + 7) % 7;
                var (wy, wm, wdd) = AddDays(year, month, day, ahead);
                list.Add(new FaithDay { id = w.id, name = w.name, kind = w.kind, mayNot = w.mayNot, year = wy, month = wm, day = wdd, daysAhead = ahead, weekly = true });
            }
            list.Sort((a, b) => a.daysAhead != b.daysAhead ? a.daysAhead.CompareTo(b.daysAhead) : a.weekly.CompareTo(b.weekly));
            if (list.Count > most)
                list.RemoveRange(most, list.Count - most);
            return list;
        }

        /// <summary>The feasts of a faith the data gives no day for: "Yule: midwinter; the night is not known for 793".</summary>
        public List<(string name, string when)> Undated(string faith, string countsAs)
        {
            var list = new List<(string, string)>();
            foreach (var (name, when, faiths) in undated)
                if (Keeps(faiths, faith, countsAs))
                    list.Add((name, when));
            return list;
        }

        static string JewishName(string key)
        {
            switch (key)
            {
                case "passover": return "Passover";
                case "shavuot": return "Shavuot, the feast of weeks";
                case "new_year": return "The new year";
                case "day_of_atonement": return "The day of atonement";
                case "booths": return "Booths";
                case "purim": return "Purim";
                default: return key.Replace('_', ' ');
            }
        }

        // ------------------------------------------------------------------ reading

        static FaithCalendar Load()
        {
            var cal = new FaithCalendar();
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return cal;
            foreach (var x in ContentJson.Rows(root, "fixed"))
                cal.days.Add(new Fixed
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), kind = ContentJson.Text(x, "kind"), note = ContentJson.Text(x, "note"),
                    faiths = ContentJson.Strings(x, "faiths"), mayNot = ContentJson.Strings(x, "may_not"),
                    month = ContentJson.Int(x, "month"), day = ContentJson.Int(x, "day"),
                });
            foreach (var x in ContentJson.Rows(root, "movable"))
                cal.days.Add(new Fixed
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), kind = ContentJson.Text(x, "kind"), note = ContentJson.Text(x, "note"),
                    faiths = ContentJson.Strings(x, "faiths"), mayNot = ContentJson.Strings(x, "may_not"),
                    movable = true, easterOffset = ContentJson.Int(x, "easter_offset"),
                });
            foreach (var kv in ContentJson.Pairs(root, "easter_by_year"))
                if (int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) &&
                    ContentJson.Date(ContentJson.Text(kv.Value, "easter"), out _, out int m, out int d))
                    cal.easter[y] = (m, d);
            foreach (var x in ContentJson.Rows(root, "week"))
                cal.weekly.Add(new Weekly
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), kind = ContentJson.Text(x, "kind"),
                    faiths = ContentJson.Strings(x, "faiths"), mayNot = ContentJson.Strings(x, "may_not"),
                    weekday = Array.IndexOf(WeekdayNames, ContentJson.Text(x, "weekday").ToLowerInvariant()),
                });
            var jewishYears = Json.Obj(root, "jewish_by_year");
            foreach (var kv in ContentJson.Pairs(root, "jewish_by_year"))
            {
                if (!int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) || !(kv.Value is Dictionary<string, object> row))
                    continue;
                var list = new List<(string, int, int)>();
                foreach (var day in row)
                    if (day.Value is string text && ContentJson.Date(text, out _, out int m, out int d))
                        list.Add((day.Key, m, d));
                cal.jewish[y] = list;
            }
            foreach (var kv in ContentJson.Pairs(jewishYears, "rules"))
                cal.jewishRules[kv.Key] = (Math.Max(1, ContentJson.Int(kv.Value, "days", 1)), ContentJson.Strings(kv.Value, "may_not"));
            foreach (var x in ContentJson.Rows(root, "heathen"))
            {
                string when = ContentJson.Text(x, "when");
                string[] faiths = ContentJson.Strings(x, "faiths");
                if (GameDay(when, out int m, out int d))
                    cal.days.Add(new Fixed
                    {
                        id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), kind = "feast", note = ContentJson.Text(x, "what"),
                        faiths = faiths, mayNot = ContentJson.Strings(x, "may_not"), month = m, day = d,
                    });
                else if (when.Length > 0 && when.Length < 140)
                    cal.undated.Add((ContentJson.Text(x, "name"), when, faiths));
            }
            cal.FromFile = cal.days.Count > 0;
            return cal;
        }

        /// <summary>"mid October; the game's day for Ripa: 14 October (?)" gives 14 October: the day the data settles on
        /// for the game, where it settles on one.</summary>
        static bool GameDay(string when, out int month, out int day)
        {
            month = day = 0;
            int at = (when ?? "").IndexOf("game's day", StringComparison.Ordinal);
            if (at < 0)
                return false;
            int colon = when.IndexOf(':', at);
            if (colon < 0)
                return false;
            string[] words = when.Substring(colon + 1).Trim().Split(' ');
            if (words.Length < 2 || !int.TryParse(words[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
                return false;
            month = Array.IndexOf(Calendar.MonthNames, words[1].Trim('(', ')', '?', '.', ',', ';')) + 1;
            return month >= 1 && day >= 1 && day <= 31;
        }
    }
}
