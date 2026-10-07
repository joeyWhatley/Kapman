# Chain: great_army_865 (the great army and the scales)

Cards: `Assets/Kapman/Game/Data/encounters/chain_great_army_865_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_great_army_865.json` (203 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
Part of the spine's thread `price_of_england` (Design/content/spine.md no. 12): this chain tells its first fifteen years and sets two of its flags, `en_paid_share` and `en_hostage_given`.

## The story in five sentences

In the autumn of 865 a heathen army sits on Thanet taking Kent's promise of money, and beside the reeve who collects it sits a Dane called Toki with a folding balance, buying an army's dinner by weight.
The player meets the army again wherever it winters: among the East Angles, who give it horses (866); at York, where the taken are sold on the strand and a schoolboy named Wulfric can be bought free (867); in East Anglia again when King Edmund is killed (869); and in London (871-872), where Toki unwraps a gospel book written in gold on purple.
The ealdorman Alfred and his wife Werburg want that book out of heathen hands, and the player can carry their gold and bring it home, buy it first and sell it to them, skim the ransom, or sail away with gold or book.
In 876 and 877 the king's reeve at Hamwic wants a man who knows the camp to tell him which hostages matter, and in the last weeks of 877 the army's buyer in London buys fodder for a winter ride and no ale for Yule: the player can carry that to the reeve before Twelfth Night, sell the fodder, or go home.
From that come four ends: the king warned and never surprised at Chippenham; the golden book laid on the altar of Christ Church with the player as witness; a pledge left at the Lea and leave to cross between the two armies; or a plot among the Danes at Gipeswic and Toki as a sworn fellow.

## How a player meets it

- It has six ways in, so that a house which is not in Kent in the autumn of 865 still finds it. Steps 1 to 5 and step 8 each fire by themselves at their port in their years (weight 2 to 2.5, once), need no earlier step, and set `ch_great_army_865_1`:
  Kent (Sarre, Fordwic, Dofras) Aug 865 to Apr 866; Gipeswic or Dommoc Oct 865 to Oct 866; Eoforwic Nov 866 to 869; Gipeswic or Dommoc Nov 869 to 870; Lundenwic Oct 871 to 872; Hamwic 876 to 877.
- Every step also posts news at Lundenwic (the army on Thanet, the horses, York, Edmund, Wareham), so a player who reads the boards hears where the army is and can sail to meet it.
- Each step schedules the next at its port, and the next is also drawn there (weight 2 to 3), so a missed appointment is not a lost story: the army simply has moved on to its next winter.
- A place writer may add `card:harbour.chain_great_army_865.s01_thanet` to a Sarre site (the Wantsum shore), `...s05_book` to a Lundenwic site and `...end_line` to a site at the mouth of the Lea.

## The real history under it

**The army's road.** Anglo-Saxon Chronicle (Giles 1847 and Ingram 1823 read for the sense; nothing is quoted, the cards give our own rendering in the Chronicle's voice):

| Chronicle year | What it says | Card |
|---|---|---|
| 865 | The heathen army sat on Thanet and made peace with the men of Kent, who promised money; under cover of the peace and the promise the army stole away by night and ravaged all eastern Kent. | s01 |
| 866 | A great heathen army came to England and took winter quarters among the East Angles, and were horsed there; the East Angles made peace. | s02 |
| 867 | The army went over the Humber to York; the Northumbrians had cast out King Osberht and taken Aella; both kings were slain. (York taken 1 November 866, the kings killed 21 March 867: the northern annals in Symeon of Durham (?).) | s03 |
| 870 | The army rode over Mercia into East Anglia and wintered at Thetford; King Edmund fought them, and the Danes had the victory and slew the king. (20 November 869, nine days after Martinmas.) | s04 |
| 872 | The army went from Reading to London and took winter quarters; the Mercians made peace with them. | s05, s07 |
| 876 | The army stole into Wareham; the king made peace; they gave hostages, the worthiest in the army, and swore on the holy ring, which they would not do before for any people; then the mounted army stole away by night to Exeter. | s08 |
| 877 | A hundred and twenty ships lost in a storm at Swanage; hostages again at Exeter; at harvest the army went into Mercia. | s08, s09 |
| 878 | At midwinter after Twelfth Night the army stole to Chippenham; the king went into the fastnesses of the moors; Athelney at Easter; Edington; Guthrum baptised at Aller with the king as godfather. | s09, s10, the endings |
| 880 | The army went from Cirencester into East Anglia, and settled the land and shared it out. | end_camp, end_line |

