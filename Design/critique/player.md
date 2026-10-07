# Kapman as a game to play: the player's critique

7 October 2026. One of two critiques. This one judges Kapman as something to buy and play.

**How I judged.** I read the design pages, the code under `Assets/Kapman/Code`, the play test, the cards, five chains
end to end, five place files and the story tables. I looked at the three captures in `Captures/chart` and rendered
layout G of `Design/ui_mock/template3.html`. I did not run the game. Where I give silver or minutes, the sum is mine,
from the tables, and is marked. Nothing but this file was changed.

## Verdict

Not yet a game: today it is a handsome chart on which a dot crosses to the next port while nothing asks anything of
you, with a market at each end. The best thing in the project, the chains, cannot start in the scene that is the game,
and the trade under them is solved before the first cast off. The one change that would matter most is to make a leg
worth playing: a crossing of a minute or two, not ten to forty, that carries two or three cards which cost days.

## The ten worst problems, most harmful first

### 1. Sailing is waiting, and it is three quarters of the game

Evidence:

- `ChartBody.cs` moves her at real knots on a clock of one game minute a second. `GameSpeed.Speeds` is 1, 2, 4.
- So a day under sail is 24 minutes at 1x and 6 minutes at 4x. By the mock's own counts (`template3.html`, DEST)
  Lundenwic is "2 to 4 days": 12 to 24 minutes at 4x. Sliesthorp is "5 to 7 days": 30 to 42 minutes.
- `depth_design.md` 2.6 plans a year of 3 to 4 hours with "about thirty days under sail (6 minutes a day at 4x)".
  That is three hours of the dot in a year of three to four.
- Capture `02_under_way.png`: her mark is a white dot six pixels wide. No wind is shown, no bar of the way, nothing
  to click. You do not steer (concept 4.7).
- Sunless Sea and Dredge give you the helm. 80 Days and FTL make the crossing short. Kapman does neither.

Fix. I know you ruled out time warps. Then the choice is to fill the leg or to shorten the sitting. Filling a London
crossing at 4x needs a decision every minute or two, eight cards or more, and that wears out any deck. So shorten the
sitting. When no card is pending she runs at the rate the game already uses to wait for a tide (`voyage_rules.json`,
`waiting_rate` 3600: an hour a second) and stops dead for a card. The dates, the distances and the clock's rule stay
as they are; only the empty hours pass quickly. London is then one to two minutes and its cards. Keep 1x to 4x for
watching her in the 3D window. Put it in `ChartGameTests`: a leg is one to three minutes of real time.

### 2. Nothing is at stake and nothing ends

Evidence:

- The debt has no teeth. `MapTable.cs` line 179 prints "days overdue" and marks the label. No other code reads
  `Trade.DebtDue`.
- The chart game has no winter, no year's end, no heir, no wreck and no ruin. `story/winter_793.json` (165 KB) is
  read by no code. "Through your heirs to 1066" lives only in the design pages.
- There are two debts. The bar says "Owed: 480 d, due in 365 days" (`trade_rules.json`, `due_days`). The chain
  `house_debt` says Michaelmas, 100 days off, and keeps the sum in a flag (`ch_house_debt_owed`). `Effects.cs` has no
  verb for the debt. Pay the count his 480 d on his card and the bar still says 480 d owed.
- The debt is easy. By the tables wine is about 47 d at Dorestat and pays about 86 d at Lundenwic after the toll:
  about 150 d a crossing with four casks. The debt is gone in three or four crossings. (My sum from `goods_793.json`
  and `trade_rules.json`. Not a run.)

Fix. One debt, and the chain owns it: add a `debt` verb, the day is Michaelmas 793, the bar shows that. Say yes to the
earnest of 40 d on St John's day (`depth_design.md`, question 3a): it puts a deadline inside the first hour. Build
the smallest winter there can be: one board at Martinmas with the year's log lines, the debt paid or not, and one
choice. Then tune the debt or the wine until Michaelmas is in doubt for a first player in a four-cask boat.

### 3. A card cannot cost a day, and most cost nothing you can see

Evidence:

- `Effects.cs` has no verb for hours or days, and none for the hull or the sail. Days are "the one resource"
  (concept 4.2) and no card can touch them.
- The 63 cards of `sea_793.json` hold 176 morale effects, 91 flags, 53 feelings, 51 health, 21 goods, 6 silver and
  2 men. The usual one is `morale +0.02`. The player is shown "The crew's heart rises."
