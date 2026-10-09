using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// THE SEA MOVES HER, ON THE CHART AS A PLAYER HAS IT (the user, 9 Oct 2026: "We need an intelligent realistic
    /// current and weather system that can blow and push the ship off course and rough ocean areas"; by the picker:
    /// "Always adrift" and "Strained and held off").
    /// A new game; she is put on the open water of the way from Dorestat to Lundenwic with a sea the test has made
    /// steady (0.6 m/s to the north, in place of the tide and the wind of the day), and sails. She is carried off her
    /// line as the hours pass; the log says so and the clock drops back; a course laid again from where she is sets
    /// her right. Pictures and the hourly track in Captures/sea.
    /// </summary>
    public class SeaMovesHerTests
    {
        internal static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        internal static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        internal static IEnumerator AnswerAll(ChartPlay play)
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

        /// <summary>A new game on the chart, the first questions answered, the chart shown, the clock paused.</summary>
        internal static IEnumerator NewGame(string savesName, System.Action<ChartPlay> got, string pictures = "sea")
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, savesName);
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
            var play = ChartPlay.Begin(false, pictures);
            got(play);
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
        }

        /// <summary>The longest run of open water on a line, as the indices of its first and last points.</summary>
        internal static (int from, int to) LongestOpenRun(List<WorldXZ> line, List<byte> kinds)
        {
            int bestFrom = -1, bestTo = -1;
            double best = 0.0;
            for (int i = 1; i < line.Count; i++)
            {
                if (kinds[i] != LaneGraph.Sea && kinds[i] != LaneGraph.Coastal)
                    continue;
                int j = i;
                double m = 0.0;
                while (j < line.Count - 1 && (kinds[j] == LaneGraph.Sea || kinds[j] == LaneGraph.Coastal))
                {
                    m += WorldXZ.Distance(line[j - 1], line[j]);
                    j++;
                }
                if (m > best)
                {
                    best = m;
                    bestFrom = i - 1;
                    bestTo = j - 1;
                }
                i = j;
            }
            return (bestFrom, bestTo);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator SheIsSetOffHerLineAndPointingAgainSetsHerRight()
        {
            ChartPlay play = null;
            yield return NewGame("kapman_sea_saves", p => play = p);
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;                                     // (the clock's own drops are part of what is looked at)
            var dropped = new List<ClockReason>();
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
            var track = new StringBuilder();
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;

                // ---- the way to Lundenwic as the lanes have it, and a place well out on its open water
                var line = new List<WorldXZ>();
                var depths = new List<float>();
                var kinds = new List<byte>();
                Assert.IsTrue(Course.Plan("lundenwic", ChartWorld.ShipWorld(), Port.LyingId(), line, depths, kinds, out double km), "no way from Dorestat to Lundenwic");
                var (runFrom, runTo) = LongestOpenRun(line, kinds);
                Assert.Greater(runFrom, 0, "the way to Lundenwic has no open water");
                double runM = 0.0;
                int startAt = runFrom;
                for (int i = runFrom + 1; i <= runTo; i++)
                {
                    runM += WorldXZ.Distance(line[i - 1], line[i]);
                    if (startAt == runFrom && runM > 15000.0)
                        startAt = i;
                }
                Assert.Greater(runM, 80000.0, "the open water on the way to Lundenwic is short: " + runM);
                var start = line[startAt];
                track.AppendLine($"the way to Lundenwic: {km:0} km, {line.Count} points; its longest open run {runM / 1000.0:0} km, points {runFrom}..{runTo}; she is put at point {startAt}");

                // ---- she casts off (for Traiectum, the one port she knows), and is put out there, bound for Lundenwic
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return AnswerAll(play);
                var u = FloatingOrigin.ToUnity(new WorldPos(start.x, 0.0, start.z));
                game.ship.Teleport(new Vector3(u.x, 0f, u.z), 270f);
                game.ship.anchored = false;
                for (int i = startAt; i <= runTo; i += 3)
                    Exploration.See(line[i].x, line[i].z, 45000.0);          // (the water is known to her: the pictures show her course)
                Exploration.Apply();
                Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), null), "no course to Lundenwic from the open water");
                Assert.IsTrue(Course.LegIsOpen(1), "the first leg of a course laid on open water is not open water");
                SeaPush.Fixed = at => (0.0, 0.6);                            // (a steady sea: 0.6 m/s to the north)
                Adrift.Forget();
                yield return ChartPlay.Frames(6);
                map.Follow();
                yield return ChartPlay.Settle(0.6f);
                yield return play.Shot("sea_1_on_her_line");

                // ---- she sails: carried north of her line hour by hour, until the log says so and the clock drops
                int logFrom = FrameLog.Lines.Count;
                yield return play.Click(Small(play, "5"), "speed 5");
                double hour0 = game.Hours, lastHour = -1.0;
                float wall = Time.realtimeSinceStartup;
                bool told = false;
                var began = ChartWorld.ShipWorld();
                while (!told && game.Hours - hour0 < 12.0 && Time.realtimeSinceStartup - wall < 240f)
                {
                    if (play.FrontFloat() != null)
                        yield return AnswerAll(play);
                    if (game.Hours - hour0 >= lastHour + 1.0)
                    {
                        lastHour = System.Math.Floor(game.Hours - hour0);
                        var w = ChartWorld.ShipWorld();
                        track.AppendLine($"hour {lastHour:0}: at {w.x:0} {w.z:0}; helm {Adrift.Helm}; set {Adrift.Set.x:0} east {Adrift.Set.z:0} north ({Adrift.SetM / 1000.0:0.0} km); off her line {Adrift.OffLine / 1000.0:0.0} km; " +
                                         $"{ChartBody.Current.Knots:0.0} kn; wind {ChartBody.Current.Wind:0} m/s from {ChartBody.Current.WindFrom:0}; course leg {Course.Next} of {Course.Points.Count - 1}; speed {GameSpeed.Level + 1}");
                    }
                    for (int i = logFrom; i < FrameLog.Lines.Count; i++)
                        if (FrameLog.Lines[i].text.StartsWith("Wind and tide have set her"))
                            told = true;
                    // (the harbours that come in sight drop the clock too: back to speed 5 until she is told of her set)
                    if (!told && play.FrontFloat() == null && GameSpeed.Level < 4)
                    {
                        dropped.Clear();
                        yield return play.Click(Small(play, "5"), "speed 5");
                    }
                    yield return null;
                }
                var off = ChartWorld.ShipWorld();
                track.AppendLine($"told {told} after {game.Hours - hour0:0.0} h: at {off.x:0} {off.z:0}; helm {Adrift.Helm}; set {Adrift.SetM / 1000.0:0.0} km; off her line {Adrift.OffLine / 1000.0:0.0} km; speed {GameSpeed.Level + 1}; drops {string.Join(" ", dropped)}");
                track.AppendLine("the log: " + string.Join(" | ", FrameLog.Lines.Skip(logFrom).Select(l => l.text)));
                File.WriteAllText(Path.Combine(play.dir, "sea_track.txt"), track.ToString());
                Assert.IsTrue(told, "twelve hours in a sea setting north and the log never said she was off her line:\n" + track);
                Assert.Greater(Adrift.Set.z, 6000.0, "she was not carried north:\n" + track);
                Assert.Less(System.Math.Abs(Adrift.Set.x), 2500.0, "she was carried east or west by a sea setting north:\n" + track);
                Assert.Greater(Adrift.OffLine, 6000.0, "she is not off her line:\n" + track);
                Assert.Contains(ClockReason.OffLine, dropped, "the clock did not drop back when she was set off her line");
                // (WHAT THE SEA TELLS HER IS A FLOW: it was the graph that told the log and dropped the clock, not the code)
                Assert.Greater(SeaMoments.ByGraph, 0, "the graph \"Voyage - the sea moves her\" ran none of the sea's moments");
                Assert.AreEqual(0, SeaMoments.ByCode, "a moment of the sea was run by the code: no graph of the game's scene listened for it " +
                    "(the Script Machine on '--- GAME ---'; Kapman > Make physical > 3f)");
                Assert.AreEqual(0, GameSpeed.Level, "the clock did not drop to speed 1");
                Assert.IsTrue(Course.IsWater(off.x, off.z), "she was carried onto the land");
                yield return play.Click(Small(play, "Pause"), "Pause");
                map.Follow();
                yield return ChartPlay.Settle(0.6f);
                // on the chart: the line of her set from where the steersman reckons her, and the arrow of what carries her
                Assert.IsTrue(map.SetShown, "the chart does not draw how far she is from where the steersman reckons her");
                Assert.IsTrue(map.PushShown, "the chart does not draw what carries her");
                // in the Voyage window: off her line, and the course laid again from where she is
                yield return play.Click(Verb(play, "Voyage"), "Voyage");
                yield return ChartPlay.Settle(0.8f);
                string reads = ChartPlay.Read(frame.Root);
                StringAssert.Contains("Off her line", reads, "the Voyage window does not say she is off her line");
                StringAssert.Contains("to the north", reads, "the Voyage window does not say which way she was set");
                yield return play.Shot("sea_2_set_off_her_line");
                // and the Wind and tide window says what carries her and how the steersman knows her place
                var sky = FramePanels.Get("sky");
                Assert.IsNotNull(sky, "no Wind and tide window");
                frame.Show("sky");
                yield return ChartPlay.Settle(0.8f);
                reads = ChartPlay.Read(frame.Root);
                StringAssert.Contains("The steersman", reads, "the Wind and tide window does not say how the steersman knows her place");
                StringAssert.Contains("By reckoning alone", reads);
                StringAssert.Contains("Off her line", reads);
                yield return play.Shot("sea_2b_wind_and_tide");
                yield return play.Click(Verb(play, "Voyage"), "Voyage");
                yield return ChartPlay.Settle(0.6f);

                // ---- a course laid again from where she is sets her right
                int laidFrom = FrameLog.Lines.Count;
                yield return play.ClickWhen(() => play.Button("Lay the course again", frame.Root), "Lay the course again");
                yield return ChartPlay.Frames(6);
                Assert.IsTrue(FrameLog.Lines.Skip(laidFrom).Any(l => l.text.StartsWith("The course is laid again from where she is")),
                              "the log does not say the course was laid again: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.IsTrue(Course.IsSet && Course.Destination == "lundenwic", "laying the course again lost where she was bound");
                Assert.Less(Adrift.SetM, 1.0, "a course laid from where she is leaves her set");
                Assert.Less(WorldXZ.Distance(Course.Points[0], off), 200.0, "the course laid again does not start where she is");
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("sea_3_laid_again_from_where_she_is");
                track.AppendLine($"laid again: {Course.Points.Count} points, {Course.Km:0} km; the first legs: " +
                                 string.Join(" ", Enumerable.Range(1, System.Math.Min(6, Course.Points.Count - 1)).Select(i => $"{Course.LegKind(i)}:{WorldXZ.Distance(Course.Points[i - 1], Course.Points[i]) / 1000.0:0.0}km")));
                File.WriteAllText(Path.Combine(play.dir, "sea_track.txt"), track.ToString());
            }
            finally
            {
                SeaPush.Fixed = null;
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (track.Length > 0 && play != null)
                    File.WriteAllText(Path.Combine(play.dir, "sea_track.txt"), track.ToString());
                play?.End();
            }
        }
    }
}
