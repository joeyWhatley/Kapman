// ported 4 Oct: a SHIM for the first build's Kapman.Weather.LocalWeather (Code/Weather/LocalWeather.cs: the weather model
// sampled where the ship is, with fog banks). The rebuild has one weather for the whole level (Kapman.Levels.LevelWeather).
// The game layer (GameEvents; the first build's LocalBoats, SeaWatch, WorldShips3D, RaiderEncounter if they keep the
// call) asks LocalWeather.Active for .Now (a WeatherSample), .FogHere, .VisibilityKmTo and .Model (the simulation's own
// weather model, WeatherModel, which the world's ships sail by). This makes itself on the level's weather object in
// Play, and reads the level's weather into a WeatherSample each frame.
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Weather
{
    [DefaultExecutionOrder(-120)]
    public class LocalWeather : MonoBehaviour
    {
        static LocalWeather active;

        /// <summary>The weather where the ship is: made on the level's weather object on first use (null in edit mode or without a LevelWeather).</summary>
        public static LocalWeather Active
        {
            get
            {
                if (active == null && Application.isPlaying)
                {
                    var level = LevelWeather.Current;
                    if (level == null)
                        return null;
                    active = level.GetComponent<LocalWeather>();
                    if (active == null)
                        active = level.gameObject.AddComponent<LocalWeather>();
                }
                return active;
            }
        }

        [Tooltip("The seed of the world's own weather model (the simulation's; the same seed brings the same weather).")]
        public int seed = 793;
        [Tooltip("Whose weather this is (empty: the player's ship, else the camera).")]
        public Transform subject;

        /// <summary>The weather now, in the simulation's terms.</summary>
        public WeatherSample Now { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public bool OverSea { get; private set; } = true;
        /// <summary>The simulation's weather model (the world's ships sail by it; the level's own sky is LevelWeather's).</summary>
        public WeatherModel Model { get; private set; }

        /// <summary>Where the subject is, in the world frame.</summary>
        public WorldPos Here
        {
            get
            {
                var t = subject != null ? subject : ShipMotion.Player != null ? ShipMotion.Player.transform : Camera.main != null ? Camera.main.transform : transform;
                return FloatingOrigin.ToWorld(t.position);
            }
        }

        /// <summary>The fog where the ship is, 0..1 (one fog for the level: the first build read the bank she was in).</summary>
        public float FogHere => Now.fog;
        /// <summary>How far one can see, km.</summary>
        public double VisibilityKm => Now.VisibilityKm;
        /// <summary>How far one can see towards a place, km (the level's one fog: no banks between).</summary>
        public double VisibilityKmTo(WorldPos target) => Now.VisibilityKm;
        public string Summary => Now.Describe();

        void Awake()
        {
            Model = new WeatherModel(seed);
            Read();
        }

        void OnEnable()
        {
            active = this;
            if (Model == null)
                Model = new WeatherModel(seed);
        }

        void OnDisable()
        {
            if (active == this)
                active = null;
        }

        void Update() => Read();

        /// <summary>The level's weather as a WeatherSample.</summary>
        void Read()
        {
            var info = LevelInfo.Current;
            if (info != null)
            {
                Latitude = info.latitude;
                Longitude = info.longitude;
                OverSea = info.kind == LevelKind.Ocean;
            }
            var w = LevelWeather.Current;
            if (w == null)
                return;
            var v = w.WindVector;                                            // the way it blows to (east, north), m/s
            float blowsTo = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
            Now = new WeatherSample
            {
                windFrom = Mathf.Repeat(blowsTo + 180f, 360f),
                windSpeed = w.Wind,
                gustiness = 0.2f + 0.5f * w.Storm,
                cumulus = Mathf.Clamp01(w.CloudCover - w.Overcast),
                layered = w.Overcast,
                storm = w.Storm,
                rain = Mathf.Clamp01(w.Rain + w.Snowfall),
                fog = Mathf.Clamp01(Mathf.InverseLerp(0.0002f, 0.004f, w.FogDensity)),   // (LevelWeather: fair 0.00006 .. fog 0.0045)
                temperature = w.Temperature,
                pressure = 1013f - 25f * w.Storm - 8f * w.Overcast,
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics() => active = null;
    }
}
