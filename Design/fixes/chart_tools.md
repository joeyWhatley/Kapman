# Chart tools on the right, Oars in the rack, the swap button (rulings 7 and 8)

Read from the code on 7 Oct; nothing was run or compiled. Paths are under `Assets/Kapman/Code/Game/` unless said.
The stylesheet is `Assets/Kapman/Game/UI/Frame.uss` (not in the Frame code folder); the chart's is `Assets/Kapman/Game/UI/MapView.uss`.

## 0. The short of it

1. The chart has almost no public calls for these tools. Only `Follow()` (find her), `ShowBoth()`, `PutToolDown()` exist.
   Zoom, names, sails, pins need five small public members added to `MapView` (drafted in 2).
2. **Winds: there is no wind layer on the chart at all** (no wind drawing found in MapView.cs). That button has nothing to call yet.
3. **Oars is not a choice in the game today.** `Sim/Travel.cs` line 180-181 takes the faster of sail and oars by itself.
   A flag alone would only ever make her slower. A rule is needed first (question in 4).
4. `ShipStageHook` (the swap) is not in `Assets` yet (no file names it). The plan has the hook make its own Swap button; the
   frame should not make a second one blind (5).

## 1. What is there now

| Thing | Where |
|---|---|
| The eight buttons listed | `UI/Frame/FrameRoot.Rack.cs` lines 124-128, in `BuildRack(VisualElement column)` (119-144) |
| What a button is called, greyed, on | `Look(...)` 168-235; Measure's case 208-212 |
| What a press does | `Do(...)` 238-300; Measure's case 279-282: `SetMeasuring(!Measuring)` and a log line |
| The icon kinds | `UI/Frame/FrameIcon.cs` line 12: `enum Kind { CastOff, Wait, Measure, LookOut, Sound, Hail, Rest, Offer }`; drawn at 56-101 (Measure 65) |
| The measure | `UI/Frame/FrameRoot.Measure.cs`: `Measuring` 32, `SetMeasuring(bool)` 39, `MeasureBuild` 108-122, keys 161-172 ([D], [Esc]) |
| A second Measure button already exists | `MeasureBuild` line 111, `ruleButton` in the `frame-rule` slip (Frame.uss 805: left 31%, top 68px) |
| The right side | `UI/Frame/FrameRoot.cs` `Build()`: portrait `who` 196-210, ship window `shipBox` 213-222 |
| Portrait style | Frame.uss 668 `.frame-who`: absolute, right 1%, top 68px, width 24%; face 120 x 150 (677) |
| Ship window style | Frame.uss 721 `.frame-ship`: absolute, right 1%, bottom 1.6%, width 24%; view 262px high (729) |
| Chart: find her | `Map/MapView.cs` 81 `Follow()` (also sets the zoom back to `chartScale` = 400) |
| Chart: zoom | only the wheel, `OnWheel` 1399-1422: factor 1.18 a notch, between `ClosestScale` (1037) and `maxScale` (45) |
| Chart: tools | `Map/MapView.Tools.cs` 19 `enum Tool { None, Dividers, Pin, String }`, `SetTool` 125 (private), `PutToolDown()` 158 (public) |
| Chart: names, sails, pins | classes `map-port-name` (1098), `map-sim-ship` (1180), `map-note-pin` (1303); no switch for any |

## 2. Each tool and its call

`var map = MapView.Current;` (MapView.cs 55; may be null, guard every call).

