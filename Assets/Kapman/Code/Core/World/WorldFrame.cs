using System;

namespace Kapman.World
{
    /// <summary>
    /// THE WORLD FRAME - one map projection and one grid for every bake and for the game.
    /// A copy of Bake/kapman_bake/world.py: change one, change both (WorldFrameTests checks that they agree).
    ///
    ///   Projection  Lambert conformal conic on WGS84: standard parallels 45N and 65N, centred on 56N 5E.
    ///   In Unity    x = east (metres), z = north (metres), y = up (height above sea level, metres).
    ///   Levels      level L has cells of 30 * 2^L metres: L0 = 30 m, L1 = 60 m ... L4 = 480 m.
    /// </summary>
    public static class WorldFrame
    {
        public const double CentreLat = 56.0, CentreLon = 5.0;     // where x = 0 and z = 0
        public const double Parallel1 = 45.0, Parallel2 = 65.0;    // the true-scale parallels
        public const double BaseCell = 30.0;                        // metres per cell at level 0

        /// <summary>Metres per cell at a level.</summary>
        public static double Cell(int level) => BaseCell * (1 << level);

        // the WGS84 ellipsoid
        const double A = 6378137.0;
        const double Flattening = 1.0 / 298.257223563;
        static readonly double E = Math.Sqrt(Flattening * (2.0 - Flattening));
        const double Deg = Math.PI / 180.0;

        // the projection's constants (Snyder, "Map Projections: A Working Manual", chapter 15)
        static readonly double n, aF, rho0;

        static WorldFrame()
        {
            double m1 = M(Parallel1 * Deg), m2 = M(Parallel2 * Deg);
            double t1 = T(Parallel1 * Deg), t2 = T(Parallel2 * Deg);
            n = (Math.Log(m1) - Math.Log(m2)) / (Math.Log(t1) - Math.Log(t2));
            aF = A * m1 / (n * Math.Pow(t1, n));
            rho0 = aF * Math.Pow(T(CentreLat * Deg), n);
        }

        static double M(double phi)
        {
            double s = Math.Sin(phi);
            return Math.Cos(phi) / Math.Sqrt(1.0 - E * E * s * s);
        }

        static double T(double phi)
        {
            double s = Math.Sin(phi);
            return Math.Tan(Math.PI / 4.0 - phi / 2.0) / Math.Pow((1.0 - E * s) / (1.0 + E * s), E / 2.0);
        }

        /// <summary>Latitude and longitude (degrees) to world x (east) and z (north), in metres.</summary>
        public static (double x, double z) ToWorld(double lat, double lon)
        {
            double rho = aF * Math.Pow(T(lat * Deg), n);
            double theta = n * (lon - CentreLon) * Deg;
            return (rho * Math.Sin(theta), rho0 - rho * Math.Cos(theta));
        }

        /// <summary>World x (east) and z (north), in metres, to latitude and longitude (degrees).</summary>
        public static (double lat, double lon) ToLatLon(double x, double z)
        {
            double dy = rho0 - z;
            double rho = Math.Sqrt(x * x + dy * dy);          // n > 0: the cone opens to the north
            double theta = Math.Atan2(x, dy);
            double t = Math.Pow(rho / aF, 1.0 / n);
            double phi = Math.PI / 2.0 - 2.0 * Math.Atan(t);
            for (int i = 0; i < 15; i++)                       // converges in a handful of steps
            {
                double s = Math.Sin(phi);
                double next = Math.PI / 2.0 - 2.0 * Math.Atan(t * Math.Pow((1.0 - E * s) / (1.0 + E * s), E / 2.0));
                bool settled = Math.Abs(next - phi) < 1e-14;
                phi = next;
                if (settled) break;
            }
            return (phi / Deg, theta / n / Deg + CentreLon);
        }
    }
}
