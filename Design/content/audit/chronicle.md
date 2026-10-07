# Audit of the chronicle (chronicle_793.json) and the happenings (events_793.json)

Audited 6 October 2026. Nothing in the tables was edited; this file is the only thing written.

## What was checked

- `Assets/Kapman/Game/Data/chronicle_793.json`: all 582 entries (47 of them backstory before 21 June 793), each for its event, wording, date, place, sources and port tags.
- `Assets/Kapman/Game/Data/events_793.json`: it exists, but it holds no dated events. It is the 13 generic happenings at sea and in port (fever, fog, thieves on the jetty). They are audited for period accuracy in their own short section below the big table.

## The result in one paragraph

The chronicle is in good shape. Of 582 entries, 550 stand as right, 12 are wrong in some part and 20 are doubtful. None of the 12 is an invented event: they are a wrong season, a wrong month, a day that does not match its feast, sources named wrongly, and port tags on ports that the port table has already closed. The entries up to 814 follow the annals closely and carry their own honest (?) marks; most of the faults sit in the thinner part after 815.

## How to read the table

- **Verdict.** right: event, date, place and wording agree with the source named. **wrong**: something must change; the correction and its source are in the last column. *doubtful*: defensible, but a reader who knows the source would raise an eyebrow; the last column says why.
- **Basis** says how far each verdict can be trusted:
  - **F** checked this run against a fetched page (the Latin Annales regni Francorum on la.wikisource, or a Wikipedia article quoting the source).
  - **C** the weekday or Easter date was computed (Julian calendar) and agrees, or disagrees, as noted.
  - **K** checked against my own knowledge of the source's text or the standard reference, without opening it this run. This is most rows. It is a real check, but it is memory, not a page.
  - **U** not checked: I do not know the source text well enough to vouch for it (mostly single lines from the Irish and Welsh annals, a few Arabic and Byzantine items, coin datings). 78 rows. These are taken on their citation.
- Web checking was thin: several fetches came back 404 (the Annales Fuldenses on la.wikisource, the Nantes raid pages), so those points rest on memory and carry a (?).
- A mechanical pass over the whole file found: no duplicate ids, no unknown port ids, every `fire` day agrees with its `date` and `precision` rule, entries are in firing order, every entry has a source. It also found the eight port tags that fall outside a port's lifetime in `ports_793.json`; they are in the table.

## Every entry

