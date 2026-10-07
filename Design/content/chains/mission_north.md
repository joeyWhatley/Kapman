# Chain: mission_north (carrying the mission north)

Cards: `Assets/Kapman/Game/Data/encounters/chain_mission_north_793.json` (14 cards: 13 harbour, 1 sea; written 6 Oct 2026).
Words: `Assets/Kapman/Game/Data/writing/chain_mission_north.json` (225 entries: every key, an outcome text for every option and every branch of a chance, 5 ending texts, 4 news items, 14 `art.` lines).
A draft for the user to overwrite. (?) marks what could not be checked against a source tonight. No web search was used. One fetch of the Fordham sourcebook's copy of Robinson's translation was tried and came back cut off after chapter 16, so the history is from the game's own tables (`spine.json` thread `cross_and_grove` and cast, `realms/sweden_gotland.md`, `chronicle_793.json`, `npc_roles_793.json`, `unlocks_793.json`) and from Rimbert as remembered. Nothing in the words is quoted from the translation: it is all our own plain rendering.

## The story in five sentences

In the spring of 829 two monks of Corvey, Anskar and Witmar, look for passage to Birka among the merchants, with the emperor's gifts and nearly forty books, and half-way there pirates take the lot.
They come to Birka on foot with nothing, King Bjorn lets them preach all the same, the prefect Herigar is baptised and builds a church on his own land, and somewhere on a Baltic beach a dealer wraps amber in leaves of a psalter.
A generation later the people have driven the priests out, a book hangs on a fence because it killed a household, thirty-two ships ask a hundred pounds of silver for leaving the town standing, and Herigar stays dry in the rain.
About 852 Anskar, now an archbishop past fifty, sails for Birka again with a Danish king's token, and King Olaf tells him that here such things rest with the gods' lot and the whole people, not with the king.
Either the lot falls for him and an old man at the thing says that Christ has helped men at sea, as really happened, or a bell is rung before the thing and the church outlives its founder, or the church becomes a Frisian merchant's own on his own plot, or the Svear send the archbishop home and take their dead King Eric among the gods instead.

## How a player meets it

The chain has two acts a generation apart (829 to 831, and 845 to about 853), so it will usually be begun by one merchant and ended by his heir. Each act can be met on its own; nothing in act two needs act one, though act one makes the hard road easier.

- **829, April to July, at Sliesthorp or Dorestad:** `s01_passage` (weight 3.0, so nearly every call). Dorestad is the home port, so a house that is alive in 829 will see it. Going by costs nothing and it comes again after fifteen days until July.
- **830 or 831 at Birka:** `s05_church` comes to any house, with Herigar's order for Mass wine.
- **845 to 850 at Birka:** `s06_driven_out` comes to any house (weight 6).
- **850 or 851 at Sliesthorp:** `s09_hermit` comes to any house.
- **852 or 853, April to August, at Sliesthorp:** `s10_second` comes to any house (weight 10). This is the door to the finale: steps 11 to 14 need only step 10.
- Hooks already in the tables: the spine's thread `cross_and_grove` (hooks at Sliesthorp and Birka) and `book_road` ("someone has them"); `chronicle_793.json` `829-ansgar-birka`, `845-hamburg-burned`, `850-hedeby-church`; `unlocks_793.json` `unlock_work_birka_church` and `unlock_factor_birka`, which name the same people.
- If the years run out (step 11 closes at the end of 854) the house keeps what the early steps gave; nothing is taken back, and history goes its own way without a flag.

## The real history under it

**The source.** Everything rests on one book: Rimbert, *Vita Anskarii*, written about 870 by Anskar's pupil and successor (tr. C. H. Robinson, *Anskar, the Apostle of the North*, London 1921: https://archive.org/details/anskartheapostle00robiuoft). Chapter numbers below are Rimbert's; the content is given from memory of that translation and from the spine, and should be read against the book before it ships (?).

