using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// THE RIVER'S STREAM - the water on a river is seen to flow: HDRP's current map turns the surface's agitation and
    /// ripples along the channel and drifts them downstream, and flecks of foam drift down with them. The map is baked
    /// by the level builder (HdrpWater.MakeRiver) from the channel line: the stream runs along the line's tangent, full
    /// in mid-channel, slack over the harbour's shoal and in the pools and old channel scars. HDRP 17.6's map carries a
    /// direction only (one speed for the whole surface), so the slack is in this component's own copy of the map (the
    /// alpha): the drifting foam and anything that asks Drift() feel it. A rising tide holds the stream back and can turn
    /// it; a falling tide hurries it out. The stream doesn't push the ship unless pushesShips is on (the design's choice:
    /// the current is seen, the ship is rowed). Brought from the first build's RiverFlow.
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    [DefaultExecutionOrder(-110)]
    public class RiverCurrent : MonoBehaviour
    {
        [Tooltip("The current map (HdrpWater.MakeRiver bakes it): red and green the stream's direction (0.5 = none), blue 1 " +
                 "where there's water, alpha the stream's share of the full speed (0 in slack water).")]
        public Texture2D currentMap;
        [Tooltip("Metres across the map, centred on the surface's own origin (x and z).")]
        public float mapExtent = 10240f;
        [Tooltip("The stream's speed in mid-channel (m/s): a lowland river's 0.3-0.5.")]
        public float speed = 0.4f;
        [Tooltip("How fast the water is DRAWN drifting against its real speed (a little faster reads as a river's flow).")]
        public float visualSpeed = 1.5f;
        [Tooltip("The stream carries ships with it (off: it is only seen).")]
        public bool pushesShips;
        [Tooltip("How much the tide holds the stream back (0 none, 1 fully).")]
        [Range(0f, 1f)] public float tideEffect = 1f;
        [Tooltip("A tide rising this fast (m per game hour) would stop the stream altogether.")]
        public float tideRiseThatStops = 0.25f;

        [Header("Foam flecks drifting down the stream")]
        [Tooltip("HDRP/Water/Water Decal Sample, Texture type, foam only (HdrpWater makes it). Empty: no flecks.")]
        public Material foamMaterial;
        [Tooltip("How many flecks of foam drift on the river round the camera (0: none).")]
        public int flecks = 24;
        [Tooltip("Metres round the camera they're scattered over, and the seconds each lasts.")]
        public float fleckReach = 70f, fleckSeconds = 14f;
        [Tooltip("How white they are, and the stream's speed (m/s) under which there are none.")]
        [Range(0f, 1f)] public float fleckFoam = 0.7f;
        public float fleckFromSpeed = 0.12f;

        WaterSurface surface;
        Transform fleckRoot;
        WaterDecal[] fleck;
        float[] fleckBorn, fleckSize;
        Material fleckMaterial;

        /// <summary>The level's river stream (null on the sea).</summary>
        public static RiverCurrent Current { get; private set; }

        /// <summary>The tide's factor on the stream now (1 = the river's own speed, 0 slack, below 0 running upstream).</summary>
        public float Factor { get; private set; } = 1f;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            surface = GetComponent<WaterSurface>();
            Apply();
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        void Start()
        {
            MakeFlecks();
        }

        void OnDestroy()
        {
            if (fleckRoot != null)
                Destroy(fleckRoot.gameObject);
            if (fleckMaterial != null)
                Destroy(fleckMaterial);
        }

        /// <summary>Puts the map on the surface (the editor builder calls it too, so the scene is saved with it).</summary>
        public void Apply()
        {
            if (surface == null)
                surface = GetComponent<WaterSurface>();
            surface.supportLargeCurrent = currentMap != null;
            surface.largeCurrentMap = currentMap;
            surface.largeCurrentRegionExtent = new Vector2(mapExtent, mapExtent);
            surface.largeCurrentRegionOffset = Vector2.zero;                // (centred: HDRP's CPU and GPU disagree on the offset's sign)
            surface.largeCurrentMapInfluence = 1f;
            surface.largeOrientationValue = 0f;                             // the map's directions are absolute
            surface.ripplesMotionMode = WaterPropertyOverrideMode.Inherit;  // the ripples drift with the stream too
            surface.largeCurrentSpeedValue = speed * visualSpeed * 3.6f;
        }

        void Update()
        {
            if (surface == null)
                return;
            // the tide: rising holds the stream back (near the sea it turns it), falling hurries it out
            var water = LevelWater.Current;
            float rise = water != null && water.tides ? water.TideRate : 0f;
            float target = Mathf.Clamp(1f - tideEffect * rise / Mathf.Max(0.01f, tideRiseThatStops), -0.5f, 1.5f);
            Factor = Mathf.Lerp(Factor, target, 1f - Mathf.Exp(-Time.deltaTime / 3f));
            float ice = LevelWeather.Current != null ? LevelWeather.Current.Ice : 0f;
            float drawn = speed * visualSpeed * 3.6f * Factor * (1f - ice);
            surface.largeCurrentSpeedValue = drawn;
            surface.ripplesCurrentSpeedValue = drawn;
            surface.largeOrientationValue = 0f;
            UpdateFlecks(Time.deltaTime * (LevelClock.Paused ? 1f : LevelClock.Speed), ice);
        }

        /// <summary>The map at a point: the stream's direction (x east, z north, unit length or zero) and its share of the full speed.</summary>
        public bool Sample(Vector3 p, out Vector3 direction, out float share, out bool water)
        {
            direction = Vector3.zero;
            share = 0f;
            water = false;
            if (currentMap == null || mapExtent <= 0f)
                return false;
            float u = p.x / mapExtent + 0.5f, v = p.z / mapExtent + 0.5f;
            if (u < 0f || u > 1f || v < 0f || v > 1f)
                return false;
            Color c = currentMap.GetPixelBilinear(u, v);
            var d = new Vector3(c.r * 2f - 1f, 0f, c.g * 2f - 1f);
            water = c.b > 0.5f;
            share = c.a;
            direction = d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.zero;
            return true;
        }

        /// <summary>The water's drift at a point (m/s, level): what lies on the water goes with it.</summary>
        public Vector3 Drift(Vector3 p)
        {
            Sample(p, out var dir, out float share, out _);
            float ice = LevelWeather.Current != null ? LevelWeather.Current.Ice : 0f;
            return dir * (speed * share * Factor * (1f - ice));
        }

        // ------------------------------------------------------------------------------------------ the flecks

        void MakeFlecks()
        {
            if (flecks <= 0 || foamMaterial == null)
                return;
            fleckRoot = new GameObject("Foam drifting down the river (RiverCurrent)").transform;
            fleckMaterial = new Material(foamMaterial) { name = "Foam on the stream" };
            fleckMaterial.SetTexture(WaterDecalKit.FoamTexture, WaterDecalKit.FoamPatch());
            fleckMaterial.SetTexture(WaterDecalKit.DeformationTexture, Texture2D.blackTexture);
            fleck = new WaterDecal[flecks];
            fleckBorn = new float[flecks];
            fleckSize = new float[flecks];
            for (int i = 0; i < flecks; i++)
            {
                fleck[i] = WaterDecalKit.MakeDecal("Fleck " + i, fleckRoot, fleckMaterial, Vector2.one, new Vector2Int(32, 32));
                fleckBorn[i] = -1000f;
            }
        }

        /// <summary>Each fleck lasts a while, drifting with the water, coming up and fading away; a spent one comes up
        /// again somewhere on the stream round the camera.</summary>
        void UpdateFlecks(float dt, float ice)
        {
            if (fleck == null)
                return;
            float t = Time.time, life = Mathf.Max(2f, fleckSeconds);
            var cam = Camera.main;
            float level = LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : transform.position.y;
            for (int i = 0; i < fleck.Length; i++)
            {
                var d = fleck[i];
                float age = t - fleckBorn[i];
                if (age >= life && cam != null && ice < 0.5f)
                {
                    // a new one, somewhere on the stream near the camera (a few tries; else it waits)
                    var c = cam.transform.position;
                    for (int tries = 0; tries < 4; tries++)
                    {
                        var p = new Vector3(c.x + Random.Range(-fleckReach, fleckReach), 0f, c.z + Random.Range(-fleckReach, fleckReach));
                        if (Sample(p, out _, out float share, out bool water) && water && speed * share * Mathf.Abs(Factor) >= fleckFromSpeed)
                        {
                            d.transform.SetPositionAndRotation(new Vector3(p.x, level, p.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                            fleckBorn[i] = t - Random.Range(0f, 0.3f * life);
                            fleckSize[i] = Random.Range(0.5f, 1.6f);
                            age = t - fleckBorn[i];
                            break;
                        }
                    }
                }
                float f = age / life;
                if (f < 0f || f >= 1f)
                {
                    d.surfaceFoamDimmer = 0f;
                    d.deepFoamDimmer = 0f;
                    continue;
                }
                d.transform.position += Drift(d.transform.position) * dt;
                // up over the first fifth, gone over the last third
                float s01 = fleckFoam * Mathf.SmoothStep(0f, 1f, f / 0.2f) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.67f, 1f, f)));
                d.surfaceFoamDimmer = Mathf.Clamp01(0.5f * s01);
                d.deepFoamDimmer = Mathf.Clamp01(0.2f * s01);
                d.regionSize = new Vector2(fleckSize[i], fleckSize[i] * 1.6f) * (1f + 0.03f * age);
            }
        }
    }
}
