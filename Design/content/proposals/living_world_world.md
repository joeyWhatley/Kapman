<!-- Proposal, 4 Oct 2026, night. One stance of several on the same ask. Read with CHAINS.md, research_scale_to_fleets.md and economy_and_trade.md 2.1 to 2.7. Every number is a starting guess to tune in the headless harness. (?) = not checked against a source. -->

# The living world: the world first

**The stance.** Under the 138 ships the world keeps three ledgers: **settlements, realms, wars**. They tick once a game month, with or without the player. History is their default course: the chronicle's 582 entries still fire on their days. The player bends it only by changing a number in a ledger: bread carried, hulls lent, news brought early, silver paid, people moved.

Three facts from the code and the data decide the shape.

1. **The chronicle goes quiet.** 422 of its 582 entries fall before 825; after that it has about seven a decade. The world has to make its own events.
2. **A town is already half a simulation.** `Market` has a size, burnings, ruin and shut days; raiders choose a place by its `defence`. What is missing is people.
3. **A monthly tick is free.** 50 ports (450 with the river places) over 3,276 months is under a second, so any start year, and years passed ashore, cost nothing.

**The truth rule** (BRIEF rule 1). What the world makes for itself is small and nameless: a band of three ships, a reeve's quarrel, a hungry spring. Kings, battles and dates come only from the chronicle or an authored alternative, and the log shows which is which in two inks.

## 1. Settlements

One row a port in a new `settlements_793.json`, a class `Settlement` beside `Market`: **souls**; **room** (what its site, fields and trade can carry); **bread** (days of grain in store); **guard** (fighting men: the `raiders_793` defence by kind, plus works); **state** (open, hungry, starving, burned, besieged, held by a new lord, ruined).

Anchors for souls about 800: Dorestat 2,000 to 3,000 (?) (`traffic_793`); Lundenwic 5,000 to 10,000 (?), about 20,000 by 1066 (?); Hamwic 2,000 to 3,000; Birka 500 to 1,000 (all en.wikipedia); Wearmouth and Jarrow about 600 brethren in 716 (Bede (?)); a terp village 60 to 150 (?). A land grew by about one in a thousand a year; towns grew by trade and incomers, not by births.

**The monthly tick.**

1. *Births less deaths:* a village +0.3 % a year; a town over 1,000 souls -0.5 % (?).
2. *Incomers:* souls close 4 % a year of the gap to `room`. `room` rises 2 % for every 10 % more ship calls in the year than the traffic table's usual, and with works.
3. *Bread:* a soul eats 0.7 kg of grain a day (a 60 kg sack is 86 soul-days). The fields give a share (village 1.1, town 0.9, emporium 0.75) times the harvest; the rest comes by keel through the market. Under 30 days of bread a place is **hungry**: food half as dear again, 1 % leave a month. At none it is **starving**: 1 % die and 2 % leave a month. A famine year costs a town 15 to 25 % (?).
4. *Fire and sword:* a raid takes captives (the built 6 to 20, scaled by souls) and kills 1 to 3 %. A burning empties 40 %; half return in two years if timber and tools come.
5. *Size:* the market's size is its `ports_793` size plus log2(souls / souls at the start), rounded. Today's tuned table is exactly the start; a halved town is a step smaller.

**How a merchant helps one thrive.**

| You do | The ledger | Scale |
|---|---|---|
| Bring grain in a hungry month | bread rises | A Gislinge hold (660 kg) keeps 30 souls a month: a hamlet. A 24-ton ship of about 1030 keeps 1,100. Eight keels carry a town through a famine spring. |
| Call often, or fund a work (BRIEF_2 `works`) | calls raise room; granary: +60 days of bread; jetty: +2 berths; beacon: a day's warning; bank and ditch: guard x1.5 | 200 to 3,000 d |
| Bring a warning | the place hides half its stock and people: loot x0.5, captives x0.3 | outsail the fleet |
| Ransom its people; carry the burned-out | souls return or move; about a pound a head (?) | a boatload |

The honest limit: nine loaves in ten come from the fields. A merchant matters at the margin, where a town lives or dies. The grain run is the first hour's job and the last age's: one boat to a fleet, in one number.

## 2. Wars

