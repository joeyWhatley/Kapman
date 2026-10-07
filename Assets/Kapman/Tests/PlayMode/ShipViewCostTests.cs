using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.Game.ShipView;
using Kapman.Levels;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// WHAT THE 3D SHIP VIEW COSTS IN THE CHART GAME (explicit: run with -testFilter Kapman.Tests.ShipViewCostTests, with
    /// graphics). L_Chart in Play, a new game at Dorestat, cast off on the river; the whole screen (the chart and the
    /// frame) drawn into a 1920 x 1080 texture and the chart's own camera into another, as on a real screen. The time
    /// of a frame is measured over a few hundred frames with the view off, on in the small window, and filling the
    /// screen, and for each look.
    /// HOW A FRAME IS TIMED. A run without a window has no screen to wait for, so the processor would run ahead of the
    /// graphics card and the card's share would not be seen. One pixel of the screen's texture is read back at the end
    /// of each frame: that waits until the card has finished everything the frame asked of it. So a frame's time here
    /// is the processor's work AND the card's, one after the other: an upper bound on what a real screen shows (there
    /// they overlap).
    /// Two measures of each case: AS PLAYED (the small window drawn thirty times a second at most, as it ships), and
    /// A PICTURE EVERY FRAME (what one picture costs: the difference to the frame with the view off).
    /// Writes Captures/ship_view/chart/cost.txt.
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class ShipViewCostTests
    {
        [TearDown]
        public void PutBack()
        {
            GameSpeed.Rates[2] = 1440.0;
            ChartPlay.EndActive();
            FrameRoot.FreshLayout = false;
            QualitySettings.vSyncCount = vsyncWas;
            Application.targetFrameRate = rateWas;
        }

        int vsyncWas, rateWas;
        Texture2D one;

        static RenderTexture Screen() => MapView.Current.GetComponent<UIDocument>().panelSettings.targetTexture;

        /// <summary>Waits for the graphics card: one pixel of the screen read back.</summary>
        void WaitForTheCard()
        {
            var rt = Screen();
            if (rt == null)
                return;
            if (one == null)
                one = new Texture2D(1, 1, TextureFormat.RGB24, false);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            one.ReadPixels(new Rect(rt.width - 40, 40, 1, 1), 0, 0);      // (a pixel of the ship window's corner of the screen)
            RenderTexture.active = prev;
        }

        struct Timed
        {
            public float mean, median, worst;
            public int frames, pictures;
            public float seconds;
        }

        /// <summary>The time of a frame over so many frames (after some to settle), each ended by a wait for the card.</summary>
        IEnumerator Measure(int frames, System.Action<Timed> done)
        {
            for (int i = 0; i < 40; i++)
            {
                yield return null;
                WaitForTheCard();
            }
            var stage = ShipStage.Current;
            int drawnWas = stage != null ? stage.View.Drawn : 0;
            var times = new List<float>(frames);
            double last = Time.realtimeSinceStartupAsDouble, from = last;
            for (int i = 0; i < frames; i++)
            {
                yield return null;
                WaitForTheCard();
                double now = Time.realtimeSinceStartupAsDouble;
                times.Add((float)((now - last) * 1000.0));
                last = now;
            }
            float sum = 0f, worst = 0f;
            foreach (float t in times)
            {
                sum += t;
                worst = Mathf.Max(worst, t);
            }
            times.Sort();
            stage = ShipStage.Current;
            done(new Timed
            {
                mean = sum / times.Count, median = times[times.Count / 2], worst = times[(int)(times.Count * 0.99f)], frames = times.Count,
                pictures = stage != null ? stage.View.Drawn - drawnWas : 0, seconds = (float)(last - from),
            });
        }

        static string Line(string what, Timed t, Timed off)
        {
            float perPicture = t.pictures > 0 ? (t.mean - off.mean) * t.frames / t.pictures : 0f;
            return string.Format(CultureInfo.InvariantCulture,
                "  {0,-58} {1,6:0.00} ms a frame (median {2:0.00}, the slowest in a hundred {3:0.00}); {4,4:0} pictures a second; the view {5,5:0.00} ms of it = {6,3:0} %{7}",
                what, t.mean, t.median, t.worst, t.pictures / Mathf.Max(0.001f, t.seconds), t.mean - off.mean, 100f * (t.mean - off.mean) / Mathf.Max(0.001f, t.mean),
                t.pictures > 0 ? string.Format(CultureInfo.InvariantCulture, "; one picture {0:0.00} ms", perPicture) : "");
        }

        [UnityTest, Explicit("a measurement, not a check: run on its own")]
        public IEnumerator WhatTheShipViewCosts()
        {
            vsyncWas = QualitySettings.vSyncCount;
            rateWas = Application.targetFrameRate;
            yield return ShipViewChartTests.ChartWithNoGame();
            var play = ChartPlay.Begin(false);
            RenderTexture chartTarget = null;
            var report = new StringBuilder();
            ShipStageKit kitUsed = null;
            float rateWasKit = -1f;
            int everyWasKit = -1;
            try
            {
                yield return ShipViewChartTests.NewGameAtDorestat(play);
                var hook = ShipStageHook.Current;
                Assert.IsNotNull(hook, "the 3D view did not start in the chart");
                yield return ChartPlay.Until(() => hook.Stage != null && hook.Stage.View.Drawn > 3, 20f);
                var kit = kitUsed = hook.Stage.kit;

                // as on a real screen: the chart's own camera draws too (into a texture of the screen's size), nothing waits
                // for a screen's sync, and time is the wall clock's (ChartPlay holds it to 30 pictures a second for its pictures)
                Time.captureFramerate = 0;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                chartTarget = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                var chartCamera = Camera.main;
                if (chartCamera != null)
                    chartCamera.targetTexture = chartTarget;

                // under way on the river (the crew at work, her wake in the water)
                Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Lundenwic");
                yield return ChartPlay.Frames(5);
                var castOff = play.Buttons("Cast off", play.Frame.Root).Find(b => b.ClassListContains("frame-verb"));
                yield return play.Click(castOff, "the Cast off button");
                float until = Time.realtimeSinceStartup + 12f;
                while (Time.realtimeSinceStartup < until)
                {
                    yield return null;
                    var front = play.FrontFloat();
                    if (front != null)
                        foreach (var c in play.Choices(front))
                            if (c.can)
                            {
                                yield return play.Click(c.button, "a choice");
                                break;
                            }
                }
                var st = hook.Stage.State;
                var screen = Screen();
                report.AppendLine("WHAT THE 3D SHIP VIEW COSTS IN THE CHART GAME");
                report.AppendLine(string.Format(CultureInfo.InvariantCulture, "Measured {0} on: {1}, {2} MB; {3} ({4}, {5} MB); {6}; Unity {7}, in the editor without a window (batch mode).",
                    System.DateTime.Now.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture), SystemInfo.processorType, SystemInfo.systemMemorySize,
                    SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceType, SystemInfo.graphicsMemorySize, SystemInfo.operatingSystem, Application.unityVersion));
                report.AppendLine(string.Format(CultureInfo.InvariantCulture, "The screen {0} x {1}; she is {2}, {3}, at {4:0.0} m/s, wind {5:0.0} m/s; {6}.",
                    screen.width, screen.height, st.place, st.underOars ? "under oars" : st.SailSet ? "under sail" : "lying", st.speed, st.windSpeed, LevelClock.Current.Text));
                report.AppendLine("A frame's time is the processor's work and the graphics card's, one after the other (one pixel is read back each frame to wait for the card): an upper bound on a real screen.");
                report.AppendLine();

                const int N = 400;
                Timed off = default, t = default;

                // ---- the view off: no stage at all
                ShipStageHook.TurnOff();
                yield return ChartPlay.Frames(30);
                Assert.IsNull(ShipStage.Current, "the view did not go off");
                yield return Measure(N, x => off = x);
                report.AppendLine("AS PLAYED (the small window drawn " + kit.smallRate.ToString("0", CultureInfo.InvariantCulture) + " times a second at most, every " + kit.smallEvery + " frames; filling the screen " + kit.bigRate.ToString("0", CultureInfo.InvariantCulture) + " at most):");
                report.AppendLine(Line("the view off (the chart and the frame alone)", off, off));

                // ---- on, in the small window, in each look
                ShipStageHook.TurnOn();
                hook = ShipStageHook.Current;
                Assert.IsNotNull(hook, "the view did not come back on");
                yield return ChartPlay.Until(() => hook.Stage != null && hook.Stage.View.Drawn > 3, 20f);
                yield return ChartPlay.Frames(120);
                var view = hook.Stage.View;
                var sizes = new StringBuilder();
                for (int i = 0; i < PsxStyles.All.Length; i++)
                {
                    while (hook.Looks.Style.id != PsxStyles.All[i].id)
                        hook.Looks.Next();
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    var pic = view.Picture;
                    report.AppendLine(Line("in the small window, look '" + PsxStyles.All[i].name + "' (" + pic.width + " x " + pic.height + ")", t, off));
                }
                while (hook.Looks.Style.id != PsxStyles.All[0].id)
                    hook.Looks.Next();
                yield return ChartPlay.Frames(20);
                // (on this quiet river she floats on the stage's own waves; in a seaway she rides HDRP's own, which HDRP
                // must copy on the processor: the same picture with that on)
                var sea = hook.Stage.Hdrp;
                float rideWas = sea != null ? sea.rideAbove : 0f;
                if (sea != null)
                {
                    sea.rideAbove = 0f;
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    report.AppendLine(Line("in the small window, look 'None', riding HDRP's own waves (a seaway)", t, off));
                    sea.rideAbove = rideWas;
                    yield return ChartPlay.Frames(10);
                }

                // ---- the stage there but standing still (what it costs the chart's own frame by merely being in the scene)
                hook.Stage.Running = false;
                hook.enabled = false;
                yield return ChartPlay.Frames(20);
                yield return Measure(N, x => t = x);
                report.AppendLine(Line("the stage standing still (in the scene, no picture drawn)", t, off));
                hook.enabled = true;
                yield return ChartPlay.Frames(20);

                // ---- filling the screen, in each look
                hook.Swapped = true;
                yield return ChartPlay.Frames(40);
                Assert.IsTrue(hook.Applied, "the swap did not take");
                for (int i = 0; i < PsxStyles.All.Length; i++)
                {
                    while (hook.Looks.Style.id != PsxStyles.All[i].id)
                        hook.Looks.Next();
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    var pic = view.Picture;
                    report.AppendLine(Line("filling the screen, look '" + PsxStyles.All[i].name + "' (" + pic.width + " x " + pic.height + ")", t, off));
                }
                while (hook.Looks.Style.id != PsxStyles.All[0].id)
                    hook.Looks.Next();
                if (sea != null)
                {
                    sea.rideAbove = 0f;
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    report.AppendLine(Line("filling the screen, look 'None', riding HDRP's own waves (a seaway)", t, off));
                    sea.rideAbove = rideWas;
                    yield return ChartPlay.Frames(10);
                }
                // the plain look at other sizes, for comparison (the kit's bigHeight is what ships)
                int bigWas = view.bigHeight;
                foreach (int lines in new[] { 540, 720, screen.height })
                {
                    if (lines == (bigWas > 0 ? bigWas : kit.bigHeight))
                        continue;
                    view.bigHeight = lines;
                    yield return ChartPlay.Frames(40);
                    yield return Measure(N, x => t = x);
                    report.AppendLine(Line("filling the screen, look 'None' at " + lines + " lines (" + view.Picture.width + " x " + view.Picture.height + ")", t, off));
                }
                view.bigHeight = bigWas;
                hook.Swapped = false;
                yield return ChartPlay.Frames(40);

                // ---- A PICTURE EVERY FRAME: what one picture of the small window costs
                report.AppendLine();
                report.AppendLine("A PICTURE EVERY FRAME (no cap: every frame pays for a picture; the difference to the view off is one picture's cost):");
                rateWasKit = kit.smallRate;
                everyWasKit = kit.smallEvery;
                kit.smallRate = 0f;
                kit.smallEvery = 1;
                yield return ChartPlay.Frames(30);
                yield return Measure(N, x => t = x);
                report.AppendLine(Line("in the small window, look 'None' (" + view.Picture.width + " x " + view.Picture.height + ")", t, off));
                int smallWas = view.smallHeight;
                view.smallHeight = 135;
                yield return ChartPlay.Frames(30);
                yield return Measure(N, x => t = x);
                report.AppendLine(Line("the same at 135 lines (" + view.Picture.width + " x " + view.Picture.height + ")", t, off));
                view.smallHeight = smallWas;
                if (sea != null)
                {
                    sea.rideAbove = 0f;
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    report.AppendLine(Line("the same, riding HDRP's own waves (a seaway)", t, off));
                    // as it was first built: the water surface left enabled all the time (HDRP then works it out for the
                    // chart's own camera too, every frame)
                    sea.onlyWhileDrawn = false;
                    sea.Surface.enabled = true;
                    yield return ChartPlay.Frames(30);
                    yield return Measure(N, x => t = x);
                    report.AppendLine(Line("the same, and HDRP's water left on all the time (as first built)", t, off));
                    sea.onlyWhileDrawn = true;
                    sea.Surface.enabled = false;
                    sea.rideAbove = rideWas;
                    yield return ChartPlay.Frames(10);
                }
                hook.Stage.SeaKind = StageSeaKind.Mesh;
                yield return ChartPlay.Frames(30);
                yield return Measure(N, x => t = x);
                report.AppendLine(Line("the same on the stage's own mesh sea (no HDRP water)", t, off));
                hook.Stage.SeaKind = kit.sea;
                kit.smallRate = rateWasKit;
                kit.smallEvery = everyWasKit;
                yield return ChartPlay.Frames(20);

                report.AppendLine();
                report.AppendLine("Read against a screen at 60 pictures a second: a frame there has 16.7 ms.");
                Debug.Log(report.ToString());
            }
            finally
            {
                string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view", "chart");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "cost.txt"), report.ToString());
                if (kitUsed != null && everyWasKit > 0)
                {
                    kitUsed.smallRate = rateWasKit;                       // (the kit is an asset: a test must leave its numbers as they were)
                    kitUsed.smallEvery = everyWasKit;
                }
                if (Camera.main != null)
                    Camera.main.targetTexture = null;
                if (chartTarget != null)
                    chartTarget.Release();
                if (one != null)
                    Object.Destroy(one);
                play.End();
                FrameRoot.FreshLayout = false;
            }
        }
    }
}
