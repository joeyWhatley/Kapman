# Chain: wreckers (the fire on the wrong sand)

Cards: `Assets/Kapman/Game/Data/encounters/chain_wreckers_793.json` (14 cards: 13 harbour, 1 sea).
Words: `Assets/Kapman/Game/Data/writing/chain_wreckers.json` (211 entries, with an `art.<card id>` line for every card).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
It stands just after the chain `coast_watch_810`, whose last step is the emperor's fire lit on the tower at Boulogne in 811: this chain is what a second, lower fire does to masters who have learned to trust the first. It touches `pilgrim_road` only in its places (Sarre, the cell of Saint Josse).

## The story in five sentences

From 811 a fire burns by night on the old Roman tower at Boulogne, a day up the coast from Quentovic, and between 812 and 823 fishermen carry a Kentish steersman up the bank of the Canche on a hatch board: his master lost the tower's fire in the murk, took a low fire ahead for the monks' hill of Saint Josse, put the ship north of it, and found the banks.
The fire is an old salt-boiler's pan on the bare sand two leagues south of the hill; the first ship that came to it was nobody's doing, in a hungry winter, and since then the steward of the shore has sent the old man pitch and told him which nights his salt wants boiling.
By the custom of the strand the lead and the cloth are the shore lord's, less a third to the people who carry them up, and three living seamen are in his byre "for their keep" until Martinmas; the ship's owner, the abbess Cwenthryth at Sarre, has a charter that speaks of ships "lost by wreck", an old king's letter that promises her merchants an appeal to the emperor's judges, and no patience.
The merchant can buy the men out, lie in a gut between the banks on the next thick night and turn a Frisian wine ship away with a pitch torch, bring the whole matter before the emperor's judges with a saved master, free seamen and a marked pig of lead, sit down with the skippers of the wic over an empty box, or dine with the lord of the strand.
It ends with the men bought home and the fire still burning, as such things did end; or with the only fire on that shore set on the saint's hill under the emperor's ban; or with Frisian and English masters agreeing, without an oath, to help each other in shipwreck; or with a third of everything that sand will ever give.

## How a player meets it

- `s01_drowned_man` is drawn on a call at **Quentovic from 812 to 823** (weight 3; it comes again after 25 or 30 days if passed by). Every card of the chain stops after 827, the last year of the abbess.
- Asking the fishermen first opens `s02_custom_of_the_strand`, the warning, before anything is risked.
- A place writer may add `card:harbour.chain_wreckers.s01_drowned_man` to `quentovic.bank`, `card:harbour.chain_wreckers.s08_what_they_saw` to `quentovic.saint_josse`, `card:harbour.chain_wreckers.s13_help_in_shipwreck` to `quentovic.alehouse` and `card:harbour.chain_wreckers.s06_the_abbess` to `sarre.ship_house`.
- The news `chain_wreckers.news.home`, `.judgement`, `.settled` and `.club` are posted at Quentovic, Sarre and Dorestad, so the ending is heard of where the house trades.

## The real history under it

**False lights are a legend, and the chain is built to respect that.** English Wikipedia, "Wrecking (shipwreck)" (read 6 Oct 2026): "there is no clear evidence that this has ever happened", and a seaman avoids a light he cannot name. So nobody here hangs a lantern on a horse. The fire is a real thing with an honest purpose (a salt pan) that is mistaken for a known mark (a fire on the hill of Saint Josse) by a master who has just lost another known mark (the tower). The idea is as old as Roman law: fishermen are forbidden to show a light at night as if marking a harbour and so bring ships to danger (Digest 47.9.10, from memory (?)). The Digest was not read in Francia in these years and nobody in the cards cites it. No source of 793 to 1066 records a false light on any coast: **the fire is invented.**

**The tower's fire.** In 811 the emperor came to Boulogne to see his fleet, restored the lighthouse "set up long ago to guide the courses of sailors" and had a fire lit on it by night (Annales regni Francorum 811; `lore/sea_lore.md` 2; `story/works_793.json` 'beacon'). It stood about 29 km north of the Canche (`dossiers/quentovic.md`). The hill of Saint-Josse south of the river as a sea-mark is the dossier's; that the brothers ever burned furze on it for an expected ship is invented (?).

