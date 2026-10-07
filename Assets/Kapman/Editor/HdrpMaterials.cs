using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE HDRP PARTICLE MATERIALS - the smoke, rain and snow the levels' particle systems draw: transparent HDRP/Unlit
    /// with Unity's built-in soft particle dot (Default-Particle.psd), alpha-blended, no depth write, fogged, so far
    /// smoke sinks into the haze. The scene builders call these in place of their URP materials.
    ///
    /// Where the URP shader names elsewhere must change (the orchestrator wires these; the files are owned there):
    ///   - DorestadTown.SmokeMaterial(): "Universal Render Pipeline/Particles/Unlit" -> HdrpMaterials.Smoke(path).
    ///   - DorestadBuilder.Precipitation(): "Universal Render Pipeline/Particles/Unlit" -> HdrpMaterials.Rain(path) /
    ///     HdrpMaterials.Snow(path) (the renderer's stretch/billboard modes stay as they are).
    ///   - Birds.cs (runtime, no AssetDatabase): "Universal Render Pipeline/Lit" -> "HDRP/Lit"; keep _BaseColor and
    ///     _Smoothness; in place of _Cull 0 set _DoubleSidedEnable 1 and _CullMode 0, then HDMaterial.ValidateMaterial(mat)
    ///     (a runtime API in UnityEngine.Rendering.HighDefinition).
    ///
    /// HDRP/Unlit ignores the particles' own colours (vertex colour): a system's colour over lifetime does nothing.
    /// Fade with size over lifetime, with the material's _UnlitColor alpha (as the first build's HdrpRain did), or give
    /// the system a texture sheet whose alpha fades.
    /// </summary>
    public static class HdrpMaterials
    {
        /// <summary>Unity's built-in soft round particle dot.</summary>
        public static Texture2D SoftParticle() => AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");

        /// <summary>Hearth smoke through the thatch and over the bonfires: a grey, mostly see-through dot.</summary>
        public static Material Smoke(string path) => Particle(path, "Smoke", new Color(0.5f, 0.5f, 0.5f, 0.35f));

        /// <summary>Rain streaks (the renderer stretches them along their fall).</summary>
        public static Material Rain(string path) => Particle(path, "Rain", new Color(0.7f, 0.74f, 0.78f, 0.35f));

        /// <summary>Snowflakes: near-white billboards.</summary>
        public static Material Snow(string path) => Particle(path, "Snow", new Color(0.95f, 0.95f, 0.97f, 0.9f));

        /// <summary>A transparent HDRP/Unlit particle material at the path (made, or brought up to date if it is there).</summary>
        public static Material Particle(string path, string name, Color colour, Texture2D map = null)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = m == null;
            if (fresh)
                m = new Material(Shader.Find("HDRP/Unlit")) { name = name };
            else if (m.shader == null || !m.shader.name.StartsWith("HDRP/"))
                m.shader = Shader.Find("HDRP/Unlit");
            SetUnlitTransparent(m, map != null ? map : SoftParticle(), colour);
            if (fresh)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Directory.GetCurrentDirectory(), path)));
                AssetDatabase.CreateAsset(m, path);
            }
            else
                EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>
        /// HDRP/Unlit as a see-through particle: alpha blending, no depth write, drawn in the transparent pass, fogged,
        /// both faces (billboards and stretched streaks can turn their backs). HDRP's validation sets the blend state,
        /// keywords and queue from these flags.
        /// </summary>
        public static void SetUnlitTransparent(Material m, Texture2D map, Color colour)
        {
            // (3 Oct) HDRP's Unlit keeps its base colour at a fixed screen brightness whatever the exposure, so white spray and
            // smoke glared like lamps at night: particles are lit transparent surfaces now (HDRP/Lit), dull and double-sided
            if (m.shader != null && m.shader.name == "HDRP/Unlit")
                m.shader = Shader.Find("HDRP/Lit");
            m.SetTexture("_BaseColorMap", map);
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", 0f);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_ReceivesSSR", 0f);
            m.SetFloat("_EnableBlendModePreserveSpecular", 0f);
            m.SetFloat("_SurfaceType", 1f);                       // transparent
            m.SetFloat("_BlendMode", 0f);                         // alpha
            m.SetFloat("_TransparentZWrite", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_EnableFogOnTransparent", 1f);
            m.SetFloat("_DoubleSidedEnable", 1f);
            m.SetFloat("_CullMode", (float)CullMode.Off);
            m.SetFloat("_TransparentCullMode", (float)CullMode.Off);
            m.SetFloat("_AlphaCutoffEnable", 0f);
            m.SetFloat("_EmissiveExposureWeight", 1f);
            m.SetColor("_EmissiveColor", Color.black);
            m.SetOverrideTag("RenderType", "Transparent");
            HDMaterial.SetSurfaceType(m, true);
            HDMaterial.ValidateMaterial(m);
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
