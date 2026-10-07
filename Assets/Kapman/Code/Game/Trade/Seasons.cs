using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>A season of making (seasons_793.json): the share of a market's usual stock it settles toward on each day
    /// of the year, for the goods it names (the harvest, the vintage, the herring runs).</summary>
    public sealed class MakeCurve
    {
        public string id = "", note = "";
        public readonly List<string> goods = new List<string>();
        /// <summary>The share for each day of a common year (365 of them, from 1 January), smoothed.</summary>
        public float[] days = new float[Seasons.YearDays];
    }

    /// <summary>
    /// THE YEAR IN THE MARKETS (economy design 2.2 and 2.3, night two): two tables by day of the year, worked out once.
    ///   - The season's price: each good's cheap and dear months (goods_793.json, trade_rules' cheap and dear), blended
    ///     with a cosine over about 20 days at each edge, so a price drifts into its season instead of jumping on the
    ///     first of the month. A month's middle is the month's own value, as before.
    ///   - The making: the share of its usual stock a market settles toward through the year (seasons_793.json): a
    ///     granary that runs low in spring and fills at the harvest, the new wine after the vintage, the herring of the
    ///     autumn runs. The stock carries the year, so a spring market is dear because its granary is low.
    /// Also the economy's calendar: the date game day 0 falls on (the level clock's start), so a market knows the day of
    /// the year and the year it is in. A leap year's 29 February reads the 28th's row.
    /// </summary>
    public static class Seasons
    {
        public const int YearDays = 365;
        static readonly int[] FirstDay = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };   // in a common year

        /// <summary>The year and the day of the year (1 January = 1) that game day 0 begins on: the level clock's start.</summary>
        public static int StartYear { get; private set; } = 793;
        public static int StartDay { get; private set; } = 172;
        static long startAbsolute = Absolute(793, 172);

        /// <summary>Days over which a cheap or dear month blends in or out (centred on the month's first day).</summary>
        public static float EdgeDays { get; private set; } = 20f;

        static readonly List<MakeCurve> curves = new List<MakeCurve>();
        static readonly Dictionary<string, MakeCurve> curveOf = new Dictionary<string, MakeCurve>();
        static readonly Dictionary<string, float[]> priceTables = new Dictionary<string, float[]>();
        static bool loaded;

        /// <summary>The seasons of making read (seasons_793.json; read from the Data folder the first time a market needs
        /// them if nobody has loaded them).</summary>
        public static IReadOnlyList<MakeCurve> Curves
        {
            get
            {
                EnsureLoaded();
                return curves;
            }
        }

        // ---- the calendar

        /// <summary>Sets the date game day 0 begins on (the level clock's year and start day). Markets already open read
        /// their dates again with <see cref="Markets.Recalendar"/>.</summary>
        public static void SetStart(int year, int dayOfYear)
        {
            StartYear = Math.Max(1, year);
            StartDay = Mathf.Clamp(dayOfYear, 1, Calendar.DaysIn(StartYear));
            startAbsolute = Absolute(StartYear, StartDay);
        }

        // days from 1 January of year 0 (Julian: every fourth year a leap year, year 0 among them)
        static long Absolute(int year, int dayOfYear) => 365L * year + (year + 3) / 4 + (dayOfYear - 1);

        /// <summary>The year and the day of the year (1-366) a game day falls in.</summary>
        public static void DateOf(double day, out int year, out int dayOfYear)
        {
            long a = startAbsolute + (long)Math.Floor(day);
            if (a < 0)
                a = 0;
            long cycle = a / 1461, rest = a % 1461;                      // four years: a leap year, then three common ones
            year = (int)(cycle * 4);
            if (rest < 366)
            {
                dayOfYear = (int)rest + 1;
                return;
            }
            rest -= 366;
            year += 1 + (int)(rest / 365);
            dayOfYear = (int)(rest % 365) + 1;
        }

        /// <summary>The year a game day falls in.</summary>
        public static int YearOf(double day)
        {
            DateOf(day, out int year, out _);
            return year;
        }

        /// <summary>The game day a date begins (midnight; before the start it is negative).</summary>
        public static double DayOf(int year, int dayOfYear) => Absolute(year, dayOfYear) - startAbsolute;

        /// <summary>The game day a date begins, by month and day.</summary>
        public static double DayOf(int year, int month, int day)
        {
            int doy = Math.Max(1, day);
            for (int m = 1; m < Mathf.Clamp(month, 1, 12); m++)
                doy += Calendar.DaysIn(year, m);
            return DayOf(year, doy);
        }

        /// <summary>The place in the year's tables (0-364) of a day of the year; a leap year's 29 February reads the 28th.</summary>
        public static int Index(int year, int dayOfYear)
        {
            int i = dayOfYear - 1;
            if (Calendar.Leap(year) && dayOfYear >= 60)
                i = Math.Max(58, i - 1);
            return Mathf.Clamp(i, 0, YearDays - 1);
        }

        /// <summary>The place in the tables of a game day.</summary>
        public static int IndexOf(double day)
        {
            DateOf(day, out int year, out int doy);
            return Index(year, doy);
        }

        /// <summary>The month (1-12) of a place in the tables.</summary>
        public static int MonthOfIndex(int index)
        {
            int m = 11;
            while (m > 0 && FirstDay[m] > index)
                m--;
            return m + 1;
        }

        /// <summary>The place in the tables of a month's 15th: the month's own season, away from its edges.</summary>
        public static int MidMonth(int month) => FirstDay[Mathf.Clamp(month, 1, 12) - 1] + 14;

        // ---- the season's price

        /// <summary>A good's season as a share of its price on each day of the year (cheap and dear months blended at
        /// the edges). Goods with the same months share one table.</summary>
        public static float[] PriceTable(GoodDef g, TradeRules rules)
        {
            if (g == null || rules == null)
                return null;
            EnsureLoaded();
            string key = string.Join(",", g.cheap) + "|" + string.Join(",", g.dear) + "|" +
                         rules.cheap.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "|" + rules.dear.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "|" +
                         EdgeDays.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            if (!priceTables.TryGetValue(key, out var table))
            {
                table = BuildPriceTable(g, rules);
                priceTables[key] = table;
            }
            return table;
        }

        /// <summary>A good's season as a share of its price on a day of the year (the table's place), by today's rules.</summary>
        public static float PriceFactor(GoodDef g, int index)
        {
            var t = PriceTable(g, TradeRules.Current);
            return t != null ? t[Mathf.Clamp(index, 0, YearDays - 1)] : 1f;
        }

        static float[] BuildPriceTable(GoodDef g, TradeRules rules)
        {
            var month = new float[13];
            for (int m = 1; m <= 12; m++)
                month[m] = Array.IndexOf(g.cheap, m) >= 0 ? rules.cheap : Array.IndexOf(g.dear, m) >= 0 ? rules.dear : 1f;   // (as Market.Season)
            var t = new float[YearDays];
            for (int d = 0; d < YearDays; d++)
                t[d] = month[MonthOfIndex(d)];
            // each edge where the season changes: half a cosine from the old month's value to the new one's, centred on
            // the new month's first day (the edges of one month never meet: the shortest month is longer than the blend)
            float width = EdgeDays, half = width * 0.5f;
            int reach = Mathf.CeilToInt(half);
            for (int m = 1; m <= 12; m++)
            {
                float a = month[m == 1 ? 12 : m - 1], b = month[m];
                if (Mathf.Approximately(a, b))
                    continue;
                int edge = FirstDay[m - 1];
                for (int k = -reach; k < reach; k++)
                {
                    float x = Mathf.Clamp01((k + 0.5f + half) / width);
                    t[(edge + k + YearDays) % YearDays] = a + (b - a) * (1f - Mathf.Cos(Mathf.PI * x)) * 0.5f;
                }
            }
            return t;
        }

        // ---- the making

        /// <summary>A good's season of making: the share of the usual stock a market settles toward on each day of the
        /// year (null: the same all year).</summary>
        public static float[] MakeTable(GoodDef g)
        {
            if (g == null)
                return null;
            EnsureLoaded();
            return curveOf.TryGetValue(g.id, out var c) ? c.days : null;
        }

        /// <summary>The share of its usual stock a market settles toward for a good on a day of the year (1 when the
        /// good has no season of making).</summary>
        public static float MakeFactor(GoodDef g, int index)
        {
            var t = MakeTable(g);
            return t != null ? t[Mathf.Clamp(index, 0, YearDays - 1)] : 1f;
        }

        public static void Load(TextAsset json) => Load(json != null ? json.text : null);

        /// <summary>Reads seasons_793.json: the blend's width and the seasons of making.</summary>
        public static void Load(string text)
        {
            loaded = true;
            curves.Clear();
            curveOf.Clear();
            priceTables.Clear();
            EdgeDays = 20f;
            if (string.IsNullOrEmpty(text))
                return;
            var root = Json.Parse(text);
            EdgeDays = Mathf.Clamp((float)Json.Num(root, "edge_days", 20.0), 1f, 60f);
            int smooth = (int)Json.Num(root, "smooth_days", 21.0);
            foreach (var o in Json.Arr(root, "make") ?? new List<object>())
            {
                var points = Points(Json.Obj(o, "points"));
                if (points.Count == 0)
                    continue;
                var c = new MakeCurve { id = Json.Str(o, "id"), note = Json.Str(o, "note") };
                foreach (var x in Json.Arr(o, "goods") ?? new List<object>())
                    if (x is string s && s.Length > 0)
                        c.goods.Add(s);
                c.days = Curve(points, (int)Json.Num(o, "smooth_days", smooth));
                curves.Add(c);
                foreach (var id in c.goods)
                    curveOf[id] = c;
            }
        }

        /// <summary>Reads the tables from the Data folder the first time they are needed, if nobody loaded them.</summary>
        static void EnsureLoaded()
        {
            if (loaded)
                return;
            loaded = true;
            Load(GameFiles.Find("seasons_793"));
        }

        // "MM-DD": share, in the year's order
        static List<(int day, float share)> Points(Dictionary<string, object> obj)
        {
            var list = new List<(int day, float share)>();
            if (obj == null)
                return list;
            foreach (var kv in obj)
            {
                var parts = kv.Key.Split('-');
                if (parts.Length != 2 || !(kv.Value is double v) ||
                    !int.TryParse(parts[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int m) ||
                    !int.TryParse(parts[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int d) || m < 1 || m > 12)
                    continue;
                int day = Mathf.Clamp(FirstDay[m - 1] + d - 1, 0, YearDays - 1);
                list.Add((day, Mathf.Max(0f, (float)v)));
            }
            list.Sort((a, b) => a.day.CompareTo(b.day));
            return list;
        }

        // straight lines between the points, round the year's end, then a moving average over smoothDays to round the corners
        static float[] Curve(List<(int day, float share)> points, int smoothDays)
        {
            int n = points.Count;
            var raw = new float[YearDays];
            for (int d = 0; d < YearDays; d++)
            {
                int after = 0;
                while (after < n && points[after].day <= d)
                    after++;
                int a = after == 0 ? n - 1 : after - 1;                 // the point before (last year's if none this year)
                int b = after == n ? 0 : after;                         // the point after (next year's if none this year)
                float da = points[a].day - (after == 0 ? YearDays : 0);
                float db = points[b].day + (after == n ? YearDays : 0);
                float t = db > da ? (d - da) / (db - da) : 0f;
                raw[d] = Mathf.Lerp(points[a].share, points[b].share, t);
            }
            int reach = Math.Max(0, smoothDays / 2);
            if (reach == 0)
                return raw;
            var smooth = new float[YearDays];
            for (int d = 0; d < YearDays; d++)
            {
                double sum = 0.0;
                for (int k = -reach; k <= reach; k++)
                    sum += raw[((d + k) % YearDays + YearDays) % YearDays];
                smooth[d] = (float)(sum / (2 * reach + 1));
            }
            return smooth;
        }

        // Enter Play Mode runs without a domain reload: each play reads the tables again and starts on the default date
        // (the level's clock sets its own through EconomyDevKeys or KapmanGame)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            loaded = false;
            curves.Clear();
            curveOf.Clear();
            priceTables.Clear();
            SetStart(793, 172);
        }
    }
}
