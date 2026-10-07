// The look half of the drafts (Design/ship_view/draft/ShipViewLook.cs and the PsxStageLook of ShipViewJoin.cs) made
// into one class for the stage as it was really built: the stage draws by render request with its camera switched
// off, so the look draws the same way and no camera is ever enabled. First drawn and looked at on 7 Oct 2026
// (Design/ship_view/BUILD_LOG.md, part 2).
//
//   var look = new PsxStageLook("console");
//   ShipStage.Current.Look = look;        look.Next();   // the player's key
using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE SHIP VIEW'S LOOKS, AND THE SWITCH BETWEEN THEM (the user, 7 Oct 2026: "create a few different shaders, make
    /// it psx style in a few different ways"). One look object on the stage holds all of them (PsxStyles.All) and goes
    /// round them with <see cref="Next"/>. For the look chosen it:
    ///   - draws the stage into a LOW picture read with hard pixels (so many lines; each picture pixel a whole number of
    ///     screen pixels), and holds pictures back to 30 a second where the look says so;
    ///   - swaps the materials on everything on the stage between HDRP's lit ones and the console's (a twin of each is
    ///     made here at run time: the same palette, the shader Kapman/ShipView/PSX Model);
    ///   - switches between the stage's sea (HDRP's water) and the console's grid sea (PsxSeaGrid), and floats her on
    ///     the one that is drawn;
    ///   - sets the numbers the console shaders read: the vertex grid, affine, flat, the grain, the far water's
    ///     break-up, and THE SUN. Its light is worked out here from the stage's own sky and the game's lighting model
    ///     (lux for this hour and weather) times the stage's exposure; the sky's light the shaders read from HDRP itself
    ///     (its ambient probe, as its lit materials do: PsxCommon.hlsl). So the console's ship is as
    ///     bright against HDRP's sky as HDRP's own would be, at noon, at dusk and under the Moon, with nothing to tune;
    ///   - cuts the finished picture's colours down (15-bit with the console's dither, or four tones of the frame's
    ///     colours) with one triangle drawn into a second texture: outside HDRP, so it can only touch this picture.
    /// The sky stays HDRP's in every look (seen through the low picture and the dither): no console sky was made.
    /// The look "None" is the stage's own picture, untouched.
    /// The frame shows <see cref="Output"/> with no filtering (ShipStageView.Picture gives it).
    /// </summary>
    public sealed class PsxStageLook : IShipStageLook, IShipStageLookDraws, IShipStageSettles
    {
        /// <summary>Which of PsxStyles.All.</summary>
        public int style;
        /// <summary>Tiles of the grain to a metre (2.5: a tile is 0.4 m, two planks of 0.2 m).</summary>
        public float grainScale = 2.5f;

        public PsxStyle Style => PsxStyles.All[Mathf.Clamp(style, 0, PsxStyles.All.Length - 1)];
        public string Name => Style.name;
        /// <summary>The picture to show (null in the look "None": the stage's own). Read it with no filtering.</summary>
        public Texture Output => Style.lines <= 0 ? null : processed && screenRT != null ? (Texture)screenRT : stageRT;
        /// <summary>Screen pixels to a picture pixel (1 in the look "None").</summary>
        public int PixelSize { get; private set; } = 1;
        public PsxSeaGrid Grid => grid;
        /// <summary>The look was changed (the word on the view).</summary>
        public event Action<PsxStyle> Changed;

        ShipStage stage;
        ShipStageKit kit;
        PsxSeaGrid grid;
        RenderTexture stageRT, screenRT;
        Material screenMaterial;
        Texture2D rampTex, grainTex;
        CommandBuffer cmd;
        Shader modelShader, spriteShader;
        readonly Dictionary<Material, Material> twins = new Dictionary<Material, Material>(), twinsSkinned = new Dictionary<Material, Material>();
        readonly Dictionary<Material, Material> back = new Dictionary<Material, Material>();
        StageSeaKind kitSea;
        float baseFar = -1f, hold = 999f;
        bool gridOn;
        int tick;
        int shownWidth = 443, shownHeight = 256;
        bool processed, applied, warned;

        static readonly int GridId = Shader.PropertyToID("_PsxGrid"), OptionsId = Shader.PropertyToID("_PsxOptions");
        static readonly int SunDirId = Shader.PropertyToID("_PsxSunDir"), SunColourId = Shader.PropertyToID("_PsxSunColour");

        static readonly int FadeId = Shader.PropertyToID("_PsxFade"), ScreenId = Shader.PropertyToID("_PsxScreen");
        static readonly int SourceId = Shader.PropertyToID("_PsxSource"), RampId = Shader.PropertyToID("_PsxRamp");
        static readonly int LiftId = Shader.PropertyToID("_PsxLift");
        /// <summary>How many times brighter the ink look reads a deep night before it cuts it to tones.</summary>
        public float nightLift = 2.6f;

        public PsxStageLook(string styleId = "plain")
        {
            style = PsxStyles.Index(styleId);
        }

        // ------------------------------------------------------------------ what the frame, the key and the stage call

        /// <summary>The next look round (the player's key).</summary>
        public void Next() => SetStyle((style + 1) % PsxStyles.All.Length);

        public void SetStyle(string id) => SetStyle(PsxStyles.Index(id));

        public void SetStyle(int index)
        {
            style = Mathf.Clamp(index, 0, PsxStyles.All.Length - 1);
            if (stage != null)
                ApplyStyle();
            Changed?.Invoke(Style);
        }

        /// <summary>The size the picture is shown at, in screen pixels (the small window's, or the whole screen's when the
        /// ship and the chart have changed places). The stage's view tells it (ShipStageView.ShownAt); a test may.</summary>
        public void Show(int width, int height)
        {
            width = Mathf.Max(16, width);
            height = Mathf.Max(16, height);
            if (width == shownWidth && height == shownHeight)
                return;
            shownWidth = width;
            shownHeight = height;
            if (stage != null)
                ApplyStyle();
        }

        public void Apply(ShipStage onStage)
        {
            stage = onStage;
            kit = stage.kit;
            kitSea = stage.SeaKind;
            baseFar = stage.View.Camera.farClipPlane;
            modelShader = kit.psxModel != null ? kit.psxModel : Shader.Find("Kapman/ShipView/PSX Model");
            spriteShader = kit.psxSprite != null ? kit.psxSprite : Shader.Find("Kapman/ShipView/PSX Sprite");
            var seaShader = kit.psxSea != null ? kit.psxSea : Shader.Find("Kapman/ShipView/PSX Sea");
            var screenShader = kit.psxScreen != null ? kit.psxScreen : Shader.Find("Hidden/Kapman/ShipView/PSX Screen");
            if (screenShader != null)
                screenMaterial = new Material(screenShader) { name = "Ship view screen", hideFlags = HideFlags.DontSave };
            grid = new PsxSeaGrid { tile = kit.psxSeaTile };
            grid.Build(stage.transform, kit.layer, seaShader);
            if (stage.View.ShownWidth > 0 && stage.View.ShownHeight > 0)
            {
                shownWidth = stage.View.ShownWidth;
                shownHeight = stage.View.ShownHeight;
            }
            stage.View.ShownAtChanged += Show;
            stage.Changed += Recollect;
            ApplyStyle();
        }

        public void Remove(ShipStage fromStage)
        {
            if (stage == null)
                return;
            fromStage.View.ShownAtChanged -= Show;
            fromStage.Changed -= Recollect;
            int was = style;
            style = PsxStyles.Index("plain");
            if (fromStage.View != null && fromStage.View.Camera != null && fromStage.Water != null && fromStage.Water.Object != null)
                ApplyStyle();                                             // (HDRP's materials, the stage's sea and its camera as they were)
            style = was;
            grid?.Destroy();
            grid = null;
            foreach (var m in back.Keys)
                UnityEngine.Object.Destroy(m);
            twins.Clear();
            twinsSkinned.Clear();
            back.Clear();
            Release(ref stageRT);
            Release(ref screenRT);
            CoreUtils.Destroy(screenMaterial);
            CoreUtils.Destroy(rampTex);
            CoreUtils.Destroy(grainTex);
            cmd?.Release();
            cmd = null;
            stage = null;
            applied = false;
        }

        /// <summary>The next step draws a picture whatever the look's pace (a test's picture, a loaded game).</summary>
        public void Settle()
        {
            hold = 999f;
            grid?.Settle();
        }

        // THE WATER BEYOND THE GRID: the console's sea ends at its draw distance, and past it the picture shows HDRP's sky,
        // which under the horizon draws its planet's ground in the stage's own pale tint: it reads as haze. (Tried on
        // 7 Oct: giving that ground the sea's colour, for a plain far sea out to the horizon. HDRP lights its whole sky by
        // that ground, so the sky turned the river's olive. Left alone.)

        /// <summary>After the stage's contents change (the jetty comes, a man is seated): their materials too.</summary>
        void Recollect()
        {
            if (applied)
                SwapMaterials(Style.shade != PsxShade.Hdrp);
        }

        // ------------------------------------------------------------------ applying a look

        void ApplyStyle()
        {
            var s = Style;
            var view = stage.View;
            bool console = s.shade != PsxShade.Hdrp && modelShader != null;
            bool gridSea = s.sea == PsxSea.Grid && grid != null && grid.Material != null;
            if ((s.shade != PsxShade.Hdrp && modelShader == null || s.sea == PsxSea.Grid && !gridSea) && !warned)
            {
                Debug.LogWarning("Kapman ship view: a shader of the PSX looks is missing (Kapman > Ship view > Make the kit); those looks keep HDRP's materials or sea.");
                warned = true;
            }

            // the picture: so many lines, each pixel a whole number of screen pixels
            if (s.lines > 0)
            {
                int k = Mathf.Max(Mathf.Max(1, s.minPixel), Mathf.RoundToInt(shownHeight / (float)s.lines));
                int w = Mathf.Max(16, Mathf.CeilToInt(shownWidth / (float)k)), h = Mathf.Max(16, Mathf.CeilToInt(shownHeight / (float)k));
                PixelSize = k;
                processed = (s.levels < 256 || s.tones >= 2) && screenMaterial != null;
                Make(ref stageRT, w, h, 24, "Ship view (low picture)");
                if (processed)
                    Make(ref screenRT, w, h, 0, "Ship view (console screen)");
                else
                    Release(ref screenRT);
                view.drawnByLook = true;
                view.shown = Output;
                Shader.SetGlobalVector(GridId, s.snap > 0f && console ? new Vector4(w / s.snap, h / s.snap, 0f, 0f) : Vector4.zero);
                if (processed)
                {
                    int tones = s.tones >= 2 ? Mathf.Clamp(s.tones, 2, 16) : 0;
                    screenMaterial.SetVector(ScreenId, new Vector4(Mathf.Clamp(s.levels, 2, 256), s.dither, tones,
                                                                   QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : 0f));
                    screenMaterial.SetTexture(SourceId, stageRT);
                    if (tones > 0)
                        screenMaterial.SetTexture(RampId, Ramp(tones));
                }
            }
            else
            {
                PixelSize = 1;
                processed = false;
                Release(ref stageRT);
                Release(ref screenRT);
                view.drawnByLook = false;
                view.shown = null;
                Shader.SetGlobalVector(GridId, Vector4.zero);
            }
            Shader.SetGlobalVector(OptionsId, new Vector4(s.affine, s.shade == PsxShade.Flat ? 1f : 0f, s.grain, s.seaTile));
            // (the title looks down on its sea: nothing of it is far enough to be let go)
            bool far = console && s.drawDistance > 0f && !(stage != null && stage.TitleSea);
            Shader.SetGlobalVector(FadeId, far ? new Vector4(s.drawDistance * s.fadeStart, s.drawDistance * Mathf.Max(s.fadeEnd, s.fadeStart + 0.01f), 1f, 0f) : Vector4.zero);
            view.Camera.farClipPlane = far ? s.drawDistance : baseFar;

            // the camera: HDRP's water only while it is the sea drawn; no shadow maps for shaders that take no shadow
            var data = view.Data;
            foreach (var f in new[] { FrameSettingsField.Water, FrameSettingsField.WaterExclusion, FrameSettingsField.WaterDecals })
                data.renderingPathCustomFrameSettings.SetEnabled(f, !gridSea);
            data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.ShadowMaps] = console;
            if (console)
                data.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, false);

            // the sea, and what she floats on
            if (gridSea)
            {
                grid.cell = Mathf.Max(0.5f, s.seaCell);
                grid.facetBob = s.seaBob;
                stage.SeaKind = StageSeaKind.Mesh;                        // (HDRP's water, its lid and its foam off)
                stage.Water.Object.SetActive(false);                      // (and the stage's own mesh sea: the grid is drawn in its place)
                stage.Water.moveOnCpu = false;
                stage.WakeRibbonOff = true;                               // (the grid paints her wake into its own triangles)
                var sea = stage.Sea;
                var g = grid;
                stage.HeightAt = (east, north) => g.Height(sea, east, north);
            }
            else
            {
                stage.HeightAt = null;
                stage.Water.moveOnCpu = true;
                stage.WakeRibbonOff = false;
                stage.SeaKind = kitSea;
            }
            if (grid != null && grid.Object != null && grid.Object.activeSelf != gridSea)
                grid.Object.SetActive(gridSea);
            gridOn = gridSea;

            SwapMaterials(console);
            hold = 999f;
            applied = true;
        }

        static void Make(ref RenderTexture rt, int w, int h, int depth, string name)
        {
            if (rt != null && rt.width == w && rt.height == h)
                return;
            Release(ref rt);
            rt = new RenderTexture(w, h, depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                name = name, antiAliasing = 1, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, useMipMap = false,
                hideFlags = HideFlags.HideAndDontSave,
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

        /// <summary>The ink look's ramp as a texture of `tones` texels, dark to light: the frame's plum, its edge (the
        /// palette's sea), its accent, paper.</summary>
        // ramps that are not the frame's four: dark to light, each holding Byzantium (#702963)
        static readonly string[] Ramp2 = { "#702963", "#f1e8d6" };
        static readonly string[] Ramp8 = { "#0b0810", "#2e1233", "#4a1b47", "#702963", "#a8619b", "#c77fb0", "#e6c9a8", "#f1e8d6" };
        static readonly string[] Ramp16 =
        {
            "#0b0810", "#1d0a1a", "#2e1233", "#3b1540", "#4a1b47", "#5a2256", "#702963", "#86407b",
            "#9c5990", "#a8619b", "#b9789f", "#c77fb0", "#d9a3b8", "#e6c9a8", "#f0d27a", "#f1e8d6",
        };

        Texture2D Ramp(int tones)
        {
            string[] set = tones == 2 ? Ramp2 : tones == 8 ? Ramp8 : tones == 16 ? Ramp16 : null;
            Color[] from =
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
                if (set != null)
                {
                    ColorUtility.TryParseHtmlString(set[i], out Color own);
                    rampTex.SetPixel(i, 0, own);
                    continue;
                }
                float at = tones > 1 ? i / (float)(tones - 1) * (from.Length - 1) : 0f;
                int a = Mathf.Min((int)at, from.Length - 1), b = Mathf.Min(a + 1, from.Length - 1);
                rampTex.SetPixel(i, 0, Color.Lerp(from[a], from[b], at - a));
            }
            rampTex.Apply(false, false);
            return rampTex;
        }

        /// <summary>Until the user draws one: a 32 x 32 grey tile of two planks with a seam between them and a few
        /// strokes of grain along them (it tiles; it is read with hard texels).</summary>
        Texture2D Grain()
        {
            if (kit.psxGrain != null)
                return kit.psxGrain;
            if (grainTex != null)
                return grainTex;
            const int n = 32;
            grainTex = new Texture2D(n, n, TextureFormat.RGBA32, false, true)
            {
                name = "PSX grain (stand-in)", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave,
            };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int v = y % 16 == 0 ? 84 : y % 16 == 15 ? 142 : 128;      // a seam's shadow, and the light on the plank's lower edge
                    px[y * n + x] = new Color32((byte)v, (byte)v, (byte)v, 255);
                }
            uint seed = 1066u;
            for (int s = 0; s < 14; s++)
            {
                seed = seed * 1664525u + 1013904223u;
                int x = (int)((seed >> 8) % n), y = (int)((seed >> 16) % n), len = 5 + (int)((seed >> 24) % 9);
                if (y % 16 == 0 || y % 16 == 15)
                    continue;
                byte v = (byte)(s % 2 == 0 ? 112 : 140);
                for (int k = 0; k < len; k++)
                    px[y * n + (x + k) % n] = new Color32(v, v, v, 255);
            }
            grainTex.SetPixels32(px);
            grainTex.Apply(false, false);
            return grainTex;
        }

        // ------------------------------------------------------------------ the materials

        /// <summary>The console's twin of one of the stage's HDRP materials (made once), or null for one that is left alone.</summary>
        Material Twin(Renderer r, Material m)
        {
            if (m == null || m.shader == null || back.ContainsKey(m))
                return null;
            bool skinned = r is SkinnedMeshRenderer;
            var map = skinned ? twinsSkinned : twins;
            if (map.TryGetValue(m, out var t))
                return t;
            if (m.shader.name != "HDRP/Lit" && m.shader.name != "HDRP/Unlit")
                return null;
            bool seeThrough = m.HasProperty("_SurfaceType") && m.GetFloat("_SurfaceType") > 0.5f;
            Color tint = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.HasProperty("_UnlitColor") ? m.GetColor("_UnlitColor") : Color.white;
            if (seeThrough)
            {
                // rain: flat streaks in the console's style. Her wake's ribbon is left as it is (it is hidden in these looks)
                if (m != kit.rain || spriteShader == null)
                    return null;
                t = new Material(spriteShader) { name = m.name + " (console)", hideFlags = HideFlags.DontSave };
                // (stronger than HDRP's: a streak is a pixel wide in the low picture, and there is no half pixel)
                t.SetColor("_Color", new Color(1f, 1f, 1f, 1.4f));
            }
            else
            {
                t = new Material(modelShader) { name = m.name + " (console)", hideFlags = HideFlags.DontSave };
                if (m.HasProperty("_BaseColorMap") && m.GetTexture("_BaseColorMap") != null)
                    t.SetTexture("_BaseMap", m.GetTexture("_BaseColorMap"));
                t.SetColor("_BaseColor", tint);
                bool bothSides = m.HasProperty("_DoubleSidedEnable") && m.GetFloat("_DoubleSidedEnable") > 0.5f;
                t.SetFloat("_Cull", bothSides ? (float)CullMode.Off : (float)CullMode.Back);
                // the grain only where a thing keeps its shape: on a man it would swim over him as he moves
                if (!skinned)
                {
                    t.SetTexture("_DetailMap", Grain());
                    t.SetFloat("_DetailScale", grainScale);
                    t.SetFloat("_DetailAmount", 1f);
                }
            }
            map[m] = t;
            back[t] = m;
            return t;
        }

        void SwapMaterials(bool console)
        {
            if (stage == null)
                return;
            stage.Remat((r, m) =>
            {
                if (m == null || (stage.Water != null && r == stage.Water.Renderer))
                    return null;
                if (console)
                    return Twin(r, m);
                return back.TryGetValue(m, out var original) ? original : null;
            });
        }

        // ------------------------------------------------------------------ every step

        /// <summary>The stage has stepped: draw its picture in this look.</summary>
        public void Draw(ShipStage onStage, float dt)
        {
            var s = Style;
            if (s.lines <= 0 || stageRT == null)
            {
                onStage.View.Render();                                    // ("None": the stage's own picture, at its own pace)
                return;
            }
            // pictures held back: one is drawn only when it is due (the texture keeps the last one between)
            bool now = hold > 900f;                                       // (settled, or a look just put on: a picture at once)
            if (s.framesPerSecond > 0)
            {
                float every = 1f / s.framesPerSecond;
                hold += dt;
                if (!now && hold < every * 0.98f)
                    return;
                hold = hold > 4f * every ? 0f : hold - every;
            }
            else if (onStage.View.rate > 0f)
            {
                // (no pace of its own: the view's, so many pictures a real second at most, as the stage's own picture)
                float every = 1f / onStage.View.rate;
                hold += dt;
                if (!now && hold < every * 0.98f)
                    return;
                hold = hold > 4f * every ? 0f : hold - every;
            }
            else
            {
                // (every frame when she fills the screen; every other in the small window, as the stage's own picture)
                hold = 0f;
                if (!now && tick++ % Mathf.Max(1, onStage.View.every) != 0)
                    return;
            }
            Light(onStage);
            if (grid != null && grid.Object != null)
            {
                // (in the showroom there is no sea of any kind: the boat hangs over nothing)
                bool seaShown = gridOn && !onStage.Showroom;
                if (grid.Object.activeSelf != seaShown)
                    grid.Object.SetActive(seaShown);
            }
            if (gridOn && !onStage.Showroom && grid != null && grid.Object != null)
            {
                grid.Rebuild(onStage, dt);
            }
            onStage.View.RenderTo(stageRT);
            if (processed && screenRT != null)
            {
                cmd ??= new CommandBuffer { name = "Ship view console screen" };
                cmd.Clear();
                cmd.SetRenderTarget(screenRT);
                cmd.DrawProcedural(Matrix4x4.identity, screenMaterial, 0, MeshTopology.Triangles, 3);
                Graphics.ExecuteCommandBuffer(cmd);
            }
        }

        /// <summary>
        /// The Sun's light for the console shaders, exposed: on a surface square to it (the lighting model's, through the
        /// cloud the stage's own Sun is dimmed by), or the Moon's by night: lux over pi (what a white matte surface sends
        /// back) times the stage's exposure. The sky's light is not set here: the shaders read HDRP's own (PsxCommon.hlsl).
        /// </summary>
        void Light(ShipStage onStage)
        {
            var st = onStage.State;
            var sky = onStage.SkyNow;
            float exposure = (float)LightingModel.ExposureFromEV100(onStage.Ev);
            float through = st.cloudLight > 0f ? st.cloudLight : 1f;
            float sunLux = (float)(LightingModel.SunNormalLux(sky.sunAltitude) * LightingModel.CloudDim(through) * (1.0 - sky.sunCovered));
            float moonLux = onStage.Moon != null && sky.moonAltitude > 0f ? onStage.Moon.intensity : 0f;
            Vector3 toward;
            Color light;
            if (sunLux >= moonLux)
            {
                toward = sky.toSun;
                light = Color.Lerp(new Color(1f, 0.5f, 0.24f), new Color(1f, 0.96f, 0.9f), Mathf.InverseLerp(0f, 25f, sky.sunAltitude)) * (sunLux / Mathf.PI * exposure);
            }
            else
            {
                toward = sky.toMoon;
                light = new Color(0.78f, 0.85f, 1f) * (moonLux / Mathf.PI * exposure);
            }
            if (onStage.Showroom && onStage.View != null && onStage.View.Camera != null)
            {
                // (the showroom: the lamp is held to the camera, up and to its left, and it is always a fair noon)
                var c = onStage.View.Camera.transform;
                toward = -c.forward + c.up * 0.75f - c.right * 0.55f;
            }
            if (onStage.TitleSea)
            {
                // (the title's sea, seen from straight above: the light is laid low across it, so each wave has a lit side
                // and a dark one. Under a high Sun every facet throws its glint straight up at the camera and the sea is
                // one white sheet: 7 Oct 2026, TitleSeaTests)
                // Behind the camera and well up: the letters, which lie face up, are lit; and the sea throws its glint away
                // from the camera, not at it.
                var c = onStage.View.Camera.transform;
                Vector3 flat = Vector3.ProjectOnPlane(c.forward, Vector3.up).normalized;
                toward = -flat * 0.6f + Vector3.up * 0.8f + c.right * 0.35f;
                // (and a good deal weaker than the Sun: full on the flat of the sea it burnt the colour out to a pale sheet
                // wherever the cloud let it through)
                light *= ShipStage.TitleSeaSun;
            }
            if (toward.sqrMagnitude < 1e-6f)
                toward = Vector3.up;
            // (as vectors, not colours: these are amounts of light, and a colour would be taken for a screen value and bent)
            Shader.SetGlobalVector(SunDirId, toward.normalized);
            Shader.SetGlobalVector(SunColourId, new Vector4(light.r, light.g, light.b, 1f));
            // the ink look cuts the picture to a few tones by brightness: a night would fall into the darkest one whole, so
            // its brightness is lifted first, the more the deeper the night (it stays darker than day)
            if (screenMaterial != null)
                screenMaterial.SetVector(LiftId, new Vector4(onStage.TitleSea ? ShipStage.TitleSeaLift : Mathf.Lerp(1f, nightLift, Mathf.InverseLerp(-2f, -12f, sky.sunAltitude)), 0f, 0f, 0f));
        }
    }
}
