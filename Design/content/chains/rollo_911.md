# Chain: rollo_911 (the Seine in the hands of the Northmen, and Rollo made lord at Rouen)

Cards: `Assets/Kapman/Game/Data/encounters/chain_rollo_911_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_rollo_911.json` (197 keys: every key the cards use, an outcome text for every option, 3 news items, 14 picture lines).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source. No web search was used: the page rests on `Design/content/dossiers/rotomagus.md`, `realms/francia.md`, the spine and the chronicle, and on memory where it says so.

## The story in five sentences

About 904 to 910 a skipper on the Channel says that nobody sails to Rouen, and that the three boats that do are paid like bishops: the town has been the Northmen's camp for a lifetime, its market is grass, and an army with church silver buys bread.
At Rotomagus two men want the boat, Ketil, who buys for the camp, and Gozbert, a canon of the archbishop who never left; a letter goes up to Count Robert at Paris, the count's man pays for a count of ships, and the steward asks for blades and horses, which the king's law hangs a man for selling.
On 20 July 911 the Northmen are beaten before Chartres, and the player chooses whom to help while they are weak: feed the camp, carry the archbishop's clerk to the king's people, or go up to Paris and tell the count that they can be starved out.
On the true roads the king grants the city at a ford on the Epte, Rollo is baptised in 912 and wears the white for seven days, his law hangs thieves and those who knew, and new converts go back to their gods until the archbishop writes to Reims to ask what a church does with such men.
In 918 it ends one of three ways: a length of the rope on the busiest strand on the Channel, or the saint of Rouen brought home with the player's boat sounding ahead of his barge, or (history changed) a river barred at Pîtres, the fleet gone to another coast, and Rouen left a wall with nobody behind it.

## How a player meets it

- **The hook** (`s00_word`) is drawn on a call at Quentovic, Parisius, Lundenwic or Hamwic in the years 904 to 910 (weight 0.8). It gives a true price word for Rotomagus and sets `ch_rollo_911_0`. Refusing costs nothing and the skipper tells it again.
- **The start proper** (`s01_ruined_market`) is drawn on any call at Rotomagus in 904 to 910 (weight 40), whether or not the hook was heard.
- Steps 2 to 4 are before 911 and are extra: they pay, and they set the side flags that open or close the hard roads, but **the fork (`s05`) needs only step 0 or step 1**, so a house that heard the word in 910 and reaches Rouen in 912 is still in the story.
- A house that first comes to the Seine after 913 has missed it. The market it finds is the one the chain's first ending describes.
- All cards but the hook lie at Rotomagus or Parisius, so the chain is played by a house that trades the Seine: wine down, grain and salt up.

## The real history under it

**The Northmen on the Seine.** Rouen was burned on 14 May 841 (Annales Bertiniani; `chronicle_793.json` 841-05-rouen-sacked). Fleets wintered on the river in the 850s; King Charles the Bald barred it with a fortified bridge at Pîtres, 862 to 866, and on 25 June 864 his edict there laid death on whoever gave or sold arms, a byrnie or a horse to the Northmen (Edict of Pîtres c. 25 (?); `chronicle_793.json` 864-06-25-pitres; `dossiers/rotomagus.md` 5 and 6). On 25 July 885 the Northmen entered Rouen "with their whole army" (Annales Vedastini) and went on to besiege Paris for eleven months, where Count Odo held the city (`chronicle_793.json` 885-11-24-siege-paris). After that the lower Seine was theirs. The archbishops stayed: Witto from 889 to about 910 in the lists (`dossiers/rotomagus.md` 3).

**Chartres, 20 July 911.** Rollo's army besieged Chartres and was beaten by Robert, count of Paris and marquis of Neustria (brother of King Odo; himself king 922 to 923), and Richard of Burgundy; Bishop Gantelme is said to have shown the Virgin's tunic from the wall. The story is Dudo's (about 1015); the day is from the annals of Sainte-Colombe of Sens (?), and that it was a Saturday is from memory (?). Flodoard, a generation after, speaks of "the war that Count Robert fought against them at Chartres" after which they began to take the faith (Historia Remensis ecclesiae IV.14, our rendering). `realms/francia.md` has "911 Chartres (20 July)".

