# The clock: a pause and five speeds (ruling 26)

Written 7 October 2026, from the code as it stood between 12:20 and 12:50 (six builders were changing it meanwhile:
every edit below is given as text to find, with the line it stood on then). Paths are under `Assets/Kapman/Code/`
unless said. Nothing in the project was changed.

**What was checked.** All the code in section 5 was compiled outside Unity with Unity's own compiler against the
project's references, as whole assemblies with the edits swapped in: Kapman.Core, Kapman.Runtime, Kapman.Game,
Kapman.Graphs and Kapman.Tests.EditMode all build, no errors. The clock's step was also RUN outside Unity (it is
pure C#): the same seed at speed 1 and at speed 5 ended in the same world (section 6.1). **What was not:** nothing
was run in Unity, so no frame of the real game has been stepped this way; the test on the real world of ships and
markets is compiled but not run; the play-mode test is an outline. Section 8 lists the rest.

## 0. In short

1. **Speed can no longer be Unity's time scale.** Today `GameSpeed` sets `Time.timeScale` to 1, 2 or 4 and
   everything follows it. Speed 5 is 360 times speed 1; Unity's time scale stops at 100 (its documentation, as I
   recall it; not tried here). And ruling 26 wants the view of the ship calm at every speed, which a time scale of
   96 or 360 cannot give.
2. **So the game steps its own clock, one game minute at a time.** A frame at speed 1 runs no step or one; a frame
   at speed 5 runs six or more; each step is the same step, in the same order: her orders, she sails the minute, the
   minute's ticks (on the hour the world's ships and the cards' draw, on the day the markets, the bread, the pay),
   the lookout, the mooring watch, the drops. The time scale stays 1 (0 when stopped). That is what makes "the same
   seed at 1 and at 5 ends in the same world" true, and it is the only way I found that does.
3. **What breaks if the speeds are simply added to today's clock** (section 3): at speed 5 one frame is 6 game
   minutes (12 at thirty frames a second); her boat moves 12 chart metres a frame past a mark she must come within
   5 of; a card that should hold the clock holds it a frame late, and the hourly draws in between are thrown away;
   several things look by the wall clock, and half a second of it is three game hours.
4. **The clock drops back by itself** in the step, in game time, so it drops on the same game minute at any speed.
   Eleven reasons, each with a switch (section 4.3). `+` goes straight back to the speed she had.
5. **Sized for the slow clock and to be sized again** (section 7): cards at sea come as often as every 4 game hours
   (at speed 4 that is one every 2.5 seconds: make it 36 hours); a talk runs on the clock (at speed 4 a half-minute
   talk costs two days: a talk drops the clock to 1); the waits' own fast paces (30 and 60 times) are now slower
   than speeds 4 and 5 and go; bread and pay need no new numbers but need a warning and a standing order.
6. **The sum of 230 hours** holds only if winters are a board and not sailed through (section 1.2). Say which.

## 1. The five speeds, as numbers

### 1.1 The rates

`GameSpeed.Rates`, game seconds a real second. Speed 1 is today's 1x; speed 2 is today's 4x.

| Speed | Game seconds a real second | Times speed 1 | A game hour takes | A game day takes | Game minutes a frame at 60 fps | at 30 fps | World hours a real second |
|---|---|---|---|---|---|---|---|
| 1 | 60 | 1 | 60 s | 24 min | 1/60 | 1/30 | 1/60 |
| 2 | 240 | 4 | 15 s | 6 min | 1/15 | 2/15 | 1/15 |
| 3 | 1,440 | 24 | 2.5 s | 60 s | 0.4 | 0.8 | 0.4 |
| 4 | 5,760 | 96 | 0.625 s | 15 s | 1.6 | 3.2 | 1.6 |
| 5 | 21,600 | 360 | 0.167 s | 4 s | 6 | 12 | 6 |

### 1.2 The page of real hours (a critic asked for it: review_newcomer.md, "one page that sums the real hours of a season, a year and a life")

