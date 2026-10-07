# The word against where she lies, deals by the lot, and the ledger

7 October 2026. For the builders. It answers the strategy critic (Design/critique/review_systems.md): his cons 3, 4
and 15, and the first three parts of his fourth change ("a ledger of goods by known ports, buying and selling by the
lot with one settlement, the word said against where she lies and 'about 36 d' for a first-hand telling"). The fourth
part of that change, "the days if today's wind holds", is not here: it is the measure's.

**What was read.** `Assets/Kapman/Code/Game/Known` (Known.cs, Word.cs, PriceNotes.cs, Lore.cs), `Game/Trade` (Deal.cs,
Purse.cs, Hold.cs, Market.cs, Trade.cs, TradeRules.cs), `Game/Data/Wording.cs`, `Game/UI/WordLine.cs`, the frame's
`FramePanel.cs`, `FrameGame.cs`, the tick and the top bar of `FrameRoot.cs`, the panels `TradePanel.cs`, `WordPanel.cs`,
`HoldPanel.cs`; the tables `knowing_793.json`, `trade_rules.json`, the "word" part of `wording_793.json`; the tests
`KnownWordTests.cs`, `TradeTests.cs`, `FramePanelTests.cs` and the trading steps of `ChartGameTests.cs`; the rulings
of 7 October (DECISIONS.md 1 to 26), QUEUE.md, `Design/fixes/brief_faces.md`, and section 6 of
`Design/research_trade_seasons_and_hints.md` (where the word on the item was designed).

**What was done.** Nothing under Assets was changed. Unity was not run and git was not used. All the code below was
compiled outside Unity with the editor's own compiler (Roslyn, from `Kapman.Game.rsp`, the sources taken fresh from
disk at 13:00): the game's assembly with the mended files and the new file in place, **0 errors, 0 new warnings**
(one old warning goes: `TradePanel.Shown(Market)` no longer hides `FramePanel.Shown`); the EditMode tests with the
mended and the new test file against that assembly, 0 errors; the PlayMode tests against it, 0 errors; and the two
brief faces against a stand-in for the base class that `brief_faces.md` drafts, 0 errors. Every diff below was
applied to a copy of the files with `patch` and gave, byte for byte, the text that compiled. The purse's figures
come from a Python copy of `Purse.Pay` and `Purse.Receive` as they are and as mended.

**What was not done.** No test was run (they need Unity). No window was seen on a screen: the stylesheet lines are a
draft. See section 9.

**The files this touches** (as they were at 13:00; `FrameRoot.cs` and `ChartGameTests.cs` moved under me while I worked, so find their
places by their text, not their numbers):

| File | What | How it is given below |
|---|---|---|
| `Assets/Kapman/Code/Game/Known/Known.cs` | two rules of knowing | diff |
| `Assets/Kapman/Code/Game/Known/Word.cs` | the word against here; the ledger's cell | diff |
| `Assets/Kapman/Code/Game/Data/Wording.cs` | the built-in words | diff |
| `Assets/Kapman/Code/Game/Trade/Purse.cs` | no rounding to the whole coin | diff |
| `Assets/Kapman/Code/Game/Trade/Deal.cs` | a quote for a lot | diff |
| `Assets/Kapman/Code/Game/Trade/Hold.cs` | what a lot cost, without taking it out | diff |
| `Assets/Kapman/Code/Game/UI/Frame/FrameDeal.cs` | NEW: the lot, and seeing the market on mooring | whole file |
| `Assets/Kapman/Code/Game/UI/Frame/Panels/TradePanel.cs` | lots; figures that change in place | whole file |
| `Assets/Kapman/Code/Game/UI/Frame/Panels/WordPanel.cs` | the ledger | whole file |
| `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs` | two small places | find and replace |
| `Assets/Kapman/Game/Data/knowing_793.json`, `wording_793.json` | two numbers; the new lines | the lines to type |
| `Assets/Kapman/Game/UI/Frame.uss` | the lots' buttons; the ledger | draft to append |
| `Assets/Kapman/Tests/EditMode/KnownWordTests.cs` | five tests follow the new rule | diff |
| `Assets/Kapman/Tests/EditMode/WordAndLotsTests.cs` | NEW: 13 tests | whole file |
| `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs` | one check added | lines to add |

## 0. In one page

**The word.** Today the line under a good says a market "pays well", "the usual" or "poorly" by setting its price
against the good's value in the goods table, which the player never sees. The mend: it is said against the price
she has in front of her. That is the price she saw at the market where she lies; at sea, or where she has not looked
at the market, the last price of that good she saw with her own eyes. How exactly it is said goes by how she knows
it, in three steps:

| How she knows it | In a line | In a cell of the ledger |
|---|---|---|
| Her own eyes, within the week | the figure, and the difference as a figure: "You saw Rhenish wine pay 36 d at Traiectum, yesterday. That is 2 d more than here." | **36 d** |
| A man who saw it told her (within a fortnight), or her own eyes more than a week ago | "about" and a rough figure, and a word: "A Frisian skipper said Lundenwic was paying about 60 d for Rhenish wine, a few days back. That is far more than here." | about 60 d |
| Talk through more mouths, a rumour, a telling grown old | a word only: "They say at Quentovic that Lundenwic pays far more than here for Rhenish wine. A few days back." | *far more* |

Every line and every cell carries its age in words, as now. The words are five: far more, more, much the same,
less, far less. "Much the same" is within a twentieth. "Far more" is not a free number: it begins where buying the
good here and selling it there would pay, the market's gap and the toll there counted (36 in the hundred into a port
that takes a tenth, 22 where no toll is taken). So the strongest word the game says about a price means something
a trader can act on. The small wedge beside the line now points up when they pay more there than here.

**The lot.** A good's row has three buttons a side: one ("Buy", "Sell"), half of what can go, all that can go. Half
and all carry their count and their sum ("2 for 65 d", "all 4 for 121 d"). Each is one deal and one settlement. The
engine could always do this (`Deal.Buy` and `Deal.Sell` take a count and ask the purse once); only the frame dealt
one unit a click. And the purse no longer rounds to the whole coin: what is under one coin is weighed out in cut
silver, in change and in takings.

**The ledger.** The full face of The word: goods down, known ports across, the port she lies at first; each cell a
price by the three steps above and under it the age of the word, paler as it ages; one look for what they pay and
one for what they ask; all in one reckoning; a pressed cell says its whole line.

**One thing here changes a rule, and it is the user's to say.** The word on the item was built on "the number is
there only if he saw it himself" (research_trade_seasons_and_hints.md, section 6, which the ruling of 4 October
night names as the build). The critic asks for "about 36 d" where a man who saw it tells her. I have built that, and
put it behind one switch in `knowing_793.json` (`"told_figure": true`). Set it to `false` and a telling at first
hand is a word again, like all talk; everything else here stands. The draft has it ON because the user's word of
today is to move the critics' scores and this is the critic's own sentence. **Ask the user.**

## 1. What is wrong today, exactly

### 1.1 The word

- `Known/Word.cs` lines 241 to 246, `PaysKey`: the ratio is what the market pays over `good.value`; 1.15 or more is
  "pays.well", 0.85 or less is "pays.poorly", else "pays.fair" (the numbers: `knowing_793.json`, "word",
  `pays_well` and `pays_poorly`; `Known.cs` line 81).
- It is used in one place: `Word.cs` line 426, in `LedgerWord`, for every price that is noted (rules 2, 3 and 4).
  The same call gives the line its wedge (`tilt`).
- Rhenish wine has `"value": 60` (goods_793.json). So any port paying 51 d or less "pays poorly" and none under
  69 d "pays well". In the captures home pays 34 d and Traiectum 36 d: both are "poorly" by the table; the player is
  told so only of Traiectum, with a wedge pointing down, and Traiectum pays more than where she stands.
- Only a price seen in the last 7 days shows a number (`Word.cs` lines 427 to 432), and it shows it in that port's
  own reckoning (pence at Lundenwic, grams at Ripa), so two figures on one screen cannot be set side by side.

### 1.2 The lot, and the purse

- `Panels/TradePanel.cs` lines 111 and 112: one Buy and one Sell button a row, both calling `DealOne` (lines 126 to
  139), which calls `Trade.Buy(market, good, 1, ...)` or `Trade.Sell(..., 1, ...)`.
- `Trade/Deal.cs`: `Buy` (lines 50 to 88) and `Sell` (91 to 124) already take a count, move the price unit by unit
  inside it, and ask the purse once (`purse.Pay(total, ...)` line 77; `purse.Receive(total - toll, ...)` line 117).
  The other ships deal five at a time through them. **So dealing by the lot needs nothing new in the engine.**
- `Trade/Purse.cs`, `Pay` (lines 91 to 138): line 114 takes whole coins rounded UP
  (`Math.Ceiling(left / rate - 1e-9)`); line 129 gives change in whole coins rounded DOWN (`Math.Floor(back + 1e-6)`).
  `Receive` (lines 154 to 163): line 158 rounds what is taken to the NEAREST coin.
- The crew's pay goes through the same `Pay` at every call (`Voyage/Voyage.cs` lines 291 and 292), and so does the
  other ships' (`Sim/WorldSim.cs` lines 449 and 450).

What that costs, from the Python copy (frankish zone, coin the denier, other silver at a tenth less):

| Case | As it is | Mended |
|---|---|---|
| Four sacks at 5.2 d (20.8 d), one click each, from deniers | costs 24 d | costs 20.95 d |
| The same four as one lot | costs 21 d | costs 20.84 d |
| The crew's pay, 65 calls of 0.5 d (32.5 d), from the starting purse | costs 55.9 d of what the purse is worth here | costs 35.4 d |
| A sale of 36 d less 3.6 d toll (32.4 d) | takes 32 deniers | takes 32 deniers and 0.6 g of cut silver |
| A sale of 36.4 d less 3.64 d (32.76 d) | takes 33 deniers | takes 32 deniers and 1.2 g |
| Forty single sales of 3.87 d (154.8 d) | take 160 d | take 149.7 d of worth here |
| The same forty as one lot | takes 155 d | takes 154.7 d of worth here |

Two things the critic did not say. First, his sum for the pay was "about 30 d" lost in a season; the copy says 23 d
(55.9 less 32.5), because the purse runs out of deniers and goes on to pennies. Second, the rounding is not always
against her: a sale rounds to the nearest coin, so forty sacks sold one at a time at 3.87 d bring 160 d where the lot
brings 155. Today a careful player gains by selling singly when the fraction is over a half and by the lot when it
is under. The mend shuts that too: one lot is now no worse than the same units one by one (to a tenth of a gram).

The 2.9 d the mended pay still costs over 32.5 d is the rule of the realm, not rounding: the starting purse pays
small sums from its 30 g of cut silver, and cut silver is worth a tenth less in the king's lands.

### 1.3 The ledger

- `Panels/WordPanel.cs`: `MostPorts = 16, MostGoods = 3` (line 18). The goods are the first three of
  `Trade.Hold.Contents` (lines 46 to 49): only what is aboard. With an empty hold it shows no price at all. Under
  each port it stacks up to three word lines; nothing says what a port ASKS, so "where is honey cheap?" cannot be
  asked before buying.
- The ports come freshest first, so a port's place moves from day to day.

### 1.4 What rests on the slow clock, and is written here for the new one (ruling 26)

1. **The window was built again whenever a price moved.** `TradePanel.Stamp` (lines 36 to 52) folds in each good's
   stock to the unit and its price to the quarter-penny. That was written when an open panel held the clock (the
   rule of 4 October), and then for a clock whose fastest speed was a day in six minutes. Now windows never hold
   the clock (ruling 5), a day can pass in four seconds (ruling 26, speed 5), markets settle a twentieth of the way
   to their usual stock each day and 138 other ships deal in them. The stamp would move at most looks, the frame
   looks four times a second, and a click that falls across a rebuild is lost (the button under the pointer is a
   new one). **Mend: the Trade window is built only when its rows change; its figures change in place** (`Live`).
2. **One click a unit, with the clock running.** Forty sacks are forty clicks while the price moves under her and
   game days pass. A lot is one click, its sum on the button is the sum at that look, and the count is worked out
   again at the click.
3. **The market was "seen" only while the Trade window was drawn** (`TradePanel.Fill` lines 81 to 90). With brief
   faces (ruling 9) the full face is shut most of the time, and with the clock dropping back at a port and then
   set on again she may lie there a day without opening it. The word needs its "here" from the moment she moors.
   **Mend: the frame notes the market when she moors, once each day she lies there, and after every deal**
   (`FrameDeal.See`, called from the frame's own tick).
4. **The word ages fast in real time.** Section 6 of the hints research was written "at 24 real minutes a game
   day". At speed 4 a price seen is "within the week" for 105 seconds of sailing, and Dorestat to Lundenwic (2 to
   5 and a half days) is half a minute to a minute and a half. Everything that counts time counts game time (ruling
   26), so the days of the rules stay as they are (seen 7, told 14, known 90): they are about how fast a market
   moves, not about the player's patience. What changes is that the age must be loud: it is under every cell, and
   the cell pales with it.
5. **The Word window was built again every game day** (`WordPanel.Stamp` line 25). At speed 5 that is every four
   seconds, for a grid of some hundreds of cells. The ledger is built again when a port's newest word passes into
   another age word ("yesterday" to "two days back"), not with every day.
6. **Not mended here, only said:** while she lies in a port a new price note is kept each day
   (`PriceNotes.cs`, `Ledger.Note`, lines 64 to 74), each with all 135 goods, and everything of the last year is
   kept (`Ledger.Thin`, `keep.all_days` 365). At the slow clock nobody wintered at a quay with the clock running.
   At speed 5 a winter is about 150 notes for one port in the save. A later small mend: within one call keep the
   first note, one a week, and the last.

## 2. The mend, part one: the word

### 2.1 What it is said against (`Known.Here`)

In order:

