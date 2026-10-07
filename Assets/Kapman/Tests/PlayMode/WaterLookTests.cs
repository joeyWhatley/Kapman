using System.Collections;
using System.IO;
using Kapman.Levels;
using Kapman.Levels.Water;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE RIVER'S WATER FROM ABOVE (explicit: -testFilter Kapman.Tests.WaterLookTests): the 1:40 scale level in the
    /// game's view (35 degrees down) and at the widest, with a few settings of the river's scattering, so the silty river
    /// reads khaki-brown from above rather than black. Stills into Captures/water_look/.
    /// </summary>
    public class WaterLookTests
    {
        const int W = 960, H = 540;

        static readonly (string id, float scatterGain, float ambient, float body, float height, float absorption)[] Variants =
        {
            ("v0_now", 0.65f, 0.15f, 0.8f, 0.2f, 0.6f),
            ("v1_brighter", 1.0f, 0.5f, 1.0f, 0.3f, 0.6f),
            ("v2_silty", 1.25f, 0.8f, 1.2f, 0.4f, 0.5f),
            ("v3_milky", 1.5f, 1.0f, 1.5f, 0.5f, 0.4f),
        };

        static void Place(Camera cam, Vector3 focus, float yaw, float width, float pitch)
        {
            float fov = 35f;
            float hHalf = Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * ((float)W / H));
            float dist = width / (2f * Mathf.Tan(hHalf));
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            cam.fieldOfView = fov;
            cam.transform.SetPositionAndRotation(focus - rot * Vector3.forward * dist, rot);
        }

        [UnityTest, Explicit("renders the river's water settings: run on its own")]
        public IEnumerator RenderTheRiver()
        {
            var load = SceneManager.LoadSceneAsync("L_Scale_40", LoadSceneMode.Single);
            while (!load.isDone)
                yield return null;
            for (int i = 0; i < 60; i++)
                yield return null;
            var cam = Camera.main;
            var orbit = cam.GetComponent<OrbitCamera>();
            if (orbit != null) orbit.enabled = false;
            foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (v.isGlobal && v.profile != null && v.profile.TryGet(out Exposure e))
                    e.adaptationMode.Override(AdaptationMode.Fixed);
            var river = Object.FindFirstObjectByType<RiverTint>();
            Assert.IsNotNull(river, "no river");
            var surface = river.GetComponent<WaterSurface>();
            var driver = river.GetComponent<WaterProfileDriver>();
            // first the game's own water (the level's record through WaterProfileDriver and RiverTint), then the variants
            {
                var shipNow = ShipMotion.Player;
                var b0 = shipNow.GetComponent<Kapman.Ships.ShipBody>();
                if (b0 != null) b0.enabled = false;
                shipNow.enabled = false;
                var rt0 = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                cam.targetTexture = rt0;
                string dir0 = Path.Combine(Application.dataPath, "..", "Captures", "water_look");
                Directory.CreateDirectory(dir0);
                Place(cam, shipNow.transform.position, shipNow.transform.eulerAngles.y + 35f, 35f, 35f);
                for (int i = 0; i < 360; i++) yield return null;
                CameraViewsTests.Save(rt0, Path.Combine(dir0, "game_b1.png"));
                Place(cam, shipNow.transform.position, shipNow.transform.eulerAngles.y + 35f, 260f, 48f);
                for (int i = 0; i < 40; i++) yield return null;
                CameraViewsTests.Save(rt0, Path.Combine(dir0, "game_widest.png"));
                cam.targetTexture = null;
                rt0.Release();
            }
            if (driver != null) driver.enabled = false;
            river.enabled = false;                                           // the test sets the water itself
            var ship = Object.FindFirstObjectByType<ShipMotion>();               // (disabled above: Player is cleared)
            var body = ship.GetComponent<Kapman.Ships.ShipBody>();
            if (body != null) body.enabled = false;
            ship.enabled = false;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cam.targetTexture = rt;
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "water_look");
            Directory.CreateDirectory(dir);
            Color body0 = new Color(0.33f, 0.30f, 0.18f);                     // the Kromme Rijn's research colour
            Vector3 at = ship.transform.position;
            float yaw = ship.transform.eulerAngles.y + 35f;
            foreach (var (id, gain, ambient, bodyScatter, height, absorption) in Variants)
            {
                surface.scatteringColor = new Color(body0.r * gain, body0.g * gain, body0.b * gain);
                surface.refractionColor = new Color(body0.r * 0.92f, body0.g * 0.92f, body0.b * 0.92f);
                surface.ambientScattering = ambient;
                surface.directLightBodyScattering = bodyScatter;
                surface.heightScattering = height;
                surface.absorptionDistance = absorption;
                Place(cam, at, yaw, 35f, 35f);
                for (int i = 0; i < 30; i++) yield return null;
                CameraViewsTests.Save(rt, Path.Combine(dir, id + "_b1.png"));
                Place(cam, at, yaw, 260f, 48f);
                for (int i = 0; i < 30; i++) yield return null;
                CameraViewsTests.Save(rt, Path.Combine(dir, id + "_widest.png"));
            }
            cam.targetTexture = null;
            rt.Release();
        }
    }
}
