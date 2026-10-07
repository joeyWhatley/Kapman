// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/PsxSeaGrid.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity (not run: no frame of it has been seen).
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE SEA AS A 1997 CONSOLE DREW IT - a coarse grid of triangles round the ship (49 x 49 vertices at 2.5 m by
    /// default: 120 m across, 4,608 triangles), its vertices lifted on the CPU each frame by a handful of sine waves made
    /// from the game's own sea state (wind sea and swell: heights, periods, bearings), and coloured vertex by vertex:
    /// dark in the troughs, lighter on the crests, white where it breaks and in her wake. Beyond the grid one ring of
    /// long triangles runs flat to the horizon, where the fog takes it. Drawn by Shaders/ShipView/PsxSea.shader.
    ///
    /// THE SHIP MUST RIDE THE SAME WAVES: in the styles that draw this sea, float her on <see cref="Height"/> (not on
    /// HDRP's water: it is switched off then). The sea "flows past" a ship held in place: add her way made good to
    /// <see cref="flow"/> every frame and the waves, the tile and the grid itself slide by; the grid's vertices stay
    /// fixed to the water (they do not swim), re-centred a whole cell at a time.
    /// Under the hull the surface is pressed to the ship's own waterline, so no wave comes up through her bottom boards.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [DefaultExecutionOrder(150)]
    public sealed class PsxSeaGrid : MonoBehaviour
    {
        [Tooltip("Cells along a side, and metres to a cell. Waves shorter than three cells are left out.")]
        public int cells = 48;
        public float cell = 2.5f;
        [Tooltip("Metres to the far edge of the flat ring beyond the grid.")]
        public float horizon = 3000f;
        [Tooltip("Still water's height in the world (m).")]
        public float level;

        [Header("The ship in it")]
        public Transform ship;
        [Tooltip("Still water's height in the ship's own space, and the hull's half length and half beam at the water (the pack's ship: 1.2, 8.3, 2.5).")]
        public float waterline = 1.2f, hullHalfLength = 8.3f, hullHalfBeam = 2.5f;
        [Tooltip("Metres outside the hull over which the surface goes from her waterline back to the waves.")]
        public float hullBlend = 2.5f;
        [Range(0f, 1f)] [Tooltip("Her wake: 0 lying still, 1 at her best speed.")]
        public float wake;

        [Header("How it looks")]
        public Color deep = new Color(0.05f, 0.15f, 0.20f), crest = new Color(0.14f, 0.33f, 0.38f), foam = new Color(0.86f, 0.90f, 0.92f);
        [Tooltip("A small tile you draw (32 x 32 or 64 x 64, grey: the red channel is read). None: a plain stand-in is made.")]
        public Texture2D tile;
        public Material material;

        /// <summary>How far the sea has run past the ship (m, world x and z). The stage adds her way made good to it.</summary>
        [System.NonSerialized] public Vector2 flow;
        /// <summary>The waves' clock (s). Runs by itself at `timeScale`.</summary>
        [System.NonSerialized] public float clock;
        public float timeScale = 1f;

        struct Wave { public float kx, kz, amp, omega; }
        readonly Wave[] waves = new Wave[5];
        int waveCount;
        float seaHs = 0.3f, whitecaps;

        Mesh mesh;
        Vector3[] verts, normals;
        Color[] colours;
        int built = -1;
        float builtCell;

        // each train as a few waves: their periods as shares of the peak period, their bearings off the train's, their shares of its height
        static readonly float[] WindPeriods = { 0.75f, 1f, 1.3f }, WindTurns = { -25f, 0f, 20f }, WindShares = { 0.5f, 0.7f, 0.5f };
        static readonly float[] SwellPeriods = { 1f, 1.15f }, SwellTurns = { -6f, 7f }, SwellShares = { 0.75f, 0.6f };

        static readonly int FlowId = Shader.PropertyToID("_PsxSeaFlow"), TexId = Shader.PropertyToID("_SeaTex");

        void OnEnable()
        {
            if (waveCount == 0)
                SetSea(0.3f, 3.5f, 250f, 0f, 8f, 300f, 0f);
        }

        // ------------------------------------------------------------------ the waves

        /// <summary>The sea from the game's model (Kapman.Weather.SeaState: HdrpSea's, or the chart's own).</summary>
        public void SetSea(in Kapman.Weather.SeaState s)
        {
            SetSea(s.windSeaHs, s.windSeaTp, s.windSeaFrom, s.swellHs, s.swellTp, s.swellFrom, s.whitecaps);
        }

        /// <summary>
        /// The sea as two trains: the wind sea (three waves spread about its bearing) and the swell (two). Heights are
        /// significant heights (m), periods in seconds, bearings the compass point the waves come FROM (degrees, x east,
        /// z north). The amplitudes are shared so each train's significant height comes out as given.
        /// </summary>
        public void SetSea(float windSeaHs, float windSeaTp, float windSeaFrom, float swellHs, float swellTp, float swellFrom, float whitecapShare)
        {
            waveCount = 0;
            Train(windSeaHs, windSeaTp, windSeaFrom, WindPeriods, WindTurns, WindShares);
            Train(swellHs, swellTp, swellFrom, SwellPeriods, SwellTurns, SwellShares);
            seaHs = Mathf.Sqrt(windSeaHs * windSeaHs + swellHs * swellHs);
            whitecaps = Mathf.Clamp01(whitecapShare);
        }

        void Train(float hs, float tp, float from, float[] periods, float[] turns, float[] shares)
        {
            if (hs <= 0.005f || tp <= 0.1f)
                return;
            float sum = 0f;
            foreach (float sh in shares)
                sum += sh * sh;
            // the shortest wave the grid can carry: three cells long
            float shortest = Mathf.Sqrt(2f * Mathf.PI * 3f * Mathf.Max(0.5f, cell) / 9.81f);
            for (int i = 0; i < periods.Length && waveCount < waves.Length; i++)
            {
                float t = Mathf.Max(shortest, tp * periods[i]);
                float k = 4f * Mathf.PI * Mathf.PI / (9.81f * t * t);          // deep water: k = (2 pi / T)^2 / g
                float going = (from + 180f + turns[i]) * Mathf.Deg2Rad;         // where it goes
                waves[waveCount++] = new Wave
                {
                    kx = k * Mathf.Sin(going), kz = k * Mathf.Cos(going),
                    amp = shares[i] * hs / Mathf.Sqrt(8f * sum),               // sum of amp^2 = Hs^2 / 8
                    omega = 2f * Mathf.PI / t,
                };
            }
        }

        /// <summary>The water's height at a world point now (m): what the ship floats on in the styles that draw this sea.</summary>
        public float Height(float x, float z)
        {
            Sample(x, z, out float h, out _, out _);
            return level + h;
        }

        public float Height(Vector3 world) => Height(world.x, world.z);

        void Sample(float x, float z, out float h, out float dx, out float dz)
        {
            h = dx = dz = 0f;
            float wx = x + flow.x, wz = z + flow.y;
            for (int i = 0; i < waveCount; i++)
            {
                var w = waves[i];
                float a = w.kx * wx + w.kz * wz - w.omega * clock;
                float c = Mathf.Cos(a);
                h += w.amp * Mathf.Sin(a);
                dx += w.amp * w.kx * c;
                dz += w.amp * w.kz * c;
            }
        }

        // ------------------------------------------------------------------ the mesh

        void Build()
        {
            cells = Mathf.Clamp(cells, 8, 120);
            int n = cells + 1;
            verts = new Vector3[n * n];
            normals = new Vector3[n * n];
            colours = new Color[n * n];
            var tris = new int[cells * cells * 6];
            int t = 0;
            for (int j = 0; j < cells; j++)
                for (int i = 0; i < cells; i++)
                {
                    int a = j * n + i, b = a + 1, c = a + n, d = c + 1;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }
            if (mesh == null)
            {
                mesh = new Mesh { name = "PSX sea grid", hideFlags = HideFlags.DontSave };
                mesh.MarkDynamic();
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.triangles = tris;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            built = cells;
            builtCell = cell;

            var r = GetComponent<MeshRenderer>();
            if (material == null)
            {
                var sh = Shader.Find("Kapman/ShipView/PSX Sea");
                if (sh != null)
                    material = new Material(sh) { name = "PSX sea", hideFlags = HideFlags.DontSave };
            }
            if (material != null)
            {
                if (tile == null)
                    tile = StandInTile();
                material.SetTexture(TexId, tile);
                r.sharedMaterial = material;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        /// <summary>Until the user draws one: a 32 x 32 grey tile of short strokes (it tiles; no filtering is ever used).</summary>
        static Texture2D StandInTile()
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true)
            {
                name = "PSX sea tile (stand-in)", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave,
            };
            var px = new Color32[n * n];
            for (int i = 0; i < px.Length; i++)
                px[i] = new Color32(118, 118, 118, 255);
            uint seed = 793u;
            for (int s = 0; s < 22; s++)
            {
                seed = seed * 1664525u + 1013904223u;
                int x = (int)(seed >> 8) % n, y = (int)(seed >> 16) % n, len = 3 + (int)(seed >> 24) % 5;
                byte v = (byte)(s % 3 == 0 ? 84 : 168);
                for (int k = 0; k < len; k++)
                    px[y * n + (x + k) % n] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        void LateUpdate()
        {
            clock += Time.deltaTime * timeScale;
            if (built != cells || builtCell != cell || mesh == null)
                Build();
            Shader.SetGlobalVector(FlowId, new Vector4(flow.x, flow.y, clock, 0f));

            int n = cells + 1;
            Vector3 mid = ship != null ? ship.position : transform.position;
            // the grid is fixed to the water: its corner sits on a whole cell of (world + flow)
            float x0 = (Mathf.Floor((mid.x + flow.x) / cell) - cells / 2) * cell - flow.x;
            float z0 = (Mathf.Floor((mid.z + flow.y) / cell) - cells / 2) * cell - flow.y;

            // her waterline as a plane, and her own axes, to press the surface to the hull and to lay the wake
            bool hull = ship != null;
            Vector3 onPlane = hull ? ship.TransformPoint(new Vector3(0f, waterline, 0f)) : Vector3.zero;
            Vector3 up = hull ? ship.up : Vector3.up, ahead = hull ? ship.forward : Vector3.forward, right = hull ? ship.right : Vector3.right;
            float wakeLength = 12f + 50f * wake;
            float range = 1.5f * seaHs + 0.05f;

            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    int v = j * n + i;
                    float x = x0 + i * cell, z = z0 + j * cell;
                    int edge = Mathf.Min(Mathf.Min(i, cells - i), Mathf.Min(j, cells - j));
                    if (edge == 0)
                    {
                        // the outer ring: thrown out flat to the horizon
                        Vector2 d = new Vector2(x - mid.x, z - mid.z).normalized * horizon;
                        verts[v] = new Vector3(mid.x + d.x, level, mid.z + d.y);
                        normals[v] = Vector3.up;
                        colours[v] = Shade(Color.Lerp(deep, crest, 0.5f), 0f);
                        continue;
                    }
                    Sample(x, z, out float h, out float dx, out float dz);
                    float fade = Mathf.Clamp01((edge - 1) / 3f);              // the waves die away into the flat ring
                    h *= fade; dx *= fade; dz *= fade;
                    float y = level + h;
                    float white = 0f;
                    if (hull)
                    {
                        Vector3 off = new Vector3(x - onPlane.x, 0f, z - onPlane.z);
                        float along = Vector3.Dot(off, ahead), across = Vector3.Dot(off, right);
                        float e = Mathf.Sqrt(along * along / (hullHalfLength * hullHalfLength) + across * across / (hullHalfBeam * hullHalfBeam));
                        float press = 1f - Mathf.SmoothStep(1f, 1f + hullBlend / hullHalfBeam, e);
                        if (press > 0f)
                        {
                            float plane = onPlane.y - (up.x * off.x + up.z * off.z) / Mathf.Max(0.2f, up.y);
                            y = Mathf.Lerp(y, plane, press);
                            dx *= 1f - press; dz *= 1f - press;
                        }
                        // the wake: a widening band of broken water astern
                        float astern = -along - hullHalfLength * 0.7f;
                        if (wake > 0.01f && astern > 0f && astern < wakeLength && Mathf.Abs(across) < hullHalfBeam * 0.7f + 0.2f * astern)
                            white = wake * (1f - astern / wakeLength);
                    }
                    verts[v] = new Vector3(x, y, z);
                    normals[v] = new Vector3(-dx, 1f, -dz).normalized;
                    float t = Mathf.Clamp01(0.5f + h / range * 0.5f);
                    // breaking crests: the top share of the sea goes white
                    if (whitecaps > 0.001f)
                        white = Mathf.Max(white, Mathf.Clamp01((t - (1f - 2.5f * whitecaps)) * 6f));
                    colours[v] = Shade(Color.Lerp(Color.Lerp(deep, crest, t), foam, white), 1f);
                }
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.bounds = new Bounds(new Vector3(mid.x, level, mid.z), new Vector3(2f * horizon, 30f, 2f * horizon));
            // the mesh's vertices are world places: keep its object at the origin, unturned
            if (transform.position != Vector3.zero || transform.rotation != Quaternion.identity)
                transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        // (vertex colours reach the shader as they are written: in a linear project they must be written linear)
        static Color Shade(Color c, float tileShows)
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                c = c.linear;
            return new Color(c.r, c.g, c.b, tileShows);
        }

        void OnDestroy()
        {
            if (mesh != null)
                Destroy(mesh);
        }
    }
}
