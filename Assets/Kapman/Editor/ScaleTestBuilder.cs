using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Levels;
using Kapman.Levels.Water;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE SCALE TEST (the user, 4 Oct pm: "build several tiny levels of various scales to figure out which one to
    /// choose"): the same real stretch, Dorestad down the Kromme Rijn and the Old Rhine past Traiectum to the sea at
    /// Katwijk (about 75 km), built at 1:20, 1:40, 1:80 and 1:150. In each: the route shortened by the scale; the river at
    /// its true width (100 m at Dorestad, widening to 120 m at the mouth) with the bends a real lowland river that wide
    /// makes (a wavelength of 12 widths; at these scales the real loops are far too small to keep, only the route's big
    /// shape survives); the 60 m waterfront band; fog sheets over the inland beyond it; the ports at their importance
    /// (Dorestad 300 m of dams and storehouses, Traiectum 150 m, the hamlet at the mouth 50 m); the user's stand-in people
    /// on the jetties; the Gislinge at Dorestad. scale_info.json beside each level holds its line, length and sailing
    /// time for the filming test (ScaleTests) and the page.
    /// </summary>
    public static class ScaleTestBuilder
    {
        public static readonly int[] Scales = { 20, 40, 80, 150 };
        const string Prefabs = "Assets/Kapman/Levels/Dorestad/Prefabs/";
        const string Ground = "Assets/Kapman/Levels/Dorestad/Ground/";
        const float WidthTop = 100f, WidthMouth = 120f, Band = 60f, FogDepth = 260f, Step = 4f, Margin = 380f;
        public const float SailSpeed = 2f;                       // m/s, rowing with the stream: the time shown on the page

        // the c. 800 route: Dorestad, the Kromme Rijn by Cothen, Werkhoven and Bunnik to Traiectum (Utrecht), the Old Rhine
        // by Harmelen, Woerden, Bodegraven, Zwammerdam, Alphen, Koudekerk and Leiden to the mouth at Katwijk
        static readonly (double lat, double lon)[] Route =
        {
            (51.972, 5.343), (51.995, 5.310), (52.024, 5.240), (52.060, 5.190), (52.091, 5.120), (52.090, 4.960),
            (52.086, 4.883), (52.085, 4.820), (52.081, 4.750), (52.103, 4.710), (52.129, 4.660), (52.135, 4.600),
            (52.158, 4.490), (52.190, 4.440), (52.205, 4.400),
        };
        static readonly (string name, float at, float length)[] Ports =
        {
            ("Dorestat", 0.0f, 300f), ("Traiectum", 0.24f, 150f), ("the hamlet at the Rhine mouth", 1.0f, 50f),
        };

        public static string Scene(int scale) => $"Assets/Kapman/Scenes/L_Scale_{scale}.unity";
        public static string Dir(int scale) => $"Assets/Kapman/Levels/ScaleTest/{scale}";

        [MenuItem("Kapman/Levels/Build the scale test (four scales)")]
        public static void BuildAll()
        {
            ProjectSetup.Apply();
            StandInCharacters.ConvertMaterials();
            foreach (int s in Scales)
                Build(s);
            var scenes = EditorBuildSettings.scenes.Where(x => !Scales.Any(s => x.path == Scene(s))).ToList();
            foreach (int s in Scales)
                scenes.Add(new EditorBuildSettingsScene(Scene(s), true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ------------------------------------------------------------------ the line

        static float WidthAt(float share) => Mathf.Lerp(WidthTop, WidthMouth, share);

        /// <summary>The route in level metres at a scale: Dorestad at the origin, x east, z north.</summary>
        static List<Vector2> Base(int scale)
        {
            double lat0 = Route[0].lat, lon0 = Route[0].lon, k = Math.Cos(lat0 * Math.PI / 180.0);
            return Route.Select(p => new Vector2((float)((p.lon - lon0) * k * 111320.0 / scale), (float)((p.lat - lat0) * 110540.0 / scale))).ToList();
        }

        static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        static List<Vector2> Resample(List<Vector2> pts, float step)
        {
            var dense = new List<Vector2>();
            for (int i = 0; i < pts.Count - 1; i++)
            {
                Vector2 p0 = pts[Mathf.Max(0, i - 1)], p1 = pts[i], p2 = pts[i + 1], p3 = pts[Mathf.Min(pts.Count - 1, i + 2)];
                int n = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(p1, p2) / (step * 0.5f)));
                for (int j = 0; j < n; j++)
                    dense.Add(CatmullRom(p0, p1, p2, p3, (float)j / n));
            }
            dense.Add(pts[pts.Count - 1]);
            var outp = new List<Vector2> { dense[0] };
            float carry = 0f;
            for (int i = 1; i < dense.Count; i++)
            {
                Vector2 a = dense[i - 1], b = dense[i];
                float seg = Vector2.Distance(a, b), pos = step - carry;
                while (pos <= seg)
                {
                    outp.Add(Vector2.Lerp(a, b, pos / seg));
                    pos += step;
                }
                carry = seg - (pos - step);
            }
            if (Vector2.Distance(outp[outp.Count - 1], dense[dense.Count - 1]) > step * 0.3f)
                outp.Add(dense[dense.Count - 1]);
            return outp;
        }

        /// <summary>The route with a lowland river's own bends laid on it: a wavelength of 12 widths, an amplitude of about
        /// 1.3 widths (the tightest bend then has a radius of nearly three widths), the phase wandering a little.</summary>
        static List<Vector2> Meander(List<Vector2> line)
        {
            float total = 0f;
            for (int i = 1; i < line.Count; i++)
                total += Vector2.Distance(line[i - 1], line[i]);
            var outp = new List<Vector2>(line.Count);
            float s = 0f, phase = 0f;
            for (int i = 0; i < line.Count; i++)
            {
                if (i > 0)
                    s += Vector2.Distance(line[i - 1], line[i]);
                float w = WidthAt(s / Mathf.Max(1f, total));
                if (i > 0)
                    phase += 2f * Mathf.PI * Vector2.Distance(line[i - 1], line[i]) / (12f * w);
                Vector2 a = line[Mathf.Max(0, i - 2)], b = line[Mathf.Min(line.Count - 1, i + 2)];
                Vector2 t = (b - a).normalized, n = new Vector2(-t.y, t.x);
                float amp = 1.3f * w * (0.75f + 0.5f * Mathf.PerlinNoise(s / (30f * w), 3.7f));
                float fadeIn = Mathf.Clamp01(s / (3f * w)) * Mathf.Clamp01((total - s) / (3f * w));   // straight at the ends
                outp.Add(line[i] + n * (amp * fadeIn * Mathf.Sin(phase + 0.6f * Mathf.PerlinNoise(s / (50f * w), 9.1f))));
            }
            return outp;
        }

        // ------------------------------------------------------------------ the level

        public static void Build(int scale)
        {
            string dir = Dir(scale);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), dir));
            var line = Resample(Meander(Resample(Base(scale), Step)), Step);
            // shift into 0..size (MakeRiver's level spans 0..levelSize), the route centred
            Vector2 min = new Vector2(line.Min(p => p.x), line.Min(p => p.y)), max = new Vector2(line.Max(p => p.x), line.Max(p => p.y));
            float span = Mathf.Max(max.x - min.x, max.y - min.y) + 2f * Margin;
            float size = Mathf.Max(512f, Mathf.Ceil(span / 64f) * 64f);
            Vector2 shift = new Vector2(size * 0.5f, size * 0.5f) - (min + max) * 0.5f;
            for (int i = 0; i < line.Count; i++)
                line[i] += shift;
            var cum = new float[line.Count];
            for (int i = 1; i < line.Count; i++)
                cum[i] = cum[i - 1] + Vector2.Distance(line[i - 1], line[i]);
            float length = cum[cum.Length - 1];

            // the distance field: for every cell, the distance to the line and the nearest point's share of the length
            int res = size > 2048f ? 2049 : 1025;
            float cell = size / (res - 1);
            var dist = new float[res * res];
            var share = new float[res * res];
            for (int i = 0; i < dist.Length; i++) dist[i] = 1e9f;
            float reach = WidthMouth * 0.5f + Band + FogDepth + 40f;
            int rc = Mathf.CeilToInt(reach / cell);
            for (int k = 0; k < line.Count; k += 2)
            {
                Vector2 p = line[k];
                int cx = Mathf.RoundToInt(p.x / cell), cz = Mathf.RoundToInt(p.y / cell);
                for (int z = Mathf.Max(0, cz - rc); z <= Mathf.Min(res - 1, cz + rc); z++)
                    for (int x = Mathf.Max(0, cx - rc); x <= Mathf.Min(res - 1, cx + rc); x++)
                    {
                        float dx = x * cell - p.x, dz = z * cell - p.y, d = Mathf.Sqrt(dx * dx + dz * dz);
                        int j = z * res + x;
                        if (d < dist[j]) { dist[j] = d; share[j] = cum[k] / length; }
                    }
            }
            float HeightAt(int j)
            {
                float w = WidthAt(share[j]) * 0.5f, d = dist[j];
                if (d <= w - 12f) return -3.5f;
                if (d <= w) return Mathf.Lerp(-3.5f, -0.3f, (d - (w - 12f)) / 12f);
                if (d <= w + 5f) return Mathf.Lerp(-0.3f, 0.9f, (d - w) / 5f);
                if (d <= w + Band) return 1.2f + 0.5f * Mathf.InverseLerp(w + 5f, w + Band, d);
                return 2.0f;
            }
            const float lo = -6f, range = 12f;
            var td = new TerrainData { heightmapResolution = res, size = new Vector3(size, range, size) };
            var h = new float[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                    h[z, x] = Mathf.Clamp01((HeightAt(z * res + x) - lo) / range);
            td.SetHeights(0, 0, h);
            var layers = new[] { "3_mud", "0_meadow", "12_dry_grass" }.Select(n => AssetDatabase.LoadAssetAtPath<TerrainLayer>(Ground + n + ".terrainlayer")).ToArray();
            if (layers.All(l => l != null))
            {
                td.terrainLayers = layers;
                int ar = 1024;
                td.alphamapResolution = ar;
                var a = new float[ar, ar, 3];
                for (int z = 0; z < ar; z++)
                    for (int x = 0; x < ar; x++)
                    {
                        int j = Mathf.RoundToInt(z * (res - 1f) / (ar - 1f)) * res + Mathf.RoundToInt(x * (res - 1f) / (ar - 1f));
                        float w = WidthAt(share[j]) * 0.5f, d = dist[j];
                        float mud = Mathf.Clamp01(1f - Mathf.Abs(d - w) / 9f), beyond = Mathf.Clamp01((d - (w + Band)) / 20f);
                        a[z, x, 0] = mud;
                        a[z, x, 2] = (1f - mud) * beyond;
                        a[z, x, 1] = 1f - a[z, x, 0] - a[z, x, 2];
                    }
                td.SetAlphamaps(0, 0, a);
            }
            AssetDatabase.DeleteAsset(dir + "/Ground.asset");
            AssetDatabase.CreateAsset(td, dir + "/Ground.asset");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var levelGo = new GameObject($"Level: the scale test at 1:{scale} (Dorestad to the sea, {length / 1000f:0.00} km of river)");
            var info = levelGo.AddComponent<LevelInfo>();
            info.levelId = $"scale_{scale}";
            info.displayName = $"The scale test, 1:{scale}";
            info.kind = LevelKind.RiverPlace;
            info.latitude = 52.08;
            info.longitude = 4.9;
            info.baseWaterLevel = 0f;
            info.WaterLevel = 0f;
            info.realMetresPerUnit = 1f;
            info.shipSpeed = SailSpeed;
            var clock = levelGo.AddComponent<LevelClock>();
            clock.startDay = 200;
            clock.startHour = 15f;
            var weather = levelGo.AddComponent<LevelWeather>();
            weather.kind = WeatherKind.Fair;
            weather.manual = true;
            var water = levelGo.AddComponent<LevelWater>();
            water.tideShare = 0f;
            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            var moon = new GameObject("Moon", typeof(Light)).GetComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.None;
            clock.sun = sun;
            clock.moon = moon;

            var world = new GameObject("World").transform;
            var tgo = Terrain.CreateTerrainGameObject(td);
            tgo.name = "Ground: the river, the band, the land under the fog";
            tgo.transform.SetParent(world, false);
            tgo.transform.position = new Vector3(0f, lo, 0f);
            tgo.isStatic = true;
            var terrain = tgo.GetComponent<Terrain>();
            terrain.heightmapPixelError = 2f;
            terrain.drawInstanced = true;

            // the river: Dorestad's own surface code on a mesh of the channel
            var mesh = ChannelMesh(dist, share, res, cell);
            AssetDatabase.CreateAsset(mesh, dir + "/RiverMesh.asset");
            float SD(Vector2 p)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(p.x / cell), 0, res - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / cell), 0, res - 1);
                int j = z * res + x;
                return WidthAt(share[j]) * 0.5f - dist[j];
            }
            var river = HdrpWater.MakeRiver(world, mesh, 0f, line, (WidthTop + WidthMouth) * 0.5f, size, SD, dir, null, out _, 0.4f, true);
            water.surfaces = new[] { river.transform };
            var driver = river.gameObject.AddComponent<WaterProfileDriver>();
            driver.data = AssetDatabase.LoadAssetAtPath<TextAsset>(WaterProfileSetup.DataPath);
            driver.recordId = "kromme_rijn_dorestad";

            FogCaps(world, dist, share, res, cell, size, dir);
            var portMarks = PlacePorts(world, line, cum, length, dist, share, res, cell);

            // the ship at Dorestad's lowest dam, heading down the stream
            int s0 = Mathf.Min(line.Count - 2, Mathf.RoundToInt(40f / Step));
            Vector2 at = line[s0], dir2 = (line[s0 + 1] - line[s0]).normalized;
            float yaw = Mathf.Atan2(dir2.x, dir2.y) * Mathf.Rad2Deg;
            var shipPrefab = AssetLibrary.Gislinge("Assets/Kapman/Prefabs/Ships/Ship_Gislinge.prefab");
            var ship = (GameObject)PrefabUtility.InstantiatePrefab(shipPrefab, world);
            ship.name = "Your ship: the Gislinge";
            ship.transform.SetPositionAndRotation(new Vector3(at.x, 0f, at.y), Quaternion.Euler(0f, yaw, 0f));
            var sm = ship.AddComponent<ShipMotion>();
            sm.player = true;
            sm.heading = yaw;
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
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 8000f;
            cam.fieldOfView = 40f;
            camGo.AddComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            var orbit = camGo.AddComponent<OrbitCamera>();
            orbit.target = ship.transform;
            camGo.transform.position = ship.transform.position + new Vector3(-30f, 25f, -30f);
            camGo.transform.LookAt(ship.transform.position);
            new GameObject("Test panel").AddComponent<DevHud>();
            GameSetup.AddToScene(false);
            clock.Apply();
            EditorSceneManager.SaveScene(scene, Scene(scale));

            // what the filming test and the page need
            var sb = new StringBuilder();
            sb.Append("{\"scale\":").Append(scale).Append(",\"size\":").Append(F(size)).Append(",\"length\":").Append(F(length))
              .Append(",\"minutes\":").Append(F(length / SailSpeed / 60f)).Append(",\"line\":[");
            for (int i = 0; i < line.Count; i += 5)
                sb.Append(i > 0 ? "," : "").Append('[').Append(F(line[i].x)).Append(',').Append(F(line[i].y)).Append(']');
            sb.Append("],\"ports\":[");
            for (int i = 0; i < portMarks.Count; i++)
                sb.Append(i > 0 ? "," : "").Append("{\"name\":\"").Append(portMarks[i].name).Append("\",\"s\":").Append(F(portMarks[i].s)).Append(",\"length\":").Append(F(portMarks[i].len)).Append('}');
            sb.Append("]}");
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), dir, "scale_info.json"), sb.ToString());
            AssetDatabase.ImportAsset(dir + "/scale_info.json");
            Debug.Log($"Kapman scale test 1:{scale}: {length:0} m of river, {length / SailSpeed / 60f:0.0} minutes at {SailSpeed} m/s; level {size:0} m square.");
        }

        static string F(float v) => v.ToString("0.#", CultureInfo.InvariantCulture);

        /// <summary>The water's mesh: 4 m cells wherever the channel (and a little of its banks) is.</summary>
        static Mesh ChannelMesh(float[] dist, float[] share, int res, float cell)
        {
            float step = 4f;
            int n = Mathf.CeilToInt((res - 1) * cell / step) + 1;
            var index = new int[n * n];
            var verts = new List<Vector3>();
            bool Wet(int gx, int gz)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(gx * step / cell), 0, res - 1), z = Mathf.Clamp(Mathf.RoundToInt(gz * step / cell), 0, res - 1);
                int j = z * res + x;
                return dist[j] < WidthAt(share[j]) * 0.5f + 10f;
            }
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    index[z * n + x] = -1;
                    if (Wet(x, z))
                    {
                        index[z * n + x] = verts.Count;
                        verts.Add(new Vector3(x * step, 0f, z * step));
                    }
                }
            var tris = new List<int>();
            for (int z = 0; z < n - 1; z++)
                for (int x = 0; x < n - 1; x++)
                {
                    int a = index[z * n + x], b = index[z * n + x + 1], c = index[(z + 1) * n + x], d = index[(z + 1) * n + x + 1];
                    if (a < 0 || b < 0 || c < 0 || d < 0)
                        continue;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            var mesh = new Mesh { name = "The river's channel", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// The fog over the inland: two big sheets over the whole level (7 m and 13 m up), each with a soft cloud mask taken
        /// from the distance to the water, clear over the river and its 60 m band and thickening beyond it, so from above the
        /// river runs between fog banks; nothing can fold over the water at a bend.
        /// </summary>
        static void FogCaps(Transform world, float[] dist, float[] share, int res, float cell, float size, string dir)
        {
            var root = new GameObject("Fog over the inland").transform;
            root.SetParent(world, false);
            foreach (var (height, extra, seed) in new[] { (7f, 0f, 11f), (13f, 45f, 37f) })
            {
                const int n = 1024;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, name = "Fog mask" };
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        int gx = Mathf.RoundToInt(x * (res - 1f) / (n - 1f)), gz = Mathf.RoundToInt(y * (res - 1f) / (n - 1f));
                        int j = gz * res + gx;
                        float edge = WidthAt(share[j]) * 0.5f + Band + extra;
                        float wx = x * size / n, wz = y * size / n, cloud = 0f, a = 0.5f, f = 1f / 140f;
                        for (int o = 0; o < 4; o++)
                        {
                            cloud += a * Mathf.PerlinNoise(seed + wx * f, seed * 1.7f + wz * f);
                            a *= 0.5f;
                            f *= 2.1f;
                        }
                        float thick = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge - 5f, edge + 70f, dist[j]));
                        float alpha = Mathf.Clamp01(thick * (0.6f + 0.9f * (cloud - 0.3f)));
                        byte shade = (byte)(232 + 23 * Mathf.Clamp01(cloud));
                        px[y * n + x] = new Color32(shade, shade, (byte)Mathf.Min(255, shade + 4), (byte)(255 * alpha));
                    }
                tex.SetPixels32(px);
                tex.Apply(true);
                string path = $"{dir}/FogMask_{height:0}.png";
                File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.alphaIsTransparency = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.maxTextureSize = 2048;
                imp.SaveAndReimport();
                var mat = new Material(Shader.Find("HDRP/Lit")) { name = $"Fog over the inland ({height:0} m)" };
                mat.SetFloat("_SurfaceType", 1f);
                mat.SetFloat("_BlendMode", 0f);
                mat.SetFloat("_DoubleSidedEnable", 1f);
                mat.SetFloat("_Smoothness", 0f);
                mat.SetFloat("_Metallic", 0f);
                mat.SetColor("_BaseColor", new Color(0.95f, 0.95f, 0.96f, 1f));
                mat.SetTexture("_BaseColorMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                mat.SetFloat("_ZWrite", 0f);
                mat.SetFloat("_TransparentZWrite", 0f);
                UnityEditor.Rendering.HighDefinition.HDShaderUtils.ResetMaterialKeywords(mat);
                AssetDatabase.CreateAsset(mat, $"{dir}/FogMask_{height:0}.mat");
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = $"Fog over the inland, {height:0} m up";
                UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(root, false);
                go.transform.SetPositionAndRotation(new Vector3(size * 0.5f, height, size * 0.5f), Quaternion.Euler(90f, 0f, 0f));
                go.transform.localScale = new Vector3(size, size, 1f);
                var r = go.GetComponent<MeshRenderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        /// <summary>
        /// The fog over the inland: soft sheets beyond each band (two layers, lit by the sun, a cloud texture's alpha fading
        /// in from the band's edge), so from above the river runs between fog banks.
        /// </summary>
        static void FogSheets(Transform world, List<Vector2> line, float[] cum, float length, string dir)
        {
            var tex = FogTexture(dir);
            var mat = new Material(Shader.Find("HDRP/Lit")) { name = "Fog over the inland" };
            mat.SetFloat("_SurfaceType", 1f);
            mat.SetFloat("_BlendMode", 0f);
            mat.SetFloat("_DoubleSidedEnable", 1f);
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_Metallic", 0f);
            mat.SetColor("_BaseColor", new Color(0.93f, 0.94f, 0.95f, 1f));
            mat.SetTexture("_BaseColorMap", tex);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_TransparentZWrite", 0f);
            UnityEditor.Rendering.HighDefinition.HDShaderUtils.ResetMaterialKeywords(mat);
            AssetDatabase.CreateAsset(mat, dir + "/FogSheet.mat");
            var root = new GameObject("Fog over the inland (sheets)").transform;
            root.SetParent(world, false);
            foreach (float side in new[] { -1f, 1f })
                foreach (var (height, inner, tile) in new[] { (7f, 0f, 1f), (12f, 40f, 1.7f) })
                {
                    var verts = new List<Vector3>();
                    var uvs = new List<Vector2>();
                    var tris = new List<int>();
                    for (int i = 0; i < line.Count; i += 3)
                    {
                        float w = WidthAt(cum[i] / length) * 0.5f;
                        Vector2 a = line[Mathf.Max(0, i - 3)], b = line[Mathf.Min(line.Count - 1, i + 3)];
                        Vector2 t = (b - a).normalized, n = new Vector2(-t.y, t.x) * side;
                        Vector2 p0 = line[i] + n * (w + Band - 15f + inner), p1 = line[i] + n * (w + Band + FogDepth);
                        int v = verts.Count;
                        verts.Add(new Vector3(p0.x, height, p0.y));
                        verts.Add(new Vector3(p1.x, height, p1.y));
                        uvs.Add(new Vector2(cum[i] / 150f * tile, 0f));
                        uvs.Add(new Vector2(cum[i] / 150f * tile, 1f));
                        if (v >= 2)
                        {
                            tris.Add(v - 2); tris.Add(v); tris.Add(v - 1);
                            tris.Add(v - 1); tris.Add(v); tris.Add(v + 1);
                        }
                    }
                    var mesh = new Mesh { name = "Fog sheet" };
                    mesh.SetVertices(verts);
                    mesh.SetUVs(0, uvs);
                    mesh.SetTriangles(tris, 0);
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, $"{dir}/FogSheet_{(side < 0 ? "left" : "right")}_{height:0}.asset");
                    var go = new GameObject($"Fog sheet, {(side < 0 ? "left" : "right")} bank, {height:0} m up", typeof(MeshFilter), typeof(MeshRenderer));
                    go.transform.SetParent(root, false);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                    var r = go.GetComponent<MeshRenderer>();
                    r.sharedMaterial = mat;
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    r.receiveShadows = false;
                }
        }

        /// <summary>The fog sheet's texture: white, its alpha a soft cloud noise that rises from nothing at the band's edge
        /// (v = 0) to thick a little way out.</summary>
        static Texture2D FogTexture(string dir)
        {
            const int w = 256, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, true) { wrapModeU = TextureWrapMode.Repeat, wrapModeV = TextureWrapMode.Clamp, name = "Fog sheet" };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / w, v = (float)y / h, n = 0f, a = 0.5f, f = 4f;
                    for (int o = 0; o < 4; o++)
                    {
                        float s0 = Mathf.PerlinNoise(u * f, v * f * 0.6f + 11f), s1 = Mathf.PerlinNoise((u - 1f) * f, v * f * 0.6f + 11f);
                        n += a * Mathf.Lerp(s0, s1, u);
                        a *= 0.5f;
                        f *= 2f;
                    }
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.35f, v));
                    float alpha = Mathf.Clamp01(edge * (0.55f + 0.7f * (n - 0.35f)));
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(255 * alpha));
                }
            tex.SetPixels32(px);
            tex.Apply(true);
            string path = dir + "/FogSheet.png";
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.alphaIsTransparency = true;
            imp.wrapModeU = TextureWrapMode.Repeat;
            imp.wrapModeV = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>The ports along the left bank at their importance (true size), with stand-in people on the jetties.</summary>
        static List<(string name, float s, float len)> PlacePorts(Transform world, List<Vector2> line, float[] cum, float length,
                                                                  float[] dist, float[] share, int res, float cell)
        {
            var marks = new List<(string, float, float)>();
            var people = StandInCharacters.People();
            var root = new GameObject("Ports (at their importance, true size)").transform;
            root.SetParent(world, false);
            int person = 0;
            var rnd = new System.Random(793);
            GameObject Put(string prefab, Vector2 at, float yaw, float y)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
                if (p == null)
                    return null;
                var g = (GameObject)PrefabUtility.InstantiatePrefab(p, root);
                g.transform.SetPositionAndRotation(new Vector3(at.x, y, at.y), Quaternion.Euler(0f, yaw, 0f));
                return g;
            }
            void Person(Vector2 at, float y)
            {
                if (people.Length == 0)
                    return;
                var p = people[person++ % people.Length];
                var g = (GameObject)PrefabUtility.InstantiatePrefab(p, root);
                g.transform.SetPositionAndRotation(new Vector3(at.x, y, at.y), Quaternion.Euler(0f, rnd.Next(360), 0f));
                var rs = g.GetComponentsInChildren<Renderer>();
                if (rs.Length > 0)
                {
                    var b = rs[0].bounds;
                    foreach (var r in rs) b.Encapsulate(r.bounds);
                    if (b.size.y > 0.01f)
                        g.transform.localScale *= (1.6f + 0.2f * (float)rnd.NextDouble()) / b.size.y;
                }
            }
            foreach (var (name, at, len) in Ports)
            {
                float s0 = Mathf.Clamp(at * length - (at >= 1f ? len : 0f), 20f, Mathf.Max(20f, length - len - 10f));
                marks.Add((name, s0, len));
                for (float s = s0; s <= s0 + len; s += 18f)
                {
                    int i = Array.FindIndex(cum, c => c >= s);
                    if (i < 0) break;
                    Vector2 a = line[Mathf.Max(0, i - 2)], b = line[Mathf.Min(line.Count - 1, i + 2)];
                    Vector2 t = (b - a).normalized, n = new Vector2(-t.y, t.x);
                    float w = WidthAt(cum[i] / length) * 0.5f;
                    float yaw = Mathf.Atan2(-n.x, -n.y) * Mathf.Rad2Deg;                 // facing the water
                    Put("Gangway", line[i] + n * (w - 2f), yaw + 90f, 0.9f);
                    Put("Revetment", line[i] + n * (w + 2f), yaw, 1.0f);
                    Put((int)(s / 18f) % 2 == 0 ? "Storehouse_4.6x8_v1" : "Warehouse_4.6x10", line[i] + n * (w + 16f), yaw + 90f, 1.4f);
                    if (len >= 150f)
                        Put("House_5x12_v0", line[i] + n * (w + 40f), yaw + 90f, 1.6f);
                    Put("Barrels", line[i] + n * (w + 7f) + t * 4f, yaw + 30f, 1.0f);
                    Person(line[i] + n * (w + 4f) + t * (float)(rnd.NextDouble() * 6.0 - 3.0), 1.0f);
                    if (len >= 150f)
                        Person(line[i] + n * (w + 9f) - t * 3f, 1.1f);
                }
            }
            return marks;
        }
    }
}
