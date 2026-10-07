using System;
using System.Collections.Generic;
using Kapman.Game;
using Kapman.Game.Sim;

namespace Kapman.Weather
{
    /// <summary>The day's circulation over the North Sea (after Lamb's types, simplified: weather document 2.2).</summary>
    public enum CirculationType
    {
        /// <summary>Anticyclonic: a high over or near the North Sea; light winds, clear or stratus, fog.</summary>
        A,
        /// <summary>Cyclonic: a low over the North Sea; the wind shifting, showers.</summary>
        C,
        /// <summary>Westerly: lows passing north of Scotland; SW-W winds, fronts every one to three days.</summary>
        W,
        /// <summary>South-westerly: the storm track further south, deep lows across the North Sea; the gale type.</summary>
        SW,
        /// <summary>North-westerly: lows into Scandinavia; NW winds, cold showers.</summary>
        NW,
        /// <summary>Northerly: a high to the west; cold, clear, showers on the coast.</summary>
        N,
        /// <summary>Easterly: a blocking high over Scandinavia; E-NE winds, dry; cold in winter, warm in late spring.</summary>
        E,
        /// <summary>Southerly: a high to the east, a low to the west; S-SE winds, mild and moist.</summary>
        S,
    }

    /// <summary>
    /// THE CIRCULATION CHAIN - one circulation type a day, a Markov chain whose 8x8 matrix changes with the month (the
    /// matrices are data: Assets/Kapman/Data/Weather/circulation.json, made by Bake/weather/make_circulation.py so that
    /// each month's chain settles to the measured share of each type). The chain changes state once a day at midnight;
    /// a day between two months' middles draws from the two matrices blended. Each year's days are drawn in one go from
    /// the year's own stream (SimRandom.For(seed ^ (year * 0x9E37), "weather"), weather document 2.2), so the same world
    /// seed replays the same century, any day can be asked for in any order, and a year never depends on the one before.
    /// Thread-safe (a year is drawn once, under a lock, and never changes).
    /// </summary>
    public sealed class CirculationChain
    {
        public const int Count = 8;
        const int WarmUpDays = 30;
        const double YearDays = 365.25;

        readonly int seed;
        readonly double[][,] months = new double[12][,];
        readonly Dictionary<long, CirculationType[]> years = new Dictionary<long, CirculationType[]>();
        readonly object gate = new object();

        public CirculationChain(int seed, double[][,] monthMatrices)
        {
            this.seed = seed;
            if (monthMatrices == null || monthMatrices.Length != 12)
                throw new ArgumentException("the circulation chain needs twelve month matrices");
            for (int m = 0; m < 12; m++)
            {
                var p = monthMatrices[m];
                if (p == null || p.GetLength(0) != Count || p.GetLength(1) != Count)
                    throw new ArgumentException($"the circulation chain's month {m + 1} is not an 8x8 matrix");
                for (int i = 0; i < Count; i++)
                {
                    double row = 0.0;
                    for (int j = 0; j < Count; j++)
                        row += p[i, j];
                    if (Math.Abs(row - 1.0) > 0.01)
                        throw new ArgumentException($"the circulation chain's month {m + 1}, row {(CirculationType)i}, sums to {row:0.000}, not 1");
                }
                months[m] = p;
            }
        }

        /// <summary>Reads the month matrices from circulation.json's root object (the "types" must be A C W SW NW N E S).</summary>
        public static CirculationChain Parse(object root, int seed)
        {
            var types = Json.Arr(root, "types");
            if (types == null || types.Count != Count)
                throw new FormatException("circulation.json: 'types' must list the eight types");
            for (int i = 0; i < Count; i++)
                if (!(types[i] is string s) || s != ((CirculationType)i).ToString())
                    throw new FormatException($"circulation.json: type {i} is '{types[i]}', expected {(CirculationType)i}");
            var ms = Json.Arr(root, "months");
            if (ms == null || ms.Count != 12)
                throw new FormatException("circulation.json: 'months' must hold twelve months");
            var mats = new double[12][,];
            for (int m = 0; m < 12; m++)
            {
                var rows = Json.Arr(ms[m], "matrix");
                if (rows == null || rows.Count != Count)
                    throw new FormatException($"circulation.json: month {m + 1} has no 8x8 matrix");
                mats[m] = new double[Count, Count];
                for (int i = 0; i < Count; i++)
                {
                    var row = rows[i] as List<object>;
                    if (row == null || row.Count != Count)
                        throw new FormatException($"circulation.json: month {m + 1}, row {i} is not eight numbers");
                    for (int j = 0; j < Count; j++)
                        mats[m][i, j] = row[j] is double d ? d : throw new FormatException($"circulation.json: month {m + 1}, row {i}, column {j} is not a number");
                }
            }
            return new CirculationChain(seed, mats);
        }

