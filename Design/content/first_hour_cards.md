<!-- 7 Oct 2026. One agent. Read: DECISIONS.md (7 October, later the same day, 1 to 13), research/next/cards.md, SCHEMAS.md,
the engine's card reader (EncounterDef, Effects, Cond, EncounterLoader, EncounterSession, NpcRegistry), every card that
can open in the first release (420 of 1,392) with its words, the places files of the twelve ports, the spine's cast,
weight_793.json, Art/art_slots.json, lore/period_words.md part (b). Ran: Bake/content/card_census.py and
Bake/content/card_rules.py. Unity and git were not run. No card file was edited. -->

# The first hour out of Dorestat: sixty cards, and the faults met on the way

## How this list was made

A new game, June 793, a first call at each place, no flag set. The road is the one a new player takes: home, down the
river, Traiectum, Witla, Walichrum, the crossing, Lundenwic. At each place the cards that can open there were taken in
the order the scheduler would most likely deal them: the arrival card first, then by weight (in port) or by rate (under
way). A card that needs a second visit, a later year or an earlier card is left out, except the one step that an
earlier card of this hour sends on (number 50).

- **What it asks** is the card in one line.
- **The stake** is what its choices can cost or bring, in the card's own numbers. "d" is pennies.
- **Words** is lines written out of lines the card has. All sixty are full. They are the writers' drafts, yours to
  overwrite. A word in brackets after the count is a word from part (b) of `lore/period_words.md` found in them.
- **Pressing** cards say their time, and what the card does by itself if the clock is let run
  (`Bake/content/out/card_rules_793.json`). Every other card here waits a week and then takes the plain answer.

Not every one of these comes in one hour. The weights at home add up to far more than one card a call
(`research/next/cards.md`, section 2), so the first hour is the top of each list.

## The sixty, in the order they would come

### At home: Dorestat (12)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 1 | `harbour.tollman.frankish` | The toll man asks what you carry. It comes on every call at a Frankish port. | The king's tenth on what you sell. 12 d buys half toll for 30 days and is a bribe. Hiding arms: 1 time in 4 the cargo is seized. | 16/16 |
| 2 | `harbour.chain_house_debt.s01_tally` | The count's steward shows the tally: 480 d, due at Michaelmas. | 12 d for the count's table, or the steward's ill will. Starts the debt: the rival in 20 days, the count's day in 100. | 10/10 |
| 3 | `harbour.midsummer_fires.dorestad` | A feast day, and nobody is selling. Keep it, work through it, or mind the boat. Only on a feast day in June. | 8 d for the crew's feast. Loading in secret: 4 times in 10 it costs 6 d and the priest. | 8/8 |
| 4 | `harbour.chain_black_stones.s01_unsold_stones` | A quern cutter has forty pairs nobody fetches since the kings fell out. | Four pairs on trust, or four for 24 d. Every choice sends the reeve's card to Lundenwic in 2 days. | 10/10 |
| 5 | `harbour.book_for_utrecht.dorestad` | The priest asks you to carry a book to Traiectum. | Nothing now. 12 d at the school when it is handed over. A sack of rye for his poor, if you like. | 9/9 |
| 6 | `harbour.first_question.dorestad` | Ravn has a riddle for you. | No silver. The first riddle goes in the chest. | 11/11 (hour) |
| 7 | `harbour.aldswiths_bench.dorestad` | The ale-house. Pay the crew's score, ask the news, buy a riddle, or listen. | 4 d or 8 d. The news makes rye dear at Doccinga for 90 days. | 13/13 |
| 8 | `harbour.kings_mint.dorestad` | The moneyer weighs your purse. His clerk sells a riddle. | 10 d for the riddle. | 14/14 (money) |
| 9 | `harbour.black_haired_merchant.dorestad` | A man sells wax at thirty the cake and names the price first. | 60 d for two cakes. They are short weight unless you weigh them first, and the prior at Traiectum (24) will know. | 9/9 |
| 10 | `harbour.salter_on_the_dams.dorestad` | A salter with a wax tablet in his belt. | 6 d for two sacks of salt, or 12 d for the riddle on the tablet. | 10/10 |
| 11 | `harbour.years_arms.mail_buyer` | A Dane asks, pleasantly, whether a Frisian boat ever carries a mail coat. | 260 d if you have one to sell, and 3 times in 10 the law. Telling the toll man eases the toll and loses the Dane. | 8/8 |
| 12 | `crew.hunger.wheat` | Famine ashore and wheat in your hold. The men watch what you do. Only with wheat aboard. | A sack given, the king's price, or the dearest price. The crew's heart either way. | 9/9 |

