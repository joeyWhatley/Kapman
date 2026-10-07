using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>The land's height at a point, readable from any thread (for the shore's working-out).</summary>
    public interface ILandHeights
    {
        float HeightAt(double x, double z);
    }

    /// <summary>
    /// The scene's Unity terrains copied into plain arrays once (TerrainData.SampleHeight may only be asked on the main
    /// thread). Dorestad's 5 km square at 2.5 m is 4 million floats: 16 MB, read once.
    /// </summary>
    public sealed class TerrainHeights : ILandHeights
    {
        sealed class Patch
        {
            public float x0, z0, sx, sz, y0, sy;
            public int res;
            public float[] h;                                                // rows along z, columns along x, 0-1 of sy
        }

        readonly List<Patch> patches = new List<Patch>();

        /// <summary>Every active terrain in the scene.</summary>
        public static TerrainHeights FromScene()
        {
            var th = new TerrainHeights();
            foreach (var t in Terrain.activeTerrains)
            {
                var td = t.terrainData;
                if (td == null)
                    continue;
                int res = td.heightmapResolution;
                var hh = td.GetHeights(0, 0, res, res);                      // [z, x]
                var p = new Patch
                {
                    x0 = t.transform.position.x, z0 = t.transform.position.z, y0 = t.transform.position.y,
                    sx = td.size.x, sz = td.size.z, sy = td.size.y, res = res, h = new float[res * res],
                };
                for (int j = 0; j < res; j++)
                    for (int i = 0; i < res; i++)
                        p.h[j * res + i] = hh[j, i];
                th.patches.Add(p);
            }
            return th;
        }

        public int Terrains => patches.Count;

        public float HeightAt(double x, double z)
        {
            foreach (var p in patches)
            {
                double u = (x - p.x0) / p.sx, v = (z - p.z0) / p.sz;
                if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0)
                    continue;
                double fx = u * (p.res - 1), fz = v * (p.res - 1);
                int ix = Math.Min(p.res - 2, (int)fx), iz = Math.Min(p.res - 2, (int)fz);
                float tx = (float)(fx - ix), tz = (float)(fz - iz);
                int i = iz * p.res + ix;
                float a = p.h[i], b = p.h[i + 1], c = p.h[i + p.res], d = p.h[i + p.res + 1];
                float v01 = Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz);
                return p.y0 + v01 * p.sy;
            }
            return -100f;                                                    // off every terrain: deep water
        }
    }

    /// <summary>
    /// SHORE WATER - makes the water notice the land, round the ship:
    ///   - CALM WHERE IT SHOULD BE: the swell dies away in shallow water and near the shore, and up estuaries and
    ///     rivers it's all but gone; the wind's chop is smaller in sheltered water. (HDRP's water mask: red damps
    ///     the swell, green the chop, blue the ripples; on a River surface red is its agitation, so it takes the chop's rule.)
    ///   - FOAM: a line of foam where the water meets the land, strongest on open coasts and faint on sheltered
    ///     banks, breathing slowly as the swell comes in (a foam water decal with a texture made here).
    ///   - SURF: waves rolling in to the shore, steepening, breaking white and running up the sand, every few
    ///     seconds (HDRP's shore-wave water decals, one every 35 m along the shore, each turned to face it, as waves
    ///     turn to meet a beach). Their height follows the wind, and they're lower where the shore is sheltered.
    /// All worked out from the terrain's heights on a worker thread for a square round the ship, and again whenever
    /// she's moved a quarter of the way to its edge or the tide has moved the shoreline. The square keeps to a grid
    /// fixed on the level, so working it out again gives the same foam and surf in the same places; what the tide
    /// does change eases over (surf patches glide or fade, the foam cross-fades) - nothing on the shore ever jumps.
    /// Brought from the first build (its streamed land became the scene's terrains).
    /// </summary>
    public class ShoreWater : MonoBehaviour
    {
        public WaterSurface surface;
        [Tooltip("What the square follows: the player's ship (found at start if empty), else the camera.")]
        public Transform focus;
        [Tooltip("The foam decal's material: HDRP/Water/Water Decal Sample set to a texture of foam (HdrpWater makes it). Empty: no shore foam.")]
        public Material foamMaterial;

        [Header("Calm water")]
        [Tooltip("Metres across the square where the water knows about the land.")]
        public float maskSize = 1024f;
        [Tooltip("Texels across the calm-water mask.")]
        public int maskResolution = 256;
        [Tooltip("Metres of depth over which the swell comes back.")]
        public float swellDepth = 15f;
        [Tooltip("Metres from the shore over which the swell comes back.")]
        public float swellReach = 150f;
        [Tooltip("Metres from the shore over which the chop comes back, and how much is left right at the shore.")]
        public float chopReach = 40f;
        [Range(0f, 1f)] public float chopAtShore = 0.3f;
        [Tooltip("Metres round about over which the open water is judged. A river or a narrow sound (little room " +
                 "from the land anywhere near) gets no swell, small waves, a faint foam line and no surf.")]
        public float fetchWindow = 150f;
        [Tooltip("Metres of room from the land at which the water counts as fully open (a river 50 m across has 25).")]
        public float fetchFull = 120f;
        [Tooltip("How much of the chop a river keeps (the wind still ruffles it).")]
        [Range(0f, 1f)] public float riverChop = 0.04f;
        [Tooltip("How much of the ripples a river keeps.")]
        [Range(0f, 1f)] public float riverRipples = 0.2f;

        [Header("Foam")]
        [Tooltip("Metres across the square with shore foam.")]
        public float foamSize = 512f;
        [Tooltip("Texels across the foam texture.")]
        public int foamResolution = 512;
        [Tooltip("Metres of foam off the shore.")]
        public float foamWidth = 5f;
        [Range(0f, 1f)] public float foamStrength = 1f;
        [Tooltip("Seconds between swells coming in (the foam breathes with them).")]
        public float swellPeriod = 7f;

        [Header("Surf")]
        [Tooltip("HDRP's Water Decal Sample set to its shore wave, deformation and foam (HdrpWater makes it). Empty: no surf.")]
        public Material surfMaterial;
        [Tooltip("Metres between surf patches along the shore, and the most there are round the ship.")]
        public float surfSpacing = 35f;
        public int surfSpots = 28;
        [Tooltip("Metres of each surf patch: from the sea to the land, and along the shore.")]
        public Vector2 surfSize = new Vector2(40f, 48f);
        [Tooltip("Share of a surf patch that lies over the land (the waves run up it).")]
        [Range(0f, 0.5f)] public float surfOnLand = 0.2f;
        [Tooltip("Surf height in metres: at no wind, and for each metre a second of wind; at most.")]
        public float surfCalm = 0.15f, surfPerWind = 0.05f, surfMost = 1f;

        [Tooltip("Seconds a change on the shore takes to ease over (surf gliding or fading, foam cross-fading).")]
        public float easeSeconds = 2f;

        // the shore foam: two decals, the newest fading in over the older (so a change in the foam never pops)
        readonly WaterDecal[] foamDecals = new WaterDecal[2];
        readonly Texture2D[] foamTextures = new Texture2D[2];
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[2];
        readonly Vector2[] foamCentres = new Vector2[2];
        readonly float[] foamWeight = new float[2];
        int newestFoam = -1;
        Texture2D maskTexture;
        ILandHeights land;
        Vector2 centre;
        bool hasCentre;
        Task<Result> job;
        float workedTide;
        const float tideStep = 0.15f;                                       // metres of tide before the shore is worked out again

        // the surf patches: each keeps to its stretch of shore while that's still there, gliding to where the tide
        // puts the waterline, and fades in or out when a stretch comes into the square or leaves it
        WaterDecal[] surf;
        SurfSlot[] slots;

        sealed class SurfSlot
        {
            public bool used, leaving;
            public float weight;
            public double x, z, toX, toZ;                                    // where it is, and gliding to
            public float landX, landZ, toLandX, toLandZ, open, toOpen;
            public float size = 1f, height = 1f;                             // this patch's own (stable) variation: no two alike
        }

        sealed class Result
        {
            public Vector2 centre;
            public Color32[] mask, foam;
            public SurfSpot[] surf;
        }

        /// <summary>Where a surf patch goes: its middle, which way the land is, how open the water is.</summary>
        struct SurfSpot
        {
            public double x, z;
            public float toLandX, toLandZ, open;
        }

        void Start()
        {
            if (surface == null)
                surface = GetComponent<WaterSurface>();
            land = TerrainHeights.FromScene();
            if (focus == null)
            {
                var ship = ShipMotion.Player;
                focus = ship != null ? ship.transform : Camera.main != null ? Camera.main.transform : transform;
            }
            maskTexture = new Texture2D(maskResolution, maskResolution, TextureFormat.RGBA32, false, true)
                { name = "Shore water mask", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            if (foamMaterial != null)
                for (int k = 0; k < 2; k++)
                {
                    foamTextures[k] = new Texture2D(foamResolution, foamResolution, TextureFormat.RGBA32, false, true)
                        { name = "Shore foam " + k, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                    var go = new GameObject("Shore foam " + k + " (made by ShoreWater)");
                    go.transform.SetParent(transform, false);
                    var d = go.AddComponent<WaterDecal>();
                    d.material = foamMaterial;
                    d.regionSize = new Vector2(foamSize, foamSize);
                    d.resolution = new Vector2Int(foamResolution, foamResolution);
                    d.updateMode = CustomRenderTextureUpdateMode.OnDemand;
                    d.amplitude = 0f;
                    d.surfaceFoamDimmer = 0f;
                    d.deepFoamDimmer = 0f;
                    foamDecals[k] = d;
                    blocks[k] = new MaterialPropertyBlock();
                }
            if (surfMaterial != null)
            {
                // the surf patches share one material, so HDRP draws the rolling waves once for all of them
                var holder = new GameObject("Surf (made by ShoreWater)").transform;
                holder.SetParent(transform, false);
                surf = new WaterDecal[Mathf.Max(1, surfSpots)];
                slots = new SurfSlot[surf.Length];
                for (int i = 0; i < surf.Length; i++)
                {
                    var go = new GameObject("Surf " + i);
                    go.transform.SetParent(holder, false);
                    var d = go.AddComponent<WaterDecal>();
                    d.material = surfMaterial;
                    d.regionSize = surfSize;
                    d.resolution = new Vector2Int(128, 128);
                    d.updateMode = CustomRenderTextureUpdateMode.Realtime;    // the waves roll in: drawn every frame
                    d.amplitude = 0f;
                    d.surfaceFoamDimmer = 0f;
                    d.deepFoamDimmer = 0f;
                    go.SetActive(false);
                    surf[i] = d;
                    slots[i] = new SurfSlot();
                }
            }
        }

        /// <summary>
        /// Metres the square's middle keeps to: a whole number of the foam's texels, the mask's texels and the surf's
        /// stretches of shore, so every working-out lays them all on the same places in the level.
        /// </summary>
        float GridStep()
        {
            float foamTexel = foamSize / Mathf.Max(1, foamResolution);
            int mask = Mathf.Max(1, Mathf.RoundToInt(maskSize / Mathf.Max(1, maskResolution) / foamTexel));
            int cell = Mathf.Max(4, Mathf.RoundToInt(surfSpacing / foamTexel));
            return Lcm(mask, cell) * foamTexel;
        }

        static int Lcm(int a, int b)
        {
            int x = a, y = b;
            while (y != 0)
                (x, y) = (y, x % y);
            return a / x * b;
        }

        float WaterLevel => LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : (surface != null ? surface.transform.position.y : 0f);

        void Update()
        {
            if (surface == null || land == null || focus == null)
                return;
            var here = new Vector2(focus.position.x, focus.position.z);

            // a finished job: show it
            if (job != null && job.IsCompleted)
            {
                if (job.Status == TaskStatus.RanToCompletion)
                    Apply(job.Result);
                else if (job.Exception != null)
                    Debug.LogError("Kapman shore water: " + job.Exception.GetBaseException());
                job = null;
            }

            // far enough from the square's centre, or the tide has moved the shoreline: work it out again round her
            float reach = foamSize * 0.25f;
            bool moved = !hasCentre || Mathf.Abs(here.x - centre.x) > reach || Mathf.Abs(here.y - centre.y) > reach;
            float level = WaterLevel;
            if (job == null && (moved || Mathf.Abs(level - workedTide) > tideStep))
            {
                var src = land;
                float step = GridStep();
                var at = moved ? new Vector2(Mathf.Round(here.x / step) * step, Mathf.Round(here.y / step) * step) : centre;
                var settings = (maskSize, maskResolution, swellDepth, swellReach, chopReach, chopAtShore,
                                foamSize, foamResolution, foamWidth, fetchWindow, fetchFull, riverChop, riverRipples);
                float spacing = surfSpacing, off = surfSize.x * (0.5f - surfOnLand);
                int most = surf != null ? surf.Length : 0;
                bool river = surface.surfaceType == WaterSurfaceType.River;
                workedTide = level;
                job = Task.Run(() => Work(src, at, settings, spacing, off, most, level, river));
            }

            if (hasCentre)
            {
                surface.waterMaskOffset = centre;
                PlaceFoam();
                PlaceSurf();
            }
        }

        float Ease => 1f - Mathf.Exp(-Time.deltaTime * 3f / Mathf.Max(0.1f, easeSeconds));

        /// <summary>The shore foam on its place, breathing with the swell; the newest fading in over the older.</summary>
        void PlaceFoam()
        {
            if (newestFoam < 0)
                return;
            float breathe = 0.78f + 0.22f * Mathf.Sin(Time.time * 2f * Mathf.PI / Mathf.Max(1f, swellPeriod));
            float step = Time.deltaTime / Mathf.Max(0.1f, easeSeconds);
            float level = WaterLevel;
            for (int k = 0; k < 2; k++)
            {
                var d = foamDecals[k];
                if (d == null)
                    continue;
                foamWeight[k] = Mathf.MoveTowards(foamWeight[k], k == newestFoam ? 1f : 0f, step);
                float w = Mathf.SmoothStep(0f, 1f, foamWeight[k]);
                bool on = foamWeight[k] > 0f;
                if (d.gameObject.activeSelf != on)
                    d.gameObject.SetActive(on);
                if (!on)
                    continue;
                d.transform.position = new Vector3(foamCentres[k].x, level, foamCentres[k].y);
                d.surfaceFoamDimmer = foamStrength * breathe * w;
                d.deepFoamDimmer = foamStrength * breathe * 0.4f * w;
            }
        }

        /// <summary>
        /// The surf patches on their places along the shore, each turned so its waves roll in square to the shore,
        /// as high as the wind and the open water make them; each gliding to where the waterline now is, and fading
        /// in or out as its stretch of shore comes or goes.
        /// </summary>
        void PlaceSurf()
        {
            if (surf == null)
                return;
            var weather = LevelWeather.Current;
            float metres = Mathf.Min(surfMost, surfCalm + surfPerWind * (weather != null ? weather.Wind : 5f));
            float ice = weather != null ? weather.Ice : 0f;
            float glide = Ease, step = Time.deltaTime / Mathf.Max(0.1f, easeSeconds);
            float level = WaterLevel;
            for (int i = 0; i < surf.Length; i++)
            {
                var d = surf[i];
                var s = slots[i];
                if (s.used)
                {
                    s.weight = Mathf.MoveTowards(s.weight, s.leaving ? 0f : 1f, step);
                    if (s.leaving && s.weight <= 0f)
                        s.used = false;
                }
                if (d.gameObject.activeSelf != s.used)
                    d.gameObject.SetActive(s.used);
                if (!s.used)
                    continue;
                s.x += (s.toX - s.x) * glide;
                s.z += (s.toZ - s.z) * glide;
                s.landX = Mathf.Lerp(s.landX, s.toLandX, glide);
                s.landZ = Mathf.Lerp(s.landZ, s.toLandZ, glide);
                s.open = Mathf.Lerp(s.open, s.toOpen, glide);
                // the decal's own x runs from the sea to the land; its waves roll that way
                float yaw = Mathf.Atan2(-s.landZ, s.landX) * Mathf.Rad2Deg;
                d.transform.SetPositionAndRotation(new Vector3((float)s.x, level, (float)s.z), Quaternion.Euler(0f, yaw, 0f));
                d.regionSize = new Vector2(surfSize.x, surfSize.y * s.size);
                float w = Mathf.SmoothStep(0f, 1f, s.weight) * (1f - ice);
                float here = metres * Mathf.Sqrt(s.open) * w * s.height;
                d.amplitude = here * 2f;                                     // (HDRP's shore wave peaks at half its amplitude)
                d.surfaceFoamDimmer = Mathf.Clamp01(0.4f + here) * w;
                d.deepFoamDimmer = Mathf.Clamp01(0.25f + 0.5f * here) * w;
            }
        }

        void Apply(Result r)
        {
            centre = r.centre;
            hasCentre = true;
            maskTexture.SetPixels32(r.mask);
            maskTexture.Apply(false);
            surface.simulationMask = true;
            surface.waterMask = maskTexture;
            surface.waterMaskExtent = new Vector2(maskSize, maskSize);
            surface.waterMaskRemap = new Vector2(0f, 1f);
            if (foamDecals[0] != null)
            {
                // into the decal that's fading (or faded) out; it then fades in over the other
                int k = newestFoam < 0 ? 0 : 1 - newestFoam;
                foamTextures[k].SetPixels32(r.foam);
                foamTextures[k].Apply(false);
                WaterDecalKit.SetTextures(foamDecals[k], blocks[k], Texture2D.blackTexture, foamTextures[k]);
                foamCentres[k] = r.centre;
                if (newestFoam < 0)
                    foamWeight[k] = 1f;                                      // the first: straight in
                newestFoam = k;
            }
            MatchSurf(r.surf ?? new SurfSpot[0]);
        }

        /// <summary>
        /// Hands the new surf spots to the patches: a patch whose stretch of shore is still there glides to its new
        /// spot; one whose stretch has gone fades out; a new stretch gets a free patch that fades in.
        /// </summary>
        void MatchSurf(SurfSpot[] spots)
        {
            if (slots == null)
                return;
            double near = surfSpacing * 0.5;
            var taken = new bool[spots.Length];
            foreach (var s in slots)
            {
                if (!s.used || s.leaving)
                    continue;
                int best = -1;
                double bestD = near * near;
                for (int j = 0; j < spots.Length; j++)
                {
                    if (taken[j])
                        continue;
                    double dx = spots[j].x - s.toX, dz = spots[j].z - s.toZ, d = dx * dx + dz * dz;
                    if (d < bestD)
                    {
                        bestD = d;
                        best = j;
                    }
                }
                if (best < 0)
                {
                    s.leaving = true;
                    continue;
                }
                taken[best] = true;
                Aim(s, spots[best]);
            }
            for (int j = 0; j < spots.Length; j++)
            {
                if (taken[j])
                    continue;
                var free = Array.Find(slots, s => !s.used);
                if (free == null)
                    break;
                free.used = true;
                free.leaving = false;
                free.weight = 0f;
                Aim(free, spots[j]);
                free.x = free.toX;
                free.z = free.toZ;
                free.landX = free.toLandX;
                free.landZ = free.toLandZ;
                free.open = free.toOpen;
                // its own size and height, from its place on the shore (the same patch of shore is always the same)
                uint h = (uint)(Math.Floor(free.toX / 7.0) * 73856093.0 % 4294967296.0) ^ (uint)(Math.Floor(free.toZ / 7.0) * 19349663.0 % 4294967296.0);
                h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
                free.size = 0.75f + 0.5f * (h & 0xffff) / 65535f;
                free.height = 0.65f + 0.7f * ((h >> 16) & 0xffff) / 65535f;
            }
        }

        static void Aim(SurfSlot s, SurfSpot spot)
        {
            s.toX = spot.x;
            s.toZ = spot.z;
            s.toLandX = spot.toLandX;
            s.toLandZ = spot.toLandZ;
            s.toOpen = spot.open;
        }

        // ------------------------------------------------------------------------------------------ the worker

        static Result Work(ILandHeights land, Vector2 at,
                           (float maskSize, int maskRes, float swellDepth, float swellReach, float chopReach, float chopAtShore,
                            float foamSize, int foamRes, float foamWidth, float fetchWindow, float fetchFull, float riverChop,
                            float riverRipples) s,
                           float surfSpacing, float surfOffSea, int surfMost, float tide, bool riverSurface)
        {
            var r = new Result { centre = at };

            // the calm-water mask
            {
                int n = s.maskRes;
                float texel = s.maskSize / n;
                var depth = Heights(land, at, n, texel, tide, out var dry);
                var dist = DistanceToLand(dry, n);
                // how open the water round about is: the most room from the land within the window (a river 50 m
                // across has 25 m; the open sea the whole window). Waves need room to grow; the swell can't get in.
                var room = MaxFilter(dist, n, Mathf.Max(1, Mathf.RoundToInt(s.fetchWindow * 0.5f / texel)));
                r.mask = new Color32[n * n];
                for (int i = 0; i < n * n; i++)
                {
                    if (dry[i])
                    {
                        r.mask[i] = new Color32(0, 0, 255, 255);
                        continue;
                    }
                    float d = dist[i] * texel;
                    float open = Sat((room[i] * texel - 10f) / Mathf.Max(1f, s.fetchFull - 10f));
                    float swell = Sat((depth[i] - 1f) / s.swellDepth) * Sat(d / s.swellReach) * open * open;
                    float chop = Sat(depth[i] / 2f) * Mathf.Lerp(s.chopAtShore, 1f, Sat(d / s.chopReach)) *
                                 Mathf.Lerp(s.riverChop, 1f, open);
                    float ripples = Mathf.Lerp(s.riverRipples, 1f, open);
                    // a river surface has no swell band: its first band is its agitation, which takes the chop's rule
                    r.mask[i] = new Color32(WaterDecalKit.B(riverSurface ? Mathf.Max(chop, 0.5f) : swell), WaterDecalKit.B(chop), WaterDecalKit.B(ripples), 255);
                }
            }

            // the shore foam: a band off the shore, strongest where there's open water beyond it (a river's banks get
            // none: rivers don't break on their banks)
            {
                int n = s.foamRes;
                float texel = s.foamSize / n;
                var depthHere = Heights(land, at, n, texel, tide, out var dry);
                var dist = DistanceToLand(dry, n);
                var room = MaxFilter(dist, n, Mathf.Max(1, Mathf.RoundToInt(s.fetchWindow * 0.5f / texel)));
                // the channel's own width (a creek through flooded flats is still a creek: its banks are metres apart)
                var localRoom = MaxFilter(dist, n, Mathf.Max(1, Mathf.RoundToInt(8f / texel)));
                // and how deep the water gets just off the shore: waves break on a beach that shelves into deep water,
                // not on half a metre of water over the tidal flats
                var offshore = MaxFilter(depthHere, n, Mathf.Max(1, Mathf.RoundToInt(15f / texel)));
                var open = new float[n * n];                                   // 0 = a narrow river, 1 = open water
                for (int i = 0; i < open.Length; i++)
                    open[i] = Sat((room[i] * texel - 15f) / Mathf.Max(1f, s.fetchFull - 15f))
                              * Sat((localRoom[i] * texel - 2f) / 5f) * Sat((offshore[i] - 0.5f) / 2f);
                r.foam = new Color32[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        int i = y * n + x;
                        if (dry[i])
                            continue;
                        float d = dist[i] * texel;
                        float band = 1f - Sat((d - 0.5f * texel) / s.foamWidth);
                        if (band <= 0f)
                            continue;
                        double wx = at.x + (x + 0.5 - n * 0.5) * texel, wz = at.y + (y + 0.5 - n * 0.5) * texel;
                        float breakup = 0.45f + 0.55f * (float)WaterDecalKit.Noise(wx / 40.0, wz / 40.0, 7);
                        float f = Mathf.Min(1f, 1.4f * band * (open[i] < 0.2f ? 0f : Mathf.Sqrt(open[i]) * open[i]) * breakup);   // (no foam line on a river)
                        r.foam[i] = new Color32(WaterDecalKit.B(f), WaterDecalKit.B(f * 0.6f), 0, 255);
                    }
                r.surf = surfMost > 0 ? SurfSpots(dist, open, dry, n, texel, at, surfSpacing, surfOffSea, surfMost) : new SurfSpot[0];
            }
            return r;
        }

        /// <summary>
        /// Where the surf goes: along the shore, one patch per stretch of about <paramref name="spacing"/> metres, where
        /// open water comes in to it (a sheltered bank or a river gets none), each facing the land. The nearest the ship first.
        /// </summary>
        static SurfSpot[] SurfSpots(float[] dist, float[] open, bool[] dry, int n, float texel, Vector2 at,
                                    float spacing, float offSea, int most)
        {
            var found = new List<(SurfSpot spot, float fromMiddle)>();
            int cell = Math.Max(4, Mathf.RoundToInt(spacing / texel));
            int round = Math.Max(2, Mathf.RoundToInt(4f / texel));            // metres either side for the land's direction
            for (int cy = 0; cy < n; cy += cell)
                for (int cx = 0; cx < n; cx += cell)
                {
                    // the shore texel in this stretch nearest its middle: water right against the land
                    int best = -1;
                    float bestD = float.MaxValue;
                    int mx = cx + cell / 2, my = cy + cell / 2;
                    for (int y = cy; y < Math.Min(n, cy + cell); y++)
                        for (int x = cx; x < Math.Min(n, cx + cell); x++)
                        {
                            int i = y * n + x;
                            if (dry[i] || dist[i] > 1.5f)
                                continue;
                            float d = (x - mx) * (x - mx) + (y - my) * (y - my);
                            if (d < bestD)
                            {
                                bestD = d;
                                best = i;
                            }
                        }
                    if (best < 0)
                        continue;
                    int bx = best % n, by = best / n;
                    // which way the land lies: down the slope of the distance to it, measured a few metres round
                    int x0 = Math.Max(0, bx - round), x1 = Math.Min(n - 1, bx + round);
                    int y0 = Math.Max(0, by - round), y1 = Math.Min(n - 1, by + round);
                    float gx = (dist[by * n + x1] - dist[by * n + x0]) / Math.Max(1, x1 - x0);
                    float gz = (dist[y1 * n + bx] - dist[y0 * n + bx]) / Math.Max(1, y1 - y0);
                    float len = (float)Math.Sqrt(gx * gx + gz * gz);
                    if (len < 0.2f)
                        continue;                                                // a point, a spit or a pool: no clear shore
                    float lx = -gx / len, lz = -gz / len;
                    float openness = open[best];
                    if (openness < 0.5f)
                        continue;                                                // sheltered water or a river: no surf to speak of
                    // the patch's middle lies out to sea from the waterline, so its landward end runs up the shore
                    double wx = at.x + (bx + 0.5 - n * 0.5) * texel - lx * offSea;
                    double wz = at.y + (by + 0.5 - n * 0.5) * texel - lz * offSea;
                    float fromMiddle = (bx - n * 0.5f) * (bx - n * 0.5f) + (by - n * 0.5f) * (by - n * 0.5f);
                    found.Add((new SurfSpot { x = wx, z = wz, toLandX = lx, toLandZ = lz, open = openness }, fromMiddle));
                }
            found.Sort((a, b) => a.fromMiddle.CompareTo(b.fromMiddle));
            var spots = new SurfSpot[Math.Min(most, found.Count)];
            for (int i = 0; i < spots.Length; i++)
                spots[i] = found[i].spot;
            return spots;
        }

        /// <summary>
        /// Water depth (metres, 0 on land) over an n x n square of texels round a place, rows south to north, with the
        /// water at <paramref name="tide"/> metres (the flats dry at low water).
        /// </summary>
        static float[] Heights(ILandHeights land, Vector2 at, int n, float texel, float tide, out bool[] dry)
        {
            var depth = new float[n * n];
            dry = new bool[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    double wx = at.x + (x + 0.5 - n * 0.5) * texel;
                    double wz = at.y + (y + 0.5 - n * 0.5) * texel;
                    float h = land.HeightAt(wx, wz);
                    dry[y * n + x] = h > tide + 0.05f;
                    depth[y * n + x] = Math.Max(0f, tide - h);
                }
            return depth;
        }

        /// <summary>Distance (texels) from each wet texel to the nearest dry one: a two-pass chamfer (3-4) transform.</summary>
        static float[] DistanceToLand(bool[] dry, int n)
        {
            const float big = 1e6f, a = 1f, b = 1.4142f;
            var d = new float[n * n];
            for (int i = 0; i < d.Length; i++)
                d[i] = dry[i] ? 0f : big;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int i = y * n + x;
                    if (d[i] == 0f) continue;
                    float v = d[i];
                    if (x > 0) v = Math.Min(v, d[i - 1] + a);
                    if (y > 0)
                    {
                        v = Math.Min(v, d[i - n] + a);
                        if (x > 0) v = Math.Min(v, d[i - n - 1] + b);
                        if (x < n - 1) v = Math.Min(v, d[i - n + 1] + b);
                    }
                    d[i] = v;
                }
            for (int y = n - 1; y >= 0; y--)
                for (int x = n - 1; x >= 0; x--)
                {
                    int i = y * n + x;
                    if (d[i] == 0f) continue;
                    float v = d[i];
                    if (x < n - 1) v = Math.Min(v, d[i + 1] + a);
                    if (y < n - 1)
                    {
                        v = Math.Min(v, d[i + n] + a);
                        if (x < n - 1) v = Math.Min(v, d[i + n + 1] + b);
                        if (x > 0) v = Math.Min(v, d[i + n - 1] + b);
                    }
                    d[i] = v;
                }
            // no land in the square at all: everything counts as far
            for (int i = 0; i < d.Length; i++)
                if (d[i] >= big) d[i] = n;
            return d;
        }

        /// <summary>
        /// The largest value within r texels (a square window), in two passes, each in linear time whatever r is (van
        /// Herk's method: running maxima forward and back inside blocks as long as the window).
        /// </summary>
        static float[] MaxFilter(float[] v, int n, int r)
        {
            var tmp = new float[n * n];
            var o = new float[n * n];
            var line = new float[n];
            var res = new float[n];
            for (int y = 0; y < n; y++)
            {
                Array.Copy(v, y * n, line, 0, n);
                MaxLine(line, res, n, r);
                Array.Copy(res, 0, tmp, y * n, n);
            }
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++) line[y] = tmp[y * n + x];
                MaxLine(line, res, n, r);
                for (int y = 0; y < n; y++) o[y * n + x] = res[y];
            }
            return o;
        }

        static void MaxLine(float[] a, float[] o, int n, int r)
        {
            int w = 2 * r + 1;
            var fwd = new float[n];
            var back = new float[n];
            for (int i = 0; i < n; i++)
                fwd[i] = i % w == 0 ? a[i] : Math.Max(fwd[i - 1], a[i]);
            for (int i = n - 1; i >= 0; i--)
                back[i] = i == n - 1 || (i + 1) % w == 0 ? a[i] : Math.Max(back[i + 1], a[i]);
            for (int i = 0; i < n; i++)
                o[i] = Math.Max(back[Math.Max(0, i - r)], fwd[Math.Min(n - 1, i + r)]);
        }

        static float Sat(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
