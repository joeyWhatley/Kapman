# Audit: `ship_classes_793.json` and `texts_793.json`

Audited 6 October 2026. Neither table was edited. Verdicts: **right** (agrees with a source I opened, or with one
already opened and cited in `Design/content/ships.md`), **wrong** (with the correction and its source), **doubtful**
(with why). `(?)` marks what I could not check. Web searching was not available in this session (the budget was
used up), so everything was checked by fetching named pages and against the project's own scratch copies of the books.

How deep the check went, so nobody trusts it further than it reaches:

- **Ships:** every row read; the finds' sizes and dates checked against the pages listed under Sources. The replica
  speeds were taken from `ships.md` section 5 (Viking Ship Museum pages opened 4 October by its writer); I re-opened
  only the Langoe (Gislinge) page. Speeds with no replica behind them are estimates and are marked doubtful only where
  a measured figure contradicts them.
- **Texts:** all 267 entries had their source, translator, year and death year checked. The wording of the text itself
  was read in full for the single entries and spot-checked for the long uniform series (Gestumblindi, the Edda
  stanzas, the Exeter riddles, Beowulf): the first and last entry and three or four between. A row marked right in a
  series means "the credit is right and the sample agreed", not "every letter was collated".

The rule the texts table sets itself: translator died in 1955 or before, translation printed before 1931.

---

## Part 1. Ships: `ship_classes_793.json`

Eighteen classes. One row per class and group of fields.