| # | id | date | what the table says (shortened) | verdict | basis | correction or doubt |
|---|---|---|---|---|---|---|
| 1 | 793-great-famine | 793 | Great famine in Burgundy, Francia, Italy, Gothia and Provence; many die | right | K |  |
| 2 | 793-02-22-sicga | 793-02-22 | Ealdorman Sicga, slayer of King Aelfwald, dies | right | K |  |
| 3 | 793-spring-benevento-famine | 793 | Famine in Benevento: the Frankish army under Pippin and Louis must eat meat... | right | C |  |
| 4 | 793-spring-york-blood-rain | 793 | Blood-red rain falls from the roof of St Peter's church at York in Lent | right | K |  |
| 5 | 793-spring-false-grain | 793 | False grain seen in fields, woods and marshes that could be touched but not... | right | K |  |
| 6 | 793-spring-northumbria-portents | 793 | Portents over Northumbria: flashes of light, whirlwinds and fiery dragons i... | right | K |  |
| 7 | 793-northumbria-famine | 793 | Great famine in Northumbria | right | K |  |
| 8 | 793-05-armeniakon-revolt | 793-05 | Constantine VI crushes the revolt of the Armeniakon theme | right | C |  |
| 9 | 793-06-08-lindisfarena-raid | 793-06-08 | Heathen seamen sack the monastery of Lindisfarne, killing and carrying off ... | right | K |  |
| 10 | 793-summer-rustringen-defeat | 793 | News: Count Theoderic's force wiped out by Saxons in Rüstringen by the Weser | right | K |  |
| 11 | 793-offa-heavy-penny | 793 | Offa's heavier, broader penny replaces the lighter Mercian coinage (c. 792/3) | right | K |  |
| 12 | 793-munster-artri | 793 | Artri son of Cathal ordained king of Munster; the law of Ailbe proclaimed i... | right | K |  |
| 13 | 793-connacht-coman-law | 793 | Law of Coman proclaimed in the three Connachta | right | U | Taken on its citation; I could not check the source text. |
| 14 | 793-summer-narbonne-raid | 793 | Cordoban army burns the suburbs of Narbonne and attacks Gerona | **wrong** | K | Event right. Note only: AH 177 ran 18 Apr 793 to 6 Apr 794, not 17 Apr to 9 Apr (the table itself starts AH 178 on 7 Apr 794 in 794-lutos-battle). Source: hijri conversion tables (Wuestenfeld-Mahler). |
| 15 | 793-summer-kamachon-raid | 793 | Arab raiders take the Byzantine frontier forts of Kamachon and Thebasa | right | K |  |
| 16 | 793-summer-orbieu-battle | 793 | Count William of Toulouse beaten by the Cordoban army on the Orbieu | right | K |  |
| 17 | 793-08-01-st-albans | 793-08-01 | Offa raises St Alban's relics and founds St Albans abbey (St Albans tradition) | right | K |  |
| 18 | 793-autumn-alcuin-captives | 793 | Alcuin writes from Francia, offering to ask Charles about the Lindisfarne c... | right | K |  |
| 19 | 793-autumn-saxon-revolt | 793 | Saxons and East Frisians in revolt: churches burned, priests driven out | right | K |  |
| 20 | 793-autumn-fossa-carolina | 793 | Charles digs a ship canal (Fossa Carolina) between Altmühl and Rednitz; rai... | right | K |  |
| 21 | 793-12-25-wurzburg-christmas | 793-12-25 | Charles brings his boats down the Rednitz and Main; Christmas at Würzburg | right | K |  |
| 22 | 794-aethelberht-beheaded | 794 | Offa has King Aethelberht of East Anglia beheaded and takes East Anglia | right | K |  |
| 23 | 794-06-01-frankfurt-prices | 794-06-01 | Synod of Frankfurt fixes grain and bread prices and a new public modius | right | K |  |
| 24 | 794-06-frankfurt-new-denier | 794-06 | Frankfurt: the new deniers must be taken in every place, town and market | right | K |  |
| 25 | 794-06-frankfurt-synod | 794-06 | Frankfurt synod condemns Adoptionism and rejects the Greek council on images | right | K |  |
| 26 | 794-06-tassilo-renounces | 794-06 | Tassilo, former duke of Bavaria, gives up all his rights at Frankfurt | right | K |  |
| 27 | 794-lutos-battle | 794 | Cordoban armies raid Alava and Asturias; Alfonso II ambushes one at Lutos | right | K |  |
| 28 | 794-hebrides-raid | 794 | Heathens lay waste all the islands of Britain | right | K |  |
| 29 | 794-summer-donemuthe-raid | 794 | Heathens plunder the monastery at Donemuthe (Jarrow); a storm wrecks their ... | right | K |  |
| 30 | 794-08-01-aethelheard-york | 794-08-01 | Aethelheard, former ealdorman turned cleric, dies at York | right | K |  |
| 31 | 794-08-10-fastrada | 794-08-10 | Queen Fastrada dies at Frankfurt; buried at St Alban's, Mainz | right | K |  |
| 32 | 794-drought-harvest | 794 | Great drought, yet an abundant harvest | right | K |  |
| 33 | 794-autumn-sintfeld | 794 | Two Frankish armies enter Saxony; the Saxons at the Sintfeld submit and giv... | right | K |  |
| 34 | 794-12-25-aachen-winter | 794-12-25 | Charles winters at Aachen, from now on his usual winter seat | right | K |  |
| 35 | 795-05-06-bran-ardchenn | 795-05-06 | Bran Ardchenn, king of Leinster, and his queen killed; Finsnechta takes Lei... | right | C |  |
| 36 | 795-oviedo-sacked | 795 | Abd al-Karim ibn Mughith sacks Oviedo, the Asturian capital | right | K |  |
| 37 | 795-rechru-burned | 795 | Heathens burn Rechru (Rathlin, or Lambay off Dublin) | right | K |  |
| 38 | 795-skye-raid | 795 | Skye overwhelmed and laid waste by heathens (?) | right | K |  |
| 39 | 795-iona-plundered | 795 | Heathens plunder Iona | right | K |  |
| 40 | 795-connacht-islands-raid | 795 | Heathens plunder Inishmurray and Inishbofin off western Ireland | right | K |  |
| 41 | 795-ac-southern-irish | 795 | Welsh annals note the first coming of the heathens among the southern Irish... | right | K |  |
| 42 | 795-rhieinwg-ravaged | 795 | Offa ravages Rhieinwg in south-west Wales | *doubtful* | K | AC: Vastatio Reinuch ab Offa. Where Rheinwg lay is disputed (Dyfed, or the Brycheiniog side); place and coordinates should carry a (?). |
| 43 | 795-summer-bardowick-campaign | 795 | After an assembly at Kostheim, Charles ravages Saxony to the Elbe at Lüne a... | right | K |  |
| 44 | 795-summer-witzin-killed | 795 | Witzin, king of the Abodrites and Frankish ally, killed by Saxons crossing ... | right | K |  |
| 45 | 795-summer-tudun-envoys | 795 | Envoys of the Avar tudun offer his submission and baptism | right | K |  |
| 46 | 795-12-25-hadrian-dies | 795-12-25 | Pope Hadrian I dies; Leo III elected | right | K |  |
| 47 | 796-avar-khagan-killed | 796 | Avar khagan and jugurrus killed by their own people in civil war | right | K |  |
| 48 | 796-winter-avar-ring-plundered | 796 | Duke Eric of Friuli and the Slav Wonomir plunder the Avar Ring; treasure se... | right | K |  |
| 49 | 796-03-28-lunar-eclipse | 796-03-28 | Eclipse of the moon between cock-crow and dawn | right | K |  |
| 50 | 796-spring-leo-keys | 796 | Leo III sends Charles the keys of St Peter's tomb and the banner of Rome | right | K |  |
| 51 | 796-spring-offa-letter | 796 | Charles writes to Offa: each realm's merchants protected; pilgrim-traders p... | right | K |  |
| 52 | 796-04-16-hisham-dies | 796-04-16 | Emir Hisham I of Cordoba dies; his son al-Hakam I succeeds | right | K |  |
| 53 | 796-04-18-aethelred-killed | 796-04-18 | King Aethelred I of Northumbria killed at Corbridge; Osbald made king | right | K |  |
| 54 | 796-05-14-eardwulf-king | 796-05-14 | Eardwulf recalled from exile and made king of Northumbria; consecrated at Y... | right | K |  |
| 55 | 796-05-osbald-flees | 796-05 | Osbald deposed after 27 days; sails from Lindisfarne to the king of the Picts | right | K |  |
| 56 | 796-spring-charles-angry-northumbria | 796 | Charles, angered by Aethelred's murder, recalls his gifts and threatens the... | right | K |  |
| 57 | 796-globes-sun | 796 | Small globes seen around the sun (later English tradition) | right | U | Taken on its citation; I could not check the source text. |
| 58 | 796-rhuddlan | 796 | Battle of Rhuddlan; Maredudd, king of Dyfed, dies | right | K |  |
| 59 | 796-heathens-ireland | 796 | Heathens in Ireland | right | K |  |
| 60 | 796-kardam-versinikia | 796 | Kardam of Bulgaria demands tribute; Constantine VI waits at Versinikia, no ... | right | K |  |
| 61 | 796-amorion-raid | 796 | Arab raid reaches Amorion in Anatolia | right | U | Taken on its citation; I could not check the source text. |
| 62 | 796-harun-raqqa | 796 | Harun al-Rashid moves his court to al-Raqqa on the Euphrates | right | K |  |
| 63 | 796-summer-avar-ring-destroyed | 796 | Pippin crosses the Danube and destroys the Avar Ring; the new khagan submits | right | K |  |
| 64 | 796-summer-danube-synod | 796 | Bishops meet on the Danube to settle how the Avars are to be baptised | right | K |  |
| 65 | 796-summer-weser-alisni | 796 | Charles ravages Saxony; the Franks bridge the Weser at Alisni | right | K |  |
| 66 | 796-summer-spain-raid-796 | 796 | A Frankish army raids Saracen lands in Spain | right | U | Taken on its citation; I could not check the source text. |
| 67 | 796-07-29-offa-dies | 796-07-29 | Offa, king of the Mercians, dies; his son Ecgfrith succeeds | right | K |  |
| 68 | 796-08-10-eanbald-dies | 796-08-10 | Archbishop Eanbald I of York dies; Eanbald II consecrated at Sockburn on 14... | right | C |  |
| 69 | 796-summer-eadberht-kent | 796 | Eadberht Praen seizes Kent; Archbishop Aethelheard flees Canterbury | right | K |  |
| 70 | 796-eadwald-east-anglia | 796 | Eadwald rules East Anglia as an independent king after Offa's death (c. 796... | right | K |  |
| 71 | 796-tudun-baptised | 796 | The Avar tudun comes to Aachen with many Avars and is baptised | right | K |  |
| 72 | 796-autumn-avar-treasure-shared | 796 | Pippin brings the rest of the Avar treasure to Aachen; Charles shares it out | right | K |  |
| 73 | 796-12-cenwulf-king | 796-12 | Ecgfrith of Mercia dies after 141 days; Cenwulf king of the Mercians | right | K |  |
| 74 | 797-zatun-barcelona | 797 | Zatun, lord of Barcelona, comes to Aachen and hands over his city | right | K |  |
| 75 | 797-abdallah-aachen | 797 | Abdallah, exiled uncle of al-Hakam, seeks Charles's help at Aachen | right | K |  |
| 76 | 797-theoktistos | 797 | Theoktistos, envoy of the governor of Sicily, brings Constantine VI's lette... | right | K |  |
| 77 | 797-harun-embassy-sent | 797 | Isaac the Jew, Lantfrid and Sigimund sent as envoys to Harun al-Rashid | right | K |  |
| 78 | 797-harun-asia-minor | 797 | Harun campaigns in Asia Minor and takes al-Safsaf; a column raids to Ankara | right | U | Taken on its citation; I could not check the source text. |
| 79 | 797-07-mars-unseen | 797-07 | Mars not seen anywhere in the sky for a whole year | right | K |  |
| 80 | 797-day-of-the-ditch | 797 | Day of the Ditch: Toledo's notables massacred by al-Hakam's governor (?) | right | K |  |
| 81 | 797-aed-oirdnide | 797 | King Donnchad of Tara dies; Aed son of Niall wins at Druim Rig and takes th... | right | K |  |
| 82 | 797-summer-huesca-siege | 797 | Louis sent to besiege Huesca | right | K |  |
| 83 | 797-summer-hadeln | 797 | Charles marches through the marshes to Hadeln on the sea; Wigmodia taken | right | K |  |
| 84 | 797-summer-ships-into-saxony | 797 | Royal ships hauled overland and by river into Saxony; camp fortified with them | right | U | Taken on its citation; I could not check the source text. |
| 85 | 797-08-19-irene-sole-ruler | 797-08-19 | Constantine VI blinded in the Porphyra; Irene rules alone | right | C |  |
| 86 | 797-08-sun-darkened | 797-08 | Sun darkened for seventeen days after the blinding; ships lose their way | right | K |  |
| 87 | 797-09-08-eanbald-pallium | 797-09-08 | Archbishop Eanbald II of York receives the pallium | right | K |  |
| 88 | 797-10-28-capitulare-saxonicum | 797-10-28 | Capitulare Saxonicum issued at Aachen with Saxons from all parts | right | K |  |
| 89 | 797-11-herstelle-winter | 797-11 | Charles winters in Saxony at Herstelle on the Weser; army billeted across S... | right | K |  |
| 90 | 797-winter-herstelle-embassies | 797 | At Herstelle: Avar envoys with rich gifts and an envoy of Alfonso II of Ast... | right | K |  |
| 91 | 798-winter-lisbon-sacked | 798 | Alfonso II sacks Lisbon; sends Charles mail-coats, mules and Moorish captives | **wrong** | F | Wrong season. The ARF put it in the LAST sentence of 798: Alfonso, having plundered Lisbon, sent the spoils by Froia and Basiliscus hiemis tempore - so the sack is the campaign season of 798 and the envoys came in winter 798/799, not 797/798. (The tent Froia brought is a different, earlier visit, already in 797-winter-herstelle-embassies.) Source: ARF 798, end of the annal (la.wikisource text read this run). |
| 92 | 798-04-02-whalley | 798-04-02 | Rebels under Wada beaten by King Eardwulf near Whalley; Alric son of Heardb... | right | C |  |
| 93 | 798-04-nordliudi-revolt | 798-04 | Saxons beyond the Elbe kill royal envoys, among them Godescalc back from th... | right | C |  |
| 94 | 798-irene-envoys | 798 | Irene's envoys Michael and Theophilus seek peace at Aachen; Sisinnius released | right | K |  |
| 95 | 798-balearics-plundered | 798 | Moors and Saracens plunder the Balearic Islands | right | K |  |
| 96 | 798-malagina | 798 | Arabs raid to Malagina and seize the imperial horses; Irene buys a truce | right | K |  |
| 97 | 798-london-fire-798 | 798 | London destroyed by an accidental fire with great loss of life | right | K |  |
| 98 | 798-kent-ravaged | 798 | Cenwulf ravages Kent, captures Eadberht Praen and maims him; Cuthred made king | right | K |  |
| 99 | 798-leo-canterbury | 798 | Leo III refuses to move the southern archbishopric to London; condemns Eadb... | right | K |  |
| 100 | 798-finchale | 798 | Synod at Pincanhalh (Finchale) under Eanbald II on Easter and church law | right | K |  |
| 101 | 798-inis-patraic | 798 | Heathens burn Inis Patraic, take the cattle-tribute and break the shrine of... | right | K |  |
| 102 | 798-caradog | 798 | Caradog, king of Gwynedd, killed by the Saxons | right | K |  |
| 103 | 798-summer-minden | 798 | From Minden Charles ravages Saxony between Weser and Elbe; 1,600 hostages d... | right | K |  |
| 104 | 798-summer-bornhoved | 798 | Abodrites under Thrasco crush the Nordalbingians at Bornhöved (Suentana) | right | K |  |
| 105 | 799-winter-ireland-snow | 799 | Great snowfall in Ireland kills many people and cattle | right | U | Taken on its citation; I could not check the source text. |
| 106 | 799-04-01-irene-procession | 799-04-01 | Irene rides in a gold chariot through Constantinople on Easter Monday, scat... | right | C |  |
| 107 | 799-04-25-leo-attacked | 799-04-25 | Pope Leo III attacked in Rome during the Greater Litany procession | right | K |  |
| 108 | 799-daniel-sicily | 799 | Daniel, envoy of Michael, governor of Sicily, received by Charles in Saxony | right | K |  |
| 109 | 799-jerusalem-relics | 799 | A monk from Jerusalem brings relics of the Holy Sepulchre from the patriarch | right | K |  |
| 110 | 799-saxons-deported-799 | 799 | Saxons deported with wives and children; their land shared among Frankish l... | right | K |  |
| 111 | 799-paderborn-church | 799 | A great church built and dedicated at Paderborn | right | K |  |
| 112 | 799-avar-revolt | 799 | The Avars break faith and rise against Frankish rule | right | K |  |
| 113 | 799-eric-killed | 799 | Duke Eric of Friuli killed in an ambush by the townsmen of Tarsatica | right | K |  |
| 114 | 799-balearics-submit | 799 | The Balearics submit to the Franks after help against raiders; Moorish bann... | right | K |  |
| 115 | 799-brittany-799 | 799 | Count Wido of the Breton march tours Brittany and takes its submission | right | K |  |
| 116 | 799-huesca-keys | 799 | Azan, governor of Huesca, sends Charles the keys of his city | right | K |  |
| 117 | 799-aquitaine-islands | 799 | Pagan ships raid islands off Aquitaine; about 105 raiders killed on the shore | right | K |  |
| 118 | 799-derbent | 799 | Khazars break through Derbent into Armenia and carry off many captives | right | U | Taken on its citation; I could not check the source text. |
| 119 | 799-akameros | 799 | Plot of Akameros, lord of the Slavs of Belzetia, for the sons of Constantin... | right | U | Taken on its citation; I could not check the source text. |
| 120 | 799-british-sea-storm | 799 | Many ships wrecked with their crews by a violent storm in the British sea | right | K |  |
| 121 | 799-moll | 799 | Duke Moll killed on King Eardwulf's orders | right | U | Taken on its citation; I could not check the source text. |
| 122 | 799-osbald-dies | 799 | Osbald, once king, later abbot, dies and is buried at York | right | U | Taken on its citation; I could not check the source text. |
| 123 | 799-aldred-killed | 799 | Duke Aldred, slayer of King Aethelred, killed by Torhtmund in revenge | right | U | Taken on its citation; I could not check the source text. |
| 124 | 799-brorda | 799 | Brorda (Hildegils), a leading Mercian ealdorman, dies | right | U | Taken on its citation; I could not check the source text. |
| 125 | 799-sigeric-rome | 799 | Sigeric, king of the East Saxons, leaves his kingdom for Rome | *doubtful* | F | Sigeric of Essex going to Rome is normally dated 798 (ASC F 798; Wikipedia, Sigeric of Essex). Suggest 798; certainty stays uncertain. |
| 126 | 799-aelfhun | 799 | Bishop Aelfhun of Dommoc dies at Sudbury and is buried at Dunwich | right | U | Taken on its citation; I could not check the source text. |
| 127 | 799-connacht-patrick-799 | 799 | Law of Patrick proclaimed in Connacht by Gormgal son of Dindathach | right | U | Taken on its citation; I could not check the source text. |
| 128 | 799-summer-paderborn-leo | 799 | Assembly at Lippeham; Charles camps at Paderborn and receives Pope Leo there | right | K |  |
| 129 | 799-summer-bardengau-799 | 799 | Charles the Younger meets the Slavs and the Nordalbingians in the Bardengau | right | K |  |
| 130 | 799-09-01-gerold-killed | 799-09-01 | Gerold, prefect of Bavaria, falls fighting the Avars | right | K |  |
| 131 | 799-09-29-st-michael-thunder | 799-09-29 | Thunder and lightning on St Michael's Day, called fire from heaven | right | K |  |
| 132 | 800-01-16-lunar-eclipse | 800-01-16 | Eclipse of the moon in the evening | *doubtful* | K | The eclipse fell on the evening of 15 January 800 (?); xvii kal. Feb. is 16 Jan with the night counted to the next day. Suggest fire 800-01-15. |
| 133 | 800-03-channel-coastguard | 800-03 | Charles inspects the Channel coast, sets up a fleet and posts guards agains... | right | K |  |
| 134 | 800-06-04-liutgard | 800-06-04 | Queen Liutgard dies at Tours | right | K |  |
| 135 | 800-07-aghlabids | 800-07 | Harun makes Ibrahim ibn al-Aghlab hereditary emir of Ifriqiya | right | K |  |
| 136 | 800-alhmund | 800 | Alhmund, son of King Alhred, seized and killed by King Eardwulf's guards | right | U | Taken on its citation; I could not check the source text. |
| 137 | 800-hartness-tynemouth | 800 | Heathens plunder the churches of Hartness and Tynemouth (?) | right | U | Taken on its citation; I could not check the source text. |
| 138 | 800-07-06-july-frost | 800-07-06 | Unusual hard frost in July (again on 9 July), with no harm to the crops | right | K |  |
| 139 | 800-08-mainz-800 | 800-08 | Assembly at Mainz; Charles sets out for Italy | right | K |  |
| 140 | 800-autumn-benevento-800 | 800 | Pippin sent to ravage the duchy of Benevento | right | K |  |
| 141 | 800-11-24-charles-rome | 800-11-24 | Charles enters Rome, met by Leo at Mentana the day before | right | K |  |
| 142 | 800-12-23-sepulchre-keys | 800-12-23 | Leo III clears himself by oath; envoys bring the keys of the Holy Sepulchre | right | K |  |
| 143 | 800-12-24-christmas-eve-gale | 800-12-24 | Christmas Eve gale throws down towns and houses; the sea floods the land; m... | right | K |  |
| 144 | 800-12-25-charles-emperor | 800-12-25 | Leo III crowns Charles emperor of the Romans in St Peter's | right | C |  |
| 145 | 801-01-leo-attackers-tried | 801-01 | Leo's attackers condemned for treason; spared and exiled at the pope's plea | right | K |  |
| 146 | 801-winter-pestilence-801 | 801 | Pestilence after a mild winter | right | K |  |
| 147 | 801-03-irene-customs | 801-03 | Irene remits Constantinople's city taxes and the customs of Abydos and Hieron | right | K |  |
| 148 | 801-04-25-charles-leaves-rome | 801-04-25 | Charles leaves Rome after Easter; Pippin's second Benevento campaign | right | K |  |
| 149 | 801-04-30-italy-earthquake | 801-04-30 | Great earthquake shakes Italy; the roof of St Paul's in Rome falls in | right | K |  |
| 150 | 801-rhine-tremors | 801 | Earth tremors around the Rhine, in Gaul and in Germany | right | K |  |
| 151 | 801-summer-pisa-envoys | 801 | Envoys of Harun and of Ibrahim of Kairouan land at Pisa; Charles meets them... | right | K |  |
| 152 | 801-eardwulf-cenwulf | 801 | Eardwulf of Northumbria makes war on Cenwulf of Mercia; peace sworn on the ... | right | K |  |
| 153 | 801-london-fire-801 | 801 | Bishop Heathoberht of London dies; soon after a great part of London burns | right | K |  |
| 154 | 801-aethelheard-rome | 801 | Archbishop Aethelheard and Bishop Cyneberht go to Rome over Canterbury's ri... | right | K |  |
| 155 | 801-bresal | 801 | Bresal son of Segene, abbot of Iona for 31 years, dies | right | U | Taken on its citation; I could not check the source text. |
| 156 | 801-summer-ercanbald-fleet | 801 | Notary Ercanbald sent to Liguria to fit out ships for the elephant | right | K |  |
| 157 | 801-summer-barcelona-falls | 801 | Barcelona falls to the Franks after a long siege; Zatun captured | right | K |  |
| 158 | 801-summer-chieti | 801 | Chieti taken and burned; its forts surrender | right | K |  |
| 159 | 801-summer-ireland-rain | 801 | A rainy summer in Ireland | right | U | Taken on its citation; I could not check the source text. |
| 160 | 801-10-isaac-elephant-lands | 801-10 | Isaac lands at Porto Venere with the elephant; Alpine snow keeps him at Ver... | right | K |  |
| 161 | 802-01-18-canterbury-privilege | 802-01-18 | Leo III confirms Canterbury's authority over all the southern sees | right | K |  |
| 162 | 802-spring-irene-envoy-802 | 802 | Irene's envoy Leo at Aachen; Bishop Jesse and Count Helmgaud sent to Consta... | right | K |  |
| 163 | 802-spring-oath-of-802 | 802 | Capitulare missorum generale: all free men over 12 swear fidelity to the em... | right | K |  |
| 164 | 802-spring-coast-ships-802 | 802 | Missi told to have ships readied on the sea coasts and coast-dwellers ready... | right | K |  |
| 165 | 802-radbert-embassy | 802 | Charles sends a second embassy (Radbert) to Harun al-Rashid | right | K |  |
| 166 | 802-lucera | 802 | Ortona and Lucera surrender; Grimoald later captures Count Winigis at Lucera | right | K |  |
| 167 | 802-grado-patriarch | 802 | Patriarch John of Grado killed on the orders of the Venetian doge's son | right | K |  |
| 168 | 802-guntio | 802 | Counts Cadaloh and Goteram killed by Avars near the castle of Guntio | right | U | Taken on its citation; I could not check the source text. |
| 169 | 802-ecgberht-wessex | 802 | Beorhtric of Wessex dies, said to be poisoned; Ecgberht back from exile as ... | right | K |  |
| 170 | 802-kempsford | 802 | Battle at Kempsford: ealdormen Aethelmund and Weohstan both killed; Wiltshi... | right | K |  |
| 171 | 802-eadburh-flees | 802 | Beorhtric's widow Eadburh flees with treasure to Charles, who gives her a n... | right | K |  |
| 172 | 802-iona-burned | 802 | Iona burned by heathens | right | K |  |
| 173 | 802-summer-transalbians-802 | 802 | A Saxon army ravages the Saxons beyond the Elbe | right | K |  |
| 174 | 802-07-20-elephant-aachen | 802-07-20 | The elephant Abul Abaz reaches Aachen with Harun's gifts | right | K |  |
| 175 | 802-10-aachen-laws-802 | 802-10 | Aachen synod and assembly: canons, the Rule and the peoples' laws read and ... | right | K |  |
| 176 | 802-10-31-nikephoros | 802-10-31 | Nikephoros, minister of finance, deposes Irene and is crowned emperor | right | K |  |
| 177 | 802-12-mecca-pact | 802-12 | Harun's pilgrimage: the succession pact for al-Amin and al-Ma'mun hung in t... | right | U | Taken on its citation; I could not check the source text. |
| 178 | 802-12-20-lunar-eclipse | 802-12-20 | Eclipse of the moon at dawn (date doubtful) | *doubtful* | K | Already marked doubtful by the table: no eclipse fits. Could be dropped from the log. |
| 179 | 803-winter-aachen-earthquake | 803 | Earthquake around Aachen and nearby lands, followed by a mortality | right | K |  |
| 180 | 803-01-29-jafar-executed | 803-01-29 | Ja'far the Barmakid executed; the Barmakid ministers fall | right | K |  |
| 181 | 803-spring-nikephoros-stops-tribute | 803 | Nikephoros stops Irene's tribute; Harun's son al-Qasim raids Byzantine Anat... | right | K |  |
| 182 | 803-05-25-higbald | 803-05-25 | Higbald, bishop of Lindisfarne, dies; Ecgberht consecrated at Bywell on 11 ... | right | C |  |
| 183 | 803-mainz-803 | 803 | Assembly at Mainz; no campaign this year | right | K |  |
| 184 | 803-arms-to-merchants-803 | 803 | Capitulary for the missi: no arm-rings or byrnies to merchants; measures to... | right | K |  |
| 185 | 803-kaupang-jetty | 803 | Skiringssal (Kaupang): earliest tree-ring date, a jetty timber felled in 803 | right | K |  |
| 186 | 803-summer-salz | 803 | Nikephoros's envoys meet Charles at Salz and take home a written peace draft | right | K |  |
| 187 | 803-07-19-bardanes | 803-07-19 | Bardanes Tourkos proclaimed emperor by the Anatolian army; the revolt soon ... | right | K |  |
| 188 | 803-08-09-irene-dies | 803-08-09 | Irene dies in exile on Lesbos | right | K |  |
| 189 | 803-10-12-clofesho | 803-10-12 | Council of Clofesho abolishes the Lichfield archbishopric; Canterbury's pro... | right | K |  |
| 190 | 803-autumn-pannonia-settled | 803 | In Bavaria Charles settles Pannonian affairs; Avar and Slav leaders submit | right | K |  |
| 191 | 804-03-16-clare-storm | 804-03-16 | Storm on St Patrick's Eve kills 1,010 in Corcu Baiscinn; the sea splits the... | right | K |  |
| 192 | 804-05-19-alcuin-dies | 804-05-19 | Alcuin, abbot of St Martin's at Tours, dies | right | C |  |
| 193 | 804-dun-cuair | 804 | Synod of the Ui Neill at Dun Cuair; clergy freed from going on hostings | right | K |  |
| 194 | 804-leinster-ravaged-804 | 804 | Aed son of Niall ravages Leinster twice in a month; King Finsnechta submits | right | U | Taken on its citation; I could not check the source text. |
| 195 | 804-kells-given | 804 | Kells (Cenannas) given to the community of Colum Cille | right | K |  |
| 196 | 804-lyminge-refuge | 804 | Lyminge nuns given a plot inside Canterbury as a refuge | right | K |  |
| 197 | 804-summer-nordalbingia-emptied | 804 | Saxons of Wigmodia and beyond the Elbe deported; Nordalbingia given to the ... | right | K |  |
| 198 | 804-summer-godfred-sliesthorp | 804 | Godfred of the Danes brings his fleet and all his horse to Sliesthorp | right | K |  |
| 199 | 804-summer-hollenstedt | 804 | Charles camps at Hollenstedt; the Abodrite prince Thrasco brings gifts | right | K |  |
| 200 | 804-08-krasos | 804-08 | Nikephoros beaten by the Arabs at Krasos in Phrygia, barely escaping | right | K |  |
| 201 | 804-12-25-leo-quierzy | 804-12-25 | Pope Leo III visits Charles: met at Reims, Christmas at Quierzy, then Aachen | right | K |  |
| 202 | 805-winter-lamos-exchange-805 | 805 | Prisoner exchange on the Lamos in Cilicia: about 3,700 Muslim captives freed | right | U | Taken on its citation; I could not check the source text. |
| 203 | 805-spring-capcan-theodore | 805 | Avar capcan Theodore given land between Sabaria and Carnuntum; khagan restored | right | K |  |
| 204 | 805-05-12-aethelheard-dies | 805-05-12 | Archbishop Aethelheard of Canterbury dies; Wulfred chosen to succeed | right | K |  |
| 205 | 805-abraham-baptised | 805 | Avar khagan baptised as Abraham at the river Fischa | right | K |  |
| 206 | 805-leinster-divided | 805 | Aed son of Niall divides Leinster between two Muiredachs at Dun Cuair | right | U | Taken on its citation; I could not check the source text. |
| 207 | 805-peterhof-hoard | 805 | Peterhof hoard: youngest dirham about 804/5, Abbasid silver at the Gulf of ... | right | U | Taken on its citation; I could not check the source text. |
| 208 | 805-patras-siege | 805 | Peloponnesian Slavs, with Saracen ships, besiege Patras and fail (?) | right | U | Taken on its citation; I could not check the source text. |
| 209 | 805-cordoba-plot | 805 | Plot of Cordoban notables against al-Hakam crushed; 72 executed (?) | right | U | Taken on its citation; I could not check the source text. |
| 210 | 805-krum-avars | 805 | Krum destroys what is left of the eastern Avar khaganate (?) | right | U | Taken on its citation; I could not check the source text. |
| 211 | 805-summer-bohemia-805 | 805 | Charles the Younger ravages Bohemia; Duke Lecho killed | right | K |  |
| 212 | 805-summer-elbe-magdeburg | 805 | A Frankish force sails up the Elbe to Magdeburg and ravages Genewara | right | K |  |
| 213 | 805-famine-805 | 805 | Famine in many parts of the realm | right | K |  |
| 214 | 805-12-thionville-border-posts | 805-12 | Thionville: trade with Slavs and Avars only at named border posts; no arms ... | right | K |  |
| 215 | 805-12-thionville-famine | 805-12 | Thionville: in this famine each must feed his own; no dear grain, no food s... | right | K |  |
| 216 | 805-12-thionville-tolls | 805-12 | Thionville: only old, just tolls on bridges, ships and markets; new tolls b... | right | K |  |
| 217 | 805-12-thionville-coin | 805-12 | Thionville: coin to be struck only at the palace; good deniers in use stay ... | right | K |  |
| 218 | 805-12-thionville-arms | 805-12 | Thionville: a byrnie for every 12 manses; no arms carried at home; feuds to... | right | K |  |
| 219 | 806-01-venice-submits | 806-01 | Doges of Venice and envoys of Zadar submit to Charles at Thionville | right | K |  |
| 220 | 806-02-06-divisio-regnorum | 806-02-06 | Divisio regnorum at Thionville: the realm split among Charles, Pippin and L... | right | K |  |
| 221 | 806-03-nijmegen-806 | 806-03 | Charles sails down Moselle and Rhine to Nijmegen; Lent and Easter there | right | C |  |
| 222 | 806-03-nijmegen-prices | 806-03 | Nijmegen capitulary: famine price caps; buying grain at harvest to sell dea... | right | K |  |
| 223 | 806-03-nijmegen-church-treasure | 806-03 | Nijmegen capitulary: church treasure not to be sold off to Jewish and other... | right | K |  |
| 224 | 806-06-04-cross-in-moon | 806-06-04 | A cross seen in the moon at dawn | right | C |  |
| 225 | 806-06-11-harun-invasion | 806-06-11 | Harun's great army leaves Raqqa to invade Byzantine Cappadocia | right | K |  |
| 226 | 806-06-29-william-monk | 806-06-29 | Count William of Toulouse becomes a monk at Gellone | right | U | Taken on its citation; I could not check the source text. |
| 227 | 806-corsica-806 | 806 | Moors ravage Corsica; Pippin's fleet sent; Count Hadumar of Genoa killed | right | K |  |
| 228 | 806-pantelleria-monks | 806 | Moors seize sixty monks on Pantelleria and sell them as slaves in Spain | right | K |  |
| 229 | 806-navarre-806 | 806 | Navarrese and Pamplonans return to Frankish allegiance | right | K |  |
| 230 | 806-niketas-fleet | 806 | Nikephoros sends a fleet under the patrician Niketas to recover Dalmatia | right | K |  |
| 231 | 806-treviso-envoys | 806 | Charles's envoys back from Harun slip past the Greek fleet into the port of... | right | K |  |
| 232 | 806-cyprus-raid | 806 | Arab fleet under Humayd raids Cyprus and carries off some 16,000 captives | right | K |  |
| 233 | 806-rafi-samarqand | 806 | Rafi ibn al-Layth rebels at Samarqand against Abbasid rule | right | U | Taken on its citation; I could not check the source text. |
| 234 | 806-ireland-pestilence | 806 | Great pestilence breaks out in Ireland | right | U | Taken on its citation; I could not check the source text. |
| 235 | 806-iona-68-killed | 806 | Heathens kill sixty-eight of the community of Iona | right | K |  |
| 236 | 806-patrick-law-806 | 806 | Law of Patrick proclaimed by Aed son of Niall | right | U | Taken on its citation; I could not check the source text. |
| 237 | 806-finsnechta-back | 806 | Finsnechta son of Cellach takes back the kingship of Leinster | right | U | Taken on its citation; I could not check the source text. |
| 238 | 806-eardwulf-expelled | 806 | King Eardwulf driven out of Northumbria; Aelfwald II takes the kingdom | right | K |  |
| 239 | 806-wulfred-pallium | 806 | Archbishop Wulfred of Canterbury receives the pallium | right | K |  |
| 240 | 806-summer-sorbs-806 | 806 | Charles the Younger beats the Sorbs; Miliduoch killed; forts on the Saale a... | right | K |  |
| 241 | 806-summer-bohemia-806 | 806 | An army from Bavaria, Alemannia and Burgundy ravages Bohemia again | right | K |  |
| 242 | 806-summer-herakleia | 806 | Harun takes Herakleia after a month's siege; Nikephoros agrees to pay yearl... | right | K |  |
| 243 | 806-08-30-ring-round-sun | 806-08-30 | A great ring seen around the sun | right | K |  |
| 244 | 806-09-01-lunar-eclipse | 806-09-01 | Eclipse of the moon | right | K |  |
| 245 | 806-autumn-ireland-mast | 806 | Great crop of mast (acorns and nuts) in Ireland | right | U | Taken on its citation; I could not check the source text. |
| 246 | 807-01-31-jupiter-moon | 807-01-31 | Jupiter seen passing across the moon | right | K |  |
| 247 | 807-02-11-solar-eclipse | 807-02-11 | Eclipse of the sun at midday | right | K |  |
| 248 | 807-02-26-lunar-eclipse | 807-02-26 | Eclipse of the moon; great battle-lines (an aurora?) seen in the sky that n... | right | K |  |
| 249 | 807-03-17-sunspot | 807-03-17 | A small black spot, taken for Mercury, seen on the sun for eight days | right | K |  |
| 250 | 807-spring-call-up-807 | 807 | Famine: only men west of the Seine called up; poorer men club together to e... | right | K |  |
| 251 | 807-water-clock | 807 | Harun's envoy and Jerusalem monks bring a tent, silks, balsam and a brass w... | right | K |  |
| 252 | 807-burchard-corsica | 807 | Moors lose 3,000 men in Sardinia; Burchard's fleet beats them off Corsica, ... | right | K |  |
| 253 | 807-niketas-truce | 807 | Niketas, with the Byzantine fleet at Venice, makes a truce with Pippin unti... | right | K |  |
| 254 | 807-adrianople-807 | 807 | Nikephoros's march on Bulgaria turned back at Adrianople by a plot | right | K |  |
| 255 | 807-ireland-blood-moon | 807 | The moon turns the colour of blood | right | K |  |
| 256 | 807-inishmurray-807 | 807 | Heathens burn Inishmurray and attack Ros Comain | right | K |  |
| 257 | 807-kells-monastery | 807 | New monastery of Colum Cille built at Kells | right | K |  |
| 258 | 807-conall-kintyre | 807 | Conall son of Tadc killed by Conall son of Aedan in Kintyre | right | U | Taken on its citation; I could not check the source text. |
| 259 | 807-cuthred-kent | 807 | Cuthred, king of Kent under Mercia, dies; Cenwulf rules Kent directly | right | K |  |
| 260 | 807-arthen | 807 | Arthen, king of Ceredigion, dies | right | U | Taken on its citation; I could not check the source text. |
| 261 | 807-08-ingelheim-807 | 807-08 | Assembly at Ingelheim; no campaign this year | right | K |  |
| 262 | 807-08-22-lunar-eclipse | 807-08-22 | Eclipse of the moon at the third hour of the night | right | K |  |
| 263 | 807-summer-rhodes-raid | 807 | Humayd's Arab fleet sacks Rhodes; a storm wrecks ships off Myra | right | K |  |
| 264 | 807-11-three-fasts | 807-11 | Charles orders three three-day fasts against barren land, famine and pestil... | *doubtful* | K | Letter to Ghaerbald right in substance; the nine fast days listed in the note were not checked (the last, 26 Feb, breaks the pattern of the others and looks like a slip) (?). |
| 265 | 808-winter-mild-winter-808 | 808 | A very mild and pestilent winter | right | K |  |
| 266 | 808-03-eardwulf-nijmegen | 808-03 | Exiled King Eardwulf of Northumbria comes to Charles at Nijmegen, then goes... | right | K |  |
| 267 | 808-cloak-prices | 808 | Capitulary caps cloak and fur prices; only palace deniers to pass | right | K |  |
| 268 | 808-march-land-sea | 808 | Agenda of 808: guard the march by land and sea; the ships ordered to be built | right | K |  |
| 269 | 808-lamos-exchange-808 | 808 | Second prisoner exchange on the Lamos; new Arab-Byzantine truce | right | U | Taken on its citation; I could not check the source text. |
| 270 | 808-harun-khurasan | 808 | Harun sets out for Khurasan to put down Rafi's revolt | right | U | Taken on its citation; I could not check the source text. |
| 271 | 808-finsnechta-dies | 808 | Finsnechta son of Cellach, king of Leinster, dies at Kildare | right | U | Taken on its citation; I could not check the source text. |
| 272 | 808-liffey-808 | 808 | The men of Leinster defeat Aed son of Niall at the Liffey | right | U | Taken on its citation; I could not check the source text. |
| 273 | 808-rhain-cadell | 808 | Rhain, king of Dyfed, and Cadell, king of Powys, die | right | U | Taken on its citation; I could not check the source text. |
| 274 | 808-summer-godfred-abodrites | 808 | Godfred attacks the Abodrites; two thirds made tributary; his nephew Regino... | right | K |  |
| 275 | 808-summer-linones-808 | 808 | Charles the Younger bridges the Elbe and ravages the Linones and Smeldingi | right | K |  |
| 276 | 808-summer-reric-destroyed | 808 | Godfred destroys the emporium of Reric and moves its merchants to Sliesthorp | right | K |  |
| 277 | 808-summer-danevirke | 808 | Godfred orders a rampart along the Eider from the Baltic to the North Sea, ... | right | K |  |
| 278 | 808-autumn-elbe-forts | 808 | Two Frankish forts built on the Elbe against Slav raids | right | K |  |
| 279 | 808-autumn-eardwulf-restored | 808 | Eardwulf brought back to Northumbria by papal and imperial envoys | right | K |  |
| 280 | 809-winter-strymon-pay | 809 | Bulgars fall on the army at the Strymon and seize its pay of 1,100 pounds o... | right | K |  |
| 281 | 809-winter-comacchio | 809 | Byzantine fleet winters at Venice; its attack on Comacchio is beaten off | right | K |  |
| 282 | 809-03-24-harun-dies | 809-03-24 | Harun al-Rashid dies at Tus; al-Amin caliph in Baghdad, al-Ma'mun holds Khu... | right | K |  |
| 283 | 809-03-26-liudger | 809-03-26 | Liudger, bishop of Münster and missionary to Frisia, dies at Billerbeck | right | K |  |
| 284 | 809-spring-serdica | 809 | Krum takes Serdica and kills its garrison of 6,000 | right | C |  |
| 285 | 809-04-07-corsica-holy-saturday | 809-04-07 | Moors from Spain sack a Corsican town on Holy Saturday | right | C |  |
| 286 | 809-populonia | 809 | Greek raiders (Orobiotae) plunder Populonia on the Tuscan coast | right | K |  |
| 287 | 809-tortosa-809 | 809 | Louis besieges Tortosa on the Ebro and withdraws | right | K |  |
| 288 | 809-aureolus | 809 | Count Aureolus of the Pyrenean march dies; Amoroz of Zaragoza seizes his forts | right | K |  |
| 289 | 809-badenfliot | 809 | Godfred sends word by merchants; talks at Badenfliot end without result | right | K |  |
| 290 | 809-thrasco-wilzi | 809 | Thrasco ravages the Wilzi and storms the chief fort of the Smeldingi | right | K |  |
| 291 | 809-aldulf-pirates | 809 | Papal envoy Aldulf seized by pirates crossing to the continent; ransomed in... | right | K |  |
| 292 | 809-sunday-markets | 809 | Capitularies of 809: Sunday markets only where old; no toll where no bridge... | right | K |  |
| 293 | 809-cattle-plague-809 | 809 | Great cattle plague comes from the east and spreads west | right | K |  |
| 294 | 809-tullylish | 809 | Aed son of Niall invades Ulaid after Patrick's shrine at Tullylish is violated | right | U | Taken on its citation; I could not check the source text. |
| 295 | 809-cornwall-809 | 809 | Ecgberht of Wessex attacks Cornwall (?) | right | U | Taken on its citation; I could not check the source text. |
| 296 | 809-07-16-solar-eclipse | 809-07-16 | Eclipse of the sun in late morning | right | K |  |
| 297 | 809-09-sklaviniai | 809-09 | Nikephoros orders settlers from the themes moved into the Slav lands of Greece | right | K |  |
| 298 | 809-11-aachen-council-809 | 809-11 | Council at Aachen on the filioque and church discipline | right | K |  |
| 299 | 809-winter-thrasco-murdered | 809 | Abodrite prince Thrasco murdered at the emporium of Reric by Godfred's men | *doubtful* | K | Entry right (ARF 809: in emporio Reric). It shows the PORT table is off: ports_793 closes reric in 808, yet the ARF still call Reric an emporium in 809. Set reric to 809 in ports_793, or drop the port tag here. |
| 300 | 809-12-26-lunar-eclipse | 809-12-26 | Eclipse of the moon | right | F |  |
| 301 | 809-12-28-saint-amand-flood | 809-12-28 | Flood greater than any seen before; the waters highest on 28 December | right | U | Taken on its citation; I could not check the source text. |
| 302 | 810-03-esesfeld | 810-03 | Count Egbert occupies Esesfeld on the Stör and begins to fortify it | right | K |  |
| 303 | 810-spring-venice-810 | 810 | Pippin attacks Venice by land and sea and takes its submission; his fleet r... | right | K |  |
| 304 | 810-spring-sardinia-corsica-810 | 810 | A great Moorish fleet from Spain strikes Sardinia, then takes almost all Co... | right | K |  |
| 305 | 810-06-07-rotrud | 810-06-07 | Rotrud, the emperor's eldest daughter, dies | right | F |  |
| 306 | 810-06-07-solar-eclipse | 810-06-07 | Eclipse of the sun | right | F |  |
| 307 | 810-summer-frisia-raid-810 | 810 | Danish fleet of 200 ships ravages the Frisian islands; Frisians pay 100 pou... | right | K |  |
| 308 | 810-06-21-lunar-eclipse | 810-06-21 | Eclipse of the moon | right | F |  |
| 309 | 810-summer-elephant-dies | 810 | Charles musters at Lippeham; the elephant Abul Abaz dies there | right | K |  |
| 310 | 810-nikephoros-vexations | 810 | Nikephoros's 'vexations': forced loans to shipowners; tax on slaves bought ... | right | K |  |
| 311 | 810-summer-verden-810 | 810 | Charles camps at the Aller-Weser confluence (Verden) awaiting Godfred | right | K |  |
| 312 | 810-britain-cattle-plague | 810 | Cattle plague throughout Britain | right | K |  |
| 313 | 810-mynyw-burned | 810 | Mynyw (St Davids) burned | right | K |  |
| 314 | 810-moon-covered | 810 | The moon darkened (Welsh annals) | right | K |  |
| 315 | 810-echaid-ulaid | 810 | Echaid son of Fiachna, king of Ulaid, dies | right | U | Taken on its citation; I could not check the source text. |
| 316 | 810-munster-cattle-law | 810 | Law on cattle proclaimed in Munster by Daire and Aduar son of Echen | right | U | Taken on its citation; I could not check the source text. |
| 317 | 810-eanred | 810 | Eanred son of Eardwulf becomes king of Northumbria (date uncertain) | right | U | Taken on its citation; I could not check the source text. |
| 318 | 810-musa-heir | 810 | Caliph al-Amin names his son Musa heir in place of his brother al-Ma'mun | right | U | Taken on its citation; I could not check the source text. |
| 319 | 810-zaragoza-810 | 810 | Amoroz driven out of Zaragoza by al-Hakam's son Abd al-Rahman, forced into ... | right | K |  |
| 320 | 810-summer-fireball | 810 | A fireball crosses the sky; Charles thrown from his horse on the march agai... | right | K |  |
| 321 | 810-summer-godfred-murdered | 810 | Godfred of the Danes murdered by one of his men; nephew Hemming succeeds, m... | right | K |  |
| 322 | 810-summer-hohbuoki-taken | 810 | Wilzi take the Elbe fort of Hohbuoki and its garrison | right | K |  |
| 323 | 810-07-08-pippin-dies | 810-07-08 | Pippin, king of Italy, dies | right | K |  |
| 324 | 810-cattle-plague-810 | 810 | Cattle plague kills nearly all the army's oxen and rages through every prov... | right | K |  |
| 325 | 810-10-aachen-peace-810 | 810-10 | At Aachen Charles makes peace with Nikephoros, returning Venice, and with a... | right | K |  |
| 326 | 810-corsica-again-810 | 810 | Corsica ravaged by the Moors a second time | right | K |  |
| 327 | 810-fleet-ordered-810 | 810 | Charles orders a fleet built against the Northmen; ship timber gathered | right | K |  |
| 328 | 810-11-30-solar-eclipse | 810-11-30 | Eclipse of the sun | right | F |  |
| 329 | 810-12-09-mainz-fast | 810-12-09 | Three-day fast ordered in Mainz province against plagues on men and beasts | right | U | Taken on its citation; I could not check the source text. |
| 330 | 810-12-15-lunar-eclipse | 810-12-15 | Eclipse of the moon | right | F |  |
| 331 | 810-winter-hard-winter-810 | 810 | Very hard winter; frost closes the roads between Franks and Danes | right | K |  |
| 332 | 811-spring-eider-peace | 811 | Peace with Hemming sworn on the Eider by twelve Frankish and twelve Danish ... | right | K |  |
| 333 | 811-05-rayy | 811-05 | Tahir ibn al-Husayn routs al-Amin's army at Rayy; its general Ali ibn Isa k... | right | K |  |
| 334 | 811-haido-hugh | 811 | Haido of Basel, Hugh of Tours and Aio sent to Constantinople; Doge Willeri ... | right | K |  |
| 335 | 811-agnello-particiaco | 811 | Agnello Particiaco becomes doge; the ducal seat settles at Rialto | right | K |  |
| 336 | 811-treasure-divided | 811 | Charles divides his treasure; alms for 21 metropolitan sees, Cologne and Ma... | right | K |  |
| 337 | 811-eclipses-811 | 811 | Two eclipses of the sun noted | right | U | Taken on its citation; I could not check the source text. |
| 338 | 811-connacht-patrick-811 | 811 | Nuadu, abbot of Armagh, takes the law of Patrick and his shrine to Connacht | right | U | Taken on its citation; I could not check the source text. |
| 339 | 811-ulaid-heathens | 811 | The Ulaid slaughter a band of heathens | right | U | Taken on its citation; I could not check the source text. |
| 340 | 811-ireland-prodigies | 811 | A year of prodigies in Ireland: bread bleeds when cut, birds speak | right | U | Taken on its citation; I could not check the source text. |
| 341 | 811-owain-dyfed | 811 | Owain son of Maredudd of Dyfed dies | right | U | Taken on its citation; I could not check the source text. |
| 342 | 811-wales-ravaged-811 | 811 | Ecgberht said to ravage Wales from north to south (?) | right | U | Taken on its citation; I could not check the source text. |
| 343 | 811-summer-linones-811 | 811 | An army ravages the Linones beyond the Elbe and rebuilds Hohbuoki | right | K |  |
| 344 | 811-summer-pannonia-811 | 811 | A Frankish army sent into Pannonia to settle quarrels between Avars and Slavs | right | K |  |
| 345 | 811-summer-bretons-811 | 811 | An army sent against the Bretons to punish their breach of faith | right | K |  |
| 346 | 811-07-20-pliska-burned | 811-07-20 | Nikephoros takes and burns Krum's residence at Pliska | *doubtful* | K | Theophanes: Nikephoros entered Bulgaria on 20 July; Pliska was taken in the days after and burned before the retreat. 20 July is the day of entry, not of the burning. Suggest precision month, or the title enters Bulgaria and takes Pliska. |
| 347 | 811-07-26-nikephoros-killed | 811-07-26 | Nikephoros I killed with his army in a Balkan pass; Krum makes his skull a cup | right | C |  |
| 348 | 811-08-tailtiu-fair | 811-08 | Fair of Tailtiu not held: no horse or chariot comes, a boycott by the Talla... | right | C |  |
| 349 | 811-10-boulogne-lighthouse | 811-10 | At Boulogne Charles inspects the new fleet and relights the old Roman light... | right | K |  |
| 350 | 811-10-boulogne-capitulary | 811-10 | Boulogne capitulary: lords to sail with the fleet; churches not to sell byr... | right | K |  |
| 351 | 811-10-02-michael-i | 811-10-02 | Staurakios abdicates; Michael I Rangabe proclaimed emperor | right | K |  |
| 352 | 811-11-ghent-ships | 811-11 | At Ghent on the Scheldt Charles views the ships built for the fleet | right | K |  |
| 353 | 811-autumn-hemming-envoys-811 | 811 | Hemming's envoys and Avar and Danube Slav leaders await Charles at Aachen | right | K |  |
| 354 | 811-12-04-charles-younger | 811-12-04 | Charles the Younger, the emperor's eldest son, dies | right | K |  |
| 355 | 812-winter-danish-succession-war | 812 | Hemming dies; Sigfrid and Anulo fight for the Danish throne; both fall, wit... | right | K |  |
| 356 | 812-04-02-pro-hispanis | 812-04-02 | Praeceptum pro Hispanis protects Spanish settlers in Septimania | right | K |  |
| 357 | 812-05-15-solar-eclipse | 812-05-15 | Eclipse of the sun in the afternoon | *doubtful* | F | The ARF text is Id. Mai. (15 May), confirmed this run; the eclipse itself was on 14 May 812. The note says so. Better to fire it on 812-05-14 (the sky is the fact) and keep the ARF day in the note. |
| 358 | 812-06-develtos | 812-06 | Krum takes Develtos and deports its people; Thracian towns abandoned in panic | right | K |  |
| 359 | 812-basileus | 812 | Envoys of Michael I at Aachen hail Charles as basileus and take the treaty | right | K |  |
| 360 | 812-bernard-italy | 812 | Bernard sent to Italy with Wala against a rumoured African and Spanish fleet | right | K |  |
| 361 | 812-corsica-sardinia-812 | 812 | Moorish fleet reaches Corsica and Sardinia; the part at Sardinia almost who... | right | K |  |
| 362 | 812-ireland-northmen-812 | 812 | A Northman fleet attacks Ireland and is beaten off with heavy losses | right | K |  |
| 363 | 812-alhakam-peace | 812 | Peace with al-Hakam of Cordoba (kept three years) | right | K |  |
| 364 | 812-grimoald-peace | 812 | Peace with Grimoald of Benevento; 25,000 gold solidi paid as tribute | right | K |  |
| 365 | 812-wilzi-812 | 812 | Campaign against the Wilzi; hostages taken | right | K |  |
| 366 | 812-07-ibrahim-aghlab | 812-07 | Ibrahim I ibn al-Aghlab dies; his son Abdallah I succeeds in Ifriqiya | right | U | Taken on its citation; I could not check the source text. |
| 367 | 812-umall-812 | 812 | The men of Umall defeat a band of heathens; heathens slaughter the Conmaicne | right | U | Taken on its citation; I could not check the source text. |
| 368 | 812-loch-lein | 812 | Cobthach son of Mael Duin, king of Loch Lein, slaughters heathens in Munster | right | U | Taken on its citation; I could not check the source text. |
| 369 | 812-connacht-daire-812 | 812 | Law of Daire proclaimed in Connacht | right | U | Taken on its citation; I could not check the source text. |
| 370 | 812-degannwy | 812 | The fortress of Degannwy struck by lightning and burned | right | U | Taken on its citation; I could not check the source text. |
| 371 | 812-disert-diarmata | 812 | Disert Diarmata (Castledermot) founded | right | U | Taken on its citation; I could not check the source text. |
| 372 | 812-harald-envoys | 812 | Danish kings Harald and Reginfrid ask peace and their brother Hemming's return | right | K |  |
| 373 | 812-08-baghdad-siege | 812-08 | Al-Ma'mun's generals begin the siege of Baghdad | right | K |  |
| 374 | 812-autumn-krum-offer | 812 | Krum offers peace on the old terms, with sealed passes for merchants; Micha... | right | K |  |
| 375 | 812-11-mesembria | 812-11 | Krum takes Mesembria with its gold, silver and Greek-fire siphons | right | K |  |
| 376 | 812-11-04-comet-812 | 812-11-04 | A comet seen at Constantinople, like two moons joined and then parted | right | K |  |
| 377 | 813-spring-amalarius | 813 | Amalarius of Trier and Peter of Nonantola sent to Constantinople to confirm... | right | K |  |
| 378 | 813-05-04-solar-eclipse | 813-05-04 | Eclipse of the sun | right | K |  |
| 379 | 813-aachen-portico | 813 | Aachen portico collapses on Ascension Day; tremors and lightning at the pal... | right | C |  |
| 380 | 813-06-22-versinikia-813 | 813-06-22 | Krum defeats Michael I at Versinikia near Adrianople | right | K |  |
| 381 | 813-frisia-raid-813 | 813 | Northmen raid Frisia, carrying off men, women and much booty | right | K |  |
| 382 | 813-majorca-813 | 813 | Count Irmingar of Ampurias takes 8 Moorish ships off Majorca, freeing 500 C... | right | K |  |
| 383 | 813-civitavecchia-nice | 813 | In revenge the Moors sack Civitavecchia and Nice; beaten off in Sardinia | right | K |  |
| 384 | 813-mainz-bridge | 813 | The wooden Rhine bridge at Mainz burns down in three hours | right | K |  |
| 385 | 813-leo-mesembria | 813 | Leo V defeats a Bulgar army near Mesembria (?) | right | U | Taken on its citation; I could not check the source text. |
| 386 | 813-umall-813 | 813 | Heathens slaughter the men of Umall; Dunadach, king of Umall, falls | right | U | Taken on its citation; I could not check the source text. |
| 387 | 813-ui-neill-daire-813 | 813 | Law of Daire proclaimed among the Ui Neill | right | U | Taken on its citation; I could not check the source text. |
| 388 | 813-hywel-cynan-813 | 813 | Battle between Hywel and Cynan for Gwynedd; Hywel wins | right | U | Taken on its citation; I could not check the source text. |
| 389 | 813-07-11-leo-v | 813-07-11 | Michael I abdicates and becomes a monk; Leo V the Armenian crowned next day | right | K |  |
| 390 | 813-summer-reform-councils | 813 | Reform councils at Mainz, Reims, Tours, Chalon and Arles | right | K |  |
| 391 | 813-summer-danish-peace-813 | 813 | Frankish and Saxon lords and 16 Danish nobles swear peace beyond the Elbe; ... | right | K |  |
| 392 | 813-summer-vestfold-813 | 813 | Danish kings Harald and Reginfrid campaign in Vestfold to force its submission | right | K |  |
| 393 | 813-07-17-krum-at-walls | 813-07-17 | Krum before the walls of Constantinople; wounded in an ambush at a parley; ... | right | K |  |
| 394 | 813-09-11-louis-co-emperor | 813-09-11 | Aachen assembly: Louis crowned co-emperor; Bernard made king of Italy | right | C |  |
| 395 | 813-09-25-al-amin-killed | 813-09-25 | Caliph al-Amin killed as Baghdad falls; al-Ma'mun sole caliph | right | K |  |
| 396 | 813-autumn-godfred-sons-return | 813 | Godfred's sons return from exile among the Swedes and drive out Harald and ... | right | K |  |
| 397 | 813-autumn-adrianople-813 | 813 | Krum takes Adrianople and deports its people beyond the Danube | right | K |  |
| 398 | 813-winter-arkadiopolis | 813 | Bulgar army ravages Thrace and takes Arkadiopolis with many captives | right | K |  |
| 399 | 813-winter-hard-winter-813 | 813 | Very hard winter | right | K |  |
| 400 | 814-01-28-charlemagne-dies | 814-01-28 | Charlemagne dies at Aachen at the third hour and is buried the same day | right | K |  |
| 401 | 814-02-louis-takes-over | 814-02 | Louis, told at Doué in Anjou, reaches Aachen 30 days later and takes over | right | K |  |
| 402 | 814-04-13-krum-dies | 814-04-13 | Krum dies while preparing an assault on Constantinople | right | K |  |
| 403 | 814-leo-v-envoys | 814 | Louis receives Leo V's envoys with the confirmed peace and sends his own | right | K |  |
| 404 | 814-missi-814 | 814 | Assembly at Aachen sends missi through the realm to right wrongs | right | K |  |
| 405 | 814-grimoald-814 | 814 | Pact with Grimoald: Benevento to pay 7,000 solidi every year | right | K |  |
| 406 | 814-pippin-aquitaine | 814 | Louis sends Lothar to Bavaria and Pippin to Aquitaine as king | right | K |  |
| 407 | 814-danish-war-814 | 814 | Harald and Reginfrid attack Godfred's sons; Reginfrid and Godfred's eldest ... | right | K |  |
| 408 | 814-harald-commends | 814 | Harald commends himself to Louis and is told to wait in Saxony | right | K |  |
| 409 | 814-wulfred-rome | 814 | Archbishop Wulfred and Bishop Wigberht of Wessex go to Rome | right | K |  |
| 410 | 814-ireland-distress-814 | 814 | Great distress and severe sickness in Ireland | right | U | Taken on its citation; I could not check the source text. |
| 411 | 814-cellach-resigns | 814 | Cellach, abbot of Iona, resigns when the church at Kells is finished; Diarm... | right | U | Taken on its citation; I could not check the source text. |
| 412 | 814-ciaran-law | 814 | Law of Ciaran proclaimed at Cruachain by Muirgius, king of Connacht | right | U | Taken on its citation; I could not check the source text. |
| 413 | 814-leinster-814 | 814 | Leinster civil war: the Ui Cheinnselaig beaten by the sons of Bran | right | U | Taken on its citation; I could not check the source text. |
| 414 | 814-wales-thunder | 814 | Great thunder in Wales causes many fires | right | U | Taken on its citation; I could not check the source text. |
| 415 | 814-gruffydd-powys | 814 | Gruffydd son of Cyngen of Powys killed by his brother Elisedd; Tryffin son ... | right | U | Taken on its citation; I could not check the source text. |
| 416 | 814-anglesey-814 | 814 | Hywel takes Anglesey and drives out Cynan, with heavy losses | right | U | Taken on its citation; I could not check the source text. |
| 417 | 814-winter-elbe-thaw | 814 | Two attempts to cross the frozen Elbe to aid Harald foiled by sudden thaws | right | K |  |
| 418 | 815-05-eider-campaign | 815-05 | A Saxon and Obodrite army crosses the Eider into Danish land to restore Har... | right | K |  |
| 419 | 817-07-ordinatio-imperii | 817-07 | Ordinatio imperii at Aachen: Lothar made co-emperor and heir, with sub-king... | right | K |  |
| 420 | 820-north-pirates | 820 | Thirteen pirate ships from the north beaten off in Flanders and at the Sein... | right | K |  |
| 421 | 821-winter-rivers-frozen | 821 | A long hard winter: the Rhine, Danube, Elbe and Seine freeze so hard that c... | right | K |  |
| 422 | 823-famine-epidemic | 823 | Famine and an epidemic in many parts of Francia; a hailstorm destroys crops | *doubtful* | K | ARF 823 as I recall it has crops destroyed by hail in many places, houses and men struck by lightning, and a great sickness and mortality through all Francia - but no word fames. Famine is said in ARF 820 (crops failed after rain) and 824. Reword to hail and a great sickness, or move the famine to 824. Not checked against the text this run (the online text I could reach stops at 813). |
| 423 | 826-06-harald-baptised | 826-06 | The Danish king Harald Klak is baptised at Mainz with his wife and many fol... | right | K |  |
| 424 | 827-06-sicily-landing | 827-06 | An Aghlabid army from Ifriqiya lands at Mazara: the Muslim conquest of Sici... | right | K |  |
| 425 | 827-harald-expelled | 827 | Harald Klak driven out of the Danish kingdom by the sons of Godfred | right | K |  |
| 426 | 829-ansgar-birka | 829 | Ansgar sails for Birka with merchants; robbed by pirates on the way, he rea... | *doubtful* | K | Rimbert right. The year is reckoned: 829 or 830 are both in use. No change needed beyond a (?) on the year. |
| 427 | 830-04-sons-revolt | 830-04 | Louis the Pious's elder sons rise against him and Empress Judith; Louis is ... | right | K |  |
| 428 | 831-hamburg-see | 831 | Hamburg made an archbishopric for the mission to the north, with Ansgar as ... | *doubtful* | K | As Rimbert tells it. Recent work (Knibbs, Ansgar, Rimbert and the Forged Foundations of Hamburg-Bremen, 2011) holds the 831 archbishopric to be a later claim resting on forged privileges. Keep, but certainty uncertain rather than likely. |
| 429 | 833-06-field-of-lies | 833-06 | The 'Field of Lies' near Colmar: Louis the Pious's army deserts him to his ... | right | K |  |
| 430 | 834-03-01-louis-restored | 834-03-01 | Louis the Pious restored to his arms and throne at Saint-Denis | right | K |  |
| 431 | 834-dorestad-sacked | 834 | A Danish fleet ravages Frisia, comes by Utrecht to Dorestad and destroys ev... | right | K |  |
| 432 | 835-dorestad-again | 835 | The Northmen ravage Dorestad again and lay waste Frisia | right | K |  |
| 433 | 835-noirmoutier-raid | 835 | Northmen plunder the island monastery of Noirmoutier (Herio), the salt isla... | right | K |  |
| 434 | 836-witla-antwerp-burned | 836 | The Northmen burn Antwerp and the emporium of Witla at the Meuse mouth, and... | **wrong** | F | Event right, source wrong. Antwerp and Witla burned stand in the Annales Fuldenses 836 (Nordmanni Andwerpam civitatem incendunt, similiter et Witlam emporium iuxta ostium Mosae fluminis, et a Frisionibus tributum acceperunt); AB 836 has only Dorestad and Frisia ravaged again. Cite Ann. Fuld. 836 for Antwerp and Witla, AB 836 for Dorestad. (nl.wikipedia Witla read this run; Latin from memory.) |
| 435 | 837-04-halley-comet | 837-04 | A comet seen for 25 days in Virgo (Halley's comet) | right | K |  |
| 436 | 837-06-17-walcheren-raid | 837-06-17 | Northmen fall on Walcheren, kill Count Eggihard and Hemming son of Halfdan,... | *doubtful* | K | Event right (AB 837). I could not confirm that the AB give the day xv kal. Iul.; the day may come from another source (?). |
| 437 | 838-hingston-down | 838 | King Ecgberht of Wessex defeats a great Viking fleet and the Cornish togeth... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 438 | 838-12-26-frisia-flood | 838-12-26 | A storm flood breaks over Frisia: the sea covers almost all of it and 2,437... | right | K |  |
| 439 | 839-05-18-rhos-envoys | 839-05-18 | Byzantine envoys reach Louis at Ingelheim with men who call themselves 'Rho... | right | K |  |
| 440 | 840-06-20-louis-pious-dies | 840-06-20 | Emperor Louis the Pious dies on an island in the Rhine near Ingelheim | right | K |  |
| 441 | 841-05-rouen-sacked | 841-05 | Northmen under Oscherus come up the Seine, sack Rouen and burn the abbey of... | right | K |  |
| 442 | 841-06-25-fontenoy | 841-06-25 | Battle of Fontenoy: Charles the Bald and Louis the German defeat Lothar in ... | right | K |  |
| 443 | 841-duiblinn-longphort | 841 | Northmen build a ship-fort (longphort) at Dublin and winter there | right | K |  |
| 444 | 841-walcheren-harald | 841 | Lothar grants Walcheren and the neighbouring places to the Danish exile Har... | right | K |  |
| 445 | 842-02-14-strasbourg-oaths | 842-02-14 | Oaths of Strasbourg: Louis the German and Charles the Bald swear alliance, ... | right | K |  |
| 446 | 842-quentovic-sacked | 842 | A Northman fleet sacks Quentovic at dawn, killing and carrying off its peop... | right | K |  |
| 447 | 843-06-24-nantes-sacked | 843-06-24 | Northmen, led by the rebel count Lambert, storm Nantes on St John's day and... | **wrong** | K | Event and day right. Two faults: (1) the name Westfaldingi is not in the AB; it is in the Annales Engolismenses (Angouleme annals) 843, MGH SS 16 (?) - fix the note and add that source. (2) That Count Lambert led the Northmen in is said only by the 11th-century Chronicle of Nantes; the AB do not say it. The title should not state it as fact. |
| 448 | 843-kenneth-alba | 843 | Kenneth son of Alpin, king of Dal Riata, becomes king of the Picts as well ... | right | K |  |
| 449 | 843-08-verdun | 843-08 | Treaty of Verdun: the Frankish empire divided into west (Charles), middle (... | right | K |  |
| 450 | 844-autumn-seville-raid | 844 | A Viking fleet comes up the Guadalquivir and takes Seville; the emir's army... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 451 | 845-03-28-paris-sacked | 845-03-28 | Ragnar's fleet of 120 ships comes up the Seine and sacks Paris on Easter Su... | **wrong** | F | Day and feast do not agree. 28 March 845 was a Saturday (Holy Saturday); Easter Sunday was 29 March 845 (computed, Julian; Wikipedia, Siege of Paris (845), also gives Easter Sunday 29 March). Either keep 845-03-28 and say Easter eve, or keep Easter Sunday and make date and fire 845-03-29. The 120 ships, Reginherus and the 7,000 pounds are right (AB 845; Translatio S. Germani). |
| 452 | 845-hamburg-burned | 845 | A Danish fleet takes Hamburg by surprise and burns it, church, books and al... | right | K |  |
| 453 | 845-turgeis-drowned | 845 | The Viking leader Turgéis captured by Máel Sechnaill and drowned in Loch Uair | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 454 | 846-dorestad-burned | 846 | Danish pirates burn Dorestad and two other villages while Lothar watches fr... | right | K |  |
| 455 | 846-08-rome-raid | 846-08 | Arab raiders from Africa land at Ostia, plunder St Peter's and St Paul's ou... | right | K |  |
| 456 | 850-rorik-dorestad | 850 | Lothar grants Dorestad and other counties to the Danish chief Rorik, who ha... | right | K |  |
| 457 | 850-hedeby-church | 850 | King Horik lets Ansgar build a church at Sliesthorp, the port where merchan... | right | K |  |
| 458 | 851-thanet-canterbury-london | 851 | Heathens winter on Thanet for the first time; 350 ships storm Canterbury an... | right | K |  |
| 459 | 854-horik-killed | 854 | Horik I and most of the Danish royal house fall in a civil war with his nep... | right | K |  |
| 460 | 854-courland-war | 854 | The Svear under King Olof take Seeburg in Courland and besiege Apulia until... | *doubtful* | K | Rimbert c. 30 right; the year about 854 is a reckoning. ports_793 closes grobin in 850, four years before this entry burns it: move the port table end to about 855 or drop the tag. Seeburg = Grobina is itself only likely. |
| 461 | 859-mediterranean-voyage | 859 | Danish pirates sail through the Straits of Gibraltar, raid Spain and Morocc... | right | K |  |
| 462 | 859-winter-hard | 859 | A very hard, long winter, with snow and frost from November to April; in It... | right | K |  |
| 463 | 860-06-18-rus-constantinople | 860-06-18 | A Rus fleet of 200 ships attacks Constantinople while the emperor is away, ... | right | K |  |
| 464 | 862-rurik-ladoga | 862 | The Primary Chronicle's year for Rurik and his brothers being called from o... | right | K |  |
| 465 | 863-dorestad-last | 863 | The Danes sack Dorestad once more and sail up the Rhine towards Cologne | **wrong** | F | Month wrong. AB 863 opens: Dani mense Ianuario per Rhenum versus Coloniam navigio ascendunt, after sacking Dorestad. Make date 863-01, precision month, fire 863-01-01 (now year / 1 March). Source: AB 863 (quoted in Wikipedia, Rorik of Dorestad, read this run). |
| 466 | 864-06-25-pitres | 864-06-25 | Edict of Pîtres: a new coinage from ten mints only, fortified bridges on th... | right | K |  |
| 467 | 865-autumn-great-army | 865 | A great heathen army comes to England and winters in East Anglia, where the... | right | K |  |
| 468 | 866-11-01-york-taken | 866-11-01 | The great army crosses the Humber and takes York on All Saints' Day | right | K |  |
| 469 | 867-03-21-york-battle | 867-03-21 | The Northumbrian kings Osberht and Ælla storm York together and both are ki... | right | K |  |
| 470 | 869-11-20-edmund-killed | 869-11-20 | The great army winters at Thetford; King Edmund of East Anglia fights them ... | right | K |  |
| 471 | 870-dumbarton-taken | 870 | Amlaíb and Ímar besiege Dumbarton Rock for four months and destroy it; next... | right | K |  |
| 472 | 870-08-08-meerssen | 870-08-08 | Treaty of Meerssen: Charles the Bald and Louis the German divide Lotharingi... | right | K |  |
| 473 | 871-01-08-ashdown | 871-01-08 | Battle of Ashdown: King Æthelred and his brother Alfred defeat the Danish army | right | K |  |
| 474 | 871-04-alfred-king | 871-04 | King Æthelred of Wessex dies after Easter; his brother Alfred becomes king | right | C |  |
| 475 | 872-hafrsfjord | 872 | Battle of Hafrsfjord: Harald Fairhair defeats the kings of the west and rul... | right | K |  |
| 476 | 874-burgred-mercia | 874 | The great army winters at Repton and drives King Burgred out of Mercia; he ... | right | K |  |
| 477 | 874-iceland-settled | 874 | Ingólfr Arnarson settles at Reykjavík: the beginning of the settlement of I... | right | K |  |
| 478 | 876-northumbria-shared | 876 | Halfdan shares out the land of Northumbria and his men begin to plough and ... | right | K |  |
| 479 | 878-01-chippenham | 878-01 | The Danish army takes Chippenham by stealth after Twelfth Night; Alfred esc... | right | K |  |
| 480 | 878-05-edington | 878-05 | Battle of Edington: Alfred defeats Guthrum, who gives hostages and is bapti... | right | C |  |
| 481 | 879-11-ghent-army | 879-11 | The great army crosses from England to Francia and winters at Ghent, raidin... | right | K |  |
| 482 | 880-02-02-luneburg-heath | 880-02-02 | A Viking army crushes the Saxons in a winter battle; Duke Brun, two bishops... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 483 | 881-08-03-saucourt | 881-08-03 | Battle of Saucourt: King Louis III defeats a Viking army in Vimeu | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 484 | 881-winter-aachen-cologne | 881 | From their camp at Elsloo on the Meuse the Northmen burn Liège, Maastricht,... | *doubtful* | F | Content right (Ann. Fuld. 881-882, Regino). The camp Ascloha is put here at Elsloo (50.95, 5.77) and in 882-07-asselt-godfrid at Asselt (51.2, 6.0): one camp, two places. Make them agree. |
| 485 | 882-oleg-kiev | 882 | The Primary Chronicle's year for Oleg taking Kiev and making it the Rus cap... | right | K |  |
| 486 | 882-07-asselt-godfrid | 882-07 | At Asselt, Charles the Fat buys peace: Godfrid is baptised and given Frisia... | **wrong** | F | Event right. Faults: (1) port dorestad is tagged, but ports_793 closes Dorestad in 863 - drop the port tag. (2) place says Asselt (Elsloo): these are two villages 30 km apart, both proposed for the one camp Ascloha; the coordinates given are Asselt, while 881-winter-aachen-cologne puts the same camp at Elsloo. Pick one for both entries (most scholars now Asselt, 51.19 N 5.99 E) and name the other in the note. The 2,080 pounds agrees with Ann. Fuld. 882 as I recall it (?). |
| 487 | 885-06-godfrid-killed | 885-06 | Duke Godfrid of Frisia killed at Herispich in the Betuwe by the emperor's men | **wrong** | K | Event right (Herispich, May-June 885; Ann. Fuld., Regino). Port dorestad is tagged, but ports_793 closes Dorestad in 863: drop the port tag (the region is enough). |
| 488 | 885-11-24-siege-paris | 885-11-24 | Siege of Paris: a huge Viking fleet under Sigfrid besieges the city for ele... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 489 | 886-london-alfred | 886 | Alfred occupies London and gives it to Ealdorman Æthelred of Mercia; all th... | right | K |  |
| 490 | 887-11-charles-fat-deposed | 887-11 | Charles the Fat deposed at Tribur; Arnulf of Carinthia is made king in the ... | right | K |  |
| 491 | 890-ohthere-wulfstan | 890 | Ohthere of Hålogaland and Wulfstan tell King Alfred of their voyages: north... | *doubtful* | K | Sailing times right (Old English Orosius). ports_793 closes truso in 850, but Wulfstan sails to Truso about 890 and the site was used well into the 10th century (Jagodzinski, Janow Pomorski) (?): the port table should run Truso to about 950. |
| 492 | 891-dyle | 891 | King Arnulf storms the Viking camp on the Dyle near Leuven and destroys the... | *doubtful* | K | Battle right. The traditional 1 September sits badly with the Fulda annals (Arnulf marched after the defeat on the Geul of 26 June, and the fight is told late in the year); October-November is also argued. Keep season precision; do not show a day. Also: the title says Viking - see change 8. |
| 493 | 892-kent-armies | 892 | The army from Boulogne crosses to Kent with 250 ships and builds a fort at ... | right | K |  |
| 494 | 896-alfred-ships | 896 | Alfred has longships built against the raiders, nearly twice as long as the... | right | K |  |
| 495 | 899-10-26-alfred-dies | 899-10-26 | King Alfred dies; his son Edward the Elder succeeds | right | K |  |
| 496 | 902-dublin-expelled | 902 | The men of Brega and Leinster drive the Norse out of Dublin; they leave man... | right | K |  |
| 497 | 907-oleg-tsargrad | 907 | The Primary Chronicle's year for Oleg's campaign against Constantinople wit... | right | K |  |
| 498 | 910-08-05-tettenhall | 910-08-05 | Battle of Tettenhall: the West Saxons and Mercians destroy a Northumbrian D... | right | K |  |
| 499 | 911-09-02-rus-treaty | 911-09-02 | Oleg's envoys sign a treaty with the emperors Leo and Alexander: rules for ... | right | K |  |
| 500 | 911-rollo-rouen | 911 | King Charles the Simple grants Rouen and the lands round the lower Seine to... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 501 | 913-rus-caspian | 913 | A Rus fleet of 500 ships passes the Khazars by the Don and Volga and ravage... | right | K |  |
| 502 | 917-dublin-retaken | 917 | Sihtric grandson of Ímar retakes Dublin for the Norse; his kinsman Ragnall ... | right | K |  |
| 503 | 918-06-12-aethelflaed-dies | 918-06-12 | Æthelflæd, Lady of the Mercians, dies at Tamworth; the people of York had o... | right | K |  |
| 504 | 922-05-12-ibn-fadlan | 922-05-12 | The caliph's envoy Ibn Fadlan reaches the Volga Bulghars; he sees the Rus m... | right | K |  |
| 505 | 927-07-12-eamont | 927-07-12 | Æthelstan takes York; at Eamont King Constantine of Alba, Hywel of the Wels... | right | K |  |
| 506 | 928-grately-laws | 928 | Æthelstan's laws at Grately: one coinage throughout his realm, moneyers nam... | right | K |  |
| 507 | 929-01-16-cordoba-caliph | 929-01-16 | Abd al-Rahman III takes the title of caliph at Cordoba | right | K |  |
| 508 | 930-althing | 930 | The Althing, the general assembly of Iceland, first meets at Þingvellir wit... | right | K |  |
| 509 | 934-gnupa-tribute | 934 | King Henry I of the East Franks defeats the Danes and makes their king Gnup... | right | U | Taken on its citation; I could not check the source text. |
| 510 | 936-08-07-otto-crowned | 936-08-07 | Otto I crowned king of the East Franks at Aachen | right | K |  |
| 511 | 937-brunanburh | 937 | Battle of Brunanburh: Æthelstan and Edmund defeat Olaf of Dublin, Constanti... | right | K |  |
| 512 | 939-eldgja | 939 | The Eldgjá fissure eruption in Iceland pours out the largest lava flood of ... | **wrong** | F | Dating right. Source wrong: the blood-red sun is in the Chronicon Scotorum (s.a. 939), not the Annals of Ulster, and it reads from the beginning of one day to midday on the next, not from morning to midday. Change AU 939 to CS 939 and the note. Source: Oppenheimer et al. 2018, who quote CS. |
| 513 | 939-10-27-aethelstan-dies | 939-10-27 | King Æthelstan dies; Olaf Guthfrithson of Dublin takes York and the Danish ... | right | K |  |
| 514 | 941-06-11-igor-greek-fire | 941-06-11 | Prince Igor's Rus fleet attacks Constantinople and the Bithynian coast; the... | right | K |  |
| 515 | 944-igor-treaty | 944 | Igor's treaty with Byzantium: Rus ships must carry the prince's charter, me... | right | K |  |
| 516 | 948-danish-bishops | 948 | Bishops of Schleswig, Ribe and Aarhus sit at the synod of Ingelheim: the fi... | right | K |  |
| 517 | 954-eric-york | 954 | The Northumbrians drive out Eric (Bloodaxe), the last king of York; he is k... | right | K |  |
| 518 | 955-08-10-lechfeld | 955-08-10 | Battle of the Lechfeld: Otto I destroys the Magyar army near Augsburg | right | K |  |
| 519 | 957-olga-constantinople | 957 | Princess Olga of Kiev is received at the Great Palace in Constantinople | right | K |  |
| 520 | 958-gorm-jelling | 958 | King Gorm the Old is buried in the great mound at Jelling (dated by its tim... | right | K |  |
| 521 | 961-03-07-crete | 961-03-07 | Nikephoros Phokas takes Chandax: Crete back in Byzantine hands after some 1... | *doubtful* | F | The fall of Chandax is now usually given as 6 March 961 (Wikipedia, Siege of Chandax); 7 March appears in older books. Suggest 961-03-06, or precision month. |
| 522 | 965-harald-baptised | 965 | Harald Bluetooth is baptised after the priest Poppo carries hot iron unharm... | right | K |  |
| 523 | 965-ibrahim-hedeby | 965 | Ibrahim ibn Yaqub of Tortosa travels in the north: at Mainz he sees Samarka... | right | K |  |
| 524 | 968-danevirke | 968 | The Danevirke strengthened: a connecting wall built to join Hedeby's rampar... | right | U | Taken on its citation; I could not check the source text. |
| 525 | 969-khazaria-fall | 969 | Svyatoslav of Kiev breaks the Khazar kaganate: Sarkel taken, Itil and Saman... | right | K |  |
| 526 | 969-07-fatimid-cairo | 969-07 | The Fatimids take Egypt and found Cairo beside Fustat | *doubtful* | K | Jawhar entered Fustat on 6 July 969 (?); 9 July is given by some. Fire 969-07-06 or leave at month. |
| 527 | 973-05-11-edgar-bath | 973-05-11 | King Edgar crowned at Bath; afterwards at Chester six kings come to him and... | right | C |  |
| 528 | 973-edgar-coinage | 973 | Edgar's coin reform: one design of penny for all England, struck by named m... | right | K |  |
| 529 | 974-otto-danevirke | 974 | Otto II storms the Danevirke and sets a garrison in the Schleswig march; th... | right | K |  |
| 530 | 975-birka-sigtuna | 975 | Birka is abandoned in the later 10th century; a new royal town, Sigtuna, is... | right | K |  |
| 531 | 980-raids-southampton-chester | 980 | Viking raids begin again: Southampton sacked and most of its people killed ... | right | K | Facts right. Wording: the title says Viking (content rule 2) - see change 8. |
| 532 | 980-tara | 980 | Battle of Tara: Máel Sechnaill defeats the Norse of Dublin; King Amlaíb Cua... | right | K |  |
| 533 | 980-trelleborgs | 980 | Harald Bluetooth builds the ring fortresses: Trelleborg, Fyrkat, Aggersborg... | right | K |  |
| 534 | 983-slav-rising | 983 | The great Slav rising: the Lutici and Obodrites destroy Havelberg and Brand... | **wrong** | K | Event and days right (Thietmar iii.17-19). Port reric is tagged, but Reric was destroyed in 808 and ports_793 closes it then: drop the port tag. |
| 535 | 985-greenland | 985 | Erik the Red leads settlers from Iceland to Greenland; of 25 ships, 14 arri... | right | K |  |
| 536 | 985-bulghar-peace | 985 | Vladimir of Kiev campaigns against the Volga Bulghars and makes peace 'unti... | right | U | Taken on its citation; I could not check the source text. |
| 537 | 986-harald-dies | 986 | Harald Bluetooth dies of wounds after his son Sweyn Forkbeard rises against... | right | K |  |
| 538 | 988-vladimir-baptised | 988 | Vladimir of Kiev is baptised, marries the emperor's sister Anna and has the... | right | K |  |
| 539 | 991-08-maldon | 991-08 | Olaf's fleet of 93 ships ravages Folkestone, Sandwich and Ipswich; Ealdorma... | right | K |  |
| 540 | 992-03-venice-chrysobull | 992-03 | Basil II grants Venice a chrysobull: lower tolls for Venetian ships at Abyd... | right | K |  |
| 541 | 994-09-08-london-attack | 994-09-08 | Olaf and Sweyn come to London with 94 ships and try to burn it; the townsme... | right | K |  |
| 542 | 995-olaf-tryggvason | 995 | Olaf Tryggvason comes to Norway and is made king after the fall of Jarl Hák... | right | K |  |
| 543 | 995-sigtuna-coins | 995 | King Olof Skötkonung has the first Swedish coins struck at Sigtuna, copied ... | right | U | Taken on its citation; I could not check the source text. |
| 544 | 997-dublin-coins | 997 | The first coins of Dublin are struck under King Sihtric Silkbeard, copied f... | right | U | Taken on its citation; I could not check the source text. |
| 545 | 1000-iceland-christian | 1000 | The Althing decides that all Icelanders shall be Christian, though eating h... | right | K |  |
| 546 | 1000-silver-fades | 1000 | Islamic silver stops reaching the Baltic: the last dirhams in northern hoar... | right | K |  |
| 547 | 1000-vinland | 1000 | Leif Eriksson sails from Greenland to lands in the west, Helluland, Marklan... | right | K |  |
| 548 | 1000-london-tolls | 1000 | London's toll rules: at Billingsgate a small boat pays a halfpenny, a saili... | right | K |  |
| 549 | 1000-09-09-svolder | 1000-09-09 | Sea battle of Svolder: Olaf Tryggvason is beaten by the kings of the Danes ... | right | K |  |
| 550 | 1002-11-13-st-brice | 1002-11-13 | St Brice's Day: King Æthelred orders all the Danish men in England killed; ... | right | K |  |
| 551 | 1006-tiel-burned | 1006 | Northmen sail up the Waal and burn Tiel; at Utrecht the people burn their o... | *doubtful* | K | Alpert (book i) tells the burning of Tiel under 1006 and the Northmen coming back the next year, 1007, when the men of Utrecht burned their own portus (?). Split the Utrecht half to 1007 or say the next year in the title. |
| 552 | 1007-tribute | 1007 | Æthelred pays the Danish army 36,000 pounds to leave | right | K |  |
| 553 | 1009-08-thorkell | 1009-08 | 'Thorkell's army' lands at Sandwich; East Kent pays 3,000 pounds; the army ... | right | K |  |
| 554 | 1012-04-19-aelfheah | 1012-04-19 | The Danish army kills Archbishop Ælfheah at Greenwich; 48,000 pounds of tri... | right | K |  |
| 555 | 1013-sweyn-conquers | 1013 | Sweyn Forkbeard conquers England: the north submits at Gainsborough, then O... | right | K |  |
| 556 | 1014-02-03-sweyn-dies | 1014-02-03 | Sweyn Forkbeard dies at Gainsborough; the fleet chooses his son Cnut as kin... | right | K |  |
| 557 | 1014-04-23-clontarf | 1014-04-23 | Battle of Clontarf: Brian Boru's army defeats the Leinstermen, the Dublin N... | right | C |  |
| 558 | 1014-09-28-sea-flood | 1014-09-28 | The great sea-flood: on Michaelmas eve the sea runs further inland than eve... | right | K |  |
| 559 | 1016-yaroslav-pravda | 1016 | Yaroslav gives the men of Novgorod a written law, the oldest part of the Ru... | right | U | Taken on its citation; I could not check the source text. |
| 560 | 1016-10-18-assandun | 1016-10-18 | Battle of Assandun: Cnut defeats Edmund Ironside; they share England, and o... | right | K |  |
| 561 | 1018-heregeld | 1018 | Cnut takes a tribute of 72,000 pounds from England and 10,500 from London, ... | right | K |  |
| 562 | 1018-07-29-vlaardingen | 1018-07-29 | Battle of Vlaardingen: Count Dirk III of West Frisia beats the emperor's ar... | right | K |  |
| 563 | 1020-tiel-merchants | 1020 | The merchants of Tiel live by their own law: they claim an imperial charter... | right | K |  |
| 564 | 1021-lanse-aux-meadows | 1021 | Norsemen cut wood with iron tools at L'Anse aux Meadows in Newfoundland (da... | right | K |  |
| 565 | 1027-03-26-cnut-rome | 1027-03-26 | Cnut at Rome for Conrad II's crowning wins lower tolls and safe passage for... | right | C |  |
| 566 | 1028-cnut-norway | 1028 | Cnut sails to Norway with fifty ships, drives out King Olaf Haraldsson and ... | **wrong** | K | Event right (ASC E 1028: fifty ships). Port skiringssal is tagged, but Kaupang was given up about 930-960 (ports_793: to 960): drop it. |
| 567 | 1030-07-29-stiklestad | 1030-07-29 | Battle of Stiklestad: Olaf Haraldsson, back from Russia, is killed by the f... | right | K |  |
| 568 | 1031-11-30-cordoba-falls | 1031-11-30 | The caliphate of Cordoba is abolished; al-Andalus breaks up into the taifa ... | right | K |  |
| 569 | 1035-11-12-cnut-dies | 1035-11-12 | Cnut dies at Shaftesbury; his empire breaks up among his sons | right | K |  |
| 570 | 1042-06-08-edward-king | 1042-06-08 | Harthacnut dies at a wedding feast at Lambeth; Edward, son of Æthelred, is ... | right | K |  |
| 571 | 1043-rus-constantinople | 1043 | The last Rus attack on Constantinople, under Vladimir son of Yaroslav, is b... | right | K |  |
| 572 | 1043-09-28-lyrskov | 1043-09-28 | Battle of Lyrskov Heath near Hedeby: King Magnus of Norway and Denmark crus... | right | K |  |
| 573 | 1047-10-25-harald-hardrada | 1047-10-25 | Magnus the Good dies; Harald Hardrada, back from the Varangian Guard with g... | right | K |  |
| 574 | 1050-hedeby-burned | 1050 | Harald Hardrada burns Hedeby from end to end | right | K |  |
| 575 | 1054-07-16-schism | 1054-07-16 | Cardinal Humbert lays a bull excommunicating Patriarch Michael Keroularios ... | right | K |  |
| 576 | 1066-01-05-edward-dies | 1066-01-05 | Edward the Confessor dies; Harold Godwinson is crowned king the next day | right | K |  |
| 577 | 1066-04-24-halley | 1066-04-24 | A great comet (Halley's comet) seen for a week from 24 April | right | K |  |
| 578 | 1066-hedeby-slavs | 1066 | After the Obodrite prince Gottschalk is killed, the Slavs rise, burn Hambur... | **wrong** | K | Event right (Adam iii.50-51; Gottschalk killed 7 June 1066 at Lenzen). Port reric is tagged, but Reric ended in 808: drop it. |
| 579 | 1066-09-25-stamford-bridge | 1066-09-25 | Harald Hardrada and Tostig beat the northern earls at Fulford, then are kil... | right | K |  |
| 580 | 1066-09-28-pevensey | 1066-09-28 | Duke William of Normandy lands at Pevensey and builds a castle at Hastings | *doubtful* | K | Event right. Port dofras (Dover) is some 80 km from Pevensey; harmless, but the log will show the landing at Dover. |
| 581 | 1066-10-14-hastings | 1066-10-14 | Battle of Hastings: King Harold is killed and the English army destroyed by... | right | C |  |
| 582 | 1066-12-25-william-crowned | 1066-12-25 | William of Normandy crowned king of England at Westminster on Christmas Day | right | K |  |

## events_793.json: the 13 happenings

No dates, places or people, so nothing to check against annals. Checked for anachronism and wording against content rules 2 and 3.

| id | verdict | correction or doubt |
|---|---|---|
| fever | right | A man sick at sea, rested or kept at the oars: nothing out of period. |
| casks_spoil | *doubtful* | Stave-built casks are right for the period (Rhine wine casks reused as well linings at Dorestad). The fact text says "60 litres": a modern unit in shown text. |
| becalmed | right | Sail and oars both right for a Gislinge-size boat. |
| fog | right | Sounding with a lead or pole is attested (Old English sundgyrd, sundline in Aelfric's glossary) (?). "The lookout can't see the bow" is odd on a 7.7 m boat with four men; "You can't see the stem" would do. |
| leak | right | Caulking with tarred wool is exactly what clinker boats of the time had between the strakes (Skuldelev, Gislinge). |
| catch | *doubtful* | Hand lines and hooks are right. The effect gives `dried_fish` for a fresh catch: a data oddity, not an anachronism. |
| gale | *doubtful* | "Heave to" and "hove to" are terms of later fore-and-aft and multi-mast rigs. A one-masted square-sail boat lowers the sail and rides it out, bows to the sea, under oars or a drag. Reword: "Lower the sail and ride it out". "{n} kg" is a modern unit in shown text. |
| grumbling | *doubtful* | "Promise them a bonus": modern word. Say "Promise them a larger share in port". "6 deniers": see the coin point below. |
| passenger | *doubtful* | Fare "8 deniers": see the coin point below. |
| letters | right | Merchants and pilgrims carried the letters of the time (Alcuin's and Boniface's letters went this way). |
| theft | *doubtful* | Fine as an event. "10 deniers' worth": see the coin point below. |
| dispute | **wrong** outside England | "Take it to the reeve": a reeve (gerefa) is an English officer. The card fires in any emporium, market or town. In a Frankish port the man is the count's officer or the toll-keeper (procurator, as at Quentovic and Dorestad); at Birka Rimbert calls him the king's prefect; in Ireland the king's steward. Either limit the card to English regions or make the label and deed text use an officer word per realm. |
| leak, gale, casks_spoil (units) | *doubtful* | Litres and kilograms appear in the fact text the player reads. If the interface shows weights elsewhere in kilograms this is consistent; if cards are meant to sound of the time, say "a cask of water" and "a fifth of the food". |

The coin point, for all cards that name money: every text says "deniers". That is right in Frankish ports and close enough in English ones (pennies), but content rule 2 says there is no coin in Scandinavia before about 825 and silver goes by weight in the Baltic. The same card fired at Birka or Reric in 793 would pay "8 deniers". Since the cards have no year or region gate, use a word that holds everywhere ("8 pennies' weight of silver"), or let the text take the coin word from the port's realm.

## The ten changes that matter most

Written for whoever edits `chronicle_793.json`. Entries are found by `id`. Do not rename ids (other files may point at them) unless you search for references first.

1. **845-03-28-paris-sacked: the day and the feast disagree.** 28 March 845 was Holy Saturday; Easter Sunday was 29 March. Choose one:
   - keep `"date":"845-03-28"` and change the title's "on Easter Sunday" to "on Easter eve"; or
   - keep "on Easter Sunday" and set `"date":"845-03-29"` and `"fire":"845-03-29"`.
   Add to the note: "Easter 845 fell on 29 March."

2. **863-dorestad-last: January, not March.** Set `"date":"863-01"`, `"precision":"month"`, `"fire":"863-01-01"`. Start the note with "The AB: in January." This is the entry that closes Dorestad, so the port should close two months earlier than it does now.

3. **798-winter-lisbon-sacked: wrong winter.** The sack belongs to the campaign season of 798; the envoys with the spoils came in winter 798/799. Set `"precision":"year"`, remove `"season"`, set `"fire":"798-07-01"`, and change the title to "Alfonso II sacks Lisbon; in the winter he sends Charles mail-coats, mules and Moorish captives". Keep the id. Move the line so the file stays in firing order (after the other 798-07-01 entries).

4. **Port tags on closed ports.** Remove the port from `ports` in these entries (keep `regions`):
   - `882-07-asselt-godfrid`: remove `dorestad`
   - `885-06-godfrid-killed`: remove `dorestad`
   - `983-slav-rising`: remove `reric`
   - `1066-hedeby-slavs`: remove `reric`
   - `1028-cnut-norway`: remove `skiringssal`
   Three more mismatches are the port table's fault, not the chronicle's, and need a decision by whoever owns `ports_793.json`: `reric` should run to 809 (the Frankish annals still call it an emporium when Thrasco is murdered there, entry `809-winter-thrasco-murdered`); `truso` should run to about 950, not 850 (Wulfstan sails there about 890, entry `890-ohthere-wulfstan`); `grobin` should run to about 855 (entry `854-courland-war` burns it in 854).

5. **836-witla-antwerp-burned: wrong source.** Change `"sources":["AB 836"]` to `"sources":["Ann. Fuld. 836","AB 836"]` and add to the note: "Antwerp and Witla are in the Fulda annals; the AB have only Dorestad and Frisia."

6. **843-06-24-nantes-sacked: two claims put on the wrong source.**
   - Title: change "Northmen, led by the rebel count Lambert, storm Nantes" to "Northmen storm Nantes".
   - Note: replace the first sentence with "The Angouleme annals (Annales Engolismenses 843) call the fleet Westfaldingi, men of Vestfold (?). The later Chronicle of Nantes says the rebel count Lambert guided them in; the AB do not."
   - Add `"Ann. Engolismenses 843"` to `sources`, and add the abbreviation to the `abbreviations` block: `"Ann. Engolismenses":"the annals of Angouleme (MGH SS 16)"`.

7. **One camp, two places (881-winter-aachen-cologne and 882-07-asselt-godfrid).** The Northmen's camp Ascloha is Elsloo in one entry and Asselt in the other, 30 km apart. Use Asselt in both: `"place":"Ascloha (Asselt, near Roermond)"`, `"at":[51.19,5.99]`, and in both notes: "Ascloha is Asselt or Elsloo; most scholars now say Asselt." In the 881 title change "From their camp at Elsloo on the Meuse" to "From their camp at Ascloha on the Meuse".

8. **The word "Viking" in text the player reads.** Content rule 2 keeps "Viking" out of the game's words (Northmen, Danes, heathen men; wicing only for raiders at sea). Nine titles use it; replace as follows:
   - `838-hingston-down`: "a great Viking fleet" -> "a great ship-army of Danes"
   - `844-autumn-seville-raid`: "A Viking fleet" -> "A fleet of Northmen"
   - `845-turgeis-drowned`: "The Viking leader" -> "The Northmen's leader"
   - `880-02-02-luneburg-heath`: "A Viking army" -> "An army of Northmen"
   - `881-08-03-saucourt`: "a Viking army" -> "an army of Northmen"
   - `885-11-24-siege-paris`: "a huge Viking fleet" -> "a huge fleet of Northmen"
   - `891-dyle`: "the Viking camp" -> "the Northmen's camp"
   - `911-rollo-rouen`: "the Viking leader Rollo" -> "the Northman Rollo"
   - `980-raids-southampton-chester`: "Viking raids begin again" -> "The ship-armies come again"
   The word also stands in the `effect` or `note` of `795-rechru-burned`, `845-03-28-paris-sacked`, `876-northumbria-shared`, `980-raids-southampton-chester` and `1066-12-25-william-crowned` ("the end of the Viking Age in England"). Notes are not shown; effects may be. Change the effects the same way.

9. **823-famine-epidemic: the famine is not in the source named.** Change the title to "Hail destroys the crops in many parts of Francia; a great sickness follows", `"kind":"epidemic"`, and the effect to "crops lost to hail: grain dear where it fell; deaths from sickness across Francia". If a famine entry is wanted for these years, the annals name one under 824 (?). Check the text of ARF 823 before shipping: I could not open it.

10. **Small fixes, one line each.**
    - `939-eldgja`: in `sources` change `"AU 939"` to `"CS 939"`; in the note change "The Irish annals of 939 tell of the sun like blood from morning to midday" to "The Chronicon Scotorum of 939 tells of the sun the colour of blood from the start of one day to midday the next".
    - `961-03-07-crete`: set `date` and `fire` to `961-03-06` (keep the id), or set precision to month.
    - `793-summer-narbonne-raid`: in the note change "17 Apr 793-9 Apr 794" to "18 Apr 793-6 Apr 794".
    - `799-sigeric-rome`: set `"date":"798"` and `"fire":"798-07-01"`, move the line, and change the note to "ASC F has it under 798; Ingram's text under 799."
    - `1006-tiel-burned`: change the title's second half to "the next year, at Utrecht, the people burn their own trading quarter rather than leave it to them".
    - `811-07-20-pliska-burned`: change the title to "Nikephoros enters Bulgaria and in the days after takes and burns Krum's residence at Pliska".
    - `812-05-15-solar-eclipse`: set `date` and `fire` to `812-05-14`; keep the note.
    - `831-hamburg-see`: set `"certainty":"uncertain"` and add to the note: "The archbishopric of 831 rests on privileges now thought forged; Ansgar may have been a missionary bishop only."
    - `891-dyle`: change the note to "The day is not known; 1 September is a later tradition."
    - `events_793.json`, card `dispute`: the reeve is English only (see the section above); cards that pay in "deniers" fire in ports with no coin.

## What I could not check

- The 78 rows marked U.
- The Latin of the Annales Fuldenses (836, 882) and the Angouleme annals (843): the pages would not load, so those corrections rest on memory and on the Dutch Wikipedia article for Witla.
- ARF 823 (change 9) and Alpert of Metz on Utrecht in 1007.
- Coordinates were only checked for gross errors (none found), not to the second decimal.
