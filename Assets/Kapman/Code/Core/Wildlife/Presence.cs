using System;
using System.Collections.Generic;

namespace Kapman.Nature
{
    /// <summary>The bands of the day the animals keep, by the Sun's altitude: dawn -6 to +10 degrees and rising, dusk the
    /// same falling, night below -6, day the rest (Design/wildlife_and_seasons.md 2.4).</summary>
    public enum HourBand { Dawn, Day, Dusk, Night }

    /// <summary>
    /// PRESENCE - how much of a species is here on a day, by the clock of the year (Design/wildlife_and_seasons.md 2.2-2.4):
    /// the species' curve, written on Gregorian days, read at the game's Julian day plus the century's lag, slid by the
    /// year's own die (an early or a late spring) and by the warm index (decision 17); then the hour's band and the
    /// weather on top for what is seen at this moment. Pure functions; the year state (hard winters, trends) is in
    /// YearState and the whole sum in WildlifeModel.
    /// </summary>
    public static class Presence
    {
        /// <summary>The band edges (degrees of the Sun's altitude).</summary>
        public const float NightBelow = -6f, TwilightAbove = 10f;

        /// <summary>
        /// The days the Julian calendar runs behind the Gregorian on a Julian date: 4 to the end of February 900, 5 to the
        /// end of February 1000, 6 after (the lag grows on the Julian leap day of each century year the Gregorian skips).
        /// </summary>
        public static int CenturyOffset(int year, int dayOfYear)
        {
            int y = year % 100 == 0 && dayOfYear < 61 ? year - 1 : year;   // (before 1 March of 900 or 1000: the old lag)
            int c = y / 100;
            return c - c / 4 - 2;
        }

        /// <summary>A window's strength on a day: rising over its first third, 1 across the middle third, falling over the
        /// last third, 0 outside; a window may run over the new year (from > to). Days of a 365-day year.</summary>
        public static double Shape(double from, double to, double day)
        {
            double len = Wrap(to - from);
            if (len <= 0.0)
                return 0.0;
            double t = Wrap(day - from);
            if (t > len)
                return 0.0;
            double third = len / 3.0;
            if (t < third)
                return t / third;
            if (t > len - third)
                return (len - t) / third;
            return 1.0;
        }

        /// <summary>A piecewise-linear curve at x, held flat beyond its ends (the trend over years).</summary>
        public static double Linear(List<(double x, double y)> curve, double x)
        {
            if (curve == null || curve.Count == 0)
                return 1.0;
            if (x <= curve[0].x)
                return curve[0].y;
            for (int i = 1; i < curve.Count; i++)
                if (x <= curve[i].x)
                {
                    var a = curve[i - 1];
                    var b = curve[i];
                    return b.x > a.x ? a.y + (b.y - a.y) * (x - a.x) / (b.x - a.x) : b.y;
                }
            return curve[curve.Count - 1].y;
        }

        /// <summary>A species' presence curve on a Gregorian day (any number: folded into the year), the two ends joined
        /// across the new year.</summary>
        public static double Curve(Species s, double gregorianDay)
        {
            var c = s.presence;
            if (c == null || c.Count == 0)
                return 0.0;
            if (c.Count == 1)
                return c[0].y;
            double d = Wrap(gregorianDay - 1.0) + 1.0;          // 1 .. 366
            if (d < c[0].x || d > c[c.Count - 1].x)
            {
                // across the new year, from the last point to the first one a year on
                var a = c[c.Count - 1];
                var b = (x: c[0].x + 365.0, c[0].y);
                double x = d < c[0].x ? d + 365.0 : d;
                return b.x > a.x ? a.y + (b.y - a.y) * (x - a.x) / (b.x - a.x) : a.y;
            }
            return Linear(c, d);
        }