| Class | Fields | Verdict | Note, correction and source |
|---|---|---|---|
| `gislinge` | length_m 7.7; oars 6; built c. 1130; found 1993 in the Lammefjord | right | Viking Ship Museum (VSM), Langoe page: 7.70 m, 6 oars; Gislinge project page: 1130, 7.7 m. |
| `gislinge` | beam_m 1.5 | wrong | **1.64** (VSM Langoe page, the 2017 reconstruction: "Width: 1.64 meter"). Every `placeholder.beam_x` in the table is the class beam / 1.5, so all eighteen must be recomputed as beam / 1.64 if this changes. |
| `gislinge` | draught_m 0.25, marked (?) | right | VSM Langoe: "Depth/draft: 0.25 meter". Take the (?) off. |
| `gislinge` | crew 4; crew_range "2-5" | wrong (the range) | Crew 4 is the user's decision and inside the museum's range. The range is **"2-4"** (VSM Langoe: "Crew: 2-4 men"). |
| `gislinge` | cargo_t 0.9 "beyond crew and gear" | wrong | The museum's "about 1 t" is the whole load, "four men and ten sheep", so it includes the men. BRIEF.md says 660 kg. Use **0.66** in both places (1,000 kg less four men at 85 kg) and say so in `evidence`. |
| `gislinge` | speed_max_kn 6.5; speed_oars_kn 3.5 | wrong (the maximum) | VSM Langoe: "Estimated maximum speed, sail: 6 knots", "oars: 5 knots". Set speed_max_kn **6**. 3.5 as a cruising speed under oars sits well under a 5 kn maximum. |
| `gislinge` | speed_sail_kn 4.8; closest_to_wind_deg 58; polar_factor 1; freeboard 0.34; hull_top 0.8; sail_top 5; rowing_hours 6; gale_fragility 0.6 | doubtful | The game's own asset values; no measured figure exists for this boat. The museum gives a sail of 10 m2, which the table does not record. Keep them, but they are design numbers, not finds. |
| `gislinge` | evidence "used about 50 years" | doubtful | Not on either museum page I opened (?). |
| `norse_small_boat` | Kvalsund I 9.5 m, 780-800 (Nordeide, Bonde and Thun 2020) | right | en.wikipedia Kvalsund ship (9.5 m; tree-ring date 780-800, the 2020 study). A mast was found with it, ownership unclear: mast_share 0.4 is a fair guess. |
| `norse_small_boat` | Gokstad faering 6.5 m; Skuldelev 6 11.2 x 2.5 m, c. 1030; Gislinge c. 1130 | right | VSM pages, via ships.md 2.3. |
| `norse_small_boat` | Valsgarde and Vendel, Reric, Staraya Ladoga lines | doubtful | Not opened (?). Plausible; no page read. |
| `norse_small_boat` | 8 x 1.6 m, crew 4, cargo 0.8 t, speeds as the Gislinge | right | Inside the finds' range (6.5 to 9.5 m). speed_max_kn should follow the Gislinge down to 6. |
| `logboat` | sizes, speeds, no sail | right | Typical of the catalogues; marked as all-period in `uncertain` already. |
| `logboat` | period_name "OE aesc or bat (?)" | doubtful | OE aesc in the Chronicle (896) is the Danes' warship, not a dugout. Drop "aesc" here; it is already on the warship rows. |
| `logboat` | evidence: Welham Bridge, Callow; "Lanting 2000" | doubtful | Not opened (?). Lanting's survey of logboat dates is in Palaeohistoria 39/40 for 1997/98 (?): check the year. ships.md adds: 28 early medieval logboats reported in Britain, none from a dig context. |
| `logboat` | Constantine VII, c. 950: monoxyla sold to the Rus | right | De Administrando Imperio ch. 9. LATER, as marked. |
| `logboat_expanded` | uncertain: "no Utrecht-type hull securely dated to the 8th c" | wrong | There is one: **Vleuten 1**, felled after about 734, about 15 x 1.10 m (Brouwers, Jansma and Manders 2015, in ships.md 2.1). Add it to `evidence`; strike the line from `uncertain`. |
| `logboat_expanded` | length 10, beam 1.5 | doubtful | That is the Weser boat (Eke). The Rhine one is longer and narrower (15 x 1.1). For Dorestad's river, 13 x 1.2 lies between the two. |
| `logboat_expanded` | Utrecht 1: 17.45 x 3.84 m, 10-15 t, trunk at least 14 m, mast step by the bow, after 1019 | right | MASS, Utrecht 1 (opened today): all five figures as given. |
| `logboat_expanded` | Eke boat 10.25 x 1.4-1.5 m, 8th c | doubtful | Not opened (?). |
| `logboat_expanded` | towed_upstream_km_day 18 | wrong | **10 to 12 km a day** upstream (Brouwers, Jansma and Manders 2015, via ships.md 5.2). Use 11. |
| `logboat_expanded` | cargo_t 1.7 | right | The row's own arithmetic holds (10 x 1.2 x 0.35 x 0.6 = 2.5 t, less 0.8 t of hull). |
| `river_barge` | 16 x 3 m, cargo 7 t, towing mast | right | Krefeld-Gellep III and "Karl" as cited; right for 793. From about 826 the finds are 20 m and more (Cothen, Vleuten 2): a later class, not this row. |
| `river_barge` | Krefeld-Gellep "6,961 kg" | doubtful | The paper was opened neither by me nor for ships.md (?). |
| `river_barge` | Kalkar-Niedermormter "c. 805" | wrong | **About 802** (Brouwers, Jansma and Manders 2015). |
| `river_barge` | towed_upstream_km_day 18 | wrong | **11** (10 to 12), as above. |
| `river_barge` | Wandalbert, Miracula S. Goaris (c. 839); Thionville 805 | right | Both as I know the texts; LATER, as marked. Not re-opened (?). |
| `river_estate_ship` | RFA 792 (bridge of boats), RFA 793 (by ship from Regensburg to the ditch and on to the Main) | right | Annales regni Francorum, those years. |
| `river_estate_ship` | D.122, 27 March 779, Saint-Germain-des-Pres | right | MGH DD Kar. 1 no. 122. |
| `river_estate_ship` | Vita Anskarii ch. 7: "Bishop Hadebald of Cologne", a boat with two cabins, 826 | right | Rimbert ch. 7 (Robinson 1921). Hadebald was archbishop: write "Archbishop". |
| `river_estate_ship` | towed_upstream_km_day 20 | wrong | **12** (the top of the 10 to 12 range, for a lighter hull with more men). |
| `river_estate_ship` | size, crew 8, oars 8, speeds | doubtful | No find; marked (?) in the row already. |
| `skiff_punt` | Pactus legis Salicae XXI; IV Aethelred 2 (half-penny, penny, four pence) | right | Both as cited. |
| `skiff_punt` | all numbers | doubtful | No find behind any of them; the row says so. |
| `ferry_barge` | Einhard, Vita Karoli 17 (Mainz bridge, 500 paces, ten years, burnt 813) | right | Einhard ch. 17 and 32. |
| `ferry_barge` | Walenstadt and Sackingen 965; all numbers | doubtful | Not opened (?); no find. |
| `raft` | Bede, prose Life of Cuthbert ch. 3 | right | As cited (written about 721). |
| `raft` | speed_max_kn 3 with downstream_km_day 30; cargo_t 10 | doubtful | Estimates; the row says so. |
| `currach` | Adomnan; Dicuil ("duorum navicula transtrorum", two summer days and the night between); Gildas ("de curucis"); Broighter (mast, nine oars a side) | right | All four as cited. |
| `currach` | speed_sail_kn 4.5; polar_factor 0.94; speed_max_kn 6 | doubtful | Too fast on the beam for a keelless hide boat. ships.md 5.3 fits 3.5 kn abeam and 4.5 on the quarter; the row's own Dicuil sum gives 3.3 to 4.3 kn made good with a fair wind astern. Use speed_sail_kn **3.5**, polar_factor **0.73**; keep 6 as the most. |
| `currach` | closest_to_wind_deg 75 | right | As good a guess as any; she barely beats. |
| `gaelic_wooden_ship` | Senchus: two seven-benchers from twenty houses; 560 + 430 + 420 = 1,410 houses = 141 ships | right | The sums hold; the source as cited. |
| `gaelic_wooden_ship` | AU 719 Ard Nesbi; AT 729, 150 Pictish ships; St Orland's Stone | right | As cited. |
| `gaelic_wooden_ship` | oars 14 from seven benches; length 11; rig | doubtful | Derived; the row says so. |
| `west_small_boat` | Bede HE IV.13 (eel-nets, 300 fish); Barrett, Locker and Roberts 2004; Blackwater weirs | right | As cited. |
| `west_small_boat` | all numbers; clench-nail sites | doubtful | No hull; estimates. Sites not opened (?). |
| `baltic_coastal_boat` | Truso boats 9-11 x 2.5-3 m | doubtful | From an abstract; not opened (?). |
| `baltic_coastal_boat` | length 10 as typical | doubtful | The largest Ralswiek boat is 14 x 3.4 m (de.wikipedia Ralswiek, via ships.md 2.5). 10 m is the small end: say so in `uncertain`. |
| `baltic_coastal_boat` | cargo_t 2.5 | doubtful | The row's own sum gives more: 7 m3 of displacement less 2 to 3 t of hull leaves 4 to 5 t for crew and cargo. With six men, **3.5**. |
| `norse_allround_ship` | Salme II 17-17.5 x 3 m, 34 dead, keel so a sail; Salme I 11.5 x 2 m, 7 dead | right | en.wikipedia Salme ships (opened today). It dates their use to 700-750: "c. 750" is the late end. |
| `norse_allround_ship` | Kvalsund ship ~18 x 3.2 m, 780-800, removable side rudder, no mast step | right | en.wikipedia Kvalsund ship (18 m, the date, the rudder). The 3.2 m beam is not on that page (?). |
| `norse_allround_ship` | Kanhave canal 726-729, 500 x 11 m, draught to 1 m | right | en.wikipedia Kanhave Canal (opened today). |
| `norse_allround_ship` | Roar Ege 4-5 kn average, 8-10 top; Wulfstan seven days; Ohthere five days | right | ships.md 5.1 and 5.2. Roar Ege is "about 4 kn over long distances": write 4, not 4-5. |
| `norse_allround_ship` | speed_sail_kn 5; speed_max_kn 9; speed_oars_kn 3; closest 60 | right | Agree with ships.md 5.3. |
| `norse_allround_ship` | oars 16 with crew 12 | doubtful | Twelve men cannot pull sixteen oars. Either oars **10** (what the trading crew mans) or say in `units` that oars means oar ports. |
| `norse_longship` | ASC 789, three ships; Aethelweard on Beaduheard; ASC 793; RFA 810, 200 ships | right | As cited. |
| `norse_longship` | Oseberg 21.6 x 5.1 m, 15 pairs, built 820 | right | en.wikipedia Oseberg Ship: 21.58 x 5.10, 15 pairs, buried 834. |
| `norse_longship` | Saga Oseberg "sails 10-12 kn top", "sail 82 m2", "crew 40-45" | doubtful | Wikipedia gives 10 kn under full sail (March 2014) and a sail of about 90 m2 for the find. The replica's own site was not opened (?). Write "10 kn". |
| `norse_longship` | Storhaug: built c. 770, buried 779, keel ~22 m, "a sailing ship (?)" | doubtful | The dates are Bill and Daly 2012. She is usually described as a large rowing ship, and longer than 22 m (?); not opened. Do not rest the sail of 793 on her. |
| `norse_longship` | "Grondhaug ... revised 2025 to 854-867" | doubtful | I know only Bill and Daly's 790-795 for that burial. The 2025 revision could not be checked (?). |
| `norse_longship` | speed_max_kn 11; speed_sail_kn 6; speed_oars_kn 3.5 | right | Gokstad replica 10 to 11 kn, Saga Oseberg 10; Helge Ask keeps 3.5 to 4 under oars (ships.md 5.1). |
| `norse_longship` | rowing_sprint "5.5 kn for 1.5 h, then 4 kn" | wrong | Helge Ask sprints at **5 kn** over a short race and keeps 3.5 to 4; the Sea Stallion averages 2.5 and tops 4.2 (VSM pages, ships.md 5.1). Set **"5 kn for 15 minutes, then 3.5 kn"**. This is the chase rule: it decides how often a trader escapes. |
| `norse_longship` | mast_share 0.85 in 793 | doubtful | Sails in the west of Norway before 800 rest on Salme and on hull shape (Kvalsund has no mast step). The row says so. Fine as a game choice. |
| `north_sea_merchant_ship` | Willibald (716, Lundenwic to Dorestad; the Cuent); Bede HE IV.22 (Imma, 679) | right | As cited. |
| `north_sea_merchant_ship` | Graveney: c. 895, 13.6 m, ~4 m, 6-7 t, low freeboard, lava querns and hops | right | MASS, Graveney boat (opened today). |
| `north_sea_merchant_ship` | period_name: "OE ceol (keel) and hulc are attested later" | wrong | **Ceol is early.** Gildas, De Excidio 23 (c. 540), says the Saxons came "tribus, ut lingua eius exprimitur, cyulis": in three keels, as their tongue has it. Only hulc is later (IV Aethelred, c. 1000). The Frisian word cog (the cog-due of the Utrecht charters, 9th-10th c.) could be added (?). |
| `north_sea_merchant_ship` | uncertain: "the 8th-c Frisian sea-going hull is unknown" | doubtful | One fragment exists: the Dorestad keel ship, after about 739, Norse-built clinker with iron rivets (MASS, Dorestad ship fragment, in ships.md 2.1). Add it to `evidence`. |
| `north_sea_merchant_ship` | speed_oars_kn 2 for 3 h, with four oars and 6 t | wrong | Ottar (20 t, a few oars) is put at 1.5 kn at the most; Roar Ege with 4.6 t and six oars keeps 2.0 (VSM, ships.md 5.1). Set **1.2**. |
| `north_sea_merchant_ship` | gale_fragility 0.4 | doubtful | MASS on Graveney: the low freeboard "made the boat less capable of handling rough seas". **0.5**. |
| `north_sea_merchant_ship` | beam 3.6; cargo 6; speed_sail 4.5; max 7.5 | right | Graveney's scale; ships.md 5.3 gives 4 abeam and 7.5 at the most. |
| `coaster_minster_ship` | S 86, S 88, S 98 (London tolls); S 29 (two ships at Fordwich, a third at Fordwich and Sarre) | right | Electronic Sawyer S 29 opened today: king, abbess, date and ships as given. The (?) on S 88 and S 98 stays: not opened. |
| `coaster_minster_ship` | speed_oars_kn 3 with six oars and 2.5 t | wrong | Roar Ege keeps 2.0 and tops 3.2 with six oars; Kraka Fyr makes 2.5 with four (VSM, ships.md 5.1). Set **2.2**. |
| `coaster_minster_ship` | size 10 x 2.5 m, cargo 2.5 t | right | Skuldelev 6 as rebuilt for carrying: 11.2 x 2.5 m, about 1.5 to 2 t. 2.5 t is the generous end. |
| `warship_west` | Sutton Hoo ~27 x 4.5 m, about 40 oars, c. 625 | right | en.wikipedia Sutton Hoo: 27 x 4.4 m, places for forty, buried 610-635. Write 4.4. |
| `warship_west` | RFA 789 (Frisians by ship up the Havel); RFA 800; RFA 811; Bede HE II.9, IV.26 | right | As cited. |
| `warship_west` | uncertain: "S 134 (792) ... wording not checked (?)" | right | Electronic Sawyer S 134 opened today: "nisi expeditione intra Cantiam contra paganos marinos cum classis migrantibus". Move it from `uncertain` to `evidence`. |
| `warship_west` | 22 m, 32 oars, crew 40 | doubtful | Scaled from Sutton Hoo of 170 years before; no hull of the 790s. The row says so. |
| all eighteen | from 0, to 0 | doubtful | True for 793; no use to a game that runs to 1066 (ships.md 11.1 has the later classes and their years). |
| all eighteen | placeholder length_x, beam_x, depth_x | right | I recomputed all 54: each is the class value over 7.7, 1.5 and 0.59. They agree with the table as it stands (and beam_x moves if the Gislinge beam does). |
| `later_types_not_in_table` | "the cog (12th c)" | doubtful | The hull found is 12th century; the word is 9th-10th (ships.md 11.2) (?). |
| `later_types_not_in_table` | Askekarr, Klastad, Skuldelev 1 and 3, Puck 2, Gokstad, Graveney, Skuldelev 2 and 5 | right | Dates and sizes as in ships.md 2.3 to 2.5. |

