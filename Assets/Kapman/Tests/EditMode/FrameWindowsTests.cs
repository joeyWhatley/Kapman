using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The frame's windows without the scene (7 Oct 2026): the tiling window manager's state as the keys and the clicks
    /// drive it (workspaces, layouts, a new window, close, focus, a window's panel stepped and picked), the arrangement
    /// kept between runs, the panels found by themselves, the stylesheet's palette classes in step with the game's
    /// palettes, and the plain words the panels use for days.
    /// </summary>
    public class FrameWindowsTests
    {
        static readonly string[] Eight = { "voyage", "trade", "hold", "tasks", "word", "chronicle", "event", "port" };

        static FrameWindows Make(params string[] ids) => new FrameWindows(() => ids);

        [Test]
        public void FourNamedWorkspacesEachWithItsOwnWindows()
        {
            var w = Make(Eight);
            Assert.AreEqual(4, w.spaces.Count);
            CollectionAssert.AreEqual(new[] { "Sea", "Port", "Self", "House" }, w.spaces.Select(s => s.name).ToArray());
            CollectionAssert.AreEqual(new[] { "voyage", "hold" }, w.spaces[0].slots);
            CollectionAssert.AreEqual(new[] { "trade", "port" }, w.spaces[1].slots);
            Assert.AreEqual(FrameLayout.Tabs, w.spaces[2].layout);
            Assert.AreEqual(0, w.Current);
            Assert.IsTrue(w.Go(1));
            Assert.IsFalse(w.Go(1), "already there");
            Assert.IsFalse(w.Go(4), "there are four");
            w.SetLayout(FrameLayout.Columns);
            Assert.AreEqual(FrameLayout.Columns, w.spaces[1].layout);
            Assert.AreEqual(FrameLayout.Rows, w.spaces[0].layout, "a layout is its workspace's own");
        }

        [Test]
        public void AWorkspaceOnlyHoldsPanelsThatAreThere()
        {
            var w = Make("hold", "chronicle");
            CollectionAssert.AreEqual(new[] { "hold" }, w.spaces[0].slots, "Voyage is not built: the Sea workspace shows what there is");
            CollectionAssert.AreEqual(new[] { "hold" }, w.spaces[1].slots, "a workspace with none of its panels takes the first there is");
            CollectionAssert.AreEqual(new[] { "chronicle" }, w.spaces[3].slots);
            Assert.AreEqual(0, Make().spaces[0].slots.Count, "no panels at all: no window");
        }

        [Test]
        public void NewCloseAndFocus()
        {
            var w = Make(Eight);
            int v = w.Version;
            Assert.IsTrue(w.Add());
            Assert.Greater(w.Version, v);
            CollectionAssert.AreEqual(new[] { "voyage", "hold", "trade" }, w.Space.slots, "a new window takes the first panel not shown");
            Assert.AreEqual(2, w.Space.focus, "and the focus");
            Assert.IsTrue(w.Add());
            Assert.IsFalse(w.Add(), "four windows at most");
            Assert.AreEqual(FrameWindows.MaxWindows, w.Space.slots.Count);
            w.Focus(0);
            w.FocusBy(-1);
            Assert.AreEqual(3, w.Space.focus, "the focus goes round the ends");
            w.FocusBy(1);
            Assert.AreEqual(0, w.Space.focus);
            Assert.AreEqual("voyage", w.Focused);
            w.Focus(3);
            Assert.IsTrue(w.Close(1));
            CollectionAssert.AreEqual(new[] { "voyage", "trade", "tasks" }, w.Space.slots);
            Assert.AreEqual(2, w.Space.focus, "the focus stays on the window it was on");
            Assert.IsTrue(w.Close(2));
            Assert.AreEqual(1, w.Space.focus);
            Assert.IsTrue(w.Close(0));
            Assert.IsFalse(w.Close(0), "the last window stays");
            CollectionAssert.AreEqual(new[] { "trade" }, w.Space.slots);
        }

        [Test]
        public void AWindowStepsThroughAndPicksAnyPanel()
        {
            var w = Make(Eight);
            Assert.IsTrue(w.Step(0, 1));
            Assert.AreEqual("trade", w.Space.slots[0], "the next panel");
            Assert.IsTrue(w.Step(0, 1));
            Assert.AreEqual("tasks", w.Space.slots[0], "Hold is shown beside it: stepped over");
            Assert.IsTrue(w.Step(0, -1));
            Assert.IsTrue(w.Step(0, -1));
            Assert.AreEqual("voyage", w.Space.slots[0]);
            Assert.IsTrue(w.Step(0, -1));
            Assert.AreEqual("port", w.Space.slots[0], "round the end of the list");
            Assert.IsTrue(w.Set(0, "chronicle"));
            Assert.IsFalse(w.Set(0, "nothing"), "not a panel");
            Assert.IsTrue(w.Set(0, "hold"));
            CollectionAssert.AreEqual(new[] { "hold", "chronicle" }, w.Space.slots, "a panel shown beside it changes places");
            var two = Make("hold", "chronicle");
            two.Add();
            Assert.IsFalse(two.Step(0, 1), "every panel is shown: nothing to step to");
        }

        [Test]
        public void ShowBringsAPanelUp()
        {
            var w = Make(Eight);
            w.Show("hold");
            Assert.AreEqual(0, w.Current);
            Assert.AreEqual(1, w.Space.focus);
            w.Show("trade");
            Assert.AreEqual(FrameWindows.PortSpace, w.Current, "the first workspace that shows it");
            Assert.AreEqual("trade", w.Focused);
            w.Show("chronicle");
            Assert.AreEqual(3, w.Current);
            Assert.AreEqual("chronicle", w.Focused);
            var few = Make("hold", "chronicle", "extra");
            few.Show("extra");
            Assert.AreEqual("extra", few.Focused, "shown nowhere: the focused window takes it");
        }

        [Test]
        public void TheArrangementIsKeptBetweenRuns()
        {
            var w = Make(Eight);
            w.Go(1);
            w.SetLayout(FrameLayout.Columns);
            w.Add();
            w.Go(3);
            w.Focus(1);
            string kept = w.Capture();
            var again = Make(Eight);
            Assert.IsTrue(again.Restore(kept));
            Assert.AreEqual(kept, again.Capture());
            Assert.AreEqual(3, again.Current);
            Assert.AreEqual(FrameLayout.Columns, again.spaces[1].layout);
            // a panel that is gone since is dropped, and what is not an arrangement changes nothing
            var fewer = Make("voyage", "hold", "trade");
            Assert.IsTrue(fewer.Restore(kept));
            CollectionAssert.AreEqual(new[] { "trade", "voyage" }, fewer.spaces[1].slots);
            CollectionAssert.AreEqual(new[] { "voyage" }, fewer.spaces[2].slots, "a workspace left empty takes the first panel");
            string before = again.Capture();
            Assert.IsFalse(again.Restore("not an arrangement"));
            Assert.IsFalse(again.Restore(""));
            Assert.IsFalse(again.Restore("0;a|9|0|hold;b|0|0|;c|0|0|;d|0|0|"));
            Assert.AreEqual(before, again.Capture());
        }

        [Test]
        public void ThePanelsAreFoundByThemselves()
        {
            var ids = FramePanels.Ids;
            CollectionAssert.Contains(ids, "hold");
            CollectionAssert.Contains(ids, "chronicle");
            CollectionAssert.AllItemsAreUnique(ids);
            var all = FramePanels.All;
            for (int i = 1; i < all.Count; i++)
                Assert.LessOrEqual(all[i - 1].Order, all[i].Order, "the panels stand in their order");
            foreach (var p in all)
                Assert.IsFalse(string.IsNullOrEmpty(p.Title), p.Id + " has no title");
            Assert.AreEqual("Hold", FramePanels.Title("hold"));
            Assert.AreSame(FramePanels.Get("hold"), FramePanels.Get("hold"), "one object of each panel");
        }

        [Test]
        public void TheStylesheetHoldsEveryPalettesColours()
        {
            string uss = File.ReadAllText("Assets/Kapman/Game/UI/Frame.uss");
            foreach (var p in MapPalette.All.Concat(new MapPalette.Palette[] { null }))
            {
                string cls = FrameColours.Class(p);
                var m = Regex.Match(uss, @"\.frame-root\." + Regex.Escape(cls) + @"\s*\{\s*--frame-edge:\s*rgb\((\d+),\s*(\d+),\s*(\d+)\);\s*--frame-accent:\s*rgb\((\d+),\s*(\d+),\s*(\d+)\)");
                Assert.IsTrue(m.Success, "Frame.uss has no class ." + cls + " with --frame-edge and --frame-accent");
                var edge = FrameColours.Edge(p);
                var accent = FrameColours.Accent(p);
                int[] got = Enumerable.Range(1, 6).Select(i => int.Parse(m.Groups[i].Value)).ToArray();
                int[] want = { edge.r, edge.g, edge.b, accent.r, accent.g, accent.b };
                for (int i = 0; i < 6; i++)
                    Assert.LessOrEqual(System.Math.Abs(got[i] - want[i]), 1, $".{cls}: Frame.uss has {string.Join(",", got)}, FrameColours makes {string.Join(",", want)}");
            }
        }

        /// <summary>The user's rule for the world's colours, and the palette mend of 7 Oct (ruling 15): every palette is
        /// eight colours, Byzantium (#702963) is one of them and only one, and no two grounds of a palette share a colour.</summary>
        [Test]
        public void EveryPaletteHoldsByzantiumExactlyOnce()
        {
            Assert.AreEqual(8, MapPalette.All.Length, "eight peoples, eight palettes");
            foreach (var p in MapPalette.All)
            {
                Assert.AreEqual(8, p.colours.Length, p.name + " is three bits: eight colours");
                int byzantium = p.colours.Count(c => c.r == 0x70 && c.g == 0x29 && c.b == 0x63);
                Assert.AreEqual(1, byzantium, $"{p.name} ({p.people}) holds Byzantium #702963 {byzantium} times, not once");
                var seen = p.colours.Select(c => (c.r << 16) | (c.g << 8) | c.b).Distinct().Count();
                Assert.AreEqual(8, seen, $"{p.name} ({p.people}) has two grounds of one colour");
            }
        }

        [Test]
        public void DaysInPlainWords()
        {
            Assert.AreEqual("half a day", FrameUi.Days(0.2));
            Assert.AreEqual("half a day", FrameUi.Days(0.6));
            Assert.AreEqual("1 day", FrameUi.Days(1.1));
            Assert.AreEqual("1 and a half days", FrameUi.Days(1.4));
            Assert.AreEqual("3 days", FrameUi.Days(3.2));
            Assert.AreEqual("1 to 2 days", FrameUi.Days(1.1, 2.2));
            Assert.AreEqual("1 and a half to 3 days", FrameUi.Days(1.6, 3.1));
            Assert.AreEqual("half a day to 1 day", FrameUi.Days(0.3, 1.0));
            Assert.AreEqual("half a day to 2 and a half days", FrameUi.Days(0.5, 2.4));
            Assert.AreEqual("2 days", FrameUi.Days(1.9, 2.1), "a low and a high that come to the same are one figure");
            Assert.AreEqual("today", FrameUi.Ago(0.4));
            Assert.AreEqual("yesterday", FrameUi.Ago(1.5));
            Assert.AreEqual("6 days ago", FrameUi.Ago(6.2));
            Assert.AreEqual("north-west", FrameUi.Airt(315.0));
            Assert.AreEqual("north", FrameUi.Airt(350.0));
            Assert.AreEqual("morning", FrameUi.PartOfDay(9.0));
        }
    }
}
