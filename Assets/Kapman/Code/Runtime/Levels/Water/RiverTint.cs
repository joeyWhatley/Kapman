using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// RIVER WATER'S COLOUR UNDER THE WEATHER - the level builder gives the river its resting colour (the brown-green of
    /// the silty Kromme Rijn and Lek, HdrpWater.MakeRiver). This moves it with the weather, as a real lowland river goes:
    /// after rain and in a flood it runs browner and thicker with silt (the bed shows only in the first few centimetres);
    /// in a hard frost and under ice it clears a little. Everything eases over a few seconds. In the first build this
    /// blended river water into the sea over a plume; here each level is all river or all sea, so the plume is gone.
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    public class RiverTint : MonoBehaviour
    {
        [Header("In spate (a flood, or after heavy rain)")]
        public Color spateRefraction = new Color(0.52f, 0.40f, 0.20f);
        public Color spateScattering = new Color(0.46f, 0.34f, 0.16f);
        [Tooltip("Metres of water the light goes through in spate before it's lost (the resting river's is ~1.4).")]
        public float spateAbsorption = 0.6f;
        [Header("Clear (a hard frost, ice)")]
        public Color clearRefraction = new Color(0.30f, 0.38f, 0.26f);
        public Color clearScattering = new Color(0.22f, 0.30f, 0.20f);
        public float clearAbsorption = 2.6f;
        [Tooltip("Seconds a change takes.")]
        public float ease = 4f;

        WaterSurface surface;
        Color restRefraction, restScattering;
        float restAbsorption, restCaustics;
        float spate, clear;

        void Awake()
        {
            surface = GetComponent<WaterSurface>();
            restRefraction = surface.refractionColor;
            restScattering = surface.scatteringColor;
            restAbsorption = surface.absorptionDistance;
            restCaustics = surface.causticsIntensity;
        }

        /// <summary>The river's resting water from its record (WaterProfileDriver: the season's colour and clarity) and its
        /// clarity in spate; the weather's moves are around these.</summary>
        public void SetRest(Color refraction, Color scattering, float absorption, float spateAbsorption)
        {
            if (surface == null)
                Awake();
            restRefraction = refraction;
            restScattering = scattering;
            restAbsorption = absorption;
            this.spateAbsorption = spateAbsorption;
        }

        void OnDisable()
        {
            if (surface == null)
                return;
            surface.refractionColor = restRefraction;
            surface.scatteringColor = restScattering;
            surface.absorptionDistance = restAbsorption;
            surface.causticsIntensity = restCaustics;
        }

        void Update()
        {
            var w = LevelWeather.Current;
            if (w == null || surface == null)
                return;
            float spateTarget = Mathf.Clamp01(Mathf.Max(w.flood, w.Wetness * 0.6f + w.Rain * 0.3f));
            float clearTarget = Mathf.Clamp01(Mathf.Max(w.Frost * 0.6f, w.Ice)) * (1f - spateTarget);
            float k = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.05f, ease));
            spate += (spateTarget - spate) * k;
            clear += (clearTarget - clear) * k;
            var refraction = Color.Lerp(Color.Lerp(restRefraction, spateRefraction, spate), clearRefraction, clear);
            var scattering = Color.Lerp(Color.Lerp(restScattering, spateScattering, spate), clearScattering, clear);
            surface.refractionColor = refraction;
            surface.scatteringColor = scattering;
            surface.absorptionDistance = Mathf.Lerp(Mathf.Lerp(restAbsorption, spateAbsorption, spate), clearAbsorption, clear);
            surface.causticsIntensity = restCaustics * (1f - 0.8f * spate) * (1f + 0.5f * clear);
        }
    }
}