---

## Part 2. Texts: `texts_793.json`

267 entries. Columns: the entry; the verdict; the note. Each series shares one credit, checked once and stated above
its block.

### What is wrong across the whole table

- **The `about` line** says each text has "a free translation that passes the rule". 35 entries have no English at
  all (the 33 Latin riddles, `exeter_25_onion` and `adam`), 3 have the game's own draft, and 12 (the `verse:` entries)
  carry no translator's name, year or death year, only a key.
- **Field shapes differ.** `where` is a string in 183 entries and an object in the rest; `translator` is a name in
  most and a key (`cpb1883`, `laing1844`, `anderson1880`) in twelve; `translated` and `translatorDied` are missing in
  46 entries and 0 in others. Code that reads the credit must cope with all of these, or the table should be made
  uniform.

**Count:** 267 entries: 208 right, 41 doubtful, 18 wrong.

### Gestumblindi's riddles (36 entries)

Credit checked: Corpus Poeticum Boreale vol. I (Oxford, 1883), by Gudbrand Vigfusson (died 1889) and F. York Powell (died 1904). Both dates right; passes the rule. "Read my, etc." and "What was that wonder, etc." are the translators' own shortenings. Told later (a 13th-century saga): right. Wording sampled in nos. 1, 2, 5, 12, 20, 30, 36.

| Entry | Verdict | Note |
|---|---|---|
| `t-gest-01` | right | CPB vol. I, p. 87. |
| `t-gest-02` | right | CPB vol. I, p. 87. |
| `t-gest-03` | right | CPB vol. I, p. 87. |
| `t-gest-04` | right | CPB vol. I, pp. 87-88. |
| `t-gest-05` | right | CPB vol. I, p. 88. |
| `t-gest-06` | right | CPB vol. I, p. 88. |
| `t-gest-07` | right | CPB vol. I, p. 88. |
| `t-gest-08` | right | CPB vol. I, p. 88. |
| `t-gest-09` | right | CPB vol. I, p. 88. |
| `t-gest-10` | right | CPB vol. I, p. 88. |
| `t-gest-11` | right | CPB vol. I, pp. 88-89. |
| `t-gest-12` | right | CPB vol. I, p. 89. |
| `t-gest-13` | doubtful | Credit right (CPB vol. I, p. 89). The English breaks off ('. . . .'): the translators left the rest out. |
| `t-gest-14` | right | CPB vol. I, p. 89. |
| `t-gest-15` | right | CPB vol. I, p. 89. |
| `t-gest-16` | right | CPB vol. I, p. 89. |
| `t-gest-17` | right | CPB vol. I, p. 89. |
| `t-gest-18` | right | CPB vol. I, pp. 89-90. |
| `t-gest-19` | right | CPB vol. I, p. 90. |
| `t-gest-20` | right | CPB vol. I, p. 90. CPB's own Final Readings change line 79; the entry says so in `where`. |
| `t-gest-21` | right | CPB vol. I, p. 90. |
| `t-gest-22` | right | CPB vol. I, p. 90. |
| `t-gest-23` | right | CPB vol. I, p. 90. |
| `t-gest-24` | right | CPB vol. I, pp. 90-91. |
| `t-gest-25` | doubtful | Credit right (CPB vol. I, p. 91). The English breaks off ('. . . .'): the translators left the rest out. |
| `t-gest-26` | right | CPB vol. I, p. 91. |
| `t-gest-27` | doubtful | Credit right (CPB vol. I, p. 91). The English breaks off ('. . . .'): the translators left the rest out. |
| `t-gest-28` | right | CPB vol. I, p. 91. |
| `t-gest-29` | right | CPB vol. I, p. 91. |
| `t-gest-30` | doubtful | Credit right (CPB vol. I, p. 91). The English breaks off ('. . . .'): the translators left the rest out. |
| `t-gest-31` | right | CPB vol. I, p. 92. |
| `t-gest-32` | right | CPB vol. I, p. 92. |
| `t-gest-33` | right | CPB vol. I, p. 92. |
| `t-gest-34` | right | CPB vol. I, p. 92. |
| `t-gest-35` | right | CPB vol. I, p. 92. |
| `t-gest-36` | right | CPB vol. I, p. 92. |

