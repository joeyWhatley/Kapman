// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/ShipViewLook.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity (not run: no frame of it has been seen).
using System;
using System.Collections.Generic;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE SHIP WINDOW'S LOOK, AND THE SWITCH BETWEEN ITS STYLES (the user, 7 Oct 2026: "create a few different shaders,
    /// make it psx style in a few different ways"). One component on the stage. Give it the stage's camera and root; it
    /// owns the picture the frame shows (<see cref="Output"/>) and, for the style chosen (PsxStyles.All):
    ///   - makes the stage camera draw into a low RenderTexture read with hard pixels (so many lines; each picture pixel
    ///     a whole number of screen pixels), and holds pictures back to 30 a second where the style says so;
    ///   - sets the camera up for it (no anti-aliasing, HDRP's lens effects and motion vectors off, the water off when
    ///     the grid sea is drawn): per-camera frame settings, as Editor/DorestadVegetation.cs already does;
    ///   - swaps the materials on everything under the stage's root between HDRP's lit ones and the console's
    ///     (the pack's own materials are URP's and draw pink here: see LOOK_AND_MOTION.md);
    ///   - switches between HDRP's water and the grid sea;
    ///   - sets the numbers the console shaders read (the vertex grid, affine, flat, the light, the fog);
    ///   - cuts the finished picture's colours down (15-bit with the console's dither, or a few tones of the frame's
    ///     colours) by a plain Graphics.Blit into a second RenderTexture: outside HDRP, so it can only touch this picture.
    /// The frame shows Output with no filtering, each picture pixel PixelSize screen pixels.
    /// </summary>
    [DefaultExecutionOrder(210)]
    public sealed class ShipViewLook : MonoBehaviour
    {
        [Serializable]
        public sealed class Swap
        {
            [Tooltip("The pack's material this stands for, by name: SMV_BasePallete, SMV_Environment, SMV_Weapons.")]
            public string source;
            [Tooltip("HDRP/Lit with the same palette texture.")]
            public Material hdrp;
            [Tooltip("Kapman/ShipView/PSX Model with the same palette texture.")]
            public Material psx;
        }

        [Header("The stage (the other half makes these)")]
        public Camera stageCamera;
        [Tooltip("Everything the stage draws hangs under this: the ship, the men, the dock.")]
        public Transform stageRoot;
        [Tooltip("The object that holds HDRP's water surface (switched off while the grid sea is drawn).")]
        public GameObject hdrpSea;
        public PsxSeaGrid gridSea;

        [Header("The look")]
        public Swap[] swaps = new Swap[0];
        [Tooltip("Hidden/Kapman/ShipView/PSX Screen (set here so a build takes the shader with it).")]
        public Shader screenShader;
        [Tooltip("Which of PsxStyles.All.")]
        public int style;
        [Tooltip("The ink style's tones, dark to light. Empty: the frame's plum, the palette's sea, its accent, paper.")]
        public Color[] ramp = new Color[0];

        [Header("The light for the console shaders (from the clock unless switched off)")]
        public bool lightFromClock = true;
        [Tooltip("The clock's own values scaled: the sun, the sky's light from above, and from below.")]
        public float sunScale = 0.45f, skyScale = 1.4f, groundScale = 0.9f;
        public Vector3 lightToward = new Vector3(0.3f, 0.8f, 0.5f);
        public Color lightColour = Color.white, skyColour = new Color(0.3f, 0.43f, 0.6f), groundColour = new Color(0.25f, 0.27f, 0.29f), fogColour = new Color(0.64f, 0.68f, 0.72f);
        [Tooltip("How far one can see now, metres (the stage sets it from the weather): the console styles' fog closes at it.")]
        public float visibility = 20000f;

        /// <summary>The picture to show: read it with no filtering, each of its pixels PixelSize screen pixels.</summary>
        public Texture Output => processed && screenRT != null ? (Texture)screenRT : stageRT;
        /// <summary>Screen pixels to a picture pixel (1 in the plain style).</summary>
        public int PixelSize { get; private set; } = 1;
        public PsxStyle Style => PsxStyles.All[Mathf.Clamp(style, 0, PsxStyles.All.Length - 1)];
        /// <summary>The picture was made anew (another size, another style): show the new one.</summary>
        public event Action<Texture> OutputChanged;

        RenderTexture stageRT, screenRT;
        Material screenMaterial;
        Texture2D rampTex;
        bool processed, applied;
        int shownWidth = 448, shownHeight = 256;
        float baseFar = -1f, hold;

        struct Slot { public Renderer renderer; public int index; public Swap swap; }
        readonly List<Slot> slots = new List<Slot>();
        bool collected;

        static readonly int GridId = Shader.PropertyToID("_PsxGrid"), OptionsId = Shader.PropertyToID("_PsxOptions");
        static readonly int SunDirId = Shader.PropertyToID("_PsxSunDir"), SunColourId = Shader.PropertyToID("_PsxSunColour");
        static readonly int SkyColourId = Shader.PropertyToID("_PsxSkyColour"), GroundColourId = Shader.PropertyToID("_PsxGroundColour");
        static readonly int FogColourId = Shader.PropertyToID("_PsxFogColour"), FogId = Shader.PropertyToID("_PsxFog");
        static readonly int ScreenId = Shader.PropertyToID("_PsxScreen"), RampId = Shader.PropertyToID("_RampTex");

        // HDRP's lens and film effects, off in every style but the plain one (tone mapping, grading and exposure stay)
        static readonly FrameSettingsField[] Lens =
        {
            FrameSettingsField.Bloom, FrameSettingsField.DepthOfField, FrameSettingsField.MotionBlur, FrameSettingsField.FilmGrain,
            FrameSettingsField.ChromaticAberration, FrameSettingsField.Vignette, FrameSettingsField.LensDistortion, FrameSettingsField.Dithering,
        };
        // what the console shaders make no use of
        static readonly FrameSettingsField[] Heavy =
        {
            FrameSettingsField.SSR, FrameSettingsField.SSAO, FrameSettingsField.ContactShadows, FrameSettingsField.Volumetrics,
        };

        // ------------------------------------------------------------------ what the frame and the stage call

        /// <summary>The next style round (the user's button).</summary>
        public void Next() => SetStyle((style + 1) % PsxStyles.All.Length);

        public void SetStyle(int index)
        {
            style = Mathf.Clamp(index, 0, PsxStyles.All.Length - 1);
            Apply();
        }

        /// <summary>The size the picture is shown at, in screen pixels (the small window's, or the whole screen's when the
        /// ship and the chart have changed places). Call it when that changes.</summary>
        public void Show(int width, int height)
        {
            width = Mathf.Max(16, width);
            height = Mathf.Max(16, height);
            if (width == shownWidth && height == shownHeight && applied)
                return;
            shownWidth = width;
            shownHeight = height;
            Apply();
        }

        /// <summary>After the stage's contents change (the dock comes, a man is added): find the materials to swap again.</summary>
        public void Recollect()
        {
            collected = false;
            if (applied)
                SwapMaterials(Style.shade != PsxShade.Hdrp);
        }

        // ------------------------------------------------------------------ applying a style

        void OnEnable()
        {
            RenderPipelineManager.endContextRendering += AfterRendering;
            Apply();
        }

        void OnDisable()
        {
            RenderPipelineManager.endContextRendering -= AfterRendering;
            if (stageCamera != null)
            {
                // (off first: a camera with no target would draw over the chart)
                stageCamera.enabled = false;
                stageCamera.targetTexture = null;
            }
            Release(ref stageRT);
            Release(ref screenRT);
            CoreUtils.Destroy(screenMaterial);
            CoreUtils.Destroy(rampTex);
            applied = false;
        }

        void Apply()
        {
            if (stageCamera == null || !isActiveAndEnabled)
                return;
            var s = Style;

            // the picture: so many lines, each pixel a whole number of screen pixels
            int k = 1, w = shownWidth, h = shownHeight;
            if (s.lines > 0)
            {
                k = Mathf.Max(Mathf.Max(1, s.minPixel), Mathf.RoundToInt(shownHeight / (float)s.lines));
                w = Mathf.Max(16, Mathf.CeilToInt(shownWidth / (float)k));
                h = Mathf.Max(16, Mathf.CeilToInt(shownHeight / (float)k));
            }
            PixelSize = k;
            processed = s.levels < 256 || s.tones >= 2;
            Make(ref stageRT, w, h, 24, s.lines > 0 ? FilterMode.Point : FilterMode.Bilinear, "Ship view (stage)");
            if (processed)
                Make(ref screenRT, w, h, 0, FilterMode.Point, "Ship view (screen)");
            else
                Release(ref screenRT);
            stageCamera.targetTexture = stageRT;
            stageCamera.ResetAspect();                                    // (a shape set by hand would outlive the picture's)

            // the camera
            bool console = s.shade != PsxShade.Hdrp;
            if (!stageCamera.TryGetComponent<HDAdditionalCameraData>(out var hd))
                hd = stageCamera.gameObject.AddComponent<HDAdditionalCameraData>();
            hd.antialiasing = s.smoothEdges ? HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing : HDAdditionalCameraData.AntialiasingMode.None;
            hd.dithering = false;
            hd.allowDynamicResolution = false;
            hd.customRenderingSettings = true;
            foreach (var f in Lens)
                Set(hd, f, s.lensEffects);
            foreach (var f in Heavy)
                Set(hd, f, !console && s.lines <= 0);
            // with object motion vectors on, HDRP leaves moving and skinned things out of its depth prepass and expects
            // their MotionVectors pass to write depth; the console shaders have none
            Set(hd, FrameSettingsField.MotionVectors, s.smoothEdges);
            Set(hd, FrameSettingsField.Antialiasing, s.smoothEdges);
            Set(hd, FrameSettingsField.ObjectMotionVectors, s.smoothEdges && !console);
            Set(hd, FrameSettingsField.Water, s.sea == PsxSea.Hdrp);
            if (baseFar < 0f)
                baseFar = stageCamera.farClipPlane;
            stageCamera.farClipPlane = s.drawDistance > 0f ? s.drawDistance : baseFar;

            // the models and the sea
            SwapMaterials(console);
            if (hdrpSea != null && hdrpSea.activeSelf != (s.sea == PsxSea.Hdrp))
                hdrpSea.SetActive(s.sea == PsxSea.Hdrp);
            if (gridSea != null)
            {
                gridSea.cell = Mathf.Max(0.5f, s.seaCell);
                if (gridSea.gameObject.activeSelf != (s.sea == PsxSea.Grid))
                    gridSea.gameObject.SetActive(s.sea == PsxSea.Grid);
            }

            // the numbers the console shaders read that only a style changes
            Shader.SetGlobalVector(GridId, s.snap > 0f ? new Vector4(w / s.snap, h / s.snap, 0f, 0f) : Vector4.zero);
            Shader.SetGlobalVector(OptionsId, new Vector4(s.affine, s.shade == PsxShade.Flat ? 1f : 0f, s.depthSteps, s.white));

            // the screen pass
            if (processed)
            {
                if (screenMaterial == null)
                {
                    var sh = screenShader != null ? screenShader : Shader.Find("Hidden/Kapman/ShipView/PSX Screen");
                    if (sh != null)
                        screenMaterial = new Material(sh) { name = "Ship view screen", hideFlags = HideFlags.DontSave };
                    else
                        Debug.LogWarning("Kapman ship view: the shader 'Hidden/Kapman/ShipView/PSX Screen' is missing; the picture's colours are not cut down.");
                }
                if (screenMaterial != null)
                {
                    int tones = s.tones >= 2 ? Mathf.Clamp(s.tones, 2, 8) : 0;
                    screenMaterial.SetVector(ScreenId, new Vector4(Mathf.Clamp(s.levels, 2, 256), s.dither, tones,
                                                                   QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : 0f));
                    if (tones > 0)
                        screenMaterial.SetTexture(RampId, Ramp(tones));
                }
            }
            hold = 0f;
            stageCamera.enabled = true;
            applied = true;
            OutputChanged?.Invoke(Output);
        }

        static void Set(HDAdditionalCameraData hd, FrameSettingsField field, bool on)
        {
            hd.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)field] = true;
            hd.renderingPathCustomFrameSettings.SetEnabled(field, on);
        }

        static void Make(ref RenderTexture rt, int w, int h, int depth, FilterMode filter, string name)
        {
            if (rt != null && rt.width == w && rt.height == h && rt.filterMode == filter)
                return;
            Release(ref rt);
            rt = new RenderTexture(w, h, depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                name = name, antiAliasing = 1, filterMode = filter, wrapMode = TextureWrapMode.Clamp, useMipMap = false,
                hideFlags = HideFlags.DontSave,
            };
            rt.Create();
        }

        static void Release(ref RenderTexture rt)
        {
            if (rt == null)
                return;
            rt.Release();
            CoreUtils.Destroy(rt);
            rt = null;
        }

        /// <summary>The ink style's ramp as a texture of `tones` texels, dark to light.</summary>
        Texture2D Ramp(int tones)
        {
            Color[] from = ramp != null && ramp.Length >= 2 ? ramp : new Color[]
            {
                Kapman.Game.Frame.FrameColours.Plum, Kapman.Game.Frame.FrameColours.EdgeNow,
                Kapman.Game.Frame.FrameColours.AccentNow, Kapman.Game.Frame.FrameColours.Paper,
            };
            if (rampTex == null || rampTex.width != tones)
            {
                CoreUtils.Destroy(rampTex);
                rampTex = new Texture2D(tones, 1, TextureFormat.RGBA32, false)
                {
                    name = "Ship view ramp", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave,
                };
            }
            for (int i = 0; i < tones; i++)
            {
                float at = tones > 1 ? i / (float)(tones - 1) * (from.Length - 1) : 0f;
                int a = Mathf.Min((int)at, from.Length - 1), b = Mathf.Min(a + 1, from.Length - 1);
                rampTex.SetPixel(i, 0, Color.Lerp(from[a], from[b], at - a));
            }
            rampTex.Apply(false, false);
            return rampTex;
        }

        // ------------------------------------------------------------------ the materials

        Swap Find(Material m)
        {
            if (m == null)
                return null;
            foreach (var sw in swaps)
                if (sw != null && (m == sw.hdrp || m == sw.psx || m.name == sw.source))
                    return sw;
            return null;
        }

        void SwapMaterials(bool console)
        {
            if (stageRoot == null)
                return;
            if (!collected)
            {
                slots.Clear();
                foreach (var r in stageRoot.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        var sw = Find(mats[i]);
                        if (sw != null)
                            slots.Add(new Slot { renderer = r, index = i, swap = sw });
                    }
                }
                collected = true;
            }
            foreach (var slot in slots)
            {
                if (slot.renderer == null)
                    continue;
                var want = console ? slot.swap.psx : slot.swap.hdrp;
                if (want == null)
                    continue;
                var mats = slot.renderer.sharedMaterials;
                if (slot.index < mats.Length && mats[slot.index] != want)
                {
                    mats[slot.index] = want;
                    slot.renderer.sharedMaterials = mats;
                }
            }
        }

        // ------------------------------------------------------------------ every frame

        void Update()
        {
            if (stageCamera == null || !applied)
                return;
            // pictures held back: the camera draws only when one is due (its RenderTexture keeps the last one between)
            int fps = Style.framesPerSecond;
            if (fps <= 0)
            {
                if (!stageCamera.enabled)
                    stageCamera.enabled = true;
                return;
            }
            hold += Time.unscaledDeltaTime;
            float every = 1f / fps;
            bool due = hold >= every;
            if (due)
                hold = Mathf.Min(hold - every, every);
            if (stageCamera.enabled != due)
                stageCamera.enabled = due;
        }

        void LateUpdate()
        {
            if (!applied)
                return;
            var s = Style;
            Vector3 toward;
            Color light, sky, ground, fog;
            if (lightFromClock && LevelClock.Current != null)
            {
                // the clock's own picture of the light (LevelClock: relative values, about 2 for the full sun), by day
                // the sun and by night the moon, whichever is the stronger
                Color sun = LevelClock.SunColour * sunScale;
                float moon = Mathf.Clamp01(LevelClock.MoonLightLux.maxColorComponent / 0.3f) * 0.18f;
                if (sun.maxColorComponent >= moon)
                {
                    toward = LevelClock.SunDirection;
                    light = sun;
                }
                else
                {
                    toward = LevelClock.MoonDirection;
                    light = new Color(0.78f, 0.85f, 1f) * moon;
                }
                sky = LevelClock.SkyZenith * skyScale;
                ground = LevelClock.SkyHorizon * groundScale;
                fog = LevelClock.SkyHorizon;
            }
            else
            {
                toward = lightToward;
                light = lightColour;
                sky = skyColour;
                ground = groundColour;
                fog = fogColour;
            }
            if (toward.sqrMagnitude < 1e-6f)
                toward = Vector3.up;
            Shader.SetGlobalVector(SunDirId, toward.normalized);
            Shader.SetGlobalColor(SunColourId, light);
            Shader.SetGlobalColor(SkyColourId, sky);
            Shader.SetGlobalColor(GroundColourId, ground);
            Shader.SetGlobalColor(FogColourId, fog);
            float far = Mathf.Min(Mathf.Max(20f, visibility), stageCamera != null ? stageCamera.farClipPlane : 1000f);
            Shader.SetGlobalVector(FogId, new Vector4(far * s.fogStart, far * Mathf.Max(s.fogEnd, s.fogStart + 0.01f), s.fogMost, 0f));
        }

        void AfterRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            // the finished low picture, its colours cut down, into the picture the frame shows
            if (!processed || screenMaterial == null || stageRT == null || screenRT == null)
                return;
            if (stageCamera == null || !cameras.Contains(stageCamera))
                return;
            Graphics.Blit(stageRT, screenRT, screenMaterial);
        }
    }
}
