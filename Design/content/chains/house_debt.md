# Chain: house_debt (the count's 480 pennies)

Written 4 Oct 2026. Cards: `Assets/Kapman/Game/Data/encounters/chain_house_debt_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_house_debt.json`. A draft for the user to overwrite.
(?) marks what could not be checked against a source tonight. LATER marks a thing attested only after its year here.

## The story in five sentences

The boat was bought on the count's silver: two pounds, 480 pennies, cut on a split hazel stick that his steward holds, and due at Michaelmas.
The king's law lets the count ask back no more than he lent, so a late debtor pays in other coin: the boat's work, a pledge, a gift, or himself.
Three others would like to hold the stick: the rival skipper Thiadbod, who buys men's tallies to own the men; his sister the widow Thiadburg, who lends on goods; and the bishop of Utrecht, whose men on Dorestad's banks owe the count nothing.
In 794 the king's new heavy penny makes the same 480 pennies near a third more silver, and count and debtor each read the stick their own way.
The house ends as the count's friend with ground on the king's bank, as his enemy under the king's word or behind the bishop's wall, as Saint Martin's man free of the toll, or as Thiadbod's second boat.

## How a player meets it

It is the game's opening. Step 1 is drawn on the first call at Dorestad of a new game (21 June 793) with a weight of 40, far above an ordinary card's, so that it is all but sure to be the first card; it stays open to the end of 795 for a player who sails off first. Every later step is scheduled by the one before it, or (steps 8 to 10) drawn at its own port once the tally is closed. Two items go on Dorestad's board on the way and tell the player where to go next: wheat wanted before the frost, and hulls wanted at Daventre for the host.

## The people (spine cast)

| In the cards | Spine id | Who |
|---|---|---|
| the count | cast.folcbert | Count over the Dorestad district; invented (no count there is named in the 790s). Calls every Frisian "shipman". |
| the steward | cast.hartwin | Keeps the tallies; counts in dozens and scores; invented. |
| the rival | cast.thiadbod | The skipper with the best dam head; buys tallies; invented. |
| the widow | cast.thiadburg | His elder sister; lends on goods; invented. |
| the moneyer | cast.sigimar | Of the king's mint at Dorestad; invented. |
| the bishop | cast.hamacar | Bishop of Utrecht about 790 to 806: the one historical person. Only his name and years are known, so every word he says is invented. |
| others | none | The priest of Saint Martin's church above Dorestad, the count's man at Daventre, the king's man and the bishop's clerk are invented and unnamed; Sibrad, who rows for Thiadbod, is an invented name (Sib- + -rad). |

## The real history under it

