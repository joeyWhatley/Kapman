<!-- 7 Oct 2026. One of two critiques asked for by the user. Read from the designs, the code, the play test and its
three captures, the data files and the git log. The game was not run. No web search was made (the session's search
budget was spent): figures for other games are the ones in research_indie_and_concept.md. Counts come from scripts run
over the data files that day. (E) = my estimate. -->

# Kapman: the designer and producer's critique

## Verdict

There is no game yet. There is a good trade-and-news simulation with a map on top, a card engine that cannot fire in
the scene that is meant to be the game, and about 700,000 words of content that nothing has played.
The project found its right shape on the night of 4 October (one table, days as fuel, five beats, twelve ports) and has
built away from it since the morning of 5 October: wider, later, and on paper.
Stop everything wide, close the five beats on four ports with one debt that bites, and play that hour every night
until you want a second one.

## What is there today

What a player can do in `L_Chart`, from the code and the captures: click a port, set a course, cast off, watch her
mark, arrive, open the market, buy and sell, press Talk.

What does not happen:

- **No card opens in port.** `EncounterRunner.Moorings` takes its berths from `chain.json`. `L_Chart` has none. The
  play log says so: "no berth in chain.json for the level '': no port calls here". 734 of the 1,022 cards are harbour
  cards.
- **Sea cards cannot see the weather.** `EncounterGameVars` reads `wind`, `storm` and `fog` from `LevelWeather.Current`
  and `level_kind` from `LevelInfo.Current`. `ChartBuilder` adds neither, so they read 0 and "". The chart's own wind
  comes from `WorldSim.WeatherAt`. The storm_vow chain starts at `storm >= 0.6`: never.
- **Nothing can be lost.** `Trade.DebtDue` is shown and never acted on. `Voyage.Health` and `Voyage.Morale` are
  written by everything and read only by the screen and by card conditions: no man dies or leaves by them. Having no
  bread lowers a number.
- **Hearing is free and whole.** `KnowHome` gives prices for 34 markets at minute zero (it is on the first capture).
  `PortLife.Talk` gives up to eight news items and every price the port knows in one click.
- No reveal on arrival. No reckoning. No winter. No end.

**The smallest fun thing** is one leg: you hold a word nine days old, you choose a port by it, you spend days getting
there, the game tells you whether you were right and why, and counts the leg in one line. The hard half is built and
good: 138 ships that move prices and carry news, `Known`, `PriceNotes`, `Word.cs`. The cheap half is missing: scarce
hearing, a cost in days, the reveal, the reckoning, a debt day that does something. About ten nights (E). Since
5 October the work has gone elsewhere.

## The ten worst problems

**1. The loop is not closed and has no stakes.** Evidence: the list above. A merchant with "one boat and a debt" has a
debt that is a label and stores that are a number. Do: stage 1 below, and nothing else until it passes.

**2. The project left its own build order after one step.** `chart_game.md` lists seven. Step 1, the word on the
item, was built on 5 October. The day stick, cards at sea, the voyage log and winter, four men, the hold and the
window were not started. 5 and 6 October made instead: 50 place files, 43 chains, three new designs, three interface
mockups, a map of the whole world, five colour schemes, 2,709 art slots. Decision 5 of the concept says "twelve ports,
done well". Do: the seven steps are the only list again.

**3. The content was written for an engine that does not exist.**

- No effect can cost time. `Effects.cs` has no hours or days. The concept's one resource cannot be spent by any card.
  A card's note says "costs the afternoon"; the engine charges nothing.
- No effect can touch the debt. The house_debt chain takes 480 d of silver and sets a flag, `ch_house_debt_owed = 0`.
  `Trade.Debt` still reads 480 on the top bar. The chain says Michaelmas. The code says 365 days.
- Verbs that return null in `GameEffects.cs`: `passenger` (97 uses), `learn` (94), `tongue` (50), `encounter` (27),
  `open_fetch`. An option that asks `tongue.old_norse >= 0.5` can never open.
