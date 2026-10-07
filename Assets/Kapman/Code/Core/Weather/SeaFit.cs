using System;

namespace Kapman.Weather
{
    /// <summary>
    /// FROM THE SEA STATE TO HDRP'S WATER (weather document 2.5): HDRP's ocean has one wind for its two swell bands, two
    /// amplitude-only multipliers (0..1) and a repetition size fixed at load, so a model sea (Hs, Tp) is fitted, not
    /// inverted: the wind is chosen for the period, then the multipliers for the height exactly; where HDRP's sea at that
    /// period is lower than the model's (a steep young sea) the wind is raised until the height is met with the
    /// multipliers full, and the period's error is reported.
    ///
    /// The link between HDRP's wind and its waves is HDRP's own: its table of the mean greatest displacement of a band by
    /// patch size and wind (HDRP's k_MaximumAmplitudeTable, 32 x 32, wind 0-250 km/h across, patch 25-5,000 m down, read
    /// by reflection at runtime and passed in here), evaluated as HDRP evaluates it. Two numbers turn it into the sea:
    ///   - <see cref="HsPerAmplitude"/>: the table holds the mean over time of the greatest height among 4,096 points of a
    ///     Gaussian sea, about 3.7 standard deviations; Hs is 4, so Hs is about 1.1 x the table (guess, from the statistics,
    ///     until the measuring tool of 2.5 replaces it with measured heights);
    ///   - <see cref="PeriodPerWind"/>: HDRP's Phillips spectrum, exp(-1/(kL)^2)/k^4 with L = V^2/g, peaks in frequency at
    ///     omega = 0.946 g/V, a period of 0.68 V (s, V in m/s) (derived from the spectrum's form).
    /// Pure arithmetic: the runtime (HdrpSea) owns the HDRP side.
    /// </summary>
    public static class SeaFit
    {
        /// <summary>Significant height over HDRP's tabled greatest displacement (guess, see the class).</summary>
        public const double HsPerAmplitude = 1.1;
        /// <summary>The peak period of HDRP's spectrum per m/s of its wind (derived, see the class).</summary>
        public const double PeriodPerWind = 0.68;
        /// <summary>HDRP's highest swell wind, km/h, and its table's size.</summary>
        public const double MaxWindKmh = 250.0;
        public const int TableSize = 32;

        public struct Result
        {
            /// <summary>HDRP's largeWindSpeed, km/h.</summary>
            public float windKmh;
            /// <summary>largeBand0Multiplier and largeBand1Multiplier, 0..1.</summary>
            public float band0, band1;
            /// <summary>The height the two bands give at full multiplier, m.</summary>
            public float band0Hs, band1Hs;
            /// <summary>The height the fit gives (m) and its period's error (s, HDRP's less the model's).</summary>
            public float hs, tpError;
        }

        /// <summary>HDRP's second swell band's patch size for a repetition size (WaterSystem.EvaluateSwellSecondPatchSize:
        /// the first over a ratio from 5 at 250 m to 50 at 5,000 m).</summary>
        public static double SecondPatch(double repetitionSize)
        {
            double relative = (repetitionSize - 250.0) / (5000.0 - 250.0);
            return repetitionSize / (5.0 + (50.0 - 5.0) * relative);
        }

        /// <summary>The table's greatest displacement of a band (m) at a patch size (m) and wind (km/h), bilinear as HDRP
        /// reads it (EvaluateMaxAmplitude with PrepareCoordinates).</summary>
        public static double MaxAmplitude(float[] table, double patch, double windKmh)
        {
            if (table == null || table.Length < TableSize * TableSize)
                return 0.0;
            double u = windKmh / MaxWindKmh, v = Math.Max(0.0, Math.Min(1.0, (patch - 25.0) / 4975.0));
            double x = u * (TableSize - 1) - 0.5, y = v * (TableSize - 1) - 0.5;
            double fx0 = Math.Floor(x), fy0 = Math.Floor(y);
            int x0 = (int)fx0, y0 = (int)fy0;
            double fx = x - fx0, fy = y - fy0;
            double p0 = Tap(table, x0, y0), p1 = Tap(table, x0 + 1, y0), p2 = Tap(table, x0, y0 + 1), p3 = Tap(table, x0 + 1, y0 + 1);
            double i0 = p0 + (p1 - p0) * fx, i1 = p2 + (p3 - p2) * fx;
            return i0 + (i1 - i0) * fy;
        }

        static double Tap(float[] table, int x, int y)
        {
            x = Math.Max(0, Math.Min(TableSize - 1, x));
            y = Math.Max(0, Math.Min(TableSize - 1, y));
            return table[x + y * TableSize];
        }

        /// <summary>The significant height (m) of one band at full multiplier.</summary>
        public static double BandHs(float[] table, double patch, double windKmh) => HsPerAmplitude * MaxAmplitude(table, patch, windKmh);

        /// <summary>
        /// The fit for a whole sea of height Hs (m) whose larger part has peak period Tp (s), on a surface of a repetition
        /// size (m). The wind sea's steepness (its Hs and Tp) sets how much of the short second band shows: a young steep sea
        /// is all chop, a swell almost none.
        /// </summary>
        public static Result Fit(float[] table, double repetitionSize, double hs, double tp, double windSeaHs, double windSeaTp)
        {
            var r = new Result();
            if (hs <= 0.005 || table == null)
                return r;
            double second = SecondPatch(repetitionSize);
            double l0 = WaveGrowth.G * windSeaTp * windSeaTp / (2.0 * Math.PI);
            double steep = l0 > 0.01 ? windSeaHs / l0 : 0.0;
            double m1 = Math.Max(0.3, Math.Min(1.0, 0.3 + 10.0 * steep));
            double v = Math.Max(1.0, Math.Min(MaxWindKmh, tp / PeriodPerWind * 3.6));
            double Reach(double w)
            {
                double h0 = BandHs(table, repetitionSize, w), h1 = m1 * BandHs(table, second, w);
                return Math.Sqrt(h0 * h0 + h1 * h1);
            }
            if (Reach(v) < hs)
            {
                // HDRP's sea at this period is lower than the model's: raise the wind until the height is met
                double lo = v, hi = MaxWindKmh;
                if (Reach(hi) < hs)
                    v = hi;
                else
                {
                    for (int i = 0; i < 40; i++)
                    {
                        double mid = 0.5 * (lo + hi);
                        if (Reach(mid) < hs)
                            lo = mid;
                        else
                            hi = mid;
                    }
                    v = hi;
                }
            }
            double b0 = BandHs(table, repetitionSize, v), b1 = BandHs(table, second, v);
            double chop = Math.Min(m1 * b1, hs);
            r.windKmh = (float)v;
            r.band0Hs = (float)b0;
            r.band1Hs = (float)b1;
            r.band1 = b1 > 1e-6 ? (float)(chop / b1) : 0f;
            r.band0 = b0 > 1e-6 ? (float)Math.Min(1.0, Math.Sqrt(Math.Max(0.0, hs * hs - chop * chop)) / b0) : 0f;
            double h0f = r.band0 * b0, h1f = r.band1 * b1;
            r.hs = (float)Math.Sqrt(h0f * h0f + h1f * h1f);
            r.tpError = (float)(PeriodPerWind * v / 3.6 - tp);
            return r;
        }
    }
}
