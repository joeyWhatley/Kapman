# Wildlife, populations, migration and the farm year

Design document, 4 October 2026. Scope: every living thing the player sees, hears or trades that is not a person: birds, fish runs, seals and whales, deer and the big beasts, insects, and the farmed year (herds, hay, harvest, slaughter, salt, weirs) in each region of the map, 793 to 1066. It is written to build on `Birds.cs` (`Flock`), `LevelClock`, `LevelWeather`, `LevelWater`, `Feasts` and `TownLife`, none of which it rewrites; it leaves sound to audio_design.md (which replaces `LevelSoundscape`) and prices to economy_and_trade.md, and gives both of them their wildlife calendar from one set of data files (2.1, 2.9).

Rules kept: shippable data only (everything here is from public sources; recordings CC0; any animal meshes come from packs the user picks). Birds are the only animals built so far (the rule of 2 October); everything else is designed here so it can be switched on when a pack is in. Guesses are marked (guess). Modern counts are given as the only numbers there are; the text says how to scale them back to 793.

The regions used throughout: `rhine_delta` (Dorestad, the Rhine and Maas mouths, the Zuiderzee-to-be), `frisia` (the Wadden coast, Terschelling to the Weser and Elbe), `thames` (Thames and Kent), `east_anglia` (Essex to the Wash, the Fens), `humber` (Humber, Yorkshire coast, Farnes), `jutland` (Hedeby, Ribe, Limfjord, Skagen), `oslofjord` (Kaupang, the fjord, the Skagerrak shore), `baltic` (Øresund, Scania, Gotland, Birka, the Gulfs), `ireland`, `channel` (Quentovic, Normandy, Brittany, the Solent), `biscay` (Loire to the Basque coast), `iberia` (Galicia to Cadiz), `mediterranean` (Andalus, Marseille, the Camargue, Italy), `far_north` (Hålogaland, Lofoten, Finnmark, Iceland after 870).

---

## 1. What is true

### 1.1 The birds' calendar

