using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kapman.Game.ShipView;
using Kapman.Levels;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE 3D SHIP VIEW, ALONE (the user, 7 Oct 2026: "start working on the 3D view for the ship"). The stage is built
    /// in the test's empty scene from the kit, with a made-up state and no chart game at all, and a picture is saved for
    /// each of seven states into Captures/ship_view/: a river by day, the coast at dusk, the open sea in a gale, a fog
    /// bank, a night with a moon, moored at the jetty, under oars. Open them after a run: they are what the frame's ship
    /// window will show. ship_view.txt beside them says what each was drawn from (the sea's height and period, the Sun's
    /// and the Moon's height, the exposure).
    /// What it checks by itself: no picture is pink (a pack material with no HDRP shader) or one flat colour; the stage
    /// brings none of the 3D levels' singletons, no HDRP water surface, no collider and no main camera into the scene;
    /// its Sun and Moon are the level clock's for the same place; its waves carry the sea model's height.
    /// Needs the kit: Kapman > Ship view > Make the kit.
    /// Run: -runTests -testPlatform PlayMode -testFilter ShipStageTests   (with a graphics device: not -nographics)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class ShipStageTests : IPrebuildSetup
    {
        const int W = 960, H = 540;

        /// <summary>Before the run, in the editor: the kit is made, or filled in where it lacks something (the same as
        /// Kapman > Ship view > Make the kit; found by name, since the tests do not reference the editor assembly).</summary>
        public void Setup()
        {
#if UNITY_EDITOR
            var builder = System.Type.GetType("Kapman.EditorTools.ShipStageKitBuilder, Kapman.Editor");
            // (KAPMAN_SHIP_KIT_ANEW=1: the kit's numbers are taken from the code again, for a run after the code's have changed)
            bool anew = System.Environment.GetEnvironmentVariable("KAPMAN_SHIP_KIT_ANEW") == "1";
            builder?.GetMethod(anew ? "MakeAnew" : "Make")?.Invoke(null, null);
#endif
        }

        sealed class Fake : IShipStageSource
        {
            public ShipStageState state;
            public bool Read(ref ShipStageState s)
            {
                s = state;
                return true;
            }
        }

        static Texture2D Read(RenderTexture rt)
        {
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false);
            RenderTexture.active = prev;
            return tex;
        }

        /// <summary>The picture's mean brightness, how much it varies, and the share of it that is error-shader pink.</summary>
        static (float mean, float spread, float pink) Measure(Texture2D tex)
        {
            var px = tex.GetPixels32();
            double sum = 0.0, sq = 0.0;
            int pink = 0;
            foreach (var c in px)
            {
                double y = (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                sum += y;
                sq += y * y;
                if (c.r > 200 && c.b > 200 && c.g < 90)
                    pink++;
            }
            double mean = sum / px.Length;
            return ((float)mean, (float)System.Math.Sqrt(System.Math.Max(0.0, sq / px.Length - mean * mean)), pink / (float)px.Length);
        }

        /// <summary>A dark hour within five months of midsummer 793 with the fullest, highest Moon (the stage's own sky).</summary>
        static double MoonNight(double lat, double lon)
        {
            double best = ShipStageState.JulianDayOf(793, 172, 24.0);
            float bestScore = -1f;
            for (int day = 172; day < 322; day++)
                for (double hour = 22.0; hour <= 26.0; hour += 1.0)
                {
                    double jd = ShipStageState.JulianDayOf(793, day, hour);
                    var sky = StageSky.At(jd, lat, lon);
                    if (sky.sunAltitude > -10f)
                        continue;
                    float score = sky.moonLit * Mathf.Clamp01(sky.moonAltitude / 30f);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = jd;
                    }
                }
            return best;
        }

        internal static IEnumerable<(string name, ShipStageState state)> Cases()
        {
            var river = ShipStageState.Fair();
            yield return ("1_river_by_day", river);

            var dusk = ShipStageState.Fair();
            dusk.place = StagePlace.Coast;
            dusk.fetchKm = 40f;
            dusk.depth = 15f;
            dusk.latitude = 53.4;
            dusk.longitude = 5.6;
            dusk.julianDay = ShipStageState.JulianDayOf(793, 172, 19.9);
            dusk.hours = 19.9;
            dusk.heading = 20f;
            dusk.speed = 2.8f;
            dusk.windFrom = 250f;
            dusk.windSpeed = 8f;
            dusk.cloud = 0.5f;
            yield return ("2_coast_at_dusk", dusk);

            var gale = ShipStageState.Fair();
            gale.place = StagePlace.OpenSea;
            gale.fetchKm = 300f;
            gale.depth = 40f;
            gale.latitude = 55.0;
            gale.longitude = 4.0;
            gale.julianDay = ShipStageState.JulianDayOf(793, 260, 14.0);
            gale.hours = 14.0;
            gale.heading = 300f;
            gale.speed = 0f;
            gale.lyingTo = true;
            gale.windFrom = 285f;
            gale.windSpeed = 19f;
            gale.gust = 27f;
            gale.cloud = 1f;
            gale.overcast = 0.9f;
            gale.storm = 0.4f;
            gale.rain = 0.7f;
            gale.visibility = 4000f;
            gale.cloudLight = 0.03f;
            yield return ("3_open_sea_in_a_gale", gale);

            var fog = dusk;
            fog.julianDay = ShipStageState.JulianDayOf(793, 172, 7.0);
            fog.hours = 7.0;
            fog.speed = 1.2f;
            fog.windFrom = 200f;
            fog.windSpeed = 2f;
            fog.cloud = 0.6f;
            fog.overcast = 0.5f;
            fog.fog = 0.85f;
            fog.visibility = 120f;
            fog.cloudLight = 0.35f;
            yield return ("4_a_fog_bank", fog);

            var night = gale;
            night.julianDay = MoonNight(night.latitude, night.longitude);
            night.hours = 24.0;
            night.lyingTo = false;
            night.speed = 2.5f;
            night.windFrom = 240f;
            night.windSpeed = 6f;
            night.gust = 8f;
            night.cloud = 0.15f;
            night.overcast = 0f;
            night.storm = 0f;
            night.rain = 0f;
            night.visibility = 30000f;
            night.cloudLight = 0.95f;
            yield return ("5_night_with_a_moon", night);

            var moored = ShipStageState.Fair();
            moored.place = StagePlace.Port;
            moored.moored = true;
            moored.portId = "dorestad";
            moored.speed = 0f;
            moored.heading = 270f;
            moored.windSpeed = 3f;
            moored.fetchKm = 0.8f;
            moored.julianDay = ShipStageState.JulianDayOf(793, 172, 10.0);
            moored.hours = 10.0;
            yield return ("6_moored_at_the_jetty", moored);

            var oars = ShipStageState.Fair();
            oars.underOars = true;
            oars.speed = 1.6f;
            oars.windSpeed = 0.8f;
            oars.gust = 1.2f;
            oars.julianDay = ShipStageState.JulianDayOf(793, 172, 17.5);
            oars.hours = 17.5;
            yield return ("7_under_oars", oars);

            // rowed in to the berth: the jetty is in sight ahead, 22 m off and still a little wide of her
            var coming = moored;
            coming.moored = false;
            coming.berthing = true;
            coming.berthDistance = 22f;
            coming.underOars = true;
            coming.speed = 1.1f;
            yield return ("8_coming_alongside", coming);
        }

        /// <summary>
        /// SIX MOMENTS OF HER UNDER WAY, a third of a second apart, under oars on the river and under sail off the coast
        /// at dusk: Captures/ship_view/moving_oars_0..5.png and moving_sail_0..5.png. A still cannot show which way the
        /// water runs, whether the oars sweep, or whether she shakes on the sea: laid side by side these can.
        /// </summary>
        [UnityTest]
        public IEnumerator SixMomentsUnderWay()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view");
            Directory.CreateDirectory(dir);
            var fake = new Fake { state = ShipStageState.Fair() };
            var stage = ShipStage.Build(kit, fake);
            stage.Crew = new PackCrewRig(kit.men);
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; her height on the water m; pitch deg; roll deg; stroke\n");
            foreach (var (name, state) in Cases())
            {
                string tag = name.StartsWith("7_") ? "oars" : name.StartsWith("2_") ? "sail" : null;
                if (tag == null)
                    continue;
                fake.state = state;
                stage.Settle();
                for (int f = 0; f < 60; f++)
                    yield return null;
                for (int i = 0; i < 6; i++)
                {
                    for (int f = 0; f < 10; f++)
                        yield return null;
                    stage.View.RenderTo(rt);
                    Save(rt, Path.Combine(dir, "moving_" + tag + "_" + i + ".png"));
                    Vector3 e = stage.Ship.Root.localEulerAngles;
                    log.AppendLine(string.Format(CultureInfo.InvariantCulture, "moving_{0}_{1}; {2:0.000}; {3:0.00}; {4:0.00}; {5:0.00}",
                        tag, i, stage.Ship.Root.localPosition.y, Mathf.DeltaAngle(0f, e.x), Mathf.DeltaAngle(0f, e.z), stage.Ship.StrokePhase));
                }
            }
            File.WriteAllText(Path.Combine(dir, "moving.txt"), log.ToString());
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SevenPicturesOfTheStage()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view");
            Directory.CreateDirectory(dir);
            var fake = new Fake { state = ShipStageState.Fair() };
            var stage = ShipStage.Build(kit, fake);
            stage.Crew = new PackCrewRig(kit.men);                            // (the rowing crew: three at the oars, one at the steering oar)
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; sea Hs m; Tp s; whitecaps; sun alt; moon alt; moon lit; mean; spread; pink; model EV; drawn EV; sea drawn; HDRP answers; HDRP wind kmh; band0; band1\n");
            var bad = new List<string>();
            var cases = new List<(string name, ShipStageState state, StageSeaKind sea)>();
            foreach (var (name, state) in Cases())
                cases.Add((name, state, kit.sea));
            // the same two with the stage's own mesh sea (what the PSX looks will draw on), to set beside HDRP's water
            if (stage.Hdrp != null)
                foreach (var (name, state) in Cases())
                    if (name.StartsWith("1_") || name.StartsWith("3_"))
                        cases.Add((name + "_mesh_sea", state, StageSeaKind.Mesh));
            foreach (var (name, state, sea) in cases)
            {
                fake.state = state;
                stage.SeaKind = sea;
                stage.Settle();
                for (int f = 0; f < 45; f++)                                // (HDRP works the sky's light out over some frames; the boat rides a few seas)
                    yield return null;
                stage.View.RenderTo(rt);
                var tex = Read(rt);
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
                var (mean, spread, pink) = Measure(tex);
                Object.Destroy(tex);
                // the small window's own picture too, as the frame shows it
                var smallTex = Read(stage.View.Texture);
                File.WriteAllBytes(Path.Combine(dir, name + "_small.png"), smallTex.EncodeToPNG());
                Object.Destroy(smallTex);
                var sky = stage.SkyNow;
                var fit = stage.Hdrp != null ? stage.Hdrp.Fit : default;
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.00}; {2:0.0}; {3:0.000}; {4:0.0}; {5:0.0}; {6:0.00}; {7:0.000}; {8:0.000}; {9:0.000}; {10:0.0}; {11:0.0}; {12}; {13}; {14:0}; {15:0.00}; {16:0.00}",
                    name, stage.Sea.State.hs, stage.Sea.State.tp, stage.Sea.Whitecaps, sky.sunAltitude, sky.moonAltitude, sky.moonLit, mean, spread, pink,
                    stage.SceneEv, stage.Ev, stage.SeaKind, stage.Hdrp != null && stage.Hdrp.Live, fit.windKmh, fit.band0, fit.band1));
                if (pink > 0.01f)
                    bad.Add(name + ": " + (pink * 100f).ToString("0", CultureInfo.InvariantCulture) + "% of it is pink (a material with no HDRP shader)");
                if (spread < 0.004f)
                    bad.Add(name + ": the picture is one flat colour");
                if (name.StartsWith("6_"))
                    Assert.IsTrue(stage.DockUp && stage.DockAt.magnitude < 3f, "moored, the jetty lies alongside");
            }
            File.WriteAllText(Path.Combine(dir, "ship_view.txt"), log.ToString());
            // and one as it is when the ship and the chart have changed places and the ship fills the screen (the camera
            // stands a little further off, with a wider lens)
            foreach (var (name, state, sea) in cases)
                if (name == "2_coast_at_dusk")
                {
                    fake.state = state;
                    stage.SeaKind = sea;
                    stage.Big = true;
                    stage.Settle();
                    for (int f = 0; f < 45; f++)
                        yield return null;
                    stage.View.RenderTo(rt);
                    Save(rt, Path.Combine(dir, name + "_filling_the_screen.png"));
                    stage.Big = false;
                }
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
            Assert.IsEmpty(bad, string.Join("; ", bad));
        }

        /// <summary>
        /// HER SIZE, FOR THE USER TO CHOOSE (7 Oct 2026: the pack's ship is a longship of 17.9 m; the player's boat is 7.7 m
        /// and worked by four men). The same boat with the same four full-size men aboard, at her true 7.7 m and at the
        /// length the kit shows her at (ShipStageKit.shownLength, the size judged best by eye), each under oars and under
        /// sail, from the game's own view and from close abeam: Captures/ship_view/size_*.png, with size.txt beside them
        /// (her beam, the height of her side and of her thwarts, how many oars). Set KAPMAN_SHIP_SIZES (lengths in metres,
        /// comma separated) to look at others.
        /// </summary>
        [UnityTest]
        public IEnumerator TheBoatAtTwoSizes()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view");
            Directory.CreateDirectory(dir);
            // her true length, and the other: the length the kit shows her at if it is not the true one, else the next size
            // up that four men could still work (9 m), so that there are always two to choose between
            var sizes = new List<float> { 7.7f };
            sizes.Add(kit.shownLength > 0f && Mathf.Abs(kit.shownLength - 7.7f) > 0.05f ? kit.shownLength : AlternativeLength);
            string asked = System.Environment.GetEnvironmentVariable("KAPMAN_SHIP_SIZES");
            if (!string.IsNullOrEmpty(asked))
            {
                sizes.Clear();
                foreach (string part in asked.Split(','))
                    if (float.TryParse(part.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float m) && m > 2f)
                        sizes.Add(m);
            }
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            var log = new StringBuilder("shown length m; scale; beam m; rail over the water m; rail over her boards m; thwart over her boards m; oars; men aboard\n");
            foreach (float size in sizes)
            {
                // (a copy of the kit: the asset itself is not to be changed by a test)
                var own = Object.Instantiate(kit);
                own.shownLength = size;
                var oars = ShipStageState.Fair();
                oars.underOars = true;
                oars.speed = 1.6f;
                oars.windSpeed = 2.5f;
                oars.gust = 3f;
                oars.julianDay = ShipStageState.JulianDayOf(793, 172, 15.5);
                oars.hours = 15.5;
                var fake = new Fake { state = oars };
                var stage = ShipStage.Build(own, fake);
                var crew = new PackCrewRig(own.men);
                stage.Crew = crew;
                stage.View.Size(480, 270);
                string tag = "size_" + size.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', '_') + "m";
                foreach (string what in new[] { "oars", "sail" })
                {
                    if (what == "sail")
                    {
                        var sail = oars;
                        sail.underOars = false;
                        sail.speed = 2.4f;
                        sail.windSpeed = 6f;
                        sail.gust = 8f;
                        fake.state = sail;
                    }
                    stage.Settle();
                    for (int f = 0; f < 50; f++)
                        yield return null;
                    stage.View.RenderTo(rt);
                    Save(rt, Path.Combine(dir, tag + "_" + what + ".png"));
                    // close abeam, a man's height over the water, from her starboard side (the stage stood still for it)
                    stage.Running = false;
                    var cam = stage.View.Camera.transform;
                    Vector3 eye = cam.localPosition, aim = new Vector3(0f, 0.55f, 0f);
                    Quaternion look = cam.localRotation;
                    float fov = stage.View.Camera.fieldOfView;
                    float h = fake.state.heading * Mathf.Deg2Rad;
                    Vector3 fwd = new Vector3(Mathf.Sin(h), 0f, Mathf.Cos(h)), right = new Vector3(fwd.z, 0f, -fwd.x);
                    Vector3 abeam = right * (stage.Ship.Length * 0.62f + 2.2f) - fwd * (stage.Ship.Length * 0.12f) + Vector3.up * 1.9f;
                    cam.localPosition = abeam;
                    cam.localRotation = Quaternion.LookRotation(aim - fwd * 0.4f - abeam, Vector3.up);
                    stage.View.Camera.fieldOfView = 40f;
                    stage.View.RenderTo(rt);
                    Save(rt, Path.Combine(dir, tag + "_" + what + "_abeam.png"));
                    cam.localPosition = eye;
                    cam.localRotation = look;
                    stage.View.Camera.fieldOfView = fov;
                    stage.Running = true;
                }
                int oarCount = 0;
                foreach (var o in stage.Ship.OarsPort) if (o != null) oarCount++;
                foreach (var o in stage.Ship.OarsStarboard) if (o != null) oarCount++;
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:0.0}; {1:0.000}; {2:0.00}; {3:0.00}; {4:0.00}; {5:0.00}; {6}; {7}",
                    stage.Ship.Length, stage.Ship.Scale, stage.Ship.Beam, stage.Ship.Gunwale, stage.Ship.Gunwale - stage.Ship.Deck, stage.Ship.SeatHeight, oarCount, crew.Aboard));
                Assert.AreEqual(4, crew.Aboard, "four men aboard at " + size + " m");
                Object.Destroy(stage.gameObject);
                yield return null;
                Object.Destroy(own);
            }
            File.WriteAllText(Path.Combine(dir, "size.txt"), log.ToString());
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        /// <summary>The camera's four angles, each a picture under oars and under sail (the user, 7 Oct 2026: "try all of them
        /// with different angles, maybe one from almost directly above but angled slightly"). Captures/ship_view/angles.</summary>
        [UnityTest]
        public IEnumerator TheFourAngles()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view", "angles");
            Directory.CreateDirectory(dir);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            var oars = ShipStageState.Fair();
            oars.underOars = true;
            oars.speed = 1.6f;
            oars.windSpeed = 2.5f;
            oars.gust = 3f;
            oars.julianDay = ShipStageState.JulianDayOf(793, 172, 15.5);
            oars.hours = 15.5;
            var fake = new Fake { state = oars };
            var stage = ShipStage.Build(kit, fake);
            stage.Crew = new PackCrewRig(kit.men);
            stage.View.Size(W, H);
            Assert.AreEqual(4, ShipStage.AngleNames.Length);
            for (int a = 0; a < ShipStage.AngleNames.Length; a++)
            {
                foreach (string what in new[] { "oars", "sail" })
                {
                    var now = oars;
                    if (what == "sail")
                    {
                        now.underOars = false;
                        now.speed = 2.4f;
                        now.windSpeed = 6f;
                        now.gust = 8f;
                    }
                    fake.state = now;
                    stage.SetAngle(a);
                    Assert.AreEqual(a, stage.Angle);
                    stage.Settle();
                    for (int f = 0; f < 50; f++)
                        yield return null;
                    stage.View.RenderTo(rt);
                    Save(rt, Path.Combine(dir, "angle_" + (a + 1) + "_" + ShipStage.AngleNames[a].Replace(' ', '_') + "_" + what + ".png"));
                }
            }
            Object.Destroy(stage.gameObject);
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        /// <summary>The second size shown beside her true 7.7 m when the kit shows her at her true size.</summary>
        const float AlternativeLength = 9f;

        static void Save(RenderTexture rt, string path)
        {
            var tex = Read(rt);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator TheStageBringsNothingOfALevel()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            var moored = ShipStageState.Fair();
            moored.moored = true;
            moored.place = StagePlace.Port;
            moored.portId = "dorestad";
            moored.speed = 0f;
            // a camera as the chart's is: it draws no layer at all (HDRP draws water for such a camera whatever the layer)
            var chartCamera = new GameObject("A camera that draws nothing, as the chart's", typeof(Camera)).GetComponent<Camera>();
            chartCamera.cullingMask = 0;
            var stage = ShipStage.Build(kit, new Fake { state = moored });
            stage.View.Size(480, 270);
            if (stage.Hdrp != null)
                stage.Hdrp.rideAbove = 0f;                                // (on a harbour's quiet water she floats on the stage's own waves: here HDRP's own are asked for)
            for (int f = 0; f < 12; f++)
                yield return null;
            if (kit.sea == StageSeaKind.HdrpWater && StageHdrpSea.Supported)
            {
                Assert.IsNotNull(stage.Hdrp, "HDRP's water is the sea");
                Assert.IsNotNull(stage.Hdrp.Surface, "HDRP's water surface");
                stage.Height(0f, 0f);
                Assert.IsTrue(stage.Hdrp.Live, "HDRP answers the height of its waves under her");
                Assert.IsNull(Kapman.Levels.Water.WaterSampler.Surface, "the game's water sampler has found the stage's sea (the chart ship would ask it for depths)");
                var data = chartCamera.GetComponent<HDAdditionalCameraData>();
                Assert.IsTrue(data != null && data.customRenderingSettings && !data.renderingPathCustomFrameSettings.IsEnabled(FrameSettingsField.Water),
                    "a camera that draws no layer has Water off in its frame settings");
            }
            // the chart game reads these statics and finders: with any of them in the scene it takes the chart for a level
            Assert.IsNull(LevelInfo.Current, "a LevelInfo");
            Assert.IsNull(LevelWeather.Current, "a LevelWeather");
            Assert.IsNull(Kapman.Levels.Water.HdrpSea.Current, "an HdrpSea");
            Assert.IsNull(Object.FindAnyObjectByType<WaterSurface>(), "an HDRP water surface (WaterSampler finds any)");
            Assert.IsEmpty(stage.GetComponentsInChildren<Collider>(true), "colliders on the stage (the chart's ship asks Physics what lies under her)");
            Assert.IsEmpty(stage.GetComponentsInChildren<AudioListener>(true), "an audio listener");
            Assert.IsFalse(stage.View.Camera.enabled, "the stage's camera is drawn by request only");
            Assert.AreNotEqual("MainCamera", stage.View.Camera.tag);
            foreach (var t in stage.GetComponentsInChildren<Transform>(true))
                Assert.AreEqual(kit.layer, t.gameObject.layer, t.name + " is off the stage's layer");
            Assert.Greater(stage.View.Drawn, 0, "it drew");
            Object.Destroy(stage.gameObject);
            Object.Destroy(chartCamera.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        [Test]
        public void TheStageSkyIsTheClocksSky()
        {
            double jd = ShipStageState.JulianDayOf(793, 172, 15.0);
            Sky.Compute(Sky.ToUt(jd, 5.0), 52.0, 5.0);
            var mine = StageSky.At(jd, 52.0, 5.0);
            Assert.AreEqual(Sky.Sun.altitude, mine.sunAltitude, 1e-3);
            Assert.AreEqual(Sky.Moon.altitude, mine.moonAltitude, 1e-3);
            Assert.AreEqual(Sky.MoonLit, mine.moonLit, 1e-4);
            Assert.AreEqual(Sky.Sun.east, mine.toSun.x, 1e-4);
            // and it moves with the latitude: the midsummer Sun at three in the afternoon stands lower at 62 N (41.1 against 45.5 degrees)
            Assert.Less(StageSky.At(jd, 62.0, 5.0).sunAltitude, mine.sunAltitude - 3f);
        }

        [Test]
        public void TheWavesCarryTheSeaModelsHeight()
        {
            var sea = new StageSea();
            var s = ShipStageState.Fair();
            s.place = StagePlace.OpenSea;
            s.windSpeed = 12f;
            s.fetchKm = 300f;
            s.depth = 40f;
            sea.Settle();
            sea.Step(s, 0.02f);
            Assert.Greater(sea.State.hs, 1.5f, "a strong breeze over 300 km raises a sea");
            double m0 = 0.0;
            for (int i = 0; i < StageSea.WindWaves; i++)
                m0 += sea.A[i].z * sea.A[i].z * 0.5;
            Assert.AreEqual(sea.State.windSeaHs, 4.0 * System.Math.Sqrt(m0), 0.1 * sea.State.windSeaHs, "the six waves share the significant height's energy");
            // a river's fetch raises a ripple, not a sea
            var river = new StageSea();
            river.Settle();
            river.Step(ShipStageState.Fair(), 0.02f);
            Assert.Less(river.State.hs, 0.25f);
        }
    }
}
