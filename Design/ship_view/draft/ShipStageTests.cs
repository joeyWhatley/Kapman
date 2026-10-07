// PATH IN THE PROJECT: Assets/Kapman/Tests/PlayMode/ShipStageTests.cs   (assembly Kapman.Tests.PlayMode)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Tests.PlayMode.rsp (with the drafts' Kapman.Game); never run.
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
    public class ShipStageTests
    {
        const int W = 960, H = 540;

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

        static IEnumerable<(string name, ShipStageState state)> Cases()
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
        }

        [UnityTest]
        public IEnumerator SevenPicturesOfTheStage()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.Off = true;
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view");
            Directory.CreateDirectory(dir);
            var fake = new Fake { state = ShipStageState.Fair() };
            var stage = ShipStage.Build(kit, fake);
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; sea Hs m; Tp s; whitecaps; sun alt; moon alt; moon lit; mean; spread; pink\n");
            var bad = new List<string>();
            foreach (var (name, state) in Cases())
            {
                fake.state = state;
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
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.00}; {2:0.0}; {3:0.000}; {4:0.0}; {5:0.0}; {6:0.00}; {7:0.000}; {8:0.000}; {9:0.000}",
                    name, stage.Sea.State.hs, stage.Sea.State.tp, stage.Sea.Whitecaps, sky.sunAltitude, sky.moonAltitude, sky.moonLit, mean, spread, pink));
                if (pink > 0.01f)
                    bad.Add(name + ": " + (pink * 100f).ToString("0", CultureInfo.InvariantCulture) + "% of it is pink (a material with no HDRP shader)");
                if (spread < 0.004f)
                    bad.Add(name + ": the picture is one flat colour");
                if (name.StartsWith("6_"))
                    Assert.IsTrue(stage.DockUp && stage.DockAt.magnitude < 3f, "moored, the jetty lies alongside");
            }
            File.WriteAllText(Path.Combine(dir, "ship_view.txt"), log.ToString());
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = false;
            yield return null;
            Assert.IsEmpty(bad, string.Join("; ", bad));
        }

        [UnityTest]
        public IEnumerator TheStageBringsNothingOfALevel()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.Off = true;
            var moored = ShipStageState.Fair();
            moored.moored = true;
            moored.place = StagePlace.Port;
            moored.portId = "dorestad";
            moored.speed = 0f;
            var stage = ShipStage.Build(kit, new Fake { state = moored });
            stage.View.Size(480, 270);
            for (int f = 0; f < 5; f++)
                yield return null;
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
            ShipStageHook.Off = false;
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
