using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// WHAT EACH THING COSTS (4 Oct, the user: "still around 20 fps"): in Play, F8 runs through the level's expensive
    /// parts, switching one off at a time for a few seconds, and times the frames (the mean of the last 2.5 s of each),
    /// then puts everything back and writes the table to the console and to Logs/perf_probe.txt. Hold the camera still
    /// while it runs (about a minute). Changes only runtime copies: the volume profile is the runtime copy, never the asset.
    /// </summary>
    public class PerfProbe : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Make()
        {
            if (FindFirstObjectByType<PerfProbe>() != null)
                return;
            var go = new GameObject("Perf probe (F8)") { hideFlags = HideFlags.DontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<PerfProbe>();
        }

        bool running;
        string status = "";
        public static PerfProbe Instance { get; private set; }
        /// <summary>The last table (for the headless Play test); null while none.</summary>
        public static string LastTable { get; private set; }
        public bool Running => running;

        void Awake() => Instance = this;

        void Update()
        {
            var kb = Keyboard.current;
            if (!running && kb != null && kb.f8Key.wasPressedThisFrame)
                StartCoroutine(Run());
        }

        void OnGUI()
        {
            if (running)
                GUI.Label(new Rect(Screen.width - 420, 12, 410, 24), "Measuring (F8): hold still.  " + status);
        }

        static IEnumerator Measure(List<float> into)
        {
            yield return new WaitForSecondsRealtime(1.2f);
            float sum = 0f;
            int n = 0;
            float until = Time.realtimeSinceStartup + 2.5f;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
                sum += Time.unscaledDeltaTime;
                n++;
            }
            into.Add(n > 0 ? sum / n * 1000f : 0f);
        }

        public IEnumerator Run()
        {
            running = true;
            var terrains = Terrain.activeTerrains;
            var profile = GlobalProfile();
            var steps = new List<(string name, Action off, Action on)>();
            var detail = new float[terrains.Length];
            var trees = new float[terrains.Length];
            steps.Add(("grass (terrain details)", () => { for (int i = 0; i < terrains.Length; i++) { detail[i] = terrains[i].detailObjectDistance; terrains[i].detailObjectDistance = 0f; } },
                                                   () => { for (int i = 0; i < terrains.Length; i++) terrains[i].detailObjectDistance = detail[i]; }));
            steps.Add(("trees", () => { for (int i = 0; i < terrains.Length; i++) { trees[i] = terrains[i].treeDistance; terrains[i].treeDistance = 0f; } },
                                () => { for (int i = 0; i < terrains.Length; i++) terrains[i].treeDistance = trees[i]; }));
            var particles = FindObjectsByType<ParticleSystemRenderer>(FindObjectsSortMode.None);
            steps.Add(("smoke and particles", () => Each(particles, true), () => Each(particles, false)));
            var lights = new List<Light>();
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type != LightType.Directional && l.enabled)
                    lights.Add(l);
            steps.Add(($"town lights ({lights.Count})", () => { foreach (var l in lights) l.enabled = false; }, () => { foreach (var l in lights) l.enabled = true; }));
            Light sun = null;
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional && l.shadows != LightShadows.None && (sun == null || l.intensity > sun.intensity))
                    sun = l;
            var sunShadows = sun != null ? sun.shadows : LightShadows.None;
            if (sun != null)
                steps.Add(("sun shadows", () => sun.shadows = LightShadows.None, () => sun.shadows = sunShadows));
            var waters = FindObjectsByType<WaterSurface>(FindObjectsSortMode.None);
            steps.Add(($"water surfaces ({waters.Length})", () => { foreach (var w in waters) w.enabled = false; }, () => { foreach (var w in waters) w.enabled = true; }));
            if (profile != null)
            {
                Toggle<GlobalIllumination>(steps, profile, "screen-space GI", c => c.enable);
                Toggle<ScreenSpaceReflection>(steps, profile, "screen-space reflections", c => c.enabled);
                Toggle<VolumetricClouds>(steps, profile, "volumetric clouds", c => c.enable);
                Toggle<Fog>(steps, profile, "volumetric fog", c => c.enableVolumetricFog);
                Toggle<ContactShadows>(steps, profile, "contact shadows", c => c.enable);
                Toggle<MicroShadowing>(steps, profile, "micro shadowing", c => c.enable);
                if (profile.TryGet(out MotionBlur blur))
                {
                    float was = blur.intensity.value;
                    bool over = blur.intensity.overrideState;
                    steps.Add(("motion blur", () => blur.intensity.Override(0f), () => { blur.intensity.value = was; blur.intensity.overrideState = over; }));
                }
            }

            var times = new List<float>();
            status = "baseline";
            yield return Measure(times);
            float baseline = times[0];
            var rows = new List<(string name, float ms)>();
            foreach (var s in steps)
            {
                status = "without " + s.name;
                s.off();
                var t = new List<float>();
                yield return Measure(t);
                s.on();
                rows.Add((s.name, t[0]));
            }
            status = "everything above off";
            foreach (var s in steps) s.off();
            var all = new List<float>();
            yield return Measure(all);
            foreach (var s in steps) s.on();

            var sb = new StringBuilder();
            sb.AppendLine($"Kapman perf probe, {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, {Screen.width}x{Screen.height}, {DateTime.Now:HH:mm}");
            sb.AppendLine($"  baseline: {baseline:0.0} ms ({1000f / Mathf.Max(baseline, 0.01f):0} fps)");
            rows.Sort((a, b) => a.ms.CompareTo(b.ms));
            foreach (var r in rows)
                sb.AppendLine($"  without {r.name,-28}: {r.ms,6:0.0} ms  (saves {baseline - r.ms,5:0.0} ms)");
            sb.AppendLine($"  everything above off: {all[0]:0.0} ms (the rest: the town, the terrain, the sky, the editor)");
            LastTable = sb.ToString();
            Debug.Log(sb.ToString());
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "..", "Logs", "perf_probe.txt"), sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman perf probe: the table could not be written: " + e.Message);
            }
            status = "";
            running = false;
        }

        static void Each(ParticleSystemRenderer[] rs, bool off)
        {
            foreach (var r in rs)
                if (r != null)
                    r.enabled = !off;
        }

        static void Toggle<T>(List<(string, Action, Action)> steps, VolumeProfile profile, string name, Func<T, BoolParameter> pick) where T : VolumeComponent
        {
            if (!profile.TryGet(out T c))
                return;
            var p = pick(c);
            bool was = p.value, over = p.overrideState;
            if (!was && over)
                return;                                                       // already off
            steps.Add((name, () => p.Override(false), () => { p.value = was; p.overrideState = over; }));
        }

        static VolumeProfile GlobalProfile()
        {
            foreach (var v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (v.isGlobal && v.profile != null)
                    return v.profile;                                         // (the runtime copy: the asset is never changed)
            return null;
        }
    }
}
