# Fix: meetings at sea, raiders, guard boats and dated events on the chart

Written 7 October 2026 (sea-meetings task, twelve minutes). Nothing under Assets was changed. Draft code is below.
Paths are under `Assets/Kapman/Code/`. Line numbers are of the files as read today.
Marked NOT CHECKED where I did not read the code.

## 1. The cause

On the chart (scene L_Chart) the map is the game. `MapView.chartMode` is true (`Game/Map/MapView.cs:51`) and the map
opens itself whenever it is shut (`MapView.cs:728`: `if (chartMode && state == State.Shut)`).
So `MapView.Current.IsOpen` (`MapView.cs:57`: `state != State.Shut`) is true for the whole game.
Five places read `IsOpen` as "the player is away from the sea, looking at the map: wait". On the chart they wait for ever.

The rest of the chain is whole in L_Chart: the scene has `VoyagePlanner`, `SeaWatch`, `EncounterRunner`, `TextPanel`,
`KapmanGame` and one `FloatingOrigin` (so `VoyagePlanner.FrameReady`, `Game/Voyage/VoyagePlanner.cs:55`, passes).
The callers run: `KapmanGame.cs:497` calls `GameEvents.Look()`; `VoyagePlanner.cs:124` calls `Patrols.LookAtSea()`;
`VoyagePlanner.cs:138` calls `RaiderEncounter.Look()`. The bridges to the graphs are static and always on
(`Graphs/EncounterNodes.cs:521-558`, `Graphs/EventNodes.cs:89-99`).
NOT CHECKED: that L_Chart has the script graphs that answer "On raiders made out", "On ship made out",
"On king's men" and "On game event" (the scene has script machines; I did not open which graphs).

## 2. Each thing, its gate, what the chart has, the change

### 2.1 A dated or drawn event (events_793.json)

- Gate: `Game/Events/GameEvents.cs:98-102`, in `Look()`:
  `if (MapView.Current != null && MapView.Current.IsOpen) { Alerts.EventWaiting(pending); return; }`
- On the chart: every frame it raises the alert "Something needs you aboard." (`Game/UI/Alerts.cs:162`, not urgent)
  and returns. `Current` is never set, `Due` (line 110) never fires, so `Graphs/EventNodes.cs:98` never triggers.
  The alert is not urgent, so the frame only says it once in the log (`UI/Frame/FrameRoot.Floats.cs:601-606`).
  Side effect: `Alerts.Raise` calls `GameSpeed.Slow()` (`Alerts.cs:54`) for it.
- What the chart has in place of "map closed": the frame shows whatever the graphs put in `TextPanel` as a
  floating window (`FrameRoot.Floats.cs:543-581`). So the event may simply go ahead.
- Change (line 98): `if (MapView.AwayFromSea)` (see 3).
- Second gate, same function, line 103: `if (TextPanel.Current == null || TextPanel.Current.Open) return;`
  In port on the chart the text panel may be "open" on the port's own menu (the runner allows for that:
  `Game/Encounters/EncounterRunner.cs:202-203`). An event with `where == "port"` would still wait while moored.
  Change line 103 to:
  `if (TextPanel.Current == null || (TextPanel.Current.Open && TextPanel.Current.Key != TextPanel.PortMenuKey)) return;`
  NOT CHECKED: whether the port menu counts as `Open` under the frame (`TextPanel.External`, `FrameRoot.cs:23`).

### 2.2 A meeting with a trader

- Gate: `Game/Sim/Encounter.cs:41-42`, in `CanOffer(s, ignoreMap)`:
  `if (!ignoreMap && MapView.Current != null && MapView.Current.IsOpen) return false;`
  `Offer` (line 47-49) calls `CanOffer(s)` so it always fails; the graph node at `Graphs/EncounterNodes.cs:94`
  takes its "not" branch.
- On the chart: `Alerts.MadeOut` (`Alerts.cs:125-136`) raises "Made out ... Go to the ship to hail her." Its answer is
  `retell = s; ToTheShip()`. `Alerts.Tick` (`Alerts.cs:105-121`) only re-tells `if (!MapOpen)`, which never comes.
  The frame drops the words for kind "ship" (`FrameRoot.Floats.cs:604`). Each one still calls `GameSpeed.Slow()`.
- Change (line 41): `if (!ignoreMap && MapView.AwayFromSea)`.
- Change `Alerts.cs:40`: `static bool MapOpen => MapView.AwayFromSea;`
  That one line stops the "meet." alerts on the chart (line 127 needs `MapOpen`), stops the slowing, and lets
  `Tick` clear anything left.
