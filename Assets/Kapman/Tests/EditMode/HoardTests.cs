using System.Collections.Generic;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Hoard hunts: the real hoards load with their landmarks and their clue's measures; following the measures
    /// from the landmarks lands on the find; a reward with a place opens the nearest hoard's hunt; hunts save and load.</summary>
    public class HoardTests
    {
        [SetUp]
        public void Load()
        {
            Hoards.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/hoards_793.json"));
            Hoards.Clear();
        }

        [TearDown]
        public void Done() => Hoards.Clear();

        [Test]
        public void TheRealHoardsLoadWithTheirClues()
        {
            Assert.GreaterOrEqual(Hoards.All.Count, 30, "the hoards");
            var cuerdale = Hoards.Get("cuerdale");
            Assert.IsNotNull(cuerdale);
            Assert.Greater(cuerdale.silverG, 10000.0, "tens of kilograms of silver");
            Assert.Greater(cuerdale.landmarks.Count, 2);
            Assert.Greater(cuerdale.measures.Count, 1);
            StringAssert.Contains("paces", cuerdale.measures[0]);
            foreach (var h in Hoards.All.Values)
                Assert.Greater(h.measures.Count, 0, h.id + " has a clue");
        }

        [Test]
        public void FollowingTheFirstMeasureFromItsLandmarkGoesTheRightWay()
        {
            // Cuerdale: from the Darwen mouth toward Samlesbury, 1,181 paces: the pin lies along that line
            var h = Hoards.Get("cuerdale");
            var from = h.Landmark("darwen_mouth");
            var toward = h.Landmark("samlesbury");
            Assert.IsNotNull(from);
            Assert.IsNotNull(toward);
            var (ax, az) = Kapman.World.WorldFrame.ToWorld(from.lat, from.lon);
            var (bx, bz) = Kapman.World.WorldFrame.ToWorld(toward.lat, toward.lon);
            double dx = bx - ax, dz = bz - az, len = System.Math.Sqrt(dx * dx + dz * dz);
            double px = ax + dx / len * 1181 * Hoards.PaceM, pz = az + dz / len * 1181 * Hoards.PaceM;
            var (lat, lon) = Kapman.World.WorldFrame.ToLatLon(px, pz);
            Assert.AreEqual(53.75443, lat, 0.0005, "the pin's latitude, as the measures have it");
            Assert.AreEqual(-2.65998, lon, 0.0008, "and longitude");
        }

        [Test]
        public void ARewardWithAPlaceOpensTheNearestHunt()
        {
            var h = Hoards.OpenNear(53.76, -2.648);
            Assert.IsNotNull(h);
            Assert.AreEqual("cuerdale", h.id);
            Assert.IsTrue(Hoards.IsOpen("cuerdale"));
            Assert.IsNull(Hoards.OpenNear(0.0, 0.0), "nothing near the Gulf of Guinea");
        }

        [Test]
        public void HuntsSaveAndLoad()
        {
            Hoards.OpenHunt("cuerdale");
            string saved = Json.Write(Hoards.Capture());
            Hoards.Clear();
            Assert.IsFalse(Hoards.IsOpen("cuerdale"));
            Hoards.Restore(Json.Parse(saved) as Dictionary<string, object>);
            Assert.IsTrue(Hoards.IsOpen("cuerdale"));
        }
    }
}