Whether the Thanet force of 865 was the same army that wintered in East Anglia is not certain (?): the Chronicle's year begins in the autumn and the two entries may be one season.

**The camp as a market.** The winter camp at Torksey on the Trent (872-873) has given hundreds of lead weights, some set with pieces of cut-up English metalwork, folding balances, hack-silver, cut dirhams, Northumbrian stycas, gaming pieces and signs of smiths and of women: an army with its own traders (finds published by Hadley and Richards, from memory (?)). The game reads this back to the camp of 866 (?). The ertog and the ore are in `lore/law_tolls.md`. Toki the weigher is that find made into a man.

**The golden book.** The Codex Aureus, a gospel book written about 750, probably at Canterbury, in gold and silver on alternate purple and plain leaves, now in the Royal Library at Stockholm. On folio 11, above and below the opening of Matthew, an Old English inscription says that Alfred the ealdorman and Werburg his wife got these books from the heathen army with their pure money, that was with pure gold, for the love of God and for their souls' need, and because they would not have the holy books stay longer in heathen hands, and that they give them to Christ Church, to be read every month for Alfred, Werburg and their daughter Alhthryth (our rendering; not a quotation). Alfred was ealdorman of Surrey; his will survives (S 1508, between 871 and 889). **Not recorded:** where or when the army held the book, what was paid, or who carried it. The game puts it in the London camp of 871-872 (?), sets the price at thirty mancuses of gold (900 d; a mancus is thirty pennies) (?), and invents the boat, the priest and the weighing.

**London's tribute.** Bishop Waerferth of Worcester leased land at Nuthurst for twenty mancuses of gold "because of the very pressing affliction and immense tribute of the barbarians" in the year the heathens stayed in London (charter S 1278, of 872; the sense from memory, the wording not to be trusted (?)). Pennies in Halfdan's name with a London monogram exist; their date is disputed (?), so the card only says that dies are being cut for whoever holds the town.

**King Edmund.** The Chronicle says only that he was killed. The arrows, the wolf and the head are Abbo of Fleury's, about 985: the priest in step 4 refuses to tell what he did not see.

**The peace and the crossing.** The treaty of Alfred and Guthrum (`lore/law_tolls.md` 7.2): the border up the Thames and the Lea; one price for a slain man, Dane or English; and clause 5, no slave or free man goes over to the other army without leave, but if they need to trade in cattle and in goods it is allowed when hostages are given as a pledge of peace and as proof that a man goes with a clean back. It is undated, between 878 and 890, and likely after Alfred took London in 886 (?): the game lets the rule hold from 880. A silver bond in place of a living hostage is the game's (`Design/economy_and_trade.md`) (?).

**Prices.** `goods_793.json`: a captive 48 d, a book 360 d, church plate 240 d the piece, a horse 120 d. The boy at 60 d is a dealer's price for one who reads.

**Invented, and said so.** Toki and his sister's son Tofi (both short names common on Danish rune stones, `names_placeholder.json`); Wulfric the schoolboy (Wulf- + -ric); Hunsige, priest of Christ Church (Hun- + -sige); every unnamed reeve, king's man, priest, clerk, dealer and chief; that a merchant sorted the hostages of 876; that the army's buyer was in London before the ride on Chippenham; that anybody warned the king. Gloucester as the camp of autumn 877 is from Aethelweard (?); that the king kept Christmas at Chippenham is a common reading, not the Chronicle's (?).

No web search was used: the history is the spine's, the realms' and the dossiers', with the sources they name, and the Chronicle from memory. Each (?) above is a thing to check before it ships.

## The people

