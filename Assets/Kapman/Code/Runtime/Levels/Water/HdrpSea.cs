using System;
using System.Collections.Generic;
using System.Reflection;
using Kapman.Weather;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// THE SEA (OR THE RIVER) UNDER THE WEATHER - keeps an HDRP water surface in step with the level: its clock, its wind
    /// (LevelWeather), and its ice. Nothing ever switches: every change eases in.
    /// On the sea (weather document 2.4-2.5, night 2) the surface is driven by the sea state at full real height (the
    /// user's decision 4): a <see cref="SeaStateModel"/> grows the wind sea from the wind over the level's own fetch (its
    /// water record's fetch by sector and depth), brings in the swell of storms far off, and is warm-started from the
    /// weather engine whenever the level is entered, the clock jumps or the weather is taken in or out of hand, so a level
    /// opens on the sea it should have. The sea is fitted to HDRP's two swell bands with HDRP's own amplitude table
    /// (SeaFit): the wind for the period, the band multipliers for the height; the waves run at real speed
    /// (timeMultiplier 1) while the sea state steps with the clock. The ripples still follow the wind within seconds, the
    /// whitecaps come from the share of breaking crests the model gives, and in a dead calm the water goes glassy.
    /// On a River surface (and on the sea when the amplitude table cannot be read, or with useSeaState off) the first
    /// build's path runs as before: the swell follows the wind with its lags, the large band of a river is its own small
    /// agitation. Brought from the first build's HdrpWaterSource.
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    [DefaultExecutionOrder(-120)]
    public class HdrpSea : MonoBehaviour
    {
        [Tooltip("Keep the swell and ripples in step with the level's wind.")]
        public bool followWind = true;
        [Tooltip("The swell's wind is this share of the level's wind (the rest of the sea state comes from far away). Unused on a river, and on the sea when the sea state drives it.")]
        public float swellShare = 1f;
        [Tooltip("A river's own agitation (HDRP km/h of wind on its large band), before the wind adds a little to it.")]
        public float riverAgitation = 6f;
        [Tooltip("Real seconds the swell takes to build up to a stronger wind (90 = an hour and a half of game time). The old path only: the sea state carries its own growth.")]
        [Range(1f, 600f)] public float swellLag = 90f;
        [Tooltip("Real seconds an old swell takes to die down after the wind drops (360 = six game hours to fall to a third). The old path only.")]
        [Range(1f, 3600f)] public float swellDecay = 360f;
        [Tooltip("Whitecaps: none below this wind (m/s; force 3 begins at 3.4), the first scattered ones at force 4, many in a gale. The old path only.")]
        public float whitecapsFrom = 3.4f;
        [Tooltip("The sea's foam in a gale (force 8).")]
        public float galeFoam = 0.9f;
        [Tooltip("A calm: below this wind (m/s) the ripples die and the water goes glassy.")]
        public float glassyBelow = 2f;
        [Tooltip("How smooth the water is in a glassy calm, near and far (HDRP's smoothness; the surface's own values in a breeze).")]
        public float glassySmoothness = 0.995f;
        [Tooltip("Real seconds the ripples and whitecaps take to follow the wind (its speed and its direction).")]
        public float rippleLag = 4f;
        [Tooltip("Real seconds a confused sea (the wind shifting across the swell) takes to come and go.")]
        public float chaosLag = 30f;
        [Tooltip("The ripples' wind at most (km/h): HDRP's ripples look wrong above this.")]
        public float ripplesMost = 15f;

        [Header("The sea state (weather document 2.4-2.5)")]
        [Tooltip("On the sea: drive the surface from the sea state at full real height. Off: the old wind-to-swell path.")]
        public bool useSeaState = true;
        [Tooltip("The fetch every way (km) and the depth (m) when the level has no water record with a fetch.")]
        public float defaultFetchKm = 300f, defaultDepth = 30f;
        [Tooltip("The share of breaking crests (Monahan's whitecap fraction) that shows as full foam (10% is a gale's white sea).")]
        public float whitecapsFull = 0.12f;

        WaterSurface surface;
        Vector2 swellWind;                                                   // the wind the swell has come to (m/s, blowing towards x east, y north)
        Vector2 rippleWind;                                                  // the wind the ripples have come to
        float chaos;
        bool started, started2;
        float breezeSmoothNear, breezeSmoothFar;                             // the surface's smoothness in a breeze
        float breezeRipplesChaos;
        bool river;

        // the sea state
        SeaStateModel model;
        bool modelFromEngine, seaMode, settle = true;
        double lastHours = double.NaN;
        AnimationCurve beaufortFoam;
        static AnimationCurve flatFoam;
        static float[] amplitudeTable;
        static bool tableLooked;
        static readonly Dictionary<string, FetchTable> fetches = new Dictionary<string, FetchTable>();

        /// <summary>The HDRP surface this drives.</summary>
        public WaterSurface Surface => surface != null ? surface : (surface = GetComponent<WaterSurface>());

        /// <summary>The wind (m/s) the ripples and whitecaps have come to.</summary>
        public float RippleWind => rippleWind.magnitude;

        /// <summary>The same as a vector (m/s): where the wind blows TO, x east, y north.</summary>
        public Vector2 RippleWindTo => rippleWind;

        /// <summary>How glassy the water is (0 in a breeze, 1 in a dead calm): reflections and other calm looks follow it.</summary>
        public float Glassy { get; private set; }

        /// <summary>The level's HDRP water (the one ships ride and the calm looks follow).</summary>
        public static HdrpSea Current { get; private set; }

        /// <summary>The sea state now (set once the sea has stepped; not on a river).</summary>
        public SeaState Sea { get; private set; }

        /// <summary>The sea state's model (null on a river or before the first step).</summary>
        public SeaStateModel Model => model;

        /// <summary>How the sea state was fitted to HDRP's bands this frame (the wind, the multipliers, the period's error).</summary>
        public SeaFit.Result Fit { get; private set; }

        /// <summary>Whether the surface is driven by the sea state now (else the old wind path).</summary>
        public bool FromSeaState => seaMode;

        void Awake()
        {
            surface = GetComponent<WaterSurface>();
            river = surface.surfaceType == WaterSurfaceType.River;
            breezeSmoothNear = surface.startSmoothness;
            breezeSmoothFar = surface.endSmoothness;
            breezeRipplesChaos = surface.ripplesChaos;
            // HDRP's own curve gives no whitecaps below 50 km/h (force 7): the sea's foam by Beaufort instead - none in
            // light airs, a few white horses at force 4, many at force 6, the sea white-streaked from force 8. (Its x is
            // the swell's wind over HDRP's 250 km/h; the swell is raised by swellShare of the wind, so the keys are too.)
            float B(float metresPerSecond) => metresPerSecond * Mathf.Max(0.05f, swellShare) * 3.6f / 250f;
            beaufortFoam = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(B(3.4f), 0f), new Keyframe(B(5.5f), 0.25f), new Keyframe(B(8f), 0.5f),
                new Keyframe(B(10.8f), 0.72f), new Keyframe(B(13.9f), 0.88f), new Keyframe(B(17.2f), 1f), new Keyframe(1f, 1f));
            surface.simulationFoamWindCurve = beaufortFoam;
        }

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            WaterSampler.Forget();
            settle = true;
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            WaterSampler.Forget();
        }

        /// <summary>Lets the swell and ripples catch up with the wind at once (after jumping the clock, or for a shot): on
        /// the sea the sea state is rebuilt from the weather of the last two days.</summary>
        public void SettleSea()
        {
            started = false;
            started2 = false;
            settle = true;
        }

        void Update()
        {
            if (surface == null)
                return;
            var weather = LevelWeather.Current;
            bool fromSea = !river && followWind && useSeaState && weather != null && StepSea(weather);
            if (fromSea != seaMode)
            {
                seaMode = fromSea;
                // the sea state sets the foam by itself; the old path keys it to the swell's wind by Beaufort
                surface.simulationFoamWindCurve = seaMode ? FlatFoam() : beaufortFoam;
            }
            // the waves keep moving while the game is paused. Driven by the sea state they pass at their real speed (the
            // state itself steps with the clock); on the old path they run faster at 2x and 4x with everything else
            surface.timeMultiplier = seaMode || LevelClock.Paused ? 1f : Mathf.Max(0.01f, LevelClock.Speed);

            if (!followWind || weather == null)
                return;
            float ice = weather.Ice;
            Vector2 now = weather.WindVector;
            if (now.sqrMagnitude < 1e-4f)
                now = new Vector2(0.01f, 0f);
            now = now.normalized * weather.Wind;

            // the swell builds with the wind but runs on for hours after it drops (a calm keeps the old swell, slowly dying)
            float lag = now.sqrMagnitude < swellWind.sqrMagnitude ? swellDecay : swellLag;
            swellWind = started ? Vector2.Lerp(swellWind, now, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(1f, lag))) : now;
            started = true;
            // HDRP measures orientation in degrees from +x (east), turning towards +z (north): the way the waves travel
            float swellTo = Orientation(swellWind);
            if (river)
            {
                // a river's large band is its own gentle agitation, ruffled a little more by the wind, running with the
                // stream (RiverCurrent turns it along the channel; without one it keeps the wind's way)
                surface.largeWindSpeed = riverAgitation * (1f + 0.08f * swellWind.magnitude) * (1f - ice);
                if (RiverCurrent.Current == null)
                    surface.largeOrientationValue = swellTo;
            }
            else if (!seaMode)
            {
                surface.largeWindSpeed = swellWind.magnitude * 3.6f * swellShare * (1f - ice);     // km/h
                surface.largeOrientationValue = swellTo;
            }
            // the ripples and whitecaps follow the wind over a few seconds, its direction as well as its speed (a
            // squall darkens the water as it comes, and a shift of wind turns the ripples round; neither switches)
            float follow = started2 ? 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.1f, rippleLag)) : 1f;
            rippleWind = Vector2.Lerp(rippleWind, now, follow);
            float ripple = rippleWind.magnitude;
            float rain = Mathf.Max(weather.Rain, weather.Snowfall * 0.2f);
            surface.ripplesWindSpeed = Mathf.Clamp(ripple * 3.6f + rain * 3f, 0f, ripplesMost) * (1f - ice);
            if (ripple > 0.05f)
                surface.ripplesOrientationValue = Orientation(rippleWind);
            surface.ripplesChaos = Mathf.Clamp01(breezeRipplesChaos + 0.6f * rain);
            // whitecaps: on the sea state, the share of breaking crests the model gives (Monahan: 1% at 10 m/s, 10% in a
            // gale), eased with the ripples; on the old path none in light airs, a few at force 3-4, many from force 6 up
            float foam;
            if (river)
                foam = 0f;
            else if (seaMode)
                foam = galeFoam * Mathf.SmoothStep(0f, 1f, Mathf.Sqrt(Mathf.Clamp01(Sea.whitecaps / Mathf.Max(whitecapsFull, 1e-3f))));
            else
                foam = galeFoam * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(whitecapsFrom, whitecapsFrom + 2.5f, ripple));
            surface.simulationFoamAmount = started2 && seaMode ? Mathf.Lerp(surface.simulationFoamAmount, foam * (1f - ice), follow) : foam * (1f - ice);
            // a calm: the ripples are gone, the water goes glassy and mirrors the sky (the old swell still heaves under
            // it); ice is glassier still; rain pits the surface
            Glassy = Mathf.Max(1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(glassyBelow * 0.25f, glassyBelow, ripple)), ice);
            surface.startSmoothness = Mathf.Lerp(breezeSmoothNear, glassySmoothness, Glassy) - 0.05f * rain * (1f - ice);
            surface.endSmoothness = Mathf.Lerp(breezeSmoothFar, glassySmoothness, Glassy) - 0.05f * rain * (1f - ice);
            // a confused sea: on the sea state a swell running across the wind sea, or a young steep sea; on the old path
            // the wind shifting across the swell. It comes and goes over half a minute
            float confused = seaMode ? SeaChaos(Sea)
                           : Mathf.Lerp(0.6f, 0.95f, Mathf.Clamp01(Vector2.Angle(swellWind, now) / 60f + weather.Storm * 0.3f));
            chaos = started2 ? Mathf.Lerp(chaos, confused, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.1f, chaosLag))) : confused;
            if (!river)
                surface.largeChaos = chaos;
            if (seaMode)
            {
                // the sea state's own height, fitted to HDRP's bands (ice stills them)
                var fit = Fit;
                surface.largeWindSpeed = fit.windKmh;
                surface.largeOrientationValue = Orientation(Travel(Sea.from));
                surface.largeBand0Multiplier = fit.band0 * (1f - ice);
                surface.largeBand1Multiplier = fit.band1 * (1f - ice);
            }
            else
            {
                // ice stills the bands themselves (the wind speed alone leaves a residual heave)
                surface.largeBand0Multiplier = 1f - ice;
                surface.largeBand1Multiplier = 1f - ice;
            }
            started2 = true;
        }

        // ------------------------------------------------------------------------------------------- the sea state

        /// <summary>Steps the sea state to the weather's moment (warm-starting it on entry, after a clock jump, or when the
        /// weather is taken into or out of hand) and fits it to HDRP; false when it cannot drive the surface (no clock or
        /// level yet, or no amplitude table), and the old path runs.</summary>
        bool StepSea(LevelWeather weather)
        {
            var clock = LevelClock.Current;
            var info = LevelInfo.Current;
            if (clock == null || info == null || double.IsNaN(weather.Hours))
                return false;
            var table = AmplitudeTable();
            if (table == null)
                return false;

            bool driven = weather.SkyFromEngine;
            if (model == null || modelFromEngine != driven)
            {
                var engine = driven ? WeatherEngine.Shared : null;
                string place = info.kind == LevelKind.Ocean ? "north_sea" : engine != null ? engine.ClassFor(info.latitude, info.longitude, true) : "north_sea";
                model = new SeaStateModel(engine, info.latitude, info.longitude, place, FetchHere()) { Swell = driven };
                modelFromEngine = driven;
                settle = true;
            }
            double now = weather.Hours;
            Vector2 to = weather.WindVector;
            float windFrom = Mathf.Repeat(Mathf.Atan2(-to.x, -to.y) * Mathf.Rad2Deg, 360f);
            bool jumped = double.IsNaN(lastHours) || now < lastHours - 1e-6 || now - lastHours > 0.75;
            if (settle || jumped)
            {
                // a level entered, a clock jump, a load: the sea of the last two days, at once
                if (driven)
                    model.WarmStart(now);
                else
                    model.WarmStart(now, weather.Wind, windFrom, 1013.0);
                settle = false;
            }
            else if (now > lastHours)
            {
                if (driven)
                    model.Step(now, now - lastHours, weather.EngineSample);
                else
                    model.Step(now, now - lastHours, weather.Wind, windFrom, 1013.0);
            }
            lastHours = now;
            Sea = model.State;
            Fit = SeaFit.Fit(table, surface.repetitionSize, Sea.hs, Sea.tp, Sea.windSeaHs, Sea.windSeaTp);
            return true;
        }

        /// <summary>The level's fetch: its water record's (WaterProfileDriver on this surface names it), else the defaults.</summary>
        FetchTable FetchHere()
        {
            var driver = GetComponent<WaterProfileDriver>();
            if (driver != null && driver.data != null && !string.IsNullOrEmpty(driver.recordId))
            {
                if (!fetches.TryGetValue(driver.recordId, out var f))
                {
                    try
                    {
                        f = FetchTable.FromWaterRecord(driver.data.text, driver.recordId);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Kapman sea: the water record '{driver.recordId}' could not be read for its fetch ({e.Message}).");
                        f = null;
                    }
                    fetches[driver.recordId] = f;
                }
                if (f != null)
                    return f;
            }
            return FetchTable.Open(Mathf.Max(0.05f, defaultFetchKm), defaultDepth);
        }

        /// <summary>
        /// HDRP's table of its bands' greatest displacement by patch size and wind (WaterConsts.k_MaximumAmplitudeTable,
        /// internal to HDRP, read once by reflection): the link SeaFit fits the sea state through. Null if this HDRP has
        /// no such table, and the old path runs.
        /// </summary>
        static float[] AmplitudeTable()
        {
            if (tableLooked)
                return amplitudeTable;
            tableLooked = true;
            try
            {
                var type = typeof(WaterSurface).Assembly.GetType("UnityEngine.Rendering.HighDefinition.WaterConsts");
                var field = type?.GetField("k_MaximumAmplitudeTable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var table = field?.GetValue(null) as float[];
                amplitudeTable = table != null && table.Length >= SeaFit.TableSize * SeaFit.TableSize ? table : null;
            }
            catch (Exception)
            {
                amplitudeTable = null;
            }
            if (amplitudeTable == null)
                Debug.LogWarning("Kapman sea: HDRP's amplitude table was not found; the sea follows the wind the old way, not the sea state.");
            return amplitudeTable;
        }

        static AnimationCurve FlatFoam() => flatFoam ?? (flatFoam = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f)));

        /// <summary>How confused the sea looks (HDRP's large chaos): a clean swell low, a swell across the wind sea or a young
        /// steep sea high.</summary>
        static float SeaChaos(SeaState s)
        {
            if (s.swellHs > s.windSeaHs)
                return Mathf.Lerp(0.3f, 0.9f, s.confused);
            return Mathf.Lerp(0.6f, 0.9f, Mathf.Max(s.confused, Mathf.Clamp01(s.steepness / 0.07f)));
        }

        /// <summary>The way waves coming from a compass bearing travel, as a vector (x east, y north).</summary>
        static Vector2 Travel(float fromDegrees)
        {
            float a = fromDegrees * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Sin(a), -Mathf.Cos(a));
        }

        /// <summary>HDRP's orientation (degrees from +x east towards +z north) of a wind blowing towards (x east, y north).</summary>
        static float Orientation(Vector2 to) => Mathf.Repeat(Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg, 360f);
    }
}