- 275 `unlock_` flags and 81 `changed_` flags are set by cards. No line of C# reads either. Plots, charters, factors,
  partners, titles, heirlooms and ships are prizes for systems not built. 906 of 2,455 flags are never read by a card.
- `morale` is the commonest outcome after flags and feelings: 786 effects. Nothing acts on morale.

Do: add `hours`, `days` and `debt`. Give health and heart one rule each. Treat every other missing verb as a reason to
leave a card out, not a reason to build its system.

**4. Most of the content cannot be reached.**

- By their first card, 12 of 43 chains can start in 793 or 794. 24 cannot start before 800, 17 not before 828. About
  240 cards cannot come before 834. They need heirs, which are not built.
- About 250 cards live in a window of three years or less, about 165 of them at one or two ports. `dorestad_834`
  opens only if she lies at one of five ports between April and June 834.
- The clock is one game minute a second, four times at most: a day at sea is six real minutes at 4x. The fleets plan
  puts Birka 35 days out at the fleet's pace. That is three and a half hours each way. lindisfarne_book needs Norway,
  Northumbria and Hedeby.
- 1,105 of 1,478 story beats in the place files name no card.

Do: reach is the first test a card must pass (see "The content").

**5. Four interfaces, one built.** `ui_direction.md` (6 Oct): no frame, no tabs, click the thing. `depth_design.md`
(same day): "this overrules it", a thick frame, eight buttons, a dock. `template3.html` layout G (now): a focus panel
with five tabs, "the chart is a picture you read". Built: `MapTable`'s bar, an outliner, a numbered menu from
`TextPanel`. depth_design's 55 nights were written against layout E. Do: choose G today (reasons in the table below),
write it on one page, move the other two out of sight.

**6. Seven build orders, about 150 nights, none started.** chart_game 7 steps; ui_direction 15; town_interactivity
12; living_world 16 (26 nights); fleets 14; depth_design 20 (55 nights); and `GAME_PLAN.md` with M0 to M8 for the 3D
chain of levels, still called the master plan. Each later page overrules an earlier one. Most say "nothing was run in
Unity". The Design folder holds 193 markdown files and about a million words. Do: one list on one page. The rest go
to `Design/later`.

**7. The tests pass because they test something else.** `ChartGameTests` closes the title by reflection, sets the
course in code, presses the menu by reflection, runs at 40x and asserts that she comes within 3 km. It never opens the
market, buys, sells, talks, saves or loads. It saves three pictures and checks none: the third shows "Bound for
Traiectum: 4 km to go" in one panel and "No course set" in another, after she has arrived. EditMode has 276 tests in
56 files; well over a third of the files are for the 3D world (bilge, helm, grounding, sea state, sky, soundscape).
Do: the tests under "Process".

**8. Card money and trade money are two different games, and the home river has no prices.** She starts with 86 d,
owes 480 and holds five casks. My rough sum from `goods_793.json` at base prices, not a run: wine to Lundenwic clears
about 29 d a cask, 145 d a full hold. Dorestat, Traiectum and Witla are one region (`rhine_delta`), so anything bought
at one sells at a loss at the other: spread and toll take about a quarter. That is the test's leg and the first leg of
depth_design's first hour. Against that, 351 card options pay 480 d or more, and chain endings pay 600 to 2,600 d.
One ending pays the debt five times. Do: run a soak to learn what a leg earns. Until then no card pays more than one
leg (about 60 d) and no chain more than one season. Give the short river legs a reason: an order, or a want of
Traiectum's own.

**9. Every port was cast in one mould, and the audits were not applied.** Doccinga, size 1: 8 sites in 793 and 62
things to do. Fordwic, size 1: 12 sites, 88. Dorestat, size 5: 12 sites, 112. `town_interactivity.md` says a landing
has three sites. Each file is 60 to 150 KB with about five stories, five works, eight people and ten sayings. The
project's own audit of `ports_793.json`: 18 rows right, 14 wrong, 18 doubtful, "the table was not edited". The texts:
208 right, 41 doubtful, 18 wrong, not edited. Do: when a port is admitted, cut it to its size and apply its audit
first.

