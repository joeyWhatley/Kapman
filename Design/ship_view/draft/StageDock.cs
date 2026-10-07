// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/StageDock.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run: every position is worked
// out from the props' measured sizes (PLAN.md part 4) and wants looking at in the "moored" picture.
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
    /// </summary>
    public sealed class StageDock
    {
        public float length = 9.8f, deckTop = 0.55f, gap = 0.35f, pileLength = 3.4f;

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
            float w = Width, half = length * 0.5f;

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
            for (int row = -1; row <= 1; row += 2)
                for (int j = -1; j <= 1; j++)
                {
                    var s = stage.Spawn(kit.stringer, Root, "Stringer", PrimitiveType.Cube, kit.stringerSize);
                    s.transform.localPosition = new Vector3(-w * 0.5f + row * 0.7f, under, j * kit.stringerSize.x);
                    s.transform.localRotation = Quaternion.Euler(0f, 90f, 0f) * Quaternion.Euler(90f, 0f, 0f);
                }
            // the piles: the water side ones stand over the boards as posts
            float[] bents = { -half + 0.35f, -length / 6f, length / 6f, half - 0.35f };
            float stretch = pileLength / Mathf.Max(0.01f, kit.pileSize.y);
            for (int i = 0; i < bents.Length; i++)
                for (int side = 0; side < 2; side++)
                {
                    bool water = side == 0;
                    float top = deckTop + (water ? 0.45f : 0.08f);
                    var p = stage.Spawn(kit.pile, Root, water ? "Post" : "Pile", PrimitiveType.Cylinder, new Vector3(kit.pileSize.x, kit.pileSize.y * 0.5f, kit.pileSize.x));
                    p.transform.localPosition = new Vector3(water ? -0.05f : -w + 0.05f, top - pileLength, bents[i]);
                    p.transform.localRotation = Quaternion.Euler(R(1.5f), R(180f), R(1.5f));
                    p.transform.localScale = Vector3.Scale(p.transform.localScale, new Vector3(1f, stretch, 1f));
                    if (water && i == bents.Length - 1) BowPost = new Vector3(-0.05f, top - 0.1f, bents[i]);
                    if (water && i == 0) SternPost = new Vector3(-0.05f, top - 0.1f, bents[i]);
                }
            // what stands on the boards
            Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.55f, deckTop, half - 1.2f), R(180f), 1f, new Vector3(0.8f, 0.9f, 0.8f));
            Prop(stage, kit.barrel, "Barrel", new Vector3(-w + 0.62f, deckTop, half - 2.15f), R(180f), 0.8f, new Vector3(0.8f, 0.9f, 0.8f));
            Prop(stage, kit.crate, "Crate", new Vector3(-w + 0.5f, deckTop, half - 3.2f), 18f + R(10f), 1f, Vector3.one * 0.7f);
            Prop(stage, kit.sackA, "Sack", new Vector3(-w + 0.45f, deckTop, -half + 1.3f), 40f + R(30f), 1f, new Vector3(0.6f, 1f, 0.4f));
            Prop(stage, kit.sackB, "Sack", new Vector3(-w + 0.95f, deckTop, -half + 1.9f), R(180f), 1f, new Vector3(0.6f, 0.75f, 0.6f));
            Prop(stage, kit.ropeCoil, "Rope", new Vector3(-0.6f, deckTop, -half + 0.95f), R(180f), 0.55f, new Vector3(1f, 0.3f, 1.3f));
            Prop(stage, kit.bucket, "Bucket", new Vector3(-0.55f, deckTop, 1.2f + R(0.4f)), R(180f), 0.5f, new Vector3(0.6f, 0.7f, 0.6f));

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
            Vector2[] across = { new Vector2(-w + 0.35f, -1.6f), new Vector2(-w - 0.4f, deckTop - 0.13f), new Vector2(-w - 3.5f, deckTop + 0.2f),
                                 new Vector2(-w - 14f, deckTop + 0.75f), new Vector2(-w - 60f, deckTop + 1.65f) };
            const int steps = 12;
            const float along = 180f;
            var v = new Vector3[(steps + 1) * across.Length];
            var tris = new int[steps * (across.Length - 1) * 6];
            for (int i = 0; i <= steps; i++)
                for (int k = 0; k < across.Length; k++)
                {
                    float z = -along * 0.5f + along * i / steps;
                    float lift = k < 2 ? 0f : 0.25f * Mathf.Sin(i * 1.7f + k * 2.3f);
                    v[i * across.Length + k] = new Vector3(across[k].x, across[k].y + lift, z);
                }
            int t = 0;
            for (int i = 0; i < steps; i++)
                for (int k = 0; k < across.Length - 1; k++)
                {
                    int a = i * across.Length + k, b = a + across.Length;
                    // (seen from above with x across and z along: a, a + 1 lies towards -x, so this order faces up)
                    tris[t++] = a; tris[t++] = a + 1; tris[t++] = b;
                    tris[t++] = a + 1; tris[t++] = b + 1; tris[t++] = b;
                }
            bank = new Mesh { name = "Ship stage bank", hideFlags = HideFlags.HideAndDontSave };
            bank.vertices = v;
            bank.triangles = tris;
            bank.RecalculateNormals();
            bank.RecalculateBounds();
            var go = new GameObject("Bank");
            go.transform.SetParent(Root, false);
            go.AddComponent<MeshFilter>().sharedMesh = bank;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = stage.Lit(kit.shore, new Color(0.33f, 0.3f, 0.2f), 0.15f);
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
