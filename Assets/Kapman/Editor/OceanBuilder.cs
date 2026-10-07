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
    /// BUILDS THE SOUTHERN NORTH SEA - the first ocean level, shortened hard (about 1:150 across) as the design has it:
    /// the coast as it was about 793 from the first build's whole-map bake (Bake/levels/make_ocean.py), heights stood up
    /// more than the shortening would give (land x0.12, the sea bed x0.1) so cliffs and dunes read; HDRP's ocean to the
    /// horizon with real-size waves from the wind (HdrpWater.MakeSea); the tide (EOT20, scaled with the sea bed); haze
    /// that keeps the far coasts a low line; and the prompts at the river mouths ("Sail up the Rhine to Dorestad").
    /// Ships, waves and people stay real size.
    ///
    /// The coasts are painted by what they were (the bake's surface types first, the heights second): the chalk cliffs of
    /// Kent and the Boulonnais from the slope, dunes and strand on the Holland coast, the Wadden flats, the salt marshes
    /// of Zeeland, Essex and Flanders, the peat behind, mud at the river mouths. Terrain trees stand on the dune and
    /// marsh coasts, the wooded hills and the Weald. The places of 793 (meta.json) are clusters of real-size blockout
    /// houses at the real spots, with the sea marks that stood then: the Roman pharos at Dover, Boulogne's lighthouse,
    /// Reculver's minster, Bradwell's chapel, Richborough's walls, Londinium's wall. A few traders are under way on
    /// fixed courses, gulls and geese fly over the flats, and fish weirs stand in the estuaries. No buoys: there were
    /// none in 793; the marks are churches, towers and headlands.
    /// Menu: Kapman > Levels > Build the southern North Sea. It writes Scenes/L_NorthSea.unity.
    /// </summary>
    public static class OceanBuilder
    {
        public const string DataDir = @"G:\kapman_unity\data\levels\northsea_south";
        public const string LevelDir = "Assets/Kapman/Levels/NorthSea";
        public const string SceneName = "L_NorthSea";
        public const string ScenePath = "Assets/Kapman/Scenes/" + SceneName + ".unity";
        const float LandV = 0.12f, SeaV = 0.1f;
        const int AlphaRes = 1024;
        public const int MaxTrees = 60000;

        // the bake's surface types: Bake/layer1_land/vos800_rules.csv on the Dutch 800 AD map, Bake/world/world_surfaces.csv beyond it
        const byte Sea = 1, Flats = 2, Marsh = 3, MarshRidge = 4, Peat = 5, Dunes = 6, DuneValley = 7, SandUpland = 8, BrookValley = 9,
                   Moraine = 10, Loess = 11, RiverDune = 12, InlandWater = 13, OlderGround = 14, River = 15, RiverReeds = 16, Bar = 17,
                   CutBank = 18, Lowland = 20, Hills = 21, Uplands = 22;

        // the terrain's layers, in order: eight, the most HDRP's terrain shader draws in one go
        const int LSeaBed = 0, LFlats = 1, LBeach = 2, LGrass = 3, LSaltMarsh = 4, LUpland = 5, LMud = 6, LChalk = 7, LayerCount = 8;

        // the tree scans (as DorestadVegetation): ash/alder, oak, willow, osier scrub; their scale up to real trees; four leaf tints
        const int Ash = 0, Oak = 1, Willow = 2, Osier = 3;
        static readonly string[] TreeKinds = { "tree_small_02", "island_tree_01", "island_tree_02", "island_tree_03" };
        static readonly float[] TreeScale = { 2.4f, 2.7f, 1.6f, 1.8f };
        static readonly Color[] LeafTints = { new Color(1f, 1f, 1f), new Color(0.78f, 0.86f, 0.70f), new Color(0.95f, 0.98f, 0.72f), new Color(1.08f, 1.0f, 0.80f) };

        enum Kind { House, Church, Chapel, Pharos, Lighthouse, Wall }

        struct Building
        {
            public Kind kind;
            public Vector2 at;
            public float yaw, y, width, length, wallHeight, ridge;
            public int walls, thatch;
        }

        class Town
        {
            public string name;
            public Vector2 centre;
            public float radius;
            public readonly List<Building> buildings = new List<Building>();
        }

        class Kit
        {
            public Material[] walls, thatch;
            public Material stone, piles, wattle;
        }

        /// <summary>The level's data and the maps derived from it, shared by the steps of the build.</summary>
        class Level
        {
            public int n;
            public float size, cell, scale;
            public double lat, lon;
            public float[] real, h;                // real metres over the 793 sea; game metres (stood up)
            public byte[] surface;
            public float[] dSea, dLand, dRiver;    // in cells: land to the sea, sea to the land, anywhere to the rivers and lakes of 793
            public readonly Dictionary<string, Vector2> places = new Dictionary<string, Vector2>();
            public readonly Dictionary<string, Vector2> marks = new Dictionary<string, Vector2>();
            public readonly List<Vector4> woods = new List<Vector4>();      // centre x, z and the half-sizes (game m)
            public readonly List<Town> towns = new List<Town>();
            public readonly Dictionary<string, Vector3> spots = new Dictionary<string, Vector3>();   // for the views
            public readonly System.Random rnd = new System.Random(793);

            public float R() => (float)rnd.NextDouble();
            public float R(float a, float b) => a + R() * (b - a);
            public bool Inside(Vector2 p) => p.x >= 0f && p.y >= 0f && p.x <= size && p.y <= size;

            public int Idx(Vector2 p)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(p.x / cell), 0, n - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / cell), 0, n - 1);
                return z * n + x;
            }

            public float H(Vector2 p) => h[Idx(p)];
            public float Real(Vector2 p) => real[Idx(p)];
            public float Water(Vector2 p) => Mathf.Min(dSea[Idx(p)], dRiver[Idx(p)]);

            /// <summary>The ground's slope in game units (the heights are stood up, so about 18 times the real slope).</summary>
            public float Slope(int i)
            {
                int x = i % n, z = i / n;
                int x0 = Mathf.Max(x - 1, 0), x1 = Mathf.Min(x + 1, n - 1), z0 = Mathf.Max(z - 1, 0), z1 = Mathf.Min(z + 1, n - 1);
                float dx = (h[z * n + x1] - h[z * n + x0]) / ((x1 - x0) * cell);
                float dz = (h[z1 * n + x] - h[z0 * n + x]) / ((z1 - z0) * cell);
                return Mathf.Sqrt(dx * dx + dz * dz);
            }

            public float Slope(Vector2 p) => Slope(Idx(p));

            public bool InTown(Vector2 p, float margin, Town except = null)
            {
                foreach (var t in towns)
                    if (t != except && Vector2.Distance(p, t.centre) < t.radius + margin)
                        return true;
                return false;
            }

            public bool NearMark(string prefix, Vector2 p, float within)
            {
                foreach (var kv in marks)
                    if (kv.Key.StartsWith(prefix) && Vector2.Distance(kv.Value, p) < within)
                        return true;
                return false;
            }
        }

        [MenuItem("Kapman/Levels/Build the southern North Sea")]
        public static void Build()
        {
            ProjectSetup.Apply();
            var L = Load();
            float size = L.size, scale = L.scale;
            foreach (var sub in new[] { "", "Ground", "Materials", "Meshes", "Courses" })
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), LevelDir, sub));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var levelGo = new GameObject("Level: the southern North Sea c. 793 (shortened 1:" + scale.ToString("0") + ")");
            var info = levelGo.AddComponent<LevelInfo>();
            info.levelId = "northsea_south";
            info.displayName = "The southern North Sea";
            info.kind = LevelKind.Ocean;
            info.latitude = L.lat;
            info.longitude = L.lon;
            info.baseWaterLevel = 0f;
            info.WaterLevel = 0f;
            info.realMetresPerUnit = scale;
            info.shipSpeed = 2.8f;
            var clock = levelGo.AddComponent<LevelClock>();
            clock.startDay = 172;
            clock.startHour = 17.5f;
            var weather = levelGo.AddComponent<LevelWeather>();
            weather.kind = WeatherKind.Haze;
            weather.manual = false;    // the weather engine: the sea's real climate, fronts and lows (G sets a kind by hand)
            weather.mistHeight = 3f;
            var water = levelGo.AddComponent<LevelWater>();
            water.tideLatitude = info.latitude;
            water.tideLongitude = info.longitude;
            water.tideShare = SeaV;
            water.tideLagHours = 0f;
            water.floodRise = 0.6f;
            var waves = levelGo.AddComponent<WaterWaves>();
            waves.waves = new[]
            {
                // the sine set the ships ride when there is no HDRP surface: one long swell with the wind sea nearly
                // along it, then two short ripples. The old set had two big waves crossing at a wide angle: from 700 m up
                // their crests made a grid of dark dots (the "halftone" in the 3 October shots)
                new Vector4(1f, 0.3f, 0.35f, 46f),
                new Vector4(0.9f, 0.1f, 0.16f, 23f),
                new Vector4(0.55f, 0.8f, 0.07f, 11f),
                new Vector4(0.9f, 0.6f, 0.035f, 4.5f),
            };

            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            var moon = new GameObject("Moon", typeof(Light)).GetComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.None;
            clock.sun = sun;
            clock.moon = moon;

            var world = new GameObject("World").transform;
            PlanTowns(L);
            var terrain = BuildTerrain(L, world);
            // the sea: HDRP's infinite ocean at the level's zero, its swell and whitecaps from the weather (HdrpSea),
            // calm shallows and surf along the coast from the terrain (ShoreWater)
            var sea = HdrpWater.MakeSea(world, 0f, size, LevelDir);
            water.surfaces = new[] { sea.transform };
            Plant(L, terrain, out var leaves, out var leafColours, out var leafCutoffs);
            weather.leaves = leaves;
            weather.leafColours = leafColours;
            weather.leafCutoffs = leafCutoffs;
            var kit = MakeKit();
            BuildTowns(L, kit, world);
            Weirs(L, kit, world);
            Birds(L, world);

            // the player's ship off the Rhine's mouth at Katwijk, and the ways in and out
            Vector2 rhine = L.places.TryGetValue("The Rhine mouth at Katwijk", out var rm) ? rm : new Vector2(size * 0.8f, size * 0.7f);
            Vector2 off = Offshore(rhine, L.H, 130f);
            float outYaw = Mathf.Atan2(off.x - rhine.x, off.y - rhine.y) * Mathf.Rad2Deg;
            var shipPrefab = AssetLibrary.Gislinge("Assets/Kapman/Prefabs/Ships/Ship_Gislinge.prefab");
            var ship = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab, world);
            ship.name = "Your ship: the Gislinge";
            ship.transform.SetPositionAndRotation(new Vector3(off.x, 0f, off.y), Quaternion.Euler(0f, outYaw, 0f));
            var sm = ship.AddComponent<ShipMotion>();
            sm.player = true;
            sm.heading = outYaw;
            HdrpWater.DressShip(ship, true);
            var ways = new GameObject("Ways in and out").transform;
            Vector2 mouthIn = Vector2.Lerp(rhine, off, 0.35f);
            DorestadBuilder.ExitAndEntry(ways, "Sail up the Rhine to Dorestad", "L_Dorestad", "from_sea", new Vector3(mouthIn.x, 0f, mouthIn.y),
                                         "rhine_mouth", new Vector3(off.x, 0f, off.y), outYaw, 60f);
            if (L.places.TryGetValue("The Thames mouth", out var thames))
            {
                Vector2 t = Offshore(thames, L.H, 60f);
                DorestadBuilder.ExitAndEntry(ways, "Sail up the Thames to Lundenwic", "L_Lundenwic", "from_sea", new Vector3(t.x, 0f, t.y),
                                             "thames_mouth", new Vector3(t.x, 0f, t.y), 90f, 60f);
            }
            if (L.places.TryGetValue("Walichrum", out var wal))
            {
                Vector2 t = Offshore(wal, L.H, 50f);
                DorestadBuilder.ExitAndEntry(ways, "Put in at Walichrum", "L_Walichrum", "from_sea", new Vector3(t.x, 0f, t.y),
                                             "walichrum", new Vector3(t.x, 0f, t.y), 270f, 50f);
            }
            Traders(L, shipPrefab, world);

            // the sky volume, the camera, the sounds (the look is HDRP's own now; the user said no filters till it is settled)
            var vol = new GameObject("Sky and post-processing volume", typeof(Volume)).GetComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProjectSetup.VolumePath);
            var skyGo = new GameObject("Sky");
            var dome = skyGo.AddComponent<SkyDome>();
            dome.volume = vol;
            dome.stars = AssetDatabase.LoadAssetAtPath<Cubemap>(ProjectSetup.StarsPath);

            var camGo = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 9000f;
            cam.fieldOfView = 50f;
            var camData = camGo.AddComponent<HDAdditionalCameraData>();
            camData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            var orbit = camGo.AddComponent<OrbitCamera>();
            orbit.target = ship.transform;
            orbit.yaw = outYaw + 150f;
            orbit.pitch = 10f;
            orbit.distance = 30f;
            orbit.maxDistance = 1200f;
            camGo.transform.position = ship.transform.position + Quaternion.Euler(10f, outYaw + 150f, 0f) * Vector3.back * 30f;
            camGo.transform.LookAt(ship.transform.position);
            weather.rain = DorestadBuilder.Precipitation(camGo.transform, "Rain", false, LevelDir);
            weather.snow = DorestadBuilder.Precipitation(camGo.transform, "Snow", true, LevelDir);
            var hud = new GameObject("Test panel").AddComponent<DevHud>();
            hud.water = water;
            var sounds = DorestadBuilder.Sounds();
            sounds.waterBirds = new[] { DorestadBuilder.Clip("black_headed_gull_554806"), DorestadBuilder.Clip("geese_407490") };
            sounds.dayBirds = sounds.waterBirds;

            Views(L, ship.transform, rhine, size);

            // the game itself (ported from the first build, 4 Oct)
            GameSetup.AddToScene(false);
            WaterProfileSetup.AddToOpenScene("northsea_south");
            clock.Apply();
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman North Sea: built {ScenePath} ({size:0} m across = {size * scale / 1000f:0} km real): {L.towns.Count} places and sea marks, " +
                      $"{L.towns.Sum(t => t.buildings.Count)} buildings.");
        }

        // ------------------------------------------------------------------ the data

        static Level Load()
        {
            var meta = Json.Parse(File.ReadAllText(Path.Combine(DataDir, "meta.json")));
            var L = new Level { n = (int)Json.Num(meta, "n"), size = (float)Json.Num(meta, "size"), scale = (float)Json.Num(meta, "scale") };
            L.cell = L.size / (L.n - 1);
            var centre = Json.Arr(meta, "center_latlon");
            L.lat = System.Convert.ToDouble(centre[0]);
            L.lon = System.Convert.ToDouble(centre[1]);
            void Points(string key, Dictionary<string, Vector2> into)
            {
                var obj = Json.Obj(meta, key);
                if (obj == null)
                    return;
                foreach (var kv in obj)
                    if (kv.Value is List<object> p && p.Count >= 2)
                        into[kv.Key] = new Vector2(System.Convert.ToSingle(p[0]), System.Convert.ToSingle(p[1]));
            }
            Points("places", L.places);
            Points("marks", L.marks);
            var woods = Json.Obj(meta, "woods");
            if (woods != null)
                foreach (var kv in woods)
                    if (kv.Value is List<object> p && p.Count >= 4)
                        L.woods.Add(new Vector4(System.Convert.ToSingle(p[0]), System.Convert.ToSingle(p[1]), System.Convert.ToSingle(p[2]), System.Convert.ToSingle(p[3])));
            if (L.marks.Count == 0)
                Debug.LogWarning("Kapman North Sea: meta.json has no marks (run Bake/levels/make_ocean.py again): no sea marks, chalk coasts or weirs.");
            var raw = File.ReadAllBytes(Path.Combine(DataDir, "height.f32"));
            L.real = new float[L.n * L.n];
            System.Buffer.BlockCopy(raw, 0, L.real, 0, raw.Length);
            L.surface = File.ReadAllBytes(Path.Combine(DataDir, "surface.u8"));
            L.h = L.real.Select(v => v > 0f ? v * LandV : v * SeaV).ToArray();
            L.dSea = Chamfer(L.n, i => L.real[i] < 0f);
            L.dLand = Chamfer(L.n, i => L.real[i] >= 0f);
            L.dRiver = Chamfer(L.n, i => L.surface[i] == InlandWater || L.surface[i] == River);
            return L;
        }

        /// <summary>The distance (cells, near enough to straight-line) from every cell to the nearest seed cell: two sweeps.</summary>
        static float[] Chamfer(int n, System.Func<int, bool> seed)
        {
            const float Far = 1e8f, Diag = 1.4142f;
            var d = new float[n * n];
            for (int i = 0; i < d.Length; i++)
                d[i] = seed(i) ? 0f : Far;
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    int i = z * n + x;
                    float best = d[i];
                    if (best == 0f)
                        continue;
                    if (x > 0) best = Mathf.Min(best, d[i - 1] + 1f);
                    if (z > 0)
                    {
                        best = Mathf.Min(best, d[i - n] + 1f);
                        if (x > 0) best = Mathf.Min(best, d[i - n - 1] + Diag);
                        if (x < n - 1) best = Mathf.Min(best, d[i - n + 1] + Diag);
                    }
                    d[i] = best;
                }
            for (int z = n - 1; z >= 0; z--)
                for (int x = n - 1; x >= 0; x--)
                {
                    int i = z * n + x;
                    float best = d[i];
                    if (best == 0f)
                        continue;
                    if (x < n - 1) best = Mathf.Min(best, d[i + 1] + 1f);
                    if (z < n - 1)
                    {
                        best = Mathf.Min(best, d[i + n] + 1f);
                        if (x < n - 1) best = Mathf.Min(best, d[i + n + 1] + Diag);
                        if (x > 0) best = Mathf.Min(best, d[i + n - 1] + Diag);
                    }
                    d[i] = best;
                }
            return d;
        }

        static float Fbm(float x, float z)
        {
            return 0.55f * Mathf.PerlinNoise(x / 300f + 11f, z / 300f + 3f)
                 + 0.30f * Mathf.PerlinNoise(x / 70f + 5f, z / 70f + 7f)
                 + 0.15f * Mathf.PerlinNoise(x / 15f + 2f, z / 15f + 9f);
        }

        /// <summary>
        /// How thick the woods stand at a point (0..1): by the ground's kind (the old dunes carried oak and birch woods, the
        /// hills were wooded, the lowland had copses), the great woods of meta.json (the Weald, the Boulonnais), and
        /// clumping noise. None on the marsh, the flats, the bars, the sea faces or the strand.
        /// </summary>
        static float Wood(Level L, int i, Vector2 p)
        {
            float r = L.real[i];
            if (r < 0.5f || L.dSea[i] < 1.5f)
                return 0f;
            float baseD;
            switch (L.surface[i])
            {
                case Dunes: baseD = L.dSea[i] > 2.5f ? 0.55f : 0.1f; break;
                case DuneValley: baseD = 0.25f; break;
                case Peat: baseD = 0.12f; break;
                case RiverReeds: case BrookValley: baseD = 0.3f; break;
                case RiverDune: baseD = 0.35f; break;
                case SandUpland: case Loess: case OlderGround: baseD = 0.4f; break;
                case Moraine: baseD = 0.65f; break;
                case Lowland: baseD = 0.14f; break;
                case Hills: baseD = 0.4f; break;
                case Uplands: baseD = 0.3f; break;
                default: return 0f;
            }
            foreach (var wd in L.woods)
            {
                float ex = (p.x - wd.x) / wd.z, ez = (p.y - wd.y) / wd.w;
                float e = ex * ex + ez * ez;
                if (e < 1f)
                    baseD = Mathf.Max(baseD, Mathf.Lerp(0.85f, 0.35f, e));
            }
            float clump = 0.6f * Mathf.PerlinNoise(p.x / 170f + 31f, p.y / 170f + 5f) + 0.4f * Mathf.PerlinNoise(p.x / 45f + 7f, p.y / 45f + 19f);
            return Mathf.Clamp01(baseD * (0.3f + 1.4f * clump));
        }

        // ------------------------------------------------------------------ the places of 793

        /// <summary>
        /// THE PLACES OF 793 at the real spots (meta.json), each from what is known of it, and the sea marks. A place is a
        /// cluster of real-size blockout houses on level ground by the water: the land is shortened 1:150 and the houses
        /// are not, as the ships are not (the design's vibe, not a map). The ground under every building is padded flat
        /// before the terrain is made, and raised to 0.45 m (about 4 m real) so the tide never reaches a floor.
        /// </summary>
        static void PlanTowns(Level L)
        {
            Vector2 P(string name) => L.places.TryGetValue(name, out var p) ? p : new Vector2(-1f, -1f);
            Vector2 M(string name) => L.marks.TryGetValue(name, out var p) ? p : new Vector2(-1f, -1f);

            // Londinium: the Roman walled city, mostly empty in 793 but for St Paul's (604) and a few folk (sure). The
            // wall ring here is a token 90 m across; the real wall was 3 km round.
            var lond = Place(L, M("Londinium"), "Londinium: the old walled city with St Paul's", 8, true, 3.5f);
            if (lond != null)
                Ring(L, lond, 45f, 4.5f, 12);
            // Lundenwic: the strand market along the Thames west of the old city, about 60 ha of houses c. 600-886 (sure).
            // It is set 115 m (17 km real) west so the two don't overlap: real-size houses on a 1:150 land (a liberty)
            Place(L, P("Lundenwic") + new Vector2(-115f, 0f), "Lundenwic: the strand market", 36, false, 3.5f);
            // Dover: the port in the Dour's gap with St Martin's minster (c. 640, sure) and the Roman pharos on the eastern
            // heights (sure: it still stands in the castle)
            var dover = Place(L, P("Dover"), "Dover: the port under the cliffs, with St Martin's minster", 18, true, 3.5f);
            if (dover != null)
                Tower(L, dover, Kind.Pharos, P("Dover"), 70f, 25f, "pharos");
            // Walichrum (Domburg): the beach market on the dunes, rich in finds (sure); no church, and the ring fort is a
            // century later
            Place(L, P("Walichrum"), "Walichrum: the beach market on the dunes", 24, false, 2f);
            // the Rhine mouth: a ferry hamlet by the Brittenburg, the Roman fort whose ruin lay in the dunes (the hamlet
            // is a guess; the ruin stood: it was still mapped in the 1500s)
            var kat = Place(L, P("The Rhine mouth at Katwijk"), "The Rhine mouth: a hamlet by the Brittenburg's ruin", 10, false, 2f);
            if (kat != null)
                Ruin(L, kat, 3);
            // Witla: the emporium at the Maas mouth, sacked by the Danes in 836 (sure it existed; its spot is a guess)
            Place(L, P("The Maas mouth"), "Witla: the market at the Maas mouth", 16, false, 2f);
            // Quentovic: the great emporium on the Canche with its mint, and Saint-Josse's abbey church nearby (Alcuin
            // was its abbot from 792; sure). The Canche is too narrow for the bake, so the town stands on the hill at
            // the real spot above the estuary.
            Place(L, P("Quentovic"), "Quentovic: the emporium on the Canche, with Saint-Josse's church", 36, true, 3.5f);
            // Boulogne: the Roman port under its lighthouse, the Tour d'Ordre (sure: it stood till 1644)
            var bou = Place(L, M("Boulogne"), "Boulogne: the old port under its lighthouse", 10, false, 3.5f);
            if (bou != null)
                Tower(L, bou, Kind.Lighthouse, M("Boulogne"), 60f, 15f, "lighthouse");
            // Reculver: the minster church of 669 inside the Roman fort on the Thames approaches (sure)
            var rec = Place(L, M("Reculver"), "Reculver: the minster in the Roman fort", 0, true, 2.5f);
            if (rec != null)
                Square(L, rec, 60f, 3f, 4);
            // Richborough: the Roman fort Rutupiae, its walls standing on the Wantsum channel (sure)
            var rich = Place(L, M("Richborough"), "Richborough: the Roman fort's walls", 0, false, 2.5f);
            if (rich != null)
                Square(L, rich, 90f, 6f, 5);
            // Bradwell: St Peter-on-the-Wall (654) in the Roman fort Othona on the Blackwater's mouth (sure)
            var brad = Place(L, M("Bradwell"), "Bradwell: St Peter's chapel on the wall", 0, false, 2.5f);
            if (brad != null)
            {
                brad.buildings.Add(new Building { kind = Kind.Chapel, at = brad.centre, yaw = 90f, width = 7f, length = 15f, wallHeight = 4.5f, ridge = 8f });
                Ruin(L, brad, 2);
            }
            Pads(L);
        }

        /// <summary>A place: its centre on level ground by the water near the spot, and its houses scattered round it.</summary>
        static Town Place(Level L, Vector2 near, string name, int houses, bool church, float minReal)
        {
            if (near.x < 0f)
                return null;
            if (!FindGround(L, near, 30, minReal, out Vector2 c))
            {
                Debug.LogWarning($"Kapman North Sea: no ground for {name} near {near}.");
                return null;
            }
            var t = new Town { name = name, centre = c, radius = houses > 0 ? 14f + 9f * Mathf.Sqrt(houses) : 26f };
            L.towns.Add(t);
            int made = 0;
            for (int tries = 0; tries < houses * 60 && made < houses; tries++)
            {
                float a = L.R(0f, Mathf.PI * 2f), rr = t.radius * Mathf.Sqrt(L.R());
                Vector2 p = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr;
                if (!Buildable(L, p, minReal, 1.0f) || Crowded(t, p, 10f) || L.InTown(p, 4f, t))
                    continue;
                // most houses gable-on to the water as at Dorestad; some any way
                float yaw = L.R() < 0.3f ? L.R(0f, 360f) : Facing(L, p) + L.R(-15f, 15f);
                float w = L.R(5f, 6.5f), len = L.R(11f, 16f), wall = L.R(1.8f, 2.2f);
                t.buildings.Add(new Building
                {
                    kind = Kind.House, at = p, yaw = yaw, width = w, length = len, wallHeight = wall,
                    ridge = wall + w / 2f * Mathf.Tan(48f * Mathf.Deg2Rad), walls = L.rnd.Next(3), thatch = L.rnd.Next(2),
                });
                made++;
            }
            if (church)
            {
                // the church stands apart, east-west with the chancel east
                for (int tries = 0; tries < 300; tries++)
                {
                    float a = L.R(0f, Mathf.PI * 2f);
                    Vector2 p = houses > 0 ? c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * t.radius * L.R(0.3f, 0.8f) : c;
                    if (!Buildable(L, p, Mathf.Min(minReal, 2.5f), 0.9f) || Crowded(t, p, 16f))
                    {
                        if (houses == 0 && tries > 0)
                            break;
                        continue;
                    }
                    t.buildings.Add(new Building { kind = Kind.Church, at = p, yaw = 90f + L.R(-8f, 8f), width = 7f, length = 16f, wallHeight = 3.6f, ridge = 7.5f });
                    break;
                }
                if (houses == 0 && !t.buildings.Any(b => b.kind == Kind.Church))
                    t.buildings.Add(new Building { kind = Kind.Church, at = c, yaw = 90f, width = 7f, length = 16f, wallHeight = 3.6f, ridge = 7.5f });
            }
            L.spots[name] = new Vector3(c.x, 0f, c.y);
            return t;
        }

        /// <summary>Level ground by the water within a window of the spot: the nearest cell that is high enough, not steep, and near water.</summary>
        static bool FindGround(Level L, Vector2 near, int window, float minReal, out Vector2 at)
        {
            int cx = Mathf.RoundToInt(near.x / L.cell), cz = Mathf.RoundToInt(near.y / L.cell);
            float bestScore = float.MaxValue;
            at = near;
            for (int z = Mathf.Max(1, cz - window); z <= Mathf.Min(L.n - 2, cz + window); z++)
                for (int x = Mathf.Max(1, cx - window); x <= Mathf.Min(L.n - 2, cx + window); x++)
                {
                    int i = z * L.n + x;
                    float r = L.real[i];
                    if (r < minReal || r > 90f)
                        continue;
                    float score = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                    if (L.Slope(i) > 1.0f) score += 40f;
                    if (Mathf.Min(L.dSea[i], L.dRiver[i]) > 3f) score += 25f;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        at = new Vector2(x * L.cell, z * L.cell);
                    }
                }
            return bestScore < float.MaxValue;
        }

        static bool Buildable(Level L, Vector2 p, float minReal, float maxSlope)
        {
            if (!L.Inside(p))
                return false;
            int i = L.Idx(p);
            return L.real[i] >= minReal && L.real[i] < 90f && L.Slope(i) < maxSlope && L.dSea[i] >= 0.9f;
        }

        static bool Crowded(Town t, Vector2 p, float dist) => t.buildings.Any(b => Vector2.Distance(b.at, p) < dist);

        /// <summary>The way to the nearest water from a point (degrees from north), for a house's gable.</summary>
        static float Facing(Level L, Vector2 p)
        {
            Vector2 best = Vector2.up;
            float least = float.MaxValue;
            for (int a = 0; a < 360; a += 22)
            {
                var d = new Vector2(Mathf.Sin(a * Mathf.Deg2Rad), Mathf.Cos(a * Mathf.Deg2Rad));
                Vector2 q = p + d * 16f;
                if (!L.Inside(q))
                    continue;
                float wd = L.Water(q);
                if (wd < least)
                {
                    least = wd;
                    best = d;
                }
            }
            return Mathf.Atan2(best.x, best.y) * Mathf.Rad2Deg;
        }

        /// <summary>A tower on the highest ground within reach of a spot (the pharos on Dover's eastern heights, Boulogne's lighthouse on its cliff).</summary>
        static void Tower(Level L, Town t, Kind kind, Vector2 near, float within, float minReal, string spot)
        {
            int cx = Mathf.RoundToInt(near.x / L.cell), cz = Mathf.RoundToInt(near.y / L.cell), w = Mathf.CeilToInt(within / L.cell);
            float best = -1f;
            Vector2 at = near;
            for (int z = Mathf.Max(1, cz - w); z <= Mathf.Min(L.n - 2, cz + w); z++)
                for (int x = Mathf.Max(1, cx - w); x <= Mathf.Min(L.n - 2, cx + w); x++)
                {
                    int i = z * L.n + x;
                    var p = new Vector2(x * L.cell, z * L.cell);
                    if (Vector2.Distance(p, near) > within || L.real[i] < minReal || L.Slope(i) > 2.5f || L.dSea[i] < 1.5f)
                        continue;
                    // the highest, and the nearer the sea the better: a mark is for ships
                    float score = L.real[i] - L.dSea[i] * 2f;
                    if (score > best)
                    {
                        best = score;
                        at = p;
                    }
                }
            if (best < 0f)
            {
                Debug.LogWarning($"Kapman North Sea: no high ground for the {kind} at {t.name}.");
                return;
            }
            t.buildings.Add(new Building { kind = kind, at = at, yaw = 0f, width = kind == Kind.Lighthouse ? 12f : 7.2f, length = kind == Kind.Lighthouse ? 12f : 7.2f });
            L.spots[spot] = new Vector3(at.x, 0f, at.y);
            if (Vector2.Distance(at, t.centre) > t.radius)
                t.radius = Vector2.Distance(at, t.centre) + 8f;
        }

        /// <summary>A ring of wall round a place's centre, in straight pieces; pieces over water or marsh are left out (the river took them).</summary>
        static void Ring(Level L, Town t, float radius, float height, int pieces)
        {
            float len = 2f * Mathf.PI * radius / pieces + 0.6f;
            for (int k = 0; k < pieces; k++)
            {
                float a = (k + 0.5f) / pieces * Mathf.PI * 2f;
                Vector2 c = t.centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                if (!Buildable(L, c, 1.5f, 1.2f))
                    continue;
                Vector2 tangent = new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
                t.buildings.Add(new Building { kind = Kind.Wall, at = c, yaw = Mathf.Atan2(tangent.x, tangent.y) * Mathf.Rad2Deg, width = 2.4f, length = len, wallHeight = height });
            }
            t.radius = Mathf.Max(t.radius, radius + 6f);
        }

        /// <summary>A square fort's walls (the Roman shore forts), in pieces per side.</summary>
        static void Square(Level L, Town t, float side, float height, int piecesPerSide)
        {
            float half = side / 2f, len = side / piecesPerSide + 0.6f;
            Vector2[] dirs = { Vector2.right, Vector2.up, Vector2.left, Vector2.down };
            for (int s = 0; s < 4; s++)
            {
                Vector2 along = dirs[s], outward = dirs[(s + 3) % 4];
                for (int k = 0; k < piecesPerSide; k++)
                {
                    Vector2 c = t.centre + outward * half + along * ((k + 0.5f) / piecesPerSide * side - half);
                    if (!Buildable(L, c, 1.5f, 1.2f))
                        continue;
                    t.buildings.Add(new Building { kind = Kind.Wall, at = c, yaw = Mathf.Atan2(along.x, along.y) * Mathf.Rad2Deg, width = 2.6f, length = len, wallHeight = height });
                }
            }
            t.radius = Mathf.Max(t.radius, half * 1.42f + 6f);
        }

        /// <summary>Broken pieces of Roman wall lying about a place.</summary>
        static void Ruin(Level L, Town t, int pieces)
        {
            for (int k = 0, tries = 0; k < pieces && tries < 100; tries++)
            {
                float a = L.R(0f, Mathf.PI * 2f);
                Vector2 p = t.centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * L.R(8f, t.radius * 0.7f);
                if (!Buildable(L, p, 1.5f, 1.2f) || Crowded(t, p, 9f))
                    continue;
                t.buildings.Add(new Building { kind = Kind.Wall, at = p, yaw = L.R(0f, 360f), width = 2.2f, length = L.R(6f, 14f), wallHeight = L.R(1.5f, 3.5f) });
                k++;
            }
        }

        /// <summary>Pads the ground flat under every building (and never below 0.45 m, about 4 m real: above the tide and the floods' first stage).</summary>
        static void Pads(Level L)
        {
            foreach (var t in L.towns)
                for (int b = 0; b < t.buildings.Count; b++)
                {
                    var bd = t.buildings[b];
                    float pad = Mathf.Max(bd.width, bd.length) / 2f + 1.5f;
                    float hc = Mathf.Max(L.H(bd.at), 0.45f);
                    int cx = Mathf.RoundToInt(bd.at.x / L.cell), cz = Mathf.RoundToInt(bd.at.y / L.cell), w = Mathf.CeilToInt((pad + 2.5f) / L.cell);
                    for (int z = Mathf.Max(0, cz - w); z <= Mathf.Min(L.n - 1, cz + w); z++)
                        for (int x = Mathf.Max(0, cx - w); x <= Mathf.Min(L.n - 1, cx + w); x++)
                        {
                            int i = z * L.n + x;
                            if (L.real[i] <= 0f)
                                continue;
                            float d = Vector2.Distance(new Vector2(x * L.cell, z * L.cell), bd.at);
                            if (d <= pad)
                                L.h[i] = hc;
                            else if (d < pad + 2.5f)
                                L.h[i] = Mathf.Lerp(hc, L.h[i], (d - pad) / 2.5f);
                        }
                    bd.y = hc;
                    t.buildings[b] = bd;
                }
        }

        static Kit MakeKit()
        {
            Material M(string source, string slug, string name, float smooth, Color? tint = null) =>
                AssetLibrary.Lit(source, slug, $"{LevelDir}/Materials/{name}.mat", smooth, tint);
            return new Kit
            {
                walls = new[]
                {
                    M("polyhaven", "clay_plaster", "Daub_pale", 0.08f, new Color(0.98f, 0.95f, 0.86f)),
                    M("polyhaven", "clay_plaster", "Daub_ochre", 0.08f, new Color(0.86f, 0.74f, 0.55f)),
                    M("ambientcg", "Wicker002", "Wattle_wall", 0.1f, new Color(0.9f, 0.85f, 0.72f)),
                },
                thatch = new[]
                {
                    M("polyhaven", "reed_roof_03", "Thatch_gold", 0.05f, new Color(1f, 0.92f, 0.7f)),
                    M("polyhaven", "thatch_roof_angled", "Thatch_grey", 0.05f, new Color(0.72f, 0.72f, 0.66f)),
                },
                stone = M("polyhaven", "gray_rocks", "Roman_stone", 0.05f, new Color(0.82f, 0.78f, 0.70f)),
                piles = M("polyhaven", "bark_willow", "Piles", 0.08f),
                wattle = M("ambientcg", "Wicker002", "Wattle", 0.1f),
            };
        }

        /// <summary>Each place as one mesh (a submesh per material) so the whole coast's towns cost a dozen draws.</summary>
        static void BuildTowns(Level L, Kit kit, Transform parent)
        {
            var root = new GameObject("The coast's places and sea marks (blockouts to replace)").transform;
            root.SetParent(parent, false);
            var scratch = new List<Mesh>();
            foreach (var t in L.towns)
            {
                if (t.buildings.Count == 0)
                    continue;
                var groups = new Dictionary<Material, List<CombineInstance>>();
                var order = new List<Material>();
                void Add(Mesh mesh, int sub, Material mat, Matrix4x4 m)
                {
                    if (!groups.TryGetValue(mat, out var list))
                    {
                        groups[mat] = list = new List<CombineInstance>();
                        order.Add(mat);
                    }
                    list.Add(new CombineInstance { mesh = mesh, subMeshIndex = sub, transform = m });
                }
                int k = 0;
                foreach (var b in t.buildings)
                {
                    var rot = Quaternion.Euler(0f, b.yaw, 0f);
                    Matrix4x4 M(Vector3 offset) => Matrix4x4.TRS(new Vector3(b.at.x, b.y, b.at.y) + rot * offset, rot, Vector3.one);
                    string nm = $"b{k++}";
                    switch (b.kind)
                    {
                        case Kind.House:
                        {
                            var mesh = MeshKit.House(nm, b.width, b.length, b.wallHeight, b.ridge, 0.5f, 2f, 2.5f);
                            scratch.Add(mesh);
                            Add(mesh, 0, kit.walls[b.walls], M(Vector3.zero));
                            Add(mesh, 1, kit.thatch[b.thatch], M(Vector3.zero));
                            break;
                        }
                        case Kind.Church:
                        {
                            // a stone nave, a lower chancel east and a square tower west: the plan of the Kentish churches
                            // of the 600s (Reculver, St Martin's); the thatch is a guess (they may have been tiled or shingled)
                            var nave = MeshKit.House(nm, b.width, b.length, b.wallHeight, b.ridge, 0.4f, 2f, 2.5f);
                            var chancel = MeshKit.House(nm + "c", b.width * 0.65f, 5f, b.wallHeight * 0.8f, b.ridge * 0.75f, 0.4f, 2f, 2.5f);
                            var tower = MeshKit.Box(nm + "t", new Vector3(4.5f, b.ridge + 5f, 4.5f), 2f);
                            scratch.AddRange(new[] { nave, chancel, tower });
                            Add(nave, 0, kit.stone, M(Vector3.zero));
                            Add(nave, 1, kit.thatch[1], M(Vector3.zero));
                            Add(chancel, 0, kit.stone, M(new Vector3(0f, 0f, b.length / 2f + 2.3f)));
                            Add(chancel, 1, kit.thatch[1], M(new Vector3(0f, 0f, b.length / 2f + 2.3f)));
                            Add(tower, 0, kit.stone, M(new Vector3(0f, -0.5f, -b.length / 2f - 2.2f)));
                            break;
                        }
                        case Kind.Chapel:
                        {
                            var nave = MeshKit.House(nm, b.width, b.length, b.wallHeight, b.ridge, 0.4f, 2f, 2.5f);
                            scratch.Add(nave);
                            Add(nave, 0, kit.stone, M(Vector3.zero));
                            Add(nave, 1, kit.thatch[1], M(Vector3.zero));
                            break;
                        }
                        case Kind.Pharos:
                        {
                            // Dover's Roman lighthouse: an octagonal tower in diminishing stages, about 24 m when whole
                            var lower = MeshKit.Cylinder(nm, 3.6f, 12f, 8, 2f);
                            var upper = MeshKit.Cylinder(nm + "u", 3.0f, 8f, 8, 2f);
                            scratch.AddRange(new[] { lower, upper });
                            Add(lower, 0, kit.stone, M(new Vector3(0f, -0.6f, 0f)));
                            Add(lower, 1, kit.stone, M(new Vector3(0f, -0.6f, 0f)));
                            Add(upper, 0, kit.stone, M(new Vector3(0f, 11.4f, 0f)));
                            Add(upper, 1, kit.stone, M(new Vector3(0f, 11.4f, 0f)));
                            break;
                        }
                        case Kind.Lighthouse:
                        {
                            // Boulogne's Tour d'Ordre: twelve-sided, in stepped stages (it had twelve; three stand for it here)
                            float[] rad = { 6f, 5f, 4f }, hgt = { 10f, 9f, 8f };
                            float y = -0.6f;
                            for (int s = 0; s < 3; s++)
                            {
                                var stage = MeshKit.Cylinder(nm + s, rad[s], hgt[s], 12, 2f);
                                scratch.Add(stage);
                                Add(stage, 0, kit.stone, M(new Vector3(0f, y, 0f)));
                                Add(stage, 1, kit.stone, M(new Vector3(0f, y, 0f)));
                                y += hgt[s];
                            }
                            break;
                        }
                        case Kind.Wall:
                        {
                            var wall = MeshKit.Box(nm, new Vector3(b.width, b.wallHeight + 0.8f, b.length), 2f);
                            scratch.Add(wall);
                            Add(wall, 0, kit.stone, M(new Vector3(0f, -0.8f, 0f)));
                            break;
                        }
                    }
                }
                var parts = new List<Mesh>();
                foreach (var mat in order)
                {
                    var m = new Mesh { indexFormat = IndexFormat.UInt32 };
                    m.CombineMeshes(groups[mat].ToArray(), true, true);
                    parts.Add(m);
                }
                var combined = new Mesh { name = "Place_" + Safe(t.name), indexFormat = IndexFormat.UInt32 };
                combined.CombineMeshes(parts.Select(p => new CombineInstance { mesh = p, transform = Matrix4x4.identity }).ToArray(), false, false);
                combined.RecalculateBounds();
                foreach (var p in parts)
                    Object.DestroyImmediate(p);
                SaveMesh(combined, combined.name);
                var go = new GameObject(t.name, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(root, false);
                go.GetComponent<MeshFilter>().sharedMesh = combined;
                go.GetComponent<MeshRenderer>().sharedMaterials = order.ToArray();
                go.isStatic = true;
            }
            foreach (var m in scratch)
                Object.DestroyImmediate(m);
            Debug.Log("Kapman North Sea: " + string.Join("; ", L.towns.Select(t => $"{t.name} ({t.buildings.Count})")));
        }

        static string Safe(string name) => new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

        static Mesh SaveMesh(Mesh m, string name)
        {
            string path = $"{LevelDir}/Meshes/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // ------------------------------------------------------------------ the terrain

        static TerrainLayer Layer(string source, string slug, float tile, string file, Color? tint)
        {
            string path = LevelDir + "/Ground/" + file + ".terrainlayer";
            var layer = AssetLibrary.Layer(source, slug, tile, path);
            // the layer may be an old one at this path: bring it up to today's texture, tile and tint
            var (d, nm) = AssetLibrary.Surface(source, slug);
            layer.diffuseTexture = d;
            layer.normalMapTexture = nm;
            layer.tileSize = new Vector2(tile, tile);
            layer.diffuseRemapMin = Vector4.zero;
            layer.diffuseRemapMax = tint.HasValue ? new Vector4(tint.Value.r, tint.Value.g, tint.Value.b, 1f) : Vector4.one;
            EditorUtility.SetDirty(layer);
            return layer;
        }

        static Terrain BuildTerrain(Level L, Transform parent)
        {
            int n = L.n;
            float size = L.size, cell = L.cell;
            var h = L.h;
            float lo = h.Min() - 1f, hi = h.Max() + 1f;
            var td = new TerrainData { heightmapResolution = n };
            td.size = new Vector3(size, hi - lo, size);
            var hts = new float[n, n];
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                    hts[z, x] = (h[z * n + x] - lo) / (hi - lo);
            td.SetHeights(0, 0, hts);
            var layers = new[]
            {
                Layer("polyhaven", "sand_01", 6f, "0_sea_bed", new Color(0.38f, 0.42f, 0.40f)),        // greened: not the dunes' beige
                Layer("polyhaven", "damp_sand", 5f, "1_tidal_flats", new Color(0.80f, 0.78f, 0.72f)),
                Layer("polyhaven", "coast_sand_01", 5f, "2_beach_dunes", null),
                Layer("ambientcg", "Grass001", 4f, "3_grass", null),
                Layer("polyhaven", "withered_grass", 4f, "4_salt_marsh", new Color(0.62f, 0.64f, 0.46f)),
                Layer("ambientcg", "Grass004", 4f, "5_upland", new Color(0.80f, 0.84f, 0.70f)),
                Layer("polyhaven", "brown_mud_02", 4f, "6_river_mud", null),
                Layer("polyhaven", "marble_cliff_01", 6f, "7_chalk", null),                            // the white cliffs
            };
            td.terrainLayers = layers;
            td.alphamapResolution = AlphaRes;
            td.baseMapResolution = 2048;
            var a = new float[AlphaRes, AlphaRes, LayerCount];
            var w = new float[LayerCount];
            for (int z = 0; z < AlphaRes; z++)
                for (int x = 0; x < AlphaRes; x++)
                {
                    int gx = Mathf.Clamp((int)((x + 0.5f) / AlphaRes * (n - 1)), 0, n - 1), gz = Mathf.Clamp((int)((z + 0.5f) / AlphaRes * (n - 1)), 0, n - 1);
                    int i = gz * n + gx;
                    var p = new Vector2(gx * cell, gz * cell);
                    float r = L.real[i];
                    byte s = L.surface[i];
                    float ds = L.dSea[i], dr = L.dRiver[i];
                    float noise = Fbm(p.x, p.y), fine = Mathf.PerlinNoise(p.x / 9f + 40f, p.y / 9f + 40f);
                    float slope = L.Slope(i);
                    System.Array.Clear(w, 0, LayerCount);
                    if (r < 0f)
                    {
                        // under the sea's mean level: the flats within a metre or two of it, the bed below; mud where the
                        // rivers come out (within 2 cells of them) and in the lakes; the upper Wadden flats muddy too
                        float flats = Mathf.InverseLerp(-3f, -1f, r);
                        w[LFlats] = flats;
                        w[LSeaBed] = 1f - flats;
                        if (s == InlandWater || dr <= 2.5f)
                        {
                            float m = s == InlandWater ? 0.8f : Mathf.Clamp01(1f - dr / 2.5f) * (0.45f + 0.4f * flats);
                            w[LFlats] *= 1f - m;
                            w[LSeaBed] *= 1f - m;
                            w[LMud] += m;
                        }
                        if (s == Flats && r > -0.7f)
                            w[LMud] += 0.35f * (1f - fine);
                    }
                    else
                    {
                        // a sea face: the ground falls to the sea more than about 1 in 13 (real) from over 10 m up: chalk on
                        // the chalk coasts of meta.json, dark clay or sandstone elsewhere (Sheppey, Hastings, Gris-Nez:
                        // mud and upland grass stand for those, there being no ninth layer)
                        bool seaFace = ds <= 3.5f && r > 10f && slope > 1.4f;
                        bool inTown = !seaFace && L.InTown(p, 4f);
                        float wood = inTown ? 0f : Wood(L, i, p);
                        if (seaFace)
                        {
                            float c = Mathf.InverseLerp(1.4f, 3f, slope);
                            if (L.NearMark("Chalk", p, 60f))
                                w[LChalk] = c;
                            else
                            {
                                w[LMud] = c * 0.65f;
                                w[LUpland] = c * 0.35f;
                            }
                            w[LGrass] += (1f - c) * 0.7f;
                            w[LUpland] += (1f - c) * 0.3f;
                        }
                        else if (inTown)
                        {
                            // trodden ground between the houses
                            w[LGrass] = 0.5f + 0.3f * fine;
                            w[LMud] = 0.5f - 0.3f * fine;
                        }
                        else
                            switch (s)
                            {
                                case Marsh:
                                    w[LSaltMarsh] = 0.8f + 0.2f * noise;
                                    w[LMud] = 0.15f * (1f - noise) * Mathf.Clamp01(1f - r / 1.5f);
                                    break;
                                case MarshRidge:
                                    w[LSaltMarsh] = 0.65f;
                                    w[LGrass] = 0.35f + 0.2f * noise;
                                    break;
                                case Peat:
                                {
                                    // the bogs: the withered marsh grass darkened with the peat's own mud, in patches
                                    float pt = 0.3f + 0.35f * noise;
                                    w[LMud] = pt;
                                    w[LSaltMarsh] = 1f - pt;
                                    break;
                                }
                                case RiverReeds:
                                    w[LSaltMarsh] = 0.7f;
                                    w[LMud] = 0.3f;
                                    break;
                                case Bar:
                                    w[LBeach] = 0.6f + 0.3f * fine;
                                    w[LMud] = 0.4f - 0.3f * fine;
                                    break;
                                case CutBank:
                                    w[LMud] = 1f;
                                    break;
                                case Dunes:
                                    if (r < 0.4f && ds <= 2.5f)
                                        w[LBeach] = 1f;
                                    else
                                    {
                                        // bare sand on the sea side, marram and dune grass behind
                                        float grassy = Mathf.Clamp01((ds - 2f) / 4f) * (0.3f + 0.5f * noise);
                                        w[LBeach] = 1f - grassy;
                                        w[LUpland] = grassy;
                                    }
                                    break;
                                case DuneValley:
                                    w[LGrass] = 0.55f + 0.2f * noise;
                                    w[LBeach] = 0.45f - 0.2f * noise;
                                    break;
                                case Flats:
                                    // flats standing over the mean level: the top of the Wadden, marsh creeping on
                                    w[LFlats] = 0.6f;
                                    w[LSaltMarsh] = 0.4f;
                                    break;
                                case InlandWater:
                                case River:
                                    w[LMud] = 0.55f;
                                    w[LSaltMarsh] = 0.45f;
                                    break;
                                default:
                                    // the land beyond the Dutch map (England, Flanders, France) and the Dutch uplands: the
                                    // strand where it meets the sea, salt marsh where the lowland lies under 2.5 m within
                                    // 3 km of the sea (Essex, Kent, Flanders: a guess from the later maps and the names),
                                    // grass, the hills darker above 35 m
                                    if (r < 0.4f && ds <= 2.5f)
                                        w[LBeach] = 1f;
                                    else if (s == Lowland && r < 2.5f && ds <= 14f)
                                    {
                                        float m = Mathf.Clamp01((2.5f - r) / 2f) * Mathf.Clamp01((14f - ds) / 6f) * (0.5f + 0.6f * noise);
                                        m = Mathf.Clamp01(m);
                                        w[LSaltMarsh] = m;
                                        w[LGrass] = 1f - m;
                                    }
                                    else
                                    {
                                        float up = Mathf.Clamp01(Mathf.InverseLerp(35f, 150f, r) * (0.6f + 0.6f * noise));
                                        w[LGrass] = 1f - up;
                                        w[LUpland] = up;
                                    }
                                    break;
                            }
                        // under thick woods the ground goes to the darker upland grass with a little mud (the leaf floor
                        // would be a ninth layer)
                        if (wood > 0.5f && !seaFace)
                        {
                            float f = Mathf.InverseLerp(0.5f, 0.85f, wood) * 0.7f;
                            for (int k = 0; k < LayerCount; k++)
                                w[k] *= 1f - f;
                            w[LUpland] += f * 0.75f;
                            w[LMud] += f * 0.25f;
                        }
                        // the strand: every low shore gets a line of sand, or of mud at the river mouths and on the marsh
                        if (ds <= 2.2f && r < 3f && slope < 0.8f && !seaFace)
                        {
                            float sh = Mathf.Clamp01((2.2f - ds) / 1.2f) * 0.7f;
                            bool muddy = dr <= 3f || s == Flats || s == Marsh || s == RiverReeds;
                            for (int k = 0; k < LayerCount; k++)
                                w[k] *= 1f - sh;
                            w[muddy ? LMud : LBeach] += sh;
                        }
                    }
                    float sum = 0f;
                    for (int k = 0; k < LayerCount; k++)
                        sum += w[k];
                    for (int k = 0; k < LayerCount; k++)
                        a[z, x, k] = sum > 0f ? w[k] / sum : (k == LGrass ? 1f : 0f);
                }
            td.SetAlphamaps(0, 0, a);
            AssetDatabase.CreateAsset(td, LevelDir + "/NorthSea_TerrainData.asset");
            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "The coasts c. 793 (shortened; heights stood up)";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, lo, 0f);
            var t = go.GetComponent<Terrain>();
            t.heightmapPixelError = 2f;
            t.basemapDistance = 2500f;
            t.drawInstanced = true;
            go.isStatic = true;
            return t;
        }

        // ------------------------------------------------------------------ the trees

        /// <summary>
        /// Terrain trees where the woods stood: oak and birch on the old dunes, willows along the rivers and in the
        /// dune valleys, birch on the bogs' edges, the Weald and the wooded hills, copses on the lowland. Real-size trees
        /// on a 1:150 land, as the ships are: what the coast looks like from the deck, not a map. The four CC0 scans in
        /// four leaf tints, as at Dorestad (the same tinted prefabs, so the seasons colour them alike).
        /// </summary>
        static void Plant(Level L, Terrain terrain, out Material[] leafMaterials, out Color[] leafColours, out float[] leafCutoffs)
        {
            var td = terrain.terrainData;
            var protos = new List<TreePrototype>();
            var allLeaves = new List<Material>();
            var allColours = new List<Color>();
            var allCutoffs = new List<float>();
            string tintDir = "Assets/Kapman/Prefabs/Nature/TintsCoast";   // its own: Dorestad's builder writes Tints with other colours
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), tintDir));
            for (int k = 0; k < TreeKinds.Length; k++)
            {
                var basePrefab = AssetLibrary.Tree(TreeKinds[k], $"Assets/Kapman/Prefabs/Nature/{TreeKinds[k]}.prefab", out var baseLeaves);
                for (int t = 0; t < LeafTints.Length; t++)
                {
                    string matPath = $"{tintDir}/{TreeKinds[k]}_leaves_{t}.mat";
                    var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (m == null && baseLeaves != null)
                    {
                        m = new Material(baseLeaves);
                        AssetDatabase.CreateAsset(m, matPath);
                    }
                    if (m != null)
                    {
                        m.SetColor("_BaseColor", LeafTints[t]);
                        allLeaves.Add(m);
                        allColours.Add(LeafTints[t]);
                        allCutoffs.Add(m.HasProperty("_Cutoff") ? m.GetFloat("_Cutoff") : 0.45f);
                    }
                    string prefabPath = $"{tintDir}/{TreeKinds[k]}_t{t}.prefab";
                    var variant = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (variant == null)
                    {
                        var inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                        PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        if (m != null && baseLeaves != null)
                            foreach (var r in inst.GetComponentsInChildren<MeshRenderer>())
                            {
                                var ms = r.sharedMaterials;
                                bool any = false;
                                for (int j = 0; j < ms.Length; j++)
                                    if (ms[j] == baseLeaves) { ms[j] = m; any = true; }
                                if (any) r.sharedMaterials = ms;
                            }
                        variant = PrefabUtility.SaveAsPrefabAsset(inst, prefabPath);
                        Object.DestroyImmediate(inst);
                    }
                    protos.Add(new TreePrototype { prefab = variant, bendFactor = 0f });
                }
            }
            leafMaterials = allLeaves.ToArray();
            leafColours = allColours.ToArray();
            leafCutoffs = allCutoffs.ToArray();
            td.treePrototypes = protos.ToArray();

            var trees = new List<TreeInstance>();
            var counts = new Dictionary<string, int>();
            float size = td.size.x;
            void Put(int kind, string group, Vector2 p, float scaleLo, float scaleHi, int tint)
            {
                if (trees.Count >= MaxTrees)
                    return;
                float s = TreeScale[kind] * L.R(scaleLo, scaleHi);
                int ti = tint >= 0 ? tint : L.rnd.Next(3);
                trees.Add(new TreeInstance
                {
                    position = new Vector3(p.x / size, 0f, p.y / size),
                    prototypeIndex = kind * LeafTints.Length + ti,
                    widthScale = s * L.R(0.9f, 1.1f),
                    heightScale = s * L.R(0.88f, 1.15f),
                    rotation = L.R() * Mathf.PI * 2f,
                    color = Color.white,
                    lightmapColor = Color.white,
                });
                counts[group] = counts.TryGetValue(group, out int c) ? c + 1 : 1;
            }
            // the cells in a scrambled order, so the cap (if reached) thins the whole coast evenly
            const float Step = 8f;
            int across = Mathf.CeilToInt(L.size / Step);
            long cellCount = (long)across * across;
            for (long k = 0; k < cellCount; k++)
            {
                long idx = (k * 7919L) % cellCount;
                var p = new Vector2((idx % across) * Step + Step / 2f + L.R(-3.5f, 3.5f), (idx / across) * Step + Step / 2f + L.R(-3.5f, 3.5f));
                if (!L.Inside(p))
                    continue;
                int i = L.Idx(p);
                float wood = Wood(L, i, p);
                if (wood <= 0.03f || L.Slope(i) > 1.0f || L.InTown(p, 6f))
                    continue;
                if (L.R() >= wood * 0.6f)
                    continue;
                float roll = L.R();
                switch (L.surface[i])
                {
                    case Dunes:
                    case DuneValley:
                        if (roll < 0.5f) Put(Oak, "Dune woods: oak, birch and scrub", p, 0.8f, 1.15f, -1);
                        else if (roll < 0.75f) Put(Ash, "Dune woods: oak, birch and scrub", p, 0.75f, 1.0f, 3);
                        else Put(Osier, "Dune woods: oak, birch and scrub", p, 0.7f, 1.0f, -1);
                        break;
                    case Peat:
                        Put(Ash, "Birch on the bogs' edges", p, 0.7f, 1.0f, 3);
                        break;
                    case RiverReeds:
                    case BrookValley:
                        Put(roll < 0.6f ? Willow : Osier, "Willows along the rivers", p, 0.75f, 1.2f, -1);
                        break;
                    case Hills:
                    case Uplands:
                    case Moraine:
                    case SandUpland:
                    case Loess:
                    case OlderGround:
                        Put(roll < 0.55f ? Oak : Ash, "The wooded hills", p, 0.9f, 1.3f, -1);
                        break;
                    default:
                        Put(roll < 0.5f ? Oak : Ash, "Lowland woods and copses (the Weald among them)", p, 0.9f, 1.3f, -1);
                        break;
                }
            }
            td.SetTreeInstances(trees.ToArray(), true);
            terrain.treeDistance = 3000f;
            terrain.treeBillboardDistance = 600f;
            terrain.treeCrossFadeLength = 20f;
            terrain.treeMaximumFullLODCount = 300;
            terrain.treeLODBiasMultiplier = 1.5f;
            EditorUtility.SetDirty(td);
            Debug.Log("Kapman North Sea: " + trees.Count + " trees on the coasts: " + string.Join(", ", counts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}")));
        }

        // ------------------------------------------------------------------ the weirs

        /// <summary>
        /// Fish weirs on the flats: V-shaped fences of posts and wattle with the point and the trap toward the ebb's channel,
        /// as excavated on the Essex Blackwater (7th-10th c.: Collins Creek, Sales Point) and known from later medieval
        /// flats everywhere round this sea. Real size, on the fishing grounds meta.json marks; one mesh each.
        /// </summary>
        static void Weirs(Level L, Kit kit, Transform parent)
        {
            var root = new GameObject("Fish weirs on the flats").transform;
            root.SetParent(parent, false);
            var post = MeshKit.Cylinder("Weir_post", 0.07f, 2.2f, 6, 1f);
            var panel = MeshKit.Box("Weir_panel", new Vector3(1.4f, 1.0f, 0.06f), 1.2f);
            int made = 0;
            foreach (var kv in L.marks)
            {
                if (!kv.Key.StartsWith("Weirs"))
                    continue;
                var spots = new List<Vector2>();
                for (int tries = 0; tries < 500 && spots.Count < 3; tries++)
                {
                    Vector2 p = kv.Value + new Vector2(L.R(-90f, 90f), L.R(-90f, 90f));
                    if (!L.Inside(p))
                        continue;
                    int i = L.Idx(p);
                    float r = L.real[i];
                    // the middle of the intertidal zone, within 1.2 km (real) of the shore
                    if (r < -1.3f || r > -0.25f || L.dLand[i] > 5f || spots.Any(s => Vector2.Distance(s, p) < 26f))
                        continue;
                    spots.Add(p);
                }
                foreach (var p in spots)
                {
                    Weir(L, root, kit, post, panel, p, Seaward(L, p), L.R(16f, 30f), kv.Key.Substring(6).Trim(), made);
                    if (made == 0)
                        L.spots["weir"] = new Vector3(p.x, 0f, p.y);
                    made++;
                }
            }
            Object.DestroyImmediate(post);
            Object.DestroyImmediate(panel);
            Debug.Log($"Kapman North Sea: {made} fish weirs.");
        }

        /// <summary>The way the bed falls from a point: toward the ebb's channel.</summary>
        static Vector2 Seaward(Level L, Vector2 p)
        {
            Vector2 best = Vector2.up;
            float lowest = float.MaxValue;
            for (int a = 0; a < 360; a += 30)
            {
                var d = new Vector2(Mathf.Sin(a * Mathf.Deg2Rad), Mathf.Cos(a * Mathf.Deg2Rad));
                Vector2 q = p + d * 10f;
                if (!L.Inside(q))
                    continue;
                float r = L.Real(q);
                if (r < lowest)
                {
                    lowest = r;
                    best = d;
                }
            }
            return best;
        }

        static void Weir(Level L, Transform root, Kit kit, Mesh post, Mesh panel, Vector2 at, Vector2 seaward, float arm, string ground, int index)
        {
            var posts = new List<CombineInstance>();
            var panels = new List<CombineInstance>();
            Vector2 apex = at + seaward * arm * 0.45f;
            for (int side = -1; side <= 1; side += 2)
            {
                float ang = side * 28f * Mathf.Deg2Rad;
                Vector2 back = -seaward;
                var dir = new Vector2(back.x * Mathf.Cos(ang) - back.y * Mathf.Sin(ang), back.x * Mathf.Sin(ang) + back.y * Mathf.Cos(ang));
                for (float u = 0f; u < arm; u += 1.4f)
                {
                    Vector2 q = apex + dir * u;
                    if (!L.Inside(q))
                        break;
                    float bed = L.H(q);
                    if (bed > 0.3f)
                        break;                      // up onto the shore: the arm ends
                    float top = 0.6f + L.R(0f, 0.3f);
                    var rot = Quaternion.Euler(L.R(-4f, 4f), L.R(0f, 360f), L.R(-4f, 4f));
                    var m = Matrix4x4.TRS(new Vector3(q.x, bed - 0.3f, q.y), rot, new Vector3(1f, (top - (bed - 0.3f)) / 2.2f, 1f));
                    posts.Add(new CombineInstance { mesh = post, subMeshIndex = 0, transform = m });
                    posts.Add(new CombineInstance { mesh = post, subMeshIndex = 1, transform = m });
                    Vector2 mid = q + dir * 0.7f;
                    float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f;
                    panels.Add(new CombineInstance { mesh = panel, subMeshIndex = 0, transform = Matrix4x4.TRS(new Vector3(mid.x, bed - 0.1f, mid.y), Quaternion.Euler(0f, yaw, 0f), Vector3.one) });
                }
            }
            if (posts.Count == 0)
                return;
            var pm = new Mesh();
            pm.CombineMeshes(posts.ToArray(), true, true);
            var wm = new Mesh();
            wm.CombineMeshes(panels.ToArray(), true, true);
            var mesh = new Mesh { name = "Weir_" + index };
            mesh.CombineMeshes(new[] { new CombineInstance { mesh = pm, transform = Matrix4x4.identity }, new CombineInstance { mesh = wm, transform = Matrix4x4.identity } }, false, false);
            mesh.RecalculateBounds();
            Object.DestroyImmediate(pm);
            Object.DestroyImmediate(wm);
            SaveMesh(mesh, mesh.name);
            var go = new GameObject($"Fish weir on {ground}", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(root, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = new[] { kit.piles, kit.wattle };
            go.isStatic = true;
        }

        // ------------------------------------------------------------------ the birds

        /// <summary>Gulls over the ports, the cliffs and the river mouths; skeins of geese over the great flats and marshes.</summary>
        static void Birds(Level L, Transform parent)
        {
            var root = new GameObject("Birds (flocks on the clock)").transform;
            root.SetParent(parent, false);
            int gulls = 0, geese = 0;
            void Add(Flock.Kind kind, Vector2 at, int count, float radius, float height, float speed, float span, Color colour, string where)
            {
                var go = new GameObject($"{kind} ({count}) over {where}");
                go.transform.SetParent(root, false);
                var p = new Vector3(at.x, Mathf.Max(L.H(at), 0f), at.y);
                go.transform.position = p;
                var f = go.AddComponent<Flock>();
                f.kind = kind;
                f.count = count;
                f.radius = radius;
                f.height = height;
                f.speed = speed;
                f.wingspan = span;
                f.colour = colour;
                f.groundCentre = p;
            }
            var gull = new Color(0.85f, 0.85f, 0.85f);
            var goose = new Color(0.2f, 0.18f, 0.16f);
            foreach (var name in new[] { "Dover", "The Thames mouth", "Walichrum", "The Rhine mouth at Katwijk", "The Maas mouth", "Quentovic" })
                if (L.places.TryGetValue(name, out var p))
                {
                    Add(Flock.Kind.Gulls, p, 10 + L.rnd.Next(8), 160f, 30f, 10f, 1.3f, gull, name);
                    gulls++;
                }
            foreach (var kv in L.marks)
            {
                if (kv.Key == "Boulogne" || kv.Key == "Reculver" || kv.Key == "Bradwell")
                {
                    Add(Flock.Kind.Gulls, kv.Value, 8 + L.rnd.Next(8), 140f, 28f, 10f, 1.3f, gull, kv.Key);
                    gulls++;
                }
                if (kv.Key.StartsWith("Birds"))
                {
                    // greylags: the geese that breed in these fens and marshes; the brent and barnacle geese of the Wadden
                    // are winter birds and would be gone by June (a guess at which geese fly on a June evening)
                    string where = kv.Key.Substring(6).Trim();
                    int skeins = kv.Key.Contains("Wadden") ? 2 : 1;
                    for (int s = 0; s < skeins; s++)
                    {
                        Add(Flock.Kind.Geese, kv.Value + new Vector2(L.R(-120f, 120f), L.R(-120f, 120f)), 9 + L.rnd.Next(10), L.R(240f, 400f), L.R(40f, 80f), 13f, 1.6f, goose, where);
                        geese++;
                    }
                    Add(Flock.Kind.Gulls, kv.Value, 12, 220f, 25f, 10f, 1.3f, gull, where);
                    gulls++;
                }
            }
            Debug.Log($"Kapman North Sea birds: {gulls} gull flocks, {geese} geese skeins.");
        }

        // ------------------------------------------------------------------ other ships

        /// <summary>
        /// A few traders under way on fixed courses (closed loops in deep water between the places, run by a looping
        /// animation on a carrier; the ship under it rides the waves with ShipMotion and takes its heading from the
        /// course). The Gislinge stands in for every ship until there are more hulls. Note: the courses run on the
        /// game's own clock, not the level's speed-up or pause.
        /// </summary>
        static void Traders(Level L, GameObject shipPrefab, Transform parent)
        {
            var root = new GameObject("Other ships under way (on fixed courses)").transform;
            root.SetParent(parent, false);
            Vector2? Off(string place, float margin) => L.places.TryGetValue(place, out var p) ? Offshore(p, L.H, margin) : (Vector2?)null;
            Vector2? OffMark(string mark, float margin) => L.marks.TryGetValue(mark, out var p) ? Offshore(p, L.H, margin) : (Vector2?)null;
            int made = 0;
            void Course(string name, Vector2? a, Vector2? b, float speed, float offset)
            {
                if (!a.HasValue || !b.HasValue || Vector2.Distance(a.Value, b.Value) < 100f)
                    return;
                var loop = Loop(L, a.Value, b.Value, offset);
                if (loop == null || loop.Count < 3)
                    return;
                MakeCourse(L, root, shipPrefab, name, loop, speed, made);
                made++;
            }
            // Frisian traders ran the Rhine mouth - Thames run; Kent and Boulogne faced each other across the Narrows as
            // they had since the Romans; Walichrum was a port of call on the way north (all sure in outline)
            Course("A Frisian trader between the Rhine mouth and the Thames", Off("The Rhine mouth at Katwijk", 220f), Off("The Thames mouth", 110f), 2.4f, 45f);
            Course("A Kentish boat crossing to Boulogne", Off("Dover", 150f), OffMark("Boulogne", 140f), 2.0f, 35f);
            Course("A trader bound up the coast for Walichrum", Off("Walichrum", 170f), Off("The Maas mouth", 170f), 2.2f, 40f);
            Debug.Log($"Kapman North Sea: {made} ships under way.");
        }

        /// <summary>A closed loop out along one side and back along the other, bent round any shoal, starting a way along the first leg.</summary>
        static List<Vector2> Loop(Level L, Vector2 a, Vector2 b, float offset)
        {
            Vector2 dir = (b - a).normalized, perp = new Vector2(-dir.y, dir.x);
            Vector2 Corner(Vector2 c, float side)
            {
                Vector2 q = c + perp * side * offset;
                return L.Inside(q) && L.Real(q) < -3f ? q : c;
            }
            Vector2 p0 = Corner(a, 1f), p1 = Corner(b, 1f), p2 = Corner(b, -1f), p3 = Corner(a, -1f);
            var pts = new List<Vector2> { p0 };
            pts.AddRange(DeepLeg(L, p0, p1, 0));
            pts.AddRange(DeepLeg(L, p1, p2, 0));
            pts.AddRange(DeepLeg(L, p2, p3, 0));
            pts.AddRange(DeepLeg(L, p3, p0, 0));
            pts.RemoveAt(pts.Count - 1);
            var clean = new List<Vector2>();
            foreach (var p in pts)
                if (clean.Count == 0 || Vector2.Distance(clean[clean.Count - 1], p) > 2f)
                    clean.Add(p);
            if (clean.Count > 1 && Vector2.Distance(clean[0], clean[clean.Count - 1]) <= 2f)
                clean.RemoveAt(clean.Count - 1);
            if (clean.Count < 3)
                return null;
            // start a way along the first leg, so no turn falls on the loop's seam
            Vector2 start = Vector2.Lerp(clean[0], clean[1], 0.4f);
            var loop = new List<Vector2> { start };
            loop.AddRange(clean.Skip(1));
            loop.Add(clean[0]);
            return loop;
        }

        /// <summary>The points after a (not including it) that take a ship from a to b through water at least 3 m deep.</summary>
        static List<Vector2> DeepLeg(Level L, Vector2 a, Vector2 b, int depth)
        {
            if (LegDeep(L, a, b) || depth >= 3)
                return new List<Vector2> { b };
            Vector2 mid = (a + b) / 2f, dir = (b - a).normalized, perp = new Vector2(-dir.y, dir.x);
            float[] offs = { 40f, -40f, 90f, -90f, 180f, -180f, 320f, -320f };
            foreach (float off in offs)
            {
                Vector2 m = mid + perp * off;
                if (L.Inside(m) && L.Real(m) < -4f && LegDeep(L, a, m) && LegDeep(L, m, b))
                    return new List<Vector2> { m, b };
            }
            foreach (float off in offs)
            {
                Vector2 m = mid + perp * off;
                if (L.Inside(m) && L.Real(m) < -4f)
                {
                    var r = DeepLeg(L, a, m, depth + 1);
                    r.AddRange(DeepLeg(L, m, b, depth + 1));
                    return r;
                }
            }
            return new List<Vector2> { b };
        }

        static bool LegDeep(Level L, Vector2 a, Vector2 b)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f)
                return true;
            for (float u = 0f; u <= len; u += 6f)
            {
                Vector2 p = Vector2.Lerp(a, b, u / len);
                if (!L.Inside(p) || L.Real(p) > -3f)
                    return false;
            }
            return true;
        }

        static void Linear(AnimationCurve c)
        {
            for (int i = 0; i < c.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.Linear);
            }
        }

        static void MakeCourse(Level L, Transform root, GameObject prefab, string name, List<Vector2> loop, float speed, int index)
        {
            int cnt = loop.Count;
            var ts = new float[cnt + 1];
            for (int i = 1; i <= cnt; i++)
                ts[i] = ts[i - 1] + Vector2.Distance(loop[i - 1], loop[i % cnt]) / speed;
            var cx = new AnimationCurve();
            var cy = new AnimationCurve();
            var cz = new AnimationCurve();
            var ch = new AnimationCurve();
            for (int i = 0; i <= cnt; i++)
            {
                var p = loop[i % cnt];
                cx.AddKey(ts[i], p.x);
                cy.AddKey(ts[i], 0f);
                cz.AddKey(ts[i], p.y);
            }
            // the heading holds along each leg and swings over a few seconds at each turn; unwrapped, so every turn goes
            // the short way round. The last leg runs the first leg's line, so the loop's seam has no turn.
            float head = 0f;
            for (int i = 0; i < cnt; i++)
            {
                Vector2 a = loop[i], b = loop[(i + 1) % cnt];
                float want = Mathf.Atan2(b.x - a.x, b.y - a.y) * Mathf.Rad2Deg;
                head = i == 0 ? want : head + Mathf.DeltaAngle(head, want);
                float turn = Mathf.Min(10f, (ts[i + 1] - ts[i]) * 0.3f);
                ch.AddKey(i == 0 ? 0f : ts[i] + turn, head);
                ch.AddKey(i == cnt - 1 ? ts[cnt] : ts[i + 1] - turn, head);
            }
            Linear(cx);
            Linear(cy);
            Linear(cz);
            Linear(ch);
            var clip = new AnimationClip { name = "Course_" + index, legacy = true, wrapMode = WrapMode.Loop, frameRate = 30f };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), cx);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), cy);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), cz);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Ship", typeof(ShipMotion), "heading"), ch);
            string clipPath = $"{LevelDir}/Courses/{clip.name}.anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
                AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);

            var course = new GameObject(name + $" ({ts[cnt] / 60f:0} min round)");
            course.transform.SetParent(root, false);
            course.transform.position = new Vector3(loop[0].x, 0f, loop[0].y);
            var anim = course.AddComponent<Animation>();
            anim.clip = clip;
            anim.AddClip(clip, clip.name);
            anim.playAutomatically = true;
            anim.wrapMode = WrapMode.Loop;
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            var ship = (GameObject)PrefabUtility.InstantiatePrefab(prefab, course.transform);
            ship.name = "Ship";
            float head0 = ch.length > 0 ? ch.keys[0].value : 0f;
            ship.transform.localPosition = Vector3.zero;
            ship.transform.localRotation = Quaternion.Euler(0f, head0, 0f);
            var sm = ship.AddComponent<ShipMotion>();
            sm.player = false;
            sm.heading = head0;
            HdrpWater.DressShip(ship, false);
            if (index == 0)
            {
                L.spots["trader"] = new Vector3(loop[0].x, 0f, loop[0].y);
                L.spots["trader heading"] = new Vector3(Mathf.Sin(head0 * Mathf.Deg2Rad), 0f, Mathf.Cos(head0 * Mathf.Deg2Rad));
            }
        }

        // ------------------------------------------------------------------ views for the shots

        static void Views(Level L, Transform ship, Vector2 rhine, float size)
        {
            var views = new GameObject("Views (for the shots; move them as you like)").transform;
            void View(string name, Vector3 at, Vector3 lookAt)
            {
                var v = new GameObject(name).transform;
                v.SetParent(views, false);
                v.position = at;
                v.rotation = Quaternion.LookRotation(lookAt - at, Vector3.up);
            }
            Vector3 sp = ship.position, fwd = ship.forward, side = Vector3.Cross(Vector3.up, fwd);
            View("1 Off the Rhine's mouth", sp - fwd * 24f + side * 10f + Vector3.up * 5f, sp + fwd * 60f + Vector3.up * 2f);
            View("2 Looking back at the coast", sp + fwd * 30f + side * 8f + Vector3.up * 4f, new Vector3(rhine.x, 2f, rhine.y));
            View("3 The sea from high up", new Vector3(size * 0.5f, 700f, size * 0.15f), new Vector3(size * 0.5f, 0f, size * 0.62f));
            if (L.places.TryGetValue("Dover", out var dover))
            {
                Vector2 d = Offshore(dover, L.H, 260f);
                View("4 Toward Dover", new Vector3(d.x, 6f, d.y), new Vector3(dover.x, 6f, dover.y));
                Vector2 near = Offshore(dover, L.H, 120f);
                Vector3 at = L.spots.TryGetValue("pharos", out var ph) ? ph : new Vector3(dover.x, 0f, dover.y);
                View("6 Dover's cliffs and the Roman pharos", new Vector3(near.x, 7f, near.y), at + Vector3.up * 10f);
            }
            View("5 Low over the waves", sp + side * 3f + Vector3.up * 1.6f - fwd * 6f, sp + fwd * 200f);
            foreach (var t in L.towns)
            {
                if (!t.name.StartsWith("Lundenwic") && !t.name.StartsWith("Walichrum"))
                    continue;
                float face = Facing(L, t.centre);
                var toWater = new Vector3(Mathf.Sin(face * Mathf.Deg2Rad), 0f, Mathf.Cos(face * Mathf.Deg2Rad));
                var c = new Vector3(t.centre.x, L.H(t.centre), t.centre.y);
                string label = t.name.StartsWith("Lundenwic") ? "7 Lundenwic from the Thames" : "8 Walichrum's strand from the sea";
                View(label, c + toWater * (t.radius + 60f) + Vector3.up * 6f, c + Vector3.up * 3f);
            }
            if (L.marks.TryGetValue("Birds: the Wadden flats", out var wadden))
                View("9 The Wadden flats", new Vector3(wadden.x - 160f, 140f, wadden.y - 260f), new Vector3(wadden.x, 0f, wadden.y));
            if (L.spots.TryGetValue("lighthouse", out var lh))
            {
                Vector2 bou = new Vector2(lh.x, lh.z);
                Vector2 o = Offshore(bou, L.H, 150f);
                View("10 Boulogne's lighthouse from the sea", new Vector3(o.x, 6f, o.y), lh + Vector3.up * 14f);
            }
            if (L.spots.TryGetValue("trader", out var tr) && L.spots.TryGetValue("trader heading", out var th))
            {
                Vector3 tside = Vector3.Cross(Vector3.up, th);
                View("11 A trader under way", tr - th * 28f + tside * 12f + Vector3.up * 5f, tr + th * 10f + Vector3.up * 2f);
            }
            if (L.spots.TryGetValue("weir", out var wr))
                View("12 Fish weirs on the flats", wr + new Vector3(-30f, 9f, -40f), wr + Vector3.up * 0.5f);
        }

        // ------------------------------------------------------------------ helpers kept from the first build

        /// <summary>From a point on the coast, out to sea to where it's deep enough to sail, plus a margin.</summary>
        static Vector2 Offshore(Vector2 from, System.Func<Vector2, float> H, float margin)
        {
            Vector2 best = from;
            float bestDepth = 0f;
            for (int a = 0; a < 360; a += 15)
            {
                Vector2 dir = new Vector2(Mathf.Sin(a * Mathf.Deg2Rad), Mathf.Cos(a * Mathf.Deg2Rad));
                for (float r = 10f; r < 400f; r += 10f)
                {
                    float d = -H(from + dir * r);
                    if (d > 0.8f)
                    {
                        Vector2 p = from + dir * (r + margin);
                        float dp = -H(p);
                        if (dp > bestDepth || best == from)
                        {
                            if (r < 200f && dp > 0.6f)
                            {
                                best = p;
                                bestDepth = dp;
                            }
                        }
                        break;
                    }
                }
            }
            return best;
        }
    }
}
