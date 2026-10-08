# Kapman: the spine

Written 4 October 2026 for the writers of the game's content, and for you. The same as `spine.json`, in prose and shorter: fourteen long stories that cross regions and years, the people who recur in them, the voice, the calendar, and what each people would say about God or the gods. Every fact has its source in `spine.json`; (?) marks what could not be verified; LATER marks a source written long after the thing it tells. It is a draft in the game's own files, for you to overwrite.

## How the writers use it

- A story in a places file takes a thread id from here in its 'thread' field. Place what the thread's hook for your port asks for, in your own words, and add stories of your own freely; a hook is a floor, not a ceiling.
- A person of the cast is named by id (cast.gervold) in any 'ties' or 'links'. If a cast person's 'home' is one of your ports, write them into that port's people under the id in 'place_id' with the same facts, and tie that row to the cast id with the tie 'kin' and the note 'the same person', so the game has one person and the checker finds both ids.
- Never give another invented person the name of anyone in the cast, and never give an invented person a historical person's name and role. Names for your own people: two attested parts of their people, as encounters/names_placeholder.json shows; people ids and roles are those of encounters/npc_roles_793.json.
- Threads share flags. A card sets one with the effect 'flag <name>' and any card anywhere reads it as flag.<name> in its 'when' or a 'check'; that is how a card in one region answers a card in another. Use the flags listed under each thread only for the step they name. Flags of your own take your file's name as a prefix. A chain (Design/content/CHAINS.md) keeps its own step flags, ch_<id>_<n>; where a chain's step is also a thread's step, set the thread's flag as well, so that cards outside the chain can answer it.
- Gate everything by year. A thread's 'years' are when it is live; a hook's 'when' narrows it. The game starts on 21 June 793: anything earlier is told as news or memory. A card that belongs to one port says so with port.id (Design/content/BRIEF_2.md), and may also be reached from that port's places file (a site's 'do' or a story's beat).
- People say days by feasts and moons (calendar), distances by days' sail, odds by a word (sure, likely, doubtful). Prices are in pennies (d) and agree with goods_793.json and Design/economy_and_trade.md.
- Texts the chest already holds and a card may give (give_text): alcuin_793, chronicle_793, saxon_vow, aelfric_merchant, irish_viking_terror, dicuil, ohthere, wulfstan, adam, maldon_991, the Hávamál stanzas (havamal_001 ...), Beowulf by fitt (beowulf_01 ... beowulf_43), seafarer, wanderer, deor. Riddles and their sets are riddles_793.json; the thread riddle_road says how they hang together.
- The user set the game's music aside for now (4 Oct 2026): write no card, site, reward or unlock that needs music to be heard. A singer is a person who knows the old stories and tells them.
- If you need a role, a deed, a variable, a verb or a flag for a thread step that is not here, list it under 'needs' in your report. Do not add it yourself.

## The fourteen threads

| No. | Thread | Years | In one line |
|---|---|---|---|
| 1 | The black stones and the cloaks (`black_stones`) | 789-796 | About 789 King Charles asked for a daughter of Offa of Mercia for his son, Offa asked for a daughter of Charles for his own, and Charles in his anger shut his shores to English merchants; the English shut theirs. |
| 2 | What was carried off from the holy island (`holy_island`) | 793-806 | On 8 June 793 heathen men fell on Saint Cuthbert's church at Lindisfarne, killed some of the brethren, took what was precious and carried others off in their ships, youths among them. |
| 3 | Godfred's market (`godfreds_market`) | 804-813 | In 804 Godfred, king of the Danes, comes to Sliesthorp with his fleet and all the horsemen of his kingdom and will not meet the emperor. |
| 4 | The end of the Saxon war (`saxon_end`) | 792-805 | The Saxons, baptised by force since 785, rise again in 792 and 793 while the king is busy with the Avars, and the East Frisians rise with them and burn their churches. |
| 5 | The cross and the old gods (`cross_and_grove`) | 793-865 | In 793 the mission to the Frisians is two generations old and still thin: east of the Lauwers the churches burn that year, and the priests come back a year later. |
| 6 | The blood owed at York (`york_blood`) | 773-812 | Twenty years before the game begins a Frisian merchant killed a Northumbrian nobleman's young son in a brawl at York, and every Frisian in the city took ship for home for fear of the dead man's kin. |
| 7 | Silver, stamped and weighed (`silver_road`) | 786-1015 | In the game's first winter the king makes his penny heavier and orders that it be taken in every place, town and market: old coin and foreign silver go to the mint, at a loss the moneyer names. |
| 8 | The hungry years (`hungry_years`) | 792-807 | The game opens in a famine: the harvest of 792 failed across the Frankish lands and in Northumbria, men saw false grain standing in the fields that could be touched but not eaten, and the poor are on every road. |
| 9 | Swords under the ballast (`swords_north`) | 779-900 | A Frankish blade is the best in the north and worth three times its price at Sliesthorp or Birka, and since 779 the king's law says no mail coat leaves the realm. |
| 10 | The book road (`book_road`) | 793-875 | A book is the dearest thing a small boat can carry: the skins of a herd of calves, a year of a scribe's life, and worth more than the boat. |
| 11 | Dorestad falls, and the rivers with it (`dorestad_falls`) | 834-911 | From 834 a Danish fleet comes up the river to Dorestad year after year: it burns, kills, carries people off and takes tribute, and in 846 the emperor watches from Nijmegen and can do nothing. |
| 12 | The price of England (`price_of_england`) | 865-1066 | In 865 a great heathen army lands in East Anglia and does not go home: York falls in 866, kings are killed or driven out, and in 876 the army shares out Northumbria and begins to plough, until Alfred beats it in 878 and there are two laws in England, with hostages given before a man may cross to trade. |
| 13 | The riddle road (`riddle_road`) | 793-1066 | Ravn, the Northman who sails with you, sets the first riddle at Dorestad and gives the next when it is answered; after that riddles are had from people in ports, each in its own tongue and its own letters. |
| 14 | The count's day (`counts_day`) | 793-1066 | The boat was bought on the count's silver: four hundred and eighty pennies, two pounds, to be paid at Michaelmas. |

### 1. The black stones and the cloaks

`black_stones`, 789 to 796.

About 789 King Charles asked for a daughter of Offa of Mercia for his son, Offa asked for a daughter of Charles for his own, and Charles in his anger shut his shores to English merchants; the English shut theirs. For some years a Frisian with English cloth, or a Mercian after Frankish wine, trades at the word of a toll man, while Abbot Gervold, who takes the king's tolls at Quentovic and is Offa's friend, carries messages between the two courts. In 796 Charles writes to Offa as a brother about pilgrims, tolls, black stones and the length of cloaks, and in July of that year Offa dies.

**What is true.** Gesta abbatum Fontanellensium c. 12 (written 823-867; ed. Loewenfeld 1886 pp. 46-47): Gervold, abbot of Saint-Wandrille from 787, was 'for many years procurator over the kingdom's trade, collecting tributes and tolls in divers ports and cities, chiefly at Quentawich' (our rendering); he was Offa's friend, Offa's letters to him still lay at the abbey, and he went to Offa many times as the king's envoy, the last time over the marriage. The younger Charles asked for Offa's daughter; Offa would not agree unless Bertha, Charles's daughter, were given to his son; the king, 'somewhat moved', ordered that no one from Britain and the English people should touch the shore of Gaul to trade. (The full account, with every source, is in `spine.json`.)

**Who.** Gervold, Wighelm (invented), Alcuin, Sigewine (invented), Thiadbod (invented), Æthelheard.

**Flags the cards share** (what each means is in `spine.json`): `bs_ban_heard`, `bs_letter`, `bs_peace`.

**What to place where.**

- **Quentovic** (before 797): Gervold's toll house. His clerk wants a quiet ship to take a letter to the wic-reeve at Lundenwic while the ban stands (sets bs_letter).
- **Rotomagus** (before 807): Saint-Wandrille downstream is Gervold's own house: Offa's letters lie in its chest, and a monk there will say what the quarrel was really about (a marriage).
- **Lundenwic** (before 797): The wic-reeve Wighelm turns Frankish hulls away, or lets a Frisian land for a gift and a story (sets bs_ban_heard).
- **Dofras** (before 800): Pilgrims waiting for a ship to Quentovic, one of them a trader with a bundle (npc_text_encounters.md sample 8). Also at Sarre and Fordwic.
- **Hamwic** (before 797): How far the ban reached is not recorded (?): the game lets the West Saxon wic stay half open, the way round for anyone with nerve.
- **Dorestat** (before 797): Skippers argue whether the king's order binds Frisians. Quernstones finished here for the English trade pile up unsold and cheap.
- **Colonia** (before 800): Mayen quern roughouts come down the Rhine. A stone dealer wants to know what length the English king asked for, and will pay for the answer.
- **Eoforwic** (before 797): Alcuin was here in 790-793 and wrote of the quarrel; the minster's people know his letters.
- **At sea** (before 797): A hail in the Channel: an English ship sailing light, turned back from the Canche; or a Frankish king's boat asking whose cloth that is.
- **The later ages** (from 797): The letter's rule outlives both kings: true pilgrims free, traders pay.
- **Legacæstir** (before 797): Offa's far shore. Lead from the hills behind comes down to the Dee: the abbot who keeps the Frankish tolls wants lead for his church roofs, and while the ports are shut it cannot lawfully go to him.
- **Dommoc** (794 to 796): In 794 Offa has the East Angles' king beheaded and takes the kingdom: the bishop's town is under a new lord, and his ban on Frankish ships reaches this coast too (?).

### 2. What was carried off from the holy island

`holy_island`, 793 to 806.

On 8 June 793 heathen men fell on Saint Cuthbert's church at Lindisfarne, killed some of the brethren, took what was precious and carried others off in their ships, youths among them. All that summer the news runs down every coast as fast as a ship and no faster, and what was taken goes north to be sold: a book cover is worth more to its monks than to any Northman, and a boy can be ransomed if someone finds him. Alcuin writes that he will speak to King Charles for the boys; no one wrote down whether any came home.

**What is true.** Anglo-Saxon Chronicle 793 (texts_793.json chronicle_793, Giles 1847): 'the harrowing inroads of heathen men made lamentable havoc in the church of God in Holy-island, by rapine and slaughter'. The Chronicle's 'sixth day before the ides of January' is taken as a slip for June, 8 June, the day in the Annals of Lindisfarne. Alcuin's letters of 793: to King Æthelred (MGH Epistolae IV no. 16; texts_793.json alcuin_793) and to Bishop Higbald and the brethren (no. 20), where he says that when King Charles comes home he will go to him and do what he can about the youths led away captive by the heathen. Symeon of Durham (12th century, LATER): some brethren killed, some carried off in fetters, some drowned. (The full account, with every source, is in `spine.json`.)

**Who.** Higbald, Alcuin, Sigewine (invented), Tidwine (invented), Beornwine (invented), Thorleif (invented), Asulf (invented), Holmfast (invented).

**Flags the cards share** (what each means is in `spine.json`): `lf_knows_boys`, `lf_trail_north`, `lf_boy_found`, `lf_ransom_paid`.

**What to place where.**

- **Lindisfarena** (before 804): Higbald counting what is gone: the names of the boys, the silver from the shrine, the cover of a gospel book.
- **Bebbanburg** (before 797): The king's men saw the sails from the rock and did not come. A thegn is angry or ashamed.
- **Gyrwe** (before 800): From 794: the wreck of the raiders on the bar, a dead leader, things washed up (a rune-cut stick, a foreign knife).
- **Eoforwic** (before 800): Alcuin's letter to the king read aloud; the minster collecting for ransom; the priest with the letter that reaches Dorestad at the game's start.
- **Dorestat** (before 795): The news is thirteen days old on the first day. A priest has a letter from York and wants a book carried to Traiectum.
- **Quentovic** (before 805): At Alcuin's hospice of Saint-Josse cast.sigewine waits with Alcuin's answer and a small bag of the king's alms for ransom (?), looking for a ship that goes north.
- **Ögvaldsnes** (before 807): Cast.thorleif's hall. A woman wears a brooch that was a book mount. Two English boys herd swine; one was sold on.
- **Skíringssalr** (before 807): At the summer market church things are sold as curios: a little house-shaped box, a cup, a strip of gilt.
- **Sliesthorp** (before 807): Cast.asulf, the dealer in captives, remembers every lot: the English boys went east with a Svea, because Christians cannot be sold south to the Franks.
- **Birka** (before 830): A Svea trader (cast.holmfast or his heir) keeps an Englishman who can tally and read marks.
- **The crew** (before 807): Beornwine's younger brother Tidwine was an oblate on the island.
- **At sea** (before 800): A hail from a ship out of the north with church plate under her boards; a boat of monks adrift; the same square sail seen again off the Tyne in 794.
- **The later ages** (from 807): Iona's turn (795, 802, 806); in 875 Cuthbert's people leave the island with his body; a ransomed book given to a church raises standing (riddles_793.json rw_book_gospels).
- **Streonæshalch** (before 868): Hild's house on the cliff watches the sea after 793 and wonders whether to move its books and its saints' bones inland.

### 3. Godfred's market

`godfreds_market`, 804 to 813.

In 804 Godfred, king of the Danes, comes to Sliesthorp with his fleet and all the horsemen of his kingdom and will not meet the emperor. In 808 he falls on the Obodrites, destroys their trading place at Reric, puts its merchants aboard his ships and sets them down at Sliesthorp, and orders a wall from sea to sea with one gate in it. In 810 two hundred of his ships take a hundred pounds of silver from the Frisians, and before the emperor's army can reach him one of his own men kills him.

**What is true.** Annales regni Francorum 804, 808, 809, 810, 811 (chronicle_793.json). 804: Godfred came with his fleet and all the horsemen of his kingdom to Sliesthorp on the border of his realm and Saxony, promised to meet the emperor and did not. 808: he took Obodrite strongholds and made two thirds of that people pay him tribute, losing his nephew Reginold; he destroyed the emporium on the shore which the Danes called Reric and which by its tolls was of great profit to his kingdom, carried the merchants off with him and sailed to the harbour called Sliesthorp; he had the border of his kingdom toward Saxony closed with a rampart along the north bank of the Eider from the eastern bay to the western sea, with a single gate for wagons and horsemen. (The full account, with every source, is in `spine.json`.)