**10. The art plan is years of work, and the screen is harder to read than the placeholder.** 2,709 slots: 328
portraits, 335 scenes, 508 card pictures. The whole-world map is 540 tiles of 4096 by 4096. At two hours a piece that
is 5,400 hours (E), for someone learning to draw. In the captures the sea is magenta, the land is green and yellow
noise, her mark is a few white pixels, I cannot find the course line, the port menu lies over Andoverpis and Ganda,
and the same crew line is printed twice. `MapArt/world/overview.png` with its blue sea reads far better. Do: blue sea
back, one colour scheme, a mark and a course you can see. The art list for stage 1 is about 40 pieces.

## The contradictions, and which side should win

| # | The clash | Where | Should win |
|---|---|---|---|
| 1 | Slips on a bare chart / a thick frame with eight buttons / a Command frame with five tabs / what is built | ui_direction; depth_design 1; template3 G; MapTable, TextPanel | **G.** Voyage, Trade, People, Tasks, Event are the five beats. It is the cheapest to build and to test. Shelve the hand-drawn kit of 150 sprites |
| 2 | "The chart is the game" / "the chart is a picture you read" | DECISIONS 4 Oct; template3 G | Both, if a click on a port still sets the course. Write that sentence down |
| 3 | Twelve ports done well / 50 ports, 43 chains to 1066, 15,447 gazetteer places, a map to the Caspian | concept decision 5; Data; MapArt/world_whole | Twelve. Four first |
| 4 | Days are the only fuel / no card can cost time | concept 4.2; Effects.cs | The concept |
| 5 | Three talks a call / Talk gives everything, 34 markets known at the start | concept 4.3, town_interactivity 3; PortLife, KnowHome | Three talks |
| 6 | 480 d by Michaelmas / 365 days / a second debt kept in chain flags / an earnest on St John's day | DECISIONS 10; trade_rules.json; chain_house_debt; depth_design 3 | Michaelmas, one number in `Trade.Debt`. Keep the earnest: it is a good first card |
| 7 | One purse, no second currency / four coins, zones, Exchange, "two purses from 865" | concept 4.2 and 4.7; trade_rules.json; depth_design 2.6 | One purse in the first release |
| 8 | "No text the game invents", and the older rule that you write / 700,000 words of draft | concept 4.7; content/BRIEF.md | Yours to say. My advice: drafts only for ports in the current release, each read by you before it loads |
| 9 | A landing has three sites / hamlets with eight to twelve | town_interactivity 6; Data/places | The design |
| 10 | "No list of verbs" / 53 acts on up to 17 sites | town_interactivity 2; town_actions_793 | G's tabs and five acts. The rest wait |
| 11 | Six keels in shares, the house divides at a death / eight keels / the heir keeps every hull | fleets plan, depth_design 2.5; unlocks_793; winter_793 | The fleets plan, frozen. depth_design flags this itself |
| 12 | Ship prices differ for four hulls | ships_793 rules; unlocks_793 | ships_793. One table |
| 13 | One clock at 4x most / "a leg is 5 to 15 minutes" / Birka 35 days out, a map to Vinland | DECISIONS; concept 4.3; fleets plan 2.7 | The small world now. The far world needs a decision from you, and it can wait |
| 14 | "Nothing new is managed, no new screen" / four ledgers, forty wars, hinges, a World panel, a War layer | living_world intro against its sections 2, 9, 10 | Neither yet. Frozen until a year is fun |
| 15 | History is real and dated / 81 `changed_` flags, "which England there is" | chronicle; living_world 3; chains | Real history in version one |
| 16 | Two ways to arrive, two event systems | Port graphs and MooringWatch; GameEvents and the cards | The port's graph starts the call. Cards only |
| 17 | The master plan describes a 3D chain of levels | GAME_PLAN.md, PROGRESS.md | Mark both retired |