**The grant.** That King Charles the Simple granted Rouen and the land to the sea to Rollo is certain, because his charter of 14 March 918 for Saint-Germain-des-Prés excepts from a gift the part of an abbey's land "which we granted to the Northmen of the Seine, namely Rollo and his companions, for the safeguarding of the kingdom" (our rendering; Recueil des actes de Charles III le Simple, ed. Lauer, no. 92 (?); spine `cast.rollo`). The year 911, the meeting at Saint-Clair on the Epte, the terms and the story of the king's foot are all Dudo's and have no other witness (`chronicle_793.json` 911-rollo-rouen calls 911 the traditional date). The card tells the foot story as a story already going round, by a man who says he was holding horses.

**The baptism.** Dudo (c. 13 in Lifshitz's numbering; retold, never quoted) puts it in 912: the archbishop baptises Rollo, Count Robert is his sponsor and gives him the name Robert; for seven days he wears the white garments and each day gives land to a church (Saint Mary's of Rouen, of Bayeux and of Evreux, Saint Michael's in peril of the sea, Saint Peter's and Saint Ouen's at Rouen, Saint Peter's at Jumièges, Saint Denis's); on the eighth day he begins to share out the land to his men by the rope. One article gives 915 (`dossiers/rotomagus.md` 8).

**Which archbishop.** Dudo's archbishop is Franco. Flodoard's, writing much nearer the time, is Wido (the lists' Witto). The lists set Franco from 911 to 919 (`dossiers/rotomagus.md` 3). The cards never name the archbishop, so neither source is contradicted.

**The letter to Reims.** Flodoard, Historia Remensis ecclesiae IV.14: Archbishop Hervé of Reims (900 to 922) worked for the conversion of the Northmen; at the asking of the archbishop of Rouen he sent twenty-three chapters gathered from the fathers on how such people should be handled; and he asked Pope John (John X, 914 to 928) what to do with men who had been baptised, and baptised again, and still lived as heathens, killed Christians and priests and ate what had been offered to idols. The pope's answer was to go gently with those so new to the faith. This is step 9 and the answer in ending 2. Men baptised more than once for the white garment is also Notker's joke about Northmen at Louis the Pious's court (Gesta Karoli II.19). In 943 a Northman called Turmod "went back to idols and heathen rite" (Flodoard, Annales; `dossiers/rotomagus.md` 6).

**Rollo's law.** Dudo c. 13: theft forbidden; ploughs to be left in the field; the wife at Longpaon who hid her husband's plough irons, the five shillings, the ordeal of hot iron, and both hanged, she for the theft and he for knowing. William of Jumièges ii: gold arm rings hung on an oak by a pool, untouched for three years (`dossiers/rotomagus.md` 7). Step 8 is that law met by a merchant.

**The saint.** Saint Ouen's body had been carried "to Francia" out of the raids (Dudo 13); the monks of Saint Peter's (later Saint-Ouen) and the saint came back to Rouen in 918 (fr.wikipedia "Abbaye Saint-Ouen de Rouen" and "Ouen de Rouen", as read in `dossiers/rotomagus.md` 2 and 5). That Rollo walked out to Longpaon and carried the shrine is a later Rouen story (?): the card gives it as "so the monks say". A cloth laid on a shrine and kept as a relic (brandea) is the old Roman custom (Gregory the Great, letter to the empress Constantina, Epistolae IV.30).

**The market after.** About 1000 the men of Rouen pay toll at London on wine and blubber fish (IV Æthelred 2; `dossiers/rotomagus.md` 4). By the time of Rollo's grandson, Rouen spoke Romance and a boy was sent to Bayeux to learn Danish (Dudo, LATER): hence the children on the strand in step 7. The spine's thread `dorestad_falls` ends with this chain: "Rollo's Rouen becomes the best market on the Channel".

**Invented, and said so.** Ketil the steward (Old Norse Ketill), Gozbert the canon (Goz- + -bert), Odilard the wine seller (Odil- + -ard), Warin the count's man (an attested Frankish name), Thorstein the shipmaster (Old Norse Thorsteinn), the net-maker and his boy, the newcomers of step 9. The archbishop's letter to Count Robert in step 2. The hanged cousin. Horses in the amphitheatre; eleven canons. Every price and sum. That the camp was short of bread after Chartres (it is what beaten armies were: the great army left Francia for Kent in the famine year 892, ASC). That a merchant carried men to the Epte. And the whole of ending 3.

## The people

| Who | Id | In the cards |
|---|---|---|
| Rollo, leader of the Northmen of the Seine, lord of Rouen from 911 | `cast.rollo`, historical | actor `rollo` (role chief: the roles table says to use chief for a lord of the Northmen) |
| Robert, count of Paris, marquis of Neustria | historical | actor `count` in `s06_bridge` only |
| The archbishop of Rouen | historical, unnamed (Witto or Franco) | actor `bishop` |
| Hervé, archbishop of Reims, 900 to 922 | historical | named in the words only |
| Ketil, who buys for the camp | invented | actor `steward` |
| Gozbert, canon of the archbishop's house | invented | actor `canon` (role clerk) |
| Odilard, the last wine seller of Rouen | invented | actor `seller` (role merchant) |
| Warin, Count Robert's man on the Paris strand | invented | actor `man` (role lords_man) |
| Thorstein, a shipmaster with a farm he does not want | invented | actor `partner` in `end_peace` |
| A Channel skipper; a newly come shipmaster; a monk of Saint Peter's; the player's mate | unnamed | `skipper`, `newcomer`, `monk`, `mate` |

## The steps

All cards are stage `harbour`. "Wait" is the timed flag `ch_rollo_911_wait`: the next card cannot be drawn until it runs out.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 0 | `harbour.chain_rollo_911.s00_word` | Quentovic, Parisius, Lundenwic or Hamwic; 904 to 910 | ask the price of bread; buy him ale and ask the way; shrug | `ch_rollo_911_0` (not on shrug) |
| 1 | `...s01_ruined_market` | Rotomagus; 904 to 910 | sell to the steward; sell to the archbishop's house; sell nothing and walk the town | `ch_rollo_911_1`; `ch_rollo_911_sold_north`; wait 30 days |
| 2 | `...s02_canons_errand` | Rotomagus; before 911 | carry the letter; carry it and let the steward hear it (only if you sold to him); refuse | `ch_rollo_911_2`; `ch_rollo_911_letter`; `ch_rollo_911_letter_shown` |
| 3 | `...s03_counts_man` | Parisius; before 912 | tell what you counted; count only casks; double the ships (doubtful) | `ch_rollo_911_3`; `ch_rollo_911_told_franks`; `ch_rollo_911_lied_franks`; wait 40 |
| 4 | `...s04_blades` | Rotomagus; before 911 | promise the iron and horses in the hold; offer wheat; refuse | `ch_rollo_911_4`; `ch_rollo_911_armed` (lasting); wait 40 |
| 5 | `...s05_after_chartres` | Rotomagus; August 911 to 913 | feed the camp (60 d); carry the archbishop's clerk; go up to Paris | `ch_rollo_911_5`; one of `ch_rollo_911_road_peace` (+ `ch_rollo_911_fed`), `ch_rollo_911_road_church`, `ch_rollo_911_road_franks`; wait 60 (20 on the Paris road) |
| 6a | `...s06_epte` | Rotomagus; roads of peace and church | ask the toll; have your name written; ask nothing | `ch_rollo_911_6`; wait 120 |
| 6b | `...s06_bridge` | Parisius; road of the Franks | tell all; tell and ask mercy for those who leave; say they are strong (back to the true road) | `ch_rollo_911_6`; `ch_rollo_911_northmen_feud` (lasting) or `ch_rollo_911_mercy`; or `ch_rollo_911_road_franks = 0` and `ch_rollo_911_road_peace`; wait 120 |
| 7 | `...s07_baptism` | Rotomagus; from March 912 | give linen (40 d); learn their speech; sell wine | `ch_rollo_911_7`; `ch_rollo_911_linen`; wait 200 |
| 8 | `...s08_plough` | Rotomagus; from 913 | the count's court (likely); fetch it back quietly (likely); let it go | `ch_rollo_911_8`; `ch_rollo_911_court`; `ch_rollo_911_quiet`; wait 200 |
| 9 | `...s09_question` | Rotomagus; from 914 | tell the steward; carry the letter for Reims; buy the plunder | `ch_rollo_911_9`; sets the road to peace or to church; `ch_rollo_911_bought_loot` |
| E1 | `...end_peace` | Rotomagus; from 918; road of peace | a plot and a free keel; join hands with Thorstein (hard road); Rollo's peace and no land | `ch_rollo_911_end_peace` and the unlocks below |
| E2 | `...end_saint` | Rotomagus; from 918; road of the church | the monks' cellar; a cloth from the shrine; the archbishop's writing (hard road) | `ch_rollo_911_end_saint` and the unlocks below |
| E3 | `...end_river_shut` | Parisius; four months after the bridge | the count's protection and a free keel; the river's marks as well (hard road); silver | `ch_rollo_911_end_river_shut`, `changed_rollo_911_no_grant` and the unlocks below |

No dead ends: every card has an option that costs only time or pride; steps 2 to 4 cannot block the fork; a failed check (the lie at Paris, the court, the quiet road) costs a little and moves on; the Paris road can be turned back at the bridge; step 9 lets a player change between the two true roads. Every danger warns first: the skipper and the wine seller name the law on arms before step 4's offer; the steward says "before noon" before the theft is judged.

## The endings

**1. A length of the rope (`ch_rollo_911_end_peace`). What happened.** Rollo holds Rouen of the king, hangs thieves, takes the old toll and no more, and the wine boats come back. The player is one of the few who came when nobody came. Ketil has a farm; Odilard passes his father's forty casks a week.
Picture: high summer on the strand, wine boats three deep, two men stretching a rope from the river gate to a new stake, old Rollo pointing with a staff, timber houses rising behind the Roman wall.

**2. The saint comes home (`ch_rollo_911_end_saint`). What happened, on the church's side.** In 918 the monks of Saint Peter's bring Saint Ouen down the river; the answer from Reims says to go gently with the new Christians. The player is the saint's man on the strand. The Northmen's Rouen and the archbishop's Rouen become one town.
Picture: a barge with a gabled shrine and singing monks on the Seine, the player's small boat ahead with a man sounding, a crowd on the bank and one very big old man walking down to the water alone.

**3. The river shut (`ch_rollo_911_end_river_shut`, `changed_rollo_911_no_grant`). History changed.** Count Robert believes the player's count, timbers the bridge at Pîtres and tells the king to grant nothing. At the first frost the fleet goes down the river and out to sea. Rouen is the archbishop's again: eleven canons and grass in the market.
Picture: a cold clear morning, a new timber bridge on old stone piers with armed men on it, the last long ships small on the ebb far down river, an empty strand under the wall and one boat passing without stopping.

**What would follow from ending 3 in later years** (for cards that read `changed_rollo_911_no_grant`):
- No county of Rouen and no line of counts and dukes there: no William Longsword at Rouen (927 to 942), no Richard I, no Emma married to an English king in 1002, no English court in exile at Rouen in 1013, and no duke with a claim on England in 1066. Cards of those years at Rotomagus should not be drawn, or should show a small Frankish town.
- Rotomagus stays an archbishop's town of the western kingdom, held when it can be by the counts of Paris. Its market does not recover: the wine of the Paris basin goes to sea by other hands, or not at all (the ending drops the wine price at Parisius for a year).
- The Seine has no Northman sworn to keep other Northmen out. The next fleet finds a bridge and a tired garrison, as in 885. Raid cards on the Seine may go on past 911.
- Saint Ouen does not come home in 918 by Rollo's leave; the monks' return is the archbishop's to arrange, later (?).
- The fleet goes somewhere. The words leave it open ("some say to the Loire, some over to the English"): the Loire was in fact held by Northmen until 937, and a fleet from Brittany raided the Severn in 914 (ASC).
- If the player told all and asked no mercy, `ch_rollo_911_northmen_feud` is set: Northmen met at sea may know who counted the cook fires.

## Payouts

| Step | Option | Silver | And |
|---|---|---|---|
| s00 | ask / ale | 0 / -2 | a true price word: wheat dear at Rotomagus (x1.5, 150 days), or wine cheap there (x0.8) |
| s01 | steward / church / look | +30 / +12 / 0 | steward's feeling; or deed `fair_dealing` and the canon's feeling; or a wine price word (x0.75, 180 days) |
| s02 | carry / show / refuse | +10 / +40 / 0 | deed `carried_letters`; or the steward's feeling and a warmed seal |
| s03 (turn) | truth / casks / lie | +100 / +20 / +140 or +20 | Warin's feeling; a wine price word at Parisius |
| s04 (turn) | iron / wheat / refuse | +250 / 0 / 0 | a commission: 6 loads of wheat within 180 days at a premium of 1.6; or a salt price word (x1.4) |
| s05 | feed / clerk / Paris | -60 / 0 / 0 | the steward's feeling +15; deed `carried_letters` |
| s06_epte | toll / roll / silent | +30 each | half toll at Rotomagus for 180 days; or Latin +0.1; or the crew's heart |
| s06_bridge | all / mercy / back | +150 / +80 / 0 | deed `fair_dealing` on mercy |
| s07 (turn) | linen / speech / sell | -40 / 0 / +150 | deed `gave_alms` and two feelings; or Old Norse to 0.4; or silver |
| s08 | court / quiet / leave | +60 or -10 / 0 or -40 / 0 | deed `won_case` or `lost_case`; two loads of eels; the riddle `riddle_exeter_21_plough` |
| s09 | steward / letter / buy | +40 / 0 / +90 clear | deed `carried_letters` and Latin +0.15; or two loads of wax |
| **end_peace** | plot | **+900** | `unlock_plot_rotomagus`, `unlock_charter_rotomagus` |
| | Thorstein (hard road: fed the camp, sold no iron, bought no plunder) | **+1,400** | those two and `unlock_partner_thorstein` |
| | no land | **+1,100** | `unlock_title_rollos_peace` |
| **end_saint** | cellar | **+800** | `unlock_title_st_ouens_man`, `unlock_plot_rotomagus` |
| | cloth | **+600** | `unlock_relic_st_ouen_cloth`, `unlock_title_st_ouens_man` |
| | writing (hard road: no letter shown, no plunder bought) | **+1,200** | `unlock_charter_rotomagus`, `unlock_title_st_ouens_man`, `unlock_relic_st_ouen_cloth` |
| **end_river_shut** | protection | **+1,200** | `unlock_title_marquis_protection`, `unlock_charter_parisius` |
| | the river's marks (hard road: asked mercy, sold no iron) | **+1,500** | those two and `unlock_pilot_seine` |
| | silver | **+1,800** | `unlock_title_marquis_protection` |

A full run on the best true road pays about 2,000 d in silver and three grand rewards; the least any road pays at its end is 600 d and two.

**Why a merchant could really have won these.**
- A plot by the rope and a free keel at Rotomagus: Rollo shared the land out to those who had followed him (Dudo 13), and freedom from toll at named landings is what charters gave traders (Saint-Germain-des-Prés at Rouen, 779: `dossiers/rotomagus.md` 4).
- A sworn partner with a second ship: the Norse félag, hands joined and silver put in one purse (CHAINS.md).
- Rollo's peace: a lord's spoken protection over a guest and his goods, the ordinary northern way; his own fear was other Northmen on his river (spine `cast.rollo`).
- The saint's man, the abbey's cellar: abbeys kept their own traders and cellars at river ports.
- A cloth from the shrine: brandea, given to laymen since Gregory the Great.
- The count's protection and a free keel at Paris: lords paid for news of the Northmen, and the tolls of the Paris strand were a lord's to forgive.
- The marks of the Seine: `unlock_pilot_seine` is already a reward of the wine_fleet chain; here it is the count's own river man who shows them.

**Costs that last.** `ch_rollo_911_armed` (sold arms to the Northmen against the law of Pîtres: the Franks up river remember; it closes the hard road of endings 1 and 3). `ch_rollo_911_northmen_feud` (told all at the bridge and asked no mercy). `ch_rollo_911_letter_shown` and `ch_rollo_911_bought_loot` (the archbishop knows; they close the hard road of ending 2, and the second also that of ending 1).

## How long it takes

The hook can come in 904; the fork cannot come before August 911; the endings at Rotomagus cannot come before 918. So the true roads take **seven to fourteen years** of game time, a card every half year or so, lying under ordinary trade on the Seine. The changed road is shorter: from the fork to the end is about five months (20 days to the bridge, 120 to the frost), so it can be over by the spring of 912. Waits between steps are 30 to 200 days and are kept by a timed flag, not by the calendar, so a house that is away for a year loses nothing.

## Pictures (one for each card)

| Card | Picture |
|---|---|
| s00_word | A Channel strand on a grey afternoon: a skipper on an upturned tub with a cup, pointing south-west with his thumb; one listener already turning away. |
| s01_ruined_market | Rotomagus from the water in low morning light: the patched Roman wall, six long ships under it, grass and goats where a market was; a steward with an arm ring and a thin canon in black walking to one small boat from opposite sides. |
| s02_canons_errand | Before sunrise, mist on the river: the canon holding out a small sealed letter to the merchant in his boat; one Northman on the wall looking down. |
| s03_counts_man | The Paris strand at noon, the island behind: a broad man in mail and a red cloak sitting sideways on the gunwale with a wax tablet; the merchant counting on his fingers. |
| s04_blades | A stone storehouse lit by one door: a heap of hacked silver on the floor; the steward holding a sword blade to the light; the merchant in the doorway, the wine seller at his shoulder. |
| s05_after_chartres | A dull late-summer evening under the wall: fewer ships, two scorched; bandaged men at three small cook fires; three people standing at one small boat. |
| s06_epte | A small river joining a great one under yellow trees: boats tied at the bank, the merchant mending sail; a very big grey-bearded Northman in a Frankish cloak coming down the path, a steward and a canon behind. |
| s06_bridge | A timber hall by lamplight: a heavy grey-haired count in mail at table, leaning on his elbows; his man with a wax board; the merchant cap in hand; the river dark in the door. |
| s07_baptism | A bright spring day before the two joined churches: an old, very big man in long white linen with a small cross, the archbishop beside him; wet-haired Northmen in white shirts; a wine cart doing good trade. |
| s08_plough | Early morning: bales, casks and nets unguarded along the whole strand; one gap in the merchant's stack; the mate pointing at a boy who looks away; on the hill, small, a plough alone in a furrow. |
| s09_question | Evening behind a new timber hall: a stone dark with blood, a pig's head on a stake; a young shipmaster with a new cross holding out cloth and wax; the archbishop at the gate with a letter. |
| end_peace | As under ending 1. |
| end_saint | As under ending 2. |
| end_river_shut | As under ending 3. |

## For the integrator

- New flags the game should read: `unlock_title_rollos_peace`, `unlock_title_st_ouens_man`, `unlock_title_marquis_protection`, `unlock_relic_st_ouen_cloth`, `unlock_partner_thorstein`, `unlock_charter_rotomagus`, `unlock_charter_parisius`; and `changed_rollo_911_no_grant`, `ch_rollo_911_northmen_feud`, `ch_rollo_911_armed` for later cards. (`unlock_plot_rotomagus` and `unlock_pilot_seine` are already used by wine_fleet.)
- The chain assumes the house can lie at Rotomagus and Parisius in 904 to 918 and that `port.id`, `month` and timed flags (`flag x for n`) work as in `Cond.cs` and `Effects.cs`.
- Rollo is given the role `chief`, not `count`, so that he is not taken for the count who holds the house's debt.
- The spine's `cast.rollo` has `from: 905`; the chain shows him only from the autumn of 911.
- Not checked against a text tonight, and marked (?) above: the chapter number of the Edict of Pîtres; the number of the charter of 918 in Lauer; the source for the day of Chartres; Rollo carrying the shrine.