Running clock only; what the player stops to do comes on top. A sailing season is 1 May to Martinmas, 194 days
(review_systems.md's count); a winter is the other 171.

| What | Game time | Speed 1 | Speed 2 | Speed 3 | Speed 4 | Speed 5 |
|---|---|---|---|---|---|---|
| Dorestat to Traiectum and such river legs | 8 to 60 hours | 8 to 60 min | 2 to 15 min | 20 s to 2.5 min | 5 to 38 s | 1.3 to 10 s |
| Dorestat to Lundenwic | 2 to 5.5 days | 48 min to 2 h 12 | 12 to 33 min | 2 to 5.5 min | 30 to 83 s | 8 to 22 s |
| An ordinary card's week | 168 hours | 2 h 48 | 42 min | 7 min | 1 min 45 | 28 s |
| A pressing card's hour | 1 hour | 60 s | 15 s | 2.5 s | 0.6 s | 0.17 s |
| The bread aboard at the start | 7 days | 2 h 48 | 42 min | 7 min | 1 min 45 | 28 s |
| To the debt's day (1 May to Michaelmas) | 151 days | 60 h | 15 h | 2.5 h | 38 min | 10 min |
| A sailing season | 194 days | 78 h | 19.4 h | 3.2 h | 48.5 min | 12.9 min |
| A winter, if it is sailed through | 171 days | 68 h | 17 h | 2.9 h | 43 min | 11.4 min |
| The first life, sailing seasons only (793 to 805) | 2,328 days | 931 h | 233 h | 38.8 h | 9.7 h | 2.6 h |
| The whole timeline, sailing seasons only (793 to 1066) | 52,962 days | | | 883 h | 221 h | 59 h |
| The whole timeline, every day | 99,713 days | | | 1,662 h | 415 h | 111 h |

So ruling 26's two figures are right as counted on sailing days: ten hours of sailing at speed 4 for the first life,
and about 220 for the whole timeline. They are NOT right if winters are sailed through at speed 4 (415 hours). With
the season at speed 4 and each winter at speed 5 the whole is 221 + 52 = 273 hours; with the winter as one board
(as review_systems.md assumed, "every winter as one board") it is 221. **To decide, by the user:** is the winter a
board, or days that run? This write-up works either way; the sum does not.

Three sums written before today that the new clock makes right again, and one it leaves wrong:
- "A season is an evening" (GAME_MAKER.md line 194): true at speed 3 (3.2 hours).
- "Twelve to fifteen hours for a life" (GAME_MAKER.md line 416): true at speed 4 with the stops (9.7 hours of
  sailing). Ruling 23 called it wrong; ruling 26 corrects ruling 23.
- "The first thing at stake falls due fifteen real hours away" (review_newcomer.md, the first thing that "would
  stop me playing"): 38 minutes at speed 4, 2.5 hours at speed 3.
- "One good hour, Dorestat to Lundenwic" (QUEUE.md, GAME_MAKER.md) was sized for the old 4x, which is now speed 2.
  At speed 3 that crossing is 2 to 5.5 minutes. The first hour now holds weeks, not one crossing: see section 7.

## 2. How time runs today

| What | Where | How |
|---|---|---|
| The speed | `Game/UI/GameSpeed.cs` | `Speeds = { 1, 2, 4 }` (line 23). `Tick()` (109-130) reads Space, `=`/numpad `+`, `-`/numpad `-`, then sets `Time.timeScale = Factor` (122-124). `Factor` (41) is 0 when paused or held, else the speed. It pins `LevelClock.Speed = 1` and `LevelClock.Paused = false` each frame (128-129): the time scale carries everything. |
| Holds | `GameSpeed.Hold(why)` / `Release(why)` (86-96) | A set of reasons: "menu", the title, "alert" (an urgent alert), "panel" (`PanelClock`), "alongside" (`SimStage.HoldClock`), "aground". Any one makes `Paused` true. |
| A card holds the clock | `Game/UI/PanelClock.cs` 46-54, `FrameRoot.Floats.cs` 357 | `PanelClock.Tick()` (each frame, from `KapmanGame.Update` line 504, BEFORE the clock moves) asks `Holds()`, which is the frame's `FloatHoldsClock` (Floats.cs 96-108): is a floating window with `holdsClock` up and not let go. Under the frame `OnlyCards` is true (FrameRoot.cs 103): tiled windows never hold it. |
| The game's hours | `Runtime/Levels/LevelClock.cs` 160-161 | `GameHours += Time.deltaTime * rate * Speed / 3600`, order -100. `rate` is 60. The time scale is inside `Time.deltaTime`. |
| The ticks | `Core/Sim/TickLadder.cs` 65-93, driven from `KapmanGame.Update` 513-521 | Every minute, hour, day and year between two clock readings is raised, in order, once. A step over 2 game hours is told as a jump (`JumpHours`, line 34). Nothing listens to the minute. |
| The world's ships | `Game/Sim/WorldSim.cs` 244-277 `Hour()` | Called by a script graph ("Sim - the world": On game hour, then Sim: Hour; `Graphs/SimNodes.cs` 21). One call is one hour: a weather snapshot, every ship steps, they keep clear, they move, raiders hunt. It reads where the player's ship is (`UpdatePlayer`, `playerNow`). |
| The markets | `Game/Trade/Market.cs` 155-167 `CatchUp`, 399 `CatchUpAll` | Once a game day from `WorldSim.Hour` (line 251), and whenever a market is asked for (`Markets.Of`). A closed form over the span: catching up 30 days costs the same as one. |
| Bread, water, pay | `Game/Voyage/Voyage.cs` 224 `Day()` | Once a game day, from the graph "Voyage - each day" (`Graphs/VoyageNodes.cs` 39). |
| The cards' draw | `Core/Encounters/Scheduler.cs` 171-199 `Tick(hour)` | Once a game hour (`EncounterRunner.OnHour`, line 120), seeded by the world's seed and the hour. **A draw made while a card waits in line is dropped** (195-196). On the chart no sea card within 4 game hours of the last (`EncounterRunner.chartSeaGapHours`, line 49; `EncounterDirector.Tick` 317). |
| Her boat on the chart | `Game/Chart/ChartBody.cs` 66-122, order 20 | Each frame: `step = Time.deltaTime * rate * LevelClock.Speed / scale` (74); she turns up to 40 degrees a second (101), gathers way 0.6 m/s a second (113), and moves `speed * step` (115). When she is within 5 chart metres of her goal the order ends (95). |
| Her orders | `Game/Voyage/VoyagePlanner.cs` 87-345, order 0 | Each frame: aground, the tide wait, the meetings, then `Course.Advance(here, 15 chart metres)` and a new goal. |
| The lookout | `Game/Sim/SeaWatch.cs` 40-92 | When `game.Hours` has passed `nextLook` (every 10 game minutes, line 23): every ship of the world against how far she can be seen. |
| The weather | `WorldSim.WeatherAt` 130-136 | On the chart there is no level weather: the wind where she is comes from the world's model, a snapshot taken once a game hour in `Hour()` (257). It is a pure function of the seed and the hour. |
| Rest, and the wait for a tide | `Game/Voyage/Rest.cs` 21, 90; `TideWait.cs` (the class, its `Begin`); `voyage_rules.json` "waiting_rate" | They used to raise `clock.rate`: resting to 1800 (30 times), the tide wait and "aground: wait for the tide" to 3600 (60 times), with the speed buttons standing at 1x meanwhile (`GameSpeed.OwnPace`, line 46). |
| How the first builder capped them | `FrameRoot.cs` 104-105 | `Rest.RateCap = 1f; TideWait.RateCap = 1f;`. With a cap over 0 the rate becomes `min(1800, max(60, 1))` = 60: no change, so resting and the tide wait run at the chosen speed. **`Aground` was not capped** (`Aground.cs` 275 still sets `clock.rate = 3600`): on the chart "wait for the tide" when aground still runs the clock at 60 times today. |
| The frame's Wait button | `FrameRoot.Rack.cs` 16-82 `FrameWait` | A wait at the clock's own pace, looked at every half second of the wall clock (75-77). |
| The top bar | `FrameRoot.cs` 200-207, 303-319, 693-722 | Pause, then one button a speed labelled "1x", "2x", "4x"; a word after them: "paused", "waits for your answer", "held". |
| The old dev keys | `KapmanGame.cs` 460-478 | `[` and `]` set `clock.rate` to 60, 3600 or 86400 (editor and development builds). |

## 3. What happens at a day in fifteen seconds and a day in four

At speed 4 one frame is 1.6 game minutes at sixty frames a second and 3.2 at thirty. At speed 5 it is 6 and 12, and a
half-second hitch is three game hours. This table is what would happen if speeds 3 to 5 were added to `Speeds` and
nothing else changed. "Fix" is what section 5 does about it; "done" means it is in the compiled draft.

### 3.1 Steps too coarsely

| What | At speed 5 | Fix |
|---|---|---|
| Unity's time scale | 96 is under Unity's limit of 100, 360 is not (not tried here). Also, a time scale of 96 makes every animation, every `WaitForSeconds` and the view of the ship run 96 times fast. | The speed is not the time scale: the game steps its own clock. Done. |
| Her boat, `ChartBody` | At 4 knots she moves 741 real metres a frame: 12.4 chart metres. Her order ends within 5 of the goal (line 95): she steps over it and turns back, 40 degrees times the frame. At 8 knots, 25 chart metres a frame against the planner's 15 (`passGameM`) and the port's 30. | One step is one game minute: 2 chart metres at 4 knots, at every speed. Done (`ChartBody.Step`). |
| Her orders, `VoyagePlanner` | The next mark of the course is given once a frame. A river's marks are "often only metres off" (EncounterRunner.cs line 154): she reaches one in the middle of a frame and has no order for the rest of it. She is slower at speed 5 than at speed 1 over the same water. | The planner runs in the step, before she sails it. Done (`VoyagePlanner.Step`). |
| A card holds the clock a frame late | The draw is in the hour's tick; the window that holds the clock is made by the frame later in the same frame; `PanelClock` sees it the frame after. One more frame of clock has run by then: 1 game second at speed 1, 6 to 12 game minutes at speed 5, 3 hours in a hitch. The world is not the same world. | After a step in which anything of that kind happened the clock stands two frames (`KapmanGame.Sign`, `SettleFrames`). Done. |
| The lookout, `SeaWatch` | "Every 10 game minutes" is tested once a frame, so at speed 5 it looks every 12 and never on the same minutes twice: a sail is made out at a different minute than at speed 1. | It looks in the step, on every tenth minute. Done (`SeaWatch.Step`). |
| The world's hour reads her place | `WorldSim.Hour` runs from the tick ladder BEFORE her boat has moved this frame (KapmanGame is order -40, ChartBody 20): the world's ships keep clear of where she was a frame ago. A frame is 741 metres at speed 5. | In the step she sails the minute first, then the hour ticks. Done (`StepMinute`). |
| The mooring watch | `EncounterRunner.dwellSeconds = 2` is counted in real seconds on the chart (line 158): the call begins 2 real seconds after she is laid to. That is 2 game minutes at speed 1 and 12 game hours at speed 5. | Game seconds, from the step. Done (`EncounterRunner.Step`). |
| The Wait button | `FrameWait.Tick` looks every half second of the wall clock (Rack.cs 75-77): 3 game hours at speed 5. "First light" comes at nine in the morning; a fair slant of two hours is never seen. It also reads `LevelClock.SunAltitude`, which is the sun at 52 N 5 E and a frame old. | Looked at in the step: first light every minute, by the sun where she is; the wind every tenth minute. Done (`FrameWait.Step`). |
| Raiders coming on | `RaiderEncounter.Look` every half second of the wall clock, 5 seconds after it has told the graph (lines 44, 57). | Game minutes on the chart. Done. |
| A timed talk | `TextWaitForChoiceTimed` (TextNodes.cs 132) counts `Time.deltaTime`: with the time scale at 1 "sixty seconds is an hour of the game" would become sixty real seconds at every speed. The raiders' "no answer: run" after twenty game minutes would take twenty real seconds: two hours at speed 5. | It counts the steps the frame ran. Done. |
| Aground | `Aground.MinutesThisFrame` (line 49) is the frame's time. | The step's time. Done. |
| Dawn, for a rest | `Rest.Tick` reads the sun once a frame. | While she rests the clock runs one step a frame at most (`OneAFrame`): a night is 8 real seconds. Done. |
| The fog of war | `KapmanGame.Explore` lifts it once a real second (279), 30 km round her. At speed 5 and 8 knots she is 88 km on by the next: gaps. | Every 30 game minutes, in the step. Done. |
| The port's graph | "Ports - coming in and leaving" has two per-frame nodes: is she near and slow enough to lay to. | Within twice a port's reach the clock runs one step a frame at most (`OneAFrame`), so the graph looks between every two minutes as it does at speed 1. Done; to be proven by the play-mode test (6.3). |
| A hold with no talk | `HoldWatch.LostSeconds = 6` real seconds (SeaHolds.cs 19) is 36 game hours at speed 5. | It is a last resort for a broken window. Left; one step a frame while anything holds her. |

### 3.2 Could be skipped

| What | How it would be lost | Fix |
|---|---|---|
| Cards | `Scheduler.Tick` drops the hour's draw while a card waits in line (195-196). With the hold a frame late, the hours that tick in that frame draw into a full line: cards that would have come at speed 1 never come. | The clock stands before the next hour can tick. Done. |
| A card's own option | fixes/card_clock.md counts the wait each frame (`d.Wait(dh)` in `EncounterRunner.Update`). A pressing card's hour is a sixth of a second at speed 5; it would take its option up to 12 game minutes late, with the world running. | Count it in the step: the line is marked in `EncounterRunner.Step`. For the builder of the card clock. |
| Ticks | None are skipped today: the ladder raises every one. But a step over 2 game hours raises `Jumped` first, and at speed 5 a 0.34-second frame is that. `Wildlife` listens to it. | Under the stepper a step is one minute: no jump. |
| The weather | Nothing: it is a function of the hour, sampled by the world each hour and by her boat each step. | None needed. |
| The markets | Nothing: `CatchUp` is exact over any span. | None needed. |
| The log | Not lost, but at 6 world hours a second the frame's log scrolls past unread. | Section 7.4. |

### 3.3 Costs too much to compute

Measured figures are from the test log of 4 October (`Logs/tests_weather_wire.xml`), in the editor, set-up included:
a year of the world (8,760 hours) in 27.9 s; 40 summer days (960 hours) in 13.3 s. So one `WorldSim.Hour()` is at
most 3.2 ms averaged over a year and at most 13.9 ms in summer (the true figures are lower by the set-up's share,
which I could not split off).

| What | How often at speed 5 | Cost | Verdict |
|---|---|---|---|
| `WorldSim.Hour()` | 6 a real second | 3 to 14 ms each, all in one frame | The one real risk: a spike of up to a whole 60-fps frame, six times a second. The stepper stops stepping when a frame has spent 8 ms in steps (`StepBudgetMs`) and carries the rest: the clock then runs slower than the speed says, as CK3's does, and nothing is skipped. If the spike itself is over 8 ms the fix is to spread the ships of an hour over its minutes; that changes the order they deal in, so money.md's tables would have to be run again. Not drafted. **Measure first:** `KapmanGame.StepMsLastFrame`. |
| The cards' hourly draw | 6 a real second | A loop over about 1,390 cards; about 220 (sea and crew) reach their condition | Not measured. Small beside the world's hour. |
| The lookout | 36 looks a real second | One range sum a ship | Small. |
| Her boat's wind | 360 samples a real second (one a step) | One sample of the hour's snapshot | Not measured; if it shows, sample every tenth step. |
| The planner | 360 a real second | Distance sums; `Port.Nearest` | Small. |
| `KapmanGame.Sign` | 720 a real second | A dozen reads and one `Port.Nearest` | Small. |
| The markets, bread, pay, events of the day | once in 4 seconds | As today | Nothing. |
| The frame's top bar | each frame | As today | Nothing. |
| The sky | each frame | `LevelClock.Apply`: one sky sum. The chart's clock has no Sun or Moon light (ship_view/PLAN.md line 35). | Nothing on the chart. For the ship view see 4.6. |

## 4. The design, made exact

### 4.1 The step

- One step is one game minute (`ClockStepper.StepSeconds = 60`), at every speed.
- Each frame: real seconds (`Time.unscaledDeltaTime`, never more than 0.1 counted) times the speed's rate is added
  to what is owed; each whole 60 owed is one step. What is left under a step waits for the next frame.
- At most 40 steps a frame. A frame may spend at most 8 real milliseconds in steps. Past either, the clock runs
  slower than its speed says. Nothing is ever skipped to catch up.
- A step, always in this order (`KapmanGame.StepMinute`): (1) her orders; (2) she sails the minute; (3) the clock
  is set to the minute and the tick ladder raises the minute, then on the hour the world's ships and the cards'
  draw, then on the day the markets, the bread and pay, the day's events; (4) the lookout, on every tenth minute;
  (5) the mooring watch, and an open card's waiting time; (6) a wait for light or wind; (7) the fog of war, every
  thirtieth minute; (8) the reasons to drop back.
- After a step in which something happened that the per-frame side of the game must answer (a card in line, a talk,
  she was laid to, a meeting's stage, the speed), the frame's remaining steps are let go and the clock stands two
  frames. The frame then has the card up and holding the clock before another minute passes.
- Within twice a port's reach, in a meeting at sea, aground, resting, or with a talk open: one step a frame at most.
- A guard: should something the clock watches change every frame, it would never run. After thirty such frames in
  a row it runs anyway and says so once in the log (`KapmanGame.Sign`).
- `Time.timeScale` is 1 while the clock runs and 0 when it is stopped, as today. `LevelClock.Speed` stays 1, so
  `LevelClock.TimeFactor` is 1: that is the view's pace (4.6).
- In a 3D level (no chart) nothing changes: the speed is the time scale there, and speeds 2 to 5 are all four
  times. The first release has no 3D level.
- Paused, the planner and the mooring watch still look each frame with no time passing, as they do today.

### 4.2 Keys and buttons

| Key | Does |
|---|---|
| Space | Pause, and go on. With a card holding the clock: lets the clock run with the card open (ruling 5), no faster than speed 2 (4.5). |
| `+` (the `=` key, numpad `+`) | One speed faster. **Right after the clock dropped back by itself: straight back to the speed she had.** |
| `-` (numpad `-`) | One speed slower. |
| 1 to 5 | Not the clock's: the workspaces (`FrameRoot.Keys`, line 438) and a card's choices (`FrameRoot.Floats.cs` 521). |

Buttons on the top bar: `Pause`, `1`, `2`, `3`, `4`, `5`. Not "1x" and so on: the multiples are 1, 4, 24, 96 and 360
and nobody wants to read them. Each button's tip says what the speed is (`GameSpeed.Tips`). A click chooses that
speed; chosen by hand, the "speed she had" is forgotten.

`[` and `]` (the dev rates) do nothing on the chart after this: they set `clock.rate`, which the stepped clock does
not read. The five speeds replace them.

### 4.3 What stops the clock, what drops it, to which speed, and the switch

A HOLD stops the clock (it is `GameSpeed.Hold`, as today). A DROP lowers the speed and never stops it
(`GameSpeed.DropTo`); it never raises the speed; a second drop keeps the first speed to go back to. Every drop is
asked in the step, so it comes on the same game minute at every speed. Each has a switch (`ClockSettings`, kept in
PlayerPrefs under `kapman.clock.drop.<name>`), all on from the start but one.

| Reason | Exactly when | What the clock does | Switch (`ClockReason`) |
|---|---|---|---|
| A card that needs an answer | The card's window is up and not let go (`PanelClock`, as today) | HOLD. Space or a speed lets it run with the card open. | `PanelClock.On` is the switch and is in the code already (PanelClock.cs line 22); it is not kept anywhere. One line keeps it (5.3). With it off a card opens over a running clock: then it must drop the clock to speed 2 as letting it run does, or a week is 28 seconds. |
| A pressing matter | `SeaHolds.Now` is the guard boat, a port's search, raiders coming, chasing or alongside; or `Aground.Stage` is on; or a pressing card is open (`ClockDrops.PressingCard`, for the card clock to set) | Drop to 1. **And no faster than speed 2 until it is answered** (`GameSpeed.Cap`): the chosen speed is kept and comes back when it is over. | `Pressing` |
| Raiders made out | `SeaWatch.MadeOut` for a raider's ship (within 10 real km, in sight) | Drop to 1 | `Raiders` |
| Any other sail made out | `SeaWatch.MadeOut` for any other ship | Drop to 2 | `Sail`, **off from the start**: on the Rhine a sail is made out every few minutes (fix_sea_meetings.md line 55) and speed 4 would be unusable. A meeting that is offered is a talk, and holds. |
| Nearing port | She is within 5 real km of the last mark of her course (40 game minutes at 4 knots: 10 real seconds at speed 2) | Drop to 2 | `NearPort` |
| Made fast | `ship.anchored` and `Port.LyingId()` is a port | Drop to 1. In port the clock runs at a game minute a second while she trades (ruling 5: only a card stops it). | `Moored` |
| A wait is over | `FrameWait`: first light, or the wind has come fair. (Not in the draft: the tide has made, `TideWait.cs` 212 and 217; dawn after a rest, `Rest.cs` 62. One line each: 5.3.) | Drop to 1 | `WaitOver` |
| Bread for under two days | `Voyage.FoodDays` falls under 2 (the top bar's own warning line, FrameRoot.cs 713) | Drop to 2. Armed again when there is bread for 3 days. | `BreadLow` |
| No bread | `Voyage.FoodDays` under a quarter of a day (the bar's "no bread aboard", 712) | Drop to 1 | `BreadGone` |
| A week to the debt's day | `Trade.DebtDue - Day` falls to 7 | Drop to 2 | `DebtNear` |
| The debt's day | `Trade.DebtDue - Day` falls to 1 | Drop to 1. The reckoning itself is a card (fixes/debt_day.md) and holds. | `DebtDay` |
| A talk, or the chest | The player opens a talk with someone, or the chest (not in the compiled draft: one line each, 5.3) | Drop to 1 | `Talk` |

Not reasons, and why: a gale and she lies to (she is safe; the log says it; the player may well want it to blow
over fast); night at sea; news heard; a sail merely sighted; a port passed that she is not bound for. An order that
falls due and the fair that opens are candidates for later, with the debt's two rows as the pattern.

After a drop the top bar says why and how to go back (4.4). Nothing ever raises the speed by itself.

Where the switches go: there is no settings page in the game yet (`MainMenu.cs` has none). Until there is, put them
in the pause menu under a heading "The clock slows by itself for": one row a reason, the words from
`ClockSettings.Label(reason)`, a tick from `ClockSettings.On(reason)`, set by `ClockSettings.Set(reason, value)`.

### 4.4 What the top bar shows

- `Pause 1 2 3 4 5`, the chosen one lit (class `on`, as today).
- After a drop: the speed she had is marked (class `was`: an underline, say), and the word after the buttons reads
  `slowed: nearing port.  [+] back to 4`. Its tip: "The clock slowed by itself. Press + to go back to the speed you
  had. Each reason has a switch in the settings."
- While a pressing matter caps the clock: the buttons over the cap are dimmed (class `capped`), and if the chosen
  speed is over it the word reads `no faster than 2 until it is answered`.
- Paused: `paused`. A card: `waits for your answer` (as today).
- The date always. The part of the day ("morning") at speeds 1 to 4; at speed 5 it lasts under a second and is left
  out. The hour itself is not on the bar today.
- Silver, bread and the debt as today: they tick by the game day, which at speed 5 is every four seconds.

Two rules for `Assets/Kapman/Game/UI/Frame.uss`, after `.frame-root .frame-k.on` (line 568; the colours are the
builder's to pick from the frame's own):

```css
.frame-root .frame-k.was { border-bottom-width: 3px; }
.frame-root .frame-k.capped { opacity: 0.45; }
```

### 4.5 How a card's week and a pressing card's hours read at speed 5

Everything counts game time (ruling 26), so the numbers on a card do not change with the speed. What changes is how
long they last:

- **An ordinary card** waits 168 game hours of running clock: 28 real seconds at speed 5. It holds the clock when
  it opens, so none of that runs until the player lets it. **When the player lets the clock run with a card open
  (Space), it goes on at speed 2 at most** (`GameSpeed.LetRunLevel`): the week is then 42 minutes, and the bar says
  `slowed: a card is open.  [+] back to 5`. A speed chosen by hand is taken as meant.
- **A pressing card** waits 1 to 72 game hours (card_rules_793.json: 1, 2, 3, 6, 12, 24, 48, 72). An hour is a
  sixth of a second at speed 5. While it is unanswered the clock cannot run faster than speed 2 (an hour is 15
  seconds; 72 hours is 18 minutes), and it drops to 1 when the matter begins.
- **The line on the card** (fixes/card_clock.md section 6, `WaitLine` and `Span`) stays in game time: "It will wait
  6 days and 4 hours more, then: ...". Two changes to it for the new clock: at speeds 4 and 5 say whole days only
  while more than a day is left (the hours are a blur); and when what is left is under two real minutes at the
  running speed, add the real time, because that is the number the player needs: "It will wait 2 hours more (30
  seconds at this speed), then: Run." Real seconds left = hours left x 3600 / `GameSpeed.Rate`.
- **Count the wait in the step**, not in `Update` as card_clock.md section 3 has it (it was written for the slow
  clock): `d.Wait(gameSeconds / 3600.0)` in `EncounterRunner.Step`, where the draft below marks the place. Then a
  card takes its own option on the same game minute at every speed, which the same-seed test needs.

### 4.6 What the view of the ship does at high speed

Ruling 26: "it keeps its own calm motion and lets the sky and light pass".

- **Her motion is on real time.** The time scale is 1 at every speed and `LevelClock.TimeFactor` is 1. So the
  waves, the oars, the sail and the men move as they do at speed 1. ship_view/LOOK_AND_MOTION.md line 465 already
  has the stroke on real time. ship_view/PLAN.md lines 109, 232, 272 and 494 multiply her way through the water by
  "the clock's factor" (`timeFactor`, `LevelClock.TimeFactor`): that factor is now always 1, which is right, and the
  stage must NOT take the game's rate in its place.
- **The sky follows the clock.** The Sun, the Moon and the stars stand where the game's hour puts them. At speed 3
  that is a day a minute: a time-lapse, and good to look at. At speed 4 a day is 15 seconds; at speed 5 it is 4
  seconds and the night is about one. That is a slow pulse, under any rate that harms (a quarter of a hertz), but
  it is tiring. So at speeds 4 and 5 the view's LIGHT is eased: the brightness it shows follows the true one with a
  lag of about a second and a half of real time, so the night is a dimming and not a blink; the Sun and the Moon
  are still drawn where they are. One switch, "Soften the light at the highest speeds", on from the start. Not
  drafted: the stage is not built.
- **The weather is eased the same way.** On the chart the wind changes on the game hour, six times a second at
  speed 5. The stage must take the wind, the cloud, the rain and the fog through an easing of about three real
  seconds, or the sea will twitch. At speed 5 it then shows the day's weather, not the hour's.
- **No warm start in every frame.** PLAN.md line 102 warm-starts the sea model "after a jump of the clock". With the
  stepped clock a frame at speed 5 is 6 to 36 game minutes: that is not a jump. Call it one only over 2 game hours
  in a frame (`TickLadder.JumpHours`), which the stepper never does.
- **Sounds.** One-shots tied to things that happen in game time (a call, a bell, a gull at a sighting) must not
  come more than once in four real seconds at speeds 3 and over, or they rattle. The beds change by where she is
  with a cross-fade in real time (fixes/chart_sound.md): at speed 5 she crosses a zone in a second or two, so a
  fade of two real seconds is the floor.
- **On the chart** her mark is moved once a step: once a real second at speed 1. `ChartBody.From` and
  `KapmanGame.StepFraction` are there so the mark can be drawn between the two places (5.3). The world's ships
  already glide between their hourly places (`MapView.cs` 1164).

## 5. The code

Three new files, one file replaced whole, and 26 edits in 12 files. Every block here is the text that was compiled
(the edits were applied by a script to copies of the project's files, and the same script wrote the blocks down).
The line numbers are where the text stood when I read it; six builders are in these files, so find by the text.

### 5.1 New files, and one replaced

**New.** `Assets/Kapman/Code/Core/Sim/ClockStepper.cs` (beside `TickLadder.cs`, in Kapman.Core: no Unity in it)

```csharp
using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE CLOCK'S STEP (ruling 26 of 7 October 2026: a pause and five speeds). The game's time moves in whole game
    /// minutes, at every speed: a frame at speed 1 runs no step or one, a frame at speed 5 runs six or more, and each
    /// step is the same step. So what happens in the world does not depend on the speed it was played at, nor on how
    /// long the frames were: only on how many steps have been run. Pure C#, no scene: the game (KapmanGame.RunClock)
    /// asks <see cref="Take"/> how many steps this frame wants, and calls <see cref="Step"/> for each one it runs.
    /// </summary>
    public sealed class ClockStepper
    {
        /// <summary>One step is one game minute, at every speed.</summary>
        public const double StepSeconds = 60.0;
        /// <summary>A frame longer than this (real seconds) counts as this long: a hitch is not a jump of the clock.</summary>
        public const double LongestFrame = 0.1;
        /// <summary>The most steps one frame may run (speed 5 at ten frames a second is 36).</summary>
        public const int MostSteps = 40;

        double owed;                                              // game seconds wanted and not yet run

        /// <summary>Whole game minutes since the clock's start.</summary>
        public long Minute { get; private set; }
        /// <summary>The same in game hours.</summary>
        public double Hours => Minute / 60.0;
        /// <summary>How far the clock is into the step to come, 0..1 (for drawing her mark between two steps).</summary>
        public double Fraction => Math.Max(0.0, Math.Min(1.0, owed / StepSeconds));

        /// <summary>The clock was set from outside (a new game, a loaded one): the stepper stands at its minute, owing nothing.</summary>
        public void SetTo(double gameHours)
        {
            Minute = (long)Math.Floor(gameHours * 60.0 + 1e-6);
            owed = 0.0;
        }

        /// <summary>
        /// A frame of `realSeconds` at `rate` game seconds a real second: how many whole steps it wants now. What is
        /// left under one step is kept for the next frame. A rate of 0 (paused, held) wants none and owes none.
        /// </summary>
        public int Take(double realSeconds, double rate)
        {
            if (!(rate > 0.0))
            {
                owed = 0.0;
                return 0;
            }
            if (realSeconds > 0.0)
                owed += Math.Min(realSeconds, LongestFrame) * rate;
            double most = MostSteps * StepSeconds;
            if (owed > most)
                owed = most;                                      // (what the machine could not run is not owed: the clock runs slower, nothing is skipped)
            return (int)Math.Floor(owed / StepSeconds + 1e-9);
        }

        /// <summary>One step is run: the minute it ends on.</summary>
        public long Step()
        {
            owed = Math.Max(0.0, owed - StepSeconds);
            return ++Minute;
        }

        /// <summary>Something needs the player, or the speed changed: what was still wanted of this frame is let go.</summary>
        public void Stop() => owed = 0.0;
    }
}
```

**Replaces the whole file.** `Assets/Kapman/Code/Game/UI/GameSpeed.cs` (what is gone: `Speeds`; what is new: `Rates`, `Tips`, `Count`, `Stepped`, `Cap`, `Running`, `Rate`, `Before`, `DroppedFor`, `Faster`, `Slower`, `DropTo`, `LetRunLevel`; `Slow()` stays for `Alerts`)

```csharp
using System;
using System.Collections.Generic;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME'S SPEED - one owner for how fast the game runs. Ruling 26 of 7 October 2026: a pause and five speeds,
    /// like CK3. Speed 1 is a game minute a real second (a day in 24 minutes); 2 is four times that (a day in 6
    /// minutes); 3 a day in a minute; 4 a day in fifteen seconds; 5 a day in four seconds.
    ///   - [Space] pauses and goes on; [+] and [-] (or the top bar's buttons) choose the speed. The number keys are the
    ///     workspaces' and the cards'.
    ///   - Things that must stop the game hold it while they're on (the menu, a card that wants an answer); it runs
    ///     again when the last hold is let go.
    ///   - The clock drops back by itself when something needs the player (<see cref="DropTo"/>; the reasons and their
    ///     switches are ClockDrops and ClockSettings). The speed she ran at is kept (<see cref="Before"/>): [+] goes
    ///     straight back to it.
    ///   - On the chart the speed is NOT Unity's time scale. The game steps its own clock, a game minute at a time
    ///     (<see cref="Stepped"/>, KapmanGame.RunClock, <see cref="Rate"/>), and the time scale stays 1 (0 when stopped):
    ///     so the view of the ship, the water and every animation keep their own calm pace at every speed.
    ///   - In a 3D level (no chart) the old way holds: the speed is the time scale, and nothing there runs faster
    ///     than four times (speeds 2 to 5 are all 4x there).
    /// The clock's own hold (two ships alongside, dealing) stops only the game's clock, not the sea: that stays with the
    /// clock (GameClock.held).
    /// </summary>
    public static class GameSpeed
    {
        /// <summary>Game seconds a real second at each of the five speeds.</summary>
        public static readonly double[] Rates = { 60.0, 240.0, 1440.0, 5760.0, 21600.0 };
        /// <summary>What each speed is, for the buttons (placeholder wording).</summary>
        public static readonly string[] Tips =
        {
            "Speed 1. A game minute a second: a day in 24 minutes. For being aboard.",
            "Speed 2. Four game minutes a second: a day in 6 minutes.",
            "Speed 3. An hour in two and a half seconds: a day in a minute.",
            "Speed 4. A day in fifteen seconds.",
            "Speed 5. A day in four seconds.",
        };
        public const int Count = 5;

        /// <summary>Which of the five is chosen: 0 is speed 1.</summary>
        public static int Level { get; private set; }
        /// <summary>Paused by the player.</summary>
        public static bool PlayerPaused { get; private set; }
        /// <summary>Multiplayer: no pausing, no faster speeds (the world is everyone's).</summary>
        public static bool Multiplayer;
        /// <summary>The game steps its own clock (the chart: KapmanGame sets it each frame). Off: the speed is the time scale.</summary>
        public static bool Stepped;
        /// <summary>The fastest level the clock may run at now (a pressing matter unanswered: ClockDrops sets it). The
        /// chosen level is kept: when the cap goes, the clock runs at it again.</summary>
        public static int Cap = Count - 1;

        /// <summary>The level she ran at before the clock dropped back by itself (-1: it has not, or the player has
        /// chosen a speed since).</summary>
        public static int Before { get; private set; } = -1;
        /// <summary>Why it dropped, in a few words for the top bar ("" none).</summary>
        public static string DroppedFor { get; private set; } = "";

        static readonly HashSet<string> holds = new HashSet<string>();

        /// <summary>The speed or the pause changed.</summary>
        public static event Action Changed;

        public static bool Paused => !Multiplayer && (PlayerPaused || holds.Count > 0);
        public static bool HeldFor(string why) => holds.Contains(why);

        /// <summary>The level the clock runs at now: the chosen one, under the cap.</summary>
        public static int Running => Multiplayer ? 0 : Mathf.Clamp(Mathf.Min(Level, Cap), 0, Count - 1);

        /// <summary>Game seconds a real second now: 0 stopped, else the running speed's.</summary>
        public static double Rate => Paused ? 0.0 : Rates[Running];

        /// <summary>The time scale wanted now. Stepped (the chart): 0 stopped, else 1 at every speed. Not stepped (a 3D
        /// level): the speed itself, no more than four times (1x while resting or in multiplayer).</summary>
        public static float Factor => Paused ? 0f : Stepped || Multiplayer || OwnPace ? 1f : Mathf.Min(4f, (float)(Rates[Running] / 60.0));

        /// <summary>A 3D level only: resting or waiting for a tide at a pace of its own (the clock's rate raised).</summary>
        static bool OwnPace => (Rest.On && Rest.RateCap <= 0f) || (TideWait.On && TideWait.RateCap <= 0f);

        public static string Describe => Paused ? (!PlayerPaused && holds.Count == 1 && holds.Contains(PanelClock.Why) ? "the clock waits" : "paused") : $"speed {Running + 1}";

        /// <summary>The player chooses a speed (a button, or through <see cref="Faster"/> and <see cref="Slower"/>).</summary>
        public static void SetLevel(int level)
        {
            level = Mathf.Clamp(level, 0, Count - 1);
            if (Multiplayer)
                level = 0;
            bool unpause = PlayerPaused;
            PlayerPaused = false;
            bool forgot = Before >= 0;                                    // (the player has chosen: nothing to go back to)
            Before = -1;
            DroppedFor = "";
            if (PanelClock.GoOn() || forgot)                              // (a card held the clock: a speed chosen sets it running, the card waits)
                Changed?.Invoke();
            if (level == Level && !unpause)
                return;
            Level = level;
            if (unpause)
                Alerts.AnswerWaitingUrgent();
            Changed?.Invoke();
        }

        /// <summary>[+]: after the clock dropped back by itself, straight back to the speed she ran at; else one faster.</summary>
        public static void Faster() => SetLevel(Before > Level ? Before : Level + 1);

        /// <summary>[-]: one slower.</summary>
        public static void Slower() => SetLevel(Level - 1);

        public static void TogglePause()
        {
            if (Multiplayer)
                return;
            if (PlayerPaused || holds.Contains(Alerts.Hold))
            {
                PlayerPaused = false;
                Alerts.AnswerWaitingUrgent();                              // (going on answers what was waiting)
            }
            else if (PanelClock.GoOn())
            {
                // (a card held the clock and the player goes on: the clock runs and the card waits, still open. Its
                // week or its hours are counted from now, so the clock goes on no faster than speed 2 until the
                // player asks for more: at speed 5 a card's week is 28 seconds)
                DropTo(LetRunLevel, "a card is open");
            }
            else
                PlayerPaused = true;
            Changed?.Invoke();
        }

        /// <summary>The fastest level the clock goes on at when the player lets it run with a card open (1: speed 2).</summary>
        public const int LetRunLevel = 1;

        /// <summary>Stops the game for something (the menu, a card) until <see cref="Release"/>.</summary>
        public static void Hold(string why)
        {
            if (!string.IsNullOrEmpty(why) && holds.Add(why))
                Changed?.Invoke();
        }

        public static void Release(string why)
        {
            if (why != null && holds.Remove(why))
                Changed?.Invoke();
        }

        /// <summary>The clock drops back by itself to a level (0 is speed 1), and says why. It never raises the speed.
        /// The speed she ran at is kept for <see cref="Faster"/>. True if it dropped.</summary>
        public static bool DropTo(int level, string why)
        {
            level = Mathf.Clamp(level, 0, Count - 1);
            if (Multiplayer || Level <= level)
                return false;
            if (Before < 0)
                Before = Level;                                           // (a second drop keeps the first speed to go back to)
            Level = level;
            DroppedFor = why ?? "";
            Changed?.Invoke();
            return true;
        }

        /// <summary>Back to speed 1 (a non-urgent alert: you'll want to see it come).</summary>
        public static void Slow() => DropTo(0, "");

        /// <summary>Each frame (the game): the keys, and the time scale set to what's wanted.</summary>
        public static void Tick()
        {
            var kb = Keyboard.current;
            bool menu = MainMenu.Current != null && MainMenu.Current.Open;
            if (kb != null && !menu && !Multiplayer && !UiTyping.Active)
            {
                if (kb.spaceKey.wasPressedThisFrame)
                    TogglePause();
                else if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
                    Faster();
                else if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
                    Slower();
            }
            float want = Factor;
            if (!Mathf.Approximately(Time.timeScale, want))
                Time.timeScale = want;
            // the level clock has a speed and a pause of its own (its Tab and Space keys): kept at 1x and unpaused, so
            // what reads LevelClock.TimeFactor (the view of the ship, the water) runs at its own calm pace. Stepped, the
            // level clock does not add the frame's time to its hours either: KapmanGame.RunClock moves them
            LevelClock.Speed = 1f;
            LevelClock.Paused = false;
            LevelClock.External = Stepped;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Level = 0;
            PlayerPaused = false;
            Stepped = false;
            Cap = Count - 1;
            Before = -1;
            DroppedFor = "";
            holds.Clear();
            Changed = null;
            Time.timeScale = 1f;
            LevelClock.External = false;
        }
    }
}
```

**New.** `Assets/Kapman/Code/Game/UI/ClockDrops.cs`

```csharp
using System;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>Why the clock drops back by itself (ruling 26 of 7 October 2026). Each has a switch in the settings.</summary>
    public enum ClockReason
    {
        /// <summary>A pressing matter waits for an answer: raiders, the king's men, aground, a pressing card.</summary>
        Pressing,
        /// <summary>A band of raiders is made out by the lookout.</summary>
        Raiders,
        /// <summary>Any other sail is made out (off by default: on the Rhine that is every few minutes).</summary>
        Sail,
        /// <summary>She is within <see cref="ClockDrops.NearPortM"/> of the berth she is bound for.</summary>
        NearPort,
        /// <summary>She is made fast at a port.</summary>
        Moored,
        /// <summary>A wait is over: first light, a fair wind, the tide has made.</summary>
        WaitOver,
        /// <summary>Bread for under two days.</summary>
        BreadLow,
        /// <summary>No bread aboard.</summary>
        BreadGone,
        /// <summary>The debt falls due within seven days.</summary>
        DebtNear,
        /// <summary>The debt falls due within a day.</summary>
        DebtDay,
        /// <summary>The player opens a talk with someone, or the chest: reading takes the time it takes.</summary>
        Talk,
    }

    /// <summary>The switches for the clock's own drops (kept on this machine, not in the saved game).</summary>
    public static class ClockSettings
    {
        public const string Pref = "kapman.clock.drop.";
        static bool[] on;

        /// <summary>On from the start, all but <see cref="ClockReason.Sail"/>.</summary>
        public static bool Default(ClockReason why) => why != ClockReason.Sail;

        public static bool On(ClockReason why)
        {
            Load();
            return on[(int)why];
        }

        public static void Set(ClockReason why, bool value)
        {
            Load();
            on[(int)why] = value;
            PlayerPrefs.SetInt(Pref + why, value ? 1 : 0);
        }

        static void Load()
        {
            if (on != null)
                return;
            var all = (ClockReason[])Enum.GetValues(typeof(ClockReason));
            on = new bool[all.Length];
            foreach (var why in all)
                on[(int)why] = PlayerPrefs.GetInt(Pref + why, Default(why) ? 1 : 0) != 0;
        }

        /// <summary>The level each reason drops the clock to (0 is speed 1). What is happening now drops it to speed 1;
        /// what is coming drops it to speed 2.</summary>
        public static int Level(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Sail:
                case ClockReason.NearPort:
                case ClockReason.BreadLow:
                case ClockReason.DebtNear:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>The few words the top bar says after the drop (placeholder wording).</summary>
        public static string Says(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Pressing: return "a pressing matter";
                case ClockReason.Raiders: return "raiders made out";
                case ClockReason.Sail: return "a sail close";
                case ClockReason.NearPort: return "nearing port";
                case ClockReason.Moored: return "moored";
                case ClockReason.WaitOver: return "the wait is over";
                case ClockReason.BreadLow: return "bread for two days";
                case ClockReason.BreadGone: return "no bread";
                case ClockReason.DebtNear: return "the debt, in a week";
                case ClockReason.DebtDay: return "the debt's day";
                case ClockReason.Talk: return "talking";
                default: return "";
            }
        }

        /// <summary>The line beside the switch in the settings (placeholder wording).</summary>
        public static string Label(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Pressing: return "A pressing matter: slow to speed 1, and no faster than speed 2 until it is answered";
                case ClockReason.Raiders: return "Raiders made out: slow to speed 1";
                case ClockReason.Sail: return "Any sail made out: slow to speed 2";
                case ClockReason.NearPort: return "Nearing the port she is bound for: slow to speed 2";
                case ClockReason.Moored: return "Made fast in port: slow to speed 1";
                case ClockReason.WaitOver: return "First light, a fair wind, the tide: slow to speed 1 when the wait is over";
                case ClockReason.BreadLow: return "Bread for under two days: slow to speed 2";
                case ClockReason.BreadGone: return "No bread aboard: slow to speed 1";
                case ClockReason.DebtNear: return "A week before the debt's day: slow to speed 2";
                case ClockReason.DebtDay: return "The debt's day: slow to speed 1";
                case ClockReason.Talk: return "Talking with someone, or reading in the chest: slow to speed 1";
                default: return why.ToString();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Forget() => on = null;
    }

    /// <summary>
    /// THE CLOCK DROPS BACK BY ITSELF (ruling 26): each step of the clock (KapmanGame.StepMinute, last of all) this looks
    /// at what has BECOME true in that minute and drops the speed for it, once: it never holds the clock (a card does
    /// that: PanelClock) and never raises it. Each reason is told once and armed again when it is over, so the player who
    /// goes back to speed 5 with bread for a day is not slowed again every minute. Because it is asked in the step, in
    /// game time, the drop comes at the same game minute whatever speed the clock was running at.
    /// </summary>
    public static class ClockDrops
    {
        /// <summary>Real metres from the berth she is bound for at which she is "nearing port" (40 game minutes at 4 knots).</summary>
        public const double NearPortM = 5000.0;
        public const double BreadLowDays = 2.0, BreadGoneDays = 0.25, BreadArmDays = 3.0;
        public const double DebtNearDays = 7.0, DebtDayDays = 1.0;
        /// <summary>The fastest level while a pressing matter is unanswered (1: speed 2, where an hour is 15 seconds).</summary>
        public const int PressingCap = 1;

        /// <summary>A pressing card is open and unanswered (the card clock sets it: fixes/card_clock.md; null: none).</summary>
        public static Func<bool> PressingCard;

        static bool pressing, moored, nearPort, breadLow, breadGone, debtNear, debtDay;

        /// <summary>The last reason the clock dropped for (null: none since the game began).</summary>
        public static ClockReason? Last { get; private set; }
        /// <summary>A drop happened (the reason): for the log and the sound.</summary>
        public static event Action<ClockReason> Dropped;

        /// <summary>Drops the clock for a reason, if its switch is on and the clock runs faster than the reason's speed.</summary>
        public static bool Raise(ClockReason why)
        {
            if (!ClockSettings.On(why) || !GameSpeed.DropTo(ClockSettings.Level(why), ClockSettings.Says(why)))
                return false;
            Last = why;
            Dropped?.Invoke(why);
            return true;
        }

        /// <summary>Does a pressing matter wait for an answer now?</summary>
        public static bool Pressing()
        {
            switch (SeaHolds.Now)
            {
                case SeaHold.GuardBoat:
                case SeaHold.PortSearch:
                case SeaHold.RaidersComing:
                case SeaHold.RaidersChasing:
                case SeaHold.RaidersAlongside:
                    return true;
            }
            if (Aground.Stage != AgroundStage.None)
                return true;
            return PressingCard != null && PressingCard();
        }

        /// <summary>After each step of the clock: what became true in this minute.</summary>
        public static void Step(long minute)
        {
            var game = KapmanGame.Current;
            if (game == null)
                return;
            Look(game, out bool press, out bool lying, out bool near, out double bread, out double debtLeft);

            // a pressing matter: the clock drops, and may not run fast while it waits (its hours are short)
            GameSpeed.Cap = press && ClockSettings.On(ClockReason.Pressing) ? PressingCap : GameSpeed.Count - 1;
            if (press && !pressing)
                Raise(ClockReason.Pressing);
            pressing = press;

            if (lying && !moored)
                Raise(ClockReason.Moored);
            moored = lying;
            if (near && !nearPort)
                Raise(ClockReason.NearPort);
            nearPort = near;

            if (bread < BreadGoneDays)
            {
                if (!breadGone)
                    Raise(ClockReason.BreadGone);
                breadGone = breadLow = true;
            }
            else if (bread < BreadLowDays)
            {
                if (!breadLow)
                    Raise(ClockReason.BreadLow);
                breadLow = true;
                breadGone = false;
            }
            else if (bread >= BreadArmDays)
                breadLow = breadGone = false;

            if (debtLeft <= DebtDayDays)
            {
                if (!debtDay)
                    Raise(ClockReason.DebtDay);
                debtDay = debtNear = true;
            }
            else if (debtLeft <= DebtNearDays)
            {
                if (!debtNear)
                    Raise(ClockReason.DebtNear);
                debtNear = true;
            }
            else
                debtNear = debtDay = false;
        }

        /// <summary>A game was loaded or begun: every reason stands as it is now, and none is told for it.</summary>
        public static void Rearm()
        {
            var game = KapmanGame.Current;
            pressing = moored = nearPort = breadLow = breadGone = debtNear = debtDay = false;
            GameSpeed.Cap = GameSpeed.Count - 1;
            if (game == null)
                return;
            Look(game, out pressing, out moored, out nearPort, out double bread, out double debtLeft);
            breadGone = bread < BreadGoneDays;
            breadLow = bread < BreadLowDays;
            debtDay = debtLeft <= DebtDayDays;
            debtNear = debtLeft <= DebtNearDays;
        }

        static void Look(KapmanGame game, out bool press, out bool lying, out bool near, out double bread, out double debtLeft)
        {
            press = Pressing();
            lying = game.ship != null && game.ship.anchored && Port.LyingId() != null;
            near = false;
            if (!lying && game.ship != null && Course.IsSet && Course.Points.Count > 0)
                near = WorldXZ.Distance(ChartWorld.ShipWorld(), Course.Points[Course.Points.Count - 1]) < NearPortM;
            bread = Voyage.Started ? Voyage.FoodDays : double.PositiveInfinity;
            // (when fixes/debt_day.md is in: Trade.DebtDaysLeft(game.Day), which is whole days to the day itself)
            debtLeft = Trade.Started && Trade.Debt > 0.01 ? Trade.DebtDue - game.Day : double.PositiveInfinity;
        }

        static void MadeOut(SimShip s)
        {
            if (s != null && GameSpeed.Stepped)
                Raise(s.raider ? ClockReason.Raiders : ClockReason.Sail);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            pressing = moored = nearPort = breadLow = breadGone = debtNear = debtDay = false;
            PressingCard = null;
            Last = null;
            Dropped = null;
            SeaWatch.MadeOut -= MadeOut;
            SeaWatch.MadeOut += MadeOut;
        }
    }
}
```

### 5.2 The edits

#### `Assets/Kapman/Code/Runtime/Levels/LevelClock.cs`

Edit 1, at line 69. Find:

```csharp
        public static bool Paused;
```

Put in its place:

```csharp
        public static bool Paused;
        /// <summary>The game moves the hours itself, a game minute at a time (Kapman.Game.KapmanGame.RunClock, on the
        /// chart): the clock does not add the frame's time, it only turns the sky for the hour it is given.</summary>
        public static bool External;
```

Edit 2, at line 160. Find:

```csharp
                if (!Paused)
                    GameHours += Time.deltaTime * rate * Speed / 3600.0;
```

Put in its place:

```csharp
                if (!Paused && !External)
                    GameHours += Time.deltaTime * rate * Speed / 3600.0;
```


#### `Assets/Kapman/Code/Game/Chart/ChartBody.cs`

Edit 1, at line 68. Find:

```csharp
            if (!motion.Driven)
                motion.Drive(true);
            var clock = LevelClock.Current;
            float rate = clock != null ? clock.rate : 60f;
            float scale = (float)FloatingOrigin.HorizontalScale;
            // game seconds this frame over real metres a game metre: real m/s -> game metres this frame
            float step = LevelClock.Paused ? 0f : Time.deltaTime * rate * LevelClock.Speed / Mathf.Max(0.001f, scale);

            Vector3 pos = transform.position;
```

Put in its place:

```csharp
            if (!motion.Driven)
                motion.Drive(true);
            if (GameSpeed.Stepped)
                return;                                               // (the game's clock steps her: KapmanGame.StepMinute calls Step)
            var clock = LevelClock.Current;
            float rate = clock != null ? clock.rate : 60f;
            Move(LevelClock.Paused ? 0f : Time.deltaTime * rate * LevelClock.Speed, Time.deltaTime * LevelClock.TimeFactor);
        }

        /// <summary>Where she was when the last step began, and her heading then: her mark is drawn between that and
        /// where she is, by KapmanGame.StepFraction, so she glides at speed 1 though she is moved once a second.</summary>
        public Vector3 From { get; private set; }
        public float FromHeading { get; private set; }

        /// <summary>One step of the game's clock: she sails this many game seconds, by the order she has.</summary>
        public void Step(double gameSeconds)
        {
            if (!motion.Driven)
                motion.Drive(true);
            Move((float)gameSeconds, (float)(gameSeconds / 60.0));
        }

        /// <summary>She sails `gameSeconds`. `helm` is the same span in real seconds at speed 1 (a game minute is one):
        /// her turning and her gathering way are counted in it, as they always were.</summary>
        void Move(float gameSeconds, float helm)
        {
            float scale = (float)FloatingOrigin.HorizontalScale;
            // game seconds over real metres a game metre: real m/s -> game metres this step
            float step = gameSeconds / Mathf.Max(0.001f, scale);

            Vector3 pos = transform.position;
            From = pos;
            FromHeading = motion.heading;
```

Edit 2, at line 101. Find:

```csharp
                    heading += Mathf.Clamp(delta, -turnRate * Time.deltaTime * LevelClock.TimeFactor, turnRate * Time.deltaTime * LevelClock.TimeFactor);
```

Put in its place:

```csharp
                    heading += Mathf.Clamp(delta, -turnRate * helm, turnRate * helm);
```

Edit 3, at line 113. Find:

```csharp
            speed = Mathf.MoveTowards(speed, target, 0.6f * Time.deltaTime * LevelClock.TimeFactor);
```

Put in its place:

```csharp
            speed = Mathf.MoveTowards(speed, target, 0.6f * helm);
```

Edit 4, at line 119. Find:

```csharp
            float perSecond = rate / Mathf.Max(0.001f, scale);
```

Put in its place:

```csharp
            float perSecond = 60f / Mathf.Max(0.001f, scale);
```


#### `Assets/Kapman/Code/Game/KapmanGame.cs`

Edit 1, at line 484. Find:

```csharp
        public void ResyncHours() => lastHours = clock != null ? clock.GameHours : -1.0;
```

Put in its place:

```csharp
        public void ResyncHours()
        {
            lastHours = clock != null ? clock.GameHours : -1.0;
            resync = true;                                               // (the stepped clock takes the hour up at its next frame)
        }
```

Edit 2, at line 504. Find:

```csharp
            PanelClock.Tick();                                           // (the clock waits while a panel is open)
            GameSpeed.Tick();                                            // ([Space] pause, [+] [-] 1x 2x 4x)
            Alerts.Tick();
            Rest.Tick(clock);                                            // ([Z] at night: rest till dawn)
            GameEvents.Look();
            if (clock == null)
                return;
```

Put in its place:

```csharp
            PanelClock.Tick();                                           // (the clock waits while a card wants an answer)
            GameSpeed.Stepped = ChartWorld.Current != null;              // (the chart: the game steps its own clock, below)
            GameSpeed.Tick();                                            // ([Space] pause, [+] [-] the five speeds)
            Alerts.Tick();
            Rest.Tick(clock);                                            // ([Z] at night: rest till dawn)
            GameEvents.Look();
            if (clock == null)
                return;
            if (GameSpeed.Stepped)
            {
                if (!ClockByHand)
                    RunClock(Time.unscaledDeltaTime);                    // (ruling 26: whole game minutes, the same at every speed)
                return;
            }
```

Edit 3, at line 524. Find:

```csharp
        // play mode starts without a domain reload: a new game each play
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            world = null;
            lastHours = -1.0;
```

Put in its place:

```csharp
        // ------------------------------------------------------------------ the stepped clock (the chart; ruling 26)

        static readonly Kapman.Game.Sim.ClockStepper stepper = new Kapman.Game.Sim.ClockStepper();
        static int settle, lastSign, restless;
        static bool restlessTold;
        static bool resync = true;

        /// <summary>Frames the clock stands after a step in which something happened that the frame, a port's graph or
        /// a talk must see before time goes on (they look once a frame, not once a step).</summary>
        public const int SettleFrames = 2;
        /// <summary>The real milliseconds one frame may spend in steps; what is left waits for the next frame (the clock
        /// then runs slower than its speed says; nothing is skipped).</summary>
        public static double StepBudgetMs = 8.0;
        /// <summary>A test runs the clock itself (RunClock with its own seconds): Update leaves it alone.</summary>
        public static bool ClockByHand;
        /// <summary>The steps the last frame ran, and the real milliseconds they took.</summary>
        public static int StepsLastFrame { get; private set; }
        public static double StepMsLastFrame { get; private set; }
        /// <summary>Game seconds of the step being run now (0 outside a step).</summary>
        public static double StepSeconds { get; private set; }
        /// <summary>How far the clock is into the step to come, 0..1 (her mark is drawn between two steps).</summary>
        public static double StepFraction => stepper.Fraction;
        /// <summary>A step was run (the game minute it ended on), after everything of the game has had it.</summary>
        public static event Action<long> Stepped;

        /// <summary>
        /// One frame of the stepped clock: `realSeconds` of the wall clock at the game's speed, as whole game minutes.
        /// Each step is the same at every speed (<see cref="StepMinute"/>). The frame's steps end early when a step
        /// changed something the per-frame side of the game must answer (<see cref="Sign"/>): the clock then stands
        /// <see cref="SettleFrames"/> frames, so a card is up and holding it before another minute passes. Near a port
        /// and in a meeting at sea a frame runs one step at most (<see cref="OneAFrame"/>). A test calls this itself
        /// with its own seconds.
        /// </summary>
        public void RunClock(double realSeconds)
        {
            StepsLastFrame = 0;
            StepMsLastFrame = 0.0;
            if (clock == null)
                return;
            double hours = clock.GameHours;
            ladder ??= new Kapman.Game.Sim.TickLadder(clock.year, clock.startDay);
            if (lastHours < 0.0)
            {
                lastHours = hours;                                       // (the first frame: nothing has passed yet)
                resync = true;
            }
            else if (Math.Abs(hours - lastHours) > 1e-9)
            {
                // the clock was moved from outside (a dev key, a test): the ladder replays what passed, as it always did
                ladder.Advance(lastHours, hours, false, ticks);
                lastHours = hours;
                resync = true;
            }
            if (resync)
            {
                resync = false;
                stepper.SetTo(hours);
                ClockDrops.Rearm();                                      // (what is so now is not news)
                lastSign = Sign();
                settle = 0;
            }
            int sign = Sign();
            if (sign != lastSign)
            {
                lastSign = sign;                                         // (the frame's own side did something: let it settle)
                settle = SettleFrames;
                // (a guard: if something in the sign changed every frame the clock would never run. After thirty such
                // frames in a row it runs anyway, and says so once)
                if (++restless > 30)
                {
                    settle = 0;
                    if (!restlessTold)
                        Debug.LogWarning("Kapman clock: something the clock watches changes every frame (KapmanGame.Sign): the clock runs on regardless.");
                    restlessTold = true;
                }
            }
            else
                restless = 0;
            if (settle > 0)
            {
                settle--;
                stepper.Stop();
                return;
            }
            int want = stepper.Take(realSeconds, GameSpeed.Rate);
            bool one = want > 1 && OneAFrame();
            if (one)
                want = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < want; i++)
            {
                StepMinute();
                StepsLastFrame++;
                int now = Sign();
                if (now != lastSign || GameSpeed.Paused)
                {
                    lastSign = now;
                    settle = SettleFrames;
                    stepper.Stop();
                    break;
                }
                if (watch.Elapsed.TotalMilliseconds > StepBudgetMs)
                    break;                                               // (the rest of this frame's minutes wait for the next)
            }
            if (one)
                stepper.Stop();
            StepMsLastFrame = watch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// ONE GAME MINUTE, always in this order: her orders; she sails the minute; the minute is over (the tick ladder:
        /// the minute, then on the hour the world's ships and the cards' draw, then on the day the markets, the bread and
        /// the pay); the lookout; the mooring watch and an open card's waiting time; a wait for light or wind; the fog of
        /// war; and last the reasons for the clock to drop back.
        /// </summary>
        void StepMinute()
        {
            const double seconds = Kapman.Game.Sim.ClockStepper.StepSeconds;
            StepSeconds = seconds;
            long minute = stepper.Step();
            double to = minute / 60.0;
            VoyagePlanner.Current?.Step(seconds);
            ChartBody.Current?.Step(seconds);
            clock.GameHours = to;
            ladder.Advance(lastHours, to, false, ticks);
            lastHours = to;
            Kapman.Game.Sim.SeaWatch.Current?.Step(minute);
            EncounterRunner.Instance?.Step(seconds);
            Kapman.Game.Frame.FrameWait.Step(minute);
            if (minute % 30 == 0 && ship != null && Exploration.Ready)
            {
                var w = Kapman.World.FloatingOrigin.ToWorld(ship.transform.position);
                Exploration.Reveal(w.x, w.z, Exploration.Sight);
            }
            ClockDrops.Step(minute);
            StepSeconds = 0.0;
            Stepped?.Invoke(minute);
        }

        /// <summary>A number that changes when something happens that the per-frame side of the game answers: a card in
        /// line or open or answered, a talk, a happening, she is laid to or let go, a meeting's stage, a wait, her
        /// course, the speed, the port she is at. Nothing in it changes merely because time passes.</summary>
        int Sign()
        {
            unchecked
            {
                int h = 17;
                var d = EncounterRunner.Director;
                h = h * 31 + (d == null ? 0 : (d.Current != null ? (d.Current.Chosen ? 2 : 1) : 0) + (d.Waiting ? 4 : 0) + (d.InCall ? 8 : 0));
                var text = TextPanel.Current;
                h = h * 31 + (text != null && text.Open ? 1 + (text.Key ?? "").GetHashCode() : 0);
                h = h * 31 + (GameEvents.Current != null ? 1 : 0);
                h = h * 31 + (ship != null && ship.anchored ? 1 : 0);
                h = h * 31 + (int)Kapman.Game.Sim.SeaHolds.Now;
                h = h * 31 + (int)Aground.Stage;
                h = h * 31 + (Rest.On ? 1 : 0) + (TideWait.On ? 2 : 0) + (Kapman.Game.Frame.FrameWait.On ? 4 : 0);
                h = h * 31 + (Course.IsSet ? 1 + (Course.Destination ?? "").GetHashCode() : 0);
                h = h * 31 + GameSpeed.Level + (GameSpeed.Paused ? 8 : 0) + GameSpeed.Cap * 16;
                var port = ship != null ? Port.Nearest(ShipPosition) : null;
                h = h * 31 + (port != null && port.DistanceTo(ShipPosition) < port.arrivalRadius ? 1 + (port.id ?? "").GetHashCode() : 0);
                return h;
            }
        }

        /// <summary>Where the per-frame side of the game watches something that changes smoothly (the port's graph: is
        /// she near and slow enough to lay to; a talk that counts minutes; a chase), the clock runs one step a frame at
        /// most, so those looks come between every two minutes as they do at speed 1.</summary>
        bool OneAFrame()
        {
            if (Kapman.Game.Sim.SeaHolds.Now != Kapman.Game.Sim.SeaHold.None || Aground.Stage != AgroundStage.None || Rest.On)
                return true;
            var text = TextPanel.Current;
            if (text != null && text.Open && text.Key != TextPanel.PortMenuKey)
                return true;
            if (ship == null || ship.anchored)
                return false;
            var port = Port.Nearest(ShipPosition);
            return port != null && port.DistanceTo(ShipPosition) < port.arrivalRadius * 2f;
        }

        // play mode starts without a domain reload: a new game each play
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            settle = lastSign = restless = 0;
            restlessTold = false;
            ClockByHand = false;
            resync = true;
            StepsLastFrame = 0;
            StepMsLastFrame = StepSeconds = 0.0;
            Stepped = null;
            world = null;
            lastHours = -1.0;
```


#### `Assets/Kapman/Code/Game/Voyage/VoyagePlanner.cs`

Edit 1, at line 87. Find:

```csharp
        void Update()
        {
            var game = KapmanGame.Current;
            ship = game != null ? game.ship : null;
            if (ship == null)
                return;
```

Put in its place:

```csharp
        /// <summary>The planner at work (the game's clock steps it: KapmanGame.StepMinute).</summary>
        public static VoyagePlanner Current { get; private set; }
        double stepHours;                          // game hours of the step (or the frame) being run

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            // the stepped clock (the chart) runs the planner once a game minute, from its own step. While that clock
            // stands (paused, a card) the planner still looks each frame, with no time passing, as it always did
            if (GameSpeed.Stepped && !GameSpeed.Paused)
                return;
            var clock = LevelClock.Current;
            stepHours = GameSpeed.Stepped || clock == null || LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 3600.0;
            Run();
        }

        /// <summary>One step of the game's clock: her orders for the minute to come.</summary>
        public void Step(double gameSeconds)
        {
            stepHours = gameSeconds / 3600.0;
            Run();
        }

        void Run()
        {
            var game = KapmanGame.Current;
            ship = game != null ? game.ship : null;
            if (ship == null)
                return;
```

Edit 2, at line 147. Find:

```csharp
                double hours = LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 3600.0;
```

Put in its place:

```csharp
                double hours = stepHours;
```


#### `Assets/Kapman/Code/Game/Sim/SeaWatch.cs`

Edit 1, at line 40. Find:

```csharp
        void Update()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            if (world == null || game.ship == null || game.Hours < nextLook)
                return;
            nextLook = game.Hours + everyGameMinutes / 60.0;
```

Put in its place:

```csharp
        void Update()
        {
            if (GameSpeed.Stepped)
                return;                                                 // (the game's clock steps the lookout: Step)
            var game = KapmanGame.Current;
            if (game == null || game.Hours < nextLook)
                return;
            nextLook = game.Hours + everyGameMinutes / 60.0;
            Look();
        }

        /// <summary>One step of the game's clock (the game minute it ended on): the lookout looks round every
        /// <see cref="everyGameMinutes"/>, on the same minutes at every speed.</summary>
        public void Step(long minute)
        {
            if (minute % Math.Max(1L, (long)Math.Round(everyGameMinutes)) == 0)
                Look();
        }

        void Look()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            if (world == null || game.ship == null)
                return;
```


#### `Assets/Kapman/Code/Game/Encounters/EncounterRunner.cs`

Edit 1, at line 147. Find:

```csharp
            if (ship != null && chart)
            {
                // the chart: she has moored when the port's graph has laid her to (the clock is held from then, so her
                // speed never falls and game seconds never pass: lying still is "anchored", the dwell real seconds)
                var p = ship.transform.position;
                bool lying = game != null && game.ship != null && game.ship.anchored;
                // (under way: on a charted course, or ordered somewhere else than where she is; an order to where she
                // already is is how she is stopped, and the next point of a river's course is often only metres off)
                bool sailing = !lying && (Course.IsSet || (ship.Goal.HasValue && (ship.Goal.Value - p).sqrMagnitude > 36f));
                HomeCall(d, lying);
                // (real seconds by the wall clock: the game's own seconds stand still while the port's menu holds it)
                float now = Time.realtimeSinceStartup;
                float dt = lastLook > 0f ? Mathf.Clamp(now - lastLook, 0f, 0.25f) : 0f;
                lastLook = now;
                Autosaved(d.Look(p.x, p.z, sailing, sailing ? ship.Speed : 0.0, dt));
            }
```

Put in its place:

```csharp
            if (ship != null && chart)
            {
                // (real seconds by the wall clock: the game's own seconds stand still while the clock is held)
                float now = Time.realtimeSinceStartup;
                float dt = lastLook > 0f ? Mathf.Clamp(now - lastLook, 0f, 0.25f) : 0f;
                lastLook = now;
                // the stepped clock looks from its own step, in game seconds (Step, below); here only while it stands
                if (!GameSpeed.Stepped || GameSpeed.Paused || !homeLooked)
                    ChartLook(d, game, ship, dt);
            }
```

Edit 2, at line 176. Find:

```csharp
        /// <summary>She has moored: the game saves itself (held if something is in the way). She has cast off: a save
```

Put in its place:

```csharp
        /// <summary>The chart: she has moored when the port's graph has laid her to (lying still is "anchored"); the
        /// dwell is `dt`: real seconds while the clock stands, game seconds from the clock's step.</summary>
        void ChartLook(EncounterDirector d, KapmanGame game, ShipMotion ship, double dt)
        {
            var p = ship.transform.position;
            bool lying = game != null && game.ship != null && game.ship.anchored;
            // (under way: on a charted course, or ordered somewhere else than where she is; an order to where she
            // already is is how she is stopped, and the next point of a river's course is often only metres off)
            bool sailing = !lying && (Course.IsSet || (ship.Goal.HasValue && (ship.Goal.Value - p).sqrMagnitude > 36f));
            HomeCall(d, lying);
            Autosaved(d.Look(p.x, p.z, sailing, sailing ? ship.Speed : 0.0, dt));
        }

        /// <summary>One step of the game's clock on the chart: the mooring watch, in game seconds, so a call begins in
        /// the same game minute at every speed.</summary>
        public void Step(double gameSeconds)
        {
            var d = Director;
            var game = KapmanGame.Current;
            var ship = ShipMotion.Player;
            if (Instance != this || d == null || game == null || ship == null || !OnChart)
                return;
            d.SetTime(game.Day, game.Year, game.Month, (long)Math.Floor(game.Hours));
            ChartLook(d, game, ship, gameSeconds);
            // (fixes/card_clock.md section 3: the open card's waiting time is counted HERE, d.Wait(gameSeconds / 3600.0),
            // not in Update: then a card takes its own option in the same game minute at every speed)
        }

        /// <summary>She has moored: the game saves itself (held if something is in the way). She has cast off: a save
```


#### `Assets/Kapman/Code/Game/Voyage/Aground.cs`

Edit 1, at line 49. Find:

```csharp
            LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 60.0;
```

Put in its place:

```csharp
            GameSpeed.Stepped ? KapmanGame.StepSeconds / 60.0 : LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 60.0;
```


#### `Assets/Kapman/Code/Game/Sim/RaiderEncounter.cs`

Edit 1, at line 35. Find:

```csharp
        static float nextLook;
```

Put in its place:

```csharp
        static double nextLook;
        /// <summary>What the looks are spaced by: game minutes where the game steps its clock (the chart), else real seconds.</summary>
        static double LookClock => GameSpeed.Stepped && KapmanGame.Current != null ? KapmanGame.Current.Hours * 60.0 : UnityEngine.Time.unscaledTime;
```

Edit 2, at line 42. Find:

```csharp
            if (w == null || w.raiders == null || Stage != RaidStage.None || SeaWatch.Current == null || UnityEngine.Time.unscaledTime < nextLook)
                return;
            nextLook = UnityEngine.Time.unscaledTime + 0.5f;
```

Put in its place:

```csharp
            if (nextLook - LookClock > 10.0)
                nextLook = 0.0;                                              // (the clock went back: a loaded game)
            if (w == null || w.raiders == null || Stage != RaidStage.None || SeaWatch.Current == null || LookClock < nextLook)
                return;
            nextLook = LookClock + 0.5;
```

Edit 3, at line 57. Find:

```csharp
                    nextLook = UnityEngine.Time.unscaledTime + 5f;           // (the graph answers by calling Threaten)
```

Put in its place:

```csharp
                    nextLook = LookClock + 5.0;                              // (the graph answers by calling Threaten)
```


#### `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.Rack.cs`

Edit 1, at line 47. Find:

```csharp
            if (resume.Length > 0 && FrameGame.PortHere == null && !Course.IsSet)
                Course.SetTo(resume, ChartWorld.ShipWorld(), null);
            resume = "";
            if (!string.IsNullOrEmpty(say))
                FrameLog.Add(say);
        }
```

Put in its place:

```csharp
            if (resume.Length > 0 && FrameGame.PortHere == null && !Course.IsSet)
                Course.SetTo(resume, ChartWorld.ShipWorld(), null);
            resume = "";
            if (!string.IsNullOrEmpty(say))
                FrameLog.Add(say);
        }

        /// <summary>What she waited for has come: the wait ends, and the clock drops back (the player set it fast to
        /// wait, and would not want the days to go on running in port).</summary>
        static void Over(string say)
        {
            End(say);
            ClockDrops.Raise(ClockReason.WaitOver);
        }
```

Edit 2, at line 55. Find:

```csharp
        public static bool Dark => LevelClock.SunAltitude < -6f;

        /// <summary>The port her course (set, or laid by while she waits) is for; null with none.</summary>
```

Put in its place:

```csharp
        public static bool Dark => GameSpeed.Stepped ? SimStage.SunElevationAt(ChartWorld.ShipWorld()) < -6.0 : LevelClock.SunAltitude < -6f;

        /// <summary>The port her course (set, or laid by while she waits) is for; null with none.</summary>
```

Edit 3, at line 73. Find:

```csharp
        public static void Tick()
        {
            if (!On || Time.realtimeSinceStartup < nextLook)
                return;
            nextLook = Time.realtimeSinceStartup + 0.5f;
            if (Until == For.FirstLight && !Dark)
                End("First light.");
            else if (Until == For.FairWind && WindFair())
                End("The wind has come fair.");
        }
```

Put in its place:

```csharp
        public static void Tick()
        {
            if (GameSpeed.Stepped)
                return;                                                   // (the game's clock steps the wait: Step)
            if (!On || Time.realtimeSinceStartup < nextLook)
                return;
            nextLook = Time.realtimeSinceStartup + 0.5f;
            if (Until == For.FirstLight && !Dark)
                Over("First light.");
            else if (Until == For.FairWind && WindFair())
                Over("The wind has come fair.");
        }

        /// <summary>One step of the game's clock (the game minute it ended on): first light is looked for every minute,
        /// a fair wind every tenth (it plans her course to ask).</summary>
        public static void Step(long minute)
        {
            if (!On)
                return;
            if (Until == For.FirstLight)
            {
                if (!Dark)
                    Over("First light.");
            }
            else if (Until == For.FairWind && minute % 10 == 0 && WindFair())
                Over("The wind has come fair.");
        }
```


#### `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`

Edit 1, at line 203. Find:

```csharp
            for (int i = 0; i < GameSpeed.Speeds.Length; i++)
                topRight.Add(Speed(GameSpeed.Speeds[i] + "x", i, i == 0 ? "A game minute a second  [-]" : GameSpeed.Speeds[i] + " times as fast  [+]"));
```

Put in its place:

```csharp
            for (int i = 0; i < GameSpeed.Count; i++)
                topRight.Add(Speed((i + 1).ToString(), i, GameSpeed.Tips[i] + (i == 0 ? "  [-]" : "  [+]")));
```

Edit 2, at line 705. Find:

```csharp
                part.text = FrameUi.PartOfDay(clock.HourOfDay);
            }
            if (Trade.Started)
```

Put in its place:

```csharp
                // (at speed 5 a part of the day lasts under a second: the date alone is told)
                part.text = !GameSpeed.Paused && GameSpeed.Running >= 4 ? "" : FrameUi.PartOfDay(clock.HourOfDay);
            }
            if (Trade.Started)
```

Edit 3, at line 723. Find:

```csharp
                b.EnableInClassList("on", level < 0 ? GameSpeed.PlayerPaused : !GameSpeed.PlayerPaused && GameSpeed.Level == level);
            }
            state.text = GameSpeed.Paused ? (GameSpeed.PlayerPaused ? "paused" : PanelClock.Holding ? "waits for your answer" : "held") : "";
            state.tooltip = PanelClock.Holding && !GameSpeed.PlayerPaused ? "A card holds the clock. Answer it, or press Space or a speed to let the clock run while it waits." : "";
```

Put in its place:

```csharp
                b.EnableInClassList("on", level < 0 ? GameSpeed.PlayerPaused : !GameSpeed.PlayerPaused && GameSpeed.Level == level);
                b.EnableInClassList("was", level >= 0 && level == GameSpeed.Before);   // (the speed she ran at before the clock dropped back)
                b.EnableInClassList("capped", level > GameSpeed.Cap);                  // (a pressing matter: no faster than the cap for now)
            }
            bool dropped = !GameSpeed.Paused && GameSpeed.Before > GameSpeed.Level;
            state.text = GameSpeed.Paused ? (GameSpeed.PlayerPaused ? "paused" : PanelClock.Holding ? "waits for your answer" : "held")
                       : dropped ? "slowed: " + (GameSpeed.DroppedFor.Length > 0 ? GameSpeed.DroppedFor + ".  " : "") + "[+] back to " + (GameSpeed.Before + 1)
                       : GameSpeed.Level > GameSpeed.Cap ? "no faster than " + (GameSpeed.Cap + 1) + " until it is answered" : "";
            state.tooltip = PanelClock.Holding && !GameSpeed.PlayerPaused ? "A card holds the clock. Answer it, or press Space or a speed to let the clock run while it waits."
                          : dropped ? "The clock slowed by itself. Press + to go back to the speed you had. Each reason has a switch in the settings." : "";
```


#### `Assets/Kapman/Code/Game/Map/MapTable.cs`

Edit 1, at line 43. Find:

```csharp
            for (int i = 0; i < GameSpeed.Speeds.Length; i++)
                speed.Add(SpeedButton($"{GameSpeed.Speeds[i]}x", i, i == 0 ? "A game minute a second [-]" : $"{GameSpeed.Speeds[i]} times as fast: the ship and the clock together [+]"));
```

Put in its place:

```csharp
            for (int i = 0; i < GameSpeed.Count; i++)
                speed.Add(SpeedButton((i + 1).ToString(), i, GameSpeed.Tips[i] + (i == 0 ? " [-]" : " [+]")));
```


#### `Assets/Kapman/Code/Graphs/TextNodes.cs`

Edit 1, at line 132. Find:

```csharp
                left -= TextPanel.WaitsByGameClock ? UnityEngine.Time.deltaTime : UnityEngine.Time.unscaledDeltaTime;
```

Put in its place:

```csharp
                // (the stepped clock: a "second" of this wait is a game minute, and the frame has run so many of them)
                left -= !TextPanel.WaitsByGameClock ? UnityEngine.Time.unscaledDeltaTime
                      : GameSpeed.Stepped ? KapmanGame.StepsLastFrame : UnityEngine.Time.deltaTime;
```

### 5.3 Small things not in the compiled draft (one or two lines each)

| What | Where | The line |
|---|---|---|
| A talk drops the clock | `Game/UI/Frame/PersonTalk.cs`, just before `frame.OpenFloat(greeting);` (line 159) | `ClockDrops.Raise(ClockReason.Talk);` |
| The chest drops the clock | where the chest is opened (`MapView.ChestOpen` reads `table.ChestOpen`, MapView.cs 64; I did not find the opening call) | `ClockDrops.Raise(ClockReason.Talk);` |
| The tide has made | `Game/Voyage/TideWait.cs` 212 and 217, after each `End(clock, "...")` that says she goes on | `ClockDrops.Raise(ClockReason.WaitOver);` |
| Dawn after a rest | `Game/Voyage/Rest.cs` 62, after `End(clock, "Dawn.");` | `ClockDrops.Raise(ClockReason.WaitOver);` |
| A pressing card | where the card clock is wired (fixes/card_clock.md section 3, `EnsureDirector`) | `ClockDrops.PressingCard = () => Director != null && Director.Current != null && !Director.Current.Chosen && Director.RuleNow.pressing;` |
| The card's waiting time | `EncounterRunner.Step`, at the comment the draft leaves there | `d.Wait(gameSeconds / 3600.0);` and take the call out of `Update` |
| The card hold's switch kept | `FrameRoot.OnEnable`, beside `PanelClock.OnlyCards = true;` (line 103) | `PanelClock.On = PlayerPrefs.GetInt("kapman.clock.hold.card", 1) != 0;` |
| The switches on screen | the pause menu (`MainMenu.cs`: there is no settings page) | one row for each `ClockReason`: `ClockSettings.Label`, `ClockSettings.On`, `ClockSettings.Set` |
| The two styles | `Assets/Kapman/Game/UI/Frame.uss` after line 568 | section 4.4 |
| Her mark drawn between steps | `Game/Map/MapView.cs` 1330, `var w = MapWorld.ToWorld(game.ship.transform.position);` | take the place from `var body = ChartBody.Current;` when `GameSpeed.Stepped && body != null`: `Vector3.Lerp(body.From, game.ship.transform.position, (float)KapmanGame.StepFraction)`. This draws her up to one game minute behind where she is; only the mark, nothing reads it. Leave it out if she looks right without it. |
| The dead rate switches | `Rest.cs` 90 and 98, `TideWait.cs` (its `Begin` and `End`), `Aground.cs` 274-275 and 354, `FrameRoot.cs` 104-105 and 139-140, `KapmanGame.FastForwardKeys` | They set `clock.rate`, which the stepped clock does not read. Harmless on the chart; they still work in a 3D level. Take them out when the 3D levels are next touched, not now. |

### 5.4 Tests that must change with it

- `Tests/PlayMode/ChartGameTests.cs` no longer compiles: `GameSpeed.Speeds` is gone (checked: exactly eight
  errors, at lines 577, 653, 772, 957, 1228, 1355, 1364, 1464 as the file stood). Each is the test's hurry,
  `GameSpeed.Speeds[2] = 40;` or its undoing `= 4;`. Delete all eight. The hurry is now a click on speed 4, which
  is 96 times and faster than the 40 it had.
- The same file clicks the buttons by their words: `Small(play, "4x")` at lines 578, 958 and 1356, `Small(play,
  "1x")` at 652 and 1363. They become `Small(play, "4")` and `Small(play, "1")`. Line 579 then expects
  `GameSpeed.Level` 3, not 2.
- Wherever those tests wait for her to arrive, the clock now drops back by itself near the port and when she is
  made fast. A test in a hurry can switch the drops off at its start: `foreach (ClockReason r in
  Enum.GetValues(typeof(ClockReason))) ClockSettings.Set(r, false);` and must put them back (they are PlayerPrefs:
  the player's own).
- `Tests/EditMode/AlertTests.cs` compiles and its numbers still hold (not run): off the chart level 2 is still a
  time scale of 4, because nothing there runs faster than four times.
- `Tests/EditMode/FrameFloatTests.cs` compiles (not run).

## 6. The tests

### 6.1 The clock's step, with no scene: `Assets/Kapman/Tests/EditMode/ClockStepperTests.cs` (new)

A small game on the step: a boat that sails each minute, a world whose hour reads where the boat is, cards that
come on seeded hours and hold the clock until answered some frames later, things that drop the speed. It is run
frame by frame the way `KapmanGame.RunClock` runs the real one.

**This one was run** (outside Unity, with the .NET that ships inside Unity, on the same two source files and the
project's own `TickLadder.cs`). Twenty game days, seed 793:

```
speed 1: 1729184 frames, most steps a frame 1, hash 88040D0EEC8779A7, cards 13, drops 7
speed 5: 2929 frames, most steps a frame 36, hash 88040D0EEC8779A7, cards 13, drops 7
THE SAME WORLD
speed 2: 63484 frames, most 1: the same
speed 3: 10600 frames, most 3: the same
speed 4: 2675 frames, most 10: the same
speed 5: 733 frames, most 36: the same
another seed: another world
a ten second hitch at speed 5 wants 36 steps; a second one unrun, 40 (most 40)
three real minutes at speed 1, sixty frames a second: 180 game minutes
```

The speed 5 run had ragged frames (sixty, thirty and a hundred and forty-four a second, a half-second hitch, short
ones); the speed 1 run had even ones. Two of my own asserts were wrong on the first run and are corrected in the
file below (a hitch wants 36 steps, not 40; the mock world did not take the seed into its hash).

`Assets/Kapman/Tests/EditMode/ClockStepperTests.cs`

```csharp
using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
#if !STEPCHECK
using NUnit.Framework;
#endif

namespace Kapman.Tests
{
    /// <summary>
    /// A small game on the clock's step, with no scene: a boat that sails each minute, a world that does something each
    /// hour and reads where the boat is, cards that come on seeded hours and hold the clock until a player answers some
    /// frames later, and things that drop the speed. It is run frame by frame as KapmanGame.RunClock runs the real one
    /// (Take, Step, Stop), so the test can ask the one question of ruling 26: does the same seed end in the same world
    /// at speed 1 and at speed 5, whatever the frames were?
    /// </summary>
    public sealed class ClockRun : ITickSink
    {
        public static readonly double[] Rates = { 60.0, 240.0, 1440.0, 5760.0, 21600.0 };

        public readonly ClockStepper stepper = new ClockStepper();
        readonly TickLadder ladder = new TickLadder(793, 121);
        readonly int seed, wanted, answerFrames;

        /// <summary>The world, as one number: every tick and everything it did goes into it in order.</summary>
        public ulong hash = 14695981039346656037UL;
        /// <summary>The game minutes at which a card came (and held the clock), and at which the speed dropped.</summary>
        public readonly List<long> cards = new List<long>(), drops = new List<long>();
        public long until = long.MaxValue;
        public int frames, mostSteps;
        public double x, z;

        int level, hold;
        double lastHours;
        bool card, drop;

        public ClockRun(int seed, int level, int answerFrames)
        {
            this.seed = seed;
            this.level = wanted = level;
            this.answerFrames = answerFrames;
        }

        void Mix(long v)
        {
            unchecked
            {
                for (int b = 0; b < 8; b++)
                {
                    hash ^= (byte)(v >> (8 * b));
                    hash *= 1099511628211UL;
                }
            }
        }

        static uint Dice(int seed, long n)
        {
            unchecked
            {
                ulong v = (ulong)n * 6364136223846793005UL + (ulong)seed * 1442695040888963407UL;
                v ^= v >> 29;
                v *= 0xBF58476D1CE4E5B9UL;
                v ^= v >> 32;
                return (uint)v;
            }
        }

        public void Minute(long minute) => Mix(minute);

        public void Hour(long hour)
        {
            // the world's hour reads where the boat is (the world's ships keep clear of her): a boat moved by the frame
            // and not by the step would be somewhere else here at speed 5
            Mix(hour);
            Mix((long)Math.Round(x * 1000.0));
            Mix((long)Math.Round(z * 1000.0));
            if (Dice(seed, hour) % 37 == 0)
            {
                cards.Add(hour * 60);
                Mix(hour * 31 + 5);                               // (what the card did is in the world)
                card = true;
            }
            if (Dice(seed, hour + 7777) % 53 == 0)
            {
                drops.Add(hour * 60);
                drop = true;
            }
        }

        public void Day(int day) => Mix(day * 1000003L);
        public void Year(int year) => Mix(year * 7919L);
        public void Jumped(double fromHours, double toHours) => Mix(-1);
        public void Reversed(double fromHours, double toHours) => Mix(-2);

        /// <summary>One frame of `realSeconds`.</summary>
        public void Frame(double realSeconds)
        {
            frames++;
            if (hold > 0)
            {
                stepper.Take(realSeconds, 0.0);                   // (a card holds the clock: nothing runs, nothing is owed)
                if (--hold == 0)
                    level = wanted;                               // (answered: the player picks the speed again)
                return;
            }
            int want = stepper.Take(realSeconds, Rates[level]);
            int ran = 0;
            for (int i = 0; i < want && stepper.Minute < until; i++)
            {
                long minute = stepper.Step();
                ran++;
                double a = (minute % 360) * Math.PI / 180.0;      // she sails the minute (a heading that turns with it)
                x += Math.Sin(a) * 123.6;
                z += Math.Cos(a) * 123.6;
                double to = minute / 60.0;
                ladder.Advance(lastHours, to, false, this);
                lastHours = to;
                if (card)
                {
                    card = drop = false;
                    hold = answerFrames;
                    stepper.Stop();
                    break;
                }
                if (drop)
                {
                    drop = false;
                    level = 0;                                    // (the clock drops back to speed 1 by itself...)
                    hold = 2;                                     // (...and two frames on the player has it back)
                    stepper.Stop();
                    break;
                }
            }
            if (stepper.Minute >= until)
                stepper.Stop();
            mostSteps = Math.Max(mostSteps, ran);
        }

        /// <summary>Runs to a game minute with the frames a pattern gives, and tells how it ended.</summary>
        public static ClockRun To(long minute, int seed, int level, double[] frameSeconds, int answerFrames)
        {
            var run = new ClockRun(seed, level, answerFrames) { until = minute };
            for (int i = 0; run.stepper.Minute < minute; i++)
            {
                if (i > 400000000)
                    throw new InvalidOperationException("the clock does not get there");
                run.Frame(frameSeconds[i % frameSeconds.Length]);
            }
            return run;
        }

        /// <summary>Frames as a real machine gives them: sixty a second, thirty, a hundred and forty-four, a half-second
        /// hitch, a few short ones.</summary>
        public static readonly double[] Ragged = { 1.0 / 60.0, 1.0 / 30.0, 1.0 / 144.0, 0.5, 1.0 / 60.0, 0.004, 0.05, 1.0 / 60.0, 0.011 };
        public static readonly double[] Even = { 1.0 / 60.0 };
    }

#if !STEPCHECK
    /// <summary>Ruling 26 of 7 October 2026: the clock's step. Nothing may be skipped or lost at speed 5 that would have
    /// happened at speed 1.</summary>
    public class ClockStepperTests
    {
        const long TwentyDays = 20L * 24 * 60;

        [Test]
        public void TheSameSeedAtSpeedOneAndAtSpeedFiveEndsTheSame()
        {
            var slow = ClockRun.To(TwentyDays, 793, 0, ClockRun.Even, 90);
            var fast = ClockRun.To(TwentyDays, 793, 4, ClockRun.Ragged, 7);
            Assert.AreEqual(slow.stepper.Minute, fast.stepper.Minute, "the minute they ended on");
            Assert.AreEqual(slow.hash, fast.hash, "the world");
            Assert.AreEqual(slow.x, fast.x, "her boat, east");
            Assert.AreEqual(slow.z, fast.z, "her boat, north");
            CollectionAssert.AreEqual(slow.cards, fast.cards, "the minutes the cards came on");
            CollectionAssert.AreEqual(slow.drops, fast.drops, "the minutes the clock dropped on");
            Assert.Greater(slow.cards.Count, 5, "the test met some cards");
            Assert.LessOrEqual(slow.mostSteps, 1, "speed 1 at sixty frames a second never runs two steps in a frame");
            Assert.Greater(fast.mostSteps, 5, "speed 5 runs several steps a frame");
            Assert.Less(fast.frames, slow.frames / 50, "and gets there in far fewer frames");
        }

        [Test]
        public void EverySpeedEndsTheSame()
        {
            var first = ClockRun.To(TwentyDays / 4, 805, 0, ClockRun.Even, 30);
            for (int level = 1; level < ClockRun.Rates.Length; level++)
            {
                var run = ClockRun.To(TwentyDays / 4, 805, level, ClockRun.Ragged, 3 + level);
                Assert.AreEqual(first.hash, run.hash, "speed " + (level + 1));
                CollectionAssert.AreEqual(first.cards, run.cards, "speed " + (level + 1));
            }
            Assert.AreNotEqual(first.hash, ClockRun.To(TwentyDays / 4, 806, 0, ClockRun.Even, 30).hash, "another seed is another world");
        }

        [Test]
        public void AHitchIsNotAJump()
        {
            var s = new ClockStepper();
            // ten real seconds in one frame at speed 5 count as a tenth of a second: 36 game minutes, not 3600
            Assert.AreEqual(36, s.Take(10.0, 21600.0), "a hitch counts as the longest frame");
            // frames whose steps are not run (the machine cannot keep up) do not pile up without end
            Assert.AreEqual(ClockStepper.MostSteps, s.Take(10.0, 21600.0), "what is owed stops at the most steps of a frame");
            Assert.AreEqual(ClockStepper.MostSteps, s.Take(10.0, 21600.0));
            for (int i = 0; i < ClockStepper.MostSteps; i++)
                s.Step();
            Assert.AreEqual(0, s.Take(0.0, 21600.0), "and when they are run nothing more is owed");
        }

        [Test]
        public void AStoppedClockOwesNothing()
        {
            var s = new ClockStepper();
            Assert.AreEqual(3, s.Take(1.0 / 120.0, 21600.0), "half a frame at speed 5 is three game minutes");
            Assert.AreEqual(0, s.Take(1.0, 0.0), "held");
            Assert.AreEqual(0, s.Take(1.0 / 60.0, 60.0), "and going on at speed 1 there is no burst");
            Assert.AreEqual(0L, s.Minute);
        }

        [Test]
        public void SpeedOneIsAGameMinuteARealSecond()
        {
            var s = new ClockStepper();
            for (int frame = 0; frame < 60 * 60 * 3; frame++)          // three real minutes at sixty frames a second
                for (int n = s.Take(1.0 / 60.0, 60.0); n > 0; n--)
                    s.Step();
            Assert.AreEqual(180L, s.Minute);
        }

        [Test]
        public void TheFiveSpeedsAreAsRuled()
        {
            var r = Kapman.Game.GameSpeed.Rates;
            Assert.AreEqual(5, r.Length);
            Assert.AreEqual(60.0, r[0], "speed 1: a game minute a real second");
            Assert.AreEqual(4.0, r[1] / r[0], "speed 2: four times that");
            Assert.AreEqual(60.0, 86400.0 / r[2], "speed 3: a day in a minute");
            Assert.AreEqual(15.0, 86400.0 / r[3], "speed 4: a day in fifteen seconds");
            Assert.AreEqual(4.0, 86400.0 / r[4], "speed 5: a day in four seconds");
        }

        [Test]
        public void TheClockDropsBackAndOneKeyGoesBack()
        {
            Kapman.Game.GameSpeed.Multiplayer = false;
            Kapman.Game.GameSpeed.SetLevel(4);
            try
            {
                Assert.IsTrue(Kapman.Game.GameSpeed.DropTo(1, "nearing port"));
                Assert.AreEqual(1, Kapman.Game.GameSpeed.Level);
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Before, "the speed she ran at is kept");
                Assert.IsTrue(Kapman.Game.GameSpeed.DropTo(0, "moored"), "a second drop");
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Before, "keeps the first speed to go back to");
                Assert.IsFalse(Kapman.Game.GameSpeed.DropTo(2, "a sail"), "a drop never raises the speed");
                Kapman.Game.GameSpeed.Faster();
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Level, "one key: back to the speed she had");
                Assert.AreEqual(-1, Kapman.Game.GameSpeed.Before);
                Kapman.Game.GameSpeed.Slower();
                Assert.AreEqual(3, Kapman.Game.GameSpeed.Level);
            }
            finally
            {
                Kapman.Game.GameSpeed.SetLevel(0);
            }
        }
    }
#else
    public static class StepCheck
    {
        public static int Main()
        {
            const long twentyDays = 20L * 24 * 60;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var slow = ClockRun.To(twentyDays, 793, 0, ClockRun.Even, 90);
            var fast = ClockRun.To(twentyDays, 793, 4, ClockRun.Ragged, 7);
            bool same = slow.hash == fast.hash && slow.x == fast.x && slow.z == fast.z && slow.stepper.Minute == fast.stepper.Minute
                        && slow.cards.Count == fast.cards.Count && slow.drops.Count == fast.drops.Count;
            for (int i = 0; same && i < slow.cards.Count; i++)
                same = slow.cards[i] == fast.cards[i];
            Console.WriteLine($"speed 1: {slow.frames} frames, most steps a frame {slow.mostSteps}, hash {slow.hash:X16}, cards {slow.cards.Count}, drops {slow.drops.Count}");
            Console.WriteLine($"speed 5: {fast.frames} frames, most steps a frame {fast.mostSteps}, hash {fast.hash:X16}, cards {fast.cards.Count}, drops {fast.drops.Count}");
            Console.WriteLine(same ? "THE SAME WORLD" : "NOT THE SAME");
            var first = ClockRun.To(twentyDays / 4, 805, 0, ClockRun.Even, 30);
            for (int level = 1; level < ClockRun.Rates.Length; level++)
            {
                var run = ClockRun.To(twentyDays / 4, 805, level, ClockRun.Ragged, 3 + level);
                bool ok = run.hash == first.hash && run.cards.Count == first.cards.Count;
                Console.WriteLine($"speed {level + 1}: {run.frames} frames, most {run.mostSteps}: {(ok ? "the same" : "NOT THE SAME")}");
                same &= ok;
            }
            var other = ClockRun.To(twentyDays / 4, 806, 0, ClockRun.Even, 30);
            Console.WriteLine("another seed: " + (other.hash != first.hash ? "another world" : "THE SAME (wrong)"));
            var s = new ClockStepper();
            int hitch = s.Take(10.0, 21600.0);
            int piled = s.Take(10.0, 21600.0);
            Console.WriteLine($"a ten second hitch at speed 5 wants {hitch} steps; a second one unrun, {piled} (most {ClockStepper.MostSteps})");
            var one = new ClockStepper();
            for (int frame = 0; frame < 60 * 60 * 3; frame++)
                for (int n = one.Take(1.0 / 60.0, 60.0); n > 0; n--)
                    one.Step();
            Console.WriteLine($"three real minutes at speed 1, sixty frames a second: {one.Minute} game minutes");
            Console.WriteLine($"{watch.ElapsedMilliseconds} ms");
            return same && hitch == 36 && piled == ClockStepper.MostSteps && one.Minute == 180 && other.hash != first.hash ? 0 : 1;
        }
    }
#endif
}
```

### 6.2 The real world on the step: `Assets/Kapman/Tests/EditMode/ClockWorldTests.cs` (new)

The game's own `WorldSim` (traders, raiders, markets, weather, made as `KapmanGame.MakeWorld` makes it, through
`SimHarness.MakeWorld`), with a boat of the player's sailing from Dorestad toward Lundenwic that the world's ships
keep clear of, four game days, at speed 1 and at speed 5. **Compiled, not run** (it needs Unity for the tables). It
should take about as long as `SimTests.TheSameSeedPlaysTheSame` (30 s in the log of 4 October). If it fails while
6.1 passes, something in `WorldSim.Hour` depends on more than the hour and her place: `SimTests` would then be
failing too.

`Assets/Kapman/Tests/EditMode/ClockWorldTests.cs`

```csharp
using System;
using System.Collections.Generic;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// Ruling 26 of 7 October 2026: "prove it by a test that runs the same seed at 1 and at 5". The real world of the game
    /// (its traders, raiders, markets and weather: WorldSim, as KapmanGame makes it) on the clock's step, with a boat of
    /// the player's sailing from Dorestad toward Lundenwic that the world's ships keep clear of. Run once at speed 1 with
    /// even frames and once at speed 5 with ragged ones: the ships, the markets and her boat must end the same.
    /// </summary>
    public class ClockWorldTests
    {
        sealed class Run : ITickSink
        {
            public WorldSim world;
            public readonly ClockStepper stepper = new ClockStepper();
            public readonly List<ulong> marketsByDay = new List<ulong>();
            public WorldXZ boat, perStep;
            public int frames;
            readonly TickLadder ladder = new TickLadder(793, 172);
            double lastHours;

            public void Minute(long minute) { }
            public void Hour(long hour) => world.Hour();
            public void Day(int day) => marketsByDay.Add(Markets.Hash());
            public void Year(int year) { }
            public void Jumped(double fromHours, double toHours) { }
            public void Reversed(double fromHours, double toHours) { }

            public void Frame(double realSeconds, double rate, long until)
            {
                frames++;
                int want = stepper.Take(realSeconds, rate);
                for (int i = 0; i < want && stepper.Minute < until; i++)
                {
                    long minute = stepper.Step();
                    boat = new WorldXZ(boat.x + perStep.x, boat.z + perStep.z);      // she sails the minute
                    double to = minute / 60.0;
                    ladder.Advance(lastHours, to, false, this);                      // the hour: the world's ships, by where she is
                    lastHours = to;
                }
                if (stepper.Minute >= until)
                    stepper.Stop();
            }
        }

        static Run Play(int seed, double rate, double[] frameSeconds, int days)
        {
            var run = new Run { world = SimHarness.MakeWorld(seed) };
            var from = GameData.Port("dorestad");
            var to = GameData.Port("lundenwic");
            Assert.IsNotNull(from, "dorestad");
            Assert.IsNotNull(to, "lundenwic");
            var (ax, az) = Kapman.World.WorldFrame.ToWorld(from.lat, from.lon);
            var (bx, bz) = Kapman.World.WorldFrame.ToWorld(to.lat, to.lon);
            double far = Math.Sqrt((bx - ax) * (bx - ax) + (bz - az) * (bz - az));
            const double metresAStep = 2.0 * 60.0;                                   // about four knots
            run.boat = new WorldXZ(ax, az);
            run.perStep = new WorldXZ((bx - ax) / far * metresAStep, (bz - az) / far * metresAStep);
            run.world.playerNow = () => (run.boat, new WorldXZ(run.perStep.x * 60.0, run.perStep.z * 60.0));
            run.world.keepClear = true;                                              // (so her place matters to the world)
            run.world.stats.csvEvery = run.world.stats.sampleEvery = 0;
            run.world.Populate();
            long until = days * 24L * 60L;
            for (int i = 0; run.stepper.Minute < until; i++)
                run.Frame(frameSeconds[i % frameSeconds.Length], rate, until);
            return run;
        }

        [Test]
        public void TheSameSeedAtSpeedOneAndAtSpeedFiveEndsInTheSameWorld()
        {
            const int days = 4;
            var slow = Play(793, GameSpeed.Rates[0], ClockRun.Even, days);
            long slowShips = slow.world.StateHash();
            ulong slowMarkets = Markets.Hash();
            var fast = Play(793, GameSpeed.Rates[4], ClockRun.Ragged, days);
            long fastShips = fast.world.StateHash();
            ulong fastMarkets = Markets.Hash();
            Markets.Clear();

            Assert.AreEqual(slow.stepper.Minute, fast.stepper.Minute, "the minute they ended on");
            Assert.AreEqual(slow.world.Hours, fast.world.Hours, "the world's hours");
            Assert.AreEqual(slowShips, fastShips, "the world's ships");
            Assert.AreEqual(slowMarkets, fastMarkets, "the markets");
            CollectionAssert.AreEqual(slow.marketsByDay, fast.marketsByDay, "the markets, day by day");
            Assert.AreEqual(slow.boat.x, fast.boat.x, "her boat, east");
            Assert.AreEqual(slow.boat.z, fast.boat.z, "her boat, north");
            Assert.AreEqual(days, slow.marketsByDay.Count, "a tick for every day");
            Assert.Less(fast.frames, slow.frames / 50, "speed 5 got there in far fewer frames");
        }
    }
}
```

### 6.3 The whole game on the chart: a play-mode test (outline, NOT compiled)

6.1 and 6.2 prove the step. They do not prove the part of the game that still looks once a frame: the port's
graph, the frame's windows, the talks. Only the game itself can, so this is the test ruling 26 asks for. It goes in
`Tests/PlayMode/ChartGameTests.cs` and uses that file's own helpers (`OpenChart`, `Frames`, and the lines of
`SheSailsTheChart` that make the person and cast off, which I have not copied). It runs the clock by hand
(`KapmanGame.ClockByHand`, `RunClock`), so it does not wait on the wall clock, and it answers every card and talk
through the model, the same way in both runs.

```csharp
        /// <summary>Ruling 26: the same seed on the real chart, once with never more than one step a frame and once
        /// at speed 5 with six. Three game days toward Lundenwic; every card and talk answered the same way.</summary>
        [UnityTest]
        public IEnumerator TheSameSeedAtSpeedOneAndAtSpeedFiveEndsTheSame()
        {
            var ends = new List<string>();
            // (the slow run is speed 2 with frames of a tenth of a second: 0.4 of a step a frame, never two. Speed 1
            // the same way is ten frames a step: right, and 43,000 frames. Run it once by hand.)
            foreach (var (level, frame) in new[] { (1, 0.1), (4, 1.0 / 60.0) })
            {
                yield return OpenChart();
                MainMenu.Current.NewGame();
                yield return Frames(120);
                // ... here: make the person and cast off for Lundenwic as SheSailsTheChart does, the clock paused ...
                var game = KapmanGame.Current;
                var seen = new List<string>();
                KapmanGame.ClockByHand = true;
                try
                {
                    double until = game.Hours + 72.0;
                    for (int i = 0; game.Hours < until && i < 100000; i++)
                    {
                        if (GameSpeed.PlayerPaused || GameSpeed.Level != level)
                            GameSpeed.SetLevel(level);                   // (the player picks the speed again after a drop)
                        game.RunClock(frame);
                        yield return null;                               // (a real frame: the frame, the graphs, the runner)
                        var d = EncounterRunner.Director;
                        if (d != null && d.Current != null && !d.Current.Chosen)
                        {
                            seen.Add(Math.Round(game.Hours * 60.0) + " " + d.Current.card.id);
                            d.ChooseDefault();
                            d.Continue();
                        }
                        var talk = TextPanel.Current;
                        if (talk != null && talk.Open && talk.Choice < 0 && talk.Key != TextPanel.PortMenuKey)
                        {
                            seen.Add(Math.Round(game.Hours * 60.0) + " " + talk.Key);
                            talk.Pick(0);
                        }
                    }
                    var p = game.ShipPosition;
                    ends.Add($"{game.Hours:0.###} ships {game.World.StateHash()} markets {Markets.Hash():X16} her {Math.Round(p.x)} {Math.Round(p.z)} " +
                             $"silver {FrameGame.Silver:0.##} bread {Voyage.FoodDays:0.###}\n" + string.Join("\n", seen));
                }
                finally
                {
                    KapmanGame.ClockByHand = false;
                    GameSpeed.SetLevel(0);
                }
            }
            Assert.AreEqual(ends[0], ends[1], "the same seed ended differently at speed 5");
        }
```

How to read a failure: the two texts list every card and talk with the game minute it came on. The first line that
differs names the thing that is still looked at by the frame and not by the step. The likely ones, in the order I
would suspect them: the port's graph laying her to (raise `OneAFrame`'s reach, or lay her to from the step); a card
that slipped a step past its hold (raise `KapmanGame.SettleFrames` from 2 to 4); `Patrols`, `Encounter` or
`RaiderEncounter` reading the wall clock somewhere I did not find; `Rest`'s dawn.

NOT CHECKED: whether a new game twice in one test run starts from the same seed and the same first morning (it
should: `KapmanGame.worldSeed` is a field, 793); whether `ChooseDefault` can refuse (a card with no default: then
take the first option that is enabled, as `Answer` in that file does).

## 7. What else was sized for the slow clock, and must be sized again

"The slow clock" is ruling 1 to 24's: a game day in six real minutes at best. Each row says what the thing was
sized for, what it is now, and what to do. Numbers that count game time need no change of number; what changes is
what a number means in a sitting.

### 7.1 How many cards a day at sea

- Today: at most one drawn sea or crew card every 4 game hours (`EncounterRunner.chartSeaGapHours = 4`, line 49):
  up to six a game day. The cards' own rates do not pace it: 93 sea cards and 126 crew cards carry a rate, the
  median is 0.03 and 0.1 a game hour, and summed they are 9.2 and 34.7 an hour if every condition held (counted from
  the 54 card files). So whenever she is at sea the draw nearly always has a card, and the gap is the pace.
- At the slow clock that was a card every 60 real seconds at the old 4x. **At speed 4 it is one every 2.5 seconds of
  running clock**; at speed 3, one every 10. The clock would never run.
- Over the first life: 2,328 sailing days, of which she is at sea perhaps six in ten: 1,400 days. At a 4-hour gap
  that is up to 8,400 sea cards drawn from 227: each one 37 times. The cooldowns do not stop it (of the 227 sea and crew cards,
  44 have none, 63 are 365 or 400 days, the rest 5 to 300).
- **Make it 36 game hours** (`chartSeaGapHours = 36`): at most two in three days at sea. One every 22 seconds of
  running clock at speed 4, every 90 at speed 3, every 9 minutes at speed 2. Over the first life at most 930: 78 a
  season, each card about four times in twelve years, and about five hours of reading on top of ten of sailing. A
  crossing to Lundenwic carries one to three, which is what the plan wanted of it ("two cards in it",
  review_words.md). It is one number; tune it by play.
- With slots this scarce, WHICH card comes matters more. The draw picks by the rate's share (`Scheduler.Draw`): the
  nine `crew.song.fitt_*` cards at 0.6 an hour take a slot twenty times as often as a card at 0.03 when both can
  come. For the content pass: either cap the rate used for choosing at 0.1, or look at the high rates again
  (`crew.death.*` at 1.0 are right: they are forced). Not changed here.
- In port nothing changes: two arrival cards at most and one drawn card a call (`callDrawMax = 1`).
- The dated hours (Game/Data/story/hours_793.json): `aethelberht_794` is open 22 days, `godescalc_798` 29,
  `offa_dead_796` 27. That was two to three hours of play; it is five to eight minutes at speed 4. A player at the
  other end of the map will miss them without knowing. For the content pass: a word in the log when an hour she
  could reach opens, and later a drop.

### 7.2 Bread

- 0.7 kg a man a day, four men: 2.8 kg a day; bread for 7 days at the start; water for 20 (240 litres), free in any
  port (`voyage_rules.json`). Game time: the numbers hold.
- Without food a man loses 0.08 of his health a day: dead in twelve and a half days. Without drink 0.25: four days.
  That was 75 and 24 minutes of warning at the old 4x. At speed 4 it is 3 minutes and 1 minute; at speed 5, 50 and
  16 seconds. Hence the two drops of 4.3: at two days of bread the clock goes to speed 2 (12 real minutes to act),
  at none to speed 1.
- **A chore that the new clock makes worse:** a week's bread was bought once every 42 minutes of play; at speed 4 it
  is once every 1 minute 45. A standing order would take it away: "keep bread for N days aboard", bought at each
  mooring where it is sold, at the day's price, told in the log; off from the start; N is 10. Not drafted (it is
  the Hold window's and `Voyage.InPort`'s).

### 7.3 Wages

- A quarter of a denier a man a day: 1 d a day for four, 194 d a season. Game time: holds.
- `Voyage.InPort` pays at every call and says so ("The men are paid 1 d."). At speed 4 that line comes every half
  minute or so on the river. Say it as a sum when she casts off, or once a week.
- A man owed 14 days may leave at the next port (`leave_if_owed_days`): three and a half minutes at speed 4. The
  drop when she is made fast is what gives the player the call in which to pay.

### 7.4 How fast news travels

- News goes by ship, at the ships' own speed, from board to board (`WorldSim.Board`, `SeedKnowledge` at 60 km a day,
  WorldSim.cs 308); a deed counts far away "only when its news gets there". All in game time: nothing to change.
- `knowing_793.json`: a price seen is good for 7 days, told for 14, news 60, known 90. They hold. But "seen 7 days
  ago" was 42 minutes of play and is 1 minute 45 at speed 4: nearly everything the player knows is stale at high
  speed. That is true to the game. It makes the mark of how old a word is (the word on the item) the thing to get
  right, more than before.
- The frame's log (`FrameLog`): at speed 5 the world does six hours a second. Under speed 3 keep it as it is. From
  speed 3 up: one heading a game day, and under it at most six lines, then "and 9 more" (the Word window has them
  all). Sightings already stay out of it (`SeaWatch.Write`, notice only for "made out").
- The lookout's own line ("Made out: ...") goes to `Voyage.Say` for every ship. On the Rhine at speed 4 that is a
  line every few seconds. Fold it the same way.

### 7.5 The length of a talk

- A talk with a person does not hold the clock (`PersonTalk.cs` 99, `holdsClock = false`), and its lines come over
  real seconds (`FrameTongues.cs` 69-71). When ruling 5 was made a half-minute talk cost half a game hour.
- **At speed 4 the same talk costs two days. At speed 5, a week.** Reading a text in the chest for five minutes at
  speed 4 costs twenty days.
- So opening a talk or the chest drops the clock to speed 1 (`ClockReason.Talk`, 4.3 and 5.3). The player can raise
  it again; then they meant it.
- Another way, for the user to rule on if the drop is not liked: a talk holds the clock while it is open and costs
  a fixed twenty game minutes when it shuts. Not drafted.

### 7.6 The waits

- Resting ran the clock at 30 times, the tide wait and "aground: wait for the tide" at 60 times
  (`Rest.Rate = 1800`, `waiting_rate: 3600`). They were the game's fast-forward. Speeds 4 and 5 are 96 and 360
  times: the waits' own paces are now the slow way to wait.
- On the chart they are gone with this change (the stepped clock does not read `clock.rate`): every wait is a wait
  at the speed the player picks, and the clock drops back when it is over. The unseen `Aground` case of section 2
  is mended by the same stroke.
- The frame's Wait button says the clock runs "at its own pace (1x, 2x, 4x)" in its summary (`FrameRoot.Rack.cs`
  12): the words to the player, wherever they say 1x, 2x or 4x, must be looked for and changed. I did not list them.

### 7.7 The meetings at sea, and aground

- The floor times in `SeaHolds.LimitHours` (2 hours for the king's men and for raiders coming, 3 for a trader's
  offer) and the raiders' graph's "no answer: run" after 20 game minutes were sized for a clock where an hour was 15
  seconds at the fastest. At speed 5 two hours is a third of a second. The cap of 4.3 (no faster than speed 2 while
  a pressing matter waits) is what keeps them usable: two hours is 30 seconds at speed 2 and two minutes at speed 1.
- Aground: the choice is offered after 15 game minutes stuck, again after 60, the men row for 30. Fine in the step.

### 7.8 The first hour, and the tests' hurry

- "One good hour, Dorestat to Lundenwic" was an hour at the old 4x. At speed 3 an hour is two months of the
  calendar. checks/calendar_first_season.md covers the season; the sixty first-hour cards
  (Design/content/first_hour_cards.md) were picked for one crossing and are now the cards of the first weeks.
- The play tests hurried by making the top speed 40 times (5.4).
- The debt: 480 d by Michaelmas, 151 days. It was the stake nobody would reach in a sitting; at speed 4 it falls
  due 38 minutes of running clock after the start. Nothing to change; it is the best thing the new clock does for
  the first hour.

## 8. What I could not check

1. Nothing was run in Unity. The edits compile; they have not stepped one frame of the real game.
2. Unity's limit on `Time.timeScale` (100) and whether `Time.unscaledDeltaTime` follows `Time.captureFramerate`
   (the play tests set it to 30): from memory of the documentation. The draft does not lean on either.
3. That two frames (`SettleFrames`) are always enough for the frame to put a card up and `PanelClock` to hold the
   clock. I traced it for a card from the hourly draw (the runner opens it at order 10 and the frame builds its
   window at order 900 in the same frame; `PanelClock` holds at the start of the next). I did not trace a talk
   from a script graph, a happening of the events table, or the alerts.
4. That nothing else on the chart counted on the time scale to hurry. I searched `Game`, `Core` and `Graphs` for
   `Time.deltaTime`, `Time.time`, `WaitForSeconds`, the level clock's speed and rate: the hits are `ChartBody`,
   `VoyagePlanner`, `Aground`, `TextNodes` (all four in the draft), `EncounterRunner` (its 3D branch), and
   `LocalBoats` (bobbing). I did not search `Runtime/Levels`: whatever of it is in the chart scene now runs at real
   time, which is what the ruling asks of the view. The script graphs use no Unity timer nodes (I listed every node
   type in the fourteen graphs: the only waits are `TextWaitForChoice`, `TextWaitForChoiceTimed`,
   `EncounterWaitAlongside`; "Ports - coming in and leaving" has two per-frame nodes).
5. The cost of `WorldSim.Hour()` in the player (the 3 to 14 ms is from editor test logs, set-up included), of the
   cards' hourly draw, and of 360 wind samples a second.
6. The world test (6.2) and the play-mode test (6.3) have not been run. Until 6.3 passes, "the same world at 1 and
   at 5" is proven for the step and argued for the rest.
7. `GameEvents.Look()`, `Alerts.Tick()` and `Rest.Tick()` still run once a frame from `KapmanGame.Update`. I read
   that `Rest` only watches for dawn and a key; I did not read `GameEvents.Look` through.
8. The save. `SaveGame.Load` calls `ResyncHours` (which the draft extends); I did not read the order in which the
   ship, the trade and the clock come back, so `ClockDrops.Rearm` is called on the next frame, not there. A game
   saved before this change has a clock that is not on a whole minute: the first step after loading is then up to
   a minute short on the clock while she sails a whole one. Once per old save.
9. Multiplayer is as it was: speed 1 only, no pause.
10. The wording on screen is placeholder (the user writes): the tips, the eleven switch lines, the words on the bar.

## 9. The order of work for the builder

Each step compiles and can be committed alone.

1. `Core/Sim/ClockStepper.cs` and `Tests/EditMode/ClockStepperTests.cs` (take out its last two tests until step 2
   is in: they name `GameSpeed.Rates` and `DropTo`). Run the tests.
2. `Game/UI/GameSpeed.cs` (whole file), `Game/UI/ClockDrops.cs` (new), `LevelClock.cs` (2 edits), `FrameRoot.cs`
   (3 edits), `MapTable.cs` (1 edit), and the eight lines of 5.4. Now there are five buttons; off the chart
   everything is as before; on the chart `Stepped` is not yet set, so the old way still runs. Compile, run all
   edit-mode tests.
3. The step itself: `KapmanGame.cs` (3 edits), `ChartBody.cs` (4), `VoyagePlanner.cs` (2), `SeaWatch.cs` (1),
   `EncounterRunner.cs` (2), `Aground.cs` (1), `RaiderEncounter.cs` (3), `FrameRoot.Rack.cs` (3), `TextNodes.cs`
   (1). These go in together: `KapmanGame` sets `Stepped`, and the others answer to it. Run
   `ClockWorldTests`. Play: speed 1 from Dorestat to Traiectum must look as it did yesterday; then speed 5.
4. Look at `KapmanGame.StepMsLastFrame` at speed 5 with the world running (a line in the dev HUD): if it sits at
   the budget, the world's hour is the cost (3.3).
5. The one-liners of 5.3, the styles, the switches in the pause menu.
6. The play-mode test of 6.3. It will find what I could not.
7. `chartSeaGapHours = 36` (7.1), then play a crossing at speeds 3 and 4.
