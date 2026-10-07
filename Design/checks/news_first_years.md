# News of the first years, 793 to 805

From `Assets/Kapman/Game/Data/chronicle_793.json` (582 entries in all; 218 dated 793 to 805, every one listed below) and the ports' lat and lon in `ports_793.json`. Nothing here is invented: date, place and line are as the chronicle holds them.

## How the arrival days are reckoned (rough)

- This is a rough reckoning, not a sailed route. The game has no news-travel rule in this data; the days are for judging only.
- News leaves on the entry's `fire` day (the day the game shows it; for month, season and year entries that is a set day, not a known one).
- Where the entry names ports (`ports`), the news starts at the nearest of those ports. Where it names only trade regions (`regions`), it starts at the nearest port of `ports_793.json` that lies in one of those regions and trades by 805. So a realm-wide entry (famine over all Frankish lands) is 'at' Dorestat on the day itself.
- Distance: great-circle between the two ports' lat and lon, times 1.3 for the coast's bends (my rough factor, there is no sea-way table in the data). 100 km a day, rounded up; same port = same day.
- Entries with no port and no region that holds a port (`regions` empty, or regions with no port in the file) cannot be reckoned from the data: marked `-`. The chronicle says an empty region list means news only, seen everywhere or off the map (`area` says where).
- All dates Julian, as in the chronicle. A new game begins 1 May 793; the chronicle's own `start` field says 793-06-21; entries fired before the start are backstory.

Columns: fire day | date as the sources give it (precision) | place (ports; regions; area; realm) | the line (title) | earliest day at Dorestat, Lundenwic, Quentovic, Ripa (days after the fire day).


## 793

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Jan 793 | 793 (year) | famine | regions: italy, rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Great famine in Burgundy, Francia, Italy, Gothia and Provence; many die | 15 Jan 793 (+0) | 18 Jan 793 (+3) | 15 Jan 793 (+0) | 19 Jan 793 (+4) |
| 22 Feb 793 | 793-02-22 (day) | death | regions: northumbria | Ealdorman Sicga, slayer of King Aelfwald, dies | 1 Mar 793 (+7) | 26 Feb 793 (+4) | 28 Feb 793 (+6) | 2 Mar 793 (+8) |
| 1 Mar 793 | 793 (season) | famine | regions: italy | Famine in Benevento: the Frankish army under Pippin and Louis must eat meat in Lent | - | - | - | - |
| 15 Mar 793 | 793 (season) | portent | ports: eoforwic; regions: northumbria | Blood-red rain falls from the roof of St Peter's church at York in Lent [likely] | 22 Mar 793 (+7) | 19 Mar 793 (+4) | 21 Mar 793 (+6) | 24 Mar 793 (+9) |
| 15 Apr 793 | 793 (season) | portent | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | False grain seen in fields, woods and marshes that could be touched but not eaten [likely] | 15 Apr 793 (+0) | 18 Apr 793 (+3) | 15 Apr 793 (+0) | 19 Apr 793 (+4) |
| 10 May 793 | 793 (season) | portent | regions: northumbria | Portents over Northumbria: flashes of light, whirlwinds and fiery dragons in the sky | 17 May 793 (+7) | 14 May 793 (+4) | 16 May 793 (+6) | 18 May 793 (+8) |
| 20 May 793 | 793 (year) | famine | regions: northumbria | Great famine in Northumbria | 27 May 793 (+7) | 24 May 793 (+4) | 26 May 793 (+6) | 28 May 793 (+8) |
| 26 May 793 | 793-05 (month) | revolt | regions: byzantium | Constantine VI crushes the revolt of the Armeniakon theme [likely] | - | - | - | - |
| 8 Jun 793 | 793-06-08 (day) | raid | ports: lindisfarena; regions: northumbria | Heathen seamen sack the monastery of Lindisfarne, killing and carrying off monks | 17 Jun 793 (+9) | 15 Jun 793 (+7) | 17 Jun 793 (+9) | 17 Jun 793 (+9) |
| 1 Jul 793 | 793 (season) | war | regions: frisia, saxony | News: Count Theoderic's force wiped out by Saxons in Rüstringen by the Weser [likely] | 3 Jul 793 (+2) | 6 Jul 793 (+5) | 6 Jul 793 (+5) | 5 Jul 793 (+4) |
| 1 Jul 793 | 793 (year) | coin | ports: lundenwic; regions: england_south, england_east; realm: mercia | Offa's heavier, broader penny replaces the lighter Mercian coinage (c. 792/3) [uncertain] | 6 Jul 793 (+5) | 1 Jul 793 (+0) | 4 Jul 793 (+3) | 11 Jul 793 (+10) |
| 1 Jul 793 | 793 (year) | accession | regions: ireland | Artri son of Cathal ordained king of Munster; the law of Ailbe proclaimed in Munster | 12 Jul 793 (+11) | 8 Jul 793 (+7) | 10 Jul 793 (+9) | 14 Jul 793 (+13) |
| 1 Jul 793 | 793 (year) | law | regions: ireland | Law of Coman proclaimed in the three Connachta | 12 Jul 793 (+11) | 8 Jul 793 (+7) | 10 Jul 793 (+9) | 14 Jul 793 (+13) |
| 15 Jul 793 | 793 (season) | raid | regions: provence; area: Septimania | Cordoban army burns the suburbs of Narbonne and attacks Gerona | - | - | - | - |
| 15 Jul 793 | 793 (season) | raid | regions: byzantium | Arab raiders take the Byzantine frontier forts of Kamachon and Thebasa [likely] | - | - | - | - |
| 20 Jul 793 | 793 (season) | battle | regions: provence; area: Septimania | Count William of Toulouse beaten by the Cordoban army on the Orbieu | - | - | - | - |
| 1 Aug 793 | 793-08-01 (day) | founding | regions: england_south | Offa raises St Alban's relics and founds St Albans abbey (St Albans tradition) [uncertain] | 5 Aug 793 (+4) | 1 Aug 793 (+0) | 3 Aug 793 (+2) | 10 Aug 793 (+9) |
| 15 Sep 793 | 793 (season) | embassy | ports: lindisfarena; regions: northumbria | Alcuin writes from Francia, offering to ask Charles about the Lindisfarne captives [likely] | 24 Sep 793 (+9) | 22 Sep 793 (+7) | 24 Sep 793 (+9) | 24 Sep 793 (+9) |
| 15 Oct 793 | 793 (season) | war | regions: saxony, frisia | Saxons and East Frisians in revolt: churches burned, priests driven out | 17 Oct 793 (+2) | 20 Oct 793 (+5) | 20 Oct 793 (+5) | 19 Oct 793 (+4) |
| 15 Oct 793 | 793 (season) | works | regions: rhineland; area: Bavaria | Charles digs a ship canal (Fossa Carolina) between Altmühl and Rednitz; rain defeats it | 17 Oct 793 (+2) | 22 Oct 793 (+7) | 20 Oct 793 (+5) | 21 Oct 793 (+6) |
| 25 Dec 793 | 793-12-25 (day) | court | regions: rhineland | Charles brings his boats down the Rednitz and Main; Christmas at Würzburg | 27 Dec 793 (+2) | 1 Jan 794 (+7) | 30 Dec 793 (+5) | 31 Dec 793 (+6) |