- The safe way is free. `sea.river.tow`: "Anchor and wait for a wind (sure)" has no effect at all.
  `sea.sick.fever`: "Put in to the nearest landing and find him a roof (sure)" costs 0.01 of heart.
  `sea.swell.long`: "Hold on under full sail while the wind serves (doubtful)" wins 0.03 of heart and no time.
  After ten cards a player picks the first "(sure)" without reading.
- The most silver on any sea card is 20 d. A cask is 60 d and the debt is 480 d.
- Every label ends in "(sure)", even where nothing is risked. On a chance the panel adds its own odds word after
  the label's (the file's own note says so).
- 43 of the 112 cards in `crew_793.json` are the evening song: "Listen" or "Not tonight", at a rate of 0.6 an hour
  from the eighteenth hour. That is a stop every evening at sea to ask a question that is not one.
- The written outcomes exist (`writing/sea.json`: `.done`, `.success`, `.failure`). `EncounterPanel.cs` does not
  show them.

Fix. Add `hours +n` (she lies to and the day stick moves), `damage sail` and `damage hull`, and `lose_lot`. Reprice
the sea deck by one rule: the safe choice costs hours; the bold one saves them and risks a lot, a wound or the sail.
Cut any card where no option moves days, 10 d or more, a lot, a man, or a flag the player can see. Take the song off
the cards: one log line, and the text in the chest. Show the written outcome. Drop "(sure)".

### 4. The scene that is the game cannot deal the deck

Read from the code, not run. `chart_game.md` lists part of this as a known gap.

Evidence:

- `ChartBuilder.cs` puts no `LevelInfo` and no `LevelWeather` in `L_Chart`. `EncounterGameVars.cs` reads
  `level_kind` from the first and wind, storm, fog and ice from the second. In the chart they read as nothing and 0,
  while `ChartBody` sails on `WorldSim.WeatherAt`. The cards and the boat are in two different weathers.
- 32 of the 63 sea cards name `level_kind`. Nine more need wind, storm, fog or ice above nought. Two thirds of the
  deck is shut, and every gale, squall and fog with it. `storm_vow` and `old_gods` start on a storm.
- What can still open has no month in its `when`. `sea.frost.dawn` ("ice in the men's beards") and
  `sea.geese.skeins` ("finish this run and lay her up") can come on 22 June.
- `EncounterRunner.Moorings`: this level has no berth in `chain.json`, so "no port calls here". `moored` is never
  true. No toll man, no harbour card. 40 of the 42 chains start "moored and port.id = ..." and cannot start. That
  includes `house_debt.s01_tally`, the opening of the game.
- One fault in any one of the 50 card files and "none will open" (`EnsureDirector`). The last content commit calls
  itself "unchecked drafts".
- `ChartGameTests.cs` proves that she sails 20 km and moors. It buys nothing, sells nothing and opens no card. No
  capture shows a market or a card. Nobody has seen the loop run.

Fix, in this order. Every `Port` that `ChartWorld` makes is a mooring, so arriving begins a call and casting off ends
it. Feed the cards the chart's own weather and whether she is on a river or the sea. Put months on the frost and the
geese. A file with a fault shuts itself off, not the whole book. Then three new checks in the play test: the tally
card opens at Dorestat in a new game; a sea card opens on a crossing to Lundenwic; a cask sold there raises the silver.

### 5. Hearing and choosing are solved before you cast off

Evidence:

- `KapmanGame.KnowHome` copies Dorestat's whole board into your ledger. Capture 01: "Prices for 34 markets: the
  freshest 4 days ago, the oldest 20 days ago." Knowledge is meant to be the treasure. You start rich.
- `PortLife.Talk` is one free button with no limit. It gives up to eight pieces of news and every price on the
  waterfront. You press it once in every port, always. That is not a choice.
- The hold is 660 kg: four casks of wine at 138 kg of room each, one pair of querns, one sack of bread. That is
  654 kg. There is nothing to fit. The answer is four of the dearest thing.
- The first leg everyone sails is to Traiectum (the play test, and minute 5 of `depth_design.md` section 3). It is in
  the same region as Dorestat (`ports_793.json`: both `rhine_delta`), so wine has the same price less the spread and
  the tenth: about 38 d a cask where Lundenwic pays about 86. The first sale teaches the wrong lesson.
