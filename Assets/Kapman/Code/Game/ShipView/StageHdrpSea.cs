// HDRP names are from Library/PackageCache/com.unity.render-pipelines.high-definition@8aadf824e504/Runtime/Water:
//   WaterSurface/WaterSurface.Simulation.cs:240 (EvaluateWaterSurfaceMatrices: an ocean's waves are worked out in the
//   surface's own space, so they go where its transform goes), HDRenderPipeline.WaterSystem.Utilities.cs:250
//   (EvaluateSwellSecondPatchSize), WaterSystemDef.cs:46 (k_RipplesBandSize 10 m), WaterSurface.cs:656
//   (FillWaterSearchData: heights are answered only with scriptInteractions on).
using System;
using System.Reflection;
using Kapman.Weather;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE STAGE'S SEA AS HDRP'S OWN WATER: the sea of the look lab (KapmanSail's Tank scene, the look the user tuned by
    /// eye and approved on 4 Oct 2026), brought beside the chart. Taken from the lab and from the game's own HdrpSea,
    /// which the lab's TankSea was made from:
    ///   - the surface's set-up (an infinite ocean, 500 m repetition, chaos 0.6, ripples 0.8, foam on) and the water's
    ///     look (refraction 0.10 0.62 0.66, scattering 0.04 0.42 0.38, seen 9 m into, smoothness 0.95);
    ///   - the sea state fitted to HDRP's two swell bands through HDRP's own amplitude table (SeaFit): the wind for the
    ///     period, the multipliers for the height; ripples from the wind within seconds; whitecaps from the share of
    ///     breaking crests; glassy in a calm; a confused sea when it is young and steep.
    /// The sea state is the stage's own (StageSea steps the game's SeaStateModel): this only draws it.
    ///
    /// HOW IT LIVES BESIDE THE CHART (PLAN.md part 1 said it could not):
    ///   - WaterSampler finds any WaterSurface in the scene and the chart game would then ask it for depths. This one's
    ///     object is HideFlags.DontSave, and Unity's finders do not return such objects, so it is not found. HDRP draws
    ///     it all the same (a surface puts itself on HDRP's own list when it is enabled).
    ///   - The chart's camera has a culling mask of 0 and HDRP draws every water surface for such a camera. The stage
    ///     switches Water off in that camera's own frame settings (ShipStage.KeepSeaFromOtherCameras).
    ///   - No HdrpSea, LevelWeather or LevelInfo: this drives the surface itself.
    ///
    /// THE TREADMILL. She is held still and the sea goes by. HDRP works its waves out in the surface's own space, so
    /// the whole pattern goes where the surface's transform goes: the transform is moved astern at her speed, and turned
    /// about her (not about itself) when the sea's bearing changes. The bands' own orientation stays 0, so the pattern
    /// repeats exactly every 9,500 m along the surface's axes (19 patches of the first band, 140 of the second, 950 of
    /// the ripples), and the transform is brought back by that length whenever it has gone half of it: nothing jumps
    /// and nothing loses precision however long she sails.
    /// </summary>
    public sealed class StageHdrpSea
    {
        /// <summary>The length after which all three of HDRP's bands repeat, with a 500 m repetition size (m).</summary>
        const double Period = 9500.0;
        const float Repetition = 500f;

        // ---- the water's look: the look lab's numbers (TankSea), and the game's river for a river (HdrpWater.MakeRiver)
        public Color seaRefraction = new Color(0.1f, 0.62f, 0.66f), seaScattering = new Color(0.04f, 0.42f, 0.38f);
        public float seaSeenInto = 9f;
        public Color riverRefraction = new Color(0.30f, 0.28f, 0.16f), riverScattering = new Color(0.20f, 0.19f, 0.10f);
        public float riverSeenInto = 0.6f;
        public float smoothness = 0.95f, glassySmoothness = 0.995f;
        /// <summary>A calm: below this wind (m/s) the ripples die and the water goes glassy.</summary>
        public float glassyBelow = 2f;
        /// <summary>The share of the wind's ripples that a river shows, and a harbour (she lies at a jetty: quiet water).</summary>
        public float shelteredRipples = 0.5f, harbourRipples = 0.15f;
        /// <summary>The sea's foam in a gale, and the share of breaking crests that shows as that much.</summary>
        public float galeFoam = 0.9f, whitecapsFull = 0.12f;
        /// <summary>Real seconds the ripples take to follow the wind, a confused sea to come and go, the pattern to turn
        /// to a new bearing, and the water's colour to change between river and sea.</summary>
        public float rippleLag = 4f, chaosLag = 30f, turnLag = 25f, colourLag = 20f;
        /// <summary>The sea is never shown higher than this (m; 0: no cap). The model raises 7 m in a gale on the open sea.</summary>
        public float heightCap = 0f;

        // ---- WHAT IT COSTS, AND HOW THAT IS KEPT DOWN (measured in the chart on 7 Oct 2026: BUILD_LOG.md, part 3).
        // HDRP works every enabled water surface out anew each time it draws ANY camera (HDRenderPipeline.Render calls
        // WaterSystem.UpdateWaterSurfaces before it looks at the camera), and with this project's HDRP asset (the waves
        // copied on the processor at 256 x 256 for the levels' ships) one such working-out is about 10 ms. Left enabled,
        // the stage's sea cost the chart's own frame that much though the chart draws no sea, and every picture of the
        // stage the same again. So:
        /// <summary>On: the surface is enabled only for the moment the stage's own picture is drawn (ShipStageView.Drawing),
        /// so the chart's frames pay nothing for it. (Its heights are still answered between: they come from the buffers
        /// of the last working-out.)</summary>
        public bool onlyWhileDrawn = true;
        /// <summary>She rides HDRP's own waves (the surface's scriptInteractions: the costly copy on the processor) only
        /// when the sea is at least this high, metres of significant height; on quieter water she floats on the stage's
        /// own eight waves (StageSea), which are the same sea in the small. 0: always HDRP's.</summary>
        public float rideAbove = 0.3f;
        /// <summary>She is riding HDRP's own waves now.</summary>
        public bool Rides => Surface != null && Surface.scriptInteractions;

        /// <summary>The stage's picture is about to be drawn (on), or has been (off).</summary>
        public void Drawing(bool on)
        {
            if (Surface != null && onlyWhileDrawn && Surface.enabled != on)
                Surface.enabled = on;
        }

        public WaterSurface Surface { get; private set; }
        public GameObject Object { get; private set; }
        /// <summary>How the sea state was fitted to HDRP's bands this frame.</summary>
        public SeaFit.Result Fit { get; private set; }
        /// <summary>HDRP answered the last height asked (its copy of the waves on the CPU is running).</summary>
        public bool Live { get; private set; }
        /// <summary>0 in a breeze, 1 in a dead calm.</summary>
        public float Glassy { get; private set; }

        double ux, uz;                                                    // where she is in the pattern's own axes (m)
        float theta, river, chaos, foam;
        Vector2 rippleWind;
        bool started;
        static float[] table;
        static bool tableLooked;
        static AnimationCurve flatFoam;

        /// <summary>Whether this HDRP asset draws water at all.</summary>
        public static bool Supported
        {
            get
            {
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset asset
                    && asset.currentPlatformRenderPipelineSettings.supportWater;
            }
        }

        public void Build(Transform parent, int layer)
        {
            // (saved in the scene the surface is taken as it is, its look the Inspector's; else made with the code's.
            // DontSave at play: Object.FindAnyObjectByType does not return such objects, and WaterSampler looks for its
            // surface with exactly that. Shown in the Hierarchy all the same.)
            Object = ShipStage.Child(parent, "Sea (HDRP water)", out bool made);
            Object.layer = layer;
            if (Application.isPlaying)
                Object.hideFlags = HideFlags.DontSave;
            if (!Object.TryGetComponent(out WaterSurface s))
            {
                s = Object.AddComponent<WaterSurface>();
                made = true;
            }
            Surface = s;
            s.decalRegionAnchor = parent;
            s.scriptInteractions = rideAbove <= 0f;                       // (Tick switches it on when there is a sea to ride)
            if (onlyWhileDrawn && Application.isPlaying)
                s.enabled = false;
            if (!made)
                return;
            s.surfaceType = WaterSurfaceType.OceanSeaLake;
            s.geometryType = WaterGeometryType.Infinite;
            s.scriptInteractions = true;                                  // (she floats on its own waves)
            s.cpuEvaluateRipples = false;
            s.timeMultiplier = 1f;                                        // (the waves pass at their real speed whatever the clock's)
            s.repetitionSize = Repetition;
            s.largeOrientationValue = 0f;                                 // (the transform turns the pattern: see the class)
            s.largeChaos = 0.6f;
            s.largeBand0Multiplier = 0f;
            s.largeBand1Multiplier = 0f;
            s.ripples = true;
            s.ripplesChaos = 0.8f;
            s.ripplesMotionMode = WaterPropertyOverrideMode.Custom;
            s.caustics = false;
            s.foam = true;
            s.foamResolution = WaterSurface.WaterDecalRegionResolution.Resolution512;
            s.foamPersistenceMultiplier = 1f;
            s.simulationFoamAmount = 0f;
            s.simulationFoamWindCurve = FlatFoam();                       // (the foam's amount is ours, from the sea state)
            s.deformation = false;
            // (her wake's foam is put into the water by decals: the patch of water they may mark is kept round her, not round
            // whichever camera HDRP draws first, which is the chart's, far away)
            s.decalRegionSize = new Vector2(64f, 64f);
            s.underWater = false;
            s.refractionColor = seaRefraction;
            s.scatteringColor = seaScattering;
            s.absorptionDistance = seaSeenInto;
            s.startSmoothness = smoothness;
            s.endSmoothness = Mathf.Max(0.5f, smoothness - 0.08f);
        }

        public void Destroy()
        {
            if (Object != null)
                UnityEngine.Object.Destroy(Object);
            Object = null;
            Surface = null;
        }

        /// <summary>Moves the pattern past her and sets the surface to the sea of this moment. at: where she is held (the
        /// stage's middle, at the water's level).</summary>
        static FieldInfo simulationField, simulationTimeField;
        static bool timeLooked;

        /// <summary>
        /// THE WATER'S OWN CLOCK. HDRP moves its waves on by Time.time (WaterSimulationResources.Update), and in the chart
        /// game Time.time runs at the clock's speed (Unity's time scale is the game's speed: GameSpeed). Left alone the
        /// sea would race at the higher speeds and freeze under every card that holds the clock. So the surface's own
        /// multiplier undoes the time scale; and while the time scale is 0 the simulation's time is moved on by hand
        /// (the field is internal to HDRP: read once by reflection, as the amplitude table is; if it is not found the
        /// water simply stands still while the clock is held).
        /// </summary>
        void KeepTime(float dt)
        {
            float scale = Time.timeScale;
            if (scale > 1e-4f)
            {
                Surface.timeMultiplier = 1f / scale;
                return;
            }
            Surface.timeMultiplier = 1f;
            try
            {
                if (!timeLooked)
                {
                    timeLooked = true;
                    simulationField = typeof(WaterSurface).GetField("simulation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    simulationTimeField = simulationField?.FieldType.GetField("simulationTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                object simulation = simulationField?.GetValue(Surface);
                if (simulation != null && simulationTimeField != null)
                    simulationTimeField.SetValue(simulation, (float)simulationTimeField.GetValue(simulation) + dt);
            }
            catch (Exception)
            {
                simulationTimeField = null;
            }
        }

        public void Tick(in ShipStageState s, in SeaState sea, Vector3 at, float dt, bool snap)
        {
            if (Surface == null)
                return;
            KeepTime(dt);
            bool first = snap || !started;
            float K(float lag) => first ? 1f : 1f - Mathf.Exp(-dt / Mathf.Max(0.1f, lag));

            // the pattern: turned to the way the sea runs, and gone by under her at her speed
            float to = (sea.set ? sea.from : s.windFrom) + 180f;
            theta = first ? to : Mathf.LerpAngle(theta, to, K(turnLag));
            var turn = Quaternion.Euler(0f, theta - 90f, 0f);             // (HDRP's waves run along the surface's own +x)
            Vector3 ax = turn * Vector3.right, az = turn * Vector3.forward;
            float h = s.heading * Mathf.Deg2Rad;
            Vector3 way = new Vector3(Mathf.Sin(h), 0f, Mathf.Cos(h)) * s.Way;
            ux = Wrap(ux + Vector3.Dot(way, ax) * dt);
            uz = Wrap(uz + Vector3.Dot(way, az) * dt);
            Object.transform.SetPositionAndRotation(at - ax * (float)ux - az * (float)uz, turn);

            // the sea state, fitted to HDRP's bands
            float hs = sea.hs, windSeaHs = sea.windSeaHs;
            if (heightCap > 0f && hs > heightCap)
            {
                windSeaHs *= heightCap / hs;
                hs = heightCap;
            }
            // she rides HDRP's own waves only when there is a sea to ride (the cost: see rideAbove); the switch is rare
            // (a third of the mark's height between on and off), and she is some centimetres off for the moment of it
            bool ride = rideAbove <= 0f || hs > (Surface.scriptInteractions ? rideAbove * 0.7f : rideAbove);
            if (ride != Surface.scriptInteractions)
                Surface.scriptInteractions = ride;
            var t = Table();
            if (t != null)
            {
                Fit = SeaFit.Fit(t, Surface.repetitionSize, hs, sea.tp, windSeaHs, sea.windSeaTp);
                Surface.largeWindSpeed = Fit.windKmh;
                Surface.largeBand0Multiplier = Fit.band0;
                Surface.largeBand1Multiplier = Fit.band1;
            }
            else
            {
                Surface.largeWindSpeed = sea.tp / (float)SeaFit.PeriodPerWind * 3.6f;
                Surface.largeBand0Multiplier = Surface.largeBand1Multiplier = Mathf.Clamp01(hs);
            }
            float confused = sea.swellHs > sea.windSeaHs ? Mathf.Lerp(0.3f, 0.9f, sea.confused)
                           : Mathf.Lerp(0.6f, 0.9f, Mathf.Max(sea.confused, Mathf.Clamp01(sea.steepness / 0.07f)));
            chaos = Mathf.Lerp(chaos, confused, K(chaosLag));
            Surface.largeChaos = chaos;

            // the ripples and the whitecaps follow the wind within seconds, its bearing too (given in the pattern's own axes)
            float w = (s.windFrom + 180f) * Mathf.Deg2Rad;
            Vector2 now = new Vector2(Mathf.Sin(w), Mathf.Cos(w)) * Mathf.Max(s.windSpeed, 0.01f);
            rippleWind = Vector2.Lerp(rippleWind, now, K(rippleLag));
            float ripple = rippleWind.magnitude;
            float rain = Mathf.Max(s.rain, s.snow * 0.2f);
            // (sheltered water is ruffled less by the same wind: a river or a harbour has no room for the ripples to grow)
            Surface.ripplesWindSpeed = Mathf.Clamp(ripple * 3.6f * Mathf.Lerp(1f, s.place == StagePlace.Port ? harbourRipples : shelteredRipples, river) + rain * 3f, 0f, 15f);
            var blow = new Vector3(rippleWind.x, 0f, rippleWind.y);
            if (ripple > 0.05f)
                Surface.ripplesOrientationValue = Mathf.Repeat(Mathf.Atan2(Vector3.Dot(blow, az), Vector3.Dot(blow, ax)) * Mathf.Rad2Deg, 360f);
            Surface.ripplesChaos = Mathf.Clamp01(0.8f + 0.6f * rain);

            // river or sea: the water's colour and how far one sees into it; no whitecaps on a river
            bool inland = s.place == StagePlace.River || s.place == StagePlace.Port;
            river = Mathf.Lerp(river, inland ? 1f : 0f, K(colourLag));
            Surface.refractionColor = Color.Lerp(seaRefraction, riverRefraction, river);
            Surface.scatteringColor = Color.Lerp(seaScattering, riverScattering, river);
            Surface.absorptionDistance = Mathf.Lerp(seaSeenInto, riverSeenInto, river);
            float white = galeFoam * Mathf.SmoothStep(0f, 1f, Mathf.Sqrt(Mathf.Clamp01(sea.whitecaps / Mathf.Max(whitecapsFull, 1e-3f)))) * (1f - river);
            foam = Mathf.Lerp(foam, white, K(rippleLag));
            Surface.simulationFoamAmount = foam;

            // a calm: the ripples are gone and the water mirrors the sky; rain pits it
            Glassy = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(glassyBelow * 0.25f, glassyBelow, ripple));
            Surface.startSmoothness = Mathf.Lerp(smoothness, glassySmoothness, Glassy) - 0.05f * rain;
            Surface.endSmoothness = Mathf.Lerp(Mathf.Max(0.5f, smoothness - 0.08f), glassySmoothness, Glassy) - 0.05f * rain;
            started = true;
        }

        static double Wrap(double v) => v > Period * 0.5 ? v - Period : v < -Period * 0.5 ? v + Period : v;

        /// <summary>The water's height (the world's y) under a point of the world, from HDRP's own copy of the waves.
        /// False when HDRP has no answer (its first frames, or water switched off): the caller falls back.</summary>
        public bool TryHeight(Vector3 world, out float height)
        {
            height = 0f;
            // (the surface need not be enabled now: its heights come from the buffers of its last working-out)
            if (Surface == null || Object == null || !Object.activeInHierarchy || !Surface.scriptInteractions)
                return Live = false;
            var search = new WaterSearchParameters
            {
                targetPositionWS = world,
                startPositionWS = world,
                error = 0.003f,                                           // (tight: a loose search jitters frame to frame; WaterSampler's number)
                maxIterations = 8,
                includeDeformation = false,
                excludeSimulation = false,
                outputNormal = false,
            };
            if (!Surface.ProjectPointOnWaterSurface(search, out var found))
                return Live = false;
            Vector3 hit = found.projectedPositionWS;
            height = hit.y;
            return Live = true;
        }

        /// <summary>HDRP's table of its bands' greatest displacement (WaterConsts.k_MaximumAmplitudeTable, internal to HDRP,
        /// read once by reflection as the game's HdrpSea and the look lab's TankSea read it): the link SeaFit fits through.</summary>
        static float[] Table()
        {
            if (tableLooked)
                return table;
            tableLooked = true;
            try
            {
                var type = typeof(WaterSurface).Assembly.GetType("UnityEngine.Rendering.HighDefinition.WaterConsts");
                var field = type?.GetField("k_MaximumAmplitudeTable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var found = field?.GetValue(null) as float[];
                table = found != null && found.Length >= SeaFit.TableSize * SeaFit.TableSize ? found : null;
            }
            catch (Exception)
            {
                table = null;
            }
            if (table == null)
                Debug.LogWarning("Kapman ship view: HDRP's amplitude table was not found; the sea's height follows its period only roughly.");
            return table;
        }

        static AnimationCurve FlatFoam() => flatFoam ?? (flatFoam = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f)));
    }
}
