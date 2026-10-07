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
    /// THE FREE ASSETS - brings the CC0 textures and models (Poly Haven, ambientCG) from G:\kapman_unity\data\raw\assets
    /// into the project as HDRP materials, terrain layers and prefabs: textures at 1K, the scanned trees baked into one
    /// mesh each with Unity's mesh levels of detail. Materials made under URP are brought over to HDRP/Lit or HDRP/Unlit
    /// when they are met again (Upgrade), or all at once (Kapman > Setup > Upgrade the URP materials to HDRP).
    /// Credits: ThirdParty/CREDITS.md.
    /// </summary>
    public static class AssetLibrary
    {
        public const string Raw = @"G:\kapman_unity\data\raw\assets";
        public const string Root = "Assets/Kapman/ThirdParty";

        static string Full(string assetPath) => Path.Combine(Directory.GetCurrentDirectory(), assetPath);

        /// <summary>Copies a file into the project (once) and imports it.</summary>
        public static string Bring(string source, string assetPath)
        {
            if (!File.Exists(Full(assetPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Full(assetPath)));
                File.Copy(source, Full(assetPath));
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            }
            return assetPath;
        }

        static Texture2D Tex(string source, string assetPath, int maxSize, bool normal)
        {
            Bring(source, assetPath);
            var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            bool dirty = false;
            if (imp.maxTextureSize != maxSize) { imp.maxTextureSize = maxSize; dirty = true; }
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (imp.textureType != type) { imp.textureType = type; dirty = true; }
            if (dirty)
                imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        /// <summary>A ground or building texture set: colour and normal (Poly Haven or ambientCG names).</summary>
        public static (Texture2D diff, Texture2D normal) Surface(string source, string slug, int maxSize = 1024)
        {
            string dir = Path.Combine(Raw, source, slug);
            string diff, nrm;
            if (source == "ambientcg")
            {
                diff = Directory.GetFiles(dir, "*_Color.jpg").FirstOrDefault();
                nrm = Directory.GetFiles(dir, "*_NormalGL.jpg").FirstOrDefault();
            }
            else
            {
                diff = Directory.GetFiles(dir, slug + "_diff_*.jpg").Concat(Directory.GetFiles(dir, slug + "_diffuse_*.jpg")).Concat(Directory.GetFiles(dir, slug + "_diff_*.png")).FirstOrDefault();
                nrm = Directory.GetFiles(dir, slug + "_nor_gl_*.jpg").Concat(Directory.GetFiles(dir, slug + "_nor_gl_*.png")).Concat(Directory.GetFiles(dir, slug + "_nor_gl_*.exr")).FirstOrDefault();
                // a Poly Haven model keeps its textures in a subfolder; a surface can be taken from there too (stone, barrels)
                string sub = Path.Combine(dir, "textures");
                if (diff == null && Directory.Exists(sub))
                {
                    diff = Directory.GetFiles(sub, slug + "_diff_*.jpg").Concat(Directory.GetFiles(sub, slug + "_diff_*.png")).FirstOrDefault();
                    nrm = Directory.GetFiles(sub, slug + "_nor_gl_*.jpg").Concat(Directory.GetFiles(sub, slug + "_nor_gl_*.png")).Concat(Directory.GetFiles(sub, slug + "_nor_gl_*.exr")).FirstOrDefault();
                }
            }
            if (diff == null)
                throw new System.Exception("Kapman assets: no colour texture for " + source + "/" + slug);
            string to = $"{Root}/{(source == "ambientcg" ? "AmbientCG" : "PolyHaven")}/{slug}/";
            var d = Tex(diff, to + Path.GetFileName(diff), maxSize, false);
            var n = nrm != null ? Tex(nrm, to + Path.GetFileName(nrm), maxSize, true) : null;
            return (d, n);
        }

        public static TerrainLayer Layer(string source, string slug, float tile, string path)
        {
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer != null)
                return layer;
            var (d, n) = Surface(source, slug);
            layer = new TerrainLayer { diffuseTexture = d, normalMapTexture = n, tileSize = new Vector2(tile, tile), normalScale = 0.6f, smoothness = 0f, metallic = 0f };
            Directory.CreateDirectory(Path.GetDirectoryName(Full(path)));
            AssetDatabase.CreateAsset(layer, path);
            return layer;
        }

        /// <summary>An HDRP/Lit material from a texture set. Meshes from MeshKit carry UVs in tiles already.</summary>
        public static Material Lit(string source, string slug, string path, float smoothness = 0.1f, Color? tint = null)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null)
            {
                Upgrade(m);
                if (tint.HasValue && m.GetColor("_BaseColor") != tint.Value)
                {
                    m.SetColor("_BaseColor", tint.Value);
                    EditorUtility.SetDirty(m);
                }
                return m;
            }
            var (d, n) = Surface(source, slug);
            m = NewLit(d, n, smoothness, tint ?? Color.white);
            Directory.CreateDirectory(Path.GetDirectoryName(Full(path)));
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        public static Material NewLit(Texture2D baseMap, Texture2D normal, float smoothness, Color tint)
        {
            var m = new Material(Shader.Find("HDRP/Lit"));
            SetLit(m, baseMap, normal, smoothness, tint);
            return m;
        }

        /// <summary>HDRP/Lit's colour, normal, smoothness and metallic on a material, then HDRP's validation (it sets the
        /// _NORMALMAP keyword from the texture, the passes and the render queue).</summary>
        static void SetLit(Material m, Texture2D baseMap, Texture2D normal, float smoothness, Color tint)
        {
            m.SetTexture("_BaseColorMap", baseMap);
            m.SetColor("_BaseColor", tint);
            m.SetTexture("_NormalMap", normal);
            m.SetFloat("_NormalScale", normal != null ? 0.8f : 1f);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            m.enableInstancing = true;
            HDMaterial.ValidateMaterial(m);
        }

        /// <summary>Alpha clipping on an HDRP/Lit or HDRP/Unlit material (leaves, grass, branch cards), two-sided if asked.</summary>
        public static void Cutout(Material m, float cutoff, bool twoSided)
        {
            HDMaterial.SetAlphaClipping(m, true);
            HDMaterial.SetAlphaCutoff(m, cutoff);
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetOverrideTag("RenderType", "TransparentCutout");
            m.renderQueue = (int)RenderQueue.AlphaTest;
            if (twoSided)
                TwoSided(m);
            HDMaterial.ValidateMaterial(m);
        }

        /// <summary>Both faces drawn (HDRP's double-sided mode, the normal mirrored on the back face), then validated.</summary>
        public static void TwoSided(Material m)
        {
            m.SetFloat("_DoubleSidedEnable", 1f);
            if (m.HasProperty("_DoubleSidedNormalMode"))
                m.SetFloat("_DoubleSidedNormalMode", 1f);     // mirror
            m.SetFloat("_CullMode", (float)CullMode.Off);
            if (m.HasProperty("_CullModeForward"))
                m.SetFloat("_CullModeForward", (float)CullMode.Off);
            m.doubleSidedGI = true;
            HDMaterial.ValidateMaterial(m);
        }

        /// <summary>
        /// Nits per unit of a Glow's strength. HDRP's unlit colour is in physical units and pre-exposed, so a flame's
        /// glow is given as emission in nits: a lantern flame at strength 9 is 3,600 nits, bright against a night
        /// exposed around 0 EV and blooming, a dull ember at 1.3 about 500.
        /// </summary>
        public const float NitsPerGlow = 120f;    // 3 Oct: 400 made every flame a white star at night

        /// <summary>An unlit glowing material (embers, lantern flames): HDRP/Unlit, black, with the glow as emission the bloom picks up.</summary>
        public static Material Glow(string path, Color colour, float strength)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = m == null;
            if (fresh)
                m = new Material(Shader.Find("HDRP/Unlit"));
            else if (!IsHdrp(m))
                m.shader = Shader.Find("HDRP/Unlit");
            SetGlow(m, colour, strength * NitsPerGlow);
            if (fresh)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Full(path)));
                AssetDatabase.CreateAsset(m, path);
            }
            else
                EditorUtility.SetDirty(m);
            return m;
        }

        static void SetGlow(Material m, Color colour, float nits)
        {
            m.SetColor("_UnlitColor", Color.black);
            m.SetFloat("_EmissiveExposureWeight", 0.55f);           // half a real light: steady by day, not a white star at night
            HDMaterial.SetUseEmissiveIntensity(m, true);
            HDMaterial.SetEmissiveColor(m, colour);
            HDMaterial.SetEmissiveIntensity(m, nits, EmissiveIntensityUnit.Nits);
            HDMaterial.ValidateMaterial(m);
        }

        // ------------------------------------------------------------------ URP materials met again

        static bool IsHdrp(Material m) => m != null && m.shader != null && m.shader.name.StartsWith("HDRP/");

        /// <summary>
        /// A material made under URP (its shader is gone with the package) brought over to HDRP in place, from its saved
        /// properties: Lit's colour, normal, smoothness and cut-out become HDRP/Lit; a particle material (its _Surface
        /// flag, or a Smoke/Rain/Snow name) becomes transparent HDRP/Unlit with the same map and colour; a bare colour
        /// (the glows) becomes HDRP/Unlit emission. Returns true if it changed the material.
        /// </summary>
        public static bool Upgrade(Material m)
        {
            if (m == null || IsHdrp(m))
                return false;
            var saved = new SavedProperties(m);
            var baseMap = saved.Tex("_BaseMap") ?? saved.Tex("_MainTex");
            var normal = saved.Tex("_BumpMap");
            var colour = saved.Colour("_BaseColor", Color.white);
            string n = m.name.ToLowerInvariant();
            bool particle = saved.Float("_Surface", 0f) > 0.5f || n.Contains("smoke") || n.Contains("rain") || n.Contains("snow");
            if (particle)
            {
                m.shader = Shader.Find("HDRP/Unlit");
                HdrpMaterials.SetUnlitTransparent(m, baseMap, colour);
            }
            else if (baseMap != null || normal != null || saved.Has("_Smoothness"))
            {
                m.shader = Shader.Find("HDRP/Lit");
                SetLit(m, baseMap, normal, saved.Float("_Smoothness", 0.1f), colour);
                if (saved.Float("_AlphaClip", 0f) > 0.5f)
                    Cutout(m, saved.Float("_Cutoff", 0.5f), saved.Float("_Cull", 2f) < 0.5f);
                else if (saved.Float("_Cull", 2f) < 0.5f)
                    TwoSided(m);
            }
            else
            {
                // an unlit colour: URP's glows carried their strength in the colour (colour x strength)
                m.shader = Shader.Find("HDRP/Unlit");
                float strength = Mathf.Max(colour.maxColorComponent, 0.01f);
                SetGlow(m, colour / strength, strength * NitsPerGlow);
            }
            EditorUtility.SetDirty(m);
            Debug.Log($"Kapman assets: {m.name} brought over to {m.shader.name}.");
            return true;
        }

        [MenuItem("Kapman/Setup/Upgrade the URP materials to HDRP")]
        public static void UpgradeAll()
        {
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Kapman" }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (Upgrade(m))
                    n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman assets: {n} materials brought over to HDRP.");
        }

        /// <summary>A material's saved properties, read through serialization: they stay when the shader is gone.</summary>
        class SavedProperties
        {
            readonly Dictionary<string, Texture> tex = new Dictionary<string, Texture>();
            readonly Dictionary<string, float> floats = new Dictionary<string, float>();
            readonly Dictionary<string, Color> colours = new Dictionary<string, Color>();

            public SavedProperties(Material m)
            {
                var so = new SerializedObject(m);
                var t = so.FindProperty("m_SavedProperties.m_TexEnvs");
                for (int i = 0; t != null && i < t.arraySize; i++)
                {
                    var e = t.GetArrayElementAtIndex(i);
                    tex[e.FindPropertyRelative("first").stringValue] = e.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                }
                var f = so.FindProperty("m_SavedProperties.m_Floats");
                for (int i = 0; f != null && i < f.arraySize; i++)
                {
                    var e = f.GetArrayElementAtIndex(i);
                    floats[e.FindPropertyRelative("first").stringValue] = e.FindPropertyRelative("second").floatValue;
                }
                var c = so.FindProperty("m_SavedProperties.m_Colors");
                for (int i = 0; c != null && i < c.arraySize; i++)
                {
                    var e = c.GetArrayElementAtIndex(i);
                    colours[e.FindPropertyRelative("first").stringValue] = e.FindPropertyRelative("second").colorValue;
                }
            }

            public bool Has(string name) => floats.ContainsKey(name) || tex.ContainsKey(name) || colours.ContainsKey(name);
            public Texture2D Tex(string name) => tex.TryGetValue(name, out var t) ? t as Texture2D : null;
            public float Float(string name, float fallback) => floats.TryGetValue(name, out var v) ? v : fallback;
            public Color Colour(string name, Color fallback) => colours.TryGetValue(name, out var v) ? v : fallback;
        }

        /// <summary>Writes colour with its alpha taken from a separate mask, as one texture in the project.</summary>
        public static Texture2D CombineAlpha(string colourFile, string alphaFile, string assetPath, int size)
        {
            if (!File.Exists(Full(assetPath)))
            {
                var c = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                c.LoadImage(File.ReadAllBytes(colourFile));
                var a = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                a.LoadImage(File.ReadAllBytes(alphaFile));
                var outTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var px = new Color32[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size, v = (y + 0.5f) / size;
                        Color cc = c.GetPixelBilinear(u, v), aa = a.GetPixelBilinear(u, v);
                        px[y * size + x] = new Color(cc.r, cc.g, cc.b, aa.r);
                    }
                outTex.SetPixels32(px);
                outTex.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(Full(assetPath)));
                File.WriteAllBytes(Full(assetPath), outTex.EncodeToPNG());
                Object.DestroyImmediate(c);
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(outTex);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = true;
                imp.maxTextureSize = size;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        /// <summary>
        /// A scanned tree as one prefab: its parts baked into one mesh (bark, branches, leaves as submeshes), standing on
        /// y = 0, with mesh levels of detail, and HDRP materials (cut-out, two-sided leaves). Returns the prefab, the
        /// leaves' material, and its colour.
        /// </summary>
        public static GameObject Tree(string slug, string prefabPath, out Material leaves)
        {
            string src = Path.Combine(Raw, "polyhaven", slug), tex = Path.Combine(src, "textures");
            string dir = $"{Root}/PolyHaven/{slug}";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            string leafPath = $"{dir}/{slug}_leaves.mat";
            if (existing != null)
            {
                leaves = AssetDatabase.LoadAssetAtPath<Material>(leafPath);
                Upgrade(AssetDatabase.LoadAssetAtPath<Material>($"{dir}/{slug}_bark.mat"));
                Upgrade(AssetDatabase.LoadAssetAtPath<Material>($"{dir}/{slug}_branches.mat"));
                Upgrade(leaves);
                return existing;
            }
            string fbxFile = Directory.GetFiles(src, "*.fbx").First();
            string fbx = Bring(fbxFile, $"{dir}/{Path.GetFileName(fbxFile)}");
            var mi = (ModelImporter)AssetImporter.GetAtPath(fbx);
            mi.importAnimation = false;
            mi.isReadable = true;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            mi.SaveAndReimport();

            // the materials: bark, branches (cut out when the texture has alpha), leaves (colour + its alpha mask)
            string barkFile = Path.Combine(tex, slug + "_diff_2k.jpg");
            var bark = NewLit(Tex(barkFile, $"{dir}/{Path.GetFileName(barkFile)}", 1024, false), null, 0.05f, Color.white);
            AssetDatabase.CreateAsset(bark, $"{dir}/{slug}_bark.mat");
            string branchFile = Directory.GetFiles(tex, slug + "_branch*_diff_2k.png").FirstOrDefault();
            Material branches = bark;
            if (branchFile != null)
            {
                bool hasAlpha = PngHasAlpha(branchFile);
                branches = NewLit(Tex(branchFile, $"{dir}/{Path.GetFileName(branchFile)}", 1024, false), null, 0.05f, Color.white);
                if (hasAlpha)
                    Cutout(branches, 0.4f, true);
                AssetDatabase.CreateAsset(branches, $"{dir}/{slug}_branches.mat");
            }
            string leafColour = Path.Combine(tex, slug + "_leaves_diff_2k.png"), leafAlpha = Path.Combine(tex, slug + "_leaves_alpha_2k.png");
            var leafTex = CombineAlpha(leafColour, leafAlpha, $"{dir}/{slug}_leaves_rgba.png", 1024);
            leaves = NewLit(leafTex, null, 0.1f, Color.white);
            Cutout(leaves, 0.45f, true);
            AssetDatabase.CreateAsset(leaves, leafPath);

            // bake the model's parts into one mesh, by what each material is
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var inst = (GameObject)Object.Instantiate(model);
            inst.transform.SetPositionAndRotation(Vector3.zero, model.transform.rotation);
            var groups = new[] { new List<CombineInstance>(), new List<CombineInstance>(), new List<CombineInstance>() };
            foreach (var mr in inst.GetComponentsInChildren<MeshRenderer>())
            {
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    continue;
                var mats = mr.sharedMaterials;
                for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                {
                    string n = s < mats.Length && mats[s] != null ? mats[s].name.ToLowerInvariant() : "";
                    int g = n.Contains("leaf") || n.Contains("leaves") ? 2 : n.Contains("branch") ? 1 : 0;
                    groups[g].Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = s, transform = mr.transform.localToWorldMatrix });
                }
            }
            Object.DestroyImmediate(inst);
            var parts = new List<Mesh>();
            var partMats = new List<Material>();
            var mats3 = new[] { bark, branches, leaves };
            for (int g = 0; g < 3; g++)
            {
                if (groups[g].Count == 0)
                    continue;
                var m = new Mesh { indexFormat = IndexFormat.UInt32 };
                m.CombineMeshes(groups[g].ToArray(), true, true);
                parts.Add(m);
                partMats.Add(mats3[g]);
            }
            var mesh = new Mesh { name = slug + "_tree", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(parts.Select(p => new CombineInstance { mesh = p, transform = Matrix4x4.identity }).ToArray(), false, false);
            // stand it on y = 0, in metres
            var b = mesh.bounds;
            float scale = b.size.y > 80f ? 0.01f : 1f;
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
                verts[i] = new Vector3(verts[i].x - b.center.x, verts[i].y - b.min.y, verts[i].z - b.center.z) * scale;
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            // the scans come at full detail (a million vertices and more). Three levels for the tree's LOD group, made
            // part by part: the wood by Unity's mesh simplification (made once per part: a second run on a cut mesh
            // crashed Unity 6000.6.3 in MeshLod::BuildRedirect), the leaves (and cut-out branch cards) by thinning:
            // simplification melts leaf cards away, so whole cards are kept at random and grown to keep the crown full.
            int[][] budgets =
            {
                new[] { 8000, 2500, 800 },      // bark and wood
                new[] { 8000, 3000, 1000 },     // branches
                new[] { 160000, 48000, 12000 },   // leaves (3 Oct: the thinned scans read as bare sticks; the cards are small)
            };
            var groupOfPart = new List<int>();
            for (int g = 0; g < 3; g++)
                if (groups[g].Count > 0)
                    groupOfPart.Add(g);
            bool branchCards = branches != bark && branches.IsKeywordEnabled("_ALPHATEST_ON");
            var perPart = new List<Mesh[]>();
            for (int pi = 0; pi < groupOfPart.Count; pi++)
            {
                int g = groupOfPart[pi];
                var sub = SubMesh(mesh, pi);
                var lv = new Mesh[3];
                bool cards = g == 2 || (g == 1 && branchCards);
                for (int i = 0; i < 3; i++)
                    lv[i] = cards ? Thin(sub, budgets[g][i], 79 + i) : null;
                if (!cards)
                {
                    try
                    {
                        MeshLodUtility.GenerateMeshLods(sub, MeshLodUtility.LodGenerationFlags.DiscardOddLevels, 8);
                        for (int i = 0; i < 3; i++)
                        {
                            int k = 0;
                            while (k < sub.lodCount - 1 && Triangles(sub, k) > budgets[g][i])
                                k++;
                            lv[i] = LevelMesh(sub, k);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Kapman assets: {slug} part {g} not simplified: {e.Message}");
                        for (int i = 0; i < 3; i++)
                            lv[i] = sub;
                    }
                }
                perPart.Add(lv);
            }
            var levels = new List<Mesh>();
            for (int i = 0; i < 3; i++)
            {
                var lm = new Mesh { name = $"{slug}_tree_lod{i}", indexFormat = IndexFormat.UInt32 };
                lm.CombineMeshes(perPart.Select(p => new CombineInstance { mesh = p[i], transform = Matrix4x4.identity }).ToArray(), false, false);
                lm.RecalculateBounds();
                levels.Add(lm);
            }
            for (int i = 0; i < levels.Count; i++)
                AssetDatabase.CreateAsset(levels[i], $"{dir}/{slug}_tree_lod{i}.asset");
            foreach (var p in parts)
                Object.DestroyImmediate(p);

            var go = new GameObject(slug);
            var lods = new List<LOD>();
            float[] screen = { 0.22f, 0.07f, 0.012f };
            for (int i = 0; i < levels.Count; i++)
            {
                var child = new GameObject("LOD" + i, typeof(MeshFilter), typeof(MeshRenderer));
                child.transform.SetParent(go.transform, false);
                child.GetComponent<MeshFilter>().sharedMesh = levels[i];
                var r = child.GetComponent<MeshRenderer>();
                r.sharedMaterials = partMats.ToArray();
                r.shadowCastingMode = i == 0 ? ShadowCastingMode.On : i == 1 ? ShadowCastingMode.On : ShadowCastingMode.Off;
                lods.Add(new LOD(screen[Mathf.Min(i, screen.Length - 1)], new Renderer[] { r }));
            }
            var group = go.AddComponent<LODGroup>();
            group.SetLODs(lods.ToArray());
            group.RecalculateBounds();
            Directory.CreateDirectory(Path.GetDirectoryName(Full(prefabPath)));
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"Kapman assets: tree {slug}: levels of {string.Join(", ", levels.Select(l => (Triangles(l, 0)).ToString("n0")))} triangles, {b.size.y * scale:0.0} m tall.");
            return prefab;
        }

        /// <summary>
        /// A scanned grass clump as a light prefab for the terrain's detail layers (meadow grass, and taller as reeds):
        /// one cut-out, two-sided material, one mesh cut to maxTris triangles, standing on y = 0.
        /// </summary>
        public static GameObject Grass(string slug, string prefabPath, int maxTris)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            string dir = $"{Root}/PolyHaven/{slug}";
            if (existing != null)
            {
                Upgrade(AssetDatabase.LoadAssetAtPath<Material>($"{dir}/{slug}_grass.mat"));
                return existing;
            }
            string src = Path.Combine(Raw, "polyhaven", slug), tex = Path.Combine(src, "textures");
            string fbxFile = Directory.GetFiles(src, "*.fbx").First();
            string fbx = Bring(fbxFile, $"{dir}/{Path.GetFileName(fbxFile)}");
            var mi = (ModelImporter)AssetImporter.GetAtPath(fbx);
            mi.importAnimation = false;
            mi.isReadable = true;
            mi.SaveAndReimport();
            var rgba = CombineAlpha(Path.Combine(tex, slug + "_diff_2k.jpg"), Path.Combine(tex, slug + "_alpha_2k.png"), $"{dir}/{slug}_rgba.png", 512);
            var mat = NewLit(rgba, null, 0.05f, Color.white);
            Cutout(mat, 0.5f, true);
            AssetDatabase.CreateAsset(mat, $"{dir}/{slug}_grass.mat");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var inst = (GameObject)Object.Instantiate(model);
            var parts = new List<CombineInstance>();
            foreach (var mf in inst.GetComponentsInChildren<MeshFilter>())
                if (mf.sharedMesh != null)
                    for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                        parts.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = s, transform = mf.transform.localToWorldMatrix });
            Object.DestroyImmediate(inst);
            var mesh = new Mesh { name = slug + "_clump", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(parts.ToArray(), true, true);
            var b = mesh.bounds;
            float scale = b.size.y > 20f ? 0.01f : 1f;
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
                verts[i] = new Vector3(verts[i].x - b.center.x, verts[i].y - b.min.y, verts[i].z - b.center.z) * scale;
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            try
            {
                MeshLodUtility.GenerateMeshLods(mesh, MeshLodUtility.LodGenerationFlags.DiscardOddLevels, 8);
                var reduced = Reduce(mesh, maxTris);
                if (reduced != mesh)
                {
                    Object.DestroyImmediate(mesh);
                    mesh = reduced;
                    mesh.name = slug + "_clump";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Kapman assets: grass {slug} not cut: {e.Message}");
            }
            AssetDatabase.CreateAsset(mesh, $"{dir}/{slug}_clump.asset");
            var go = new GameObject(slug);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            Directory.CreateDirectory(Path.GetDirectoryName(Full(prefabPath)));
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"Kapman assets: grass {slug}: {mesh.vertexCount:n0} vertices, {mesh.bounds.size.y:0.00} m tall.");
            return prefab;
        }

        public const int MaxTreeTriangles = 40000;

        static int Triangles(Mesh m, int lod)
        {
            int n = 0;
            for (int s = 0; s < m.subMeshCount; s++)
                n += m.GetTriangles(s, lod, true).Length / 3;
            return n;
        }

        /// <summary>A mesh made from the first of its levels of detail with no more than maxTris triangles.</summary>
        static Mesh Reduce(Mesh mesh, int maxTris)
        {
            if (mesh.lodCount <= 1 || Triangles(mesh, 0) <= maxTris)
                return mesh;
            int lod = 0;
            while (lod < mesh.lodCount - 1 && Triangles(mesh, lod) > maxTris)
                lod++;
            return LevelMesh(mesh, lod);
        }

        /// <summary>One submesh of a mesh as a mesh of its own (its vertices only).</summary>
        static Mesh SubMesh(Mesh mesh, int sub)
        {
            var verts = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            var tris = mesh.GetTriangles(sub);
            var remap = new Dictionary<int, int>();
            var nv = new List<Vector3>();
            var nn = new List<Vector3>();
            var nu = new List<Vector2>();
            var list = new List<int>(tris.Length);
            foreach (int v in tris)
            {
                if (!remap.TryGetValue(v, out int to))
                {
                    to = nv.Count;
                    remap[v] = to;
                    nv.Add(verts[v]);
                    if (normals.Length == verts.Length) nn.Add(normals[v]);
                    if (uvs.Length == verts.Length) nu.Add(uvs[v]);
                }
                list.Add(to);
            }
            var m = new Mesh { indexFormat = IndexFormat.UInt32 };
            m.SetVertices(nv);
            if (nn.Count == nv.Count) m.SetNormals(nn);
            if (nu.Count == nv.Count) m.SetUVs(0, nu);
            m.SetTriangles(list, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>
        /// Leaf cards thinned to a triangle budget: the cards (the mesh's separate pieces) are kept at random, and each
        /// kept card grows about its middle so the crown stays about as full (up to 3.5 times).
        /// </summary>
        static Mesh Thin(Mesh src, int budget, int seed)
        {
            var verts = src.vertices;
            var normals = src.normals;
            var uvs = src.uv;
            var tris = src.triangles;
            int nTri = tris.Length / 3;
            if (nTri <= budget)
                return src;
            var parent = new int[verts.Length];
            for (int i = 0; i < parent.Length; i++)
                parent[i] = i;
            int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }
            for (int t = 0; t < nTri; t++)
            {
                int a = Find(tris[t * 3]), b = Find(tris[t * 3 + 1]), c = Find(tris[t * 3 + 2]);
                parent[b] = a;
                parent[c] = a;
            }
            var cards = new Dictionary<int, List<int>>();
            for (int t = 0; t < nTri; t++)
            {
                int r = Find(tris[t * 3]);
                if (!cards.TryGetValue(r, out var l))
                    cards[r] = l = new List<int>();
                l.Add(t);
            }
            float keep = Mathf.Clamp01(budget / (float)nTri);
            float grow = Mathf.Min(2.4f, Mathf.Sqrt(1f / Mathf.Max(keep, 1e-4f)));
            var rnd = new System.Random(seed);
            var nv = new List<Vector3>();
            var nn = new List<Vector3>();
            var nu = new List<Vector2>();
            var nt = new List<int>();
            var map = new Dictionary<int, int>();
            foreach (var card in cards.Values)
            {
                if (rnd.NextDouble() > keep)
                    continue;
                map.Clear();
                Vector3 centre = Vector3.zero;
                int count = 0;
                foreach (int t in card)
                    for (int k = 0; k < 3; k++)
                    {
                        centre += verts[tris[t * 3 + k]];
                        count++;
                    }
                centre /= count;
                foreach (int t in card)
                    for (int k = 0; k < 3; k++)
                    {
                        int v = tris[t * 3 + k];
                        if (!map.TryGetValue(v, out int to))
                        {
                            to = nv.Count;
                            map[v] = to;
                            nv.Add(centre + (verts[v] - centre) * grow);
                            if (normals.Length == verts.Length) nn.Add(normals[v]);
                            if (uvs.Length == verts.Length) nu.Add(uvs[v]);
                        }
                        nt.Add(to);
                    }
            }
            var m = new Mesh { indexFormat = IndexFormat.UInt32 };
            m.SetVertices(nv);
            if (nn.Count == nv.Count) m.SetNormals(nn);
            if (nu.Count == nv.Count) m.SetUVs(0, nu);
            m.SetTriangles(nt, 0);
            m.RecalculateBounds();
            Debug.Log($"Kapman assets: leaf cards thinned from {nTri:n0} triangles ({cards.Count:n0} cards) to {nt.Count / 3:n0}, grown x{grow:0.0}.");
            return m;
        }

        /// <summary>A plain mesh (its own vertices, no levels) from one of a mesh's levels of detail.</summary>
        static Mesh LevelMesh(Mesh mesh, int lod)
        {
            var verts = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            var remap = new Dictionary<int, int>();
            var nv = new List<Vector3>();
            var nn = new List<Vector3>();
            var nu = new List<Vector2>();
            var subs = new List<List<int>>();
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                var tris = mesh.GetTriangles(s, lod, true);
                var list = new List<int>(tris.Length);
                foreach (int v in tris)
                {
                    if (!remap.TryGetValue(v, out int to))
                    {
                        to = nv.Count;
                        remap[v] = to;
                        nv.Add(verts[v]);
                        if (normals.Length == verts.Length) nn.Add(normals[v]);
                        if (uvs.Length == verts.Length) nu.Add(uvs[v]);
                    }
                    list.Add(to);
                }
                subs.Add(list);
            }
            var reduced = new Mesh { indexFormat = nv.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            reduced.SetVertices(nv);
            if (nn.Count == nv.Count) reduced.SetNormals(nn);
            if (nu.Count == nv.Count) reduced.SetUVs(0, nu);
            reduced.subMeshCount = subs.Count;
            for (int s = 0; s < subs.Count; s++)
                reduced.SetTriangles(subs[s], s);
            reduced.RecalculateBounds();
            Debug.Log($"Kapman assets: {mesh.name} cut from {Triangles(mesh, 0):n0} to {Triangles(reduced, 0):n0} triangles (its level {lod}).");
            return reduced;
        }

        static bool PngHasAlpha(string file)
        {
            using (var f = File.OpenRead(file))
            {
                f.Seek(25, SeekOrigin.Begin);
                int t = f.ReadByte();
                return t == 4 || t == 6;
            }
        }

        /// <summary>The Gislinge boat (CC BY 4.0) with HDRP materials: hull and sail.</summary>
        public static GameObject Gislinge(string prefabPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            const string dir = "Assets/Kapman/Models/Ships/Gislinge";
            if (existing != null)
            {
                foreach (var r in existing.GetComponentsInChildren<Renderer>(true))
                    foreach (var mat in r.sharedMaterials)
                        Upgrade(mat);
                return existing;
            }
            string fbx = dir + "/Gislinge_Boat.fbx";
            var mi = (ModelImporter)AssetImporter.GetAtPath(fbx);
            mi.importAnimation = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            mi.SaveAndReimport();
            Texture2D T(string f, bool n)
            {
                string p = dir + "/Textures/" + f;
                var imp = (TextureImporter)AssetImporter.GetAtPath(p);
                imp.maxTextureSize = 1024;
                imp.textureType = n ? TextureImporterType.NormalMap : TextureImporterType.Default;
                imp.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            }
            var hull = NewLit(T("Viking_Ship_basecolor_04.jpg", false), T("Viking_Ship_normal.jpg", true), 0.15f, Color.white);
            AssetDatabase.CreateAsset(hull, dir + "/Gislinge_Hull_URP.mat");
            var sail = NewLit(T("Sailcloth_basecolor.jpg", false), T("Sailcloth_normal.jpg", true), 0.05f, Color.white);
            TwoSided(sail);
            AssetDatabase.CreateAsset(sail, dir + "/Gislinge_Sail_URP.mat");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = "Gislinge";
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    string n = (ms[i] != null ? ms[i].name : r.name).ToLowerInvariant();
                    ms[i] = n.Contains("sail") ? sail : hull;
                }
                r.sharedMaterials = ms;
            }
            // lie along +z (the bow), the keel at the waterline
            var bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                bounds.Encapsulate(r.bounds);
            var holder = new GameObject("Ship_Gislinge");
            go.transform.SetParent(holder.transform, true);
            if (bounds.size.x > bounds.size.z)
                go.transform.RotateAround(bounds.center, Vector3.up, 90f);
            bounds = new Bounds(go.transform.position, Vector3.zero);
            bool firstB = true;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (firstB) { bounds = r.bounds; firstB = false; }
                else bounds.Encapsulate(r.bounds);
            }
            go.transform.position -= new Vector3(bounds.center.x, bounds.min.y + 0.25f, bounds.center.z);
            Directory.CreateDirectory(Path.GetDirectoryName(Full(prefabPath)));
            var prefab = PrefabUtility.SaveAsPrefabAsset(holder, prefabPath);
            Object.DestroyImmediate(holder);
            Debug.Log($"Kapman assets: the Gislinge, {bounds.size.z:0.0} m long, {bounds.size.x:0.0} m wide.");
            return prefab;
        }
    }
}