## 794

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 20 May 794 | 794 (year) | death | regions: england_east; realm: eastanglia | Offa has King Aethelberht of East Anglia beheaded and takes East Anglia [likely] | 24 May 794 (+4) | 22 May 794 (+2) | 23 May 794 (+3) | 28 May 794 (+8) |
| 1 Jun 794 | 794-06-01 (day) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Synod of Frankfurt fixes grain and bread prices and a new public modius | 1 Jun 794 (+0) | 4 Jun 794 (+3) | 1 Jun 794 (+0) | 5 Jun 794 (+4) |
| 1 Jun 794 | 794-06 (month) | coin | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Frankfurt: the new deniers must be taken in every place, town and market | 1 Jun 794 (+0) | 4 Jun 794 (+3) | 1 Jun 794 (+0) | 5 Jun 794 (+4) |
| 1 Jun 794 | 794-06 (month) | synod | regions: rhineland | Frankfurt synod condemns Adoptionism and rejects the Greek council on images | 3 Jun 794 (+2) | 8 Jun 794 (+7) | 6 Jun 794 (+5) | 7 Jun 794 (+6) |
| 1 Jun 794 | 794-06 (month) | politics | regions: rhineland; area: Bavaria | Tassilo, former duke of Bavaria, gives up all his rights at Frankfurt | 3 Jun 794 (+2) | 8 Jun 794 (+7) | 6 Jun 794 (+5) | 7 Jun 794 (+6) |
| 1 Jul 794 | 794 (year) | battle | regions: spain | Cordoban armies raid Alava and Asturias; Alfonso II ambushes one at Lutos [likely] | - | - | - | - |
| 1 Jul 794 | 794 (year) | raid | regions: dalriata, shetland; area: Hebrides | Heathens lay waste all the islands of Britain | 13 Jul 794 (+12) | 10 Jul 794 (+9) | 12 Jul 794 (+11) | 14 Jul 794 (+13) |
| 15 Jul 794 | 794 (season) | raid | ports: gyrwe; regions: northumbria | Heathens plunder the monastery at Donemuthe (Jarrow); a storm wrecks their ships | 23 Jul 794 (+8) | 21 Jul 794 (+6) | 23 Jul 794 (+8) | 24 Jul 794 (+9) |
| 1 Aug 794 | 794-08-01 (day) | death | ports: eoforwic; regions: northumbria | Aethelheard, former ealdorman turned cleric, dies at York | 8 Aug 794 (+7) | 5 Aug 794 (+4) | 7 Aug 794 (+6) | 10 Aug 794 (+9) |
| 10 Aug 794 | 794-08-10 (day) | death | regions: rhineland | Queen Fastrada dies at Frankfurt; buried at St Alban's, Mainz [likely] | 12 Aug 794 (+2) | 17 Aug 794 (+7) | 15 Aug 794 (+5) | 16 Aug 794 (+6) |
| 15 Aug 794 | 794 (year) | harvest | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Great drought, yet an abundant harvest [likely] | 15 Aug 794 (+0) | 18 Aug 794 (+3) | 15 Aug 794 (+0) | 19 Aug 794 (+4) |
| 15 Oct 794 | 794 (season) | campaign | regions: saxony | Two Frankish armies enter Saxony; the Saxons at the Sintfeld submit and give hostages | - | - | - | - |
| 25 Dec 794 | 794-12-25 (day) | court | regions: meuse | Charles winters at Aachen, from now on his usual winter seat | - | - | - | - |

## 795

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 6 May 795 | 795-05-06 (day) | death | regions: ireland | Bran Ardchenn, king of Leinster, and his queen killed; Finsnechta takes Leinster | 17 May 795 (+11) | 13 May 795 (+7) | 15 May 795 (+9) | 19 May 795 (+13) |
| 1 Jul 795 | 795 (year) | raid | regions: spain | Abd al-Karim ibn Mughith sacks Oviedo, the Asturian capital [likely] | - | - | - | - |
| 1 Jul 795 | 795 (year) | raid | regions: ireland | Heathens burn Rechru (Rathlin, or Lambay off Dublin) | 12 Jul 795 (+11) | 8 Jul 795 (+7) | 10 Jul 795 (+9) | 14 Jul 795 (+13) |
| 1 Jul 795 | 795 (year) | raid | regions: dalriata | Skye overwhelmed and laid waste by heathens (?) [uncertain] | 13 Jul 795 (+12) | 10 Jul 795 (+9) | 12 Jul 795 (+11) | 14 Jul 795 (+13) |
| 1 Jul 795 | 795 (year) | raid | ports: hi; regions: dalriata | Heathens plunder Iona [likely] | 13 Jul 795 (+12) | 10 Jul 795 (+9) | 12 Jul 795 (+11) | 14 Jul 795 (+13) |
| 1 Jul 795 | 795 (year) | raid | regions: ireland | Heathens plunder Inishmurray and Inishbofin off western Ireland [likely] | 12 Jul 795 (+11) | 8 Jul 795 (+7) | 10 Jul 795 (+9) | 14 Jul 795 (+13) |
| 1 Jul 795 | 795 (year) | raid | regions: ireland | Welsh annals note the first coming of the heathens among the southern Irish (?) [uncertain] | 12 Jul 795 (+11) | 8 Jul 795 (+7) | 10 Jul 795 (+9) | 14 Jul 795 (+13) |
| 1 Jul 795 | 795 (year) | war | regions: wales | Offa ravages Rhieinwg in south-west Wales [likely] | - | - | - | - |
| 15 Jul 795 | 795 (season) | campaign | regions: saxony | After an assembly at Kostheim, Charles ravages Saxony to the Elbe at Lüne and Bardowick | - | - | - | - |
| 15 Jul 795 | 795 (season) | death | regions: wendland, saxony; realm: obodrites | Witzin, king of the Abodrites and Frankish ally, killed by Saxons crossing the Elbe | 22 Jul 795 (+7) | 26 Jul 795 (+11) | 26 Jul 795 (+11) | 19 Jul 795 (+4) |
| 15 Jul 795 | 795 (season) | embassy | area: Pannonia | Envoys of the Avar tudun offer his submission and baptism | - | - | - | - |
| 25 Dec 795 | 795-12-25 (day) | death | regions: italy | Pope Hadrian I dies; Leo III elected | - | - | - | - |