1. **The debt is the game's own.** 480 d for the boat, owed to a Frankish count and due at Michaelmas: `trade_rules.json` start, `Design/DECISIONS.md` no. 10, spine thread `counts_day`. No count, reeve or toll man of Dorestad is named for 793; the attested officers are the king's "procuratores rei publicae" (charter of 815, by `dossiers/dorestad.md` 1 and 8). The count's hall is put at De Geer, a ditched lord's seat with a hall 10 by 22 m found by the digs (dossier 2); that it was a count's is our guess (?).
2. **No more than was lent.** The Admonitio generalis of March 789 made the Church's ban on lending at interest the king's law for everyone (MGH Capitularia I no. 22 c. 5; German Wikipedia "Zinsverbot", read 4 Oct 2026; the clause's wording from memory (?)). The capitulary of Nijmegen, 806, defines it: usury is where more is asked back than was given (MGH Capit. I no. 46 c. 11) (?). So the count's silver comes back as the sum and a service, a gift, a pledge or a man, never as interest by name. The cards rest on this at every step.
3. **Michaelmas.** The feast of 29 September is among those the council of Mainz ordered kept in 813 (c. 36, by the spine's calendar). That it was a day for reckoning debts in the 790s is the game's rule (?).
4. **The famine of 793.** Great hunger in Francia, Burgundy, Italy and Provence, eased only by the harvest of 794 (Annales Laureshamenses 793, Chronicon Moissiacense 793, Annales Mosellani: `chronicle_793.json` 793-great-famine). At Frankfurt in June 794 the king fixed the price of the public measure (oats 1 d, barley 2, rye 3, wheat 4) and told those who held his land to see that none of their people starved (MGH Capit. I no. 28 c. 4; the prices read 4 Oct 2026 in the Fordham sourcebook, "Capitulary of Frankfort: The Price of Staples"; chronicle 794-06-01-frankfurt-prices). Step 4 falls in November 793, before that law: the price there is the count's own, and six and twelve pennies the sack are the game's.
5. **The loss on the Weser.** Count Theoderic's force was destroyed by Saxons in Rustringen, and the king gave up his march on the Avars (Annales regni Francorum, revised, 793; chronicle 793-summer-rustringen-defeat). The count's line in step 3 is that news; that he was near it is invented.
6. **The king's new penny.** Frankfurt, June 794, c. 5: the new denarii are to pass and be taken in every place, town and market, and a free man who refuses them pays fifteen shillings (MGH Capit. I no. 28; read 4 Oct 2026 in the Fordham sourcebook, "Monetary Regulations of the Carolingians"; chronicle 794-06-frankfurt-new-denier). The Dorestad mint struck a light penny of about 1.3 g until 793/4 and the monogram penny of about 1.7 g after (`dossiers/dorestad.md` 4). So 480 old pennies weigh what 367 new ones weigh (480 x 1.3 / 1.7). Whether a debt cut in old pennies was owed by tale or by weight no source says: the quarrel in step 5 is our own (?), built on a true change. The game's penny is one unit throughout; the weight reading is priced at 367 d.
7. **The host.** Two armies into Saxony in autumn 794; to the Elbe at Bardowick in 795 with more hostages than before; in 797 the king's ships hauled over land and by river into Saxony (Annales regni Francorum 794, 795; Annales Guelferbytani 797: chronicle 794-autumn-sintfeld, 795-summer-bardowick-campaign, 797-summer-ships-into-saxony). Daventre lies on the host's road east (spine thread `saxon_end`; `dossiers/daventre.md` 1). A penny a load is the game's.
8. **Saint Martin's men.** The church of Utrecht held Saint Martin's church above Dorestad with its bank from 8 June 777 (Oorkondenboek van het Sticht Utrecht I no. 48), and on 18 March 815 Louis the Pious confirmed that its men on the banks at Dorestad owed the king's officers nothing, that nobody might lodge in their houses or take their goods at death (no. 56; both by `dossiers/dorestad.md` 2, 4, 5). That this already held in 794 is assumed (?). On 24 June 896 King Zwentibold gave Utrecht's men at Deventer and Tiel the freedom they had had "in Dorestadio" (no. 88).
9. **Nobody else was free of the tenth.** In 828 the emperor's own merchants, free of toll everywhere else, still paid the tenth at Quentovic, Dorestad and the Alpine passes (Praeceptum negotiatorum, MGH Formulae imperiales no. 37; by the dossier and `peoples/frankish.md`, not read tonight (?)). The same writing forbids anyone to take those merchants' ships for the king's service: in the 790s a merchant had no such shield, which is why the count's man can keep the boat in step 8.
10. **The king's man.** Charles to Offa, 796: "We will, too, that traders have due protection within our kingdom, according to our mandate, and if in any place they suffer wrongful oppression, let them appeal to us or our judges, and we will see full justice done" (in E. P. Cheyney, Readings in English History, Boston 1922, p. 56, read 4 Oct 2026 at the Fordham sourcebook; chronicle 796-spring-offa-letter). The letter is about English traders in Francia; the card's king's man says it of traders at large, in our own words. In 802 the emperor sent great men through the whole kingdom to do justice "to the holy churches of God, or the poor, or wards and widows, or the whole people", none to be "kept back from the right path of justice by the adulation or the reward of any man, by the obstacle of any relationship, or by the fear of powerful persons", and every man from twelve years up swore fidelity (Capitulare missorum generale c. 1-2, tr. E. F. Henderson, Select Historical Documents of the Middle Ages, 1896, read 4 Oct 2026 at the Avalon Project; chronicle 802-spring-oath-of-802). The Lorsch annals say that in 802 the king no longer wished to send poorer vassals of the palace to do justice, because of gifts, and chose men who had no need of them (Annales Laureshamenses 802, from memory (?)): so before 802 the complaint in step 10 is only likely to be heard, and from 802 it is sure. An earlier general oath, after the plot of 792, is MGH Capit. I no. 25 (?). No envoy to the Dorestad district is recorded: the visit is invented.
11. **Smaller things.** The king's ban of sixty shillings (Lex Ribuaria 65; Capitulare Saxonicum 797) (?). Twofold amends: the Lex Frisionum has a thief pay double (title III) (?). Seven judgment-finders in a count's court (`peoples/frankish.md` 3; a Frisian merchant among them (?)). Falcons as gifts between the great: Boniface sent a hawk and two falcons to King Aethelbald of Mercia about 745, and King Aethelberht of Kent asked him for two more (Boniface's letters, Tangl nos. 69 and 105) (?). The bolt of silk: the Avar treasure, shared by the king among his great men in 796 (Annales regni Francorum 796; Einhard, Vita Karoli c. 13; chronicle 796-winter-avar-ring-plundered). A hen as a yearly due: the polyptych of Irminon (`peoples/frankish.md` 2). Hands between hands: Tassilo commends himself so in 757 (Annales regni Francorum) (?). The ports of Charles and Offa shut to each other's merchants about 789 to 796 (spine thread `black_stones`): why the rival sends another man's boat for English cloth.
12. **The game's own devices.** The split tally stick is recorded only from about 1100 (English Wikipedia "Tally stick", read 4 Oct 2026): LATER, the game's device, as the spine says. The widow's third of three cargoes follows the shape of the southern commenda, thin in the north (`Design/economy_and_trade.md` 2.9) (?). A sworn partnership by that name (felag) is Norse and later (?).

## The steps

Card ids are `harbour.chain_house_debt.<step>`. Every card has an option that costs only time or pride. Progress flags are `ch_house_debt_<n>` with a letter for the branch; the flags with words in them are the ones later checks read.

| Step | Where and when | What happens and the choices | Flags set |
|---|---|---|---|
| s01_tally | Dorestad, the first call (21 June 793); drawn, weight 40 | The steward hands over half the split stick: 480 d, due at Michaelmas. Take it; leave twelve pennies for the count's table (12 d); or argue the price. All three schedule steps 2 and 3. | `ch_house_debt_1`, `ch_house_debt_owed = 480`, `ch_house_debt_gift` |
| s02_rival | Dorestad, 20 days later | Thiadbod offers an order for English cloth and his good word; the widow warns that he buys tallies. Take the order (he is obliged, she cools); decline; or ask him to his face whose sticks he holds (he cools, she warms). | `ch_house_debt_2`, `_2a` took the order, `_2b` declined, `_2c` asked |
| s03_michaelmas | Dorestad, from Michaelmas 793 (100 days after step 1); "on the day" lasts to the end of October | The reckoning before the count, who also asks for wheat before the frost. Pay 480 d on the day; pay 480 d late; ask a year for the boat's work (only in 793, standing known or better); or pledge the boat. | `ch_house_debt_3`; `_3a` paid (`ch_house_debt_paid_on_the_day` if on the day, `ch_house_debt_cleared`, `ch_house_debt_long`); `_3b` grace (`db_extension`, `db_service`); `_3c` pledge (`ch_house_debt_pledged`) |
| s04_bread | Dorestad, 45 days after step 3 (about Martinmas 793) | The hungry winter. With four sacks of wheat aboard: sell them to the count at 6 d, on the dams at 12 d, or carry two to the church door and sell him two. Or have none to spare. A side step for every road. | `ch_house_debt_4`; `_4a` count, `_4b` dams, `_4c` church door, `_4d` none; `ch_house_debt_fed_his_people` (4a, 4c) |
| s05_new_penny | Dorestad, 290 days after step 3 (July 794); only for those who still owe | The king's new penny. Pay by tale (480 d); pay by the moneyer's weight (367 d); or answer at Michaelmas. | `ch_house_debt_5`; `_5a` tale (`ch_house_debt_tale_taken`), `_5b` weight, `_5c` waits; paying sets `ch_house_debt_cleared`, `ch_house_debt_long` |
| s06_bid | Dorestad, 80 days after step 5 (Michaelmas 794); comes again if the bishop gives nothing | Thiadbod offers the count the full tale for the stick. Match it (480 d); take the widow's surety (120 d and a third of three cargoes, if she is easy with you); go to the bishop (once); or let it be sold (ending: sold). | `ch_house_debt_6`; `_6a` matched (`ch_house_debt_tale_taken`), `_6b` surety (`db_surety`, `ch_house_debt_widow_share`), `_6c` bishop, `_6d` sold (`db_bought`) |
| s07_bishop | Traiectum, a day after step 6 | The bishop of Utrecht. Kneel and be Saint Martin's man (ending: Saint Martin); take his letter on usury and pay the weight through his clerk (367 d); or go back with nothing, to step 6 again. | `ch_house_debt_7`; `_7a` knelt, `_7b` letter (`ch_house_debt_bishops_letter`, `db_surety`, cleared, long), `_7c` back |
| s08_host | Daventre, April to August, 794 to 799; drawn, on the long road | Stores for the host. Carry them twelve days (likely; if the water falls the boat is kept unpaid); take an order for salt pork instead; or refuse. | `ch_house_debt_8`; `_8a` served (`ch_house_debt_served_host`; on failure `ch_house_debt_pressed`), `_8b` supplied, `_8c` refused |
| s09_hawk | Dorestad, any call with a falcon aboard, to 805; drawn, on the long road | The hawk the count has wanted since step 1. Give her for the king; sell her at the dam's price; or keep her (the card may come again). | `ch_house_debt_9`; `_9a` given (`ch_house_debt_hawk`), `_9b` sold |
| s10_envoys | Dorestad, April to October, 796 to 805; drawn, on the long road | The king's man takes the oath and hears wrongs. Speak for the count (ending: friend); or tell how sticks are sold in his hall (ending: enemy; before 802 only likely to be heard). | `ch_house_debt_10`; `ch_house_debt_spoke_for`; `ch_house_debt_judged` or `ch_house_debt_unheard` |
| s11_friend | Dorestad, at once after step 10 | Ending: the count's friendship. Three finales (below). | `ch_house_debt_end_friend` |
| s12_enemy | Dorestad, at once after step 10 | Ending: the count's enmity. Three finales (below). | `ch_house_debt_end_enemy`, `ch_house_debt_count_enemy` |
| s13_st_martin | Traiectum, at once after step 7 | Ending: Saint Martin's man. Two finales (below). | `ch_house_debt_end_st_martin`, `ch_house_debt_st_martins_man` |
| s14_sold | Dorestad, at once after step 6 | Ending: sold to Thiadbod. Two finales (below). | `ch_house_debt_end_sold`, `ch_house_debt_thiadbods_man` |

The roads through:

- **Paid on the day** (the hard and honest road): 1, 2, 3 (pay), 4, then the long road 8, 9, 10 and ending 11 or 12. Eight or nine cards.
- **Paid later**: 1, 2, 3 (grace or pledge), 4, 5 (by tale or by weight) or 6 (matched, or the widow's surety) or 7 (the bishop's letter), then the long road and ending 11 or 12. Up to twelve cards.
- **Saint Martin**: 1, 2, 3, 4, 5, 6, 7, 13. Eight cards.
- **Sold**: 1, 2, 3, 4, 5, 6, (7, 6 again,) 14. Seven to nine cards.

No dead ends: a man with no silver at all can always pledge (step 3), wait (5), go to the bishop or let the stick be sold (6), and each of those ends in a finale. The dangers warn first: the steward says not to be late (1), the widow says what her brother does with tallies (2), the count asks for corn a season ahead (3), the count's man says the boat may be kept (8), the steward says what kind of man the king has sent (10).

## The endings

| Ending | Flag | How | What becomes of the people and the place | Grand reward |
|---|---|---|---|---|
| The count's friendship | `ch_house_debt_end_friend` | The tally closed, and before the king's man you said the count dealt justly. | The count keeps his county and owes you what he cannot pay in pennies. Your boat lies on the king's bank between Thiadbod's dam head and the widow's, for a hen at Michaelmas; or you sit with the seven who find judgment in his court. The steward burns the stick. | `unlock_plot_dorestad`, or `unlock_title_doomsman_dorestad` |
| The same, by the hard road | `ch_house_debt_end_friend` and `ch_house_debt_end_friend_hard` | Paid on the day in 793, and the hawk given for the king. | The count carries your asking to the king, and the house gets what no layman had: the freedom of the tenth at Dorestad. **History changed.** | `unlock_plot_dorestad` and `unlock_charter_dorestad`; `changed_dorestad_toll_charter` |
| The count's enmity, heard | `ch_house_debt_end_enemy` | You told the king's man how sticks are sold in the hall, and he judged it. | Every bought stick is burned, Sibrad among the freed; the count pays the king's ban and keeps his county; the steward takes the blame; the count holds the toll, the court and the levy against you from now on (`ch_house_debt_count_enemy`). You stand under the king's word. | `unlock_title_kings_protection` |
| The count's enmity, unheard | `ch_house_debt_end_enemy` | The same complaint, before 802, to a king's man who rode home with one of the count's horses. | Nothing is found against the count. The bishop of Utrecht, who has waited for such a complaint, takes you in: you trade from behind his wall. | `unlock_factor_traiectum` |
| Freedom bought another way | `ch_house_debt_end_st_martin` | At Utrecht you put your hands between the bishop's. | The church pays the count, who cannot refuse it. You have a house-place on Saint Martin's bank above Dorestad; no officer lodges there and none takes the tenth; you carry wax and wine when asked and answer in the bishop's hall (`ch_house_debt_st_martins_man`). Count and bishop have one more quarrel. **History as it was**: these are the men the charter of 815 names. | `unlock_charter_dorestad` |
| Sold | `ch_house_debt_end_sold` | You let the count sell the stick to Thiadbod. | The stick hangs in his storehouse between Sibrad's and a Dane's. You lie at his dam head and carry what he loads; a third of what the boat earns is his (`ch_house_debt_thiadbods_man`), fixed for good if the widow stood witness (`ch_house_debt_terms_witnessed`). | `unlock_partner_thiadbod` |

**What is history and what is changed.** The chain's creditor is invented, so nothing in the annals turns on it. Its fixed points are the laws: the usury ban of 789, Frankfurt 794, the tenth at Dorestad. The Saint Martin ending is what the sources show: the only men at Dorestad known to be free of the king's dues are the church's (815), and their freedom is carried to Deventer and Tiel when Dorestad is gone (896). The hard road's charter is history changed: no lay house is known to have been free of the tenth there, and in 828 even the emperor's own merchants paid it. If the flag `changed_dorestad_toll_charter` is set, later cards may follow it: in 814 the new emperor must confirm the writing or it lapses (the engine's `succession` tag); in 828 the Praeceptum would have to name one Frisian house as the exception, and other houses will ask why; in 850 Rorik, who holds Dorestad on condition of handling the king's dues, must honour the charter or break it; after 863 the house carries its freedom to Tiel or Deventer as Utrecht's men carried theirs.

**Later years on the other roads.** Friend: the hen is due each Michaelmas; the plot stands until the raids of 834 to 863 (chain `dorestad_834`). Enemy: the count lives to about 812 in the spine and his successor inherits half the grudge. Saint Martin: Hamacar dies about 806 and Bishop Ricfried inherits his men; the flag `ch_house_debt_st_martins_man` is the hook for a yearly carrying. Sold: the house can buy the stick back, or the rival chain (`house_rival`) can end the partnership in a quarrel; the widow's share (`ch_house_debt_widow_share`) is a hook for the spine thread `york_blood`, where she backs a skipper who will carry a blood-price to York.

## Every payout

| Step | Option | Silver | And |
|---|---|---|---|
| s01 | take / gift / argue | 0 / -12 / 0 | a true price word on Dorestad's board (wheat is wanted before the frost); the steward's feeling +2 / +6 / -4 |
| s02 | take the order | 0 | an order: 6 bolts of woollen cloth in 60 days at 30% over the price (about 140 d); the rival +6, the widow -3 |
| s02 | decline / ask | 0 | the widow +3 / +8 (her surety in step 6 needs her easy or better) |
| s03 | pay on the day | -480 | the debt closed; the deed `kept_commission`; the count +15; half toll at Dorestad for 90 days; the news of the host |
| s03 | pay late | -480 | the debt closed; the count +5 |
| s03 | grace / pledge | 0 | a year's delay; the count +2 / -6 |
| s04 | to the count | +24 | his people fed; the steward +10; half toll for 60 days |
| s04 | to the dams | +48 | the steward -8, the priest -4 |
| s04 | to the church door | +12 | the deed `gave_alms`; the priest +12; the crew's heart up; his people fed |
| s05 | by tale | -480 | the debt closed; the count +12; a true grievance kept for step 12 |
| s05 | by weight | -367 | the debt closed for 113 d less; the count -6; the moneyer's riddle of money (`riddle_symphosius_91_pecunia`) |
| s06 | match the bid | -480 | the debt closed; the rival -10; the grievance |
| s06 | the widow's surety | -120 | the debt closed for 360 d less; a third of three cargoes owed her (flag) |
| s07 | the letter | -367 | the debt closed; the deed `carried_letters`; the bishop +6 |
| s08 (turn) | carry the stores | +120 (or +50 if the water falls) | an order for 4 barrels of salt pork at 50% over the price for next year's host; the count's service done |
| s08 | take the order | 0 | the same order, in 40 days |
| s09 (turn) | give the hawk | +120 | half toll at Dorestad for 180 days; the count +20; the hard road's finale opened |
| s09 | sell the hawk | +200 | the count +3 |
| s11 (finale) | the ground | 0 | `unlock_plot_dorestad`; a bolt of silk from the Avar spoil (about 360 d); an order for 12 casks of wine at 80% over the price (about 840 d, some 500 d of it gain); half toll for a year |
| s11 (finale) | the seat | 0 | `unlock_title_doomsman_dorestad`; the silk; the same order; half toll for a year |
| s11 (finale, hard road) | the charter | 0 | `unlock_plot_dorestad` and `unlock_charter_dorestad`; two bolts of silk (about 720 d); an order for 16 casks at double the price (about 1,500 d, some 900 d of it gain); no toll at Dorestad from now on |
| s12 (finale) | the letter | +240 | `unlock_title_kings_protection`; orders for the palace: 10 bolts of Frisian cloth and 8 casks of wine at 60% over the price (about 1,500 d, some 580 d of it gain) |
| s12 (finale) | the letter and amends | +466 | the same, and twofold what was taken over the loan (2 x 113 d) |
| s12 (finale) | Utrecht | +300 | `unlock_factor_traiectum`; wax sells at half as much again and wine at two fifths more at Traiectum for 240 days |
| s13 (finale) | wax and wine | 0 | the debt (480 d) paid by the church; `unlock_charter_dorestad`; orders for 8 cakes of wax and 8 casks of wine at 60% over the price (about 1,280 d, some 480 d of it gain) |
| s13 (finale) | letters | 0 | the debt paid by the church; `unlock_charter_dorestad`; Latin half learned; Aldhelm's riddle of the scales on a leaf; the wax order |
| s14 (finale) | swear | +100 | the debt paid by Thiadbod; `unlock_partner_thiadbod`; 8 bolts of Frisian cloth and 3 casks of wine in the hold (about 560 d); half toll for a year |
| s14 (finale) | swear before the widow | +100 | the same with 6 bolts; the third fixed by witness |

Tiers: steps 0 to 48 d; two turns of 120 to 200 d; finales worth about 900 to 1,300 d each, and about twice that with two grand rewards by the hard road. The largest sum in the chain is the debt itself: the three roads that close it cheaply (the weight, the widow, the church or the rival) each carry a cost that lasts.

**Why a merchant could have won each.** Ground on the king's bank: the count gives the king's ground, and the banks at Dorestad were parcelled in strips (dossier 2). A seat among the judgment-finders: local free men of good name found judgment in a count's court. The king's protection: attested for the emperor's merchants a generation later (828); the letter of 796 promises traders protection and appeal. A trusted man at Utrecht: the bishop's stone fort is the one safe storehouse on the river. The freedom of the toll as Saint Martin's man: the charter of 815. A sworn partner with a dam head: Frisian merchants sailed in company. The freedom of the toll for a lay house: never recorded, and the chain says so by its flag.

## How long it takes

- Step 1 to the first reckoning: 100 days (21 June to Michaelmas 793).
- To Saint Martin's man or to Thiadbod's second boat: about 16 months (to October 794).
- To the count's friendship or enmity: about three years at the soonest (the king's man comes from spring 796), open to 805. Between Lent 794 and 796 the chain gives two turns of its own (Daventre in a summer of 794 to 799; the hawk whenever one is brought) and otherwise leaves the house to trade and to the other chains.
- Real waits: 20 days, 100 days, 45 days, 290 days, 80 days, a day's row to Utrecht, the summer at Daventre, the wait for the king's man.

## Pictures

One for each step (the same lines are in the writing file as `art.<card id>`):

1. **s01_tally.** Dawn at a dam head at Dorestad, mist on the river: a thin grey steward in a plain brown cloak, notched sticks at his belt, holds out half of a split hazel stick to a young Frisian skipper standing in a small open boat.
2. **s02_rival.** Midday on the plank bank road, bright and windy: a well-fed skipper in a long blue cloak with a silver pin stands over the small boat, smiling, one ringed hand on a wine cask; behind him a woman in a white head-cloth watches from a storehouse door, keys at her belt.
3. **s03_michaelmas.** A Michaelmas evening in a timber hall, firelight and smoke: the count in a red cloak sits behind a board, a sword across his knees; the steward fits the two halves of a hazel stick together; the skipper stands before them holding a leather purse.
4. **s04_bread.** A grey winter morning on the river bank, thin ice in the ditches: the steward with a wax tablet looks down into the open hold of the small boat; behind him thin people wait at the door of a small wooden church, where a priest stands with empty hands.
5. **s05_new_penny.** Noon inside the mint, a charcoal fire glowing: the moneyer in a leather apron holds up a pair of scales, old pennies in one pan and fewer new ones in the other; the count and the steward watch; a guard stands in the bright doorway.
6. **s06_bid.** An early autumn morning in the yard of the hall, light rain, horses saddled: the count, cloaked for the road, points at a hazel stick lying on a trestle beside a purse and a wine cask; the rival in his blue cloak stands at his elbow; the widow watches from the gate.
7. **s07_bishop.** Late afternoon inside the old stone fort at Utrecht, low sun on pale tuff walls: the bishop in a white alb and dark cloak, ring and plain curled staff, sits on a bench before a small stone church; a deacon holds a book; the skipper stands before him, cap in hand.
8. **s08_host.** A May morning on the river bank at Daventre, wet grass and a clear sky: carts, sacks and horse lines along the water; one of the count's men with a wax tablet counts loads into the small boat; a wooden church stands on a river dune behind.
9. **s09_hawk.** Evening at the dam head, calm and golden: the count himself bends over a white hawk on a perch in the stern of the small boat as the skipper draws the cloth from her; the steward stands behind with his sticks, forgotten.
10. **s10_envoys.** A summer day in the count's hall, doors open, dust in the light: a stranger in a travel-worn cloak sits in the count's seat with a clerk writing beside him; tally sticks cover the board; the count stands by the door with folded arms; free men wait in a line to swear.

One for each ending:

- **The count's friendship (s11_friend).** A Michaelmas morning on the river bank, clear and cold: the count paces out a strip of ground between two dam heads; the steward drives in a hazel peg; the skipper stands by with a live hen under one arm.
- **The count's enmity (s12_enemy).** Late afternoon in the hall, long shadows: the king's man holds out a folded, sealed leaf to the skipper; the count stands apart by the fire, half turned away; the steward kneels at the hearth, feeding hazel sticks to the flames.
- **Saint Martin's man (s13_st_martin).** Morning in the small stone church of Saint Martin at Utrecht, light from one narrow window: the skipper kneels with his hands between the bishop's; a clerk writes a name on a parchment roll; a wax candle burns on the altar.
- **Sold (s14_sold).** Dusk inside a storehouse on a dam head, a tallow lamp lit and rain at the door: a row of hazel sticks on pegs, one newly hung; the rival pours wine into two cups; the widow stands in the doorway; a gaunt oarsman looks up from a coil of rope.

## Flags

- Progress: `ch_house_debt_1` to `ch_house_debt_10`, with branch letters as in the table of steps.
- Read by later checks, named in plain words because a greyed option shows them: `ch_house_debt_paid_on_the_day`, `ch_house_debt_fed_his_people`, `ch_house_debt_served_host`, `ch_house_debt_hawk`, `ch_house_debt_tale_taken`, `ch_house_debt_pressed`, `ch_house_debt_pledged`, `ch_house_debt_bishops_letter`, `ch_house_debt_spoke_for`, `ch_house_debt_judged`, `ch_house_debt_unheard`, `ch_house_debt_long` (the tally closed and the house its own), `ch_house_debt_gift`.
- The debt itself: `ch_house_debt_owed` (480, or 0 once closed) and `ch_house_debt_cleared`.
- Endings: `ch_house_debt_end_friend` (and `ch_house_debt_end_friend_hard`), `ch_house_debt_end_enemy`, `ch_house_debt_end_st_martin`, `ch_house_debt_end_sold`.
- Costs that last: `ch_house_debt_count_enemy`, `ch_house_debt_st_martins_man`, `ch_house_debt_thiadbods_man`, `ch_house_debt_terms_witnessed`, `ch_house_debt_widow_share`.
- Grand rewards: `unlock_plot_dorestad`, `unlock_charter_dorestad`, `unlock_title_doomsman_dorestad`, `unlock_title_kings_protection`, `unlock_factor_traiectum`, `unlock_partner_thiadbod`. History changed: `changed_dorestad_toll_charter`.
- The spine's (thread `counts_day`), set at the steps they name: `db_extension` and `db_service` (step 3, grace), `db_surety` (the widow in step 6, the bishop in steps 7 and 13), `db_bought` (step 6, sold).

## Needs (not added by this writer)

1. **The debt figure.** No effect verb changes `Trade.Debt`, so the cards take silver as a cost and set `ch_house_debt_owed` and `ch_house_debt_cleared`. Wanted: a verb (`debt = 0`, `debt -n`) or the game closing its own debt when `ch_house_debt_cleared` is set; until then the top bar goes on showing 480 d owed. If the planned DebtClock is built, it and this chain must not both dun the player.
2. **Names.** `npc_roles_793.json` has no `named` rows yet, so the forge will name the fixed count, steward, moneyer, skipper and widow at Dorestad and the bishop at Traiectum as it likes. The words say "Thiadbod" and "Thiadburg"; rows for the spine's cast would make the speaker's label agree.
3. **The grand rewards and the lasting costs** are flags for the game to read. The toll card's own option reads `text.charter_toll_dorestad`, which is not in `texts_793.json`; it should also honour `unlock_charter_dorestad`.
4. **A deed for a debt paid on its day** (the nearest, `kept_commission`, is used) and one for a day broken.
5. **An order at another port.** `commission` posts at the card's own port, so the bishop's wants in the unheard ending are price shocks at Traiectum instead.
6. **The creditor's name.** `creditor.count` (trade_rules.json) has no writing entry; "the count" or "Count Folcbert" would fit. Left alone: it is not this file's key.
7. **A place to find step 1.** The Dorestad places file could put `card:harbour.chain_house_debt.s01_tally` on the dam heads, so that the start does not rest on a draw alone.
8. **No day of the month** in the conditions: "on the day" is to the end of October 793.
