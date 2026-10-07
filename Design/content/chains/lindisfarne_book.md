# Chain: lindisfarne_book (the book from the island)

Cards: `Assets/Kapman/Game/Data/encounters/chain_lindisfarne_book_793.json` (13 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_lindisfarne_book.json` (201 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
Part of the spine's threads `holy_island` (no. 2) and `book_road` (no. 10). It tells the book; the boys are left to the thread's own cards.

## The story in five sentences

On 8 June 793 heathen men emptied Saint Cuthbert's church, and among what went into their ships was the gospel book of the daily altar; its gold was broken off for brooches and the leaves were kept because the pictures were fine.
The player meets it from either end: a strip of gilt on a dealer's cloth in a northern market, or Bishop Higbald counting on his fingers what is gone and telling the one mark by which the book is known, a hole in the first leaf of John that the scribe wrote round.
The trail goes west to the hall of Thorleif on the Karmsund, who will sell for two swords or an axe head's weight of silver and never for prayers; the player can pay, tell him honestly what it is and leave it, or come back for it after dark.
A book bought or stolen has to cross the sea dry in a seven-metre boat, a stolen one with a striped sail astern; a book left behind goes south to the dealer Asulf at Sliesthorp, where a Svea is bidding for the skins and there is one more chance.
It ends on the island's altar, in the archbishop's minster at York, in the hands of Alcuin's monk as King Charles's gift, or at Birka in a bale of furs, which is the ending the sources allow.

## How a player meets it

- **In the north:** `s01_strip_of_gilt` is drawn on a call at Skiringssal, Ripa or Sliesthorp from August 793 to 803 (weight 2, once).
- **On the island:** `s02_what_is_gone` is drawn on a call at Lindisfarena while Higbald lives, to May 803 (weight 6, once). It does not need step 1.
- Either one opens `s05_box_of_skins` at Ogvaldsnes or Skiringssal. The news of the raid is thirteen days old at Dorestad on the game's first day and the spine has a priest there with a letter from York: a place writer may point him at the island, and may add `card:harbour.chain_lindisfarne_book.s02_what_is_gone` to the church site at Lindisfarena and `card:harbour.chain_lindisfarne_book.s01_strip_of_gilt` to a market site in the three northern ports.

## The real history under it

**The raid.** Anglo-Saxon Chronicle 793 (Giles 1847; `texts_793.json` chronicle_793): heathen men made havoc in the church of God on the island "by rapine and slaughter"; the day is taken as 8 June. Symeon of Durham, Historia Dunelmensis ecclesiae xx (about 1104-07, three centuries LATER; tr. Stevenson 1855): some brethren killed, some carried off in chains, some drowned; the see stayed. What was taken is not listed by any source, and who the raiders were is not known (`dossiers/lindisfarena.md` section 8).

**Alcuin's promise.** Alcuin to Bishop Higbald, 793 (MGH Epistolae IV no. 20): when King Charles comes home he will go to him and do what he can about the youths led away captive. Not quoted (the English of it is not free). Whether the king gave anything, and whether anyone came home, is not recorded: the bag of alms at Quentovic is the spine's guess (?).

**Books bought back from the heathen.** The Stockholm Codex Aureus, a Kentish gospel book of the eighth century, carries an Old English note of the mid ninth: the ealdorman Aelfred and Werburg his wife bought it from the heathen army with pure gold, for the love of God and their souls' good, because they would not have the holy book stay longer among heathens, and gave it to Christ Church (our rendering, from memory; the spine lists the book among its sources) (?). That is the whole warrant for this chain: a layman found a looted gospel book in heathen hands, paid for it and gave it to a church. It is fifty years later and in the south.

**What became of the covers.** Mounts of Irish and Northumbrian church metalwork, cut down and fitted with pins, lie in Norwegian women's graves of the ninth century (Wamers 1985, from memory) (?). The little house-shaped box on the dealer's cloth is modelled on the Insular shrine in Copenhagen with the later runes "Ranvaik owns this casket" (from memory) (?).

**What a book is.** The great Lindisfarne Gospels took some 150 calfskins (`dossiers/lindisfarena.md`). Scribes wrote round holes in the skin; such holes are in most early books. The book riddle of the Exeter Book (no. 26, Tupper 1910; `riddles_793.json`) tells the making of a gospel book from the killing of the calf to the gold: the scribe at York gives it.

**The people and their dates.** Higbald, bishop from 780 or 781, died 25 May 803; Ecgberht was consecrated at Bywell on 11 June 803 and died in 821 (Symeon; the dossier). Eanbald I, archbishop of York, died 10 August 796; Eanbald II followed and is last heard of in 808; Alcuin scolded him for his armed retinue (spine). Alcuin was abbot of Saint Martin's at Tours from 796 and died 19 May 804. Gervold of Saint-Wandrille took the king's tolls at Quentovic to 806 (Gesta abbatum Fontanellensium c. 12). Cuthbert's day is 20 March, Aidan's 31 August. The community left the island with the saint's body in 875.

**Invented, and said so.** The book itself: a lesser gospel book of the house, written by a monk Wigfrith in Bishop Aethelwold's time (721-740), with its hole and its closing prayer. It is not the Lindisfarne Gospels: no early source says the great book or its cover was harmed in 793, and Higbald says on his card that the great book was not found. Symeon's gospel book that fell into the sea about 880 and was found on the sands is another book and a later story; the sea card names it only in its note. Invented people: Thorleif, Asulf, Holmfast, Sigewine, Beornwine and Aldger are the spine's; Ketil the dealer, Gunnhild (Thorleif's wife), Eadwulf the scribe and the unnamed thegn are this chain's. That the king's men at Bebbanburg saw the sails and did not come is the spine's hook, in no source (?). That Alcuin kept a hospice at Saint-Josse by Quentovic from 792 is the spine's (?).

No web search was used: the history is the spine's and the Lindisfarena dossier's, with the sources they name, and what is marked "from memory" above.

## The people

| Who | Id | In the cards |
|---|---|---|
| Higbald, bishop of Lindisfarne | `cast.higbald` (to 25 May 803), historical | actor `bishop` at steps 2, 9 and the home ending |
| Ecgberht, bishop after him | historical (803-821) | the same actor, by the year; after 821 the bishop is unnamed |
| Eanbald, archbishop of York | `cast.eanbald` (two men of one name, 796), historical | actor `archbishop` |
| Alcuin; Gervold, abbot and toll-gatherer | `cast.alcuin`, `cast.gervold`, historical | spoken of, never met |
| Thorleif, ship's chief at Ogvaldsnes | `cast.thorleif` | actor `chief` |
| Asulf, dealer in captives | `cast.asulf` | actor `dealer` at Sliesthorp |
| Holmfast, the Svea | `cast.holmfast` | actor `svea` |
| Sigewine, Alcuin's letter-carrier | `cast.sigewine` | actor `monk` at Quentovic |
| Beornwine, the English lookout | `cast.crew.beornwine` | actor `lookout` |
| Aldger, the steersman | `cast.crew.aldger` | actor `steersman` |
| Ketil, Gunnhild, Eadwulf, a thegn of Bebbanburg | invented | actors `dealer` (step 1), `wife`, `scribe`, `thegn` |

## The steps

Scheduled cards (weight 0) open on the first call at their port after their day. "Doubtful" is a chance of 0.45 to 0.5 before the crew's skill. `P_` stands for `ch_lindisfarne_book_`.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_lindisfarne_book.s01_strip_of_gilt` | Skiringssal, Ripa or Sliesthorp, Aug 793 to 803; drawn | buy the strip (12 d) / leave it and ask where the skins went / drink with him in Norse (needs the tongue) | `P_1`, `lf_trail_north` (spine); `P_mount`; `P_name` |
| 2 | `harbour.chain_lindisfarne_book.s02_what_is_gone` | Lindisfarena, to May 803; drawn | promise to look / hand him the strip (needs it) / take his order for wine | `P_2`, `P_marks`, `lf_knows_boys` (spine) |
| 3 | `harbour.chain_lindisfarne_book.s03_calves` | Eoforwic, 3 days after step 2; optional | stay three days and learn books / take the order for colours / ask only for Alcuin's letter | `P_3`; `bk_knows_worth` or `bk_parchment` (spine) |
| 4 | `crew.chain_lindisfarne_book.s04_hole_in_the_page` | the crew, any time after 1 or 2; optional | sew a sealskin wrap / learn the first words of John / tell him to mind the sea | `P_crew`; `P_wrap`, or `P_words` with `P_marks` |
| 5 | `harbour.chain_lindisfarne_book.s05_box_of_skins` | Ogvaldsnes or Skiringssal, May to September, Aug 793 to 814; drawn after 1 or 2 | 300 d of weighed silver / two swords / say what it is and leave it / come back after dark (doubtful) | `P_5`; `P_bought` + `P_aboard` + `lf_ransom_paid`; or `P_seen`; or `P_stolen` + `P_feud` + `P_aboard`; or `P_caught` |
| 6 | `sea.chain_lindisfarne_book.s06_green_water` | at sea with the book aboard | lie to and nurse it (sure) / sail on, it is wrapped (needs the wrap; sure) / drive on (doubtful) | `P_6`; `P_wet` on a failure |
| 7 | `sea.chain_lindisfarne_book.s07_square_sail` | at sea with the stolen book | pay his price and more (360 d) / run for the shoals (doubtful) / hand it back | `P_7`; paying clears `P_feud`; losing or handing back clears `P_aboard`, sets `P_retaken` |
| 8 | `harbour.chain_lindisfarne_book.s08_asulfs_lot` | Sliesthorp, 240 days after leaving it or being caught (200 after losing it at sea) | outbid the Svea (250 d) / two swords / tell the Svea its true worth and let it go | `P_8`; `P_bought` + `P_aboard`; or `P_left` and the timer `P_left_wait` (300 days) |
| 9 | `harbour.chain_lindisfarne_book.s09_low_water` | Lindisfarena with the book aboard, any year before 875 | give it and ask nothing / give it and ask its cost and freight / show it and carry it away | `P_9` with `P_gift` or `P_ransom`, `bk_carried`, `P_aboard` cleared; or `P_shown` |
| E1 | `harbour.chain_lindisfarne_book.end_home` | Lindisfarena, the next March or August after step 9 | the saint's peace, a relic and the wine (givers only) / the ransom in silver and wax / ground on the island | `P_end_home`, `changed_lindisfarne_book_home` |
| E2 | `harbour.chain_lindisfarne_book.end_york` | Eoforwic before 809, book aboard; drawn | his first price and a clerk / the scribe's price and a clerk (needs `bk_knows_worth`) / less silver and a plot / refuse | `P_end_york`, `changed_lindisfarne_book_york`, `P_island_remembers` |
| E3 | `harbour.chain_lindisfarne_book.end_alms` | Quentovic, 794 to May 804, book aboard; drawn | give it over as the king's gift / take part of the alms and carry on / let him read | `P_end_alms`, `changed_lindisfarne_book_kings_gift`; or `P_alms` |
| E4 | `harbour.chain_lindisfarne_book.end_left` | Ogvaldsnes or Skiringssal, May to September, 300 days after step 8's "let it go" | tusks and a guest's seat / a partner (needs Norse) / silver, the sound's marks and word for the island | `P_end_left`; `P_raiders_friend` or `P_word` |

**Roads through.** Buyer: 1 or 2, (3), (4), 5, 6, 9, E1; or 5, 6, then E2 or E3. Thief: 5 (night), 6, 7, then as the buyer with a feud, or back to 8. The one who left it: 5 (tell), 8, then buy after all, or E4. Caught: 5 (night, failed), 8, then buy or E4.

**No dead ends.** Every card has an option that costs only time or pride (ask, promise, the letter, the wrap, tell and leave, lie to, hand it back, tell and let go, give, refuse, read; each ending has an option with no check). A failed theft is a beating and the long road by Sliesthorp. A lost race costs the book, not the boat. A sea in the boat spoils leaves and does not sink the story: the island takes a wet book. The island takes its book in any year before 875, whoever is bishop. The book is a flag, not a unit of `books` in the hold, so it cannot be sold by mistake.

**Every danger warns first.** The hounds on the chest are a line before the night option; the steersman says the worst before the sea; Thorleif says what he will and will not take before the race.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 (the strip costs 12 d) | the trail north; with Norse, the name and what the man wants |
| Step | 2 | 0, or a cake of wax (40 d) for the strip | the marks of the book; the bishop's feeling |
| Turn | 2 (order) | a premium of 1.6 on four casks of Rhine wine (about 140 d over the price) | a commission |
| Step | 3 (learn) | 0 | Latin +0.1, the book riddle, `bk_knows_worth` (worth 500 d at York) |
| Turn | 3 (order) | a premium of 1.7 on two loads of pigments | a copy of Alcuin's letter (`alcuin_793`), `bk_parchment` |
| Step | 4 | 0 | the wrap (step 6 made sure), or Latin +0.05 and the marks |
| Cost | 5 | -300 d, or two swords (168 d at their common price) | the book |
| Cost | 7 | -360 d to end the feud | or nothing, and a race |
| Cost | 8 | -250 d, or two swords | the book, cheaper than in the west |
| Turn | E3 (carry) | 150 d of the king's alms | deed carried_letters, Alcuin's letter |
| Finale | E1 ransom | 900 d and ten cakes of wax (400 d) | `unlock_title_cuthberts_man` |
| Finale | E1 ground | 400 d and six cakes of wax (240 d) | `unlock_plot_lindisfarena` |
| Hard road | E1 gift | 600 d, and a premium of 1.8 on six casks of wine | `unlock_title_cuthberts_man` and `unlock_relic_cuthbert_cloth` |
| Finale | E2 | 1,200 d (1,700 d with the scribe's lesson), or 600 d with the plot | `unlock_factor_eoforwic` or `unlock_plot_eoforwic`; the cost `ch_lindisfarne_book_island_remembers` |
| Finale | E3 | 1,000 d | `unlock_charter_quentovic` |
| Finale | E4 | eight tusks (720 d), or five bundles of marten (600 d), or 600 d | `unlock_title_thorleifs_guest`, or `unlock_partner_felagi` with the cost `ch_lindisfarne_book_raiders_friend`, or `unlock_pilot_karmsund` |

**Why a merchant could have won these.**
- *Saint Cuthbert's man* (`unlock_title_cuthberts_man`): names of friends and givers were written in a book of life kept on the altar and read at Mass. Lindisfarne's own, the Durham Liber Vitae, is of about 840; whether a list was kept in the 790s is not known (?). In the game: a guest's welcome at the Northumbrian minsters and a bishop who speaks for him.
- *A cloth from the coffin* (`unlock_relic_cuthbert_cloth`, already in `story/unlocks_793.json`): cloths that had lain on a saint's body were the ordinary gift of a shrine to its friends.
- *Ground on the island* (`unlock_plot_lindisfarena`): the island was the bishop's to give. No merchant's strand is recorded there (?).
- *A man or a plot at Eoforwic* (both already in the unlock table): the archbishop was the greatest landlord in the city.
- *Toll freedom at Quentovic* (already in the unlock table): the king's to give, as to Saint-Germain in 779, through the abbot who took the toll.
- *Thorleif's guest* (`unlock_title_thorleifs_guest`): guest-friendship with a chief, which keeps his ships and his friends' off yours. *The Karmsund* (`unlock_pilot_karmsund`): the sheltered way north past his hall. *A partner* (`unlock_partner_felagi`, in the table): the word felag is on rune stones from the tenth century on; for the 790s it is an older custom under a later name (?).

**Costs that last.** `ch_lindisfarne_book_feud`: Thorleif thinks he is owed (set by the theft; cleared only by paying at step 7 or losing the book back). `ch_lindisfarne_book_island_remembers`: the bishop of Lindisfarne knows who sold his book to York. `ch_lindisfarne_book_raiders_friend`: the partner's kin are known in Christian ports. `ch_lindisfarne_book_wet`: the book came home spoiled in part; it is remarked on and changes no payout.

## The endings

| Ending | Flag | How | What becomes of it | Picture |
|---|---|---|---|---|
| Home | `ch_lindisfarne_book_end_home`, `changed_lindisfarne_book_home` | carried over the sand and given, or ransomed | **History changed.** Something taken on 8 June 793 comes back, by a Frisian's hand. The house has its daily gospel again, a thegn of the king's rock has paid for his shame, and a merchant of Dorestad is Saint Cuthbert's man. | The church on the feast, new doors open, the book on the altar at the first leaf of John, the merchant at the front. |
| York | `ch_lindisfarne_book_end_york`, `changed_lindisfarne_book_york` | sold to the archbishop | **History changed.** The book is safe behind stone and the island never forgives it. The merchant has silver and a foothold in the richest market of the north. | The staith at York: the archbishop and his thegns standing over a small boat, the scribe looking at the river. |
| The king's alms | `ch_lindisfarne_book_end_alms`, `changed_lindisfarne_book_kings_gift` | given to Alcuin's monk at Quentovic | **History changed.** The book goes home as King Charles's gift: the island thanks the king of the Franks, Alcuin's promise is kept in part, and the merchant goes toll-free on the Canche. | The bank at Quentovic at evening: the stooping monk on a mooring post with the open book on his knees. |
| Left | `ch_lindisfarne_book_end_left` | told its worth and let go east | **What the sources allow.** Nothing is recorded as coming home. The mount is a brooch, the leaves are at Birka, and the man who emptied the church pays for a true word. | The hall at night: the chief holding out a walrus tusk, the gilt mount on his wife's cloak. |

**What would follow in later years** (for later cards to read from the flags):
- `changed_lindisfarne_book_home`: the book is among the things carried from the island with Cuthbert's body in 875; a bishop's letter can name the house that brought it back; an heir can ask at Chester-le-Street, and later at Durham, to see the leaf with the hole. If `ch_lindisfarne_book_feud` is still set, a striped sail is a card the sea may deal.
- `changed_lindisfarne_book_york`: the book lies in the minster library and is lost with it when the army takes York in 866-867 (?); until then the island's bishops are cool to the house (`ch_lindisfarne_book_island_remembers`).
- `changed_lindisfarne_book_kings_gift`: Northumbria's bishop is in the Frankish king's debt, and the house is known at court as the one that found the book; Sigewine is a friend.
- `ch_lindisfarne_book_end_left` with `ch_lindisfarne_book_word`: the island knows its book is whole and at Birka. The spine has an English captive there who reads marks (cast.tidwine): the thread `holy_island` can pick that up.

## How long it takes

The earliest sight of the book is August 793. The buyer's shortest road is one sailing season out and home and then the wait for the feast: a book bought in summer 794 lies on the altar at Aidan's day at the end of August, or on Cuthbert's day the next March. The road by Sliesthorp adds 240 days (the autumn lot, seen the next spring); the ending in the north adds 300 more. A full telling runs from one summer to three years, alongside ordinary trading: the trail stays open to 814 and the island takes its book in any year before 875.

## For the integrator

- New flags the game will read: `unlock_title_cuthberts_man`, `unlock_plot_lindisfarena`, `unlock_title_thorleifs_guest`, `unlock_pilot_karmsund`; the three `changed_` flags above.
- The outcome texts (`...out_<option>`) are in the writing file; each option's `note` names its own.
- If the first mate of the player's boat is not the spine's crew, steps 4, 6 and 7 still work: the actors are roles (lookout, steersman).