| Rimbert | What it says | Card |
|---|---|---|
| c. 9 | Envoys of the Svear tell the emperor Louis that many of their people want the Christian worship and their king would allow priests. Anskar is chosen; Witmar, a monk of his house, goes with him. | s01 |
| c. 10 | They travel with merchants. About half-way they fall among pirates. The merchants fight hard and at first win, then are beaten, and lose ships and goods. The monks get ashore on foot. The emperor's gifts and "nearly forty books" are lost. Some want to turn back; Anskar will not. | s01, s02 |
| c. 11 | After a long road on foot, and by ship over the waters between, they reach Birka. King Bjorn (Bern) receives them kindly, takes counsel with his friends and gives leave to preach. Christian captives there are glad. The prefect of the town, Herigar, "a counsellor of the king", is baptised and builds a church on his own inherited land. | s03, s05 |
| c. 12 | They go home after a year and a half with letters written by the king's own hand in the manner of his people (taken to be runes (?)). Hamburg is made an archbishop's see, 831. | s05 |
| c. 14, 17 | Gautbert is made bishop for the Svear and builds a church. About 845 the people rise, "not by command of the king", kill his nephew Nithard, bind the bishop and put him out. No priest for nearly seven years. | s06, s10 |
| c. 18 | A man whose son brought a book home from the plunder loses wife, son, daughter and goods. A soothsayer finds every god content with him but Christ. He ties the book to a fence for anyone to take; a Christian carries it home. | s07 |
| c. 19 | Herigar holds to the faith alone; rain falls on the thing and not on him and his boy. The exiled king Anound comes with 21 Danish ships and 11 of his own; the king is away; the town buys peace for a hundred pounds of silver; Herigar tells them to vow to Christ; the Danes' lots forbid the sack and send them against a Slav town. Anound later returns the silver (?). The hermit Ardgar is sent (about 851); Herigar dies with the sacrament. | s06, s08, s09, s10 |
| c. 20 | Frideburg, rich and old, keeps bought wine by her to be given her at death in place of the sacrament; her daughter Catla carries her alms to Dorstadt. | s06 (Catla's journey is left to other writers: spine hook at Dorestad) |
| c. 24 | Horik the elder lets Anskar build a church at Sliaswic, where merchants of Dorstadt come (about 850). | s09 |
| c. 25-26 | Gautbert will not go back and sends his nephew Erimbert. Horik sends his man and token with Anskar to King Olaf. Nearly twenty days' sail. At Birka a man says he was at the gods' meeting: they complain of thin offerings and offer to take the dead King Eric among them if more gods are wanted. Anskar's friends tell him to buy his life. He feasts the king and gives gifts. Olaf: priests were driven out before by the people, not the king; he will ask the lot, then the thing. | s10, s11 |
| c. 27 | The king meets his chiefs; lots are cast in the open field and fall for the mission. At the thing an old man says many of them know this god helps in peril at sea; some who went to Dorstadt took the faith there of their own will; the way there is now beset by pirates; why refuse at home what they sought far off. The people agree; a second assembly elsewhere agrees. | s12, s13 |
| c. 28 | The king gives a hall for an oratory; Anskar buys a plot for the priest's house; Erimbert stays. | s13 |
| c. 32 | At Sliaswic the younger Horik allows a bell, "the use of which the pagans regarded as unlawful". | s10, s13 (the bell) |

The one sentence taken as Robinson's own words is the king's, in the card note of s11, copied from `spine.json`: "It is our custom that the control of public business of every kind should rest with the whole people and not with the king."

**Dates.** 829 for the first voyage and 831 for Hamburg are from `chronicle_793.json`. The rising is "about 845", Anound's ships between 845 and 851, Ardgar about 851, the second voyage "about 852" (`realms/sweden_gotland.md`). The cards hold each inside a window a year or two wider than that.

**What is invented, and marked so in the card notes.** That a Frisian of Dorestad carried any of them (Rimbert says only "merchants"). The port they sailed from (?): Sliesthorp is likeliest. That the chests rode in the convoy's largest hull. What became of the forty books: the dealer at Ahus or Paviken is invented, on the model of church metalwork cut from books in Norse graves and of the Canterbury Golden Gospels bought back from a heathen army for gold. The English captive who reads is `cast.tidwine` (the spine's invented boy of Lindisfarne). A visiting boat's share of the hundred pounds. The chief of the old party. The man who throws the lots, and that he could be paid. How the lots were thrown: marked slips on a white cloth are Tacitus, *Germania* 10, far older and of other peoples (?). The bell carried to Birka.

**People not in the spine's cast but real and named:** Witmar, King Bjorn, King Olaf, Anound, Ardgar, Erimbert, Gautbert, Nithard, King Horik the elder. The spine spells the prefect Herigar (Rimbert's Herigarius; the task's "Hergeir" is the Norse form of the same name); the cards follow the spine.

## The steps

