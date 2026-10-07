// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/ShipStage.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run.
// HDRP names are from Library/PackageCache/com.unity.render-pipelines.high-definition@8aadf824e504/Runtime:
//   Sky/VisualEnvironment.cs (skyType, cloudType, skyAmbientMode, renderingSpace), Sky/PhysicallyBasedSky,
//   Sky/CloudSystem/CloudLayer/CloudLayer.cs (opacity, layerA.opacityR/G/B/A, layerA.tint),
//   Lighting/AtmosphericScattering/Fog.cs (enabled, meanFreePath, baseHeight, maximumHeight, enableVolumetricFog),
//   PostProcessing/Components/Exposure.cs (mode, fixedExposure), Material/MaterialExtension.cs:369 (HDMaterial.ValidateMaterial).
// The Sun's and the Moon's set-up is LevelClock.SetUp's (Assets/Kapman/Code/Runtime/Levels/LevelClock.cs), line for line.
using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE STAGE OF THE 3D SHIP VIEW (the user, 7 Oct 2026: "start working on the 3D view for the ship, it'll just be
    /// water and when it docks just a simple dock model ... simulated ocean weather wind everything"). A small world of
    /// its own, built at run time from a kit (ShipStageKit), on its own layer and far from the chart's sailing world:
    /// the sea (StageSea, StageWater), the boat held at its middle (StageShip), the jetty she lies at (StageDock), a Sun
    /// and a Moon placed for her own latitude (StageSky), a sky, cloud, fog and exposure of its own on a volume only its
    /// camera takes, and that camera (ShipStageView). It reads one record a frame (ShipStageState, from Source) and
    /// knows nothing else of the game. It holds none of the 3D levels' singletons (no LevelInfo, LevelWeather, HdrpSea,
    /// SkyDome, WaterSurface): the chart game reads those statics and would take the chart for a level.
    /// Its axes are the compass: x east, z north. She keeps her true heading and the water goes by under her.
    /// </summary>
    [DefaultExecutionOrder(960)]                                        // after the chart's ship (20) and the frame (900), before the map draws (1000)
    public sealed class ShipStage : MonoBehaviour
    {
        public static ShipStage Current { get; private set; }

        public ShipStageKit kit;
        /// <summary>Who fills the record (the chart game, a test).</summary>
        public IShipStageSource Source;
        /// <summary>The record as last read.</summary>
        [NonSerialized] public ShipStageState State = ShipStageState.Fair();
        /// <summary>The picture fills the screen (the camera stands a little further off).</summary>
        public bool Big;
        /// <summary>Off: the stage stands still and draws nothing (the frame is hidden).</summary>
        public bool Running = true;

        public StageSea Sea { get; private set; }
        public StageWater Water { get; private set; }
        public StageShip Ship { get; private set; }
        public StageDock Dock { get; private set; }
        public ShipStageView View { get; private set; }
        public Volume Volume { get; private set; }
        public Light Sun { get; private set; }
        public Light Moon { get; private set; }
        public StageSkyNow SkyNow { get; private set; }
        /// <summary>The jetty is on the stage, and where its water-side edge is (stage metres east and north of her).</summary>
        public bool DockUp { get; private set; }
        public Vector3 DockAt => dockAt;
        /// <summary>Another sea than StageSea for her to float on (a look that draws waves of its own, the other builder's
        /// PsxSeaGrid, sets this while its sea is the one drawn): the height over the still water at a point so many metres
        /// east and north of her. Null: StageSea's.</summary>
        public Func<float, float, float> HeightAt;
        /// <summary>Something was put on the stage or taken onto it (the jetty came, she was fitted out, a man was seated):
        /// a look that swaps materials finds them again. Once a frame at most.</summary>
        public event Action Changed;
        bool changed;

        /// <summary>The water's height over its level where she and the camera are concerned.</summary>
        public float Height(float east, float north) => HeightAt != null ? HeightAt(east, north) : Sea.Height(east, north);

        HDAdditionalLightData sunData, moonData;
        VisualEnvironment environment;
        PhysicallyBasedSky sky;
        CloudLayer clouds;
        Fog fog;
        Exposure exposure;
        ParticleSystem rain;
        LineRenderer bowLine, sternLine;
        Mesh wakeMesh;
        Vector3[] wakeVerts;
        Color32[] wakeColours;
        Vector3 dockAt;
        float dockYaw, camAzimuth = -148f, camHeading, ev = 12f;
        string dockFor;
        double[] precession;
        double precessionDay;
        bool snap = true;
        IShipStageLook look;
        IShipCrewRig crew;
        readonly Dictionary<Color, Material> made = new Dictionary<Color, Material>();

        // ------------------------------------------------------------------ making it

        /// <summary>Makes the stage. One at a time: a second call gives the first back.</summary>
        public static ShipStage Build(ShipStageKit kit, IShipStageSource source)
        {
            if (Current != null)
                return Current;
            var go = new GameObject("--- SHIP STAGE (the 3D view) ---");
            var stage = go.AddComponent<ShipStage>();
            stage.kit = kit;
            stage.Source = source;
            stage.Make();
            return stage;
        }

        void Make()
        {
            Current = this;
            gameObject.layer = kit.layer;
            transform.position = kit.origin;
            Sea = new StageSea();
            Water = new StageWater();
            Water.Build(transform, Lit(kit.water, new Color(0.05f, 0.14f, 0.18f), 0.86f), kit.layer);
            Ship = new StageShip();
            Ship.Build(this, kit, transform);
            Dock = new StageDock();
            View = new ShipStageView();
            View.Build(transform, kit);
            Lights();
            Atmosphere();
            Rain();
            Wake();
            bowLine = Line("Bow line");
            sternLine = Line("Stern line");
            Adopt(gameObject);
        }

        void OnDestroy()
        {
            look?.Remove(this);
            crew?.Detach();
            Water?.Destroy();
            Dock?.Destroy();
            View?.Release();
            if (wakeMesh != null)
                Destroy(wakeMesh);
            foreach (var m in made.Values)
                if (m != null)
                    Destroy(m);
            if (Volume != null && Volume.profile != null)
                Destroy(Volume.profile);
            if (Current == this)
                Current = null;
        }

        /// <summary>The Sun and the Moon as the level clock makes them: real lux, drawn on the physically based sky.</summary>
        void Lights()
        {
            Sun = NewLight("Sun", out sunData);
            Sun.useColorTemperature = true;
            Sun.colorTemperature = 5778f;
            Sun.color = Color.white;
            sunData.interactsWithSky = true;
            sunData.angularDiameter = 0.53f;
            sunData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.Emission;
            sunData.flareMultiplier = 0.15f;
            sunData.flareSize = 1.2f;
            sunData.flareFalloff = 6f;
            sunData.distance = 1.496e11f;
            Moon = NewLight("Moon", out moonData);
            Moon.useColorTemperature = false;
            Moon.color = new Color(0.78f, 0.85f, 1f);
            moonData.interactsWithSky = true;
            moonData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.ReflectSunLight;
            moonData.sunLightOverride = Sun;
            moonData.angularDiameter = 0.52f;
            moonData.diameterMultiplerMode = false;
            moonData.diameterOverride = 0.52f;
            moonData.distance = 3.84e8f;
            moonData.flareMultiplier = 0f;
            moonData.earthshine = 1f;
        }

        Light NewLight(string name, out HDAdditionalLightData data)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.lightUnit = LightUnit.Lux;
            l.intensity = 0f;
            l.shadows = LightShadows.None;
            data = go.AddComponent<HDAdditionalLightData>();
            return l;
        }

        /// <summary>The stage's own volume, on its own layer: only the stage's camera takes it (its volumeLayerMask), and
        /// that camera takes no other but the project's defaults, which this overrides where it matters.</summary>
        void Atmosphere()
        {
            var go = new GameObject("Sky and air");
            go.transform.SetParent(transform, false);
            Volume = go.AddComponent<Volume>();
            Volume.isGlobal = true;
            Volume.priority = 100f;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Ship stage (runtime)";
            profile.hideFlags = HideFlags.DontSave;
            Volume.profile = profile;
            environment = profile.Add<VisualEnvironment>(false);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.cloudType.Override((int)CloudType.CloudLayer);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            environment.renderingSpace.Override(RenderingSpace.Camera);   // (the sky goes with the camera: the stage may stand anywhere)
            sky = profile.Add<PhysicallyBasedSky>(false);
            sky.groundTint.Override(new Color(0.6f, 0.64f, 0.68f));
            if (kit.stars != null)
            {
                sky.spaceEmissionTexture.Override(kit.stars);
                sky.spaceEmissionMultiplier.Override(5f);
            }
            clouds = profile.Add<CloudLayer>(false);
            fog = profile.Add<Fog>(false);
            fog.enabled.Override(true);
            fog.enableVolumetricFog.Override(false);
            fog.maxFogDistance.Override(20000f);
            exposure = profile.Add<Exposure>(false);
            exposure.mode.Override(ExposureMode.Fixed);
            var vc = profile.Add<VolumetricClouds>(false);
            vc.enable.Override(false);
        }

        void Rain()
        {
            var go = new GameObject("Rain", typeof(ParticleSystem));
            go.transform.SetParent(View.Camera.transform, false);
            go.transform.localPosition = new Vector3(0f, 9f, 9f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rain = go.GetComponent<ParticleSystem>();
            var main = rain.main;
            main.loop = true;
            main.startLifetime = 1.3f;
            main.startSpeed = 13f;
            main.startSize = 0.03f;
            main.startColor = new Color(0.72f, 0.76f, 0.8f, 0.4f);
            main.maxParticles = 1500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;   // (its camera is never enabled: nothing would count it as seen)
            var emission = rain.emission;
            emission.rateOverTime = 0f;
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 22f, 1f);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Unlit(kit.wake, new Color(0.72f, 0.76f, 0.8f, 0.4f));
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.05f;
            r.lengthScale = 1f;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>Her wake: a ribbon of foam astern, laid on the water (the levels' ShipWake draws into HDRP's water
        /// surface, which the stage has none of).</summary>
        void Wake()
        {
            const int n = 14;
            wakeVerts = new Vector3[n * 2];
            wakeColours = new Color32[n * 2];
            var tris = new int[(n - 1) * 6];
            for (int i = 0, t = 0; i < n - 1; i++)
            {
                int a = i * 2;
                tris[t++] = a; tris[t++] = a + 1; tris[t++] = a + 2;
                tris[t++] = a + 1; tris[t++] = a + 3; tris[t++] = a + 2;
            }
            wakeMesh = new Mesh { name = "Ship stage wake", hideFlags = HideFlags.HideAndDontSave };
            wakeMesh.MarkDynamic();
            wakeMesh.vertices = wakeVerts;
            wakeMesh.colors32 = wakeColours;
            wakeMesh.triangles = tris;
            wakeMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 200f);
            var go = new GameObject("Wake");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = wakeMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = Unlit(kit.wake, new Color(0.9f, 0.94f, 0.95f, 0.5f));
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        LineRenderer Line(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<LineRenderer>();
            l.useWorldSpace = false;
            l.positionCount = 3;
            l.widthMultiplier = 0.035f;
            l.sharedMaterial = Unlit(kit.line, new Color(0.55f, 0.47f, 0.33f, 1f));
            l.shadowCastingMode = ShadowCastingMode.Off;
            l.enabled = false;
            return l;
        }

        // ------------------------------------------------------------------ what the builders of its parts call

        /// <summary>One of the kit's prefabs under a parent, or (the prefab missing) a plain shape of the size it would
        /// have: an empty holder with the shape inside it, so the caller may scale and turn either the same way.</summary>
        public GameObject Spawn(GameObject prefab, Transform parent, string name, PrimitiveType standIn, Vector3 standInSize)
        {
            GameObject go;
            if (prefab != null)
                go = Instantiate(prefab, parent);
            else
            {
                go = new GameObject();
                go.transform.SetParent(parent, false);
                var shape = GameObject.CreatePrimitive(standIn);
                shape.name = "Stand-in";
                shape.transform.SetParent(go.transform, false);
                shape.transform.localScale = standInSize;
                shape.GetComponent<Renderer>().sharedMaterial = Lit(null, new Color(0.42f, 0.3f, 0.2f), 0.2f);
            }
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Adopt(go);
            return go;
        }

        /// <summary>Takes an object onto the stage: its layer, no colliders (the chart's ship asks Physics what lies under
        /// her: SailingShip.GroundAt, ShipMotion.Ground), and the kit's HDRP materials for the pack's URP ones.</summary>
        public void Adopt(GameObject go)
        {
            changed = true;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = kit.layer;
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Destroy(c);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var to = kit.Swap(mats[i]);
                    if (to == null && mats[i] != null && (mats[i].shader == null || !mats[i].shader.isSupported || mats[i].shader.name == "Hidden/InternalErrorShader"))
                        to = Lit(null, new Color(0.45f, 0.33f, 0.22f), 0.2f);     // (pink otherwise: a pack material the kit has no swap for)
                    if (to != null)
                    {
                        mats[i] = to;
                        changed = true;
                    }
                }
                if (changed)
                    r.sharedMaterials = mats;
            }
        }

        /// <summary>Every renderer's materials through a look's own choice (null: left as it is).</summary>
        public void Remat(Func<Renderer, Material, Material> pick)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = pick(r, mats[i]) ?? mats[i];
                r.sharedMaterials = mats;
            }
        }

        /// <summary>The kit's material, or (missing) a plain HDRP/Lit of a colour, made once.</summary>
        public Material Lit(Material fromKit, Color colour, float smoothness)
        {
            if (fromKit != null)
                return fromKit;
            if (made.TryGetValue(colour, out var m) && m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Lit")) { name = "Ship stage (made)", hideFlags = HideFlags.DontSave };
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            HDMaterial.ValidateMaterial(m);
            made[colour] = m;
            return m;
        }

        Material Unlit(Material fromKit, Color colour)
        {
            if (fromKit != null)
                return fromKit;
            if (made.TryGetValue(colour, out var m) && m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Unlit")) { name = "Ship stage (made, unlit)", hideFlags = HideFlags.DontSave };
            m.SetColor("_UnlitColor", colour);
            if (colour.a < 0.99f)
            {
                m.SetFloat("_SurfaceType", 1f);
                m.SetFloat("_BlendMode", 0f);
            }
            HDMaterial.ValidateMaterial(m);
            made[colour] = m;
            return m;
        }

        /// <summary>The look in use (null: the plain one). Setting it takes the old one off first.</summary>
        public IShipStageLook Look
        {
            get => look;
            set
            {
                if (look == value)
                    return;
                look?.Remove(this);
                look = value;
                look?.Apply(this);
            }
        }

        /// <summary>The crew aboard (null: none).</summary>
        public IShipCrewRig Crew
        {
            get => crew;
            set
            {
                if (crew == value)
                    return;
                crew?.Detach();
                crew = value;
                crew?.Attach(this, Ship);
            }
        }

        /// <summary>The next step shows the state as if it had long been so: the sea raised, the boat settled, the jetty in
        /// its place, the camera where it wants to be (a test's picture, a loaded game).</summary>
        public void Settle()
        {
            snap = true;
            Sea.Settle();
        }

        // ------------------------------------------------------------------ each frame

        void LateUpdate()
        {
            if (Running)
                Step(Time.deltaTime);
        }

        /// <summary>Reads the record and moves the stage on by dt real seconds, then draws the picture if its turn has come.</summary>
        public void Step(float dt)
        {
            if (Source != null)
                Source.Read(ref State);
            var s = State;
            Sea.Step(s, dt);
            Ship.Tick(s, Sea, dt, snap);
            Water.Tick(Sea);
            SkyAndAir(s, dt);
            DockStep(s, dt);
            CameraStep(s, dt);
            WakeStep(s);
            crew?.Tick(s, dt);
            snap = false;
            if (changed)
            {
                changed = false;
                Changed?.Invoke();
            }
            View.Render();
        }

        void SkyAndAir(in ShipStageState s, float dt)
        {
            var now = StageSky.At(s.julianDay, s.latitude, s.longitude);
            SkyNow = now;
            float through = s.cloudLight > 0f ? s.cloudLight : 1f;
            Sun.transform.localRotation = Quaternion.LookRotation(-now.toSun, Vector3.up);
            Sun.intensity = kit.sunLux * (float)LightingModel.CloudDim(through) * (1f - now.sunCovered);
            Sun.enabled = true;                                           // (all night: the sky's twilight comes from where it stands under the horizon)
            bool sunShadows = now.sunAltitude > 1f;
            sunData.EnableShadows(sunShadows);
            bool moonUp = now.moonAltitude > -1f;
            float veil = Mathf.Lerp(1f, 0.3f, Mathf.Max(s.overcast, s.cloud * 0.5f));
            Moon.transform.localRotation = Quaternion.LookRotation(-now.toMoon, Vector3.up);
            Moon.intensity = moonUp ? now.moonLux * kit.moonBoost * veil : 0f;
            Moon.enabled = true;
            moonData.EnableShadows(!sunShadows && moonUp && Moon.intensity > 0.01f);
            float night = Mathf.InverseLerp(0f, -12f, now.sunAltitude);
            moonData.surfaceTint = Color.white * Mathf.Lerp(0.15f, 0.01f, night);
            sky.horizonTint.Override(Color.Lerp(Color.white, new Color(0.62f, 0.78f, 1f), night));
            sky.zenithTint.Override(Color.Lerp(Color.white, new Color(0.85f, 0.9f, 1f), night));
            if (kit.stars != null)
            {
                // the stars, turned to the moment and her place as the levels' SkyDome turns them (precession once a game day)
                double ut = Kapman.World.Sky.ToUt(s.julianDay, s.longitude);
                if (precession == null || Math.Abs(ut - precessionDay) > 1.0)
                {
                    precession = SkyMath.Precession(SkyMath.Jde(ut));
                    precessionDay = ut;
                }
                sky.spaceRotation.Override(Kapman.Levels.SkyDome.SkyRotation(s.latitude, SkyMath.Gmst(ut) + s.longitude, precession).eulerAngles);
            }

            // the exposure: fixed at what the lighting model says the scene is, so it never hunts and a test's picture is
            // the same every run (the levels' SkyDome lets HDRP meter the picture and floors it by the eye's adaptation)
            double lux = LightingModel.TotalLux(now.sunAltitude, s.overcast, s.storm, now.moonLux, now.moonAltitude, now.sunCovered);
            float want = Mathf.Clamp((float)LightingModel.EV100FromLux(lux) + kit.exposureBias, kit.exposureFloor, kit.exposureCeiling);
            ev = snap ? want : Mathf.Lerp(ev, want, 1f - Mathf.Exp(-dt * 2f));
            exposure.fixedExposure.Override(ev);

            // the air: fog by how far one sees (Koschmieder: the range is 3.912 mean free paths), lying low when it is thick
            fog.meanFreePath.Override(Mathf.Clamp(Mathf.Max(s.visibility, 30f) / 3.912f, 8f, 40000f));
            fog.baseHeight.Override(kit.origin.y + 6f);
            fog.maximumHeight.Override(kit.origin.y + Mathf.Lerp(2500f, 120f, Mathf.Clamp01(s.fog)));

            // the cloud: HDRP's cloud layer (one flat layer drawn with the sky: cheap), more of its stratus as it greys over
            clouds.opacity.Override(Mathf.Clamp01(s.cloud));
            clouds.layerA.opacityR.Override(Mathf.Clamp01(1f - 0.6f * s.overcast));
            clouds.layerA.opacityG.Override(Mathf.Clamp01(s.overcast));
            clouds.layerA.opacityB.Override(0.35f);
            clouds.layerA.opacityA.Override(0.2f);
            clouds.layerA.tint.Override(Color.Lerp(Color.white, new Color(0.5f, 0.52f, 0.56f), Mathf.Max(s.overcast, s.storm)));

            // rain, driven down the wind
            var emission = rain.emission;
            emission.rateOverTime = (s.rain + s.snow * 0.5f) * 900f;
            var vel = rain.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            float to = (s.windFrom + 180f) * Mathf.Deg2Rad;
            vel.x = new ParticleSystem.MinMaxCurve(Mathf.Sin(to) * s.windSpeed * 0.35f);
            vel.y = new ParticleSystem.MinMaxCurve(0f);
            vel.z = new ParticleSystem.MinMaxCurve(Mathf.Cos(to) * s.windSpeed * 0.35f);
            if (emission.rateOverTime.constant > 1f && !rain.isPlaying)
                rain.Play();
        }

        /// <summary>
        /// The jetty comes and goes without a jump. It stands in the world: while it is on the stage it goes astern with
        /// the water at her speed. Coming in (berthing) it is put ahead of her by the chart's own distance to the berth
        /// (one game metre of the chart is one metre here, and both close at her speed, so jetty and water move as one)
        /// and a little wide, closing to her side as she comes up; once she is made fast it is eased the last of the way
        /// alongside (she stops up to five metres short on the chart: here she is warped in). Leaving, it is simply left
        /// astern and taken away at 260 m.
        /// </summary>
        void DockStep(in ShipStageState s, float dt)
        {
            float a = s.heading * Mathf.Deg2Rad;
            Vector3 fwd = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)), right = new Vector3(fwd.z, 0f, -fwd.x);
            Vector3 way = fwd * (s.speed * s.timeFactor);
            bool want = s.moored || s.berthing;
            if (want)
            {
                Vector3 target = -right * (Ship.Beam * 0.5f + Dock.gap);
                if (!s.moored)
                    target += fwd * s.berthDistance - right * (6f * Mathf.Clamp01(s.berthDistance / 40f));
                if (!DockUp || dockFor != s.portId)
                {
                    Dock.Build(this, kit, transform, s.portId);
                    dockFor = s.portId;
                    dockAt = target;
                    dockYaw = s.heading;
                    DockUp = true;
                }
                else
                {
                    dockAt -= way * dt;
                    float k = snap ? 1f : 1f - Mathf.Exp(-dt * (s.moored ? 1.2f : 1.5f));
                    dockAt = Vector3.Lerp(dockAt, target, k);
                    dockYaw = Mathf.LerpAngle(dockYaw, s.heading, k);
                }
            }
            else if (DockUp)
            {
                dockAt -= way * dt;
                if (dockAt.magnitude > 260f)
                {
                    Dock.Destroy();
                    DockUp = false;
                    dockFor = null;
                }
            }
            bool lines = false;
            if (DockUp && Dock.Root != null)
            {
                Dock.Root.localPosition = new Vector3(dockAt.x, 0f, dockAt.z);
                Dock.Root.localRotation = Quaternion.Euler(0f, dockYaw, 0f);
                Vector3 berth = -right * (Ship.Beam * 0.5f + Dock.gap);
                lines = s.moored && (dockAt - berth).magnitude < 1.5f;
                if (lines)
                {
                    Rope(bowLine, Ship.Root.localPosition + Ship.Root.localRotation * Ship.BowCleat, Dock.Root.localPosition + Dock.Root.localRotation * Dock.BowPost);
                    Rope(sternLine, Ship.Root.localPosition + Ship.Root.localRotation * Ship.SternCleat, Dock.Root.localPosition + Dock.Root.localRotation * Dock.SternPost);
                }
            }
            bowLine.enabled = sternLine.enabled = lines;
        }

        static void Rope(LineRenderer line, Vector3 from, Vector3 to)
        {
            line.SetPosition(0, from);
            line.SetPosition(1, (from + to) * 0.5f + Vector3.down * 0.12f);
            line.SetPosition(2, to);
        }

        /// <summary>The camera stands off her port quarter and turns with her, slowly; with the jetty alongside (to port)
        /// it goes round to her starboard side, so the jetty and the bank are behind her. It keeps its height over the
        /// still water (she is seen to ride the seas), but never under a crest.</summary>
        void CameraStep(in ShipStageState s, float dt)
        {
            float L = Ship.Length;
            bool dockSide = DockUp && dockAt.magnitude < 45f;
            float wantAz = dockSide ? 68f : -148f;
            if (snap)
            {
                camAzimuth = wantAz;
                camHeading = s.heading;
            }
            else
            {
                camAzimuth = Mathf.LerpAngle(camAzimuth, wantAz + 9f * Mathf.Sin(Time.time * 0.11f), 1f - Mathf.Exp(-dt * 0.5f));
                camHeading = Mathf.LerpAngle(camHeading, s.heading, 1f - Mathf.Exp(-dt * 0.8f));
            }
            float off = L * (Big ? 1.75f : 1.5f);
            Vector3 eye = Quaternion.Euler(0f, camHeading + camAzimuth, 0f) * Vector3.forward * off;
            eye.y = Mathf.Max(L * 0.42f, Height(eye.x, eye.z) + 0.7f);
            Vector3 aim = new Vector3(0f, L * 0.14f + Ship.Root.localPosition.y * 0.5f, 0f);
            var cam = View.Camera.transform;
            cam.localPosition = eye;
            cam.localRotation = Quaternion.LookRotation(aim - eye, Vector3.up);
            View.Camera.fieldOfView = Big ? 40f : 34f;
        }

        void WakeStep(in ShipStageState s)
        {
            int n = wakeVerts.Length / 2;
            float a = s.heading * Mathf.Deg2Rad;
            Vector3 fwd = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)), right = new Vector3(fwd.z, 0f, -fwd.x);
            float strength = Mathf.Clamp01(s.speed / 3f) * (s.moored ? 0f : 1f);
            float half = Ship.Length * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float back = half * 0.8f + i * (0.6f + 0.5f * s.speed);
                // (a streak of foam that narrows to nothing, and to no width at all with no way on: HDRP/Unlit does not read the vertex alpha below, a look may)
                float wide = Ship.Beam * 0.42f * strength * Mathf.Pow(1f - i / (float)(n - 1), 0.7f);
                byte alpha = (byte)(255f * strength * (1f - i / (float)(n - 1)) * 0.55f);
                for (int side = 0; side < 2; side++)
                {
                    Vector3 p = -fwd * back + right * (side == 0 ? -wide : wide);
                    p.y = Height(p.x, p.z) + 0.04f;
                    wakeVerts[i * 2 + side] = p;
                    wakeColours[i * 2 + side] = new Color32(255, 255, 255, alpha);
                }
            }
            wakeMesh.vertices = wakeVerts;
            wakeMesh.colors32 = wakeColours;
        }
    }
}