1. She lies at a port and has seen its market: the price of the good there, as she last saw it there. (The frame
   notes the market when she moors, so this is today's.) The words say "here".
2. Else (at sea; or at a port whose market she has never looked at): the newest price of the good she saw with her
   own eyes anywhere, if it is younger than `known_days` (90). The words say "at Dorestat".
3. Else nothing. Then, and only then, the old words stand ("well", "the usual", "poorly" by the good's table value):
   a merchant with nothing of her own to set a price against has only what every merchant knows. In play this can
   hardly happen (she starts at a market); it keeps the tests of a bare ledger true.

It reads the price notes only. No market is asked, as before (asking a market brings it up to date).

A price at the port it is said against is not set against itself: "You saw Rhenish wine pay 34 d at Dorestat, this
morning." and no wedge.

### 2.2 The five words (`Known.Lean`, `Known.FarOver`)

With `r` = what they pay there over what they pay here:

| Word | When | Wedge |
|---|---|---|
| far more than here | `r` at or over 1 + far | up |
| more than here | `r` at or over 1.05 | up |
| much the same as here | between | none |
| less than here | `r` at or under 1 / 1.05 | down |
| far less than here | `r` at or under 1 / (1 + far of here) | down |

- 0.05 is `same_within` in `knowing_793.json` ("word").
- far = (1 + spread/2) / (1 - spread/2) / (1 - toll where it is sold) - 1, from `trade_rules.json`. With the spread
  of 0.20: 0.358 into a port of the Frankish realm (toll a tenth), 0.287 into an English port today (a twentieth;
  0.358 once the money fix makes it a tenth), 0.222 where silver is weighed (no toll). Why this number: she buys
  here at what they ASK, which is what they pay times 1.222; she sells there at what they pay less the toll; the
  two meet exactly at 1 + far. So "far more than here" says: buying it here to sell it there would pay, if the word
  still holds. "Far less" is the same the other way (buying there to sell here).
- The bands the critic faulted were 15 in the hundred wide against a table value. These are 5 wide against where
  she lies, and the wide step is the trade's own break-even.

His two cases under the new rule: Traiectum at 36 d against home at 34 d is "more than here" (and, both seen, "2 d
more than here"). After the money fix Dorestat asks 60 d, so pays about 49 d, and Lundenwic pays 59 d: "more than
here", not "far more", and rightly: 60 d laid out to take 59 d less the toll is a loss.

### 2.3 How exactly it is said (`Known.Cell`, one function for the line and for the ledger)

The note is chosen exactly as today (the newest own sight under 7 days; else the newest telling at first or second
hand under 14 days; else the newest of anything under 90 days). What is new is the grade:

- **Exact**: her own eyes, under `seen_days` (7). The figure, in the reckoning of the port it is set against.
- **About**: her own eyes, older (to 90 days); or told or written by one who saw it himself (`hops == 0`, `How.Told`
  or `How.Written`), under `told_days` (14), when `told_figure` is true. A rough figure: to the whole unit under 20,
  to the nearest 2 under 50, to the nearest 5 under 200, then to the nearest 10 (`Known.Rough`), always behind the
  word "about".
- **Word**: everything else that is noted: talk through one mouth or more, a rumour, a telling older than a
  fortnight.

In the live game a telling at first hand comes from a ship's master who saw the market (`Sim/Encounter.cs` line
209: `witness ? How.Told : How.Talk`; `Ports/PortLife.cs` line 77; the chest's riddle reward, `Chest.cs` line 597).
"Talk to people" on a waterfront is talk at one mouth (`KapmanGame.cs` line 375): a word.

### 2.4 One reckoning

A figure is shown in the reckoning of the port it is set against (`Known.Reckoning`): lying at Dorestat everything
is in d, Lundenwic's pence too; lying at Lundenwic everything is in p. With nothing to set it against, the port's
own, as today. The ledger says which in its first line.

### 2.5 The lines (built-in words; `wording_793.json` gets the same)

| Key | Line |
|---|---|
| `seen` (changed) | You saw {good} pay {price} at {port}, {age}. {Against} |
| `told.about` (new) | {Person} said {port} was paying about {price} for {good}, {age}. {Against} |
| `told` (as it was) | {Person} said {port} was paying {pays} for {good}. {Age}. |
| `saw` (changed) | You saw {port} paying about {price} for {good}, {age}. It may have changed. {Against} |
| `talk`, `talk.at`, `rumour` (as they were) | They say {port} pays {pays} for {good}. {Age}. |

`{pays}` is now "more than here" and its fellows (or, with nothing to set it against, "well" and its fellows).
`{Against}` is a whole sentence or nothing: "That is 2 d more than here." (both seen: the difference as a figure),
"That is far more than here." (one of them rough: a word). A line the user rewrites without `{Against}` simply
leaves the comparison out.

How they read:

- You saw Rhenish wine pay 36 d at Traiectum, yesterday. That is 2 d more than here.
- A Frisian skipper said Lundenwic was paying about 60 d for Rhenish wine, a few days back. That is far more than here.
- A boatman at Dorestat said Traiectum was paying more than here for Rhenish wine. A few days back.
- They say at Quentovic that Lundenwic pays far more than at Dorestat for Rhenish wine. A fortnight ago. (at sea)
- You saw Lundenwic paying about 60 d for Rhenish wine, last week. It may have changed. That is far more than here.
- There is a rumour that Ripa pays much the same as here for ship's bread.

These reach every place the word is shown without another change: the Trade window, the Voyage window's picked port
(`VoyagePanel.cs`, the line `body.Add(new WordLine(Known.Word(good, def, ask)));`: the capture 08 the critic quoted),
the Word window, the chart's port card (`Map/MapView.cs` line 631).

### 2.6 The tables (the builder types these: they are under Assets)

`Assets/Kapman/Game/Data/knowing_793.json`, the last line, gains two entries:

```json
  "word": { "news_days": 60, "seen_days": 7, "told_days": 14, "known_days": 90, "ahead_days": 3, "pays_well": 1.15, "pays_poorly": 0.85, "same_within": 0.05, "told_figure": true }
```

and in its `"_word"` note, in place of the sentence that begins "A told price is said as a word, never a number":

```
A price is said against the price where she lies (or where she last saw the good priced): within same_within of it 'much the same', beyond that 'more' or 'less', and 'far more' or 'far less' where carrying the good between the two would pay (the market's spread and the toll: worked out, not a number here). A price she saw within seen_days shows its figure; one told by a man who saw it himself, or her own older sight, a rough figure after 'about' (told_figure false: a telling is a word, like all talk); the rest a word. Only with no price of her own to set it against is it 'well' (pays_well times the good's table value or more), 'poorly' (pays_poorly or less) or 'the usual'.
```

`Assets/Kapman/Game/Data/wording_793.json`, in the top-level `"word"` object (it begins at line 603 today): change
the two lines `"seen"` (line 610) and `"saw"` (line 616), and add the rest (anywhere in the object; mind the commas:
the last entry of the object takes none):

```json
  "seen": "You saw {good} pay {price} at {port}, {age}. {Against}",
  "saw": "You saw {port} paying about {price} for {good}, {age}. It may have changed. {Against}",
  "told.about": "{Person} said {port} was paying about {price} for {good}, {age}. {Against}",
  "against.line": "That is {against}.",
  "against.far_more": "far more than {here}",
  "against.more": "more than {here}",
  "against.same": "much the same as {here}",
  "against.less": "less than {here}",
  "against.far_less": "far less than {here}",
  "against.by.more": "{diff} more than {here}",
  "against.by.less": "{diff} less than {here}",
  "here.lying": "here",
  "here.last": "at {place}",
  "cell.about": "about {price}",
  "cell.far_more": "far more",
  "cell.more": "more",
  "cell.same": "the same",
  "cell.less": "less",
  "cell.far_less": "far less",
  "cell.asks.far_more": "far dearer",
  "cell.asks.more": "dearer",
  "cell.asks.same": "the same",
  "cell.asks.less": "cheaper",
  "cell.asks.far_less": "far cheaper",
  "cell.none": "none",
  "cell.offer": "on offer",
```

and add to its `"slots"` note: `{against} a whole sentence setting the price against where you lie, or nothing;
{here} 'here', or 'at Dorestat' where you last saw the good priced; {diff} the difference between two prices you
saw; {pays} is now 'more than here' and its fellows`. The test `TheWordingTableHasEveryLineAndKeepsToldPricesInWords`
fails until every key above is in the table, by design.

## 3. The mend, part two: by the lot

### 3.1 The buttons

A good's row keeps its name, its quote and the word under it. At its right end stand two short rows of buttons:

- Sell (if any is aboard): **Sell**, **2 for 65 d**, **all 4 for 121 d**
- Buy (if any is on offer): **Buy**, **3 for 127 d**, **all 6 for 262 d**

"All" is all that can go: for selling, all of the good aboard; for buying, the least of what is on offer, what the
hold has room for, and what the silver pays (`Deal.QuoteBuy` asked for `int.MaxValue`). "Half" is half of that,
rounded up. Half is hidden where it would be the same as one or as all; all is hidden where only one can go. The
sums are what the market reckons (a sale's is before the toll, as "They pay 34 d" is; the toll is on the window's
second line and in the words after the deal).

The first button of each side still reads exactly "Buy" and "Sell" and still deals one, so the play test's clicks
(`ChartGameTests.cs`, the three `First(b => b.text == "Sell")` and `"Buy"` clicks at Dorestat and the one at
Traiectum) work unchanged. Where nothing can be bought the Buy button
still stands, and a press says why ("No room in the hold.", "Not enough silver."), as today.

The count is not kept on the button. A click asks the market again (`FrameDeal.DealLot`), so at a fast clock the
lot is the lot of that instant, and the words after it say what was truly dealt: "Bought 3 sacks of ship's bread
for 16 d: paid 16 deniers. Your silver ran out."

No keys are bound. The critic offered Shift and Ctrl; three plain buttons say more to a newcomer and need no
teaching. If keys are wanted later they call the same `DealLot`.

### 3.2 One settlement, and no rounding to the whole coin (`Purse.Pay`, `Purse.Receive`)

- **Paying.** Whole coins up to the sum. What is left under one coin is made up from the cut silver aboard if there
  is enough of it (weighed at what it is worth there). Only if there is not is one more coin handed over, and then
  the change under a coin comes back weighed out in cut silver, by weight, to the tenth of a gram and never over.
- **Taking.** Whole coins, and what is under one coin in cut silver, the same way. Nothing is rounded up.
- Where silver goes by weight (Ripa) nothing changes: it was always to the tenth of a gram.
- By weight she now loses at most a tenth of a gram (0.06 d) a settlement. In a land of coin the cut silver she is
  given is worth a tenth less there until she spends it where silver is weighed: that is the rule of the four
  coinages the critic asked to keep (his pro 15), now seen at work in the purse, and it is why one lot is better
  than the same units one by one (one small heap of cut silver in place of many).
- Change is weighed at the silver's weight, not at what it buys there. Weighed at what it buys there, every sum
  paid with a coin would hand back a ninth more silver than was due, to be carried to Ripa: a pump.
- It is the same purse for the player and the 138 other ships (his pro 1 stands). See 9.3 for what that does to a
  seeded run.
- The money study's soak (`Bake/economy/soak.py`) never rounded payments at all, so the mended purse is nearer to
  what the debt was tuned on than the purse as it is.

### 3.3 A quote (`Deal.QuoteBuy`, `Deal.QuoteSell`, `Hold.CostOf`)

To write "all 4 for 121 d" on a button the frame must know what a lot would come to without dealing it. The two
quotes run the same loop as `Buy` and `Sell` and put the market back as it was (`QuoteSell` saves and restores the
good's stock and glut, which is all `Market.GotOne` touches). `Buy` and `Sell` themselves are not touched, so the
other ships' path is as it was; a test holds the quote and the deal to the same sum.

## 4. The mend, part three: the ledger

- **Ports across**: every market there is a price note of younger than 90 days; the port she lies at first, the
  rest in the order of the ports table, so a column keeps its place from day to day. Sixteen at most (the
  freshest); a line at the foot says how many more.
- **Goods down**: what she carries first; then, by a pair of buttons, either what was on offer where she lies when
  she last looked, or every good any of those ports is known to have on offer.
- **Two looks**, by a pair of buttons: *What they pay* (where to sell) and *What they ask* (where to buy; "none"
  where none was on offer when the word was fresh).
- **A cell**: the price by its grade (2.3) and under it the age of the word. Exact figures are in the bold face,
  "about" plain, words in italics; the cell pales in three steps as the word ages (today and yesterday full; then to
  ten days; to two months; older). The best cell of a row is gold: where they pay most, or where it is asked cheapest. Only figures
  are ranked: a gold mark on a word would give away the number behind it.
- **A port's head**: its name; how the newest word of it came and its age ("seen, yesterday", "talk, last month",
  "here"); its toll ("toll 10 in 100", "no toll").
- **One reckoning** for the whole sheet, said in its first line.
- **A pressed cell** says its whole line above the sheet, with its mark and wedge: who said it, where, when. (The
  frame draws no tooltips at run time: nothing here lives only in one.)
- Every cell is `Known.Cell`, which picks the note by the same rules as `Known.Word`. A test holds each cell to its
  line.

## 5. The panels, in words

The figures in these pictures are for showing the layout. Four are from the captures (wine: ask 40 d, pay 34 d at
Dorestat, 36 d at Traiectum; bread asked 5.2 d, paying 4.3 d); the lots' sums and the other ports' prices are made
up.

### 5.1 Trade, the brief face (left column; one to four lines; no buttons)

In a market:

```
TRADE
Your silver is worth 81 d here. Room for 0 kg.
All 4 casks of Rhenish wine would bring 109 d here, the toll paid.
The toll takes 10 in the hundred of what you sell.
A boatman at Dorestat said Traiectum was paying more than here for Rhenish wine. A few days back.
```

Line 1 is what she can spend and carry. Line 2 is the lot aboard that would bring most into the purse here, after
the toll (with nothing aboard: "She carries nothing to sell here."). Line 4 is the word on that good. Shut: "The
market is shut." At sea: "She lies at no market." and the word on each good aboard, now said "than at Dorestat".

### 5.2 Trade, the full face (a floating window; today, a tiled window)

The picture is just after the first cask is sold (a cask takes 138 kg of room: 115 kg at a bulk of 1.2).

```
TRADE                                                                                  x
The emporium at Dorestat
Prices in d. The toll takes 10 in the hundred of what you sell here.
Your silver is worth 112 d here. Room left in the hold: 138 kg.
Sold 1 cask of rhenish wine for 34 d, less 3.4 d toll: got 30 deniers and 1 g of hacksilver.

+--------------------------------------------------------------------------------------+
| Rhenish wine                                      3   [Sell] [2 for 52 d] [all 3 for 71 d]
| They ask 35 d, 28 here. They pay 29 d. By the cask.   [Buy]
| Yours cost you nothing.
+--------------------------------------------------------------------------------------+
  (eye) (wedge up) You saw Rhenish wine pay 36 d at Traiectum, a few days back. That is 7 d more than here.
+--------------------------------------------------------------------------------------+
| Ship's bread                                          [Buy] [2 for 11 d] [all 4 for 22 d]
| They ask 5.2 d, 60 here. They pay 4.3 d. By the sack.
+--------------------------------------------------------------------------------------+
  (ear) A boatman at Dorestat said Traiectum was paying much the same as here for ship's bread. A few days back.
+--------------------------------------------------------------------------------------+
| Mayen quernstones                                     [Buy]
| They ask 13 d, 9 here. They pay 11 d. By the pair.
+--------------------------------------------------------------------------------------+
  (and so on down: what she carries first, then what is on offer, then what is only wanted)
```

- Room is what limits the lots here: one more cask fits (so the wine has only "Buy"), four sacks of bread (30 kg of
  room each), one pair of querns (72 kg). With room to spare it would be the silver, or the stock.

- The head, the toll line and the purse line stand still at the top. The purse line is what her silver buys HERE
  (`Purse.Worth` in this market's reckoning: for the starting purse 81 d at Dorestat, not the 86 d of all her silver
  by weight; 112 d after the sale in the picture), so the figure the window shows is the figure `Deal.Buy` reckons
  on. The top bar is made to agree
  (6.10).
- The fourth line is what came of the last deal, in the game's own words; it is not there before the first.
- A row is the frame's line (`frame-ln`): the good's name; under it the quote in small print, with "Yours cost
  30 d each." when she carries some she paid for (that was a tooltip, and tooltips are not drawn at run time); the
  count aboard in gold; and the two rows of buttons, selling over buying.
- Under each row, the word on that good from elsewhere, said against the price in this row.
- Nothing in the window jumps while she reads: a price that moves changes its figure and the buttons' sums in
  place. The window is built again only when a row comes or goes (she sells her last cask; a good sells out).

### 5.3 The word, the brief face

```
THE WORD
A Frisian skipper said Lundenwic was paying about 60 d for Rhenish wine, a few days back. That is far more than here.
You saw Mayen quernstones pay 12 d at Traiectum, yesterday. That is 1 d more than here.
5 markets in the ledger.
```

The word on the first two goods she carries, then how many markets the ledger holds. With an empty hold: "She
carries nothing to have word on." and the count.

### 5.4 The word, the full face: the ledger (a wide floating window)

```
THE WORD                                                                                                       x
The ledger: 5 markets known
Every price is set against Dorestat, where she lies, and reckoned in d as it is there. A plain figure you saw
yourself this week. "About" is a figure a man who saw it told you, or one you saw longer ago. A word is talk.
Paler is older.
[What they pay] [What they ask]   [Aboard, and on offer here] [Every good on offer anywhere]
(ear) (wedge up) A Frisian skipper said Lundenwic was paying about 60 d for Rhenish wine, a few days back. That is
far more than here.

 What they pay     | Dorestat       | Traiectum       | Walichrum        | Lundenwic             | Quentovic
 for one of each   | here           | seen, yesterday | told, last week  | told, a few days back | talk, last month
                   | toll 10 in 100 | toll 10 in 100  | toll 10 in 100   | toll 5 in 100         | toll 10 in 100
 ------------------+----------------+-----------------+------------------+-----------------------+------------------
 Rhenish wine      | 34 d           | 36 d            | about 40 d       | ABOUT 60 d (gold)     | far more
 4 aboard          | here           | yesterday       | last week        | a few days back       | last month
 ------------------+----------------+-----------------+------------------+-----------------------+------------------
 Ship's bread      | 4.3 d          | 4.6 d (gold)    | about 4 d        | about 4 d             | less
 Provisions        | here           | yesterday       | last week        | a few days back       | last month
 ------------------+----------------+-----------------+------------------+-----------------------+------------------
 Mayen quernstones | 11 d           | 12 d            | about 12 d       | ABOUT 16 d (gold)     | more
 Stone             | here           | yesterday       | last week        | a few days back       | last month
```

- One note of a port gives one grade to its whole column (a skipper told all he saw; talk is talk for every good),
  so a column reads alike from top to bottom unless an older note fills a gap. Where no note of a port has a good
  at all, the cell is a dash (seldom: a note mostly carries every good the market deals in).

- The names' column stands still; the ports scroll sideways beside it when there are more than the window is wide
  (a column is 124 px, the names 210 px; twelve ports are 1,698 px).
- The line above the sheet is the pressed cell's whole line (here: wine at Lundenwic). Before any press: "Press a
  cell to hear who said it, where and when."
- At sea the first line reads: "Every price is set against Dorestat, where you last saw a market (two days back),
  and reckoned in d as it is there." and no column is marked "here".
- *What they ask* turns the same sheet: "40 d", "about 44 d", "none" (none was on offer), "on offer" (told so, with
  no price of hers to set it against), "dearer", "far cheaper". Gold is then the cheapest figure.
- Under a good's name stands how many are aboard (in gold), else its kind.

### 5.5 Where the word shows without a panel of its own

The Voyage window, under a picked port, and the chart's port card show the same lines for what she carries. They
need no change: capture 08's "The toll man at Dorestat said Traiectum was paying poorly for Rhenish wine. A few days
back." becomes "The toll man at Dorestat said Traiectum was paying more than here for Rhenish wine. A few days
back.", or "much the same as here" if his word was a little off (talk carries an error of 3 in the hundred).

## 6. The code

Order of work. Each step compiles by itself.

1. The word: 6.1, 6.2, 6.3, the two tables (2.6), the test diff 7.1.
2. The purse: 6.4.
3. The quotes: 6.5, 6.6.
4. The frame: 6.7 (new file), 6.8, 6.9, 6.10, the stylesheet 6.11. Then the new tests 7.2 and the play test 7.3.
5. When the brief faces are in (`brief_faces.md`, step 1 of its section 8): 6.12.

The diffs are against the files as they were at 13:00 on 7 October and apply from the project folder
(`git apply file.diff`, or by hand: each hunk shows three lines either side). If a file has been changed since, its
diff may not apply as it stands. The files as I had them, by the first twelve letters of `md5sum`:

| File | md5 begins |
|---|---|
| `Code/Game/Known/Word.cs` | `4d376740c3ce` |
| `Code/Game/Known/Known.cs` | `11c940fd7202` |
| `Code/Game/Data/Wording.cs` | `1dd36cbbd49d` |
| `Code/Game/Trade/Purse.cs` | `5fa745e319e5` |
| `Code/Game/Trade/Deal.cs` | `ae7887d4efdb` |
| `Code/Game/Trade/Hold.cs` | `dcfd87320b00` |
| `Code/Game/UI/Frame/Panels/TradePanel.cs` (replaced whole) | `b48a0ddfe0d2` |
| `Code/Game/UI/Frame/Panels/WordPanel.cs` (replaced whole) | `dd52965132da` |
| `Tests/EditMode/KnownWordTests.cs` | `05ecc5f13fb6` |

### 6.1 `Assets/Kapman/Code/Game/Known/Known.cs`

```diff
--- a/Assets/Kapman/Code/Game/Known/Known.cs
+++ b/Assets/Kapman/Code/Game/Known/Known.cs
@@ -79,6 +79,13 @@
         /// <summary>A told price is said as a word: "well" at this many times the good's table value or more, "poorly" at
         /// this many times or less, else "the usual".</summary>
         public float paysWell = 1.15f, paysPoorly = 0.85f;
+        /// <summary>The word is said against the price where she lies: a price within this share of it is "much the
+        /// same". (Where "far more" begins is not a number here: it is worked out from the market's gap and the toll,
+        /// Known.FarOver.)</summary>
+        public float sameWithin = 0.05f;
+        /// <summary>A price told by one who saw it himself is said as a rough figure ("about 36 d"). False: as a word,
+        /// like all talk (the rule of 4 October: a number only for what she saw herself).</summary>
+        public bool toldFigure = true;
 
         public static KnowingRules Load(TextAsset json)
         {
@@ -119,6 +126,8 @@
             r.wordAheadDays = Json.Num(word, "ahead_days", r.wordAheadDays);
             r.paysWell = (float)Json.Num(word, "pays_well", r.paysWell);
             r.paysPoorly = (float)Json.Num(word, "pays_poorly", r.paysPoorly);
+            r.sameWithin = (float)Json.Num(word, "same_within", r.sameWithin);
+            r.toldFigure = Json.Bool(word, "told_figure", r.toldFigure);
             return r;
         }
     }
```

### 6.2 `Assets/Kapman/Code/Game/Known/Word.cs`

New: the types `PriceGrade`, `WordHere`, `PriceCell`; `Known.AskFor`, `Here`, `FarOver`, `Lean`, `Reckoning`, `Rough`,
`Cell`, `CellText`. Changed: `Word` and `WordOn` work out what the line is said against, once; `LedgerWord` is
written again on top of the cell. `PaysKey` stays, for the merchant with nothing to compare.

```diff
--- a/Assets/Kapman/Code/Game/Known/Word.cs
+++ b/Assets/Kapman/Code/Game/Known/Word.cs
@@ -1,5 +1,6 @@
 using System;
 using System.Collections.Generic;
+using System.Globalization;
 using Kapman.Game.Sim;
 
 namespace Kapman.Game
@@ -73,6 +74,54 @@
         public PortDef here;
         /// <summary>The world's ships (null: none are looked at).</summary>
         public IReadOnlyList<SimShip> ships;
+        // the price the word on one good is said against, worked out once for a line or for a row of the ledger
+        internal WordHere against;
+        internal string againstGood;
+    }
+
+    /// <summary>How exactly a price is known, from least to most: not at all; as a word against where she lies (talk, a
+    /// rumour, word grown old); as a rough figure ("about 36 d": told by one who saw it himself, or her own sight of it
+    /// more than a week old); to the penny (seen with her own eyes within the week).</summary>
+    public enum PriceGrade { None, Word, About, Exact }
+
+    /// <summary>The price the word is said against: what she saw where she lies now, or (at sea, or where she has not
+    /// looked at the market) the last price of the good she saw with her own eyes.</summary>
+    public struct WordHere
+    {
+        /// <summary>The port it was seen at (null: she has seen no price of the good in the days a note counts).</summary>
+        public PortDef port;
+        /// <summary>She lies there now ("than here"); false: it is where she last saw one ("than at Dorestat").</summary>
+        public bool lying;
+        /// <summary>What they paid and asked there for one unit as she saw them (deniers), and whether any was on offer.</summary>
+        public float pays, asks;
+        public bool onOffer;
+        /// <summary>The game day she saw them.</summary>
+        public double day;
+        public bool Known => port != null;
+    }
+
+    /// <summary>One cell of the ledger: what the merchant knows of a good's price at a port, how exactly, how old, and
+    /// how it stands against where she lies.</summary>
+    public struct PriceCell
+    {
+        public PriceGrade grade;
+        /// <summary>The rule of the word it comes by: 2 a price seen, 3 a price told, 4 talk, a rumour, or grown old (0: nothing).</summary>
+        public int rule;
+        /// <summary>What they pay and ask there for one unit, as noted (deniers; asks is NaN where none was on offer).</summary>
+        public float pays, asks;
+        public bool onOffer;
+        /// <summary>The game day it was true, and its age as a wording key ("age.last_week").</summary>
+        public double day;
+        public string ageKey;
+        /// <summary>True when there is a price to set it against, and this port is not that price's own.</summary>
+        public bool compared;
+        /// <summary>What they pay there against what they pay here, and what they ask against what they ask here: +2 far
+        /// more, +1 more, 0 much the same, -1 less, -2 far less (0 too when nothing is compared, or none is on offer).</summary>
+        public int paysLean, asksLean;
+        /// <summary>The note it rests on (who told it: Known.Person(note)); null when nothing is known.</summary>
+        public PriceNote note;
+        /// <summary>The good it is about (its id).</summary>
+        public string goodId;
     }
 
     public static partial class Known
@@ -116,12 +165,16 @@
         /// than 14 days; (4) talk and rumour, and whatever else is noted and not yet 90 days old; (5) the season: what
         /// comes to market and what is wanted there this month, and the good's own cheap and dear months; (6) ships that
         /// left for there ahead of her with the good aboard; (7) the law on the good there; (8) nothing known. The days
-        /// are in knowing_793.json ("word"). Only a price seen shows a number; a told one is "well", "poorly" or "the
-        /// usual". The same every time for the same knowledge and day: nothing is drawn, and no market is asked.
+        /// are in knowing_793.json ("word"). A price is said AGAINST THE PRICE WHERE SHE LIES (<see cref="Here"/>): a
+        /// price she saw within the week with its figure and the difference ("2 d more than here"); one told by a man
+        /// who saw it, or her own older sight, as a rough figure ("about 36 d"); talk and rumour as a word ("more than
+        /// here"). Only with nothing to set it against is it "well", "poorly" or "the usual" by the good's table value.
+        /// The same every time for the same knowledge and day: nothing is drawn, and no market is asked.
         /// </summary>
         public static GoodWord Word(GoodDef good, PortDef port, WordAsk ask)
         {
             Now(ref ask);
+            ask = AskFor(good, ask);
             if (good == null || port == null)
                 return Make(8, "none.anywhere", WordMark.None, 0, good, port, ask, double.NaN);
             if (TryKnown(good, port, ask, out var w))
@@ -143,6 +196,7 @@
         public static GoodWord WordOn(GoodDef good, WordAsk ask)
         {
             Now(ref ask);
+            ask = AskFor(good, ask);
             if (good == null)
                 return Make(8, "none.anywhere", WordMark.None, 0, null, null, ask, double.NaN);
             var best = default(GoodWord);
@@ -245,6 +299,236 @@
             return tilt > 0 ? "pays.well" : tilt < 0 ? "pays.poorly" : "pays.fair";
         }
 
+        // ------------------------------------------------------------------------------------------ against where she lies
+
+        /// <summary>The ask with the price it is said against worked out for this good: once for a line, or once for a
+        /// row of the ledger (ask every cell of the row with what this returns).</summary>
+        public static WordAsk AskFor(GoodDef good, WordAsk ask)
+        {
+            if (good != null && ask.againstGood != good.id)
+            {
+                ask.against = Here(good, ask);
+                ask.againstGood = good.id;
+            }
+            return ask;
+        }
+
+        /// <summary>
+        /// The price the word on a good is said against (the strategy critic, 7 October: "say it against where she
+        /// lies"): what she saw at the port she lies at, once she has seen its market (the frame notes it when she
+        /// moors: FrameDeal.See); else the last price of the good she saw with her own eyes anywhere, while it is young
+        /// enough to count (knowing_793.json "word": known_days). From the price notes alone: no market is asked.
+        /// </summary>
+        public static WordHere Here(GoodDef good, WordAsk ask)
+        {
+            var here = default(WordHere);
+            if (good == null || !Trade.Started)
+                return here;
+            if (ask.here != null && OwnSight(ask.here.id, good.id, ask.today, out double at, out var seen))
+                return new WordHere { port = ask.here, lying = true, pays = seen.pays, asks = seen.asks, onOffer = seen.onOffer, day = at };
+            var ports = GameData.Ports;
+            for (int i = 0; i < ports.Count; i++)
+            {
+                if (!OwnSight(ports[i].id, good.id, ask.today, out at, out seen) || (here.port != null && at <= here.day))
+                    continue;
+                here = new WordHere { port = ports[i], lying = false, pays = seen.pays, asks = seen.asks, onOffer = seen.onOffer, day = at };
+            }
+            return here;
+        }
+
+        /// <summary>The newest price of a good the merchant saw with her own eyes at a port, while it still counts.</summary>
+        static bool OwnSight(string port, string good, double today, out double day, out SeenPrice price)
+        {
+            day = double.NaN;
+            price = default;
+            var notes = Trade.Ledger.History(port);
+            for (int i = notes.Count - 1; i >= 0; i--)
+            {
+                var n = notes[i];
+                double age = today - n.day;
+                if (age < 0.0)
+                    continue;
+                if (age >= Rules.wordKnownDays)
+                    return false;                                        // (the notes before it are older still)
+                if ((n.how == How.Seen || n.how == How.Own) && n.prices.TryGetValue(good, out price))
+                {
+                    day = n.day;
+                    return true;
+                }
+            }
+            return false;
+        }
+
+        /// <summary>
+        /// The share by which the price at the place of sale must stand over the price at the place of buying before
+        /// carrying the good from the one to the other pays: the market's own gap between what it asks and what it pays
+        /// (trade_rules.json "spread"), and the toll where it is sold. With a spread of 0.2 that is 0.36 into a port that
+        /// takes a tenth and 0.22 into one that takes no toll. This is where "far more than here" begins, so the word
+        /// means: buying it here to sell it there would pay, if the word still holds.
+        /// </summary>
+        public static float FarOver(PortDef sellAt)
+        {
+            var rules = TradeRules.Current;
+            if (rules == null)
+                return 0.25f;
+            float half = Math.Max(0f, Math.Min(0.9f, rules.spread * 0.5f));
+            float toll = sellAt != null ? rules.ZoneOf(sellAt.region)?.toll ?? 0f : 0f;
+            return (1f + half) / (1f - half) / Math.Max(0.5f, 1f - toll) - 1f;
+        }
+
+        /// <summary>How a price there stands against a price here: +2 far more (by `farUp` or more: carrying it there
+        /// pays), +1 more, 0 much the same (within knowing_793.json "word" same_within), -1 less, -2 far less (so much
+        /// less that carrying it from there to here pays: `farDown`).</summary>
+        public static int Lean(float there, float here, float farUp, float farDown)
+        {
+            if (float.IsNaN(there) || float.IsNaN(here) || here <= 0f || there <= 0f)
+                return 0;
+            float r = there / here, same = Math.Max(0f, Rules.sameWithin);
+            if (r >= 1f + Math.Max(farUp, same)) return 2;
+            if (r >= 1f + same) return 1;
+            if (r <= 1f / (1f + Math.Max(farDown, same))) return -2;
+            if (r <= 1f / (1f + same)) return -1;
+            return 0;
+        }
+
+        static string LeanKey(int lean) => lean >= 2 ? "far_more" : lean == 1 ? "more" : lean == 0 ? "same" : lean == -1 ? "less" : "far_less";
+
+        /// <summary>The reckoning a price is said in: that of the port the word is said against (so two prices can be set
+        /// side by side), and with nothing to set it against the port's own.</summary>
+        public static ZoneDef Reckoning(PortDef port, WordAsk ask) => TradeRules.Current?.ZoneOf((ask.against.port ?? port)?.region);
+
+        static string Shown(double deniers, ZoneDef zone) =>
+            zone != null ? zone.Show(deniers) : deniers.ToString("0", CultureInfo.InvariantCulture) + " d";
+
+        /// <summary>A price as a rough figure ("36 d", said as "about 36 d"): to the whole unit under twenty, to the
+        /// nearest two under fifty, to the nearest five under two hundred, then to the nearest ten, so it cannot be taken
+        /// for a price seen.</summary>
+        public static string Rough(double deniers, ZoneDef zone)
+        {
+            double v = deniers / (zone != null ? Math.Max(0.0001, zone.unitDeniers) : 1.0);
+            double step = v < 20.0 ? 1.0 : v < 50.0 ? 2.0 : v < 200.0 ? 5.0 : 10.0;
+            double r = Math.Max(step, Math.Round(v / step, MidpointRounding.AwayFromZero) * step);
+            return r.ToString("0", CultureInfo.InvariantCulture) + " " + (zone != null ? zone.unit : "d");
+        }
+
+        /// <summary>"here", or "at Dorestat" where the word is said against the last price she saw and not where she lies.</summary>
+        static string HereWords(PortDef port, WordAsk ask) =>
+            ask.against.lying ? Wording.WordText(port, "here.lying")
+                              : Wording.Fill(Wording.WordText(port, "here.last"), ("place", ask.against.port != null ? ask.against.port.name : ""));
+
+        /// <summary>A price as words against where she lies: "more than here", "far less than at Dorestat".</summary>
+        static string AgainstWords(PortDef port, WordAsk ask, int lean) =>
+            Wording.Fill(Wording.WordText(port, "against." + LeanKey(lean)), ("here", HereWords(port, ask)));
+
+        /// <summary>The sentence that sets a figure against where she lies ("That is 2 d more than here."): the difference
+        /// as a figure when both prices were seen (`exact`), else as a word.</summary>
+        static string AgainstLine(PortDef port, WordAsk ask, PriceCell cell, bool exact)
+        {
+            string words = AgainstWords(port, ask, cell.paysLean);
+            if (exact)
+            {
+                var zone = Reckoning(port, ask);
+                double unit = zone != null ? Math.Max(0.0001, zone.unitDeniers) : 1.0;
+                double by = cell.pays - ask.against.pays;
+                if (Math.Abs(by) / unit >= 0.05)
+                    words = Wording.Fill(Wording.WordText(port, by > 0.0 ? "against.by.more" : "against.by.less"),
+                                         ("diff", Shown(Math.Abs(by), zone)), ("here", HereWords(port, ask)));
+                else
+                    words = AgainstWords(port, ask, 0);
+            }
+            return Wording.Fill(Wording.WordText(port, "against.line"), ("against", words));
+        }
+
+        /// <summary>
+        /// ONE CELL OF THE LEDGER: what the merchant knows of a good's price at a port, by the same notes and the same
+        /// order as the word on the item (rules 2 to 4), so the cell and the line never disagree. Ask a whole row with
+        /// one <see cref="AskFor"/>.
+        /// </summary>
+        public static PriceCell Cell(GoodDef good, PortDef port, WordAsk ask)
+        {
+            Now(ref ask);
+            if (good == null || port == null)
+                return new PriceCell { ageKey = "", pays = float.NaN, asks = float.NaN, day = double.NaN };
+            return CellOf(good, port, AskFor(good, ask));
+        }
+
+        static PriceCell CellOf(GoodDef good, PortDef port, WordAsk ask)
+        {
+            var cell = new PriceCell { ageKey = "", pays = float.NaN, asks = float.NaN, day = double.NaN, goodId = good.id };
+            if (!Trade.Started)
+                return cell;
+            var notes = Trade.Ledger.History(port.id);
+            PriceNote seen = null, told = null, other = null;
+            for (int i = notes.Count - 1, looked = 0; i >= 0 && looked < 12; i--)
+            {
+                var n = notes[i];
+                if (!n.prices.ContainsKey(good.id))
+                    continue;
+                looked++;
+                double age = ask.today - n.day;
+                if (age < 0.0)
+                    continue;
+                if (age >= Rules.wordKnownDays)
+                    break;                                               // (the notes before it are older still)
+                bool mine = n.how == How.Seen || n.how == How.Own;
+                if (mine && age < Rules.wordSeenDays)
+                    seen = seen ?? n;
+                else if (!mine && n.how != How.Rumour && n.how != How.Lore && n.hops <= 1 && age < Rules.wordToldDays)
+                    told = told ?? n;
+                else
+                    other = other ?? n;
+            }
+            var note = seen ?? told ?? other;
+            if (note == null)
+                return cell;
+            var price = note.prices[good.id];
+            bool own = note.how == How.Seen || note.how == How.Own;
+            bool firstHand = note == told && note.hops == 0 && (note.how == How.Told || note.how == How.Written);
+            cell.note = note;
+            cell.rule = note == seen ? 2 : note == told ? 3 : 4;
+            cell.grade = note == seen ? PriceGrade.Exact : own || (firstHand && Rules.toldFigure) ? PriceGrade.About : PriceGrade.Word;
+            cell.pays = price.pays;
+            cell.onOffer = price.onOffer;
+            cell.asks = price.onOffer ? price.asks : float.NaN;
+            cell.day = note.day;
+            cell.ageKey = AgeKey(note.day, ask.today);
+            if (cell.ageKey.StartsWith("age.since_", StringComparison.Ordinal))
+                cell.ageKey = "age.old_talk";
+            var here = ask.against;
+            cell.compared = here.Known && here.port != port;
+            if (cell.compared)
+            {
+                float up = FarOver(port), down = FarOver(here.port);
+                cell.paysLean = Lean(price.pays, here.pays, up, down);
+                cell.asksLean = price.onOffer && here.onOffer ? Lean(price.asks, here.asks, up, down) : 0;
+            }
+            return cell;
+        }
+
+        /// <summary>
+        /// A cell as it is written in the ledger: the figure she saw ("36 d"), a rough figure ("about 36 d"), a word
+        /// against where she lies ("far more", "cheaper"), "none" where none was on offer (`asks`), "" where nothing is
+        /// known. In the reckoning of the port the row is said against. `ask` is the row's (<see cref="AskFor"/>).
+        /// </summary>
+        public static string CellText(PriceCell cell, PortDef port, WordAsk ask, bool asks)
+        {
+            if (cell.grade == PriceGrade.None)
+                return "";
+            if (asks && !cell.onOffer)
+                return Wording.WordText(port, "cell.none");
+            var zone = Reckoning(port, ask);
+            float v = asks ? cell.asks : cell.pays;
+            if (cell.grade == PriceGrade.Exact)
+                return Shown(v, zone);
+            if (cell.grade == PriceGrade.About)
+                return Wording.Fill(Wording.WordText(port, "cell.about"), ("price", Rough(v, zone)));
+            if (!cell.compared)
+                return asks ? Wording.WordText(port, "cell.offer") : Wording.WordText(port, PaysKey(cell.pays, GameData.Good(cell.goodId), out _));
+            if (asks && !ask.against.onOffer)
+                return Wording.WordText(port, "cell.offer");
+            return Wording.WordText(port, (asks ? "cell.asks." : "cell.") + LeanKey(asks ? cell.asksLean : cell.paysLean));
+        }
+
         /// <summary>A good's name as it reads in the middle of a line: without what is in brackets, and with a small first
         /// letter unless its first word is a place's or a people's ("Rhenish wine", "salt herring").</summary>
         public static string GoodName(GoodDef g)
@@ -390,50 +674,45 @@
             return false;
         }
 
-        /// <summary>Rules 2, 3 and 4 from the price notes: a price seen lately (said with its number), a price told lately
-        /// by someone at first or second hand (said as a word, by a person), and whatever else is noted and not yet too
-        /// old: talk through more mouths or grown old ("they say"), a rumour, the merchant's own older sight of it.</summary>
+        /// <summary>Rules 2, 3 and 4 from the price notes, each said against where she lies (<see cref="Here"/>): a price
+        /// seen lately (its figure, and the difference as a figure); a price told lately by someone at first or second
+        /// hand (a man who saw it himself: "about" and a rough figure; one mouth further: a word); and whatever else is
+        /// noted and not yet too old: talk through more mouths or grown old ("they say"), a rumour, the merchant's own
+        /// older sight of it (a rough figure: she did see it). The tilt is the comparison's: up when they pay more there
+        /// than here. Only with no price to set it against at all is it the good's table value that speaks.</summary>
         static bool LedgerWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
         {
             w = default;
-            if (!Trade.Started)
+            var cell = CellOf(good, port, ask);
+            var note = cell.note;
+            if (note == null)
                 return false;
-            var notes = Trade.Ledger.History(port.id);
-            PriceNote seen = null, told = null, other = null;
-            for (int i = notes.Count - 1, looked = 0; i >= 0 && looked < 12; i--)
+            var zone = Reckoning(port, ask);
+            int tilt;
+            string pays, against = "";
+            if (cell.compared)
             {
-                var n = notes[i];
-                if (!n.prices.ContainsKey(good.id))
-                    continue;
-                looked++;
-                double age = ask.today - n.day;
-                if (age < 0.0)
-                    continue;
-                if (age >= Rules.wordKnownDays)
-                    break;                                               // (the notes before it are older still)
-                bool own = n.how == How.Seen || n.how == How.Own;
-                if (own && age < Rules.wordSeenDays)
-                    seen = seen ?? n;
-                else if (!own && n.how != How.Rumour && n.how != How.Lore && n.hops <= 1 && age < Rules.wordToldDays)
-                    told = told ?? n;
-                else
-                    other = other ?? n;
+                tilt = Math.Sign(cell.paysLean);
+                pays = AgainstWords(port, ask, cell.paysLean);
+                against = AgainstLine(port, ask, cell, cell.grade == PriceGrade.Exact);
             }
-            var note = seen ?? told ?? other;
-            if (note == null)
-                return false;
-            var price = note.prices[good.id];
-            string pays = Wording.WordText(port, PaysKey(price.pays, good, out int tilt));
-            if (note == seen)
+            else
             {
-                var zone = TradeRules.Current?.ZoneOf(port.region);
-                string shown = zone != null ? zone.Show(price.pays) : price.pays.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " d";
-                w = Make(2, "seen", WordMark.Seen, tilt, good, port, ask, note.day, ("price", shown), ("pays", pays));
+                // nothing to set it against: the word every merchant would use of the good (and no lean at all where the
+                // port asked about is itself the one place she has seen it)
+                pays = Wording.WordText(port, PaysKey(cell.pays, good, out tilt));
+                if (ask.against.Known)
+                    tilt = 0;
             }
-            else if (note == told)
+            if (cell.rule == 2)
+                w = Make(2, "seen", WordMark.Seen, tilt, good, port, ask, note.day, ("price", Shown(cell.pays, zone)), ("pays", pays), ("against", against));
+            else if (cell.rule == 3 && cell.grade == PriceGrade.About)
+                w = Make(3, "told.about", WordMark.Heard, tilt, good, port, ask, note.day,
+                         ("person", Person(note)), ("price", Rough(cell.pays, zone)), ("pays", pays), ("against", against));
+            else if (cell.rule == 3)
                 w = Make(3, "told", WordMark.Heard, tilt, good, port, ask, note.day, ("person", Person(note)), ("pays", pays));
             else if (note.how == How.Seen || note.how == How.Own)
-                w = Make(4, "saw", WordMark.Seen, tilt, good, port, ask, note.day, ("pays", pays));
+                w = Make(4, "saw", WordMark.Seen, tilt, good, port, ask, note.day, ("price", Rough(cell.pays, zone)), ("pays", pays), ("against", against));
             else if (note.how == How.Rumour)
                 w = Make(4, "rumour", WordMark.Heard, tilt, good, port, ask, note.day, ("pays", pays));
             else
```

### 6.3 `Assets/Kapman/Code/Game/Data/Wording.cs`

```diff
--- a/Assets/Kapman/Code/Game/Data/Wording.cs
+++ b/Assets/Kapman/Code/Game/Data/Wording.cs
@@ -132,15 +132,16 @@
             ["news.dear"] = new[] { "{news} {Good} is dear at {port} now." },
             ["news.cheap"] = new[] { "{news} {Good} goes cheap at {port} now." },
             // rule 2: a price seen (the only line with a number)
-            ["seen"] = new[] { "You saw {good} pay {price} at {port}, {age}." },
-            // rule 3: a price told
+            ["seen"] = new[] { "You saw {good} pay {price} at {port}, {age}. {Against}" },
+            // rule 3: a price told (by a man who saw it himself: a rough figure; through another mouth: a word)
             ["told"] = new[] { "{Person} said {port} was paying {pays} for {good}. {Age}." },
+            ["told.about"] = new[] { "{Person} said {port} was paying about {price} for {good}, {age}. {Against}" },
             // rule 4: talk and rumour
             ["talk"] = new[] { "They say {port} pays {pays} for {good}. {Age}." },
             ["talk.at"] = new[] { "They say at {place} that {port} pays {pays} for {good}. {Age}." },
             ["talk.fair"] = new[] { "They say there is a fair on at {port}: plenty of goods, and many buyers." },
             ["rumour"] = new[] { "There is a rumour that {port} pays {pays} for {good}." },
-            ["saw"] = new[] { "You saw {port} paying {pays} for {good}, {age}. It may have changed." },
+            ["saw"] = new[] { "You saw {port} paying about {price} for {good}, {age}. It may have changed. {Against}" },
             // rule 5: the season
             ["season.wanted"] = new[] { "{port} wants {good} in {month}." },
             ["season.comes"] = new[] { "{Good} comes to market at {port} in {month}: plenty, and cheap." },
@@ -180,6 +181,30 @@
             ["pays.well"] = new[] { "well" },
             ["pays.fair"] = new[] { "the usual" },
             ["pays.poorly"] = new[] { "poorly" },
+            // a price against where she lies (or where she last saw one): in a line, and short for a cell of the ledger
+            ["against.line"] = new[] { "That is {against}." },
+            ["against.far_more"] = new[] { "far more than {here}" },
+            ["against.more"] = new[] { "more than {here}" },
+            ["against.same"] = new[] { "much the same as {here}" },
+            ["against.less"] = new[] { "less than {here}" },
+            ["against.far_less"] = new[] { "far less than {here}" },
+            ["against.by.more"] = new[] { "{diff} more than {here}" },
+            ["against.by.less"] = new[] { "{diff} less than {here}" },
+            ["here.lying"] = new[] { "here" },
+            ["here.last"] = new[] { "at {place}" },
+            ["cell.about"] = new[] { "about {price}" },
+            ["cell.far_more"] = new[] { "far more" },
+            ["cell.more"] = new[] { "more" },
+            ["cell.same"] = new[] { "the same" },
+            ["cell.less"] = new[] { "less" },
+            ["cell.far_less"] = new[] { "far less" },
+            ["cell.asks.far_more"] = new[] { "far dearer" },
+            ["cell.asks.more"] = new[] { "dearer" },
+            ["cell.asks.same"] = new[] { "the same" },
+            ["cell.asks.less"] = new[] { "cheaper" },
+            ["cell.asks.far_less"] = new[] { "far cheaper" },
+            ["cell.none"] = new[] { "none" },
+            ["cell.offer"] = new[] { "on offer" },
             // who told it
             ["person.skipper"] = new[] { "a {people} skipper" },
             ["person.skipper.plain"] = new[] { "a skipper" },
```

### 6.4 `Assets/Kapman/Code/Game/Trade/Purse.cs`

```diff
--- a/Assets/Kapman/Code/Game/Trade/Purse.cs
+++ b/Assets/Kapman/Code/Game/Trade/Purse.cs
@@ -81,13 +81,26 @@
             return worth;
         }
 
-        static bool Divisible(string coin) => coin == "hacksilver";
+        const string Cut = "hacksilver";
+
+        static bool Divisible(string coin) => coin == Cut;
+
+        /// <summary>Cut silver for a sum under one coin, weighed out (by weight: no zone's loss), to the tenth of a gram
+        /// and never over the sum. 0 where the rules have no cut silver.</summary>
+        static double CutFor(double deniers, TradeRules rules)
+        {
+            double perGram = rules.Coin(Cut)?.Deniers ?? 0.0;
+            return perGram <= 0.0 || deniers <= 0.0 ? 0.0 : Math.Floor(deniers / perGram * 10.0 + 1e-6) / 10.0;
+        }
 
         /// <summary>The coins of the last payment or takings ("12 deniers and 3 pennies"), and the change given ("" if none).</summary>
         public string LastCoins { get; private set; } = "";
         public string LastBack { get; private set; } = "";
 
-        /// <summary>Pays deniers' worth here. False (and nothing paid) if the purse can't. `how` says what changed hands.</summary>
+        /// <summary>Pays deniers' worth here. False (and nothing paid) if the purse can't. `how` says what changed hands.
+        /// Whole coins are not rounded against the payer any more (the strategy critic, 7 October): a coin is broken for
+        /// a sum under its worth only when the cut silver aboard cannot make that sum up, and then the change under one
+        /// coin comes back weighed out in cut silver.</summary>
         public bool Pay(double deniers, ZoneDef zone, TradeRules rules, out string how)
         {
             how = "";
@@ -102,6 +115,7 @@
             var order = All.Select(kv => kv.Key).OrderBy(c => c == zone?.coin ? 0 : Divisible(c) ? 2 : 1)
                            .ThenByDescending(c => Rate(c, zone, rules) / Math.Max(1e-6, rules.Coin(c)?.Deniers ?? 1.0)).ToList();
             double left = deniers;
+            double cutWorth = Count(Cut) * Rate(Cut, zone, rules);       // what the cut silver aboard makes up here
             var paid = new List<string>();
             foreach (var coin in order)
             {
@@ -111,7 +125,15 @@
                 if (rate <= 0.0)
                     continue;
                 double have = Count(coin);
-                double take = Divisible(coin) ? Math.Min(have, Math.Round(left / rate + 0.049, 1)) : Math.Min(have, Math.Ceiling(left / rate - 1e-9));
+                double take;
+                if (Divisible(coin))
+                    take = Math.Min(have, Math.Round(left / rate + 0.049, 1));
+                else
+                {
+                    // whole coins up to the sum; one more is broken only when the cut silver cannot make up the rest
+                    double whole = Math.Floor(left / rate + 1e-9);
+                    take = Math.Min(have, cutWorth + 1e-9 >= left - whole * rate ? whole : Math.Ceiling(left / rate - 1e-9));
+                }
                 if (take <= 0.0)
                     continue;
                 held[coin] = have - take;
@@ -123,15 +145,27 @@
             LastBack = "";
             if (left < -0.01)
             {
-                // change, in the local coin (cut silver where silver goes by weight)
+                // change: whole coins of the local coin, and what is under one coin weighed out in cut silver
                 string local = zone?.coin ?? "denier";
-                double back = -left / Math.Max(1e-6, Rate(local, zone, rules));
-                back = Divisible(local) ? Math.Round(back, 1) : Math.Floor(back + 1e-6);
-                if (back > 0.0)
-                {
-                    Add(local, back);
-                    how += ", " + Amount(local, back, rules) + " back";
-                    LastBack = Amount(local, back, rules);
+                double rate = Math.Max(1e-6, Rate(local, zone, rules)), over = -left;
+                var back = new List<string>();
+                double coins = Divisible(local) ? Math.Round(over / rate, 1) : Math.Floor(over / rate + 1e-6);
+                if (coins > 0.0)
+                {
+                    Add(local, coins);
+                    back.Add(Amount(local, coins, rules));
+                    over -= coins * rate;
+                }
+                double cut = Divisible(local) ? 0.0 : CutFor(over, rules);
+                if (cut > 0.0)
+                {
+                    Add(Cut, cut);
+                    back.Add(Amount(Cut, cut, rules));
+                }
+                if (back.Count > 0)
+                {
+                    LastBack = Join(back);
+                    how += ", " + LastBack + " back";
                 }
             }
             return true;
@@ -150,15 +184,37 @@
             return deniers;
         }
 
-        /// <summary>Takes deniers' worth paid here, in the local coin (cut silver where silver goes by weight).</summary>
+        /// <summary>Takes deniers' worth paid here: whole coins of the local coin, and what is under one coin weighed out
+        /// in cut silver (all in cut silver where silver goes by weight). Nothing is rounded up, and no more than a tenth
+        /// of a gram is lost.</summary>
         public void Receive(double deniers, ZoneDef zone, TradeRules rules, out string how)
         {
             string local = zone?.coin ?? "denier";
-            double n = deniers / Math.Max(1e-6, Rate(local, zone, rules));
-            n = Divisible(local) ? Math.Round(n, 1) : Math.Round(n);
-            Add(local, n);
-            how = "took " + Amount(local, n, rules);
-            LastCoins = Amount(local, n, rules);
+            double rate = Math.Max(1e-6, Rate(local, zone, rules));
+            var got = new List<string>();
+            if (Divisible(local))
+            {
+                double n = Math.Round(deniers / rate, 1);
+                Add(local, n);
+                got.Add(Amount(local, n, rules));
+            }
+            else
+            {
+                double n = Math.Max(0.0, Math.Floor(deniers / rate + 1e-9));
+                double cut = CutFor(deniers - n * rate, rules);
+                if (n > 0.0)
+                {
+                    Add(local, n);
+                    got.Add(Amount(local, n, rules));
+                }
+                if (cut > 0.0)
+                {
+                    Add(Cut, cut);
+                    got.Add(Amount(Cut, cut, rules));
+                }
+            }
+            how = "took " + Join(got);
+            LastCoins = Join(got);
             LastBack = "";
         }
 
```

### 6.5 `Assets/Kapman/Code/Game/Trade/Deal.cs`

```diff
--- a/Assets/Kapman/Code/Game/Trade/Deal.cs
+++ b/Assets/Kapman/Code/Game/Trade/Deal.cs
@@ -123,6 +123,80 @@
             return r;
         }
 
+        /// <summary>What buying up to `count` units would come to now, with nothing changing hands: how many there are
+        /// on offer, room for and silver to pay, and their price in all (each unit dearer than the last, exactly as
+        /// <see cref="Buy"/> reckons it). Ask for int.MaxValue to learn all that fits.</summary>
+        public static DealResult QuoteBuy(Market m, Purse purse, Hold hold, string goodId, int count, int month, double day, string who)
+        {
+            var r = new DealResult { why = "" };
+            var g = m?.Get(goodId);
+            if (g == null || count <= 0)
+                return Fail(ref r, "not_dealt");
+            if (m.Disrupted(day))
+                return Fail(ref r, m.RuinedOn(day) ? "ruined" : "disrupted");
+            int room = hold.RoomFor(g.good);
+            double worth = purse.Worth(m.zone, TradeRules.Current);
+            double total = 0.0;
+            int n = 0;
+            float stock = g.stock;
+            while (n < count && n < room && stock - n >= 1f)
+            {
+                g.stock = stock - n;
+                double each = AsksFor(m, g, month, who);
+                if (total + each > worth + 1e-6)
+                    break;
+                total += each;
+                n++;
+            }
+            g.stock = stock;                                         // nothing has changed hands
+            if (n == 0)
+                return Fail(ref r, stock < 1f ? "none_left" : room <= 0 ? "no_room" : "no_silver");
+            r.ok = true;
+            r.count = n;
+            r.deniers = Math.Round(total, 2);
+            if (n < count)
+                r.why = stock - n < 1f ? "none_left" : n >= room ? "no_room" : "no_silver";
+            return r;
+        }
+
+        /// <summary>What selling up to `count` units from the hold would come to now, with nothing changing hands: how
+        /// many, what the market pays for them in all (each unit cheaper than the last, exactly as <see cref="Sell"/>
+        /// reckons it), the toll, and what they had cost.</summary>
+        public static DealResult QuoteSell(Market m, Hold hold, string goodId, int count, int month, double day, string who)
+        {
+            var r = new DealResult { why = "" };
+            var good = GameData.Good(goodId);
+            if (good == null || count <= 0)
+                return Fail(ref r, "nothing");
+            var g = m?.Get(goodId);
+            if (g == null)
+                return Fail(ref r, "not_dealt");
+            if (m.Disrupted(day))
+                return Fail(ref r, m.RuinedOn(day) ? "ruined" : "disrupted");
+            int n = Math.Min(count, hold.Count(goodId));
+            if (n <= 0)
+                return Fail(ref r, "none_aboard");
+            float stock = g.stock, glut = g.glut;
+            double total = 0.0;
+            for (int i = 0; i < n; i++)
+            {
+                total += PaysFor(m, g, month, who);
+                m.GotOne(g);
+            }
+            g.stock = stock;                                         // nothing has changed hands
+            g.glut = glut;
+            total = Math.Round(total, 2);
+            bool free = who == "player" && Chest.TollFreeAt(m.port, KapmanGame.Current != null ? KapmanGame.Current.Year : 793);
+            r.ok = true;
+            r.count = n;
+            r.deniers = total;
+            r.toll = free ? 0.0 : Math.Round(total * m.zone.toll * (who == "player" ? EncounterRunner.TollFactor(m.port.id, day) : 1.0), 2);
+            r.cost = hold.CostOf(good, n);
+            if (n < count)
+                r.why = "none_aboard";
+            return r;
+        }
+
         static DealResult Fail(ref DealResult r, string why)
         {
             r.ok = false;
```

### 6.6 `Assets/Kapman/Code/Game/Trade/Hold.cs`

```diff
--- a/Assets/Kapman/Code/Game/Trade/Hold.cs
+++ b/Assets/Kapman/Code/Game/Trade/Hold.cs
@@ -96,6 +96,24 @@
             return cost;
         }
 
+        /// <summary>What the oldest `n` units of a good cost (deniers), as <see cref="Remove"/> would reckon it, with
+        /// nothing taken out.</summary>
+        public double CostOf(GoodDef g, int n)
+        {
+            double cost = 0.0;
+            foreach (var l in lots)
+            {
+                if (n <= 0)
+                    break;
+                if (l.good != g || l.count <= 0)
+                    continue;
+                int take = Math.Min(n, l.count);
+                cost += l.cost * take / l.count;
+                n -= take;
+            }
+            return cost;
+        }
+
         /// <summary>The goods aboard (each once) and how many of each.</summary>
         public IEnumerable<(GoodDef good, int count)> Contents =>
             lots.GroupBy(l => l.good).Select(g => (g.Key, g.Sum(l => l.count)));
```

### 6.7 `Assets/Kapman/Code/Game/UI/Frame/FrameDeal.cs` (new file)

The enum is `LotSize`, not `Lot`: the hold already has a class `Kapman.Game.Lot`, and the compiler caught the clash
in the tests.

```csharp
using System;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>How much of a good one button deals in: one unit, half of what can go, or all that can go.</summary>
    public enum LotSize { One, Half, All }

    /// <summary>
    /// DEALING BY THE LOT, and SEEING THE MARKET SHE LIES AT (the strategy critic, 7 October: "deals by the lot with one
    /// settling"). A lot is one deal of the game's own (Trade.Buy, Trade.Sell with a count): the price moves unit by unit
    /// inside it as it always did, and the purse is asked ONCE for the sum, so there is one rounding where a click a unit
    /// made one for each. "All" is all that can go: for buying, what is on offer, what the hold has room for and what the
    /// silver pays; for selling, all of the good aboard. "Half" is half of that, rounded up.
    /// The market she lies at is SEEN (its prices go into the price notes) when she moors, once each day she lies
    /// there, and again after every deal, whatever windows are up: the word on a good is said against that note
    /// (Known.Here), so it must be there before the first window is opened.
    /// </summary>
    public static class FrameDeal
    {
        static string seenAt = "";
        static int seenDay = int.MinValue;

        /// <summary>Counts up each time the market she lies at is noted (a panel's stamp).</summary>
        public static int Seen { get; private set; }

        /// <summary>The market where she lies moored, brought up to now; null at sea or with no trade begun.</summary>
        public static Market MarketHere()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            return here != null && game != null && Trade.Started ? Markets.Of(here, game.Year, game.Day) : null;
        }

        /// <summary>She looks at the market she lies at: its prices go into the price notes, once a call and a day
        /// (`again`: now, whatever was noted: after a deal has moved them). True when a note was made.</summary>
        public static bool See(bool again = false)
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            if (game == null || here == null || !Trade.Started)
            {
                if (here == null)
                    seenAt = "";                                          // (cast off: the next mooring is a new call)
                return false;
            }
            int today = (int)Math.Floor(game.Day);
            if (!again && seenAt == here.id && seenDay == today)
                return false;
            var market = Markets.Of(here, game.Year, game.Day);
            if (market == null)
                return false;
            seenAt = here.id;
            seenDay = today;
            Trade.Ledger.Note(market, game.Day, game.Month);
            Seen++;
            return true;
        }

        /// <summary>How many units make a lot, of the most that can go: one, half of it rounded up, or all of it.</summary>
        public static int CountOf(LotSize lot, int most) =>
            most <= 0 ? 0 : lot == LotSize.One ? 1 : lot == LotSize.All ? most : (most + 1) / 2;

        /// <summary>What a lot would come to if it were dealt now (nothing changes hands): how many units, the sum, and
        /// for a sale the toll and what the goods had cost. Not ok, with why, when nothing can go.</summary>
        public static DealResult Quote(Market m, string good, bool buy, LotSize lot)
        {
            var game = KapmanGame.Current;
            if (m == null || game == null || !Trade.Started)
                return new DealResult { why = "not_dealt" };
            var all = buy ? Deal.QuoteBuy(m, Trade.Purse, Trade.Hold, good, int.MaxValue, game.Month, game.Day, "player")
                          : Deal.QuoteSell(m, Trade.Hold, good, int.MaxValue, game.Month, game.Day, "player");
            if (!all.ok || lot == LotSize.All)
                return all;
            int n = CountOf(lot, all.count);
            return buy ? Deal.QuoteBuy(m, Trade.Purse, Trade.Hold, good, n, game.Month, game.Day, "player")
                       : Deal.QuoteSell(m, Trade.Hold, good, n, game.Month, game.Day, "player");
        }

        /// <summary>Buys or sells a lot of a good at the market where she lies: ONE deal and one settlement, in the
        /// game's own words. The count is worked out at the click, from the market as it then is. Where nothing can go,
        /// one unit is asked for, so the deal itself says why not. Null when she lies at no market.</summary>
        public static TradeResult DealLot(string good, bool buy, LotSize lot)
        {
            var game = KapmanGame.Current;
            var market = MarketHere();
            if (game == null || market == null)
                return null;
            int n = lot == LotSize.One ? 1 : Math.Max(1, Quote(market, good, buy, lot).count);
            var r = buy ? Trade.Buy(market, good, n, game.Day, game.Month, game.Year)
                        : Trade.Sell(market, good, n, game.Day, game.Month, game.Year);
            if (r.ok)
                See(true);                                                // (the market as it is after the deal)
            return r;
        }

        // Enter Play Mode runs without a domain reload: each play begins having seen no market
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            seenAt = "";
            seenDay = int.MinValue;
            Seen = 0;
        }
    }
}
```

### 6.8 `Assets/Kapman/Code/Game/UI/Frame/Panels/TradePanel.cs` (the whole file, in place of the one there)

Kept from the file there: the id, title and order; the rows and their order; the at-sea face; the words of every
line; `DealOne(good, buy)` by name. Gone: the note-taking in `Fill` (it is `FrameDeal.See` now), the unused `Cap`.

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TRADE - the market where she lies: one row for each good that is on offer here, wanted here, or aboard, with the
    /// price as seen (what they ask, what they pay), how many lie here and how many are aboard, and under it the word on
    /// it from elsewhere, said against the price here (Known.WordOn). Buying and selling are BY THE LOT: one, half of
    /// what can go, all that can go, each button with its count and its sum on it, each one deal and one settlement
    /// (FrameDeal: the game's own Trade.Buy and Trade.Sell, with the toll, the laws and the coins).
    /// The window is BUILT only when its rows change (another port, a good sold out or bought in). The figures on it
    /// change in place a few times a second (Live): at the clock's fast speeds (ruling 26 of 7 October) prices move at
    /// every look, and a window built again under the pointer loses the click.
    /// At sea there is no market: the window says what is known of what she carries.
    /// </summary>
    public sealed class TradePanel : FramePanel
    {
        public override string Id => "trade";
        public override string Title => "Trade";
        public override int Order => 20;

        /// <summary>One good's row: the labels and buttons whose words change in place.</summary>
        sealed class Row
        {
            public GoodDef good;
            public Label quote, aboard;
            public VisualElement word;
            public string wordSaid;
            public readonly Button[] buy = new Button[3], sell = new Button[3];
        }

        static readonly LotSize[] Lots = { LotSize.One, LotSize.Half, LotSize.All };

        readonly List<Row> rows = new List<Row>();
        Label purse, saidLabel;
        string said = "", saidAt = "";
        int wordKey;

        static IEnumerable<MarketGood> Goods(Market m) =>
            m.Goods.Where(g => g.OnOffer || g.wanted > 0f || Trade.Hold.Count(g.good.id) > 0)
             .OrderByDescending(g => Trade.Hold.Count(g.good.id) > 0)
             .ThenByDescending(g => g.OnOffer)
             .ThenBy(g => g.good.name);

        /// <summary>What makes the window be built again: where she lies, whether its market is open, and which goods
        /// have a row with which buttons. Not the prices: they change in place.</summary>
        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(FrameUi.Mix(17, here != null ? here.id : ""), Trade.Started);
            var m = FrameDeal.MarketHere();
            if (m == null || game == null)
            {
                // at sea: what she carries and what is known of it (no buttons here, so it may be built again each day)
                h = FrameUi.Mix(FrameUi.Mix(h, FrameGame.HoldStamp()), Known.Version);
                return FrameUi.Mix(h, game != null ? (int)System.Math.Floor(game.Day) : 0);
            }
            FrameDeal.See();
            h = FrameUi.Mix(h, m.Disrupted(game.Day));
            foreach (var g in Goods(m))
                h = FrameUi.Mix(FrameUi.Mix(h, g.good.id), (g.OnOffer ? 1 : 0) + (Trade.Hold.Count(g.good.id) > 0 ? 2 : 0));
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            rows.Clear();
            purse = saidLabel = null;
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("No trade yet."));
                return;
            }
            var here = FrameGame.PortHere;
            var ask = Known.AskNow();
            if (here == null)
            {
                body.Add(FrameUi.H("No market here"));
                body.Add(FrameUi.P("She lies at no market. What you know of what she carries:"));
                int n = 0;
                foreach (var (good, count) in Trade.Hold.Contents)
                {
                    n++;
                    body.Add(FrameUi.Ln(FrameGame.Units(good, count), null));
                    body.Add(new WordLine(Known.WordOn(good, ask)));
                }
                if (n == 0)
                    body.Add(FrameUi.P("She is empty."));
                return;
            }
            var market = FrameDeal.MarketHere();
            FrameDeal.See();                                              // (looking at the market is seeing its prices)
            if (saidAt != here.id)
            {
                said = "";                                                // (the last deal's words were another market's)
                saidAt = here.id;
            }
            body.Add(FrameUi.H(Wording.MarketTitle(here)));
            double toll = market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day);
            body.Add(FrameUi.P($"Prices in {market.zone.unit}. " +
                               (toll > 0.0005 ? $"The toll takes {toll * 100.0:0.#} in the hundred of what you sell here." : "No toll is taken on what you sell here.")));
            purse = FrameUi.P("");
            body.Add(purse);
            saidLabel = FrameUi.Text(said);
            body.Add(saidLabel);
            if (market.Disrupted(game.Day))
            {
                body.Add(FrameUi.P(market.Ruined ? "Nobody trades here any more." : "The market is shut."));
                Live();
                return;
            }
            foreach (var g in Goods(market))
            {
                var row = new Row { good = g.good };
                bool held = Trade.Hold.Count(g.good.id) > 0;
                var deals = new VisualElement();
                deals.AddToClassList("frame-lots");
                if (held)
                    deals.Add(LotRow(row, false));
                if (g.OnOffer)
                    deals.Add(LotRow(row, true));
                row.aboard = held ? FrameUi.Num("") : null;
                var line = FrameUi.Ln(g.good.name, " ", row.aboard, deals);
                row.quote = line.Q<Label>(className: "frame-ln-small");
                body.Add(line);
                row.word = new VisualElement();
                body.Add(row.word);
                rows.Add(row);
            }
            if (rows.Count == 0)
                body.Add(FrameUi.P("Nothing is bought or sold here now."));
            wordKey = 0;
            Live();
        }

        /// <summary>The three buttons of one side of a row: one unit ("Buy", "Sell"), half of what can go, all of it.</summary>
        VisualElement LotRow(Row row, bool buy)
        {
            var buttons = buy ? row.buy : row.sell;
            string id = row.good.id;
            for (int i = 0; i < Lots.Length; i++)
            {
                LotSize lot = Lots[i];
                buttons[i] = FrameUi.Btn(buy ? "Buy" : "Sell", () => DealLot(id, buy, lot));
            }
            return FrameUi.Row(buttons);
        }

        /// <summary>The figures, in place: what her silver is worth here and the room left, the last deal's words, each
        /// row's quote and its lots' counts and sums, and the word on each good when what is known has moved.</summary>
        protected override void Live()
        {
            var game = KapmanGame.Current;
            var market = FrameDeal.MarketHere();
            if (game == null || market == null || purse == null)
                return;
            var zone = market.zone;
            int month = game.Month;
            purse.text = $"Your silver is worth {zone.Show(Trade.Purse.Worth(zone, TradeRules.Current))} here. Room left in the hold: {Trade.Hold.Free:0} kg.";
            saidLabel.text = said;
            saidLabel.style.display = said.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            // the word on each good is asked again only when something was learned, the market was looked at, or the day turned
            int key = FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameDeal.Seen), (int)System.Math.Floor(game.Day));
            bool words = key != wordKey;
            wordKey = key;
            var ask = words ? Known.AskNow() : default;
            foreach (var row in rows)
            {
                var g = market.Get(row.good.id);
                if (g == null)
                    continue;
                string id = row.good.id;
                int held = Trade.Hold.Count(id);
                string asks = g.OnOffer ? $"They ask {zone.Show(Deal.AsksFor(market, g, month, "player"))}, {Mathf.FloorToInt(g.stock)} here. " : "None on offer. ";
                string pays = $"They pay {zone.Show(Deal.PaysFor(market, g, month, "player"))}.";
                string unit = string.IsNullOrEmpty(row.good.unit) ? "" : $" By the {row.good.unit.Split(' ')[0]}.";
                double cost = held > 0 ? Trade.Hold.CostEach(id) : 0.0;
                string yours = held == 0 ? "" : cost > 0.05 ? $" Yours cost {zone.Show(cost)} each." : " Yours cost you nothing.";
                if (row.quote != null)
                    row.quote.text = asks + pays + unit + yours;
                if (row.aboard != null)
                    row.aboard.text = held.ToString();
                Lots3(row.sell, market, id, false, zone);
                Lots3(row.buy, market, id, true, zone);
                if (!words)
                    continue;
                var w = Known.WordOn(row.good, ask);
                string text = w.Text;
                if (text == row.wordSaid)
                    continue;
                row.wordSaid = text;
                row.word.Clear();
                row.word.Add(new WordLine(w));
            }
        }

        /// <summary>One side's three buttons: "Buy" (one), "3 for 127 d" (half), "all 6 for 262 d". Half is hidden where
        /// it is the same as one or as all; all is hidden where only one can go.</summary>
        static void Lots3(Button[] b, Market market, string good, bool buy, ZoneDef zone)
        {
            if (b[0] == null)
                return;
            var all = FrameDeal.Quote(market, good, buy, LotSize.All);
            int most = all.ok ? all.count : 0;
            int half = FrameDeal.CountOf(LotSize.Half, most);
            b[0].tooltip = most > 0 ? null : buy ? "None can be bought now: press it to hear why." : "None can be sold now: press it to hear why.";
            // (a hidden button says nothing: only the first of a side ever reads "Buy" or "Sell")
            bool showHalf = half > 1 && half < most;
            b[1].style.display = showHalf ? DisplayStyle.Flex : DisplayStyle.None;
            b[1].text = showHalf ? $"{half} for {zone.Show(FrameDeal.Quote(market, good, buy, LotSize.Half).deniers)}" : "";
            b[2].style.display = most > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            b[2].text = most > 1 ? $"all {most} for {zone.Show(all.deniers)}" : "";
        }

        /// <summary>Buys or sells a lot of a good at the market where she lies (the buttons of a row): one deal, one
        /// settlement, what came of it said in the window and in the log. Null when she lies at no market.</summary>
        public TradeResult DealLot(string good, bool buy, LotSize lot)
        {
            var r = FrameDeal.DealLot(good, buy, lot);
            if (r == null)
                return null;
            said = r.text ?? "";
            FrameLog.Add(said);
            Redraw();
            return r;
        }

        /// <summary>One unit (the name the first build of the frame gave it).</summary>
        public TradeResult DealOne(string good, bool buy) => DealLot(good, buy, LotSize.One);
    }
}
```

### 6.9 `Assets/Kapman/Code/Game/UI/Frame/Panels/WordPanel.cs` (the whole file, in place of the one there)

`Ports`, `Goods`, `Best` and `AgeClass` are static and need no screen, so the tests reach them.

```csharp
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE WORD - its full face is THE LEDGER (the strategy critic, 7 October: "goods down, known ports across, the price
    /// as a number where seen and a word where told, coloured by age"). Goods down the left; across, every market the
    /// merchant has word of, the one she lies at first; in each cell what is known of that good's price there and how old
    /// the word is: a plain figure she saw herself within the week, "about" a figure a man who saw it told her (or she
    /// saw longer ago), a word against where she lies for talk, a dash for nothing. Two looks: what they pay (where to
    /// sell) and what they ask (where to buy, and whether there is any). All in one reckoning, that of the port the row
    /// is said against. A pressed cell says its whole line: who said it, where and when.
    /// Every cell comes from Known.Cell, which reads the same notes by the same rules as the word on an item
    /// (Known.Word), so the ledger and the line under a good never disagree. No market is asked.
    /// </summary>
    public sealed class WordPanel : FramePanel
    {
        public override string Id => "word";
        public override string Title => "The word";
        public override int Order => 50;

        /// <summary>The most ports across (the freshest are kept).</summary>
        public const int MostPorts = 16;
        // the sizes that keep the names, the rows and the columns in line (the fonts are the stylesheet's)
        const float NameWide = 210f, CellWide = 124f, HeadHigh = 88f, RowHigh = 58f;

        bool asks;                              // false: what they pay (where to sell); true: what they ask (where to buy)
        bool every;                             // false: what she carries and what is on offer where she lies; true: every good on offer anywhere
        string pickedGood = "", pickedPort = "";

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameGame.HoldStamp());
            h = FrameUi.Mix(FrameUi.Mix(h, FrameDeal.Seen), Port.LyingId() ?? "");
            if (!Trade.Started || game == null)
                return h;
            h = FrameUi.Mix(h, Trade.Ledger.Count);
            // the ages: built again when a port's newest word passes into another age word, not with every day (at the
            // clock's fast speeds a day is a few seconds: ruling 26)
            foreach (var n in Trade.Ledger.Notes)
                h = FrameUi.Mix(h, Known.AgeKey(n.day, game.Day));
            return h;
        }

        /// <summary>Its floating window is a wide one: the ledger wants the room (the stylesheet's frame-float-ledger).
        /// In a tiled window of the left side this finds no floating window and does nothing.</summary>
        protected override void OnShown()
        {
            for (var e = Body; e != null; e = e.parent)
                if (e.ClassListContains("frame-float"))
                {
                    e.AddToClassList("frame-float-ledger");
                    return;
                }
        }

        // ------------------------------------------------------------------ what the ledger holds (no screen needed)

        /// <summary>The ports across: every market there is a price note of, young enough to count (knowing_793.json
        /// "word" known_days), the freshest <see cref="MostPorts"/> of them; the port she lies at first, then as the
        /// ports table has them, so a column keeps its place. `more`: how many were left out.</summary>
        public static List<PortDef> Ports(WordAsk ask, out int more)
        {
            more = 0;
            var kept = new List<PortDef>();
            if (!Trade.Started)
                return kept;
            var found = new List<(PortDef port, double day)>();
            foreach (var n in Trade.Ledger.Notes)
            {
                var p = GameData.Port(n.port);
                if (p != null && n.day <= ask.today && ask.today - n.day < Known.Rules.wordKnownDays)
                    found.Add((p, n.day));
            }
            found.Sort((a, b) => a.day != b.day ? b.day.CompareTo(a.day) : a.port.index.CompareTo(b.port.index));
            PortDef first = ask.here != null && found.Exists(f => f.port == ask.here) ? ask.here : null;
            if (first != null)
                kept.Add(first);
            foreach (var f in found)
                if (kept.Count < MostPorts && !kept.Contains(f.port))
                    kept.Add(f.port);
            more = found.Count - kept.Count;
            kept.Sort((a, b) => a == b ? 0 : a == first ? -1 : b == first ? 1 : a.index.CompareTo(b.index));
            return kept;
        }

        /// <summary>The goods down: what she carries first; then what was on offer where she lies when she last looked
        /// (`every`: what any of the ports is known to have on offer), as the goods table has them.</summary>
        public static List<GoodDef> Goods(WordAsk ask, List<PortDef> ports, bool every)
        {
            var goods = new List<GoodDef>();
            if (!Trade.Started)
                return goods;
            foreach (var (good, _) in Trade.Hold.Contents)
                goods.Add(good);
            foreach (var g in GameData.Goods)
            {
                if (goods.Contains(g))
                    continue;
                bool offered = false;
                if (every)
                {
                    var rowAsk = Known.AskFor(g, ask);
                    for (int i = 0; i < ports.Count && !offered; i++)
                    {
                        var c = Known.Cell(g, ports[i], rowAsk);
                        offered = c.grade != PriceGrade.None && c.onOffer;
                    }
                }
                else if (ask.here != null)
                {
                    var c = Known.Cell(g, ask.here, Known.AskFor(g, ask));
                    offered = c.grade != PriceGrade.None && c.onOffer;
                }
                if (offered)
                    goods.Add(g);
            }
            return goods;
        }

        /// <summary>The best cell of a row among those that are figures (seen, or "about"): where they pay most, or
        /// (`asks`) where it is on offer cheapest. -1 with fewer than two figures to choose between. A cell that is only
        /// a word is never marked: the mark would give away the number behind it.</summary>
        public static int Best(IReadOnlyList<PriceCell> cells, bool asks)
        {
            int best = -1, figures = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if ((c.grade != PriceGrade.Exact && c.grade != PriceGrade.About) || (asks && !c.onOffer))
                    continue;
                figures++;
                if (best < 0 || (asks ? c.asks < cells[best].asks : c.pays > cells[best].pays))
                    best = i;
            }
            return figures >= 2 ? best : -1;
        }

        /// <summary>The class a cell takes from the age of its word: age-new (today, yesterday), age-week (to ten days),
        /// age-month (to two months), age-old.</summary>
        public static string AgeClass(string ageKey)
        {
            switch (ageKey)
            {
                case "age.this_morning": case "age.yesterday": return "age-new";
                case "age.two_days": case "age.few_days": case "age.last_week": return "age-week";
                case "age.fortnight": case "age.last_month": return "age-month";
                default: return "age-old";
            }
        }

        // ------------------------------------------------------------------ the full face

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("No word of any market yet."));
                return;
            }
            FrameDeal.See();                                              // (the market she lies at is in front of her)
            var ask = Known.AskNow();
            var ports = Ports(ask, out int more);
            if (ports.Count == 0)
            {
                body.Add(FrameUi.P("You have no word of any market's prices yet. Look at a market, or talk to people in port."));
                return;
            }
            var goods = Goods(ask, ports, every);
            body.Add(FrameUi.H(ports.Count == 1 ? "The ledger: one market known" : $"The ledger: {ports.Count} markets known"));
            // (what it is all set against is asked of the first good down; with no row at all, of any good she has seen priced)
            var probe = goods.Count > 0 ? goods[0] : null;
            for (int i = 0; probe == null && i < GameData.Goods.Count; i++)
                if (Known.Here(GameData.Goods[i], ask).Known)
                    probe = GameData.Goods[i];
            body.Add(FrameUi.P(Against(ask, probe) +
                               " A plain figure you saw yourself this week. \"About\" is a figure a man who saw it told you, or one you saw longer ago. " +
                               "A word is talk. Paler is older."));
            body.Add(FrameUi.Row(
                Pick("What they pay", !asks, () => asks = false),
                Pick("What they ask", asks, () => asks = true),
                Pick("Aboard, and on offer here", !every, () => every = false),
                Pick("Every good on offer anywhere", every, () => every = true)));
            var pGood = GameData.Good(pickedGood);
            var pPort = GameData.Port(pickedPort);
            if (pGood != null && pPort != null && ports.Contains(pPort))
                body.Add(new WordLine(Known.Word(pGood, pPort, ask)));
            else
                body.Add(FrameUi.P("Press a cell to hear who said it, where and when."));
            if (goods.Count == 0)
            {
                body.Add(FrameUi.P("She carries nothing, and you have seen nothing on offer where she lies. The second pair of buttons shows every good you know to be on offer anywhere."));
                return;
            }

            // the names stand still at the left; the ports scroll sideways beside them; every row is one height
            var grid = new VisualElement();
            grid.AddToClassList("ledger");
            grid.style.flexDirection = FlexDirection.Row;
            var names = new VisualElement();
            names.AddToClassList("ledger-names");
            names.style.width = NameWide;
            names.style.flexShrink = 0;
            var scroll = new ScrollView(ScrollViewMode.Horizontal);
            scroll.AddToClassList("ledger-scroll");
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            var table = new VisualElement();
            table.AddToClassList("ledger-table");

            names.Add(Box(NameWide, HeadHigh, "ledger-corner", asks ? "What they ask" : "What they pay", "for one of each"));
            var head = Line(HeadHigh);
            foreach (var port in ports)
                head.Add(PortHead(port, ask, game.Day));
            table.Add(head);
            var cells = new List<PriceCell>(ports.Count);
            foreach (var good in goods)
            {
                int held = Trade.Hold.Count(good.id);
                var name = Box(NameWide, RowHigh, "ledger-name", good.name, held > 0 ? held + " aboard" : good.cat);
                name.EnableInClassList("aboard", held > 0);
                names.Add(name);
                var rowAsk = Known.AskFor(good, ask);                     // (what the row is said against, worked out once)
                cells.Clear();
                foreach (var port in ports)
                    cells.Add(Known.Cell(good, port, rowAsk));
                int best = Best(cells, asks);
                var row = Line(RowHigh);
                for (int i = 0; i < ports.Count; i++)
                    row.Add(CellBox(good, ports[i], cells[i], rowAsk, i == best));
                table.Add(row);
            }
            scroll.Add(table);
            grid.Add(names);
            grid.Add(scroll);
            body.Add(grid);
            if (more > 0)
                body.Add(FrameUi.P(more == 1 ? "And one market more, whose word is older." : $"And {more} markets more, whose word is older."));
        }

        /// <summary>What the ledger is set against, and in whose reckoning, in a sentence.</summary>
        static string Against(WordAsk ask, GoodDef good)
        {
            var here = Known.Here(good, ask);
            if (!here.Known)
                return "You have seen no market yourself lately, so there is nothing to set these prices against: each stands in its own port's reckoning.";
            string unit = TradeRules.Current.ZoneOf(here.port.region)?.unit ?? "d";
            return here.lying
                ? $"Every price is set against {here.port.name}, where she lies, and reckoned in {unit} as it is there."
                : $"Every price is set against {here.port.name}, where you last saw a market ({Wording.WordText(here.port, Known.AgeKey(here.day, ask.today))}), and reckoned in {unit} as it is there.";
        }

        Button Pick(string text, bool on, System.Action set)
        {
            var b = FrameUi.Btn(text, () =>
            {
                set();
                Redraw();
            });
            b.EnableInClassList("on", on);
            return b;
        }

        static VisualElement Line(float high)
        {
            var row = new VisualElement();
            row.AddToClassList("ledger-row");
            row.style.flexDirection = FlexDirection.Row;
            row.style.height = high;
            row.style.flexShrink = 0;
            return row;
        }

        static VisualElement Box(float wide, float high, string cls, string top, string under)
        {
            var box = new VisualElement();
            box.AddToClassList(cls);
            box.style.width = wide;
            box.style.height = high;
            box.style.flexShrink = 0;
            var a = new Label(top ?? "");
            a.AddToClassList("ledger-top");
            box.Add(a);
            if (!string.IsNullOrEmpty(under))
            {
                var b = new Label(under);
                b.AddToClassList("ledger-under");
                box.Add(b);
            }
            return box;
        }

        /// <summary>A port's head: its name, how the newest word of it came and how old that is, and its toll.</summary>
        static VisualElement PortHead(PortDef port, WordAsk ask, double today)
        {
            var newest = Trade.Ledger.Latest(port.id);
            string how = port == ask.here ? "here" : newest == null ? "" : HowWord(newest) + ", " + Wording.WordText(port, Known.AgeKey(newest.day, today));
            var zone = TradeRules.Current.ZoneOf(port.region);
            string toll = zone != null && zone.toll > 0.0005f ? $"toll {zone.toll * 100f:0.#} in 100" : "no toll";
            var box = Box(CellWide, HeadHigh, "ledger-port", port.name, how);
            box.EnableInClassList("here", port == ask.here);
            var t = new Label(toll);
            t.AddToClassList("ledger-under");
            box.Add(t);
            return box;
        }

        static string HowWord(PriceNote n)
        {
            switch (n.how)
            {
                case How.Seen: case How.Own: return "seen";
                case How.Told: return "told";
                case How.Written: return "a letter";
                case How.Rumour: return "a rumour";
                default: return "talk";
            }
        }

        /// <summary>One cell: the price as it is known, and under it the age of the word. Pressed, its whole line is said
        /// above the ledger.</summary>
        VisualElement CellBox(GoodDef good, PortDef port, PriceCell cell, WordAsk rowAsk, bool best)
        {
            if (cell.grade == PriceGrade.None)
            {
                var none = Box(CellWide, RowHigh, "ledger-cell", "-", null);
                none.AddToClassList("none");
                return none;
            }
            var b = new Button(() =>
            {
                pickedGood = good.id;
                pickedPort = port.id;
                Redraw();
            });
            b.AddToClassList("ledger-cell");
            b.AddToClassList(cell.grade == PriceGrade.Exact ? "exact" : cell.grade == PriceGrade.About ? "about" : "word");
            b.AddToClassList(AgeClass(cell.ageKey));
            b.EnableInClassList("best", best);
            b.EnableInClassList("sel", good.id == pickedGood && port.id == pickedPort);
            b.style.flexDirection = FlexDirection.Column;                 // (the price over the age, whatever a button's own style says)
            b.style.width = CellWide;
            b.style.height = RowHigh;
            b.style.flexShrink = 0;
            var price = new Label(Known.CellText(cell, port, rowAsk, asks)) { pickingMode = PickingMode.Ignore };
            price.AddToClassList("ledger-top");
            var age = new Label(port == rowAsk.here && cell.grade == PriceGrade.Exact ? "here" : Wording.WordText(port, cell.ageKey)) { pickingMode = PickingMode.Ignore };
            age.AddToClassList("ledger-under");
            b.Add(price);
            b.Add(age);
            return b;
        }
    }
}
```

### 6.10 `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs` (two places; find them by their text)

In `Update`, in the block that runs four times a second (about line 360 when I read it). Find:

```csharp
                nextFill = now + 0.25f;
                for (int i = 0; i < shown.Count; i++)
                    shown[i].Tick();