**Who.** Godfred, Thrasco, Dragomir (invented), Asulf (invented), Ravn (invented), Folcbert (invented).

**Flags the cards share** (what each means is in `spine.json`): `gf_reric_friend`, `gf_paid_share`, `gf_warned`.

**What to place where.**

- **Reric** (before 809): The Wends' market under Thrasco, with Danes and Saxons trading in it and a toll that goes to Godfred.
- **Sliesthorp** (804 to 813): 804: the king's fleet and horsemen lie here, and fodder, ale and bread are dear.
- **Ripa** (808 to 811): The king's men gather ships. In 810 a Frisian hull at Ripa is asked whose side she is on: the envoy's card (npc_text_encounters.md sample 9).
- **Emuthon** (in 810): The two hundred ships are on the islands. Each terp is told its share of the hundred pounds; cattle are driven inland; three fights are lost.
- **Dorestat** (808 to 811): The count calls out men and boats for the emperor's host in 810; the price of everything an army eats goes up.
- **Diusburg** (in 810): An easter egg: the army crosses the Rhine at Lippeham, a day downstream, and the emperor's elephant dies there.
- **Ganda** (in 811): The emperor comes to see the ships he ordered built against the Northmen; shipwrights, timber and pitch are wanted.
- **Hleiðr** (804 to 813): The king's hall when he is not with the fleet. After 810 Hemming keeps it for two winters; in 812 the hall is half empty of men.
- **Skíringssalr** (in 813): The Danish kings Harald and Reginfrid come with an army to make Vestfold's chiefs submit.
- **At sea** (in 810): The fleet seen from the masthead off the Frisian islands: run, pay or parley.
- **The crew** (804 to 811): Ravn knows men in that fleet and says less than he means. Aldger's kin are on the islands.
- **The later ages** (from 814): Sliesthorp is the gate between the two seas for 250 years: its own coin about 825, a church about 850, burned in 1050, sacked by the Wends in 1066.

### 4. The end of the Saxon war

`saxon_end`, 792 to 805.

The Saxons, baptised by force since 785, rise again in 792 and 793 while the king is busy with the Avars, and the East Frisians rise with them and burn their churches. Year after year the king comes back with fire, takes hostages by the hundred and gives them to bishops and counts far from home; in 804 he empties the land beyond the Elbe of men, women and children and gives it to the Obodrites. A merchant meets this war as grain and horses bought up for the host, as hostages on the Rhine road, and as men who slip north to the Danes.

**What is true.** Annales regni Francorum and Annales Laureshamenses 792-804 (chronicle_793.json). 793: Count Theoderic's force is destroyed in Rustringen on the lower Weser. 794: two armies; the Saxons submit on the Sintfeld and give hostages. 795: the king lays waste the land to the Elbe at Bardowick and takes more hostages than ever before (the Lorsch annals say every third man (?)). 797: the Capitulare Saxonicum softens the first Saxon capitulary (Capitulatio de partibus Saxoniae, about 782-785), which had set death for refusing baptism, for eating meat in Lent and for burning the dead, and had forbidden general assemblies. 798: the Saxons beyond the Elbe kill the king's envoys; Thrasco's Obodrites beat them on the Sventana field; 1,600 hostages are taken. (The full account, with every source, is in `spine.json`.)

**Who.** Brunger (invented), Liudger, Thrasco, Folcbert (invented), Bernlef, Asulf (invented).

**Flags the cards share** (what each means is in `spine.json`): `sx_brother_trail`, `sx_brother_found`, `sx_carried_fugitive`.

**What to place where.**

- **Daventre** (before 806): The border: Lebuin's church, once burned by Saxons; the host's road east. Grain, horses and salt meat are bought up each May for the army.
- **Diusburg** (before 806): The Hellweg meets the Rhine: hostages and deported families come down it on foot under guard; an officer wants hulls to carry them upriver or across.
- **Colonia** (before 806): The king's crossing place. In 799 and 804 whole villages wait on the bank to be moved into Francia; a priest reads them the vow again.
- **Mogontia** (804 to 806): About 805 the hostages kept in the south are ordered brought here at mid-Lent.
- **Emuthon** (before 795): 793: the East Frisians have risen with the Saxons under Unno and Eilrat: churches burned, priests driven out, the count's men dead on the Weser.
- **Sliesthorp** (before 811): Saxons who would not be moved live among the Danes.
- **Reric** (798 to 808): Thrasco's men, the king's heathen allies, sell what they took on the Sventana field; in 804 they are given the Saxons' land and need ploughs, seed and iron.
- **Rotomagus** (804 to 814): Saxon families set down on abbey land, speaking to nobody, with a priest set over them. A man asks if any ship goes to the Weser.
- **Dorestat** (before 806): The count is away with the host most summers and his steward keeps the tally.
- **The crew** (before 807): Brunger: baptised by the vow, loyal to the boat, hates the king's men. His brother was given as hostage in 785.
- **The later ages** (from 806): In 841 the Saxon freemen and bondmen rise as the Stellinga for the law of their heathen fathers (Nithard IV.2) and are put down.

### 5. The cross and the old gods

`cross_and_grove`, 793 to 865.

In 793 the mission to the Frisians is two generations old and still thin: east of the Lauwers the churches burn that year, and the priests come back a year later. Liudger, a Frisian taught at Utrecht and York, breaks the shrines of Fosite on the island at the Danes' edge, baptises in the spring that no one dared speak beside, and gives the blind singer Bernlef his sight, who then baptises dying children when no priest dares the road. A generation on, Anskar carries the same work to the Danes at Sliaswic and the Svear at Birka, in merchants' ships and by the leave of kings who can take it back.

**What is true.** Altfrid, Vita Liudgeri, book I (written 839-849; ed. Diekamp 1881). Liudger, born about 742 near Utrecht, learned under Gregory at Utrecht and Alcuin at York (I.9-12); rebuilt Lebuin's burned church at Deventer (I.13-15); was sent to destroy the shrines of the Frisians and brought back the treasure found in them, of which the king took two thirds (I.16); taught seven years at Dokkum until Widukind's rising of 784 (I.17-21); went to Rome and Monte Cassino; in 787 was set over five Frisian districts east of the Lauwers, Hugmerchi, Hunusga, Fivilga, Emisga, Federitga, and the island Bant. (The full account, with every source, is in `spine.json`.)

**Who.** Liudger, Bernlef, Landric, Notburg (invented), Hamacar, Anskar, Herigar, Frideburg, Catla, Horik, Ravn (invented), Aldger (invented).

**Flags the cards share** (what each means is in `spine.json`): `ms_carried_priest`, `ms_bernlef_met`, `ms_fosite_seen`.

**What to place where.**

- **Emuthon** (before 810): 793: the rising. A burned church, a priest hiding in a byre who wants passage to Traiectum (sets ms_carried_priest), Bernlef going from house to house with a bowl of water, the wise woman Notburg saying the old gods have come home.
- **Doccinga** (before 810): Boniface's church and the place of the killing; his day is kept on the fifth of June. Liudger passes in spring and autumn.
- **Winaldum** (before 810): The old lords' terp. A mound where a shrine stood until Liudger's men broke it and carried its silver to the bishop and the king.
- **Traiectum** (before 810): Saint Martin's and the school. The bishop wants wax, wine and oil, and a ship for letters to York.
- **Daventre** (before 810): Lebuin's grave under the church Liudger rebuilt; Saxon converts and Frankish soldiers at the same Mass.
- **Diusburg** (799 to 809): Liudger is buying land up the Ruhr for his own house of monks at Werden and wants books, a bell and glass.
- **At sea** (before 866): Fosite's island out in the bight, seen in fog and then clear: a landing, a spring, cattle no one may touch, and since about 791 a small church (sets ms_fosite_seen).
- **Sliesthorp** (826 to 865): 826: Anskar's school for boys he has bought free.
- **Ripa** (855 to 865): The younger Horik gives a plot for a second church.
- **Birka** (829 to 865): 829: Herigar the prefect baptised; Christian captives glad of a priest. 845: the priest driven out.
- **Dorestat** (850 to 863): Catla of Birka comes with her dead mother's silver for the poor of Dorstadt, is led round the holy places by devout women, buys four pennies' worth of wine when they are weary, and finds her bag full again (Rimbert c. 20).
- **The crew** (before 866): Ravn keeps the old gods and Aldger and Beornwine keep the fasts: a fast day and a feast aboard one small boat (npc_text_encounters.md sample 22).
- **The later ages** (from 866): Harald Bluetooth baptised about 965 after the priest Poppo carries hot iron; bishops at Schleswig, Ribe and Aarhus from 948; Iceland takes the faith by law in 1000 and keeps a few old customs.
- **Grobiņa** (850 to 855): About 854 the Svear come against the Cori: at the siege their lots find no god willing to help, until merchants among them say to ask Christ, and the stronghold yields (Rimbert c. 30).

### 6. The blood owed at York

`york_blood`, 773 to 812.

Twenty years before the game begins a Frisian merchant killed a Northumbrian nobleman's young son in a brawl at York, and every Frisian in the city took ship for home for fear of the dead man's kin. Liudger, then a student under Alcuin, was sent home with them, carrying a load of books. No source says the Frisians ever went back: in the game the blood-price was never paid, a Dorestad widow's storehouse stands on what one ship brought away, your own steersman was the boy aboard her, and the richest market of the north is shut to Frisians until someone pays.

**What is true.** Altfrid, Vita Liudgeri I.11-12 (written 839-849; ed. Diekamp 1881): Liudger stayed at York three years and six months; 'as the citizens were going out to war against their enemies it happened that in a brawl the son of a certain count of that province was killed by a Frisian merchant, and for that reason the Frisians made haste to leave the land of the English, fearing the anger of the dead youth's kinsmen' (our rendering). Alcuin 'compelled by necessity' sent Liudger away with those merchants and his own deacon Putul with him, fearing that for love of learning he would go to some other city of that land and be caught by the vengeance; Liudger came home with a fair voyage 'having with him a store of books'; Putul went on to Rome and later came to Gaul with Alcuin as a priest. (The full account, with every source, is in `spine.json`.)

**Who.** Aldger (invented), Thiadburg (invented), Thiadbod (invented), Ecgwulf (invented), Liudger, Alcuin, Eanbald, Sigewine (invented).

**Flags the cards share** (what each means is in `spine.json`): `yk_feud_known`, `yk_named`, `yk_price_set`, `yk_paid`.

**What to place where.**

- **Eoforwic** (before 813): The wic by the Foss: a Frisian hull is looked at twice, asked whose she is (sets yk_named), and may trade only through an English host.
- **Dorestat** (before 813): Thiadburg's storehouse on the dam: she tells it plainly (sets yk_feud_known), will put up part of the price, and wants her son named in the settlement.
- **Doccinga** (before 813): Aldger's home terp and his father's grave; his mother still has the ship's boy's wage from that voyage, unspent.
- **Traiectum** (before 813): The books Liudger carried home from York are in the school's chest: an easter egg, with the year he came.
- **Gyrwe** (before 813): A monk who was a boy at York's school with Liudger remembers the tall Frisian who did not want to leave. Also at Lindisfarena.
- **Quentovic** (before 805): An easter egg: Alcuin's old deacon Putul, who sailed home with Liudger and the merchants, is a priest in Gaul now and can be asked.
- **The crew** (before 813): Aldger will not go ashore at Eoforwic under his own name, and says why only when asked on a quiet night (sets yk_feud_known).
- **At sea** (before 813): A Northumbrian ship hailed off the Humber whose master asks, pleasantly, what port a Frisian is bound for.
- **The later ages** (from 813): On 1 November 866 the great army takes York and the old feud is nobody's business.

### 7. Silver, stamped and weighed

`silver_road`, 786 to 1015.

In the game's first winter the king makes his penny heavier and orders that it be taken in every place, town and market: old coin and foreign silver go to the mint, at a loss the moneyer names. North of the Eider no stamp counts: silver is weighed on a folding scale, nicked to test it and cut to make the weight. And from about this time a third silver comes up the rivers of the east to Aldeigjuborg, Gotland and Birka, coins of the caliphs paid for with furs, wax, swords and people, so that a Frisian who learns all three reckonings can carry the east's silver to the Rhine.

**What is true.** The reform: the heavier denier of about 1.7 g and the pound of 240 are dated to 793-794; the synod of Frankfurt, June 794, c. 5 orders that the new deniers with the king's name, if of pure silver and full weight, be accepted everywhere, with a fine for a free man who refuses them (MGH Capitularia I no. 28; chronicle_793.json 794-06-frankfurt-new-denier). Offa's broad heavy penny follows about 792-793 (793-offa-heavy-penny). Thionville, 805: coin to be struck only at the palace (805-12-thionville-coin). Pitres, 864: a new coinage from ten named mints, Quentovic and Rouen among them. In England one coinage under Æthelstan (Grately, about 928) and one design recalled every few years under Edgar (973). (The full account, with every source, is in `spine.json`.)

**Who.** Sigimar (invented), Holmfast (invented), Hartwin (invented), Dragomir (invented), Wighelm (invented), Ravn (invented), Ohthere, Wulfstan.

**Flags the cards share** (what each means is in `spine.json`): `sv_first_dirham`, `sv_scale`, `sv_moneyer_trusts`, `sv_old_coin`.

**What to place where.**