        /// <summary>The type of a day (whole days since the start of year 0, as the weather model's hours / 24).</summary>
        public CirculationType TypeOn(long day)
        {
            long year = YearOf(day);
            CirculationType[] days;
            lock (gate)
            {
                if (!years.TryGetValue(year, out days))
                {
                    days = DrawYear(year);
                    years[year] = days;
                    if (years.Count > 64)                          // a long session that has wandered the centuries
                    {
                        years.Clear();
                        years[year] = days;
                    }
                }
            }
            return days[(int)(day - FirstDay(year))];
        }

        /// <summary>The type at a moment (game hours since the start of year 0).</summary>
        public CirculationType TypeAt(double hours) => TypeOn((long)Math.Floor(hours / 24.0));

        /// <summary>The chance of each type tomorrow given today's, on a day of the year (the two months' matrices blended).</summary>
        public double Transition(int from, int to, double dayOfYear)
        {
            ClimateNormals.Between(dayOfYear, out int m0, out int m1, out double t);
            return months[m0][from, to] * (1.0 - t) + months[m1][from, to] * t;
        }

        /// <summary>The share of days of each type a month's chain settles to (its stationary distribution).</summary>
        public double[] Stationary(int month)
        {
            var p = months[month];
            var pi = new double[Count];
            for (int i = 0; i < Count; i++)
                pi[i] = 1.0 / Count;
            var next = new double[Count];
            for (int k = 0; k < 2000; k++)
            {
                Array.Clear(next, 0, Count);
                for (int i = 0; i < Count; i++)
                    for (int j = 0; j < Count; j++)
                        next[j] += pi[i] * p[i, j];
                Array.Copy(next, pi, Count);
            }
            return pi;
        }

        /// <summary>The first day (whole days since year 0) of a year: the day that holds the year's first hour.</summary>
        public static long FirstDay(long year) => (long)Math.Floor(year * YearDays);

        /// <summary>The year a day falls in.</summary>
        public static long YearOf(long day)
        {
            long y = (long)Math.Floor(day / YearDays);
            while (FirstDay(y + 1) <= day)
                y++;
            while (FirstDay(y) > day)
                y--;
            return y;
        }

        /// <summary>A year's days, drawn from the year's own stream: a month's warm-up from the stationary share, then a
        /// draw at each midnight from the day's blended matrix.</summary>
        CirculationType[] DrawYear(long year)
        {
            var rng = SimRandom.For(unchecked(seed ^ (int)(year * 0x9E37)), "weather");
            long first = FirstDay(year), end = FirstDay(year + 1);
            var days = new CirculationType[end - first];
            long start = first - WarmUpDays;
            double doy0 = DayOfYear(start);
            var pi = Stationary(ClimateNormals.MonthOf(doy0));
            int state = Pick(pi, rng.Next());
            for (long d = start; d < end; d++)
            {
                if (d > start)
                {
                    double doy = DayOfYear(d);
                    double u = rng.Next(), acc = 0.0;
                    int next = Count - 1;
                    for (int j = 0; j < Count; j++)
                    {
                        acc += Transition(state, j, doy);
                        if (u < acc)
                        {
                            next = j;
                            break;
                        }
                    }
                    state = next;
                }
                if (d >= first)
                    days[d - first] = (CirculationType)state;
            }
            return days;
        }

        static double DayOfYear(long day) => ((day + 0.5) % YearDays + YearDays) % YearDays;

        static int Pick(double[] shares, double u)
        {
            double acc = 0.0;
            for (int i = 0; i < shares.Length; i++)
            {
                acc += shares[i];
                if (u < acc)
                    return i;
            }
            return shares.Length - 1;
        }
    }
}
