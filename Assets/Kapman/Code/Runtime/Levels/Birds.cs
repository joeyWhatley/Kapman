using System.Collections.Generic;
using Kapman.Nature;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// BIRDS - the only animals for now (the user's rule of 2 October): flocks that live on the level's clock. Geese fly
    /// in skeins over the basins at dawn and dusk and graze the wet meadows by day; gulls wheel over the river and the
    /// harbour and follow a moving boat; crows and rooks work the fields and roost in the woods at dusk; swallows skim
    /// the water on summer evenings; herons stand in the shallows on the mud and sand bars and lift off, slow and heavy,
    /// when the ship comes close; ducks sit on the slack water of the inner bends and paddle away from her. Each flying
    /// bird is a small flapping mesh (two wings), cheap enough for hundreds; a standing heron is a few cylinders and
    /// boxes. A flock can also be given perches (dam heads, posts, a stretch of mud): the birds sit on them, one each,
    /// and only fly when startled.
    /// The year's clock (wildlife slice 1, 4 October): the level's Wildlife sets each flock's presence from the species'
    /// calendar (no swallows in January, the geese in from October), and the geese and crows fly by the Sun's height,
    /// not the clock's hours, so their dawn flight comes at a December dawn too.
    /// </summary>
    public class Flock : MonoBehaviour
    {
        public enum Kind { Geese, Gulls, Crows, Swallows, Herons, Ducks }

        public Kind kind;
        [Tooltip("The species or species group in species.json (Assets/Kapman/Data/Wildlife) this flock shows: 'geese', " +
                 "'gulls', 'swallow'... Empty: the kind's default (DefaultSpecies).")]
        public string speciesId = "";
        [Tooltip("How much of the flock is here (0..1), set by the level's Wildlife from the calendar: the birds beyond " +
                 "count x presence are hidden, and under 0.05 the whole flock is away.")]
        [Range(0f, 1f)] public float presence = 1f;
        public int count = 12;
        public float wingspan = 1.5f;
        public float radius = 120f;
        public float height = 25f;
        public float speed = 9f;
        public Color colour = new Color(0.12f, 0.11f, 0.1f);
        public Vector3 groundCentre;
        [Tooltip("Gulls follow boats: the flock's ground moves after the player's ship while she moves.")]
        public bool follow;
        [Tooltip("Where the birds sit or stand (dam heads, posts, the mud), one bird each: they fly only when the ship comes within 'startle' metres.")]
        public Vector3[] perches;
        [Tooltip("How close the ship comes (m) before perched or standing birds lift off.")]
        public float startle = 45f;

        Transform[] birds;
        Vector3[] offsets;
        float[] phases;
        Vector3[] rests;
        float[] restYaw;
        GameObject[] standing, wings;
        float angle, lift, wander;
        bool flying = true;
        Material mat;
        int shown = -1;

        /// <summary>Below this presence the flock is away altogether (its GameObject off).</summary>
        public const float AwayBelow = 0.05f;

        bool Rests => perches != null && perches.Length > 0;

        /// <summary>The species group a flock of a kind stands for when its speciesId is empty (scenes built before the
        /// wildlife tables).</summary>
        public static string DefaultSpecies(Kind kind)
        {
            switch (kind)
            {
                case Kind.Geese: return "geese";
                case Kind.Gulls: return "gulls";
                case Kind.Crows: return "crows";
                case Kind.Swallows: return "swallow";
                case Kind.Herons: return "heron";
                default: return "mallard";
            }
        }

        /// <summary>The species id this flock answers to: its own, else its kind's default.</summary>
        public string Species => string.IsNullOrEmpty(speciesId) ? DefaultSpecies(kind) : speciesId;

        /// <summary>Sets how much of the flock is here (0..1): fewer birds shown, and the flock away (its GameObject off)
        /// under AwayBelow. Called by the level's Wildlife; the flock can be switched back on by it at any time.</summary>
        public void SetPresence(float p)
        {
            presence = Mathf.Clamp01(p);
            bool here = presence >= AwayBelow;
            if (gameObject.activeSelf != here)
                gameObject.SetActive(here);
            ShowBirds();
        }

        /// <summary>The birds beyond round(count x presence) are hidden (at least one while the flock is here).</summary>
        void ShowBirds()
        {
            if (birds == null)
                return;
            int n = presence < AwayBelow ? 0 : Mathf.Clamp(Mathf.RoundToInt(birds.Length * presence), 1, birds.Length);
            if (n == shown)
                return;
            shown = n;
            for (int i = 0; i < birds.Length; i++)
                if (birds[i] != null && birds[i].gameObject.activeSelf != i < n)
                    birds[i].gameObject.SetActive(i < n);
        }

        /// <summary>Takes the built birds away again (the shots build flocks in the editor for a frame and clear them after).</summary>
        public void Clear()
        {
            if (birds != null)
                foreach (var b in birds)
                    if (b != null)
                    {
                        if (Application.isPlaying)
                            Destroy(b.gameObject);
                        else
                            DestroyImmediate(b.gameObject);
                    }
            if (mat != null)
            {
                if (Application.isPlaying)
                    Destroy(mat);
                else
                    DestroyImmediate(mat);
            }
            birds = null;
            mat = null;
            shown = -1;
        }

        // ---- the Sun's band of the day, worked out once a frame for every flock (rising: the Sun higher than last frame)

        static int bandFrame = -1;
        static float lastAltitude = float.NaN;
        static bool sunRising = true;
        static HourBand band = HourBand.Day;

        /// <summary>The band of the day now, from LevelClock.SunAltitude: dawn -6 to +10 degrees and rising, dusk the same
        /// falling, night below -6, day the rest. After a jump (a step of more than two degrees) or on the first frame,
        /// "rising" is the clock's morning half; while paused it keeps what it was.</summary>
        public static HourBand Band
        {
            get
            {
                if (bandFrame == Time.frameCount)
                    return band;
                bandFrame = Time.frameCount;
                float alt = LevelClock.SunAltitude;
                float step = alt - lastAltitude;
                if (float.IsNaN(lastAltitude) || Mathf.Abs(step) > 2f)
                {
                    var clock = LevelClock.Current;
                    sunRising = clock == null || clock.HourOfDay < 12f;
                }
                else if (Mathf.Abs(step) > 1e-5f)
                    sunRising = step > 0f;
                lastAltitude = alt;
                band = Presence.Band(alt, sunRising);
                return band;
            }
        }

        /// <summary>Forgets the band worked out this frame (the shots move the clock without a new frame).</summary>
        public static void ForgetBand()
        {
            bandFrame = -1;
            lastAltitude = float.NaN;
        }

        public void Build()
        {
            var mesh = BirdMesh(wingspan);
            mat = new Material(Shader.Find("HDRP/Lit"));
            mat.SetColor("_BaseColor", colour);
            mat.SetFloat("_Smoothness", 0.05f);
            mat.SetFloat("_DoubleSidedEnable", 1f);
            mat.SetFloat("_CullMode", 0f);
            HDMaterial.ValidateMaterial(mat);
            mat.enableInstancing = true;
            Mesh stand = kind == Kind.Herons ? HeronMesh() : null;
            birds = new Transform[count];
            offsets = new Vector3[count];
            phases = new float[count];
            rests = new Vector3[count];
            restYaw = new float[count];
            standing = new GameObject[count];
            wings = new GameObject[count];
            var rnd = new System.Random(Mathf.RoundToInt(groundCentre.x * 13f + groundCentre.z * 7f) + count);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Bird");
                go.transform.SetParent(transform, false);
                var w = new GameObject("Wings", typeof(MeshFilter), typeof(MeshRenderer));
                w.transform.SetParent(go.transform, false);
                w.GetComponent<MeshFilter>().sharedMesh = mesh;
                var wr = w.GetComponent<MeshRenderer>();
                wr.sharedMaterial = mat;
                wr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                wings[i] = w;
                if (stand != null)
                {
                    var s = new GameObject("Standing", typeof(MeshFilter), typeof(MeshRenderer));
                    s.transform.SetParent(go.transform, false);
                    s.GetComponent<MeshFilter>().sharedMesh = stand;
                    var sr = s.GetComponent<MeshRenderer>();
                    sr.sharedMaterial = mat;
                    sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    standing[i] = s;
                    w.SetActive(false);
                }
                birds[i] = go.transform;
                phases[i] = (float)rnd.NextDouble() * 6.28f;
                if (kind == Kind.Ducks)
                {
                    // a dark lump on the water: the bird shape pressed flat and narrowed
                    w.transform.localScale = new Vector3(0.45f, 0.4f, 1f);
                    float a = (float)rnd.NextDouble() * 6.28f, r = Mathf.Sqrt((float)rnd.NextDouble()) * radius * 0.8f;
                    offsets[i] = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                }
                else if (kind == Kind.Geese)
                {
                    // a skein: a V behind the leader
                    int side = i % 2 == 0 ? 1 : -1, rank = (i + 1) / 2;
                    offsets[i] = new Vector3(side * rank * wingspan * 1.4f, rank * 0.2f, -rank * wingspan * 1.1f);
                }
                else
                    offsets[i] = new Vector3((float)(rnd.NextDouble() - 0.5) * radius * 0.5f, (float)(rnd.NextDouble() - 0.5) * height * 0.6f, (float)(rnd.NextDouble() - 0.5) * radius * 0.5f);
                if (Rests)
                {
                    rests[i] = perches[i % perches.Length];
                    if (i >= perches.Length)
                        rests[i] += new Vector3((float)(rnd.NextDouble() - 0.5) * 1.2f, 0f, (float)(rnd.NextDouble() - 0.5) * 1.2f);
                    restYaw[i] = (float)rnd.NextDouble() * 360f;
                }
            }
            angle = (float)rnd.NextDouble() * 6.28f;
            wander = (float)rnd.NextDouble() * 100f;
            lift = Rests ? 0f : 1f;
            shown = -1;
            ShowBirds();
        }

        void Start()
        {
            if (birds == null)
                Build();
        }

        void Update()
        {
            if (birds == null)
                return;
            float dt = Time.deltaTime * LevelClock.TimeFactor;
            float t = Time.time;
            float night = LevelClock.Night;
            var dayBand = Band;
            bool twilight = dayBand == HourBand.Dawn || dayBand == HourBand.Dusk;
            var info = LevelInfo.Current;
            float wl = info != null ? info.WaterLevel : 0f;
            var ship = ShipMotion.Player;
            Vector3 shipPos = ship != null ? ship.transform.position : groundCentre + Vector3.one * 1e5f;

            // gulls follow boats: the flock's ground drifts after the player's ship while she moves
            if (follow && ship != null && ship.Speed > 0.05f)
            {
                Vector3 to = new Vector3(shipPos.x, groundCentre.y, shipPos.z);
                groundCentre = Vector3.MoveTowards(groundCentre, to, Mathf.Max(ship.Speed * 1.5f, 2f) * dt);
            }
            if (kind == Kind.Ducks)
            {
                Drift(dt, t, wl, ship, shipPos);
                return;
            }
            bool near = ship != null && Vector3.Distance(shipPos, groundCentre) < (Rests ? startle : radius * 0.8f);

            // what the flock does now. The geese fly their lines between roost and meadow at dawn and dusk and the crows
            // go out from the roost and back in, by the Sun's height (a December dawn is dark at the clock's 7); the crows
            // also wander up by day; the swallows hawk by daylight, dawn to dusk (the design's 2.4), and sit at night
            bool wantFly;
            if (Rests)
                wantFly = near;
            else
                switch (kind)
                {
                    case Kind.Geese: wantFly = near || twilight; break;
                    case Kind.Gulls: wantFly = night < 0.5f; break;
                    case Kind.Crows: wantFly = near || twilight || (dayBand == HourBand.Day && Mathf.PerlinNoise(t * 0.02f, wander) > 0.6f); break;
                    case Kind.Herons: wantFly = near; break;
                    default: wantFly = dayBand != HourBand.Night; break;
                }
            lift = Mathf.MoveTowards(lift, wantFly ? 1f : 0f, dt * (kind == Kind.Herons ? 0.18f : 0.25f));
            flying = lift > 0.01f;
            float sp = speed * Mathf.Lerp(0.15f, 1f, lift);
            angle += sp / Mathf.Max(radius, 10f) * dt;
            wander += dt * 0.1f;
            float r = radius * (0.7f + 0.3f * Mathf.PerlinNoise(wander, 3f));
            Vector3 centre = groundCentre + new Vector3(Mathf.Cos(angle) * r, Mathf.Lerp(0.3f, height, lift) + 6f * Mathf.PerlinNoise(wander, 7f), Mathf.Sin(angle) * r);
            Vector3 heading = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            Quaternion rot = Quaternion.LookRotation(heading, Vector3.up);
            float ease = Mathf.SmoothStep(0f, 1f, lift);
            float flapHz = kind == Kind.Swallows ? 14f : kind == Kind.Geese ? 4f : kind == Kind.Herons ? 2.2f : 6f;
            int here = shown >= 0 ? shown : birds.Length;      // (the hidden ones are not moved)
            for (int i = 0; i < here; i++)
            {
                float ph = phases[i];
                Vector3 off = rot * offsets[i] * Mathf.Lerp(0.35f, 1f, lift);
                float bob = Mathf.Sin(t * (kind == Kind.Swallows ? 6f : 2f) + ph) * 0.4f * lift;
                Vector3 p = centre + off + Vector3.up * bob;
                if (Rests)
                    p = Vector3.Lerp(rests[i], p, ease);
                else if (lift < 0.5f)
                    p.y = Mathf.Max(p.y, wl + 0.2f);
                // a standing heron shows its standing shape; as soon as it lifts, its wings
                if (standing[i] != null)
                {
                    bool stands = lift < 0.05f;
                    if (standing[i].activeSelf != stands)
                        standing[i].SetActive(stands);
                    if (wings[i].activeSelf == stands)
                        wings[i].SetActive(!stands);
                }
                if (Rests && lift < 0.02f)
                {
                    birds[i].SetPositionAndRotation(rests[i], Quaternion.Euler(0f, restYaw[i], 0f));
                    birds[i].localScale = Vector3.one;
                    continue;
                }
                float flap = Mathf.Sin(t * flapHz + ph) * 35f * lift;
                // the wings flap by rotating the two halves: done with the mesh's y-scale as a cheap stand-in
                birds[i].SetPositionAndRotation(p, rot * Quaternion.Euler(0f, 0f, flap * 0.3f));
                birds[i].localScale = new Vector3(1f, 1f + 0.3f * Mathf.Abs(Mathf.Sin(flap * Mathf.Deg2Rad)), 1f);
            }
        }

        /// <summary>Ducks: they never fly here. They drift about on the water and paddle away from the ship.</summary>
        void Drift(float dt, float t, float wl, ShipMotion ship, Vector3 shipPos)
        {
            int here = shown >= 0 ? shown : birds.Length;      // (the hidden ones are not moved)
            for (int i = 0; i < here; i++)
            {
                float ph = phases[i];
                Vector2 drift = new Vector2(Mathf.PerlinNoise(t * 0.04f + ph, wander + i) - 0.5f, Mathf.PerlinNoise(wander + i, t * 0.04f + ph) - 0.5f) * 0.5f;
                Vector3 move = new Vector3(drift.x, 0f, drift.y);
                Vector3 o = offsets[i];
                Vector3 pos = groundCentre + o;
                if (ship != null)
                {
                    Vector3 away = pos - shipPos;
                    away.y = 0f;
                    float d = away.magnitude;
                    if (d < 30f && d > 0.01f)
                        move += away / d * 0.9f;
                }
                o += move * dt;
                Vector2 flat = new Vector2(o.x, o.z);
                if (flat.magnitude > radius)
                {
                    flat = flat.normalized * radius;
                    o.x = flat.x;
                    o.z = flat.y;
                }
                o.y = 0f;
                offsets[i] = o;
                Vector3 p = groundCentre + o;
                p.y = wl + 0.05f + 0.03f * Mathf.Sin(t * 1.3f + ph);
                Quaternion rot = move.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(move.normalized, Vector3.up) : birds[i].rotation;
                birds[i].SetPositionAndRotation(p, rot);
                birds[i].localScale = Vector3.one;
            }
        }

        static Mesh BirdMesh(float span)
        {
            // two wings swept back, a stub of a body: a dark shape that reads as a bird at any distance
            var m = new Mesh { name = "Bird" };
            float s = span / 2f, l = span * 0.28f;
            m.vertices = new[]
            {
                new Vector3(0f, 0f, l * 0.6f), new Vector3(-s, 0.08f * s, -l * 0.4f), new Vector3(0f, 0f, -l),
                new Vector3(s, 0.08f * s, -l * 0.4f),
            };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>A grey heron standing, about a metre tall, facing +z: legs, a body tilted up to the front, neck, head and bill.</summary>
        static Mesh HeronMesh()
        {
            var v = new List<Vector3>();
            var tr = new List<int>();
            AddBox(v, tr, new Vector3(0.06f, 0.21f, -0.02f), new Vector3(0.02f, 0.42f, 0.02f));
            AddBox(v, tr, new Vector3(-0.06f, 0.21f, -0.02f), new Vector3(0.02f, 0.42f, 0.02f));
            AddCylinder(v, tr, new Vector3(0f, 0.40f, -0.22f), new Vector3(0f, 0.55f, 0.20f), 0.11f, 8);
            AddCylinder(v, tr, new Vector3(0f, 0.54f, 0.17f), new Vector3(0f, 0.95f, 0.26f), 0.035f, 6);
            AddCylinder(v, tr, new Vector3(0f, 0.95f, 0.20f), new Vector3(0f, 0.95f, 0.34f), 0.045f, 6);
            AddCylinder(v, tr, new Vector3(0f, 0.95f, 0.34f), new Vector3(0f, 0.93f, 0.50f), 0.012f, 4);
            var m = new Mesh { name = "Heron standing" };
            m.SetVertices(v);
            m.SetTriangles(tr, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static void AddBox(List<Vector3> v, List<int> tr, Vector3 c, Vector3 s)
        {
            Vector3 h = s / 2f;
            int i0 = v.Count;
            for (int k = 0; k < 8; k++)
                v.Add(c + new Vector3((k & 1) == 0 ? -h.x : h.x, (k & 2) == 0 ? -h.y : h.y, (k & 4) == 0 ? -h.z : h.z));
            int[] faces = { 0, 1, 3, 2, 4, 6, 7, 5, 0, 4, 5, 1, 2, 3, 7, 6, 0, 2, 6, 4, 1, 5, 7, 3 };
            for (int f = 0; f < 6; f++)
            {
                int a = i0 + faces[f * 4], b = i0 + faces[f * 4 + 1], d = i0 + faces[f * 4 + 2], e = i0 + faces[f * 4 + 3];
                tr.AddRange(new[] { a, b, d, a, d, e });
            }
        }

        static void AddCylinder(List<Vector3> v, List<int> tr, Vector3 a, Vector3 b, float r, int sides)
        {
            Vector3 axis = (b - a).normalized;
            Vector3 u = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            Vector3 w = Vector3.Cross(axis, u);
            int i0 = v.Count;
            for (int s = 0; s < sides; s++)
            {
                float ang = s * Mathf.PI * 2f / sides;
                Vector3 o = (u * Mathf.Cos(ang) + w * Mathf.Sin(ang)) * r;
                v.Add(a + o);
                v.Add(b + o);
            }
            int ca = v.Count;
            v.Add(a);
            int cb = v.Count;
            v.Add(b);
            for (int s = 0; s < sides; s++)
            {
                int n = (s + 1) % sides;
                int a0 = i0 + s * 2, a1 = a0 + 1, b0 = i0 + n * 2, b1 = b0 + 1;
                tr.AddRange(new[] { a0, a1, b1, a0, b1, b0, ca, b0, a0, cb, a1, b1 });
            }
        }
    }
}
