# Later ports: the Channel, Seine, Loire and Biscay coasts of Francia

Written 6 Oct 2026 for the question "all visitable locations, 793 to 1066". The port table
(`Assets/Kapman/Game/Data/ports_793.json`) already has six places on this coast: `quentovic`, `rotomagus` (Rouen),
`parisius`, `namnetis` (Nantes), `herio` (Noirmoutier) and `burdigala` (Bordeaux). This file proposes 25 more, ranked.
Nothing here has been put into the game's tables: the rows are ready to paste, and the spine decides.

`(?)` marks a thing I could not check against a page this session, or that the sources themselves leave open.

## How to read a row

The rows follow `ports_793.json` exactly: `id, name, modern, lat, lon, realm, county, region, kind, landing, size,
from, to, note`. Three things had to be decided, and each is a question for the spine:

- **`realm`** holds the ruler in the first year the place trades (`from`, or 793 where `from` is 0), because the table
  has one realm to a port. Who ruled later is under "Who ruled" in each entry. Two realm ids are used that the table
  does not have yet: **`bretons`** and **`normans`** (see Needs at the bottom).
- **`region`** uses the table's own keys where one fits: `neustria` (the Channel and the Seine, as Quentovic and
  Rouen), `loire`, `aquitaine`. Breton places use a new key, **`brittany`** (Needs).
- **`county`** is left empty: I did not match these places to `Build/Guide/lands_793.json`.
- `kind` and `landing` use only the words the table already uses (emporium, town, market, monastery, royal seat, fort,
  village; beach, jetty, quay, riverbank). `size` is 1 to 5 as in the table, set at the place's best years.

## The ranking

| Rank | Place (name then) | Modern | Years | One line |
|---|---|---|---|---|
| must | Bononia | Boulogne-sur-Mer | all | The lighthouse and the short crossing; the great army ships here in 892 |
| must | Monasteriolum | Montreuil-sur-Mer | 898 on | Quentovic's heir on the Canche; the king's one sea port after 980 |
| must | Sanctus Walaricus | Saint-Valery-sur-Somme | all | The Somme mouth; William's fleet waits here for a wind in 1066 |
| must | Fiscannum | Fécamp | all | The dukes' own house and abbey; herring; lands in Sussex |
| must | Cadomus | Caen | 1025 on | The new town of the last forty years: market, toll, port, stone |
| must | Aletum | Saint-Servan (Saint-Malo) | all | The Bretons' north harbour and a bishop's seat; the great tides |
| must | Venetis | Vannes | all | The Breton march city on the Morbihan gulf; Nominoe's seat |
| must | Andecavis | Angers | all | Wine and salt on the Maine; the Northmen's market of 873 |
| must | Turonis | Tours | all | Saint Martin: pilgrims, books, a mint, the best wine customers on the Loire |
| must | Aurelianis | Orléans | all | The top of the Loire trade and the short road to Paris |
| must | Santonas | Saintes | all | The Charente city: salt of the marshes, wine, and a river the Northmen used |
| good | Witsand | Wissant | 933 on | The beach crossing to Dover in the tenth and eleventh centuries |
| good | Ambianis | Amiens | all | A king's toll of 779 on the Somme; Corbie's landing |
| good | Auga | Eu and Le Tréport | 925 on | A Norman fort stormed in 925; the Bresle mouth |
| good | Deppa | Dieppe | 1030 on | A herring haven first named in 1030 |
| good | Harofloth | Harfleur | 1006 on | The Seine mouth: wait for the tide, take a pilot |
| good | Gemeticum | Jumièges | all | The great Seine abbey, burned 841, rebuilt 1040 to 1067 |
| good | Pistis | Pîtres and Pont-de-l'Arche | 862 on | The king's fortified bridge that bars the Seine |
| good | Baiocas | Bayeux (Port-en-Bessin) | all | The bishop's city where Danish was still spoken in the 940s |
| good | Barbefloth | Barfleur | 1000 on (?) | The Norman crossing port of the Cotentin |
| good | Mons Sancti Michaelis | Mont-Saint-Michel | all | Pilgrims over the sands; a deadly tide |
| good | Roton | Redon | 832 on | The Bretons' abbey where two rivers meet; its charters are the evidence for the salt |
| good | Wenran | Guérande and Batz | 850 on | The salt pans themselves |
| leave out | Rupella | La Rochelle | 1000 on (?) | Salt marsh and a hamlet until the 1130s |
| leave out | Lapurdum | Bayonne | all | Too far, and almost nothing certain before 1056 |

Places looked at and not written up are listed near the end, with the reason.

## What changes on this coast, 793 to 1066 (for `eras`)

- **793 to 840.** One Frankish kingdom. The king's toll is taken at named gates (Rouen, Quentovic, Amiens, Dorestad,
  Maastricht in the charter of 779). Abbeys run the river trade: their boats go toll-free by royal charter, carrying
  wine down and salt up. The Bretons west of the march pay tribute when made to.
- **841 to 911.** The raids. Rouen and Jumièges burn in 841, Nantes in 843, Saintes in 845, Tours in 853, Orléans in
  856 and 865, Angers is held 872 to 873. Monks carry their saints inland. Kings answer with fortified bridges
  (Pîtres, 862) and a coinage reform (864). The Bretons make a kingdom (Nominoe, Erispoe, Salomon).
- **911 to 960.** Rouen and its country go to Rollo (911), the Bessin in 924, the Cotentin and Avranchin in 933.
  Brittany is overrun by Northmen 919 to 936, then won back by Alan Barbetorte (Nantes 937, Trans 939). Quentovic
  fades; Montreuil, Wissant and Boulogne take the crossing.
- **960 to 1066.** Counts and dukes, not kings, own the tolls. Anjou (Fulk Nerra, Geoffrey Martel) pushes down the
  Loire to Tours (1044) and south to Saintes. Normandy's dukes build abbeys and new towns (Fécamp 1001, Caen from the
  1020s) and hold lands across the Channel before 1066. The herring fishery and the salt trade grow.

---

# Must have

## 1. Bononia (Boulogne-sur-Mer)

- **Name then:** Bononia (Latin of the annals); the English wrote Bunne (?).
- **Modern place:** Boulogne-sur-Mer, Pas-de-Calais. **Lat, lon:** 50.725, 1.605.
- **Active:** the whole game. A Roman port city still walled in 793; never abandoned.
- **Kind:** a walled upper town on a hill over the tidal mouth of the Liane, with the old Roman lighthouse on the cliff
  to the north.
- **Who ruled:** the Frankish king to the later ninth century; then the counts of Flanders (Baldwin II from about 896
  (?), Arnulf I), with a cadet as count of Boulogne (Adalolf, 918 to 933). From the early eleventh century its own
  counts: Eustace I (about 1024 to 1047) and Eustace II (1047 to about 1087), who fought at Hastings.
- **Traded:** the crossing itself (people, horses, letters); fish; whatever goes over to Dover and Kent. Not a great
  market: a ferry port with a wall.
- **How a boat comes in:** into the Liane on the flood and onto the strand under the hill. By night, after 811, there
  is a fire on the lighthouse (how long it was kept burning after Charles is not known (?)).
- **Why go:** the shortest safe crossing to the English; the one light on this coast; news of both shores.
- **Evidence:** the Royal Frankish Annals, 811: Charles came to Boulogne to see the fleet he had ordered built the
  year before, "restored the lighthouse set up there long ago to guide the course of sailors, and had a fire lit on
  top of it at night". The Miracles of St Wandrille (Grippo's storm) see the same light about 860 (in
  `dossiers/quentovic.md`). The Anglo-Saxon Chronicle, 892 (893 in some texts): the great army came west to Boulogne
  and was shipped there, crossing in one passage "horses and all" with 250 ships to the Limen mouth. Flodoard, 936:
  Louis, sent home by his uncle Athelstan, lands on the sands at Boulogne and is met by Hugh the Great.
