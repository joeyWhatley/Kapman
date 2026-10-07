using System;
using System.Collections.Generic;

namespace Kapman.Weather
{
    /// <summary>
    /// HOW THE WIND RAISES THE SEA - the static formulas of the sea state (weather document 1.3, 2.4), at full real height
    /// (the user's decision 4: storm waves at full real height on the sea levels). SI units: m, s, m/s; fetch in metres,
    /// duration in hours where named.
    ///   - the Shore Protection Manual's fetch law in the adjusted wind U_A = 0.71 U10^1.23 (SPM 1984 eq. 3-28a):
    ///     Hs = 0.0016 U_A sqrt(X / g), Tp = 0.2857 (U_A / g) (g X / U_A^2)^(1/3);
    ///   - the Coastal Engineering Manual's duration law (2002), in the raw wind: the time the fetch needs,
    ///     t_x = 77.23 X^0.67 / (U10^0.34 g^0.33) seconds; a shorter blow acts as a shorter fetch;
    ///   - capped by the fully developed sea (Pierson-Moskowitz: Hs = 0.028 U10^2, Tp = 0.83 U10) and by the depth
    ///     (Hs at most half the depth: a working cap; breaking itself is 0.78 h);
    ///   - Rayleigh: the expected highest of N waves is Hs sqrt(0.5 ln N), with N = duration / Tz and Tz = 0.78 Tp;
    ///   - the swell's group speed in deep water, g Tp / (4 pi);
    ///   - the storm surge: 1 cm per hPa below 1013, plus the wind's set-up summed along the fetch's depth profile,
    ///     k U10^2 cos(theta) sum(dF / h) with k = C_d rho_air / (rho_water g).
    /// </summary>
    public static class WaveGrowth
    {
        public const double G = 9.81;
        const double RhoAir = 1.25, RhoWater = 1025.0;

        /// <summary>The SPM's adjusted wind (the "wind stress factor"), m/s: 0.71 U10^1.23.</summary>
        public static double AdjustedWind(double u10) => 0.71 * Math.Pow(Math.Max(u10, 0.0), 1.23);

        /// <summary>The time a wind must blow for a fetch to count in full (CEM 2002), hours.</summary>
        public static double DurationForFetchHours(double fetchMetres, double u10)
        {
            if (u10 <= 0.0 || fetchMetres <= 0.0)
                return 0.0;
            return 77.23 * Math.Pow(fetchMetres, 0.67) / (Math.Pow(u10, 0.34) * Math.Pow(G, 0.33)) / 3600.0;
        }

        /// <summary>The fetch a blow of <paramref name="hours"/> counts as (m): the lesser of the real fetch and the
        /// duration-limited one, X = (t U10^0.34 g^0.33 / 77.23)^(1/0.67).</summary>
        public static double EffectiveFetch(double fetchMetres, double u10, double hours)
        {
            if (u10 <= 0.0 || hours <= 0.0)
                return 0.0;
            double byTime = Math.Pow(hours * 3600.0 * Math.Pow(u10, 0.34) * Math.Pow(G, 0.33) / 77.23, 1.0 / 0.67);
            return Math.Min(fetchMetres, byTime);
        }

        /// <summary>The fetch-limited sea (SPM, in the adjusted wind): significant height (m) and peak period (s).</summary>
        public static void FetchLimited(double u10, double fetchMetres, out double hs, out double tp)
        {
            double ua = AdjustedWind(u10);
            if (ua <= 0.0 || fetchMetres <= 0.0)
            {
                hs = 0.0;
                tp = 0.0;
                return;
            }
            hs = 0.0016 * ua * Math.Sqrt(fetchMetres / G);
            tp = 0.2857 * (ua / G) * Math.Pow(G * fetchMetres / (ua * ua), 1.0 / 3.0);
        }

        /// <summary>The fully developed sea of a wind (Pierson-Moskowitz): Hs = 0.028 U10^2 (m).</summary>
        public static double FullyDevelopedHeight(double u10) => 0.028 * u10 * u10;

        /// <summary>The fully developed sea's peak period: 0.83 U10 (s).</summary>
        public static double FullyDevelopedPeriod(double u10) => 0.83 * u10;

