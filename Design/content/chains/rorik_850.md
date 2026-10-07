# Chain: rorik_850 (the Dane who holds Dorestad)

Written 6 Oct 2026. Cards: `Assets/Kapman/Game/Data/encounters/chain_rorik_850_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_rorik_850.json`. A draft for the user to overwrite.
(?) marks what could not be checked against a source. "Ours" marks what the sources do not say and the chain adds.

## The story in five sentences

In the summer of 850 the fleet that burned the Betuwe ties up at Dorestad as the count's fleet: the emperor Lothar could not beat Rorik the Dane, so he made him his man, on his oath to pay in the king's dues and keep other Danes off the river.
A heathen now takes the king's tenth in a Christian port and calls its free men in to put their hands between his, and each house must choose: his man, the emperor's man only, or nobody's.
For some years it works; then he sails north after a kingdom (855) and after land by the Eider (857), other Danes storm the town while he is gone, and the houses ask what the tenth was for.
He takes baptism, and in the flood winter of 863 a greater fleet burns Dorestad and the estate where the merchants had fled, then goes home "on Rorik's advice", and a bishop is told to find out what that advice was.
The house ends as the count's man with a ship and his mark on her stem, as the one who took the merchants out by boat and changed what the annal says, as the bishop's witness with Saint Martin's bank free, as the friend of the coast men who drive the Dane out in 867, or as nobody's man with a storehouse on the IJssel.

## How a player meets it

Step 1 is drawn on a call at Dorestad between July 850 and the end of 856, with a weight of 8 (close to sure on the first call). It is the toll man: he is a Dane now. Nothing else is needed: the heir of the house in those years meets it by going home. Step 2 follows at Dorestad a month or more later. After step 2 no step is a gate: steps 3 to 10 are roads that come in their true years if the boat is at the right port, and step 14 (867 to 870, at Traiectum, Daventre, Noviomagus, Medemelacha, Winaldum or Doccinga) asks only for step 2 and ends every road that is still open. The news item `chain_rorik_850.news.grant` on Dorestad's board tells a player who asks what the terms are.

## The people

