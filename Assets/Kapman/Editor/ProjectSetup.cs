using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE PROJECT'S SETTINGS - HDRP for the most realistic water, sky and fog Unity can do (the user, 3 October 2026:
    /// no stylizing filters at all; realism, dark and quiet, the warm light in the dark from real lights).
    ///
    /// Apply() makes (or brings up to date) one HDRP asset, Look/KapmanHDRP.asset, with the water system (decals for
    /// wakes and shore foam, exclusion so the sea stays out of hulls, the CPU copy of the sea the ships ride), volumetric
    /// fog and clouds, screen-space reflections on the water, soft (PCSS) shadows, and sets it as the project's default
    /// pipeline and on every quality level. It makes sure HDRP's global settings exist, and writes the one volume
    /// profile the levels use (Look/KapmanVolume.asset): the physically based sky with the star cube as its space
    /// emission, volumetric clouds, volumetric fog, a histogram auto exposure whose floor is low enough that moonlight
    /// and lamps show, a modest bloom only for lamps, fires and glints, ACES, no vignette, a touch of contrast.
    /// Menu: Kapman > Setup > Apply the project settings.
    /// </summary>
    public static class ProjectSetup
    {
        public const string LookDir = "Assets/Kapman/Look";
        public const string PipelinePath = LookDir + "/KapmanHDRP.asset";
        public const string VolumePath = LookDir + "/KapmanVolume.asset";
        public const string StarsPath = "Assets/Kapman/Sky/Stars_930.exr";

        // the URP materials of the first look are gone (their shaders with them). These names stay so the scene builders
        // compile until they're rewired: nothing is made at these paths any more, and loading them gives null.
        public const string AtmosphereMatPath = LookDir + "/KapmanAtmosphere.mat";
        public const string SkyMatPath = LookDir + "/KapmanSky.mat";
        public const string WaterMatPath = LookDir + "/KapmanWater.mat";

        /// <summary>The exposure's floor and ceiling (EV100). The floor is where a moonlit night sits: -3 rendered black in the first build.</summary>
        public const float ExposureMin = -5f, ExposureMax = 15f;

        /// <summary>The star cube's brightness over the true value: a little more shows the sky as the dark-adapted eye sees it.</summary>
        public const float StarLight = 3f;

        [MenuItem("Kapman/Setup/Apply the project settings")]
        public static void Apply()
        {
            System.IO.Directory.CreateDirectory(LookDir);
            var asset = PipelineAsset();
            Configure(asset);
            Install(asset);
            EnsureGlobalSettings();
            Volume();
            RegisterDiffusionProfiles();
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman setup: HDRP set (water, volumetric fog and clouds, SSR, soft shadows) and the volume profile written to " + VolumePath + ".");
        }

        /// <summary>
        /// Every diffusion profile in the project's assets (the meadow pack's foliage and trees) on HDRP's default list, so
        /// translucent leaves and grass light as their makers set them; a material whose profile is not listed renders
        /// magenta-tinted (4 Oct: the pack's grass came out red-purple). HDRP takes at most 15.
        /// </summary>
        public static void RegisterDiffusionProfiles()
        {
            const string DefaultsPath = "Assets/HDRPDefaultResources/DefaultSettingsVolumeProfile.asset";
            var defaults = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DefaultsPath);
            if (defaults == null || !defaults.TryGet(out DiffusionProfileList list))
            {
                Debug.LogWarning("Kapman setup: no diffusion profile list in " + DefaultsPath + "; the pack's foliage may render magenta.");
                return;
            }
            var have = new System.Collections.Generic.List<DiffusionProfileSettings>(list.diffusionProfiles.value ?? new DiffusionProfileSettings[0]);
            int added = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:DiffusionProfileSettings", new[] { "Assets" }))
            {
                var p = AssetDatabase.LoadAssetAtPath<DiffusionProfileSettings>(AssetDatabase.GUIDToAssetPath(guid));
                if (p != null && !have.Contains(p) && have.Count < 15)
                {
                    have.Add(p);
                    added++;
                }
            }
            if (added == 0)
                return;
            list.diffusionProfiles.Override(have.ToArray());
            EditorUtility.SetDirty(list);
            EditorUtility.SetDirty(defaults);
            Debug.Log($"Kapman setup: {added} diffusion profiles registered ({have.Count} in all).");
        }

        /// <summary>The project's HDRP asset, made at PipelinePath if it is not there yet.</summary>
        public static HDRenderPipelineAsset PipelineAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PipelinePath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
                asset.name = "KapmanHDRP";
                AssetDatabase.CreateAsset(asset, PipelinePath);
            }
            return asset;
        }

        static void Configure(HDRenderPipelineAsset asset)
        {
            var s = asset.currentPlatformRenderPipelineSettings;

            // the water system: the sea and the rivers, with decals (wakes, bow waves, shore foam) and exclusion (the lid
            // inside a hull that keeps the sea out of her)
            s.supportWater = true;
            s.supportWaterDecals = true;
            s.supportWaterExclusion = true;
            s.supportWaterHorizontalDeformation = false;
            s.waterSimulationResolution = WaterSimulationResolution.High256;
            if (s.maximumWaterDecalCount < 128)
                s.maximumWaterDecalCount = 128;            // a ship's trail and strikes plus the shore's foam take more than 48
            s.waterScriptInteractionsMode = WaterScriptInteractionsMode.CPUSimulation;
            s.waterFullCPUSimulation = true;              // the ships read the sea at the resolution the screen shows

            // fog, clouds, reflections
            s.supportVolumetrics = true;
            s.supportVolumetricClouds = true;
            s.supportSSR = true;
            s.supportSSRTransparent = true;               // the water's reflections
            s.supportMotionVectors = true;                // SSR and TAA need them
            s.supportSSAO = true;
            s.supportSSGI = true;                 // screen-space bounce (the volume turns it on)
            s.supportDecals = true;
            s.supportCustomPass = true;                   // fog banks and the like
            s.supportTransparentBackface = true;
            s.supportTransparentDepthPrepass = true;
            s.supportTransparentDepthPostpass = true;
            s.supportTerrainHole = false;
            s.msaaSampleCount = MSAASamples.None;         // the cameras use TAA; MSAA would force forward-only lighting

            // shadows: soft (PCSS) filtering for the sun and the lamps, the sun's map at 4K; the reach (900 m) and the
            // four cascades are in the volume profile (HDShadowSettings) below
            var shadows = s.hdShadowInitParams;
            shadows.directionalShadowFilteringQuality = HDShadowFilteringQuality.High;
            shadows.punctualShadowFilteringQuality = HDShadowFilteringQuality.High;
            shadows.shadowResolutionDirectional = new IntScalableSetting(new[] { 1024, 2048, 4096, 4096 }, ScalableSettingSchemaId.With4Levels);
            shadows.maxDirectionalShadowMapResolution = 4096;
            shadows.supportContactShadows = true;
            s.hdShadowInitParams = shadows;

            asset.currentPlatformRenderPipelineSettings = s;
            EditorUtility.SetDirty(asset);
        }

        /// <summary>The asset as the project's default pipeline and on every quality level (the old URP assets were there).</summary>
        static void Install(HDRenderPipelineAsset asset)
        {
            GraphicsSettings.defaultRenderPipeline = asset;
            int level = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                if (QualitySettings.GetRenderPipelineAssetAt(i) == asset)
                    continue;
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
            }
            QualitySettings.SetQualityLevel(level, false);
        }

        /// <summary>
        /// HDRP keeps its resources, default volume profile and frame settings in a global settings asset
        /// (Assets/HDRPDefaultResources/HDRenderPipelineGlobalSettings.asset). HDRP makes it when its asset is first
        /// made in the editor; this makes sure, through HDRP's own (internal) Ensure.
        /// </summary>
        public static void EnsureGlobalSettings()
        {
            if (GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>() != null)
                return;
            var type = typeof(HDRenderPipeline).Assembly.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings");
            var ensure = type?.GetMethod("Ensure", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (ensure != null)
                ensure.Invoke(null, new object[] { true });
            if (GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>() == null)
                Debug.LogWarning("Kapman setup: HDRP's global settings were not made; open Project Settings > Graphics > HDRP once.");
        }

        /// <summary>The volume profile the levels use (made or brought up to date).</summary>
        public static VolumeProfile Volume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumePath);
            }

            // the sky: physically based, its light following the sun (dynamic ambient)
            var environment = Get<VisualEnvironment>(profile);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);

            // the stars: the baked cube (J2000 frame) as the space emission; the sky owner turns spaceRotation at runtime
            var sky = Get<PhysicallyBasedSky>(profile);
            sky.type.Override(PhysicallyBasedSkyModel.EarthAdvanced);
            var stars = AssetDatabase.LoadAssetAtPath<Cubemap>(StarsPath);
            if (stars != null)
            {
                sky.spaceEmissionTexture.Override(stars);
                sky.spaceEmissionMultiplier.Override(StarLight);
            }
            else
                Debug.LogWarning("Kapman setup: no star cube at " + StarsPath);
            sky.spaceRotation.Override(Vector3.zero);
            sky.groundTint.Override(new Color(0.10f, 0.10f, 0.09f));      // the planet below the horizon: dull earth

            // the fog: volumetric, lit and shadowed near the camera; the weather owner drives the density and heights
            var fog = Get<Fog>(profile);
            fog.enabled.Override(true);
            fog.colorMode.Override(FogColorMode.SkyColor);
            fog.meanFreePath.Override(4000f);                                // a clear day's haze
            fog.baseHeight.Override(0f);
            fog.maximumHeight.Override(400f);
            fog.maxFogDistance.Override(20000f);
            fog.mipFogMaxMip.Override(0.6f);
            fog.mipFogNear.Override(0f);
            fog.mipFogFar.Override(3000f);
            fog.enableVolumetricFog.Override(true);
            fog.depthExtent.Override(800f);
            fog.anisotropy.Override(0.5f);                                   // brighter towards the sun
            fog.albedo.Override(Color.white);
            fog.multipleScatteringIntensity.Override(0.5f);

            // the clouds: volumetric, true size (the levels are at true scale); the preset first, since setting it
            // writes the shape values, then our own
            var clouds = Get<VolumetricClouds>(profile);
            clouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
            clouds.enable.Override(true);
            clouds.cloudControl.Override(VolumetricClouds.CloudControl.Simple);
            clouds.cloudSimpleMode.Override(VolumetricClouds.CloudSimpleMode.Quality);
            clouds.bottomAltitude.Override(1200f);
            clouds.altitudeRange.Override(2000f);
            clouds.shadows.Override(true);                                   // cloud shadows sweep the land and water
            clouds.shadowDistance.Override(8000f);
            clouds.shadowResolution.Override(VolumetricClouds.CloudShadowResolution.Medium256);
            clouds.shadowOpacity.Override(0.8f);
            clouds.temporalAccumulationFactor.Override(0.95f);
            clouds.ghostingReduction.Override(true);

            // the exposure: automatic from a histogram (the brightest and darkest parts ignored), floor low enough for
            // a moonlit night and lamps, ceiling for the summer noon
            var exposure = Get<Exposure>(profile);
            exposure.mode.Override(ExposureMode.AutomaticHistogram);
            exposure.meteringMode.Override(MeteringMode.CenterWeighted);
            exposure.limitMin.Override(ExposureMin);
            exposure.limitMax.Override(ExposureMax);
            exposure.histogramPercentages.Override(new Vector2(40f, 90f));
            exposure.compensation.Override(0f);
            exposure.adaptationMode.Override(AdaptationMode.Progressive);
            exposure.adaptationSpeedDarkToLight.Override(3f);
            exposure.adaptationSpeedLightToDark.Override(1f);

            // the bloom: modest, and only what is far brighter than the exposed white blooms (lamps, fires, sun glints)
            var bloom = Get<Bloom>(profile);
            bloom.threshold.Override(1.8f);
            bloom.intensity.Override(0.06f);
            bloom.scatter.Override(0.5f);
            bloom.anamorphic.Override(false);

            var tone = Get<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);

            var vignette = Get<Vignette>(profile);
            vignette.intensity.Override(0f);

            // the grade: a photograph, not a filter. A touch of contrast, the greens a little less loud than a
            // monitor's, the blacks lifted a hair so the night and the shade keep their detail, fine grain
            var adjust = Get<ColorAdjustments>(profile);
            adjust.postExposure.Override(0f);
            adjust.contrast.Override(8f);
            adjust.saturation.Override(-8f);
            adjust.hueShift.Override(0f);
            adjust.colorFilter.Override(Color.white);
            var lift = Get<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(1f, 1f, 1f, 0.015f));
            lift.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
            lift.gain.Override(new Vector4(1f, 1f, 1f, 0f));
            var grain = Get<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.28f);
            grain.response.Override(0.8f);

            // the ground sits in its shade: screen-space ambient occlusion, contact shadows, micro shadows
            var ao = Get<ScreenSpaceAmbientOcclusion>(profile);
            ao.intensity.Override(1f);
            ao.directLightingStrength.Override(0.2f);
            ao.radius.Override(1.5f);
            ao.temporalAccumulation.Override(true);
            var contact = Get<ContactShadows>(profile);
            contact.enable.Override(true);
            contact.length.Override(0.2f);
            contact.opacity.Override(0.9f);
            contact.maxDistance.Override(120f);
            var micro = Get<MicroShadowing>(profile);
            micro.enable.Override(true);
            micro.opacity.Override(0.8f);

            // bounced light: screen-space global illumination (the fires light the walls and the water at night, the
            // sky's light stays out of the carr's shade); it needs supportSSGI on the pipeline asset
            var gi = Get<GlobalIllumination>(profile);
            gi.enable.Override(true);
            gi.quality.Override(1);
            gi.fullResolutionSS.Override(false);

            // no motion blur: HDRP's default profile blurs camera moves at half strength (it smeared the first build)
            var blur = Get<MotionBlur>(profile);
            blur.intensity.Override(0f);

            // shadows to 900 m in four cascades
            var shadows = Get<HDShadowSettings>(profile);
            shadows.maxShadowDistance.Override(900f);
            shadows.cascadeShadowSplitCount.Override(4);
            shadows.cascadeShadowSplit0.Override(0.04f);
            shadows.cascadeShadowSplit1.Override(0.12f);
            shadows.cascadeShadowSplit2.Override(0.35f);

            // reflections on the water (and the wet ground): screen space, accumulated over frames
            var ssr = Get<ScreenSpaceReflection>(profile);
            ssr.enabled.Override(true);
            ssr.enabledTransparent.Override(true);
            ssr.reflectSky.Override(true);
            ssr.usedAlgorithm.Override(ScreenSpaceReflectionAlgorithm.PBRAccumulation);

            EditorUtility.SetDirty(profile);
            foreach (var component in profile.components)
                EditorUtility.SetDirty(component);
            return profile;
        }

        /// <summary>A volume component on the profile (added, with nothing overridden, if it is not there).</summary>
        public static T Get<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
                return component;
            component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