```

Put in its place:

```csharp
                nextFill = now + 0.25f;
                FrameDeal.See();                                        // (moored: she sees the market, whatever windows are up)
                for (int i = 0; i < shown.Count; i++)
                    shown[i].Tick();
```

In `FillTop` (line 709 at 13:00; it was 705 ten minutes before). Find:

```csharp
                silver.text = $"{FrameGame.Silver:0} d";
```

Put in its place:

```csharp
                // in port: what the purse buys here, in this market's own reckoning (the same figure the Trade window
                // gives); at sea: all her silver by weight
                var lying = FrameGame.PortHere;
                var zoneHere = lying != null ? TradeRules.Current.ZoneOf(lying.region) : null;
                silver.text = zoneHere != null ? zoneHere.Show(Trade.Purse.Worth(zoneHere, TradeRules.Current)) : $"{FrameGame.Silver:0} d";
```

The second is the smallest mend of the critic's con 7 and half of his con 6 (the market in grams and the purse in
d at Ripa). It is here because the Trade window now says "Your silver is worth 81 d here" and the bar above it must
not say 86. If another builder has the top bar, give them this and leave it out. With it, the figure on the bar
steps when she moors (86 d at sea, 81 d at Dorestat, 139 g silver at Ripa): that step is the mint's tenth, and it is
true. Both places compiled in a copy of `FrameRoot.cs` as it was at 13:00.

### 6.11 `Assets/Kapman/Game/UI/Frame.uss` (a draft to append; never seen on a screen)

The font sizes are the frame's today (16 and 13 px). Give `.ledger-top` whatever the text-size mend gives
`.frame-ln-name` and `.ledger-under` whatever it gives `.frame-ln-small`: the ledger's row is 58 px high and its head
88 px (`WordPanel.cs`, the constants at its top), which hold a 22 px line over an 18 px one.

```css
/* Trade: the two short rows of lot buttons at the right end of a good's line (selling over buying) */
.frame-root .frame-lots {
    flex-direction: column;
    align-items: flex-end;
    flex-shrink: 0;
    margin-left: 8px;
}

