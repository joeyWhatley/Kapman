using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// SIGHT AT SEA - how far a lookout sees another ship (real km): to where her sail top clears the horizon (the Earth's
    /// curve: 3.86 x (sqrt(eye) + sqrt(target)) km with heights in metres, the usual sea-sight rule with refraction), but
    /// no further than the air lets him (the weather's visibility: rain and fog), and at night only as far as the moon
    /// allows (half a kilometre on a moonless night, two and a half under a full moon; twilight between).
    /// Plans/ai_fleets.md, phase 2.
    /// </summary>
    public static class Sight
    {
        /// <summary>Real km at which a target this high (m) clears the horizon for an eye this high (m).</summary>
        public static double HorizonKm(double eyeM, double targetM) => 3.86 * (Math.Sqrt(Math.Max(0.0, eyeM)) + Math.Sqrt(Math.Max(0.0, targetM)));

        /// <summary>The furthest a ship is seen (real km), all told.</summary>
        public static double RangeKm(double eyeM, double targetM, double visibilityKm, double sunElevationDeg, double moonLit)
        {
            double km = Math.Min(HorizonKm(eyeM, targetM), Math.Max(0.05, visibilityKm));
            double night = 0.5 + 2.0 * Math.Max(0.0, Math.Min(1.0, moonLit));
            if (sunElevationDeg <= -6.0)
                return Math.Min(km, night);
            if (sunElevationDeg < 0.0)                                  // twilight: from full sight down to the night's
                return Math.Min(km, night + (km - night) * (sunElevationDeg + 6.0) / 6.0);
            return km;
        }

        /// <summary>The sun's height above the horizon (degrees) at a latitude, day of the year and solar hour.</summary>
        public static double SunElevation(double latDeg, int dayOfYear, double hour)
        {
            double lat = latDeg * Math.PI / 180.0;
            double decl = 23.44 * Math.PI / 180.0 * Math.Sin(2.0 * Math.PI * (284 + dayOfYear) / 365.0);
            double ha = (hour - 12.0) * 15.0 * Math.PI / 180.0;
            return Math.Asin(Math.Sin(lat) * Math.Sin(decl) + Math.Cos(lat) * Math.Cos(decl) * Math.Cos(ha)) * 180.0 / Math.PI;
        }

        /// <summary>The share of the moon that's lit (0 new, 1 full) on a Julian day: the real Moon (SkyMath; the old mean
        /// lunation put full moons up to 14 hours out).</summary>
        public static double MoonLit(double julianDay)
        {
            double jde = Kapman.World.SkyMath.Jde(julianDay);
            Kapman.World.SkyMath.Sun(jde, out double sunLon, out double sunR);
            Kapman.World.SkyMath.Moon(jde, out double moonLon, out double moonLat, out double moonKm);
            Kapman.World.SkyMath.MoonPhase(sunLon, sunR, moonLon, moonLat, moonKm, out double lit, out _, out _, out _, out _);
            return lit;
        }
    }
}
