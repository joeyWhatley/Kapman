using System;
using Kapman.Weather;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// Weather night two, the sky from the model (Design/weather_and_sea_state.md 4.3): fog, frost, thunder and snow days
    /// against the measured tables (test 4, loose), the day's cycle (test 5), the light through the cloud (test 10), and the
    /// two scenes of section 2.5 coming out of a date and a place with nothing set by hand. "The coast" of the tables is De
    /// Kooy (the wadden class); the river land is De Bilt's (Dorestad). Seed 793, years 800-809 (the climate of 793: the
    /// modern normals cooled by 0.8 C, so frost runs a little over the modern counts).
    /// </summary>
    public class WeatherSkyTests
    {
        const double DorestadLat = 51.972, DorestadLon = 5.343;
        const double DeKooyLat = 52.92, DeKooyLon = 4.78;
        const int FirstYear = 800, Years = 10;

        struct YearCounts
        {
            public double fogDays, frostDays, thunderDays, thunderSummerDays, snowDays;
        }

        /// <summary>Days a year with an hour of fog (visibility under 1 km, KNMI's fog day), of frost (under 0 C), of thunder
        /// (a storm near), and of snow or sleet falling (2 C or colder), over the test's years, hour by hour.</summary>
        static YearCounts Count(string placeClass, double lat, double lon)
        {
            var e = WeatherEngine.Shared;
            var c = new YearCounts();
            for (int y = FirstYear; y < FirstYear + Years; y++)
                for (int d = 1; d <= 365; d++)
                {
                    bool fog = false, frost = false, thunder = false, snow = false;
                    for (int h = 0; h < 24; h++)
                    {
                        var s = e.Sample(lat, lon, WeatherEngine.HoursAt(y, d, h), placeClass);
                        fog |= s.visibility < 1000f;
                        frost |= s.temperature < 0f;
                        thunder |= s.thunder > 0.5f;
                        snow |= s.rain > 0.1f && s.temperature < 1.5f;
                    }
                    if (fog) c.fogDays++;
                    if (frost) c.frostDays++;
                    if (snow) c.snowDays++;
                    if (thunder)
                    {
                        c.thunderDays++;
                        if (d >= 152 && d < 274)                                   // June to September
                            c.thunderSummerDays++;
                    }
                }
            c.fogDays /= Years;
            c.frostDays /= Years;
            c.thunderDays /= Years;
            c.thunderSummerDays /= Years;
            c.snowDays /= Years;
            return c;
        }

        [Test]
        public void FogFrostThunderAndSnowDaysLookMeasured()
        {
            WeatherEngine.Reset(793);
            // measured (weather document 1.1, Tables D and E): De Kooy fog 44, frost 35, thunder 24 (two thirds June to
            // September), snow 18 days a year; De Bilt fog 66 (the undrained river land of 800 x 1.3: 86), frost 53.
            // The design's loose bounds, widened a little for the colder climate of 793 and ten years' spread
            var coast = Count("wadden", DeKooyLat, DeKooyLon);
            Assert.That(coast.fogDays, Is.InRange(25.0, 60.0), "fog days a year on the coast");
            Assert.That(coast.frostDays, Is.InRange(25.0, 55.0), "frost days a year on the coast");
            Assert.That(coast.thunderDays, Is.InRange(14.0, 36.0), "thunder days a year on the coast");
            Assert.That(coast.thunderSummerDays / Math.Max(coast.thunderDays, 1.0), Is.InRange(0.45, 0.85), "the share of thunder days in June to September");
            Assert.That(coast.snowDays, Is.InRange(8.0, 30.0), "snow days a year on the coast");

            var land = Count("river_land", DorestadLat, DorestadLon);
            Assert.That(land.fogDays, Is.InRange(50.0, 95.0), "fog days a year on the river land");
            Assert.That(land.frostDays, Is.InRange(38.0, 70.0), "frost days a year on the river land");
            Assert.Greater(land.fogDays, coast.fogDays, "the river land fogs more than the coast");
        }

        [Test]
        public void TheDaysCycle()
        {
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            double night = 0.0, afternoon = 0.0;
            int nNight = 0, nAfternoon = 0, n10 = 0, n17 = 0, nnw10 = 0, nnw17 = 0;
            var fogByHour = new int[24];
            for (int y = FirstYear; y < FirstYear + 5; y++)
            {
                for (int d = 152; d < 244; d++)                                    // June to August on the coast
                    for (int h = 0; h < 24; h++)
                    {
                        var s = e.Sample(DeKooyLat, DeKooyLon, WeatherEngine.HoursAt(y, d, h), "wadden");
                        if (h <= 4) { night += s.windSpeed; nNight++; }
                        if (h >= 13 && h <= 15) { afternoon += s.windSpeed; nAfternoon++; }
                        if (d >= 182 && d < 213 && (h == 10 || h == 17))           // July: the sea breeze turns the wind N-NW
                        {
                            bool nnw = s.windFrom >= 300f || s.windFrom <= 30f;
                            if (h == 10) { n10++; if (nnw) nnw10++; }
                            else { n17++; if (nnw) nnw17++; }
                        }
                    }
                for (int d = 1; d <= 365; d++)                                     // the river land's fog through the day
                    for (int h = 0; h < 24; h++)
                        if (e.Sample(DorestadLat, DorestadLon, WeatherEngine.HoursAt(y, d, h), "river_land").visibility < 1000f)
                            fogByHour[h]++;
            }
            // measured at De Kooy: the summer afternoon wind is half again the night's; N-NW 20% of hours at 10-11 h, 31% at
            // 17-18 h (the sea breeze); fog peaks about dawn (weather document 1.1, the day's cycle)
            Assert.That((afternoon / nAfternoon) / (night / nNight), Is.InRange(1.4, 1.6), "summer afternoon wind over the night's");
            Assert.GreaterOrEqual((double)nnw17 / n17, 1.3 * nnw10 / n10, $"N-NW in July at 17 h ({nnw17}/{n17}) against 10 h ({nnw10}/{n10})");
            int peak = 0;
            for (int h = 1; h < 24; h++)
                if (fogByHour[h] > fogByHour[peak])
                    peak = h;
            Assert.That(peak, Is.InRange(3, 8), "the hour fog is commonest");
            Assert.Greater(fogByHour[peak], 5 * Math.Max(1, fogByHour[14]), "fog at dawn against fog at 14 h");
        }

        [Test]
        public void TheLightThroughTheCloud()
        {
            // the table of weather document 2.5 at full cover, for every kind and thickness
            foreach (CloudType type in Enum.GetValues(typeof(CloudType)))
            {
                CloudLight.TransmittanceRange(type, out double thick, out double thin);
                for (double t = 0.0; t <= 1.0001; t += 0.25)
                {
                    double full = CloudLight.Layer(type, 1.0, t);
                    Assert.That(full, Is.InRange(thick - 1e-9, thin + 1e-9), $"{type} at thickness {t}");
                    Assert.LessOrEqual(full, CloudLight.TwoStream(CloudLight.OpticalDepth(type, t)) + 1e-9, $"{type} under the two-stream ceiling");
                    Assert.AreEqual(full, CloudLight.Column(type, 1.0, t, CloudType.Clear, 0.0, 0.0, CloudType.Clear, 0.0, 0.0), 1e-12);
                }
            }
            // a nimbostratus deck at a 30-degree Sun (50,000 lux clear on the flat) lets through 500-2,000 lux
            for (double t = 0.0; t <= 1.0001; t += 0.25)
                Assert.That(50000.0 * CloudLight.Layer(CloudType.Nimbostratus, 1.0, t), Is.InRange(500.0, 2000.0), "lux under nimbostratus");
            // a clear June noon at 52 N: 90,000-110,000 lux
            double clearNoon = LightingModel.TotalLux(LightingModel.JuneNoonAltitude, 0.0, 0.0, 0.0, -30.0);
            Assert.That(clearNoon * CloudLight.Layer(CloudType.Clear, 1.0, 0.5), Is.InRange(90000.0, 110000.0), "a clear June noon");
            // half a sky of cumulus passes about half the light; a thin veil of cirrus most of it
            Assert.That(CloudLight.Layer(CloudType.Cumulus, 0.5, 0.5), Is.InRange(0.55, 0.6));
            Assert.That(CloudLight.Layer(CloudType.Cirrus, 0.6, 0.5), Is.InRange(0.9, 0.95));

            // and the engine's own sky: June noons at Dorestad run from clear (all the Sun) to a rain deck (a hundredth)
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            double most = 0.0, least = 1.0;
            for (int y = FirstYear; y < FirstYear + 3; y++)
                for (int d = 152; d < 182; d++)
                {
                    var s = e.Sample(DorestadLat, DorestadLon, WeatherEngine.HoursAt(y, d, 12.0), "river_land");
                    Assert.IsTrue(s.HasSky, "the engine works out the sky");
                    Assert.That(s.cloudTransmittance, Is.InRange(1e-4f, 1f));
                    most = Math.Max(most, s.cloudTransmittance);
                    least = Math.Min(least, s.cloudTransmittance);
                    if (s.cloudType == CloudType.Nimbostratus && s.lowCloud >= 0.99f)
                        Assert.That(s.cloudTransmittance, Is.InRange(0.003f, 0.03f), "under a whole nimbostratus deck");
                }
            Assert.Greater(most, 0.9, "some clear June noons");
            Assert.Less(least, 0.06, "some June noons under a deck");
        }

        [Test]
        public void TheFrostyDawnAndTheSleetDayComeOutOfTheModel()
        {
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            // weather document 2.5: a frosty calm dawn (a January high on the river land: -5 C, dew point -6, a breath of
            // wind, a clear sky, mist or radiation fog lifting) and an overcast sleet day (a February warm front on the
            // coast: nimbostratus at a few hundred metres, sleet at +1.5 C, 2-3 km visibility, a few hundred lux)
            int frostyDays = 0, sleetDays = 0;
            for (int y = 793; y < 803; y++)
            {
                for (int d = 1; d <= 31; d++)
                    for (int h = 5; h <= 9; h++)
                    {
                        var s = e.Sample(DorestadLat, DorestadLon, WeatherEngine.HoursAt(y, d, h), "river_land");
                        if (s.temperature <= -2f && s.windSpeed <= 2.5f && s.lowCloud < 0.35f && s.midCloud < 0.35f
                            && s.dewPoint >= s.temperature - 3f && s.visibility < 10000f && s.cloudTransmittance > 0.6f && s.rain < 0.05f)
                        {
                            frostyDays++;
                            break;
                        }
                    }
                for (int d = 32; d <= 59; d++)
                    for (int h = 9; h <= 17; h++)
                    {
                        var s = e.Sample(DeKooyLat, DeKooyLon, WeatherEngine.HoursAt(y, d, h), "wadden");
                        if (s.Sleet && s.rain >= 0.2f && s.lowCloud >= 0.85f
                            && (s.cloudType == CloudType.Nimbostratus || s.cloudType == CloudType.Stratus)
                            && s.cloudTransmittance <= 0.06f && s.visibility <= 8000f)
                        {
                            sleetDays++;
                            break;
                        }
                    }
            }
            Assert.GreaterOrEqual(frostyDays, 3, "frosty calm dawns in ten Januaries at Dorestad");
            Assert.GreaterOrEqual(sleetDays, 2, "sleet days in ten Februaries on the coast");
        }

        [Test]
        public void TheSkyIsAllNumbersAndHoldsTogether()
        {
            WeatherEngine.Reset(793);
            var e = WeatherEngine.Shared;
            string[] classes = { "river_land", "wadden", "coast", "north_sea" };
            double[,] places = { { DorestadLat, DorestadLon }, { DeKooyLat, DeKooyLon }, { 51.99, 4.12 }, { 52.6, 3.0 } };
            for (int k = 0; k < classes.Length; k++)
                for (int d = 1; d <= 365; d += 4)
                    for (int h = 0; h < 24; h += 5)
                    {
                        var s = e.Sample(places[k, 0], places[k, 1], WeatherEngine.HoursAt(820, d, h), classes[k]);
                        string at = $"{classes[k]} day {d} {h}:00";
                        foreach (float v in new[] { s.dewPoint, s.lowCloud, s.midCloud, s.highCloud, s.cloudBase, s.cloudDepth, s.cloudTransmittance, s.frontPhase, s.thunder, s.fogTop, s.visibility })
                            Assert.IsFalse(float.IsNaN(v) || float.IsInfinity(v), "a number at " + at);
                        Assert.LessOrEqual(s.dewPoint, s.temperature + 1e-3f, "the dew point is never over the air's temperature, " + at);
                        Assert.That(s.lowCloud, Is.InRange(0f, 1f));
                        Assert.That(s.midCloud, Is.InRange(0f, 1f));
                        Assert.That(s.highCloud, Is.InRange(0f, 1f));
                        Assert.That(s.thunder, Is.InRange(0f, 1f));
                        Assert.That(s.cloudBase, Is.InRange(100f, 12000f), at);
                        Assert.That(s.cloudDepth, Is.InRange(200f, 12000f), at);
                        Assert.That(s.visibility, Is.InRange((float)Visibility.MinRange, (float)Visibility.MaxRange), at);
                        Assert.GreaterOrEqual(s.rain, 0f);
                        // the first build's cover fields still carry the sky for their readers
                        Assert.That(s.Cover, Is.InRange(0f, 1f));
                        if (s.lowCloud >= 0.9f)
                            Assert.Greater(s.Cover, 0.5f, "a whole low deck reads as cloud, " + at);
                    }
        }
    }
}