.frame-root .frame-lots .frame-row {
    margin: 1px 0;
    justify-content: flex-end;
}

.frame-root .frame-lots .frame-row .frame-k {
    margin-right: 0;
    margin-left: 6px;
    margin-bottom: 2px;
}

/* The word: the ledger. Its floating window is a wide one (WordPanel.OnShown puts the class on it) */
.frame-root .frame-float.frame-float-ledger {
    width: 76%;
    max-width: 1500px;
    max-height: 84%;
}

.frame-root .ledger {
    margin-top: 6px;
}

.frame-root .ledger-names {
    border-right-width: 1px;
    border-right-color: var(--frame-rule);
}

.frame-root .ledger-corner,
.frame-root .ledger-name,
.frame-root .ledger-port,
.frame-root .ledger-cell {
    flex-direction: column;
    padding: 3px 6px;
    justify-content: center;
    border-bottom-width: 1px;
    border-bottom-color: var(--frame-rule);
}

.frame-root .ledger-cell {
    flex-direction: column;
    margin: 0;
    border-radius: 0;
    border-left-width: 0;
    border-right-width: 0;
    border-top-width: 0;
    background-color: rgba(0, 0, 0, 0);
    align-items: flex-end;
}

.frame-root .ledger-cell:hover {
    background-color: rgba(240, 210, 122, 0.1);
}