### Down the river (10)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 13 | `crew.faith.offering` | The land drops astern. The Dane wants ale poured for his god, the steersman a prayer. | How two of your men feel about you. Nothing else. | 8/8 |
| 14 | `crew.song.fitt_01` | The first evening under way. The lookout offers the first part of his long song. | Nothing. The first part of Beowulf goes in the chest. | 8/8 |
| 15 | `sea.shoal.lead` | The water ahead has changed colour. | **Pressing, 2 hours.** Sound, go round, or hold on: 4 times in 10 holding on hurts the crew. Left alone she holds on. | 7/7 |
| 16 | `sea.bar.ebb` | The river bar, with the ebb running against the wind. | **Pressing, 2 hours.** Wait for the flood, or take it now: more often than not that costs health and a sack each of salt and wheat. Left alone the ebb takes her on. | 7/7 |
| 17 | `sea.fisher.boat` | A fishing boat at her lines. | 1 d for fish, 3 d for a pilot, or a question for nothing. | 8/8 |
| 18 | `sea.river.weir` | A fish weir built too far into the channel. | Go round, buy eels for 2 d, or push through: more often than not that costs 6 d and the fisher's anger. | 7/7 |
| 19 | `sea.seals.bank` | Seals on a bare sandbank. | Clubbing them: one time in two a bundle of skins, else a hurt man. | 8/8 |
| 20 | `sea.smoke.ashore` | Two columns of smoke on the shore. | A little heart, lost or won, if you stand in to look. | 9/9 |
| 21 | `sea.river.mist` | Mist on the river at dawn, and a sound in the reeds like a bull in a cask. | Nothing. | 7/7 |
| 22 | `sea.river.tow` | No wind, and the stream against you. A man with two oxen has a rope. | 2 d for the tow, or your own men's backs, or wait. | 7/7 |

### Traiectum (5)

The toll man comes again here, and at Witla and Walichrum.

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 23 | `harbour.school_chest.traiectum` | The schoolmaster. He takes the book from Dorestat, and has three lines of riddle. | 12 d for the book carried. A riddle for nothing. | 13/13 |
| 24 | `harbour.priors_store.traiectum` | The old monk at the store door buys wax and rye on his own scale. | 72 d for two honest cakes of wax, 58 d for the short ones and his ill will. Rye at his poor price is a good deed. | 12/12 |
| 25 | `harbour.gate_bench.traiectum` | A man sits in sanctuary inside the gate and needs twenty pennies. | 20 d lent, 26 d back later through the prior. Or 2 d in the bowl. | 13/13 |
| 26 | `harbour.vecht_crossing.traiectum` | A man hollowing a boat knows the way through the Vecht mouth. | 6 d for a pilot, two loads of oak for his marks, or pole it yourself: about an even chance. | 8/8 |
| 27 | `harbour.saint_martins_plate.traiectum` | Saint Martin's church, and a priest by the treasure chest. | 4 d in alms, or a cake of wax, or hear Mass and keep your purse shut. | 10/10 |

### Witla (4)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 28 | `harbour.five_cows.witla` | A hungry farm, in the hungry years 793 and 794. A woman at a loom sees what kind of boat you have. | Four sacks of rye, if you have them: for three cheeses and her thanks, or for her own bolt of cloth and her ill will. | 10/10 |
| 29 | `harbour.pilots_shed.witla` | The old pilot watches the bar from his net shed. | 12 d to be taken over now, or wait a tide. Asking him about the water costs nothing and he likes it. | 11/11 |
| 30 | `harbour.dice_at_wilsuits.witla` | Three shipmasters wait on a wind. One rattles bones in a cup. | 10 d on an even throw. 2 d for a cup and news from over the water. | 10/10 |
| 31 | `harbour.racks_and_salt_fires.witla` | Fish racks and salt fires along the strand. | 8 d for four sacks of salt, 12 d for three bundles of dried fish. | 9/9 |