**The custom of the strand.** "What the sea throws up belongs to the lord of the shore; men thrown up with it can be seized" (`lore/law_tolls.md` 12; `lore/sea_lore.md` 13; de.wikipedia "Strandrecht": curbed only from the 12th century). On this very coast in 1064 Earl Harold was driven ashore, seized by Count Guy of Ponthieu and held at Beaurain on the Canche until Duke William bought him out; William of Poitiers calls it the custom of that shore (Bayeux Tapestry; William of Poitiers (?); `dossiers/quentovic.md`). Wilfrid's stranded ship was attacked as a matter of course in Sussex about 666 (Vita Wilfridi c. 13). The finders' third is the later northern rule (Gulathing law, Gragas: LATER as text), used here as local custom (?). No king of these years wrote a law of wreck that survives.

**The lost ship and her owner.** The minster in Thanet held ships free of the Mercian kings' toll, and one charter provides that if a ship is broken, worn out with great age or lost by wreck another may take her place under the same freedom (Sawyer 91, 29; `places/sarre.json`, story `sarre.third_ship`). Cwenthryth, daughter of King Coenwulf of Mercia, was abbess of Winchcombe, Reculver and Minster in Thanet; Archbishop Wulfred went to law against her in 825 and had the houses by 827, after which she is not recorded (en.wikipedia "Cwenthryth", read 6 Oct 2026). She is `sarre.cwenthryth`, 811 to 827, which sets the chain's years. That she lost a ship on the Canche is invented.

**English lead on the Canche.** Lupus of Ferrieres asks for English lead to be brought to Stapulas (Etaples) about 852 (`dossiers/quentovic.md`): thirty years LATER than the chain. The abbess of Repton's lead at Wirksworth paid a rent in lead to Christ Church, Canterbury, in 835 (Sawyer 1624 (?)). A cross cut in each pig as the minster's mark is invented (Roman pigs were lettered).

**The appeal.** King Charles to King Offa, 796: merchants are to have the king's protection in his realm by the old custom of trading, and if they are wronged anywhere they may appeal to him or to his judges (MGH Epistolae IV no. 100; chronicle `796-spring-offa-letter`). The missi dominici rode in pairs, a churchman and a count, to do justice above all to churches, widows, orphans, pilgrims and strangers (the general capitulary for the missi of 802, from memory (?)). Proof is by oath with helpers; the king's men could also put the neighbours on oath in an inquest (?). The king's ban is sixty solidi. **No judgement on wreck survives from these years: the one in ending 2 is invented, and is the change.**

**Help in shipwreck.** Herstal, 779, c. 16: nobody is to swear a sworn fellowship (*gildonia*), but men may make agreements among themselves for alms, for fire and for shipwreck, so long as nobody swears (MGH Capitularia I no. 20; `lore/law_tolls.md` 12 and the table of Herstal). So such an agreement was lawful from 779 and common enough to legislate for; none is recorded by name. **The skippers' box in ending 3 is invented, and is the change.**

**Hunger.** The Royal Frankish Annals under 820 tell of constant rain, a sickness of men and cattle and a failed harvest (from memory (?)). The fisherman's line about the wet summer shows from 820 only.

**Salt.** Brine boiled over fires on the sand is how salt was made on these coasts (`economy/wine_salt.md`); pans on this particular strand are a guess (?).

**Small things.** "Cursed be he who makes the blind man lose his way" is Deuteronomy 27:18 in our own words. The anchor riddle is Exeter Book riddle 16 (`riddles_793.json`; the book is of about 975, the riddles older (?)). Christians were not to be sold out of the realm (the capitularies; `reputation_793.json` 'sold_captives'), which is why the lord speaks of keep and not of price; the dealer "on the Amiens road" is a guess (?).

**Invented, and said so.** Hunfrith the steersman, Ealdred the master, Eadhelm, Sigehere and the boy Wiga; Gundbert the fisherman and his brother Ragenulf the salt-boiler; Adalwin the steward; Odalric, the emperor's vassal who holds the strand (no count is named at Quentovic); Winrad the guest-master; Liudbrand of Dorestad; the banks and the gut (the river mouth of these years is unknown).

Web: no searches; two fetches (en.wikipedia "Wrecking (shipwreck)" and "Cwenthryth"). Everything else is from the project's own research and from memory, marked (?).

## The people

