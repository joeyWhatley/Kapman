# The ship window: the look (PSX style, five ways) and the rowing crew

7 October 2026. Written without Unity running (other agents had it), so **no frame of any of this has been seen**.
What could be checked without the editor was: the C# drafts compile with Unity's own compiler against this project's
assemblies, and the crew's motion was run on the pack's real skeleton outside Unity and drawn as stick figures
(`check/pictures`). The shaders are unverified. Section 9 says what to look at first in the editor.

The stage itself (water, sky, camera, RenderTexture, dock, the hook into the frame) is the other agent's. Section 7 says
where these parts plug into it.

## 0. What is in this folder

| File | Later path in the project | State |
|---|---|---|
| `draft/PsxStyle.cs` | `Assets/Kapman/Code/Game/ShipView/` | compiles; the five styles as data |
| `draft/ShipViewLook.cs` | same | compiles; the style switch (camera, picture, materials, sea, light, screen pass) |
| `draft/PsxSeaGrid.cs` | same | compiles; the 1997 sea as a CPU-moved grid |
| `draft/CrewBody.cs` | same | compiles; **run** on the real skeleton |
| `draft/RowingRig.cs` | same | compiles; **run** on the real skeleton |
| `draft/ShipCrew.cs` | same | compiles; the rule and the helmsman **run** |
| `draft/ShipCrewFeed.cs` | same | compiles against `ChartBody`, `ChartWorld`, `KapmanGame`, `Port`, `Travel`; not run |
| `draft/ShipViewJoin.cs` | same | compiles **together with the other half's drafts**; the two plugs of the stage (`PsxStageLook`, `PackCrewRig`); not run |
| `draft/Editor/ShipViewSeating.cs` | `Assets/Kapman/Editor/Game/` | compiles (assembly `Kapman.Editor`); needs the editor to run; only for the longship at her own size (section 9.5) |
| `draft/Shaders/PsxCommon.hlsl` | `Assets/Kapman/Shaders/ShipView/` | **unverified** |
| `draft/Shaders/PsxModel.shader` | same | **unverified** (ship, men, dock) |
| `draft/Shaders/PsxSea.shader` | same | **unverified** (the grid sea) |
| `draft/Shaders/PsxSprite.shader` | same | **unverified** (spray, rain, splashes, wake foam) |
| `draft/Shaders/PsxScreen.shader` | same | **unverified** (15-bit colour, dither, ink tones) |
| `check/` | not for the project | a stand-in for `UnityEngine` and a program that runs the crew's drafts; `check/pictures/*.png` |

How the C# was checked: `Library/Bee/artifacts/1900b0aE.dag/Kapman.Game.rsp` and `Kapman.Editor.rsp` copied to a temp
folder outside the project, paths made absolute, the drafts added, compiled with
`Editor/Data/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll` under `Editor/Data/NetCoreRuntime/dotnet.exe`: 0 errors, and
no warning from a draft. That proves every Unity and HDRP name used exists in this project's versions
(`FrameSettingsField.ObjectMotionVectors`, `HDAdditionalCameraData.renderingPathCustomFrameSettings`,
`RenderPipelineManager.endContextRendering`, `AssetLibrary.NewLit`, `Port.Nearest` ...). It proves nothing about how
it looks.

The same folder holds the other half's drafts (`ShipStage*.cs`, `Stage*.cs`, `ChartStageSource.cs`; its paper is
`PLAN.md`). All of this half's game files, the join and the other half's eleven game drafts as they stood at 11:10
were compiled as one `Kapman.Game`: no error, no name in both. Section 7 is the join.

To run the crew check again after changing a draft: `dotnet run --project Design/ship_view/check/rowcheck.csproj -- Design/ship_view/check/pictures`
(needs the .NET SDK on the machine: 10.0.401 is there). It ends with `ALL CHECKS PASSED` or says which failed.

---

# PART 1. PSX STYLE, IN A FEW DIFFERENT WAYS

## 1. What made that look, each as its own switch

