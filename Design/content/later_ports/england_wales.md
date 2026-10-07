# Ports the game lacks: England and Wales, 793-1066

Written 6 Oct 2026 for the port table (`Assets/Kapman/Game/Data/ports_793.json`). The table was made for 793 and
already holds, for this region: Lundenwic, Hamwic, Gipeswic, Eoforwic, Serræ (Sarre), Fordwic, Dofras, Dommoc,
Legacæstir (Chester), and the Northumbrian houses. This file adds 25 places that are not in it and that traded at some
time between 793 and 1066.

How to read it:

- **Rank**: must have (10), good to have (13), leave out (2, with rows in case the user wants them), then a list of
  places looked at and dropped without a row.
- **Row**: one line in the exact format of `ports_793.json`, ready to paste. `from` is the first year the place is
  worth a merchant's visit, `to` 0 = no end before 1066. `realm` is the realm that holds it in the `from` year (or in
  793 where `from` is 0), using the ids the table already uses. Who holds it later is in the entry.
- **(?)** marks what is doubtful in the sources, or what I state from memory and could not check tonight. Only five
  pages were fetched tonight (Wikipedia: Torksey, Meols, Llanbedrgoch, Thetford, History of Bristol), and the
  Llanbedrgoch page had nothing of use. The rest is from the standard works named under each place, not re-read
  tonight: treat house counts, charter numbers and page-level details as unchecked until someone opens the book.
- **ASC** = Anglo-Saxon Chronicle, with the manuscript letter and the true year. **S** = Sawyer number of a charter
  (esawyer.lib.cam.ac.uk). **DB** = Domesday Book (1086), where "TRE" means "in King Edward's day", that is 1066.
- Coordinates are the landing where I know it, otherwise the middle of the old town; from memory, to be checked
  against the chart when the rows go in.

## What the spine must add before these rows load (needs)