### Walichrum (5)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 32 | `harbour.creek_strand.walichrum` | An English pilgrim on the mud wants a passage to Dorestat. | 8 d fare, or carry him for nothing and be named at Rome. A passenger aboard. | 10/10 |
| 33 | `harbour.menders_bank.walichrum` | A man splits the island's last oak log. | 84 d for two loads of oak, if you carry them. A riddle for nothing. | 10/10 |
| 34 | `harbour.wiltruds_house.walichrum` | A shipmaster nobody knows tells his news for the third time. | 3 d for his score, or 20 d for two pairs of quernstones on the strength of what he says. | 9/9 (skipper) |
| 35 | `harbour.sheep_on_the_dune.walichrum` | A boy on the dune says the sheep know tomorrow's weather. | Lie a day, or sail anyway: more often than not that hurts the crew. | 9/9 |
| 36 | `harbour.tan_pits.walichrum` | The tan pits, and a tanner with his father's riddle. | 24 d for two bundles of hides. A riddle for nothing. | 10/10 |

### The crossing (13)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 37 | `crew.song.fitt_02` | The next evening at sea. The lookout has the next part ready. | Nothing. The second part goes in the chest. | 6/6 |
| 38 | `sea.swell.long` | A long swell from a quarter the wind is not blowing from. | **Pressing, 6 hours.** Shorten sail, make for a lee, or hold on: one time in two that hurts. It is the warning before the gale card. Left alone she holds on. | 8/8 |
| 39 | `sea.squall.line` | A black line of cloud coming up against the wind. | **Pressing, 1 hour.** Drop the sail, or ride it: more often than not that hurts. Left alone it hits with the sail up. | 7/7 |
| 40 | `sea.chain_old_gods.s01_storm` | The sea comes over the quarter faster than two men can bail. The steersman and the hand each say what a man does now. | **Pressing, 2 hours.** A vow to Saint Martin, a vow to the sea, or no vow. A vow must be paid later: it opens the chain of the old gods. Left alone no vow is made. | 10/10 |
| 41 | `sea.calm.falls` | The wind drops to nothing. | Row and tire the men, or wait, or fish. | 7/7 |
| 42 | `sea.night.light` | A June night that never goes dark. | Nothing much: a little heart or health. | 8/8 |
| 43 | `sea.sun.noon` | No land since yesterday, and the wind has shifted twice. | Heart. The steersman can teach you to find south by the noon shadow. | 7/7 |
| 44 | `sea.land.smell` | The wind brings the smell of hay and peat smoke off a land you cannot see. | A little heart. | 6/6 |
| 45 | `sea.land.wrong` | Dawn shows land, and the headland is the wrong shape. | Coast along, land a man to ask, or stand on: 6 times in 10 that hurts crew and heart. | 7/7 |
| 46 | `sea.watch.asleep` | You wake to find the man at the oar asleep on his feet. | Heart and a little health. | 6/6 |
| 47 | `sea.birds.land` | Birds fly over, all the same way, with fish in their beaks. | A little heart. The steersman can teach you the birds. | 8/8 |
| 48 | `sea.sail.far` | A sail on the edge of the sea. | Closing to speak her: 3 times in 10 it goes badly for the crew. | 10/10 |
| 49 | `sea.porpoise.roll` | Porpoises off the bow. | A little heart. | 7/7 (luck) |

### Lundenwic (11)