### Vafthruthnir's lay and Alvis's lay (70 entries)

Credit checked: Henry Adams Bellows (1885-1939), The Poetic Edda (New York: American-Scandinavian Foundation, 1923): passes the rule. Old Norse from Hildebrand and Gering, Die Lieder der älteren Edda (Paderborn, 1904): right. Bellows numbers these two poems as the editions do. `acquire` on 22 of the Vafthruthnir entries offers the stanzas from 793 at Uppsala, Lejre, Birka (to 975), Hedeby, Ribe, Paviken, Ladoga, Avaldsnes and Ahus: no fault found, except that "a Rus trader" at Ladoga in 793 is early for the name (?). Wording sampled in Vafthruthnir 11, 20, 55 and Alvis 9, 34.

| Entry | Verdict | Note |
|---|---|---|
| `t-vafth-11` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 63. |
| `t-vafth-12` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 64. |
| `t-vafth-13` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 64. |
| `t-vafth-14` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 64. |
| `t-vafth-15` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 64. |
| `t-vafth-16` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 65. |
| `t-vafth-17` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 65. |
| `t-vafth-18` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 65. |
| `t-vafth-20` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 66. |
| `t-vafth-21` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 66. |
| `t-vafth-22` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 66. |
| `t-vafth-23` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 66. |
| `t-vafth-24` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 67. |
| `t-vafth-25` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 67. |
| `t-vafth-26` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 67. |
| `t-vafth-27` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 67. |
| `t-vafth-28` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 68. |
| `t-vafth-29` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 68. |
| `t-vafth-30` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 68. |
| `t-vafth-31` | right | Bellows 1923; Norse: Hildebrand and Gering 1904. |
| `t-vafth-32` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 69. |
| `t-vafth-33` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 69. |
| `t-vafth-34` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 69. |
| `t-vafth-35` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 70. |
| `t-vafth-36` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 70. |
| `t-vafth-37` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 70. |
| `t-vafth-38` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 70. |
| `t-vafth-39` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 71. |
| `t-vafth-40` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 71. |
| `t-vafth-41` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 71. |
| `t-vafth-42` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 72. |
| `t-vafth-43` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 72. |
| `t-vafth-44` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 72. |
| `t-vafth-45` | right | Bellows 1923; Norse: Hildebrand and Gering 1904. |
| `t-vafth-46` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 73. |
| `t-vafth-47` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 73. |
| `t-vafth-48` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 73. |
| `t-vafth-49` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 74. |
| `t-vafth-50` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 74. |
| `t-vafth-51` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 74. |
| `t-vafth-52` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 74. |
| `t-vafth-53` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 75. |
| `t-vafth-54` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 75. |
| `t-vafth-55` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 75. |
| `t-alv-09` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 156. |
| `t-alv-10` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 156. |
| `t-alv-11` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 156. |
| `t-alv-12` | right | Bellows 1923; Norse: Hildebrand and Gering 1904. |
| `t-alv-13` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 157. |
| `t-alv-14` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 157. |
| `t-alv-15` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 157. |
| `t-alv-16` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 157. |
| `t-alv-17` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 158. |
| `t-alv-18` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 158. |
| `t-alv-19` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 158. |
| `t-alv-20` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 158. |
| `t-alv-21` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 159. |
| `t-alv-22` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 159. |
| `t-alv-23` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 159. |
| `t-alv-24` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 159. |
| `t-alv-25` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 159. |
| `t-alv-26` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 160. |
| `t-alv-27` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 160. |
| `t-alv-28` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 160. |
| `t-alv-29` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 160. |
| `t-alv-30` | right | Bellows 1923; Norse: Hildebrand and Gering 1904. |
| `t-alv-31` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 161. |
| `t-alv-32` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 161. |
| `t-alv-33` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 161. |
| `t-alv-34` | right | Bellows 1923; Norse: Hildebrand and Gering 1904, p. 161. |

### Exeter Book riddles (32 entries)

Credit checked: Benjamin Thorpe (1782-1870), Codex Exoniensis (London, 1842): passes the rule. Old English from Frederick Tupper, The Riddles of the Exeter Book (Boston, 1910): right (Tupper died in 1950). The ids use Krapp and Dobbie's numbers; Tupper's and Thorpe's run one higher for the first sixty, as each `where` says. Written down about 970: right. Thorpe's English says "call'd" and "sate": it is his, so it stays, but BRIEF.md rule 3 means the game's own words around it must not copy the manner. Wording sampled in 1, 25, 33, 47, 86.

