# Chain: maldon_991 (the tribute years, 991 to 1002)

Cards: `Assets/Kapman/Game/Data/encounters/chain_maldon_991_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_maldon_991.json` (218 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
Flags are written short below: `_1` stands for `ch_maldon_991_1`, and so on.

## The story in five sentences

In August 991 ninety-three ships plunder Ipswich and kill the ealdorman at Maldon, and the English king's council agrees for the first time to pay the fleet to go away: ten thousand pounds, raised on every hide, so that minsters sell their chalices and the people the ships took are sold on the quay at Rouen.
The player can pay, lend, ransom and carry for the English, or follow the silver: in the winter of 994 the fleet lies at Southampton with sixteen thousand pounds and nothing to buy, and at Hedeby a fisherman pays for a knife with King Aethelred's head.
Between the two shores stands Toke, a merchant of Hedeby who followed the fleet to sell to it, stayed for the peace sworn to merchant ships, and built a house at Billingsgate with an English wife; the player can go shares with him, stand surety for him, or keep his name off other men's sticks.
In 1001 the king's own Danish captain goes over to the raiders, the reeve begins asking who deals with Danes, and in November 1002 a priest brings word of the king's writ: on Saint Brice's mass-day, all the Danish men in England.
The player keeps his hatch shut, or carries Toke over the sea, or tries the door of the house where the king of the Danes' sister is held hostage; and is paid at Hedeby by a father, at London by a frightened reeve on an empty hythe, or by a king whose sister is alive.

## How a player meets it

- It starts by itself at **Gipeswic, Lundenwic or Sarre** on any call from August 991 to the end of 993: card `s01_first_tribute` (weight 3, once). Asking where the ships took their people is enough to begin.
- It can also be met at **Lundenwic from September 994 to the end of 995**: `s04_london_holds` needs no earlier step and sets `_1` itself.
- Step 1 schedules step 2 at Rotomagus; step 4 schedules step 5 at Hamwic. After that Lundenwic, Gipeswic, Hamwic, Rotomagus and Sliesthorp each carry a piece, so a house trading the Channel and the Danish run meets it without looking.
- The night of the writ (`s10`) is only at Lundenwic in October and November 1002. Step 9 warns of it from June 1001. A player who is elsewhere that autumn finds step 11 and an ending afterwards all the same.
- A place writer may add `card:harbour.chain_maldon_991.s08_billingsgate` to London's hythe and `card:harbour.chain_maldon_991.s07_pennies_north` to Sliesthorp's jetty for the years they hold.
- A player who first reaches England after 995 has missed it.

## The real history under it

**991.** "This year was Ipswich plundered; and very soon afterwards was Alderman Britnoth slain at Maldon", and in the same year it was first resolved to pay tribute to the Danes, 10,000 pounds, on the counsel of Archbishop Sigeric (Anglo-Saxon Chronicle 991, tr. J. Ingram 1823, read 6 Oct 2026 at avalon.law.yale.edu/medieval/ang10.asp; `chronicle_793.json` 991-08-maldon). The A text, under 993, gives Olaf ninety-three ships and the road Folkestone, Sandwich, Ipswich, Maldon. The battle was on 10 or 11 August (?); Byrhtnoth was buried at Ely, without his head according to the Liber Eliensis (?). The poem is in the chest (`texts_793.json` maldon_991, tr. Lumsden) and is given at step 3. The word of the time is *gafol*; "Danegeld" is Norman and is not used (`realms/england_south.md`).

**Rouen, 1 March 991.** King Aethelred and Duke Richard I made peace at Rouen before Leo, bishop of Trevi, legate of Pope John XV: neither would receive the other's enemies. The pope's letter is kept by William of Malmesbury, Gesta Regum ii.166 (from memory (?)). Rouen was where the fleets sold what they took in England (`spine.json`, price_of_england). That Ipswich people were sold there in 991 is ours (?).

**The tribute and the church.** Archbishop Sigeric borrowed 90 pounds of silver and 200 mancuses of gold from Bishop Aescwig of Dorchester against an estate at Risborough, so that the army would not burn Christ Church (charter S 882, 995; from memory (?)). Step 3's loan to the minster copies it in small. That the tribute of 991 was raised by the hide is assumed from the later gelds (?).

**994.** "This year came Anlaf and Sweyne to London, on the Nativity of St. Mary, with four and ninety ships"; the townsmen beat them off; the army harried Essex, Kent, Sussex and Hampshire, took winter quarters at Southampton, was fed from all the West Saxon kingdom and paid 16,000 pounds; Olaf came to the king at Andover with hostages sent to the ships, was received at the bishop's hands with the king as sponsor, and promised never to come again to England in enmity, which he kept (Anglo-Saxon Chronicle 994, Ingram for the quoted phrase; `chronicle_793.json` 994-09-08-london-attack). The treaty that followed, II Aethelred, with Olaf, Jostein and Guthmund, names 22,000 pounds and gives every merchant ship peace in a river mouth (`lore/law_tolls.md` 7.3; from memory (?)).

**Pennies in the north.** More pennies of Aethelred have been found in Scandinavia than in England (`spine.json`). King Swein struck a penny about 995 copied from Aethelred's, signed by a moneyer Godwine (`economy/silver_coin.md`; the legend from memory (?); the mint is not known and the game puts the bench at Hedeby (?)).

**London about 1000.** IV Aethelred 2: a small boat pays a halfpenny at Billingsgate, a larger boat with a sail a penny, a keel or hulk fourpence; the emperor's men are worthy of good laws like the Londoners (`chronicle_793.json` 1000-london-tolls; the date is uncertain (?)). A house of Tiel or Deventer is the emperor's.

**1001 and 1002.** Pallig joined the raiders with the ships he could gather, against all his pledges and after the king's gifts of land, gold and silver (Anglo-Saxon Chronicle A 1001; from memory (?)). In 1002 the king and council paid 24,000 pounds through Ealdorman Leofsige; in the spring "the lady, Richard's daughter" came to England; and the king ordered all the Danish men in England to be killed on Saint Brice's mass-day, 13 November, because he was told they meant to kill him and his councillors and take the kingdom (Anglo-Saxon Chronicle 1002, read in Ingram at avalon.law.yale.edu/medieval/ang11.asp; `chronicle_793.json` 1002-11-13-st-brice). The king's charter of 1004 for Saint Frideswide's, Oxford (S 909) speaks of Danes sprung up like cockle among the wheat and tells how those of Oxford broke into the church and were burned in it (from memory (?)). How many died, and where besides Oxford, is not known. Mass graves of young men at St John's College, Oxford, and on Ridgeway Hill in Dorset have been linked to these years, not surely (?).

**Gunhild.** William of Malmesbury (Gesta Regum ii.177, about 1125) says that Gunhild, Swein's sister and Pallig's wife, a hostage for the peace, was beheaded after her husband and son were killed before her, and foretold that her blood would cost England dear. No source of the time names her, and many doubt her (?). The chain treats her as real, puts her in the port-reeve's guest house at London, which is ours, and lets the player change her end.

**After.** "This year was Exeter demolished, through the French churl Hugh, whom the lady had appointed her steward there" (Anglo-Saxon Chronicle 1003, Ingram); Swein then burned Wilton, and Norwich and Thetford in 1004. He was king of England at Christmas 1013.

**Invented, and said so.** Everyone the player speaks to except Gunhild: Toke Asgotsson (Toke: DR 295), his father Asgot (Ronninge stone), his wife Leofflaed (name from the will S 1462), the port-reeve Wulfmund (Wulf- + -mund), the reeve Eadnoth (Ead- + -noth), Wulfgyth and Leofric of Ipswich, the priest Leofwine (Leof- + -wine), and the unnamed dealer, duke's man, singer, lighterman, steersmen, alewife and king's man. Also: merchant boats hired to feed the fleet; the reeve's list of Danes; the sale of the dead men's goods; a king's writ for a merchant house.

Web: no searches; two page fetches (the Avalon Project's Anglo-Saxon Chronicle, tenth and eleventh centuries).

## The people

| Who | Id | In the cards |
|---|---|---|
| Toke Asgotsson, merchant of Hedeby, then of London | invented | actor `toke` (s05, s08, s09, s10, end_north, end_saved) |
| Leofflaed, his wife, a Londoner | invented | actor `wife` (s08, s11, end_london) |
| Asgot, his father, at the south jetty of Hedeby | invented | actor `asgot` (s07, end_north) |
| Wulfmund, port-reeve of London | invented | actor `reeve` (s04, s08, s09, s11, end_london) |
| Eadnoth, the king's reeve at Gipeswic | invented | actor `reeve` (s01) |
| Wulfgyth and Leofric, a fuller's household of Gipeswic | invented | actors `woman` (s01, s03), `captive` (s02) |
| Leofwine, a mass-priest who writes for the reeve | invented | actor `priest` (s10, s11) |
| King Swein's English moneyer (the coin names a Godwine (?)) | half real | actor `moneyer` (s07) |
| Gunhild, King Swein's sister | William of Malmesbury only (?) | actor `gunhild` (s10, end_saved) |

King Aethelred, Archbishop Sigeric, Ealdorman Byrhtnoth, Olaf Tryggvason, King Swein, Duke Richard, Pallig and the lady Emma are spoken of and never met. The spine's thread is `price_of_england`; its cast (Cnut, Thorkell) belongs to later years and is not used. The moneyer of Gipeswic in `places/gipeswic.json` (Leofsige) is named in a note only.

## The steps

Scheduled cards open on the first call at their port after their day; all of them can also be drawn. "Likely" is a chance of 0.6, "doubtful" 0.4 to 0.45, before the crew's skill.

| # | Card id (after `chain_maldon_991.`) | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour. ... s01_first_tribute` | Gipeswic, Lundenwic or Sarre, Aug 991 to 993; drawn | pay 40 d toward the town's share / buy the minster's chalice (150 d) / pay nothing and ask | `_1`; `_paid` and `en_paid_share`, or `_bought` |
| 2 | `harbour. ... s02_rouen_quay` | Rotomagus, 12 days on, to 994 | ransom three (150 d) / speak of the oath (doubtful) / buy the plunder (80 d) / walk on | `_2`; `_ransomed` or `_loot` |
| 3 | `harbour. ... s03_song_of_the_fight` | Gipeswic, 10 days after a ransom, or drawn Sept 991 to 995 | take the street's thanks (needs `_ransomed`) / lend the minster 100 d / hear the song | `_3`; `_lent` |
| 4 | `harbour. ... s04_london_holds` | Lundenwic, Sept 994 to 995; drawn; a second way in | carry the king's food / pay 60 d toward London's share / ask what the army is short of | `_1`, `_4`; `_victual`, or `_paid` and `en_paid_share` |
| 5 | `harbour. ... s05_winter_camp` | Hamwic, 6 days on, Oct 994 to May 995 | hand over the king's load (needs it aboard) / sell to the fleet / sit by Toke's fire | `_5`, `_toki`; `_served`, or `_fleet` and `en_followed_silver` |
| 6 | `sea. ... s06_silver_ship` | at sea, 995 to 1001; optional | pass a cask of ale across / heave to and ask / run (likely) | `_hailed`; `_fleet` and `en_followed_silver` |
| 7 | `harbour. ... s07_pennies_north` | Sliesthorp, 995 to 1001; drawn | take Asgot's wine order / sell into the silver / weigh the new penny | `_7`, `_asgot`; `_fleet` and `en_followed_silver` |
| 8 | `harbour. ... s08_billingsgate` | Lundenwic, 997 to 1001; drawn | go shares with Toke (120 d) / stand surety / eat and sign nothing | `_8`, `_toki`; `_partner` or `_surety` |
| 9 | `harbour. ... s09_who_deals_with_danes` | Lundenwic or Hamwic, June 1001 to Oct 1002; drawn | vouch for Toke (likely) / warn Toke / give names (60 d) / "I sell wine" | `_9`; `_surety` or `_watched`, `_warned`, `_informer` |
| 10 | `harbour. ... s10_the_kings_writ` | Lundenwic, Oct to Nov 1002; drawn | bring out Gunhild (doubtful; needs `_partner`, `_surety` or `_warned`) / take Toke and his sons / keep the hatch shut | `_10`; `_saved`, `changed_brice_1002_gunhild_lives`, `_wanted`, `_toki_saved`; or `_toki_saved` (and `_watched` on a failure); or `_stood_by` |
| 11 | `harbour. ... s11_brices_morrow` | Lundenwic or Gipeswic, Nov 1002 to 1003, not `_saved`; drawn | buy the dead men's goods (100 d) / pay for graves (30 d) / carry the news | `_11`; `_blood_goods` or `_buried` |
| E1 | `harbour. ... end_north` | Sliesthorp, Nov 1002 to 1006; `_fleet` or `_toki_saved`, not `_saved` | fellowship with Toke (needs `_toki_saved`) / Asgot's ship for 300 d / pennies and a plot | `_end_north`, `_done`; `_fed_the_fleet` |
| E2 | `harbour. ... end_london` | Lundenwic, Dec 1002 to 1006; `_1`, not `_saved` | the king's writ (needs `_paid`, `_lent`, `_served` or `_ransomed`) / a dead Dane's toft (needs `_informer` or `_blood_goods`) / Leofflaed as factor (needs `_partner` or `_surety`) / the Danes' freight and the river (anyone) | `_end_london`, `_done`; `_dead_mans_ground` |
| E3 | `harbour. ... end_saved` | Sliesthorp 14 days after step 10, or Sliesthorp or Ripa to 1008; `_saved` | the king's guest and a free keel / a ship and a partner / only the merchants' peace | `_end_saved`, `changed_brice_1002_gunhild_lives`, `_done`; `_peace_asked` |

**Roads through.** The English road: 1 (pay), 2 (ransom), 3, 4 (carry), 5 (deliver), 9, 10 (stay), 11, E2 with the writ. The silver road: 1, 4, 5 (sell), 6, 7 (sell), 11, E1. Toke's friend: 5, 7, 8 (shares or surety), 9 (vouch or warn), 10 (Toke), then E1 with the fellowship or E2 with Leofflaed. The informer: 9 (names), 11 (buy), E2 with the toft. The hard road: 8 or 9 for Toke's trust, 10 (Gunhild), then E3 if the door opens, or E1 if it does not. The bystander: 1, 11, and E2's open option.

**No dead ends.** Only step 1 (or step 4, which sets it) is needed for step 11 and for ending 2, whose last option is open to anyone; every other step can be skipped or missed. Every card has an option that costs only time or pride (ask; walk on; hear the song; ask what the army lacks; sit by the fire; heave to; weigh a penny; eat and sign nothing; "I sell wine"; keep the hatch shut; carry the news; and one open option at each ending). A failed chance hurts and goes on: the duke's man laughs and the rope goes up the hill; the longship overhauls the boat and takes twenty pennies for a joke; the reeve writes the player's name under Toke's; the guest-house door stays shut, at the cost of 120 d and a cut scalp, and Toke is still got away and ending 1 opens. Each danger warns first: Wulfgyth names Rouen, the lighterman names the fleet's thirst, the alewife says to be gone before the dark evenings, the reeve shows his wax board a year ahead, and the priest comes two days before the bells.

**If the king's load was sold.** A player who takes the flour and ale at step 4 and sells them cannot choose "hand over" at step 5; he has had about 140 d of the king's goods and the reeve's feeling stays as it was. No card punishes it yet; a later card may read `_victual` without `_served`.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 (or costs 40 d) | half toll for 60 days; or a chalice worth 240 d for 150 d; or the news |
| Step | 2 | 0 | a passenger and a friend; or 350 d of plunder for 80 d |
| Turn | 3 | 100 d and 8 bolts of woollen cloth (144 d) | half toll at Gipeswic for 120 days; or the poem of Maldon; or latin +0.1 and half toll for 180 days for a loan of 100 d |
| Step | 4 | 0 | 140 d of the king's food on trust; or half toll at Lundenwic for 90 days; or two true price words (ale x1.8, wine x1.6 at Hamwic) |
| Turn | 5 | 160 d, or 220 d | deed kept_commission; or wine at double at Hamwic for 90 days; or half the Danish tongue and a true price word |
| Step | 6 | 70 d for a 10 d cask | or two true price words for Sliesthorp |
| Turn | 7 | 160 d | or a commission of 8 casks of Rhenish wine at +80% (about 380 d over the market); or the news and a price word |
| Turn | 8 | costs 120 d | a commission of 10 casks of Frankish wine at +60% (about 480 d over the market at London's price); or half toll for 90 days |
| Step | 9 | 60 d | half toll for 120 days (the informer); or Toke's and the reeve's feeling |
| Step | 10 | 0 (or a fine of 120 d) | a king's sister, or a friend, for a passenger |
| Turn | 11 | costs 100 d | 360 d of ivory, cloth and brooches; or deed gave_alms |
| Finale E1, honest road | Sliesthorp | 700 d | `unlock_partner_toke` and `unlock_factor_sliesthorp` |
| Finale E1 | | costs 300 d | `unlock_ship_norse_allround_ship` (worth several thousand) |
| Finale E1 | | 900 d | `unlock_plot_sliesthorp` |
| Finale E2, the English road | Lundenwic | 800 d | `unlock_charter_lundenwic` |
| Finale E2, the informer | | 600 d | `unlock_plot_lundenwic` |
| Finale E2, Toke's friend | | 700 d | `unlock_factor_lundenwic` |
| Finale E2, anyone | | 600 d and a commission of 12 casks of Frankish wine at +60% (about 570 d over the market) | `unlock_pilot_thames` |
| Finale E3, hard road | Sliesthorp or Ripa | 1,500 d | `unlock_title_sweins_guest` and `unlock_charter_sliesthorp` |
| Finale E3, hard road | | 1,000 d | `unlock_ship_norse_allround_ship` and `unlock_partner_toke` |
| Finale E3, hard road | | 800 d | `unlock_title_sweins_guest` and `unlock_heirloom_gunhilds_ring` |

**Why a merchant could have won these.** A sworn partner with a second boat is the *felag* of the Danish rune stones of these very years. A plot by the jetty at Hedeby is what a foreign house held there; a factor is the old man who keeps the key. A ship built for a son who will not sail her, sold for the price of her timber, is how a sea-going trader comes within a small house's reach. At London, IV Aethelred already says the emperor's men trade like Londoners; a writ naming one house is a short step, though the writs that survive from these years are for churches (?). A forfeited toft was the king's to give through his reeve. A wife left holding a foreign husband's storehouse is a factor in all but name. The Thames is learned by being shown its shoals at low water by a man who knows them. A king's guest-right for a merchant is as old as Rimbert's Birka; Swein's son wrote in 1027 of the tolls his merchants should not pay. A ring from a king's sister is the ordinary gift for a life.

**Costs that last.** `_wanted` (step 10, success): the house took a king's hostage out of London; English reeves know it until 1013, when it becomes a merit. `_watched` (steps 9 and 10): the player's name is under a Dane's on the reeve's board. `_informer` (step 9): Toke's kin and Leofflaed know who gave names. `_blood_goods` (step 11): the brooches can be read at Hedeby, and Asgot says so. `_dead_mans_ground` (ending 2, the toft): Danes rule London from 1013 and 1016, and the plot has an owner's kin. `_fed_the_fleet` (ending 1, the plain option): the reverse, on the English shore. `_loot` (step 2): the altar cross has a saint's name under its foot.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_maldon_991.ending.*`) |
|---|---|---|---|---|
| What the silver bought | `_end_north` | followed the silver, or carried Toke over the sea | Saint Brice's day happens as it did; Swein readies his fleet with a reason he can say aloud; the player is paid at Hedeby in a fellowship, a ship or a plot | `north`: a bag of English pennies spilled beside a Danish balance |
| An empty hythe | `_end_london` | stood with the English, informed, or stood by | the same year from the other shore: no Danish keel at Billingsgate, Swein coming, and a reeve who needs boats pays in a writ, a toft, a factor or the river | `london`: empty mooring posts, one Frisian boat at the last |
| The king's sister | `_end_saved`, `changed_brice_1002_gunhild_lives` | won Toke's trust, tried the guest-house door on the night of the writ, and got her down river | Gunhild and her son reach her brother alive; the player is King Swein's guest and an outlaw in England | `saved`: a woman's and a child's hand on a ship's rail |

**What really happened** is endings 1 and 2: the same November seen from the two shores. Toke lives or dies by the player's choice at step 10, but he is ours and history does not notice.

**History changed, and what follows** (`changed_brice_1002_gunhild_lives`).

- The killing of Saint Brice's day still happens: the player has taken three or four people out of London, not stopped a king's writ. `chronicle_793.json` 1002-11-13-st-brice stands, and so does Oxford.
- Swein still comes in 1003. He came for tribute in 994 before he had any grievance, and the chronicle's entries for Exeter, Wilton, Norwich and Thetford stand. But he comes for silver and not for a sister's blood: William of Malmesbury's story of her prophecy never arises, and later cards may make his army readier to be paid and go (?). A writer of the years 1003 to 1013 might let a town that pays early be left unburnt when this flag is set.
- Gunhild lives at her brother's court and is there to be met at Sliesthorp or Hleidr. Her son is a nephew of the king and a cousin of Cnut, and could be an earl in England after 1016 (?), with a debt to one merchant house.
- The house holds `unlock_title_sweins_guest`. It should open Danish kings' cards from now on, be worth a great deal in London after Christmas 1013, and be read by `cnut_1016`: Cnut is Gunhild's nephew.
- Until then the house is under `_wanted` in England: a reeve who knows the boat may seize her. A later card should let the player buy that off, or wait for a Danish king.
- If the player chose `_peace_asked`, Swein's steersmen have been told to keep the peace sworn in 994 for merchant ships in river mouths: a sea card of 1003 to 1013 may let a raider of his fleet pass the player by.

## Needs (for the integrator; nothing here was added to a shared table)

- Grand reward flags new to this chain: `unlock_partner_toke`, `unlock_pilot_thames`, `unlock_title_sweins_guest`, `unlock_heirloom_gunhilds_ring`. (`unlock_factor_sliesthorp`, `unlock_plot_sliesthorp`, `unlock_charter_sliesthorp`, `unlock_ship_norse_allround_ship`, `unlock_charter_lundenwic`, `unlock_plot_lundenwic` and `unlock_factor_lundenwic` are already set by other chains.)
- The history flag `changed_brice_1002_gunhild_lives`, for the chains `cnut_1016` and `year_1066` and any card of 1003 to 1013.
- The spine's shared flags `en_paid_share` and `en_followed_silver` are set here as the spine defines them. `long.maldon.first_tribute` sets `long_follows_tribute`; step 5 reads it for one line.
- `lore/law_tolls.md` 7.3 proposes a flag `kings_peace` on the ship from 994 in English waters. This chain speaks of the peace and does not set the flag: it is the engine's to give to every merchant ship, not one card's.
- Port ids keep their 793 names. In these years Lundenwic is the walled city with its hythe at Billingsgate, Hamwic is Southampton, Sliesthorp is Hedeby, Rotomagus is Rouen, and Sarre stands for Sandwich. Maldon and Oxford are not ports in `ports_793.json` (`later_ports/england_wales.md` proposes `maeldun`); if they are added, step 1 could also be drawn at Maldon.
- The commission verb takes its port from the call, so Asgot's wine is delivered at Sliesthorp and Toke's and the reeve's at Lundenwic.
- No deed fits "ransomed captives" (the chain uses `gave_alms`), "informed on neighbours" or "bought forfeited goods" (flags `_informer`, `_blood_goods`). `fled_kings_men` is used for carrying off the hostage.
- Gipeswic's places file has a moneyer, Leofsige, and a story of the raid of 991 with its own flags; this chain does not read them. Eadnoth the reeve, Wulfgyth and Leofric could be added to its people, and Toke, Leofflaed and Wulfmund to London's for 994 to 1006, Asgot to Sliesthorp's.
- `chronicle_793.json` 991-08-maldon lists the port dofras; the fleet touched Folkestone and Sandwich (`realms/england_south.md` already notes it). The chain uses Sarre.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | A grey strand below a half-burnt town: a reeve at a trestle with tally sticks and pennies; a woman in a fuller's apron pointing out to sea |
| s02 | The quay at Rouen, bright and cold: eleven people on one rope, folded cloth with a silver cross on top, a seller with an arm ring, a Norman steward looking away |
| s03 | An ale-house by firelight: a grey man singing with his eyes shut, a harp on his knee; a priest waiting by the door; new thatch outside |
| s04 | London's river wall at low tide: scorched Roman stone, the ribs of a burnt longship on the mud, a reeve talking down to a skipper |
| s05 | A river bank at winter dusk, light snow: longships under tilts, men dicing for heaps of pennies, a young Danish trader by a fire with a cross and a hammer on one cord |
| s06 | Open sea, broken sun: a deep-laden longship under a striped sail running down on a small boat, her steersman shouting, men laughing |
| s07 | The south jetty at Hedeby: an old merchant in his doorway; a moneyer holding up one bright penny; young men with new swords at a stall |
| s08 | Billingsgate on a busy morning: a reeve calling the toll; a new timber house, a Dane with a child on his arm, his English wife with a tally |
| s09 | The hythe in the wet: the reeve leaning over a boat's side, a clerk with a wax board; an alewife in her door; a Dane standing very still among his bales |
| s10 | Under a boat's tilt at night, one lantern: a priest bent under the canvas, the skipper on a cask, a Dane's face in the gap; the dark river |
| s11 | The hythe on a still November morning: goods on cloths with nobody behind them, a balance, cloaks, a child's shoes; the reeve; a woman at the edge |
| end_north | Hedeby in hard frost: hulls being tarred on the shore; a table in a doorway, an old merchant with a bag of pennies and an empty stool |
| end_london | Billingsgate in thin spring sun: three boats where thirty lay, the reeve alone, a row of empty mooring posts |
| end_saved | Hedeby's jetty on a cold bright morning: a tall woman leading a boy up a gangplank; a king's man, twelve armed men and a led horse |

## How long it takes

The chain is held to its true dates, so it is the longest kind: eleven years from the first tribute to Saint Brice's day, and it may well pass from one head of the house to the next. It comes in three bursts with long quiet between them. The first is the autumn of 991 and the year after: Ipswich, Rouen twelve days later, Ipswich again ten days after that. The second is the winter of 994 to 995: London in September, the camp at Southampton a week on. Then the years of trading into the silver, 995 to 1001, with one card at Hedeby, one at Billingsgate and perhaps one at sea. The third begins in the summer of 1001 with the reeve's wax board, comes to its night in October or November 1002, and is paid from December 1002 (at Hedeby from November; ending 3 fourteen days after the night). The endings stay open to the end of 1006, ending 3 to 1008. For most of those years the chain is a handful of flags, three commissions, a friend at Billingsgate and the question of which shore the house is known on.
