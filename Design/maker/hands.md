# The hands: how Kapman is played through the frame

7 October 2026. The first apprentice's page for the game maker.

**How I worked.** I read the code under `Assets/Kapman/Code`, layout G of `Design/ui_mock/template3.html`, the three
captures, both critiques, `depth_design.md`, the fleets plan, the concept and the town page. I did not run the game and
made no web search. Only this file was written. A number with a file beside it comes from that file. Any other number
is a first guess and is marked (g).

**Marks.** *Built*: the code is there and works in `L_Chart`. *Half*: the code is there, but the chart scene cannot
reach it or it needs one change. *Missing*: to be written. **H**: needed for the first good hour (four ports, one
debt). **Y**: the first year. **L**: later.

## The short of it

- The mock has two switches for one place: five tabs and eight buttons. Make it **one rack and one focus place**.
- "Intricate controls" is **one order of five switches** (cast off, way, dusk, sail, calm), set at the quay and live at
  sea. The leg asks about them again through **signs**: a man in a seat says one line, one switch lights, the clock
  stops. Signs come from the simulation, so they never wear out the deck. Cards stay for people and stories.
- The same five switches are what you later tell a captain. One language for your boat and for the fleet.
- **The measure in days is the calendar.** Whatever you measure on the chart is laid on the day rule, beside the
  bread line and Michaelmas.
- Most controls already have code under them. Four things are truly missing: the frame, a kept log with the leg's
  record, a list of dues, and the order with its signs.

## 0. Six rules of the hand

1. **Left presses, right shows, the chart answers, the log remembers.** Every press that plays the game is in the
   left column. The face and the ship window are pictures; pressing one is a shortcut to a left panel.
2. **One rack, one focus place.** A function button puts its panel in the focus place. Pressed again, the panel folds
   and the chart runs to the rim ("with the empty space you could just make more map").
3. **The panel asks, the chart answers.** The panel in focus decides what the chart is the key to (section 3.2).
4. **A click on the chart is a shortcut, never the only way.** Click a port and it is picked in the panel in focus.
   This keeps the 4 October decision beside the 6 October wish.
5. **The cost is on the control before the press**, always in one order: silver, days, risk as a word. The hint line
   says what the press will do. (`depth_design.md` 1.6 says the hint line never does. The player critic is right.)
6. **The clock runs only with Voyage in focus and no sign lit.** Any other panel is you turning away to think, and the
   clock waits, as decided on 4 October. In port it is held until you press Wait. Today `PanelClock.PanelOpen` lists
   panels by name; with a frame that is always open it must read the focus.

```
+-- settings ---+------------- day rule (the measure) -------------+-- portrait --+
|   rack        |                                                  |   four heads |
+---------------+                                                  +--------------+
|               |                    the chart                     |
|  focus panel  |          (shows through between the pieces)      +--------------+
|               |                                                  |  ship window |
+---------------+------------ log, hint line on top ---------------+--------------+
```

## 1. The frame, control by control

### 1.1 Settings block (top left) **H**

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **Menu** | Opens Save, Load, Options, Quit | The four words | Save, with the reason, while something is going on | *Built*: `SaveGame.Save`, `SaveGame.Load`, `SaveGame.Slots`, `SaveGame.Busy`, `MainMenu`. Today Save and Load sit in a strip under the chart |
| **Hold** (Space) | Stops the clock | Lit, with "the clock waits" when a panel holds it | Never | *Built*: `GameSpeed.TogglePause`, `GameSpeed.Describe` |
| **Watch** | Runs at 1x; pressed again 2x, 4x. For watching her in the window | The rate | In port | *Built*: `GameSpeed.SetLevel` |
| **Sail on** | Runs an hour a second until a sign, a card, a sail made out, or the berth | Lit while running | In port, where it reads **Wait** (till dawn, the tide, the wind) | *Half*: the rate exists as `VoyageRules.tideWaitRate` (3600) and `TideWait` uses it; `Rest` runs at 1800. Missing: `GameSpeed.SailOn` and its stops (`GameSpeed.Slow` and `Alerts.Raise` are the hooks) |
| **Silver** | Opens Trade | "86 d" | Never | *Built*: `Trade.Purse.Silver(TradeRules.Current)` |
| **Bread** | Opens Trade at the hold | "bread for 7 days"; red when the course set is longer | Never | *Built*: `Voyage.FoodDays`, `Voyage.DrinkDays` (show the smaller) |

Sail on is the player critic's answer to "sailing is waiting". It is not a zoom: the dates, the distances and the
clock's rule stay, and the empty hours pass at the rate a tide wait already passes. You have ruled out time warps
before, so this one is yours to say yes to. The scripts in section 4 depend on it.

The date is not in this block. It is the left end of the day rule. The debt is not here either: it is a flag on the
rule and the first row of Tasks, so it is always in sight.

### 1.2 The rack (function buttons, under the settings) **H**

A button is there only when its thing exists (`depth_design.md` 1.3 has this right). It wears a small count when
something in it is new. Keys 1 to 7.

| Key | Button | Arrives | Why this game needs it |
|---|---|---|---|
| 1 | **Voyage** | the start | choose and sail: the order |
| 2 | **Trade** | the start | market, hold and the word in one place |
| 3 | **People** | the start | the three talks, gifts, and your four men |
| 4 | **Tasks** | the tally card | the debt, promises, orders taken |
| 5 | **Chest** | the first text or news | texts, riddles, notes, the year's chronicle: the culture half of the game has no other home |
| 6 | **Ship** | she is first hurt, or first lies at a yard | hull, sail, gear, the yard: "upgrades, different ships" |
| 7 | **House** | the first Martinmas | keels, kin, holdings, orders to a steersman |
| | **Event lamp** | always, at the rack's end | dark, or lit with a count. A card takes the focus by itself; the lamp brings it back |