| Who | In the cards |
|---|---|
| Alfred, ealdorman of Surrey, historical (the Codex Aureus inscription; will S 1508) | actor `ealdorman` (role count) at s06 and end_ransomer |
| Werburg his wife, historical | spoken of, in the outcome texts and in two pictures: there is no role for a lady (see Needs) |
| King Alfred, King Edmund, King Guthrum (baptised Aethelstan), Bishop Waerferth, Halfdan | spoken of, never met |
| Toki, the army's weigher and buyer; invented | actor `weigher` (role merchant) at s01, s02, s04, s05, s07, s09, end_line, end_camp |
| Tofi, his sister's son; invented | actor `captive` at s08, `nephew` (role warrior) at s09 |
| Wulfric, a pupil of the school at York, about twelve in 867; invented | actor `boy` (role child) at s03; hired aboard if bought free |
| Hunsige, mass-priest of Christ Church; invented | actor `priest` at s06, s07, end_ransomer |
| The king's reeve at Hamwic; invented, unnamed | actor `reeve` at s08, s10, end_warned |
| A Kentish reeve, King Edmund's thegn, an East Anglian priest, the bishop's clerk, a slave dealer, a ship-lord, the reeve at the Lea | invented, unnamed |

The army moves, so Toki is forged anew at each port. What he remembers of the player is carried by the chain's flags (`_scales`, `_camp`, `_tofi`), not by an NPC feeling.

## The steps

All cards are harbour cards. "Likely" is a chance of 0.55 and "doubtful" 0.5 before the crew's trade skill. Every step sets its own flag `ch_great_army_865_<n>`; steps 1 to 5 and 8 also set `_1`. Short flag names below drop the prefix `ch_great_army_865`.

| # | Card id (after `harbour.chain_great_army_865.`) | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `s01_thanet` | Sarre, Fordwic or Dofras; Aug 865 to Apr 866 | **pay** 20 d into Kent's share / **sell** the ship's ale to the army / **watch** the weighing (free) | `_1`; pay: `en_paid_share`, `_english`; sell: `_camp` |
| 2 | `s02_horses` | Gipeswic or Dommoc; Oct 865 to Oct 866 | **scales**: sit two days and learn the weights (free) / **iron**: take the army's order / **tell** the king's man what Kent saw (needs step 1) | `_2`; `_scales` / `_camp` / `_english` |
| 3 | `s03_york` | Eoforwic; Nov 866 to 869 | **ransom** the boy for 60 d / **buy** four captives for 120 d / carry the **names** south (free) | `_3`; ransom: `_boy`, `_english`; buy: `_camp`; names: `_english` |
| 4 | `s04_edmund` | Gipeswic or Dommoc; Nov 869 to 870 | **carry** the priest and the plate to Kent / **buy** the camp's church silver for 150 d / **hear** it and carry the news (free) | `_4`; carry: `_plate`, `_english`; buy: `_camp` |
| 5 | `s05_book` | Lundenwic; Oct 871 to 872 | carry **word** of the book to Kent (free) / **buy** it for 400 d / pay 30 d to the town's **share** | `_5`; `_book_word`, `_english` / `_book_bought`, `_camp` / `en_paid_share` |
| 6 | `s06_gold` | Fordwic; 871 to 874, after step 5 | without the book: **carry** the gold, or **refuse**; with it: **sell** to them for 600 d, or **keep** it for a market over the sea | `_6`; carry: `_gold`; sell: `_book_home`; keep: `_book_lost`, `_cost_church`, `changed_codex_aureus_lost` |
| 7 | `s07_weighing` | Lundenwic; to 874, carrying the gold | **pay** the thirty as agreed / **skim** (likely) / **run** with the gold | `_7`; pay: `_book_home`; skim: `_book_home`, `_skimmed`; run: `_book_lost`, `_cost_church`, `changed_codex_aureus_lost` |
| 8 | `s08_ring` | Hamwic; 876 to 877 | **tell** the reeve which hostages matter (needs Norse, `_scales` or `_camp`) / **free** the young Dane for 80 d / stay **aside** (free) | `_8`; tell: `_reeve`, `_english`; free: `_tofi`, `_camp` |
| 9 | `s09_fodder` | Lundenwic; Nov and Dec 877 | **warn** the king's reeve / **sell** the fodder / go **home** (free) | `_9`; warn: `_warn`; sell: `_camp` |
| 10 | `s10_twelfth_night` | Hamwic; from Nov 877, after "warn" | **known**: he believes you (sure; needs `_reeve`) / **swear** on his relics (doubtful) / stay **silent** (free). In 878 it is too late and only silence is left. | `_10`; believed: `_warned`, `_cost_danes`, `changed_chippenham_warned`; not believed: `_unheard` |
| E | `end_warned` | Hamwic; June 878 to 890; needs `_warned` | the king's **gifts** / **land** for half the silver / **wait** | `_end_warned`, `_done` |
| E | `end_ransomer` | Fordwic; June 878 to 890; needs `_book_home` | stand **witness** / **foster**: Wulfric freed at the altar (needs `_boy`, not `_skimmed`) / **wait** | `_end_ransomer`, `_done` |
| E | `end_line` | Lundenwic; 880 to 890; needs `_1` | leave a **bond** of 60 d / stand **surety** for other keels (needs `_scales` or `_camp`) / **wait** | `_end_line`, `_done`, `en_hostage_given` |
| E | `end_camp` | Gipeswic or Dommoc; 880 to 890; needs `_camp` | the **plot** and Toki's fellowship / your part in **silver** / **wait** | `_end_camp`, `_done`, `_cost_church` |

