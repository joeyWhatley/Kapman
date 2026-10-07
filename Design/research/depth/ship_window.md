# The live ship window

6 October 2026. For the bottom-right window of the third mockup (`ui_mock/template3.html`): "an immersive 3d view of
the trader ship in a simulated environment based on the location of the ship on the map". Read against
`research_indie_and_concept.md`, `ui_direction.md`, `DECISIONS.md`, the sail tank (`KapmanSail/Assets/Sail/Code`) and
`Kapman2/Assets/Kapman/Code`. Nothing here was run in Unity. **(?)** marks what is from memory or not checked: the web
search budget was spent, so only six pages were fetched.

## 1. Findings: what such a window is good for

| Game | The picture of your vehicle | What it does for the player |
|---|---|---|
| **FTL** | The ship cut open is the main screen | The picture is the status report. Fire, a breach, a man running to the engines: no meter is needed. The strongest lesson here. |
| **Silent Hunter III** | A 3D control room and an outside camera | "Unlike other submarine simulations, where the crew is often heard but not seen", you see the men. The outside view is a realism option: a comfort, never a need. |
| **Sid Meier's Pirates!** (2004) | A 3D ship on an oversized map | The ship carries the scene. Wind shows in the sails, storms as passing cloud. |
| **Dredge** | The boat on a simple sea | Black Salt chose "simple sine wave shapes": "our game isn't realistic". Mood comes from the hour and the fog, not from water detail. |
| **Sunless Sea** | A small ship from above, a lamp, the dark | "The ship movement is quite slow. This, accompanied by the music, creates an atmosphere of tension." A slow picture with nothing to read in it is a wait (?). |
| **HighFleet** | Dials, a radar scope, a landing from the side | Praised for "the immersive interface", faulted for "the steep learning curve". Facts that live only in a dial lock people out. |
| **Railroad Tycoon 3** | Free camera down to your own train | The reward for building is watching it run. Up to six detail steps a model, "931 or as low as 55 polygons". |
| **Bounty Train** | The train side-on is the play space (?) | A window you act in is never dead. |

**Good for five things.** (1) State as a picture: load, damage, sail, men. (2) Scale: the chart shows 3,000 km, the
window shows 8 metres of boat. (3) Pace and mood: the fair day against the dark edge. (4) The men are seen, so losing
one is felt. (5) Reward: the thing you paid for, sailing.

**Dead weight in five ways.** (1) It repeats the chart. (2) It loops: the same clip whatever happens. (3) It needs eyes
at the same moment the chart does. (4) It holds a fact that kills and exists nowhere else. (5) It costs frames.
Dorestad ran at about 20 fps on this machine, so (5) is real.

**The rule for Kapman.** Every pixel in the window is driven by a number the simulation already holds. If nothing in
the simulation changed, nothing in the window changes but the waves. Nothing in it is canned.

## 2. What is already built

- **The small scene:** `Tank/TankRig.cs` carries a 60 m water square under the ship. `Tank/TankCamera.cs` has the B1, Deck and Side views.
- **The sea from numbers:** `Tank/TankSea.cs` and `Core/Weather/SeaStateModel.cs` give wave height, period, whitecaps and swell from the wind.
- **The weather where she is:** `WorldSim.WeatherAt` gives wind, rain, fog, snow, three cloud layers, fronts, visibility. `LocalSky` already puts cirrus about 1,100 km ahead of a warm front.
- **Her state:** `ShipMotion` has `SailOrder`, `OarsOrder`, `BailOrder`, `WaterAboard`, `Aground`, `AtAnchor`, `AtBerth`, `Capsized`. `ChartBody` has `Wind`, `WindFrom`, `Knots`, `Sheltering`.
- **Also:** `Sim/Sight.cs` (how far a sail is seen), `TideModel`, `Core/World/Sky.cs`, `Core/Wildlife`, the Gislinge model, 19 CC0 sound files in the tank.
- **Missing:** the men as figures, the shore, damage states, any link from the chart to a 3D scene.

## 3. Proposals

### P1. The one job the chart cannot do: the sky is the forecast

