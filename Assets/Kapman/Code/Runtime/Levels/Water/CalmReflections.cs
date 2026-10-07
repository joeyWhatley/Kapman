using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// CALM REFLECTIONS - in a glassy calm (and under ice) the water mirrors the ships, the dams, the houses and the
    /// trees, not just the sky: HDRP's screen-space reflections, switched on only while the water is glassy
    /// (HdrpSea.Glassy), so a breeze costs nothing. HDRP's water takes its reflections from the transparent SSR pass,
    /// so the HDRP asset must support SSR and transparent SSR (HdrpWater.EnableWater switches them on).
    /// </summary>
    [DefaultExecutionOrder(1030)]
    public class CalmReflections : MonoBehaviour
    {
        [Tooltip("The sky-and-fog volume (its runtime copy gets the reflections). Empty: the first global volume in the scene.")]
        public Volume volume;
        [Tooltip("How glassy the water must be (0-1) before the reflections come on.")]
        [Range(0f, 1f)] public float from = 0.35f;

        ScreenSpaceReflection ssr;

        void Start()
        {
            if (volume == null)
                foreach (var v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
                    if (v.isGlobal && v.profile != null)
                    {
                        volume = v;
                        break;
                    }
            if (volume == null || volume.profile == null)
            {
                enabled = false;
                return;
            }
            var profile = volume.profile;                                    // (the runtime copy: the asset is never changed)
            if (!profile.TryGet(out ssr))
                ssr = profile.Add<ScreenSpaceReflection>(true);
            ssr.enabled.Override(false);
            ssr.enabledTransparent.Override(true);                          // (the water is drawn as a transparent)
        }

        void LateUpdate()
        {
            if (ssr == null)
                return;
            var sea = HdrpSea.Current;
            bool on = sea != null && sea.Glassy >= from;
            if (ssr.enabled.value != on)
                ssr.enabled.Override(on);
        }
    }
}
