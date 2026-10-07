# Routes fix: four ports with no way, and the Maas mouth

7 October 2026. No Unity run. I read the code and the bake scripts and walked
`G:/kapman_unity/data/baked/world/lanes.bin` with a script (plain shortest way, the router's base costs,
dry legs times 6; not the game's A-star and without its shallow-water rule).

## 0. The short answer

- **One cause for all five.** The player's profile shuts every lane of kind 4 ("pending"):
  `Assets/Kapman/Code/Game/Voyage/Course.cs` line 64,
  `p.cost[LaneGraph.Pending] = float.PositiveInfinity; // (its water isn't in the 3D world yet)`.
- "Pending" means: a river that had no water yet in the old 3D world. The chart game has no 3D water,
  so the reason is gone. Nothing in the data is broken or missing.
- **Only the player is shut out.** The world's ships use `LaneRouter.Profile.For` (`Sim/Travel.cs` line 57),
  where pending costs 1.3 a metre (`Sim/LaneGraph.cs` line 182). They already sail these lanes.
- All four berths ARE joined to the lanes, each only by a pending gate. None is too far from a lane.
- The Maas mouth is three short pending edges (3.6 km) between two carved river legs. They have water
  (depth 4.2 to 4.6 m, not flagged dry). They are pending only by a rule of the bake.
- **Smallest fix: 12 edges get kind 3 (river) in place of 4.** Three at the Maas mouth, nine in four gates.
  Either as a small allow-list in `Course.Profile`, or by changing the bake's inputs and baking the lanes again.
  Do not simply open all pending lanes: that opens 31,556 km of rivers (section 4).

## 1. How the planner reads the lanes

| What | Where |
|---|---|
| The file | `G:/kapman_unity/data/baked/world/lanes.bin`, format KLN1, baked 2 Oct 07:31. Path set in `KapmanGame.cs` line 73 (`lanesFile`) and `Sim/Travel.cs` line 106 (`DefaultLanes`). It is outside the project: a build will not carry it |
| The reader | `Sim/LaneGraph.cs` `Load`, line 38. Per edge: `edgeKind` (byte), `edgeFlags` (byte), `edgeLength`, `edgeDepth` |
| The kinds | `LaneGraph.cs` line 20: Sea 0, Coastal 1, Sound 2, River 3, **Pending 4**, Flats 5, Portage 6, Approach 7 |
| The flags | line 21: Gate 1, Uncertain 2, Dry 4 |
| The player's profile | `Voyage/Course.cs` `Profile`, lines 61 to 76. Pending and Portage are infinity; `dry = 6`; `minDepth = 2` |
| The planner | `Course.Plan`, lines 91 on. It tries off the flats, then with flats (line 119). False if neither finds a way |
| A second user of the same profile | `UI/Frame/FramePassage.cs` line 129 |
| Pending is otherwise treated as river | `Chart/ChartWorld.cs` line 184; `LaneGraph.cs` lines 201 and 212 (no depth rule on it) |

The file holds 26,045 nodes, 116,596 edges, 50 berths, 16 gates. By kind: river 311, **pending 662**, portage 5.
Of the 662 pending edges, 21 are gate edges, 14 are flagged dry, 11 uncertain.
The player reaches 25,354 nodes today; with pending open, 26,042.

## 2. Where "pending" is set

The baker is not in Kapman2. It is in the first project: `G:/kapman_unity/Kapman/Bake/lanes/`
(`bake_lanes.py` runs it; `README.md` there explains it; work files in `G:/kapman_unity/data/work/lanes/`).

| Source of a pending edge | File and line |
|---|---|
| A gate whose row says `pending` | `gates_793.csv`, column `kind` |
| A HydroRIVERS river (mean flow 30 m3/s or more) where no carved river runs | `lanes_rivers.py` line 246; its join to a carved river, line 255 |
| A gap between two river pieces, joined straight (up to 1.5 km): pending if either side is pending | `lanes_graph.py` line 640 |
| The same, by a path through wet ground (up to 12 km) | `lanes_graph.py` line 690 |
| A river piece cut off from the sea, joined to it: pending if it steps over land or does not meet river or sea | `lanes_graph.py` line 788 |

The README says it in one line (line 165): kind 4 is "a river still waiting for its 3D water (HydroRIVERS) or a
gate river; off-screen only". Its own check (line 238): "50/50 have a berth: 23 sea, 4 carved river,
23 pending river, 0 unreachable". The four ports are among the 23.
`G:/kapman_unity/data/work/lanes/reachability.csv` rows 8, 12, 25, 35 say the same: `reach = pending river`.

## 3. Each case

Edge numbers are indexes in today's `lanes.bin`. They change if the lanes are baked again.

### Quentovic: joined, only by a pending gate

- Berth node 9707 at 50.4780 N 1.6930 E. One edge at the berth: 47938, pending, flags 5 (gate, dry),
  11.75 km, depth unknown (-1), gate `canche`.
- The gate's other edge: 47937, pending, flags 1, 2.2 km, from the gate's sea end to the lanes.
- Nearest node the player can reach: node 9612, 9.8 km off, at the Canche's mouth.
- Cause: `gates_793.csv` row `canche`, `kind = pending`. The README: "no water in the tile store".
- Way from Dorestad with it open: 371 km.

### Ripa: joined, only by a pending gate

- Berth node 15062 at 55.3300 N 8.7620 E. One edge: 69341, pending, flags 5 (gate, dry), 6.4 km, gate `ribe_a`.
- The gate's other edge: 69283, pending, flags 1, 3.3 km, to the lanes at 55.3610 N 8.6487 E.
- Nearest node the player can reach: node 14912, 7.9 km off.
- Cause: `gates_793.csv` row `ribe_a`, `kind = pending`.
- Way from Dorestad: 562 km. It needs no other pending lane (it goes north by the Old Rhine or the Vlie).

### Doccinga: joined, only by a pending gate, and the gate is marked uncertain

- Berth node 12568 at 53.3260 N 5.9990 E. One edge: 59476, pending, flags 7 (gate, uncertain, dry), 6.95 km,
  gate `dokkumer_ee`.
- The gate's other edge: 59664, pending, flags 7, 4.8 km, to the lanes at 53.3174 N 6.1220 E.
- Nearest node the player can reach: node 12680, 8.1 km off.
- Cause: `gates_793.csv` row `dokkumer_ee`, `kind = pending`, name marked "(?)": the course in 793 is a guess.
- Way from Dorestad: 274 km. No other pending lane on it.

### Fordwic: joined, only by a pending gate that hangs off Sarre

- Berth node 9437 at 51.2967 N 1.1296 E. Two edges: 46749, pending, flags 5, 8.4 km; 46748, pending, flags 1,
  191 m. Both gate `stour_fordwic`.
- The gate's third edge: 47013, pending, flags 5, 505 m, which joins the Stour to the Wantsum at Sarre.
- Nearest node the player can reach: node 9480, 8.8 km off. That node is Sarre's berth.
  Sarre works because its gate `wantsum` is kind `sound`, though it is flagged dry too.
- Cause: `gates_793.csv` row `stour_fordwic`, `kind = pending`.
- Way from Dorestad: 330 km.

### The Maas mouth: three gap edges between two carved rivers

| Edge | Kind | Flags | Length | Depth | From | To |
|---|---|---|---|---|---|---|
| 51866 | pending | 0 | 1,277 m | 4.5 | 51.9034 N 4.2722 E | 51.8984 N 4.2891 E |
| 51872 | pending | 0 | 773 m | 4.2 | 51.8984 N 4.2891 E | 51.8979 N 4.3005 E |
| 51883 | pending | 0 | 1,578 m | 4.6 | 51.8979 N 4.3005 E | 51.8915 N 4.3213 E |

- No gate. Not dry, not uncertain. Each is a straight line of two points.
- West of them: river edge 51741 (10.4 km, depth 4.5), on to Witla and the sea.
  East of them: river edges 51885 and 51886 (26.5 km, depth 3.5), up the Lek.
- So the carved river network has a 3.6 km hole here, and the bake filled it.
- Cause: not a flag anyone set by hand. One of two rules of the bake made them, and the file does not say which
  (the `src` field is not written to `lanes.bin`):
  1. `lanes_rivers.py` line 246: where the carved river is missing, the HydroRIVERS line fills in, as pending.
     The three depths differ, which fits pieces with their own river-model depth. This is the likelier one.
  2. `lanes_graph.py` lines 640 or 690: a gap join, pending because one side was already pending.
- The README's open issue (line 252) names the root: "Rivers v2 isn't cut into the tile store yet ... v2 also
  has gaps ... joined by gap paths, some as pending legs. Bake the lanes again after the river bake."
- These three edges are on the way to Witla (111 km), Walichrum (166), Sarre (321), Lundenwic (411),
  Fordwic and Quentovic. All but the last two have another way today, so the player gets a way, only a longer one.

### The other seven ports

Dorestad, Traiectum, Medemelacha, Daventre: a way with no pending lane. Witla, Walichrum, Lundenwic, Sarre:
a way today, shorter by the Lek once the Maas mouth is open.

## 4. The smallest fix, three ways

**A. In code, one allow-list (smallest, no bake).** In `Course.Profile`, keep pending shut but let through a
named set. `LaneRouter.Profile` has no per-edge hook today, so this needs one of:

- a new field on the profile, for example `HashSet<int> open`, checked in the router where it reads
  `profile.cost[g.edgeKind[e]]` (`LaneGraph.cs` line 284) and in `LaneGraph.Nearest` (line 144); or
- after `LaneGraph.Load`, re-kind the edges once: every edge of gates `canche`, `ribe_a`, `dokkumer_ee`,
  `stour_fordwic` (read from `g.gates`, so no edge numbers in code) from Pending to River, and the three Maas
  edges found by place (pending, no gate, within 3 km of 51.897 N 4.297 E).
  This also opens them a little cheaper for the world's ships (1.1, not 1.3).

Twelve edges in all: 47937, 47938; 69283, 69341; 59476, 59664; 46748, 46749, 47013; 51866, 51872, 51883.

**B. In the bake's inputs (the clean fix).** In `G:/kapman_unity/Kapman/Bake/lanes/gates_793.csv` change `kind`
from `pending` to `river` in the rows `canche`, `ribe_a`, `dokkumer_ee`, `stour_fordwic`. For the Maas mouth add
a gate row of kind `river`, `join = both`, over the three points above. Then run the graph step of
`bake_lanes.py` (step 2, rivers, need not run again). Not checked: whether a gate laid over an existing pending
edge replaces it or lies beside it; and whether the bake still runs from the old project. Edge numbers will change.

**C. Open all pending lanes** (delete line 64 of `Course.cs`). One line, but see below.

I would do A by the gates' names now, and B when the lanes are next baked.

## 5. What could go wrong if pending lanes are simply opened

1. **31,556 km of rivers open at once**, on 688 more nodes: the Rhine to Mainz, the Seine to Paris, the Scheldt,
   the Loire, the Garonne, the Ouse, the Dee, the Tyne, the Neva and the Volkhov, and every HydroRIVERS river of
   30 m3/s or more. The first release has twelve ports. The course line, the measure and `Nearest` (a click near
   a river far inland) would all start to use them.
2. **Short cuts through the land.** The README names the Waal at Nijmegen, joined by gap paths as pending legs.
   With pending open, Dorestad to Daventre or to the sea may go by a way that is not there. Check the
   Dorestad to Daventre and Dorestad to Witla lines after any wider opening.
3. **No depth on them.** Gate edges have depth -1, and `ShallowFactor` and `Avoids` skip pending
   (`LaneGraph.cs` lines 201, 212). Anything that reads `edgeDepth` for the player (soundings, the bar sign,
   `Course.Depths` line 162 on) gets -1 or a model number, not sampled water.
4. **Dry flags.** 14 pending edges are flagged dry. The player's `dry = 6` makes each cost six times its length
   in the search. A way is still found where it is the only one (all four ports), but the planner's choice
   between two ways tilts, and if any time or distance is read from the cost and not the length it is six
   times too long. Check that `km` comes from lengths.
5. **Uncertain history shown as fact.** 11 pending edges are flagged uncertain: `dokkumer_ee` (Doccinga),
   `treene`, `fyris_uppsalir`, `truso_elbing`, `alande_grobin`. Doccinga is in the twelve, so its guess is in
   the first release either way; the others need not be.
6. **Lake Ladoga and the like.** `ladoga_south` is a pending gate over water the tile store does not have.
   Not in the twelve ports, but open to a click.
7. **The chart's art.** These rivers were never drawn for the boat. On the 2D chart her mark may run over
   land where the map tiles show no river (the Canche, the Ribe A, the Dokkumer Ee and the Stour are all
   "course approximate" in `gates_793.csv`). This holds for fix A too: look at the four on the chart.
8. **River state.** `ChartWorld.cs` line 184 counts pending as river, so she is "on a river" on them
   (river sounds, river speed rules). Likely wanted; not checked in play.

## 6. Not checked

- No run in Unity. `Course.Plan` itself was not called; my walk copies its base costs only.
- Which of the two bake rules made the three Maas edges. The work files
  (`G:/kapman_unity/data/work/lanes/rivers_lanes.pkl`) would say; I did not open them.
- Whether the old bake still runs, and how long it takes.
- Whether the chart's map tiles show water at the four gates and at the Maas mouth.
- The (?) on `dokkumer_ee`: the history, not the data.

## 7. Test for the fix

`Course.Plan` returns true from Dorestad's berth to each of the twelve ports. Dorestad to Lundenwic is
about 411 to 415 km and its kinds hold no stretch by Traiectum. Dorestad to Daventre is still 179 km.
The count of nodes the player reaches rises from 25,354 by a few tens, not by 688.