**Rule.** No weather forecast is written anywhere. The window sky is drawn from the same `WeatherSample` the wind
comes from, so the signs come in their true order. The day stick marks a gale only when it is sure; the window shows
it first, in kind.

**Numbers.** Cirrus streaks when a warm front is under 1,100 km; a lowering grey sheet under 450 km; rain in the last
200 km. A front moving 40 km/h takes about 27 real minutes at 1x from the first streak to the rain, 7 at 4x. A cold
front is a dark line on the windward horizon about 60 km out: 90 real seconds. Swell from a far storm arrives before
its wind (`SwellTrain.arrives`).

**The player sees.** Mares' tails at noon. A long swell with no wind. A skipper who watches shortens sail before the
stick says so. One who does not still gets the notch and the card: no fact that kills lives only here.

**Build.** `WindowSky`: three cloud layers as scrolling textures on a dome, their cover and kind from `lowCloud`,
`midCloud`, `highCloud`, `cloudType`, `front`, `frontPhase`. Sun and moon from `Sky.cs`. No volumetric cloud.

### P2. Six places, read from the chart

**Rule.** The place is a class worked out once a game hour from the map, not drawn by hand per port.

| Place | Test | In the window |
|---|---|---|
| River | Land within 300 m on both sides | Two near banks: reeds, willows. Flat brown water with a current line. Oars out going upstream. |
| Estuary | Land both sides, 300 m to 5 km; tidal | Low far banks. At low water (`TideModel`) mud flats and withies show and a man sounds with a pole. |
| Coast | Land on one side within sight (`Sight.HorizonKm`, eye 2 m) | The shore on the true side by her heading. Dune, cliff, wood or marsh by the map's land class. Surf if the wind is onshore. |
| Open sea | No land in sight | Horizon only. Swell. The sky takes two thirds of the picture. |
| Port | `AtBerth` | Moored, sail furled, a jetty, one to three other hulls by the port's size, empty seats for men ashore. |
| Ice | Air under -2 C for 5 days, on a river or the inner Baltic (guess) | Rime on the rigging, floes going by, a man fending off with an oar. |

**Numbers.** The shore is three flat silhouette strips at 150 m, 600 m and the horizon, each 512x64, scrolling at her
true speed: 5 knots is 2.6 m/s on the near strip. The chart mark moves 60 times faster; the window does not, or the
banks would be a blur. About 12 strip sets cover the world. Named sea-marks are single drawings, 64x48, placed at
their true bearing: the first twelve ports get one each.

**Build.** `WindowPlace`: samples `MapLand` at 8 bearings round her; returns the class, the side and distance of the
nearest land, and its land cover. `WindowShore`: three quads with scrolling UVs and a strip atlas.

### P3. Her state as a picture, not a meter

**Rule.** Each line is one simulation value shown one way. A fact with a picture here gets no icon in the HUD.

| Fact | Source | Picture |
|---|---|---|
| Sea state | `SeaState.hs`, `tp`, `whitecaps` | Wave height and length; white tops from force 4; spray over the bow from 2 m |
| Load | Lots in the hold, 0 to 12 | 1.5 cm deeper a lot (guess): 45 cm of side showing light, 27 cm full. Casks and bales stand in her |
| Sail trim | `WindFrom` minus heading, `SailOrder` | Yard braced for the wind; sail full, shaking when she beats, reefed, or down on the yard |
| Oars | `OarsOrder`, wind under 1.6 m/s | Two men rowing, in time with the oar sound |
| Water aboard | `WaterAboard` | One man bailing, two at "all hands"; she rides lower |
| Damage | `Rot.cs`, cards | Four steps each. Sail: whole, patched, split, bare mast. Hull: clean, weeping seam, a strake stove, awash |
| The men | Crew data, hour, mood | Four figures at bow, two thwarts, helm. Asleep at night but for one. An uneasy man sits turned away. An empty seat is empty |
| Land near | `Wildlife/Presence`, map | Gulls within about 30 km of land, none in mid-sea (guess). Brown water off a river mouth |

