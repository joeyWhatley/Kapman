# The living world: the least that works

*A proposal, 4 October 2026, night. One stance: build it from what already runs, with the fewest new systems. Every
number is a starting guess to tune; (?) marks what I could not check.*

## The idea

Three things are new: **a head count for each town**, **a note of who holds each port when**, and **a pacing rule**.
The rest is in the game already. A market's food price says whether a town eats. The chronicle and the raiders deal the
blows. Cards and flags carry wars, promises and turning points. Commissions are the quests. The news rule decides what
the player may see. Left alone, the world follows the annals; the player's weight is the distance between a town and
what history made of it.

## What runs today, and the gaps (read in the code)

| Part | Runs | Gap |
|---|---|---|
| Cards (`Core/Encounters`) | conditions, effects, flags (a number that can lapse), follow-up cards, one draw a call and one an hour, cool-downs | a flag cannot be added to; no card reads a town's state; nothing paces the draws; no card comes at a berth in the chart scene yet (`chart_game.md`); `passenger` does nothing |
| Chronicle | 582 dated entries come on their days; famine, raid and burning move markets (`shocks_793.json`) | 422 entries fall before 825, 160 after, and 130 of the 242 years from 825 have none; the 123 wars, battles and campaigns move nothing |
| Markets | stock, glut, dearness, shut days; size 1 to 5 falls when burned or ruined | size never rises; no head count anywhere |
| Other ships | 138 traders with beliefs; raider bands; king's men | a lost ship is never replaced; band numbers stop at 814; a port's realm stays as in 793 |
| Time | a year tick (`KapmanGame.YearPassed`); year dice for hard winters and herring (`Core/Wildlife/YearState`) | nothing listens to either |

## 1. Towns

**Data.** `people_793.json` (new): for each port a **true line**, dated head counts from the sources. Dorestad 2,500 in
793 (2,000 to 3,000 is the usual estimate: its dossier), near nothing by 870. Hamwic 2,000 to 3,000. Birka 500 to
1,000, gone about 975. Hedeby at least 1,500 in the tenth century. London perhaps 20,000 by 1066. Where no figure is
found yet, a band by the table's size: 1 under 150, 2 to 500, 3 to 1,200, 4 to 2,500, 5 above (?).

**State.** One number a port, `heads`, saved with its market. **The reckoning**, each Martinmas:

    heads += pull x (true line - heads) + heads x (calls + works) - losses

| Term | Read from | Number |
|---|---|---|
| pull | the true line this year | 15% of the gap when under it (people come back), 5% when over; none in a year of war or hunger |
| hungry month | on the 1st, food there costs 1.5 times its usual price or more (`Market.Price` against `UsualPrice`) | 1% of heads lost; at 2.5 times, 3% |
| calls | visiting ships against berths, looked at monthly (`WorldSim.Visitors`, `Berths`) | 0 to +1% |
| works | flags `unlock_work_<port>_<what>` | +0.2% each, to +1% |
| raid, burning | the day it falls | the captives taken (6 to 20, `raiders_793.json`) and 2%; half the heads |
| sickness, flood | chronicle kinds `epidemic`, `storm` | a tenth; 15% (?) |

**Size follows heads.** The market's size is the table's, plus a step for each doubling of heads from the line's first
figure, less a step for each halving, 0 to 7. A burning halves a town, which is the step its market loses today; with
the pull it trades a step smaller for four years, two if helped. Once a year the **muster** sets each home port's
traders by its size and replaces lost ships: a town that grows sends out more sails, a dead one none.

**One word for the player**, from last year's change: thriving (+2% or more), growing, steady, thinning, dying (-5% or
worse). Before it, a word for today: hungry, shut, burned, besieged, sick.

**How a merchant helps.** No new verb of play.

| Help | Done by | Changes |
|---|---|---|
| Feed it | selling food there while it is hungry; the dear price is the pay | the price falls back and the dying stops. A man eats 0.7 kg a day (`voyage_rules.json`): the first boat's 660 kg keeps 30 people a month, a 6-ton trader 285. One boat saves a hamlet; a town needs a house of ships |
| Take its orders | a hungry, burned or besieged town posts commissions (grain; timber, iron, tar; wine and wax), premium 0.4 to 0.8 | each one kept adds 5 points of pull that year, to 30 |
| Call often | lying there and dealing | calls |
| Pay for a work | the `works` of its places file (BRIEF_2) | wall and watch: defence +40 men, so a market stands off a band as a town does; granary: the first two hungry months of a year do no harm; well or hospice: sickness halved; jetty: a berth more |
| Warn it | a card that asks for fresh news of the band in the ledger | no captives, losses halved |
| Ransom its people | captives bought at the raiders' market and brought home | heads back, a deed |

## 2. Wars

