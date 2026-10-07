// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/ShipStageView.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run.
// HDRP names are from Library/PackageCache/com.unity.render-pipelines.high-definition@8aadf824e504/Runtime:
//   RenderPipeline/Camera/HDAdditionalCameraData.cs (volumeLayerMask, customRenderingSettings,
//   renderingPathCustomFrameSettings, renderingPathCustomFrameSettingsOverrideMask, clearColorMode, antialiasing),
//   RenderPipeline/Settings/FrameSettings.cs (FrameSettingsField, litShaderMode),
//   RenderPipeline/HDRenderPipeline.cs:2651 (IsRenderRequestSupported: HDRP takes RenderPipeline.StandardRequest).
// The way of drawing (a camera that is switched off, drawn by a render request into a texture) is the one
// Assets/Kapman/Editor/DorestadVegetation.cs:690-715 and Editor/Shots.cs:475-486 already use in this project.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE STAGE'S CAMERA AND ITS PICTURE. The camera is never enabled, so it never draws to the screen and never costs
    /// a frame it was not asked for: <see cref="Render"/> draws it into <see cref="Texture"/> by a render request, every
    /// frame or every few (<see cref="every"/>). The frame's ship window and the full-screen view both show that one
    /// texture; only its size changes when they change places (<see cref="Size"/>).
    /// Kept cheap: it draws one layer and takes volumes from that layer only (so the chart's camera, which draws
    /// nothing, and the stage never see each other's sky, fog or exposure), forward lit, and everything a small picture
    /// of a boat does not need is off in its frame settings.
    /// </summary>
    public sealed class ShipStageView
    {
        /// <summary>What a small boat on open water does not need. Left on: opaque and transparent objects, the Sun's
        /// shadow map, fog (AtmosphericScattering), the sky's reflection, exposure, tonemapping, and custom passes and
        /// custom post-processes (the looks).</summary>
        static readonly FrameSettingsField[] Off =
        {
            FrameSettingsField.SSR, FrameSettingsField.TransparentSSR, FrameSettingsField.SSAO, FrameSettingsField.SSGI,
            FrameSettingsField.ContactShadows, FrameSettingsField.ScreenSpaceShadows, FrameSettingsField.Shadowmask,
            FrameSettingsField.SubsurfaceScattering, FrameSettingsField.Transmission,
            FrameSettingsField.Volumetrics, FrameSettingsField.ReprojectionForVolumetrics,
            FrameSettingsField.VolumetricClouds, FrameSettingsField.FullResolutionCloudsForSky,
            FrameSettingsField.MotionVectors, FrameSettingsField.ObjectMotionVectors, FrameSettingsField.TransparentsWriteMotionVector,
            FrameSettingsField.Decals, FrameSettingsField.DecalLayers, FrameSettingsField.Refraction, FrameSettingsField.Distortion,
            FrameSettingsField.RoughDistortion, FrameSettingsField.LowResTransparent, FrameSettingsField.TransparentPrepass,
            FrameSettingsField.TransparentPostpass, FrameSettingsField.DepthOfField, FrameSettingsField.MotionBlur,
            FrameSettingsField.PaniniProjection, FrameSettingsField.Bloom, FrameSettingsField.LensFlareScreenSpace,
            FrameSettingsField.LensFlareDataDriven, FrameSettingsField.LensDistortion, FrameSettingsField.ChromaticAberration,
            FrameSettingsField.Vignette, FrameSettingsField.FilmGrain, FrameSettingsField.Dithering, FrameSettingsField.Antialiasing,
            FrameSettingsField.StopNaN, FrameSettingsField.Water, FrameSettingsField.WaterDecals, FrameSettingsField.WaterExclusion,
            FrameSettingsField.RayTracing, FrameSettingsField.ReflectionProbe, FrameSettingsField.PlanarProbe,
            FrameSettingsField.AdaptiveProbeVolume, FrameSettingsField.VirtualTexturing, FrameSettingsField.HighQualityLineRendering,
            FrameSettingsField.ComputeThickness,
        };

        public Camera Camera { get; private set; }
        public HDAdditionalCameraData Data { get; private set; }
        /// <summary>The picture (made by <see cref="Size"/>; null before).</summary>
        public RenderTexture Texture { get; private set; }
        /// <summary>The picture is drawn every this many frames.</summary>
        public int every = 2;
        /// <summary>How the picture is stretched when it is shown larger than it was drawn (Point: hard pixels).</summary>
        public FilterMode filter = FilterMode.Point;
        /// <summary>The heights in pixels a look asks for (0: the kit's): in the frame's window, and filling the screen.</summary>
        public int smallHeight, bigHeight;
        /// <summary>How many pictures have been drawn.</summary>
        public int Drawn { get; private set; }

        // ---- for a look that makes the picture itself (the other builder's ShipViewLook gives the camera a texture of its
        // own, enables it on its own frames, and cuts the colours down into a second texture)
        /// <summary>The look draws the camera: <see cref="Render"/> then does nothing and <see cref="Size"/> is not called.</summary>
        public bool drawnByLook;
        /// <summary>The look's finished picture (null: <see cref="Texture"/>).</summary>
        public Texture shown;
        /// <summary>What the frame shows.</summary>
        public Texture Picture => shown != null ? shown : Texture;
        /// <summary>The size, in screen pixels, of the place the picture is shown in now (the frame's window, or the whole
        /// screen when swapped). The hook keeps it up to date; a look reads it, or listens.</summary>
        public int ShownWidth { get; private set; }
        public int ShownHeight { get; private set; }
        public event System.Action<int, int> ShownAtChanged;

        public void ShownAt(int width, int height)
        {
            if (width == ShownWidth && height == ShownHeight)
                return;
            ShownWidth = width;
            ShownHeight = height;
            ShownAtChanged?.Invoke(width, height);
        }

        int tick;

        public void Build(Transform parent, ShipStageKit kit)
        {
            var go = new GameObject("Ship stage camera") { layer = kit.layer };       // (not tagged MainCamera: the chart's is)
            go.transform.SetParent(parent, false);
            Camera = go.AddComponent<Camera>();
            Camera.enabled = false;
            Camera.cullingMask = 1 << kit.layer;
            Camera.nearClipPlane = 0.2f;
            Camera.farClipPlane = 20000f;
            Camera.fieldOfView = 34f;
            Camera.allowMSAA = false;
            Data = go.AddComponent<HDAdditionalCameraData>();
            Data.volumeLayerMask = 1 << kit.layer;
            Data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
            Data.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
            Data.customRenderingSettings = true;
            foreach (var f in Off)
            {
                Data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)f] = true;
                Data.renderingPathCustomFrameSettings.SetEnabled(f, false);
            }
            Data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.LitShaderMode] = true;
            Data.renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Forward;
            every = Mathf.Max(1, kit.smallEvery);
        }

        /// <summary>The picture's size in pixels. Makes the texture anew when it changes; true when it did.</summary>
        public bool Size(int width, int height)
        {
            width = Mathf.Clamp(width, 16, 4096);
            height = Mathf.Clamp(height, 16, 4096);
            if (Texture != null && Texture.width == width && Texture.height == height)
            {
                Texture.filterMode = filter;
                return false;
            }
            Release();
            Texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                name = "Ship stage picture", antiAliasing = 1, filterMode = filter, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave,
            };
            Texture.Create();
            tick = 0;
            return true;
        }

        /// <summary>Draws the picture if its turn has come (or now, when forced). True when it drew.</summary>
        public bool Render(bool force = false)
        {
            if (Camera == null || Texture == null || drawnByLook)
                return false;
            if (!force && tick++ % Mathf.Max(1, every) != 0)
                return false;
            RenderTo(Texture);
            return true;
        }

        /// <summary>Draws the stage into any texture (a test's large picture).</summary>
        public void RenderTo(RenderTexture target)
        {
            Camera.aspect = target.width / (float)target.height;
            var request = new RenderPipeline.StandardRequest { destination = target };
            if (RenderPipeline.SupportsRenderRequest(Camera, request))
                RenderPipeline.SubmitRenderRequest(Camera, request);
            else
            {
                Camera.targetTexture = target;
                Camera.Render();
                Camera.targetTexture = null;
            }
            Drawn++;
        }

        public void Release()
        {
            if (Texture == null)
                return;
            Texture.Release();
            Object.Destroy(Texture);
            Texture = null;
        }
    }
}
