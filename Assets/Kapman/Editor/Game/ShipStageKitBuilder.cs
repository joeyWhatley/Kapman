using System.Collections.Generic;
using System.IO;
using Kapman.Game.ShipView;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// MAKES THE 3D SHIP VIEW'S KIT (Assets/Kapman/ShipView/Resources/ShipStageKit.asset): the pack's prefabs the stage
    /// is built from, and HDRP materials for it under Assets/Kapman/ShipView/Materials. The pack (Assets/
    /// Stylized_Medieval_Vikings_Pack) ships three URP/Lit materials (this project has HDRP only, so they draw pink):
    /// each gets an HDRP/Lit twin with the pack's own palette texture, and the stage swaps them in on its own copies.
    /// Nothing in the pack's folder is changed.
    /// Run again at any time: it fills in what is missing and leaves the kit's numbers and any material already made as
    /// they are (so a material or a number tuned by hand is kept). "Make the kit anew" throws the kit's numbers away and
    /// takes the code's again (the materials are still kept). With the pack missing the kit is still made, empty of
    /// prefabs, and the stage stands plain shapes in their places.
    /// Menu: Kapman > Ship view > Make the kit.   Batch: -executeMethod Kapman.EditorTools.ShipStageKitBuilder.MakeBatch
    /// </summary>
    public static class ShipStageKitBuilder
    {
        const string Pack = "Assets/Stylized_Medieval_Vikings_Pack";
        const string Folder = "Assets/Kapman/ShipView/Materials";
        const string ExclusionMaterialPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Material/MaterialWaterExclusion.mat";
        public const string KitPath = "Assets/Kapman/ShipView/Resources/" + ShipStageKit.ResourceName + ".asset";

        /// <summary>The pack's materials and the palette each one draws with (read from the .mat files, 7 Oct 2026).</summary>
        static readonly (string material, string texture)[] Palettes =
        {
            ("SMV_BasePallete", "BasePallete.png"),                       // the ship's hull, mast and sails; the men
            ("SMV_Weapons", "UniversalWeaponsColorsVariant.png"),         // the props: planks, logs, barrels, oars, rope
            ("SMV_Environment", "SMV_EnvironmentPallete.png"),            // the pack's own water mesh (not used)
        };

        [MenuItem("Kapman/Ship view/Make the kit")]
        public static void Make() => Make(false);

        [MenuItem("Kapman/Ship view/Make the kit anew (its numbers back to the code's)")]
        public static void MakeAnew() => Make(true);

        static void Make(bool anew)
        {
            Directory.CreateDirectory("Assets/Kapman/ShipView/Resources");
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            var kit = AssetDatabase.LoadAssetAtPath<ShipStageKit>(KitPath);
            if (kit != null && anew)
            {
                // (the same asset, so nothing that points at it loses it: only its values go back to the class's own)
                var fresh = ScriptableObject.CreateInstance<ShipStageKit>();
                EditorUtility.CopySerialized(fresh, kit);
                Object.DestroyImmediate(fresh);
                kit.name = ShipStageKit.ResourceName;
            }
            if (kit == null)
            {
                kit = ScriptableObject.CreateInstance<ShipStageKit>();
                AssetDatabase.CreateAsset(kit, KitPath);
            }
            var missing = new List<string>();
            GameObject P(string name)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>($"{Pack}/Prefabs - Environment/{name}.prefab");
                if (p == null)
                    missing.Add(name);
                return p;
            }
            kit.ship = P("SMV_Ship");
            kit.oar = P("Oar_1");
            kit.thwart = P("Plank_4");
            kit.deckPlank = P("Plank_2");
            kit.stringer = P("Plank_3");
            kit.pile = P("Short_log_1");
            kit.barrel = P("Barrel_1");
            kit.crate = P("Crate_1");
            kit.sackA = P("Sack_1");
            kit.sackB = P("Sack_3");
            kit.ropeCoil = P("Rope_1");
            kit.bucket = P("Bucket_1");
            // the crew: a trading boat's men, settlers first (the names are the other builder's choice, Design/ship_view/JOIN.md E5)
            GameObject C(string name)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>($"{Pack}/Prefabs - Characters/{name}.prefab");
                if (p == null)
                    missing.Add(name);
                return p;
            }
            kit.men = new[] { C("SMV_Viking_Settler_1"), C("SMV_Viking_Settler_3"), C("SMV_Viking_Karl_2"), C("SMV_Viking_Settler_2") };

            var swaps = new List<StageMaterialSwap>();
            foreach (var (material, texture) in Palettes)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Pack}/Materials/Textures/{texture}");
                if (tex == null)
                {
                    missing.Add(texture);
                    continue;
                }
                var m = Lit("SV_" + material, Color.white, 0.12f, out bool made);
                if (made)
                {
                    m.SetTexture("_BaseColorMap", tex);
                    HDShaderUtils.ResetMaterialKeywords(m);
                    EditorUtility.SetDirty(m);
                }
                swaps.Add(new StageMaterialSwap { from = material, to = m });
            }
            kit.swaps = swaps.ToArray();
            kit.water = Lit("SV_Water", new Color(0.05f, 0.14f, 0.18f), 0.86f, out _);
            kit.shore = Lit("SV_Shore", new Color(0.26f, 0.3f, 0.16f), 0.1f, out _);
            kit.line = Lit("SV_Rope", new Color(0.55f, 0.47f, 0.33f, 1f), 0.1f, out _);
            kit.wake = SeeThrough("SV_Wake", new Color(0.9f, 0.94f, 0.95f, 0.3f), WakeFade());
            kit.rain = SeeThrough("SV_Rain", new Color(0.7f, 0.74f, 0.78f, 0.35f), null);
            kit.waterExclusion = AssetDatabase.LoadAssetAtPath<Material>(ExclusionMaterialPath);
            if (kit.waterExclusion == null)
                missing.Add("HDRP's water exclusion material (the sea may be drawn inside her hull)");
            kit.wakeFoam = AssetDatabase.LoadAssetAtPath<Material>("Assets/Kapman/Water/Ship_Foam.mat");
            if (kit.wakeFoam == null)
                missing.Add("Assets/Kapman/Water/Ship_Foam.mat (she leaves no foam astern on HDRP's water)");
            kit.stars = AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/Kapman/Sky/Stars_930.exr");
            if (kit.stars == null)
                missing.Add("Assets/Kapman/Sky/Stars_930.exr");
            // the PSX looks' shaders (Assets/Kapman/ShipView/Shaders): held by the kit so a build takes them with it
            kit.psxModel = Shader.Find("Kapman/ShipView/PSX Model");
            kit.psxSea = Shader.Find("Kapman/ShipView/PSX Sea");
            kit.psxSprite = Shader.Find("Kapman/ShipView/PSX Sprite");
            kit.psxScreen = Shader.Find("Hidden/Kapman/ShipView/PSX Screen");
            if (kit.psxModel == null || kit.psxSea == null || kit.psxSprite == null || kit.psxScreen == null)
                missing.Add("a shader of the PSX looks (Assets/Kapman/ShipView/Shaders): those looks fall back to the plain picture");
            EditorUtility.SetDirty(kit);
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman ship view: the kit is at " + KitPath + (missing.Count == 0 ? ", complete." : "; missing (a plain stand-in is drawn for each): " + string.Join(", ", missing) + "."));
        }

        /// <summary>A plain HDRP/Lit material of the stage's. One made before is kept as it is, tuned or not.</summary>
        static Material Lit(string name, Color colour, float smoothness, out bool made)
        {
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            made = m == null;
            if (m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Lit")) { name = name };
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            HDShaderUtils.ResetMaterialKeywords(m);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        /// <summary>
        /// A lit see-through material, dull and double-sided: rain, and the streak of her wake. The look lab's recipe
        /// (KapmanSail's TankBuilder.ParticleMaterial, itself the game's HdrpMaterials.SetUnlitTransparent): lit, so it
        /// is dark at night (an unlit one glowed), and fogged with everything else.
        /// </summary>
        static Material SeeThrough(string name, Color colour, Texture2D map)
        {
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Lit")) { name = name };
            m.SetColor("_BaseColor", colour);
            if (map != null)
                m.SetTexture("_BaseColorMap", map);
            m.SetFloat("_Smoothness", 0f);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_ReceivesSSR", 0f);
            m.SetFloat("_SurfaceType", 1f);
            m.SetFloat("_BlendMode", 0f);
            m.SetFloat("_TransparentZWrite", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_EnableFogOnTransparent", 1f);
            m.SetFloat("_DoubleSidedEnable", 1f);
            m.SetFloat("_CullMode", (float)CullMode.Off);
            m.SetFloat("_TransparentCullMode", (float)CullMode.Off);
            m.SetFloat("_AlphaCutoffEnable", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            HDMaterial.SetSurfaceType(m, true);
            HDMaterial.ValidateMaterial(m);
            m.renderQueue = (int)RenderQueue.Transparent;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        /// <summary>
        /// The wake's fade, as a small texture: white, its alpha full along the middle near her stern, gone at both edges
        /// and at the far end, broken into a few streaks. (HDRP/Lit does not read a vertex's alpha, so the ribbon's fade
        /// has to be in a texture: u across, v from the stern to the end.)
        /// </summary>
        static Texture2D WakeFade()
        {
            string path = $"{Folder}/SV_WakeFade.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;
            const int w = 32, h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f - u) * 3.2f);
                    float along = Mathf.Pow(1f - v, 1.4f) * Mathf.SmoothStep(0f, 1f, v * 10f);
                    // two troughs either side of the middle: the water her quarters throw off, not one flat band
                    float streaks = 0.55f + 0.45f * Mathf.Abs(Mathf.Cos((u - 0.5f) * Mathf.PI * 2.4f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * along * streaks)));
                }
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static void MakeBatch()
        {
            try
            {
                Make();
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
