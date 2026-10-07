using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// BUILDS THE DORESTAD LEVEL - the first river place, at true size (1:1), as a scene to open and edit:
    ///   - the ground c. 800 from the lidar (Bake/levels/make_dorestad.py) as a Unity Terrain, 5.12 km square at 2.5 m;
    ///   - the 800 Lek and Kromme Rijn as water at their true width;
    ///   - the land painted by what it was: river banks and mud, levee meadows, wet basin meadows, fields, the plots,
    ///     lanes and street of the emporium, the farmyards, and the floors of the wet woods;
    ///   - the emporium (DorestadTown) and the trees (CC0 stand-ins until a pack is picked);
    ///   - the sky, the weather and seasons, the water's level and floods, the sounds, the camera and the test panel.
    /// Menu: Kapman > Levels > Build Dorestad. It writes Scenes/L_Dorestad.unity (a rebuild replaces it).
    /// </summary>
    public static class DorestadBuilder
    {
        public const string DataDir = @"G:\kapman_unity\data\levels\dorestad";
        public const string LevelDir = "Assets/Kapman/Levels/Dorestad";
        public const string ScenePath = "Assets/Kapman/Scenes/L_Dorestad.unity";
        public const int AlphaRes = 2048;

        public class Grid
        {
            public int n;
            public float cell, size, wl, hmin, hmax;
            public float[] h, sd;
            public byte[] cls;
            public Vector2 port;
            public List<Vector2> channel = new List<Vector2>();
            public double lat, lon;

            float Sample(float[] a, Vector2 p)
            {
                float fx = Mathf.Clamp(p.x / cell, 0, n - 1.001f), fz = Mathf.Clamp(p.y / cell, 0, n - 1.001f);
                int x = (int)fx, z = (int)fz;
                float tx = fx - x, tz = fz - z;
                int i = z * n + x;
                return Mathf.Lerp(Mathf.Lerp(a[i], a[i + 1], tx), Mathf.Lerp(a[i + n], a[i + n + 1], tx), tz);
            }

            public float H(Vector2 p) => Sample(h, p);
            public float SD(Vector2 p) => Sample(sd, p);

            public byte Class(Vector2 p)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(p.x / cell), 0, n - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / cell), 0, n - 1);
                return cls[z * n + x];
            }
        }

        public static Grid Load()
        {
            var meta = Json.Parse(File.ReadAllText(Path.Combine(DataDir, "meta.json")));
            var g = new Grid
            {
                n = (int)Json.Num(meta, "n"),
                cell = (float)Json.Num(meta, "cell"),
                size = (float)Json.Num(meta, "size"),
                wl = (float)Json.Num(meta, "water_level"),
                hmin = (float)Json.Num(meta, "height_min"),
                hmax = (float)Json.Num(meta, "height_max"),
            };
            var centre = Json.Arr(meta, "center_latlon");
            g.lat = System.Convert.ToDouble(centre[0]);
            g.lon = System.Convert.ToDouble(centre[1]);
            var port = Json.Arr(meta, "port");
            g.port = new Vector2(System.Convert.ToSingle(port[0]), System.Convert.ToSingle(port[1]));
            foreach (var o in Json.Arr(meta, "channel"))
            {
                var pt = (List<object>)o;
                g.channel.Add(new Vector2(System.Convert.ToSingle(pt[0]), System.Convert.ToSingle(pt[1])));
            }
            int count = g.n * g.n;
            g.h = ReadFloats(Path.Combine(DataDir, "height.f32"), count);
            g.sd = ReadFloats(Path.Combine(DataDir, "water_sd.f32"), count);
            g.cls = File.ReadAllBytes(Path.Combine(DataDir, "class.u8"));
            return g;
        }

        static float[] ReadFloats(string file, int count)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes.Length != count * 4)
                throw new System.Exception($"Kapman Dorestad: {file} has {bytes.Length} bytes, not {count * 4}.");
            var a = new float[count];
            System.Buffer.BlockCopy(bytes, 0, a, 0, bytes.Length);
            return a;
        }

        [MenuItem("Kapman/Levels/Build Dorestad")]
        public static void Build()
        {
            ProjectSetup.Apply();
            var g = Load();
            Directory.CreateDirectory(LevelDir);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // the level, its clock, weather and water
            var levelGo = new GameObject("Level: Dorestad c. 800 (true size)");
            var info = levelGo.AddComponent<LevelInfo>();
            info.levelId = "dorestad";
            info.displayName = "Dorestad";
            info.kind = LevelKind.RiverPlace;
            info.latitude = g.lat;
            info.longitude = g.lon;
            info.baseWaterLevel = g.wl;
            info.WaterLevel = g.wl;
            info.realMetresPerUnit = 1f;
            info.shipSpeed = 1.6f;
            var clock = levelGo.AddComponent<LevelClock>();
            clock.startDay = 172;
            clock.startHour = 19.25f;
            var weather = levelGo.AddComponent<LevelWeather>();
            weather.kind = WeatherKind.Fair;
            weather.manual = false;    // the weather engine: the place's real climate, fronts and lows (G sets a kind by hand)
            var levelWater = levelGo.AddComponent<LevelWater>();
            levelWater.tideShare = 0f;    // no tide here: Dorestad is ~100 km from the sea (Research/water_look.md); the level moves with the seasons and floods
            var waves = levelGo.AddComponent<WaterWaves>();

            // the one global volume: the sky, the clouds, the fog, the exposure and the post-processing (HDRP)
            var volGo = new GameObject("Sky, fog and post-processing", typeof(Volume));
            var vol = volGo.GetComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProjectSetup.VolumePath);
            weather.volume = vol;

            // the sun and the moon
            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.9f;
            var moon = new GameObject("Moon", typeof(Light)).GetComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.None;
            clock.sun = sun;
            clock.moon = moon;

            // the harbour's parcels first (the river's shoal is lowered under them), the river's shapes, then the town
            // (it marks where the land is used and flattens under its buildings), the ground painted from the real land
            // cover, the water, the planting, the river's props and the birds
            var world = new GameObject("World").transform;
            var cover = DorestadLandCover.LoadCover();
            var wood = DorestadLandCover.LoadWood();
            var rnd = new System.Random(793);
            var parcels = DorestadTown.Harbour(g, 800);
            DorestadRiver.Shape(g, cover, parcels);
            var plan = DorestadTown.Build(g, world, 800, parcels);
            var terrain = MakeTerrain(g, plan, cover, wood, world);
            // the water: HDRP's Water System on the river's own mesh, with the stream baked down the channel
            var riverMesh = WaterMesh(g);
            var floodMesh = FloodSheet(g);
            bool firstUpstream = DistToLek(g, g.channel[0]) <= DistToLek(g, g.channel[g.channel.Count - 1]);
            var surface = HdrpWater.MakeRiver(world, riverMesh, g.wl, g.channel, 60f, g.size, g.SD, LevelDir, floodMesh,
                                              out var floodSheet, streamSpeed: 0.4f, firstPointUpstream: firstUpstream);
            levelWater.surfaces = new[] { surface.transform };
            levelWater.floodSheet = floodSheet;
            // whether the stream pushes the ship comes from the chain (the user, 4 Oct: it does)
            var chainLevel = Kapman.Chain.ChainData.Load(Kapman.Chain.ChainData.ProjectPath).Level("dorestad");
            var current = surface.GetComponent<Kapman.Levels.Water.RiverCurrent>();
            if (current != null && chainLevel != null && chainLevel.water != null)
                current.pushesShips = chainLevel.water.pushesShips;
            DorestadVegetation.Plant(g, plan, cover, wood, terrain, world, rnd, out var leaves, out var leafColours, out var leafCutoffs);
            DorestadLandCover.Details(terrain, g, cover, wood, plan);
            DorestadVegetation.Reeds(terrain, g, cover, plan);    // after Details: it swaps the stretched-grass reeds for clumps
            MeadowMix.Mix(terrain.terrainData);    // the meadow pack's grasses and flowers as a mosaic, not one grass
            EditorUtility.SetDirty(terrain.terrainData);
            DorestadRiver.Dress(g, plan, cover, world, rnd);
            DorestadBirds.Make(g, plan, world, rnd, cover);
            // the ships on the real water: the player's with her wake, the moored boats with the water kept out of them
            HdrpWater.DressShip(plan.player.gameObject, wake: true);
            foreach (var boat in world.GetComponentsInChildren<ShipMotion>(true))
                if (boat.transform != plan.player)
                    HdrpWater.DressShip(boat.gameObject, wake: false);
            HdrpWater.AddWaterLine(levelGo);
            HdrpWater.AddCalmReflections(levelGo, vol);
            weather.leaves = leaves;
            weather.leafColours = leafColours;
            weather.leafCutoffs = leafCutoffs;

            // the town's life
            var life = levelGo.AddComponent<TownLife>();
            life.hearths = plan.hearths.ToArray();
            life.lanterns = plan.lanterns.ToArray();
            life.smokes = plan.smokes.ToArray();
            life.bonfires = plan.bonfires.ToArray();
            life.flames = plan.flames.ToArray();

            // the year in the animals: the flocks keep the wildlife tables' calendar (Design/wildlife_and_seasons.md, slice 1)
            var wildlife = levelGo.AddComponent<Wildlife>();
            string wild = Kapman.Nature.WildlifeModel.ProjectFolder + "/";
            wildlife.regionsFile = AssetDatabase.LoadAssetAtPath<TextAsset>(wild + Kapman.Nature.RegionTable.FileName);
            wildlife.speciesFile = AssetDatabase.LoadAssetAtPath<TextAsset>(wild + Kapman.Nature.SpeciesTable.FileName);
            wildlife.farmYearFile = AssetDatabase.LoadAssetAtPath<TextAsset>(wild + Kapman.Nature.FarmYear.FileName);
            wildlife.eventsFile = AssetDatabase.LoadAssetAtPath<TextAsset>(wild + Kapman.Nature.WildlifeEvents.FileName);
            wildlife.chronicleFile = AssetDatabase.LoadAssetAtPath<TextAsset>(Kapman.Nature.WildlifeModel.ChroniclePath);

            // the sky: HDRP's physically based sky driven by the clock, with the real stars (HDRP lights and reflects from it itself)
            var skyGo = new GameObject("Sky");
            var dome = skyGo.AddComponent<SkyDome>();
            dome.stars = AssetDatabase.LoadAssetAtPath<Cubemap>(ProjectSetup.StarsPath);
            dome.volume = vol;

            // the camera, the post-processing, the look and the test panel
            var camGo = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 15000f;
            cam.fieldOfView = 50f;
            var camData = camGo.AddComponent<HDAdditionalCameraData>();
            camData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            camData.volumeLayerMask = ~0;
            camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
            var orbit = camGo.AddComponent<OrbitCamera>();
            orbit.target = plan.player;
            Vector3 toTown = plan.townCentre - plan.player.position;
            orbit.yaw = Mathf.Atan2(toTown.x, toTown.z) * Mathf.Rad2Deg + 25f;
            orbit.pitch = 12f;
            orbit.distance = 32f;
            camGo.transform.position = plan.waterfrontView;
            camGo.transform.LookAt(plan.waterfrontFocus);
            weather.rain = Precipitation(camGo.transform, "Rain", false);
            weather.snow = Precipitation(camGo.transform, "Snow", true);

            var hudGo = new GameObject("Test panel");
            var hud = hudGo.AddComponent<DevHud>();
            hud.water = levelWater;

            var sounds = Sounds();
            SoundscapeSetup.AddTo(sounds.gameObject, SoundscapeSetup.BakeShoreMap(g), g.size);
            Views(plan, g);
            var ways = new GameObject("Ways in and out").transform;
            ExitAndEntry(ways, "Sail down the Rhine to the sea", OceanBuilder.SceneName, "rhine_mouth", plan.exitDownstream,
                         "from_sea", plan.entryFromSea, plan.entryYaw, 90f);

            // the game itself: the tables, the sim, the chart, the panels and the graphs (ported from the first build, 4 Oct)
            GameSetup.AddToScene(false);
            WaterProfileSetup.AddToOpenScene("dorestad");
            clock.Apply();
            LandmarkExport.WriteDorestad(scene, g.wl);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman Dorestad: built {ScenePath} (water level {g.wl:0.00} m).");
        }

        // ------------------------------------------------------------------ the ground

        static Terrain BuildTerrain(Grid g, DorestadTown.Plan plan, Transform parent)
        {
            float lo = g.hmin - 1f, hi = g.hmax + 1f;
            var td = new TerrainData { heightmapResolution = g.n };
            td.size = new Vector3(g.size, hi - lo, g.size);
            var hts = new float[g.n, g.n];
            for (int z = 0; z < g.n; z++)
                for (int x = 0; x < g.n; x++)
                    hts[z, x] = (g.h[z * g.n + x] - lo) / (hi - lo);
            td.SetHeights(0, 0, hts);

            string L = LevelDir + "/Ground";
            var layers = new[]
            {
                AssetLibrary.Layer("ambientcg", "Grass001", 3f, L + "/0_meadow_grass001.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Grass002", 3f, L + "/1_wet_meadow_grass002.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "farm_furrows", 6f, L + "/2_field.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "brown_mud_02", 3f, L + "/3_mud.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "muddy_tracks", 4f, L + "/4_track.terrainlayer"),
                AssetLibrary.Layer("polyhaven", "forest_ground_04", 4f, L + "/5_wood_floor.terrainlayer"),
                AssetLibrary.Layer("ambientcg", "Grass004", 3f, L + "/6_trodden_grass004.terrainlayer"),
            };
            td.terrainLayers = layers;
            td.alphamapResolution = AlphaRes;
            td.baseMapResolution = 1024;
            var a = new float[AlphaRes, AlphaRes, layers.Length];
            float tx = g.size / AlphaRes;
            var w = new float[layers.Length];
            for (int z = 0; z < AlphaRes; z++)
                for (int x = 0; x < AlphaRes; x++)
                {
                    var p = new Vector2((x + 0.5f) * tx, (z + 0.5f) * tx);
                    float h = g.H(p) - g.wl, sd = g.SD(p);
                    float noise = Mathf.PerlinNoise(p.x / 90f, p.y / 90f), fine = Mathf.PerlinNoise(p.x / 13f + 40f, p.y / 13f + 40f);
                    System.Array.Clear(w, 0, w.Length);
                    byte use = plan.use[z * AlphaRes + x];
                    if (sd > -6f || use == (byte)DorestadTown.Use.Foreshore)
                        w[3] = 1f;
                    else if (use == (byte)DorestadTown.Use.Lane)
                        w[4] = 1f;
                    else if (use == (byte)DorestadTown.Use.Yard || use == (byte)DorestadTown.Use.Farmyard)
                    {
                        w[6] = 0.7f + 0.3f * fine;
                        w[4] = 0.3f * (1f - fine);
                    }
                    else if (use == (byte)DorestadTown.Use.Field)
                        w[2] = 1f;
                    else if (Woods(g, plan, p) > 0.5f)
                        w[5] = 1f;
                    else
                    {
                        float wet = Mathf.Clamp01(Mathf.InverseLerp(1.0f, 0.2f, h) + (noise - 0.5f) * 0.6f);
                        w[0] = 1f - wet;
                        w[1] = wet;
                        if (sd > -14f)
                        {
                            float bank = Mathf.InverseLerp(-14f, -6f, sd);
                            for (int k = 0; k < w.Length; k++)
                                w[k] *= 1f - bank;
                            w[3] += bank;
                        }
                    }
                    float sum = w.Sum();
                    for (int k = 0; k < w.Length; k++)
                        a[z, x, k] = sum > 0 ? w[k] / sum : (k == 0 ? 1f : 0f);
                }
            td.SetAlphamaps(0, 0, a);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), LevelDir));
            AssetDatabase.CreateAsset(td, LevelDir + "/Dorestad_TerrainData.asset");
            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "Ground c. 800 (AHN4 lidar, today's works taken off)";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, lo, 0f);
            var t = go.GetComponent<Terrain>();
            t.heightmapPixelError = 1f;
            t.basemapDistance = 1500f;
            t.drawInstanced = true;
            go.isStatic = true;
            return t;
        }

        /// <summary>The wet woods: alder and willow carr in the low basins away from the town (patchy).</summary>
        public static float Woods(Grid g, DorestadTown.Plan plan, Vector2 p)
        {
            if (plan.Get(p) != 0)
                return 0f;
            float h = g.H(p) - g.wl, sd = g.SD(p);
            if (sd > -35f || h > 1.1f)
                return 0f;
            if (Vector2.Distance(p, new Vector2(plan.townCentre.x, plan.townCentre.z)) < 350f)
                return 0f;
            float patches = Mathf.PerlinNoise(p.x / 320f + 7f, p.y / 320f + 3f) + 0.25f * Mathf.PerlinNoise(p.x / 60f, p.y / 60f);
            return patches - 0.15f * h > 0.66f ? 1f : 0f;
        }

        /// <summary>The terrain from the shaped heights, painted by the real land cover (DorestadLandCover).</summary>
        static Terrain MakeTerrain(Grid g, DorestadTown.Plan plan, byte[] cover, byte[] wood, Transform parent)
        {
            float lo = g.h.Min() - 1f, hi = g.h.Max() + 1f;
            var td = new TerrainData { heightmapResolution = g.n };
            td.size = new Vector3(g.size, hi - lo, g.size);
            var hts = new float[g.n, g.n];
            for (int z = 0; z < g.n; z++)
                for (int x = 0; x < g.n; x++)
                    hts[z, x] = (g.h[z * g.n + x] - lo) / (hi - lo);
            td.SetHeights(0, 0, hts);
            td.terrainLayers = DorestadLandCover.Layers();
            // the asset first: alphamaps set on an unsaved TerrainData were lost when it was saved (every texel came
            // out as layer 0, found 3 October with TerrainCheck)
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), LevelDir));
            string tdPath = LevelDir + "/Dorestad_TerrainData.asset";
            AssetDatabase.DeleteAsset(tdPath);
            AssetDatabase.CreateAsset(td, tdPath);
            DorestadLandCover.Paint(td, g, cover, wood, plan);
            EditorUtility.SetDirty(td);
            AssetDatabase.SaveAssets();
            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "Ground c. 800 (AHN4 lidar, today's works taken off; painted by the land cover)";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, lo, 0f);
            var t = go.GetComponent<Terrain>();
            t.heightmapPixelError = 1f;
            t.basemapDistance = 4000f;
            t.drawInstanced = true;
            go.isStatic = true;
            return t;
        }

        // ------------------------------------------------------------------ the water

        /// <summary>How far a point is from the Lek's own water (class 10), to tell which end of the channel is upstream.</summary>
        static float DistToLek(Grid g, Vector2 p)
        {
            float best = 1e9f;
            for (float a = 0; a < 360; a += 30)
                for (float r = 10; r < 600; r += 20)
                {
                    Vector2 q = p + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * r;
                    if (g.Class(q) == 10 && r < best)
                        best = r;
                }
            return best;
        }

        /// <summary>The river's surface mesh (every cell within 8 m of the water), saved as an asset for the HDRP water surface.</summary>
        static Mesh WaterMesh(Grid g)
        {
            const int step = 2;
            int m = (g.n - 1) / step + 1;
            var index = new int[m * m];
            var verts = new List<Vector3>();
            for (int i = 0; i < index.Length; i++)
                index[i] = -1;
            int V(int x, int z)
            {
                int k = z * m + x;
                if (index[k] < 0)
                {
                    index[k] = verts.Count;
                    verts.Add(new Vector3(x * step * g.cell, 0f, z * step * g.cell));
                }
                return index[k];
            }
            var tris = new List<int>();
            for (int z = 0; z < m - 1; z++)
                for (int x = 0; x < m - 1; x++)
                {
                    float best = Mathf.Max(Mathf.Max(g.sd[(z * step) * g.n + x * step], g.sd[(z * step) * g.n + (x + 1) * step]),
                                           Mathf.Max(g.sd[((z + 1) * step) * g.n + x * step], g.sd[((z + 1) * step) * g.n + (x + 1) * step]));
                    if (best < -8f)
                        continue;
                    int a = V(x, z), b = V(x + 1, z), c = V(x + 1, z + 1), d = V(x, z + 1);
                    tris.AddRange(new[] { a, c, b, a, d, c });
                }
            var mesh = new Mesh { name = "Dorestad_water", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.bounds = new Bounds(mesh.bounds.center, mesh.bounds.size + Vector3.up * 4f);
            AssetDatabase.CreateAsset(mesh, LevelDir + "/Dorestad_water.asset");
            return mesh;
        }

        /// <summary>A flat sheet over the whole level: the same water surface shows it when the river is in flood.</summary>
        static Mesh FloodSheet(Grid g)
        {
            const int cells = 64;
            float s = g.size / cells;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int z = 0; z <= cells; z++)
                for (int x = 0; x <= cells; x++)
                    verts.Add(new Vector3(x * s, 0f, z * s));
            for (int z = 0; z < cells; z++)
                for (int x = 0; x < cells; x++)
                {
                    int a = z * (cells + 1) + x, b = a + 1, c = a + cells + 2, d = a + cells + 1;
                    tris.AddRange(new[] { a, c, b, a, d, c });
                }
            var mesh = new Mesh { name = "Dorestad_flood_sheet", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, LevelDir + "/Dorestad_flood_sheet.asset");
            return mesh;
        }

        // ------------------------------------------------------------------ the trees

        static Material[] Plant(Grid g, DorestadTown.Plan plan, Transform parent, out Color[] colours, out float[] cutoffs)
        {
            var kinds = new[] { "tree_small_02", "island_tree_01", "island_tree_02", "island_tree_03" };
            var prefabs = new GameObject[kinds.Length];
            var leaves = new Material[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
                prefabs[i] = AssetLibrary.Tree(kinds[i], $"Assets/Kapman/Prefabs/Nature/{kinds[i]}.prefab", out leaves[i]);
            colours = leaves.Select(l => l != null ? l.GetColor("_BaseColor") : Color.white).ToArray();
            cutoffs = leaves.Select(l => l != null ? l.GetFloat("_AlphaCutoff") : 0.45f).ToArray();

            var root = new GameObject("Trees (CC0 stand-ins: ash/alder, old oak, willow scrub)").transform;
            root.SetParent(parent, false);
            var gWoods = new GameObject("Wet woods").transform;
            gWoods.SetParent(root, false);
            var gBanks = new GameObject("Willow scrub on the banks").transform;
            gBanks.SetParent(root, false);
            var gLevee = new GameObject("Trees on the levees and by the farms").transform;
            gLevee.SetParent(root, false);
            var gHedges = new GameObject("Trees along the yards and fields").transform;
            gHedges.SetParent(root, false);
            var rnd = new System.Random(1793);
            float R() => (float)rnd.NextDouble();
            int placed = 0;
            // the scans are small trees (2.6-5 m): grown to the river land's heights (alder and ash 9-14 m, old oaks
            // 12-16 m, willow scrub 4-7 m)
            var baseScale = new[] { 2.4f, 2.7f, 1.6f, 1.8f };
            void Put(int kind, Transform group, Vector2 p, float scaleLo, float scaleHi)
            {
                var t = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[kind], group);
                t.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) - 0.2f, p.y), Quaternion.Euler(0f, R() * 360f, 0f));
                t.transform.localScale = Vector3.one * baseScale[kind] * Mathf.Lerp(scaleLo, scaleHi, R());
                t.isStatic = true;
                placed++;
            }
            bool NearWorkedLand(Vector2 p)
            {
                foreach (var o in new[] { new Vector2(9f, 0f), new Vector2(-9f, 0f), new Vector2(0f, 9f), new Vector2(0f, -9f) })
                {
                    byte u = plan.Get(p + o);
                    if (u == (byte)DorestadTown.Use.Farmyard || u == (byte)DorestadTown.Use.Field)
                        return true;
                }
                return false;
            }
            const float Step = 11f;
            for (float z = Step / 2; z < g.size; z += Step)
                for (float x = Step / 2; x < g.size; x += Step)
                {
                    var p = new Vector2(x + (R() - 0.5f) * Step * 0.8f, z + (R() - 0.5f) * Step * 0.8f);
                    if (plan.Get(p) != 0)
                        continue;
                    float sd = g.SD(p), h = g.H(p) - g.wl;
                    if (sd > -3f)
                        continue;
                    if (Woods(g, plan, p) > 0.5f)
                    {
                        if (R() < 0.42f)
                            Put(R() < 0.75f ? 0 : 1, gWoods, p, 0.85f, 1.35f);
                    }
                    else if (sd > -16f)
                    {
                        bool town = Vector2.Distance(p, new Vector2(plan.townCentre.x, plan.townCentre.z)) < 250f;
                        if (!town && R() < 0.14f)
                            Put(R() < 0.5f ? 2 : 3, gBanks, p, 0.7f, 1.2f);
                    }
                    else if (NearWorkedLand(p))
                    {
                        if (R() < 0.10f)
                            Put(R() < 0.55f ? 1 : 0, gHedges, p, 0.85f, 1.25f);
                    }
                    else if (h > 0.9f && R() < 0.012f)
                        Put(R() < 0.6f ? 1 : 0, gLevee, p, 0.9f, 1.3f);
                }
            Debug.Log($"Kapman Dorestad: {placed} trees planted.");
            return leaves;
        }

        // ------------------------------------------------------------------ grass and reeds

        /// <summary>Meadow grass on the open land, and (the same scan, grown tall: a stand-in) reeds along the banks.</summary>
        static void Grass(Terrain terrain, Grid g, DorestadTown.Plan plan)
        {
            var clump = AssetLibrary.Grass("grass_medium_02", "Assets/Kapman/Prefabs/Nature/grass_medium_02.prefab", 1200);
            var td = terrain.terrainData;
            var grass = new DetailPrototype
            {
                prototype = clump, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minWidth = 0.9f, maxWidth = 1.4f, minHeight = 0.8f, maxHeight = 1.3f, noiseSpread = 0.4f,
                healthyColor = Color.white, dryColor = new Color(0.86f, 0.82f, 0.66f),
            };
            var reeds = new DetailPrototype
            {
                prototype = clump, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                minWidth = 0.8f, maxWidth = 1.1f, minHeight = 3.2f, maxHeight = 4.6f, noiseSpread = 0.3f,
                healthyColor = new Color(0.9f, 0.88f, 0.75f), dryColor = new Color(0.82f, 0.74f, 0.55f),
            };
            td.detailPrototypes = new[] { grass, reeds };
            const int Res = 1024;
            td.SetDetailResolution(Res, 32);
            var gm = new int[Res, Res];
            var rm = new int[Res, Res];
            float cell = g.size / Res;
            for (int z = 0; z < Res; z++)
                for (int x = 0; x < Res; x++)
                {
                    var p = new Vector2((x + 0.5f) * cell, (z + 0.5f) * cell);
                    byte use = plan.Get(p);
                    float sd = g.SD(p);
                    if (sd > -7f && sd < 1.5f && use != (byte)DorestadTown.Use.Foreshore && use != (byte)DorestadTown.Use.Yard)
                    {
                        rm[z, x] = Mathf.PerlinNoise(p.x / 25f, p.y / 25f) > 0.35f ? 14 : 3;
                        continue;
                    }
                    if (sd > 0f || use == (byte)DorestadTown.Use.Lane || use == (byte)DorestadTown.Use.Field || Woods(g, plan, p) > 0.5f)
                        continue;
                    float n = Mathf.PerlinNoise(p.x / 40f + 9f, p.y / 40f + 2f);
                    gm[z, x] = use == (byte)DorestadTown.Use.Yard || use == (byte)DorestadTown.Use.Farmyard ? (n > 0.6f ? 3 : 0) : (int)(6 + n * 10);
                }
            td.SetDetailLayer(0, 0, 0, gm);
            td.SetDetailLayer(0, 0, 1, rm);
            terrain.detailObjectDistance = 90f;
            terrain.detailObjectDensity = 1f;
            EditorUtility.SetDirty(td);
        }

        // ------------------------------------------------------------------ rain and snow

        public static ParticleSystem Precipitation(Transform cam, string name, bool snow, string dir = LevelDir)
        {
            var go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(cam, false);
            go.transform.localPosition = new Vector3(0f, 16f, 8f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = snow ? 9f : 1.6f;
            main.startSpeed = snow ? 1.2f : 13f;
            main.startSize = snow ? 0.07f : 0.025f;
            main.startColor = snow ? new Color(0.95f, 0.95f, 0.97f, 0.9f) : new Color(0.7f, 0.74f, 0.78f, 0.35f);
            main.maxParticles = snow ? 6000 : 9000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(70f, 70f, 1f);
            if (snow)
            {
                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = 0.6f;
                noise.frequency = 0.4f;
            }
            var r = go.GetComponent<ParticleSystemRenderer>();
            var mat = snow ? HdrpMaterials.Snow($"{dir}/Materials/{name}.mat") : HdrpMaterials.Rain($"{dir}/Materials/{name}.mat");
            r.sharedMaterial = mat;
            r.renderMode = snow ? ParticleSystemRenderMode.Billboard : ParticleSystemRenderMode.Stretch;
            r.velocityScale = snow ? 0f : 0.05f;
            r.lengthScale = 1f;
            r.shadowCastingMode = ShadowCastingMode.Off;
            ps.Play();
            return ps;
        }

        // ------------------------------------------------------------------ sounds

        public static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Kapman/Audio/Sounds/{name}.mp3");

        public static LevelSoundscape Sounds()
        {
            var go = new GameObject("Sounds (CC0 recordings)");
            var sc = go.AddComponent<LevelSoundscape>();
            AudioSource Loop(string name, string clip, bool spatial = false)
            {
                var c = new GameObject(name, typeof(AudioSource));
                c.transform.SetParent(go.transform, false);
                var a = c.GetComponent<AudioSource>();
                a.clip = Clip(clip);
                a.loop = true;
                a.playOnAwake = true;
                a.volume = 0f;
                a.spatialBlend = spatial ? 1f : 0f;
                return a;
            }
            sc.wind = Loop("Wind", "light_breeze_620099");
            sc.river = Loop("River lapping", "calm_water_lapping_the_hull_851577");
            sc.rain = Loop("Rain", "light_rain_200273");
            sc.crickets = Loop("Crickets", "night_crickets_522299");
            sc.oars = Loop("Oars", "oars_234361");
            sc.underKeel = Loop("Water under the keel", "water_past_the_hull_while_sailing_360631");
            var bird = new GameObject("Bird", typeof(AudioSource));
            bird.transform.SetParent(go.transform, false);
            sc.birdVoice = bird.GetComponent<AudioSource>();
            sc.birdVoice.spatialBlend = 1f;
            sc.birdVoice.minDistance = 15f;
            sc.birdVoice.maxDistance = 400f;
            sc.birdVoice.playOnAwake = false;
            var thunder = new GameObject("Thunder", typeof(AudioSource));
            thunder.transform.SetParent(go.transform, false);
            sc.thunderVoice = thunder.GetComponent<AudioSource>();
            sc.thunderVoice.playOnAwake = false;
            sc.dayBirds = new[] { Clip("skylark_387426"), Clip("skylark_635912"), Clip("crow_741366"), Clip("curlew_475971") };
            sc.waterBirds = new[] { Clip("black_headed_gull_554806"), Clip("geese_407490"), Clip("curlew_829120") };
            sc.nightBirds = new[] { Clip("tawny_owl_688869"), Clip("tawny_owl_203598") };
            sc.thunderNear = new[] { Clip("close_thunder_347853"), Clip("close_thunder_574412") };
            sc.thunderFar = new[] { Clip("distant_thunder_107518"), Clip("distant_thunder_361401") };
            return sc;
        }

        /// <summary>A way out of a level (a prompt at the spot) and the way the ship comes in by.</summary>
        public static void ExitAndEntry(Transform parent, string prompt, string scene, string entryThere, Vector3 exitAt,
                                        string entryHere, Vector3 entryAt, float entryYaw, float radius)
        {
            var ex = new GameObject("Way out: " + prompt).AddComponent<LevelExit>();
            ex.transform.SetParent(parent, false);
            ex.transform.position = exitAt;
            ex.prompt = prompt;
            ex.targetScene = scene;
            ex.targetEntry = entryThere;
            ex.radius = radius;
            var en = new GameObject("Way in: " + entryHere).AddComponent<LevelEntry>();
            en.transform.SetParent(parent, false);
            en.transform.SetPositionAndRotation(entryAt, Quaternion.Euler(0f, entryYaw, 0f));
            en.entryId = entryHere;
        }

        // ------------------------------------------------------------------ views for the shots

        static void Views(DorestadTown.Plan plan, Grid g)
        {
            var root = new GameObject("Views (for the shots; move them as you like)").transform;
            void View(string name, Vector3 at, Vector3 lookAt)
            {
                var v = new GameObject(name).transform;
                v.SetParent(root, false);
                v.position = at;
                v.rotation = Quaternion.LookRotation(lookAt - at, Vector3.up);
            }
            Vector3 ship = plan.player.position;
            Vector3 toTown = (plan.townCentre - ship);
            toTown.y = 0f;
            toTown.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, toTown);
            View("1 The waterfront from the river", ship - toTown * 26f + side * 14f + Vector3.up * 4.5f, plan.townCentre + Vector3.up * 2f);
            View("2 Over the fork with the Lek", plan.forkFocus + new Vector3(-260f, 180f, -320f), plan.forkFocus + Vector3.up * 2f);
            View("3 Down the Kromme Rijn", plan.downRiverView, plan.downRiverFocus);
            Vector3 bow = plan.player.forward;
            View("4 Over the stern", ship - bow * 9f + side * 3.5f + Vector3.up * 4.2f, ship + bow * 30f + Vector3.up * 1f);
            View("5 High over the emporium", plan.townCentre + new Vector3(-400f, 320f, -200f), plan.townCentre);
            if (plan.bonfires.Count > 0)
            {
                Vector3 f = plan.bonfires[0].transform.position;
                View("6 The midsummer fire", f + side * 22f - toTown * 18f + Vector3.up * 5f, f + Vector3.up * 2f);
            }
            Vector3 hf = plan.houseFocus, hi = plan.houseInland;
            Vector3 hs = Vector3.Cross(Vector3.up, hi);
            View("7 A house and its yard, close", hf - hi * 16f + hs * 9f + Vector3.up * 3.2f, hf + Vector3.up * 2.5f);
            if (plan.parcels != null && plan.parcels.Count > 4)
            {
                var pc = plan.parcels.OrderBy(p => Mathf.Abs(p.s - 850f)).First();
                Vector3 damRoot = new Vector3(pc.bank.x, g.wl + 1f, pc.bank.y), outw = new Vector3(pc.outward.x, 0f, pc.outward.y), al = new Vector3(pc.along.x, 0f, pc.along.y);
                Vector3 head = damRoot + outw * pc.built;
                View("8 The dams from the water", head + outw * 45f - al * 60f + Vector3.up * 6f, damRoot + outw * (pc.built * 0.5f) + Vector3.up * 1f);
                View("9 Along a dam to the storehouses", head + outw * 2f + al * 3.5f + Vector3.up * 1.7f, damRoot + al * 2f - outw * (pc.built * 0.2f) + Vector3.up * 1.6f);
                View("10 The market and the vicus from the harbour", damRoot + outw * 20f + Vector3.up * 14f, plan.townCentre - outw * 60f + Vector3.up * 2f);
                View("11 The farm strip and the fields", plan.townCentre - outw * 120f + al * 80f + Vector3.up * 28f, plan.townCentre - outw * 330f - al * 40f + Vector3.up * 2f);
                View("12 Over the whole emporium, low sun", plan.townCentre + outw * 220f - al * 420f + Vector3.up * 120f, plan.townCentre - outw * 100f + al * 200f);
            }
        }
    }
}
