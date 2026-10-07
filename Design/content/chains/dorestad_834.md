# Chain: dorestad_834 (the four summers of the fleet)

Cards: `Assets/Kapman/Game/Data/encounters/chain_dorestad_834_793.json` (14 cards: ten steps, four endings).
Words: `Assets/Kapman/Game/Data/writing/chain_dorestad_834.json` (228 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
The spine's thread `dorestad_falls` (Design/content/spine.md no. 11), its first part: 834 to 837. Rorik (850) and the last sack (863) are other chains' (`rorik_850`, `thread_11`).

## The story in five sentences

In the early summer of 834 a coast fisherman has seen forty ships of the Danes in Frisia asking the way up the Rhine, and Dorestad, an hour's walk of storehouses with no ditch round it, laughs at him.
The player can carry the word and be believed or mocked, bury the house's chest, run up the Lek with the neighbour's children or stand on a dam head with Thiadlef, Thiadbod's son; the fleet comes by Utrecht all the same, and among the people it carries off is Thiadlef's daughter, who can be found that year or the next on King Horik's landing at Sliesthorp.
In the second summer the men of the dam heads meet in the upper church with three opinions: dig a ditch and a bank, lay silver on a cloth at the river, or load a barge for the IJssel.
The player picks a road and pays for it: shares of 300 and 500 pennies and a journey to the palace at Nijmegen for leave and oak; or a house moved to Daventre a boatload a month; or stock kept short and a boat kept loaded; or a gamble on a full store in a frightened town.
In June 837 the fleet kills the count on Walcheren and comes up the river a fourth time, and finds a shut gate, or a cloth of silver, or a house that has gone, or one that burned the year before.

## How a player meets it

- It starts by itself in the delta in April, May or June 834: card `s01_sails` is drawn (weight 5, once) on a call at Dorestad, Traiectum, Witla, Walichrum or Medemelacha.
- A house that was away that spring meets it at step 4: `s04_aftermath` is drawn at Dorestad from July 834 to the end of 835 for anyone who has not seen it. The long card `long.dorestad.sacked_834` (in `long_793.json`) tells the news in other ports and points home.
- A place writer may add `card:harbour.chain_dorestad_834.s01_sails` to `dorestad.quay` and `card:harbour.chain_dorestad_834.s07_second_summer` to `dorestad.upkirika`.

## The real history under it

**834.** Annales Bertiniani 834: a fleet of Danes came into Frisia and laid waste a part of it; from there they came by Utrecht (per Vetus Treiectum) to the emporium called Dorestad and plundered everything; some of the people they killed, some they carried off, and a part of the place they burned (our rendering). Annales Xantenses 834 has the same more shortly. The emperor Louis had been put down by his sons in 833 and was restored on 1 March 834; Lothar was still in arms that summer. The realm file (`realms/frisia_lotharingia.md`) reads the coast watch as lapsed in those months (?). The month of the raid is not recorded (?): the game puts it between Pentecost and Lammas.

**835.** Annales Bertiniani 835: the Northmen fell on Dorestad again and laid it waste; the emperor, angered, came to Aachen and set guards along the coasts (our rendering, from memory tonight (?)). Annales Xantenses 835.

**836.** Annales Bertiniani 836: Dorestad and Frisia again. Annales Fuldenses 836: Antwerp burned, and Witla, the emporium by the mouth of the Maas, and tribute taken of the Frisians. Horik, king of the Danes, sent envoys to the emperor to say he had no part in it, and complained of envoys of his killed near Cologne (Annales Bertiniani 836; the place of the assembly not checked (?)); in 838 he said he had taken and killed the chiefs.

**837.** Annales Bertiniani 837: the Northmen fell on Walcheren, killed many and plundered more, stayed a while and took what tribute they chose, then came with the same fury to Dorestad and took tribute there in the same way. The emperor gave up his journey to Rome and hurried to the fort at Nijmegen; they withdrew on hearing of it; his inquiry found the coast lost partly by what could not be done and partly by the disobedience of some, and he ordered a fleet built. Annales Fuldenses 837 names the dead: Eggihard, count of the place, and Hemming, Halfdan's son; the day is 17 June. (`chronicle_793.json`: 834-dorestad-sacked, 835-dorestad-again, 836-witla-antwerp-burned, 837-06-17-walcheren-raid.)

**The place.** Dorestad had no wall, rampart or ditch; no count is named there, only the king's agents (procuratores rei publicae); Saint Martin's upper church is the one church named (charter of 8 June 777); the diggers found no burnt layer (`dossiers/dorestad.md`). The king's tenth was taken there even from the emperor's own merchants: the Praeceptum negotiatorum of 828 frees them of toll everywhere except Quentovic, Dorestad and the Clusae (Formulae imperiales 37).

**Sliesthorp.** Dorestad merchants traded at Sliaswich (Rimbert, Vita Anskarii 24). A Christian captive seen for sale there and ransomed on the spot is in the Vita Rimberti c. 18, a generation LATER (about 870 to 888). Anskar bought boys' freedom (Vita Anskarii 15).

**Daventre.** Lebuin's church of about 770, burned by the Saxons and rebuilt by Liudger (Altfrid, Vita Liudgeri I); a landing on the IJssel; where Dorestad's trade goes after 863 (the spine).

**Invented, and said so.** The fisherman and his count of forty hulls (no number is recorded). That anyone warned Dorestad. The bank, ditch, gate and watch: nothing of the kind was built there, and the ring forts of the Zeeland coast are of about 880 to 890. That a bank needed the palace's leave in the 830s: the rule is written down only in the Edict of Pitres, 864 (?). The oak "marked for a bridge". How the tribute of 837 was raised, its size, and that captives were given back with it. The ransom price (180 d; `goods_793.json` values a captive at 48 d). The penny a year to Saint Lebuin for ground at Daventre (?). People: Thiadlef is the place file's `dorestad.thiadlef` (Thiadbod's son, 825 to 852); ours are his daughter Hildswith (Hild- + -swith), the dealer Ulf, and the unnamed fisherman, king's agent, priest, widow, king's man of Horik's, steersman, palace clerk, ship's chief, pacer of plots and Dorestad skipper.

No web search was used: the history is the spine's, the dossiers' and the realm file's, with the sources they name; what was added from memory is marked (?).

## The people

| Who | Id | In the cards |
|---|---|---|
| Thiadlef, Thiadbod's son, skipper of Dorestad | `dorestad.thiadlef` (825 to 852) | actor `rival` |
| Hildswith, his daughter, fourteen in 834 | invented | actor `girl` |
| The king's agent on the dams | invented, unnamed | actor `agent` (role steward) |
| The priest of the upper church | invented, unnamed | actor `priest` |
| A widow with a dam head | invented, unnamed | actor `widow` |
| Ulf, who sells what the ships bring to Sliesthorp | invented | actor `dealer` (role slave_dealer) |
| A man of King Horik's on the landing | invented, unnamed | actor `kingsman` |
| A clerk of the palace at Noviomagus | invented, unnamed | actor `clerk` |
| A ship's chief of the fleet of 837; a steersman of the fleet of 836 | invented, unnamed | actors `chief`, `raider` |
| The pacer of plots at Daventre; a Dorestad skipper with the news | invented, unnamed | actors `steward`, `skipper` |

The emperor Louis (`noviomagus.louis`), King Horik (`cast.horik`), Count Eggihard and Hemming are spoken of and never met.

## The steps

Card ids are `harbour.chain_dorestad_834.<step>` (step 9 is `sea.`). All flags below are `ch_dorestad_834_<name>` unless they begin `df_`, `changed_` or `unlock_`. "Likely" is a chance of 0.55 to 0.7, "doubtful" 0.4 to 0.5, before the crew's skill.

| # | Card | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `s01_sails` | a delta port, April to June 834; drawn | carry the word at once (1 day) / feed him, ask it twice, then carry it (3 days) / finish trading (6 days) | `1`; `warn`; `count` |
| 2 | `s02_dams` | Dorestad, scheduled | swear to the agent (needs the count; sure) / cry it on the dams (doubtful) / tell the priest / say nothing and buy three bolts at 20 d | `2`, `df_warned`; `believed` with `changed_dorestad_834_warned`; `church`; `profit` |
| 3 | `s03_past_utrecht` | Dorestad, 5 days on: the eve of the sack | run up the Lek with the neighbour's children / bury the chest and watch from the alders (likely) / stand on the dam head with Thiadlef (doubtful) | `3`; `ran_834`; `df_buried`, `buried`, `saw`; `stood` |
| 4 | `s04_aftermath` | Dorestad, 8 days on; or drawn there to the end of 835 | promise to ask after the girl / 30 d for shrouds and bread / buy a silver cup with the cross filed off (90 d) | `4`; `seek`; `plate` |
| 5 | `s05_horiks_harbour` | Sliesthorp, 834 to 836; drawn | ransom her at 180 d / bargain in Norse, 110 d / buy nothing and ask the king's man when they sail | `5`; `ransomed`; `knows` |
| 6 | `s06_homecoming` | Dorestad, 10 days after a ransom | the price twice over / the price and a wine order / nothing | `6`; `thiadlef_owes` |
| 7 | `s07_second_summer` | Dorestad, March 835 to March 836; drawn | first share for the bank, 300 d / start moving to Daventre / keep stock short | `7`; `road_wall` and `wall_1`; or `road_flight` |
| 8 | `s08_palace` | Noviomagus, 835 to 837, once a share is paid; drawn | ask leave as the man who warned / ask leave with 60 d / ask nothing | `8`; `leave` |
| 9 | `sea.…s09_fleet_at_sea` | at sea, May to August 836; drawn, optional | run for the Lek with the word (likely) / lie under the dunes and count / sell them two casks of ale | `9`; `df_warned`, `warned_836`; `saw`; `fed_fleet`, `knows` |
| 10 | `s10_third_summer` | Dorestad, April 836 to May 837; drawn | second share, 500 d: the gate and the watch / take the rest up the river / fill the store on the cheap, 150 d (an even chance) / change nothing | `10`; `wall_2`; `road_flight`; `filled` or `burned` with `df_buried` |
| E1 | `end_wall` | Dorestad, June 837 to 838, with `wall_2` | the emperor's writing / a seat in judgment / both (needs `leave` and `df_warned`) | `end_wall`, `changed_dorestad_837`, `watch_due`; `end_wall_hard` |
| E2 | `end_tribute` | Dorestad, June 837 to 838, by default | advance 300 d against pledges / carry the silver down and speak for the taken / bind the houses (needs the ransom) / pay your share and carry by night | `end_tribute`; `spoke` |
| E3 | `end_flight` | Daventre, June 837 to 838, with `road_flight` | ground by the church / a sworn man and a hired roof / two plots and Thiadlef's household | `end_flight`, `df_moved`, `left_early`; `end_flight_hard` |
| E4 | `end_ashes` | Dorestad, June 837 to 838, with `burned` | the widow's dam head, cheap / one roof with Thiadlef / the scales out of the ashes | `end_ashes`, `df_buried` cleared; `bought_cheap` |

**Which ending.** The bank finished (`wall_2`) comes first; then a burned store; then a house that has moved; and everyone else has the tribute. A player who paid the first share and not the second, or who was not at Dorestad in the third summer, has the tribute ending, with a line about the bank that stops at the road.

**Roads through.** Builder: 1, 2, 3, 4, (5, 6), 7 wall, 8, (9), 10 gate, E1. Payer: 1 to 4, (5, 6), 7 short, (9), 10 short, E2. Mover: 1 to 4, 7 move or 10 leave, E3. Gambler: 1 to 4, 7, 10 fill and lose, E4.

**No dead ends.** Every card has an option that costs only time or pride (carry or trade on; tell the priest; run; promise; drink and ask; take nothing; keep stock short; listen; lie under the dunes; change nothing; and at every ending at least one option with no check). A failed chance hurts and goes on: laughed off the dams, the warning still counts and is remembered; found in the alders, a beating and the chest safe; on the dam head, a wound and Thiadlef's good word; caught at sea, a toll of stores; the full store burned, and its own ending. Each danger warns first: Thiadlef says himself that standing is not likely; the straggler "has more oars than you have men"; the gamble's label says what is staked.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 | the news of the fleet, true; the fisherman's count |
| Step | 2 | 0, or three bolts for 60 d (worth 144 d) | `df_warned`; the agent's or the priest's feeling |
| Step | 3 | 0, or 40 d (an arm ring) | a riddle (`riddle_exeter_83_ore` or `riddle_exeter_05_shield`); deed fought_raiders; Thiadlef's feeling |
| Step | 4 | 0, or a piece of church plate for 90 d (worth 240 d) | deed gave_alms; the news of the sack |
| Turn | 5 and 6 | the ransom back twice (360 d), or 180 d and an order for ten casks at +50% | or Thiadlef's steersman hired and his debt of honour |
| Step | 7 | 0 | half toll at Dorestad for 60 days (builders) |
| Turn | 8 | six loads of the king's oak (180 d), or three for a gift of 60 d | leave for the bank; latin +0.05 |
| Step | 9 | 60 d for two casks of ale | `df_warned` again, or the fleet's course |
| Turn | 10 | 420 d for 150 d staked (an even chance) | or half toll for 90 days (builders) |
| Finale E1 | Dorestad | 1,200 or 1,500 d | `unlock_work_dorestad_wall` and `unlock_charter_dorestad` or `unlock_title_doomsman_dorestad` |
| Finale E1, hard road | Dorestad | 2,400 d | `unlock_work_dorestad_wall`, `unlock_charter_dorestad` and `unlock_title_doomsman_dorestad` |
| Finale E2 | Dorestad | 600 to 900 d, or about 1,050 d in cloth and wine for 300 d advanced | one of `unlock_work_dorestad_jetty`, `unlock_factor_sliesthorp`, `unlock_kin_thiadlef`, `unlock_pilot_lek` |
| Finale E3 | Daventre | 700 to 1,100 d | `unlock_plot_daventre`, or `unlock_factor_daventre`, or the plot with `unlock_partner_thiadlef` (hard road) |
| Finale E4 | Dorestad | 700 to 900 d (the chest from the ground) and an order for ten loads of oak at +50% | one of `unlock_work_dorestad_jetty`, `unlock_partner_thiadlef`, `unlock_heirloom_dorestad_scales` |

The builder pays 800 d in shares (and 60 d if he buys his leave) and has them back with a third over, or twice over on the hard road.

**Why a merchant could have won these.** Freedom from the tenth at Dorestad was the king's to give and he gave it to churches; the rule of 828 shows that laymen asked. A seat among those who find judgment is what a free man of standing held in a count's court. A dam head forfeited on a pledge or sold by a widow is how plots changed hands on the bank. A go-between at Sliesthorp is the kind of man Rimbert shows there. A betrothal seals a debt of honour between two houses. A pilot's knowledge of the Lek is four summers of running it in the dark. Ground at Daventre is the church's to pace out. A sworn partner with his own boat is the oldest form of company on the river. A pair of scales that went through a fire and still weigh true is what a merchant's heir would be shown.

**Costs that last.** `ch_dorestad_834_watch_due` (E1): the house owes the watch a man and his bread from Pentecost to Lammas, every year, and so does the heir. `ch_dorestad_834_left_early` (E3, unless Thiadlef came too): the dams remember who went first. `ch_dorestad_834_bought_cheap` (E4, the widow's dam head). `ch_dorestad_834_fed_fleet` (step 9): ale sold to the fleet, for a priest or a count's man to hear of. `ch_dorestad_834_plate` (step 4): a cup from the fire under the bottom boards. `ch_dorestad_834_profit` (step 2): bought cheap and said nothing.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_dorestad_834.ending.*`) |
|---|---|---|---|---|
| The gate was shut | `ch_dorestad_834_end_wall`, `changed_dorestad_837` | paid both shares | the fleet lies two days before the bank and goes away; Thiadlef stands watch on it; the agent takes one piece of good news to Nijmegen | a shut oak gate in an earth bank at dawn, masts going away down a misty river |
| Weighed out on the dam head | `ch_dorestad_834_end_tribute` | neither built, left nor burned | silver on a cloth, nothing burned, the emperor in a rage at Nijmegen; Thiadlef says it was cheap and asks to be asked again next year | a cloth heaped with pennies, a pair of scales, a long ship's prow above |
| A new river | `ch_dorestad_834_end_flight`, `df_moved` | moved the house to Daventre | the house's mark on a new post on the IJssel, a lifetime before the rest of the bank follows | a barge with a loom, a house door and a cat going upstream, smoke astern |
| What is in the ground | `ch_dorestad_834_end_ashes` | filled the store and lost | black posts, a chest under an alder, and a choice of how to start again | bent scale pans and a box of weights in ash, a spade and an earthy chest |

**What really happened** is the tribute (E2). E3 and E4 are private fates inside the same history: the annals read the same.

**History changed, and what follows.**

- `changed_dorestad_834_warned` (step 2, when the dams believe the warning). The annal for 834 says some were killed and some carried off. Here most of the people and much of the stock are up the Lek when the ships come: fewer dead, fewer on the benches at Sliesthorp, and a fleet that goes home lighter than it came for. It comes back in 835 all the same. Later cards may have the town remember whose word it was; the count of people in the place should fall less in 834.
- `changed_dorestad_837` (E1). The annal for 837 then has tribute taken on Walcheren and not at Dorestad. What follows: the town is no longer the easy place, so the fleets of the next years go by the Maas and the Scheldt and to the Seine sooner; the bank needs twelve men every summer for ever (`watch_due`), and it is the "wall and watch paid for" that the hinge `dorestad_863` asks for (SCHEMAS.md; `proposals/living_world_least.md`), read here as the flag `unlock_work_dorestad_wall`. It does not turn that hinge by itself: the burning of about 846, Rorik in 850 and the river's leaving still come, and whether the bank stands in 863 is the later chains' to say. The emperor's inquiry at Nijmegen (chain `thread_02`) has one place to hold up against his counts.

## Needs (for the integrator; nothing here was added to a shared table)

- Grand reward flags new to this chain: `unlock_work_dorestad_wall`, `unlock_kin_thiadlef`, `unlock_pilot_lek`, `unlock_plot_daventre`, `unlock_factor_daventre`, `unlock_heirloom_dorestad_scales`. Used and already set elsewhere: `unlock_charter_dorestad`, `unlock_title_doomsman_dorestad`, `unlock_work_dorestad_jetty`, `unlock_factor_sliesthorp`, `unlock_partner_thiadlef`.
- The hinge `dorestad_863` should count `flag.unlock_work_dorestad_wall` as "the wall and watch paid for", and may read `flag.ch_dorestad_834_watch_due`.
- `df_moved` is set by E3 in 837. The chain `thread_11` (the house moves after 863) should read it: this house has its ground at Daventre already.
- `df_buried` is set by burying the chest (steps 3 and 10) and cleared only in E4. The chest is a flag, not a hoard: if `hoards_793.json` gets a row for the house's chest at De Heul, step 3 could use `open_fetch`.
- The long card `long.dorestad.sacked_834` and the chronicle entries for 834 and 837 should soften their words when `flag.changed_dorestad_834_warned` or `flag.changed_dorestad_837` is set.
- A site in `places/dorestad.json` for the bank and gate (kind `wall`, from 837) when `unlock_work_dorestad_wall` is set; the place writer's to add.
- Hildswith is carried as a passenger by actor (`passenger girl to dorestad`); if she should become a person in the place file, her id would be `dorestad.hildswith`.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | A delta landing, windy noon: a sunburned fisherman on a gunwale with blistered hands open, talking up at a merchant; nobody else listening |
| s02 | Dorestad from the river, afternoon: the comb of dams and storehouses, no wall; an agent with a tally stick, a grey skipper laughing, a priest listening apart |
| s03 | The bank road at dusk under a red sky: a boy running, cloth thrown into overloaded boats, twelve men with spears standing still on one dam head |
| s04 | A grey morning after: black posts and thin smoke; a skipper with a child's shoe in his hand; a young widow on a mooring post with her keys |
| s05 | The landing at Sliesthorp in flat light: a Dane holding up cloth with a lead seal; captives on a bench, a girl fingering knots in her belt; a king's man with a spear |
| s06 | Thiadlef's dam head, evening: the heavy grey man kneeling, holding his daughter by the shoulders; she shows him her belt |
| s07 | Inside the timber church: a priest scratching a ditch and bank on the earth floor; a skipper spreading a cloth; through the door a barge loading a loom |
| s08 | The gatehouse at Noviomagus above the Waal: a young clerk with seal and parchment, an empty court, oak trunks stacked far below |
| s09 | Off the Maas mouth in haze: twenty and more sails standing in; one long ship turned toward us, a man in her bow with a hand raised |
| s10 | Dorestad on a hot day: a chest-high raw bank with a gap for the road, men digging; wine sold cheap by a man looking downstream; bedding in the boats |
| end_wall | Dawn mist: ditch, bank, shut gate, a horn blown from the watch-house; long ships lying on their oars |
| end_tribute | Noon on a dam head: silver on a cloth, scales in a Danish chief's hand, merchants in a silent half circle, captives among the rowers |
| end_flight | The IJssel at Daventre, evening: a small church, a man pacing ground with a pole, looms under a lean-to, two Frisian boats side by side |
| end_ashes | A burned plot between whole roofs: a merchant lifting a warped scale pan from the ash; a widow and a grey skipper waiting at the edge |

## How long it takes

Three years of game time, almost all of it ordinary trading. The first summer is quick: the waits are 1 to 6 days, then 5, then 8, so the warning, the sack and the aftermath fill about three weeks of June 834. The ransom at Sliesthorp can be made any time to the end of 836, and its homecoming is ten days after. The meeting in the church waits for the spring of 835; the palace for any call at Noviomagus after it; the fleet at sea and the third summer for 836; and every ending for June 837, three days after the count dies on Walcheren, or any call up to the end of 838. Between those dates the chain is three or four flags and a watch on the river.