Modern windows; in 793 the calendar was the same to within a week or two, the numbers were not (see 1.6 and 1.7). The Julian calendar of the game runs behind the Gregorian dates below by 4 days in 793, 5 days from 1 March 900, and 6 days from 1 March 1000 to 1066 ([Conversion between Julian and Gregorian calendars](https://en.wikipedia.org/wiki/Conversion_between_Julian_and_Gregorian_calendars)), so "1 October" here is about 27 September on the game's clock in 793 and 25 September in 1066. The model subtracts the century's offset (2.2), not a fixed 4.

| Species | Regions (peak in bold) | Comes | Goes | Numbers and habits | Source |
|---|---|---|---|---|---|
| Pink-footed goose, Greenland/Iceland birds | **east_anglia**, humber, Scotland | south mid-Sept to early Oct | north mid-April to early May | 292,000 wintered in Britain in October 2004 (147,000 in Norfolk), only 30,000 in 1950 (shooting kept them down); one flock of 66,000 in Scotland in 2003; fly between roost and feeding at dawn and dusk | [Pink-footed goose](https://en.wikipedia.org/wiki/Pink-footed_goose) |
| Pink-footed goose, Svalbard birds | **frisia** (south-west Friesland), **jutland** (West Jutland), rhine_delta (weight 0.3) | same | same | a separate, much smaller population: about 34,000 wintering in Denmark and the Netherlands in 1993; the two populations are "largely discrete", so the delta never sees the British numbers | same |
| Brent goose, dark-bellied | **thames**, **east_anglia** (over half the population winters in southern England), frisia, rhine_delta, channel, biscay | October (guess at the week) | March to April (guess) | about 250,000; eats eelgrass on the mud; the 1931 eelgrass blight nearly wiped them out: the model of a food crash | [Brant](https://en.wikipedia.org/wiki/Brant_(goose)) |
| Barnacle goose | **frisia**, rhine_delta (Russian birds); Solway (Svalbard birds) | October | April to May | (guess) tens of thousands in the Wadden in 793, as the salt marsh was far larger than now | (guess; modern pattern) |
| Greylag goose | **rhine_delta**, frisia, east_anglia, jutland, baltic | part resident, part winter visitor | breeds here | the wild goose of the delta's reed marshes and the ancestor of the farm goose; the Martinmas bird | (guess from range) |
| White-fronted goose | **rhine_delta**, frisia, thames | October to November | March | the loud, laughing skeins over the Dutch river meadows | (guess from range) |
| Whooper swan | **ireland**, humber, east_anglia (Iceland birds); jutland, rhine_delta, baltic (Fennoscandian birds) | October to December (a flock was recorded crossing to Ireland on 9 Dec) | March to April | families stay together; pairs for life; signal before a flock lifts | [Whooper swan](https://en.wikipedia.org/wiki/Whooper_swan) |
| Bewick's swan | **rhine_delta**, east_anglia, thames | mid-October | early March | (guess) a few thousand in the delta's wet basins | (guess) |
| Red knot | **frisia** (the Wadden), east_anglia (the Wash), humber, thames | July to August | May | huge tight flocks that wheel as one; a flock's feeding range covers 800 km² in a week and it changes roost each week | [Red knot](https://en.wikipedia.org/wiki/Red_knot) |
| Shelduck | **frisia** | all year | moult gathering July to August | moult flocks of 100,000 on the Wadden Sea; nests in holes (no rabbits in 793: rabbits came with the Normans, so holes in dunes and under roots) | [Common shelduck](https://en.wikipedia.org/wiki/Common_shelduck) |
| Curlew, oystercatcher, dunlin, godwit, grey plover | **frisia**, east_anglia, humber, thames, jutland, ireland | Aug to Oct on passage; winter | April to May | the Wadden Sea carries "hundreds of thousands of waders, ducks and geese" at a time; the trilateral figure of 10-12 million birds a year passing is (guess) from memory, the page did not load | [Wadden Sea](https://en.wikipedia.org/wiki/Wadden_Sea) |
| Barn swallow | all regions south of the far north | spring, early to mid-April in Britain and the delta (guess at the week; the RSPB page gives no date) | September to early October, gathering in reedbeds first | the "early sign of summer"; nests in barns and under wharves; feeds low over water in bad weather | [RSPB swallow](https://www.rspb.org.uk/birds-and-wildlife/swallow), [Barn swallow](https://en.wikipedia.org/wiki/Barn_swallow) |
| Common swift | all regions with towns | second half of April to first third of May | early August (breeding females last) | screaming parties of 10-20 round the roofs at dusk; fly low before rain; ride low-pressure fronts | [Common swift](https://en.wikipedia.org/wiki/Common_swift) |
| Common crane | **rhine_delta**, jutland, baltic, channel, biscay, iberia on passage; **east_anglia** (the Fens) as a breeder in 793 | autumn passage August to early December (peak late Oct to mid-Nov, guess); spring February to May (peak early March, guess) | | flocks up to 400 in a V, thousands at staging sites; bred in the English fens until extirpated in the late medieval / 17th century; winters in Spain | [Common crane](https://en.wikipedia.org/wiki/Common_crane), [extinct animals of the British Isles](https://en.wikipedia.org/wiki/List_of_extinct_animals_of_the_British_Isles) |
| White stork | **rhine_delta**, frisia, jutland, baltic, channel, biscay, **iberia** | late March to April | August | nests on roofs and churches; crosses at Gibraltar, never the open sea; no English nest but one on St Giles, Edinburgh, 1416: so none in thames/east_anglia/humber | [White stork](https://en.wikipedia.org/wiki/White_stork) |
| Grey heron | all | resident | | heronries start in Feb to June; birds forage up to 20 km from the nest; a hard winter crashes them (1962-63) and they recover in a few years; 400 herons served at Archbishop Neville's feast, 1465 | [Grey heron](https://en.wikipedia.org/wiki/Grey_heron) |
| Northern gannet | **humber** (Bass Rock, 75,000+ pairs in 2014, 150,000 birds), St Kilda 60,000, Ailsa Craig 33,000; Heligoland and Runde are modern colonies (none in 793) | January to February | October | nest from March/April; plunge-dive at 60 mph on herring and mackerel; forage 320 km out; young ("gugas") were harvested | [Northern gannet](https://en.wikipedia.org/wiki/Northern_gannet), [Bass Rock](https://en.wikipedia.org/wiki/Bass_Rock) |
| Puffin, guillemot, kittiwake, tern, eider | **humber** (Farnes: 36,211 puffin pairs, 59,168 guillemots, 4,772 kittiwake pairs, 882 Arctic tern pairs, 417-1,000 eider pairs), far_north, ireland, Heligoland (guillemots; chicks jump in June) | April | July | St Cuthbert's eider rule of 676 is legend (first written 500 years later) but the Farne eiders are "Cuddy's ducks" | [Farne Islands](https://en.wikipedia.org/wiki/Farne_Islands), [Common eider](https://en.wikipedia.org/wiki/Common_eider), [Heligoland](https://en.wikipedia.org/wiki/Heligoland) |
| Great bustard | **east_anglia**, Salisbury Plain; Germany, Denmark | resident | | leks in March; gatherings of several dozen in winter; the heaviest flying bird; last British bird shot 1832 | [Great bustard](https://en.wikipedia.org/wiki/Great_bustard) |
| Dalmatian pelican | none in play | | | bones from 1900-600 BP in the Netherlands and Britain, gone from Britain c. 3000 BC by the list: left out, or (guess) one legendary sighting event in the Fens | [Dalmatian pelican](https://en.wikipedia.org/wiki/Dalmatian_pelican) |
| Greater flamingo | **mediterranean** (the Camargue's brine ponds, "one of the few European habitats") | resident/dispersive | | 400 bird species in the Camargue; mosquitoes | [Camargue](https://en.wikipedia.org/wiki/Camargue) |

Resident birds that need no table: gulls (herring, black-headed, great black-backed), crows, rooks, ravens, jackdaws, magpies, skylarks, blackbirds, wrens, owls (tawny, barn, short-eared on the marshes), kites and white-tailed eagles (common scavengers in every 793 town; gone from Britain 1916), buzzards, kestrels, ducks (mallard, teal, wigeon in winter, pintail), coot, moorhen, cormorant, bittern (booms April to June in reed), nightingale (May to June nights, south of the Humber), cuckoo (late April to June), lapwing (winter flocks, display in March). The CC0 recordings in `G:\kapman_unity\data\sounds_cc0` already cover gulls, geese, curlew, oystercatcher, crow, raven, rooks, swallows, heron, mallard, blackbird, nightingale, skylark, tawny owl, crickets.

### 1.2 The fish runs and the sea beasts

| Run | Where | When | Facts | Source |
|---|---|---|---|---|
| Herring, Buchan-Shetland spawners | humber (north), far_north (south) | August to September | the North Sea has four spawning groups; a shoal can fill up to 4.8 km³ at 0.5-1 fish/m³ (Radakov's estimate, re-checked on the page): millions of fish | [Atlantic herring](https://en.wikipedia.org/wiki/Atlantic_herring) |
| Herring, Dogger Bank spawners | humber, east_anglia | August to October | | same |
| Herring, Downs spawners | thames, east_anglia, channel (the Southern Bight, off Yarmouth and Dunwich) | November to January | Dunwich, c. 3,000 people in 1086 with three churches and already losing land to the sea, was a herring town; the Yarmouth autumn fair is later (Michaelmas to Martinmas, guess for the exact medieval dates) | [Dunwich](https://en.wikipedia.org/wiki/Dunwich), same |
| Herring, Baltic spring spawners | baltic, oslofjord | March to May | reach the North Sea by the Skagerrak | same |
| Herring, the Øresund (Scania) | baltic, jutland | September to October in the sound; the market ran 24 August to 9 October | the market is 12th century and later (300,000 people at its height, "so thick you can cut them with a dagger", collapse early 15th c.). In 793-1066 the shoals are there, the market is not: the fishing is local and Hedeby's | [Scania Market](https://en.wikipedia.org/wiki/Scania_Market) |
| Herring, Norwegian spring spawners | far_north, oslofjord | February to March (guess at the month) | the Bohuslän periods (whole decades of herring, then none) are post-1066 | (guess) |
| Cod, the skrei | far_north (Lofoten, Vågan: Viking-age town) | January to April | "for more than 1,000 years" the winter cod gather to spawn; dried on racks (hjell) from late winter in frost and wind; stockfish went south by Bergen (later) | [Lofoten](https://en.wikipedia.org/wiki/Lofoten) |
| Silver eel (the run down) | rhine_delta, frisia, east_anglia, thames, jutland, baltic, ireland | October to November, into December if mild | moves on stormy, rainy nights with rising water; the fish turns silver over four weeks before it goes | [Europäischer Aal](https://de.wikipedia.org/wiki/Europ%C3%A4ischer_Aal) |
| Glass eel and elver (the run up) | all estuaries | February to May at the coast; spring ascent in great shoals | | same |
| Eel as rent | east_anglia (Ely = "district of eels") | | Domesday: Stuntney 24,000 eels a year, Littleport 17,000, Little Thetford 3,250; the Fen economy was eels, wildfowling, reed, sedge, peat | [Ely](https://en.wikipedia.org/wiki/Ely,_Cambridgeshire) |
| Salmon | rhine_delta (the Rhine was Europe's greatest salmon river), ireland, humber, far_north, biscay, oslofjord | runs through spring and summer; spawn late autumn to early winter | last Rhine salmon 1950; 71-76 cm, 3.6-5.4 kg after two sea years | [Atlantischer Lachs](https://de.wikipedia.org/wiki/Atlantischer_Lachs) |
| Sturgeon | rhine_delta, frisia (Elbe, Weser), baltic (Vistula), biscay (Gironde) | spring to early summer run (guess: May to June) | "numerous" in the Rhine, Elbe, Weser and Vistula into the 19th century; up to 3.4 m and 300 kg (6 m claimed) | [Europäischer Stör](https://de.wikipedia.org/wiki/Europ%C3%A4ischer_St%C3%B6r), [European sea sturgeon](https://en.wikipedia.org/wiki/European_sea_sturgeon) |
| Allis shad | rhine_delta, biscay, channel (Severn is off the map) | April to June | 30-60 cm; now critically endangered | [Allis shad](https://en.wikipedia.org/wiki/Allis_shad) |
| Bluefin tuna | iberia, mediterranean (the Strait) | into the Mediterranean February to July to spawn, out again late summer | the almadraba traps; the Dukes of Medina Sidonia held them from the 12th century; Phoenician and Roman before | [Almadraba](https://en.wikipedia.org/wiki/Almadraba) |
| Right whale | biscay (Basque coast) | October/November to February/March, peak January | a 670 record of 40 casks of whale oil from Bayonne; a 1059 Bayonne market rule for whale meat; stone watchtowers (vigías) and a straw fire or bell when one is seen; tongues to the church | [Basque whaling](https://en.wikipedia.org/wiki/Basque_whaling) |
| Harbour porpoise | all the North Sea | all year, inshore in summer | 339,000 in the North Sea (2022); alone or in fives; avoid boats, roll quietly, never bow-ride; a drive hunt in the Little Belt until 1899; a nobles' dish | [Harbour porpoise](https://en.wikipedia.org/wiki/Harbour_porpoise) |
| Sperm whale strandings | frisia, jutland, rhine_delta | winter (guess: the young bulls' habit is well recorded in modern times; the page did not say) | 16 m, up to 57 t; spermaceti, ambergris, teeth | [Sperm whale](https://en.wikipedia.org/wiki/Sperm_whale) |
| Mayfly (Ephemera danica) | every clean river | May to June | nymphs 1-3 years in the gravel; adults four days, do not feed; trout and swallows feast | [Ephemera danica](https://en.wikipedia.org/wiki/Ephemera_danica) |

Dorestad itself: 19 fish species in the sieved bone, bream kept alive in pools, salted and smoked sea fish brought in, charters that grant *piscationes*, iron net sinkers and a hook (Research/dorestad_c800_notes.md, citing Prummel and Brinkhuizen). Sturgeon and salmon are expected in the great rivers but poorly preserved.

### 1.3 Seals, deer and the big beasts

| Animal | Where in 793 | Season | Facts | Source |
|---|---|---|---|---|
| Grey seal | humber (Farnes), east_anglia, ireland, frisia, baltic, far_north | pups: Ireland and SW Britain Sept; east coast Oct-Dec; Wadden (Heligoland's Düne) Nov-Jan; Baltic Feb-Mar on the ice | the Baltic seals haul out at night; nearly wiped out by bounties in later centuries. (guess) grey seals were hunted out of the Wadden Sea in the Middle Ages and only returned in the 1950s; in 793 the model keeps them, fewer each century | [Grey seal](https://en.wikipedia.org/wiki/Grey_seal), [Farne Islands](https://en.wikipedia.org/wiki/Farne_Islands), [Heligoland](https://en.wikipedia.org/wiki/Heligoland) |
| Harbour seal | **frisia**, rhine_delta, thames, east_anglia, humber, jutland, baltic, oslofjord | pups June-July; mating July to early Sept; moult August on the sandbanks | lie on sandbanks that dry at low water; in 1988 the phocine distemper virus killed about 18,000 harbour seals along the north European coasts, about two thirds of the population (so about 27,000 in the North Sea before it; the Wadden Sea held the largest part, and the Norwegian and Icelandic coasts were hardly touched); 353,329 seals were killed for bounties in the Baltic 1886-1927 | [Seehund](https://de.wikipedia.org/wiki/Seehund), [Phocine morbillivirus](https://en.wikipedia.org/wiki/Phocine_morbillivirus) |
| Ringed seal | baltic (the Gulfs) | pups March-April in snow lairs on the ice | 200,000 in 1900, 4,000 by the 1970s | [Ringed seal](https://en.wikipedia.org/wiki/Ringed_seal) |
| Walrus | far_north | summer | Ohthere (c. 890): walrus of 7 ells, sixty killed in two days by five men; whales of 50 ells; "best for whale-hunting" | [Ohthere](https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland) |
| Reindeer | far_north | | Ohthere had 600 tame reindeer, six of them decoys for catching wild ones; the Sami tribute: 15 marten skins, 5 reindeer hides, 1 bear skin, 10 ambers of feathers, a bear- or otter-skin coat, two 60-ell ship ropes of whale or seal hide | same |
| Red deer | all wooded and upland regions | rut late Sept to Nov (roaring); calves May-June; antlers cast in spring, velvet off by autumn; hinds in herds up to 50 | | [Red deer](https://en.wikipedia.org/wiki/Red_deer) |
| Wild boar | all mainland regions; Britain until c. 1400 (or the 13th c.) | rut Nov to Jan; farrow March to May; sounders led by an old sow | Carolingian forests were full of them; pannage pigs ran half-wild beside them | [Wild boar](https://en.wikipedia.org/wiki/Wild_boar), [extinct animals of the British Isles](https://en.wikipedia.org/wiki/List_of_extinct_animals_of_the_British_Isles) |
| Elk (moose) | baltic, oslofjord, far_north; gone from the Netherlands in the 9th century and from France after Charlemagne | rut Sept-Oct | Sweden holds 300-400,000 today | [Moose](https://en.wikipedia.org/wiki/Moose) |
| Beaver | rhine_delta, frisia (rivers), baltic, oslofjord, far_north, channel, biscay; Britain thin (last record 1526); Denmark gone before our period | all year; most seen at dusk | hunted for fur and castoreum; Netherlands last 1826, Sweden c. 1870; by 1900 only 1,200 left in Europe | [Eurasian beaver](https://en.wikipedia.org/wiki/Eurasian_beaver) |
| Wolf | every mainland region; Britain and Ireland throughout the period | winter hunts: Charlemagne's *Capitulare de villis* c. 69 orders every steward to report the wolves taken, hand in the pelts, and in May hunt the cubs "with poison and hooks, with pits and dogs" (*De lupis omni tempore nobis adnuntient, quantos unusquisque conpraehenderit, et ipsas pelles nobis praesentare faciant. Et in mense Maio illos lupellos perquirant et conpraehendant, tam cum pulvere et hamis, quamque cum fossis et canibus*); pups in spring; packs howl from autumn | King Edgar's 300 wolf pelts a year from Wales (957) is first recorded by William of Malmesbury in the 12th century (a tale, not a tax roll); the "Wolf-monath" name for January that the Wolves-in-Britain page credits to the Anglo-Saxon Chronicle is not period (the Chronicle has no month names; Bede gives *Giuli*), so the game does not use it; Gerald of Wales has wolves eating the dead at Holywell in 1165; last wolves England c. 1500, Scotland 1680, Ireland 1786; the Irish annals of 690 have a wolf that spoke | [Capitularia (Carolus Magnus), la.wikisource](https://la.wikisource.org/wiki/Capitularia_(Carolus_Magnus)), [Wolves in Great Britain](https://en.wikipedia.org/wiki/Wolves_in_Great_Britain), [Wolves in Ireland](https://en.wikipedia.org/wiki/Wolves_in_Ireland), [Germanic calendar](https://en.wikipedia.org/wiki/Germanic_calendar) |
| Brown bear | oslofjord, baltic, far_north, iberia (Pyrenees, Cantabria), the German uplands; gone from Britain c. 500 and from Ireland in prehistory | dens in winter | a bear skin is in Ohthere's tribute | [extinct animals of the British Isles](https://en.wikipedia.org/wiki/List_of_extinct_animals_of_the_British_Isles), [Ohthere](https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland) |
| Lynx | oslofjord, baltic, far_north, iberia; Britain gone c. 700 | | | same list |
| Aurochs | none on the map (Poland only, last 1627; Britain gone c. 1000 BC) | | kept out | [Aurochs](https://en.wikipedia.org/wiki/Aurochs) |
| Otter, fox, marten, hare, badger, polecat, wildcat | all | | the fur trade's small skins; Ireland's wildcat is extinct | [Fauna of Ireland](https://en.wikipedia.org/wiki/Fauna_of_Ireland) |

Ireland has 27 native land mammals, no snakes, native red deer, grey wolves to 1786, bears and lynx long gone; 375 coastal and 40 freshwater fish, salmon chief among them.

### 1.4 Insects

- **Mosquitoes of the marsh.** *Anopheles atroparvus* breeds in shallow brackish water and was the Netherlands' main malaria vector ("ague", the marsh fever); it keeps biting through winter in houses and byres without laying eggs ([Anopheles atroparvus](https://en.wikipedia.org/wiki/Anopheles_atroparvus)). The Rhine delta, Frisia, the Fens, the Thames marshes and the Camargue are its country.
- **Midges.** The highland midge flies late spring to late summer, worst before dawn and at dusk, and stops above 3 m/s of wind, in dry sun, and below 60-75% humidity ([Culicoides impunctatus](https://en.wikipedia.org/wiki/Culicoides_impunctatus)): Scotland, the Norwegian coast, Ireland, the far north.
- **Mayflies** in May-June on gravel rivers (1.2). Crickets on summer nights are already in the soundscape. Horseflies in hayfields in July, wasps at the fruit in September, cockchafers in May dusk (guess, common knowledge, no source fetched).

### 1.5 The farmed year

The names people gave the months are the farm year in one line each.

| Month | Bede's Old English (725) | Einhard's Frankish (Charlemagne's names) | What the land shows |
|---|---|---|---|
| Jan | Æftera Geola, after Yule (Bede: *Giuli*) | Wintarmanoth, winter month | the wolf hunts and the pelts coming in (1.3); byres full; ice |
| Feb | Solmonath, Bede: the month of cakes offered to the gods ("mud month" is a later etymology; recent finds of *sol* = hearth-cake support Bede) | Hornung, horn-shedding | first ploughing on dry land; lambing begins late Feb (guess) |
| Mar | Hrethmonath | Lentzinmanoth, spring month | ploughing and sowing oats and barley; rooks build |
| Apr | Eosturmonath | Ostarmanoth, Easter month | cattle out to grass; calving; first swallows |
| May | Thrimilce, three milkings a day | Winnemanoth, pasture month | the flush of milk; cheese and butter; sheep washed and shorn late May-June (guess) |
| Jun | Ærra Litha, before midsummer | Brachmanoth, fallow-plough month | haymaking starts; midsummer fires (Feasts) |
| Jul | Æftera Litha | Hewimanoth, hay month | hay carted; stacks go up |
| Aug | Weodmonath, weed month | Aranmanoth, harvest month | wheat and rye cut; Lammas 1 Aug; stubble; gleaners; swifts gone |
| Sep | Haligmonath, holy month | Witumanoth, wood month | threshing; Michaelmas 29 Sept; pigs to the woods |
| Oct | Winterfylleth, winter full moon | Windumemanoth, vintage month | ploughing and winter sowing; acorns; wine on the Rhine; geese arrive |
| Nov | Blotmonath, blood month | Herbistmanoth, autumn month | Martinmas slaughter 11 Nov; salting; feathers; eels |
| Dec | Ærra Geola, before Yule | Heilagmanoth, holy month | pig killing and baking; Yule |

Sources: [Germanic calendar](https://en.wikipedia.org/wiki/Germanic_calendar), [Solmonath](https://en.wikipedia.org/wiki/Solm%C5%8Dna%C3%BE); the labours-of-the-months cycle (June hay, July wheat, August threshing, September grapes, October ploughing/sowing, November acorns for pigs, December pig killing; Italy a month earlier) [Labours of the Months](https://en.wikipedia.org/wiki/Labours_of_the_Months). The Gerefa page was not reachable; the lambing, shearing and calving weeks are guesses from the May name and general farming.

- **Pannage.** Pigs go to the oak and beech woods when the mast falls; Domesday valued woods by their swine renders; the New Forest still runs a minimum of 60 days from when the acorns fall ([Pannage](https://en.wikipedia.org/wiki/Pannage)). In the game: Michaelmas to Martinmas.
- **Martinmas, 11 November.** Livestock slaughtered for winter provision; the goose the dish (duck or hen for the poor); "Martinmas beef"; bonfires on the eve in the Rhineland; the first wine tasted ([Martinmas](https://en.wikipedia.org/wiki/Martinmas)). `Feasts.cs` already lights the fires.
- **Salt.** The delta and Zeeland made salt by burning salt-soaked peat (*moernering*, *selnering*): dig the *darink* from under the clay, dry it, burn it, leach the ash with sea water, boil: about 15 kg of salt per m³ of peat; the trade peaked in the 13th century and was banned in Zeeland in 1515 for the land it sank ([Moernering](https://nl.wikipedia.org/wiki/Moernering)). In 793 it is the small summer trade of the Frisian and Zeeland coast (guess for its scale). Droitwich's brine springs are inland of the map; the Camargue's salt abbeys (Psalmody) and the Mediterranean pans dry salt in the summer sun ([Camargue](https://en.wikipedia.org/wiki/Camargue)). Boiling needs dry fuel and the dried peat: June to September (guess).
- **Weirs and traps.** V-weirs and wicker traps in side channels and basin gullies (Research notes, "likely"); eel traps set on the autumn nights; salmon weirs in spring and summer; herring with drift nets at night when the shoals rise.
- **Fowling.** The Fen economy was eels, wildfowling, reed, sedge and peat ([Ely](https://en.wikipedia.org/wiki/Ely,_Cambridgeshire)). Nets and snares on the marsh in winter, when the geese and ducks are in; eggs from the cliffs in May-June (the Farnes, the Bass, Heligoland); eiderdown plucked from the nest after the ducklings leave ([Common eider](https://en.wikipedia.org/wiki/Common_eider)); the Oseberg burial (autumn 834 by dendrochronology; the ship's timbers from c. 800) held feather and down bedding according to the [Eiderdown (bedding)](https://en.wikipedia.org/wiki/Eiderdown_(bedding)) page; the [Oseberg Ship](https://en.wikipedia.org/wiki/Oseberg_Ship) page lists the bed but not its filling: check before the crew say it; Ohthere's "ten ambers of feathers".
- **The byre and the yard, heard.** The library already holds CC0 livestock recordings (cattle_163727, cattle_513565, sheep_200329, sheep_210511, pigs_158746, pigs_692838, chickens_197103, chickens_553068, dogs_440865, dogs_832435, horses_416873, horses_538438; `CREDITS_sounds.md`), so the farm year can be heard now without any pack: lowing from the byres in winter, ewes and lambs at lambing, pigs at the wood edge in pannage, the slaughter yard at Martinmas. Only the meshes wait for a pack (6, question 1).
- **Markets and feasts the animals set.** Easter (lambs, eggs), Thrimilce (new cheese), St John's Eve fires at the hay, Lammas (first bread), Michaelmas (geese fattened on the stubble; rents), Martinmas (meat, hides, feathers, tallow; the hiring of hands), the Scania and Yarmouth herring seasons in later centuries, the Lofoten winter.

### 1.6 Numbers: people and stock to size the herds

- England 1086: about 1.1 million people (Russell), 1.7 million for the British Isles ([Demography of England](https://en.wikipedia.org/wiki/Demography_of_England)); in 793 (guess) 0.6-0.9 million.
- Dorestad c. 800: cattle by far the main animal, then sheep and goat, pig, horse; 5-6 km² of high land in its territory for fields and stock; the basin edges grazed (Research notes, "sure"). A rule of thumb for the game: one head of cattle per 1.5 ha of grazed levee and basin edge, two sheep per cow, one pig per two households, one horse per ten (guess). For Dorestad's square that is about 300-500 cattle, 600-1,000 sheep, 200-400 pigs, 50-100 horses, 1,000-2,000 geese and hens (guess).
- Flocks and herds the player sees are fractions of these: a herd on a meadow 8-30 cattle, a flock 20-80 sheep, a drove of pigs 10-25, a farmyard's geese 6-20.

### 1.7 Year to year, and 793 against 1066

Famines and hard years in our window ([List of famines](https://en.wikipedia.org/wiki/List_of_famines)): 791-794 (heavy rain; England 793), 805-807 (hard winter and drought), 820-824 (heavy rain, hard winters, hail, cattle pest; the Katla eruption 822/823; rivers frozen for weeks: the Royal Frankish Annals' winter of 821-822, from memory), 841-845 (rain, hard winters), 867-869 (rain, floods, cattle pest), 872-874 (hail, locusts, a hard winter, drought, Saharan dust), 895-897 (hail, hard winter), 939-944 (hard winters, floods, locusts), 1005-1006, 1016, 1045 (England). The winter of 1010-1011 froze the Bosporus and the Nile ([Liste von Wetterereignissen](https://de.wikipedia.org/wiki/Liste_von_Wetterereignissen_in_Europa)). The Medieval Warm Period runs c. 950-1250, exceptionally warm in western Europe, Iceland (settled c. 870) and Greenland (985) ([Medieval Warm Period](https://en.wikipedia.org/wiki/Medieval_Warm_Period)). The Frisian flood of 26 December 838 (Annals of St-Bertin, 2,437 drowned) is from memory: the Chronicle already carries it.

What a hard winter does, from the best-recorded modern one (1962-63): the sea froze a mile out at Herne Bay and 6 km off Dunkirk, the upper Thames froze solid, inland water birds lost their last resort in the estuaries ([Winter of 1962-63](https://en.wikipedia.org/wiki/Winter_of_1962%E2%80%9363_in_the_United_Kingdom)); herons crashed and took years to recover ([Grey heron](https://en.wikipedia.org/wiki/Grey_heron)). A food crash does the same to one species: the 1931 eelgrass blight nearly ended the brent goose ([Brant](https://en.wikipedia.org/wiki/Brant_(goose))).

| Thing | 793 | 1066 | Why |
|---|---|---|---|
| Beaver on the Rhine, Maas, Scheldt, Thames | common (guess: a lodge every few km of quiet bank) | half as many in the delta, rare in England | fur and castoreum; the trend to 1526/1826 |
| Elk in the Rhine delta | a last few on the peat and marsh edges east of the Vecht and on the wet side of the Gelderse Vallei (the Moose page: gone "as the marshlands were drained"; the Veluwe's dry sand is not its country); probably gone soon after | none | gone "by the 9th century" |
| Wolves in England | common | still common; Malmesbury's tale of Edgar's 300 pelts (12th c.) at least shows the memory of a stock | gone only c. 1500 |
| Bears in Britain | none (gone c. 500) | none | |
| Boar in England | yes | yes | gone c. 1400 |
| Cranes breeding in the Fens | yes | yes, fewer as the Fens are reclaimed around abbeys | extinct late medieval |
| Storks on the roofs | delta, Frisia, Denmark, Francia, Iberia | same | |
| Herring fishing | local, Hedeby, the Frisian and Flemish coast | the Downs fishery off Yarmouth and Dunwich growing; Scania still before its fair | Dunwich 3,000 in 1086 |
| Springs | a cool century (famines of the 820s-890s) | the warm period: first swallows about 5 days earlier, fewer frozen rivers (guess at 5 days) | MWP 950-1250 |
| Rabbits | none | none (Norman) | |
| Fallow deer | none | none (Norman) | |
| Salt marsh and reed | far larger; the Almere not yet the Zuiderzee | reclaimed edges, first dikes; the Zuiderzee opening (12th c.) | |

---

## 2. The model

### 2.1 Data files (`G:\kapman_unity\data\wildlife\`, copied into `Assets/Kapman/Data/wildlife/` by the bake)

**`regions.json`** - the fourteen land regions as lat/lon polygons (a few vertices each, not shapes to draw), plus per-region habitat weights, plus the sea regions (`southern_bight`, `german_bight`, `skagerrak`, `kattegat`, `irish_sea`, `channel_sea`, `norwegian_sea`, `biscay_sea`, `baltic_sea`) with `open_sea`, `bank` and `cliff` habitats only. Each region lists the `chronicle_793.json` region ids it answers to (the chronicle uses finer ids: `zeeland`, `rhineland`, `schleswig`, `saxony` ...), so the Chronicle's `regions` arrays can be matched without renaming either file.

```json
{"regions":[
 {"id":"rhine_delta","name":"The Rhine delta","kind":"land","polygon":[[51.3,3.2],[52.6,3.9],[52.6,6.2],[51.3,6.2]],
  "chronicleIds":["rhine_delta","zeeland","rhineland","meuse"],
  "habitats":{"wet_meadow":1.0,"reed":1.0,"alder_carr":1.0,"mud":0.6,"saltmarsh":0.4,"field":0.8,"wood":0.5,"dune":0.3,"cliff":0,"open_sea":0.3,"brackish":0.8}},
 {"id":"southern_bight","name":"The southern North Sea","kind":"sea","polygon":[[51.0,1.5],[53.5,1.5],[53.5,4.8],[51.0,4.8]],
  "chronicleIds":["frisia","zeeland","flanders","kent","essex"],
  "habitats":{"open_sea":1.0,"bank":0.6,"cliff":0}}
]}
```

`Regions.At(lat, lon, kind)` returns the first polygon of the matching kind (`land` for ports and river places, `sea` for `LevelKind.Ocean`) containing the point (point-in-polygon on the lat/lon plane; good enough at this size), falling back to the nearest centroid of that kind. The land and sea polygons overlap on purpose (Katwijk is inside both), which is why the kind is an argument. `LevelInfo` gains an optional `region` string: empty means "by lat/lon and kind"; a level that sits awkwardly (the Thames mouth, the Limfjord) names its region outright.

**`species.json`** - one entry per species or species group (about 60 at the end; 14 in the first slice).

```json
{"id":"pinkfoot","name":"Pink-footed goose","group":"geese","kind":"bird",
 "mesh":"bird_v","wingspan":1.5,"colour":[0.2,0.18,0.16],
 "range":{"frisia":1.0,"rhine_delta":0.3,"east_anglia":1.0,"humber":0.5,"jutland":0.9,"thames":0.2},
 "perKm2":60,
 "density":{"wet_meadow":1.0,"saltmarsh":1.0,"mud":0.7,"field":0.5},
 "presence":[[1,1.0],[100,1.0],[120,0.1],[130,0.0],[255,0.0],[270,0.4],[285,1.0],[365,1.0]],
 "jitterDays":7,"warmShiftDays":-4,
 "hours":{"dawn":1.0,"day":0.5,"dusk":1.0,"night":0.3},
 "weather":{"windMax":16,"rain":0.8,"fog":0.6,"frost":1.0,"storm":0.2},
 "flock":{"min":12,"max":400,"typical":40,"formation":"skein"},
 "behaviours":["graze","roost_water","flight_line","migrant_skein","startle"],
 "flight":{"height":[40,120],"speed":16,"flapHz":4},
 "voice":["curlew_829120#pinkfoot","geese_407490"],"voiceRate":0.6,
 "omen":null,
 "migration":{"autumn":[255,300,"SW"],"spring":[90,125,"NE"],"skeinsPerDay":6,"height":[200,900]},
 "trend":[[793,1.0],[1066,1.0]],
 "hardWinter":{"crash":0.15,"recoverYears":1},
 "goods":{"feathers":0.02,"goose":0.0}}
```

Two more records, abbreviated, to fix the clip ownership and show the livestock form:

```json
{"id":"greylag","name":"Greylag goose","group":"geese","kind":"bird","perKm2":25,
 "range":{"rhine_delta":1.0,"frisia":0.8,"east_anglia":0.6,"jutland":0.6,"baltic":0.6},
 "presence":[[1,1.0],[365,1.0]],"hours":{"dawn":1.0,"day":0.4,"dusk":1.0,"night":0.3},
 "voice":["geese_flock_landing_484825#greylag","geese_wild_meadow_535067"],"voiceRate":0.5}

{"id":"cattle","name":"Cattle","group":"stock","kind":"stock","mesh":null,"perKm2":0,
 "range":{"*":1.0},
 "presence":[[1,1.0],[365,1.0]],
 "places":{"byre_sound":{"window":[305,95],"anchor":"farm","hours":{"dawn":1.0,"day":0.6,"dusk":1.0,"night":0.3}},
           "pasture":{"window":[95,305],"anchor":"meadow","hours":{"dawn":0.8,"day":1.0,"dusk":0.8,"night":0.1}},
           "slaughter":{"window":[305,325],"anchor":"yard","hours":{"day":1.0}}},
 "voice":["cattle_163727","cattle_513565"],"voiceRate":0.4,
 "omen":{"sign":"cattle_lying","reliability":0.6,"predicts":"rain"}}
```

- `perKm2`: the species' count per km² of its best habitat, in 793. Set it from the modern counts in 1.1 divided by the region's area of that habitat (the Svalbard pink-feet: 34,000 over roughly 600 km² of Frisian and West Jutland wet meadow gives about 55-60; the figure above is a guess at that sum), then × 0.3 for the species whose modern numbers are recovery from shooting (geese, swans, gannets, seals: the 1950 figures are nearer 793 than the 2004 ones) and × 1 for the rest. The validator (4.1) refuses a species record without it.
- `presence`: a piecewise-linear curve of (day-of-year, 0..1). Linear interpolation; the two ends join.
- `jitterDays`: each year's whole curve slides by `jitter * (2·hash(seed, y, s) - 1)` in [-1, 1], so the geese are early one year and late the next.
- `warmShiftDays`: the slide for a fully warm year (the MWP index below), negative = earlier.
- `hours`: multipliers by the Sun's altitude band (dawn = altitude -6 to +10 and rising, dusk the same falling, night below -6, day the rest), smoothed.
- `weather`: `windMax` is the wind (m/s) above which the species does not fly (it sits); `rain`, `fog`, `storm` are multipliers on visible activity at full strength of each; `frost` the multiplier under frost.
- `formation`: `skein` (V), `cloud` (waders, starlings), `line` (cormorants, swans), `pair`, `loose` (gulls, crows), `stand` (herons, waders feeding), `raft` (ducks on water).
- `voice`: clip ids from `sounds_cc0`; `clip#tag` names a cut of a clip (the cuts are listed in `CREDITS_sounds.md` with a `species` column, so a clip can only be assigned to a species the credits name for it). The pink-foot calls are in `curlew_829120`, which is a pink-footed goose collage with curlew in it, and in `geese_407490`; `geese_flock_landing_484825` is a mixed Canada and greylag flock (Hanningfield, Essex) and only its greylag passages are used, under greylag: Canada geese were not in 793 Europe (`CREDITS_new.csv` notes, and audio_design.md's clip list). This file is the one wildlife calendar: audio_design.md's WILDLIFE voices bus reads `presence`, `hours`, `weather`, `voice` and `voiceRate` from here and keeps no table of its own (its 1.4 becomes the source list for these fields).
- `omen`: null, or the sign this species gives, with its reliability (2.11).
- `kind: "stock"`: a farmed animal, no mesh until a pack is in, heard at the builder's anchors (`DorestadBuilder` already places 37 farms and kraals; `yard` is the slaughter yard, `meadow` the grazed basin edge, `wood` the pannage edge). `places` are its presence windows per place. Entries: cattle, sheep (lambing 50-95 near farms, pasture 95-305, washed and shorn 140-170 at the river), pigs (yard all year, `pigs_in_wood` 272-315 at the wood edge, slaughter 305-325 and 335-360), farm geese (yard; the pond; loud at Martinmas), hens (yard, dawn), dogs (every farm, loudest at night and when the ship comes), horses (yard, the road).
- `migration`: passage windows (day-of-year start, end, bearing) for skeins passing over at height; `skeinsPerDay` is the rate at the peak of the window (2.3's shape multiplies it).
- `trend`: long-term multiplier by year, piecewise linear.
- `hardWinter`: fraction lost after a hard winter, years to recover (2.7).
- `goods`: ids in `goods_793.json` this species feeds, with the per-head yield; the economy reads it (2.9).

**`farm_year.json`** - per region, the tasks with their windows, what they show on the land, what is heard, and the supply curve amplitudes the economy reads (2.9: `supply` is the peak change in supply of a good while the task is on, positive = more of it; the economy turns it into price).

```json
{"region":"rhine_delta","tasks":[
 {"id":"lambing","window":[50,95],"show":["lambs_in_fold"],"voice":["sheep_210511","sheep_200329"],"anchor":"farm","supply":{"sheep":0.1}},
 {"id":"cattle_out","window":[95,110],"show":["herds_on_meadow"],"voice":["cattle_163727"],"anchor":"meadow","supply":{"butter":0.15,"cheese":0.15}},
 {"id":"haymaking","window":[160,200],"show":["mown_meadow","haystacks","carts"],"voice":["horses_538438"],"anchor":"meadow","supply":{}},
 {"id":"harvest","window":[213,245],"show":["stubble","sheaves","gleaners"],"voice":["horses_538438","chickens_197103"],"anchor":"field","supply":{"rye":0.25,"wheat":0.25,"barley":0.2,"oats":0.2}},
 {"id":"pannage","window":[272,315],"show":["pigs_in_wood"],"voice":["pigs_692838"],"anchor":"wood","supply":{}},
 {"id":"martinmas","window":[305,325],"show":["slaughter_yard","hides_drying","smoke"],"voice":["cattle_513565","pigs_158746","dogs_832435"],"anchor":"yard","supply":{"salt":-0.3,"salt_pork":0.3,"cattle_hides":0.25,"feathers":0.3,"cattle":0.1}},
 {"id":"eel_traps","window":[274,335],"show":["traps_set","smoke_racks"],"voice":[],"anchor":"weir","supply":{"eels":0.35}},
 {"id":"salt_boiling","window":[160,260],"show":["peat_stacks","salt_fires"],"voice":[],"anchor":"shore","supply":{"sea_salt":0.1}}
]}
```

**`wildlife_events.json`** - the events the player witnesses (2.10).

### 2.2 Abundance

For species *s* in region *r* on day *d* of year *y*, hour *h*, weather *w*, habitat cell *c*:

```
A = N(s, r) · P_s(d') · T_s(h) · W_s(w) · Y_s(y) · K_s(y) · D_s(c)

d' = d + J(y) + jitterDays · (2·hash(seed, y, s) - 1) + warmShiftDays · M(y)
```

- `J(y)` is the Julian-Gregorian offset of the century (4 for 793-899, 5 for 900-999, 6 for 1000-1066; 1.1): the curves are written on Gregorian day numbers, and the game's day is Julian, so the slide puts the modern "1 October" on the right Julian day.
- `N(s, r)` = `range[r]` × `perKm2` (2.1).
- `P_s` the presence curve at the slid day.
- `T_s` the hours multiplier, `W_s` the weather multiplier, all terms clamped and the wind a ramp, not a step, so a flock does not flip on and off as `LevelWeather` lerps `Wind` through the threshold: `W = (1 - 0.85·smoothstep(windMax - 3, windMax + 1, wind)) · lerp(1, rain, Rain) · lerp(1, fog, clamp01(FogDensity / 0.004)) · lerp(1, storm, Storm) · lerp(1, frost, Frost)` (`LevelWeather`'s fog target is 0.0045, above the 0.004 full-fog reference, hence the clamp).
- `hash(seed, a, b)` is `Dice.Hash(seed ^ a, b)` with `Dice.Hash(int, int)` the integer mix `LevelWeather.Hash` uses (copied into Core as `Dice.cs`; `LevelWeather` keeps its private copy until the user says to refactor) and strings reduced first by `Dice.Id(string)` = FNV-1a 32-bit (the same construction `SimRandom.For` uses with 64-bit FNV). So `hash(seed, y, "pinkfoot")` = `Dice.Hash(seed ^ y, Dice.Id("pinkfoot"))`. No `string.GetHashCode` anywhere: it is not stable across Mono, IL2CPP and platforms, and the saves and the later peer-to-peer world must agree bit for bit. `seed` is the save's world seed, as economy_and_trade.md's `SimRandom.For(seed, name)` uses it.
- `Y_s` the trend, `K_s` the hard-winter state (2.7), `M(y)` the warm index: 0 before 940, rising to 1 by 980, 1 to 1066 (from the MWP dates; the exact ramp is a guess).
- `D_s(c)` the habitat weight of the cell under the flock (the builders already know mud, bar, wet basin, field, wood from the land cover).

Flocks shown on a level = `clamp(round(A · levelAreaKm2 / typicalFlock), 0, budget)`; the Flock's `count` is `typical · (0.5 + hash)`, clamped by the per-level bird budget (4.5). Presence under 0.05 switches a flock off (its GameObject inactive): the swallows are simply not there in January.

### 2.3 The clock of the year, as the model has it

Day-of-year windows for the first-slice species, written as Gregorian day numbers; the model subtracts the century's offset `J(y)` (2.2) to land them on the Julian clock. Transitions are 10-14 days wide unless stated. The shape of every window, for presence, runs and tasks alike, is the same piecewise curve: rising linearly over the first third of the window, 1 across the middle third, falling over the last third (`shape(d)`); the "arriving" and "leaving" columns below are those thirds where they are not stated.

| Species | Absent | Arriving | Present (1.0) | Leaving | Notes |
|---|---|---|---|---|---|
| Pink-footed goose (delta/Frisia) | 130-255 | 255-285 | 285-100 | 100-130 | peak numbers Nov-Feb |
| Brent (Thames, Essex, Wadden) | 120-265 | 265-295 | 295-80 | 80-120 | on the mud only |
| White-fronted goose (delta) | 100-270 | 270-305 | 305-70 | 70-100 | |
| Bewick's swan (delta) | 80-280 | 280-300 | 300-50 | 50-80 | |
| Whooper swan (Ireland, Humber, Jutland) | 100-270 | 270-320 | 320-60 | 60-100 | |
| Knot and the waders (Wadden, Wash) | 140-195 | 195-230 | 230-110 | 110-140 | huge July-Oct and spring |
| Barn swallow | 275-95 | 95-120 | 120-240 | 240-275 | reedbed roosts 230-270 |
| Swift | 220-110 | 110-125 | 125-205 | 205-220 | screaming parties 150-205 at dusk |
| Crane, on passage only | | autumn skeins 255-330 (shape: rising 255-280, peak 280-305, falling 305-330) | | spring skeins 45-120 (peak 70-95) | none on the ground in the delta; `skeinsPerDay` is the peak rate |
| Crane, Fen breeder | 290-60 | 60-80 | 80-270 | 270-290 | dances in March-April |
| White stork (delta, Jutland, Francia, Iberia) | 230-80 | 80-105 | 105-215 | 215-230 | on the roof |
| Grey heron | | | all year | | stands still in frost; heronry noise 50-150 |
| Gulls | | | all year | | follow boats; inland in storms |
| Crows, rooks, ravens | | | all year | | rookeries loud 60-130; roost flights at dusk |
| Mallard and the ducks | | | all year, more 280-80 | | |
| Gannet (Bass Rock, Farnes sea) | 290-20 | 20-60 | 60-275 | 275-290 | diving on the herring shoals |
| Puffin, guillemot (Farnes, cliffs) | 200-95 | 95-110 | 110-190 | 190-200 | |
| Harbour seal | | | all year | | pups 150-200; moult 210-240; haul-out at low water |
| Grey seal (east coast) | | | all year | | pups 275-345; (Wadden 305-20) |
| Porpoise | | | all year, inshore 120-270 | | groups of 1-5; roll, never leap |
| Red deer | | | all year | | roaring 265-310 at dawn and dusk; calves 130-160 |
| Boar | | | all year | | farrow 70-130; in the mast 270-320 |
| Beaver | | | all year | | seen 1 hour either side of sunset; lodges on quiet banks |
| Mosquitoes (delta, Fens, marsh) | | | 120-280 by the water, dusk | | and in the byres all winter |
| Midges (Scotland, Norway) | | | 130-260 dawn and dusk, wind < 3 m/s | | |
| Mayfly | | | 125-165 on gravel rivers | | |

### 2.4 Time of day and weather, per behaviour class

| Class | Dawn | Day | Dusk | Night | Wind stop | Rain | Fog | Frost | Storm |
|---|---|---|---|---|---|---|---|---|---|
| Geese (graze/roost) | 1.0 flight lines | 0.5 grazing | 1.0 flight lines | 0.3 on the roost water, calling | 16 m/s | 0.8 | 0.6 (fly low, call more) | 1.0 | 0.2 |
| Waders (feed on the tide) | by tide: feed on the falling and low water, roost on the high | | | same | 18 | 1.0 | 0.9 | 0.8 | 0.3 |
| Gulls | 0.8 | 1.0 | 0.8 | 0.1 | none (they ride it) | 0.9 | 0.6 | 1.0 | 1.0 inland over the town |
| Swallows and swifts | 0.6 | 1.0 (high in fair, low over water before rain) | 1.0 | 0 | 12 | 0.4 low over the water | 0.3 | 0 | 0 |
| Crows | 1.0 roost out | 0.8 fields | 1.0 roost in | 0 | 15 | 0.7 | 0.8 | 1.0 | 0.3 |
| Herons | 1.0 | 0.6 | 1.0 | 0.4 | stand | 0.8 | 1.0 | 0.5 (stand hunched) | 0.3 |
| Ducks | 0.8 | 0.6 | 1.0 | 0.5 | raft | 1.0 | 1.0 | 1.0 (open water only) | 0.5 |
| Seabirds at sea | 0.8 | 1.0 | 0.8 | 0.1 | ride it | 0.9 | 0.4 | 1.0 | 0.6 |
| Seals (haul-out) | by tide: 1.0 on the dry banks at low water in calm, 0.3 in rain, 0 in a gale | | | 0.6 | 12 | 0.3 | 1.0 | 1.0 | 0 |
| Deer | 1.0 | 0.3 | 1.0 | 0.6 | 1.0 | 0.6 | 1.0 | 1.0 | 0.3 |
| Beaver | 0.5 | 0 | 1.0 | 0.8 | 1.0 | 0.9 | 1.0 | 0.4 (under the ice) | 0.3 |
| Insects | 1.0 | 0.3 | 1.0 | 0.6 | 3 m/s | 0.7 (midges fly in rain) | 1.0 | 0 | 0 |

Hour bands come from `LevelClock.SunAltitude` and whether it is rising (compare with the last frame): dawn = -6..+10 rising, dusk = +10..-6 falling, night below -6 (`LevelClock.Night` > 0.5), day the rest. The tide is `LevelInfo.WaterLevel - baseWaterLevel` where the level has one (LevelWater).

### 2.5 Flock and herd sizes

| Species | Typical flock on screen | Largest | Formation, spacing |
|---|---|---|---|
| Pink-foot / white-front | 40-120 flying, 300-1,500 grazing (as a carpet at distance) | 5,000 (guess; modern 66,000) | skein 1.4 wingspans apart; grazing 2 m apart |
| Brent | 60-300 on the mud | 2,000 | loose lines on the water's edge |
| Swans | 4-12 (families) | 60 | line, 2 wingspans |
| Knot and waders | 500-3,000 | 20,000 | cloud; a wheeling flock turns dark/light as it banks |
| Shelduck moult (Wadden, July-Aug) | 2,000 | 100,000 | rafts far out |
| Swallows | 12-40 | 2,000 at an autumn roost | loose, fast |
| Swifts | 10-20 screaming parties | 200 | loose, high |
| Cranes on passage | 30-120 | 400 | skein, high, calling |
| Storks | 1 pair per roof, 3-6 roofs in a town | 30 on passage | |
| Herons | 1-4 standing | 20 at a heronry | stand |
| Gulls | 10-40 | 300 behind a fishing boat | loose |
| Rooks | 20-80 | 500 at a winter roost | loose |
| Gannets | 20-100 over a shoal | 1,000 | plunge dives |
| Puffins, guillemots | 200-2,000 on a cliff | 30,000 | stand on ledges, rafts below |
| Harbour seals | 10-60 on a bank | 300 | lying, heads up when the ship passes 200 m |
| Grey seals | 5-30; a pup beach 50-200 | 1,000 | |
| Porpoises | 1-5 | 10 | surfacing roll, 3-4 breaths then a dive |
| Cattle | 8-30 | 80 | grazing, 4 m apart; lie down before rain (folk sign, use it) |
| Sheep | 20-80 | 300 | tighter, 2 m |
| Pigs in pannage | 10-25 with a swineherd | 60 | in the wood edge |
| Geese (farm) | 6-20 | 60 | the yard and the pond |
| Red deer | 6-30 hinds; stags 1-3 | 50 | at the wood's edge at dusk |
| Boar | 5-15 | 25 | sounder |
| Wolves | heard: 3-8 | | never seen near a town by day (guess: a rare sight on a winter dusk in the north) |

### 2.6 Behaviours

Each species lists behaviours; the `Flock` runs one at a time by the state machine below. The existing `Flock` already does `loose` flight, `skein`, `follow` (gulls after the ship), `perch`/`stand` with `startle`, and `raft` (ducks). Added:

- `graze`: the flock sits on the ground cells of its habitat and creeps (0.1 m/s), heads down, lifting a few at a time; it flies off together when startled and lands 150-400 m away downwind.
- `flight_line`: at dawn the flock lifts from its roost anchor (water) and flies a straight line to a feeding anchor (meadow), at dusk back; both anchors are placed by the builder (water within 2 km of wet meadow). The player sees the same lines at the same hours every day they are in port: the geese become a clock.
- `roost_water`: at night the flock sits on open water, calling now and then (voice at a low rate).
- `migrant_skein`: a flock spawned on the level's upwind edge at height, crossing on the bearing, calling, gone at the far edge; not persistent.
- `tide_feed`: waders follow the water's edge as it falls, roost in a tight pack on the highest mud or salt marsh at high water.
- `haul_out`: seals lie on the exposed bank cells when the tide is below a threshold; slide in when the ship comes within `startle` (200 m); heads bob in the water around the ship.
- `surface_roll`: porpoises: a line of 3-4 surfacings then nothing for 2-4 minutes, 100-800 m off the ship, never toward it.
- `stork_roof`: a pair on a named roof anchor; clattering (needs a CC0 clip: none yet) at dawn.
- `screaming_party`: swifts round the roofs 1 hour before sunset, June-July.
- `rut_call`: deer heard, not seen, from the wood anchors at dawn and dusk in the window (a CC0 red deer roar is needed; none in the library yet).
- `swarm`: insects near the camera (a particle system, 4.5) with a sound layer; a "midge index" the crew comments on.

### 2.7 Year-to-year variation

- **Hard winter index** `HW(y, r)`: computed in Core, once per year and region, by `YearState.HardWinter(seed, y, region, chronicle)`. It is *not* read from `LevelWeather`'s temperature: `LevelWeather.cs` has no year term (`mean = 9.8 - 7.8·cos(2π(day-20)/365)`, the same every year, about 2.5 °C in January-February), so a threshold on it would never fire. Instead: if `chronicle_793.json` has an entry of `kind: "winter"` whose `fire` date falls in the winter y-1/y and whose `regions` touch the region's `chronicleIds` (2.1), `HW = 1` (the file has six such entries: 799 Ireland, 810-11 and 813-14 Francia and the delta, 815 the Elbe, 821-22 the Rhine frozen for thirty days, 859-60 Francia and Italy; none are hard-coded here, the file is read); otherwise `HW = clamp01((hash(seed, y, "winter") - 0.85) / 0.15)`, which gives `HW > 0.5` in about one year in thirteen and `HW > 0` in one in seven, the rate of the hard-winter famines in 1.7. The same offset is written back so the level agrees with the index: `LevelWeather` gains a public `yearTempOffset` (°C) that `Wildlife` sets to `-6·HW` for days 335-60 (one line in `LevelWeather.Update`: `mean += yearTempOffset`), so a hard-winter year shows ice, snow cover and frost on the level and `Frost`/`Ice` in the context are the same winter the herons are dying in.
- **Crash and recovery** `K_s(y)` (linear, so the test in 4.3 is exact): after a winter with `HW > 0.5`, `K_s ← K_s · (1 - crash_s · HW)` and the loss `L_s = crash_s · HW` is remembered; each following year `K_s ← min(1, K_s + L_s / recoverYears_s)`. Herons `crash 0.6, recover 4`: 0.4 → 0.55 → 0.70 → 0.85 → 1.0 after four mild years. Wrens, kingfishers 0.7/3; waders 0.3/2; geese 0.15/1 (they move south); seals 0 (pups suffer, adults do not).
- **Food crashes**: one rare event per species with a food line (brent/eelgrass 1 in 150 years; a herring component failing 1 in 8 years, the economy document's 15 % bad-herring year, one figure for both documents; mast failure 1 in 4 years, which empties the pannage and raises pork), rolled from `hash(seed, y, species)`, deterministic for a year and seed. The Bohuslän decades-long herring periods are real but post-1066 and no source gives a cycle length, so the 60-year sine of the first draft is dropped until one is found.
- **Mast years**: `mast(y) = hash(seed, y, "mast") < 0.3 ? 1.5 : hash < 0.55 ? 0.3 : 1.0`: a good, a bad or a plain year; it scales pigs in the wood, boar numbers next spring, and the Martinmas pork supply.
- **Spring timing**: the jitter above plus the warm index: in 1050 the swallows come about four days earlier than in 800 (guess at 4-5 days from the MWP).
- **The Chronicle wins**: a Chronicle entry (famine 793, flood 838, the winters above) sets the year's HW, flood and food values directly; the dice only fill the gaps. The murrain the economy document rolls at 5 % a year is replaced by `HW` (cattle losses follow the hard winter; see 2.9).
- **What the player will actually meet.** A game year is 146 real hours at 1x and 36.5 at 4x, so a single-player life sees a handful of years and the per-year dice (a mast year 1 in 4, a herring failure 1 in 8, a hard winter 1 in 13) are met once or never. They are the heirs' layer and the multiplayer layer (3.2), and the year's state is said once, at New Year, by the crew, so it is noticed when it does come. The within-year variation (the weather-driven nights and evenings of 2.10) is the replay lever the player meets every voyage.

### 2.8 Runs

A run is a 0..1 intensity on an ocean level or river place that drives three things: the animals on the water (gulls and gannets diving, seals, porpoises, splashing shoals as a water decal), the boats out (AI fishing boats in `ai_fleets`), and the market.

```
R_herring(component, d, y) = shape(window, d) · stock(component, y)
stock(component, y) = hash(seed, y, component) < 0.125 ? 0.3 : 1      (a failed year 1 in 8; 2.7)
```

`stock` is the single figure the economy's herring shock reads (2.9): there is one dice roll for "the herring failed off Yarmouth this year", in `YearState`, and both the sea and the market take it from there.

- Herring at sea: visible as gannets and gulls over a patch, a dark slick, boats; at night (the shoals rise) the fishing boats' lanterns. Downs component off Kent and Suffolk Nov-Jan; Dogger Aug-Oct; Buchan Aug-Sep; Øresund Sept-Oct; Norwegian spring Feb-Mar.
- Silver eel: `R_eel = window(274..335) · night · (1 - moonLit) · (0.4 + 0.6·Rain) · (1 + flood)` where `moonLit` is `Sky.MoonLit` × the Moon above the horizon: eels run on dark, wet, rising nights (the German page's "stormy, rainy, rising water"). The traps are full the morning after such a night.
- Glass eel: `window(45..140)` at the river mouths on spring tides at night.
- Salmon: `window(90..260)` on the Rhine and the northern rivers; the weirs busy; `window(300..350)` the spawning where the gravel is (not in our levels, told by the crew).
- Sturgeon: `window(130..180)` on the Rhine and the Elbe: a 3 m fish in a weir is an event (2.10).
- Shad: `window(100..170)`.
- Skrei: `window(10..110)` at Lofoten; the racks full `window(40..150)`.
- Tuna: `window(50..200)` at the Strait; the trap towns busy.
- Right whale: `window(275..80)` on the Basque coast, peak January.

### 2.9 Market hooks

`goods_793.json` already has herring, stockfish, dried_fish, eels, salmon, whale_meat, cattle, sheep, pigs, horses, cheese, butter, salt_pork, cattle_hides, leather, squirrel, marten, sable, beaver, castoreum, fox, otter, ermine, wolf_skins, bear_skins, reindeer_hides, seal_skins, walrus_rope, walrus_ivory, narwhal, antler, combs, horn, whalebone, honey, sea_salt. Feathers and down are missing and should be added (`feathers`, `eiderdown`) with a port list (Frisia, the Fens, the far north).

**One owner of the seasons.** economy_and_trade.md's price is `p_base × season × scarcity × shock × silver`, and its `Seasons` class was to carry its own cosine `make_season_g` curves (herring in the autumn runs, furs best in winter, eider down in June, salt made in summer, stockfish from May) and a `shocks_793.json` with a 15 % bad-herring year and a 5 % murrain. This document does not add a second multiplier on top: there is no `Market.cs` here. Instead `species.json`, `farm_year.json` and `wildlife_events.json` are the single source of the economy's `make_season` curves, through one Core function:

```
SeasonalSupply(goodId, region, day, year) = 1 + Σ_i supply_i · shape_i(day) · stock_i(year)
```

summed over the farm tasks (`FarmYear.Tasks(region, day)`), the runs (`Runs.*`) and the species windows (`goods` on a species in its presence window) that name the good; `shape_i` is the window's 0..1 curve (2.3), `stock_i` the year state (herring `stock`, `mast`, `K_s`), and `supply_i` the amplitude from the tables below. economy_and_trade.md's `Seasons` calls `SeasonalSupply` for `make_season_g` instead of blending its own cosines, and its `shocks_793.json` takes the herring failure from `YearState.Stock(component, y)` (2.8; one figure, 1 in 8 years) and the murrain from `HW` (2.7) rather than rolling them a second time. The table below is therefore the specification of the amplitudes of those curves, written as the price direction the player feels (a supply of +0.3 reads here as about -30 % on the price at the economy's unit elasticity; the economy document owns the elasticity per category):

| Cause | Where | Effect |
|---|---|---|
| Herring run on (any component) | ports within 100 km | herring -30%, salt +20%, sea_salt +20%, boats hire out at +50% |
| Herring failure year (`stock` 0.3, 1 in 8) | same | herring +60%, stockfish +20% |
| Silver eel run after dark wet nights | river places | eels -35% for two weeks, then +10% |
| Skrei season | far north | stockfish -25% at Vågan, Feb-May; by Kaupang in June (lag 60 days), the delta in August |
| Martinmas | every farmed region | salt_pork -30%, cattle_hides -25%, feathers -30%, salt +30%, tallow (if added) -30%, from 5 to 25 November |
| Pannage good mast year | wooded regions | pigs -15%, salt_pork -20% at Martinmas |
| Mast failure | same | pigs +25%, salt_pork +30% |
| Hard winter (`HW` > 0.5; this is the economy's murrain) | the regions the winter touched | cattle +20% next spring (losses), honey +20%, hides -15% (dead stock flayed), herons/wrens gone from the soundscape |
| Fur season (pelts prime Nov-Mar) | baltic, oslofjord, far_north | squirrel, marten, beaver, fox -20% Dec-Mar, +20% Jun-Sep |
| Whale stranded | the port nearest | whale_meat -60%, whalebone -40% for a month; the lord's officer takes his share (see 6) |
| Seal pupping (grey) | Farnes, Wadden, Baltic | seal_skins -20% Dec-Feb |
| Eiderdown | far north, Farnes | eiderdown appears June-July only |
| Cranes/geese in (fowling) | Fens, delta | feathers -15%, a "fowl" fresh good (not yet in the table) Oct-Feb |
| Beaver trend | delta, England | beaver and castoreum base price +0.3%/year from 793 (×2.2 by 1066) |

### 2.10 Wildlife events

Each event has a region set, a date window, a trigger, a probability per day in the window, a cool-down, what the player sees and hears, the crew line (placeholder text; the user writes the real words), and the market hook. The dice are `Dice.Hash(seed ^ (year·366 + day), Dice.Id(eventId))`, so a saved game replays the same.

**When the triggers are tested.** Most triggers are hour-and-weather conditions (night, dusk, moonlight, rain, low water, a tailwind), so `WildlifeEvents.Due` is evaluated **every game hour**, in the same tick that recomputes presence (the cost is nil: a few dozen comparisons). Each event has a `cooldown` of `day` (fires at most once a day, so `eel_night` fires once a night, not once an hour), `season` (once in its window: `geese_arrive`, `first_swallow`, `stork_back`) or `year`. The date-only events (`martinmas`, `harvest`, `first_swallow`, `cattle_out`) are tested on the first hour of the day, which is the same thing as a daily tick. The cool-down state (last fire day per event per region) is a small JSON object in the save.

| Id | Regions | Window | Trigger | What you witness | Hook |
|---|---|---|---|---|---|
| first_swallow | all but far_north | 95-120 | first day P_swallow > 0.1 | one swallow over the harbour; the crew say the sailing season is open | morale +; nothing on prices |
| swifts_scream | towns | 150-205 | dusk, fair, wind < 8 | screaming parties round the roofs | |
| geese_arrive | delta, Frisia, East Anglia, Humber, Jutland, Thames | 255-300 | first dusk with skeins | skeins all evening, calling; the next dawn the flight lines start | fowling opens: feathers -15% |
| geese_leave | same | 95-130 | last skeins north | the meadows go quiet | |
| cranes_over | delta, Jutland, Channel, Biscay, Iberia, Baltic | 255-330 and 45-120 | fair, tailwind within 60°, dawn or dusk | high skeins trumpeting; a sailor's sign of frost coming (autumn) or spring (spring) | |
| stork_back | delta, Frisia, Jutland, Francia, Iberia | 80-105 | daylight | a stork on the named roof; bill-clattering at dawn for a week | the town's luck (reputation flavour) |
| stork_gone | same | 215-230 | | the roof empty | |
| waders_wheel | Wadden, Wash, Humber, Thames mud | 195-110 | falling tide, fair | a cloud of knot turning dark and light | |
| shelduck_moult | Wadden | 195-240 | | rafts of thousands far out, flightless | |
| herring_on | by component | see 2.8 | R > 0.5 | gannets and gulls diving on a slick; boats out; lanterns at night | herring -30%, salt +20% |
| herring_fail | same | | stock < 0.5 | empty sea, idle boats, grumbling | herring +60% |
| eel_night | river places, Fens, delta | 274-335 | night, moonLit < 0.3, Rain > 0.3 or flood > 0 | lanterns at the weirs; the traps full at dawn; smoke racks | eels -35% |
| elvers_up | river mouths | 45-140 | spring tide at night | a black ribbon along the bank's edge | |
| salmon_run | Rhine, Irish, northern rivers | 90-260 | | fish leaping at the weir; nets | salmon -25% |
| sturgeon_caught | Rhine, Elbe | 130-180 | 1 in 20 days | a 3 m fish carried through the town; the lord's man comes | a text adventure: who owns it |
| mayfly_hatch | gravel rivers | 125-165 | dusk, warm, calm | the air thick over the water; swallows and fish | |
| seals_pupping | Farnes, Wadden, Baltic | by region | low water | white pups on the sand; crying | seal_skins -20% |
| seal_bank | Wadden, Thames, Humber | all year | low water, fair | 60 seals on the bank, heads up as you pass | |
| porpoises | all seas | all year, more 120-270 | calm | a line of rolls off the bow; the crew call it luck or a storm sign (region's lore) | |
| whale_sighted | ocean levels, far north | all year | 1 in 30 days; far north 1 in 5 | a blow on the horizon; a back; in the far north a whale 50 ells long | |
| whale_stranded | Frisia, Jutland, delta, Thames, East Anglia | 330-80 (guess) | 1 in 400 days per region | a whale on the sand; the whole town out with knives and carts; crows and gulls; the stink after a week | whale_meat -60%, whalebone -40%; the lord's share; a text adventure |
| gannets_cliff | Bass Rock, far north | 60-275 | sailing past | the white rock, the plunge-diving | gugas for sale 200-240 |
| cliff_eggs | Farnes, cliffs, Heligoland | 125-170 | | men on ropes; eggs in the market | eggs cheap |
| lummensprung | Heligoland, cliffs | 160-180 | dusk | chicks dropping to the sea, the din | |
| eider_down | far north, Farnes | 150-200 | | women at the nests with baskets | eiderdown appears |
| deer_rut | wooded regions | 265-310 | dawn, dusk | roaring from the wood; the lord's hunt rides out | antler -10% after; venison (not traded) |
| boar_in_mast | wooded regions | 270-320 | mast year | sounders at the wood edge with the pigs; the swineherd's horn | |
| wolves_heard | mainland, Britain, Ireland | 300-60 | night, snow or frost, outside the town | howling from the far bank; dogs answer; the crew will not row at night | a night watch; sheep +5% in the port |
| wolf_hunt | Francia, England, Jutland | 335-60 (the winter hunts; the May cub hunt of *Capitulare de villis* c. 69 is a second, quiet window 121-151: men with dogs and spades going out to the woods) | 1 in 10 days | the hunt out; pelts handed in at the hall, then on a stall | wolf_skins -20% |
| bear_skin | Oslofjord, Baltic, far north | 300-60 | | a bear skin on a stall; a hunter's story (a text) | |
| beaver_dusk | quiet banks of the delta, Thames, Baltic rivers | all year | dusk, calm, trend > 0.3 | a V-wake and a tail-slap; the lodge; cut stumps | beaver/castoreum offered |
| elk_last | Rhine delta: the peat and marsh edges east of the Vecht and the wet side of the Gelderse Vallei (1.7) | 793-850 | 1 in 2,000 days | an elk at the marsh edge at dusk; the old men say they were common; probably gone soon after | a tale |
| crane_dance | the Fens | 60-110 | dawn | cranes dancing on the fen; fowlers watching | |
| bustard_lek | East Anglia heaths, Salisbury | 60-120 | dawn | the males puffed white | |
| stork_omen / heron_feast | Francia, England | | | a lord's feast with herons: a commission to bring 100 (reputation) | |
| hard_winter | all | 335-60 | HW > 0.5 | the sea frozen a mile out, the river crossed on foot, dead birds on the ice, herons gone for years | 2.9 |
| midges | Scotland, Norway, far north | 130-260 | dawn/dusk, wind < 3, humid | the crew slap and curse; morale -; "we sail at noon" | |
| ague | delta, Fens, Thames marsh, Camargue | 120-280 | a crewman from a dry country, nights ashore | the marsh fever: shivering fits; a crewman out for a fortnight (see 6) | crew |
| cattle_out | farmed regions | 95-110 | | the herds let out, running; the first milk | butter, cheese -15% |
| haymaking | | 160-200 | fair days | mowers in lines, carts, stacks; the fires on St John's Eve | |
| harvest | | 213-245 | | reapers, sheaves, gleaners, stubble geese | grain -25% |
| martinmas | | 305-325 | | the slaughter yard, hides on frames, smoke, the goose, the fires on the eve | 2.9 |
| pannage_in | | 272-315 | | pigs driven to the wood each morning, back at dusk | |
| salt_fires | Frisia, Zeeland, Camargue | 160-260 | dry | peat stacks, smoke along the shore; the pans white in the south | sea_salt -10% |
| eelgrass_blight | brent coasts | 1 in 150 years | | the mud bare, the brent starving; a strange year | feathers +; a tale |

### 2.11 Omens: signs that are ahead of the weather, and wrong a known fraction of the time

A sign is only a game if it tells the player something they cannot see in the sky. Swallows flying low because `LevelWeather.Rain` is already above zero tell them nothing. `LevelWeather` picks its weather per six-hour block by a pure, deterministic `Auto(year, day, hour)` (`Hash(year·7 + block, day)` against the season's table), so the next block is knowable in advance; `Auto` is made `public static` (it is pure; nothing else changes) and Core gets:

```
Omen(sign, seed, year, day, hour):
  next   = LevelWeather.Auto(year, day, hour + 6)         // the coming block's WeatherKind
  truth  = predicts(sign, next)                           // e.g. cattle_lying: Rain or Storm; swallows_low: Rain; cranes_early: Snow or Fog with frost; porpoises: Storm
  roll   = Dice.Hash(seed ^ (year·366 + day), Dice.Id(sign) + block)
  return roll < reliability_sign ? truth : !truth
```

The sign is given (the behaviour plays: the cattle lie down, the swallows skim the water, the porpoises roll) when `Omen` returns true, and the crew's oldest hand says the line. Reliabilities are in `species.json` as `omen.reliability` (all guesses, to tune): swallows low 0.75, cattle lying 0.6, cranes early 0.7, porpoises playing 0.5, gulls inland 0.7, herons hunched 0.6. So the sign is one block (six game hours, six real minutes at 1x) ahead of the weather and wrong a known fraction of the time: a player who learns which signs to trust gains six minutes to reef, make a creek or stay in port. The multiplayer world sees the same signs because the same hash does.

---

## 3. In the game

### 3.1 What the player sees

**The 3D voyage.** The sea is never empty and never the same: gulls at the stern, a line of porpoises in a calm, gannets folding into a shoal off Flamborough in September, a seal bank at low water on the way out of the Wadden, cranes trumpeting over the mast on a clear October dusk with a north-east wind behind them, a whale's blow on the horizon in the far north. The sky's traffic is a calendar: a skein going south-west is October, swifts over the town are June, no swallows and geese on the meadow is winter. Arriving at a port shows the season at once: mown meadows and stacks, stubble, pigs at the wood's edge with the swineherd, the slaughter yard's smoke and hides, the river's ice with the ducks crammed on the one open pool. The animals react to the ship: herons lift at 45 m, seals slide in at 200 m, ducks paddle off, geese rise as one and settle downwind, gulls follow. Nothing speaks to the camera; it is a photograph of a working coast.

**The 2D chart.** The chart side tabs get a **calendar tab**: the year as a ring with the runs, the arrivals, the farm tasks and the feasts drawn as arcs per region, filled in as the player learns them (a run the player has seen, a thing the crew told them, a line read in a text). An unknown year is blank. This is the knowledge game: the chart is your table, and a merchant who knows that the Downs herring is off Yarmouth in December, the skrei racks are full at Vågan in April, the pelts are prime in February and the feathers cheap after Martinmas plans a year, not a voyage.

**The deck.** The crew comment (text, the user's words later): the lookout calls the skeins and the whales; the oldest hand reads the signs (cattle lying down, swallows low, porpoises playing, the cranes early: "frost within the week"); a crewman from the marsh country knows which nights the eels run; a Norseman knows the skrei. A sign runs six game hours ahead of the weather and is wrong a known fraction of the time (2.11): the swallows skim the water in a dry noon because the next block is rain, three times in four; the fourth time the afternoon stays fair and the old hand shrugs. It is the next block that the sign reads, never the weather already on the deck.

**Text adventures.** The sturgeon (whose fish is it: the lord's, the abbey's, the fisher's; what you offer), the stranded whale (the town's day of knives; the reeve's claim; your hold of blubber and the stink in it; the baleen), the heron commission (a feast wants 100 herons: the fowlers, the price, the season), the fowler's offer (a boatload of geese and their feathers after a hard frost), the fur man at Hedeby (prime winter pelts against summer ones: look at the hair), the bear hunter's tale, the midge-ridden crew that will not row at dawn.

### 3.2 Fun and replay

- **Knowing the year is the skill.** Prices follow the animals, so the calendar is the deepest trade knowledge in the game and it is learnt by sailing and listening, never from a menu.
- **No two evenings are the same; years are for the heirs.** Honestly: at 1 s = 1 game minute a year is 146 real hours at 1x (36.5 at 4x), so one life sees a handful of years and a one-in-eight herring failure or a one-in-thirteen hard winter is met once or never. The replay lever the player actually meets is within the year and driven by the weather: the dark wet October night the eels run and the traps are full at dawn; the clear evening with a north-easter when the cranes cross and not the three grey evenings either side; the seal bank that is there only at low water in a calm; the herring slick the gannets mark on a fair day and the lanterns of the boats on it at night; the swallows low before the squall. The per-year dice (jitter of a week, mast, herring, hard winters, the Chronicle's own years, the warm clock after 950) are the heirs' layer and the multiplayer layer, where a line and a server see dozens of years, and the year's state is said once at New Year by the crew ("a mast year", "the herring failed off Yarmouth", "the Rhine froze for a month") so that it is noticed when it comes.
- **Signs the player learns to read**, six hours ahead of the weather and wrong a known fraction of the time (2.11), which makes them a game and not a HUD.
- **Rare things to tell of.** The last elk, the beached whale, the frozen sea, a bear skin, the Dalmatian pelican if the user wants a legend: one-in-a-lifetime sights that every playthrough gets a different few of.
- **The heirs.** A line that runs to 1066 sees the beavers thin, the Fens drain around the abbeys, the herring towns rise; the chart's calendar tab, inherited, is the family's book.
- **Multiplayer later:** one world clock, so every player sees the same skeins the same evening; a run is a race to the port that has it.

---

## 4. Build plan for Unity

### 4.1 Where things go

Pure C# (no UnityEngine) in `Assets/Kapman/Code/Core/Wildlife/` so the EditMode tests (which reference `Kapman.Core` only) can run headless:

- `Dice.cs` - `Hash(int a, int b)` (the integer mix from `LevelWeather.Hash`, copied; `LevelWeather` keeps its private copy until the user says to refactor) and `Id(string)` (FNV-1a 32-bit). Every die in this document goes through these two.
- `Regions.cs` - loads `regions.json`; `Regions.At(lat, lon, kind)`; `Region.Habitat(id)`; `Region.MatchesChronicle(string[] ids)`.
- `Species.cs` - the record (fields as in 2.1, including `perKm2`, `omen`, `places` for stock) and `SpeciesTable.Load(json)`.
- `Presence.cs` - `Presence.At(species, seed, dayOfYear, year, warmIndex)` (curve + century offset + jitter + warm shift), `Presence.Hours(species, sunAltitude, rising)`, `Presence.Weather(species, wind, rain, fog, frost, storm)`, `Presence.Shape(window, day)`.
- `YearState.cs` - `HardWinter(seed, year, region, chronicleWinters)`, `Crash/Recover` (linear), `Mast(seed, year)`, `Stock(seed, component, year)`; it loads the `kind: "winter"` entries from `chronicle_793.json` once.
- `Runs.cs` - `Eel(day, night, moonLit, rain, flood)`, `Herring(component, day, year)`, etc. per 2.8.
- `FarmYear.cs` - loads `farm_year.json`; `Tasks(region, day)` with strengths and voices.
- `Supply.cs` - `SeasonalSupply(goodId, region, day, year)` (2.9), the one function the economy's `Seasons` calls; no price code here.
- `WildlifeEvents.cs` - loads `wildlife_events.json`; `Due(region, year, day, hour, context)` returns the events that fire this hour, given a `WildlifeContext` struct (hour band, wind, rain, fog, frost, storm, flood, tide, moonLit, HW, mast, runs); it keeps the cool-downs (last fire day per event and region) in a small state object that saves as JSON (`Json.Write`).
- `Omens.cs` - `Omen(sign, seed, year, day, hour, Func<int,int,float,WeatherKind> auto)` (2.11); the `auto` delegate is `LevelWeather.Auto` at runtime and a stub in the tests, so Core does not reference the Runtime assembly.

Runtime (`Assets/Kapman/Code/Runtime/Levels/Wildlife/`, assembly `Kapman.Runtime`):

- `Wildlife.cs` (MonoBehaviour, one per level, `[DefaultExecutionOrder(-50)]`, `[ExecuteAlways]` like the others, but it only pushes presence into flocks when `Application.isPlaying`, because `Flock.Build()` runs from `Start` and its `birds` array is null in the editor): finds `LevelInfo` (`region` field, else latitude/longitude and kind → region), `LevelClock` (date, hour, Sun altitude, `Sky.MoonLit`), `LevelWeather` (wind, rain, fog, frost, storm, flood), `LevelInfo.WaterLevel` (tide); **once a game hour** it recomputes the context, the presence of every species on the level, sets each `Flock`'s `presence`/behaviour, asks `WildlifeEvents.Due` for this hour and raises `public static event Action<WildlifeEvent> Fired` (the DevHud and the panel listen; later the crew text). At New Year it sets `LevelWeather.yearTempOffset` from `HW` (2.7). Public statics `Wildlife.Current`, `Wildlife.Context`, `Wildlife.Present` (species id → presence, which the audio pass reads for its WILDLIFE bus; there is no `VoicesNow` and no edit to `LevelSoundscape`, which audio_design.md replaces).
- `Flock.cs` - **kept**; added fields: `string speciesId`, `float presence` (0..1 from `Wildlife`: `Build()` still allocates its arrays from `count`, so `presence` hides the birds beyond `round(count·presence)` by moving them to the rest point and scaling them to zero, and disables the GameObject under 0.05), `Behaviour behaviour` (enum: Loose, Skein, Graze, FlightLine, RoostWater, TideFeed, HaulOut, Raft, Stand, Perch, Migrant), `Vector3[] anchors` (roost, feed; from the builder). The geese's fixed clock hours (`hour > 4.5 && hour < 7.5`, `19.5-22`), which are dark at a December dawn at 52 °N, are replaced by `LevelClock.SunAltitude` bands (dawn = -6..+10 rising, dusk = +10..-6 falling), as are the crows'. Later: a `Render` path with `Graphics.RenderMeshInstanced` for big flocks (`count > 32`): at most 1,023 instances per call, so 6,000 birds is six to eight calls, and the per-frame `Matrix4x4[]` must be built in a Burst job to stay inside the budget (`com.unity.burst` is **not** in `Packages/manifest.json` yet, only `com.unity.mathematics`; adding it is part of that step, with the user's say). The per-bird GameObjects stay for small flocks so nothing existing breaks. `Kind` stays and maps to a default species for scenes built before this.
- `MigrantSkeins.cs` - spawns and retires passing flocks per 2.6 `migrant_skein` with a Poisson draw per game hour: `λ = skeinsPerDay/24 · shape(d) · hourBand · tailwind · fair`, where `shape(d)` is 2.3's piecewise curve (rising over the first third of the window, 1 across the middle third, falling over the last) and `skeinsPerDay` the rate at the peak; `hourBand` is 1 at dawn and dusk, 0.3 by day, 0.15 by night (cranes and geese do cross at night, calling); `tailwind` = clamp01(cos(angle between wind and bearing)) × (0.5 + 0.5·clamp01(wind/8)); `fair` = 1 - Rain - Storm, clamped.
- `SeaLife.cs` - porpoises, seal banks, whale blows, gannet dives on ocean levels: small meshes and water decals through `WaterDecalKit` (the strike and splash decals exist), driven by the runs.
- `Insects.cs` - one particle system that follows the camera below 20 m over marsh or alder cells in the season, intensity from 2.4; a loop in the soundscape (a CC0 mosquito/midge recording is needed; none in the library).
- `FarmDressing.cs` - per task window, toggles the builder-placed sets (`haystacks`, `stubble` material swap on field blocks, `hides_frames`, `peat_stacks`, `traps_set`) and tells `TownLife` to add the slaughter smoke (`TownLife.smokes` already lean with the wind; a `extraSmoke` multiplier is one line). The farm year is heard before it is seen: each task's `voice` and the `stock` species' `places` play at the builder's farm, meadow, wood and yard anchors through the audio pass's WILDLIFE bus (it reads `species.json` and `farm_year.json`; this document adds no sound code). Herd meshes wait for a pack: the component already takes a `Herd` list so that nothing else changes when they arrive.
- The soundscape: nothing here. audio_design.md replaces `LevelSoundscape.cs` with `LevelSoundscape2` and its WILDLIFE voices bus takes its calendar from `species.json` (presence, hours, weather, voice, voiceRate, places) and `farm_year.json` (voice per task), so there is one calendar and this document does not edit the old component.
- `DevHud.cs` - two lines: the region and the events fired today, and the five most present species with their presence (a debugging aid, off in release).

Editor (`Assets/Kapman/Editor/`):

- `DorestadBirds.cs` **kept**; it gains `speciesId` and `anchors` on each flock it makes (geese: roost on the nearest basin pool, feed on the wet meadow; herons stay as they are), and it writes `levels/dorestad/habitats.json` (the cells by cover: wet_meadow, reed, mud, bar, field, wood, water, roofs for storks) so `Wildlife` can place extra flocks at runtime without the builder.
- `OceanBuilder.cs` gains the same for the sea: sandbanks (seal banks), the mud (brent, waders), cliffs (seabirds), the shoal patches (herring).
- `WildlifeBake.py` (under `Bake/`) copies the JSON into `Assets/Kapman/Data/wildlife/` and validates it: every species has `id`, `range`, `presence`, `perKm2`, `flock` (or `places` for stock); every range id is a region id; every good is in `goods_793.json`; every voice is a file in `sounds_cc0`; and every voice's species is allowed by the credits: `CREDITS_sounds.md` gains a `species` column (comma-separated ids, with `#tag` for a cut) and the validator refuses a clip listed under a species the credits do not name for it, which is what stops a Canada-goose clip playing for a pink-foot.

### 4.2 Data files

`G:\kapman_unity\data\wildlife\regions.json`, `species.json`, `farm_year.json`, `wildlife_events.json`, and `CREDITS_wildlife.md` (the sources above, so the credits line is kept per the shippable-data rule). Per level: `levels/<id>/habitats.json` from the builder.

### 4.3 Tests (EditMode, headless, `Kapman.Tests.EditMode`, pure C#)

`WildlifeTests.cs`:

1. `SpeciesJsonParses` - every species has id, range, presence, perKm2, flock or places; every range key is a region; every voice file exists under `G:\kapman_unity\data\sounds_cc0` and its species is named in the credits (Inconclusive if the folder is missing, as the tide test does).
2. `DorestadIsTheRhineDelta`, `LundenwicIsTheThames`, `HedebyIsJutland`, `KaupangIsOslofjord` - `Regions.At` on the ports' coordinates with kind `land`; `TheSouthernNorthSeaIsNotTheDelta` - `Regions.At(52.3, 3.6, sea)` is `southern_bight` while `Regions.At(52.2, 4.3, land)` (Katwijk) is `rhine_delta`; a `LevelInfo.region` override wins over both.
3. `PinkfootAbsentInJulyPresentInDecember` - presence at Dorestad: day 182 < 0.05, day 335 > 0.8 (with jitter at its extremes).
4. `SwiftsComeInMayAndGoInAugust` - day 100 = 0, day 160 > 0.9, day 240 = 0.
5. `JitterIsBoundedAndDeterministic` - over 793..1066 the first day with presence > 0.5 moves by at most `jitterDays + |warmShiftDays| + 2` (the extra 2 for the century offset step) and the same year gives the same day twice.
6. `WarmYearsAreEarlier` - the swallow's arrival day in 1050 is less than in 800.
7. `CranesCrossInAutumnNotInJuly` - with `hourBand`, `tailwind` and `fair` all 1, the integral of `skeinsPerDay · shape(d)` over 255-330 is 6 × (75 - 2·25/2) = 300 ± 10 % (the trapezoid: 25 days rising, 25 full, 25 falling), and 0 over 160-240.
8. `EelsRunOnDarkWetAutumnNights` - `Runs.Eel` on day 300, night, moonLit 0.05, rain 0.6 > 0.7; the same at full moon < 0.3; day 150 = 0.
9. `HardWinterCrashesHeronsAndTheyRecover` - after HW = 1 the heron K = 0.4; with the linear rule of 2.7, after two mild years K = 0.7 (± 0.01) and after four > 0.9 (it is 1.0).
10. `HardWintersAreRareAndChronicleWins` - over 793..1066, between 15 and 35 years have `HW > 0.5` for `rhine_delta` from the dice (the expected count is 274 × 0.075 ≈ 21), and 822 (the Rhine frozen thirty days, `ARF 821`) has `HW = 1` from the chronicle entry, read from `chronicle_793.json`, not from a literal in the test.
11. `FarmYearDrivesTheEconomySeasons` - `SeasonalSupply("salt", "rhine_delta", day 315, 800)` < 0.8 and `SeasonalSupply("salt_pork", ...)` > 1.2 (the Martinmas task); day 200 gives 1.0 for both; `SeasonalSupply("herring", "east_anglia", day 340, y)` is > 1.2 in a year where `Stock("downs", y)` is 1 and < 1 where it is 0.3.
12. `EventsFireOnceASeason` - 24 hourly contexts a day over a season with the trigger true fire `geese_arrive` exactly once; a saved and reloaded state does not fire it again. `EelNightFiresOnceANight` - over three October days of hourly contexts with rain 0.6 and moonLit 0.1, `eel_night` fires exactly three times, only in hours with `Night > 0.5`, and never on a fourth day with rain 0.
13. `SameSeedSameYear` - two runs over a year of the whole model produce identical event lists; and `Dice.Hash(793, Dice.Id("pinkfoot"))` equals a fixed literal written into the test when it first passes, so a platform or runtime change in the hash is caught.
14. `BeaverThinsTo1066` - trend at 1066 in `rhine_delta` is about half of 793.
15. `OmensBeatChance` - with a stub `auto` that returns Rain on 30 % of blocks by its own hash, over 1,000 days the swallows-low sign predicts the next block's rain with an accuracy above 0.6 and below 0.9 (reliability 0.75).

Run: `Unity.exe -batchmode -nographics -projectPath G:\kapman_unity\Kapman2 -runTests -testPlatform EditMode -testResults <xml> -logFile <log>` (PROGRESS.md).

### 4.4 Hooks into the existing code, in one list

| Existing | Reads from it | Writes to it |
|---|---|---|
| `LevelInfo` | latitude/longitude and kind (region), WaterLevel, baseWaterLevel (tide), realMetresPerUnit (ocean levels scale flock distances) | an optional `region` string (empty = by lat/lon and kind) |
| `LevelClock` | Date, DayOfYear, Year, HourOfDay, SunAltitude, Night, `Sky.MoonLit`, TimeFactor | nothing |
| `LevelWeather` | Wind, WindVector, Rain, FogDensity, Frost, Storm, Snowfall, Ice, flood; `Auto(year, day, hour)` made `public static` for the omens | a public `yearTempOffset` float, added to `mean` in `Update` (one line), set by `Wildlife` from `HW` so the level's ice and snow agree with the year |
| `chronicle_793.json` | the `kind: "winter"` entries (fire date, regions) | nothing |
| economy_and_trade.md's `Seasons` and `shocks_793.json` | | `Seasons` calls `SeasonalSupply` for `make_season_g`; the herring and murrain shocks read `YearState.Stock` and `HW` instead of their own dice |
| `LevelWater` | the tide/flood through WaterLevel | nothing |
| `Feasts` / `TownLife` | FeastToday; Martinmas fires already | `TownLife` gains an `extraSmoke` float (slaughter days) |
| `Flock` | its fields | `speciesId`, `presence`, `behaviour`, `anchors`, Sun-altitude bands for the geese and crows instead of clock hours; later the instanced render for big flocks |
| `LevelSoundscape` / audio_design.md's `LevelSoundscape2` | | nothing here: the audio pass reads `species.json` and `farm_year.json` for its WILDLIFE bus (one calendar) |
| `ShipMotion.Player` | position and speed (startle, follow) | nothing |
| `WaterDecalKit` | | splashes for dives, blows and rolls |
| `DevHud` | | two debug lines |
| `DorestadBirds`, `OceanBuilder` | | anchors, speciesId, `habitats.json` |
| `goods_793.json` (first build, to be carried over) | ids | add `feathers`, `eiderdown`, `fowl`, `tallow` |

### 4.5 Performance budget (RTX 4070, 1440p, 60 fps: 16.6 ms a frame; wildlife gets 1.2 ms CPU and 1.0 ms GPU)

| Thing | Cap per level | Cost | How |
|---|---|---|---|
| Flying birds, small flocks (GameObjects) | 400 transforms | 0.3 ms CPU | as now; `SetPositionAndRotation` only |
| Flying birds, big flocks (instanced; a later step) | 6,000 instances in 6 to 8 calls of up to 1,023 | 0.2 ms CPU only with the matrices built in a Burst job (`com.unity.burst` to be added to the manifest), 0.3 ms GPU | `RenderMeshInstanced`; 4 vertices each |
| Grazing carpets (geese at distance, waders) | 10,000 | 0.1 ms GPU | one instanced quad batch, no per-frame CPU beyond the flock centre |
| Migrant skeins | 4 at once, 120 birds each | in the instanced budget | |
| Sea life | 40 meshes, 20 decals | 0.1 ms | |
| Insects | 1 particle system, 2,000 particles, within 20 m of the camera | 0.1 ms GPU | off above 20 m |
| Herds (when a pack comes) | 200 skinned at 3 LODs, impostors beyond 150 m | 0.4 ms GPU | not now |
| Model update | once per game hour (every 60 real seconds at 1x; 15 s at 4x) | < 0.05 ms amortised | |
| Events | once per game hour, with the model update | nil (a few dozen comparisons) | |
| Voices | ≤ 8 one-shots at once, 6 loops | audio thread | |
| Memory | species and events JSON < 200 KB; meshes shared | | |

---

## 5. First slice (one night)

**Night 1** (the Core model, its tests, and the existing flocks obeying the calendar). Build in this order and stop when the done-when passes:

1. `regions.json` (14 land polygons, the `southern_bight` sea polygon, `chronicleIds`), `species.json` with the ten birds the Dorestad scene already has kinds for or can show with the existing meshes: the geese group (pink-foot, white-front, greylag as one `geese` flock with three voices), brent, swallow, swift, crane (passage only), white stork, grey heron, gulls, crows/rooks, mallard; plus the `stock` entries (cattle, sheep, pigs, farm geese, hens, dogs) as data only. `farm_year.json` for `rhine_delta` with the eight tasks and their voices. `wildlife_events.json` with three events: first_swallow, geese_arrive, martinmas.
2. `Dice`, `Regions`, `Species`, `Presence`, `YearState`, `FarmYear`, `Supply`, `WildlifeEvents` in Core with tests 1-6 and 9-14 (not 7, 8 and 15, which need `MigrantSkeins`, `Runs` and `Omens`); run them headless and get green.
3. `Wildlife.cs` on the Dorestad scene (added by `DorestadBuilder`), doing only this: set each `Flock`'s new `presence` (hide beyond `count·presence`, disable under 0.05) by species, replace the geese's and crows' clock-hour test with `LevelClock.SunAltitude` bands, and raise the events into `DevHud` (two lines). No `MigrantSkeins`, no ocean scene, no anchors, no `habitats.json`, no herds, no instancing, no insects, no sound.
4. Two shots added to `Batch.BuildAndShootDorestadPlain` (`Shots.cs` already takes `day` and `hour` per shot): day 200 at 07:00 and day 330 at 07:00.

Done when: the EditMode run is green (the headless command in 4.3), and the two shots show swallows over the harbour in the first and the geese up at dawn in the second with no swallows in it; the DevHud in the second shot lists `rhine_delta`, the geese at presence > 0.8 and the swallows absent. This can be checked without pressing anything in Play.

**Night 2**: `Runs` and test 8; `MigrantSkeins` and test 7 (cranes and geese over the basins in October-November and March); `Omens` and test 15 with `LevelWeather.Auto` made public; `eel_night` and `cranes_over` with the hourly trigger test; `Wildlife.cs` on the North Sea scene with the `southern_bight` region and the harbour seal as data (no mesh); `DorestadBirds` anchors and `habitats.json`. The soundscape step is not here: it belongs to the audio pass, which reads `species.json`.

Then the user play-tests (U a month on, twelve times from June) and says what reads wrong before anything else is built (the plan-before-building rule).

---

## 6. Open questions for the user

1. **Packs, for the meshes only.** Livestock, deer, seals and a stork mesh need packs; the sounds of the byre, the fold, the pannage and the slaughter yard are already in the library (CC0) and play from the first slice's data. Which packs, and when? Until then the farm year shows stacks, stubble, smoke and hides, and is heard. (Birds-only stays the rule for meshes until you say.)
2. **Teeth or atmosphere.** The market multipliers in 2.9 (±20-60%) make the calendar a real trade strategy. Do you want them that strong, or wildlife mostly as atmosphere with small price effects (±10%)?
3. **The marsh fever.** The ague as a crew sickness in the delta, the Fens, the Thames marsh and the Camargue in summer (a crewman out for a fortnight, a death now and then): in or out?
4. **Hunting and fowling by the player.** Trade in the products only, or also the activity itself on the map table (set eel traps for the night, buy a fowler's catch after a frost, take gugas off the Bass)?
5. **The stranded whale.** It belongs to the king or the lord in every law of the period. Do we let the player buy from the town's day of knives, bribe the reeve, or only watch?
6. **Legends.** A Dalmatian pelican in the Fens and the last elk in the delta are not provable for 793. Leave them in as once-in-a-life sights, or keep strictly to what is attested?
