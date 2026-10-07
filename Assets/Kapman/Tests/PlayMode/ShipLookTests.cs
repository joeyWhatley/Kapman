using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kapman.Game.ShipView;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE LOOKS OF THE SHIP VIEW, FOR THE USER TO CHOOSE FROM (7 Oct 2026: "create a few different shaders, make it psx
    /// style in a few different ways"). The same two moments (the river by day under oars; the open sea in a gale under
    /// sail) drawn in every look of PsxStyles.All, each saved large as the player would see it with the ship filling a
    /// 1920 x 1080 screen (each picture pixel shown as a square of whole screen pixels), and once more as the frame's
    /// small window shows it (443 x 256 on that screen; saved at twice that so it can be looked at):
    ///   Captures/ship_view/looks/<moment>__<look>.png, <moment>__<look>__window.png, and <moment>__sheet.png with all
    ///   of them side by side. looks.txt has each picture's size, how many colours it holds and how far it is from the others.
    /// What it checks by itself: no picture is pink or flat; every look differs from every other; the 15-bit looks hold
    /// no colour outside 32 levels a channel; the ink look holds four colours.
    /// Run: -runTests -testPlatform PlayMode -testFilter ShipLookTests   (with a graphics device)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class ShipLookTests
    {
        const int ScreenW = 1920, ScreenH = 1080, WindowW = 443, WindowH = 256;

        static ShipStageState Gale()
        {
            var gale = ShipStageState.Fair();
            gale.place = StagePlace.OpenSea;
            gale.fetchKm = 300f;
            gale.depth = 40f;
            gale.latitude = 55.0;
            gale.longitude = 4.0;
            gale.julianDay = ShipStageState.JulianDayOf(793, 260, 14.0);
            gale.hours = 14.0;
            // running before it under sail: the wind on her port quarter
            gale.heading = 120f;
            gale.speed = 3.6f;
            gale.windFrom = 285f;
            gale.windSpeed = 16f;
            gale.gust = 23f;
            gale.cloud = 1f;
            gale.overcast = 0.9f;
            gale.storm = 0.3f;
            gale.rain = 0.5f;
            gale.visibility = 5000f;
            gale.cloudLight = 0.05f;
            return gale;
        }

        static ShipStageState River()
        {
            var oars = ShipViewShots.UnderOars();
            oars.julianDay = ShipStageState.JulianDayOf(793, 172, 13.5);
            oars.hours = 13.5;
            return oars;
        }

        /// <summary>The look's picture as the screen would show it: each of its pixels a square of `k` pixels.</summary>
        static Color32[] Large(Texture2D low, int k, out int w, out int h)
        {
            var src = low.GetPixels32();
            w = low.width * k;
            h = low.height * k;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                int row = (y / k) * low.width;
                for (int x = 0; x < w; x++)
                    px[y * w + x] = src[row + x / k];
            }
            return px;
        }

        static void SavePixels(Color32[] px, int w, int h, string path)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.SetPixels32(px);
            tex.Apply(false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        /// <summary>Cut or padded to a size (a look's picture may hang a pixel over the screen's edge).</summary>
        static Color32[] Fit(Color32[] px, int w, int h, int toW, int toH)
        {
            if (w == toW && h == toH)
                return px;
            var o = new Color32[toW * toH];
            for (int y = 0; y < toH; y++)
                for (int x = 0; x < toW; x++)
                    o[y * toW + x] = px[Mathf.Min(y + (h - toH > 0 ? h - toH : 0), h - 1) * w + Mathf.Min(x, w - 1)];
            return o;
        }

        static (float mean, float spread, float pink, int colours) Measure(Color32[] px)
        {
            double sum = 0.0, sq = 0.0;
            int pink = 0;
            var seen = new HashSet<int>();
            foreach (var c in px)
            {
                double y = (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                sum += y;
                sq += y * y;
                if (c.r > 200 && c.b > 200 && c.g < 90)
                    pink++;
                seen.Add(c.r << 16 | c.g << 8 | c.b);
            }
            double mean = sum / px.Length;
            return ((float)mean, (float)System.Math.Sqrt(System.Math.Max(0.0, sq / px.Length - mean * mean)), pink / (float)px.Length, seen.Count);
        }

        static float Apart(Color32[] a, Color32[] b)
        {
            double sum = 0.0;
            int n = Mathf.Min(a.Length, b.Length);
            for (int i = 0; i < n; i += 7)
                sum += (System.Math.Abs(a[i].r - b[i].r) + System.Math.Abs(a[i].g - b[i].g) + System.Math.Abs(a[i].b - b[i].b)) / 765.0;
            return (float)(sum / (n / 7));
        }

        [UnityTest]
        public IEnumerator TwoMomentsInEveryLook()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = ShipViewShots.Folder("looks");
            var fake = new ShipViewShots.Fake { state = River() };
            var stage = ShipStage.Build(kit, fake);
            var crew = new PackCrewRig(kit.men);
            stage.Crew = crew;
            var look = new PsxStageLook();
            stage.Look = look;
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; picture's own size; screen pixels to one of its; colours in it; mean; spread; pink\n");
            var bad = new List<string>();
            foreach (var (moment, state) in new[] { ("river_under_oars", River()), ("gale_under_sail", Gale()) })
            {
                fake.state = state;
                var large = new List<Color32[]>();
                var names = new List<string>();
                for (int i = 0; i < PsxStyles.All.Length; i++)
                {
                    var s = PsxStyles.All[i];
                    look.SetStyle(i);
                    foreach (bool window in new[] { false, true })
                    {
                        int showW = window ? WindowW : ScreenW, showH = window ? WindowH : ScreenH;
                        stage.Big = !window;
                        look.Show(showW, showH);
                        if (s.lines <= 0)
                            stage.View.Size(showW, showH);                    // ("None": the stage's own picture, a pixel for a pixel)
                        stage.Settle();
                        crew.holdStroke = false;
                        for (int f = 0; f < (window ? 6 : 40); f++)
                        {
                            if (f == 2 && crew.Rig != null && state.underOars)
                            {
                                // the same point of the stroke in every look: the blades in, the men half way through the pull
                                crew.Rig.Phase = 0.2f;
                                crew.holdStroke = true;
                            }
                            yield return null;
                        }
                        if (s.lines <= 0)
                            stage.View.Render(true);
                        var picture = look.Output != null ? (RenderTexture)look.Output : stage.View.Texture;
                        var low = ShipViewShots.Read(picture);
                        int k = look.Output != null ? look.PixelSize : 1;
                        int save = window ? k * 2 : k;                         // (the small window is saved at twice its size on the screen)
                        var px = Large(low, save, out int w, out int h);
                        var own = low.GetPixels32();
                        string name = moment + "__" + s.id + (window ? "__window" : "");
                        SavePixels(px, w, h, Path.Combine(dir, name + ".png"));
                        var (mean, spread, pink, colours) = Measure(own);
                        log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1} x {2}; {3}; {4}; {5:0.000}; {6:0.000}; {7:0.000}",
                            name, low.width, low.height, k, colours, mean, spread, pink));
                        if (pink > 0.01f)
                            bad.Add(name + ": " + (pink * 100f).ToString("0", CultureInfo.InvariantCulture) + "% of it is pink (a shader that did not compile)");
                        if (spread < 0.004f)
                            bad.Add(name + ": the picture is one flat colour");
                        if (!window)
                        {
                            large.Add(Fit(px, w, h, ScreenW, ScreenH));
                            names.Add(s.id);
                            if (s.levels == 32 && s.tones < 2)
                                foreach (var c in own)
                                    if (c.r % 8 != 0 && c.r != 255 || c.g % 8 != 0 && c.g != 255 || c.b % 8 != 0 && c.b != 255)
                                    {
                                        // (32 levels are written as 0, 8.2, 16.5 ... 255 of 255: allow the rounding)
                                        int lr = Mathf.RoundToInt(c.r / 255f * 31f), lg = Mathf.RoundToInt(c.g / 255f * 31f), lb = Mathf.RoundToInt(c.b / 255f * 31f);
                                        if (Mathf.Abs(lr * 255f / 31f - c.r) > 1.01f || Mathf.Abs(lg * 255f / 31f - c.g) > 1.01f || Mathf.Abs(lb * 255f / 31f - c.b) > 1.01f)
                                        {
                                            bad.Add(name + ": a colour outside the console's 32 levels a channel (" + c.r + ", " + c.g + ", " + c.b + ")");
                                            break;
                                        }
                                    }
                            if (s.tones >= 2 && colours > s.tones)
                                bad.Add(name + ": " + colours + " colours where the look has " + s.tones + " tones");
                        }
                        Object.Destroy(low);
                        // and the men at the oars from close abeam in this look (the screen's size): what the look does to
                        // faces, hands, the hull's planks and the water by her side
                        if (!window && state.underOars)
                        {
                            stage.Running = false;
                            var cam = stage.View.Camera.transform;
                            Vector3 wasAt = cam.localPosition;
                            Quaternion wasLook = cam.localRotation;
                            float wasFov = stage.View.Camera.fieldOfView;
                            var turn = Quaternion.Euler(0f, state.heading, 0f);
                            Vector3 eye = turn * new Vector3(5.4f, 1.55f, -0.5f), aim = turn * new Vector3(0f, 0.6f, -0.5f);
                            cam.localPosition = eye;
                            cam.localRotation = Quaternion.LookRotation(aim - eye, Vector3.up);
                            stage.View.Camera.fieldOfView = 38f;
                            look.Settle();
                            look.Draw(stage, 0f);
                            if (s.lines <= 0)
                                stage.View.Render(true);
                            var closeLow = ShipViewShots.Read(look.Output != null ? (RenderTexture)look.Output : stage.View.Texture);
                            var closePx = Large(closeLow, k, out int cw, out int ch);
                            SavePixels(closePx, cw, ch, Path.Combine(dir, moment + "__" + s.id + "__close.png"));
                            Object.Destroy(closeLow);
                            cam.localPosition = wasAt;
                            cam.localRotation = wasLook;
                            stage.View.Camera.fieldOfView = wasFov;
                            stage.Running = true;
                        }
                    }
                }
                ShipViewShots.Sheet(large, ScreenW, ScreenH, 3, Path.Combine(dir, moment + "__sheet.png"));
                log.AppendLine("how far apart the large pictures of " + moment + " are (0: the same; mean difference of a pixel):");
                for (int a = 0; a < large.Count; a++)
                    for (int b = a + 1; b < large.Count; b++)
                    {
                        float apart = Apart(large[a], large[b]);
                        log.AppendLine(string.Format(CultureInfo.InvariantCulture, "  {0} / {1}: {2:0.000}", names[a], names[b], apart));
                        if (apart < 0.012f)
                            bad.Add(moment + ": the looks " + names[a] + " and " + names[b] + " can hardly be told apart (" + apart.ToString("0.000", CultureInfo.InvariantCulture) + ")");
                    }
            }
            File.WriteAllText(Path.Combine(dir, "looks.txt"), log.ToString());
            Time.captureFramerate = 0;
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
            Assert.IsEmpty(bad, string.Join("; ", bad));
        }

        /// <summary>
        /// EVERY LOOK AT OTHER HOURS: the coast at dusk under sail, a fog bank, a night with a moon, moored at the jetty
        /// (the stage's own four states, ShipStageTests.Cases). The console's ship is lit by numbers worked out here and
        /// not by HDRP, so this is where a look that is too bright or too dark against HDRP's sky would show.
        /// Captures/ship_view/looks/hours__<look>.png has the four of each look on one sheet (dusk, fog / night, jetty),
        /// hours_<moment>__<look>.png each alone, and hours.txt each picture's mean brightness beside the plain look's.
        /// </summary>
        [UnityTest]
        public IEnumerator EveryLookAtDuskInFogAtNightAndAtTheJetty()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = ShipViewShots.Folder("looks");
            var moments = new List<(string name, ShipStageState state)>();
            foreach (var (name, state) in ShipStageTests.Cases())
                if (name.StartsWith("2_") || name.StartsWith("4_") || name.StartsWith("5_") || name.StartsWith("6_"))
                    moments.Add((name.Substring(2), state));
            var fake = new ShipViewShots.Fake { state = moments[0].state };
            var stage = ShipStage.Build(kit, fake);
            stage.Crew = new PackCrewRig(kit.men);
            var look = new PsxStageLook();
            stage.Look = look;
            stage.Big = true;
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; mean brightness; the plain look's; colours in it\n");
            var plain = new Dictionary<string, float>();
            var bad = new List<string>();
            for (int i = 0; i < PsxStyles.All.Length; i++)
            {
                var s = PsxStyles.All[i];
                look.SetStyle(i);
                look.Show(ScreenW, ScreenH);
                if (s.lines <= 0)
                    stage.View.Size(ScreenW, ScreenH);
                var sheet = new List<Color32[]>();
                foreach (var (moment, state) in moments)
                {
                    fake.state = state;
                    stage.Settle();
                    for (int f = 0; f < 45; f++)
                        yield return null;
                    if (s.lines <= 0)
                        stage.View.Render(true);
                    var picture = look.Output != null ? (RenderTexture)look.Output : stage.View.Texture;
                    var low = ShipViewShots.Read(picture);
                    var px = Large(low, look.Output != null ? look.PixelSize : 1, out int w, out int h);
                    var (mean, spread, pink, colours) = Measure(low.GetPixels32());
                    Object.Destroy(low);
                    string name = "hours_" + moment + "__" + s.id;
                    SavePixels(px, w, h, Path.Combine(dir, name + ".png"));
                    sheet.Add(Fit(px, w, h, ScreenW, ScreenH));
                    if (s.lines <= 0)
                        plain[moment] = mean;
                    log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.000}; {2:0.000}; {3}", name, mean, plain.TryGetValue(moment, out float p) ? p : 0f, colours));
                    if (pink > 0.01f)
                        bad.Add(name + ": pink");
                    if (spread < 0.004f)
                        bad.Add(name + ": the picture is one flat colour");
                }
                ShipViewShots.Sheet(sheet, ScreenW, ScreenH, 2, Path.Combine(dir, "hours__" + s.id + ".png"));
            }
            File.WriteAllText(Path.Combine(dir, "hours.txt"), log.ToString());
            Time.captureFramerate = 0;
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
            Assert.IsEmpty(bad, string.Join("; ", bad));
        }

        /// <summary>The player's key goes round every look and back to the first; taking the look off leaves the stage as it was.</summary>
        [UnityTest]
        public IEnumerator TheLooksGoRoundAndComeOffClean()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            var stage = ShipStage.Build(kit, new ShipViewShots.Fake { state = River() });
            stage.Crew = new PackCrewRig(kit.men);
            stage.View.Size(480, 270);
            for (int f = 0; f < 5; f++)
                yield return null;
            var seaBefore = stage.SeaKind;
            float farBefore = stage.View.Camera.farClipPlane;
            var before = new Dictionary<Renderer, Material>();
            foreach (var r in stage.GetComponentsInChildren<Renderer>(true))
                before[r] = r.sharedMaterial;
            var look = new PsxStageLook();
            stage.Look = look;
            var seen = new HashSet<string>();
            string said = null;
            look.Changed += s => said = s.name;
            for (int i = 0; i < PsxStyles.All.Length; i++)
            {
                Assert.IsTrue(seen.Add(look.Name), "two looks are called " + look.Name);
                for (int f = 0; f < 4; f++)
                    yield return null;
                var s = look.Style;
                Assert.AreEqual(s.lines > 0, look.Output != null, s.id + ": a low picture of its own");
                Assert.AreEqual(s.sea == PsxSea.Grid, look.Grid.Object.activeSelf, s.id + ": the console's sea is drawn");
                if (s.sea == PsxSea.Grid)
                    Assert.IsNotNull(stage.HeightAt, s.id + ": she floats on the console's sea");
                Assert.AreSame(stage.View.Picture, s.lines > 0 ? look.Output : stage.View.Texture, s.id + ": the frame is given the look's picture");
                look.Next();
                Assert.AreEqual(look.Name, said, "the change of look is told (the word on the view)");
            }
            Assert.AreEqual(PsxStyles.All[0].name, look.Name, "round and back to the first");

            // what a picture costs THE PROCESSOR in each look: the stage's whole step (the sea, the crew, the grid, and
            // handing HDRP its camera) with a picture drawn every step, in the small window's size. Not the graphics
            // card's own time, which this cannot see: Captures/ship_view/looks/cost.txt
            var cost = new StringBuilder("look; processor ms a step with a picture drawn (mean of 40; the small window, 443 x 256); the graphics card's time is not in it\n");
            stage.Running = false;
            int everyWas = stage.View.every;
            stage.View.every = 1;
            var watch = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < PsxStyles.All.Length; i++)
            {
                look.SetStyle(i);
                look.Show(WindowW, WindowH);
                if (look.Style.lines <= 0)
                    stage.View.Size(WindowW, WindowH);
                for (int f = 0; f < 6; f++)
                {
                    look.Settle();
                    stage.Step(1f / 60f);
                }
                yield return null;
                watch.Restart();
                for (int f = 0; f < 40; f++)
                {
                    look.Settle();
                    stage.Step(1f / 60f);
                }
                watch.Stop();
                cost.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.00}", look.Style.id, watch.Elapsed.TotalMilliseconds / 40.0));
            }
            File.WriteAllText(Path.Combine(ShipViewShots.Folder("looks"), "cost.txt"), cost.ToString());
            stage.View.every = everyWas;
            stage.Running = true;

            look.SetStyle("console");
            for (int f = 0; f < 4; f++)
                yield return null;
            stage.Look = null;
            yield return null;
            Assert.AreEqual(seaBefore, stage.SeaKind, "the sea is the stage's own again");
            Assert.AreEqual(farBefore, stage.View.Camera.farClipPlane, 0.01f);
            Assert.IsNull(stage.HeightAt);
            Assert.IsFalse(stage.View.drawnByLook);
            foreach (var pair in before)
                if (pair.Key != null)
                    Assert.AreSame(pair.Value, pair.Key.sharedMaterial, pair.Key.name + " kept a look's material");
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }
    }
}