One object, `War`: two sides, a cause, a theatre (ports, lanes), phases with days, an end. Each phase sets states the game already reads and posts needs (section 4). No battle is simulated: it is a day with an outcome, from the chronicle or one weighted draw, and it arrives as news.

| Phase | What it sets | What the merchant plays |
|---|---|---|
| Muster, 20 to 60 days | grain, horses, iron, sailcloth, tar keenly wanted; the levy takes hulls | **Supply** at 1.5 to 2.5 times the market; arms and horses are under the laws of 779, 805 and 864, so the best pay is unlawful. **Ruin:** your ship pressed unless a charter frees her. |
| A host or fleet moving | lane risk up; the fleet is a mark as old as your news | **News:** outsail it and sell the warning. **Envoys:** a passenger and a letter. |
| Siege, 10 to 330 days (Paris 885) | the port `besieged`: market shut to all but runners, bread falling | **Blockade running** by night on the built chase rules, at 3 to 5 times; or sell to the camp. |
| Sack or tribute | the built shocks; souls as above; silver moves | **Ransom:** silver in, people out. **Refuge:** monks, relics, books. **Profit:** loot cheap at the raiders' markets. |
| Peace | tolls reset; a new lord may hold the port (Dorestad 850, York 866, Rouen 911) | charters void or renewed; friends on the losing side now cost you |

**The real wars** are bound from the chronicle in a new `wars_793.json`: its 230 war, battle, campaign, raid, fleet and treaty entries make about 35 wars, each on its dates.

**The wars the world makes** come from four makers, each a monthly test on the ledgers.

| Maker | It fires when | True to |
|---|---|---|
| A raiding season | each spring, bands by the year's pressure, more when their home ports are hungry; a target is a place the band outnumbers 1.5 to 1 (built). From 834 a fleet of 4 to 30; from the 840s it may winter | the built raiders; `pressure.by_year` stops at 814 and must run to 1066 |
| A toll quarrel | a lord sets a new toll on a busy lane; merchants complain; ports shut; an army comes by boat | Charles and Offa about 790; Tiel 1018 |
| A border and a tribute | two realms with a grievance: one summer, a tribute, a peace | Franks and Danes; Svear and Curonians 854 |
| A succession | a ruler dies with two claimants and the chronicle is silent on that realm that year: 30 % (?) | the Danes, 812 and 854 |

The supply is endless because the ledgers never rest: hunger sends bands out, bands empty towns, empty towns cannot pay, an unpaid tribute is a grievance.

## 3. Changing history

A **hinge** is a chronicle entry with a test. Three new fields: `unless` (a condition on the ledgers; if it holds on the day, the entry's `else` fires instead), `needs` (entries it rests on; if one was turned, this one is dropped), `else` (an authored alternative). A turned hinge sets `changed_<event>` (CHAINS.md). A hinge is turned by a merchant's own acts, never by who wins a fight.

| Entry | To turn it, the world must be so | What follows |
|---|---|---|
| `834-dorestad-sacked` to `863-dorestad-last` | at each raid, guard over two thirds of the band's men (today 210; a bank, the count's men paid, a beacon); in 863 still 1,200 souls and half the calls of 793 | Dorestad lives as a size 3 town; `850-rorik-dorestad` needs it taken. The raids can be turned, the river cannot: it silts, and trade drifts to Tiel by 900 |
| `878-05-edington` | Athelney fed through the spring, or Chippenham instead (needs a landing on the Parrett) | if Alfred starves, `886-london-alfred` and some 35 later English entries drop: Danish kings, weighed silver, wars the ledgers make |
| 1066 (needs one new entry: the fleet sent home on 8 September, its provisions gone, ASC C) | Harold's fleet fed to Michaelmas: for 10,000 men (?) 7 tons a day, 140 tons, six loads of a 24-ton ship; or the news from the north carried south by sea; or William's horses carried | William, Harold or Hardrada. Only a house of ships can turn it: the point of the last age |

Later: Reric 808, Hamburg's books 845, Paris 885, London 994 to 1016, Vlaardingen 1018 (pilots and stores for the host from Tiel, or a warning to Dirk), Hedeby 1050.