- **Dorestat** (before 830): The mint in the upper town. From 794 the moneyer Sigimar takes old deniers and sceattas by weight less his cut, and the toll man refuses them (reform.coin; sets sv_old_coin if you try).
- **Colonia** (before 870): A mint and the archbishop's clerks: new coin for old at the king's rate, and a clerk who reads out the Frankfurt rule to anyone who argues.
- **Quentovic** (before 865): The Channel mint: English pennies go into the pot and come out Frankish. After 864 one of only ten mints left. Also at Rotomagus.
- **Lundenwic** (before 830): Offa's moneyers, who sign their coin (Eoba, Ibba and others), change deniers at a loss.
- **Ripa** (before 950): No coin passes by its face: everything on the scale, old sceattas too.
- **Birka** (before 976): Holmfast counts twice and weighs once more.
- **Aldeigjuborg** (from 793): The head of the road: furs, wax, honey and people go down the rivers, silver comes up. A dealer pays in dirhams and wants swords.
- **Reric** (before 809): Dragomir has a cut dirham on a thong as a curiosity and wants to know what the Franks would give for a bag of them.
- **The crew** (before 830): Ravn tests silver with a knife and his teeth and trusts no stamp; Aldger trusts the king's penny and nothing else.
- **At sea** (from 793): A Svea ship hailed in the Baltic, low in the water with a chest nobody mentions; a Frisian who will change coin over the gunwale at a worse rate than any mint.
- **The later ages** (from 830): About 900-950 the flood of Samanid silver; after 1010 none, and pennies of Æthelred and Cnut by the ten thousand in the north.
- **Paviken** (from 793): Gotland's shipwrights and smiths: men who sail east each summer and come home with silver they put under the floor and never speak of.
- **Áhús** (from 793): A beach market at the river's mouth: bead and comb makers paid in cut silver. The first place a Frisian sees a coin cut in half on purpose.
- **Truso** (before 851): Amber weighed against silver at the lake market of the Este; Danish craftsmen with scales; wax and honey from inland.
- **Helgö** (before 801): The old craft island going quiet as Birka grows: bronze-casters, weights, and things from very far away in a chief's keeping.

### 8. The hungry years

`hungry_years`, 792 to 807.

The game opens in a famine: the harvest of 792 failed across the Frankish lands and in Northumbria, men saw false grain standing in the fields that could be touched but not eaten, and the poor are on every road. At Frankfurt in June 794 the king names the most that may be asked for a measure of oats, barley, rye and wheat and for a penny's worth of bread, makes one new measure for the whole realm, and sells his own grain cheaper still. In 805 and 806 hunger comes again, and the law forbids selling food out of the realm or buying a harvest cheap to sell it dear: the grain that would make a merchant rich is the grain he may not carry.

**What is true.** Famine of 792-794: Annales Laureshamenses and Chronicon Moissiacense 793, Annales Mosellani 792 (a year behind), which tell of people eating people; false grain in fields, woods and marshes (chronicle_793.json 793-great-famine, 793-spring-false-grain). Northumbria: 'these tremendous tokens were soon followed by a great famine' (Anglo-Saxon Chronicle 793, Giles 1847: texts_793.json chronicle_793), and blood-red rain from the roof of Saint Peter's at York in Lent (793-spring-york-blood-rain). (The full account, with every source, is in `spine.json`.)

**Who.** Folcbert (invented), Hartwin (invented), Hamacar, Thiadbod (invented), Thiadburg (invented), Higbald, Brunger (invented).

**Flags the cards share** (what each means is in `spine.json`): `hy_fed_poor`, `hy_sold_dear`, `hy_ran_grain`, `hy_new_measure`.

**What to place where.**

- **Dorestat** (before 795, and 805 to 807): The first summer: bread is dear, the church doors are crowded with the poor, and grain is the cargo everyone asks for.
- **Traiectum** (before 795, and 805 to 807): The bishop is ordered to feed the starving until harvest and has not the grain: he will pay in prayers, in a charter, or in the church's silver for a cargo sold fairly (sets hy_fed_poor).
- **Colonia** (before 795, and 805 to 807): The Rhine is the road grain comes down. The king's estates sell his own grain below the cap; a steward wants hulls to move it and pays in grain.
- **Parisius** (before 795): Saint-Denis and the other abbeys feed hundreds at their gates.
- **Eoforwic** (before 795): The same year as the dragons in the sky and the blood on the minster roof in Lent: famine, and then the heathen at Lindisfarne.
- **Winaldum** (before 795, and 805 to 807): The terp country grows cattle, not corn, and buys its bread every year: in a famine year a terp will pay in cloth, cheese, hides or a daughter's dowry silver.
- **Ripa** (805 to 807): The Danes pay well for grain in any year and very well in this one, and the law of 805 says no food leaves the realm (sets hy_ran_grain).
- **The crew** (before 795, and 805 to 807): Ship's bread is short and dear; the men watch what the merchant does with a hold full of wheat in a hungry port.
- **At sea** (before 795): A boat of people who have left a famine village, asking only to be set down where there is bread.
- **The later ages** (from 808): The king's price outlives the famine as law.

### 9. Swords under the ballast

`swords_north`, 779 to 900.

A Frankish blade is the best in the north and worth three times its price at Sliesthorp or Birka, and since 779 the king's law says no mail coat leaves the realm. In 803 no merchant may be given one, in 805 nothing goes east but by the king's border posts and no arms at all, in 811 churches may not let their swords and mail go to strangers, and in 864 the price of arming Northmen is a man's life. The blades go north all the same, by the hundred; and the emperor's own heathen ally, Thrasco of the Obodrites, cannot lawfully be sold the swords he fights the emperor's wars with.

**What is true.** Capitulary of Herstal, March 779, c. 20: byrnies (bruniae) are not to be sold outside the realm (MGH Capitularia I no. 20). Capitulary for the missi, 803: no arm-rings or byrnies to be given to merchants (chronicle_793.json 803-arms-to-merchants-803). Thionville, late 805, c. 7 (MGH Capitularia I no. 44): merchants going to the Slavs and Avars may go only as far as Bardowick, Schezla, Magdeburg, Erfurt, Hallstadt, Forchheim, Premberg, Regensburg and Lorch, each post under a named officer; they are to carry no arms or byrnies for sale, and if found carrying them lose all their goods, half to the palace and half shared between the officer and the finder (805-12-thionville-border-posts). (The full account, with every source, is in `spine.json`.)

**Who.** Thiadbod (invented), Thrasco, Thorleif (invented), Asulf (invented), Holmfast (invented), Folcbert (invented), Ravn (invented), Brunger (invented).

**Flags the cards share** (what each means is in `spine.json`): `sw_ran_blades`, `sw_marked`, `sw_buyer_known`, `sw_thrasco_armed`.

**What to place where.**

- **Colonia** (before 901): The smiths' street. A blade may be sold to a count's man and to no stranger; a dealer who asks no questions wants silver by weight, not coin.
- **Dorestat** (before 864): The toll man's search (the card harbour.tollman.frankish: hide the swords under the ballast).
- **Medemelacha** (before 901): The inland water north from Dorestad, out of the count's sight: where a cargo changes boats at night.
- **Reric** (798 to 808): Thrasco's men fight the emperor's wars with what they can get.
- **Sliesthorp** (before 901): The buyers: a Dane pays double in weighed silver; Asulf pays in captives (sets sw_buyer_known). Also at Ripa.
- **Ögvaldsnes** (before 901): Thorleif wants swords more than silver, and gives a walrus tusk or an English boy for one.
- **Birka** (before 901): Swords go on east: Holmfast buys blades without hilts and sells them at Aldeigjuborg for furs and dirhams.
- **At sea** (800 to 900): A king's ship off the Frisian coast stops and searches (npc_text_encounters.md sample 18): the ballast, the casks, the bilge.
- **The crew** (before 901): Ravn knows who buys and says the Franks made the law because they fear the edge. Aldger will not row a cargo that can hang him.
- **The later ages** (from 864): From 864 the penalty is death, and the blades still reach the north for two hundred years.

### 10. The book road

`book_road`, 793 to 875.

A book is the dearest thing a small boat can carry: the skins of a herd of calves, a year of a scribe's life, and worth more than the boat. They cross the sea in merchants' hulls with the monks who beg passage: Liudger's from York to Utrecht, Alcuin's from York to Tours, Anskar's forty toward Birka, which pirates took. The same books are what the heathen carry off for their covers and what Christians buy back with gold, so a merchant who learns what a book is worth, and to whom, has a trade no rival understands.

