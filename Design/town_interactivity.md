# How a town plays

4 October 2026. The user asked for "town interactivity". This page says how a port call works on the chart.
`Assets/Kapman/Game/Data/story/town_actions_793.json` says what each of the 53 acts is (the period fact under it, its
cost, hours, condition and effects); the place files (`Data/places/<port>.json`) say which sites a port has and what
is done at each. Beside it: `story/unlocks_793.json` (what an act can win, and its price), `works_793.json`,
`town_games_793.json`. A draft for you to overwrite; every number is a first guess.

## 1. Arriving

She is moored, and from here only acts and waiting spend time. In order:

1. **The reveal.** One line on what changed since your word of the place: "Three Frisians got in first. Wine is down a third."
2. **The arrival set**, at most two cards, as built: the toll man where there is a toll ("As last time" on a later call), the berth only when it matters.
3. **The town panel** opens beside the chart, never over it: the port's picture with its sites on it, and the day stick with the hour, a Sunday or feast, and what the day still holds.

## 2. The sites are the menu

There is no list of verbs. You click the place itself: the dam heads, the toll house, the church, the ale-house. A
site opens as a small panel: what is seen there, who is there, and what can be done: the market where goods are sold,
its people, its cards, and its acts as buttons with price and hours on them ("Mend the boat. 8 d. 4 hours."). An act
that is shut stays in its place, greyed, with the reason in plain words. Acts need no pictures: the site's is enough.

## 3. People, and three talks a call

Hearing is the game, so it is rationed: **three talks a call.** A talk is one conversation with a person of a site,
or one of three asking acts: the keeper of the port (who is in, who left and for where, the tide: sure), a skipper
just in (news and prices as old as his passage: told), the benches (rumours: doubtful). Each talk is **plain**, **warm**
(one thing more; he may ask a favour) or **hard** (only what you asked; he likes you less). Each dawn she still lies
there gives one talk back. Mass, a feast and a round of ale each give a piece of news without a talk: time buys what
talk cannot. People remember: the host of three nights will swear with you in court. (To settle: the town-games table
counts a full game as a talk; the short dice, tafl, song, riddle and ale acts here do not.)

## 4. What a day's hours allow

The clock is held while you look, trade and talk. **An act takes hours, and the clock moves on by them when you choose
it**; the day stick shows the cost first. Work runs from hour 6 to 18, the evening to 23 (fixed for now).

| Takes | Acts |
|---|---|
| no time | pay the toll, show a charter, hire carriers, take an order, the three asking acts |
| half an hour | haggle (once a call), try the goods, weigh or test silver, change coin, witnesses, alms, a vow, an offering, the lots, a gift, an oath, ship's tools, a pilot, store room |
| an hour | Mass, hear the law, hire a hand, dice, tafl, a song, riddles, a round, stores, your own store, a ferry, order a copy, fund a work, a name remembered |
| two hours | load or unload (half an hour with carriers), read a book, an audience, buy or order a ship, a plot, the leech, buy from the maker |
| three or four | bury a man, bring a case, mend the boat, fit her out, hold a feast |
| twelve | scrape and tar her: two tides |
| to dawn; until it turns | lodge ashore; wait for a wind |

A working day holds the cargo, one big job and the small things, then two or three hours at the hall or the
ale-house. A call can be an hour or a winter.

## 5. What shuts

| When | Shut | Open, or more so |
|---|---|---|
| **Night** | everything with an hour on it: market, toll, mint, yard, court, church, carriers (the ale-house and the hall at 23) | lodging, sanctuary, a vow, the leech, waiting for a wind, casting off |
| **Sunday**, Christian ports | all work and all dealing: cargo, wright, tar, carriers, market and haggling, mint, scribe, witnesses, court and law, smith, yard, store, plots, makers (19 acts: `rules.sunday.shut`) | Mass, alms, talk, the ale-house after Mass, the hall, books, stores (food may be carried), the ferry for people |
| **A feast**, Christian ports | the same as Sunday | the lord is in his hall: an audience without waiting; a feast costs half and counts double; the singer is free; a fair keeps its market open |
| **A fast** | feasts, the court | fish at the guest table; alms count twice in Lent |
| **Ice** | waiting for a wind: it becomes wintering | crossing on foot; the yard builds; tarring costs no hours |

A heathen port has no Sunday. The rule is the period's own: the Admonitio generalis of 789 forbids work, building and
courts on the Lord's day and allows three cartings only (for the army, for food, for a burial); in the Frisians' law
Sunday work costs 4 solidi. The engine has no Sunday yet: until it has, these acts shut on feasts only.

## 6. A landing is not Dorestad

| | Dorestad (emporium, size 5) | A landing (village, size 1) |
|---|---|---|
| Sites | eight to twelve along a long ribbon of bank | three: where she lies, a farm or hall, a holy place |
| Officers | the king's toll man, the mint, the court | none: one farmer or one priest is the lord |
| Buying | a market on the dams, a tenth to the king, witnessed by the toll | from the maker: cheap, little of it, untolled, unwitnessed |
| Silver | the king's pennies only; other coin to the mint at a tenth's loss | what he will take: often goods for goods |
| The boat | a wright, carriers, a yard | your own hands and kit, on the sand |
| Talk | many ships, far prices, three talks never enough | one true thing, about the place itself |
| Night | a host, for a penny | under the sail, unless you are taken in |

