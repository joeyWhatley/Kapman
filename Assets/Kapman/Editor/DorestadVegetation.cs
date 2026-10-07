using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// PLANTING WITH STRUCTURE - trees where the land cover and the wood-density map put them, not an even sprinkle:
    /// alder carr closed in its cores (willow and osier beneath the alder) and thinning at its edges, ash and oak on the
    /// levee woods and the old ridges, birch on the peat's edge, copses and lone standards on the open land, coppice
    /// stools (the harbour's 50,000-100,000 posts came from somewhere) on the shores and in a ring behind the town,
    /// osier and willow scrub on the flat shores and bars (none on the cut banks), a willow fringe with an alder belt
    /// behind it on the far bank, hedgerows as continuous lines along the fields, yards and tracks (a shrub scan for
    /// hawthorn, osier on the wet ditch sides, a gateway now and then and an ash every so often), an apple tree or two
    /// in the yards, and a big tree at most farms. The four CC0 tree scans stand in for alder, ash, oak and willow until a
    /// tree pack is picked, each in four leaf tints and an uneven scale so no two stand alike. Their far level is an
    /// impostor baked from the scan itself (Impostor), so from the air the woods read as crowns, not black sticks. The
    /// trees are the terrain's own (instanced, cheap, hundreds of thousands), so the woods can be as thick as the
    /// pollen says they were. Reeds() lays the reed beds after the land cover's details.
    /// </summary>
    public static class DorestadVegetation
    {
        const int Ash = 0, Oak = 1, Willow = 2, Osier = 3, Shrub = 4, Poplar = 5;
        static readonly string[] Kinds = { "tree_small_02", "island_tree_01", "island_tree_02", "island_tree_03", "shrub_02", "poplar" };
        // the bought meadow pack (MeadowPack) stands in where it fits, one prefab per tint slot: grey willow (Salix cinerea,
        // the wet-ground willow) for the osier, field maple for the hedge plant, black poplar (Populus nigra, the Rhine
        // floodplain's own) as a kind of its own. Without the pack the CC0 scans stand in and the poplars become ash.
        static readonly string[][] PackKinds =
        {
            null, null, null,
            new[] { "Bushes/Prefabs/prefab_grey_willow_01.prefab", "Bushes/Prefabs/prefab_grey_willow_02.prefab", "Bushes/Prefabs/prefab_grey_willow_03.prefab", "Bushes/Prefabs/prefab_grey_willow_04.prefab" },
            new[] { "Bushes/Prefabs/prefab_maple_bush_01.prefab", "Bushes/Prefabs/prefab_maple_bush_02.prefab", "Bushes/Prefabs/prefab_maple_bush_03.prefab", "Bushes/Prefabs/prefab_maple_bush_04.prefab" },
            new[] { "Trees/Baked and Optimised Poplar Prefabs/prefab_Baked_01_Poplar.prefab", "Trees/Baked and Optimised Poplar Prefabs/prefab_Baked_03_Poplar.prefab", "Trees/Baked and Optimised Poplar Prefabs/prefab_Baked_05_Poplar.prefab", "Trees/Baked and Optimised Poplar Prefabs/prefab_Baked_07_Poplar.prefab" },
        };
        // the scans are 2.6-5 m tall: grown toward the river land's trees (alder and ash 9-14 m, oaks 12-16 m, willows
        // 4-7 m) but no more than twice, as blown-up scans read as giant single leaves up close; the woods' mass comes
        // from their count instead. The shrub's entry is worked out from its own height, so Put() gives a hedge plant
        // its height in metres.
        static readonly float[] BaseScale = { 1.8f, 2.0f, 1.3f, 1.4f, 1f, 1f };
        // leaf tints: plain, two greener ones (the shore kinds, so the fringe reads dark against the water), one yellowish (birch)
        static readonly Color[] Tints = { new Color(1f, 1f, 1f), new Color(0.66f, 0.84f, 0.58f), new Color(0.80f, 0.94f, 0.62f), new Color(1.08f, 1.0f, 0.80f) };
        public const int MaxTrees = 320000;
        const string NatureDir = "Assets/Kapman/Prefabs/Nature";
        const string TintDir = NatureDir + "/Tints", ImpostorDir = NatureDir + "/Impostors";
        public const string ReedPrefabPath = NatureDir + "/reed_clump.prefab";
        const float ImpostorCutoff = 0.5f;

        static string Full(string assetPath) => Path.Combine(Directory.GetCurrentDirectory(), assetPath);

        public static void Plant(DorestadBuilder.Grid g, DorestadTown.Plan plan, byte[] cover, byte[] wood, Terrain terrain, Transform parent, System.Random rnd,
                                 out Material[] leafMaterials, out Color[] leafColours, out float[] leafCutoffs)
        {
            float R() => (float)rnd.NextDouble();
            float Range(float a, float b) => a + R() * (b - a);
            var td = terrain.terrainData;
            Directory.CreateDirectory(Full(TintDir));
            Directory.CreateDirectory(Full(ImpostorDir));

            // the prototypes: each kind in each tint, as its own prefab (the terrain's trees take no per-instance
            // colour), with the far level swapped for an impostor baked from the scan
            var protos = new List<TreePrototype>();
            var allLeaves = new List<Material>();
            var allColours = new List<Color>();
            var allCutoffs = new List<float>();
            var baseScale = (float[])BaseScale.Clone();
            bool shrubOk = false, poplarOk = false;
            for (int i = 0; i < Kinds.Length; i++)
            {
                // the meadow pack's plants, where installed: their own LODs, materials and wind; real size
                if (PackKinds[i] != null)
                {
                    var packs = PackKinds[i].Select(MeadowPack.Prefab).ToArray();
                    if (packs.All(pf => pf != null))
                    {
                        float tall = packs.Average(MeadowPack.Height);
                        // the osier's Put() scales sit round 0.8 (a bush its own size there); the hedge plant's are its height in
                        // metres; the poplar stands at its own height
                        baseScale[i] = i == Shrub ? 1f / Mathf.Max(0.3f, tall) : i == Osier ? 1.25f : 1f;
                        for (int t = 0; t < Tints.Length; t++)
                            protos.Add(new TreePrototype { prefab = packs[t % packs.Length], bendFactor = 0f });
                        if (i == Shrub) shrubOk = true;
                        if (i == Poplar) poplarOk = true;
                        Debug.Log($"Kapman planting: {Kinds[i]} from the meadow pack ({tall:0.0} m).");
                        continue;
                    }
                    if (i == Poplar)
                    {
                        // no pack: the poplars are ash; keep the slots so the prototype indices stay kind * tints + tint
                        for (int t = 0; t < Tints.Length; t++)
                            protos.Add(protos[Ash * Tints.Length + t]);
                        continue;
                    }
                }
                GameObject basePrefab;
                Material baseLeaves;
                if (i == Shrub)
                {
                    // the hedge plant (hawthorn's stand-in): a Poly Haven shrub scan, brought in through the grass
                    // route (its textures are named like a grass clump's), standing on y = 0
                    try
                    {
                        basePrefab = AssetLibrary.Grass(Kinds[i], $"{NatureDir}/{Kinds[i]}.prefab", 14000);
                        var mr = basePrefab.GetComponentInChildren<MeshRenderer>();
                        var mf = basePrefab.GetComponentInChildren<MeshFilter>();
                        baseLeaves = mr != null ? mr.sharedMaterial : null;
                        float tall = mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds.size.y : 0f;
                        if (tall < 0.2f || baseLeaves == null)
                            throw new System.Exception("no mesh or material in the shrub prefab");
                        baseScale[i] = 1f / tall;
                        shrubOk = true;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Kapman planting: {Kinds[i]} not brought in ({e.Message}); osier stands in for the hedge plant.");
                        continue;
                    }
                }
                else
                    basePrefab = AssetLibrary.Tree(Kinds[i], $"{NatureDir}/{Kinds[i]}.prefab", out baseLeaves);

                Mesh quads = null;
                Texture2D atlas = null;
                bool imp = false;
                try
                {
                    imp = Impostor(basePrefab, baseLeaves, Kinds[i], out quads, out atlas);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Kapman planting: no impostor for {Kinds[i]} ({e.Message}); its far level stays the thinned scan.");
                }

                for (int t = 0; t < Tints.Length; t++)
                {
                    string matPath = $"{TintDir}/{Kinds[i]}_leaves_{t}.mat";
                    var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (m == null && baseLeaves != null)
                    {
                        m = new Material(baseLeaves);
                        AssetDatabase.CreateAsset(m, matPath);
                    }
                    if (m != null)
                    {
                        AssetLibrary.Upgrade(m);
                        m.SetColor("_BaseColor", Tints[t]);
                        EditorUtility.SetDirty(m);
                        allLeaves.Add(m);
                        allColours.Add(Tints[t]);
                        allCutoffs.Add(Cutoff(m));
                    }
                    Material im = null;
                    if (imp)
                    {
                        string impPath = $"{ImpostorDir}/{Kinds[i]}_impostor_{t}.mat";
                        im = AssetDatabase.LoadAssetAtPath<Material>(impPath);
                        if (im == null)
                        {
                            im = AssetLibrary.NewLit(atlas, null, 0.05f, Dim(Tints[t]));
                            AssetLibrary.Cutout(im, ImpostorCutoff, false);
                            AssetDatabase.CreateAsset(im, impPath);
                        }
                        else
                        {
                            AssetLibrary.Upgrade(im);
                            im.SetTexture("_BaseColorMap", atlas);
                            im.SetColor("_BaseColor", Dim(Tints[t]));
                            EditorUtility.SetDirty(im);
                        }
                        allLeaves.Add(im);
                        allColours.Add(Dim(Tints[t]));                       // (the impostor's own, darker colour)
                        allCutoffs.Add(ImpostorCutoff);
                    }
                    var variant = Variant(basePrefab, i == Shrub, baseLeaves, m, imp ? quads : null, im, $"{TintDir}/{Kinds[i]}_t{t}.prefab");
                    protos.Add(new TreePrototype { prefab = variant, bendFactor = 0f });
                }
            }
            // the reed clump's material joins the weather's leaves: green through the summer, straw in the autumn
            ReedClump(out var reedMat);
            if (reedMat != null)
            {
                allLeaves.Add(reedMat);
                allColours.Add(new Color(0.78f, 0.88f, 0.62f));
                allCutoffs.Add(0.5f);
            }
            leafMaterials = allLeaves.ToArray();
            leafColours = allColours.ToArray();
            leafCutoffs = allCutoffs.ToArray();
            td.treePrototypes = protos.ToArray();

            var trees = new List<TreeInstance>(MaxTrees);
            var later = new List<(int kind, string group, Vector2 p, float lo, float hi, int tint)>();
            var counts = new Dictionary<string, int>();
            float size = td.size.x;
            void Put(int kind, string group, Vector2 p, float scaleLo, float scaleHi, int tint = -1)
            {
                if (trees.Count >= MaxTrees || p.x < 1f || p.y < 1f || p.x > size - 1f || p.y > size - 1f)
                    return;
                if (kind == Poplar && !poplarOk)
                    kind = Ash;
                if (kind == Shrub && !shrubOk)
                {
                    kind = Osier;
                    scaleLo = 0.42f;
                    scaleHi = 0.58f;
                }
                float s = baseScale[kind] * Range(scaleLo, scaleHi);
                int ti = tint >= 0 ? tint : rnd.Next(3);
                trees.Add(new TreeInstance
                {
                    position = new Vector3(p.x / size, 0f, p.y / size),
                    prototypeIndex = kind * Tints.Length + ti,
                    widthScale = s * Range(0.9f, 1.1f),
                    heightScale = s * Range(0.88f, 1.15f),
                    rotation = R() * Mathf.PI * 2f,
                    color = Color.white,
                    lightmapColor = Color.white,
                });
                counts[group] = counts.TryGetValue(group, out int c) ? c + 1 : 1;
            }
            // the woods go in after everything near the water and the town, so the cap (if it is reached) thins the
            // woods and not the hedges and fringes
            void PutLater(int kind, string group, Vector2 p, float scaleLo, float scaleHi, int tint = -1) => later.Add((kind, group, p, scaleLo, scaleHi, tint));
            Vector2 Off(float lo, float hi)
            {
                float a = R() * Mathf.PI * 2f, d = Range(lo, hi);
                return new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d);
            }

            byte Cover(Vector2 p) => cover != null ? DorestadLandCover.Sample(cover, g, p) : DorestadLandCover.Crest;
            float Wood(Vector2 p) => wood != null ? DorestadLandCover.Sample(wood, g, p) / 255f : 0f;
            bool Worked(byte u) => u == (byte)DorestadTown.Use.Field || u == (byte)DorestadTown.Use.Farmyard || u == (byte)DorestadTown.Use.Yard
                                || u == (byte)DorestadTown.Use.Orchard || u == (byte)DorestadTown.Use.Cemetery || u == (byte)DorestadTown.Use.Market;
            Vector2 townC = new Vector2(plan.townCentre.x, plan.townCentre.z);

            // the town's side of the river: the apex parcel's outward points across the water. The far bank is where
            // the straight line to the town's centre crosses water; the ferry landing lies straight across from the
            // apex dam head (a guess: a ferry is attested in the notes, not its place)
            Vector2 outw = Vector2.zero, apexHead = townC;
            if (plan.parcels != null && plan.parcels.Count > 0)
            {
                var apex = plan.parcels.OrderBy(pc => Mathf.Abs(pc.s - 850f)).First();
                outw = apex.outward;
                apexHead = apex.bank + apex.outward * apex.damLength;
            }
            Vector2 ferry = apexHead;
            if (outw != Vector2.zero)
                for (float k = 2f; k < 500f; k += 2f)
                {
                    var q = apexHead + outw * k;
                    if (g.SD(q) < -1f)
                    {
                        ferry = q;
                        break;
                    }
                }
            bool Across(Vector2 p)
            {
                for (int i = 1; i < 16; i++)
                    if (g.SD(Vector2.Lerp(p, townC, i / 16f)) > 0f)
                        return true;
                return false;
            }

            const float Step = 5f;
            var eight = new[]
            {
                new Vector2(4f, 0f), new Vector2(-4f, 0f), new Vector2(0f, 4f), new Vector2(0f, -4f),
                new Vector2(2.83f, 2.83f), new Vector2(-2.83f, 2.83f), new Vector2(2.83f, -2.83f), new Vector2(-2.83f, -2.83f),
            };
            // a hedge along the boundary between this (unworked) cell and the worked land on the side n points to: a
            // continuous line of plants across the cell's width, a gateway now and then, a standard tree every so often
            void Hedge(Vector2 p, Vector2 n, byte cv, float h)
            {
                if (n.sqrMagnitude < 1e-3f)
                    return;
                n.Normalize();
                Vector2 t = new Vector2(-n.y, n.x);
                float d = -1f;
                for (float k = 0.5f; k <= 3.2f; k += 0.5f)
                    if (Worked(plan.Get(p + n * k)))
                    {
                        d = k;
                        break;
                    }
                if (d < 0f)
                    return;
                Vector2 c = p + n * Mathf.Max(d - 0.6f, 0f);
                if (Mathf.PerlinNoise(c.x / 7f + 3f, c.y / 7f + 1f) > 0.74f)
                    return;                                                       // a gateway
                if (Mathf.PerlinNoise(c.x / 11f + 51f, c.y / 11f + 17f) > 0.78f)
                    Put(Ash, "Hedgerow trees", c + t * Range(-1f, 1f), 0.9f, 1.15f);
                bool wet = h < 0.45f || cv == DorestadLandCover.Basin || cv == DorestadLandCover.Carr || cv == DorestadLandCover.Reed || cv == DorestadLandCover.Peat;
                float spacing = Range(0.7f, 1.1f);
                int count = Mathf.Max(3, Mathf.RoundToInt(Step / spacing));
                for (int i = 0; i < count; i++)
                {
                    float along = (i + 0.5f) / count * Step - Step / 2f + Range(-0.2f, 0.2f);
                    Vector2 q = c + t * along + n * Range(-0.25f, 0.25f);
                    if (wet)
                        Put(Osier, "Hedgerows (osier on the wet sides)", q, 0.42f, 0.58f);
                    else
                        Put(Shrub, "Hedgerows", q, 1.5f, 2.0f);
                }
            }

            // the cells in a scrambled order, so the cap (if it is reached) thins the whole level evenly instead of
            // leaving the north bare (as it did on 3 October)
            int cellsAcross = Mathf.CeilToInt(g.size / Step);
            long cellCount = (long)cellsAcross * cellsAcross;
            for (long k = 0; k < cellCount; k++)
            {
                long idx = (k * 7919L) % cellCount;
                float x = (idx % cellsAcross) * Step + Step / 2f, z = (idx / cellsAcross) * Step + Step / 2f;
                var p = new Vector2(x + Range(-2.2f, 2.2f), z + Range(-2.2f, 2.2f));
                byte use = plan.Get(p);
                float sd = g.SD(p);
                if (sd > -1.5f || use == 255)
                    continue;
                byte cv = Cover(p);
                float wd = Wood(p);
                float h = g.H(p) - g.wl;
                float dTown = Vector2.Distance(p, townC);

                // the plan's orchard plots: a meadow with an apple tree or two (orchard blocks round Wijk are
                // post-medieval; for 800 apple is a collected or garden fruit)
                if (use == (byte)DorestadTown.Use.Orchard)
                {
                    if (R() < 0.025f)
                        Put(Willow, "Apple trees", p, 0.75f, 0.9f, 1);
                    continue;
                }
                // the town's own hedge lines (Use.Hedge from DorestadTown): osier with the odd ash standard
                if (use == (byte)DorestadTown.Use.Hedge)
                {
                    if (R() < 0.8f)
                        Put(R() < 0.85f ? Osier : Ash, "Hedges", p, 0.6f, 0.9f);
                    continue;
                }
                // hedgerows: where worked land meets the rest (the planting-hole rows of the excavations), or along a track
                bool edge = false, trackSide = false;
                Vector2 n = Vector2.zero;
                foreach (var o in eight)
                {
                    byte u2 = plan.Get(p + o);
                    if (u2 == 255)
                        continue;
                    if (Worked(use) != Worked(u2) && use != (byte)DorestadTown.Use.Yard && u2 != (byte)DorestadTown.Use.Yard)
                    {
                        edge = true;
                        if (Worked(u2))
                            n += o;
                    }
                    if (u2 == (byte)DorestadTown.Use.Track && use != (byte)DorestadTown.Use.Track)
                        trackSide = true;
                }
                if (use == (byte)DorestadTown.Use.Lane || use == (byte)DorestadTown.Use.Track || use == (byte)DorestadTown.Use.Shoal
                    || use == (byte)DorestadTown.Use.Market || use == (byte)DorestadTown.Use.Yard || use == (byte)DorestadTown.Use.Foreshore)
                    continue;
                if (edge && !Worked(use))
                {
                    Hedge(p, n, cv, h);
                    continue;
                }
                if (trackSide && R() < 0.15f)
                {
                    Put(R() < 0.7f ? Osier : Ash, "Hedgerows", p, 0.6f, 0.95f);
                    continue;
                }
                if (Worked(use))
                {
                    // a big tree on most yards' edges (the notes' farmyard trees), and an apple tree or two inside the fence
                    if (use == (byte)DorestadTown.Use.Farmyard && edge)
                    {
                        float r = R();
                        if (r < 0.08f)
                            Put(Oak, "Farm standards", p, 1.1f, 1.35f);
                        else if (r < 0.13f)
                            Put(Willow, "Apple trees", p, 0.75f, 0.9f, 1);
                    }
                    else if (use == (byte)DorestadTown.Use.Cemetery && R() < 0.01f)
                        Put(Oak, "Farm standards", p, 1.0f, 1.3f);
                    continue;
                }

                // the far bank: from the water it is half the picture. A willow and osier fringe along the water
                // (thin on the sand bars, so they show), an alder belt 20-40 m deep behind it, an open ferry landing
                // straight across from the apex
                if (dTown < 1200f && sd > -90f && Across(p))
                {
                    if (Vector2.Distance(p, ferry) < 60f)
                        continue;
                    float slope = Slope(g, p);
                    if (sd > -50f)
                    {
                        float chance = cv == DorestadLandCover.Bar ? 0.1f : slope < 0.12f ? 0.45f : 0.06f;
                        if (R() < chance)
                            Put(R() < 0.55f ? Osier : Willow, "The far bank: willow and osier fringe", p, 0.75f, 1.2f, R() < 0.5f ? 1 : 2);
                    }
                    else if (-sd < 70f + 20f * Mathf.PerlinNoise(p.x / 40f + 2f, p.y / 40f + 5f))
                    {
                        float r = R();
                        if (r < 0.3f)
                            Put(R() < 0.2f ? Poplar : Ash, "The far bank: alder belt", p, 0.85f, 1.15f, 1);
                        else if (r < 0.35f)
                            Put(Oak, "The far bank: alder belt", p, 0.95f, 1.25f);
                    }
                    continue;
                }

                // the shores: osier beds and willow on the flat insides of the bends, bare cut banks; within 600 m of
                // the town the shore is cut over and over for wattle and posts: coppice stools, not trees
                if (sd > -18f || cv == DorestadLandCover.Mud || cv == DorestadLandCover.Bar)
                {
                    float slope = Slope(g, p);
                    bool coppice = dTown < 600f;
                    float dense = slope < 0.06f ? (coppice ? 0.5f : 0.36f) : 0.05f;
                    float bed = Mathf.PerlinNoise(p.x / 45f + 3f, p.y / 45f + 9f);
                    if (R() < dense * (0.3f + bed))
                    {
                        if (coppice)
                            Put(Osier, "Coppice stools on the shore", p, 0.45f, 0.7f, R() < 0.5f ? 1 : 2);
                        else
                            Put(R() < 0.08f ? Poplar : R() < 0.6f ? Osier : Willow, "Osier beds and willows on the shore", p, 0.75f, 1.25f, R() < 0.5f ? 1 : 2);
                    }
                    continue;
                }
                // the coppice ring behind the vicus: the levee was cut for the harbour's posts and wattle
                if (dTown > 250f && dTown < 600f && (cv == DorestadLandCover.Crest || cv == DorestadLandCover.Flank || cv == DorestadLandCover.LeveeWood))
                {
                    float glade = Mathf.PerlinNoise(p.x / 60f + 7f, p.y / 60f + 11f);
                    if (R() < 0.25f * (0.4f + glade))
                        Put(R() < 0.6f ? Osier : Ash, "Coppice behind the town", p, 0.5f, 0.75f);
                    continue;
                }

                switch (cv)
                {
                    case DorestadLandCover.Carr:
                        // closed canopy in the cores: alder (ash stands in) and willow, with osier beneath and a second
                        // tree where the wood map is thickest
                        if (R() < wd * 1.1f + 0.08f)
                            PutLater(R() < 0.72f ? Ash : Willow, "Alder carr", p, 0.85f, 1.25f);
                        else if (R() < 0.08f)
                            PutLater(Osier, "Alder carr", p, 0.7f, 1.0f);
                        if (wd > 0.6f)
                            PutLater(Osier, "Alder carr: willow and osier beneath", p + Off(2f, 3f), 0.5f, 0.8f);
                        if (wd > 0.75f && R() < 0.95f)
                            PutLater(R() < 0.6f ? Ash : Willow, "Alder carr", p + Off(2f, 3f), 0.8f, 1.15f);
                        break;
                    case DorestadLandCover.LeveeWood:
                        if (R() < wd * 0.7f + 0.04f)
                            PutLater(R() < 0.3f ? Poplar : R() < 0.5f ? Oak : Ash, "Levee woods: black poplar, ash, elm and oak", p, R() < 0.3f ? 0.75f : 0.95f, 1.3f);
                        break;
                    case DorestadLandCover.Ridge:
                        if (R() < wd * 0.75f + 0.03f)
                            PutLater(R() < 0.65f ? Oak : Ash, "The old ridges: oak and hazel", p, 0.9f, 1.3f);
                        else if (wd < 0.3f && R() < 0.012f)
                            PutLater(Oak, "Lone standards", p, 1.1f, 1.4f);
                        break;
                    case DorestadLandCover.Peat:
                        if (R() < wd * 0.45f + 0.03f)
                            PutLater(Ash, "Birch on the peat", p, 0.75f, 1.0f, 3);
                        break;
                    case DorestadLandCover.Reed:
                        if (R() < 0.04f)
                            PutLater(Osier, "Scrub in the marsh", p, 0.7f, 1.0f);
                        break;
                    default:
                    {
                        // open land: copses from clustered noise, lone standards, scrub clumps in the basins; the levee
                        // round the town was cleared (the far bank was handled above, so this is the town's side)
                        float copse = Mathf.PerlinNoise(p.x / 130f + 21f, p.y / 130f + 8f) + 0.3f * Mathf.PerlinNoise(p.x / 30f, p.y / 30f);
                        float nearTown = Mathf.Clamp01(dTown / 600f);
                        if (wd > 0.35f && R() < wd * 0.6f * nearTown)
                            PutLater(R() < 0.6f ? Ash : Oak, "Copses", p, 0.9f, 1.25f);
                        else if (copse > 0.9f && R() < 0.6f * nearTown)
                            PutLater(R() < 0.6f ? Ash : Oak, "Copses", p, 0.9f, 1.25f);
                        else if (cv == DorestadLandCover.Basin && h < 0.5f && R() < 0.05f)
                            PutLater(Osier, "Scrub in the basins", p, 0.7f, 1.0f);
                        else if (R() < 0.004f)
                            PutLater(Oak, "Lone standards", p, 1.15f, 1.45f);
                        break;
                    }
                }
            }
            int near = trees.Count;
            // the trees the town planned itself: the farms' apple trees, the fort's oaks
            if (plan.trees != null)
                foreach (var pt in plan.trees)
                    Put(pt.kind, "Planned trees", pt.p, pt.scale, pt.scale, pt.tint);
            foreach (var q in later)
            {
                if (trees.Count >= MaxTrees)
                    break;
                Put(q.kind, q.group, q.p, q.lo, q.hi, q.tint);
            }
            td.SetTreeInstances(trees.ToArray(), true);
            terrain.treeDistance = 3000f;
            terrain.treeBillboardDistance = Kapman.Levels.LevelPerformance.TreeBillboardFrom;
            terrain.treeCrossFadeLength = 20f;
            terrain.treeMaximumFullLODCount = Kapman.Levels.LevelPerformance.TreesAtFullDetail;
            terrain.treeLODBiasMultiplier = 1.5f;
            EditorUtility.SetDirty(td);
            string thinned = later.Count > trees.Count - near ? $" (the woods thinned to {100f * (trees.Count - near) / later.Count:0}% to fit the cap of {MaxTrees:n0})" : "";
            Debug.Log($"Kapman planting: {trees.Count:n0} trees on the terrain{thinned}: " + string.Join(", ", counts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value:n0}")));
        }

        static float Cutoff(Material m) => m.HasProperty("_AlphaCutoff") ? m.GetFloat("_AlphaCutoff") : m.HasProperty("_Cutoff") ? m.GetFloat("_Cutoff") : 0.45f;

        /// <summary>
        /// One tinted variant of a kind as a prefab (rebuilt each time: cheap, and it takes a new impostor or scale).
        /// A tree keeps the scan's LOD group with its leaves swapped for the tint, and its last level swapped for the
        /// impostor; a shrub gets a group of its own: the scan, then the impostor.
        /// </summary>
        static GameObject Variant(GameObject basePrefab, bool shrub, Material baseLeaves, Material leaves, Mesh impostor, Material impostorMat, string path)
        {
            AssetDatabase.DeleteAsset(path);
            GameObject root;
            if (shrub)
            {
                root = new GameObject(Path.GetFileNameWithoutExtension(path));
                var srcMf = basePrefab.GetComponentInChildren<MeshFilter>();
                var lod0 = new GameObject("LOD0", typeof(MeshFilter), typeof(MeshRenderer));
                lod0.transform.SetParent(root.transform, false);
                lod0.GetComponent<MeshFilter>().sharedMesh = srcMf.sharedMesh;
                var r0 = lod0.GetComponent<MeshRenderer>();
                r0.sharedMaterial = leaves != null ? leaves : baseLeaves;
                r0.shadowCastingMode = ShadowCastingMode.On;
                var lods = new List<LOD> { new LOD(0.06f, new Renderer[] { r0 }) };
                if (impostor != null)
                    lods.Add(new LOD(0.004f, new Renderer[] { ImpostorRenderer(root.transform, impostor, impostorMat) }));
                var group = root.AddComponent<LODGroup>();
                group.SetLODs(lods.ToArray());
                group.RecalculateBounds();
            }
            else
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                root.name = Path.GetFileNameWithoutExtension(path);
                if (leaves != null && baseLeaves != null)
                    foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
                    {
                        var ms = r.sharedMaterials;
                        bool any = false;
                        for (int k = 0; k < ms.Length; k++)
                            if (ms[k] == baseLeaves)
                            {
                                ms[k] = leaves;
                                any = true;
                            }
                        if (any)
                            r.sharedMaterials = ms;
                    }
                var group = root.GetComponent<LODGroup>();
                if (impostor != null && group != null)
                {
                    var lods = group.GetLODs();
                    if (lods.Length >= 2)
                    {
                        var last = lods[lods.Length - 1];
                        foreach (var r in last.renderers)
                            if (r != null)
                                Object.DestroyImmediate(r.gameObject);
                        last.renderers = new Renderer[] { ImpostorRenderer(root.transform, impostor, impostorMat) };
                        last.screenRelativeTransitionHeight = 0.004f;      // kept to about 3 km, as the whole level shows from the air
                        lods[lods.Length - 1] = last;
                        lods[0].screenRelativeTransitionHeight = Mathf.Max(lods[0].screenRelativeTransitionHeight, 0.1f);
                        if (lods.Length >= 3)
                            lods[1].screenRelativeTransitionHeight = Mathf.Max(lods[1].screenRelativeTransitionHeight, 0.03f);
                        group.SetLODs(lods);
                        group.RecalculateBounds();
                    }
                }
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>The impostor atlas is baked unlit, so its colours are the scans' own pale albedo: the far trees take a darker tint.</summary>
        static Color Dim(Color c) => new Color(c.r * 0.68f, c.g * 0.72f, c.b * 0.62f, 1f);

        static MeshRenderer ImpostorRenderer(Transform parent, Mesh mesh, Material mat)
        {
            var go = new GameObject("Impostor", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return r;
        }

        // ------------------------------------------------------------------ the far level baked from the scan

        const int Tile = 512, Cols = 4, Views = 9;        // 8 yaws and the view from above, in a 2048 atlas of 16 tiles
        static readonly Color BakeGrey = new Color(0.5f, 0.5f, 0.5f, 0f);

        /// <summary>
        /// A far level baked from the scan itself: the prefab's most detailed level rendered unlit (no exposure, no
        /// post-processing, no fog) from 8 yaws and from above into one atlas, then shown on four one-sided quads
        /// (north, east, south, west, each its own view) and one lying quad (the view from above, for the shots from
        /// the air). The atlas alpha is 1 where wood shows and 0.7 where only leaves do, so the weather's winter cutoff
        /// bares the far trees as well. Not hand-made foliage: every pixel is the scan's. False if the bake came out
        /// empty (then the thinned scan stays as the far level).
        /// </summary>
        static bool Impostor(GameObject basePrefab, Material leaves, string slug, out Mesh quads, out Texture2D atlas)
        {
            quads = null;
            atlas = null;
            Renderer src = null;
            var group = basePrefab.GetComponent<LODGroup>();
            if (group != null)
            {
                var lods = group.GetLODs();
                if (lods.Length > 0 && lods[0].renderers.Length > 0)
                    src = lods[0].renderers[0];
            }
            if (src == null)
                src = basePrefab.GetComponentInChildren<MeshRenderer>();
            var mf = src != null ? src.GetComponent<MeshFilter>() : null;
            if (mf == null || mf.sharedMesh == null)
                return false;
            var mesh = mf.sharedMesh;
            var mats = src.sharedMaterials;
            var b = mesh.bounds;
            float half = Mathf.Max(b.extents.y, Mathf.Max(b.extents.x, b.extents.z)) * 1.12f;
            Vector3 centre = b.center;
            string pngPath = $"{ImpostorDir}/{slug}_impostor.png", meshPath = $"{ImpostorDir}/{slug}_impostor.asset";

            var rig = new GameObject("Impostor bake rig");
            var temp = new List<Object>();
            var atlasPx = new Color32[Cols * Tile * Cols * Tile];
            bool any = false;
            try
            {
                rig.transform.position = new Vector3(-4000f, 3000f, -4000f);     // well outside the level
                var tree = new GameObject("Tree", typeof(MeshFilter), typeof(MeshRenderer));
                tree.transform.SetParent(rig.transform, false);
                tree.GetComponent<MeshFilter>().sharedMesh = mesh;
                var tr = tree.GetComponent<MeshRenderer>();
                tr.shadowCastingMode = ShadowCastingMode.Off;
                // three sets of unlit materials: the colours, a flat magenta for what is covered, and the same with the
                // leaves hidden for what is wood
                var colourMats = new Material[mats.Length];
                var maskMats = new Material[mats.Length];
                var woodMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    colourMats[i] = BakeMaterial(mats[i], Color.white, null);
                    maskMats[i] = BakeMaterial(mats[i], new Color(8f, 0f, 8f, 1f), null);
                    woodMats[i] = mats[i] == leaves ? BakeMaterial(mats[i], new Color(1f, 1f, 1f, 0f), 0.5f) : maskMats[i];
                    temp.Add(colourMats[i]);
                    temp.Add(maskMats[i]);
                    if (woodMats[i] != maskMats[i])
                        temp.Add(woodMats[i]);
                }
                // a grey patch in the frame's corner, to check the render came through at face value
                var patch = new GameObject("Grey patch", typeof(MeshFilter), typeof(MeshRenderer));
                var patchMesh = new Mesh { name = "patch" };
                patchMesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) };
                patchMesh.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 };
                patchMesh.RecalculateNormals();
                patch.GetComponent<MeshFilter>().sharedMesh = patchMesh;
                var patchMat = BakeMaterial(null, new Color(0.5f, 0.5f, 0.5f, 1f), null);
                patch.GetComponent<MeshRenderer>().sharedMaterial = patchMat;
                temp.Add(patchMesh);
                temp.Add(patchMat);

                var camGo = new GameObject("Impostor camera", typeof(Camera));
                camGo.transform.SetParent(rig.transform, false);
                var cam = camGo.GetComponent<Camera>();
                cam.enabled = false;
                cam.orthographic = true;
                cam.orthographicSize = half;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = half * 8f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = BakeGrey;
                cam.allowHDR = false;
                cam.allowMSAA = false;
                var hd = camGo.AddComponent<HDAdditionalCameraData>();
                hd.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                hd.backgroundColorHDR = BakeGrey;
                hd.volumeLayerMask = 0;
                hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                hd.customRenderingSettings = true;
                foreach (var f in new[] { FrameSettingsField.Postprocess, FrameSettingsField.ExposureControl, FrameSettingsField.AtmosphericScattering, FrameSettingsField.CustomPass, FrameSettingsField.ShadowMaps })
                {
                    hd.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)f] = true;
                    hd.renderingPathCustomFrameSettings.SetEnabled(f, false);
                }
                patch.transform.SetParent(camGo.transform, false);
                patch.transform.localPosition = new Vector3(-half * 0.92f, half * 0.92f, half * 2f);
                patch.transform.localScale = new Vector3(half * 0.1f, half * 0.1f, 1f);

                var rt = new RenderTexture(Tile, Tile, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
                rt.Create();
                var read = new Texture2D(Tile, Tile, TextureFormat.RGBA32, false);
                temp.Add(read);
                Color32[] Snap(Material[] set)
                {
                    tr.sharedMaterials = set;
                    var request = new RenderPipeline.StandardRequest { destination = rt };
                    if (RenderPipeline.SupportsRenderRequest(cam, request))
                        RenderPipeline.SubmitRenderRequest(cam, request);
                    else
                    {
                        cam.targetTexture = rt;
                        cam.Render();
                        cam.targetTexture = null;
                    }
                    var prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    read.ReadPixels(new Rect(0, 0, Tile, Tile), 0, 0);
                    read.Apply();
                    RenderTexture.active = prev;
                    return read.GetPixels32();
                }
                bool Magenta(Color32 c) => c.g < 40 && (c.r > 110 || c.b > 110);

                float factor = 1f;
                for (int v = 0; v < Views; v++)
                {
                    Vector3 dir, up;
                    if (v < 8)
                    {
                        float yaw = v * 45f * Mathf.Deg2Rad;
                        dir = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
                        up = Vector3.up;
                    }
                    else
                    {
                        dir = Vector3.up;
                        up = Vector3.forward;
                    }
                    camGo.transform.position = rig.transform.position + centre + dir * (half * 4f);
                    camGo.transform.rotation = Quaternion.LookRotation(-dir, up);
                    var colourPx = Snap(colourMats);
                    var maskPx = Snap(maskMats);
                    var woodPx = Snap(woodMats);
                    if (v == 0)
                    {
                        // the grey patch: a factor off 1 means the render came through exposed or graded
                        Color32 pc = colourPx[(Tile - 21) * Tile + 20];
                        float lin = Mathf.GammaToLinearSpace(pc.r / 255f);
                        factor = lin / 0.5f;
                        if (factor < 0.1f || factor > 10f || float.IsNaN(factor))
                        {
                            Debug.LogWarning($"Kapman planting: the impostor bake of {slug} read its grey patch at {pc.r} (expected 188): colours taken as they came.");
                            factor = 1f;
                        }
                    }
                    bool fix = Mathf.Abs(factor - 1f) > 0.03f;
                    var tilePx = new Color32[Tile * Tile];
                    int covered = 0;
                    for (int i = 0; i < tilePx.Length; i++)
                    {
                        if (!Magenta(maskPx[i]))
                            continue;
                        covered++;
                        Color32 c = colourPx[i];
                        if (fix)
                        {
                            Color lc = new Color(Mathf.GammaToLinearSpace(c.r / 255f) / factor, Mathf.GammaToLinearSpace(c.g / 255f) / factor, Mathf.GammaToLinearSpace(c.b / 255f) / factor);
                            c = new Color32((byte)Mathf.Clamp(Mathf.LinearToGammaSpace(lc.r) * 255f, 0f, 255f), (byte)Mathf.Clamp(Mathf.LinearToGammaSpace(lc.g) * 255f, 0f, 255f),
                                            (byte)Mathf.Clamp(Mathf.LinearToGammaSpace(lc.b) * 255f, 0f, 255f), 255);
                        }
                        tilePx[i] = new Color32(c.r, c.g, c.b, Magenta(woodPx[i]) ? (byte)255 : (byte)178);
                    }
                    if (covered > tilePx.Length / 500)
                        any = true;
                    // the empty pixels next to a covered one take its colour, so the cut-out's edge has no grey halo
                    var dil = (Color32[])tilePx.Clone();
                    for (int y = 0; y < Tile; y++)
                        for (int x = 0; x < Tile; x++)
                        {
                            int i = y * Tile + x;
                            if (tilePx[i].a != 0)
                                continue;
                            if (x > 0 && tilePx[i - 1].a != 0) dil[i] = new Color32(tilePx[i - 1].r, tilePx[i - 1].g, tilePx[i - 1].b, 0);
                            else if (x < Tile - 1 && tilePx[i + 1].a != 0) dil[i] = new Color32(tilePx[i + 1].r, tilePx[i + 1].g, tilePx[i + 1].b, 0);
                            else if (y > 0 && tilePx[i - Tile].a != 0) dil[i] = new Color32(tilePx[i - Tile].r, tilePx[i - Tile].g, tilePx[i - Tile].b, 0);
                            else if (y < Tile - 1 && tilePx[i + Tile].a != 0) dil[i] = new Color32(tilePx[i + Tile].r, tilePx[i + Tile].g, tilePx[i + Tile].b, 0);
                        }
                    int col = v % Cols, row = v / Cols;
                    for (int y = 0; y < Tile; y++)
                        System.Array.Copy(dil, y * Tile, atlasPx, (row * Tile + y) * (Cols * Tile) + col * Tile, Tile);
                }
                rt.Release();
                Object.DestroyImmediate(rt);
            }
            finally
            {
                Object.DestroyImmediate(rig);
                foreach (var o in temp)
                    if (o != null)
                        Object.DestroyImmediate(o);
            }
            if (!any)
            {
                Debug.LogWarning($"Kapman planting: the impostor bake of {slug} came out empty.");
                return false;
            }

            // the atlas as a texture in the project
            var tex = new Texture2D(Cols * Tile, Cols * Tile, TextureFormat.RGBA32, false);
            tex.SetPixels32(atlasPx);
            tex.Apply();
            File.WriteAllBytes(Full(pngPath), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = true;
            imp.maxTextureSize = Cols * Tile;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);

            // the quads: four standing (each facing its own view) and one lying under the view from above
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            void Quad(Vector3 c, Vector3 right, Vector3 up, float h, int view)
            {
                int col = view % Cols, row = view / Cols;
                float u0 = col / (float)Cols, v0 = row / (float)Cols, u1 = u0 + 1f / Cols, v1 = v0 + 1f / Cols;
                int i0 = verts.Count;
                verts.Add(c - right * h - up * h);
                verts.Add(c + right * h - up * h);
                verts.Add(c + right * h + up * h);
                verts.Add(c - right * h + up * h);
                uvs.Add(new Vector2(u0, v0));
                uvs.Add(new Vector2(u1, v0));
                uvs.Add(new Vector2(u1, v1));
                uvs.Add(new Vector2(u0, v1));
                for (int k = 0; k < 4; k++)
                    norms.Add(Vector3.up);       // lit like the ground around it, the same from every side
                tris.AddRange(new[] { i0, i0 + 3, i0 + 2, i0, i0 + 2, i0 + 1 });
            }
            for (int v = 0; v < 8; v += 2)
            {
                float yaw = v * 45f * Mathf.Deg2Rad;
                Vector3 d = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
                Quad(centre, -Vector3.Cross(Vector3.up, d), Vector3.up, half, v);
            }
            Quad(centre + Vector3.up * (half * 0.25f), Vector3.right, Vector3.forward, half, 8);
            quads = new Mesh { name = slug + "_impostor" };
            quads.SetVertices(verts);
            quads.SetNormals(norms);
            quads.SetUVs(0, uvs);
            quads.SetTriangles(tris, 0);
            quads.RecalculateBounds();
            AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(quads, meshPath);
            Debug.Log($"Kapman planting: impostor baked for {slug} ({half * 2f:0.0} m frame).");
            return true;
        }

        /// <summary>An HDRP/Unlit stand-in for a scan's material: its colour map, a flat colour, alpha clipped as the original was (or as asked).</summary>
        static Material BakeMaterial(Material from, Color flat, float? forceCutoff)
        {
            var u = new Material(Shader.Find("HDRP/Unlit"));
            Texture tex = null;
            if (from != null)
                tex = from.HasProperty("_BaseColorMap") ? from.GetTexture("_BaseColorMap") : from.HasProperty("_BaseMap") ? from.GetTexture("_BaseMap") : null;
            u.SetTexture("_UnlitColorMap", tex);
            u.SetColor("_UnlitColor", flat);
            bool clip = from != null && from.IsKeywordEnabled("_ALPHATEST_ON");
            float cutoff = clip ? Cutoff(from) : 0.5f;
            if (forceCutoff.HasValue)
            {
                clip = true;
                cutoff = forceCutoff.Value;
            }
            HDMaterial.SetAlphaClipping(u, clip);
            if (clip)
            {
                HDMaterial.SetAlphaCutoff(u, cutoff);
                u.EnableKeyword("_ALPHATEST_ON");
            }
            u.SetFloat("_DoubleSidedEnable", 1f);
            u.SetFloat("_CullMode", (float)CullMode.Off);
            HDMaterial.ValidateMaterial(u);
            return u;
        }

        // ------------------------------------------------------------------ the reeds

        /// <summary>
        /// A reed clump for the terrain's detail layer: 24 tapered blades 2.2-3 m tall, leaning outward from a 0.6 m
        /// stool, straw at the tips and olive at the base (a gradient texture made here: HDRP/Lit takes no vertex
        /// colours), on a plain two-sided lit material. Built in code against the rule of no home-made foliage, because
        /// no CC0 reed scan was found (3 October): swap it for a scan when one turns up. Returns the prefab and its
        /// material (the weather tints it: green through the summer, straw in the autumn).
        /// </summary>
        public static GameObject ReedClump(out Material material)
        {
            string matPath = NatureDir + "/reed_clump.mat", texPath = NatureDir + "/reed_blade.png", meshPath = NatureDir + "/reed_clump.asset";
            Directory.CreateDirectory(Full(NatureDir));
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ReedPrefabPath);
            material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null && material != null)
            {
                AssetLibrary.Upgrade(material);
                return existing;
            }
            if (!File.Exists(Full(texPath)))
            {
                // the blade's colours, base (v = 0) to tip (v = 1)
                Color baseC = new Color(0.45f, 0.5f, 0.28f), tip = new Color(0.72f, 0.66f, 0.42f);
                var tex = new Texture2D(8, 64, TextureFormat.RGBA32, false);
                var px = new Color[8 * 64];
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 8; x++)
                        px[y * 8 + x] = Color.Lerp(baseC, tip, Mathf.Pow(y / 63f, 1.4f));
                tex.SetPixels(px);
                tex.Apply();
                File.WriteAllBytes(Full(texPath), tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);
                var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.mipmapEnabled = true;
                imp.SaveAndReimport();
            }
            var blade = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (material == null)
            {
                material = AssetLibrary.NewLit(blade, null, 0f, Color.white);
                AssetLibrary.TwoSided(material);
                AssetDatabase.CreateAsset(material, matPath);
            }
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = ReedMesh(new System.Random(41), 24, 2.2f, 3.0f, 0.03f, 0.6f);
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            var go = new GameObject("reed_clump");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = ShadowCastingMode.Off;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ReedPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static Mesh ReedMesh(System.Random rnd, int blades, float hLo, float hHi, float width, float radius)
        {
            float R() => (float)rnd.NextDouble();
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tr = new List<int>();
            const int Seg = 4;
            for (int b = 0; b < blades; b++)
            {
                float a = R() * Mathf.PI * 2f, r = radius * Mathf.Sqrt(R());
                Vector3 root = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                float h = hLo + R() * (hHi - hLo);
                Vector3 lean = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (0.08f + R() * 0.22f);
                float yaw = R() * Mathf.PI * 2f;
                Vector3 side = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
                Vector3 nrm = (Vector3.Cross(side, Vector3.up) + Vector3.up * 1.5f).normalized;
                int i0 = v.Count;
                for (int s = 0; s <= Seg; s++)
                {
                    float f = s / (float)Seg;
                    Vector3 c = root + Vector3.up * (h * f) + lean * (h * f * f);    // bending outward toward the tip
                    float w = width * (1f - f * 0.9f) / 2f;
                    v.Add(c - side * w);
                    v.Add(c + side * w);
                    n.Add(nrm);
                    n.Add(nrm);
                    uv.Add(new Vector2(0f, f));
                    uv.Add(new Vector2(1f, f));
                }
                for (int s = 0; s < Seg; s++)
                {
                    int a0 = i0 + s * 2, a1 = a0 + 1, b0 = a0 + 2, b1 = a0 + 3;
                    tr.AddRange(new[] { a0, b0, b1, a0, b1, a1 });
                }
            }
            var m = new Mesh { name = "reed_clump" };
            m.SetVertices(v);
            m.SetNormals(n);
            m.SetUVs(0, uv);
            m.SetTriangles(tr, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>
        /// The reed beds, laid after DorestadLandCover.Details(): its stretched-grass 'reeds' prototype is replaced by
        /// the reed clump and its layer rewritten - slack flat shores and the shallows (never a cut bank), densest on
        /// the inside bends, the shoal's outer edge and the fork's bar (the mud and sand cover is the cue), the pools
        /// and a 6 m ring round them, and the reed marsh. It also darkens the grass prototypes if they still carry
        /// their first pale colours, cuts the crest's grass density if it is still clumps, gives the pools' rims sedge,
        /// and pushes the detail distance out to 260 m.
        /// </summary>
        public static void Reeds(Terrain terrain, DorestadBuilder.Grid g, byte[] cover, DorestadTown.Plan plan)
        {
            var td = terrain.terrainData;
            var clump = ReedClump(out _);
            var protos = td.detailPrototypes.ToList();
            int ri = -1;
            float tallest = 2f;
            for (int i = 0; i < protos.Count; i++)
                if (protos[i].maxHeight > tallest)
                {
                    tallest = protos[i].maxHeight;
                    ri = i;
                }
            var reeds = new DetailPrototype
            {
                prototype = clump, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minWidth = 0.8f, maxWidth = 1.2f, minHeight = 0.85f, maxHeight = 1.15f, noiseSpread = 0.2f,
                healthyColor = Color.white, dryColor = new Color(0.92f, 0.88f, 0.72f),
            };
            if (ri >= 0)
                protos[ri] = reeds;
            else
            {
                ri = protos.Count;
                protos.Add(reeds);
            }
            // the grass prototypes, darkened to sit under the ground's colour if they still carry their first colours
            bool Same(Color a, Color b) => Mathf.Abs(a.r - b.r) < 0.02f && Mathf.Abs(a.g - b.g) < 0.02f && Mathf.Abs(a.b - b.b) < 0.02f;
            int meadow = -1, sedge = -1;
            for (int i = 0; i < protos.Count; i++)
            {
                if (i == ri)
                    continue;
                var p = protos[i];
                if (Same(p.healthyColor, new Color(0.62f, 0.72f, 0.38f)))
                {
                    p.healthyColor = new Color(0.40f, 0.50f, 0.24f);
                    p.dryColor = new Color(0.52f, 0.48f, 0.28f);
                    p.noiseSpread = 0.12f;
                    meadow = i;
                }
                else if (Same(p.healthyColor, new Color(0.40f, 0.50f, 0.24f)))
                    meadow = i;
                else if (Same(p.healthyColor, new Color(0.58f, 0.66f, 0.34f)))
                {
                    p.healthyColor = new Color(0.38f, 0.46f, 0.22f);
                    p.dryColor = new Color(0.56f, 0.50f, 0.30f);
                }
                else if (Same(p.healthyColor, new Color(0.46f, 0.58f, 0.30f)))
                {
                    p.healthyColor = new Color(0.30f, 0.42f, 0.20f);
                    p.dryColor = new Color(0.36f, 0.44f, 0.24f);
                    sedge = i;
                }
                else if (Same(p.healthyColor, new Color(0.30f, 0.42f, 0.20f)))
                    sedge = i;
                else if (Same(p.healthyColor, new Color(0.30f, 0.44f, 0.22f)))
                {
                    p.healthyColor = new Color(0.22f, 0.34f, 0.16f);
                    p.dryColor = new Color(0.26f, 0.34f, 0.18f);
                }
                protos[i] = p;
                // the clump's material: toned down and matt, if it is still as the scan came
                var mr = p.prototype != null ? p.prototype.GetComponentInChildren<MeshRenderer>() : null;
                var m = mr != null ? mr.sharedMaterial : null;
                if (m != null && m.HasProperty("_BaseColor") && Same(m.GetColor("_BaseColor"), Color.white))
                {
                    m.SetColor("_BaseColor", new Color(0.75f, 0.8f, 0.65f));
                    if (m.HasProperty("_Smoothness"))
                        m.SetFloat("_Smoothness", 0f);
                    EditorUtility.SetDirty(m);
                }
            }
            if (meadow < 0)
            {
                // the meadow grass recoloured by someone else already: it is the shortest prototype
                float low = 99f;
                for (int i = 0; i < protos.Count; i++)
                    if (i != ri && protos[i].maxHeight < low)
                    {
                        low = protos[i].maxHeight;
                        meadow = i;
                    }
            }
            if (meadow >= 0)
                protos[meadow].noiseSpread = 0.12f;       // a sward, not clumps 2 m apart
            td.detailPrototypes = protos.ToArray();

            int res = td.detailResolution;
            float cell = g.size / res;
            byte Cover(Vector2 p) => cover != null ? DorestadLandCover.Sample(cover, g, p) : DorestadLandCover.Crest;
            var ring = new[]
            {
                new Vector2(6f, 0f), new Vector2(-6f, 0f), new Vector2(0f, 6f), new Vector2(0f, -6f),
                new Vector2(4.2f, 4.2f), new Vector2(-4.2f, 4.2f), new Vector2(4.2f, -4.2f), new Vector2(-4.2f, -4.2f),
            };
            var map = new int[res, res];
            int[,] sedgeMap = sedge >= 0 ? td.GetDetailLayer(0, 0, res, res, sedge) : null;
            int[,] meadowMap = meadow >= 0 ? td.GetDetailLayer(0, 0, res, res, meadow) : null;
            int meadowMax = 0;
            if (meadowMap != null)
                foreach (int v in meadowMap)
                    meadowMax = Mathf.Max(meadowMax, v);
            bool cutMeadow = meadowMax > 12;       // still the first density (clumps 2 m apart): thinned to a sward
            int reedCells = 0;
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    var p = new Vector2((x + 0.5f) * cell, (z + 0.5f) * cell);
                    byte cv = Cover(p);
                    if (cutMeadow && (cv == DorestadLandCover.Crest || cv == DorestadLandCover.Ridge))
                        meadowMap[z, x] = Mathf.RoundToInt(meadowMap[z, x] * 0.62f);
                    byte use = plan != null ? plan.Get(p) : (byte)0;
                    if (use == (byte)DorestadTown.Use.Lane || use == (byte)DorestadTown.Use.Track || use == (byte)DorestadTown.Use.Shoal || use == (byte)DorestadTown.Use.Foreshore)
                        continue;
                    float sd = g.SD(p), h = g.H(p) - g.wl;
                    float slope = Slope(g, p);
                    int v = 0;
                    // the reed marsh: thin stands with open water and sedge between
                    if (cv == DorestadLandCover.Reed)
                    {
                        float n = Mathf.PerlinNoise(p.x / 28f, p.y / 28f);
                        v = n > 0.3f ? 4 + (int)(4f * n) : 2;
                    }
                    // the pools and a ring round them: reeds, with sedge on the rim
                    bool pool = cv == DorestadLandCover.Pool;
                    for (int k = 0; k < ring.Length && !pool; k++)
                        pool = Cover(p + ring[k]) == DorestadLandCover.Pool;
                    if (pool)
                    {
                        v = Mathf.Max(v, 10 + (int)(6f * Mathf.PerlinNoise(p.x / 15f + 9f, p.y / 15f + 4f)));
                        if (sedgeMap != null && cv != DorestadLandCover.Pool)
                            sedgeMap[z, x] = Mathf.Max(sedgeMap[z, x], 8);
                    }
                    // the shores: slack flat ground within 20 m of the water and the shallows up to 2.5 m out (6 m
                    // where the bed stays within 0.6 m of the surface), never the steep cut banks
                    bool shore = sd > -20f && sd <= 2.5f && slope < 0.14f && !(cv == DorestadLandCover.Mud && slope > 0.10f);
                    if (!shore && sd > 2.5f && sd <= 6f && h > -0.6f && slope < 0.14f)
                        shore = true;
                    if (shore)
                    {
                        float dense = Mathf.PerlinNoise(p.x / 28f + 5f, p.y / 28f + 2f);
                        bool slack = (cv == DorestadLandCover.Mud || cv == DorestadLandCover.Bar) && slope < 0.1f;
                        if (!slack)
                            for (int k = 0; k < ring.Length && !slack; k++)
                            {
                                byte c2 = Cover(p + ring[k] * 2f);
                                slack = c2 == DorestadLandCover.Bar;
                            }
                        if (dense > 0.3f)
                            v = Mathf.Max(v, slack ? 10 + (int)(8f * dense) : 6 + (int)(6f * dense));
                    }
                    if (slope >= 0.14f || sd > 6f || (sd > 0f && h < -0.6f))
                        v = 0;
                    v = Mathf.Min(v, 16);
                    map[z, x] = v;
                    if (v > 0)
                        reedCells++;
                }
            td.SetDetailLayer(0, 0, ri, map);
            if (sedgeMap != null)
                td.SetDetailLayer(0, 0, sedge, sedgeMap);
            if (cutMeadow)
                td.SetDetailLayer(0, 0, meadow, meadowMap);
            terrain.detailObjectDistance = Kapman.Levels.LevelPerformance.GrassDistance;   // (was 260: the fps, 4 Oct)
            EditorUtility.SetDirty(td);
            Debug.Log($"Kapman reeds: {reedCells:n0} detail cells of reeds (layer {ri}), the grass {(cutMeadow ? "thinned on the crest" : "left as it was")}, details to 260 m.");
        }

        static float Slope(DorestadBuilder.Grid g, Vector2 p)
        {
            float d = 5f;
            float dx = g.H(p + new Vector2(d, 0f)) - g.H(p - new Vector2(d, 0f));
            float dz = g.H(p + new Vector2(0f, d)) - g.H(p - new Vector2(0f, d));
            return Mathf.Sqrt(dx * dx + dz * dz) / (2f * d);
        }
    }
}