.frame-root .ledger-cell.sel {
    background-color: rgba(240, 210, 122, 0.16);
}

.frame-root .ledger-top {
    font-size: 16px;
    color: var(--frame-paper);
    white-space: nowrap;
}

.frame-root .ledger-under {
    font-size: 13px;
    color: var(--frame-mute);
    white-space: nowrap;
}

.frame-root .ledger-name.aboard .ledger-under,
.frame-root .ledger-port.here .ledger-top,
.frame-root .ledger-cell.best .ledger-top {
    color: var(--frame-gold);
}

/* how exactly it is known: a figure she saw (bold), about (plain), a word (italic), nothing */
.frame-root .ledger-cell.exact .ledger-top {
    -unity-font: url("Fonts/GentiumBookPlus-Bold.ttf");
}

.frame-root .ledger-cell.word .ledger-top {
    -unity-font-style: italic;
}

.frame-root .ledger-cell.none .ledger-top {
    color: var(--frame-mute);
}

/* paler is older (the age is also written under every price, so nothing rests on the shade alone) */
.frame-root .ledger-cell.age-week {
    opacity: 0.88;
}

.frame-root .ledger-cell.age-month {
    opacity: 0.72;
}

.frame-root .ledger-cell.age-old {
    opacity: 0.56;
}
```

### 6.12 The two brief faces (paste when `FramePanel` has `Brief` and `BriefStamp`: brief_faces.md, section 3)

Into `TradePanel`, before the class's last brace (it compiled against a copy of `FramePanel.cs` with the two
members `protected virtual void Brief(List<string> lines) { }` and `protected virtual int BriefStamp() => Stamp();`
added). The Trade panel needs its own `BriefStamp`: its `Stamp` no longer moves with the prices.

```csharp
        /// <summary>The brief face (ruling 9): what her silver is worth here and the room left; the lot aboard that would
        /// bring most into the purse here; the toll; the word on that good. At sea: the word on what she carries.</summary>
        protected override void Brief(List<string> lines)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                lines.Add("No trade yet.");
                return;
            }
            var ask = Known.AskNow();
            var market = FrameDeal.MarketHere();
            if (market == null)
            {
                lines.Add("She lies at no market.");
                foreach (var (good, _) in Trade.Hold.Contents)
                    lines.Add(Known.WordOn(good, ask).Text);
                return;
            }
            if (market.Disrupted(game.Day))
            {
                lines.Add(market.Ruined ? "Nobody trades here any more." : "The market is shut.");
                return;
            }
            var zone = market.zone;
            lines.Add($"Your silver is worth {zone.Show(Trade.Purse.Worth(zone, TradeRules.Current))} here. Room for {Trade.Hold.Free:0} kg.");
            GoodDef bestGood = null;
            var best = default(DealResult);
            foreach (var (good, _) in Trade.Hold.Contents)
            {
                var q = FrameDeal.Quote(market, good.id, false, LotSize.All);
                if (q.ok && (bestGood == null || q.deniers - q.toll > best.deniers - best.toll))
                {
                    bestGood = good;
                    best = q;
                }
            }
            if (bestGood != null)
                lines.Add($"All {FrameGame.Units(bestGood, best.count)} would bring {zone.Show(best.deniers - best.toll)} here" + (best.toll > 0.005 ? ", the toll paid." : "."));
            else
                lines.Add("She carries nothing to sell here.");
            double toll = zone.toll * EncounterRunner.TollFactor(market.port.id, game.Day);
            lines.Add(toll > 0.0005 ? $"The toll takes {toll * 100.0:0.#} in the hundred of what you sell." : "No toll is taken here.");
            if (bestGood != null)
                lines.Add(Known.WordOn(bestGood, ask).Text);
        }

        /// <summary>The brief face follows the figures (the full face's stamp does not: there they change in place). It
        /// has no buttons, so building it again loses no click.</summary>
        protected override int BriefStamp()
        {
            int h = FrameUi.Mix(FrameUi.Mix(Stamp(), FrameGame.HoldStamp()), Known.Version);
            var game = KapmanGame.Current;
            var market = FrameDeal.MarketHere();
            if (game == null || market == null)
                return h;
            h = FrameUi.Mix(FrameUi.Mix(h, FrameDeal.Seen), (int)System.Math.Floor(game.Day));
            h = FrameUi.Mix(h, (int)(Trade.Purse.Worth(market.zone, TradeRules.Current) * 4.0));
            foreach (var (good, _) in Trade.Hold.Contents)
                h = FrameUi.Mix(h, (int)FrameDeal.Quote(market, good.id, false, LotSize.All).deniers);
            return h;
        }