Greyed: nothing in the rack is greyed. While a card is up the rack takes no press, and the card's own cost lines show
what it touches ("bread: 5 days, then 3").

Code: *missing*, one `Frame` (UI Toolkit) that owns the places. It replaces `MapTable` (the top bar, the outliner,
the foot strip). The panels that exist are moved in, not written again: `MarketPanel.Open(port, docked)`,
`ChestPanel.Show(into, tab)`.

### 1.3 The focus panels (left)

**Voyage (H).** Three parts: where to, the order, the go keys.

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **Where to** rows | Picks the port; the chart draws its ways | Name, days as words ("2 to 4 days"), fair or foul, your best word there ("wine pays well, 6 days old") | No way by water you know (reason shown). A raided or shut port is struck through but can be picked | *Built*: `Course.Plan` gives line, depths, kinds and km without setting a course; `Known.IsHeardOf`; `Trade.Ledger.Notes`. It is the same press as a click on the port, which today goes through `MapView`'s port card and `SetCourseToCard` |
| **The order**: five switches | Section 2.1 | One sentence read back, and the cost line | Each position with its reason ("No reef bands. 12 d at a yard.") | Section 2.1 |
| **Cast off** (Enter) | Sets the course and lets her go | "Cast off on the ebb, in 2 hours" when the order says wait | No port picked. Short bread warns, it does not block | *Built*: `Course.SetTo(port, here, fromPort)`, then `ship.anchored = false` (what the graph node `ShipMoor` does); `VoyagePlanner` sails her; `Patrols.CastOff` fires by itself |
| **Change course**, **Lie to**, **Put in**, **Turn back** | At sea, in place of Cast off. Put in lists the two nearest landings you know, with hours | Hours to each | Put in: no landing known in reach | *Built*: `Course.SetTo`, `ship.anchored`, `Port.Nearest`. Missing: landings that are not ports |
| **Hail**, **Hold course**, **Stand in** | When a sail is made out | Her kind and home once made out | Stand in: she draws too much for the shoal | *Built*: `SeaWatch.MadeOut`, `Encounter.Offer`, `Encounter.CloseAndHail`, `Encounter.Part` |
| **Run** | Raiders | The chase in hours | | *Built*: `RaiderEncounter.Run`, `RaiderEncounter.Status` |
| **Heave to**, **Gift**, **Run**, **Dump** | The king's men | What they look for | Gift: under 12 d aboard (`giftMinD`) | *Built*: `Patrols.Answer(what)`, `Patrols.Facts` |
| **Wait for the tide**, **Row off**, **Push off** | Aground | Hours to float | Push: too far from deep water | *Built*: `Aground.Options`, `Aground.Choose(what)` |

Those last four sets exist. Meetings, raiders and the king's men already come in the chart (`chart_game.md`);
grounding I could not confirm there. Today each opens as a `TextPanel` menu on top of her mark. In the frame they are
keys that light in Voyage.

**Trade (H).** The market above, the hold below, the word on every row. This is Ledger, Hold and the market in one.

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **A good's row** | Picks the good; the chart wears its mark at every known port | Price asked and paid; the word ("A Frisian skipper said Lundenwic pays well. Last week.") | Never | *Built*: `Market.Goods`, `Deal.AsksFor`, `Deal.PaysFor`, `Known.WordOn`, `GoodWord.Text` |
| **Where it pays** (under the picked row) | A line picks that port in Voyage | Three ports: the word and its age, the days, the gain a lot and for all that fits | No word of it anywhere: "Nobody has told you." | *Half*: `Known.Price(port, good, month, today)` gives pays, age and sureness; the days come from the measure. The sum is missing. `TraderBrain` works this sum for the 138 ships and never shows it to you |
| **Buy 1**, **Buy all that fits** | One lot into the hold | Silver left, the load line | No silver ("short by 12 d"), no room, none on offer | *Built*: `Trade.Buy`, `Hold.RoomFor`, `MarketGood.OnOffer` |
| **Sell 1**, **Sell all** | One lot to the market | The toll's tenth, the silver | None aboard; the port will not deal with you | *Built*: `Trade.Sell`, `Reputation.Refuses` |
| **Haggle**, **For silver / for wares**, **On the tally** | Section 2.3 | | | Section 2.3 |
| **The hold**: three places, the load line, first over the side | Section 2.2 | | At sea only "first over the side" changes without lying to | Section 2.2 |

At sea the market half shows your ledger for the port she is bound for. Nothing can be bought, so no Buy key is drawn.

**People (H).** Hearing, giving, and the crew. This is People and Crew in one.

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **Talks left** | Nothing | Three pips; one comes back each dawn she lies there | | *Missing*. Today `PortLife.Talk(port)` is one free press that gives up to eight news items and every price |
| **Talk** on a row, then **Plain / Warm / Hard** | Spends one talk | The keeper of the port (sure), a skipper just in (as old as his passage), the benches (doubtful), then the port's named people | No talks left: "Three talks a call. One comes back at dawn." | *Half*: split `PortLife.Talk` by source; `Known.HearNews(item, today, from, how, hops, at)` already keeps source and age; `WorldSim.Board(port)` and `Visitors(port)` say who is in. The named people of `places/*.json` are read by no code |
| **Gift** | Section 2.3 | | Given already this year | Section 2.3 |
| **A crewman's row** | Puts him in the portrait | Name, seat, one word of mood, pay owed | | *Missing*: `Voyage.Men` is a count; four records are needed. `NpcRegistry` can hold them |
| **Rations**, **Watches**, **Pay**, **Seats** | Section 2.4 | | | Section 2.4 |

