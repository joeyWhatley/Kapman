using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;
using Kapman.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SCALE TEST, FILMED (explicit: -testFilter Kapman.Tests.ScaleTests): for each scale level (ScaleTestBuilder) an
    /// overhead picture of the whole stretch, a 6 s clip in the chosen view (B1: 35 degrees down, 35 m across, the soft lens)
    /// of the ship leaving Dorestad's dams, and the widest view at Traiectum, into Captures/scale_test/1to{scale}/.
    /// </summary>
    public class ScaleTests
    {
        const int Fps = 30, Every = 3, Seconds = 6, W = 960, H = 540;
        const float Speed = 2f;                                             // m/s, as ScaleTestBuilder.SailSpeed
        static readonly int[] Scales = { 20, 40, 80, 150 };

        static List<Vector2> Line(int scale, out float length, out float trajectumS)
        {
            string json = File.ReadAllText(Path.Combine(Application.dataPath, "Kapman", "Levels", "ScaleTest", scale.ToString(), "scale_info.json"));
            var o = Json.Parse(json);
            length = (float)Json.Num(o, "length");
            trajectumS = length * 0.24f;
            foreach (var p in Json.Arr(o, "ports") ?? new List<object>())
                if (Json.Str(p, "name") == "Traiectum")
                    trajectumS = (float)Json.Num(p, "s") + 75f;
            var line = new List<Vector2>();
            foreach (var p in Json.Arr(o, "line"))
            {
                var a = (List<object>)p;
                line.Add(new Vector2((float)(double)a[0], (float)(double)a[1]));
            }
            return line;
        }

        /// <summary>A point and the direction along the line at a distance from its start.</summary>
        static (Vector2 at, Vector2 dir) Along(List<Vector2> line, float s)
        {
            for (int i = 1; i < line.Count; i++)
            {
                float seg = Vector2.Distance(line[i - 1], line[i]);
                if (s <= seg)
                    return (Vector2.Lerp(line[i - 1], line[i], s / Mathf.Max(seg, 1e-3f)), (line[i] - line[i - 1]).normalized);
                s -= seg;
            }
            return (line[line.Count - 1], (line[line.Count - 1] - line[line.Count - 2]).normalized);
        }

        static void B1(Camera cam, Vector3 focus, float yaw, float width, float pitch)
        {
            float fov = 35f;
            float hHalf = Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * ((float)W / H));
            float dist = width / (2f * Mathf.Tan(hHalf));
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            cam.fieldOfView = fov;
            cam.transform.SetPositionAndRotation(focus - rot * Vector3.forward * dist, rot);
        }

        [UnityTest, Explicit("films the scale levels: run on its own")]
        public IEnumerator FilmTheScales()
        {
            string root = Path.Combine(Application.dataPath, "..", "Captures", "scale_test");
            foreach (int scale in Scales)
            {
                var load = SceneManager.LoadSceneAsync($"L_Scale_{scale}", LoadSceneMode.Single);
                while (!load.isDone)
                    yield return null;
                for (int i = 0; i < 60; i++)
                    yield return null;
                var cam = Camera.main;
                Assert.IsNotNull(cam, $"no camera at 1:{scale}");
                var orbit = cam.GetComponent<OrbitCamera>();
                if (orbit != null) orbit.enabled = false;
                LensDistortion lens = null;
                foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                    if (v.isGlobal && v.profile != null)
                    {
                        if (v.profile.TryGet(out Exposure e)) e.adaptationMode.Override(AdaptationMode.Fixed);
                        if (!v.profile.TryGet(out lens)) lens = v.profile.Add<LensDistortion>(true);
                    }
                var ship = ShipMotion.Player;
                Assert.IsNotNull(ship);
                var body = ship.GetComponent<Kapman.Ships.ShipBody>();
                if (body != null) body.enabled = false;              // the clip drives her along the line at the sailing speed
                ship.enabled = false;
                var line = Line(scale, out float length, out float trajS);
                var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                cam.targetTexture = rt;
                string dir = Path.Combine(root, $"1to{scale}");
                Directory.CreateDirectory(dir);
                Time.captureFramerate = Fps;

                // 1. overhead: the whole stretch between its fog banks
                if (lens != null) { lens.intensity.Override(0f); lens.active = false; }
                Vector2 min = line[0], max = line[0];
                foreach (var p in line) { min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
                Vector2 mid = (min + max) * 0.5f;
                float spanX = (max.x - min.x) + 700f, spanZ = (max.y - min.y) + 700f;
                float vfov = 30f, aspect = (float)W / H;
                float height = Mathf.Max(spanZ, spanX / aspect) * 0.5f / Mathf.Tan(vfov * 0.5f * Mathf.Deg2Rad);
                cam.fieldOfView = vfov;
                cam.farClipPlane = height * 2f + 2000f;
                cam.transform.SetPositionAndRotation(new Vector3(mid.x, height, mid.y), Quaternion.Euler(90f, 0f, 0f));
                for (int i = 0; i < 40; i++) yield return null;
                CameraViewsTests.Save(rt, Path.Combine(dir, "overhead.png"));
                cam.farClipPlane = 8000f;

                // 2. B1 at Dorestad: 6 s of her leaving the dams
                if (lens != null) { lens.active = true; lens.intensity.Override(0.25f); lens.scale.Override(1.08f); }
                float s = 40f;
                Vector3 focus = Vector3.zero;
                float yaw = 0f;
                int saved = 0;
                for (int f = 0; f < (Seconds + 2) * Fps; f++)
                {
                    s += Speed / Fps;
                    var (at, d) = Along(line, s);
                    ship.transform.SetPositionAndRotation(new Vector3(at.x, 0f, at.y), Quaternion.LookRotation(new Vector3(d.x, 0f, d.y)));
                    float want = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg + 35f;
                    yaw = f == 0 ? want : Mathf.LerpAngle(yaw, want, 1f - Mathf.Exp(-2f / Fps));
                    focus = f == 0 ? ship.transform.position : Vector3.Lerp(focus, ship.transform.position + Vector3.up, 1f - Mathf.Exp(-4f / Fps));
                    B1(cam, focus, yaw, 35f, 35f);
                    yield return null;
                    if (f >= 2 * Fps && (f - 2 * Fps) % Every == 0)
                    {
                        string clip = Path.Combine(dir, "b1_dorestad");
                        Directory.CreateDirectory(clip);
                        CameraViewsTests.Save(rt, Path.Combine(clip, $"f{saved++:000}.png"));
                    }
                }

                // 3. the widest view at Traiectum: the water, both bands and the fog's edges
                {
                    var (at, d) = Along(line, trajS);
                    ship.transform.SetPositionAndRotation(new Vector3(at.x, 0f, at.y), Quaternion.LookRotation(new Vector3(d.x, 0f, d.y)));
                    B1(cam, ship.transform.position, Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg + 35f, 110f + 2.5f * 60f, 48f);
                    for (int i = 0; i < 40; i++) yield return null;
                    CameraViewsTests.Save(rt, Path.Combine(dir, "widest_traiectum.png"));
                    B1(cam, ship.transform.position, Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg + 35f, 35f, 35f);
                    for (int i = 0; i < 40; i++) yield return null;
                    CameraViewsTests.Save(rt, Path.Combine(dir, "b1_traiectum.png"));
                }
                File.WriteAllText(Path.Combine(dir, "info.txt"), $"1:{scale}, {length:0} m of river, {length / Speed / 60f:0.0} min at {Speed} m/s");
                Time.captureFramerate = 0;
                cam.targetTexture = null;
                rt.Release();
            }
        }
    }
}

