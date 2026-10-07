# The level chain and the gateways

How a voyage runs through true-size river places and hard-shortened seas with no loading screen and, wherever the geometry allows it, no seam at all. Written 4 October 2026 against the code in `Assets/Kapman/Code` (LevelInfo, LevelExit/LevelEntry, LevelClock, LevelWeather, LevelWater, ShipMotion, OrbitCamera, FloatingOrigin), the GDD's "Levels and phases" and "Level outline" tabs, and the first build's notes. Facts carry their source; estimates are marked (guess); things I derived myself are marked (derived). Revised the same evening after review: the time budget (2.3), the simulation's pace through the chain (2.9), the prepare/cut order against the existing `OnEnable` singletons (2.5), the mouth beat's framing (2.6), the smoke test (4.5) and the first slice (5) were corrected; the facts they rest on were re-verified.

The one-line design: **every join owns one real corridor of water that is baked into both levels; the handover is a single-frame cut made while the ship is inside that corridor, where both levels show the same metres of river or sea; the camera is bound to the ship for those metres.** Where the two sides cannot share a corridor (a change of scale at a river mouth or a strait between seas of different scale), the cut is hidden inside a short "deck beat" that the player asked for anyway: the prompt.

---

## 1. What is true

### 1.1 The chain the game already has, in real numbers

| Leg | Real | Source |
|---|---|---|
| Dorestad (51.972 N, 5.343 E) to Traiectum (52.09 N, 5.121 E) to the Rhine's mouth at Katwijk (52.21 N, 4.38 E), by the Kromme Rijn and the Oude Rijn | about 85 to 90 km: the Kromme Rijn is 28 km (Wijk bij Duurstede to Utrecht), the Oude Rijn 52 km today (Harmelen to Katwijk), and Utrecht to Harmelen about 10 km (guess); the river of 800 wound more. The first build's "131 km" was its lane-graph course with detours round a 480 m mask (commit 4d43142), not the river | https://nl.wikipedia.org/wiki/Kromme_Rijn ("De 28 kilometer lange rivier"); https://en.wikipedia.org/wiki/Oude_Rijn_(Utrecht_and_South_Holland) ("Its present-day length is 52 kilometres") |
| Katwijk to the Thames mouth, open sea | about 225 km | first build's GameClock note (85 Rhine + 225 sea + 120 Thames = 430 km Dorestad to Lundenwic) |
| The Thames mouth to Lundenwic | about 75 km from the Nore to the Strand (guess; the first build counted 120 km from further out) | |
| The whole voyage | about 430 km; about 46 hours under way at 5 knots (430 km / 9.26 km/h), which with nights and tides was three to four days. The first build's "87 h" came from its 1:32 fudge: `SimRandom.cs:63` makes a game hour carry "1,920 x v real metres (5 kn: 4,939 m)", half a true knot, and ai_fleets.md inherited it | derived; ai_fleets.md's 87 h is superseded |
| The game's budget for it | about 30 minutes at 1x, about 8 at 4x | the user, 3 Oct 2026 (GDD) |

So the player's voyage is **30 game hours** (30 real minutes at 1 s = 1 game minute) against about 46 real hours under way. Time is not scaled at all; the shortening is in the metres sailed: the places are at 1:1 and the sea at 1:150 across. AI ships on the same route take the same 30 game hours, because the chain is the simulation's road (section 2.9). The river places are a few per cent of the real river; the rest is cut.

### 1.2 The ship's real pace