Every one of these is a number in `PsxStyle` (the style's data), read by `ShipViewLook` and the shaders. None is a
shader keyword: one material serves every style and nothing recompiles when the user flips.

| # | The console (1994 to 2000) | Here | Switch (`PsxStyle`) |
|---|---|---|---|
| 1 | A low picture, 320 x 240 or so, each pixel hard | The stage camera draws into a low `RenderTexture` read with `FilterMode.Point` | `lines`, `minPixel` |
| 2 | No sub-pixel precision: vertices land on whole pixels, so edges crawl and models shiver | `PsxClip` in `PsxCommon.hlsl` rounds clip x, y to a grid | `snap` (0 off, 1 whole pixels, 2 every other) |
| 3 | No perspective-correct texturing: textures swim across big triangles | `noperspective` interpolators in the hand-written shaders | `affine` (0 to 1) |
| 4 | Lighting worked out per vertex (Gouraud) | `PsxLight` in the vertex shader, from the game's own light numbers | `shade = Vertex` |
| 5 | Or one shade to a triangle (flat) | The triangle's own normal from `ddx`, `ddy` of the position | `shade = Flat` |
| 6 | 15-bit colour, with the video chip's 4 x 4 ordered dither | `PsxScreen.shader`: the console's own table, then 32 levels a channel | `levels`, `dither` |
| 7 | Small textures, no filtering, no mip-maps | `SAMPLE_TEXTURE2D_LOD(..., s_point_clamp_sampler, uv, 0)` | always, in the console shaders |
| 8 | A short draw distance hidden by fog | Far clip pulled in; fog by distance per vertex toward one colour | `drawDistance`, `fogStart`, `fogEnd`, `fogMost` |
| 9 | No depth buffer: polygons sorted whole, near ones fight | Only hinted: depth rounded to steps so near surfaces flicker | `depthSteps` (0 in every style as shipped) |
| 10 | 30 pictures a second or fewer | The stage camera is enabled only on the frames a picture is due | `framesPerSecond` |

Number 9 is the one thing not really done. A true ordering table means no depth test and triangles sorted on the CPU;
that is a lot of work for a flaw. `depthSteps = 4096` gives steps of about 8 cm at 10 m (reversed depth, near plane
0.3 m), which makes the shields flicker against the hull. It is off everywhere; try it on the honest console if the user
wants more wrongness.

## 2. How each is really done in HDRP 17.6, and what it costs

### 2.1 The low picture

`ShipViewLook.Apply()`: `stageCamera.targetTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32,
RenderTextureReadWrite.sRGB)` with `filterMode = FilterMode.Point` (the same call the project's tests use, for
instance `Tests/PlayMode/ChartGameTests.cs:101`). HDRP writes its finished, tone-mapped picture into it.

The size is not simply 320 x 240. Hard pixels only look right when each picture pixel is a **whole number** of screen
pixels, and the ship window is small: `Frame.uss` gives `.frame-ship` 24% of the width and `.frame-ship-view` 262 px
of height, so about 443 x 256 inside its border on a 1920 screen. 240 lines in 256 px would be a ragged 1.07 times.
So: `k = max(minPixel, round(shown height / lines))`, picture = shown size / k.

| Shown at | `lines` 240, `minPixel` 2 | `lines` 360, `minPixel` 2 |
|---|---|---|
| the small window, 443 x 256 | k = 2: **222 x 128** | k = 2: 222 x 128 |
| the whole screen, 1920 x 1080 (ship and chart swapped) | k = 4: **480 x 270** | k = 3: 640 x 360 |

The frame must call `look.Show(width, height)` with the real pixel size of the element (after the panel's scaling) and
show `look.Output` with no filtering at `look.PixelSize` screen pixels to a picture pixel (up to k - 1 px may hang over
the edge; crop it).

In the small window a man is about 20 pixels tall. That is the size at which simple motion reads best.

**Cost.** The pixels cost nothing. What costs is that this is a **second HDRP camera**: HDRP's work per camera
(culling, its render graph, shadow maps, sky, fog, the water's own passes) is mostly the same at 222 x 128 as at
1080p. I could not measure it. What the drafts do about it: per-camera frame settings (the way
`Editor/DorestadVegetation.cs:692` already does) switch off SSR, SSAO, contact shadows, volumetric fog, motion
vectors and all lens effects in every style but the plain one, HDRP's water when the grid sea is drawn, and
`framesPerSecond = 30` halves what is left. The chart's own camera draws nothing (`cullingMask = 0` in
`ChartBuilder.cs`), which helps. The stage's own view already draws by render request every second frame with most
of HDRP switched off (`ShipStageView`, PLAN.md part 2); while a look of this half is on, the look draws the camera
instead (`ShipStageView.drawnByLook`) and the two must not both run.

Anti-aliasing must be off on the stage camera in every style but the plain one
(`HDAdditionalCameraData.antialiasing = None`, `allowDynamicResolution = false`): TAA and DLSS smear hard pixels and
undo the snapping.

### 2.2 Colour depth and dither: three ways, one chosen

**Chosen: a plain blit, outside HDRP.** `ShipViewLook.AfterRendering` (on
`RenderPipelineManager.endContextRendering`) runs `Graphics.Blit(stageRT, screenRT, screenMaterial)` with
`PsxScreen.shader`. The source is the finished 8-bit picture, which is exactly where the console's dither sat (it was
applied as a pixel was written to the 15-bit frame buffer). Why this and not HDRP's own hooks:

- it can only ever touch this one picture: the frame and the chart are other pictures, so "limited to the stage's
  camera" needs no mechanism at all;
- nothing to register, no volume, no injection point, no render-graph rules;
- this project has already been bitten by a full-screen custom pass that reads the colour buffer:
  `Code/Runtime/Levels/Hdrp/SnowFrostPass.cs` has its ground pass switched off since 4 Oct because "the opaque image
  came back black at BeforePreRefraction".

The shader does the work in screen (sRGB) values, converts back for the sRGB target, and has two modes: `levels` a
channel with the console's table (-4, 0, -3, 1 / 2, -2, 3, -1 / -3, 1, -4, 0 / 3, -1, 2, -2, in eighths of a level), or
`tones` of one ramp by brightness (the ink style).

Unverified points: that `Graphics.Blit` with a `CGPROGRAM` / `UnityCG.cginc` shader runs as usual under HDRP (it draws
its own quad, so it should); and whether HDRP has submitted the stage camera's commands when `endContextRendering`
fires. If it has not, the blit simply shows the picture one frame late, which nobody will see.

**If the blit fails, the two HDRP ways** (both read in the package, neither drafted):

- A `CustomPassVolume` with a `FullScreenCustomPass`, `injectionPoint = CustomPassInjectionPoint.AfterPostProcess`,
  `fetchColorBuffer = true`, and **`volume.targetCamera = stageCamera`**
  (`Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassVolume.cs:64`: "force the custom pass volume to be executed
  only for one camera"). At that point `CustomPassLoadCameraColor` reads `_AfterPostProcessColorBuffer`
  (`CustomPassSampling.hlsl:38`), the tone-mapped picture. Made at runtime as `SnowFrostPass` makes its volume.
- A `CustomPostProcessVolumeComponent` at `CustomPostProcessInjectionPoint.AfterPostProcess`. It must be added to the
  list in `Assets/HDRPDefaultResources/HDRenderPipelineGlobalSettings.asset`
  (`CustomPostProcessOrdersSettings.afterPostProcessCustomPostProcesses`), and it is limited to a camera only through
  a volume on a layer that only the stage camera's `volumeLayerMask` sees. More to set up, more to go wrong.

### 2.3 Vertex snapping and affine texturing

In a hand-written shader both are three lines (`PsxClip`, and `noperspective float2 uvAffine`). What HDRP makes hard is
not the trick but **getting a hand-written shader drawn rightly at all**:

1. HDRP makes the depth its fog and its water read from a **depth prepass**. An opaque thing that only writes depth
   in its colour pass is missing from it, and is then fogged as if it were the horizon behind it. So each console
   shader has two passes: `"LightMode" = "DepthForwardOnly"` (depth only, `ColorMask 0`, because HDRP binds its normal
   buffer there: `HDRenderPipeline.Prepass.cs:613`) and `"LightMode" = "ForwardOnly"` (the colour). Both are names HDRP
   draws (`HDRenderPipeline.cs:176` and `:194`). Both use the same vertex function, so the snapped vertex is the same
   in both; the colour pass still tests `LEqual`, not `Equal`.
2. With object motion vectors on, HDRP **leaves moving and skinned things out of that prepass**
   (`HDRenderPipeline.Prepass.cs:566`, `excludeObjectMotionVectors: objectMotionEnabled`) and expects their
   `MotionVectors` pass to write the depth. The console shaders have none. So `ShipViewLook` switches
   `FrameSettingsField.ObjectMotionVectors` off on the stage camera in the console styles. Nothing there needs them
   (no TAA, no motion blur).
3. The stencil: copied from HDRP's own `Unlit.shader` (depth pass `WriteMask 8, Ref 0`; colour pass
   `WriteMask 3, Ref 0`), so HDRP's deferred lighting and subsurface passes leave these pixels alone.

**Shader Graph instead?** It can do both (snap: a Custom Function in the vertex stage that goes to clip space, rounds,
and comes back through `UNITY_MATRIX_I_VP`; affine: two custom interpolators carrying `uv * w` and `w`, divided in the
fragment stage), and HDRP would then generate every pass itself. But a `.shadergraph` is a large JSON of nodes and
GUIDs that cannot be written or checked without the editor, and it is built by hand in a window. The hand-written
files are 130 lines each and can be read. If they fight HDRP, Shader Graph (HDRP Unlit target) is the fallback.

**Affine texturing barely shows on the pack's models.** See 6: their faces each sit inside one flat swatch of a
palette, so there is no picture on the triangle to swim. It shows on the sea's tile, and on anything given the
optional grain tile (`_DetailMap` on `PsxModel.shader`: a 32 x 32 tile the user draws, laid flat on each face, off by
default). The shiver from snapping is what carries the console's feel on these models.

### 2.4 Per-vertex lighting

`PsxLight(n)` = light from above and below (a hemisphere) + one directional light, times the texture. The numbers come
from the game, **not** from HDRP's light loop: `ShipViewLook.LateUpdate` reads `LevelClock.SunDirection`,
`LevelClock.SunColour` (the clock's relative value, about 2 for the full sun), `LevelClock.MoonDirection` and
`MoonLightLux`, `LevelClock.SkyZenith` and `SkyHorizon`, and sets `_PsxSunDir`, `_PsxSunColour`, `_PsxSkyColour`,
`_PsxGroundColour`, `_PsxFogColour`. `L_Chart` has a `LevelClock` (`ChartBuilder.cs:42`), so these are live there.
On the other half's stage the join does otherwise: in the chart scene the clock's statics are the sky of 52 N 5 E, not
of where she is (PLAN.md, part 2), so `PsxStageLook` switches `lightFromClock` off and fills the look's light fields
from the stage's own sky each frame (section 7).

The colour written is the final colour times `white`; **`GetCurrentExposureMultiplier()` is not applied**. So the
ship is as bright as those numbers say, whatever HDRP's automatic exposure does. That is on purpose: a wrong unit
cannot black her out or blow her out. What follows from it:

- by day and by night her brightness is set by `sunScale`, `skyScale`, `groundScale` on `ShipViewLook` and by
  `PsxStyle.white`, by eye;
- HDRP's sky behind her **does** follow HDRP's exposure. They must be matched by eye at noon, at dusk and at night;
- HDRP's tone mapping (ACES in this project, `Editor/ProjectSetup.cs:262`) still runs over her and shifts the
  palette's colours a little. If that hurts, the stage's volume can use Neutral.

Reading HDRP's own sun in the shader (`_DirectionalLightDatas`, the ambient probe) would match the sky by
construction, but needs HDRP's light-loop includes in a hand-written pass. Not worth the risk before a first frame.

### 2.5 What HDRP makes hard, in one list

1. An opaque hand-written shader needs the depth prepass pass, or fog and water treat it as sky (2.3).
2. Moving and skinned things drop out of that prepass while object motion vectors are on (2.3).
3. The colour buffer is HDR, pre-exposed and tone-mapped later: a "plain colour" is not plain (2.4).
4. TAA, DLSS and dynamic resolution undo hard pixels and snapping: off on this camera.
5. A second HDRP camera costs CPU whatever its size (2.1).
6. Shader Graph cannot be written blind (2.3).
7. There is no way to draw without a depth buffer (1, number 9).
8. The pack's materials are URP's and draw pink (6).
9. HDRP's fog is laid over the console shaders' pixels too (they are in the depth buffer), on top of their own fog.
   That is wanted for the weather; if a style ends up too foggy, set its `fogMost` to 0 and let HDRP's fog do it all.

## 3. The sea, three ways

**A. The grid sea** (`PsxSeaGrid.cs` + `PsxSea.shader`; styles console, lowpoly, ink).
49 x 49 vertices at 2.5 or 3 m (120 to 144 m across, 4,608 triangles), plus one ring of long triangles thrown flat to
3 km. Each frame, on the CPU, every vertex is lifted by up to five sine waves made from the game's own
`Kapman.Weather.SeaState` (`SetSea`: the wind sea as three waves spread about its bearing, the swell as two, the
amplitudes shared so each train's significant height comes out right; waves shorter than three cells are left out).
About 24,000 sines a frame: well under half a millisecond. Each vertex is coloured: `deep` in the troughs, `crest` on
the tops, `foam` where it breaks (`SeaState.whitecaps`) and in the wake. Over that the shader lays one small tile
twice, hard-texelled and affine, the two sliding over each other, and a hard glint of the sun on the facets.

- **The ship must float on the same waves**: `PsxSeaGrid.Height(x, z)`. The stage's own sea mesh is switched off in
  these styles, and the join (`PsxStageLook`) sets `stage.HeightAt`, so the boat, the camera and the wake ride the
  grid's waves while it is drawn and the stage's own again when it is not.
- The treadmill: add her way made good to `gridSea.flow` each frame. Waves, tile and the grid itself then slide past;
  the vertices stay fixed to the water and re-centre a whole cell at a time, so nothing swims.
- Under the hull the surface is pressed to her own waterline plane (an ellipse of 8.3 x 2.5 m with a 2.5 m soft
  edge), so no wave comes up through the bottom boards. A cheap trick of the period.
- The tile: `PsxSeaGrid.tile`, 32 x 32 or 64 x 64, grey. A plain stand-in of short strokes is generated in code until
  **the user draws one**.

**B. The stage's own sea seen through the console's screen** (style screen). The other half's stage has **no HDRP
`WaterSurface`**: it cannot stand in the chart scene without the chart game taking the chart for a 3D level (PLAN.md,
part 1). Its sea is a mesh of its own, moved on the CPU by `StageSea`'s eight waves, under an HDRP/Lit material
(`StageWater`). That is "the modern sea" here: **wherever this paper or the code says HDRP's water (`PsxSea.Hdrp`,
`ShipViewLook.hdrpSea`), on this stage it means that mesh.** Shown at 240 lines in 15-bit colour with the dither.
Nothing new to build. Without TAA its highlights will alias at this size; the dither hides some of it.

**C. A flat sea** (a variant of A, no new code): `SetSea(0, ...)` gives a still plane with the two tiles sliding and
the ship bobbing on her own. It is the cheapest and the most 1996. On a river the game's own sea is 5 cm high and a
metre long (PLAN.md, part 2): the grid leaves out any wave shorter than three cells, so in the grid styles a river
*is* this flat sea with its tiles sliding, without anything being done.

| | Grid sea (A, C) | The stage's sea (B) |
|---|---|---|
| Spray at the bow | 10 to 30 `PsxSprite` cards (cut-out, 8 x 8 texels) from a `ParticleSystem` when she pitches in | the foam in the mesh's vertex colours (`StageWater`); particles with an HDRP/Unlit material |
| Oar splashes | `RowingRig.BladeIn` / `BladeOut` give the blade's place: one `PsxSprite` puff each | the same events, HDRP particles |
| Rain | 60 to 150 stretched `PsxSprite` streaks in a box round the camera (`_Dst = One`: the console's bright blend) | the stage's (PLAN.md) |
| Fog | the console shaders' distance fog toward `_PsxFogColour`, closing at `visibility` (from the stage's record) and at the style's `drawDistance`; HDRP's fog on top | HDRP's fog |
| Wake | white in the grid's vertex colours astern (`PsxSeaGrid.wake`): a widening band of pale triangles | the stage's own wake (`kit.wake`) |

The spray, rain and splash emitters are the stage's to place; `PsxSprite.shader` is their material.

**The first look risk in the grid styles is the horizon.** The far sea ends in the console's fog colour
(`LevelClock.SkyHorizon`), and behind it is HDRP's real sky under HDRP's exposure. If the two do not meet there is a
line. Remedies, in order: tune `fogColour` / the scales by eye; set the style's `fogMost` to 0 and its
`drawDistance` to 3000 so HDRP's own fog takes the far sea into its own sky; or give the console styles their own
sky, a dome shaded from `SkyZenith` to `SkyHorizon` in the same shader family (the real 1997 answer, exposure-proof,
and it would let HDRP's sky, clouds and fog be switched off for those styles; not drafted, and it would lose the
aurora and the clouds there).

## 4. The five styles

`PsxStyles.All` in `PsxStyle.cs`. `ShipViewLook.Next()` goes round them; `SetStyle(PsxStyles.Index("ink"))` picks one.

| | console | lowpoly | screen | ink | plain |
|---|---|---|---|---|---|
| name shown | The honest console | The clean low-poly | A modern sea behind a console screen | The dithered ink | None |
| `lines` | 240 | 360 | 240 | 240 | the window's own |
| small window / whole screen | 222 x 128 / 480 x 270 | 222 x 128 / 640 x 360 | 222 x 128 / 480 x 270 | 222 x 128 / 480 x 270 | 443 x 256 / 1920 x 1080 |
| pictures a second | 30 | every frame | every frame | 30 | every frame |
| colour | 15-bit, console dither | full | 15-bit, console dither | 4 tones of the frame's colours, heavy dither | full |
| ship and men | per-vertex light | one shade a triangle | HDRP lit | one shade a triangle | HDRP lit |
| vertex snap | whole pixels | off | off | whole pixels | off |
| affine | on | off | off | off | off |
| sea | grid, 3 m cells | grid, 2.5 m cells | the stage's own | grid, 3 m cells | the stage's own |
| own fog / draw distance | to 400 m | to 800 m | none (HDRP's) | to 500 m | none (HDRP's) |
| HDRP lens effects, TAA | off | off | off | off | on |

**1. The honest console.** Everything on. *The user will see:* a chunky picture, the ship and the men shivering
slightly as she rolls (vertices jumping from pixel to pixel), smooth bands of light across the hull broken into the
console's cross-hatch, a sea of big moving triangles with a sliding hard-texelled tile that warps near the camera,
the fog closing in at 400 m, motion at 30 pictures a second. *Cost:* the lowest of the three console styles (half the
frames; no HDRP water, SSR, SSAO or volumetrics). *Risk:* at 128 lines a one-pixel snap is two screen pixels: the
men's faces may swim too much in the small window (set `snap` to 0.5 there, or 0); the horizon line (3); brightness
against the sky.

**2. The clean low-poly.** Hard pixels, flat facets, nothing wobbles, full colour. *The user will see:* the same
models as cut paper: every triangle one clean shade, a faceted sea whose facets catch the sun one by one. It reads as
drawn, not as a console. Of the five it is the least likely to "read like made by AI". *Cost:* as the console at
every frame. *Risk:* the pack's hull was modelled for smooth shading, so its facets may look rough along the planks;
the flat normal's sign (it is turned to face the eye, so a wrong sign cannot show, but look).

**3. A modern sea behind a console screen.** HDRP does everything; only the screen is old. *The user will see:*
today's water, sky and lit ship at 240 lines, banded into 15-bit colour with the cross-hatch: like a cut-scene on the
old machine. *Cost:* a full HDRP camera with lit materials and the sun's shadows (the dearest after plain). Everything for it exists. *Risk:*
shimmer on the water without TAA; dark scenes band heavily (that is the look, but it may be too much at night).

**4. The dithered ink.** Brightness cut to four tones of the frame's own colours (`FrameColours.Plum`, `EdgeNow`: the
palette's sea, `AccentNow`, `Paper`) through the same 4 x 4 pattern, over flat shading and the grid sea, with the
snap on. *The user will see:* the ship as a print in the frame's colours, moving. It ties the window to the chart and
the ink slips. Change the tones to 2 for pure ink on paper. *Cost:* as the console. *Risk:* night. The console
shaders go dark at night and everything falls into the darkest tone; raise `skyScale`, or give this style a floor.
Big plain shapes are what a dither draws well, which is why this style does not use HDRP's water.

**5. None.** HDRP as it is, at the window's own size, with TAA and the lens effects: to compare the others against,
and the fallback if the user tires of all of them. *Cost:* the dearest. *Risk:* none new.

Adding a sixth is one more entry in `PsxStyles.All`. Worth trying once a frame is up: the console with `tones = 0`,
`levels = 8` (a harsher machine); the low-poly with `dither = 0.5`, `levels = 32`.

## 5. Where each switch lives in the code

- `PsxStyle` fields, applied in `ShipViewLook.Apply()`: the picture's size, the camera's frame settings and far
  plane, the material swap, which sea, `_PsxGrid` (the snap grid: the picture's size over `snap`), `_PsxOptions`
  (affine, flat, depth steps, white), and `PsxScreen`'s `_PsxScreen` (levels, dither, tones).
- Every frame in `ShipViewLook.LateUpdate()`: the light and the fog's colour and range.
- Every frame in `ShipViewLook.Update()`: whether the stage camera draws this frame.

## 6. The pack's materials

Read from `Assets/Stylized_Medieval_Vikings_Pack/Materials/*.mat`:

- `SMV_BasePallete`, `SMV_Environment`, `SMV_Weapons` (and `Demo_Ground`) all use the shader with GUID
  `933532a4fcc9baf4fa0491de14d08ed7`, which is **URP's Lit**; each file carries URP's
  `UnityEditor.Rendering.Universal.AssetVersion` object and the keyword `_METALLICSPECGLOSSMAP`. URP is not in
  `Packages/manifest.json`. **They are not HDRP-ready and will draw pink**, as happened before in this project.
- What they hold: `_BaseMap` = `Textures/BasePallete.png` (**32 x 32**, point filtered, mip-maps on),
  `SMV_EnvironmentPallete.png` and `UniversalWeaponsColorsVariant.png` (**256 x 256**), each with a metallic map,
  smoothness 0.5 to 0.6.
- Which meshes use which: the ship and the men use `SMV_BasePallete`; barrels, planks and the pack's water use
  `SMV_Environment`; weapons, shields **and the oars** use `SMV_Weapons`.

What to do: **leave the pack's files alone** (the pack is not in git; a re-import would undo any change) and make our
own two sets in `Assets/Kapman/Models/ShipView/`. `ShipViewSeating.Materials()` does it: for each of the three,
`SV_<name>_Lit.mat` (through the project's own `AssetLibrary.NewLit(texture, null, 0.25f, white)`: HDRP/Lit with the
palette as base colour) and `SV_<name>_Psx.mat` (`Kapman/ShipView/PSX Model` with the same texture). It puts the Lit
ones on the prefab at once, so it is never pink, and `ShipViewLook` swaps Lit for Psx by style
(`ShipViewLook.swaps`; it finds the renderers under the stage's root once and swaps `sharedMaterials`).
**On the other half's stage this is already half done:** its kit builder makes the HDRP twins (`kit.swaps`) and the
stage puts them on everything it adopts; the join then makes the console twins from those at run time (section 7),
and `ShipViewSeating.Materials()` is only needed for the longship prefab of section 9.5. The other way,
if the pack is to be changed in place after all: `AssetLibrary.Upgrade(material)` already turns a URP material into
HDRP/Lit from its saved properties.

How each style treats them:

- console: `PsxModel.shader`, palette read at mip 0 with hard texels (the mip-maps would bleed neighbouring swatches
  together at a distance; the shader never uses them), light per vertex, snapped. The metallic map is dropped.
- lowpoly, ink: the same shader with the flat switch on.
- screen, plain: the HDRP/Lit copies. The metallic map is not carried over (URP packs it differently from HDRP's
  mask map); helmets lose their sheen. It can be repacked later if anyone misses it.

## 7. Where the look and the crew plug into the stage: the join

The other half's stage gives two plugs (`ShipStageState.cs`): `IShipStageLook` and `IShipCrewRig`. This half's parts
were written as components before those existed, so **`draft/ShipViewJoin.cs` wraps them**, following the table in
PLAN.md part 9. It compiles with both halves. It has not run.

```
ShipStage.Current.Look = new PsxStageLook(PsxStyles.Index("console"));   // .Component.Next() for the style button
ShipStage.Current.Crew = new PackCrewRig(menPrefabs, readTheChart: true);
```

**`PsxStageLook`** (`Apply`, `Remove`):

- makes a `PsxSeaGrid` and a `ShipViewLook` as children of the stage; gives the look `stage.View.Camera`, the stage's
  root, `stage.Water.Object` (in the `hdrpSea` slot) and the grid;
- makes the console twin of each of the kit's HDRP materials **at run time** (`kit.swaps`: same palette texture, shader
  `Kapman/ShipView/PSX Model`), so the kit needs nothing added. For a build, the three console shaders and
  `PsxScreen` must be referenced from somewhere serialized (the kit, or Always Included Shaders), or `Shader.Find`
  returns nothing there;
- sets `stage.View.drawnByLook`, shows the look's `Output` through `stage.View.shown`, and follows the shown size
  (`stage.View.ShownAtChanged`) and the stage's comings and goings (`stage.Changed`: the jetty, a new fit-out);
- each frame, after the stage has stepped: the grid gets `stage.Sea.State`, her way (`flow`), her wake; `stage.HeightAt`
  is set while a grid style is on; and the console shaders' light is worked out from **the stage's own sky**
  (`stage.SkyNow`, the record's `overcast` and `cloudLight`) by `PsxStageLook.Light`, which is `LevelClock.Apply`'s
  recipe for the sky's colours made into a function. `lightFromClock` is off: in the chart scene the clock's statics
  are the sky of 52 N 5 E, not of where she is.

**`PackCrewRig`** (`Attach`, `Tick`, `Detach`): section 9.5.

Things the join leaves to the lead: the men's prefabs are not in the stage's kit (add a `GameObject[] men` to
`ShipStageKit` and fill it in `ShipStageKitBuilder`, or pass them in); and when the crew is taken off again the oars
it took over go with it (the stage fits new ones at its next `Fit`).

The shader globals (`_PsxGrid`, `_PsxOptions`, the light) are global: only the stage's camera may draw objects with the
console shaders, which holds because the stage is on its own layer (31) that the chart's camera does not draw.

---

# PART 2. THE ROWING CREW

## 8. What the pack's files say

**The men** (`Prefabs - Characters/SMV_Viking_*.prefab`, all nineteen read):

- One skeleton of 31 bones for all of them, the same names and the same rest pose (the head's end is at
  (-0.003, 1.786, 0.028) in every prefab). No `Animator` on any prefab; the FBX files import as `animationType: 2`
  (Generic), `avatarSetup: 0`, no clips.
- The prefab root is plain (scale 1, he faces **+z**, his **left side is at -x**). Under it `metarig` (scale 100,
  turned -90 degrees about x) and then the bones, whose own axes are Blender's (y along the bone):
  `Main` (the hips, 0.939 m up) > `pelvis.L/R` > `thigh.L/R` (0.414 m) > `shin.L/R` (0.409 m) > `foot.L/R` (ankle
  0.126 m up) > `toe.L/R`; `Main` > `Stomach` > `Middle` > `Chest` > `LowNeck` > `Neck` > `Head` (top 1.79 m);
  `Chest` > `shoulder.L/R` > `upper_arm.L/R` (0.261 m; the joints are 1.48 m up and 0.365 m apart) > `forearm.L/R`
  (0.238 m) > `hand.L/R`. Shoulder to wrist is 0.50 m: short, chunky arms.
- The rest pose is an A-pose: arms out and about 32 degrees down. **Any man whose arms the code does not place
  stands like a scarecrow**, so the code always places both.
- The body is three to six skinned parts on the same bones (`Outfit`, `Helmet`, one or more `Addon`), all with the
  material `SMV_BasePallete`. A man is roughly 1,000 to 2,000 polygons.
- Every prefab holds a weapon under `hand.R`, and ten hold a shield under `hand.L`, one a second axe (nested prefabs). The seating
  tool switches them off. For a trading crew the four `Settler` prefabs (tunics, hats) and the `Karl`s are right.

**The ship** (`SMV_Ship.prefab`, vertices read from `Models/SMV_Ship.fbx`; x to starboard, z to the bow):

- **17.9 m long, 5.0 m in the beam**, keel at y -0.41, stems to 5.9 m, mast top 9.5 m. The steering oar hangs on the
  starboard quarter, so the stern is -z. 2,354 polygons with one sail.
- The rail's top: y 2.09 at x 2.48 amidships, 2.15 at x 2.22 at z +-2.2, 2.30 at x 1.87 at z +-4.3
  (`ShipViewSeating.Rail`). Bottom boards at y 1.3 to 1.5; a raised deck at y 1.87 aft of z -6.6 and forward of 6.5.
- **Benches:** 13 `Ship_Bench` (a `Plank_4` on a `Short_log_1`), 7 to starboard and 6 to port, plank middles 1.2 to
  1.7 m off the centre line, **bench top y 1.745** (the two aftmost 1.96), about 1.1 m apart. So a bench top is 0.35 m
  under the rail and 0.25 to 0.45 m over the bottom boards.
- **The pack's own oars are decoration.** 9 to port and 8 to starboard (`Oar_2`), all frozen at 45 degrees, not
  paired with the benches, and the starboard group sits 0.32 m further out and 0.26 m lower than the port one (its
  parent `Ship_Oars_R` is offset): outside the hull. The tool hides both groups and makes its own.
- `Oar_2`: runs along its own **y**, the origin where it should turn: 0.764 m to the handle's end (+y), 2.364 m to
  the blade's tip (-y); the blade is 0.95 m long, 0.24 m wide, its face across z. 3.13 m in all, which is short for a
  ship with 0.9 m of freeboard.
- `Ship_Sail_Open` (active) and `Ship_Sail_Close` (inactive) are whole meshes at the ship's origin: setting and
  furling is switching between them.
- `ShipSteerOar_1` at (1.19, 3.42, -6.35), its shaft along (-0.12, 0.94, 0.33); the mesh is 7.6 m long and 0.15 to
  0.24 m thick, with no tiller. Its top is out of a man's reach; he holds the shaft.
- **Where the water stands:** the pack's `Demo.unity` puts its water at y 1.22 with the ships at 0.02, so still water
  is at about **1.2 in the ship's space** (freeboard 0.9 m at the rail, the bottom boards just above the water).
  `RowingRig.waterline` and `PsxSeaGrid.waterline` are 1.2. The other agent sets how she really floats.

## 9. The rowing

### 9.1 Three ways, and the one chosen

| | Bones driven by code each frame | An AnimationClip written by an editor tool | The Animation Rigging package |
|---|---|---|---|
| Needs | nothing | an `Animator` or a playable per man, a controller, keyframes on 20 bone paths | a package (not installed), an `Animator`, a rig per man, Burst and Jobs |
| Hands on the oar | exact, by a two-bone reach each frame | only for the one seat the keys were made for; every bench has another rail height | exact (its two-bone constraint) |
| Change the stroke | a number, while it runs | re-bake the clip | targets still have to be driven by code |
| Can be checked without Unity | **yes, and was** | no | no |

**Chosen: bones by code.** The rail's height and the oar's slope differ from bench to bench, so the hands must be
solved against each oar anyway; once that is code, a clip adds an asset and an `Animator` for nothing. A clip is
still the right tool later for canned moves that touch nothing (stretching, eating, hauling the yard).

### 9.2 The stroke

One number, `RowingRig.Phase`, 0 to 1. `Stroke.At(phase)` turns it into four values.

| Beat | Phase | At 22 strokes a minute (2.73 s) | The oar | The man |
|---|---|---|---|---|
| Drive | 0.00 to 0.40 | 1.09 s | blade in; sweeps from 26 degrees toward the bow to 24 toward the stern | the back swings first (from 22 degrees forward to 14 back), the arms bend after |
| Finish | 0.40 to 0.50 | 0.27 s | blade lifts out and turns flat (35 degrees) | hands drop |
| Recovery | 0.50 to 0.92 | 1.15 s | blade goes back toward the bow in the air | hands away first, then the back follows |
| Catch | 0.92 to 1.00 | 0.22 s | blade turns square and drops in | he is at full reach |

A working boat, not a race: 22 strokes a minute in a calm, 18 near a berth, 24 against a foul wind. All oars run on
the one phase. The stroke runs on real time (`Time.deltaTime`), not on the game's clock: at 4x the men do not row four
times as fast.

**The oar** turns about its thole on the rail. How far it dips is not a fixed angle: it is worked out each frame
from the thole's height over the water where the blade is (`RowingRig.waterHeight`, or the ship's own waterline), so
the middle of the blade is 0.12 m under on the drive and 0.30 m clear on the way back whatever the rail's height,
and a wave lifts the blade.

**The oar is stretched to 1.5 times its length** (4.7 m) and slid outboard so 0.80 m is left inboard. This came out
of the check: with the pack's 3.1 m oar and 0.9 m of freeboard the oar had to slope 33 degrees, and the hands were at
chin height all through the drive. At 4.7 m it slopes 19 degrees and the hands come to the upper chest. The blade
becomes long and narrow, which is right for the period. `ShipViewSeating.Build(rowers, oarLength, shields)`.

**The man**, every frame: back to the rest pose; hips put on the seat (0.10 m above the bench top) facing the stern;
each ankle taken to a brace point 0.55 m ahead and 0.20 m below the bench top (so the knees come up, as on a low
bench); the back leant from the hips over three bones (`Stomach` 50%, `Middle` 30%, `Chest` 20%); each wrist taken
to its grip on **his own oar** (0.40 and 0.68 m inboard of the thole, the inner hand being the one further from the
rail, 7 cm short of the grip for the palm), elbows down and out; the head held up against the lean.

All turning is about axes in the man's own root frame, never a bone's own axes, so **the bones' axes do not matter**
(`CrewBody.Turn`). The two-bone reach (`Limb.Solve`) places the elbow or knee from the two lengths and a hint, and
swings each bone the shortest way onto its line.

**Life:** breath (the chest and collar bones; on the stroke when rowing, 15 breaths a minute at rest); each man is up
to 2% of a cycle late and swings 90 to 110% as far (from his `seed`); heads wander slowly when there is nothing to
do. `RowingRig.BladeIn` and `BladeOut` fire with each blade's place, for a splash and for the sound (the sound is the
stage's, from public-domain recordings only).

### 9.3 When she rows, when she sails, and what the men do

`ShipCrew.Decide` (pure, tested). It follows the game's own rule (`Travel.Speed`: she rows in a calm and when rowing
beats tacking), with two things the eye expects added.

| She is | Shown |
|---|---|
| made fast at a berth (`ship.anchored`) | sail furled, oars in, the men sit, hands on their thighs; the helmsman looks about |
| within twice a port's reach of a berth, under way | **oars**, 18 a minute: she is rowed off her berth and onto it |
| in a calm (wind under 1.5 m/s) | **oars**, 22 a minute |
| on a river with the wind closer than her `closestToWind` (58 degrees) | **oars**, 24 a minute: no room to tack |
| anywhere, when her oars are faster than her sail is now | **oars** |
| otherwise | **sail** set, oars in, the men sit; the helmsman at the steering oar, which turns with her turning |
| lying to in a gale (`ChartBody.Sheltering`) | sail furled, oars in |

`ShipCrewFeed.Read` fills the inputs from `ChartBody` (knots, wind, sheltering), `ChartWorld.OnRiver()`,
`Port.Nearest`, and the class's own figures worked as `Travel.Speed` works them, so the window shows oars exactly
when the game is moving her by oars. Better still would be for `Travel` to say so itself (one rule in one place):
a small `Travel.UnderOars(...)` beside `Speed` when this is built in.

A change takes time: a mode is held 6 s; the oars come out in 2.5 s, then the stroke builds over 3 s; going to sail,
the stroke dies, the oars are tossed up and vanish (they are "laid along the benches", which is not drawn), and only
then is the sail set. The check's minute of it: moored, oars out by 8 s, full stroke by 12 s, sail set 6 s after the
order.

### 9.4 What was checked without Unity

`check/RowCheck.cs` reads `SMV_Viking_Settler_1.prefab` and `Karl_1` straight from their YAML into stand-in
transforms (`check/UnityShim.cs`: Unity's own quaternion maths), seats them as the seating tool would on the pack's
real bench and rail figures, and runs the three crew drafts unchanged. Result:

- the skeleton reads as expected (hips 0.939 m, reach 0.500 m, faces +z, left at -x);
- over the whole stroke both wrists are 7.0 cm from their grips (the palm) with **0.0 cm** of miss, the ankles are on
  their braces, the blade is 0.12 m under on the drive and 0.30 m over on the recovery;
- the port man mirrors the starboard man (`pictures/port.png`);
- turning and moving the ship changes nothing in her frame (0.00 cm);
- the nine cases of the rule give the modes in the table above;
- the helmsman's hand reaches the steering oar from where the tool stands him (`pictures/helm.png`);
- in the stage's small boat one man pulls a pair with both hands on their handles through the whole stroke (worst miss
  0.6 cm), both blades in and out as they should be, and the seated helmsman holds the steering oar
  (`pictures/small_boat.png`).

`pictures/stroke_side.png`, `stroke_astern.png`, `stroke_above.png`: eight moments of the stroke as stick figures.
They show the pose is a rower's. They do not show the skin, the clothes, or how the mesh folds.

### 9.5 Two boats, two ways of seating

The two halves met on the size of the boat (PLAN.md, part 5 and its question 2). The crew code does not care: it needs
seats and tholes. Both ways are drafted and both were run on the skeleton.

| | The longship at her own 17.9 m | The stage's boat: the hull at 0.43 (7.7 m), full-size men |
|---|---|---|
| Made by | `ShipViewSeating.Build` (editor): a prefab, the pack's benches and rail | `PackCrewRig.Attach` (run time): the stage's thwarts, `Seats`, `OarPivots`, its oars |
| Rowing | one man to an oar, both hands on it, sitting 0.56 m inboard of his thole | **one man to a pair**, a hand to each oar (`RowingRig.Rower.oar2`), sitting in the middle of the thwart |
| Men | 4 or 8 rowers and a helmsman standing aft | a man to each thwart (three for six oars) and a helmsman **sitting** aft (`ShipCrew.helmSits`) |
| The oar | `Oar_2` stretched to 4.7 m, 0.80 m inboard | the kit's `Oar_1` stretched by half (4.0 m) and slid inboard so its handle ends a hand short of the middle |
| Slope of the oar on the drive | 19 degrees (rail 0.9 m over the water) | 13 degrees (thole 0.41 m over the water): a small boat rows more naturally |
| Hands | upper chest | 0.32 to 0.43 m over the seat: low chest, passing just over the knees |
| Sweep | 26 degrees to the bow, 24 to the stern | 20 and 22 (the handles are longer inboard; with 26 the catch was 8 cm out of reach) |
| Checked | `pictures/stroke_*.png`, `port.png`, `helm.png` | `pictures/small_boat.png` |

In the small boat the thwart's top is only 0.10 m over the boards, so the men sit low with their knees up
(`footDown` comes out at -0.03: the ankles are level with the seat). Look at this first: it is the pose most likely to
fold the pack's low-poly hips badly. Raising the thwarts by 0.10 to 0.15 m in `StageShip.Fit` would help the men and
cost nothing else.

A man to each side seat with one oar each (`StageShip.SideSeats`) is not in the join; the rig can do it (it is the
longship's way).



## 10. What must be looked at in Unity first

In this order, each a minute's look, before anything is tuned:

1. **Pink.** Does `Kapman/ShipView/PSX Model` compile? If the ship is pink in the console styles, read the shader's
   error in the Inspector first. The two likeliest: an include path, or `noperspective` on a target.
2. **Is she in the fog rightly?** With fog on, in a console style, the ship should fade with distance like the sea,
   not vanish as if at the horizon. If she vanishes, the `DepthForwardOnly` pass is not being drawn (frame debugger:
   "Forward Depth Prepass").
3. **Holes or flicker on moving parts** in the console styles: object motion vectors still on (2.3).
4. **Brightness** of the ship against the sky at noon, dusk, midnight (`sunScale`, `skyScale`, `groundScale`,
   `PsxStyle.white`), and **the horizon line** in the grid styles (3).
5. **The men's scale and seat.** Each man should sit on his plank, not in it or over it: `RowingRig.hipAboveSeat`
   (0.10). Feet on the bottom boards, not through them: `footDown` (0.20), `footAhead` (0.55).
6. **The bones' axes.** The code does not depend on them, but check one thing: the knees must point up and the
   elbows down and out. If a limb folds the wrong way, its hint is on the wrong side (`RowingRig.PoseMan`).
7. **The skin.** Low-poly shoulders and hips may pinch when a man sits and reaches (the pack's own README warns of
   this). If the hips fold badly, raise `hipAboveSeat` and lower the thigh's angle by raising `footDown`.
8. **The oars.** Do they lie on the rail (not through it, not floating)? `ShipViewSeating.Rail` holds the measured
   rail; the thole is 3 cm inside it and 2 cm above. Is the blade in the water on the drive and clear on the way
   back? That depends on where she really floats: `RowingRig.waterline`, or better `rig.waterHeight`.
9. **The hands' height.** They should be at the upper chest. If they are at the chin, lengthen the oar
   (`oarLength` 1.7) or float her deeper.
10. **The helmsman**: is there deck under (0.78, 1.87, -6.75), and does the steering oar's shaft pass where his
    hand is? Its mesh is thick there (about 0.2 m).
11. **The sail** from both sides: if it is see-through from behind, give its console material `_Cull` Off.
12. **The dither's size** in the small window and on the whole screen (`minPixel`).

Items 5 and 8 to 10 name the longship's figures. In the stage's small boat the same things are set in
`PackCrewRig.Fit` (`draft/ShipViewJoin.cs`): `footDown` from the thwart's height over the boards, the tholes at
`ship.OarPivots`, the oar's stretch (`OarStretch`), the helm seat; and there the first thing to look at is the men's
hips and knees, because they sit only 0.10 m over the boards (section 9.5).

## 11. Not done, and open

- No emitters for spray, rain and splashes (the stage's; the shader is here).
- No console-style sky (3).
- The men do not move about the ship, haul the yard or bail: they sit, row and steer.
- Nothing has been measured for cost.
- **Which boat, and how many oars: the user's to choose.** The game's boat is a `gislinge`: 7.7 x 1.5 m, six oars, a
  crew of four (`Data/Ships/ships.json`, the user's decision of 1 Oct); the pack's ship is a 17.9 m longship with 13
  benches. PLAN.md asks it as two questions (its end): six oars or four, and the hull shrunk to 7.7 m with full-size
  men or the longship at her own size. Section 9.5 has both seated. A third way, not looked into further: the project
  already holds a true Gislinge boat model (`Assets/Kapman/Models/Ships/Gislinge/Gislinge_Boat.fbx`, CC BY 4.0, with
  thole straps, a rudder and a tiller, about 15,000 polygons, real textures), which four of the pack's men would
  fit, and on whose planks the console's affine stretching would at last have a picture to stretch.
