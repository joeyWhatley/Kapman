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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// CLICK AND SAIL, ON THE CHART AS A PLAYER HAS IT (the user, 9 Oct 2026: "We need a more intuitive sailing system for
    /// navigating unknown waters", "right now the path is just set straight to the location and weaves", "Maybe don't even
    /// show dots for unfound locations, needs to be more vague"; by the picker: "Click and sail", "A wide hazy ring").
    /// A new game. Witla is heard of and not found: it has no dot, it is a ring, and the Voyage window lists it under
    /// "By hearsay" with a bearing and no way. The pointer clicks the dark where Witla is said to lie: the Voyage window
    /// shows "the place you pointed at, through waters you do not know", and its button charts the course. Of that
    /// course only the strokes over known water are drawn. She casts off and sails; Witla comes in sight: the log says
    /// so, the clock drops back, its dot stands on the chart and its ring is gone. She sails on to the place and lies
    /// to there, and the clock drops back again. Then Witla, found, is charted by its name like any port.
    /// Pictures in Captures/chart (sail_*.png).
    /// </summary>
    public class SailByPointingTests
    {
        static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        static IEnumerator AnswerAll(ChartPlay play)
        {
            for (int i = 0; i < 12 && play.FrontFloat() != null; i++)
            {
                var front = play.FrontFloat();
                for (int k = 0; k < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; k++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                var pick = play.Choices(front).FirstOrDefault(c => c.can);
                if (pick.button != null)
                    yield return play.Click(pick.button, pick.words);
                else
                {
                    var cross = front.Q<Button>(className: "frame-float-shut");
                    if (cross == null)
                        yield break;
                    yield return play.Click(cross, "the cross");
                }
                yield return ChartPlay.Frames(4);
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator SheSailsWhereThePointerPointsAndFindsAHarbour()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_pointing_saves");
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
            ClockDrops.Quiet = false;                                     // (the clock's own drops are part of what is looked at)
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var play = ChartPlay.Begin(false);
            var dropped = new List<ClockReason>();
            bool quietWas = true;                                        // (the tests after this one have the clock quiet, as they had)
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
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
                yield return play.Click(Small(play, "Pause"), "Pause");
                yield return ChartPlay.Settle(1.0f);
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;
                var voyage = FramePanels.Get("voyage") as VoyagePanel;
                Assert.IsNotNull(voyage, "no Voyage window");
                var hole = frame.Root.Q<VisualElement>("viewport").worldBound;
                Vector2 Px(WorldXZ w) => map.ChartOverlays.LocalToWorld(map.ChartPx(w));

                // ---- Witla is heard of, not found: no dot, a ring, and a line under "By hearsay"
                var witla = GameData.Port("witla");
                var (wx, wz) = WorldFrame.ToWorld(witla.lat, witla.lon);
                var witlaAt = new WorldXZ(wx, wz);
                Assert.IsTrue(Hearsay.Ringed(witla), "Witla should be heard of and not found on the first morning");
                Assert.IsNull(play.Screen.Q<VisualElement>("map-port-witla"), "a port only heard of has a dot");
                Assert.Greater(map.RingsShown, 0, "no rings on the chart");
                yield return play.Click(Verb(play, "Voyage"), "Voyage");
                yield return ChartPlay.Settle(0.8f);
                string reads = ChartPlay.Read(frame.Root);
                StringAssert.Contains("By hearsay", reads, "the Voyage window has no list of the ports only heard of");
                var hearsayLine = play.Line(witla.name, frame.Root);
                Assert.IsNotNull(hearsayLine, "Witla is not listed in the Voyage window");
                string witlaSays = ChartPlay.Read(hearsayLine);
                StringAssert.Contains("west", witlaSays, "the hearsay of Witla does not say which way it lies: " + witlaSays);
                StringAssert.DoesNotContain("km", witlaSays, "the hearsay of Witla gives a distance by water: " + witlaSays);
                Assert.IsFalse(voyage.Reckoned("witla"), "a way is reckoned to a port only heard of");

                // ---- the pointer clicks the dark, where Witla is said to lie (six km short of it, for the test's sake)
                var pointed = new WorldXZ(witlaAt.x + 6000.0, witlaAt.z);
                Vector2 click = Px(pointed);
                Assert.IsTrue(hole.Contains(click), "the place to point at is not on the chart: " + click);
                Assert.AreEqual(0f, Exploration.Seen(pointed.x, pointed.z), "the place to point at is not in the dark");
                play.ClickAt(click);
                yield return ChartPlay.Settle(0.8f);
                Assert.IsTrue(voyage.PlacePicked, "a click on the open chart did not point at a place");
                Assert.IsFalse(voyage.PlaceKnown, "a way through the dark is said to be known");
                reads = ChartPlay.Read(frame.Root);
                StringAssert.Contains("The place you pointed at", reads);
                StringAssert.Contains("Through waters you do not know", reads, "the Voyage window does not say that the way is not known");
                Assert.Greater(frame.PlannedCount, 1, "pointing at a place did not plan a way");
                Assert.IsFalse(Course.IsSet, "pointing at a place set the course before the button was pressed");
                yield return play.Shot("sail_1_pointed_into_the_dark");
                yield return play.ClickWhen(() => play.Button("Set this course", frame.Root), "Set this course");
                yield return ChartPlay.Frames(6);
                Assert.IsTrue(Course.IsSet && Course.ToPlace, "the button did not chart the course to the place pointed at");
                Assert.Less(WorldXZ.Distance(Course.Place, pointed), 400.0, "the course is not to the place the pointer pointed at");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.Whole.StartsWith("Course set for the place you pointed at, through waters you do not know.")),
                              "the log does not say the course into the dark: " + FrameLog.Lines[FrameLog.Lines.Count - 1].Whole);
                Assert.IsTrue(game.ship.anchored, "setting a course in port cast her off");
                yield return ChartPlay.Frames(6);
                var strokes = map.CourseStrokes;
                Assert.Greater(strokes.drawn, 3, "nothing of her course is drawn over the water she knows");
                Assert.Greater(strokes.hidden, strokes.drawn, "her course is drawn through the dark: " + strokes);
                {
                    // on the screen: no stroke of the course stands at its place in the dark
                    var px = play.ScreenPixels(out int width, out int height);
                    int pale = 0, looked = 0;
                    for (int i = Course.Points.Count - 1; i > 0 && looked < 60; i--)
                    {
                        var w = Course.Points[i];
                        Vector2 at = Px(w);
                        if (Exploration.Seen(w.x, w.z) > 0f || !hole.Contains(at) || WorldXZ.Distance(w, Course.Place) < 3000.0)
                            continue;
                        bool inRing = false;                              // (a ring's own pale strokes are not her course's)
                        foreach (var def in GameData.Ports)
                            if (Hearsay.Ringed(def) && System.Math.Abs(WorldXZ.Distance(Hearsay.RingMiddle(def), w) - Hearsay.RingRadius * 0.965) < 3000.0)
                                inRing = true;
                        if (inRing)
                            continue;
                        looked++;
                        var c = px[Mathf.Clamp((int)at.y, 0, height - 1) * width + Mathf.Clamp((int)at.x, 0, width - 1)];
                        if (Mathf.Max(Mathf.Abs(c.r - Exploration.Dark.r), Mathf.Abs(c.g - Exploration.Dark.g), Mathf.Abs(c.b - Exploration.Dark.b)) > 25)
                            pale++;
                    }
                    Assert.Greater(looked, 15, "too little of her course lies in the dark on the chart to be looked at");
                    Assert.LessOrEqual(pale, looked / 10, $"her course shows in the dark: {pale} of {looked} of its points there are not the dark of the fog");
                }
                yield return play.Shot("sail_2_the_course_only_where_she_knows");

                // ---- she casts off and sails: Witla comes in sight, and then she lies to at the place
                int logFrom = FrameLog.Lines.Count;
                yield return play.Click(Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return play.Click(Small(play, "5"), "speed 5");
                double day0 = game.Day;
                float wall = Time.realtimeSinceStartup;
                bool sighted = false, there = false;
                int levelAtSight = -1, levelThere = -1;
                while (!there && game.Day - day0 < 4.0 && Time.realtimeSinceStartup - wall < 300f)
                {
                    if (play.FrontFloat() != null)
                        yield return AnswerAll(play);
                    for (int i = logFrom; i < FrameLog.Lines.Count; i++)
                    {
                        string text = FrameLog.Lines[i].Whole;
                        if (!sighted && text.StartsWith("A harbour in sight") && text.Contains(witla.name))
                        {
                            sighted = true;
                            levelAtSight = GameSpeed.Level;
                            yield return play.Click(Small(play, "Pause"), "Pause");
                            yield return ChartPlay.Settle(0.6f);
                            var dot = play.Screen.Q<VisualElement>("map-port-witla");
                            Assert.IsTrue(dot != null && ChartPlay.Shown(dot), "Witla is in sight and has no dot on the chart");
                            Assert.IsFalse(Hearsay.Ringed(witla), "Witla is found and still has its ring");
                            Assert.IsTrue(Course.ToPlace, "sighting a harbour changed her course");
                            if (hole.Contains(dot.worldBound.center))
                            {
                                play.MoveTo(dot.worldBound.center);
                                yield return ChartPlay.Frames(6);
                            }
                            yield return play.Shot("sail_3_a_harbour_in_sight");
                            play.MoveTo(new Vector2(hole.center.x, 20f));
                        }
                        if (text == VoyagePlanner.PlaceThereWords || text == VoyagePlanner.PlaceShortWords)
                        {
                            there = true;
                            levelThere = GameSpeed.Level;
                        }
                    }
                    logFrom = FrameLog.Lines.Count;
                    if (!there && play.FrontFloat() == null && GameSpeed.Level < 4)
                        yield return play.Click(Small(play, "5"), "speed 5");
                    yield return null;
                }
                Assert.IsTrue(sighted, "she sailed for " + (game.Day - day0).ToString("0.0") + " days towards Witla and no harbour came in sight. The log: " +
                                       string.Join(" / ", FrameLog.Lines.Skip(System.Math.Max(0, FrameLog.Lines.Count - 8)).Select(l => l.Whole)));
                Assert.AreEqual(0, levelAtSight, "the clock did not drop back to speed 1 when a harbour came in sight");
                CollectionAssert.Contains(dropped, ClockReason.Sighted, "the clock gave no reason for dropping back at the sighting");
                Assert.IsTrue(there, "she never came to the place pointed at. The log: " + string.Join(" / ", FrameLog.Lines.Skip(System.Math.Max(0, FrameLog.Lines.Count - 8)).Select(l => l.Whole)));
                Assert.AreEqual(0, levelThere, "the clock did not drop back to speed 1 when she came to the place");
                CollectionAssert.Contains(dropped, ClockReason.AtPlace);
                yield return play.Click(Small(play, "Pause"), "Pause");
                yield return AnswerAll(play);
                yield return ChartPlay.Settle(0.6f);
                Assert.IsFalse(Course.IsSet, "come to the place, she is still bound somewhere");
                Assert.IsTrue(game.ship.anchored, "come to the place, she does not lie to");
                Assert.IsNull(Port.LyingId(), "she was made fast at a port on the way to a place: a port beside her course is passed");
                var her = ChartWorld.ShipWorld();
                Assert.Less(WorldXZ.Distance(her, pointed), 6000.0, "she lies to far from the place pointed at");
                yield return play.Shot("sail_4_lying_to_at_the_place");

                // ---- found, Witla is a port like any other: its dot is clicked, its way is reckoned, its course is charted
                var mark = play.Screen.Q<VisualElement>("map-port-witla");
                Assert.IsTrue(mark != null && ChartPlay.Shown(mark) && play.Reachable(mark), "the pointer cannot rest on Witla's dot");
                yield return play.Click(mark, "Witla's dot on the chart");
                yield return ChartPlay.Until(() => voyage.Reckoned("witla"), 20f);
                Assert.AreEqual("witla", voyage.Picked, "a click on a found port's dot did not pick it");
                Assert.IsTrue(voyage.Reckoned("witla"), "no way is reckoned to Witla now that it is found");
                yield return play.ClickWhen(() => play.Button("Set this course", frame.Root), "Set this course, for Witla");
                Assert.IsTrue(Course.IsSet && !Course.ToPlace && Course.Destination == "witla", "found, Witla cannot be charted by its name");
                yield return play.Shot("sail_5_found_and_charted_by_name");
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                play.End();
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