No dead ends: every step has a free choice; a failed chance (the skimming, the oath) hurts and the story goes on; a step missed in its years is simply skipped, and the ending `end_line` is open to anyone who touched the chain. The one danger, a warning that is not believed, is said plainly on the card before the choice (the reeve: "I do not know you").

## The endings

A house takes one ending: each sets `ch_great_army_865_done`, and each can be put off ("not yet") to sail to another.

| Ending | Flag | How | What becomes of people and place |
|---|---|---|---|
| The king was not at table | `ch_great_army_865_end_warned`, with `changed_chippenham_warned` | Carry the buyer's talk to Hamwic before the year turns and be believed (sure if you sorted the hostages for the reeve in 876-877; doubtful if he does not know you). | **History changed.** The king meets the army at Chippenham with his host; peace at Easter 878. The reeve is your friend for life; the Danes ask who talked (`_cost_danes`). |
| Written above and below | `ch_great_army_865_end_ransomer` | Bring the golden book home, by carrying the gold honestly, by skimming, or by buying and selling it. | **What happened.** The book is given to Christ Church and the gift written into it. On the hard road Wulfric is freed at the altar as the house's foster-son. |
| Hostages at the Lea | `ch_great_army_865_end_line`, with the spine's `en_hostage_given` | Leave a pledge with the reeve at the Lea from 880. | **What happened.** Two laws in England and a river between them; the Frisian is one of the few allowed over. Toki, with land he does not want, trades from the far bank. |
| The land shared out | `ch_great_army_865_end_camp` (and `changed_codex_aureus_lost` if the book was kept or the gold stolen) | Deal with the camp at any step, then come to Gipeswic from 880. | Toki has a share by the river and makes the player his fellow. The English priests of the shire remember (`_cost_church`). |

**What would follow from the changes** (for later cards):

- `changed_chippenham_warned`: no flight to Athelney, no Edington, and no baptism at Aller. Guthrum keeps his own name and gods, so no pennies of "Aethelstan" in East Anglia and a slower church there; the Saint Edmund pennies of about 895 would come later or not at all (?). Alfred is never the king in the marshes: the peace is made from strength a season sooner, and the border may lie further east (?). The burhs and the ships of the 880s still follow, since the army of 892 still comes. A later card at Gipeswic or Lundenwic can read the flag and speak of "the Twelfth Night ride that failed".
- `changed_codex_aureus_lost`: Christ Church never has the golden gospels, and the inscription is never written, so nothing is known afterwards of the ealdorman's ransom. A later card at Sliesthorp or Hleidr can show a king's guest-gift of gold letters that nobody there can read. (In our world the book did go north in the end: it was bought in Spain in 1690 for the Swedish royal collection. A page note only; nothing in the game says so.)

## Payouts

