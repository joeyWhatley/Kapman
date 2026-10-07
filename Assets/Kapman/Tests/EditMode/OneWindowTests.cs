using System.Linq;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The tests of this folder are of the windows as they tile (workspaces, rows, columns, tabs): they run with
    /// the side's one window switched off, whatever a player has.</summary>
    [SetUpFixture]
    public class TiledWindowsFixture
    {
        bool was;

        [OneTimeSetUp]
        public void Tiled()
        {
            was = FrameWindows.OneWindow;
            FrameWindows.OneWindow = false;
        }

        [OneTimeTearDown]
        public void Back() => FrameWindows.OneWindow = was;
    }

    /// <summary>
    /// ONE WINDOW ON THE SIDE (the user, 7 Oct 2026, night: "the text is far too crowded on the side so maybe just one
    /// panel"): each workspace starts with one window (Voyage at sea, Port in port), a panel asked for takes that
    /// window's place, no second window opens, and an arrangement of several kept from before is cut to the one that
    /// had the focus.
    /// </summary>
    public class OneWindowTests
    {
        static readonly string[] Some = { "voyage", "trade", "hold", "tasks", "port", "people", "self", "house", "wares" };

        [SetUp]
        public void One() => FrameWindows.OneWindow = true;

        [TearDown]
        public void Tiled() => FrameWindows.OneWindow = false;

        [Test]
        public void EachWorkspaceStartsWithOneWindow()
        {
            var w = new FrameWindows(() => Some);
            Assert.AreEqual(4, w.spaces.Count);
            CollectionAssert.AreEqual(new[] { "voyage" }, w.spaces[FrameWindows.SeaSpace].slots);
            CollectionAssert.AreEqual(new[] { "port" }, w.spaces[FrameWindows.PortSpace].slots);
            Assert.IsTrue(w.spaces.All(s => s.slots.Count == 1 && s.layout == FrameLayout.Rows));
        }

        [Test]
        public void APanelAskedForTakesTheWindow()
        {
            var w = new FrameWindows(() => Some);
            w.Go(FrameWindows.PortSpace);
            w.Show("trade");
            CollectionAssert.AreEqual(new[] { "trade" }, w.Space.slots, "the one window did not turn to the panel asked for");
            Assert.AreEqual(FrameWindows.PortSpace, w.Current, "asking for a panel changed the workspace");
            w.Show("wares");
            CollectionAssert.AreEqual(new[] { "wares" }, w.Space.slots);
            Assert.IsFalse(w.Add(), "a second window opened");
            Assert.AreEqual(1, w.Space.slots.Count);
            // (at sea she comes up on her own window again, and the port keeps the one it had)
            w.Go(FrameWindows.SeaSpace);
            CollectionAssert.AreEqual(new[] { "voyage" }, w.Space.slots);
            w.Go(FrameWindows.PortSpace);
            CollectionAssert.AreEqual(new[] { "wares" }, w.Space.slots);
        }

        [Test]
        public void AnArrangementOfSeveralIsCutToTheOneInFocus()
        {
            FrameWindows.OneWindow = false;
            var tiled = new FrameWindows(() => Some);
            tiled.Go(FrameWindows.PortSpace);
            Assert.Greater(tiled.Space.slots.Count, 1);
            tiled.Focus(1);
            string focused = tiled.Focused;
            string kept = tiled.Capture();
            FrameWindows.OneWindow = true;
            var one = new FrameWindows(() => Some);
            Assert.IsTrue(one.Restore(kept));
            one.Go(FrameWindows.PortSpace);
            CollectionAssert.AreEqual(new[] { focused }, one.Space.slots, "the kept arrangement was not cut to the window that had the focus");
            Assert.IsTrue(one.spaces.All(s => s.slots.Count == 1));
        }
    }
}
