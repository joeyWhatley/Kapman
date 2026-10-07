using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Hdrp
{
    /// <summary>
    /// SNOW, FROST, WET GROUND AND THE AURORA - two full-screen HDRP custom passes (Shaders/Hdrp/KapmanWeatherPasses.
    /// shader) in a custom pass volume of their own, made at runtime and kept out of the scene file:
    ///   - "Snow Frost Wet", before the pre-refraction transparents (where HDRP binds the camera's colour for reading,
    ///     and the water hasn't been drawn yet): reads the depth and the normal buffer and lays snow and frost on what
    ///     faces up, lit as fresh snow would be by the sky overhead, the Sun and the Moon (bounded by what a white
    ///     surface could get here, so snow in shade stays in shade), and darkens wet ground. The fog is already on the
    ///     picture there, so the snow fades with the fog's own transmittance;
    ///   - "Aurora", added over the sky in the north on clear nights, a slow curtain in real nits under HDRP's exposure.
    /// Made and fed by <see cref="HdrpWeatherDraw"/>.
    /// </summary>
    public class SnowFrostPass : System.IDisposable
    {
        public const string ShaderName = "Hidden/Kapman/Weather Passes";

        GameObject go;
        CustomPassVolume volume;
        Material material;
        FullScreenCustomPass ground, aurora;

        static readonly int GroundId = Shader.PropertyToID("_KapmanGround"), FogShapeId = Shader.PropertyToID("_KapmanFogShape");
        static readonly int SunLightId = Shader.PropertyToID("_KapmanSunLight"), MoonLightId = Shader.PropertyToID("_KapmanMoonLight");
        static readonly int AuroraId = Shader.PropertyToID("_KapmanAurora");

        public bool Ready => volume != null && material != null;

        public SnowFrostPass(Transform parent, Shader shader)
        {
            if (shader == null)
                shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning("Kapman weather: the shader '" + ShaderName + "' is missing; no snow, frost, wet ground or aurora.");
                return;
            }
            material = new Material(shader) { name = "Kapman weather passes", hideFlags = HideFlags.DontSave };
            const string goName = "Weather passes (custom pass)";
            var old = parent.Find(goName);                                   // left from before a domain reload
            if (old != null)
                CoreUtils.Destroy(old.gameObject);
            go = new GameObject(goName) { hideFlags = HideFlags.HideAndDontSave };
            go.transform.SetParent(parent, false);
            volume = go.AddComponent<CustomPassVolume>();
            volume.isGlobal = true;
            volume.injectionPoint = CustomPassInjectionPoint.BeforePreRefraction;
            volume.priority = 0f;
            ground = Pass("Snow, frost and wet ground", "Snow Frost Wet", fetchColour: true);
            aurora = Pass("Aurora", "Aurora", fetchColour: false);
        }

        FullScreenCustomPass Pass(string name, string passName, bool fetchColour)
        {
            var pass = volume.AddPassOfType<FullScreenCustomPass>();
            pass.name = name;
            pass.fullscreenPassMaterial = material;
            pass.materialPassName = passName;
            pass.fetchColorBuffer = fetchColour;
            pass.targetColorBuffer = CustomPass.TargetBuffer.Camera;
            pass.targetDepthBuffer = CustomPass.TargetBuffer.None;
            pass.clearFlags = ClearFlag.None;
            pass.enabled = false;
            return pass;
        }

        /// <summary>
        /// This frame's amounts: snow cover, frost and wetness (0..1), the water's level (m), the fog's extinction at its
        /// base (1/m), base height (m) and height falloff (1/m), the Sun's and the Moon's light on the ground (lux), the
        /// aurora's strength (0..1, already faded by the night) and its full light (nits).
        /// </summary>
        public void Push(float snow, float frost, float wetness, float waterLevel, float fogExtinction, float fogBase, float fogFalloff,
                         Color sunLux, Color moonLux, float auroraStrength, float auroraNits)
        {
            if (!Ready)
                return;
            // 4 Oct: the ground pass blacked out every frame it ran on (every weather with wetness, snow or frost: the opaque
            // image came back black at BeforePreRefraction, only the water and clouds drawn after it survived). Off until the
            // shader is rewritten against a frame; the aurora pass is unaffected.
            bool groundOn = false && (snow > 0.002f || frost > 0.002f || wetness > 0.01f);
            bool auroraOn = auroraStrength > 0.002f && auroraNits > 0f;
            ground.enabled = groundOn;
            aurora.enabled = auroraOn;
            volume.enabled = groundOn || auroraOn;
            if (!volume.enabled)
                return;
            material.SetVector(GroundId, new Vector4(snow, frost, wetness, waterLevel));
            material.SetVector(FogShapeId, new Vector4(fogExtinction, fogBase, fogFalloff, 0f));
            material.SetVector(SunLightId, new Vector4(sunLux.r, sunLux.g, sunLux.b, 0f));
            material.SetVector(MoonLightId, new Vector4(moonLux.r, moonLux.g, moonLux.b, 0f));
            material.SetVector(AuroraId, new Vector4(auroraStrength, auroraNits, Time.realtimeSinceStartup, 0f));
        }

        public void Dispose()
        {
            // the volume's object is switched off, not destroyed (it goes with its parent; destroying a child while the
            // hierarchy is coming down logs an error); the next SnowFrostPass replaces it
            if (volume != null)
                volume.enabled = false;
            if (go != null)
                go.SetActive(false);
            CoreUtils.Destroy(material);
            go = null;
            volume = null;
            material = null;
            ground = aurora = null;
        }
    }
}