| In the cards | Spine id | Who |
|---|---|---|
| the count | `cast.rorik` | Rorik, a Dane of the royal kin; real. Heathen in 850, baptised by about 863. |
| the toll man | `cast.eystein` | Eystein, Rorik's man at the toll; invented (the spine's). |
| the bishop | (none; Traiectum's dossier) | Hunger, bishop of Utrecht 854 to 866; real. |
| the merchant | (none) | Thiadward, a grandson of `cast.thiadbod`; invented (Thiad-, -ward). He speaks for the houses of the bank. |
| the priest | (none) | Adalger, priest of a church at Dorestad; invented (Adal-, -ger). |
| the lawman | (none) | Hiddo, who speaks for the Cokingi; invented (?). |
| named in lines only | | Godfrid, Harald's son (AB 855); King Lothar II; the boy king Horik II; Archbishop Hincmar of Reims. |

Roles used: tollman, count, merchant, priest, steward, moneyer, skipper, bishop, clerk, lawman. All are in `npc_roles_793.json`.

## The real history under it

- **Before 850.** Rorik and his brother (or uncle) Harald held Dorestad in benefice under Louis the Pious; under Lothar he was accused of treason, held, escaped to Louis the German and lived among the Saxons, then gathered a fleet (Annales Fuldenses 850).
- **850.** "Rorik, the nephew of Harald ... raised whole armies of Northmen with a vast number of ships and laid waste Frisia and the island of Batavia and other places along the Rhine and the Waal. Lothar, since he could not crush him, received him into his fealty and granted him Dorestad and other counties" (Annales Bertiniani 850). He was taken back on condition that he handle the taxes and the king's dues faithfully and resist the Danish pirates (Annales Fuldenses 850). Both in our rendering; `chronicle_793.json` 850-rorik-dorestad.
- **About 850 on.** The last pennies of Dorestad: poor coins in Lothar's name, probably struck under Rorik (`dossiers/dorestad.md`, coin; Coupland 1988 (?)). Step 3.
- **854-855.** King Horik and most of the Danish royal house fall in a civil war (AB 854, AF 854). In 855 Lothar gives Frisia to his son Lothar II; "Rorik and Godfrid headed back to their native Denmark in the hope of gaining royal power", fail, and are back at Dorestad the same year, holding most of Frisia (AB 855). Step 4.
- **857.** Rorik, "who ruled in Dorestad", takes a fleet north with King Lothar's leave, and with King Horik's consent holds the land between the sea and the Eider (AF 857). While he is away "other Danes stormed the emporium called Dorestad and laid waste the whole island of Batavia" (AB 857). Utrecht is wasted; Bishop Hunger and his clergy leave it, and in 858 Lothar II gives them the monastery of Berg on the Roer; tradition has them at Deventer between (`dossiers/traiectum.md`; the year of the flight is disputed (?)). Steps 5 to 9.
- **The church at the Slie.** Built with Horik's leave about 850, shut after 854, opened again under Horik II with a bell (Rimbert, Vita Anskarii c. 24, 31, 32; the year of the reopening is not given (?)). Step 6.
- **Baptism.** Known only from Archbishop Hincmar's letter "to Rorik the Northman, converted to the faith of Christ", of about 863 (Flodoard, Historia Remensis ecclesiae III.26 (?)). When, where and by whom is not written; the chain puts it at Dorestad between 859 and 862 (ours). The story of the Northman and the baptismal robe is Notker's (Gesta Karoli II.19), retold in our words. Step 10.
- **January 863.** "The Danes sailed up the Rhine towards Cologne, after sacking the emporium called Dorestad and also a fairly large villa at which the Frisians had taken refuge, and after slaying many Frisian merchants and taking captive large numbers of people"; they came to an island near Neuss; Lothar came up on one bank and the Saxons on the other; "on the advice of Rorik they withdrew by the way they had come" (AB 863). A winter of floods (Annales Xantenses (?)). `chronicle_793.json` 863-dorestad-last. Steps 11 and 12.
- **About 863.** Hincmar writes to Bishop Hunger: if the Danes acted by Rorik's counsel, lay a fitting penance on him; and to Rorik: give no counsel or aid to pagans against Christians, and do not shelter Baldwin, who carried off the king's daughter Judith (Flodoard III.23, III.26 (?)). What Hunger found is not written. Hunger died in 866. Step 13.
- **867.** Lothar II calls out his host because "Rorik, whom the local people, called by a new name Cokingi, had driven out of Frisia, would return bringing some Danes to help him" (AB 867). Who the Cokingi were is not known. **870**: Charles the Bald meets Rorik at Nijmegen and binds him by a treaty (AB 870). **873**: he swears to Louis the German (Annales Xantenses 873). **882**: what he held goes to Godfrid (AB 882). Step 14.

Ours, and marked so in the card notes: that Rorik took the hand-oath of Dorestad's free men; that the houses sent a complaint to Lothar II; the standing at the dam and the peace bought by the house in 857; the timber and the eased toll for those who rebuilt; the bishop's use of a merchant as a go-between; that the flood let the ships reach the villa; the toll chest carried out beforehand; the penance; the pilot's marks of the Vlie.

Sources: Annales Bertiniani 850, 854, 855, 857, 863, 867, 870, 882; Annales Fuldenses 850, 854, 857; Annales Xantenses 863, 873 (all as summarised in `Design/content/spine.json`, `realms/frisia_lotharingia.md`, `dossiers/dorestad.md`, `dossiers/traiectum.md`, `dossiers/noviomagus.md` and at gjallar.nl and en.wikipedia.org "Rorik of Dorestad", read by the dossier writers 4 Oct 2026; the wording here is our own rendering, not a quotation from a modern translation); Rimbert, Vita Anskarii (tr. Robinson 1921); Notker, Gesta Karoli (tr. Grant 1907); Flodoard, Historia Remensis ecclesiae III (MGH SS 13) (?). No web search was used for this chain.

## The steps

All cards are `harbour` stage, `once`, drawn by their `when` (none is scheduled). Flags are `ch_rorik_850_<x>`; only the part after the prefix is given below.

| # | Card id | Where and when | The choices and their flags | Picture |
|---|---|---|---|---|
| 1 | `harbour.chain_rorik_850.s01_dane_at_the_dam` | Dorestad, July 850 to 856 | Give your name (half toll 30 days) / ask the terms (news: the grant) / say you will go round by the Waal (`waal`). All: `1`, wait 30 days. | The Dane at the toll house door, nine long ships with masts down behind. |
| 2 | `...s02_hands` | Dorestad, 30 days on, before 863 | Hands between his (`sworn`, spine `df_rorik_man`, half toll a year) / the emperor's peace on relics (`kings_man`) / swear nothing (`refused`). All: `2`, wait 60 days. | The count on a bench holding out open hands; a merchant on the mat; a priest with a relic box. |
| 3 | `...s03_light_pennies` | Dorestad, 60 days on, before 857 (side road) | Take 96 light pennies by tale (72 d, half toll 60 days) / ask for the scale (likely: 96 d; else 72 d and the steward cooler) / take a timber of marten instead. All: `3`. | A steward tipping thin pennies onto a hatch; a moneyer with a balance. |
| 4 | `...s04_kings_sons` | Dorestad, spring to autumn 855 (side road) | Victual the fleet (order: 30 sacks of ship's bread in 30 days at 1.8; `victualled`) / put your mark on the houses' word to Lothar II (`complaint`) / wish him a fair wind (price word). All: `4`. | The fleet being stored; the count with one foot on a gunwale. |
| 5 | `...s05_fleet_goes_north` | Dorestad, March to June 857 | Follow the fleet north (sworn only; `north`) / stay and bury the silver (`stayed`, spine `df_buried`) / go upriver (`away`). All: `5`. The warning before the danger. | The fleet rowing away; the toll man left with a bailing scoop and one old ship. |
| 6 | `...s06_between_sea_and_eider` | Sliesthorp, 857 to 858, if `north` | Buy marten and amber under his peace, 110 d for 240 d, and begin Danish / ask for his token (`token`) / hear the news and go home (`heard`). All: `6`. | The count on a sea chest on the strand at the Slie; a church with a bell behind. |
| 7 | `...s07_other_danes` | Dorestad or Traiectum, July 857 on, if not `north` | Carry the priest and the books to Daventre (120 d freight, Latin; `church_friend`) / stand at the dam (doubtful: 180 d; else 60 d and a hurt; `stood`; deed fought_raiders) / buy your storehouse free for 60 d (`paid`; deed paid_raiders). All: `7`. | Many sails below the town at dusk; clerks carrying a chest to a boat; men on a staked dam. |
| 8 | `...s08_count_comes_home` | Dorestad, October 857 to 861 | Speak for him (120 d; `spoke`) / carry the complaint to Noviomagus (90 d; `complaint`; deed carried_letters) / mend your dam head and keep quiet (half toll 120 days). All: `8`. | Burned posts and new planks; the count facing merchants with folded arms. |
| 9 | `...s09_bishop_without_church` | Daventre or Traiectum, 858 to 861 (side road) | Carry the bishop's letter and white cloth (30 d, Latin; `letter`) / give counsel only (`counsel`) / decline (price word: wax). All: `9`, wait 30 days. | The bishop at a trestle in a borrowed room with a letter and white linen. |
| 10 | `...s10_white_robe` | Dorestad, 859 to 862, after step 9 | Stand witness, 40 d of wax (half toll 180 days; `font`) / ask Eystein what a robe is worth (Danish) / stay outside with the heathen men (`old_way`). All: `10`. | The count in his shirt by a tub in a church; armed backs in the doorway. |
| 11 | `...s11_flood_winter` | Dorestad, November 862 to February 863 | Take the houses out by boat to Daventre (likely: `saved`; else `saved_few`; spine `df_warned`) / as his man, ask the count to stand (sworn only; `asked`) / watch from the reeds (`saw`). All: `11`. | Flooded fields and ox carts; a merchant sounding the flood with an oar. |
| 12 | `...s12_on_roriks_advice` | Traiectum, Daventre, Noviomagus or Dorestad, March 863 to 866 | **Ending saved** (needs `saved`) / **ending sworn** (needs `sworn`) / take nothing yet. All: `12`. | The count at a board with a chest of pennies; a burned town far off; merchants apart. |
| 13 | `...s13_what_the_bishop_asks` | Traiectum or Daventre, 863 to 865, after step 12 with no ending | **Ending witness**, the full account (needs `saw` or `asked`) / **ending witness**, the plain account / say nothing (`silent`). All: `13`. | The bishop with a letter and a list; a merchant with his hand on a relic box. |
| 14 | `...s14_cokingi` | Traiectum, Daventre, Noviomagus, Medemelacha, Winaldum or Doccinga, 867 to 870, any road still open | **Ending cokingi** / **ending sworn**, the long road (needs `sworn`) / **ending own man**. | Coast men with eel spears on a terp; the toll man with two women, a boy and a chest by a boat. |

No dead ends: every card has an option that costs only time or pride (marked `default` where it is the plain one); a failed chance pays less and goes on by another flag (`saved_few`, the dam lost, the scale refused); the danger of 857 is warned in step 5 and that of 863 in step 11.

## The endings

Every ending sets `ch_rorik_850_done` and its own flag.

| Ending | Flag | How | What becomes of them | Picture |
|---|---|---|---|---|
| The count's man | `ch_rorik_850_end_sworn` | Sworn in step 2, and takes the count's reward after the flood winter (step 12), or carries his household off the coast in 867 (step 14, the long road). | **History as it was.** Dorestad burns, the fleet goes home on Rorik's advice, he keeps his counties and comes back from every fall until 873. The house has his peace and a ship from his bank. The church, or the coast, remembers whose man it was (`ch_rorik_850_cost_church` or `ch_rorik_850_cost_frisians`). | A broad Danish-built ship with three strokes cut in her stem; the count raising a hand on the bank; a clerk writing. |
| The houses brought out | `ch_rorik_850_end_saved` | Persuades the houses in the flood winter that the flood is a road for ships, takes them by boat to Daventre (step 11, a likely chance), and swears fellowship with them (step 12). | **History changed** (`changed_dorestad_863_merchants_saved`). The villa burns empty; no "many Frisian merchants" are killed. Six Dorestad houses stand together on the IJssel. | A new landing at Daventre, six house frames, merchants with their hands on one chest. |
| The bishop's witness | `ch_rorik_850_end_witness` | Takes nothing at step 12 and tells Bishop Hunger what happened (step 13): all of it if he watched or was told, or the plain account. | The count does penance at a church door. The sources are silent on whether he did: this is what Hincmar asked for. The house is Saint Martin's friend and Rorik's bad debt (`ch_rorik_850_cost_rorik`). | A church door in Lent; a big man in his shirt; widows going in past him. |
| The coast's man | `ch_rorik_850_end_cokingi` | Carries bread and word for the Cokingi in 867 (step 14). | **History as it was**, from the other side: the Dane is driven out, and is back by 870. The coast trusts the house; Rorik does not (`ch_rorik_850_cost_rorik`). | A boat in the Vlie; a pilot pointing at a church, a dune and two withies. |
| Nobody's man | `ch_rorik_850_end_own_man` | Sells to both in 867 and moves the goods to Daventre (step 14). | The house is out of the quarrel and out of Dorestad (spine `df_moved`). Neither side thanks it; neither hunts it. | A storehouse door being barred at Daventre; a boat rowing away from both sides. |

### What follows from history changed

If `changed_dorestad_863_merchants_saved` is set, the annal for 863 loses a line: the fleet still burns Dorestad and the estate, still goes up to Neuss and still leaves on Rorik's advice, but the Frisian merchant houses are alive. Later cards can read the flag for these:

- Daventre from 863 has Dorestad's names on a whole row of plots at once, not a trickle of survivors, and a sworn fellowship of merchants a hundred and fifty years before Tiel's (Alpert of Metz, about 1020, describes Tiel's merchants as sworn drinkers and judges of their own cases). When Daventre is burned in 882, they have a common chest to build again.
- The house has a partner, Thiadward, who sails a second boat (`unlock_partner_thiadward`), and the spine's rival house of Thiadbod is bound to it by a debt of life rather than a grudge.
- Rorik is not the lord under whom the merchants died. Hincmar's question (step 13) has less behind it, and the Cokingi of 867 have one grievance fewer: a later writer may make that rising smaller or later.
- Dorestad itself is not saved. The river is leaving it, and the port table ends it in 863 either way.

## Payouts

Values by `goods_793.json`: marten 120 d a timber, amber 30 d a sack, Frisian cloth 48 d a bolt.

| Where | Tier | Silver or worth | And |
|---|---|---|---|
| Step 1 | Step | 0 | half toll 30 days, or the grant as news, or Thiadward's good will |
| Step 2 | Step | 0 | half toll for a year (sworn), or the priest's good will |
| Step 3 | Step | 72 to 96 d, or marten worth 120 d | half toll 60 days; a price word on furs |
| Step 4 | Turn | an order for 30 sacks of ship's bread at 1.8 times the price | or the houses' trust; or a price word on bread and ale |
| Step 5 | Step | 0 | a price word for Sliesthorp; the silver safe in the ground |
| Step 6 | Turn | goods worth 240 d for 110 d | the Danish tongue begun (0.5); or the count's token |
| Step 7 | Turn | 120 d, or 180 d (60 d if beaten), or the storehouse kept for 60 d | Latin +0.1 and the priest's friendship; or the deed fought_raiders |
| Step 8 | Turn | 120 d, or 90 d | or half toll 120 days |
| Step 9 | Step | 30 d | Latin +0.1; deed carried_letters; or a price word on wax |
| Step 10 | Step | 0 (40 d given) | half toll 180 days; or the Danish tongue |
| Ending sworn, step 12 | Finale | 900 d | `unlock_title_roriks_peace`, `unlock_ship_norse_allround_ship` |
| Ending sworn, step 14 | Finale, hard road | 1,300 d | `unlock_title_roriks_peace`, `unlock_ship_norse_allround_ship` |
| Ending saved | Finale, hard road | 1,500 d and 10 bolts of cloth (1,980 d) | `unlock_partner_thiadward`, `unlock_plot_daventre` |
| Ending witness, full | Finale, hard road | 1,100 d | `unlock_charter_traiectum`, `unlock_title_saint_martins_man`, Latin +0.2 |
| Ending witness, plain | Finale | 700 d | `unlock_charter_traiectum` |
| Ending cokingi | Finale | 800 d and 8 bolts of cloth (1,184 d) | `unlock_pilot_vlie` |
| Ending own man | Finale | 600 d | `unlock_factor_daventre` |

Why a merchant could have won each:

- **Rorik's peace and a ship.** A lord answered for his sworn men; Rorik's fleet was the only one on the river that others feared, and the Danes of 863 took his advice. A ship from a fleet's bank is a lord's gift in kind. The class is the Norse all-round ship of `ship_classes_793.json`.
- **A partner and a plot at Daventre.** Dorestad's trade did go to Deventer and Tiel after 863 (the spine, thread dorestad_falls). Sworn fellowships of merchants are attested on this river by about 1020 (Tiel); a common chest among six rescued houses is a small early one (ours).
- **A charter at Traiectum and the saint's protection.** The church of Utrecht held its own bank and a tenth of the king's tolls by royal grants, and its men traded under Saint Martin's name (`dossiers/traiectum.md` (?)). A bishop could free a friend's boat from his own toll.
- **The marks of the Vlie.** There are no charts: a channel is known by a pilot's marks, kept in families. To be shown them is the coast's own kind of charter.
- **A factor at Daventre.** A kinsman of a Dorestad house who keeps another's goods for a share: the ordinary arrangement of the houses that moved.

Lasting costs (flags a later card may read): `ch_rorik_850_cost_church` (the bishop's clerk has the house on a list), `ch_rorik_850_cost_rorik` (the count "remembers like a king"; he is on the river until 873), `ch_rorik_850_cost_frisians` (the coast does not sell to the house for a generation).

## How long it takes

From the summer of 850 to the spring of 863 at the soonest (the saved or sworn ending at step 12): about thirteen years of game time, with long gaps for ordinary trade. The bishop's ending falls in 863 to 865. The last endings fall in 867 to 870: seventeen to twenty years. The waits inside it are real: 30 days between steps 1 and 2, 60 days after step 2, 30 days after step 9, and otherwise the true years (855, 857, the winter of 862-863, 867). A player who comes to Dorestad for the first time in 856 can still begin; one who misses a year loses that year's card and no more.

## Flags

- Progress: `ch_rorik_850_1` to `ch_rorik_850_13`, `ch_rorik_850_wait` (set for a number of days), `ch_rorik_850_done`.
- Branches: `_waal`, `_sworn`, `_kings_man`, `_refused`, `_victualled`, `_complaint`, `_north`, `_stayed`, `_away`, `_token`, `_heard`, `_church_friend`, `_stood`, `_paid`, `_spoke`, `_letter`, `_counsel`, `_font`, `_old_way`, `_saved`, `_saved_few`, `_asked`, `_saw`, `_silent`.
- Endings: `ch_rorik_850_end_sworn`, `ch_rorik_850_end_saved`, `ch_rorik_850_end_witness`, `ch_rorik_850_end_cokingi`, `ch_rorik_850_end_own_man`.
- History changed: `changed_dorestad_863_merchants_saved`.
- The spine's: `df_rorik_man` (step 2), `df_buried` (step 5), `df_warned` (step 11), `df_moved` (endings saved and own man).
- Grand rewards: `unlock_title_roriks_peace`, `unlock_ship_norse_allround_ship`, `unlock_partner_thiadward`, `unlock_plot_daventre`, `unlock_charter_traiectum`, `unlock_title_saint_martins_man`, `unlock_pilot_vlie`, `unlock_factor_daventre`.

## Needs (for the integrator; nothing was added to a shared table)

- The game must read the new grand-reward flags `unlock_title_roriks_peace`, `unlock_partner_thiadward` and `unlock_pilot_vlie` (the others already occur in other chains).
- `ch_rorik_850_token` (the count's token at the stem) is set and has no effect yet: a sea card with Danish raiders between 857 and 873 could read it.
- The cards at Dorestad in January and February 863 (step 11) and after (step 12 lists Dorestad among its ports) need the port to be callable to the end of 863; `ports_793.json` has `"to": 863`. If the port closes at the start of that year, step 11 still comes in November and December 862.
- The engine has no verb to pay a later sum on a condition, so nothing is lent to the count and repaid; if such a verb comes, step 4 could have a loan.
- People for the places files, if their writers want them: `dorestad.thiadward` (merchant, Frisian, born about 818, grandson of cast.thiadbod), `dorestad.adalger` (priest, born about 805), `medemelacha.hiddo` (lawman, born about 815). Bishop Hunger (854 to 866) belongs in Traiectum's and Daventre's people.

## Not verified

- Flodoard's chapter numbers for Hincmar's two letters (III.23 to Hunger, III.26 to Rorik), and the letters' date of about 863.
- The year, place and minister of Rorik's baptism (the chain: Dorestad, 859 to 862).
- Whether Hunger left Utrecht in 857 and stayed at Deventer before 858.
- The flood of the winter of 862-863 as the Annales Xantenses give it, and that it let ships reach the villa.
- The weight of the last Dorestad pennies (the game's three quarters).
- The year the church at the Slie was reopened and given a bell (about 856).
- That Harald's commendation of 826 was by the hands (Ermoldus), and so that Rorik would take oaths in that form.
- The name Hiddo for the 860s; who the Cokingi were.
- That the church of Utrecht could free a merchant from toll on its own bank at Traiectum in the 860s.
