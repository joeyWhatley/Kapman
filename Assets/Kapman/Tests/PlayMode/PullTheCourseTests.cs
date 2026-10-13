using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE COURSE PULLED WITH THE MOUSE, ON THE CHART AS A PLAYER HAS IT (the user, 13 Oct 2026: "you should want to be
    /// able to drag and correct the course on the map with your mouse"; by the picker: pull the line anywhere). A new
    /// game at Dorestat, a course set for Witla. The pointer presses on her line a few km down the river and pulls it
    /// aside: a knot stands where it was let go (on the water nearest it), the course runs through it and on to Witla,
    /// the log says so in its short form, and the graph ran it, not the code. The knot is pulled again and moves.
    /// Pictures in Captures/pull (pull_*.png).
    /// </summary>
    public class PullTheCourseTests
    {
        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        [UnityTest, Timeout(600000)]
        public IEnumerator TheCourseIsPulledThroughAKnotWithTheMouse()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_pull_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings");
            while (!load.isDone)
                yield return null;
            yield return ChartPlay.Frames(5);
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = saves;
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var play = ChartPlay.Begin(false, "pull");
            var heard = new List<string>();
            System.Action<string> hear = words => heard.Add(words);
            Voyage.Notice += hear;
            try
            {
                yield return ChartPlay.Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                yield return ChartPlay.Frames(30);
                var view = PortView.Current;
                if (view != null && view.Shown)
                {
                    yield return play.Click(view.Flip, "Chart");
                    yield return ChartPlay.Frames(6);
                }
                var pause = Small(play, "Pause");
                if (pause != null)
                    yield return play.Click(pause, "Pause");
                yield return ChartPlay.Settle(1.0f);
                var map = MapView.Current;
                var frame = play.Frame;
                Assert.IsNotNull(map, "no chart");

                // ---- a course for Witla, set as the Voyage window sets it
                Assert.IsTrue(Course.SetTo("witla", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Witla");
                Assert.AreEqual(0, Course.Knots.Count);
                int before = Course.Points.Count;
                double direct = Course.Km;
                yield return ChartPlay.Settle(0.6f);

                // ---- a point of her line four km down the river, in water she knows
                double along = 0.0;
                int at = 1;
                for (; at + 1 < Course.Points.Count && along < 4000.0; at++)
                    along += WorldXZ.Distance(Course.Points[at - 1], Course.Points[at]);
                var on = Course.Points[at];
                Exploration.Reveal(on.x, on.z, 6000.0);
                yield return ChartPlay.Settle(0.6f);
                Assert.Greater(Exploration.Seen(on.x, on.z), 0.35f, "the point of the line to press on is in the dark");
                var hole = frame.Root.Q<VisualElement>("viewport").worldBound;
                Vector2 Px(WorldXZ w) => map.ChartOverlays.LocalToWorld(map.ChartPx(w));
                Vector2 press = Px(on);
                Assert.IsTrue(hole.Contains(press), "the point of the line to press on is not on the chart: " + press + " in " + hole);
                yield return play.Shot("pull_0_the_line");

                // ---- pulled aside and let go
                heard.Clear();
                int graphs = CoursePulls.ByGraph, codes = CoursePulls.ByCode;
                // (pulled on down the river, which runs west here: a point pulled aside snaps back to the river, and one
                // pulled towards Dorestat lands under the port's own dot, which takes the press)
                Vector2 to = press + new Vector2(-22f, -6f);
                yield return play.Drag(press, to, 8);
                yield return ChartPlay.Settle(0.8f);
                Assert.IsFalse(map.Pulling, "the knot is still being pulled after the hand let go");
                Assert.AreEqual(1, Course.Knots.Count, "the pull left no knot. Said: " + string.Join(" | ", heard));
                Assert.AreEqual(1, Course.KnotAt.Count);
                Assert.AreEqual("witla", Course.Destination, "the pull changed her end");
                Assert.Less(WorldXZ.Distance(Course.Points[Course.KnotAt[0]], Course.Knots[0]), 1.0, "the knot is not a point of the line");
                Assert.AreEqual(graphs + 1, CoursePulls.ByGraph, "the pull was not run by the graph (code " + (CoursePulls.ByCode - codes) + ")");
                Assert.AreEqual(1, map.KnotsShown, "the chart does not show the knot");
                Assert.IsTrue(heard.Any(w => w.Contains("knot")), "the log was not told of the pull: " + string.Join(" | ", heard));
                var knotMark = play.Screen.Q<VisualElement>("map-knot-0");
                Assert.IsNotNull(knotMark, "no mark of the knot on the chart");
                Assert.IsTrue(ChartPlay.Shown(knotMark), "the knot's mark is hidden");
                yield return play.Shot("pull_1_the_knot");
                Debug.Log($"The course pulled on the chart: {Course.Km:0} km against {direct:0} km before, {Course.Points.Count} points against {before}; the knot {WorldXZ.Distance(Course.Knots[0], on) / 1000.0:0.0} km from the point pressed. Said: {string.Join(" | ", heard)}");

                // ---- the knot pulled again: it moves, and the log says so
                var knotAt = Course.Knots[0];
                Vector2 k = knotMark.worldBound.center;
                var picked = play.Panel.Pick(k);
                Debug.Log($"The knot's mark: at {knotMark.worldBound}, {knotMark.childCount} child(ren), {knotMark.resolvedStyle.width}x{knotMark.resolvedStyle.height}, classes {string.Join(" ", knotMark.GetClasses())}; " +
                          $"the knot itself at {Px(Course.Knots[0])}; under the pointer there: {(picked != null ? picked.name + " (" + string.Join(" ", picked.GetClasses()) + ")" : "nothing")}");
                Assert.AreEqual(1, knotMark.childCount, "the knot's mark has no dot: the part file was not taken");
                Assert.Less(Vector2.Distance(k, Px(Course.Knots[0])), 3f, "the knot's mark does not stand on the knot");
                Assert.IsNotNull(picked, "nothing is under the pointer at the knot");
                Assert.AreEqual("map-knot-0", picked.name, "the knot is not what the pointer finds at its mark");
                heard.Clear();
                yield return play.Drag(k, k + new Vector2(-26f, 0f), 8);
                yield return ChartPlay.Settle(0.8f);
                Assert.AreEqual(1, Course.Knots.Count, "a knot pulled again is a knot more or less");
                Assert.Greater(WorldXZ.Distance(Course.Knots[0], knotAt), 200.0, "the knot did not move when pulled again. Said: " + string.Join(" | ", heard));
                Assert.IsTrue(heard.Any(w => w.Contains("moved")), "the log was not told the knot moved: " + string.Join(" | ", heard));
                yield return play.Shot("pull_2_moved");

                // ---- a press on the open chart away from the line is still the chart's own (a click to sail)
                var voyage = FramePanels.Get("voyage") as VoyagePanel;
                Vector2 away = press + new Vector2(0f, 90f);
                if (hole.Contains(away) && voyage != null)
                {
                    play.ClickAt(away);
                    yield return ChartPlay.Settle(0.8f);
                    Assert.AreEqual(1, Course.Knots.Count, "a click away from the line pulled the course");
                    Assert.IsTrue(voyage.PlacePicked, "a click on the open chart away from the line did not point at a place");
                }
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                Voyage.Notice -= hear;
                play.End();
            }
        }
    }
}