At sea the panel is the four men, and any ship alongside (`Encounter.Stage`, `Encounter.Facts`, `Known.Spoke`).

**Tasks (H).** The debt is always the first row.

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **A task's row** | Nothing | What, for whom, the day, days left | | *Built*: `Trade.Debt`, `Trade.DebtDue`, `Trade.Creditor`, `Commissions.Mine` |
| **Show** | The chart goes there, the flag stands out, the measure is laid from her to it | "4 days away, due in 41" | | *Half*: needs the measure of 3.3 |
| **Pay** | Pays down the debt | The sum left | Not at his port; no silver | *Built*: `Trade.PayDebt(m, deniers)`. But the `house_debt` chain keeps a second sum in a flag: one debt first, as both critics say |
| **Take**, **Deliver** | An order offered here; an order brought | Pay and deadline | Deliver: the goods are not aboard | *Built*: `Commissions.At(port)`, `Commissions.Take`, `Commissions.Deliver` |
| **Give it up** | Breaks a promise | What it costs in standing | On the debt | *Half*: `Reputation.Deed` can carry the cost; the act is missing |

**Event (H).** The card, in the focus place, as layout G has it. The face it is about is top right.

| Control | Pressed | Shows | Greyed when | Code |
|---|---|---|---|---|
| **Choices 1 to 3** | Takes the choice; the written outcome follows | Each with its cost line: "12 d. He likes you more." or "half a day, likely" | The check fails, with the reason ("needs 12 d") | *Half*: `EncounterSession` holds title, lines, options and result. `EncounterPanel` draws its own box over the chart and does not show the written outcomes. And no card can open in `L_Chart` yet: `EncounterRunner.Moorings` finds no berths and `EncounterGameVars` reads a weather the chart does not have |
| **Go on** | Closes the card; the outcome becomes a log line | | Until a choice is made | |