- `worldSeed` is fixed at 793 (`KapmanGame.cs` line 71) and every draw is seeded by it and the hour. Every new game
  is the same game.
- The world's ships work out the gain of a run before they choose it (`TraderBrain.cs`). The player is never shown one.

Fix. Start knowing home and three ports. Build three talks a call (`town_interactivity.md` 3) before any other town
work: it is the one rationed act in the design, and the game has no rationed act today. Give Traiectum a reason: the
priest's four casks for 12 d, as your own first hour says. Show the likely gain on the choice: "Four casks to
Lundenwic: about 150 d, 2 to 4 days." Roll the seed on New game.

### 6. The first ten minutes say nothing about who you are or what to do

Evidence, from the three captures:

- The screen opens on "The waters round her", "No course set: click a port." and a menu: Explore the town and
  markets, Talk to people, Cast off, Visit the toll house, Exchange. Not a word on the boat, the debt or the count.
- To sail you click a port on the chart, press "Set course" on its card, go back to the menu and press 3. The menu
  sits on her mark and on the ports south of her.
- "5. Exchange" changes all your foreign coin at a tenth's loss (`PortLife.Exchange`). Four or five pennies gone for
  a curious click.
- "Hold: 654 of 660 kg of room" reads as empty. She is full.
- Under way the left panel says 18 km to go and the right says 20. After she moors the right still says "Bound for
  Traiectum: 4 km to go, about 0 days".
- The estimate was "about 0.1 days". It took six hours, 07:00 to 13:01. `Course.PlanKnots` is a flat 3.5 whatever
  the wind does.
- The sea is magenta in all three captures. The rivers, which are your roads for the first hour, are hairlines.
- At Traiectum the same menu comes up. Nothing happened on the way and nothing has changed. At minute ten a new
  player has moved a dot 20 km and has no reason to move it again.

Fix. Build minutes 0 to 15 of `depth_design.md` section 3 and nothing else until a stranger passes its own marks
(cast off by minute 10, a sale by minute 20): the reeve's card, the priest's wine, one river card, the line on
arrival that says what changed, a buyer with a name, and one log line that says what the run made. One click on a
port sets the course and offers "Cast off". Days as words ("half a day"), worked out from the wind. The sea of
`MapArt/world/overview.png` is good as it is: use that one.

### 7. A library has been written, and the game cannot open six megabytes of it

Evidence:

- No `.cs` file reads `places/*.json` (50 ports, 610 sites, 444 people, 4.6 MB), `town_actions_793.json`,
  `unlocks_793.json` (128 unlocks), `winter_793.json`, `ships_793.json`, `works_793.json` or `sayings_793.json`. The
  town in capture 01 is five lines of menu.
- So the grand reward of every chain is a flag that nothing reads: `unlock_plot_parisius`,
  `unlock_ship_north_sea_merchant_ship`, `unlock_title_thegn`.
- Lundenwic has 17 sites and about a hundred acts on paper. Doccinga, a church on a mound, has 8 sites open in 793;
  `town_interactivity.md` 6 gives a landing three. A port call like that is a maze of menus, not one table.
- 42 chains of 13 or 14 cards. Fifteen start in 830 or later. `year_1066` is written. The first Michaelmas cannot be
  reached. `MapArt/world_whole/overview.png` runs from Greenland to Arabia; the game sails from Dorestat to Utrecht.
- The drafts disagree with each other. The debt's day: 365 days, or Michaelmas. The haggle: a coin flip
  (`town_actions_793.json`), or your notes (`depth_design.md` 2.1). The crew: three fixed parts in `sea_793.json`,
  five in `crew_793.json`, four seats in `voyage_rules.json`, other names again in the mock. What an heir keeps:
  `depth_design.md` section 4 says itself that the files disagree.
- Your own research said it (`research_indie_and_concept.md` 2 and 3): "Small on purpose." "The first release needs
  about twelve done well."

Fix. Stop writing. Choose 12 ports and the 6 chains that can start by 795: `house_debt`, `black_stones`,
`pilgrim_road`, `pilots_secret`, `storm_vow`, `lindisfarne_book`. Move the rest out of the game's data into a `later`
folder. Wire the 12 places: three to five sites each, four acts a site at most. A reward that is not built is built,
or cut from the card.

### 8. One voice in 444 mouths

Evidence:

