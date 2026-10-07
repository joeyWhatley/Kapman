using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The frame's first panels and its measure, without the scene (7 Oct 2026): the eight panels are found, in their
    /// order, and the workspaces a new player has hold only panels that are there; the days of a passage are a low and a
    /// high figure that grow with the way; and the way by water between two places goes by the lanes.
    /// </summary>
    public class FramePanelTests
    {
        [Test]
        public void TheFirstPanelsAreAllThere()
        {
            var ids = FramePanels.Ids;
            foreach (string id in new[] { "voyage", "trade", "port", "hold", "tasks", "word", "chronicle", "event" })
                CollectionAssert.Contains(ids, id);
            Assert.AreEqual("voyage", ids[0], "Voyage stands first");
            Assert.AreEqual("The word", FramePanels.Title("word"));
            // every panel the default workspaces name is one of them
            foreach (var (name, _, panels) in FrameWindows.Defaults)
                foreach (string id in panels)
                    CollectionAssert.Contains(ids, id, "the workspace " + name + " names a panel that is not there");
            var windows = new FrameWindows(() => FramePanels.Ids);
            CollectionAssert.AreEqual(new[] { "voyage", "hold" }, windows.spaces[FrameWindows.SeaSpace].slots);
            CollectionAssert.AreEqual(new[] { "trade", "port", "people" }, windows.spaces[FrameWindows.PortSpace].slots);
        }

        [Test]
        public void APassageIsALowAndAHighFigureOfDays()
        {
            var (low, high) = FramePassage.Days(100.0);
            Assert.Greater(low, 0.0);
            Assert.Greater(high, low, "a foul wind takes longer than a fair one");
            var (low2, high2) = FramePassage.Days(300.0);
            Assert.AreEqual(low * 3.0, low2, 1e-9, "three times the way, three times the days");
            Assert.AreEqual(high * 3.0, high2, 1e-9);
            StringAssert.Contains("day", FramePassage.DaysWords(100.0));
            // her own class: the fair figure from a wind on the quarter, the foul one from a light wind ahead
            var cls = new ShipClassDef { sailKn = 4.8f, maxKn = 6.5f, oarsKn = 3.5f, rowingHours = 6f, closestToWind = 58f, polar = 1f, hasSail = true };
            var (fair, foul) = FramePassage.Speeds(cls);
            Assert.AreEqual(4.8 * 1.12 * SimUnits.KnotMs, fair, 0.01, "a fresh breeze on the quarter is her best");
            Assert.AreEqual(3.5 * 0.5 * SimUnits.KnotMs, foul, 0.01, "against a light wind the oars beat tacking");
        }

        [Test]
        public void TheMeasureGoesByWater()
        {
            if (SimRoutes.Graph() == null)
                Assert.Ignore("no sea lanes baked on this machine (G:/kapman_unity/data/baked/world/lanes.bin)");
            var a = At(51.97, 5.34);                                       // Dorestat
            var b = At(51.51, -0.12);                                      // Lundenwic
            var line = new List<WorldXZ>();
            Assert.IsTrue(FramePassage.Between(a, b, line, out double km));
            Assert.Greater(line.Count, 4, "a way of many points, not a straight line");
            double straight = WorldXZ.Distance(a, b) / 1000.0;
            Assert.Greater(km, straight * 1.05, "by water it is farther than as the crow flies");
            Assert.Less(km, straight * 3.0);
            Assert.AreEqual(a.x, line[0].x, 1e-6);
            Assert.AreEqual(b.x, line[line.Count - 1].x, 1e-6);
            Assert.AreEqual(km, FramePassage.Km(line), 1e-9);
            // and the other way round it is as far, near enough
            var back = new List<WorldXZ>();
            Assert.IsTrue(FramePassage.Between(b, a, back, out double kmBack));
            Assert.AreEqual(km, kmBack, km * 0.05);
        }

        static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }
    }
}
