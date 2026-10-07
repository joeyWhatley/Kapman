# Ports of the first release (793 to 805) - check

Written 7 Oct 2026 from Assets/Kapman/Game/Data/ports_793.json, Assets/Kapman/Game/Data/places/<port>.json and Art/art_list.csv. Nothing here is invented; every number is a count of what the files hold.

How the counts are made: 'open 793-805' counts an entry whose from is 805 or earlier and whose to is 793 or later (a to of 0 is read as no end). Church means a site of kind church, minster or abbey; yard means kind shipyard; alehouse means kind tavern; market means kind market.

## The twelve at a glance

| port | name then | now | realm | kind | size | landing | sites (open 793-805) | people (open) | stories | works (open) |
|---|---|---|---|---|---|---|---|---|---|---|
| dorestad | Dorestat | Wijk bij Duurstede | franks | emporium | 5 | jetty | 12 (12) | 16 (11) | 12 | 6 (6) |
| traiectum | Traiectum | Utrecht | franks | town | 3 | riverbank | 14 (10) | 9 (5) | 7 | 5 (2) |
| walichrum | Walichrum | Domburg, Walcheren | franks | emporium | 3 | beach | 13 (12) | 8 (4) | 5 | 5 (4) |
| lundenwic | Lundenwic | London (the Strand) | mercia | emporium | 5 | beach | 17 (12) | 16 (8) | 6 | 6 (4) |
| witla | Witla | lost, the Maas mouth (?) | franks | emporium | 2 | beach | 10 (10) | 7 (7) | 4 | 5 (5) |
| medemelacha | Medemelacha | Medemblik | franks | market | 2 | jetty | 10 (10) | 8 (5) | 6 | 5 (4) |
| doccinga | Doccinga | Dokkum | franks | village | 1 | beach | 11 (8) | 10 (7) | 5 | 5 (4) |
| daventre | Daventre | Deventer | franks | town | 2 | riverbank | 16 (11) | 8 (5) | 5 | 5 (2) |
| quentovic | Quentovic | Visemarest, La Calotterie | franks | emporium | 4 | riverbank | 12 (11) | 9 (7) | 5 | 5 (4) |
| sarre | Serræ | Sarre, Thanet | kent | market | 2 | beach | 10 (10) | 8 (5) | 6 | 5 (4) |
| fordwic | Fordwic | Fordwich | kent | market | 1 | riverbank | 12 (12) | 8 (6) | 5 | 4 (3) |
| ripa | Ripa | Ribe | danes | emporium | 3 | riverbank | 12 (10) | 10 (7) | 5 | 5 (4) |

## Market, church, yard, alehouse

yes = a site of that kind open in 793-805; later = the file has one but it is not open in 793-805; no = the file has none.

| port | market | church | yard | alehouse |
|---|---|---|---|---|
| dorestad | yes | yes | yes | no |
| traiectum | later | yes | no | yes |
| walichrum | yes | no | yes | yes |
| lundenwic | yes | yes | yes | yes |
| witla | yes | no | no | yes |
| medemelacha | yes | yes | no | no |
| doccinga | later | yes | no | no |
| daventre | yes | yes | yes | yes |
| quentovic | yes | yes | yes | yes |
| sarre | yes | no | no | yes |
| fordwic | yes | yes | no | yes |
| ripa | yes | later | yes | no |

## Harbour view and chart mark in Art/art_list.csv

The art list has kinds 'harbour' (the harbour view) and 'port_mark' (the mark on the chart). This says whether the list has a row; it does not say the picture is drawn.

| port | harbour view | chart mark | site scenes listed | sites in the data |
|---|---|---|---|---|
| dorestad | harbour/harbour.dorestad.png | port_mark/portmark.dorestad.png | 12 | 12 |
| traiectum | harbour/harbour.traiectum.png | port_mark/portmark.traiectum.png | 14 | 14 |
| walichrum | harbour/harbour.walichrum.png | port_mark/portmark.walichrum.png | 13 | 13 |
| lundenwic | harbour/harbour.lundenwic.png | port_mark/portmark.lundenwic.png | 17 | 17 |
| witla | harbour/harbour.witla.png | port_mark/portmark.witla.png | 10 | 10 |
| medemelacha | harbour/harbour.medemelacha.png | port_mark/portmark.medemelacha.png | 10 | 10 |
| doccinga | harbour/harbour.doccinga.png | port_mark/portmark.doccinga.png | 11 | 11 |
| daventre | harbour/harbour.daventre.png | port_mark/portmark.daventre.png | 16 | 16 |
| quentovic | harbour/harbour.quentovic.png | port_mark/portmark.quentovic.png | 12 | 12 |
| sarre | harbour/harbour.sarre.png | port_mark/portmark.sarre.png | 10 | 10 |
| fordwic | harbour/harbour.fordwic.png | port_mark/portmark.fordwic.png | 12 | 12 |
| ripa | harbour/harbour.ripa.png | port_mark/portmark.ripa.png | 12 | 12 |

