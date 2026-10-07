# Money that pays

7 October 2026. For the builder, and one question for the user. It answers con 2 of `Design/critique/review_systems.md`
("After the money fix an hour of trading makes eight pennies") and the first half of its change 2 ("Make an hour of
trading pay").

**State of this write-up.** Nothing under `Assets` was changed, and `apply_money_fix.py` was not edited. One new
file beside the soak: `Bake/economy/soak2.py` (the soak with the traders' margin made a rule, the orders added, and
a report of its own). Every number below marked *(soak2)* is from its report, pasted whole at the end: 300 seeded
seasons for each kind of player, 200 or 24 where a table says so. The addition to `apply_money_fix.py` in section 7 was run on a copy of the files outside the project: dry,
applied, and applied again. Unity was not run, no C# of the game was run, no test was run.

**It needs the user's yes.** The cure changes the sum of the debt, and the sum is the user's own (`DECISIONS.md`,
4 October, no. 10: "easy: 480 d by Michaelmas"; ruling 14 of 7 October was built on it). Section 4 says what is
asked.

## 1. The answer

1. **Two numbers, and they go together.** The world's traders stop buying while a quarter is still in the gap:
   `SimRules.minMargin` 0.12 to **0.25**. The count is owed **eight pounds by Michaelmas**: `start.debt` 480 to
   **1,920**. Nothing else changes: not a price, not a toll, not the spread, not wages, not the day.
2. **What it buys** *(soak2, sensible player, 1 May to Michaelmas)*:

   | | Approved (ruling 14) | Proposed |
   |---|---|---|
   | A laden leg clears, mean | 13 d | **27 d** |
   | A good laden leg (one in ten) | 38 d | **73 d** |
   | Laden legs that lose silver | 29 in 100 | **3 in 100** |
   | Legs sailed with goods aboard | 45 in 100 | **84 in 100** |
   | An hour at speed 2 (ten game days), middle | 8 d: 1.7 % of the debt | **106 d: 5.5 % of the debt** |
   | The same hour, a good one (one in four) | 30 d | **162 d** |
   | Pays the debt on the day | 65 of 100 | **68 of 100** |
   | The tally first reads the debt, middle season | day 91 | **day 124** of 151 |
   | Still short on day 100 | 58 of 100 | **99 of 100** |
   | The careless player pays | 0 of 100 | **0 of 100** |
   | The perfect player first holds the debt | day 22 | **day 100** |
   | The perfect player's silver on the day | 2.7 debts | **1.3 debts** |

3. **Why 0.12 made the trade not pay.** The sensible player wants 15 % over what he pays. The others kept buying
   until under 14 % was left. So every gap was closed below his own mark before he came, and he lived on leavings: the count's casks and
   stale news. It was never a matter of small prices; it was whose mark was lower (section 2).
4. **Eight pounds, not eight and a half.** The bare model pays eight pounds in 68 seasons of 100 and eight and a
   half in 45. But the bare model has no rounding of coins, and the game has (the critic's con 3). With a denier
   lost at every call, which stands for it, eight pounds is paid in 52 of 100; with the reeve's 20 d at Lundenwic
   and no rounding, in 50; with both, in 36. So eight pounds is "about half" for the game as the code stands, and
   seven and a half pounds (1,800 d) is the sum once the reeve is built too. Half a pound is about twenty seasons
   in a hundred: that is the dial (section 8).
5. **The cap on cards needs no change, and now means what the critic asked.** 60 d was "a good ten days" in the
   approved game. It is now under one good laden leg (73 d) and about two ordinary ones. The cap and the toll on
   orders are `Design/fixes/orders.md` (premium at most 0.5, at most 60 d over the market, tolled): the same
   numbers hold here, and the soak says what happens without them (section 6).
6. **How sure.** Of the direction and the size, fairly: about four times the silver on the day, and a debt of seven
   to nine pounds. Of the exact pound, no better than a coin toss until a C# run says so, because the season's sum
   climbs steeply with the margin (about 170 d for each 0.01 between 0.20 and 0.25) and the model is a Python copy.
   The rule for settling it is in section 8.
7. **The five-speed clock changes no sum in wages or bread.** It changes what a day's keep is paid in: silver
   only, no longer the player's minutes. And with this proposal the keep falls from two fifths of the gain on goods
   to a tenth (section 9).

## 2. Why the middle hour made 8 d, and what "an hour" is on the new clock

**The hour.** The critic's hour was "ten game days at 4x", when the fastest clock was a game day in six real
minutes. Ruling 26 keeps that pace as **speed 2**. So his hour is an hour at speed 2, and that is what this file
means by it. At speed 3 the same real hour holds 60 game days, at speed 4 the whole season to Michaelmas runs in 38
minutes. A faster clock therefore mends the sum for an hour by itself (the approved game makes about 110 d in an
hour at speed 3, at its 1.9 d a day) and mends nothing that matters: the player still makes one decision a leg, and
the leg still clears 13 d. The measure that holds at every speed is **silver for a leg**, and beside it silver for
ten game days.

**The mechanism.** `TraderBrain.Plan` (`Assets/Kapman/Code/Game/Sim/TraderBrain.cs`) weighs a good for a port when
what she believes it fetches there, after the toll, is more than `(1 + minMargin)` times what the market asks here
(line 142; `Ballast` has the same test at 253). Then she buys it five at a time and stops when the price asked has
risen to within `minMargin` of what it should fetch (line 208: `Asks < sells * (1 - minMargin)`). Each lot she buys
makes the next dearer here, and each lot landed makes the next cheaper there. So on every way that pays at all, the
56 traders of these waters press the gap down to their stopping mark, wherever their silver and their numbers
reach. At 0.12 that mark is 13.6 % over the asking price. At 0.25 it is 33 %.

The sensible player in the soak (and any player who has been told to want a margin) buys only what should fetch
15 % over what he pays. With the others stopping at 13.6 % nothing passes his test but mistakes: a price that moved
since the last ship brought word, a season turning, the famine. That is the approved game: 45 legs in 100 with
goods aboard, 29 laden legs in 100 at a loss, 8 d in the middle ten days. Money.md saw the two ends of it ("he asks
10 %: pays 92 of 100; he asks 20 %: 35") and read it as the player's keenness. It is the distance between two
marks. Move the traders' mark above the player's and the legs fill:

| The traders' least margin | Half the seasons pay | Made a day | Laden legs, of all legs | A laden leg: mean / middle / 1 in 4 / 1 in 10 | Laden legs that lose | Ten days: middle / 1 in 4 / 1 in 10 |
|---|---|---|---|---|---|---|
| 0.12 (today) | 511 d (2.1 pounds) | 1.9 d | 32 of 72 | 13 / 6 / 17 / 38 d | 30 % | 8 / 30 / 67 d |
| 0.15 | 595 d (2.5) | 2.6 d | 43 of 77 | 11 / 5 / 14 / 33 d | 27 % | 14 / 39 / 79 d |
| 0.18 | 856 d (3.6) | 4.3 d | 53 of 80 | 14 / 7 / 18 / 38 d | 15 % | 32 / 68 / 107 d |
| 0.20 | 1,167 d (4.9) | 6.3 d | 59 of 82 | 18 / 9 / 23 / 47 d | 8 % | 53 / 95 / 137 d |
| 0.22 | 1,502 d (6.3) | 8.4 d | 63 of 84 | 21 / 11 / 29 / 58 d | 5 % | 75 / 124 / 171 d |
| **0.25** | **2,019 d (8.4)** | **11.8 d** | **69 of 82** | **27 / 15 / 38 / 73 d** | **3 %** | **106 / 161 / 224 d** |
| 0.30 | 2,359 d (9.8) | 14.3 d | 72 of 84 | 31 / 19 / 47 / 78 d | 3 % | 133 / 185 / 250 d |
| 0.35 | 2,804 d (11.7) | 17.1 d | 71 of 82 | 37 / 25 / 60 / 91 d | 2 % | 170 / 218 / 270 d |

*(soak2, the sensible player, 200 seasons each, all else as approved. "Half the seasons pay" is the silver he
holds at home on Michaelmas in the middle season: the debt that would be paid one season in two.)*

Two things to read in it. Up to 0.15 little moves, because the player's own mark is 15 %. Above it the season's sum
climbs fast and nearly straight: the gain on a hold is about the margin times the silver laid out, and nearly every
leg is now laden. And a leg does not grow as fast as the season: at 0.25 a laden leg is twice what it was and there
are twice as many of them. The boat is small (660 kg of room: four casks of wine, 220 to 270 d laid out), so a
quarter on a hold of wine is 55 to 70 d, less what his own buying and selling move the price.

What the first morning looks like, mean of 24 seasons *(soak2, section 1 of the output)*:

| Bought at, sold at (asked there, paid here after the toll) | Approved, first morning | Proposed, first morning | Proposed, mean of the season |
|---|---|---|---|
| Rhenish wine, Dorestat to Lundenwic | 60 > 59: -1 d a cask | 50 > 62: **+12** | 56 > 71: +15 |
| Rhenish wine, Dorestat to Sarre | 60 > 57: -3 | 50 > 60: **+10** | 56 > 68: +12 |
| Rhenish wine, Dorestat to Ripa | 60 > 62: +2 | 50 > 68: **+18** | 56 > 78: +22 |
| Frankish wine, Quentovic to Lundenwic | 46 > 57: +11 | 34 > 65: **+31** | 39 > 70: +30 |
| Frisian cloth, Medemelacha to Quentovic | 40 > 41: 0 | 36 > 40: **+5** | 37 > 43: +6 |
| Honey, Lundenwic to Medemelacha | 27 > 27: 0 | 25 > 32: **+7** | 22 > 28: +5 |

Money.md's line "Dorestat asks 60 d and Lundenwic pays 59 d. Nothing." becomes "asks 50, pays 62": the first
crossing, with the count's four casks and four more bought, is a crossing worth making.

**What it does to the world's own traders** *(soak2, no player, 24 seasons)*. At 0.12 each of the 56 lays out
1,595 d on goods in the season and ends it poorer than she began (279 d against 301 d): the margins she works for
do not cover her crew. At 0.25 she lays out 797 d and ends at 430 d. So the traders carry half the goods and make a
living, which is what the critic's pro 4 ("the rivals have purses, wages and bad days") needs if they are to last
ten years. The price: 41 sailings in 100 are laden, where 76 were. More of the sails the lookout makes out will be
"in ballast". That is a change the player will see (section 10).

## 3. The targets, and why these

| # | Target | Why this and not another | Proposed set *(soak2)* |
|---|---|---|---|
| 1 | **A sensible laden leg clears 25 d in the mean and 60 d one time in ten; fewer than one in ten lose.** | (a) It is the middle of the book's own scale (`CHAINS.md`: 10 to 40 d a leg). (b) A laden leg is two to three days: 3 to 4 d of pay and bread. Of what the cargo made on a 13 d leg the keep took a fifth; on a 27 d leg it takes a tenth: the cargo decides, not the crew's bread. (c) The frame rounds every coin against the player (the critic's con 3: about a denier a click and a denier a call). On four casks that is 4 to 5 d: two fifths of a 13 d leg, a sixth of a 27 d one. (d) What the player is told of a far price is a word, and the word turns at 15 % (`knowing_793.json` line 9: `pays_well` 1.15, `pays_poorly` 0.85). A gap pressed down to 14 % is narrower than the word's step, so "the usual" there can hide the whole gain; a gap left at a quarter and more is wider, so "pays well" is a thing to sail on. (e) 60 d for the good leg, so that ruling 14's "no card pays over 60 d" reads "no card pays more than a good leg". | 27 d; 73 d; 3 in 100 |
| 2 | **An hour at speed 2 moves the tally by a twentieth of the debt in the middle hour, and by a twelfth in a good one.** | The top bar shows "Owed N. In hand M." A move under a fiftieth of the debt (8 of 480) cannot be seen as progress. And a twentieth is close to the most there can be: from 1 May the debt has 15 such hours, so if it is to be in doubt at the end the mean hour cannot be more than a fifteenth of it, and less for what she starts with. | 106 d of 1,920: 5.5 %; a good hour 162 d: 8.4 % |
| 3 | **The debt is paid in 40 to 70 seasons of 100; on day 100 nobody has it in hand; on day 125 more than half are still short; the plain pace calls the ending wrongly one time in five on day 100.** | 40 to 70 is the book's own test (`GAME_MAKER.md` stage 1, step 11, as money.md has it). The rest is "in doubt until late", made countable. | 68; 99 short; 63 short; wrong 25 in 100 |
| 4 | **The careless player never pays.** | He sells into gluts he does not look at and is away on the day. | 0 of 100; 104 d on the day; at home one season in five |
| 5 | **The perfect player does not hold the debt before day 90 and ends under one and a half debts.** | "Does not run away": the one who knows every price this hour still needs three fifths of the season, and cannot pay the debt and buy the next hull in the same summer. | day 100; 2,528 d: 1.3 debts |

## 4. The numbers that change

| # | Number | Where | Before | After |
|---|---|---|---|---|
| 1 | The least margin the world's traders carry for | `Assets/Kapman/Code/Game/Sim/WorldSim.cs` line 16, `public float minMargin = 0.12f;` (read at `TraderBrain.cs` 142, 208, 253) | 0.12 | **0.25** |
| 2 | The debt | `Assets/Kapman/Game/Data/trade_rules.json` line 59, `"debt": 480, "_debt": "deniers owed (two pounds of silver) ..."` | 480 d, two pounds | **1,920 d, eight pounds** |

They go together or not at all. The margin without the debt: 480 d is in hand by day 11 and the debt is no stake
again (the tally reads four pounds, 960 d, by day 43 in the middle season). The debt without the margin: the
sensible player ends with 521 d, the perfect one with 1,312 d; nobody pays.

**If eight pounds is more than the story will bear,** the same cure at half strength is the margin 0.20 and five
pounds (1,200 d, paid in 45 seasons of 100; the tally first reads it on day 112). It gives a laden leg of 18 d in
the mean and 47 d one time in ten, 8 in 100 at a loss, and a middle hour at speed 2 of 53 d (4.4 % of the debt). It
is two thirds of the way and misses target 1 of section 3. The row is in the table of section 2.

**Not changed:** every price in `goods_793.json`; the spread (0.20); the tolls (Frankish 0.10, English 0.10 as
approved, Ripa none); wages (0.25 d a man a day) and bread; the purse and the cargo at the start; Michaelmas; the
60 d a card and 240 d a chain of ruling 14; the three parts of the approved fix (the world from 1 April, the famine
on grain only, the English tenth), which this stands on.

**What the 60 d and the 240 d now are.** 60 d was a good ten days (67 d, one in ten); it is now under a good laden
leg (73 d) and about two ordinary ones (27 d). 240 d was half the debt; it is now an eighth of it, about two
middling hours at speed 2. Both stand. Nothing in `cap_payouts.py` needs to move.

**What is asked of the user.** One yes or no:

> The debt to the count becomes eight pounds of silver (1,920 d) by Michaelmas, in place of two pounds (480 d), so
> that a laden leg can clear about 27 d and an hour's trading about 100 d and the debt still be in doubt in
> September. Your number of 4 October (no. 10) was 480 d. Yes, or keep 480 d and thin legs?

Three things that follow from a yes, and are the user's to word or to rule:

- **The story of the sum.** `ships_roster.md` prices the six-oar boat at 600 d, and a note in the cards has "two
  pounds for a small boat is the game's price" (`encounters/northumbria_irish_sea_793.json` line 1283). Two pounds
  was the boat. Eight pounds is three or four boats: the stick at the dam head (`chain_house_debt` s01: "480
  pennies lent for the boat") then has to be for more than the boat. The father's debts, the boat and her first
  lading, or a hard count: yours to write.
- **The other terms.** No. 10 has "hard: 20 pounds". In this model nobody holds 20 pounds by Michaelmas (the
  perfect player ends with ten and a half), and nobody did before either. By the pace of the first season, 20
  pounds is about what the perfect player would hold by Michaelmas of the *second* year; that was not run. The table
  in section 5 gives each sum's meaning, if terms by the pound are wanted (seven pounds: 94 of 100; nine: 19).
- **The ships.** `ships_roster.md` calls a coaster (1,200 d) "two and a half debts, six good voyages". Six good
  voyages at 200 d is the proposed game (a good ten days is 226 d), not the approved one (67 d). So the roster's
  prices already fit this scale. Against the new debt a coaster is five eighths of it: after the debt is paid, one
  more summer at the first one's pace buys her. Under the approved numbers it was four summers.

## 5. What the model says

### Before and after, the three players

*(soak2, 300 seasons each, 1 May to Michaelmas. Before: the approved set, the debt 480 d. After: the margin 0.25,
the debt 1,920 d. The "before" column is money.md's "after" to the last digit: soak2 with the margin at 0.12 and the
orders off is soak.py.)*

| | Perfect, before | Perfect, after | Sensible, before | Sensible, after | Careless, before | Careless, after |
|---|---|---|---|---|---|---|
| Pays the debt on the day, of 100 | 100 | 100 | 65 | **68** | 0 | 0 |
| At home on the day, of 100 | 100 | 100 | 98 | 99 | 16 | 20 |
| Silver on the day, middle | 1,312 | 2,528 | 521 | **2,026** | 0 | 104 |
| The same, in debts | 2.7 | 1.3 | 1.1 | 1.06 | 0 | 0.05 |
| Silver, one in ten under / over | 1,120 / 1,481 | 2,268 / 2,776 | 360 / 701 | 1,785 / 2,215 | 0 / 54 | 66 / 140 |
| Made a day, after wages and bread | 7.1 | 15.3 | 1.9 | **11.9** | -1.7 | -0.7 |
| Legs a season; with goods aboard | 103; 73 % | 109; 78 % | 72; 45 % | 82; **84 %** | 53; 55 % | 66; 90 % |
| A laden leg: mean / middle / 1 in 4 / 1 in 10 | 16 / 7 / 22 / 45 | 28 / 16 / 44 / 74 | 13 / 6 / 17 / 38 | **27 / 15 / 38 / 73** | -5 / -2 / 0 / 8 | -1 / -2 / 7 / 19 |
| Laden legs that lose silver, of 100 | 13 | 10 | 29 | **3** | 74 | 59 |
| Ten days: middle / 1 in 4 / 1 in 10 / 1 in 50 | 64 / 97 / 133 / 191 | 141 / 197 / 264 / 411 | 8 / 30 / 67 / 117 | **106 / 162 / 226 / 318** | -10 / -7 / 2 / 23 | -9 / 13 / 42 / 146 |
| The middle ten days, as a share of the debt | 13 % | 7 % | 1.7 % | **5.5 %** | | |
| Tally first reads the debt: 1 in 4 / middle / 3 in 4 | day 16 / 22 / 30 | day 93 / 100 / 109 | day 61 / 91 / 107 | day **115 / 124 / 134** | never | never |
| Tally never reads the debt, of 100 | 0 | 0 | 29 | 30 | 100 | 100 |
| Still short on day 100; on day 125, of 100 | 0; 0 | 50; 6 | 58; 40 | **99; 63** | 100; 100 | 100; 100 |
| The plain pace wrong on day 100, of 100 | 0 | 0 | 22 | 25 | 0 | 0 |
| Gain on goods / tolls paid / wages / bread | 1,280 / 1,522 / 151 / 55 | 2,521 / 2,130 / 151 / 55 | 500 / 263 / 151 / 55 | 2,004 / 1,091 / 151 / 56 | -69 / 260 / 151 / 37 | 97 / 861 / 151 / 53 |

Read against the targets of section 3: all five are met. The perfect player is 25 % ahead of the sensible one, where
he was 150 % ahead: the best that can be known is worth a quarter more, and what is worth twenty times more is
looking at the glut before selling into it (the careless player: 104 d).

**What the sensible player carries** *(after)*: Frankish wine 40 % of the gain, Rhenish wine 12 %, glass 10 %,
hounds 6 %, lead 5 %, linen 5 %, wheat 4 %, woad 3 %. **This is the proposal's weak side: three fifths of the gain
is bought at Quentovic** (sold at Lundenwic 28 %, Sarre 20 %, Medemelacha 8 %, Fordwic 7 %: 63 %, where it was
45 %). Frankish wine alone is 40 % (it was 37 %). It is money.md's leak 4, not mended and by the ways made larger: a
player who finds the Quentovic run has found most of the game. The lever money.md tried is Frankish wine made 2 at
Quentovic and wanted 2 in England; at the proposed margin half the seasons then pay 1,564 d (six and a half
pounds), a laden leg clears 21 d and a good one 51 d (section 11, the third single row). That costs a quarter of
the season and target 1, so it is not in the proposed set; a better mend is more goods worth carrying from the
other eleven ports, which is content.

### The tally sentence through the season

"Owed 1,920. In hand and in the hold ..." *(soak2, sensible, one in ten under / middle / one in ten over)*:

| Day | 0 | 25 | 50 | 75 | 100 | 125 | 151 |
|---|---|---|---|---|---|---|---|
| Proposed | 206 / 213 / 223 | 595 / 740 / 866 | 832 / 1,062 / 1,202 | 1,115 / 1,351 / 1,510 | 1,377 / 1,605 / 1,796 | 1,631 / 1,874 / 2,063 | 1,786 / 2,027 / 2,217 |
| Approved (owed 480) | 217 / 232 / 246 | 303 / 345 / 422 | 312 / 384 / 469 | 325 / 415 / 526 | 351 / 463 / 608 | 374 / 504 / 663 | 361 / 522 / 702 |

The pace the debt asks from the first morning is (1,920 - 213) / 151 = 11.3 d a day; the sensible player makes
11.9. The first 25 days run at twice that (the count's casks sold abroad, and the gaps of a world five weeks old);
after day 25 the middle season makes 100 to 130 d in each ten days until the last fortnight, which goes on the way
home. The line "at this pace" is wrong about the ending in 31 seasons of 100 on day 25 and still in 22 on day 125.

### The lord's terms: what each sum would mean at the margin 0.25

*(soak2, the same 300 seasons; the players do not look at the debt, so one run answers for every sum.)* The sum the
sensible player holds at home on the day in 9 seasons of 10 is 1,761 d; in 8, 1,861; in 2 of 3, 1,925; in half,
2,023; in 1 of 3, 2,106; in 2 of 10, 2,157; in 1 of 10, 2,217.

| The debt | Sensible pays, of 100 | Tally first reads it: 1 in 4 / middle day | Never, of 100 | Short on day 100 / 125, of 100 | Pace wrong on day 75 / 100 / 125 | Perfect pays | Perfect first holds it | Careless pays |
|---|---|---|---|---|---|---|---|---|
| 4 pounds (960 d) | 99 | 36 / 43 | 0 | 0 / 0 | 1 / 1 / 1 | 100 | day 32 | 0 |
| 5 pounds (1,200 d) | 99 | 56 / 62 | 0 | 1 / 0 | 1 / 1 / 1 | 100 | day 47 | 0 |
| 6 pounds (1,440 d) | 99 | 76 / 85 | 0 | 17 / 2 | 1 / 1 / 1 | 100 | day 65 | 0 |
| 7 pounds (1,680 d) | 94 | 97 / 106 | 5 | 68 / 16 | 5 / 5 / 5 | 100 | day 84 | 0 |
| 7.5 pounds (1,800 d) | 87 | 107 / 115 | 11 | 90 / 37 | 11 / 10 / 9 | 100 | day 92 | 0 |
| **8 pounds (1,920 d)** | **68** | **115 / 124** | **30** | **99 / 63** | **26 / 25 / 22** | **100** | **day 100** | **0** |
| 8.5 pounds (2,040 d) | 45 | 124 / 132 | 53 | 100 / 87 | 44 / 41 / 31 | 100 | day 109 | 0 |
| 9 pounds (2,160 d) | 19 | 128 / 138 | 79 | 100 / 97 | 64 / 53 / 39 | 96 | day 118 | 0 |
| 10 pounds (2,400 d) | 0 | - | 100 | 100 / 100 | 62 / 34 / 16 | 75 | day 135 | 0 |
| 12 pounds (2,880 d) | 0 | - | 100 | 100 / 100 | 7 / 1 / 0 | 3 | day 144 | 0 |
| *Approved: 2 pounds (480 d) at 0.12* | *65* | *61 / 91* | *29* | *58 / 40* | *27 / 22 / 13* | *100* | *day 22* | *0* |

The sums lie close: from "nearly always" to "one in five" is two pounds. That is because the proposed game pays
steadily (one in ten under and over are 12 % either side of the middle, where they were 30 %). It makes the pound a
fine dial, and it means the sum must be set from a measured run, not from this page alone (section 8).

## 6. Orders and cards: the doors that pay more than trade

The rules for orders are written and compiled in `Design/fixes/orders.md` (an order pays the market's price plus at
most a half, at most 60 d over the market in all, and is tolled; `premium_max` and `bonus_max_d` in
`commissions_793.json`). This file does not repeat them. What the soak adds:

| The orders | Posted in the season | Seen at a call | Too big for her | Taken | Delivered | Let run out | Gain, d | Hoped for, d | Half the seasons pay |
|---|---|---|---|---|---|---|---|---|---|
| As the code pays them today: no toll, the table's premiums (0.25 to 0.4) | 10.5 | 4.0 | 0.1 | 0.7 | 0.2 | 0.5 | 15 | 58 | 2,019 d |
| As orders.md rules them: tolled, at most 0.5, at most 60 d over the market | 10.4 | 4.1 | 0.2 | 0.5 | 0.0 | 0.4 | 2 | 31 | 2,020 d |
| What if: every posted order at the cards' premium of 1.5, no toll | 10.6 | 3.9 | 0.1 | 2.6 | 1.7 | 0.8 | 374 | 498 | 2,270 d |
| The same what-if, as orders.md rules them | 10.4 | 4.0 | 0.2 | 0.5 | 0.1 | 0.4 | 4 | 33 | 2,027 d |

*(soak2, the sensible player, 200 seasons each, the proposed set. "Hoped for" is what the orders he took should have
cleared when he took them. The soak's player is poor at keeping an order: he takes it where it is posted, and goes
for the goods only when that leg beats his trade; most he lets run out. So "gain" is a floor and "hoped for" a
ceiling. Of orders.md the soak has the cap and the toll only, not the parts and not the usual-price ceiling.)*

- **The table's own orders hardly touch the money in the twelve ports.** About ten are posted in a season at the
  eight ports whose kind posts any (six emporia, two towns; the three markets and the village post none). The boat
  sees four of them at her calls and less than one is worth taking. An order is priced from the port's own market,
  and the lists mostly name goods that market prices low: an emporium asks for amber, honey, cloth or beads, which
  many of them neither make nor want (the 0.6 of `unwanted`), so a quarter over that price is under what the goods
  cost where they are made; a town asks for wine in the wine country, and for wax and pepper, which none of the
  twelve ports makes. They neither break the debt nor help it. They are also not yet play: for the first release
  the lists in `commissions_793.json` want goods the twelve ports can bring to a place that wants them (a content
  matter, not a number here).
- **The cards' orders would undo this fix as they undid the last.** 126 of the cards' 220 orders ask a premium over
  0.5, 86 ask 1.0 or more, 49 ask 1.5 or more (my count; orders.md has the same). The what-if row gives every posted
  order the cards' 1.5 and no toll: the 1.7 orders he keeps bring 374 d (220 d each: eight ordinary laden legs) and
  the middle season rises by 250 d; had he kept all 2.6 he took, about 500 d. And those are orders for goods the
  ports do not much want. One for a good they do want, at Sarre, four casks, by the season's mean prices of the
  proposed game:
  the market pays 76 d a cask before the toll, so 4 x 76 x 2.5 = 760 d untolled, for casks that cost 4 x 56 = 224 d at
  Dorestat: 536 d on one leg, twenty ordinary laden legs, more than a quarter of the eight pounds.
- **With orders.md's two numbers the same order is a good leg and no more.** 4 x 76 + 60 = 364 d, less the tenth:
  328 d in the purse, about 100 d over the casks' cost. A plain sale of the four at Sarre clears about 45 d. So an
  order is worth one good laden leg over the market, which is what 60 d now is. The cap needs no other number for
  this scale. It will want raising when the ships grow: 60 d on a keel's forty casks is nothing.

## 7. The exact edits

### 7.1 The addition to `Bake/economy/apply_money_fix.py`

Not applied by me. Insert the block below **before line 60** of the script as it stands today, the line that begins
`print("MONEY FIX (data): %s" % (...`, and after the blank line 59 that ends the English toll's part. The four lines
after it (the report and `sys.exit`) stay as they are. It uses only what the script already has (`io`, `json`, `os`,
`re`, `sys`, `ROOT`, `DATA`, `APPLY`, `problems`, `changes`).

Also add to the script's docstring, after its point 5 (they are numbered on from it), the two lines:

```text
  6. the world's traders leave a quarter: SimRules.minMargin 0.25   (Assets/Kapman/Code/Game/Sim/WorldSim.cs)
  7. the count is owed eight pounds: start.debt 1920                (Assets/Kapman/Game/Data/trade_rules.json)
```

```python
# ---------------------------------------------------------------------------------------------------------------
# MONEY THAT PAYS (Design/fixes/money_that_pays.md). NEEDS THE USER'S YES: it changes the 480 d of DECISIONS no. 10.
#   6. the world's traders leave a quarter: SimRules.minMargin 0.12 -> 0.25   (Assets/Kapman/Code/Game/Sim/WorldSim.cs)
#   7. the count is owed eight pounds: start.debt 480 -> 1920                 (Assets/Kapman/Game/Data/trade_rules.json)
# The two go together or not at all: with the margin and not the debt the 480 d is in hand by day 11; with the debt
# and not the margin nobody can pay it. It also LISTS, and does not change, every line of the cards and their words that
# names the old sum. Add --no-pays to leave both out and apply only the approved fix above.
# (The cap and the toll on orders are not here: they are Design/fixes/orders.md, in commissions_793.json.)
MARGIN, DEBT, POUNDS = "0.25", 1920, "eight"
if "--no-pays" not in sys.argv:
    import glob
    todo = []                                                   # (path, new text): written together, last, if nothing is wrong

    p = os.path.join(ROOT, "Assets", "Kapman", "Code", "Game", "Sim", "WorldSim.cs")
    t = io.open(p, encoding="utf-8", newline="").read()
    m = re.search(r'(public float minMargin = )([0-9.]+)(f;)', t)
    if not m:
        problems.append("WorldSim.cs: 'public float minMargin = ...f;' was not found")
    elif abs(float(m.group(2)) - float(MARGIN)) < 1e-9:
        changes.append("the traders' least margin is already %s" % MARGIN)
    else:
        changes.append("SimRules.minMargin (WorldSim.cs): %s -> %s" % (m.group(2), MARGIN))
        todo.append((p, t[:m.start(2)] + MARGIN + t[m.end(2):]))

    p = os.path.join(DATA, "trade_rules.json")
    t = io.open(p, encoding="utf-8", newline="").read()         # read again: the toll above may just have been written
    old_debt = None
    m = re.search(r'("debt": )(\d+)(\s*,)', t)
    if not m:
        problems.append("trade_rules.json: start.debt was not found")
    else:
        old_debt = int(m.group(2))
        if old_debt == DEBT:
            changes.append("the debt is already %d d" % DEBT)
        else:
            changes.append("the debt: %d -> %d d (%s pounds)" % (old_debt, DEBT, POUNDS))
            t2 = t[:m.start(2)] + str(DEBT) + t[m.end(2):]
            t2 = re.sub(r'(deniers owed \()[a-z]+( pounds of silver\))', lambda k: k.group(1) + POUNDS + k.group(2), t2, count=1)
            try:
                if json.loads(t2.lstrip(chr(0xFEFF)))["start"]["debt"] != DEBT:
                    problems.append("trade_rules.json: the changed text does not read back as meant")
            except ValueError as e:
                problems.append("trade_rules.json: the changed text is not JSON: %s" % e)
            todo.append((p, t2))

    # the cards and their words that still name the old sum (listed, never changed: a rewrite, done with the pay cap)
    if old_debt is not None and old_debt != DEBT:
        by_weight = int(round(old_debt * 1.3 / 1.7))            # 367: the old sum paid by weight in chain_house_debt s05
        pat = re.compile(r'\b(%d|%d)\b|two pounds|four hundred and eighty\b' % (old_debt, by_weight))
        named, total = [], 0
        files = sorted(glob.glob(os.path.join(DATA, "encounters", "*.json"))) + sorted(glob.glob(os.path.join(DATA, "writing", "*.json")))
        files.append(os.path.join(DATA, "writing.json"))
        for f in files:
            if not os.path.exists(f):
                continue
            n = sum(1 for line in io.open(f, encoding="utf-8-sig") if pat.search(line))
            if n:
                named.append("%s %d" % (os.path.relpath(f, DATA).replace(os.sep, "/"), n))
                total += n
        if total:
            changes.append("STILL TO DO BY HAND: %d lines of the cards and their words name %d, %d (the old sum by weight) or 'two pounds'; "
                           "those that mean the count's debt must change with it (not all do: read them): %s" % (total, old_debt, by_weight, "; ".join(named)))

    if APPLY and not problems:
        for path, text in todo:
            io.open(path, "w", encoding="utf-8", newline="").write(text)
```

What it does, as run on a copy of the files (the script with the block in, `ROOT` a copy outside the project):

```text
> apply_money_fix.py
MONEY FIX (data): checked only (add --apply)
  famine of 793: goods food -> wheat,rye,barley,oats,malt,flour
  English toll: 0.05 -> 0.10
  SimRules.minMargin (WorldSim.cs): 0.12 -> 0.25
  the debt: 480 -> 1920 d (eight pounds)
  STILL TO DO BY HAND: 79 lines of the cards and their words name 480, 367 (the old sum by weight) or 'two pounds'; those that mean the count's debt must change with it (not all do: read them): encounters/chain_dirham_hoard_793.json 1; encounters/chain_house_debt_793.json 22; ...
> apply_money_fix.py --apply          (the same lines under "APPLIED"; besides the famine line in shocks_793.json, the files then differ from the project by exactly:)
  trade_rules.json  15: "toll": 0.05  ->  "toll": 0.10                       (the approved part)
  trade_rules.json  59: "debt": 480, "_debt": "deniers owed (two pounds of silver) ..."  ->  "debt": 1920, "_debt": "deniers owed (eight pounds of silver) ..."
  WorldSim.cs       16: public float minMargin = 0.12f;  ->  public float minMargin = 0.25f;
> apply_money_fix.py --apply          (again)
  the traders' least margin is already 0.25
  the debt is already 1920 d
> apply_money_fix.py --no-pays        (only the approved fix: the famine and the toll)
```

A dry run leaves every file byte for byte as it was (checked with `cmp`). The block writes its two files together
and last, and only if nothing was wrong. The script's two older parts write as they go, as before.

It reads `"debt": <digits>,` wherever it stands in `trade_rules.json`, so it does not matter whether the debt-day
edit of `Design/fixes/debt_day.md` (2a) or the pre-run's `world_from` (`Design/fixes/prerun.md`) went into the
"start" block first.

### 7.2 The sum in the cards (not done by the script: it is a rewrite, to go with the pay cap)

The game would say two sums if only the table changed. The count's own chain names the debt in its effects and its
words. `Assets/Kapman/Game/Data/encounters/chain_house_debt_793.json` as it stood at about 12:30 (the content pass is
still at work on the cards: run the script again when it lands, the list prints fresh):

| Lines | What | Now | To |
|---|---|---|---|
| 28, 34, 39 | `flag ch_house_debt_owed = 480` | 480 | 1920 |
| 114, 121 | `"check": "silver >= 480 and ..."` (s03, on the day and late) | 480 | 1920 |
| 115, 122 | `"cost": "silver 480"` | 480 | 1920 |
| 218, 267 | `"check": "silver >= 480"` (s05 by tale, s06) | 480 | 1920 |
| 219, 268 | `"cost": "silver 480"` | 480 | 1920 |
| 223, 316 | `"check": "silver >= 367 ..."` (s05 and s07, by weight: the sum times 1.3 / 1.7, the old penny against the new) | 367 | 1468 |
| 224, 317 | `"cost": "silver 367"` | 367 | 1468 |
| 38, 114, 121, 218, 223, 267, 316 | the labels: "two pounds", "480 of the new pennies", "367 new pennies" | | "eight pounds", "1,920", "1,468" |
| 2, 6, 18, 138, 139, 194 | notes and the intention ("480 pennies lent for the boat") | | the same |

Their words: `Assets/Kapman/Game/Data/writing/chain_house_debt.json` lines 5, 13, 37, 38, 66, 67, 81, 94, and the
same six texts in `writing.json` (lines 6750, 6782, 6870, 6874, 6978, 7034).

The checks could read the sum from the game and not carry a number: the cards' language already has `debt`
(`Assets/Kapman/Code/Core/Encounters/Cond.cs` line 30; `EncounterGameVars.cs` 53-54), so `"check": "silver >= debt"`
is true to whatever the table says. The costs and the words cannot, as far as I read.

Of the other 48 lines the script lists, these are the count's debt too: the note in `encounters/crew_793.json` line
127, the note in `encounters/npc_roles_793.json` line 15, and the text "a debt of two pounds to a count" in
`writing.json` line 34522. The rest that I read are other sums and stay: a bride-price
(`chain_house_marriage_793.json` 514-515), another man's goods (`chain_reric_808_793.json` 559-560), "the two
pounds the letter left" (`northumbria_irish_sea_793.json` 432-434), loaves "of two pounds each", and rewards of
`silver +480`, which are the pay cap's. I did not read every one.

The chain also pays `silver +466`, `+300`, `+240`, `+200`: those are the pay cap's (ruling 14), not this file's.

### 7.3 `DECISIONS.md`

If the user says yes, one new item under 7 October, in the lead's words; mine would be: "**The count is owed eight
pounds, and the trade pays** (the user, 7 Oct, to `Design/fixes/money_that_pays.md`). The world's traders leave a
quarter (`SimRules.minMargin` 0.25); the debt is 1,920 d by Michaelmas. This corrects the 480 d of no. 10 of
4 October and of ruling 14." And `QUEUE.md` part 1, "The debt is no stake": add this file beside money.md.

### 7.4 Tests

- `EconomyTests` after the script, as the script's own header says.
- `SimTests.TradersSailAndDeal` (`Assets/Kapman/Tests/EditMode/SimTests.cs` 25-33) asks for more than 100 deals in
  40 summer days. The soak has the traders dealing about half as much at 0.25. With 138 ships I expect it to hold;
  I could not run it.
- **Run by name, once, before trusting the number: `SimTests.TestRunTenYears`** (line 71, Explicit, a few
  minutes). It fails if any trader stops sailing or a watched good's price drifts more than twofold in ten years.
  A wider margin moves both. If it fails on drift, say so before changing anything: the test's twofold was set for
  the old margin.
- New, and the one that matters: the bot's hour of money.md 11.7, in C#. Pass mark as there: the plain trader pays
  the debt in 40 to 70 runs of 100. Section 8 says what to turn when it does not.
- Nothing in `Assets/Kapman/Tests` names `minMargin` or the debt's 480 (searched; the one 480 there is a distance in
  `LaneTests.cs`).

### 7.5 The order of work with the other fixes

1. `apply_money_fix.py --apply` with the block in (the famine, the toll, the margin, the debt: one commit).
2. The pre-run (`Design/fixes/prerun.md`). At the proposed margin it matters far less than it did: without it the
   sensible player's middle season is 2,231 d against 2,020 d, a tenth more, where the approved set swung from
   840 d to 1,360 d on it. So the money no longer waits on the pre-run. Until it is built the debt is paid in 85
   seasons of 100, not 66: build it, but nothing breaks meanwhile.
3. The debt's day (`Design/fixes/debt_day.md`) and the orders (`Design/fixes/orders.md`).
4. The debt chain's sums and words (7.2), with the pay cap.
5. The measured run (7.4, last point), then the pound if it must move.

## 8. How sure the model is

**What is firm.** soak2 with the margin at 0.12 and the orders off gives soak.py's numbers to the last digit on the
same seeds (65 of 100; 521 d; 13 / 38 d a laden leg; 8 / 67 d in ten days). The mechanism of section 2 is read in the
C# itself (`TraderBrain.cs` 142, 208, 253), not only in the model. The direction cannot be otherwise: a trader who
stops at a quarter leaves a quarter.

**What moves the result** *(soak2, the sensible player, 200 seasons each; every row against a debt of 1,920 d)*:

| What is moved | Pays 1,920 d, of 100 | Half the seasons pay | Silver on the day: low / middle / high | A laden leg: mean / 1 in 10 | Ten days, middle |
|---|---|---|---|---|---|
| **The proposed set** | 66 | 2,019 d (8.4 pounds) | 1,760 / 2,020 / 2,209 | 27 / 73 d | 106 d |
| He wants 10 % over what he pays, not 15 % | 98 | 2,254 d (9.4 pounds) | 2,073 / 2,255 / 2,446 | 29 / 80 d | 122 d |
| He wants 20 % | 26 | 1,795 d (7.5 pounds) | 1,489 / 1,792 / 2,091 | 26 / 74 d | 89 d |
| He wants 30 % | 0 | 885 d (3.7 pounds) | 501 / 886 / 1,501 | 31 / 101 d | 24 d |
| Other traders x0.75 (42 ships, not 56) | 80 | 2,115 d (8.8 pounds) | 1,857 / 2,116 / 2,315 | 28 / 76 d | 112 d |
| Other traders x1.25 (70 ships) | 54 | 1,943 d (8.1 pounds) | 1,734 / 1,951 / 2,165 | 26 / 70 d | 100 d |
| No talks at all | 52 | 1,928 d (8.0 pounds) | 1,715 / 1,933 / 2,129 | 27 / 71 d | 103 d |
| Every price free at each call (the code today) | 70 | 2,018 d (8.4 pounds) | 1,749 / 2,018 / 2,230 | 27 / 73 d | 106 d |
| The reeve at Lundenwic takes 20 d, eased 30 days | 50 | 1,926 d (8.0 pounds) | 1,715 / 1,928 / 2,158 | 26 / 70 d | 102 d |
| A landing due of 1 d at every call (stands for rounding) | 52 | 1,932 d (8.0 pounds) | 1,692 / 1,932 / 2,123 | 26 / 71 d | 99 d |
| The famine off | 84 | 2,072 d (8.6 pounds) | 1,896 / 2,072 / 2,261 | 27 / 78 d | 107 d |
| NOT APPLIED: the famine still on all food | 84 | 2,137 d (8.9 pounds) | 1,865 / 2,135 / 2,388 | 31 / 83 d | 115 d |
| NOT APPLIED: the English toll still 0.05 | 98 | 2,623 d (10.9 pounds) | 2,348 / 2,624 / 2,893 | 35 / 95 d | 154 d |
| NOT BUILT: no pre-run (the traders start on the first morning) | 85 | 2,231 d (9.3 pounds) | 1,887 / 2,231 / 2,539 | 30 / 85 d | 103 d |
| The margin 0.22 | 1 | 1,502 d (6.3 pounds) | 1,145 / 1,505 / 1,779 | 21 / 58 d | 75 d |
| The margin 0.28 | 91 | 2,237 d (9.3 pounds) | 1,932 / 2,236 / 2,416 | 30 / 75 d | 121 d |
| The start on 21 June (100 days) | 0 | 1,512 d (6.3 pounds) | 1,344 / 1,516 / 1,737 | 30 / 85 d | 107 d |

Read it this way:

- **The count of traders, which money.md called the strongest number in the economy, is now a weak one.** 42 ships
  or 70 moved the approved result from 95 of 100 to 30. Here they move the middle season by 4 to 5 % either way.
  The gap is held open by the traders' own rule now, not by how many of them reach it.
- **How keen the player is still decides, and that is right.** 10 % or 20 % is 2,254 d or 1,795 d: 12 % either side.
  A player who wants 30 % starves (885 d): he waits for gaps the traders close at 25 %. In the approved game the
  same two marks were 92 and 35 of 100, with a middle season of 521 d.
- **The season's sum is steep in the margin**: 1,502 d at 0.22, 2,019 d at 0.25, 2,359 d at 0.30. An error of 0.03
  in how the C# traders really behave is two pounds. This is the weakest point of the proposal.
- **The reeve and the rounding.** Each alone brings eight pounds to one season in two (50 and 52 of 100). Both
  together: 36 of 100, and half the seasons pay 1,839 d (section 11, the fourth single row). The rounding is in the
  game today; the reeve is ruled and not built. So eight pounds now, seven and a half when the reeve comes.
- **The rows "not applied" and "not built"** say what the eight pounds meets if a part of the approved fix is still
  missing on the day this goes in. The English toll at 0.05 is the one that matters: nearly eleven pounds.
- **21 June.** If the clock still starts on 21 June (the code's `startDay` 172; the calendar check has 1 May), the
  season is 100 days and half the seasons pay 1,512 d. Then the sum is six pounds, not eight.

**What the model does not have**, each of which can move a pound: the C# itself (a port by reading; money.md 12
lists what was not ported); the world beyond the twelve ports (in the game 138 traders work 50 ports and 82 more come
in: more hulls on some of these ways, fewer on others); the game's own weather; cards, the reeve's other choices,
river tolls, raiders, the king's men, storms and their repairs (`Design/fixes/risk_is_real.md`), Sundays when the
markets are shut; and a human, who is not the bot.

**So: the rule for the builder.** Apply both numbers. Then measure, in C#, a plain trader's silver at home on
Michaelmas in 100 runs (money.md 11.7; set `SimUnits.AcrossDefault` to 60 for it, money.md 11.6).

| The plain trader pays the debt in | Do |
|---|---|
| 40 to 70 runs of 100 | Nothing. |
| fewer than 40 | Take half a pound off the debt (120 d is about twenty runs in a hundred) until it is in the band. |
| more than 70 | Put half a pound on. |
| his middle silver is under 1,200 d or over 2,900 d (five to twelve pounds) | The traders in C# do not behave as the model's. Do not chase it with the debt: say so, and look at `minMargin` again with the measured run in hand. |

Turn the **debt**, in half pounds, and leave the margin alone once it is in: the margin changes every price in the
world and every leg; the debt changes one line in the top bar.

## 9. The five-speed clock: wages and bread by the day

`Design/fixes/clock_five_speeds.md` 7.2 and 7.3 already say that the numbers hold in game time, give the two drops
for bread (under two days: speed 2; none: speed 1), and ask for a standing order for bread and for the pay to be
said as a sum. This adds only the money.

**The sums do not change.** Ruling 26: everything that counts time counts game time. Four men at a quarter are 1 d
a day; 2.8 kg of bread a day from a 20 kg sack at about 2.6 d is 0.37 d a day; 1.37 d a day, 207 d from 1 May to
Michaelmas, at every speed. The soak has no clock speed in it and needs none. Money.md's "A real hour. At 4x an hour
is ten game days" is the old clock: read "at speed 2".

**What an hour of the clock now costs in keep:**

| Speed | A game day lasts | Game days in a real hour | Pay and bread in that hour | A sack of bread lasts | The season to Michaelmas lasts |
|---|---|---|---|---|---|
| 1 | 24 min | 2.5 | 3.4 d | 2 h 51 min | 60 h |
| 2 | 6 min | 10 | 13.7 d | 43 min | 15 h |
| 3 | about 1 min | 60 | 82 d | 7 min | 2 h 31 min |
| 4 | about 15 s | 240 | 329 d | 1 min 47 s | 38 min |
| 5 | about 4 s | 900 | 1,233 d | 29 s | 10 min |

**What that changes.**

1. **A day's keep used to be paid twice: 1.4 d and six minutes of the player's evening.** The minutes were the
   dearer coin, and they are gone: a day lying for a wind is four seconds at speed 5. So the keep has to be seen as
   silver or it is not seen. Say it by the day and by the leg, never by the hour: "This leg: about 3 days. Pay and
   bread 4 d." on the order to cast off; "Lying here costs 1.4 d a day" where she waits.
2. **With the proposed margin the keep is a tenth of the trade, not two fifths.** Approved: 206 d of pay and bread
   against 500 d gained on goods. Proposed: 207 d against 2,004 d. A day lying idle costs 1.4 d in keep and about
   12 d in trade not done. So what presses the player is no longer the crew's bread but the debt's pace, and the
   line that carries it is the tally: "Owed 1,920. In hand 1,062. 101 days left: 8.5 d a day will do it." That line
   is true at every speed and is the one to put on the House panel's brief face.
3. **If the keep should bite as before**, the number is wages: 0.5 d a man a day (money.md 9: "a skilled man's
   pay") costs 151 d more a season; at the proposed margin half the seasons then pay 1,836 d, so the debt would be
   seven and a half pounds, and laden legs at a loss go from 3 to 9 in 100 (section 11, the second single row). It
   is not in the proposed set, because the smallest set is two numbers and the keep now has the clock's own drops
   to make it felt.
4. **Speed makes a short purse sudden.** At speed 4 the fourteen days after which an unpaid man may leave
   (`voyage_rules.json` `leave_if_owed_days`; `Voyage.cs` 297) pass in three and a half minutes. The clock write-up
   relies on the drop at "made fast" to give the player the call in which to pay. That holds while he calls often.
   It does not cover the purse that cannot pay at that call. One more reason for the list in clock_five_speeds 4.3:
   **pay owed for 10 days and the purse short of it: drop to 2**, armed again when the men are paid.
5. **Rounding** (the critic's con 3: `Voyage.InPort` pays whole coins, about 30 d a season by his sum, and every
   click of a deal rounds too). The clock does not change how many calls a season holds. The proposed margin makes
   those 30 d a seventieth of the gain on goods, where they were a seventeenth. It still moves the debt: the row "a
   landing due of 1 d at every call" in section 8 (about 85 d a season, a stand-in for all the rounding) takes
   eight pounds from 66 seasons in 100 to 52. Mend it or leave it, but measure the debt after.

## 10. What I could not check, and what will look different

- The game was not run, no C# was executed, no test was run. soak2 is a Python copy of the rules, as soak.py is;
  every caveat of money.md section 12 stands.
- Whether the C# traders at 0.25 behave as the model's: half the goods carried, 41 sailings in 100 laden. **The
  player will see more ships in ballast.** If that reads as an empty sea, the mend is in what the lookout says of
  them, or a second look at the number; I could not judge it without the game.
- Whether `TestRunTenYears` still passes (prices within twofold over ten years, no trader stuck).
- The second season. The model stops at Michaelmas. "A coaster after one more summer" and "twenty pounds by the
  second Michaelmas" are the first season's pace carried on, not runs.
- The soak's player keeps orders badly (section 6), so what a good player makes of the capped orders is between the
  two columns there, not a number.
- The 79 lines that name the old sum were counted by pattern; I read the debt chain's own 31 and a sample of the
  rest.
- The cards as they stood at about 12:30; the content pass was still writing.
- The ships' prices and the hard terms were read in `ships_roster.md` and `DECISIONS.md`, not modelled.

## 11. soak2.py

`Bake/economy/soak2.py` is soak.py of this morning with three additions, each marked in its header:

1. `min_margin` is a rule (soak.py had the constant 0.12). It is read from `WorldSim.cs` as the code stands, so
   after the fix a bare run is the fixed game. The config `approved` pins 0.12, so the "before" of this page can be
   run again at any time.
2. The orders of `commissions_793.json` at the twelve ports (`--set orders=1`, `order_tolled`, `order_premium_max`,
   `order_extra_max`, and the what-if `order_premium_all`): posted as `Commissions.Daily` posts them, taken by the
   perfect and the sensible player where they are posted, fetched and delivered.
3. A report of its own, `--config pays`, and three sums in every summary: the debt that half the seasons pay, the
   other traders' dealings, and what the orders brought.

```text
& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\economy\soak2.py --config pays
      the whole report below: 300 seeds, 13 minutes on this machine
soak2.py --seeds 100 --config approved --brief                     ruling 14 as approved (soak.py's "tuned")
soak2.py --seeds 100 --config pays --brief                         the proposed set, one line a player
soak2.py --seeds 100 --config pays --kinds sensible --brief --set debt=2040 --set door_fee=20
soak2.py --seeds 1 --config pays --kinds sensible --trace          one season, deal by deal
```

The four single rows quoted above that are not in the report were run so (200 seeds, the sensible player; "clear"
is the day the tally first reads the debt, one in four / middle / three in four):

```text
> soak2.py --seeds 200 --config pays --kinds sensible --brief --set debt=480
sensible  pays  98% home  98% silver  1760/ 2020/ 2209  a day  11.8  legs 81.9  a leg   23 (mid   11, 1in4   32, 1in10   65)  clear 9/11/12 never   0%  d100 1371/1602/1787  short@100   0% wrong@100 2  ten 106/161/224  laden 84% of legs: 27/15/38/73
          half the seasons pay 2019 d (4 in 5: 1855; 1 in 5: 2152)  ten days mean 120, lose 1%  laden legs lose 3%  orders posted 0.0 seen 0.0 too big 0.0 taken 0.0 done 0.0 broken 0.0 gain 0 (over the market 0)

> soak2.py --seeds 200 --config pays --kinds sensible --brief --set pay=0.5
sensible  pays  34% home  98% silver  1632/ 1837/ 2063  a day  10.8  legs 82.6  a leg   20 (mid    9, 1in4   28, 1in10   62)  clear 122/132/138 never  63%  d100 1270/1490/1684  short@100 100% wrong@100 46  ten 94/151/215  laden 83% of legs: 25/13/35/70
          half the seasons pay 1836 d (4 in 5: 1673; 1 in 5: 2003)  ten days mean 109, lose 2%  laden legs lose 9%  orders posted 0.0 seen 0.0 too big 0.0 taken 0.0 done 0.0 broken 0.0 gain 0 (over the market 0)

> soak2.py --seeds 200 --config pays --kinds sensible --brief --set made.frankish_wine.neustria=2 --set wanted.frankish_wine.england_south=2
sensible  pays   0% home  98% silver  1420/ 1566/ 1700  a day   8.9  legs 83.6  a leg   17 (mid    9, 1in4   25, 1in10   46)  clear nan/nan/nan never 100%  d100 1079/1233/1354  short@100 100% wrong@100 12  ten 83/118/157  laden 80% of legs: 21/14/30/51
          half the seasons pay 1564 d (4 in 5: 1451; 1 in 5: 1661)  ten days mean 89, lose 2%  laden legs lose 4%  orders posted 0.0 seen 0.0 too big 0.0 taken 0.0 done 0.0 broken 0.0 gain 0 (over the market 0)

> soak2.py --seeds 200 --config pays --kinds sensible --brief --set door_fee=20 --set call_fee=1
sensible  pays  36% home  98% silver  1593/ 1848/ 2091  a day  10.8  legs 82.7  a leg   20 (mid    9, 1in4   28, 1in10   61)  clear 123/131/140 never  64%  d100 1245/1490/1670  short@100 100% wrong@100 42  ten 95/149/210  laden 83% of legs: 25/14/35/69
          half the seasons pay 1839 d (4 in 5: 1678; 1 in 5: 1996)  ten days mean 108, lose 2%  laden legs lose 7%  orders posted 0.0 seen 0.0 too big 0.0 taken 0.0 done 0.0 broken 0.0 gain 0 (over the market 0)
```

### The script's output

`soak2.py --config pays`, 7 October 2026, 300 seeds. "low / mid / high" are one in ten under, the middle, one in ten
over; "lb" is pounds of 240 d.

```text
# MONEY THAT PAYS: soak2.py, 300 seeded seasons for each kind of player (200 where a table says so)

## 1. What the world's traders leave open, by the least margin they will carry for (24 seeds, no player)
the approved set, margin 0.12
  good            bought at, sold at       the season: asks > pays, gain first morning: asks > pays, gain
  rhenish_wine    Dorestat > Lundenwic           64.3 >  68.1:   +3.7 ( +6%)       60.2 >  59.2:   -1.0 ( -2%)
  rhenish_wine    Dorestat > Ripa                64.3 >  69.4:   +5.0 ( +8%)       60.2 >  62.3:   +2.1 ( +4%)
  frankish_wine   Quentovic > Lundenwic          49.1 >  64.0:  +14.8 (+30%)       45.9 >  56.6:  +10.8 (+23%)
  frisian_cloth   Medemelacha > Quentovic        41.0 >  41.5:   +0.5 ( +1%)       40.1 >  40.6:   +0.4 ( +1%)
  glass_vessels   Quentovic > Lundenwic          75.0 >  79.0:   +4.1 ( +5%)       74.7 >  78.8:   +4.1 ( +6%)
  honey           Lundenwic > Medemelacha        24.0 >  24.3:   +0.3 ( +1%)       27.2 >  26.7:   -0.4 ( -2%)
  wheat           Lundenwic > Doccinga            7.9 >  11.2:   +3.3 (+41%)       10.9 >  11.7:   +0.8 ( +7%)
  cheese          Medemelacha > Quentovic         2.6 >   2.6:   +0.1 ( +3%)        2.9 >   2.9:   -0.0 ( -1%)
  bar_iron        Dorestat > Medemelacha          8.0 >   8.0:   +0.0 ( +0%)        8.1 >   7.5:   -0.6 ( -8%)
  books           Quentovic > Lundenwic         361.9 > 408.2:  +46.3 (+13%)      363.5 > 408.2:  +44.7 (+12%)
  wool_cloth      Lundenwic > Dorestat           13.0 >   8.7:   -4.2 (-33%)       13.1 >   8.7:   -4.3 (-33%)
  quernstones     Dorestat > Lundenwic            7.9 >  13.6:   +5.7 (+72%)        7.9 >  13.6:   +5.7 (+72%)
  rhenish_wine    Dorestat > Sarre               64.3 >  65.8:   +1.4 ( +2%)       60.2 >  57.2:   -2.9 ( -5%)
  frankish_wine   Quentovic > Sarre              49.1 >  60.9:  +11.8 (+24%)       45.9 >  52.2:   +6.4 (+14%)
  the 56 other traders, from the first morning to Michaelmas: 26.1 sailings each, 76% of them laden; 2.6 times each found nothing worth carrying; 1595 d laid out on goods each; worth 279 d at the end (301 d at the start)

margin 0.20
  good            bought at, sold at       the season: asks > pays, gain first morning: asks > pays, gain
  rhenish_wine    Dorestat > Lundenwic           60.6 >  69.4:   +8.9 (+15%)       54.2 >  59.8:   +5.6 (+10%)
  rhenish_wine    Dorestat > Ripa                60.6 >  72.4:  +11.8 (+20%)       54.2 >  64.3:  +10.0 (+18%)
  frankish_wine   Quentovic > Lundenwic          47.4 >  65.0:  +17.5 (+37%)       45.8 >  57.1:  +11.2 (+24%)
  frisian_cloth   Medemelacha > Quentovic        39.2 >  43.0:   +3.8 (+10%)       38.5 >  40.5:   +2.0 ( +5%)
  glass_vessels   Quentovic > Lundenwic          71.8 >  80.0:   +8.2 (+11%)       73.2 >  80.1:   +6.9 ( +9%)
  honey           Lundenwic > Medemelacha        23.0 >  26.0:   +2.9 (+13%)       26.1 >  29.8:   +3.7 (+14%)
  wheat           Lundenwic > Doccinga            7.4 >  11.2:   +3.8 (+52%)        9.9 >  12.3:   +2.4 (+24%)
  cheese          Medemelacha > Quentovic         2.5 >   2.8:   +0.3 (+13%)        2.9 >   3.1:   +0.2 ( +5%)
  bar_iron        Dorestat > Medemelacha          7.7 >   8.6:   +0.9 (+11%)        7.8 >   7.7:   -0.1 ( -1%)
  books           Quentovic > Lundenwic         361.9 > 408.2:  +46.3 (+13%)      363.5 > 408.2:  +44.7 (+12%)
  wool_cloth      Lundenwic > Dorestat           13.0 >   8.7:   -4.2 (-33%)       13.1 >   8.7:   -4.3 (-33%)
  quernstones     Dorestat > Lundenwic            7.9 >  13.6:   +5.7 (+72%)        7.9 >  13.6:   +5.7 (+72%)
  rhenish_wine    Dorestat > Sarre               60.6 >  67.1:   +6.5 (+11%)       54.2 >  57.4:   +3.2 ( +6%)
  frankish_wine   Quentovic > Sarre              47.4 >  61.2:  +13.8 (+29%)       45.8 >  53.9:   +8.0 (+18%)
  the 56 other traders, from the first morning to Michaelmas: 26.6 sailings each, 55% of them laden; 1.0 times each found nothing worth carrying; 1193 d laid out on goods each; worth 392 d at the end (301 d at the start)

margin 0.25 (proposed)
  good            bought at, sold at       the season: asks > pays, gain first morning: asks > pays, gain
  rhenish_wine    Dorestat > Lundenwic           56.1 >  70.8:  +14.7 (+26%)       49.9 >  61.6:  +11.7 (+24%)
  rhenish_wine    Dorestat > Ripa                56.1 >  78.0:  +21.9 (+39%)       49.9 >  67.5:  +17.6 (+35%)
  frankish_wine   Quentovic > Lundenwic          39.3 >  69.6:  +30.2 (+77%)       34.2 >  65.1:  +30.9 (+90%)
  frisian_cloth   Medemelacha > Quentovic        37.3 >  43.2:   +5.9 (+16%)       35.5 >  40.4:   +4.9 (+14%)
  glass_vessels   Quentovic > Lundenwic          69.2 >  80.8:  +11.7 (+17%)       66.8 >  81.4:  +14.6 (+22%)
  honey           Lundenwic > Medemelacha        22.2 >  27.6:   +5.4 (+24%)       25.1 >  32.1:   +7.1 (+28%)
  wheat           Lundenwic > Doccinga            6.3 >  11.7:   +5.3 (+84%)        8.9 >  12.7:   +3.8 (+43%)
  cheese          Medemelacha > Quentovic         2.4 >   2.9:   +0.5 (+20%)        2.9 >   3.2:   +0.3 (+11%)
  bar_iron        Dorestat > Medemelacha          7.5 >   9.0:   +1.5 (+21%)        7.6 >   8.8:   +1.2 (+15%)
  books           Quentovic > Lundenwic         361.9 > 408.2:  +46.3 (+13%)      363.5 > 408.2:  +44.7 (+12%)
  wool_cloth      Lundenwic > Dorestat           13.0 >   8.7:   -4.2 (-33%)       13.1 >   8.7:   -4.3 (-33%)
  quernstones     Dorestat > Lundenwic            7.9 >  13.6:   +5.7 (+72%)        7.9 >  13.6:   +5.7 (+72%)
  rhenish_wine    Dorestat > Sarre               56.1 >  68.3:  +12.2 (+22%)       49.9 >  59.9:  +10.0 (+20%)
  frankish_wine   Quentovic > Sarre              39.3 >  65.7:  +26.4 (+67%)       34.2 >  58.1:  +23.8 (+70%)
  the 56 other traders, from the first morning to Michaelmas: 27.1 sailings each, 41% of them laden; 0.5 times each found nothing worth carrying; 797 d laid out on goods each; worth 430 d at the end (301 d at the start)

margin 0.30
  good            bought at, sold at       the season: asks > pays, gain first morning: asks > pays, gain
  rhenish_wine    Dorestat > Lundenwic           53.3 >  72.2:  +18.9 (+35%)       47.3 >  64.9:  +17.6 (+37%)
  rhenish_wine    Dorestat > Ripa                53.3 >  86.0:  +32.7 (+61%)       47.3 >  70.8:  +23.5 (+50%)
  frankish_wine   Quentovic > Lundenwic          37.5 >  71.1:  +33.6 (+90%)       33.0 >  65.2:  +32.2 (+97%)
  frisian_cloth   Medemelacha > Quentovic        36.2 >  44.2:   +8.1 (+22%)       34.5 >  41.9:   +7.4 (+21%)
  glass_vessels   Quentovic > Lundenwic          66.3 >  81.4:  +15.1 (+23%)       66.7 >  81.4:  +14.7 (+22%)
  honey           Lundenwic > Medemelacha        22.0 >  28.4:   +6.4 (+29%)       25.0 >  32.6:   +7.6 (+31%)
  wheat           Lundenwic > Doccinga            5.8 >  12.0:   +6.1 (+105%)        6.2 >  13.5:   +7.2 (+116%)
  cheese          Medemelacha > Quentovic         2.4 >   3.0:   +0.7 (+28%)        2.8 >   3.3:   +0.4 (+16%)
  bar_iron        Dorestat > Medemelacha          7.4 >   9.7:   +2.3 (+31%)        7.5 >   9.3:   +1.8 (+23%)
  books           Quentovic > Lundenwic         361.9 > 408.2:  +46.3 (+13%)      363.5 > 408.2:  +44.7 (+12%)
  wool_cloth      Lundenwic > Dorestat           13.0 >   8.7:   -4.2 (-33%)       13.1 >   8.7:   -4.3 (-33%)
  quernstones     Dorestat > Lundenwic            7.9 >  13.6:   +5.7 (+72%)        7.9 >  13.6:   +5.7 (+72%)
  rhenish_wine    Dorestat > Sarre               53.3 >  70.0:  +16.8 (+31%)       47.3 >  61.7:  +14.4 (+30%)
  frankish_wine   Quentovic > Sarre              37.5 >  67.2:  +29.6 (+79%)       33.0 >  60.3:  +27.3 (+83%)
  the 56 other traders, from the first morning to Michaelmas: 25.0 sailings each, 36% of them laden; 0.5 times each found nothing worth carrying; 626 d laid out on goods each; worth 432 d at the end (301 d at the start)

## 2. The margin, a step at a time (the sensible player, 1 May to Michaelmas, 200 seasons each; all else as approved)
margin         half the   silver on the day    made   laden legs      a laden leg makes   lose          ten days make   lose
            seasons pay    low / mid / high   a day  of all legs mean/mid/1 in 4/1 in 10      %     mid/1 in 4/1 in 10      %
0.12        511 ( 2.1 lb)   373 /  514 /  667     1.9  31.8 of 71.5    13 /  6 /  17 /  38     30      8 /   30 /   67     34
0.15        595 ( 2.5 lb)   442 /  600 /  820     2.6  42.9 of 76.7    11 /  5 /  14 /  33     27     14 /   39 /   79     26
0.18        856 ( 3.6 lb)   627 /  856 / 1160     4.3  53.1 of 79.7    14 /  7 /  18 /  38     15     32 /   68 /  107     14
0.20       1167 ( 4.9 lb)   842 / 1168 / 1490     6.3  58.7 of 82.3    18 /  9 /  23 /  47      8     53 /   95 /  137      8
0.22       1502 ( 6.3 lb)  1145 / 1505 / 1779     8.4  62.7 of 83.7    21 / 11 /  29 /  58      5     75 /  124 /  171      6
0.25       2019 ( 8.4 lb)  1760 / 2020 / 2209    11.8  69.0 of 81.9    27 / 15 /  38 /  73      3    106 /  161 /  224      1
0.30       2359 ( 9.8 lb)  2154 / 2361 / 2584    14.3  71.7 of 83.5    31 / 19 /  47 /  78      3    133 /  185 /  250      0
0.35       2804 (11.7 lb)  2481 / 2811 / 3046    17.1  70.8 of 82.1    37 / 25 /  60 /  91      2    170 /  218 /  270      0

## 3. BEFORE: the approved set (ruling 14), the debt 480 d
### approved, 1 May to Michaelmas
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

(900 runs in 80 s)

## 4. AFTER: the margin 0.25 and the debt 1920 d (8.0 pounds)
### proposed, 1 May to Michaelmas
start 05-01, due 09-29 (day 151); debt 1920 d; purse 85.5 d; cargo 4 rhenish_wine, 2 quernstones, 1 ships_bread, 1 ale; hold 660 kg of room
spread 0.20; toll Frankish 0.10, English 0.10, weighed silver 0.00; wage 0.25 d a man a day (4 men); appetite_value 80; stock_value 150; regen 0.05; glut_fade 0.05
made_factor {"0.5": 1.4, "1.0": 0.8, "2.0": 0.65, "3.0": 0.5}; wanted_factor {"1.0": 1.15, "2.0": 1.4, "3.0": 1.7}
famine on (wheat,rye,barley,oats,malt,flour); other traders x1.00, at work 30 days before day one; talks a call 3

                                         perfect    sensible    careless
seasons                                      300         300         300
PAYS 1920 d on the day, %                    100          68           0
home on the day, %                           100          99          20
silver on the day: 1 in 10 under            2268        1785          66
silver on the day: middle                   2528        2026         104
silver on the day: 1 in 10 over             2776        2215         140
the sum paid in 8 seasons of 10             2360        1861           0
the sum paid in 5 seasons of 10             2528        2021           0
the sum paid in 2 seasons of 10             2697        2156          51
made in the season (middle), d              2313        1810        -109
made a day, d                               15.3        11.9        -0.7
legs a season                              109.3        82.0        65.5
a leg: days                                  1.4         1.8         1.3
a leg: km                                    183         172         119
made a leg: mean, d                           22          23          -1
made a leg: middle, d                          6          11          -2
made a leg: 1 in 4 over, d                    33          32           5
made a leg: 1 in 10 over, d                   66          66          17
legs that lost silver, %                      30          18          63
legs with goods aboard, %                     78          84          90
  their share of the days, %                  84          92          95
  a laden leg: days                          1.5         2.0         1.4
  made a laden leg: mean, d                   28          27          -1
  made a laden leg: middle, d                 16          15          -2
  made a laden leg: 1 in 4 over               44          38           7
  made a laden leg: 1 in 10 over              74          73          19
  laden legs that lost silver, %              10           3          59
made in ten days: middle, d                  141         106          -9
made in ten days: 1 in 4 over, d             197         162          13
made in ten days: 1 in 10 over, d            264         226          42
made in ten days: 1 in 50 over, d            411         318         146
gain on goods a season, d                   2521        2004          97
  tolls paid (already off), d               2130        1091         861
  wages, d                                   151         151         151
  bread, d                                    55          56          53
  the reeve and landing dues, d                0           0           0
days under way                               151         149          86
days lying in port for want of a cargo           0           3           0
days with no bread                           0.0         0.0         3.6
tally first reads 1920: 1 in 4 by day          93         115         nan
tally first reads 1920: middle day           100         124         nan
tally first reads 1920: 3 in 4 by day         109         134         nan
tally never reads 1920, %                      0          30         100

In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over
  day   0   perfect     206 /  213 /  223   sensible    206 /  213 /  223   careless    206 /  213 /  223
  day  25   perfect     736 /  859 /  985   sensible    595 /  740 /  866   careless    191 /  284 /  374
  day  50   perfect    1071 / 1240 / 1397   sensible    832 / 1062 / 1202   careless     96 /  141 /  207
  day  75   perfect    1381 / 1583 / 1780   sensible   1115 / 1351 / 1510   careless     76 /  106 /  159
  day 100   perfect    1682 / 1921 / 2138   sensible   1377 / 1605 / 1796   careless     84 /  106 /  151
  day 125   perfect    1997 / 2238 / 2471   sensible   1631 / 1874 / 2063   careless     83 /  113 /  158
  day 150   perfect    2271 / 2526 / 2769   sensible   1787 / 2028 / 2218   careless     76 /  106 /  142
  day 151   perfect    2271 / 2528 / 2774   sensible   1786 / 2027 / 2217   careless     76 /  106 /  141

Still short of 1920 on the day (% of seasons), and how often the plain pace calls the ending wrongly (%)
  day  25   perfect   short 100, pace wrong   1   sensible  short 100, pace wrong  31   careless  short 100, pace wrong   0
  day  50   perfect   short 100, pace wrong   0   sensible  short 100, pace wrong  28   careless  short 100, pace wrong   0
  day  75   perfect   short  99, pace wrong   0   sensible  short 100, pace wrong  26   careless  short 100, pace wrong   0
  day 100   perfect   short  50, pace wrong   0   sensible  short  99, pace wrong  25   careless  short 100, pace wrong   0
  day 125   perfect   short   6, pace wrong   0   sensible  short  63, pace wrong  22   careless  short 100, pace wrong   0

perfect: goods by share of the gain: frankish_wine 28% (714 d); rhenish_wine 14% (341 d); glass_vessels 9% (217 d); books 6% (151 d); hounds 5% (122 d); linen 4% (110 d); woad 4% (110 d); lead 4% (107 d)
perfect: bought at, sold at, by share of the gain: Quentovic to Lundenwic 21%; Quentovic to Sarre 17%; Quentovic to Fordwic 10%; Quentovic to Medemelacha 7%; Dorestat to Medemelacha 6%; Lundenwic to Quentovic 5%; Medemelacha to Quentovic 4%; Sarre to Quentovic 4%
perfect: legs sailed most (times a season, made a leg, days): Traiectum to Dorestat 10.1x 2 d 0.2 days; Quentovic to Sarre 7.6x 54 d 0.9 days; Medemelacha to Traiectum 7.3x 2 d 0.9 days; Quentovic to Lundenwic 7.3x 65 d 1.9 days; Sarre to Quentovic 6.8x 12 d 0.8 days; Lundenwic to Quentovic 6.1x 18 d 1.8 days; Dorestat to Medemelacha 4.6x 29 d 1.1 days; Daventre to Traiectum 4.2x -1 d 1.3 days

sensible: goods by share of the gain: frankish_wine 40% (801 d); rhenish_wine 12% (239 d); glass_vessels 10% (192 d); hounds 6% (119 d); lead 5% (104 d); linen 5% (97 d); wheat 4% (86 d); woad 3% (68 d)
sensible: bought at, sold at, by share of the gain: Quentovic to Lundenwic 28%; Quentovic to Sarre 20%; Quentovic to Medemelacha 8%; Quentovic to Fordwic 7%; Lundenwic to Quentovic 6%; Dorestat to Medemelacha 5%; Sarre to Quentovic 4%; Dorestat to Sarre 4%
sensible: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 9.0x 42 d 1.3 days; Sarre to Quentovic 8.0x 10 d 1.2 days; Traiectum to Dorestat 7.4x -0 d 0.3 days; Medemelacha to Traiectum 7.2x 3 d 1.3 days; Lundenwic to Quentovic 7.1x 14 d 2.6 days; Quentovic to Lundenwic 6.1x 69 d 2.8 days; Dorestat to Medemelacha 5.0x 19 d 1.6 days; Medemelacha to Quentovic 3.3x 21 d 4.6 days

careless: goods by share of the gain: frankish_wine 61% (185 d); rhenish_wine 11% (33 d); wheat 9% (28 d); glass_vessels 7% (20 d); sheep 6% (18 d); cheese 3% (9 d); hounds 2% (5 d); woad 1% (4 d)
careless: goods that lost: bar_iron -8 d; linen -59 d; frisian_cloth -132 d
careless: bought at, sold at, by share of the gain: Quentovic to Sarre 61%; Quentovic to Medemelacha 12%; Medemelacha to Quentovic 10%; Medemelacha to Traiectum 7%; Dorestat to Medemelacha 7%; Traiectum to Medemelacha 3%; Dorestat to Traiectum 0%; Sarre to Traiectum 0%
careless: legs sailed most (times a season, made a leg, days): Quentovic to Sarre 22.8x 4 d 1.2 days; Sarre to Quentovic 22.5x -9 d 1.2 days; Medemelacha to Traiectum 5.5x 1 d 1.3 days; Traiectum to Dorestat 5.4x -1 d 0.3 days; Dorestat to Medemelacha 4.7x 1 d 1.7 days; Traiectum to Medemelacha 1.0x 5 d 1.2 days; Dorestat to Traiectum 1.0x -1 d 0.2 days; Medemelacha to Quentovic 1.0x 19 d 4.4 days

(900 runs in 86 s)

## 5. The lord's terms at the margin 0.25: what each sum would mean (the same 300 seasons; the players do not look at the debt)
the sum the sensible player holds at home on the day in 9 seasons of 10: 1761 d; in 8: 1861; in 2 of 3: 1925; in half: 2023; in 1 of 3: 2106; in 2 of 10: 2157; in 1 of 10: 2217
the debt              sensible   tally first reads it   never short, day 100 short, day 125     pace wrong, day   perfect perfect first  careless
                        pays %    1 in 4 / middle day       %              %              %    75 / 100 / 125 %    pays % reads it, day    pays %
 4.0 pounds ( 960 d)        99           36 / 43            0              0              0       1 /   1 /   1       100           32         0
 5.0 pounds (1200 d)        99           56 / 62            0              1              0       1 /   1 /   1       100           47         0
 6.0 pounds (1440 d)        99           76 / 85            0             17              2       1 /   1 /   1       100           65         0
 7.0 pounds (1680 d)        94           97 / 106           5             68             16       5 /   5 /   5       100           84         0
 7.5 pounds (1800 d)        87          107 / 115          11             90             37      11 /  10 /   9       100           92         0
 8.0 pounds (1920 d)        68          115 / 124          30             99             63      26 /  25 /  22       100          100         0
 8.5 pounds (2040 d)        45          124 / 132          53            100             87      44 /  41 /  31       100          109         0
 9.0 pounds (2160 d)        19          128 / 138          79            100             97      64 /  53 /  39        96          118         0
10.0 pounds (2400 d)         0          144 / 144         100            100            100      62 /  34 /  16        75          135         0
12.0 pounds (2880 d)         0          nan / nan         100            100            100       7 /   1 /   0         3          144         0

the same for the approved set (margin 0.12), for comparison
 2.0 pounds ( 480 d)        65           61 / 91           29             58             40      27 /  22 /  13       100           22         0

the tally sentence, proposed: in hand and in the hold, 1 in 10 under / middle / 1 in 10 over; and ten days' gain by where in the season
  day   0     206 /  213 /  223
  day  25     595 /  740 /  866
  day  50     832 / 1062 / 1202
  day  75    1115 / 1351 / 1510
  day 100    1377 / 1605 / 1796
  day 125    1631 / 1874 / 2063
  day 150    1787 / 2028 / 2218
  day 151    1786 / 2027 / 2217

## 6. How firm it is (the sensible player, 200 seasons each): each row against the debt 1920 d
                                                                 pays %       half the   silver on the day      a laden leg     ten days
                                                                           seasons pay    low / mid / high   mean / 1 in 10       middle
THE PROPOSED SET                                                     66   2019 ( 8.4 lb)  1760 / 2020 / 2209       27 /   73          106
he wants 10 % over what he pays, not 15 %                            98   2254 ( 9.4 lb)  2073 / 2255 / 2446       29 /   80          122
he wants 20 %                                                        26   1795 ( 7.5 lb)  1489 / 1792 / 2091       26 /   74           89
he wants 30 %                                                         0    885 ( 3.7 lb)   501 /  886 / 1501       31 /  101           24
other traders x0.75 (42 ships, not 56)                               80   2115 ( 8.8 lb)  1857 / 2116 / 2315       28 /   76          112
other traders x1.25 (70 ships)                                       54   1943 ( 8.1 lb)  1734 / 1951 / 2165       26 /   70          100
no talks at all                                                      52   1928 ( 8.0 lb)  1715 / 1933 / 2129       27 /   71          103
every price free at each call (the code today)                       70   2018 ( 8.4 lb)  1749 / 2018 / 2230       27 /   73          106
the reeve at Lundenwic takes 20 d, eased 30 days                     50   1926 ( 8.0 lb)  1715 / 1928 / 2158       26 /   70          102
a landing due of 1 d at every call (stands for rounding)             52   1932 ( 8.0 lb)  1692 / 1932 / 2123       26 /   71           99
the famine off                                                       84   2072 ( 8.6 lb)  1896 / 2072 / 2261       27 /   78          107
NOT APPLIED: the famine still on all food                            84   2137 ( 8.9 lb)  1865 / 2135 / 2388       31 /   83          115
NOT APPLIED: the English toll still 0.05                             98   2623 (10.9 lb)  2348 / 2624 / 2893       35 /   95          154
NOT BUILT: no pre-run (the traders start on the first morning)       85   2231 ( 9.3 lb)  1887 / 2231 / 2539       30 /   85          103
the margin 0.22                                                       1   1502 ( 6.3 lb)  1145 / 1505 / 1779       21 /   58           75
the margin 0.28                                                      91   2237 ( 9.3 lb)  1932 / 2236 / 2416       30 /   75          121
the start on 21 June (100 days)                                       0   1512 ( 6.3 lb)  1344 / 1516 / 1737       30 /   85          107

## 7. The orders of commissions_793.json at the twelve ports (the sensible player, 200 seasons each, the proposed set)
                                                            posted   seen  too big   taken  delivered   broken      gain, d   hoped for, d     half pay
as the code pays them (no toll, the table's premiums)         10.5    4.0      0.1     0.7        0.2      0.5           15             58         2019
tolled, premium at most 0.5, at most 60 d over the market     10.4    4.1      0.2     0.5        0.0      0.4            2             31         2020
WHAT IF the table paid the cards' premium of 1.5, no toll     10.6    3.9      0.1     2.6        1.7      0.8          374            498         2270
the same, tolled and capped                                   10.4    4.0      0.2     0.5        0.1      0.4            4             33         2027
```