- **Game notes:** in 793 there is no light: it comes in 811 (already in the Quentovic dossier's "do not" list). A
  card for 892: the harbour full of the army's ships, and nothing to buy.
- **Sources:** ARF 811 (Scholz, Carolingian Chronicles; Latin at dmgh.de); ASC 892 (Giles 1847 is public domain);
  Flodoard, Annals 936; https://en.wikipedia.org/wiki/Boulogne-sur-Mer ; https://en.wikipedia.org/wiki/Tour_d%27Ordre ;
  https://en.wikipedia.org/wiki/Count_of_Boulogne . The two Wikipedia pages on the town that I read this session say
  nothing for 793 to 1066: the facts above are from the annals as I know them, so the counts' dates carry (?).

```json
{"id": "bononia", "name": "Bononia", "modern": "Boulogne-sur-Mer", "lat": 50.725, "lon": 1.605, "realm": "franks", "county": "", "region": "neustria", "kind": "town", "landing": "beach", "size": 3, "from": 0, "to": 0, "note": "A walled Roman town over the Liane mouth and the short crossing to the English; from 811 a fire burns on the old lighthouse."}
```

## 2. Monasteriolum (Montreuil-sur-Mer)

- **Name then:** Monasteriolum, "the little minster"; Monsteriolum about 1000.
- **Modern place:** Montreuil, Pas-de-Calais. **Lat, lon:** 50.464, 1.763.
- **Active:** first named 898; a port until the Canche silted, long after 1066.
- **Kind:** a fortified hill town above the Canche, three kilometres upstream of dying Quentovic, with a count's
  castle, an abbey of Breton exiles and a landing below.
- **Who ruled:** Count Helgaud (dead by 926); Herluin his son (killed 945); Roger. Taken by Arnulf of Flanders in 939
  and 948. From 980 Hugh Capet's, and from 987 the king's: by 988 it is called his only sea port.
- **Traded:** what Quentovic traded, in smaller boats: wine and salt outward, English wool, cloth, lead and tin
  inward (?); toll and, later, a mint. Its own cloth is a later fame.
- **How a boat comes in:** up the Canche on the tide past Quentovic's empty bank to the foot of the hill; she lies on
  the riverbank.
- **Why go:** after about 900 this is where the Canche toll, the buyers and the news of England are. The Quentovic
  panel should send the player here.
- **Evidence:** Annals of St Vaast, 898 (first naming). Flodoard, 929 (the siege of "the castle of Herluin, son of
  Count Hilgaud"), 939 (Arnulf takes it by a traitor and sends Herluin's wife and sons over the sea to Athelstan;
  Herluin retakes it with Northmen) and 948. Monks of Landévennec in Brittany, fleeing the Northmen after 913, settle
  here with St Winwaloe's relics about 926 (the abbey of Saint-Walloy, later Saint-Saulve).
- **Game notes:** the Breton monks tie this place to Brittany: a passenger or a letter for Alet or Vannes.
- **Sources:** `Design/content/dossiers/quentovic.md` (its years 898 to 988 and references FL, WFM);
  https://fr.wikipedia.org/wiki/Montreuil-sur-Mer (read this session).

```json
{"id": "monasteriolum", "name": "Monasteriolum", "modern": "Montreuil-sur-Mer", "lat": 50.464, "lon": 1.763, "realm": "franks", "county": "", "region": "neustria", "kind": "town", "landing": "riverbank", "size": 3, "from": 898, "to": 0, "note": "A count's castle on the Canche above dead Quentovic; from 980 Hugh Capet's, and the king's one sea port."}
```

## 3. Sanctus Walaricus (Saint-Valery-sur-Somme)

- **Name then:** Leuconaus (the old estate); the abbey and haven of Saint Walaric.
- **Modern place:** Saint-Valery-sur-Somme, Somme. **Lat, lon:** 50.187, 1.631.
- **Active:** the whole game as an abbey with a landing; famous for one month in 1066.
- **Kind:** an abbey on a headland at the south side of the Somme's wide tidal bay.
- **Who ruled:** the Frankish king; then the counts of Ponthieu (Enguerrand I, about 1026 to 1045; Guy I, 1053 to
  1100, who seized Harold of Wessex on this coast in 1064).
- **Traded:** fish and salt from the bay; a ferry over the estuary on the coast road between Rouen and Boulogne;
  pilgrims to the saint until the eleventh century.
- **How a boat comes in:** over the sands on the flood only; the bay dries for miles at low water. She takes the
  ground under the headland.
- **Why go:** the way into the Somme for Amiens and Corbie; a safe bay in a south-west wind, and a trap in a north
  one. In September 1066, the greatest fleet of the age.
- **Evidence:** the Life of St Walaric (the abbey, about 611 (?)). William of Poitiers and the Carmen de Hastingae
  Proelio: William's fleet moved from the Dives to Saint-Valery about 12 September 1066, lay windbound, the saint's
  body was carried out in procession, and the fleet sailed on the evening of 27 September. The saint's relics were
  taken to Saint-Bertin by Arnulf of Flanders and brought back by Hugh Capet in 981 (?).
- **Game notes:** the last card of the game can be here: the wind turns south.
- **Sources:** https://fr.wikipedia.org/wiki/Saint-Valery-sur-Somme (read this session: Leuconaus, Ponthieu, the
  fleet "at the end of September 1066"); William of Poitiers, Gesta Guillelmi; https://en.wikipedia.org/wiki/Guy_I,_Count_of_Ponthieu .

```json
{"id": "walaricus", "name": "Sanctus Walaricus", "modern": "Saint-Valery-sur-Somme", "lat": 50.187, "lon": 1.631, "realm": "franks", "county": "", "region": "neustria", "kind": "monastery", "landing": "beach", "size": 2, "from": 0, "to": 0, "note": "Saint Walaric's abbey on the headland of the Somme bay, which dries at low water; Duke William's fleet waits here for a wind in September 1066."}
```

## 4. Fiscannum (Fécamp)

- **Name then:** Fiscannum (875, of the river); Fiscannus (990).
- **Modern place:** Fécamp, Seine-Maritime. **Lat, lon:** 49.758, 0.374.
- **Active:** the whole game, in three lives: a nunnery (from about 658) until the Northmen destroyed it in the
  ninth century; the Norman counts' own residence from about 930; a great abbey from 990 and 1001.
- **Kind:** a gap in the chalk cliffs with a small river, a shingle bar and a haven behind it; a ducal hall and an
  abbey church side by side.
- **Who ruled:** the Frankish king; from 911 the counts of Rouen (the later dukes). William Longsword kept house
  here; Richard I was born here about 933 and died here in 996; Richard I and Richard II are buried here.
- **Traded:** herring and other salt fish; the abbey's wine and corn coming in; pilgrims to the relic of the Precious
  Blood (the story is written down only in the twelfth century (?)). Through the abbey, a standing link to Sussex.
- **How a boat comes in:** between the cliffs onto the shingle, or over the bar into the haven on the tide. An open
  coast: no place to be caught by a north-west wind.
- **Why go:** the duke is often here, and so is everyone who wants something from him. After 1017 the abbey's men
  cross to their own English harbours.
- **Evidence:** Dudo of Saint-Quentin (the dukes at Fécamp). The church dedicated 990; William of Volpiano brought
  from Dijon as abbot in 1001 by Richard II. The abbey held Rameslie in Sussex (Rye and Winchelsea) by grant of Cnut
  about 1017, and Steyning by gift of Edward the Confessor; Harold took Steyning back, and William swore to return it.
  William kept Easter 1067 here and showed the English spoils (William of Poitiers). Carolingian coins and chapel
  footings were dug up by the ducal palace.
