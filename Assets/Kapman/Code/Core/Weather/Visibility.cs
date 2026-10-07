using System;

namespace Kapman.Weather
{
    /// <summary>
    /// HOW FAR ONE CAN SEE - the meteorological optical range (MOR) and its link to HDRP's fog (weather document 1.1, 2.3,
    /// 2.5). By Koschmieder, MOR = 3.912 / b_ext for a 2% contrast threshold (https://en.wikipedia.org/wiki/Visibility;
    /// WMO: fog under 1 km, mist 1-2 km, haze 2-5 km). HDRP's fog attenuation distance is "the distance at which the fog has
    /// absorbed and out-scattered 63% of background light", the mean free path 1 / b_ext, so the mean free path is
    /// MOR / 3.912. What dims the view adds as extinction: 1/MOR = 1/MOR_air + 1/MOR_fog + 1/MOR_rain + 1/MOR_snow +
    /// 1/MOR_spray. All ranges in metres unless named km.
    /// </summary>
    public static class Visibility
    {
        /// <summary>Koschmieder's constant: -ln(0.02).</summary>
        public const double Koschmieder = 3.912;
        /// <summary>The farthest range the engine reports (very clean polar air), m.</summary>
        public const double MaxRange = 80000.0;
        /// <summary>The nearest (the thickest fog), m.</summary>
        public const double MinRange = 20.0;

        /// <summary>The extinction coefficient (per metre) of a range.</summary>
        public static double Extinction(double morMetres) => Koschmieder / Math.Max(morMetres, 1e-3);

        /// <summary>The range (m) of an extinction coefficient (per metre).</summary>
        public static double RangeOf(double extinctionPerMetre) => Koschmieder / Math.Max(extinctionPerMetre, 1e-9);

        /// <summary>HDRP's fog mean free path (attenuation distance) for a range, m: MOR / 3.912.</summary>
        public static double MeanFreePath(double morMetres) => morMetres / Koschmieder;

        /// <summary>The range (m) of HDRP's fog mean free path: 3.912 x the path.</summary>
        public static double RangeOfMeanFreePath(double meanFreePath) => meanFreePath * Koschmieder;

        /// <summary>The range in rain (m): about 10 km at 1 mm/h and 3 km at 10 mm/h (weather document 2.3, a standard order
        /// of magnitude, marked a guess there): 10 km x R^-0.523. No rain: no limit.</summary>
        public static double RainRange(double mmPerHour) => mmPerHour <= 0.01 ? double.PositiveInfinity : 10000.0 * Math.Pow(mmPerHour, -0.523);

        /// <summary>The range in falling snow (m): about 1 km in moderate snow (1 mm/h of water), 0.5 km in heavy (a guess,
        /// the document's 0.5-1 km): 1 km x R^-0.7.</summary>
        public static double SnowRange(double mmPerHourWater) => mmPerHourWater <= 0.01 ? double.PositiveInfinity : 1000.0 * Math.Pow(mmPerHourWater, -0.7);

        /// <summary>The range in fog (m) from the weather model's fog (0 none .. 1 thick), as the first build's visibility
        /// read it (1 / (16 fog^2.6) km): fog 0.35 is about 1 km (fog's WMO edge) and 0.65 about 200 m (thick fog).</summary>
        public static double FogRange(double fog) => fog <= 0.0 ? double.PositiveInfinity : 1000.0 / (16.0 * Math.Pow(fog, 2.6));

        /// <summary>The range in blown spray at sea (m): none under 14 m/s; 5 km at force 8 (17.2 m/s), 2 km at force 10
        /// (24.5 m/s) (weather document 2.3): 1/MOR = 0.0816 (U - 14)^0.771 per km.</summary>
        public static double SprayRange(double windMetresPerSecond)
        {
            double over = windMetresPerSecond - 14.0;
            return over <= 0.0 ? double.PositiveInfinity : 1000.0 / (0.0816 * Math.Pow(over, 0.771));
        }

        /// <summary>The combined range (m) of what dims the view, each given as its own range (m; infinity for none): the
        /// extinctions add. Clamped to <see cref="MinRange"/>..<see cref="MaxRange"/>.</summary>
        public static double Combine(double airRange, double fogRange, double rainRange, double snowRange, double sprayRange)
        {
            double b = Inv(airRange) + Inv(fogRange) + Inv(rainRange) + Inv(snowRange) + Inv(sprayRange);
            double mor = b > 0.0 ? 1.0 / b : MaxRange;
            return Math.Max(MinRange, Math.Min(MaxRange, mor));
        }

        static double Inv(double range) => double.IsInfinity(range) || range <= 0.0 ? 0.0 : 1.0 / range;

        /// <summary>A range in words (WMO): thick fog, fog, mist, haze, or clear.</summary>
        public static string Describe(double morMetres) =>
            morMetres < 200.0 ? "thick fog" : morMetres < 1000.0 ? "fog" : morMetres < 2000.0 ? "mist" : morMetres < 5000.0 ? "haze" : "clear";
    }
}