- **Realm ids** that do not exist yet: `gwynedd` (Llanbedrgoch; Rhuddlan after 1039) and `dyfed` (Mynyw; Deheubarth
  from c. 920). The two Welsh rows below use them, so those two rows will not validate until they exist. Also some
  way to say "the Danes' army holds it" for the Midlands and East Anglia, 869-920 (the rows below use `mercia` and
  `eastanglia`, the land's own realm), and "the kingdom of the English" from 927.
- **Region ids** that do not exist yet: `wales` (used by the three Welsh rows). The rows for Lincoln, Torksey,
  Stamford and Nottingham use `england_east`; a region of their own would be truer (iron, glazed pots, wool; they
  want wine, querns, silver).
- **Changes to rows already in the table** (not made by me; the table is not my file):
  - `lundenwic`: `"to": 0` should be about 886. The wic on the Strand is given up between c. 850 and 886 and the
    trade moves inside the walls (see Lundenburh). The old place is "Ealdwic" later, hence Aldwych.
  - `hamwic`: the emporium on the Itchen shrinks through the 9th century (raided 840 and 842) and the town moves west
    to Hamtun, the later walled Southampton, by c. 900-950. One row with a note, or a second row `hamtun` from 900.
  - `fordwic`: stays as Canterbury's landing; see Cantwaraburh below for whether the city needs its own row.
  - `legacaestir`: a ruin in 793 ("a deserted city in Wirral", ASC A 893); refounded as a burh in 907 and from
    c. 920 one of the busiest mints in England, on the Dublin trade. Size 2 is right for 793, 4 for 930-980.

---

# MUST HAVE

## 1. Lundenburh (London within the walls)

- **Name then**: Lundenburh, Lundenburg (ASC 886); Lundonia in Latin.
- **Modern place**: the City of London; the landings at Queenhithe and Billingsgate.
- **Lat, lon**: 51.511, -0.094 (Queenhithe).
- **Active**: 886 to the end of the game, and on. It replaces Lundenwic, two kilometres upstream.
- **Kind**: a walled town, by 1000 the largest in England and its richest mint.
- **Who ruled**: 886 Alfred of Wessex "set" the burh and gave it to Æthelred, ealdorman of the Mercians (ASC A 886).
  911 Edward the Elder took London and Oxford to himself on Æthelred's death (ASC A). From 927 the kingdom of the
  English. 1013 it yields to Swein; 1016 Cnut besieges it and digs a ditch round Southwark to drag his ships past
  the bridge (ASC D,E 1016); Danish kings 1016-1042; Edward the Confessor 1042-1066.
- **Traded**: in: wine and blubber-fish (whale) from Rouen; cloth from Flanders; pepper, gloves, vinegar and fine
  cloth with the Emperor's men (Rhinelanders); timber; fish; hens, eggs, butter and cheese from the country. Out:
  wool, melted fat, live pigs (named in the toll list as what the Emperor's men may buy), slaves, tin and lead (?),
  cloth, coin of full weight.
- **How a boat comes in**: on the flood up the Thames, fifty sea miles from the Nore. Under Alfred two hards are laid
  out on the foreshore inside the Roman wall: Æthelred's hythe (Queenhithe) above the bridge, and later Billingsgate
  below it. Ships are beached on the foreshore against timber and clay banks; there is no deep-water quay in these
  years. From c. 1000 there is a timber bridge: a masted ship stops at Billingsgate, a small boat can go under.
- **Why a merchant goes**: it is where everything is sold and where the king's reeve keeps the toll list. A small boat
  pays a halfpenny. The best coin in England is struck here. News from every shore.
- **Evidence**: ASC 886. Two charters giving plots with mooring rights to the bishop of Worcester and the archbishop
  of Canterbury: S 346 (889, "Hwætmundes stan") and S 1628 (898/899, "Ætheredes hyd"). The toll list called
  IV Æthelred (c. 1000, parts perhaps later): at Billingsgate a small ship pays a halfpenny, a larger ship with sails
  a penny, a keel or hulk fourpence; the men of Rouen with wine or blubber-fish pay six shillings for a great ship
  and a twentieth of the fish; men of Flanders, Ponthieu, Normandy and Francia show their goods and pay toll; so do
  men of Huy, Liège and Nivelles; the Emperor's men who come in their ships are "worthy of good laws like
  ourselves" and give at Christmas and Easter two grey cloths and one brown, ten pounds of pepper, five pairs of
  men's gloves and two kegs of vinegar. Digs: the waterfront banks at Bull Wharf (Queenhithe) and New Fresh Wharf,
  10th and 11th century. 1018: London alone pays 10,500 pounds of Cnut's tribute (ASC).
- **Sources**: ASC (Giles 1847; Ingram 1823). A. J. Robertson, Laws of the Kings of England from Edmund to Henry I
  (1925), IV Æthelred. T. Dyson, "King Alfred and the restoration of London", London Journal 15 (1990). J. Ayre and
  R. Wroe-Brown, "The post-Roman foreshore and the origins of the late Anglo-Saxon waterfront and dock of
  Æthelred's Hithe", Archaeological Journal 172 (2015). R. Naismith, Citadel of the Saxons (2018).
  https://en.wikipedia.org/wiki/Anglo-Saxon_London

```json
{"id": "lundenburh", "name": "Lundenburh", "modern": "London (the City, Queenhithe)", "lat": 51.511, "lon": -0.094, "realm": "mercia", "county": "", "region": "england_south", "kind": "town", "landing": "beach", "size": 5, "from": 886, "to": 0, "note": "Alfred's burh inside the Roman walls, 886: ships on the hards at Æthelred's hythe and Billingsgate; the king's toll list of c. 1000; a bridge from c. 1000."},
```

## 2. Sandwic (Sandwich)

- **Name then**: Sandwic; "portus Sandwicae" in Latin.
- **Modern place**: Sandwich, Kent.
- **Lat, lon**: 51.275, 1.340.
- **Active**: a landing before the game (Wilfrid lands there c. 666); a fleet haven from 851; a town from c. 1000.
  It grows as Sarre and Fordwic fade and outlives the game.
- **Kind**: haven and market at the south mouth of the Wantsum; the king's fleet station in the 11th century.
- **Who ruled**: Kent under Mercia in 793; Wessex from 825; the English kings. The harbour and its tolls are said to
  be given to Christ Church, Canterbury, by Cnut in 1023 (S 959; the charter as we have it is doubted (?)); taken
  by Harold Harefoot c. 1037 and given back (?).
- **Traded**: herring (the town pays 40,000 herrings a year to the monks' table, DB); salt; fish; passage money;
  ship's stores for the fleet. In from over the sea: wine, cloth, millstones.
- **How a boat comes in**: round the North Foreland or inside Thanet by the Wantsum, into the Stour mouth behind
  the Stonar shingle bank. A sheltered tidal haven with a soft bank to lie on; the Downs anchorage is outside, with
  the Goodwin Sands beyond. Ships wait here for a wind for London or for Flanders.
- **Why a merchant goes**: the first English harbour from the Rhine mouth and Flanders, and the door to Canterbury.
  The fleet lies here most summers after 1009: news of the king, the earls and the raiders, and a market for
  stores. The monks' reeve takes the toll.
- **Evidence**: Stephen of Ripon, Life of Wilfrid ch. 13 (the landing "in portum Sandwicae"). ASC A 851: King
  Athelstan and Ealdorman Ealhhere fought in ships and slew a great army at Sandwich, and took nine ships: the
  first sea fight in the Chronicle. ASC 1006, 1009 (the great fleet gathered at Sandwich), 1013 (Swein), 1014
  (Cnut puts his father's hostages ashore there, maimed), 1040, 1044-1052 (Edward's fleet). The Encomium Emmae
  (c. 1041) calls it the most famous of all the ports of the English. DB Kent: 307 houses TRE, 383 in 1086; it
  owes the king the same ship service as Dover.
- **Sources**: ASC. Encomium Emmae Reginae (ed. Campbell 1949). DB Kent. H. Clarke and others, Sandwich: the
  completest medieval town in England (2010). https://en.wikipedia.org/wiki/Sandwich,_Kent

```json
{"id": "sandwic", "name": "Sandwic", "modern": "Sandwich", "lat": 51.275, "lon": 1.34, "realm": "kent", "county": "", "region": "england_south", "kind": "market", "landing": "beach", "size": 3, "from": 0, "to": 0, "note": "The haven at the south mouth of the Wantsum: a landing in 793, the king's fleet station from 1009, 'the most famous port of the English' by 1040. Herring and passage."},
```

## 3. Norðwic (Norwich)

- **Name then**: Norðwic (NORVIC on coins of Æthelstan).
- **Modern place**: Norwich, Norfolk.
- **Lat, lon**: 52.630, 1.300.
- **Active**: several small settlements on the Wensum in 793 (not worth a row); a town from c. 900; one of the four
  or five largest in England by 1066.
- **Kind**: town and mint, with a strong Danish share.
- **Who ruled**: East Anglia to 869; the Danes of East Anglia (Guthrum from 880) to 917; the English kings. Sacked
  by Swein in 1004. Earls of East Anglia in the 11th century (Thorkell, Harold, Ælfgar, Gyrth).
- **Traded**: wool and cloth, grain, herring from the estuary, pots (Thetford-type ware made in the town), leather,
  ironwork. In: Rhenish lava querns, Norwegian hones, wine, walrus ivory (?).
- **How a boat comes in**: from the sea by the Yare. The great estuary behind the sandbank at Yarmouth is still
  wide tidal water in this period; then the Yare and the Wensum, twenty miles, to gravel hards at Fye Bridge and
  along King Street (Conesford, "the king's ford"). Riverbank landing; sea ships come right up.
- **Why a merchant goes**: the market of the richest and most peopled shire in England, straight across from
  Frisia and Jutland. Danish is understood. A mint from the 930s.
- **Evidence**: coins of Æthelstan (924-939) with the town's name. ASC E 1004: Swein came with his fleet to
  Norwich, and plundered and burned the whole borough. DB Norfolk: 1,320 burgesses TRE. Churches of St Olave and
  St Clement, saints the Danes favoured. Digs: the Anglia TV site, Fishergate, the castle site (11th-century houses
  cleared for the castle); kilns in Pottergate.
- **Sources**: ASC. DB Norfolk. B. Ayers, Norwich: archaeology of a fine city (2009). J. Campbell, "Norwich", in
  Historic Towns Atlas II (1975). https://en.wikipedia.org/wiki/History_of_Norwich

```json
{"id": "northwic", "name": "Norðwic", "modern": "Norwich", "lat": 52.63, "lon": 1.3, "realm": "eastanglia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 4, "from": 900, "to": 0, "note": "The Danes' and East Angles' town on the Wensum, up the Yare from the sea: a mint from the 930s, burned by Swein in 1004, 1,320 burgesses by 1066."},
```

## 4. Lindcylene (Lincoln)

- **Name then**: Lindcylene (ASC 942), Lincolia; Bede's Lindocolina.
- **Modern place**: Lincoln; the landing is Brayford Pool and the suburb of Wigford.
- **Lat, lon**: 53.228, -0.545 (Brayford Pool).
- **Active**: a walled ruin with a church in 793; a town again from c. 880 under the Danes; great from c. 950.
- **Kind**: town; one of the Five Boroughs; a leading mint.
- **Who ruled**: Lindsey under Mercia in 793. The Danish army from the 870s. It comes to the English kings
  c. 918-920 (?), is under the Norse kings of York for a time, and is "redeemed" by Edmund in 942 with Leicester,
  Nottingham, Stamford and Derby (ASC 942, in verse). Twelve lawmen hold the town's law (DB).
- **Traded**: wool and cloth; pots (Lincoln shelly wares); glass and copper-alloy work made in the Flaxengate
  workshops; grain; salt and fish from the fen edge. In: silk (a piece from Lincoln is said to match one from
  York, cut from the same cloth (?)), querns, hones, wine, silver.
- **How a boat comes in**: two ways. From the Wash up the Witham, some forty miles of slow fen river, to Brayford
  Pool below the hill. Or from the Humber up the Trent to Torksey and through the Foss Dyke, the Roman cut, eleven
  miles to the same pool (whether the cut was open all through these years is not known (?)). The pool is a broad
  still water with shelving banks: riverbank landing.
- **Why a merchant goes**: the Danes' chief town south of the Humber, with silver to spend and goods from York and
  the east. The short way between the Trent and the Wash.
- **Evidence**: Bede, HE II.16 (Paulinus and the reeve Blæcca). The "St Martin of Lincoln" pennies, c. 920s. ASC
  942. DB Lincolnshire: 1,150 inhabited houses TRE (?). Digs at Flaxengate (houses and workshops from the late 9th
  century), Wigford and the waterfront.
- **Sources**: ASC. DB. D. Perring, Early Medieval Occupation at Flaxengate (1981). M. Jones, D. Stocker and
  A. Vince, The City by the Pool (2003). https://en.wikipedia.org/wiki/History_of_Lincoln,_England

```json
{"id": "lindcylene", "name": "Lindcylene", "modern": "Lincoln (Brayford Pool)", "lat": 53.228, "lon": -0.545, "realm": "mercia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 4, "from": 880, "to": 0, "note": "The Danes' borough on the hill above the pool, reached by the Witham from the Wash or by the Foss Dyke from the Trent; a great mint from the 920s."},
```

## 5. Turecesieg (Torksey)

- **Name then**: Turecesieg, Turcesige (ASC 873): "Turoc's island" (?).
- **Modern place**: Torksey, Lincolnshire.
- **Lat, lon**: 53.299, -0.745.
- **Active**: winter 872-873 as the Great Army's camp; then a potters' and boatmen's town to 1066 and after.
- **Kind**: 872-873 a camp-market of thousands; after that a small river town at a canal mouth.
- **Who ruled**: Lindsey (Mercia) to 872; the army; the Danes of Lincoln; the English kings from c. 920. Queen
  Edith holds it TRE (DB (?)).
- **Traded**: in the camp: silver by weight (hacksilver, hackgold, dirhams cut small), loot, slaves, metal melted
  down, gaming. Later: Torksey ware pots, sold from York to the Midlands; carriage of goods between Trent and
  Lincoln; grain.
- **How a boat comes in**: up the Humber and the tidal Trent, about forty miles from the Humber mouth; the tide and
  the bore (the aegir) carry a boat most of the way. Land on the east bank on a sandy rise above the flood plain,
  just north of where the Foss Dyke leaves the river. Riverbank.
- **Why a merchant goes**: in 872-873, the best and most dangerous market in England: an army with silver and
  nothing to eat. After, the turn for Lincoln, and pots by the boatload.
- **Evidence**: ASC A 873 (true 872-873): the army took winter quarters at Torksey in Lindsey. The camp found by
  metal detecting and survey: about 55 hectares on a bluff that was an island in winter; more than a thousand
  pieces of metalwork, among them hundreds of coins (English pennies, Northumbrian stycas, over a hundred
  dirhams (?)), weights, hacksilver, ingots, lead gaming pieces. Torksey ware made near by from the late 9th
  century. DB Lincolnshire: 213 burgesses TRE; when the king's messengers came, the men of the town took them to
  York in their own ships (?).
- **Sources**: ASC. D. Hadley and J. Richards, "The winter camp of the Viking Great Army, AD 872-3, Torksey,
  Lincolnshire", Antiquaries Journal 96 (2016). https://en.wikipedia.org/wiki/Torksey (fetched 6 Oct 2026: the
  place, the 55 hectares, "over one thousand items", the pottery date, the Foss Dyke).

```json
{"id": "turecesieg", "name": "Turecesieg", "modern": "Torksey", "lat": 53.299, "lon": -0.745, "realm": "mercia", "county": "", "region": "england_east", "kind": "market", "landing": "riverbank", "size": 2, "from": 872, "to": 0, "note": "Where the Foss Dyke leaves the Trent: the Great Army's winter camp of 872-873, silver by weight; afterwards a town of potters and boatmen."},
```

## 6. Þeodford (Thetford)

- **Name then**: Þeodford, Theodford: "the people's ford".
- **Modern place**: Thetford, Norfolk.
- **Lat, lon**: 52.412, 0.745.
- **Active**: 869 (the army's winter); a large town from c. 900 to 1066; it shrinks after 1100.
- **Kind**: town, mint and pottery centre; in 1066 the sixth town of England or so.
- **Who ruled**: East Anglia to 869; the Danes; the English kings from 917. Burned by Swein 1004 and by Thorkell's
  army 1010. The bishop's seat 1071-1094 (after the game).
- **Traded**: Thetford ware (hard grey wheel-made pots, sold all over East Anglia); iron and bronze work; wool,
  cloth, grain. In: querns, hones, salt, herring, wine.
- **How a boat comes in**: a long river road. From the Wash by the Great Ouse (its mouth then at Wisbech (?)) and
  the Little Ouse, some fifty miles through the fens, to the fords where the Thet joins. Shallow and slow: right
  for the Gislinge boat, wrong for a ship. Riverbank.
- **Why a merchant goes**: a big inland market where the Icknield Way crosses the river; pots at the kiln's price;
  East Anglia's news.
- **Evidence**: ASC A 870 (true 869): the army took winter quarters at Thetford; that winter King Edmund fought
  them, the Danes had the victory and slew the king. ASC E 1004, 1010. DB Norfolk: 943 burgesses TRE (the page
  fetched says 944). A mint: the page fetched says "9th century", which I doubt; coins are sure from Edgar's reform
  (c. 973) on (?). Digs by Knocker (1948-52), Davison (1960s) and Dallas: a town of some 60-75 hectares (?)
  within a bank and ditch, with streets and kilns.
- **Sources**: ASC. DB. A. Rogerson and C. Dallas, Excavations in Thetford 1948-59 and 1973-80 (1984).
  https://en.wikipedia.org/wiki/Thetford (fetched 6 Oct 2026).

```json
{"id": "theodford", "name": "Þeodford", "modern": "Thetford", "lat": 52.412, "lon": 0.745, "realm": "eastanglia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 4, "from": 869, "to": 0, "note": "Where the army wintered in 869 and King Edmund was killed; then a great town of potters far up the Little Ouse. Burned 1004 and 1010."},
```

## 7. Brycgstow (Bristol)

- **Name then**: Brycgstow: "the place at the bridge".
- **Modern place**: Bristol.
- **Lat, lon**: 51.455, -2.592.
- **Active**: from c. 1000 (first coins c. 1010-1020). Nothing is known of it before; it should not be on the
  chart in 793.
- **Kind**: a new burh and port on the Irish trade.
- **Who ruled**: the English kings; in the earldom of Swein Godwinson in the 1040s, then of Harold (?).
- **Traded**: slaves, English and Welsh, to Dublin and Waterford: the trade Bishop Wulfstan of Worcester
  (1062-1095) preached down. Also cloth, grain, iron from the Forest of Dean. In from Ireland: marten skins (?),
  hides, Dublin silver. From the south: wine.
- **How a boat comes in**: from the Severn Sea into the Avon mouth and seven miles up through the gorge to where the
  Frome joins. The tide here is among the greatest in the world, some twelve metres at the river mouth on springs:
  a boat goes up on the flood and sits in the mud at low water. Ships wait for the tide in the road off the
  mouth. Riverbank.
- **Why a merchant goes**: the shortest English crossing to the Norse towns of Ireland; a market where questions are
  not asked. A card place: the roped lines of the young for sale.
- **Evidence**: a penny of Æthelred II of c. 1010 with the mint name (the page fetched also says "a mint by the
  10th century", which I doubt (?)). ASC D,E 1051: Harold and Leofwine go to Bristol, take the ship Earl Swein had
  made ready, and cross to Ireland (the page fetched gives 1052). ASC D 1063: Harold sails from Bristol round Wales
  (the page gives 1062). ASC D 1067: Harold's sons come from Ireland into the Avon and attack the town; the
  townsmen fight them off. William of Malmesbury, Life of Wulfstan II.20, on the slave market.
- **Sources**: ASC. William of Malmesbury, Vita Wulfstani (ed. Darlington 1928). D. Pelteret, Slavery in Early
  Mediaeval England (1995). https://en.wikipedia.org/wiki/History_of_Bristol (fetched 6 Oct 2026).

```json
{"id": "brycgstow", "name": "Brycgstow", "modern": "Bristol", "lat": 51.455, "lon": -2.592, "realm": "wessex", "county": "", "region": "mercia_west", "kind": "town", "landing": "riverbank", "size": 3, "from": 1000, "to": 0, "note": "A new town at the bridge on the Avon, up the gorge on a great tide: the crossing to Dublin, and the slave market Bishop Wulfstan preached against."},
```

## 8. Gleawanceaster (Gloucester)

- **Name then**: Gleawanceaster, Gleaweceaster (ÆT GLEAPA on a coin of Alfred).
- **Modern place**: Gloucester.
- **Lat, lon**: 51.867, -2.251.
- **Active**: St Peter's minster in the Roman walls from c. 679; a burh and market from c. 900.
- **Kind**: Mercian royal town; under Edward the Confessor a place where the king holds court at Christmas (?).
- **Who ruled**: the Hwicce under Mercia in 793. Æthelred and Æthelflæd of the Mercians refound it c. 900 and are
  buried there (911, 918). The English kings from 918.
- **Traded**: iron from the Forest of Dean (the town's render to the king TRE: 36 dicres of iron and 100 rods of
  iron drawn out for nails for the king's ships, with 12 sesters of honey; DB); salt from Droitwich coming down
  river; grain, salmon and lampreys. In: wine, Welsh cattle and slaves.
- **How a boat comes in**: up the Severn estuary on the flood, past the sands, with the bore on spring tides; the
  town is at the lowest crossing. A quay on the old east channel below the walls. A hard pilotage: the sandbanks
  move.
- **Why a merchant goes**: ship nails and iron at the source; the Mercian court; the saint (Oswald's bones, brought
  from Bardney in 909).
- **Evidence**: ASC (Mercian Register) 909, 918. Æthelweard on the Danish army at Gloucester in 877. The Alfred
  coin. DB Gloucestershire. Digs at St Oswald's priory (Heighway and Bryant).
- **Sources**: ASC. DB. C. Heighway and R. Bryant, The Golden Minster (1999).
  https://en.wikipedia.org/wiki/History_of_Gloucester

```json
{"id": "gleawanceaster", "name": "Gleawanceaster", "modern": "Gloucester", "lat": 51.867, "lon": -2.251, "realm": "mercia", "county": "", "region": "mercia_west", "kind": "town", "landing": "quay", "size": 3, "from": 0, "to": 0, "note": "The lowest crossing of the Severn, in Roman walls: a minster in 793, Æthelflæd's burh from c. 900. Iron and ship nails from the Forest of Dean."},
```

## 9. Exanceaster (Exeter)

- **Name then**: Exanceaster.
- **Modern place**: Exeter; the sea landing is Topsham, four miles down.
- **Lat, lon**: 50.719, -3.534.
- **Active**: a minster in the Roman walls before the game (Boniface was schooled there c. 680); a burh and mint
  from Alfred's time; a rich town by 1000.
- **Kind**: walled town, mint, from 1050 a bishop's seat.
- **Who ruled**: Wessex, then the English kings. Held by the Danish army 876-877; besieged 893. Æthelstan drove out
  the Britons and rebuilt the walls c. 927 (William of Malmesbury (?)). Stormed by Swein 1003. Queen Emma, then
  Queen Edith, hold it as dower. Bishop Leofric moves his seat from Crediton in 1050 and leaves the church the
  book of English verse and riddles now called the Exeter Book.
- **Traded**: tin from Dartmoor and Cornwall (the working is thin in the evidence for these years (?)); wool,
  hides, cloth; fish. In: wine from the Loire and the Seine, salt, pots from northern France.
- **How a boat comes in**: into the Exe mouth past the bar and the Warren sand, up a wide tidal estuary to Topsham,
  then on the top of the tide to the bank below the west gate. No weir closes the river until the 13th century.
- **Why a merchant goes**: the westernmost English town of any size; tin; the Breton and Norman trade; a bishop who
  owns books (the riddles in the game come from his book).
- **Evidence**: ASC A 876, 877, 893. ASC E 1003: Exeter stormed "because of the French churl Hugh, whom the Lady
  had set there as reeve". Coins of Alfred with EXA. II Æthelstan (the Grately code, c. 930): two moneyers at
  Exeter. Leofric's list of gifts. DB Devon.
- **Sources**: ASC. F. Attenborough, Laws of the Earliest English Kings (1922), II Æthelstan 14. J. Allan,
  C. Henderson and R. Higham, "Saxon Exeter", in Anglo-Saxon Towns in Southern England (1984).
  https://en.wikipedia.org/wiki/History_of_Exeter

```json
{"id": "exanceaster", "name": "Exanceaster", "modern": "Exeter", "lat": 50.719, "lon": -3.534, "realm": "wessex", "county": "", "region": "england_south", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The western city on the Exe: a minster in Roman walls in 793, Alfred's burh and mint, stormed by Swein in 1003, a bishop's seat from 1050. Tin and wine."},
```

## 10. Wintanceaster (Winchester)

- **Name then**: Wintanceaster; Wintonia.
- **Modern place**: Winchester.
- **Lat, lon**: 51.061, -1.311.
- **Active**: all through the game; a bishop's church and royal hall in 793, a planned town from c. 880.
- **Kind**: the royal city of Wessex and then of the English: the treasury, three minsters, six moneyers.
- **Who ruled**: Wessex; the English kings. Stormed by a ship-army in 860 (ASC). The streets are laid out in
  Alfred's time; Edward the Elder founds the New Minster in 901. Cnut is buried there in 1035; Queen Emma keeps
  the treasure there until Edward takes it in 1043 (ASC).
- **Traded**: it buys more than it sells: wine, silk, spices, wax, furs, walrus ivory, gold and glass for three
  great churches and the court. It sells coin, fine metalwork, books and embroidery, wool and cloth.
- **How a boat comes in**: it is twelve miles up the Itchen from Hamwic. Whether the Itchen carried boats in these
  years is not known (?): the Gislinge boat might be poled up at a good height of water; a ship never. The sure
  way is to lie at Hamwic or Hamtun and go up by road with pack horses.
- **Why a merchant goes**: the best buyers in England, and the king. A charter or a court judgment is had here.
- **Evidence**: ASC A 860, 1035, 1043. II Æthelstan 14: six moneyers at Winchester (London eight, Canterbury
  seven). The digs of 1961-71 (Biddle): the street grid of c. 880-900, the Old Minster, tenements. The survey of
  the king's lands in the town made for Edward c. 1057 (in the Winton Domesday).
- **Sources**: ASC. M. Biddle (ed.), Winchester in the Early Middle Ages (1976).
  https://en.wikipedia.org/wiki/History_of_Winchester
- **Note for the game**: it could be a second panel of `hamwic` (a "go up to the city" act, a day each way) in
  place of a port of its own. It is ranked must have because the court is a place the story needs.

```json
{"id": "wintanceaster", "name": "Wintanceaster", "modern": "Winchester", "lat": 51.061, "lon": -1.311, "realm": "wessex", "county": "", "region": "england_south", "kind": "royal seat", "landing": "riverbank", "size": 4, "from": 0, "to": 0, "note": "The royal city of Wessex, twelve miles up the Itchen from Hamwic (by road; a small boat perhaps (?)): the king's treasure, three minsters, six moneyers."},
```

---

# GOOD TO HAVE

## 11. Cantwaraburh (Canterbury)

- **Name then**: Cantwaraburh; Dorobernia in the charters.
- **Modern place**: Canterbury.
- **Lat, lon**: 51.280, 1.080.
- **Active**: all through the game.
- **Kind**: the archbishop's city; the second mint of England (seven moneyers c. 930).
- **Who ruled**: Kent under Mercia 793-825, then Wessex; the archbishop and the two great houses (Christ Church,
  St Augustine's) hold most of it. Stormed 851 (ASC); besieged and taken 1011, Archbishop Ælfheah carried off and
  killed at Greenwich in 1012.
- **Traded**: coin; books and church goods; grain and wool from the church lands. It buys wine, wax, oil, silk,
  incense, lead and glass.
- **How a boat comes in**: it does not. The landing is Fordwic, two miles down the Stour, already in the table; from
  c. 1000 heavier ships stop at Sandwic.
- **Why a merchant goes**: the archbishop's reeve buys the dearest goods; pilgrims; letters to carry to Rome and the
  Franks.
- **Evidence**: ASC 851, 1011, 1012. II Æthelstan 14 (seven moneyers: four the king's, two the bishop's, one the
  abbot's). A charter of c. 860 (S 1199 (?)) names a company of "cnihtas" in the city among its witnesses, the
  oldest such body in England; do not call it a guild in the game before c. 1000 (BRIEF rule 2).
- **Sources**: ASC. N. Brooks, The Early History of the Church of Canterbury (1984).
  https://en.wikipedia.org/wiki/History_of_Canterbury
- **Note**: better as a second panel reached from `fordwic` than as a port. The row is here if it is wanted.

```json
{"id": "cantwaraburh", "name": "Cantwaraburh", "modern": "Canterbury", "lat": 51.28, "lon": 1.08, "realm": "kent", "county": "", "region": "england_south", "kind": "town", "landing": "riverbank", "size": 4, "from": 0, "to": 0, "note": "The archbishop's city and the second mint of England, two miles above its landing at Fordwic. Stormed 851, taken 1011."},
```

## 12. Steanford (Stamford)

- **Name then**: Steanford (ASC 918): "the stone ford".
- **Modern place**: Stamford, Lincolnshire.
- **Lat, lon**: 52.651, -0.480.
- **Active**: c. 880 to 1066 and after.
- **Kind**: one of the Five Boroughs; potters' and smiths' town.
- **Who ruled**: Mercia to 877; a Danish army's borough; Edward the Elder takes it and builds a second burh south
  of the river in 918 (ASC A); the English kings; lawmen hold its law (DB).
- **Traded**: Stamford ware: fine pale pots with a clear yellow or green lead glaze, the first glazed ware made in
  England since the Romans, from c. 900 (?), sold from York to London. Iron smelted from the local ore. Wool.
- **How a boat comes in**: from the Wash up the Welland through the fens, about thirty-five miles, to the ford
  where the great road north crosses. Shallow: a small boat only. Riverbank.
- **Why a merchant goes**: glazed pitchers that sell anywhere a lord dines; iron.
- **Evidence**: ASC A 918, 942. Kilns dug in the town; the ware found across England. A mint from Edgar's
  time (?). DB Lincolnshire: 141 houses in five wards TRE (?).
- **Sources**: K. Kilmurry, The Pottery Industry of Stamford, Lincolnshire, c. AD 850-1250 (1980). C. Mahany and
  D. Roffe, "Stamford: the development of an Anglo-Scandinavian borough", Anglo-Norman Studies 5 (1983).
  https://en.wikipedia.org/wiki/Stamford,_Lincolnshire

```json
{"id": "steanford", "name": "Steanford", "modern": "Stamford", "lat": 52.651, "lon": -0.48, "realm": "mercia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 3, "from": 880, "to": 0, "note": "A Danish borough at the stone ford of the Welland: glazed Stamford pots and iron. Edward's burh of 918 stands across the river."},
```

## 13. Hrofesceaster (Rochester)

- **Name then**: Hrofesceaster; Hrofi civitas.
- **Modern place**: Rochester, Kent.
- **Lat, lon**: 51.389, 0.503.
- **Active**: all through the game.
- **Kind**: small bishop's city in Roman walls, with a mint and the Medway bridge.
- **Who ruled**: Kent under Mercia, then Wessex; the bishop. "Great slaughter" there in 842; besieged in 885 by an
  army from Francia, which built its own fort before the gate until Alfred drove it to its ships (ASC A 885).
  Æthelred II ravaged the bishop's lands in 986; the Danes came up the Medway in 999.
- **Traded**: grain, salt from the marshes, oysters and fish, Kentish ragstone (quarried up river near Maidstone
  and sent to London (?)); coin.
- **How a boat comes in**: from the Thames mouth round the Isle of Grain or through the Swale, twelve miles up the
  tidal Medway to the hard below the bridge. Good shelter; strong tide.
- **Why a merchant goes**: a safe night between Sandwic and London; the bishop buys; stone for builders.
- **Evidence**: ASC 842, 885, 986, 999. II Æthelstan 14: three moneyers (two the king's, one the bishop's).
  The list of who mends which pier of the bridge (11th century (?)).
- **Sources**: ASC. N. Brooks, "Rochester Bridge, AD 43-1381", in Traffic and Politics (1994).
  https://en.wikipedia.org/wiki/Rochester,_Kent

```json
{"id": "hrofesceaster", "name": "Hrofesceaster", "modern": "Rochester", "lat": 51.389, "lon": 0.503, "realm": "kent", "county": "", "region": "england_south", "kind": "town", "landing": "beach", "size": 2, "from": 0, "to": 0, "note": "A bishop's small city in Roman walls at the Medway bridge; three moneyers. Besieged by an army from Francia in 885."},
```

## 14. Mældun (Maldon)

- **Name then**: Mældun: "the hill with the cross" (?).
- **Modern place**: Maldon, Essex; the landing is the Hythe.
- **Lat, lon**: 51.732, 0.676.
- **Active**: 916 to 1066 and after.
- **Kind**: burh, small port and mint at the head of the Blackwater.
- **Who ruled**: Essex under the Danes of East Anglia until Edward the Elder; he camps there in 912 and builds the
  burh in 916 (ASC A); the English kings. Ealdorman Byrhtnoth of Essex dies there in August 991.
- **Traded**: salt (the Blackwater shore is lined with salterns in DB), oysters, fish, wool, cheese from the marsh
  sheep.
- **How a boat comes in**: ten miles up the Blackwater from the sea, a wide shallow estuary of mud and saltings;
  come in on the flood past Northey Island, whose causeway dries at low water. Beach on the Hythe.
- **Why a merchant goes**: salt at the pan; a market for a small boat. After 991, the place where the first great
  tribute was agreed: 10,000 pounds.
- **Evidence**: ASC A 912, 916, 917 (besieged by the Danes and relieved); ASC A,E 991. The poem The Battle of
  Maldon. DB Essex: the king has 180 houses held by burgesses; the town finds a horse for the army and a ship (?).
- **Sources**: ASC. D. Scragg (ed.), The Battle of Maldon AD 991 (1991). https://en.wikipedia.org/wiki/Maldon

```json
{"id": "maeldun", "name": "Mældun", "modern": "Maldon", "lat": 51.732, "lon": 0.676, "realm": "wessex", "county": "", "region": "england_east", "kind": "market", "landing": "beach", "size": 2, "from": 916, "to": 0, "note": "Edward's burh of 916 at the head of the Blackwater: salt, oysters, a small mint. Byrhtnoth fell here in 991."},
```

## 15. Werham (Wareham)

- **Name then**: Werham.
- **Modern place**: Wareham, Dorset.
- **Lat, lon**: 50.686, -2.110.
- **Active**: a minster before the game (King Beorhtric of Wessex is buried there in 802); a burh from c. 880.
- **Kind**: walled burh with a nunnery, two moneyers; the port of Dorset.
- **Who ruled**: Wessex; the English kings. The Danish army slips into it in 876 and swears peace on the holy ring
  (ASC A 876). Edward the Martyr, killed at Corfe in 978, is buried there without a king's honours and moved the
  next year.
- **Traded**: Purbeck stone (?), salt and fish from Poole Harbour, wool, grain. In: wine and pots from Normandy
  and the Seine.
- **How a boat comes in**: through the narrow mouth of Poole Harbour, where the tide runs hard, across four miles of
  shallow harbour by the channel, and a mile up the Frome to the bank under the south wall. Asser says it is the
  safest of places but for the west side, where it joins the land.
- **Why a merchant goes**: the Channel crossing to the Cotentin and the Seine; a walled night.
- **Evidence**: ASC 802 (told under 784), 876, 978-979. Asser, Life of Alfred ch. 49. The Burghal Hidage (1,600
  hides). II Æthelstan 14 (two moneyers). The earth walls still stand on three sides.
- **Sources**: ASC. Asser (ed. Stevenson 1904). D. Hill and A. Rumble, The Defence of Wessex (1996).
  https://en.wikipedia.org/wiki/Wareham,_Dorset

```json
{"id": "werham", "name": "Werham", "modern": "Wareham", "lat": 50.686, "lon": -2.11, "realm": "wessex", "county": "", "region": "england_south", "kind": "town", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "A minster, then Alfred's walled burh between the Frome and the Piddle at the head of Poole Harbour. The army lay here in 876."},
```

## 16. Oxnaford (Oxford)

- **Name then**: Oxnaford.
- **Modern place**: Oxford.
- **Lat, lon**: 51.752, -1.257.
- **Active**: c. 900 to 1066 and after (St Frideswide's minster is older (?)).
- **Kind**: burh and river town on the border of Mercia and Wessex; a meeting place for the king's council.
- **Who ruled**: the Mercians under Æthelred; Edward the Elder takes "London and Oxford and the lands that
  belonged to them" in 911 (ASC A). The Danes of the town are killed on St Brice's day 1002 and St Frideswide's
  burned over them (the king's own charter of 1004, S 909). Burned by the army 1009. Cnut's peace of 1018 is sworn
  there.
- **Traded**: wool, grain, cloth, leather; Thames fish. In: herring, salt, wine, querns, by boat from London.
- **How a boat comes in**: up the Thames above London, about a hundred miles of river with fords, shallows and
  mill weirs; rowing, poling and towing. Days, not hours. Riverbank.
- **Why a merchant goes**: the farthest a boat can take sea goods into the middle of England.
- **Evidence**: ASC 911, 1009, 1013, 1018, 1065. S 909. The Burghal Hidage. The Abingdon chronicle: in Abbot
  Ordric's time (1052-1066) the Oxford boatmen asked for a new cut past the abbey, and each boat passing paid the
  cellarer a hundred herrings in Lent (?): proof that herring came up by boat.
- **Sources**: ASC. J. Blair, Anglo-Saxon Oxfordshire (1994). Historia Ecclesie Abbendonensis (ed. Hudson 2002-7).
  https://en.wikipedia.org/wiki/History_of_Oxford

```json
{"id": "oxnaford", "name": "Oxnaford", "modern": "Oxford", "lat": 51.752, "lon": -1.257, "realm": "mercia", "county": "", "region": "england_south", "kind": "town", "landing": "riverbank", "size": 3, "from": 900, "to": 0, "note": "A burh at the oxen's ford, a hundred river miles above London: as far as a boat takes herring into England. Burned 1009."},
```

## 17. Grantebrycg (Cambridge)

- **Name then**: Grantebrycg, Grantanbrycg. (Bede's Grantacæstir, "a little ruined city", 695.)
- **Modern place**: Cambridge.
- **Lat, lon**: 52.209, 0.117.
- **Active**: 875 to 1066 and after.
- **Kind**: Danish army borough, then an English shire town and mint at the edge of the fens.
- **Who ruled**: Mercia or East Anglia in 793 (?). Three Danish kings, Guthrum, Oscytel and Anwend, sit there a
  year from 875 (ASC A). Its army gives in to Edward the Elder in 917. Burned 1010.
- **Traded**: grain, eels and fish and wildfowl from the fens, wool. In: cloaks and cloth from Ireland, querns,
  wine, salt.
- **How a boat comes in**: from the Wash by the Ouse and the Cam, some forty miles of fen river, past Ely on its
  island. Slow, sheltered, easy to lose the channel in flood. Riverbank below the bridge.
- **Why a merchant goes**: the fen abbeys (Ely, Ramsey, Thorney, refounded c. 970) buy what a church needs and sell
  eels by the thousand.
- **Evidence**: ASC A 875, 917; ASC 1010. The Liber Eliensis (II.32 (?)) tells of Irish merchants who came to
  Cambridge in King Edgar's time with cloaks and other goods and were robbed by a priest (?). A mint from Edgar.
  DB: ten wards, about 400 houses TRE (?).
- **Sources**: ASC. Liber Eliensis (ed. Blake 1962; tr. Fairweather 2005).
  https://en.wikipedia.org/wiki/History_of_Cambridge

```json
{"id": "grantebrycg", "name": "Grantebrycg", "modern": "Cambridge", "lat": 52.209, "lon": 0.117, "realm": "eastanglia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 2, "from": 875, "to": 0, "note": "The bridge on the Granta at the fen edge: three Danish kings sat here in 875; later a shire town where Irish merchants sold cloaks."},
```

## 18. Weogornaceaster (Worcester)

- **Name then**: Weogornaceaster, Wigraceaster.
- **Modern place**: Worcester.
- **Lat, lon**: 52.189, -2.221.
- **Active**: the bishop's church of the Hwicce all through the game; a burh and market from c. 890.
- **Kind**: bishop's town on the Severn; the salt road's river head.
- **Who ruled**: Mercia; the bishop holds half the lordship of the town by the charter below; the English kings.
  In 1041 Harthacnut has the town burned for killing two of his tax men (ASC C 1041; John of Worcester).
- **Traded**: salt from Droitwich (Saltwic), seven miles off: the brine salt the goods table already has. Grain,
  hides, books from the bishop's writing house. In: wine, wax, lead for the salt pans.
- **How a boat comes in**: thirty river miles above Gloucester on the Severn; the tide is felt almost to the town
  on big springs, the rest is rowing and towing. Riverbank below the church.
- **Why a merchant goes**: salt at nearly the pan's price; a careful bishop who writes every bargain down.
- **Evidence**: the charter of Ealdorman Æthelred and Æthelflæd for Bishop Wærferth (S 223, c. 889-899): they
  have the burh built "for the shelter of all the people", and give the church half their rights in the market
  and the street, the fines for fighting, theft and bad dealing, and the wall money; the king keeps the
  wagon-shilling and the load-penny at Saltwic. It is the best single text on how a burh market worked. London
  plot S 346 (889) is this same bishop's.
- **Sources**: S 223, in F. Harmer, Select English Historical Documents (1914), no. 13. N. Baker and R. Holt,
  Urban Growth and the Medieval Church: Gloucester and Worcester (2004).
  https://en.wikipedia.org/wiki/History_of_Worcester

```json
{"id": "weogornaceaster", "name": "Weogornaceaster", "modern": "Worcester", "lat": 52.189, "lon": -2.221, "realm": "mercia", "county": "", "region": "mercia_west", "kind": "town", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The bishop's town on the Severn above Gloucester, a burh from c. 890: the river head for Droitwich salt. Burned by Harthacnut's men in 1041."},
```

## 19. Meols (the Wirral beach market)

- **Name then**: not written down in these years. The name is Norse melr, "sandbank"; DB has Melas (?).
- **Modern place**: Meols, Wirral; the old shore is under the sea off Dove Point.
- **Lat, lon**: 53.401, -3.155.
- **Active**: before the game (a landing since the Iron Age) to 1066 and after; busiest c. 900-1050.
- **Kind**: a beach market with no lord's wall and no mint.
- **Who ruled**: Mercia in 793. From c. 902 the north Wirral is settled by Norse out of Ireland under Ingimund,
  with leave from Æthelflæd (the Fragmentary Annals of Ireland; a late telling (?)); their meeting place is
  Thingwall. Under the English kings and the earls of Mercia after, in law at least.
- **Traded**: small wares: ringed pins and brooches of the Irish Sea Norse kind, knives, beads, lead and
  pewter (?). Through it: slaves, hides, Cheshire salt, grain out; Dublin silver in.
- **How a boat comes in**: a sandy bay behind the Hoyle Bank at the corner of the Dee and Mersey mouths. Run up on
  the sand at half tide and wait for the ebb. Open to the north-west.
- **Why a merchant goes**: to trade with Dublin men without paying the reeve at Chester.
- **Evidence**: more than three thousand finds picked off the eroding shore since c. 1810, from the Iron Age to
  the 15th century, among them Norse pins and 10th- and 11th-century pennies (?). The Norse place names of Wirral.
  A clinker boat lies buried under the Railway Inn car park, found in 1938, age not yet known.
- **Sources**: D. Griffiths, R. Philpott and G. Egan, Meols: The Archaeology of the North Wirral Coast (2007).
  https://en.wikipedia.org/wiki/Meols (fetched 6 Oct 2026: the place, the finds since c. 1810, the boat).

```json
{"id": "meols", "name": "Meols", "modern": "Meols, Wirral", "lat": 53.401, "lon": -3.155, "realm": "mercia", "county": "", "region": "mercia_west", "kind": "market", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "An old beach market behind the Hoyle Bank on the tip of Wirral; from c. 902 among Norse settlers out of Ireland. No reeve, no wall."},
```

## 20. Llanbedrgoch (Anglesey)

- **Name then**: not known (?). The island is Môn to the Welsh and Önguls-ey to the Norse; the bay is Traeth Coch.
- **Modern place**: Llanbedrgoch, above Red Wharf Bay, Anglesey.
- **Lat, lon**: 53.309, -4.233 (?) (the dig is in a field near the village; the landing is the bay, a kilometre
  north-east).
- **Active**: a farm from c. 600; a walled trading place c. 850-1000 (?).
- **Kind**: an enclosed farmstead of a lord of Gwynedd, or of Norse incomers, that weighed silver and worked metal.
- **Who ruled**: Gwynedd: Rhodri Mawr (844-878), who killed the Danish chief Horm in 856; Anarawd; Hywel Dda's
  line and Gwynedd's own by turns. Anglesey is raided again and again from Dublin and Man: in 987 Godfrey
  Haraldsson takes two thousand captives (Annales Cambriae; Brut y Tywysogion). Whether Norsemen ever held this
  place is argued (?).
- **Traded**: grain (Anglesey is the granary of Gwynedd), cattle, hides, slaves; silver by weight; bronze work
  made on the spot. In: Dublin goods, salt, wine (?).
- **How a boat comes in**: into Red Wharf Bay on the east coast of Anglesey, a wide bay that dries to firm sand for
  two miles: ground on the sand and walk up the hill. Sheltered from the west, open to the north-east.
- **Why a merchant goes**: the only known market on the Welsh coast in these years, halfway between Chester and
  Dublin. Grain and hides for weighed silver.
- **Evidence**: found by metal detecting in 1992; dug 1994-2001 by the National Museum of Wales (M. Redknap): a
  ditch and bank rebuilt in the 9th century as a stone wall some two metres thick round about a hectare; halls;
  hacksilver, ingots, lead weights of Norse kinds, Carolingian and English pennies (?); five bodies thrown in the
  ditch in the later 10th century (?).
- **Sources**: M. Redknap, Vikings in Wales: An Archaeological Quest (2000); and "Viking-age settlement in Wales
  and the evidence from Llanbedrgoch", in Land, Sea and Home (2004). The page
  https://en.wikipedia.org/wiki/Llanbedrgoch was fetched 6 Oct 2026 and has nothing on the dig: every detail
  above is from memory of Redknap and is (?) until checked against the museum's pages.

```json
{"id": "llanbedrgoch", "name": "Llanbedrgoch", "modern": "Llanbedrgoch, Red Wharf Bay, Anglesey", "lat": 53.309, "lon": -4.233, "realm": "gwynedd", "county": "", "region": "wales", "kind": "market", "landing": "beach", "size": 1, "from": 850, "to": 1000, "note": "A walled farm above a drying bay on Môn where silver was weighed: the one known market of the Welsh coast, between Chester and Dublin (name then unknown (?))."},
```

## 21. Cledemutha (Rhuddlan)

- **Name then**: Cledemutha, "the mouth of the Clwyd" (the English burh, 921); Rhuddlan to the Welsh.
- **Modern place**: Rhuddlan, Denbighshire.
- **Lat, lon**: 53.290, -3.468.
- **Active**: 921 to c. 950 as an English burh (?); a Welsh king's seat by 1063; a Norman borough from 1073.
- **Kind**: a fort at the lowest ford of the Clwyd; under Gruffudd ap Llywelyn a king's hall with a fleet.
- **Who ruled**: Gwynedd and Mercia fight over it (a battle at Rhuddlan in 796 or 797, Annales Cambriae). Edward
  the Elder builds the burh in 921 (ASC, Mercian Register). Gruffudd ap Llywelyn, king of all Wales 1055-1063,
  keeps his hall and ships there until Harold burns both at midwinter 1062-1063 (ASC D 1063).
- **Traded**: Welsh cattle, hides, slaves and hawks; lead and iron from the hills behind (DB has iron mines
  there); fish. In: salt, grain, wine, ale for the hall.
- **How a boat comes in**: over the bar at the Clwyd mouth and three miles up a marshy tidal river to the ford
  under the hill. Small boats only; on the ebb the river runs out fast.
- **Why a merchant goes**: for most of the game, hardly at all. In 1055-1063, a king who rules all Wales, raids
  Hereford, marries an earl's daughter and wants wine, weapons and a market of his own.
- **Evidence**: ASC (Mercian Register) 921; ASC D 1063: Harold went from Gloucester to Rhuddlan, burned the hall,
  the ships and all their gear. Annales Cambriae. DB Cheshire (Atiscros): the new borough with eighteen
  burgesses, a mint, a toll, fisheries, mills and iron mines (after the game, but it shows what was there). Digs
  (Quinnell and Blockley) found the 10th-century ditch (?).
- **Sources**: ASC. H. Quinnell and M. Blockley, Excavations at Rhuddlan, Clwyd 1969-73 (1994).
  https://en.wikipedia.org/wiki/Rhuddlan

```json
{"id": "cledemutha", "name": "Cledemutha", "modern": "Rhuddlan", "lat": 53.29, "lon": -3.468, "realm": "mercia", "county": "", "region": "wales", "kind": "fort", "landing": "riverbank", "size": 2, "from": 921, "to": 0, "note": "Edward's burh of 921 at the lowest ford of the Clwyd; by 1063 the hall and ships of Gruffudd ap Llywelyn, king of all Wales, burned by Harold."},
```

## 22. Mynyw (St Davids)

- **Name then**: Mynyw; Menevia in Latin; "Dewi's house".
- **Modern place**: St Davids, Pembrokeshire; the landing is Porth Clais.
- **Lat, lon**: 51.871, -5.283 (Porth Clais; the church is a mile inland at 51.882, -5.268).
- **Active**: all through the game.
- **Kind**: the chief church of Wales, a bishop and a school; a pilgrims' landing.
- **Who ruled**: the bishop, under the kings of Dyfed, then of Deheubarth (Hywel Dda, c. 920-950; later Gruffudd
  ap Llywelyn). Asser, Alfred's biographer, came from this house c. 885. Raided from the sea at least eight times
  in the game's years: among them 982, 988, 999 (Bishop Morgeneu killed), 1022 (?), 1073 and 1080 (after) (Annales
  Cambriae; Brut y Tywysogion).
- **Traded**: little. Wool, hides, butter and cheese, honey; the church buys wine, wax, oil and books and sends
  letters. Passage to Ireland (Ferns and Wexford are a day's sail).
- **How a boat comes in**: Porth Clais is a narrow rock inlet on the north shore of St Brides Bay, dry at low
  water, good for small boats only. Outside are the tide races of Ramsey Sound and the reefs called the Bishops
  and Clerks: come in slack water, in daylight.
- **Why a merchant goes**: pilgrims and church letters pay for passage; the last shelter before the Irish
  crossing; men who read.
- **Evidence**: Asser, Life of Alfred ch. 79 (his own house and its troubles with King Hyfaidd). The Welsh annals
  for the raids. Rhygyfarch's Life of David (c. 1090) for Porth Clais as the landing. No dig has found a market.
- **Sources**: Annales Cambriae (ed. Williams ab Ithel 1860); Brut y Tywysogion (same editor, 1860). Asser (ed.
  Stevenson 1904). https://en.wikipedia.org/wiki/St_Davids

```json
{"id": "mynyw", "name": "Mynyw", "modern": "St Davids (Porth Clais)", "lat": 51.871, "lon": -5.283, "realm": "dyfed", "county": "", "region": "wales", "kind": "monastery", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "Dewi's house, the chief church of Wales, a mile above a rock inlet that dries: pilgrims, letters, the crossing to Ireland. Raided from the sea many times."},
```

## 23. Gernemutha (Great Yarmouth)

- **Name then**: Gernemutha, Gernemwa (DB): "the mouth of the Yare".
- **Modern place**: Great Yarmouth, Norfolk.
- **Lat, lon**: 52.607, 1.729.
- **Active**: from c. 1000 (?), when the sandbank across the estuary mouth is dry enough to live on; small until
  after the game.
- **Kind**: a fishermen's shore camp that becomes a town; the autumn herring.
- **Who ruled**: the English kings and the earls of East Anglia.
- **Traded**: herring, fresh and salted, at Michaelmas; salt (many pans round the estuary in DB); nets and cordage.
- **How a boat comes in**: straight on to the sand bank from the sea, or round it into the estuary mouth and on to
  the sheltered inner shore. From here the Yare leads to Norðwic.
- **Why a merchant goes**: in the herring weeks boats from Kent, Flanders and Frisia all lie on the one bank: fish
  cheap, salt dear, news from everywhere.
- **Evidence**: DB Norfolk: 70 burgesses TRE. Digs at Fuller's Hill found 11th-century fishermen's huts and fish
  bone on the old sand (Rogerson 1976). The great fair is written of only after 1066: its size in the game's
  years is a guess (?).
- **Sources**: A. Rogerson, "Excavations on Fuller's Hill, Great Yarmouth", East Anglian Archaeology 2 (1976).
  https://en.wikipedia.org/wiki/Great_Yarmouth

```json
{"id": "gernemutha", "name": "Gernemutha", "modern": "Great Yarmouth", "lat": 52.607, "lon": 1.729, "realm": "eastanglia", "county": "", "region": "england_east", "kind": "market", "landing": "beach", "size": 1, "from": 1000, "to": 0, "note": "A new sand bank across the Yare mouth where the herring boats of three coasts lie in autumn; seventy burgesses by 1066."},
```

---

# LEAVE OUT (rows given in case they are wanted)

## 24. Snotengaham (Nottingham)

- **Name then**: Snotengaham.
- **Modern place**: Nottingham.
- **Lat, lon**: 52.951, -1.143.
- **Active**: 868 to 1066 and after.
- **Kind**: a Danish army borough on a sandstone cliff above the Trent; one of the Five Boroughs.
- **Who ruled**: Mercia; the army winters there in 868 and Burgred of Mercia, with Æthelred and Alfred of Wessex,
  cannot get it out (ASC A 868). Edward the Elder takes it in 918 and in 920 builds a second burh south of the
  river and a bridge between (ASC A).
- **Traded**: wool, grain, pots; little that the other boroughs do not have.
- **How a boat comes in**: sixty miles of Trent above Torksey, beyond the tide.
- **Why a merchant goes**: no strong reason once Torksey and Lincoln are in the game.
- **Evidence**: ASC 868, 918, 920, 942. DB Nottinghamshire: the Trent, the Foss and the York road are in the
  king's keeping, and whoever hinders the passage of boats pays eight pounds (?).
- **Why left out**: too far up river for a merchant's season, and its story is told by Torksey. The DB rule on
  the Trent is worth a card at Torksey.
- **Sources**: ASC. DB. https://en.wikipedia.org/wiki/History_of_Nottingham

```json
{"id": "snotengaham", "name": "Snotengaham", "modern": "Nottingham", "lat": 52.951, "lon": -1.143, "realm": "mercia", "county": "", "region": "england_east", "kind": "town", "landing": "riverbank", "size": 2, "from": 868, "to": 0, "note": "A Danish borough on a cliff far up the Trent; the army wintered here in 868. Edward's bridge of 920."},
```

## 25. Wæced (Watchet)

- **Name then**: Wæced, Weced.
- **Modern place**: Watchet, Somerset.
- **Lat, lon**: 51.181, -3.330.
- **Active**: c. 900 to 1066.
- **Kind**: a small burh and mint on the Severn Sea.
- **Who ruled**: Wessex; the English kings.
- **Traded**: fish, wool, a few pennies struck. Its coins turn up in Scandinavia because it was robbed, not
  because it traded.
- **How a boat comes in**: a small river mouth on a rock and mud shore with a very great tide; dry at low water.
- **Why a merchant goes**: shelter between the Avon and the west.
- **Evidence**: the Burghal Hidage (513 hides). ASC A 918 (true 914): a fleet from Brittany under the jarls Ohtor
  and Hroald is beaten off east of Watchet and at Porlock. ASC 988 and 997: Watchet ravaged. A mint under
  Æthelred II.
- **Why left out**: nothing a merchant would cross water for. Good as the place of a raid in the chronicle.
- **Sources**: ASC. Hill and Rumble, The Defence of Wessex (1996). https://en.wikipedia.org/wiki/Watchet

```json
{"id": "waeced", "name": "Wæced", "modern": "Watchet", "lat": 51.181, "lon": -3.33, "realm": "wessex", "county": "", "region": "england_south", "kind": "fort", "landing": "beach", "size": 1, "from": 900, "to": 0, "note": "A small burh and mint on the Severn Sea, raided in 914, 988 and 997."},
```

---

# Looked at and dropped (no row)

- **Hamtun (Southampton)**: the same place as `hamwic` moved half a mile; change that row, do not add one.
- **Suthgeweorc (Southwark)**: the south end of London bridge; part of Lundenburh's panel.
- **Hæstingas, Rumenea (Romney), Hythe, Pevensey, Lewes, Chichester, Bosham**: Channel landings with small mints
  and ship service to the king by 1066 (Godwine gathers ships from them in 1052, ASC E). Each is real, none is a
  market worth a row; one of them (Romney or Hastings) could stand for all if the Sussex coast looks empty.
- **Deoraby (Derby), Ligeraceaster (Leicester)**: two of the Five Boroughs, on rivers too small for a sea boat.
- **Saltwic (Droitwich)**: the brine springs; no water to it. It is the "source" behind Worcester and Gloucester.
- **Scrobbesbyrig (Shrewsbury), Hereford**: border burhs; the Severn above Worcester and the Wye are too slow.
- **Ely, Medeshamstede (Peterborough), Ramsey**: fen abbeys (refounded c. 970), reached by boat and rich; they
  would fit a later "abbeys" pass better than a list of trading places.
- **Grimsby, Hull, Boston, King's Lynn, Harwich, Portsmouth, Plymouth, Liverpool, Cardiff, Swansea**: no good
  evidence of a trading place before 1066; all are later ports. Do not put them on the chart.
- **Barnstaple, Totnes, Lydford**: Devon burhs with mints; nothing a ship would come for.
- **Aberffraw, Deganwy, Dinefwr**: Welsh kings' seats, not markets (Deganwy is destroyed in 822).
- **Portskewett**: Harold builds a hunting hall there in 1065 and Caradog ap Gruffudd burns it (ASC C,D 1065): a
  card, not a port.
- **Caerleon, Chepstow, Monmouth**: Norman foundations of 1067 and after.

# Summary table

| # | id | name then | modern | from | to | rank |
|---|----|-----------|--------|------|----|------|
| 1 | lundenburh | Lundenburh | London, the City | 886 | 0 | must |
| 2 | sandwic | Sandwic | Sandwich | 0 | 0 | must |
| 3 | northwic | Norðwic | Norwich | 900 | 0 | must |
| 4 | lindcylene | Lindcylene | Lincoln | 880 | 0 | must |
| 5 | turecesieg | Turecesieg | Torksey | 872 | 0 | must |
| 6 | theodford | Þeodford | Thetford | 869 | 0 | must |
| 7 | brycgstow | Brycgstow | Bristol | 1000 | 0 | must |
| 8 | gleawanceaster | Gleawanceaster | Gloucester | 0 | 0 | must |
| 9 | exanceaster | Exanceaster | Exeter | 0 | 0 | must |
| 10 | wintanceaster | Wintanceaster | Winchester | 0 | 0 | must |
| 11 | cantwaraburh | Cantwaraburh | Canterbury | 0 | 0 | good |
| 12 | steanford | Steanford | Stamford | 880 | 0 | good |
| 13 | hrofesceaster | Hrofesceaster | Rochester | 0 | 0 | good |
| 14 | maeldun | Mældun | Maldon | 916 | 0 | good |
| 15 | werham | Werham | Wareham | 0 | 0 | good |
| 16 | oxnaford | Oxnaford | Oxford | 900 | 0 | good |
| 17 | grantebrycg | Grantebrycg | Cambridge | 875 | 0 | good |
| 18 | weogornaceaster | Weogornaceaster | Worcester | 0 | 0 | good |
| 19 | meols | Meols | Meols, Wirral | 0 | 0 | good |
| 20 | llanbedrgoch | (unknown) | Llanbedrgoch, Anglesey | 850 | 1000 | good |
| 21 | cledemutha | Cledemutha | Rhuddlan | 921 | 0 | good |
| 22 | mynyw | Mynyw | St Davids | 0 | 0 | good |
| 23 | gernemutha | Gernemutha | Great Yarmouth | 1000 | 0 | good |
| 24 | snotengaham | Snotengaham | Nottingham | 868 | 0 | leave out |
| 25 | waeced | Wæced | Watchet | 900 | 0 | leave out |