**Night** (decision 12): stars, outlines, no colour. The lantern shows the boat and blacks out the sea beyond 10 m.
**Fog:** the bow fades at 20 m; the strips are gone; one man stands forward listening.

**Build.** `WindowShip`: a state struct filled each frame from `ShipMotion`, `ChartBody`, `Voyage` and the hold, and one
method that poses the model. Sail: four meshes. Men: see decision 5.

### P4. What the lookout sees

**Rule.** The window and the chart learn of a sail at the same instant, from `Sight.RangeKm`. The window shows what
the chart cannot: how big she is, and whether she grows.

**Numbers.** A sail is drawn at its true angle. In the Quarter view (35 degrees across 160 pixels) a 10 m mast at 2 km
is 1 pixel: too small. So the Deck view uses a long lens (12 degrees): 1 pixel at 8 km, 4 at 2 km, 15 at 500 m, 76 at
100 m. A raider gaining 3 knots closes from 8 km to 500 m in about 80 game minutes: 80 real seconds at 1x. Under 8
pixels it is only "a sail"; over that, its colour and cut say whose.

**The player sees.** The bow man stands and points. The window cuts to the Deck view on that bearing for 3 seconds,
then back. During a chase it stays there.

**Sea-marks fix the place** (decision 11: offshore her position is a fuzzy area). When a named sea-mark is in the
window, click it: the fuzzy area on the chart shrinks to 2 km. This is the one click that changes the game.

**Build.** `WindowSails`: up to 4 billboards from the nearest sighted `SimShip`s, sized by distance. A `Point` pose and
a camera cue. `Landmark` entries in the port data with a bearing test.

### P5. Five things to click, and no more

Click the **sail**: Full, Reefed, Struck in turn. Click the **oars**: as needed, all, shipped. Click a **man**: his slip
opens. Click a **far sail**: its slip opens on the chart. Click a **sea-mark**: P4. Click bare **water or sky**: the next
of three views: Quarter (default: 16 m off her lee quarter, 3 m up, the horizon a third down), Deck (the lookout's
eye), Side (the waterline: load and heel).

**Build.** No physics. Each frame project six anchors on the model to picture pixels; a click takes the nearest within
6 pixels.

### P6. Cards and danger

**Rule.** A card may name a shot: `"shot": "sail_astern"`, `"man:odda"`, `"sky_west"`, `"shoal"`. When the card opens the
clock holds, the window cuts to the shot, and the water goes on at half speed. The window never freezes. A card's
result plays in the window before the card closes: the mast goes, the seat is empty.

**Danger has no red border.** It is read from the men and the camera. Calm: the Quarter view, men seated. Uneasy
(force 6, fog, a strange sail under 4 km): sail reefed, bow man standing. Hard (force 8, a chase under 1 km, water
aboard): the camera drops to 1.5 m and closes to 9 m, all four men working, spray on the picture. Lost: she goes over
(`Capsized`), the window holds on the water for 4 seconds, then shuts.

**Sound.** `L_Chart` has no sound. The window brings it: the tank's CC0 loops, mixed from the same state. Shut the
window and the sound stays at half level.

### P7. When it goes away

Shut by its own corner: the frame goes and the chart shows (your rule: map, not empty frame). It shuts itself in the
winter. While shut, the camera is off and costs nothing.

### P8. Rendering it cheaply in Unity 6

**The scene.** One prefab, `WindowStage`, on its own layer in `L_Chart`: the boat, a 60 m water quad, the dome, three
shore quads, up to four sail billboards. About 15 draw calls, under 20,000 triangles. No HDRP `WaterSurface` in the
first version: at 160x90 its detail is thrown away and its simulation is not free. Use the sine set `ShipMotion`
already rides (`WaterWaves.cs`), four waves fitted to `hs` and `tp`, on a flat-shaded mesh of 64x64 quads.

**The picture.** `new RenderTexture(160, 90, 24) { filterMode = FilterMode.Point, antiAliasing = 1 }`, shown in UI
Toolkit as an `Image`. At the 640x360 base, one picture pixel is one art pixel: it sits in the chart's own grid. In
the full-frame layout E it is 160x100. Offer a "fine" step of 320x180 in the options.