| Entry | Verdict | Note |
|---|---|---|
| `exeter_01_storm_land` | right | Tupper no. 2; Thorpe's English, pp. 380-381. |
| `exeter_02_storm_sea` | right | Tupper no. 3; Thorpe's English, pp. 381-382. |
| `exeter_03_storm_sky` | right | Tupper no. 4; Thorpe's English, pp. 382-387. |
| `exeter_05_shield` | right | Tupper no. 6; Thorpe's English, p. 388. |
| `exeter_06_sun` | right | Tupper no. 7; Thorpe's English, p. 389. |
| `exeter_07_swan` | right | Tupper no. 8; Thorpe's English, pp. 389-390. |
| `exeter_09_cuckoo` | right | Tupper no. 10; Thorpe's English, p. 391. |
| `exeter_10_barnacle_goose` | right | Tupper no. 11; Thorpe's English, p. 392. |
| `exeter_12_ox` | right | Tupper no. 13; Thorpe's English, pp. 393-394. |
| `exeter_13_ten_chickens` | right | Tupper no. 14; Thorpe's English, p. 394. |
| `exeter_14_horn` | right | Tupper no. 15; Thorpe's English, pp. 395-396. |
| `exeter_15_badger` | right | Tupper no. 16; Thorpe's English, pp. 396-397. |
| `exeter_16_anchor` | right | Tupper no. 17; Thorpe's English, p. 398. |
| `exeter_19_rider_runes` | right | Tupper no. 20; Thorpe's English, p. 400. |
| `exeter_20_sword` | right | Tupper no. 21; Thorpe's English, pp. 400-402. |
| `exeter_21_plough` | right | Tupper no. 22; Thorpe's English, p. 403. |
| `exeter_23_bow` | right | Tupper no. 24; Thorpe's English, pp. 405-406. |
| `exeter_24_jay_runes` | right | Tupper no. 25; Thorpe's English, pp. 406-407. |
| `exeter_25_onion` | doubtful | Source right (Tupper 26 = Krapp and Dobbie 25). No English: Thorpe left it untranslated. The game shows Old English only, or needs its own draft. |
| `exeter_26_book` | right | Tupper no. 27; Thorpe's English, pp. 407-409. |
| `exeter_27_mead` | right | Tupper no. 28; Thorpe's English, pp. 409-410. |
| `exeter_29_moon_sun` | right | Tupper no. 30; Thorpe's English, pp. 411-412. |
| `exeter_32_ship` | right | Tupper no. 33; Thorpe's English, pp. 414-415. |
| `exeter_33_iceberg` | right | Tupper no. 34; Thorpe's English, pp. 415-416. |
| `exeter_34_rake` | right | Tupper no. 35; Thorpe's English, pp. 416-417. |
| `exeter_35_mail_coat` | right | Tupper no. 36; Thorpe's English, pp. 417-418. |
| `exeter_47_bookworm` | right | Tupper no. 48; Thorpe's English, p. 432. |
| `exeter_68_69_ice` | right | Tupper no. 69; Thorpe's English. |
| `exeter_77_oyster` | right | Tupper no. 77; Thorpe's English, pp. 488-489. |
| `exeter_83_ore` | right | Tupper no. 83; Thorpe's English, pp. 490-491. |
| `exeter_85_fish_river` | right | Tupper no. 85; Thorpe's English, p. 494. |
| `exeter_86_garlic_seller` | right | Tupper no. 86; Thorpe's English, pp. 494-495. |

### Latin riddles: Aldhelm, Symphosius, Tatwine, Eusebius (33 entries)

Editions checked: Aldhelm, ed. Rudolf Ehwald, MGH Auct. Ant. XV (Berlin, 1919); Symphosius, ed. Alexander Riese, Anthologia Latina I.1 (2nd ed., Leipzig, 1894), no. 286; Tatwine, ed. Thomas Wright, Anglo-Latin Satirical Poets vol. II (Rolls Series 59, 1872); Eusebius, ed. J. A. Giles, Anecdota Bedae (Caxton Society, 1851). All four right, all out of copyright. The riddle numbers and subjects agree with the editions in every entry (Aldhelm 2 wind, 19 salt, 23 scales, 25 magnet, 32 tablets, 33 mail-coat, 40 pepper, 56 beaver, 78 wine-cask, 80 glass cup, 89 book-chest, 92 lighthouse; Symphosius 4 key, 5 chain, 10 ice, 11 snow, 12 river and fish, 13 ship, 16 bookworm, 44 onion, 61 anchor, 82 spiced wine, 91 money, 94 one-eyed garlic seller; Tatwine 5 parchment, 15 snow, hail and ice, 17 squirrel, 30 sword and sheath, 39 whetstone; Eusebius 13 cow, 23 sea, 31 wax, 40 fish). Dates of composition right. None has an English translation.

| Entry | Verdict | Note |
|---|---|---|
| `aldhelm_02_ventus` | right | Edition right (pp. 99-100). No English in the entry. |
| `aldhelm_19_salis` | right | Edition right (pp. 105-106). No English in the entry. |
| `aldhelm_23_trutina` | right | Edition right (p. 107). No English in the entry. |
| `aldhelm_25_magnes` | right | Edition right (p. 108). No English in the entry. |
| `aldhelm_32_pugillares` | right | Edition right (p. 111). No English in the entry. |
| `aldhelm_33_lorica` | right | Edition right (pp. 111-112). No English in the entry. |
| `aldhelm_40_piper` | right | Edition right (pp. 114-115). No English in the entry. |
| `aldhelm_56_castor` | right | Edition right (p. 122). No English in the entry. |
| `aldhelm_78_cupa_vinaria` | right | Edition right (p. 133). No English in the entry. |
| `aldhelm_80_calix_vitreus` | right | Edition right (p. 134). No English in the entry. |
| `aldhelm_89_arca_libraria` | right | Edition right (p. 138). No English in the entry. |
| `aldhelm_92_farus` | right | Edition right (p. 140). No English in the entry. |
| `symphosius_04_clavis` | right | Edition right (p. 224). No English in the entry. |
| `symphosius_05_catena` | right | Edition right (p. 224). No English in the entry. |
| `symphosius_10_glacies` | right | Edition right (p. 225). No English in the entry. |
| `symphosius_11_nix` | right | Edition right (p. 225). No English in the entry. |
| `symphosius_12_flumen` | right | Edition right (p. 226). No English in the entry. |
| `symphosius_13_navis` | right | Edition right (p. 226). No English in the entry. |
| `symphosius_16_tinea` | right | Edition right (p. 226). No English in the entry. |
| `symphosius_44_cepa` | doubtful | Edition right (p. 231). No English in the entry. The entry itself marks a reading or a page (?): look at the page image. |
| `symphosius_61_ancora` | right | Edition right (p. 236). No English in the entry. |
| `symphosius_82_conditum` | right | Edition right (p. 241). No English in the entry. |
| `symphosius_91_pecunia` | doubtful | Edition right (p. 243). No English in the entry. The entry itself marks a reading or a page (?): look at the page image. |
| `symphosius_94_luscus` | right | Edition right (p. 244). No English in the entry. |
| `tatwine_05_membranis` | right | Edition right (p. 526). No English in the entry. |
| `tatwine_15_nive_glacie` | right | Edition right (p. 529). No English in the entry. |
| `tatwine_17_sciuro` | right | Edition right (p. 529). No English in the entry. |
| `tatwine_30_ense_vagina` | right | Edition right (p. 532). No English in the entry. |
| `tatwine_39_coticulo` | right | Edition right (p. 534). No English in the entry. |
| `eusebius_13_vacca` | right | Edition right (p. 56). No English in the entry. |
| `eusebius_23_aequore` | doubtful | Edition right (p. 58). No English in the entry. The entry itself marks a reading or a page (?): look at the page image. |
| `eusebius_31_cera` | right | Edition right (p. 59). No English in the entry. |
| `eusebius_40_pisce` | right | Edition right (p. 60). No English in the entry. |

### Voyage texts (5 entries)

Each read in full.