        /// <summary>
        /// The wind sea a wind raises (m, s): over a fetch (m), blowing for some hours, over water of a depth (m; 0 or less
        /// for deep water). Fetch- or duration-limited in the adjusted wind; capped by the fully developed sea (whose height
        /// and period then stand) and by half the depth; in a storm the period is at least 5 sqrt(Hs).
        /// </summary>
        public static void Grow(double u10, double fetchMetres, double hours, double depthMetres, out double hs, out double tp)
        {
            double x = EffectiveFetch(fetchMetres, u10, hours);
            FetchLimited(u10, x, out hs, out tp);
            double full = FullyDevelopedHeight(u10);
            if (hs > full)
            {
                hs = full;
                tp = FullyDevelopedPeriod(u10);
            }
            if (depthMetres > 0.0 && hs > 0.5 * depthMetres)
                hs = 0.5 * depthMetres;
            if (u10 >= 17.2)
                tp = Math.Max(tp, 5.0 * Math.Sqrt(hs));
        }

        /// <summary>The expected highest of N waves of a sea of significant height Hs: Hs sqrt(0.5 ln N) (Rayleigh).</summary>
        public static double ExpectedHighest(double hs, double waves) => waves <= 1.0 ? hs : hs * Math.Sqrt(0.5 * Math.Log(waves));

        /// <summary>The expected highest wave (as a share of Hs) met over a duration (s) in a sea of peak period Tp:
        /// N = duration / Tz, Tz = 0.78 Tp. Thirty minutes at Tp 8 s: about 1.68; three hours: about 1.93.</summary>
        public static double HighestOverHs(double seconds, double tp)
        {
            double tz = 0.78 * Math.Max(tp, 0.1);
            return ExpectedHighest(1.0, seconds / tz);
        }

        /// <summary>The group speed of a swell in deep water (m/s): g Tp / (4 pi). 12 s: 9.4 m/s.</summary>
        public static double GroupSpeed(double tp) => G * tp / (4.0 * Math.PI);

        /// <summary>How long a swell of period Tp takes to cross a distance (km), hours.</summary>
        public static double SwellTravelHours(double km, double tp) => km * 1000.0 / Math.Max(GroupSpeed(tp), 1e-3) / 3600.0;

        /// <summary>The whitecap share of the sea surface (0..1): 3.8e-6 U10^3.4 (Monahan's law, from memory; 10 m/s about
        /// 1%, 20 m/s about 10%).</summary>
        public static double Whitecaps(double u10) => Math.Min(1.0, 3.8e-6 * Math.Pow(Math.Max(u10, 0.0), 3.4));

        /// <summary>
        /// The drag coefficient of the sea surface: 1.2e-3 up to 11 m/s, rising as (0.49 + 0.065 U10) 1e-3 above (Large and
        /// Pond 1981, from memory), held at the weather document's gale value 2.0e-3 from about 23 m/s.
        /// </summary>
        public static double DragCoefficient(double u10)
        {
            double cd = u10 <= 11.0 ? 1.2e-3 : (0.49 + 0.065 * u10) * 1e-3;
            return Math.Min(cd, 2.0e-3);
        }

        /// <summary>The set-up constant k = C_d rho_air / (rho_water g) for a wind, s^2/m (2.5e-7 at gale strength).</summary>
        public static double SetupConstant(double u10) => DragCoefficient(u10) * RhoAir / (RhoWater * G);

        /// <summary>The inverse barometer: 1 cm of sea level for each hPa below 1013 (m; negative under a high).</summary>
        public static double InverseBarometer(double pressureHpa) => 0.01 * (1013.0 - pressureHpa);

        /// <summary>
        /// The storm surge (m): the inverse barometer plus the wind's set-up along the fetch, k U10^2 cos(theta) sum(dF / h),
        /// with the fetch given as stretches of (length m, depth m) from the open sea in to the coast, and theta the angle
        /// between the wind and the fetch's line. The 838 storm (26 m/s NW at 970 hPa over 400 km at 40 m and 100 km at
        /// 20 m) gives about 3 m.
        /// </summary>
        public static double Surge(double pressureHpa, double u10, double cosTheta, IEnumerable<(double lengthMetres, double depthMetres)> fetch)
        {
            double sum = 0.0;
            if (fetch != null)
                foreach (var (length, depth) in fetch)
                    sum += Math.Max(length, 0.0) / Math.Max(depth, 1.0);
            return InverseBarometer(pressureHpa) + SetupConstant(u10) * u10 * u10 * cosTheta * sum;
        }
    }
}