- Line by line the writing is good. In bulk it is one man talking. Everyone is dry, exact and sparing, and ends on
  a saying.
  - The steward: "A day has no price, and that is what makes it dear."
  - A mother: "You can make another candle. I cannot make another son."
  - A heathen shipmaster: "Otherwise you have only dropped something."
  - A priest: "I did not write them."
  - A bishop: "He has had gold from kings, and I think he liked it less."
  - A cellarer, of last year's wine: it "has stopped arguing with itself".
  - A daughter, of her dead father: "He would have asked what they charged."
- The people files say it of themselves, in `trait` and `voice`. Holmfast: "no word wasted". Fastulf: "Few words,
  each one weighed." Aldger: "sparing with words". Ibba: "Short, exact". Hartwin: "never raises his voice". Sigimar:
  "Says a weight, then a loss, then nothing". Holmfast of Birka "counts everything twice". So does Holmfrid of Helgo,
  the next island.
- The facts follow one mould: two plain sentences and a wry third. "He looks at the candle, and then at your shoes."
  "... three men have found reasons to be standing next to it."
- Every chain is 13 or 14 cards. Choices come in threes and fours, and the last is free.
- Lines that do not work, because they tell the player what to think, or nothing: "Evening at sea. The lookout has
  the next part of his song ready, if you want it." (43 times.) "It is a thing a merchant can use." "Two of your men
  have reasons to care." "The men are in poor health. A leech at the landing offers his craft; there is also a woman
  up the lane."
- Nobody is stupid, loud, drunk, afraid or lying badly. Nobody talks too much. After an hour it reads as written by
  one hand at one sitting, which it was.

Fix. You were meant to overwrite the drafts. At this size you will not. Keep the best 150 cards. Give each of the 12
ports one loud person: a bore, a braggart, a bad liar, a child. One saying a card at most. Let one card in five be
short and plain: one line, two choices. The test: read ten cards aloud with the names covered. Where you cannot tell
who is speaking, rewrite.

### 9. The Command frame turns the chart into wallpaper, and says everything twice

Evidence, from my render of `template3.html`, layout G:

- Its own words: "You run the game from the frame, and the chart is a picture you read." You pick the port from a
  list. Your decision of 4 October was "the chart is the game".
- The list shows seven ports. The game has 50 and starts with 34 known.
- Five tabs (Voyage, Trade, People, Tasks, Event) and eight buttons (Ledger, Crew, Hold, Chest, Chronicle, House,
  Tasks, World). Tasks is in both. Trade, Ledger and Hold are one job in three places. People and Crew are one job
  in two.
- One decision is cut in half. The Trade tab has the word ("A Frisian skipper said Lundenwic pays well"). The Voyage
  tab has the days. Neither shows the gain.
- The top bar has the silver and the bread. The debt's day is not on it. The debt is under your own portrait: "You.
  Merchant of Dorestat · 31. Sound · owed 480 d." That block is about 7% of the screen for your own face.
- The log says "Prime" and "Terce" and the bar says "third hour": the period labels you called obnoxious.
- Four directions in three days: three things on a table (concept 4.1); no frame, no tabs (`ui_direction.md`); a
  thick frame with eight buttons (`depth_design.md`); tabs (layout G). The game still shows `MapTable`'s bar.

What I would never use: World; House ("Boats 1", "Heir none yet"); Chronicle as a button; my own portrait; the
measuring line once the list shows days.

