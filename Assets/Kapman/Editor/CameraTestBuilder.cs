using System.IO;
using System.Linq;
using Kapman.Levels;
using Kapman.Levels.Hdrp;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE CAMERA TEST (the user, 4 Oct pm: "make a small simulated ocean with simulated wind/water waves everything and
    /// create the scene and animate it and create several different angles and views to choose from"): a small level of
    /// open sea (HDRP's ocean, its swell and chop from the weather, as on the North Sea) with a stretch of 60 m waterfront
    /// band on her east side (a beach, a meadow, a jetty, storehouses and houses from Dorestad, all true size), and the
    /// Gislinge with her body and sail sailing north along it. PlayMode CameraViewsTests films it from each candidate view.
    /// Kapman > Levels > Build the camera test, or Batch.BuildCameraTest.
    /// </summary>
    public static class CameraTestBuilder
    {
        public const string SceneName = "L_CameraTest";
        public const string ScenePath = "Assets/Kapman/Scenes/" + SceneName + ".unity";
        public const string LevelDir = "Assets/Kapman/Levels/CameraTest";
        const string Prefabs = "Assets/Kapman/Levels/Dorestad/Prefabs/";
        const string Ground = "Assets/Kapman/Levels/Dorestad/Ground/";

        /// <summary>The shore: the waterline runs north along x = 80 m; the band is the 60 m behind it.</summary>
        const float Waterline = 80f, Band = 60f;
        /// <summary>She sails north 45 m off the beach.</summary>
        public const float ShipTrackX = 35f;

        [MenuItem("Kapman/Levels/Build the camera test")]
        public static void Build()
        {
            ProjectSetup.Apply();
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), LevelDir));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var levelGo = new GameObject("Level: the camera test (open sea and a stretch of shore)");
            var info = levelGo.AddComponent<LevelInfo>();
            info.levelId = "camera_test";
            info.displayName = "The camera test";
            info.kind = LevelKind.Ocean;
            info.latitude = 52.6;
            info.longitude = 3.0;
            info.baseWaterLevel = 0f;
            info.WaterLevel = 0f;
            info.realMetresPerUnit = 1f;
            info.shipSpeed = 2.5f;
            var clock = levelGo.AddComponent<LevelClock>();
            clock.startDay = 280;
            clock.startHour = 14f;
            var weather = levelGo.AddComponent<LevelWeather>();
            weather.kind = WeatherKind.Overcast;
            weather.manual = true;                                   // the test sets the weather it films
            var water = levelGo.AddComponent<LevelWater>();
            water.tideLatitude = info.latitude;
            water.tideLongitude = info.longitude;
            water.tideShare = 0f;
            var waves = levelGo.AddComponent<WaterWaves>();
            waves.waves = new[]
            {
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
            var sea = HdrpWater.MakeSea(world, 0f, 4000f, LevelDir);
            water.surfaces = new[] { sea.transform };
            Shore(world);

            // the ship: off the shore, heading north along it (the test orders her on)
            var shipPrefab = AssetLibrary.Gislinge("Assets/Kapman/Prefabs/Ships/Ship_Gislinge.prefab");
            var ship = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab, world);
            ship.name = "Your ship: the Gislinge";
            ship.transform.SetPositionAndRotation(new Vector3(ShipTrackX, 0f, 0f), Quaternion.identity);
            var sm = ship.AddComponent<ShipMotion>();
            sm.player = true;
            sm.heading = 0f;
            HdrpWater.DressShip(ship, true);

            var vol = new GameObject("Sky and post-processing volume", typeof(Volume)).GetComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProjectSetup.VolumePath);
            var dome = new GameObject("Sky").AddComponent<SkyDome>();
            dome.volume = vol;
            dome.stars = AssetDatabase.LoadAssetAtPath<Cubemap>(ProjectSetup.StarsPath);

            var camGo = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 6000f;
            cam.fieldOfView = 40f;
            var camData = camGo.AddComponent<HDAdditionalCameraData>();
            camData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            var orbit = camGo.AddComponent<OrbitCamera>();
            orbit.target = ship.transform;
            orbit.isometric = false;
            orbit.pitch = 18f;
            orbit.distance = 40f;
            camGo.transform.position = new Vector3(-25f, 12f, -30f);
            camGo.transform.LookAt(ship.transform.position);
            weather.rain = DorestadBuilder.Precipitation(camGo.transform, "Rain", false, LevelDir);
            weather.snow = DorestadBuilder.Precipitation(camGo.transform, "Snow", true, LevelDir);
            new GameObject("Test panel").AddComponent<DevHud>();

            GameSetup.AddToScene(false);                             // (the ship's body, the game's parts)
            WaterProfileSetup.AddToOpenScene("northsea_south");     // the southern North Sea's own water
            clock.Apply();
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"Kapman camera test: built {ScenePath}.");
        }

        /// <summary>A height in metres for a point east of the ship's track: the sea bed, the beach, the band's meadow.</summary>
        static float ShoreHeight(float x, float z)
        {
            float wobble = 6f * Mathf.Sin(z / 70f) + 3f * Mathf.Sin(z / 23f + 1f);
            float d = x - (Waterline + wobble);                       // metres inland of the waterline
            if (d < -40f) return Mathf.Max(-25f, -4f + (d + 40f) * 0.04f);   // shelving away to the deep
            if (d < 0f) return Mathf.Lerp(-4f, 0f, (d + 40f) / 40f);  // the beach under the water
            if (d < 12f) return Mathf.Lerp(0f, 1.2f, d / 12f);         // the beach
            if (d < 20f) return Mathf.Lerp(1.2f, 2.6f, (d - 12f) / 8f);  // the bank
            return 2.6f + 0.4f * Mathf.PerlinNoise(x / 30f, z / 30f);  // the meadow of the band (and the haze beyond)
        }

        static void Shore(Transform world)
        {
            const float x0 = -760f, z0 = -300f, size = 1024f, lo = -30f, range = 40f;   // (the sea bed under the whole view)
            const int res = 513;
            var td = new TerrainData { heightmapResolution = res, size = new Vector3(size, range, size) };
            var h = new float[res, res];
            for (int j = 0; j < res; j++)
                for (int i = 0; i < res; i++)
                {
                    float x = x0 + i * size / (res - 1), z = z0 + j * size / (res - 1);
                    h[j, i] = Mathf.Clamp01((ShoreHeight(x, z) - lo) / range);
                }
            td.SetHeights(0, 0, h);
            var sand = AssetDatabase.LoadAssetAtPath<TerrainLayer>(Ground + "9_sand.terrainlayer");
            var meadow = AssetDatabase.LoadAssetAtPath<TerrainLayer>(Ground + "0_meadow.terrainlayer");
            var mud = AssetDatabase.LoadAssetAtPath<TerrainLayer>(Ground + "3_mud.terrainlayer");
            td.terrainLayers = new[] { sand, meadow, mud }.Where(l => l != null).ToArray();
            int n = td.terrainLayers.Length;
            if (n >= 2)
            {
                td.alphamapResolution = 512;
                var a = new float[512, 512, n];
                for (int j = 0; j < 512; j++)
                    for (int i = 0; i < 512; i++)
                    {
                        float x = x0 + i * size / 511f, z = z0 + j * size / 511f;
                        float y = ShoreHeight(x, z);
                        float s = Mathf.Clamp01(Mathf.InverseLerp(2.2f, 1.0f, y));       // sand to the top of the beach
                        a[j, i, 0] = s;
                        a[j, i, 1] = 1f - s;
                        if (n > 2) { float m = Mathf.Clamp01(Mathf.InverseLerp(-0.2f, -1.5f, y)) * 0.6f; a[j, i, 2] = m; a[j, i, 0] *= 1f - m; a[j, i, 1] *= 1f - m; }
                    }
                td.SetAlphamaps(0, 0, a);
            }
            AssetDatabase.CreateAsset(td, LevelDir + "/CameraTest_Shore.asset");
            var go = Terrain.CreateTerrainGameObject(td);
            go.name = "The shore (a 60 m waterfront band)";
            go.transform.SetParent(world, false);
            go.transform.position = new Vector3(x0, lo, z0);
            go.isStatic = true;

            // a jetty and the first row of buildings, true size, a little ahead of the ship's start
            void Put(string prefab, Vector3 at, float yaw)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
                if (p == null)
                    return;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(p, world);
                at.y = ShoreHeight(at.x, at.z);
                g.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            }
            for (int k = 0; k < 4; k++)
            {
                float z = 140f + k * 26f;
                Put("Gangway", new Vector3(Waterline - 2f, 0f, z), 90f);
                Put("Revetment", new Vector3(Waterline + 2f, 0f, z + 9f), 0f);
                Put(k % 2 == 0 ? "Storehouse_4.6x8_v1" : "Warehouse_4.6x10", new Vector3(Waterline + 22f, 0f, z + 4f), 90f);
                Put("Barrels", new Vector3(Waterline + 12f, 0f, z - 5f), 30f * k);
            }
            Put("House_5x12_v0", new Vector3(Waterline + 42f, 0f, 150f), 0f);
            Put("House_5x12_v0", new Vector3(Waterline + 44f, 0f, 190f), 8f);
            Put("Postrow", new Vector3(Waterline + 6f, 0f, 260f), 0f);
        }
    }
}
