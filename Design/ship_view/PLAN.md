# The 3D ship view: the stage and its wiring

Plan of 7 October 2026, for the lead. Nothing here has run in Unity. What was checked, and how, is said where it matters
and listed at the end. The drafts are in `Design/ship_view/draft/` (13 C# files, all compile; part 10).

The user's words this answers: "start working on the 3D view for the ship, it'll just be water and when it docks just a
simple dock model ... simulated ocean weather wind everything"; "the map and the 3D sailing boat can be changed focus
with a simple button on the ... top left of the 3D sailing boat that switches place with the map and miniaturises the map".

## 0. In short

- **The stage is built at run time from one small kit asset** (`Assets/Kapman/Resources/ShipStageKit.asset`), on layer
  31, standing at (0, 0, -50000). No scene of its own, nothing added to L_Chart, no line changed in ChartBuilder.
- **None of the 3D levels' components can go into the chart scene.** LevelInfo, LevelWeather, HdrpSea, SkyDome and an
  HDRP WaterSurface each set a static or are found by a finder that the chart game reads, and the chart game then takes
  the chart for a 3D level (part 1). So the stage reuses the game's *models* (plain C# in Kapman.Core: the sea state,
  the wave growth laws, the sky's astronomy, the lighting model) and the level code's *recipes* (the Sun's and Moon's
  light set-up, the five-point float), and has small parts of its own to draw them.
- **The sea is the stage's own mesh, moved by eight waves made from the game's SeaStateModel.** Not HDRP's water. She is
  held at the middle and the waves' phases run past her at her speed, so she meets a head sea faster than a following one.
- **One camera, never enabled, drawn by render request into one texture.** The frame's ship window and the full-screen
  view both show that texture. The swap changes no camera: it changes the texture's size and makes the map's own UI
  document small. The chart's camera keeps the screen always.
- **The hook needs no edit to the frame.** `ShipStageHook` starts itself in a scene that has a ChartWorld, finds the
  frame's ship window by its class name and lays its picture over the drawn stand-in.
- **The boat is the pack's longship at 0.43 scale** (17.88 m shown as 7.7 m), floated on the pack's own waterline so her rail stands 0.38 m out of the water (her class says 0.34 m), her shields, seventeen oars, thirteen
  benches and deck clutter switched off, with three thwarts and six full-size oars of ours.
- **The pack's materials are URP and will draw pink** in this HDRP project. The kit builder makes an HDRP/Lit twin for
  each and the stage swaps them in on its own copies.

## 1. What is reused

### The finding that shapes everything

The chart scene (ChartBuilder.cs) has a LevelClock with no Sun or Moon, a camera whose culling mask is 0, and no
LevelInfo, LevelWeather, sky dome or water. The chart game *relies* on those being absent:

| If the stage brought | this happens in the chart game | where |
|---|---|---|
| a `LevelInfo` (anywhere in the loaded scenes: `LevelInfo.Current` finds any) | world positions go through the level's centre and scale instead of the FloatingOrigin; "level_kind" stops asking the chart; the save names a level | MapWorld.cs 28-76, SimStage.cs 74-80, EncounterGameVars.cs 122, SaveGame.cs 86 and 188, LevelFrame.cs 28 |
| a `LevelWeather` (it takes `LevelWeather.Current` in OnEnable) | the lookout's haze, the sim stage's wind, the SailingShip shim's TrueWind and the LocalWeather shim all switch from the chart's weather to the stage's | MapView.cs 1154, SimStage.cs 135 and 152, SailingShip.cs (TrueWind), LocalWeather.cs 25 and 98 |
| any HDRP `WaterSurface` | `WaterSampler.Surface` finds it (`FindAnyObjectByType<WaterSurface>()`), and the chart ship's depth is asked of it | WaterSampler.cs 22-30, SailingShip.Depth |
| a collider | `SailingShip.GroundAt` and `ShipMotion.Ground` cast a ray down from 500 m at the chart ship's place; a jetty under that ray grounds her | SailingShip.cs GroundAt, ShipMotion.cs Ground |
| an HDRP `WaterSurface`, again | **the chart's camera would draw it.** HDRP skips a water surface for a camera only when the camera's mask is not 0 and lacks the surface's layer; the chart camera's mask is 0, and water rendering is on for every camera in the project's default volume profile | HDRP Runtime/Water/HDRenderPipeline.WaterSystem.cs 956-959; Assets/HDRPDefaultResources/DefaultSettingsVolumeProfile.asset (WaterRendering.enable = 1) |

And the level components cannot be fed the chart's truth anyway. LevelWeather's values have private setters: it takes
its weather from a kind set by hand (eight kinds, each with a fixed wind) or from `WeatherEngine.Shared` at
LevelInfo's latitude. The chart ship sails by `WorldSim.WeatherAt` (ChartBody.cs, Travel.Speed). HdrpSea needs
`LevelWeather.Current` for its wind and `LevelInfo.Current` for its sea state (HdrpSea.StepSea returns false without it).

So: **the stage holds no LevelInfo, LevelWeather, HdrpSea, SkyDome, LevelWater, WaterWaves, ShipMotion, WaterSurface,
collider, AudioListener or MainCamera tag.** The draft test `TheStageBringsNothingOfALevel` asserts each of these.

### Class by class

| The game's class | Verdict | How the stage uses it, and what it needs |
|---|---|---|
| `Kapman.Weather.SeaStateModel`, `FetchTable`, `SeaState` (Core/Weather/SeaStateModel.cs) | **reused whole** | The sea's state. Made with no engine: `new SeaStateModel(null, lat, lon, "north_sea", FetchTable.Open(km, depth)) { Swell = false }`, then `WarmStart(hours, wind, from, 1013)` once and `Step(hours, dt, wind, from, 1013)` after, exactly as HdrpSea.StepSea does for weather set by hand. Plain C#, no statics. Needs nothing. |
| `WaveGrowth` (Core/Weather) | reused through the model | The Shore Protection Manual growth laws: the sea from wind, fetch, depth. |
| `Kapman.World.SkyMath` (Core/World) | **reused whole** | Sun, Moon, phase, refraction. `StageSky.At` calls the same functions in the same order as `Sky.Compute`, into a value. Checked outside Unity: for 21 June 793, 15:00, 52 N 5 E both give Sun 45.520, Moon 25.447, lit 0.5163. |
| `Kapman.World.Sky` (statics) | **not written** | `Sky.Compute` fills statics that LevelClock fills each frame for 52 N 5 E and others read. Only `Sky.ToUt` (a pure function) is called. |
| `Kapman.World.LightingModel` (Core/Light) | **reused** | `CloudDim` dims the Sun by the cloud's transmittance; `TotalLux` and `EV100FromLux` give the fixed exposure. |
| `LevelClock` (Runtime/Levels) | **read only, and its recipe copied** | The chart's own clock gives the moment (`JulianDay`, `GameHours`, `TimeFactor`), through the state. Its `SetUp` (the Sun at 130,000 lux with a 0.53 degree disc drawn on the sky, the Moon lit by the Sun so its phase is right) is copied line for line into `ShipStage.Lights`. The clock itself is not given the stage's lights: it would light them for 52 N 5 E. |
| `SkyDome.SkyRotation` (a public static) | reused | Turns the star cube (`Assets/Kapman/Sky/Stars_930.exr`, held by the kit) to the moment and her latitude. The rest of SkyDome (the eye model, the planet's radius) is left: the stage's exposure is fixed from the lighting model. |
| `ShipMotion` (Runtime/Levels) | **recipe copied** | Its five-point float (bow, stern, both sides, twice the middle, over six; pitch from stern less bow over the length; roll at 0.6) is `StageShip.Tick`. The component itself reads LevelInfo and WaterSampler and takes `ShipMotion.Player`. |
| `HdrpSea`, `WaterProfileDriver`, `WaterSampler`, `LevelWater`, `ShoreWater`, `RiverCurrent`, `CalmReflections` | **left out** | All drive or read an HDRP WaterSurface. See the table above. |
| `ShipWake` (Runtime/Levels/Water) | **left out** | It draws into HDRP's water with water decals ("only HDRP's water takes water decals", its own summary). The stage has a foam ribbon astern instead (`ShipStage.WakeStep`), 14 sections, tapering to nothing, longer with her speed. |
| `HdrpWeatherDraw`, `CloudPass`, `SnowFrostPass` | **left out, too heavy** | Volumetric clouds from a cloud map, volumetric fog, a local fog volume, two full-screen passes. For a 480 x 270 picture the stage uses HDRP's CloudLayer (one flat layer drawn with the sky), plain (not volumetric) fog, and one particle system for rain. |
| `Kapman.Ships.*` (ShipBody, ShipSim, Hydrostatics, HullForces, HullProbes, Helm, Mooring, HdrpWaterField) | **left out** | A Rigidbody stepped at 50 Hz over HDRP water, with `Physics.simulationMode = Script`. Kapman.Game does not reference Kapman.Ships. A later step could give ShipSim a water field backed by `StageSea.Height`; not needed for a picture. |
| `ChartWorld.TryWeather` / `WorldSim.WeatherAt` | **the weather's source** | Through the state (part 3). |
| KapmanSail_dev (the look lab) | lessons only | Its treadmill carried the water square along with a ship that really moved. Here nothing moves but phases. Its TankSea shows the fit of a sea state to HDRP's bands (SeaFit) should HDRP's water ever be wanted as one look. |

### Left out because the window is small

Volumetric clouds and fog, SSR, SSAO, SSGI, contact shadows, motion vectors, decals, refraction, bloom, depth of field,
film grain, probes, ray tracing, HDRP water. All off in the camera's frame settings (the list is in ShipStageView.cs).
Also not built: snow (rain only), lightning, aurora, the tide's height at the jetty, river banks, swell from far storms.

## 2. The stage

### What stands on it

`ShipStage.Build(kit, source)` makes one root object, `--- SHIP STAGE (the 3D view) ---`, at `kit.origin` (0, 0, -50000),
everything under it on `kit.layer` (31; a layer needs no name to be used, so TagManager is not touched):

| Child | Made by | What it is |
|---|---|---|
| Water | `StageWater` | 81 x 81 points: half-metre cells out to 10 m, then 20 cells a side growing by a fixed ratio to 420 m, and four flat skirts to 14 km. Moved on the CPU each frame by `StageSea`; true normals; waves shorter than two cells of the mesh where they stand are left out. Material `SV_Water` (HDRP/Lit, dark, smoothness 0.86): it works with no shader of ours. |
| Ship | `StageShip` | Part 5. |
| Jetty | `StageDock` | Part 4. Only while she is at or near a berth. |
| Sun, Moon | `ShipStage.Lights` | Two directional lights in lux, as LevelClock makes them. |
| Sky and air | `ShipStage.Atmosphere` | A global Volume with a run-time profile: Physically Based Sky (rendering space Camera, so the stage may stand anywhere), CloudLayer, Fog (not volumetric), Exposure (fixed), VolumetricClouds off. |
| Ship stage camera | `ShipStageView` | Disabled; culling mask and volume layer mask both `1 << 31`. Rain hangs under it. |
| Wake, Bow line, Stern line | `ShipStage` | A foam ribbon; two ropes (LineRenderer, three points) shown when she is made fast. |

Its axes are the compass (x east, z north), as the game's are. **She keeps her true heading**; the Sun is then on the
right side of her and the wind and the seas come from where the chart says.

### The sea and the treadmill

`StageSea.Step(state, dt)`:
1. The model: the fetch and depth of the place (part 3), warm-started on the first frame and after a jump of the clock
   (more than 0.75 game hours, the same test as HdrpSea), else stepped by the game hours gone by. So the sea rises over
   game hours when she leaves a river, as in the 3D levels.
2. Eight sine waves from that state. Six for the wind sea: periods 1.25, 1, 0.85, 0.7, 0.55 and 0.42 of the peak period;
   energy shares 0.12, 0.34, 0.22, 0.16, 0.10, 0.06 of m0 = (Hs / 4)^2; fanned up to 1.5 spreads either side of
   downwind. Two for a swell (zero until the engine's swell is brought in). Deep-water speed. No wave steeper than
   amplitude x wavenumber 0.3. Heights follow in 3 s, periods and bearings in 20 s.
3. Each wave keeps a running phase, moved by `(k d.v - omega) dt`, v her way through the water (speed x the clock's
   factor, as ShipMotion moves her). That is the treadmill: the mesh, the boat and the camera stay put. Nothing grows
   without bound, so nothing loses precision however long she sails.
4. The same waves go out as shader globals `_StageWaveA[8]` and `_StageWaveB[8]` (the formula is in StageSea.cs) for a
   look that moves its water in a vertex shader.

What the model gives, worked out outside Unity with the project's own Kapman.Core.dll:

| Place (fetch, depth) | Wind | Hs | Tp |
|---|---|---|---|
| river (0.4 km, 3 m) | 5 m/s | 0.05 m | 0.8 s |
| river | 9 m/s | 0.11 m | 1.0 s |
| port (0.8 km, 3 m) | 3 m/s | 0.04 m | 0.8 s |
| coast (40 km, 15 m) | 8 m/s | 0.94 m | 4.5 s |
| open sea (300 km, 40 m) | 6 m/s | 1.01 m | 5.0 s |
| open sea | 12 m/s | 4.03 m | 10.0 s |
| open sea | 19 m/s (she lies to over 17) | 7.43 m | 13.6 s |

Two things follow. **A river's waves are 5 cm high and a metre long**: the mesh cannot carry them (they are under two
cells) and should not; a river will look like a flat mirror until a look gives the water ripples in its shader. And **a
gale on the open sea is 7 m**: the user's decision for the levels was the sea at full real height (HdrpSea's summary,
"the user's decision 4"), so the stage shows it; the camera is kept 0.7 m over the water wherever it stands.

### Light, sky, air

Each frame `ShipStage.SkyAndAir`:
- `StageSky.At(julianDay, latitude, longitude)` places the Sun and the Moon for **her own latitude**. (The chart's
  LevelClock cannot: with no LevelInfo it works the sky out for 52 N 5 E wherever she is. LevelClock.cs, Apply. So
  `LevelClock.Night` and `SunAltitude`, which the frame's drawn stand-in reads, are Dorestad's everywhere. Not changed here.)
- The Sun: `kit.sunLux` (130,000) x `LightingModel.CloudDim(cloudLight)` x (1 - the share the Moon hides). It stays on
  under the horizon, as the clock's does: the sky's twilight comes from it. HDRP's sky thins and reddens it.
- The Moon: its own lux x `kit.moonBoost` (3, the clock's), dimmed by cloud; it casts the shadows when the Sun is down.
- Exposure: **fixed**, at `EV100FromLux(TotalLux(...))`, clamped to `kit.exposureFloor .. exposureCeiling` (-5 .. 15.5),
  eased over half a second. Worked out for 53.4 N on 21 June 793: noon 15.5, 19:54 (Sun 3.4 degrees up) 10.6, 20:30 6.7,
  midnight with a half Moon low -7.2 (so the floor holds it at -5); under a full Moon 46 degrees up, -3.2. Fixed
  because the camera draws only on some frames (metering would hunt) and so that a test's picture is the same every run.
- Fog: mean free path = visibility / 3.912; its top falls from 2500 m to 120 m as `fog` goes 0 to 1.
- Cloud: CloudLayer's opacity = cover; more of its stratus channel and a greyer tint as it greys over.
- Rain: up to 900 drops a second in a 34 x 22 m box over the camera, driven down the wind.

### The camera and the second HDRP camera's cost

HDRP has no camera stacking: every enabled camera is a whole frame of its own (culling, light lists, shadows, sky,
post). The chart's camera already is one, though it draws nothing. The stage's is kept cheap like this:

- **It is never enabled.** `ShipStageView.Render` draws it with `RenderPipeline.SubmitRenderRequest(camera, new
  RenderPipeline.StandardRequest { destination = texture })`. HDRP supports this (HDRenderPipeline.cs 2651), and the
  project already draws this way in the editor (Editor/DorestadVegetation.cs 705-715, Editor/Shots.cs 475-486).
- **Every second frame** in the small window (`kit.smallEvery`), every frame when it fills the screen.
- **Small.** 270 pixels high in the window (the frame's `.frame-ship-view` is the slip's width, 24 % of the screen less
  its edges, by 262 px in Frame.uss: about 441 x 262 at 1920 x 1080, so the texture comes out about 454 x 270, 6 % of
  the screen's pixels). 540 high when it fills the screen, stretched with point filtering. Both are the kit's numbers
  and a look may ask for its own.
- **One layer.** `cullingMask = 1 << 31`.
- **Its own volumes only.** `HDAdditionalCameraData.volumeLayerMask = 1 << 31`. The chart camera's mask is the default
  (layer 0), so it never takes the stage's sky, fog or exposure. The stage's camera takes no volume of any scene's,
  only HDRP's default profile (Assets/HDRPDefaultResources/DefaultSettingsVolumeProfile.asset: an HDRI sky, bloom,
  motion blur, its own exposure, water rendering on). The stage's volume overrides that at priority 100 where it
  matters (the sky's kind, the exposure, the fog) and the frame settings switch off the rest (bloom, motion blur, water).
  (Look/KapmanVolume.asset is the 3D levels' own scene volume; the chart scene has no volume at all.)
- **Forward, and 47 frame settings off** (ShipStageView.Off). On: opaque, transparent, the Sun's shadow map, fog, sky
  reflection, exposure, tonemapping, custom passes and custom post-processes (for the looks).

**Its cost is not measured.** The fixed part of an HDRP camera does not shrink with the picture. Step 7 measures it:
play L_Chart with `ShipStageHook.Off` true and false and compare frame times. If it bites: risk R7.

The view: off her port quarter, 1.5 lengths away and 0.42 lengths up, 34 degrees; it turns with her over a second or
two and drifts 9 degrees either way so a long watch is not a still. With the jetty within 45 m it goes round to her
starboard side, so jetty and bank are behind her. Filling the screen: 1.75 lengths, 40 degrees.

### Can HDRP's WaterSurface live beside the chart scene?

It can be made to, and it is not worth it now. Three things would have to be done: turn Water off in the chart camera's
frame settings (or it draws the sea, its mask being 0); keep `WaterSampler` from finding the surface; and drive the
surface with code of our own, since HdrpSea cannot run without LevelInfo and LevelWeather. What it would buy is HDRP's
water shading, which a PSX look throws away. If it is wanted later as one look among several, it goes behind
`StageWater` (the boat already floats on `StageSea.Height`, not on what is drawn), fitted with `SeaFit` as the look
lab's TankSea does.

### The swap

Nothing changes target. In both states the chart's camera owns the screen and the stage's camera draws to its texture.

| | Normal | Swapped |
|---|---|---|
| Stage texture | about 454 x 270, every 2nd frame | 960 x 540 (or the screen's own height if `kit.bigHeight` is 0), every frame |
| Shown by | an `Image` filling `.frame-ship-view` | an `Image` filling the screen, first child of `FrameRoot.Root` (behind every slip) |
| The map's UIDocument | fills the screen, sorting order 39 | its root element set to the ship window's rectangle, clipped, sorting order 42.5 (over the frame at 42, under the notices at 43 and the menu at 50) |
| Swap button | top left of the ship window | top left of the small chart (a second button, on the map's document) |

The chart is not drawn into a texture: it stays a live document, so in the small window it still pans, zooms and takes
clicks. MapView sizes itself from `layer.resolvedStyle` each frame (MapView.Draw), so it needs no change; `MapView.Follow()`
is called at each swap to put her back in the middle of the new rectangle.
The swap stands down while the frame is hidden (the title; the chest open on the map), and comes back after.

### One scene, or a kit: the kit

- ChartBuilder makes L_Chart from an empty scene every time it runs. Anything put in that scene by hand is lost, and
  anything put there by code is an edit to ChartBuilder. An additive scene needs loading by someone: again an edit, and
  a new entry in the build settings.
- The jetty is built per port at run time whatever is chosen, and the boat is fitted per class.
- The test builds the same stage in an empty scene with the same one call.
- A scene file is one more thing for Unity's own assistant and the other builders to collide on. The kit is one small
  asset with prefab references and numbers, editable in the inspector.
- The price: the stage cannot be dressed by hand in the editor. It can be looked at in the Hierarchy during Play.

## 3. The state

`ShipStageState` (a struct) is all that passes. `IShipStageSource.Read(ref state)` fills it; `ChartStageSource` is the
chart game's. Bearings are compass degrees, speeds real m/s.

| Field | From, in the chart game today |
|---|---|
| `moored`, `portId` | `Port.LyingId()`: anchored and inside the nearest port's reach |
| `berthing`, `berthDistance` | `Course.Destination`'s `Port`, `Port.DistanceTo(game.ShipPosition)` under 120 game metres |
| `heading`, `speed` | `SailingShip.Heading`, `SailingShip.Speed` (ChartBody publishes them through ShipMotion) |
| `lyingTo` | `ChartBody.Sheltering` (Travel.Speed: over 17 m/s) |
| `underOars` | **Not in the chart game.** Worked out by Travel.Speed's own rule: she rows when `Travel.Speed(cls, heading, windFrom, wind)` is no more than `cls.oarsKn * min(1, cls.rowingHours / 12) * KnotMs`, i.e. the sail made no more than the oars |
| `windFrom`, `windSpeed`, `gust` | `ChartWorld.TryWeather` (= `WorldSim.WeatherAt`), the sample her speed is worked out from. `gust` is 0 from a model sampled on its own: then 1.4 x the wind |
| `cloud`, `overcast`, `storm` | the sample's `Cover`, `layered`, `storm` |
| `rain`, `snow` | the sample's `rain` (mm an hour) over 4, clamped, split by `SnowShare` |
| `fog`, `visibility` | the sample's `fog` and `VisibilityKm` |
| `cloudLight` | the sample's `cloudTransmittance` when it has a sky (`HasSky`), else `LightingModel.CloudTransmittance(layered, storm)` |
| `julianDay`, `hours`, `timeFactor` | `LevelClock.Current.JulianDay`, `.GameHours`, `LevelClock.TimeFactor` |
| `latitude`, `longitude` | `WorldFrame.ToLatLon(ChartWorld.ShipWorld())` |
| `place` | moored: Port. Under way on a course: `Course.Kinds[Course.Next]` (River, Pending, Portage: river; Sea: open sea; Coastal, Sound, Flats, Approach: coast). Lying still: `ChartWorld.OnRiver()`, else coast within 250 game metres (15 km) of a port, else open sea |
| `fetchKm`, `depth` | **Not in the chart game.** Four pairs by place, in ChartStageSource: river 0.4 km / 3 m, port 0.8 / 3, coast 40 / 15, open sea 300 / 40. The honest fix later: the lanes or the ports' water records carry a fetch by sector, as the levels' water records do (`FetchTable.FromWaterRecord`) |
| `classId`, `length`, `beam`, `draught`, `closestToWind`, `hasSail` | `WorldSim.ClassOf(ChartBody.classId)` |
| `crew` | `Voyage.Men` once the voyage has started, else the class's crew |
| `oars` | **Not read by the game.** The class row has `"oars": 6` for the Gislinge boat; `ShipClassDef` has no such field. A number on ChartStageSource (6) until SimData reads it |

Missing and not stood in for: **which tack she is on.** Beating, the chart points her straight at her goal and takes
the made-good speed. The stage never braces the sail closer than `closestToWind` (58 degrees), but her hull points into
the wind. Later: when the wind is inside that angle, show her 58 degrees off it and tack her every few minutes.

## 4. The dock

The pack has no dock. Sizes below are the meshes' own, read from `Models/SMV_Environment_1.fbx` (centimetres, imported
at 0.01); every prefab is unscaled and uses the material `SMV_Weapons`.

`StageDock.Build` makes a timber staging along a bank. Its axes are hers when she lies at it: z the way her bow
points, x towards her; x = 0 is the water-side edge of the boards, y = 0 the still water.

| Part | Prefab (size, m) | Count | Where |
|---|---|---|---|
| Deck boards | `Plank_2` (2.12 x 0.10 x 0.47) | 20 | Laid across, 0.489 m apart from z = -4.9 to +4.9; centres at x = -1.06; tops at y = 0.55; each a few centimetres and a degree or so askew |
| Stringers | `Plank_3` (3.09 x 0.11 x 0.56) | 6 | On edge, fore and aft, under the boards: two rows at x = -1.06 +/- 0.7, three in each row at z = -3.09, 0, +3.09; centres at y = 0.17 |
| Mooring posts | `Short_log_1` (0.33 across, 0.23 high) stretched 14.5 times to 3.4 m | 4 | Water side, x = -0.05, at z = -4.55, -1.63, +1.63, +4.55; tops 0.45 m over the boards |
| Piles | the same | 4 | Land side, x = -2.07, same z; tops 0.08 m over the boards |
| Barrels | `Barrel_1` (0.82 x 0.92 x 0.86) | 2 | x = -1.57, z = 3.7 (full size) and x = -1.5, z = 2.75 (0.8) |
| Crate | `Crate_1` (0.71 cube) | 1 | x = -1.62, z = 1.7 |
| Sacks | `Sack_1` (0.62 x 1.06 x 0.41), `Sack_3` (0.59 x 0.77 x 0.58) | 2 | x = -1.67, z = -3.6; x = -1.17, z = -3.0 |
| Rope | `Rope_1` (1.05 x 0.34 x 1.31), at 0.55 | 1 | x = -0.6, z = -3.95, by the stern post |
| Bucket | `Bucket_1` (0.64 x 0.71), at 0.5 | 1 | x = -0.55, z = 1.2 |
| The bank | made in code, material `SV_Shore` | 1 strip | Five points across: under water at x = -1.77, y = -1.6; the bank's lip at x = -2.52, y = 0.42; then y = 0.75 at x = -5.6, 1.3 at -16, 2.2 at -62. 180 m long. No trees (the user: none home-made) |

Stretching a 0.23 m log into a 3.4 m pile is honest only because the pack paints by palette: a flat colour does not
smear. If it looks wrong, a plain cylinder in the same material is one line (the stand-in path already makes one).

She lies **port side to** (her steering oar is on the starboard quarter: ShipSteerOar_1 at x = +1.19 in the prefab),
her side 0.35 m off the boards. Two ropes go from her bow and stern (port gunwale) to the end posts.

**Coming alongside and leaving without a jump** (`ShipStage.DockStep`). The jetty stands in the world: while it is on the
stage it goes astern with the water, at her speed x the clock's factor.
- *Coming in.* When the port she is bound for is within 120 game metres the jetty is made and put ahead of her by that
  distance and up to 6 m wide of her track, closing to her side over the last 40 m. One game metre of the chart is one
  metre of the stage, and both close at her speed (ChartBody: game metres a real second are her real m/s), so the
  jetty and the water move as one. A soft correction (time constant 0.7 s) keeps it on the chart's distance.
- *Made fast.* The chart stops her up to 5 m short of the berth (ChartBody: "dist < 5"). The jetty is eased the rest of
  the way alongside over about two seconds: she is warped in on her lines. Then the lines show.
- *Leaving.* Nothing special: the jetty is left astern as she gathers way, and taken off the stage at 260 m.
- A different port gets its own jetty: the boards' and props' small offsets are seeded by the port's id.

Not done: the tide's height against the posts; a jetty for a beach port or a great port; halls behind the bank.

## 5. Which ship

Measured from the pack's files (Models/SMV_Ship.fbx, centimetres; the prefab's YAML for the fittings):

| | The pack's `SMV_Ship` | The game's Gislinge boat (ship_classes_793.json) |
|---|---|---|
| Length | 17.88 m (hull mesh, z from -8.94 to +8.94) | 7.7 m |
| Beam | 5.01 m | 1.5 m (the find itself is about 2.1 m) |
| Keel under the origin | 0.41 m | draught 0.25 m |
| The water she was made for | 1.2 m over the origin: the pack's Demo.unity has `Water_1` at y 1.22 and its four ships at 0.024. So 1.6 m of hull is meant to be under water | |
| Stems | to 5.90 m over the origin | hull top 0.8 m over the water |
| Rail's top amidships | 2.09 m over the origin (the other builder's reading of the mesh; the oar ports are at y = 2.10 to 2.26) | freeboard 0.34 m |
| Deck amidships | about 1.45 m over the origin (the crate and weapon rack stand at 1.43 to 1.50) | |
| Mast top | 9.51 m | sail top 5 m |
| Sail | `Ship_Sail_Open`: 5.97 m wide, 6.45 m deep (39 m2); `Ship_Sail_Close`: furled on the yard at the masthead; five colour variants of each in the FBX | square |
| Oars | 17 in the prefab (9 to port, 8 to starboard, `Oar_2`, 3.13 m), about 0.85 m apart | "oars": 6 |
| Benches | 13 `Ship_Bench` | |
| Steering oar | `ShipSteerOar_1`, 7.62 m, starboard quarter | |
| Shields | 22 on two rails (`Ship_Shields_L`, `_R`, `Ship_Shiled_Holder`) | none |
| Other | a MeshCollider on `Ship_Hull`; deck clutter: weapon rack, 3 cups, 2 bedrolls, 3 sacks, 2 barrels, 2 crates, 2 buckets, 2 rope coils | |
| Material | `SMV_BasePallete` (URP/Lit) | |

**How she is shown** (`StageShip`):
- **Scaled as one by 7.7 / 17.88 = 0.431.** That gives 7.7 m by 2.16 m: the real Gislinge boat's beam almost exactly
  (the class table's 1.5 m is narrower than the find).
- **Floated on the pack's own waterline**, scaled with her: the model's origin 0.52 m under the stage's water
  (`kit.modelWaterline` 1.2 x 0.431). Her rail then stands (2.09 - 1.2) x 0.431 = **0.38 m** out of the water; the class
  table says 0.34 m. (Floating her by her class's draught instead, keel 0.25 m down, would leave 0.83 m of side and
  the whole of a bottom that was never meant to be seen.)
- **Switched off** (`kit.hide`): the shields and their rails, all seventeen oars, all thirteen benches, and the deck
  clutter. At 0.43 they would be 39 cm shields and benches 37 cm apart beside full-size men.
- **Ours, at full size** (`FitOut`): three thwarts (`Plank_4`, 0.9 m apart, the first 1.3 m ahead of amidships), and six
  oars (`Oar_1`, 2.65 m, its origin already at the oar port), one pair to a thwart, the thole 0.3 m aft of it. Six oars
  and four men is three men each pulling a pair and one at the steering oar, which is what the class row's
  "a six-oar boat, sexaeringr" and "crew": 4 say. The lead's brief said four oars: question 1 at the end.
- **Seats** for the crew rig: `Seats` (three, facing aft, on the thwarts) and `HelmSeat` (starboard quarter).
- **Sail**: `Ship_Sail_Open` when she is under sail, braced round the mast (0.6 of the wind's angle from astern, to 55
  degrees); `Ship_Sail_Close` otherwise. Under sail she heels to leeward, up to 10 degrees.
- **Oars**: out and sweeping at 26 strokes a minute under oars (28 degrees either way, dipping about 15 degrees to reach
  the water from the rail); shipped along the boards otherwise.

**Where she still lies about the player's boat**, to be said plainly:
- her mast top is about 3.6 m over the water; the table says 5 m. A longship's rig is low for her length;
- she draws 0.69 m here where her class draws 0.25 m. It is under water and not seen, but the chart lets her over
  shoals this hull would touch;
- her stems rise 2.0 m; a working boat's do not;
- her boards are 0.11 m over the water and her thwarts 0.17 m, the rail 0.2 m above them: the men sit low in her;
- a longship's row of oar ports shows along her sheer strake (a small boat rows on tholes);
- her yard stays at the masthead with the sail furled; a small boat's is lowered.
At 270 pixels and in a PSX look most of this will not read. The length, the beam, the sail's shape, the crew's number
and the oars' number are right.

**For a larger class** (the roster's `norse_longship`, `norse_allround_ship`): the same code shows the model nearer its
own size, and the kit's `hide` list should then be shorter (her own benches and oars are right at full scale). Not built.

**What a truer model needs, later**: 7.7 m by 2.1 m, six strakes a side, 0.6 m deep amidships, low stems, three thwarts,
tholes not ports, a mast of about 5 m that can be seen lowered, a yard that lowers, a side rudder; about a thousand
triangles and the pack's palette so it sits with the men.

## 6. The hook

The frame's ship window (FrameRoot.cs, Build, "bottom right"): a slip `frame-ship` holding `FrameShipView` (class
`frame-ship-view`, which paints the drawn stand-in in `generateVisualContent`, 15 times a second) and a caption
(`frame-ship-where`, `frame-ship-wind`) that FrameRoot.FillShip fills.

`ShipStageHook` changes none of that:
- **It starts itself**: a `sceneLoaded` handler (set in SubsystemRegistration, so it survives Play without a domain
  reload) and an AfterSceneLoad call. It does nothing unless there is a ChartWorld, a graphics device, and the kit in
  Resources. So the 3D levels, batch runs with -nographics and a checkout without the kit are untouched.
- **The small picture**: `FrameRoot.Current.Root.Q(null, "frame-ship-view")`, and an `Image` added as its child, filling
  it. A child draws over its parent's own painting, and the window clips to its rounded border (`overflow: hidden` in
  Frame.uss), so the stand-in is covered. The caption stays the frame's.
- **The frame rebuilds its elements when it is enabled** (FrameRoot.OnEnable calls Build). The hook notices (its
  element has no panel any more) and attaches again.
- **The Swap button**: a Button at the top left of that window, dressed inline (Frame.uss is the frame builder's file,
  and the second button sits on the map's document, which has none of the frame's styles). Label "Swap" for now; no key
  (the frame's keys are in FrameRoot).
- **The chart small**: part 2, "The swap". `MapView.Current.GetComponent<UIDocument>()`: its `rootVisualElement`'s
  left, top, width and height (MapView.OnEnable sets left, top, right, bottom to 0 once and never again), and its
  `sortingOrder`. Put back exactly on swapping back.
- `ShipStageHook.Off = true` turns the whole thing off (a test of the frame that wants the old picture).

Two optional one-line edits to the frame's files, for its builder, neither needed:
1. FrameRoot.Update: `if (ShipView.ShipStageHook.Current == null) shipView.MarkDirtyRepaint();` saves painting a stand-in nobody sees.
2. A key for the swap in FrameRoot.Keys: `ShipView.ShipStageHook.Current.Swapped ^= true`.

## 7. The test

`ShipStageTests` (PlayMode, in the style of ChartGameTests and WaterLookTests: a RenderTexture 960 x 540, ARGB32, sRGB,
read back and written as PNG under Captures/):

- `SevenPicturesOfTheStage`: builds the stage in the test's empty scene from the kit with a fake source, and for each
  state settles it (`ShipStage.Settle`: the sea warm-started, the boat, jetty and camera put where they want to be),
  runs 45 frames at a fixed 30 a second, and saves `Captures/ship_view/<name>.png` and `<name>_small.png` (the window's
  own texture), with `ship_view.txt` (Hs, Tp, whitecaps, the Sun's and Moon's height, the picture's mean and spread).

| Picture | State |
|---|---|
| 1_river_by_day | River, 21 June noon, 52 N, 5 m/s from 230, heading 300 at 2 m/s under sail |
| 2_coast_at_dusk | Coast, 53.4 N, 19:54 (Sun 3.4 degrees up), 8 m/s from 250, heading 020 at 2.8 m/s; sea 0.9 m |
| 3_open_sea_in_a_gale | Open sea, 55 N, day 260 at 14:00, 19 m/s from 285, lying to, overcast 0.9, rain 0.7, sees 4 km; sea 7.4 m |
| 4_a_fog_bank | Coast, 07:00, 2 m/s, fog 0.85, sees 120 m |
| 5_night_with_a_moon | Open sea; the test finds the darkest hour with the fullest, highest Moon in the five months after midsummer 793 (it is day 297: full, 46 degrees up); 6 m/s |
| 6_moored_at_the_jetty | Port "dorestad", moored, 10:00, 3 m/s |
| 7_under_oars | River, 17:30, 0.8 m/s of wind, 1.6 m/s under oars |

  It fails by itself if more than 1 % of a picture is error-shader pink, if a picture is one flat colour, or if the
  jetty is not within 3 m of alongside in picture 6. Everything else is for the lead's eye.
- `TheStageBringsNothingOfALevel`: no LevelInfo, LevelWeather, HdrpSea, WaterSurface, collider or AudioListener; the
  camera disabled and not MainCamera; every object on the stage's layer; it drew.
- `TheStageSkyIsTheClocksSky`: StageSky against Sky.Compute for the same place, and lower at 62 N.
- `TheWavesCarryTheSeaModelsHeight`: 12 m/s over 300 km raises more than 1.5 m (it gives 4.0) and the six waves share
  that energy; a river's fetch stays under 0.25 m.

Run: `-runTests -testPlatform PlayMode -testFilter ShipStageTests`, with a graphics device (not -nographics).

## 8. The order of building

Half an hour each. Each ends with something that compiles and can be looked at.

1. **Files in, kit made.** Copy the eleven game drafts to `Assets/Kapman/Code/Game/ShipView/`, `ShipStageKitBuilder.cs`
   to `Assets/Kapman/Editor/Game/`, `ShipStageTests.cs` to `Assets/Kapman/Tests/PlayMode/`. Compile. Run Kapman > Ship
   view > Make the kit. *Look at:* the log says "complete" or names what is missing; `Assets/Kapman/ShipView/Materials`
   has seven materials, none pink; the kit's prefab fields are filled.
2. **The three quick tests.** `TheStageSkyIsTheClocksSky`, `TheWavesCarryTheSeaModelsHeight`,
   `TheStageBringsNothingOfALevel`. *Look at:* green; this is the first time the stage is built in Unity, so read the log.
3. **The first picture.** Run `SevenPicturesOfTheStage`; open `1_river_by_day.png`. *Look at:* is it upright; is there
   a horizon; is she the right size and at the right depth; is anything pink or black; is the sail bellied away from
   the wind. Fix before going on (risks R1 to R5).
4. **Sky and exposure**, from pictures 2 to 5. Tune in the kit: `exposureFloor`, `exposureBias`, `moonBoost`; in
   ShipStage.SkyAndAir: the Moon's disc tint, the fog's top, the cloud's tint. *Look at:* dusk is warm and low, the gale
   is dark and wet, the fog hides the horizon, the night shows a Moon and a boat.
5. **The boat**, from pictures 1 and 7. Tune in the kit: `modelWaterline`, `deckHeight`, `gunwaleHeight`, `mastAhead`; in StageShip: the
   thwarts, the oars' sweep and dip, the brace's sign. *Look at:* thwarts sit on the boards, blades reach the water on
   the pull and clear it on the way back, the sail turns about the mast and not about the hull's middle.
6. **The jetty**, from picture 6. Tune StageDock's `deckTop`, `gap`, the props. *Look at:* boards level and inside the
   posts, she lies clear of them, the lines reach, the bank meets the boards.
7. **In the chart.** Play L_Chart. *Look at:* the window shows the stage; the caption still reads; the frame time with
   `ShipStageHook.Off` true and false (this is the cost of the second camera: write it down).
8. **The swap.** Press Swap. *Look at:* the stage fills the screen behind the frame; the chart is in the window, still
   pans and zooms, and its Swap button brings it back; open the chest and the title while swapped.
9. **Coming in and leaving.** Set a course Dorestad to Traiectum at 1x, then at 4x. *Look at:* the jetty comes up
   ahead without a pop or a slide across the water; she ends alongside; casting off leaves it astern. Tune
   `berthWithin` and the two gains in DockStep.
10. **Nothing else broke.** ChartGameTests and the EditMode tests. Their chart pictures now carry the 3D window.
    Commit.

After that, in any order: the looks and the crew rig (the other builder's, part 9), river banks, tacking, swell from
the engine, snow, the hold's cargo shown aboard, sound (public-domain recordings only; the stage has no listener of
its own, the chart's camera has the one).

### Risks, and what to do if each bites

| | Risk | If it bites |
|---|---|---|
| R1 | The picture is upside down in the Image (a camera's texture on Direct3D) | `small.uv = new Rect(0, 1, 1, -1)` and the same for `big`, in ShipStageHook.Picture |
| R2 | A render request from LateUpdate is refused, slow, or draws a frame late | In ShipStageView.RenderTo use the fallback path always (`targetTexture` + `Camera.Render()`); or set `targetTexture` once and enable the camera on its frames only |
| R3 | The sky is black or unlit in the first pictures (HDRP works the sky's light out over several frames, and this camera draws on few) | Draw every frame for the first 30 after a new texture; the test already waits 45 |
| R4 | The fixed exposure is wrong for the picture | `kit.exposureBias`; if hopeless, Exposure mode Automatic with `limitMin`/`limitMax` from the same numbers |
| R5 | Pink: a pack material the kit has no twin for (the men's, when the crew comes) | `ShipStage.Adopt` already swaps by material name and falls back to a plain brown; add the name to ShipStageKitBuilder.Palettes |
| R6 | `UIDocument.sortingOrder` does not re-sort at run time, so the small chart stays under the frame | Keep the map under the frame and make a hole instead: while swapped set `.frame-ship` and `.frame-ship-view` background colours to clear (inline) |
| R7 | The second camera costs too much | `kit.smallEvery` 3 or 4; `smallHeight` 180; ShadowMaps off in ShipStageView.Off; do not draw while the window is covered |
| R8 | Moving 6,561 points by eight waves on the CPU costs too much | `StageWater.outer` 14 and `cell` 0.6; or the look moves the water in its vertex shader and sets `moveOnCpu` false |
| R9 | A river looks like glass (its waves are 5 cm: the model is right) | The look gives the water ripples; or a scrolling normal map on SV_Water |
| R10 | The 7 m gale looks absurd round a 7.7 m boat | It is what the model says and what she would meet. If the user wants it tamer, cap the height shown in StageSea.Waves |
| R11 | The stage slows or changes the frame builder's ChartGameTests | `ShipStageHook.Off = true` in that test's start |
| R12 | The chart's sample has no sky (a model sampled on its own: `gust`, `visibility`, `cloudTransmittance` are 0) | Already handled in ChartStageSource (1.4 x the wind, `VisibilityKm`, the lighting model's transmittance) |
| R13 | CloudLayer does not draw for this camera, or looks wrong at 270 px | Opacity 0 and let the fog and the Sun's dimming carry the weather; or VolumetricClouds on for the big view only |
| R14 | The stretched log looks wrong as a pile | A plain cylinder in SV_SMV_Weapons |

## 9. Where the other builder's parts plug in

**The PSX looks**: `IShipStageLook` (in ShipStageState.cs), set with `ShipStage.Current.Look = ...`. A look's `Apply` may:
- set `stage.View.smallHeight`, `bigHeight` and `filter` (the picture's pixels and how they are stretched);
- swap materials on everything: `stage.Remat((renderer, material) => ...)`. The water is `stage.Water.Renderer`; its mesh
  carries foam in vertex colour red; the wake's alpha is in its vertex colour;
- move the water itself: `stage.Water.moveOnCpu = false` and read `_StageWaveA`, `_StageWaveB`;
- add a custom pass for this camera only: a `CustomPassVolume` with `targetCamera = stage.View.Camera`
  (CustomPassVolume.cs 64), or a volume on layer 31 (`stage.Volume` is the stage's own, priority 100);
- change the sky: its own volume on layer 31 at a higher priority (a Gradient sky, say).
`Remove` puts it back. Custom passes and custom post-processes are left on in the camera's frame settings for this.
Anything a look spawns should go through `stage.Spawn` or `stage.Adopt` (layer, no colliders, HDRP materials).

**The rowing crew**: `IShipCrewRig`, set with `ShipStage.Current.Crew = ...`. `Attach(stage, ship)` gets `StageShip`:
`Seats` (one in the middle of each thwart, facing aft), `SideSeats` (two more on each thwart), `HelmSeat`, `OarsPort`,
`OarsStarboard` (each oar's origin is at its thole), `OarPivots`, `Deck`, `Gunwale`, `Scale`. Each frame
`Tick(state, dt)` after the boat has moved; `ship.StrokePhase` is 0 at the catch, 0.4 at the release. A rig that wants
the oars in the men's hands sets `ship.OarsDriven = true` and poses them itself; one that sets and furls the sail sets
`ship.SailDriven = true`. The men are at full size under `ship.FitOut`, not under the scaled `ship.Model`.

### The other builder's drafts as they stood at 11:03, and how the two halves join

Their files are in the same folder and the same namespace (`Kapman.Game.ShipView`): ShipViewLook, PsxStyle, PsxSeaGrid
and four shaders; ShipCrew, RowingRig, CrewBody, ShipCrewFeed; an editor tool, ShipViewSeating. **All eighteen game files and both editor files of the two halves were compiled
together into Kapman.Game and Kapman.Editor: no error, no name in both.** They were written before this stage existed, as components
with open fields ("the other half makes these"), not against the two interfaces above. So the join is a short piece of
code the lead writes once both are in (an `IShipStageLook` and an `IShipCrewRig` that wrap their components); what it
has to do, field by field:

| Theirs | Give it | Note |
|---|---|---|
| `ShipViewLook.stageCamera`, `stageRoot` | `stage.View.Camera`, `stage.transform` | |
| `ShipViewLook.hdrpSea` (the sea switched off while the grid sea is drawn) | `stage.Water.Object` | It was meant for HDRP's water. The stage's own water mesh fits the slot. Their style also turns `FrameSettingsField.Water` on for the camera: harmless, there is no water surface |
| `ShipViewLook.gridSea` | a `PsxSeaGrid` on a new child of the stage (through `stage.Adopt`) | |
| it draws the camera itself (`targetTexture` and `enabled` on its own frames) | `stage.View.drawnByLook = true`; `stage.View.shown = look.Output`, again on `look.OutputChanged` | **Two ways of drawing must not both run.** With this set, the stage's render requests stop and the hook shows `View.Picture` |
| `ShipViewLook.Show(width, height)` (screen pixels) | `stage.View.ShownAtChanged += look.Show` | The hook keeps `ShownAt` to the window's size, or the screen's when swapped |
| `ShipViewLook.Recollect()` | `stage.Changed += look.Recollect` | The jetty and the fit-out come and go |
| `ShipViewLook.swaps` (`source`, `hdrp`, `psx`) | `hdrp` from `kit.swaps` (the same three names) | The stage has already put the HDRP twins on (`Adopt`), so their swap starts from those, not from the pack's pink ones: check that their `source` match is by the pack's name and still finds them |
| `ShipViewLook.lightFromClock` | **false**, and set `lightToward`, `lightColour`, `skyColour`, `fogColour`, `visibility` from `stage.SkyNow` and `stage.State` | LevelClock's statics are the sky of 52 N 5 E in the chart scene (part 2) |
| `PsxSeaGrid.SetSea(in SeaState)` | `stage.Sea.State`, each frame | The same model's state; their grid makes its own waves from it |
| `PsxSeaGrid.flow` | add her way each frame: (sin heading, cos heading) x speed x timeFactor x dt | |
| `PsxSeaGrid.level`, `ship`, `waterline`, `hullHalfLength`, `hullHalfBeam` | `kit.origin.y`, `stage.Ship.Root`, 0, `Ship.Length * 0.46`, `Ship.Beam * 0.5` | Their defaults (1.2, 8.3, 2.5) are the longship at full size |
| she must ride their waves while their grid is drawn | `stage.HeightAt = (e, n) => grid.Height(origin.x + e, origin.z + n) - grid.level`, and null again when it is not | The boat, the camera and the wake all ask `stage.Height` |
| `ShipCrew.sailSet`, `sailFurled`, `steerOar` | `stage.Ship.SailOpen`, `SailFurled`, `SteerOar` (their GameObjects), and `ship.SailDriven = true` | Their crew decides sail or oars itself, with its own delays (ShipCrew.Decide) |
| `RowingRig.Oar.thole`, `pivot`, `shaft` | a thole and pivot made at each `ship.OarPivots[i]`; `shaft` = the oar in `OarsPort` / `OarsStarboard`; `ship.OarsDriven = true` | Their rig is one man to an oar, two hands on it. The stage fits oars in pairs to a thwart: with four oars that is two thwarts and four `SideSeats` |
| `RowingRig.Rower.seat` | `ship.Seats` or `ship.SideSeats` | Both face aft, y up, as theirs wants |
| `ShipCrew.inputs` (`CrewInputs`) | in the chart: their `ShipCrewFeed.Read(dt)`. In the test, from the fake state: `moored`, `lyingTo`, `onRiver = place == River`, `nearPort = berthing`, `knots = speed / 0.5144`, `wind`, `windOffBow = |WindOffBow|`, `closestToWind`; `sailKnots` and `oarKnots` from the class | Two readers of the chart game now exist (ChartStageSource, ShipCrewFeed) and two rules for when she rows. With a crew aboard theirs rules, and the stage's `underOars` only matters when there is none |

**The one thing to settle between the halves is the size of the boat.** Their editor tool (ShipViewSeating) makes a
prefab of the longship **at her own 17.9 m**, the men seated on her own benches at her own rail. This stage shows the
hull **at 0.43** (7.7 m) and seats full-size men on thwarts of its own. Both cannot be on the stage at once. The two ways:

- **A, the boat's true size (this plan).** Hull at 0.43, the stage's fit-out, their men and rig placed on
  `ship.Seats` / `SideSeats` and `ship.OarPivots` instead of on the longship's benches (their measured stations of the
  rail and benches are for the full-size hull: times `ship.Scale`, or not used). Four men fill a 7.7 m boat, which is
  the truth of her. The longship's details are then small beside them (part 5).
- **B, the model's true size.** Their prefab as `kit.ship`, with `kit.scaleToClass` and `kit.fitOut` off and `kit.hide`
  emptied: an 18 m longship with her rowers, everything in proportion, working at once with their tool. It is not the
  player's boat: it shows a ship more than twice as long as the one the chart is sailing.

A is what the brief asked for ("not a lie about the player's boat"). B needs no seating of the stage's own. Question 2.

## 10. The drafts

In `Design/ship_view/draft/`, each with its place in the project on its first line. (The same folder holds the other builder's files: the Psx*, ShipViewLook, ShipCrew*, RowingRig and CrewBody sources and the Shaders folder are theirs, as is Design/ship_view/check.)

| Draft | Goes to | Assembly |
|---|---|---|
| ShipStageState.cs (the state, the source, the look and crew interfaces) | Assets/Kapman/Code/Game/ShipView/ | Kapman.Game |
| ChartStageSource.cs | same | Kapman.Game |
| StageSky.cs | same | Kapman.Game |
| StageSea.cs | same | Kapman.Game |
| StageWater.cs | same | Kapman.Game |
| StageShip.cs | same | Kapman.Game |
| StageDock.cs | same | Kapman.Game |
| ShipStageKit.cs | same | Kapman.Game |
| ShipStageView.cs | same | Kapman.Game |
| ShipStage.cs | same | Kapman.Game |
| ShipStageHook.cs | same | Kapman.Game |
| ShipStageKitBuilder.cs | Assets/Kapman/Editor/Game/ | Kapman.Editor |
| ShipStageTests.cs | Assets/Kapman/Tests/PlayMode/ | Kapman.Tests.PlayMode |

**Compile-checked, all thirteen**, outside Unity and outside the project, with the editor's own Roslyn
(`Editor/Data/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll`) and Unity's own response files
(`Library/Bee/artifacts/1900b0aE.dag/Kapman.Game.rsp`, `Kapman.Editor.rsp`, `Kapman.Tests.PlayMode.rsp`): the eleven
were added to the whole of Kapman.Game as it stood at 11:03 on 7 October and it built with no error and no warning from
them; the editor tool and the test were then built against that. (Kapman.Core had to be built first from its current
sources: the other builders' Game code already used a member its compiled copy lacked. If the lead's first compile
fails in FrameRoot.Floats.cs or EncounterPanel.cs on `ShownOption.Tagged`, that is theirs, not these files.)

**Also run outside Unity** (a small console program against the project's Kapman.Core.dll and the drafts): StageSky
against Sky.Compute; the Sun's and Moon's heights and the exposure for the test's hours; the Moon night the test
finds; the sea's height and period for nine winds and places. The numbers in this plan are from that run.

**Not verified at all**, because only Unity can:
- that anything draws: the camera by render request in Play, the sky, the CloudLayer, the fog, the fixed exposure;
- which way up the texture is in a UI Toolkit Image;
- that `UIDocument.sortingOrder` re-sorts a live document, and that MapView is content in a small rectangle;
- every position that came from reading the pack's files instead of looking: the deck's and gunwale's heights, the
  mast's place, the thwarts, the oars' pose, the sail's brace and its sign, the jetty's parts, the bank;
- the cost of the second camera and of the CPU water;
- that the stretched log, the foam ribbon and the rain look like anything.
No shader was written: the drafts use HDRP/Lit and HDRP/Unlit only.

## Questions for the user

1. The class table gives the Gislinge boat six oars and a crew of four (three men pulling a pair each, one steering).
   The brief said four oars and four men. The drafts follow the table. Which is it to be?
2. Your boat is 7.7 m and your model is an 18 m longship. Show the model shrunk to 7.7 m with full-size men in her
   (her true size; the longship's small details look a little odd beside them), or show the longship at her own size
   with her rowers (it looks right, but it is not the boat you are sailing)?
