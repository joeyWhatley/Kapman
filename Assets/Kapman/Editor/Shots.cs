using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kapman.Levels;
using Kapman.Levels.Water;
using Kapman.Levels.Hdrp;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE SHOTS - renders a level from its marked views at set times, weathers and seasons, into Captures/ (outside
    /// Assets, so nothing is imported), without opening a window: for checking the level and the light. The scene's own
    /// colours, as HDRP draws them, no look on top. Menu: Kapman > Levels > Shoot Dorestad.
    ///
    /// HDRP settles over frames (the exposure adapts, the clouds, fog and reflections accumulate), so each shot is
    /// rendered several times and the exposure's adaptation is made instant while shooting.
    /// </summary>
    public static class Shots
    {
        public static string OutDir => Path.Combine(Directory.GetCurrentDirectory(), "Captures");

        /// <summary>Renders per shot before the one that is kept: the exposure, clouds, fog and reflections settle.</summary>
        public const int Settle = 6;
        /// <summary>The frames rendered before the one kept (diagnostics may raise it; Settle by default).</summary>
        public static int SettleFrames = Settle;
        /// <summary>(diagnostics) Render with Camera.Render instead of a render request.</summary>
        public static bool CameraRender;

        struct Shot
        {
            public string name;
            public string view;
            public int day;
            public float hour;
            public WeatherKind weather;
            public float flood;
            public bool aurora;
            public System.Action setup;      // (diagnostics) something to switch before the shot
            public System.Action<Camera> place;   // (diagnostics) move the camera after it is put at the view
            public System.Action after;      // something to note or put back after the shot is written
        }

        static Shot S(string name, string view, int day, float hour, WeatherKind weather, float flood = 0f, bool aurora = false) =>
            new Shot { name = name, view = view, day = day, hour = hour, weather = weather, flood = flood, aurora = aurora };

        [MenuItem("Kapman/Levels/Shoot Dorestad plain")]
        /// <summary>The shot moved down to a standing man's eye (1.7 m over the ground) and looking a little down.</summary>
        static Shot Eye(Shot s)
        {
            s.place = cam =>
            {
                var t = Terrain.activeTerrain;
                var p = cam.transform.position;
                p.y = (t != null ? t.SampleHeight(p) + t.transform.position.y : p.y) + 1.7f;
                cam.transform.SetPositionAndRotation(p, Quaternion.Euler(6f, cam.transform.eulerAngles.y, 0f));
            };
            return s;
        }

        /// <summary>A light look at Dorestad as it is (1080p, eight views): for checks while memory is short.</summary>
        public static void DorestadLook()
        {
            const WeatherKind F = WeatherKind.Fair;
            var shots = new List<Shot>
            {
                S("L1 the waterfront, evening", "1", 172, 19.25f, F),
                S("L2 the dams from the water, evening", "8", 172, 19.25f, F),
                S("L3 a house and its yard, afternoon", "7", 172, 16f, F),
                S("L4 the farm strip and the fields, afternoon", "11", 172, 16f, F),
                S("L5 over the stern, noon", "4", 172, 12.5f, F),
                S("L6 down the Kromme Rijn, morning haze", "3", 172, 8f, WeatherKind.Haze),
                S("L7 over the whole emporium, low sun", "12", 172, 19.6f, F),
                S("L8 dusk with the lamps", "8", 172, 21.4f, F),
                Eye(S("L9 the meadow at eye height, afternoon", "11", 172, 16f, F)),
                Eye(S("L10 the bank at eye height, morning", "3", 172, 9f, F)),
            };
            Shoot(DorestadBuilder.ScenePath, shots, 1920, 1080);
        }

        public static void DorestadPlain()
        {
            const WeatherKind F = WeatherKind.Fair;
            var shots = new List<Shot>
            {
                S("P01 the dams from the water, evening", "8", 172, 19.25f, F),
                S("P02 along a dam to the storehouses, evening", "9", 172, 19.25f, F),
                S("P03 the market and the vicus from the harbour, evening", "10", 172, 19.25f, F),
                S("P04 the waterfront, evening", "1", 172, 19.25f, F),
                S("P05 a house and its yard, evening", "7", 172, 19.25f, F),
                S("P06 the farm strip and the fields, afternoon", "11", 172, 16f, F),
                S("P07 over the whole emporium, low sun", "12", 172, 19.6f, F),
                S("P08 the emporium from high up, noon", "5", 172, 12.5f, F),
                S("P09 the fork with the Lek from above, noon", "2", 172, 12.5f, F),
                S("P10 down the Kromme Rijn, dawn mist", "3", 172, 5f, WeatherKind.Mist),
                S("P11 over the stern, noon", "4", 172, 12.5f, F),
                S("P12 dusk with the lamps", "8", 172, 21.4f, F),
                S("P13 a summer storm", "1", 200, 16f, WeatherKind.Storm),
                S("P14 January snow, ice on the river", "8", 20, 13f, WeatherKind.Snow),
                S("P15 autumn, the farms", "11", 288, 15.5f, WeatherKind.Haze),
                S("P16 St John's Eve, the midsummer fire", "6", 174, 22.2f, F),
                // the year in the birds (wildlife slice 1): swallows over the harbour in July; in late November the geese
                // up at dawn and no swallows (the presence the calendar gave is written beside each picture, .txt)
                W(S("P17 wildlife, 19 July 7am, swallows over the harbour", "1", 200, 7f, F)),
                W(S("P18 wildlife, 26 November 7am, the geese up at dawn, no swallows", "12", 330, 7f, F)),
            };
            Shoot(DorestadBuilder.ScenePath, shots, 3840, 2160);
            MapShot();
        }

        // ---------------------------------------------------------------- the wildlife shots

        /// <summary>
        /// A wildlife shot: the flocks build their birds only in Play, so for the shot they are built here, the level's
        /// Wildlife puts the calendar of the shot's day into them (birds hidden, flocks away) and each is placed once; after
        /// the render the calendar's lines and every flock's presence are written beside the picture and the birds are
        /// taken away again, so the scene is left as it was.
        /// </summary>
        static Shot W(Shot s)
        {
            var built = new List<(Flock flock, bool active, float presence)>();
            string name = s.name;
            s.setup = () => WildlifeSetup(built);
            s.after = () => WildlifeAfter(name, built);
            return s;
        }

        static void WildlifeSetup(List<(Flock flock, bool active, float presence)> built)
        {
            // the clock and the weather of the shot's day first: the calendar reads them
            var clock = Object.FindAnyObjectByType<LevelClock>();
            if (clock != null)
                clock.Apply();
            Call(Object.FindAnyObjectByType<LevelWeather>(), "Update");
            built.Clear();
            foreach (var f in Object.FindObjectsByType<Flock>(FindObjectsInactive.Include))
            {
                built.Add((f, f.gameObject.activeSelf, f.presence));
                f.Build();
            }
            var wildlife = Object.FindAnyObjectByType<Wildlife>();
            if (wildlife == null)
                Debug.LogWarning("Kapman shots: the scene has no Wildlife (rebuild the level): the flocks show without their calendar.");
            else if (!wildlife.ShowNow())
                Debug.LogWarning("Kapman shots: Wildlife could not read the calendar (see the error above): the flocks show without it.");
            foreach (var (f, _, _) in built)
                if (f.gameObject.activeInHierarchy)
                    Call(f, "Update");
        }

        static void WildlifeAfter(string name, List<(Flock flock, bool active, float presence)> built)
        {
            var lines = new List<string> { name, Wildlife.HudLines, "" };
            foreach (var (f, _, _) in built)
                lines.Add($"{f.name}: {f.Species} presence {f.presence:0.00}, {(f.gameObject.activeSelf ? "here" : "away")}");
            File.WriteAllLines(Path.Combine(OutDir, name + ".txt"), lines);
            foreach (var (f, active, presence) in built)
            {
                f.Clear();
                f.presence = presence;
                f.gameObject.SetActive(active);
            }
            built.Clear();
        }

        [MenuItem("Kapman/Levels/Shoot Dorestad")]
        public static void Dorestad()
        {
            var shots = new List<Shot>
            {
                S("01 waterfront, 21 June evening", "1", 172, 19.25f, WeatherKind.Fair),
                S("05 waterfront, dusk with lamps", "1", 172, 21.4f, WeatherKind.Fair),
                S("06 the fork with the Lek from above", "2", 172, 19.25f, WeatherKind.Fair),
                S("07 down the Kromme Rijn, dawn mist", "3", 172, 5.0f, WeatherKind.Mist),
                S("08 over the stern, noon", "4", 172, 12.5f, WeatherKind.Fair),
                S("09 the emporium from high up, noon", "5", 172, 12.5f, WeatherKind.Fair),
                S("10 midnight, stars and moon", "3", 172, 23.6f, WeatherKind.Fair),
                S("11 a summer storm", "1", 200, 16f, WeatherKind.Storm),
                S("12 January snow, ice on the river", "1", 20, 13f, WeatherKind.Snow),
                S("13 a February flood", "2", 45, 14f, WeatherKind.Overcast, 0.8f),
                S("14 an aurora in October", "3", 290, 22.5f, WeatherKind.Fair, 0f, true),
                S("15 autumn", "5", 288, 15.5f, WeatherKind.Haze),
                S("16 St John's Eve, the midsummer fire", "6", 174, 22.2f, WeatherKind.Fair),
                S("17 a house and its jetty, evening", "7", 172, 19.25f, WeatherKind.Fair),
                S("19 a house and its jetty, dusk", "7", 172, 21.6f, WeatherKind.Fair),
            };
            Shoot(DorestadBuilder.ScenePath, shots, 3840, 2160);
            MapShot();
        }

        [MenuItem("Kapman/Levels/Shoot the southern North Sea")]
        public static void NorthSea()
        {
            var shots = new List<Shot>
            {
                S("N1 off the Rhine's mouth, evening", "1", 172, 17.5f, WeatherKind.Haze),
                S("N3 looking back at the coast, sunset", "2", 172, 20.2f, WeatherKind.Haze),
                S("N4 the sea from high up, noon", "3", 172, 13f, WeatherKind.Fair),
                S("N5 toward Dover", "4", 172, 15f, WeatherKind.Fair),
                S("N6 low over the waves, a storm", "5", 200, 15f, WeatherKind.Storm),
                S("N7 night at sea, moon and stars", "5", 172, 23.6f, WeatherKind.Fair),
                S("N8 sea fog at dawn", "1", 172, 5f, WeatherKind.Fog),
                S("N9 Dover's cliffs and the Roman pharos", "6", 172, 16f, WeatherKind.Fair),
                S("N10 Lundenwic from the Thames", "7", 172, 18.5f, WeatherKind.Haze),
                S("N11 Walichrum's strand", "8", 172, 10f, WeatherKind.Fair),
                S("N12 the Wadden flats", "9", 172, 14f, WeatherKind.Fair),
                S("N13 Boulogne's lighthouse from the sea", "10", 172, 19.5f, WeatherKind.Haze),
                S("N14 a trader under way", "11", 172, 11f, WeatherKind.Fair),
                S("N15 fish weirs on the flats", "12", 172, 7f, WeatherKind.Mist),
            };
            Shoot(OceanBuilder.ScenePath, shots, 3840, 2160);
        }

        // ---------------------------------------------------------------- diagnostics (3 Oct: the black dawn, the night dots)

        public static void DiagDorestad()
        {
            var shots = new List<Shot>();
            foreach (var h in new[] { 4.5f, 5f, 5.5f, 6f, 7f })
                shots.Add(S($"D dawn {h:0.0} fair", "10", 172, h, WeatherKind.Fair));
            shots.Add(S("D dawn 5.0 mist", "10", 172, 5f, WeatherKind.Mist));
            shots.Add(S("D dawn 5.0 fog", "10", 172, 5f, WeatherKind.Fog));
            shots.Add(S("D noon fog", "10", 172, 12f, WeatherKind.Fog));
            shots.Add(S("D dawn 5.0 fair view 1", "1", 172, 5f, WeatherKind.Fair));
            Shoot(DorestadBuilder.ScenePath, shots, 1920, 1080);
        }

        public static void DiagFog()
        {
            void Set(bool noVol, float depth, float aniso, float dens, float top, bool constant)
            {
                HdrpWeatherDraw.DiagNoVolumetric = noVol; HdrpWeatherDraw.DiagDepthExtent = depth; HdrpWeatherDraw.DiagAnisotropy = aniso;
                HdrpWeatherDraw.DiagDensityScale = dens; HdrpWeatherDraw.DiagTop = top; HdrpWeatherDraw.DiagConstantColour = constant;
            }
            Shot F(string name, System.Action a) { var s = S(name, "10", 172, 12f, WeatherKind.Fog); s.setup = a; return s; }
            var shots = new List<Shot>
            {
                F("F fog as is", () => Set(false, -1f, -1f, 1f, -1f, false)),
                F("F fog not volumetric", () => Set(true, -1f, -1f, 1f, -1f, false)),
                F("F fog depth 300", () => Set(false, 300f, -1f, 1f, -1f, false)),
                F("F fog anisotropy 0", () => Set(false, -1f, 0f, 1f, -1f, false)),
                F("F fog quarter density", () => Set(false, -1f, -1f, 0.25f, -1f, false)),
                F("F fog top 400", () => Set(false, -1f, -1f, 1f, 400f, false)),
                F("F fog constant colour", () => Set(false, -1f, -1f, 1f, -1f, true)),
                F("F fog as is again", () => Set(false, -1f, -1f, 1f, -1f, false)),
            };
            Shoot(DorestadBuilder.ScenePath, shots, 1920, 1080);
            Set(false, -1f, -1f, 1f, -1f, false);
        }

        public static void DiagFog2()
        {
            void Set(bool noClouds, float fixedEv, float overcastLight)
            {
                HdrpWeatherDraw.DiagNoClouds = noClouds; SkyDome.DiagFixedExposure = fixedEv; LevelClock.DiagOvercastLight = overcastLight;
            }
            Shot F(string name, System.Action a) { var s = S(name, "10", 172, 12f, WeatherKind.Fog); s.setup = a; return s; }
            var shots = new List<Shot>
            {
                F("G fog as is", () => Set(false, -100f, -1f)),
                F("G fog no clouds", () => Set(true, -100f, -1f)),
                F("G fog fixed 12 EV", () => Set(false, 12f, -1f)),
                F("G fog fixed 9 EV", () => Set(false, 9f, -1f)),
                F("G fog sun undimmed", () => Set(false, -100f, 1f)),
                F("G fog no clouds fixed 12", () => Set(true, 12f, -1f)),
                new Shot { name = "G fair fixed 12 EV", view = "10", day = 172, hour = 12f, weather = WeatherKind.Fair, setup = () => Set(false, 12f, -1f) },
            };
            Shoot(DorestadBuilder.ScenePath, shots, 1280, 720);
            Set(false, -100f, -1f);
        }

        /// <summary>Why the terrain's grass does not show in the shots: the same meadow from eye height with 6 and with 90
        /// settle frames (Unity builds detail patches a few a frame).</summary>
        public static void DiagGrass()
        {
            void Eye(Camera cam)
            {
                var t = Terrain.activeTerrain;
                var p = cam.transform.position;
                p.y = (t != null ? t.SampleHeight(p) + t.transform.position.y : p.y) + 1.7f;
                cam.transform.SetPositionAndRotation(p, Quaternion.Euler(6f, cam.transform.eulerAngles.y, 0f));
            }
            Shot G(string name, int settle) { var s = S(name, "11", 172, 16f, WeatherKind.Fair); s.setup = () => SettleFrames = settle; s.place = Eye; return s; }
            // the thickest sward on the map: the camera stands in it, looking along the levee
            void InSward(Camera cam)
            {
                var t = Terrain.activeTerrain;
                var td = t.terrainData;
                var layer = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, 0);
                int bx = 0, bz = 0, best = -1;
                for (int z = 8; z < td.detailHeight - 8; z += 4)
                    for (int x = 8; x < td.detailWidth - 8; x += 4)
                    {
                        int sum = 0;
                        for (int dz = -4; dz <= 4; dz += 2)
                            for (int dx = -4; dx <= 4; dx += 2)
                                sum += layer[z + dz, x + dx];
                        if (sum > best) { best = sum; bx = x; bz = z; }
                    }
                var p = new Vector3((bx + 0.5f) / td.detailWidth * td.size.x, 0f, (bz + 0.5f) / td.detailHeight * td.size.z) + t.transform.position;
                p.y = t.SampleHeight(p) + t.transform.position.y + 1.7f;
                cam.transform.SetPositionAndRotation(p, Quaternion.Euler(8f, 40f, 0f));
                Debug.Log($"Kapman grass check: the thickest sward ({best} in 25 cells) at {p}");
            }
            var z2 = S("Z2 the thickest sward at eye height", "11", 172, 16f, WeatherKind.Fair);
            z2.place = InSward;
            var shots = new List<Shot> { z2 };
            Shoot(DorestadBuilder.ScenePath, shots, 1600, 900);
            SettleFrames = Settle;
            CameraRender = false;
        }

        public static void DiagNorthSea()
        {
            void Set(bool wake, float moon, float stars, bool sail)
            {
                foreach (var w in Object.FindObjectsByType<ShipWake>(FindObjectsInactive.Include))
                {
                    w.enabled = wake;
                    foreach (var ps in w.GetComponentsInChildren<ParticleSystem>(true))
                        ps.gameObject.SetActive(wake);
                    foreach (Transform t in w.transform)
                        if (t.name.Contains("ake") || t.name.Contains("trike") || t.name.Contains("pray") || t.name.Contains("oam"))
                            t.gameObject.SetActive(wake);
                }
                var clock = Object.FindAnyObjectByType<LevelClock>();
                if (clock != null) clock.moonBoost = moon;
                var dome = Object.FindAnyObjectByType<SkyDome>();
                if (dome != null) dome.starLight = stars;
                foreach (var r in Object.FindObjectsByType<MeshRenderer>())
                    if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Sail"))
                        r.enabled = sail;
            }
            Shot N(string name, System.Action a) { var s = S(name, "5", 172, 23.6f, WeatherKind.Fair); s.setup = a; return s; }
            var shots = new List<Shot>
            {
                N("X night as is", () => Set(true, 3f, 5f, true)),
                N("X night no wake", () => Set(false, 3f, 5f, true)),
                N("X night no moon light", () => Set(true, 0f, 5f, true)),
                N("X night no stars", () => Set(true, 3f, 0f, true)),
                N("X night no sail", () => Set(true, 3f, 5f, false)),
                N("X night as is again", () => Set(true, 3f, 5f, true)),
            };
            Shoot(OceanBuilder.ScenePath, shots, 1920, 1080);
        }

        /// <summary>The whole level from straight above at noon (to check the land and the trees).</summary>
        public static void MapShot()
        {
            var cam = Object.FindAnyObjectByType<Camera>();
            var clock = Object.FindAnyObjectByType<LevelClock>();
            var weather = Object.FindAnyObjectByType<LevelWeather>();
            var terrain = Object.FindAnyObjectByType<Terrain>();
            if (cam == null || terrain == null || clock == null)
                return;
            float size = terrain.terrainData.size.x;
            clock.startDay = 172;
            clock.GameHours = 12.5;
            if (weather != null)
            {
                weather.kind = WeatherKind.Fair;
                Call(weather, "Update");
                Call(weather, "LateUpdate");    // the HDRP side (clouds, fog, mist, the snow pass) is pushed in LateUpdate
            }
            clock.Apply();
            float far = cam.farClipPlane, fov = cam.fieldOfView, bias = QualitySettings.lodBias;
            cam.fieldOfView = 64f;
            cam.farClipPlane = 9000f;
            QualitySettings.lodBias = 60f;   // so the trees show at this height
            cam.transform.SetPositionAndRotation(new Vector3(size / 2f, size / 2f / Mathf.Tan(32f * Mathf.Deg2Rad), size / 2f), Quaternion.Euler(90f, 0f, 0f));
            using (new InstantExposure())
                Render(cam, Path.Combine(OutDir, "20 the whole level from above, noon.png"), 4096, 4096);
            cam.fieldOfView = fov;
            cam.farClipPlane = far;
            QualitySettings.lodBias = bias;
        }

        static void Shoot(string scenePath, List<Shot> shots, int w, int h)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(OutDir);
            var cam = Object.FindAnyObjectByType<Camera>();
            var clock = Object.FindAnyObjectByType<LevelClock>();
            var weather = Object.FindAnyObjectByType<LevelWeather>();
            var water = Object.FindAnyObjectByType<LevelWater>();
            var waves = Object.FindAnyObjectByType<WaterWaves>();
            var info = Object.FindAnyObjectByType<LevelInfo>();
            var life = Object.FindAnyObjectByType<TownLife>();
            var dome = Object.FindAnyObjectByType<SkyDome>();
            var views = Object.FindObjectsByType<Transform>().Where(t => t.parent != null && t.parent.name.StartsWith("Views")).ToList();
            if (cam == null || clock == null || weather == null)
            {
                Debug.LogWarning("Kapman shots: the scene has no camera, clock or weather: " + scenePath);
                return;
            }
            // a state to come back to
            int day0 = clock.startDay;
            double hours0 = clock.GameHours;
            var kind0 = weather.kind;
            bool manual0 = weather.manual;
            using (new InstantExposure())
            {
                foreach (var s in shots)
                {
                    var view = views.FirstOrDefault(v => v.name.StartsWith(s.view + " "));
                    if (view == null)
                    {
                        Debug.LogWarning("Kapman shots: no view " + s.view);
                        continue;
                    }
                    clock.startDay = s.day;
                    clock.GameHours = s.hour;
                    weather.manual = true;                     // a shot asks for its weather; the engine waits
                    weather.kind = s.weather;
                    weather.flood = s.flood;
                    weather.forceAurora = s.aurora;
                    s.setup?.Invoke();
                    cam.transform.SetPositionAndRotation(view.position, view.rotation);
                    s.place?.Invoke(cam);
                    for (int k = 0; k < 2; k++)
                    {
                        Call(info, "Update");
                        clock.Apply();
                        Call(weather, "Update");
                Call(weather, "LateUpdate");    // the HDRP side (clouds, fog, mist, the snow pass) is pushed in LateUpdate
                        Call(water, "Update");
                        Call(waves, "Update");
                        Call(life, "Update");
                        clock.Apply();
                        if (dome != null)
                            dome.Refresh();
                    }
                    string file = Path.Combine(OutDir, s.name + ".png");
                    Render(cam, file, w, h);
                    s.after?.Invoke();
                    Debug.Log("Kapman shots: " + file);
                }
            }
            clock.startDay = day0;
            clock.GameHours = hours0;
            weather.kind = kind0;
            weather.manual = manual0;
            weather.flood = 0f;
            weather.forceAurora = false;
            Call(weather, "Update");
                Call(weather, "LateUpdate");    // the HDRP side (clouds, fog, mist, the snow pass) is pushed in LateUpdate
            clock.Apply();
        }

        static void Call(Object o, string method)
        {
            if (o == null)
                return;
            var m = o.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            m?.Invoke(o, null);
        }

        /// <summary>
        /// The camera rendered through HDRP into a texture and written as a PNG. The camera's HDAdditionalCameraData
        /// (added if it has none) carries its volumes and post-processing; the render goes through HDRP's render request,
        /// so the full frame (sky, clouds, fog, water, post-processing) is drawn, several times so it settles.
        /// </summary>
        public static void Render(Camera cam, string file, int w, int h)
        {
            if (!cam.TryGetComponent<HDAdditionalCameraData>(out var data))
                data = cam.gameObject.AddComponent<HDAdditionalCameraData>();
            var aa0 = data.antialiasing;
            data.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;   // the settle frames build its history; it also quiets the specular fireflies on ropes and ridges at 4K
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            var request = new RenderPipeline.StandardRequest { destination = rt };
            bool supported = RenderPipeline.SupportsRenderRequest(cam, request);
            for (int i = 0; i <= SettleFrames; i++)
            {
                if (supported && !CameraRender)
                    RenderPipeline.SubmitRenderRequest(cam, request);
                else
                {
                    cam.targetTexture = rt;
                    cam.Render();
                    cam.targetTexture = null;
                }
            }
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
            data.antialiasing = aa0;
        }

        /// <summary>
        /// While it lives, every Exposure in the open scene's volumes adapts instantly (HDRP's progressive adaptation
        /// takes seconds, which a handful of renders would not give), then it puts the profiles back as they were.
        /// </summary>
        class InstantExposure : System.IDisposable
        {
            readonly List<(Exposure exposure, bool overridden, AdaptationMode mode)> was = new List<(Exposure, bool, AdaptationMode)>();

            public InstantExposure()
            {
                foreach (var volume in Object.FindObjectsByType<Volume>())
                {
                    var profile = volume.sharedProfile;
                    if (profile == null || !profile.TryGet(out Exposure exposure))
                        continue;
                    was.Add((exposure, exposure.adaptationMode.overrideState, exposure.adaptationMode.value));
                    exposure.adaptationMode.Override(AdaptationMode.Fixed);
                }
            }

            public void Dispose()
            {
                foreach (var (exposure, overridden, mode) in was)
                {
                    exposure.adaptationMode.value = mode;
                    exposure.adaptationMode.overrideState = overridden;
                }
            }
        }
    }

    /// <summary>Entry points for Unity in batch mode (-executeMethod).</summary>
    public static class Batch
    {
        public static void BuildDorestad() => Run(DorestadBuilder.Build);
        public static void AddSoundscapeToDorestad() => Run(SoundscapeSetup.AddToDorestad);
        public static void BuildNorthSea() => Run(OceanBuilder.Build);
        public static void WriteDorestadLandmarks() => Run(LandmarkExport.FromSavedDorestad);
        public static void DiagRoute() => Run(LandmarkExport.DiagRoute);
        public static void GiveLevelsTheirWater() => Run(WaterProfileSetup.AddToSavedScenes);
        public static void BuildCameraTest() => Run(CameraTestBuilder.Build);
        public static void BuildScaleTests() => Run(ScaleTestBuilder.BuildAll);
        public static void RefreshScenes() => Run(ShowLevel.BringUpToDate);
        public static void AddGameToDorestad() => Run(() =>
        {
            EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);
            GameSetup.AddToScene(true);
        });
        public static void ShootDorestad() => Run(Shots.Dorestad);

        public static void BuildAndShootAll() => Run(() =>
        {
            DorestadBuilder.Build();
            OceanBuilder.Build();
            Shots.Dorestad();
            Shots.NorthSea();
        });

        public static void ShootNorthSea() => Run(Shots.NorthSea);
        public static void DiagDorestad() => Run(Shots.DiagDorestad);
        public static void DiagNorthSea() => Run(Shots.DiagNorthSea);
        public static void DiagGrass() => Run(Shots.DiagGrass);
        public static void DiagFog() => Run(Shots.DiagFog);
        public static void ShootDorestadLook() => Run(Shots.DorestadLook);
        public static void DiagFog2() => Run(Shots.DiagFog2);

        public static void BuildAndShootNorthSea() => Run(() =>
        {
            OceanBuilder.Build();
            Shots.NorthSea();
        });

        public static void ShootDorestadPlain() => Run(Shots.DorestadPlain);

        public static void BuildAndShootDorestadPlain() => Run(() =>
        {
            DorestadBuilder.Build();
            Shots.DorestadPlain();
        });

        public static void BuildAndShootDorestad() => Run(() =>
        {
            DorestadBuilder.Build();
            Shots.Dorestad();
        });

        public static void ApplyProjectSettings() => Run(ProjectSetup.Apply);

        static void Run(System.Action action)
        {
            try
            {
                action();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
    }
}
