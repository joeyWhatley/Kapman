using System.IO;
using Kapman.Weather;
using Kapman.World;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// THE WATER'S LEVEL - the river or the sea at its resting level, plus the tide and any flood. The tide comes from the
    /// first build's EOT20 model at a point on the coast, damped and late as far up the river as the level lies (for
    /// Dorestad c. 800 the share and the lag are guesses, still to check). A flood raises the water over the banks.
    /// The surfaces are HDRP water surfaces (HdrpWater makes them): the whole surface rises and falls with its
    /// transform. The flood is the SAME river surface raised: while the water stands over the banks, the surface draws
    /// the flood sheet's mesh (a sheet over the whole level, the terrain hiding it wherever the land is higher) instead
    /// of the river's own mesh, so the flood has the river's colour, waves and stream. Older plain-transform surfaces
    /// (no WaterSurface) are still just moved, and a plain flood sheet shown and hidden.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-140)]
    public class LevelWater : MonoBehaviour
    {
        [Tooltip("The water surfaces, raised and lowered with the water.")]
        public Transform[] surfaces;
        [Tooltip("A sheet over the whole level, shown when the water rises over the banks.")]
        public Transform floodSheet;
        [Tooltip("How far the worst flood raises the water (m).")]
        public float floodRise = 2.2f;

        [Header("The tide")]
        public bool tides = true;
        public string tidesFile = @"G:\kapman_unity\data\baked\world\tides.bin";
        [Tooltip("Where on the coast the tide is taken (degrees): the Rhine's mouth at Katwijk.")]
        public double tideLatitude = 52.21, tideLongitude = 4.38;
        [Tooltip("How much of the coast's tide reaches this far up the river (0..1).")]
        [Range(0f, 1f)] public float tideShare = 0.15f;
        [Tooltip("How many hours later the tide comes here than at the coast.")]
        public float tideLagHours = 5f;

        /// <summary>The tide's part of the level now (m).</summary>
        public float Tide { get; private set; }

        /// <summary>How fast the tide is rising (+) or falling (-), metres per game hour.</summary>
        public float TideRate { get; private set; }

        /// <summary>The level's water (the one with the tide).</summary>
        public static LevelWater Current { get; private set; }

        TideModel model;
        bool tried;
        double rateHours = double.NaN;
        float rateTide;

        // the HDRP surface and its two meshes (the river's, the flood's)
        WaterSurface hdrp;
        MeshRenderer riverRenderer, floodRenderer;
        bool floodShown;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            Resolve();
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        /// <summary>Finds the HDRP surface among the surfaces, and the flood sheet's mesh renderer (HDRP draws it: the renderer itself stays off).</summary>
        void Resolve()
        {
            hdrp = null;
            riverRenderer = null;
            floodRenderer = null;
            if (surfaces != null)
                foreach (var s in surfaces)
                    if (s != null && s.TryGetComponent(out WaterSurface ws))
                    {
                        hdrp = ws;
                        break;
                    }
            if (hdrp == null)
                return;
            if (hdrp.geometryType == WaterGeometryType.Custom && hdrp.meshRenderers != null && hdrp.meshRenderers.Count > 0)
                riverRenderer = hdrp.meshRenderers[0];
            if (floodSheet != null && floodSheet.TryGetComponent(out MeshRenderer fr))
            {
                floodRenderer = fr;
                floodRenderer.enabled = false;
                if (!floodSheet.gameObject.activeSelf)
                    floodSheet.gameObject.SetActive(true);
            }
            floodShown = riverRenderer != null && floodRenderer != null && hdrp.meshRenderers.Contains(floodRenderer);
        }

        void Update()
        {
            var info = LevelInfo.Current;
            if (info == null)
                return;
            float tide = 0f;
            var clock = LevelClock.Current;
            if (tides && clock != null)
            {
                if (!tried)
                {
                    tried = true;
                    try
                    {
                        if (File.Exists(tidesFile))
                            model = TideModel.Load(tidesFile);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Kapman water: no tides (" + e.Message + ")");
                    }
                }
                if (model != null)
                {
                    double jd = Sky.ToUt(clock.JulianDay, tideLongitude) - tideLagHours / 24.0;
                    float h = model.HeightAt(tideLatitude, tideLongitude, jd);
                    if (!float.IsNaN(h))
                        tide = h * tideShare;
                }
                // the tide's rate: from the change over the last quarter of a game hour
                double hours = clock.GameHours;
                if (double.IsNaN(rateHours) || hours < rateHours)
                {
                    rateHours = hours;
                    rateTide = tide;
                }
                else if (hours - rateHours >= 0.25)
                {
                    TideRate = (float)((tide - rateTide) / (hours - rateHours));
                    rateHours = hours;
                    rateTide = tide;
                }
            }
            Tide = tide;
            var weather = LevelWeather.Current;
            float flood = weather != null ? weather.flood * floodRise : 0f;
            float level = info.baseWaterLevel + tide + flood;
            info.WaterLevel = level;
            if (surfaces != null)
                foreach (var s in surfaces)
                    if (s != null)
                        s.position = new Vector3(s.position.x, level, s.position.z);
            if (floodSheet == null)
                return;
            bool show = flood > 0.05f;
            if (hdrp == null || (riverRenderer == null && floodRenderer == null))
                Resolve();
            if (hdrp != null && riverRenderer != null && floodRenderer != null)
            {
                // the same surface, drawing the flood sheet's mesh instead of the river's while the water is over the banks
                if (show != floodShown)
                {
                    hdrp.meshRenderers.Clear();
                    hdrp.meshRenderers.Add(show ? floodRenderer : riverRenderer);
                    floodShown = show;
                }
                if (floodSheet.parent != hdrp.transform)
                    floodSheet.position = new Vector3(floodSheet.position.x, level, floodSheet.position.z);
            }
            else
            {
                if (floodSheet.gameObject.activeSelf != show)
                    floodSheet.gameObject.SetActive(show);
                floodSheet.position = new Vector3(floodSheet.position.x, level, floodSheet.position.z);
            }
        }
    }
}
