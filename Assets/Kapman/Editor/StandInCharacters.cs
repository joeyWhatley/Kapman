using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE STAND-IN PEOPLE (the user's Primitive Characters Pack, 4 Oct pm; an Asset Store package: not in git): their
    /// materials are Unity's built-in Standard shader, which HDRP draws magenta. This turns each into HDRP/Lit: the albedo
    /// map and colour, the normal map, alpha clipping where the material was cut out, a plain matte smoothness. Run by the
    /// scale-test builder; Kapman > Setup > Convert the stand-in characters for HDRP.
    /// </summary>
    public static class StandInCharacters
    {
        public const string Root = "Assets/Primitive_Characters_Pack";

        public static bool Installed => AssetDatabase.IsValidFolder(Root);

        [MenuItem("Kapman/Setup/Convert the stand-in characters for HDRP")]
        public static void ConvertMaterials()
        {
            if (!Installed)
                return;
            var lit = Shader.Find("HDRP/Lit");
            int n = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { Root }))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (m == null || m.shader == null || m.shader.name != "Standard")
                    continue;
                var albedo = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
                var colour = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                var normal = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;
                float mode = m.HasProperty("_Mode") ? m.GetFloat("_Mode") : 0f;
                float cutoff = m.HasProperty("_Cutoff") ? m.GetFloat("_Cutoff") : 0.5f;
                bool hair = m.name.ToLowerInvariant().Contains("hair") || m.name.ToLowerInvariant().Contains("wool");
                m.shader = lit;
                m.SetTexture("_BaseColorMap", albedo);
                m.SetColor("_BaseColor", colour);
                if (normal != null)
                {
                    m.SetTexture("_NormalMap", normal);
                    m.SetFloat("_NormalScale", 1f);
                }
                m.SetFloat("_Metallic", 0f);
                m.SetFloat("_Smoothness", 0.25f);
                if (mode >= 1f || hair)
                {
                    m.SetFloat("_AlphaCutoffEnable", 1f);
                    m.SetFloat("_AlphaCutoff", mode >= 1f ? cutoff : 0.35f);
                    m.SetFloat("_DoubleSidedEnable", 1f);
                }
                UnityEditor.Rendering.HighDefinition.HDShaderUtils.ResetMaterialKeywords(m);
                EditorUtility.SetDirty(m);
                n++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman stand-ins: {n} materials turned into HDRP/Lit.");
        }

        /// <summary>The figures' prefabs (the people, not their weapons, torches or stands).</summary>
        public static GameObject[] People()
        {
            var list = new System.Collections.Generic.List<GameObject>();
            if (!Installed)
                return list.ToArray();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (file.Contains("weapon") || file.Contains("torch") || file.Contains("stand"))
                    continue;
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null)
                    list.Add(p);
            }
            return list.ToArray();
        }
    }
}
