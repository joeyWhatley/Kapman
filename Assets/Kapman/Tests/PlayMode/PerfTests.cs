using System.Collections;
using System.IO;
using System.Text;
using Kapman.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// What Dorestad costs to draw, headless (explicit: run with -testFilter Kapman.Tests.PerfTests): the level in Play,
    /// the main camera drawing into a 1920x1080 target, the isometric view at three zooms and the free view, frame times
    /// measured, the F8 probe's table, and a picture of each view in Captures/iso (to judge the look without a screen).
    /// </summary>
    public class PerfTests
    {
        static IEnumerator Measure(float seconds, float[] into, int slot)
        {
            yield return new WaitForSecondsRealtime(1.5f);
            float sum = 0f;
            int n = 0;
            float until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
                sum += Time.unscaledDeltaTime;
                n++;
            }
            into[slot] = n > 0 ? sum / n * 1000f : 0f;
        }

        static void Save(RenderTexture rt, string name)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "iso");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest, Explicit("a measurement, not a check: run on its own")]
        public IEnumerator DorestadIsometricCost()
        {
            var load = SceneManager.LoadSceneAsync("L_Dorestad", LoadSceneMode.Single);
            while (!load.isDone)
                yield return null;
            for (int i = 0; i < 120; i++)
                yield return null;
            var cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera");
            // the exposure adapts at once (as the shot tool does), so a picture taken a few seconds in is exposed right
            foreach (var v in Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None))
                if (v.profile != null && v.profile.TryGet(out UnityEngine.Rendering.HighDefinition.Exposure exposure))
                    exposure.adaptationMode.Override(UnityEngine.Rendering.HighDefinition.AdaptationMode.Fixed);
            var orbit = cam.GetComponent<OrbitCamera>();
            Assert.IsNotNull(orbit, "no OrbitCamera");
            var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cam.targetTexture = rt;
            var ms = new float[4];
            var report = new StringBuilder("Kapman Dorestad in Play at 1920x1080 (headless):\n");

            orbit.isometric = true;
            float[] widths = { 25f, 35f, 120f };
            for (int i = 0; i < widths.Length; i++)
            {
                orbit.isoWidth = widths[i];
                yield return Measure(3f, ms, i);
                Save(rt, $"iso_{widths[i]:0}m");
                report.AppendLine($"  isometric, {widths[i]:0} m across: {ms[i]:0.0} ms ({1000f / Mathf.Max(ms[i], 0.01f):0} fps)");
            }
            orbit.isoWidth = 300f;
            orbit.isometric = false;
            orbit.distance = 40f;
            orbit.pitch = 14f;
            yield return Measure(3f, ms, 3);
            Save(rt, "free_chase");
            report.AppendLine($"  free chase view: {ms[3]:0.0} ms ({1000f / Mathf.Max(ms[3], 0.01f):0} fps)");

            orbit.isometric = true;
            yield return new WaitForSecondsRealtime(2f);
            var probe = PerfProbe.Instance;
            if (probe != null)
            {
                yield return probe.StartCoroutine(probe.Run());
                report.AppendLine("The probe in the isometric view (300 m):");
                report.Append(PerfProbe.LastTable);
            }
            Debug.Log(report.ToString());
            File.WriteAllText(Path.Combine(Application.dataPath, "..", "Logs", "perf_dorestad.txt"), report.ToString());
            cam.targetTexture = null;
            rt.Release();
        }
    }
}
