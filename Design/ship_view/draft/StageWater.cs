// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/StageWater.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run.
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE WATER'S MESH: a square of sea round her, fine under the boat (half-metre cells out to <see cref="fine"/>
    /// metres) and coarser with distance out to <see cref="reach"/>, with four flat skirts beyond it to the horizon.
    /// Its points are moved by StageSea on the CPU (height and true normals; no shader of our own is needed, so the
    /// first picture works with HDRP/Lit), every wave too short for a cell left out of it. Vertex colour: red is foam
    /// (the crests, by the sea's share of whitecaps), for a look's shader to use; HDRP/Lit ignores it.
    /// A look that moves the water in its own vertex shader sets <see cref="moveOnCpu"/> off and reads StageSea's globals.
    /// Its axes are the stage's: x east, z north, she at the middle.
    /// </summary>
    public sealed class StageWater
    {
        public float fine = 10f, cell = 0.5f, reach = 420f, horizon = 14000f;
        public int outer = 20;
        public bool moveOnCpu = true;

        public GameObject Object { get; private set; }
        public MeshRenderer Renderer { get; private set; }

        Mesh mesh;
        Vector3[] rest, moved, normals;
        Color32[] colours;
        float[] cellAt;
        int grid;

        /// <summary>The line of points along one axis: even cells in the middle, growing by a fixed ratio outside.</summary>
        float[] Line()
        {
            int inner = Mathf.Max(2, Mathf.RoundToInt(fine / cell)) * 2;
            // the ratio that takes `outer` cells from the fine edge to the reach
            float lo = 1.01f, hi = 3f;
            for (int it = 0; it < 40; it++)
            {
                float r = 0.5f * (lo + hi), sum = 0f, c = cell;
                for (int i = 0; i < outer; i++)
                {
                    c *= r;
                    sum += c;
                }
                if (fine + sum > reach) hi = r; else lo = r;
            }
            float ratio = 0.5f * (lo + hi);
            var line = new float[inner + 1 + 2 * outer];
            int mid = outer + inner / 2;
            for (int i = 0; i <= inner / 2; i++)
            {
                line[mid + i] = i * cell;
                line[mid - i] = -i * cell;
            }
            float at = fine, step = cell;
            for (int i = 1; i <= outer; i++)
            {
                step *= ratio;
                at += step;
                line[mid + inner / 2 + i] = at;
                line[mid - inner / 2 - i] = -at;
            }
            return line;
        }

        public GameObject Build(Transform parent, Material material, int layer)
        {
            float[] line = Line();
            grid = line.Length;
            int n = grid * grid;
            rest = new Vector3[n + 8];
            moved = new Vector3[n + 8];
            normals = new Vector3[n + 8];
            colours = new Color32[n + 8];
            cellAt = new float[n];
            for (int z = 0; z < grid; z++)
                for (int x = 0; x < grid; x++)
                {
                    int i = z * grid + x;
                    rest[i] = new Vector3(line[x], 0f, line[z]);
                    float cx = x + 1 < grid ? line[x + 1] - line[x] : line[x] - line[x - 1];
                    float cz = z + 1 < grid ? line[z + 1] - line[z] : line[z] - line[z - 1];
                    cellAt[i] = Mathf.Max(cx, cz);
                    normals[i] = Vector3.up;
                }
            var tris = new int[(grid - 1) * (grid - 1) * 6 + 24];
            int t = 0;
            for (int z = 0; z < grid - 1; z++)
                for (int x = 0; x < grid - 1; x++)
                {
                    int i = z * grid + x;
                    tris[t++] = i; tris[t++] = i + grid; tris[t++] = i + 1;
                    tris[t++] = i + 1; tris[t++] = i + grid; tris[t++] = i + grid + 1;
                }
            // the skirts: from the square's edge (left flat: the outermost ring is not moved) out to the horizon
            float e = line[grid - 1], h = horizon;
            Vector3[] ring = { new Vector3(-e, 0, -e), new Vector3(e, 0, -e), new Vector3(e, 0, e), new Vector3(-e, 0, e),
                               new Vector3(-h, 0, -h), new Vector3(h, 0, -h), new Vector3(h, 0, h), new Vector3(-h, 0, h) };
            for (int i = 0; i < 8; i++)
            {
                rest[n + i] = ring[i];
                normals[n + i] = Vector3.up;
            }
            for (int s = 0; s < 4; s++)
            {
                int a = n + s, b = n + (s + 1) % 4, c = n + 4 + s, d = n + 4 + (s + 1) % 4;
                tris[t++] = a; tris[t++] = b; tris[t++] = c;
                tris[t++] = b; tris[t++] = d; tris[t++] = c;
            }
            System.Array.Copy(rest, moved, rest.Length);

            mesh = new Mesh { name = "Ship stage water", indexFormat = IndexFormat.UInt32, hideFlags = HideFlags.HideAndDontSave };
            mesh.MarkDynamic();
            mesh.vertices = rest;
            mesh.normals = normals;
            mesh.colors32 = colours;
            mesh.triangles = tris;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(2f * h, 40f, 2f * h));

            Object = new GameObject("Water") { layer = layer };
            Object.transform.SetParent(parent, false);
            Object.AddComponent<MeshFilter>().sharedMesh = mesh;
            Renderer = Object.AddComponent<MeshRenderer>();
            Renderer.sharedMaterial = material;
            Renderer.shadowCastingMode = ShadowCastingMode.Off;
            return Object;
        }

        /// <summary>Moves the mesh to the sea of this moment.</summary>
        public void Tick(StageSea sea)
        {
            if (mesh == null || !moveOnCpu)
                return;
            int n = grid * grid;
            // foam on the crests: over this height, more of it the more of the sea is breaking
            float hs = Mathf.Max(sea.State.hs, 0.02f);
            float white = Mathf.Clamp01(sea.Whitecaps / 0.08f);
            float from = hs * Mathf.Lerp(0.75f, 0.2f, white);
            for (int i = 0; i < n; i++)
            {
                int x = i % grid, z = i / grid;
                if (x == 0 || z == 0 || x == grid - 1 || z == grid - 1)
                    continue;                                             // (the rim stays level with the skirts)
                Vector3 p = rest[i];
                float y = sea.Height(p.x, p.z, cellAt[i], out float sx, out float sz);
                moved[i] = new Vector3(p.x, y, p.z);
                normals[i] = new Vector3(-sx, 1f, -sz).normalized;
                colours[i] = new Color32((byte)(255f * white * Mathf.Clamp01((y - from) / (0.35f * hs))), 0, 0, 255);
            }
            mesh.vertices = moved;
            mesh.normals = normals;
            mesh.colors32 = colours;
        }

        public void Destroy()
        {
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
            mesh = null;
        }
    }
}