| Fact | Value | Source |
|---|---|---|
| Replica clinker ships under sail | 4 to 6 knots (2.1 to 3.1 m/s); rowing about 3.5 knots (1.8 m/s) | project memory, replica trials (first build's seamanship data) |
| The game's ship in a river place | `LevelInfo.shipSpeed` = 1.6 m/s rowing; turns at 14 degrees/s; draught 0.45 m | `LevelInfo.cs`, `ShipMotion.cs` |
| Current at Dorestad | the Lek took about 75 % of the Rhine, the Kromme Rijn 25 %, c. 800 | Research/water_look.md |
| The Kromme Rijn at Dorestad | one main channel 60 to 100 m wide, a 140 to 150 degree bend at the town | Research/water_simulation.md, dorestad_c800_notes.md |

At 1.6 m/s a game minute of river is **96 m**; at 2.0 m/s (rowing with a 0.4 m/s stream) it is 120 m. The GDD's "river places 1 to 5 minutes each" therefore means **100 to 500 m of sailed channel per place at rowing speed**, or up to about 600 m with the current under her. Dorestad's dam field alone is about a kilometre long. The current only helps if `RiverCurrent.pushesShips` is on: it defaults to false, no builder sets it, and only `ShipMotion.cs:106` reads it. This document decides it **on** for river places (2.3), so going downstream she makes 2.0 m/s and coming up 1.2 m/s; the budget below is for the downstream leg. This arithmetic drives where the joins go (section 2.3) and is the first open question (section 6).

### 1.3 How far you can see, and what closes the view

| Fact | Value | Source |
|---|---|---|
| Distance to the sea horizon for an eye h metres up | d = 3.57 sqrt(h) km geometric; about 3.86 sqrt(h) km with normal refraction ("the difference is about 8 %") | https://en.wikipedia.org/wiki/Horizon |
| From the deck (eye 2.5 m) | 6.1 km | derived from the above |
| From the masthead (10 m) or the chase camera at 10 m | 12 km | derived |
| From the orbit camera at 30 m | 21 km | derived |
| Fog | visibility under 1 km; mist is "lesser impairment of visibility"; radiation fog forms on clear calm nights and "is thickest shortly after sunrise"; valley fog can last "for several days in calm conditions" in winter | https://en.wikipedia.org/wiki/Fog |
| Fog days in the Dutch river land | 40 to 90 a year by station (KNMI). How many in 793 is not known (guess: of the same order; the post-1980s fall is tied to cleaner air, so it says nothing sure about 793) | Research/look_realism.md citing https://www.knmi.nl/kennis-en-datacentrum/uitleg/mist |
| A bend closes the view along a channel of width W and centreline radius R. Along the centreline the view closes after theta = 2 arccos(1 - W / 2R); for an eye on the outer bank looking past the inner bank it closes after theta = 2 arccos((R - W/2) / (R + W/2)) | Kromme Rijn, W 80 m, R 300 m (guess): 60 degrees along the centreline, 80 degrees from the outer bank. Dorestad's 140-150 degree bend closes it twice over | (derived; chord geometry) |
| The sea off Holland | a brown ribbon 3 km wide, then grey-green; the Rhine plume drifts north along the coast | Research/water_look.md (van der Hout et al. 2015) |

What this means for a join: on a river, **a bend of 80 degrees or more is a wall by the ground alone; a bend of 60 degrees is a wall only with trees on the outer bank** (the checker takes 60 as the floor and the bake must then plant the outer bank); on the sea, nothing is a wall but haze and distance, so a sea cut must change nothing within 20 km of the camera, or be made while the camera is looking at the ship.

### 1.4 Unity facts that bound the design (fetched 4 Oct 2026)

| Fact | Consequence | Source |
|---|---|---|
| `SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive)` returns an `AsyncOperation`; "If a single mode scene is loaded, Unity calls Resources.UnloadUnusedAssets automatically" (so an additive one does not) | additive loading is the tool; unloading needs a hand call | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html |
| `allowSceneActivation = false`: "Unity stops progress at 0.9, and AsyncOperation.isDone remains false"; while so, "the AsyncOperation queue is stalled" | do not hold a scene at 0.9: nothing else loads meanwhile. Load and activate; hide the level by other means | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AsyncOperation-allowSceneActivation.html |
| Addressables `LoadSceneAsync(key, loadMode, activateOnLoad, priority)`; `activateOnLoad = false` "blocks the AsyncOperation queue, including the loading of any other Addressable assets, until you activate the scene"; unload with `Addressables.UnloadSceneAsync(handle)` | the same limit; Addressables buys nothing for scenes in a single-build game, so plain SceneManager is enough for now | https://docs.unity3d.com/Packages/com.unity.addressables@2.6/manual/LoadingScenes.html |
| `UnloadSceneAsync`: "Assets are currently not unloaded. In order to free up asset memory call Resources.UnloadUnusedAssets." | the unload is two steps, and the second can hitch | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/SceneManagement.SceneManager.UnloadSceneAsync.html |
| `Application.backgroundLoadingPriority`: Low 2 ms, BelowNormal 4 ms (default), Normal 10 ms, High 50 ms of loading per frame | set BelowNormal while sailing (4 ms of a 16.7 ms frame), High while moored | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Application-backgroundLoadingPriority.html |
| `MoveGameObjectToScene`: "You can only move root GameObjects from one Scene to another" | the ship and the camera must be scene roots, or live in a persistent scene | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html |
| "RenderSettings and LightmapSettings ... are specific to each scene"; switching the active scene "replaces all previous settings with the settings from the new active scene" | the active scene must be switched at the cut, and both levels must carry the same render settings (HDRP volumes carry most of it anyway) | https://docs.unity3d.com/6000.2/Documentation/Manual/setupmultiplescenes.html |
| `Terrain.SetNeighbors` "ensures LOD matches up on neighboring Terrain tiles"; "you need to set the neighbors of each Terrain"; `allowAutoConnect` "Specifies if the terrain tile will be automatically connected to adjacent tiles" | only matters if a level is several terrain tiles; two levels never touch as terrains in this design | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Terrain.SetNeighbors.html |
| HDRP water: the surface transform "supports translation/rotation/negative scale in Unity 6"; one current speed per surface; no per-pixel water level | a river place's water can be moved and turned as a whole; a water level that differs between two levels must match at the corridor by data | Research/water_simulation.md |
| HDRP water's simulation time is multiplied by `Time.timeScale` | a pause must hold the game clock, never timeScale (already the rule) | Plans/ai_fleets.md spot check |
| World Streamer 2 (NatureManufacture, USD 60): "additive scene streaming by distance", a "floating point fix" (origin shift), "terrain and object streaming layers", background loading; HDRP, Unity 6000.3 | a continuous-world streamer; this design does not need one (section 1.6) | https://assetstore.unity.com/packages/tools/terrain/world-streamer-2-176482 |
| float32 precision | about 0.5 mm at 5 km from the origin, 4 mm at 40 km (24-bit significand) | (derived) |

### 1.5 What the first build taught (project memory, 27 Sep to 3 Oct 2026)

| Lesson | What the chain does about it |
|---|---|
| One continuous world at 1:32 made rivers narrower than the ship; every fix (boat-scaled widths, bend rules, lane gates) was a patch | River places are 1:1. There is no width rule any more |
| Runtime placement of ports put berths on land and courses over the flats | Ports are placed by an editor tool, checked by a test, then hand-finished; nothing is placed at runtime |
| Floating-origin shifts lost the ship's place and the wake's phase; the fly camera sank at 1:4 | No level is bigger than about 6 km, so the origin never shifts inside a level (section 2.5). The shift is replaced by one rigid transform at the cut |
| Tile gaps between LOD levels of the land | Two levels never meet as geometry. The only shared ground is the corridor, baked once and copied into both |
| HDRP water time follows timeScale | pause = hold the clock |
| Opening a second scene mid-batch broke the URP render graph | one level per headless run stays the rule for shots; runtime additive loading is a different path and must be smoke-tested in Play |

### 1.6 How others do scale (brief and honest)

Of the four games named in the brief I could verify little by page fetch today (the web-search budget was spent), so this is short.

- **Sea of Thieves**: one shared seamless sea; the designer expected "a player will see another player-controlled ship every 15 minutes to half an hour" (https://en.wikipedia.org/wiki/Sea_of_Thieves). The whole map is a few kilometres across and nothing is to true scale (guess). Lesson: a sea reads as vast through emptiness and weather, not metres.
- **Assassin's Creed Valhalla**: England, Norway and Vinland in one streamed world; "Eivor's longship acts more as a means of travel when performing raids" and River Raids are "replayable raiding locations in new regions of England" (https://en.wikipedia.org/wiki/Assassin%27s_Creed_Valhalla). The rivers are widened to carry the ship and the country is compressed (guess on the ratio). Lesson: the user's own reference, and the thing the GDD says we are smaller than in detail.
- **Red Dead Redemption 2**: five states in one world; "Stagecoaches and trains can be used to travel" (https://en.wikipedia.org/wiki/Red_Dead_Redemption_2). Continuous streaming with no loading in play is (knowledge, unverified today). Lesson: fast travel is a diegetic vehicle, not a menu; our hauls and the prompt follow that.
- **Uncharted Waters** (Koei, 1990s): the sea is an abstract map with days ticking and ports as separate screens (knowledge, unverified today). Lesson: the opposite pole; the chart mode is our heir to it, the 3D chain is not.

The chain is the middle way: **streamed continuity wherever there is something to see, a cut wherever there is not.**

---

## 2. The model

### 2.1 Vocabulary

- **Level**: one Unity scene with one `LevelInfo`. Two kinds: a **river place** (true size on real ground) and an **ocean level** (shortened hard, `realMetresPerUnit` 100 to 300). A harbour without a river is a river place with a sea surface.
- **Anchor**: a pose (x, z, heading) in a level's local frame where a join meets it, with the water level there.
- **Corridor**: the stretch of water shared by both sides of a join: a polyline of the channel, 150 m either side of the anchor on a river (300 m on the sea), baked from one real place into both levels.
- **Join**: a pair of anchors plus a kind, a hide rule and the real route it stands for.
- **Cut**: the single frame in which the ship, the camera and whatever else is in the corridor are moved from level A's frame to level B's, by one rigid transform, and A's level stops drawing.
- **Graft**: the 100 to 200 m of ground on one side of a corridor where the corridor's real ground blends into the level's own real ground. The one place in the world that is not real ground.
- **Carry**: the state that crosses with the ship.

### 2.2 The data: `Assets/Kapman/Data/chain.json`

One file under git holds every level, anchor and join. The builders read it to place anchors and bake corridors; the runtime reads it to stream and cut; the chart reads it to map progress onto the real route; the tests read it to prove the graph is sound.

```json
{
  "version": 1,
  "levels": [
    {
      "id": "dorestad",
      "name": "Dorestad",
      "kind": "river_place",
      "scene": "L_Dorestad",
      "bake": "levels/dorestad",
      "realMetresPerUnit": 1,
      "layer": "Level0",
      "latitude": 51.979, "longitude": 5.339,
      "frame": { "crs": "EPSG:28992", "origin": [149140.0, 440840.0], "yaw": 0 },
      "water": { "base": 2.44, "tide": false, "currentMps": 0.4, "pushesShips": true, "preset": "river_brown" },
      "state": "generated",
      "ports": ["dorestad"],
      "anchors": [
        { "id": "down", "pos": [2100.0, 2650.0], "heading": 318, "waterLevel": 2.40, "tideShare": 0,
          "corridor": [[2250.0, 2500.0], [2100.0, 2650.0], [1960.0, 2790.0]] }
      ],
      "chartRoute": [[51.972, 5.343], [51.985, 5.325]]
    },
    {
      "id": "northsea_south",
      "name": "The southern North Sea",
      "kind": "ocean",
      "scene": "L_NorthSea",
      "bake": "levels/northsea_south",
      "realMetresPerUnit": 150,
      "layer": "Level1",
      "latitude": 51.8, "longitude": 2.6,
      "frame": { "crs": "kapman_world", "origin": [0, 0], "yaw": 0 },
      "water": { "base": 0, "tide": true, "preset": "sea_south" },
      "state": "generated",
      "ports": ["lundenwic", "dover", "walichrum", "witla", "quentovic"],
      "anchors": [
        { "id": "rhine_mouth", "pos": [2860.0, 1410.0], "heading": 250, "waterLevel": 0,
          "prompt": "Sail up the Rhine to Dorestad" },
        { "id": "thames_mouth", "pos": [420.0, 1180.0], "heading": 280, "waterLevel": 0,
          "prompt": "Sail up the Thames to Lundenwic" }
      ]
    }
  ],
  "joins": [
    { "id": "dorestad_down", "a": "dorestad/down", "b": "traiectum/up",
      "kind": "bend", "hide": "bend", "graftIn": "b", "realKm": 21.0, "twoWay": true },
    { "id": "oude_rijn_mouth", "a": "katwijk_mouth/sea", "b": "northsea_south/rhine_mouth",
      "kind": "mouth", "hide": "deck_beat", "realKm": 3.0, "twoWay": true,
      "promptIn": "Sail up the Rhine to Dorestad", "promptOut": "Stand out to sea" },
    { "id": "dover_strait", "a": "northsea_south/dover", "b": "channel/dover",
      "kind": "strait", "hide": "none", "realKm": 0, "twoWay": true },
    { "id": "jutland_haul", "a": "eider_hollingstedt/haul", "b": "schlei_hedeby/haul",
      "kind": "haul", "hide": "scene", "realKm": 15.0, "twoWay": true, "maxTonnes": 3 }
  ],
  "routes": [
    { "id": "dorestad_lundenwic", "budgetMinutes": 30,
      "levels": ["dorestad", "traiectum", "katwijk_mouth", "northsea_south", "nore", "lundenwic"] }
  ]
}
```

A level's sailed metres (the channel length between its two anchors on a route, or berth to anchor at the ends) come from its `channel` and anchors, so a route's minutes are computable from this file alone (2.3, `ChainDataTests.RouteMinutes`).

Rules the checker enforces (section 4.5):

| Rule | Why |
|---|---|
| Every `a`/`b` names an existing level and anchor | a dangling join is a loading screen waiting to happen |
| `twoWay` joins have a corridor on both anchors; the two corridors are the same real ground (the bake test compares heights) | the cut is only invisible if the metres match |
| A `bend`, `narrows`, `reeds` or `strait` join needs `realMetresPerUnit` equal on both sides | a cut across a change of scale is always visible |
| A `strait` join between ocean levels has yaw 0 on both frames and a translation that is a whole multiple of the sea's largest wave patch | so the FFT wave field lines up across the cut (section 2.4) |
| A `mouth` join has `promptIn` and `promptOut` | the mouth is the one join the player chooses |
| Every anchor's water depth under the corridor is at least the player's draught plus 0.3 m at the lowest water the level can show | no ship stops dead in a corridor ("Too shallow ahead: she stops.") |
| `water.tide` is equal on both sides of every corridor join, and both anchors of a level sit where the level's `tideShare` is 0 or 1 | `LevelWater` moves a whole level's water with the tide; if one side moved +-1 m and the other stood still, the ship would step up or down at the cut. Where a river road crosses its tidal limit (the Oude Rijn's, the Thames's), the limit lies **inside** one level: that level's anchors carry `tideShare` (1 at the sea end, 0 at the river end) and `LevelWater` lerps the share along the channel parameter, so the water is still at one anchor and fully tidal at the other, and the neighbour on each side matches |
| A `bend`, `narrows` or `reeds` join has equal `water.currentMps`, equal `water.pushesShips` and the same `water.preset` on both sides; `make_corridor.py` writes the corridor's current direction into both levels' direction maps from one channel polyline | `RiverCurrent` has one speed per surface and a direction map per level, `RiverTint` one colour per level: a different speed changes the ship's drift at the cut, a different direction turns the foam flecks, a different preset changes the colour and the ripple |
| `layer` alternates `Level0`/`Level1` along every route (a 2-colouring of the chain; the checker fails an odd cycle and the join is then given a `weather` hide instead) | two resident levels must sit on different layers for the camera's culling mask and the physics mask (2.5); the layer is baked by the builder, never set at runtime |
| The graph is connected: every level reachable from `dorestad` | the 240 ports all sit in one world |
| `realKm` along every route sums to the real route length within 5 % | the chart stays true |

Anchors are in the level's **local Unity metres**, because that is what the builders and the scenes already use (meta.json's `origin_rd` for Dorestad, the ocean bake's own frame for the sea). Real coordinates come from `frame` + `WorldFrame`. The level's `chartRoute` is the real-world polyline (lat, lon) that the level's channel stands for; the chart mapping (section 2.7) walks it.

### 2.3 Kinds of join, and where the join goes

| Kind | Where it is used | What hides the cut | Corridor | Visible transition |
|---|---|---|---|---|
| `bend` | place to place on a river; the common case | a bend of 60 degrees or more with trees on the outer bank; the camera bound to the ship | 300 m of real channel at the bend, apex at the anchor | none |
| `narrows` | where a river closes between high banks, a weir, a bridge of boats, a ferry rope | the banks and the structure; the current quickens so the ship slows and the eye drops to the water | 200 m | none |
| `reeds` | flat marsh with no bend (the Fens, the Wantsum, the Wadden creeks, the Thames flats) | reed walls 2 to 3 m high seen from a camera held at 6 m or lower; aerial haze beyond 5 km | 300 m | none |
| `mouth` | a river's mouth onto an ocean level: a change of scale | the deck beat (2.6): the prompt, the camera comes down to the deck, the crew's call, 3 to 5 s | none; the ship is re-posed to the other anchor | the beat itself |
| `strait` | sea to sea at a narrows (Dover, the Pentland Firth, the Oresund, Messina, the Bosporus) | nothing is needed if both seas are at one scale and the strait is baked into both; otherwise the deck beat | 600 m of open water | none, or the beat |
| `haul` | overland: Jutland, the Dnieper to the Volga, the Volkhov to the Dnieper | a short true-size place of its own (2.8) | none | the haul scene |
| `weather` | the fallback on open sea between ocean levels of different scale when no strait exists | a squall or fog bank the weather system is asked to bring up when the ship nears the anchor; only used if the weather would plausibly allow it (season, wind) | 600 m | none, but weather |

**Where the join goes is set by the time budget, not by the bake's edge.** A bake is 5 km square because the ground should run to the horizon; the sailed channel inside it is only as long as the place's minutes allow. The arithmetic is minutes = metres / (m/s) / 60, with the speeds the code has: `LevelInfo.shipSpeed` 1.6 m/s rowing in a river place, 2.0 m/s with the 0.4 m/s stream under her once `pushesShips` is on (decided on, 1.2), and `OceanBuilder`'s 2.8 m/s on the sea (`OceanBuilder.cs:159`). For Dorestad: the player's berth is at a dam head; the downstream anchor goes about **700 m of channel** below the berth, at the first bend of 60 degrees or more that has trees on its outer bank (to be set in the editor from meta.json's channel), so Dorestad takes about 6 minutes downstream (5.8 at 2.0 m/s; 7.3 at 1.6 if the stream is left off) and about 10 coming home against the stream. Joining at the bake's edge instead, 5 km of channel, would make Dorestad alone 42 minutes at 2.0 m/s and 52 at 1.6. The rest of the bake stays as scenery and horizon.

Worked budget, Dorestad to Lundenwic at 1x (derived; the user's 30 minutes; the sea's 225 km at 1:150 is 1,500 m):

| Level | Kind | Sailed metres | m/s | Minutes | Join after it |
|---|---|---|---|---|---|
| Dorestad | place | 700 (berth to the bend) | 2.0 (1.6 + stream) | 5.8 | bend |
| Traiectum's reach | place | 500 | 2.0 | 4.2 | bend |
| The Old Rhine's mouth (Katwijk) | place | 400 river + 500 sea | 2.0 / 2.8 | 3.3 + 3.0 | mouth (deck beat) |
| The southern North Sea | ocean | 1,500 (Rhine mouth to Thames mouth at 1:150) | 2.8 | 8.9 | mouth (deck beat) |
| The Thames mouth (the Nore, Sheppey) | place | 500 | 2.8 | 3.0 | reeds |
| Lundenwic's approach | place | 600 | 2.0 | 5.0 | arrive |
| | | | | **about 33** | |

The return leg is longer (the river places against the stream at 1.2 m/s add about 5 minutes), which is true to the river and is accepted. Every figure in the table is a design knob, not a fact: the anchors move in chain.json and the budget is a test (`ChainDataTests.RouteMinutes`, 4.5) that fails when a named route leaves its budget by more than 20 %.

### 2.4 The cut: one rigid transform

Each anchor is a pose P = (x, z, heading) in its level's local frame. For a join a -> b:

```
T_ab = pose(b.anchor) * inverse(pose(a.anchor))        // a 2-D rigid transform: yaw then translation
yaw   = b.heading - a.heading
pos_b = R(yaw) * (pos_a - a.pos) + b.pos
hdg_b = hdg_a + yaw
```

At the cut, T_ab is applied to: the player's ship (position, heading, goal point, velocity vector), the camera pivot and its orbit yaw, every AI body inside the corridor, the wake decal's anchor, the floating things near the ship (a logboat, flotsam), and any bird flock within 100 m of the ship (the gulls that follow her, `Birds.cs`: "the flock's ground moves after the player's ship"), which is reparented to B's root so it does not vanish mid-air when A unloads. Everything else belongs to one level or the other and is never moved. Heights are not transformed: the corridor's water level is the same number in both levels by data (`waterLevel`), the ground heights match by the bake test, and the tide is the same on both sides by the checker's `tide`/`tideShare` rule (2.2), so the water does not step at a corridor even on a tidal road.

Why the picture does not change on the cut frame:

| What the eye could catch | Why it does not |
|---|---|
| The ground and banks | the corridor's heightmap patch and cover patch are the same data in both levels, placed at the same relative pose; difference is float error (sub-millimetre) |
| The trees on the banks | the corridor's trees are planted by the same seeded routine (seed = hash(join id)) in both levels |
| The water's height and ripples | both surfaces use the same preset (wind, patch sizes, current); the FFT is a function of time and wind, and both levels read the one clock and one weather. On a river the patches are small (ripples), so a phase shift from the rotation is below notice; on the sea there is no rotation and the translation is a multiple of the largest patch size, so the swell field is identical |
| The water's current and colour | equal `currentMps`, equal `pushesShips` and one `preset` on both sides (checker rule, 2.2), and the corridor's direction map written into both levels from one polyline, so the ship's drift, the foam's direction and the tint are the same numbers before and after |
| The sky, sun, moon, clouds, fog | one clock, one weather, one sky dome (section 2.5): the same numbers before and after. The clock's latitude and longitude are lerped from A's to B's over the next 10 game minutes (or over the deck beat at a mouth), never stepped: between Dorestad and the sea level the longitude differs by 2.7 degrees, which would snap every shadow on deck |
| Exposure | once the camera lives in `L_Boot` (night 2 of the slice, 5), it is one camera never swapped and HDRP's adaptation state carries. In the first slice the camera is still per level, A's is disabled and B's enabled on the cut frame, and the exposure pop is accepted as a known issue |
| The ship's bob | on the cut `WaterSampler.Forget()` is called and the sampler's surface is set explicitly to B's (today it caches `FindAnyObjectByType<WaterSurface>()`, which with two levels resident is any surface, `WaterSampler.cs:26`); `ShipMotion` holds its **own last height** (not A's sample, which is the wrong number at a mouth where the levels' water differs) for up to 3 frames until B's readback is live, so there is no dip into the sine fallback. At a corridor join the two heights are equal by data anyway |
| Sound | the loops (wind, water under the keel, the river) belong to the persistent scene or copy their `AudioSource.time` to the new sources, same clip, same sample: no click. The town's own sources (smithy, hearths) are hundreds of metres away by the time of a cut and faded by distance |
| The cursor's goal | the goal point is transformed with the ship, so "making for there" continues |

**Hysteresis**: a -> b cuts when the ship's corridor parameter s crosses +20 m past the anchor heading towards b; b -> a cuts at -20 m heading towards a. The player may turn round in the corridor as often as they like; both levels are resident while the ship is inside it.

### 2.5 The runtime: a persistent scene and the level scenes

Today each level scene carries its own clock, weather, sky dome, ship, camera and soundscape, all `[ExecuteAlways]`, found by statics (`LevelInfo.Current` by `FindAnyObjectByType`, `LevelClock.Current`, `LevelWeather.Current`, `LevelWater.Current`, `HdrpSea.Current`, `RiverCurrent.Current`, `ShipMotion.Player`). With two levels loaded at once, every one of those statics is ambiguous. The design:

| Lives in `L_Boot` (persistent, loaded once) | Lives in each level scene |
|---|---|
| the player's ship (`ShipMotion`, its wake, its water line, its sounds) | `LevelInfo`, `LevelWater`, the terrain, the water surfaces, `RiverCurrent`, `RiverTint`, `HdrpSea` |
| the camera (`OrbitCamera`), the audio listener | the town (`TownLife`), the birds, the moored boats, the local sound sources |
| `LevelClock`, `LevelWeather` + `HdrpWeatherDraw`, `SkyDome`, the global HDRP volume for sky and fog | the level's own global volume for what is local (fog density by place, the mist height) at a lower priority, weight 0 until the cut |
| `LevelSoundscape`'s loops | `JoinAnchor` objects (replace `LevelEntry`), `JoinTrigger` (replaces `LevelExit`) |
| `ChainRuntime` (the streamer), `DevHud`, `Feasts` | a `LevelRoot` on the one root object: saved disabled; its renderers, terrain, colliders and water enabled at prepare, its singleton components only at the cut (two-stage, see the timeline) |

The statics become explicit: `ChainRuntime.Current` is the level the ship is in, and it sets `LevelInfo.Current`, `LevelWater.Current`, `RiverCurrent.Current` and `HdrpSea.Current` at the cut (a one-line setter on each, no more `FindAnyObjectByType`). Every existing `OnEnable` self-assign (`LevelInfo.cs:47`, `LevelClock.cs:122`, `LevelWater.cs:59`, `LevelWeather.cs:101`, `HdrpSea.cs:85`, `RiverCurrent.cs:59`, `ShipMotion.cs:45`, `SkyDome.cs:75`) is **guarded**: it assigns `Current` only when `ChainRuntime.Current` is null (the editor path, a level opened alone) or is this component's level; otherwise it leaves the static alone and waits for the cut. Without the guard, enabling B's root before the cut would give two suns in lux (HDRP lights the deck twice as bright and the exposure pops), two sky volumes at one priority, two clocks pushing shader globals, two `AudioListener`s and two `MainCamera` tags, and `ShipMotion` reading the sea's `WaterLevel` 0 while still in Dorestad (2.44 m): she would stop on "Too shallow" or sink 2.4 m. `LevelClock.Apply` reads latitude and longitude from `ChainRuntime.Current.Info` and lerps them (2.4); between two places 20 km apart the sun moves by a fraction of a degree, below notice.

**The floating origin is not used inside a level.** Every level is under 6 km across; float error at 5 km is half a millimetre. `FloatingOrigin` stays in Core for the chart and the simulation's real coordinates (`WorldPos`, `WorldFrame`), with `shiftDistance` set beyond any level's size. The cut's rigid transform replaces the shift, and it happens once per join instead of every 2 km. This removes a whole class of first-build bugs (the ship losing her place, the wake's phase jumping, the camera sinking).

**Loading timeline** (ocean to place, or place to place):

1. *Decide the next level.* The ship's goal (click-to-make-for) or its chart course names which anchor she is heading for. Moored in port, both neighbours are candidates; the one the chart route favours is preloaded, the other when memory allows.
2. *Preload* when the real-time ETA to the anchor drops under 90 s (distance <= 90 x speed x `LevelClock.TimeFactor`), or at once when the ship is moored. `SceneManager.LoadSceneAsync(scene, Additive)`, `allowSceneActivation` left true, `backgroundLoadingPriority` BelowNormal while sailing (4 ms a frame), High while moored. The scene's one root object is saved disabled by the builder, so nothing in it runs or draws on load.
3. *Prepare* (a coroutine, spread over frames). The level's layer is **baked by the builder** (chain.json's `layer`, `Level0`/`Level1` by parity; `ChainBuilder` sets it on every object: re-layering Dorestad's 409 houses, 651 wells and 119 dams at runtime is work done once instead). `LevelRoot` is **two-stage**: at prepare it enables only the terrain, the renderers, the colliders and the `WaterSurface`s, with the singleton components (`LevelInfo`, `LevelWater`, `LevelClock`, `LevelWeather`, `HdrpSea`, `RiverCurrent`, `SkyDome`, the camera, the listener, `ShipMotion` while per level) left disabled, the local volume at weight 0, the camera's culling mask excluding the new layer and physics queries masked to the current level. The terrain is enabled with `drawHeightmap` and `drawTreesAndFoliage` off: `Terrain.OnEnable` for a 2049x2049 heightmap with 320k trees and 769k detail cells is one call that cannot be yielded and likely costs tens to hundreds of ms (guess; it is a measured row in 4.6), so the enable happens with the draw flags off and Unity's async scene integration spreads the rest at 4 ms a frame; the flags flip at the cut. The water: hiding an HDRP `WaterSurface` by the camera's culling mask is **unverified** (HDRP's water exposes a Rendering Layer Mask for decals and lights, and the surface is drawn by the water system, not as an ordinary renderer), so it is tested in Play first; the fallback is to keep B's `WaterSurface` component disabled until the frame before the cut and accept a few frames of CPU-readback warm-up with the ship holding her last height. Two surfaces with Script Interactions double the CPU wave simulation while both are resident (the first build capped bodies because of 21+ queries a frame), which is budgeted in 4.6.
4. *Cut* (one frame, when s crosses +20 m): copy `LevelCarry` out of A's components; apply T_ab to the ship, camera, corridor bodies and near flocks; flip the camera's culling mask and the physics mask; flip B's terrain draw flags on; disable A's singleton components and enable B's (the guarded `OnEnable`s now take `Current`); `ChainRuntime.Current = B`; `WaterSampler.Forget()` and set its surface to B's; `SetActiveScene(B)`; B's volume weight to 1, A's to 0; start the clock's latitude/longitude lerp; copy loop sample positions if any loop is still per level; in the first slice, disable A's `Camera` and `AudioListener` and enable B's on this frame.
5. *Unload* when the ship is 150 m past the anchor (the corridor's end, where A can no longer be seen) or at the next port call if the frame budget is tight: flocks still parented to A and within 100 m of the ship were moved at the cut, and any other flock near the view fades over 10 s first; then `UnloadSceneAsync(A)` and `Resources.UnloadUnusedAssets()`. The second call hitches; it runs while the view is still inside the bend, and is measured (section 4.6).

If the preload has not finished when the ship reaches the anchor (a slow disk, a cold cache), the cut waits: the ship keeps sailing in A's corridor, which has 150 m of real river past the anchor before A's own continuation begins. At 2 m/s that is 75 s of grace. If even that runs out, the ship is held with "She waits for the lookout" and the event is logged as a performance bug; it is never a loading screen.

### 2.6 The deck beat (the mouth, and any change of scale)

A river mouth joins 1:1 water to a 1:150 coast. In the ocean level, the mouth is a point on a coast where 20 m dunes stand 2.4 m tall (land x0.12 in `OceanBuilder`) a few tens of metres from a 7.7 m ship. No corridor can match that against the mouth place's real dunes 2 km off. So the mouth cut is hidden by **framing, not geometry**, inside the prompt the user already asked for:

1. In the ocean level the ship nears the mouth anchor; the panel offers "Sail up the Rhine to Dorestad" (Enter) and "Sail on" (nothing). In the mouth place, sailing out, the outer anchor offers "Stand out to sea".
2. On Enter, the beat begins: the camera comes down to the deck (2 s ease), the lookout calls the mark ("the dune gap, by the Rhine's mouth"), the sail is trimmed, the oars come out. For 3 to 5 s the frame holds the ship, her people and the water around her. On the sea side the coast is in the haze that the ocean level already draws ("keeps the far coasts a low line", `OceanBuilder`), but on the river side it is not: the Kromme Rijn is 60 to 100 m wide (1.2), so at deck height the banks and the alders are 30 to 50 m off and inside any level or chase framing, and they would pop to dunes or open sea on the cut frame. So **the cut-frame shot is specified**: the camera sits on the deck looking aft and down, pitched at least 25 degrees below horizontal, vertical field of view at most 40 degrees, the horizon out of frame, held for the 0.5 s around the cut. Nothing but deck, people, wake and water is in the picture. It is proved with two `Shots` captures in the first slice (the last river frame and the first sea frame, 5).
3. The cut happens at the middle of the beat: the ship is re-posed at the other level's anchor. The water around her is HDRP sea in both; the wind is the same; the clock's latitude and longitude lerp across the beat's 3 to 5 s so the sun's azimuth does not snap (2.7 degrees of longitude between Dorestad and the sea level); only the far land changed, and the camera is not looking at it.
4. The camera eases back up to the chase view over 2 s and the new place is there.

The beat is also the only place the game ever "takes the camera"; it is short, it is diegetic (the crew does something), and it is skippable after the first time by a tap (the cut then happens behind a 0.5 s dip to the deck, the same shot, held 0.5 s). Multiplayer uses the same beat per client (2.9).

What makes it feel like arrival rather than a trick: the sea state changes for real at a mouth. Entering, the swell dies and the ship steadies (the river place has ripples, not swell); leaving, the first swell lifts her bow inside the beat. The sound goes from gulls and surf to the river's lapping, or back. The water changes colour from the brown ribbon to grey-green over the corridor (Research/water_look.md: the plume is real).

### 2.7 The chart's view of the chain

The 2D chart is the real map, and the ship's mark moves along the real route "in step with her progress through each level" (GDD). The mapping:

- Each level has a `chartRoute` (real polyline) and a `channel` (local polyline, from the bake's meta.json). Progress in the level is the arc-length parameter u in [0, 1] of the ship's projection onto the channel. The mark sits at `chartRoute(u)`.
- Each join has `realKm`, the cut-out reach. While the ship crosses the corridor (300 m at about 2 m/s = 150 s), the mark advances across the cut-out reach at `realKm / 150 s`. For Dorestad to Traiectum that is 21 km in 150 s: the mark glides, it does not jump.
- The clock never changes rate (the user's rule). The chart therefore shows a true position and a true distance, and the days a voyage takes come from the chain's minutes, as the GDD says. The honest consequence: the chart's days are the game's, not history's; the panel never claims "four days to London".
- The chart also draws the chain itself as a thin thread along the route: a solid line where there is a level, a dotted one across a cut reach, a small mark at each mouth prompt and haul. The player learns where places are, and the dotted reaches are where "nothing happens", which is a promise the game keeps.
- Fog of war, places and lands come from Plans/places.md unchanged.

### 2.8 The haul

A haul is a short level of its own, a river place in all but water: 100 to 150 m of true-size portage (the landing beach, the carts or the rollers, the first rise of the track, the far water glimpsed through the trees), and the rest cut like a reach. The real one is Hedeby's: from the Schlei "less than 15 km to the Treene River", where "goods and ships could be pulled on a corduroy road overland for an almost uninterrupted seaway between the Baltic and the North Sea" (https://en.wikipedia.org/wiki/Hedeby); `realKm` is 15. The evidence for dragging whole seagoing hulls is thin and carts for goods are the better-attested practice, so **transhipment is the default**: the cargo goes over by cart (hired men, oxen, a toll) and the player buys or hires a hull on the far water, keeping or selling the one left behind; only a boat up to `maxTonnes` (3) may be hauled whole on rollers, at about 0.3 m/s (guess: a crew of ten with a 2 to 3 tonne hull), so the place takes 5 to 8 minutes, the chart's mark covers the 15 km, and the clock runs at 1x. The panel says which applies ("She is too heavy to haul; the carts will take the lading"). Costs and risks are mechanics in their own documents: the toll, hired men, the hull's strain for a rolled boat (a chance of sprung planks by weight and the track's state in the wet season), the far hull's price, and the week's provisions. Entering a haul is a prompt like a mouth, and uses the deck beat with the crew going over the side.

### 2.9 Who else crosses

The first build's simulation moved every AI ship in real metres per game hour on the real lane graph (Plans/ai_fleets.md, `SimRandom.cs`). That cannot be kept as it stands, because the player's rate through the chain is not one number: Dorestad's 700 m takes her about 6 game hours (1 s = 1 game minute), the cut reach to Traiectum glides 21 km in 2.5 game hours (the 150 s of the corridor), and the sea does 225 km in 9 game hours. A trader rowing at 3.5 knots in real metres, mapped onto Dorestad's `chartRoute` slice, would cross those 700 m in 6.5 game minutes, that is 6.5 real seconds: a body passing at over 100 m/s beside a player doing 2 m/s. The first build hid this with its 1:32 fudge (1.1); at 1:1 places it cannot be hidden. So:

- **The chain is the simulation's road.** An AI ship's progress along a route is accounted in **chain minutes**: for each level, the sailed metres of its channel divided by her class speed in real seconds (the same `shipSpeed` arithmetic as the player's, 2.3); for each cut reach, the corridor-glide time (150 s on a river, 300 s on the sea). Her real position is **derived** from that progress by the chart mapping (2.7), and only the derived real km is used for off-chain things: the chart, markets, news, patrol schedules, "a ship was seen off the Nore". The sim keeps game hours as its clock; it is only the metres that are chain metres.
- A sim ship whose chain position lies inside a resident level gets a body at the level's local pose (u along the channel from her chain progress, lateral offset from her side-of-channel rule), moving at her class speed in local metres, so she passes the player at a believable rate. A sim ship in a cut reach has no body. With both levels resident during a corridor, her body is mapped through T_ab like the player's.
- Ocean levels map by the bake's own affine (origin and 1:150), which is the same thing: the sea's chain metres are its local metres.
- Meetings (sighting, hailing, panels) key off chain distance, which is local distance inside a level and corridor-glide seconds across a reach; both are the same quantity, so a trader and the player who leave Dorestad together arrive at Lundenwic together (the test `ChainSimTests.TraderKeepsPace`, 4.5, within one game hour). The ai_fleets.md numbers (speeds in knots, timetables in game hours) survive; its "87 h Dorestad to Lundenwic" becomes 30 game hours like the player's.

**Multiplayer (the 128-peer case; multiplayer.md decides the rest).** Each client holds its own resident pair of levels; the sim is shared data and the chain positions above are what is shared. AI bodies are per client, spawned from the shared chain position, so two clients in one level see the same trader in the same place without any body being networked. A player's ship is a body only in levels the viewing client has resident: two players meet in 3D only when both are inside the same level, or both inside one corridor with both its levels resident; a player in a cut reach is a chart mark and a sighting line, not a hull. A raider chasing the player across a join keeps chasing in the sim (chain minutes, her class speed against the player's) and re-bodies on the far side where the client has the level resident, so the chase does not break at a bend. The deck beat at a mouth runs per client and does not pause anyone. What multiplayer.md must still decide: who is authoritative for a boarding that straddles a join; whether a player may hold a level resident to lie in wait inside a corridor; and the memory rule when two players' corridors would make three levels resident on one client (the answer here: never three; the preload waits).

### 2.10 Carrying the ship's state

`LevelCarry` is a plain struct, JSON-able, with everything the ship brings across. It is the same struct a save writes.

| Field | From | Notes |
|---|---|---|
| speed, heading, pitch, roll, goal | `ShipMotion` | heading and goal are transformed by T_ab |
| hull (damage per strake, leaks, sprung planks), rig (sail state, oars out) | the ship model (M6) | untouched by the cut |
| cargo (lots: good, quality, kg), purse, debts | the merchant (M3) | untouched |
| crew (each man, his state, his place on deck) | the crew (M6) | untouched |
| game hours, year, speed factor, paused | `LevelClock` | one instance in `L_Boot`; nothing to copy once it lives there |
| weather kind, manual flag, wind vector, temperature, snow lying, flood stage, aurora | `LevelWeather` | likewise; while per-level, a copy at the cut. The lightning dice (`LevelWeather.cs:98`, a `System.Random`) has no copyable state and is not carried: it is reseeded from the game hour at load and at the cut, and nothing is lost because the automatic weather is already a hash of (year, day) in `LevelWeather.Auto`. The flood stage is per level by nature (Dorestad floods, the sea does not): the carry holds the front, the level holds its own water stage |
| the chart position (route id, real km) | `RouteMap` | recomputed from the new level's u after the cut |

### 2.11 Save and load

- A save is gzipped JSON with a version (GDD rule). It holds `LevelCarry`, the current level id and local pose, the sim's ships, markets and the chronicle's state, and per-level deltas (moved boats, bought and sold lots, a moored ship's berth).
- A save inside a corridor stores the side the ship is on by hysteresis (never "half across": the cut is atomic). On load, `L_Boot` loads, then the saved level additively, then the ship is posed; the neighbour preloads as usual. If the saved level's scene is missing (a build with fewer levels), the load falls back to the nearest built level on the chain with a panel saying so, rather than refusing.
- Build-mode edits are **world data**, not save data: `World/Build/<level>.json` under git, applied over the generated scene by the builder (section 2.12). A save never carries them.
- An autosave on arrival in each port (GDD), and the smoke test's save-load round trip after a cut.

### 2.12 240 generated bases, and hand-finish beside them

Every one of the 240 ports and 210 river places gets a generated base before any is finished by hand, so the chain is complete and the smoke test can sail it end to end from the first region on.

1. **Bake**: `Bake/levels/make_place.py <place id>` cuts the square (2 to 5 km, 2.5 m where lidar exists, 5 m elsewhere) from the real-ground store and the 800 AD water, writes `data/levels/<id>/` (height.f32, cover.u8, wood.u8, water_sd.f32, meta.json with the channel polyline), exactly as make_dorestad.py does. `make_corridor.py <join id>` cuts each join's corridor (height, cover, the channel) from ONE real place and writes `data/joins/<id>/`; `make_place.py` for both levels of the join then stamps that corridor into their ground over a 100 to 200 m blend (the graft) on the side `graftIn` names.
2. **Build**: `PlaceBuilder` (a generalisation of `DorestadBuilder`: terrain, land cover, river water from the channel, exits and anchors from chain.json, a town from the port's row in ports data by kind, size and landing, trees by cover, birds, sounds) writes `L_<Id>.unity` and `Assets/Kapman/Levels/<Id>/`. The ocean levels keep `OceanBuilder`.
3. **Hand-finish**: the user works in Build mode in Play (brushes, drawers, the texture painter), which writes `World/Build/<id>.json` deltas: paints, scatters, moved and removed generated objects, props. The level's `state` flips to `hand_finished` in chain.json.
4. **Coexistence**: a rebuild of a `hand_finished` level re-runs the generator and re-applies the deltas; a delta that names a generated object no longer there is dropped and listed in the build report, never silently. A `generated` level and a `hand_finished` one differ only in whether a deltas file exists; the runtime cannot tell them apart. The anchors and the corridor band (corridor polyline +- 20 m) are **locked** in Build mode: the brush refuses them, and a corridor edit (if ever wanted) goes in `World/Build/join_<id>.json`, applied to both levels so they stay matched.
5. **Check**: `ChainChecker` fails the build when an anchor is on land, too shallow, or outside the level; when a corridor's heights differ between its two levels by more than 2 cm; when the two sides of a corridor join differ in `tide`, `currentMps`, `pushesShips` or `preset`, or their direction maps differ over the corridor by more than 5 degrees; when two neighbouring levels share a `layer`; when a port's berth is cut off from its anchor by the channel's depth.

### 2.13 How the model answers season, weather and events

- The chain is static data; what changes with time is what each level shows and whether a join is open. A join can carry `closedWhen`: a river frozen (`LevelWeather.Ice` > 0.7), a flats join at low water (the tide below the corridor's sill: the ship waits, as `TideWait` did in the first build), a haul in the wet season (the track's state), a mouth in a storm (the bar breaks: the prompt says so and offers to lie off).
- The chronicle can close a join or a level by year: Dorestad after 863, a bridge of boats at a narrows after a king builds it, a weir. `chain.json` joins and levels take `from` and `to` years like ports do.
- The weather front crosses with the ship and the sea state changes for real at a mouth (2.6). An ocean-to-ocean join between seas with different prevailing weather (the North Sea to the Norwegian Sea) keeps the front and lets the level's own climate pull it over the next game hours, so the sky never pops.

---

## 3. In the game

What the player sees and hears at each kind of join, and why it is fun rather than plumbing.

**A bend join (most of the river).** The town's noise is behind. The channel curves, the alders on the outer bank close the view, the water under the keel is the same brown, the oars keep their stroke. Coming out of the bend there is a new reach: a different bank, a fish weir, a ferry, the next place's smoke a kilometre off. Nothing announced it. The player only knows a join happened if they open the chart and see the mark glide across the dotted reach. Fun: the river is "the good parts, one after another" (GDD), so every few minutes something is worth seeing, and the player never rows a dull hour; replayability: the same chain reads differently by season (a flooded Dorestad, ice at the narrows, fog at the Nore), by year (the chronicle closes and opens joins) and by what is met in each reach (the sim's traders are data, so they are never in the same place twice).

**A narrows.** The banks come in, the stream quickens, she slows and the crew pulls harder (the oar loop quickens); a ferry rope must be ducked or the ferryman hailed; a weir takes half the channel. Here the seamanship rules bite (keep your side, wait for the one coming down). The cut is in the middle of it and nobody looks for it.

**Reeds and haze.** The flats: reed walls to either hand, a heron, the far line of Sheppey or Walcheren soft in haze; the camera is low because the place is low. The join is simply more reeds. The player feels the flatness, which is the truth of these coasts.

**The mouth, the one seen transition.** The lookout's call, the choice on the panel ("Sail up the Thames to Lundenwic" / sail on), the camera down to the deck, the sail trimmed, the swell either dying or arriving, gulls either coming or going, the water's colour changing over the next minutes. This is the voyage's punctuation, like the first sight of land in any sea story; it is where the crew's talk fits (a line from a man who knows the river, a pilot offered at the mouth, as the GDD's seamanship section has it). Fun: arrival is a moment, not a fade.

**The strait.** Dover's cliffs on one hand, the Boulonnais on the other, the tide race under the keel; the cut is nowhere. Only the chart shows a new sea.

**The haul.** The ship drawn up, the rollers laid, the crew over the side, a toll man, the far water seen through the trees at the top of the rise. Five minutes of a different kind of work and the real risk of a sprung plank. Fun because it is rare, costly, and the only road to the east for a small ship.

**Open water** is where the join is invisible and the crew mode is meant to be used (riddles, reading, games), as the GDD says. The ocean level's 10 minutes are not empty: the sim's ships, the weather, the birds and the day's light are all live, and a join between seas does not interrupt a story being read.

**What the player never sees:** a loading screen, a black frame, a pop in the water, a jump of the sun, a change of exposure (from `L_Boot` on; the first slice's per-level cameras pop once at a mouth, a known issue for a few nights), a sound restarting, the ship's speed or heading changing, the chart's mark teleporting. Each of these is a test (section 4.5).

**Replayability from the chain itself.** The 72 stretches and 14 river roads are a graph. Voyages are routes through it, and a route is made of choices at mouths and straits that the sim's prices, the chronicle's events and the weather make different each time: the Jutland haul against the Skagerrak when raiders are abroad; up the Scheldt or the Zwin; the Wantsum against the Foreland in a westerly. The chart's dotted reaches tell the player the shape of the world honestly, so plans can be made on it. Because AI ships move on the same chain at the same pace (2.9), a trader seen leaving Dorestad ahead of you is the one you overhaul off the Nore, and a raider on your heels at a bend is still on your heels in the next reach; in multiplayer the same rule makes a chase across a join continue rather than reset.

---

## 4. Build plan for Unity

### 4.1 Assemblies and classes

| Class | Assembly / folder | Kind | What it does |
|---|---|---|---|
| `ChainData`, `ChainLevel`, `ChainAnchor`, `ChainJoin` | Kapman.Core, `Code/Core/Chain/ChainData.cs` | plain C# | parses `chain.json` through `Json`; look-ups by id; the graph (neighbours, reachability) |
| `JoinTransform` | Core, `Code/Core/Chain/JoinTransform.cs` | plain C# struct | T_ab from two anchors: `Apply(x, z)`, `ApplyHeading`, `Inverse`, `Compose`; doubles |
| `RouteMap` | Core, `Code/Core/Chain/RouteMap.cs` | plain C# | channel arc length u <-> real lat/lon on `chartRoute`; the corridor glide; the inverse for sim ships |
| `LevelCarry` | Core, `Code/Core/Chain/LevelCarry.cs` | plain C# struct | the carried state, JSON-able |
| `ChainCheck` | Core, `Code/Core/Chain/ChainCheck.cs` | plain C# | the rules of 2.2 as a list of findings (used by the test and the editor menu) |
| `ChainRuntime` | Kapman.Runtime, `Code/Runtime/Levels/Chain/ChainRuntime.cs` | MonoBehaviour in `L_Boot` | the streamer: decide, preload, prepare, cut, unload; owns `Current`; sets the statics |
| `LevelRoot` | Runtime | MonoBehaviour on each level's one root | the two-stage enable (2.5): `Prepare()` enables terrain (draw flags off), renderers, colliders and water surfaces; `Activate()` enables the singleton components and flips the draw flags; `Deactivate()` the reverse; lists which components are singletons; the local volume; the `LevelInfo` |
| `JoinAnchor` | Runtime (replaces `LevelEntry`) | MonoBehaviour | anchor id, pose, the corridor polyline (gizmo), the prompt |
| `JoinTrigger` | Runtime (replaces `LevelExit`) | MonoBehaviour | watches the ship's corridor parameter s; fires preload at ETA 90 s; fires the cut at +20 m; shows the prompt for `mouth` and `haul` |
| `DeckBeat` | Runtime, `Code/Runtime/Levels/Chain/DeckBeat.cs` | MonoBehaviour | the 3 to 5 s camera and crew beat with the cut at its middle; skippable |
| `SaveGame` (chain part) | Kapman.Game | plain C# | writes and reads `LevelCarry`, level id, pose, deltas |
| `ChainBuilder` | Kapman.Editor, `Editor/ChainBuilder.cs` | editor | writes `JoinAnchor`s and `JoinTrigger`s into every level scene from chain.json; sets every object's layer from the level's `layer`; stamps corridor patches; menu Kapman > Chain > Apply chain to scenes |
| `ChainChecker` | Editor | editor | menu Kapman > Chain > Check (runs `ChainCheck` plus the scene-side checks: depth under the corridor, anchor on water) |
| `PlaceBuilder` | Editor (later, M3-M5) | editor | the generalised place builder |
| `ChainSim` | Core, `Code/Core/Chain/ChainSim.cs` | plain C# | chain-minute accounting for AI ships (2.9): progress along a route from class speed and the levels' sailed metres; chain position to local pose and to real km |
| `ChainPlayTests.SailThroughJoin` | `Kapman.Tests.PlayMode` (new assembly, `Tests/PlayMode/`) | `[UnityTest]` coroutine | the smoke of a cut in Play mode (4.5); replaces the idea of a `-executeMethod` batch, which runs in edit mode and cannot drive `ShipMotion.Update`, coroutines or `LoadSceneAsync` over frames (`Batch.Run`, `Shots.cs:391-432`, builds, shoots stills and quits; it never enters Play) |

Data: `Assets/Kapman/Data/chain.json` (git); `data/levels/<id>/` and `data/joins/<id>/` (the bake store, outside git as now); `World/Build/<id>.json` and `World/Build/join_<id>.json` (git).

### 4.2 Where it hooks into the existing code

| Existing | Change |
|---|---|
| `LevelExit` / `LevelEntry` / `LevelTravel` | replaced by `JoinTrigger` / `JoinAnchor`; `DorestadBuilder.ExitAndEntry` becomes `ChainBuilder.Anchors(level)` reading chain.json. `LevelExit.Go`'s `SceneManager.LoadScene` is the one line the whole design removes |
| `LevelInfo.Current` (FindAnyObjectByType) | a setter used by `ChainRuntime`; the editor path (no runtime) keeps the find for `[ExecuteAlways]` |
| `LevelWater.Current`, `RiverCurrent.Current`, `HdrpSea.Current`, `ShipMotion.Player`, `LevelClock.Current`, `LevelWeather.Current`, `SkyDome` | every `OnEnable` self-assign is guarded: assign only if `ChainRuntime.Current` is null or is this level (2.5); set by the streamer at the cut; `ShipMotion.Player` is the boot scene's ship once `L_Boot` exists |
| `WaterSampler` (`FindAnyObjectByType<WaterSurface>()`, `WaterSampler.cs:26`) | gains `Forget()` and an explicit `Surface` set by the streamer at the cut; never finds a surface by itself while a chain is running |
| `RiverCurrent.pushesShips` | set true by `DorestadBuilder` (and `PlaceBuilder`) from chain.json's `water.pushesShips` (1.2, 2.3) |
| `LevelClock`, `LevelWeather`, `SkyDome`, `LevelSoundscape` loops, `OrbitCamera`, `DevHud` | moved into `L_Boot` (night 2 of the first slice; night 1 copies state instead). `LevelClock` gains a latitude/longitude lerp (2.4) |
| `Shots.Shoot` (`OpenScene(path, Single)`, `Shots.cs:246`), Build mode, the editor path | once the clock, weather and sky live in `L_Boot`, a level scene opened alone is unlit and the shot pipeline PROGRESS.md relies on breaks. So `Shots`, Build mode and the editor path open `L_Boot` additively (`OpenSceneMode.Additive`) beside the level, and `ChainRuntime` in edit mode (`[ExecuteAlways]`) sets `Current` to the open level. "Shoot Dorestad still renders" is in night 2's done-when (5) |
| `ShipMotion.Ground` (`Physics.Raycast` with `~0`) | takes the current level's layer mask |
| `ShipMotion.Update` | holds its own last water height for 3 frames when `WaterSampler.Sample` is not live right after a cut |
| `OrbitCamera` | its orbit yaw is stored relative to the ship's heading; `WASD` wander is clamped to 60 m from the ship inside a corridor; `DeckBeat` drives its cut-frame shot (2.6) |
| `Birds` (`Flock.follow`) | a flock within 100 m of the ship is transformed and reparented at the cut; others fade before the unload (2.5) |
| `DorestadBuilder.Build`, `OceanBuilder.Build` | save the level under one disabled root `Level (<id>)` with a `LevelRoot` listing its singleton components; set every object's layer from chain.json; stop creating the clock, weather, sky, ship and camera once `L_Boot` exists |
| Build Profiles | `L_Boot` first, then every `L_<Id>` in chain order, so `Application.CanStreamedLevelBeLoaded` holds |
| `Feasts`, `TownLife` | unchanged; `TownLife` reads the clock as now |

### 4.3 The bake

- `Bake/levels/make_place.py` from `make_dorestad.py` with the place id, box and corridor stamps as arguments; `make_corridor.py` new. Both in QGIS Python under `Bake\run.bat` as now.
- `meta.json` gains `anchors` (local metres, from chain.json so the two never drift: the bake reads chain.json, the builder reads meta.json) and `corridor_source` (which real place the stamped corridor came from).

### 4.4 Scenes

- `L_Boot`: the persistent scene. The ship, camera, clock, weather, sky dome, soundscape loops, `ChainRuntime`, HUD. Opened first in Play; a `[RuntimeInitializeOnLoadMethod]` loads it if a level scene was opened directly in the editor, so the user's habit of pressing Play in `L_Dorestad` keeps working. In edit mode it is opened additively by `Shots`, by Build mode and by a menu item (Kapman > Chain > Open boot beside this level), so a level is lit in the editor and the stills pipeline keeps working (4.2).
- `L_<Id>`: one root object, disabled on save, with `LevelRoot`. The HDRP global volume inside it carries only local overrides (fog density, mist height) at priority 0; the boot scene's volume at priority -1 carries the sky and the weather's clouds.

### 4.5 Tests

EditMode, headless (`Unity.exe -batchmode -nographics ... -runTests -testPlatform EditMode`), no scenes opened; they read `chain.json` and, where present, the bake files:

| Test | Asserts |
|---|---|
| `ChainDataTests.Parses` | chain.json loads; ids unique; every join's anchors exist |
| `ChainDataTests.Connected` | every level reachable from `dorestad`; every port in ports data belongs to exactly one level |
| `ChainDataTests.Rules` | the table in 2.2: scale equality on corridor joins, equal `tide`, `currentMps`, `pushesShips` and `preset` on corridor joins, anchors at `tideShare` 0 or 1, alternating `layer`, yaw 0 on strait joins, prompts on mouths, `twoWay` symmetry, realKm sums within 5 % per route |
| `ChainDataTests.RouteMinutes` | for each named route in chain.json (`routes: [{ id, levels, budgetMinutes }]`), the sum of sailed metres / speed plus corridor glides is within 20 % of the budget (Dorestad to Lundenwic: 30) |
| `ChainSimTests.TraderKeepsPace` | a `ChainSim` trader of the player's class and the player (the same arithmetic, run as plain C#) leaving Dorestad at the same game hour arrive at Lundenwic within one game hour of each other; a trader at 3.5 kn against a player at 5 kn arrives later by the right ratio |
| `ChainSimTests.RealKmIsDerived` | a sim ship's real km on the chart is monotonic along the route and equals the sum of the levels' `realKm` at each anchor |
| `JoinTransformTests.MapsAnchorToAnchor` | T_ab(a.pos) == b.pos within 1 mm; T_ab heading(a) == b.heading; T_ab * T_ba == identity |
| `JoinTransformTests.CorridorParamIsInvariant` | a point 100 m before the anchor in a maps to 100 m before it in b |
| `RouteMapTests.Monotonic` | u in [0,1] gives a monotonic arc along `chartRoute`; the corridor glide reaches the next level's start exactly |
| `RouteMapTests.SimShipInverse` | a real position on the route maps into the level's bounds and back within 1 m |
| `LevelCarryTests.RoundTrip` | every field survives JSON out and in |
| `ChainSaveTests.CorridorNormalises` | a save at s = +10 m is in a, at +30 m in b; load restores pose within 1 cm |
| `CorridorBakeTests.HeightsMatch` | for every join with both bakes present, the corridor's heights sampled in both `height.f32` files agree within 2 cm; skipped (not failed) when a bake is missing |
| `ChainCheckTests.DepthUnderCorridor` | the lowest water the level can show minus the corridor's bed >= draught + 0.3 m |

**The Play-mode smoke** (needs the GPU; the editor's `-executeMethod` batch runs in edit mode and cannot do this, 4.1). A new assembly `Kapman.Tests.PlayMode` holds `ChainPlayTests.SailThroughJoin`, a `[UnityTest]` coroutine, run headless by `Unity.exe -batchmode -projectPath ... -runTests -testPlatform PlayMode -testResults <xml>` (no `-nographics`: the water and the terrain need the GPU; the scenes are listed in a Build Profile so `LoadSceneAsync` can find them). It opens `L_Boot`, loads `L_Dorestad`, puts the ship 200 m before the downstream anchor at 50x `LevelClock.Speed` (test only; the player's cap stays 4x), runs until 200 m past it, and **asserts**: a cut happened once; `ChainRuntime.Current` is the North Sea (Traiectum once it exists); `L_Dorestad` is unloaded within 30 s; the ship's speed and heading on the cut frame equal the frame before within 0.01 m/s and 0.01 degrees after T_ab; the clock advanced by exactly the frames' sum; a save written at +30 m loads to the same pose; exactly one enabled `Camera`, `AudioListener`, `LevelClock` and `LevelInfo` before and after. It **logs** (to `Tests/out/chain_frames.csv`) rather than asserts: every frame's length through the corridor, the `UnloadUnusedAssets` hitch, the terrain-enable call and the second water simulation's cost, because frame times in a batch editor are not the player's and "no frame over 33 ms" would be a meaningless assertion there. The CSV's worst three frames go into PROGRESS.md by hand after each run.

### 4.6 Performance budget (RTX 4070, 1440p, 60 fps, 16.7 ms)

| Moment | Budget | How |
|---|---|---|
| Steady, one level resident | GPU <= 14 ms: level 9 to 10, water 2 to 3 (HDRP water about 4 ms on consoles per Research/water_simulation.md), sky+clouds+fog 2, post 1. CPU <= 10 ms | as the level builds are tuned now |
| Memory, one river place resident | <= 3 GB VRAM, <= 2.5 GB RAM (Dorestad's level assets are 746 MB on disk, its scene 32 MB; the terrain data 119 MB; trees and impostors are the VRAM) (guess on the resident figures: measure in the first slice) | the tree cap 320k and impostors beyond 9 % of the screen, as now |
| Memory, one ocean level | <= 1.5 GB VRAM (65 MB on disk now) | |
| The overlap (two levels resident, 60 to 150 s) | <= 6 GB VRAM, <= 5 GB RAM, leaving 6 GB VRAM for water, sky, post and the shared textures | the budget rule: no river place over 3 GB; the checker's size report |
| The second water simulation during the overlap | <= 1.5 ms CPU for the hidden surface's wave simulation and Script Interactions (guess; measured in the smoke's CSV); if more, B's `WaterSurface` stays disabled until the frame before the cut (2.5) | the smoke's CSV |
| The terrain enable at prepare | one call, `Terrain.OnEnable` with draw flags off; expected tens of ms, feared hundreds (guess); measured in the smoke's CSV; if over 50 ms, the enable is moved to the moored preload only and a sailing preload shows a lower-LOD proxy terrain (a later fallback, not built in the slice) | the smoke's CSV |
| Preload, per frame | <= 4 ms main thread (`BelowNormal`); the prepare coroutine yields under 4 ms except the terrain call above | measured by the smoke test's frame log |
| The cut frame | <= 1 frame over budget, <= 25 ms | the heavy costs moved to prepare |
| The exposure pop in the first slice (per-level cameras) | accepted, listed as a known issue until `L_Boot` (night 2) | |
| `UnloadUnusedAssets` | one hitch, measured; if over 50 ms, defer it to the next port call and accept the two-level residency | |
| Load time, cold, NVMe | 5 to 15 s for a river place (guess); the 90 s ETA rule leaves 6x that | |
| The deck beat | nothing extra: the same scene, a camera move | |

---

## 5. First slice (three nights)

**Goal:** sail from Dorestad's berth to the sea and back with no loading screen, no black frame, and the state carried, on the two levels that exist, with the chain data and its tests in place. It is three nights, not one, under the user's "stable first" rule (compile, test, smoke, commit each step): a Dorestad rebuild alone re-bakes 320k trees and their impostors, and the Play-mode harness does not exist yet.

**Night 1: the data, the tests and the roots.** Nothing the player sees changes.

1. **`chain.json`** with `dorestad` and `northsea_south`, one `mouth` join (`dorestad/down` <-> `northsea_south/rhine_mouth`, hide `deck_beat`, since the two sides differ in scale and there is no Katwijk place yet), the Thames and Walichrum anchors as `mouth` anchors whose targets are not built (so the prompt reads "(not built yet)" as `LevelExit.Built` does now), `layer` `Level0`/`Level1`, `water.pushesShips` true for Dorestad, and one route (`dorestad_lundenwic`, budget 30) with the sailed metres of 2.3. Anchor poses copied from where the builders put the exits today (`plan.exitDownstream`, `mouthIn`), then the downstream anchor moved to about 700 m of channel.
2. **Core**: `ChainData`, `JoinTransform`, `RouteMap` (the glide only), `LevelCarry`, `ChainCheck`, `ChainSim` (the chain-minute arithmetic only); the EditMode tests `ChainDataTests` (with `Rules` and `RouteMinutes`), `JoinTransformTests`, `LevelCarryTests`, `ChainSaveTests.CorridorNormalises`, `ChainSimTests` (against the structs, no scene). Run headless; green.
3. **Roots**: `LevelRoot` (two-stage, listing the singleton components); `DorestadBuilder` and `OceanBuilder` save their content under one disabled root with a `LevelRoot`, set the layer from chain.json, set `pushesShips`, and write anchors from chain.json through `ChainBuilder.Anchors` (the old `LevelExit`/`LevelEntry` still present and working beside them). The `OnEnable` guards go in now, with `ChainRuntime.Current` null so they are no-ops. Rebuild both levels headless (unattended; the Dorestad bake is long).

*Done when:* the EditMode tests pass headless; Play from `L_Dorestad` still works and the old `LevelExit.Go` path still loads the sea; `Shoot Dorestad` still renders; committed.

**Night 2: the cut.**

4. **Runtime**: `ChainRuntime` (preload at ETA 90 s, prepare through `LevelRoot.Prepare` with the camera's culling mask and the physics mask, the cut through `LevelRoot.Activate`/`Deactivate`, `WaterSampler.Forget()`, the unload with `UnloadUnusedAssets`), `JoinTrigger`, `JoinAnchor`, and a bare `DeckBeat` (camera to the specified deck shot of 2.6 for 3 s, cut at 1.5 s, back up). State copied at the cut by `LevelCarry` between the two levels' own clock, weather, ship and camera, with A's `Camera` and `AudioListener` disabled and B's enabled on the cut frame. `LevelExit.Go` is deleted. The water-surface hide by culling mask is tried first in Play; if the surface still draws, the fallback of 2.5 (B's `WaterSurface` disabled until the frame before the cut) goes in the same night.
5. **The two captures**: `Shots` gains `LastRiverFrame`/`FirstSeaFrame` taken from the deck shot's pose at the anchor in each level, to prove the framing shows no bank or dune.

*Done when:* in Play from `L_Dorestad`, sailing to the downstream anchor and pressing Enter at the prompt brings the North Sea up with no loading screen and no black or grey frame; the DevHud's clock, weather line, speed and heading read the same on the first sea frame as on the last river frame; the sea's prompt off Katwijk brings Dorestad back the same way; exactly one sun, one clock and one listener are enabled at any time (the DevHud shows the counts); the two captures show deck and water only; the exposure pop is noted in PROGRESS.md as known; committed.

**Night 3: the proof.**

6. **`Kapman.Tests.PlayMode`** with `ChainPlayTests.SailThroughJoin` as in 4.5, a Build Profile listing the scenes, and the headless command in PROGRESS.md.
7. **Measure**: the `UnloadUnusedAssets` hitch, the terrain-enable call and the second water simulation from the CSV, into 4.6's rows and PROGRESS.md; the save-load round trip at +30 m.

*Done when:* the PlayMode test passes headless and its CSV is committed with the run; the three measured numbers are in PROGRESS.md; committed.

**The nights after:** `L_Boot` with the clock, weather, sky, ship, camera and loops moved out of the level scenes (the exposure pop goes away; `Shots` opens `L_Boot` additively); then the first real corridor join when Traiectum's reach is baked (M1/M2), which is when "you can't spot the seam" (the GDD's M2 test) can first be judged; then `ChainSim` wired into the fleets.

---

## 6. Open questions for the user

1. **The time budget decides where Dorestad ends.** The GDD's 30 minutes means a river place is a few hundred metres of sailed channel (minutes = metres / speed: 1.6 m/s rowing, 2.0 with the stream under her, 2.8 on the sea). I propose the downstream join about 700 m of channel below the berth, at the first tree-walled bend (Dorestad about 6 minutes going down, 10 coming up against the stream), with the rest of the 5 km bake as scenery, and the stream switched on so it pushes the ship (`pushesShips`, off today). The alternative, joining at the bake's edge, makes Dorestad alone about 42 minutes going down (52 at rowing pace with the stream off). The whole Dorestad to Lundenwic voyage then comes to about 33 minutes (2.3). Which, and is the stream to push her?
2. **The deck beat at river mouths.** A change of scale cannot be cut invisibly, so the mouth (and a strait between seas of different scale, and a haul) uses a 3 to 5 s beat on the deck with the crew, inside the prompt you asked for, skippable after the first time. Is one visible kind of transition acceptable, or should mouths get 1:1 pockets inside the ocean levels (costly: a kilometre of true-size coast per mouth, 50 mouths)?
3. **Grafts.** Each river join needs 100 to 200 m of blended bank on its upstream side where the corridor's real ground meets the next place's real ground: the only invented ground in the world (about 400 joins, so 40 to 80 km of bank in all), left for you to hand-finish. Accept, or join only where two real places happen to run together (fewer, longer places; more minutes per voyage)?
4. **Hauls as short places (5 to 8 minutes of true-size portage with a hull-strain risk) or as a chart-only vignette with a toll?**
5. **The chart's mark gliding across cut reaches** (21 km in the 150 s of a corridor) while the clock keeps its rate. The only alternative is the clock jumping, which you have ruled out. Confirm.
6. **AI ships keep the player's pace.** For a trader or a raider to be where the player expects her (overhauled off the Nore, still astern after a bend), the fleets must move in chain minutes, not real km per game hour (2.9): a Dorestad to Lundenwic run is 30 game hours for everyone, and the chart's "real" distances are derived from that. The cost is that the chronicle's and the markets' timetables (a ship a week from Quentovic) are in game hours as before, but a ship's real speed in knots is only ever seen on the chart as an average. Accept?