A war here is not an army. It is **a state of some ports for some seasons**: one record (sides, ports, from, to, a tide
from -3 to +3, real or made) that pulls five levers the code has.

| Lever | Has | In a war |
|---|---|---|
| Bands | `RaiderBrain`: a home, targets, a class | each side sends 1 to 4 bands a season at the other's ports |
| Blows | `Shocks`: raid, burning, shut days | as they fall; a siege is a market shut by the month, each month a hungry one |
| Dear goods | `Market.Shock` | grain, salt meat, horses and arms 40% dearer in both lands; loot cheap at the winner's market |
| King's men | `Patrols` | twice as strict at both sides' ports |
| Word | news on the boards; flags | `war_<port>`, `siege_<port>`, `war_tide_<port>`, read by any card |

**The real wars** come on their dates. `wars_793.json` (new) binds the chronicle's entries into about forty wars with
sides, ports and span: Kent 796 to 798, Dorestad 834 to 863, the great army 865 to 878, Paris 885 to 886, the tribute
years 991 to 1016, the fleets of 1066. Their tide is history's. Twelve have chains already (`CHAINS_LIST.md`).

**Made wars** start when the storyteller wants one (7), between neighbours who really fought in those years
(`quarrels_793.json`, new: Mercia and Wessex to 829, Danes and Obodrites, Dublin and Leinster, Wessex and the
Danelaw). Each season the tide moves a step by a die weighted by each side's heads; the player may move it one more.
At 3 either way, or on the end date, the ending card comes.

| Kind | Lasts | Ends in |
|---|---|---|
| A sea-king's summer: 2 to 6 bands on one coast, a camp on an island | a season | paid off, beaten, or wintering |
| Two lords' feud: the toll taken twice | 1 to 2 seasons | a wergild, or one lord gone |
| A border war: bands both ways | 1 to 3 seasons | peace and hostages; a port changes hands |
| A rising against a lord | 1 to 2 seasons | pardon, or a burning |
| A siege of one town | 2 to 6 months | relief, tribute, or sack |
| A levy year: the king calls ships and food | a season | your boat, or its price |

**The merchant's part**: about 24 cards written once, each reading the war's flags at the port she lies at.

| Part | The card | Pay and cost |
|---|---|---|
| Supply | a steward orders grain, meat, horses, arms (`commission`, premium 0.5 to 1.0; the arms laws may forbid it) | 200 to 800 d a season; the tide a step |
| Blockade | ships lie across the river: run it in fog or dark, pay the watch, or turn back | inside, food at two to three times its price; caught, the cargo is seized |
| Envoy | carry a man or a letter across by a day | standing, a charter; the other side's king's men |
| Ransom | take silver north and bring named captives home | 80 to 250 d a head from kin or church |
| Refuge | take people, a minster's books or its plate out before the blow | texts for the chest, a friend, a hold full of people |
| Profit | buy loot cheap, sell to both sides | the best margins in the game; standing lost as fast as the news sails |
| Ruin | your plot burns, a debtor dies, your boat is called up, cargo is taken at the king's price | a loss the house carries; a warning card always comes first |