## 796

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 10 Jan 796 | 796 (year) | death | area: Pannonia | Avar khagan and jugurrus killed by their own people in civil war | - | - | - | - |
| 15 Jan 796 | 796 (season) | raid | area: Pannonia | Duke Eric of Friuli and the Slav Wonomir plunder the Avar Ring; treasure sent to Aachen | - | - | - | - |
| 28 Mar 796 | 796-03-28 (day) | eclipse | (none given) | Eclipse of the moon between cock-crow and dawn | - | - | - | - |
| 15 Apr 796 | 796 (season) | embassy | regions: meuse, italy | Leo III sends Charles the keys of St Peter's tomb and the banner of Rome | - | - | - | - |
| 15 Apr 796 | 796 (season) | treaty | regions: england_south, neustria, rhine_delta | Charles writes to Offa: each realm's merchants protected; pilgrim-traders pay toll | 15 Apr 796 (+0) | 15 Apr 796 (+0) | 15 Apr 796 (+0) | 20 Apr 796 (+5) |
| 16 Apr 796 | 796-04-16 (day) | death | regions: spain | Emir Hisham I of Cordoba dies; his son al-Hakam I succeeds | - | - | - | - |
| 18 Apr 796 | 796-04-18 (day) | death | regions: northumbria; realm: northumbria | King Aethelred I of Northumbria killed at Corbridge; Osbald made king | 25 Apr 796 (+7) | 22 Apr 796 (+4) | 24 Apr 796 (+6) | 26 Apr 796 (+8) |
| 14 May 796 | 796-05-14 (day) | accession | ports: eoforwic; regions: northumbria; realm: northumbria | Eardwulf recalled from exile and made king of Northumbria; consecrated at York 26 May | 21 May 796 (+7) | 18 May 796 (+4) | 20 May 796 (+6) | 23 May 796 (+9) |
| 15 May 796 | 796-05 (month) | deposition | ports: lindisfarena; regions: northumbria; realm: northumbria | Osbald deposed after 27 days; sails from Lindisfarne to the king of the Picts | 24 May 796 (+9) | 22 May 796 (+7) | 24 May 796 (+9) | 24 May 796 (+9) |
| 1 Jun 796 | 796 (season) | politics | regions: northumbria | Charles, angered by Aethelred's murder, recalls his gifts and threatens the Northumbrians [likely] | 8 Jun 796 (+7) | 5 Jun 796 (+4) | 7 Jun 796 (+6) | 9 Jun 796 (+8) |
| 1 Jul 796 | 796 (year) | portent | regions: england_south | Small globes seen around the sun (later English tradition) [uncertain] | 5 Jul 796 (+4) | 1 Jul 796 (+0) | 3 Jul 796 (+2) | 10 Jul 796 (+9) |
| 1 Jul 796 | 796 (year) | battle | ports: legacaestir; regions: wales, mercia_west | Battle of Rhuddlan; Maredudd, king of Dyfed, dies [likely] | 9 Jul 796 (+8) | 5 Jul 796 (+4) | 7 Jul 796 (+6) | 12 Jul 796 (+11) |
| 1 Jul 796 | 796 (year) | raid | regions: ireland | Heathens in Ireland [likely] | 12 Jul 796 (+11) | 8 Jul 796 (+7) | 10 Jul 796 (+9) | 14 Jul 796 (+13) |
| 1 Jul 796 | 796 (year) | war | regions: byzantium | Kardam of Bulgaria demands tribute; Constantine VI waits at Versinikia, no battle [likely] | - | - | - | - |
| 1 Jul 796 | 796 (year) | raid | regions: byzantium | Arab raid reaches Amorion in Anatolia [likely] | - | - | - | - |
| 1 Jul 796 | 796 (year) | court | regions: caliphate | Harun al-Rashid moves his court to al-Raqqa on the Euphrates [likely] | - | - | - | - |
| 15 Jul 796 | 796 (season) | campaign | area: Pannonia | Pippin crosses the Danube and destroys the Avar Ring; the new khagan submits | - | - | - | - |
| 15 Jul 796 | 796 (season) | synod | area: Pannonia | Bishops meet on the Danube to settle how the Avars are to be baptised [likely] | - | - | - | - |
| 15 Jul 796 | 796 (season) | campaign | regions: saxony | Charles ravages Saxony; the Franks bridge the Weser at Alisni [likely] | - | - | - | - |
| 15 Jul 796 | 796 (season) | raid | regions: spain | A Frankish army raids Saracen lands in Spain | - | - | - | - |
| 29 Jul 796 | 796-07-29 (day) | death | regions: england_south, mercia_west, england_east; realm: mercia | Offa, king of the Mercians, dies; his son Ecgfrith succeeds | 2 Aug 796 (+4) | 29 Jul 796 (+0) | 31 Jul 796 (+2) | 6 Aug 796 (+8) |
| 10 Aug 796 | 796-08-10 (day) | death | ports: eoforwic; regions: northumbria | Archbishop Eanbald I of York dies; Eanbald II consecrated at Sockburn on 14 August | 17 Aug 796 (+7) | 14 Aug 796 (+4) | 16 Aug 796 (+6) | 19 Aug 796 (+9) |
| 20 Aug 796 | 796 (season) | war | ports: fordwic; regions: england_south; realm: kent | Eadberht Praen seizes Kent; Archbishop Aethelheard flees Canterbury [likely] | 24 Aug 796 (+4) | 22 Aug 796 (+2) | 22 Aug 796 (+2) | 29 Aug 796 (+9) |
| 1 Sep 796 | 796 (year) | accession | ports: gipeswic, dommoc; regions: england_east; realm: eastanglia | Eadwald rules East Anglia as an independent king after Offa's death (c. 796-798) [uncertain] | 5 Sep 796 (+4) | 3 Sep 796 (+2) | 4 Sep 796 (+3) | 9 Sep 796 (+8) |
| 15 Sep 796 | 796 (year) | mission | regions: meuse; area: Pannonia | The Avar tudun comes to Aachen with many Avars and is baptised | - | - | - | - |
| 15 Nov 796 | 796 (season) | trade | regions: meuse, italy | Pippin brings the rest of the Avar treasure to Aachen; Charles shares it out | - | - | - | - |
| 15 Dec 796 | 796-12 (month) | accession | regions: england_south, mercia_west; realm: mercia | Ecgfrith of Mercia dies after 141 days; Cenwulf king of the Mercians | 19 Dec 796 (+4) | 15 Dec 796 (+0) | 17 Dec 796 (+2) | 24 Dec 796 (+9) |

## 797

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Apr 797 | 797 (year) | politics | regions: spain, meuse | Zatun, lord of Barcelona, comes to Aachen and hands over his city | - | - | - | - |
| 20 Apr 797 | 797 (year) | embassy | regions: spain, meuse | Abdallah, exiled uncle of al-Hakam, seeks Charles's help at Aachen | - | - | - | - |
| 25 Apr 797 | 797 (year) | embassy | regions: byzantium, meuse | Theoktistos, envoy of the governor of Sicily, brings Constantine VI's letter to Aachen | - | - | - | - |
| 1 Jul 797 | 797 (year) | embassy | regions: caliphate | Isaac the Jew, Lantfrid and Sigimund sent as envoys to Harun al-Rashid [likely] | - | - | - | - |
| 1 Jul 797 | 797 (year) | war | regions: byzantium | Harun campaigns in Asia Minor and takes al-Safsaf; a column raids to Ankara [likely] | - | - | - | - |
| 1 Jul 797 | 797-07 (month) | portent | (none given) | Mars not seen anywhere in the sky for a whole year | - | - | - | - |
| 1 Jul 797 | 797 (year) | politics | regions: spain | Day of the Ditch: Toledo's notables massacred by al-Hakam's governor (?) [uncertain] | - | - | - | - |
| 1 Jul 797 | 797 (year) | accession | regions: ireland | King Donnchad of Tara dies; Aed son of Niall wins at Druim Rig and takes the kingship | 12 Jul 797 (+11) | 8 Jul 797 (+7) | 10 Jul 797 (+9) | 14 Jul 797 (+13) |
| 15 Jul 797 | 797 (season) | campaign | regions: spain | Louis sent to besiege Huesca [likely] | - | - | - | - |
| 15 Jul 797 | 797 (season) | campaign | regions: saxony, frisia | Charles marches through the marshes to Hadeln on the sea; Wigmodia taken | 17 Jul 797 (+2) | 20 Jul 797 (+5) | 20 Jul 797 (+5) | 19 Jul 797 (+4) |
| 15 Jul 797 | 797 (season) | fleet | regions: saxony | Royal ships hauled overland and by river into Saxony; camp fortified with them [likely] | - | - | - | - |
| 19 Aug 797 | 797-08-19 (day) | accession | regions: byzantium | Constantine VI blinded in the Porphyra; Irene rules alone | - | - | - | - |
| 20 Aug 797 | 797-08 (month) | portent | regions: byzantium | Sun darkened for seventeen days after the blinding; ships lose their way [likely] | - | - | - | - |
| 8 Sep 797 | 797-09-08 (day) | church | ports: eoforwic; regions: northumbria | Archbishop Eanbald II of York receives the pallium | 15 Sep 797 (+7) | 12 Sep 797 (+4) | 14 Sep 797 (+6) | 17 Sep 797 (+9) |
| 28 Oct 797 | 797-10-28 (day) | law | regions: saxony | Capitulare Saxonicum issued at Aachen with Saxons from all parts | - | - | - | - |
| 15 Nov 797 | 797-11 (month) | campaign | regions: saxony | Charles winters in Saxony at Herstelle on the Weser; army billeted across Saxony | - | - | - | - |
| 15 Dec 797 | 797 (season) | embassy | regions: saxony, spain; area: Pannonia | At Herstelle: Avar envoys with rich gifts and an envoy of Alfonso II of Asturias | - | - | - | - |

