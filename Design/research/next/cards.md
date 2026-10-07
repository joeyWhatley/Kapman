<!-- 7 Oct 2026. One agent, no sub-agents. Read: GAME_MAKER.md, maker/heart.md, fate_faith_and_self.md, both critiques
(the card sections), DECISIONS.md, living_world.md, npc_text_encounters.md 2.3, and the engine (Scheduler, Cond, Effects,
EncounterDef, EncounterState, EncounterVars, EncounterLoader, EncounterDirector, EncounterGameVars, GameEffects). Ran:
Bake/content/card_census.py (written for this page). The web: the session's search budget was already spent, so no
search was made; fourteen pages were fetched by name and are listed at the foot. (?) = not checked against a source.
(g) = a first guess for the soak. Unity and git were not run. -->

# Choosing the right card: how Kapman decides what happens next

## The short of it

1. **The scheduler has dice and no judgement.** One Poisson draw a port call, one an hour at sea, a cool-down and a
   "once". It has no order of need, no budget, no idea which chain is open, and it drops whatever it cannot show.
2. **The content has outgrown it.** The census counts 1,392 cards. 420 can ever open in the first release, 248 of
   them with a real stake. At Dorestat the weights of the cards that can be drawn add up to 57 with no flag set and
   238 with every flag set, where the design meant about 0.7. A card comes on every call, and a chain step written at
   weight 40 takes the one draw seven to nine times in ten.
3. **Nine chains can start in the release and can never end in it.** Their first cards open at one of the twelve
   ports; every ending is at a far port or a late year. They must be shut before a player meets them.
4. **No card asks who you are.** 0 conditions read `me.*`; the grammar has no such name. 226 of the 420 open cards
   ask only for a place, a time and a flag.
5. **The answer is four tiers and a hand.** *Must* cards come on their day, in order. *Should* cards (a chain's
   next step) come at the next chance, picked by need and not by dice. *May* cards are drawn under a budget. *Colour*
   never stops the clock: it is a lit head or a log line the player may open. What is over the budget is not dropped:
   it waits as a row under People here. That is how the frame gets "lots of popups" without a flood: a few are pushed
   and the rest are offered.
6. **The deck sets the budget, not the wish.** 69 loose sea and crew cards have a stake. With none repeated inside a
   year that is one a day under way, two days in three. Signs, the field and the offers must carry the rest of the
   leg. Cards are the salt of a crossing. They cannot be its bread.
7. **Tasks can be built from data the engine already saves.** `EncounterState.scheduled` holds every promised step
   with its card, day and port: 137 `schedule` effects sit in the open cards, 133 with a port, and no screen shows one.
8. **About twelve nights of code** (section 6), no new card format, one optional field, one small table.

## 1. What picks a card today

Read from `Scheduler.cs`, `EncounterState.cs` and `EncounterDirector.cs`.

| Moment | The rule | The numbers |
|---|---|---|
| She moors | The arrival set: every `arrival` card whose `when` holds, in the file's order | 2 at most |
| She moors | One visit draw over every harbour card that holds: a card comes with `1 - exp(-sum of weights)` and is card i with `weight_i / sum`. Weight is times `m_port` (0.4 to 1.5) and `m_standing` | one card |
| Each game hour | One draw over sea cards (rate; a third at anchor), crew cards (rate) and, after 24 hours moored, harbour cards at weight / 72 | one card; dropped if another waits |
| A choice | `then`: the named card opens next. `schedule <card> in <days> [at <port>]`: it goes in line on that day, at that port. Its `when` is not asked again | no limit |
| Repeats | `once`; `cooldown_days` (the middle one of the open cards is 60 days) | |
| The pace (7 Oct, `EncounterDirector`) | `callDrawMax` 1 in the chart; `seaGapHours` 4. A card over the pace is let go unshown | 6 a day at sea at most |

What it lacks: tiers of need; a budget for a day or a season; a memory of decks ("fog, fog, fog"); any notion of a
chain being open; a place for a card that lost the draw; a hint for the player of where a step waits; a reason shown
for why a card came; and any name for the player's own people, faith, past or stats.

Three faults the census found in passing:

