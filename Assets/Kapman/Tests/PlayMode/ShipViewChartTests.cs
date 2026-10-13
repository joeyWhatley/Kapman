using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.Game.ShipView;
using Kapman.Levels;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE 3D SHIP VIEW IN THE CHART GAME, PLAYED AS A PLAYER PLAYS IT (the user, 7 Oct 2026: "start working on the 3D
    /// view for the ship"; "the map and the 3D sailing boat can be changed focus with a simple button on the ... top
    /// left of the 3D sailing boat that switches place with the map and miniaturises the map"). the Kapman scene is opened in
    /// Play, a new game is begun at Dorestat, and the ship window of the frame is looked at through the real controls
    /// (ChartPlay), with a picture of the whole screen at each step in Captures/ship_view/chart, and the ship window
    /// alone, twice its size, beside each (..__window.png):
    ///   1  moored at Dorestat's jetty            2  cast off, under way on the river        3  out at sea
    ///   4  swapped: she fills the screen and the chart is small in her window, still alive
    ///   5  swapped back                           6  the looks, flipped with the key L
    ///   7  the clock at its fastest: the water does not race; and held by the pause: the sea goes on
    /// Open the pictures after a run: they are what the player sees.
    /// Run: -runTests -testPlatform PlayMode -testFilter ShipViewChartTests   (with graphics: no -nographics)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class ShipViewChartTests
    {
        const string Shots = "../ship_view/chart/";

        [TearDown]
        public void PutBack()
        {
            GameSpeed.Rates[2] = 1440.0;
            ChartPlay.EndActive();
            FrameRoot.FreshLayout = false;
        }

        static IEnumerator Frames(int n) => ChartPlay.Frames(n);

        static string TestSaves()
        {
            string folder = Path.Combine(Application.temporaryCachePath, "kapman_test_saves");
            Directory.CreateDirectory(folder);
            return folder;
        }

        static string Folder()
        {
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view", "chart");
            Directory.CreateDirectory(dir);
            return dir;
        }

        static IEnumerator OpenChart()
        {
            var load = SceneManager.LoadSceneAsync("Kapman", LoadSceneMode.Single);
            Assert.IsNotNull(load, "the Kapman scene is not in the build settings (Kapman > Levels > Build the chart game)");
            while (!load.isDone)
                yield return null;
            yield return Frames(5);
        }

        static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        /// <summary>Answers whatever floats in front as a player in a hurry would: the first choice that can be chosen, or
        /// the cross.</summary>
        static IEnumerator Answer(ChartPlay play)
        {
            var front = play.FrontFloat();
            if (front == null)
                yield break;
            foreach (var c in play.Choices(front))
                if (c.can)
                {
                    yield return play.Click(c.button, "the choice '" + c.words + "'");
                    yield break;
                }
            var cross = front.Q<Button>(className: "frame-float-shut");
            if (cross != null)
                yield return play.Click(cross, "the cross of a floating window");
        }

        static IEnumerator AnswerAll(ChartPlay play, int most = 12)
        {
            for (int i = 0; i < most && play.FrontFloat() != null; i++)
            {
                yield return Answer(play);
                yield return Frames(3);
            }
        }

        /// <summary>The chart open with no game begun yet: at the title (a run of its own), or, when a test before this
        /// one left a game going, at the creation window of a new one (the state cleared, the scene loaded again). To be
        /// called BEFORE the screen is taken (ChartPlay.Begin): a new scene has a new screen.</summary>
        public static IEnumerator ChartWithNoGame()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (the clock is not dropped back under this test: it had been left so by whichever test ran before)
            SaveGame.FolderOverride = TestSaves();
            ShipStageHook.TurnOn();                                       // (a test before may have left it off; these are about the view)
            if (SceneManager.GetActiveScene().name != "Kapman")
                yield return OpenChart();
            Assert.IsNotNull(MainMenu.Current, "no main menu in the Kapman scene");
            if (MainMenu.Current.OnTitle)
                yield break;
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = TestSaves();                        // (a new game forgets the folder a test named: named again)
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (the clock is not dropped back under this test: it had been left so by whichever test ran before)
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            Assert.IsTrue(FrameRoot.Current != null && FrameRoot.Current.AskingSelf, "a new game did not ask who the player is");
        }

        /// <summary>A new game, begun as the first of each choice (a Frisian of Dorestat), lying at her jetty.</summary>
        public static IEnumerator NewGameAtDorestat(ChartPlay play)
        {
            var menu = MainMenu.Current;
            Assert.IsNotNull(menu, "no main menu in the Kapman scene");
            if (menu.OnTitle)
                yield return play.ClickButton("1. New game");
            yield return Frames(120);
            var frame = FrameRoot.Current;
            Assert.IsNotNull(frame, "no frame in the Kapman scene");
            if (frame.AskingSelf)
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
            float from = Time.realtimeSinceStartup;
            while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                yield return null;
            Assert.AreEqual("dorestad", FrameGame.PortHere?.id, "a Frisian does not lie at Dorestat");
        }

        /// <summary>The screen as the panel is drawn (ChartPlay draws it into a texture).</summary>
        static RenderTexture Screen() => MapView.Current.GetComponent<UIDocument>().panelSettings.targetTexture;

        /// <summary>A part of the screen (in the panel's own measure, y down), saved so many times its size.</summary>
        static Texture2D Part(Rect r, int times, string name)
        {
            var rt = Screen();
            var root = MapView.Current.GetComponent<UIDocument>().rootVisualElement.panel.visualTree;
            float px = rt.width / root.worldBound.width;
            int x = Mathf.Clamp(Mathf.RoundToInt(r.x * px), 0, rt.width - 2), w = Mathf.Clamp(Mathf.RoundToInt(r.width * px), 1, rt.width - x);
            int h = Mathf.Clamp(Mathf.RoundToInt(r.height * px), 1, rt.height);
            int top = Mathf.Clamp(Mathf.RoundToInt(r.y * px), 0, rt.height - h);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            // (ReadPixels counts its rows from the bottom of the texture)
            tex.ReadPixels(new Rect(x, rt.height - top - h, w, h), 0, 0);
            tex.Apply(false);
            RenderTexture.active = prev;
            if (name != null)
            {
                var large = new Texture2D(w * times, h * times, TextureFormat.RGB24, false);
                var from = tex.GetPixels32();
                var to = new Color32[w * times * h * times];
                for (int y = 0; y < h * times; y++)
                    for (int xx = 0; xx < w * times; xx++)
                        to[y * w * times + xx] = from[(y / times) * w + xx / times];
                large.SetPixels32(to);
                large.Apply(false);
                File.WriteAllBytes(Path.Combine(Folder(), name + ".png"), large.EncodeToPNG());
                Object.Destroy(large);
            }
            return tex;
        }

        /// <summary>What a picture holds, roughly: its mean brightness, how much its pixels differ among themselves, and the
        /// share of them that are the pink of a missing shader.</summary>
        static (float mean, float spread, float pink) Read(Texture2D tex)
        {
            var px = tex.GetPixels32();
            double sum = 0, sq = 0;
            int pink = 0;
            foreach (var c in px)
            {
                float l = (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;
                sum += l;
                sq += l * l;
                if (c.r > 235 && c.b > 235 && c.g < 40)
                    pink++;
            }
            float mean = (float)(sum / px.Length);
            return (mean, Mathf.Sqrt(Mathf.Max(0f, (float)(sq / px.Length) - mean * mean)), pink / (float)px.Length);
        }

        /// <summary>How far two pictures of the same size are apart: the mean difference of their pixels, 0 .. 1.</summary>
        static float Apart(Texture2D a, Texture2D b)
        {
            var pa = a.GetPixels32();
            var pb = b.GetPixels32();
            if (pa.Length != pb.Length)
                return 1f;
            double sum = 0;
            for (int i = 0; i < pa.Length; i += 3)
                sum += (Mathf.Abs(pa[i].r - pb[i].r) + Mathf.Abs(pa[i].g - pb[i].g) + Mathf.Abs(pa[i].b - pb[i].b)) / 765.0;
            return (float)(sum / (pa.Length / 3));
        }

        /// <summary>A picture of the whole screen, the ship window alone beside it at twice its size, and what the window holds.</summary>
        static IEnumerator Shot(ChartPlay play, string name, StringBuilder notes, System.Action<(float mean, float spread, float pink)> seen = null)
        {
            yield return play.Shot(Shots + name);
            var hook = ShipStageHook.Current;
            var window = play.Frame.Root.Q(null, "frame-ship-view");
            var tex = Part(window.LocalToWorld(window.contentRect), 2, name + "__window");
            var holds = Read(tex);
            Object.Destroy(tex);
            var st = hook != null && hook.Stage != null ? hook.Stage.State : default;
            notes.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0}: window mean {1:0.000} spread {2:0.000} pink {3:0.0000}; place {4}, moored {5}, berthing {6}, speed {7:0.00} m/s, oars {8}, lying to {9}, wind {10:0.0} m/s from {11:0}, " +
                "cloud {12:0.00}, rain {13:0.00}, fog {14:0.00}, sees {15:0} m, clock factor {16:0.##}; sea {18:0.00} m, rides HDRP waves {19}; {17}",
                name, holds.mean, holds.spread, holds.pink, st.place, st.moored, st.berthing, st.speed, st.underOars, st.lyingTo, st.windSpeed, st.windFrom,
                st.cloud, st.rain, st.fog, st.visibility, st.timeFactor, LevelClock.Current != null ? LevelClock.Current.Text : "",
                hook != null && hook.Stage != null ? hook.Stage.Sea.State.hs : 0f, hook != null && hook.Stage != null && hook.Stage.Hdrp != null && hook.Stage.Hdrp.Rides));
            seen?.Invoke(holds);
        }

        // =====================================================================================================
        [UnityTest]
        public IEnumerator SheIsSeenInTheShipWindowAndChangesPlacesWithTheChart()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (the clock is not dropped back under this test: it had been left so by whichever test ran before)
            SaveGame.FolderOverride = TestSaves();
            foreach (string old in Directory.GetFiles(SaveGame.FolderOverride))
                File.Delete(old);
            foreach (string old in Directory.GetFiles(Folder(), "*.png"))
                File.Delete(old);
            Assert.IsFalse(ShipStageHook.StartsOff, "the 3D view is switched off in the code (ShipStageHook.StartsOff)");
            yield return ChartWithNoGame();
            var play = ChartPlay.Begin(false);
            var notes = new StringBuilder();
            try
            {
                yield return NewGameAtDorestat(play);
                // (the wall clock: ChartPlay counts every frame a thirtieth of a second for its own pictures, which on a fast
                // machine runs the game many times too fast beside the stage, and the stage keeps the wall clock)
                Time.captureFramerate = 0;
                var frame = FrameRoot.Current;
                var game = KapmanGame.Current;
                var map = MapView.Current;

                // ---- 1. THE WINDOW: the view starts by itself, and she lies at Dorestat's jetty in it
                var hook = ShipStageHook.Current;
                Assert.IsNotNull(hook, "the 3D view did not start in the chart (no ShipStageHook: is Resources/ShipStageKit there?)");
                yield return ChartPlay.Until(() => hook.Stage != null && hook.Stage.View.Drawn > 3, 20f);
                var stage = hook.Stage;
                Assert.IsNotNull(stage, "the hook made no stage");
                Assert.Greater(stage.View.Drawn, 3, "the stage drew no picture");
                yield return ChartPlay.Settle(2.5f);                       // (the stage keeps the wall clock: its waits are real seconds)
                var window = frame.Root.Q(null, "frame-ship-view");
                Assert.IsNotNull(window, "the frame has no ship window");
                var small = hook.SmallPicture;
                Assert.IsTrue(ChartPlay.Shown(small), "the picture is not shown in the ship window");
                Rect inWindow = window.LocalToWorld(window.contentRect), picture = small.worldBound;
                Assert.Less(Mathf.Abs(picture.width - inWindow.width) + Mathf.Abs(picture.height - inWindow.height), 2f, $"the picture ({picture}) does not fill the ship window ({inWindow})");
                var shownTexture = small.image;
                Assert.IsNotNull(shownTexture, "the picture shows no texture");
                float windowShape = inWindow.width / inWindow.height, textureShape = shownTexture.width / (float)shownTexture.height;
                Assert.AreEqual(windowShape, textureShape, windowShape * 0.02f, $"the picture is stretched: the window is {inWindow.width:0} x {inWindow.height:0}, the texture {shownTexture.width} x {shownTexture.height}");
                Assert.IsTrue(stage.State.moored, "she lies at Dorestat on the chart, but the stage does not show her moored");
                Assert.AreEqual(StagePlace.Port, stage.State.place);
                Assert.IsTrue(stage.DockUp, "moored, and no jetty on the stage");
                Assert.IsNotNull(stage.Crew, "no crew aboard");
                // the frame's own caption under the window still reads
                var where = frame.Root.Q<Label>(className: "frame-ship-where");
                Assert.IsTrue(where != null && ChartPlay.Shown(where) && !string.IsNullOrEmpty(where.text), "the caption under the ship window does not read");
                // the Swap button and the Look button are in the top bar, with the buttons that change what is looked at
                var swap = hook.SwapButtonOnShip;
                Assert.IsTrue(ChartPlay.Shown(swap), "no Swap button");
                Assert.IsNotNull(frame.ViewButtons, "the top bar has no place for the view buttons");
                Assert.IsTrue(frame.ViewButtons.Contains(swap), "the Swap button is not in the top bar");
                Assert.IsTrue(hook.LookButton != null && frame.ViewButtons.Contains(hook.LookButton), "the Look button is not in the top bar");
                Assert.Less(swap.worldBound.yMax, inWindow.y, "the Swap button still lies on the ship window");
                Assert.IsTrue(play.Reachable(swap), "the Swap button cannot be clicked");
                // the chart's own camera must not draw the stage's sea (HDRP draws every water surface for a camera that draws nothing)
                var chartCamera = Camera.main;
                Assert.IsNotNull(chartCamera, "the chart has no main camera");
                Assert.IsTrue(chartCamera.TryGetComponent<HDAdditionalCameraData>(out var chartData), "the chart's camera has no HDRP data");
                Assert.IsTrue(chartData.customRenderingSettings && chartData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Water]
                              && !chartData.renderingPathCustomFrameSettings.IsEnabled(FrameSettingsField.Water),
                              "Water is not switched off for the chart's own camera: HDRP would draw the stage's sea over the chart");
                // and the chart's ship must not find the stage's sea (it would ask HDRP for the water's height under her)
                Assert.IsNull(Object.FindAnyObjectByType<WaterSurface>(), "the game's finders can see the stage's water surface");
                (float mean, float spread, float pink) moored = default;
                yield return Shot(play, "1_moored_at_dorestat", notes, h => moored = h);
                Assert.Less(moored.pink, 0.002f, "pink in the ship window: a material without a shader");
                Assert.Greater(moored.spread, 0.02f, "the ship window is one flat colour");
                Debug.Log($"Ship view in the chart: the window is {inWindow.width:0} x {inWindow.height:0} at ({inWindow.x:0}, {inWindow.y:0}); the texture {shownTexture.width} x {shownTexture.height}; " +
                          $"the caption reads '{where.text}'; drawn {stage.View.Drawn} pictures so far");

                // ---- 2. CAST OFF: under way on the river
                Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Lundenwic");
                yield return Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                yield return Frames(10);
                Assert.IsFalse(game.ship.anchored, "she did not cast off");
                yield return ChartPlay.Settle(4f);
                yield return Shot(play, "2a_pushing_off", notes);
                // (a quarter of a minute of the wall clock: the oars come out or the sail is set, the jetty falls astern)
                for (float until = Time.realtimeSinceStartup + 14f; Time.realtimeSinceStartup < until;)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play);
                }
                Assert.IsFalse(stage.State.moored, "cast off on the chart, still moored on the stage");
                Assert.IsTrue(stage.State.UnderWay, "cast off, and the stage shows her with no way on");
                Assert.AreEqual(StagePlace.River, stage.State.place, "below Dorestat she is on the river");
                yield return Shot(play, "2_under_way_on_the_river", notes, h =>
                {
                    Assert.Less(h.pink, 0.002f, "pink in the ship window under way");
                    Assert.Greater(h.spread, 0.02f, "the ship window is one flat colour under way");
                });

                // ---- 7a. THE CLOCK AT ITS FASTEST: the water does not race (the same wave pattern goes by at her own speed)
                GameSpeed.Rates[2] = 2400.0;                                 // the test's hurry: the fastest speed made 40x for this run
                yield return play.Click(Small(play, "3"), "the fastest speed");
                yield return ChartPlay.Settle(1.5f);
                Assert.Greater((float)(GameSpeed.Rate / 60.0), 30f, "the hurry did not take");
                Assert.Greater(stage.State.timeFactor, 30f, "the stage's record does not carry the clock's speed");
                Assert.AreEqual(stage.State.speed, stage.State.Way, 1e-4f, "the water must go by at her own speed whatever the clock's");
                if (stage.Hdrp != null && stage.Hdrp.Surface != null && stage.SeaKind == StageSeaKind.HdrpWater)
                    Assert.AreEqual(1f / Time.timeScale, stage.Hdrp.Surface.timeMultiplier, 1e-4f, "HDRP's water is not held to its own clock at the higher speeds");
                yield return Shot(play, "7_the_clock_at_its_fastest", notes);

                // ---- 3. AT SEA: down the Lek and out by the Maas mouth (the hurry is counted in pictures, not seconds: two
                // days of the game in some seconds)
                Time.captureFramerate = 30;
                bool atSea = false;
                var places = new List<string>();
                for (int i = 0; i < 9000 && !atSea; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play);
                    string place = stage.State.place.ToString();
                    if (places.Count == 0 || places[places.Count - 1] != place)
                        places.Add(place);
                    atSea = stage.State.place == StagePlace.OpenSea && stage.State.UnderWay;
                }
                yield return AnswerAll(play);
                yield return play.Click(Small(play, "1"), "the first speed");
                Time.captureFramerate = 0;                                // (the wall clock again)
                Debug.Log($"Ship view in the chart: the stage's places on the way out: {string.Join(" > ", places)}; {LevelClock.Current.Text}");
                Assert.IsTrue(atSea, "she never came out on the open sea: the stage's places were " + string.Join(" > ", places));
                // (some seconds of the wall clock at 1x: the sea settles to where she is now)
                for (float until = Time.realtimeSinceStartup + 7f; Time.realtimeSinceStartup < until;)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play);
                }
                Assert.IsFalse(stage.DockUp && stage.DockAt.magnitude < 200f, "a jetty beside her on the open sea");
                yield return Shot(play, "3_at_sea_at_the_hour_she_came_out", notes, h => Assert.Less(h.pink, 0.002f, "pink in the ship window at sea"));
                // and on into the forenoon, for a picture of the sea by day
                Time.captureFramerate = 30;
                yield return play.Click(Small(play, "3"), "the fastest speed");
                for (int i = 0; i < 4000; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play);
                    double hour = LevelClock.Current.GameHours % 24.0;
                    if (hour > 9.5 && hour < 15.0 && stage.State.place == StagePlace.OpenSea && stage.State.UnderWay)
                        break;
                }
                yield return AnswerAll(play);
                yield return play.Click(Small(play, "1"), "the first speed");
                Time.captureFramerate = 0;
                GameSpeed.Rates[2] = 1440.0;
                for (float until = Time.realtimeSinceStartup + 9f; Time.realtimeSinceStartup < until;)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play);
                }
                Assert.AreEqual(StagePlace.OpenSea, stage.State.place, "she left the open sea before the forenoon came");
                yield return Shot(play, "3_at_sea", notes, h =>
                {
                    Assert.Less(h.pink, 0.002f, "pink in the ship window at sea");
                    Assert.Greater(h.spread, 0.015f, "the ship window is one flat colour at sea");
                });

                // ---- 7b. HELD BY THE PAUSE: the sea goes on (two pictures a second apart are not the same)
                yield return play.Press(Key.Space);
                yield return ChartPlay.Settle(0.5f);
                Assert.IsTrue(GameSpeed.Paused, "Space did not pause");
                Assert.AreEqual(0f, stage.State.Way, "paused, the water must not go by");
                var before = Part(window.LocalToWorld(window.contentRect), 1, null);
                yield return ChartPlay.Settle(1.5f);
                var after = Part(window.LocalToWorld(window.contentRect), 1, null);
                float moved = Apart(before, after);
                Object.Destroy(before);
                Object.Destroy(after);
                notes.AppendLine(string.Format(CultureInfo.InvariantCulture, "paused: the window changed by {0:0.0000} in a second and a half", moved));
                Assert.Greater(moved, 0.0015f, "with the clock held the picture stands dead still: the sea should go on");
                yield return play.Press(Key.Space);
                yield return ChartPlay.Settle(0.3f);
                Assert.IsFalse(GameSpeed.Paused);

                // ---- 4. THE SWAP: she fills the screen behind the frame and the chart goes small into her window, still alive
                Rect all = frame.Root.worldBound;
                yield return play.Click(hook.SwapButtonOnShip, "the Swap button on the ship window");
                yield return ChartPlay.Settle(1.5f);
                Assert.IsTrue(hook.Swapped && hook.Applied, "the Swap button did not change their places");
                Assert.IsTrue(ChartPlay.Shown(hook.BigPicture), "swapped, the ship does not fill the screen");
                Assert.Less(Mathf.Abs(hook.BigPicture.worldBound.width - all.width) + Mathf.Abs(hook.BigPicture.worldBound.height - all.height), 2f, "swapped, the ship's picture is not the screen's size");
                var mapRoot = map.GetComponent<UIDocument>().rootVisualElement;
                Rect chartNow = mapRoot.worldBound;
                inWindow = window.LocalToWorld(window.contentRect);
                Assert.Less(Mathf.Abs(chartNow.x - inWindow.x) + Mathf.Abs(chartNow.y - inWindow.y) + Mathf.Abs(chartNow.width - inWindow.width) + Mathf.Abs(chartNow.height - inWindow.height), 3f,
                            $"swapped, the chart ({chartNow}) is not in the ship window ({inWindow})");
                var big = hook.BigPicture.image;
                Assert.AreEqual(all.width / all.height, big.width / (float)big.height, 0.03f, $"swapped, the big picture is stretched ({big.width} x {big.height})");
                // the small chart is over the frame there: the pointer at its middle hits the chart, not the ship window
                var hit = play.Panel.Pick(inWindow.center);
                bool onChart = false;
                for (var e = hit; e != null; e = e.hierarchy.parent)
                    onChart |= e == mapRoot;
                Assert.IsTrue(onChart, "swapped, the pointer over the small chart does not reach the chart");
                // still alive: the clock runs, she moves on it, and the wheel zooms it
                double hoursWas = LevelClock.Current.GameHours;
                var whereWas = game.ship.transform.position;
                for (int f = 0; f < 900 && LevelClock.Current.GameHours <= hoursWas; f++)    // (the clock steps a whole game minute at a time)
                    yield return null;
                yield return Frames(30);
                Assert.Greater(LevelClock.Current.GameHours, hoursWas, "swapped, the clock stands");
                Assert.Greater((game.ship.transform.position - whereWas).magnitude, 0.01f, "swapped, she does not move on the chart");
                yield return Shot(play, "4_swapped_she_fills_the_screen", notes);
                play.WheelAt(inWindow.center, 3f);
                yield return Frames(30);
                yield return play.Shot(Shots + "4_swapped_the_small_chart_zoomed_out");
                yield return play.Press(Key.F);
                yield return ChartPlay.Settle(1f);
                // the frame is still the frame: a workspace's button works over her
                yield return play.Press(Key.Digit2);
                yield return Frames(5);
                yield return play.Shot(Shots + "4_swapped_another_workspace");
                yield return play.Press(Key.Digit1);
                yield return Frames(5);

                // ---- 6. THE LOOKS, with the key L (swapped, so each fills the screen), round and back to the first
                var seenLooks = new List<string> { hook.Looks.Style.id };
                for (int i = 1; i < PsxStyles.All.Length; i++)
                {
                    yield return play.Press(Key.L);
                    yield return ChartPlay.Settle(1.2f);
                    Assert.AreEqual(PsxStyles.All[i].id, hook.Looks.Style.id, "the key L did not bring the next look");
                    seenLooks.Add(hook.Looks.Style.id);
                    yield return play.Shot(Shots + "6_look_" + hook.Looks.Style.id + "_filling_the_screen");
                }
                yield return play.Press(Key.L);
                yield return ChartPlay.Settle(1f);
                Assert.AreEqual(PsxStyles.All[0].id, hook.Looks.Style.id, "the key L did not come round to the first look");

                // ---- 5. SWAPPED BACK, by the button on the small chart
                Assert.IsTrue(ChartPlay.Shown(hook.SwapButtonOnChart), "swapped, the small chart has no Swap button");
                yield return play.Click(hook.SwapButtonOnChart, "the Swap button on the small chart");
                yield return ChartPlay.Settle(1.5f);
                Assert.IsFalse(hook.Swapped || hook.Applied, "the second Swap did not change them back");
                Assert.Less(Mathf.Abs(mapRoot.worldBound.width - all.width) + Mathf.Abs(mapRoot.worldBound.height - all.height), 2f, $"swapped back, the chart does not fill the screen ({mapRoot.worldBound})");
                Assert.IsFalse(ChartPlay.Shown(hook.BigPicture), "swapped back, the big picture is still up");
                Assert.IsTrue(ChartPlay.Shown(hook.SmallPicture), "swapped back, the ship window is empty");
                yield return Shot(play, "5_swapped_back", notes);

                // the looks in the small window
                for (int i = 1; i < PsxStyles.All.Length; i++)
                {
                    yield return play.Press(Key.L);
                    yield return ChartPlay.Settle(1.2f);
                    yield return Shot(play, "6_look_" + hook.Looks.Style.id + "_in_the_window", notes, h => Assert.Less(h.pink, 0.002f, "pink in a look"));
                }
                yield return play.Press(Key.L);
                yield return ChartPlay.Settle(0.6f);

                Debug.Log("Ship view in the chart:\n" + notes);
                File.WriteAllText(Path.Combine(Folder(), "chart.txt"), notes.ToString());
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                File.WriteAllText(Path.Combine(Folder(), "chart.txt"), notes.ToString());
                play.End();
                GameSpeed.Rates[2] = 1440.0;
                FrameRoot.FreshLayout = false;
            }
        }

        // =====================================================================================================
        /// <summary>
        /// THE JETTY NEVER FLIES OVER THE WATER (the user, 7 Oct 2026: the view of the ship does not race at high speed).
        /// A new game; she sails from Dorestat to Traiectum with the clock at the test's fastest (40 times), so the chart
        /// has her in the port's reach and made fast within seconds while the stage's water goes by at her own walking
        /// pace. The jetty must not be dragged across the water to her: the mist closes for a moment, and when it opens
        /// she lies at it. Pictures: the mist closed, and made fast at Traiectum.
        /// </summary>
        [UnityTest]
        public IEnumerator ThenSheComesInAtSpeedAndTheMistBringsTheJetty()
        {
            yield return ChartWithNoGame();                               // (a game of its own)
            float from;
            var play = ChartPlay.Begin(false);
            var notes = new StringBuilder();
            try
            {
                yield return NewGameAtDorestat(play);
                Time.captureFramerate = 0;                                // (the wall clock, as a player has it)
                var hook = ShipStageHook.Current;
                Assert.IsNotNull(hook, "the 3D view did not start in the new game");
                yield return ChartPlay.Until(() => hook.Stage != null && hook.Stage.View.Drawn > 3, 20f);
                var stage = hook.Stage;
                Assert.IsNotNull(stage, "the hook made no stage");
                yield return ChartPlay.Settle(1.5f);
                Assert.IsTrue(stage.State.moored && stage.DockUp, "a new game: she should lie at Dorestat's jetty on the stage");
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Traiectum");
                yield return Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                yield return ChartPlay.Settle(1f);
                GameSpeed.Rates[2] = 2400.0;
                yield return play.Click(Small(play, "3"), "the fastest speed");

                // watched all the way: how fast the jetty ever moves over the stage while it can be seen (measured over a
                // quarter of a second at a time; not while the mist is closed, which is when it is put in its place)
                float fastest = 0f, markAt = Time.realtimeSinceStartup, veilMost = 0f;
                Vector3 mark = stage.DockAt;
                bool markUp = stage.DockUp, shotVeil = false, sawBerthing = false, sawGone = false;
                from = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - from < 150f)
                {
                    yield return null;
                    if (play.FrontFloat() != null && !stage.State.moored)
                        yield return Answer(play);
                    sawBerthing |= stage.State.berthing && stage.State.portId == "traiectum";
                    sawGone |= !stage.DockUp;
                    veilMost = Mathf.Max(veilMost, stage.Veil);
                    float now = Time.realtimeSinceStartup;
                    if (stage.Veil > 0.5f || stage.DockUp != markUp || stage.PushingOff)
                    {
                        mark = stage.DockAt;
                        markAt = now;
                        markUp = stage.DockUp;
                    }
                    else if (now - markAt > 0.25f)
                    {
                        if (stage.DockUp)
                            fastest = Mathf.Max(fastest, (stage.DockAt - mark).magnitude / (now - markAt));
                        mark = stage.DockAt;
                        markAt = now;
                    }
                    if (!shotVeil && stage.Veil > 0.93f && stage.State.moored && stage.State.portId == "traiectum")
                    {
                        shotVeil = true;
                        yield return play.Shot(Shots + "8_the_mist_closes_as_she_is_made_fast", 0f);
                    }
                    if (stage.State.moored && stage.State.portId == "traiectum" && stage.DockUp && stage.Veil <= 0.01f
                        && stage.DockAt.magnitude < stage.Ship.Beam * 0.5f + stage.Dock.gap + 1.5f)
                        break;
                }
                notes.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "to Traiectum at 40 times: the jetty's greatest speed over the stage while in sight {0:0.00} m/s (she makes {1:0.00}); the mist closed to {2:0.00}; the jetty of Dorestat was gone before: {3}; berthing seen: {4}; took {5:0} s; {6}",
                    fastest, stage.State.speed, veilMost, sawGone, sawBerthing, Time.realtimeSinceStartup - from, LevelClock.Current.Text));
                Debug.Log("Ship view in the chart: " + notes);
                Assert.IsTrue(stage.State.moored && stage.State.portId == "traiectum", "she did not come in to Traiectum: " + notes);
                Assert.IsTrue(stage.DockUp && stage.DockAt.magnitude < stage.Ship.Beam * 0.5f + stage.Dock.gap + 1.5f, "made fast at Traiectum, and the jetty is not at her side");
                Assert.Less(fastest, 6f, "the jetty flew over the water");
                Assert.IsTrue(shotVeil, "the jetty was not brought behind the mist (the clock ran at 40 times: it cannot have closed by itself)");
                yield return AnswerAll(play);
                yield return play.Click(Small(play, "1"), "the first speed");
                GameSpeed.Rates[2] = 1440.0;
                yield return ChartPlay.Settle(3f);
                yield return Shot(play, "8_made_fast_at_traiectum", notes, h => Assert.Less(h.pink, 0.002f, "pink in the ship window at Traiectum"));
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                File.AppendAllText(Path.Combine(Folder(), "chart.txt"), notes.ToString());
                play.End();
                GameSpeed.Rates[2] = 1440.0;
                FrameRoot.FreshLayout = false;
            }
        }
    }
}
