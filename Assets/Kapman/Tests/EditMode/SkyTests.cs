using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The sky maths (Code/World/SkyMath.cs) against JPL Horizons (DE441) at four moments of the game's centuries, the
    /// equinox of 793, and the eclipses of 796 and 807 (NASA's tables, Espenak & Meeus). Reference values fetched for
    /// Plans/environment_research/sky_maths.md: apparent ecliptic longitudes and latitudes of the date, degrees.
    /// </summary>
    public class SkyTests
    {
        static readonly double[] Jd = { 2010859.5, 2013272.3333333, 2068104.5, 2110701.0 };          // UT: 793, 800, 950, 1066
        static readonly double[] HorizonsDeltaT = { 3008.104940, 2956.827427, 1931.100789, 1357.779951 };
        static readonly double[] SunLon = { 80.1060091, 299.4169720, 345.3382823, 206.8268505 };
        static readonly double[] MoonLon = { 17.6181575, 119.0272533, 87.5196806, 119.2868612 };
        static readonly double[] MoonLat = { -3.7034841, -0.4908055, 0.2531368, -4.3416177 };
        static readonly double[,] Planets =                 // Mercury, Venus, Mars, Jupiter, Saturn: longitude, latitude
        {
            { 69.3114027, -0.0666200, 83.8387871, 0.5553466, 131.2414126, 1.2764644, 118.2350880, 0.7183985, 47.8409731, -1.9792567 },
            { 274.7565347, 0.2643212, 322.4441592, -1.5283988, 333.3269021, -0.8620581, 315.1863485, -0.8833546, 135.6940304, 1.4463528 },
            { 329.4367708, -2.2313309, 353.5607719, -1.2808752, 305.9614701, -1.0910478, 204.2495623, 1.5243534, 172.2977791, 2.5930249 },
            { 212.8270779, -1.3208285, 209.5488314, 0.8216154, 252.6521111, -1.1960705, 152.1367786, 0.9835818, 163.0568043, 1.8852616 },
        };

        static double Diff(double a, double b)
        {
            double d = (a - b) % 360.0;
            if (d > 180.0) d -= 360.0;
            if (d < -180.0) d += 360.0;
            return System.Math.Abs(d);
        }

        [Test]
        public void TheClockAndTheMathsCountDaysAlike()
        {
            // GameClock.JulianDay: 1721423.5 + 365 (year - 1) + floor((year - 1) / 4) + (day of year - 1); 21 June 793 is day 172
            double clock = 1721423.5 + 365.0 * 792 + System.Math.Floor(792 / 4.0) + 171;
            Assert.AreEqual(clock, SkyMath.JulianDay(793, 6, 21), 1e-9);
            Assert.AreEqual(2010859.5, SkyMath.JulianDay(793, 6, 8), 1e-9, "Horizons: JD 2010859.5 = 793-06-08");
        }

        [Test]
        public void DeltaTIsNearHorizons()
        {
            for (int i = 0; i < Jd.Length; i++)
                Assert.AreEqual(HorizonsDeltaT[i], SkyMath.DeltaT(SkyMath.YearOf(Jd[i])), 150.0, "Delta T, s, at " + SkyMath.YearOf(Jd[i]).ToString("0"));
        }

        [Test]
        public void TheSunMoonAndPlanetsAreWhereHorizonsHasThem()
        {
            for (int i = 0; i < Jd.Length; i++)
            {
                double jde = Jd[i] + HorizonsDeltaT[i] / 86400.0;
                SkyMath.Sun(jde, out double sun, out _);
                Assert.Less(Diff(sun, SunLon[i]), 0.01, "Sun at " + i);
                SkyMath.Moon(jde, out double ml, out double mb, out _);
                Assert.Less(Diff(ml, MoonLon[i]), 0.01, "Moon longitude at " + i);
                Assert.Less(System.Math.Abs(mb - MoonLat[i]), 0.01, "Moon latitude at " + i);
                var order = new[] { Planet.Mercury, Planet.Venus, Planet.Mars, Planet.Jupiter, Planet.Saturn };
                for (int p = 0; p < order.Length; p++)
                {
                    SkyMath.PlanetPosition(order[p], jde, out double pl, out double pb, out _, out _, out _);
                    Assert.Less(Diff(pl, Planets[i, 2 * p]), 0.2, order[p] + " longitude at " + i);
                    Assert.Less(System.Math.Abs(pb - Planets[i, 2 * p + 1]), 0.2, order[p] + " latitude at " + i);
                }
            }
        }

        [Test]
        public void SpringCameOnTheSixteenthOfMarch793()
        {
            // the equinox: 16 March 793, 19:33 UT on the Julian calendar (not 21-22 March: the calendar had drifted)
            double jd = SkyMath.JulianDay(793, 3, 16, 19.55);
            SkyMath.Sun(SkyMath.Jde(jd), out double sun, out _);
            Assert.Less(Diff(sun, 0.0), 0.05);
        }

        [Test]
        public void TheEclipsesComeOnTheirHours()
        {
            // the annular eclipse of 11 February 807: most of the Sun hidden at York at 10:53 UT, none three hours before
            double lat = 53.96, lon = -1.08;
            Sky.Compute(SkyMath.JulianDay(807, 2, 11, 10.0 + 53.0 / 60.0), lat, lon);
            Assert.Greater(Sky.SunCovered, 0.5, "covered at York at 10:53");
            Assert.Greater(Sky.Sun.altitude, 0.0, "the Sun is up");
            Sky.Compute(SkyMath.JulianDay(807, 2, 11, 7.9), lat, lon);
            Assert.AreEqual(0.0, Sky.SunCovered, 1e-9, "nothing at 07:54");
            // the total lunar eclipse of 28 March 796, 05:58 UT: the Moon opposite the Sun and on the ecliptic
            double jde = SkyMath.Jde(SkyMath.JulianDay(796, 3, 28, 5.0 + 58.0 / 60.0));
            SkyMath.Sun(jde, out double sun, out _);
            SkyMath.Moon(jde, out double moon, out double moonLat, out _);
            Assert.Less(Diff(moon, sun + 180.0), 0.3, "opposite");
            Assert.Less(System.Math.Abs(moonLat), 0.5, "on the ecliptic");
        }

        [Test]
        public void PrecessionMovesTheStarsAsItShould()
        {
            // the J2000 equinox point, seen in 800: about 16.6 degrees along the equator (the vernal point has moved)
            double[] P = SkyMath.Precession(SkyMath.Jde(SkyMath.JulianDay(800, 1, 1)));
            double ra = System.Math.Atan2(P[3], P[0]) / SkyMath.D2R;
            Assert.That(ra < 0 ? ra + 360.0 : ra, Is.InRange(343.0, 346.0), "J2000's equinox at 800 AD (RA of date)");
            // Polaris was about 7.4 degrees from the pole in 793
            SkyMath.ProperMotion(37.95456067, 89.26410897, 0.04448, -0.01185, 793 - 2000, out double x, out double y, out double z);
            P = SkyMath.Precession(SkyMath.Jde(SkyMath.JulianDay(793, 6, 21)));
            double zd = P[6] * x + P[7] * y + P[8] * z;
            Assert.AreEqual(7.4, 90.0 - System.Math.Asin(zd) / SkyMath.D2R, 0.2, "Polaris from the pole");
        }

        [Test]
        public void MidsummerNightAtSixtyNorthIsTwilight()
        {
            // 18 June 793 (the solstice), midnight local mean time at 60 N 5 E: the Sun about 6.4 degrees down
            double jd = Sky.ToUt(SkyMath.JulianDay(793, 6, 18, 24.0), 5.0);
            Sky.Compute(jd, 60.0, 5.0);
            Assert.AreEqual(-6.4, Sky.Sun.altitude, 0.4);
        }
    }
}