Flags all begin `ch_mission_north_`; the table leaves the prefix off. "Free" marks the option that costs only time or pride.

| # | Card id | Where and when | Choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_mission_north.s01_passage` | Sliesthorp or Dorestad, Apr-Jul 829 | take the monks aboard (fare 24 d) / sail in the convoy (free) / go by (free; comes again) | `_1`; `_1_aboard`; or `_seen` for 15 days |
| 2 | `sea.chain_mission_north.s02_pirates` | under way, 829-830, after 1 | fight beside the merchants (a chance, doubtful) / take off one chest and run for the beach (a chance, likely) / run (pride) | `_2`; `_books_lost` (and `_chest`, `_2_ran`), or `_books_safe` with `changed_anskar_books_saved` |
| 3 | `harbour.chain_mission_north.s03_bjorn` | Birka, 829-830, after 1 | a cask of wine for the king / speak for them (free) / ask the price of cloth (free) | `_3`; `_3_gift` or `_3_vouched`; `_3_wait` for 200 days |
| 4 | `harbour.chain_mission_north.s04_books` | Ahus or Paviken, 829-836, books lost (a side step) | buy the heap for 90 d to give back / buy it and sell the best book / tell him who pays (free) | `_4`; `_books_bought`, `_books_sold` or `_4_told` |
| 5 | `harbour.chain_mission_north.s05_church` | Birka, 830-831, any house | the wine order only (free) / hand over the books / carry Anskar south / buy the English scribe free, 96 d | `_5`; `_5_books`, `_5_passage` or `_5_scribe` |
| 6 | `harbour.chain_mission_north.s06_driven_out` | Birka, 845-850, any house | sell Frideburg her last wine / carry Herigar's word (free) / stand off (pride) | `_6`; `_6_wine` or `_6_word`; `_6_wait` for 60 days |
| 7 | `harbour.chain_mission_north.s07_fence` | Birka, 845-851, 60 days after 6 | carry the book to the Christians / keep it to sell / leave it (free) | `_7`; `_7_book` or `_7_sold`; `_7_wait` for 90 days |
| 8 | `harbour.chain_mission_north.s08_ransom` | Birka, 846-850, 90 days after 7 | pay a share, 60 d / stand by Herigar (free) / slip out by night (a chance) | `_8`; `_8_paid`, `_8_stood` or `_8_slipped` |
| 9 | `harbour.chain_mission_north.s09_hermit` | Sliesthorp, 850-851, any house | carry Ardgar free / for the fare, 40 d / no room (free) | `_9`; `_9_carried` and the spine's `ms_carried_priest` |
| 10 | `harbour.chain_mission_north.s10_second` | Sliesthorp, Apr-Aug 852-853, any house | take them (fare 60 d) / take them and buy a bell, 120 d / carry their stores / not yet (free) | `_10`; `_10_aboard`; `_bell`; `_10_wait` for 15 days |
| 11 | `harbour.chain_mission_north.s11_feast` | Birka, 852-854, 15 days after 10 | speak for the mission / drink with the old party / ask about a stranger's own plot (all free) | `_11`; `_11_mission`, `_11_old` or `_11_house`; `_11_wait` for 3 days |
| 12 | `harbour.chain_mission_north.s12_lots` | Birka, 3 days after 11 | watch (free; the lot is likely) / vow alms, 30 d / pay the thrower 80 d for it / pay him 80 d against it (old party only) | `_12`; `_lot_for` or `_lot_against`; `_lot_bought`; `_12_wait` for 2 days |
| 13 | `harbour.chain_mission_north.s13_thing` | Birka, 2 days after 12, lot for | bear the old man out (free) / the book and the bell (hard road) / the church on your own plot / speak against (old party only) | an ending flag; `_done` |
| 14 | `harbour.chain_mission_north.s14_against` | Birka, 2 days after 12, lot against | carry the archbishop home (free) / ask a second lot on a smaller question (a chance) / bring the old man before the chiefs (a chance) / the old party's thanks | an ending flag; `_done` |

The hard road's check at step 13: the bell from step 10, **and** a book brought back (the heap bought at step 4, or the fence book carried at step 7, or the fight won at step 2), **and** a priest carried at step 9 or Herigar stood by at step 8.

No dead ends: every card has a free option; a lost fight, a lost chest, a failed night passage and a lot that falls against all move the story on; a failed chance at step 14 pays the loyal ending in full.

## The endings