## Each port

### dorestad (Dorestat; now Wijk bij Duurstede)

- Realm franks, region rhine_delta, kind emporium, size 5, landing jetty; port open from 0 to 863 (0 = no limit).
- Sites 12 (12 open 793-805), people 16 (11 open), stories 12, works 6 (6 open).
- Site kinds open 793-805: church, graveyard, hall, lodging, market, mint, quay x2, shipyard, storehouse, toll, workshop.
- Of market, church, yard, alehouse it lacks in 793-805: alehouse.
- Other kinds at least half the twelve have and it lacks: field (9 of 12 have it), tavern (8 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 26, ending 1, harbour 1, port_mark 1, scene 12, ship_offer 7, story 12, work 6.

### traiectum (Traiectum; now Utrecht)

- Realm franks, region rhine_delta, kind town, size 3, landing riverbank; port open from 0 to 0 (0 = no limit).
- Sites 14 (10 open 793-805), people 9 (5 open), stories 7, works 5 (2 open).
- Site kinds open 793-805: abbey, beach, church, ferry, gate, hall, lodging, minster, storehouse, tavern.
- Sites in the file but not open 793-805: market 'The Stathe' (920-0); quay 'The Vecht side' (950-0); mint 'The bishop's mint' (953-0); minster 'Adelbold's cathedral' (1023-0).
- Of market, church, yard, alehouse it lacks in 793-805: market, yard.
- Other kinds at least half the twelve have and it lacks: market (10 of 12 have it), field (9 of 12 have it), workshop (8 of 12 have it), quay (6 of 12 have it), toll (6 of 12 have it), shipyard (6 of 12 have it), graveyard (6 of 12 have it).
- Art list rows for this port, portraits left out: card 17, harbour 1, port_mark 1, scene 14, ship_offer 6, story 7, work 5.

### walichrum (Walichrum; now Domburg, Walcheren)

- Realm franks, region zeeland, kind emporium, size 3, landing beach; port open from 0 to 850 (0 = no limit).
- Sites 13 (12 open 793-805), people 8 (4 open), stories 5, works 5 (4 open).
- Site kinds open 793-805: beach, field, graveyard, hall, market, mound, saltern, shipyard, shrine, tavern, workshop x2.
- Sites in the file but not open 793-805: wall 'The ring wall' (838-1066).
- Of market, church, yard, alehouse it lacks in 793-805: church.
- Other kinds at least half the twelve have and it lacks: church (7 of 12 have it), lodging (7 of 12 have it), quay (6 of 12 have it), toll (6 of 12 have it).
- Art list rows for this port, portraits left out: card 13, harbour 1, port_mark 1, scene 13, ship_offer 5, story 5, work 5.

### lundenwic (Lundenwic; now London (the Strand))

- Realm mercia, region england_south, kind emporium, size 5, landing beach; port open from 0 to 0 (0 = no limit).
- Sites 17 (12 open 793-805), people 16 (8 open), stories 6, works 6 (4 open).
- Site kinds open 793-805: beach, field, graveyard, hall, lodging, market, minster, mint, shipyard, tavern, workshop x2.
- Sites in the file but not open 793-805: quay 'Aethelred's hithe' (886-1066); toll 'The toll at Billingsgate' (990-1066); bridge 'The bridge' (975-1066); graveyard 'The Danes' graves at Saint Paul's' (1017-1066); abbey 'Saint Peter's on Thorney' (959-1066).
- Of market, church, yard, alehouse it lacks in 793-805: none.
- Other kinds at least half the twelve have and it lacks: church (7 of 12 have it), quay (6 of 12 have it), toll (6 of 12 have it).
- Art list rows for this port, portraits left out: card 18, harbour 1, port_mark 1, scene 17, ship_offer 4, story 6, work 6.

### witla (Witla; now lost, the Maas mouth (?))

- Realm franks, region rhine_delta, kind emporium, size 2, landing beach; port open from 0 to 836 (0 = no limit).
- Sites 10 (10 open 793-805), people 7 (7 open), stories 4, works 5 (5 open).
- Site kinds open 793-805: beach, field, fishery, market, mound, saltern, shrine, tavern, toll, workshop.
- Of market, church, yard, alehouse it lacks in 793-805: church, yard.
- Other kinds at least half the twelve have and it lacks: church (7 of 12 have it), lodging (7 of 12 have it), quay (6 of 12 have it), shipyard (6 of 12 have it), graveyard (6 of 12 have it).
- Art list rows for this port, portraits left out: card 7, harbour 1, port_mark 1, scene 10, ship_offer 4, story 4, work 5.

### medemelacha (Medemelacha; now Medemblik)

- Realm franks, region frisia, kind market, size 2, landing jetty; port open from 0 to 0 (0 = no limit).
- Sites 10 (10 open 793-805), people 8 (5 open), stories 6, works 5 (4 open).
- Site kinds open 793-805: church, field, fishery, grove, lodging, market, quay, toll, well, workshop.
- Of market, church, yard, alehouse it lacks in 793-805: yard, alehouse.
- Other kinds at least half the twelve have and it lacks: tavern (8 of 12 have it), shipyard (6 of 12 have it), graveyard (6 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 7, harbour 1, port_mark 1, scene 10, ship_offer 4, story 6, work 5.

### doccinga (Doccinga; now Dokkum)

- Realm franks, region frisia, kind village, size 1, landing beach; port open from 0 to 0 (0 = no limit).
- Sites 11 (8 open 793-805), people 10 (7 open), stories 5, works 5 (4 open).
- Site kinds open 793-805: beach, church, field, graveyard, lodging, thing, well, workshop.
- Sites in the file but not open 793-805: hall 'The Northman's farm' (850-885); market 'The market on the long mound' (1000-0); mint 'Count Bruno's mint' (1038-1057).
- Of market, church, yard, alehouse it lacks in 793-805: market, yard, alehouse.
- Other kinds at least half the twelve have and it lacks: market (10 of 12 have it), tavern (8 of 12 have it), quay (6 of 12 have it), toll (6 of 12 have it), shipyard (6 of 12 have it).
- Art list rows for this port, portraits left out: card 8, harbour 1, port_mark 1, scene 11, ship_offer 3, story 5, work 5.

### daventre (Daventre; now Deventer)

- Realm franks, region rhine_delta, kind town, size 2, landing riverbank; port open from 0 to 0 (0 = no limit).
- Sites 16 (11 open 793-805), people 8 (5 open), stories 5, works 5 (2 open).
- Site kinds open 793-805: church, ferry, field, fishery, grove, lodging, market, quay, shipyard, smithy, tavern.
- Sites in the file but not open 793-805: minster 'Saint Lebuin's basilica' (1040-0); hall 'The bishop's house' (857-920); wall 'The earth wall' (883-0); toll 'The king's toll' (896-0); mint 'The mint' (990-0).
- Of market, church, yard, alehouse it lacks in 793-805: none.
- Other kinds at least half the twelve have and it lacks: workshop (8 of 12 have it), toll (6 of 12 have it), graveyard (6 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 16, harbour 1, port_mark 1, scene 16, ship_offer 5, story 5, work 5.

### quentovic (Quentovic; now Visemarest, La Calotterie)

- Realm franks, region neustria, kind emporium, size 4, landing riverbank; port open from 0 to 0 (0 = no limit).
- Sites 12 (11 open 793-805), people 9 (7 open), stories 5, works 5 (4 open).
- Site kinds open 793-805: abbey, church, field, market, mint, quay, shipyard, storehouse, tavern, toll, workshop.
- Sites in the file but not open 793-805: beach 'The strand at Stapulas' (849-1066).
- Of market, church, yard, alehouse it lacks in 793-805: none.
- Other kinds at least half the twelve have and it lacks: lodging (7 of 12 have it), graveyard (6 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 16, harbour 1, port_mark 1, scene 12, ship_offer 4, story 5, work 5.

### sarre (Serræ; now Sarre, Thanet)

- Realm kent, region england_south, kind market, size 2, landing beach; port open from 0 to 0 (0 = no limit).
- Sites 10 (10 open 793-805), people 8 (5 open), stories 6, works 5 (4 open).
- Site kinds open 793-805: beach, ferry, field, fishery, graveyard, market, shrine, storehouse, tavern, toll.
- Of market, church, yard, alehouse it lacks in 793-805: church, yard.
- Other kinds at least half the twelve have and it lacks: workshop (8 of 12 have it), church (7 of 12 have it), lodging (7 of 12 have it), quay (6 of 12 have it), shipyard (6 of 12 have it).
- Art list rows for this port, portraits left out: card 9, harbour 1, port_mark 1, scene 10, ship_offer 4, story 6, work 5.

### fordwic (Fordwic; now Fordwich)

- Realm kent, region england_south, kind market, size 1, landing riverbank; port open from 0 to 0 (0 = no limit).
- Sites 12 (12 open 793-805), people 8 (6 open), stories 5, works 4 (3 open).
- Site kinds open 793-805: abbey, church, field, ford, gate, market, minster, mint, quay, storehouse, tavern, toll.
- Of market, church, yard, alehouse it lacks in 793-805: yard.
- Other kinds at least half the twelve have and it lacks: workshop (8 of 12 have it), lodging (7 of 12 have it), shipyard (6 of 12 have it), graveyard (6 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 11, harbour 1, port_mark 1, scene 12, ship_offer 4, story 5, work 4.

### ripa (Ripa; now Ribe)

- Realm danes, region jutland, kind emporium, size 3, landing riverbank; port open from 0 to 0 (0 = no limit).
- Sites 12 (10 open 793-805), people 10 (7 open), stories 5, works 5 (4 open).
- Site kinds open 793-805: ford, graveyard, lodging, market, quay, shipyard, shrine, well, workshop x2.
- Sites in the file but not open 793-805: church 'The church on the king's plot' (857-0); mint 'The moneyer's house' (1018-0).
- Of market, church, yard, alehouse it lacks in 793-805: church, alehouse.
- Other kinds at least half the twelve have and it lacks: field (9 of 12 have it), tavern (8 of 12 have it), church (7 of 12 have it), toll (6 of 12 have it), beach (6 of 12 have it).
- Art list rows for this port, portraits left out: card 13, ending 1, harbour 1, port_mark 1, scene 12, ship_offer 4, story 5, work 5.

## Thinnest first

Ranked by what is open in 793-805: sites + people + works, plus stories (stories carry no from/to on the story itself, so all are counted).

| rank | port | size | sites open | people open | stories | works open | sum |
|---|---|---|---|---|---|---|---|
| 1 | daventre | 2 | 11 | 5 | 5 | 2 | 23 |
| 2 | traiectum | 3 | 10 | 5 | 7 | 2 | 24 |
| 3 | doccinga | 1 | 8 | 7 | 5 | 4 | 24 |
| 4 | walichrum | 3 | 12 | 4 | 5 | 4 | 25 |
| 5 | medemelacha | 2 | 10 | 5 | 6 | 4 | 25 |
| 6 | sarre | 2 | 10 | 5 | 6 | 4 | 25 |
| 7 | witla | 2 | 10 | 7 | 4 | 5 | 26 |
| 8 | fordwic | 1 | 12 | 6 | 5 | 3 | 26 |
| 9 | ripa | 3 | 10 | 7 | 5 | 4 | 26 |
| 10 | quentovic | 4 | 11 | 7 | 5 | 4 | 27 |
| 11 | lundenwic | 5 | 12 | 8 | 6 | 4 | 30 |
| 12 | dorestad | 5 | 12 | 11 | 12 | 6 | 41 |

## The three thinnest and what each most needs

The need is read off the counts only: the lowest of its four counts against the middle value of the twelve, and the kinds of site it lacks.

Middle values of the twelve: sites open 793-805 10.5, people open 793-805 6.5, stories 5.0, works open 793-805 4.0.

- daventre (Daventre, size 2, sum 23): furthest below the middle in works open 793-805 (2 against 4.0), then people open 793-805 (5 against 6.5). Lacks in 793-805: none of the four. People in the file not open in 793-805: 3; works not open: 3.
- traiectum (Traiectum, size 3, sum 24): furthest below the middle in works open 793-805 (2 against 4.0), then people open 793-805 (5 against 6.5). Lacks in 793-805: market, yard (of these the file has, but later: market). People in the file not open in 793-805: 4; works not open: 3.
- doccinga (Doccinga, size 1, sum 24): furthest below the middle in sites open 793-805 (8 against 10.5), then stories (5 against 5.0). Lacks in 793-805: market, yard, alehouse (of these the file has, but later: market). People in the file not open in 793-805: 3; works not open: 1.

