using System;

namespace Kapman.World
{
    /// <summary>A body on the sky as seen from a place: its direction (east, up, north; the air's lift included), true
    /// altitude (degrees, before the lift) and magnitude.</summary>
    public struct SkyBody
    {
        public double east, up, north;
        public double altitude;
        public double magnitude;
        public bool Up => altitude > -1.0;
    }

    /// <summary>
    /// THE SKY NOW - where the Sun, the Moon and the planets are for the game's moment and the place the camera is, worked
    /// out each frame by the clock (GameClock) from SkyMath, for the light, the stars, the Moon and the tides to share.
    /// One rule for time: the game's hours are LOCAL MEAN TIME (noon when the Sun is south, as the period's sundials
    /// reckoned), so UT = the clock's Julian Day - longitude / 360.
    /// </summary>
    public static class Sky
    {
        public static double JdUt, Jde, Latitude, Longitude;
        /// <summary>Local sidereal time, degrees.</summary>
        public static double Lst;
        public static SkyBody Sun, Moon;
        /// <summary>The Moon: share of its disc lit (0..1), phase angle (degrees, 0 full), waxing, light on the ground (lux).</summary>
        public static double MoonLit, MoonPhaseAngle, MoonLux;
        public static bool MoonWaxing;
        /// <summary>Mercury, Venus, Mars, Jupiter, Saturn.</summary>
        public static readonly SkyBody[] Planets = new SkyBody[5];
        public static readonly Planet[] PlanetOrder = { Planet.Mercury, Planet.Venus, Planet.Mars, Planet.Jupiter, Planet.Saturn };
        /// <summary>The share of the Sun's disc the Moon hides here now (an eclipse; 0 almost always).</summary>
        public static double SunCovered;
        /// <summary>J2000 equator to the equator of the date (row-major 3x3), renewed once a game day.</summary>
        public static double[] Precession = { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        public static bool Known;

        static double precessionDay = double.NaN;

        /// <summary>UT Julian Day for the clock's Julian Day (local mean time) at a longitude (degrees east).</summary>
        public static double ToUt(double clockJulianDay, double longitude) => clockJulianDay - longitude / 360.0;

        /// <summary>Works out the sky for a moment (UT) and a place.</summary>
        public static void Compute(double jdUt, double latitude, double longitude)
        {
            JdUt = jdUt;
            Latitude = latitude;
            Longitude = longitude;
            Jde = SkyMath.Jde(jdUt);
            double T = (Jde - 2451545.0) / 36525.0, eps = SkyMath.Obliquity(T);
            Lst = SkyMath.Gmst(jdUt) + longitude;
            if (double.IsNaN(precessionDay) || Math.Abs(jdUt - precessionDay) > 1.0)
            {
                Precession = SkyMath.Precession(Jde);
                precessionDay = jdUt;
            }

            SkyMath.Sun(Jde, out double sunLon, out double sunR);
            SkyMath.EclipticToEquatorial(sunLon, 0.0, eps, out double sunRa, out double sunDec);
            Sun = Place(sunRa, sunDec, -26.74);

            SkyMath.Moon(Jde, out double moonLon, out double moonLat, out double moonKm);
            SkyMath.EclipticToEquatorial(moonLon, moonLat, eps, out double moonRa, out double moonDec);
            SkyMath.Topocentric(moonRa, moonDec, moonKm, Lst, latitude, out double mRa, out double mDec, out double mKm);
            SkyMath.MoonPhase(sunLon, sunR, moonLon, moonLat, moonKm, out MoonLit, out MoonPhaseAngle, out MoonWaxing, out double moonMag, out MoonLux);
            Moon = Place(mRa, mDec, moonMag);

            SunCovered = SkyMath.SunCovered(sunRa, sunDec, sunR * SkyMath.AuKm, mRa, mDec, mKm);

            for (int i = 0; i < PlanetOrder.Length; i++)
            {
                SkyMath.PlanetPosition(PlanetOrder[i], Jde, out double l, out double b, out _, out _, out double mag);
                SkyMath.EclipticToEquatorial(l, b, eps, out double ra, out double dec);
                Planets[i] = Place(ra, dec, mag);
            }
            Known = true;
        }

        static SkyBody Place(double ra, double dec, double magnitude)
        {
            SkyMath.ToHorizon(ra, dec, Lst, Latitude, out double e, out double u, out double n);
            double alt = Math.Asin(Math.Max(-1.0, Math.Min(1.0, u))) / SkyMath.D2R;
            // lifted by the air: the same bearing, a little higher
            double seen = (alt + SkyMath.Refraction(alt)) * SkyMath.D2R, flat = Math.Sqrt(e * e + n * n);
            if (flat > 1e-9)
            {
                double k = Math.Cos(seen) / flat;
                e *= k;
                n *= k;
                u = Math.Sin(seen);
            }
            return new SkyBody { east = e, up = u, north = n, altitude = alt, magnitude = magnitude };
        }
    }
}
