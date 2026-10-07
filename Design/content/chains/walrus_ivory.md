# Chain: walrus_ivory (teeth and rope from the far north)

Cards: `Assets/Kapman/Game/Data/encounters/chain_walrus_ivory_793.json` (14 cards: 12 harbour, 2 sea; written 6 Oct 2026).
Words: `Assets/Kapman/Game/Data/writing/chain_walrus_ivory.json` (213 entries: every key, an outcome text for every option, 6 news items, 14 `art.` lines).
A draft for the user to overwrite. (?) marks what could not be checked against a source tonight. No web search was used: the history is from the game's own tables (`texts_793.json` ohthere and wulfstan, `chronicle_793.json`, `spine.json`, `realms/norway.md`, `peoples/norse.md`, `ships.md`) and from the Old English text as remembered.

## The story in five sentences

In a summer between 886 and 896 a rich Northman from north of everybody, Ohthere, lays walrus teeth and hide rope on the jetty at Skíringssalr and asks a Frisian merchant to do one thing: stand in a southern market with a tooth in his hand and come back and say what he saw.
A comb-maker at Sliesthorp cuts the tooth open and shows why the thick ones are dear, and the reeve in King Alfred's new London has never seen such ivory and knows a king who would rather hear a hunter than a flatterer.
The next summer the king's steward at the farm on the Karmsund has carried Ohthere's sail ashore and says the tribute of the Finnas is the king's to share out, and the merchant pays, pleads, waits, or tells the steward what the teeth are worth.
Five days on, at the Heaths, with Wulfstan drinking beside them and a Frisian's silver already on the scale, the cargo goes to England in company or up the Rhine without its owner.
Either Ohthere sits before Alfred and says "I do not know" until the clerk has three leaves of it, as really happened, or the merchant tells the voyage as his own and the king leaves it out of his book, or the teeth cover altars at Cologne and the north stays "waste, and cold", or the tribute becomes King Harald's trade a generation early and the stewards tell no stories.

## How a player meets it

- It starts by itself at **Skíringssalr** (`skiringssal`): card `s01_tusks` is drawn on a call there in July, August or September of any year from 886 to 896 (weight 2.0, so nearly every call). Passing by costs nothing; it comes again after twenty days and the next summer.
- Hooks elsewhere (already in the tables, no new card needed): the price of `walrus_ivory` in `goods_793.json` (90 d a pair, wanted at the highest mark in england_south, rhineland and neustria); `realms/norway.md` scene 4 "Ohthere's cargo"; the spine's cast entry `cast.ohthere` (home Skíringssalr; also met at Sliesthorp, Ögvaldsnes, Lundenwic, which are exactly this chain's ports).
- It does not come to a house that has ended it (`ch_walrus_ivory_done`).
- If the years run out (Alfred dies on 26 October 899; the spine has Ohthere to 900) the England road closes and card `s12_altar` at Colonia takes a merchant who was on it. A merchant who never reaches the Karmsund before 900 simply keeps what the early steps gave him; nothing is taken back.

## The real history under it

**The source.** Everything rests on one text: the report of Ohthere (Old Norse Óttarr) that was written into the Old English translation of Orosius's history at or near King Alfred's court in the 890s, beside the report of Wulfstan. `texts_793.json` holds it (`ohthere`: the Old English from Sweet 1883, EETS o.s. 79, pp. 17-19; the translation of Joseph Bosworth, 1859, who died in 1876). `chronicle_793.json` has the entry `890-ohthere-wulfstan`. The date is only "in Alfred's reign", 871-899; "about 890" is the usual guess. Where the two men met is not recorded (?); the game puts it in London, which Alfred held from 886 (`886-london-alfred`).

What the report says, and which card uses it (our own plain rendering; the part of the text about teeth, dues and deer is not in the excerpt held in `texts_793.json` and is given here from the Old English as remembered (?): check against Sweet p. 18 before it ships):