```

Into `WordPanel`, before the class's last brace (its `Stamp` serves the brief face as it is):

```csharp
        /// <summary>The brief face (ruling 9): the word on the first two goods she carries, each said against where she
        /// lies, and how many markets the ledger holds.</summary>
        protected override void Brief(List<string> lines)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                lines.Add("No word of any market yet.");
                return;
            }
            var ask = Known.AskNow();
            var ports = Ports(ask, out int more);
            if (ports.Count == 0)
            {
                lines.Add("No word of any market yet.");
                return;
            }
            int said = 0;
            foreach (var (good, _) in Trade.Hold.Contents)
            {
                if (said >= 2)
                    break;
                lines.Add(Known.WordOn(good, ask).Text);
                said++;
            }
            if (said == 0)
                lines.Add("She carries nothing to have word on.");
            int known = ports.Count + more;
            lines.Add(known == 1 ? "One market in the ledger." : $"{known} markets in the ledger.");
        }
```

These take the place of the brief lines drafted for trade and word in brief_faces.md, section 2 (which were marked
there as read by grep only).

## 7. The tests

None was run. All compile against the mended game. What each expects was worked by hand from the code, and for the
purse from the Python copy.

### 7.1 `Assets/Kapman/Tests/EditMode/KnownWordTests.cs` (five of its twenty tests follow the new rule; the other fifteen are untouched)

I walked the fifteen through the new code by hand. They tell prices to a merchant who has seen none, so nothing is
set against anything and the old words and wedges stand ("paying well", "pays poorly", wedge up or down); and
`TheNoteOnAnItemIsTheFreshestWordFromElsewhere` asks only for rules and ports, which have not moved.

| Test | What changes |
|---|---|
| `Rule2APriceSeenLatelyIsSaidWithItsNumber` | the wedge is 0, not up: London is the one place she has seen wine priced, so there is nothing to set it against (it took its wedge from the table value) |
| `Rule2LastsSevenDays` | her own sight a week old now says "about" and a rough figure, where it said a word |
| `Rule3APriceToldLatelyIsSaidByAPerson` | a skipper who saw it: key `told.about`, "about" and a rough figure |
| `AToldPriceNeverRendersANumber` | becomes `OnlyATellingAtFirstHandSaysAFigureAndItSaysAbout`: the skipper's lines say "about" and a figure; the other four ways of hearing still never show a digit; with `told_figure` off the skipper's do not either |
| `TheWordingTableHasEveryLineAndKeepsToldPricesInWords` | the lines that may ask for `{price}` are `seen`, `saw`, `told.about`, `cell.about`; the last three must say "about {price}"; the first three must have `{Against}` |

```diff
--- a/Assets/Kapman/Tests/EditMode/KnownWordTests.cs
+++ b/Assets/Kapman/Tests/EditMode/KnownWordTests.cs
@@ -153,9 +153,7 @@
             Assert.AreEqual(price, w.Slot("price"), "in the port's own reckoning");
             Assert.IsTrue(HasDigit(price));
             Assert.AreEqual($"You saw Rhenish wine pay {price} at Lundenwic, two days back.", w.Text);
-            Known.PaysKey(pays, Wine, out int tilt);
-            Assert.AreEqual(tilt, w.tilt);
-            Assert.AreEqual(1, w.tilt, "London wants wine: it pays well there");
+            Assert.AreEqual(0, w.tilt, "the one place she has seen wine priced: there is nothing to set it against");
         }
 
         [Test]
@@ -167,8 +165,9 @@
             Assert.AreEqual(4, later.rule, "seen a week ago and more: no longer a number, but still the merchant's own sight");
             Assert.AreEqual("saw", later.key);
             Assert.AreEqual(WordMark.Seen, later.mark);
-            Assert.IsFalse(HasDigit(later.Text), later.Text);
-            StringAssert.StartsWith("You saw Lundenwic paying ", later.Text);
+            string about = Known.Rough(Trade.Ledger.Latest("lundenwic").prices["rhenish_wine"].pays, TradeRules.Current.ZoneOf(London.region));
+            Assert.AreEqual(about, later.Slot("price"), "she did see it: a rough figure, no longer the price to the penny");
+            Assert.AreEqual($"You saw Lundenwic paying about {about} for Rhenish wine, last week. It may have changed.", later.Text);
         }
 
         [Test]
@@ -177,14 +176,16 @@
             Tell(London, Today - 5.0, How.Told, 0, "skipper", "dorestad");
             var w = Known.Word(Wine, London, Ask());
             Assert.AreEqual(3, w.rule);
-            Assert.AreEqual("told", w.key);
+            Assert.AreEqual("told.about", w.key, "he saw it himself: about, and a rough figure");
             Assert.AreEqual(WordMark.Heard, w.mark);
             Assert.AreEqual("age.few_days", w.ageKey);
             // (with no wording table loaded every port's people read as Frankish; the table makes Dorestad's Frisian)
             Assert.AreEqual("a Frankish skipper", w.Slot("person"), "a person of his people, not the note's label");
             Assert.AreEqual("well", w.Slot("pays"));
             Assert.AreEqual(1, w.tilt);
-            Assert.AreEqual("A Frankish skipper said Lundenwic was paying well for Rhenish wine. A few days back.", w.Text);
+            string about = Known.Rough(Trade.Ledger.Latest("lundenwic").prices["rhenish_wine"].pays, TradeRules.Current.ZoneOf(London.region));
+            Assert.AreEqual(about, w.Slot("price"), "in the port's own reckoning while she has seen no price to set it against");
+            Assert.AreEqual($"A Frankish skipper said Lundenwic was paying about {about} for Rhenish wine, a few days back.", w.Text);
             StringAssert.DoesNotContain("label", w.Text);
             Wording.Load(Asset("wording_793.json"));
             Assert.AreEqual("frisian", Wording.CultureOf(Dorestad));
@@ -414,9 +415,9 @@
         // ------------------------------------------------------------------ a told price never shows a number
 
         [Test]
-        public void AToldPriceNeverRendersANumber()
+        public void OnlyATellingAtFirstHandSaysAFigureAndItSaysAbout()
         {
-            Tell(London, Today - 1.0, How.Told, 0, "skipper", "dorestad");          // rule 3: he said
+            Tell(London, Today - 1.0, How.Told, 0, "skipper", "dorestad");          // rule 3: he saw it and said so: about
             Tell(Quentovic, Today - 9.0, How.Talk, 1, "quay", "dorestad");          // rule 3: someone on the quay said
             Tell(Dorestad, Today - 5.0, How.Talk, 3, "quay", "quentovic");          // rule 4: they say
             Tell(GameData.Port("birka"), Today - 20.0, How.Rumour, 4);              // rule 4: a rumour
@@ -434,16 +435,34 @@
                     var w = Known.Word(good, port, Ask());
                     Assert.IsTrue(w.rule == 3 || w.rule == 4, $"{good.id} at {id}: rule {w.rule}");
                     Assert.AreEqual(WordMark.Heard, w.mark);
+                    CollectionAssert.Contains(new[] { "well", "the usual", "poorly" }, w.Slot("pays"), "with no price of her own to set it against, the word every merchant would use");
+                    lines++;
+                    if (id == "lundenwic")
+                    {
+                        Assert.AreEqual("told.about", w.key);
+                        Assert.IsTrue(HasDigit(w.Slot("price")), $"{good.id}: a rough figure");
+                        StringAssert.Contains("paying about " + w.Slot("price"), w.Text, "never the figure without 'about'");
+                        continue;
+                    }
                     Assert.AreEqual("", w.Slot("price"), "no price is handed to the words");
-                    CollectionAssert.Contains(new[] { "well", "the usual", "poorly" }, w.Slot("pays"));
                     Assert.IsFalse(HasDigit(w.Text), $"{good.id} at {id}: {w.Text}");
                     foreach (var (name, value) in w.slots)
                         Assert.IsFalse(HasDigit(value), $"{good.id} at {id}: the slot {name} is '{value}'");
-                    lines++;
                 }
             }
             Assert.Greater(lines, 400, "every good of five markets");