## Systems

**These feed each other, and they are the game:** the world's ships move prices; ships carry news; `Known` and
`Word.cs` turn that into what you know and how old it is; the chronicle strikes markets through `Shocks`; raiders,
king's men and traders meet her on the chart. Keep all of it.

**These stand alone:** the card engine (sound, seeded, linted, and cut off from the chart scene, from time and from
the debt); `GameEvents` (an older, second event system with 13 happenings); the chest's side games (hoards, rune
reading, dividers, pin, string); `Beats`, `VoyageDirector` and the lessons (3D only, switched off in the chart);
health and heart (gauges with no rule).

**The one that is missing: the leg.** A record from cast-off to mooring that owns the card count, the days, the wages
and bread, the reveal and the reckoning line. Every design assumes it: the five beats, the teller's "one to three
cards a leg", the three crossings, known waters, the winter read back from the log. Nothing in code knows what a leg
is. `Voyage` counts days and `Course` is a path.

| Cut from the game's path now (keep in git) | Freeze: no code, no pages | Keep and build on |
|---|---|---|
| `GameEvents` and events_793.json | Living world: towns, holds, wars, hinges | WorldSim, markets, news, `Known`, `Word` |
| Four coins and Exchange | Fleets, heirs, the division | The card engine and its lint |
| The gazetteer (5.4 MB, read by nothing) | depth_design 2.1 to 2.6 beyond talks and the reveal | The chronicle (550 of 582 right by its audit) |
| The whole-world map bake | The ship window, ship physics, level weather, wildlife, soundscape | Save, the tick ladder, `PanelClock` |
| Four of the five colour schemes | Hoards, runes, dividers, pin, string | Concept sections 4 to 6, DECISIONS |
| 48 of the 53 town acts | 46 place files, 42 chains, the story files | house_debt; the Dorestat, Traiectum and Lundenwic texts |
| | All art past about 40 pieces | `SimHarness`, for soaks |

About 17,000 of the 50,000 lines in `Code` serve the 3D world (E). `Code/Game/Chart` is 214 lines. Do not delete the
3D code and do not move off HDRP now: either is another restart. Just do not open it.

## The content: asset or debt

**A debt with an asset inside it.** If the loop is not fun it is worth nothing: more cards do not rescue a card game.

What I read: five place files (Dorestad, Lundenwic, Birka, Doccinga, Sarre); five chains end to end with their words
(house_debt, wine_fleet, storm_vow, dorestad_834, lindisfarne_book); thirty cards drawn at random from sea_793,
crew_793 and rhine_frisia_793; ships_793, town_actions_793, unlocks_793.

What I found:

- **The prose is the best thing made since 4 October.** One dry voice, sources on everything, people with ends of
  their own. "A drowned man pays nobody." "You can make another candle. I cannot make another son." "One night you
  are a guest, three nights you are my trouble." house_debt is a real short story with four honest endings.
- **The fit is poor.** Trade is done by card: wine_fleet hands over casks and 90, 240 and 1,500 d with no market in
  it. Stakes are soft: in ten sea cards no option costs time, one costs cargo (a barrel of tar), a few cost two to six
  pennies, and most move heart or health by 0.03. 43 of the 112 crew cards are "Listen / Not tonight" for a fitt of
  Beowulf: a nice ritual, and a line in the log, not a card.
- **Each file ends in a list of what it needs from the engine.** unlocks_793 asks for every `unlock_` flag to be
  wired, a yearly render at winter, counts the engine does not keep, two ports that do not exist.
- **The pieces disagree with each other** on prices, keels and what an heir keeps.
- One chain, saga_rhineland_flanders, has cards and no words: the run stopped first.

**How much to throw away:** delete nothing; move nine tenths out of the game's path. My guess is that a fifth to a
third survives play near its present form (E). The rest is a quarry.

**How to find out which part is good.** Six gates, in order, forty cards at a time:

