# The 3D ship view: how it comes home

For the lead. Written 7 Oct 2026 by the builder of the last part (the view in the chart). The whole story is
`BUILD_LOG.md` beside this file; its part 3 is how things stand. **Nothing here has been merged, and
`G:/kapman_unity/Kapman2` was never touched or run by any of the three builders.**

## 1. Where it is

- The copy: `G:/kapman_unity/Kapman2_ship`, a git worktree of the game's own repository.
- The branch: `shipview`. Never pushed.
- It holds main up to `259223e` (merged in three times, the last at 14:27 on 7 Oct). At 14:50 main was six commits
  further (`60b2554`); a trial merge of those into the branch (`git merge-tree`, nothing written) had no conflict,
  but they were not compiled or tested on the copy.
- **What it does NOT hold: the user's two asks recorded on main at 14:28 and 14:30** (DECISIONS items 34 and 36: a
  thin edge of land at the fog's edge, other ships passing by). The branch is "no land at all, a far soft mist"
  (items 28 and 29) and no other ship. The log's part 3 says where each of the two plugs in.
- Its own commits, oldest first:

| Commit | What |
|---|---|
| `ee91c43` | The stage, first run: the pack's ship on HDRP's water, four men seated in her |
| `2b14955` | The horizon a third down, clouds that read and drift, six moments under way |
| `0099c33` | A brighter fair day; the oars shipped at the berth; the ship filling the screen |
| `fb2cf2c` | (merge of main) |
| `c64bdbb` | After the merge: fog is not bright; the log |
| `249e2a8` | The rowing crew: three men each pulling a pair, one at the steering oar |
| `c876191` | Five looks to flip between, the console sea, the key L and the word |
| `c1f4dcd` | The looks lit by HDRP's own sky; every look at dusk, in fog, at night, at the jetty; the log, part 2 |
| `bbc2e66` | (merge of main) |
| `42245a3` | **Into the chart's frame: the window, the swap, no land, no racing, the cost cut** |
| `d3afd08` | (merge of main) |
| `664f35d` | This file, the log's part 3, and the view's tests made to clean up after one another |
| the last one | The two newer asks (items 34 and 36) noted as not built |

## 2. Every file it adds

All new, each with its `.meta` (91 files and metas in all). Nothing under `Packages` or `ProjectSettings`.