- The pace (the first builder's worry: on the Rhine a sail is made out every few minutes). Add a gap on the chart,
  and let Hail pass it. Draft, in `Encounter.cs`, in place of `Offer` (lines 47-55):

```csharp
        /// <summary>Game hours between two meetings offered by themselves on the chart (Hail asks at any time).</summary>
        public static double ChartOfferGapHours = 12.0;
        static double lastOffered = double.MinValue;

        public static bool Offer(SimShip s) => Offer(s, false);

        /// <summary>`asked`: the player hailed her (no gap).</summary>
        public static bool Offer(SimShip s, bool asked)
        {
            if (!CanOffer(s))
                return false;
            var game = KapmanGame.Current;
            bool chart = MapView.Current != null && MapView.Current.chartMode;
            if (!asked && chart && game != null && game.Hours - lastOffered < ChartOfferGapHours)
                return false;
            if (game != null)
                lastOffered = game.Hours;
            Ship = s;
            Stage = EncounterStage.Offered;
            Changed?.Invoke();
            return true;
        }
```

  `lastOffered` must be reset where the other statics are reset on a new game. NOT CHECKED: where that is.
  Hail (`UI/Frame/FrameRoot.Rack.cs:348` on, it reads `SeaWatch.Current`) should call `Encounter.Offer(s, true)` on the
  nearest made-out trader. NOT CHECKED: what Hail does today.
  A trader meeting is an ORDINARY card: it waits; if she sails out of sight, `Encounter.Part()` (line 81).

### 2.3 A raider's chase

- Gate one: `Game/Sim/RaiderEncounter.cs:48-56`, in `Look()`:
  `if (MapView.Current != null && MapView.Current.IsOpen) { ... Alerts.RaidersComing(...); return; }`
  So line 58 `Coming?.Invoke(s)` is never reached; `Graphs/EncounterNodes.cs:556` never triggers "On raiders made out";
  `Threaten` (line 82) is never called; `Stage` stays `None`; `VoyagePlanner.cs:139-150` never runs the chase.
- On the chart today: an urgent alert floats ("Raiders made out ... They're coming for you.", one option "Go on":
  `FrameRoot.Floats.cs:617-630`). "Go on" calls `Alerts.ToTheShip` (`Alerts.cs:95`), which asks the map to close.
  It stays open. Half a second later `Look` raises the same alert; the frame answers it silently for a day
  (`FrameRoot.Floats.cs:609-612`, `632-633`). Nothing comes of it.
- Gate two: `RaiderEncounter.cs:95-96`, in `Threaten()`: `if (IsOpen) MapView.Current.Close();`
  On the chart this must not run (NOT CHECKED what `Close` does in chartMode; `MapView.cs:425` branches on it).
- Changes: line 48 `if (MapView.AwayFromSea)`; line 95 `if (MapView.AwayFromSea)`.
- How the raider reaches the player as a PRESSING card: see 4.

### 2.4 A guard boat (the king's men at sea)

- Gate: `Game/Sim/Patrols.cs:197`, in `LookAtSea()`:
  `bool mapOpen = MapView.Current != null && MapView.Current.IsOpen;`
  used at line 198 (`signalWaiting && Stage == Signal && !mapOpen` -> `Due("sea")`) and line 227
  (`if (mapOpen) { signalWaiting = true; Alerts.GuardBoat(); return; }`).
- On the chart: `Stage` becomes `Signal`, the alert "A guard boat signals you to heave to." floats, "Go on" does
  nothing, `Due("sea")` (line 201 or 233) never fires. WORSE than the others: `Stage` is now `Signal` for good, so
  `VoyagePlanner.cs:125-133` anchors her and holds her every frame (`ship.anchored = true; Hold(); return;`), and
  `GameEvents.Look` (line 105), `Encounter` and `RaiderEncounter` all stand down: the ship is stuck.
  This can only happen from the year 800 (`Patrols.cs:205`, `guardBoatsFrom`; NOT CHECKED: the number in the data)
  or with `testGuardBoatsNow`. The first release ends in 805, so it is in reach.
- Change (line 197): `bool mapOpen = MapView.AwayFromSea;`
  Then line 233 fires `Due("sea")`, `Graphs/EncounterNodes.cs:530` triggers "On king's men", the graph's talk goes to
  `TextPanel`, and the frame floats it.
- The search on casting off (`Patrols.cs:186-189`, `Due("port")`) has no map gate. NOT CHECKED on the chart.
- A guard boat is PRESSING in the same sense (she is held: `VoyagePlanner.cs:129-130`). Its own option when the time
  runs out should be "heave_to" (`Patrols.Answer("heave_to")`, line 253). Suggested time: 2 game hours.

## 3. The one switch (draft)

Add to `Game/Map/MapView.cs`, beside `IsOpen` (line 57):

```csharp
        /// <summary>Is the player away from the sea, at the map table, so that what happens at sea must wait for her?
        /// In the 3D levels: while the map is open. On the chart never: the chart is the sea, and cards float over it.</summary>
        public static bool AwayFromSea => Current != null && Current.IsOpen && !Current.chartMode;
```

Then the exact line changes:

| File:line | Now | Becomes |
|---|---|---|
| `Game/Events/GameEvents.cs:98` | `if (MapView.Current != null && MapView.Current.IsOpen)` | `if (MapView.AwayFromSea)` |
| `Game/Events/GameEvents.cs:103` | `if (TextPanel.Current == null \|\| TextPanel.Current.Open)` | `if (TextPanel.Current == null \|\| (TextPanel.Current.Open && TextPanel.Current.Key != TextPanel.PortMenuKey))` |
| `Game/Sim/Encounter.cs:41` | `if (!ignoreMap && MapView.Current != null && MapView.Current.IsOpen)` | `if (!ignoreMap && MapView.AwayFromSea)` |
| `Game/Sim/RaiderEncounter.cs:48` | `if (MapView.Current != null && MapView.Current.IsOpen)` | `if (MapView.AwayFromSea)` |
| `Game/Sim/RaiderEncounter.cs:95` | `if (MapView.Current != null && MapView.Current.IsOpen)` | `if (MapView.AwayFromSea)` |
| `Game/Sim/Patrols.cs:197` | `bool mapOpen = MapView.Current != null && MapView.Current.IsOpen;` | `bool mapOpen = MapView.AwayFromSea;` |
| `Game/UI/Alerts.cs:40` | `static bool MapOpen => MapView.Current != null && MapView.Current.IsOpen;` | `static bool MapOpen => MapView.AwayFromSea;` |

The 3D levels behave as before (there `chartMode` is false).

A warning on the last row. After it, `Alerts.Tick` (`Alerts.cs:107-120`) clears every alert each frame on the chart.
That is right for raiders, the guard boat, events and ships, which now happen directly.
It is WRONG for `Alerts.Aground` (`Alerts.cs:158`) if the chart needs that alert. NOT CHECKED: who raises it there.
The safe form if in doubt: leave line 40 alone and change only line 127 (`!MapOpen` -> `!MapView.AwayFromSea`).
Then no "meet." alert is raised on the chart, and the aground alert stays as it is.

Read and left alone (they use `IsOpen` but are not meetings): `Game/Sim/ShotDirector.cs:103`,
`Game/UI/MainMenu.cs:326`, `Game/UI/Notices.cs:84`, `Game/SaveGame.cs:161`.

## 4. The raider as a PRESSING card (rulings 10 and 12 of 7 October)

What is already there, once the switch is in:

1. `SeaWatch.Update` makes the band out (`Game/Sim/SeaWatch.cs:74-79`).
2. `RaiderEncounter.Look` (line 46) finds it, `CanThreaten` holds, `Coming` fires (line 58).
3. The graph ("On raiders made out") calls `Threaten` (`Graphs/EncounterNodes.cs:332`): `Stage = Threat`, the band
   chases (`Band.chasing = true`, line 87).
4. The graph puts the choices in `TextPanel`; the frame shows a talk as a floating window (`FrameRoot.Floats.cs:543-581`).
5. `VoyagePlanner.cs:139-150` runs `RaiderEncounter.Tick(hours, w)`: with the clock held, `hours` is 0
   (`LevelClock.Paused`, line 143) and nothing moves; if the player lets the clock run (ruling 6), they close on her.

So the raider's "time of its own" is already in the sim: it is the chase. What ruling 10 asks and is missing:

- (a) the window is marked pressing: it must be answered, and it holds the clock when it opens;
- (b) she is HELD where she is while the card is unanswered (stage `Threat`);
- (c) when its time passes with the clock running, the card takes an option by itself and the log says so.

### (c) the time and the option: `RaiderEncounter.cs`

New members:

```csharp
        /// <summary>Game hours an unanswered threat waits before the card answers itself (ruling 10, 7 Oct 2026).</summary>
        public static double PressHours = 2.0;
        /// <summary>True from the card answering itself until the next threat.</summary>
        public static bool TookItself { get; private set; }
        /// <summary>Is a pressing card up and unanswered (she is held where she is)?</summary>
        public static bool Pressing => Stage == RaidStage.Threat;
        /// <summary>Game hours left before it answers itself.</summary>
        public static double PressLeft => Math.Max(0.0, PressHours - ChaseHours);
        /// <summary>The stage changed without the player (the graph and the frame look again).</summary>
        public static event Action Changed;
```

In `Threaten`, after line 91 (`ParleyTried = false;`): `TookItself = false;`

In `Tick`, in place of lines 138-139 (`if (Stage != RaidStage.Running) return;`):

```csharp
            if (Stage == RaidStage.Threat && ChaseHours >= PressHours)
            {
                TookItself = true;                                           // (no order given in time: she runs)
                Stage = RaidStage.Running;
                Voyage.Say("No order was given. She ran from the raiders by herself.");
                Changed?.Invoke();
            }
            if (Stage != RaidStage.Running)
                return;
```

Why "run": it is the one answer that takes nothing from the player by itself (fight and submit cost men or cargo).
If it fails the stage goes to `Caught` (lines 133-137) and the card comes back with the alongside choices.
The user has not named the option; this is a default to confirm. `PressHours` belongs in the raider rules
(`RaiderRules`, beside `giveUpHours` and `sprintHours`) once the cards' grammar has an urgency.
`Caught` is pressing with no time of its own: they are alongside, she lies to, and the card waits.
`Changed` must be set to null where the class's statics are reset, as `Alerts.Init` does (`Alerts.cs:165-170`).

### (b) she is held: `VoyagePlanner.cs`

In the raid block, after line 145 (`var band = ...`). NOT CHECKED: lines 150-170 may already do this for `Threat`;
read them first.

```csharp
                if (raid == Kapman.Game.Sim.RaidStage.Threat)
                {
                    Hold();                                                  // (a pressing card: she is held where she is)
                    meeting = true;
                    return;
                }
```

### (a) the window: the frame

`FloatSpec` (`UI/Frame/FrameRoot.Floats.cs:20-45`) has no urgency. Add:

```csharp
        /// <summary>A PRESSING card (ruling 10): it holds the ship; left unanswered with the clock running it takes
        /// `pressTakes` by itself when `pressLeft` reaches 0. Null `pressLeft`: an ordinary card.</summary>
        public Func<double> pressLeft;
        public string pressTakes = "";
```

Where the talk float is built (`FrameRoot.Floats.cs:543-581`), before `OpenFloat(spec)` at line 581:

```csharp
            if (Sim.RaiderEncounter.Stage != Sim.RaidStage.None)
            {
                spec.mustAnswer = true;                                      // (no cross, no Esc)
                spec.holdsClock = true;                                      // (held when it opens; Space lets it run: ruling 6)
                spec.small = Sim.RaiderEncounter.Status();
                if (Sim.RaiderEncounter.Pressing)
                {
                    spec.pressLeft = () => Sim.RaiderEncounter.PressLeft;
                    spec.pressTakes = "run from them";
                }
            }
            else if (Sim.Patrols.Stage == Sim.PatrolStage.Signal)
            {
                spec.mustAnswer = true;
                spec.holdsClock = true;
            }
```

The line under a waiting float (`FrameRoot.Floats.cs:279`) becomes, for a spec with `pressLeft` whose hold was let go
(`f.released`):

```csharp
                if (f.spec.pressLeft != null && f.released)
                    return "It waits for your answer. The clock runs: in " + f.spec.pressLeft().ToString("0.0")
                           + " hours she will " + f.spec.pressTakes + ".";
```

NOT CHECKED: whether the raider graph reads `Stage` each frame or waits on its own choice node. If it waits, bridge
`RaiderEncounter.Changed` to a graph event, as `Coming` is bridged (`Graphs/EncounterNodes.cs:546-556`), so the talk
moves on to the running stage when the card has answered itself.

With the switch in, `Alerts.RaidersComing` (`Alerts.cs:145`) and `Alerts.GuardBoat` (`Alerts.cs:154`) are never raised
on the chart, so the "Go on" float (`FrameRoot.Floats.cs:617`) no longer comes up for them. One card, not two.

## 5. Order of work and how to test

1. `MapView.AwayFromSea` and the seven line changes of section 3. Compile; run the tests.
2. On the chart: `GameEvents.Force("<an id from events_793.json>")` (`GameEvents.cs:84`) must float a card at once.
   Set `testGuardBoatsNow` in the patrol rules and sail out of Dorestad with goods under the law.
3. The trader gap and Hail (2.2).
4. The pressing raider (4): put a band near her, check the float cannot be shut, the clock holds, Space lets it run,
   and after 2 game hours the log has the line and she runs.

## 6. Not done

- No code was compiled or run; the drafts are untested.
- Not read: the graphs themselves, `TraderBrain`, `RaiderBrain`, `WorldShips3D`, `EncounterDirector`,
  `VoyagePlanner.cs` after line 150, the Hail button in `FrameRoot.Rack.cs`, `MapView.Close` in chart mode,
  who raises the aground alert.
- The Rhine pace (12 game hours between meetings offered by themselves) is my number; the user has not ruled it.
- The option a pressing raider card takes by itself ("run") and its time (2 game hours) are my defaults, not rulings.