| Entry | Verdict | Note |
|---|---|---|
| `ohthere` | doubtful | Source right: Bosworth 1859 (died 1876); Old English from Sweet 1883 (died 1912). But `acquire`: Hamwic and York in 890 are the wrong places for a king's book-priest (change 6). Sweet's text and Bosworth's English follow different manuscripts (the Iraland / Iceland note): keep the note. |
| `wulfstan` | doubtful | Source right, as `ohthere`. Same `acquire` fault (change 6). |
| `dicuil` | right | Joseph Anderson (1832-1916), in the Orkneyinga Saga of 1873; Latin from Parthey 1870 (died 1872). It is a paraphrase inside an introduction, and the entry says so. `people` 'norse' is doubtful: Dicuil was an Irish monk at the Frankish court. |
| `adam` | wrong | `acquire.from` 1060: the book was written in 1075-76, after the game ends (change 6). The Latin source (Lappenberg and Weiland 1876) and the statement that no free English exists (Tschan 1959) are right. `people` 'norse' is doubtful. |
| `landnama` | wrong | `acquire`: from 1000 at Skiringssal and Birka, both gone by then (change 6). Also doubtful: the Old Norse is the Hauksbók text (Prose Reader 1879) and the English is Ellwood's from Sturlubók, so they do not match sentence for sentence (the entry admits it). Ellwood 1898 right; his death year 1911 not checked (?). |

### Skaldic verses (12 entries)

All twelve fail the table's own form: `translator` holds a key (`cpb1883`, `laing1844`, `anderson1880`), and `translated` and `translatorDied` are missing. The books behind the keys do pass the rule: Corpus Poeticum Boreale (1883; Vigfusson died 1889, Powell 1904); Samuel Laing (1780-1868), The Heimskringla (London, 1844); Rasmus B. Anderson (1846-1936), The Younger Edda (Chicago, 1880). No entry names the edition its Old Norse was copied from. `writtenDown` gives the year the verse was made, where the rest of the table gives the manuscript. Page numbers not checked (?).

| Entry | Verdict | Note |
|---|---|---|
| `verse:v01_hofudlausn_west_over_sea` | wrong | `translator` is a key, with no year or death year (change 5). CPB vol. I pp. 267-268 (?). The English has a gap (dots). |
| `verse:v02_hofudlausn_beasts_of_battle` | wrong | `translator` is a key, with no year or death year (change 5). CPB vol. I p. 269 (?). |
| `verse:v03_hofudlausn_gold` | wrong | `translator` is a key, with no year or death year (change 5). CPB vol. I p. 270 (?). |
| `verse:v04_eyvindr_buried_gold` | wrong | `translator` is a key, with no year or death year (change 5). Laing 1844, Harald Grafeld's saga ch. 1: the right place. Laing's rhyme is very free; it is one English for two stanzas. |
| `verse:v05_bragi_gefjun` | wrong | `translator` is a key, with no year or death year (change 5). Anderson 1880, The Fooling of Gylfe ch. 1: the right place. |
| `verse:v06_bragi_hamdir_sorli` | wrong | `translator` is a key, with no year or death year (change 5). `original` has 'Raudvés': read 'Randvés'. |
| `verse:v07_haustlong_thor` | wrong | `translator` is a key, with no year or death year (change 5). Anderson 1880, Thor and Hrungner: the right place. |
| `verse:v08_hallfredr_raid_map` | wrong | `translator` is a key, with no year or death year (change 5). The English contains '[word unreadable in our OCR copy]': read it from CPB vol. II p. 95 ('Cumbrians' (?)). |
| `verse:v09_hallfredr_land_as_bride` | wrong | `translator` is a key, with no year or death year (change 5). CPB vol. II p. 95 (?). The English has a gap (dots) in the middle. |
| `verse:v10_bjarkamal_gold_list` | wrong | `translator` is a key, with no year or death year (change 5). The title repeats 'Bjarkamál'. The English ends 'Awake, awake, O king!', which does not answer the last two lines of the Norse given: check the page and make the two match. |
| `verse:v11_harald_sicily` | wrong | `translator` is a key, with no year or death year (change 5). Laing 1844, Harald Hardrade's saga ch. 15: the right place. |
| `verse:v12_egill_lund` | wrong | `translator` is a key, with no year or death year (change 5). CPB vol. I p. 373 (?). CPB's gloss 'Lund [in Sweden]' is wrong for the tenth century: Lund is in Scania, then Danish. It is inside the quoted English: cut the gloss. |

### Beowulf (43 entries)

Credit checked: John Lesslie Hall (1856-1928), Beowulf: An Anglo-Saxon Epic Poem (Boston: D. C. Heath, 1892), Gutenberg #16328: passes the rule. Old English from Harrison and Sharp, 4th ed. (Boston: Ginn, 1894), Gutenberg #9701: right. 43 fitts, as Hall and the manuscript number them. Manuscript date right. Wording sampled in fitts I, II, XLIII.

