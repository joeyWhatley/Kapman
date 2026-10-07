# Kāpman 2: the game plan

The master plan, 4 October 2026, written after reading the nine system designs in this folder (`gateways_and_level_chain`, `weather_and_sea_state`, `ship_physics`, `lighting_and_atmosphere`, `audio_design`, `wildlife_and_seasons`, `economy_and_trade`, `npc_text_encounters`, `playability_and_fun`), PROGRESS.md, the code in `Assets/Kapman/Code` and the last test runs. It stitches the nine into one game: who owns what, how they talk, the data, the save, the order of building with a done-when per milestone, what to build tonight, the decisions only the user can make, the risks and the frame budget. It does not repeat the nine documents' facts or equations; it cites them by section. Where two documents disagree, this plan decides and says so (2.4). Guesses are marked (guess).

**The baseline, measured today.** The project compiles (every `Kapman.*.dll` in `Library/ScriptAssemblies` is from 05:56 this morning). The first build's game layer is **committed** (c7756df, "The game layer from the first build is in Kapman2") with its 25 EditMode test files; the last headless EditMode run was **100 tests, 98 passed, 2 skipped, 0 failed** (`Logs/tests_game_4.xml`, 05:59) and the PlayMode smoke (`Tests/PlayMode/SmokeTests.cs`: both levels open in Play with the clock running, the ship floating and the world sim made) **2 of 2** (`Logs/tests_play_1.xml`). `GameClock` survives only in comments; `KapmanGame.clock` is a `LevelClock`. The Gentium fonts and the UI stylesheets are in `Assets/Kapman/Game/UI` (the same commit). So the preconditions written into `playability_and_fun.md` section 4 and `npc_text_encounters.md` section 4.0 ("the port is untracked, does not compile, `GameClock` unresolved") are **already met**, and the first slices below start from a green build. Uncommitted today: edits to `SimRandom.cs`, `SimStage.cs`, `Aground.cs`, `TimeManager.asset`, the impostor materials, three design documents and PROGRESS.md; untracked: `Editor/Game/SimHarness.cs`, `Tests/EditMode/ShotTests.cs`, `Tests/PlayMode/`. Step 0 tonight is to run the tests and commit them.

---

## 1. The game in one page

A Frisian merchant in debt at Dorestad, 21 June 793. One clock: 1 real second = 1 game minute, up to 4x, never faster, never a jump the player did not ask for. The ship moves at her real speed through places at true size; the seas between are shortened so that Dorestad to Lundenwic is about 30 real minutes (30 game hours). The world is a **chain** of levels joined at bends, narrows, reeds, mouths and straits with no loading screen; the chain is also the road the AI fleet sails, so a trader seen leaving Dorestad ahead of you is the one overhauled off the Nore. One **weather engine** gives every level and every AI ship the same sky, from measured normals and the chronicle's recorded storms; the sea state follows the wind's history; the ship is a rigid body on that water, handled by orders to a crew, and she can be lost. The **economy** runs on 140 goods in 450 places, moved by the seasons the animals and the farms set, by the chronicle's 582 dated events, and by the player's own cargo. **People** in port and at sea are cards with checks, costs and memory; the words are the user's. **Light** is physical (lux, EV, a dark-adapting eye) and **sound** is CC0 recordings mixed by physics. Birds, fish runs, seals, herds and the farm year make the calendar visible and audible. The player grows by what the house knows (water, prices, tongues, people), not by numbers; heirs carry the line to 1066.

The loop at four scales (playability 2.1): a beat (one question with a cost, every 2-4 real minutes), a voyage (5-60 minutes), a season (evenings in port), a generation. Everything in this plan exists to put the right beat in front of the player at the right time, and to make the world between beats worth looking at.

---

## 2. Architecture

### 2.1 Assemblies

The project has `Kapman.Core` (no references), `Kapman.Runtime` (Core, Input System, HDRP), `Kapman.Game` (Core, Runtime, Input, HDRP, Mathematics), `Kapman.Graphs` (visual scripting), `Kapman.Editor`, `Kapman.Tests.EditMode` (references Core, Runtime, Game, Editor; `-nographics` headless) and `Kapman.Tests.PlayMode` (Core, Runtime, Game; needs the GPU). The rule that makes the whole plan testable: **everything that simulates is pure C# in `Kapman.Core` (or `Kapman.Ships`) and runs in EditMode without a scene; `Kapman.Runtime` holds the MonoBehaviours that read the pure models and push them into HDRP; `Kapman.Game` holds the game state and anything that needs both.**