| Who | Id | In the cards |
|---|---|---|
| Cwenthryth, abbess of Minster in Thanet | `sarre.cwenthryth`, historical, 811 to 827 | actor `abbess` at steps 6 and E1 |
| King Charles, King Offa, King Aethelbald, Archbishop Wulfred | historical | spoken of |
| Hunfrith, steersman of the lost ship | invented | actor `survivor` at 1, 3; `steersman` at 8 (can be hired) |
| Eadhelm, Sigehere, Wiga | invented | actor `captive` at 5, 7; `crewman` at 8 |
| Gundbert, fisherman of the river mouth | invented | actor `fisherman` at 1, 2 |
| Ragenulf, salt-boiler, his brother; a granddaughter | invented | actors `keeper`, `child` at 9 |
| Adalwin, steward of the strand | invented | actor `steward` at 4, E4 |
| Odalric, lord of the strand | invented | actor `lord` at 5, 7, E2, E4 |
| Winrad, guest-master at Saint Josse | invented (after `quentovic.osmund`, to 812) | actor `guest_master` at 8 |
| Liudbrand, master of a Dorestad wine ship | invented | actor `master` at 10, E3 |
| The emperor's judges; the toll clerk | unnamed (the clerk is `quentovic.hildric` until 826) | actors `judge`, `clerk` at E2 |

## The steps

`P_` stands for `ch_wreckers_`. Waits are flags with a term. "Likely" is a chance of 0.55 before the crew's skill, "doubtful" 0.5, "sure" 0.85.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_wreckers.s01_drowned_man` | Quentovic, 812 to 823; drawn | hear him out / a cloak and 8 d / ask the fishermen first / pass | `P_1`; `P_kind`; `P_warned` |
| 2 | `harbour.chain_wreckers.s02_custom_of_the_strand` | Quentovic, after 1 or after asking; optional; **the warning** | ale for the lie of the banks (4 d) / who holds the strand / thank him | `P_2`; `P_banks`; `P_knows_lord`; `P_2_done` |
| 3 | `harbour.chain_wreckers.s03_evening_ebb` | Quentovic, after 1: a night off the south sand | watch till dawn / creep in with the lead (likely) / lie in the gut (needs `P_banks`; sure) / stay in the river | `P_3`, `P_saw_fire`; `P_place` |
| 4 | `harbour.chain_wreckers.s04_the_south_sand` | Quentovic, after 3 | look over the wreck / buy off the cart (60 d) / ask the woman who keeps the fire | `P_4`, `P_4_wait` (12 days); `P_mark`, `P_bought` or `P_keeper_known` |
| 5 | `harbour.chain_wreckers.s05_lord_of_the_strand` | Quentovic, after 4 | pay the keep now (150 d) / your word till Martinmas / ask by what right | `P_5`, `P_5_wait` (20 days); `P_freed` or `P_pledged`; `P_asked_right` |
| 6 | `harbour.chain_wreckers.s06_the_abbess` | **Sarre**, after 4 | her silver and her sealed complaint / the complaint only / give back what you bought | `P_6`, `P_writ`, `P_court_wait` (40 days); `P_abbess_silver`; `P_returned`, `P_mark`, clears `P_bought` |
| 7 | `harbour.chain_wreckers.s07_the_byre` | Quentovic, pledged road only | pay 150 d / haggle (doubtful) / pay nothing yet | `P_7`, `P_freed`; `P_owes_lord`; `P_7_hold` (30 days) |
| 8 | `harbour.chain_wreckers.s08_what_they_saw` | Quentovic, once the men are free | take them home / hire Hunfrith / ask all four to swear (needs `P_writ`) | `P_8`; `P_men_aboard`, `P_hunfrith` or `P_witness` |
| 9 | `harbour.chain_wreckers.s09_the_salt_boiler` | Quentovic, 12 days after 4 | lie between his fire and the ships / pay for a cold pan (30 d) / let it burn / not tonight | `P_9`; `P_dark`; `P_let_burn`, `P_bought` |
| 10 | `sea.chain_wreckers.s10_second_ship` | at sea, the first passage after 9; once | a torch from the gut (needs `P_banks` or `P_place`; sure) / run down and hail (likely) / anchor and stand by / let her strike | `P_10`, `P_10_wait` (14 days); `P_saved_ship` or `P_took_off`; `P_first_on_sand`, `P_bought` |
| E2 | `harbour.chain_wreckers.s11_the_judges` | Quentovic, 40 days after 6, after 5 | call the oaths (hard road; sure) / plead alone (doubtful) / not this sitting (60 days) | `P_end_judgement`, `P_done`, `P_lord_enemy`; `changed_canche_strand_peace`; or `P_lost_case` |
| E1 | `harbour.chain_wreckers.s12_the_ship_in_her_place` | **Sarre**, men free, after 6 | your keel in her place / the mastering of a new ship / ask nothing / not yet (60 days) | `P_end_ransom`, `P_done` |
| E3 | `harbour.chain_wreckers.s13_help_in_shipwreck` | Quentovic, 14 days after 10 (not if you let her strike) | 200 d in the box and your name first / your name only / not yet (30 days) | `P_end_club`, `changed_quentovic_wreck_club`, `P_done` |
| E4 | `harbour.chain_wreckers.s14_the_lords_table` | Quentovic, 20 days after 5, only with `P_bought`, `P_let_burn` or `P_first_on_sand` | a third of the strand / sell for him once / refuse | `P_end_share`, `P_blood_silver`, `P_done`; or `P_refused_share` |