**Later history follows in three layers.** Dependents drop by `needs`. Three to eight authored `else` entries carry the first consequences. Then the ledgers carry the change themselves: nobody writes "Dorestad in 950"; it is whatever its souls, bread and calls have made it.

## 4. Quests

A quest is a **need**: something a settlement, a lord or a person lacks, posted by the ledgers, with a day and a stake. `Need` is the built `Commission` plus a `cause`, a `stake` (what the ledger does if it is met, and if not) and an `asker` who remembers. Other traders fill needs too (the built `ai_fill_per_day`): the world does not wait.

| Cause | Asks for | Pays | Stake |
|---|---|---|---|
| hungry | grain by a day | 1.5 to 2.2 times the market, a deed | bread, souls |
| burned | timber, iron, tools | 1.3 times, a plot offered | leavers return twice as fast |
| besieged | grain by night | 3 to 5 times | how the siege ends |
| muster | horses, arms, sailcloth, tar | 1.5 to 2.5 times | the war's one draw |
| captive, envoy | silver in and a person out; a passenger and a letter | 80 to 250 d, standing, a charter | souls; peace 30 to 90 days sooner |

Three needs can be taken at once; the rest are rumours with an age. A need met at a hinge pays a finale and a grand reward (CHAINS.md). Tonight's chains stand on needs: `dorestad_834` begins because Dorestad is rich and unwalled, not because a counter reached three. **Stories end differently** because an ending reads the ledger: the town saved, held by a Dane, or in ashes. An unlock can be lost: a plot burns with its town.

## 5. Different starts

A start is a row in `starts_793.json`: year, home, people, tongues, ship, purse and debt, waters known, standing, a first need. The ledgers run from 793 to the start; the ships are made fresh on the day.

| Start | You are | What differs | Opens when |
|---|---|---|---|
| 793, Dorestat | a Frisian with a Gislinge boat, 480 d owed | the base game | from the first |
| 808, Sliesthorp | a merchant of Reric carried off by Godfred | weighed silver, Wends, no kin | you reached Reric before 808 |
| 841, Duiblinn | a trader in the raiders' ship-fort | captives are lawful goods; Christians mistrust you | you sold at a Norse market |
| 1000, Tiel | a man of Tiel with rights at London, partners, a 10 to 24 ton ship | towns, coin, the tribute years | three crossings sailed |
| the year a life ended | the heir of the last game's house | its chart, chest, feud and friend | always |

A start is one row of code and a great deal of writing: only the first has its waters written.

## 6. The campaign and its endings

The arithmetic first: 273 years at a game minute a second is 39,900 hours. So the campaign is nine or ten lives, and in each the player sails the seasons that matter. The winter council gets one more choice: **stay ashore, one to five years**. The ledgers run, the house's ships trade by their orders, each year is read back in five lines, and the count stops for anything of weight: a war in reach, a hinge two years off, a death. This is waiting by choice (DECISIONS 8), not a faster clock, and it is the user's to decide. Rough size: ten lives of six to eight sailed seasons, about 70 evenings; one age is a short campaign.

| Age | The rule of the sea | Hinges |
|---|---|---|
| I. The emporia, 793 to 833 | beach markets, the king's tenth, raiders few and far | Reric |
| II. The burning, 834 to 878 | fleets, winter camps, tribute; Danes hold tolls | Dorestad, Edington |
| III. Walls, 879 to 954 | forts and bridges, trade only in towns (928), Northmen lords at Rouen and York | Paris |
| IV. Silver and towns, 955 to 1016 | one coinage (973), the dirhams end, the tribute fleets | London |
| V. One king's sea, 1017 to 1066 | Cnut's peace, sworn merchants, then three fleets | 1066 |

**Endings, three layers.** A *life* ends by age, wreck or ruin and gets one line in the Chronicle's voice. The *house* ends at Christmas 1066, or sooner with no heir, weighed on what the ledgers hold: its rank, keels and plots, its chest, the souls alive in the places it kept. Each named end is something a merchant could become: a thegn by three crossings (?); a sworn brother of Tiel, or of the Frisian guild at Sigtuna with a stone raised (U 379, U 391); a moneyer with his name on the penny; a house gone into a minster with its books; a name nobody carries. The *world* ends as it was bent: the last screen is two charts, history's 1066 and yours.

## 7. The storyteller