| Ending | Flag | How | What becomes of it | History |
|---|---|---|---|---|
| **The priests may stay** | `ch_mission_north_end_church` | Lot for, and you bear the old man out; or lot against, and the old man is heard anyway | The king gives a hall for the church, Anskar buys a plot for the priest, Erimbert stays, Anskar goes home. After 865 no priest comes for seventy years. | What really happened (Rimbert c. 27-28). |
| **The bell** | `ch_mission_north_end_endures` | The hard road: lot for, with the bell, a book brought back and a priest carried or Herigar stood by | A bell is rung before the thing and the king has it hung. The traders come to tell the hours by it, and the church does not shut when its founder dies. | **Changed**, `changed_birka_church_endures`: the mission to the Svear does not lapse in 865. When Archbishop Unni comes to Birka in 936 (he really died there) he finds a living church and not a memory. Later cards may give Birka the faith `mixed`, a priest who can write a letter in any year to 975, and Christian Svear at Dorestad's successors a century early. |
| **The merchants' church** | `ch_mission_north_end_merchants` | You asked the king about a stranger's own plot (step 11) and ask the thing for it; or lot against and a second lot on the smaller question | No bishop's church. A Frisian house keeps a priest on its own plot for any trader. It outlives Anskar because it belongs to a house and not to a see. | **Changed**, `changed_birka_merchants_church`: Birka's Christianity is a traders' affair under a lay lord, as the churches of merchant companies in northern ports were centuries later. Cost that lasts: `ch_mission_north_bishop_watches` (the archbishop and his successors want that priest under their hand: a later card may send a visitor, or a demand for the church's tenth). |
| **King Eric among the gods** | `ch_mission_north_end_refused` | Lot against and you carry Anskar home (the loyal way); or you drank with the old party and speak against, or take their thanks | Anskar goes home. The Svear take the dead King Eric among their gods, as the gods' messenger offered. No priest at Birka again in that age. | **Changed**, `changed_birka_refused_mission`: the second mission fails as the first did; Hamburg-Bremen gives up the Svear a decade early, and the old man's argument is never made. Later cards may close `unlock_work_birka_church` and keep Birka `old_gods` to its end. If the house worked for it, the cost that lasts is `ch_mission_north_church_remembers` (bishops on the Rhine and at Bremen know the name: a later card may refuse a charter, or a monastery its custom). |

An early change besides: `changed_anskar_books_saved` (the fight at step 2 won). The mission reaches Birka with the emperor's gifts and its forty books; step 3 has the king liking the belt. It does not alter the endings, but it counts as the book on the hard road, and a later card may put a Frankish book in a Svea's hands in the 830s.

## The payouts

A cask of Rhenish wine is 60 d, a book 360 d, a timber of marten 120 d, a timber of sable 480 d (`goods_793.json`).

| Tier | Card, option | Pays | Why a merchant could have won it |
|---|---|---|---|
| Step | s01, take | 24 d fare | monks paid passage |
| Step | s02, stand (won) | deed `fought_raiders`, the crew's heart | |
| Step | s03, gift / trade | half toll at Birka for 60 days / cloth x1.15 at Birka for 30 days | the prefect's favour; a price word |
| Turn | s04, sell | one sound book (360 d) for 90 d | plunder sold by men who wanted only the fittings |
| Turn | s05, books | 200 d and a start in Latin | the monks buy their own books back |
| Turn | s05, scribe | a hired man who reads Latin and tallies (96 d) | a Christian captive bought free: alms, and a clerk |
| Step | s05, any | an order for 4 casks of Mass wine at premium 1.5 | a new church has no vineyard |
| Step | s05, passage | 30 d and a start in runes | he shows the king's letter |
| Turn | s06, wine | 110 d for one cask | Frideburg does not bargain |
| Step | s06, word | deed `carried_letters` | |
| Step | s07, carry / keep | 20 d and Latin / a psalter (360 d), and an uneasy crew | he pays to be rid of it |
| Step | s08, pay / stand | half toll at Birka for 90 days / Herigar a fast friend | |
| Step | s09 | 40 d fare, or the deed `gave_alms` | |
| Step | s10, take | 60 d fare | |
| Step | s11, old | marten x0.85 at Birka for 60 days | furs at a friend's price |
| **Finale** | s13 witness; s14 elder (won) | **700 d**, `unlock_factor_birka`, an order for 6 casks at premium 1.5 | Anskar bought a plot for the priest's house (c. 28); a Christian household at Birka is where a Dorestad house leaves its goods (`unlocks_793.json`) |
| **Hard road** | s13 bell | **1,400 d**, `unlock_work_birka_church` and `unlock_factor_birka`, an order for 8 casks | the house is counted the church's founder with the king: the work that otherwise costs 1,800 d is granted |
| **Finale** | s13 house; s14 again (won) | **5 timbers of marten and 200 d (800 d)**, `unlock_plot_birka`, `unlock_title_church_lord_birka`; cost `ch_mission_north_bishop_watches` | a church on a man's own land, with its priest his man, is how Herigar's stood and how most churches of the age were held; the traders pay in for their share |
| **Finale** | s13 against; s14 thanks | **2 timbers of sable and 300 d (1,260 d)**, `unlock_plot_birka`; cost `ch_mission_north_church_remembers` | the old party's thanks: a plot by the jetties and the fur they do not sell to strangers |
| **Finale** | s14 home; s14 again or elder (lost) | **600 d**, `unlock_title_lords_protection` | the silver meant for a priest's house, and a free man taken under an archbishop's hand (`unlocks_793.json`) |

