using System;
using Kapman.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// THE CLOCK - the game's time, as set on 26 and 28 September: 1 real second is 1 game minute at 1x, with 2x and 4x
    /// in single player, and never faster when you zoom out. It turns the sky for the level's real place (SkyMath: the
    /// Sun and the Moon as they stood in 793) and lights the world from them with HDRP lights in real units: the Sun at
    /// its true 130,000 lux outside the air (HDRP's Physically Based Sky thins and reddens it on the way down, to about
    /// 100,000 lux at noon and a few hundred on the horizon, and to nothing once it's under), with its true half-degree
    /// disc; the Moon where it really stands, lit by the Sun so its phase comes out right, giving its true light (0.3
    /// lux at full) times a little; a faint fill on moonless nights; and lightning as a light of its own. HDRP's
    /// exposure then does what the eye does. The sky's colours are still worked out for the water, the mist and the
    /// snow (the public statics and the shader globals, which other code reads).
    /// Keys in Play: Tab 1x/2x/4x, Space pause, T an hour on (Shift+T back), Y a day on (Shift+Y back), U a month on
    /// (Shift+U back).
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class LevelClock : MonoBehaviour
    {
        [Tooltip("Game seconds per real second at 1x (60: a real second is a game minute).")]
        public float rate = 60f;
        public int year = 793;
        [Tooltip("Day of the year the level starts on (172 = 21 June, Julian calendar).")]
        [Range(1, 365)] public int startDay = 172;
        [Tooltip("The hour the level starts at (local mean time: noon when the Sun is south).")]
        [Range(0f, 24f)] public float startHour = 19f;
        public Light sun;
        public Light moon;

        [Header("Light, in real units")]
        [Tooltip("The Sun's light outside the air (lux). HDRP's sky thins it to the ground's 100,000 at noon and reddens it low down.")]
        public float sunLux = 130000f;
        [Tooltip("The Sun's and the Moon's discs (degrees across): also the softness of their shadows.")]
        public float sunDiameter = 0.53f, moonDiameter = 0.52f;
        [Tooltip("How much brighter than the true Moon its light is on the scene (the camera can't adapt as far as the eye).")]
        [Min(0f)] public float moonBoost = 3f;
        [Tooltip("The colour of moonlight (the eye sees it a little blue).")]
        public Color moonColour = new Color(0.78f, 0.85f, 1f);
        [Tooltip("The Moon's surface (none: a plain disc).")]
        public Texture moonSurface;
        [Tooltip("The Moon's disc's brightness by day and at full night (HDRP draws it from the Sun's light; at night's exposure the true value burns out white).")]
        public float moonDayTint = 0.15f, moonNightTint = 0.002f;
        [Tooltip("A faint light from above on moonless nights, lux (the eye adapts further than the camera can).")]
        [Min(0f)] public float nightFill = 0.04f;
        [Tooltip("The Sun keeps the one shadow-casting light down to this altitude (degrees); below it the Moon takes it while it's up.")]
        public float sunShadowsTo = -10f;
        [Tooltip("How much of the Sun's light a full overcast lets through to the ground (on top of HDRP's cloud shadows), for " +
                 "weather set by hand. Under the weather engine the cloud's own transmittance dims the Sun instead.")]
        [Range(0.05f, 1f)] public float overcastLight = 0.45f;
        public static float DiagOvercastLight = -1f;    // (diagnostics) >= 0 overrides overcastLight
        [Tooltip("A flash of lightning at full strength, lux.")]
        public float lightningLux = 25000f;
        [Tooltip("On: HDRP's sky thins and colours the Sun's light itself (the Physically Based Sky's atmospheric attenuation, " +
                 "HDRP's default). Off: the clock does it roughly, for a sky that doesn't.")]
        public bool hdrpAttenuatesSun = true;

        [NonSerialized] double hours = double.NaN;

        public static LevelClock Current { get; private set; }
        /// <summary>1, 2 or 4 (single player).</summary>
        public static float Speed = 1f;
        public static bool Paused;
        /// <summary>The game moves the hours itself, a game minute at a time (Kapman.Game.KapmanGame.RunClock, on the
        /// chart): the clock does not add the frame's time, it only turns the sky for the hour it is given.</summary>
        public static bool External;
        /// <summary>The clock's own keys work (T an hour on, Y a day, U a month, Space, Tab). Off while the chart's frame
        /// is up: T is its "tabs", and there the game's speed is the game's own (GameSpeed: 1x, 2x, 4x, nothing faster).</summary>
        public static bool DevKeys = true;
        /// <summary>How fast the game runs now: 0 paused, else the speed (the ship and the water use it).</summary>
        public static float TimeFactor => Paused ? 0f : Speed;

        /// <summary>Game hours since the start day's midnight.</summary>
        public double GameHours
        {
            get => double.IsNaN(hours) ? startHour : hours;
            set => hours = value;
        }

        public float HourOfDay => (float)(((GameHours % 24.0) + 24.0) % 24.0);
        public GameDate Date => Calendar.At(year, startDay, GameHours);
        public int DayOfYear => Date.dayOfYear;
        public int Year => Date.year;

        /// <summary>The moment as a Julian day (days since noon, 1 January 4713 BC), the clock's local mean time.</summary>
        public double JulianDay => 1721423.5 + 365.0 * (year - 1) + Math.Floor((year - 1) / 4.0) + (startDay - 1) + GameHours / 24.0;

        public string Text => $"{Date}, {(int)HourOfDay:00}:{(int)(GameHours * 60.0 % 60.0):00}";

        /// <summary>0 by day, 1 at full night (the Sun 12 degrees under).</summary>
        public static float Night { get; private set; }
        /// <summary>The warm, low sun: 1 a few degrees up, 0 high in the sky or under the horizon.</summary>
        public static float Golden { get; private set; }
        public static Vector3 SunDirection { get; private set; } = Vector3.up;
        public static Vector3 MoonDirection { get; private set; } = Vector3.up;
        public static float SunAltitude { get; private set; }
        public static Color SkyZenith { get; private set; }
        public static Color SkyHorizon { get; private set; }
        /// <summary>The Sun's colour and strength as the first build's water and sky read it (0 under the horizon, about
        /// 2 at full day): a relative value, not lux.</summary>
        public static Color SunColour { get; private set; }
        /// <summary>The Sun's light on the ground now, through the air (lux, by colour): for the snow and the mist.</summary>
        public static Color SunLightLux { get; private set; }
        /// <summary>The Moon's light on the ground now (lux, by colour).</summary>
        public static Color MoonLightLux { get; private set; }
        /// <summary>A flash of lightning, 0..1, set by the weather.</summary>
        public static float Flash;

        static readonly int ZenithId = Shader.PropertyToID("_KapmanSkyZenith");
        static readonly int HorizonId = Shader.PropertyToID("_KapmanSkyHorizon");
        static readonly int SunDirId = Shader.PropertyToID("_KapmanSunDir");
        static readonly int SunColourId = Shader.PropertyToID("_KapmanSunColour");
        static readonly int MoonDirId = Shader.PropertyToID("_KapmanMoonDir");
        static readonly int MoonColourId = Shader.PropertyToID("_KapmanMoonColour");
        static readonly int NightId = Shader.PropertyToID("_KapmanNight");
        static readonly int GoldenId = Shader.PropertyToID("_KapmanGolden");
        static readonly int FlashId = Shader.PropertyToID("_KapmanFlash");

        HDAdditionalLightData sunData, moonData;
        Light fill, lightning;
        Light sunSetUp, moonSetUp;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            Apply();
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            // our own lights are switched off, not destroyed (they go with this object; destroying a child while the
            // hierarchy is coming down logs an error), and found again by name the next time
            if (fill != null)
                fill.enabled = false;
            if (lightning != null)
                lightning.enabled = false;
            fill = lightning = null;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                var kb = Keyboard.current;
                if (kb != null && DevKeys)
                {
                    bool back = kb.shiftKey.isPressed;
                    if (kb.tKey.wasPressedThisFrame) GameHours += back ? -1.0 : 1.0;
                    if (kb.yKey.wasPressedThisFrame) GameHours += back ? -24.0 : 24.0;
                    if (kb.uKey.wasPressedThisFrame) GameHours += back ? -24.0 * 30.0 : 24.0 * 30.0;
                    if (kb.spaceKey.wasPressedThisFrame) Paused = !Paused;
                    if (kb.tabKey.wasPressedThisFrame) Speed = Speed >= 4f ? 1f : Speed * 2f;
                }
                if (!Paused && !External)
                    GameHours += Time.deltaTime * rate * Speed / 3600.0;
                if (GameHours < 0.0)
                    GameHours = 0.0;
            }
            Apply();
        }

        /// <summary>Gives the Sun and the Moon their HDRP side (once per light), and makes the fill and the lightning.</summary>
        void SetUp()
        {
            if (sun != null && (sunData == null || sunSetUp != sun))
            {
                sunSetUp = sun;
                sunData = HdData(sun);
                sun.type = LightType.Directional;
                sun.lightUnit = LightUnit.Lux;
                sun.useColorTemperature = true;
                sun.colorTemperature = 5778f;                                 // the Sun outside the air; the sky colours it
                sun.color = Color.white;
                sunData.interactsWithSky = true;                              // drawn on the sky, thinned by its air
                sunData.angularDiameter = sunDiameter;
                sunData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.Emission;
                sunData.flareMultiplier = 0.15f;                              // a little glare, not a lamp (3 Oct: 1 was a white ball)
                sunData.flareSize = 1.2f;
                sunData.flareFalloff = 6f;
                sunData.distance = 1.496e11f;
            }
            if (moon != null && (moonData == null || moonSetUp != moon))
            {
                moonSetUp = moon;
                moonData = HdData(moon);
                moon.type = LightType.Directional;
                moon.lightUnit = LightUnit.Lux;
                moon.useColorTemperature = false;
                moonData.interactsWithSky = true;
                moonData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.ReflectSunLight;
                moonData.sunLightOverride = sun;
                moonData.angularDiameter = moonDiameter;                      // (its light's softness)
                moonData.diameterMultiplerMode = false;
                moonData.diameterOverride = moonDiameter;                     // (its disc: HDRP draws this one)
                moonData.distance = 3.84e8f;                                  // nearer than the Sun: drawn over it (an eclipse)
                moonData.flareMultiplier = 0f;                                // (in this mode the glow is the Sun's: blinding)
                moonData.earthshine = 1f;
                moonData.surfaceTexture = moonSurface;
            }
            if (fill == null)
            {
                fill = Make("Night fill");
                fill.transform.rotation = Quaternion.Euler(90f, 0f, 0f);       // straight down
            }
            if (lightning == null)
            {
                lightning = Make("Lightning");
                lightning.color = new Color(0.82f, 0.86f, 1f);
            }
        }

        static HDAdditionalLightData HdData(Light light)
        {
            var data = light.GetComponent<HDAdditionalLightData>();
            if (data == null)
                data = light.gameObject.AddComponent<HDAdditionalLightData>();
            return data;
        }

        /// <summary>A directional light of our own, kept out of the scene file (and found again after a domain reload).</summary>
        Light Make(string name)
        {
            var old = transform.Find(name);
            if (old != null)
            {
                var had = old.GetComponent<Light>();
                if (had != null)
                    return had;
                CoreUtils.Destroy(old.gameObject);
            }
            var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
            go.transform.SetParent(transform, false);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.lightUnit = LightUnit.Lux;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.enabled = false;
            var data = HdData(light);
            data.interactsWithSky = false;
            return light;
        }

        /// <summary>Turns the sky for this moment and lights the world from it (also in the editor).</summary>
        public void Apply()
        {
            SetUp();
            var info = LevelInfo.Current;
            double lat = info != null ? info.latitude : 52.0, lon = info != null ? info.longitude : 5.0;
            Sky.Compute(Sky.ToUt(JulianDay, lon), lat, lon);
            Vector3 toSun = Dir(Sky.Sun), toMoon = Dir(Sky.Moon);
            SunDirection = toSun;
            MoonDirection = toMoon;
            float alt = (float)Sky.Sun.altitude;
            SunAltitude = alt;
            Night = Mathf.InverseLerp(0f, -12f, alt);
            Golden = Mathf.Clamp01(1f - Mathf.Abs(alt - 3f) / 13f);
            var weatherNow = LevelWeather.Current;
            float overcast = weatherNow != null ? weatherNow.Overcast : 0f;
            // the cloud's dimming of the Sun. Under the weather engine it is the cloud column's transmittance (weather 2.5:
            // a nimbostratus deck passes 1-2.5%, a storm's core under half a percent), and the HDRP Sun is dimmed by it less
            // what HDRP's own volumetric cloud shadows already take, so the ground gets the model's light once
            // (LightingModel.CloudDim, lighting 4.2). Set by hand (a shot, the G key) the fixed share below still holds
            float cloudDim = Mathf.Lerp(1f, DiagOvercastLight >= 0f ? DiagOvercastLight : overcastLight, overcast);
            float groundDim = cloudDim;
            if (DiagOvercastLight < 0f && weatherNow != null && weatherNow.SkyFromEngine)
            {
                groundDim = (float)LightingModel.CloudDim(weatherNow.CloudTransmittance);
                cloudDim = (float)LightingModel.CloudDim(weatherNow.CloudTransmittance, weatherNow.HdrpCloudShadow);
            }

            // the Sun's colour and strength as the first build's shaders read them (relative, not lux)
            float up = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-1.5f, 10f, alt));
            Color low = new Color(1f, 0.50f, 0.24f), high = new Color(1f, 0.95f, 0.88f);
            Color sunColour = Color.Lerp(low, high, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2f, 32f, alt)));
            float sunLight = 2.2f * up * Mathf.Lerp(1f, 0.25f, overcast);
            SunColour = sunColour * sunLight;

            // the Sun's light on the ground through the air, lux: what the snow and the mist are lit by
            Color through = Transmittance(alt);
            float groundLux = sunLux * Mathf.Clamp01(Mathf.InverseLerp(-0.8f, 0.5f, alt)) * groundDim * (1f - (float)Sky.SunCovered);
            SunLightLux = through * groundLux;

            // the Sun: its true light outside the air; HDRP's sky thins and reddens it, and takes it away under the horizon.
            // It stays on all night: the sky's twilight is worked out from where it stands under the horizon
            bool sunShadows = alt > sunShadowsTo;
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.LookRotation(-toSun, Vector3.up);
                float lux = sunLux * SkyDome.SunInSky * cloudDim * (1f - (float)Sky.SunCovered);
                if (!hdrpAttenuatesSun)
                {
                    lux *= Mathf.Clamp01(Mathf.InverseLerp(-0.8f, 0.5f, alt));
                    sun.useColorTemperature = false;
                    sun.color = through;
                }
                sun.intensity = lux;
                sun.enabled = true;
                sunData.EnableShadows(sunShadows);
                sunData.angularDiameter = sunDiameter;
                RenderSettings.sun = sun;
            }

            // the Moon: its true light by its phase and height, a little boosted; dimmed and reddened in the Earth's shadow
            bool moonUp = Sky.Moon.altitude > -1.0;
            float moonHeight = Mathf.Clamp01((float)Sky.Moon.altitude / 15f);
            float shade = LunarShade();
            float moonLux = moonUp ? (float)Sky.MoonLux * moonBoost * (1f - 0.995f * shade) * Mathf.Lerp(1f, 0.3f, overcast) : 0f;
            Color moonTint = Color.Lerp(moonColour, new Color(1f, 0.35f, 0.18f), shade);
            MoonLightLux = moonTint * (moonUp ? (float)Sky.MoonLux * (1f - 0.995f * shade) * Mathf.Lerp(1f, 0.3f, overcast) : 0f);
            bool moonShadows = !sunShadows && moonUp && moonLux > 0.01f;
            if (moon != null)
            {
                moon.transform.rotation = Quaternion.LookRotation(-toMoon, Vector3.up);
                moon.color = moonTint;
                moon.intensity = moonLux;
                moon.enabled = true;                                          // (always drawn; its light is 0 when it's under)
                moonData.surfaceTint = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.12f), shade) * 0.08f   // dimmed hard: the physical disc (thousands of nits) smeared into the sky probe and glinted off every eyelet and ridge at night; at night exposure a 400-nit disc still reads white
                                       * Mathf.Lerp(moonDayTint, moonNightTint, Night) * (1f - 0.97f * shade);
                moonData.EnableShadows(moonShadows);
                moonData.sunLightOverride = sun;
            }

            // the fill: on moonless nights, less as the Moon rises and gives light of its own; it comes in with the dusk
            float moonShare = moonUp ? Mathf.Clamp01((float)Sky.MoonLit * moonHeight) : 0f;
            if (fill != null)
            {
                fill.color = moonColour;
                fill.intensity = nightFill * SkyDome.Dusk * (1f - moonShare);
                fill.enabled = fill.intensity > 0.0005f;
            }

            // lightning: a flash from overhead, a little off the vertical
            if (lightning != null)
            {
                lightning.intensity = lightningLux * Flash;
                lightning.enabled = Flash > 0.001f;
                if (lightning.enabled)
                    lightning.transform.rotation = Quaternion.Euler(70f, 40f + 200f * Mathf.Repeat(Flash * 7.3f, 1f), 0f);
            }

            // the sky's colours, for the water's reflections, the mist and the snow (relative, as the first build's)
            float day = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-8f, 8f, alt));
            float moonLitUp = Mathf.InverseLerp(-1f, 10f, (float)Sky.Moon.altitude);
            Color zenNight = new Color(0.008f, 0.012f, 0.024f), zenDay = new Color(0.30f, 0.43f, 0.60f);
            Color horNight = new Color(0.025f, 0.032f, 0.05f), horDay = new Color(0.64f, 0.68f, 0.72f);
            Color horGold = new Color(0.98f, 0.56f, 0.30f);
            Color grey = new Color(0.55f, 0.57f, 0.60f);
            Color zenith = Color.Lerp(zenNight, Color.Lerp(zenDay, grey * 0.85f, overcast), day);
            Color horizon = Color.Lerp(Color.Lerp(horNight, Color.Lerp(horDay, grey, overcast), day), horGold,
                                       Golden * 0.7f * (1f - overcast * 0.8f));
            float moonGlow = (float)Sky.MoonLit * moonLitUp * Night * (1f - overcast * 0.7f);
            zenith += new Color(0.025f, 0.03f, 0.05f) * moonGlow;
            horizon += new Color(0.03f, 0.035f, 0.05f) * moonGlow;
            zenith += new Color(0.5f, 0.55f, 0.7f) * Flash;
            horizon += new Color(0.5f, 0.55f, 0.7f) * Flash;
            SkyZenith = zenith;
            SkyHorizon = horizon;

            Shader.SetGlobalColor(ZenithId, zenith);
            Shader.SetGlobalColor(HorizonId, horizon);
            Shader.SetGlobalVector(SunDirId, toSun);
            Shader.SetGlobalColor(SunColourId, SunColour);
            Shader.SetGlobalVector(MoonDirId, toMoon);
            Shader.SetGlobalColor(MoonColourId, moonColour * (0.16f * (float)Sky.MoonLit * moonLitUp * Night * Mathf.Lerp(1f, 0.2f, overcast)));
            Shader.SetGlobalFloat(NightId, Night);
            Shader.SetGlobalFloat(GoldenId, Golden);
            Shader.SetGlobalFloat(FlashId, Flash);
        }

        /// <summary>
        /// How much of the Sun's light gets down through the clear air at an altitude (degrees), by colour: Kasten and
        /// Young's air mass with sea-level optical depths of about 0.10, 0.16 and 0.30 for red, green and blue (the air
        /// and a little haze), so a low Sun comes out orange. A rough stand-in for what HDRP's sky does exactly.
        /// </summary>
        public static Color Transmittance(float altitude)
        {
            float a = Mathf.Max(altitude, -0.8f);
            float m = 1f / (Mathf.Sin(a * Mathf.Deg2Rad) + 0.50572f * Mathf.Pow(a + 6.07995f, -1.6364f));
            m = Mathf.Clamp(m, 1f, 40f);
            return new Color(Mathf.Exp(-0.10f * m), Mathf.Exp(-0.16f * m), Mathf.Exp(-0.30f * m));
        }

        /// <summary>How deep the Moon is in the Earth's shadow: 0 out of it .. 1 wholly in the umbra (about 0.7 degrees
        /// across the middle, opposite the Sun).</summary>
        static float LunarShade()
        {
            var s = Sky.Sun;
            var m = Sky.Moon;
            double dot = -(s.east * m.east + s.up * m.up + s.north * m.north);   // the Moon against the anti-Sun
            double sep = Math.Acos(Math.Max(-1.0, Math.Min(1.0, dot))) / SkyMath.D2R;
            const double umbra = 0.70, moonRadius = 0.26, penumbra = 1.25;
            if (sep >= penumbra + moonRadius)
                return 0f;
            float inUmbra = Mathf.Clamp01((float)((umbra + moonRadius - sep) / (2.0 * moonRadius)));
            float inPenumbra = Mathf.Clamp01((float)((penumbra + moonRadius - sep) / (penumbra + moonRadius - umbra)));
            return Mathf.Max(inUmbra, 0.25f * inPenumbra);
        }

        static Vector3 Dir(SkyBody b) => new Vector3((float)b.east, (float)b.up, (float)b.north).normalized;
    }
}
