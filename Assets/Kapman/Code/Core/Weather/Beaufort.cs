using System;

namespace Kapman.Weather
{
    /// <summary>
    /// The Beaufort scale as the weather engine counts it. Lower bounds of forces 1-12 in m/s at 10 m
    /// (https://en.wikipedia.org/wiki/Beaufort_scale): 0.3, 1.6, 3.4, 5.5, 8.0, 10.8, 13.9, 17.2, 20.8, 24.5, 28.5, 32.7.
    /// KNMI reports the hourly wind in whole tenths, so a reading of 1.6 is force 2 and 1.5 is force 1: as a continuous
    /// speed each force begins half a tenth below its bound (<see cref="Edge"/>), which is how the measured shares of the
    /// climate normals and the engine's own speeds are compared.
    /// </summary>
    public static class Beaufort
    {
        /// <summary>The lower bounds of forces 1 to 12, m/s.</summary>
        public static readonly double[] LowerBounds = { 0.3, 1.6, 3.4, 5.5, 8.0, 10.8, 13.9, 17.2, 20.8, 24.5, 28.5, 32.7 };

        /// <summary>The fastest wind the scale's top band is taken to reach (m/s): force 12 is "32.7 and over"; the engine's
        /// winds are capped here.</summary>
        public const double Top = 40.0;

        /// <summary>Where force <paramref name="force"/> (0-13) begins as a continuous speed: 0 for force 0, the bound less
        /// half a tenth for forces 1-12, and <see cref="Top"/> for 13 (the end of force 12).</summary>
        public static double Edge(int force) => force <= 0 ? 0.0 : force >= 13 ? Top : LowerBounds[force - 1] - 0.05;

        /// <summary>The force (0-12) of a wind speed, m/s.</summary>
        public static int Force(double metresPerSecond)
        {
            int f = 0;
            while (f < 12 && metresPerSecond >= Edge(f + 1))
                f++;
            return f;
        }

        /// <summary>A force in words, as a sailor would put it.</summary>
        public static string Name(int force)
        {
            switch (Math.Max(0, Math.Min(12, force)))
            {
                case 0: return "calm";
                case 1: return "light air";
                case 2: return "light breeze";
                case 3: return "gentle breeze";
                case 4: return "moderate breeze";
                case 5: return "fresh breeze";
                case 6: return "strong breeze";
                case 7: return "near gale";
                case 8: return "gale";
                case 9: return "strong gale";
                case 10: return "storm";
                case 11: return "violent storm";
                default: return "hurricane";
            }
        }
    }
}