1. **Reach.** Can it come in the first ten hours in the twelve ports? About 450 of 1,022 pass by year and port.
2. **Verbs.** It uses only effects the engine has.
3. **Stakes.** One option costs silver, cargo, a man or time.
4. **Money.** It pays no more than a leg; a chain no more than a season.
5. **Read.** You read it aloud once and keep, fix or bin it.
6. **Play.** Count what players choose. A card always answered one way is fixed or cut.

## Scope: an honest reading

The project's own table: Dredge, four people, two years, a scope that never grew. Sunless Sea, three core people,
fifteen months. 80 Days, three people, nine months, and burned out at a hundred cities. Kapman's list is longer than
all three together: wars without end, towns that grow, history that bends, six keels known by word, nine lives across
273 years, a 3D window, Vinland to the Caspian, 2,709 drawings. One person cannot finish that, with or without help
that writes fast. By depth_design's own sum the campaign is about a hundred hours. Nobody can play-test a
hundred-hour game alone.

What one person can finish: **one life.** 793 to about 805, twelve to twenty ports, a hundred and fifty cards you have
read, a winter screen, a handover to the heir as the ending. Build it so that each life is a whole game, and the long
campaign becomes later releases instead of a wall.

## Process: why "all tests pass" sits beside "a massive mess"

1. **Done means compiled, tested, committed.** It should mean you played it for ten minutes and said yes.
2. **The work is done wide, at night, unattended.** Commit 39b3010 is 345 files and 127,130 lines: "unchecked drafts;
   runs stopped at 95 percent usage". 66 commits in four days, 47 of them on 4 October.
3. **Every ask is answered wide, and nobody says no.** "Everything", "all settlements": each got a folder. The answer
   should have been "after the hour is fun".
4. **Checks are made by more writing.** The audits found 14 wrong ports and edited nothing.
5. **The play path runs through the least tested parts:** 14 generated visual-scripting graphs, the UI panels, scene
   wiring. The tested parts are models.
6. **There is no bug list.** "Extremely buggy" is written nowhere in the project.

To close the gap:

- **The hour, played by a bot through the real buttons:** hear, buy, click the port, cast off, answer a card, arrive,
  read the reveal, sell, read the reckoning, save, load, same state. No reflection.
- **An economy soak:** 200 seeded seasons of a plain trader from the player's start. Print profit a leg and how many
  pay the debt.
- **A card count:** cards a leg in the chart scene, and the cards that can never fire because a variable reads 0. The
  rates in crew_793 sum to 32 an hour if every condition held. Nobody has measured the real figure.
- **A screen check:** no "[write:", no stale line, no panel over her mark.
- **`Design/BUGS.md`:** each thing you see gets a line, then a test, then a fix.
- **One thing in work at a time.** No commit of data that you have not sampled. A fresh first hour, by you, every week.

## The five things most likely to sink it

1. **It is never played.** The plans get better and the hour does not exist.
2. **The fifth restart.** 3 Oct: true-size HDRP levels. 4 Oct afternoon: an open world at 1:40. 4 Oct night: the
   chart. 6 Oct: the thick frame. 7 Oct: the Command frame. There are five Kapman projects in `G:/kapman_unity`.
3. **The content sinks the loop.** The loader takes the whole folder and opens no card at all if one has a fault
   (`EncounterRunner.EnsureDirector`). Unread cards with wrong money arrive together, and you stop trusting the text.
4. **The art wall.** 2,709 slots and 540 map tiles for a beginner. The game waits for art, or you burn out.
5. **You lose hold of your own project.** 50,000 lines and generated graphs, written at night under a usage cap, with
   reports that say it works. When it breaks, only the writer can say why.

## The staged plan

### Stage 0: clear the table (2 nights)

1. One page, `Design/NOW.md`: four ports (Dorestat, Traiectum, Walichrum, Lundenwic: three price regions), one boat,
   one debt, layout G, the list below. Every other design page gets a first line: "frozen 7 Oct".
