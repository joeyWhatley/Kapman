using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// SHIP WAKE - the water answering the ship, on HDRP's water (only HDRP's water takes water decals):
    ///   - HER WAVES: the bow wave heaped up at her stem, the hollow along her sides, the hump of water closing in under
    ///     her stern, and behind her the Kelvin wake every ship trails: the V of waves 19.5 degrees each side of her
    ///     track with the crosswise waves inside it, as long as her speed makes them (2 pi v^2 / g: 1.6 m when rowed at
    ///     1.6 m/s, 5 m under sail at 2.8). The waves are worked out once, on a worker thread, for a ladder of speeds
    ///     (a water decal texture each: heights and foam); at any speed the two nearest are blended by amplitude, so
    ///     her wake grows smoothly as she gathers way and never jumps from one set to the next. No custom shader.
    ///   - FOAM: white water off the bow wave and along her sides, churned water under her stern, and a trail along
    ///     the way she's really come that spreads and fades (foam decals; the water keeps foam for a while). On a river
    ///     the trail drifts away downstream with the stream.
    ///   - WHERE THE WATER STRIKES HER: the water is felt at points along both sides and at her stem. Where it surges
    ///     up against her, it heaps against the planking there, breaks white, and throws a burst of spray droplets
    ///     (hardest when she puts her bow into a sea). So each part of her answers the waves that reach it.
    /// Her waves and foam grow with her speed; the strikes come with the sea. HdrpWater.DressShip adds this with its
    /// materials. Brought from the first build (its GPU-drawn waves became the speed ladder).
    /// </summary>
    public class ShipWake : MonoBehaviour
    {
        [Tooltip("Her motion (found on this object if empty).")]
        public ShipMotion ship;

        [Header("Her hull at the waterline (metres from her pivot, forward +)")]
        public float bowZ = 3.4f;
        public float sternZ = -3.4f;
        [Tooltip("Half her breadth at the waterline, metres.")]
        public float halfBeam = 0.75f;
        [Tooltip("Her freeboard amidships (m): a strike's heap never tops her gunwale.")]
        public float freeboard = 0.34f;
        [Tooltip("Her steering oar, and where its blade cuts the waterline in the oar's own space (the builder measures it). " +
                 "The water breaks white on the blade; empty = no oar.")]
        public Transform steeringOar;
        public Vector3 oarBlade;

        [Header("Her waves")]
        [Tooltip("HDRP's Water Decal Sample, Texture type, deformation and foam (HdrpWater makes it).")]
        public Material waveMaterial;
        [Tooltip("Seconds her waves take to follow a change in her speed (a gust or a sea's push shouldn't flicker them).")]
        public float waveFollow = 0.5f;
        [Tooltip("Her waves' height against a real hull of her size (1 = the real thing; a little more reads better in a chop).")]
        public float waveHeight = 1.4f;
        [Tooltip("The ladder of speeds (m/s) her waves are worked out for, lowest first.")]
        public float[] speeds = { 0.6f, 1f, 1.5f, 2.1f, 2.8f, 3.6f, 4.5f };
        [Tooltip("Texels across and along each patch of waves.")]
        public Vector2Int waveTexels = new Vector2Int(128, 256);

        [Header("Foam")]
        [Tooltip("HDRP's Water Decal Sample, Texture type, foam only (her trail).")]
        public Material foamMaterial;
        [Tooltip("Foam off her bow and sides and under her stern (0-1).")]
        [Range(0f, 1f)] public float foam = 1f;
        [Tooltip("Seconds her trail of foam lasts, and how many patches it's made of.")]
        public float trailSeconds = 20f;
        public int trailPatches = 32;
        [Tooltip("Metres a second the trail spreads out, each side.")]
        public float trailSpread = 0.12f;

        [Header("Where the water strikes her")]
        [Tooltip("Points along each side where the water is felt (and her stem besides).")]
        public int sideStations = 6;
        [Tooltip("Metres a second the water must surge up against her before it breaks white and flies up there.")]
        public float strikeSpeed = 0.45f;
        [Tooltip("Seconds a strike's heap of water and foam last (the water keeps the foam a little longer).")]
        public float strikeSeconds = 1.2f;
        [Tooltip("Metres the water heaps up against her planking in a hard strike.")]
        public float strikeHeap = 0.12f;
        [Tooltip("Patches for the strikes' heaps and foam (reused).")]
        public int strikePatches = 16;
        [Tooltip("The spray's material (transparent HDRP/Lit, HdrpWater makes it).")]
        public Material sprayMaterial;
        [Tooltip("How much spray the strikes throw up (droplets per strike, roughly: 90 at the stem, 35 on a side, times this).")]
        public float spray = 1f;

        [Header("The water")]
        [Tooltip("Metres round the ship where the water takes decals: her wake, the river's foam, the shore's foam.")]
        public float regionSize = 256f;
        [Tooltip("Texels across that region for foam and heights (1024 over 256 m = a quarter metre).")]
        public WaterSurface.WaterDecalRegionResolution decalResolution = WaterSurface.WaterDecalRegionResolution.Resolution1024;

        /// <summary>Metres a second the water under her bow is rising up it (+) - she's putting her bow into a sea.</summary>
        public float BowPlunge { get; private set; }
        /// <summary>Metres a second the water is surging against her hardest-struck side point right now.</summary>
        public float Slap { get; private set; }
        /// <summary>Strikes so far (for testing).</summary>
        public int Strikes { get; private set; }
        /// <summary>Her waves' textures are worked out and on the water.</summary>
        public bool WavesReady => fields != null;

        const float Ahead = 3f;                                              // metres of the waves' patch ahead of her bow
        const float Gravity = 9.81f;

        WaterSurface surface;

        // her waves: the ladder of fields and the two decals that show the nearest two
        sealed class Field
        {
            public float speed;
            public Vector2 area;
            public float[] heights;
            public Color32[] foam;
            public Texture2D heightTex, foamTex;
        }

        Field[] fields;
        Task<Field[]> fieldJob;
        readonly WaterDecal[] waveDecals = new WaterDecal[2];
        readonly MaterialPropertyBlock[] waveBlocks = new MaterialPropertyBlock[2];
        readonly int[] waveBucket = { -1, -1 };
        Material waveInstance;
        float fieldSpeed;
        bool hasField;

        Transform trailRoot;
        WaterDecal[] trail;
        float[] trailBorn, trailStrength, trailLength;
        int nextPatch;
        float lastPatch;
        Material trailMaterial;

        ParticleSystem sprayParticles;

        // the points where the water is felt: her waterline along each side (port, starboard in turn), her stem, her oar
        Vector2[] stations;                                                  // x (starboard +), z (forward +) in her own metres
        float[] lastImmersion, surge, nextStrike;
        bool hasImmersion;
        Transform strikeRoot;
        WaterDecal[] strikes;
        float[] strikeBorn, strikeStrength, strikeRoom;
        int nextStrikePatch;
        Material strikeMaterial;

        // ------------------------------------------------------------------------------------------ setting up

        void Start()
        {
            if (ship == null)
                ship = GetComponent<ShipMotion>();
            surface = WaterSampler.Surface;
            if (surface == null || ship == null)
            {
                enabled = false;                                             // no HDRP water here: nothing takes decals
                return;
            }
            surface.foam = true;
            surface.deformation = true;
            surface.foamPersistenceMultiplier = 1f;                         // foam fades over ~2 s (the most HDRP keeps it)
            surface.decalRegionSize = new Vector2(regionSize, regionSize);
            surface.decalRegionAnchor = transform;
            surface.foamResolution = decalResolution;
            surface.deformationRes = decalResolution;

            if (waveMaterial != null)
            {
                waveInstance = new Material(waveMaterial) { name = "Ship's waves" };
                for (int i = 0; i < 2; i++)
                {
                    waveDecals[i] = WaterDecalKit.MakeDecal("Her waves " + i + " (ShipWake)", transform, waveInstance, new Vector2(20f, 40f), waveTexels);
                    waveBlocks[i] = new MaterialPropertyBlock();
                }
                // the ladder of fields, on a worker: a few hundred milliseconds, while she's still moored
                var ladder = (float[])speeds.Clone();
                Array.Sort(ladder);
                var texels = waveTexels;
                float b = bowZ, s = sternZ, hb = halfBeam;
                fieldJob = Task.Run(() => ComputeFields(ladder, texels, b, s, hb));
            }
            if (foamMaterial != null)
                MakeTrail();
            MakeStations();
            if (waveMaterial != null)
                MakeStrikes();
            MakeSpray();
        }

        void OnDestroy()
        {
            if (trailRoot != null)
                Destroy(trailRoot.gameObject);
            if (strikeRoot != null)
                Destroy(strikeRoot.gameObject);
            if (fields != null)
                foreach (var f in fields)
                {
                    if (f.heightTex != null) Destroy(f.heightTex);
                    if (f.foamTex != null) Destroy(f.foamTex);
                }
            if (waveInstance != null) Destroy(waveInstance);
            if (trailMaterial != null) Destroy(trailMaterial);
            if (strikeMaterial != null) Destroy(strikeMaterial);
        }

        // ------------------------------------------------------------------------------------------ each frame

        Vector3 Forward => new Vector3(Mathf.Sin(ship.heading * Mathf.Deg2Rad), 0f, Mathf.Cos(ship.heading * Mathf.Deg2Rad));
        Vector3 Right => new Vector3(Mathf.Cos(ship.heading * Mathf.Deg2Rad), 0f, -Mathf.Sin(ship.heading * Mathf.Deg2Rad));

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;
            if (fieldJob != null && fieldJob.IsCompleted)
            {
                if (fieldJob.Status == TaskStatus.RanToCompletion)
                    TakeFields(fieldJob.Result);
                else if (fieldJob.Exception != null)
                    Debug.LogError("Kapman ship wake: " + fieldJob.Exception.GetBaseException());
                fieldJob = null;
            }
            float speed = ship.Speed;
            float running = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.8f, 2.4f, speed));     // 1.5 knots: nothing; 5: all

            UpdateWaves(speed, running, dt);
            FeelTheSea(dt);
            UpdateStrikes();
            // her trail lies on the water: on a river it drifts away downstream with the stream
            var river = RiverCurrent.Current;
            if (trailRoot != null && river != null)
                trailRoot.position += river.Drift(transform.position) * dt;
            UpdateTrail(speed, running);
        }

        // ------------------------------------------------------------------------------------------ her waves

        void TakeFields(Field[] made)
        {
            fields = made;
            for (int k = 0; k < fields.Length; k++)
            {
                var f = fields[k];
                f.heightTex = WaterDecalKit.HeightTexture(f.heights, waveTexels.x, waveTexels.y, "Ship's waves (heights) " + f.speed.ToString("0.0"));
                f.foamTex = WaterDecalKit.Upload(f.foam, waveTexels.x, waveTexels.y, "Ship's waves (foam) " + f.speed.ToString("0.0"));
                f.heights = null;
                f.foam = null;
            }
        }

        /// <summary>
        /// The two nearest fields of the ladder to her speed (followed over waveFollow seconds), blended by amplitude;
        /// a decal keeps its field as long as that field is still one of the two, so nothing is swapped under her.
        /// </summary>
        void UpdateWaves(float speed, float running, float dt)
        {
            if (fields == null || waveDecals[0] == null)
                return;
            fieldSpeed = hasField ? Mathf.Lerp(fieldSpeed, speed, 1f - Mathf.Exp(-dt / Mathf.Max(0.05f, waveFollow))) : speed;
            hasField = true;
            int last = fields.Length - 1;
            int k;
            float w;
            if (fieldSpeed <= fields[0].speed) { k = 0; w = 0f; }
            else if (fieldSpeed >= fields[last].speed) { k = last; w = 0f; }
            else
            {
                k = 0;
                while (k < last - 1 && fields[k + 1].speed <= fieldSpeed)
                    k++;
                w = Mathf.InverseLerp(fields[k].speed, fields[k + 1].speed, fieldSpeed);
            }
            int second = w > 0f && k < last ? k + 1 : -1;
            // hand the fields to the decals: whichever decal isn't showing a wanted field takes the missing one
            if (waveBucket[0] != k && waveBucket[1] != k)
                Assign(waveBucket[0] == second ? 1 : 0, k);
            if (second >= 0 && waveBucket[0] != second && waveBucket[1] != second)
                Assign(waveBucket[0] == k ? 1 : 0, second);
            float ramp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.6f, fieldSpeed));
            float plunge = Mathf.Clamp01(BowPlunge / 1.2f);
            for (int i = 0; i < 2; i++)
            {
                var d = waveDecals[i];
                float weight = waveBucket[i] == k ? 1f - w : waveBucket[i] == second ? w : 0f;
                weight *= ramp;
                d.amplitude = waveHeight * weight;
                d.surfaceFoamDimmer = Mathf.Clamp01(foam * running * weight * (0.7f + 0.6f * plunge));
                d.deepFoamDimmer = Mathf.Clamp01(foam * running * weight * 0.6f);
            }
        }

        void Assign(int decal, int bucket)
        {
            var f = fields[bucket];
            var d = waveDecals[decal];
            waveBucket[decal] = bucket;
            d.regionSize = f.area;
            d.transform.localPosition = new Vector3(0f, 0f, bowZ + Ahead - f.area.y * 0.5f);
            WaterDecalKit.SetTextures(d, waveBlocks[decal], f.heightTex, f.foamTex);
        }

        /// <summary>Her half-breadth at the waterline, metres, at a point along her (a pointed oval, fullest amidships).</summary>
        static float HalfBreadthOf(float z, float bowZ, float sternZ, float halfBeam)
        {
            float halfLength = 0.5f * (bowZ - sternZ), u = (z - 0.5f * (bowZ + sternZ)) / halfLength;
            return Mathf.Abs(u) < 1f ? halfBeam * Mathf.Pow(1f - u * u, 1.1f) : 0f;
        }

        float HalfBreadth(float z) => HalfBreadthOf(z, bowZ, sternZ, halfBeam);

        static Field[] ComputeFields(float[] ladder, Vector2Int texels, float bowZ, float sternZ, float halfBeam)
        {
            var result = new Field[ladder.Length];
            Parallel.For(0, ladder.Length, k => result[k] = ComputeField(ladder[k], texels, bowZ, sternZ, halfBeam));
            return result;
        }

        /// <summary>
        /// Her waves at one speed: the bow heap, the hollow along her sides and the stern hump by hand, and the Kelvin
        /// wake as the sum of plane waves over their angles from two sources (her bow raising the water, her stern
        /// drawing it down), each wave as long as that angle's speed makes it (k = g / (v cos a)^2). Heights in
        /// metres; the foam where the water is broken: the bow wave, along her sides, under her stern, the crests
        /// near her when she's going fast. Rows run from the back of the patch to its front.
        /// </summary>
        static Field ComputeField(float v, Vector2Int texels, float bowZ, float sternZ, float halfBeam)
        {
            int nx = texels.x, nz = texels.y;
            float W = Mathf.Clamp(8f + 10f * v, 14f, 48f), L = Mathf.Clamp(16f + 16f * v, 24f, 84f);
            float back = bowZ + Ahead - L;
            float texel = W / nx;
            float k0 = Gravity / (v * v);
            float lambda0 = 2f * Mathf.PI / k0;
            float hb = 0.35f * v * v / (2f * Gravity);                       // the bow heap: a share of the stagnation rise
            float rb = Mathf.Max(0.45f, halfBeam * 0.9f);
            bool kelvin = lambda0 >= 4f * texel;                             // too short for the texels: the ripples carry it
            // the angles whose waves the texels can still hold (cos^2 a >= 3 texels / lambda0), at most 78 degrees
            float cosMin = Mathf.Sqrt(Mathf.Clamp01(3f * texel / Mathf.Max(lambda0, 1e-3f)));
            float thetaMax = Mathf.Min(78f * Mathf.Deg2Rad, Mathf.Acos(Mathf.Clamp(cosMin, 0f, 1f)));
            const int N = 192;
            float dTheta = 2f * thetaMax / N;
            float rAlias = Mathf.PI / (k0 * Mathf.Max(dTheta, 1e-4f));      // beyond this the sum aliases: fade it out
            var cosT = new float[N];
            var sinT = new float[N];
            var wT = new float[N];
            var kT = new float[N];
            for (int a = 0; a < N; a++)
            {
                float th = -thetaMax + (a + 0.5f) * dTheta;
                cosT[a] = Mathf.Cos(th);
                sinT[a] = Mathf.Sin(th);
                wT[a] = cosT[a] * cosT[a] * dTheta;
                kT[a] = k0 / (cosT[a] * cosT[a]);
            }
            var h = new float[nx * nz];
            var kel = new float[nx * nz];
            var fx = new float[nx * nz];                                     // foam: surface
            var fz = new float[nx * nz];                                     // foam: deep
            float peak = 1e-6f;
            float crestFoam = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.8f, 3f, v));
            for (int j = 0; j < nz; j++)
            {
                float z = back + (j + 0.5f) / nz * L;
                float breadth = HalfBreadthOf(z, bowZ, sternZ, halfBeam);
                for (int i = 0; i < nx; i++)
                {
                    float x = (i + 0.5f) / nx * W - W * 0.5f;
                    int idx = j * nx + i;
                    // the near field
                    float heap = Mathf.Exp(-(x * x + (z - bowZ) * (z - bowZ)) / (rb * rb));
                    float sz = z - (sternZ - 0.9f);
                    float hump = Mathf.Exp(-(x * x + sz * sz) / (1.1f * rb * 1.1f * rb));
                    float height = hb * heap + 0.45f * hb * hump;
                    float along = 0f;
                    if (z > sternZ + 0.5f && z < bowZ - 0.8f)
                    {
                        float d = Mathf.Abs(x) - breadth;
                        if (d > 0f)
                        {
                            along = Mathf.Exp(-(d / 0.6f) * (d / 0.6f)) * Mathf.Sin(Mathf.PI * (z - sternZ) / (bowZ - sternZ));
                            height -= 0.3f * hb * along;
                        }
                    }
                    h[idx] = height;
                    // the Kelvin wake
                    float sum = 0f;
                    if (kelvin)
                    {
                        for (int src = 0; src < 2; src++)
                        {
                            float zs = src == 0 ? bowZ : sternZ, sign = src == 0 ? 1f : -1f;
                            float dz = z - zs;
                            float f = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-1f, 0.5f, dz));   // astern of the source only
                            if (f <= 0f)
                                continue;
                            float r = Mathf.Sqrt(dz * dz + x * x);
                            float taper = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.75f * rAlias, rAlias, r));
                            if (taper <= 0f)
                                continue;
                            float s = 0f;
                            for (int a = 0; a < N; a++)
                                s += wT[a] * Mathf.Cos(kT[a] * (dz * cosT[a] + x * sinT[a]) + 0.785398f);
                            sum += sign * f * taper * s;
                        }
                    }
                    kel[idx] = sum;
                    if (Mathf.Abs(sum) > peak)
                        peak = Mathf.Abs(sum);
                    // the foam
                    float stern = Mathf.Exp(-(x * x + (z - sternZ + 0.3f) * (z - sternZ + 0.3f)) / (1.2f * rb * 1.2f * rb));
                    float side = z > sternZ + 0.3f && z < bowZ - 0.3f ? Mathf.Clamp01(1f - Mathf.Abs(Mathf.Abs(x) - breadth - 0.15f) / 0.45f) : 0f;
                    fx[idx] = Mathf.Clamp01(0.9f * heap * (z > bowZ - 1f ? 1.3f : 0.6f)) + 0.5f * side + 0.9f * stern;
                    fz[idx] = 0.6f * heap + 0.8f * side + 1f * stern;
                }
            }
            float gain = 0.6f * hb / peak;
            var foam = new Color32[nx * nz];
            for (int idx = 0; idx < h.Length; idx++)
            {
                h[idx] += kel[idx] * gain;
                float crest = crestFoam * 0.5f * Mathf.Clamp01((kel[idx] / peak - 0.55f) / 0.3f);
                foam[idx] = new Color32(WaterDecalKit.B(Mathf.Clamp01(fx[idx] + crest)), WaterDecalKit.B(Mathf.Clamp01(fz[idx] + 0.5f * crest)), 0, 255);
            }
            return new Field { speed = v, area = new Vector2(W, L), heights = h, foam = foam };
        }

        // ------------------------------------------------------------------------------------------ where the water strikes her

        /// <summary>The points where the water is felt: her waterline along each side, from her quarters to her bow, her stem, her oar.</summary>
        void MakeStations()
        {
            int per = Mathf.Max(2, sideStations);
            stations = new Vector2[per * 2 + 2];
            float from = sternZ + 0.45f, to = bowZ - 0.55f;
            for (int k = 0; k < per; k++)
            {
                float z = Mathf.Lerp(from, to, k / (per - 1f)), b = HalfBreadth(z);
                stations[2 * k] = new Vector2(-b, z);
                stations[2 * k + 1] = new Vector2(b, z);
            }
            stations[per * 2] = new Vector2(0f, bowZ - 0.2f);                // the stem
            stations[per * 2 + 1] = Vector2.zero;                              // the oar's blade (placed from the oar itself)
            lastImmersion = new float[stations.Length];
            surge = new float[stations.Length];
            nextStrike = new float[stations.Length];
        }

        /// <summary>
        /// The water at each point of her: how fast it's surging up her there (rising against her, or she dropping into
        /// it). Where that's fast and the water's at her waterline, it strikes: a heap, foam and spray there.
        /// </summary>
        void FeelTheSea(float dt)
        {
            if (stations == null)
                return;
            var right = Right;
            var fwd = Forward;
            float wl = -ship.waterline;                                       // her waterline in her own space
            float t = Time.time, slapNow = 0f;
            int stem = stations.Length - 2, oar = stations.Length - 1;
            for (int i = 0; i < stations.Length; i++)
            {
                if (i == oar && steeringOar == null)
                    continue;
                var s = stations[i];
                var p = i == oar ? steeringOar.TransformPoint(oarBlade) : transform.TransformPoint(new Vector3(s.x, wl, s.y));
                WaterSampler.Sample(p, out float h, out _, false, 6);
                float immersion = h - p.y;
                float rate = hasImmersion ? (immersion - lastImmersion[i]) / dt : 0f;
                lastImmersion[i] = immersion;
                surge[i] = Mathf.Lerp(surge[i], rate, 1f - Mathf.Exp(-dt / 0.05f));
                if (i == stem)
                    BowPlunge = surge[i];
                else
                    slapNow = Mathf.Max(slapNow, surge[i]);
                // a strike: the water surging fast up her here, near her waterline (not a crest passing under her lifted bow)
                if (hasImmersion && surge[i] > strikeSpeed && immersion > -0.2f && t >= nextStrike[i])
                {
                    float strength = Mathf.Clamp01((surge[i] - strikeSpeed) / 0.8f);
                    var outward = i == stem ? fwd : i == oar ? right : (s.x < 0f ? -right : right);
                    // the heap never tops her gunwale: it only uses the room left above the water there
                    float room = GunwaleAbove(i == oar ? new Vector2(0f, sternZ + 0.45f) : s, h) - 0.08f;
                    Strike(new Vector3(p.x, h, p.z), outward, i == stem, i == oar ? strength * 0.6f : strength, Mathf.Max(0f, room));
                    nextStrike[i] = t + 0.3f;
                }
            }
            hasImmersion = true;
            Slap = slapNow;
        }

        void MakeStrikes()
        {
            strikeRoot = new GameObject("Where the water strikes her (ShipWake)").transform;
            strikeRoot.position = Vector3.zero;
            strikeMaterial = new Material(waveMaterial) { name = "A strike" };
            WaterDecalKit.StrikeTextures(out var heap, out var froth);
            strikeMaterial.SetTexture(WaterDecalKit.DeformationTexture, heap);
            strikeMaterial.SetTexture(WaterDecalKit.FoamTexture, froth);
            int n = Mathf.Max(2, strikePatches);
            strikes = new WaterDecal[n];
            strikeBorn = new float[n];
            strikeStrength = new float[n];
            strikeRoom = new float[n];
            for (int i = 0; i < n; i++)
            {
                strikes[i] = WaterDecalKit.MakeDecal("Strike " + i, strikeRoot, strikeMaterial, new Vector2(1f, 1f), new Vector2Int(32, 32));
                strikeBorn[i] = -1000f;
            }
        }

        /// <summary>
        /// Metres from the water at a point of her waterline up to her gunwale there: her freeboard amidships, rising to
        /// the ends as her sheer does, and less on the side she's heeled or pitched down to (her own tilt).
        /// </summary>
        float GunwaleAbove(Vector2 station, float water)
        {
            float halfLength = 0.5f * (bowZ - sternZ), u = (station.y - 0.5f * (bowZ + sternZ)) / halfLength;
            float up = freeboard * (1f + u * u);
            var top = transform.TransformPoint(new Vector3(station.x, -ship.waterline + up, station.y));
            return top.y - water;
        }

        /// <summary>The water strikes her at a point: it heaps up against her (never over her gunwale), breaks white, and flies up as spray.</summary>
        void Strike(Vector3 at, Vector3 outward, bool atStem, float strength, float room)
        {
            Strikes++;
            if (strikes != null)
            {
                int i = nextStrikePatch;
                nextStrikePatch = (nextStrikePatch + 1) % strikes.Length;
                var c = at + outward * (atStem ? 0.15f : 0.3f);
                strikes[i].transform.SetPositionAndRotation(new Vector3(c.x, at.y, c.z), Quaternion.Euler(0f, ship.heading + UnityEngine.Random.Range(-20f, 20f), 0f));
                strikeBorn[i] = Time.time;
                strikeStrength[i] = strength;
                strikeRoom[i] = room;
            }
            // droplets: off both sides of her stem when she puts her bow into a sea, else out from her side
            if (sprayParticles == null)
                return;
            int count = Mathf.RoundToInt((atStem ? 90f : 35f) * (0.3f + strength) * spray);
            var right = Right;
            var fwd = Forward;
            var carry = fwd * (ship.Speed * 0.9f);
            var ep = new ParticleSystem.EmitParams { applyShapeToPosition = false };
            float e = 0.6f + strength;
            for (int k = 0; k < count; k++)
            {
                var side = atStem ? (k % 2 == 0 ? right : -right) : outward;
                ep.position = at + fwd * UnityEngine.Random.Range(-0.3f, 0.3f) + side * 0.06f + Vector3.up * 0.03f;
                ep.velocity = carry + side * (UnityEngine.Random.Range(0.4f, 1.9f) * e) + Vector3.up * (UnityEngine.Random.Range(0.8f, 2.6f) * e) +
                              fwd * UnityEngine.Random.Range(atStem ? 0.2f : -0.4f, atStem ? 1.2f : 0.4f);
                ep.startSize = UnityEngine.Random.Range(0.012f, 0.035f);
                ep.startLifetime = UnityEngine.Random.Range(0.45f, 0.9f);
                sprayParticles.Emit(ep, 1);
            }
        }

        void UpdateStrikes()
        {
            if (strikes == null)
                return;
            float t = Time.time, life = Mathf.Max(0.2f, strikeSeconds);
            for (int i = 0; i < strikes.Length; i++)
            {
                var d = strikes[i];
                float f = (t - strikeBorn[i]) / life;
                if (f >= 1f || f < 0f)
                {
                    d.amplitude = 0f;
                    d.surfaceFoamDimmer = 0f;
                    d.deepFoamDimmer = 0f;
                    continue;
                }
                float s = strikeStrength[i];
                // the heap swells up quickly (but smoothly, over a fifth of a second) and falls away; the white water
                // spreads and fades (the water keeps it a while)
                float heap = Mathf.SmoothStep(0f, 1f, f / 0.16f) * (1f - f) * (1f - f);
                float white = Mathf.SmoothStep(0f, 1f, f / 0.1f) * (1f - f);
                d.amplitude = Mathf.Min(strikeHeap * (0.4f + s), strikeRoom[i]) * heap;
                d.surfaceFoamDimmer = Mathf.Clamp01(foam * (0.5f + 0.5f * s) * white);
                d.deepFoamDimmer = Mathf.Clamp01(foam * 0.5f * (0.5f + 0.5f * s) * white);
                float grow = 1f + 0.8f * f;
                d.regionSize = new Vector2(0.9f * grow, 1.4f * grow);
            }
        }

        // ------------------------------------------------------------------------------------------ the trail

        void MakeTrail()
        {
            trailRoot = new GameObject("Her trail of foam (ShipWake)").transform;
            trailRoot.position = Vector3.zero;
            trailMaterial = new Material(foamMaterial) { name = "Her trail of foam" };
            trailMaterial.SetTexture(WaterDecalKit.FoamTexture, WaterDecalKit.FoamPatch());
            trailMaterial.SetTexture(WaterDecalKit.DeformationTexture, Texture2D.blackTexture);
            int n = Mathf.Max(4, trailPatches);
            trail = new WaterDecal[n];
            trailBorn = new float[n];
            trailStrength = new float[n];
            trailLength = new float[n];
            for (int i = 0; i < n; i++)
            {
                trail[i] = WaterDecalKit.MakeDecal("Foam " + i, trailRoot, trailMaterial, new Vector2(1f, 1f), new Vector2Int(64, 64));
                trailBorn[i] = -1000f;
            }
        }

        void UpdateTrail(float speed, float running)
        {
            if (trail == null)
                return;
            float t = Time.time, life = Mathf.Max(1f, trailSeconds), every = life / trail.Length;
            // lay a patch under her stern every so often, while she's making way
            if (speed > 0.5f && t - lastPatch >= every)
            {
                lastPatch = t;
                int i = nextPatch;
                nextPatch = (nextPatch + 1) % trail.Length;
                var fwd = Forward;
                var at = transform.position + fwd * (sternZ - 0.6f) + Right * UnityEngine.Random.Range(-0.15f, 0.15f);
                // each patch turned and flipped a little differently, so the trail doesn't repeat itself
                trail[i].transform.SetPositionAndRotation(new Vector3(at.x, at.y, at.z), Quaternion.Euler(0f, ship.heading + UnityEngine.Random.Range(-8f, 8f), 0f));
                trail[i].transform.localScale = new Vector3(UnityEngine.Random.value < 0.5f ? -1f : 1f, 1f, UnityEngine.Random.value < 0.5f ? -1f : 1f);
                trailBorn[i] = t;
                trailStrength[i] = Mathf.Max(0.35f, running);
                trailLength[i] = Mathf.Max(1.6f, speed * every * 2.2f);     // each overlaps the next
            }
            for (int i = 0; i < trail.Length; i++)
            {
                var d = trail[i];
                float age = t - trailBorn[i], f = age / life;
                if (f >= 1f || f < 0f)
                {
                    d.surfaceFoamDimmer = 0f;
                    d.deepFoamDimmer = 0f;
                    continue;
                }
                // white, broken water just astern, soon only scattered patches and the paler slick of churned water
                // (each patch comes up over the first half second, so the trail grows rather than popping in)
                float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, f));
                float s = foam * trailStrength[i] * end * Mathf.SmoothStep(0f, 1f, age / 0.5f);
                d.surfaceFoamDimmer = Mathf.Clamp01(0.12f * s * Mathf.Exp(-age / 2f));
                d.deepFoamDimmer = Mathf.Clamp01(0.15f * s * Mathf.Exp(-age / 7f));
                d.regionSize = new Vector2(halfBeam * 1.6f + 2f * trailSpread * age, trailLength[i] * (1f + 0.02f * age));
            }
        }

        // ------------------------------------------------------------------------------------------ the spray

        /// <summary>
        /// The spray: droplets a centimetre or three across, drawn as short streaks along their flight, thrown only
        /// where the water strikes her (Strike emits them, each with its own place and speed).
        /// </summary>
        void MakeSpray()
        {
            if (sprayMaterial == null)
                return;
            var go = new GameObject("Spray where the water strikes her (ShipWake)");
            go.transform.SetParent(transform, false);
            sprayParticles = go.AddComponent<ParticleSystem>();
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 2500;
            main.startSpeed = 0f;
            main.gravityModifier = 1f;
            main.playOnAwake = false;
            var emission = sprayParticles.emission;
            emission.enabled = false;
            var shape = sprayParticles.shape;
            shape.enabled = false;
            var size = sprayParticles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.35f));
            var render = go.GetComponent<ParticleSystemRenderer>();
            render.material = sprayMaterial;                                 // (an instance, with its drop texture)
            render.material.SetTexture("_BaseColorMap", WaterDecalKit.SoftDot());
            render.renderMode = ParticleSystemRenderMode.Stretch;            // streaks along their flight, not balls
            render.velocityScale = 0.045f;
            render.lengthScale = 1.2f;
            render.shadowCastingMode = ShadowCastingMode.Off;
            render.receiveShadows = false;
            sprayParticles.Play();
        }
    }
}