| Entry | Verdict | Note |
|---|---|---|
| `beowulf_01` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_02` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_03` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_04` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_05` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_06` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_07` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_08` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_09` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_10` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_11` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_12` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_13` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_14` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_15` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_16` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_17` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_18` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_19` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_20` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_21` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_22` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_23` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_24` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_25` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_26` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_27` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_28` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_29` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_30` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_31` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_32` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_33` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_34` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_35` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_36` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_37` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_38` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_39` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_40` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_41` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_42` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |
| `beowulf_43` | right | Hall 1892 (died 1928); Old English: Harrison and Sharp 1894. |

### Old English poems from Cook and Tinker (4 entries)

Each read in full. All four were taken from one uncorrected scan of Select Translations from Old English Poetry (Boston: Ginn, 1902).

| Entry | Verdict | Note |
|---|---|---|
| `seafarer` | wrong | `translatorDied` 0: she died in **1918** (Internet Archive catalogue: Iddings, Lola La Motte, 1858-1918). So it passes; delete `rightsNote`. `original` is a note in brackets, not the poem. The English still has scan dirt and her signature as its last line (changes 3, 4). |
| `wanderer` | doubtful | Credit right: Emily H. Hickey (died 1924), The Academy 1881, in Cook and Tinker 1902; Old English from Bright 1891. But the English has scan dirt (broken words such as 'with- stand', stray quote marks) and her signature as the last line (change 4). |
| `deor` | wrong | The translator is **Charlton M. Lewis** (1866-1923), not Blackburn. The entry also contains the whole of the next poem, The Husband's Message (that one is Blackburn's), a page heading and scan dirt. `original` is a note in brackets (change 2). |
| `maldon_991` | wrong | `translatorDied` 0: Lumsden died in **1909** (1829-1909). `translated` 1902 is the reprint; first printed 1887 (?). Passes the rule; delete `rightsNote`. Scan dirt, and the signature 'H. W. LUMSDEN.' and 'II' at the end (changes 3, 4). |

### Hávamál (27 entries)

Credit checked: Bellows 1923, as above: passes the rule. The English and the Norse are paired rightly in every entry sampled (1, 12, 54, 68, 70, 76, 77, 81). Doubtful throughout for one reason: the Old Norse is given in modern Icelandic spelling.

| Entry | Verdict | Note |
|---|---|---|
| `havamal_001` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_002` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_003` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_004` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_005` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_006` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_007` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_010` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_011` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_012` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_015` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_021` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_023` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_034` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_035` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_036` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_038` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_041` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_043` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_044` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_047` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_054` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_068` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_070` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |
| `havamal_076` | doubtful | As the series. Bellows prints this stanza as his 77 (his 76 is Fitjung's sons): the pairing in the entry is right, the number is the editions'. |
| `havamal_077` | doubtful | As the series. Bellows prints this stanza as his 78; the pairing is right. |
| `havamal_081` | doubtful | Bellows 1923: right. The Old Norse is in modern Icelandic spelling (is.wikisource): take it from Hildebrand and Gering 1904. |

### Single texts (5 entries)

Each read in full.

| Entry | Verdict | Note |
|---|---|---|
| `aelfric_merchant` | doubtful | The game's own English, marked so; it is true to the Old English (read line by line). The Old English is from Wikisource with no edition named: name one (Thorpe, Analecta Anglo-Saxonica, 1834). The Colloquy is of about 1000: it must not be in the chest in 793. |
| `saxon_vow` | doubtful | The game's own English, true to the text. MGH Capit. I pp. 222-223 is right. 'Uoden' is probably a slip for 'Uuoden' (?): the Old Saxon was copied from Wikipedia, not from the page. |
| `alcuin_793` | doubtful | Letter 16, MGH Epp. IV p. 42: right, and the Latin agrees with the text as I know it. Still quoted from memory and flagged `check`: look at the page, then drop the flag. The game's own English is true to it. |
| `chronicle_793` | wrong | The English is **James Ingram's (1823; he died in 1850)**, not Giles's. Gutenberg #657 is Ingram's translation with Giles's readings in double parentheses; this annal has none (change 1). The Old English (MS E) and the note on the January for June slip are right. |
| `irish_viking_terror` | right | Kuno Meyer (1858-1919), Selections from Ancient Irish Poetry (1911), p. 101; the four lines are as he printed them. St Gall, Cod. Sang. 904, p. 112. The Old Irish is in normalized spelling from Wikipedia: Thesaurus Palaeohibernicus II p. 290 is the printed source to name. |

---

## Part 3. The ten changes that matter most

Written for whoever edits the two tables. Field names are the JSON's own.

1. **`texts_793.json`, entry `chronicle_793`: the translator is wrong.** The English is James Ingram's, not Giles's.
   Set `translator` "James Ingram", `translated` 1823, `translatorDied` 1850; in `where.title` and `credit` replace
   "J. A. Giles (London: Bohn, 1847)" and "J. A. Giles (1847)" with "James Ingram (London: Longman, 1823)". Proof:
   Gutenberg #657 says "Translation by Rev. James Ingram (London, 1823), with additional readings from the translation
   of Dr. J. A. Giles", marks Giles's readings with double parentheses, and prints the 793 annal without them (the
   project's own copy: scratchpad `asc_ingram.txt`, lines 2364-2373). BRIEF.md rule 4 lists both men as safe, so
   nothing is lost but a false credit line in a shipped game.

2. **Entry `deor`: wrong translator, and a second poem inside it.** The translation is by **Charlton M. Lewis**
   (1866-1923; Wikisource author page), signed so in Cook and Tinker. After his name the entry runs on into the next
   poem in the book, "A Love-Letter" (The Husband's Message), which is Francis A. Blackburn's. Cut `translation` after
   the line "He has his day; he overcame; but peace! break not, my heart!" (delete from "Charlton M. Lewis." to the
   end). In what is left, delete " S8" (line 7) and the line "6o SECULAR LYRICS", and mend "ever- more" to "evermore",
   "as zo well" to "as well", "p'ith" to "with", "the test .?" to "the test?". Set `translator` "Charlton M. Lewis",
   `translatorDied` 1923, delete `rightsNote`, and rewrite `credit`: "English: Charlton M. Lewis, in Cook and Tinker,
   Select Translations from Old English Poetry (Boston: Ginn, 1902), pp. 58-60". The entry then passes the rule.

3. **Entries `seafarer` and `maldon_991`: the death years exist; fill them in.** `seafarer`: `translatorDied` **1918**
   (the Internet Archive's catalogue names her "Iddings, Lola La Motte, 1858-1918"). `maldon_991`: `translatorDied`
   **1909** (Henry William Lumsden, 1829-1909, Wikisource author page) and `translated` **1887** (first printed in
   Macmillan's Magazine that year (?); 1902 is only the reprint). Delete both `rightsNote` fields and the "check before
   shipping" sentences in `credit`. Both pass the rule.

4. **Clean the four texts taken from the Cook and Tinker scan** (`seafarer`, `wanderer`, `deor`, `maldon_991`). Each
   still carries the scan's dirt in `translation`: the translator's signature as the last line ("LaMotte Iddings.",
   "Emily H. Hickey.", "H. W. LUMSDEN." followed by "II"); words broken at line ends ("wine- flushed", "gold- givers",
   "with- stand", "Byrht- noth's", "cors- lets'", about twenty in all); stray marks (a line-number "no" and a lone "I"
   in the Seafarer, "God the CltQi" for "God the Creator" (?), "Off a" for "Offa" and "thelgar" for "Aethelgar" in
   Maldon, stray quote marks at the end of the Wanderer). Read each against the page images (Internet Archive
   cu31924059963672) and mend it. And put in the two missing originals: `seafarer.original` and `deor.original` are
   notes in square brackets, which the game would show as the Old English. Thorpe's Codex Exoniensis (1842), already
   in the scratchpad as `thorpe_exon.txt`, prints both poems.

5. **The twelve `verse:` entries: give them real credits.** In each, `translator` is a key. Replace it and add the two
   year fields: `cpb1883` becomes "Gudbrand Vigfusson (d. 1889) and F. York Powell (d. 1904)", `translated` 1883,
   `translatorDied` 1904; `laing1844` becomes "Samuel Laing", 1844, 1868; `anderson1880` becomes "Rasmus B. Anderson",
   1880, 1936. Write `credit` in full, as the Gestumblindi entries do. Then: (a) `verse:v08_hallfredr_raid_map`: the
   translation contains "[word unreadable in our OCR copy]". Read the word from the page (CPB vol. II p. 95); the Norse
   is "kumrskar þjóðir", so it should be "Cumbrians" (?). (b) `verse:v06_bragi_hamdir_sorli`: `original` has
   "Raudvés"; it is "Randvés" (Anderson's own English has "Randver's father"). (c) `verse:v10_bjarkamal_gold_list`:
   the title says "Bjarkamál" twice. (d) None of the twelve says which edition the Old Norse was copied from: name it
   in `where`. (e) `writtenDown` holds the year the verse was made, not the year of the manuscript as everywhere else
   in the table: either give the manuscript (the sagas and Snorri's Edda, 13th c.) and set `toldLater` true, or add a
   `composed` field and use it for all 267.

6. **Three texts can be bought where or when they could not exist** (`acquire`). `adam`: `from` 1060, but Adam wrote
   in 1075-76, after the game ends in 1066. Either take the entry out of the buyable chest or make it `how` "told"
   (sailing times as a Bremen cleric says them). `landnama`: `from` 1000 at `skiringssal` and `birka`, but Kaupang was
   given up by about 950 and Birka by about 975 (the table's own Vafthruthnir entries end Birka in 975). Leave only
   `sliesthorp`, and make it `how` "told": no Landnámabók was written before the 12th century. `ohthere` and
   `wulfstan`: `from` 890 at `hamwic`, `lundenwic`, `eoforwic` from "a priest who copies books for the king". York was
   under Danish kings in 890 and Hamwic was all but empty; keep `lundenwic` (Alfred's from 886) and add Winchester if
   the ports table has it (?).

7. **`ship_classes_793.json`, class `gislinge`: the player's boat disagrees with the museum and with the brief.**
   `beam_m` 1.5 to **1.64**; `crew_range` "2-5" to **"2-4"**; `speed_max_kn` 6.5 to **6**; `cargo_t` 0.9 to **0.66**
   (the museum's tonne is "four men and ten sheep", men included; BRIEF.md already says 660 kg); take "(?)" off the
   draught in `uncertain`. Then recompute `placeholder.beam_x` for all eighteen classes as beam_m / 1.64 (they are all
   beam_m / 1.5 now), and set the same numbers in `Gislinge_WorkingBoat.asset` (cargoKg 660, maxKnots 6). Also lower
   `norse_small_boat.speed_max_kn` to 6. Source: vikingeskibsmuseet.dk, the boat collection, Langoe.

8. **The chase rule.** Class `norse_longship`, `rowing_sprint`: "5.5 kn for 1.5 h, then 4 kn" to **"5 kn for 15
   minutes, then 3.5 kn"** (Helge Ask and the Sea Stallion, Viking Ship Museum, via ships.md 5.1). No crew holds a
   sprint for an hour and a half. With the old value a raider under oars overhauls every trader in a calm; with the
   new one a trader with a lead and any wind at all has a chance.

9. **Oars and tow-ropes are too fast on five rows.** `towed_upstream_km_day`: 18 to **11** (`logboat_expanded`,
   `river_barge`), 20 to **12** (`river_estate_ship`): the Dutch study reckons 10 to 12 km a day against the stream.
   `speed_oars_kn`: `north_sea_merchant_ship` 2 to **1.2**; `coaster_minster_ship` 3 to **2.2** (Roar Ege keeps 2.0
   with six oars and 4.6 t aboard; Ottar makes 1.5 at the most). And `north_sea_merchant_ship.gale_fragility` 0.4 to
   **0.5** (Graveney's low freeboard). The river legs get about half as long again upstream: check that Dorestad to
   Cologne still fits the design's day budget.

10. **Statements of fact to put right in the ships table.** (a) `north_sea_merchant_ship.period_name`: "OE ceol
    (keel) and hulc are attested later" becomes "OE ceol (keel: Gildas's cyulae, c. 540); hulc is attested later
    (c. 1000, LATER)". (b) `logboat_expanded.uncertain`: delete "no Utrecht-type hull securely dated to the 8th c" and
    add to `evidence` "Vleuten 1 (old Rhine): Utrecht type, felled after c. 734, about 15 x 1.10 m (Brouwers, Jansma
    and Manders 2015)"; add to `north_sea_merchant_ship.evidence` "Dorestad keel ship fragment, after c. 739:
    Norse-built clinker, iron rivets, wool caulking (MASS)". (c) `river_barge.evidence`: Kalkar-Niedermormter "c. 805"
    becomes "c. 802". (d) `warship_west`: move the S 134 line from `uncertain` to `evidence`; the wording is now
    checked.

### After the ten

- The 27 `havamal_*` entries give the Old Norse in **modern Icelandic spelling** ("Eldur er bestur", "því að"), from
  is.wikisource. The Vafthruthnir and Alvis entries use Hildebrand and Gering 1904, which is already in the scratchpad:
  take the Hávamál from the same book so the chest has one spelling.
- `exeter_25_onion` and the 33 Latin riddles have no English. That is honest, but the `about` line should say so, and
  the game needs a rule for a text with no translation (only a reader of that tongue can read it).
- `t-gest-30`: the English breaks off ("she shook a . . . .?") because Vigfusson and Powell would not print the rest.
  Either leave the riddle out or finish it as the game's own draft, marked `ownTranslation`.
- `saxon_vow`: "Uoden" should probably be "Uuoden" (?): check the MGH page.
- `alcuin_793`: the Latin agrees with the MGH text as I know it, but nobody has yet looked at the page. Look, then
  delete `check`.
- `aelfric_merchant` is of about 1000 and has no `acquire`: make sure nothing hands it out in 793.
- `dicuil` and `adam` have `people` "norse". Both are Latin churchmen (an Irishman at the Frankish court; a canon of
  Bremen): "latin-church", unless the field means whom the text is about.
- Ships: every `from` and `to` is 0. ships.md 11.1 lists the ten later classes the game needs (knarr, great cargo
  ship, Utrecht ship, 20 m barge and so on) with their years; until they exist no port after 900 can offer the
  right ship.
- `currach`: `speed_sail_kn` 4.5 to 3.5 and `polar_factor` 0.94 to 0.73; `baltic_coastal_boat.cargo_t` 2.5 to 3.5;
  `norse_allround_ship.oars` 16 with a crew of 12 needs a note, or 10.

## What I could not check

- The death year of Thomas Ellwood (1911) and of Francis A. Blackburn: no page opened. Blackburn no longer matters once
  change 2 is made.
- Lumsden's Maldon in Macmillan's Magazine, 1887: from memory.
- The page numbers in Corpus Poeticum Boreale, Laing and Anderson for the twelve verses; the chapter and verse numbers
  given for Snorri's Edda in v06, v07, v09, v10.
- The wording of the long series beyond the samples named above each block.
- Krefeld-Gellep III's 6,961 kg, the Eke boat, Truso's boats, Storhaug's length and rig, the 2025 Gronhaug date, the
  Saga Oseberg's own figures, Welham Bridge and Callow.
- The law of the country the game ships from. Every credited translator named above died in 1939 or earlier, so each
  passes life plus 70 years as well as the table's own rule; Ellwood (?) is the one death year I took on trust.

## Sources opened for this audit (6 October 2026)

- Viking Ship Museum, Roskilde: the boat collection, Langoe; the Gislinge boat open source project.
- MASS (Rijksdienst voor het Cultureel Erfgoed): Utrecht 1; Graveney boat.
- Wikipedia (English): Kvalsund ship; Salme ships; Oseberg Ship; Kanhave Canal; Sutton Hoo.
- The Electronic Sawyer: S 29, S 134.
- Project Gutenberg #657 (the catalogue page, and the project's scratch copy `asc_ingram.txt`).
- Wikisource author pages: Henry William Lumsden; Charlton Miner Lewis. Internet Archive catalogue search: Iddings;
  Blackburn.
- In the project: `Design/content/ships.md` (sections 2, 5, 11, 13: its sources were opened on 4 October by its
  writer), `BRIEF.md`, `BRIEF_2.md`, and the two tables.
