using System;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>Where the Sun and the Moon stand for the stage, and how much light they give.</summary>
    public struct StageSkyNow
    {
        /// <summary>Unit vectors towards them in Unity's axes (x east, y up, z north), the air's lift included.</summary>
        public Vector3 toSun, toMoon;
        /// <summary>True altitudes, degrees.</summary>
        public float sunAltitude, moonAltitude;
        /// <summary>The share of the Moon's disc that is lit, its light on the ground (lux), and the share of the Sun it hides.</summary>
        public float moonLit, moonLux, sunCovered;
    }

    /// <summary>
    /// THE SUN AND THE MOON FOR THE STAGE'S OWN PLACE, worked out from SkyMath exactly as Kapman.World.Sky.Compute does,
    /// but into a value: Sky's statics are the level clock's (LevelClock.Apply fills them for LevelInfo's place, or for
    /// 52 N 5 E in the chart scene, which has no LevelInfo) and other code reads them in the same frame, so the stage must
    /// not write them. The chart scene cannot be given a LevelInfo to move the clock's sky to her place: LevelInfo.Current
    /// finds any LevelInfo in the loaded scenes, and MapWorld, SimStage, SaveGame and the encounter variables then take
    /// the chart for a 3D level.
    /// </summary>
    public static class StageSky
    {
        /// <summary>The sky at the clock's Julian day (local mean time) for a latitude and longitude (degrees).</summary>
        public static StageSkyNow At(double clockJulianDay, double latitude, double longitude)
        {
            double jdUt = Sky.ToUt(clockJulianDay, longitude);
            double jde = SkyMath.Jde(jdUt);
            double T = (jde - 2451545.0) / 36525.0, eps = SkyMath.Obliquity(T);
            double lst = SkyMath.Gmst(jdUt) + longitude;

            SkyMath.Sun(jde, out double sunLon, out double sunR);
            SkyMath.EclipticToEquatorial(sunLon, 0.0, eps, out double sunRa, out double sunDec);
            SkyMath.Moon(jde, out double moonLon, out double moonLat, out double moonKm);
            SkyMath.EclipticToEquatorial(moonLon, moonLat, eps, out double moonRa, out double moonDec);
            SkyMath.Topocentric(moonRa, moonDec, moonKm, lst, latitude, out double mRa, out double mDec, out double mKm);
            SkyMath.MoonPhase(sunLon, sunR, moonLon, moonLat, moonKm, out double lit, out _, out _, out _, out double lux);

            var now = new StageSkyNow { moonLit = (float)lit, moonLux = (float)lux };
            now.toSun = Place(sunRa, sunDec, lst, latitude, out now.sunAltitude);
            now.toMoon = Place(mRa, mDec, lst, latitude, out now.moonAltitude);
            now.sunCovered = (float)SkyMath.SunCovered(sunRa, sunDec, sunR * SkyMath.AuKm, mRa, mDec, mKm);
            return now;
        }

        static Vector3 Place(double ra, double dec, double lst, double latitude, out float altitude)
        {
            SkyMath.ToHorizon(ra, dec, lst, latitude, out double e, out double u, out double n);
            double alt = Math.Asin(Math.Max(-1.0, Math.Min(1.0, u))) / SkyMath.D2R;
            // lifted by the air: the same bearing, a little higher (as Sky.Place)
            double seen = (alt + SkyMath.Refraction(alt)) * SkyMath.D2R, flat = Math.Sqrt(e * e + n * n);
            if (flat > 1e-9)
            {
                double k = Math.Cos(seen) / flat;
                e *= k;
                n *= k;
                u = Math.Sin(seen);
            }
            altitude = (float)alt;
            return new Vector3((float)e, (float)u, (float)n).normalized;
        }
    }
}
