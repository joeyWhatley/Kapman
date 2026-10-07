using System;
using Kapman.Weather;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The weather engine at Dorestad over a year of 793: the numbers the level's weather now takes from it.</summary>
    public class WeatherEngineTests
    {
        const double Lat = 51.972, Lon = 5.343;

        static WeatherSample At(int day, double hour) =>
            WeatherEngine.Shared.Sample(Lat, Lon, WeatherEngine.HoursAt(793, day, hour), "river_land");

        [Test]
        public void AYearAtDorestadLooksLikeTheRhineDelta()
        {
            WeatherEngine.Reset(793);
            double jan = 0, jul = 0, windSum = 0, windMax = 0;
            int n = 0, raining = 0, foggy = 0;
            for (int day = 1; day <= 365; day++)
                for (int h = 0; h < 24; h += 3)
                {
                    var s = At(day, h);
                    Assert.IsFalse(float.IsNaN(s.temperature) || float.IsNaN(s.windSpeed) || float.IsNaN(s.visibility), $"NaN on day {day} at {h}:00");
                    Assert.That(s.windSpeed, Is.InRange(0f, 45f), $"wind on day {day}");
                    Assert.That(s.temperature, Is.InRange(-25f, 38f), $"temperature on day {day}");
                    Assert.That(s.windFrom, Is.InRange(0f, 360f));
                    Assert.Greater(s.VisibilityKm, 0f);
                    if (day <= 31) jan += s.temperature;
                    if (day >= 182 && day <= 212) jul += s.temperature;
                    windSum += s.windSpeed;
                    windMax = Math.Max(windMax, s.windSpeed);
                    if (s.rain > 0.1f) raining++;
                    if (s.VisibilityKm < 1f) foggy++;
                    n++;
                }
            jan /= 31 * 8;
            jul /= 31 * 8;
            // De Bilt 1991-2020: January about 3.7 C, July about 18.5 C; 793 is set 0.8 C cooler (decision 17). Loose bounds:
            // this checks the engine runs on the real data and has seasons, not the exact normals
            Assert.That(jul - jan, Is.InRange(9.0, 22.0), $"January {jan:0.0} C, July {jul:0.0} C");
            Assert.That(windSum / n, Is.InRange(2.0, 9.0), "mean wind (m/s)");
            Assert.Greater(windMax, 12.0, "some gales in a year");
            Assert.That((double)raining / n, Is.InRange(0.02, 0.45), "share of hours with rain");
            Assert.That((double)foggy / n, Is.InRange(0.0, 0.2), "share of hours in fog");
        }

        [Test]
        public void TheSameSeedGivesTheSameWeather()
        {
            WeatherEngine.Reset(1234);
            var a = At(40, 6.0);
            WeatherEngine.Reset(5678);
            WeatherEngine.Reset(1234);
            var b = At(40, 6.0);
            Assert.AreEqual(a.windSpeed, b.windSpeed, 1e-6);
            Assert.AreEqual(a.temperature, b.temperature, 1e-6);
            WeatherEngine.Reset(793);
        }
    }
}