## 798

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Jan 798 | 798 (season) | raid | regions: spain | Alfonso II sacks Lisbon; sends Charles mail-coats, mules and Moorish captives | - | - | - | - |
| 2 Apr 798 | 798-04-02 (day) | battle | regions: northumbria | Rebels under Wada beaten by King Eardwulf near Whalley; Alric son of Heardberht slain | 9 Apr 798 (+7) | 6 Apr 798 (+4) | 8 Apr 798 (+6) | 10 Apr 798 (+8) |
| 8 Apr 798 | 798-04 (month) | revolt | regions: saxony, schleswig | Saxons beyond the Elbe kill royal envoys, among them Godescalc back from the Danish king | 14 Apr 798 (+6) | 18 Apr 798 (+10) | 18 Apr 798 (+10) | 10 Apr 798 (+2) |
| 1 Jul 798 | 798 (year) | embassy | regions: byzantium, meuse | Irene's envoys Michael and Theophilus seek peace at Aachen; Sisinnius released | - | - | - | - |
| 1 Jul 798 | 798 (year) | raid | regions: spain | Moors and Saracens plunder the Balearic Islands | - | - | - | - |
| 1 Jul 798 | 798 (year) | treaty | regions: byzantium | Arabs raid to Malagina and seize the imperial horses; Irene buys a truce [likely] | - | - | - | - |
| 1 Jul 798 | 798 (year) | burning | ports: lundenwic; regions: england_south | London destroyed by an accidental fire with great loss of life | 6 Jul 798 (+5) | 1 Jul 798 (+0) | 4 Jul 798 (+3) | 11 Jul 798 (+10) |
| 1 Jul 798 | 798 (year) | war | ports: fordwic, sarre, dofras; regions: england_south; realm: kent | Cenwulf ravages Kent, captures Eadberht Praen and maims him; Cuthred made king | 5 Jul 798 (+4) | 3 Jul 798 (+2) | 3 Jul 798 (+2) | 10 Jul 798 (+9) |
| 1 Jul 798 | 798 (year) | church | regions: england_south | Leo III refuses to move the southern archbishopric to London; condemns Eadberht [likely] | 5 Jul 798 (+4) | 1 Jul 798 (+0) | 3 Jul 798 (+2) | 10 Jul 798 (+9) |
| 1 Jul 798 | 798 (year) | synod | regions: northumbria | Synod at Pincanhalh (Finchale) under Eanbald II on Easter and church law | 8 Jul 798 (+7) | 5 Jul 798 (+4) | 7 Jul 798 (+6) | 9 Jul 798 (+8) |
| 1 Jul 798 | 798 (year) | raid | ports: duiblinn; regions: ireland, dalriata | Heathens burn Inis Patraic, take the cattle-tribute and break the shrine of Do-Chonna | 12 Jul 798 (+11) | 8 Jul 798 (+7) | 10 Jul 798 (+9) | 14 Jul 798 (+13) |
| 1 Jul 798 | 798 (year) | death | regions: wales | Caradog, king of Gwynedd, killed by the Saxons | - | - | - | - |
| 15 Jul 798 | 798 (season) | campaign | regions: saxony | From Minden Charles ravages Saxony between Weser and Elbe; 1,600 hostages deported | - | - | - | - |
| 15 Jul 798 | 798 (season) | battle | regions: wendland, saxony | Abodrites under Thrasco crush the Nordalbingians at Bornhöved (Suentana) | 22 Jul 798 (+7) | 26 Jul 798 (+11) | 26 Jul 798 (+11) | 19 Jul 798 (+4) |

## 799

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Jan 799 | 799 (season) | winter | regions: ireland | Great snowfall in Ireland kills many people and cattle | 26 Jan 799 (+11) | 22 Jan 799 (+7) | 24 Jan 799 (+9) | 28 Jan 799 (+13) |
| 1 Apr 799 | 799-04-01 (day) | court | regions: byzantium | Irene rides in a gold chariot through Constantinople on Easter Monday, scattering coin [likely] | - | - | - | - |
| 25 Apr 799 | 799-04-25 (day) | politics | regions: italy | Pope Leo III attacked in Rome during the Greater Litany procession | - | - | - | - |
| 1 Jul 799 | 799 (year) | embassy | regions: italy, saxony | Daniel, envoy of Michael, governor of Sicily, received by Charles in Saxony | - | - | - | - |
| 1 Jul 799 | 799 (year) | embassy | regions: caliphate | A monk from Jerusalem brings relics of the Holy Sepulchre from the patriarch | - | - | - | - |
| 1 Jul 799 | 799 (year) | deportation | regions: saxony | Saxons deported with wives and children; their land shared among Frankish lords | - | - | - | - |
| 1 Jul 799 | 799 (year) | founding | regions: saxony | A great church built and dedicated at Paderborn | - | - | - | - |
| 1 Jul 799 | 799 (year) | revolt | area: Pannonia | The Avars break faith and rise against Frankish rule | - | - | - | - |
| 1 Jul 799 | 799 (year) | death | area: Liburnia | Duke Eric of Friuli killed in an ambush by the townsmen of Tarsatica | - | - | - | - |
| 1 Jul 799 | 799 (year) | politics | regions: spain | The Balearics submit to the Franks after help against raiders; Moorish banners sent | - | - | - | - |
| 1 Jul 799 | 799 (year) | campaign | ports: namnetis; regions: loire; area: Brittany | Count Wido of the Breton march tours Brittany and takes its submission | 11 Jul 799 (+10) | 8 Jul 799 (+7) | 7 Jul 799 (+6) | 16 Jul 799 (+15) |
| 1 Jul 799 | 799 (year) | politics | regions: spain | Azan, governor of Huesca, sends Charles the keys of his city | - | - | - | - |
| 1 Jul 799 | 799 (year) | raid | ports: herio; regions: loire, aquitaine | Pagan ships raid islands off Aquitaine; about 105 raiders killed on the shore [likely] | 12 Jul 799 (+11) | 8 Jul 799 (+7) | 8 Jul 799 (+7) | 17 Jul 799 (+16) |
| 1 Jul 799 | 799 (year) | raid | regions: khazaria, caliphate | Khazars break through Derbent into Armenia and carry off many captives [likely] | - | - | - | - |
| 1 Jul 799 | 799 (year) | revolt | regions: byzantium | Plot of Akameros, lord of the Slavs of Belzetia, for the sons of Constantine V crushed [likely] | - | - | - | - |
| 1 Jul 799 | 799 (year) | storm | regions: northumbria, england_east, england_south | Many ships wrecked with their crews by a violent storm in the British sea | 5 Jul 799 (+4) | 1 Jul 799 (+0) | 3 Jul 799 (+2) | 9 Jul 799 (+8) |
| 1 Jul 799 | 799 (year) | death | regions: northumbria | Duke Moll killed on King Eardwulf's orders | 8 Jul 799 (+7) | 5 Jul 799 (+4) | 7 Jul 799 (+6) | 9 Jul 799 (+8) |
| 1 Jul 799 | 799 (year) | death | ports: eoforwic; regions: northumbria | Osbald, once king, later abbot, dies and is buried at York | 8 Jul 799 (+7) | 5 Jul 799 (+4) | 7 Jul 799 (+6) | 10 Jul 799 (+9) |
| 1 Jul 799 | 799 (year) | death | regions: northumbria | Duke Aldred, slayer of King Aethelred, killed by Torhtmund in revenge | 8 Jul 799 (+7) | 5 Jul 799 (+4) | 7 Jul 799 (+6) | 9 Jul 799 (+8) |
| 1 Jul 799 | 799 (year) | death | regions: mercia_west, england_south | Brorda (Hildegils), a leading Mercian ealdorman, dies | 5 Jul 799 (+4) | 1 Jul 799 (+0) | 3 Jul 799 (+2) | 10 Jul 799 (+9) |
| 1 Jul 799 | 799 (year) | deposition | regions: england_south | Sigeric, king of the East Saxons, leaves his kingdom for Rome [uncertain] | 5 Jul 799 (+4) | 1 Jul 799 (+0) | 3 Jul 799 (+2) | 10 Jul 799 (+9) |
| 1 Jul 799 | 799 (year) | death | ports: dommoc; regions: england_east | Bishop Aelfhun of Dommoc dies at Sudbury and is buried at Dunwich [uncertain] | 5 Jul 799 (+4) | 3 Jul 799 (+2) | 4 Jul 799 (+3) | 9 Jul 799 (+8) |
| 1 Jul 799 | 799 (year) | law | regions: ireland | Law of Patrick proclaimed in Connacht by Gormgal son of Dindathach | 12 Jul 799 (+11) | 8 Jul 799 (+7) | 10 Jul 799 (+9) | 14 Jul 799 (+13) |
| 15 Jul 799 | 799 (season) | court | regions: saxony | Assembly at Lippeham; Charles camps at Paderborn and receives Pope Leo there | - | - | - | - |
| 15 Jul 799 | 799 (season) | embassy | regions: saxony, wendland | Charles the Younger meets the Slavs and the Nordalbingians in the Bardengau | 22 Jul 799 (+7) | 26 Jul 799 (+11) | 26 Jul 799 (+11) | 19 Jul 799 (+4) |
| 1 Sep 799 | 799-09-01 (day) | death | area: Pannonia | Gerold, prefect of Bavaria, falls fighting the Avars [likely] | - | - | - | - |
| 29 Sep 799 | 799-09-29 (day) | storm | regions: ireland | Thunder and lightning on St Michael's Day, called fire from heaven | 10 Oct 799 (+11) | 6 Oct 799 (+7) | 8 Oct 799 (+9) | 12 Oct 799 (+13) |