**Roads through.** The short one, as it happened: 1, 3, 4, 5 (pay), 6, E1. The careful one: 1, 2 (banks), 3 (gut), 4 (look), 5, 6, 7 or 5 (pay), 8 (swear), 9, 10 (torch), E2 by the oaths. The skippers' road: any road to 9 and 10, then E3. The dark one: 4 (buy) or 9 (burn) or 10 (follow), 5, E4. The first ending taken closes the chain (`P_done`); "not yet" on E1, E2 and E3 keeps the others open.

**No dead ends.** Every card has an option that costs only time or pride (hear, thank him, watch, look, your word, the complaint only, pay nothing yet, take them home, lie out there, anchor, not this sitting, ask nothing or not yet, your name only or not yet, refuse). A failed lead-line is a night aground. A failed haggle still frees the men and leaves a tally. A failed torch or hail loses the Frisian ship and saves her nine men, which still opens E3. A lost case costs 30 d and a deed and leaves E1, E3 and E4 open. The men in the byre are never sold: the card returns until they are free, and E2 and E4 free them too.

**Every danger warns first.** Gundbert tells what happened to the pedlar who asked about the fire, before the player goes near it. Hunfrith says what the lead-line risks. Odalric names Martinmas and counts his twelve oath-helpers aloud before the suit. The steersman says what a torch may be taken for. The lord says at his table, once, what becomes of a man who eats there and then goes to the judges.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 (cloak) | -8 d | deed gave_alms; Hunfrith's feeling |
| Step | 2 (banks) | -4 d | the gut between the banks: steps 3 and 10 become sure |
| Step | 4 (any) | 0 | a true price word: English cloth at Quentovic x0.8 for 20 days |
| Step | 4 (buy) | -60 d for six bolts of cloth and two pigs of lead (about 138 d) | the stain `ch_wreckers_bought` |
| Cost | 5 or 7 | -150 d (or -100 d by a won haggle; or -100 d and a tally for 70) | the three men free |
| Turn | 6 (both) | +180 d | the sealed complaint; deed carried_letters |
| Turn | 6 (complaint only) | a premium of 1.6 on four casks of Rhine wine (about 145 d over the price) | deeds carried_letters and fair_dealing; the abbess's trust |
| Turn | 6 (returned) | +240 d for the goods given back | the marked pig as proof; deed fair_dealing |
| Turn | 8 | 0 | a hired steersman who knows the Kentish shore, or three passengers and a start in English, or four oaths; the anchor riddle |
| Step | 9 (cold pan) | -30 d | deed gave_alms; the keeper will speak |
| Step | 9 (burn), 10 (follow) | three or four casks of Rhine wine (180 to 240 d) | the stain, and a crew that looks away |
| Turn | 10 (ship saved) | +120 d and two casks of Rhine wine (120 d) | `ch_wreckers_saved_ship`: a Frisian master who will swear |
| Finale | E1 charter | 650 d | `unlock_charter_sarre` |
| Finale | E1 ship | 600 d | `unlock_ship_coaster_minster_ship` |
| Finale | E1 ask nothing | 300 d and twenty bolts of cloth (360 d) | `unlock_charter_sarre`; deeds gave_alms and kept_commission |
| Finale | E2 plead, won | 600 d | `unlock_title_kings_protection`; deed won_case; the cost `ch_wreckers_lord_enemy` |
| Hard road | E2 by the oaths | 1,100 d; half toll at Quentovic for a year | `unlock_title_kings_protection` **and** `unlock_work_quentovic_beacon`; deed won_case; the same cost |
| Finale | E3 first | 900 d (200 d put in) | `unlock_title_wreck_club`, `unlock_pilot_canche`, `unlock_factor_quentovic`; deed fair_dealing |
| Finale | E3 name only | 600 d | `unlock_title_wreck_club`, `unlock_pilot_canche` |
| Finale | E4 share | 900 d and ten bolts of cloth (180 d) | `unlock_title_lords_protection`, `unlock_pilot_canche`; the cost `ch_wreckers_blood_silver` |
| Finale | E4 once | 650 d | `unlock_title_lords_protection`; the same cost |