-            // the three words, by what the market pays against the good's table value
+            // the switch (knowing_793.json word.told_figure): off, a first-hand telling is a word like all talk
+            Known.Rules.toldFigure = false;
+            foreach (var good in GameData.Goods)
+            {
+                if (!Trade.Ledger.Latest("lundenwic").prices.ContainsKey(good.id))
+                    continue;
+                var w = Known.Word(good, London, Ask());
+                Assert.AreEqual("told", w.key);
+                Assert.IsFalse(HasDigit(w.Text), w.Text);
+            }
+            Known.Rules.toldFigure = true;
+            // the three words of a merchant with nothing to compare, by what the market pays against the good's table value
             Assert.AreEqual("pays.well", Known.PaysKey(Wine.value * 1.2f, Wine, out int up));
             Assert.AreEqual("pays.fair", Known.PaysKey(Wine.value, Wine, out int level));
             Assert.AreEqual("pays.poorly", Known.PaysKey(Wine.value * 0.8f, Wine, out int down));
@@ -459,14 +478,21 @@
                 Assert.IsTrue(Wording.HasWord(key), $"wording_793.json has no word.{key}");
                 Assert.IsNotEmpty(Wording.WordText(null, key), key);
             }
-            // whatever the user writes there: only the line for a price seen may ask for the number
-            foreach (var key in Wording.WordKeys.Where(k => k != "seen"))
+            // whatever the user writes there: only the lines of a price she saw (seen, saw) and of a telling at first
+            // hand (told.about, and its cell in the ledger) may ask for the number, and a telling must say "about"
+            var figures = new[] { "seen", "saw", "told.about", "cell.about" };
+            foreach (var key in Wording.WordKeys.Where(k => !figures.Contains(k)))
             {
                 string line = Wording.WordText(null, key);
-                StringAssert.DoesNotContain("{price}", line, $"word.{key}: only word.seen may show a number");
+                StringAssert.DoesNotContain("{price}", line, $"word.{key}: only a price seen or told at first hand may show a number");
                 Assert.IsFalse(HasDigit(line), $"word.{key}: '{line}' has a number in it");
             }
-            StringAssert.Contains("{price}", Wording.WordText(null, "seen"));
+            foreach (var key in figures)
+                StringAssert.Contains("{price}", Wording.WordText(null, key), $"word.{key}");
+            foreach (var key in new[] { "saw", "told.about", "cell.about" })
+                StringAssert.Contains("about {price}", Wording.WordText(null, key), $"word.{key}: a figure that is not fresh from her own eyes says 'about'");
+            foreach (var key in new[] { "seen", "saw", "told.about" })
+                StringAssert.Contains("{Against}", Wording.WordText(null, key), $"word.{key}: the place for what it comes to against where she lies");
             // a people's own words win over the table's, and a key nothing has falls back to its parent
             var ripa = GameData.Port("ripa");
             StringAssert.DoesNotContain("toll man", Wording.WordText(ripa, "person.quay", 0f), "no toll is taken where silver goes by weight");
```

### 7.2 `Assets/Kapman/Tests/EditMode/WordAndLotsTests.cs` (new file; 13 tests)

| Test | It holds |
|---|---|
| `TraiectumPaysMoreThanHomeAndTheWordSaysSo` | the critic's case: 36 against 34 is "more than here", wedge up, never "poorly"; nine tenths is "less"; three in the hundred over is "much the same" |
| `AManWhoSawItSaysAboutAndAFigure` | a telling at first hand: "about", a rough figure, and a word against here; one mouth further: a word; the switch off: a word |
| `ARoughFigureCannotBeTakenForAPriceSeen` | the steps of rounding (1, 2, 5, 10), never nothing, the zone's own unit |
| `APriceSeenIsSetAgainstHereAsAFigure` | both seen: the difference as a figure; the port she lies at is not set against itself |
| `AtSeaTheWordIsSaidAgainstWhereSheLastSawOne` | "than at Dorestat"; lying where she has not looked, still the last she saw; ninety days and it is gone |
| `FarMoreMeansCarryingItTherePays` | the far step is the spread and the toll; and for eight prices "far more" is said exactly when buying here to sell there gains |
| `ACellAndItsLineAgree` | the three grades and nothing, each cell the same figure or word as its line, the wedge the cell's lean, "none" where none was on offer |
| `TheLedgerHasHerPortFirstAndWhatSheCarriesFirst` | the columns and rows; too old a word is left out; gold only among figures; the age classes |
| `AQuoteIsWhatTheDealComesToAndMovesNothing` | a quote changes neither market, purse nor hold, and the deal comes to the quote (buying and selling, toll and cost too) |
| `AllThatFitsStopsAtTheStockTheRoomAndTheSilver` | each of the three limits, with its reason; the counts of one, half, all |
| `ThePurseGivesChangeUnderACoinInCutSilver` | 5.2 d from ten deniers: six coins out, 1.3 g back; a small sum from the cut silver while there is some; 32.4 d taken is 32 deniers and 0.6 g; 32.76 is not 33; the lands of weighed silver as before |
| `TheCrewsPayNoLongerCostsAWholePennyACall` | 65 calls of half a penny cost between 32.5 and 37 d (they cost 56) |
| `OneLotIsNeverDearerThanTheSameUnitsOneByOne` | four at once against four singly, from two purses alike |

```csharp
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The word against where she lies, the ledger's cells, and dealing by the lot (Design/fixes/word_and_lots.md, after
    /// the strategy critic of 7 October). The word: said against the price she has in front of her, never against the
    /// good's table value while she has one; a figure for what she saw, "about" for what a man who saw it told her, a
    /// word for talk. The ledger: a cell and the line under a good never disagree. The lot: a quote is what the deal
    /// comes to and moves nothing; the purse settles once and no longer rounds to the whole coin.
    /// The lines are checked against the built-in words (the table is not loaded), as in KnownWordTests.
    /// Game day 0 is 21 June 793; day 40 is 31 July.
    /// </summary>
    public class WordAndLotsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const double Today = 40.0;

        static TextAsset Asset(string file) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + file);

        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            rules = TradeRules.Load(Asset("trade_rules.json"));
            Known.Load(Asset("knowing_793.json"));
            Chronicle.Load(Asset("chronicle_793.json"));
            Shocks.Load(Asset("shocks_793.json"));
            Seasons.Load(Asset("seasons_793.json"));
            Seasons.SetStart(793, 172);
            TradeYear.Load(File.ReadAllText(Data + "trade_year_793.json"));
            Wording.Load(null);                                              // the built-in words
            Markets.Clear();
            Known.Clear();
            Trade.Begin(TradeRules.Current, 900f, 0.0);                      // a new merchant: an empty ledger
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Known.Clear();
            Known.Load(Asset("knowing_793.json"));
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Wording.Load(Asset("wording_793.json"));
        }

        static PortDef Dorestad => GameData.Port("dorestad");
        static PortDef Traiectum => GameData.Port("traiectum");
        static PortDef London => GameData.Port("lundenwic");
        static PortDef Quentovic => GameData.Port("quentovic");
        static PortDef Ripa => GameData.Port("ripa");
        static GoodDef Wine => GameData.Good("rhenish_wine");

        static WordAsk Ask(double today = Today, PortDef here = null) => new WordAsk { today = today, here = here };

        static Market MarketOf(PortDef port, double day) => new Market(port, TradeRules.Current, 793, day);

        /// <summary>The merchant sees a market's prices on a day.</summary>
        static void See(PortDef port, double day) => Trade.Ledger.Note(MarketOf(port, day), day, 7);

        /// <summary>What she noted that a market paid for a good (its newest note).</summary>
        static float Noted(PortDef port, GoodDef good) => Trade.Ledger.Latest(port.id).prices[good.id].pays;

        /// <summary>The merchant is told what one market paid for one good on a day: a price of the test's own choosing.</summary>
        static void TellPays(PortDef port, GoodDef good, float pays, double day, How how, int hops, string teller = null, string tellerOf = null)
        {
            int n = GameData.Goods.Count;
            var snap = new PriceSnapshot { port = port.index, day = day, asks = new float[n], pays = new float[n] };
            for (int i = 0; i < n; i++)
                snap.asks[i] = snap.pays[i] = float.NaN;
            snap.pays[good.index] = pays;
            Trade.Ledger.Hear(snap, "a label that is never shown", how, hops, day, teller, tellerOf);
        }

        static bool HasDigit(string text) => Regex.IsMatch(text ?? "", @"\d");

        // ------------------------------------------------------------------ the word, against where she lies

        [Test]
        public void TraiectumPaysMoreThanHomeAndTheWordSaysSo()
        {
            // the critic's case: home pays 34 d, Traiectum 36 d, and the word said "Traiectum was paying poorly"
            See(Dorestad, Today);
            float home = Noted(Dorestad, Wine), there = home * 36f / 34f;
            TellPays(Traiectum, Wine, there, Today - 20.0, How.Told, 0);     // (told long enough ago to be talk: a word, and no error in it)
            var w = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual(4, w.rule);
            Assert.AreEqual("more than here", w.Slot("pays"));
            Assert.AreEqual(1, w.tilt, "the wedge points up: they pay more there than where she lies");
            Assert.AreEqual("They say Traiectum pays more than here for Rhenish wine. A fortnight ago.", w.Text);
            StringAssert.DoesNotContain("poorly", w.Text);
            Assert.IsFalse(HasDigit(w.Text), "talk is a word, never a figure: " + w.Text);
            // and the other way round: a port that pays less than here is said to pay less, whatever the good's table value
            TellPays(Quentovic, Wine, home * 0.9f, Today - 20.0, How.Told, 0);
            var less = Known.Word(Wine, Quentovic, Ask(Today, Dorestad));
            Assert.AreEqual("less than here", less.Slot("pays"));
            Assert.AreEqual(-1, less.tilt);
            // a price within a twentieth of the one here is much the same
            TellPays(London, Wine, home * 1.03f, Today - 20.0, How.Told, 0);
            var same = Known.Word(Wine, London, Ask(Today, Dorestad));
            Assert.AreEqual("much the same as here", same.Slot("pays"));
            Assert.AreEqual(0, same.tilt);
        }

        [Test]
        public void AManWhoSawItSaysAboutAndAFigure()
        {
            See(Dorestad, Today);
            float home = Noted(Dorestad, Wine), there = home * 1.5f;
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Told, 0, "skipper", "dorestad");
            var zone = TradeRules.Current.ZoneOf(Dorestad.region);
            string about = Known.Rough(there, zone);
            var w = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual(3, w.rule);
            Assert.AreEqual("told.about", w.key);
            Assert.AreEqual(WordMark.Heard, w.mark);
            Assert.AreEqual(about, w.Slot("price"));
            Assert.AreEqual($"A Frankish skipper said Traiectum was paying about {about} for Rhenish wine, yesterday. That is far more than here.", w.Text);
            Assert.AreEqual(1, w.tilt);
            // the same telling through one more mouth is a word, and the same word
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            See(Dorestad, Today);
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Talk, 1, "quay", "dorestad");
            var talk = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual("told", talk.key);
            Assert.AreEqual("far more than here", talk.Slot("pays"));
            Assert.IsFalse(HasDigit(talk.Text), talk.Text);
            // with the switch off (knowing_793.json word.told_figure) a telling at first hand is a word too: the rule of 4 October
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            See(Dorestad, Today);
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Told, 0, "skipper", "dorestad");
            Known.Rules.toldFigure = false;
            var word = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual("told", word.key);
            Assert.IsFalse(HasDigit(word.Text), word.Text);
        }

        [Test]
        public void ARoughFigureCannotBeTakenForAPriceSeen()
        {
            var d = TradeRules.Current.ZoneOf(Dorestad.region);
            Assert.AreEqual("4 d", Known.Rough(4.3, d));
            Assert.AreEqual("19 d", Known.Rough(19.4, d));
            Assert.AreEqual("36 d", Known.Rough(36.4, d), "under fifty: to the nearest two");
            Assert.AreEqual("38 d", Known.Rough(37.0, d));
            Assert.AreEqual("60 d", Known.Rough(59.0, d), "under two hundred: to the nearest five");
            Assert.AreEqual("480 d", Known.Rough(476.0, d), "then to the nearest ten");
            Assert.AreEqual("1 d", Known.Rough(0.2, d), "never nothing");
            StringAssert.EndsWith(" p", Known.Rough(59.0, TradeRules.Current.ZoneOf(London.region)));
        }

        [Test]
        public void APriceSeenIsSetAgainstHereAsAFigure()
        {
            See(Dorestad, Today);
            See(Traiectum, Today - 1.0);
            float home = Noted(Dorestad, Wine), there = Noted(Traiectum, Wine);
            var ask = Ask(Today, Dorestad);
            var cell = Known.Cell(Wine, Traiectum, ask);
            Assert.AreEqual(PriceGrade.Exact, cell.grade);
            Assert.IsTrue(cell.compared);
            var w = Known.Word(Wine, Traiectum, ask);
            Assert.AreEqual(2, w.rule);
            Assert.AreEqual("seen", w.key);
            Assert.AreEqual(System.Math.Sign(cell.paysLean), w.tilt);
            var zone = TradeRules.Current.ZoneOf(Dorestad.region);
            string start = $"You saw Rhenish wine pay {zone.Show(there)} at Traiectum, yesterday. That is ";
            StringAssert.StartsWith(start, w.Text);
            double by = there - home;
            if (System.Math.Abs(by) >= 0.05)
                Assert.AreEqual(start + zone.Show(System.Math.Abs(by)) + (by > 0.0 ? " more than here." : " less than here."), w.Text);
            else
                Assert.AreEqual(start + "much the same as here.", w.Text);
            // the port she lies at is not set against itself
            var here = Known.Word(Wine, Dorestad, ask);
            Assert.AreEqual($"You saw Rhenish wine pay {zone.Show(home)} at Dorestat, this morning.", here.Text);
            Assert.AreEqual(0, here.tilt);
        }

        [Test]
        public void AtSeaTheWordIsSaidAgainstWhereSheLastSawOne()
        {
            See(Traiectum, Today - 9.0);
            See(Dorestad, Today - 2.0);
            var here = Known.Here(Wine, Ask());
            Assert.IsTrue(here.Known);
            Assert.IsFalse(here.lying);
            Assert.AreEqual("dorestad", here.port.id, "the newest of her own sights");
            Assert.AreEqual(Noted(Dorestad, Wine), here.pays);
            TellPays(London, Wine, here.pays * 1.6f, Today - 20.0, How.Told, 0);
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual("They say Lundenwic pays far more than at Dorestat for Rhenish wine. A fortnight ago.", w.Text);
            // lying at a port whose market she has seen today, that port is "here"
            See(Traiectum, Today);
            var lying = Known.Here(Wine, Ask(Today, Traiectum));
            Assert.IsTrue(lying.lying);
            Assert.AreEqual("traiectum", lying.port.id);
            // lying where she has never looked at the market, it is still the last one she saw
            Assert.AreEqual("traiectum", Known.Here(Wine, Ask(Today, Quentovic)).port.id);
            Assert.IsFalse(Known.Here(Wine, Ask(Today, Quentovic)).lying);
            // a sight ninety days old is no longer a price to set anything against
            Assert.IsFalse(Known.Here(Wine, Ask(Today + 95.0)).Known);
        }

        [Test]
        public void FarMoreMeansCarryingItTherePays()
        {
            float half = rules.spread * 0.5f, gap = (1f + half) / (1f - half);
            float tollD = rules.ZoneOf(Dorestad.region).toll, tollR = rules.ZoneOf(Ripa.region).toll;
            Assert.AreEqual(gap / (1f - tollD) - 1f, Known.FarOver(Dorestad), 1e-5f);
            Assert.AreEqual(gap / (1f - tollR) - 1f, Known.FarOver(Ripa), 1e-5f);
            Assert.Greater(Known.FarOver(Dorestad), Known.FarOver(Ripa), "a toll where it is sold asks a wider gap");
            float far = Known.FarOver(Dorestad);
            Assert.AreEqual(2, Known.Lean(100f * (1f + far) + 0.5f, 100f, far, far));
            Assert.AreEqual(1, Known.Lean(100f * (1f + far) - 0.5f, 100f, far, far));
            Assert.AreEqual(1, Known.Lean(106f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(104f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(96f, 100f, far, far));
            Assert.AreEqual(-1, Known.Lean(95f, 100f, far, far));
            Assert.AreEqual(-2, Known.Lean(100f / (1f + far) - 0.5f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(float.NaN, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(50f, 0f, far, far));
            // what the word means: bought here at what they ask, sold there at what they pay less the toll
            foreach (float paysThere in new[] { 60f, 100f, 120f, 130f, 135f, 136f, 140f, 200f })
            {
                const float paysHere = 100f;
                float asksHere = paysHere * gap, inHand = paysThere * (1f - tollD);
                Assert.AreEqual(inHand > asksHere, Known.Lean(paysThere, paysHere, far, far) == 2, $"they pay {paysThere} there");
            }
        }

        // ------------------------------------------------------------------ the ledger

        [Test]
        public void ACellAndItsLineAgree()
        {
            See(London, Today);                                              // where she lies
            See(Quentovic, Today - 2.0);                                     // her own eyes this week: the figure
            See(Dorestad, Today - 10.0);                                     // her own eyes, longer ago: about
            TellPays(Traiectum, Wine, 40f, Today - 3.0, How.Told, 0, "skipper", "dorestad");   // a man who saw it: about
            TellPays(Ripa, Wine, 90f, Today - 3.0, How.Talk, 3, "quay", "dorestad");            // talk: a word
            var ask = Known.AskFor(Wine, Ask(Today, London));
            var grades = new Dictionary<string, PriceGrade>
            {
                ["quentovic"] = PriceGrade.Exact, ["dorestad"] = PriceGrade.About, ["traiectum"] = PriceGrade.About,
                ["ripa"] = PriceGrade.Word, ["birka"] = PriceGrade.None, ["lundenwic"] = PriceGrade.Exact,
            };
            foreach (var kv in grades)
            {
                var port = GameData.Port(kv.Key);
                var cell = Known.Cell(Wine, port, ask);
                string text = Known.CellText(cell, port, ask, false);
                var line = Known.Word(Wine, port, ask);
                Assert.AreEqual(kv.Value, cell.grade, kv.Key);
                Assert.AreEqual(kv.Key != "lundenwic" && kv.Value != PriceGrade.None, cell.compared, kv.Key + ": set against where she lies, but for that port itself");
                switch (kv.Value)
                {
                    case PriceGrade.Exact:
                        Assert.AreEqual(line.Slot("price"), text, kv.Key + ": the cell is the line's figure");
                        Assert.IsTrue(HasDigit(text));
                        StringAssert.EndsWith(" p", text, "in the reckoning of where she lies");
                        break;
                    case PriceGrade.About:
                        Assert.AreEqual("about " + line.Slot("price"), text, kv.Key + ": the cell is the line's rough figure");
                        StringAssert.Contains("about " + line.Slot("price"), line.Text);
                        break;
                    case PriceGrade.Word:
                        Assert.IsFalse(HasDigit(text), text);
                        Assert.IsFalse(HasDigit(line.Text), line.Text);
                        StringAssert.Contains(text, line.Slot("pays"), "'far more' in the cell, 'far more than here' in the line");
                        break;
                    default:
                        Assert.AreEqual("", text);
                        Assert.GreaterOrEqual(line.rule, 5, "nothing noted: the line is the season, the ships ahead, the law, or no word");
                        break;
                }
                if (cell.compared)
                    Assert.AreEqual(System.Math.Sign(cell.paysLean), line.tilt, kv.Key + ": the wedge is the cell's lean");
            }
            // what they ask: where none was on offer the cell says so
            var none = Known.Cell(Wine, GameData.Port("traiectum"), ask);
            Assert.IsFalse(none.onOffer, "he told what they paid, and nothing was on offer");
            Assert.AreEqual("none", Known.CellText(none, GameData.Port("traiectum"), ask, true));
        }

        [Test]
        public void TheLedgerHasHerPortFirstAndWhatSheCarriesFirst()
        {
            See(Quentovic, Today - 5.0);
            See(Dorestad, Today - 3.0);
            See(London, Today);
            TellPays(Ripa, Wine, 90f, Today - 95.0, How.Talk, 3);            // too old to count
            var ask = Ask(Today, London);
            var ports = WordPanel.Ports(ask, out int more);
            Assert.AreEqual(0, more);
            Assert.AreEqual("lundenwic", ports[0].id, "where she lies stands first");
            CollectionAssert.AreEquivalent(new[] { "lundenwic", "dorestad", "quentovic" }, ports.Select(p => p.id).ToList());
            Assert.Less(ports[1].index, ports[2].index, "the rest as the ports table has them, so a column keeps its place");
            Trade.Hold.Add(Wine, 4, 0.0, Today);
            var goods = WordPanel.Goods(ask, ports, false);
            Assert.AreEqual("rhenish_wine", goods[0].id, "what she carries first");
            var onOffer = Trade.Ledger.Latest("lundenwic").prices.Where(kv => kv.Value.onOffer).Select(kv => kv.Key).ToList();
            CollectionAssert.IsSubsetOf(onOffer, goods.Select(g => g.id).ToList(), "then what is on offer where she lies");
            Assert.GreaterOrEqual(WordPanel.Goods(ask, ports, true).Count, goods.Count, "every good on offer anywhere is no fewer");
            // the best of a row is marked only among figures
            var cells = new List<PriceCell>
            {
                new PriceCell { grade = PriceGrade.Exact, pays = 30f, asks = 40f, onOffer = true },
                new PriceCell { grade = PriceGrade.Word, pays = 99f, asks = 10f, onOffer = true },
                new PriceCell { grade = PriceGrade.About, pays = 50f, asks = float.NaN, onOffer = false },
                new PriceCell { grade = PriceGrade.About, pays = 45f, asks = 35f, onOffer = true },
            };
            Assert.AreEqual(2, WordPanel.Best(cells, false), "where they pay most, among the figures");
            Assert.AreEqual(3, WordPanel.Best(cells, true), "where it is on offer cheapest, among the figures");
            Assert.AreEqual(-1, WordPanel.Best(cells.Take(2).ToList(), false), "one figure is nothing to choose between");
            Assert.AreEqual("age-new", WordPanel.AgeClass("age.yesterday"));
            Assert.AreEqual("age-week", WordPanel.AgeClass("age.last_week"));
            Assert.AreEqual("age-month", WordPanel.AgeClass("age.last_month"));
            Assert.AreEqual("age-old", WordPanel.AgeClass("age.old_talk"));
        }

        // ------------------------------------------------------------------ by the lot: one deal, one settlement

        [Test]
        public void AQuoteIsWhatTheDealComesToAndMovesNothing()
        {
            var market = MarketOf(Dorestad, 0);
            var g = market.Goods.First(x => x.OnOffer && x.good.value < 10f && x.stock >= 6f);
            string id = g.good.id;
            float stock = g.stock, glut = g.glut;
            double worth = Trade.Purse.Worth(market.zone, rules);
            var quote = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.IsTrue(quote.ok);
            Assert.AreEqual(3, quote.count);
            Assert.AreEqual(stock, g.stock, "a quote takes nothing from the market");
            Assert.AreEqual(worth, Trade.Purse.Worth(market.zone, rules), 1e-9, "nor from the purse");
            Assert.AreEqual(0, Trade.Hold.Count(id));
            var bought = Deal.Buy(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.AreEqual(quote.count, bought.count);
            Assert.AreEqual(quote.deniers, bought.deniers, 1e-9, "the lot costs what it was quoted at");
            Assert.AreEqual(worth - bought.deniers, Trade.Purse.Worth(market.zone, rules), 0.25, "one settlement: the purse is down by the sum, to within the scales and the mint's tenth on the change");
            // and the sale
            stock = g.stock;
            glut = g.glut;
            var sellQuote = Deal.QuoteSell(market, Trade.Hold, id, 3, 6, 0, "player");
            Assert.IsTrue(sellQuote.ok);
            Assert.AreEqual(stock, g.stock);
            Assert.AreEqual(glut, g.glut);
            Assert.AreEqual(3, Trade.Hold.Count(id));
            Assert.AreEqual(bought.deniers, sellQuote.cost, 1e-6, "what they cost her, with nothing taken out of the hold");
            var sold = Deal.Sell(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.AreEqual(sellQuote.count, sold.count);
            Assert.AreEqual(sellQuote.deniers, sold.deniers, 1e-9);
            Assert.AreEqual(sellQuote.toll, sold.toll, 1e-9);
            Assert.AreEqual(sellQuote.cost, sold.cost, 1e-6);
        }

        [Test]
        public void AllThatFitsStopsAtTheStockTheRoomAndTheSilver()
        {
            var market = MarketOf(Dorestad, 0);
            var g = market.Goods.First(x => x.OnOffer && x.good.value >= 2f && x.good.value < 10f && x.stock >= 6f);
            string id = g.good.id;
            // plenty of silver and room: all that is on offer
            Trade.Purse.Add("denier", 100000);
            Trade.Hold.capacity = 1e7f;
            var all = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(Mathf.FloorToInt(g.stock), all.count);
            Assert.AreEqual("none_left", all.why);
            // room for two only
            Trade.Hold.capacity = g.good.Space * 2.5f;
            var room = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(2, room.count);
            Assert.AreEqual("no_room", room.why);
            // silver for the first two only
            Trade.Hold.capacity = 1e7f;
            double two = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, 2, 6, 0, "player").deniers;
            Trade.Purse.Clear();
            Trade.Purse.Add("denier", System.Math.Ceiling(two));
            var silver = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(2, silver.count);
            Assert.AreEqual("no_silver", silver.why);
            // the lots of the buttons
            Assert.AreEqual(0, FrameDeal.CountOf(LotSize.One, 0));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.One, 5));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.Half, 1));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.Half, 2));
            Assert.AreEqual(2, FrameDeal.CountOf(LotSize.Half, 3));
            Assert.AreEqual(2, FrameDeal.CountOf(LotSize.Half, 4));
            Assert.AreEqual(3, FrameDeal.CountOf(LotSize.Half, 5));
            Assert.AreEqual(5, FrameDeal.CountOf(LotSize.All, 5));
        }

        [Test]
        public void ThePurseGivesChangeUnderACoinInCutSilver()
        {
            var frankish = rules.ZoneOf("rhine_delta");
            double perGram = rules.Coin("hacksilver").Deniers;
            var p = new Purse();
            p.Add("denier", 10);
            Assert.IsTrue(p.Pay(5.2, frankish, rules, out string how));
            Assert.AreEqual(4.0, p.Count("denier"), 1e-9, "six whole coins are handed over");
            Assert.AreEqual(1.3, p.Count("hacksilver"), 1e-9, "and the 0.8 d over comes back weighed out, never more than is due");
            Assert.AreEqual("1.3 g of hacksilver", p.LastBack);
            StringAssert.Contains("back", how);
            Assert.AreEqual(10.0 - 5.2, p.Silver(rules), 0.06, "by weight she is down by the sum, to a tenth of a gram");
            Assert.LessOrEqual(p.Silver(rules), 10.0 - 5.2 + 1e-9, "the market never gives over");
            // a sum under one coin is made up from the cut silver while there is enough of it: no coin is broken
            Assert.IsTrue(p.Pay(0.5, frankish, rules, out _));
            Assert.AreEqual(4.0, p.Count("denier"), 1e-9);
            Assert.Less(p.Count("hacksilver"), 1.3);
            // a whole sum takes whole coins and gives nothing back
            var q = new Purse();
            q.Add("denier", 10);
            Assert.IsTrue(q.Pay(8, frankish, rules, out _));
            Assert.AreEqual(2.0, q.Count("denier"), 1e-9);
            Assert.AreEqual(0.0, q.Count("hacksilver"), 1e-9);
            Assert.AreEqual("", q.LastBack);
            // what is taken in a sale: whole coins and the rest weighed out, not the nearest coin
            var r = new Purse();
            r.Receive(32.4, frankish, rules, out string took);
            Assert.AreEqual(32.0, r.Count("denier"), 1e-9);
            Assert.AreEqual(0.6, r.Count("hacksilver"), 1e-9);
            Assert.AreEqual("took 32 deniers and 0.6 g of hacksilver", took);
            Assert.AreEqual(32.4, r.Silver(rules), perGram * 0.1 + 1e-9);
            var s = new Purse();
            s.Receive(32.76, frankish, rules, out _);
            Assert.AreEqual(32.0, s.Count("denier"), 1e-9, "32.76 is no longer 33");
            Assert.AreEqual(1.2, s.Count("hacksilver"), 1e-9);
            // where silver goes by weight nothing has changed: all of it on the scales
            var north = rules.ZoneOf("jutland");
            var w = new Purse();
            w.Receive(10.0, north, rules, out _);
            Assert.AreEqual(0.0, w.Count("denier"));
            Assert.AreEqual(10.0, w.Worth(north, rules), 0.05);
        }

        [Test]
        public void TheCrewsPayNoLongerCostsAWholePennyACall()
        {
            // sixty-five calls in a season, half a penny owed at each (the critic's sum): 32.5 d in all
            var frankish = rules.ZoneOf("rhine_delta");
            var p = new Purse();
            foreach (var kv in rules.startCoins)
                p.Add(kv.Key, kv.Value);
            double before = p.Worth(frankish, rules);
            for (int i = 0; i < 65; i++)
                Assert.IsTrue(p.Pay(0.5, frankish, rules, out _));
            double cost = before - p.Worth(frankish, rules);
            Assert.GreaterOrEqual(cost, 32.5 - 1e-6, "never less than was owed");
            Assert.Less(cost, 37.0, "and not the 56 d it cost when every call took a whole coin (purse_sim.py: 35.35 d)");
        }

        [Test]
        public void OneLotIsNeverDearerThanTheSameUnitsOneByOne()
        {
            // two merchants alike, two markets alike: one buys four at once, the other one at a time
            double Spend(bool byTheLot)
            {
                var market = MarketOf(Dorestad, 0);
                var g = market.Goods.First(x => x.OnOffer && x.good.value < 10f && x.stock >= 6f);
                var purse = new Purse();
                purse.Add("denier", 200);
                var hold = new Hold { capacity = 1e6f };
                double before = purse.Worth(market.zone, rules);
                if (byTheLot)
                    Assert.AreEqual(4, Deal.Buy(market, purse, hold, g.good.id, 4, 6, 0, "ship").count);
                else
                    for (int i = 0; i < 4; i++)
                        Assert.AreEqual(1, Deal.Buy(market, purse, hold, g.good.id, 1, 6, 0, "ship").count);
                return before - purse.Worth(market.zone, rules);
            }
            double lot = Spend(true), singles = Spend(false);
            Assert.LessOrEqual(lot, singles + 0.06, "one settlement is not the dearer (to a tenth of a gram on the scales)");
            Assert.Less(singles - lot, 0.5, "and a unit at a time no longer costs a penny a click more (it was up to 4 d on four units)");
        }
    }
}
```

### 7.3 `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs` (one check to add; it deals nothing, so the rest of the walk is as it was)

After the line `yield return play.Shot("16_bought_at_dorestat");` (line 466 when I first read it, 475 half an hour
later: find it by its text):

```csharp
                // ---- BY THE LOT: beside the wine stand the lots she could sell, each with its count and its sum on it
                // (nothing is dealt here: the walk goes on with the hold as it is)
                int casks = Trade.Hold.Count("rhenish_wine");
                var lotQuote = FrameDeal.Quote(FrameDeal.MarketHere(), "rhenish_wine", false, LotSize.All);
                string allText = $"all {casks} for {FrameDeal.MarketHere().zone.Show(lotQuote.deniers)}";
                var lots = play.Line(wine, Window(play, "Trade")).Query<Button>().ToList().Select(b => b.text).ToList();
                if (casks > 1)
                    CollectionAssert.Contains(lots, allText, "no button sells all the wine in one deal: " + string.Join(" | ", lots));
                Assert.AreEqual(1, lots.Count(t => t == "Sell"), "one button, and only one, sells a single cask");
                Assert.IsNotNull(Known.Here(GameData.Good("rhenish_wine"), Known.AskNow()).port, "she lies at a market and the word has nothing to be said against");