New flag for the spine to wire: `unlock_title_church_lord_birka` (the lord of a church of his own at Birka: he names its priest, Christians of the town count him a patron, and it is inherited with the plot). Every other reward flag is already in `unlocks_793.json`.

## How long it takes

- Act one: spring 829 (step 1), the passage (step 2), Birka that summer or the next (step 3), then 200 days, so step 5 falls in 830 or 831. Step 4 is met in passing on the Baltic any time to 836. Two to three sailing seasons.
- Fourteen years of ordinary trading, and very likely a death and an heir.
- Act two: steps 6, 7 and 8 need three calls at Birka between 845 and 850, with 60 and 90 days between; step 9 at Sliesthorp in 850 or 851; step 10 in the summer of 852 or 853; then fifteen days' passage, three days to the lot and two to the thing. Seven or eight years, the last of them one crowded summer.
- From the last door alone (step 10): one summer, about a month.

## Pictures

One for each step (the `art.` entries in the writing file hold the full line for each):

1. `s01_passage`: a jetty in spring; a barefoot young monk talking to a shipmaster, another sitting on the book chests, counting.
2. `s02_pirates`: a huddle of merchant ships in haze; long low ships rowing at them with masts down.
3. `s03_bjorn`: a king's hall by firelight; two muddy monks, the prefect with his keys, a bench of merchants.
4. `s04_books`: a beach market; amber laid out on a written leaf, a heap of books without boards under a hide.
5. `s05_church`: a small new timber church with a cross over the door; Herigar counting, a monk packing, an old thrall holding a stick like a pen.
6. `s06_driven_out`: the same church with its door nailed; a grey man with a cross, dry in the rain; an old woman at a house door.
7. `s07_fence`: a book hanging by a cord from a wattle fence on a wet road; people on the far side; a man watching from a cold house.
8. `s08_ransom`: thirty ships in the sound at evening; merchants weighing silver on the jetty; the fort on its rock.
9. `s09_hermit`: the harbour church at Sliesthorp; a bishop in a plain cloak and a gaunt hermit who dreads the boats.
10. `s10_second`: the old archbishop helped aboard a small boat; a pale priest, a king's man, a bell in sheepskin.
11. `s11_feast`: the archbishop serving the king at table; chiefs muttering down the benches; a horn held out to a Frisian.
12. `s12_lots`: an open field; a ring of chiefs round a white cloth; an old man with slips of wood in his cupped hands.
13. `s13_thing`: a great crowd on a slope, gone quiet; one very old man on his feet with his arm raised.
14. `s14_against`: the archbishop on a sea chest on the jetty, a gold ring in his open hand; an old townsman shaking his stick.

One for each ending:

- **The priests may stay:** a king's hall with its high seat carried out and a plain table carried in; a young priest hanging a cross on the gable post; the archbishop already walking down to the boat.
- **The bell:** a small bronze bell on top of a battered book on the trampled grass of the thing, a Frisian's hand just leaving it, the whole crowd looking at it and nobody moving.
- **The merchants' church:** a fenced plot by the stone jetties, a storehouse at one end and a tiny timber church at the other, fur traders ducking in at its door with their caps in their hands.
- **King Eric among the gods:** a new mound outside the town with a post set on it and offerings laid at its foot; far out on the lake, one small sail going south.
