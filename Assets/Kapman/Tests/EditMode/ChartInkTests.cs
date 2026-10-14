using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE NAMES INKED ON THE CHART (14 Oct 2026): a river's name lies along its water, a point on dry land is brought
    /// to the nearest water, an island's name stands level, and a shoal keeps a sign.
    /// </summary>
    public class ChartInkTests
    {
        static DiscoveryDef Thing(string kind, double x, double z, double reachKm) =>
            new DiscoveryDef { id = kind + "-1", kind = kind, name = "A " + kind, x = x, z = z, reach = reachKm * 1000.0 };

        [Test]
        public void ARiverRunningEastIsLetteredLevel()
        {
            bool water(double x, double z) => z > -300.0 && z < 300.0;   // a river 600 m wide running east
            Assert.IsTrue(ChartInk.Place(Thing("river", 0.0, 0.0, 9.0), water, out double x, out double z, out float angle));
            Assert.AreEqual(0.0, x, 1e-9);
            Assert.AreEqual(0.0, z, 1e-9);
            Assert.AreEqual(0f, angle, 2f, "a river running east should read level");
        }

        [Test]
        public void ARiverRunningNorthIsLetteredUpTheChart()
        {
            bool water(double x, double z) => x > -300.0 && x < 300.0;   // a river running north
            Assert.IsTrue(ChartInk.Place(Thing("river", 0.0, 0.0, 9.0), water, out _, out _, out float angle));
            Assert.AreEqual(90f, System.Math.Abs(angle), 2f, "a river running north should be lettered up or down the chart");
        }

        [Test]
        public void ARiverRunningNorthEastIsLetteredAlongIt()
        {
            bool water(double x, double z) => System.Math.Abs(x - z) < 300.0;   // a river running north-east (x = z)
            Assert.IsTrue(ChartInk.Place(Thing("river", 0.0, 0.0, 9.0), water, out _, out _, out float angle));
            Assert.AreEqual(-45f, angle, 3f, "a river running north-east should read up to the right (the screen's y runs south)");
        }

        [Test]
        public void APointOnDryLandIsBroughtToTheNearestWater()
        {
            bool water(double x, double z) => z > 1700.0 && z < 2300.0;  // the river lies 2 km north of the point
            Assert.IsTrue(ChartInk.Place(Thing("river", 0.0, 0.0, 9.0), water, out double x, out double z, out float angle));
            Assert.IsTrue(water(x, z), "the name's place is not on the water");
            Assert.Less(System.Math.Abs(x), 600.0, "the name was brought farther than need be (the ring is searched at 24 points: the first hit lies within 15 degrees of straight)");
            Assert.AreEqual(0f, angle, 2f);
        }

        [Test]
        public void ARiverWithNoWaterWithinReachIsNotLettered()
        {
            bool water(double x, double z) => false;
            Assert.IsFalse(ChartInk.Place(Thing("river", 0.0, 0.0, 9.0), water, out _, out _, out _));
        }

        [Test]
        public void AnIslandAndAShoalStandWhereTheyAreAndTheShoalHasASign()
        {
            bool water(double x, double z) => z > 5000.0;               // the point itself is land: an island is named where it is
            Assert.IsTrue(ChartInk.Place(Thing("island", 100.0, 200.0, 20.0), water, out double x, out double z, out float angle));
            Assert.AreEqual(100.0, x, 1e-9);
            Assert.AreEqual(200.0, z, 1e-9);
            Assert.AreEqual(0f, angle);
            Assert.IsFalse(ChartInk.Along("island"));
            Assert.IsFalse(ChartInk.Signed("island"));
            Assert.IsTrue(ChartInk.Along("river") && ChartInk.Along("water"));
            Assert.IsTrue(ChartInk.Signed("bank") && ChartInk.Signed("race"));
            Assert.IsFalse(ChartInk.Signed("river"));
        }

        [Test]
        public void WithoutTheBakeANameGoesAtItsPointLevel()
        {
            Assert.IsTrue(ChartInk.Place(Thing("water", 10.0, 20.0, 24.0), null, out double x, out double z, out float angle));
            Assert.AreEqual(10.0, x);
            Assert.AreEqual(20.0, z);
            Assert.AreEqual(0f, angle);
        }
    }
}
