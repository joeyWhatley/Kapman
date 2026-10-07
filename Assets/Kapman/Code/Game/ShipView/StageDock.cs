// First run and looked at on 7 Oct 2026 (Design/ship_view/BUILD_LOG.md). Every position is worked out from the props'
// measured sizes (PLAN.md part 4).
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE JETTY SHE LIES AT (the user, 7 Oct 2026: "when it docks just a simple dock model maybe we already have one
    /// from my assets if not just make one"). The pack has no dock model; it has planks, a short log, barrels, crates,
    /// sacks and rope, and this builds a timber staging along a bank from them:
    ///   20 deck boards    Plank_2 (2.12 m long), laid across, 0.49 m apart: a staging 2.12 m wide and 9.8 m long
    ///    6 stringers      Plank_3 (3.09 m), on edge under the boards, two rows of three
    ///    8 piles          Short_log_1 (0.33 m across) stretched to 3.4 m, four on the water side standing 0.45 m over
    ///                     the boards as mooring posts, four on the land side
    ///    2 barrels, 1 crate, 2 sacks, 1 coil of rope, 1 bucket on the boards
    ///    the bank         a strip of ground behind, made here (five points across, 180 m long)
    /// Its own axes are hers when she lies at it: z along the staging the way her bow points, x towards her. The water
    /// side edge of the boards is x = 0 and the staging runs back to x = -2.12; y = 0 is the still water. She lies with
    /// her port side to it (her steering oar is on the other), her side <see cref="gap"/> metres off the boards.
    /// Boards and props are set a little askew by a seed (the port's name), so no two ports' stagings are the same.
    /// NO LAND (the user, 7 Oct 2026: "you cant see anything other than the boat and the water and just the port no
    /// land"; the kit's `land` is off): there is no bank. A staging ten metres long standing by itself in open water
    /// reads as a raft, so without land it is a PIER, kit.pierLength long (48 m): the same boards on the same piles,
    /// run on past both edges of the picture, with water and mist behind it. A post stands every three metres along its
    /// water side; her lines go to the two by her bow and her stern.
    /// </summary>
    public sealed class StageDock
    {
        public float length = 9.8f, deckTop = 0.55f, gap = 0.35f, pileLength = 3.4f;
        /// <summary>The staging's length where there is a bank behind it (and the reach of the props near her either way).</summary>
        const float StagingLength = 9.8f;

        public Transform Root { get; private set; }
        /// <summary>The tops of the two mooring posts her lines go to (in Root's axes).</summary>
        public Vector3 BowPost { get; private set; }
        public Vector3 SternPost { get; private set; }
        public float Width { get; private set; } = 2.12f;

        Mesh bank;

        public void Build(ShipStage stage, ShipStageKit kit, Transform parent, string seedName)
        {
            if (Root != null)
                Destroy();
            int seed = 17;
            foreach (char c in seedName ?? "")
                seed = seed * 31 + c;
            var dice = new System.Random(seed);
            float R(float span) => ((float)dice.NextDouble() * 2f - 1f) * span;

            Root = new GameObject("Jetty").transform;
            Root.SetParent(parent, false);
            Width = kit.deckPlankSize.x;
            length = kit.land ? StagingLength : Mathf.Max(StagingLength, kit.pierLength);
            float w = Width, half = length * 0.5f, near = StagingLength * 0.5f;

            // the boards, across
            float pitch = kit.deckPlankSize.z + 0.02f;
            int boards = Mathf.Max(1, Mathf.RoundToInt(length / pitch));
            for (int i = 0; i < boards; i++)
            {
                var b = stage.Spawn(kit.deckPlank, Root, "Board", PrimitiveType.Cube, kit.deckPlankSize);
                b.transform.localPosition = new Vector3(-w * 0.5f + R(0.03f), deckTop - kit.deckPlankSize.y * 0.56f, -half + pitch * (i + 0.5f));
                b.transform.localRotation = Quaternion.Euler(R(0.6f), R(1.5f), R(0.4f));
            }
            // the stringers, fore and aft on edge under the boards
            float under = deckTop - kit.deckPlankSize.y - kit.stringerSize.z * 0.5f;
            int spans = Mathf.Max(3, Mathf.CeilToInt(length / Mathf.Max(0.5f, kit.stringerSize.x)));
            for (int row = -1; row <= 1; row += 2)
                for (int j = 0; j < spans; j++)
                {
                    float z = (j - (spans - 1) * 0.5f) * kit.stringerSize.x;
                    // (the land side row is only ever glimpsed under the boards near her: far along the pier it is left out)
                    if (row < 0 && Mathf.Abs(z) > near + kit.stringerSize.x)
                        continue;
                    var s = stage.Spawn(kit.stringer, Root, "Stringer", PrimitiveType.Cube, kit.stringerSize);
                    s.transform.localPosition = new Vector3(-w * 0.5f + row * 0.7f, under, z);
                    s.transform.localRotation = Quaternion.Euler(0f, 90f, 0f) * Quaternion.Euler(90f, 0f, 0f);
                }
            // the piles: the water side ones stand over the boards as posts. With a bank, four bents (the short staging);
            // without, a bent every three metres all along the pier, placed so that two of them stand where the short
            // staging's end posts stood, by her bow and her stern.
            float[] bents;
            if (kit.land)
                bents = new[] { -half + 0.35f, -length / 6f, length / 6f, half - 0.35f };
            else
            {
                float apart = (near - 0.35f) * 2f / 3f;                    // (3.03 m: a post at +-1.52, +-4.55, +-7.58 ...)
                int n = Mathf.Max(2, Mathf.FloorToInt((length - 0.7f) / apart / 2f) * 2 + 2);
                while ((n - 1) * 0.5f * apart > half - 0.3f)
                    n -= 2;
                bents = new float[n];
                for (int i = 0; i < n; i++)
                    bents[i] = (i - (n - 1) * 0.5f) * apart;
            }
            float stretch = pileLength / Mathf.Max(0.01f, kit.pileSize.y);
            float bowAt = near - 0.35f, sternAt = -(near - 0.35f), bowBest = float.MaxValue, sternBest = float.MaxValue;
            for (int i = 0; i < bents.Length; i++)
                for (int side = 0; side < 2; side++)
                {
                    bool water = side == 0;
                    if (!water && !kit.land && i % 2 == 1)
                        continue;                                         // (the back row: every other bent is enough to be seen through the gaps)
                    float top = deckTop + (water ? 0.45f : 0.08f);
                    var p = stage.Spawn(kit.pile, Root, water ? "Post" : "Pile", PrimitiveType.Cylinder, new Vector3(kit.pileSize.x, kit.pileSize.y * 0.5f, kit.pileSize.x));
                    p.transform.localPosition = new Vector3(water ? -0.05f : -w + 0.05f, top - pileLength, bents[i]);
                    p.transform.localRotation = Quaternion.Euler(R(1.5f), R(180f), R(1.5f));
                    p.transform.localScale = Vector3.Scale(p.transform.localScale, new Vector3(1f, stretch, 1f));
                    if (water && Mathf.Abs(bents[i] - bowAt) < bowBest)
                    {
                        bowBest = Mathf.Abs(bents[i] - bowAt);
                        BowPost = new Vector3(-0.05f, top - 0.1f, bents[i]);
                    }
                    if (water && Mathf.Abs(bents[i] - sternAt) < sternBest)
                    {
                        sternBest = Mathf.Abs(bents[i] - sternAt);
                        SternPost = new Vector3(-0.05f, top - 0.1f, bents[i]);
                    }
                }
            // what stands on the boards, near her
            Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.55f, deckTop, near - 1.2f), R(180f), 1f, new Vector3(0.8f, 0.9f, 0.8f));
            Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.62f, deckTop, near - 2.15f), R(180f), 0.8f, new Vector3(0.8f, 0.9f, 0.8f));
            Prop(stage, kit.crate, "Crate", new Vector3(-w + 0.5f, deckTop, near - 3.2f), 18f + R(10f), 1f, Vector3.one * 0.7f);
            Prop(stage, kit.sackA, "Sack", new Vector3(-w + 0.45f, deckTop, -near + 1.3f), 40f + R(30f), 1f, new Vector3(0.6f, 1f, 0.4f));
            Prop(stage, kit.sackB, "Sack", new Vector3(-w + 0.95f, deckTop, -near + 1.9f), R(180f), 1f, new Vector3(0.6f, 0.75f, 0.6f));
            Prop(stage, kit.ropeCoil, "Rope", new Vector3(-0.6f, deckTop, -near + 0.95f), R(180f), 0.55f, new Vector3(1f, 0.3f, 1.3f));
            Prop(stage, kit.bucket, "Bucket", new Vector3(-0.55f, deckTop, 1.2f + R(0.4f)), R(180f), 0.5f, new Vector3(0.6f, 0.7f, 0.6f));
            if (!kit.land && half > near + 8f)
            {
                // and a little more of the same further along the pier, either way
                Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.6f, deckTop, near + 5.6f + R(0.5f)), R(180f), 0.9f, new Vector3(0.8f, 0.9f, 0.8f));
                Prop(stage, kit.sackA, "Sack", new Vector3(-w + 0.5f, deckTop, near + 6.7f + R(0.3f)), R(180f), 1f, new Vector3(0.6f, 1f, 0.4f));
                Prop(stage, kit.crate, "Crate", new Vector3(-w + 0.55f, deckTop, -near - 4.8f + R(0.5f)), R(25f), 1f, Vector3.one * 0.7f);
                Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.6f, deckTop, -near - 6.1f + R(0.4f)), R(180f), 1f, new Vector3(0.8f, 0.9f, 0.8f));
            }

            if (kit.land)
                Bank(stage, kit, w);
            stage.Adopt(Root.gameObject);
        }

        void Prop(ShipStage stage, GameObject prefab, string name, Vector3 at, float yaw, float scale, Vector3 standIn)
        {
            var g = stage.Spawn(prefab, Root, name, PrimitiveType.Cube, standIn);
            g.transform.localPosition = at;
            g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            g.transform.localScale = g.transform.localScale * scale;
        }

        /// <summary>The bank behind the staging: a foreshore that comes up out of the water under the back of the boards,
        /// a low bank at their height, and the ground rising gently behind. One strip, five points across.</summary>
        void Bank(ShipStage stage, ShipStageKit kit, float w)
        {
            // across (x from the boards' water side, height over the still water): the foreshore's mud coming up from
            // under the water to the bank's lip, then grass rising slowly to a low skyline well behind, higher than the
            // camera's eye so that no water shows over the land. The first two strips are mud, the rest grass.
            Vector2[] across = { new Vector2(-w + 0.6f, -1.6f), new Vector2(-w - 0.5f, deckTop - 0.2f), new Vector2(-w - 1.2f, deckTop + 0.02f),
                                 new Vector2(-w - 6f, deckTop + 0.45f), new Vector2(-w - 22f, deckTop + 1.2f), new Vector2(-w - 70f, deckTop + 2.6f),
                                 new Vector2(-w - 220f, deckTop + 6.5f) };
            const int mudStrips = 2;
            const int steps = 40;
            const float along = 900f;
            var v = new Vector3[(steps + 1) * across.Length];
            var mud = new System.Collections.Generic.List<int>();
            var grass = new System.Collections.Generic.List<int>();
            for (int i = 0; i <= steps; i++)
                for (int k = 0; k < across.Length; k++)
                {
                    // (finer near the staging, where it is looked at: the steps crowd towards the middle)
                    float u = i / (float)steps * 2f - 1f;
                    float z = along * 0.5f * Mathf.Sign(u) * Mathf.Pow(Mathf.Abs(u), 1.8f);
                    // the ground is never ruler-straight: the lip wanders a little, the land behind rolls more the further back
                    float roll = k < 2 ? 0f : (Mathf.PerlinNoise(z * 0.021f + 3.7f, k * 1.9f) - 0.5f) * (0.25f + 0.55f * k);
                    float wander = k == 0 ? 0f : (Mathf.PerlinNoise(z * 0.05f + 11.3f, k * 0.7f) - 0.5f) * (k < 3 ? 0.5f : 2.5f);
                    v[i * across.Length + k] = new Vector3(across[k].x + wander, across[k].y + roll, z);
                }
            for (int i = 0; i < steps; i++)
                for (int k = 0; k < across.Length - 1; k++)
                {
                    int a = i * across.Length + k, b = a + across.Length;
                    var into = k < mudStrips ? mud : grass;
                    // (seen from above with x across and z along: a, a + 1 lies towards -x, so this order faces up)
                    into.Add(a); into.Add(a + 1); into.Add(b);
                    into.Add(a + 1); into.Add(b + 1); into.Add(b);
                }
            bank = new Mesh { name = "Ship stage bank", hideFlags = HideFlags.HideAndDontSave };
            bank.vertices = v;
            bank.subMeshCount = 2;
            bank.SetTriangles(mud, 0);
            bank.SetTriangles(grass, 1);
            bank.RecalculateNormals();
            bank.RecalculateBounds();
            var go = new GameObject("Bank");
            go.transform.SetParent(Root, false);
            go.AddComponent<MeshFilter>().sharedMesh = bank;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterials = new[] { stage.Lit(null, new Color(0.23f, 0.19f, 0.14f), 0.3f), stage.Lit(kit.shore, new Color(0.26f, 0.3f, 0.16f), 0.1f) };
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Destroy()
        {
            if (Root != null)
                Object.Destroy(Root.gameObject);
            if (bank != null)
                Object.Destroy(bank);
            Root = null;
            bank = null;
        }
    }
}