## 7. Four calls

**Dorestad, a working day in July 793** (the famine year; home; standing "known"). In from Lundenwic at hour 9 with
wool cloth and tin. The toll man: "As last time." *The dam heads:* carriers (2 d) unload while you work. *The keeper*
(talk 1): two Frisians left for Lundenwic yesterday with wine. *The market on the dams:* try the wine (one cask in
five is sour), haggle and win, buy four casks; the tenth comes off your cloth. *The mint:* English pennies into
deniers, a tenth lost. *The beach by the dam heads:* the wright mends her (8 d, 4 hours). *The church above the
town:* alms, which count twice in a famine. Evening, the ale-house: a round, then the benches (talk 2): Quentovic is
said to be short of querns. Lodge. Dawn gives a talk back; stores for six days; cast off. One day, one evening, 16 d
besides cargo, toll and stores.

**Lundenwic, from a Saturday evening.** Run up on the strand at hour 17: too late to unload. The ale-house, a skipper
from Gipeswic (talk 1). Lodge. **Sunday:** no market, no carriers, no court. Mass at St Paul's inside the old walls
(the priest gives out the news), alms, two hours with a book at the minster, a gift at the hall (12 d) for tomorrow.
**Monday:** unload, change deniers for Offa's pennies, sell the wine, a twentieth to the reeve; buy wool cloth
**before two true men or the wic-reeve**, as the law of Kent has asked since about 680, because unwitnessed goods can
be claimed as stolen. The audience, sure after the gift: the reeve speaks of a plot on the strand when you are better
known. Two nights for one day's trade, and the day stick showed the Sunday before the course was set.

**Birka, late summer** (heathen; silver by weight). No toll man: the king's man looks for a gift worth a fiftieth of
what you will sell. *The black earth:* weigh on your own scales, bought at Dorestad: no loss. Test what you are paid:
one ring is copper inside, and the buyer makes it good. Furs from the hunters themselves, cheap and unwitnessed.
*The hall:* tafl with your host, lost well; he likes you better. *The grove:* the crew want the lots cast for the
passage home, and they fall foul: wait three days (a talk back each dawn, a feast the second evening) or sail with
heavier hearts. Your one Christian hand stays by the boat through all of it. Leaving, a pilot for the skerries (6 d).
In winter the lake is ice: goods come by sledge, and a ship is ordered here.

**A beach: Doccinga, June 793.** A church on a mound in the salt marsh where Boniface was killed, a farm, a creek. No
toll, mint, market, wright or carriers. *The creek:* she takes the ground on the ebb and the four of you scrape and tar
her dry. *The farm:* cheese, butter and Frisian cloth from the maker, for querns and iron as gladly as for silver. *The
church:* the priest wants wax and wine and gives an order for them; Mass; a vow for the crossing. The one talk tells the
one thing worth knowing: the Saxons beyond the Lauwers have risen and burned churches. Sleep under the sail. On 5 June,
the martyr's day, pilgrims come (likely; not recorded) and nobody works.

## 8. What must be coded, in order

1. **Load the tables.** `town_actions_793.json` and `places/*.json` into GameData; parse each act's `when` (Cond.Parse), `effects` (Effects.ParseList) and `cost` (Effects.ParseCost) at load and refuse errors, as the card loader does. A test: every act of BRIEF_2 has an entry; every site's `do` names a real act, person or card.
2. **The town panel.** The sites as places to click (a list until the picture is drawn) and the site panel. It replaces "Explore the town" and "Talk to people" on the port menu; "Cast off" stays.
3. **The act runner.** Check `when` (greyed, with the `refused` line), pay the cost, move the clock by `hours` (as Hoards.Dig does, so the hourly draws still run), apply `effects` through Effects.Apply with the act's `who` forged at this port, show the `done` line and the facts.
4. **The day.** Hours on the day stick; a talk counter in EncounterState (three a call, one back each dawn); PortLife.Talk split into keeper, skipper and benches, with the three attitudes.
5. **Variables.** `sunday` (Calendar.Weekday is there), `dark` (Rest.Dark), `fast` (named, not filled), `talks_left`, `port.size`, `port.zone`, `fair`. Then "and not sunday" goes beside each feast clause.
6. **Acts that only need wiring:** the market, pay_toll and show_charter, change_coin (PortLife's exchange), provision (Voyage stores as days), wait_wind (the TideWait clock, on the wind), lodge (Rest to dawn), hire_crew, commission, read_text, song and riddles (the chest).
7. **Small new state.** The by-port flags, the pilot, the vow and the lots, the kit and the scales; the ship's hull and fouling.
8. **A store by port:** a second Hold at a port for store, rent_store and buy_plot.
9. **Ships:** buy, build (a day due and a pick-up card), fit out; prices and fittings from `story/unlocks_793.json`.
10. **The law:** `law hearing` in GameEffects (only seizure acts today); the hearing, sanctuary and claim cards.
11. **Works:** part-paid sums, the day done, the change to the town.
12. **For the spine:** nine deeds and the heathen feasts (the table's `needs`).

Steps 1 to 4 make a town playable with what the engine has: 41 of the 53 acts already carry effects in its verbs.
Each later step adds a handful of acts and leaves the game running.