What I would reach for and not find: the gain of a run before I commit; the debt and its day always in sight; the
wind on the chart; "sail on until something happens"; a line on arrival that says what the run made and why the
price moved; what a click will do before I click. (`depth_design.md` 1.6: the hint line "never says what a click
does". That is wrong for a new player.)

Fix. Stop choosing frames. Build four things on the game as it stands: the day stick (the date, the debt's day, the
bread line, the course's days); the tally (silver and debt); the port slip beside the chart, with the word, the days
and the gain together; the card under a face. You go to a port by clicking it. A button arrives only when its thing
exists: `depth_design.md` 1.3 has that right.

### 10. Accuracy wins every argument, and play pays for it

Evidence:

- The boat is a 7.7 m rowing boat of about 1130 standing in for 793 (`ships_793.json` says so). True to her size she
  carries four casks, so there is no cargo game until a ship of 2,400 d (`unlocks_793.json`).
- Real knots on a clock of a minute a second (problem 1).
- Four coinages in the first purse (`trade_rules.json`, `start`: deniers, pennies, dirhams, hacksilver), and a tenth
  lost at each change. Your concept says: "No second currency."
- The ban on usury is right, and it means the debt never grows. A fixed sum with no bite is not pressure.
- Sunday shuts 19 acts (`town_interactivity.md` 5): "Two nights for one day's trade." With a crossing of "2 to 4
  days" you cannot plan round it. It is a tax, not a choice.
- The chart names ports only as the period did: Sliesthorp, Gyrwe, Hi, Ogvaldsnes, Legacaestir. The cards send you
  to them by those names.
- Four audits check the facts (`Design/content/audit`). None checks play. The goods audit says it did not judge
  `value`, `bulk`, the made and wanted factors or the `market` block. Those numbers are the game.
- Where it does feel like 793, and not like a textbook: Lindisfarne as news 13 days old; "fiery dragons in the sky"
  on the news panel; silver weighed and never counted at Birka; the tally stick, which your own page says is later
  and keeps because it plays. That last one is the right instinct. Use it more.

Fix. One rule: where a true thing makes a choice, keep it; where it makes a chore, bend it and say so in the credits,
as you did for the day stick. One silver until 865. A bigger hull within reach of the first season, or the hold of
twelve lots that `depth_design.md` 1.3 already assumes. The modern name beside the period one until you have called
at a port. Sunday shown on the day stick before the course is set, or left out. And one audit of play: a stranger
plays the first hour each week while you watch and say nothing.

## What is good and must be kept

1. **The chains at their best.** `house_debt` and `storm_vow` are real dilemmas. Pay the count by tale, 480 d, or by
   the moneyer's weight, 367 d, and he likes you less. Later cards remember what you did. The endings say which is
   history as it was and which is history changed. The finales, 900 to 2,600 d, are sized to the 480 d debt. "Say
   nothing. Steer." is an ending I would remember.
2. **The word on the item** (`Known/Word.cs`, `PriceNotes.cs`). A price is something a named skipper said last week,
   and a world of 138 ships and a dated chronicle makes it true or stale. I know no other trading game with this, and
   it is built. Make the word scarce and it is the game.
3. **The clock that waits while you think** (`PanelClock.cs`), and days as the one fuel. The right rule. Now let the
   cards spend it.
4. **The sea cards as prose.** "The sail hangs from the yard like washing and the sea is as flat as poured lead."
   "They do not wave. They have been waving since yesterday." "There are four men aboard and no door."
5. **The rule in `sea_793.json`:** every danger warns first and wounds before it kills. Keep it when the cards get
   teeth.
6. **The small true oddities in the place files.** The bronze man on a peg in the hall at Helgo, "and the game never
   tells them". This is what makes a port worth a second call.
7. **The engine.** Seeded, saved, checked by a loader, with some 276 tests. It can carry every fix above without
   being written again.

## The game I would rather it were

One summer and one winter, done well. Dorestat and eleven ports round the southern North Sea, 793 to 795, one small
boat, four named men, and the count's 480 pennies as the spine. You know three prices when you start and you buy the
rest with three talks a call. A crossing takes a minute or two: her mark runs, and stops two or three times for a
card that asks for half a day, a cask or a man's goodwill, and you can always see Michaelmas coming on the stick. You
arrive, and one line tells you why the price moved and what the run made. Winter is one board that reads your own log
back to you and asks one question. I would rather play 80 Days on the North Sea of 793 than Crusader Kings with a
loading bar in the shape of a boat. When that loop holds a stranger for ten hours on twelve ports, add the heir, and
only then the other 38 ports and the road to 1066.

## What to do next, in order

1. Make port calls and sea cards happen in `L_Chart` (moorings, the chart's weather, months on the frost and the
   geese), and prove it in `ChartGameTests` with one sale and two cards.
2. One debt, owned by the `house_debt` chain, due at Michaelmas, with the earnest of 40 d; then build minutes 0 to 15
   of the first hour.
3. Give the cards teeth: the verbs `hours`, `damage`, `lose_lot` and `debt`; reprice the sea deck so the safe choice
   costs hours; the song off the cards; the written outcomes shown.
4. Make a crossing one to two minutes: the waiting rate between cards, and the wind, the days and the likely gain
   shown before cast off.
5. Freeze the content at 12 ports and 6 chains; cut the first ledger to home and three ports; three talks a call;
   then put a stranger in front of the first hour, every week.
