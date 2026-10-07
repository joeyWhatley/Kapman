# The gazetteer of 793-1066

Asked for by the user on 4 Oct 2026: "Everything single play in the known trade routes and all settlements all ports cities all locations everything" - read as every single place on the known trade routes, and all settlements, ports, cities and locations. The game has 50 ports (`Assets/Kapman/Game/Data/ports_793.json`). This is the rest of the world around them.

- The list: `Assets/Kapman/Game/Data/gazetteer_793.json` - **15447 places** and, as a light layer of their own, **3239 stones**.
- The script: `Bake/gazetteer/make_gazetteer.py` (run it with `Bake\run.bat gazetteer\make_gazetteer.py`; it only asks what is not yet on disk).
- The raw answers: `G:\kapman_unity\data\gazetteer\raw_<kind>.json`, each with the query that was asked.
- The picture: `Captures/map/gazetteer.png` (6144 x 4096): every place as a dot over the chart's overview, and one small map for each group of kinds.
- Asked on 2026-10-06. Window: longitude -11 to 33, latitude 43 to 69. Time: alive some time between 750 and 1066.

## The credit line

> Places, names, coordinates and dates: Wikidata (wikidata.org), CC0 1.0 Universal public domain dedication.

Wikidata is the only source read. CC0 asks for no credit, but the project records one for every source, so this line goes in the credits. Nothing with a share-alike or non-commercial licence was used: no OpenStreetMap, no Wikipedia text, no Pleiades or OpenDomesday data.

One thing to know: three layers are found by an **id that Wikidata holds** for another list - the English vills by their OpenDomesday id, the Roman towns by a Pleiades, DARE or vici.org id, the inscriptions by a Rundata id. Only the fact that Wikidata's item carries such an id is used, and the id itself is not kept; the place, its name and its coordinates are Wikidata's. If that is too near the other lists for comfort, `--no-domesday` leaves the English vills out, and the kinds `roman_town` and `runic_inscription` can be dropped by kind.

## What a record holds