**What is true.** Liudger came home from York about 773 'having with him a store of books' (Altfrid, Vita Liudgeri I.12, our rendering). Alcuin's verses on York list the authors in its library, and from Tours in 796-797 he asks the king to let him send boys to fetch from York 'the flowers of Britain', the books he lacks (MGH Epistolae IV no. 121) (?). Lul of Mainz and others wrote to Wearmouth-Jarrow for Bede's works a generation earlier (Boniface and Lul's letters) (?). The Admonitio generalis of 789, c. 72, orders schools and corrected books, copied by grown men with all care. (The full account, with every source, is in `spine.json`.)

**Who.** Alcuin, Sigewine (invented), Liudger, Hamacar, Higbald, Gervold, Eanbald, Anskar, Beornwine (invented), Cellach.

**Flags the cards share** (what each means is in `spine.json`): `bk_carried`, `bk_knows_worth`, `bk_parchment`, `bk_lost`.

**What to place where.**

- **Eoforwic** (before 867): The minster library, the richest north of the Alps.
- **Gyrwe** (before 870): Bede's house: the continent still writes for copies of his books, and the scriptorium wants calfskin, pigments and a ship that does not sink.
- **Traiectum** (before 858): The school reads the books Liudger carried from York. The bishop wants more and pays in standing, wax or a charter (sets bk_parchment).
- **Diusburg** (799 to 881): Liudger's new house at Werden needs a library from nothing: a first cargo of books up the Ruhr.
- **Rotomagus** (before 842): Saint-Wandrille's new school and the books Gervold gave it. Up the Seine, Saint-Denis by Parisius buys parchment by the bundle at the October fair.
- **Namnetis** (796 to 843): The way up the Loire to Tours, where Alcuin's scribes turn out whole Bibles in a clear new hand: a monk wants passage upriver with skins, or down with a finished book for a bishop in the north.
- **Fordwic** (from 793): Christ Church's scribes (the riddles of Tatwine).
- **Ögvaldsnes** (before 876): A book in a heathen house: the cover gone for a brooch, the leaves kept because the pictures are fine.
- **Sliesthorp** (826 to 865): Anskar's school needs psalters. In 829 he sets out for Birka with nearly forty books and comes there on foot with none: someone has them.
- **Hī** (before 826): The great gospel book in the making, and after 806 the question of carrying it to Ireland in a boat.
- **The crew** (before 876): Beornwine can read a book aloud at anchor, and wants to.
- **The later ages** (from 876): Books bought back from armies; King Alfred's own books sent to every see about 890; the Exeter book of riddles written about 970 (riddle_road).

### 11. Dorestad falls, and the rivers with it

`dorestad_falls`, 834 to 911.

From 834 a Danish fleet comes up the river to Dorestad year after year: it burns, kills, carries people off and takes tribute, and in 846 the emperor watches from Nijmegen and can do nothing. The answer the kings find is to give a river to a Northman on his oath to keep the others out: Dorestad to Rorik in 850, and sixty years later Rouen and the mouth of the Seine to Rollo. After the last sack in 863 the Rhine itself leaves Dorestad and its merchants go to Tiel, Deventer and Utrecht; Rollo's Rouen becomes the best market on the Channel.

**What is true.** The Rhine: Annales Bertiniani 834, 835, 836, 837 (tribute taken), 847, 850, 863; Annales Fuldenses 836 (Antwerp and Witla burned), 850, 857; Annales Xantenses 834-873 (chronicle_793.json). Louis the Pious's grant of Rustringen to Harald in 826; Lothar's grant of Walcheren to Harald in 841 and of Dorestad to Rorik in 850, after Rorik had taken it, on condition that he handle the king's dues and hold off the pirates; in 857 Rorik takes land between the sea and the Eider; in 863 Dorestad is sacked for the last time and the fleet goes on up the Rhine; in 867 the people the annal calls Cokingi drive Rorik out; he meets Charles the Bald at Nijmegen in 870 and swears to Louis the German in 873; in 882 his lands go to Godfrid, who is killed in 885. (The full account, with every source, is in `spine.json`.)

**Who.** Rorik, Eystein (invented), Horik, Catla, Frideburg, Anskar, Rollo, Thiadbod (invented), Thiadburg (invented), Gerlef (invented).

**Flags the cards share** (what each means is in `spine.json`): `df_buried`, `df_warned`, `df_rorik_man`, `df_moved`.

**What to place where.**

- **Dorestat** (834 to 863): Each raid year: loot sold cheap, survivors, a widow, silver put in the ground (sets df_buried). 837: tribute. 846: burned in sight of the emperor.
- **Traiectum** (834 to 919): The fleet of 834 comes by Utrecht. In 857 the church is wasted and the bishop goes inland for a lifetime.
- **Witla** (834 to 836): The last seasons of the Maas-mouth market: in 836 it burns and is not rebuilt. The pilot's family take their marks elsewhere.
- **Walichrum** (837 to 850): 837: Count Eggihard and the Dane Hemming, Halfdan's son, die defending the island.
- **Noviomagus** (846 to 881): The palace from which Lothar watches Dorestad burn in 846; where Rorik meets King Charles in 870; held and burned by Northmen in 880-881.
- **Daventre** (863 to 911): Where Dorestad's trade goes: new plots on the IJssel, old Dorestad names on them. Burned in 882.
- **Medemelacha** (838 to 839): The winter after the flood of Saint Stephen's day 838: drowned terps, dead cattle, salt in the wells; everything is wanted and there is nothing to pay with.
- **Sliesthorp** (834 to 854): King Horik says the fleets are not his, and that he has hanged their chiefs; men in the harbour sell Dorestad cloth and Dorestad people.
- **Rotomagus** (841 to 942): 841: sacked. A lifetime as the raiders' harbour, where loot and captives are sold.
- **Parisius** (845 to 911): 845: the fair's silver goes to buy the fleet off. 885-886: the siege. The October fair shrinks and moves behind walls.
- **Herio** (834 to 911): The monks gone since 836, the salt pans worked by whoever holds the island; Nantes stormed in 843, Bordeaux taken in 848.
- **The later ages** (from 834): The heir's choices: bury or carry, swear to the Dane or not, stay on a dying river or move. One feud and one friend pass down.
- **Andoverpis** (834 to 836): In 836 the Northmen burn the fort and landing on the Scheldt in the same summer as Witla.
- **Burdigala** (844 to 848): Northmen on the Garonne from 844; the city is taken in 848. Wine still wants buyers, and buyers a way out to sea.

### 12. The price of England

`price_of_england`, 865 to 1066.

In 865 a great heathen army lands in East Anglia and does not go home: York falls in 866, kings are killed or driven out, and in 876 the army shares out Northumbria and begins to plough, until Alfred beats it in 878 and there are two laws in England, with hostages given before a man may cross to trade. A hundred years on the raids begin again and the English king pays, ten thousand pounds after Maldon in 991 and more each time, until in 1016 the Dane Cnut is king of all of it, pays off his fleet with seventy-two thousand, and keeps one peace on both shores of the North Sea. It breaks at his death, and in the autumn of 1066 three kings fight for England; the house's story ends at Christmas with a Norman crowned at Westminster.

**What is true.** Anglo-Saxon Chronicle 865-1066 (Giles 1847; chronicle_793.json). The army: East Anglia 865, York on 1 November 866, King Edmund killed 20 November 869, Mercia's king driven out 874, Northumbria shared out 876; the Danes swear to Alfred 'on the holy ring' at Wareham in 876 (Giles has 'bracelet'), which they would do for no people before; Edington, May 878; the army crosses to Ghent in 879; Alfred holds London from 886. The treaty of Alfred and Guthrum (880s): no man, free or slave, crosses to the other army without leave, but if they need to trade in cattle and goods it is allowed when hostages are given as a pledge of peace. Lindisfarne's people leave their island in 875. Ohthere and Wulfstan tell Alfred of their voyages about 890. York is a Norse kings' town until 954. (The full account, with every source, is in `spine.json`.)

**Who.** Cnut, Thorkell, Ohthere, Wulfstan.

**Flags the cards share** (what each means is in `spine.json`): `en_hostage_given`, `en_paid_share`, `en_followed_silver`, `en_cnut_peace`.

**What to place where.**

- **Gipeswic** (from 865): 865: the army's winter camp wants horses and gets them. 869: the king killed.
- **Eoforwic** (from 866): 1 November 866: taken. From 876 Danes with land, wives and a market: a town where a Frisian, a Dane and an Englishman can all be cheated in each other's tongue.
- **Lundenwic** (from 871): 871-872: the army winters here and takes its price. 886: Alfred's. About 1000: the toll rules at Billingsgate, read out to every keel.
- **Serræ** (from 980): Thanet ravaged in 980; Thorkell's army lands at Sandwich in 1009 and East Kent pays 3,000 pounds. Also at Fordwic and Dofras.
- **Lindisfarena** (875 to 884): 875: the bishop and the people of Saint Cuthbert leave with the body, the books and Oswald's head, and wander seven years.
- **Ganda** (879 to 881): The great army crosses from England and winters here, selling what it took there.
- **Rotomagus** (from 991): The Norman duke's town shelters English exiles (King Æthelred in 1013) and sells the raiders' loot; its men pay toll in London on wine and blubber fish.
- **Sliesthorp** (from 991): English pennies everywhere, and men home from England with more than they can spend. Burned by Harald Hardrada in 1050; sacked by the Wends in 1066.
- **Traiectum** (from 1000): In 1006-1007 the Northmen burn Tiel and Utrecht's people fire their own harbour quarter.
- **At sea** (from 865): A fleet with a tribute aboard, in a good humour; a ship of Englishmen sold at Rouen or Dublin; in 1066 three fleets in one summer, and the question of whose pilot you are.
- **The later ages** (from 865): For the house: trade across a line of hostages; follow the silver or keep clear of it; a Danish king's peace as the best years the North Sea ever had; and where to stand at Christmas 1066, which is the end of the game.
- **Legacæstir** (from 893): 893: a Danish army winters in the waste city. Restored in 907, then a busy mint and the harbour for Dublin's Norse.

### 13. The riddle road

`riddle_road`, 793 to 1066.

Ravn, the Northman who sails with you, sets the first riddle at Dorestad and gives the next when it is answered; after that riddles are had from people in ports, each in its own tongue and its own letters. Some are parts of one thing held in three ports; some are chains, where the answer to one is a word in the next; and the hardest sets end at a place where silver lies in the ground. The Latin riddles go the monks' way, from school to school, and the Norse ones from hall to hall, where a guest who cannot answer drinks for it.

**What is true.** The riddles and their answers are real and are in riddles_793.json and texts_793.json: Gestumblindi's riddles from the saga of King Heidrek (written down in the 13th century, older in substance; Vigfusson and Powell 1883); the wisdom contests of the lays of Vafthruthnir and Alvis (Bellows 1923); the Old English riddles of the Exeter Book (the book about 970, the riddles 8th to 10th century; Tupper 1910); the Latin riddles of Aldhelm of Malmesbury (died 709), Symphosius (late Roman), Tatwine, archbishop of Canterbury (died 734), and 'Eusebius', taken to be Hwaetberht, abbot of Wearmouth-Jarrow from 716. Riddling was a schoolroom and a hall game: Alcuin wrote a dialogue of riddles for Charlemagne's son Pippin about 790 (Disputatio Pippini cum Albino). (The full account, with every source, is in `spine.json`.)

**Who.** Ravn (invented), Beornwine (invented), Alcuin, Sigimar (invented), Sigewine (invented).

**Flags the cards share** (what each means is in `spine.json`): `rd_last_question`, `rd_setter_known`.

**What to place where.**

- **Dorestat** (before 864): Ravn's first riddle (r-gest-01, ale) and its reward stanza (r-alv-34).
- **Traiectum** (from 793): The cathedral schoolmaster's key (riddle_symphosius_04_clavis).
- **Quentovic** (from 793): A monk on his abbey's business with Aldhelm's salt; a cleric waiting to cross with the ship riddle.
- **Lindisfarena** (before 876): Three leaves of one quire of English riddles, scattered when the scriptorium was plundered: here, at Gyrwe and at Streonaeshalch (the third comes ashore in a sea chest from 799).
- **Fordwic** (from 793): The scribes of Christ Church sell Archbishop Tatwine's own riddles; one frees a ship of toll here.
- **Hleiðr** (from 793): The king's skald sells the hard ones. This is the hall where Ravn was set the last question (sets rd_last_question if he is aboard and trusted).
- **Sliesthorp** (from 793): A ferryman on the Schlei has one of Aegir's daughters; a Vestfold skipper at Skiringssal and a skald at Birka have her sisters.
- **Uppsalir** (from 793): The Svear king's skald; from 871 the chain of Delling's door begins and ends here, by Paviken, Aldeigjuborg and Ahus, and leads to silver under a floor on Gotland.
- **Duiblinn** (from 905): The chain of Ymir's kin among the Norse halls of Dublin and York and at Hi, ending at the silver by the Ribble.
- **At sea** (from 793): A master hailed at sea gives a riddle for news (riddles_793.json acquire 'sea'): Norse, English or Irish by the ship.
- **The crew** (from 793): Reading with the crew (npc_text_encounters.md sample 23): Ravn reads a rune stick in part, Beornwine a Latin leaf in part, and the merchant finishes it.
- **The later ages** (from 860): The later sets open by year (860, 871, 905, 928). An heir inherits the chest, the solved riddles and whatever Ravn told the house before he died.

### 14. The count's day

`counts_day`, 793 to 1066.

The boat was bought on the count's silver: four hundred and eighty pennies, two pounds, to be paid at Michaelmas. Every generation of the house has such a day: a debt to a count, then to the Dane who holds Dorestad's toll, then to a bishop, a count of the coast, a sworn company of merchants. The house lives as long as it can meet the day, or find someone who will stand surety for it.

**What is true.** The debt is the game's own device: 480 d owed for the ship to a Frankish count, due in 365 days (trade_rules.json start; the user's decisions of 26 Sep and 4 Oct 2026: 480 d by Michaelmas on the easy terms). What it rests on: 240 pennies make a pound from the reform of 793-794; a small sea-going boat at about two pounds is the game's own price (?). Michaelmas, 29 September, is among the feasts the council of Mainz ordered kept in 813 (c. 36), and autumn after harvest is the usual term for dues (?). A count holds the king's court and toll in his district; no count of Dorestad is named for the 790s, so cast.folcbert is invented. (The full account, with every source, is in `spine.json`.)

**Who.** Folcbert (invented), Hartwin (invented), Thiadburg (invented), Thiadbod (invented), Sigimar (invented), Hamacar, Rorik, Eystein (invented).

**Flags the cards share** (what each means is in `spine.json`): `db_extension`, `db_service`, `db_surety`, `db_bought`.

**What to place where.**

- **Dorestat** (before 864): The count's hall and his steward Hartwin with the tally.
- **Noviomagus** (before 840): When the king keeps a feast at the palace the count is there: the one place to ask for a delay to his face, with a gift fit for the company (sets db_extension).
- **Traiectum** (from 793): The bishop will stand surety for a house that carries his letters and sells him wax fairly (sets db_surety).
- **Lundenwic** (before 813): An errand for the count: English hounds and a cloak of the old length for his table and his back.
- **The crew** (from 793): The men's shares against the count's day: a crew paid late at the venture's end, or a count paid late (npc_text_encounters.md sample 20).
- **At sea** (from 793): A gale in the last week before Michaelmas with the count's silver in the hold as cargo: what goes over the side.
- **The later ages** (from 813): Each heir begins with a day of his own. 834-863: debts in a town that burns. 850: the Dane's toll. After 863: a new town and a new lord.

## The cast

Forty-four people who appear in more than one port or matter to more than one thread. The four of the crew come first, in the order the crew stories use. A real person has real dates; an invented one is an ordinary person with a name made of attested parts.

**Ravn** (`cast.crew.ravn`; invented; born about 750; with the house from the first day, 21 June 793). People: norse. Role: the hand. Home: Dorestat, also met at Hleiðr, Skíringssalr, Sliesthorp. Talks a great deal and means well; keeps his word, and loses most other things; pious to the old gods in a small way of his own; hands out riddles he only half remembers, and is delighted when somebody finishes one. Wants: To stand in the hall at Hleidr once more and hear the last question asked, the one he was set there as a young man and could not answer (texts_793.json t-gest-36). Fears: To die among Christians unable to speak, be signed with their cross and put in their ground. Voice: Many short sentences, one tumbling over the next; says a thing, then says it was most of it; asked where he is from, he starts three answers and ends on a riddle. Looks: About forty-five, broad and weathered, a full dark beard going grey at the chin, thick hair tied back out of his eyes, a ready grin; a plain dark wool tunic and sea cloak, a knife and a whetstone at his belt, a small leather bag of rune-cut sticks that he is always losing.
Ties: rival Thorleif (knows him of old and does not say how); friend Aldger (ten years in the same boat; they argue about gods only on fast days); feud Asulf (will not sit at a table with a dealer in people; the reason is his own).