2. The loader reads a list of card files, not the folder (`EncounterRunner.cardFiles` allows it). Start with about 60.
3. Blue sea, quiet land, a mark and a course line you can see. Start `BUGS.md` with what you have seen.

### Stage 1: the playable hour (about 14 nights of code, two evenings of your reading) (E)

| # | Step | Nights | Its test |
|---|---|---|---|
| 1 | A call begins when the port's graph moors her; card weather from `WorldSim.WeatherAt` | 1 | The toll man opens at Traiectum in `L_Chart` |
| 2 | `hours`, `days` and `debt` effects. One debt, due at Michaelmas, 100 days off. On the day, house_debt's own card: pay, serve, pledge the boat. The earnest of 40 d on St John's day is the first card | 1.5 | Paying by card lowers the bar's figure. The day comes with nothing paid: the card opens |
| 3 | Stores bite: no bread for two days and a man goes ashore at the next port; heart under a third, the same | 1 | A seeded run with no food loses a man |
| 4 | **The leg:** two cards at most; wages and bread counted; the reveal line on arrival; the reckoning line; a log of legs | 3 | The line's pennies equal the change in the purse. No leg has three cards |
| 5 | Hearing: three talks a call (the keeper, a skipper just in, the benches). The start knows Dorestat's prices and three words, not 34 markets | 2 | A fourth talk is refused. A new game's ledger has one port |
| 6 | Layout G: five tabs holding the panels that exist. A click on a port still sets the course. One top line: the date, silver, days of bread, days to Michaelmas. No frame art | 3 | Nothing covers her mark. Every tab opens with the clock held |
| 7 | **Yours.** Read aloud and fix about 60 cards: house_debt 1 to 3, six for each port, twelve for river and sea, eight for the crew. Cut every option whose only effect is heart. The fitts become one line to click | 2 evenings | The lint passes. No card pays over 60 d |
| 8 | The bot's hour and the soak | 2 | Under "Process" |

**Pass marks**, played by you on a fresh game with a clock on the desk:

- cast off before minute 5, a sale by minute 20;
- at least one arrival where your word was wrong and the reveal said why;
- every card cost or won silver, cargo, a man or hours;
- at minute 60 you know whether Michaelmas is in reach, and you care;
- the soak's plain trader pays the debt in 40 to 70 runs of 100 (E);
- you start a second hour without being asked.

If the last one fails, change the loop, not the scope.

### Stage 2: ten hours, the first year (six to ten weeks of evenings) (E)

- Twelve ports within four days of home: the Rhine mouth, the Frisian coast, the Thames, Quentovic.
- The year: the sea closes at Martinmas; one winter screen (the legs read back, the debt settled, one thing bought);
  spring.
- Four named men with one trait and one want each. An empty seat is felt at the oars.
- About 150 cards, let in a port at a time through the six gates. Four chains that start in 793 or 794 in these
  waters: house_debt, black_stones, pilots_secret, pilgrim_road.
- Three friends play it, with a "send this" button that attaches the save and the log.
- Art: your own home-waters tile and about 60 sprites.

Gate: do players begin a second year?

### Stage 3: a game, the first life (E)

793 to about 805. Ten sailed years, twenty ports at most, the chronicle of those years, the heir's handover as the
ending. After that, one at a time, each played before the next is begun: a second life; one boat sent and known only
by word, which is the fleets plan's best idea; the Baltic. Not in version one: wars the world makes, hinges, six
keels, the 3D window, the world to the Caspian.

## What has earned praise

- **The simulation of trade and news.** Ships that move prices and carry word as old as their passage, and the word
  on the item built on it. No game in the project's own research has this. It is the game.
- **The concept of 4 October** (`research_indie_and_concept.md` 4 to 6). It is right, it is short, and it already
  says most of what this page says.
- **The home-waters writing,** above all house_debt and the people of Dorestat. Good enough to keep, once it is cut to
  fit a loop that exists.