`id` (a slug; for a port, the port's id), `qid` (Wikidata), `name` (today's), `name_local` (in the land's own tongue), `lat`, `lon`, `x`, `z` (metres in the game's world frame: the Lambert conformal projection `WORLD_PROJ` of `Bake/world/make_map_art.py`; x east, z north), `kind`, `from`, `to` (years; 0 = not known), `tier`, `source`. Two more for convenience: `country` (today's, ISO code) and `also` (what else the place is).

- **Tier 1**: the 50 ports, matched by name and nearness (48 matched to a Wikidata place; written in from the ports file itself: witla, herio). Their `lat`/`lon` here are Wikidata's; the game keeps its own in `ports_793.json`.
- **Tier 2**: a place a merchant would know by name - the places on the script's list of names, every emporium, burh, palace, ring fortress, longphort, mint, thing site, cathedral and bishop's seat found by class, and any other place well known today (by its count of Wikipedia articles: a town 20, a monastery or stronghold 8, a fort 10, a church or site 15, a Domesday vill 25).
- **Tier 3**: the rest.

## The counts by kind

| kind | what it is | places | tier 1 | tier 2 | tier 3 | with a date |
|---|---:|---:|---:|---:|---:|---:|
| emporium | an emporium or trading place | 60 | 25 | 35 | 0 | 10 |
| longphort | a longphort, or a Norse town of Ireland | 12 | 1 | 11 | 0 | 2 |
| ring_fort | a ring fortress | 12 | 0 | 12 | 0 | 1 |
| burh | an Anglo-Saxon burh | 58 | 0 | 54 | 4 | 6 |
| palace | a royal palace, Pfalz, royal vill or king's farm | 105 | 4 | 81 | 20 | 17 |
| thing | a thing site | 168 | 0 | 13 | 155 | 0 |
| mint | a mint | 55 | 0 | 55 | 0 | 3 |
| monastic_town | a monastic town of Ireland | 48 | 0 | 48 | 0 | 6 |
| cathedral | a cathedral | 79 | 0 | 79 | 0 | 77 |
| bishopric | a bishop's seat (the bishopric) | 146 | 0 | 146 | 0 | 146 |
| monastery | a monastery or abbey | 628 | 3 | 132 | 493 | 547 |
| stronghold | a Slavic or Baltic stronghold | 161 | 0 | 40 | 121 | 14 |
| ship_find | a ship find | 39 | 0 | 0 | 39 | 6 |
| battle | a battle or siege | 197 | 0 | 0 | 197 | 196 |
| roman_town | a Roman town still lived in | 1583 | 0 | 1357 | 226 | 126 |
| town | a town, city or village known before 1067 | 832 | 17 | 452 | 363 | 745 |
| fort | a fort, castle or rampart | 697 | 0 | 57 | 640 | 358 |
| church | a church | 1096 | 0 | 42 | 1054 | 997 |
| site | an archaeological site or other remains of the time | 1261 | 0 | 35 | 1226 | 248 |
| settlement | an English vill of Domesday (1066-86) | 8210 | 0 | 266 | 7944 | 0 |
| **all places** |  | 15447 | 50 | 2915 | 12482 | 3505 |

The stones (their own layer, `stones` in the file, all tier 3):

| kind | what it is | stones |
|---|---:|---:|
| runestone | a rune stone | 1286 |
| picture_stone | a picture stone | 30 |
| runic_inscription | a runic inscription (a Rundata id) | 1923 |

## The counts by country (today's borders)

| country | places | tier 1 and 2 | with a date | dated | stones |
|---|---:|---:|---:|---:|---:|
| United Kingdom | 9431 | 506 | 731 | 8% | 1 |
| Germany | 2130 | 450 | 827 | 39% | 5 |
| France | 1031 | 732 | 400 | 39% | 0 |
| Italy | 858 | 431 | 534 | 62% | 1 |
| Czechia | 296 | 135 | 255 | 86% | 0 |
| Switzerland | 205 | 64 | 84 | 41% | 0 |
| Sweden | 202 | 29 | 20 | 10% | 3054 |
| Austria | 179 | 136 | 41 | 23% | 0 |
| Ireland | 158 | 62 | 38 | 24% | 1 |
| Belgium | 100 | 52 | 65 | 65% | 0 |
| Ukraine | 89 | 38 | 86 | 97% | 1 |
| Poland | 88 | 42 | 40 | 45% | 1 |
| Croatia | 83 | 47 | 40 | 48% | 0 |
| Netherlands | 66 | 31 | 35 | 53% | 0 |
| Spain | 62 | 19 | 47 | 76% | 0 |
| Belarus | 58 | 12 | 56 | 97% | 0 |
| Norway | 53 | 7 | 25 | 47% | 37 |
| Romania | 42 | 23 | 26 | 62% | 0 |
| Bulgaria | 42 | 13 | 13 | 31% | 0 |
| Denmark | 41 | 21 | 22 | 54% | 135 |
| Slovenia | 40 | 11 | 2 | 5% | 0 |
| Hungary | 35 | 33 | 15 | 43% | 0 |
| Russia | 27 | 14 | 16 | 59% | 0 |
| Luxembourg | 22 | 5 | 18 | 82% | 0 |
| Serbia | 21 | 9 | 12 | 57% | 0 |
| Slovakia | 20 | 12 | 18 | 90% | 0 |
| Bosnia and Herzegovina | 17 | 8 | 14 | 82% | 0 |
| Latvia | 14 | 2 | 12 | 86% | 0 |
| Finland | 10 | 4 | 4 | 40% | 1 |
| Estonia | 6 | 5 | 1 | 17% | 0 |
| not known (at sea, or no country in Wikidata) | 4 | 2 | 1 | 25% | 0 |
| Isle of Man | 4 | 2 | 0 | 0% | 0 |
| Liechtenstein | 4 | 2 | 3 | 75% | 0 |
| Faroe Islands | 3 | 3 | 1 | 33% | 2 |
| Lithuania | 3 | 1 | 1 | 33% | 0 |
| Guernsey | 1 | 0 | 1 | 100% | 0 |
| San Marino | 1 | 1 | 1 | 100% | 0 |
| Montenegro | 1 | 1 | 0 | 0% | 0 |

## What is thin

Wikidata is broad and free, and it is uneven. Read the map with this in mind:

- **Dates are few.** Only 3505 of the 15447 places have a year. A place without one is in the list because of what it is (a burh, a gord, a Domesday vill), not because Wikidata says when. Under a quarter of the places are dated in: United Kingdom, Sweden, Austria, Ireland, Slovenia.
- **A date is often not the beginning.** Wikidata's inception of a town is sometimes its charter, and a town whose charter is later than 1066 is left out even if it was there (Winchester, Lejre and Hedeby have no date at all). The famous ones are on the script's list of names; the small ones are missing.
- **England looks full, the rest looks empty.** The 8210 Domesday vills are one list for one kingdom. No such list exists in Wikidata for Francia, Frisia, Saxony, Denmark, Norway, Sweden, Ireland or the Slavic lands, so their villages are missing, not absent.
- **Scandinavia, the Baltic and the east are the thinnest**: few dated towns, few monasteries (there were few), and the Viking Age sites are mostly untagged. Sweden's weight is in the stones layer.
- **Kinds Wikidata has no class for.** Burhs, royal vills, longphorts and emporia have almost no items of their own class (burh: 3, royal vill: 0, longphort: 0), so these kinds are mostly the script's list of names: the Burghal Hidage, the known emporia, the Norse towns of Ireland.
- **Mints** have no dates in Wikidata. Only mints named on the script's list of early mints are kept, and most Anglo-Saxon mints have no item.
- **Irish monastic sites**: only the larger houses have a founding date; the hundreds of small early churches are missing.
- **Thing sites and ship finds** are a handful each.
- **Found by words only** (`source` says "its words"): these were found by a phrase in a label or description ("Slawischer Burgwall", "Viking Age") and are right more often than not, but not checked one by one. They are tier 3 unless well known.
- **Roman towns still lived in** are living places that carry an id of the ancient world. Most were lived in through our time; a few may have lain empty.
- **The window is a box**, so it holds the north of Spain and Italy and the north Balkans too, which the chart does not show.

Names on the script's list that Wikidata has no placed item for (within 30 km): Fröjel, Pfalz Paderborn, Gulating, Frostating, Myklebust Ship, Graveney boat, Utrecht ship.

Left out in the merge: a person, a work, a museum: 879; no details: 542; not of the time (founded after 1066, or gone before 750): 479; dated, but not a place (a realm, an office, a people): 348; found by words only, on a plain town or village: 292; the same place twice: 83; a Rundata id on something that is not a stone: 19; no name in any of the languages read: 3; a mint with an early name, but by no place of the time: 1.

## How the curators fill it

The script never loses hand work, because hand work does not go in the output file. It goes in `Design/content/gazetteer/curated.json`, which the script merges last on every run:

```json
{
 "places": [
  {"qid": "Q172157", "from": 648, "source": "wikidata; date: Anglo-Saxon Chronicle s.a. 648 (public domain)"},
  {"id": "witla", "name": "Witla", "name_local": "Witla", "lat": 51.89, "lon": 4.22, "kind": "emporium", "from": 0, "to": 836, "tier": 2,
   "source": "Annales Fuldenses s.a. 836 (public domain)"}
 ],
 "drop": ["Q12345"]
}
```

- A record with a `qid` or an `id` that is already in the list **changes** that place (only the fields given). Any other record is **added**. `drop` leaves places out.
- Fill in this order: (1) dates and period names for tier 1 and 2; (2) the places a port's dossier names that are missing here (`Design/content/dossiers/<port>.md`); (3) the thin lands above, region by region; (4) wrong kinds and wrong hits among the places found by words.
- **Sources**: only ones free for commercial use, with the credit line written in `source`. Good: the period texts themselves (annals, charters, sagas, itineraries - public domain), Wikidata (CC0), national heritage registers under CC0 or CC BY. Not allowed: OpenStreetMap (share-alike), Wikipedia text (share-alike), anything non-commercial. A fact not checked gets `(?)` in its source, as in the ports file.
- To fix a thing at the root, fix it in Wikidata (add the inception, the class, the coordinates) and run the script with `--fresh`.

## The ports and their places

| port | place | qid | kind | km apart |
|---|---:|---:|---:|---:|
| dorestad | Dorestad | Q739353 | emporium | 0.4 |
| traiectum | Utrecht | Q803 | town | 0.1 |
| walichrum | Domburg | Q980728 | emporium | 0.3 |
| witla | Witla | - | - | - |
| medemelacha | Medemblik | Q9947 | emporium | 0.4 |
| wijnaldum | Wijnaldum | Q1871629 | town | 1.7 |
| doccinga | Dokkum | Q41976 | town | 0.1 |
| emuthon | Emden | Q4174 | emporium | 0.1 |
| daventre | Deventer | Q10001 | emporium | 0.6 |
| noviomagus | Nijmegen | Q47887 | town | 0.3 |
| quentovic | Quentovic | Q1367746 | emporium | 2.0 |
| rotomagus | Rouen | Q30974 | town | 0.5 |
| parisius | Paris | Q90 | town | 0.5 |
| ganda | Ghent | Q1296 | town | 0.1 |
| andoverpis | Antwerp | Q12892 | town | 0.2 |
| colonia | Cologne | Q365 | town | 0.6 |
| mogontia | Mainz | Q1720 | town | 0.1 |
| diusburg | Duisburg | Q2100 | town | 0.3 |
| lundenwic | London | Q84 | emporium | 0.6 |
| hamwic | Southampton | Q79848 | emporium | 0.9 |
| gipeswic | Ipswich | Q184775 | emporium | 0.7 |
| eoforwic | York | Q42462 | emporium | 1.0 |
| sarre | Sarre | Q7424565 | emporium | 0.1 |
| fordwic | Fordwich | Q2177468 | emporium | 0.4 |
| dofras | Dover | Q179224 | emporium | 0.3 |
| lindisfarena | Lindisfarne Priory | Q354305 | monastery | 0.0 |
| bebbanburg | Bamburgh Castle | Q2026185 | palace | 0.1 |
| gyrwe | Jarrow | Q782155 | town | 1.0 |
| streonaeshalch | Whitby | Q852778 | town | 0.9 |
| dommoc | Dunwich | Q188020 | town | 0.3 |
| legacaestir | Chester | Q170263 | town | 1.2 |
| hi | Iona | Q610 | monastery | 1.2 |
| duiblinn | Dublin | Q1761 | longphort | 0.9 |
| ripa | Ribe | Q322361 | emporium | 0.3 |
| sliesthorp | Hedeby | Q165414 | emporium | 0.0 |
| reric | Rarog Gord | Q569174 | emporium | 0.7 |
| skiringssal | Kaupang | Q1737156 | emporium | 4.4 |
| birka | Birka | Q208177 | emporium | 0.2 |
| helgo | Helgö | Q18290555 | emporium | 4.9 |
| uppsalir | Gamla Uppsala | Q433032 | palace | 0.5 |
| paviken | Västergarn and Stora Mafrids | Q3392833 | emporium | 3.8 |
| ahus | Åhus | Q127419 | emporium | 1.0 |
| truso | Truso | Q1752073 | emporium | 23.0 |
| grobin | Grobiņa | Q757049 | emporium | 0.3 |
| aldeigjuborg | Staraya Ladoga | Q216794 | emporium | 0.3 |
| ogvaldsnes | Avaldsnes | Q790084 | palace | 0.3 |
| hleidr | Lejre Municipality | Q505369 | palace | 0.3 |
| namnetis | Nantes | Q12191 | town | 0.5 |
| herio | Herio | - | - | - |
| burdigala | Bordeaux | Q1479 | town | 0.8 |

## What was asked, and what came back

| raw file | query | rows |
|---|---:|---:|
| monastery | monasteries and abbeys founded before 1067 | 606 |
| cathedral | cathedrals founded before 1067 | 82 |
| cathedral | bishoprics founded before 1067 | 47 |
| cathedral | bishoprics founded before 1067, placed at their seat | 205 |
| palace | pfalzen, royal vills and kings' farms | 24 |
| palace | palaces founded before 1067 | 14 |
| palace | by their words: Königspfalz | 16 |
| palace | by their words: Kaiserpfalz | 46 |
| palace | by their words: Karolingerpfalz | 0 |
| palace | by their words: "karolingische Pfalz" | 0 |
| palace | by their words: "ottonische Pfalz" | 0 |
| palace | by their words: Pfalz karolingisch | 0 |
| palace | by their words: Pfalz ottonisch | 0 |
| palace | by their words: "royal vill" | 0 |
| palace | by their words: "villa regia" | 7 |
| palace | by their words: "Anglo-Saxon palace" | 0 |
| palace | by their words: "Carolingian palace" | 1 |
| palace | by their words: "palais carolingien" | 0 |
| palace | by their words: "Anglo-Saxon royal" | 1 |
| palace | by their words: kongsgård vikingtid | 0 |
| palace | by their words: "royal estate" "Anglo-Saxon" | 0 |
| palace | by their words: "royal estate" Carolingian | 0 |
| palace | by their words: Königshof karolingisch | 0 |
| palace | by their words: Königshof fränkisch | 0 |
| palace | by their words: "fränkischer Königshof" | 2 |
| palace | by their words: "karolingischer Königshof" | 1 |
| palace | by their words: "royal seat" Viking | 0 |
| palace | by their words: kungsgård vikingatid | 0 |
| burh | burhs | 3 |
| burh | by their words: "Anglo-Saxon burh" | 1 |
| burh | by their words: "Burghal Hidage" | 1 |
| burh | by their words: "Saxon burh" | 3 |
| burh | by their words: burh "Anglo-Saxon" | 1 |
| mint | mints | 328 |
| emporium | by their words: "trading place" Viking | 0 |
| emporium | by their words: "trading settlement" Viking | 0 |
| emporium | by their words: "trading centre" Viking | 0 |
| emporium | by their words: "trading center" Viking | 0 |
| emporium | by their words: "trading post" Viking | 0 |
| emporium | by their words: "market place" Viking | 0 |
| emporium | by their words: "trading town" Viking | 0 |
| emporium | by their words: emporium "early medieval" | 0 |
| emporium | by their words: emporium Carolingian | 0 |
| emporium | by their words: emporium "Anglo-Saxon" | 0 |
| emporium | by their words: emporium Viking | 2 |
| emporium | by their words: emporium Frisian | 0 |
| emporium | by their words: Handelsplatz wikingerzeitlich | 0 |
| emporium | by their words: Handelsplatz wikingerzeitlicher | 0 |
| emporium | by their words: Handelsplatz frühmittelalterlich | 0 |
| emporium | by their words: Handelsplatz frühmittelalterlicher | 0 |
| emporium | by their words: Seehandelsplatz | 0 |
| emporium | by their words: Handelsplatz slawisch | 0 |
| emporium | by their words: Handelsplatz slawischer | 0 |
| emporium | by their words: handelsplads vikingetid | 0 |
| emporium | by their words: handelsplads vikingetiden | 0 |
| emporium | by their words: handelsplats vikingatid | 0 |
| emporium | by their words: handelsplats vikingatida | 0 |
| emporium | by their words: handelsplass vikingtid | 0 |
| emporium | by their words: handelsplass vikingtiden | 0 |
| emporium | by their words: kaupang vikingtid | 0 |
| emporium | by their words: wic "Anglo-Saxon" | 0 |
| fort | Viking ring fortresses | 3 |
| fort | parts of the Viking Age ring fortresses | 5 |
| fort | circular ramparts | 204 |
| fort | forts and castles founded before 1067 | 453 |
| fort | by their words: "ring fortress" | 1 |
| fort | by their words: ringborg | 2 |
| fort | by their words: trelleborg vikingetid | 0 |
| fort | by their words: ringwalburg | 5 |
| fort | by their words: "Viking fortress" | 0 |
| fort | by their words: "Viking fort" | 6 |
| fort | by their words: "Viking camp" | 1 |
| fort | by their words: "winter camp" Viking | 0 |
| fort | by their words: Ungarnrefugium | 0 |
| fort | by their words: Ungarnwall | 1 |
| fort | by their words: "karolingische Burg" | 1 |
| fort | by their words: "ottonische Burg" | 0 |
| fort | by their words: "frühmittelalterliche Burg" | 3 |
| fort | by their words: "frühmittelalterliche Befestigung" | 6 |
| fort | by their words: "frühmittelalterlicher Ringwall" | 17 |
| fort | by their words: "frühmittelalterliche Wallanlage" | 10 |
| fort | by their words: "frühmittelalterlicher Burgstall" | 1 |
| fort | by their words: "sächsische Wallburg" | 0 |
| fort | by their words: "château carolingien" | 0 |
| fort | by their words: "enceinte carolingienne" | 0 |
| fort | by their words: "Anglo-Saxon fort" | 0 |
| fort | by their words: "Pictish fort" | 4 |
| fort | by their words: fornborg vikingatid | 0 |
| fort | by their words: bygdeborg vikingtid | 0 |
| stronghold | gords | 101 |
| stronghold | by their words: "Slawischer Burgwall" | 12 |
| stronghold | by their words: Slawenburg | 6 |
| stronghold | by their words: Burgwall slawisch | 0 |
| stronghold | by their words: Burgwall slawischer | 12 |
| stronghold | by their words: Burgwall slawische | 1 |
| stronghold | by their words: "slawische Burg" | 1 |
| stronghold | by their words: "slawische Burganlage" | 0 |
| stronghold | by their words: "slawischer Ringwall" | 0 |
| stronghold | by their words: "slawische Wallanlage" | 0 |
| stronghold | by their words: "Slavic stronghold" | 0 |
| stronghold | by their words: "Slavic fort" | 0 |
| stronghold | by their words: "Slavic hillfort" | 3 |
| stronghold | by their words: "Slavic fortress" | 0 |
| stronghold | by their words: "Slavic gord" | 10 |
| stronghold | by their words: "Slavic settlement" | 14 |
| stronghold | by their words: "Slavic castle" | 0 |
| stronghold | by their words: "Slavic burgwall" | 0 |
| stronghold | by their words: gord Slavic | 10 |
| stronghold | by their words: grodzisko wczesnośredniowieczne | 2 |
| stronghold | by their words: gród wczesnośredniowieczny | 0 |
| stronghold | by their words: "slovanské hradiště" | 10 |
| stronghold | by their words: "slovanské hradisko" | 0 |
| stronghold | by their words: hradiště slovanské | 12 |
| stronghold | by their words: "древнерусское городище" | 0 |
| stronghold | by their words: "Great Moravian" | 3 |
| stronghold | by their words: velkomoravské hradiště | 2 |
| irish | longphorts | 0 |
| irish | Monasticon Hibernicum ids | 11 |
| irish | by their words: longphort Viking | 0 |
| irish | by their words: "Viking longphort" | 0 |
| irish | by their words: "monastic site" Ireland | 27 |
| irish | by their words: "monastic settlement" Ireland | 2 |
| irish | by their words: "early Christian" monastery Ireland | 3 |
| irish | by their words: "early Christian" "monastic site" | 26 |
| irish | by their words: "Irish round tower" | 4 |
| irish | by their words: "round tower" Ireland | 37 |
| irish | by their words: "early monastic" Ireland | 3 |
| irish | by their words: "Celtic monastery" | 0 |
| irish | by their words: "early Christian" monastery Scotland | 0 |
| irish | by their words: "early Christian" monastery Wales | 0 |
| town | towns, cities and villages with an inception before 1067 | 1003 |
| town | places first written of before 1067 | 722 |
| town | ancient and medieval cities by class | 6 |
| roman_town | living places with an id of the ancient world | 1821 |
| settlement | places with an OpenDomesday settlement id | 8479 |
| site | tagged with a period or a culture of the time | 54 |
| site | built in a style of the time | 132 |
| site | by their words: "Viking Age" | 63 |
| site | by their words: "Viking era" | 7 |
| site | by their words: "Viking period" | 13 |
| site | by their words: "Viking settlement" | 3 |
| site | by their words: "Viking burial" | 5 |
| site | by their words: "Viking grave" | 7 |
| site | by their words: "Viking cemetery" | 7 |
| site | by their words: "Viking harbour" | 0 |
| site | by their words: "Viking harbor" | 0 |
| site | by their words: "Norse settlement" | 10 |
| site | by their words: "Norse farmstead" | 1 |
| site | by their words: vikingatid | 1 |
| site | by their words: vikingatida | 101 |
| site | by their words: vikingatiden | 3 |
| site | by their words: vikingetid | 3 |
| site | by their words: vikingetiden | 6 |
| site | by their words: vikingtid | 1 |
| site | by their words: vikingtiden | 3 |
| site | by their words: vikingtida | 1 |
| site | by their words: Wikingerzeit | 19 |
| site | by their words: wikingerzeitlich | 0 |
| site | by their words: wikingerzeitliche | 8 |
| site | by their words: wikingerzeitlicher | 2 |
| site | by their words: wikingerzeitliches | 8 |
| site | by their words: "Anglo-Saxon" | 153 |
| site | by their words: angelsächsisch | 0 |
| site | by their words: angelsächsische | 2 |
| site | by their words: Carolingian | 8 |
| site | by their words: karolingisch | 119 |
| site | by their words: karolingische | 22 |
| site | by their words: karolingischer | 8 |
| site | by their words: karolingisches | 22 |
| site | by their words: carolingien | 6 |
| site | by their words: carolingienne | 5 |
| site | by their words: Karolingisch | 119 |
| site | by their words: Ottonian | 1 |
| site | by their words: ottonisch | 1 |
| site | by their words: ottonische | 39 |
| site | by their words: "early medieval" | 159 |
| site | by their words: "Early Middle Ages" | 6 |
| site | by their words: frühmittelalterlich | 7 |
| site | by their words: frühmittelalterliche | 420 |
| site | by their words: frühmittelalterlicher | 86 |
| site | by their words: frühmittelalterliches | 420 |
| site | by their words: Frühmittelalter | 276 |
| site | by their words: "haut Moyen Âge" | 16 |
| site | by their words: vroegmiddeleeuws | 1 |
| site | by their words: vroegmiddeleeuwse | 1 |
| site | by their words: vendeltid | 0 |
| site | by their words: vendeltida | 1 |
| site | by their words: "Vendel Period" | 0 |
| site | by their words: "Vendel era" | 0 |
| site | by their words: merovingertid | 1 |
| site | by their words: "early Slavic" | 0 |
| site | by their words: frühslawisch | 0 |
| site | by their words: frühslawische | 0 |
| site | by their words: altslawisch | 0 |
| site | by their words: altslawische | 0 |
| site | by their words: wczesnośredniowieczne | 6 |
| site | by their words: wczesnośredniowieczny | 1 |
| site | by their words: wczesnośredniowieczna | 0 |
| site | by their words: "raně středověké" | 14 |
| site | by their words: "yngre jernalder" | 2 |
| site | by their words: "yngre järnålder" | 0 |
| site | by their words: Pictish | 125 |
| site | by their words: "Hiberno-Norse" | 1 |
| site | by their words: "Anglo-Scandinavian" | 21 |
| site | by their words: "Anglo-Saxon cemetery" | 48 |
| site | by their words: "Anglo-Saxon church" | 2 |
| thing | things | 14 |
| thing | by their words: "thing site" | 0 |
| thing | by their words: thingstead | 1 |
| thing | by their words: tingsted vikingetid | 0 |
| thing | by their words: tingsplats | 2 |
| thing | by their words: tingplats vikingatid | 0 |
| thing | by their words: tingstad | 187 |
| thing | by their words: tinghaug | 7 |
| thing | by their words: tingshög | 5 |
| thing | by their words: tingplass vikingtid | 0 |
| thing | by their words: "assembly site" | 4 |
| thing | by their words: "assembly mound" | 0 |
| thing | by their words: "moot hill" | 7 |
| thing | by their words: "Viking parliament" | 0 |
| thing | by their words: "Norse assembly" | 0 |
| thing | by their words: "Norse parliament" | 0 |
| battle | battles and sieges of 793 to 1066 | 681 |
| ship_find | ship burials, boat graves and Viking ships | 54 |
| ship_find | wrecks of 700 to 1066 | 0 |
| ship_find | by their words: "Viking ship" | 15 |
| ship_find | by their words: "ship burial" | 5 |
| ship_find | by their words: "boat burial" | 2 |
| ship_find | by their words: "boat grave" | 1 |
| ship_find | by their words: "ship grave" | 0 |
| ship_find | by their words: båtgrav | 3 |
| ship_find | by their words: båtgravar | 3 |
| ship_find | by their words: båtgravfält | 0 |
| ship_find | by their words: skibsgrav | 2 |
| ship_find | by their words: skipsgrav | 0 |
| ship_find | by their words: skipsgraven | 0 |
| ship_find | by their words: Schiffsgrab | 5 |
| ship_find | by their words: Bootsgrab | 5 |
| ship_find | by their words: Bootsgräber | 0 |
| ship_find | by their words: vikingaskepp | 11 |
| ship_find | by their words: vikingeskib | 7 |
| ship_find | by their words: vikingskip | 4 |
| ship_find | by their words: Wikingerschiff | 3 |
| stone | rune stones and picture stones | 1393 |
| stone | inscriptions with a Rundata id | 3347 |
| dated | everything with an inception before 1067 | 3978 |

Every query is in its raw file word for word. One query at a time, a second between two, waiting on a 429 or a timeout; a query still too heavy is cut into four boxes.
