# The economy, the trade routes and the chronicle's effects

Design document for Kāpman 2, written 4 October 2026 from the user's brief of that night ("the trade routes, the economy, the effects on the economy from events and NPC interactivity through text based adventure"; "the playability, the fun, the replay ability"), revised the same day after review. It covers what was really traded between 793 and 1066 and where, what it cost where we know, how the trade moved, who moved it, how the chronicle's events bent it, and the model, the player's loop, the Unity build plan, a first slice and the open questions.

What already exists and is kept, not rewritten: the first build's tables (`G:\kapman_unity\Kapman\Assets\Kapman\Game\Data\goods_793.json` with 140 goods, `trade_rules.json`, `ports_793.json` with 50 ports, `chronicle_793.json` with 582 dated entries, `fairs_793.json`, `commissions_793.json`, `knowing_793.json`, `reputation_793.json`, `events_793.json`, `ship_classes_793.json`, `voyage_rules.json`, `writing.json`), the first build's `Market`, `Deal`, `Purse`, `Hold`, `TradeRules`, `GameData`, `Chronicle`, `GameEvents`, `Reputation`, `TraderBrain` and `Knowledge` code (`G:\kapman_unity\Kapman\Assets\Kapman\Code\Game`), the approved AI-fleets plan (`G:\kapman_unity\Kapman\Plans\ai_fleets.md`) and Kapman 2's Core (`Calendar`, `Json`, `SimRandom`) and `Runtime/Levels` (`LevelClock`, `LevelInfo`, `LevelExit`, `Feasts`, `ShipMotion`). Where this document goes beyond them it says so. Line counts and formulas quoted below were read from that code today.

Web search ran out of budget during this pass; what is marked (verified) was read today from the page cited; what is marked (memory, check) is a figure I know from the editions named but could not open today; (guess) is a design number to tune.

---

## 1. What is true

### 1.1 The money

