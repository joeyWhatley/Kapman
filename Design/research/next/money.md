# The money, reckoned

7 October 2026. One task: settle the game's money by arithmetic. I read the trade, voyage and world code and the
data tables, wrote a model of a season on those tables (`Bake/economy/soak.py`), and ran it for 300 seeded seasons
for each of three kinds of player, before and after tuning. The game was not run. No C# was run: the port is by
reading. No web search was made: the check against history rests on this project's own research pages. A (?) marks
what I could not check. The script's whole output is at the end.

## 1. The answer

1. **As the code stands the debt is no stake.** A sensible player pays the 480 d in 99 seasons of 100. His tally
   first reads 480 on day 12. He ends the season with 1,414 d. From 21 June (the book's start) it is 98 of 100, day 13.
2. **Both of the design's scales are true, of two different markets.** "About 150 d a crossing" is the table on the
   first morning of a new game, before any other ship has traded. "10 to 40 d a leg" (`CHAINS.md`) is the same table
   thirty days later, once the world's traders have closed the gaps. On an untouched table in July four casks to
   Lundenwic clear 116 d and to Ripa 205 d. With the others at work the same four casks clear about 10 d.
3. **Four things make the free money**, in this order: every market starts untouched (worth about 520 d a season to
   the sensible player); the famine doubles all Food, honey and cheese too (about 160 d); Frankish wine is bought
   at Quentovic at 29 d and sold in Kent at 73 d, one day away (the top good in nearly every run); and the four casks
   given at the start sell for about 335 d at Ripa on day 5.
4. **The smallest set that makes the debt a stake is three numbers and a cap.** The world's traders start on
   1 April, not on the player's first morning. The famine of 793 falls on grain only. The English toll is a tenth,
   like the Frankish. No card pays over 60 d and no chain over 240 d.
5. **With that set** the sensible player pays in 65 seasons of 100 from 1 May and in 48 of 100 from 21 June. With
   the reeve's 20 d at Lundenwic (already ruled in the book) it is 43 of 100 from 1 May. The perfect player always
   pays. The careless one never does. On day 100 of 151 the sensible player is still short in 58 seasons of 100,
   and the plain pace calls the ending wrongly in 22.
6. **The price of that is thin legs.** A laden leg makes 13 d in the mean, 38 d one time in ten. Ten days make 8 d
   in the middle and 67 d one time in ten. At 4x ten days are one real hour.
7. **Two things I had to guess move the result more than any price**: how many traders work these waters (42
   ships: 95 of 100; 70 ships: 30 of 100) and how keen the player is (a 10 % margin: 92; a 20 % margin: 35). The
   wind is a third guess, and I did not test it. The dial to turn when the real game disagrees is the English
   toll, then the day the world starts.

## 2. How money moves in the code

| What | The rule, as coded | The number | Where |
|---|---|---|---|
| A price | value x made factor x wanted factor x season x scarcity x glut x shock | made 3: x0.5, 2: x0.65, 1: x0.8; wanted 3: x1.7, 2: x1.4, 1: x1.15 | `Market.Price`, `goods_793.json` |
| Asked and paid | the market asks a tenth over its price and pays a tenth under | spread 0.20 | `Market.Asks`, `Pays` |
| Each unit moves the price | a unit bought makes the next dearer: (usual stock / stock)^0.4, between x0.6 and x2.2. A unit sold to a market that does not make the good gluts it: (1 + glut / appetite)^-0.5 | usual stock = size x made x 150 d / value; appetite = size x wanted x 80 d / value | `Deal.Buy`, `Deal.Sell`, `Market.GotOne` |
| Settling | stock, glut and shock each go a twentieth of the way back a day | regen 0.05, glut_fade 0.05, shock_fade 0.03 | `Market.Settle` |
| The toll | off every sale, by the zone | Frankish 0.10, English 0.05, weighed silver (Ripa) 0 | `Deal.Sell`, `trade_rules.json` |
| The hold | room is kg x bulk. 900 kg less four 60 l water casks | 660 kg of room: four casks of wine (138 each), one pair of querns (72), one sack of bread (30) | `Hold`, `KapmanGame` line 204 |
| The start | one purse and a cargo given at no cost | 85.5 d of silver (81 d at Dorestat while the four coins stand); 4 casks, 1 quern, 1 sack of bread | `trade_rules.json`, `ChartWorld` line 72 |
| The debt | a sum and a day; nothing happens on the day | 480 d, due in 365 days (the book: Michaelmas) | `Trade.Begin` |
| Wages | owed by the day, paid in port; a man owed 14 days goes ashore | 4 men x 0.25 d = 1 d a day | `Voyage.Day`, `InPort` |
| Bread | 0.7 kg a man a day from sacks in the hold | 2.8 kg a day; a 20 kg sack asks 2.64 d: 0.37 d a day | `Voyage.Take` |
| Ale | water is free in every port and is drunk first | 0 d. Ale is never needed | `Voyage.Day` |
| Time in port | the clock is held while a panel is open | buying and selling take no time | DECISIONS, 4 October |
| The other traders | 138 ships, 56 of them at home in the twelve ports. Each sells near the best she believes, weighs every port in reach, buys in lots of five while 12 % is left | start silver 60 d + 50 d a tonne; a fifth kept back; belief fades by half in 30 days | `TraderBrain`, `WorldSim` |
| The famine | Food twice as dear in the Frankish regions, held to August 794 | all 21 goods of the Food category | `Shocks`, `shocks_793.json` |
| A new game | the markets open and the world's ships are made on the player's first morning | no day of trade has happened | `KapmanGame`, at `Markets.OpenAll` and `new WorldSim` |

## 3. The sums by hand

- **Wages.** 1 d a day. From 1 May to Michaelmas is 151 days: 151 d. From 21 June: 100 d. The "24 d" of the
  Martinmas card is the winter keep, 6 d a man. Both stand; they are two different things.
- **Bread.** 0.37 d a day: 56 d from 1 May. Ship's bread is Provisions, not Food, so the famine does not touch it.
- **Ale.** Not needed. If the men drank ale and not water it would be 12 l a day: a cask every 9.6 days, 0.92 d a
  day, 139 d a season, and a cask's room (138 of 660 kg) gone from every hold.
- **What the day asks.** The start is worth 232 d (the purse and the cargo at home prices). To have 480 d on day
  151 the player must make 248 d after costs: 1.6 d a day. Before wages and bread that is 3.0 d a day on goods.
  From 21 June it is 2.3 d a day after costs, 3.7 d before. **A leg of two to three days must clear about 8 d.**
  Any world in which a plain leg clears 30 d pays the debt four times.
- **One crossing, cold table, 1 May.** Wine asks 44 d at Dorestat and pays 72 d at Lundenwic after the toll: 28 d
  on the first cask and about 100 d on the four that fit, once the price has moved under them, less 4 d wages and
  1.5 d bread for four days. At Ripa (no toll): 48 d on the first cask, about 165 d on the four. In July, when wine
  is dear at both ends, 116 d and 205 d (output, section 2).
- **The same crossing with the others at work**, first morning, traders sailing since 1 April: Dorestat asks 60 d
  and Lundenwic pays 59 d. Nothing. Over the whole season the mean is asks 68, pays 71: 3 d a cask.
- **Real time.** A game day is 24 real minutes at 1x and 6 at 4x. 151 days are 60 hours at 1x and 15 hours at 4x.
  100 days are 40 and 10 hours. Dorestat to Lundenwic is 3.8 days straight through and 4.5 lying up at dusk:
  23 to 27 minutes at 4x.

First-morning prices, mean of 24 seeds (asked where bought, paid where sold after the toll, gain on one unit):

| Good, bought at, sold at | Cold table, 1 May | Traders at work since 1 April | Cold table, 21 June | Since 1 April, 21 June |
|---|---|---|---|---|
| Rhenish wine, Dorestat to Lundenwic | 44 > 72: +28 | 60 > 59: -1 | 52 > 86: +34 | 69 > 73: +4 |
| Rhenish wine, Dorestat to Ripa | 44 > 92: +48 | 60 > 62: +2 | 52 > 110: +58 | 69 > 76: +7 |
| Frankish wine, Quentovic to Lundenwic | 29 > 73: +44 | 46 > 56: +10 | 35 > 87: +53 | 54 > 70: +16 |
| Frisian cloth, Medemelacha to Quentovic | 26 > 54: +29 | 41 > 41: -1 | 26 > 54: +29 | 40 > 42: +2 |
| Honey, Lundenwic to Medemelacha | 24 > 65: +42 | 27 > 27: 0 | 24 > 65: +42 | 27 > 28: +1 |
| Glass, Quentovic to Lundenwic | 60 > 86: +26 | 75 > 79: +3 | 60 > 86: +26 | 75 > 79: +5 |

(The second and fourth columns are the tuned set: its tolls, and the famine on grain only.)

## 4. The model

`Bake/economy/soak.py`. One file, about 2,000 lines, Python with numpy. A season of 300 runs takes about 30
seconds on this machine; the whole report takes about ten minutes.

**Ported from the code**: the price and every factor in it; unit-by-unit dealing; the hashed starting stock;
the seasons' price blend and the harvest, vintage and herring curves; the hold; wages and food; `Travel.Speed`
(the polar, beating, rowing at 1.75 knots, sheltering over 17 m/s); the ways by water from `lanes.bin` with the
course's own costs (433 km to Lundenwic, 563 km to Ripa); the famine and the Ripa fair; and the other traders,
rule for rule, with their beliefs, their boards and their ballast runs. The twelve ports are those of the book's
stage 2: Dorestat, Traiectum, Witla, Walichrum, Medemelacha, Doccinga, Daventre, Quentovic, Lundenwic, Sarre,
Fordwic, Ripa.

**My own guesses (?)**: the wind (one wind for the whole water, westerly five days in eight, a middle strength of
5.2 to 6.3 m/s by month, changing every six hours); the twelve ports as a closed world worked by their own 56
traders; and the three players.

| Player | What he knows | How he buys | How he sails |
|---|---|---|---|
| **Perfect** | the true price and stock of every market this hour, and the wind ahead | unit by unit against the true price at both ends; no margin wanted; keeps the law | through the dark; home to the hour |
| **Sensible** | the port he lies in; what he saw before (fading by half in 30 days); three talks a call, each the board's word of one port, as old as the ship that brought it; else what everyone knows (`Lore`) | expects the price to sag as he sells; buys only what should fetch 15 % over what he pays; never more than the market's appetite | lies up in the dark near land; no further than 150 km empty without a fresh word; counts the long way home |
| **Careless** | what everyone knows, and nothing else | by the almanac; never thinks of the glut | a day idle in every port; counts the short way home |

The debt is paid if the boat lies at Dorestat at noon on Michaelmas with 480 d in silver. What is in the hold is
sold there first. "In hand and in the hold" is silver, less wages owed, plus the hold at what it cost.

## 5. Before: the code and the data as they stand

300 seasons each. The full tables are in the output, section 4.

| 1 May to Michaelmas | Perfect | Sensible | Careless |
|---|---|---|---|
| Pays 480 d on the day | 100 of 100 | 99 of 100 | 0 of 100 |
| Silver on the day (one in ten under / middle / one in ten over) | 1,978 / 2,349 / 2,827 | 996 / 1,414 / 1,882 | 2 / 58 / 119 |
| Made a day, after wages and bread | 14.4 d | 8.2 d | -0.9 d |
| Legs a season; with goods aboard | 101; 89 % | 65; 68 % | 57; 95 % |
| A laden leg makes (mean / one in ten over) | 25 / 66 d | 30 / 95 d | -1 / 30 d |
| Ten days make (middle / one in ten over) | 104 / 285 d | 42 / 203 d | -12 / 71 d |
| The tally first reads 480 (middle season) | day 10 | day 12 | day 11, in 79 seasons of 100 |
| Gain on goods; tolls; wages; bread | 2,384; 1,424; 151; 56 | 1,440; 522; 151; 56 | 66; 727; 151; 52 |

From 21 June: perfect 100 of 100 (1,813 d), sensible 98 of 100 (1,048 d, day 13), careless 0.

**The careless player fails even here**, and how he fails is worth knowing. In four seasons of five he holds
480 d by day 11: he sells the count's casks. Then he trades it away into gluts he does not look at. And he is at
home on the day in only one season of eight. Being away on the day is the commonest way to lose.

**Which goods and legs dominate** (share of the season's gain on goods):

| | Goods | Bought at, sold at |
|---|---|---|
| Perfect | Frankish wine 35 %, honey 12 %, Rhenish wine 10 %, books 9 %, wheat 6 %, glass 6 % | Quentovic to Lundenwic 21 %, to Sarre 18 %, to Fordwic 12 %; Lundenwic to Medemelacha 11 % |
| Sensible | Frankish wine 35 %, honey 15 %, Rhenish wine 12 %, books 10 %, wheat 9 %, Frisian cloth 5 % | Quentovic to Lundenwic 22 %, to Sarre 19 %; Lundenwic to Medemelacha 17 %; Dorestat to Medemelacha 10 % |

Half of everything is one loop: wine from Quentovic to Kent, a day or two away, and back nearly empty. The home
river earns almost nothing: Dorestat, Traiectum, Witla and Daventre are one region, and the region makes only wine,
iron and bread.

## 6. Where the data makes free money

Section 2 of the output lists the 26 best single cargoes on an untouched table. 294 pairs of a good and a way
gain something; 35 gain over 100 d a hold; 5 gain over 300 d.

| # | The leak | The arithmetic | Worth |
|---|---|---|---|
| 1 | **The cold table.** A new game opens every market at its usual stock on the first morning. The 56 traders need about a month to close the gaps. The player races them with a faster turn-round (no time in port) | every gap in the table is open at once: see the first-morning prices in section 3 | the sensible player ends with 1,360 d on a cold table and 840 d when the traders start on 1 April: **520 d a season** |
| 2 | **The casks given at the start.** Four casks and a quern at no cost | about 335 d at Ripa on day 5 of a cold table (the first pays 92 d, no toll), and 18 d for the quern; 254 d at Sarre in a settled market, 228 d after the toll. On the cold table, with the purse, that is about 435 d in hand after one leg: nine tenths of the debt | half the debt |
| 3 | **The famine falls on all Food.** Honey, cheese, butter, salt, oil and fish double in the Frankish ports with the grain. English honey asks 23 d and pays 59 d in Frisia | honey, Lundenwic to Medemelacha: 22 jars, 391 d a hold | 840 d falls to 678 d with the famine on grain only: **160 d a season** |
| 4 | **Frankish wine.** Made 3 at Quentovic (x0.5: 25 d), wanted 3 in England (x1.7: 85 d). 114 km to Sarre | four casks: 191 to 204 d a hold on a cold table, in a day | the first good in nearly every run: 31 to 38 % of the gain |
| 5 | **What the law forbids.** Swords, mail and helmets from Quentovic or Dorestat to Ripa. The world's traders will not carry them (`TraderBrain.Forbidden`), so the gap never closes. `Deal.Buy` lets the player | ten swords: 718 d out, 343 d gain, 18 kg of room | a perfect player who keeps the law ends with 1,297 d; one who does not, with **4,217 d** |
| 6 | **Ripa takes no toll.** Every sale there keeps the tenth | wine pays 92 d at Ripa and 72 d at Lundenwic on the same morning | small once the traders are at work: 678 d to 634 d with a toll of 0.05 |
| 7 | **No time in port.** The clock is held while panels are open, so a call costs nothing | the perfect player makes 101 legs a season; the world's traders lie half a day to two days at every call | part of why the player beats the field at all. I would keep it |
| 8 | **One region, three markets.** Sarre, Fordwic and Lundenwic share one price and have separate stocks and gluts. Nine km from Sarre a second helping sells at the full price | the perfect player hops Fordwic to Sarre four times a season | small |
| 9 | **Books and glass at Quentovic.** A book asks 362 d and pays 431 d at Lundenwic | 69 d a book in 8 kg of room, for whoever has the silver | 9 to 13 % of the gain |

## 7. The tuning

Each row is 200 seasons of the sensible player from 1 May, on the same seeds. Each row keeps the changes above it.

| Step | Pays, of 100 | Silver on the day (low / middle / high) | Tally first reads 480 | Still short on day 100, of 100 |
|---|---|---|---|---|
| The code as it stands (every price free at each call) | 98 | 996 / 1,421 / 1,901 | day 12 | 0 |
| + three talks a call (the book's rule) | 98 | 841 / 1,360 / 1,811 | day 13 | 0 |
| + **the world's traders at work from 1 April** | 98 | 618 / 840 / 1,083 | day 50 | 4 |
| + **the famine on grain only** | 88 | 471 / 678 / 905 | day 71 | 26 |
| + **the English toll a tenth** (the tuned set) | 62 | 373 / 514 / 667 | day 92 | 59 |

Other levers, each alone in place of the English tenth (the row above it pays 88 of 100):

| Lever | Pays, of 100 | Middle silver |
|---|---|---|
| The reeve at Lundenwic, 20 d, eased 30 days | 74 | 596 |
| A landing due of 1 d at every call | 72 | 552 |
| Wages 0.375 d a man a day | 68 | 551 |
| Frankish wine made 2 at Quentovic and wanted 2 in England | 50 | 481 |
| Two casks aboard at the start, not four | 50 | 481 |
| Wages 0.5 d a man a day | 42 | 452 |
| The spread 0.30 | 18 | 395 |
| A toll of 0.05 at Ripa | 86 | 634 |

### The tuned set

| # | Number | Now | Tuned | File |
|---|---|---|---|---|
| 1 | The day the world's traders start | the player's first morning | **1 April** (30 days before 1 May, 81 before 21 June). Easter 793 was 7 April, my sum (?) | new: `world_from` in `trade_rules.json` "start"; a pre-run in `KapmanGame` |
| 2 | The goods of the great famine | `"food"` (21 goods) | **`"wheat,rye,barley,oats,malt,flour"`** | `shocks_793.json`, `793-great-famine` |
| 3 | The English toll | 0.05 | **0.10** | `trade_rules.json`, zone `english` |
| 4 | The most silver one card may pay | none (the largest is 2,600 d) | **60 d a card; 240 d a chain, start to finish** | the card lint; `CHAINS.md` |

Not changed: the debt (480 d), its day (Michaelmas), wages (0.25 d), the spread, every price in `goods_793.json`,
the start purse and cargo. The rule of three talks a call is the book's, and is in the tuned runs.

### Before and after

300 seasons each.

| 1 May to Michaelmas | Perfect, before | Perfect, after | Sensible, before | Sensible, after | Careless, before | Careless, after |
|---|---|---|---|---|---|---|
| Pays 480 d on the day, of 100 | 100 | 100 | 99 | **65** | 0 | 0 |
| At home on the day, of 100 | 100 | 100 | 99 | 98 | 12 | 16 |
| Silver on the day, middle | 2,349 | 1,312 | 1,414 | **521** | 58 | 0 |
| Silver, one in ten under / over | 1,978 / 2,827 | 1,120 / 1,481 | 996 / 1,882 | 360 / 701 | 2 / 119 | 0 / 54 |
| Made a day | 14.4 | 7.1 | 8.2 | 1.9 | -0.9 | -1.7 |
| A laden leg: mean / one in ten over | 25 / 66 | 16 / 45 | 30 / 95 | 13 / 38 | -1 / 30 | -5 / 8 |
| Laden legs that lose silver, of 100 | 13 | 13 | 18 | 29 | 64 | 74 |
| Ten days: middle / one in ten over | 104 / 285 | 64 / 133 | 42 / 203 | 8 / 67 | -12 / 71 | -10 / 2 |
| Tally first reads 480: one in four / middle / three in four | 8 / 10 / 12 | 16 / 22 / 30 | 8 / 12 / 15 | **61 / 91 / 107** | 10 / 11 / 12 | never |
| Tally never reads 480, of 100 | 0 | 0 | 0 | 29 | 21 | 100 |
| Still short on day 100, of 100 | 0 | 0 | 0 | **58** | 100 | 100 |
| The pace calls the ending wrongly on day 100, of 100 | 0 | 0 | 1 | 22 | 0 | 0 |
| Gain on goods / tolls paid | 2,384 / 1,424 | 1,280 / 1,522 | 1,440 / 522 | 500 / 263 | 66 / 727 | -69 / 260 |

| 21 June to Michaelmas (the book and the code) | Perfect, before | Perfect, after | Sensible, before | Sensible, after | Careless, before | Careless, after |
|---|---|---|---|---|---|---|
| Pays 480 d on the day, of 100 | 100 | 100 | 98 | **48** | 0 | 0 |
| Silver on the day, middle | 1,813 | 990 | 1,048 | **477** | 57 | 46 |
| Tally first reads 480, middle season | day 10 | day 17 | day 13 | day 52 | day 11 | never |
| Tally never reads 480, of 100 | 0 | 0 | 0 | 43 | 12 | 100 |
| Ten days: middle / one in ten over | 107 / 392 | 64 / 148 | 45 / 220 | 8 / 85 | -21 / 187 | -13 / 11 |

After tuning the sensible player's gain is still Frankish wine (37 % from 1 May) and the casks he was given
(Rhenish wine, 32 %), then wheat into the famine (18 %).

### The dial, when another rule is built

| Start; the reeve's 20 d; the English toll | Pays, of 100 (200 seasons) |
|---|---|
| 1 May; no reeve; 0.10 (the tuned set) | 62 |
| 1 May; no reeve; 0.075 | 71 |
| 1 May; the reeve; 0.10 | 43 |
| 1 May; the reeve; 0.075 | 50 |
| 1 May; the reeve; 0.05 | 74 |
| 21 June; no reeve; 0.10 | 48 |
| 21 June; no reeve; 0.075 | 58 |
| 21 June; no reeve; 0.05 | 76 |
| 21 June; the reeve; 0.05 | 70 |

So: **start on 21 June and the three numbers give one season in two.** Start on 1 May and they give two in three;
add the reeve, which the book has already ruled, and it is two in five.

### How firm it is

| What I guessed, moved | Pays, of 100 |
|---|---|
| The tuned set | 62 |
| 42 traders in these waters, not 56 | 95 |
| 70 traders | 30 |
| The player asks 10 % over what he pays, not 15 % | 92 |
| He asks 20 % | 35 |
| No talks at all | 48 |
| Every price free at each call (the code today) | 70 |
| The famine off | 71 |

Two things follow. **Skill decides**: the same numbers give 35 or 92 of 100 by how boldly the player buys. That is
what a game wants, and it means "half" is true of my sensible player, not of yours. And **the count of traders is
the strongest number in the economy**. `traffic_793.json` calls its own counts model estimates. When the real leg
exists, measure a plain trader with the bot's hour and turn the English toll or the world's start day until he
pays in 40 to 70 runs of 100, which is the book's own test (stage 1, step 11).

### What the tuning does not fix

The margins a settled market leaves are thin, because the world's traders stop only when less than 12 % is left
(`SimRules.minMargin`). A sensible laden leg clears 13 d. 55 of 100 legs carry nothing. If you want `CHAINS.md`'s scale to
be the daily truth (a laden leg at 30 d, a good ten days at 200 d), that is the cold table, and on it this boat
makes about 1,400 d a season. The debt that is then in doubt is about **1,400 d by Michaelmas**, or **480 d by
about day 12**. You cannot have a 480 d debt, a 150-day season and 30 d legs together.

## 8. The cap on cards

"No card pays more than a good voyage." In the tuned game ten days make 8 d in the middle, 30 d one time in four,
67 d one time in ten and 117 d one time in fifty. A good voyage is the one-in-ten: about 65 d. A whole season
makes 286 d in the middle.

- **A card: 60 d at most**, in coin or in goods at the home price. Sea and crew cards 10 to 40 d, as the book has.
- **A chain: 240 d at most in coin over all its steps** (one pound, half the debt, less than a middle season). The
  rest of a finale is paid in doors. The book's 300 d for one chain card is a whole season's gain.
- **A card's cost** should be of the same size: 10 to 40 d, a lot (a cask is 60 d), a day (1.4 d of wages and bread,
  and about 2 d of gain forgone).

Counted today in `Assets/Kapman/Game/Data/encounters` (every `silver +N`):

| Deck | Files | Payouts | Over 60 d | Over 300 d | 480 d or more | Largest |
|---|---|---|---|---|---|---|
| The six chains the book keeps | 6 | 85 | 66 | 42 | 39 | 1,500 d |
| The other chains | 37 | 607 | 501 | 342 | 317 | 2,600 d |
| Ports and the rest | 11 | 289 | 79 | 7 | 2 | 1,200 d |
| Sea and crew | 2 | 4 | 0 | 0 | 0 | 20 d |
| All | 56 | 985 | 646 | 391 | 358 | |

The middle payout is 160 d. All the payouts together are 398,312 d: 830 debts. `house_debt` itself pays 466 d on
one card.

**The ale for a talk.** The book prices a talk at 2 d of ale when you have no tidings to give. Three talks a call
are worth 37 d a season to the sensible player (514 d against 477 d with none), over about 70 calls: half a penny a
call. At 2 d a talk he would pay 420 d a season for 37 d of knowledge. Price it at **an obol (half a penny) for
the three**, or leave it free and let it cost only the choice of whom to ask.

## 9. Against what is known of about 800

From `Design/economy_and_trade.md` section 1.2 and `Design/content/economy`. I did not go back to the sources.

| Figure | In the game | What is known | Verdict |
|---|---|---|---|
| Grain | wheat 6 d a sack of a modius and a half; rye 4; oats 2; barley 4 | Frankfurt 794: a modius of wheat 4 d, rye 3, barley 2, oats 1 | Agrees. Barley is 3 d by the cap |
| Bread | 2.64 d for 20 kg: 7.6 kg a penny | Frankfurt 794: twelve two-pound wheat loaves a penny, about 9.8 kg | Agrees; ship's bread is baked twice |
| Wages | 0.25 d a man a day and his food | No wage of a boatman survives (?). 0.25 d buys 2.4 kg of bread at the 794 cap, a man's bread for two or three days. A mancus of 30 pence as "a month's skilled wages" is English and later | Stands. 0.5 d would be a skilled man's pay; a crew on shares is as likely as day wages (?) |
| Wine | 60 d for 100 l where it is neither made nor wanted | Déas, about 838: a sester for a penny, which is 50 to 180 d for 100 l by the size of the sester. Nijmegen 806: bought at the vintage for 2 d a modius and held for 4 or 6 | The level stands. The year's swing in the table (0.85 to 1.2) is far flatter than the law's two or three times |
| The Frankish toll | a tenth of every sale in every Frankish port | The king's tenth at Dorestad and Quentovic (charter of 777; the precept of 828 names them and the Clusae). Abbeys' men went free | Agrees at the two toll places. History took it there, not at every landing |
| **The English toll** | 0.05; tuned to 0.10 | No rate survives. About 1000, at Billingsgate, a small boat paid a halfpenny, a boat with a sail a penny, a keel four pence: by the hull, not by the cargo. A tenth is what kings took wherever a rate is written (Frankish, Lombard, Byzantine, Khazar, Bulgar) | **Play parts from history here.** The tuned tenth is a game number, not a finding |
| Ripa | no toll | The Frankish annals for 808 say Godfred moved the merchants of Reric to Sliesthorp for the sake of the toll | A Danish king took toll. The table's 0 is not history; it matters little |
| The famine of 793 | all Food twice as dear | The annals speak of hunger. Thionville 805 forbids selling grain dear in a famine | Grain only is the closer reading. Twice is a guess |
| The debt | 480 d: two pounds | An ox 24 d, a sword with its scabbard 84 d, a horse 120 d, a byrnie 144 d (Lex Ribuaria, reckoned at 12 d the solidus, which is the game's choice). The six-oar boat is 600 d in `ships_roster.md` | Of the right order: four fifths of the boat, twenty oxen |
| The boat | 660 kg of room, crossing to Lundenwic and Ripa | The Gislinge boat is a farm boat of about 1130. Frisian traders used far larger hulls | Play parts from history. It is the game's premise, and it is why four casks are a cargo |
| Arms | sold freely to the player | Herstal 779: no byrnie out of the realm. Thionville 805: all that a man carries is forfeit | The law is in the table and bites nobody: see section 6, row 5 |
| The traders | 56 hulls at home in the twelve ports | No count survives for any port | A model estimate, and the strongest number in the economy |

## 10. What the player sees in the frame

- **The tally sentence**, middle season of the tuned game from 1 May: day 0 "Owed 480. In hand and in the hold
  232." Day 25: 345. Day 50: 384. Day 75: 415. Day 100: 463. Day 125: 504. Michaelmas: 522. One season in ten ends
  at 360, short by 120; one in ten at 700. The sentence crosses 480 on day 91 in the middle season, and in three
  seasons of ten it never does.
- **"At this pace"** is wrong about the ending in 34 seasons of 100 on day 25 and 22 of 100 on day 100. Good: it is
  a word to doubt, like every other word in the game. Value the hold at what it cost, as the model does.
- **The first sale.** The four casks cost nothing, so the first reckoning reads "made 215 d" at Sarre. That is the
  count's capital, not gain. Say so in the line: "Four casks of the count's: 215 d after the tenth."
- **The gain line** on Where it pays must carry the sag, or the player cannot do what the sensible bot does. One
  more phrase: "Lundenwic takes about 13 casks before the price sags", "Sarre 5", "Fordwic 3", "Ripa 12" (Rhenish
  wine; the appetite in `Market.Resize`). For Frankish wine: 24, 10, 5.
- **The toll line** on every sale, as built: "Sold four casks for 240 d, less the tenth, 24 d."
- **The reveal** carries the field: on the tuned first morning Dorestat asks 60 d a cask because eleven casks are
  left of twenty-five. "Three keels loaded here this week."
- **The day rule.** The flag "Michaelmas: 480 d" and, under it, the days it takes to be home: the careless player
  loses by being away. A sign when the long count home equals the days left.
- **A real hour.** At 4x an hour is ten game days: 8 d in the middle hour, 67 d in a good one. The first hour
  from a tuned start on 21 June holds one crossing and one sale of the count's casks.

## 11. What a programmer must build

1. **The pre-run.** At a new game, make the markets and the world's ships as of 1 April and step them, with no
   player and no drawing, to the clock's start (`KapmanGame`, where `Markets.OpenAll` and `new WorldSim` are called;
   the world's hours and the markets' days may run negative, or the economy's day 0 moves to 1 April). The date is
   a new number, `world_from: "04-01"`, in `trade_rules.json` "start". The chronicle's held shocks apply from the
   same day. *Test:* on the first morning Dorestat asks about 60 d for a cask of Rhenish wine, not 44; two runs of
   one seed give the same `Markets.Hash()`.
2. **Two edits to data.** `shocks_793.json`, `793-great-famine`: `"goods": "wheat,rye,barley,oats,malt,flour"`.
   `trade_rules.json`, zone `english`: `"toll": 0.10`. *Test:* honey at Medemelacha pays about 27 d after the toll, not 55 to 65.
3. **The debt's day.** `Trade.DebtDue` is Michaelmas of the start year, not `due_days` 365 (the book, 4.5).
4. **The card lint.** Refuse `silver +N` over 60 on a card and over 240 summed along a chain. 646 payouts fail
   today, 66 of them in the six kept chains.
5. **The law on arms.** Either `Trade.Buy` refuses what `TraderBrain.Forbidden` refuses, or the search bites. With
   the cargo alone forfeit, a run of ten swords (343 d gained, 718 d laid out) breaks even at one search in three.
   Take "all that he carries", as Thionville has it, and one search in five is enough (g).
6. **The headless pace.** `SimUnits.AcrossDefault` is 32; the chart's scale is 60. In a headless run the world's
   ships then sail at about half speed against a true-speed player, and the field looks weaker than it is. Set
   60 for any C# soak (read in `SimRandom.cs` and `ChartWorld.cs`; not run).
7. **The soak as a test.** Keep `soak.py` beside the bot's hour. The book's pass mark, "the plain trader pays in
   40 to 70 runs of 100", is met by the tuned set from 21 June (48) and from 1 May (65, or 43 with the reeve).
   Re-run it whenever a price, a toll or the count of traders changes: `soak.py --seeds 100 --config tuned --brief`.
8. **The four casks.** No change to the number. Mark them in the hold as the count's, so the first reckoning does
   not read as gain.

## 12. Not checked

- The game was not run and no C# was executed. Every rule is ported by reading; a slip in the port is possible.
- The wind is my own. The game's `WeatherModel` (793 lines) is not ported. Passage times agree with the book's
  "three to four days" to Lundenwic.
- The twelve ports are a closed world here. In the game the 56 traders also sail to some forty other ports and 82
  more ships come in. The pressure in these waters may be higher or lower.
- The three players are bots of my making. A human will differ.
- Not modelled: the earnest of 40 d at St John's; the reeve's other two choices (three tides, round by Sarre:
  the bot does go round); the river toll gates in `lanes.bin` (16); the king's men and raiders; spoilage of the
  player's hold (`keeps` is in the data and `Hold.cs` does not use it); standing's price favour; the four coins
  (one purse of 85.5 d, as the book rules); the men leaving for want of bread or pay.
- Whether the famine's shock is applied to a new game begun in June I read in `Shocks.Apply` and did not see run.
- The English toll rate and a boatman's wage about 800: nothing I know of survives (?). Easter 793 on 7 April and
  Michaelmas 793 on a Sunday are my sums from the Julian calendar (?).
- The counts of card payouts are a text search for `silver +N`. Goods given by cards are not counted.

## The script's output

`& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\economy\soak.py`, 7 October
2026, 300 seeds. In the tables "low / mid / high" are one in ten under, the middle, one in ten over.

```text
# THE MONEY, RECKONED: soak.py, 300 seeded seasons for each kind of player

## 1. The water: km by the lanes from Dorestat, and days in this script's wind (?)
to               km       straight through   lying up in the dark near land
                       mean (1 in 10 over)              mean (1 in 10 over)
Traiectum        30            0.2 ( 0.4)                    0.2 ( 0.4)
Witla           146            1.3 ( 1.7)                    1.7 ( 2.2)
Walichrum       191            1.7 ( 2.3)                    2.3 ( 3.1)
Medemelacha     138            1.1 ( 1.5)                    1.4 ( 2.1)
Doccinga        374            3.0 ( 3.7)                    4.2 ( 5.2)
Daventre        179            1.4 ( 1.8)                    1.8 ( 2.4)
Quentovic       371            3.3 ( 4.3)                    4.2 ( 5.4)
Lundenwic       433            3.8 ( 5.0)                    4.5 ( 6.0)
Sarre           344            3.1 ( 4.0)                    3.4 ( 4.3)
Fordwic         331            3.0 ( 4.0)                    3.6 ( 4.6)
Ripa            563            4.3 ( 5.6)                    5.1 ( 6.5)

## 2. Free money in the tables as they stand: one good, one way, a full hold of 660 kg of room
(markets at their usual stock, 1 July, the famine on, no other ship; the gain is after the spread and the toll;
 'law' marks a good the capitularies forbid: the world's traders will not carry it, the player may)
good            bought at, sold at       units  outlay    gain  first unit       km 
honey           Lundenwic > Medemelacha     22     616     391    23 > 59       482 
byrnies         Quentovic > Ripa             6     732     370   103 > 220      746 law
swords          Quentovic > Ripa            10     718     343    60 > 129      746 law
glass_vessels   Quentovic > Ripa            11     668     339    51 > 110      746 
helmets         Quentovic > Ripa            11     668     339    51 > 110      746 law
honey           Lundenwic > Doccinga        17     447     268    23 > 59       567 
honey           Sarre > Medemelacha         11     332     233    23 > 59       401 
frisian_cloth   Medemelacha > Ripa          13     411     224    26 > 60       399 
rhenish_wine    Dorestat > Ripa              4     211     205    51 > 110      563 
frankish_wine   Quentovic > Lundenwic        4     134     204    33 > 87       240 
rhenish_wine    Traiectum > Ripa             4     215     201    51 > 110      534 
rhenish_wine    Witla > Ripa                 4     221     196    51 > 110      514 
rhenish_wine    Daventre > Ripa              4     221     196    51 > 110      489 
frankish_wine   Quentovic > Sarre            4     134     191    33 > 87       114 
frisian_cloth   Medemelacha > Quentovic     12     372     185    26 > 54       469 
honey           Sarre > Doccinga            10     288     184    23 > 59       487 
frankish_wine   Quentovic > Fordwic          4     134     174    33 > 87       122 
swords          Dorestat > Ripa              5     414     169    74 > 129      563 law
frisian_cloth   Doccinga > Ripa              8     268     153    26 > 60       315 
glass_vessels   Quentovic > Lundenwic        8     457     150    51 > 86       240 
honey           Fordwic > Medemelacha        6     183     145    23 > 59       410 
swords          Traiectum > Ripa             4     346     131    74 > 129      534 law
honey           Fordwic > Doccinga           6     183     125    23 > 59       496 
frisian_cloth   Doccinga > Quentovic         7     222     124    26 > 54       554 
rhenish_wine    Dorestat > Lundenwic         4     211     116    51 > 86       433 
rhenish_wine    Traiectum > Lundenwic        4     215     112    51 > 86       404 
pairs of good and way that gain at all: 294; over 100 d a hold: 35; over 300 d: 5

## 3. What the other traders do to prices (mean over the season, 24 seeds)
good            bought at, sold at                        no other ship           with the other traders
                                             asks / pays net / on offer       asks / pays net / on offer
rhenish_wine    Dorestat > Lundenwic              50.5 /    80.5 / 100%            68.0 /    70.6 / 100%
rhenish_wine    Dorestat > Ripa                   50.5 /   101.7 / 100%            68.0 /    71.4 / 100%
frankish_wine   Quentovic > Lundenwic             32.7 /    81.4 / 100%            52.2 /    68.8 / 100%
frisian_cloth   Medemelacha > Quentovic           26.2 /    54.4 / 100%            40.9 /    41.3 / 100%
glass_vessels   Quentovic > Lundenwic             51.7 /    86.2 / 100%            76.3 /    81.5 / 100%
honey           Lundenwic > Medemelacha           21.3 /    55.5 / 100%            30.3 /    37.4 / 100%
wheat           Lundenwic > Doccinga               4.5 /    16.9 / 100%             7.2 /    10.6 / 100%
cheese          Medemelacha > Quentovic            4.3 /     7.1 / 100%             4.8 /     5.7 / 100%
bar_iron        Dorestat > Medemelacha             7.2 /    11.3 / 100%             8.0 /     8.0 / 100%
books           Quentovic > Lundenwic            362.2 /   430.9 / 100%           387.2 /   416.6 / 100%
wool_cloth      Lundenwic > Dorestat              13.0 /     8.7 / 100%            13.0 /     8.7 / 100%
quernstones     Dorestat > Lundenwic               7.9 /    14.4 /   0%             7.9 /    14.4 /   0%

## 4. BEFORE: the code and the data as they stand
### before, 1 May to Michaelmas
start 05-01, due 09-29 (day 151); debt 480 d; purse 85.5 d; cargo 4 rhenish_wine, 2 quernstones, 1 ships_bread, 1 ale; hold 660 kg of room
spread 0.20; toll Frankish 0.10, English 0.05, weighed silver 0.00; wage 0.25 d a man a day (4 men); appetite_value 80; stock_value 150; regen 0.05; glut_fade 0.05
made_factor {"0.5": 1.4, "1.0": 0.8, "2.0": 0.65, "3.0": 0.5}; wanted_factor {"1.0": 1.15, "2.0": 1.4, "3.0": 1.7}
famine on (food); other traders x1.00, at work 0 days before day one; talks a call 12

                                         perfect    sensible    careless
seasons                                      300         300         300
PAYS 480 d on the day, %                     100          99           0
home on the day, %                           100          99          12
silver on the day: 1 in 10 under            1978         996           2
silver on the day: middle                   2349        1414          58
silver on the day: 1 in 10 over             2827        1882         119
the sum paid in 8 seasons of 10             2116        1126           0
the sum paid in 5 seasons of 10             2349        1411           0
the sum paid in 2 seasons of 10             2621        1738           0
made in the season (middle), d              2149        1207        -139
made a day, d                               14.4         8.2        -0.9
legs a season                              101.2        64.8        57.2
a leg: days                                  1.5         2.3         1.6
a leg: km                                    194         222         152
made a leg: mean, d                           22          20          -1
made a leg: middle, d                          8           2          -4
made a leg: 1 in 4 over, d                    27          18           4
made a leg: 1 in 10 over, d                   61          66          27
legs that lost silver, %                      22          45          66
legs with goods aboard, %                     89          68          95
  their share of the days, %                  91          77          93
  a laden leg: days                          1.5         2.6         1.6
  made a laden leg: mean, d                   25          30          -1
  made a laden leg: middle, d                 10           9          -5
  made a laden leg: 1 in 4 over               31          37           5
  made a laden leg: 1 in 10 over              66          95          30
  laden legs that lost silver, %              13          18          64
made in ten days: middle, d                  104          42         -12
made in ten days: 1 in 4 over, d             159          99           6
made in ten days: 1 in 10 over, d            285         203          71
made in ten days: 1 in 50 over, d            611         505         285
gain on goods a season, d                   2384        1440          66
  tolls paid (already off), d               1424         522         727
  wages, d                                   151         151         151
  bread, d                                    56          56          52
  the reeve and landing dues, d                0           0           0
days under way                               151         149          94
days lying in port for want of a cargo           0           3           0
days with no bread                           0.0         0.0         8.2
tally first reads 480: 1 in 4 by day           8           8          10
tally first reads 480: middle day             10          12          11
tally first reads 480: 3 in 4 by day          12          15          12
tally never reads 480, %                       0           0          21

In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over
  day   0   perfect     192 /  202 /  213   sensible    192 /  202 /  213   careless    192 /  202 /  213
  day  25   perfect     821 / 1113 / 1482   sensible    546 /  892 / 1209   careless    249 /  493 /  710
  day  50   perfect    1078 / 1373 / 1751   sensible    642 / 1034 / 1348   careless    116 /  167 /  244
  day  75   perfect    1302 / 1624 / 2013   sensible    749 / 1152 / 1504   careless     86 /  116 /  151
  day 100   perfect    1560 / 1901 / 2317   sensible    848 / 1269 / 1674   careless     78 /  119 /  174
  day 125   perfect    1812 / 2127 / 2576   sensible    976 / 1363 / 1809   careless     48 /   77 /  116
  day 150   perfect    1979 / 2349 / 2822   sensible    999 / 1416 / 1885   careless     19 /   64 /  117
  day 151   perfect    1979 / 2349 / 2827   sensible    997 / 1415 / 1885   careless     18 /   63 /  116

Still short of 480 on the day (% of seasons), and how often the plain pace calls the ending wrongly (%)
  day  25   perfect   short   0, pace wrong   0   sensible  short   4, pace wrong   1   careless  short  48, pace wrong  90
  day  50   perfect   short   0, pace wrong   0   sensible  short   0, pace wrong   1   careless  short 100, pace wrong   4
  day  75   perfect   short   0, pace wrong   0   sensible  short   0, pace wrong   1   careless  short 100, pace wrong   0
  day 100   perfect   short   0, pace wrong   0   sensible  short   0, pace wrong   1   careless  short 100, pace wrong   0
  day 125   perfect   short   0, pace wrong   0   sensible  short   0, pace wrong   1   careless  short 100, pace wrong   0

perfect: goods by share of the gain: frankish_wine 35% (826 d); honey 12% (278 d); rhenish_wine 10% (231 d); books 9% (210 d); wheat 6% (140 d); glass_vessels 6% (137 d); cheese 4% (86 d); hounds 4% (85 d)
perfect: bought at, sold at, by share of the gain: Quentovic to Lundenwic 21%; Quentovic to Sarre 18%; Quentovic to Fordwic 12%; Lundenwic to Medemelacha 11%; Dorestat to Medemelacha 5%; Medemelacha to Quentovic 5%; Lundenwic to Quentovic 4%; Sarre to Quentovic 2%
perfect: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 11.5x 34 d 0.9 days; Quentovic to Lundenwic 10.2x 40 d 2.0 days; Sarre to Quentovic 9.4x 6 d 0.8 days; Lundenwic to Quentovic 9.1x 8 d 1.8 days; Quentovic to Fordwic 6.7x 39 d 0.9 days; Fordwic to Quentovic 5.4x 6 d 0.9 days; Fordwic to Sarre 3.8x 13 d 0.1 days; Medemelacha to Quentovic 3.7x 30 d 3.8 days

sensible: goods by share of the gain: frankish_wine 35% (506 d); honey 15% (220 d); rhenish_wine 12% (171 d); books 10% (148 d); wheat 9% (123 d); frisian_cloth 5% (67 d); cheese 4% (58 d); glass_vessels 3% (41 d)
sensible: bought at, sold at, by share of the gain: Quentovic to Lundenwic 22%; Quentovic to Sarre 19%; Lundenwic to Medemelacha 17%; Dorestat to Medemelacha 10%; Medemelacha to Quentovic 9%; Quentovic to Fordwic 8%; Sarre to Medemelacha 3%; Lundenwic to Doccinga 2%
sensible: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 7.0x 36 d 1.3 days; Quentovic to Lundenwic 6.5x 38 d 2.8 days; Sarre to Quentovic 4.7x 3 d 1.2 days; Medemelacha to Quentovic 4.5x 24 d 4.6 days; Lundenwic to Quentovic 4.4x 2 d 2.6 days; Lundenwic to Medemelacha 3.5x 61 d 4.2 days; Quentovic to Fordwic 2.9x 35 d 1.4 days; Sarre to Lundenwic 2.1x 17 d 1.4 days

careless: goods by share of the gain: frankish_wine 52% (236 d); rhenish_wine 24% (109 d); cheese 12% (55 d); books 5% (21 d); bar_iron 3% (14 d); wheat 3% (13 d); glass_vessels 1% (2 d); sheep 0% (1 d)
careless: goods that lost: parchment -56 d; frisian_cloth -118 d; linen -132 d
careless: bought at, sold at, by share of the gain: Traiectum to Medemelacha 43%; Medemelacha to Quentovic 25%; Quentovic to Sarre 25%; Quentovic to Lundenwic 5%; Quentovic to Medemelacha 1%; Medemelacha to Traiectum 0%; Sarre to Traiectum 0%; Quentovic to Fordwic 0%
careless: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 19.9x 1 d 1.2 days; Sarre to Quentovic 19.6x -13 d 1.2 days; Quentovic to Lundenwic 4.0x 0 d 2.7 days; Lundenwic to Quentovic 3.9x -5 d 2.5 days; Quentovic to Medemelacha 1.8x -3 d 4.3 days; Traiectum to Dorestat 1.6x -2 d 0.4 days; Medemelacha to Traiectum 1.4x -2 d 1.4 days; Medemelacha to Quentovic 1.3x 51 d 4.6 days

(900 runs in 71 s)

### before, 21 June to Michaelmas (the start in the book and in the code)
start 06-21, due 09-29 (day 100); debt 480 d; purse 85.5 d; cargo 4 rhenish_wine, 2 quernstones, 1 ships_bread, 1 ale; hold 660 kg of room
spread 0.20; toll Frankish 0.10, English 0.05, weighed silver 0.00; wage 0.25 d a man a day (4 men); appetite_value 80; stock_value 150; regen 0.05; glut_fade 0.05
made_factor {"0.5": 1.4, "1.0": 0.8, "2.0": 0.65, "3.0": 0.5}; wanted_factor {"1.0": 1.15, "2.0": 1.4, "3.0": 1.7}
famine on (food); other traders x1.00, at work 0 days before day one; talks a call 12

                                         perfect    sensible    careless
seasons                                      300         300         300
PAYS 480 d on the day, %                     100          98           0
home on the day, %                           100          99          12
silver on the day: 1 in 10 under            1496         723           7
silver on the day: middle                   1813        1048          57
silver on the day: 1 in 10 over             2136        1395         114
the sum paid in 8 seasons of 10             1600         794           0
the sum paid in 5 seasons of 10             1813        1042           0
the sum paid in 2 seasons of 10             2049        1290           0
made in the season (middle), d              1596         830        -161
made a day, d                               16.0         8.4        -1.6
legs a season                               67.9        43.1        38.4
a leg: days                                  1.5         2.3         1.6
a leg: km                                    192         213         145
made a leg: mean, d                           24          20          -3
made a leg: middle, d                          7           1          -7
made a leg: 1 in 4 over, d                    27          17           1
made a leg: 1 in 10 over, d                   64          70          39
legs that lost silver, %                      22          47          73
legs with goods aboard, %                     90          68          94
  their share of the days, %                  92          76          91
  a laden leg: days                          1.5         2.5         1.5
  made a laden leg: mean, d                   27          31          -3
  made a laden leg: middle, d                 10           8          -9
  made a laden leg: 1 in 4 over               31          36           3
  made a laden leg: 1 in 10 over              70         101          46
  laden legs that lost silver, %              13          22          71
made in ten days: middle, d                  107          45         -21
made in ten days: 1 in 4 over, d             185         122           2
made in ten days: 1 in 10 over, d            392         220         187
made in ten days: 1 in 50 over, d            649         413         358
gain on goods a season, d                   1735         977         -26
  tolls paid (already off), d                939         346         547
  wages, d                                   100         100         100
  bread, d                                    37          37          34
  the reeve and landing dues, d                0           0           0
days under way                               100          98          62
days lying in port for want of a cargo           0           3           0
days with no bread                           0.0         0.0         5.9
tally first reads 480: 1 in 4 by day           7          11          10
tally first reads 480: middle day             10          13          11
tally first reads 480: 3 in 4 by day          12          16          12
tally never reads 480, %                       0           0          12

In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over
  day   0   perfect     212 /  221 /  234   sensible    212 /  221 /  234   careless    212 /  221 /  234
  day  25   perfect     856 / 1125 / 1421   sensible    487 /  755 / 1053   careless    289 /  540 /  787
  day  50   perfect    1101 / 1385 / 1690   sensible    596 /  879 / 1239   careless     92 /  155 /  264
  day  75   perfect    1329 / 1623 / 1947   sensible    699 / 1002 / 1349   careless     49 /   79 /  119
  day 100   perfect    1497 / 1814 / 2136   sensible    724 / 1049 / 1396   careless     13 /   62 /  113

Still short of 480 on the day (% of seasons), and how often the plain pace calls the ending wrongly (%)
  day  25   perfect   short   0, pace wrong   0   sensible  short   9, pace wrong   2   careless  short  43, pace wrong  91
  day  50   perfect   short   0, pace wrong   0   sensible  short   2, pace wrong   2   careless  short  99, pace wrong   3
  day  75   perfect   short   0, pace wrong   0   sensible  short   0, pace wrong   2   careless  short 100, pace wrong   0

perfect: goods by share of the gain: frankish_wine 38% (652 d); rhenish_wine 13% (231 d); books 9% (153 d); honey 9% (149 d); glass_vessels 5% (95 d); wheat 4% (78 d); frisian_cloth 3% (59 d); hounds 3% (58 d)
perfect: goods that lost: amber -1 d
perfect: bought at, sold at, by share of the gain: Quentovic to Lundenwic 21%; Quentovic to Sarre 20%; Quentovic to Fordwic 13%; Dorestat to Medemelacha 9%; Lundenwic to Medemelacha 8%; Medemelacha to Quentovic 3%; Lundenwic to Quentovic 3%; Sarre to Quentovic 2%
perfect: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 7.8x 39 d 0.9 days; Quentovic to Lundenwic 6.9x 42 d 2.0 days; Sarre to Quentovic 6.4x 6 d 0.8 days; Lundenwic to Quentovic 6.3x 8 d 1.8 days; Quentovic to Fordwic 4.5x 46 d 0.9 days; Fordwic to Quentovic 3.8x 7 d 0.9 days; Sarre to Fordwic 2.6x 11 d 0.1 days; Fordwic to Sarre 2.5x 18 d 0.1 days

sensible: goods by share of the gain: frankish_wine 34% (329 d); rhenish_wine 22% (211 d); honey 14% (133 d); books 11% (105 d); wheat 6% (63 d); glass_vessels 3% (28 d); cheese 3% (25 d); hounds 2% (20 d)
sensible: bought at, sold at, by share of the gain: Quentovic to Lundenwic 20%; Quentovic to Sarre 19%; Dorestat to Medemelacha 18%; Lundenwic to Medemelacha 15%; Quentovic to Fordwic 9%; Medemelacha to Quentovic 4%; Traiectum to Lundenwic 2%; Lundenwic to Doccinga 2%
sensible: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 4.0x 41 d 1.3 days; Quentovic to Lundenwic 3.5x 36 d 3.0 days; Lundenwic to Quentovic 2.9x 2 d 2.7 days; Sarre to Quentovic 2.9x 3 d 1.3 days; Lundenwic to Medemelacha 2.4x 55 d 4.3 days; Medemelacha to Quentovic 2.2x 13 d 4.6 days; Traiectum to Dorestat 2.0x -0 d 0.3 days; Quentovic to Fordwic 1.9x 40 d 1.5 days

careless: goods by share of the gain: rhenish_wine 40% (131 d); frankish_wine 34% (110 d); cheese 11% (35 d); books 6% (20 d); bar_iron 4% (15 d); wheat 4% (13 d); sheep 0% (1 d); whale_oil 0% (1 d)
careless: goods that lost: frisian_cloth -50 d; parchment -66 d; linen -142 d
careless: bought at, sold at, by share of the gain: Traiectum to Medemelacha 71%; Medemelacha to Quentovic 26%; Quentovic to Medemelacha 2%; Medemelacha to Traiectum 0%; Quentovic to Lundenwic 0%; Lundenwic to Quentovic 0%; Quentovic to Fordwic 0%; Lundenwic to Traiectum 0%
careless: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 14.1x -6 d 1.3 days; Sarre to Quentovic 13.9x -14 d 1.2 days; Quentovic to Medemelacha 1.8x -4 d 4.2 days; Traiectum to Dorestat 1.6x -2 d 0.4 days; Dorestat to Traiectum 1.4x -7 d 0.3 days; Medemelacha to Traiectum 1.3x -2 d 1.5 days; Medemelacha to Quentovic 1.3x 37 d 4.5 days; Dorestat to Quentovic 1.0x -6 d 4.7 days

(900 runs in 48 s)

## 5. Where the gain goes: one change at a time (the sensible player, 200 seasons each)
From the code as it stands to the tuned set, a step at a time (1 May to Michaelmas)
                                                            pays  home silver on the day  a day     a laden leg    ten days tally reads it  never   short
                                                               %     %  low / mid / high      d  mean / 1 in 10  mid / good   by day (mid)      %  day100
the code as it stands (every price free at each call)         98    98   996 / 1421 / 1901    8.3      30 /   96    43 / 202             12      0      0%
+ three talks a call (the rule in the book)                   98    98   841 / 1360 / 1811    7.6      28 /   88    38 / 186             13      0      0%
+ the other traders at work from 1 April                      98    98   618 /  840 / 1083    4.0      18 /   58    28 / 106             50      0      4%
+ the famine on grain only                                    88    99   471 /  678 /  905    3.0      17 /   59    14 /  92             71     10     26%
+ the English toll a tenth  = THE TUNED SET                   62    98   373 /  514 /  667    1.9      13 /   38     8 /  67             92     31     59%

Other levers, each alone on top of: from 1 April, three talks, famine on grain (English toll still 0.05)
                                                            pays  home silver on the day  a day     a laden leg    ten days tally reads it  never   short
                                                               %     %  low / mid / high      d  mean / 1 in 10  mid / good   by day (mid)      %  day100
(none)                                                        88    99   471 /  678 /  905    3.0      17 /   59    14 /  92             71     10     26%
the reeve at Lundenwic, 20 d, eased 30 days                   74    99   371 /  596 /  782    2.4      14 /   52     9 /  80             77     22     42%
wages 0.375 d a man a day                                     68   100   344 /  551 /  783    2.2      14 /   55     6 /  81             72     26     48%
wages 0.5 d a man a day                                       42    99   245 /  452 /  695    1.5      12 /   49     1 /  70             79     48     64%
the spread 0.30                                               18    99   308 /  395 /  517    1.3      13 /   35     2 /  54            104     74     90%
Frankish wine: made 2 at Quentovic, wanted 2 in England       50    98   359 /  481 /  618    1.7      12 /   34     6 /  60            100     40     70%
two casks aboard at the start, not four                       50    98   278 /  481 /  695    2.0      12 /   37    11 /  66            104     47     76%
a landing due of 1 d at every call                            72    98   363 /  552 /  799    2.3      15 /   56     8 /  82             84     22     47%
a toll of 0.05 in the lands of weighed silver (Ripa)          86    98   465 /  634 /  859    2.7      15 /   56    12 /  85             80     10     31%

## 6. AFTER: the tuned set
### tuned, 1 May to Michaelmas
start 05-01, due 09-29 (day 151); debt 480 d; purse 85.5 d; cargo 4 rhenish_wine, 2 quernstones, 1 ships_bread, 1 ale; hold 660 kg of room
spread 0.20; toll Frankish 0.10, English 0.10, weighed silver 0.00; wage 0.25 d a man a day (4 men); appetite_value 80; stock_value 150; regen 0.05; glut_fade 0.05
made_factor {"0.5": 1.4, "1.0": 0.8, "2.0": 0.65, "3.0": 0.5}; wanted_factor {"1.0": 1.15, "2.0": 1.4, "3.0": 1.7}
famine on (wheat,rye,barley,oats,malt,flour); other traders x1.00, at work 30 days before day one; talks a call 3

                                         perfect    sensible    careless
seasons                                      300         300         300
PAYS 480 d on the day, %                     100          65           0
home on the day, %                           100          98          16
silver on the day: 1 in 10 under            1120         360           0
silver on the day: middle                   1312         521           0
silver on the day: 1 in 10 over             1481         701          54
the sum paid in 8 seasons of 10             1194         409           0
the sum paid in 5 seasons of 10             1312         519           0
the sum paid in 2 seasons of 10             1421         614           0
made in the season (middle), d              1080         286        -279
made a day, d                                7.1         1.9        -1.7
legs a season                              102.8        71.5        52.9
a leg: days                                  1.5         2.1         1.8
a leg: km                                    192         199         173
made a leg: mean, d                           11           5          -4
made a leg: middle, d                          3          -1          -2
made a leg: 1 in 4 over, d                    15           4          -1
made a leg: 1 in 10 over, d                   37          19           3
legs that lost silver, %                      36          68          85
legs with goods aboard, %                     73          45          55
  their share of the days, %                  74          58          49
  a laden leg: days                          1.5         2.7         1.6
  made a laden leg: mean, d                   16          13          -5
  made a laden leg: middle, d                  7           6          -2
  made a laden leg: 1 in 4 over               22          17           0
  made a laden leg: 1 in 10 over              45          38           8
  laden legs that lost silver, %              13          29          74
made in ten days: middle, d                   64           8         -10
made in ten days: 1 in 4 over, d              97          30          -7
made in ten days: 1 in 10 over, d            133          67           2
made in ten days: 1 in 50 over, d            191         117          23
gain on goods a season, d                   1280         500         -69
  tolls paid (already off), d               1522         263         260
  wages, d                                   151         151         151
  bread, d                                    55          55          37
  the reeve and landing dues, d                0           0           0
days under way                               151         149          98
days lying in port for want of a cargo           0           3           0
days with no bread                           0.0         0.0        45.3
tally first reads 480: 1 in 4 by day          16          61         nan
tally first reads 480: middle day             22          91         nan
tally first reads 480: 3 in 4 by day          30         107         nan
tally never reads 480, %                       0          29         100

In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over
  day   0   perfect     217 /  232 /  246   sensible    217 /  232 /  246   careless    217 /  232 /  246
  day  25   perfect     424 /  502 /  591   sensible    303 /  345 /  422   careless     58 /   91 /  168
  day  50   perfect     557 /  677 /  792   sensible    312 /  384 /  469   careless     18 /   51 /   93
  day  75   perfect     719 /  843 /  977   sensible    325 /  415 /  526   careless     -9 /   30 /   92
  day 100   perfect     867 / 1024 / 1160   sensible    351 /  463 /  608   careless    -33 /    9 /   91
  day 125   perfect    1006 / 1179 / 1345   sensible    374 /  504 /  663   careless    -59 /  -20 /   83
  day 150   perfect    1122 / 1313 / 1479   sensible    362 /  523 /  704   careless    -85 /  -44 /   57
  day 151   perfect    1121 / 1313 / 1482   sensible    361 /  522 /  702   careless    -85 /  -45 /   54

Still short of 480 on the day (% of seasons), and how often the plain pace calls the ending wrongly (%)
  day  25   perfect   short  39, pace wrong   0   sensible  short  98, pace wrong  34   careless  short 100, pace wrong   0
  day  50   perfect   short   1, pace wrong   0   sensible  short  92, pace wrong  30   careless  short 100, pace wrong   0
  day  75   perfect   short   0, pace wrong   0   sensible  short  77, pace wrong  27   careless  short 100, pace wrong   0
  day 100   perfect   short   0, pace wrong   0   sensible  short  58, pace wrong  22   careless  short 100, pace wrong   0
  day 125   perfect   short   0, pace wrong   0   sensible  short  40, pace wrong  13   careless  short 100, pace wrong   0

perfect: goods by share of the gain: frankish_wine 36% (456 d); rhenish_wine 16% (208 d); books 12% (148 d); glass_vessels 8% (103 d); hounds 5% (70 d); wheat 5% (63 d); lead 5% (58 d); linen 3% (45 d)
perfect: bought at, sold at, by share of the gain: Quentovic to Lundenwic 26%; Quentovic to Sarre 22%; Quentovic to Fordwic 12%; Lundenwic to Quentovic 6%; Dorestat to Ripa 4%; Sarre to Quentovic 3%; Quentovic to Medemelacha 3%; Dorestat to Medemelacha 3%
perfect: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 10.2x 26 d 0.9 days; Sarre to Quentovic 8.9x 4 d 0.8 days; Lundenwic to Quentovic 8.6x 7 d 1.8 days; Quentovic to Lundenwic 8.4x 34 d 1.9 days; Quentovic to Fordwic 5.5x 25 d 1.0 days; Traiectum to Dorestat 4.8x 0 d 0.2 days; Fordwic to Quentovic 4.6x 3 d 0.9 days; Medemelacha to Traiectum 2.4x 1 d 0.9 days

sensible: goods by share of the gain: frankish_wine 37% (184 d); rhenish_wine 32% (161 d); wheat 18% (91 d); glass_vessels 3% (16 d); hounds 2% (9 d); quernstones 2% (8 d); barley 1% (5 d); rye 1% (5 d)
sensible: bought at, sold at, by share of the gain: Quentovic to Lundenwic 23%; Quentovic to Sarre 14%; Dorestat to Sarre 11%; Dorestat to Lundenwic 7%; Lundenwic to Medemelacha 5%; Quentovic to Medemelacha 5%; Sarre to Medemelacha 3%; Quentovic to Fordwic 3%
sensible: legs sailed most (times a season, made a leg, days): Traiectum to Dorestat 5.7x -0 d 0.3 days; Quentovic to Sarre 3.4x 19 d 1.3 days; Medemelacha to Traiectum 3.2x -1 d 1.2 days; Sarre to Quentovic 3.0x 1 d 1.2 days; Medemelacha to Daventre 2.9x -1 d 1.1 days; Daventre to Traiectum 2.5x -2 d 1.8 days; Quentovic to Lundenwic 2.5x 31 d 2.8 days; Lundenwic to Quentovic 2.3x -0 d 2.6 days

careless: goods by share of the gain: frankish_wine 49% (42 d); wheat 37% (31 d); rhenish_wine 10% (9 d); whale_oil 2% (2 d); woad 2% (1 d); glass_vessels 0% (0 d); madder 0% (0 d); eels 0% (0 d)
careless: goods that lost: parchment -17 d; frisian_cloth -28 d; bar_iron -101 d
careless: bought at, sold at, by share of the gain: Quentovic to Sarre 55%; Quentovic to Medemelacha 42%; Medemelacha to Traiectum 2%; Medemelacha to Quentovic 1%; Dorestat to Dorestat -0%; Sarre to Traiectum -0%; Dorestat to Quentovic -0%; Doccinga to Medemelacha -1%
careless: legs sailed most (times a season, made a leg, days): Traiectum to Dorestat 12.1x -1 d 0.3 days; Medemelacha to Traiectum 9.0x -2 d 1.3 days; Dorestat to Quentovic 7.1x -5 d 4.4 days; Quentovic to Sarre 5.1x 5 d 1.2 days; Sarre to Quentovic 5.1x -9 d 1.2 days; Quentovic to Medemelacha 4.7x 1 d 4.1 days; Dorestat to Medemelacha 3.9x -17 d 1.6 days; Quentovic to Traiectum 2.5x -4 d 3.5 days

(900 runs in 89 s)

### tuned, 21 June to Michaelmas (the start in the book and in the code)
start 06-21, due 09-29 (day 100); debt 480 d; purse 85.5 d; cargo 4 rhenish_wine, 2 quernstones, 1 ships_bread, 1 ale; hold 660 kg of room
spread 0.20; toll Frankish 0.10, English 0.10, weighed silver 0.00; wage 0.25 d a man a day (4 men); appetite_value 80; stock_value 150; regen 0.05; glut_fade 0.05
made_factor {"0.5": 1.4, "1.0": 0.8, "2.0": 0.65, "3.0": 0.5}; wanted_factor {"1.0": 1.15, "2.0": 1.4, "3.0": 1.7}
famine on (wheat,rye,barley,oats,malt,flour); other traders x1.00, at work 81 days before day one; talks a call 3

                                         perfect    sensible    careless
seasons                                      300         300         300
PAYS 480 d on the day, %                     100          48           0
home on the day, %                           100          99          27
silver on the day: 1 in 10 under             869         377           8
silver on the day: middle                    990         477          46
silver on the day: 1 in 10 over             1129         621          76
the sum paid in 8 seasons of 10              925         410           0
the sum paid in 5 seasons of 10              990         476           0
the sum paid in 2 seasons of 10             1089         580          44
made in the season (middle), d               736         218        -205
made a day, d                                7.5         2.4        -2.0
legs a season                               69.9        46.3        40.5
a leg: days                                  1.4         2.1         1.4
a leg: km                                    188         196         129
made a leg: mean, d                           11           6          -4
made a leg: middle, d                          2          -1          -2
made a leg: 1 in 4 over, d                    14           4          -1
made a leg: 1 in 10 over, d                   36          20           7
legs that lost silver, %                      38          68          79
legs with goods aboard, %                     72          46          72
  their share of the days, %                  72          60          78
  a laden leg: days                          1.4         2.7         1.6
  made a laden leg: mean, d                   16          15          -5
  made a laden leg: middle, d                  7           5          -3
  made a laden leg: 1 in 4 over               22          16           1
  made a laden leg: 1 in 10 over              46          44          10
  laden legs that lost silver, %              14          31          71
made in ten days: middle, d                   64           8         -13
made in ten days: 1 in 4 over, d             101          33          -1
made in ten days: 1 in 10 over, d            148          85          11
made in ten days: 1 in 50 over, d            214         146          42
gain on goods a season, d                    884         375         -69
  tolls paid (already off), d               1010         184         316
  wages, d                                   100         100         100
  bread, d                                    37          37          34
  the reeve and landing dues, d                0           0           0
days under way                               100          98          60
days lying in port for want of a cargo           0           3           0
days with no bread                           0.0         0.0         4.7
tally first reads 480: 1 in 4 by day          12          40         nan
tally first reads 480: middle day             17          52         nan
tally first reads 480: 3 in 4 by day          22          67         nan
tally never reads 480, %                       0          43         100

In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over
  day   0   perfect     240 /  254 /  272   sensible    240 /  254 /  272   careless    240 /  254 /  272
  day  25   perfect     460 /  537 /  618   sensible    351 /  394 /  461   careless     90 /  124 /  196
  day  50   perfect     619 /  732 /  831   sensible    361 /  429 /  533   careless     74 /   95 /  139
  day  75   perfect     759 /  881 / 1002   sensible    383 /  469 /  599   careless     55 /   80 /  104
  day 100   perfect     870 /  992 / 1129   sensible    379 /  478 /  622   careless     32 /   49 /   80

Still short of 480 on the day (% of seasons), and how often the plain pace calls the ending wrongly (%)
  day  25   perfect   short  18, pace wrong   0   sensible  short  94, pace wrong  51   careless  short 100, pace wrong   0
  day  50   perfect   short   0, pace wrong   0   sensible  short  76, pace wrong  41   careless  short 100, pace wrong   0
  day  75   perfect   short   0, pace wrong   0   sensible  short  56, pace wrong  25   careless  short 100, pace wrong   0

perfect: goods by share of the gain: frankish_wine 31% (273 d); rhenish_wine 22% (197 d); books 13% (118 d); glass_vessels 7% (65 d); hounds 5% (48 d); lead 4% (39 d); wheat 3% (28 d); linen 3% (28 d)
perfect: bought at, sold at, by share of the gain: Quentovic to Lundenwic 22%; Quentovic to Sarre 21%; Quentovic to Fordwic 12%; Lundenwic to Quentovic 6%; Dorestat to Ripa 5%; Dorestat to Medemelacha 5%; Sarre to Quentovic 3%; Dorestat to Lundenwic 3%
perfect: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 6.6x 27 d 0.9 days; Sarre to Quentovic 5.8x 4 d 0.8 days; Lundenwic to Quentovic 5.4x 8 d 1.8 days; Quentovic to Lundenwic 5.2x 33 d 1.9 days; Quentovic to Fordwic 3.7x 26 d 0.9 days; Traiectum to Dorestat 3.4x 0 d 0.2 days; Fordwic to Quentovic 3.1x 3 d 0.9 days; Medemelacha to Traiectum 1.7x 1 d 0.9 days

sensible: goods by share of the gain: rhenish_wine 44% (167 d); frankish_wine 33% (122 d); wheat 12% (45 d); glass_vessels 3% (10 d); quernstones 2% (8 d); hounds 2% (7 d); lead 1% (3 d); rye 1% (2 d)
sensible: bought at, sold at, by share of the gain: Quentovic to Lundenwic 19%; Dorestat to Lundenwic 15%; Dorestat to Sarre 15%; Quentovic to Sarre 14%; Dorestat to Ripa 4%; Lundenwic to Medemelacha 4%; Quentovic to Medemelacha 3%; Quentovic to Fordwic 3%
sensible: legs sailed most (times a season, made a leg, days): Traiectum to Dorestat 3.7x -0 d 0.3 days; Quentovic to Sarre 2.4x 20 d 1.4 days; Medemelacha to Traiectum 2.1x -1 d 1.3 days; Sarre to Quentovic 1.9x 1 d 1.2 days; Medemelacha to Daventre 1.7x -1 d 1.1 days; Lundenwic to Quentovic 1.7x 0 d 2.7 days; Quentovic to Lundenwic 1.6x 30 d 3.0 days; Daventre to Traiectum 1.5x -2 d 1.9 days

careless: goods by share of the gain: frankish_wine 56% (60 d); wheat 22% (24 d); rhenish_wine 18% (19 d); woad 2% (2 d); whale_oil 2% (2 d); glass_vessels 0% (0 d); madder 0% (0 d); quernstones 0% (0 d)
careless: goods that lost: linen -7 d; frisian_cloth -31 d; bar_iron -125 d
careless: bought at, sold at, by share of the gain: Quentovic to Sarre 69%; Quentovic to Medemelacha 30%; Medemelacha to Traiectum 1%; Dorestat to Dorestat -0%; Sarre to Traiectum -0%; Doccinga to Medemelacha -0%; Traiectum to Dorestat -0%; Medemelacha to Quentovic -1%
careless: legs sailed most (times a season, made a leg, days): Traiectum to Dorestat 8.2x -1 d 0.3 days; Medemelacha to Traiectum 8.1x -2 d 1.3 days; Quentovic to Sarre 6.2x 6 d 1.3 days; Sarre to Quentovic 6.1x -8 d 1.3 days; Dorestat to Medemelacha 5.4x -17 d 1.6 days; Dorestat to Quentovic 2.2x -5 d 4.5 days; Quentovic to Medemelacha 2.0x 6 d 4.2 days; Dorestat to Traiectum 1.2x -3 d 0.3 days

(900 runs in 85 s)

## 7. How firm the tuned set is (the sensible player, 200 seasons each)
What this script had to guess, moved either way (1 May to Michaelmas)
                                                            pays  home silver on the day  a day     a laden leg    ten days tally reads it  never   short
                                                               %     %  low / mid / high      d  mean / 1 in 10  mid / good   by day (mid)      %  day100
THE TUNED SET                                                 62    98   373 /  514 /  667    1.9      13 /   38     8 /  67             92     31     59%
other traders x0.75 (42 ships, not 56)                        95    98   514 /  733 / 1018    3.5      18 /   58    20 /  98             56      2     19%
other traders x1.25 (70 ships)                                30    98   313 /  425 /  542    1.3      11 /   30     4 /  49             99     62     81%
no talks at all                                               48    99   357 /  477 /  646    1.7       9 /   31     7 /  64             95     42     68%
every price free at each call (the code today)                70   100   397 /  535 /  723    2.1      14 /   39    11 /  69             89     25     52%
he wants 10 % over what he pays, not 15 %                     92    99   513 /  691 /  918    3.1      12 /   44    18 /  88             64      5     16%
he wants 20 % over what he pays                               35    98   317 /  446 /  604    1.4      15 /   41     4 /  55             95     58     78%
the famine off                                                71    99   395 /  555 /  808    2.3      15 /   52     8 /  83             96     27     56%

The dial to turn when another rule is built: the English toll
                                                            pays  home silver on the day  a day     a laden leg    ten days tally reads it  never   short
                                                               %     %  low / mid / high      d  mean / 1 in 10  mid / good   by day (mid)      %  day100
1 May, no reeve, toll 0.10  = THE TUNED SET                   62    98   373 /  514 /  667    1.9      13 /   38     8 /  67             92     31     59%
1 May, no reeve, toll 0.075                                   71    98   400 /  541 /  738    2.1      14 /   43     9 /  71             86     22     50%
1 May, the reeve takes 20 d, toll 0.10                        43   100   332 /  456 /  632    1.6      12 /   33     6 /  58             87     52     71%
1 May, the reeve takes 20 d, toll 0.075                       50   100   343 /  481 /  686    1.7      12 /   39     6 /  62             92     44     70%
1 May, the reeve takes 20 d, toll 0.05                        74    99   371 /  596 /  782    2.4      14 /   52     9 /  80             77     22     42%
21 June, no reeve, toll 0.10                                  48    99   375 /  477 /  622    2.4      15 /   44     9 /  87             51     42       -
21 June, no reeve, toll 0.075                                 58    98   393 /  508 /  652    2.6      15 /   52     8 /  93             51     36       -
21 June, no reeve, toll 0.05                                  76    99   425 /  566 /  728    3.2      17 /   60    13 / 104             43     18       -
21 June, the reeve takes 20 d, toll 0.05                      70    98   406 /  520 /  683    2.8      16 /   58     9 /  91             51     22       -

The perfect player and the law (the tuned set)
                                                            pays  home silver on the day  a day     a laden leg    ten days tally reads it  never   short
                                                               %     %  low / mid / high      d  mean / 1 in 10  mid / good   by day (mid)      %  day100
keeps the law                                                100   100  1124 / 1297 / 1511    7.0      15 /   44    63 / 132             22      0      0%
carries swords, mail and helmets too                         100   100  4046 / 4217 / 4402   26.5     144 /  424   295 / 501             16      0      0%
```