| # | Card | What it asks | The stake | Words |
|---|---|---|---|---|
| 50 | `harbour.chain_black_stones.s02_reeve` | The king's reeve walks down the Strand: no ship of the Frankish king's people is to land. It comes only if card 4 was met at home. | 20 d and a bribe for half toll, or three tides lying off, or go round by Hamwic. | 10/10 |
| 51 | `harbour.chain_pilgrim_road.s01_waiting` | A widow, her priest and a girl want a keel to Quentovic. The priest keeps his back to the road. | 30 d in fares, or carry them for a prayer. Two passengers, and the chain of the pilgrim road. | 10/10 |
| 52 | `harbour.three_nights.lundenwic` | Tatberht's yard, where Frisians lodge. | 4 d for three nights. In winter, 30 d for the Thames marks from the old pilot. | 13/13 (hour) |
| 53 | `harbour.black_stones_wanted.lundenwic` | A baker holds up a quern worn so thin the light comes through. | 80 d for four pairs now, or an order for eight pairs at well over the price. | 10/10 |
| 54 | `harbour.hounds_at_the_pens.lundenwic` | The cattle pens. Two long-legged hounds, and Cynred the wool man. | 72 d for the pair of hounds. | 14/14 (money) |
| 55 | `harbour.leofruns_looms.lundenwic` | Leofrun the cloak maker. | 45 d now and 45 d in two weeks for six cloaks, or 16 d for a short one. | 13/13 |
| 56 | `harbour.moneyers_pan.lundenwic` | Ibba the moneyer weighs your purse. | Nothing on a first visit. | 12/12 |
| 57 | `harbour.priest_of_saint_pauls.lundenwic` | The priest of the bishop's church, a walk from the beach. | 4 d for a copy of Alcuin's letter. A letter to carry to Canterbury. 380 d for a book, if you carry one. | 14/14 |
| 58 | `harbour.wine_over_the_side.lundenwic` | A Frankish master who is not let land offers two casks over the side. | 70 d for two casks, and 4 times in 10 a fine. | 10/10 |
| 59 | `harbour.wynflaeds_stick.lundenwic` | Wynflaed's ale-house, and a young man who owes eight pennies. | 8 d clears him and he is your hand, if there is an empty oar. 3 d for a round. | 13/13 |
| 60 | `harbour.comb_pit.lundenwic` | Hunfrith the comb maker. | 27 d for three dozen combs, or an order for six bundles of antler. | 12/12 |

### What the hour holds

- 5 of the sixty are pressing (15, 16, 38, 39, 40). The other 55 wait a week.
- By the census's own measure 34 have a real stake (10 d or more, goods, a man, the law, an order), 5 give knowledge
  and ask little, and 21 move only heart, health and what people think of you. Of those 21, 18 are met under way.
- 4 start a chain: the debt (2), the black stones (4, 50), the old gods (40), the pilgrim road (51). All four can be
  played to an end inside the twelve ports.
- None of the nine chains that can start and never end (`research/next/cards.md`) is in the hour.
- 6 give a riddle or a text for nothing (6, 14, 23, 33, 36, 37). 4 sell one (7, 8, 10, 57).
- The dearest single choice is the book at Saint Paul's (57, 380 d), then the mail coat (11, 260 d). Both need a rare
  thing in the hold. Nothing else in the hour passes 90 d.
- 43 of the sixty show a named person's face, 17 a role's stand-in.

---

## Faults met while reading, by card

All 420 cards that can open in the first release were read. Nothing here was changed. Each line is for you to decide.

### 1. A choice sends the story out of the twelve ports

The next step is set at a port the first release does not have. The step never opens.

| Card | Choice | Sends to |
|---|---|---|
| `harbour.chain_black_stones.s02_reeve` | round | Hamwic |
| `harbour.chain_black_stones.s04_letter` | refuse | Hamwic |
| `harbour.chain_black_stones.s06_quentovic` | all three | Rotomagus |
| `harbour.chain_house_marriage.s02_four_houses` | frisian, english | Winaldum, Eoforwic |
| `harbour.chain_pilgrim_road.s05_half_penny` | take, price | Dofras |
| `harbour.chain_saxon_exiles_804.s01_column` | all three | Emuthon |
| `harbour.priest_for_the_ems.traiectum` | carry, fee | Emuthon (a passenger for a port not in the game) |
| `harbour.hounds_at_the_pens.lundenwic` | tally | Hamwic (an errand) |
| `harbour.quentovic.pilgrims_grave` | bundle | Dofras (an errand) |
| `harbour.heath_iron.daventre` | load | Noviomagus (three crates to deliver) |

### 2. A card with no way to say no, or not yet

When the week runs out the card must take one of its choices, and each of them commits you.

- `harbour.chain_house_marriage.s02_four_houses`: four roads to a bride and no "not this year". The side file takes the
  Frankish house (the nearest, and inside the twelve ports).
- `harbour.years_famine.corn_805`: with no wheat aboard the only open choice is a promise of six sacks in forty days.
- `harbour.chain_kentish_revolt_796.s10_as_far_as_the_marsh`: every choice takes people or goods off the beach.
- `harbour.chain_black_stones.s01_unsold_stones`: all three choices send the reeve's card, even "ask what the kings
  fell out over".
- `crew.bored.wonders`: the cup must go to one of three men. Small.