**Why a merchant could have won these.**
- *The abbess's freedom at Sarre* (`unlock_charter_sarre`, in `story/unlocks_793.json`): the charter itself says a ship lost by wreck may be replaced under the same freedom, and does not say the minster must build her. The abbess names the merchant's keel.
- *A minster's coaster* (`unlock_ship_coaster_minster_ship`, in the table): the minster must replace the ship; the wright builds a second on the same moulds.
- *The emperor's protection* (`unlock_title_kings_protection`, in the table): the letter of 796 promises it to merchants in general; the judges give it to one man by name because he has made an enemy doing their work.
- *A fire on the saint's hill* (`unlock_work_quentovic_beacon`: the work `beacon` of `story/works_793.json` at Quentovic): the emperor had just done the same at Boulogne. Here it is kept by the brothers at the shore lord's cost.
- *A seat in the shipwreck agreement* (`unlock_title_wreck_club`, **new**): lawful under Herstal c. 16. What it should do: a lost keel of the house is half made good from the common box (`lore/law_tolls.md` 12 proposes exactly this as a work at Dorestad).
- *The bar of the Canche* (`unlock_pilot_canche`, in the table): the masters cut their marks on one stick, or the lord's man shows the gut.
- *A host at Quentovic* (`unlock_factor_quentovic`, in the table): the woman who keeps the box answers for it, and for the goods of the man who filled it first.
- *A lord's protection* (`unlock_title_lords_protection`, in the table): what a vassal has to give a merchant who sells for him.

**Costs that last.**
- `ch_wreckers_lord_enemy` (E2): Odalric and his men. To be read by cards on the coast south of the Canche, and by a stranding there.
- `ch_wreckers_blood_silver` (E4): the merchant sells what the strand takes. To be read by Cwenthryth and her nuns, by Kentish skippers, by the guest-master at Saint Josse, and by any priest hearing the house's confession.
- `ch_wreckers_owes_lord` (7, a failed haggle): 70 d owed on a split tally. The abbess remarks on it; nothing collects it yet.
- `ch_wreckers_bought` still set at the end: wrecked goods bought and never given back. `ch_wreckers_let_burn`, `ch_wreckers_first_on_sand`: a ship let strike. Words only so far, and the crew's hearts at the time.

## The endings

| Ending | Flag | How | What becomes of it | Picture |
|---|---|---|---|---|
| The ship in her place | `ch_wreckers_end_ransom` | the men bought out and brought home; the abbess thanked or not | **What happened.** Nothing changes on the sand. The men are bought back as Harold was in 1064; the strand stays the lord's and the low fire burns on thick nights. The abbess has three men and a ship's freedom to give away. | A chalk strand: a mother holding a thin boy by the ear, two seamen kneeling to an abbess at the ship-house doors, a small Frisian boat afloat behind. |
| The emperor's judges | `ch_wreckers_end_judgement` | the complaint pleaded alone and believed | **A private win.** The men go free without payment and the lead is paid for. Nothing is said about the fire. | The bench before Saint Peter's: a lord making a small bow to an abbot, a merchant alone on the other side. |
| The fire on the hill (the hard road) | `ch_wreckers_end_judgement` with `changed_canche_strand_peace` | the gut learned, the second ship turned, the men free, a proof, and all of them sworn before the judges | **History changed.** One fire only on that shore by night, on the saint's hill, under the emperor's ban; the lord's twelve would not swear to the carts. | Dusk on a round hill above dunes: two monks feeding a fire in a stone hearth, a ship's sail far out turning north for the river. |
| Help in shipwreck | `ch_wreckers_end_club`, `changed_quentovic_wreck_club` | the second ship turned or her men saved, and the skippers' table joined | **History changed.** Frisian and English masters at the wic keep a common box and a horn boat at the bar. The strand is still the lord's; it is fed less often. | An ale-house table: masters of two peoples shoulder to shoulder, a small oak box, a long tally stick being notched. |
| A third of the strand | `ch_wreckers_end_share` | bought on the sand or let a ship strike, then sat at the lord's table | **What happened, from the other side.** The custom stands and the merchant is the one who sells what it brings in. | A lord's table by torchlight: a roast heron, a steward holding out half a tally stick. |