**Chest (Y).** Texts, Riddles, Notes, The year so far. *Built*: `ChestPanel.Show(into, tab)`. The evening fitt is one
log line ("Ubbi has the next fitt. It is in the chest."); pressing the line opens the chest at it. A note can be
pinned to the chart from here (`MapView`'s Pin tool, built).

**Ship (Y).** Her hull, sail and rig as words (sound, strained, hurt). Gear fitted. At a yard: **Mend**, **Fit**,
**Tar**, with silver and hours on each. Pressing the ship window opens it. *Missing*: no code reads
`ship_upgrades_793.json` or `ships_793.json`, and `Effects.cs` has no verb to hurt a hull or a sail.

**House (L).** Keels, hearth, holdings, and the order you give a steersman at the quay (2.5).

### 1.4 The portrait (top right) **H**

It shows **who is speaking to you**. In port: the person you talk to. On a card: the man it is about. At sea: the man
on watch, with his last sign as one line under his name. It does not show your own face in play; the player critic is
right that this is 7% of the screen for nothing. Your face belongs on the winter board and in House.

| Control | Pressed | Shows | Code |
|---|---|---|---|
| **The face** | Opens People at that person | Name, his feeling as one word | *Built*: `NpcRegistry` keeps the feeling. Six drawn heads carry the first hour |
| **Four heads** under it | Opens People at that man | Each man in his seat; an uneasy man looks away; an empty seat is empty | *Missing*: the four records |

### 1.5 The ship window (bottom right)

| Control | Pressed | Shows | Code |
|---|---|---|---|
| **The view** | Opens Ship | Where she is (river, river mouth, coast, open sea, port), the sky and sea, the hour; the sail full, reefed or down; oars out; how deep she rides and what stands in her; four men; a sail made out on its true bearing | *Missing in `L_Chart`*: nothing is attached (a known gap in `chart_game.md`). The numbers exist: `Course.LegIsRiver`, `ChartBody.Wind`, `WindFrom`, `Knots`, `Sheltering`, `WorldSim.WeatherAt`, `Hold.Weight`, `SeaWatch` |
| **The caption** | Opens the view larger | Words: "The Lek below Dorestat. West, a fair breeze." | *Missing* |

How to have it soon: one small record, `ShipView`, of about ten numbers, filled each game hour. A drawn stand-in
reads it first (the mock already draws four places, three weathers, three hours). The 3D stage from the sail tank
reads the same record later. Then every control in section 2 has its picture from the first night, and the 3D can
come when it comes without the frame changing.

### 1.6 The log (bottom centre) **H**

| Control | Pressed | Shows | Code |
|---|---|---|---|
| **The hint line** (top) | Nothing | What the thing under the pointer does and costs: "Cast off for Lundenwic: 3 to 4 days, bread for 7" | *Half*: `HoverCards` |
| **A line** | Opens its panel at its subject; a place name in it moves the chart there | The hour as a plain word (dawn, morning, noon, afternoon, dusk, night), then the fact | *Missing*: one kept log. Today `Voyage.Notice`, `SeaWatch.Log`, `Deeds.Log`, `Trade.Traded`, `Deal.Dealt` and `Known.Learned` each go their own way, and `Notices.Show` fades three lines in seven seconds |
| **The wheel** | Reads back the leg, then the year | | The same log; it later feeds the winter board |

Two lines matter more than all the others. **The reveal** on arrival: why the price is what it is. **The reckoning**
after the sale: what the leg made after cost, toll, wages and bread, and whether your word was true. Both need the
record the designer critic names as the one missing system: the **leg**, from cast off to mooring.

### 1.7 The measure in days (top centre, and on the chart) **H**

| Control | Pressed | Shows | Code |
|---|---|---|---|
| **The day rule** | A notch: the hint line says what falls on it. A flag: Tasks opens at it | Today's date at the left end. A notch a day. The bread line under the days your bread lasts. Flags only for what bears on you: Michaelmas with the sum, a promise's day, a due on the tally, a fair | *Missing*. `KapmanGame.Date`, `Trade.DebtDue`, `Commissions.Mine`, `Voyage.FoodDays` and `Fairs` feed it |
| **Measure** (D) | Then press one place (from her) or two | The way by water is drawn; its days are laid on the rule as a pale stretch with a dark bar for each night | *Half*: the Dividers of `MapView.Tools` measure a straight drag in km. Use `Course.Plan` for the way by water |
| **Follow her** (F) | The chart comes back to her | | *Built*: `MapView` |

`Course.Days` uses a flat 3.5 knots (`Course.PlanKnots`). In the capture it said "about 0.1 days" for a leg that took
six hours. The measure needs a short and a long count: walk the course with `Travel.Speed` in today's wind for the
short one and in a foul wind for the long one, and say them as words.

## 2. Intricate controls: real decisions without steering

### 2.1 Sailing a leg: the order

Five switches in Voyage. Set at the quay, live at sea. Under them the frame reads the order back as one sentence
("On the ebb, by the coast to Lundenwic, lying up at dusk"), and that sentence is the cast-off line in the log.
The mock's three ready-made ways ("By the coast, lie up at dusk", "Straight across, sail by night", "Wait for a
fair wind first") stay above the switches as **presets**: one press sets all five. A new player uses the presets.
The depth is there when he wants it.

| Switch | Positions | What it costs | Log, chart, window | Code |
|---|---|---|---|---|
| **Cast off** | now / at first light / on the ebb / when the wind serves | Each day waited: 1 d of wages (4 men at 0.25 d, `VoyageRules.pay`), a day of bread, and every word you hold is a day older. The ebb down a river gives 1 to 1.5 knots for six hours (`depth_design.md` 2.3) | Log: "Waited two hours for the ebb." Chart: the tide arrow at the river mouth turns; the wind streaks on her way go from foul to fair. Window: she lies at the piles, the water moving past | *Half*. First light: `Rest`. Wind: `WorldSim.FairWind` is the world ships' own test and is private. Tide: `TideWait` is built but `VoyageTide.Known` needs a `LevelWater`, which the chart has not |
| **Way** | inside / by the coast / straight across | Inside (rivers, behind the islands, over the flats): the longest, out of the sea's weather, but it dries at low water and has toll stations. By the coast: you can lie up; a lee shore in an onshore gale; raiders wait off headlands. Across: the shortest; no landing, so nights at sea; beyond 40 km her place is a blot (2.3 of the depth page) | Chart: three lines with day ticks, the chosen one bold. Log: the way is in the cast-off sentence | *Half*: `Course.Profile` makes one `LaneRouter.Profile`; its `cost[]` is by lane kind (`Sea`, `Coastal`, `Sound`, `River`, `Flats`). Three ways are three cost rows. `Course.Plan` already plans without setting |
| **At dusk** | lie up / sail on | Lie up: the dark hours are lost (5 in June, 13 in October, from the depth page) and the men rest. It needs a landing you know within reach. Sail on: no hours lost; near a coast each dark hour is 2% to touch ground with no moon, 0.5% under one; the men tire | Chart: dark bars on the course where night falls; a small fire where she lies. Window: the boat drawn up by a fire, or a dark hull under stars. Log: "Dusk. Lay up at Witla, as ordered." | *Half*: `Rest.Dark` and `Sight.MoonLit` are built. `Rest` today is a key that hurries the night while she sails on. Lying up is `VoyagePlanner` steering for a landing and setting `ship.anchored`. A touch is `Aground.Offer` |
| **Sail** | full / reefed / down; and a rule: shorten at a fresh wind / at a strong wind / never | Full in a strong wind is the fastest, and each hour is 2% to hurt the sail or a strake (depth page). Reefed: about seven tenths of her speed (g), and she keeps sailing one step of wind longer before she must lie to. It needs `reef_bands` (12 d, a day at a yard: `ship_upgrades.md`). Down: she rows or lies to and nothing breaks | Window: this is the switch the window is for. The sail is full, shortened or on the yard. Chart: her mark's sail is large or small. Log: "Reefed at noon. She makes four knots." | *Half*: `Travel.Speed(cls, course, windFrom, wind, out sheltering)` gives her speed and when she lies to. Add the sail's factor there. `ChartBody` calls it each frame. Hurting the sail needs the missing verb |
| **In a calm** (or a foul wind) | row / drift / tow (upriver) | Row: she keeps moving; after four hours the men are weary (g). Drift: the men rest and the hours go; a river carries her down, a tide carries her both ways. Tow: needs `tow_gear` (15 d), a walking pace upriver | Window: oars out and four backs, or the sail hanging "like washing" (the deck's own words). Log: "Calm from noon. Rowed till dusk." | *Half*: `Travel.Speed` already rows the world's ships in a calm; `VoyageRules.rowHealth` and `rowMorale` exist. The switch is missing |

**Signs: how the leg asks.** An order that covers the case is obeyed without a word, and only the log speaks. The
clock stops for a sign when the order cannot be kept, when its cost has changed, or when the man in the seat speaks
against it. One line in the portrait, one switch lit in Voyage. **As ordered** is Space. Otherwise flip the switch.

| Sign | Read from | Lights | Said by |
|---|---|---|---|
| Dusk in an hour and no landing in reach | `Sky`, `Port.Nearest` | At dusk; Watches | the helmsman |
| The wind has turned foul for the way, or risen a step, or died | `WorldSim.WeatherAt`, looked at six hours ahead along `Course.Points` | Way, Sail, In a calm | the weather-wise man |
| A sail made out | `SeaWatch.MadeOut` | Hail, Hold course, Stand in | the lookout |
| A bar or shoal ahead on a falling tide, and she is too deep | `TideWait.ShallowRunAhead`, `Course.LegWater`, `SailingShip.NeedsDepth` | Wait for the flood, lighten her, go round | the leadsman |
| Bread for fewer days than the way left | `Voyage.FoodDays`, `Course.KmLeft` | Rations; Put in | any man |

No more than four signs in a game day (g), and never the same one twice in six hours. This is the answer to the
critic's sum that a London crossing needs eight cards: it needs two cards and three signs, and the signs are free.

### 2.2 Loading: what low, what dry, how deep

The hold half of Trade. Today the hold is one number (`Hold.Used` of `Hold.capacity`), and the critic is right that
there is nothing to fit.

| Control | The choice | What it costs | Log, chart, window | Code |
|---|---|---|---|---|
| **Three places.** Press a lot and it moves to the next | **Low**, on the bottom boards. **High**, on top amidships. **Dry**, under the hide fore and aft: room for two lots (g), four with `hold_cover` (20 d) | Heavy things low (querns, casks, salt, iron) make her stiff: she carries sail a step of wind longer. Heavy things high make her tender: she lies to a step sooner and loses lots twice as often in a gale. Cloth, grain, bread and fur spoil low in any sea over 1.2 m unless `dunnage` (1 d) is laid. Nothing spoils in Dry | Window: casks stand above the rail when stowed high. Log: "Sea in the bilge. A bale of cloth spoiled." | *Missing*: a place on `Lot`. `Hold.Lots`, `Hold.Weight` and `Voyage.Spoil(share)` are built |
| **The load line** | light / fair / deep | Deep: up to a quarter over (825 kg in a 660 kg boat (g)). Half a knot slower. She draws a hand more, so bars and flats want more tide. In a sea she ships water. Light: over the bar at half tide and quick, but tender unless `ballast` (1 d, a tenth of the room) is in her | Window: she sits a plank lower. Chart: every bar and flat she cannot cross at this hour is hatched, and the way's days change. Log: "Loaded deep: 792 kg. The bar at Walichrum only on the flood." | *Half*: `SailingShip.draught`, `NeedsDepth`, `TideWait.Needs` and `Course.LegWater` are built. Draught from weight, and loading past `capacity`, are missing |
| **First over the side** | Mark one lot | In a gale each hour is 6% a lot lost (depth page). Unmarked, the cheapest goes first | Log: "A cask of wine went over the side: 47 d." | *Missing*, with the gale rule |
| **Stow again** at sea | The same presses | An hour lying to | | |

Loading in port takes two hours, or half an hour with carriers (`town_interactivity.md` 4).
"Hold: 654 of 660 kg of room" reads as empty. Say "Hold: full. Room for one cask more if you load her deep."

### 2.3 Dealing: haggle, credit, gift, part-exchange

| Control | The choice | What it costs | How it shows | Code |
|---|---|---|---|---|
| **Haggle**, once a call | Press a note in your ledger to name that port's price | Half an hour. A note you saw yourself, under 14 days old, moves the price a third of the way to it, 8% at most. Any other note under 30 days: 4%. Older, or none: a bluff, 3 chances in 10; failed, the price is 5% worse and he likes you less. If his port has newer word than yours, he says so: a free fact (`depth_design.md` 2.1) | Log: "Named Lundenwic's price, six days old. He came down 3 d a cask." Portrait: his face changes | *Half*: `Known.Price` gives age, how and sureness; `Deal.AsksFor` and `PaysFor` take a `who`; `WorldSim.Board(port)` knows his port's word. The act is missing |
| **For silver / for wares** | A switch on the sale line | For wares: what you take from this port is bought with no gap between his buying and selling price (`Market.Spread`). What is unspent at cast off is paid in silver at the usual price. You sail with goods, not coin | The hold shows the return cargo as you choose it. Log: "Sold for wares: four casks against 30 ells of cloth." | *Missing*: credit held at a port until cast off |
| **On the tally** | Take goods now and pay on a named day. No interest: the ban on usury stands | Nothing in silver. A day goes on the day rule with his name. How much he trusts you comes from your standing there: nothing at a strange port, a cask or so where you are known, more with your host as surety. Missed, your standing falls there and the news travels | Day rule: a new flag. Tasks: a new row. Log: "Two casks on the tally from the vintner. Due by Lammas." | *Half*: `Trade.Debt`, `DebtDue`, `Creditor` and `PayDebt` hold one debt. A **list of dues** (who, where, how much, what day) is missing. The count's 480 d is its first row |
| **Gift** | To a person, from the hold or the purse | One gift a person a year counts. The floor is his rank: 6, 24, 60 or 240 d. It wins his goodwill, a fourth talk, and at a hall the right to deal at all (depth page) | Portrait: his face. People: his row. Log: "Gave the toll-keeper a cask." | *Half*: `NpcRegistry`, `Reputation.Deed`. The king's men already take a gift: `Patrols.Answer`, `giftShare`, `giftMinD`. A gift to a person is missing |
| **The gain line** | Nothing to press | Before any Buy: "Four casks to Lundenwic: about 150 d, 2 to 4 days" | On the row, and in the hint line | *Half*: section 1.3, Where it pays |

The list of dues is the one new record here that pays three times: it is the debt, the tally and the flags on the
day rule.

### 2.4 The crew: rations, watches, pay, seats

Four switches at the foot of People.

| Control | Positions | What it costs | How it shows | Code |
|---|---|---|---|---|
| **Rations** | full / short / a feast | Short: bread lasts half as long again; heart falls each day and after three days the oars give less (g). A feast (in port, or a saint's day): two days' bread and a little ale in one day; it mends heart | Bread line on the day rule grows or shrinks at once. Window: the men's posture. Log: "Short rations, second day." | *Half*: `VoyageRules.foodKg` (0.7), `drinkL` (3), `hungryPerDay`, `Voyage.Day`, `Voyage.FoodDays`. A ration factor is missing |
| **Watches**, for nights at sea | all hands / watch and watch / one man | All hands: a sail is made out a third further off and the night risk is halved; the men tire. One man: the rest sleep; a sail is made out late; the risk is half as much again (g) | Window: who is awake. Chart: the ring in which a sail is made out is wider or narrower | *Half*: `SeaWatch.madeOutKm`, `Sight.RangeKm`. Weariness is missing |
| **Pay** | by the day / at the home quay / a share | By the day: 0.25 d a man; a man unpaid 14 days leaves (`leaveIfOwedDays`). At the home quay: the purse stays whole abroad; they grumble after 14 days. A share of the run's gain in place of wages (how large, and whether the period knew it for a crew: to check): a bad run costs you nothing, a good one costs more, and a man on shares speaks against every wait | People: pay owed on each row. Key **Pay them now**. Log: "Paid the men 4 d at Lundenwic." | *Half*: `VoyageRules.pay`, `Voyage.PayOwed`, `unpaidPerDay`. The choice is missing |
| **Seats** | Press a man, press a seat: helm, lookout, lead, weather | The man in a seat speaks its sign, and is right more or less often by his craft (depth page 2.4). An empty seat is a sign nobody speaks | The four heads under the portrait sit in seat order. Window: the man at the steering oar | *Missing*: the four records |

### 2.5 What you tell a captain you send off **L**

The same five switches, with a cargo and a day in front. The fleets plan (section 4) already has the sentence:

> **Folkmar** carries **wine** to **Lundenwic**, brings **wool cloth** home, back by **Lammas**. He goes **by the
> coast**, **lies up at dusk**, **shelters in any gale**.

| | |
|---|---|
| The control | Press a word. A strip of at most six other words opens: only runs you have crossed three times, goods seen there, feast days. The last three words are your own order switches, so nothing new is learned |
| Where | Face to face at one quay, or on the winter page. Never by pressing his mark on the chart |
| The cost | His share (a third to kin, a half to a partner). The days until you hear: about 6 for Lundenwic, 13 for Hedeby, 35 for Birka. He may do otherwise, and says why |
| How it shows | Chart: his mark is solid when seen today, hollow with its age to 30 days, faint beyond or past his day. Day rule: his due notch, red when it passes with no boat. Log: one line with the reason when he is home. Window: never; the window is your own boat |
| Code | He is a `SimShip`. `TraderBrain.Plan` chooses for him and `WorldSim.SetSail` sails him. *Missing*: the order as a fence round `Plan`, and who owns a ship. Word of him is `Known.Sighted` with its age (`shipSeenHalfLife`), built for strangers' sails |

This is how the frame grows from one slow boat to a house of ships with no fleet screen: the player has pressed these
switches a hundred times on his own boat before he gives them to Folkmar.

## 3. The chart as a visual key

### 3.1 What it shows with no click, whatever panel is open

| Thing | Drawn as | From |
|---|---|---|
| **Her** | A hull with a Byzantium sail, 16 pixels at least, pointing the way she heads, with a short wake. A small sail when reefed, none when lying to | `ChartBody`, the order. Today she is six white pixels |
| **Her course** | A bold line. A tick where each dawn finds her. A dark bar where night falls on it | `Course.Points`, the measure |
| **How far today** | A pale fan ahead of her, to where dusk finds her in this wind | `Travel.Speed`, `ChartBody.Wind` |
| **Wind** | Streaks on the water that drift with it; longer is stronger; a gale is hatched dark | `WorldSim.WeatherAt` on a coarse grid |
| **Tide** | At each river mouth and bar on her way: an arrow in or out; a hatched bar when she cannot cross | *Missing in the chart* (2.1). `TideWait.ShallowRunAhead` is ready for it |
| **What is known, and how old** | A port's name in full ink when you saw it within 14 days, grey to 30, faint beyond. A hollow ring for a port only heard of. Fog on water never sailed | `Known.PricesAge`, `Known.IsHeardOf`, `Exploration`; the 14 and 30 are in `KnowingRules` |
| **Other sails** | A grey sail: sighted. With her kind and home: made out. A dark sail with a number of days: raiders, as old as the telling | `SeaWatch.Of`, `Known.RaidersSeen` |
| **Danger** | A raided port struck through. A watch station's ring. Gale hatch. Flats that dry | `WorldSim.RaidedUntil`, `PatrolStation.radiusKm`, the lanes' `Flats` |
| **Where tasks point** | A flag at the port with the days left: "480 d, 100 days" at Dorestat | `Trade.DebtDue`, `Commissions.Mine` |
| **Scale** | A bar that reads "a day's sail in this wind", not "20 km" | the measure |

No panel ever lies on her mark. The sea goes back to the blue of `MapArt/world/overview.png`, as both critics ask:
streaks, hatch and ink need a quiet ground. Byzantium is kept for what is yours.

### 3.2 The lens: the panel asks, the chart answers

| Panel in focus | The chart adds |
|---|---|
| Voyage | The ways to the picked port with ticks and night bars; streaks coloured fair or foul for that way; bars she cannot cross now |
| Trade, with a good picked | At every known port: pays well, fair or poorly as a mark, and the word's age in days ("wine, up, 6 d"). From `Known.Word(good, port)`: `GoodWord.tilt` and `age` |
| People | Where each person you know lives; where you have a host; where you are not welcome (`Reputation.At`) |
| Tasks | The flags stand out; Show draws the way and measures it |
| Chest | Your pins and notes; the place a text or a riddle names |
| Event | A ring at the place the card is about |
| Folded | Only the layer of 3.1 |

### 3.3 What the measure in days does

1. Press **Measure**, press Lundenwic. The way by water is drawn from her. (A port picked in Voyage is measured
   without the press.)
2. On the day rule a pale stretch runs from today to the short count, and a paler one on to the long count. A dark
   bar marks each night.
3. Everything already on the rule is now read against it. The bread line ends inside the stretch or beyond it. A
   Sunday falls in it or not. Michaelmas is so many such runs away.
4. The hint line says it once: "By the coast, 3 to 4 days. Two nights. Bread lasts. You would come in on Monday."
5. Press two places and it measures between them: the run after this one.

So the measure is never a number alone. It is a length of your own calendar.

## 4. One port call and one sea leg, press by press

The words the frame answers with are placeholders; the wording is yours. Prices are the player critic's sums from
the tables, not a run. The start is as `ChartWorld.Start` makes it: four casks of wine, a pair of querns, bread
for 7 days, 86 d, 480 d owed. 21 June 793 was a Friday (my sum from the Julian calendar; check it), so the 23rd is a
Sunday.

### 4.1 Dorestat, the first morning: about a minute and a half

| # | You press | The frame answers | s |
|---|---|---|---|
| 1 | Nothing. New game | The Event lamp is lit and the card is in focus: **The tally**. Portrait: the count's steward. Three choices, each with its cost: "Take your half and say you will be there." "Leave twelve pennies for the count's table, as the custom is": 12 d, he likes you more. "Say the boat was never worth two pounds": he likes you less | 15 |
| 2 | **1** | The outcome line, then Go on. Log: "Dawn. Took the count's tally: 480 d at Michaelmas." A flag at the far end of the day rule: "Michaelmas, 100 days". Tasks appears in the rack with a count of 1. People takes the focus | 4 |
| 3 | **Talk** on "A skipper in from Lundenwic", then **Plain** | Portrait: the skipper. Talks: two pips left. Log: "Morning. A skipper six days out of Lundenwic: wine pays well there." Chart: Lundenwic's name inks in | 10 |
| 4 | **Talk** on "The keeper of the port", then **Plain** | "The ebb begins in two hours. Two Frisians left for Lundenwic yesterday with wine." Chart: two grey sails at the river mouth, marked a day old. One pip left; you keep it | 8 |
| 5 | **2** (Trade), then the row **Rhenish wine** | The word on the row. Under it, Where it pays: "Lundenwic: pays well, 6 days old, 2 to 4 days, about 39 d a cask. Traiectum: seen today, half a day, a loss of 9 d a cask." Chart: every known port wears its wine mark | 10 |
| 6 | **Buy 1** | Silver 86 d, then 39 d. Five casks. The load line moves from fair to **deep**. Hint: "Deep: half a knot slower. The bar at Walichrum only on the flood." Window: she sits a plank lower | 6 |
| 7 | The **querns** (to Low), the **bread** (to Dry), the corner of the fifth cask (**first over the side**) | Each lot moves. Hint: "Stiff. Bread dry." | 8 |
| 8 | **1** (Voyage), then the row **Lundenwic** | Three ways drawn on the chart with day ticks: inside 4 to 5 days, by the coast 3 to 4, across 2 to 3 with two nights at sea | 5 |
| 9 | Way: **By the coast**. Cast off: **On the ebb**. At dusk: **Lie up**. Sail: **Full**, shorten at **strong**. In a calm: **Row** | "Reefed" is greyed: "No reef bands. 12 d at a yard." Read back: "On the ebb, by the coast to Lundenwic, lying up at dusk. 3 to 4 days. Bread for 7. Wages 3 to 4 d. Five casks: about 190 d if the word holds." Day rule: the pale stretch runs to Monday or Tuesday, past the Sunday notch. Had you chosen across, it would end on the Sunday: "The market is shut." | 20 |
| 10 | **Cast off on the ebb** (Enter) | The two hours pass in two seconds. Log: "Cast off on the ebb for Lundenwic, by the coast. Five casks, deep." Window: oars out, the dam heads slide by | 4 |

Ten steps, about 90 seconds. Every one of the five beats' first two (hear, choose) was a press with a cost on it.
A second call at the same port is quicker: no card, the presets, about 40 seconds.

### 4.2 Dorestat to Lundenwic by the coast: about two minutes

Sail on runs an hour a second. Friday 09:00 to Monday 13:00 is 76 game hours: 76 seconds of her mark moving, and
five stops.

| Game time | What happens | You press | s |
|---|---|---|---|
| Fri 09:00 | She runs down the Lek on the ebb. Ticks pass under her. The streaks say west, light | **Sail on** (Space) | 1 |
| Fri 19:30 | No stop: the order covers it. Log: "Dusk. Lay up at Witla, as ordered." A fire on the chart. The night passes | | 0 |
| Sat 05:30 | **Sign**, the weather seat: "The wind has backed south-west and risen. Foul for the coast to Walichrum." In a calm and Way light. Three keys: **Beat on** (half a day more), **Row under the dunes** (no time lost, the men weary by noon), **Back to Witla and wait** (a day; you would come in on Tuesday) | **Beat on** | 8 |
| Sat 14:00 | **Card**, `sea.wreck.survivors`: "Two men and a boy stand on the last dry hump of the bank... They do not wave. They have been waving since yesterday." Its three choices, each now with a cost line: take them off (an hour lying to; three more mouths as far as Walichrum); take them off and keep the strongest as a hand (a fifth man at the oars and at the bread); take them off for what they saved from her. The card as written has no way to refuse and charges nothing: it needs the hour and the bread | **2** | 12 |
| Sat 19:30 | **Sign**, the helm: the order cannot be kept. "Dusk in an hour. No landing you know short of Walichrum, three hours on." At dusk and Watches light. Keys: **Sail on to Walichrum in the dark** (three dark hours by a coast, doubtful with one man on watch, likely with all hands), **Anchor off the dunes** (an onshore wind and a stone anchor: doubtful) | **All hands**, then **Sail on** | 8 |
| Sat 22:30 | Log: "Walichrum by night. Two went ashore. The third stayed at an oar." She lies till first light | | 0 |
| Sun 03:30 | **Sign**, the lead: "The bar. She is a hand too deep for this water. The flood makes in three hours." Keys: **Wait for the flood** (3 hours), **Put the marked cask over** (47 d), **Go round by the deep channel** (5 hours). This is the fifth cask from the quay | **Wait** | 8 |
| Sun 15:00 | **Sign**, the lookout: "A sail to the north-west: low, long, no cargo showing." Keys: **Hold your course** (likely fine), **Stand in for the shoals** (greyed: "She draws too much, loaded deep"), **Hail her** (doubtful), and Sail: **Carry everything** to outrun her (each hour may hurt the sail) | **Hold your course** | 8 |
| Sun night | No stop: watches are as set. Log: "Night at sea. She altered away at dusk." | | 0 |
| Mon 07:00 | No stop: the order waits for the tide. Log: "Rowed up the Thames on the flood." | | 0 |
| Mon 13:00 | Moored. **The reveal**, the newest log line: "Two Frisians came in on Saturday with wine. It pays a fifth less than your word said." They are the two sails the keeper told you of | | 5 |
| | The market is in focus | **Rhenish wine**, **Sell all** | 6 |
| | **The reckoning**: "Dorestat to Lundenwic, 3 days. Five casks sold for 345 d. They cost 235 d at Dorestat's price. Wages 3 d. Bread for 3 days left. Made about 105 d. Your word was six days old when you bought and nine when you sold." | | 5 |

**The sum.** The leg is 76 seconds running and 45 seconds deciding: two minutes from cast off to mooring. The sale
and the reckoning add 16 seconds. Straight across in the same wind is about 50 hours and three stops: 75 seconds,
and you land on the Sunday. Dorestat to
Traiectum is six seconds and wants one sign at most.

**What made each stop a decision.** The foul wind cost half a day because of the way chosen at the quay. The bar
cost three hours because of the fifth cask. Stand in was shut because of the fifth cask. The reveal was the keeper's
two sails. Nothing was a coin flip, and nothing was "(sure)".

## 5. Dead weight: what should go

| Go | Why |
|---|---|
| Tabs **and** buttons | Two switches for one place. One rack |
| The buttons Ledger, Hold, Crew, Chronicle, World | Ledger and Hold are Trade. Crew is People. Chronicle is a page of the Chest. World is ink on the chart and lines in the chronicle |
| Your own face in the portrait during play | It tells you nothing. Show who is speaking |
| "Prime", "Terce", "third hour" | The period labels you called obnoxious. Dawn, morning, noon, afternoon, dusk, night |
| "Weigh silver", "Exchange", four coins | One purse in the first release (both critics). Exchange today costs a tenth for a curious click |
| The port's numbered menu over the chart (`TextPanel`) | It lies on her mark and on the ports south of her. People, Trade and Voyage are the menu |
| "Explore the town and markets" as a step, and "Set course" then "Cast off" in two places | One press picks the port; Cast off is in the same panel |
| `MapTable`'s outliner and "The waters round her" box | They say the course twice (18 km and 20 km in one capture) and go stale after she moors |
| Kilometres and per cents anywhere a player reads | "18 km to go", "20 km", "health 100%, heart 80%". Days and words |
| Pause, 1x, 2x, 4x as four buttons | Three keys: Hold, Watch, Sail on |
| "No alerts" | A button that says nothing is there. The Event lamp, dark |
| String (S), K for colours | String is a side game; K is a developer's key |
| "(sure)" on choices | Every label wears it, even where nothing is risked |
| The ship window's clicks on a man, a far sail and a sea-mark, and its three views | One press: it opens Ship. Until the 3D stage is attached, the stand-in |
| The mock's Layout, Buttons, Size, Colours and Frames rows | Choose G, thick rounded, one colour set, and stop. That is the fifth interface not built |
| Store goods, grades, named buyers, claims | Good, and later. Not before the hour holds |
| A day rule with every feast on it | Flags only for what bears on you |

## 6. For the master

**The order I would wire the hands in** (it sits inside the designer critic's stage 1):

1. The `Frame` with Voyage, Trade, People, Tasks and Event holding what exists. Nothing on her mark.
2. The kept log, and the leg's record with the reveal and the reckoning.
3. The measure: the short and long count from `Travel.Speed`, laid on the day rule.
4. Sail on, with its stops.
5. Three talks, by splitting `PortLife.Talk`.
6. The order: the three ways first (`Course.Profile`), then dusk, then the cast-off waits. Sail and calm when the
   window can show them.
7. The signs: dusk, the wind, a sail, the bar.
8. The load line and the three places. Then the list of dues and the tally.

**Yours to rule on:**

1. Sail on at an hour a second: is it inside "no time warps", as a wait already is?
2. The portrait never shows you in play. Yes?
3. The card sits in the focus place on the left (layout G), not under the face (`depth_design.md`). Yes?
4. A crew on shares in place of wages: wanted, and true to the period?