**The code, `Assets/Kapman/Code/Game/ShipView/` (assembly Kapman.Game, namespace `Kapman.Game.ShipView`), 21 files:**
`ShipStage.cs` (the stage), `ShipStageView.cs` (its camera and picture), `ShipStageState.cs` (the one record that
passes between the game and the stage), `ChartStageSource.cs` (the chart game read into that record),
`ShipStageHook.cs` (the view hung on the frame: the window, the Swap button, the key L), `ShipStageKit.cs` (the
kit's fields), `StageShip.cs`, `ShipRail.cs`, `StageDock.cs`, `StageSea.cs`, `StageWater.cs` (the mesh sea),
`StageHdrpSea.cs` (HDRP's water), `StageWake.cs`, `StageSky.cs`, `CrewBody.cs`, `RowingRig.cs`, `ShipCrew.cs`,
`PackCrewRig.cs`, `PsxStyle.cs`, `PsxSeaGrid.cs`, `PsxStageLook.cs`.

**The editor tool:** `Assets/Kapman/Editor/Game/ShipStageKitBuilder.cs` (menu Kapman > Ship view > Make the kit;
Make the kit anew).

**The kit and what it is made of, `Assets/Kapman/ShipView/`:**
- `Resources/ShipStageKit.asset` (found by name at run time: no scene holds it);
- `Materials/`: `SV_SMV_BasePallete.mat`, `SV_SMV_Environment.mat`, `SV_SMV_Weapons.mat` (the pack's three palettes
  on HDRP/Lit), `SV_Water.mat`, `SV_Shore.mat`, `SV_Wake.mat`, `SV_WakeFade.png`, `SV_Rope.mat`, `SV_Rain.mat`;
- `Shaders/`: `PsxCommon.hlsl`, `PsxModel.shader`, `PsxSea.shader`, `PsxSprite.shader`, `PsxScreen.shader`.

**The tests, `Assets/Kapman/Tests/PlayMode/`:** `ShipStageTests.cs`, `ShipCrewTests.cs`, `ShipLookTests.cs`,
`ShipViewChartTests.cs`, `ShipViewCostTests.cs` (explicit: a measurement).

**The design folder:** `Design/ship_view/BUILD_LOG.md`, `Design/ship_view/BRING_HOME.md`.

## 3. Every existing file it touches

**One file, one line.** `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs`, in `ThenNothingHoldsHerAtSea`, straight
after the check that the raiders' chase has ended:

```csharp
band.state = bandWas.state;      // (at once; ... )
```

Why: the test leaves the band sailing beside her after the chase and restores it a few frames later.
`RaiderEncounter.Look` goes by the wall clock (every half second) and has the band come on again as soon as the talk
is shut. Whether that half second falls inside those few frames depends on how long a frame takes; with the view on
a frame at sea takes longer, and the test's last check then found `RaidersComing`. With the line it passes with the
view on and off. (It also means: in the game a band that has just robbed her can come on again half a second
later, if it has room in its hold. I did not change the game.) If you would rather not take the line, drop it and
the test will fail now and then at sea with the view on, and now and then on any slower machine without it.

No line of the frame, of `ChartBuilder`, of any scene, of the HDRP asset or of the settings is changed. The hook
finds the frame's parts by their class names (`frame-ship-view`, `frame-log`); if those are renamed the hook finds
nothing and the drawn stand-in simply stays.

## 4. What the main project must have that git does not carry

All of it is in `G:/kapman_unity/Kapman2` today (the copy was made from it). It matters for a fresh checkout:

- **The pack, `Assets/Stylized_Medieval_Vikings_Pack`** (bought, ignored by `.gitignore`). The kit's asset points at
  sixteen of its prefabs by their GUIDs: `Prefabs - Environment/SMV_Ship`, `Oar_1`, `Plank_2`, `Plank_3`, `Plank_4`,
  `Short_log_1`, `Barrel_1`, `Crate_1`, `Sack_1`, `Sack_3`, `Rope_1`, `Bucket_1`, and
  `Prefabs - Characters/SMV_Viking_Settler_1`, `_Settler_2`, `_Settler_3`, `SMV_Viking_Karl_2`. The three `SV_SMV_*`
  materials point at the pack's palette pictures. **Without the pack the game still runs**: the stage stands plain
  boxes and posts in their places (it was built so), and Kapman > Ship view > Make the kit fills the kit again once
  the pack is there.
- Tracked already, and only pointed at: `Assets/Kapman/Water/Ship_Foam.mat` (her wake's foam),
  `Assets/Kapman/Sky/Stars_930.exr`, and HDRP's own `MaterialWaterExclusion.mat` in the package.
- Settings the view counts on, all as main has them: HDRP's water supported in `Assets/Kapman/Look/KapmanHDRP.asset`
  (without it the stage falls back to its mesh sea); layer 31 free (it needs no name); `L_Chart` in the build's scenes.
- Nothing from the look lab (`KapmanSail_dev`) is needed at run time: what was taken from it is numbers and ways
  of doing things, written into the view's own code.

## 5. The steps

1. In `G:/kapman_unity/Kapman2`, with no Unity open on it: `git status`. The branch touches one file main also
   has, `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs`: if that file has uncommitted changes git will refuse the
   merge until they are committed. Uncommitted work in any other file can stay as it is.
2. `git merge shipview`. Main as far as 259223e is already in the branch (merged there without a conflict), so
   only the view's own files and that one line come over. If main has moved on and `ChartGameTests.cs` conflicts,
   keep main's side and put the one line of part 3 back, or leave it out.
3. Open Unity on the project once (it imports 21 scripts and four shaders; no reimport of the pack).
   If the ship window stays the drawn boat, run Kapman > Ship view > Make the kit and look at the Console.
4. EditMode, the whole project. On the copy: 383 run, 381 passed, 2 skipped, 0 failed.
5. PlayMode with graphics (no `-nographics`), filter `(Ship(Stage|Crew|Look|ViewChart|ViewCost)Tests|ChartGameTests)`.
   On the copy: 20 run, 20 passed, in six minutes (main's four chart tests, the view's fifteen, and the cost's measure).
6. **Look at the pictures**: `Captures/ship_view/chart/` (the chart game with the view in it, 30 pictures, and
   `cost.txt`, `chart.txt`), `Captures/ship_view/` (the stage alone), `Captures/ship_view/looks/`, `.../crew/`.
   The log's part T says what each should show.
7. The cost alone, on any machine: PlayMode filter `Kapman.Tests.ShipViewCostTests` (explicit; one minute; it writes `cost.txt`).
8. Play it: open L_Chart, press Play, begin a game. The ship window is bottom right.

## 6. Switches

| To | Do |
|---|---|
| Run the game or its tests with no 3D view | `KAPMAN_SHIP_VIEW_OFF=1` in the environment; or `ShipStageHook.StartsOff = true` (one constant) |
| Take the view down and put it up while playing (for a setting) | `ShipStageHook.TurnOff()`, `ShipStageHook.TurnOn()` |
| The look it starts in | `startLook` on the kit's asset: `plain`, `console`, `lowpoly`, `screen`, `ink`. The look last chosen with L is kept in the settings (`kapman.shipview.look`) |
| Land behind the jetty again | `land` on the kit |
| How far one sees, how high the mist lies | `mist` (900 m; 0: as far as the weather lets one), `mistTop` (70 m) |
| The boat's size | `shownLength` (0: her class's own 7.7 m) |
| Cheaper | `smallRate`, `bigRate`, `smallRateRiding`, `bigRateRiding` (pictures a second), `bigHeight` (lines when she fills the screen); `sea` = Mesh (no HDRP water at all) |
| A seaway cheaper without changing the view | `waterFullCPUSimulation: 0` in `Assets/Kapman/Look/KapmanHDRP.asset` (the lead's call: the levels' ships float on that copy of the waves) |

## 7. Things to know before building on it

- **The five-speed clock** (DECISIONS item 26). The view takes the clock's speed from `Time.timeScale` (that is the
  game's speed today: `GameSpeed`) and uses it for one thing only: is it 0. If the new clock stops setting the time
  scale, change one line in `ChartStageSource.Read` (`s.timeFactor = ...`), or the water will go by under a card.
  The stage itself counts real seconds and needs nothing.
- **The frame.** If the ship window's element loses the class `frame-ship-view`, the view does not show (and says
  nothing). `ShipViewChartTests` fails on it at once.
- **The seaway's cost** is the one thing I would not call done (log, part W).
- **Before the copy is ever deleted**: it has four junctions into the main project (`Art`, and
  `MapArt/world/L1`, `L2`, `L3`). Take them away with `rmdir` (cmd: `rmdir G:\kapman_unity\Kapman2_ship\Art`, and
  the three under `MapArt\world`) BEFORE `git worktree remove`. A tool that follows a junction would delete the
  main project's art. The copy's `Packages` and `ProjectSettings` show as changed there (they are the main
  project's own uncommitted settings, copied in to make it run): never commit them from the copy.
- A file `Design/ship_view/USER_RULINGS_READ_FIRST.md` lies in the copy, not in git. It is the lead's own note
  (the same text heads `JOIN.md` on main). It can go with the copy.