**The honest limit.** BRIEF rule 1 forbids invented history. So a made war is small, its leaders are invented ordinary
men (a jarl, a reeve, a claimant's cousin), no named king does what he did not do, and it is shown as news, never among
the annals. Made wars will feel alike after a dozen unless each card has several lines by kind, tide, people and year:
about 150 lines of writing.

## 3. Changing history

Three devices, and no second chronicle:

1. A chronicle entry may carry `unless: <flag>` (it does not come) or `only_if: <flag>` (an annal that comes only then).
2. A town's true line may carry a second line for a flag.
3. An entry may say who holds a port from its day (`holds`); a flag may switch that.

After a hinge turns, prices, traders, the raiders' targets and the towns go on by themselves from the changed state.
So a hinge needs only three to eight new annals, in the Chronicle's voice, always shown apart as "Otherwise than the
annals tell". An event is a hinge when it lies at one of the game's ports, when a merchant's food, silver, warning,
wall or ship could bear on it, and when what follows stays within towns, tolls and trade. Crowns do not change, except
in the last year.

| Hinge | The annals | Turned if | What follows |
|---|---|---|---|
| Dorestad, 834 to 863 (`dorestad_834`, `rorik_850`) | sacked 834, 835, 836, 837; burned 846; the last sack 863; empty soon after | before that entry comes, two of three: the wall and watch paid for (about 2,400 d (?)); 1,200 heads or more; Rorik's man warm to the house | `changed_dorestad_863`: the last sack does not come; the line runs on to 900 heads in 900 and 1,500 in 1066; its last year is lifted. The river left Dorestad too, so this is a gift to the player, and the notes say so |
| Utrecht, 1006 (`1006-tiel-burned`) | Tiel burned; Utrecht's people burn their own trading quarter (Alpert of Metz) | your boat brought the news from Tiel within three days, or you paid for the bank wall | the quarter stands; its market a step larger for a generation |
| The fleet, 1066 (`year_1066`) | Harold's ships go home on 8 September, their food gone (ASC C); William lands on the 28th | the house lands 36 tons of food on the south coast between Lammas and that day: 2,500 men for twenty days (?), six cargoes of the 6-ton trader | the fleet keeps the sea and the crossing is fought on the water: an ending. Or feed the duke, or carry Tostig's men from Flanders: two more |

Next, with the same devices: Reric 808, Quentovic 842 (the houses that paid were spared), Kent 796 to 798. **Not
hinges**, and the game says so: Birka's end about 975, Kaupang's, the silting of harbours. No merchant could turn them.

## 4. Quests

A quest is a promise with a day, and the game has one already: the commission (a port, a good, a count, a day, a
price; kept or broken is a deed; other merchants may fill it first). Three shapes cover everything:

| Shape | Built from | Example |
|---|---|---|
| Bring | `commission` | 40 sacks of wheat to a hungry town by Lammas, premium 0.6 |
| Carry | `passenger` (to finish), a follow-up card at the port | a widow's ransom to Sliesthorp; a letter to York |
| Find out | the ledger: news of a port no older than so many days | "Has my ship come to Lundenwic?" Knowledge is the proof |

They come from a town's state (1), a war (2), a person's `want` in a places file, a crewman's want, and the chains. At
most three are open; each shows its day and pay before it is taken; a broken one costs standing and never ends a
story. Pay follows `CHAINS.md`: a town's order 20 to 120 d over the market, a war order 200 to 800 d. Lasting rewards
hang on the standing the game keeps: well regarded (30) opens credit and a plot, trusted (60) a charter and a factor.
A kept order is worth 4 points: eight make a stranger well regarded.

## 5. Different starts

A start is one row of `starts_793.json` (new): year and day, home, people and tongue, ship, silver, debt, standing,
ports known, flags, first card. A later start needs no run of the years between: each town's heads are read off its
true line, the chronicle's lasting effects (holds, ruin) are replayed in silence, and the markets and the fleet are
made for that year, as they are now for 793.

| Year, place | Who | The burden |
|---|---|---|
| 793, Dorestad | a Frisian with a Gislinge boat | 480 d owed to the count (built) |
| 808, Sliesthorp | a merchant of Reric carried off by Godfred | no home; a king who owns your plot |
| 834, Dorestad | the heir of a burned storehouse | the house's name and debts, no boat |
| 866, Eoforwic | a trader in the great army's train | heathen buyers, weighed silver |
| 886, Lundenwic | an Englishman in Alfred's London | a plot by the wall, Danes across the Lea |
| 991, Gipeswic | a merchant in the year of Maldon | the tributes; silver flowing north |
| 1020, Ripa | a Dane under Cnut's peace | a 6-ton trader, rivals already rich |

**Unlockables.** A later start opens when a campaign reaches its year or its war's chain is finished. A small profile
file beside the saves keeps them, with the ships won (`unlock_ship_`) and one heirloom carried into a new game. Each
start costs writing, not code: its ports' `eras` and people must be true for that year. Ship three first.

## 6. The campaign and its endings

**The honest sum.** 793 to 1066 is about 10,000 hours at 4x (`economy_and_trade.md` 2.0). So the campaign is lives, and
a life is the seasons the player chooses to sail. Winter is already one screen where the date jumps. The least
addition: at the winter council, "stay ashore until" a spring, the heir's fifteenth year, or a notch on the day stick.
The date jumps as winter's does; the clock's rate never changes. In the years ashore the chronicle comes, the towns
are reckoned, wars run by dice without cards, handed-over routes pay by a yearly roll, and one card a year reads it
back. Nine or ten heirs reach 1066; at three seasons a life that is about thirty evenings.

**Five ages**, each with its own danger: the emporia (793 to 833), the raids (834 to 885), the new towns (886 to 954),
silver and tribute (955 to 1016), the sea kings (1017 to 1066).

**The endings.** The house is weighed at Christmas 1066, or sooner if it has no heir and no silver. Each ending is read
from what the save holds; none needs a counter of its own.

| Ending | Read from |
|---|---|
| The thegn: a king's merchant with a seat in hall | `unlock_title_`, three crossings, trusted at a royal seat |
| The sworn brother among the merchants of Tiel | the title flag; charters held |
| The town that stands: Dorestad alive in 1066 | `changed_` flags; a town's heads above its true line |
| The house of ships | keels, factors, silver |
| The book: the last heir takes the cowl, the chest goes to a minster | texts, riddles solved, alms |
| The north road: the house gone to Dublin or the Baltic | where its plots and standing lie |
| Harold's man, the duke's man, or Hardrada's | the last hinge |
| The line ends | told as the Chronicle would tell it |

## 7. The storyteller

One small class, seeded like every other draw, with five rules.

1. **Truth first.** A chronicle entry always comes on its day. The storyteller never moves, hides or invents one.
2. **The season's count**, each spring. Within the house's reach (ports heard of, ports with a plot) it adds up what
   the season already holds: a war counts 3; a raid, burning or famine 2; an open chain step 2; a promise or any
   other entry 1. **Under 4** it starts one thing in reach: a made war (4 times in 10), a town's trouble from the year
   dice (3), a rival house's move (1.5), a windfall such as a wreck's cargo (1.5). **Over 12** it starts nothing, chain
   starts weigh a third, and towns post one order, not three. Before 825 it mostly thins; after, it starts something
   most years.
3. **The leg's count.** One to three cards a leg. After any card, six game hours with the hourly draw at a fifth. A
   call shows at most two cards beyond the toll man. Twelve hours under way with none: the next draw is tripled.
4. **The pulse.** One number, 0 to 1: what the house lost lately (a man, a cargo, a third of its silver, the home port
   raided: +0.3 each), halving every 20 days. Above 0.6, cards marked `danger` weigh a third and cards marked `kind`
   twice. Under 0.2 for 30 days while the silver grows, danger weighs half again. After a war or a chain ends, ten
   days with nothing started.
5. **It grows with the house.** By keels (1, 2 to 3, 4 or more) the floor and ceiling rise (4 to 12, 6 to 14, 8 to
   16), and what it starts shifts from the boat's troubles to the house's: a factor's letter, a levy, a rival.

Caps: one war of its making in reach, two chains open, three promises. Test: a headless run of twenty years that
prints cards a leg and things a season.

## 8. On a small interface

Nothing new lies on the table, and there is no new screen.

| Where | Added |
|---|---|
| A port's mark on the chart | its dot by heads, and one sign: a ring (war), a flame (burned or shut), hollow (hungry), grey (ruin). As old as the news: "9 days ago" |
| The port's panel | one line under the name: "About 2,400 souls. Hungry: the grain is half gone." |
| The day stick | up to three promises, a war's season end, the next chain step |
| The log on the map | three headings: The annals (with sources); Heard (news and made wars, with source and age); Otherwise (annals the house changed) |
| The ledger | promises, each with its day and pay |
| The winter screen | the year read back: a line for each town known (up or down, and why), a line for each war |

## What a programmer builds, in order

Needed first, and not this proposal's: cards at berths and at sea in the chart scene, the winter screen, the heir.

| # | Step | About | Then playable |
|---|---|---|---|
| 1 | Engine bits: `flag <name> +n`; `here.<stem>` (the flag `<stem>_<port id>`); a chronicle entry sets `c_<kind>_<port>` for a span; `unless`, `only_if`; a card's `mood` | 150 lines, 1 night | cards that answer a raid, a famine, a king's death |
| 2 | Towns: the table, heads in the market, the reckoning, size by doublings, the port's line, state orders, `port.heads`, a `people` verb | 350 lines, 2 nights | towns starve, burn and recover; grain matters |
| 3 | The leg's count and the pulse | 120 lines, 1 night | no empty legs, no floods |
| 4 | Holds: who holds a port by date; `port.realm` reads it | 120 lines, 40 dated rows, 1 night | York is Danish in 867 |
| 5 | Wars: the record, five levers, the tide, 24 cards | 300 lines, 3 nights | real wars to play through |
| 6 | The season's count, made wars, year dice wired | 200 lines, 2 nights | the years after 825 fill |
| 7 | Three hinges | 60 lines, 20 annals, 1 night | Dorestad can live |
| 8 | Starts: the table, Dorestad taken out of the code, the silent replay | 150 lines, 1 night | three starts |
| 9 | Years ashore, the muster, the endings | 300 lines, 2 nights | 793 to 1066 |

About 1,750 lines and fourteen nights, and beside them the data: 50 true lines; 40 wars' rows; and about 150 more real
annals for 825 to 1066, the best spend on this page, because truth is cheaper than invention.

**Left out for good:** a town's wealth, classes of people and production chains (the food price already says whether
it eats); realms with treasuries and armies (a realm is the list of ports it holds); armies on the chart; a second
chronicle; a quest log; a headless run for later starts. **Can wait:** four of the six made wars (begin with the
sea-king's summer and the siege), starts beyond three, endings beyond five.

**Yours to decide:** made wars as small news-wars, or real wars only; Dorestad living past 863 (`economy_and_trade.md`,
question 5); "stay ashore until" at the winter council, since the rule against time warps is yours; a town's count as
a number or only as a word.
