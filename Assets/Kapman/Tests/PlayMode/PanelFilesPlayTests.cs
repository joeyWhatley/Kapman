using System.Collections;
using System.Linq;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SIDE WINDOWS AS PLAYED ARE THEIR FILES' (14 Oct 2026, KAPMAN_PLAN step 2d). A new game, the Hold, Crew and
    /// Port panels picked for the first window by its title: each window's body holds a copy of its file's "panel", and the panel's lines
    /// stand inside the file's sections, not loose in the body. A picture is taken (Captures/windows).
    /// </summary>
    public class PanelFilesPlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator TheWindowsOnScreenAreTheirFilesOwn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_windows_saves", p => play = p, "windows");
            try
            {
                var frame = FrameRoot.Current;
                Assert.IsNotNull(frame, "no frame");
                foreach (var (id, button, section) in new[] { ("hold", "Hold", "goods"), ("crew", "Crew", "state"), ("port", "Port", "choices") })
                {
                    // (as ChartGameTests does: the window's title opens the list of panels, and the panel is picked there)
                    var title = frame.Root.Q<Button>(className: "frame-win-title");
                    Assert.IsNotNull(title, "no window title to pick a panel by");
                    yield return play.Click(title, "the window's title");
                    yield return ChartPlay.Settle(0.3f);
                    var pick = play.Buttons(button, frame.Root).FirstOrDefault(b => b.ClassListContains("frame-pick"));
                    Assert.IsNotNull(pick, "no panel " + button + " in the window's list");
                    yield return play.Click(pick, button);
                    yield return ChartPlay.Settle(0.5f);
                    var panel = FramePanels.Get(id);
                    Assert.IsNotNull(panel, "no panel " + id);
                    Assert.IsTrue(panel.Shown, "the " + button + " window is not shown after its rack button");
                    var copy = panel.Body.Q<VisualElement>("panel");
                    Assert.IsNotNull(copy, "the " + button + " window's body holds no copy of " + id + ".uxml");
                    Assert.AreSame(panel.Body, copy.parent, "the copy of " + id + ".uxml is not the body's own child");
                    Assert.AreEqual(1, panel.Body.childCount, "the " + button + " window has lines loose in its body beside the file's copy");
                    var filled = copy.Q<VisualElement>(section);
                    Assert.IsNotNull(filled, id + ".uxml has no section \"" + section + "\" on screen");
                    Assert.Greater(filled.childCount, 0, "the section \"" + section + "\" of the " + button + " window is empty");
                    Assert.IsTrue(ChartPlay.Shown(filled.ElementAt(0)), "the first line of \"" + section + "\" in the " + button + " window is not shown");
                    if (id == "hold")
                        yield return play.Shot("windows_0_the_hold_from_its_file");
                }
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
