using System.IO;
using Kapman.Weather;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The tide must match the real sea: at Dover a lunar month of June-July 793 must hold spring tides of about 6 m
    /// and neaps of about 3.5 m (Dover's mean spring range is 6.0 m, its mean neap 3.2 m), springs falling a day or
    /// three after the full moon, as the Channel's are. Needs the baked tides (Bake/tides/bake_tides.py).
    /// </summary>
    public class TideModelTests
    {
        const string TidesFile = @"G:\kapman_unity\data\baked\world\tides.bin";
        const double June10Of793 = 1721423.5 + 365.0 * 792 + 198 + 160;         // the Julian day

        static double DailyRange(TideModel model, double lat, double lon, double jd)
        {
            double lo = double.MaxValue, hi = double.MinValue;
            for (int k = 0; k < 24 * 6; k++)
            {
                double h = model.HeightAt(lat, lon, jd + k / (24.0 * 6.0));
                lo = System.Math.Min(lo, h);
                hi = System.Math.Max(hi, h);
            }
            return hi - lo;
        }

        [Test]
        public void DoverHasItsSpringsAndNeaps()
        {
            if (!File.Exists(TidesFile))
                Assert.Inconclusive("No baked tides: run Bake/tides/bake_tides.py.");
            var model = TideModel.Load(TidesFile);
            double spring = 0, neap = double.MaxValue;
            int springDay = -1;
            for (int d = 0; d < 30; d++)
            {
                double range = DailyRange(model, 51.10, 1.35, June10Of793 + d);
                if (range > spring)
                {
                    spring = range;
                    springDay = d;
                }
                neap = System.Math.Min(neap, range);
            }
            Assert.That(spring, Is.InRange(5.5, 7.5), "Dover's spring range");
            Assert.That(neap, Is.InRange(2.8, 4.2), "Dover's neap range");
            // the full moon of that month falls on day 18 (the Moon 180 degrees from the Sun): springs 1-4 days later
            Assert.That(springDay, Is.InRange(19, 22), "springs a day or three after the full moon");
        }

        [Test]
        public void TheDutchCoastHasASmallTide()
        {
            if (!File.Exists(TidesFile))
                Assert.Inconclusive("No baked tides: run Bake/tides/bake_tides.py.");
            var model = TideModel.Load(TidesFile);
            double range = DailyRange(model, 52.20, 4.30, June10Of793 + 11);       // Katwijk, at neaps and springs' middle
            Assert.That(range, Is.InRange(0.8, 2.2), "Katwijk's range is a metre and a half or so");
        }
    }
}