**Aldger** (`cast.crew.aldger`; invented; born about 761). People: frisian. Role: the steersman. Home: Doccinga, also met at Dorestat, Eoforwic. Steady, sparing with words, counts tides in his head; baptised as a child at Doccinga and keeps every fast. Wants: To see the blood-price paid at Eoforwic, so that he can walk its strand again under his own name: he was the boy on the ship that fled in 773. Fears: The dead youth's kin; and fog on a lee shore. Voice: Dry Frisian: says the worst first and plainly, then what can be done about it. Looks: A little over thirty, broad, short-legged, sandy hair and a cropped beard, eyes narrowed from watching water; a short Frisian cloak, a wooden cross on a thong, the steering oar's callus on his right hand.
Ties: serves Thiadburg (sailed for her husband Radger as a boy and still does her errands unasked); feud Ecgwulf (was on the ship of the man who killed Ecgwulf's brother); friend Liudger (Liudger taught at Doccinga for seven years and knows his family).

**Brunger** (`cast.crew.brunger`; invented; born about 765 on the Weser). People: saxon. Role: the mate. Home: Daventre, also met at Mogontia, Colonia, Diusburg. Strong, loyal to the boat, slow to speak and slower to forgive; said the baptismal vow because the law is baptism or death. Wants: To find his younger brother, given as a hostage when their district submitted in 785 and sent to a lord in the south; the trail ends at Mogontia about 805. Fears: The king's men, and a priest asking what he ate in Lent. Voice: Few words, Saxon ones; a proverb when he is pleased, silence when he is not. Looks: Late twenties, tall and heavy in the shoulder, long fair hair, a moustache and no beard; an undyed tunic, a long knife (a seax) across his belly.
Ties: feud Folcbert (the count led the levy that burned his village; the count does not know him); friend Beornwine (the boy reads; Brunger thinks reading is how his brother will be found).

**Beornwine** (`cast.crew.beornwine`; invented; born about 775). People: english. Role: the lookout. Home: Gyrwe, also met at Lindisfarena, Eoforwic. Quick, sharp-eyed, bookish and homesick: schooled as an oblate at Gyrwe, he left before his vows and reads Latin slowly but truly. Wants: To find his younger brother Tidwine, an oblate of Lindisfarena carried off on 8 June 793, and bring him home. Fears: The heathen men; and that he left the cloister and God has noticed. Voice: Talks too much when afraid; quotes a psalm and then is embarrassed. Looks: Eighteen, thin, dark hair growing out of a cropped head, ink-stained fingers that the oar has since blistered; a monk's cast-off tunic cut short, a wax tablet in his shirt.
Ties: kin Tidwine (his younger brother); serves Higbald (the bishop knows the family and sends him word); friend Sigewine (taught him his letters at Gyrwe).

**Folcbert** (`cast.folcbert`; invented; born about 748; count over the Dorestad district in the 790s (no count there is named in the sources)). People: frankish. Role: the count. Home: Dorestat, also met at Noviomagus, Daventre. A soldier who holds a court because the king says so: fair by his own lights, bored by merchants, away with the host most summers. Wants: His silver back at Michaelmas, or something better than silver: a service, a rare thing for his table, a favour he can show the king. Fears: The king's envoys finding his toll short or his court bought. Voice: Brief and loud, a man used to being obeyed outdoors; calls every Frisian 'shipman'. Looks: Mid forties, thick-set, short brown hair and a long moustache in the Frankish way, a red cloak pinned on the right shoulder over a mail-worn tunic, leg bindings, a sword he never takes off.
Ties: serves Hartwin (Hartwin is his steward and keeps his tallies); friend Thiadbod (takes Thiadbod's gifts and thinks well of him); rival Hamacar (count and bishop share the district and dispute every fine).

**Hartwin** (`cast.hartwin`; invented; born about 740). People: frankish. Role: the steward. Home: Dorestat. The count's steward: remembers every debt in the district and cuts each on a split tally stick; honest, and proud of being hard to cheat. Wants: The tally to close clean on the day; failing that, a pledge he can hold in his hand. Fears: Being blamed for his lord's shortfalls; fire in the hall where the tallies hang. Voice: Counts aloud, in dozens and scores; never raises his voice. Looks: Past fifty, thin, grey, clean-shaven, a plain brown cloak with a bronze pin, a bundle of notched sticks at his belt and a small pair of scales in a box.
Ties: serves Folcbert (his lord for twenty years); friend Sigimar (the steward and the moneyer check each other's weights).

**Thiadbod** (`cast.thiadbod`; invented; born about 755; his sons and grandsons keep the house after him). People: frisian. Role: the skipper. Home: Dorestat, also met at Lundenwic, Hamwic, Ripa, Sliesthorp, Mogontia. The rival: a bigger ship, a dam head of his own, a gift for every officer; never breaks a law where he can be seen. Wants: To be the first Frisian house on the river, and to own the player's debt so as to own the player. Fears: A search of his wine casks on the wrong day; a younger, luckier skipper. Voice: Friendly, generous with news that is a week old, never with news that is fresh. Looks: About forty, well fed, combed fair beard, a long blue cloak of the best Frisian cloth with a silver pin, rings on two fingers, soft boots that have never hauled a boat.
Ties: kin Thiadburg (his elder sister; she does not trust him with her storehouse); friend Folcbert (gives the count hawks and wine); owes Asulf (blades went north on credit; Asulf is owed captives or silver); friend Wighelm (the reeve lets his ship land when others are turned away).

**Thiadburg** (`cast.thiadburg`; invented; born about 750; widowed about 785). People: frisian. Role: the widow. Home: Dorestat. Keeps the storehouse on the dam her husband built with what one ship brought away from York; lends on goods, forgets nothing, gives to the church without saying so. Wants: The blood-price her husband Radger owed at Eoforwic paid before she dies, so her son can trade there; she will back a skipper who takes it. Fears: That her brother Thiadbod will have the storehouse when she is dead. Voice: Plain, quick, a little amused; asks what a thing weighs before she asks what it cost. Looks: A little over forty, upright, a white linen head-cloth, a dark gown with two round brooches and a string of amber and glass beads, a ring of keys at her belt.
Ties: kin Thiadbod (her younger brother, and her rival for the dam head); owes Ecgwulf (her dead husband killed his brother in a brawl in 773; the wergild was never paid); friend Aldger (he was her husband's ship's boy).

**Sigimar** (`cast.sigimar`; invented; born about 752). People: frankish. Role: the moneyer. Home: Dorestat. The moneyer of the king's mint in the upper town: exact, incurious about people, curious about every coin; the first man on the Rhine to puzzle at a dirham. Wants: Old pennies and foreign silver brought to him and not to the scales of a Dane; to know what the strange lettering says. Fears: A light coin with his mint's name on it; the king's law of 794 and the men who enforce it. Voice: Says a weight, then a loss, then nothing; warms only to a coin he has not seen before. Looks: About forty, stooped, burn-scarred forearms, a leather apron, a cap, a touchstone on a cord round his neck; behind him the anvil, the dies and a guard.
Ties: friend Hartwin (they check each other's weights); rival Holmfast (the Svea weighs silver the moneyer says must be struck).

**Gerlef** (`cast.gerlef`; invented; born about 745). People: frisian. Role: the pilot. Home: Witla, also met at Walichrum, Lundenwic, Dofras, Dorestat. The pilot of the Maas mouth: knows where the channel went after each winter's floods, and the Thames to the Strand; tells the truth about water and lies about his age. Wants: A son or a pupil to take the marks from him before his eyes go. Fears: Going blind; a new sandbank where his father's channel was. Voice: Talks to the water more than to the merchant: 'she shoals here', 'she runs north of the bank now'. Looks: Nearly fifty and looks older, wiry, white stubble, a salt-stiff hooded cloak, a sounding pole taller than himself and a lead on a line with tallow in its foot.
Ties: serves Thiadbod (Thiadbod pays him by the year, so others wait); taught Aldger (taught Aldger the Thames marks).

**Notburg** (`cast.notburg`; invented; born about 730). People: frisian. Role: the wise woman. Home: Emuthon. The wise woman of the Ems mouth: herbs, births, knots against fever, the right words at a spring; baptised twice by order and unmoved both times. Wants: The old customs left alone at the edges: the silence at the spring, a feast at the graves in the dark of the year. Fears: Not the priests, who only talk: the count's men, and being forgotten. Voice: Kind, exact, a little mocking; calls Liudger 'the Utrecht boy'. Looks: Past sixty, small, straight-backed, grey plaits under a dark cloth, a pouch of dried herbs, a knotted cord and a small knife at her girdle, a staff of ash.
Ties: friend Bernlef (they are of an age and know the same stories); rival Liudger (he breaks what she tends, and asks after her health).

**Sigewine** (`cast.sigewine`; invented; born about 758). People: english. Role: the monk. Home: Eoforwic, also met at Quentovic, Dorestat, Traiectum, Lindisfarena, Gyrwe. A Northumbrian monk who carries Alcuin's letters and brings back books: a bad sailor, a good walker, incapable of passing a library. Wants: A ship that sails when it says it will; a copy of a book York lacks. Fears: The sea; arriving with a letter after the man it is for has died. Voice: Apologises, then asks for exactly what he needs; complains of the cold like a scribe in a margin. Looks: Mid thirties, tall and stooping, tonsured, a patched black cowl, a leather letter case under his arm and a wax-sealed satchel he will not put down.
Ties: serves Alcuin (his old master, whose letters he carries); taught Beornwine (taught the boy his letters at Gyrwe); friend Higbald (brings him Alcuin's answers); owes Gervold (crosses free at Quentovic by the abbot's word).

**Wighelm** (`cast.wighelm`; invented; born about 750; wic-reeve under Offa and then Coenwulf). People: english. Role: the reeve. Home: Lundenwic, also met at Serræ. The king's wic-reeve on the Strand: witnesses every sale, enforces the ban on Frankish ships with regret and a private price. Wants: The ports open again, and the first good cargo after the peace to pass through his hands. Fears: King Offa, who beheaded a king in 794; after 796, the men who remember whose reeve he was. Voice: Courteous, slow, asks the same question twice to see if the answer changes. Looks: About forty-five, grey-fair, clipped beard, a green cloak with a gilt disc brooch, a short sword, a rod of office, two armed men behind him.
Ties: friend Gervold (they write to each other through the ban); friend Æthelheard (both are Offa's men in the south); friend Thiadbod (takes his gifts).

**Ecgwulf** (`cast.ecgwulf`; invented; born about 755). People: english. Role: the lord's man. Home: Eoforwic, also met at Bebbanburg. A Northumbrian thegn whose elder brother, a nobleman's son, was killed by a Frisian merchant in a brawl when Ecgwulf was a youth; patient, correct, and still counting. Wants: The blood-price, paid before witnesses, or a Frisian of that house within reach. Fears: Dying with it unpaid, which would shame his sons. Voice: Formal and quiet; names his brother every time he speaks of Frisians. Looks: Late thirties, tall, a scar through one eyebrow, long hair and a full beard, a dark red cloak with a silver ring pin, a pattern-welded sword in a worn scabbard.
Ties: feud Thiadburg (her husband struck the blow in 773); feud Aldger (the boy on the ship is a man now); serves Eanbald (his lands lie under the archbishop's protection).

**Tidwine** (`cast.tidwine`; invented; born about 781; carried off from Lindisfarena on 8 June 793). People: english. Role: the captive. Home: Lindisfarena, also met at Ögvaldsnes, Skíringssalr, Sliesthorp, Birka. An oblate of twelve who could write a fair hand: sold north, he lives by being useful and forgets his English a word at a time. Wants: At first, home. Later, he is not sure: he has a place, a skill and perhaps a wife among the heathen. Fears: Being sold again. Voice: As a boy, whispers; as a man, speaks Norse with an English turn and Latin only in his sleep. Looks: As a boy: small, cropped hair, a torn habit, bare feet, a thrall's collar of rope. As a man of forty: a trader's servant in northern dress who still holds a stick like a pen.
Ties: kin Beornwine (his elder brother); serves Thorleif (taken on Thorleif's ship (?)); serves Holmfast (bought for his tallying and kept); taught Higbald (the bishop's own oblate).

**Asulf** (`cast.asulf`; invented; born about 755). People: danish. Role: the dealer in captives. Home: Sliesthorp, also met at Ripa, Hleiðr, Reric, Skíringssalr. A dealer in captives who thinks himself an honest man: feeds his lots, keeps families together when it costs nothing, remembers every sale. Wants: Frankish blades, which buy more captives than silver does; and a quiet trade. Fears: A king who decides the trade is his; sickness in the pen. Voice: Says less than he means; names prices in ounces of silver and never haggles twice. Looks: Around forty, stout, a plaited beard, a good grey cloak with a silver ring pin, an arm ring he cuts pieces from to pay, a switch of hazel in his hand.
Ties: serves Godfred (sells what the king's wars take); rival Thiadbod (is owed for a lot Thiadbod took on credit); friend Thorleif (buys what Thorleif brings home); feud Ravn (Ravn will not sit with him).

**Thorleif** (`cast.thorleif`; invented; born about 760). People: norse. Role: the chief of the raiders. Home: Ögvaldsnes, also met at Skíringssalr. A ship's chief of the west coast who farms in winter and sails in summer; was west over the sea in 793 and came home rich in things he has no use for. Wants: Swords and a second ship; a name that outlasts him. Fears: A summer with nothing to show; his own crew's opinion. Voice: Understates everything: a sacked church is 'a good landing'. Laughs before he threatens. Looks: Early thirties, big, wind-burned, red-fair hair and beard, a blue tunic with tablet-woven braid, a silver neck ring, an axe with a long haft; on his wife's breast a brooch that was once a book's mount.
Ties: friend Asulf (sells him the people he takes); rival Ravn (they know each other of old); owes Tidwine (nothing, as he sees it: he took the boy and sold him).

**Dragomir** (`cast.dragomir`; invented; born about 760; of Reric until 808, then set down at Sliesthorp). People: wendish. Role: the merchant. Home: Reric, also met at Sliesthorp. An Obodrite trader in honey, wax, furs and people: careful with oaths, generous at table, never forgets a service or a slight. Wants: Before 808: Frankish iron and a friend in the west. After: word of his kin, and a way home that does not pass Godfred's gate. Fears: The Danish king's ships in the bay; after 808, being counted a Dane's man by his own people. Voice: Speaks through an interpreter until he trusts you, then in rough Danish; will not swear lightly, and says why. Looks: About thirty-five, wide-faced, brown hair cut round, a moustache, a long belted tunic and a fur-edged cap, a ring of twisted silver wire at his temple (his wife wears several), a wax tally in his hand.
Ties: serves Thrasco (his prince, whose toll he paid); feud Godfred (the king who burned his booth and carried him off); rival Asulf (they sold in the same market and now in the same harbour).

**Holmfast** (`cast.holmfast`; invented; born about 758; his son keeps the booth after him). People: svea. Role: the merchant. Home: Birka, also met at Aldeigjuborg, Paviken, Sliesthorp, Dorestat. A Svea trader who counts everything twice and weighs it a third time: furs east to west, silver west to east, and no word wasted on either. Wants: A steady buyer on the Rhine for furs and wax, and Frankish glass and blades to take east. Fears: Short weight; the ice coming early with his goods on the wrong side of it. Voice: Numbers first. Repeats your offer back to you slowly, then is silent until you raise it. Looks: About thirty-five, spare, pale-eyed, a trimmed beard, a fur-trimmed cap and a belted coat of eastern cut, folding scales in a round bronze case and a bag of lead weights at his belt.
Ties: rival Sigimar (the scale against the stamp); owes Tidwine (bought the boy for his tallying; owes him, by his own reckoning, nothing); friend Dragomir (they have traded wax for years without a quarrel).

**Eystein** (`cast.eystein`; invented; born about 815; Rorik's man at Dorestad from 850). People: danish. Role: the toll man. Home: Dorestat, also met at Traiectum. A Dane taking a Frankish king's toll for a Danish lord: exact, a little ashamed of the work, proud of keeping other pirates off the river. Wants: The toll full, so that his lord keeps his grant; a Frisian wife and a house that does not burn. Fears: The next fleet being bigger than his lord's; being left behind when Rorik sails. Voice: Frisian with Danish bones in it; says 'the king's tenth' and means two kings. Looks: Mid thirties, fair, a moustache, a Frankish cloak over a northern tunic, an arm ring beside a toll man's tally stick; he may wear a small cross for the day's business.
Ties: serves Rorik (his lord); friend Thiadbod (Thiadbod's grandsons pay him well).

**Gervold** (`cast.gervold`; real; bishop of Evreux from about 785; abbot of Saint-Wandrille from 787 (or 788) until his death in 806 or 807). People: frankish. Role: the abbot. Home: Quentovic, also met at Rotomagus, Lundenwic. A king's officer more than a monk: the procurator who takes the tolls of the Channel ports, Offa's friend, by his own monks' account no great scholar and a builder. Wants: The two kings at peace so the tolls flow again; English lead for the roofs of his abbey's churches. Fears: The king hearing that the tolls are short, or that his friendship with Offa was worth more to him than his office. Voice: Smooth and exact; names sums and saints in the same breath; never says no at once. Looks: About fifty-five, portly, tonsured, a fine dark cowl with a silver cross, an abbot's staff carried by a boy, a seal on a cord and a clerk with tablets at his elbow.
Ties: friend Wighelm (they write to each other while the ports are shut); rival Alcuin (each is spoken of as the man to make the peace with Offa); friend Sigewine (lets Alcuin's letter-carrier cross free).

**Alcuin** (`cast.alcuin`; real; born about 735 at York; at Charles's court from 782; in Northumbria 790-793; back at court by 793; abbot of Saint Martin's at Tours from 796; died 19 May 804). People: english. Role: the abbot. Home: Eoforwic, also met at Noviomagus, Quentovic, Namnetis. The best teacher of his age and a tireless writer of letters: homesick for York's books, fond of riddles and pet names, scolds kings and bishops alike. Wants: His letters carried and answered; the youths of Lindisfarne ransomed; York's books copied for Tours. Fears: That the sack of Saint Cuthbert's church is a judgement on Northumbria's kings, and the beginning of worse. Voice: Warm, learned, turns every piece of news into a lesson; met mostly as a letter read aloud. Looks: Nearing sixty, slight, tonsured grey, a deacon's plain dark dress, ink on his fingers, a pile of wax tablets and a boy to carry them; never seen without a book.
Ties: taught Liudger (his pupil at York for three years and a half); taught Eanbald (his pupil, whom he scolds as archbishop); friend Higbald (writes to him after the raid); friend Æthelheard (writes to him in his exile); taught Sigewine (who carries his letters).

**Higbald** (`cast.higbald`; real; bishop of Lindisfarne from 780 or 781 until his death on 25 May 803). People: english. Role: the bishop. Home: Lindisfarena, also met at Bebbanburg. The bishop who was there on 8 June 793 and stayed: counts what is gone, writes to Alcuin, keeps the brethren on the island. Wants: The youths found and bought back; the shrine made whole; wax and wine for the altar. Fears: A second sail; the brethren asking to leave the island. Voice: Tired and exact; lists what is missing on his fingers. Looks: Past sixty, gaunt, tonsured white, a patched habit and a plain wooden staff since the good one was taken; behind him a church with its doors off.
Ties: friend Alcuin (Alcuin's letters of 793 are to him); taught Tidwine (one of the youths carried off); friend Beornwine (knows the brothers' family).

**Liudger** (`cast.liudger`; real; born about 742 near Utrecht; priest 777; teacher of five East Frisian districts from 787; driven out about 793 and back within a year; founds Werden about 799; bishop at Mimigernaford (Munster) 30 March 805; died 26 March 809). People: frisian. Role: the priest. Home: Emuthon, also met at Doccinga, Traiectum, Daventre, Diusburg. A Frisian nobleman's son taught at Utrecht and York: gentle with people, hard on shrines, always carrying books. Wants: Priests, books and a bell for the Frisians and the western Saxons; a house of monks on his own land at Werden. Fears: The Northmen: he dreamed of the sun fleeing over the sea from the north with black cloud behind it, and wept. Voice: Plain Frisian with a little of York's English in it; asks before he tells. Looks: About fifty, tall and spare, tonsured, a travel-stained cloak over a priest's tunic, a book satchel, a small cross of bronze; he rides a horse and leads it through villages.
Ties: taught Alcuin (his master at York); friend Bernlef (gave him back his sight and taught him the psalms); taught Landric (baptised him on Fosite's island and made him a priest); rival Notburg (breaks what she tends); serves Hamacar (Utrecht's bishop is his old school's lord).

**Bernlef** (`cast.bernlef`; real; of Helewyret (Holwierde) in Fivelgo; blind three years, healed by Liudger between 787 and 793; alive in the rising of about 793; died 'old and full of days', the year not known (?)). People: frisian. Role: the singer. Home: Emuthon. Loved by his neighbours because he is courteous and knows the deeds of the men of old and the wars of kings by heart; kept the secret of his healing as he was told. Wants: To keep both: the old kings' stories, and the psalms Liudger is teaching him. Fears: The dark coming back. Voice: Easy and courteous; begins every answer with a story about a king. Looks: About sixty, big-framed and gentle, white hair to the shoulder, eyes that look a little past you from habit, a blue-grey cloak, a stick he no longer needs; his wife leads him by the sleeve out of long use.
Ties: friend Liudger (who healed him); friend Notburg (they know the same stories); friend Landric (the island priest asks him for the old names of things).

**Landric** (`cast.landric`; real; son of a chief of Fosetesland (Heligoland); baptised by Liudger about 791 (?); ordained priest and 'for many years set over the teaching of the Frisians'). People: frisian. Role: the priest. Home: Emuthon, also met at Doccinga, Winaldum. Island-born: knows the old rites from the inside and preaches against them without scorn. Wants: A church that stays standing on Fosite's island; his father's people left alone by both kings. Fears: Danish ships at the island; being called a traitor to his father's gods by people he baptised. Voice: Slow and careful; uses sea words for holy things. Looks: Thirty, weathered like a fisherman under the tonsure, a priest's dark tunic, a seal-skin cloak against the rain, a small wooden box with the oil and the host.
Ties: taught Liudger (who lifted him from the font); friend Bernlef (asks him the old names).

**Hamacar** (`cast.hamacar`; real; bishop of Utrecht about 790-806 in the bishops' list; nothing else is known of him (?)). People: frankish. Role: the bishop. Home: Traiectum, also met at Dorestat. Keeper of Willibrord's house and its school: careful, formal, more a steward of lands than a preacher (our reading; the sources are silent). Wants: Wax, wine and oil for Saint Martin's; a ship that will carry letters to York and bring books back. Fears: The East Frisian rising reaching the Vecht; the count taking the church's fines. Voice: Formal; speaks of the blessed Willibrord as of a neighbour who has stepped out. Looks: About fifty, tonsured, a white linen alb under a dark wool cloak, a bishop's ring and a plain staff with a curled head, a deacon with a book beside him.
Ties: rival Folcbert (bishop and count dispute every fine); friend Liudger (Liudger was schooled in his church).

**Eanbald** (`cast.eanbald`; real; Eanbald II: archbishop of York from August 796 (the pallium in 797), last heard of in 808. His predecessor of the same name died on 10 August 796). People: english. Role: the bishop. Home: Eoforwic. Alcuin's pupil made a lord: keeps more armed men about him than his old master thinks an archbishop should, and shelters the king's enemies. Wants: Books back from Francia for the minster's library; the upper hand over King Eardwulf. Fears: The king. Voice: Quick, with schoolroom Latin tags; impatient with merchants unless they carry letters. Looks: About forty, vigorous, tonsured, a white woollen band (the pallium) over a rich dark vestment on feast days, a riding cloak and spurs on others, thegns at his back.
Ties: taught Alcuin (his master, who writes to rebuke his retinue); friend Ecgwulf (Ecgwulf is one of the thegns under his protection).

**Æthelheard** (`cast.aethelheard`; real; archbishop of Canterbury 792/3 to 12 May 805; fled Kent during Eadberht Praen's rising, 796-798; at Rome 801-803). People: english. Role: the bishop. Home: Fordwic, also met at Dofras, Quentovic, Lundenwic. Offa's man in a Kentish see, and the Kentish know it: learned, cautious, in exile when Kent rises. Wants: Canterbury's old rank made whole against Offa's new archbishop at Lichfield; a safe crossing to Rome. Fears: The men of Kent. Voice: Measured; says 'the king' and means the Mercian one. Looks: About fifty, tonsured, the pallium over a dark vestment, a crozier, a train of clerks with a book chest; on the road a plain travelling cloak and a mule.
Ties: friend Alcuin (Alcuin writes to him in his flight and tells him to go back); friend Wighelm (both are Offa's men).

**Godfred** (`cast.godfred`; real; king of the Danes; first named in 804; killed by one of his own men in 810). People: danish. Role: the king. Home: Sliesthorp, also met at Hleiðr, Reric. Proud and exact: counts his ships against the emperor's and builds a wall across a neck of land. Wants: The toll of every ship between the two seas; Frisia and Saxony as his own provinces. Fears: Nothing he would name. He should have feared his own following. Voice: Short and public; never alone; speaks to merchants through a spokesman. Looks: In his forties, tall, long fair hair combed back, a red cloak with a great gilt pin, gold arm rings, a sword with a silvered hilt; always shown among armed men, horses and ships.
Ties: feud Thrasco (has him murdered at Reric in 809); feud Dragomir (carried him off from Reric); friend Asulf (the dealer sells what the king's wars take); kin Horik (his son).

**Thrasco** (`cast.thrasco`; real; prince (dux) of the Obodrites from about 795, after Witzan was killed by Saxons; beat the Saxons beyond the Elbe in 798; given their land in 804; driven out by Godfred in 808; murdered at Reric in 809). People: wendish. Role: the chief. Home: Reric. The emperor's friend among the heathen, and proud of it: a fighter who keeps his word to the Franks and his own gods at home. Wants: Frankish swords for his men, which the law forbids from 805; the Saxons' land beyond the Elbe. Fears: Godfred's ships in the bay, and the Wilzi at his back. Voice: Courteous to Franks, through an interpreter; does not swear lightly. Looks: About forty, broad, dark hair cut round and a long moustache, a fur-lined cloak with a Frankish sword the emperor gave him, a belt set with silver, guards with round shields and spears.
Ties: feud Godfred (who drives him out and has him killed); friend Dragomir (takes his toll and his advice).

**Bresal** (`cast.bresal`; real; Bresal son of Segene, abbot of Iona for thirty-one years; died 801). People: gaelic. Role: the abbot. Home: Hī. Thirty years abbot of Columba's island: has buried kings who came to die as pilgrims, and sees the first sails from the north in his last years. Wants: The island's peace; vellum and wine; word of Lindisfarne, Columba's daughter house. Fears: A sail from the north: Iona is plundered in 795. Voice: Few words, the blessing first; Irish, with Latin for anything that must be exact. Looks: Very old, small, the Roman crown tonsure (his house gave up the old Irish one in 716), an undyed wool habit, a plain staff with a crook, a book satchel hung on a peg behind him.
Ties: friend Higbald (Lindisfarne was founded from Iona; they pray for each other's dead); taught Cellach (the abbot who comes after him, at one remove).

**Cellach** (`cast.cellach`; real; abbot of Iona 802-814; builds the new house at Kells from 807 after sixty-eight of the community are killed in 806; resigns in 814; died 815). People: gaelic. Role: the abbot. Home: Hī, also met at Duiblinn. The abbot who moved the house: practical, counts hands and boats, will not leave the island empty. Wants: Timber, stone and safe hulls to carry Columba's relics and books to Ireland. Fears: To be remembered as the abbot under whom Iona ended. Voice: Brisk; gives a blessing and an order in the same breath. Looks: About fifty, strong, tonsured, sleeves rolled, a habit hitched up for work, a wax tablet with a building's plan scratched on it, monks carrying a carved stone behind him.
Ties: taught Bresal (Bresal's house, which he inherits after Connachtach).

**Anskar** (`cast.anskar`; real; born 8 September 801 near Corbie; to the Danes in 826; at Birka 829-831 and again about 852; archbishop at Hamburg 831, at Bremen from 848; died 3 February 865). People: frankish. Role: the bishop. Home: Sliesthorp, also met at Birka, Dorestat, Ripa. A visionary and a stubborn one: buys boys out of slavery to make them priests, gives gifts before he asks, travels in merchants' ships. Wants: A church and a bell at Sliaswic, a priest at Birka, and the leave of kings who can take it back. Fears: That the work will die with him. Voice: Soft and patient; never argues with a king in public. Looks: As first met, twenty-five, thin, tonsured, a monk's black habit worn at the hem, barefoot in the boat; later a greying archbishop in a hair shirt under a plain cloak, the pallium only at the altar.
Ties: friend Herigar (his first great convert); friend Horik (wins him with gifts and talk); friend Frideburg (one of his flock at Birka).

**Herigar** (`cast.herigar`; real; prefect of Birka and a counsellor of the king; baptised about 830; died about 851, before Anskar's second coming). People: svea. Role: the keeper of the port. Home: Birka. The keeper of the town, baptised, with a church on his own land; argues with the assembly in the rain and stays dry; blunt as a judge. Wants: A priest at Birka again after 845. Fears: The town paying for his faith: when the exiled king Anound came with Danish ships, the merchants bought peace with a hundred pounds of silver. Voice: Blunt; a townsman who talks like a lawman. Looks: About fifty, heavy, a grey beard, a fur-trimmed cloak with a ring pin, a small silver cross worn openly, a staff of office, warehouse keys.
Ties: friend Anskar (who baptised him); friend Frideburg (the two pillars of Birka's Christians).

**Frideburg** (`cast.frideburg`; real; a rich Christian widow of Birka; old when the priest was driven out in 845; died about 852-855, after the hermit Ardgar came (?)). People: svea. Role: the widow. Home: Birka. Rich, old and unbending: with no priest left in Birka she kept bought wine by her for three years, to be given her at the last in place of the sacrament. Wants: Everything she owns given to the poor; and since Birka has few poor, carried to Dorstadt, where there are many. Fears: Dying without the last rites. Voice: Dry and certain; quotes her own promises back to people. Looks: Past seventy, upright in a chair, a fine dark gown with silver oval brooches and beads of glass and carnelian, a small cross among them, a sealed jar of wine on the shelf behind her.
Ties: kin Catla (her daughter); friend Herigar (the other pillar of Birka's Christians).

**Catla** (`cast.catla`; real; Frideburg's daughter; carried her mother's wealth from Birka to Dorestad after her death, about 852-860 (?)). People: svea. Role: the pilgrim. Home: Birka, also met at Dorestat, Sliesthorp. Dutiful and practical: she does exactly what her mother asked, and when she and her helpers are weary she buys four pennies' worth of wine and goes on. Wants: To give it all away in the right places and go home. Fears: The road: the way to Dorstadt is beset by pirates. Voice: Counts aloud; asks devout women, not men, whom she can trust. Looks: About forty, sunburned from the voyage, a plain travelling cloak over a good gown, her mother's beads, a heavy leather bag she carries herself and keeps on her lap.
Ties: kin Frideburg (her mother); friend Anskar (his flock at Birka).

**Rorik** (`cast.rorik`; real; a Dane of the royal kin; held Dorestad with his brother Harald under Louis the Pious; fled Lothar about 844; took Dorestad in 850 and was granted it; driven out by the Cokingi in 867 and back by 870; last heard of in 873; dead by 882). People: danish. Role: the count. Home: Dorestat, also met at Noviomagus, Traiectum, Sliesthorp. A sea-raider made a count: keeps his oath for as long as it keeps him, and keeps other raiders off the river better than the emperor could. Wants: The tolls of Dorestad, and a kingdom in Denmark if one comes loose. Fears: A stronger fleet than his own; the Frisians who drive him out in 867. Voice: Courteous Frankish with a Danish turn; bargains like a merchant and remembers like a king. Looks: As granted the town, about forty: tall, fair, a moustache, a Frankish count's cloak and sword-belt over a northern tunic, gold arm rings, a ship's prow behind him; a cross on his breast by the 860s.
Ties: friend Eystein (his man at the toll); rival Horik (king of the Danes, whose kingdom he tries for in 855-857).

**Horik** (`cast.horik`; real; Horik I, son of Godfred; one of the kings of the Danes from 813, sole king from about 827; killed in a civil war in 854). People: danish. Role: the king. Home: Sliesthorp, also met at Hleiðr, Ripa. Godfred's son, who learned what his father's pride cost: disowns the fleets that burn Dorestad, takes the bishop's gifts, lets a church be built and stays unbaptised. Wants: The emperor's friendship on his own terms; Frisia, if it is offered. Fears: His own nephews. Voice: Guarded; answers a question with a gift. Looks: In age: grey, heavy, a king's red cloak and gold arm rings, a hall full of benches and shields behind him, a bishop's gift of Frankish glass on the table.
Ties: kin Godfred (his father); friend Anskar (gives him the plot for the church at Sliaswic); rival Rorik (an exile of the royal kin with a fleet).

**Ohthere** (`cast.ohthere`; real; a Northman of Halogaland who told King Alfred of his voyages about 890). People: norse. Role: the merchant. Home: Skíringssalr, also met at Sliesthorp, Ögvaldsnes, Lundenwic. Rich by northern reckoning, six hundred tame reindeer and the tribute of the Finnas, and a careful witness: says what he saw himself and marks what he was only told. Wants: A market in the south for walrus ivory and ship's rope of hide; to know how far north the land runs. Fears: Not recorded. (Ours: the Cwenas, who carry their boats over the mountains to raid.) Voice: Understated; measures everything in days' sail and says which hand the land was on. Looks: About fifty, tall, weather-dark, a walrus-ivory comb and knife handle, a cloak of fine marten, a coil of hide rope and a pair of tusks carried behind him as a gift for a king.
Ties: friend Wulfstan (their two accounts were written down together for the same king).

**Wulfstan** (`cast.wulfstan`; real; sailed from Hedeby to Truso in seven days and nights and told King Alfred of it, about 890). People: english. Role: the skipper. Home: Sliesthorp, also met at Truso. A seaman with an eye for other people's customs: the Este's drinking, their kings, the horse-race for a dead man's goods, the cold they can make in summer. Wants: Amber and a fair wind home. Fears: Not recorded. Voice: Lists the lands to port and to starboard like a rope of knots. Looks: Around forty, a sea cloak, a trimmed beard, an English knife with an inlaid blade, a string of rough amber just bought.
Ties: friend Ohthere (their accounts stand side by side).

**Rollo** (`cast.rollo`; real; a leader of the Northmen on the Seine; granted Rouen and the land about it by King Charles the Simple in 911 and baptised; dead by about 930 (?)). People: norse. Role: the count. Home: Rotomagus, also met at Parisius. A raider who took a river and kept it: gives his men land by the rope, hangs thieves, and lets the archbishop back into his city. Wants: The Seine quiet under his own peace, and its tolls. Fears: Other Northmen coming up his river. Voice: Few words, in Norse or through a Frankish clerk; a promise is short and kept. Looks: Old, very big, grey-bearded, a Frankish cloak and sword over northern dress, an arm ring, a new and awkward cross; behind him Rouen's Roman walls and a row of ships drawn up.

**Cnut** (`cast.cnut`; real; born about 995; chosen king by the Danish fleet in 1014; king of all England from 30 November 1016, of Denmark from about 1018, of Norway from 1028; at Rome in 1027; died 12 November 1035). People: danish. Role: the king. Home: Lundenwic, also met at Ripa, Sliesthorp. A conqueror who becomes a careful Christian king: pays off his fleet, keeps forty ships, gives to churches, and bargains with emperors over merchants' tolls. Wants: One peace on both shores of the North Sea, and his merchants free of unjust tolls on the road to Rome. Fears: What happens to it all when he dies. Voice: Formal, through writs and reeves; a merchant hears him as a law read out. Looks: About thirty, tall, fair, a long nose (so later writers say (?)), a crown and a gold-hilted sword on feast days, housecarls with gilded axes behind him.
Ties: rival Thorkell (his greatest earl, outlawed in 1021 and reconciled in 1023).

**Thorkell** (`cast.thorkell`; real; Thorkell the Tall: brings his army to Sandwich in 1009; takes Æthelred's pay with 45 ships from 1012; Cnut's earl in East Anglia from 1017; outlawed 1021, reconciled 1023). People: danish. Role: the chief of the raiders. Home: Lundenwic, also met at Serræ, Gipeswic. A sea-king for hire: takes East Kent's three thousand pounds, tries to buy the archbishop's life from his own drunken men, then sells his ships to the English king. Wants: Pay, and a lord worth serving. Fears: His own army at a feast. Voice: Loud, open-handed, sudden. Looks: Very tall, in his forties, a long moustache, a mail shirt and a gilded helmet, a great axe, forty-five ships at anchor behind him in the Thames.
Ties: serves Cnut (after 1016).

## Tone

Five lines in the game's voice, each with who says it and why it is right; then five things never to write. Plain words a clever person of any time would say; the humour is dry and is the period's own (Design/content/BRIEF.md rule 3).

- *the game, telling a fact (the Chronicle's flat, dated voice)*: "This year, before Saint John's day, heathen men came to the holy island, and the bishop counted what was gone." A year, a feast for the date, what happened, no adjective doing the work. Model: the Anglo-Saxon Chronicle (texts_793.json chronicle_793).
- *a Frisian toll man at Dorestad*: "A tenth to the king. The rest is yours, if the river lets you keep it." A Frisian is dry: he says the hard thing first and plainly, and the joke is that it is true.
- *a Dane, home from the west*: "The crossing was wet. We lost the mast, and Thorir." A Northman says less than he means: understatement, the worst thing last and smallest.
- *an English scribe at Gyrwe, to a merchant*: "The ink freezes, my hand shakes, and the abbot wants the psalter by Easter. Have you wax? Have you news of York?" A monk's complaint from the margin of his page, and homesickness for books and for home; he wants concrete things.
- *a Svea trader at Birka*: "Forty skins. I counted them at Aldeigjuborg and again this morning. Forty. Now weigh your silver where I can see the scale." A Svea counts everything twice; the talk is of numbers, weights and things, never of feelings.

Never write:

1. Mock-old or modern speech: no thee and thou, no 'aye', 'verily' or 'my lord merchant'; no 'okay', 'deal', 'no problem'; no joke with a punchline, and nothing that winks at the player or knows what comes after its own year.
2. The wrong names for people and things: never 'Viking' as a people (Northmen, Danes, heathen men; 'wicing' only for a raider at sea), never 'adventure', 'quest', 'loot' or 'tavern' in a speaker's mouth; no horned helmets, compasses, charts, clocks, chimneys, buttons or forks; ale-house, not inn.
3. Numbers a person of 793 would not say: no month dates ('the ninth of October'), no clock hours, no miles, no percentages. Say the feast, the moon, the tide, the days' sail; a share as a tenth or a third; the odds as sure, likely or doubtful.
4. Sermons and history lessons: nobody explains their own age, their faith or their law at length. Faith is ordinary and brief (a fast day, a relic, a hammer under a shirt beside a cross); no speaker judges slavery, the old gods or the king with the eyes of a later time, and no card only punishes.
5. Anything untrue to its year or its source: no quotation from a modern book, game or translation (only texts_793.json and translations free to use, credited); no real person given invented deeds without saying so in 'source'; no invented person with a real person's name and office; no coin of the north before about 825, no Norse Dublin before 841, no Danelaw before 878, no tribute called Danegeld before 991, no Normandy before 911.

## The calendar

The days every writer uses. Dates are Julian, as the chronicle's. People say a day by its feast, a week by the moon, a season by the work. 'certain' true means the feast or custom is attested for the 790s in the place named; false means it is likely, or known only from a later source (the note says which). The engine knows the feasts in Code/Runtime/Levels/Feasts.cs (Easter by the computus, Good Friday, Ascension, Pentecost, New Year, Epiphany, Candlemas, Saint John's eve and day, Peter and Paul, the Assumption, Michaelmas, All Saints, Martinmas, Christmas eve and day).

Easter falls between 22 March and 25 April. Ash Wednesday is 46 days before it, Ascension 39 days after, Pentecost 49 days after. Easter by year: 793: 7 April, 794: 23 March, 795: 12 April, 796: 3 April, 797: 23 April, 798: 8 April, 799: 31 March, 800: 19 April.

| When | Day | Who keeps it | Sure for 793? | What it does |
|---|---|---|---|---|
| 6 January | Epiphany (Twelfth Day) | all Christians | yes | The end of the Christmas feasting; in 878 the Danes fall on Chippenham just after it. |
| 2 February | Candlemas (the Purification) | all Christians | yes | Every church wants wax; wax is dearest in January. |
| late winter, in the month of Goi (February to March) | The Disting at Uppsalir | the Svear | no | A thing of all the Svear with a market and a sacrifice to the disir, while the ice still bears sledges. LATER: Snorri, 13th century. |
| Ash Wednesday to Easter eve, forty fast days | Lent | all Christians | yes | No meat: fish is wanted everywhere, and a Saxon who eats meat can die for it under the first Saxon law. The sea is still shut; boats are tarred. |
| 20 March | Saint Cuthbert | Lindisfarena and Northumbria | yes | The island's own day; pilgrims at the shrine. |
| 25 March | The Annunciation (Lady Day) | all Christians | yes | Spring sowing; in many places the year is reckoned from here or from Christmas. |
| 22 March to 25 April (see easter.by_year) | Easter | all Christians | yes | The great feast and the end of the fast: wine for every altar, new clothes, and the sea opens. The game's sailing season runs from Easter to Martinmas. The king keeps Easter at a palace (Noviomagus in some years) and the counts go to him. |
| mid April | The first day of summer in the north | Danes, Northmen, Svear | no | The ships are launched; an offering for victory and a good season. LATER: the three yearly sacrifices are Snorri's (Ynglinga saga c. 8). |
| 25 April | The Greater Litany | Frankish and English churches | yes | A procession through the fields with relics; no trading while it passes. |
| about 1 May to the end of September | The raiding season | the sea | no | The game's raiders are out between these days (Plans/ai_fleets.md). Lindisfarne fell on 8 June. |
| May to July, where the king is | The king's assembly and the host | the Franks | yes | The great men gather and the army marches from it (to Saxony most years to 804): grain, horses and carts are bought up along its road, and counts are away from home. |
| Monday to Wednesday before Ascension | The Rogation days | Frankish churches; the English since the council of Clofesho, 747 (?) | yes | Three days of fasting and processions for the crops, then Ascension Thursday. |
| 49 days after Easter | Pentecost (Whitsun) | all Christians | yes | Kept like Easter, a whole week; a day for baptisms. |
| 5 June | Saint Boniface | Doccinga, Traiectum, Mogontia | yes | The day he was killed at Doccinga in 754: a gathering at his church there. |
| 9 June | Saint Columba | Hi and its daughter houses | yes | The island's own feast. |
| 24 June; fires on the eve, 23 June | Saint John the Baptist (midsummer) | all Christians | yes | The game's first feast: three days after the start. Fires on the eve are well known later and likely now (?). Nantes is stormed on this day in 843. |
| late May to mid August | The summer markets of the north | Ripa, Skiringssal, Birka, Sliesthorp | no | The seasonal markets are full: beads, combs, furs, cloth, wine, people. Outside these weeks a northern landing may be half empty. |
| 29 June | Saints Peter and Paul | all Christians | yes | A feast day; Rome's own, dear to the English. |
| 1 August | Lammas (loaf-mass) | the English | no | The first loaf of the new corn is blessed; grain prices break as the harvest comes in. The name is in English writing from the 9th century, so likely in 793 (?). In Ireland the fair of Tailtiu is held by the king of Tara at this season (the Annals of Ulster note the year it was not held, 811). |
| 15 August | The Assumption of Saint Mary | all Christians | yes | Harvest in full swing; honey is taken. |
| September to November | The herring | the English east coast and the North Sea | no | The shoals come inshore; salt is wanted by the boatload, and the barrels are cheap by November. |
| 29 September | Michaelmas: the count's day | all Christians; the house | yes | The day the house's debt falls due (the game's rule). Rents and dues are reckoned after harvest; the autumn gales begin. |
| 1 October | Saint Remigius; Saint Bavo at Ganda | the Franks; Ganda | yes | A Frankish feast of the first rank, and Ganda's own saint's day; the vintage begins on the Rhine. |
| from 9 October, about four weeks | The fair of Saint-Denis | Parisius | yes | English ('Saxon') and Frisian merchants sell wool and cloth and buy wine and honey; the abbey takes the tolls. The riddle chain salt_to_pepper pays here. |
| mid October | The winter nights | Danes, Northmen, Svear | no | The start of winter: a feast and an offering for a good year; raiders are home. LATER: saga accounts. |
| 1 November | All Saints | the English and Irish; the Franks only from about 800 | no | Alcuin urges it on his Frankish friends about 800 and Louis the Pious orders it kept everywhere in 835: before that do not put it in a Frankish mouth. York falls on this day in 866. |
| 7 November | Saint Willibrord | Traiectum | yes | The day he died in 739: Utrecht's founder. |
| 11 November | Martinmas | all Christians; Traiectum above all, whose church is Saint Martin's | yes | The sea closes: the end of the game's sailing year. The slaughter month: salt is dearest, hides and tallow cheap, the new wine comes down the Rhine. |
| from Martinmas or from late November to Christmas eve | The fast before Christmas | all Christians | no | Fish again; how long the fast runs differs from church to church (?). |
| 25 December and the days after | Christmas | all Christians | yes | Four days kept whole, twelve of feasting. The Frankish annals turn the year here. Charles is crowned emperor on this day in 800, William king in 1066: the end of the game. |
| midwinter | Yule | Danes, Northmen, Svear | no | Ale brewed and drunk, pork, oaths sworn; the old English held 'Mothers' Night' at the same turn of the year (Bede). The exact night is not known for 793: Snorri says the heathen Yule was at midwinter night and was moved to Christmas about 950. LATER. |
| not recorded; in the sailing season when the king is there (?) | The thing at Birka | the Svear | yes | The assembly of the town, where the king must put a matter to the people and the lot is cast: 'It is our custom that the control of public business of every kind should rest with the whole people and not with the king' (the king's answer to Anskar, tr. Robinson 1921). |
| three general courts a year, and lesser ones between (?) | The count's court | the Franks and Frisians | no | Where a suit is heard, an oath sworn with oath-helpers and a fine set: the 'law hearing' of the cards. Frisian law is spoken by wise men (Lex Frisionum). |
| once a year, by the Weser | The Saxons' assembly at Marklo | the Old Saxons, before the conquest | no | A memory by 793: the yearly meeting of the districts' men, forbidden by the first Saxon law (c. 34: no general assemblies). The crew's Brunger has heard of it from his father. |
| two weeks at midsummer, from 930 | The Althing of Iceland | Icelanders | no | For the later ages only. |
| every week | Sunday | the Frankish realm | yes | No market on a Sunday except where it is an old custom (from 809); no work. A port card should not open a market on a Sunday without saying why. |
| a Wednesday, Friday and Saturday in each season; among the Irish every Wednesday and Friday | The fasts of the four seasons, and Wednesdays and Fridays | all Christians; the Irish strictly | yes | Small fasts aboard and ashore: a pious crewman keeps them, a heathen one does not (npc_text_encounters.md sample 22). |
| every month | The moon and the tide | every seaman | yes | The tide follows the moon, and high water comes at a different hour in each port; springs at full and new moon. A skipper names a day as 'the third night of the new moon'. |

## Gods and faith

What a person of each people would actually say or do about God or the gods in 793: two sentences each, with the source. 'later' true means the best source was written long after 793 and must not be put in a mouth of 793 as fact. Faith is ordinary and brief in the game: a feast, a fast, an oath, a thing worn under a shirt.

**A Frank.** A Frank swears by Saint Martin and Saint Denis, pays his tithe because the king's law now makes him, and holds that the king's wars on the heathen are God's work. His priest still has to forbid him the old things: charms tied on, auguries from birds and horses, feasts at graves and vows at springs, trees and stones. *Source: Capitulary of Herstal 779 c. 7 (tithes); Admonitio generalis 789 c. 65 (against enchanters, weather-makers and vows at trees, stones and springs) (?); Indiculus superstitionum et paganiarum (a list of about 743 in a manuscript of about 800, Vatican Pal. lat. 577)*

**A Frisian.** A Frisian west of the Lauwers is two generations baptised, keeps Boniface's day at Doccinga, and tells how King Radbod drew his foot back from the font rather than be parted from his fathers; east of the Lauwers the churches burned as lately as 793. He remembers that Fosite's spring was drawn in silence, and that the old law gave a man who robbed a shrine to the sea, at the tide line, to the gods he had wronged. *Source: Altfrid, Vita Liudgeri I.22; Alcuin, Vita Willibrordi c. 10; Lex Frisionum, Additio sapientum XI (about 802); the story of Radbod at the font is in the Vita Vulframni c. 9 (early 9th century) (?)*

**A Saxon.** A Saxon has said the vow, 'I forsake all the devil's works and words, Thunaer and Woden and Saxnot', because since about 785 the law is baptism or death, and it is death too to eat meat in Lent or to burn the dead. He pays the tithe and keeps quiet, and some still go to the springs and groves that the same law names. *Source: The Old Saxon baptismal vow (texts_793.json saxon_vow; Vatican Pal. lat. 577); Capitulatio de partibus Saxoniae c. 4, 7, 8, 21 (about 782-785), softened by the Capitulare Saxonicum of 797*

**An Englishman.** An Englishman's people have been Christian five or six generations: he counts time by saints' days, gives to his minster, and has kings who went to Rome to die. Yet monks still listen at table to stories of the old heathen kings, so that Alcuin asks what Ingeld has to do with Christ, and in 786 the pope's legates had to forbid the English to cast lots, eat horse and mark their bodies as the heathen do. *Source: Alcuin's letter of 797 to a bishop he calls Speratus, once taken to be Higbald (MGH Epistolae IV no. 124: 'Quid Hinieldus cum Christo?'); the legates' report of 786, c. 19 (MGH Epistolae IV no. 3) (?); Bede, De temporum ratione c. 15, for the old English months and Mothers' Night*

**A Dane.** A Dane gives the gods their due at the turns of the year and asks them by lot before a voyage or a war; an oath on the holy ring binds him as nothing else does, and what he wants to leave behind is a good name. If his king allows it he will hear a Christian priest out, and take the sign of the cross as a guest's courtesy, without leaving his own gods. *Source: Anglo-Saxon Chronicle 876 (the Danes swear to Alfred on the holy ring; Giles 1847 has 'bracelet'); Rimbert, Vita Anskarii c. 19, 24 (lots among the Danes; men 'willingly signed with the cross' at Sliaswic who put off baptism); Hávamál 76-77 for the name that never dies (Bellows 1923: texts_793.json havamal_076, havamal_077; written down about 1270, older in substance); Thietmar of Merseburg I.17 on a nine-yearly sacrifice at Lejre is of about 1015 and LATER*

**A Svea.** A Svea asks the gods by lot whether a thing may be done, and holds that a matter for the whole people is settled at the thing and not by the king. His gods must be useful: at Birka in the 850s they offered, through a man who said he had been at their meeting, to take the dead King Eric among them if the people wanted more gods, and an old man answered that Christ had helped men in peril at sea. *Source: Rimbert, Vita Anskarii c. 26-27 (tr. Robinson 1921), of about 852. The temple at Uppsala with Thor, Wodan and Fricco and the feast every nine years is Adam of Bremen IV.26-27, written about 1075: LATER, never as fact for 793*

**A Wend (later sources only).** A Wend of the Obodrites keeps his gods in groves no stranger may enter, takes his people's decisions in assembly, asks the lot, and holds an oath a dangerous thing, not lightly sworn. Nothing written of his worship is as old as 793: the accounts are a Saxon bishop's and a Saxon priest's, two and four centuries on. *Source: Thietmar of Merseburg VI.23-25 (about 1018, on the Liutizi and Riedegost: the sacred wood, lots, the horse oracle, the assembly that must agree); Helmold, Chronica Slavorum I.52, I.84 (about 1170: the grove of Prove; the Slavs 'most unwilling to swear') (?). The Annales regni Francorum say only that the Obodrites were heathen and the Franks' allies. The brief's 'oaths sworn on water' is not in these sources (?)*

**An Irish monk.** An Irish monk fasts on Wednesdays and Fridays, sings the psalms through, and counts leaving his own land for God the best road there is: brothers of his have sailed to islands in the northern ocean where the midsummer night is light enough to pick the lice from a shirt. At home the stricter sort, the clients of God, want fewer wanderers and tighter houses, and can stop a king's fair. *Source: Dicuil, De mensura orbis terrae VII (825), on clerics who stayed in Thule from February to August thirty years before (texts_793.json dicuil); Annals of Ulster 811, the fair of Tailtiu prevented by the community of Tallaght (chronicle_793.json 811-08-tailtiu-fair); the Rule of Tallaght (9th century) for the fasts (?)*

## The shared tables

- `encounters/npc_roles_793.json`: 70 roles (the toll man kept), 15 peoples, the 17 realms and a row for each of the 50 ports with its peoples, faith, mint and a sourced note.
- Roles: abbess, abbot, alewife, beggar, bishop, captive, carter, cellarer, chief, child, clerk, commander, count, craftsman, crewman, envoy, factor, farm_wife, farmer, ferryman, fisherman, fowler, goldsmith, guest_master, harbour_master, herdsman, host, hostage, hunter, interpreter, king, kings_man, lawman, leech, lookout, lords_man, master, mate, merchant, moneyer, monk, nun, owner, pilgrim, pilot, porter, priest, raider, raider_chief, reeve, rune_carver, sailor, salter, schoolmaster, scribe, setter, shipwright, singer, skald, skipper, slave_dealer, smith, steersman, steward, survivor, tollman, warrior, weaver, widow, wise_woman.
- Peoples: frankish (Franks of the Rhine, Meuse and Scheldt, speaking Frankish; culture frankish); romance (the Romance-speaking people of Neustria, the Loire and Aquitaine; culture frankish); frisian (Frisians from the Scheldt to the Weser; culture frisian); saxon (the Old Saxons between Rhine and Elbe, baptised by force since 785; culture frankish); english (Mercians, West Saxons, Kentishmen, East Angles, Northumbrians; culture english); danish (Danes of Jutland, the islands and Skane; culture norse); norse (Northmen of Vestfold and the west coast; culture norse); svea (the Svear of Lake Malaren; culture norse); gute (the people of Gotland; culture norse); wendish (the Slavs of the Baltic coast, Obodrites and Wilzi; culture slavic); slavic (the Slavs of Lake Ilmen and the Volkhov; culture slavic); baltic (Prussians (the Este) and Curonians; culture baltic); finnic_rus (the Finnic peoples round Ladoga; culture finnic_rus); irish (the Irish of Ireland; culture gaelic); gaelic (the Gaels of Dal Riata and the Isles; culture gaelic).
- `encounters/names_placeholder.json`: 233 names, men and women of each people, none a famous person's and none a name of the cast.

## Sources

- Read in full on 4 Oct 2026 (primary): Gesta abbatum Fontanellensium, ed. S. Loewenfeld, MGH SS rer. Germ. 28 (1886), c. 12 on Gervold: https://archive.org/details/gestaabbatumfon00wangoog
- Read in full (primary): Altfrid, Vita sancti Liudgeri, in W. Diekamp, Die Vitae sancti Liudgeri (Munster 1881), book I c. 9-27: https://archive.org/details/dievitaesanctili00altfuoft
- Read in full (primary, in a translation free to quote): Rimbert, Life of Anskar, tr. C. H. Robinson, Anskar, the Apostle of the North (London 1921), c. 7-32: https://archive.org/details/anskartheapostle00robiuoft
- Charlemagne to Offa, 796: William of Malmesbury, Chronicle of the Kings of England, tr. J. A. Giles (London 1847), in the Fordham Medieval Sourcebook: https://sourcebooks.web.fordham.edu/source/796CharlemagentoOffa.asp ; full text MGH Epistolae IV no. 100 and Haddan and Stubbs, Councils III p. 496
- Alcuin's letters: MGH Epistolae IV (ed. E. Dummler 1895), nos. 7 (to Colcu, 790), 16 (to King Æthelred, 793), 20 (to Higbald, 793), 100 (Charles to Offa), 121, 124; the letter to Higbald read in D. Whitelock's English (English Historical Documents I) at https://danmarkshistorien.lex.dk/Alcuins_brev_til_Higbald,_biskop_af_Lindisfarne,_793 (not quoted: not free to use)
- The game's own tables, which carry their own sources: chronicle_793.json (Annales regni Francorum, Annales Bertiniani, Fuldenses, Xantenses, the Anglo-Saxon Chronicle, the Annals of Ulster), texts_793.json, riddles_793.json, goods_793.json, trade_rules.json, fairs_793.json
- Wikipedia (English), read 4 Oct 2026, for dates and cited facts: Gervold; Offa of Mercia; Alcuin; Ludger; Bernlef; Willehad; Gudfred; Thrasco; Saxon Wars; Old Saxon Baptismal Vow; Indiculus superstitionum et paganiarum; Lex Frisionum; List of bishops and archbishops of Utrecht; Higbald of Lindisfarne; Lindisfarne; Eardwulf of Northumbria; Ansgar; Rorik of Dorestad; Quentovic; Volga trade route; Ulfberht swords; Temple at Uppsala; All Saints' Day; Rogation days; Dicuil; Stockholm Codex Aureus. https://en.wikipedia.org/
- Lex Frisionum, Additio sapientum XI (De honore templorum), the Latin as quoted on Wikipedia and in MGH Fontes iuris 12
- The capitularies by number in MGH Capitularia regum Francorum I (ed. Boretius 1883): no. 20 Herstal 779, no. 22 Admonitio generalis 789, no. 26 Capitulatio de partibus Saxoniae, no. 27 Capitulare Saxonicum 797, no. 28 Frankfurt 794, no. 44 Thionville 805, no. 46 Nijmegen 806; clause numbers given from memory are marked (?) where they could not be checked tonight
- Bede, Historia ecclesiastica (II.3 London; III.3 Oswald as interpreter; III.6 Oswald's arm; IV.22 the Frisian in London; V.22 Iona's Easter) and De temporum ratione c. 15, c. 29
- Notker, Gesta Karoli I.34 (the Frisians' short cloaks), tr. A. J. Grant 1905: https://sourcebooks.fordham.edu/basis/stgall-charlemagne.asp
- Archaeology named in passing, not checked beyond a summary tonight (?): the Insular shrine in a woman's grave at Melhus and other church metalwork in Norwegian graves; the digs at Gross Stromkendorf (Reric), Hedeby, Ribe, Kaupang, Birka, Helgo, Staraya Ladoga, Janow Pomorski (Truso), Fishergate in York
- The project's own research: Design/economy_and_trade.md, Design/research_trade_seasons_and_hints.md, Design/npc_text_encounters.md, Research/dorestad_c800_notes.md, Research/traiectum_c800_notes.md