| Coin | Silver | Where it was money | Source |
|---|---|---|---|
| Sceat (Frisian and English) | 0.8–1.3 g; primary series c. 680–720, secondary c. 710–750; struck at Dorestad; current in Francia until Pippin's reform at Ver, 755 | Frisia, England, the Rhine mouth; old by 793 but still in purses | [Sceat](https://en.wikipedia.org/wiki/Sceat) (verified) |
| Denier of Charlemagne, reformed 793/4 | about 1.7 g; 12 to the solidus (shilling), 240 to the pound (about 408 g) | the whole Frankish realm; Frankfurt 794 ordered the new deniers taken "in every place, town and market" | MGH Capit. I no. 28 c. 5; the game's `trade_rules.json`; chronicle entry 794-06-frankfurt-new-denier |
| Offa's broad penny, c. 792/3 | heavier, broader and thinner than the pennies before, following the Carolingian reform | Mercia, Kent, then all England | [Offa of Mercia](https://en.wikipedia.org/wiki/Offa_of_Mercia) (verified) |
| The English pound | 240 pennies; the tower (Saxon) pound of about 350 g, the troy pound 373.24 g | England, for the tributes | [Tower pound](https://en.wikipedia.org/wiki/Tower_pound) (verified) |
| Mancus | a unit of account of 30 silver pence, and a gold weight of 4.25 g (the dinar); first seen in the 770s, at its peak 9th–11th c. | England, for large sums (a month's skilled wages) | [Mancus](https://en.wikipedia.org/wiki/Mancus) (verified) |
| Dirham | about 2.9 g; cut and weighed in the north | Volga, Baltic, Scandinavia; at Mainz Ibrahim ibn Yaqub saw Samarkand dirhams in 965 | chronicle 965; [Volga trade route](https://en.wikipedia.org/wiki/Volga_trade_route) (verified) |
| Hacksilver, by weight | any silver, nicked and tested on folding scales; the mark (about 200 g) and the ounce | Scandinavia, the Baltic, Ireland, the Danelaw | Alfred–Guthrum treaty: a noble's life "8 half-marks of pure gold" (verified) |

The silver stream from the east: dirhams reach the Baltic from at least 786 and stop around 1009 (the Volga route page, verified); the latest at Peterhof on the Gulf of Finland is 804/5 (verified); the last dirhams in northern hoards are of the 1010s, after which German and English pennies take their place (chronicle 1000). Hedeby struck its own coins from about 825 (verified), Sigtuna from 995 and Dublin from 997 (chronicle).

Silver going west to east by force: the Frisians paid 100 pounds in 810; Paris 7,000 pounds in 845; England 10,000 pounds in 991, 16,000 in 994, 24,000 in 1002, 36,000 in 1007, 3,000 from East Kent in 1009, 48,000 in 1012, and 72,000 plus London's 10,500 in 1018 (chronicle entries 810-summer-frisia-raid-810, 845-03-28-paris-sacked, 991-08-maldon, 994-09-08-london-attack, 1002-11-13-st-brice (the 24,000 is in its note), 1007-tribute, 1009-08-thorkell, 1012-04-19-aelfheah, 1018-heregeld; the 1002 figure also [Æthelred the Unready](https://en.wikipedia.org/wiki/%C3%86thelred_the_Unready), verified). What a "pound" weighed decides the tonnage: at the Carolingian 408 g the 1018 tribute is about 29 tonnes; at the English tower pound of about 350 g it is 25 tonnes, at the troy pound 27 tonnes (the [Danegeld](https://en.wikipedia.org/wiki/Danegeld) page gives 26,900 kg plus 3,900 kg from London, verified, and "more than 100 tonnes" over the whole run of payments). The game's silver model (2.4) scales the pounds, so the choice only moves the labels.

### 1.2 The goods and what they cost

The only royal price list of the period is the Synod of Frankfurt, June 794, c. 4: maximum prices for a modius (the new public measure) of oats 1 denier, barley 2, rye 3, wheat 4; and bread: 12 wheat loaves of 2 pounds for a denier, 15 of rye, 20 of barley, 25 of oats (MGH Capit. I no. 28; the denier figures are already in `goods_793.json` under source `frankfurt794`; the loaf counts for rye, barley and oats are memory, check). The first build's goods table turns a sack of about a modius and a half into wheat 6 d, rye 4, oats 2, which match the cap; barley is 3 d by the cap and 4 in the table (rounded up; change the table to 3 if the user wants the cap exact).

Other hard numbers:

| Thing | Figure | Source and standing |
|---|---|---|
| Lex Ribuaria title 40 (36) c. 11, paying a wergild in kind, in solidi: horned ox 2, cow 1 (3 in some manuscripts), stallion 6, mare 3, sword with scabbard 7, sword alone 3, byrnie 12, helmet 6, greaves 6, shield with lance 2, hawk untamed 3, mewed 6, tamed 12 | the text is 7th-century, in gold solidi (then 40 d); reckoning the solidus at Charlemagne's 12 d is the game's choice, giving a byrnie 144 d, a sword 84 d, an ox 24 d, a cow 12 d | MGH LL nat. Germ. III 2 (Beyerle & Buchner 1954); figures from memory, check against the Cologne [Bibliotheca legum](https://www.leges.uni-koeln.de/en/lex/lex-ribuaria/) edition before they enter `goods_793.json` (the Wikisource text was 404 today) |
| Capitulary of 808: cloaks and furs capped (a best cloak with sable 30 solidi, a middling one 20, with marten 10, with otter 10) (?) | 360 d for the best fur-lined cloak | chronicle 808 "Capitulary caps cloak and fur prices"; the figures are memory, check |
| 805 Thionville: in the famine nobody may sell grain dear nor abroad; 806 Nijmegen: buying grain at harvest to sell dear is banned | price caps and an export ban in famine years | chronicle 805-12, 806-03 (MGH Capit. I nos. 44, 46) |
| VI Æthelstan 6.2 (930s): horse 120 d, ox 30 d (a mancus), cow 20 d, pig 10 d, sheep 5 d (?) | English livestock | memory, check (Liebermann; Attenborough's 1922 translation) |
| London tolls, c. 1000 (IV Æthelred 2): at Billingsgate a small boat ½ d, a sailing ship 1 d, a keel or hulk 4 d; the men of Rouen with wine or blubber, of Flanders, Ponthieu, Normandy and Francia show their goods and pay toll; the Emperor's men (the Rhine towns) pay at Christmas and Easter in cloth, pepper, gloves and vinegar | a toll by the ship, then by the cargo | chronicle 1000-london-tolls (the game's own data); IV Æthelred |
| Quentovic's royal toll at its height: 10 % per exchange | the Frankish tenth | [Quentovic](https://en.wikipedia.org/wiki/Quentovic) (verified) |
| Dorestad: Charlemagne's charter of 8 June 777 names a riverbank tax (ripaticum) there | a toll at Dorestad from the first | [nl: Dorestad](https://nl.wikipedia.org/wiki/Dorestad) (verified) |
| Raffelstetten, 903–906 (the Danube, the only full toll tariff of the age): a salt ship pays "half a drachma, that is one scoti" at landing and three bushels of salt at Linz; traders may land and trade as far as the Bohemian Forest; slaves and most goods pay by the head or load | a model for how a toll station worked | [Raffelstetten customs regulations](https://en.wikipedia.org/wiki/Raffelstetten_customs_regulations) (verified) |
| Slaves: a toll of a tremissis a head at Raffelstetten (?); in al-Andalus an exceptional eunuch fetched up to 1,000 dinars; the Church banned selling Christians to pagans (Koblenz 922 and others; the pactum Lotharii 840 for Venice) | ordinary captives in the north: 1–2 pounds of silver (guess) | [Slavery in medieval Europe](https://en.wikipedia.org/wiki/Slavery_in_medieval_Europe) (verified for the dinar figure and the bans) |
| Rus treaty of 944: Rus merchants may buy silk at Constantinople only up to 50 gold coins' worth each | a hard cap on the richest good | chronicle 944 (PVL) |

What the goods were and where they came from, 793–1066 (the table the game already has, `goods_793.json`, holds 140 of them with made/wanted regions and cheap/dear months; this is the backbone):

| Good | Made | Wanted | Evidence |
|---|---|---|---|
| Rhenish wine, in silver-fir and oak casks from the Hessian and South German Rhine (3 m by 0.66 m, willow hoops), later sunk as well linings at Dorestad: about 650 real wells in the excavated strip, perhaps 1,000 in use at once | Rhineland, "vineyards south of Mainz" | Frisia, England, Denmark, Norway, Sweden | Research/dorestad_c800_notes.md (Van Es & Verwers); [Dorestad](https://en.wikipedia.org/wiki/Dorestad) (verified: wine from south of Mainz "one of the most prominent products") |
| Badorf ware (almost three-quarters of the wheel-thrown pottery at Dorestad; relief-band amphorae) and Tating-ware jugs | Cologne–Bonn (the Vorgebirge) | every northern market: Hedeby, Birka, Kaupang, Ipswich, York | dorestad_c800_notes.md; Birka's graves hold "pottery from the Rhineland" (verified) |
| Mayen (Eifel) basalt-lava querns, shipped as roughouts and finished at Dorestad by specialist stonecutters; thousands found; the "black stones" Charlemagne sent Offa in 796 are read as these (?) | Mayen/Laacher See | England, Frisia, Scandinavia | dorestad_c800_notes.md; [Offa of Mercia](https://en.wikipedia.org/wiki/Offa_of_Mercia) (verified that the letter speaks of black stones and cloaks) |
| Rhenish glass vessels and beads | Rhineland | Scandinavia, England | dorestad_c800_notes.md; Kaupang "hundreds of glass beads" (verified) |
| Frisian cloth (pallia fresonica), English cloaks | Frisia; England (the 796 letter asks that English cloaks be made as long as they used to be) | Francia, the courts | Offa letter (verified for cloaks); Notker II.9 for Frisian cloaks (memory) |
| Swords and byrnies | Rhineland (Ulfberht blades later) | everywhere; export forbidden from 779 (Herstal c. 20), 805 (Thionville c. 7), death from 864 (Pîtres c. 25) | `trade_rules.json` laws; chronicle 864-06-25 |
| Furs: marten, sable, squirrel, beaver, bear, otter, reindeer hides | the Sámi tribute to Ohthere: from the highest-born "15 marten skins, 5 reindeer skins, 1 bear skin, 10 ambers of feathers, 1 coat of bear or otter skin and two ship's ropes each 60 ells long of whale or seal skin"; the Rus sell "sable, squirrel and other furs" (Ibn Fadlan) and "beaver hides, the pelts of the black fox and swords" (Ibn Khurradadhbih) | Francia, Byzantium, the Caliphate | [Ohthere](https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland) (verified); [Volga trade route](https://en.wikipedia.org/wiki/Volga_trade_route) (verified) |
| Walrus ivory and hide rope | Hålogaland and the White Sea (Ohthere gave Alfred tusks c. 890, "which may mark the start of this trade"); Greenland from c. 1000; nearly all Anglo-Saxon ivory carving is walrus | England, Francia, the Rhine; it replaced elephant ivory, cut off by the Muslim conquests | [Walrus ivory](https://en.wikipedia.org/wiki/Walrus_ivory) (verified) |
| Slaves | war captives everywhere; Slavs through Verdun to Spain; the Volga Rus; Dublin "where captives were traded northwards"; 871: 200 ships bring Britons, Picts and Angles to Dublin | the Caliphate, Byzantium, Scandinavia | chronicle 870; Slavery page (verified) |
| Amber | Baltic (Prussia, Curonia) | Rhineland, everywhere | dorestad_c800_notes.md (Baltic amber at Dorestad) |
| Salt | Noirmoutier and the Loire bay, Lüneburg, the sea-salt of Zeeland and the Frisian peat-salt | every fish and meat curer | goods table; Raffelstetten (salt the chief Danube cargo) |
| Grain and flour | the Frankish heartland, England | Frisia, Norway, the terp country, every famine region | Frankfurt 794; 805/806 capitularies |
| Wool, cloth, lead, tin | England | Francia | London tolls c. 1000; the Rhine towns' cloth payments |
| Honey, wax | Baltic, Wendland, the Rus; wax for every church | monasteries, Byzantium | `commissions_793.json`; the Rus treaties |
| Soapstone, whetstones | Norway (soapstone at Dorestad only after 850) | Frisia, Denmark | dorestad_c800_notes.md |
| Stockfish | Arctic Norway; the long-range dried-cod trade is detectable in the towns from about 1000 (Barrett et al. 2008, J. Arch. Sci. 35; the ScienceDirect page refused the fetch today) | Hedeby, Schleswig, London | `goods_793.json` src `barrett2008` |
| Silk, spices, pepper, incense | Byzantium, the Caliphate, by the Rus rivers and by Venice and the Rhône | courts, churches, the Rhine towns' pepper paid at London | chronicle 944; IV Æthelred |

### 1.3 The routes and the sailing season

| Leg | Time | Source |
|---|---|---|
| Dorestad – Lundenwic, about 430 km real (`ai_fleets.md` gives 380–480 km by the lane) | real: 46 hours at 5 kn (9.26 km/h), 2–4 days with the weather and the tides; in the game's level chain: about 30 game hours (the GDD's ~30 real minutes at 1 s = 1 game minute) plus waits at the phases for tide, wind and dark | `ai_fleets.md`; the GDD; the first build's "87 h" figure was a slip (it is 2.7 kn) |
| Hålogaland – Skiringssal (Kaupang) | "one could not sail in one month if one camped at night" | Ohthere (verified) |
| Skiringssal – Hedeby | 5 days: 3 with Denmark to port and open sea to starboard, 2 with Danish islands to port | Ohthere (verified) |
| Hedeby – Truso | 7 days and nights under sail, Wendland to starboard, Langeland, Lolland, Falster, Skåne, Bornholm, Blekinge, Möre, Öland, Gotland to port | Wulfstan (verified) |
| The Rhine to Birka (Ansgar, 852) | "nearly twenty days in a ship" | [Birka](https://en.wikipedia.org/wiki/Birka) (verified) |
| Hedeby – the Baltic by the Treene | portage of under 15 km at Hollingstedt | [Hedeby](https://en.wikipedia.org/wiki/Hedeby) (verified) |

The AI fleet sails the real distances at the ship's real speed (`ai_fleets.md`: the class polar, 5 kn = 9.26 km/h, a sim hour per game hour), so its passages take the real days above; only the player's levels are shortened. The two meet at the phases (2.10).

The web: the Rhine (Mainz–Cologne–Dorestad) to the Frisian coast and either (a) across to the Thames (Lundenwic), the Wash (Ipswich), the Humber (York), or (b) north by the terp coast to Ribe, Hedeby, then over the portage or round Skagen to Kaupang, Birka, Truso, Grobin, Ladoga; (c) south by the Channel to Quentovic, the Seine (Rouen, the Saint-Denis fair from 9 October), the Loire (Nantes, Noirmoutier's salt), Bordeaux and across Biscay; (d) the road to the Greeks from Ladoga and Novgorod by the Dnieper to Kiev and Constantinople, in the Rus' hands, with Birka and Gotland as the western ends.

The season: the Norse laid up for winter and sailed from April to October; raiders in the game go out about 1 May and are home by 30 September (`ai_fleets.md`); the AI traders are laid up at home in winter (built). The chronicle gives the hard winters that closed the roads: 810/11 (frost closes the roads between Franks and Danes; entry 810-winter-hard-winter-810), 813/14 (813-winter-hard-winter-813, 814-winter-elbe-thaw), 821/22 (the Rhine, Danube, Elbe and Seine frozen so hard that carts crossed for thirty days; 821-winter-rivers-frozen), 859/60 (snow and frost from November to April; 859-winter-hard). The hard dates of the Roman closed sea (Vegetius: 11 November to 10 March) are a guide, not a rule; I could not verify them today.

Ice, where it closes a port every year and where only in the chronicle's hard winters:

| Water | Closed | Standing |
|---|---|---|
| Gulf of Finland (Ladoga's sea road, Peterhof) | "can freeze from late November to late April; the freezing starts in the east"; complete by late January, not in mild winters | [Gulf of Finland](https://en.wikipedia.org/wiki/Gulf_of_Finland) (verified) |
| Lake Ladoga (Aldeigjuborg) | the shores freeze in December, the middle by February in hard winters only; the ice breaks up in April–May | [ru: Ладожское озеро](https://ru.wikipedia.org/wiki/%D0%9B%D0%B0%D0%B4%D0%BE%D0%B6%D1%81%D0%BA%D0%BE%D0%B5_%D0%BE%D0%B7%D0%B5%D1%80%D0%BE) (verified) |
| Bay of Bothnia | ice-covered 110–190 days a year, "up to six months" | [Bay of Bothnia](https://en.wikipedia.org/wiki/Bay_of_Bothnia) (verified) |
| Mälaren (Birka, Sigtuna) | December–March most years (guess; neither Wikipedia page gives months) | (guess) |
| The Danish belts, Hedeby's Schlei, the Rhine, Seine and Thames | only in the chronicle's hard winters above | chronicle (the 821/22 entry names the rivers; verified in the game's data) |

### 1.4 Tolls, markets, the king's rights

- The Frankish tenth: a royal toll on the sale at Dorestad (the ripaticum of 777, verified), Quentovic (10 % at its height, verified), Rouen, Maastricht and the river tolls; exemptions by charter for great abbeys (Saint-Germain-des-Prés had one for Rouen, Quentovic, Amiens, Maastricht and Dorestad, 779: memory, check). The chronicle's laws: only old and just tolls on bridges, ships and markets, new tolls banned (805); no toll where no bridge or ferry helps (809); Sunday markets only where old (809).
- Coin: only the palace's deniers from 805 (coin to be struck only at the palace; good deniers in use stay valid); Pîtres 864: a new coinage from ten mints only. In England, Æthelstan 928: one coinage, moneyers named for each town, and no buying worth over 20 pence outside a town; Edgar 973: one design, called in every few years (all chronicle).
- Merchants' law: Charlemagne to Offa, 796: each realm's merchants protected, and pilgrims who are really traders pay toll (chronicle 796; the letter's content verified); the Alfred–Guthrum treaty (878–890): nobody crosses to the other side without permission, but trade is allowed "on condition that hostages are given as security for peaceful behaviour" (verified); the Rus treaties of 911 and 944 with Byzantium: a prince's charter, the St Mamas quarter, the silk cap (chronicle); Basil II's chrysobull to Venice, 992: lower tolls at Abydos and Constantinople (chronicle); Cnut at Rome, 1027: lower tolls and safe passage for English and Danish merchants and pilgrims (chronicle); the merchants of Tiel in 1020 "live by their own law", claim an imperial charter to judge their own cases, "swear and drink together" (chronicle 1020-tiel-merchants, Alpert of Metz).
- The king's men in kind: in the lands of weighed silver there was no toll in coin; the king's reeve took his due in gifts (the Danish king's man at Hedeby; Rimbert) (guess at the share).

### 1.5 Who the traders were

Frisians (the carriers of the Rhine, with quarters at York, Mainz and Worms, and at Birka: Rimbert's Frideburg (memory); "merchants from Dorestad and Hamburg" come to Hedeby in 850, chronicle), Franks and Rhinelanders (the Rhine towns' men at London c. 1000), Saxons (the English at the Saint-Denis fair; `fairs_793.json`), Danes (at Dorestad itself after Rorik's grant of 850; the Danelaw's York–Dublin axis after 866), Swedes and the Rus east (the "Rhos" of 839 proved to be Swedes, chronicle; Ibn Fadlan 922), Jews (the Radhanites and Verdun's trade to Spain), and the merchants of Quentovic "primarily Frankish, Saxon or Frisian" (verified).

### 1.6 How the chronicle moved the trade

| Date | Event | Effect on trade (real) |
|---|---|---|
| 793–794 | Great famine in Francia and Northumbria; false grain in the fields; then 794 a drought yet an abundant harvest | grain dear through spring 794, cheap after harvest |
| 796 | Offa dies; Charles–Offa accord on merchants | the Channel trade safe again after the embargo of c. 789–790 (verified: Charlemagne had forbidden English ships his ports) |
| 805–807 | famine, Thionville price rules, three-day fasts | grain caps and an export ban; fewer cargoes |
| 808 | Godfred destroys Reric and moves its merchants to Hedeby; the Danevirke | the Baltic–North Sea crossing fixed at Hedeby for 250 years (verified) |
| 810 | 200 Danish ships ravage the Frisian islands; 100 pounds tribute | Frisia's coast unsafe; silver north |
| 834, 835, 836, 837, 846, 863 | Dorestad raided (the Dutch page counts 834 from Utrecht, 837 from Walcheren, 847 with the Betuwe, 863 the last; the English page says between 834 and 839 Dorestad was raided only once, in 834, and "never fully recovered") | the emporium's stock looted, its market shut for weeks each time; tribute 837 |
| 836 | Witla and Antwerp burned | the Maas-mouth market ends |
| 838-12-26 | a storm flood over Frisia: 2,437 counted drowned | cattle, cheese, butter, wool gone for a year; grain and timber wanted |
| 841, 850 | Lothar grants Walcheren to Harald, Dorestad to Rorik | Danes hold the toll; the Frankish tenth in Danish hands |
| 842 | Quentovic sacked at dawn, sparing the houses that paid | the Channel emporium hurt; it struck coin to 980 and then gave way to Montreuil and Saint-Omer (verified) |
| 845 | Paris sacked, 7,000 pounds paid | silver north; the Seine trade shaken |
| 860s | the Rhine's main stream was moving to the Lek and Waal through the 9th century and the Kromme Rijn silted, with the Katwijk mouth; a storm-dam thrown up by the townsfolk in 863 that sent the flood down the Lek is Dr R. van Luttervelt's conjecture, reported as such by the Dutch page ("zou volgens dr. R. van Luttervelt ... zijn opgeworpen", verified); the English page says only that Dorestad's "disappearance in the 860s was a consequence of a shift in the course of the river channel" rather than the raids (Forte, Oram & Pedersen, verified); the Kromme Rijn was finally dammed at Wijk bij Duurstede in 1122 by Bishop Godebald's order ([nl: Kromme Rijn](https://nl.wikipedia.org/wiki/Kromme_Rijn), verified) | Dorestad's harbour dies; the trade goes to Tiel (toll right 896, verified), Deventer and Utrecht. The game keeps `ruined` from 863: chronicle entry 863-dorestad-last and `ports_793.json`'s `to: 863` already carry it |
| 865–878 | the great army; York 866; East Anglia 869; the Danelaw | English markets reorganised; York a Norse town; weighed silver in eastern England |
| 882 | the Northmen burn Liège, Maastricht, Cologne, Bonn | the Rhine–Meuse trade cut for a season |
| 886 | Alfred holds London | London the English emporium again |
| c. 900–960 | Kaupang abandoned in the early 10th c. (verified); Birka dies out c. 960 (verified), Sigtuna laid out c. 980 | the Norwegian and Swedish gateways move |
| 928, 973 | Æthelstan's and Edgar's coin laws | foreign coin to the moneyer at a loss; trade only in towns |
| 965 | Ibrahim ibn Yaqub finds Hedeby poor and its people heathen; Samarkand dirhams at Mainz | the east reaches the Rhine |
| 980–1018 | the raids and tributes of Æthelred; Sweyn and Cnut | about 220,000 pounds of silver out of England (the sum of the figures in 1.1) |
| 1000s | dirhams stop; German and English pennies take their place | the Baltic's silver now comes from the west |
| 1006 | Tiel burned; Utrecht's people burn their own trading quarter | the Waal and Vecht markets shut for a year |
| 1018 | Vlaardingen: Dirk III holds his toll on the Merwede, where the Tiel merchants pass | a new toll on the Rhine–England road |
| 1050, 1066 | Hedeby burned by Harald Hardrada, then by the Slavs; its people move to Schleswig | the Baltic gateway moves across the fjord |
| 1066 | Hastings; William crowned at Christmas | the game's end: the heir's line is weighed |

---

## 2. The model

### 2.0 Tempo: game days in real hours

The fixed clock is 1 real second = 1 game minute, 4x at most in single player, never faster, and no zoom time warp (the user's rule). Every duration in this document is in game days; this is what each costs the player at the keyboard:

| Game time | Real time at 1x | At 4x |
|---|---|---|
| 1 hour | 1 minute | 15 s |
| 1 day | 24 minutes | 6 minutes |
| a Dorestad–Lundenwic passage in the levels (~30 game hours) | ~30 minutes | ~7.5 minutes |
| 10 days | 4 hours | 1 hour |
| 30 days | 12 hours | 3 hours |
| 60 days | 24 hours | 6 hours |
| 180 days (a winter lay-up, October to April; the credit term) | 72 hours | 18 hours |
| 365 days (the count's loan) | 146 hours | 36.5 hours |
| 793 to 1066 (273 years, ~99,700 days) | ~39,900 hours | ~10,000 hours |

So a year holds about 290 one-way passages of 30 game hours, or about 140 round trips before any wait, and a "60-day market closure" is a full real day of play. Nothing in `LevelClock` passes time but the dev keys (T/Y/U, an hour, a day, a month), and a wait mechanic does not exist yet. Two consequences:

1. **Until the user answers open question 0 (a "wait in port" action), every rule below is tuned to what a player at 1x will actually see.** A raided market is `disrupted` until the player's next visit or 10 days, whichever is first (not 30–60); credit is due at the next visit (not in 180 days); the AI traders' winter lay-up matters only if the player winters too (otherwise the player trades through a winter in which the AI stays home, which is a quiet sea and dear imports, not a wall). The famine's span is real (793-01 to 794-08) but the player experiences it as the state of the markets, not as time passing.
2. **If the user grants a wait action** (winter over; wait N days at the harbour, the panel ticking the days and the news arriving; the visible clock rate never changes, the player chooses to let days pass), the longer durations in 2.5 and 2.9 come back: 30–60-day closures, the 180-day credit, the season arbitrage of 2.2 (buy the new wine in November, sell it in June). The model holds both; only the parameters differ, so `trade_rules.json` carries a `tempo` block with both sets and the clock picks one.

Open question 1 (section 6) also asks whether a phase at a level edge may advance the date by the cut-out distance's sailing time (a jump of hours, not a rate change). If yes, the Dorestad–Lundenwic passage becomes ~46 game hours including the cut-outs, and news lag, spoilage and crew pay follow the real passage; if no, they follow the ~30 game hours of the level chain. The numbers below give both where they differ.

### 2.1 State

The economy runs on plain C# data, no scene objects, brought up to date per port when something touches it (2.3) and once a game hour for every AI ship (the AI-fleets plan's WorldSim). Nothing is drawn; the levels read it.

**A port** (one of ~450 across the 240 ports and 210 river places; the first build's 50 in `ports_793.json`, plus `ports_more.md`'s 119 once written into `ports_more.json`, plus the level chart's places) holds:

| Field | Meaning |
|---|---|
| region, realm, kind, size (1–5), from/to years | from `ports_793.json` |
| state | `open`; `disrupted` (raided or burned: no dealing until `disruptedUntil`, then stock regrows at half regen for 90 days); `ruined` (its `to` year passed: size falls one step a year to 0); `closed_ice` (a port in a frozen water in a frozen month, table 1.3) |
| wealth W | deniers' worth the place can pay out this season; starts at size² × 400 d (guess); it rises when the port sells, falls when it buys, drains in a raid or tribute, and recovers toward its usual at 2 % a day |
| stock, usual, appetite, glut, dearer per good | the first build's `MarketGood` (kept as is) |
| production P_g(t), use C_g(t) per good | new: the daily making and using, which `usual` is derived from, so that a season, a harvest or a famine changes what the market settles back to |
| board | the port's price board: the snapshots ships have brought (the first build's `PortBoard`) |
| toll rule | from the zone, overridden by the port (Dorestad a tenth, Hedeby a gift in kind, London by the ship after 1000) |

**A region** (the 40 trade regions of `goods_793.json`) holds its silver level M (see 2.4), its harvest factor h for the year, its season flags (ice months, closed sea), and its risk (raids seen lately).

**A ship** (the player's or an AI trader's): hold (lots with cost and the day bought), purse (coins by kind), crew owed, the snapshots it carries, its route.

### 2.2 Prices

The first build's `Market.Price` (read today) is:

```
p = value_g × MadeFactor(made) × WantedFactor(wanted) × Season(g, month)
if usual > 0:        p *= clamp((usual / max(stock, 0.5)) ^ elasticity, price_min, price_max)   (scarcity)
else if made = 0 and wanted = 0:  p *= unwanted (0.6)
p *= (1 + glut / appetite) ^ -glut_elasticity                                                (glut)
p *= 1 + dearer                                                                              (shock)
asks = p × 1.1;  pays = p × 0.9                                                              (spread 0.2)
```

with MadeFactor 0.5→1.4, 1→0.8, 2→0.65, 3→0.5; WantedFactor 1→1.15, 2→1.4, 3→1.7; Season 0.85 in the good's cheap months, 1.2 in its dear months, by good, not by place; elasticity 0.4; `usual = clamp(size × made × 150 / value, 1, 120)` so an imported good (made 0) has usual 0 and no scarcity term; `appetite = max(1, size × (wanted or 0.5) × 80 / value)`; `GotOne` adds a unit of glut when the port does not make the good; every term applies per unit, to asks and pays alike, so a buyer's tenth unit is dearer and a seller's tenth cheaper. `Deal.AsksFor/PaysFor` then apply the merchant's standing: `Reputation.PriceFavour(port) = standing/100 × price_at_100 (0.1)`, so standing 100 buys 10 % cheaper and sells 10 % dearer (built; `reputation_793.json`).

This document keeps all of it and adds four terms:

```
season   = cosine-blended over 20 days at each edge of the cheap and dear months, so prices drift
            instead of jumping on the first of the month (2.3)
scarcity exponent by category: grain 0.6, provisions 0.5, drink 0.45, cloth 0.4, luxuries 0.3
            (the first build's 0.4 for all is kept until tuned)
silver   = (M_r / M_r,793) ^ 0.3                 (the region's silver level; 2.4)
wealthcap= min(1, W / (W_usual × 0.25)) ^ 0.5    on pays only: a drained port pays less, whatever it wants
```

With stock at zero the market asks at `price_max` and there is nothing to buy. A good the port neither makes nor wants is paid at 0.6 × value (`unwanted`), and only up to a small appetite.

Worked example, 21 June 793 (no shocks, standing 0), through the code as it stands. Rhenish wine, value 60 d, dear in June–August, cheap in October–December; made `{rhineland: 3}`, wanted `{frisia: 2, england_south: 2, ...}`. Dorestad is in `rhine_delta`, which neither makes nor wants it, so today the code gives usual 0, stock 0 and nothing on offer: the table needs `rhine_delta: 2` in `rhenish_wine.made` (Dorestad was where the Rhine's wine was sold on; this is the one data change the example rests on). With it: usual = 5 × 2 × 150 / 60 = 25 casks, MadeFactor 0.65, June 1.2: the first cask asks 60 × 0.65 × 1.2 × 1.1 = 51 d; buying ten takes the stock from 25 to 15 and the scarcity term lifts the tenth to 62 d; the ten cost about 560 d (510 d if the market stood at 1.25 × usual that day; the first build seeds stock at 0.75–1.25 × usual by a hash of port and good). At Lundenwic (size 5, `england_south` wanted 2, made 0): appetite = 5 × 2 × 80 / 60 = 13.3 casks; the first cask pays 60 × 1.4 × 1.2 × 0.9 = 91 d; each cask sold adds a unit of glut, so the tenth pays 91 × (1 + 9 / 13.3)^-0.5 = 70 d; the ten fetch about 790 d, less the English toll of 5 % = 750 d. Gross about 190 d, less the crew (4 men in the player's Gislinge boat per `ship_classes_793.json`; pay 0.25 d a man a day (guess): 1–2 d for the passage) and bread. The debt is 480 d: three such runs clear it, two if the hold also carries querns or Badorf ware for a second line of profit. In November the same wine asks 37 d at Dorestad and pays 64 d at London (cheap season at both ends; the code's season is by good): the real money is the season gap, wine bought at 37 d in November and sold at 91 d in June, about 350 d gross on ten casks, and that needs the wine held over a winter, which is open question 0. One bad run (a glutted London: three Frisian ships in before you, glut 30, the first cask paying 50 d) loses money. That is the right scale: the user's "buy low, sell far".

### 2.3 The tick, per port

```
Day(port, t):                                  (closed form over the days since the port was last touched,
  for each good:                                the first build's CatchUp extended by production)
    usual_t  = usual_0 × make_season[cat, doy(t)] × h_region,year × state_factor
    stock   += (usual_t − stock) × (1 − (1 − regen)^days)
    stock   -= use_g × min(stock, appetite × 0.02) × days        (the town eats its imports)
    glut    *= (1 − glut_fade)^days;  dearer *= (1 − shock_fade)^days
  W += (W_usual − W) × (1 − 0.98^days)
```

`make_season[cat, doy]` is a table precomputed once at load, one row per category (about 10) by day of year (366): grain 0 from March to July and a spike at harvest (the stock, not the price, carries the year: a granary that empties by June); the vintage in October–November; herring in the autumn runs; furs best in winter (bought at the summer markets); walrus and whale in summer; salt made in summer (evaporation), dear in the autumn curing; eider down in June; wool at the June shearing; stockfish dried in late winter, shipped from May. The season price curve (2.2) is a second table of the same shape built from each good's cheap and dear months with the 20-day cosine edges.

**When a port is ticked.** Lazily: a port is caught up to today only when something touches it (the player lands, an AI ship arrives or plans against it, the chronicle fires on it, the UI reads it), stepping in chunks of at most 10 days so the season table is read at the right day. The ~30 ports the AI traders are heading to are caught up at the day rollover; the rest wait. A 273-year run therefore costs about as much as the ports' visits, not 63,000 rows × 99,700 days. The tick runs on the main thread at the day rollover; a full catch-up of every port is well under 2 ms and happens once in 24 real minutes, so there is no worker thread and no copy for the UI: `Deal` and the tick mutate the same `MarketGood` on one thread.

### 2.4 Silver

Each region has a silver level M, a dimensionless 1.0 in 793. It is moved by: tributes (−X for the payer's regions, +X for the receiver's, the chronicle's pound figures scaled: 72,000 pounds in 1018 takes England from 1.0 to about 0.7 and lifts Denmark from 1.0 to 1.6 (guess at the pool sizes)); the dirham stream (the Baltic, Svealand, Gotland, Ladoga: +0.004 a month from 800, +0.012 at the peak 900–950, falling to 0 by 1015: the Volga chronology, verified); the Rammelsberg silver from 968 (the Rhineland and Saxony +0.003 a month from 968, and from there the north: chronicle 1000); raids (−2 % of a looted port's region, +1 % at the raiders' home); and trade itself (silver moves with every deal between regions: the sum of what buyers pay). The price level follows M^0.3, so England's 1018 prices in silver fall about 10 % and Denmark's rise 15 %: enough that a merchant notices the north paying more, not so much that the tables break.

### 2.5 The chronicle's shocks

`chronicle_793.json` fires an entry on its `fire` day (582 entries; kinds counted today: raid 67, war 56, death 64, battle 42, law 34, embassy 32, treaty 28, accession 29, campaign 25, politics 26, eclipse 18, portent 16, fleet 12, founding 12, burning 10, deposition 10, famine 8, synod 7, storm 7, winter 6, coin 6, evidence 6, epidemic 4, murrain 3, trade 3, harvest 2, flood 1, fair 1, and a few more). There is no `tribute` kind: the tributes sit under raid, treaty, battle, death and politics, and the one great flood is kind `storm`. So `shocks_793.json` (new) has two parts: **by entry id** for the entries that carry a number or a named port effect, and **by kind** for the rest.

By id (the list to write first; every one is in the chronicle today):

| Entry id | Effect |
|---|---|
| 793-great-famine, 793-northumbria-famine, 805-famine-805, 823-famine-epidemic | famine over the entry's regions for its span (`fire` to `end`) |
| 794-drought-harvest | h = 1.2 for the Frankish regions, 794 |
| 806-autumn-ireland-mast | h = 1.2 for pigs and acorn-fed goods, Ireland |
| 793-06-08-lindisfarena-raid | raid on lindisfarena, loot 0.7 |
| 808-summer-reric-destroyed | reric `to` 808 (built); sliesthorp size +1 and its stock of Baltic goods × 1.5 over 90 days |
| 810-summer-frisia-raid-810 | raid on the Frisian coast ports; silver 100 pounds frisia → jutland |
| 836-witla-antwerp-burned, 846-dorestad-burned, 1006-tiel-burned, 1050-hedeby-burned, 1066-hedeby-slavs, 845-hamburg-burned | burning: `disrupted` 180 days (or until the next visit, 2.0), then size −1 |
| 837-06-17-walcheren-raid | raid on walichrum; dorestad W × 0.7 (the tribute) |
| 838-12-26-frisia-flood | the flood row below over frisia, rhine_delta, zeeland |
| 842-quentovic-sacked | raid on quentovic, loot 0.5 (the houses that paid were spared) |
| 845-03-28-paris-sacked | raid on parisius; silver 7,000 pounds neustria → jutland/vestfold |
| 850-rorik-dorestad, 841-walcheren-harald | the port's toll rule → `danish_gift` |
| 863-dorestad-last | raid on dorestad, loot 0.7; `ruined` from 863 (the `to` year, built) |
| 810-winter-hard-winter-810, 813-winter-hard-winter-813, 821-winter-rivers-frozen, 859-winter-hard | hard winter over the entry's regions for its span: the named rivers and belts `closed_ice` |
| 809-cattle-plague-809, 810-britain-cattle-plague, 810-cattle-plague-810 | murrain: cattle, hides, cheese, butter usual × 0.5 for 365 days |
| 991-08-maldon, 994-09-08-london-attack, 1002-11-13-st-brice, 1007-tribute, 1009-08-thorkell, 1012-04-19-aelfheah, 1018-heregeld | tribute: silver by the pounds named (10,000; 16,000; 24,000; 36,000; 3,000; 48,000; 82,500) England → Denmark; the raiders' sale markets glut with loot for 60 days |
| 794-06-frankfurt-prices, 805-12-thionville-coin, 973-edgar-coinage, 995-sigtuna-coins, 997-dublin-coins | coin and price laws: the zone's `foreign_loss` and price caps by year |
| 1000-london-tolls, 1018-07-29-vlaardingen | toll rule changes at lundenwic (by the ship) and the Merwede |
| 796 Charles–Offa, 811-spring-eider-peace, 992 Venice, 1027 Cnut at Rome | treaty: risk and tolls lowered on the named lanes |

By kind, for everything else:

| kind | At the port(s) | In the region(s) | Elsewhere |
|---|---|---|---|
| raid | stock of every good × (1 − loot), loot 0.4 (a village) to 0.7 (an emporium); W × 0.5; `disrupted` until the player's next visit or 10 days (30–60 with a wait action; dealing refused, then stock regrows at half regen for 90 days); captives +N at the raiders' sale markets 20–40 days later (N = size × 10) | risk +1 for 120 days: AI traders weigh it; the player's crew ask about it | news travels at ship speed |
| famine | — | grain and flour usual × 0.5 for the span; wanted(grain) = 3; dearer(grain) = +1.0 reset monthly; W × 0.7 (a hungry town buys little else); the 805 export ban as a law flag | grain exporters' prices rise as ships come |
| harvest (good or bad) | — | h = 1.2 or 0.7 for the year's grain | — |
| flood / storm surge | livestock, cheese, butter, wool, hides usual × 0.3 for 365 days; salt-marsh salt × 0.2; grain, timber, tar wanted = 3 | wealth × 0.6 | — |
| storm | AI ships at sea that day: loss chance × 5; a `WeatherRequest` for the entry's regions (below) | imports' stock −10 % (wrecks) | — |
| winter (hard) | the waters of table 1.3 `closed_ice` until the entry's `end`; fodder, firewood, salt pork dear (+0.5) | sailing season closed; the AI stays in port; a `WeatherRequest(frost)` | — |
| law: toll | the zone's or port's toll changes (805: new tolls void; 864: ten mints; 1018: the Merwede toll) | — | — |
| law: coin | foreign-coin loss changes (928, 973: recoinage, old pennies at a loss) | — | — |
| law: arms, horses, captives | `trade_rules.json` laws by year (built): the king's men search; goods seized | — | — |
| treaty | tolls or risk lowered on the named lanes; the Alfred–Guthrum hostage rule: crossing the Danelaw line costs a day and a hostage bond of 20 d (guess) | — | — |
| war, campaign, battle | lanes through the war's regions get risk +2; requisitions: grain and horses wanted = 3 in the army's region; W × 0.8 | — | — |
| burning, founding | the port's `to` or `from` year (built); a burned port is `disrupted` 180 days (or until the next visit) then rebuilt at size −1 | — | — |
| murrain, epidemic | livestock × 0.5 (murrain); W × 0.8 and labour goods dear (epidemic) | — | — |
| fair | `fairs_793.json` (built) | — | — |
| coin (new mint), evidence, portent, eclipse, death, accession, embassy, synod, church | news only, a rumour in the tavern (an NPC's knowledge, 3.2); an accession resets the realm's toll rule to its default | — | — |

**The weather hook.** The storms plan owns `LevelWeather`/`HdrpWeatherDraw`; the economy does not drive them. `Chronicle` posts `WeatherRequest(date, regions, kind)` (storm, surge, frost, thaw) into a list `LevelWeather` reads for its own level's region on that day and may honour; the storm of 838 in Frisia is a request for the Dorestad and Walcheren levels on 26 December 838, and the frost of 821/22 a request for every Rhine level from 15 December to the end of February. What the level draws is the storms plan's business.

Random shocks within the chronicle (replayability): each year, per region, a seeded draw (`SimRandom` from the save's seed, the region and the year): the harvest factor h ~ clamp(1 + 0.25 N(0,1), 0.5, 1.3) unless the chronicle names the year; a 10 % chance of a local fire (a market `disrupted` 20 days or until the next visit); a 15 % chance of a bad herring year; a 5 % chance of a murrain (cattle × 0.5). The chronicle's named events always happen; the small ones differ from game to game.

### 2.6 News and knowledge

Kept from the first build (`knowing_793.json`, `Knowledge.cs`): a price is a note with a day, a sureness and a source (seen 1.0, told 0.85, talk 0.7, written 0.8, rumour 0.45); told prices lose 0.1 sureness a mouth and carry a 3 % error plus 2 % a hop; price knowledge halves in sureness every 30 days, news every 90, raid sightings every 4 days. Ships carry snapshots of the last three ports they lay at; boards merge on arrival; two AI ships meeting at sea swap. News therefore crosses Dorestad–London in about 1.5 days (a 30-game-hour passage plus a wait for a tide or daylight at the phase), reaches Hedeby in 3–4 days, Birka in 2–3 weeks, Constantinople in months: in the player's Dorestad–London trade the lag is a tide, not a season, and what the player does not know is what happened at London since the last ship left it, about a day and a half ago. The player hears prices by talking at the harbour (a text panel, 3.2), by hailing a ship, by a letter from a factor (3.4), or by seeing them: only a seen price is sure.

### 2.7 NPC traders

The approved AI-fleets plan stands: ~130 traders (0.4 × size² × a kind factor per home port), each with a seeded RNG, laid up in winter, choosing ports by softmax over believed margins less tolls, spoilage and the crew's keep, dealing through the same `Deal` as the player, sailing the real distances at the real speed (1.3). Additions:

- The population follows the ports' states and the century: Frisian traders are 50 % of the fleet in 793, 25 % by 900 (Dorestad gone; Tiel's men take over), Danes 10 % in 793 and 35 % by 900; York–Dublin Norse traders appear from 870; Rus traders at Birka, Gotland and Ladoga from 860; Rhineland "Emperor's men" at London from 950.
- Each trader has a `beat` (home, 2–4 usual ports), so a port sees the same faces: the player can learn that "Hrothgar of Ribe is in London every June" and sell before him.
- Traders fill commissions (`commissions_793.json`, built: `ai_fill_per_day` 0.04).
- Traders avoid regions with fresh raid news (built) and winter at home; after 863 the Rhine traders' home becomes Tiel.

**Backfill for a later start year** (3.3) covers the markets, the silver, the port states and the chronicle only: `WorldEconomy` runs headless from 793 to the start day in a few seconds (the lazy tick makes it cheap). The AI fleet is not backfilled (its approved target is 365 days in 60 s, so 273 years would be 4.5 hours): the traders spawn fresh on the start day at their homes, with usual-price beliefs, empty boards and no standing with the player. Boards fill within the first season of play.

### 2.8 Tolls and the king

At a sale in a toll port the zone's toll is taken from the seller (built; `Deal` also honours `Chest.TollFreeAt`, a riddle's reward). Additions: toll stations only (emporia, royal seats, towns; a village or a beach market takes nothing); a charter aboard (a monastery's letter, won by a commission kept) exempts that cargo; the toll house is a text panel (3.2) where the toll-keeper reckons, searches (arms, captives, horses after the laws), and can be bribed (a deed in `reputation_793.json`, built). The king's men patrol (`patrols_793.json`, built). In the lands of weighed silver the reeve takes a gift: 2 % of the sale's worth in kind, or standing falls (guess).

### 2.9 Credit and the debt

Lending at interest was banned to clerics by the Admonitio generalis of 789 (c. 5: memory, check; the Wikipedia page does not quote the chapter) and to everyone, with the definition of usury as "turpe lucrum", by the capitulary of Nijmegen, 806 (MGH Capit. I no. 46 c. 11–17: memory, check; the same capitulary that bans buying grain at harvest to sell dear, 1.2). So the count's loan is a fixed sum with a day: 480 d (two pounds) due in 365 days (`trade_rules.json` `start`, built). Beyond it, credit is by standing: at a port where standing ≥ 40 a merchant can buy on credit up to size × 100 d, due at his next visit (within 180 days once a wait action exists); a debt unpaid at the day is a deed (`broke_credit`, −8 among Franks and English, −4 Norse (guess)) and the port refuses credit for a year; two broken credits and the count's men take the ship at any Frankish port (game over in single player, as a wreck is). A partner's share: a rich man of the port offers silver for a voyage against half the profit (the Mediterranean commenda's shape; thin evidence in the north, guess, and marked as such in the text).

### 2.10 The player's ship between levels

The player's ship moves at its real speed in true-size places and shortened oceans; the economy only needs to know which port the ship is at and the date. At a phase (`LevelExit`/`LevelEntry`) the date is whatever `LevelClock` says on arrival; if open question 1 is answered yes, `LevelEntry` adds the cut-out distance's sailing time to the clock before the next level starts, and the hold's lots age by it. Spoilage is new: `goods_793.json` has `keeps` days per good and `Hold`'s lots carry the day bought, and the AI's `TraderBrain` already refuses routes longer than 0.8 × `keeps` (built), but no code yet spoils a lot in the player's hold; `Hold.Spoil(today)` (a lot older than `keeps` loses its count, told as a fact) goes in with night two.

---

## 3. In the game

### 3.1 What makes a trade run exciting

1. **The gap you believe in.** You bought wine at 51 d because the ledger says Lundenwic paid 91 d yesterday morning (a note a day and a half old, "likely"), and three Frisian ships left Dorestad ahead of you. The passage is spent not knowing whether it still does.
2. **The clock.** Wine keeps 540 days; herring 270; the fair at Saint-Denis opens 9 October and the Ribe summer market closes in August; the count's day is a year from 21 June; the sea closes in November and the Baltic freezes. Every cargo has a date, and at 24 real minutes a day the dates are near: a round trip to London and back is a day and a half of the year, and a bad tide costs a quarter of a day.
3. **The unknown on arrival.** The harbour is full of Frisian sails: three ships in before you and the price down a third (glut). Or the town was raided a fortnight ago and buys nothing. Or a famine has made your 10 sacks of wheat worth a sword each.
4. **The choice on the way.** At Quentovic the toll-keeper says Rouen's wine is cheap this year; sell your casks here now at 70 d, or carry on to London for 91 and the risk. The "Talk to people" panel is where the game is played (3.2).
5. **The reveal.** The market panel shows the deal line by line, each cask a little cheaper as the glut sets in, the toll taken, the purse in deniers, pennies and hacksilver; then the ledger's new line: what the run made, after the crew and the bread.
6. **The long game.** A second ship, a factor left at London who writes, a hoard buried against raids, standing high enough to buy on credit at Hedeby, a daughter married to a Tiel merchant, the line to 1066.

### 3.2 What the player sees and does

- **The harbour (3D, the level):** the ship at a dam head at Dorestad; walk or row to the toll house, the merchants' houses, the church, the tavern; each opens a text panel (the user's "text based adventure", below): a fact, options, a factual outcome, placeholders for the user's own words. The level's `TownLife` shows the market's state: stacks of querns and casks on the dams when stock is high, empty dams and burned warehouses after a raid, no smoke in a famine winter.
- **The market panel:** goods on offer with asks, what it pays for what is aboard, stock, a one-line "why" (made here; dear this month; a famine; glutted by your own sale); buy and sell by the unit with the price moving; the toll shown.
- **The ledger (2D chart, a side tab):** every price note with its day and sureness ("Lundenwic, wine pays 91 d, 2 days ago, likely"), the voyage accounts, the debt and its day, commissions taken, credit owed, the fairs' dates, the season's end.
- **At sea:** hailing traders for their prices and weather (built in the plan); raiders; the king's men searching for arms and captives.
- **The chronicle's days:** the log shows the entry on its day; where it touches the player's port the panel offers what to do (a raid coming up the river: load and go, or stay and fight; a famine: the count asks for grain at a fixed price, 805's law).

**People: the NPC model.** The host is the first build's `GameEvents` (`events_793.json`, `GameEvents.cs`, 223 lines): an event is a label, an intention and options whose outcomes are facts, each with a key in `writing.json` for the user's words; the game never writes story text. People in port are the same shape with three additions:

| Part | What it is |
|---|---|
| **Role** | toll-keeper, moneyer, reeve, harbour-master, tavern-keeper, priest, the lord's man, a merchant of the port, a ship's master in from elsewhere, a factor you hired. One or two per port by kind and size; the same name every visit (seeded from port and role). |
| **Knowledge** | what this person can tell, drawn from live state: the port's own board (sure), their beat's ports (a ship's master: told, with `knowing_793.json`'s per-hop loss of 0.1 sureness and 2 % error), chronicle entries within N days in their region (N = 30 for a reeve or priest, 10 for a tavern-keeper, who also knows rumours at sureness 0.45), the laws in force here (toll-keeper, reeve), the fairs' dates (merchant), who is in harbour and who left when (harbour-master). They do not know what they cannot: a tavern-keeper has no London price unless a ship brought one. |
| **Options per visit** (3–5) | ask prices (fills the ledger at the role's sureness); ask news (the chronicle entries they hold, in their words, as the user writes them); a gift (silver or a unit of a good the port wants: standing +1 to +3 by its worth, a deed); one haggle roll a visit (chance = 0.3 + standing/200; success: this deal's spread halved; failure: the market's price for you +5 % today, a deed `haggled_hard`); the role's own act: pay or dispute the toll, change coin, hire a factor or a crewman, take a commission, buy a charter copy, ask for credit (2.9). |
| **Outcome** | facts only, through wording templates (`wording_793.json`, built) and `writing.json` keys (`person.<role>.<option>.<result>`); every outcome a deed in the deeds log (built in `Reputation`), so a bribe, a gift or a haggle is remembered at that port and, by the deeds' spread rules, at others. |

A worked exchange, the toll-keeper at Quentovic, 2 October 793, ten casks of Rhenish wine aboard, standing 0:

```
[toll-keeper]  Quentovic takes the king's tenth on what is sold here. You carry ten casks of wine and
               two sacks of querns. (writing key: person.tollkeeper.greet)
   1. Pay the tenth and trade.         -> the toll flag set; the market panel opens; 10 % off each sale
   2. Ask what wine fetches here.      -> "Wine pays 68 d a cask today" (seen: the port's own board, sure)
   3. Ask the news.                    -> the chronicle entries of neustria in the last 30 days, e.g.
                                          "the Saint-Denis fair opens on the 9th" (fairs_793.json)
                                          and any raid or famine fired on neustria since 2 September
   4. Offer him a cask.                -> standing at Quentovic +2; a deed `gift_tollkeeper`; his
                                          price-talk sureness rises to 1.0 for this visit (guess)
   5. Say the casks are for Rouen, not for sale here. (a lie if you then sell)
                                       -> no toll today; if you sell here after all: a deed `cheated_toll`,
                                          standing −6 at Quentovic, −2 across neustria, and the reeve
                                          searches you on your next visit (Reputation.SearchFactor, built)
```

Every number here is a starting guess in `people_793.json` (new, the roles, their knowledge sets and option chances); the words are the user's.

### 3.3 Replayability

- **Start years:** 793 (the user's start, Dorestad in its heyday), 834 (the raids begin), 865 (the great army; the Danelaw), 896 (Tiel's toll; Alfred's ships), 991 (Maldon, the tributes), 1016 (Cnut); each with its ports' states and silver levels set by running the markets, silver, port states and chronicle headless from 793 to that day (a few seconds); the AI traders start fresh on the day (2.7).
- **Seeded small shocks** within the chronicle (2.5): harvests, fires, herring, murrains differ each game.
- **The player's weight:** at a size-2 market the player's own cargo moves the price; at Dorestad it does not; a run that worked once gluts the next time.
- **Homes:** later, a Dane at Hedeby, a Saxon at Hamwic, a Norwegian at Kaupang: the same world, different tables of made and wanted, different laws.
- **Difficulty by truth, not by sliders:** the hard years are the hard years.

---

## 4. Build plan for Unity

### 4.1 Where it lives

The project has `Kapman.Core` (no references), `Kapman.Runtime` (namespace `Kapman.Levels`; references Core, the Input System and HDRP), `Kapman.Game` (references Core and Runtime), `Kapman.Editor`, and `Kapman.Tests.EditMode`, which today references only `Kapman.Core`. New assembly `Kapman.Economy` at `Assets/Kapman/Code/Economy/` (plain C#, nothing from UnityEngine but `Mathf`; it still links UnityEngine, which is fine for EditMode tests): it references `Kapman.Core`; `Kapman.Runtime` adds a reference to `Kapman.Economy` for the clock and port hooks; `Kapman.Tests.EditMode` adds `Kapman.Economy` to its references or the tests will not compile. Data at `Assets/Kapman/Data/Economy/` (a new folder; the first build's JSON copied over from `G:\kapman_unity\Kapman\Assets\Kapman\Game\Data` and kept editable; read with Core's `Json`).

| Class | Does | From |
|---|---|---|
| `GoodsTable`, `PortsTable`, `TradeRules` | load `goods_793.json`, `ports_793.json` (+ the 119 of `ports_more.md` as `ports_more.json` when written), `trade_rules.json` | first build's `GameData` (199 lines) / `TradeRules` (167), ported |
| `Market`, `MarketGood`, `Deal`, `Purse`, `Hold`, `Lot`, `TradeResult` | the market and the dealing | first build's `Market` (265), `Deal` (129), `Purse` (180), `Hold` (106), `Trade` (200), ported; `Deal`'s calls to `Reputation.PriceFavour`, `Chest.TollFreeAt` and `KapmanGame.Current.Year` go behind one interface `IDealContext { double Favour(PortDef); bool TollFree(PortDef, int year); int Year; }` with a null context for tests |
| `WorldEconomy` | owns every port's `Market`, every region's silver and harvest, the lazy tick (2.3), `Capture`/`Restore` for saves, a deterministic state hash (FNV over every row in table order) | new |
| `Seasons` | the per-category make/use tables and the cheap/dear blending (precomputed at load) | new |
| `Silver` | the regional silver levels and their moves | new |
| `Chronicle` | reads `chronicle_793.json`, fires entries by day, applies `shocks_793.json` (by id, then by kind); posts `WeatherRequest`s; backfills markets, silver and port states when the start day is past | first build's `Chronicle.cs` (162 lines, the entry and date parsing), extended |
| `Shocks` | the id and kind tables | new |
| `Tolls` | toll stations, charters, the reeve's gift | new, on `Deal` |
| `Credit` | the count's loan, port credit, the partner's share | new |
| `Knowledge`, `PriceNote`, `PortBoard` | news and the ledger | first build, ported |
| `People` | roles, knowledge sets, options (3.2), on `GameEvents` | new; `GameEvents` ported |
| `Fairs`, `Commissions`, `Reputation` | built in the first build; ported | first build |
| `EconomyClock` (MonoBehaviour, `Kapman.Runtime`) | watches `LevelClock.Current.Date`; on a new day calls `WorldEconomy.Day(date)`; a date earlier than the last ticked day (Shift+T/Y/U move time backwards) is ignored, and the dev HUD says so; a dev field `setYear` jumps the economy (and `LevelClock.year`) to a year for testing | new; hooks `LevelClock` |
| `PortMarker` (MonoBehaviour) | one per place in a level: `portId`, `landingRadius`; placed by the builders beside the blockouts (`L_NorthSea` holds eleven places: Lundenwic, Londinium, Dover, Walichrum, Witla, Quentovic, Boulogne, Reculver, Richborough, Bradwell, the Rhine mouth; a river road is a chain of them) | new |
| `PortMarketLink` (MonoBehaviour) | finds the nearest `PortMarker` within its radius of `ShipMotion.Player` and exposes that `Market` to the UI and `TownLife`; `LevelInfo` gets no port field | new |
| `MarketPanel`, `LedgerTab`, `TalkPanel` (UI Toolkit, black glass, Gentium, per `kapman-ui-look`) | the panels of 3.2 | new; the first build's `MarketPanel` as a start |
| `TownLife` hook | reads the market's stock and state to pick dressing counts and smoke | small change in `TownLife` |

The AI traders (`WorldSim`, `SimShip`, `TraderBrain`, `Travel`, lanes) follow the AI-fleets plan into `Kapman.Economy.Sim`; they call `WorldEconomy`'s markets.

**Multiplayer (128 peers, later):** one host ticks `WorldEconomy` and the chronicle (or every peer ticks in lockstep by day from the shared seed); the host orders the day's deals, so two players selling into the same market get the glut in the host's order; the state hash (test 8) is the desync check at each day rollover.

### 4.2 Data files (all editable, all in `Assets/Kapman/Data/Economy/`)

`goods_793.json` (with `rhine_delta: 2` added to `rhenish_wine.made` and the elasticity per category), `ports_793.json`, `ports_more.json` (when written), `trade_rules.json` (toll stations, the reeve's gift, credit, the `tempo` block of 2.0), `chronicle_793.json`, `shocks_793.json` (new, the id and kind tables of 2.5), `seasons_793.json` (new, the make/use curves), `silver_793.json` (new, the pools and streams), `people_793.json` (new, 3.2), `events_793.json`, `fairs_793.json`, `commissions_793.json`, `knowing_793.json`, `reputation_793.json`, `wording_793.json`, `writing.json` keys for every panel (the user's words).

### 4.3 Tests (EditMode, headless, no scene; `Assets/Kapman/Tests/EditMode/EconomyTests.cs`, beside `SkyTests.cs`, `TideModelTests.cs` and `WorldFrameTests.cs`)

1. Loading: every good's made/wanted regions exist; every port's region exists; every chronicle `ports` id exists; every `shocks_793.json` id exists in the chronicle.
2. Price formula: stock below usual is dearer, above cheaper; bounds hold; the cheap and dear months blend over 20 days (no step larger than 2 % a day without a shock).
3. A deal of 10 units moves the price monotonically on buys and on sells and leaves stock, purse and hold consistent (the first build's `TradeTests`, ported); the worked example of 2.2 gives 10 casks for 540–580 d at Dorestad and 770–810 d at Lundenwic before toll.
4. Season: wheat at Parisius is cheapest in September and dearest in May in a plain year.
5. The chronicle: starting 21 June 793, the famine entry fires on day 1 (backstory) and wheat at Dorestad asks ≥ 1.4 × its plain-year June price; Lindisfarne is `disrupted` on 8 June 793 and open again by 18 June (10 days) or on the first visit; Reric's market closes in 808 and Hedeby's stock rises within 90 days; Dorestad's size is 0 by 870.
6. Silver: after 1018 England's level is below 0.8 and Denmark's above 1.3; the dirham stream is 0 after 1015.
7. News: a snapshot posted at Lundenwic reaches Dorestad's board within 3 game days with 300 AI ships (the AI plan's sim).
8. Determinism: seed 793 twice gives the same state hash after 365 days; seed 794 differs.
9. Performance: 50 ports × 140 goods × 100 years (36,500 days) with every port touched every 10 days completes in under 10 s; a full catch-up of all 50 ports for one day under 2 ms.
10. The start-year backfill: `WorldEconomy.StartAt(1016)` gives the same `WorldEconomy` hash (markets, silver, port states) as a fresh run of markets, silver and the chronicle from 793 to 1016; the AI fleet is outside the hash.
11. The clock backwards: `Day(date)` with a date before the last ticked day changes nothing and the hash is unchanged.

### 4.4 Performance (RTX 4070, 1440p, 60 fps)

The economy is CPU only. 450 ports × 140 goods = 63,000 `MarketGood` rows at about 48 bytes = 3 MB. The tick is lazy (2.3): at a day rollover only the ports the AI is heading to and the player's port are caught up, under 0.5 ms; a full catch-up of every port (a save, a start-year backfill step) is one pass over 63,000 rows with a table lookup, under 2 ms, once in 24 real minutes at most. Main thread only, no copies. The AI sim's hour tick (300 ships) is under 0.5 ms (the plan's 365 days in 60 s target). Budget: 1 ms a frame averaged, 4 ms worst on a day rollover, zero GPU. Saves: `Capture` as JSON, about 2 MB, written off the main thread.

---

## 5. First slice (two nights)

**Night one: the markets live in Kapman 2.** Create `Kapman.Economy` and `Assets/Kapman/Data/Economy/`; copy `goods_793.json`, `ports_793.json`, `trade_rules.json`, `chronicle_793.json`; port `GameData`'s goods/ports/regions loaders, `TradeRules`, `Market`, `MarketGood`, `Deal`, `Purse`, `Hold`, `Trade` and `Chronicle.cs`'s entry parsing as they are, with `Deal`'s three outside calls behind `IDealContext` and a null context; write `WorldEconomy.Day()` on the existing closed-form `CatchUp` (no production curves yet); `Chronicle` firing famine and raid only (famine = `Market.Shock` on food with dearer +1.0 for the span; raid = `Market.Shock` on everything, loot by kind, plus a `disruptedUntil` day on the market, which does not exist in the first build and is the one new field); a four-row `shocks_793.json` by id (793-great-famine, 793-06-08-lindisfarena-raid, 794-drought-harvest, 863-dorestad-last); `EconomyClock` on `L_Dorestad` ticking from `LevelClock` and ignoring backward days; a `DevHud` line that lists Dorestad's wine, wheat, querns and Badorf ware with asks, pays and stock. Add `Kapman.Economy` to the test assembly's references.

**Done when** (EditMode, headless): tests 1, 3, 5 (the 793 famine and Lindisfarne rows only), 8, 9 (50 ports × 100 years under 10 s) and 11 pass; and in Play on `L_Dorestad` the HUD line shows wheat dearer than 8 d on 21 June 793 (the famine) and the four goods' prices change when Y (a day on) is pressed.

**Night two: the year turns.** `Seasons` with the precomputed tables and the 20-day cosine edges; production curves in the tick (harvest, vintage, herring); `burning` and the `ruined` state; B to buy and N to sell a cask of wine for the dev purse through `Deal`; the dev `setYear` field on `EconomyClock` so 834 and 863 are reachable without 492 presses of U; `rhine_delta: 2` in the wine row. **Done when** tests 2 and 4 pass, and in Play pressing U (a month on) twelve times from 21 June 793 shows wheat dear in spring and cheap after harvest and wine cheap in November, and `setYear` 863 followed by U to March shows Dorestad's market refusing deals and its size falling by 864.

What is deliberately not in the slice: `ports_more.json` (not written yet; the 50 ports of `ports_793.json` are the slice), silver, the AI fleet, the panels, `People`, `WeatherRequest`.

---

## 6. Open questions for the user

0. **Waiting.** May the player choose to let days pass in port ("winter over", "wait for the fair", "wait ten days for news") with the panel ticking the days and the visible clock rate unchanged? Without it a 60-day closure is a real day of play, a winter three real days, and the season gap in prices (new wine in November, sold in June) is never played; with it the durations of 2.5 and 2.9 come back at full length. This is the one decision the tempo of the whole economy hangs on.
1. **Phases and the date.** May a phase at a level edge advance the date by the cut-out distance's sailing time (Dorestad–London: about 46 game hours in all instead of 30; a jump, not a rate change)? Then news lag, spoilage and the crew's pay follow the real passage.
2. **The debt's shape.** A fixed 480 d due in a year (as built), or the count takes half of every voyage's profit until two pounds are paid (a partner's share)? The first is a cleaner clock; the second is a longer leash.
3. **Ruin.** Two broken credits (or the count's day missed) ends the game in single player, like a wreck ashore. Or the count takes the ship and the merchant goes on as a hired man until he can buy a boat?
4. **Start years.** Offer 793 only at first, or the six of 3.3 from the start? (Backfill of the markets is cheap; the levels are the cost: each start year needs its ports' states dressed.)
5. **Dorestad's end.** The chronicle says 863 and the river did the rest. Let the player's own trade keep a dying port alive a few years longer (a visible reward, not true), or hold to the truth?
