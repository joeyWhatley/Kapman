# READ THIS FIRST: THE USER'S RULINGS SINCE THIS WAS WRITTEN (7 Oct)

- **The boat is the pack's ship** (SMV_Ship), not the Gislinge and not as this file first assumed in every detail: see Design/DECISIONS.md items 19 and 21.
- **HER SIZE: 7.7 m WITH SIX OARS** (the user chose by the picker at 13:05, from the builder's own side-by-side pictures size_7_7m_oars.png and size_9_0m_oars.png): her true length, three men each pulling a pair, one at the steering oar. If the build stands at 9 m with three oars, set it to 7.7 m with six before any more pictures are made, and say so in BUILD_LOG.md.
- **NO LAND, EVER** (the user, 13:10): "from the 3D view it should be very restrictive you cant see anything other than the boat and the water and just the port no land". So the view shows only the boat, the water and the sky, and when she is moored the jetty itself (its boards, piles, barrels and lines) standing in the water: no bank behind it, no shore, no river banks, no hills, no far coast, no buildings. Take out the green bank of picture 6 and anything else that is ground. Close the distance with mist so there is no bare horizon line to show that nothing is there, and keep the camera near and a little above her so the water fills the frame. This holds for every place (river, coast, open sea, in port) and every look.
- **A THIN EDGE OF LAND FROM WHERE SHE REALLY IS** (the user, about 14:00, loosening "no land"): "Is there a way to use a super complex but simple land generation technique based on where the boat is at any time, so it creates more depth and complexity to the 3D ship view? Not much just 2-5 meters and the rest is a fog in PSX style." So: still no landscape, no far shore, no hills. But where the chart says land is truly close on a side (a river bank, a quay, a shore she is hugging), the nearest 2 to 5 metres of it stand at the edge of the fog on that side, and everything beyond is fog. Make it from data, not by hand: each moment ask the chart how far the nearest land lies to port and to starboard and what kind of ground it is (the chart's own classes: shore sand, marsh and reed, cleared land, woodland, high ground, a town), and build a low strip of bank a few metres deep from short pieces that are made ahead of her and dropped astern as the water flows past, each piece shaped and dressed by a number made from its place on the chart, so the same reach looks the same every time she passes. Dressing by kind (reed, posts, a sand lip, roots, a wattle fence) from the user's packs or as flat sprites the user will draw (give them art slots); never home-made trees. Far land is only fog; the fog is the PSX kind: a short draw distance closed by a flat colour. On open sea nothing changes.
- **OTHER SHIPS PASS BY, ON THE CHART AND IN THE VIEW** (the user, about 14:10): "With proximity ships passing by and you can see them passing on the map plus the 3D view." The world has 138 other ships that really sail (Assets/Kapman/Code/Game/Sim: WorldSim, SimShip, SeaWatch, which already tells the log when a sail is sighted). When one of them is near her on the chart it is drawn passing on the chart, and it comes out of the fog in the 3D view on the side and heading it truly has relative to her, passes, and goes back into the fog. The distance is squeezed to fit the short view (a ship that passes within a few hundred metres on the chart passes within sight in the view; one far off stays in the fog): feel before measure. Use the pack's ship with a different sail or no sail so they are not all her twin; a boat under oars near a port, a sail at sea. The ship the player hails or that stops her is the one they can see.
- **The clock now has a pause and five speeds** (Design/DECISIONS.md item 26: at speed 4 a day passes in about fifteen seconds, at 5 in about four). The view must not race: it keeps its own calm motion of sea, oars and sail at every speed, and lets the sky and the light pass.

---

# JOIN: the stage and the look/crew halves, under the two rulings (7 Oct)

Written from a read of PLAN.md (parts 5, 9, 11), LOOK_AND_MOTION.md (parts 7, 9), and the drafts
ShipViewJoin.cs (all of it), Editor/ShipViewSeating.cs (all of it), and the field lists of StageShip.cs, ShipStageKit.cs,
ShipStageState.cs, RowingRig.cs, ShipCrew.cs. Nothing was compiled or run. Line numbers are those of the drafts as they
stood at 11:13 on 7 Oct.

## 0. The rulings and what they settle

1. **True size.** The pack longship shrunk to 7.7 m (7.7 / 17.88 = 0.431), full-size men in her. This is PLAN.md part 9
   way A (lines 506-509). Way B (lines 510-512, the 18 m longship with her own benches) is dropped.
2. **Six oars, four men.** Three thwarts, a pair of oars to each, one man pulling each pair, one man steering.
   PLAN.md questions 1 and 2 (lines 560-564) are answered; the drafts already follow the table.

**The short answer: the join draft (ShipViewJoin.cs, `PackCrewRig`) was already written for exactly this** (one man to a
thwart, a hand to each oar through `RowingRig.Rower.oar2`, a seated helmsman). No field of the join table is overturned.
What the rulings make wrong is the seating tool's prefab (section 3) and a few figures that were measured for the full-size
hull (section 2). Three things in the drafts are not wrong by ruling but will show at 7.7 m (section 2, E1 to E3).

## 1. The join table (PLAN.md part 9), field by field, under the rulings

| Field | Wired in ShipViewJoin.cs | Under the rulings |
|---|---|---|
| `ShipViewLook.hdrpSea` | line 62 | stands |
| `ShipViewLook.gridSea` | lines 44-48, 63 | stands |
| camera drawn by the look | lines 67-69, 76 | stands |
| `ShipViewLook.Show` | lines 69, 74-75 | stands |
| `ShipViewLook.Recollect` | line 70 | stands; it must also run after the crew is seated (E4) |
| `ShipViewLook.swaps` | lines 64, 109-130 | stands; the men's material `SMV_BasePallete` must be in `kit.swaps` (E5) |
| `ShipViewLook.lightFromClock` | lines 65, 185-191 | stands |
| `PsxSeaGrid.SetSea`, `flow` | lines 173-175 | stands |
| `PsxSeaGrid.level`, `ship`, `waterline`, `hullHalfLength`, `hullHalfBeam` | lines 49-54, 177-178 | stands: `Ship.Length * 0.46` = 3.54 m, `Ship.Beam * 0.5` = 1.08 m. Their defaults (1.2, 8.3, 2.5) are never used on the stage |
| `stage.HeightAt` | lines 179-182 | stands |
| `ShipCrew.sailSet`, `sailFurled` | lines 347-352 | stands |
| `ShipCrew.steerOar` | left empty on purpose (line 361): the stage turns the oar | stands |
| `RowingRig.Oar.thole`, `pivot`, `shaft` | lines 377-399 | stands in shape; the thole's place needs the rail's curve (E1) |
| `RowingRig.Rower.seat` | lines 311-323: `ship.Seats` only | stands. **`SideSeats` is not used**: PLAN.md line 498 ("with four oars that is two thwarts and four SideSeats") is out of date, see section 4 |
| `ShipCrew.inputs` | line 274, `From` at 280-290 | stands; E6 for the two hard-coded speeds |

## 2. Edits to the drafts

### E1. The tholes must follow the rail's curve (StageShip.cs lines 122-135)

StageShip puts every thole at `side * halfBeam` (line 135) and every thwart at one width (line 125). The three thwarts
are at z = 0.17 x 7.7 - i x 0.9 = **+1.31, +0.41, -0.49 m** and their tholes 0.3 m aft: **+1.01, +0.11, -0.79 m**.
In the model's own metres (divide by 0.431) those tholes are at z = 2.34, 0.26, -1.83. The measured rail
(ShipViewSeating.cs lines 32-36) there is x = 2.20, 2.47, 2.28, y = 2.16, 2.09, 2.14; times 0.431: half-beam
**0.95, 1.06, 0.98 m**, not 1.08 for all three. So the forward pair of tholes hangs 0.13 m outside the hull and the
after pair 0.10 m, and the forward thwart pokes through the planking.

The rail table is in the Editor assembly; the stage needs it at run time. Move it (new file, Kapman.Game):

```csharp
// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/ShipRail.cs      assembly: Kapman.Game
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>The pack longship's rail top, starboard side, in the model's own metres (z toward the bow, x, y over
    /// the model's origin). Measured from Models/SMV_Ship.fbx. The port side mirrors. Times StageShip.Scale on the stage.</summary>
    public static class ShipRail
    {
        static readonly float[,] Rail =
        {
            { -5.25f, 1.57f, 2.47f }, { -4.25f, 1.87f, 2.30f }, { -2.21f, 2.22f, 2.15f }, { -1.00f, 2.41f, 2.11f }, { 0f, 2.48f, 2.09f },
            { 1.00f, 2.41f, 2.12f }, { 2.21f, 2.22f, 2.15f }, { 4.30f, 1.87f, 2.30f }, { 5.25f, 1.57f, 2.47f },
        };

        public static void At(float z, out float x, out float y)
        {
            int last = Rail.GetLength(0) - 1;
            z = Mathf.Clamp(z, Rail[0, 0], Rail[last, 0]);
            for (int i = 0; i < last; i++)
                if (z <= Rail[i + 1, 0])
                {
                    float t = Mathf.InverseLerp(Rail[i, 0], Rail[i + 1, 0], z);
                    x = Mathf.Lerp(Rail[i, 1], Rail[i + 1, 1], t);
                    y = Mathf.Lerp(Rail[i, 2], Rail[i + 1, 2], t);
                    return;
                }
            x = Rail[last, 1];
            y = Rail[last, 2];
        }
    }
}
```

StageShip.cs, in `Fit`, inside the loop over the pairs. Line 125 (the thwart's width), replace `halfBeam` by the rail at
that thwart:

```csharp
ShipRail.At(z / Scale, out float thwartRailX, out _);
float halfHere = thwartRailX * Scale;
thwart.transform.localScale = Vector3.Scale(thwart.transform.localScale, new Vector3(2f * halfHere * 0.92f / kit.thwartLength, 1f, 0.7f));
```

Line 135 (the thole), replace with:

```csharp
float tholeZ = z - 0.3f;                                                  // the thole lies aft of the thwart
ShipRail.At(tholeZ / Scale, out float railX, out float railY);
oarPivots.Add(new Vector3(side * (railX - 0.03f) * Scale, (railY - kit.modelWaterline) * Scale + 0.03f, tholeZ));
```

(This holds only while `kit.scaleToClass` is on and the kit's ship is the pack longship. If `Scale` is 1 the same lines
give the full-size rail, so nothing else has to know.) The oar spawned at line 134 should take the same x and y as its
pivot. `PackCrewRig.Oar` (ShipViewJoin.cs lines 390-392) already works the handle's length from the pivot's x, so the
narrower forward pair gets shorter grips by itself: grip = 0.95 - 0.22 = 0.73 m forward, 0.84 amidships, 0.76 aft.

### E2. The oar's figures at 7.7 m (ShipViewJoin.cs lines 219, 388-397): check, no change proposed

Worked through with the numbers the drafts give (Oar_1: 0.47 m inboard, blade middle 1.70 m out, stretch 1.5):

- thole over the water: Gunwale = (2.09 - 1.2) x 0.431 = 0.38 m, plus 0.03 = **0.41 m**
- amidships: grip 0.84 m, shift = 0.705 - 0.92 = **-0.22 m** (the oar is slid inboard, not outboard as on the longship),
  bladeOut = 2.55 - 0.22 = **2.33 m**, so the oar slopes about asin(0.51 / 2.33) = **13 degrees** to put its blade 0.10 m under
- the two handles of a pair end 0.14 m either side of the centre line: they do not cross
- the oar is 2.65 x 1.5 = 3.98 m; a hand on each, 0.44 m apart; the handle about 0.6 m over the water, the hips about 0.3 m: at the chest

That is a sound small-boat stroke on paper. It has not been run in `check/` with the pair (LOOK_AND_MOTION says the
sweep of 20 / 22 degrees was "found by running it"; confirm that run used a pair and a 0.41 m thole, not the 0.9 m freeboard).

**One thing to look at in the code, not verified here:** the rower faces aft, so his LEFT hand is on the STARBOARD oar.
The join gives `oar = port, oar2 = starboard` (line 322). Read RowingRig.cs near line 329 ("the inner hand is the one
further from the rail") and make sure that with `oar2` set, each hand goes to the oar on its own side (by the oar's
`side` against the seat's right, not by the order oar / oar2). If it goes by order, the arms cross. Safe form:

```csharp
// in RowingRig, where a pair's hands are chosen: the oar whose thole lies on his left takes his left hand
bool firstIsOnHisLeft = Vector3.Dot(oars[r.oar].thole.position - r.seat.position, r.seat.right) < 0f;
```

### E3. Rowing rates: one number (StageShip.cs line 55)

`StageShip.strokesPerMinute = 26f` (the stage's own sweep when no crew is aboard) against the crew's 22 / 18 / 24
(RowingRig.cs line 132, ShipCrew). With a crew the rig rules (`OarsDriven`), so nothing clashes; make the fallback agree:

```csharp
public float strokesPerMinute = 22f, thwartSpacing = 0.9f;
```

### E4. The look must collect the men (ShipViewJoin.cs, `PackCrewRig.Fit`, after line 374)

The men are instantiated after the look made its list of renderers. `stage.Adopt(go)` (line 417) puts the HDRP twins
on; the console styles then miss them until the stage next raises `Changed`. If `ShipStage` has no public way to raise
it, add one; then, after `crew.Build();`:

```csharp
stage.RaiseChanged();      // (ShipViewLook.Recollect hears it: the men and the re-parented oars are new renderers)
```

Also the oars were re-parented and stretched (lines 387-388): `Detach` (lines 248-261) destroys `holder`, **and the
stage's six oars with it**, since they now hang under holder / thole / pivot. LOOK_AND_MOTION line 385 names this. Fix in
`Detach` and at the top of `Fit`, before `Object.Destroy(holder)`: hand the shafts back.

```csharp
void GiveOarsBack()
{
    if (rig == null || ship == null || ship.FitOut == null)
        return;
    foreach (var o in rig.oars)
    {
        if (o.shaft == null)
            continue;
        o.shaft.SetParent(ship.FitOut, true);
        var s = o.shaft.localScale;
        o.shaft.localScale = new Vector3(s.x, s.y / OarStretch, s.z);
    }
}
```

Call `GiveOarsBack();` as the first line of `Detach` and as the first line of `Fit`. (Or simpler: have `Detach` ask the
stage to fit her out again, which makes fresh oars. Either way the stage must not be left with six dead references in
`OarsPort` / `OarsStarboard`.)

### E5. The men's prefabs and material belong in the kit (ShipStageKit.cs after line 51; ShipStageKitBuilder.cs near line 58)

`PackCrewRig` takes `menPrefabs` from its caller and the kit has none (LOOK_AND_MOTION line 385). Four men are now a
fixed part of the boat, so:

ShipStageKit.cs, after line 51:

```csharp
[Tooltip("The crew: the pack's men, taken in turn (three at the oars, one at the helm for the Gislinge boat).")]
public GameObject[] men = new GameObject[0];
```

ShipStageKitBuilder.cs, beside `kit.thwart = P("Plank_4");` (line 58). The names are ShipViewSeating's list (lines 46-50),
a trading crew, settlers first; `C` is a twin of `P` that loads from `Prefabs - Characters/`:

```csharp
kit.men = new[] { C("SMV_Viking_Settler_1"), C("SMV_Viking_Settler_3"), C("SMV_Viking_Karl_2"), C("SMV_Viking_Settler_2") };
```

and in its `Palettes` (line 32 block) make sure the men's material is there, or they draw brown (PLAN.md R5):

```csharp
("SMV_BasePallete", "BasePallete.png"),                       // the ship and the men
```

The caller's line (LOOK_AND_MOTION line 363, ShipViewJoin.cs line 8) becomes:

```csharp
ShipStage.Current.Crew = new PackCrewRig(ShipStage.Current.kit.men, readTheChart: true);
```

### E6. `PackCrewRig.From` (ShipViewJoin.cs line 288): the class's own speeds

`sailKnots = 4f`, `oarKnots = 2.5f` are stand-ins. They only feed the test's fake state, and the line already forces the
rule to agree with `s.underOars`, so it is harmless. Leave it; note it in the test.

### E7. How many men: the crew count, not the seat count (ShipViewJoin.cs lines 311-323, 353-355)

The loop seats a man on every thwart and then a helmsman: 3 + 1 = 4, which is the ruling. But `s.crew` comes from
`Voyage.Men` (ChartStageSource.cs line 64) and can be fewer after losses. If the lead wants the picture to show the men
she has, bound the loop; the helmsman is kept first:

```csharp
int rowersAboard = Mathf.Clamp(stage.State.crew - 1, 0, ship.Seats.Count);
for (int i = 0; i < ship.Seats.Count; i++)
{
    ...
    var man = i < rowersAboard ? Man(manIndex++, "Rower " + (i + 1)) : null;
```

(An unmanned thwart keeps its pair of oars shipped: give those oars no rower and leave them out of `rig.oars`.) Not
required by the ruling; a choice for the lead. Without it the picture always shows four.

## 3. The seating tool (Editor/ShipViewSeating.cs): what must change

Under the rulings the seating is done **at run time** by `StageShip.Fit` (thwarts, oars, seats) and `PackCrewRig.Fit`
(men, tholes, grips). The tool's prefab is the thing the rulings make wrong:

| Lines | What it does | Under the rulings |
|---|---|---|
| 28-29 `PrefabPath` ShipView_Ship.prefab | the 18 m longship with men on her own benches | **Not to be used as `kit.ship`.** `kit.ship` stays the pack's plain SMV_Ship |
| 32-36 `Rail` | the rail at full size | **keep the figures, move to ShipRail.cs** (E1); the tool calls `ShipRail.At` |
| 40 `OarInboard = 0.764`, `OarBlade = 1.85` | Oar_2's measures | wrong oar: the stage fits `Oar_1` (0.47, 1.70; ShipViewJoin.cs line 219). Delete with `Build` |
| 44 `SeatInboard = 0.56`, `SeatAhead = 0.30` | one man to an oar, sat 0.56 m in from his rail | wrong: the man sits on the centre line. `SeatAhead` 0.30 lives on as StageShip's `z - 0.3f` |
| 46-50 `Men` | eight names | keep; it feeds `kit.men` (E5). Four are needed |
| 52-58 `BuildFour`, `BuildEight`, `BuildBatch` (4 oars) | menus | **wrong: four and eight oars, one man each.** Remove, or replace by the one preview below |
| 65-211 `Build(rowers, oarLength, shields)` | benches found by name, a thole and seat per bench side, `rig.waterline = 1.2f` (164), helm stand at (0.78, 1.87, -6.75) (173), grip under the steering oar (182) | **wrong throughout for a 7.7 m hull**: her benches are hidden (`kit.hide`), the helmsman sits (`helmSits = true`), the grip must not be parented under the stage's oar. Remove |
| 213-228 `RailAt` | | becomes `ShipRail.At` |
| 238-263 `Man` | puts weapons away, `updateWhenOffscreen` | duplicated at run time in ShipViewJoin.cs lines 401-419; the editor copy goes with `Build` |
| 265-316 `Materials` | HDRP and console materials as assets | **keep.** The join makes console twins at run time instead (lines 109-130), so this is now optional: either drop it, or have the join prefer these assets when they exist. Do not make both and use neither |

What the tool should be after the change: a **preview**, so the lead can look at the seated boat in the editor without
entering Play. It builds nothing of its own: it calls the same two functions the stage calls.

```csharp
[MenuItem("Kapman/Ship view/Preview the Gislinge boat and her crew (6 oars, 4 men)")]
public static void Preview()
{
    var kit = AssetDatabase.LoadAssetAtPath<ShipStageKit>(ShipStageKitBuilder.KitPath);
    if (kit == null || kit.men == null || kit.men.Length == 0)
    {
        Debug.LogError("Kapman ship view: make the kit first (Kapman > Ship view > Make the kit); it has no men.");
        return;
    }
    // the same figures the stage will use, written out to be read against the model:
    float scale = 7.7f / kit.modelLength;
    var log = new System.Text.StringBuilder();
    for (int i = 0; i < 3; i++)
    {
        float z = 0.17f * 7.7f - i * 0.9f, tholeZ = z - 0.3f;
        ShipRail.At(tholeZ / scale, out float x, out float y);
        log.AppendLine($"  thwart {i + 1}: z {z:0.00}; tholes at x +/-{(x - 0.03f) * scale:0.00}, {(y - kit.modelWaterline) * scale + 0.03f:0.00} over the water, z {tholeZ:0.00}");
    }
    Debug.Log("Kapman ship view: the Gislinge boat at " + scale.ToString("0.000") + "\n" + log);
}
```

(A preview that instantiates the men in an edit-mode scene needs `ShipStage` to be buildable outside Play; whether
ShipStage.cs allows that was not read. The figures above are what the tool can print today.)

Expected printout, for checking the edit: thwarts at z 1.31, 0.41, -0.49; tholes at x 0.94, 1.05, 0.97 (rail less 0.03 m, scaled); 0.44, 0.41, 0.43 m over the water.

## 4. Drafts and text the rulings make wrong

| Where | What | Do |
|---|---|---|
| Editor/ShipViewSeating.cs lines 52-211 | the 18 m prefab, 4 / 8 oars, one man to an oar | remove or replace (section 3) |
| PLAN.md line 498, last cell | "with four oars that is two thwarts and four `SideSeats`" | now: six oars, three thwarts, the three `Seats`; `SideSeats` unused |
| PLAN.md lines 503-514 | "The one thing to settle": ways A and B | A is ruled; strike B |
| PLAN.md lines 560-564 | questions 1 and 2 | answered: six oars, four men; shrunk model, full-size men |
| PLAN.md line 318 | "The lead's brief said four oars: question 1 at the end" | strike |
| PLAN.md line 337 | the note that at her own size `kit.hide` would be shorter | moot; `kit.scaleToClass` and `kit.fitOut` stay on. The off branches (ShipStageKit.cs lines 37-40, StageShip.cs line 118-119) are now dead code: keep them only if another class of ship will be shown at her own size |
| LOOK_AND_MOTION.md lines 415-435, 473-476 | the ship's measures at full size, 0.9 m freeboard, the 4.7 m oar slid out to leave 0.80 m inboard, `ShipViewSeating.Build(rowers, oarLength, shields)` | true of the model, not of the boat shown: freeboard 0.38 m, oar 3.98 m slid IN 0.22 m, 0.84 m inboard. Head the section "the model at full size (not shown)" |
| LOOK_AND_MOTION.md lines 478-481 | grips 0.40 and 0.68 m inboard of the thole on "his own oar" | for a pair: one hand to each oar, 0.73 to 0.84 m inboard (the join's `gripInner`); `gripOuter` is not held |
| RowingRig.cs line 158 `waterline = 1.2f`; PsxSeaGrid defaults 1.2, 8.3, 2.5 | full-size defaults | never read on the stage (the join sets `waterHeight` at line 340 and the grid's fields at 49-54). Change the defaults to 0, 3.54, 1.08 so a rig used without the join is not silently wrong |
| ShipStageState.cs line 81 `beam = 1.5f` | the class table's beam | not wrong by ruling, but the hull shown is 2.16 m (`StageShip.Beam`). Nothing reads `s.beam` for the picture as far as was read; leave |
| StageShip.cs line 119 | `pairs = Clamp(oars / 2, 1, Floor(Length * 0.5 / thwartSpacing))` | gives 3 for six oars at 7.7 m (the cap is 4). Stands |
| StageShip.cs line 145 `HelmSeat` at (0.25, Deck + 0.1, -0.34 Length) | z = -2.62 m | stands: the model's raised after deck starts at -6.6 x 0.431 = -2.84 m, so he sits just ahead of it on the low boards, 2.1 m aft of the last rower. The steering oar is at x = 0.51, z = -2.74: within his reach |

## 5. Not verified (no time; three minutes, nothing run)

- RowingRig.cs's handling of `oar2` (which hand goes to which oar): E2.
- Whether `ShipStage` has a public way to raise `Changed`: E4.
- ShipStageKitBuilder.cs's helper names (`P`, `Palettes`) and exact lines beyond those grepped: E5.
- ShipStageTests.cs: any test that expects four oars, two thwarts or `SideSeats` to be filled must expect 6 / 3 / unused. Not read.
- `check/` was not re-run with a pair of oars and a 0.41 m thole.
- ShipViewLook.cs, PsxSeaGrid.cs, CrewBody.cs, ShipCrewFeed.cs, ShipStage.cs, ShipStageHook.cs, ShipStageView.cs, StageDock.cs were not opened. The dock (the boat's side 0.35 m off the boards, PLAN.md line 269) should be checked against oars that are 2.3 m outboard when she is rowed onto her berth: the port oars must be shipped before the last 3 m, or they pass through the jetty.

## 6. Checked after writing (greps of the drafts)

- **E4:** `ShipStage.Changed` is `public event Action Changed;` (ShipStage.cs line 62), raised only inside the stage (line 455). An event cannot be raised from PackCrewRig, so the public `RaiseChanged()` of E4 is needed (one line: `public void RaiseChanged() => Changed?.Invoke();`), or PackCrewRig calls the look's `Recollect` itself.
- **E5:** `SMV_BasePallete` is already in ShipStageKitBuilder's `Palettes` (line 31): no edit there. The men will not be brown. Only `kit.men` is to be added.
- **E2:** RowingRig takes a pair at line 247 (`o2` from `r.oar2`). Which hand goes to which oar is decided after line 329; see the lines there before trusting the port / starboard order of ShipViewJoin.cs line 322.
- **Tests:** ShipStageTests.cs names neither `SideSeats` nor a seat count; its one oar case starts from `ShipStageState.Fair()` (line 179), which is six oars. Nothing there is made wrong by the rulings.
- **E2, settled:** RowingRig.cs lines 333-337 choose the hands by which shoulder is nearer the first oar's thole ("the left hand to the oar on his left"), not by the order oar / oar2. The arms will not cross; the draft line offered in E2 is not needed. A pair holds `gripInner` only, as section 4 says.