It makes nothing happen. The world holds more than can be told; the storyteller chooses what **reaches** the player and when. What it passes over still happens, and comes later as news, as old as the ship that brought it. The age of news is the flood gate.

1. Every card, need, letter and piece of news carries a `heat`: 0 colour, 1 a choice, 2 a cost or danger, 3 a crisis.
2. It sums heat over the last three legs. The band: 2 to 5 a leg in stage 1 (the concept's one to three cards), up to 8 in stages 4 and 5, where letters and one matter of the house a season fill it.
3. It acts through one new factor on the built draw, **m_story**: under the band, heat 1 and more comes twice as often; over it, heat 2 and more a quarter as often. The scheduler, its seed and its tests stay.
4. Hard rules: never four real minutes of sailing with nothing to answer or see (the built `gap_real_minutes`); never two crises open; five quiet days after one; every danger shows hours ahead.
5. Never empty: when the line is dry it pulls the nearest unmet need, then a crewman's want, then a rumour of a far war with its price.
6. Caps on the world itself: four wars alive; one siege a year within ten days' sail; no port struck twice in 60 days (built). Bands a season, to tune: 1 to 3 in the 790s, 6 to 10 from the 840s to the 880s, 2 to 4 in the mid 900s, 6 to 10 from 991 to 1016.
7. Tested before anyone plays: the headless harness (`SimReport`) draws heat by the leg over twenty years and marks every gap and flood.

## 8. On a small interface

Nothing new on the table. The world shows in five places, in words, never numbers.

| Where | What |
|---|---|
| A port's mark on the chart | its size; a ring for its state as last known, with the age of the word |
| The port panel's first line | "Dorestat. A great town, thinning. Hungry since Candlemas. The count's peace. Wants grain and timber." |
| The day stick | your three needs as notches; a known war as a hatched stretch of days |
| One more chart layer, War | lanes at risk, a fleet's mark as old as the news, a town besieged |
| The log in the chest, two inks | black: the annals, with their sources. Byzantium: of this telling |

Words for souls: hamlet, village, market, town, great town; growing, holding, thinning, emptying. No realm screen, no war score.

## 9. The cost, in order

| # | Build | Nights of code | Data and writing | Then playable |
|---|---|---|---|---|
| 1 | `Settlement`, the monthly tick, size from souls, the port line and ring | 3 | `settlements_793.json`, 50 rows | towns grow, starve, recover |
| 2 | `Need` on `Commissions`; three notches | 2 | five causes, about 40 cards | quests from the world |
| 3 | `heat`, `m_story`, the harness page | 2 | a heat on every card | never empty, never a flood |
| 4 | `realms_793.json`: rulers, who holds each port by year, the lord's toll and law | 3 | about 30 realms | Rorik's Dorestad, Danish York |
| 5 | `War`: phases, port states, lane risk, the levy | 4 | `wars_793.json`; five card families | the real wars as play |
| 6 | Hinges: `unless`, `needs`, `else`; two inks | 2 | a night a hinge; Dorestad and 1066 first | history bends |
| 7 | The four makers; raid pressure to 1066 | 4, and tuning | wording for nameless wars | wars without end |
| 8 | Years ashore; lives and heirs (the winter screen is not built) | 3 | none | the campaign reaches 1066 |
| 9 | Starts, the endings screen | 3 | each start's waters | different starts, the end |

About 26 nights of code, and more writing than code. Steps 1 to 3 alone give a living world round the first twelve ports.

**Leave out:** souls by age, sex or craft; a sickness model (the chronicle's four epidemics and hunger are enough); production chains; treasuries, marriages and treaties between realms as a system; simulated battles; the succession maker, if step 7 runs long; every start and hinge after the first two.

**Where it is weak.** Most ports' souls are guesses. A changed England after 878 leaves 188 years to nameless wars, which will read thinner than history: ship hinges two at a time and play them before writing more. The pace of 273 years rests on years ashore. Tonight's chains gate on flags: to stand on the world they need condition variables `Cond.cs` lacks (`port.state`, `port.souls_word`, `port.bread`, `war.near`, `realm.at_war`).

**Decisions for the user:** years ashore; the two inks; made wars nameless only; which two hinges first; words, not numbers, for souls.