- **Game notes:** a late-game unlock: a Fécamp charter lets the player land toll-free at Rye or Steyning (?).
- **Sources:** https://fr.wikipedia.org/wiki/F%C3%A9camp (read this session);
  https://en.wikipedia.org/wiki/F%C3%A9camp_Abbey ; https://en.wikipedia.org/wiki/Steyning .

```json
{"id": "fiscannum", "name": "Fiscannum", "modern": "Fécamp", "lat": 49.758, "lon": 0.374, "realm": "franks", "county": "", "region": "neustria", "kind": "monastery", "landing": "beach", "size": 3, "from": 0, "to": 0, "note": "A haven in a gap of the chalk cliffs: a nunnery until the Northmen came, then the dukes' own hall, and from 1001 their great abbey; herring."}
```

## 5. Cadomus (Caen)

- **Name then:** Cathim (the first charter), Cadon, Cadomus.
- **Modern place:** Caen, Calvados. **Lat, lon:** 49.183, -0.370.
- **Active:** from about 1025. There were parishes here before, but no town a merchant would name.
- **Kind:** a new town where the Odon meets the tidal Orne, fifteen kilometres from the sea: by 1025 it has churches,
  a market, a toll and a port; from about 1060 the duke's castle on the rock, and two abbeys building.
- **Who ruled:** the dukes of Normandy throughout: Richard II, Robert I, William.
- **Traded:** corn and stock of the plain; salt; the duke's toll. At the very end of the game, building stone: the
  pale Caen limestone goes to England in the years just after 1066.
- **How a boat comes in:** over the bar at the Orne mouth (Ouistreham) and up the river on the flood to the quay by
  the Odon.
- **Why go:** the duke's works draw masons, smiths, carters and silver. The fastest-growing place on this coast from
  1050 to 1066.
- **Evidence:** a charter of Duke Richard II for Fécamp, about 1025: "the town called Cathim on the river Orne, with
  its churches, vineyards, meadows, mills, market, toll and port". The castle about 1060; the abbey of the Trinity
  dedicated 18 June 1066; Saint-Étienne begun about 1063 with Lanfranc as abbot. The Truce of God sworn near Caen
  after the battle of Val-ès-Dunes, 1047.