## 800

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 16 Jan 800 | 800-01-16 (day) | eclipse | (none given) | Eclipse of the moon in the evening [likely] | - | - | - | - |
| 15 Mar 800 | 800-03 (month) | fleet | ports: quentovic, rotomagus; regions: neustria, flanders | Charles inspects the Channel coast, sets up a fleet and posts guards against pirates | 19 Mar 800 (+4) | 18 Mar 800 (+3) | 15 Mar 800 (+0) | 25 Mar 800 (+10) |
| 4 Jun 800 | 800-06-04 (day) | death | regions: loire | Queen Liutgard dies at Tours | 14 Jun 800 (+10) | 11 Jun 800 (+7) | 10 Jun 800 (+6) | 19 Jun 800 (+15) |
| 1 Jul 800 | 800-07 (month) | accession | regions: caliphate | Harun makes Ibrahim ibn al-Aghlab hereditary emir of Ifriqiya [likely] | - | - | - | - |
| 1 Jul 800 | 800 (year) | death | regions: northumbria | Alhmund, son of King Alhred, seized and killed by King Eardwulf's guards | 8 Jul 800 (+7) | 5 Jul 800 (+4) | 7 Jul 800 (+6) | 9 Jul 800 (+8) |
| 1 Jul 800 | 800 (year) | raid | ports: gyrwe; regions: northumbria | Heathens plunder the churches of Hartness and Tynemouth (?) [uncertain] | 9 Jul 800 (+8) | 7 Jul 800 (+6) | 9 Jul 800 (+8) | 10 Jul 800 (+9) |
| 6 Jul 800 | 800-07-06 (day) | weather | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Unusual hard frost in July (again on 9 July), with no harm to the crops | 6 Jul 800 (+0) | 9 Jul 800 (+3) | 6 Jul 800 (+0) | 10 Jul 800 (+4) |
| 1 Aug 800 | 800-08 (month) | assembly | ports: mogontia; regions: rhineland | Assembly at Mainz; Charles sets out for Italy | 5 Aug 800 (+4) | 9 Aug 800 (+8) | 8 Aug 800 (+7) | 9 Aug 800 (+8) |
| 15 Oct 800 | 800 (season) | campaign | regions: italy | Pippin sent to ravage the duchy of Benevento | - | - | - | - |
| 24 Nov 800 | 800-11-24 (day) | court | regions: italy | Charles enters Rome, met by Leo at Mentana the day before | - | - | - | - |
| 23 Dec 800 | 800-12-23 (day) | embassy | regions: italy, caliphate | Leo III clears himself by oath; envoys bring the keys of the Holy Sepulchre [likely] | - | - | - | - |
| 24 Dec 800 | 800-12-24 (day) | storm | regions: northumbria | Christmas Eve gale throws down towns and houses; the sea floods the land; many cattle die | 31 Dec 800 (+7) | 28 Dec 800 (+4) | 30 Dec 800 (+6) | 1 Jan 801 (+8) |
| 25 Dec 800 | 800-12-25 (day) | accession | regions: italy, rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Leo III crowns Charles emperor of the Romans in St Peter's | 25 Dec 800 (+0) | 28 Dec 800 (+3) | 25 Dec 800 (+0) | 29 Dec 800 (+4) |

## 801

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 1 Jan 801 | 801-01 (month) | politics | regions: italy | Leo's attackers condemned for treason; spared and exiled at the pope's plea | - | - | - | - |
| 15 Feb 801 | 801 (season) | epidemic | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Pestilence after a mild winter | 15 Feb 801 (+0) | 18 Feb 801 (+3) | 15 Feb 801 (+0) | 19 Feb 801 (+4) |
| 1 Mar 801 | 801-03 (month) | law | regions: byzantium | Irene remits Constantinople's city taxes and the customs of Abydos and Hieron [likely] | - | - | - | - |
| 25 Apr 801 | 801-04-25 (day) | campaign | regions: italy | Charles leaves Rome after Easter; Pippin's second Benevento campaign | - | - | - | - |
| 30 Apr 801 | 801-04-30 (day) | earthquake | regions: italy | Great earthquake shakes Italy; the roof of St Paul's in Rome falls in | - | - | - | - |
| 1 May 801 | 801 (year) | earthquake | regions: rhineland, meuse | Earth tremors around the Rhine, in Gaul and in Germany | 3 May 801 (+2) | 8 May 801 (+7) | 6 May 801 (+5) | 7 May 801 (+6) |
| 15 Jun 801 | 801 (season) | embassy | regions: italy, caliphate | Envoys of Harun and of Ibrahim of Kairouan land at Pisa; Charles meets them near Vercelli | - | - | - | - |
| 1 Jul 801 | 801 (year) | war | regions: northumbria, mercia_west | Eardwulf of Northumbria makes war on Cenwulf of Mercia; peace sworn on the Gospels | 8 Jul 801 (+7) | 5 Jul 801 (+4) | 7 Jul 801 (+6) | 9 Jul 801 (+8) |
| 1 Jul 801 | 801 (year) | burning | ports: lundenwic; regions: england_south | Bishop Heathoberht of London dies; soon after a great part of London burns | 6 Jul 801 (+5) | 1 Jul 801 (+0) | 4 Jul 801 (+3) | 11 Jul 801 (+10) |
| 1 Jul 801 | 801 (year) | embassy | regions: england_south | Archbishop Aethelheard and Bishop Cyneberht go to Rome over Canterbury's rights [likely] | 5 Jul 801 (+4) | 1 Jul 801 (+0) | 3 Jul 801 (+2) | 10 Jul 801 (+9) |
| 1 Jul 801 | 801 (year) | death | ports: hi; regions: dalriata | Bresal son of Segene, abbot of Iona for 31 years, dies | 13 Jul 801 (+12) | 10 Jul 801 (+9) | 12 Jul 801 (+11) | 14 Jul 801 (+13) |
| 15 Jul 801 | 801 (season) | fleet | regions: italy | Notary Ercanbald sent to Liguria to fit out ships for the elephant | - | - | - | - |
| 15 Jul 801 | 801 (season) | war | regions: spain, provence | Barcelona falls to the Franks after a long siege; Zatun captured | - | - | - | - |
| 15 Jul 801 | 801 (season) | war | regions: italy | Chieti taken and burned; its forts surrender | - | - | - | - |
| 15 Jul 801 | 801 (season) | weather | regions: ireland | A rainy summer in Ireland | 26 Jul 801 (+11) | 22 Jul 801 (+7) | 24 Jul 801 (+9) | 28 Jul 801 (+13) |
| 1 Oct 801 | 801-10 (month) | embassy | regions: italy | Isaac lands at Porto Venere with the elephant; Alpine snow keeps him at Vercelli | - | - | - | - |

