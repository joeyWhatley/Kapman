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
    /// <summary>What the ship view's picture tests share: a made-up state for the stage, saving a picture, laying
    /// several side by side on one sheet, and showing a low picture large with hard pixels.</summary>
    public static class ShipViewShots
    {
        public sealed class Fake : IShipStageSource
        {
            public ShipStageState state;
            public bool Read(ref ShipStageState s)
            {
                s = state;
                return true;
            }
        }

        public static string Folder(string sub = null)
        {
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "ship_view");
            if (!string.IsNullOrEmpty(sub))
                dir = Path.Combine(dir, sub);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static Texture2D Read(RenderTexture rt)
        {
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false);
            RenderTexture.active = prev;
            return tex;
        }

        /// <summary>Saves the picture and gives its pixels back (for a sheet).</summary>
        public static Color32[] Save(RenderTexture rt, string path)
        {
            var tex = Read(rt);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            var px = tex.GetPixels32();
            Object.Destroy(tex);
            return px;
        }

        /// <summary>The under-oars state the stroke is looked at in: the river on a bright afternoon, nearly calm.</summary>
        public static ShipStageState UnderOars()
        {
            var oars = ShipStageState.Fair();
            oars.underOars = true;
            oars.speed = 1.6f;
            oars.windSpeed = 0.8f;
            oars.gust = 1.2f;
            oars.julianDay = ShipStageState.JulianDayOf(793, 172, 15.5);
            oars.hours = 15.5;
            return oars;
        }

        /// <summary>Pictures of one size laid in a grid, each at half size, as one PNG.</summary>
        public static void Sheet(List<Color32[]> pictures, int w, int h, int columns, string path)
        {
            if (pictures.Count == 0)
                return;
            int hw = w / 2, hh = h / 2, rows = (pictures.Count + columns - 1) / columns;
            var sheet = new Color32[hw * columns * hh * rows];
            for (int i = 0; i < pictures.Count; i++)
            {
                int cx = i % columns, cy = rows - 1 - i / columns;             // (row 0 of a texture is its bottom)
                var px = pictures[i];
                for (int y = 0; y < hh; y++)
                    for (int x = 0; x < hw; x++)
                    {
                        Color32 a = px[(y * 2) * w + x * 2], b = px[(y * 2) * w + x * 2 + 1], c = px[(y * 2 + 1) * w + x * 2], d = px[(y * 2 + 1) * w + x * 2 + 1];
                        sheet[(cy * hh + y) * (hw * columns) + cx * hw + x] = new Color32(
                            (byte)((a.r + b.r + c.r + d.r) / 4), (byte)((a.g + b.g + c.g + d.g) / 4), (byte)((a.b + b.b + c.b + d.b) / 4), 255);
                    }
            }
            var tex = new Texture2D(hw * columns, hh * rows, TextureFormat.RGB24, false);
            tex.SetPixels32(sheet);
            tex.Apply(false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        /// <summary>Moves the stage's camera to a place given in her own axes (x to starboard, y up, z to the bow; metres
        /// from her middle at the water), aimed at another, draws into rt, and puts the camera back. The stage must be
        /// standing still (Running off).</summary>
        public static void FromHerSide(ShipStage stage, RenderTexture rt, Vector3 eye, Vector3 aim, float fov)
        {
            var cam = stage.View.Camera.transform;
            Vector3 wasAt = cam.localPosition;
            Quaternion wasLook = cam.localRotation;
            float wasFov = stage.View.Camera.fieldOfView;
            var turn = Quaternion.Euler(0f, stage.State.heading, 0f);
            Vector3 e = turn * eye, a = turn * aim;
            cam.localPosition = e;
            cam.localRotation = Quaternion.LookRotation(a - e, Vector3.up);
            stage.View.Camera.fieldOfView = fov;
            stage.View.RenderTo(rt);
            cam.localPosition = wasAt;
            cam.localRotation = wasLook;
            stage.View.Camera.fieldOfView = wasFov;
        }
    }

    /// <summary>
    /// THE CREW AT THE OARS (the user, 7 Oct 2026: "give animation a shot with those models I have ... just make a simple
    /// animation for them rowing"). The stage's boat with the rowing crew aboard (PackCrewRig), looked at closely:
    ///   - eight points of one stroke, each from the stage's own camera, from close abeam and from over her stern:
    ///     Captures/ship_view/crew/stroke_*.png, and the eight of each view on one sheet (stroke_sheet_*.png);
    ///   - the men sitting with the oars away: under sail, moored, lying to in a gale (sit_*.png);
    ///   - the oars being put away and the sail set, a second apart (away_*.png, away_sheet.png).
    /// Open them after a run: arms that bend backwards, a man sunk in his thwart, oars through the hull are the faults.
    /// crew.txt beside them has the measures: how far each hand is from its handle, the blades' height over the water.
    /// Run: -runTests -testPlatform PlayMode -testFilter ShipCrewTests   (with a graphics device)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class ShipCrewTests
    {
        const int W = 960, H = 540;

        static readonly (string name, float phase)[] Points =
        {
            ("1_catch_made", 0.00f), ("2_drive_early", 0.13f), ("3_drive_late", 0.27f), ("4_drive_ends", 0.40f),
            ("5_blade_out", 0.48f), ("6_hands_away", 0.62f), ("7_reaching", 0.78f), ("8_blade_dropping_in", 0.96f),
        };

        static readonly Vector3 SideEye = new Vector3(5.4f, 1.55f, -0.5f), SideAim = new Vector3(0f, 0.6f, -0.5f);
        static readonly Vector3 AboveEye = new Vector3(-0.9f, 2.5f, -4.4f), AboveAim = new Vector3(0f, 0.55f, 0.2f);

        [UnityTest]
        public IEnumerator EightPointsOfTheStroke()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = ShipViewShots.Folder("crew");
            var fake = new ShipViewShots.Fake { state = ShipViewShots.UnderOars() };
            var stage = ShipStage.Build(kit, fake);
            var crew = new PackCrewRig(kit.men);
            stage.Crew = crew;
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            stage.Settle();
            for (int f = 0; f < 50; f++)
                yield return null;
            Assert.IsNotNull(crew.Rig, "the rowing rig was made");
            Assert.AreEqual(4, crew.Aboard, "four men aboard");
            Assert.AreEqual(6, crew.Rig.oars.Length, "six oars");
            Assert.AreEqual(CrewMode.Rowing, crew.Crew.Mode);
            var log = new StringBuilder("point; phase; worst hand off its handle cm; lowest blade middle over the water m; highest m; the hull's rail over the water m\n");
            var view = new List<Color32[]>();
            var side = new List<Color32[]>();
            var above = new List<Color32[]>();
            crew.holdStroke = true;
            float worstEver = 0f;
            foreach (var (name, phase) in Points)
            {
                crew.Rig.Phase = phase;
                yield return null;
                yield return null;
                view.Add(ShipViewShots.Save(rt0(stage, rt), Path.Combine(dir, "stroke_" + name + "_view.png")));
                stage.Running = false;
                ShipViewShots.FromHerSide(stage, rt, SideEye, SideAim, 38f);
                side.Add(ShipViewShots.Save(rt, Path.Combine(dir, "stroke_" + name + "_side.png")));
                ShipViewShots.FromHerSide(stage, rt, AboveEye, AboveAim, 46f);
                above.Add(ShipViewShots.Save(rt, Path.Combine(dir, "stroke_" + name + "_astern.png")));
                stage.Running = true;

                // the measures: each rower's wrists against the handles they hold, the blades against the water
                float worst = 0f, low = 99f, high = -99f;
                var rig = crew.Rig;
                foreach (var r in rig.rowers)
                {
                    var b = r.body;
                    foreach (int oi in new[] { r.oar, r.oar2 })
                    {
                        if (oi < 0)
                            continue;
                        var o = rig.oars[oi];
                        Vector3 grip = o.pivot.TransformPoint(new Vector3(-o.gripInner, 0f, 0f));
                        float l = (b.handL.position - grip).magnitude, rr = (b.handR.position - grip).magnitude;
                        worst = Mathf.Max(worst, Mathf.Abs(Mathf.Min(l, rr) - rig.palm));
                    }
                }
                for (int i = 0; i < rig.oars.Length; i++)
                {
                    Vector3 blade = rig.BladeMiddle(i);
                    float over = blade.y - rig.waterHeight(blade);
                    low = Mathf.Min(low, over);
                    high = Mathf.Max(high, over);
                }
                worstEver = Mathf.Max(worstEver, worst);
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.00}; {2:0.0}; {3:0.00}; {4:0.00}; {5:0.00}",
                    name, phase, worst * 100f, low, high, stage.Ship.Gunwale));
                // the blades are in the water through the drive and clear of it on the way back
                if (phase > 0.02f && phase < 0.38f)
                    Assert.Less(high, 0.02f, name + ": a blade is out of the water in the drive");
                if (phase > 0.55f && phase < 0.9f)
                    Assert.Greater(low, 0.05f, name + ": a blade is in the water on the way back");
            }
            ShipViewShots.Sheet(view, W, H, 4, Path.Combine(dir, "stroke_sheet_view.png"));
            ShipViewShots.Sheet(side, W, H, 4, Path.Combine(dir, "stroke_sheet_side.png"));
            ShipViewShots.Sheet(above, W, H, 4, Path.Combine(dir, "stroke_sheet_astern.png"));
            File.WriteAllText(Path.Combine(dir, "crew.txt"), log.ToString());
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
            Assert.Less(worstEver, 0.03f, "a hand is more than 3 cm off the handle it holds at some point of the stroke");
        }

        static RenderTexture rt0(ShipStage stage, RenderTexture rt)
        {
            stage.View.RenderTo(rt);
            return rt;
        }

        /// <summary>The men with the oars away: under sail off the coast, moored at the jetty, lying to in a gale.</summary>
        [UnityTest]
        public IEnumerator TheMenSitWhenSheSailsLiesMooredOrLiesTo()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = ShipViewShots.Folder("crew");
            var sail = ShipStageState.Fair();
            sail.speed = 2.6f;
            sail.windSpeed = 6f;
            sail.gust = 8f;
            sail.julianDay = ShipStageState.JulianDayOf(793, 172, 15.5);
            sail.hours = 15.5;
            var moored = sail;
            moored.place = StagePlace.Port;
            moored.moored = true;
            moored.portId = "dorestad";
            moored.speed = 0f;
            moored.heading = 270f;
            moored.windSpeed = 3f;
            var gale = sail;
            gale.place = StagePlace.OpenSea;
            gale.fetchKm = 300f;
            gale.depth = 40f;
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
            var fake = new ShipViewShots.Fake { state = sail };
            var stage = ShipStage.Build(kit, fake);
            var crew = new PackCrewRig(kit.men);
            stage.Crew = crew;
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            foreach (var (name, state, mode) in new[] { ("sail", sail, CrewMode.Sailing), ("moored", moored, CrewMode.Moored), ("gale", gale, CrewMode.LyingTo) })
            {
                fake.state = state;
                stage.Settle();
                for (int f = 0; f < 50; f++)
                    yield return null;
                Assert.AreEqual(mode, crew.Crew.Mode, name);
                Assert.Less(crew.Rig.oarsOut, 0.01f, name + ": the oars are away");
                stage.View.RenderTo(rt);
                ShipViewShots.Save(rt, Path.Combine(dir, "sit_" + name + "_view.png"));
                stage.Running = false;
                // (moored, the jetty is on her port side: looked at from starboard, as the others)
                ShipViewShots.FromHerSide(stage, rt, SideEye, SideAim, 38f);
                ShipViewShots.Save(rt, Path.Combine(dir, "sit_" + name + "_side.png"));
                ShipViewShots.FromHerSide(stage, rt, AboveEye, AboveAim, 46f);
                ShipViewShots.Save(rt, Path.Combine(dir, "sit_" + name + "_astern.png"));
                stage.Running = true;
            }
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        /// <summary>From oars to sail without a jump: the stroke dies, the oars are swung in and laid along the thwarts,
        /// the sail is set. Eight moments, from over her stern.</summary>
        [UnityTest]
        public IEnumerator TheOarsArePutAwayAndTheSailIsSet()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();                                // (and any view a chart test before this one left up, with its stage)
            string dir = ShipViewShots.Folder("crew");
            var fake = new ShipViewShots.Fake { state = ShipViewShots.UnderOars() };
            var stage = ShipStage.Build(kit, fake);
            var crew = new PackCrewRig(kit.men);
            stage.Crew = crew;
            stage.View.Size(480, 270);
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            Time.captureFramerate = 30;
            stage.Settle();
            for (int f = 0; f < 40; f++)
                yield return null;
            // a breeze comes up: she is put under sail
            var sail = fake.state;
            sail.underOars = false;
            sail.speed = 2.6f;
            sail.windSpeed = 6f;
            sail.gust = 8f;
            fake.state = sail;
            crew.Crew.holdSeconds = 1f;                                       // (not to wait out the six seconds a mode is held)
            var shots = new List<Color32[]>();
            var log = new StringBuilder("moment; seconds; mode; oars out; effort; sail set\n");
            for (int i = 0; i < 8; i++)
            {
                for (int f = 0; f < 30; f++)
                    yield return null;
                stage.Running = false;
                ShipViewShots.FromHerSide(stage, rt, AboveEye, AboveAim, 46f);
                shots.Add(ShipViewShots.Save(rt, Path.Combine(dir, "away_" + i + ".png")));
                stage.Running = true;
                log.AppendLine(string.Format(CultureInfo.InvariantCulture, "away_{0}; {1}; {2}; {3:0.00}; {4:0.00}; {5}", i, i + 1, crew.Crew.Mode, crew.Rig.oarsOut, crew.Rig.effort,
                    stage.Ship.SailOpen != null && stage.Ship.SailOpen.gameObject.activeSelf));
            }
            ShipViewShots.Sheet(shots, W, H, 4, Path.Combine(dir, "away_sheet.png"));
            File.WriteAllText(Path.Combine(dir, "away.txt"), log.ToString());
            Assert.AreEqual(CrewMode.Sailing, crew.Crew.Mode);
            Assert.Less(crew.Rig.oarsOut, 0.01f, "the oars are away");
            Assert.IsTrue(stage.Ship.SailOpen == null || stage.Ship.SailOpen.gameObject.activeSelf, "the sail is set");
            // and the crew can be taken off again: the stage has its oars back and moves them itself
            stage.Crew = null;
            yield return null;
            Assert.IsFalse(stage.Ship.OarsDriven);
            foreach (var o in stage.Ship.OarsPort)
                Assert.IsTrue(o != null && o.parent == stage.Ship.FitOut, "an oar was not given back to the stage");
            Time.captureFramerate = 0;
            rt.Release();
            Object.Destroy(rt);
            Object.Destroy(stage.gameObject);
            ShipStageHook.Off = ShipStageHook.StartsOff;
            yield return null;
        }

        [Test]
        public void TheCrewRowsWhenTheRecordSaysSo()
        {
            var s = ShipStageState.Fair();
            Assert.AreEqual(CrewMode.Sailing, ShipCrew.Decide(PackCrewRig.From(s, 7.7f, 0f)), "a fair wind");
            s.underOars = true;
            Assert.AreEqual(CrewMode.Rowing, ShipCrew.Decide(PackCrewRig.From(s, 7.7f, 0f)), "under oars");
            s.lyingTo = true;
            Assert.AreEqual(CrewMode.LyingTo, ShipCrew.Decide(PackCrewRig.From(s, 7.7f, 0f)), "lying to");
            s.lyingTo = false;
            s.underOars = false;
            s.berthing = true;
            s.berthDistance = 40f;
            var near = PackCrewRig.From(s, 7.7f, 0f);
            Assert.AreEqual(CrewMode.Rowing, ShipCrew.Decide(near), "she is rowed onto her berth");
            Assert.IsFalse(near.oarsIn);
            s.berthDistance = 5f;
            Assert.IsTrue(PackCrewRig.From(s, 7.7f, 0f).oarsIn, "the oars are in for the last boat's length");
            s.moored = true;
            s.speed = 0f;
            Assert.AreEqual(CrewMode.Moored, ShipCrew.Decide(PackCrewRig.From(s, 7.7f, 0f)), "moored");
            // the stroke: the blade is in for the drive, out for the way back, and the four beats join without a jump
            Assert.AreEqual(1f, Stroke.At(0.2f).dip, 1e-4f);
            Assert.AreEqual(0f, Stroke.At(0.7f).dip, 1e-4f);
            for (float p = 0f; p < 1f; p += 0.01f)
            {
                var a = Stroke.At(p);
                var b = Stroke.At(p + 0.01f);
                Assert.Less(Mathf.Abs(a.sweep - b.sweep), 0.08f, "the sweep jumps at " + p);
                Assert.Less(Mathf.Abs(a.lean - b.lean), 0.08f, "the back jumps at " + p);
            }
        }
    }
}