### 3. A card where every choice has a condition

On a bad day none can be taken, and the card cannot answer itself.

- `crew.fight.one_must_go`: needs 6 d, or a Christian port. At Ripa with an empty purse there is nothing to press.
- `harbour.chain_house_heir.s08_written`: needs the school road, or 60 d.
- `harbour.chain_house_debt.s12_enemy` and `harbour.chain_pilgrim_road.s12_after_the_marsh`: fine in practice. Their
  conditions are two halves of one flag, so one choice is always open.

### 4. A card that pays far more than a voyage

`GAME_MAKER.md` 4.1: a leg makes 10 to 40 d, a good voyage 100 to 300 d, a wine crossing clears about 150 d.
The most one choice can pay:

| Card | The most it pays |
|---|---|
| `harbour.chain_black_stones.end_peace` | 1,500 d (its lowest choice pays 600) |
| `harbour.chain_house_rival.end_partner` | 1,400 d (lowest 700) |
| `harbour.chain_kentish_revolt_796.end_spared` | 1,200 d (lowest 600) |
| `harbour.chain_pilgrim_road.end_praen` | 1,200 d (lowest 600) |
| `harbour.chain_pilgrim_road.end_minster` | 1,200 d (lowest 650) |
| `harbour.chain_pilots_secret.end_sold` | 1,200 d |
| `harbour.chain_house_heir.s08_oar` | 1,100 d (lowest 600) |
| `harbour.chain_house_rival.end_ruin` | 1,100 d (lowest 450) |
| `harbour.chain_kentish_revolt_796.end_mercia` | 1,100 d (lowest 250) |
| `harbour.chain_house_heir.s08_written` | 1,000 d (lowest 760) |
| `harbour.chain_house_rival.end_feud` | 1,000 d (lowest 600) |
| `harbour.chain_house_heir.s08_two_keels` | 900 d (lowest 500) |
| `harbour.chain_kentish_revolt_796.end_both` | 800 d |
| `harbour.chain_pilgrim_road.end_quiet_kent` | 800 d (lowest 400) |
| `harbour.chain_pilots_secret.end_book` | 700 d (lowest 400) |
| `harbour.chain_pilots_secret.end_boy` | 650 d (lowest 300) |
| `harbour.chain_old_gods.s14_end_broken` | 640 d |
| `harbour.chain_kentish_revolt_796.end_kent` | 600 d |
| `harbour.chain_house_marriage.s06_danish_handclasp` | 480 d (after 260 d paid) |
| `harbour.chain_house_debt.s12_enemy` | 466 d |
| `harbour.chain_old_gods.s13_end_custom` | 400 d (every choice) |
| `harbour.priest_of_saint_pauls.lundenwic` | 380 d for a book |
| `harbour.chain_house_marriage.s06_frankish_deed` | 300 d (after 253 d paid) |
| `harbour.chain_house_marriage.s08_keel_home` | 300 d |

All but the book are the last cards of a chain. The endings pay from 250 d to 1,500 d, against a debt of 480 d. As
much as a crossing (150 to 260 d) in one choice: `harbour.years_arms.mail_buyer` (260),
`harbour.chain_house_debt.s12_enemy` letter (240), `harbour.chain_house_rival.s07_widow` (240),
`harbour.chain_old_gods.s12_end_church` (240), `harbour.chain_house_debt.s09_hawk` (200),
`harbour.chain_house_rival.s06_thing` (200), `harbour.chain_pilgrim_road.s10_sealed_leaf` (200),
`harbour.chain_storm_vow.end_commuted` (200), `harbour.chain_kentish_revolt_796.s10_as_far_as_the_marsh` (160),
`harbour.chain_pilots_secret.s05_thiadbod` (150), `harbour.chain_storm_vow.s04_buyer` (150).

### 5. A choice that costs nothing and does nothing

25 choices. Most are the honest "leave it", and may be meant so. They are listed because nothing remembers them:
no flag, no feeling, no heart.

- Sea: `sea.squall.line` bare, `sea.smoke.ashore` on, `sea.light.ashore` off, `sea.sail.far` hold,
  `sea.pilgrims.adrift` on, `sea.birds.land` hold, `sea.whale.blow` away, `sea.herring.shoal` through,
  `sea.star.ship` lie, `sea.river.weir` round, `sea.river.tow` wait, `sea.river.mist` wait.
