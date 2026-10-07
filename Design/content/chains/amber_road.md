# Chain: amber_road (amber from the Ests at Truso)

Cards: `Assets/Kapman/Game/Data/encounters/chain_amber_road_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_amber_road.json` (227 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source; LATER marks a source written after 1066.
No spine thread is told here. The spine's hook for Truso (thread 7: "amber weighed against silver at the lake market of the Este; Danish craftsmen with scales") and its `cast.wulfstan` are used.

## The story in five sentences

A bead-maker in the west sorts amber that has come through three hands and says where it comes from: seven days east of Sliesthorp, the Wends' land to starboard all the way, a river mouth and a lake.
At Truso the king of the fort is dead and lies in his own house, kept cold by a kin that can make cold, while his kin drink and play away his goods; his store is shut until he burns, and a stranger must learn which bowl to take (mares' milk is the rich man's, mead the slave's, and nobody there has ever tasted ale), how to tell amber from resin and glass, and that every man who dies there is burned, whatever tongue he spoke.
The burning is set two moons off, when the dead man's goods will be laid in heaps along a mile and ridden for by the swiftest horses in the land, strangers too, so the player goes west for a fast horse, or for a brew-wife, or for nothing.
On the day he rides, or serves ale at the fire, or stands at the gate and watches, or robs the near heaps while every eye is on the road.
From that come four ends: the guest who learned the trade (what happened), the stranger who rode, ale brewed among the Ests two lifetimes before anyone could have recorded it, or a beach where strangers are held behind a line of stakes; and about 890 an Englishman called Wulfstan tells the heir of the house what he saw there, in words that differ by what the house did.

## How a player meets it

- **In the west:** card `s01_beadmaker` is drawn on a call at Ripa or Sliesthorp before 846 (weight 1.5, once). It costs nothing to begin: watching the bead-maker work is enough.
- **At Truso itself:** card `s02_wake` is drawn on almost any call at Truso before 850 (weight 6, once) and does not need step 1. A player who simply sails east finds the wake.
- Steps 3 to 6 follow at Truso by themselves over two weeks. Step 7 is drawn at Sliesthorp or Ripa while the two moons run. Step 8 may come at sea with amber or a horse aboard.
- **The afterword** (`x_wulfstan`) is drawn at Sliesthorp between 880 and 899 for any house, and has more to say to a house that finished the chain.
- A place writer may add `card:harbour.chain_amber_road.s02_wake` to a Truso beach site and `card:harbour.chain_amber_road.s01_beadmaker` to a bead-makers' workshop at Ripa or Sliesthorp.

## The real history under it

**Wulfstan.** The Old English Orosius, made in King Alfred's circle about 890, carries the reports of two seamen, Ohthere and Wulfstan (`texts_793.json` ids `ohthere`, `wulfstan`; `chronicle_793.json` 890-ohthere-wulfstan). Everything the Ests do in this chain is from Wulfstan's report. In our rendering of the Old English (no translation is quoted):

- He went from Hedeby and was at Truso in seven days and nights, the ship running under sail all the way. The Wends' land was to starboard; to port Langaland, Laland, Falster and Scania, which belong to Denmark; then the land of the Burgendas (Bornholm), which has its own king; then Blekinge, More, Oland and Gotland, which belong to the Svear; and the Wends' land to starboard as far as the Vistula's mouth.
- The Vistula divides Witland from the Wends' land, and Witland belongs to the Ests. It runs into the Estmere, which is at least fifteen miles broad. The Ilfing comes into the Estmere from the east, out of the lake on whose shore Truso stands. The Vistula takes the Ilfing's name from it and runs west and north to the sea.
- The Ests' land is very large, with many forts, and in every fort a king. There is much honey and fishing. The king and the richest men drink mares' milk, the poor and the slaves mead. There is much strife among them. No ale is brewed among the Ests, but there is mead enough.
- When a man dies he lies unburnt in his house among his kin and friends a month, sometimes two; kings and other great men the longer the richer they are, sometimes half a year. All the while there is drinking and play, until the day he is burned.
- On that day they divide what is left of his goods into five or six parts, sometimes more. They lay the largest about a mile from the fort, then the next, and the least nearest the fort. All the men with the swiftest horses in the land gather five or six miles away and ride for the goods. The swiftest comes to the first and largest part, and so each after the other; each keeps what he reaches. For this reason swift horses are very dear there.
- When his goods are all spent so, they carry him out and burn him with his weapons and clothes. Most of his wealth is spent in the long lying of the dead man within and in what is laid by the road, **which strangers ride for and take**.
- It is their custom that a man of every tongue shall be burned; if a bone is found unburnt, they must make great atonement for it.
- There is a kin among them that can make cold; that is why the dead lie so long and do not rot. If two vessels of ale or water are set down, they bring it about that both are frozen over, summer or winter.

"Ests" are Wulfstan's *Este*: Balts, the later Prussians, not Estonians (`peoples/baltic_east.md`). He never names a god of theirs, and neither does the chain.

**The task said "their ale". Wulfstan says they had none.** The chain turns on that sentence: the stranger's ale is the one drink that is neither the king's nor the slave's, and one ending changes it.

**Truso.** Found in 1982 at Janów Pomorski on Lake Drużno near Elbląg (Marek Jagodziński's excavations): a harbour settlement with amber, antler and metal workshops, folding scales and weights, Arab silver, and much that is Scandinavian in style (from memory, not opened tonight (?)). `ports_793.json` has it as an emporium of the Prussians, 750 to 850; the spine puts Danish craftsmen with scales on its beach. The fort and its "king" are Wulfstan's; where the fort stood is not known (?).

**Amber.** Gathered on the west shore (Witland, Samland) after the autumn storms and worked into beads at Ripa and Hedeby (`goods_793.json`: amber 30 d the sack; `economy/furs_amber_ivory_slaves.md`). Tacitus, Germania 45, about AD 98: the Aestii gather amber, which they call *glesum*, bring it unworked, and take their price with wonder (from memory (?)). The bead-maker and the Dane both say this is no longer true. The three tests (rubbed amber draws chaff; amber floats in brine where stone and glass sink; amber is warm on the lip and glass cold) are true of the material; that a bead-maker of 800 used them is not recorded (?). That amber draws chaff when rubbed was known to the ancients (Pliny, Natural History 37 (?)). Pine resin boiled hard and yellow glass as the frauds are our guess (?).

**Cloth for furs.** Adam of Bremen IV.18 (about 1075: LATER): the Sembi give marten skins for woollen cloaks (`peoples/baltic_east.md`). The steward's order for Frisian cloth and the marten in the heaps rest on it.

**Horses in boats.** The Danes crossed to England in 892 "with horses and all" (Anglo-Saxon Chronicle 893 for 892); the Bayeux embroidery shows it about 1070 (LATER). A horse in a boat of 7.7 m is a hard week, and the card says so. `goods_793.json`: a horse 120 d; the fast grey costs double.

**Brewing.** Women's household work everywhere in the north (`npc_roles_793.json` alewife). Ale before hops is flavoured with bog myrtle. The Old Prussian word *alu* is glossed "mead" and *piwis* "beer" in the Elbing vocabulary of about 1400 (from memory (?); LATER): the brew-wife's line that they call her ale by their word for mead plays on it.

**No church.** A baptised man who died at Truso before the 850s was a very long way from consecrated ground: Anskar's churches at Sliesthorp and Ripa come about 850 to 855 (`realms/denmark.md`). So "Christian ground" in step 5 is a grave in the sand of the Vistula spit with a driftwood cross.

**Invented, and said so.** That the king's store is shut during the wake and opens the day after the burning; that a guest is paid what he won at the wake's play; that the atonement for an unburnt bone falls on the house where the man died; how the cold was made (a pit of winter ice under the bench is shown, and the card says it is only half); that a stranger could win and be given a king's hand; the ale; the theft and the stakes on the beach; the clerk who writes Wulfstan's telling down at Sliesthorp (the account was written at Alfred's court). People: Toki the bead-maker, Ketil the Dane with the scales, Asbjorn the horse dealer and Tofa the brew-wife (Danish names of the rune stones); Nalube the steward, Gedune the king's brother, Drua the cold-wife and Nameda the seller (Prussian names recorded only in the 13th to 15th centuries, from `peoples/baltic_east.md` (?)); Wigbod and his son Wigmar, Frisians of the Weser mouth (Wig- + -bod, Wig- + -mar); an unnamed Wendish lord's man, Gotland skipper and English clerk. The dead king has no name in the cards.

No web search was used. Wulfstan's Old English is in `texts_793.json`; the rest is from the project's research files and from memory, marked where it is memory.

## The people

| Who | Id | In the cards |
|---|---|---|
| Wulfstan, an English seaman | `cast.wulfstan` (870 to 900), historical | actor `wulfstan`, afterword only |
| Toki, bead-maker at Ripa or Sliesthorp | invented | actor `beadmaker` (role craftsman) |
| Ketil, a Dane with a guest house and scales at Truso | invented | actor `host` |
| Nalube, steward of the dead king's house | invented | actor `steward` |
| Gedune, the dead king's brother, then king of the fort | invented | actor `heir` (role chief) |
| Drua, of the kin that makes cold | invented | actor `coldwife` (role wise_woman) |
| Nameda, who sells gleanings on the beach | invented | actor `seller` (role widow) |
| Wigmar, son of the dead Frisian Wigbod | invented | actor `son` (role merchant) |
| Asbjorn, a Jute horse dealer | invented | actor `dealer` (role merchant) |
| Tofa, a brew-wife | invented | actor `brewster` (role alewife) |
| A Wendish lord's man, a Gotland skipper, an English clerk | invented, unnamed | roles chief, skipper, clerk |

Spoken lines never use these names (the engine forges a name for each role): people are "the Dane", "the steward", "the brew-wife". The names are for the notes, the pictures and this page.

## The steps

Scheduled cards (weight 0) open on the first call at their port after their day. "Likely" is a chance of 0.55; "doubtful" 0.4 to 0.5, before the crew's skill.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_amber_road.s01_beadmaker` | Ripa or Sliesthorp, before 846; drawn | watch him sort (free) / buy a sack and have it sorted (36 d) / ask for an order | `ch_amber_road_1`; `_rub` (knows the tests) |
| 2 | `harbour.chain_amber_road.s02_wake` | Truso, before 850; drawn on any call | take the mares' milk / take the mead / pour your own ale | `ch_amber_road_2`; `_milk`, `_low_bench` or `_ale_tasted` |
| 3 | `harbour.chain_amber_road.s03_cold` | Truso, 3 days on | lay silver by the dead man (15 d) / ask how the cold is made (doubtful) / stand quiet | `ch_amber_road_3`; `_gift`, `_cold_known` |
| 4 | `harbour.chain_amber_road.s04_lesson` | Truso, 4 days on | test every piece (needs `_rub`; 45 d) / buy by eye (45 d; doubtful) / buy nothing and ask to be taught / go round her to the gatherers (needs `_low_bench`; 30 d) | `ch_amber_road_4`; `_rub`, `_gatherers` |
| 5 | `harbour.chain_amber_road.s05_stranger` | Truso, 4 days on | carry the dead Frisian out by night (doubtful) / stand by at the fire / pay the atonement beforehand (50 d) | `ch_amber_road_5`; `_body_out`, `_burned` or `_atoned` |
| 6 | `harbour.chain_amber_road.s06_steward` | Truso, 3 days on | say you will bring a horse / say you will bring ale and a brew-wife (needs `_ale_tasted`) / say only that you will be there | `ch_amber_road_6`; `_horse_errand`, `_brew_errand`; schedules the race 70 days on |
| 7 | `harbour.chain_amber_road.s07_west` | Sliesthorp or Ripa, between step 6 and the race; drawn, comes again after 12 days | buy the grey (240 d) / take the brew-wife east (needs `_ale_tasted`; 40 d) / both (280 d) / not now | `ch_amber_road_7`; `_grey`, `_brew` |
| 8 | `sea.chain_amber_road.s08_wends` | at sea after step 2 with amber or a horse aboard; drawn, optional | swear on the water / swear and give a guest's gift (15 d) / keep the sail full (doubtful) | `ch_amber_road_8`; `_wend_peace` (240 days) |
| 9 | `harbour.chain_amber_road.s09_race` | Truso, 70 days after step 6 | ride for the far heap (needs a horse aboard; likely) / have the ale carried up (needs `_brew`) / watch from the gate / rob the near heaps (doubtful) | `ch_amber_road_9`; `_won` or `_rode`; `_served`; `_thief`, `_truso_shut`, `_caught` |
| E1 | `harbour.chain_amber_road.end_rider` | Truso, the morning after, if he rode | winner: keep heap and horse / heap, sell the horse, ask for ground. Not first: sell the horse / keep it | `ch_amber_road_end_rider` |
| E2 | `harbour.chain_amber_road.end_guest` | Truso, the morning after, if he watched | the Dane as factor (100 d) / the way in / partner with the Frisian's son (needs `_body_out` or `_atoned`) / a chest of cold (needs `_cold_known`; 60 d) | `ch_amber_road_end_guest` |
| E3 | `harbour.chain_amber_road.end_ale` | Truso, the morning after, if the ale was served | brewhouse and the malt trade / amber now and the king's peace / the king's hand and first pick | `ch_amber_road_end_ale`, `changed_ests_brew_ale` |
| E4 | `harbour.chain_amber_road.end_shut` | Sliesthorp, 10 days after the theft | sell what you know and took / the way to the Curonians / send the whole atonement east (300 d) | `ch_amber_road_end_shut`; `changed_ests_bar_strangers`, or `_amends` |
| A | `harbour.chain_amber_road.x_wulfstan` | Sliesthorp, 880 to 899; drawn | hear him through / sell him the bars (needs a finished chain) / say nothing | `ch_amber_road_wulfstan` |

Every ending also sets `ch_amber_road_done`.

**Roads through.** Guest: (1), 2, 3, 4, 5, 6, (8), 9 watch, E2. Rider: the same with 7 (or any horse bought anywhere and carried to Truso), 9 ride, E1. Ale: 2 pour the ale, 6 promise it, 7 the brew-wife, 9 serve, E3. Thief: 9 steal, E4. A player with both a horse and a brew-wife chooses at the mile.

**No dead ends.** Every card has an option that costs only time or pride (watch, take the mead, stand quiet, ask to be taught, stand by at the fire, say you will be there, not now, swear on the water, watch from the gate; each ending has an option with no cost for whoever reaches it). A failed chance hurts and goes on: pried into the cold, a place by the door; bought by eye, one true sack and the lesson; caught with the body, 60 d of atonement and the man burned as he would have been; outrun at sea, 40 d; outrun on the mile, a nearer heap and the same ending. Dangers warn first: the Dane says which bowl is whose before the bowls are held out, says that the fishermen count boats, and says what a thief at a burning pays and who pays with him; the Wend has "more oars than you have men".

**If the days run out.** The race waits at Truso from its day on and does not pass: a player who comes back late finds the burning held when he lands.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 (or a sack of amber for 36 d) | the three tests; or an order for six sacks at +50% |
| Step | 2 | 0 | a seat at the high bench, or among the gatherers, or the king's brother's taste for ale |
| Step | 3 | 0 (or 15 d laid by the dead) | the cold-wife's goodwill, or half the secret of the cold |
| Turn | 4 | four sacks of amber for 45 d (120 d at the table's price), or five for 30 d | or the tests and half the Ests' tongue (`tongue baltic = 0.5`) |
| Step | 5 | 30 d freight; or 50 d paid; or 60 d lost | the riddle of ice (`riddle_symphosius_10_glacies`); the son's friendship |
| Step | 6 | 0 | an order at Truso: one horse at double, or six sacks of malt at double, or four bolts of Frisian cloth at +30% |
| Turn | 7 | costs 240 d and 40 d | a fast horse (worth 700 d and more at a burning); a brew-wife as passenger |
| Step | 8 | 0, or 15 d for a crock of honey (24 d) | the Wends' peace for 240 days |
| Step | 9 | 0 | (the thief: six sacks and a timber of marten, about 300 d, or 80 d lost) |
| Finale E1, winner (hard road) | Truso | 420 d, or 1,120 d with the horse sold | 14 sacks of amber and 4 timbers of marten (900 d); `unlock_title_est_guest_friend` and `unlock_heirloom_burning_bridle` or `unlock_plot_truso` |
| Finale E1, not first | Truso | 260 d, or 520 d with the horse sold | 6 to 8 sacks, a timber of marten; `unlock_title_est_guest_friend` |
| Finale E2 | Truso | 0 to 150 d | 16 to 26 sacks of amber (480 to 780 d); one of `unlock_factor_truso`, `unlock_pilot_estmere`, `unlock_partner_wigmar`, `unlock_upgrade_ice_chest` |
| Finale E3 (history changed) | Truso | 200 to 380 d | 12 to 30 sacks (360 to 900 d); malt at double at Truso for a year; `unlock_work_truso_brewhouse` with `unlock_factor_truso` and an order for twenty sacks of malt at double, or `unlock_charter_truso`, or `unlock_title_est_guest_friend` with amber cheap at Truso for a year |
| Finale E4 | Sliesthorp | 600 to 620 d (or 300 d paid) | `unlock_factor_sliesthorp`, or `unlock_pilot_curonia`, or by amends `unlock_factor_truso` |
| Afterword | Sliesthorp, about 890 | 0 or 120 d | Wulfstan's voyage for the chest (`give_text wulfstan`) |

Amber is counted at the table's 30 d a sack; it sells for more in the west, so every finale is worth more at home than it reads here.

**Why a merchant could have won these.** The far heap and the right to ride for it are in Wulfstan's own words: strangers ride and take, and each keeps what he reaches. A king's hand on a stranger's boat (`unlock_title_est_guest_friend`, `unlock_charter_truso`) is how peace was had where every fort has its own king: one bought it fort by fort (`peoples/baltic_east.md` 12.3), and guest-friendship across the sea is the north's own way (Hávamál 41 to 44). Ground by the lake (`unlock_plot_truso`) and a resident who keeps a stranger's goods (`unlock_factor_truso`) are what the foreign houses and workshops on Truso's shore were (?). The way in (`unlock_pilot_estmere`) is the passage Wulfstan describes: the Vistula's mouth, the Estmere, the Ilfing, the lake. A partner sworn over a cut stick (`unlock_partner_wigmar`) is the Frisian and Norse felag. The bridle (`unlock_heirloom_burning_bridle`) is a thing from the heap. The chest of cold (`unlock_upgrade_ice_chest`) is the least sure: winter ice kept in moss and straw is ordinary, the rest of what Wulfstan saw is not explained, and the card leaves it unexplained (?). The Curonians' shore (`unlock_pilot_curonia`) is the other amber coast, north of the Neman, with Gotlanders at Grobin until about 850.

**Costs that last.** `ch_amber_road_truso_shut` (the thief, unless he makes amends): the fort is shut to the house; a Truso writer can read it. `changed_ests_bar_strangers`: see below. `ch_amber_road_burned` and `ch_amber_road_body_out` stay set for any later card about what the crew or a priest thinks of it.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_amber_road.ending.*`) |
|---|---|---|---|---|
| The guest who learned | `ch_amber_road_end_guest` | sat out the wake and watched the riding | the store opens, the amber is sold in three days, the Dane grows old with a boat to wait for; the Ests go on exactly as they were | folding scales in balance, amber against cut silver, the lake behind |
| The stranger who rode | `ch_amber_road_end_rider` | carried a horse seven days and rode it | the horse is sung about and the rider called "the Frisian who was on him"; the new king gives his hand; nothing in the custom changes, and that is the point | a grey horse alone on the far heap, a silvered bridle on his neck |
| Ale among the Ests | `ch_amber_road_end_ale`, `changed_ests_brew_ale` | poured ale at the wake, brought a brew-wife, served at the fire | a brewhouse inside the palisade; barley has a price; a drink that is neither the king's nor the slave's | three bowls: milk, mead, and between them ale |
| A handful of the lake | `ch_amber_road_end_shut`, `changed_ests_bar_strangers` (not after amends) | robbed the heaps at the burning | the Dane pays the atonement with his house and goes to Grobin; strangers are held to the beach | stakes between the boats and the fort, one small heap nobody buys |

**What really happened** is the guest (E2): outsiders traded on the lake and went home, and Wulfstan found the Ests two generations later with their kings, their milk and mead, their cold and their mile. The rider (E1) is inside what Wulfstan says could happen, so it changes nothing either.

**History changed, and what follows.**

- `changed_ests_brew_ale`: ale is brewed at one fort of the Ests from the 790s to 840s. Wulfstan's sentence "no ale is brewed among the Ests" is not said in 890: his card has him say instead that they brew at the one fort and that a Frisian's brew-wife taught them. Barley and malt have a market at Truso (the ending sets a year of it; the game could keep malt wanted in `prussia`). A drink that carries no rank is a small thing with a long tail: a king there can now give to whom he likes. Later cards at Truso may pour ale; nothing else in the chronicle moves.
- `changed_ests_bar_strangers`: after a theft at a king's burning the Ests keep strangers on the beach. Wulfstan in 890 is not let see a wake or a riding and says so; his account of the custom becomes hearsay. Truso cards after this should not seat a stranger in the fort; the foreign shore loses the thing "no other shore gave". If the player sends the atonement (`ch_amber_road_amends`), the flag is not set and only the player's own house stays outside the palisade.

## Needs (for the integrator; nothing here was added to a shared table)

- **`ports_793.json` ends Truso in 850; Wulfstan sailed there about 890**, the table's own note says so, and the spine places `cast.wulfstan` "also at Truso". The settlement at Janów Pomorski is usually dated from the late 8th century into the 10th (?). Until the table is changed the body of this chain is gated to the years the port stands (start before 846, the wake before 850), and the afterword is held at Sliesthorp. If Truso's `to` is raised, raise `year < 846` in step 1 and `year < 850` in step 2 to match, and Wulfstan could be met on the lake itself.
- Grand reward flags new to this chain: `unlock_title_est_guest_friend`, `unlock_heirloom_burning_bridle`, `unlock_plot_truso`, `unlock_factor_truso`, `unlock_charter_truso`, `unlock_pilot_estmere`, `unlock_pilot_curonia`, `unlock_partner_wigmar`, `unlock_work_truso_brewhouse`, `unlock_upgrade_ice_chest`, `unlock_factor_sliesthorp`.
- History flags for later cards to read: `changed_ests_brew_ale`, `changed_ests_bar_strangers`. The saga of the Baltic east and any Truso place file should read both, and `ch_amber_road_truso_shut`.
- `goods_793.json`: horses are not listed as wanted in `prussia`, and Wulfstan says swift horses are very dear there. The chain pays for the horse through an order and the endings; a `wanted` entry would let the market say it too.
- `npc_roles_793.json` `named`: if Wulfstan is to carry his own name on the card, he needs a row (role skipper, english, Sliesthorp, 870 to 900). The other names on this page are notes only.
- `texts_793.json` `wulfstan` is given about 890 by the afterword and by nothing earlier: no card of this chain hands a text of 890 to a player of 800.
- The engine queues a scheduled card when its day comes without asking its `when`. Every scheduled card here is scheduled only by the option that makes its `when` true.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | A workshop booth by the landing: a Danish bead-maker rubbing raw amber on his sleeve among yellow chips, a bowl with pieces floating in it |
| s02 | The beach at Truso, grey and still: boats drawn up and nothing for sale; a Dane with scales, a steward holding out two bowls, the fort smoking behind |
| s03 | A dark log house: the dead king dressed on a bench with his weapons, frost on the posts, an old woman by two bowls skinned with ice |
| s04 | The shingle in morning sun: a woman behind three wet shining heaps; the Dane with his scales, looking at the lake; gatherers raking weed far down the shore |
| s05 | The guest house by rushlight: a dead Frisian under his cloak with a small cross on his chest, his son's hand on him, the steward and the Dane in the door |
| s06 | Before the gate: the steward dividing goods into five heaps, the king's brother pointing down a straight road over a rise |
| s07 | Behind the landing at Sliesthorp: a Jute holding a lean grey horse; a brew-wife in the next doorway leaning on her mash paddle |
| s08 | The southern Baltic at evening: a long Wendish boat across the bow, a man with one open hand raised; a blindfold horse lying on straw in the small boat |
| s09 | The road from the gate at midday: heaps along a straight mile, dust and the first horses far off, a crowd on the palisade |
| end_rider | The morning after: the dusty grey tied at the Dane's door, the new king's hand held out, women raking ash behind |
| end_ale | Inside the palisade: an empty cask, the brew-wife with folded arms, the king holding a bowl upside down, men setting posts for a new house |
| end_guest | The king's store thrown open: amber carried into the light, the steward with scales raised, a line of foreign skippers |
| end_shut | The bead-makers' row at Sliesthorp: a Gotlander drawing a coast in spilt ale, a bead-maker weighing silver without looking up, the row staring |
| x_wulfstan | The jetty at Sliesthorp about 890: an Englishman counting lands on his fingers, a clerk with a wax tablet, a Frisian skipper listening with folded arms |

## How long it takes

From the wake to the day the burning is named: 3, 4, 4 and 3 days, two weeks at Truso or coming and going from it. Then 70 days to the burning, in which the player sails to Sliesthorp or Ripa and back (seven days each way by Wulfstan's reckoning from Hedeby) with time to trade. The ending is the morning after; the thief's is ten days later at Sliesthorp. So the chain is one sailing season, about a hundred days from the first landing at Truso, and for seventy of them it is one promise and a date. The afterword waits two generations: it comes at Sliesthorp between 880 and 899, to the heir of the house, and what Wulfstan says there depends on what was done at the mile.
