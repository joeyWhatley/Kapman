using System.Collections;
using System.IO;
using Kapman.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CAMERA CANDIDATES, FILMED (the user, 4 Oct pm: "several different angles and views to choose from"; explicit:
    /// run with -testFilter Kapman.Tests.CameraViewsTests). In L_CameraTest the Gislinge sails north along a stretch of
    /// shore; for each weather and each view the camera follows her (softly sprung, rocking with her), and 6 seconds are
    /// saved as frames to Captures/camera_test/weather_view/ (10 a second, 960 by 540) for Bake/world/make_clips.py.
    /// </summary>
    public class CameraViewsTests
    {
        public const int Fps = 30, Every = 3, Seconds = 6, W = 960, H = 540;

        internal sealed class View
        {
            public string id, name;
            public float pitch, fov, width, yawOff, distance = -1f, orbitPerSecond, rock = 0.2f;
            public float pitchTo = -1f, widthTo = -1f;       // a zoom across the clip
            public bool deck;
        }

        internal static readonly View[] Views =
        {
            new View { id = "a_close_three_quarter", name = "Close three-quarter (35 deg, 60 m across)", pitch = 35f, fov = 30f, width = 60f, yawOff = 40f },
            new View { id = "b_low_close", name = "Low and close (22 deg, 35 m across)", pitch = 22f, fov = 35f, width = 35f, yawOff = 30f, rock = 0.25f },
            new View { id = "c_zoom_linked", name = "Pitch follows zoom (25 deg at 35 m to 45 deg at 150 m)", pitch = 25f, fov = 30f, width = 35f, pitchTo = 45f, widthTo = 150f, yawOff = 45f },
            new View { id = "d_chase", name = "Chase (10 deg, 22 m behind)", pitch = 10f, fov = 50f, distance = 22f, yawOff = 8f, rock = 0.35f },
            new View { id = "e_deck", name = "On deck (the steersman's eye)", deck = true, fov = 62f, rock = 1f },
            new View { id = "f_cinematic_orbit", name = "Cinematic orbit (15 deg, 45 m, turning)", pitch = 15f, fov = 40f, distance = 45f, yawOff = 60f, orbitPerSecond = 10f, rock = 0.3f },
            new View { id = "g_high_isometric", name = "Today's isometric (50 deg, 300 m across)", pitch = 50f, fov = 20f, width = 300f, yawOff = 45f, rock = 0.12f },
        };

        internal static float DistanceFor(float width, float fov, float aspect)
        {
            float hHalf = Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * aspect);
            return width / (2f * Mathf.Tan(hHalf));
        }

        internal static void Save(RenderTexture rt, string file)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest, Explicit("films the camera candidates: run on its own")]
        public IEnumerator FilmTheViews()
        {
            var load = SceneManager.LoadSceneAsync("L_CameraTest", LoadSceneMode.Single);
            while (!load.isDone)
                yield return null;
            for (int i = 0; i < 60; i++)
                yield return null;
            var cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera");
            var orbit = cam.GetComponent<OrbitCamera>();
            if (orbit != null)
                orbit.enabled = false;                                    // the test drives the camera
            foreach (var v in Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None))
                if (v.profile != null && v.profile.TryGet(out UnityEngine.Rendering.HighDefinition.Exposure exposure))
                    exposure.adaptationMode.Override(UnityEngine.Rendering.HighDefinition.AdaptationMode.Fixed);
            var ship = ShipMotion.Player;
            Assert.IsNotNull(ship, "no player's ship");
            var weather = LevelWeather.Current;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cam.targetTexture = rt;
            string root = Path.Combine(Application.dataPath, "..", "Captures", "camera_test");
            Directory.CreateDirectory(root);
            Time.captureFramerate = Fps;

            foreach (var kind in new[] { WeatherKind.Overcast, WeatherKind.Storm })
            {
                if (weather != null)
                {
                    weather.manual = true;
                    weather.kind = kind;
                }
                foreach (var v in Views)
                {
                    // each clip from the same place: she starts off the shore and sails north along it
                    ship.EndOrder("");
                    var body = ship.GetComponent<Kapman.Ships.ShipBody>();
                    ship.transform.SetPositionAndRotation(new Vector3(35f, 0f, 60f), Quaternion.identity);
                    if (body != null)
                        body.enabled = false;
                    yield return null;
                    if (body != null)
                        body.enabled = true;
                    ship.MakeFor(new Vector3(35f, 0f, 2000f));
                    string dir = Path.Combine(root, $"{kind.ToString().ToLowerInvariant()}_{v.id}");
                    Directory.CreateDirectory(dir);
                    int frames = (Seconds + 4) * Fps, saved = 0;
                    Vector3 focus = ship.transform.position;
                    float roll = 0f, nod = 0f, rollV = 0f, nodV = 0f, yaw = ship.heading + v.yawOff;
                    for (int f = 0; f < frames; f++)
                    {
                        yield return null;
                        float t = Mathf.Clamp01((f - 4 * Fps) / (float)(Seconds * Fps));
                        float dt = 1f / Fps;
                        focus = Vector3.Lerp(focus, ship.transform.position + Vector3.up * 1.2f, 1f - Mathf.Exp(-dt * 4f));
                        Quaternion look;
                        Vector3 pos;
                        if (v.deck)
                        {
                            var s = ship.transform;
                            pos = s.TransformPoint(new Vector3(0f, 1.9f, -2.6f));
                            look = Quaternion.LookRotation(s.forward, s.up);
                        }
                        else
                        {
                            float pitch = v.pitchTo > 0f ? Mathf.Lerp(v.pitch, v.pitchTo, Mathf.SmoothStep(0f, 1f, t)) : v.pitch;
                            float width = v.widthTo > 0f ? Mathf.Lerp(v.width, v.widthTo, Mathf.SmoothStep(0f, 1f, t)) : v.width;
                            float dist = v.distance > 0f ? v.distance : DistanceFor(width, v.fov, (float)W / H);
                            yaw = Mathf.LerpAngle(yaw, ship.heading + v.yawOff + v.orbitPerSecond * f * dt, 1f - Mathf.Exp(-dt * 2f));
                            var rot = Quaternion.Euler(pitch, yaw, 0f);
                            pos = focus - rot * Vector3.forward * dist;
                            pos.y = Mathf.Max(pos.y, 1.5f);
                            look = Quaternion.LookRotation(focus - pos, Vector3.up);
                            // her heel as the camera sees it, softly sprung (as OrbitCamera does)
                            Vector3 up = ship.transform.up, right = look * Vector3.right, fwd = Vector3.ProjectOnPlane(look * Vector3.forward, Vector3.up).normalized;
                            roll = Mathf.SmoothDamp(roll, -Mathf.Asin(Mathf.Clamp(Vector3.Dot(up, right), -1f, 1f)) * Mathf.Rad2Deg * v.rock, ref rollV, 0.35f, Mathf.Infinity, dt);
                            nod = Mathf.SmoothDamp(nod, Mathf.Asin(Mathf.Clamp(Vector3.Dot(up, fwd), -1f, 1f)) * Mathf.Rad2Deg * v.rock * 0.5f, ref nodV, 0.35f, Mathf.Infinity, dt);
                            look *= Quaternion.Euler(nod, 0f, roll);
                        }
                        cam.fieldOfView = v.fov;
                        cam.transform.SetPositionAndRotation(pos, look);
                        if (f >= 4 * Fps && (f - 4 * Fps) % Every == 0)
                            Save(rt, Path.Combine(dir, $"f{saved++:000}.png"));
                    }
                    File.WriteAllText(Path.Combine(dir, "name.txt"), v.name);
                }
            }
            Time.captureFramerate = 0;
            cam.targetTexture = null;
            rt.Release();
        }
    }
}