- Crew: `crew.faith.vow` leave, `crew.tongue.norse_more` later, `crew.tongue.english` leave, `crew.tongue.letters`
  leave, `crew.riddle.dew` bread, `crew.riddle.hail` bail, `crew.read.latin_leaf` chest, `crew.hire.second_son` no.
- Ports: `harbour.chain_house_new_ship.s04_iron` none, `harbour.quentovic.salt_leaf` later,
  `harbour.quentovic.old_deacon` left, `long.offa_quarrel.ports_shut` north, `long.arms_ban.border_posts` west.

`sea.squall.line` is the one to look at: dropping the sail before a squall is the right answer and the card gives
nothing for it.

### 6. The odds word is written into the choice

65 cards put "(sure)", "(likely)" or "(doubtful)" inside the label: all 62 open cards of the sea deck, and
`chain_pilots_secret` s04, s07 and s08. The engine already shows a chance as that same word beside the choice, so a
gamble will show it twice. The words agree with the numbers in every case.

### 7. Words from part (b) of period_words.md

Found by whole word, then read. "Said" is a line in a mouth; "telling" is the card's own voice.

**To change, I think (54 places):**

| Word | Where |
|---|---|
| England (5, all said) | `crew.chain_storm_vow.s02_what_was_said`, `crew.home.frisian_wife`, `crew.winter.haul_out`, `harbour.eobas_bench.fordwic`, `harbour.quentovic.lent_at_the_cell` |
| business, as "my affair" (9) | said: `harbour.chain_house_marriage.s02_four_houses`, `...s05_danish_hearth`, `harbour.chain_house_new_ship.s01_wright`, `harbour.chain_kentish_revolt_796.s02_reeve_asks`, `harbour.bishops_hall.traiectum`; labels: `harbour.chain_pilgrim_road.s03_no_letter`, `...s10_sealed_leaf` (and its telling), `harbour.letters_in_the_porch.fordwic` |
| skipper (14) | said: `harbour.chain_house_heir.s02_school`, `harbour.whose_guest.ripa`, `harbour.dead_guest.ripa`; label: `crew.leave.bigger_ship`; telling: `harbour.chain_house_heir.s01_three_roofs`, `...s03_word` (twice), `harbour.chain_kentish_revolt_796.end_mercia`, `harbour.chain_old_gods.s05_dane`, `...s13_end_custom`, `harbour.chain_riddle_trail.s01_stick`, `long.offa_quarrel.ports_shut`, `harbour.grave_at_de_heul.dorestad`, `harbour.wiltruds_house.walichrum` |
| soldier, soldiers (6) | `harbour.north_of_the_grave.daventre` (four lines), `harbour.hollow_on_the_heest.daventre`, `harbour.across_one_cloth.daventre` |
| officer (4) | `harbour.chain_house_debt.s02_rival`, `...s13_st_martin`, `harbour.chain_saxon_exiles_804.s01_column` (telling and label) |
| hour, as sixty minutes (4) | `harbour.first_question.dorestad` ("an hour of tide left"), `crew.bored.calm` ("an hour's pull"), `crew.heart.deor` ("a hard hour at the oars"), `harbour.three_nights.lundenwic` ("the flood at its second hour") |
| money (4) | `harbour.hounds_at_the_pens.lundenwic` ("as if it owed him money"), `harbour.kings_mint.dorestad` (two lines and a label: the riddle of what money was) |
| luck, lucky (3) | `sea.porpoise.roll` ("take them as luck"), `sea.rat.bread` ("with luck"), `crew.song.fitt_26` ("a man is lucky") |
| the rest (5) | `harbour.vecht_hand.traiectum` ("his first wages"), `harbour.chain_house_debt.s08_host` ("his dinner"), `harbour.mass_at_the_cross.witla` ("keeps better hours"), `crew.song.fitt_22` ("a kind of sorry"), `sea.raven.yard` ("the best luck there is") |

"Skipper" is one ruling, not fourteen: the role is named `skipper` in the roles table too.

