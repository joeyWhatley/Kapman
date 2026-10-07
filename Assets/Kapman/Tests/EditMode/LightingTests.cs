using System;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The light and the eye (Code/Core/Light): the lux on the ground at 52 N by the Sun's height, cloud and the Moon,
    /// the exposure values they give, and the eye adapting to the dark and back (Design/lighting_and_atmosphere.md 4.10,
    /// rows 1-7). The bounds are the design's, from its cited anchors: Wikipedia's Daylight, Lux, Moonlight and
    /// Adaptation (eye) pages and HDRP's Physical Light Units table.
    /// </summary>
    public class LightingTests
    {
        const double DorestadLat = 51.98, DorestadLon = 5.33;     // LevelInfo's defaults (the design rounds to 52.0 N)
        const double JuneNoon = LightingModel.JuneNoonAltitude;   // 61.4 degrees
        const double DecemberNoon = 14.6;                         // 21 December at 52 N (lighting 1.1)

        /// <summary>The Sun's true altitude at a place at a local mean time (Julian calendar), from SkyMath as Sky.Compute
        /// does it, without touching the shared Sky.</summary>
        static double SunAltitude(int year, int month, int day, double localHour, double lat, double lon)
        {
            double ut = Sky.ToUt(SkyMath.JulianDay(year, month, day, localHour), lon);
            double jde = SkyMath.Jde(ut), T = (jde - 2451545.0) / 36525.0;
            SkyMath.Sun(jde, out double lambda, out _);
            SkyMath.EclipticToEquatorial(lambda, 0.0, SkyMath.Obliquity(T), out double ra, out double dec);
            SkyMath.ToHorizon(ra, dec, SkyMath.Gmst(ut) + lon, lat, out _, out double up, out _);
            return Math.Asin(Math.Max(-1.0, Math.Min(1.0, up))) / SkyMath.D2R;
        }

        static double Daylight(double altitude, double overcast = 0.0, double storm = 0.0) =>
            LightingModel.SunLux(altitude, overcast, storm) + LightingModel.SkyLux(altitude, overcast, storm);

        [Test]
        public void SunAtJuneNoon52N()
        {
            double alt = SunAltitude(793, 6, 21, 12.0, DorestadLat, DorestadLon);
            Assert.AreEqual(61.4, alt, 0.3, "the Sun at Dorestad's local noon, 21 June 793");
            double lux = Daylight(alt);
            Assert.That(lux, Is.InRange(90000.0, 110000.0), "a clear June noon, lux on the flat");
            // most of it straight from the Sun, an eighth from the blue sky (the table in 1.2: 88,000 + 12,500)
            Assert.That(LightingModel.SunLux(alt, 0, 0), Is.InRange(80000.0, 95000.0));
            Assert.That(LightingModel.SkyLux(alt, 0, 0), Is.InRange(11000.0, 14000.0));
            // a December noon is about a fifth of it
            double december = SunAltitude(793, 12, 21, 12.0, DorestadLat, DorestadLon);
            Assert.AreEqual(DecemberNoon, december, 0.5, "the Sun at Dorestad's local noon, 21 December 793");
            Assert.That(Daylight(december) / lux, Is.InRange(0.15, 0.25));
        }

        [Test]
        public void Sunset()
        {
            Assert.That(Daylight(0.0), Is.InRange(300.0, 500.0), "sunset, clear");
            Assert.That(Daylight(-6.0), Is.InRange(3.0, 3.8), "the end of civil twilight");
            Assert.That(Daylight(-12.0), Is.InRange(0.02, 0.04), "the end of nautical twilight");
            Assert.That(Daylight(-18.0), Is.InRange(1e-4, 3e-4), "the end of astronomical twilight");
            Assert.AreEqual(0.0, Daylight(-18.01));
            Assert.AreEqual(0.0, Daylight(-40.0));
            // no gaps or NaN anywhere on the sky, and the light never falls as the Sun climbs
            double last = -1.0;
            for (double alt = -90.0; alt <= 90.0; alt += 0.125)
            {
                double lux = Daylight(alt), total = LightingModel.TotalLux(alt, 0.0, 0.0, 0.31, alt);
                foreach (double v in new[] { lux, total, LightingModel.TwilightLux(alt), LightingModel.SunNormalLux(alt),
                                             Daylight(alt, 1.0, 1.0), LightingModel.EV100FromLux(lux) })
                    Assert.IsFalse(double.IsNaN(v) || double.IsInfinity(v), "a number at " + alt);
                Assert.GreaterOrEqual(lux, last - 1e-9, "the light falls as the Sun climbs at " + alt);
                last = lux;
            }
        }

        [Test]
        public void Overcast()
        {
            Assert.AreEqual(0.0, LightingModel.SunLux(JuneNoon, 1.0, 0.0), 1e-9, "no direct Sun under a full deck");
            Assert.That(Daylight(JuneNoon, 1.0), Is.InRange(1200.0, 1800.0), "a June noon, full overcast");
            Assert.That(Daylight(JuneNoon, 1.0, 1.0), Is.InRange(100.0, 200.0), "a June noon under storm clouds");
            Assert.That(Daylight(DecemberNoon, 1.0), Is.InRange(300.0, 600.0), "a December noon, full overcast");
            Assert.That(Daylight(0.0, 1.0), Is.InRange(30.0, 50.0), "an overcast sunset");
            Assert.AreEqual(0.0, LightingModel.CloudDim(LightingModel.CloudTransmittance(1.0, 0.0)) * LightingModel.DirectLux(JuneNoon), 1e-9);
            Assert.AreEqual(1.0, LightingModel.CloudDim(LightingModel.CloudTransmittance(0.0, 0.0)), 1e-12);
            // the transmittance is the one cloud input: the weather engine's sample goes straight in
            Assert.AreEqual(LightingModel.DirectLux(JuneNoon) * 0.4, LightingModel.SunLuxThrough(JuneNoon, 0.4), 1e-6);
            Assert.AreEqual(1.0, LightingModel.CloudDim(double.NaN), "an unsampled weather leaves the Sun alone");
            // with HDRP's clouds already taking half, the clock dims its Sun by the rest only
            Assert.AreEqual(0.5, LightingModel.CloudDim(0.25, 0.5), 1e-12);
            Assert.AreEqual(1.0, LightingModel.CloudDim(0.8, 0.5), 1e-12, "never brighter than the Sun itself");
            // a 200 m fog with a 200 m free path passes about a third of the noon Sun, less of a low one
            double fogNoon = LightingModel.FogTransmittance(JuneNoon, 200.0, 200.0);
            Assert.That(fogNoon, Is.InRange(0.3, 0.4));
            Assert.Less(LightingModel.FogTransmittance(10.0, 200.0, 200.0), fogNoon);
            Assert.AreEqual(1.0, LightingModel.FogTransmittance(10.0, double.PositiveInfinity, 200.0));
        }

        [Test]
        public void Moon()
        {
            // SkyMath's full and quarter Moons (magnitude -12.7 and -10.1) are the inputs
            Assert.AreEqual(0.31, SkyMath.Lux(-12.717), 0.01, "the full Moon by SkyMath's magnitude");
            Assert.That(LightingModel.MoonLux(0.31, 45.0, 0.0), Is.InRange(0.18, 0.24), "a full Moon at 45 degrees");
            Assert.That(LightingModel.MoonLux(0.028, 45.0, 0.0), Is.InRange(0.015, 0.025), "a quarter Moon at 45 degrees");
            Assert.AreEqual(0.3, LightingModel.MoonLux(0.31, 45.0, 1.0) / LightingModel.MoonLux(0.31, 45.0, 0.0), 1e-9, "a full overcast leaves 30%");
            Assert.AreEqual(0.0, LightingModel.MoonLux(0.31, -0.5, 0.0), "under the horizon");
            Assert.AreEqual(0.0, LightingModel.MoonLux(0.31, -30.0, 0.0));
            Assert.AreEqual(0.002, LightingModel.StarLux(0.0), 1e-12);
            Assert.AreEqual(0.0001, LightingModel.StarLux(1.0), 1e-12);
        }

        [Test]
        public void EV100()
        {
            double Ev(double lux) => LightingModel.EV100FromLux(lux);
            Assert.That(Ev(LightingModel.TotalLux(JuneNoon, 0, 0, 0, -30)), Is.InRange(15.0, 16.0), "noon, clear");
            Assert.That(Ev(LightingModel.TotalLux(JuneNoon, 1, 0, 0, -30)), Is.InRange(9.0, 10.0), "noon, overcast");
            Assert.That(Ev(LightingModel.TotalLux(JuneNoon, 1, 1, 0, -30)), Is.InRange(6.0, 7.0), "noon, storm");
            Assert.That(Ev(0.31), Is.InRange(-3.5, -2.5), "a true full Moon");
            Assert.That(Ev(LightingModel.TotalLux(-30, 0, 0, 0, -30)), Is.InRange(-11.0, -9.0), "starlight");
            Assert.That(Ev(LightingModel.TotalLux(-6, 0, 0, 0, -30)), Is.InRange(0.0, 1.2), "the end of civil twilight");
            // HDRP's multiplier: 1 / (1.2 2^EV), and one stop halves it
            Assert.AreEqual(1.0 / 1.2, LightingModel.ExposureFromEV100(0.0), 1e-12);
            Assert.AreEqual(0.5, LightingModel.ExposureFromEV100(5.0) / LightingModel.ExposureFromEV100(4.0), 1e-12);
            // no light at all is still a number
            Assert.IsFalse(double.IsInfinity(Ev(0.0)) || double.IsNaN(Ev(0.0)));
        }

        [Test]
        public void DarkAdaptation()
        {
            var eye = new EyeModel();
            eye.Settle(10.0);
            Assert.AreEqual(0.0, eye.Adaptation, "a day's eye");
            Assert.AreEqual(2.0, eye.LimitMin, 1e-9, "the day's exposure floor");
            // into a starlit night (EV -8) in frames of a quarter game minute
            const double frame = 0.25;
            double at19 = 0, at21 = 0, at60 = 0;
            for (int i = 1; i <= 240; i++)
            {
                eye.Step(frame, -8.0);
                double minutes = i * frame;
                if (Math.Abs(minutes - 19.0) < 1e-9) at19 = eye.Adaptation;
                if (Math.Abs(minutes - 21.0) < 1e-9) at21 = eye.Adaptation;
                if (Math.Abs(minutes - 60.0) < 1e-9) at60 = eye.Adaptation;
            }
            Assert.Less(at19, 0.63, "not yet 63% at 19 game minutes");
            Assert.Greater(at21, 0.63, "63% by 21 game minutes (one 20-minute time constant)");
            Assert.GreaterOrEqual(at60, 0.95, "95% by an hour");
            Assert.Less(eye.LimitMin, -5.5, "the floor most of the way down");
            Assert.Less(eye.ColourDrain, -40.0, "the colour drained");
            // one long step is the same as many short ones
            var jumped = new EyeModel();
            jumped.Settle(10.0);
            jumped.Step(60.0, -8.0);
            Assert.AreEqual(at60, jumped.Adaptation, 1e-9);
            // paused, it holds; the clock set back settles it on the scene
            double held = eye.Adaptation;
            eye.Step(0.0, 10.0);
            Assert.AreEqual(held, eye.Adaptation);
            // back to day in under 30 game seconds, in frames of a game second
            for (int i = 0; i < 30; i++)
                eye.Step(1.0 / 60.0, 10.0);
            Assert.Less(eye.Adaptation, 0.05, "back to the day's eye within 30 game seconds");
            eye.Step(-1.0, -8.0);
            Assert.AreEqual(1.0, eye.Adaptation, "time going back settles it");
        }

        [Test]
        public void LanternResets()
        {
            var eye = new EyeModel();
            eye.Settle(-10.0);
            Assert.AreEqual(1.0, eye.Adaptation, "fully dark-adapted");
            Assert.AreEqual(-6.0, eye.LimitMin, 1e-9);
            Assert.AreEqual(1.0, eye.NightSightScale, 1e-9);
            // a fat lamp lit on deck: EV 2 in frame, in frames of a game second
            for (int i = 0; i < 15; i++)
                eye.Step(1.0 / 60.0, 2.0);
            Assert.Less(eye.Adaptation, 0.2, "a lantern undoes the dark adaptation within 15 game seconds");
            Assert.Less(eye.NightSightScale, 0.4, "the lookout's night sight goes with it");
            Assert.AreEqual(0.0, eye.ColourDrain, 1e-6, "the colour back");
            // covered again, the dark comes back only slowly: about two thirds after 20 game minutes
            double lit = eye.Adaptation;
            eye.Step(20.0, -10.0);
            Assert.AreEqual(1.0 - (1.0 - lit) * Math.Exp(-1.0), eye.Adaptation, 1e-9);
            Assert.Less(eye.Adaptation, 0.8);
        }
    }
}
