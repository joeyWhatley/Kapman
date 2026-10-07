using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Weather
{
    /// <summary>
    /// THE CLIMATE NORMALS - the weather engine's first layer (Design/weather_and_sea_state.md 2.1): the measured monthly
    /// weather of each place class (the open coast, the Wadden coast, the Zeeland and Flemish shore, the river land, the
    /// open sea), read from Assets/Kapman/Data/Weather/climate_normals.json (made by Bake/weather/make_normals.py from
    /// KNMI's station data, 1991-2020; credit "Source: KNMI"), and the year's offset from the modern climate (the user's
    /// decision 17: 793 is the modern normals cooled by 0.8 C, warming into the Medieval Climate Anomaly by about 950).
    /// Monthly values stand at the middle of their month and are interpolated by the day. Pure C#, read-only once made.
    /// </summary>
    public sealed class ClimateNormals
    {
        public const string ProjectPath = "Assets/Kapman/Data/Weather/climate_normals.json";
        const double MonthDays = 365.25 / 12.0;

        /// <summary>True while the file was typed from the design document's tables rather than made from the measured series.</summary>
        public bool Provisional { get; private set; }
        /// <summary>When the bake made the file ("2026-10-04"), or empty.</summary>
        public string Regenerated { get; private set; } = "";
        /// <summary>The credit lines the data's licences ask for ("Source: KNMI").</summary>
        public readonly List<string> Credits = new List<string>();

        readonly Dictionary<string, PlaceClass> classes = new Dictionary<string, PlaceClass>();
        readonly double[] seaTemp = new double[12];
        double earlyOffset = -0.8, earlyUntil = 900.0, warmBy = 950.0, mcaSummer = 0.3, mcaWinter = 0.0;
        readonly List<(double from, double to, double offset)> events = new List<(double, double, double)>();

        /// <summary>One place class: where its station stands and its twelve months.</summary>
        public sealed class PlaceClass
        {
            public string id = "", station = "";
            public double lat, lon;
            /// <summary>The class whose measurements this one borrows (the open sea takes the open coast's), or empty.</summary>
            public string derivedFrom = "";
            public readonly Month[] months = new Month[12];
            /// <summary>Days a year whose strongest hourly mean reached force 8, force 9, force 10 (measured).</summary>
            public double galeDays, stormDays, f10Days;
        }

        /// <summary>One month of a place class.</summary>
        public sealed class Month
        {
            /// <summary>The hourly mean wind (m/s) and the daily mean.</summary>
            public double windMean, dailyWindMean;
            /// <summary>% of hours at force 0..12, as measured.</summary>
            public double[] beaufort = new double[13];
            /// <summary>% of hours at force 0..12 with the day's cycle of the wind divided out: the distribution the engine
            /// maps its own wind onto before it puts the day's cycle back.</summary>
            public double[] beaufortNoDay = new double[13];
            /// <summary>The wind by UT hour (0 = the hour ending 01 UT) over the month's mean.</summary>
            public double[] windByHour = Filled(24, 1.0);
            /// <summary>The temperature by UT hour less the month's hourly mean, C.</summary>
            public double[] tempByHour = new double[24];
            /// <summary>The 3-second gust over the hourly mean, in a wind of 5 m/s or more.</summary>
            public double gustFactor = 1.55;
            /// <summary>Daily mean, mean minimum and mean maximum temperature, C.</summary>
            public double tMean, tMin, tMax;
            /// <summary>% of days whose strongest hourly mean reached force 8.</summary>
            public double daysF8;
            /// <summary>The cumulative shares at each force's lower edge (0..1, 14 values, forces 0..13), of
            /// <see cref="beaufortNoDay"/>, for the inverse look-up.</summary>
            internal double[] cdf = new double[14];
        }

        // ------------------------------------------------------------------------------------------- reading

        /// <summary>Reads the normals from a file.</summary>
        public static ClimateNormals Load(string path) => Parse(File.ReadAllText(path));

        /// <summary>Reads the normals from their JSON text; a file without classes or months throws (FormatException).</summary>
        public static ClimateNormals Parse(string json)
        {
            if (!(Json.Parse(json) is Dictionary<string, object> root))
                throw new FormatException("climate_normals.json: the top level is not an object");
            var n = new ClimateNormals
            {
                Provisional = Json.Bool(root, "provisional", true),
                Regenerated = Json.Str(root, "regenerated"),
            };
            var cls = Json.Obj(root, "classes");
            if (cls == null || cls.Count == 0)
                throw new FormatException("climate_normals.json: no classes");
            foreach (var kv in cls)
                n.classes[kv.Key] = ReadClass(kv.Key, kv.Value);

            var sea = Json.Arr(Json.Obj(root, "seaTemp"), "months");
            for (int m = 0; m < 12; m++)
                n.seaTemp[m] = sea != null && m < sea.Count && sea[m] is double d ? d : 10.0;

            var y = Json.Obj(root, "yearOffset");
            if (y != null)
            {
                n.earlyOffset = Json.Num(y, "earlyOffset", n.earlyOffset);
                n.earlyUntil = Json.Num(y, "earlyUntil", n.earlyUntil);
                n.warmBy = Json.Num(y, "warmBy", n.warmBy);
                n.mcaSummer = Json.Num(y, "mcaSummer", n.mcaSummer);
                n.mcaWinter = Json.Num(y, "mcaWinter", n.mcaWinter);
                foreach (var e in Json.Arr(y, "events") ?? new List<object>())
                    n.events.Add((Json.Num(e, "from"), Json.Num(e, "to"), Json.Num(e, "offset")));
            }
            foreach (var c in Json.Arr(root, "credits") ?? new List<object>())
            {
                string credit = Json.Str(c, "credit");
                if (credit.Length > 0)
                    n.Credits.Add(credit + " (" + Json.Str(c, "what") + ")");
            }
            return n;
        }

        static PlaceClass ReadClass(string id, object o)
        {
            var c = new PlaceClass
            {
                id = id,
                station = Json.Str(o, "station"),
                lat = Json.Num(o, "lat"),
                lon = Json.Num(o, "lon"),
                derivedFrom = Json.Str(o, "derivedFrom"),
            };
            var months = Json.Arr(o, "months");
            if (months == null || months.Count != 12)
                throw new FormatException($"climate_normals.json: class {id} has no twelve months");
            for (int m = 0; m < 12; m++)
            {
                var r = months[m];
                var mo = new Month
                {
                    windMean = Json.Num(r, "windMean"),
                    dailyWindMean = Json.Num(r, "dailyWindMean"),
                    gustFactor = Json.Num(r, "gustFactor", 1.55),
                    tMean = Json.Num(r, "tMean"),
                    tMin = Json.Num(r, "tMin"),
                    tMax = Json.Num(r, "tMax"),
                    daysF8 = Json.Num(r, "daysF8"),
                };
                Numbers(r, "beaufort", mo.beaufort);
                Numbers(r, "beaufortNoDay", mo.beaufortNoDay);
                if (Sum(mo.beaufortNoDay) <= 0.0)
                    Array.Copy(mo.beaufort, mo.beaufortNoDay, 13);
                if (Sum(mo.beaufortNoDay) <= 0.0)
                    throw new FormatException($"climate_normals.json: class {id} month {m + 1} has no wind shares");
                Numbers(r, "windByHour", mo.windByHour);
                Numbers(r, "tempByHour", mo.tempByHour);
                double total = Sum(mo.beaufortNoDay), acc = 0.0;
                mo.cdf[0] = 0.0;
                for (int f = 0; f < 13; f++)
                {
                    acc += mo.beaufortNoDay[f] / total;
                    mo.cdf[f + 1] = Math.Min(1.0, acc);
                }
                mo.cdf[13] = 1.0;
                c.months[m] = mo;
            }
            var year = Json.Obj(o, "year");
            c.galeDays = Json.Num(year, "galeDays");
            c.stormDays = Json.Num(year, "stormDays");
            c.f10Days = Json.Num(year, "f10Days");
            return c;
        }

        static void Numbers(object o, string key, double[] into)
        {
            var a = Json.Arr(o, key);
            if (a == null)
                return;
            for (int i = 0; i < into.Length && i < a.Count; i++)
                if (a[i] is double d)
                    into[i] = d;
        }

        static double Sum(double[] a)
        {
            double s = 0.0;
            foreach (double x in a)
                s += x;
            return s;
        }

        static double[] Filled(int n, double v)
        {
            var a = new double[n];
            for (int i = 0; i < n; i++)
                a[i] = v;
            return a;
        }

        // ------------------------------------------------------------------------------------------- looking up

        /// <summary>The place classes in the file.</summary>
        public IEnumerable<PlaceClass> Classes => classes.Values;

        /// <summary>A place class by its id; an id not in the file throws (KeyNotFoundException) naming it.</summary>
        public PlaceClass Class(string id)
        {
            if (id != null && classes.TryGetValue(id, out var c))
                return c;
            throw new KeyNotFoundException($"climate_normals.json has no place class '{id}' (it has {string.Join(", ", classes.Keys)})");
        }

        public bool Has(string id) => id != null && classes.ContainsKey(id);

        /// <summary>The two months a day of the year falls between (their middles), and how far it is from the first.</summary>
        public static void Between(double dayOfYear, out int m0, out int m1, out double t)
        {
            double x = dayOfYear / MonthDays - 0.5;                       // 0 at the middle of January
            double f = Math.Floor(x);
            t = x - f;
            m0 = (((int)f % 12) + 12) % 12;
            m1 = (m0 + 1) % 12;
        }

        /// <summary>The month (0-11) a day of the year falls in.</summary>
        public static int MonthOf(double dayOfYear) => Math.Max(0, Math.Min(11, (int)Math.Floor(dayOfYear / MonthDays)));

        /// <summary>The wind (m/s, with the day's cycle divided out) at a quantile (0..1) of the month's measured
        /// distribution: the inverse of the cumulative shares, linear within each force.</summary>
        public static double WindAtQuantile(Month m, double u)
        {
            u = Math.Max(0.0, Math.Min(1.0, u));
            var c = m.cdf;
            for (int f = 0; f < 13; f++)
            {
                if (u <= c[f + 1] || f == 12)
                {
                    double lo = Beaufort.Edge(f), hi = Beaufort.Edge(f + 1);
                    double span = c[f + 1] - c[f];
                    double t = span > 1e-12 ? (u - c[f]) / span : 0.0;
                    return lo + (hi - lo) * Math.Max(0.0, Math.Min(1.0, t));
                }
            }
            return Beaufort.Top;
        }

        /// <summary>The day's cycle of the wind at a UT hour (fractional), as a factor on the month's mean.</summary>
        public static double WindByHour(Month m, double utHour) => ByHour(m.windByHour, utHour);

        /// <summary>The day's cycle of the temperature at a UT hour (fractional), C about the month's mean.</summary>
        public static double TempByHour(Month m, double utHour) => ByHour(m.tempByHour, utHour);

        /// <summary>A value by UT hour, the hours standing at the middle of the hour they end (index 0: 00:30 UT).</summary>
        static double ByHour(double[] a, double utHour)
        {
            double x = utHour - 0.5;
            double f = Math.Floor(x);
            int i0 = (((int)f % 24) + 24) % 24, i1 = (i0 + 1) % 24;
            double t = x - f;
            return a[i0] + (a[i1] - a[i0]) * t;
        }

        /// <summary>The mean daily temperature of a class on a day of the year (modern normals, before the year's offset), C.</summary>
        public double TempMean(PlaceClass c, double dayOfYear)
        {
            Between(dayOfYear, out int m0, out int m1, out double t);
            return c.months[m0].tMean + (c.months[m1].tMean - c.months[m0].tMean) * t;
        }

        /// <summary>The temperature of a class at a day of the year and a UT hour, modern normals, C: the day's mean and the
        /// month's measured day cycle (scaled by <paramref name="cycleScale"/>: less under cloud and in wind, damped at sea).</summary>
        public double Temperature(PlaceClass c, double dayOfYear, double utHour, double cycleScale)
        {
            Between(dayOfYear, out int m0, out int m1, out double t);
            double cycle = TempByHour(c.months[m0], utHour) * (1.0 - t) + TempByHour(c.months[m1], utHour) * t;
            return TempMean(c, dayOfYear) + cycle * cycleScale;
        }

        /// <summary>The sea's surface temperature on a day of the year, C (the file's table; a guess until measured).</summary>
        public double SeaTemp(double dayOfYear)
        {
            Between(dayOfYear, out int m0, out int m1, out double t);
            return seaTemp[m0] + (seaTemp[m1] - seaTemp[m0]) * t;
        }

        /// <summary>How much of summer there is on a day of the year: 0 in mid-January, 1 in mid-July.</summary>
        public static double Summerness(double dayOfYear) => 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * (dayOfYear - 15.0) / 365.25);

        /// <summary>
        /// The year's offset from the modern normals, C (the user's decision 17): the early offset (-0.8) until 900, then a
        /// ramp to the Medieval Climate Anomaly by 950 (summers +0.3 over the modern normals, winters as now), and the
        /// file's dated events on top (Eldgja's haze, 939-942). <paramref name="year"/> is fractional (793.5 is about
        /// midsummer 793).
        /// </summary>
        public double YearOffset(double year, double summerness)
        {
            double mca = mcaWinter + (mcaSummer - mcaWinter) * Math.Max(0.0, Math.Min(1.0, summerness));
            double o;
            if (year <= earlyUntil)
                o = earlyOffset;
            else if (year >= warmBy)
                o = mca;
            else
                o = earlyOffset + (mca - earlyOffset) * (year - earlyUntil) / (warmBy - earlyUntil);
            foreach (var e in events)
                if (year >= e.from && year < e.to)
                    o += e.offset;
            return o;
        }
    }
}