**Right as they stand, I think (26 places):** the hours of prayer and of the tide ("the ninth hour", "between the hours",
"says the hours", "wait for the hour", "the day and the hour", "the first quiet hour", the schoolmaster's tide hours in
`harbour.chain_pilots_secret.s09_leaf`, the title "A bad hour"); "the luck of the house" as a thing a house has
(`harbour.chain_house_marriage.s05_danish_hearth`, twice); "sorry" as pity or as wretched, not as an apology
(`harbour.chain_house_debt.s07_bishop`, `harbour.chain_house_heir.s02_strand`, `harbour.chain_house_rival.s06_thing`);
"to please" as a verb (`harbour.chain_dirham_hoard.s01_dying_gute`, `harbour.chain_house_debt.s03_michaelmas`); "the
loan of my adze" and "on loan"; "hand-money" (`crew.hire.second_son`, `crew.winter.spring`); and two quotations from
the sources: "the cause of religion" from King Charles's letter (`harbour.chain_black_stones.end_peace`) and "contempt
for Christianity" from the Saxon law (`crew.faith.lent_meat`).

**Not the word at all (2):** "wages war" (`crew.riddle.fog`) and a shield's "boss" (`crew.song.fitt_36`).

### 8. Who is speaking

- **The schoolmaster of Traiectum is two men.** `harbour.chain_pilots_secret.s09_leaf` and `end_book` call him "an
  Englishman, unnamed". The places file has Radger, a Frisian who learned beside Liudger. The side file shows the
  role's stand-in and names Radger as likely, until you say which.
- **A passer-by marked as the port's own.** `harbour.chain_storm_vow.s08_thors_man` has "an old shipmaster who no
  longer sails" as a fixed `skipper` at Ripa. The engine keeps one holder of a fixed role at a port, so he and Ripa's
  skipper are one man with one memory; in the places file that skipper is Roulf, who is about thirty. The bride of
  `harbour.chain_house_marriage.s07_morning_gift` is a fixed `weaver` in the same way: married at Lundenwic she would
  be Leofrun the cloak maker. The side file gives both the role's stand-in.
- **Ten chain cards speak through a person the places files already have,** and their own notes call the speaker
  invented: the abbot's clerk at Quentovic (Hildric), the wright and the smith of `chain_house_new_ship` (Gerbrand,
  Einbert), the Canterbury moneyer of the Kentish revolt (Eoba), the sheep farmer's wife of `chain_old_gods`
  (Bernhild, four cards), its Danish skipper (Roulf), and the carter of the riddle trail (Alli). I joined them by
  hand in `Bake/content/card_rules_overrides.json`. Say if any is wrong.
- **Eleven open cards have a named person of their own** who is in no places file and has no portrait slot: Folkmar
  the quern cutter, Hegbjorn the Gotlander, Wigheard the priest, Beornheah the thegn (two cards), Ketil the dealer,
  Wilburh the widow (three cards), Radbern of the wine fleet, Putul the old deacon. They show the role's stand-in.

### 9. Two numbers for one debt

`harbour.chain_house_debt.s01_tally` says the 480 d are due at Michaelmas and sets the count's day 100 days on.
`trade_rules.json` (`start.due_days`) says 365. `crew.pay.count_or_crew` asks for `debt_due_days < 40`, which is the
trade's count, not the card's.

### 10. Harbour cards that read as pressing

I kept every harbour card ordinary: she is moored, and a week is your ruling. These four read as if they cannot wait a
week. Yours to rule.

- `harbour.chain_black_stones.s04_letter`: the reeve comes alone, after dark, with a letter.
- `harbour.eel_stakes.medemelacha`: a boat comes alongside without a light, and a man speaks low.
- `harbour.chain_kentish_revolt_796.s10_as_far_as_the_marsh`: an army is burning Kent and the beach is full of people.
- `harbour.chain_house_debt.s03_michaelmas`: the count's day itself.

### 11. Things the engine does not yet do with these cards

Not card faults, but met on the way.

- **No card is a raider.** No card file has a raider meeting, and none has `rate: "traffic"` (a sail within hail from
  the sea sim). The one card with pirates in its name is `sea.chain_mission_north.s02_pirates`, shut in the first
  release. Your example of a pressing card has no card yet.
- **`long.offa_quarrel.ports_shut` cannot come.** It would be among the first cards at home: it tells a new player why
  the English ports are shut. Stage `long` has no draw in the scheduler (`research/next/cards.md`, section 1).
- **A week of waiting is counted by nobody yet.** The side file names the option and the hours. The clock that runs
  them is the lead's to build.