- **Stage `long` never draws.** `HourlyWeight` gives it no weight. Four dated cards of the first life with a rate
  (the ports shut, the Frankfurt price, Offa's letter of 796, the arms ban) cannot come.
- **Seven sea and crew chain steps carry a `weight`, where the scheduler reads a `rate`.** They never come:
  dirham_hoard s03 and s07, lindisfarne_book s04, s06 and s07, silver_road s04 and s10.
- **A waiting step waits for ever, in silence.** 133 scheduled steps name a port. If she never calls there, nothing
  lapses and nothing says so.

## 2. The census

`Bake/content/card_census.py` reads every card file and works out, by the same grammar as `Cond.cs`, whether a card's
`when` can hold at one of the twelve ports (Dorestat, Traiectum, Walichrum, Lundenwic, Witla, Medemelacha, Doccinga,
Daventre, Quentovic, Sarre, Fordwic, Ripa) in 793 to 805, in one life. It then goes round by round: a card is **open**
when its `when` can hold, something can bring it (arrival, a weight, a rate, or an open card's `then` or `schedule` at
a port of the twelve), and every flag it needs is set by some open card. Run it with:

    & "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\card_census.py

**real**: a choice costs or gains 10 d or more, goods, a man, the law, the toll, an order or a hire. **door**: no hard
choice, but one gives a text, a riddle, a lesson, a tongue, a passenger, a deed, news, a price shock or an `unlock_`
flag. **soft**: only heart, health, feelings and plain flags. **keyed**: a choice shows only for what you hold or
whom you know. **far**: shut by place or year. **cut**: its flag or its `schedule` comes from a card that is shut.
**nodoor**: nothing brings it. **nodraw**: open, but stage `long` with no draw in the engine.

It is generous on purpose. It takes every flag an open card can set as reachable, does not test option checks, and
does not ask whether a delay runs past 805. The true numbers are a little lower.

### By deck

| deck | cards | open | real | door | soft | keyed | far | cut | nodoor | nodraw |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| the region decks (8 files) | 548 | 112 | 87 | 14 | 11 | 70 | 430 | 6 | 0 | 0 |
| the chains (43 files) | 595 | 130 | 106 | 7 | 17 | 61 | 373 | 85 | 7 | 0 |
| crew | 112 | 108 | 26 | 62 | 20 | 19 | 4 | 0 | 0 | 0 |
| harbour (the toll man) | 1 | 1 | 1 | 0 | 0 | 1 | 0 | 0 | 0 | 0 |
| long | 73 | 7 | 7 | 0 | 0 | 3 | 61 | 5 | 0 | 4 |
| sea | 63 | 62 | 21 | 4 | 37 | 15 | 1 | 0 | 0 | 0 |
| **ALL** | **1392** | **420** | **248** | **87** | **85** | **169** | **869** | **96** | **7** | **4** |

Of the crew's 62 "door" cards, 44 are the evening song (each gives a text and asks nothing).

### By file

| file | cards | open | real | door | soft | keyed | far | cut | nodoor | nodraw |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| baltic_east | 82 | 0 | 0 | 0 | 0 | 0 | 80 | 2 | 0 | 0 |
| chain_amber_road | 14 | 1 | 1 | 0 | 0 | 0 | 11 | 2 | 0 | 0 |
| chain_barrow | 13 | 0 | 0 | 0 | 0 | 0 | 6 | 7 | 0 | 0 |
| chain_black_stones | 14 | 6 | 6 | 0 | 0 | 4 | 5 | 3 | 0 | 0 |
| chain_cnut_1016 | 13 | 0 | 0 | 0 | 0 | 0 | 12 | 1 | 0 | 0 |
| chain_coast_watch_810 | 14 | 0 | 0 | 0 | 0 | 0 | 13 | 1 | 0 | 0 |
| chain_dirham_hoard | 14 | 2 | 2 | 0 | 0 | 0 | 10 | 0 | 2 | 0 |
| chain_dorestad_834 | 14 | 0 | 0 | 0 | 0 | 0 | 12 | 2 | 0 | 0 |
| chain_great_army_865 | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_house_debt | 14 | 14 | 13 | 1 | 0 | 6 | 0 | 0 | 0 | 0 |
| chain_house_heir | 14 | 14 | 11 | 0 | 3 | 7 | 0 | 0 | 0 | 0 |
| chain_house_marriage | 14 | 10 | 9 | 0 | 1 | 6 | 4 | 0 | 0 | 0 |
| chain_house_new_ship | 14 | 2 | 2 | 0 | 0 | 1 | 5 | 7 | 0 | 0 |
| chain_house_rival | 13 | 12 | 10 | 0 | 2 | 9 | 1 | 0 | 0 | 0 |
| chain_kentish_revolt_796 | 14 | 13 | 12 | 1 | 0 | 5 | 1 | 0 | 0 | 0 |
| chain_lindisfarne_book | 13 | 1 | 1 | 0 | 0 | 1 | 8 | 1 | 3 | 0 |
| chain_lost_brother | 14 | 0 | 0 | 0 | 0 | 0 | 8 | 6 | 0 | 0 |
| chain_maldon_991 | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_mission_north | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_old_gods | 14 | 12 | 9 | 0 | 3 | 4 | 2 | 0 | 0 | 0 |
| chain_pilgrim_road | 14 | 13 | 11 | 0 | 2 | 5 | 1 | 0 | 0 | 0 |
| chain_pilots_secret | 14 | 14 | 10 | 1 | 3 | 6 | 0 | 0 | 0 | 0 |
| chain_relic_theft | 14 | 0 | 0 | 0 | 0 | 0 | 6 | 8 | 0 | 0 |
| chain_reric_808 | 14 | 0 | 0 | 0 | 0 | 0 | 13 | 1 | 0 | 0 |
| chain_riddle_trail | 13 | 3 | 2 | 1 | 0 | 2 | 8 | 2 | 0 | 0 |
| chain_rollo_911 | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_rorik_850 | 14 | 0 | 0 | 0 | 0 | 0 | 12 | 2 | 0 | 0 |
| chain_saga_danes_norway | 13 | 0 | 0 | 0 | 0 | 0 | 13 | 0 | 0 | 0 |
| chain_saga_england_south_east | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_saga_neustria_west | 14 | 0 | 0 | 0 | 0 | 0 | 13 | 1 | 0 | 0 |
| chain_saga_northumbria_irish_sea | 14 | 0 | 0 | 0 | 0 | 0 | 12 | 2 | 0 | 0 |
| chain_saga_rhine_frisia | 14 | 1 | 1 | 0 | 0 | 0 | 8 | 5 | 0 | 0 |
| chain_saga_rhineland_flanders | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_saxon_exiles_804 | 14 | 1 | 1 | 0 | 0 | 0 | 6 | 7 | 0 | 0 |
| chain_silver_road | 14 | 1 | 1 | 0 | 0 | 1 | 9 | 2 | 2 | 0 |
| chain_storm_vow | 14 | 9 | 3 | 3 | 3 | 4 | 5 | 0 | 0 | 0 |
| chain_sword_smuggle | 13 | 0 | 0 | 0 | 0 | 0 | 5 | 8 | 0 | 0 |
| chain_the_bell | 14 | 0 | 0 | 0 | 0 | 0 | 11 | 3 | 0 | 0 |
| chain_tiel_1020 | 14 | 0 | 0 | 0 | 0 | 0 | 10 | 4 | 0 | 0 |
| chain_walrus_ivory | 14 | 0 | 0 | 0 | 0 | 0 | 9 | 5 | 0 | 0 |
| chain_wine_fleet | 14 | 1 | 1 | 0 | 0 | 0 | 8 | 5 | 0 | 0 |
| chain_wreckers | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_year_1066 | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| chain_york_954 | 14 | 0 | 0 | 0 | 0 | 0 | 14 | 0 | 0 | 0 |
| crew | 112 | 108 | 26 | 62 | 20 | 19 | 4 | 0 | 0 | 0 |
| danes_norway | 82 | 9 | 8 | 1 | 0 | 7 | 72 | 1 | 0 | 0 |
| england_south_east | 85 | 33 | 26 | 4 | 3 | 27 | 52 | 0 | 0 | 0 |
| harbour | 1 | 1 | 1 | 0 | 0 | 1 | 0 | 0 | 0 | 0 |
| long | 73 | 7 | 7 | 0 | 0 | 3 | 61 | 5 | 0 | 4 |
| neustria_west | 57 | 11 | 7 | 2 | 2 | 3 | 45 | 1 | 0 | 0 |
| northumbria_irish_sea | 83 | 0 | 0 | 0 | 0 | 0 | 82 | 1 | 0 | 0 |
| rhine_frisia | 119 | 59 | 46 | 7 | 6 | 33 | 59 | 1 | 0 | 0 |
| rhineland_flanders | 40 | 0 | 0 | 0 | 0 | 0 | 40 | 0 | 0 | 0 |
| sea | 63 | 62 | 21 | 4 | 37 | 15 | 1 | 0 | 0 | 0 |

### The chains

43 files. Every card can open: 3. Some can: 16. None can: 24.

| chain | cards open | cards that end it, open | of |
|---|---:|---:|---:|
| house_debt | 14 of 14 | 4 | 4 |
| house_heir | 14 of 14 | 3 | 3 |
| pilots_secret | 14 of 14 | 3 | 3 |
| kentish_revolt_796 | 13 of 14 | 4 | 4 |
| pilgrim_road | 13 of 14 | 3 | 3 |
| house_rival | 12 of 13 | 3 | 4 |
| old_gods | 12 of 14 | 5 | 5 |
| house_marriage | 10 of 14 | 2 | 4 |
| storm_vow | 9 of 14 | 1 | 4 |
| black_stones | 6 of 14 | 1 | 4 |
| riddle_trail | 3 of 13 | 0 | 4 |
| dirham_hoard | 2 of 14 | 0 | 5 |
| house_new_ship | 2 of 14 | 0 | 5 |
| amber_road, lindisfarne_book, saga_rhine_frisia, saxon_exiles_804, silver_road, wine_fleet | 1 each | 0 | 3 to 6 |

**Ten chains can be played to an end inside the twelve ports.** The book names six; the census adds
kentish_revolt_796, old_gods, house_rival and house_marriage as whole or nearly whole, and warns that storm_vow and
black_stones each keep one ending of four. **Nine can start and never end:** amber_road, dirham_hoard, house_new_ship,
lindisfarne_book, riddle_trail, saga_rhine_frisia, saxon_exiles_804, silver_road, wine_fleet.

### The twelve ports

"Own" cards can come at three of the twelve or fewer. The sum of weights takes every flag as set, so it is a ceiling.

| port | own | of them real | chain steps | any-port cards | sum of weights, all flags | sum, no flag set | heaviest with no flag set |
|---|---:|---:|---:|---:|---:|---:|---|
| dorestad | 56 | 46 | 41 | 7 | 238.3 | 57.3 | house_debt s01 the tally (40) |
| traiectum | 23 | 23 | 12 | 7 | 268.1 | 4.1 | lent_table (1) |
| walichrum | 14 | 10 | 4 | 7 | 131.4 | 3.4 | monk_with_a_tablet (0.5) |
| lundenwic | 25 | 23 | 11 | 2 | 38.5 | 4.5 | pilgrim_road s01 (1.5) |
| witla | 13 | 8 | 8 | 7 | 281.4 | 7.4 | pilots_secret s01 (3) |
| medemelacha | 6 | 5 | 1 | 7 | 83.1 | 4.1 | saints_tenth (2) |
| doccinga | 7 | 5 | 2 | 7 | 41.6 | 7.6 | saga_rhine_frisia s01 (3): a trap |
| daventre | 12 | 10 | 3 | 7 | 71.2 | 7.2 | saxon_exiles_804 s01 (2): a trap |
| quentovic | 18 | 14 | 7 | 7 | 25.9 | 3.9 | lent_at_the_cell (0.5) |
| sarre | 19 | 15 | 10 | 2 | 54.8 | 6.8 | kentish_revolt_796 s01 (3) |
| fordwic | 20 | 18 | 10 | 2 | 67.8 | 6.8 | kentish_revolt_796 s01 (3) |
| ripa | 19 | 17 | 10 | 2 | 73.2 | 8.2 | lindisfarne_book s01 (2): a trap |

`npc_text_encounters.md` 2.3 planned 0.6 for the common harbour cards and 0.12 for the rare ones: "about one call in
two shows one of them". The files give 3.4 to 57 before any flag is set. `1 - exp(-3.4)` is 97 in 100. Of the 176
open cards drawn per call, 104 weigh under 1, 31 weigh 1 to 5, 10 weigh 5 to 20, and 31 weigh over 20 (chain steps
written at 25 to 50 "so that it is all but sure"). Where one of those holds, a loose card at 0.3 has under one chance
in a hundred. Where two hold, one of them waits a whole call on a coin.

### The sea

73 open sea cards with rates summing to 6.36 an hour and 114 crew cards summing to 33.86 (ceilings: 49 crew cards at
0.5 or more are song parts and chain steps, each behind a flag). Real stake: sea 26 of 73, crew 27 of 114.

One day under way with no flag set (July 794, open sea, four men), hour by hour, as the hourly draw alone would deal.
Low counts only cards whose `when` surely holds; high adds those that hang on what the census cannot know.

| the day | cards open at noon | cards sent in 24 hours |
|---|---:|---:|
| fair: wind 6 m/s | 13 to 20 | 1.8 to 11.8 |
| calm: wind 1 m/s | 15 to 24 | 4.3 to 13.3 |
| fog | 5 to 15 | 2.4 to 13.4 |
| gale: wind 20 m/s, storm 0.8 | 9 to 14 | 6.3 to 10.4 |

The director's four-hour gap cuts this to six a day and lets the rest go unshown.

### How the 420 open cards come

176 drawn per port call; 114 per hour among the crew; 73 per hour at sea; 50 only as a follow-up; 4 stage `long`; 3 on
arrival (the Frankish toll man, the toll post at Sarre, "whose guest" at Ripa). 98 come once. 355 have a cool-down.
Of the 130 open chain cards, 64 are drawn by weight and 49 only follow.

### Conditions most used

Every `when` and `check`, counted each time a name is read.

| name | all cards | open cards | | name | all cards | open cards |
|---|---:|---:|---|---|---:|---:|
| flag.ch_* | 3112 | 736 | | text.<id> | 131 | 97 |
| year | 2466 | 484 | | flag.crew_* | 129 | 124 |
| flag.(other) | 1491 | 280 | | under_way | 111 | 79 |
| port.id | 1354 | 256 | | npc.<role>.feeling | 102 | 39 |
| moored | 1270 | 333 | | port.visits | 102 | 44 |
| silver | 819 | 233 | | storm | 94 | 91 |
| month | 705 | 151 | | wind | 66 | 64 |
| hold.<good> | 495 | 118 | | flag.unlock_* | 65 | 11 |
| npc.<role>.met | 443 | 66 | | fog | 50 | 50 |
| riddle.<id> | 245 | 72 | | crew | 41 | 20 |
| npc.<role>.last | 237 | 95 | | tongue.<name> | 29 | 4 |
| hour | 133 | 131 | | port.people | 28 | 4 |

In the open cards: `days_at_sea` 24, `debt` 9, `health` 7, `heart` 6, `port.faith` 5, `standing` 4, `feast` 4,
`me.*` 0. Names that read 0 because the game has no model for them: `tongue.<name>` 29 (open 4), `fast` 16 (6),
`learned.runes` 4 (3), `reform.coin` 3 (3), `chest.rune_count` 2 (1).

### Effects most used

Every option's cost and effects, a chance's branches too.

| name | all cards | open cards | | name | all cards | open cards |
|---|---:|---:|---|---|---:|---:|
| npc (a feeling) | 4736 | 1216 | | goods (as a cost) | 291 | 71 |
| flag ch_* | 4189 | 893 | | flag crew_* | 248 | 246 |
| flag (other) | 1249 | 219 | | commission | 220 | 63 |
| morale | 1102 | 529 | | price_shock | 195 | 34 |
| silver | 1086 | 197 | | flag changed_* | 195 | 29 |
| silver (as a cost) | 790 | 213 | | give_riddle | 167 | 52 |
| flag unlock_* | 752 | 144 | | health | 159 | 97 |
| goods | 691 | 149 | | give_text | 147 | 79 |
| deed | 658 | 130 | | passenger | 133 | 21 |
| news | 479 | 89 | | learn | 123 | 43 |
| schedule | 437 | 137 | | toll eased | 119 | 22 |
| chance | 340 | 136 | | tongue | 55 | 16 |

Effects the game does nothing with today (`GameEffects` returns null): passenger 133 (open 21), learn 123 (43),
tongue 55 (16), encounter 27 (8), open_fetch 2 (0). Flags: 2,861 set by cards, 1,914 read by cards, 948 set and read
by no card (ch_ 427, unlock_ 262, changed_ 75).

### What the census says

- The cards ask **where, when and which step**. They hardly ask **who**. That is the gap between this deck and a
  quality-based one.
- The commonest outcome is a named person's feeling (4,736). That is the game's real memory, and only 39 open
  conditions read a feeling. Feelings are written far more than they are read.
- `morale` is the fourth commonest effect and nothing acts on it. One threshold card would give 529 effects a meaning.
- By the tier rule of section 4.1 the 420 open cards are: **must 53, should 84, may 171, colour 112**.

## 3. How the best text games choose

For each: the rule that picks; how it stops repeats and floods; how rare things feel earned; how a chain's next step
is shown; and what Kapman takes.

| Game | The rule that picks | Against repeats and floods | Rare, and earned | The next step of a chain | Kapman takes |
|---|---|---|---|---|---|
| **Fallen London** (Failbetter) | A storylet is content, prerequisites on numbers ("qualities") and effects (Short). The player sees every storylet whose numbers he meets, and draws cards at random from the eligible ones into a small hand. He picks what to play | A hand of a few cards; a card in hand is not drawn again (?); actions are rationed. Each card has a frequency word, Rare to Ubiquitous, that sets its weight (the words are on the wiki; the weights, Standard 100 and Rare 10, are from memory (?)) | Gated by a quality raised over weeks. Dias warns the deck is "inherently misleading": it looks like a deck and draws like dice | Dias, "the chain": "a quality that monotonically increases as the player progresses ... unlocking each storylet in turn" | The hand: eligible cards wait where the player can see them. Frequency as four words, not 176 hand-set weights |
| **Sunless Sea** | Port storylets, as above, by place | "Something Awaits You": a quality earned by time at zee and spent on a port's first shore option, so a port gives once a visit (?) (from memory; both wikis refused the fetch) | Long voyages: the far port is the gate | A journal line | A call earns its loose card by the leg behind it (4.4) |
| **Salience** (Short; Valve) | "Pick a bit of content out of a large pool depending on which content element is judged to be most applicable." Valve's dialogue: the rule with the most matching criteria wins (?) | The pool is asked only at set moments | The most particular line is the rarest by nature | None: Short names the weakness, a story that loses its thread | Salience as a factor inside the may tier and as the order of the should tier. Never the only rule |
| **Bruno Dias's patterns** | Five shapes: the chain; the wheel (a chain that resets); the clock (a counter that advances a phase); the hourglass ("many storylets scattered throughout the game all increment one quality ... When that quality hits a threshold, something happens"); the gravity well (a line that ends in a repeatable stub, "to signal that the player has exhausted this vein") | The gravity well; in Improv, a "dryness" filter drops any phrase already in the history | "Dressing up the variance in some aesthetic shell ... does make it feel more motivated and 'fair'" | The chain's rising quality | Hourglasses for heart, name and troth. A stub line when a port's loose cards run dry |
| **King of Dragon Pass** | Scenes with preconditions, drawn at random; about five random scenes a year of 544 interactive ones (614 in all with follow-ups) (Dunham) | "No scene will randomly repeat for at least five game years" | With 544 scenes most are rare for free: at five a year a given scene misses a given year 99 times in 100 | Follow-up scenes fire years later from what you chose. Advice is ordered: "skill-based advice first, and then advice that depends on religion or personality" | A long floor on repeats. The crew speak in a fixed order of seats. Six Ages was not checked (?) |
| **80 Days / ink** | ink: a choice marked once is gone after use, a sticky one stays, and a fallback fires when none is left; text can run as a sequence, a cycle or a shuffle; how often a passage was seen is itself a number to test (from the ink manual, from memory). In 80 Days you can only travel a route someone has told you of (?) | Once-only by default: the language forgets nothing | Knowledge of a route is the key, and it is earned by talk | The next city is a line on the globe | Once is the default for anything with a story. The engine has `once` and `port.visits`; it lacks "seen n times" for a card |
| **Disco Elysium** | Active checks roll two dice against a number. A passive check has no button: a skill speaks unasked when it is high enough (the exact sum, skill plus six against the target, is from memory (?)) | A passive line is said once | The line is there because of who you built. Nothing says what you missed | White checks open again when the skill rises (?) | Passive lines: a `when` on a line that reads `me.lore`, shown with its tag. The engine can do it today |
| **Wildermyth** | Events cast heroes into roles by their traits, hooks and ties; an event that fits your people is picked over one that fits anyone (?) | Events seen lately are held back, across campaigns too (?) | The event names your hero's own history | A hook on the hero's sheet | The hand of four men as roles. Not checked: the wiki page was not found |
| **Reigns** | One card at a time from advisers; a swipe moves four meters; a reign ends when one fills or empties. "Completing tasks in each reign allows the player to unlock new cards" (Wikipedia). Weights and forced follow-ups are from memory (?) | One card on the table. Always | New cards enter the deck as prizes | A follow-up is forced next | `then` is this already. A prize can be a card let into the deck: a flag in a `when` |
| **Left 4 Dead** (Booth, 2009) | "Structured unpredictability": each kind of threat on its own randomised clock (mobs every 90 to 180 seconds). Bosses: "three Boss events are shuffled and dealt out: Tank, Witch, and Nothing. Successive repeats are not allowed" | Build up, sustain the peak 3 to 5 seconds, let it fade, then **relax** 30 to 45 seconds with no mobs and no special infected. "Algorithm adjusts pacing, not difficulty" | Bosses are outside the pacing: "intended to change up the pacing anyhow" | None | The pulse of `living_world.md` is this. The bag of three for the sea day (4.4). Must cards stand outside the budget |
| **RimWorld** (Cassandra) | An on-off cycle: 4.6 days on, 6.0 off; 1 or 2 big threats in each on-phase; at least 1.9 days apart; small things on a mean of 4.8 days; the first on-phase at day 11; about 8.5 big threats a year (wiki). Wealth, heads and recent deaths scale them | The off-phase. Randy has none: "no guaranteed safety period" | Threat grows with what you own | None | A year has its own cycle here: sea days are on, port days and winter are off. Count crises by the season |
| **Crusader Kings III** | The game calls named moments ("on_actions"). Each holds a weighted list: `chance_to_happen`, `chance_of_no_event`, `100 = event`. An event's `trigger` is asked first; `weight_multiplier` leans it; `first_valid` takes the first that fits; `fallback` if none does | `cooldown = { years = 5 }` on the event; "no event" has its own weight | Weight multipliers that read your traits | `trigger_event = { id = x days = { 7 14 } }`; a story cycle pulses every 30 to 60 days, keeps its own numbers, and passes to the heir | Moments (4.2). `first_valid` for must and should, weights for may. `schedule` is `trigger_event`; the heir keeps the flags |

Three things all of them agree on, and the scheduler does none: content is asked for at **named moments**, not every
hour; the **likeliest card is not the most needed card**, so need is ordered and only the rest is rolled; and a
**quiet stretch is made on purpose**.

## 4. The chooser

One small class, `Teller`, over the built `Scheduler`, as `living_world.md` section 7 already asks. It sets a card's
tier at load, enqueues must and should cards itself, gives the draw one more factor, and keeps the hand of offers.
The card format does not change. One optional field is added (`"tier"`), and one table, `chooser_793.json`, holds the
numbers below and a few id patterns.

### 4.1 Four tiers, read from the data

| Tier | A card is in it when | Open cards | How it comes | In the frame |
|---|---|---:|---|---|
| **Must** | `arrival`; or it has no weight and no rate (it comes only by `then` or `schedule`); or the table names it (the count's day) | 53 | On its moment, always, in the order the days fall due. Outside every budget | The window opens over the chart and the clock stops. A seal in its title bar. Esc is refused (as `Dismiss` already refuses an arrival card) |
| **Should** | A chain file's card with a weight or a rate; or any card of weight 20 or more (written "to be all but sure") | 84 | At the next moment that fits, one at a time, by need: the chain that has waited longest; then the card whose `when` has most terms; then the file's order. No dice. A chain's first card comes only when its lane is free (4.7) | The window opens after the toll. A knot and the chain's name in the title bar. Esc is "not now": it becomes a row under People here until cast off |
| **May** | The rest, where some choice is real or a door | 171 | The built Poisson draw, over this tier alone, under the budget of 4.4 | The window opens. Esc closes it |
| **Colour** | The rest: every choice is soft. And the table's patterns: `crew.song.*` | 112 | Never pushed. It is offered | A head lights in the crew window, or a pip under People here, with one log line: "The lookout has the next part of his song." A press opens it |

Why the should tier is not rolled: it removes the weight-40 habit. A step that is due comes, and two due steps come
in a fair order. Why colour is not pushed: it ends "43 stops that ask a question that is not one" without cutting a
word. The white shirt stays; the player asks for it.

**Heat** is a second mark, also read from the data, for the pulse of 4.8: 0 soft; 1 a real choice with no dice; 2 a
chance whose failure costs goods, silver or health (112 open cards hold a chance); 3 a chance that can lose a man, or
the law's seizure (8 open cards can lose a man).

### 4.2 Moments: when the Teller is asked

| Moment | Built? | Pool | Rule |
|---|---|---|---|
| She moors | `Scheduler.Arrive` | must (the arrival set; steps due here), then should, then may | all must; first valid should; one draw of may |
| Dawn, still moored | new | should, then may; the offers refill to three | first valid should; one draw of may; for three dawns. Then the port has no more to say. It replaces weight / 72 an hour |
| The watch changes at sea: dawn, noon, dusk | new: crew cards weigh 0 at other hours | crew may; crew colour as a lit head | one draw |
| The weather or the water turns | the hourly `Tick` | sea should (a danger's next step), sea may | one draw, by the gap |
| A sail within hail | the sim (`rate: "traffic"`) | hail cards | the sim's |
| A feast or fast of your faith | `feast` is built | faith cards | first valid |
| A threshold is crossed: heart under a third, unpaid 14 days, bread out | new | must, and a warning sign a day before | first valid |
| A day arrives: Michaelmas, a promise, a step | `schedule` | must | in order of day |

### 4.3 The rule, in order

    at a moment:
      1. every MUST card that is due and can open here goes in line, earliest day first
      2. if the pulse is over 0.6: stop here (4.8)
      3. if a SHOULD card holds here and the day's should is unspent: the neediest goes in line
      4. if the budget has a MAY left and the gap has passed: one draw over MAY cards,
           weight x m_port x m_standing x m_fresh x m_self x m_deck
      5. every other card that holds here and is not on its cool-down: an OFFER, up to the hand of three: should first, then may by weight, then colour
      6. nothing is dropped; an offer lapses at cast off (port) or after six hours (sea)

- `m_fresh` = 3 for a card never shown; else `1 + days since shown / 365`, 3 at most.
- `m_self` = `1 + 0.5 x` the number of personal terms in its `when` that hold (`me.*`, `bond.*`, `text.*`,
  `npc.*.last`, `tongue.*`), 3 at most. The most particular card comes first.
- `m_deck` = 0 if a card of the same deck (the first two parts of the id: `sea.fog`, `crew.pay`) was pushed in the
  last three days.
- All dice from `SimRandom.For(seed, ...)` as now, so a reloaded save tells the same tale.

### 4.4 Budgets

The clock: one game minute a second at 1x. A game day is 24 real minutes at 1x, 12 at 2x and 6 at 4x.

| Span | Pushed (the clock stops) | Offered (the player opens) | Real time between stops |
|---|---|---|---|
| **Arrival** | The arrival set (2 at most), each step due here, 1 should, 1 may: 4 as a rule, 2 or 3 most days | 3 rows under People here | none: she is moored, the clock is held |
| **Each further dawn moored** | 1 should and 1 may, for three dawns | back to 3 | |
| **A game day under way** | Every must. 1 should. 1 may on two days in three: the day's slot is dealt from a bag of three (a sea card, a crew card, nothing), shuffled and refilled when empty. Signs: 4 a day (the book) | 2 lit heads a day, each for six hours | 4x: a stop every 70 to 120 seconds. 1x: every 5 to 8 minutes |
| **The gap** | 4 game hours between pushed cards (built: `seaGapHours`); 1 hour after a sign; between ten at night and five in the morning only must cards and dangers | | 4x: 60 seconds at least |
| **The floor** | 8 waking hours with no stop of any kind: the next may draw is sure; if no may card holds, a head is lit | | 4x: never more than 2 minutes of nothing. 1x: 8 minutes |
| **A leg under half a day** | must and should only (the book) | 1 | |
| **A season** | 2 chains open and the debt; 1 chain start a month at most; a heat-3 card no more than once in 5 days; 1 rare a year | | |
| **A life** | 5 or 6 rare; 3 or 4 chains ended; 10 hours of history on their dates | | |

**Why two days in three.** The may tier at sea is 25 sea cards and 44 crew cards: 69. If none is to come twice in a
year, and she is under way about 70 days a year (g), that is one a day at most. Two in three leaves a margin for
cards shut by weather. A second card every day needs about 70 more sea and crew cards with teeth, or repeats in the
first autumn. The book owes forty; without "Sail on" the leg needs the larger number in the end. Until then the
leg is carried by what never wears out (signs, the field, the measure) and by what the player pulls: 101 colour
cards as lit heads, the chest, the talk.

**Why a call earns its card.** No may card at a port she left less than three days ago, unless she has moored
somewhere else since. Hopping off and on a jetty deals nothing.

**In port the deck runs dry on purpose.** A port has 6 to 25 own loose cards, and no loose card comes twice in a
season at the same port. By late summer most calls at home bring the toll man, a step if one is due, and the people
you choose to speak to. When a port has nothing left, People here shows one stub line, the same each time, in the
keeper's voice: "Nothing new on the bank since you sailed." That is Dias's gravity well: it tells the player to go
elsewhere.

### 4.5 Repeats

| Rule | Number |
|---|---|
| A chain step, a must card with `once` | once |
| A may card at sea | not within 120 days, whatever its `cooldown_days` says (the middle one says 60) |
| A may card in port | not twice in a season at the same port |
| A colour card that tells a story | once a life; it then lives in the chest |
| The same deck | not within three days (`m_deck`) |
| The toll man after the first call | one line and Enter, as built, unless his `when` lines have changed (a new law, arms aboard, a changed feeling) |

`EncounterState.lastShown` already holds the day of every card. The floors are a `max()` in `Ready`.

### 4.6 Rarity

- **Four words, not 176 weights.** Inside the may tier a card is *common* (1), *uncommon* (0.3), *seldom* (0.1) or
  *rare*. The loader reads the word off the weight the card has (1 and over; 0.2 to 1; under 0.2), so no file is
  edited; a writer may set the word instead of a number from now on.
- **A rare card** is `once`, and either dated (the fiery shapes of 793, the elephant of 802) or one of the list of
  about forty in `fate_faith_and_self.md` 5. It needs three things. **A shadow:** its `when` asks for
  `flag.heard_<id>`, set at least a day earlier by a news line or a talk two ports away. The Tasks window shows the
  shadow under Heard, as old as the ship that brought it, and never as a task. **A gap:** none within 200 days of the
  last; after that its chance is times `days since the last rare / 400`, 3 at most, so a life meets five or six and
  a long drought ends. **A key, where it has one:** the Lore, the faith, the text or the friend that opens its best
  choice.
- **A dated rare card that is missed** comes as news, as old as the ship (the patience rule of `living_world.md` 7).
- **Say why.** The foot of every window carries one small line: "This came because: you carry the psalter leaf; the
  steward is easy with you." `Cond.Describe` already turns a term into those words for a greyed choice. The Teller
  walks the card's `when`, takes the terms that hold and are personal, and prints them. A choice that only you can
  make is drawn in Byzantium, as the book rules. A rare thing feels earned when the game says what earned it.

### 4.7 Chains: lanes, the next step, the Tasks window

**Lanes.** Three, as `living_world.md` has them: House (house_debt, house_heir, house_rival, house_marriage), Place
(pilots_secret, pilgrim_road, old_gods, black_stones, storm_vow), World (kentish_revolt_796). One chain open in a
lane; the debt is outside the count in the first year. A chain's first card is a should card only while its lane is
free. So a player meets one hook at a time and can follow it.

**The release list.** The ten of section 2. The nine traps and the twenty-four far chains go to the `later` folder
with the loader's list (book, stage 0, step 2). `card_census.py` is the reach gate: a chain is let in when its
"cards that end it, open" is not 0.

**The next step is found by machine.** A chain is *open* when one of its `ch_<id>_<n>` flags is set and none of its
`ch_<id>_end_*` flags. Its next steps are found two ways, both from data that exists:

1. **Promised steps:** every entry of `EncounterState.scheduled` whose card is in the chain's file. It has a card, a
   day and a port.
2. **Waiting steps:** every card of the file not yet shown whose `when` is not false when place, hour and weather
   are left unknown. This needs a three-valued `Eval` on `Cond` (true, false, cannot say). `card_census.py` has it in
   sixty lines (`ev`). `Cond.TextWords` already gives the `port.id` words, so the place is read off the condition.
   A second pass leaves the hold and the purse unknown too: a step that fails only on those is shown with what it
   needs.

**What the Tasks window shows.** Rows in this order. Each has Show, which lays the measure from her mark to the place.

| Row | From | Reads |
|---|---|---|
| The debt | `Trade.Debt` (built) | "The count: 440 d at Michaelmas. 61 days." |
| Promises | `Commissions.Mine` (built) | "Six bolts of English cloth for the abbot, Traiectum. 41 days." |
| A promised step | `state.scheduled` | "The rival: Dorestat, in 20 days." A notch on the day rule; gold ahead, red when passed |
| A waiting step | the three-valued `Eval` | "The pilot's secret: Witla. No day." or "The black stones: Quentovic. Bring the letter." |
| Heard | `heard_` flags and the news | "A fleet fitting out in the north, a skipper said. 9 days old." No place is promised, no Show |

The book's line holds: a chain's next step is always a place and a day you can see. The fate page's line holds too:
an hour of history is never a task, only a shadow.

**Patience.** A promised step waits `max(30, twice its delay)` days past its day (the middle delay is 30, so 60).
Its notch turns red on the day. Past its patience the chain sleeps: its lane is free again, its row greys, and it
wakes when she next calls at its port. Nothing is lost and nothing is decided off-stage in the first release.

### 4.8 Who you are: how people, faith, past, stats and bonds open and shut cards

Today 0 cards ask. The names to add to `VarNames` (section 5) are `me.people`, `me.faith`, `me.past`, the five stats
`me.tongue`, `me.name`, `me.nerve`, `me.lore`, `me.troth`, the family `bond.<person>`, and `npc.<role>.people` and
`npc.<role>.faith` so that a card can say "as kin". Six ways a quality acts, five of them on the engine as it is:

| Way | Written as | The player sees |
|---|---|---|
| **It opens or shuts a card** | `when: me.faith = jewish and hour >= 16 and under_way` (the Sabbath at sea) | The card comes, with its because-line |
| **It opens a choice** | `check: me.past = clerk` or `check: me.people = npc.host.people` or `check: bond.pilots_daughter >= 2` | The choice in Byzantium with its tag: `[As kin]`, `[Lore 3]`, `[Bound]` |
| **It hides or greys** | new, small: a failed check that reads `me.people`, `me.faith` or `me.past` hides the choice (you cannot become a Frisian). One that reads a stat, a text, a tongue or a bond greys it with the rule in words (you could earn it) | Grey: "Needs Lore 3." Hidden: nothing |
| **It leans the dice** | `"by": "me.nerve"` on a chance: the built sum, `p = chance + 0.05 x (stat - 3)`, so five pips are worth ten points either way | The odds word moves: doubtful, likely, sure |
| **It speaks unasked** | a line's `when`: `me.lore >= 3`, or `me.faith = old_gods` | A passive line with its tag, once. Nothing says what a lower stat missed |
| **It pulls the card forward** | `m_self` of 4.3 | A player of the minster meets the monk's cards sooner than a steersman's child does |

Rules that keep it honest:

- **Stats rise by use.** When a chance `by me.<stat>` is won, that stat gains a use; a pip comes at 3, 8, 15 and 25
  uses (g). One line in `ChanceEffect.Apply`; no card names it.
- **Three hourglasses**, each fed by effects the cards already hold, each ending in a must card with a warning first.
  Heart: under a third, a man goes ashore at the next port (the book's rule; it gives 529 open `morale` effects a
  point). Name at a port: standing under "shunned", the reeve's card. Troth: three broken days of your faith, the
  priest's or the old hand's card.
- **A bond moves one step a season.** `bond.<person>` is 0 to 4 (none, known, trusted, bound, sworn). A bond's step
  is a should card in the House lane; after it the flag `bond_<person>_rest` holds for 30 days.
- **One card in five.** The target is about 85 of the 420 open cards with a personal term. It is a tagging pass, not
  new prose: one `check`, or one line, on a card that already has the right person in it. 169 open cards already hold
  a keyed choice; they need only the colour and the tag.

### 4.9 The pulse

Kept from `living_world.md` 7, with two numbers changed because empty hours are no longer skipped.

- **Pulse**, 0 to 1: a man lost, a cargo lost, a third of the silver gone, the home port struck: +0.3 each, halving
  every 20 days. Over 0.6 only must cards, heat 0 and heat 1 come, and chain starts wait.
- **Never two heat-3 cards inside five days.** Every danger warns first: the sea deck's own rule, held by its flags.
- **Changed:** "after any card, six game hours at a fifth" becomes the flat four-hour gap that is built. "Twelve
  hours with none: tripled" becomes the eight-hour floor of 4.4, which is sure and not a multiplier.

## 5. The qualities to track

| Name | Kind | Written by | Read by cards for | Built? |
|---|---|---|---|---|
| `me.people`, `me.faith`, `me.past` | text | the start page | kin, law, the table, the ghost of your past | no |
| `me.tongue`, `me.name`, `me.nerve`, `me.lore`, `me.troth` | 1 to 5 | use (a won chance); deeds; kept days | checks, odds, passive lines | no |
| `bond.<person>` | 0 to 4 | bond steps | choices only a friend has | no |
| `npc.<role>.people`, `npc.<role>.faith` | text | the name forge (the person has them) | "as kin" | no (two rows in `VarNames`) |
| `npc.<role>.feeling`, `.met`, `.last`, `.owed` | built | every `npc` effect (4,736) | memory lines | yes; read too little |
| `standing`, `port.visits`, `port.best_feeling` | built | deeds, calls | "they know me here" | yes |
| `tongue.<name>` | 0 to 1 | the `tongue` verb (null today) | 29 conditions that can never hold | half: the verb must write it |
| `text.<id>`, `riddle.<id>`, `learned.latin` | built | the chest | Quote; keys | yes |
| `hold.<good>`, `silver`, `debt`, `debt_due_days` | built | trade | prices, keys | yes |
| `days_at_sea`, `heart`, `health`, `crew` | built | the voyage | thresholds | yes; heart has no rule |
| `fast`, `feast` | 0 or 1 | the faith's calendar | faith cards | `feast` yes; `fast` reads 0 |
| `flag.ch_<id>_<n>`, `ch_<id>_end_<name>` | flags | chain cards | the next step | yes |
| `flag.heard_<id>` | flag with a lapse | news, talks | a rare card's shadow | the flag yes; the habit no |
| `flag.bond_<person>_rest` | flag, 30 days | a bond step | pacing a bond | the flag yes |
| `card.<id>.seen` (?) | count | `Shown` | "the third time he asks" | no: `lastShown` keeps a day, not a count. Only if a card needs it |
| The Teller's own: pulse, pushed today, last pushed hour, the bag, the last rare day, the lanes, the offers | numbers and lists | the Teller | nothing: cards do not read the storyteller | no; saved in `EncounterState` |

## 6. What a programmer must build

One step at a time, each with its test. Tests are EditMode and headless.

| # | Step | Nights (g) | Its test |
|---|---|---|---|
| 1 | Three repairs: stage `long` gets a draw while moored (`HourlyWeight`); the seven sea and crew chain steps get a `rate`; the loader takes a list of files, with the nine traps and the far chains left off it | 0.5 | `long.offa_letter.796` can be drawn in a headless 796. `card_census.py` prints nodoor 0 and nodraw 0 |
| 2 | Tier and heat at load: `EncounterDef.tier`, `.heat`, `.deck`, the personal-term count; the optional `"tier"` field; `chooser_793.json` | 1 | Over the same files the loader counts what the census counts: must 53, should 84, may 171, colour 112 |
| 3 | The `Teller`: a `story` factor in `VisitWeight` and `HourlyWeight`; must and should enqueued in order at `Arrive` and at dawn; the budgets, the gap, the night, the bag, `m_fresh`, `m_self`, `m_deck`; its counters saved | 2.5 | 200 seeded seasons by the bot: no sea day with more than one should and one may; no call over 4 and its steps; no deck twice in 3 days; no 8 waking hours empty while a may card held; of two due chain steps at one port, one opens on arrival and the other is a row at once and opens at the next dawn |
| 4 | Offers: `EncounterState.offers` (card, port, until); rows under People here; lit heads; the log line; the Event lamp's count; Esc on a should card makes an offer | 1.5 | A call at Dorestat with three loose cards holding: one window, two rows. Cast off: the rows are gone and their cool-downs unspent |
| 5 | The journal: three-valued `Eval` on `Cond`; open chains; promised and waiting steps; Tasks rows; notches; Show | 2 | New game, the tally taken: Tasks reads "The rival: Dorestat, in 20 days" and "The count's day: Dorestat, in 100 days". After pilots_secret s01: "Witla. No day." |
| 6 | The self: `me.*`, `bond.*`, `npc.<role>.people` and `.faith` in `VarNames` and `EncounterGameVars`; a text compare may have `me.` on the right; hide or grey; stats by use; passive tags | 2 | A card with `check: me.people = npc.host.people` shows the choice to a Frisian at Dorestat and hides it from a Dane. Three won chances `by me.nerve` raise Nerve a pip |
| 7 | The because-line and Byzantium for keyed choices | 0.5 | The toll man's "Show the charter" is purple; the foot reads "you carry the charter" |
| 8 | Rarity: the four words read off the weights; `heard_` shadows; the gap rule; the rare table | 1 | Twelve headless years: 4 to 7 rare cards, none within 200 days of another, none without its shadow |
| 9 | Patience, lanes and sleep | 1 | A step unmet 60 days past its day: the row is grey, the lane is free, and it wakes at its port |
| 10 | The soak's print: by leg, day and call, the cards by tier; and for every card shown, which choice was taken | 1 | The print exists. A card answered one way in 19 of 20 is listed (the designer's sixth gate) |

About 13 nights. Steps 1 to 4 belong inside the book's stage 1, at its steps 1 and 4; the rest is stage 2.

## 7. What the content needs

No new deck. In this order:

1. **Shut the nine traps** and move their files with the far chains. A day's work, by list.
2. **Give the seven stranded steps a rate** if their chains are ever let in (all seven are in trap chains, so not now).
3. **The tagging pass:** about 85 open cards get one personal term. Begin with the 25 open cards that already read a feeling.
4. **Colour needs nothing.** The 112 soft cards become offers by rule. The 37 soft sea cards are the ones to reprice
   when the `hours` verb exists (the book's "reprice twenty").
5. **Shadows:** one news line for each rare card and each hour of history: ten for the first life.
6. **The forty owed sea and crew cards** raise the sea budget from two days in three toward every day. Write them
   last, after the soak shows which decks run dry first.

## 8. Yours to decide

My advice is the first answer each time.

1. **Colour cards.** Never a window unless you press the lit head or the row. / One window a day at most. / As now.
2. **A chain step you close with Esc.** It waits as a row under People here until you cast off. / It must be
   answered, like the toll man.
3. **The Tasks window and a chain.** It names the next place and the day, with Show. / A hint in words only ("the
   pilot's daughter said to come back"). / Nothing: the player remembers.
4. **Loose cards at sea.** Two days in three, as the deck can bear. / Every day, and accept repeats inside a year
   until seventy more are written. / As the scheduler deals them.
5. **The chains of the first release.** The ten that can end inside the twelve ports. / The book's six. / Every chain
   whose first card can open.

## Not checked

- Fallen London's frequency weights, hand size and "a card in hand is not drawn again"; Sunless Sea's "Something
  Awaits You"; the Disco Elysium passive sum; how Wildermyth, Reigns and 80 Days pick; Valve's "most criteria wins";
  Six Ages. All from memory; the pages refused or were not found.
- About 70 days under way in a year: a guess until the soak counts them.
- The census is generous (see section 2). It was not checked against the game's own loader.
- Nothing here was run in Unity.

## Sources

- Emily Short, "Storylets: You Want Them" (2019): https://emshort.blog/2019/11/29/storylets-you-want-them/
- Emily Short, "Beyond Branching: Quality-Based, Salience-Based, and Waypoint Narrative Structures" (2016):
  https://emshort.blog/2016/04/12/beyond-branching-quality-based-and-salience-based-narrative-structures/
- Emily Short, "Pacing Storylet Structures" (2020): https://emshort.blog/2020/01/21/pacing-storylet-structures/
- Bruno Dias, "(Some) Recurring Design Patterns in Storylet Narrative":
  https://azhdarchid.com/some-recurring-design-patterns-in-storylet-narrative/
- Bruno Dias, "Randomness in Narrative Video Games: Aesthetics of Different Random Systems":
  https://azhdarchid.com/randomness-in-narrative-video-games-aesthetics-of-different-random-systems/
- Bruno Dias, Improv, the filters page: https://improv.readthedocs.io/en/latest/filters.html
- David Dunham, the King of Dragon Pass blog: "Repeatability" (10 September 2011), "How Many Scenes?" (2 September
  2012), "Anatomy of a Scene" (25 August 2013): https://kingofdragonpass.blogspot.com/
- Michael Booth, "The AI Systems of Left 4 Dead" (Valve, 2009), the slides, read as text:
  https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf
- RimWorld wiki, "Cassandra Classic" and "AI Storytellers": https://rimworldwiki.com/wiki/Cassandra_Classic
- Crusader Kings III wiki, "Event modding" and "Story cycles modding": https://ck3.paradoxwikis.com/Event_modding ,
  https://ck3.paradoxwikis.com/Story_cycles_modding
- Wikipedia, "Disco Elysium" and "Reigns (video game)".
- In the project: `Design/living_world.md` 7; `Design/npc_text_encounters.md` 2.3; `Design/GAME_MAKER.md` 4.3;
  `Design/maker/heart.md` 5; `Design/fate_faith_and_self.md` 2 and 5; `Assets/Kapman/Code/Core/Encounters/*.cs`;
  `Assets/Kapman/Code/Game/Encounters/EncounterDirector.cs`, `EncounterGameVars.cs`, `GameEffects.cs`.