- **Game notes:** must have only for the last heir. Before 1025 the Orne mouth is an empty river.
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_de_Caen (read this session: the charter's words);
  https://en.wikipedia.org/wiki/Caen ; https://en.wikipedia.org/wiki/Caen_stone .

```json
{"id": "cadomus", "name": "Cadomus", "modern": "Caen", "lat": 49.183, "lon": -0.370, "realm": "normans", "county": "", "region": "neustria", "kind": "town", "landing": "quay", "size": 3, "from": 1025, "to": 0, "note": "A new Norman town on the tidal Orne: market, toll and port by 1025; Duke William's castle from about 1060."}
```

## 6. Aletum (Alet, at Saint-Malo)

- **Name then:** Aleto, Alet; civitas Aletis.
- **Modern place:** the Cité d'Alet, Saint-Servan, in Saint-Malo, Ille-et-Vilaine. **Lat, lon:** 48.636, -2.027.
- **Active:** the whole game. A late Roman fort town that became a Breton bishop's seat. The see moved to the rock of
  Saint-Malo only in the twelfth century.
- **Kind:** a walled promontory at the mouth of the Rance with a sheltered cove under it (the Solidor anchorage) and
  a cathedral inside the old wall.
- **Who ruled:** Breton lords and the bishop; under Nominoe (831 to 851), Erispoe and Salomon the Breton kingdom;
  Northmen in the land 919 to 936; then the Breton dukes (Alan Barbetorte, the counts of Rennes: Conan I, Geoffrey I,
  Alan III, Conan II to 1066).
- **Traded:** Breton corn, hides, honey and wax out; wine and salt in; churchmen and pilgrims crossing to Cornwall,
  Wales and Ireland (?). Small trade, but the only town harbour on Brittany's north coast.
- **How a boat comes in:** with care. The tide here rises twelve metres and more, the highest on the coast, and the
  approach is all rocks. She lies in the cove on the sand and is dry for half of every tide.
- **Why go:** the way into the Rance and Brittany's north; a bishop who buys wine and wax; the place to learn the
  Breton coast from a pilot.
- **Evidence:** the bishops of Alet in the Redon cartulary (ninth century); the Northmen plundered and burned the town
  and its church more than once in the ninth and tenth centuries; the tenth-century cathedral's whole ground plan was
  dug in 1972 and still stands as a ruin. The saint's relics were carried east to Paris in the tenth century (?).
- **Game notes:** the written evidence for trade here is thin, and the entry says so; the harbour and the see are
  certain. The tide is the gameplay: a wait card every time.
- **Sources:** https://fr.wikipedia.org/wiki/Cit%C3%A9_d%27Alet (read this session);
  https://en.wikipedia.org/wiki/Aleth ; https://fr.wikipedia.org/wiki/Cath%C3%A9drale_Saint-Pierre_d%27Aleth .

```json
{"id": "aletum", "name": "Aletum", "modern": "Alet, Saint-Servan (Saint-Malo)", "lat": 48.636, "lon": -2.027, "realm": "bretons", "county": "", "region": "brittany", "kind": "town", "landing": "beach", "size": 2, "from": 0, "to": 0, "note": "An old walled headland at the mouth of the Rance, a Breton bishop's seat; the highest tides of the coast leave a boat dry on the sand."}
```

## 7. Venetis (Vannes)

- **Name then:** Venetis; Breton Gwened.
- **Modern place:** Vannes, Morbihan. **Lat, lon:** 47.658, -2.760.
- **Active:** the whole game.
- **Kind:** a small walled Roman city at the head of a creek of the Morbihan gulf: a bishop, a count, and the gate
  between Frankish and Breton country.
- **Who ruled:** the Franks from Pippin's conquest of 753 (the Breton march). Nominoe is count here by about 819 and
  Louis the Pious's man for all Brittany from 831; then the Breton kings; Alan the Great, count of Vannes (died 907).
  Northmen hold the land 919 to 936. Then the Breton dukes.
- **Traded:** salt of the gulf and of Guérande; corn, honey, wax, hides and horses of the Breton country (?); wine
  coming in from the Loire.
- **How a boat comes in:** through the narrow mouth of the gulf, where the tide runs like a river, then among the
  islands to the creek under the walls. She lies on the mud at low water.
- **Why go:** the Bretons buy wine and sell cheap. The count's court is where a foreign merchant gets leave to trade
  in Brittany at all.
- **Evidence:** the Royal Frankish Annals, 818: Louis the Pious comes to Vannes with his army against the Breton
  Morman. The Annals of St Bertin for Nominoe and his sons. The cathedral burned and Bishop Bili killed by Northmen
  about 919 (?). The Chronicle of Nantes and Flodoard for the return of Alan Barbetorte (936, Nantes 937, Trans 939).
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_de_Vannes (read this session);
  https://en.wikipedia.org/wiki/Nominoe ; `Design/content/realms/francia.md` (the Breton dates).

```json
{"id": "venetis", "name": "Venetis", "modern": "Vannes", "lat": 47.658, "lon": -2.760, "realm": "franks", "county": "", "region": "brittany", "kind": "town", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "A walled city at the head of the Morbihan gulf, the Franks' march town and then the Bretons' own; the tide races in the gulf mouth."}
```

## 8. Andecavis (Angers)

- **Name then:** Andecavis (civitas Andecavorum).
- **Modern place:** Angers, Maine-et-Loire. **Lat, lon:** 47.473, -0.555.
- **Active:** the whole game.
- **Kind:** a walled city on a rock over the Maine, ten kilometres above where it joins the Loire; a bishop, rich
  abbeys (Saint-Aubin, Saint-Serge), and from the tenth century the seat of the counts of Anjou.
- **Who ruled:** the Frankish king's counts; Robert the Strong holds the march and dies at Brissarthe in 866. Fulk the
  Red is viscount about 898 and count by 929; then Fulk the Good, Geoffrey Greymantle (960 to 987), Fulk Nerra (987
  to 1040) and Geoffrey Martel (1040 to 1060).
- **Traded:** wine of Anjou downriver; salt upriver from the Bay (Saint-Aubin has its own salt priory at Saillé by
  about 950); corn; roofing slate (the quarries are attested later (?)).
- **How a boat comes in:** up the Loire from Nantes, into the Maine, and to the bank under the city. Fulk Nerra built
  the first stone bridge here.
- **Why go:** the first great wine market above Nantes, and three rivers (Mayenne, Sarthe, Loir) meeting just above it.
- **Evidence:** the Annals of St Bertin, 873: Charles the Bald and Salomon of Brittany besiege the Northmen who have
  held Angers for a year; the Bretons turn the Maine out of its bed to strand their ships; the Northmen give
  hostages, and ask to stay on an island in the Loire until February and to hold a market there. Regino tells the
  digging. Earlier raids in the 850s (?).
- **Game notes:** the island market of 873 is already a story in `realms/francia.md` (number 6). This row gives it a
  port to happen at.
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_d%27Angers (read this session); Annals of St Bertin 873
  (Nelson's translation is in copyright: do not quote); `Design/content/economy/wine_salt.md` (Saillé).

```json
{"id": "andecavis", "name": "Andecavis", "modern": "Angers", "lat": 47.473, "lon": -0.555, "realm": "franks", "county": "", "region": "loire", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "A walled city on the Maine just above the Loire: wine going down, salt coming up; held by Northmen for a year until 873."}
```

## 9. Turonis (Tours)

- **Name then:** Turonis; the abbey quarter is Saint Martin's town, later Châteauneuf.
- **Modern place:** Tours, Indre-et-Loire. **Lat, lon:** 47.394, 0.689.
- **Active:** the whole game.
- **Kind:** two places a short walk apart on the south bank of the Loire: the old walled city with the archbishop,
  and to the west the abbey of Saint Martin with its own town of merchants and pilgrims, walled by 918.
- **Who ruled:** the king, through the lay abbots of Saint Martin (the Robertians: Robert the Strong, Odo, Hugh the
  Great). The counts of Blois hold the city from the 940s (?); Geoffrey Martel of Anjou takes it in 1044.
- **Traded:** pilgrims and what they need; wine; salt and fish up from Nantes; wax, oil and incense for the
  churches; books (the great Tours Bibles were written here from Alcuin's time to the raid of 853). The abbey strikes
  its own pennies: the later denier tournois.
- **How a boat comes in:** about two hundred kilometres up the Loire from Nantes, under sail on the west wind and by
  tow against a shifting, sandy stream. She lies on the bank by the abbey town.
- **Why go:** the richest buyers of wine, wax and fine goods on the river, and the holiest place in Gaul: in 813 a
  council set a pilgrimage to Tours beside one to Rome.
- **Evidence:** Alcuin is abbot of Saint Martin 796 to 804. The Annals of St Bertin, 853: the Northmen from Nantes
  burn Saint Martin's on 8 November; the saint's body has been carried away in time (to Cormery, later as far as
  Auxerre, home again in 885 (?)). Tours burns again in 903. The abbey town's wall was finished in 918. Marmoutier
  across the river is refounded in the 980s (?). The bridge of Count Odo II, about 1034.
- **Game notes:** this is where a book commission ends or begins. In 793 Alcuin is not here yet (he comes in 796).
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_de_Tours (read this session);
  https://en.wikipedia.org/wiki/Basilica_of_Saint_Martin,_Tours ; `Design/content/dossiers/namnetis.md` (Alcuin and
  Count Wido).

```json
{"id": "turonis", "name": "Turonis", "modern": "Tours", "lat": 47.394, "lon": 0.689, "realm": "franks", "county": "", "region": "loire", "kind": "town", "landing": "riverbank", "size": 4, "from": 0, "to": 0, "note": "The archbishop's city and Saint Martin's abbey town on the Loire: pilgrims, books, a mint, and buyers for wine, wax and salt."}
```

## 10. Aurelianis (Orléans)

- **Name then:** Aurelianis.
- **Modern place:** Orléans, Loiret. **Lat, lon:** 47.902, 1.909.
- **Active:** the whole game.
- **Kind:** a walled Roman city on the north bank at the Loire's most northerly bend: a bishop, the abbey of Saint
  Aignan, a royal mint, and a king's city under the Capetians.
- **Who ruled:** the king throughout, which is rare on this river: Charlemagne's bishop Theodulf (about 798 to 818);
  Charles the Bald, crowned here in 848; then the Robertians. Robert II is born here (972) and crowned here (987).
- **Traded:** the wine of Orléans, the best known in the north of the kingdom; corn; salt come up from the Bay; and
  everything that changes from boat to cart for Paris, a hundred and twenty kilometres by road.
- **How a boat comes in:** this is as far up the Loire as most trade goes. A long haul from Tours against the
  stream; low water in summer strands boats on the sandbanks for weeks.
- **Why go:** to sell salt at its dearest and buy wine at its cheapest, and to reach the Seine without sailing round
  Brittany.
- **Evidence:** the Edict of Pîtres (864) names Orléans among the ten mints of the kingdom. The Annals of St Bertin:
  in 854 the bishops of Orléans and Chartres make ready ships and men, and the Northmen turn back; in 856 the city is
  taken and plundered; in 865 it is burned. Abbey boats run toll-free on the Loire by royal charter (Fleury, Saint
  Aignan, Saint Martin). The fire of 989; the burning of the heretic canons in 1022. Raoul Glaber calls it the chief
  seat of the kings.
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_d%27Orl%C3%A9ans (read this session);
  https://en.wikipedia.org/wiki/Edict_of_P%C3%AEtres ; `Design/content/economy/wine_salt.md` (the Loire salt boats).

```json
{"id": "aurelianis", "name": "Aurelianis", "modern": "Orléans", "lat": 47.902, "lon": 1.909, "realm": "franks", "county": "", "region": "loire", "kind": "town", "landing": "riverbank", "size": 4, "from": 0, "to": 0, "note": "The king's city at the top of the Loire trade: wine, a mint, and the short road to Paris."}
```

## 11. Santonas (Saintes)

- **Name then:** Santonas (Mediolanum Santonum of the Romans).
- **Modern place:** Saintes, Charente-Maritime. **Lat, lon:** 45.746, -0.633.
- **Active:** the whole game.
- **Kind:** a shrunken Roman city inside its late wall on the Charente, with a Roman arch still standing on the
  bridge and an amphitheatre in the fields; a bishop; the shrine of Saint Eutropius outside the wall.
- **Who ruled:** the kings and sub-kings of Aquitaine (Louis the Pious as king from 781, Pippin I, Pippin II,
  Charles the Bald); the counts of Poitou as dukes of Aquitaine from the tenth century; the counts of Anjou hold it of
  them (Fulk Nerra, then Geoffrey Martel until about 1062), then the duke again.
- **Traded:** salt from the sea marshes of Saintonge; wine; corn. The river is the road: Angoulême is upstream.
- **How a boat comes in:** in past the isle of Oléron to the Charente mouth and some sixty kilometres up a slow,
  winding river on the tide.
- **Why go:** the nearest salt and wine market to the north that is not Bordeaux; quieter tolls; and in the ninth
  century the place to hear where the Northmen on the Charente are.
- **Evidence:** the Annals of St Bertin, 845: the Northmen beat Count Seguin, kill him, and plunder and burn Saintes.
  In 863 Count Turpio of Angoulême dies fighting them. Northmen stay on the Charente through the 860s. Geoffrey Martel
  and Agnes found the Abbaye aux Dames in 1047 and give it the town's mint (?).
- **Sources:** https://fr.wikipedia.org/wiki/Saintes (read this session); Annals of St Bertin 845, 863;
  https://en.wikipedia.org/wiki/Abbaye_aux_Dames,_Saintes ; `Design/content/dossiers/burdigala.md`.

```json
{"id": "santonas", "name": "Santonas", "modern": "Saintes", "lat": 45.746, "lon": -0.633, "realm": "franks", "county": "", "region": "aquitaine", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "An old Roman city on the Charente with an arch on its bridge: salt of the marshes and wine; plundered by Northmen in 845."}
```

---

# Good to have

## 12. Witsand (Wissant)

- **Name then:** Witsant (933), Witsantum (938), Guisantum (1036): "white sand".
- **Modern place:** Wissant, Pas-de-Calais. **Lat, lon:** 50.886, 1.662.
- **Active:** from about 933 to past 1066 (the main crossing place until Calais took over in the thirteenth century).
- **Kind:** an open beach in the bay between the two capes, at the end of an old lagoon. No town to speak of.
- **Who ruled:** the counts of Flanders, then the counts of Boulogne.
- **Traded:** passage. Kings, bishops, pilgrims for Rome, messengers and horses; herring.
- **How a boat comes in:** straight onto the sand, and off again on the next tide. No shelter from the north.
- **Why go:** the shortest crossing of all, and the surest place to find a fare or a letter for England.
- **Evidence:** Flodoard, 938: King Louis goes to the coast meaning to restore the fort and harbour of Wissant (?).
  William of Jumièges: the atheling Alfred sails from Wissant to Dover in 1036.
- **Sources:** https://fr.wikipedia.org/wiki/Wissant (read this session: the name forms only);
  https://en.wikipedia.org/wiki/Wissant . The 938 and 1036 crossings are from memory of Flodoard and William of
  Jumièges, hence (?).

```json
{"id": "witsand", "name": "Witsand", "modern": "Wissant", "lat": 50.886, "lon": 1.662, "realm": "franks", "county": "", "region": "neustria", "kind": "market", "landing": "beach", "size": 1, "from": 933, "to": 0, "note": "A white beach between the two capes, the shortest crossing to Dover; passengers and letters, little else."}
```

## 13. Ambianis (Amiens)

- **Name then:** Ambianis.
- **Modern place:** Amiens, Somme. **Lat, lon:** 49.894, 2.302.
- **Active:** the whole game.
- **Kind:** a walled bishop's city on the Somme among marsh channels, with a royal toll. The great abbey of Corbie is
  fifteen kilometres upstream.
- **Who ruled:** the king's counts and the bishop; Herbert II of Vermandois after 925; then the counts of Amiens,
  Valois and the Vexin until 1077.
- **Traded:** corn, cloth and wine; what Corbie buys (its abbot's list of 822 names pepper, cumin and other spices
  bought at Cambrai (?)). Woad is its later fame, not this period's.
- **How a boat comes in:** in at Saint-Valery and up a slow, marshy Somme.
- **Why go:** one of the kingdom's named toll gates, and an abbey that buys by the cartload.
- **Evidence:** Charlemagne's charter of 779 for Saint-Germain-des-Prés frees its men from toll at Rouen, Quentovic,
  Amiens, Maastricht and Dorestad: Amiens was one of the gates of the northern trade. Northmen raid in 859 and 881,
  winter here 883 to 884, hold the city for months in 891; the Northmen of Rouen burn it in 925.
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_d%27Amiens (read this session);
  https://en.wikipedia.org/wiki/Corbie_Abbey .

```json
{"id": "ambianis", "name": "Ambianis", "modern": "Amiens", "lat": 49.894, "lon": 2.302, "realm": "franks", "county": "", "region": "neustria", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "A bishop's city in the marshes of the Somme with one of the king's named tolls; the great abbey of Corbie lies just upstream."}
```

## 14. Auga (Eu, with Le Tréport)

- **Name then:** Auga (925, 927); the haven below is Ulterior Portus (Le Tréport).
- **Modern place:** Eu and Le Tréport, Seine-Maritime. **Lat, lon:** 50.048, 1.420 (Eu; the beach at Le Tréport is
  50.059, 1.376).
- **Active:** from 925 as a fort; a count's town from 996.
- **Kind:** a fort on the Bresle, the border river of Normandy, four kilometres from its mouth.
- **Who ruled:** the Northmen of Rouen; the counts of Eu from 996 (William, a son of Richard I, from 1002).
- **Traded:** little: border dues, fish, salt. A place for people and news more than cargo.
- **How a boat comes in:** onto the shingle at the Bresle mouth under the cliffs, or up the river on the tide (?).
- **Why go:** it is Normandy's last haven before Ponthieu, and things happen here.
- **Evidence:** Flodoard, 925: Rollo sends a thousand men from Rouen to hold the fort of Auga; the Franks storm it
  and kill them, and those who flee to an island close by are killed too. William and Matilda of Flanders marry here
  about 1050. Guy of Ponthieu hands Harold over to William at Eu in 1064 (William of Poitiers). The abbey of Saint
  Michael at Le Tréport is founded in 1059.
- **Sources:** https://fr.wikipedia.org/wiki/Eu_(Seine-Maritime) (read this session);
  https://en.wikipedia.org/wiki/Le_Tr%C3%A9port .

```json
{"id": "auga", "name": "Auga", "modern": "Eu and Le Tréport", "lat": 50.048, "lon": 1.420, "realm": "normans", "county": "", "region": "neustria", "kind": "fort", "landing": "beach", "size": 2, "from": 925, "to": 0, "note": "The Northmen's border fort on the Bresle, stormed by the Franks in 925; a count's town from 996, with a shingle haven at the river mouth."}
```

## 15. Deppa (Dieppe)

- **Name then:** Deppa, Deppae (from the river, "the deep", a Norse or English word).
- **Modern place:** Dieppe, Seine-Maritime. **Lat, lon:** 49.925, 1.078.
- **Active:** from about 1030.
- **Kind:** a fishing haven at a river mouth between cliffs, with salt pans in the valley behind (Bouteilles).
- **Who ruled:** the dukes of Normandy.
- **Traded:** herring, fresh and cured; salt.
- **How a boat comes in:** into the river mouth on the tide; deep water close in, which is its name.
- **Why go:** herring by the thousand in the autumn, and the nearest haven to Rouen on the open coast.
- **Evidence:** a charter of about 1030 has the port of Dieppe owing five thousand herrings a year to the abbey of
  Sainte-Catherine-du-Mont at Rouen (?). Orderic Vitalis: William sails for England from the mouth of the Dieppe river
  on 6 December 1067, just after the game ends.
- **Sources:** https://en.wikipedia.org/wiki/Dieppe ; https://fr.wikipedia.org/wiki/Dieppe . The French history page
  I tried was not found, so both facts are from memory and carry (?).

```json
{"id": "deppa", "name": "Deppa", "modern": "Dieppe", "lat": 49.925, "lon": 1.078, "realm": "normans", "county": "", "region": "neustria", "kind": "village", "landing": "beach", "size": 1, "from": 1030, "to": 0, "note": "A deep river mouth between the cliffs: herring in autumn and salt pans up the valley (?)."}
```

## 16. Harofloth (Harfleur)

- **Name then:** Harofloz (1006), Harofloth (1025), Herolfluoth (1035): a Norse or English man's name and "fleet", a
  tidal creek. The Roman Caracotinum stood here.
- **Modern place:** Harfleur, Seine-Maritime. **Lat, lon:** 49.507, 0.199.
- **Active:** named from 1006; a landing was surely here before (?).
- **Kind:** a creek (the Lézarde) on the north shore of the Seine estuary; the nuns' abbey of Montivilliers lies a
  little way up it (refounded 1035).
- **Who ruled:** the dukes of Normandy.
- **Traded:** salt from the estuary pans; fish; pilotage; whatever is waiting for the Seine.
- **How a boat comes in:** into the creek on the flood. The estuary outside is all moving sandbanks and a strong tide.
- **Why go:** to wait for the flood that carries a boat up to Rouen, and to hire a man who knows this week's channel.
- **Evidence:** the name forms in ducal charters; little else before 1066. Its great days as "the sovereign port of
  Normandy" are the twelfth century and after.
- **Sources:** https://fr.wikipedia.org/wiki/Harfleur (read this session).

```json
{"id": "harofloth", "name": "Harofloth", "modern": "Harfleur", "lat": 49.507, "lon": 0.199, "realm": "normans", "county": "", "region": "neustria", "kind": "village", "landing": "riverbank", "size": 1, "from": 1006, "to": 0, "note": "A tidal creek on the north shore of the Seine mouth: wait here for the flood to Rouen and hire a pilot for the sandbanks."}
```

## 17. Gemeticum (Jumièges)

- **Name then:** Gemeticum.
- **Modern place:** Jumièges, Seine-Maritime. **Lat, lon:** 49.432, 0.819.
- **Active:** from 654 to 841; a few monks from about 930; a great house again from about 1000.
- **Kind:** an abbey in a loop of the tidal Seine below Rouen, with its own landing and ferry.
- **Who ruled:** the Frankish king; the Norman dukes from 911. Abbot Robert (1037 to about 1044) becomes bishop of
  London and then archbishop of Canterbury (1051).
- **Traded:** the abbey's wine, salt and fish; ferry and river dues; in the eleventh century rights on the river
  traffic at Pont-de-l'Arche (charter of 1020) and landings lower down (Quillebeuf, Vieux-Port).
- **How a boat comes in:** up the Seine on the flood; she ties to the abbey's bank on the inside of the loop.
- **Why go:** a tide's rest between the sea and Rouen, and a buyer with silver. In 793 it is one of the largest
  houses in the north (the old count of 900 monks is a legend's number (?)).
- **Evidence:** the Chronicle of Fontenelle, 841: the fleet enters the Seine on 12 May, burns Rouen on the 14th,
  burns Jumièges on the 24th, and takes six pounds from Saint-Wandrille to spare it on the 25th. The monks go to
  Haspres near Cambrai. William Longsword settles twelve monks from Poitiers here about 930 to 940. The new church is
  begun in 1040 and consecrated on 1 July 1067 before Duke William.
- **Game notes:** Saint-Wandrille (Fontanella), a few kilometres downriver, is the same kind of place and is folded
  into this one; its abbot Gervold was the king's toll-master at Quentovic about 790.
- **Sources:** https://fr.wikipedia.org/wiki/Abbaye_de_Jumi%C3%A8ges (read this session);
  https://en.wikipedia.org/wiki/Jumi%C3%A8ges_Abbey ; `Design/content/dossiers/rotomagus.md`.

```json
{"id": "gemeticum", "name": "Gemeticum", "modern": "Jumièges", "lat": 49.432, "lon": 0.819, "realm": "franks", "county": "", "region": "neustria", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "Saint Philibert's great abbey in a loop of the Seine below Rouen; burned by the Northmen on 24 May 841 and rebuilt under the dukes."}
```

## 18. Pistis (Pîtres and Pont-de-l'Arche)

- **Name then:** Pistis (the royal estate where the king held his assemblies); the bridge is later Pons Arcae (?).
- **Modern place:** Pont-de-l'Arche, with Pîtres, Eure. **Lat, lon:** 49.303, 1.155.
- **Active:** from 862. A river toll under the Normans.
- **Kind:** a wooden bridge over the Seine with a fort at each end, just below where the Eure and the Andelle come in.
- **Who ruled:** Charles the Bald and his heirs; the Norman dukes from 911.
- **Traded:** nothing of its own. It takes a toll and it stops boats.
- **How a boat comes in:** she must be let through, and hauled through, for a fee.
- **Why go:** a boat for Paris has no choice. It is also where the king's law on coin is given in 864: old pennies
  cease to pass, and only ten named mints may strike.
- **Evidence:** the Annals of St Bertin: the works are ordered in 862 and pressed on again in 866 and 869; the
  assemblies at Pîtres of 862, 864 and 869. The Edict of Pîtres, 25 June 864. The great fleet of 885 gets past it on
  the way to Paris. A charter of Duke Richard II (1020) gives Jumièges rights on the river traffic here.
- **Game notes:** the only place on this coast where the chart itself is closed by a king. Before 862, and when the
  garrison has run, the river is open.
- **Sources:** https://fr.wikipedia.org/wiki/Pont-de-l%27Arche (read this session);
  https://en.wikipedia.org/wiki/Edict_of_P%C3%AEtres .

```json
{"id": "pistis", "name": "Pistis", "modern": "Pîtres and Pont-de-l'Arche", "lat": 49.303, "lon": 1.155, "realm": "franks", "county": "", "region": "neustria", "kind": "fort", "landing": "riverbank", "size": 2, "from": 862, "to": 0, "note": "King Charles's bridge across the Seine with a fort at each end, built against the Northmen from 862; every boat for Paris is stopped here."}
```

## 19. Baiocas (Bayeux, by Port-en-Bessin)

- **Name then:** Baiocas.
- **Modern place:** Bayeux, Calvados; the landing is Port-en-Bessin. **Lat, lon:** 49.347, -0.754 (the landing; the
  city is at 49.277, -0.703, nine kilometres inland).
- **Active:** the whole game.
- **Kind:** a bishop's walled city a short walk from the sea. Its river, the Aure, cannot carry a boat: a merchant
  lands in the gap in the cliffs at Port-en-Bessin and goes up by road.
- **Who ruled:** the Frankish king (the country round it is called the Saxon shore, Otlinga Saxonia, in the ninth
  century); the Northmen from 924; the dukes. Odo, the duke's half-brother, is bishop from about 1049.
- **Traded:** corn, cattle and horses of the Bessin; wine coming in.
- **How a boat comes in:** onto the shingle at Port-en-Bessin between two cliffs.
- **Why go:** in the tenth century this is the most Norse town in Normandy, and a Dane or a Northman can be
  understood here when Rouen has forgotten the speech.
- **Evidence:** Flodoard, 924: the king's grant to the Northmen of the Bessin and Maine. Dudo of Saint-Quentin:
  William Longsword sends his son Richard to Bayeux to learn Danish, because Rouen speaks Roman. Dudo also has Rollo
  take the city about 890 and carry off Poppa, the count's daughter (a story (?)). Bishop Baltfrid is killed by
  Northmen in 858 (?). Harold's oath is set at Bayeux by the Tapestry (1064).
- **Sources:** https://en.wikipedia.org/wiki/Bayeux ; https://en.wikipedia.org/wiki/Port-en-Bessin-Huppain ; Dudo
  (Christiansen's translation is in copyright: do not quote). The French history page I tried was not found: (?) on
  the dates not from Flodoard.

```json
{"id": "baiocas", "name": "Baiocas", "modern": "Bayeux (the landing at Port-en-Bessin)", "lat": 49.347, "lon": -0.754, "realm": "franks", "county": "", "region": "neustria", "kind": "town", "landing": "beach", "size": 2, "from": 0, "to": 0, "note": "A bishop's city two hours' walk from a gap in the cliffs; the Northmen's from 924, and the Danish speech lasts longest here."}
```

## 20. Barbefloth (Barfleur)

- **Name then:** Barbefloth, Barbeflueth (written 1066 to 1077).
- **Modern place:** Barfleur, Manche. **Lat, lon:** 49.671, -1.263.
- **Active:** from about 1000 (?); written evidence only from the 1060s.
- **Kind:** a small rock-bound haven on the north-east corner of the Cotentin.
- **Who ruled:** the Bretons on paper from 867; the Normans from 933; the dukes.
- **Traded:** passage to the south coast of England; fish.
- **How a boat comes in:** between ledges of rock into a haven that dries. The race off the point is the worst water
  on this coast.
- **Why go:** the crossing to Hamwic's successor (Southampton) and Wessex from the west of Normandy.
- **Evidence:** thin before 1066. Orderic Vitalis, of 1120: the master of the White Ship, Thomas son of Stephen, says
  his father Stephen son of Airard carried Duke William over in 1066. The harbour's time as the kings' own crossing
  is 1066 to 1120.
- **Game notes:** the Mora sailed from Saint-Valery, not from here; the Barfleur pilot is a family claim.
- **Sources:** https://fr.wikipedia.org/wiki/Barfleur (read this session); https://en.wikipedia.org/wiki/White_Ship .

```json
{"id": "barbefloth", "name": "Barbefloth", "modern": "Barfleur", "lat": 49.671, "lon": -1.263, "realm": "normans", "county": "", "region": "neustria", "kind": "village", "landing": "beach", "size": 1, "from": 1000, "to": 0, "note": "A rock-bound haven on the corner of the Cotentin, the Normans' crossing place to the south of England (?); a hard tide race off the point."}
```

## 21. Mons Sancti Michaelis (Mont-Saint-Michel)

- **Name then:** Mons Tumba; Saint Michael's Mount "in peril of the sea".
- **Modern place:** Le Mont-Saint-Michel, Manche. **Lat, lon:** 48.636, -1.511.
- **Active:** the whole game (the oratory dates from 708).
- **Kind:** a rock in a bay of sands with a church on top: canons until 966, Benedictine monks after.
- **Who ruled:** the Franks; the Bretons from 867; the Normans from 933. Richard I puts in the monks in 965 to 966.
- **Traded:** pilgrims, and what pilgrims buy: wax, tokens, food, a guide over the sands.
- **How a boat comes in:** at the top of a spring tide only, to ground under the rock. The sea leaves for miles and
  comes back faster than a man walks.
- **Why go:** passengers who pay; a monk who wants wax, wine and a letter carried.
- **Evidence:** the Frankish monk Bernard visits on his way home from Jerusalem about 867 to 870. Northmen attack in
  847 (?). The church of Notre-Dame-sous-Terre (tenth century) stands under the later abbey. The new church is begun
  in 1023. The Bayeux Tapestry shows the mount and Harold pulling men out of the quicksand of the Couesnon (1064).
- **Sources:** https://fr.wikipedia.org/wiki/Abbaye_du_Mont-Saint-Michel (read this session).

```json
{"id": "mons_michaelis", "name": "Mons Sancti Michaelis", "modern": "Mont-Saint-Michel", "lat": 48.636, "lon": -1.511, "realm": "franks", "county": "", "region": "neustria", "kind": "monastery", "landing": "beach", "size": 2, "from": 0, "to": 0, "note": "The archangel's church on a rock in a bay of sands; pilgrims walk over at low water, and a boat comes only on the top of the tide."}
```

## 22. Roton (Redon)

- **Name then:** Roton; the monastery of the Holy Saviour.
- **Modern place:** Redon, Ille-et-Vilaine. **Lat, lon:** 47.651, -2.084.
- **Active:** from 832.
- **Kind:** an abbey where the Oust meets the Vilaine, at the reach of the tide, on the line between Breton and
  Frankish speech.
- **Who ruled:** the Breton princes, its founders and patrons: Nominoe, Erispoe, Salomon; later the dukes.
- **Traded:** salt from its own pans at Guérande; wine; corn; dues on boats on both rivers.
- **How a boat comes in:** in at the Vilaine mouth and some forty kilometres up on the tide.
- **Why go:** the abbey buys and sells in bulk and keeps written record of it. A merchant with its goodwill is known
  across southern Brittany.
- **Evidence:** founded by Conwoion in 832; confirmed by Louis the Pious at Nominoe's asking on 18 June 834. Its
  cartulary keeps 391 acts, most of the ninth century: gifts of land, tolls, salt works and river rights. The Deeds
  of the Saints of Redon: a Northman fleet on the Vilaine in 854 is caught by a storm, vows gifts to the Saviour, sets
  candles in the church and spares the house (?). In the 860s the monks withdraw to Maxent near Plélan, where
  Conwoion dies in 868.
- **Sources:** https://fr.wikipedia.org/wiki/Abbaye_Saint-Sauveur_de_Redon (read this session);
  https://en.wikipedia.org/wiki/Cartulary_of_Redon .

```json
{"id": "roton", "name": "Roton", "modern": "Redon", "lat": 47.651, "lon": -2.084, "realm": "bretons", "county": "", "region": "brittany", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 832, "to": 0, "note": "The Bretons' abbey of the Holy Saviour where the Oust meets the Vilaine, founded in 832; it owns salt pans at Guérande and boats on both rivers."}
```

## 23. Wenran (Guérande, with Batz)

- **Name then:** Werran (854), Uuenran (857): "the white land" (?).
- **Modern place:** Guérande, with Batz-sur-Mer and Saillé, Loire-Atlantique. **Lat, lon:** 47.300, -2.440 (the
  landing among the pans; Guérande itself is at 47.328, -2.429).
- **Active:** named from 854; salt was made here before (?).
- **Kind:** a village on a low ridge over a wide tidal flat cut into salt pans. No harbour: creeks.
- **Who ruled:** Breton lords under the Breton princes; a rival bishop, Gislard, sits here after 851; Northmen on the
  Loire mouth 919 to 937; then the counts of Nantes.
- **Traded:** salt, at the source. Abbeys own the pans: Redon in the ninth century, Saint-Aubin of Angers at Saillé
  about 950, Landévennec at Batz (?).
- **How a boat comes in:** up a muddy creek on the flood between the pans, and out on the next one. A flat-bottomed
  boat's place.
- **Why go:** the cheapest salt north of the Loire. It goes up the river to Angers, Tours and Orléans and round
  Brittany to the north.
- **Evidence:** charters of the Redon cartulary of the 850s and 860s giving salt works here; the Chronicle of Nantes
  for Gislard; `Design/content/economy/wine_salt.md` for Saillé.
- **Game notes:** `herio` (Noirmoutier) already sells Bay salt. This is the north-bank source, in Breton hands, and
  matters once Herio's monks have left (836).
- **Sources:** https://fr.wikipedia.org/wiki/Gu%C3%A9rande (read this session: the name forms);
  `Design/content/economy/wine_salt.md`; `Design/content/dossiers/namnetis.md`.

```json
{"id": "wenran", "name": "Wenran", "modern": "Guérande and Batz", "lat": 47.300, "lon": -2.440, "realm": "bretons", "county": "", "region": "brittany", "kind": "village", "landing": "beach", "size": 1, "from": 850, "to": 0, "note": "Salt pans on a wide tidal flat north of the Loire mouth, owned by abbeys; the cheapest salt on the Breton shore."}
```

---

# Leave out

## 24. Rupella (La Rochelle)

- **Name then:** Rochella, Rupella: "the little rock".
- **Modern place:** La Rochelle, Charente-Maritime. **Lat, lon:** 46.160, -1.151.
- **Active:** a hamlet from about 1000; a port only from the 1130s.
- **Kind:** a rock among marshes with salt pans; the older hamlet beside it is Cougnes.
- **Who ruled:** the lords of Châtelaillon under the counts of Poitou.
- **Traded:** salt of Aunis, in small amounts.
- **How a boat comes in:** a sheltered bay behind the isles of Ré and Oléron; a creek.
- **Why go:** no reason before 1066.
- **Evidence:** first written in a charter of between 988 and 1031 that gives away salt marsh at Cougnes and
  Rochella. Walls and freedoms come from Duke William X about 1130.
- **Why leave out:** it is the great port of this coast in the next age, not this one. If the player wants Aunis
  salt, Saintes sells it.
- **Sources:** https://fr.wikipedia.org/wiki/Histoire_de_La_Rochelle (read this session).

```json
{"id": "rupella", "name": "Rupella", "modern": "La Rochelle", "lat": 46.160, "lon": -1.151, "realm": "franks", "county": "", "region": "aquitaine", "kind": "village", "landing": "beach", "size": 1, "from": 1000, "to": 0, "note": "A rock and a hamlet among the salt marshes of Aunis, behind the isles; not yet a port (?)."}
```

## 25. Lapurdum (Bayonne)

- **Name then:** Lapurdum (the Roman fort; the country is still Labourd). "Baiona" appears only about 1100 (?).
- **Modern place:** Bayonne, Pyrénées-Atlantiques. **Lat, lon:** 43.493, -1.475.
- **Active:** the whole game, barely.
- **Kind:** a Roman wall where the Nive meets the Adour.
- **Who ruled:** the Gascon dukes; viscounts of Labourd from about 1023 (?); a bishop again from about 1056 (?).
- **Traded:** unknown for this period. Whaling and the wine trade are later.
- **How a boat comes in:** over a dangerous bar at the Adour mouth.
- **Why go:** the last haven before the Basque coast and Spain.
- **Evidence:** almost none between the Roman fort and the eleventh-century bishops. A Northman base here in the
  ninth and tenth centuries is argued by one modern writer and doubted by most (?).
- **Why leave out:** two hundred kilometres past Bordeaux with nothing sure to put in the panel. It would be invention.
- **Sources:** https://en.wikipedia.org/wiki/Bayonne ; https://fr.wikipedia.org/wiki/Bayonne . The French history
  page I tried was not found: every date here carries (?).

```json
{"id": "lapurdum", "name": "Lapurdum", "modern": "Bayonne", "lat": 43.493, "lon": -1.475, "realm": "franks", "county": "", "region": "aquitaine", "kind": "fort", "landing": "riverbank", "size": 1, "from": 0, "to": 0, "note": "An old Roman wall where the Nive meets the Adour, in the Gascons' country; little is known of it (?)."}
```

---

## Looked at and not written up

| Place | Why not |
|---|---|
| Stapulas (Étaples) | Quentovic's outer landing: part of the `quentovic` port, and a *villa* only (the Quentovic dossier says so). |
| Centula (Saint-Riquier) and Abbeville | A very great abbey about 800 with a street of merchants in its own survey, but off the water. Best as a site reached from Saint-Valery. Abbeville is a castle only from the 990s. |
| Fontanella (Saint-Wandrille) | Folded into Jumièges, a few kilometres away. |
| Honfleur, Dives, Touques | Named only from the 1020s; Dives matters for one month of 1066 (the fleet gathers there before Saint-Valery). A card, not a port. |
| Cherbourg, Coutances, Avranches | A ducal castle (1020s) and two ruined sees; no trade evidence. |
| Dol | A Breton see near the Mont, besieged in 1064 (the Tapestry). Reached over the same sands: a site of Mont-Saint-Michel or Alet. |
| Quimper, Saint-Pol, Landévennec, Brest | West Brittany: sees and abbeys with almost no trade record before 1066. Landévennec (raided 913) is worth one card off the Breton coast. |
| Blois, Amboise, Saumur, Saint-Florent | Loire castles and abbeys between the ports above. Saint-Florent's island was the Northmen's camp (853 on): a card on the river, not a port. |
| Fleury (Saint-Benoît-sur-Loire) | Above Orléans; a site or a commission from Orléans. |
| Melun, Mantes, Pontoise, Meulan | Seine and Oise bridges between Rouen and Paris; the bridge at Pîtres stands for them. |
| Pictavis (Poitiers), Engolisma (Angoulême), Metullo (Melle) | Inland cities with no boat water. Melle's silver mine and mint (one of the ten of 864, raided 848) matter to the economy, and its silver should reach the coast through Saintes. |
| Oléron, Ré, Taillebourg | Islands and a Northman camp on the Charente: cards on the way to Saintes. |
| Rezé, Déas | Part of `namnetis` and `herio`. |

## Needs (for the spine; nothing here was added to a shared table)

1. **Realm ids `bretons` and `normans`.** The table has only `franks` for this coast. Alet, Redon and Guérande were
   never properly Frankish, and seven rows begin under the Norman dukes.
2. **A way to change a port's realm by year.** Fécamp, Jumièges, Bayeux, the Mont, Eu and Rouen pass from the Franks
   to the Northmen in 911, 924 and 933; Vannes from Franks to Bretons about 831 to 851; Tours and Saintes to Anjou in
   the eleventh century. The "Who ruled" lines here are written to be turned into such a list.
3. **Region key `brittany`** in `goods_793.json`: makes salt, corn, honey, wax, hides; wants wine, iron, fine cloth (?).
4. **A closed-river rule** for `pistis`: a port that blocks the route behind it unless a toll is paid or the year is
   before 862.
5. **A port whose landing is not its town** (`baiocas`, `auga`, `wenran`): the pin is at the water, the name is the
   town's. If the chart wants the town's own point, the coordinates are in each entry.
6. **Tide waits.** Alet, the Mont, Saint-Valery, Vannes and Guérande can only be entered near high water: the
   TideWait system already built should carry these.
7. **Quentovic's `to`** is 0 in the table. The Quentovic dossier puts its last coin about 980; if Montreuil goes in,
   Quentovic should probably end about then.

## What I could not check

- Boulogne's counts and the 892 Chronicle wording (both Wikipedia pages read were empty for these years).
- Wissant in 938 (Flodoard) and 1036 (William of Jumièges): from memory.
- Dieppe's herring charter of 1030 and Bayeux's dates of 858 and 890: the pages were not found.
- Bayonne altogether.
- The Redon story of the Northmen's candles (854), the relics of Saint Valery (981), and the Saintes mint grant (1047).
- Whether trade goods I name for Alet and Vannes (honey, wax, hides, horses) are attested there or only likely.
- No row's `county` was matched to `lands_793.json`.
- Coordinates are town or landing centres to three decimals from memory of the maps, not measured against the game's
  coastline bake; a river port may need nudging onto its water.

## Sources read this session

French Wikipedia: Wissant; Saint-Valery-sur-Somme; Fécamp; Histoire de Caen; Cité d'Alet; Histoire de Vannes;
Histoire d'Angers; Histoire de Tours; Histoire d'Orléans; Saintes; Montreuil-sur-Mer; Guérande; Barfleur;
Eu (Seine-Maritime); Harfleur; Abbaye de Jumièges; Pont-de-l'Arche; Abbaye Saint-Sauveur de Redon; Histoire d'Amiens;
Abbaye du Mont-Saint-Michel; Histoire de La Rochelle; Histoire de Boulogne-sur-Mer. English Wikipedia:
Boulogne-sur-Mer. In the project: `dossiers/quentovic.md`, `dossiers/namnetis.md`, `dossiers/rotomagus.md`,
`realms/francia.md`, `economy/wine_salt.md`.

Primary texts named above (Royal Frankish Annals, Annals of St Bertin, Annals of St Vaast, Flodoard, the Chronicle
of Fontenelle, Dudo, William of Poitiers, William of Jumièges, Orderic Vitalis, the Redon cartulary) were not opened
this session: they are cited as the place each fact comes from, at second hand. Of these only the Anglo-Saxon
Chronicle (Giles 1847, Ingram 1823) has a translation the game may quote; the rest are paraphrased here on purpose.
