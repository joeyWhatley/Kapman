# People and their tongues: the twelve ports, 793 to 805

Made from the data only: places/*.json (the "people" of each person at the twelve ports), story/self_793.json (peoples, first_tongue), story/tongues_793.json (tongues, words, lines, not_before). Nothing here is invented. Lines are picked by a plain word count over who_would_say_it, when_it_fits and the English (merchant, quay, price, toll, sell, buy, ship, silver ...); a (?) in an attestation is the own mark of doubt of the data file and is kept; a line whose not_before is after 805 is not offered as a living line in the first release and is listed apart.

Kinds of word in tongues_793.json: greeting, farewell, thanks, oath, curse, kin, trade, ship, sea, faith, law, food, rank

Tongues in the file: old_norse (63 words, 22 lines), old_english (62 words, 37 lines), latin (43 words, 32 lines), old_saxon (35 words, 6 lines), old_frankish (31 words, 6 lines), old_frisian (29 words, 0 lines), irish (31 words, 7 lines), slavic (15 words, 0 lines), romance (9 words, 1 lines), baltic (0 words, 0 lines), finnic (0 words, 0 lines)


## saxon

- People named at: daventre 4
- In self_793.json: NO (not one of the eight peoples a player can be)
- Tongue: **old_saxon** = Saxon (from tongues_793.json spoken_by)
- spoken_by: ["saxon"]
- written_then: a little: the baptismal vow (8th century), the Heliand and Genesis (c. 830-850), a few blessings and lists

### Greeting and farewell words (1 of 35 words of this tongue are of that kind)

- **hêl uuis thu** (say: hayl wiss thoo), kind greeting: be well: the greeting. Attested: Heliand 259: 'Hêl uuis thu, Maria' (text of the Bibliotheca Augustana)

### Two lines for a quay (6 lines in this tongue; 5 of them not before a year after 805)

- `os_vow_whole` (fit score 0)
  - original: Forsachistu diobolae? ec forsacho diabolae. end allum diobolgelde? end ec forsacho allum diobolgeldae. end allum dioboles uuercum? end ec forsacho allum dioboles uuercum and uuordum, Thunaer ende Uoden ende Saxnote ende allum them unholdum, the hira genotas sint. gelobistu in got alamehtigan fadaer? ec gelobo in got alamehtigan fadaer. gelobistu in crist godes suno? ec gelobo in crist gotes suno. gelobistu in halogan gast? ec gelobo in halogan gast.
  - English: Do you forsake the devil? I forsake the devil. And all offerings to devils? And I forsake all offerings to devils. And all the works of the devil? And I forsake all the works and words of the devil, Thunaer and Woden and Saxnot and all the fiends that are their companions. Do you believe in God the almighty Father? I believe in God the almighty Father. Do you believe in Christ, the son of God? I believe in Christ, the son of God. Do you believe in the Holy Ghost? I believe in the Holy Ghost. (the game's own (the chest's draft, with the last two questions added))
  - who would say it: a priest asks; the Saxon or Frisian at the font answers
  - when it fits: A BAPTISM in Saxony or Frisia, 772-804 and after: by choice or under the sword. If the player is the one baptised, each answer turns as he says it
  - source: THE OLD SAXON BAPTISMAL VOW, WHOLE (Vatican, Cod. Pal. lat. 577, written c. 800; the wording is of the 8th century; MGH Capitularia I, pp. 222-223). Text as transcribed in Wikipedia, 'Old Saxon Baptismal Vow'. The manuscript's Latin stage words ('et respondeat', and let him answer) are left out, and capitals and question marks are the game's. The chest's copy stops after the fourth answer
- held back by not_before (after 805): `os_hel_horned_ship` 830 (score 2), `os_hel_afraid` 830 (score 1), `os_hel_storm` 830 (score 0), `os_hel_sail` 830 (score 0), `os_hel_hail` 830 (score 0)
- all other lines of this tongue, by fit: none

## frisian

- People named at: doccinga 9, dorestad 8, witla 7, medemelacha 6, traiectum 6, walichrum 4, daventre 2, ripa 2
- In self_793.json: yes, name "Frisian", home dorestad, in_first_release True
- Tongue: **old_frisian** = Frisian (from self_793.json first_tongue)
- spoken_by: ["frisian"]
- written_then: no. About two dozen short rune inscriptions (5th to 10th century); the first connected Frisian is a psalm gloss of c. 1100 and the law books of 1276-1300
- half_known: {"old_english": 0.5, "old_frankish": 0.5, "old_saxon": 0.5}

### Greeting and farewell words (1 of 29 words of this tongue are of that kind)

- **seininge** (say: SAY-ning-eh), kind farewell: a blessing: the sign of the cross (from Latin signum). Attested: Old Frisian law books, 13th century (Bremmer 2009, as cited in Wikipedia, 'Old Frisian')

### Two lines for a quay (0 lines in this tongue; 0 of them not before a year after 805)

- all other lines of this tongue, by fit: none

## frankish

- People named at: quentovic 6, dorestad 5, traiectum 3, daventre 2, walichrum 2, medemelacha 1
- In self_793.json: yes, name "Frank", home quentovic, in_first_release True
- Tongue: **old_frankish** = Frankish (of the Rhine) (from self_793.json first_tongue)
- spoken_by: ["frankish"]
- written_then: the Low Frankish behind Dorestad: almost nothing. The Rhine Frankish of Mainz and Worms: yes, from c. 800 (catechism, oaths, the Ludwigslied)
- half_known: {"romance": 0.75, "old_frisian": 0.5, "old_english": 0.25}

### Greeting and farewell words (1 of 31 words of this tongue are of that kind)

- **heil** (say: hile), kind greeting: whole, well: the greeting (heil wis thu, be well). Attested: Old High German Tatian 3,2 'Heil wis thu' (East Frankish, c. 830) (?)

### Two lines for a quay (6 lines in this tongue; 5 of them not before a year after 805)

- `of_weissenburg_paternoster` (fit score 0)
  - original: Fater unsēr, thu in himilom bist, giuuīhit sī namo thīn. quaeme rīchi thīn. uuerdhe uuilleo thīn, sama sō in himile endi in erthu. Brooth unseraz emezzīgaz gib uns hiutu. endi farlāz uns sculdhi unsero, sama sō uuir farlāzzēm scolōm unserēm. endi ni gileidi unsih in costunga. auh arlōsi unsih fona ubile.
  - English: Our Father who art in heaven, hallowed be thy name. Thy kingdom come. Thy will be done on earth as it is in heaven. Give us this day our daily bread. And forgive us our debts, as we also forgive our debtors. And lead us not into temptation. But deliver us from evil. (the Douay-Rheims Bible (Challoner, 1749-52), Matthew 6:9-13, with 'daily' for the Gospel's 'supersubstantial', as the prayer is said)
  - who would say it: a Frankish priest teaching it; any baptised Frank of the Rhine
  - when it fits: a Frankish household at prayer; a priest drilling converts; a debtor's plea
  - source: the Weissenburg Catechism (South Rhine Frankish, c. 800: the first life's own years). Text as given in Wikipedia, 'Old High German'
- held back by not_before (after 805): `of_strasbourg_charles` 842 (score 1), `of_ludwig_opening` 881 (score 1), `of_strasbourg_army` 842 (score 0), `of_hildebrand_opening` 830 (score 0), `of_paris_give_me` 900 (score 0)
- all other lines of this tongue, by fit: none

## danish

- People named at: ripa 8, dorestad 2, lundenwic 2, walichrum 2, doccinga 1, medemelacha 1
- In self_793.json: yes, name "Dane", home ripa, in_first_release True
- Tongue: **old_norse** = Norse (the Danish tongue) (from self_793.json first_tongue)
- spoken_by: ["danish", "norse", "svea", "gute"]
- written_then: runes on stone, wood and metal; the verse and sagas were written down from the 12th century on
- half_known: {"old_frisian": 0.5, "old_saxon": 0.5}

### Greeting and farewell words (4 of 63 words of this tongue are of that kind)

- **heill** (say: hayl), kind greeting: whole, well, in luck: the greeting, to a man (heil to a woman, heilir to several). Attested: Hávamál 2 'Gefendr heilir!'; Vafþrúðnismál 4 'Heill þú farir!' (Guðni Jónsson's text, heimskringla.no)
- **heill þú farir** (say: hayl thoo FAR-ir), kind farewell: go safe (may you travel whole). Attested: Vafþrúðnismál 4, Frigg to Odin as he sets out
- **velkominn** (say: VEL-kom-in), kind greeting: welcome. Attested: Cleasby-Vigfusson, An Icelandic-English Dictionary (1874), s.v. vel-kominn (?)
- others of the kind, not used above: gestr

### Two lines for a quay (22 lines in this tongue; 4 of them not before a year after 805)

- `on_hav_005` (fit score 3)
  - original: Vits er þörf, þeim er víða ratar; dælt er heima hvat;
  - English: Wits must he have who wanders wide, But all is easy at home; (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: an old Norse trader to a young one
  - when it fits: the first voyage beyond the home waters; after the player is cheated
  - source: Hávamál 5, first half. Old Norse: Guðni Jónsson's text, heimskringla.no
- `on_hav_058` (fit score 3)
  - original: Ár skal rísa, sá er annars vill fé eða fjör hafa; sjaldan liggjandi ulfr lær of getr né sofandi maðr sigr.
  - English: He must early go forth who fain the blood Or the goods of another would get; The wolf that lies idle shall win little meat, Or the sleeping man success. (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: a raider; a hard trader
  - when it fits: a rival sails before dawn; a raider explains himself
  - source: Hávamál 58. Old Norse: Guðni Jónsson's text, heimskringla.no; English: sacred-texts.com/neu/poe/poe04.htm
- held back by not_before (after 805): `on_dr1_erik` 995 (score 1), `on_glavendrup` 900 (score 0), `on_so198_mervalla` 1000 (score 0), `on_jelling` 965 (score 0)
- all other lines of this tongue, by fit: `on_hav_076` 2, `on_hav_052` 2, `on_hav_074` 2, `on_hav_082` 2, `on_hav_047` 1, `on_hav_002` 1, `on_hav_003` 1, `on_hav_010` 1, `on_hav_035` 1, `on_hav_081` 1, `on_vaf_004` 1, `on_hav_077` 0, `on_hav_034` 0, `on_hav_036` 0, `on_hav_038` 0, `on_hav_043` 0

## norse

- People named at: dorestad 1
- In self_793.json: yes, name "Norseman", home ogvaldsnes, in_first_release False
- Tongue: **old_norse** = Norse (the Danish tongue) (from self_793.json first_tongue)
- spoken_by: ["danish", "norse", "svea", "gute"]
- written_then: runes on stone, wood and metal; the verse and sagas were written down from the 12th century on
- half_known: {"finnic": 0.25, "old_english": 0.25}

### Greeting and farewell words (4 of 63 words of this tongue are of that kind)

- **heill** (say: hayl), kind greeting: whole, well, in luck: the greeting, to a man (heil to a woman, heilir to several). Attested: Hávamál 2 'Gefendr heilir!'; Vafþrúðnismál 4 'Heill þú farir!' (Guðni Jónsson's text, heimskringla.no)
- **heill þú farir** (say: hayl thoo FAR-ir), kind farewell: go safe (may you travel whole). Attested: Vafþrúðnismál 4, Frigg to Odin as he sets out
- **velkominn** (say: VEL-kom-in), kind greeting: welcome. Attested: Cleasby-Vigfusson, An Icelandic-English Dictionary (1874), s.v. vel-kominn (?)
- others of the kind, not used above: gestr

### Two lines for a quay (22 lines in this tongue; 4 of them not before a year after 805)

- `on_hav_005` (fit score 3)
  - original: Vits er þörf, þeim er víða ratar; dælt er heima hvat;
  - English: Wits must he have who wanders wide, But all is easy at home; (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: an old Norse trader to a young one
  - when it fits: the first voyage beyond the home waters; after the player is cheated
  - source: Hávamál 5, first half. Old Norse: Guðni Jónsson's text, heimskringla.no
- `on_hav_058` (fit score 3)
  - original: Ár skal rísa, sá er annars vill fé eða fjör hafa; sjaldan liggjandi ulfr lær of getr né sofandi maðr sigr.
  - English: He must early go forth who fain the blood Or the goods of another would get; The wolf that lies idle shall win little meat, Or the sleeping man success. (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: a raider; a hard trader
  - when it fits: a rival sails before dawn; a raider explains himself
  - source: Hávamál 58. Old Norse: Guðni Jónsson's text, heimskringla.no; English: sacred-texts.com/neu/poe/poe04.htm
- held back by not_before (after 805): `on_dr1_erik` 995 (score 1), `on_glavendrup` 900 (score 0), `on_so198_mervalla` 1000 (score 0), `on_jelling` 965 (score 0)
- all other lines of this tongue, by fit: `on_hav_076` 2, `on_hav_052` 2, `on_hav_074` 2, `on_hav_082` 2, `on_hav_047` 1, `on_hav_002` 1, `on_hav_003` 1, `on_hav_010` 1, `on_hav_035` 1, `on_hav_081` 1, `on_vaf_004` 1, `on_hav_077` 0, `on_hav_034` 0, `on_hav_036` 0, `on_hav_038` 0, `on_hav_043` 0

## english

- People named at: lundenwic 14, fordwic 8, sarre 8, quentovic 3
- In self_793.json: yes, name "English", home lundenwic, in_first_release True
- Tongue: **old_english** = English (from self_793.json first_tongue)
- spoken_by: ["english"]
- written_then: yes: laws, charters, verse, glosses; the Chronicle from the 890s
- half_known: {"old_frisian": 0.5, "old_saxon": 0.5, "old_frankish": 0.25}

### Greeting and farewell words (4 of 62 words of this tongue are of that kind)

- **wes hāl** (say: wess hahl), kind greeting: be well: the greeting (wesað hāle to several). It is the root of 'wassail'. Attested: Beowulf 407 'Wæs þū, Hrōðgār, hāl!' (?)
- **wilcuma** (say: WIL-koo-ma), kind greeting: a welcome one: a guest gladly seen. Attested: Maxims I 94 'leof wilcuma Frysan wife' (Exeter Book)
- **lēof** (say: LAY-off), kind greeting: dear one: the polite word to a lord, an elder or a stranger of standing, where we say 'sir'. Attested: Maxims I 94 'leof'; as an address, the Fonthill Letter, 'Leof, ic ðe cyðe' (?)
- others of the kind, not used above: gesund

### Two lines for a quay (37 lines in this tongue; 4 of them not before a year after 805)

- `oe_dur_23` (fit score 7)
  - original: Nē sceal man tō ǣr forht nē tō ǣr fægen.
  - English: A man should not be afraid too soon nor glad too soon. (the game's own)
  - who would say it: an English steersman
  - when it fits: a strange sail that may be a trader; a price that looks too good
  - source: Durham Proverbs 23, text as given by Wikipedia, 'The Durham Proverbs'; it echoes The Wanderer 68
- `oe_max1_frisian_wife` (fit score 4)
  - original: leof wilcuma Frysan wife, þonne flota stondeð; biþ his ceol cumen ond hyre ceorl to ham, agen ætgeofa, ond heo hine in laðaþ, wæsceð his warig hrægl ond him syleþ wæde niwe,
  - English: Dear is the welcome guest to the Frisian wife, when the vessel stands, his ship is come, and her husband to home, her own provider, and she calls him in, washes his weedy garment, and to him gives new raiment (Benjamin Thorpe (d. 1870), Codex Exoniensis (London, 1842))
  - who would say it: an English singer; any Englishman who knows Frisians
  - when it fits: THE HOMECOMING: the player, a Frisian, ties up at Dorestad after a long voyage. The one period verse about a Frisian trader's house
  - source: Maxims I 94-98 (Exeter Book). Old English: sacred-texts.com/neu/ascp/a03_13.htm; Thorpe's English as recorded in Design/content/lore/texts_more.json
- held back by not_before (after 805): `oe_aelfric_merchant` 1000 (score 9), `oe_ohthere_north` 890 (score 4), `oe_thane_right` 1005 (score 3), `oe_wulfstan_truso` 890 (score 2)
- all other lines of this tongue, by fit: `oe_dur_02` 4, `oe_beo_coastguard` 3, `oe_beo_end` 3, `oe_dur_20` 3, `oe_max1_ship_nailed` 2, `oe_max2_mast` 2, `oe_beo_wyrd_spares` 1, `oe_beo_foamy_neck` 1, `oe_beo_far_countries` 1, `oe_wan_where` 1, `oe_dur_10` 1, `oe_chron_793_lindisfarne` 1, `oe_beo_wyrd_goes` 0, `oe_beo_words_and_deeds` 0, `oe_beo_farewell` 0, `oe_beo_avenge` 0, `oe_beo_death_better` 0, `oe_wan_opening` 0, `oe_wan_lent` 0, `oe_sea_true_song` 0, `oe_sea_night_shadow` 0, `oe_sea_heart_knocks` 0, `oe_max1_long_voyage` 0, `oe_max2_river` 0, `oe_max2_troth` 0, `oe_max2_fate` 0, `oe_dur_03` 0, `oe_dur_04` 0, `oe_dur_08` 0, `oe_dur_14` 0, `oe_chron_793_signs` 0

## svea

- People named at: none among the twelve ports (only in self_793.json)
- In self_793.json: yes, name "Svea or Gotlander", home birka, in_first_release False
- Tongue: **old_norse** = Norse (the Danish tongue) (from self_793.json first_tongue)
- spoken_by: ["danish", "norse", "svea", "gute"]
- written_then: runes on stone, wood and metal; the verse and sagas were written down from the 12th century on
- half_known: {"finnic": 0.5, "slavic": 0.25, "baltic": 0.25}

### Greeting and farewell words (4 of 63 words of this tongue are of that kind)

- **heill** (say: hayl), kind greeting: whole, well, in luck: the greeting, to a man (heil to a woman, heilir to several). Attested: Hávamál 2 'Gefendr heilir!'; Vafþrúðnismál 4 'Heill þú farir!' (Guðni Jónsson's text, heimskringla.no)
- **heill þú farir** (say: hayl thoo FAR-ir), kind farewell: go safe (may you travel whole). Attested: Vafþrúðnismál 4, Frigg to Odin as he sets out
- **velkominn** (say: VEL-kom-in), kind greeting: welcome. Attested: Cleasby-Vigfusson, An Icelandic-English Dictionary (1874), s.v. vel-kominn (?)
- others of the kind, not used above: gestr

### Two lines for a quay (22 lines in this tongue; 4 of them not before a year after 805)

- `on_hav_005` (fit score 3)
  - original: Vits er þörf, þeim er víða ratar; dælt er heima hvat;
  - English: Wits must he have who wanders wide, But all is easy at home; (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: an old Norse trader to a young one
  - when it fits: the first voyage beyond the home waters; after the player is cheated
  - source: Hávamál 5, first half. Old Norse: Guðni Jónsson's text, heimskringla.no
- `on_hav_058` (fit score 3)
  - original: Ár skal rísa, sá er annars vill fé eða fjör hafa; sjaldan liggjandi ulfr lær of getr né sofandi maðr sigr.
  - English: He must early go forth who fain the blood Or the goods of another would get; The wolf that lies idle shall win little meat, Or the sleeping man success. (Henry Adams Bellows (d. 1939), The Poetic Edda (1923))
  - who would say it: a raider; a hard trader
  - when it fits: a rival sails before dawn; a raider explains himself
  - source: Hávamál 58. Old Norse: Guðni Jónsson's text, heimskringla.no; English: sacred-texts.com/neu/poe/poe04.htm
- held back by not_before (after 805): `on_dr1_erik` 995 (score 1), `on_glavendrup` 900 (score 0), `on_so198_mervalla` 1000 (score 0), `on_jelling` 965 (score 0)
- all other lines of this tongue, by fit: `on_hav_076` 2, `on_hav_052` 2, `on_hav_074` 2, `on_hav_082` 2, `on_hav_047` 1, `on_hav_002` 1, `on_hav_003` 1, `on_hav_010` 1, `on_hav_035` 1, `on_hav_081` 1, `on_vaf_004` 1, `on_hav_077` 0, `on_hav_034` 0, `on_hav_036` 0, `on_hav_038` 0, `on_hav_043` 0

## wendish

- People named at: none among the twelve ports (only in self_793.json)
- In self_793.json: yes, name "Wend", home reric, in_first_release False
- Tongue: **slavic** = Wendish (West Slavic) (from self_793.json first_tongue)
- spoken_by: ["wendish", "slavic"]
- written_then: no. Names in Frankish and Saxon Latin; the Slavic written nearest in time is Old Church Slavonic (from 863), a southern kin
- half_known: {"old_saxon": 0.5, "old_norse": 0.5}

### Greeting and farewell words (1 of 15 words of this tongue are of that kind)

- **gostь** (say: gost), kind greeting: a guest; a merchant from abroad (kin: Old Church Slavonic). Attested: kin: Old Church Slavonic, 10th- and 11th-century manuscripts (?)

### Two lines for a quay (0 lines in this tongue; 0 of them not before a year after 805)

- all other lines of this tongue, by fit: none

## irish

- People named at: none among the twelve ports (only in self_793.json)
- In self_793.json: yes, name "Irish", home hi, in_first_release False
- Tongue: **irish** = Irish (from self_793.json first_tongue)
- spoken_by: ["irish", "gaelic"]
- written_then: yes, richly: glosses, verse, law, annals (from the 7th century)
- half_known: {"old_english": 0.5}

### Greeting and farewell words (2 of 31 words of this tongue are of that kind)

- **fo chen** (say: fo KHEN), kind greeting: welcome. Attested: Dictionary of the Irish Language (dil.ie) s.v. fo-chen (?)
- **bennacht** (say: BEN-akht), kind farewell: a blessing (from Latin benedictio), given at meeting and at parting. Attested: Dictionary of the Irish Language s.v. bennacht (?)

### Two lines for a quay (7 lines in this tongue; 2 of them not before a year after 805)

- `ir_lorica_christ` (fit score 0)
  - original: Crist lim. Crist remam. Crist imm degaid. Crist innum. Crist ísum. Crist úasam. Crist dessum. Crist tuathum.
  - English: Christ with me, Christ before me, Christ behind me, Christ within me, Christ beneath me, Christ above me, Christ at my right, Christ at my left. (James Henthorn Todd (d. 1869), St. Patrick, Apostle of Ireland (Dublin, 1864), pp. 426-429)
  - who would say it: an Irish monk or pilgrim setting out; an Irish crewman at dawn
  - when it fits: A PRAYER BEFORE A VOYAGE: the best line in the file for turning piece by piece
  - source: the Breastplate of St Patrick (the Faeth Fiada), an Old Irish prayer of the 8th century. Text of Whitley Stokes, The Tripartite Life of Patrick (1888), p. 381, with Todd's English, both as given in Wikipedia, 'Saint Patrick's Breastplate'
- `ir_lorica_strength` (fit score 0)
  - original: Attomriug indiu neurt nime. soillsi gréine. etrochta ésci. áne thened. déne lóchet. luathi gaithi. fudomna mara. tairismigi talman. cobsaidi alech.
  - English: I bind to myself to-day, The power of Heaven, The light of the Sun, (The whiteness of Snow,) The force of Fire, The flashing of lightning, The velocity of Wind, The depth of the Sea, The stability of the Earth, The hardness of Rocks. (James Henthorn Todd (d. 1869), St. Patrick, Apostle of Ireland (1864), pp. 426-429)
  - who would say it: an Irish monk or hermit
  - when it fits: dawn at sea with an Irish monk aboard; a prayer against a storm
  - source: the Breastplate of St Patrick. Text of Stokes (1888) as given in Wikipedia, 'Saint Patrick's Breastplate'; Stokes supplies 'indiu' and the lines of fire and lightning from the Liber Hymnorum, and reads 'moon' where Todd's manuscript had 'snow'
- held back by not_before (after 805): `ir_bitter_wind` 845 (score 1), `ir_pangur` 830 (score 0)
- all other lines of this tongue, by fit: `ir_lorica_drowning` 0, `ir_au_795` 0, `ir_au_798` 0

## latin

- People named at: none among the twelve ports (only in self_793.json)
- In self_793.json: NO (not one of the eight peoples a player can be)
- Tongue: **latin** = Latin (the tongue of the Church) (from not a people: the written tongue of clerks, priests and toll men, added here because the quay hears it)
- spoken_by: ["churchmen of every people", "clerks and envoys"]
- written_then: yes: nearly everything that was written in the West

### Greeting and farewell words (6 of 43 words of this tongue are of that kind)

- **Benedic** (say: BEN-eh-dik), kind greeting: 'bless me': how a monk greets one who comes to the gate. Attested: Rule of Benedict 66
- **Dominus vobiscum** (say: DOM-in-oos voh-BIS-koom), kind greeting: the Lord be with you. Attested: Ruth 2:4 (Vulgate): 'Dominus vobiscum'; the greeting of the priest at Mass
- **pax vobis** (say: pahks VOH-bees), kind greeting: peace be with you. Attested: John 20:19 (Vulgate) (?)
- others of the kind, not used above: Benedicat tibi Dominus, vade in pace, hospes

### Two lines for a quay (32 lines in this tongue; 1 of them not before a year after 805)

- `la_mt13_45` (fit score 11)
  - original: Iterum simile est regnum caelorum homini negotiatori, quaerenti bonas margaritas. Inventa autem una pretiosa margarita, abiit, et vendidit omnia quae habuit, et emit eam.
  - English: Again the kingdom of heaven is like to a merchant seeking good pearls. Who when he had found one pearl of great price, went his way, and sold all that he had, and bought it. (the Douay-Rheims Bible (Challoner, 1749-52))
  - who would say it: a priest who thinks well of the player; a bishop asking for a great gift
  - when it fits: THE MERCHANT'S GOSPEL: the player is asked to give or stake everything on one thing
  - source: Matthew 13:45-46 (Vulgate). Read at drbo.org
- `la_prov11_1` (fit score 8)
  - original: Statera dolosa abominatio est apud Dominum, et pondus aequum voluntas ejus.
  - English: A deceitful balance is an abomination before the Lord: and a just weight is his will. (the Douay-Rheims Bible (Challoner, 1749-52))
  - who would say it: a priest, a moneyer, a toll man catching a false weight
  - when it fits: the false weight in the player's chest; any dealing in short measure
  - source: Proverbs 11:1 (Vulgate). Read at drbo.org. The text behind the kings' laws on weights (Admonitio generalis, 789)
- held back by not_before (after 805): `la_au_806` 806 (score 0)
- all other lines of this tongue, by fit: `la_sir26_28` 8, `la_mk8_36` 6, `la_ps106_23` 3, `la_ps120_8` 3, `la_mass_ite` 3, `la_ps106_28` 2, `la_mass_dominus` 2, `la_alcuin_ship` 2, `la_ps68_2` 1, `la_ps68_16` 1, `la_mt25_35` 1, `la_rb53` 1, `la_ps106_25` 0, `la_ps92_4` 0, `la_ps76_20` 0, `la_ps138_9` 0, `la_ps129_1` 0, `la_ps69_2` 0, `la_ps30_6` 0, `la_ps22_4` 0, `la_mt8_25` 0, `la_mt8_27` 0, `la_jas4_15` 0, `la_ruth2_4` 0, `la_mass_agnus` 0, `la_gloria_patri` 0, `la_pater_noster` 0, `la_alcuin_793` 0, `la_au_794` 0

## Peoples for whom the data has too little to do this honestly

- **saxon**: old_saxon: only 1 greeting or farewell words; only 1 lines usable by 805
- **frisian**: old_frisian: only 1 greeting or farewell words; only 0 lines usable by 805
- **frankish**: old_frankish: only 1 greeting or farewell words; only 1 lines usable by 805
- **wendish**: slavic: only 1 greeting or farewell words; only 0 lines usable by 805
- **irish**: irish: only 2 greeting or farewell words; the second-best line has a fit score of 0: no line of this tongue is truly quay talk
