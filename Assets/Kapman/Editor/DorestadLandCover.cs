using System.IO;
using Kapman.Game;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE LAND COVER c. 800, PAINTED FROM THE REAL DATA - the terrain's layers come from cover.u8 and wood.u8
    /// (Bake/levels/make_dorestad.py: the levees, basins, carr, peat, ridges, mud, bars and pools from the lidar, Cohen's
    /// channel belts and the Vos map), and from what the town made of the land (plan.use: yards, lanes, tracks, fields,
    /// orchards, the foreshore, the market, the cemeteries). Nothing is one flat colour: four scales of noise break every
    /// class into its neighbours and swing the levee from hay to grazed pasture across its width; dry ground comes as
    /// long yellowed stretches on the crest, not speckle; the mud shore is wide on the point bars, a foot on the cut
    /// banks and broken by grass between (DorestadRiver's bend-side map); fields are strips in three states with
    /// headlands; lanes and tracks have soft trodden verges; the thirteen layers are each pulled to their own hue and
    /// value so levee, basin, marsh and fallow read apart from a kilometre up.
    /// </summary>
    public static class DorestadLandCover
    {
        public const byte Water = 0, Crest = 1, Flank = 2, Basin = 3, Reed = 4, Carr = 5, Peat = 6, Ridge = 7, Mud = 8, Bar = 9, Pool = 10, LeveeWood = 11;

        // the terrain layers, in order
        public const int LHay = 0, LWet = 1, LPasture = 2, LPlough = 3, LStubble = 4, LMud = 5, LTrack = 6, LCarrFloor = 7,
                         LMarsh = 8, LSand = 9, LPeat = 10, LLeaves = 11, LDryGrass = 12;

        // a land-use code for a lane's or track's verge, should DorestadTown paint one (Use.Verge = 11); the painter
        // also finds verges on its own, from the texels next to a lane or track
        public const byte UseVerge = 11;

        public static byte[] LoadCover() => Load("cover.u8");
        public static byte[] LoadWood() => Load("wood.u8");

        static byte[] Load(string name)
        {
            string path = Path.Combine(DorestadBuilder.DataDir, name);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Kapman land cover: no " + path + " (run Bake/levels/make_dorestad.py); painting from heights alone.");
                return null;
            }
            return File.ReadAllBytes(path);
        }

        // the bake's crest/flank/marsh thresholds (m over the water), near the rivers and on the far land: the cover
        // classes are ranks of the land's height, so wetness is judged against these, not against fixed metres
        static float[] nearT = { 1.6f, 1.0f, 0.45f }, farT = { 1.6f, 1.0f, 0.45f };

        static void LoadThresholds()
        {
            string path = Path.Combine(DorestadBuilder.DataDir, "meta.json");
            if (!File.Exists(path))
                return;
            var th = Json.Obj(Json.Parse(File.ReadAllText(path)), "cover_thresholds");
            if (th == null)
                return;
            var near = Json.Arr(th, "near");
            var far = Json.Arr(th, "far");
            if (near != null && near.Count >= 3)
                for (int k = 0; k < 3; k++)
                    nearT[k] = System.Convert.ToSingle(near[k]);
            if (far != null && far.Count >= 3)
                for (int k = 0; k < 3; k++)
                    farT[k] = System.Convert.ToSingle(far[k]);
        }

        /// <summary>How wet the ground is by its rank: 0 at the flank line and above, 1 well below the marsh line.</summary>
        static float Wetness(float h, float sd)
        {
            float blend = Mathf.Clamp01((-sd - 600f) / 300f);
            float flank = Mathf.Lerp(nearT[1], farT[1], blend), marsh = Mathf.Lerp(nearT[2], farT[2], blend);
            return Mathf.Clamp01(Mathf.InverseLerp(flank, marsh - 0.3f, h));
        }

        public static TerrainLayer[] Layers()
        {
            string L = DorestadBuilder.LevelDir + "/Ground";
            var layers = new[]
            {
                AssetLibrary.Layer("ambientcg", "Grass001", 3f, L + "/0_hay_meadow.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Grass002", 3f, L + "/1_wet_meadow.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Grass004", 3f, L + "/2_pasture.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "farm_furrows", 6f, L + "/3_ploughed.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "farm_soil", 5f, L + "/4_stubble.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "brown_mud_02", 3f, L + "/5_mud.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "muddy_tracks", 4f, L + "/6_track.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "mud_forest", 4f, L + "/7_carr_floor.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "aerial_mud_1", 5f, L + "/8_marsh.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "sand_01", 4f, L + "/9_sand.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Moss002", 3f, L + "/10_peat.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "forest_leaves_02", 4f, L + "/11_leaf_floor.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Grass008", 3f, L + "/12_dry_grass.terrainlayer"),
            };
            // the colour remaps (the terrain shader and the basemap both apply them, so they hold at any distance):
            // the grasses are near-identical mid greens and Grass008 the brightest of them (twice the hay's green),
            // which made white blotches wherever it was mixed in; the dry grass is pulled down into the hay's range
            // and tawny, the wet meadow to a dark olive, the pasture a fresh green, the marsh an olive apart from the
            // mud, the peat and the carr floor dark, the stubble and sand pale, and the red taken out of the tracks and
            // the furrows. No new textures.
            Color[] remap =
            {
                new Color(0.78f, 0.80f, 0.55f, 1f),   // hay meadow
                new Color(0.62f, 0.74f, 0.52f, 1f),   // wet meadow
                new Color(0.72f, 0.84f, 0.55f, 1f),   // pasture
                new Color(0.80f, 0.72f, 0.62f, 1f),   // ploughed
                new Color(0.95f, 0.88f, 0.70f, 1f),   // stubble
                new Color(1f, 1f, 1f, 1f),            // mud
                new Color(0.74f, 0.68f, 0.58f, 1f),   // track
                new Color(0.55f, 0.50f, 0.40f, 1f),   // carr floor
                new Color(0.62f, 0.64f, 0.46f, 1f),   // marsh
                new Color(1.00f, 0.97f, 0.88f, 1f),   // sand
                new Color(0.50f, 0.46f, 0.36f, 1f),   // peat
                new Color(1f, 1f, 1f, 1f),            // leaf floor
                new Color(0.70f, 0.64f, 0.42f, 1f),   // dry grass
            };
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] == null)
                    continue;
                Vector4 max = remap[i];
                if (layers[i].diffuseRemapMax != max || layers[i].diffuseRemapMin != Vector4.zero)
                {
                    layers[i].diffuseRemapMin = Vector4.zero;
                    layers[i].diffuseRemapMax = max;
                    EditorUtility.SetDirty(layers[i]);
                }
            }
            return layers;
        }

        /// <summary>Four octaves of noise, 1500 m to 13 m, in 0..1.</summary>
        static float Fbm(float x, float z)
        {
            return (0.35f * Mathf.PerlinNoise(x / 1500f + 23f, z / 1500f + 17f)
                  + 0.55f * Mathf.PerlinNoise(x / 400f + 11f, z / 400f + 3f)
                  + 0.30f * Mathf.PerlinNoise(x / 90f + 5f, z / 90f + 7f)
                  + 0.15f * Mathf.PerlinNoise(x / 13f + 2f, z / 13f + 9f)) / 1.35f;
        }

        static bool IsWay(byte u) => u == (byte)DorestadTown.Use.Lane || u == (byte)DorestadTown.Use.Track || u == UseVerge;

        /// <summary>How trodden the ground beside a lane or track is: 0.6 one texel out, 0.3 two texels out, 0 beyond.</summary>
        static float Verge(DorestadTown.Plan plan, Vector2 p, float texel)
        {
            if (plan == null)
                return 0f;
            for (int k = 1; k <= 2; k++)
            {
                float d = k * texel;
                if (IsWay(plan.Get(p + new Vector2(d, 0f))) || IsWay(plan.Get(p - new Vector2(d, 0f)))
                    || IsWay(plan.Get(p + new Vector2(0f, d))) || IsWay(plan.Get(p - new Vector2(0f, d))))
                    return k == 1 ? 0.6f : 0.3f;
            }
            return 0f;
        }

        /// <summary>Paints the terrain. plan.use overrides the cover where the town has used the land.</summary>
        public static void Paint(TerrainData td, DorestadBuilder.Grid g, byte[] cover, byte[] wood, DorestadTown.Plan plan)
        {
            LoadThresholds();
            int res = DorestadBuilder.AlphaRes;
            int layers = td.terrainLayers.Length;
            td.alphamapResolution = res;
            td.baseMapResolution = 4096;      // so the remapped colours survive out to the basemap distance
            var a = new float[res, res, layers];
            float tx = g.size / res;
            float texel = plan != null ? plan.texel : tx;
            var w = new float[layers];
            byte lane = (byte)DorestadTown.Use.Lane, track = (byte)DorestadTown.Use.Track, yard = (byte)DorestadTown.Use.Yard,
                 farmyard = (byte)DorestadTown.Use.Farmyard, field = (byte)DorestadTown.Use.Field, orchard = (byte)DorestadTown.Use.Orchard,
                 foreshore = (byte)DorestadTown.Use.Foreshore, shoal = (byte)DorestadTown.Use.Shoal, cemetery = (byte)DorestadTown.Use.Cemetery,
                 market = (byte)DorestadTown.Use.Market;
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    var p = new Vector2((x + 0.5f) * tx, (z + 0.5f) * tx);
                    System.Array.Clear(w, 0, layers);
                    byte cv = cover != null ? Sample(cover, g, p) : Crest;
                    float wd = wood != null ? Sample(wood, g, p) / 255f : 0f;
                    float h = g.H(p) - g.wl, sd = g.SD(p);
                    float n1500 = Mathf.PerlinNoise(p.x / 1500f + 23f, p.y / 1500f + 17f);
                    float n400 = Mathf.PerlinNoise(p.x / 400f + 11f, p.y / 400f + 3f);
                    float n90 = Mathf.PerlinNoise(p.x / 90f + 5f, p.y / 90f + 7f);
                    float n13 = Mathf.PerlinNoise(p.x / 13f + 2f, p.y / 13f + 9f);
                    float n = (0.35f * n1500 + 0.55f * n400 + 0.30f * n90 + 0.15f * n13) / 1.35f;
                    float fine = Mathf.PerlinNoise(p.x / 7f + 40f, p.y / 7f + 40f);
                    float slope = Slope(g, p);
                    float wet = Wetness(h, sd);
                    byte use = plan != null ? plan.Get(p) : (byte)0;
                    bool way = IsWay(use);

                    // the town's land first
                    if (way)
                    {
                        if (use == lane)
                        {
                            // a lane: the trodden core broken by mud and bits of grass, not one solid stroke
                            w[LTrack] = 0.55f + 0.3f * fine;
                            w[LMud] = 0.25f * (1f - fine);
                            w[LPasture] = 0.2f * (1f - fine);
                        }
                        else if (use == track)
                        {
                            w[LTrack] = 0.4f + 0.3f * fine;
                            w[LPasture] = 0.35f;
                            w[LMud] = 0.2f * (1f - fine);
                        }
                        else
                        {
                            // a verge painted by the town
                            w[LPasture] = 0.55f;
                            w[LTrack] = 0.25f * fine;
                            w[LHay] = 0.2f;
                        }
                    }
                    else if (use == yard || use == farmyard)
                    {
                        w[LPasture] = 0.55f + 0.3f * fine;
                        w[LMud] = 0.25f * (1f - fine);
                        w[LTrack] = 0.2f * n;
                    }
                    else if (use == field)
                    {
                        // strips: each field block has its own direction (the town's, so the ridge-and-furrow in the
                        // heights lines up) and strip width; a quarter of the strips ploughed, a third stubble and
                        // fallow (the two-field system left half the arable bare), the rest in green crop; the
                        // headlands at the strips' ends are turned on, trodden grass
                        FieldFrame(plan, p, out int block, out float along, out float alongU, out float halfLen);
                        float stripW = 9f + Hash(block + 7) * 9f;
                        int strip = Mathf.FloorToInt(along / stripW);
                        float s = Hash(strip * 31 + block);
                        if (halfLen > 0f && Mathf.Abs(alongU) > halfLen - 6f)
                        {
                            w[LPasture] = 0.7f;
                            w[LTrack] = 0.3f;
                        }
                        else
                        {
                            if (s < 0.25f) { w[LPlough] = 0.6f; w[LStubble] = 0.4f * fine; }
                            else if (s < 0.6f) { w[LStubble] = 0.7f; w[LDryGrass] = 0.3f; }
                            else { w[LHay] = 0.5f; w[LPasture] = 0.5f; }
                            float edge = Mathf.Abs(along / stripW - strip - 0.5f) * 2f;   // 1 at a strip's edge
                            if (edge > 0.88f)
                            {
                                w[LPasture] += 0.6f;
                                w[LPlough] *= 0.4f;
                            }
                        }
                    }
                    else if (use == orchard)
                    {
                        w[LHay] = 0.7f;
                        w[LPasture] = 0.3f;
                    }
                    else if (use == foreshore || use == shoal)
                    {
                        // the harbour's shoal and the landing beach: the silted mud nearer the bank, the sandy beach
                        // band toward the water (DorestadRiver sets the cover), the puddles' and the heads' bed mud
                        float sandy = Mathf.PerlinNoise(p.x / 30f, p.y / 30f);
                        if (cv == Water || sd > 0f)
                            w[LMud] = 1f;
                        else if (cv == Bar)
                        {
                            w[LSand] = 0.5f + 0.3f * fine;
                            w[LMud] = 0.5f - 0.3f * fine + (h < 0.1f ? 0.5f : 0f);
                        }
                        else
                        {
                            w[LMud] = 0.6f + 0.25f * fine;
                            w[LMarsh] = 0.15f * n;
                            w[LSand] = 0.25f * sandy;
                            if (use == shoal && h > 0.3f)
                                w[LTrack] = 0.25f * fine;       // trodden about the dams' roots
                        }
                    }
                    else if (use == market)
                    {
                        // the market: trodden to mud, a little grass left
                        w[LTrack] = 0.5f + 0.3f * fine;
                        w[LMud] = 0.25f * (1f - fine);
                        w[LPasture] = 0.2f * n;
                    }
                    else if (use == cemetery)
                    {
                        w[LPasture] = 0.6f;
                        w[LHay] = 0.4f;
                    }
                    else
                    {
                        bool grazed = sd > -300f && (cv == Flank || cv == Basin) && Mathf.PerlinNoise(p.x / 35f + 61f, p.y / 35f + 29f) > 0.62f;
                        switch (cv)
                        {
                            case Crest:
                            {
                                // across the levee the 1500 m octave swings the crest from hay meadow to grazed pasture;
                                // dry ground comes in 100-300 m yellowed stretches from the 400 m octave only
                                float swing = Mathf.Clamp01((n1500 - 0.35f) / 0.3f);
                                w[LHay] = 0.55f + 0.3f * (1f - swing) + 0.15f * n;
                                w[LPasture] = 0.15f + 0.45f * swing + 0.15f * fine;
                                w[LDryGrass] = 0.35f * Mathf.Clamp01((n400 - 0.55f) * 4f);
                                break;
                            }
                            case Flank:
                                w[LHay] = 0.45f;
                                w[LPasture] = 0.35f + 0.3f * n;
                                w[LWet] = 0.2f * (1f - n);
                                break;
                            case Basin:
                                w[LWet] = 0.55f + 0.3f * n;
                                w[LMarsh] = 0.35f * (1f - n) * wet;
                                w[LPasture] = 0.2f * fine * (1f - wet);
                                break;
                            case Reed:
                                w[LMarsh] = 0.6f + 0.3f * n;
                                w[LWet] = 0.3f;
                                w[LMud] = 0.15f * (1f - n);
                                break;
                            case Carr:
                                w[LCarrFloor] = 0.4f + 0.6f * wd;
                                w[LWet] = 0.5f * (1f - wd);
                                w[LMarsh] = 0.15f * n;
                                break;
                            case Peat:
                                w[LPeat] = 0.55f + 0.3f * n;
                                w[LMarsh] = 0.25f * (1f - n);
                                w[LWet] = 0.2f;
                                break;
                            case Ridge:
                                w[LDryGrass] = 0.5f + 0.3f * n;
                                w[LHay] = 0.3f;
                                w[LLeaves] = 0.6f * wd;
                                w[LSand] = 0.12f * fine * (1f - wd);
                                break;
                            case Mud:
                                // mud with weed, not bare chocolate
                                w[LMud] = 0.45f + 0.25f * fine;
                                w[LMarsh] = 0.3f * n;
                                w[LWet] = 0.25f * (1f - n);
                                break;
                            case Bar:
                                // tan and damp, a wet mud band at the water
                                w[LSand] = 0.5f + 0.3f * fine;
                                w[LMud] = 0.5f - 0.3f * fine + (sd > -4f ? 0.4f : 0f);
                                w[LMarsh] = 0.2f * n;
                                break;
                            case Pool:
                                w[LMud] = 0.6f;
                                w[LMarsh] = 0.4f;
                                break;
                            case LeveeWood:
                                w[LLeaves] = 0.45f + 0.55f * wd;
                                w[LHay] = 0.5f * (1f - wd);
                                w[LDryGrass] = 0.2f * fine;
                                break;
                            default:
                                w[LHay] = 1f;
                                break;
                        }
                        // the grazed patches of the basin margins: hoof-trodden pasture (the hoof-printed Lek silt)
                        if (grazed)
                        {
                            for (int k = 0; k < layers; k++)
                                w[k] *= 0.4f;
                            w[LTrack] += 0.6f * 0.3f;
                            w[LPasture] += 0.6f * 0.5f;
                            w[LMud] += 0.6f * 0.2f;
                        }
                        // wood floor wherever the woods are thick, whatever the class
                        if (wd > 0.45f && cv != Carr && cv != LeveeWood)
                        {
                            float f = Mathf.InverseLerp(0.45f, 0.8f, wd);
                            for (int k = 0; k < layers; k++)
                                w[k] *= 1f - f;
                            w[cv == Ridge ? LLeaves : LCarrFloor] += f;
                        }
                    }

                    // the shore: mud by which side of a bend the bank is on (DorestadRiver's map): a point bar's
                    // shore ten to thirty metres, a cut bank a foot of it under the hay, a straight reach a few; the
                    // band comes and goes along the river with the 90 m octave (grass to the water between) and its
                    // outer third is broken up by the fine noise. Sand and marsh mix in on the bars' shores.
                    bool shoreable = !way && use != shoal && use != foreshore && use != market && cv != Bar && cv != Water;
                    if (shoreable && sd > -45f)
                    {
                        byte side = DorestadRiver.SideAt(g, p);
                        float k = DorestadRiver.StrengthAt(g, p);
                        float gate = Mathf.Clamp01((n90 - 0.35f) * 2.5f);
                        float width;
                        if (side == DorestadRiver.SideOuter)
                            width = Mathf.Lerp(1.5f, 4f, k);
                        else if (side == DorestadRiver.SideInner)
                            width = Mathf.Lerp(10f, 30f, k) * gate;
                        else
                            width = Mathf.Lerp(4f, 10f, n400) * gate;
                        width *= Mathf.Lerp(1.2f, 0.5f, Mathf.Clamp01(slope / 0.12f));
                        float mud = width > 0.3f ? Mathf.Clamp01((width + sd) / Mathf.Max(1.5f, width * 0.35f)) : 0f;
                        if (mud > 0f && -sd > width * 0.67f && fine < 0.25f)
                            mud = 0f;
                        if (mud > 0f)
                        {
                            for (int j = 0; j < layers; j++)
                                w[j] *= 1f - mud;
                            if (side == DorestadRiver.SideOuter)
                                w[LMud] += mud;
                            else if (side == DorestadRiver.SideInner)
                            {
                                w[LMud] += mud * 0.55f;
                                w[LMarsh] += mud * 0.25f;
                                w[LSand] += mud * 0.2f;
                            }
                            else
                            {
                                w[LMud] += mud * 0.8f;
                                w[LMarsh] += mud * 0.2f;
                            }
                        }
                    }
                    // the wetter the lower, everywhere off the levee (by the land's rank, as the bake classed it)
                    if (cv != Bar && cv != Mud && use == 0 && wet > 0f)
                    {
                        float wet2 = wet * 0.5f;
                        w[LWet] += wet2 * n;
                        w[LMarsh] += wet2 * (1f - n);
                    }
                    // the soft verge of a lane or track: trodden grass fading out over two texels, its reach jittered
                    if (!way && use != shoal && use != foreshore && cv != Water)
                    {
                        float vf = Verge(plan, p, texel) * Mathf.Clamp01(0.5f + fine);
                        if (vf > 0f)
                        {
                            float f = vf * 0.75f;
                            for (int j = 0; j < layers; j++)
                                w[j] *= 1f - f;
                            w[LPasture] += f * 0.55f;
                            w[LTrack] += f * 0.25f * fine;
                            w[LHay] += f * 0.2f;
                        }
                    }

                    float sum = 0f;
                    for (int k = 0; k < layers; k++)
                        sum += w[k];
                    for (int k = 0; k < layers; k++)
                        a[z, x, k] = sum > 0f ? w[k] / sum : (k == LHay ? 1f : 0f);
                }
            td.SetAlphamaps(0, 0, a);
        }

        /// <summary>Grass, reeds, sedge and nettles as the terrain's details, by cover and by what the town did.</summary>
        public static void Details(Terrain terrain, DorestadBuilder.Grid g, byte[] cover, byte[] wood, DorestadTown.Plan plan)
        {
            var clump = AssetLibrary.Grass("grass_medium_02", "Assets/Kapman/Prefabs/Nature/grass_medium_02.prefab", 1200);
            var td = terrain.terrainData;
            DetailPrototype Proto(float wLo, float wHi, float hLo, float hHi, Color healthy, Color dry) => new DetailPrototype
            {
                prototype = clump, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minWidth = wLo, maxWidth = wHi, minHeight = hLo, maxHeight = hHi, noiseSpread = 0.35f, healthyColor = healthy, dryColor = dry,
            };
            GameObject Pack(string relative) => MeadowPack.Prefab(relative);
            DetailPrototype PackProto(GameObject prefab, float lo, float hi) => new DetailPrototype
            {
                prototype = prefab, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minWidth = lo, maxWidth = hi, minHeight = lo, maxHeight = hi, noiseSpread = 0.3f, healthyColor = Color.white, dryColor = Color.white,
            };
            // the clumps' tints sit on the meadow layers' values (pale clumps read as white speckle on the green)
            var meadow = Proto(0.8f, 1.3f, 0.6f, 1.0f, new Color(0.48f, 0.56f, 0.28f), new Color(0.58f, 0.54f, 0.32f));
            var tallGrass = Proto(1.0f, 1.5f, 1.1f, 1.7f, new Color(0.49f, 0.56f, 0.29f), new Color(0.65f, 0.60f, 0.39f));
            var reeds = Proto(0.8f, 1.1f, 2.6f, 3.8f, new Color(0.70f, 0.70f, 0.50f), new Color(0.74f, 0.66f, 0.46f));
            var sedge = Proto(0.9f, 1.3f, 0.9f, 1.4f, new Color(0.46f, 0.58f, 0.30f), new Color(0.52f, 0.56f, 0.34f));
            var nettles = Proto(0.9f, 1.2f, 1.0f, 1.4f, new Color(0.30f, 0.44f, 0.22f), new Color(0.34f, 0.44f, 0.24f));
            // the bought meadow pack (NatureManufacture, 4 Oct 2026), where it is installed: scanned meadow grass in place of
            // the stretched clump, and the meadow flowers of the Rhine delta as their own layers (clover and daisy on the
            // pasture, brown knapweed in the hay, cornflower in the corn, dead grass on the dry crest). Its "sunroot" is an
            // American plant and stays out.
            // (the pack's meadow_02 is the low sward, a 0.6 m card; meadow_01 the flowering grass, a 1.6 m card: the crossed
            // versions, so they keep their body seen from the side)
            var packGrass = Pack("Grass/Prefabs Unity Terrain Grass/prefab_Terrain_grass_meadow_02_cross_1.prefab");
            var packTall = Pack("Grass/Prefabs Unity Terrain Grass/prefab_Terrain_grass_meadow_01_cross_1.prefab");
            if (packGrass != null)
                {
                    meadow = PackProto(packGrass, 0.6f, 1.0f);       // 0.35-0.6 m of sward
                    meadow.minWidth = 1.2f;                          // wide tufts, 0.8-1.1 m: a closed sward without a crowd of them
                    meadow.maxWidth = 1.6f;
                }
            if (packTall != null)
                tallGrass = PackProto(packTall, 0.45f, 0.65f);   // 0.75-1.05 m: a hay meadow in June
            var flowerFiles = new[]
            {
                "Details/Prefabs/prefab_detail_meadow_clover_01.prefab",
                "Details/Prefabs/prefab_detail_meadow_daisy_01.prefab",
                "Grass/Prefabs Unity Terrain Flowers/prefab_Terrain_flower_brownray_knapweed_01_1.prefab",
                "Grass/Prefabs Unity Terrain Flowers/prefab_Terrain_flower_cornflower_01_1.prefab",
                "Details/Prefabs/prefab_detail_meadow_dead_grass_01.prefab",
            };
            var prototypes = new System.Collections.Generic.List<DetailPrototype> { meadow, tallGrass, reeds, sedge, nettles };
            var flowerLayer = new int[flowerFiles.Length];
            for (int f = 0; f < flowerFiles.Length; f++)
            {
                var prefab = Pack(flowerFiles[f]);
                flowerLayer[f] = prefab != null ? prototypes.Count : -1;
                if (prefab != null)
                    prototypes.Add(PackProto(prefab, 0.85f, 1.15f));
            }
            td.detailPrototypes = prototypes.ToArray();
            const int Res = 2048;    // 2.5 m per detail cell (3 Oct: the reeds and verges need it)
            td.SetDetailResolution(Res, 32);
            // the layers hold plants per cell; Unity 6 makes new terrains read them as coverage (0-255 of the cell), which
            // turned 5-9 plants into 2-4 % cover: no grass to be seen (4 Oct)
            td.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
            var maps = new int[prototypes.Count][,];
            for (int i = 0; i < maps.Length; i++)
                maps[i] = new int[Res, Res];
            float cell = g.size / Res;
            float texel = plan != null ? plan.texel : cell;
            byte lane = (byte)DorestadTown.Use.Lane, track = (byte)DorestadTown.Use.Track, yard = (byte)DorestadTown.Use.Yard,
                 farmyard = (byte)DorestadTown.Use.Farmyard, field = (byte)DorestadTown.Use.Field, foreshore = (byte)DorestadTown.Use.Foreshore,
                 shoal = (byte)DorestadTown.Use.Shoal, cemetery = (byte)DorestadTown.Use.Cemetery, market = (byte)DorestadTown.Use.Market;
            for (int z = 0; z < Res; z++)
                for (int x = 0; x < Res; x++)
                {
                    var p = new Vector2((x + 0.5f) * cell, (z + 0.5f) * cell);
                    byte cv = cover != null ? Sample(cover, g, p) : Crest;
                    float wd = wood != null ? Sample(wood, g, p) / 255f : 0f;
                    float sd = g.SD(p), h = g.H(p) - g.wl;
                    byte use = plan != null ? plan.Get(p) : (byte)0;
                    float n = Fbm(p.x, p.y);
                    float slope = Slope(g, p);
                    if (sd > 1.5f || use == lane || use == track || use == shoal || use == UseVerge)
                        continue;
                    if (use == market)
                    {
                        // the market is trodden bare; nettles along its edge
                        bool edge = plan.Get(p + new Vector2(texel, 0f)) != market || plan.Get(p - new Vector2(texel, 0f)) != market
                                 || plan.Get(p + new Vector2(0f, texel)) != market || plan.Get(p - new Vector2(0f, texel)) != market;
                        if (edge && n > 0.65f)
                            maps[4][z, x] = 4;
                        continue;
                    }
                    if (use == cemetery)
                    {
                        maps[0][z, x] = 3;                 // kept short between the graves
                        continue;
                    }
                    if (use == field)
                    {
                        FieldFrame(plan, p, out int block, out float along, out float alongU, out float halfLen);
                        float stripW = 9f + Hash(block + 7) * 9f;
                        float s = Hash(Mathf.FloorToInt(along / stripW) * 31 + block);
                        if (halfLen > 0f && Mathf.Abs(alongU) > halfLen - 6f)
                            maps[0][z, x] = 4;             // the headland's trodden grass
                        else if (s >= 0.6f)
                            maps[1][z, x] = 14;            // the green strips: the crop stands above the stubble
                        continue;
                    }
                    if (use == yard || use == farmyard)
                    {
                        if (n > 0.62f)
                            maps[4][z, x] = 4;             // nettles round the yards' edges and middens
                        continue;
                    }
                    // the verges of the lanes and tracks: bare next to them, thin grass two texels out
                    float verge = Verge(plan, p, texel);
                    if (verge >= 0.6f)
                        continue;
                    if (verge > 0f)
                    {
                        maps[0][z, x] = 2;
                        continue;
                    }
                    // reeds: in the slack water of flat shores (never on a steep cut bank), and in the marsh
                    bool flatShore = sd > -14f && sd <= 1.5f && slope < 0.08f && use != foreshore && DorestadRiver.SideAt(g, p) != DorestadRiver.SideOuter;
                    if (flatShore || cv == Reed)
                    {
                        float dense = Mathf.PerlinNoise(p.x / 28f, p.y / 28f);
                        maps[2][z, x] = dense > 0.38f ? (int)(8 + 12 * dense) : (cv == Reed ? 5 : 0);
                        if (cv == Reed && dense <= 0.42f)
                            maps[3][z, x] = 8;
                        continue;
                    }
                    if (wd > 0.6f)
                    {
                        if (n > 0.55f) maps[3][z, x] = 3;   // a little sedge on the wood floor
                        continue;
                    }
                    switch (cv)
                    {
                        case Crest:
                        case Ridge:
                            maps[0][z, x] = (int)(4 + 6 * n);
                            if (n > 0.7f) maps[1][z, x] = 4;
                            break;
                        case Flank:
                            maps[0][z, x] = (int)(8 + 9 * n);
                            maps[1][z, x] = n > 0.5f ? 4 : 0;
                            break;
                        case Basin:
                            maps[1][z, x] = (int)(6 + 9 * n);
                            maps[3][z, x] = Wetness(h, sd) > 0.4f ? (int)(4 + 6 * (1f - n)) : 2;
                            break;
                        case Carr:
                        case Peat:
                            maps[3][z, x] = (int)(5 + 6 * n);
                            break;
                        case LeveeWood:
                            maps[0][z, x] = (int)(4 + 6 * (1f - wd));
                            break;
                        case Mud:
                        case Bar:
                            break;
                        default:
                            maps[0][z, x] = 6;
                            break;
                    }
                }
            // the pack's short grass is a small clump: two and a half times the old count makes a closed sward (capped)
            if (packGrass != null)
                for (int z = 0; z < Res; z++)
                    for (int x = 0; x < Res; x++)
                        maps[0][z, x] = Mathf.Min(32, maps[0][z, x] * 15 / 4);
            // the flowers: a share of the grass where each grows (none on lanes, yards, the shoal or the market)
            if (flowerLayer[0] >= 0 || flowerLayer[2] >= 0)
                for (int z = 0; z < Res; z++)
                    for (int x = 0; x < Res; x++)
                    {
                        int grass = maps[0][z, x];
                        var p = new Vector2((x + 0.5f) * cell, (z + 0.5f) * cell);
                        byte use = plan != null ? plan.Get(p) : (byte)0;
                        byte cv = cover != null ? Sample(cover, g, p) : Crest;
                        float f1 = Mathf.PerlinNoise(p.x / 23f + 7.1f, p.y / 23f + 3.3f), f2 = Mathf.PerlinNoise(p.x / 41f + 1.7f, p.y / 41f + 9.2f);
                        void Set(int which, int count) { if (flowerLayer[which] >= 0) maps[flowerLayer[which]][z, x] = count; }
                        if (use == field)
                        {
                            if (maps[1][z, x] > 0 && f1 > 0.62f)
                                Set(3, 2);                                         // cornflower in the corn
                            continue;
                        }
                        if (grass == 0 || use == lane || use == track || use == yard || use == shoal || use == market)
                            continue;
                        if (cv == Flank || cv == Basin || use == farmyard)
                        {
                            if (f1 > 0.58f) Set(0, 3);                             // clover in the grazed grass
                            if (f2 > 0.66f) Set(1, 2);                             // daisies
                        }
                        else if (cv == Crest || cv == Ridge)
                        {
                            if (f1 > 0.6f) Set(2, 2);                              // brown knapweed in the hay
                            if (f2 > 0.62f) Set(4, 3);                             // dead grass on the dry crest
                        }
                    }
            for (int i = 0; i < maps.Length; i++)
                td.SetDetailLayer(0, 0, i, maps[i]);
            terrain.detailObjectDistance = Kapman.Levels.LevelPerformance.GrassDistance;
            terrain.detailObjectDensity = 1f;
            EditorUtility.SetDirty(td);
        }

        /// <summary>
        /// Which field block a point is in (an id for the hash), how far across its strips it lies, how far along them,
        /// and the block's half-length along the strips (0 when the point is in no laid-out block: the eng fields).
        /// </summary>
        static void FieldFrame(DorestadTown.Plan plan, Vector2 p, out int block, out float across, out float along, out float halfLen)
        {
            if (plan != null && plan.fields != null)
                for (int i = 0; i < plan.fields.Count; i++)
                {
                    var f = plan.fields[i];
                    Vector2 d = p - f.c;
                    if (Mathf.Abs(Vector2.Dot(d, f.u)) <= f.a + 0.5f && Mathf.Abs(Vector2.Dot(d, f.v)) <= f.b + 0.5f)
                    {
                        block = 1000 + i * 37;
                        across = Vector2.Dot(d, f.v);      // the strips run along u; they are counted across v
                        along = Vector2.Dot(d, f.u);
                        halfLen = f.a;
                        return;
                    }
                }
            block = (int)(p.x / 160f) * 131 + (int)(p.y / 160f) * 17;
            float ang = Hash(block) * Mathf.PI;
            across = p.x * Mathf.Cos(ang) + p.y * Mathf.Sin(ang);
            along = -p.x * Mathf.Sin(ang) + p.y * Mathf.Cos(ang);
            halfLen = 0f;
        }

        public static byte Sample(byte[] map, DorestadBuilder.Grid g, Vector2 p)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(p.x / g.cell), 0, g.n - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / g.cell), 0, g.n - 1);
            return map[z * g.n + x];
        }

        static float Slope(DorestadBuilder.Grid g, Vector2 p)
        {
            float d = 5f;
            float dx = g.H(p + new Vector2(d, 0f)) - g.H(p - new Vector2(d, 0f));
            float dz = g.H(p + new Vector2(0f, d)) - g.H(p - new Vector2(0f, d));
            return Mathf.Sqrt(dx * dx + dz * dz) / (2f * d);
        }

        static float Hash(int k)
        {
            unchecked
            {
                uint h = (uint)k * 2654435761u;
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }
    }
}