## 802

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 18 Jan 802 | 802-01-18 (day) | church | regions: england_south | Leo III confirms Canterbury's authority over all the southern sees [likely] | 22 Jan 802 (+4) | 18 Jan 802 (+0) | 20 Jan 802 (+2) | 27 Jan 802 (+9) |
| 15 Apr 802 | 802 (season) | embassy | regions: byzantium, meuse | Irene's envoy Leo at Aachen; Bishop Jesse and Count Helmgaud sent to Constantinople | - | - | - | - |
| 15 Apr 802 | 802 (season) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Capitulare missorum generale: all free men over 12 swear fidelity to the emperor | 15 Apr 802 (+0) | 18 Apr 802 (+3) | 15 Apr 802 (+0) | 19 Apr 802 (+4) |
| 15 Apr 802 | 802 (season) | fleet | regions: neustria, loire | Missi told to have ships readied on the sea coasts and coast-dwellers ready to muster | 19 Apr 802 (+4) | 18 Apr 802 (+3) | 15 Apr 802 (+0) | 25 Apr 802 (+10) |
| 1 Jul 802 | 802 (year) | embassy | regions: caliphate | Charles sends a second embassy (Radbert) to Harun al-Rashid [likely] | - | - | - | - |
| 1 Jul 802 | 802 (year) | war | regions: italy | Ortona and Lucera surrender; Grimoald later captures Count Winigis at Lucera | - | - | - | - |
| 1 Jul 802 | 802 (year) | death | regions: italy | Patriarch John of Grado killed on the orders of the Venetian doge's son [likely] | - | - | - | - |
| 1 Jul 802 | 802 (year) | battle | area: Pannonia | Counts Cadaloh and Goteram killed by Avars near the castle of Guntio [likely] | - | - | - | - |
| 1 Jul 802 | 802 (year) | accession | regions: england_south; realm: wessex | Beorhtric of Wessex dies, said to be poisoned; Ecgberht back from exile as king | 5 Jul 802 (+4) | 1 Jul 802 (+0) | 3 Jul 802 (+2) | 10 Jul 802 (+9) |
| 1 Jul 802 | 802 (year) | battle | regions: england_south, mercia_west | Battle at Kempsford: ealdormen Aethelmund and Weohstan both killed; Wiltshire wins | 5 Jul 802 (+4) | 1 Jul 802 (+0) | 3 Jul 802 (+2) | 10 Jul 802 (+9) |
| 1 Jul 802 | 802 (year) | politics | regions: england_south | Beorhtric's widow Eadburh flees with treasure to Charles, who gives her a nunnery [likely] | 5 Jul 802 (+4) | 1 Jul 802 (+0) | 3 Jul 802 (+2) | 10 Jul 802 (+9) |
| 1 Jul 802 | 802 (year) | raid | ports: hi; regions: dalriata | Iona burned by heathens | 13 Jul 802 (+12) | 10 Jul 802 (+9) | 12 Jul 802 (+11) | 14 Jul 802 (+13) |
| 15 Jul 802 | 802 (season) | campaign | regions: saxony | A Saxon army ravages the Saxons beyond the Elbe | - | - | - | - |
| 20 Jul 802 | 802-07-20 (day) | embassy | regions: meuse, caliphate | The elephant Abul Abaz reaches Aachen with Harun's gifts | - | - | - | - |
| 1 Oct 802 | 802-10 (month) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Aachen synod and assembly: canons, the Rule and the peoples' laws read and written | 1 Oct 802 (+0) | 4 Oct 802 (+3) | 1 Oct 802 (+0) | 5 Oct 802 (+4) |
| 31 Oct 802 | 802-10-31 (day) | accession | regions: byzantium | Nikephoros, minister of finance, deposes Irene and is crowned emperor | - | - | - | - |
| 1 Dec 802 | 802-12 (month) | politics | regions: caliphate | Harun's pilgrimage: the succession pact for al-Amin and al-Ma'mun hung in the Kaaba [likely] | - | - | - | - |
| 20 Dec 802 | 802-12-20 (day) | eclipse | (none given) | Eclipse of the moon at dawn (date doubtful) [uncertain] | - | - | - | - |

## 803

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Jan 803 | 803 (season) | earthquake | regions: meuse, rhineland | Earthquake around Aachen and nearby lands, followed by a mortality | 17 Jan 803 (+2) | 22 Jan 803 (+7) | 20 Jan 803 (+5) | 21 Jan 803 (+6) |
| 29 Jan 803 | 803-01-29 (day) | death | regions: caliphate | Ja'far the Barmakid executed; the Barmakid ministers fall [likely] | - | - | - | - |
| 15 Apr 803 | 803 (season) | war | regions: byzantium | Nikephoros stops Irene's tribute; Harun's son al-Qasim raids Byzantine Anatolia [likely] | - | - | - | - |
| 25 May 803 | 803-05-25 (day) | death | ports: lindisfarena; regions: northumbria | Higbald, bishop of Lindisfarne, dies; Ecgberht consecrated at Bywell on 11 June | 3 Jun 803 (+9) | 1 Jun 803 (+7) | 3 Jun 803 (+9) | 3 Jun 803 (+9) |
| 1 Jul 803 | 803 (year) | assembly | ports: mogontia; regions: rhineland | Assembly at Mainz; no campaign this year | 5 Jul 803 (+4) | 9 Jul 803 (+8) | 8 Jul 803 (+7) | 9 Jul 803 (+8) |
| 1 Jul 803 | 803 (year) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Capitulary for the missi: no arm-rings or byrnies to merchants; measures to be equal | 1 Jul 803 (+0) | 4 Jul 803 (+3) | 1 Jul 803 (+0) | 5 Jul 803 (+4) |
| 1 Jul 803 | 803 (year) | evidence | ports: skiringssal; regions: vestfold | Skiringssal (Kaupang): earliest tree-ring date, a jetty timber felled in 803 [likely] | 12 Jul 803 (+11) | 15 Jul 803 (+14) | 16 Jul 803 (+15) | 7 Jul 803 (+6) |
| 15 Jul 803 | 803 (season) | treaty | regions: byzantium, italy | Nikephoros's envoys meet Charles at Salz and take home a written peace draft | - | - | - | - |
| 19 Jul 803 | 803-07-19 (day) | revolt | regions: byzantium | Bardanes Tourkos proclaimed emperor by the Anatolian army; the revolt soon collapses [likely] | - | - | - | - |
| 9 Aug 803 | 803-08-09 (day) | death | regions: byzantium | Irene dies in exile on Lesbos [likely] | - | - | - | - |
| 12 Oct 803 | 803-10-12 (day) | synod | regions: england_south | Council of Clofesho abolishes the Lichfield archbishopric; Canterbury's province restored | 16 Oct 803 (+4) | 12 Oct 803 (+0) | 14 Oct 803 (+2) | 21 Oct 803 (+9) |
| 15 Oct 803 | 803 (season) | politics | area: Bavaria | In Bavaria Charles settles Pannonian affairs; Avar and Slav leaders submit [likely] | - | - | - | - |

