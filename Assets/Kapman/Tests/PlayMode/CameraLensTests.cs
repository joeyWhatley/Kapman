using System.Collections;
using System.IO;
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
    /// VIEW B WITH A LENS (the user, 4 Oct pm: "B but maybe a reversed fisheye ... slightly warping and enlarging the view of
    /// the boat"): the low and close view filmed plain and with HDRP's lens distortion at a few strengths each way, so the
    /// boat in the middle grows and the view bends round her; explicit: -testFilter Kapman.Tests.CameraLensTests.
    /// </summary>
    public class CameraLensTests
    {
        static readonly (string id, string name, float intensity, float scale)[] Lenses =
        {
            ("lens_0_plain", "B plain (no lens)", 0f, 1f),
            ("lens_1_plus_soft", "B, lens +0.25", 0.25f, 1f),
            ("lens_2_plus_strong", "B, lens +0.45", 0.45f, 1f),
            ("lens_3_minus_soft", "B, lens -0.25", -0.25f, 1f),
            ("lens_4_minus_strong", "B, lens -0.45", -0.45f, 1f),
        };

        [UnityTest, Explicit("films view B with lens variants: run on its own")]
        public IEnumerator FilmTheLenses()
        {
            const int Fps = CameraViewsTests.Fps, Every = CameraViewsTests.Every, Seconds = CameraViewsTests.Seconds;
            var load = SceneManager.LoadSceneAsync("L_CameraTest", LoadSceneMode.Single);
            while (!load.isDone)
                yield return null;
            for (int i = 0; i < 60; i++)
                yield return null;
            var cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera");
            var orbit = cam.GetComponent<OrbitCamera>();
            if (orbit != null)
                orbit.enabled = false;
            VolumeProfile profile = null;
            foreach (var vol in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (vol.isGlobal && vol.profile != null)
                {
                    profile = vol.profile;                                       // the runtime copy
                    if (profile.TryGet(out Exposure exposure))
                        exposure.adaptationMode.Override(AdaptationMode.Fixed);
                }
            Assert.IsNotNull(profile, "no global volume");
            if (!profile.TryGet(out LensDistortion lens))
                lens = profile.Add<LensDistortion>(true);
            var ship = ShipMotion.Player;
            Assert.IsNotNull(ship, "no player's ship");
            var weather = LevelWeather.Current;
            if (weather != null)
            {
                weather.manual = true;
                weather.kind = WeatherKind.Overcast;
            }
            var v = CameraViewsTests.Views[1];                                  // B, low and close
            var rt = new RenderTexture(CameraViewsTests.W, CameraViewsTests.H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cam.targetTexture = rt;
            string root = Path.Combine(Application.dataPath, "..", "Captures", "camera_test");
            Time.captureFramerate = Fps;
            foreach (var (id, name, intensity, scale) in Lenses)
            {
                lens.intensity.Override(intensity);
                lens.scale.Override(scale);
                lens.active = intensity != 0f;
                var body = ship.GetComponent<Kapman.Ships.ShipBody>();
                ship.EndOrder("");
                ship.transform.SetPositionAndRotation(new Vector3(35f, 0f, 60f), Quaternion.identity);
                if (body != null) body.enabled = false;
                yield return null;
                if (body != null) body.enabled = true;
                ship.MakeFor(new Vector3(35f, 0f, 2000f));
                string dir = Path.Combine(root, "overcast_" + id);
                Directory.CreateDirectory(dir);
                int frames = (Seconds + 4) * Fps, saved = 0;
                Vector3 focus = ship.transform.position;
                float roll = 0f, nod = 0f, rollV = 0f, nodV = 0f, yaw = ship.heading + v.yawOff;
                for (int f = 0; f < frames; f++)
                {
                    yield return null;
                    float dt = 1f / Fps;
                    focus = Vector3.Lerp(focus, ship.transform.position + Vector3.up * 1.2f, 1f - Mathf.Exp(-dt * 4f));
                    float dist = CameraViewsTests.DistanceFor(v.width, v.fov, (float)CameraViewsTests.W / CameraViewsTests.H);
                    yaw = Mathf.LerpAngle(yaw, ship.heading + v.yawOff, 1f - Mathf.Exp(-dt * 2f));
                    var rot = Quaternion.Euler(v.pitch, yaw, 0f);
                    var pos = focus - rot * Vector3.forward * dist;
                    pos.y = Mathf.Max(pos.y, 1.5f);
                    var look = Quaternion.LookRotation(focus - pos, Vector3.up);
                    Vector3 up = ship.transform.up, right = look * Vector3.right, fwd = Vector3.ProjectOnPlane(look * Vector3.forward, Vector3.up).normalized;
                    roll = Mathf.SmoothDamp(roll, -Mathf.Asin(Mathf.Clamp(Vector3.Dot(up, right), -1f, 1f)) * Mathf.Rad2Deg * v.rock, ref rollV, 0.35f, Mathf.Infinity, dt);
                    nod = Mathf.SmoothDamp(nod, Mathf.Asin(Mathf.Clamp(Vector3.Dot(up, fwd), -1f, 1f)) * Mathf.Rad2Deg * v.rock * 0.5f, ref nodV, 0.35f, Mathf.Infinity, dt);
                    look *= Quaternion.Euler(nod, 0f, roll);
                    cam.fieldOfView = v.fov;
                    cam.transform.SetPositionAndRotation(pos, look);
                    if (f >= 4 * Fps && (f - 4 * Fps) % Every == 0)
                        CameraViewsTests.Save(rt, Path.Combine(dir, $"f{saved++:000}.png"));
                }
                File.WriteAllText(Path.Combine(dir, "name.txt"), name);
            }
            Time.captureFramerate = 0;
            cam.targetTexture = null;
            rt.Release();
        }
    }
}