| Tool | Call | State |
|---|---|---|
| Measure | `SetMeasuring(!Measuring)` (the frame's own; FrameRoot.Measure.cs 39). On: `Measuring`. Key [D] stays | exists |
| Nearer | `map.ZoomBy(1.0 / 1.4)` | **new** |
| Farther | `map.ZoomBy(1.4)` | **new** |
| Find her | `map.Follow()` (key [F] stays) | exists |
| Names | `map.ShowNames = !map.ShowNames` | **new** |
| Winds | nothing to call: no wind layer on the chart | **missing; greyed "not yet"** |
| Sails | `map.ShowSails = !map.ShowSails` (the other ships in sight) | **new** |
| Pins | `map.TakePin()` (the pin tool, key [P]) or `map.ShowPins` toggle: see question 2 | **new** |

### Draft: add to `Map/MapView.cs` (after `ShowBoth`, about line 104)

```csharp
        // ---- for the frame's chart tools (ruling 7 of 7 Oct: they sit on the right, under the portrait)

        /// <summary>Zooms about the middle of the view: under 1 nearer, over 1 farther. Kept inside the chart's limits.</summary>
        public void ZoomBy(double factor)
        {
            if (state != State.Open || town != null)
                return;
            mpp = targetMpp = Math.Max(ClosestScale, Math.Min(maxScale, mpp * factor));
        }

        /// <summary>Can she be brought nearer, or farther (for greying the buttons)?</summary>
        public bool CanZoomIn => mpp > ClosestScale * 1.01;
        public bool CanZoomOut => mpp < maxScale * 0.99;

        bool showNames = true, showSails = true, showPins = true;
        /// <summary>The ports' names on the chart.</summary>
        public bool ShowNames { get => showNames; set { showNames = value; root?.EnableInClassList("no-names", !value); } }
        /// <summary>The other ships in sight.</summary>
        public bool ShowSails { get => showSails; set { showSails = value; root?.EnableInClassList("no-sails", !value); } }
        /// <summary>The notes pinned to the chart.</summary>
        public bool ShowPins { get => showPins; set { showPins = value; root?.EnableInClassList("no-pins", !value); } }
```

Add to `Map/MapView.Tools.cs` (beside `PutToolDown`, line 158):

```csharp
        /// <summary>Takes the pin up, or puts it down (the frame's Pins button; [P]).</summary>
        public void TakePin() => SetTool(tool == Tool.Pin ? Tool.None : Tool.Pin);
        public bool PinInHand => tool == Tool.Pin;
```

Add to `Assets/Kapman/Game/UI/MapView.uss` (visibility, not display: `NotePins` sets `display` inline each frame, MapView.cs 1314, and inline wins):

```css
.no-names .map-port-name { visibility: hidden; }
.no-sails .map-sim-ship { visibility: hidden; }
.no-pins .map-note-pin { visibility: hidden; }
```

Not checked: whether `root` is built before the frame first reads these (hence `root?.`); whether a hidden ship or pin still takes
clicks (hidden elements do not pick in UI Toolkit, so it should not); `OnEnable` should re-apply the three classes if `root` is rebuilt.
`Follow()` puts the zoom back to 400 each time: after Nearer, Find her undoes it. If that is unwanted, drop line 86 `targetMpp = chartScale;` behind a flag.

## 3. Where the block goes

Between the portrait and the ship window, in one right-hand column so the block always sits under the portrait however tall its text is
(`.frame-who` has no fixed height; its text can wrap past the face).

### `UI/Frame/FrameRoot.cs`, `Build()`

Line 196 `who = Slip("frame-who");` to line 210 `root.Add(who);` stay as they are, but line 210 changes:

```csharp
            // ---- the right side: the portrait, and under it everything about the chart (ruling 7 of 7 Oct)
            right = new VisualElement { pickingMode = PickingMode.Ignore };
            right.AddToClassList("frame-right");
            right.Add(who);                                               // (was: root.Add(who);)
            BuildChartTools(right);                                       // (FrameRoot.Chart.cs)
            root.Add(right);
```

with `VisualElement right;` beside `shipBox`, and `partial void BuildChartTools(VisualElement column);` and
`partial void ChartToolsUpdate();` beside the other partials (line 137-145), the second called next to `RackUpdate()`.

### New file `UI/Frame/FrameRoot.Chart.cs`

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>EVERYTHING ABOUT THE CHART, on the right under the portrait (the user's ruling 7 of 7 Oct 2026): the measure,
    /// nearer, farther, find her, names, winds, sails, pins. They change what the chart shows; none changes the game.</summary>
    public partial class FrameRoot
    {
        sealed class ChartTool
        {
            public Button button;
            public Func<MapView, bool> on, can;
            public string hint, cannot;
        }

        readonly List<ChartTool> chartTools = new List<ChartTool>();
        float nextChartTools;

        partial void BuildChartTools(VisualElement column)
        {
            chartTools.Clear();
            var box = Slip("frame-chart");
            Tool(box, "Measure", "The way between two places on the chart, in days: click two places.  [D]", null,
                 m => SetMeasuring(!Measuring), m => Measuring, null);
            Tool(box, "Nearer", "Bring the chart nearer.", "She is as near as the chart goes.", m => m.ZoomBy(1.0 / 1.4), null, m => m.CanZoomIn);
            Tool(box, "Farther", "Stand farther off.", "That is the whole chart.", m => m.ZoomBy(1.4), null, m => m.CanZoomOut);
            Tool(box, "Find her", "Back to your ship, and stay on her.  [F]", null, m => m.Follow(), null, null);
            Tool(box, "Names", "The names of the ports.", null, m => m.ShowNames = !m.ShowNames, m => m.ShowNames, null);
            Tool(box, "Winds", "The winds on the chart.", "The winds are not drawn on the chart yet.", m => { }, null, m => false);
            Tool(box, "Sails", "The sails in sight.", null, m => m.ShowSails = !m.ShowSails, m => m.ShowSails, null);
            Tool(box, "Pins", "Pin a note to the chart: click where.  [P]", null, m => m.TakePin(), m => m.PinInHand, null);
            column.Add(box);
            nextChartTools = 0f;
        }

        void Tool(VisualElement box, string name, string hint, string cannot, Action<MapView> press, Func<MapView, bool> on, Func<MapView, bool> can)
        {
            var t = new ChartTool { on = on, can = can, hint = hint, cannot = cannot };
            t.button = new Button(() =>
            {
                var map = MapView.Current;
                if (map == null)
                    return;
                if (t.can != null && !t.can(map))
                {
                    FrameLog.Add(name + ": " + char.ToLowerInvariant(t.cannot[0]) + t.cannot.Substring(1));
                    return;
                }
                press(map);
                nextChartTools = 0f;
            }) { text = name };
            t.button.AddToClassList("frame-chart-tool");
            box.Add(t.button);
            chartTools.Add(t);
        }

        partial void ChartToolsUpdate()
        {
            if (Time.realtimeSinceStartup < nextChartTools)
                return;
            nextChartTools = Time.realtimeSinceStartup + 0.25f;
            var map = MapView.Current;
            foreach (var t in chartTools)
            {
                bool can = map != null && (t.can == null || t.can(map));
                t.button.EnableInClassList("off", !can);
                t.button.EnableInClassList("on", map != null && t.on != null && t.on(map));
                t.button.tooltip = can || t.cannot == null ? t.hint : t.cannot;
            }
        }
    }
}
```

(`Slip`, `FrameLog.Add`, the greyed-not-disabled manner and the 0.25 s look are the rack's own: Rack.cs 146-164, 242-247.)
The Measure icon (`new FrameIcon(FrameIcon.Kind.Measure)`, FrameIcon.cs 65) can be put on the Measure tool; the other seven have no icons, so
the draft uses words only.

### `Assets/Kapman/Game/UI/Frame.uss`

`.frame-who` (668-675) loses its own place; the column takes it:

```css
/* ---------------------------------------------------------------- the right side: the portrait, and under it
   everything about the chart (the user's ruling 7 of 7 Oct) */

.frame-root .frame-right {
    position: absolute;
    right: 1%;
    top: 68px;
    width: 24%;
    flex-direction: column;
}

.frame-root .frame-who {          /* was: position absolute; right 1%; top 68px; width 24% */
    flex-direction: row;
    padding: 8px;
    flex-shrink: 0;
}

.frame-root .frame-chart {
    flex-direction: row;
    flex-wrap: wrap;
    flex-shrink: 0;
    margin-top: 6px;
    padding: 6px 4px 2px 4px;
}

.frame-root .frame-chart-tool {
    width: 23.6%;
    margin: 0 0.7% 4px 0.7%;
    padding: 5px 2px;
    -unity-text-align: middle-center;
    font-size: 14px;
    border-width: 2px;
    border-top-color: rgb(140, 106, 142);
    border-left-color: rgb(140, 106, 142);
    border-right-color: rgb(26, 15, 29);
    border-bottom-color: rgb(26, 15, 29);
    border-radius: 9px;
    background-color: rgba(52, 36, 57, 0.97);
}

.frame-root .frame-chart-tool.on {
    border-top-color: rgb(26, 15, 29);
    border-left-color: rgb(26, 15, 29);
    border-right-color: rgb(140, 106, 142);
    border-bottom-color: rgb(140, 106, 142);
    background-color: var(--frame-edge);
}

.frame-root .frame-chart-tool.off {
    background-color: rgb(40, 28, 46);
    color: rgb(140, 128, 144);
}
```

(Copied from `.frame-verb` 147-186: four across, two rows.) Room: the portrait ends near 250px, the ship window starts about
320px from the bottom; two rows of tools are about 80px, so it fits at 1080 and at 720 high. Not looked at on screen.

Things that lean on `.frame-who` being absolute and must be looked at: anything in Frame.uss with `.wide .frame-who` or a
float placed against it (not searched), and `MapView.ShowBoth` 97 which takes the chart's free middle as 36% wide (unchanged by this).

### The old Measure button over the chart

`FrameRoot.Measure.cs` line 111-112 makes `ruleButton` in the `frame-rule` slip at the chart's top left. With Measure in the
right block that button is a double. Keep the slip (it holds the figure and the words, and only shows while measuring, line 103);
drop lines 111-112 and line 86 `ruleButton.EnableInClassList("on", Measuring);` and the field. Nothing is lost: the block's button and [D] both call `SetMeasuring`.

## 4. Oars takes Measure's place in the rack

### The edits that are certain

`UI/Frame/FrameIcon.cs` line 12: add the kind at the end (Measure stays, the chart block may draw it):

```csharp
        public enum Kind { CastOff, Wait, Measure, LookOut, Sound, Hail, Rest, Offer, Oars }
```

and a `case Kind.Oars:` in the drawing switch (after line 101): two crossed oars. The painter code was not read; whoever
does this copies the manner of `case Kind.Measure:` at 65.

`UI/Frame/FrameRoot.Rack.cs` line 126:

```csharp
                (FrameIcon.Kind.CastOff, "Cast off"), (FrameIcon.Kind.Wait, "Wait"), (FrameIcon.Kind.Oars, "Oars"), (FrameIcon.Kind.LookOut, "Look out"),
```

`Look`, lines 208-212, the Measure case becomes:

```csharp
                case FrameIcon.Kind.Oars:
                {
                    var cls = FrameGame.ShipClass;                         // (NOT CHECKED: use whatever ShipPanel.cs line 45 reads `cls` from)
                    on = Oars.Out;
                    name = on ? "Ship oars" : "Oars";
                    if (cls == null || cls.oarsKn <= 0f) { can = false; why = "She has no oars."; }
                    else if (!cls.hasSail) { can = false; on = true; why = "She has no sail: she is always under oars."; }
                    else why = on ? "Ship the oars and set the sail again." : "Lower the sail and row. Slower in a fair wind, and it tires the crew.";
                    break;
                }
```

`Do`, lines 279-282, the Measure case becomes:

```csharp
                case FrameIcon.Kind.Oars:
                    Oars.Out = !Oars.Out;
                    FrameLog.Add(Oars.Out ? "The sail comes down and the crew take to the oars." : "The oars are shipped and the sail is set.");
                    break;
```

The class comment at 99-103 ("MEASURE (takes the measure up or puts it down)") changes to OARS. Tests that call
`Look(FrameIcon.Kind.Measure, ...)` or `Do(FrameIcon.Kind.Measure)` will fail to mean anything: search `Assets/Kapman/Tests` for `Kind.Measure` (not searched).

### What Oars must set in the game

Found:

- `Sim/Travel.cs` 162-182 `Travel.Speed(ShipClassDef c, courseDeg, windFrom, windSpeed, out sheltering)`: sail speed from the polar,
  then line 180 `double oars = c.oarsKn * Math.Min(1.0, c.rowingHours / 12.0);` and 181 `return Math.Max(sail, oars) * SimUnits.KnotMs;`.
  So she already rows by herself whenever rowing is faster. There is no order "row" anywhere.
- `Chart/ChartBody.cs` line 103 is the one place the player's ship gets her speed: `Travel.Speed(cls, Mathf.Repeat(want, 360f), WindFrom, Wind, out lying)`.
- `Shims/SailingShip.cs` 127 `public bool Rowing => Motion.Speed > 0.05f;` and 141 `SailFill => Rowing ? 1f : 0f;` are stand-ins (the name is wrong: it means "under way").
  `rowingKnots = 3.5f` (44) is read by nothing found.
- `Voyage/VoyagePlanner.cs` 27: "rowing is the same order (no sail to lower yet)"; `rowInGameM = 60f` (34) rows the last of a berth, as a word only.
- `Sim/SimData.cs` 10 and 57: the class's `sailKn, maxKn, oarsKn, rowingHours, hasSail` (json `speed_oars_kn`, default 2).
- `UI/Frame/FramePassage.cs` 13: the passage's days already count "the oars where they beat" the sail.

So Oars needs one flag and one line. Draft, a new small file `Voyage/Oars.cs`:

```csharp
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>THE OARS, by the Oars button (the user's ruling 7 of 7 Oct 2026): out, the sail is down and she goes by the
    /// oars alone, whatever the wind. Shipped, she goes as before (the sail, or the oars where they beat it: Travel.Speed).</summary>
    public static class Oars
    {
        public static bool Out { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Out = false;
    }
}
```

`Sim/Travel.cs`, beside `Speed` (the world's ships keep calling `Speed`, untouched):

```csharp
        /// <summary>Her speed under oars alone (m/s): the sail is down, the wind does nothing for her. A gale still stops her.</summary>
        public static double OarSpeed(ShipClassDef c, double windSpeed, out bool sheltering)
        {
            sheltering = windSpeed > 17.0;
            return sheltering ? 0.0 : c.oarsKn * Math.Min(1.0, c.rowingHours / 12.0) * SimUnits.KnotMs;
        }
```

`Chart/ChartBody.cs` line 103:

```csharp
                    double made = cls == null ? fallbackKnots * 0.5144
                                : Oars.Out ? Travel.OarSpeed(cls, Wind, out lying)
                                : Travel.Speed(cls, Mathf.Repeat(want, 360f), WindFrom, Wind, out lying);
```

And these must follow the flag, or it is lost or lies:

1. **The save.** `Oars.Out` goes in SaveGame with the course (file not read).
2. **The passage's days** (`FramePassage`, and `Course.Plan`'s days if it has its own): under oars the estimate uses `OarSpeed`.
3. **The ship window**: `SailingShip.SailFill` (141) should be 0 with `Oars.Out`, and `PointOfSail` (142) "under oars"; the ship stage in `Design/ship_view/PLAN.md` reads the sail from there.
4. **Mooring**: `Oars.Out = false` when she berths (where `FrameGame.PortHere` turns non-null), or leave it as the player set it.
5. Namespace: `Oars` in `Kapman.Game` clashes with nothing found by the search for "Oars" in Chart, Voyage, Shims and UI/Frame; the assembly that holds `Travel` (Kapman.Game.Sim) must not need it, and with the draft it does not.

**Question 1 for the user (it decides whether the button is worth pressing).** As the rule stands, oars out is never faster than leaving it to her:
`Speed` already takes the better of the two. Three ways to give it a use, pick one:
(a) as drafted, a plain choice that is only for the feel and for foul winds where it is equal;
(b) oars out rows at the full `oarsKn` (not the day's mean `rowingHours / 12`), and the crew tire or eat more: a short hard pull;
(c) the other way about: with the oars shipped she sails only (`Speed` stops taking the oars), so a calm leaves her still until the player orders the oars out.
(c) makes the button matter most and matches "no sail to lower yet" in VoyagePlanner, but it changes how she goes today in every calm.

## 5. The swap button on the ship window's top left corner

From `Design/ship_view/PLAN.md` 188-202 and 358-368:

- The swap is `ShipView.ShipStageHook.Current.Swapped ^= true` (line 368). Nothing changes camera: the stage's texture goes big behind the frame,
  the map's own UI document is made small into the ship window's rectangle, and `MapView.Follow()` is called at each swap (line 201).
- The plan has **the hook make the button itself**, inline-dressed, top left of `.frame-ship-view`, and a second one on the map's document for the
  way back (lines 197, 358-360), so the frame's files need no edit.
- `ShipStageHook` is not in `Assets` yet (no .cs file names it). Until it is, a frame button would call nothing.

So that the frame owns the look and there is one button, not two, the frame offers the place and the hook fills in the act.
`UI/Frame/FrameRoot.cs`, after line 215 `shipBox.Add(shipView);`:

```csharp
            // the swap (ruling 8 of 7 Oct): the ship window and the chart change places. The ship stage sets what it does
            // (Design/ship_view/PLAN.md, "The swap"); with no stage there is nothing to swap and the button is not shown.
            swapButton = new Button(() => SwapShipAndChart?.Invoke()) { text = "Swap", tooltip = "The ship and the chart change places." };
            swapButton.AddToClassList("frame-ship-swap");
            shipBox.Add(swapButton);
```

with, in the class:

```csharp
        Button swapButton;
        /// <summary>Set by the ship stage (ShipStageHook): changes the ship window's place with the chart's. Null: no stage.</summary>
        public static System.Action SwapShipAndChart;
```

and in the frame's update (beside `RackUpdate()`): `swapButton.style.display = SwapShipAndChart != null ? DisplayStyle.Flex : DisplayStyle.None;`
and `SwapShipAndChart = null;` in a `SubsystemRegistration` reset. The hook then does, where the plan had it build its own first button:

```csharp
            FrameRoot.SwapShipAndChart = () => Swapped ^= true;
```

Frame.uss, after `.frame-ship-view` (740):

```css
.frame-root .frame-ship-swap {
    position: absolute;
    left: 12px;
    top: 12px;
    padding: 2px 9px;
    font-size: 14px;
    border-width: 2px;
    border-top-color: rgb(140, 106, 142);
    border-left-color: rgb(140, 106, 142);
    border-right-color: rgb(26, 15, 29);
    border-bottom-color: rgb(26, 15, 29);
    border-radius: 9px;
    background-color: rgba(52, 36, 57, 0.97);
}
```

(12px = the slip's 6px padding + the view's 3px border + 3px in.) It is added after `shipView`, so it draws over the picture; the hook's `Image` is laid inside
`.frame-ship-view`, so it stays under the button. Because the button belongs to `shipBox`, it stays in the same corner after the swap, when the small chart is
in the window: one button does both ways, and the plan's second button on the map's document is not needed, provided the frame's document stays above the map's
(the plan's risk R6, line 444, says it does: "Keep the map under the frame and make a hole").
While swapped the chart tools of 2 still act on `MapView.Current`, now the small chart: nothing to change.

## 6. Questions for the user

1. Oars: (a), (b) or (c) in 4?
2. Pins: does the button take up the pin (to place a note), or show and hide the pins already placed? Drafted as taking it up; `ShowPins` is there for the other.
3. Winds: there is nothing to show yet. Greyed with "not drawn on the chart yet" until a wind layer is made, or leave the button out till then?
4. Find her also puts the zoom back to the chart's own (400 m a pixel). Keep, or only re-centre?

## 7. Not done, not checked

- Nothing compiled or run. The drafts were not tried against the files.
- `FrameGame.ShipClass` in the Oars case is a guess at a name; read `UI/Frame/Panels/ShipPanel.cs` near line 45 for where `cls` comes from.
- The icon painter (FrameIcon.cs 56-101) was not read: the Oars icon is undrawn.
- SaveGame, FramePassage's day sums, the tests folder and FrameRoot.Self.cs / Floats.cs were not read for uses of the measure button or of `.frame-who`'s place.
- Whether `mpp` eases toward `targetMpp` in chart mode was not read; `ZoomBy` sets both, as the wheel does (MapView.cs 1421).