        /// <summary>The year's slide of a species' whole curve, in days (negative = earlier): its die times jitterDays,
        /// plus warmShiftDays times the warm index.</summary>
        public static double Shift(Species s, int seed, int year, double warmIndex) =>
            s.jitterDays * (2.0 * Dice.Roll(seed, year, s.id) - 1.0) + s.warmShiftDays * warmIndex;

        /// <summary>The species' season on a Julian day of a year (0..1): the curve read at the Gregorian day, less the
        /// year's slide (a curve slid earlier is read later).</summary>
        public static double At(Species s, int seed, int dayOfYear, int year, double warmIndex) =>
            Curve(s, dayOfYear + CenturyOffset(year, dayOfYear) - Shift(s, seed, year, warmIndex));

        /// <summary>The band of the day from the Sun's altitude (degrees) and whether it is climbing.</summary>
        public static HourBand Band(float sunAltitude, bool rising)
        {
            if (sunAltitude < NightBelow)
                return HourBand.Night;
            if (sunAltitude < TwilightAbove)
                return rising ? HourBand.Dawn : HourBand.Dusk;
            return HourBand.Day;
        }

        /// <summary>The hours multiplier of a species in a band.</summary>
        public static double Hours(Species s, HourBand band) => s.hours.For(band);

        /// <summary>
        /// The weather multiplier (2.2): the wind as a ramp, not a step, so a flock does not flicker as the wind eases
        /// through its limit, times the rain, fog (full at a density of 0.004), storm and frost multipliers at their
        /// strengths. Every input is clamped.
        /// </summary>
        public static double Weather(Species s, double wind, double rain, double fogDensity, double frost, double storm)
        {
            var w = s.weather;
            double windOff = 1.0 - 0.85 * SmoothStep(w.windMax - 3.0, w.windMax + 1.0, wind);
            return windOff
                   * Lerp(1.0, w.rain, Clamp01(rain))
                   * Lerp(1.0, w.fog, Clamp01(fogDensity / 0.004))
                   * Lerp(1.0, w.storm, Clamp01(storm))
                   * Lerp(1.0, w.frost, Clamp01(frost));
        }

        /// <summary>
        /// The Sun's altitude (degrees) at a latitude, a day of the year and an hour of local mean time, near enough for
        /// the band of an hour the clock jumped over (Cooper's declination and the hour angle; the equation of time and
        /// refraction left out: a quarter of an hour at most). The live hour uses LevelClock.SunAltitude.
        /// </summary>
        public static double ApproxSunAltitude(double latitude, double dayOfYear, double hour)
        {
            double decl = 23.44 * Math.Sin(2.0 * Math.PI * (284.0 + dayOfYear) / 365.0);
            double ha = (hour - 12.0) * 15.0;
            double r = Math.PI / 180.0;
            double s = Math.Sin(latitude * r) * Math.Sin(decl * r) + Math.Cos(latitude * r) * Math.Cos(decl * r) * Math.Cos(ha * r);
            return Math.Asin(Math.Max(-1.0, Math.Min(1.0, s))) / r;
        }

        /// <summary>The band of an hour by the approximate Sun (rising before local noon).</summary>
        public static HourBand ApproxBand(double latitude, int dayOfYear, double hour) =>
            Band((float)ApproxSunAltitude(latitude, dayOfYear, hour), hour < 12.0);

        /// <summary>How dark it is from the Sun's altitude: 0 by day, 1 at full night (12 degrees under), as LevelClock.Night.</summary>
        public static double NightFrom(double sunAltitude) => Clamp01(sunAltitude / -12.0);

        static double Wrap(double days)
        {
            double d = days % 365.0;
            return d < 0.0 ? d + 365.0 : d;
        }

        public static double Clamp01(double v) => v < 0.0 ? 0.0 : v > 1.0 ? 1.0 : v;
        static double Lerp(double a, double b, double t) => a + (b - a) * t;

        static double SmoothStep(double a, double b, double x)
        {
            double t = Clamp01((x - a) / (b - a));
            return t * t * (3.0 - 2.0 * t);
        }
    }
}
