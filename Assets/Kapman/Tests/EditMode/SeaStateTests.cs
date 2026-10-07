using System;
using System.IO;
using Kapman.Weather;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The sea state (Design/weather_and_sea_state.md 2.4, 2.5, 4.3 test 13): the wind sea grown and decayed by the laws as
    /// the wind changes, the level's own fetch, the swell from storms far off, the warm start that has a level loaded
    /// mid-gale open on a sea already up, and the fit of the sea to HDRP's two swell bands.
    /// </summary>
    public class SeaStateTests
    {
        const string WaterPath = "Assets/Kapman/Data/Water/water_793.json";
        // the southern North Sea's open-water level (O1) and its record
        const double SeaLat = 52.6, SeaLon = 3.0;
        const string SeaRecord = "o1_southern_north_sea";

        static FetchTable SouthernNorthSea()
        {
            var f = FetchTable.FromWaterRecord(File.ReadAllText(WaterPath), SeaRecord);
            Assert.IsNotNull(f, "the southern North Sea's fetch is in water_793.json");
            return f;
        }

        static SeaStateModel Steady(FetchTable fetch, double wind, double from, double hours, double stepHours = 1.0 / 60.0)
        {
            var m = new SeaStateModel(null, SeaLat, SeaLon, "north_sea", fetch) { Swell = false };
            double t = 0.0;
            m.Step(t, 0.0, wind, from, 1013.0);
            while (t < hours - 1e-9)
            {
                t += stepHours;
                m.Step(t, stepHours, wind, from, 1013.0);
            }
            return m;
        }

        [Test]
        public void TheWindSeaGrowsByTheLaws()
        {
            // a steady wind from a flat sea follows the duration-limited law, however it is stepped (weather 2.4 worked:
            // 20 m/s over 150 km for 6 h gives 3.6 m, duration-limited)
            var open = FetchTable.Open(150.0, 0.0);
            var m = Steady(open, 20.0, 0.0, 6.0);
            WaveGrowth.Grow(20.0, 150000.0, 6.0, 0.0, out double hs, out double tp);
            Assert.AreEqual(hs, m.State.windSeaHs, 0.03 * hs, "stepped a minute at a time");
            Assert.That(m.State.windSeaHs, Is.InRange(3.2f, 4.2f), "the design's worked case");
            var coarse = Steady(open, 20.0, 0.0, 6.0, 0.5);
            Assert.AreEqual(m.State.windSeaHs, coarse.State.windSeaHs, 0.03 * hs, "stepped half an hour at a time");

            // ten metres a second over 600 km for a day reaches the fully developed sea (Pierson-Moskowitz: 2.8 m, 8.3 s)
            var long10 = Steady(FetchTable.Open(600.0, 0.0), 10.0, 0.0, 24.0, 0.25);
            Assert.That(long10.State.windSeaHs, Is.InRange(2.3f, 2.9f));
            Assert.That(long10.State.windSeaTp, Is.InRange(7f, 9f));
            // the Kromme Rijn at Dorestad (about 80 m across) under the same wind: a few centimetres of chop
            var river = Steady(FetchTable.Open(0.08, 3.0), 10.0, 0.0, 2.0, 0.25);
            Assert.Less(river.State.windSeaHs, 0.08f);
            // shallow water holds it down: depth 3 m caps Hs at 1.5 m
            var shallow = Steady(FetchTable.Open(600.0, 3.0), 25.0, 0.0, 24.0, 0.25);
            Assert.LessOrEqual(shallow.State.windSeaHs, 1.5f + 1e-4f);
        }

        [Test]
        public void ItDecaysWhenTheWindDropsAndTurnsWithTheWind()
        {
            var fetch = FetchTable.Open(400.0, 0.0);
            var m = Steady(fetch, 20.0, 270.0, 24.0, 0.25);
            float before = m.State.windSeaHs;
            Assert.Greater(before, 4f, "a gale's sea after a day");
            WaveGrowth.Grow(3.0, 400000.0, 1e6, 0.0, out double calm, out _);
            double t = 24.0;
            for (int i = 0; i < 24; i++)
                m.Step(t += 0.25, 0.25, 3.0, 270.0, 1013.0);
            double expected = calm + (before - calm) * Math.Exp(-6.0 / SeaStateModel.SeaDecayHours);
            Assert.AreEqual(expected, m.State.windSeaHs, 0.05 * expected, "an e-fold in six hours once the wind has dropped");

            // a wind that veers from west to north: the sea follows an hour and a half behind
            var turn = Steady(fetch, 12.0, 270.0, 12.0, 0.25);
            t = 12.0;
            for (int i = 0; i < 6; i++)
                turn.Step(t += 0.25, 0.25, 12.0, 0.0, 1013.0);
            float moved = turn.State.windSeaFrom - 270f;
            if (moved < 0f)
                moved += 360f;
            Assert.That(moved, Is.InRange(45f, 75f), "after 1.5 h of the new wind the sea has turned about two thirds of the way");
        }

        [Test]
        public void TheLevelsOwnFetchCounts()
        {
            var f = SouthernNorthSea();
            Assert.AreEqual(600.0, f.Km(0), 1e-9, "N 600 km, open to Norway");
            Assert.AreEqual(120.0, f.Km(4), 1e-9, "S 120 km");
            Assert.AreEqual(30.0, f.DepthMetres, 1e-9);
            Assert.AreEqual(1000.0 * (600.0 + 350.0) / 2.0, f.FetchMetres(22.5), 1e-6, "between the sectors");
            var north = Steady(f, 15.0, 0.0, 30.0, 0.25);
            var south = Steady(f, 15.0, 180.0, 30.0, 0.25);
            Assert.Greater(north.State.windSeaHs, south.State.windSeaHs * 1.3f, "a northerly over 600 km raises more sea than a southerly over 120");
            Assert.That(north.State.whitecaps, Is.InRange(0.02f, 0.08f), "whitecaps at 15 m/s");
            Assert.That(north.State.highestIn30Min / north.State.hs, Is.InRange(1.55f, 1.8f), "the highest wave of half an hour");
            Assert.That(north.State.steepness, Is.InRange(0.02f, 0.09f));
        }

        /// <summary>Design test 13, adapted: the 838 storm needs the chronicle's storms (week 2), so the test finds the
        /// model's own gales and swells over a winter at the southern North Sea's level.</summary>
        [Test]
        public void AWarmStartOpensOnTheSeaAlreadyUp()
        {
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            var fetch = SouthernNorthSea();
            var warm = new SeaStateModel(e, SeaLat, SeaLon, "north_sea", fetch);
            var flat = new SeaStateModel(e, SeaLat, SeaLon, "north_sea", fetch);
            bool gale = false, swell = false;
            double start = WeatherEngine.HoursAt(838, 280, 0.0), end = WeatherEngine.HoursAt(839, 90, 0.0);
            for (double h = start; h < end && !(gale && swell); h += 6.0)
            {
                var now = e.Sample(SeaLat, SeaLon, h, "north_sea");
                bool blowing = now.windSpeed >= 15f;
                for (int k = 1; k <= 6 && blowing; k++)
                    blowing = e.Sample(SeaLat, SeaLon, h - k, "north_sea").windSpeed >= 13f;
                if (!gale && blowing)
                {
                    var s = warm.WarmStart(h);
                    flat.Reset();
                    var f = flat.Step(h, 1.0 / 60.0, now);
                    Assert.Less(f.windSeaHs, 0.3f, "a flat start has no sea yet (the test of the test)");
                    if (s.windSeaHs >= 2f)
                        gale = true;
                }
                if (!swell && h >= start + 24.0 * 30.0 && ((long)(h / 6.0)) % 2 == 0)
                {
                    var s = warm.WarmStart(h);
                    if (s.swellHs >= 1f && s.swellTp >= 10f)
                    {
                        swell = true;
                        flat.Reset();
                        var f = flat.Step(h, 1.0 / 60.0, now);
                        Assert.AreEqual(0f, f.swellHs, "a flat start shows no swell (the test of the test)");
                        Assert.Greater(s.hs, s.windSeaHs, "the swell adds to the sea");
                    }
                }
            }
            Assert.IsTrue(gale, "a level entered in a gale opens on a sea of 2 m or more");
            Assert.IsTrue(swell, "some winter hour opens with a swell train of 10 s and 1 m or more already running");
        }

        [Test]
        public void AWarmStartIsTheSeaSteppedOn()
        {
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            var fetch = SouthernNorthSea();
            double h = WeatherEngine.HoursAt(838, 330, 12.0);
            var a = new SeaStateModel(e, SeaLat, SeaLon, "north_sea", fetch);
            var b = new SeaStateModel(e, SeaLat, SeaLon, "north_sea", fetch);
            var c = new SeaStateModel(e, SeaLat, SeaLon, "north_sea", fetch);
            var sa = a.WarmStart(h);
            var sc = c.WarmStart(h);
            Assert.AreEqual(sa.hs, sc.hs, "the same moment gives the same sea");
            Assert.AreEqual(sa.swellHs, sc.swellHs);
            b.WarmStart(h - 12.0);
            for (int i = 1; i <= 12; i++)
                b.Step(h - 12.0 + i, 1.0, e.Sample(SeaLat, SeaLon, h - 12.0 + i, "north_sea"));
            var sb = b.State;
            Assert.AreEqual(sa.windSeaHs, sb.windSeaHs, Math.Max(0.15, 0.1 * sa.windSeaHs), "a warm start and the sea stepped on to the same hour");
            Assert.AreEqual(sa.hs, sb.hs, Math.Max(0.25, 0.15 * sa.hs));
            foreach (var s in new[] { sa, sb })
            {
                Assert.IsTrue(s.set);
                Assert.IsFalse(float.IsNaN(s.hs) || float.IsNaN(s.tp) || float.IsNaN(s.from) || float.IsNaN(s.surge));
                Assert.That(s.from, Is.InRange(0f, 360f));
                Assert.That(s.confused, Is.InRange(0f, 1f));
            }
        }

        /// <summary>A table shaped like HDRP's: the greatest displacement grows with the wind and with the patch, and
        /// levels off once the waves outgrow the patch.</summary>
        static float[] SyntheticTable()
        {
            var t = new float[SeaFit.TableSize * SeaFit.TableSize];
            for (int y = 0; y < SeaFit.TableSize; y++)
                for (int x = 0; x < SeaFit.TableSize; x++)
                {
                    double wind = x / 31.0 * 250.0 / 3.6, patch = 25.0 + y / 31.0 * 4975.0;
                    double free = 0.012 * wind * wind;                               // the sea the wind would raise
                    double room = 0.02 * patch;                                      // the most the patch can hold
                    t[x + y * SeaFit.TableSize] = (float)(free * room / (free + room));
                }
            return t;
        }

        [Test]
        public void TheSeaFitsHdrpsBands()
        {
            var table = SyntheticTable();
            Assert.AreEqual(1500.0 / (5.0 + 45.0 * 1250.0 / 4750.0), SeaFit.SecondPatch(1500.0), 1e-9);
            // a long swell: the period sets the wind, the height comes out exactly, little chop
            var swell = SeaFit.Fit(table, 1500.0, 2.0, 12.0, 0.3, 4.0);
            Assert.AreEqual(2.0, swell.hs, 0.02, "the swell's height");
            Assert.AreEqual(0.0, swell.tpError, 0.05, "its period met");
            Assert.That(swell.band0, Is.InRange(0f, 1f));
            Assert.That(swell.band1, Is.InRange(0f, 1f));
            // a storm sea at full real height: 9 m at 12 s
            var storm = SeaFit.Fit(table, 1500.0, 9.0, 12.0, 9.0, 12.0);
            Assert.AreEqual(9.0, storm.hs, 0.1, "the storm's full height");
            Assert.That(storm.windKmh, Is.InRange(1f, (float)SeaFit.MaxWindKmh));
            // a steep young sea, higher than HDRP's sea at its period: the wind rises to meet the height, the period runs long
            var young = SeaFit.Fit(table, 1500.0, 3.0, 5.0, 3.0, 5.0);
            Assert.AreEqual(3.0, young.hs, 0.03);
            Assert.Greater(young.tpError, 0f, "HDRP's sea runs longer than the model's");
            Assert.Greater(young.band1, swell.band1, "a steep sea shows more chop");
            // no sea, no wind
            var none = SeaFit.Fit(table, 1500.0, 0.0, 0.0, 0.0, 0.0);
            Assert.AreEqual(0f, none.windKmh);
            Assert.AreEqual(0f, none.band0);
        }
    }
}