```

It compiled in a copy of the play test with these lines in that place (the file already has
`using Kapman.Game.Frame;` and `System.Linq`). Look at capture 16 after the run: the wine's row should read as in
5.2.

## 8. What changes for the player, so nothing is lost by surprise

- "Pays well", "the usual" and "poorly" are gone from play. Every price word is now against where she lies. The
  three old words stay in the tables for the bare case of 2.1 (3).
- A skipper who saw a market now gives her a rough figure. That is the one change to the rule "a number only for
  what she saw herself"; one switch puts the rule back (section 0).
- Her own sight of a price more than a week old now keeps a rough figure ("about 60 d ... It may have changed.")
  where it fell back to a word. She did see it; the figure is rough and dated.
- A figure is in the reckoning of where she lies, not of the port it is about.
- The wedge beside a line means "they pay more there than here" (down: less). It meant "over 1.15 times the
  table".
- The purse gathers a small heap of cut silver from change and takings. In the king's lands it buys a tenth less
  than its weight until she spends it where silver is weighed, or changes it at a moneyer's.
- The top bar's silver is what the purse buys where she lies (6.10), so it steps when she moors.
- The Word window no longer lists markets freshest first with lines under each: it is the ledger. What the old
  list said of each market (how the word came, how old) is in each port's head.
- Captures 01, 03, 05, 08, 09, 14 will differ: the wine's line, the buttons, and the sale's words. Capture 09's
  "for 36 d, less 3.6 d toll: got 33 deniers" becomes "got 32 deniers and" 0.6 to 1.2 "g of hacksilver" (the price
  shown as 36 d lay somewhere from 36 to 36.5).

## 9. What I could not check, and what may bite

1. **No test was run.** The expected values were worked by hand. The likeliest to need a touch: any that leans on
   a real market's price being over or under another's (`APriceSeenIsSetAgainstHereAsAFigure` reads both prices
   back and takes either sign, on purpose).
2. **Nothing was seen on a screen.** The stylesheet is a draft. Three things to look at first: a `Button` holding two
   labels at a fixed height (the ledger's cell); a sideways `ScrollView` inside the float's own up-and-down one;
   the two button rows at the right end of `frame-ln` in a narrow tiled window (they may want to wrap under the
   quote there; in the 520 px float of brief_faces.md they have room).
3. **The same seed will play a little differently once**, because the other ships' purses no longer round to the
   whole coin (they are the same `Purse`). Tests that compare two runs of one seed are untouched
   (`TheSameSeedPlaysTheSame`, the catch-up split, the save round trips). A test that pins a figure of a seeded run
   would need its figure taken again; I searched the EditMode and PlayMode tests for pinned purse figures and found
   none tighter than a penny that a whole-coin payment would not still meet (`EncounterRunnerTests` line 275 pays
   12 d in whole deniers: unchanged). I did not read `EconomyTests` or `SimTests` line by line.
4. **The money fix is not in yet.** Nothing here rests on its numbers; the far step reads the toll from the table
   as it stands on the day.
5. **`FrameRoot.cs` is being worked on.** Its two places are given by their text.
6. **`Design/fixes/brief_faces.md` is a draft.** 6.12 fits its section 3 as written. If the builder names the
   members otherwise, rename the two overrides.
7. **The standing's favour.** The Trade row shows what they pay HER (`Deal.PaysFor`: a tenth more at a standing of
   +100, `reputation_793.json` `price_at_100`); the price notes keep the market's own price (`Market.Pays`). So where
   she is "known (+25)" the row is 2.5 in the hundred over the ledger's "here" cell: under a penny on a 34 d cask,
   3 d on a 120 d one. It is so today, before this mend. If it grates: note prices through `Deal.PaysFor` and
   `Deal.AsksFor` for the player in `Ledger.Note`. Not done: it changes what the notes mean in a save.
8. **The older market panel** (`Game/UI/MarketPanel.cs`, the 3D levels' window) still deals in its own way and
   notes prices itself (lines 147 and 164). It was not touched. It shows the same new word lines.
9. **The captures** are named above from the critics' pages; I did not open the pictures.

## 10. Not done here, and where it would go

- **The form letter, and the teller's name.** The newcomer (con 15) and the words critic (con 11) both fault three
  word lines in a row from one teller with one word changed ("say it once: Traiectum pays poorly for wine and
  querns, well for bread"). That is `Design/fixes/one_cast.md`, written the same hour by another hand; I found it
  when this was done and read only the parts that touch the same code. **The two must be brought in together with
  one eye on the words:**
  - They compile side by side as far as I can see (I did not compile the two together). Its one edit to
    `Known/Word.cs` is in `Person` (line 270); mine are above line 248 and from line 393 down. Its three edits to
    `Known/Known.cs` are at lines 468, 473 and 538; mine at 81 and 121. No hunk is shared, though after the first is
    in the second will apply with an offset. It adds an eighth, optional parameter to `Ledger.Hear`; my tests call it
    with the first seven, by place. It still calls `Known.PaysKey`, which stays.
  - It too notes the market when she moors (`ProveTold`, called from `FrameRoot.Workspace`, ends with
    `Trade.Ledger.Note(market, ...)`). Let that last line be `FrameDeal.See(true);` so the market is seen in one
    place. The order is right as the two stand: `Workspace()` runs in `Update` before the quarter-second block where
    `FrameDeal.See()` is, so its judging of what she was told comes before the market is noted as seen.
  - But its part 6 begins "A told price is never a number: the player was told 'well', 'the usual' or 'poorly'",
    and it speaks those OLD words in three places: `JudgeTold` (was the teller's word true: "pays as it was said to
    (well, the usual, poorly)"), `Say` ("said Lundenwic paid well ... It pays the usual.") and `AskPrices`
    ("Lundenwic, last week: well for Rhenish wine and wax, poorly for quernstones."). With this mend in, those three
    would bring "well" and "poorly" by the hidden table back onto the screen. For the two lines, take the word
    from the cell: `var c = Known.Cell(good, port, Known.AskNow());` then `Known.CellText(c, port,
    Known.AskFor(good, Known.AskNow()), false)` gives "more", "far less", "about 60 d" by the same rules as every
    other line ("Lundenwic, last week: more than here for Rhenish wine and wax, less for quernstones."). For the
    verdict, hold the price found against the price told, not each against the table: the word held when
    `Known.Lean(found, told, far, far) == 0` with `far = Known.FarOver(port)`, that is, when the market pays within
    a twentieth of what he said.
  - The merged line it writes is the mend the two critics asked for; the cells here make the gathering easy (lines
    whose cells rest on the same `PriceNote` share a teller and an age, and can be grouped by `paysLean`).
- **The coins in the purse** (the critic's con 7, second half: "a hover lists the four heaps and what each loses").
  The bar's figure is mended (6.10); the list is not. The purse line of the Trade window is the place for it.
- **Thinning a stay's price notes** (1.4, point 6).
- **Keys for the lots** (3.1).
- **A press on a port's head** in the ledger could pick that port in the Voyage window. Not written.

## 11. What this answers, by the critic's own reckoning

| His point | His weight | What is here |
|---|---|---|
| Con 4, "pays poorly" against a hidden table | a point | the whole of his mend ("say it against where she lies ... 'about 36 d' with its age"), with the wide step given a meaning |
| Con 3, every click rounded against you | a point once the margins are thin | his mend in full: a count and one settling; change under a penny in cut silver; and the crew's pay with it |
| Con 15, no ledger, windows too narrow | half a point | the ledger as he drew it (goods down, known ports across, a number where seen, a word where told, shaded by age); one line a good in Trade is not claimed (a row is a line and the word under it) |
| Con 7, the purse shows the wrong figure | a scratch | the bar and the Trade window show what it buys here; the list of coins is not here |
| Con 6, numbers on one screen disagree | a scratch each | the purse and the market in one unit; the ledger in one reckoning |
| His fourth change | +0.25 of the 2.5 he offers | three of its four parts (the fourth is the measure's) |
| "The first trade ... the word on the item told him the wrong thing" (his verdict on today's build) | part of the 3 | mended |
