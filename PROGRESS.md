# Kāpman 2: progress

The rebuild, started 3 October 2026 from the game design document
(https://claude.ai/code/artifact/ad87e81b-b44c-44fd-b11c-e942dc636ee4). The first build stays in `G:\kapman_unity\Kapman`,
tagged `first-build-final`. The chart of every level, stretch, road and port:
https://claude.ai/artifact/QxFz5D8TewYNJQb6xdNzTD (every dot is one level; year slider 793-1066).

## What's here

- **Unity 6000.6.3f1, HDRP 17.6** (moved from URP on the evening of 3 October: the user wants a real window back in
  time, and HDRP's water, sky, clouds and fog are the best Unity has). Unity's AI assistant is left out. The engine
  decision is final: Unity (Unreal was weighed; its free Megascans turned out not to be free any more).
- **Core code from the first build**, unchanged, with its tests: WorldFrame, WorldPos, FloatingOrigin, Calendar, SkyMath,
  Sky, TideModel, Json, SimRandom (`Assets/Kapman/Code/Core`).
- **The HDRP world** (`Assets/Kapman/Code/Runtime/Levels`): the physically based sky with the real stars (the first
  build's star cube, turned by the clock), the Sun and Moon in lux with the Moon's true phase, volumetric clouds from the
  weather, volumetric fog, local mist over the water, a custom pass for snow, frost and wet ground, an aurora, HDRP's Water
  System for the river (its own mesh, a current baked down the channel, olive-brown and turbid as the research says) and
  for the sea (the first build's colours, wind-driven swell), ships that ride the real water (five probe points), a
  Kelvin wake and strikes as water decals, shore foam and surf, a wet line on hulls and piles.
- **Dorestad, the first river place, at true size (1:1)** (`Assets/Kapman/Scenes/L_Dorestad.unity`), built from the
  excavations (Research/dorestad_c800_notes.md and a three-lens critique: artist, archaeologist, player):
  - the ground c. 800 from the AHN4 lidar, 5.12 km square at 2.5 m, as a Unity Terrain, painted by the real land cover
    (cover.u8 and wood.u8 from make_dorestad.py, thresholds by rank so the basins are mostly alder carr, the lowest parts
    reed) with 13 layers; point bars, cut banks, pools, the fork's spit, the Lek's sand islands and the ford shaped in;
    a dry summer shoal under the harbour with puddles and a shelving beach;
  - the harbour as a comb of earth-and-wood DAMS on an 18 m module (86 parcels, 119 dams, 40-200 m long, longest at the
    bend's apex; caissons, post rows, wattle ends, gangways, bollards), chained storehouses out to the heads, boats drawn
    up in the dry ditches, a dismantled hull on the landing beach; the player's ship moored at a dam head;
  - the vicus 150-200 m deep: 409 houses with paired wall posts, plinths, eaves and ridge rolls, 651 wells, 305 granaries,
    18 smithies, lanes with trodden verges, infield plots, hedges; 37 bow-sided farms with yards, kraals, haystacks;
    99 strip-field blocks with ridge-and-furrow; 4 cemeteries in E-W rows; the Frankenhof church (no tower); the Roman
    fort across the Lek; De Geer as farmsteads; fire baskets at the dam heads (37) with flames that show at dusk;
  - 320,000 terrain trees by land cover (CC0 scans as stand-ins, impostors beyond 9% of the screen), hedgerows along the
    fields and tracks, osier and alder understory, coppice; reed clumps along the shores and in the marsh (769,000
    detail cells); grass, sedge and nettles as terrain details; fish weirs in the pools, logboats, mooring posts;
  - no tide (Dorestad is ~100 km from the sea; the level only changes with the seasons and floods), the clock, feast days,
    sounds and birds (geese, gulls, crows, swallows, herons on the shoal, ducks in the slack water).
- **The southern North Sea, the first ocean level** (`Assets/Kapman/Scenes/L_NorthSea.unity`): the 793 coast from the
  first build's bake, from the Thames and Kent to the Rhine's mouth at Katwijk, shortened 1:150 (492 km real, 3.3 km in
  the game); the chalk coasts, dunes, salt marsh and estuary mud painted by surface and height; 11 places and sea marks
  as real-size blockouts (Lundenwic, Londinium, Dover with the pharos, Walichrum, Witla, Quentovic, Boulogne's lighthouse,
  Reculver, Richborough, Bradwell, the Rhine mouth); traders under way; gulls and geese; fish weirs on the flats.
- **The ways between them:** sail to the end of Dorestad's Kromme Rijn and the panel offers "Sail down the Rhine to the
  sea" (Enter); off Katwijk the sea offers "Sail up the Rhine to Dorestad". For now the next level loads as a scene; the
  seamless phase comes later.
- **The game itself is in both levels** (from the first build, ported 4 October): `--- GAME ---` with the tables, the world sim
  (every craft of 793 on its lanes, traders, raiders, the king's men, fairs), markets and trade, the chronicle firing its
  real events, text events, voyages and courses on the chart, going aground, knowledge that travels by ship, reputation,
  the chest (real texts in their tongues, riddles, hoards), saves, the 2D chart and the panels, and the 14 visual-scripting
  graphs that drive it (`00 Game map (read me)` in the scene). Known gaps: the chart still draws the whole-world bake while
  the levels are shortened; the course follower needs each level's world frame; the ship has no sail or grounding yet.
- **No looks, no filters** (the user, 3 October): the grade is a photograph's: ACES, a touch of contrast, lifted blacks,
  fine grain, SSAO, contact and micro shadows, screen-space GI.
- **Built from the plan so far** (4 October; 144 tests pass): the tick ladder; the chain data with date jumps
  (`Core/Chain`, `Data/Chain/chain.json`, guards on every level singleton, `LevelRoot`); the weather engine (climate
  normals cooled 0.8 C at 793, circulation, synoptic field, wave growth, visibility); the lighting and eye models; the
  ship body (`Code/Ships`: hull probes, hydrostatics, forces, a 6-DoF integrator, `ShipBody` under `ShipMotion`); the
  wildlife calendar (`Core/Wildlife`, `Data/Wildlife`, Wildlife on Dorestad); the encounters core (the toll man's tables).
  Not yet wired into play: the weather engine into LevelWeather, the eye into the exposure, the encounter runner and
  panel. The river current pushes the ship through Dorestad (the user's answer).
- **The meadow pack** (NatureManufacture "Meadow Environment", bought 4 October, not in git: see `Editor/MeadowPack.cs`):
  scanned meadow grass and flowers (clover, daisy, brown knapweed, cornflower, dead grass) on the terrain, grey willow
  for the osier and the carr's understorey, field maple in the hedges, black poplar in the river woods.
- **Decisions** (`Design/DECISIONS.md`, 4 October): Dorestad kept long, the date jumps at joins, deck-shot joins, AI ships at
  the player's pace, full-height storms, CC0-only sound, the river current pushes the ship, and the rest.
- **Built 4 October (afternoon), each tested headless and committed:**
  - the square sail on the ship's body: the live wind (the weather engine), lift and drag by the point of sail, heel,
    reefing at 12/15/20 m/s, the crew rowing only when the sail gives less; the Gislinge makes about 5 kn on a beam reach
    in 8 m/s (the replica's 4.8) and about 4 kn under all oars (the museum's 3.5-4);
  - the modelled soundscape (audio night one): the wind crossfaded on the Beaufort force, the rain three ways, the river
    lapping by the shore map, the thunder at the speed of sound in the air's temperature, far rumbles low-passed by the
    air (ISO 9613-1), near cracks ducking the rest;
  - the chronicle moving the markets (economy night one): the great famine of 793 on at the start (Dorestad's wheat at
    least 1.4x a plain June), Lindisfarne shut ten days after the raid, the markets saved and fingerprinted;
  - the voyage's beats (playability night one): landmarks from the built scene and along the channel (boats off the
    dams, the last dam, the bends), wind changes, the way out, two placeholder lessons, the crew's orders on keys, a
    voyage log as CSV with its gap report (8 beats from the berth to the way out, no gap over 4 real minutes);
  - Dorestad rebuilt with the whole meadow (23 detail layers); the North Sea on its own leaf tints.
- **4 October, afternoon: the game's new shape** (Design/DECISIONS.md, bottom; Design/open_world_isometric.md):
  - one open world of water: the North Sea, Britain and Brittany, Norway, the Baltic and the road to Kiev, Iceland,
    Greenland and Vinland; lengths at **1:40** (the scale test), the ship, the buildings and the rivers' widths true size;
    only the water and a 60 m waterfront band built, fog over the inland;
  - the camera: view B1, 35 degrees down, about 35 m across, a soft lens bulge on the boat, rocking with her; the wheel
    out to the widest view of the water, then on up through the clouds into the 2D map;
  - built since: the weather's sky and sea state from the model, the ship's helm, bilge, anchor and grounding, the river
    reading silty from above, the far trees' tint fixed; pages: the world map, the camera test, the scale test.
- **In progress:** each level's own water from the research (Data/Water/water_793.json: the Kromme Rijn's stream low in
  August and in flood in winter, its colour and clarity by season; the North Sea grey-green with its Secchi depth and tide
  range); audio night two, economy night two and the toll man's encounter in play (written by helper agents, tested and
  committed here).
- **Designs** in `Design/` (4 October, each researched, critiqued and revised): the level chain and gateways, weather and sea
  state, ship physics, wildlife and the farm year, the economy and trade routes, NPC text encounters, audio, lighting and
  atmosphere, playability and fun; `GAME_PLAN.md` stitches them into the build order.
- **Research** in `Research/`: the excavations and the 800 landscape (dorestad_c800_notes.md), water simulation and the
  true look of the water (water_simulation.md, water_look.md), the region's soils, plants and scans (region_textures.md),
  asset packs for both engines (asset_packs.md, unreal_packs.md), the engine verdict (engine_verdict.md), what makes a
  scene read as a real place (look_realism.md), and the HDRP realism recipe (hdrp_realism_recipe.md, when it lands).

## How to build and look

- **Data:** `Bake\run.bat levels\make_dorestad.py` (QGIS Python) writes `G:\kapman_unity\data\levels\dorestad`;
  `Bake\run.bat levels\make_ocean.py` the North Sea.
- **Free assets:** `Bake\run.bat G:\kapman_unity\Kapman2\Tools\Assets\fetch_cc0_assets.py` (Poly Haven, ambientCG; CC0;
  resumable; the list is Tools/Assets/cc0_assets.csv).
- **The level:** menu Kapman > Setup > Apply the project settings (once), then Kapman > Levels > Build Dorestad, then
  Kapman > Levels > Shoot Dorestad (renders into `Captures/`).
- **Headless:** `Unity.exe -batchmode -projectPath G:\kapman_unity\Kapman2 -executeMethod Kapman.EditorTools.Batch.BuildAndShootDorestadPlain -quit -logFile <log>`
  (no `-nographics`: the shots need the GPU). The North Sea: `Batch.BuildAndShootNorthSea`. One level per Unity run.
- **Tests:** `Unity.exe -batchmode -nographics -projectPath G:\kapman_unity\Kapman2 -runTests -testPlatform EditMode -testResults <xml> -logFile <log>`.
- **Re-baking the trees:** delete `Assets/Kapman/Prefabs/Nature/<tree>.prefab` and
  `Assets/Kapman/ThirdParty/PolyHaven/<tree>/<tree>_tree_lod*.asset`; the next Build re-bakes them (and the impostors).

## Keys in Play (Dorestad)

Click the water: make for there · right-drag: turn · wheel: zoom · WASD: wander · F: back to the ship · Tab: 1x/2x/4x ·
Space: pause · T/Y/U: an hour/day/month on (Shift: back) · G: next weather · J: aurora · K: flood · M: sound · H: the panel ·
X: the sail (full, reefed, struck) · V: the oars (as needed, all, shipped) · 1-4: answer the beat on screen.

## Known issues and next

- The buildings are generated blockouts with scanned thatch, daub and planks; the trees are four CC0 scans (stand-ins).
  Free next step: the Megascans European Black Alder pack (22 trees, free on Fab for any engine) and Hazel/Beech/Hornbeam;
  the user adds them to their Fab library and downloads the FBX, then AssetLibrary imports them.
- The ship: the Gislinge is Danish c. 1130. Dorestad's own finds are a clinker ship (Dorestat 1, wood felled after 739,
  riveted; a replica sailed in 2021) and the Utrecht type (Vleuten 1, 734: an expanded logboat 15 m by 1.5 m).
- Animals: birds only (the 2 October rule); livestock needs a pack and the user's say.
- The tree cap (320k) is reached: the woods are thinned to about 40%.
- Shoot each level in its own Unity run.
- The level ends in a straight cut at its edge; a skirt beyond the level is still to make.
- Next: the seamless phases, the geodata masks (Vos 800, BRO soil, geomorphology) in the bake, people and props, the
  user's pack picks, Dorestad's next river places (Traiectum, the Old Rhine mouth).

## The game layer to bring over (from the first build, `G:\kapman_unity\Kapman\Assets\Kapman\Code\Game`, 17,700 lines, 31 test files; tag `first-build-final`)

The 3D side was restarted; the game layer was not, and it works: reuse it, adapt it to the level chain, do not reinvent it.
- **Sim/**: WorldSim (the world's ships and markets each day), TraderBrain, RaiderBrain + RaiderEncounter, Patrols (the king's men), Seamanship (lanes, give-way, waiting for a fair wind), Encounter, HarbourLife, LocalBoats, SimReport/SimStats.
- **Trade/**: Market (supply/demand prices from goods_793.json and trade_rules.json), Trade.
- **Data/**: GameData, Chronicle (chronicle_793.json: real dated events 793-1066 that fire on their day: raids, laws, famines, storms, fairs), Json.
- **Events/**: GameEvents (events_793.json: text events with options and outcomes; the user writes the words in writing.json).
- **Voyage/**: Voyage, Course, VoyagePlanner, Aground (getting her off), TideWait.
- **Known/**: Known (you only know what you have heard, with half-lives), PriceNotes (price-note rune sticks).
- **Reputation** (reputation_793.json: standing per settlement and region), **SaveGame**, **KapmanGame**.
- **Chest/**: Chest, ChestData, Hoards (real hoards, clues in paces), the texts (texts_793.json, in their tongues with PD translations), riddles (riddles_793.json), runes (runes_793.json).
- **Map/**: MapView, MapTable, MapRenderer, ChestPanel (the 2D chart); **UI/**: MarketPanel, TextPanel, MainMenu.
- **Ports/**: PortLife. **Graphs/**: 14 visual-scripting graphs (ports, trade, voyage, encounters, chronicle, town).
- Data copied into `Assets/Kapman/Game/Data/` here: chronicle_793, events_793, goods_793, ports_793, trade_rules, voyage_rules, seamanship_793, ship_classes_793 (every craft's size, rig, oars, crew, cargo, speeds), traffic_793 (how many craft of each class at each harbour by month), voyages_793 (real sailing directions as legs), reputation_793, raiders_793, patrols_793, fairs_793, commissions_793, knowing_793, hoards_793, texts_793, riddles_793, runes_793, wording_793, writing.json, stars_bsc5.csv.
- Plans in `G:\kapman_unity\Kapman\Plans\`: ai_fleets, crew, storms, places, map_play, map_play_ideas, collectables, reputation, multiplayer, ports_more, ports_south, rivers_natural, water_interaction, environment, main_menu.
