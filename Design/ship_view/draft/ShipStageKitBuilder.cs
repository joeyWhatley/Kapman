// PATH IN THE PROJECT: Assets/Kapman/Editor/Game/ShipStageKitBuilder.cs   (assembly Kapman.Editor)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Editor.rsp (with the drafts' Kapman.Game); never run.
using System.Collections.Generic;
using System.IO;
using Kapman.Game.ShipView;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// MAKES THE 3D SHIP VIEW'S KIT (Assets/Kapman/Resources/ShipStageKit.asset): the pack's prefabs the stage is built
    /// from, and HDRP materials for it under Assets/Kapman/ShipView/Materials. The pack (Assets/
    /// Stylized_Medieval_Vikings_Pack) ships three URP/Lit materials (shader guid 933532a4fcc9baf4fa0491de14d08ed7; this
    /// project has HDRP only, so they draw pink): each gets an HDRP/Lit twin with the pack's own palette texture, and the
    /// stage swaps them in on its own copies. Nothing in the pack's folder is changed.
    /// Run again at any time: it fills in what is missing and leaves the kit's numbers and any material already made as
    /// they are (so a material tuned by hand is kept). With the pack missing the kit is still made, empty of prefabs.
    /// Menu: Kapman > Ship view > Make the kit.   Batch: -executeMethod Kapman.EditorTools.ShipStageKitBuilder.MakeBatch
    /// </summary>
    public static class ShipStageKitBuilder
    {
        const string Pack = "Assets/Stylized_Medieval_Vikings_Pack";
        const string Folder = "Assets/Kapman/ShipView/Materials";
        public const string KitPath = "Assets/Kapman/Resources/" + ShipStageKit.ResourceName + ".asset";

        /// <summary>The pack's materials and the palette each one draws with (read from the .mat files, 7 Oct 2026).</summary>
        static readonly (string material, string texture)[] Palettes =
        {
            ("SMV_BasePallete", "BasePallete.png"),                       // the ship's hull, mast and sails
            ("SMV_Weapons", "UniversalWeaponsColorsVariant.png"),         // the props: planks, logs, barrels, oars, rope
            ("SMV_Environment", "SMV_EnvironmentPallete.png"),            // the pack's own water mesh (not used)
        };

        [MenuItem("Kapman/Ship view/Make the kit")]
        public static void Make()
        {
            Directory.CreateDirectory("Assets/Kapman/Resources");
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            var kit = AssetDatabase.LoadAssetAtPath<ShipStageKit>(KitPath);
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

            var swaps = new List<StageMaterialSwap>();
            foreach (var (material, texture) in Palettes)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Pack}/Materials/Textures/{texture}");
                if (tex == null)
                {
                    missing.Add(texture);
                    continue;
                }
                var m = Lit("SV_" + material, Color.white, 0.12f);
                m.SetTexture("_BaseColorMap", tex);
                HDShaderUtils.ResetMaterialKeywords(m);
                EditorUtility.SetDirty(m);
                swaps.Add(new StageMaterialSwap { from = material, to = m });
            }
            kit.swaps = swaps.ToArray();
            kit.water = Lit("SV_Water", new Color(0.05f, 0.14f, 0.18f), 0.86f);
            kit.shore = Lit("SV_Shore", new Color(0.33f, 0.3f, 0.2f), 0.15f);
            kit.wake = Unlit("SV_Foam", new Color(0.9f, 0.94f, 0.95f, 0.45f));
            kit.line = Unlit("SV_Rope", new Color(0.55f, 0.47f, 0.33f, 1f));
            kit.stars = AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/Kapman/Sky/Stars_930.exr");
            if (kit.stars == null)
                missing.Add("Assets/Kapman/Sky/Stars_930.exr");
            EditorUtility.SetDirty(kit);
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman ship view: the kit is at " + KitPath + (missing.Count == 0 ? ", complete." : "; missing (a plain stand-in is drawn for each): " + string.Join(", ", missing) + "."));
        }

        static Material Lit(string name, Color colour, float smoothness)
        {
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null)
                return m;                                                // (made before: kept as it is, tuned or not)
            m = new Material(Shader.Find("HDRP/Lit")) { name = name };
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            HDShaderUtils.ResetMaterialKeywords(m);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Material Unlit(string name, Color colour)
        {
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Unlit")) { name = name };
            m.SetColor("_UnlitColor", colour);
            if (colour.a < 0.99f)
            {
                m.SetFloat("_SurfaceType", 1f);                           // transparent, alpha blended (as the look lab's TankBuilder.Unlit)
                m.SetFloat("_BlendMode", 0f);
                m.SetFloat("_DoubleSidedEnable", 1f);
            }
            HDShaderUtils.ResetMaterialKeywords(m);
            AssetDatabase.CreateAsset(m, path);
            return m;
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