## 804

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 16 Mar 804 | 804-03-16 (day) | storm | regions: ireland | Storm on St Patrick's Eve kills 1,010 in Corcu Baiscinn; the sea splits the island of Fita | 27 Mar 804 (+11) | 23 Mar 804 (+7) | 25 Mar 804 (+9) | 29 Mar 804 (+13) |
| 19 May 804 | 804-05-19 (day) | death | regions: loire | Alcuin, abbot of St Martin's at Tours, dies | 29 May 804 (+10) | 26 May 804 (+7) | 25 May 804 (+6) | 3 Jun 804 (+15) |
| 1 Jul 804 | 804 (year) | synod | regions: ireland | Synod of the Ui Neill at Dun Cuair; clergy freed from going on hostings | 12 Jul 804 (+11) | 8 Jul 804 (+7) | 10 Jul 804 (+9) | 14 Jul 804 (+13) |
| 1 Jul 804 | 804 (year) | war | regions: ireland | Aed son of Niall ravages Leinster twice in a month; King Finsnechta submits | 12 Jul 804 (+11) | 8 Jul 804 (+7) | 10 Jul 804 (+9) | 14 Jul 804 (+13) |
| 1 Jul 804 | 804 (year) | founding | regions: ireland, dalriata | Kells (Cenannas) given to the community of Colum Cille | 12 Jul 804 (+11) | 8 Jul 804 (+7) | 10 Jul 804 (+9) | 14 Jul 804 (+13) |
| 1 Jul 804 | 804 (year) | church | ports: fordwic; regions: england_south | Lyminge nuns given a plot inside Canterbury as a refuge [likely] | 5 Jul 804 (+4) | 3 Jul 804 (+2) | 3 Jul 804 (+2) | 10 Jul 804 (+9) |
| 15 Jul 804 | 804 (season) | deportation | regions: saxony, wendland | Saxons of Wigmodia and beyond the Elbe deported; Nordalbingia given to the Abodrites | 22 Jul 804 (+7) | 26 Jul 804 (+11) | 26 Jul 804 (+11) | 19 Jul 804 (+4) |
| 15 Jul 804 | 804 (season) | fleet | ports: sliesthorp; regions: schleswig; realm: danes | Godfred of the Danes brings his fleet and all his horse to Sliesthorp | 21 Jul 804 (+6) | 25 Jul 804 (+10) | 25 Jul 804 (+10) | 17 Jul 804 (+2) |
| 20 Jul 804 | 804 (season) | court | regions: saxony, wendland | Charles camps at Hollenstedt; the Abodrite prince Thrasco brings gifts | 27 Jul 804 (+7) | 31 Jul 804 (+11) | 31 Jul 804 (+11) | 24 Jul 804 (+4) |
| 1 Aug 804 | 804-08 (month) | battle | regions: byzantium | Nikephoros beaten by the Arabs at Krasos in Phrygia, barely escaping [likely] | - | - | - | - |
| 25 Dec 804 | 804-12-25 (day) | embassy | regions: neustria | Pope Leo III visits Charles: met at Reims, Christmas at Quierzy, then Aachen | 29 Dec 804 (+4) | 28 Dec 804 (+3) | 25 Dec 804 (+0) | 4 Jan 805 (+10) |

## 805

| fire | date | kind | place | line | Dorestat | Lundenwic | Quentovic | Ripa |
|---|---|---|---|---|---|---|---|---|
| 15 Jan 805 | 805 (season) | treaty | regions: byzantium, caliphate | Prisoner exchange on the Lamos in Cilicia: about 3,700 Muslim captives freed [likely] | - | - | - | - |
| 15 Apr 805 | 805 (season) | politics | area: Pannonia | Avar capcan Theodore given land between Sabaria and Carnuntum; khagan restored | - | - | - | - |
| 12 May 805 | 805-05-12 (day) | death | ports: fordwic; regions: england_south | Archbishop Aethelheard of Canterbury dies; Wulfred chosen to succeed [likely] | 16 May 805 (+4) | 14 May 805 (+2) | 14 May 805 (+2) | 21 May 805 (+9) |
| 1 Jul 805 | 805 (year) | mission | area: Pannonia | Avar khagan baptised as Abraham at the river Fischa [likely] | - | - | - | - |
| 1 Jul 805 | 805 (year) | politics | regions: ireland | Aed son of Niall divides Leinster between two Muiredachs at Dun Cuair | 12 Jul 805 (+11) | 8 Jul 805 (+7) | 10 Jul 805 (+9) | 14 Jul 805 (+13) |
| 1 Jul 805 | 805 (year) | evidence | regions: ladoga | Peterhof hoard: youngest dirham about 804/5, Abbasid silver at the Gulf of Finland [uncertain] | 26 Jul 805 (+25) | 30 Jul 805 (+29) | 30 Jul 805 (+29) | 21 Jul 805 (+20) |
| 1 Jul 805 | 805 (year) | war | regions: byzantium | Peloponnesian Slavs, with Saracen ships, besiege Patras and fail (?) [uncertain] | - | - | - | - |
| 1 Jul 805 | 805 (year) | revolt | regions: spain | Plot of Cordoban notables against al-Hakam crushed; 72 executed (?) [uncertain] | - | - | - | - |
| 1 Jul 805 | 805 (year) | war | area: Pannonia | Krum destroys what is left of the eastern Avar khaganate (?) [uncertain] | - | - | - | - |
| 15 Jul 805 | 805 (season) | campaign | area: Bohemia | Charles the Younger ravages Bohemia; Duke Lecho killed | - | - | - | - |
| 15 Jul 805 | 805 (season) | fleet | regions: saxony | A Frankish force sails up the Elbe to Magdeburg and ravages Genewara [likely] | - | - | - | - |
| 15 Oct 805 | 805 (year) | famine | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Famine in many parts of the realm | 15 Oct 805 (+0) | 18 Oct 805 (+3) | 15 Oct 805 (+0) | 19 Oct 805 (+4) |
| 1 Dec 805 | 805-12 (month) | law | regions: saxony, wendland; area: Bavaria | Thionville: trade with Slavs and Avars only at named border posts; no arms sold | 8 Dec 805 (+7) | 12 Dec 805 (+11) | 12 Dec 805 (+11) | 5 Dec 805 (+4) |
| 1 Dec 805 | 805-12 (month) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Thionville: in this famine each must feed his own; no dear grain, no food sold abroad | 1 Dec 805 (+0) | 4 Dec 805 (+3) | 1 Dec 805 (+0) | 5 Dec 805 (+4) |
| 1 Dec 805 | 805-12 (month) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Thionville: only old, just tolls on bridges, ships and markets; new tolls banned | 1 Dec 805 (+0) | 4 Dec 805 (+3) | 1 Dec 805 (+0) | 5 Dec 805 (+4) |
| 1 Dec 805 | 805-12 (month) | coin | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Thionville: coin to be struck only at the palace; good deniers in use stay valid | 1 Dec 805 (+0) | 4 Dec 805 (+3) | 1 Dec 805 (+0) | 5 Dec 805 (+4) |
| 1 Dec 805 | 805-12 (month) | law | regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence; realm: franks | Thionville: a byrnie for every 12 manses; no arms carried at home; feuds to be settled | 1 Dec 805 (+0) | 4 Dec 805 (+3) | 1 Dec 805 (+0) | 5 Dec 805 (+4) |