| Where | Road | Silver | And |
|---|---|---|---|
| s01 | pay / sell / watch | -20 / +36 / 0 | toll eased 60 days and the reeve's goodwill / the army's memory / a true price word: oats dear at Gipeswic |
| s02 | scales / iron / tell | 0 / +20 / +15 | Old Norse to 0.5 / a commission, 6 bar iron at 1.6 / a deed (carried letters) |
| s03 | ransom / buy / names | -60 / -120 / 0 | Wulfric hired, a deed (alms) / 4 captives worth 192 d or more / a deed |
| s04 | carry / buy / hear | 0 / -150 / 0 | a passenger and a deed / 2 church plate worth 480 d (turn) / a true price word: wheat dear |
| s05 | word / buy / share | 0 / -400 / -30 | the story / the book (360 d at market, 600 d at Canterbury) / toll eased 120 days, ale dear at Lundenwic |
| s06 | sell | +600 | turn: 200 d clear on the book, a deed (fair dealing) |
| s07 | pay / skim / run | +80 / +150 or -40 / +900 | a deed (kept commission) / the priest saw / a deed (broke commission), the church's memory, the book lost |
| s08 | tell / free / aside | +100 / -80 / 0 | turn: the reeve's trust, which makes the warning sure / Tofi's debt / a true price word |
| s09 | sell | +160 | turn |
| **end_warned** | gifts / land | **+1,500 / +750** | `unlock_title_alfreds_protection` and `unlock_charter_hamwic`; land adds `unlock_plot_hamwic` |
| **end_ransomer** | witness / foster (hard road) | **+1,200 / +2,400** | `unlock_title_christ_church_friend` and `unlock_charter_fordwic`; the hard road adds `unlock_kin_fosterling_wulfric` |
| **end_line** | bond / surety | **+700 / +1,000** clear of the 60 d pledge | `unlock_title_leave_to_cross`, the spine's `en_hostage_given`, a commission of 12 wool cloth at 1.7; surety adds `unlock_factor_gipeswic` |
| **end_camp** | plot / silver | **+900 / +1,400** | `unlock_plot_gipeswic` and `unlock_partner_toki` / `unlock_title_guthrums_guest`; both carry `ch_great_army_865_cost_church` |

**Why a merchant could have won these.**

- *The king's protection and toll freedom by his writing:* English kings freed named ships of toll by charter from the 730s (`dossiers/fordwic.md`, S 29; the Lundenwic remissions); the treaty of 878-890 itself shows a king regulating who may trade. A grant to a foreign layman is the game's step (?).
- *A name in a church's book and freedom of its landing:* confraternity books list lay friends to be prayed for (the Durham Liber Vitae); the Codex Aureus inscription is exactly such a remembered gift.
- *A foster-son freed at the altar:* manumission before the altar, written into a gospel book, is English practice recorded from the 920s (?).
- *Leave to cross:* clause 5 of the treaty.
- *A plot and a fellow among the Danes:* the army "shared out the land" (Chronicle 880); Scandinavian combs and sunken houses appear at Ipswich after 880 (`dossiers/gipeswic.md`); the felag is the Norse sworn fellowship of trade, on rune stones of the next century (?).
- *Guest-right from Guthrum:* a king's peace over a foreign trader, as Danish kings gave at Sliesthorp (Rimbert).

**Costs that last.** `ch_great_army_865_cost_church`: the churches of England remember a man who kept or lost a holy book, or who took his fortune from the camp. `ch_great_army_865_cost_danes`: on the Danish side they ask who talked before Chippenham.

## How long it takes

The chain follows the army's real years, so it runs from the autumn of 865 to June 878 at the soonest (twelve and a half years) and to 880 for the two endings on the border and in East Anglia; the endings stay open to the end of 890. It is a story to live beside, not to play through: one port call every year or two, between ordinary voyages and other chains. The shortest whole road is step 8 (Hamwic, 876), step 9 and step 10 in the winter of 877, and `end_warned` the next summer: about two years. The waits are real: a winter camp that has moved by the next spring, six days' sail between London and Canterbury with the gold, and the weeks before Twelfth Night.

## Pictures