**What would follow in later years** (for later cards to read from the flags):
- `ch_wreckers_end_ransom`: nothing changes in the record. Masters bound for the Canche pass the word about the low fire; some hear it. The freedom at Sarre ends with the abbess's hold on the minster (827), unless a later card lets Christ Church renew it.
- `changed_canche_strand_peace`: there is a judgement, kept at Saint Josse, that no fire but the saint's burns on that shore and that strangers thrown up on it are the emperor's guests. The fire lasts as long as the cell does (the monks flee to England in 903; `dossiers/quentovic.md`). In 842, when the fleet comes on the wic at dawn, a watch on the hill is one more pair of eyes. In **1064** a card may let Earl Harold's men find the old judgement held up against Count Guy's: Guy still comes down the beach, but he is answering a writ, not following a custom, and the price of the earl is argued differently. A stranding of the house's own boat on that shore is helped, not stripped.
- `changed_quentovic_wreck_club`: the box outlives its founders if the wic does. When the trade moves (Quentovic fades in the tenth century, Dorestad is gone after 863) the agreement goes with the skippers to Tiel and Deventer, and a card of about 1020 may let the merchants of Tiel, who "swear and drink together" (Alpert of Metz), count the house among the oldest names on their stick. For the house: a lost boat is half made good.
- `ch_wreckers_end_share`: the house has a lord on the Ponthieu shore and an income nobody asks about. Kentish ports hear of it sooner or later; so does the next abbess. An heir inherits the half-stick.
- `ch_wreckers_lord_enemy` without the changed flag (E2, pleaded alone): the lord paid and was not judged. He is still there, and so is his fire.

## How long it takes

It can begin no earlier than 812 and no later than 823, and every card stops after 827. The night on the ebb and the morning on the sand follow at once. Sarre is a Channel crossing away. The salt-boiler waits twelve days from the sand before he comes; the judges sit forty days after the abbess seals her leaf, and again every sixty; the skippers' table sits fourteen days after the second ship; the lord's horse comes twenty days after his hall. By the short road the men are home in three or four weeks. By the hard road (banks, gut, Sarre, the men free, the salt-boiler, the second ship, the judges) it is two to three months at the least, and a season at an ordinary trader's pace, alongside other work. Martinmas is a word in the lord's mouth, not a clock: the men are never sold.

## For the integrator

- New flags the game will read: `unlock_title_wreck_club`, `unlock_work_quentovic_beacon`; `changed_canche_strand_peace`, `changed_quentovic_wreck_club`; the costs `ch_wreckers_lord_enemy`, `ch_wreckers_blood_silver`, `ch_wreckers_owes_lord`.
- Flags from the unlock table that the chain sets: `unlock_charter_sarre`, `unlock_ship_coaster_minster_ship`, `unlock_title_kings_protection`, `unlock_title_lords_protection`, `unlock_pilot_canche`, `unlock_factor_quentovic`.
- Clearing a flag is written `flag <name> = 0` (step 6, `ch_wreckers_bought`).
- Step 3 is a harbour card although it is told afloat, so that it happens off this coast and no other. Step 10 is a true sea card, drawn on the first passage after step 9 (rate 0.5 an hour): a player who sails from Quentovic meets it off the south sand; one who lies in port a month first meets it late, and the words still hold ("the next thick night").
- Step 8's `passenger crewman to sarre` carries the three seamen as one passenger; `hire steersman` hires Hunfrith.
- The outcome texts (`...out_<option>`, with `_success` and `_failure` after a chance) are in the writing file; each option's `note` names its own. The four `chain_wreckers.news.*` entries are the news the chain posts.
- People for the places files, if wanted: at Quentovic `quentovic.gundbert` (fisherman), `quentovic.ragenulf` (salter), `quentovic.winrad` (guest_master, from 812), `quentovic.odalric` (lords_man), `quentovic.adalwin` (steward); at Sarre `sarre.hunfrith` (steersman). Ending 2's picture of the hill is not a card's: it belongs to the work `beacon` at Quentovic.