| The report | Card |
|---|---|
| He "said to his lord, King Alfred, that he dwelt northmost of all Northmen" | s01, s10, s14 |
| He went north chiefly for the horse-whales (walruses), "because they have very noble bone in their teeth; some of those teeth they brought to the king; and their hide is very good for ship's ropes" | s01, s10 |
| The horse-whale is much smaller than other whales, no longer than seven ells | s01 |
| He and five others killed sixty in two days (whales, or walruses: scholars disagree) | s10 (the king's doubt is invented) |
| He had six hundred tame deer unsold when he came to the king, six of them decoy deer; but only twenty cattle, twenty sheep, twenty swine, and he ploughed the little he ploughed with horses | s02 |
| Their wealth is mostly the tribute of the Finnas: skins, feathers, whale's bone, and ship's ropes of whale and seal hide. Each pays by his birth: the best-born fifteen marten skins, five reindeer, one bear skin, ten ambers of feathers, a kirtle of bear or otter skin, and two ship's ropes, each sixty ells long | s02 |
| Sciringes heal is a month's sail south, camping at night, with a fair wind each day | s01 |
| From Sciringes heal five days to the port "æt Hæþum", which stands between Wends, Saxons and Angles and belongs to the Danes: three days with Denmark to port and open sea to starboard, two with the Danish islands to port | s07, s08 |
| North: three days as far as the whale hunters go, three more, then east four days, then south five to a great river | s10, s11 |
| "The Beormas told him many tales... but he knew not what was true, because he did not see it himself" | s10, s11 (the clerk's test) |
| Wulfstan: from the Heaths to Truso in seven days and nights, the ship under sail the whole way, Wendland to starboard | s08, s14 |

**Why it matters, and why an ending can change it.** Ohthere's report is the oldest first-hand account of the far north of Europe, and it has the first written form of the name Norway (*Norðweg*) (`peoples/norse.md`). It exists because one careful man reached one curious king. A game in which the merchant can stand between them can lose it.

**The king.** Asser's Life of Alfred (893) says Franks, Frisians, Gauls, heathens, Britons, Scots and Bretons put themselves under Alfred's lordship, and that he questioned men of every kind (c. 76; the chapter number from memory (?)). The Chronicle has his ships at sea in 875, 882 and 885 and his long ships in 896 (`896-alfred-ships`). His words on the cards are invented.

**London.** Taken in 886 and given to Ealdorman Æthelred; plots at Æthelred's hythe (Queenhithe) granted in 889 and 898 (`CHAINS.md`, thread_07). A king's remission of toll on a ship at London is known from charters of Æthelbald of Mercia in the 730s and 740s (?), which is the model for the reward `unlock_charter_lundenwic`.

**The first signing** (card s09). Heathens who dealt with Christians let themselves be marked with the cross without baptism: Rimbert, Vita Anskarii c. 24 (many at Sliesthorp took the sign of the cross and became catechumens; Robinson 1921; our rendering); Egils saga c. 50 calls it the common custom of merchants (13th century, LATER). A known man standing surety for a stranger follows the English laws on hosts and traders (Hlothhere and Eadric 15; Alfred 34; from memory (?)).

**The tooth** (cards s03, s12). A walrus tusk has a dense white outer layer and a clouded core, so carvers keep to the outer layer and prize thick tusks. Elephant ivory was scarce north of the Alps from the 7th century; many Carolingian ivories are cut on the planed backs of late Roman panels. English and Rhenish carving in walrus ivory is plentiful from the later 10th century (the Winchester-style panels; the Cologne workshops of the 11th and 12th centuries). All of this is handbook knowledge with no page read tonight (?). Walrus ivory worked at Hedeby is reported from the digs (?).

**Cologne** (card s12). Northmen burned the city in the winter of 881-882 and it was rebuilt (Annales Fuldenses 881-883 (?)). The cathedral is Saint Peter's. The archbishops (Willibert to 889, Hermann I after) are not named in the words (?).

**The Karmsund** (cards s06, s13). Ögvaldsnes is a king's farm on the sheltered coast road through the whole game, with ship burials of 779 and about 790-795 on the ridge (`realms/norway.md`). Ohthere's own report names **no king** in Norway and treats the tribute as his people's. That King Harald (Fairhair) held the west coast by the 880s or 890s, had Ögvaldsnes among his farms, and claimed the Finn tribute for the man he named is the tradition of the 13th-century sagas (Snorri; Egils saga c. 8-17), LATER and not sure (?). The quarrel on the card is therefore invented, built from that later tradition, and a steward carrying a ship's sail ashore is borrowed from the Asbjorn story at this same farm about 1023 (`realms/norway.md` scene 7, saga).

**Robbers in Danish waters** (card s07). Anskar's ship was taken by pirates on the way to Birka in 829, and the merchants with him fought and lost nearly everything (Rimbert c. 10).

**Hide rope** (card s05). Ohthere's word for it, and `Data/story/unlocks_793.json` `unlock_upgrade_walrus_rope`. Everyday rigging of bast and horsehair: `ships.md` 7.

## The steps

Flags all begin `ch_walrus_ivory_`; the table leaves the prefix off. "Free" marks the option that costs only time or pride.

| # | Card id | Where and when | Choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_walrus_ivory.s01_tusks` | Skíringssalr, Jul-Sep, 886-896 | ask what he wants (free) / tell him the south's prices / go by (free; comes again) | `_1`; `_candid`; or `_seen` |
| 2 | `harbour.chain_walrus_ivory.s02_tribute` | Skíringssalr, at once after 1 | take a pair on trust (free) / pay 70 d for it / take only the rope (free) | `_2`; `_trust` |
| 3 | `harbour.chain_walrus_ivory.s03_carver` | Sliesthorp, any call after 2 (a side step) | sell the pair for 110 d / pay 8 d for a carved gaming king / keep it whole (free) | `_3`; `_3_sold` or `_piece` |
| 4 | `harbour.chain_walrus_ivory.s04_reeve` | Lundenwic, 886-899, any call after 2 | ask the king's peace for the Northman / ask to be the bringer yourself / ask only prices (all free) | `_4`; `_4_peace` or `_4_own`; `_wait` for 150 days |
| 5 | `sea.chain_walrus_ivory.s05_rope` | under way, hide rope aboard (a side step) | reeve the hide rope (uses a coil) / knot the bast (a chance) / ride it out (free) | `_rope` |
| 6 | `harbour.chain_walrus_ivory.s06_sound` | Ögvaldsnes, Jun-Aug, before 900, after the 150 days | pay his due, 80 d / plead for him (a chance; a lost plea costs 40 d and moves on) / tell the steward the worth / wait (free) | `_6`; `_6_surety`, `_6_law`, `_6_waited`, or `_6_king` with `_king_wait` for 300 days |
| 7 | `sea.chain_walrus_ivory.s07_kattegat` | under way after 6, not on the king's road (may not come) | close up and show iron (a chance) / run inside the islands (a chance) / pay 60 d | `_7`; `_7_stood` |
| 8 | `harbour.chain_walrus_ivory.s08_heaths` | Sliesthorp, before 900, after 6 | lay 300 d in with him for England / pilot him for a tenth (free) / buy the teeth for the Rhine, 500 d / carry them to the Rhine on shares (free) | `_8`; `_8_england` (and `_felag`) or `_8_franks` (and `_franks_bought`) |
| 9 | `harbour.chain_walrus_ivory.s09_hythe` | Lundenwic, before 900, England road | swear for him (needs standing "known" or `_4_peace`) / ask him to take the first signing (free) / go in alone (free) | `_9`; `_9_vouched`, `_9_signed` or `_9_alone`; `_hall_wait` for 3 days |
| 10 | `harbour.chain_walrus_ivory.s10_hall` | Lundenwic, 3 days later, Ohthere let in | your share (if `_felag`) / your tenth / the king's peace for both houses | **`_end_told`**, `_done` |
| 11 | `harbour.chain_walrus_ivory.s11_whose` | Lundenwic, 3 days later, gone in alone | name Ohthere (free) / say you were in the ship (a chance) / ask to fetch him (back to 10) | **`_end_told`** or **`_end_own`** with `changed_ohthere_unwritten`, `_ohthere_wronged`; `_done` |
| 12 | `harbour.chain_walrus_ivory.s12_altar` | Colonia, Rhine road (or England road after 899) | ten pairs a year / sell your own teeth (if `_franks_bought`) / give the best pair to the altar | **`_end_franks`**, `changed_ivory_road_rhine`, `changed_ohthere_unwritten`, `_done` |
| 13 | `harbour.chain_walrus_ivory.s13_kings_share` | Ögvaldsnes, 300 days after the king's road was taken | carry the king's teeth / take the ship / ask that Ohthere keep his own share (pride) | **`_end_king`**, `changed_finn_tribute_kings`, `changed_ohthere_unwritten`, `_done`; `_ohthere_wronged` or `_mended` |
| 14 | `harbour.chain_walrus_ivory.s14_book` | Lundenwic or Hamwic, from 890, 700 days after any ending | buy Wulfstan's leaves, 30 d / a leaf with your name at the foot, 12 d (true ending only) / go (free) | `_book` |

## The endings

| Ending | Flag | How | What becomes of it | History |
|---|---|---|---|---|
| **He said to his lord** | `ch_walrus_ivory_end_told` | Ohthere is let in (your oath or the first signing) and tells it himself; or you go in alone and name him; or you claim it and are caught | The king buys the teeth and the rope, the clerk writes three leaves, and London's goldsmiths want walrus ivory (price up for a year). The book names no ship and no go-between. | What really happened. |
| **Whose story** | `ch_walrus_ivory_end_own` | You go in alone, say you were in the ship, and are believed | You are the king's man for the teeth. The king has the tale left out of his book: "the man knew the price of everything he had seen." Ohthere sails home wronged. | **Changed**, `changed_ohthere_unwritten`: the Old English Orosius has Wulfstan and no Ohthere. Later: no first-hand account of the far north, no early written *Norðweg*; a later card may have an English priest say the north is "waste, and cold", and any card that gives the text `ohthere` should ask `not flag.changed_ohthere_unwritten`. The house carries `ch_walrus_ivory_ohthere_wronged`: a later card in Norway may make a Halogalander refuse to deal. |
| **Tooth for the altar** | `ch_walrus_ivory_end_franks` | The teeth go up the Rhine from the Heaths (or the king died first) | The archbishop's carver at Cologne stops planing old Roman tablets. Ohthere sells at the Heaths, goes home to his deer and never sees England. | **Changed**, `changed_ivory_road_rhine` and `changed_ohthere_unwritten`: the walrus-ivory road runs Hedeby to Cologne two or three generations before the Rhenish workshops really took it up, and England's carvers stay with whale's bone for longer. Later cards may raise ivory's price at Colonia and lower it at Lundenwic. |
| **The king's share** | `ch_walrus_ivory_end_king` | At the Karmsund you tell the steward what the teeth are worth | The Finn tribute comes down the coast in King Harald's name; his steward sells it; Ohthere keeps his deer and loses his trade (unless you ask that he keep his own share: `_mended`). | **Changed**, `changed_finn_tribute_kings` and `changed_ohthere_unwritten`: the royal hold on the northern trade that the sagas and the later laws describe comes a generation or more early, because a southerner priced it. Later cards in Norway may sell walrus ivory only through a king's man. |

## The payouts

Silver is what the option pays net of nothing; costs are in the steps table. A pair of teeth is 90 d in `goods_793.json`, a coil of hide rope 30 d.

| Tier | Card, option | Pays | Why a merchant could have won it |
|---|---|---|---|
| Step | s02, any | a coil of hide rope (30 d); a pair of teeth on trust or for 70 d | a gift to a guest who will carry word |
| Turn | s03, sell | 110 d | a rare good sold to the one craft that wants it |
| Step | s04, own / prices | an order for 4 pairs at premium 1.4 / ivory x1.2 at Lundenwic for 200 days | a reeve's order; a price word |
| Step | s05, reeve | morale; the flag that makes the king ask for rope first | the rope proved in use |
| Turn | s06, surety / law won | 2 pairs (180 d) / 1 pair, the deed `won_case`, a fast friend | he repays in kind |
| Step | s06, king | 100 d and the steward's friendship | a steward pays for counsel |
| Step | s07, stand | the deed `fought_raiders` | |
| Turn | s08, fellow / pilot | Old Norse half learned (`tongue old_norse = 0.5`) | five more days in company |
| **Finale** | s10, tenth | **800 d**, `unlock_charter_lundenwic`, `unlock_upgrade_walrus_rope`, the text `ohthere` | a pilot's tenth; toll freedom for one ship at London is the old Mercian kings' kind of grant |
| **Hard road** | s10, share (300 d laid in at s08) | **1,800 d**, `unlock_charter_lundenwic`, `unlock_partner_ottar`, the rope, the text | laying goods together and sharing what comes back is the Norse fellowship; the partner will sail a second hull |
| **Finale** | s10, peace | **300 d and 6 pairs (840 d in all)**, `unlock_title_alfreds_guest`, the rope, the text | Asser's strangers under the king's lordship |
| **Finale** | s11, name him | **700 d**, `unlock_charter_lundenwic`, the rope, the text | the carrier is paid and thanked |
| **Finale** | s11, mine, believed | **1,500 d**, `unlock_title_kings_ivory_man`, the rope; cost that lasts: `ch_walrus_ivory_ohthere_wronged` | a king's named supplier |
| Finale (failed) | s11, mine, caught | 600 d, `unlock_charter_lundenwic`, the text; the king thinks worse of you | paid for the carrying, no more |
| **Finale** | s12, yearly | **1,000 d**, `unlock_factor_colonia`, an order for 10 pairs at premium 1.5 | a great church keeps a man to hold a regular supplier's goods |
| **Hard road** | s12, high (500 d risked at s08) | **2,000 d**, `unlock_factor_colonia`, `unlock_plot_sliesthorp` | the merchant owned the cargo and sold where nobody else had any; the Frisians at Hedeby held plots |
| **Finale** | s12, altar | **600 d**, `unlock_title_st_peters_man`, the deed `gave_alms` | a gift to the saint buys the church's protection on the river |
| **Finale** | s13, king's man | **900 d**, `unlock_title_haralds_guest`, an order for 8 pairs at premium 1.4; cost: `_ohthere_wronged` | a king's guest and carrier |
| **Finale** | s13, ship | **600 d**, `unlock_ship_norse_allround_ship`; cost: `_ohthere_wronged` | an old ship of the king's, given for service |
| **Finale** | s13, mend | **600 d**, `unlock_pilot_karmsund`, `ch_walrus_ivory_mended` | the steward shows the sound; Ohthere keeps his own share |
| Epilogue | s14 | the text `wulfstan` for 30 d; `unlock_heirloom_kings_book_leaf` for 12 d (true ending only) | a leaf from a king's copyist, kept by the heirs |

## How long it takes

- Summer of year one: steps 1 and 2 at Skíringssalr. Autumn to spring: Sliesthorp (step 3) and Lundenwic (step 4) in the course of ordinary trading.
- 150 days must pass after step 4; step 6 comes at the Karmsund in June to August, so the **next summer** at the soonest.
- Then five days to the Heaths (step 8), the passage to England or the Rhine, three days' wait for the king: the true ending is reached about **thirteen to fifteen months** after the start.
- The king's road waits 300 days more: about **two years**.
- The epilogue comes 700 days after any ending: **three to four years** in all.

## Pictures

One for each step (the `art.<card id>` entries in the writing file say the same in a line each):

1. `s01_tusks`: Ohthere on the jetty at Skíringssalr over a row of tusks and coils of rope, grey late-summer afternoon.
2. `s02_tribute`: the tribute laid out by kind in a boat shed; the two men pulling on a hide rope.
3. `s03_carver`: the comb-maker holding the tusk's broken end to the morning light.
4. `s04_reeve`: one tusk on a table under London's Roman wall, reeve and clerk bent over it.
5. `s05_rope`: the yard hanging from a stranded halyard in a squall, a hand on the dark coil.
6. `s06_sound`: the sail being carried up the beach at the king's farm, two ship mounds on the ridge.
7. `s07_kattegat`: a long ship under oars coming out from behind a low island at dawn.
8. `s08_heaths`: Ohthere and Wulfstan on chests with ale horns at the Schlei jetties, the Frisian waiting with his scale.
9. `s09_hythe`: the priest's raised cross, the reeve, the merchant's lifted hand, Ohthere sitting in his ship offshore.
10. `s10_hall`: the king leaning over the teeth by firelight, the clerk's pen stopped.
11. `s11_whose`: the merchant alone before the king; through the door, far off, one man in a ship.
12. `s12_altar`: the old carver with a Roman tablet in one hand, staring at the tusk.
13. `s13_kings_share`: tusks stacked like firewood in the king's shed, silver counted on a barrel head.
14. `s14_book`: the priest's pen on a line of the open quire, a blank ruled leaf beside it.

One for each ending:

- **He said to his lord**: a leaf of the king's book in close view, the pen just lifted from the name Ohthere, a walrus tusk lying across the corner of the desk.
- **Whose story**: the merchant walking down to the hythe with a heavy purse, and beyond him a northern ship hoisting her sail to leave.
- **Tooth for the altar**: a finished book board of white walrus ivory, a saint in low relief, propped on an altar cloth in the half-rebuilt cathedral; the planed Roman tablet set aside on a shelf.
- **The king's share**: the Karmsund from the ridge by the mounds: a ship going south with the king's mark on her sail, and no ship from the north on the water.

## Needs

- The engine does not show outcome texts yet (`chain_walrus_ivory.<card>.out_<option>`); each option's `note` names its text.
- Grand-reward flags this chain sets that are not yet in `Data/story/unlocks_793.json`: `unlock_partner_ottar`, `unlock_title_alfreds_guest`, `unlock_title_kings_ivory_man`, `unlock_title_st_peters_man`, `unlock_title_haralds_guest`, `unlock_pilot_karmsund`, `unlock_factor_colonia`, `unlock_plot_sliesthorp`, `unlock_charter_lundenwic`, `unlock_heirloom_kings_book_leaf` (the spine owns that table). `unlock_upgrade_walrus_rope` and `unlock_ship_norse_allround_ship` are there already.
- `changed_ohthere_unwritten`: the text `ohthere` can be bought from 890 at Hamwic, Lundenwic and Eoforwic (`texts_793.json`, `acquire`); that sale should ask `not flag.changed_ohthere_unwritten`.
- A flag is cleared by setting it to nothing (`flag ch_walrus_ivory_9_alone = 0`, card s11): this follows the grammar of `Effects.cs`; that a flag with the value 0 reads as false is taken from `Cond.cs` (a bare variable is true when not zero) and was not run.
- Ohthere is a `merchant` by role at three ports; the forge will make three people of him unless the role table learns a travelling cast member. The cards therefore keep his good will in flags, not in `npc.<role>.feeling`.
- King Alfred is not in the spine's cast; cards s10 and s11 use the role `king` with a note.
- To check before it ships: the Old English wording for teeth, dues, deer and the sixty (Sweet 1883 p. 18); Asser's chapter numbers; the laws cited for surety; the dates of the Cologne archbishops; walrus ivory among the Hedeby finds.