| Assembly | Holds (new folders in bold) | References | Notes |
|---|---|---|---|
| `Kapman.Core` | Data (Json), Sim (SimRandom, **TickLadder**, **Dice**), Weather (TideModel, **WeatherModel** moved here, **WeatherEngine, ClimateNormals, CirculationChain, SynopticField, ChronicleStorms, LocalSampler, SeaStateModel, IceModel, WaveGrowth, Visibility, FrostModel**), World (Calendar + **Weekday**, Sky, SkyMath, WorldFrame, FloatingOrigin), **Chain** (ChainData, JoinTransform, RouteMap, LevelCarry, ChainCheck, ChainSim), **Light** (LightingModel, EyeModel, FlashModel, SkyEvents), **Wildlife** (Regions, Species, Presence, YearState, Runs, FarmYear, Supply, WildlifeEvents, Omens), **Audio** (SoundContext, SoundLayerDef, SoundscapeModel, SoundSchedule, SoundLibrary), **Encounters** (EncounterDef and the loader, Cond, Effects, IGameEffects, Scheduler, NpcRegistry, NameForge, Tongues) | none | may use `UnityEngine` maths types; never a scene type |
| **`Kapman.Ships`** (new) | the ship as a body and as a thing handled: ShipClassData, HullProbes, Hydrostatics, HullForces, SquareSail, SteeringOar, Oars, Bilge, Mooring, Grounding, Helm, ShipIntegrator, ShipState, **ShipOrders** (the order set of playability 4.2, with the crew's delays), **Landmark, LandmarkSight, PositionBelief** (playability 3.2) | Core | the ship document's `Kapman.Ships` and the playability document's `Kapman.Sailing` are **one assembly** (2.4, decision 10): both are "the ship and her handling", pure, Core-only |
| `Kapman.Runtime` | Levels (as now) + **Chain** (ChainRuntime, LevelRoot, JoinAnchor, JoinTrigger, DeckBeat), **Ships** (ShipBody, ShipSeaState, ShipSounds), **Wildlife** (Wildlife, MigrantSkeins, SeaLife, Insects, FarmDressing), **Audio** (LevelSoundscape2, AcousticProbe, SoundEmitterSet, SoundEvents, SoundMix), Lamps, SkyLightProbe, Hdrp (as now, SnowFrostPass fixed) | Core, Ships, HDRP, Input | raises events; never references Game |
| `Kapman.Game` | as now (Sim, Trade, Data, Events, Voyage, Known, Chest, Map, UI, Ports, Reputation, SaveGame, KapmanGame) + **Economy** (WorldEconomy, Seasons, Silver, Shocks, Tolls, Credit, People, EconomyClock, PortMarker, PortMarketLink), **Encounters** (GameEffects, EncounterRunner, MooringTrigger, EncounterTriggers, EncounterPanel, EncounterSave), **Play** (Beat, BeatScheduler, Pressure, Hazards, StormSurvival, Lessons, DebtClock, PortBeats, VoyageLog, FunTelemetry), **Sailing adapters** (IWaterState/IWindState/IShipState over LevelWater/LevelWeather/ShipMotion) | Core, Ships, Runtime | the economy document's separate `Kapman.Economy` is **not** made (decision 8) |
| `Kapman.Editor` | the builders + **ChainBuilder, ChainChecker, PlaceBuilder, SoundscapeBuilder, HullProbeBaker, Measure sea, Thirty-year report, Profile** | all | |
| tests | EditMode (headless, the bulk) and PlayMode (`SmokeTests`, **`ChainPlayTests.SailThroughJoin`**) | | EditMode gains `Kapman.Ships` in its references |

### 2.2 The one clock and the tick ladder

`LevelClock` is the clock: `GameHours`, `Date` (Julian), `JulianDay`, `SunAltitude`, `Night`, the statics `Speed` (1, 2, 4) and `Paused`, `TimeFactor`. `Time.timeScale` is never touched (every document agrees; HDRP's water would run at 4x of 4x). `KapmanGame` already carries hours across scene loads in statics and raises `HourPassed(double)` and `DayPassed(int)`. The plan adds one pure class, **`TickLadder`** (Core/Sim), that every system's cadence hangs on:

| Tick | Real time at 1x | Raised by | Who listens |
|---|---|---|---|
| frame | 16.7 ms | Unity | the eased weather, HDRP pushes, `ShipBody` substeps (at `Speed`), camera, sound gains, `Flock` motion |
| **minute** (new `MinutePassed`) | 1 s | `KapmanGame` via `TickLadder` | `SeaStateModel.Step`, `BeatScheduler`, `Pressure`, `LandmarkSight`, `PositionBelief.Run` |
| hour (`HourPassed`) | 1 min | same | `WorldSim`, `WeatherEngine.Sample` for the level and the fleet, `Scheduler.Tick` (encounters), `Wildlife` presence and `WildlifeEvents.Due`, `FrostModel.Step`, `MigrantSkeins` draw, `LevelWeather` reads its new target |
| day (`DayPassed`) | 24 min | same | `WorldEconomy.Day` (lazy catch-up), `Chronicle` fires its entries, `NpcRegistry` decay, `IceModel` FDD, `SoundSchedule` weekday |
| **year** (new `YearPassed`) | 146 h | same | `YearState` (hard winter, mast, herring stock, trend), the crew's New Year line, the first-heard log reset |
| **jump** (new `Jumped(from, to)`) | the T/Y/U keys, `Hoards.Dig`, load | `LevelClock` → `KapmanGame` | the ladder runs every whole hour and day between in one frame (720 ticks for a month, about 50 ms: the encounters document's rule, applied to all); `SeaStateModel.WarmStart`; `LevelWeather` snaps; `Wildlife` recomputes |
| **reversed** (new `Reversed(from, to)`) | Shift+T/Y/U | same | nothing ticks; `EconomyClock` ignores it (economy test 11), `Scheduler.lastHour` follows down |

The weather document's rule that 4x is **refused** while a cold front is within 30 km, a cumulonimbus cell within 5 km or the wind at force 8 (weather 2.4) lives in `LevelClock` as `MaxSpeed` set each hour by `LevelWeather`; the chronicle's storms are therefore always met at real pace.

### 2.3 The message board: events between systems

Everything crosses on C# events (static where the raiser is a singleton), never by polling a string, so the visual-scripting graphs can also hear them.

| Event | Raised by | Carries | Listened to by |
|---|---|---|---|
| `KapmanGame.MinutePassed/HourPassed/DayPassed/YearPassed/Jumped/Reversed` | the clock through `TickLadder` | game time | everything in 2.2 |
| `ChainRuntime.Preload/Cut/Unloaded` | the streamer | level ids, `JoinTransform`, `LevelCarry` | `WaterSampler.Forget`, `LevelSoundscape2.BeginPhase`, `KapmanGame` (level change), `EncounterTriggers` (leaving cards), `RouteMap` (chart glide), `Birds` (near flocks reparented), `WorldSim` (bodies through T_ab) |
| `JoinTrigger.Prompt(join)` | the mouth and haul anchors | the prompt text | the panel, `DeckBeat`, `BeatScheduler` (a mandatory beat) |
| `LevelWeather.Lightning(distance)` (exists) + `FlashModel` | the weather | distance, stroke train, cloud-to-cloud | the clock's Lightning light, `HdrpWeatherDraw` (LUT blue, ambient dimmer), `SoundEvents` (thunder at c(T)) |
| `LevelWeather.SampleChanged(WeatherSample)` (new) | hourly | the sample | `HdrpWeatherDraw`, `LevelClock.cloudTransmittance`, `FrostModel`, `ShipSeaState`, `LevelSoundscape2`, `Wildlife`, `EncounterRunner` vars, `LevelClock.MaxSpeed` |
| `Chronicle.Fired(entry)` (new if not present) | on its day | the entry | `Shocks` (economy), `ChronicleStorms`/`WeatherRequest` (weather), `EncounterRunner` tags (`after_raid`, `famine`, `succession`), `SkyEvents` (haze, comets), `YearState` (winters), the chain (`from`/`to` years), `TownLife` (a burned dam), the log |
| `ShipMotion.Arrived` (new, one line) | the ship | nothing | `SoundEvents` (anchor splash or mooring ropes), `MooringTrigger` |
| `MooringTrigger.Moored(port, berth)/CastOff` | the dam heads and landings | port id | `EncounterRunner` (arrival set, visit draw), `PortMarketLink`, `SaveGame` autosave, `PortBeats`, `VoyageLog` |
| `ShipBody.Warning(kind, value)` (new) | the body | helm authority, GM_eff, heel margin, rig load, bilge, anchor tension | `BeatScheduler` (the crew's lines, ship 3), `Pressure`, `ShipSounds` |
| `Wildlife.Fired(WildlifeEvent)` | hourly | the event | `DevHud`, `TextPanel` (the crew's line), the market (`Supply` already carries it), `FarmDressing` |
| `Flock.Startled/TookFlight` (new, two lines) | the flocks | the flock | `SoundEvents` (skein, heron flush), `Omens` (gulls inland) |
| `SoundEvents.SoundHeard(sound, species, position, year)` | the soundscape | the sound | `TextPanel` (`remarks.json`), the first-heard log (collectables) |
| `EncounterRunner.Due/Chosen` | the cards | card, option | the graphs, `VoyageLog`, `Reputation` through `GameEffects` |
| `WorldEconomy.Touched(port)` | the lazy tick | port | `TownLife` (dressing counts: stacks, empty dams, no smoke), the panels |
| `SaveGame.Loaded` (exists) | load | slot | every `Restore` |

### 2.4 One owner for each truth (the conflicts, decided)

The nine documents were written in parallel and overlap at the seams. These are the decisions this plan makes; each names the loser so nobody builds it twice.

1. **The sky is one model.** `WeatherModel.cs` moves from `Code/Game/Sim` to `Code/Core/Weather` file-for-file (namespace `Kapman.Weather` kept; `WorldSim` and the shim compile unchanged); `WeatherEngine.Shared` is the one instance; `LevelWeather.manual = false` reads it; `LocalWeather.Model` returns it (weather 4.1). The eight `WeatherKind`s stay as hand overrides for shots and the G key.
2. **Cloud transmittance belongs to the weather.** `WeatherSample.cloudTransmittance` (the table in weather 2.5) is the Sun's dimming; `LightingModel.CloudDim` takes it as its input and `LevelClock` divides out HDRP's cloud-shadow opacity (lighting 4.2). The lighting document's `(1 - overcast)^2` is the stand-in only until the engine lands (weather night 2), and `overcastLight` 0.45 goes.
3. **Humidity.** `FrostModel`'s RH-by-kind table (lighting 2.5) and `SoundscapeModel.HumidityFromWeather` (audio 2.1) are the same interim; once the engine gives a dew point, both read `WeatherSample` and the tables are deleted.
4. **Hard winters, mast, herring: one die.** `YearState` (wildlife 2.7-2.8) rolls them from `(seed, year)` and the chronicle's `winter` entries win. `ClimateNormals` (weather 1.2) takes `YearState.HardWinter` as its winter offset, so the engine's ice (`IceModel`) and the herons' crash are the same winter; the economy's `shocks_793.json` reads `YearState.Stock` and `HW` instead of rolling a 15 % herring year and a 5 % murrain. `LevelWeather.yearTempOffset` (wildlife 4.4) is the interim write until the engine lands, then it goes.
5. **The seasons' supply curves belong to the animals and the farm.** `Supply.SeasonalSupply` (wildlife 2.9) is what the economy's `Seasons` calls for `make_season`; the economy keeps the price side (elasticity by category, the 20-day cosine on the cheap/dear months).
6. **Sea state for the ship and the ear.** `SeaStateModel` (weather 2.4) gives Hs, Tp, direction, swell trains, steepness, breakers; `ShipSeaState` and `SoundContext` read it. Until it exists both take the Beaufort table (ship 1.4, audio 1.1). The ship document's "swell_storm_seas.md" means `weather_and_sea_state.md`.
7. **HDRP water time at 4x.** The ship document sets `surface.timeMultiplier = LevelClock.Speed` so the integrator's substeps and the surface share one time; the weather document sets it to 1 so a wave takes its real seconds on screen. **The ship wins**: the body samples the surface it floats on, and with the two on different clocks the hull would meet waves four times slower in its own time at 4x (broaching, orbital velocity and the probes would all be wrong). The weather document's rule that 4x is refused in fronts, cells and gales (2.2 above) means the sped-up look is only ever seen on a quiet sea. The sea *state* (Hs, Tp) steps at clock speed either way.
8. **The economy stays in `Kapman.Game`.** The economy document planned a new `Kapman.Economy` with ported copies of `Market`, `Deal`, `Purse`, `Hold`, `Trade`, `Chronicle`; those classes are already here, committed and tested (`TradeTests`, `SimTests`, `EventTests` ...). The new classes (`WorldEconomy`, `Seasons`, `Silver`, `Shocks`, `Tolls`, `Credit`, `People`, `EconomyClock`, `PortMarker`, `PortMarketLink`) go beside them in `Code/Game/Economy/`; `Deal`'s three outside calls go behind `IDealContext` as the document says. One copy.
9. **Runtime never references Game.** The encounters document put `GameEffects` and `EncounterRunner` in `Runtime/Encounters` "needing Kapman.Game": that is a circular reference. They, `MooringTrigger`, `EncounterTriggers`, `EncounterPanel`, `EncounterSave` and the economy's `EconomyClock` live in `Kapman.Game`. The pure parts stay in Core and are tested through `FakeGame`, exactly as designed; `GameEffects` can be built on night one because the port compiles.
10. **One ship, one speed.** The playability document's `SpeedModel` (a wrapper on the sim's `Travel.Speed` replacing `LevelInfo.shipSpeed`) and the ship document's `ShipBody` are two models of the same thing. **The body wins**: `ShipBody` drives the `ShipMotion` facade; `ShipOrders` (in `Kapman.Ships`) is the order set with the crew's delays, and the playability document's `OrderBook` is folded into it. `Travel.Speed` stays as the AI fleet's polar and as the **oracle** for the ship's polar test (ship test 4 compares the body's steady speeds to it within 10 %). `LevelInfo.shipSpeed` survives only for undriven boats. `Kapman.Sailing` is not created; its classes live in `Kapman.Ships` (2.1).
11. **AI ships sail the chain** (gateways 2.9), not the real kilometres (economy 1.3, 2.7): a trader and the player leaving Dorestad together arrive at Lundenwic together (`ChainSimTests.TraderKeepsPace`). The economy's timetables stay in game hours. This is open question 3 for the user; the plan builds it this way unless told otherwise.
12. **The date never jumps at a phase.** The economy document asks (its question 1) whether a cut reach may advance the clock by its sailing time. The user's rule (28 Sep, 3 Oct) is one clock that never changes rate or jumps on its own; the chart's mark glides across the cut reach instead (gateways 2.7). News lag, spoilage and pay follow the chain's 30 game hours. Listed under the questions so the user can overrule.
13. **The game reads nothing from `G:\` at runtime** (audio 4.2). Two readers exist today: `LevelWater.tidesFile` and `KapmanGame.lanesFile` (`G:/kapman_unity/data/baked/world/lanes.bin`). Both move under `Assets/Kapman/Data/` (binary `.bytes` assets) in M2. Bakes write to `G:\kapman_unity\data\` and a bake step copies what the game needs.
14. **The phase hook has one name.** Until `ChainRuntime` exists (M2), every system's phase hook (audio `BeginPhase`, the encounters' "leaving" cards, `KapmanGame`'s carried hours) attaches to `LevelExit.Go`; `ChainRuntime` raises the same `Preload/Cut/Unloaded` events, so nothing is rewired twice. `LevelExit.Go`'s `SceneManager.LoadScene` is the one line the chain deletes.
15. **The omen's oracle.** `Omens` (wildlife 2.11) takes a delegate: `LevelWeather.Auto(year, day, hour + 6)` now, `WeatherEngine.Sample(hour + 6)` once the engine drives the level. The sign still runs six game hours ahead and is wrong a known fraction of the time.
16. **Shots and the editor keep working.** When the clock, sky and weather move into `L_Boot` (M2), `Shots`, Build mode and a level opened alone open `L_Boot` additively (gateways 4.2); "Shoot Dorestad still renders" is in M2's done-when.

### 2.5 The statics and the guard

`LevelInfo.Current`, `LevelClock.Current`, `LevelWeather.Current`, `LevelWater.Current`, `HdrpSea.Current`, `RiverCurrent.Current`, `ShipMotion.Player`, `SkyDome`, `KapmanGame.Current`, `Wildlife.Current`, `LevelSoundscape2` (singleton) are the singletons. The rule from the gateways document (2.5), applied to all of them from the first slice: **an `OnEnable` self-assign happens only when `ChainRuntime.Current` is null (a level opened alone, the editor) or is this component's level**; `ChainRuntime` sets them at the cut. Without the guard a preloaded level would give two suns, two clocks and two listeners.

### 2.6 Scenes

- `L_Boot` (M2): the persistent scene: the player's ship (`ShipMotion` + `ShipBody` + wake + water line + `ShipSounds`), the camera and listener, `LevelClock`, `LevelWeather` + `HdrpWeatherDraw`, `SkyDome`, the global sky/fog volume, `LevelSoundscape2` and its beds, `ChainRuntime`, `KapmanGame` (today `GameSetup` puts one into every level scene; its statics already survive loads, so the move is a deletion), `DevHud`, `Feasts`, the panels.
- `L_<Id>`: one disabled root with `LevelRoot` (two-stage enable), `LevelInfo`, `LevelWater`, the terrain, the water surfaces, `RiverCurrent`, `RiverTint`, `HdrpSea`, `TownLife`, `Lamps`, the flocks, `Wildlife`, `SoundEmitterSet`, `JoinAnchor`/`JoinTrigger`, `PortMarker`s, `MooringTrigger`s, a local volume at weight 0.
- Until M2 the two scenes stay as they are (each with its own clock, ship and camera), `LevelExit` loads the next as a scene, and every new component is added by the builders so a rebuild is the only way a scene changes (the builders start from empty scenes and would wipe hand-placed objects: playability 4.3).

### 2.7 The save

`SaveGame` (Game, version 2, gzipped JSON, any time) gains one block per system behind a small interface `ISaveBlock { string Key; void Capture(JsonWriter); void Restore(JsonReader); }`; a missing block on load means defaults (so an old save loads), and the version rises by one per block added, with a `SaveTests.AllBlocksRoundTrip` that captures every block, restores into a fresh set and compares a state hash.

| Block | Holds | From |
|---|---|---|
| `chain` | current level id, local pose, the corridor side by hysteresis (never half across), per-level deltas (moved boats, berth) | gateways 2.11 |
| `carry` | `LevelCarry`: speed, heading, goal, hull, rig, cargo, purse, debts, crew, weather front, chart position | gateways 2.10 |
| `ship` | `ShipState` (body, orders, bilge, integrity, cargo stowage, fatigue) | ship 2.1 |
| `weather` | world seed, nothing else: the engine is a function of (seed, year, hour) and `WarmStart` rebuilds the sea on load | weather 2.2, 2.4 |
| `economy` | `WorldEconomy.Capture` (markets, silver, port states, boards), about 2 MB, written off the main thread | economy 4.1 |
| `encounters` | `NpcRegistry`, relationships by merchant id, flags with expiry, the scheduler's pending and cooldowns, scheduled cards | encounters 4.1 |
| `wildlife` | event cool-downs (last fire day per event and region), `YearState` cache | wildlife 2.10 |
| `play` | lessons fired, `VoyageLog`, `DebtClock`, the first-heard log | playability 4.2, audio 3.2 |
| `eye` | adaptation `a` (trivial, so a night load does not dazzle) | lighting 2.2 |
| the existing blocks | the merchant, hold, purse, known, chest, reputation, voyage, the sim's ships | as built |

Autosave on every `Moored` (GDD) and the chain's save-load round trip after a cut (`ChainSaveTests.CorridorNormalises`, the PlayMode smoke).

### 2.8 The wiring, as quantities

| From | To | What crosses | Cadence |
|---|---|---|---|
| `WeatherEngine.Shared` | `LevelWeather` (the level), `WorldSim` (every AI ship), `EncounterRunner` vars, `Sight` (MOR) | one `WeatherSample` per (lat, lon, hour) | hour, eased per frame |
| `WeatherEngine` + `levels/<id>/fetch.json` | `SeaStateModel` per level | Hs, Tp, direction, spread, steepness, whitecaps, swell trains, surge, ice | minute; `WarmStart` on entry, jump, load |
| `SeaStateModel` | `HdrpSea` (band fit from the calibration file; the Gerstner deformer for cross swell), `ShipSeaState`, `SoundContext.Hs`, `ShoreWater` (breaker height) | the sea state | frame |
| `LevelWeather` | `ShipBody.SquareSail` (`WindVector`, `Gust(position)`), `Birds`/`Flock` (wind stop), `TownLife` (smoke), vegetation wind, `LevelSoundscape2` (every field of audio 2.1), `HdrpWeatherDraw`, `FrostModel`, `Lamps` (no fire in rain) | the eased state | frame |
| `WeatherSample.cloudTransmittance` → `LightingModel` | `LevelClock` (Sun lux), `SnowFrostPass`/materials (E_sky from `SkyLightProbe`) | lux | frame |
| `LevelWater` | `ShipBody` (float, ground when it leaves), `Wildlife` (tide feed, haul-out), `AcousticProbe` (shore distance shifted by the flood), `JoinTrigger` (a flats join closed at low water), `TideWait` | base + tide + surge + river stage + flood | frame |
| `ChainRuntime` | the statics, `WaterSampler`, the camera, `RouteMap` (chart), `ChainSim` (AI bodies), `LevelSoundscape2` | T_ab, `LevelCarry` | at the cut |
| `ChainSim` | `WorldSim` (progress in chain minutes), the chart (derived real km), `Encounter.CanOffer` (chain distance) | positions | hour |
| `Chronicle` | `Shocks`, `ChronicleStorms`, `WeatherRequest`, encounter tags, `SkyEvents`, `YearState`, the chain's `from`/`to`, `TownLife` | entries | day |
| `YearState` | `ClimateNormals`, `Shocks`, `Presence` (K_s), `Runs` | HW, mast, stock, trend | year |
| `Supply.SeasonalSupply` | `Seasons` → `Market.Price` | the make/use curve per good and region | on touch |
| `WorldEconomy` | `EncounterRunner` vars (silver, hold, debt), `TownLife` (dressing counts), `MarketPanel`, `TraderBrain` | prices, stock, state | on touch |
| `GameEffects` (encounters) | `Trade.Purse`, the hold, `Reputation`, `Commissions`, `Known` boards, `Chest`, the crew, `Encounter` (sim orders), the law state | effects | on choice |
| `ShipBody` → `ShipMotion` facade | `OrbitCamera`, `ShipWake`, `Birds`, `LevelSoundscape2`, `ShoreWater`, `JoinTrigger`, `LandmarkSight`, `PositionBelief`, the first build's `SailingShip` shim | position, heading, speed, pitch, roll, velocity | frame |
| `ShipBody.Warning` | `BeatScheduler` (crew lines), `Pressure`, `ShipSounds` | the numbers of ship 3 | frame |
| `BeatScheduler` + `Pressure` | `DevHud`/panels, the camera's cut rate, `EncounterRunner` (optional cards held while P > 0.5), `VoyageLog`, `FunTelemetry` | beats, P | minute |
| `Wildlife` (hourly presence) | `Flock`, `MigrantSkeins`, `SeaLife`, `FarmDressing`, `LevelSoundscape2` (voice windows), `Omens` | presence per species, behaviour, events | hour |
| `SoundscapeModel` | the AudioSources, `SoundHeard` → `TextPanel` and the first-heard log | gains, cutoffs, positions | frame / event |
| `EyeModel` | `SkyDome` (exposure floor), the grade (colour drain, grain), `Sight.RangeKm` (night term) | adaptation | frame |

---

## 3. Data files

Rule: **everything the game reads at runtime lives under `Assets/Kapman/` in git; bakes write to `G:\kapman_unity\data\` and a bake step copies in what the game needs; every file has an owner, a validator test and a credit line where its source needs one.** The existing `Assets/Kapman/Game/Data/` (the first build's 22 tables, read by `GameFiles.Find` by name) stays where it is; new files go under `Assets/Kapman/Data/<system>/` and `GameFiles.Find` is taught to search both.

| File | Owner (document) | Where | Read by | Validated by |
|---|---|---|---|---|
| `chain.json` | gateways 2.2 | `Assets/Kapman/Data/chain.json` | builders, `ChainRuntime`, the chart, `ChainSim` | `ChainDataTests` (parses, connected, rules, route minutes) |
| `data/levels/<id>/` (height.f32, cover.u8, wood.u8, water_sd.f32, meta.json + `anchors`, `corridor_source`, `channel`), `data/joins/<id>/` | gateways 4.3 | `G:\kapman_unity\data\` (bake) | the builders only | `CorridorBakeTests.HeightsMatch`, `ChainCheckTests.DepthUnderCorridor` |
| `World/Build/<level>.json`, `join_<id>.json` | gateways 2.12 | git | the builders (Build-mode deltas) | the build report lists dropped deltas |
| `climate_normals.json`, `circulation.json`, `storms_793.json` | weather 4.2 | `Assets/Kapman/Data/weather/` | `WeatherEngine` | `NormalsReproduced`, `ChronicleStorms`, `Determinism` |
| `Bake/weather/make_normals.py` + `raw/` KNMI CSVs + `SHA256SUMS` | weather step 0 | git | regenerates the tables in the weather document and the normals | the "regenerated on" stamp |
| `levels/<id>/fetch.json` | weather 4.2 | `Assets/Kapman/Data/levels/<id>/` | `SeaStateModel` | `WaveGrowth`, `Surge` |
| `water/hdrp_sea_calibration.json` | weather 2.5 | `Assets/Kapman/Data/water/` | `HdrpSea` | measured by `Kapman > Water > Measure sea` |
| `ships/*.json` (gislinge, coaster_14, knarr_skuldelev1, utrecht_logboat + the AI classes), `sail_coefficients.json`, `ground_kinds.json` | ship 4 | `Assets/Kapman/Data/ships/` | `ShipClassData`, `ShipBody`, `WorldSim` | ship tests 1-15 (test 6 is the C_r fit, the passing table is the file) |
| `flames.json`, `twilight.json`, `beacons.json`, `sky_events.json`, `weather_kinds.json` | lighting 4.3 (the document put them under `G:\data\light`; they move under Assets by rule 2.4.13) | `Assets/Kapman/Data/light/` | `Lamps`, `SkyDome`, `SkyEvents`, `FrostModel` | `FlamesTableTests`, `TwilightTableTests`, `SkyEventsTests` |
| `soundscape.json`, `schedule.json`, `clips.json`, `remarks.json`, `Kapman.mixer` | audio 4.2 | `Assets/Kapman/Audio/` | `LevelSoundscape2` | `LibraryIsShippable` (CC0/PD + URL for every clip), the June dawn and November storm contexts |
| `acoustic_cover.asset`, `acoustic_sd.asset`, `acoustic_sd_flood.asset`, `SoundEmitterSet` (serialised in the scene) | audio 4.2 | `Assets/Kapman/Levels/<level>/` | `AcousticProbe` | baked by `SoundscapeBuilder` |
| `regions.json`, `species.json`, `farm_year.json`, `wildlife_events.json`, `CREDITS_wildlife.md`; per level `habitats.json` | wildlife 2.1 | `Assets/Kapman/Data/wildlife/` | `Wildlife`, `Supply`, `LevelSoundscape2` | `SpeciesJsonParses` (voices only where the credits name the species) |
| `shocks_793.json`, `seasons_793.json` (amplitudes only; the curves come from `Supply`), `silver_793.json`, `people_793.json`, `trade_rules.json` + `tempo` block, `goods_793.json` + `rhine_delta: 2` on wine, `feathers`, `eiderdown`, `fowl`, `tallow`; `ports_more.json` when written | economy 4.2, wildlife 2.9 | `Assets/Kapman/Game/Data/` | `WorldEconomy`, `Chronicle`, `People` | economy tests 1-11 |
| `encounters/harbour_793.json`, `sea_793.json`, `crew_793.json`, `long_793.json`, `npc_roles_793.json`, `names_793.json`, `tongues_793.json`; `writing.json` keys | encounters 4.2 | `Assets/Kapman/Game/Data/` | `EncounterLoader` | `EncounterLoaderTests`, `CondParserTests`, `VoiceTests` |
| `beats_793.json`, `pressure.json`, `hazards_793.json`, `storm_survival.json`, `lessons_793.json`, `landmarks_<level>.json` (written by the builder), `progression_793.json`, `debt_terms.json`, `port_beats_793.json` | playability 4.3 | `Assets/Kapman/Data/play/`, landmarks beside the level | `BeatScheduler`, `Hazards`, `Lessons`, `DebtClock`, `LandmarkSight` | `Hazards_EveryRowHasSignLeadCounter`, `Landmarks_PeriodTrue`, `DebtClock_Dates` |
| `lanes.bin`, `tides` (EOT20) | first build | move from `G:\` to `Assets/Kapman/Data/*.bytes` (M2) | `WorldSim`, `LevelWater` | the existing `LaneTests`, `TideModelTests` |

Licences in play, all shippable with a credit line: KNMI (attribution), Met Office MIDAS Open (OGL v3), MET Norway (CC BY 4.0), DWD (CC BY 4.0), Rijkswaterstaat (open), DMI (to confirm), Freesound CC0, Poly Haven and ambientCG CC0, SIL OFL (Gentium), Wikipedia text as facts only. The audio validator refuses any clip that is not CC0/PD; the wildlife validator refuses a clip for a species the credits do not name.

---

## 4. Build order: milestones and done-when

The GDD's milestones (M0 to M8 as the playability appendix names them) are kept, and each system's first slices are placed in them. A milestone is done when its headless tests are green, its Play smoke passes, the user has played it, the fun test of the appendix is asked, and it is committed. Nights are (guess) and assume the user's "stable first" rule: compile, test, smoke, commit after each step; no blind code while the editor hangs. The level line (which places exist) runs beside the systems because the chain's seam can only be judged on a real corridor.

| Milestone | What goes in (by document and slice) | Levels | Done when (headless where possible) | Nights (guess) |
|---|---|---|---|---|
| **M0 The photograph** (the look is HDRP's physics, no filters) | lighting night 1 (`LightingModel`, `EyeModel`, `SkyDome` reads the eye; the `DiagFog` re-run) and night 2 (the snow/frost pass moved to `BeforeTransparent` and lit from irradiance via `SkyLightProbe`; the clock's Sun lux from the model; the colour drain and grain); one owner for exposure, stars and Moon (lighting 4.5); rope materials with geometric specular AA (4.7); the physical camera (4.8) | Dorestad, North Sea as they are | `LightingModelTests` and `EyeModelTests` green; the eight `DiagFog` variants differ and none is black; a Fog-kind shot renders lit with the pass on; P14 shows white snow darker in shade; N7 re-shot without fireflies | 3 |
| **M1 Dorestad, the spine in place** | tonight's slices (section 5): `TickLadder`; the chain data and tests; `LevelRoot` and the guards; the weather engine core (wind, pressure, temperature); the ship floats as a body; wildlife presence on the existing flocks; the encounter core with the toll man; then audio night 1 (`LevelSoundscape2` driving the builder's sources: WIND crossfade, RAIN, lapping by the shore map, thunder at c(T)), economy night 1 (`WorldEconomy.Day`, famine and raid shocks, `EconomyClock`, the HUD price line), playability night 1 (`ShipOrders` with four orders live on the body, `landmarks_dorestad.json` from the builder, `LandmarkSight`, `BeatScheduler`, `VoyageLog`, two lessons) | Dorestad rebuilt with `LevelRoot`, `Wildlife`, `SoundEmitterSet` later | every document's night-1 tests green in one headless run (about 60 new tests); `SmokeTests` 2/2; `BeatDensity_DorestadToExit` on the scripted route; in Play: she floats at her waterline and rides the river, the HUD shows the sample's wind and the wheat price, the toll man opens on mooring, swallows in June and geese in November; the M1 fun test (out of the harbour in under 8 minutes) | 6-8 |
| **M2 Down the Rhine to the sea, no loading screen** | gateways nights 2-3 (`ChainRuntime`, `JoinTrigger`/`JoinAnchor`, `DeckBeat`, `WaterSampler.Forget`, the two captures, `ChainPlayTests.SailThroughJoin` and its CSV) then `L_Boot` (clock, weather, sky, ship, camera, loops, `KapmanGame` moved out of the levels; `Shots` opens it additively); weather night 2 (fronts, cloud layers, transmittance, rain, fog, MOR into HDRP) and `SeaStateModel` + `WarmStart` + the calibration tool and `HdrpSea` on the fit; ship night 2 (`SquareSail`, `SteeringOar`, `Helm`, the C_r fit, the polar against `Travel.Speed`); `ChainSim` wired into `WorldSim`; `lanes.bin` and the tides under Assets; the chart's glide; the beats on the whole route; the deck-beat framing proved by the two captures | **Traiectum's reach** and **the Old Rhine mouth (Katwijk)** baked with `make_place.py`/`make_corridor.py`, built by `PlaceBuilder`; the first real `bend` corridor | `ChainPlayTests.SailThroughJoin` green headless with its frame CSV committed and the three measured numbers (UnloadUnusedAssets hitch, terrain enable, second water sim) in PROGRESS.md; `ChainSimTests.TraderKeepsPace`; `ChainDataTests.RouteMinutes` for `dorestad_lundenwic` within 20 % of 30; the weather's sleet day and frosty dawn come out of a date and a place with no hand settings; she heels to a beam wind and refuses 60 degrees; one sun, one clock, one listener at every frame; "Shoot Dorestad still renders"; the M2 fun test (the phase unnoticed by 2 of 3) | 8-10 |
| **M3 Ports and trade** | economy night 2 (`Seasons` on `SeasonalSupply`, production curves, `burning` and `ruined`, buy/sell through `Deal`, `setYear`); `PortMarker`/`PortMarketLink`; the market panel, the ledger tab, `TalkPanel` as encounter cards (`People`); encounters night 2 (`NameForge`, the berth card, "Talk to people", the styled panel, `GameEffects` over the port, the scheduler tests); `PortBeats` paced 3-5 a call; the port menu on the glass; the count's man and the debt date; `Tolls`; news by ship (`Knowledge`, kept) | **Lundenwic's approach** and **the Nore** as river places (true size on the lidar where it exists); the North Sea's blockouts get `PortMarker`s | economy tests 1-11 green; `ArrivalTests`, `SchedulerTests`, `NameForgeTests`; `PortBeats_Density`; in Play twelve U presses show wheat dear in spring and cheap after harvest, wine cheap in November, Dorestad refusing deals in 863; the M3 fun test (the reason for a buy is a note, the date known, a port beat named) | 6-8 |
| **M4 Tides and seamanship** | ship night 3 (`Bilge`, `Mooring`, `Grounding` with the tide, the Breaker field, the crew's warning lines, `ShipSounds`); `tideShare` lerped along the mouth levels; `TideWait` and `Aground` (kept) on the body's numbers; the full order panel on the glass; `Hazards` with the lever rule; `PositionBelief` and the chart's doubt ellipse (if the user keeps it); soundings and `Known` depths; the seamanship rules at the weir and the barge (kept) | the Rhine bar at Katwijk, the Thames sands and the Nore's flats as real depths | ship tests 9-12; `Hazards_EveryRowHasSignLeadCounter`, `Hazards_LeverRule`, `PositionBelief_Grows_And_Fixes`; a boat anchored at high water on the Dorestad shoal is aground at low; the M4 fun test (a deliberate tide wait; every grounding traced to a sign) | 5-6 |
| **M5 The living North Sea** | the fleet on the chain with bodies in resident levels (`WorldShips3D` on `ChainSim`); encounters at sea (hail, found, the king's ship) through `Encounter.CanOffer` on chain distance; wildlife night 2 (`Runs`, `MigrantSkeins`, `Omens`, `eel_night`, `cranes_over`, the ocean level's region, seals as data, `habitats.json`); `SeaLife` decals; audio nights 2-3 (vegetation beds, the wildlife roster from `species.json`, the ship's laws, `SoundEmitterSet`, occlusion, reverb, the two bells, `SoundHeard` and `remarks.json`); fog as one MOR for the lookout, the AI and the ear; `FogBankField` (kept) on the advection fog | **Walichrum**, **Witla**, **Quentovic** as river places; the Channel ocean level | `ChainSimTests`, `OmensBeatChance`, `CranesCrossInAutumnNotInJuly`, `EelsRunOnDarkWetAutumnNights`, `ScheduleSunday`, `LibraryIsShippable`, `Occlusion`; the M5 fun test (a course change after a sail or a sky; a hail used; P under 0.3 for five minutes without a quit) | 8-10 |
| **M6 The ship and her crew** | Plans/crew.md C1-C5 on the encounters engine (hiring at ports from their own pools, the council before a crossing, grievances, faith, reading with the crew, the riddle setter); crew stats into `Helm`, `Oars`, `Bilge`, `SquareSail` through an `ICrewStats` struct passed by Game (Ships cannot reference Game); `Tongues` and the comprehension masking; `Lamps` and the night-vision resource (`EyeModel.NightSightScale` into `Sight.RangeKm`); the deck (isometric) mode's listener and mix | Hedeby, Ribe (Norse hiring pools) | `NpcRegistryTests` (death, heirs, decay), `TonguesTests`; ship test 12 (the broach by helm skill); the M6 fun test (two names with a trait each; one reading finished) | 8-10 |
| **M7 Storms** | `ChronicleStorms` on their days with the 48-hour approach, swell trains, the surge into `LevelWater`; `IceModel` and drift ice; `FlashModel` stroke trains and the cloud-glow; spindrift and foam streaks (weather 2.5); the Breaker field replaced where the shore cap and surf enter the sampled field; `StormSurvival` Monte Carlo per class; the storm stage (Plans/storms.md S6) on the body's real numbers; the mouth closed in a storm (`closedWhen`); the gale at dusk scene of weather 2.5 as a profiled shot | the Frisian coast (838) and the Thames mouth (1014) as the storms' coasts | weather tests 6, 9, 12, 13; `StormSurvival_Careful/Careless/Recorded`; `FlashModelTests.Train`; the Profile tool's `profile_<date>.json` with the cut list applied from measurement; the M7 fun test (2 of 3 act before the stage opens; losses self-attributed 3 of 4) | 8-10 |
| **M8 History and heirs** | `Silver` (tributes, dirhams, Rammelsberg), start years with the economy's backfill, `People` with named historical rows, NPC death and heirs, the merchant's ageing and succession (feelings halved, memories tagged), quiet years (if the user wants them), `SkyEvents` (Eldgjá's haze, the comets of 837 and 1066), the climate ramp into the MCA, `DebtClock` terms as difficulty, the house's annal; the Jutland haul | Hollingstedt/the Schlei haul; Birka, Kaupang by era | economy tests 6 and 10; `SkyEventsTests`; `QuietYear_NotBetter`; `DebtClock_Dates`; the M8 fun test (a plan changed by the chronicle; a restart in another era unasked) | 8-12 |
| **The chain to 240 ports** (runs beside M3-M8) | `make_place.py`, `make_corridor.py`, `PlaceBuilder`, `ChainBuilder`, `ChainChecker` on every join; a generated base for every port and river place of `ports_793.json`, `ports_more.md` and the level chart, region by region (the Rhine road, the Thames, the Frisian coast, the Channel, Jutland, the Oslofjord, the Baltic, Ireland, Biscay, Iberia, the Mediterranean, the far north, the Rus rivers); Build mode writes `World/Build/<id>.json` deltas and the anchors and corridors are locked | one region per week (guess) | `ChainDataTests.Connected` for every port; `CorridorBakeTests.HeightsMatch` for every join with both bakes; the smoke sails each new region end to end | ongoing |
| **Multiplayer** (after the single-player core, Plans/multiplayer.md) | the host evaluates the weather engine and streams hourly samples; the host orders the day's deals; NPC forging global, relationships per merchant; chain positions shared, bodies per client; no pause, no 4x, no rest | | the economy's state hash as the desync check at each day rollover | later |

What is deliberately **cut until after M4** (playability 3.13): the full council (one seat first), board games, raids (M5), storms (M7), standing beyond the port card, quiet years, era starts, heirs, multiplayer, agents and factors, verse-making, the law case. A cut feature leaves its data table and a hook that logs "not built" to the voyage log, so the beat-density tests run on the real route from the start.

---

## 5. Tonight

Rules for the night: pure C# first (no scene rebuild while the editor is open), a headless test per slice, a commit per slice, and the GPU and bake jobs **queued at the end** as unattended runs (one level per Unity run; the editor must be closed for batch mode; after a crash the editor takes about seven minutes). Each slice's done-when is a command that a script can judge. If the night ends after slice 3, the order still stands for tomorrow. Nothing in HDRP, no new UI, no scene is touched by hand.

**Step 0 (20 minutes). The green line.** Run `Unity.exe -batchmode -nographics -projectPath G:\kapman_unity\Kapman2 -runTests -testPlatform EditMode -testResults Logs\tests_plan_0.xml -logFile Logs\tests_plan_0.log`; expect 98 passed, 2 skipped. Commit the modified and untracked files listed in the baseline (the `SimHarness`, the `ShotTests`, the PlayMode assembly and smoke, the three design revisions, PROGRESS.md, the impostor materials). Done when the XML says `failed="0"` and `git status` is clean.

**Slice 1 (1 hour). The tick ladder.** `Core/Sim/TickLadder.cs`: `Advance(double fromHours, double toHours, bool jump, ITicks sink)` raising `Minute(n)`, `Hour(h)`, `Day(d)`, `Year(y)` in order (every hour before its day's rollover), `Jumped(from, to)` when `jump` or when the step exceeds 2 game hours in one frame, `Reversed(from, to)` and no ticks when `to < from`. `KapmanGame` uses it for `HourPassed`/`DayPassed` and adds `MinutePassed`, `YearPassed`, `Jumped`, `Reversed`; `LevelClock`'s T/Y/U handlers mark the jump. Test `TickLadderTests`: a 30-day jump gives 720 hours and 30 days in order; 1.5 hours at `Speed` 4 over 16.7 ms frames gives 90 minutes and 1 hour; a backward step gives one `Reversed` and nothing else; `Year` fires once at the Julian year's turn (Calendar). Done when the test is green headless and `SmokeTests` still pass (the PlayMode run is queued, not run now). Commit.

**Slice 2 (2 hours). The chain data** (gateways night 1, steps 1-2, plus the guards of step 3 without the rebuild). `Assets/Kapman/Data/chain.json` with `dorestad` and `northsea_south`, one `mouth` join (`dorestad/down` to `northsea_south/rhine_mouth`, hide `deck_beat`), the Thames and Walichrum anchors as mouths not yet built, `layer` `Level0`/`Level1`, `water.pushesShips` true for Dorestad, the route `dorestad_lundenwic` (budget 30) with the sailed metres of gateways 2.3; anchor poses copied from `plan.exitDownstream` and `mouthIn`, the downstream anchor moved to about 700 m of channel (the time-budget question is the user's, but the data must hold a value: 700 m is the proposal). `Core/Chain/`: `ChainData`, `JoinTransform`, `RouteMap` (the glide), `LevelCarry`, `ChainCheck`, `ChainSim` (chain-minute arithmetic). Tests: `ChainDataTests.Parses/Connected/Rules/RouteMinutes`, `JoinTransformTests.MapsAnchorToAnchor/CorridorParamIsInvariant`, `RouteMapTests.Monotonic`, `LevelCarryTests.RoundTrip`, `ChainSaveTests.CorridorNormalises`, `ChainSimTests.TraderKeepsPace/RealKmIsDerived`. The `OnEnable` guards go into `LevelInfo`, `LevelClock`, `LevelWater`, `LevelWeather`, `HdrpSea`, `RiverCurrent`, `ShipMotion`, `SkyDome` with `ChainRuntime.Current` as a static that is null tonight (a stub class), so they are no-ops. `LevelRoot` (two-stage) is written but not yet placed (the rebuild is queued). Done when the eleven tests are green headless. Commit.

**Slice 3 (2-3 hours). The weather engine, wind and pressure** (weather night 1 steps 1, 2, 4). Move `WeatherModel.cs` to `Core/Weather/` (namespace kept; `WorldSim`, `LocalWeather`, `SimHarness` compile unchanged; `LocalWeather.Model` returns `WeatherEngine.Shared`'s model). `WeatherEngine` (`Sample(lat, lon, hours, placeClass)` filling pressure, windFrom, windSpeed, gust, temperature in the extended `WeatherSample`; `HourlySamples`), `ClimateNormals` from a **provisional** `climate_normals.json` typed from Tables A-E (flagged `"provisional": true` until `make_normals.py` regenerates it: queued job 4), `CirculationChain` with the starting matrices as `circulation.json`, `SynopticField` on the existing systems and `ClimatePressure`, `WaveGrowth` (in U_A), `Visibility` (3.912). `LevelWeather.manual = false` reads the sample for wind and temperature; `DevHud` shows the sample, the type and the live systems. Tests: `Determinism`, `WaveGrowth`, `Visibility`, `Surge`, `OneSky`, and the loose `NormalsReproduced` (Beaufort shares within 8 points, Hoek gale days 5-20). Done when the six are green headless and the script check holds (`HourlySamples` over 72 h at Dorestad in November has at least one 30-degree turn and one 4 m/s change). Commit.

**Slice 4 (1 hour). The light and the eye** (lighting night 1b). `Core/Light/LightingModel.cs` and `EyeModel.cs`, pure; `SkyDome.LateUpdate` reads `EyeModel.LimitMin` in place of its own dusk lerp (the clock owns and steps the eye with game minutes); `LightingModel.CloudDim` takes a transmittance argument so slice 3's sample can feed it later. Tests: `SunAtJuneNoon52N`, `Sunset`, `Overcast`, `Moon`, `EV100`, `DarkAdaptation`, `LanternResets`. Done when green headless. Commit.

**Slice 5 (3 hours). She floats** (ship night 1). The `driven` flag on `ShipMotion` first (every reader keeps working); the `Kapman.Ships` asmdef and its line in the test asmdef; `ShipClassData` (the `gislinge` row converted from `ship_classes_793.json`, with from/to); `HullProbes.Parametric` (15 cells); `Hydrostatics`; `HullForces` (buoyancy along the normal, damping, ahead and lateral resistance, yaw damping) behind `IWaterField`; `ShipIntegrator` (6-DoF semi-implicit Euler, body-frame mass matrix with the added mass); `ShipBody` with the `Speed` accumulator, the kinematic `Rigidbody`, `Physics.simulationMode = Script` and `Physics.Simulate(0.02f)` per step, a constant oar force tonight and a plain yaw torque for `MakeFor`, pause handing the follow back to `ShipMotion`. Tests: 1 (parametric), 2, 3 (GM and roll period), 14, 15. The builders add `ShipBody` beside `ShipMotion` on the player prefab (takes effect at the queued rebuild; for tonight's Play check a one-line `GameSetup` addition puts it on the existing ship). Done when the five tests are green headless; the Play checklist (HUD speed, the wake bends, sounds, birds scatter, the exit fires, pause leaves her riding) is the user's smoke tomorrow. Commit.

**Slice 6 (2 hours). The calendar in the birds** (wildlife night 1, steps 1-3). `regions.json`, `species.json` (the ten Dorestad birds plus the stock entries as data), `farm_year.json` (rhine_delta, eight tasks), `wildlife_events.json` (three events); `Core/Wildlife/`: `Dice`, `Regions`, `Species`, `Presence`, `YearState`, `FarmYear`, `Supply`, `WildlifeEvents`; tests 1-6 and 9-14. `Runtime/Levels/Wildlife/Wildlife.cs` sets each `Flock`'s new `presence` and replaces the geese's and crows' clock hours with `LevelClock.SunAltitude` bands; `DorestadBuilder` adds it (queued rebuild) and `Shots` gets the two shots (day 200 and day 330 at 07:00). Done when the tests are green headless; the two shots are judged tomorrow from the queued run. Commit.

**Slice 7 (3 hours, if the night allows). The toll man** (encounters night 1). `Core/Encounters/`: `EncounterDef` and `EncounterLoader` with the cross-checks, `Cond`, `Effects` + `IGameEffects` + `FakeGame` (tests), `Scheduler` (arrival sets and the visit draw), `NpcRegistry` (fixed roles, feeling words, memory, relationships by merchant id; `names_placeholder.json`); `Game/Encounters/`: `MooringTrigger` at the dam heads (from the builder's dam-head list), `EncounterRunner` with the `Moored` hook and `TickLadder`'s hours, `EncounterPanel` from the existing `TextPanel` with the Gentium styles that are already in `Assets/Kapman/Game/UI`; `GameEffects` over `Trade.Purse` and `Reputation` (the port compiles, so it is built now, with `FakeGame` still used by the tests). Data: `harbour_793.json` with the toll man, `npc_roles_793.json`, `writing.json` keys as `[write:]`. Tests: `CondParserTests`, `EncounterLoaderTests`, `EffectsTests`, `NpcRegistryTests` (the night-one subset), `ArrivalTests`. Done when green headless; the Play check (the card opens on mooring, keys 1-4, Enter on the second arrival) is tomorrow's smoke. Commit.

**Queued at the end of the night (unattended, in this order, each its own Unity run; the editor closed):**
1. `Batch.DiagFog` unchanged (lighting 4.6): the sheet is read in the morning; no slice waits on it.
2. The two level rebuilds with `LevelRoot`, the layer from `chain.json`, `pushesShips`, `ShipBody` on the player prefab, `Wildlife` on Dorestad and the two wildlife shots: `Batch.BuildAndShootDorestadPlain`, then `Batch.BuildAndShootNorthSea` (the Dorestad bake re-bakes 320k trees and impostors; long).
3. The PlayMode smoke (`-testPlatform PlayMode`, no `-nographics`) on the rebuilt scenes.
4. `Bake\run.bat weather\make_normals.py` once it is written (the KNMI POSTs need the network): it regenerates Tables A-E in the weather document and `climate_normals.json`, and flips `provisional` to false.

**The night after** (in this order): audio night 1 (`SoundscapeModel`, `LevelSoundscape2` on the builder's sources, the shore map, thunder at c(T); tests 1-4, 8), economy night 1 (`WorldEconomy.Day`, famine and raid, `EconomyClock`, the HUD line; tests 1, 3, 5, 8, 9, 11), playability night 1 (`ShipOrders` with four orders on the body, `landmarks_dorestad.json`, `LandmarkSight`, `BeatScheduler`, `VoyageLog`, two lessons; `BeatDensity_DorestadToExit`, `Speed_SailState` against `Travel.Speed`, `Lessons_FireOnce`), then gateways night 2 (the cut). After that M1 is judged.

---

## 6. Decisions needed from the user

Forty-seven questions were asked across the nine documents. Most have a default the documents build with. These are the ones that change what gets built, in the order they bite, each with the plan's recommendation:

**Before M2 (they shape the chain and the ship):**
1. **How long Dorestad is, and the stream.** Today the mooring to the exit is about 1,800 m of channel: 15-19 real minutes. The plan builds the join about 700 m below the berth at the first tree-walled bend with the stream pushing her (about 6 minutes down, 10 back), the rest of the bake as scenery; the alternatives are raising the river `shipSpeed` to 3-4 m/s or moving the berth downstream. (gateways 1, playability 1) *Recommend: the 700 m join and the stream on.*
2. **The deck beat.** A river mouth cannot be cut invisibly (a change of scale); the plan hides it in a 3-5 s deck shot inside the prompt, skippable after the first time. Accept one visible kind of transition, or build 1:1 pockets at 50 mouths? (gateways 2) *Recommend: the beat.*
3. **AI ships at the player's pace.** The fleet moves in chain minutes so a trader overhauled off the Nore is the one that left ahead of you; the chart's days are the game's, not history's. (gateways 6, playability 2) *Recommend: yes.*
4. **Storm seas at full height on screen.** On 2 October you chose half height; the 4 October brief says 1:1. (weather 1, ship 3) *Recommend: full height on the ocean levels, measured by the calibration tool; half only if her motion reads badly.*
5. **The starting ship.** The 7.7 m Gislinge stand-in with Dorestat 1's numbers dropped in later, or the towed Utrecht logboat (a different first game). (ship 4) *Recommend: the Gislinge stand-in.*
6. **Leeway on the shortened sea.** Real leeway and tide drift move her 150x further across the real map on the 1:150 sea (10 degrees of leeway is 87 real km of coast). Accept (wind and tide matter hard in 30 minutes), or scale the sideways drifts alone? (ship 6) *Recommend: accept; revisit after M4's play.*
7. **Capsize in a river place.** At sea a wreck is game over (your rule); in a harbour she sinks in 1-3 m: game over too, or a salvage in days and silver? (ship 2) *Recommend: salvage in port and river, loss at sea.*

**Before M3 (the economy's tempo):**
8. **Waiting in port.** May the player let days pass ("winter over", "wait ten days") with the panel ticking and the clock's visible rate unchanged? Without it a 60-day closure is a real day of play and the season gap in prices is never played. (economy 0) *Recommend: yes, from M3, as a port action.*
9. **The date at a phase.** The plan never jumps the clock across a cut reach (decision 2.4.12). Confirm, or allow the jump the economy asked for. *Recommend: no jump.*
10. **The count's terms as the only difficulty** (no sliders): easy 480 d by Michaelmas, hard 20 pounds by Michaelmas with a slack of 1.4; and the debt's shape (a fixed sum with a day, as built, or a share of each voyage). (playability 4, economy 2) *Recommend: terms only; the fixed sum.*
11. **The doubt ellipse.** The chart's mark becomes an ellipse offshore, fixed by sun and Polaris sights and by landmarks; or the mark stays exact. (playability 3) *Recommend: keep it.*

**Before M5-M7 (the look and the ear):**
12. **Eye or camera at night.** The dark-adapting eye with three documented cheats (stars, the ship's outline, no colour), or a camera's black night. (lighting 1) *Recommend: the eye.*
13. **Human sounds.** Wordless human sound (a market murmur, oar breaths, a shout) or none; any music at a feast; a cowbell strike as the riveted hand bell. (audio 1-3) *Recommend: wordless human sound yes; no music; the cowbell until something closer is found.*
14. **Own recordings.** May we record a bucket, a gate, a rope, a flat slap ourselves as public domain to close the CC0 gaps? (audio 5) *Recommend: yes.*
15. **Animal packs.** Which packs for livestock, deer, seals, a stork mesh, and when; birds-only stays the rule for meshes until you say. The sounds already play. (wildlife 1)
16. **Teeth or atmosphere.** The animals' market multipliers at ±20-60 % (the calendar as a trade strategy) or ±10 % (atmosphere). (wildlife 2) *Recommend: the strong ones; they are data.*
17. **The climate of 793.** Modern normals cooled by 0.8 °C, warming into the Medieval Climate Anomaly by 950, or modern normals throughout. (weather 2) *Recommend: the cooled start.*
18. **Chance as a word.** "Sure / likely / doubtful" on an option, or nothing shown. (encounters 2) *Recommend: the word.*

**Can wait (the documents' defaults stand until you say otherwise):** grafts of invented bank at river joins (gateways 3); hauls as short places (gateways 4); the seaman's morning reading (weather 3); tacking allowed (ship 1); throwing cargo as a player order (ship 5); ruin by two broken credits (economy 3); start years offered (economy 4); Dorestad kept alive by trade (economy 5); the spatialiser plug-in (audio 4); the Sun-swap lightning (lighting 2); Dover's pharos lit (lighting 3); the APV bake per level (lighting 4); card depth (encounters 1); named historical NPCs (encounters 3); the Church as a standing body (encounters 4); comprehension masking and the blur behind cards (encounters 5); the herb-wise trait (encounters 6); the marsh fever (wildlife 3); hunting by the player (wildlife 4); the stranded whale's law (wildlife 5); legends like the last elk (wildlife 6); quiet years (playability 5).

---

## 7. Risks

| Risk | Where it bites | What the plan does | The test or measurement that catches it |
|---|---|---|---|
| Hiding an HDRP `WaterSurface` by the camera's culling mask is unverified | the chain's prepare stage (M2) | try it first in Play; the fallback is B's surface disabled until the frame before the cut (gateways 2.5) | the PlayMode smoke's frame CSV |
| `Terrain.OnEnable` for a 2049² heightmap with 320k trees cannot be yielded | the preload while sailing | draw flags off at prepare; if over 50 ms, enable only at the moored preload and show a proxy terrain while sailing | the smoke's CSV; the row in gateways 4.6 |
| Two CPU water simulations during the overlap | the corridor | budget 1.5 ms; else B's surface stays off until the cut | the smoke's CSV |
| Two levels resident exceed VRAM | a river place over 3 GB | the checker's size report; the tree cap and impostors as now | measured in M2 |
| The exposure pops at the cut until `L_Boot` | M2's first nights | accepted and listed; `L_Boot` removes it | "one camera never swapped" in M2's done-when |
| The black-fog and black-snow frames | M0 | the cause is the custom pass's colour-pyramid read at `BeforePreRefraction` (lighting 4.4); the `DiagFog` re-run proves it | the eight variants differ and none is black |
| HDRP's water spectrum is undocumented, so the sea state cannot be inverted | the storm sea | measure it (`Measure sea`) and fit with a stated Tp residual; cross swell by our own Gerstner deformer | the residual in `DevHud`; the calibration file |
| The KNMI tables in the weather document are unaudited (the script is not in the repo) | every tolerance in weather tests 1, 2, 4, 5 | `make_normals.py` committed with the raw CSVs and sums before the tight tolerances; loose tolerances tonight | the "regenerated on" stamp; `provisional` in the normals file |
| An open-ended weather tuning loop | week 2 | the Thirty-year report tool and the fit loop are a separate, bounded step; the loose tests gate nothing else | tests 1-2 tight only in week 2 |
| Float determinism is not cross-platform (Mono/IL2CPP, HDRP's CPU water) | multiplayer | single-player replay only; the host streams hourly samples; peers own their ship's state | stated in weather 2.2 and ship test 13 |
| The Dorestad time budget (15-19 minutes today) | the 30-minute voyage | question 1; `ChainDataTests.RouteMinutes` fails a route over budget by 20 % | the test |
| The first build's shims (`SailingShip`, `LevelFrame`, `LocalWeather`) wrap the old world | every port of game code | `ShipBody` keeps the `ShipMotion` facade so `SailingShip` holds; `LocalWeather.Model` returns the shared engine; `LevelFrame` becomes `chain.json`'s `frame` in M2 | `OneSky`; `SmokeTests` |
| CC0 clip gaps and lossy previews | the "hyper-specific" audio | the gap list with found ids; originals fetched in a daytime job; `clips.json` records which are previews | `LibraryIsShippable` reports the preview count |
| The editor hangs (about seven minutes after a crash); one level per headless run; batch runs need the editor closed | every night | pure C# slices while the editor is open; GPU and bake jobs queued at the end, unattended | the job order in section 5 |
| Breadth: nine systems, each 1-3 nights for its first slice, 20+ nights before M3 | the whole plan | the milestone gates and the cut list of section 4; nothing is started in a milestone that is not in its row | the milestone's done-when |
| Data placed by hand in a scene is wiped by the builders | landmarks, emitters, anchors | everything a level needs is written by its builder from data (`landmarks_<level>.json`, `SoundEmitterSet`, `JoinAnchor`s from `chain.json`) | `Landmarks_PeriodTrue`; a rebuild is the smoke |
| Two documents build the same thing twice | the seams | section 2.4's sixteen decisions | the one-owner rule in review |

---

## 8. Performance budget (RTX 4070, 1440p, 60 fps = 16.7 ms)

All GPU figures are the documents' estimates (none measured yet); the measurement tools are the chain smoke's frame CSV (M2), `Kapman > Weather > Profile` on the three named scenes (M7) and `FunTelemetry`. The stated requirement, from both the weather and the lighting documents, is **DLSS Quality (or STP where there is no DLSS) at 1440p**; native 1440p/60 is not promised by day in town or by night.

| GPU, native 1440p (est.) | ms | Owner |
|---|---|---|
| Base HDRP deferred + TAA + terrain + trees and impostors + 769k detail cells + 409 houses | 6-8 | the levels |
| Water System, sea + river at High256 with the CPU simulation | 2-3 | water |
| Volumetric clouds (Performance 48/4, cloud shadows) | 2 | weather |
| Sun PCSS at 4096, 4 cascades | 1.5-2 | lighting |
| Volumetric fog Medium, depthExtent 500 | 1.2 | weather |
| SSR with PBR accumulation | 0.7-1 | lighting |
| SSAO | 0.7 | lighting |
| Post (exposure, grain, ACES, drain) | 0.8 | lighting |
| TAA | 0.4 | |
| Sky update at 0.25 s, APV sampling, the frost pass (until it is a material feature), lamps' cached shadows | 1.2-1.5 | lighting |
| The weather's additions: Gerstner deformer 0.3, 2D cloud layer 0.2, spindrift 0.5 in gales, foam streaks 0.3 | 1.3 | weather |
| Wildlife: instanced flocks 0.3, grazing carpets 0.1, insects 0.1, sea life, herds later 0.4 | 1.0 | wildlife |
| **Sum, native** | **18.8-22.9** | |
| **With DLSS Quality (960p internal; screen-space costs fall about 40 %)** | **about 12-15** | |

Cut list in order when a scene misses the frame with DLSS on: cloud shadows off (-0.5), PCSS to PCF Medium (-0.8), fog Medium to Low (-0.5), water simulation 256 to 128 (-0.7), spindrift off (-0.5), the impostor distance, the cloud layer off (-0.2).

| CPU, main thread (est.) | ms at 1x | at 4x | Owner |
|---|---|---|---|
| The player's ship: 21 probe cells at 50 Hz on the CPU water, the integrator, `Physics.Simulate` | 0.6 | 1.6 | ship |
| Eight AI bodies at 9 cells, 25 Hz | 0.2 | 0.8 | ship |
| Audio: the model 0.1, surroundings taps 0.02, rays 0.05, DSP under 0.5 per buffer | 0.7 | 0.7 | audio |
| Wildlife: 400 bird transforms 0.3, the hourly model amortised, Burst matrices for big flocks | 1.2 | 1.2 | wildlife |
| Economy: lazy ticks averaged 1, 4 worst at a day rollover (once in 24 real minutes) | 1 | 1 | economy |
| Encounters: the hourly tick under 0.1 once a real minute; a 30-day jump 50 ms once; 2 ms on the frame a card opens | 0.2 | 0.2 | encounters |
| Weather engine and sea state: microseconds, no allocation | <0.1 | <0.1 | weather |
| Beats, pressure, landmark sights: once a game minute, 0.2 per tick | 0.2 | 0.8 | playability |
| The chain: preload at 4 ms a frame (`BelowNormal`) while sailing; the second water simulation 1.5 during the overlap; the cut frame under 25 ms; `UnloadUnusedAssets` one hitch (measured; deferred to the port call if over 50 ms) | 0 steady; 4-5.5 in the overlap | | gateways |
| **Sum of the game's systems, steady** | **about 4.2** | **about 6.5** | leaves the rest of the frame to HDRP's own CPU side |

Memory: a river place under 3 GB VRAM and 2.5 GB RAM resident; the overlap under 6 GB VRAM and 5 GB RAM; audio under 120 MB with the long clips trimmed; the economy 3 MB; the NPC registry 1-2 MB; the wildlife data under 200 KB. Every per-frame path allocates nothing after start (conditions compiled to delegates at load, pooled AudioSources, `Matrix4x4[]` built in Burst).

---

## 9. Rules of work (the user's standing rules, applied)

- Plan before building: this document and the nine are the plan; the questions in section 6 are what still needs the user's word.
- Stable first: compile, test, smoke, commit after each slice; the headless EditMode command is the gate; no blind code while the editor hangs; one level per headless run; the editor closed for batch runs.
- Never run Unity from a design pass; the builders make the scenes from data; nothing is hand-placed.
- Shippable data only, with the credit lines recorded (`CREDITS_*`); no invented sound; no home-made trees; detail only where it is seen from the boat.
- The user writes every line of text: placeholders with an intention and an outcome, `[write: key]` where nothing is written.
- The user's Build mode dresses the world; the anchors, corridors, depths, landmarks and berths are locked data and the checker guards them.
- Other tools edit the project too: `git status` first, assess, then decide.
