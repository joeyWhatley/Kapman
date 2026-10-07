using System.Collections.Generic;
using Kapman.Water;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// THE LEVEL'S OWN WATER (the user, 4 Oct: "each level has to have the exact right and correct water"): reads the
    /// level's record from water_793.json (chain.json names it: water.profile) and, once a real second, sets the water from
    /// it by the day of the year and the weather's flood: the river's stream (low in August, in flood in winter), the
    /// colour and the clarity of the water body (HDRP's scattering, refraction and absorption distance from the record's
    /// colour and Secchi depth), and the water's temperature for whatever asks (ice, sea smoke). RiverTint still moves the
    /// river's colour with rain and frost, now around these resting values.
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    public class WaterProfileDriver : MonoBehaviour
    {
        public static WaterProfileDriver Current { get; private set; }

        public TextAsset data;
        [Tooltip("The record's id in water_793.json (chain.json: the level's water.profile).")]
        public string recordId;

        public WaterProfile Profile { get; private set; }
        /// <summary>The water's temperature now (C), its stream (m/s) and its Secchi depth (m).</summary>
        public float Temperature { get; private set; }
        public float Stream { get; private set; }
        public float Secchi { get; private set; }

        WaterSurface surface;
        RiverCurrent current;
        RiverTint tint;
        float next;
        static readonly Dictionary<string, WaterProfile> cache = new Dictionary<string, WaterProfile>();

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        void Start()
        {
            surface = GetComponent<WaterSurface>();
            current = GetComponent<RiverCurrent>();
            tint = GetComponent<RiverTint>();
            if (data == null || string.IsNullOrEmpty(recordId))
            {
                Debug.LogWarning($"Kapman water: '{name}' has no water record; it keeps the builder's water.");
                enabled = false;
                return;
            }
            if (!cache.TryGetValue(recordId, out var p))
            {
                foreach (var kv in WaterProfile.ParseAll(data.text))
                    cache[kv.Key] = kv.Value;
                cache.TryGetValue(recordId, out p);
            }
            Profile = p;
            if (Profile == null)
            {
                Debug.LogWarning($"Kapman water: no record '{recordId}' in water_793.json; '{name}' keeps the builder's water.");
                enabled = false;
                return;
            }
            Apply();
        }

        void Update()
        {
            if (Time.unscaledTime >= next)
                Apply();
        }

        void Apply()
        {
            next = Time.unscaledTime + 1f;
            var clock = LevelClock.Current;
            var weather = LevelWeather.Current;
            float day = clock != null ? clock.DayOfYear : 172f;
            float spate = weather != null ? weather.flood : 0f;
            Temperature = Profile.Temperature(day);
            Secchi = Profile.Secchi(day);
            Stream = Profile.Current(day, spate);
            if (current != null && Profile.HasRiver)
                current.speed = Stream;
            float absorption = WaterProfile.AbsorptionDistance(Secchi);
            // a turbid river scatters the light back (the silt): brighter from above; the sea keeps HdrpSea's own
            if (surface != null && surface.surfaceType == WaterSurfaceType.River)
            {
                float t = Profile.Turbidity;
                surface.ambientScattering = Mathf.Lerp(0.15f, 0.8f, t);
                surface.directLightBodyScattering = Mathf.Lerp(0.8f, 1.2f, t);
                surface.heightScattering = Mathf.Lerp(0.2f, 0.4f, t);
            }
            if (tint != null)
                tint.SetRest(Profile.Refraction, Profile.Scattering, absorption, WaterProfile.AbsorptionDistance(Profile.secchiFlood));
            else if (surface != null)
            {
                surface.refractionColor = Profile.Refraction;
                surface.scatteringColor = Profile.Scattering;
                surface.absorptionDistance = absorption;
            }
        }

        /// <summary>A line for the H panel.</summary>
        public string Line =>
            Profile == null ? "" :
            $"{Profile.name}: {Temperature:0} C, Secchi {Secchi:0.0} m" + (Profile.HasRiver ? $", stream {Stream:0.00} m/s" : "");
    }
}
