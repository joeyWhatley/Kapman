using System;
using Kapman.Levels.Hdrp;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Kapman.Levels
{
    public enum WeatherKind { Fair, Haze, Mist, Fog, Overcast, Rain, Storm, Snow }

    /// <summary>
    /// THE WEATHER AND THE SEASONS - one wind and one sky for everything: clouds, fog, ground mist at dawn and dusk on the
    /// river land, rain, storms with lightning and thunder, snow that lies and melts with the temperature, frost on cold
    /// mornings, ice along the banks and then across the river in a hard winter, the trees leafing out in spring and
    /// turning and falling in autumn, floods, and now and then an aurora. The weather is picked by hand (the default, and
    /// G in Play) or by the weather engine (manual off: Kapman.Weather.WeatherEngine, the place's real climate, its fronts
    /// and lows, and since night 2 the sky itself: cloud layers and kinds, their base and depth, the light through them,
    /// rain, sleet and snow, fog and the visibility, thunder; the seasonal die is the fallback without the engine's data).
    /// On HDRP the weather is drawn by <see cref="HdrpWeatherDraw"/>: HDRP's volumetric clouds, its volumetric fog, a
    /// local fog volume for the mist on the water, the rain and snow particles, and two full-screen passes of our own
    /// (snow, frost and wet ground on what faces up; the aurora in the north).
    /// Keys in Play: G next weather, J aurora, K flood stage.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-150)]
    public class LevelWeather : MonoBehaviour
    {
        public static LevelWeather Current { get; private set; }

        [Tooltip("On: the weather below holds. Off: the season and the day pick it.")]
        public bool manual = true;
        public WeatherKind kind = WeatherKind.Fair;
        [Tooltip("For testing the seasons: a day of the year to take the season from (-1: the clock's date).")]
        public int seasonDay = -1;
        [Tooltip("How high the water stands over its resting level: 0 none, 1 the worst flood (LevelWater's flood rise).")]
        [Range(0f, 1f)] public float flood;
        [Tooltip("An aurora tonight, whatever the dice say.")]
        public bool forceAurora;
        [Tooltip("How far the ground mist rises over the water (m).")]
        public float mistHeight = 5f;

        [Header("Wired by the level builder")]
        [Tooltip("Unused on HDRP (the URP atmosphere pass's material). Kept so older builders compile; pass null.")]
        public Material atmosphere;
        [Tooltip("Unused on HDRP (the URP water material: rain rings and ice are the water owner's now). Kept so older builders compile.")]
        public Material water;
        [Tooltip("The leaf materials (HDRP/Lit, alpha clipped): their cutoff and colour follow the season.")]
        public Material[] leaves;
        public Color[] leafColours;
        /// <summary>Each leaf material's own colour when the level started: its summer (4 Oct: the builders' leafColours
        /// list had the far trees' impostors undimmed, so Play turned them pale and wrote that into the files).</summary>
        Color[] summerColours;

        /// <summary>Material writes only when the value moves (a shared asset: an unchanged write still marks it changed,
        /// and the editor then rewrites the file with rounding noise).</summary>
        static void SetIfChanged(Material m, int id, Color c)
        {
            var now = m.GetColor(id);
            if (Mathf.Abs(now.r - c.r) + Mathf.Abs(now.g - c.g) + Mathf.Abs(now.b - c.b) + Mathf.Abs(now.a - c.a) > 1e-4f)
                m.SetColor(id, c);
        }

        static void SetIfChanged(Material m, int id, float v)
        {
            if (Mathf.Abs(m.GetFloat(id) - v) > 1e-5f)
                m.SetFloat(id, v);
        }

        Color Summer(int i)
        {
            if (summerColours == null || summerColours.Length != (leaves != null ? leaves.Length : 0))
            {
                summerColours = new Color[leaves != null ? leaves.Length : 0];
                for (int k = 0; k < summerColours.Length; k++)
                    summerColours[k] = leaves[k] != null && leaves[k].HasProperty(BaseColorId) ? leaves[k].GetColor(BaseColorId)
                                     : leafColours != null && k < leafColours.Length ? leafColours[k] : Color.white;
            }
            return i < summerColours.Length ? summerColours[i] : Color.white;
        }
        public float[] leafCutoffs;
        public ParticleSystem rain;
        public ParticleSystem snow;

        [Header("HDRP")]
        [Tooltip("The level's sky volume (global). Empty: the sky dome's, else the highest-priority global Volume in the scene.")]
        public Volume volume;
        [Tooltip("The clouds' base and depth (m).")]
        public float cloudBase = 1000f, cloudDepth = 1500f;
        [Tooltip("How wide the patches of cloud and clear sky are (m).")]
        public float cloudPatch = 1200f;
        [Tooltip("The full-screen weather passes' shader (Hidden/Kapman/Weather Passes). Empty: found by name.")]
        public Shader weatherShader;
        [Tooltip("How far the mist on the water is drawn around the camera (m).")]
        public float mistExtent = 10000f;
        [Tooltip("How wide the mist's patches are (m).")]
        public float mistPatch = 70f;
        [Tooltip("The aurora's light at full strength (nits, about the night sky's brightness times a few).")]
        public float auroraLight = 0.02f;

        public float CloudCover { get; private set; } = 0.3f;
        public float CloudDark { get; private set; } = 0.2f;
        public float Overcast { get; private set; }
        public float Rain { get; private set; }
        public float Snowfall { get; private set; }
        public float SnowCover { get; private set; }
        public float Frost { get; private set; }
        public float Wetness { get; private set; }
        public float Wind { get; private set; } = 3f;
        public Vector2 WindVector { get; private set; } = new Vector2(2.5f, 1.5f);
        public float FogDensity { get; private set; } = 0.00015f;
        public float MistDensity { get; private set; }
        public float Storm { get; private set; }
        public float Aurora { get; private set; }
        public float Leaf { get; private set; } = 1f;
        public float Autumn { get; private set; }
        public float Ice { get; private set; }
        public float Temperature { get; private set; } = 15f;

        // the weather engine's sky (weather document 2.3 and 2.5, night 2), eased like the rest; set by hand (manual, the
        // G key, a shot) these follow the kinds' numbers and SkyFromEngine is false

        /// <summary>True while the weather engine drives the sky (manual off and the engine's data present).</summary>
        public bool SkyFromEngine { get; private set; }
        /// <summary>This frame's moment in the weather engine's time, game hours (WeatherEngine.HoursAt of the clock's year,
        /// the season's day and the hour).</summary>
        public double Hours { get; private set; } = double.NaN;
        /// <summary>The engine's latest sample for this place (default while the weather is set by hand).</summary>
        public Kapman.Weather.WeatherSample EngineSample => engineSample;
        /// <summary>The gusts about the wind, m/s.</summary>
        public float Gust { get; private set; } = 4.5f;
        /// <summary>How far one can see: the meteorological optical range, m (3.912 / FogDensity).</summary>
        public float Visibility { get; private set; } = 26000f;
        /// <summary>The cloud's base above the ground and its depth, m: the engine's lowest layer's, or the fields above.</summary>
        public float CloudBase => SkyFromEngine ? skyBase : cloudBase;
        public float CloudDepth => SkyFromEngine ? skyDepth : cloudDepth;
        /// <summary>The kind of the lowest layer that covers a quarter of the sky (the engine's).</summary>
        public Kapman.Weather.CloudType CloudType { get; private set; }
        /// <summary>Cover of the low, middle and high layers, 0..1.</summary>
        public float LowCloud { get; private set; }
        public float MidCloud { get; private set; }
        public float HighCloud { get; private set; }
        /// <summary>The share of the Sun's light the cloud column passes, 0..1 (weather 2.5): the clock dims the Sun by it.</summary>
        public float CloudTransmittance { get; private set; } = 1f;
        /// <summary>The share of the Sun HDRP's own volumetric cloud shadows take on average (so the clock does not count
        /// the cloud twice: LightingModel.CloudDim).</summary>
        public float HdrpCloudShadow => draw != null ? draw.CloudShadowShare : 0f;
        /// <summary>Thunder near, 0..1 (lightning flashes from 0.3).</summary>
        public float Thunder { get; private set; }
        /// <summary>The dew point, C.</summary>
        public float DewPoint { get; private set; } = 8f;
        /// <summary>The top of the fog or haze over the ground, m.</summary>
        public float FogTop { get; private set; } = 400f;
        /// <summary>The sea the weather has raised (HdrpSea's sea state; not set on a river or before the sea's first step).</summary>
        public Kapman.Weather.SeaState Sea => Kapman.Levels.Water.HdrpSea.Current != null ? Kapman.Levels.Water.HdrpSea.Current.Sea : default;
        float skyBase = 1000f, skyDepth = 1500f;

        /// <summary>Lightning has struck this far away (m): the thunder follows at the speed of sound.</summary>
        public event Action<float> Lightning;

        public string Description =>
            $"{Name(kind)}, {Temperature:0} °C, wind {Wind:0} m/s" + (SnowCover > 0.2f ? ", snow lying" : "") +
            (Ice > 0.3f ? ", ice on the river" : "") + (Aurora > 0.2f ? ", an aurora" : "") + (flood > 0.05f ? ", the river in flood" : "") +
            (SkyFromEngine ? $"; {Kapman.Weather.CloudLight.Name(CloudType)} at {CloudBase:0} m, light {CloudTransmittance:0.000}, sees {Visibility / 1000f:0.0} km" : "") +
            (Sea.set ? $"; sea {Sea.hs:0.0} m {Sea.tp:0} s" + (Sea.swellHs > 0.2f ? $" (swell {Sea.swellHs:0.0} m {Sea.swellTp:0} s from {Sea.swellFrom:0})" : "") : "");

        static readonly int AlphaCutoffId = Shader.PropertyToID("_AlphaCutoff"), BaseColorId = Shader.PropertyToID("_BaseColor");

        float snowLying;
        float flashLeft, nextFlash;

        // the weather engine's sample (Kapman.Weather: the real climate of the place, its fronts and lows over days), taken
        // once a game minute when the weather is not set by hand
        Kapman.Weather.WeatherSample engineSample;
        double sampledAt = double.NaN;
        bool engineOk = true;

        /// <summary>The engine's weather kind for a sample (for the panel, the mist and the aurora; the numbers come from the sample).</summary>
        static WeatherKind KindOf(Kapman.Weather.WeatherSample s)
        {
            float vis = s.VisibilityKm;
            if (s.storm > 0.5f && s.windSpeed > 14f) return WeatherKind.Storm;
            if (s.Snow) return WeatherKind.Snow;
            if (s.rain > 0.1f) return WeatherKind.Rain;
            if (vis < 1f) return WeatherKind.Fog;
            if (vis < 4f) return WeatherKind.Mist;
            if (s.Cover > 0.8f) return WeatherKind.Overcast;
            if (vis < 10f) return WeatherKind.Haze;
            return WeatherKind.Fair;
        }

        /// <summary>The engine's sample for this place and time, or false if the engine cannot run (its data missing).</summary>
        bool Engine(int year, int day, float hour, out Kapman.Weather.WeatherSample s)
        {
            s = engineSample;
            if (!engineOk)
                return false;
            double hours = Kapman.Weather.WeatherEngine.HoursAt(year, day, hour);
            if (!double.IsNaN(sampledAt) && Math.Abs(hours - sampledAt) < 1.0 / 60.0)
                return true;
            try
            {
                var info = LevelInfo.Current;
                double lat = info != null ? info.latitude : 52.0, lon = info != null ? info.longitude : 5.0;
                var engine = Kapman.Weather.WeatherEngine.Shared;
                // the open sea's class at sea; on land the class whose station is nearest (De Bilt for Dorestad, the coast's
                // own stations for the coast, so the sea breeze and the coast's fog come with them)
                string place = info != null && info.kind == LevelKind.Ocean ? "north_sea" : engine.ClassFor(lat, lon, false) ?? "river_land";
                engineSample = s = engine.Sample(lat, lon, hours, place);
                sampledAt = hours;
                return true;
            }
            catch (Exception e)
            {
                engineOk = false;
                Debug.LogWarning("Kapman weather: the weather engine cannot run (" + e.Message + "); the weather follows the kinds by season.");
                return false;
            }
        }
        System.Random dice = new System.Random(793);
        HdrpWeatherDraw draw;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            draw = new HdrpWeatherDraw(this);
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            draw?.Dispose();
            draw = null;
            LevelClock.Flash = 0f;
            // the leaves are shared material assets: put summer back, or Play in the editor leaves autumn in the files
            if (leaves != null)
                for (int i = 0; i < leaves.Length; i++)
                    if (leaves[i] != null)
                    {
                        SetIfChanged(leaves[i], AlphaCutoffId, leafCutoffs != null && i < leafCutoffs.Length ? leafCutoffs[i] : 0.4f);
                        SetIfChanged(leaves[i], BaseColorId, Summer(i));
                    }
        }

        void Update()
        {
            var clock = LevelClock.Current;
            int day = seasonDay > 0 ? seasonDay : clock != null ? clock.DayOfYear : 172;
            float hour = clock != null ? clock.HourOfDay : 12f;
            int year = clock != null ? clock.Year : 793;
            bool playing = Application.isPlaying;

            if (playing)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.gKey.wasPressedThisFrame)
                    {
                        manual = true;
                        kind = (WeatherKind)(((int)kind + 1) % Enum.GetValues(typeof(WeatherKind)).Length);
                    }
                    if (kb.jKey.wasPressedThisFrame)
                        forceAurora = !forceAurora;
                    if (kb.kKey.wasPressedThisFrame)
                        flood = flood >= 0.99f ? 0f : Mathf.Min(1f, flood + 0.25f);
                }
            }
            // the weather engine drives the weather unless it is set by hand (G); the old seasonal kinds are the fallback
            bool driven = !manual && clock != null && Engine(year, day, hour, out var es);
            SkyFromEngine = driven;
            Hours = Kapman.Weather.WeatherEngine.HoursAt(year, day, hour);
            if (driven)
                kind = KindOf(engineSample);
            else if (!manual && clock != null)
                kind = Auto(year, day, hour);

            // the season: the trees, the temperature
            Leaf = Mathf.Clamp01(Mathf.InverseLerp(100f, 135f, day)) * (1f - Mathf.Clamp01(Mathf.InverseLerp(285f, 320f, day)));
            Autumn = Mathf.Clamp01(Mathf.InverseLerp(258f, 292f, day));
            float mean = 9.8f - 7.8f * Mathf.Cos(2f * Mathf.PI * (day - 20f) / 365f);
            float daily = 4f * Mathf.Sin(2f * Mathf.PI * (hour - 9f) / 24f);

            var t = Targets(kind, hour);
            if (driven)
            {
                var s = engineSample;
                t.cloud = s.Cover;
                t.overcast = s.layered;
                t.storm = s.storm;
                // what falls: rain over 3 C, snow under 1, sleet (both) between
                float snowShare = s.SnowShare;
                t.rain = s.rain * (1f - snowShare);
                t.snow = s.rain * snowShare;
                t.wind = s.windSpeed;
                // the deck's darkness from the light it lets through (a nimbostratus deck about 0.65, a storm's 0.75, a
                // fair cumulus sky 0.1-0.4)
                t.dark = s.HasSky ? Mathf.Clamp01(1f - Mathf.Pow(Mathf.Max(s.cloudTransmittance, 1e-4f), 0.25f))
                                  : Mathf.Clamp01(0.15f + 0.5f * s.storm + 0.3f * s.rain);
                t.fog = 3.912f / Mathf.Max(60f, s.VisibilityKm * 1000f);        // extinction from the visibility (MOR = 3.912 / extinction)
                t.wet = Mathf.Max(t.wet, s.rain);
            }
            float temp = driven ? engineSample.temperature : mean + daily * (1f - t.cloud * 0.6f) + t.tempOffset;
            Temperature = temp;

            float ease = playing ? 1f - Mathf.Exp(-Time.deltaTime * Mathf.Max(LevelClock.TimeFactor, 0.2f) * 0.5f) : 1f;
            CloudCover = Mathf.Lerp(CloudCover, t.cloud, ease);
            CloudDark = Mathf.Lerp(CloudDark, t.dark, ease);
            Overcast = Mathf.Lerp(Overcast, t.overcast, ease);
            Rain = Mathf.Lerp(Rain, driven ? t.rain : temp < 1f ? 0f : t.rain, ease);
            Snowfall = Mathf.Lerp(Snowfall, driven ? t.snow : temp < 2f ? Mathf.Max(t.snow, t.rain) : 0f, ease);
            Storm = Mathf.Lerp(Storm, t.storm, ease);
            Wind = Mathf.Lerp(Wind, t.wind, ease);
            float windDir = driven ? engineSample.windFrom : 225f + 40f * Mathf.Sin(day * 0.37f + year);    // the engine's, else mostly from the south-west
            Vector3 wv = Quaternion.Euler(0f, windDir + 180f, 0f) * Vector3.forward * Wind;
            WindVector = new Vector2(wv.x, wv.z);
            FogDensity = Mathf.Lerp(FogDensity, t.fog, ease);
            Visibility = 3.912f / Mathf.Max(FogDensity, 1e-6f);
            if (driven)
            {
                // the engine's sky, eased: the clouds' base, depth and layers, the light through them, thunder, the fog's top
                var s = engineSample;
                Gust = Mathf.Lerp(Gust, s.gust, ease);
                skyBase = Mathf.Lerp(skyBase, s.cloudBase, ease);
                skyDepth = Mathf.Lerp(skyDepth, s.cloudDepth, ease);
                CloudType = s.cloudType;
                LowCloud = Mathf.Lerp(LowCloud, s.lowCloud, ease);
                MidCloud = Mathf.Lerp(MidCloud, s.midCloud, ease);
                HighCloud = Mathf.Lerp(HighCloud, s.highCloud, ease);
                CloudTransmittance = Mathf.Lerp(CloudTransmittance, s.cloudTransmittance, ease);
                Thunder = Mathf.Lerp(Thunder, s.thunder, ease);
                DewPoint = Mathf.Lerp(DewPoint, s.dewPoint, ease);
                FogTop = Mathf.Lerp(FogTop, s.fogTop, ease);
            }
            else
            {
                // set by hand: the kinds' numbers (the clock keeps its fixed overcast light; lightning follows Storm)
                Gust = Wind * 1.55f;
                skyBase = cloudBase;
                skyDepth = cloudDepth;
                LowCloud = CloudCover;
                MidCloud = Overcast * 0.5f;
                HighCloud = 0f;
                CloudTransmittance = (float)Kapman.World.LightingModel.CloudTransmittance(Overcast, Storm);
                Thunder = Storm;
                DewPoint = temp - 3f;
                FogTop = Mathf.Lerp(400f, 60f, Mathf.InverseLerp(0.0005f, 0.004f, FogDensity));
            }
            float dawn = Bell(hour, 5.5f, 2f) + 0.6f * Bell(hour, 21.5f, 1.5f);
            float mistSeason = Mathf.Clamp01(Mathf.InverseLerp(150f, 230f, day)) + Mathf.Clamp01(Mathf.InverseLerp(330f, 260f, day)) * 0.6f;
            float mistTarget = t.mist + (kind <= WeatherKind.Mist ? 0.012f * dawn * Mathf.Clamp01(mistSeason) * Mathf.InverseLerp(5f, 1f, Wind) : 0f);
            MistDensity = Mathf.Lerp(MistDensity, mistTarget, ease);
            Wetness = Mathf.Lerp(Wetness, Mathf.Max(t.wet, Rain), playing ? ease * 0.3f : 1f);

            // snow lies while it's cold and melts when it isn't
            if (playing)
            {
                float gameHours = Time.deltaTime * LevelClock.TimeFactor * 60f / 3600f;
                snowLying += (Snowfall * 0.4f - (temp > 1f ? (temp - 1f) * 0.05f : 0f)) * gameHours;
                snowLying = Mathf.Clamp01(snowLying);
            }
            else
                snowLying = kind == WeatherKind.Snow ? 0.85f : temp < 0f && (day > 330 || day < 50) ? 0.3f : 0f;
            SnowCover = snowLying;
            Frost = temp < 1f && SnowCover < 0.6f ? Mathf.InverseLerp(1f, -4f, temp) * (1f - Rain) : 0f;
            float winter = Mathf.InverseLerp(3f, -6f, mean + t.tempOffset * 0.5f);
            Ice = Mathf.Clamp01(winter * (kind == WeatherKind.Snow ? 1f : 0.6f));

            // the aurora: rare, on clear dark nights (and when asked for)
            bool auroraNight = forceAurora || Hash(year, day) < 0.015f;
            Aurora = Mathf.Lerp(Aurora, auroraNight ? (1f - CloudCover * 0.8f) : 0f, ease);

            // lightning in a thunderstorm (the engine's thunder; set by hand, the storm kind's)
            float strike = driven ? Thunder : Storm;
            if (playing && strike > (driven ? 0.3f : 0.5f))
            {
                if (Time.time > nextFlash)
                {
                    flashLeft = 0.18f + (float)dice.NextDouble() * 0.25f;
                    nextFlash = Time.time + (3f + (float)dice.NextDouble() * 12f) / strike;
                    // the nearer the storm, the nearer the strikes
                    Lightning?.Invoke(300f + (float)dice.NextDouble() * 6000f * (driven ? Mathf.Clamp(1.3f - Thunder, 0.3f, 1f) : 1f));
                }
            }
            if (flashLeft > 0f)
            {
                flashLeft -= Time.deltaTime;
                LevelClock.Flash = flashLeft > 0f ? Mathf.Clamp01(flashLeft * 6f) * (0.6f + 0.4f * Mathf.PerlinNoise(Time.time * 30f, 0f)) : 0f;
            }
            else
                LevelClock.Flash = 0f;

            Push();
        }

        void LateUpdate()
        {
            // after the clock has placed the Sun and the Moon: the clouds, the fog, the mist and the passes
            draw?.Push();
        }

        struct Target
        {
            public float cloud, dark, overcast, rain, snow, storm, wind, fog, mist, wet, tempOffset;
        }

        static Target Targets(WeatherKind k, float hour)
        {
            switch (k)
            {
                case WeatherKind.Haze: return new Target { cloud = 0.4f, dark = 0.15f, wind = 2f, fog = 0.00025f, tempOffset = 1f };
                case WeatherKind.Mist: return new Target { cloud = 0.25f, dark = 0.1f, wind = 1f, fog = 0.0009f, mist = 0.02f, wet = 0.2f };
                case WeatherKind.Fog: return new Target { cloud = 0.6f, dark = 0.2f, overcast = 0.5f, wind = 1f, fog = 0.0045f, mist = 0.01f, wet = 0.3f, tempOffset = -1f };
                case WeatherKind.Overcast: return new Target { cloud = 0.88f, dark = 0.35f, overcast = 0.65f, wind = 5f, fog = 0.0005f, wet = 0.1f };
                case WeatherKind.Rain: return new Target { cloud = 0.96f, dark = 0.45f, overcast = 0.8f, rain = 0.6f, wind = 6f, fog = 0.0012f, wet = 1f, tempOffset = -2f };
                case WeatherKind.Storm: return new Target { cloud = 1f, dark = 0.7f, overcast = 0.95f, rain = 1f, storm = 1f, wind = 16f, fog = 0.0015f, wet = 1f, tempOffset = -3f };
                case WeatherKind.Snow: return new Target { cloud = 0.95f, dark = 0.3f, overcast = 0.75f, snow = 0.8f, wind = 4f, fog = 0.002f, wet = 0.2f, tempOffset = -9f };
                default: return new Target { cloud = 0.45f, dark = 0.15f, wind = 3f, fog = 0.00006f };
            }
        }

        /// <summary>The season on the leaves (HDRP/Lit: _AlphaCutoff and _BaseColor), and the rain and snow falling.</summary>
        void Push()
        {
            if (leaves != null)
            {
                for (int i = 0; i < leaves.Length; i++)
                {
                    var m = leaves[i];
                    if (m == null)
                        continue;
                    float cut = leafCutoffs != null && i < leafCutoffs.Length ? leafCutoffs[i] : 0.4f;
                    SetIfChanged(m, AlphaCutoffId, Mathf.Lerp(1f, cut, Leaf));
                    Color summer = Summer(i);
                    Color autumn = new Color(0.95f, 0.62f, 0.32f);
                    SetIfChanged(m, BaseColorId, Color.Lerp(summer, summer * autumn, Autumn));
                }
            }
            SetRate(rain, Rain * 5000f, WindVector * 0.35f);
            SetRate(snow, Snowfall * 2500f, WindVector * 0.5f);
        }

        /// <summary>Rain or snow falling at a rate, drifting with the wind (m/s over the ground).</summary>
        static void SetRate(ParticleSystem ps, float rate, Vector2 drift)
        {
            if (ps == null)
                return;
            var em = ps.emission;
            em.rateOverTime = rate;
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(drift.x);              // (all three in the same mode, as Unity needs)
            velocity.y = new ParticleSystem.MinMaxCurve(0f);
            velocity.z = new ParticleSystem.MinMaxCurve(drift.y);
            if (rate > 1f && !ps.isPlaying && Application.isPlaying)
                ps.Play();
        }

        /// <summary>The season's weather for a six-hour stretch: the same date always brings the same weather.</summary>
        static WeatherKind Auto(int year, int day, float hour)
        {
            int block = (int)(hour / 6f);
            float r = Hash(year * 7 + block, day);
            bool summer = day >= 152 && day < 244, autumn = day >= 244 && day < 335, winter = day >= 335 || day < 60;
            float[] p = summer ? new[] { 0.40f, 0.18f, 0.06f, 0.02f, 0.16f, 0.12f, 0.06f, 0f }
                      : autumn ? new[] { 0.20f, 0.08f, 0.12f, 0.12f, 0.22f, 0.18f, 0.08f, 0f }
                      : winter ? new[] { 0.15f, 0.05f, 0.05f, 0.15f, 0.25f, 0.10f, 0.08f, 0.17f }
                      : new[] { 0.30f, 0.10f, 0.08f, 0.04f, 0.22f, 0.22f, 0.04f, 0f };
            float acc = 0f;
            for (int i = 0; i < p.Length; i++)
            {
                acc += p[i];
                if (r < acc)
                    return (WeatherKind)i;
            }
            return WeatherKind.Fair;
        }

        static float Hash(int a, int b)
        {
            unchecked
            {
                uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        static float Bell(float x, float centre, float width)
        {
            float d = (x - centre) / width;
            return Mathf.Exp(-d * d);
        }

        public static string Name(WeatherKind k)
        {
            switch (k)
            {
                case WeatherKind.Haze: return "Hazy";
                case WeatherKind.Mist: return "Misty";
                case WeatherKind.Fog: return "Fog";
                case WeatherKind.Overcast: return "Overcast";
                case WeatherKind.Rain: return "Rain";
                case WeatherKind.Storm: return "Storm";
                case WeatherKind.Snow: return "Snow";
                default: return "Fair";
            }
        }
    }
}