**The camera.** HDRP charges a fixed cost per camera whatever the size. Set `HDAdditionalCameraData.customRenderingSettings
= true` and switch off shadow maps, SSAO, SSR, volumetrics, contact shadows, decals, motion vectors, bloom, depth of
field, motion blur and anti-aliasing (the page confirms the per-camera override; the field names are from memory
(?)). Draw 20 pictures a second: enable the camera one frame in three. `L_Chart` already runs one HDRP camera "that
draws nothing": try giving that camera the stage and the texture, so the window adds no camera at all (?: whether the
overlay UI still draws needs a test).

**Budget.** 1.0 ms a frame on this machine, CPU and GPU together, measured in the Profiler with the window open and
shut. Over 2.0 ms: drop to 10 pictures a second.

**Time.** Waves, oars and men run in real seconds. Sun, cloud, tide and sea state run on the game clock, so at 4x the
sky moves four times faster and the waves do not.

## 4. The first version, and the order after

| Step | What | Done when |
|---|---|---|
| 1 | `WindowStage`, the camera to a 160x90 texture, the `Image` in the HUD, open and shut | The Gislinge rides sine waves in the corner; the Profiler number is written down |
| 2 | Sea from `SeaState`; sky and sun from `WeatherSample` and `Sky.cs`; rain, fog, night | A gale on the chart is a gale in the window |
| 3 | Sail and oars from her orders and the wind; the load line from the hold | Buying 12 lots sinks her; a head wind shakes the sail |
| 4 | Three places: river, open sea, port, with four strip sets; sound | Leaving Dorestad the banks fall away |
| 5 | Clicks on sail, oars and water (P5) | The sail is reefed from the window |
| 6 | The four men and their poses; click a man | An uneasy man is seen before his card |
| 7 | Far sails, the pointing man, sea-marks and the fix (P4) | A raider is watched growing |
| 8 | Card shots, the danger stages, damage steps (P6) | A lost mast is seen, not told |
| 9 | Estuary, coast side, ice, birds | Each of the six places differs |

Steps 1 to 4 are the first version: about three nights (guess). It has no men and no clicks, and it already tells the
weather, the load and the place. If step 1's number is over budget, stop and decide before step 2.

## 5. Yours to decide

1. **Always open, or opened by you?** Recommended: open by default in the full HUD, shut by one click, off in winter.
2. **Sail and oars clickable in the window?** Recommended: yes. They are orders, not steering.
3. **The position fix from a sea-mark (P4).** Recommended: yes; it gives the window a reason to be watched on a coast.
4. **Pixel size.** Recommended: 160x90 in the chart's own grid. The other choice: 320x180, finer and less of a piece.
5. **The men.** One bought low-poly figure in six poses (sit, row, bail, steer, sleep, point), or flat drawn figures turned to the camera. Recommended: flat figures first; they suit 160x90 and you can draw them.

## Sources

- Dredge: https://www.gamedeveloper.com/design/trawling-in-the-deep-how-black-salt-games-made-spooky-fishing-rpg-i-dredge-i-
- Silent Hunter III: https://en.wikipedia.org/wiki/Silent_Hunter_III
- Sid Meier's Pirates! (2004): https://en.wikipedia.org/wiki/Sid_Meier%27s_Pirates!_(2004_video_game)
- HighFleet: https://en.wikipedia.org/wiki/HighFleet
- Railroad Tycoon 3: https://en.wikipedia.org/wiki/Railroad_Tycoon_3
- Sunless Sea: https://en.wikipedia.org/wiki/Sunless_Sea
- HDRP frame settings: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.0/manual/Frame-Settings.html
- FTL (read 4 Oct): https://gdcvault.com/play/1019036/Designing-Without-a-Pitch-FTL
- Bounty Train: from memory, not fetched (?)
- In the project: `Design/research/ui/unity_ui.md` 8.3, `Design/research_pixel_map_3d_boat.md`
