using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE MEADOW MIX - a hay meadow or a grazed pasture is a mosaic, not one grass: several grasses in patches, the
    /// flowering grass in drifts, the flowers scattered where each grows. This takes a terrain whose details already say
    /// HOW MUCH grows in each cell (the land cover's sward, hay, reeds, sedge, nettles, clover, daisies, dead grass,
    /// knapweed, cornflower) and splits it among the meadow pack's grasses by slow noise, so each grass holds its own
    /// patches tens of metres across, and adds the other flowers where their companions are:
    ///   chicory with the dead grass of the dry crest; St John's wort with the knapweed in the hay; chamomile with the
    ///   clover and daisies of the grazed grass; poppies with the cornflowers in the corn; soapwort (bouncing bet) on the
    ///   banks beside the sedge. Goldenrod and "sunroot" stay out (the pack's goldenrod is the American kind; sunroot is
    ///   American).
    /// The plant counts in each cell are kept: they are shared out, not added to. Run by the builder after the details and
    /// the reeds; Kapman > Levels > Mix Dorestad's meadow runs it on the saved terrain (no rebuild).
    /// </summary>
    public static class MeadowMix
    {
        const string G = "Grass/Prefabs Unity Terrain Grass/prefab_Terrain_grass_";
        const string F = "Grass/Prefabs Unity Terrain Flowers/prefab_Terrain_flower_";
        // the low sward (meadow_02 and meadow_03 are short grasses), the flowering grass (meadow_01), all crossed cards
        static readonly string[] Sward = { G + "meadow_02_cross_1.prefab", G + "meadow_02_cross_2.prefab", G + "meadow_02_cross_3.prefab", G + "meadow_03_1.prefab", G + "meadow_03_3.prefab" };
        static readonly string[] Hay = { G + "meadow_01_cross_1.prefab", G + "meadow_01_cross_2.prefab", G + "meadow_01_cross_3.prefab" };
        const string Chicory = F + "common_chicory_01_1.prefab", StJohns = F + "common_Saint_John's_wort_01_1.prefab",
                     Chamomile = F + "chamomile_01_1.prefab", Poppy = F + "common_poppy_01_1.prefab", Soapwort = F + "bouncing_bet_01_1.prefab",
                     Knapweed2 = F + "brownray_knapweed_01_2.prefab", Cornflower2 = F + "cornflower_01_2.prefab";

        static string PathOf(DetailPrototype p) => p.prototype != null ? AssetDatabase.GetAssetPath(p.prototype) : "";

        static int Find(List<DetailPrototype> protos, string relative)
        {
            string full = MeadowPack.Root + relative;
            for (int i = 0; i < protos.Count; i++)
                if (PathOf(protos[i]) == full)
                    return i;
            return -1;
        }

        static int FindContaining(List<DetailPrototype> protos, string part)
        {
            for (int i = 0; i < protos.Count; i++)
                if (PathOf(protos[i]).Contains(part))
                    return i;
            return -1;
        }

        /// <summary>The layer for a pack prefab, added (as a prototype and an empty layer) if the terrain lacks it.</summary>
        static int Ensure(List<DetailPrototype> protos, List<int[,]> maps, int w, int h, string relative, float lo, float hi, float wideLo = -1f, float wideHi = -1f)
        {
            int i = Find(protos, relative);
            if (i >= 0)
                return i;
            var prefab = MeadowPack.Prefab(relative);
            if (prefab == null)
                return -1;
            protos.Add(new DetailPrototype
            {
                prototype = prefab, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minHeight = lo, maxHeight = hi, minWidth = wideLo > 0f ? wideLo : lo, maxWidth = wideHi > 0f ? wideHi : hi,
                noiseSpread = 0.3f, healthyColor = Color.white, dryColor = Color.white,
            });
            maps.Add(new int[h, w]);
            return protos.Count - 1;
        }

        /// <summary>Slow noise in 0..1 for a mosaic of patches tens of metres across (cell coordinates, its own seed).</summary>
        static float Patch(int x, int z, float seed, float cells) => Mathf.PerlinNoise(x / cells + seed * 13.1f, z / cells + seed * 7.7f);

        public static void Mix(TerrainData td)
        {
            if (td == null || !MeadowPack.Installed)
                return;
            int w = td.detailWidth, h = td.detailHeight;
            var protos = new List<DetailPrototype>(td.detailPrototypes);
            var maps = new List<int[,]>();
            for (int i = 0; i < protos.Count; i++)
                maps.Add(td.GetDetailLayer(0, 0, w, h, i));

            // where the counts are now: the land cover wrote the sward on the first meadow grass and the hay on the first
            // flowering one (whatever cards they are), the flowers on their own layers
            int sward0 = FindContaining(protos, "grass_meadow_02"), hay0 = FindContaining(protos, "grass_meadow_01");
            if (sward0 < 0 || hay0 < 0)
            {
                Debug.LogWarning("Kapman meadow: the terrain has no meadow pack grass to share out; nothing mixed.");
                return;
            }
            int clover = FindContaining(protos, "meadow_clover"), daisy = FindContaining(protos, "meadow_daisy"), dead = FindContaining(protos, "dead_grass"),
                knapweed = FindContaining(protos, "knapweed"), cornflower = FindContaining(protos, "cornflower"),
                sedge = FindContaining(protos, "Kapman/Prefabs/Nature/grass_medium_02");

            // the layers to share into: the sward's five, the hay's three, the flowers (each made if missing)
            float sLo = protos[sward0].minHeight, sHi = protos[sward0].maxHeight, sWLo = protos[sward0].minWidth, sWHi = protos[sward0].maxWidth;
            float hLo = protos[hay0].minHeight, hHi = protos[hay0].maxHeight;
            var swardLayers = new List<int>();
            foreach (var s in Sward)
            {
                int i = Ensure(protos, maps, w, h, s, sLo, sHi, sWLo, sWHi);
                if (i >= 0) swardLayers.Add(i);
            }
            var hayLayers = new List<int>();
            foreach (var s in Hay)
            {
                int i = Ensure(protos, maps, w, h, s, hLo, hHi);
                if (i >= 0) hayLayers.Add(i);
            }
            int chicory = Ensure(protos, maps, w, h, Chicory, 0.8f, 1.1f), stJohns = Ensure(protos, maps, w, h, StJohns, 0.8f, 1.1f),
                chamomile = Ensure(protos, maps, w, h, Chamomile, 0.8f, 1.1f), poppy = Ensure(protos, maps, w, h, Poppy, 0.8f, 1.1f),
                soapwort = Ensure(protos, maps, w, h, Soapwort, 0.8f, 1.1f), knapweed2 = Ensure(protos, maps, w, h, Knapweed2, 0.8f, 1.1f),
                cornflower2 = Ensure(protos, maps, w, h, Cornflower2, 0.8f, 1.1f);

            void Share(int from, List<int> into, int cell0, int z, int x)
            {
                int total = maps[from][z, x];
                if (total <= 0 || into.Count == 0)
                    return;
                // weights from the slow noise, sharpened so each grass holds its own patches
                float sum = 0f;
                var wts = new float[into.Count];
                for (int k = 0; k < into.Count; k++)
                {
                    float n = Patch(x, z, cell0 + k * 3.7f, 22f) * 0.75f + Patch(x, z, cell0 + k * 5.3f + 40f, 6f) * 0.25f;
                    wts[k] = Mathf.Pow(n, 4f);
                    sum += wts[k];
                }
                int left = total;
                for (int k = 0; k < into.Count; k++)
                {
                    int part = k == into.Count - 1 ? left : Mathf.Min(left, Mathf.RoundToInt(total * wts[k] / Mathf.Max(1e-6f, sum)));
                    maps[into[k]][z, x] = (into[k] == from ? 0 : maps[into[k]][z, x]) + part;
                    left -= part;
                }
            }

            int Get(int layer, int z, int x) => layer >= 0 ? maps[layer][z, x] : 0;
            void Add(int layer, int z, int x, int n) { if (layer >= 0) maps[layer][z, x] = Mathf.Max(maps[layer][z, x], n); }

            for (int z = 0; z < h; z++)
                for (int x = 0; x < w; x++)
                {
                    Share(sward0, swardLayers, 1, z, x);
                    Share(hay0, hayLayers, 11, z, x);
                    float a = Patch(x, z, 21f, 9f), b = Patch(x, z, 22f, 13f), c = Patch(x, z, 23f, 7f);
                    if (Get(dead, z, x) > 0 && a > 0.62f) Add(chicory, z, x, 1);                    // chicory on the dry crest
                    if (Get(knapweed, z, x) > 0)
                    {
                        if (b > 0.6f) Add(stJohns, z, x, 1);                                       // St John's wort in the hay
                        if (c > 0.5f) { Add(knapweed2, z, x, Get(knapweed, z, x)); maps[knapweed][z, x] = 0; }
                    }
                    if ((Get(clover, z, x) > 0 || Get(daisy, z, x) > 0) && c > 0.64f) Add(chamomile, z, x, 2);   // chamomile in the grazing
                    if (Get(cornflower, z, x) > 0)
                    {
                        if (a > 0.55f) Add(poppy, z, x, 2);                                        // poppies in the corn
                        if (b > 0.5f) { Add(cornflower2, z, x, Get(cornflower, z, x)); maps[cornflower][z, x] = 0; }
                    }
                    if (Get(sedge, z, x) > 0 && b > 0.7f) Add(soapwort, z, x, 1);                   // soapwort on the banks
                }

            td.detailPrototypes = protos.ToArray();
            for (int i = 0; i < maps.Count; i++)
                td.SetDetailLayer(0, 0, i, maps[i]);
            EditorUtility.SetDirty(td);
            Debug.Log($"Kapman meadow: the sward shared among {swardLayers.Count} grasses, the hay among {hayLayers.Count}, flowers added; {protos.Count} detail layers.");
        }

        [MenuItem("Kapman/Levels/Mix Dorestad's meadow")]
        public static void MixDorestad()
        {
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(DorestadBuilder.LevelDir + "/Dorestad_TerrainData.asset");
            Mix(td);
            AssetDatabase.SaveAssets();
            DetailCheck.Run();
        }
    }
}
