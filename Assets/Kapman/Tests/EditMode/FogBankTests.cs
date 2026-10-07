using Kapman.Weather;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The fog banks' facts (Code/Weather/FogBankField.cs): how much of the sea they cover, and how far one sees in and
    /// through them. The lookout, the map's fog of war and the raiders all read these, and the screen draws the same.
    /// </summary>
    public class FogBankTests
    {
        static readonly WeatherSample Fog = new WeatherSample { fog = 0.7f, rain = 0f, windSpeed = 3f };

        [Test]
        public void TheBanksCoverMoreOfTheSeaAsTheFogThickens()
        {
            var f = new FogBankField();
            Assert.AreEqual(0f, f.Cover(f.fogFrom), 1e-6);
            Assert.AreEqual(1f, f.Cover(1f), 1e-6);
            Assert.Less(f.Cover(0.5f), f.Cover(0.7f));
            foreach (float strength in new[] { 0.45f, 0.6f, 0.8f })
            {
                // the share of the sea that lies under a bank (half its fog or more) is the share asked for
                double hs = FloatingOrigin.HorizontalScale;
                int under = 0, all = 0;
                for (int i = 0; i < 120; i++)
                    for (int j = 0; j < 120; j++, all++)
                        if (f.BankAt(new WorldPos((i * 61.7 + 3.0) * hs, 0.0, (j * 59.3 + 7.0) * hs), strength) >= 0.5f)
                            under++;
                Assert.AreEqual(f.Cover(strength), (double)under / all, 0.06, $"at strength {strength}");
            }
        }

        [Test]
        public void SomeBanksLieUnderTheMastheadAndSomeSwallowHer()
        {
            // the user (2026-10-02): the mast should dip in and out of the fog, not always stand above it
            var f = new FogBankField();
            const float Masthead = 4.7f;                                      // the Gislinge's, game metres
            double hs = FloatingOrigin.HorizontalScale;
            float Mean(float strength, out int under, out int over)
            {
                float sum = 0f;
                under = over = 0;
                for (int i = 0; i < 60; i++)
                    for (int j = 0; j < 60; j++)
                    {
                        float top = f.TopAt(new WorldPos((i * 47.3) * hs, 0.0, (j * 41.9) * hs), strength);
                        Assert.That(top, Is.InRange(f.topLow, f.topHigh));
                        sum += top;
                        if (top < Masthead * 0.8f) under++;
                        if (top * 0.8f > Masthead) over++;
                    }
                return sum / 3600f;
            }
            float thin = Mean(0.5f, out _, out _), mid = Mean(0.7f, out int low, out int deep), thick = Mean(1f, out _, out _);
            Assert.Less(thin, mid);
            Assert.Less(mid, thick, "deeper banks in thicker fog");
            Assert.Greater(low, 360, "in middling fog many banks leave her masthead clear");
            Assert.Greater(deep, 120, "and some swallow it");
        }

        [Test]
        public void NoBanksBelowTheFirstFog()
        {
            var f = new FogBankField();
            for (int i = 0; i < 50; i++)
                Assert.AreEqual(0f, f.BankAt(new WorldPos(i * 977.0, 0.0, i * 431.0), f.fogFrom * 0.9f));
            var clear = new WeatherSample { fog = 0.1f };
            Assert.AreEqual(FogBankField.HazeKm(clear), f.VisibilityKmAt(clear, new WorldPos(1000.0, 0.0, 2000.0)), 1e-6);
        }

        [Test]
        public void ALookoutInAClearGapStillLosesASailBehindABank()
        {
            var f = new FogBankField();
            float strength = f.Strength(Fog);
            double hs = FloatingOrigin.HorizontalScale;
            // find a place deep in a bank and a place in a clear gap
            WorldPos inBank = default, inGap = default;
            bool haveBank = false, haveGap = false;
            for (int i = 0; i < 400 && !(haveBank && haveGap); i++)
            {
                var p = new WorldPos((i * 37.1) * hs, 0.0, (i * 23.9 % 900.0) * hs);
                float b = f.BankAt(p, strength);
                if (!haveBank && b > 0.99f) { inBank = p; haveBank = true; }
                if (!haveGap && b <= f.mist * f.Cover(strength) + 1e-4f) { inGap = p; haveGap = true; }
            }
            Assert.IsTrue(haveBank && haveGap, "a bank and a gap");
            double seeInBank = f.VisibilityKmAt(Fog, inBank), seeInGap = f.VisibilityKmAt(Fog, inGap);
            Assert.Less(seeInBank, seeInGap / 3.0, "in a bank one sees far less than in the gap beside it");
            // from the gap, looking at a sail inside the bank: the fog along the line hides her sooner than clear air would
            Assert.Less(f.VisibilityKmBetween(Fog, inGap, inBank), FogBankField.HazeKm(Fog));
            // and a bank's visibility follows its mean free path: about four times it (3.9), in real km
            double expect = 3.912 / (1.0 / (f.MeanFreePath(strength) * hs) + 3.912 / (FogBankField.HazeKm(Fog) * 1000.0)) / 1000.0;
            Assert.AreEqual(expect, seeInBank, expect * 0.02);
        }
    }
}
