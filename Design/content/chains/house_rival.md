# Chain: house_rival (the rival house)

Cards: `Assets/Kapman/Game/Data/encounters/chain_house_rival_793.json` (13 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_house_rival.json` (219 entries).
Written 4 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.

## The story in five sentences

Thiadbod, the biggest Frisian shipman at Dorestad, takes no notice of the player's house until it starts to do well; then he undersells it with cloaks a hand short and corn by a short measure, and takes its berth at the dam.
When the house still will not carry for him, its boat is cut from her post one night and wrecked on the old dam posts, and the hired man who did it is sent down the river to the Maas mouth.
The matter goes to the count's court under the Frisians' own law, where the man who sets another on either swears himself clean with five free men, pays a third, or, if the doer is brought into the ring, pays nothing at all and "bears the enmity" of the house he wronged.
A season later Thiadbod's own ship lies on a new bank with the flood making, and the player decides whether to take him off, float her, help himself to the wine or stand off.
From those choices come three ends: one reckoning for two boats, Thiadbod's dam head sold by the count's steward, or a feud declared before the court, which the heirs of both houses inherit and may at last settle at the church door.

## How a player meets it

- It starts by itself at Dorestad, the home port, from 795: card `s01_short_cloaks` is drawn on a call there once the house has made two earlier calls and has 100 d aboard (weight 1.0, likelier for a house in good standing).
- **The rival grows as the player does.** Step 1 waits for silver and calls; step 3 comes when the house has 300 d aboard (or, at the latest, 220 days after step 2); Thiadbod's lines change at 300 d and 600 d ("They say you could buy a second boat"), and when the debt to the count is closed; both drawn steps want a good name (`wants: good_name`).
- **It gives way to the debt chain.** If the house's tally was sold to Thiadbod (`db_bought`, the "sold" ending of chain house_debt) he has what he wanted, and steps 1 and 3 do not come.
- The last card, `s13_heirs`, is for the player's heir: it is drawn at Dorestad from 826 only if the feud ending was taken.

## The real history under it

**The undercutting.**
- Notker of St Gall, Gesta Karoli I.34 (written 883-887, of Charlemagne's time: LATER; the chapter number as in the usual editions (?)): the Frisians sold short cloaks at the price of the long ones, and the king forbade it: "What is the good of those little napkins? I cannot cover myself with them in bed and when I am on horseback I cannot shield myself with them against wind and rain" (tr. A. J. Grant, 1907; quoted in card 1, where a Frank says the king's words). https://sourcebooks.fordham.edu/basis/stgall-charlemagne.asp
- Charles's letter to Offa, 796, on the length of cloaks: `chronicle_793.json` 796-spring-offa-letter. The cap on cloak prices of April 808 (best double cloak 20 solidi, half the fine to the informer: `dossiers/noviomagus.md`) falls after this chain's start and is not used.
- The king's price for corn and the new public measure: Synod of Frankfurt, June 794, c. 4: oats 1 d the modius, barley 2, rye 3, wheat 4, "in plenty or in dearth" (MGH Capit. I no. 28; chronicle 794-06-01-frankfurt-prices). Nijmegen, March 806: oats 2, barley 3, rye 4, wheat 6, and gain made by holding corn back is "turpe lucrum" (MGH Capit. I no. 46 c. 17-18; chronicle 806-03-nijmegen-prices). Equal measures ordered again in 803 (chronicle 803-arms-to-merchants-803).

**The law of the case** is the Lex Frisionum, the Frisians' law written down about 785-803 and known only from Herold's print of 1557. Read in the Latin with K. Nieuwenhuijsen's English and money notes: http://www.keesn.nl/lex/lex_en_text.htm (plain http; the https certificate is broken). Also https://en.wikipedia.org/wiki/Lex_Frisionum and https://de.wikipedia.org/wiki/Lex_Frisionum.
- Tit. II, Wlemar's addition: who sets another on to take a man's goods pays a third of the fine if the doer has fled the land, or clears himself by oath; if the doer has not fled, the setter "nec iuret, nec solvat, sed tantum inimicitias portet eius cuius pecuniam abstulit". This is the fork of cards 5 and 6: bringing the doer to court wins lawful enmity and no silver; letting him go over the sea makes the setter swear or pay.
- Tit. III (Thiubda): a free man charged with theft swears "sua sexta manu" (with five free men); if he owns it he pays twofold and his own wergild to the king as peace-money; one caught in the act goes to the ordeal of boiling water (III.8), and a false accuser redeems his hand (III.9).
- Tit. VII (De brand): a house burned is paid twofold, with all that was in it. The lawman's judgment in card 6, "a boat at her post is a house on the water", is modelled on it and is INVENTED (see History changed).
- Tit. VIII and Additio sapientum VIII.2: a thing taken by force is paid twofold with 12 solidi for the peace; a pledge taken by force is "pant" (card 4).
- Tit. XIV.3: between Vlie and Sincfal, where Dorestad lies, the custom is the twelve-hand oath and the ordeal of boiling water; the lot on the altar belongs to middle Frisia and is not used here.
- Additio VI (Saxmund): "Si quis in flumine viam publicam occluserit, XII solidos componat": card 3.
- Additio I.1 (Wlemar): "Homo faidosus pacem habeat in ecclesia, in domo sua, ad ecclesiam eundo, de ecclesia redeundo, ad placitum eundo, de placito redeundo"; who breaks that peace and kills pays nine times 30 solidi: the feud ending and the heirs' card.
- Money: between Vlie and Sincfal a free man's wergild is 50 solidi. The reckoning of the Lex's solidus in pennies is disputed (Nieuwenhuijsen, after Henstra, makes 50 solidi about 1,800 silver pennies) (?); the game's sums (the setter's third, 200 d; a pound of silver for a man's friendship) are set to the payout scale, not taken from the Lex.
- The kettle: a stone taken from boiling water, the hand bound and looked at after three days, is the general Frankish form (https://en.wikipedia.org/wiki/Trial_by_ordeal); the Lex names the ordeal and not its details.

**The court.** The count holds it; seven judgment-finders (scabini) are Charlemagne's reform of the late 8th century (https://de.wikipedia.org/wiki/Schöffe_(historisch)); forty nights between regular courts is the Franks' term (https://de.wikipedia.org/wiki/Thing) (?). "That judges shall judge justly, according to the written law and not according to their own judgment": capitulary of 802 c. 26 (tr. E. F. Henderson, 1896: https://avalon.law.yale.edu/medieval/capitula.asp). The peoples' laws were read and written at Aachen in October 802 (chronicle 802-10-aachen-laws-802), so the lawman's law is "in my head" before 803 and "written at Aachen" after. Where the court sat at Dorestad is not known; by Saint Martin's church (the Upkirika given to Utrecht in 777: `dossiers/dorestad.md`) is ours (?). No count of Dorestad is named: the count is the spine's invention.

**The feud.** Herstal 779: feuds are to be settled by compensation (https://en.wikipedia.org/wiki/Capitulary_of_Herstal). 802 c. 32: the kin "shall by no means dare to carry further their enmity ... but, pledging their faith, they shall make a lasting peace" (Henderson 1896; the chapter is of killings, the count stretches it). Thionville, December 805, c. 5: feuds to be settled (chronicle 805-12-thionville-arms). A marriage to end a feud is the custom the poems know (Beowulf 2020-2069, Freawaru and Ingeld).

**The fellowship.** Capitulary of Herstal, 779, c. 16: no oaths sworn in guilds ("de sacramentis per gildonia invicem coniurantibus"), but men may make agreements about their alms, about fire and about shipwreck, so long as no one swears to them (MGH Capit. I no. 20; given from memory, the text could not be opened tonight (?)). So in 800 an agreement of two houses against fire and wreck is lawful and the oath on it is not: the choice of the partnership ending. Later: Frisian guild-brothers on two rune stones at Sigtuna, 11th century (U 379, U 391) (?); the merchants of Tiel "swear and drink together" and judge their own cases about 1020 (Alpert of Metz; chronicle 1020-tiel-merchants).

**Saint Martin's men.** Louis the Pious for Utrecht, 18 March 815, confirming older grants: the church's men on the banks at Dorestad owe the king's officers nothing, and no one may lodge in their houses or seize their goods (Oorkondenboek Sticht Utrecht I no. 56, in `dossiers/dorestad.md`); that it held before 815 is assumed (?). A free man who gives himself to a saint for a yearly due of wax (a wax-payer, cerocensualis) is a Carolingian institution (from memory (?)).

**The wreck.** "Many ships wrecked with their crews by a violent storm in the British sea", 799 (Historia Regum; chronicle 799-british-sea-storm): one line in card 8, shown only in that year. Shifting banks at the Maas mouth and nothing known of Witla but its burning in 836: `dossiers/witla.md`. Wreck as the finder's "by the custom of the strand" is later law (?). Sword blades in a wine cask are the spine's fact about Thiadbod (thread swords_north); here they are only seen.

**Mainz.** "The best part of the city, where the Frisians lived" (Annales Fuldenses 886: LATER; the ports table and `dossiers/mogontia.md` have Frisian shippers on that shore in Carolingian times).

**Invented, and said so.** Thiadbod, Thiadburg, the count (Folcbert), the steward (Hartwin) and the pilot Gerlef are the spine's invented cast. Ours: Meinward (Mein- + -ward), a half-free man of the Batua who hauls for Thiadbod; Thiadlef (Thiad- + -lef), Thiadbod's son, fourteen at the wreck; the lawman, the wright, the priest and the bishop's clerk, unnamed. The sale of a debtor's goods by the count's steward is ours (?).

## The people

| Who | Id | In the cards |
|---|---|---|
| Thiadbod, the rival | `cast.thiadbod` (to 825) | actor `rival`: role skipper, fixed at Dorestad; role survivor at sea |
| Thiadburg, his elder sister | `cast.thiadburg` (to 815) | actor `widow` |
| The count | `cast.folcbert` (to 812) | actor `count` |
| The steward | `cast.hartwin` (to 808) | actor `steward` |
| Gerlef, pilot of the Maas mouth | `cast.gerlef` (to 812) | actor `pilot` at Witla |
| Meinward, the doer | invented | actor `cutter` at Witla |
| Thiadlef, the son and heir | invented | actor `son` at sea; the fixed skipper of Dorestad after 825 |
| The lawman, the wright, the priest, the clerk, a Frank, a Dane, a net-boy | invented, unnamed | roles lawman, shipwright, priest, clerk, lords_man, sailor, child |

## The steps

Scheduled cards (weight 0) open on the first call at their port after their day. "Likely" is a chance of 0.55 to 0.65, "doubtful" 0.3 to 0.5, before the crew's skill.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_house_rival.s01_short_cloaks` | Dorestad, 795-805; drawn | cut your cloth short too / measure the old length aloud / drink his wine | `ch_house_rival_1`; `_short_cloth` or `_true_cloth` |
| 2 | `harbour.chain_house_rival.s02_false_measure` | Dorestad, 70 days on | ask the steward to try both measures (likely) / have your own measure marked and sell heaped (12 d) / say nothing and go to Traiectum | `ch_house_rival_2`; `_measure_shown`, `_kings_measure`; `hy_new_measure` (spine); timers `_rest` 60 d, `_wait` 220 d |
| 3 | `harbour.chain_house_rival.s03_dam_head` | Dorestad; drawn at 300 d aboard, or after the 220 days | carry for him / name the river law (likely) / lie inside and say nothing | `ch_house_rival_3`; `_carrier`, `_way_cleared` |
| 4 | `harbour.chain_house_rival.s04_cut_rope` | Dorestad, 30 days on | raise the cry (30 d to the wright) / take a pledge by force (likely) / mend her yourself and say nothing | `ch_house_rival_4`; `_cry`, `_pant` or `_quiet`; timer `_cutter_near` 40 d |
| 5 | `harbour.chain_house_rival.s05_cutter` | Witla, from 2 days on; optional | take him to court (likely) / hear him before witnesses and let him go oversea (6 d) / leave him and ask about the bar | `ch_house_rival_5`; `_man_held`, or `_cutter_word` with `_man_fled` |
| 6 | `harbour.chain_house_rival.s06_thing` | Dorestad, forty nights after the rope | bring the doer into the ring (needs `_man_held`) / charge Thiadbod with witnesses (needs `_cry` or `_cutter_word`; likely) / charge him on your bare word (doubtful) / take 60 d in the porch | `ch_house_rival_6`; `_enmity`, `_won`, `_lost` or `_porch`; `changed_lex_frisionum_ship` |
| 7 | `harbour.chain_house_rival.s07_widow` | Dorestad, 20 days on | name a price (120 d; 240 d under lawful enmity) / ask her for room under her roof / say it stands | `ch_house_rival_7`; `_peace_bought`, `_widow_friend` or `_enmity_kept`; timer `_winter` 100 d |
| 8 | `sea.chain_house_rival.s08_on_the_bank` | at sea, under way, from 100 days on; drawn | take the men off / take them off and four casks / try to float her (doubtful) / stand off | `ch_house_rival_8`; `_saved` (with `_salvage` or `_ship_saved`) or `_left`; `_saw_blades` |
| 9 | `harbour.chain_house_rival.s09_reckoning` | Dorestad, 12 days on | offer fellowship (needs `_saved`) / let the law and his losses run (not if you floated his ship) / say there is no peace | `ch_house_rival_9` |
| 10 | `harbour.chain_house_rival.end_partner` | Dorestad, forty nights on | agreement without the oath / the oath on the altar / open the chest to every boat (the hard road) | `ch_house_rival_end_partner` |
| 11 | `harbour.chain_house_rival.end_ruin` | Dorestad, forty nights on | the dam head / the hull off the bank / let his sister have the dam head / the dam head and a boat left to him (the hard road) | `ch_house_rival_end_ruin` |
| 12 | `harbour.chain_house_rival.end_feud` | Dorestad, forty nights on | under Saint Martin's hand / his hull by the strong hand / stand alone | `ch_house_rival_end_feud`, `feud_house_thiadbod` |
| 13 | `harbour.chain_house_rival.s13_heirs` | Dorestad, 826-862, for the heir; drawn | a marriage and the old price paid (240 d) / pledge faith and take his price / keep it | `ch_house_rival_heirs`; `ch_house_rival_end_feud_healed` or `_feud_kept` |

**No dead ends.** Every card has a road that costs only time or pride (drink, Traiectum, lie inside, mend her, ask about the bar, the porch, "it stands", take the men off, and at the reckoning either fellowship or the steward's sale is always open). A failed chance hurts and goes on: a failed oath-charge is a lost case and the story continues at the widow's; a failed float still saves the men. Step 5 is optional and step 6 has two roads that need nothing from it. The one danger to life warns first: "the flood is making".

**Roads through the court.** Raise the cry, or hear Meinward before witnesses, and you may charge Thiadbod: his oath fails (200 d) or holds (a lost case). Bring Meinward in and the law gives you enmity and no silver; the silver comes at step 7, a pound for a man's friendship, if you will take it.

## The endings

| Ending | Flag | How it is reached | What becomes of the people and the place |
|---|---|---|---|
| **Partnership** | `ch_house_rival_end_partner` | You took his men off the bank and offered your hand. | Two houses make one reckoning of two boats and bear each other's fire and wreck. Without the oath it is lawful. With the oath it is a sworn band, forbidden since 779 (`ch_house_rival_sworn_band`: the count knows). On the hard road the chest is opened to every boat on the dams. |
| **Ruin** | `ch_house_rival_end_ruin` | His ship was lost, and you let the law and his losses run. | The steward sells his dam head. Thiadbod goes up the Rhine to the Frisians' shore at Mogontia to carry other men's wine; his son remembers (`ch_house_rival_sons_remember`). The house is broken, not ended: his sister's storehouse stands, and the spine has his heirs at Dorestad until 863. |
| **Feud** | `ch_house_rival_end_feud`, with the lasting cost `feud_house_thiadbod` | You said before the court that there is no peace. | Both houses go armed; each has peace only in church, at home and on the road to church and court. The count is angry, the bishop may be pleased. The feud outlives both men. |
| Feud, settled by the heirs | `ch_house_rival_end_feud_healed` (and `feud_house_thiadbod = 0`) | Card 13, a generation later. | A marriage between the houses, or a price taken with pledged faith; or the feud is kept (`ch_house_rival_feud_kept`) for the next heir. |

**What really happened, and what the player changes.** The chain rests on law and custom, not on one recorded event. Ruin is the ordinary end of a shipman who loses a ship laden on trust, and the feud runs exactly as the Lex has it: nothing is changed by either. Two things are history changed by the player:

- `changed_lex_frisionum_ship` (card 6, bringing the doer into the ring): the lawman judges that "a boat at her post is a house on the water, and who looses her by night pays as for a house burned". The real Lex Frisionum has no word on ships or boats. If this falls before the autumn of 802 it can go to Aachen with the rest of the Frisians' law; after that it is the custom of the Dorestad dams. What follows: along the Frisian coast a cut rope and a plundered wreck are paid twofold, so later cards may make wreckers, rope-cutters and salvors answer to it (the flood of 838, the raids from 834, any wreck card).
- `changed_dorestad_fellowship` (the hard road of the partnership ending): a common chest of the Dorestad shipmen against fire and wreck, a penny a voyage, with no oath, as the capitulary of 779 allows. Nothing like it is recorded at Dorestad; the first merchants' company on the river is Tiel's, about 1020. What follows: when the fleets come from 834 the chest can ransom, rebuild and warn; when the river leaves Dorestad after 863 the shipmen move as a body to Tiel and Daventre, and the sworn merchants of Tiel in 1020 have a founder to name. Cards of dorestad_834, the move after 863 and the Tiel years may read the flag.

## Payouts

Scale (CHAINS.md): step 0-40 d, turn 80-250 d, finale 600-2,500 d with a grand reward, hard road up to twice with a second grand reward.

| Card | Option | Silver and goods | And |
|---|---|---|---|
| 1 | short | +16 d | shuts the partnership's hard road |
| 1 | honest | | deed fair_dealing; Frisian cloth x1.1 at Dorestad for 21 days |
| 1 | drink | | a true price word, a week old: wine x1.15 at Lundenwic for 25 days; his feeling +8 |
| 2 | steward | | likely: toll eased 45 days, the steward's feeling +6 |
| 2 | heaped | -12 d | an order: 8 sacks of rye at 30% over, 60 days; deed fair_dealing |
| 2 | Traiectum | | a price word: rye and barley x1.3 at Traiectum for 30 days |
| 3 | carry | +30 d | toll eased 60 days |
| 3 | law | | likely: the berth back, heart up |
| 4 | cry | -30 d | witnesses for the court |
| 4 | pledge | -30 d; likely 2 casks of wine (about 120 d) | |
| 4 | mend | | the wright's feeling +6 |
| 5 | take / word / ask | word: -6 d | the doer held; or his word witnessed; or the pilot's feeling +6 |
| 6 | produce | | deed won_case; lawful enmity; history changed |
| 6 | charge / bare | likely / doubtful: **+200 d** (turn) | deed won_case, or lost_case |
| 6 | porch | +60 d | |
| 7 | price | **+120 d**, or **+240 d** under lawful enmity (turn) | |
| 7 | room | an order: 8 bolts of Frisian cloth at 35% over, 90 days (turn) | Thiadburg's feeling +14; opens "his sister" at the sale |
| 8 | salvage | 4 casks of wine (about 240 d) (turn) | shuts the partnership's hard road |
| 8 | float | doubtful: +120 d | his ship saved: opens the hard road |
| 10 | agreement | **700 d** | `unlock_partner_thiadbod`; a text (Havamal 47) |
| 10 | oath | **900 d** | `unlock_partner_thiadbod`; cost `ch_house_rival_sworn_band`; the text |
| 10 | open (hard road) | **1,400 d** | `unlock_partner_thiadbod` and `unlock_pilot_maas`; `changed_dorestad_fellowship`; deed gave_alms; the text |
| 11 | dam head | **450 d and 3 casks** (about 630 d) | `unlock_plot_dorestad` (and `ch_house_rival_thiadbods_dam`); cost `ch_house_rival_sons_remember` |
| 11 | hull | **600 d** | `unlock_ship_north_sea_merchant_ship`; the same cost |
| 11 | his sister | **700 d** | `unlock_factor_dorestad` |
| 11 | mercy (hard road) | **1,100 d** | `unlock_plot_dorestad` and `unlock_factor_mogontia`; no grudge |
| 12 | Saint Martin | **700 d** | `unlock_title_st_martins_man`; toll eased 180 days; deed gave_alms; a text (Havamal 43); costs `feud_house_thiadbod`, `ch_house_rival_owes_martin_wax`, the count's feeling -20 |
| 12 | hull | **600 d** | `unlock_ship_north_sea_merchant_ship`; the text; cost `feud_house_thiadbod`, the count's feeling -30 |
| 12 | alone | **1,000 d** | `unlock_heirloom_cut_rope`; the text; cost `feud_house_thiadbod` |
| 13 | marry | -240 d | `unlock_kin_thiadbods_house`, `unlock_partner_thiadlef`; an order: 4 casks of wine at 40% over; the feud ended |
| 13 | pay | +300 d | the feud ended |
| 13 | keep | | the feud goes on |

**Why each grand reward is one a merchant of the time could have won.**
- A partner with a second boat: partnership in a venture is ordinary, and the capitulary of 779 shows agreements against fire and wreck.
- The Maas marks (`unlock_pilot_maas`): a pilot's knowledge passes to a pupil; the spine's Gerlef wants one before his eyes go.
- A dam head at Dorestad: the dams are parcels 18 m wide, held by houses and by Saint Martin's (`dossiers/dorestad.md`). If the house already has ground at Dorestad from another chain, this is its second.
- A factor at Mogontia: Frisians lived on Mainz's shore. A factor at Dorestad: a widow with a storehouse who lends on goods (the spine; the pattern is Frideburg of Birka).
- A sea-going ship: a stranded hull floated off and mended.
- Saint Martin's man: the church's men on Dorestad's banks were free of the king's dues, of lodging and of seizure (815). Not offered to a house that is Saint Martin's already (`ch_house_debt_st_martins_man`).
- The cut rope as an heirloom, and kin by marriage: tokens and settlements every feud leaves behind.

## How long it takes

From the first card to an ending: 70 + 60 (or up to 220) + 30 + 40 + 20 + 100 days, the wait at sea, then 12 + 40: **a little over a year at the very least, two to three years in ordinary play**, since each scheduled step waits for a call at Dorestad. The court and the endings come "in forty nights". The heirs' card is a generation later (from 826).

## Pictures

Each card has a writing entry `art.<card id>`.

1. Short cloaks: a dam at Dorestad, bright windy morning; a seller holds up a blue cloak that ends at his knee; a tall Frank measures it against himself; Thiadbod in a long blue cloak watches the player's boat.
2. The king's measure: midday, grey sky; the thin old steward pours grain from one measure into another and counts on his fingers; Thiadbod with open hands.
3. The dam head: evening, low sun; a big merchant ship moored across the mouth of a ditch, shutting in a small boat; Thiadbod coming down a plank with a purse.
4. The cut rope: first light, mist; a small boat on old black posts, half full of water; a hand holds the cut end of a rope; a boy points down river.
5. The man with the split ear: low tide at the Maas mouth; an old pilot with a sounding pole at the door of a net shed; a young man with a split ear inside.
6. The count's court: noon by a small timber church; a ring of men; the count seated with a sword across his knees; an old Frisian with one hand raised; Thiadbod and five men; a kettle steaming at the edge.
7. Thiadburg's storehouse: light through the plank gaps; bales and casks; Thiadburg with her keys weighs silver on a folding scale.
8. A ship on the bank: a big merchant ship heeled on a sandbank in a rising grey sea; nine men on her high side, a boy among them; a burst cask showing straw and iron.
9. The reckoning: the dam head in flat late light; Thiadbod bareheaded, his rings gone, his sister beside him; strangers from up the river sitting on his casks.
10. Ending, partnership: the church porch on a clear cold morning; two men clasp hands over a split tally; the priest looks away; a Dane leans on the fence; two boats side by side below.
11. Ending, ruin: the dam head in light rain; the steward on a cask with his notched sticks; Thiadbod and his son at the back with a bundle each, a barge waiting.
12. Ending, feud: the court ring under a hard bright sky; two groups of men stand apart; the count on his feet; the old Frisian between them with both hands raised.
13. The heirs: the church door on a still autumn morning; two people of middle years face each other on the step, unarmed; over a house door in the distance hangs an old grey rope.

## Flags other writers may read

- Progress: `ch_house_rival_1` to `ch_house_rival_9`. Endings: `ch_house_rival_end_partner`, `ch_house_rival_end_ruin`, `ch_house_rival_end_feud`, `ch_house_rival_end_feud_healed`.
- Lasting: `feud_house_thiadbod` (1 while the feud stands), `ch_house_rival_sworn_band`, `ch_house_rival_sons_remember`, `ch_house_rival_owes_martin_wax`, `ch_house_rival_thiadbods_dam`.
- What the player knows: `ch_house_rival_saw_blades` (he has seen sword blades in Thiadbod's wine casks: for the swords_north thread).
- History changed: `changed_lex_frisionum_ship`, `changed_dorestad_fellowship`.
- Read from others: `db_bought` (spine, counts_day); from chain house_debt `ch_house_debt_cleared`, `ch_house_debt_owed`, `ch_house_debt_end_friend`, `ch_house_debt_count_enemy`, `ch_house_debt_st_martins_man`, `unlock_title_doomsman_dorestad` (one line each; nothing breaks if they are never set). Set for the spine: `hy_new_measure` (hungry_years: the player's measure tried against the public modius).

## Needs (not added by this writer)

- Named rows in `encounters/npc_roles_793.json` so the forge gives the cast their names: Thiadbod as the fixed skipper of Dorestad (his heir Thiadlef after 825), Thiadburg as its widow, Folcbert its count, Hartwin its steward, Gerlef the pilot of Witla. Until then the speaker label shows a forged name beside words that say "Thiadbod".
- An outcome text on a choice. The engine shows only the effects' facts; every option here has a written outcome (`chain_house_rival.<card>.out_<option>`, with `_success` and `_failure` for a chance), named in the option's `note`.
- Two deeds: one for sharp dealing (a short measure, short cloth) and `saved_lives` (the encounter design's sample 14 asks for it): cards 1 and 8 cannot move standing for these.
- The sea draw must be live for card 8 (`rate` 0.04 an hour under way). A scheduled card is opened without its `when` being looked at, so no harbour fallback was added: it would open a second time.
- The other house chains draw at weights of 30 and 40; this chain draws at 1.0 to 2.0 as the engine design has it, and will wait behind those on the same call.
- The grand-reward flags to wire: `unlock_partner_thiadbod`, `unlock_partner_thiadlef`, `unlock_pilot_maas`, `unlock_plot_dorestad`, `unlock_factor_dorestad`, `unlock_factor_mogontia`, `unlock_ship_north_sea_merchant_ship`, `unlock_title_st_martins_man`, `unlock_heirloom_cut_rope`, `unlock_kin_thiadbods_house`.
