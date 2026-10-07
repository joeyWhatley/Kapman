// From the draft of 7 Oct 2026 (Design/ship_view/draft/PsxSeaGrid.cs), made over for the stage as it was built: its
// waves are the stage's own (StageSea), so it has none of its own (Design/ship_view/BUILD_LOG.md, part 2).
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE SEA AS A 1997 CONSOLE DREW IT: a coarse grid of triangles round the ship (49 x 49 vertices at 2.5 or 3 m:
    /// 120 to 144 m across, 4,608 triangles), its vertices lifted on the CPU by the stage's own waves (StageSea: the very
    /// waves she floats on, every wave too short for a cell left out) and coloured vertex by vertex: dark in the troughs,
    /// lighter on the crests, white where it breaks and in her wake. Beyond the grid one ring of long triangles runs
    /// flat to the horizon. Drawn by ShipView/Shaders/PsxSea.shader, which lays one small tile over it twice.
    /// She is held in place and the sea runs past: the grid's vertices are fixed to the water (they do not swim) and are
    /// re-centred a whole cell at a time; the tile slides with them. On a river, where the stage's waves are a hand
    /// high and a metre long, every wave is too short for a cell and this is a flat sea with its tiles sliding: the
    /// most 1996 sea of all.
    /// Under the hull the surface is pressed to her own waterline, so no wave comes up through her bottom boards.
    /// Its axes are the stage's: x east, z north, metres from the stage's middle.
    /// </summary>
    public sealed class PsxSeaGrid
    {
        /// <summary>Cells along a side, and metres to a cell.</summary>
        public int cells = 48;
        public float cell = 3f;
        /// <summary>Metres to the far edge of the flat ring beyond the grid.</summary>
        public float horizon = 3000f;
        /// <summary>Metres outside the hull over which the surface goes from her waterline back to the waves.</summary>
        public float hullBlend = 1.1f;
        /// <summary>The water's own colours (as paint, picked as on a screen: what it would show under white light), and its foam's.</summary>
        public Color deep = new Color(0.08f, 0.17f, 0.22f), crest = new Color(0.20f, 0.34f, 0.38f), foam = new Color(0.82f, 0.85f, 0.86f);
        /// <summary>The river's colours (brown-green, as the stage's own river).</summary>
        public Color riverDeep = new Color(0.15f, 0.18f, 0.13f), riverCrest = new Color(0.26f, 0.29f, 0.20f);
        /// <summary>A small tile the user draws (32 x 32 or 64 x 64, grey: the red channel is read). None: a plain stand-in is made.</summary>
        public Texture2D tile;
        /// <summary>The second layer of the tile slides down the wind at this share of the wind's speed.</summary>
        public float slideWithWind = 0.04f;

        public GameObject Object { get; private set; }
        public Material Material { get; private set; }

        Mesh mesh;
        Texture2D standIn;
        Vector3[] verts, normals;
        Color[] colours;
        Vector2[] foams;
        int built = -1;
        double slideEast, slideNorth;
        float river = -1f;
        bool hadNoShip;
        bool snap;

        /// <summary>No ship lies on this sea (the title): nothing is pressed to a hull, and no wake is laid.</summary>
        public bool noShip;
        /// <summary>How white her wake is just astern at her best speed (0..1).</summary>
        public float wakeFoam = 0.5f;
        /// <summary>How far each point of the grid bobs by itself in a light air (m; more with the wind, 0.2 at most).</summary>
        public float facetBob = 0.05f;
        float clock;
        /// <summary>How much of the water is a river's now, 0..1 (its colour follows over some seconds).</summary>
        public float River { get; private set; }
        /// <summary>The water's own colour now, between trough and crest (as paint): what the far water beyond the grid is given.</summary>
        public Color Mean => Color.Lerp(Color.Lerp(deep, riverDeep, Mathf.Clamp01(river)), Color.Lerp(crest, riverCrest, Mathf.Clamp01(river)), 0.5f);
        /// <summary>The next picture shows the water as if it had long been so (a river's colour at once).</summary>
        public void Settle() => snap = true;

        static readonly int FlowId = Shader.PropertyToID("_PsxSeaFlow"), TexId = Shader.PropertyToID("_SeaTex");

        public void Build(Transform parent, int layer, Shader shader)
        {
            Object = new GameObject("PSX sea grid") { layer = layer };
            Object.transform.SetParent(parent, false);
            Object.AddComponent<MeshFilter>();
            var r = Object.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            if (shader != null)
            {
                Material = new Material(shader) { name = "PSX sea", hideFlags = HideFlags.DontSave };
                r.sharedMaterial = Material;
            }
            Object.SetActive(false);
        }

        public void Destroy()
        {
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
            if (Material != null)
                UnityEngine.Object.Destroy(Material);
            if (standIn != null)
                UnityEngine.Object.Destroy(standIn);
            if (Object != null)
                UnityEngine.Object.Destroy(Object);
            mesh = null;
            Object = null;
        }

        /// <summary>The water's height over its level at a point east and north of her (m): the stage's own waves, less
        /// those this grid cannot carry. What she floats on while this sea is drawn.</summary>
        public float Height(StageSea sea, float east, float north) => sea.Height(east, north, cell, out _, out _);

        void Make()
        {
            cells = Mathf.Clamp(cells, 8, 120);
            int n = cells + 1;
            verts = new Vector3[n * n];
            normals = new Vector3[n * n];
            colours = new Color[n * n];
            foams = new Vector2[n * n];
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
            mesh.uv = foams;
            mesh.triangles = tris;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(2f * horizon, 60f, 2f * horizon));
            Object.GetComponent<MeshFilter>().sharedMesh = mesh;
            built = cells;
            if (Material != null)
            {
                if (tile == null)
                    tile = standIn = StandInTile();
                Material.SetTexture(TexId, tile);
                Material.SetColor("_SeaFoamColour", foam);
            }
        }

        /// <summary>Until the user draws one: a 32 x 32 grey tile of short strokes (it tiles; no filtering is ever used).</summary>
        static Texture2D StandInTile()
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true)
            {
                name = "PSX sea tile (stand-in)", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave,
            };
            // (middle grey is a half: the shader adds two layers of it, and a half and a half leave the water's colour as it is)
            var px = new Color32[n * n];
            for (int i = 0; i < px.Length; i++)
                px[i] = new Color32(128, 128, 128, 255);
            uint seed = 793u;
            for (int s = 0; s < 40; s++)
            {
                seed = seed * 1664525u + 1013904223u;
                int x = (int)((seed >> 8) % n), y = (int)((seed >> 16) % n), len = 3 + (int)((seed >> 24) % 7);
                byte v = (byte)(s % 5 < 2 ? 70 : s % 5 < 4 ? 196 : 232);
                for (int k = 0; k < len; k++)
                {
                    // a short stroke that sags a texel at its ends, like the light on a wavelet's back
                    int yy = (y + (k == 0 || k == len - 1 ? 1 : 0)) % n;
                    px[yy * n + (x + k) % n] = new Color32(v, v, v, 255);
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        static double Mod(double v, double m) => v - System.Math.Floor(v / m) * m;

        /// <summary>Moves the grid to the sea of this moment (called when a picture is about to be drawn).</summary>
        public void Rebuild(ShipStage stage, float dt)
        {
            if (Object == null)
                return;
            if (built != cells || mesh == null)
                Make();
            var sea = stage.Sea;
            var ship = stage.Ship;
            var s = stage.State;
            int n = cells + 1;

            // the grid is fixed to the water: its corner sits on a whole cell of (place + the water gone by)
            float x0 = -(float)Mod(sea.DriftEast, cell) - cells / 2 * cell;
            float z0 = -(float)Mod(sea.DriftNorth, cell) - cells / 2 * cell;
            // the tile slides with the water; the second layer also down the wind (both wrapped at 64 m: a whole number
            // of tiles of either layer, so nothing ever jumps and nothing loses precision)
            float to = (s.windFrom + 180f) * Mathf.Deg2Rad;
            slideEast += Mathf.Sin(to) * s.windSpeed * slideWithWind * dt;
            slideNorth += Mathf.Cos(to) * s.windSpeed * slideWithWind * dt;
            Shader.SetGlobalVector(FlowId, new Vector4((float)Mod(sea.DriftEast, 64.0), (float)Mod(sea.DriftNorth, 64.0),
                                                       (float)Mod(sea.DriftEast + slideEast, 64.0), (float)Mod(sea.DriftNorth + slideNorth, 64.0)));

            // her waterline as a plane, and her own axes, to press the surface to the hull and to lay the wake
            Transform root = ship.Root;
            Vector3 onPlane = root.localPosition;
            Vector3 up = root.localRotation * Vector3.up, ahead = root.localRotation * Vector3.forward, right = root.localRotation * Vector3.right;
            float halfLength = ship.Length * 0.46f, halfBeam = ship.Beam * 0.5f;
            float wake = s.moored || noShip ? 0f : Mathf.Clamp01(s.speed / 3f);
            float wakeLength = 6f + 20f * wake;
            float seaHs = Mathf.Max(sea.State.hs, 0.02f), whitecaps = Mathf.Clamp01(sea.Whitecaps);
            // (the highest crests of eight waves stand about six tenths of the significant height over the level)
            float range = 0.6f * seaHs + 0.05f;
            float breaksOver = Mathf.Max(0.62f, 1f - 2f * whitecaps);
            // each point of the grid also bobs a little by itself (fixed to the water: by the cell it stands on), so a calm
            // is never a dead plane and the facets catch the light one by one
            clock += dt;
            float bob = Mathf.Min(0.2f, facetBob * (0.5f + 0.1f * s.windSpeed));
            int cellEast = (int)(System.Math.Floor(sea.DriftEast / cell) % 4096.0), cellNorth = (int)(System.Math.Floor(sea.DriftNorth / cell) % 4096.0);
            // (the title has no ship and no place: its water is the open sea, whatever port the game lies at; and the
            // change to and from it is at once, not eased)
            bool inland = !noShip && (s.place == StagePlace.River || s.place == StagePlace.Port);
            if (noShip != hadNoShip)
            {
                hadNoShip = noShip;
                river = -1f;
            }
            river = river < 0f || snap ? (inland ? 1f : 0f) : Mathf.MoveTowards(river, inland ? 1f : 0f, dt / 6f);
            River = river;
            snap = false;
            Color low = Color.Lerp(deep, riverDeep, river).linear, high = Color.Lerp(crest, riverCrest, river).linear;

            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    int v = j * n + i;
                    float x = x0 + i * cell, z = z0 + j * cell;
                    int edge = Mathf.Min(Mathf.Min(i, cells - i), Mathf.Min(j, cells - j));
                    if (edge == 0)
                    {
                        // the outer ring: thrown out flat to the horizon
                        Vector2 d = new Vector2(x, z).normalized * horizon;
                        verts[v] = new Vector3(d.x, 0f, d.y);
                        normals[v] = Vector3.up;
                        Color far = Color.Lerp(low, high, 0.5f);
                        far.a = 0f;
                        colours[v] = far;
                        foams[v] = Vector2.zero;
                        continue;
                    }
                    float h = sea.Height(x, z, cell, out float dx, out float dz);
                    if (bob > 0f)
                    {
                        uint hash = (uint)((i + cellEast) * 73856093) ^ (uint)((j + cellNorth) * 19349663);
                        hash = hash * 1664525u + 1013904223u;
                        h += bob * Mathf.Sin(clock * (0.7f + ((hash >> 8) & 255u) / 255f * 0.9f) + ((hash >> 16) & 1023u) * 0.00614f);
                    }
                    float fade = Mathf.Clamp01((edge - 1) / 3f);              // the waves die away into the flat ring
                    h *= fade; dx *= fade; dz *= fade;
                    float y = h, foamHere = 0f;
                    Vector3 off = new Vector3(x - onPlane.x, 0f, z - onPlane.z);
                    float along = Vector3.Dot(off, ahead), across = Vector3.Dot(off, right);
                    float e = Mathf.Sqrt(along * along / (halfLength * halfLength) + across * across / (halfBeam * halfBeam));
                    float press = noShip ? 0f : 1f - Mathf.SmoothStep(1f, 1f + hullBlend / halfBeam, e);
                    if (press > 0f)
                    {
                        float plane = onPlane.y - (up.x * off.x + up.z * off.z) / Mathf.Max(0.2f, up.y);
                        y = Mathf.Lerp(y, plane, press);
                        dx *= 1f - press; dz *= 1f - press;
                    }
                    // the wake: a widening band of broken water astern
                    float astern = -along - halfLength * 0.7f;
                    if (wake > 0.01f && astern > 0f && astern < wakeLength && Mathf.Abs(across) < halfBeam * 0.6f + 0.1f * astern)
                    {
                        float left = 1f - astern / wakeLength;
                        foamHere = wakeFoam * wake * left * left;
                    }
                    verts[v] = new Vector3(x, y, z);
                    normals[v] = new Vector3(-dx, 1f, -dz).normalized;
                    float t = Mathf.Clamp01(0.5f + h / range * 0.5f);
                    // breaking crests: the top share of the sea goes white
                    if (whitecaps > 0.001f)
                        foamHere = Mathf.Max(foamHere, 0.85f * Mathf.Clamp01((t - breaksOver) * 4f) * (1f - river));
                    Color c = Color.Lerp(low, high, t);
                    foams[v] = new Vector2(foamHere, 0f);
                    c.a = 1f;
                    colours[v] = c;
                }
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.uv = foams;
        }
    }
}
