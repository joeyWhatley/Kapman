using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Levels;
using Kapman.Levels.Water;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// HDRP WATER FOR THE LEVELS - makes Unity's HDRP Water System surfaces for the level builders, the most realistic
    /// water Unity can do, and everything that goes with them:
    ///   - MakeRiver: a River surface drawn with the level's own water mesh (the river at its true width, from the
    ///     signed distance to the banks), a current map baked from the channel line (the stream along it, full in
    ///     mid-channel, slack over the harbour's shoal and in the pools), the brown-green of the silty Kromme Rijn and
    ///     Lek, the stream's drifting foam, and the river's colour moving with the weather. The flood is the same
    ///     surface drawing a sheet over the whole level (LevelWater swaps the meshes). No water mask is needed for the
    ///     shoal between the dams: the terrain stands above the surface there, so it shows as mud (wet just above the
    ///     water line, by WaterLine), and the mesh ends 8 m under the banks.
    ///   - MakeSea: an infinite ocean with the first build's demo-2 colours (the look the user picked), wind-driven
    ///     swell and whitecaps from the level's weather (HdrpSea), calm shallows, shore foam and surf (ShoreWater).
    ///   - DressShip: the hull's water excluder (a lid made from her planking, so the water isn't drawn inside her)
    ///     and, for the player's ship, her wake, foam, strikes and spray (ShipWake).
    ///   - AddWaterLine / AddCalmReflections: wet things in the water; mirror-calm reflections.
    ///   - EnableWater: the HDRP asset's water switches (CPU simulation for the ships, decals, exclusion, SSR).
    /// The materials and textures go in Assets/Kapman/Water. Performance: the river simulates at the project's
    /// 128 resolution with one band plus ripples and a 1024-texel decal region; the ocean adds its second band.
    /// </summary>
    public static class HdrpWater
    {
        public const string Dir = "Assets/Kapman/Water";
        const string ExclusionMaterialPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Material/MaterialWaterExclusion.mat";
        const string DecalSampleShader = "HDRP/Water/Water Decal Sample";

        // ------------------------------------------------------------------------------------------ the HDRP asset

        /// <summary>
        /// Switches on water in every HDRP asset: the water system, exclusion (the hull lids), water decals (wakes,
        /// foam, surf; more than HDRP's 48 of them), script interactions through the CPU simulation at full resolution
        /// (the ships then feel the crests the screen shows), 128 simulation resolution (60 fps at 1080p), decals for
        /// the water line, and screen-space reflections including transparents (the water is a transparent).
        /// Project Settings > Graphics > HDRP > Water System's "mask and current in water decals" must stay OFF: the
        /// river's current map and the shore's calm-water mask are textures on the surface.
        /// </summary>
        [MenuItem("Kapman/Setup/Switch on HDRP water", priority = 40)]
        public static void EnableWater()
        {
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:HDRenderPipelineAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null)
                    continue;
                var so = new SerializedObject(asset);
                void Bool(string name, bool value)
                {
                    var p = so.FindProperty("m_RenderPipelineSettings." + name);
                    if (p != null)
                        p.boolValue = value;
                }
                Bool("supportWater", true);
                Bool("supportWaterExclusion", true);
                Bool("supportWaterDecals", true);
                Bool("waterFullCPUSimulation", true);
                Bool("supportDecals", true);
                Bool("supportSSR", true);
                Bool("supportSSRTransparent", true);
                var decalCount = so.FindProperty("m_RenderPipelineSettings.maximumWaterDecalCount");
                if (decalCount != null && decalCount.intValue < 160)
                    decalCount.intValue = 160;
                var mode = so.FindProperty("m_RenderPipelineSettings.waterScriptInteractionsMode");
                if (mode != null)
                    mode.enumValueIndex = (int)WaterScriptInteractionsMode.CPUSimulation;
                var res = so.FindProperty("m_RenderPipelineSettings.waterSimulationResolution");
                if (res != null)
                    res.intValue = (int)WaterSimulationResolution.Medium128;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman HDRP water: switched on in {n} HDRP asset(s).");
        }

        // ------------------------------------------------------------------------------------------ the river

        /// <summary>A river's channel: its centre line, its width (m) and its stream's speed in mid-channel (m/s).</summary>
        public sealed class Channel
        {
            public List<Vector2> line;
            public float width = 60f;
            public float speed = 0.4f;
            [Tooltip("The line's first point is the upstream end (the stream runs from it). Off: the last point is.")]
            public bool firstPointUpstream = true;
        }

        /// <summary>
        /// The river's HDRP surface, drawn with the level's water mesh. channel: the river's centre line (level
        /// metres); width its width; levelSize the level's extent (it spans 0..levelSize in x and z); waterDistance the
        /// signed distance to the banks (+ in the water, as DorestadBuilder.Grid.SD); floodMesh the sheet over the whole
        /// level for floods (floodSheet comes back for LevelWater.floodSheet). Returns the surface (LevelWater.surfaces).
        /// </summary>
        public static WaterSurface MakeRiver(Transform parent, Mesh riverMesh, float waterLevel, List<Vector2> channel, float width,
                                             float levelSize, Func<Vector2, float> waterDistance, string assetDir, Mesh floodMesh,
                                             out Transform floodSheet, float streamSpeed = 0.4f, bool firstPointUpstream = true)
        {
            var one = new Channel { line = channel, width = width, speed = streamSpeed, firstPointUpstream = firstPointUpstream };
            return MakeRiver(parent, riverMesh, waterLevel, new[] { one }, levelSize, waterDistance, assetDir, floodMesh, out floodSheet);
        }

        /// <summary>The same with several channels (the Kromme Rijn and the Lek, each with its own width and stream).</summary>
        public static WaterSurface MakeRiver(Transform parent, Mesh riverMesh, float waterLevel, IList<Channel> channels,
                                             float levelSize, Func<Vector2, float> waterDistance, string assetDir, Mesh floodMesh,
                                             out Transform floodSheet)
        {
            EnsureFolder(Dir);
            EnsureFolder(assetDir);
            var go = new GameObject("Water: the river (HDRP)");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, waterLevel, 0f);
            var surface = go.AddComponent<WaterSurface>();
            surface.surfaceType = WaterSurfaceType.River;
            surface.geometryType = WaterGeometryType.Custom;
            surface.meshRenderers = new List<MeshRenderer> { MeshChild(go.transform, "River mesh (drawn by the water surface)", riverMesh) };
            floodSheet = floodMesh != null ? MeshChild(go.transform, "Flood sheet (drawn by the water surface in a flood)", floodMesh).transform : null;

            // the ships ride it: HDRP's CPU copy of the waves (the ripples are centimetres: not felt)
            surface.scriptInteractions = true;
            surface.cpuEvaluateRipples = false;
            surface.timeMultiplier = 1f;
            // a lowland river's water: a gentle agitation running with the stream, the wind's ripples on top
            surface.repetitionSize = 75f;
            surface.largeWindSpeed = 6f;
            surface.largeChaos = 0.9f;
            surface.largeBand0FadeMode = WaterSurface.FadeMode.Custom;
            surface.largeBand0FadeStart = 400f;
            surface.largeBand0FadeDistance = 800f;
            surface.ripples = true;
            surface.ripplesMotionMode = WaterPropertyOverrideMode.Inherit;
            surface.ripplesWindSpeed = 8f;
            surface.ripplesChaos = 0.2f;
            // the brown-green of the silty Kromme Rijn and Lek: cloudy, the bed showing only in the shallows
            // (Research/water_look.md: Secchi 0.4-0.8 m in summer, olive-brown; the sky must not light it up at dusk)
            surface.refractionColor = new Color(0.30f, 0.28f, 0.16f);
            surface.scatteringColor = new Color(0.20f, 0.19f, 0.10f);
            surface.absorptionDistance = 0.6f;
            surface.maxRefractionDistance = 0.4f;
            surface.ambientScattering = 0.15f;
            surface.heightScattering = 0.2f;
            surface.displacementScattering = 0.1f;
            surface.directLightTipScattering = 0.6f;
            surface.directLightBodyScattering = 0.8f;
            surface.caustics = false;                                      // too turbid for caustics
            surface.causticsIntensity = 0.12f;
            surface.causticsBand = 1;
            surface.causticsPlaneBlendDistance = 1f;
            surface.startSmoothness = 0.95f;
            surface.endSmoothness = 0.9f;
            surface.smoothnessFadeStart = 100f;
            surface.smoothnessFadeDistance = 500f;
            surface.tessellation = false;                                   // the river mesh is already 5 m cells
            // foam and heights for the wake, the strikes, the drifting foam (a quarter metre a texel round the ship)
            surface.foam = true;
            surface.foamResolution = WaterSurface.WaterDecalRegionResolution.Resolution1024;
            surface.foamPersistenceMultiplier = 1f;
            surface.simulationFoamAmount = 0f;                              // no whitecaps on a river
            surface.deformation = true;
            surface.deformationRes = WaterSurface.WaterDecalRegionResolution.Resolution1024;
            surface.decalRegionSize = new Vector2(256f, 256f);
            surface.underWater = false;

            // the stream
            var current = go.AddComponent<RiverCurrent>();
            current.currentMap = BakeCurrentMap(channels, levelSize, waterDistance, assetDir, out float extent);
            current.mapExtent = extent;
            current.speed = channels.Count > 0 ? channels.Max(c => c.speed) : 0.4f;
            current.foamMaterial = WaterDecalMaterial("Ship_Foam", 4, foam: true, deform: false);
            current.Apply();
            // the weather on it, and its colour under the weather
            var sea = go.AddComponent<HdrpSea>();
            sea.swellShare = 0f;
            sea.riverAgitation = 6f;
            go.AddComponent<RiverTint>();
            return surface;
        }

        /// <summary>
        /// The current map: red and green the stream's direction (x east, z north; 0.5 = none), blue 1 where there is
        /// water, alpha the stream's share of the full speed. HDRP 17.6 reads the direction only (one speed for the whole
        /// surface); RiverCurrent reads the alpha for the drifting foam and anything pushed. The map is centred on the
        /// surface's origin and covers twice the level (HDRP's CPU and GPU disagree on the sign of an offset), 1024
        /// texels across: 10 m a texel on Dorestad. Saved as a readable texture asset beside the level.
        /// </summary>
        public static Texture2D BakeCurrentMap(IList<Channel> channels, float levelSize, Func<Vector2, float> waterDistance, string assetDir, out float extent)
        {
            const int n = 1024;
            extent = 2f * levelSize;
            float texel = extent / n;
            float topSpeed = Mathf.Max(0.01f, channels.Count > 0 ? channels.Max(c => c.speed) : 0.01f);
            var px = new Color32[n * n];
            var land = new Color32(128, 128, 0, 0);
            // every channel's segments, oriented downstream
            var segs = new List<(Vector2 a, Vector2 b, int channel)>();
            for (int c = 0; c < channels.Count; c++)
            {
                var line = channels[c].line;
                if (line == null || line.Count < 2)
                    continue;
                for (int i = 0; i + 1 < line.Count; i++)
                    segs.Add(channels[c].firstPointUpstream ? (line[i], line[i + 1], c) : (line[i + 1], line[i], c));
            }
            for (int j = 0; j < n; j++)
            {
                float z = (j + 0.5f) * texel - extent * 0.5f;
                for (int i = 0; i < n; i++)
                {
                    float x = (i + 0.5f) * texel - extent * 0.5f;
                    int idx = j * n + i;
                    if (x < 0f || z < 0f || x > levelSize || z > levelSize)
                    {
                        px[idx] = land;
                        continue;
                    }
                    var p = new Vector2(x, z);
                    float sd = waterDistance(p);
                    if (sd <= 0f || segs.Count == 0)
                    {
                        px[idx] = land;
                        continue;
                    }
                    // the nearest channel segment: its tangent is the stream's way
                    float best = float.MaxValue;
                    Vector2 tangent = Vector2.right;
                    int which = 0;
                    foreach (var s in segs)
                    {
                        Vector2 v = s.b - s.a;
                        float ll = v.sqrMagnitude;
                        float t = ll > 0f ? Mathf.Clamp01(Vector2.Dot(p - s.a, v) / ll) : 0f;
                        float d = (s.a + v * t - p).sqrMagnitude;
                        if (d < best)
                        {
                            best = d;
                            tangent = ll > 0f ? v / Mathf.Sqrt(ll) : Vector2.right;
                            which = s.channel;
                        }
                    }
                    float dist = Mathf.Sqrt(best);
                    var ch = channels[which];
                    // full in mid-channel (far from the banks), slack beyond the channel's band: the harbour's shoal
                    // between the dams, the pools, the old channel scars
                    float midChannel = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.4f * ch.width, sd));
                    float inBand = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f * ch.width, 0.6f * ch.width + 40f, dist));
                    float share = midChannel * inBand * (ch.speed / topSpeed);
                    px[idx] = new Color32(WaterDecalKit.B(tangent.x * 0.5f + 0.5f), WaterDecalKit.B(tangent.y * 0.5f + 0.5f), 255, WaterDecalKit.B(share));
                }
            }
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true)
                { name = "River current map", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            tex.SetPixels32(px);
            tex.Apply(false, false);                                        // stays readable: RiverCurrent samples it
            string path = assetDir + "/River_current.asset";
            var old = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (old != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ------------------------------------------------------------------------------------------ the sea

        /// <summary>
        /// The sea: an infinite HDRP ocean with the first build's demo-2 colours (the look the user picked), its swell,
        /// ripples and whitecaps from the level's wind (HdrpSea), calm shallows, shore foam and surf round the ship
        /// (ShoreWater, from the scene's terrain). Returns the surface (LevelWater.surfaces).
        /// </summary>
        public static WaterSurface MakeSea(Transform parent, float level, float levelSize, string assetDir)
        {
            EnsureFolder(Dir);
            var go = new GameObject("Water: the sea (HDRP ocean)");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(levelSize * 0.5f, level, levelSize * 0.5f);
            var surface = go.AddComponent<WaterSurface>();
            surface.surfaceType = WaterSurfaceType.OceanSeaLake;
            surface.geometryType = WaterGeometryType.Infinite;
            surface.scriptInteractions = true;
            surface.cpuEvaluateRipples = false;
            surface.timeMultiplier = 1f;
            surface.repetitionSize = 500f;
            surface.largeChaos = 0.6f;
            surface.ripples = true;
            surface.ripplesChaos = 0.8f;
            surface.caustics = false;                                       // no bed to see at sea
            surface.foam = true;
            surface.foamResolution = WaterSurface.WaterDecalRegionResolution.Resolution1024;
            surface.foamPersistenceMultiplier = 1f;
            surface.simulationFoamAmount = 0.55f;
            surface.deformation = true;
            surface.deformationRes = WaterSurface.WaterDecalRegionResolution.Resolution1024;
            surface.decalRegionSize = new Vector2(512f, 512f);
            // demo 2: calmer and brighter, the teal of the first build's miniature sea
            surface.refractionColor = new Color(0.1f, 0.62f, 0.66f);
            surface.scatteringColor = new Color(0.04f, 0.42f, 0.38f);
            surface.absorptionDistance = 9f;
            surface.startSmoothness = 0.97f;
            surface.endSmoothness = 0.88f;
            surface.underWater = false;
            var sea = go.AddComponent<HdrpSea>();
            sea.swellShare = 0.6f;
            // the sea notices the land: calm in shallows and sounds, foam along the shore, surf on the beaches
            var shore = go.AddComponent<ShoreWater>();
            shore.surface = surface;
            shore.foamMaterial = WaterDecalMaterial("Shore_Foam", 4, foam: true, deform: false);
            shore.surfMaterial = SurfMaterial();
            return surface;
        }

        // ------------------------------------------------------------------------------------------ the ships

        /// <summary>
        /// Dresses a ship for HDRP's water: a water excluder lid made from her hull (the water isn't drawn inside her),
        /// and, with wake on, her wake, foam, strikes and spray (ShipWake) with its materials. The lid mesh is saved
        /// once per hull mesh in Assets/Kapman/Water and shared by every boat with that hull.
        /// </summary>
        public static void DressShip(GameObject ship, bool wake)
        {
            EnsureFolder(Dir);
            var motion = ship.GetComponent<ShipMotion>();
            float waterlineY = motion != null ? -motion.waterline : 0f;
            var hull = HullMesh(ship.transform);
            if (hull != null)
                AddExcluder(ship.transform, hull, waterlineY);
            if (!wake)
                return;
            var w = ship.GetComponent<ShipWake>();
            if (w == null)
                w = ship.AddComponent<ShipWake>();
            w.ship = motion;
            MeasureWaterline(ship.transform, waterlineY, motion, out w.bowZ, out w.sternZ, out w.halfBeam);
            var oar = ship.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Rudder");
            if (oar != null)
            {
                w.steeringOar = oar;
                w.oarBlade = MeasureOarBlade(oar, ship.transform, waterlineY);
            }
            w.waveMaterial = WaterDecalMaterial("Ship_Waves", 4, foam: true, deform: true);
            w.foamMaterial = WaterDecalMaterial("Ship_Foam", 4, foam: true, deform: false);
            w.sprayMaterial = SprayMaterial();
        }

        /// <summary>The mesh her lid is made from: her Planking (the Gislinge), else her Hull (the Utrecht boats), else nothing.</summary>
        static MeshFilter HullMesh(Transform ship)
        {
            var filters = ship.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToList();
            return filters.FirstOrDefault(f => f.name == "Planking") ?? filters.FirstOrDefault(f => f.name == "Hull");
        }

        /// <summary>
        /// HDRP's water excluder: a lid over the hull, so the water isn't drawn inside the boat. The lid follows her
        /// sheer just inside the planking and below the gunwale (made from her hull mesh), so it never cuts the water
        /// away outside her. HDRP's own way (as its menu makes one): a child renderer with its exclusion material,
        /// which only marks the stencil where the lid is seen.
        /// </summary>
        static void AddExcluder(Transform ship, MeshFilter hull, float waterlineY)
        {
            string lidPath = Dir + "/" + Safe(hull.sharedMesh.name) + "_Lid.asset";
            var lid = AssetDatabase.LoadAssetAtPath<Mesh>(lidPath);
            if (lid == null)
            {
                lid = MakeHullLid(ship, hull, waterlineY);
                AssetDatabase.CreateAsset(lid, lidPath);
            }
            var old = ship.Find("Water excluder (keeps the water out of the hull)");
            if (old != null)
                UnityEngine.Object.DestroyImmediate(old.gameObject);
            var holder = new GameObject("Water excluder (keeps the water out of the hull)");
            holder.transform.SetParent(ship, false);
            var excluder = holder.AddComponent<WaterExcluder>();
            var renderer = new GameObject("Water Excluder Renderer", typeof(MeshFilter), typeof(MeshRenderer));
            renderer.hideFlags = HideFlags.NotEditable;
            renderer.transform.SetParent(holder.transform, false);
            renderer.GetComponent<MeshFilter>().sharedMesh = lid;
            var exclusion = renderer.GetComponent<MeshRenderer>();
            exclusion.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ExclusionMaterialPath);
            exclusion.rayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.Off;
            exclusion.shadowCastingMode = ShadowCastingMode.Off;
            exclusion.receiveShadows = false;
            var so = new SerializedObject(excluder);
            var rendererProp = so.FindProperty("m_ExclusionRenderer");
            if (rendererProp != null)
                rendererProp.objectReferenceValue = renderer;
            so.ApplyModifiedPropertiesWithoutUndo();
            excluder.SetExclusionMesh(lid);
        }

        /// <summary>
        /// The lid: rows of the hull's vertices 5 cm deep along the keel give the sheer strake's top and the hull's
        /// width there; the lid sits 5 cm below the top and 6 cm inside the planking, in the ship's own space.
        /// </summary>
        static Mesh MakeHullLid(Transform ship, MeshFilter hull, float waterlineY)
        {
            var toShip = ship.worldToLocalMatrix * hull.transform.localToWorldMatrix;
            var points = hull.sharedMesh.vertices.Select(v => toShip.MultiplyPoint3x4(v)).OrderBy(p => p.z).ToList();
            const float row = 0.05f, drop = 0.05f, inset = 0.06f;
            var sections = new List<Vector3>();                              // (half-breadth, height, z)
            for (int start = 0; start < points.Count;)
            {
                int end = start;
                while (end < points.Count && points[end].z < points[start].z + row)
                    end++;
                float top = float.MinValue, zSum = 0f;
                for (int i = start; i < end; i++)
                {
                    top = Mathf.Max(top, points[i].y);
                    zSum += points[i].z;
                }
                float half = 0f;
                for (int i = start; i < end; i++)
                    if (points[i].y > top - 0.08f)
                        half = Mathf.Max(half, Mathf.Abs(points[i].x));
                sections.Add(new Vector3(Mathf.Max(0f, half - inset), Mathf.Max(top - drop, waterlineY + 0.02f), zSum / (end - start)));
                start = end;
            }
            // a row with only the lower planks in it (their ends near the keel) isn't the sheer: leave it out
            sections = sections.Where(s => !sections.Any(o => Mathf.Abs(o.z - s.z) <= 0.3f &&
                                                             o.y - s.y > 0.06f + 0.4f * Mathf.Abs(o.z - s.z))).ToList();
            var v3 = new List<Vector3>();
            var tris = new List<int>();
            foreach (var s in sections)
            {
                v3.Add(new Vector3(-s.x, s.y, s.z));
                v3.Add(new Vector3(s.x, s.y, s.z));
            }
            for (int i = 0; i + 1 < sections.Count; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                tris.AddRange(new[] { a, c, b, b, c, d });                     // clockwise seen from above (HDRP culls the back)
            }
            var mesh = new Mesh { name = Safe(hull.sharedMesh.name) + "_Lid" };
            mesh.SetVertices(v3);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Her waterline from her hull meshes: where it ends at the bow and the stern, and its half-breadth.</summary>
        static void MeasureWaterline(Transform ship, float waterlineY, ShipMotion motion, out float bowZ, out float sternZ, out float halfBeam)
        {
            bowZ = float.MinValue;
            sternZ = float.MaxValue;
            halfBeam = 0f;
            foreach (float band in new[] { 0.05f, 0.15f })
            {
                foreach (var mf in ship.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null || (mf.name != "Planking" && mf.name != "Stems" && mf.name != "Hull"))
                        continue;
                    var toShip = ship.worldToLocalMatrix * mf.transform.localToWorldMatrix;
                    foreach (var v in mf.sharedMesh.vertices)
                    {
                        var p = toShip.MultiplyPoint3x4(v);
                        if (Mathf.Abs(p.y - waterlineY) > band)
                            continue;
                        bowZ = Mathf.Max(bowZ, p.z);
                        sternZ = Mathf.Min(sternZ, p.z);
                        halfBeam = Mathf.Max(halfBeam, Mathf.Abs(p.x));
                    }
                }
                if (halfBeam > 0f)
                    break;
            }
            if (halfBeam <= 0.05f || bowZ - sternZ < 1f)
            {
                float length = motion != null ? motion.length : 7.7f, beam = motion != null ? motion.beam : 1.5f;
                bowZ = length * 0.5f - 0.3f;
                sternZ = -(length * 0.5f - 0.3f);
                halfBeam = beam * 0.5f;
            }
        }

        /// <summary>Where the steering oar's blade cuts her waterline (the middle of the blade there), in the oar's own space.</summary>
        static Vector3 MeasureOarBlade(Transform oar, Transform ship, float waterlineY)
        {
            var mf = oar.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return Vector3.zero;
            var toShip = ship.worldToLocalMatrix * oar.localToWorldMatrix;
            Vector3 sum = Vector3.zero, lowest = Vector3.zero;
            float lowY = float.MaxValue;
            int n = 0;
            foreach (var v in mf.sharedMesh.vertices)
            {
                var p = toShip.MultiplyPoint3x4(v);
                if (p.y < lowY)
                {
                    lowY = p.y;
                    lowest = v;
                }
                if (Mathf.Abs(p.y - waterlineY) > 0.08f)
                    continue;
                sum += v;
                n++;
            }
            return n > 0 ? sum / n : lowest;
        }

        // ------------------------------------------------------------------------------------------ the water line, the calm

        /// <summary>Wets what stands in the water (WaterLine): on the level root, say. Makes its decal material and texture.</summary>
        public static WaterLine AddWaterLine(GameObject on)
        {
            EnsureFolder(Dir);
            var line = on.GetComponent<WaterLine>();
            if (line == null)
                line = on.AddComponent<WaterLine>();
            line.wet = WetMaterial(Gradient());
            return line;
        }

        /// <summary>Mirror-calm reflections on glassy water (CalmReflections), given the sky-and-fog volume.</summary>
        public static CalmReflections AddCalmReflections(GameObject on, Volume skyAndFog)
        {
            var calm = on.GetComponent<CalmReflections>();
            if (calm == null)
                calm = on.AddComponent<CalmReflections>();
            calm.volume = skyAndFog;
            return calm;
        }

        // ------------------------------------------------------------------------------------------ materials

        /// <summary>
        /// A water decal material asset from HDRP's "Water Decal Sample" shader. Type 0 disk, 1 rectangle, 2 bow wave,
        /// 3 shore wave, 4 texture (the textures are set at runtime: ShipWake, ShoreWater, RiverCurrent). A texture's red
        /// is the height in metres.
        /// </summary>
        public static Material WaterDecalMaterial(string materialName, int type, bool foam, bool deform)
        {
            EnsureFolder(Dir);
            string path = Dir + "/" + materialName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find(DecalSampleShader);
                if (shader == null)
                    throw new InvalidOperationException("Kapman HDRP water: the shader '" + DecalSampleShader + "' is missing (HDRP's water package resources).");
                material = new Material(shader) { name = materialName };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetFloat("_TYPE", type);
            material.SetFloat("_AffectFoam", foam ? 1f : 0f);
            material.SetFloat("_AffectDeformation", deform ? 1f : 0f);
            if (type == 1)
                material.SetVector("_Blend_Distance", new Vector4(0.35f, 0.25f, 0f, 0f));      // soft edges
            if (type == 2)
                material.SetFloat("_Elevation", 0.5f);
            if (type == 4)
            {
                material.SetTexture(WaterDecalKit.DeformationTexture, Texture2D.blackTexture);
                material.SetFloat("_Remap_Min", 0f);                                         // heights as they are
                material.SetFloat("_Remap_Max", 1f);
            }
            MaterialEditor.ApplyMaterialPropertyDrawers(material);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// The surf: HDRP's shore wave (the Water Decal Sample's type 3). Its waves roll along the decal's x, from the
        /// sea end to the land end, steepen, break white over the last part and run out. A wave every 10 m (every second
        /// one of HDRP's bells), rolling in at about 2.4 m/s on a 40 m patch.
        /// </summary>
        public static Material SurfMaterial()
        {
            var material = WaterDecalMaterial("Shore_Surf", 3, foam: true, deform: true);
            material.SetFloat("_Wave_Length", 0.25f);
            material.SetFloat("_Skipped_Waves", 2f);
            material.SetFloat("_Wave_Speed", 0.12f);
            material.SetFloat("_Wave_Offset", 0f);
            material.SetVector("_Wave_Blend", new Vector4(0.2f, 0.8f, 0f, 0f));
            material.SetVector("_Breaking_Range", new Vector4(0.55f, 0.85f, 0f, 0f));
            material.SetVector("_Deep_Foam_Range", new Vector4(0.45f, 0.85f, 0f, 0f));
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>The spray's droplets: a transparent HDRP/Lit, a soft white dot set at runtime.</summary>
        public static Material SprayMaterial()
        {
            EnsureFolder(Dir);
            string path = Dir + "/Spray.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("HDRP/Lit")) { name = "Spray" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", new Color(0.93f, 0.96f, 0.98f, 0.7f));
            material.SetFloat("_Smoothness", 0.6f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_EnableFogOnTransparent", 1f);
            HDMaterial.SetSurfaceType(material, true);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>The wet decal: darker and shining, its alpha from the gradient (opaque below, clear at the top).</summary>
        static Material WetMaterial(Texture2D gradient)
        {
            string path = Dir + "/WaterLine_Wet.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("HDRP/Decal")) { name = "WaterLine_Wet" };
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetTexture("_BaseColorMap", gradient);
            m.SetColor("_BaseColor", new Color(0.13f, 0.12f, 0.11f, 0.62f));
            m.SetFloat("_DecalBlend", 1f);
            m.SetFloat("_Smoothness", 0.86f);
            m.SetFloat("_MaskBlendSrc", 0f);                                 // the texture's alpha says how wet
            m.SetFloat("_NormalBlendSrc", 0f);
            m.SetFloat("_AffectAlbedo", 1f);
            m.SetFloat("_AffectNormal", 0f);
            m.SetFloat("_AffectSmoothness", 1f);
            m.SetFloat("_AffectMetal", 0f);
            m.SetFloat("_AffectAO", 0f);
            m.enableInstancing = true;
            HDMaterial.ValidateMaterial(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Opaque up to 90% of its height, fading to clear at the top (WaterLine's box is ten washes tall).</summary>
        static Texture2D Gradient()
        {
            string path = Dir + "/WaterLine_Gradient.png";
            if (!File.Exists(path))
            {
                const int w = 4, h = 256;
                var px = new Color32[w * h];
                for (int j = 0; j < h; j++)
                {
                    float v = (j + 0.5f) / h;
                    float a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.9f, 1f, v));
                    for (int i = 0; i < w; i++)
                        px[j * w + i] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
                t.SetPixels32(px);
                File.WriteAllBytes(path, t.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(t);
                AssetDatabase.ImportAsset(path);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.textureType = TextureImporterType.Default;
                imp.sRGBTexture = false;
                imp.alphaIsTransparency = false;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.mipmapEnabled = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ------------------------------------------------------------------------------------------ bits

        /// <summary>A child holding a mesh for the water surface to draw: its own renderer stays off (HDRP draws the mesh with the water shader).</summary>
        static MeshRenderer MeshChild(Transform parent, string name, Mesh mesh)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterials = new Material[0];
            r.enabled = false;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return r;
        }

        static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        static string Safe(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrEmpty(name) ? "Hull" : name;
        }
    }
}