100 of the 218 entries could not be given arrival days (no port or port-holding region in the data).


## The ten entries that would most change what a merchant does

The choice is mine, made from each entry's own `effect` line and from whether it touches the twelve ports or their regions (rhine_delta, zeeland, frisia, neustria, england_south, jutland). The title and effect are the data's words; the 'why' is my reading.

1. **Great famine in Burgundy, Francia, Italy, Gothia and Provence; many die** (`793-great-famine`, fire 15 Jan 793, famine)
   - Effect as written: famine across the Frankish lands: grain very dear, deaths, people sell what they have
   - Ports: none named. Regions: italy, rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence. Ends: 794-08.
   - Why: Grain is the trade of the first fifteen months: dear in every Frankish region of the twelve ports, until the 794 harvest (entry 794-drought-harvest ends it).
   - Earliest (rough): Dorestat 15 Jan 793; Lundenwic 18 Jan 793; Quentovic 15 Jan 793; Ripa 19 Jan 793
2. **Synod of Frankfurt fixes grain and bread prices and a new public modius** (`794-06-01-frankfurt-prices`, fire 1 Jun 794, law)
   - Effect as written: law: grain capped per new modius - oats 1 d., barley 2, rye 3, wheat 4; 12 two-pound wheat loaves for 1 d.; royal grain cheaper; lords must keep their people from starving
   - Ports: none named. Regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence.
   - Why: Caps what grain and bread may be sold for in the Frankish ports, in the middle of the famine.
   - Earliest (rough): Dorestat 1 Jun 794; Lundenwic 4 Jun 794; Quentovic 1 Jun 794; Ripa 5 Jun 794
3. **Frankfurt: the new deniers must be taken in every place, town and market** (`794-06-frankfurt-new-denier`, fire 1 Jun 794, coin)
   - Effect as written: coin: the new heavier denier is legal tender everywhere; a free man who refuses it pays 15 solidi
   - Ports: none named. Regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence.
   - Why: The coin in the purse changes: the new denier must be taken everywhere in the Frankish ports, with a fine for refusing.
   - Earliest (rough): Dorestat 1 Jun 794; Lundenwic 4 Jun 794; Quentovic 1 Jun 794; Ripa 5 Jun 794
4. **Charles writes to Offa: each realm's merchants protected; pilgrim-traders pay toll** (`796-spring-offa-letter`, fire 15 Apr 796, treaty)
   - Effect as written: treaty: English merchants protected in the Frankish realm and Frankish ones in Mercia, with appeal to the king; true pilgrims pass free, traders posing as pilgrims pay toll at the usual places; Offa wants the black stones, the Franks English cloaks of the old length
   - Ports: none named. Regions: england_south, neustria, rhine_delta.
   - Why: The one entry that is about merchants crossing between the Frankish ports and Lundenwic: protection both ways, and toll for traders posing as pilgrims.
   - Earliest (rough): Dorestat 15 Apr 796; Lundenwic 15 Apr 796; Quentovic 15 Apr 796; Ripa 20 Apr 796
5. **Eadberht Praen seizes Kent; Archbishop Aethelheard flees Canterbury** (`796-summer-eadberht-kent`, fire 20 Aug 796, war, likely)
   - Effect as written: Kent (sarre, fordwic, dofras) under a rebel king hostile to Mercia
   - Ports: fordwic. Regions: england_south. Ends: 798.
   - Why: Sarre and Fordwic fall under a king hostile to Mercia, so the Kent ports and Lundenwic are on opposite sides until 798 (then 798-kent-ravaged lays Kent waste).
   - Earliest (rough): Dorestat 24 Aug 796; Lundenwic 22 Aug 796; Quentovic 22 Aug 796; Ripa 29 Aug 796
6. **London destroyed by an accidental fire with great loss of life** (`798-london-fire-798`, fire 1 Jul 798, burning)
   - Effect as written: fire at lundenwic: quays and stores burned; goods scarce and dear
   - Ports: lundenwic. Regions: england_south.
   - Why: Lundenwic's quays and stores burn: goods scarce and dear there. It burns again in 801 (801-london-fire-801).
   - Earliest (rough): Dorestat 6 Jul 798; Lundenwic 1 Jul 798; Quentovic 4 Jul 798; Ripa 11 Jul 798
7. **Charles inspects the Channel coast, sets up a fleet and posts guards against pirates** (`800-03-channel-coastguard`, fire 15 Mar 800, fleet)
   - Effect as written: coastguard posts and a royal fleet on the Channel coast against pirates
   - Ports: quentovic, rotomagus. Regions: neustria, flanders.
   - Why: Names Quentovic: a royal fleet and guard posts on the Channel coast against pirates.
   - Earliest (rough): Dorestat 19 Mar 800; Lundenwic 18 Mar 800; Quentovic 15 Mar 800; Ripa 25 Mar 800
8. **Christmas Eve gale throws down towns and houses; the sea floods the land; many cattle die** (`800-12-24-christmas-eve-gale`, fire 24 Dec 800, storm)
   - Effect as written: storm surge: coastal flooding, buildings down, cattle lost; North Sea harbours damaged
   - Ports: none named. Regions: northumbria.
   - Why: Storm surge: North Sea harbours damaged, cattle lost.
   - Earliest (rough): Dorestat 31 Dec 800; Lundenwic 28 Dec 800; Quentovic 30 Dec 800; Ripa 1 Jan 801
9. **Godfred of the Danes brings his fleet and all his horse to Sliesthorp** (`804-summer-godfred-sliesthorp`, fire 15 Jul 804, fleet)
   - Effect as written: Danish fleet massed at the Schlei; border tense
   - Ports: sliesthorp. Regions: schleswig.
   - Why: The Danish fleet massed at the Schlei and the border tense; Ripa is the one Danish port of the twelve.
   - Earliest (rough): Dorestat 21 Jul 804; Lundenwic 25 Jul 804; Quentovic 25 Jul 804; Ripa 17 Jul 804
10. **Thionville: in this famine each must feed his own; no dear grain, no food sold abroad** (`805-12-thionville-famine`, fire 1 Dec 805, law)
   - Effect as written: famine law: no food sold outside the empire; grain not to be sold too dear
   - Ports: none named. Regions: rhine_delta, zeeland, frisia, rhineland, meuse, flanders, neustria, loire, aquitaine, saxony, provence.
   - Why: Famine law: no food sold outside the empire and grain not to be sold dear, so no grain run from the Frankish ports to Lundenwic, Kent or Ripa. Comes with the 805 famine (805-famine-805, to 807) and the tolls, coin and border-post laws of the same day.
   - Earliest (rough): Dorestat 1 Dec 805; Lundenwic 4 Dec 805; Quentovic 1 Dec 805; Ripa 5 Dec 805

Just outside the ten: `793-06-08-lindisfarena-raid`, `793-autumn-saxon-revolt` (Frisian coast east of the Lauwers hostile), `793-offa-heavy-penny`, `794-drought-harvest`, `796-07-29-offa-dies`, `799-british-sea-storm`, `802-spring-coast-ships-802`, `803-arms-to-merchants-803`, `805-12-thionville-tolls`, `805-12-thionville-coin`.

## Things to check

- The task brief says a new game begins 1 May 793; the chronicle's `start` field says 793-06-21. Entries fired before the start are backstory, and which day counts changes which of the 793 entries are news.
- 100 of 218 entries have no arrival days, because their regions hold no port in `ports_793.json` or they have no region at all.
- Realm-wide entries arrive at Dorestat on day 0 only because Dorestat lies in a named region; the data does not say where the word starts (Frankfurt, Aachen, Thionville are not ports).