| Card | Picture |
|---|---|
| s01_thanet | Dusk on the Wantsum shore at Sarre in autumn: an English reeve with a tally stick and a sack, and across a trestle a broad Dane holding up a small folding balance; far off over the flat island a line of camp fires under a low red sky. |
| s02_horses | A winter camp outside Gipeswic on a grey morning: ditch and bank, sailcloth tents, English grooms leading a string of horses in through the gap; in front a Dane on a sea chest with a balance and a row of lead weights. |
| s03_york | The muddy strand by the Foss under the broken Roman wall, a cold bright noon: a dozen roped people, among them a thin boy of twelve with ink on his fingers; a fur-capped dealer points along the line with a stick. |
| s04_edmund | A frozen landing on the Orwell at first light: a priest clutching a heavy sack; a Dane at a trestle with scales, a hacked silver cup in the pan; ravens in a leafless tree and smoke inland. |
| s05_book | Inside the broken Roman wall of London on a winter afternoon: a Dane opening a cloak to show a great book, its page purple with gold letters; a tonsured clerk with a wax tablet looks away; tents and horses among fallen masonry. |
| s06_gold | A timber hall at Canterbury by lamplight: a grey ealdorman standing, Werburg seated weighing gold coins in a small balance, a priest holding open a leather bag; the merchant just inside the door. |
| s07_weighing | The London camp at midday, snow on the ground: a balance with gold in one pan, the great book closed beside it; the weigher, a tall chief in a wolfskin, and a small priest gripping a leather bag. |
| s08_ring | The shore at Hamwic on a windy autumn day: a reeve with a wax tablet, Danish hostages sitting under guard on a beached boat's rail, one young man in a blanket set apart; a wreck's ribs beyond. |
| s09_fodder | The Thames strand in hard frost, late afternoon: sacks of oats stacked high beside a Dane with scales and an open silver chest; pack horses waiting; no ale cask anywhere. |
| s10_twelfth_night | A reeve's hall on a midwinter night: a Christmas table half cleared, the merchant wet from the sea with his hand on a small silver reliquary; through the door a groom saddles a horse by torchlight. |
| **Ending: warned** | The hall at Hamwic on a bright summer morning: an iron-bound chest of pennies open on the table, the reeve holding out a sealed strip of parchment; ships on the Itchen and a peaceful shore. |
| **Ending: ransomer** | Christ Church, morning light through a narrow window: the great book open on the altar at a purple page, a scribe writing small dark words in its upper margin; the ealdorman, Werburg and their young daughter, the merchant a little apart. |
| **Ending: line** | The mouth of the Lea on a still grey morning: a reeve at a post on the near bank holding a notched tally, the merchant's boat loaded with cloth, and on the far bank a Dane with horses on a rope, raising one hand. |
| **Ending: camp** | The river bank at Gipeswic on a spring evening: a new marked post on bare ground, a grey-haired Dane shaking hands with the merchant over it, a plough leaning unused against a new timber house, two boats side by side. |

The same lines are in the writing file as `art.<card id>`.

## Needs (not added: the spine owns the shared tables)

- A role for a noblewoman (`lady`) in `npc_roles_793.json`, so Werburg can speak at step 6 and at the ending.
- The new unlock flags to be read by the game: `unlock_title_alfreds_protection`, `unlock_title_christ_church_friend`, `unlock_title_leave_to_cross`, `unlock_title_guthrums_guest`, `unlock_charter_hamwic`, `unlock_charter_fordwic`, `unlock_plot_hamwic`, `unlock_partner_toki`, `unlock_kin_fosterling_wulfric` (`unlock_plot_gipeswic` and `unlock_factor_gipeswic` are already set by other chains).
- The `changed_chippenham_warned` and `changed_codex_aureus_lost` flags to be listed with the other changes of history.
- A day of the month in the condition grammar, if the warning is ever to be judged against Twelfth Night itself and not against the turn of the year.
- A deed for a ransom honestly carried and one for theft from a church would say more than `kept_commission` and `broke_commission` do.
- The treaty of Alfred and Guthrum as a law of the border (`LawDef` keys on zone and year only: `lore/law_tolls.md`), so that crossing the Lea without `en_hostage_given` can cost something.

## To check before it ships

Every (?) above, and first: the wording and date of S 1278; the day York fell and the kings died; Gloucester in 877; the Torksey finds; the date of the treaty; manumission at the altar before the 920s; whether the Thanet army of 865 was the great army.
