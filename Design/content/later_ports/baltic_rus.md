# Ports the game lacks: the south and east Baltic and the Rus rivers, 793 to 1066

Research for `Assets/Kapman/Game/Data/ports_793.json`, 6 Oct 2026. The table was made for 793 and has six places on
this side of the sea: `reric` (gone 808), `truso`, `grobin`, `paviken`, `birka`, `aldeigjuborg`. Everything a merchant
would steer for here after about 850 is missing. This page gives 19 places with a ready row each, ranked, and seven
more that were looked at and left out.

**How it was checked.** Eleven Wikipedia pages (English and German) were read on 6 Oct 2026 for coordinates and
dates; they are listed under each place and at the end. No web search was made. Everything else is from memory of
the written sources and the excavation literature, and the project's own research in `Design/content/realms/`
(`wends_baltic.md`, `rus.md`) and `Design/content/peoples/` (`wendish.md`, `baltic_east.md`). `(?)` marks what was
not checked tonight: chapter numbers above all, and any coordinate marked so. Nothing is quoted; the wording is our
own.

**Coordinates** are for the landing, not the modern town centre, to three decimals. Those marked `(?)` were placed
from memory of the map and should be set against a map before the river bake uses them.

## The ranking

| Rank | Id | Name then | Modern place | Trades | Size |
|---|---|---|---|---|---|
| must have | `jumne` | Jumne | Wolin | 800 on | 5 |
| must have | `holmgardr` | Holmgardr | Novgorod | 860 on | 4 |
| must have | `koenugardr` | Koenugardr | Kiev (Kyiv) | 880 on | 5 |
| must have | `gnezdovo` | Miliniska (?) | Gnezdovo by Smolensk | 900 to 1020 | 3 |
| must have | `starigard` | Starigard | Oldenburg in Holstein | all years | 3 |
| must have | `ralswiek` | Ralswiek | Ralswiek, Rügen | to 1000 | 3 |
| must have | `menzlin` | (name lost) | Menzlin by Anklam | to 920 | 3 |
| must have | `kaup` | Kaup | Mokhovoye by Zelenogradsk | 850 to 1050 | 2 |
| must have | `gyddanyzc` | Gyddanyzc | Gdansk | 980 on | 2 |
| good to have | `stetin` | Stetin | Szczecin | 850 on | 3 |
| good to have | `cholberg` | Cholberg | Budzistowo by Kolobrzeg | 880 on | 2 |
| good to have | `michelenburg` | Michelenburg | Dorf Mecklenburg | 810 on | 2 |
| good to have | `liubice` | Liubice | Alt Lübeck | 1055 on | 2 |
| good to have | `daugmale` | Daugmale | Daugmale by Riga | 900 on | 2 |
| good to have | `eysysla` | Eysysla | Saaremaa | all years | 1 |
| good to have | `palteskja` | Palteskja | Polotsk | 860 on | 3 |
| good to have | `arkona` | Arkona | Cape Arkona, Rügen | all years | 1 |
| good to have | `dierkow` | (name lost) | Rostock-Dierkow | to 850 | 2 |
| good to have | `pleskov` | Pleskov | Pskov | 860 on | 2 |
| leave out | | Kamien, Usedom, Puck, Palanga and Apuole, Iru, Lyubech and Chernigov, Beloozero and Timerevo | | | |

**Why this order.** The must-haves are the places without which the chart is wrong for a whole century: Wolin is
the largest town on the Baltic from about 950; Novgorod and Kiev are the two ends of the river road the design
names ("on to Kiev"); Gnezdovo is the carry between the rivers, without which there is no road; Starigard, Ralswiek
and Menzlin are where the Wendish trade goes when Reric is broken in 808 (the table leaves this shore empty from 808
to 1066); Kaup and Gdansk are the amber coast and the Vistula mouth once Truso is gone. The good-to-haves each add
one thing: salt (Cholberg), a bishop and a slave market (Michelenburg), a three-headed god (Stetin), the Dvina road
(Daugmale, Palteskja), sea robbers (Eysysla), a temple that takes a tithe from merchants (Arkona).

## What the spine must add before these rows load

The rows below use ids that are not in the shared tables yet. They are listed here, not added.

- **Realms.** `wilzi` and `pomeranians` are already in `realms/wends_baltic.json` but have no port. New: `rani` (the
  people of Rügen), `poles` (the Piast dukes, from about 960), `livs` (the lower Dvina), `osilians` (the men of
  Saaremaa), `polotsk` (its own princes from about 960; or fold it into `rus`).
- **Regions** (for `goods_793.json`). `wendland`, `prussia` and `ladoga` exist. New: `pomerania` (Oder to Vistula:
  salt, herring, amber, honey, captives; wants cloth, silver, querns, blades), `dvina` (Daugmale and Palteskja: wax,
  furs, captives; wants salt, silver, cloth, weapons), `estland` (Saaremaa: iron, captives, seal goods; wants salt,
  weapons, silver), `ilmen` (Novgorod and Pskov: squirrel, marten, sable, beaver, wax, honey, walrus ivory from the
  north; wants silver, cloth, wine, blades, salt, beads; or reuse `ladoga`), `dnieper` (Gnezdovo and Kiev: the same
  northern goods gathered as tribute, and coming up from the Greeks silk, wine, pepper, glass, oil; wants silver,
  blades, captives, amber, walrus ivory). `realms/rus.md` point 8 already asks for a Dnieper region.
- **One row to look at again.** `truso` ends in 850. The excavator (Jagodzinski) carries it into the 10th century
  (?); the latest coins are of about 850 but the place was not empty at once. If it stays at 850, the Vistula mouth
  has no port from 850 to 980. Consider `"to": 950`.
- **A rule for the rivers.** A sea ship does not go up the Volkhov rapids or over the carries. A Gislinge boat
  (7.7 m, four men) does: it is exactly the size that was dragged. For any larger class the three inland Rus ports
  should need a change of vessel at Aldeigjuborg (`river_barge` or `logboat_expanded`) and a pilot (`act:hire_pilot`).
- **Ice.** Every port from the Vistula east is shut from about Martinmas to late April; Ladoga and the Volkhov
  longer (`realms/rus.md`: "the north is ice to late April"). Kiev's boats leave in June (De Administrando Imperio 9).

## Which ports are open, by half-century

| Years | The Wendish shore | Oder and Pomerania | Amber coast and east | The rivers |
|---|---|---|---|---|
| 793-808 | Reric, Starigard, Dierkow, Menzlin, Ralswiek | (Jumne begins) | Truso, Grobin | Aldeigjuborg |
| 808-850 | Starigard, Michelenburg, Dierkow, Menzlin, Ralswiek | Jumne | Truso, Grobin | Aldeigjuborg |
| 850-900 | Starigard, Michelenburg, Menzlin, Ralswiek | Jumne, Stetin, Cholberg | Kaup | Aldeigjuborg, Holmgardr, Palteskja, Pleskov, Koenugardr |
| 900-950 | Starigard, Michelenburg, Ralswiek | Jumne, Stetin, Cholberg | Kaup, Daugmale | all, with Gnezdovo |
| 950-1000 | Starigard, Michelenburg, (Ralswiek fading) | Jumne at its height, Stetin, Cholberg, Gyddanyzc | Kaup, Daugmale | all |
| 1000-1066 | Starigard, Michelenburg, Liubice from 1055 | Jumne (burned 1043), Stetin, Cholberg, Gyddanyzc | Kaup to 1050, Daugmale | all but Gnezdovo after 1020 |

---

# MUST HAVE

## 1. Jumne (Wolin)

- **Name then:** Jumne (Adam of Bremen, about 1075); Julin (the lives of Otto of Bamberg, 1124); the people are the
  Vuloini (Widukind). "Jomsborg" is the sagas' name, two centuries late: do not use it. "Vineta" is a later
  misreading: do not use it.
- **Modern place:** Wolin, West Pomerania, Poland.
- **Lat, lon:** 53.843, 14.613.
- **Active:** a settlement from about 800; walls in the first half of the 9th century; a town with a harbour front
  from about 900; its height about 950 to 1040; much of it burned in 1043; smaller after, still a town in 1124.
- **Kind:** emporium; the largest town on the Baltic in the 10th century. A town ruled by its elders, with no king.
- **Who ruled:**
  - 800 to 967: its own elders; the Wolinians, a people of their own among the Wilzi and Pomeranians.
  - 967 to about 1005: the same elders, paying the Polish duke (Mieszko beat the Wolinians and the Saxon exile
    Wichmann in 967; Widukind III.69 (?)). The page read tonight says the Poles never fully held it.
  - about 1005 to 1066: free again, heathen, and a nest of sea raiders by the 1040s.
- **Traded:** out: captives, amber and amber beads, salt herring, honey, wax, furs, combs, boats. In: silver
  (dirhams to about 970, German and English pennies after), cloth, querns, Rhenish pots and glass, blades, soapstone,
  beads. Adam says it is full of the goods of all the northern peoples.
- **How a boat comes in:** not from the open sea. Wolin lies on the east shore of its island, on the Dziwna, the
  eastern arm of the Oder mouth, at the old crossing. Come in by the Dziwna from the north-east (narrow, shallow,
  20 km of channel), or by the Swina or the Peene arm into the lagoon and round. Timber-faced quays along the bank;
  she lies at a jetty. Adam speaks of a fire kept there for seamen, which he calls Vulcan's pot: a beacon (?).
- **Why go:** everything is here and everyone: Wends, Saxons, Northmen, Rus and (Adam says) Greeks. Fourteen days
  under sail to Novgorod, seven by land to Hamburg (Adam II.22 (?)). No king's toll before 967. A Christian may
  trade and lodge if he keeps his cross out of sight.
- **Evidence:** Ibrahim ibn Yaqub, 965: a great town by the ocean with twelve gates and a harbour, which fights
  Mieszko. Widukind III.69, 967. Adam II.22 and II.27-28 (Harald Bluetooth dies here, wounded, about 986). Adam,
  scholion 56 (?): Magnus besieges and burns it, 1043. Excavations from 1934 and 1952 on (Filipowiak): harbour
  fronts, a temple, craft quarters for amber, antler and glass, wrecks, a small four-faced wooden god.
- **Sources:** en.wikipedia.org/wiki/Wolin_(town), read 6 Oct 2026; Adam of Bremen, Gesta II.22; Widukind, Res
  gestae III.69; W. Filipowiak and H. Gundlach, Wolin Vineta (1992); `realms/wends_baltic.md`.

```json
{"id": "jumne", "name": "Jumne", "modern": "Wolin", "lat": 53.843, "lon": 14.613, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "emporium", "landing": "jetty", "size": 5, "from": 800, "to": 0, "note": "The Wolinians' town on the Dziwna arm of the Oder mouth, ruled by its elders: the greatest market of the Baltic about 950 to 1040; the Polish duke takes tribute 967 to about 1005 (?); Magnus burns much of it in 1043."}
```

## 2. Holmgardr (Novgorod)

- **Name then:** Holmgardr to Northmen; Novgorod ("new fort") to its own people; Ostrogard to Adam of Bremen;
  Nemogardas in the Greek emperor's book (about 950).
- **Modern place:** Veliky Novgorod, Russia. The first seat is Ryurikovo Gorodishche, 2 km south, where the Volkhov
  leaves Lake Ilmen; the town itself grows on both banks below it from the 930s to 950s.
- **Lat, lon:** 58.521, 31.275 (the town, at the bridge); the old fort is at about 58.493, 31.298 (?).
- **Active:** the fort from the mid 9th century (about 860); the town from about 930 to 950 (the oldest street
  timbers are of the 950s (?)); in full trade to 1066 and long after.
- **Kind:** town and prince's seat. Until about 950 a fort with a craft quarter; after, a town of timber streets,
  with a market on the east bank beside the prince's court.
- **Who ruled:**
  - about 860 to 882: Northmen who take tribute; Rurik in the Chronicle's telling (862 (?), 879 (?)).
  - 882 to 1066: the prince of Kiev's son or governor. From 882 to 1054 the town pays 300 grivnas a year to the
    Varangians "for peace" (Primary Chronicle; `realms/rus.md`).
  - about 1010 to 1019: Yaroslav sits here; 1014 he stops the tribute to Kiev; 1015 the townsmen kill his
    Varangians; 1016 he gives them the law; 1018 they break his boats so that he cannot flee.
  - 1021: sacked by Polotsk. Winter 1066-1067: burned by Vseslav of Polotsk, the bells of Holy Wisdom carried off.
- **Traded:** out: squirrel in thousands, marten, sable, beaver, wax, honey, captives, walrus ivory from the far
  north. In: silver, Frisian and English cloth, wine, sword blades, salt, glass beads, amber.
- **How a boat comes in:** by Aldeigjuborg. From there about 200 km up the Volkhov, with two runs of rapids where
  cargo is carried and boats are hauled or changed (pilots at the rapids). A small boat goes through; a sea ship
  stays at Aldeigjuborg. Riverbank landing below the market.
- **Why go:** the fur is cheaper here than at Aldeigjuborg, and cheaper again than at Birka. Hired fighting men are
  wanted in 977-980 and 1015-1019. From 989 there is a bishop; from 1030 three hundred children are set to books.
- **Evidence:** the Primary Chronicle (862, 882, 989, 1016, 1018, 1021, 1066); De Administrando Imperio 9; Adam
  II.22; the excavations (from 1932), with letters on birch bark from the 1030s on and a wax-tablet psalter of about
  1000 to 1030; Gorodishche (Nosov): Scandinavian finds of the 9th and 10th centuries, dirhams, scales.
- **Sources:** en.wikipedia.org/wiki/Rurikovo_Gorodische, read 6 Oct 2026; `realms/rus.md` and `rus.json`; E. N.
  Nosov, Novgorodskoe (Ryurikovo) gorodishche (1990); the Primary Chronicle (Cross's translation is NOT public
  domain: do not quote it).

```json
{"id": "holmgardr", "name": "Holmgardr", "modern": "Veliky Novgorod", "lat": 58.521, "lon": 31.275, "realm": "rus", "county": "", "region": "ilmen", "kind": "town", "landing": "riverbank", "size": 4, "from": 860, "to": 0, "note": "The fort where the Volkhov leaves Lake Ilmen, and from about 950 the new town below it: furs and wax for silver. Two hundred kilometres and two runs of rapids above Aldeigjuborg; a small boat only."}
```

## 3. Koenugardr (Kiev)

- **Name then:** Koenugardr to Northmen (the form is from later texts (?)); Kyiv to its people; Kioava, "also
  called Sambatas", in the Greek emperor's book; Chive to Adam; Cuiewa to Thietmar.
- **Modern place:** Kyiv, Ukraine. The harbour is the Podil, the low town under the hill, where the Pochaina ran
  into the Dnieper.
- **Lat, lon:** 50.463, 30.517.
- **Active:** hill forts from before 800, paying the Khazars; the Podil is built up from the 880s (timbers of 887
  (?)); the chief town of the Rus from 882 (?) to 1066 and after.
- **Kind:** royal seat and the largest town on the game's chart east of the Rhine by 1018.
- **Who ruled:** the Khazars' tribute to about 880; Oleg from 882 (?); Igor to 945; Olga to about 962; Sviatoslav
  to 972; the brothers' war 977-980; Vladimir 980-1015 (baptised about 988); war 1015-1019; Boleslaw of Poland in
  the town 14 August 1018; Yaroslav 1019-1054; Iziaslav from 1054; the market rising of 15 September 1068 is just
  past the game's end.
- **Traded:** the winter's tribute, sold: furs, wax, honey, captives. Bought with it in Constantinople and sold
  here: silk, wine, oil, pepper, glass, fruit. A northern skipper brings silver, blades, amber, walrus ivory and
  fighting men.
- **How a boat comes in:** down the Dnieper from the carry at Gnezdovo, about 800 km (?) of easy river. Boats lie
  on the bank under the hill. Below Kiev are the seven rapids and the Pechenegs: the prince's convoy leaves in June
  and waits for nobody.
- **Why go:** silk and pepper at a tenth of the Rhine price (the game's own price tables decide the figure); the
  treaties of 911 and 944 that let Rus merchants into Constantinople; the pay for hired men in 978 and 1018.
- **Evidence:** De Administrando Imperio 9 (the boats, the yearly round); the Primary Chronicle; the treaties of
  907 (?), 911 and 944 in it; Thietmar VIII.32 (?): in 1018 over 400 churches, eight markets and "swift Danes"
  among its defenders; Adam II.22: the rival of Constantinople; the Hebrew "Kievan letter" of about 930; the Podil
  excavations of 1972 on (timber houses under metres of river silt).
- **Sources:** `realms/rus.md` and `rus.json`; Thietmar, Chronicon VIII.32; Constantine VII, De Administrando
  Imperio ch. 9; M. Sahaidak on the Podil (1991). Not checked on the web tonight.

```json
{"id": "koenugardr", "name": "Koenugardr", "modern": "Kyiv (the Podil)", "lat": 50.463, "lon": 30.517, "realm": "rus", "county": "", "region": "dnieper", "kind": "royal seat", "landing": "riverbank", "size": 5, "from": 880, "to": 0, "note": "The prince's town on the hill and the boat harbour under it: the winter's tribute of furs, wax and captives goes down to the Greeks each June and silk and wine come back. In 1018 a Saxon bishop hears of eight markets here (?)."}
```

## 4. Gnezdovo (the carry by Smolensk)

- **Name then:** not known. "Gnezdovo" is a village name first written in the 17th century. The Greek emperor's
  book (about 950) names Miliniska, which is Smolensk, as a place the boats come from, and most scholars take that
  to be this site (?). A Norse name Syrnes has been proposed (?).
- **Modern place:** Gnezdovo, 13 km west of Smolensk, Russia, where the Svinets runs into the Dnieper.
- **Lat, lon:** 54.785, 31.879.
- **Active:** about 900 (some say the late 9th century) to the early 11th century; then Smolensk takes over.
- **Kind:** emporium at a carry: a fort, an open settlement of about 17.5 ha, and the largest field of grave
  mounds of its age in Europe (about 3,000 mounds standing; more once).
- **Who ruled:** the prince of Kiev's men, who gather tribute here; many of the dead are Northmen and their
  households. No ruler is named.
- **Traded:** boats and boat work above all: dugouts built and sold, hulls mended, rivets by the thousand.
  Captives, furs, wax. Silver (seven hoards (?)), Greek wine in jars, silk, glass. Smiths and jewellers.
- **How a boat comes in:** from the Baltic up the Dvina to the Kasplya, up that to its lake, then a carry of some
  30 km (?) on rollers and carts to the Dnieper here. Or from Holmgardr up the Lovat and over two carries. Men and
  oxen for the carry are hired at both ends.
- **Why go:** you cannot reach Kiev without passing it. A worn boat can be sold and a river boat bought. The
  cheapest place to hire men who know the lower river.
- **Evidence:** the mounds (1,300 dug since 1874), with ship rivets, swords, Thor's hammers and scales; a Greek jar
  from a mound of the early 10th century scratched with the oldest words written in the Rus tongue (one word,
  thought to mean mustard or the owner's name); dirhams and Greek coins; De Administrando Imperio 9.
- **Sources:** en.wikipedia.org/wiki/Gnezdovo, read 6 Oct 2026; V. V. Murasheva and T. A. Pushkina's excavation
  reports (State Historical Museum, Moscow); D. A. Avdusin (1949-1980s).

```json
{"id": "gnezdovo", "name": "Miliniska (?)", "modern": "Gnezdovo, by Smolensk", "lat": 54.785, "lon": 31.879, "realm": "rus", "county": "", "region": "dnieper", "kind": "emporium", "landing": "riverbank", "size": 3, "from": 900, "to": 1020, "note": "The carry between the Dvina and the Dnieper: boatwrights, smiths and three thousand grave mounds. Boats are dragged here from the Kasplya (?); its own name is lost."}
```

## 5. Starigard (Oldenburg in Holstein)

- **Name then:** Starigard, "the old fort", in Wendish; Aldinburg to Adam; in Danish Brandehuse (Helmold; 12th
  century (?)).
- **Modern place:** Oldenburg in Holstein, Germany: the great earthwork in the town.
- **Lat, lon:** 54.293, 10.886 (?).
- **Active:** a ring fort from about 680-700; enlarged about 750; a prince's hall in the 9th century; a bishop from
  968 (or 972); held all through the game and to 1149.
- **Kind:** royal seat with a harbour: the chief place of the Wagrians, the westernmost Obodrites.
- **Who ruled:** Wagrian princes under the Obodrite king (Thrasco, Sclaomir, Ceadrag and the rest are in
  `wends_baltic.json`); tribute to the Saxon duke from 929; the bishops Egward, Wago and their heirs 968-983; the
  rising of 983 and again 990 and 1018 throw the church out; Gottschalk 1043-1066 brings it back; 7 June 1066 it
  falls again (Adam III.50 (?)).
- **Traded:** out: captives, honey, wax, horses, furs, grain. In: Frankish blades (forbidden by the king's law of
  805, and found here all the same), Tating jugs, querns, glass, silver, cloth. A prince's court buys falcons and
  fine things.
- **How a boat comes in:** by the Oldenburger Graben, then a long shallow sound that ran through the neck of
  Wagria and was open to the sea at both ends (west by Weissenhaus, east by Dahme (?)). The harbour lay under the
  wall. Adam calls it a town on the sea. A small boat only; pilot wanted.
- **Why go:** the nearest Wendish market to Sliesthorp, a day and a half by sea. The only place west of the Oder
  where a merchant deals with a prince's steward and not a town's elders. After 808 it is where the Obodrite trade
  that was Reric's partly goes.
- **Evidence:** the excavations of 1953 to 1986 (Struve, Gabriel): a timber hall on the Frankish pattern, burials
  with swords, Tating ware, scales, a church under the later ones; Adam II.18 (?) and Helmold I.12 (?); the charter
  evidence for the see (968).
- **Sources:** de.wikipedia.org/wiki/Oldenburger_Wall, read 6 Oct 2026; I. Gabriel, Starigard/Oldenburg (1984,
  1988); M. Müller-Wille (ed.), Starigard/Oldenburg: ein slawischer Herrschersitz (1991).

```json
{"id": "starigard", "name": "Starigard", "modern": "Oldenburg in Holstein", "lat": 54.293, "lon": 10.886, "realm": "obodrites", "county": "", "region": "wendland", "kind": "royal seat", "landing": "beach", "size": 3, "from": 0, "to": 0, "note": "The Wagrian prince's fort and hall on a shallow sound across the neck of Wagria (?); a bishop from 968, thrown out in 983, 1018 and 1066."}
```

## 6. Ralswiek (Rügen)

- **Name then:** not recorded before 1240 (Ralswiek: the wik of Ral (?)). Use the modern name.
- **Modern place:** Ralswiek, Rügen, Germany, at the south end of the Great Jasmund Bodden.
- **Lat, lon:** 54.474, 13.449 (the page read tonight gives 54.467, 13.450 for the parish).
- **Active:** from the late 8th century; at its best in the 9th; fading in the 10th; a small landing after.
- **Kind:** emporium. Fifteen to twenty homesteads in a row along a spit, each with a house, a workshop and its own
  boat berth cut into the shore (?); a place of offering at the water's edge; over 400 grave mounds on the hill
  behind.
- **Who ruled:** the Rani (Rugini) of Rügen, under their own lords and the priest of Arkona. No ruler is named in
  these years. Never under Frank or Dane before 1168.
- **Traded:** out: salt herring, grain, captives, amber, horses. In: dirhams (the largest hoard of the south coast
  in its time), Norwegian whetstones and soapstone, Rhenish querns, glass, beads.
- **How a boat comes in:** from the north-west, past Hiddensee and through the narrows into the boddens, then 15 km
  of sheltered shallow water to the head of the bay. Each house has its berth; a stranger beaches at the common
  shore. No sea swell ever reaches it.
- **Why go:** the safest harbour on the south coast, a people who keep a guest's peace, and eastern silver earlier
  than anywhere west of the Oder.
- **Evidence:** the excavations of 1972 to 1989 (Herrmann): four clinker boats up to 14 m by 3.4 m, pegged with
  wooden nails; a hoard of 2,203 dirhams and pieces, the youngest struck in the 840s, in a basket in a house; the
  berths; the mounds, some with Scandinavian burnings in boats.
- **Sources:** en.wikipedia.org/wiki/Ralswiek, read 6 Oct 2026 (coordinates only); J. Herrmann, Ralswiek auf
  Rügen, vols. 1-5 (1997-2008); `peoples/wendish.md` [Ral].

```json
{"id": "ralswiek", "name": "Ralswiek", "modern": "Ralswiek, Rügen", "lat": 54.474, "lon": 13.449, "realm": "rani", "county": "", "region": "wendland", "kind": "emporium", "landing": "beach", "size": 3, "from": 0, "to": 1000, "note": "The Rani's market at the head of the Jasmund Bodden: a row of homesteads, each with its own boat berth (?); herring and captives for eastern silver. Four boats and 2,203 dirhams were found here."}
```

## 7. Menzlin (the Peene crossing)

- **Name then:** lost. The field is called Altes Lager, after a camp of 1676. Use "Menzlin" and say the name is not
  known.
- **Modern place:** 1.5 km south of Menzlin, near Anklam, Germany, on the north bank of the Peene.
- **Lat, lon:** 53.857, 13.640 (?).
- **Active:** about 770 to about 900 or a little after (?).
- **Kind:** emporium: about 18 ha on a sand ridge, where a road crosses the river on a timber bridge and causeway.
  Wends and Northmen live side by side.
- **Who ruled:** the Wilzi (the people here are later called Tollensians and Circipanians). Dragovit is their king
  in 789 when Charlemagne marches to the Peene; Liub, then Milegast, then Cealadrag about 810-823; after 844 no king
  is named (`wends_baltic.json`).
- **Traded:** out: captives, honey, wax, furs, grain, combs, amber. In: Rhenish querns, Frankish glass and pots,
  Norwegian whetstones, beads, Irish metalwork, silver. Smiths, comb makers and bead makers work here.
- **How a boat comes in:** from the sea into the Peene arm past Usedom, then about 10 km (?) up the Peene, a slow
  deep river with no fall. She lies at the bank by the bridge. The same river goes on 80 km inland to Demmin.
- **Why go:** a river port with a road: the goods of the whole Wilzi country come down to the bridge. Northmen's
  wives are buried here, so there are households to lodge with, not only a summer beach.
- **Evidence:** the excavations of 1965 to 1969 (Schoknecht) and later harbour work: eight stone ship settings and
  twelve stone circles over burnings, most of them women's with Scandinavian brooches; the bridge timbers; imports
  from Ireland to the east Baltic.
- **Sources:** de.wikipedia.org/wiki/Altes_Lager_(Menzlin), read 6 Oct 2026; U. Schoknecht, Menzlin: ein
  frühgeschichtlicher Handelsplatz an der Peene (1977); H. Jöns on the harbour and bridge (?).

```json
{"id": "menzlin", "name": "Menzlin", "modern": "Altes Lager, Menzlin by Anklam", "lat": 53.857, "lon": 13.64, "realm": "wilzi", "county": "", "region": "wendland", "kind": "emporium", "landing": "riverbank", "size": 3, "from": 0, "to": 920, "note": "The Wilzi's market at the Peene bridge, Wends and Northmen together; its own name is lost. Graves set with stones in the shape of ships."}
```

## 8. Kaup (Wiskiauten)

- **Name then:** not known. "Kaup" is the name of the wood where the mounds lie and means a market (?). Wiskiauten
  is the Prussian village name. Use Kaup.
- **Modern place:** Mokhovoye, 3 km south of Zelenogradsk (Cranz), Kaliningrad region, Russia, at the foot of the
  Curonian Spit.
- **Lat, lon:** 54.930, 20.490 (?).
- **Active:** Northmen's graves from about 850 to about 1050; the Prussian villages around it are older and go on.
- **Kind:** market: several hamlets and over 500 grave mounds. Smaller than Truso was, and the only place with
  Northmen settled in Sambia.
- **Who ruled:** the Prussians of Sambia (Adam's Sembi), with no king: a lord in every fort (Wulfstan). Danish
  kings claim it from time to time: Harald Bluetooth's son Hakon is said to have taken Sambia (Saxo, late (?)), and
  Cnut counts it his (?).
- **Traded:** amber, raw and worked: the richest beach in the world is 30 km west. Also furs, captives, horses,
  honey. In: silver, blades, cloth, salt, iron.
- **How a boat comes in:** a gap in the spit near Cranz then let boats from the sea into the lagoon (?); the
  landing is on the lagoon side, shallow and sheltered. By the sea coast she must be beached on open sand, in fair
  weather only.
- **Why go:** amber at the source, a third of its Dorestad price (the game's tables decide the figure). After Truso
  fades it is the one place on this coast where a stranger has a host. Adam says the Prussians come out to help
  ships in trouble and care nothing for gold or furs.
- **Evidence:** the mounds, dug from 1865: swords, spears, oval brooches, dirhams, scales; the settlement found by
  survey in 2005 to 2011 (Ibsen): houses of the 5th century on, with trade goods of the 9th to 11th. Adam IV.18.
- **Sources:** de.wikipedia.org/wiki/Wiskiauten, read 6 Oct 2026; T. Ibsen, Etwa hier die Siedlung (dissertation,
  Kiel 2009); B. von zur Mühlen, Die Kultur der Wikinger in Ostpreussen (1975).

```json
{"id": "kaup", "name": "Kaup", "modern": "Mokhovoye (Wiskiauten), by Zelenogradsk", "lat": 54.93, "lon": 20.49, "realm": "prussians", "county": "", "region": "prussia", "kind": "market", "landing": "beach", "size": 2, "from": 850, "to": 1050, "note": "Northmen settled among the Prussians of Sambia at the foot of the long spit: amber at the source. Five hundred grave mounds in a wood; its own name is lost (?)."}
```

## 9. Gyddanyzc (Gdansk)

- **Name then:** Gyddanyzc (the first Life of Adalbert, written about 999).
- **Modern place:** Gdansk, Poland, on the Motlawa just above where it joins the Vistula's western arm.
- **Lat, lon:** 54.354, 18.660.
- **Active:** a settlement from about 930 (timbers); a ducal fort and harbour from about 980 by the old dating, or
  only from the 1060s by the newer tree-ring dating (the fort and suburb of 2.7 ha). A town called "urbs" in 997
  either way.
- **Kind:** town: a fort of the Polish duke with fishermen, amber workers and a landing.
- **Who ruled:** Pomeranians; under Mieszko of the Poles from about 970-980 and Boleslaw to about 1005-1030 (?);
  after that Pomeranian lords of their own, sometimes paying the Poles.
- **Traded:** out: amber, salt herring and sturgeon, grain and potash down the Vistula, furs, captives taken from
  the Prussians. In: silver pennies, cloth, salt, querns, blades.
- **How a boat comes in:** into the bay, into the Vistula's western mouth, and a short way up the Motlawa to the
  bank under the fort. Sheltered; the bar at the river mouth shifts.
- **Why go:** the Vistula is the road into Poland, and from 980 there is a duke who wants what ships bring. The
  place to take a passenger for Prussia: in spring 997 Bishop Adalbert preached here, baptised many, and took ship
  with a guard of the duke's men; the Prussians killed him on 23 April.
- **Evidence:** John Canaparius (?), Vita sancti Adalberti, ch. 27 (?); the excavations of 1948 on under the fort
  and of the 1980s under the town hall; the tree-ring redating (B. Kosciński and H. Paner).
- **Sources:** en.wikipedia.org/wiki/History_of_Gdańsk, read 6 Oct 2026; `peoples/baltic_east.md` [Can].

```json
{"id": "gyddanyzc", "name": "Gyddanyzc", "modern": "Gdańsk", "lat": 54.354, "lon": 18.66, "realm": "poles", "county": "", "region": "pomerania", "kind": "town", "landing": "riverbank", "size": 2, "from": 980, "to": 0, "note": "The Polish duke's fort at the Vistula mouth (the date of the fort is disputed: about 980 or the 1060s). Bishop Adalbert takes ship here for Prussia in 997."}
```

---

# GOOD TO HAVE

## 10. Stetin (Szczecin)

- **Name then:** Stetin (the lives of Otto of Bamberg, 1124-1128). No earlier writing names it.
- **Modern place:** Szczecin, Poland: the castle hill on the west bank of the Oder.
- **Lat, lon:** 53.426, 14.560.
- **Active:** a fort from the 8th century; a craft and harbour suburb along the river from the 9th and 10th; the
  chief town of Pomerania by 1100.
- **Kind:** town with temples.
- **Who ruled:** its own elders and great families; Polish tribute about 967 to 1005 (?); free and heathen until
  1121. In 1124 one rich man, Domaslav, has a household of 500 (Herbord (?)).
- **Traded:** out: grain down the Oder, salt fish, honey, wax, captives. In: cloth, silver, salt, wine.
- **How a boat comes in:** through the lagoon and 30 km up the Oder's western arm; she lies at the bank under the
  hill. Markets twice a week (Herbord II.26, of 1124: used for our years with a (?)).
- **Why go:** Jumne's rival and its heir after 1043; the temple of Triglav, a god with three heads whose eyes and
  mouth are covered, with a treasury where a tenth of all plunder is laid up, and a black horse kept for omens.
- **Evidence:** Ebo and Herbord, lives of Otto (900 heads of households in 1124); the excavations on the castle
  hill and in the Podzamcze (Leciejewicz, Wilgocki).
- **Sources:** en.wikipedia.org/wiki/Szczecin, read 6 Oct 2026; `peoples/wendish.md` [Ebo, Herb].

```json
{"id": "stetin", "name": "Stetin", "modern": "Szczecin", "lat": 53.426, "lon": 14.56, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "town", "landing": "riverbank", "size": 3, "from": 850, "to": 0, "note": "The fort on the hill above the Oder, with the temple of three-headed Triglav; Jumne's rival, and after 1043 its heir. Markets twice a week (said of 1124 (?))."}
```

## 11. Cholberg (Kolobrzeg)

- **Name then:** Salsa Cholbergiensis, "salt Cholberg" (Thietmar, about 1015).
- **Modern place:** Budzistowo, 2 km south of Kolobrzeg, Poland, on the Parseta. (The town at the river mouth is of
  1255.)
- **Lat, lon:** 54.157, 15.587 (?).
- **Active:** salt boiling from the 7th century; the fort from the late 9th century; a bishop 1000 to about 1007.
- **Kind:** fort and salt works.
- **Who ruled:** Pomeranians; the Polish duke about 967 to 1005; Pomeranians again after the rising of 1005.
- **Traded:** out: salt boiled from brine springs on an island in the river, salt herring, combs, iron. In: silver,
  cloth, grain in a bad year. Northmen were settled 13 km upriver at Swielubie in the 9th century (their graves).
- **How a boat comes in:** over the bar at the Parseta mouth and 4 km (?) up a small river to the fort. Open coast:
  no shelter outside.
- **Why go:** the only salt on the Baltic's south shore east of Lüneburg, where every herring needs it. And the
  bishop: Reinbern (1000 to about 1007) burned the shrines and threw four stones anointed with oil into the sea to
  drive the devils out of it (Thietmar VII.72 (?)).
- **Evidence:** Thietmar IV.45 (?) and VII.72 (?); the excavations at Budzistowo and on the Salt Island
  (Leciejewicz); the graves at Swielubie (Losinski).
- **Sources:** en.wikipedia.org/wiki/Kołobrzeg, read 6 Oct 2026.

```json
{"id": "cholberg", "name": "Cholberg", "modern": "Budzistowo, by Kołobrzeg", "lat": 54.157, "lon": 15.587, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "fort", "landing": "riverbank", "size": 2, "from": 880, "to": 0, "note": "Salt Cholberg: brine springs and a fort a little way up the Parsęta. A bishop from 1000 to about 1007, who throws anointed stones into the sea to cleanse it (?)."}
```

## 12. Michelenburg (Mecklenburg)

- **Name then:** Michelenburg, "the great fort" (a charter of Otto III, 995); in Wendish perhaps Veligrad (?);
  "Nakon's fort" to Ibrahim ibn Yaqub (965).
- **Modern place:** Dorf Mecklenburg, 6 km south of Wismar, Germany. Reric lay 10 km north on the same bay.
- **Lat, lon:** 53.840, 11.467 (?).
- **Active:** a fort from the 7th or 8th century (?); the Obodrite king's chief seat from the 9th century to 1160.
- **Kind:** royal seat. Its harbour is the head of Wismar bay.
- **Who ruled:** the Obodrite kings: Nakon about 955-966, Mstivoj to about 990, Mstislav, Udo (killed 1028),
  Gottschalk 1043-1066; a bishop (John the Irishman) from about 1055 to 1066, when he is carried off and killed.
- **Traded:** out: captives above all, horses (Ibrahim says Nakon's country is rich in them), honey, wax, grain.
  In: silver, cloth, blades, salt.
- **How a boat comes in:** into Wismar bay to its head, then a walk or a punt up a stream: the fort stands in a
  marsh of fresh water (Ibrahim). She lies on the bay shore.
- **Why go:** when Reric is gone the bay still has a king. A merchant who has his word is safe from the Trave to
  the Warnow.
- **Evidence:** Ibrahim ibn Yaqub (965); the charter of 995; Adam III.20 (?), III.51 (?); Helmold; the excavations
  of 1973 to 1980 (Donat).
- **Sources:** P. Donat, Die Mecklenburg, eine Hauptburg der Obodriten (1984); `realms/wends_baltic.md`. Not
  checked on the web tonight.

```json
{"id": "michelenburg", "name": "Michelenburg", "modern": "Dorf Mecklenburg, by Wismar", "lat": 53.84, "lon": 11.467, "realm": "obodrites", "county": "", "region": "wendland", "kind": "royal seat", "landing": "beach", "size": 2, "from": 810, "to": 0, "note": "The Obodrite king's great fort in a marsh at the head of Wismar bay, where Reric was: captives and horses. A bishop about 1055 to 1066 (?)."}
```

## 13. Liubice (Alt Lübeck)

- **Name then:** Liubice (Adam; Helmold).
- **Modern place:** Alt Lübeck, where the Schwartau runs into the Trave, 6 km below Lübeck, Germany.
- **Lat, lon:** 53.908, 10.713 (?).
- **Active:** a fort of 819 (tree rings), almost empty, left about 900; settled again about 1000; rebuilt by
  Gottschalk in 1055; at its height only from 1093, after the game; destroyed 1138.
- **Kind:** fort with a landing; in the game's years small.
- **Who ruled:** Obodrite princes: Gottschalk 1043 to 1066; the heathen Kruto 1066 to 1093.
- **Traded:** captives, honey, furs for cloth and silver; the land road to Bardowick and Hamburg starts here.
- **How a boat comes in:** 15 km up the Trave from the sea, a deep slow river; riverbank under the wall.
- **Why go:** from 1055 a Christian prince who served Cnut in England keeps the peace here, with a church. It is
  the shortest road from the Baltic to the Elbe.
- **Evidence:** the tree-ring dates of 819, 1055 and 1087; a timber church and a later stone one; Adam III.19-20
  (?); Helmold I.20, I.34.
- **Sources:** de.wikipedia.org/wiki/Alt-Lübeck, read 6 Oct 2026.

```json
{"id": "liubice", "name": "Liubice", "modern": "Alt Lübeck", "lat": 53.908, "lon": 10.713, "realm": "obodrites", "county": "", "region": "wendland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 1055, "to": 0, "note": "Gottschalk's fort and church where the Schwartau meets the Trave, rebuilt in 1055; the short road from the Baltic to the Elbe."}
```

## 14. Daugmale (the Dvina)

- **Name then:** not recorded. The river is Dyna to Northmen. Some take it for the "harbour of the Semigallians" of
  the 13th century (?).
- **Modern place:** Daugmale hill fort, on the south bank of the Daugava about 22 km above Riga, Latvia.
- **Lat, lon:** 56.830, 24.380 (?).
- **Active:** a fort since the Bronze Age; a trading place from the 10th century to about 1200, when Riga takes its
  place.
- **Kind:** fort with a harbour and a settlement below it.
- **Who ruled:** Semigallians, and from the 10th or 11th century Livs. No ruler is named.
- **Traded:** out: wax, furs, captives, honey. In: silver (coins from the caliphate, the German towns, England and
  the Greeks, about 190 in all (?)), blades, salt, cloth, bronze. Scales and weights in numbers.
- **How a boat comes in:** over the bar of the Dvina and 35 km (?) up a broad river to a creek mouth under the fort.
  Above lie rapids, and far above them Palteskja and the carry to the Dnieper.
- **Why go:** the mouth of the second road to the Greeks, and shorter than the Ladoga one. Gotlanders use it; a
  Frisian who does is early.
- **Evidence:** the excavations of 1933 to 1937 and 1966 to 1970 (Ginters, Urtans) and later (Zemitis, Radins);
  rune stones in Sweden for men who died on the way to or in "Domesnes" and "Semgallen" (Mervalla stone, Sö 198).
- **Sources:** A. Radins and G. Zemitis, Die Verbindungen zwischen Daugmale und Skandinavien (1992). The Wikipedia
  pages tried tonight did not exist: nothing here was checked on the web.

```json
{"id": "daugmale", "name": "Daugmale", "modern": "Daugmale, above Riga", "lat": 56.83, "lon": 24.38, "realm": "livs", "county": "", "region": "dvina", "kind": "fort", "landing": "riverbank", "size": 2, "from": 900, "to": 0, "note": "A hill fort and harbour on the Dvina a day's row from the sea (?): wax and furs weighed against silver. The lower gate of the Dvina road to Palteskja."}
```

## 15. Eysysla (Saaremaa)

- **Name then:** Eysysla ("island district") to Northmen; the people are the Eistr; Osilia in 13th-century Latin.
- **Modern place:** Saaremaa, Estonia. For the landing use the Viking Age harbour site at Tornimäe on the east
  coast, facing Muhu (?); another lay at Viltina on the south-east shore (?).
- **Lat, lon:** 58.560, 23.070 (?).
- **Active:** all through; harbour sites of the 9th to 12th centuries.
- **Kind:** market at a beach, with a hill fort inland (Pöide).
- **Who ruled:** the islanders' own elders. No king, no lord from outside before 1227.
- **Traded:** out: captives, iron, seal skins and oil, wax, furs from the mainland. In: salt, silver, blades,
  cloth. They are raiders as often as traders.
- **How a boat comes in:** shallow, stony water; the harbours are in the sound between Saaremaa and Muhu. Beach
  landing, with a local man in the bow.
- **Why go:** it lies on the way from Gotland to the Dvina and to the Gulf of Finland, and it is where captives
  taken on that road are sold. The risk is to become one.
- **Evidence:** the two ship burials at Salme (about 750), 41 Svear dead in their boats; the harbour excavations
  (M. Mägi); Sigvat's verses on Olaf Haraldsson's fight in Eysysla, about 1008 (a poet of the time); the story that
  Olaf Tryggvason was taken at sea by Estonians as a child, about 967, and sold for a cloak (Heimskringla: late).
- **Sources:** M. Mägi, In Austrvegr: The Role of the Eastern Baltic in Viking Age Communication (2018);
  `peoples/baltic_east.md`. Not checked on the web tonight.

```json
{"id": "eysysla", "name": "Eysýsla", "modern": "Tornimäe, Saaremaa (?)", "lat": 58.56, "lon": 23.07, "realm": "osilians", "county": "", "region": "estland", "kind": "market", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "The islanders' beach market in the sound behind Saaremaa, on the way to the Dvina: captives, iron and seal oil. They rob as readily as they trade."}
```

## 16. Palteskja (Polotsk)

- **Name then:** Palteskja to Northmen (later texts); Polotesk in the Chronicle.
- **Modern place:** Polotsk, Belarus, where the Polota runs into the Dvina.
- **Lat, lon:** 55.486, 28.763 (?).
- **Active:** settled in the first half of the 9th century; named under 862; a prince's town to 1066 and after.
- **Kind:** royal seat on a river.
- **Who ruled:** Rogvolod, who "came from over the sea" (to about 978-980); Vladimir takes the town, kills him and
  takes his daughter Rogneda; Iziaslav 989-1001; Briachislav 1001-1044 (sacks Novgorod 1021); Vseslav 1044-1101,
  who builds Holy Wisdom here (between 1044 and 1066) and burns Novgorod in 1066-1067.
- **Traded:** wax, furs, honey, captives for silver, salt, blades and cloth.
- **How a boat comes in:** about 400 km (?) up the Dvina from Daugmale, with rapids in the lower river where boats
  are hauled. A small boat only. Riverbank under the fort. From here it is some days upriver to the Kasplya and the
  carry to Gnezdovo.
- **Why go:** a prince who is nobody's man and has no love for Novgorod or Kiev: he buys from whoever comes.
- **Evidence:** the Primary Chronicle (862, 980, 1021, 1044, 1066); the excavations of the fort on the Polota.
- **Sources:** en.wikipedia.org/wiki/Polotsk, read 6 Oct 2026; `realms/rus.md`.

```json
{"id": "palteskja", "name": "Palteskja", "modern": "Polotsk", "lat": 55.486, "lon": 28.763, "realm": "polotsk", "county": "", "region": "dvina", "kind": "royal seat", "landing": "riverbank", "size": 3, "from": 860, "to": 0, "note": "The town on the upper Dvina with princes of its own: Rogvolod from over the sea, killed about 980; Vseslav from 1044, who builds a Holy Wisdom to match Kiev's."}
```

## 17. Arkona (Rügen)

- **Name then:** Arkona (Saxo and Helmold, 12th century); the people are the Rani.
- **Modern place:** Cape Arkona, Rügen, Germany. The landing is the beach at Vitt, 1 km south.
- **Lat, lon:** 54.676, 13.437.
- **Active:** a walled holy place on the cliff from the 9th century (by excavation) to 1168.
- **Kind:** a temple fort, not a town. (The table has no kind for a shrine: "fort" is the nearest.)
- **Who ruled:** the priest of Svantevit, whom the Rani obey before their king (Helmold, of the 12th century (?)).
- **Traded:** the herring: in the storms of November the shoals come in and many ships gather; a stranger may fish
  and buy freely once he has given the god his due (Helmold II.12, of about 1170: used for our years with a (?)).
- **How a boat comes in:** an open beach under a chalk cliff, in settled weather only. No shelter from the east.
- **Why go:** herring when the Lent trade wants it; and the god's peace, which holds for anyone who has paid.
  Merchants of every people pay (Helmold I.6 (?)).
- **Evidence:** the excavations of 1921, 1969-1971 and 1994 on, as the cliff falls: offerings of weapons, silver
  and trade goods from the 9th century on; Saxo XIV (the temple and the four-headed image, as destroyed in 1168).
- **Sources:** Helmold, Chronica Slavorum I.6, I.52, II.12; Saxo XIV.39; F. Ruchhöft, Die Burg am Kap Arkona
  (2010 (?)). Not checked on the web tonight.

```json
{"id": "arkona", "name": "Arkona", "modern": "Cape Arkona, Rügen", "lat": 54.676, "lon": 13.437, "realm": "rani", "county": "", "region": "wendland", "kind": "fort", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "The Rani's holy place of Svantevit on a chalk cliff. In the herring weeks of late autumn ships gather under it, and a stranger gives the god his share first (said of the 12th century (?))."}
```

## 18. Dierkow (the Warnow)

- **Name then:** lost.
- **Modern place:** Rostock-Dierkow, Germany, on the east bank of the lower Warnow.
- **Lat, lon:** 54.098, 12.155 (?).
- **Active:** about 750 to about 850 (?).
- **Kind:** market and craft place: a smaller Reric.
- **Who ruled:** the Wilzi (the people here are later the Kessini).
- **Traded:** amber and glass beads made on the spot, combs, iron; in: Scandinavian brooches, Frankish glass,
  querns, silver.
- **How a boat comes in:** into the Warnow mouth and 10 km up a broad still estuary; riverbank, with timber
  landing stages found in the wet ground.
- **Why go:** a quiet market between Reric and Menzlin, used by Northmen, outside the Danish king's toll.
- **Evidence:** excavations of 1986 to 1990 (Warnke) and from 2013 (Messal): workshops, wells, a jar of
  Scandinavian and Frankish metalwork, harbour timbers.
- **Sources:** D. Warnke (1992); S. Messal's reports for the German Archaeological Institute (?). Not checked on
  the web tonight.

```json
{"id": "dierkow", "name": "Dierkow", "modern": "Rostock-Dierkow", "lat": 54.098, "lon": 12.155, "realm": "wilzi", "county": "", "region": "wendland", "kind": "market", "landing": "riverbank", "size": 2, "from": 0, "to": 850, "note": "A craft market on the lower Warnow, bead makers and comb makers, used by Northmen; its own name is lost (?)."}
```

## 19. Pleskov (Pskov)

- **Name then:** Pleskov (the Chronicle).
- **Modern place:** Pskov, Russia, where the Pskova runs into the Velikaya, 12 km above the lake.
- **Lat, lon:** 57.822, 28.329.
- **Active:** a fort from before 800; Northmen's chamber graves of the 10th century; named under 903.
- **Kind:** town and fort.
- **Who ruled:** Novgorod's princes. Olga, who rules Kiev from 945, was brought from here (Chronicle, 903).
  Yaroslav's brother Sudislav sits here until he is locked up in 1036.
- **Traded:** wax, furs, flax, captives for silver, salt and cloth.
- **How a boat comes in:** from the Gulf of Finland up the Narva (a fall, where everything is carried), across the
  long lake and up the Velikaya. A small boat only.
- **Why go:** the back door to Novgorod's country, without Aldeigjuborg's lord or the Volkhov rapids.
- **Evidence:** the Primary Chronicle (903, 1036); chamber graves with scales, silver and Scandinavian dress found
  in 2003 to 2009 (Yakovleva, Ershova (?)).
- **Sources:** `realms/rus.md`. Not checked on the web tonight.

```json
{"id": "pleskov", "name": "Pleskov", "modern": "Pskov", "lat": 57.822, "lon": 28.329, "realm": "rus", "county": "", "region": "ilmen", "kind": "town", "landing": "riverbank", "size": 2, "from": 860, "to": 0, "note": "The fort on the Velikaya above the long lake, reached by the Narva and its fall: the back door to Novgorod's country. Olga of Kiev came from here (?)."}
```

---

# LEAVE OUT

| Place | Modern | Lat, lon | Why not |
|---|---|---|---|
| Kamien | Kamien Pomorski, Poland | 53.969, 14.773 (?) | A fort on the Dziwna below Wolin; a duke's seat and a market only from the 1120s. In the game's years it is Jumne's outpost: give it to Jumne as a site. |
| Usedom (Uznam) | Usedom, Germany | 53.870, 13.920 (?) | A harbour suburb from the 10th century, but it is written of only from 1128. Too close to Menzlin and Jumne to earn a place. |
| Puck | Puck, Poland | 54.720, 18.410 (?) | A harbour with sunken wrecks and jetties of the 10th to 13th centuries in Puck bay. Real, but 40 km from Gyddanyzc and with nothing written. |
| Palanga and Apuole | Lithuania | 55.918, 21.060 and 56.246, 21.670 (?) | Palanga is a Curonian beach market of the 9th to 12th centuries; Apuole is the "Apulia" the Svear besiege about 854 (Rimbert 30), 40 km inland with no water to it. The table's `grobin` already stands for the Curonians: lengthen its `to` rather than add these (?). |
| Iru | by Tallinn, Estonia | 59.460, 24.900 (?) | A hill fort above a river mouth, left about 1000; Tallinn's harbour is not attested before the 12th century (?). A beach for the lookout, not a port. |
| Lyubech, Chernigov, Vyshgorod | Ukraine | 51.700, 30.660; 51.490, 31.300; 50.585, 30.490 (?) | All named in De Administrando Imperio 9 as places the boats gather from, all real. But for a western skipper they are stops on the way to Kiev: cards on the river, not ports. |
| Beloozero and Timerevo | Russia | off the chart | The Volga road to Bulghar. The design's world ends at Kiev and Novgorod (DECISIONS.md); `realms/rus.json` keeps `rus_volga` as a rumour, which is right. |

Names to refuse outright: **Jomsborg** and **Vineta** (both are Jumne, in later dress), and **Seeburg** as a new
port (Rimbert's Seeburg is taken to be Grobin, which the table has).

---

# All the rows together

Paste into `"ports"` after `aldeigjuborg`, once the realm and region ids above exist.

```json
  {"id": "jumne", "name": "Jumne", "modern": "Wolin", "lat": 53.843, "lon": 14.613, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "emporium", "landing": "jetty", "size": 5, "from": 800, "to": 0, "note": "The Wolinians' town on the Dziwna arm of the Oder mouth, ruled by its elders: the greatest market of the Baltic about 950 to 1040; the Polish duke takes tribute 967 to about 1005 (?); Magnus burns much of it in 1043."},
  {"id": "holmgardr", "name": "Holmgardr", "modern": "Veliky Novgorod", "lat": 58.521, "lon": 31.275, "realm": "rus", "county": "", "region": "ilmen", "kind": "town", "landing": "riverbank", "size": 4, "from": 860, "to": 0, "note": "The fort where the Volkhov leaves Lake Ilmen, and from about 950 the new town below it: furs and wax for silver. Two hundred kilometres and two runs of rapids above Aldeigjuborg; a small boat only."},
  {"id": "koenugardr", "name": "Koenugardr", "modern": "Kyiv (the Podil)", "lat": 50.463, "lon": 30.517, "realm": "rus", "county": "", "region": "dnieper", "kind": "royal seat", "landing": "riverbank", "size": 5, "from": 880, "to": 0, "note": "The prince's town on the hill and the boat harbour under it: the winter's tribute of furs, wax and captives goes down to the Greeks each June and silk and wine come back. In 1018 a Saxon bishop hears of eight markets here (?)."},
  {"id": "gnezdovo", "name": "Miliniska (?)", "modern": "Gnezdovo, by Smolensk", "lat": 54.785, "lon": 31.879, "realm": "rus", "county": "", "region": "dnieper", "kind": "emporium", "landing": "riverbank", "size": 3, "from": 900, "to": 1020, "note": "The carry between the Dvina and the Dnieper: boatwrights, smiths and three thousand grave mounds. Boats are dragged here from the Kasplya (?); its own name is lost."},
  {"id": "starigard", "name": "Starigard", "modern": "Oldenburg in Holstein", "lat": 54.293, "lon": 10.886, "realm": "obodrites", "county": "", "region": "wendland", "kind": "royal seat", "landing": "beach", "size": 3, "from": 0, "to": 0, "note": "The Wagrian prince's fort and hall on a shallow sound across the neck of Wagria (?); a bishop from 968, thrown out in 983, 1018 and 1066."},
  {"id": "ralswiek", "name": "Ralswiek", "modern": "Ralswiek, Rügen", "lat": 54.474, "lon": 13.449, "realm": "rani", "county": "", "region": "wendland", "kind": "emporium", "landing": "beach", "size": 3, "from": 0, "to": 1000, "note": "The Rani's market at the head of the Jasmund Bodden: a row of homesteads, each with its own boat berth (?); herring and captives for eastern silver. Four boats and 2,203 dirhams were found here."},
  {"id": "menzlin", "name": "Menzlin", "modern": "Altes Lager, Menzlin by Anklam", "lat": 53.857, "lon": 13.64, "realm": "wilzi", "county": "", "region": "wendland", "kind": "emporium", "landing": "riverbank", "size": 3, "from": 0, "to": 920, "note": "The Wilzi's market at the Peene bridge, Wends and Northmen together; its own name is lost. Graves set with stones in the shape of ships."},
  {"id": "kaup", "name": "Kaup", "modern": "Mokhovoye (Wiskiauten), by Zelenogradsk", "lat": 54.93, "lon": 20.49, "realm": "prussians", "county": "", "region": "prussia", "kind": "market", "landing": "beach", "size": 2, "from": 850, "to": 1050, "note": "Northmen settled among the Prussians of Sambia at the foot of the long spit: amber at the source. Five hundred grave mounds in a wood; its own name is lost (?)."},
  {"id": "gyddanyzc", "name": "Gyddanyzc", "modern": "Gdańsk", "lat": 54.354, "lon": 18.66, "realm": "poles", "county": "", "region": "pomerania", "kind": "town", "landing": "riverbank", "size": 2, "from": 980, "to": 0, "note": "The Polish duke's fort at the Vistula mouth (the date of the fort is disputed: about 980 or the 1060s). Bishop Adalbert takes ship here for Prussia in 997."},
  {"id": "stetin", "name": "Stetin", "modern": "Szczecin", "lat": 53.426, "lon": 14.56, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "town", "landing": "riverbank", "size": 3, "from": 850, "to": 0, "note": "The fort on the hill above the Oder, with the temple of three-headed Triglav; Jumne's rival, and after 1043 its heir. Markets twice a week (said of 1124 (?))."},
  {"id": "cholberg", "name": "Cholberg", "modern": "Budzistowo, by Kołobrzeg", "lat": 54.157, "lon": 15.587, "realm": "pomeranians", "county": "", "region": "pomerania", "kind": "fort", "landing": "riverbank", "size": 2, "from": 880, "to": 0, "note": "Salt Cholberg: brine springs and a fort a little way up the Parsęta. A bishop from 1000 to about 1007, who throws anointed stones into the sea to cleanse it (?)."},
  {"id": "michelenburg", "name": "Michelenburg", "modern": "Dorf Mecklenburg, by Wismar", "lat": 53.84, "lon": 11.467, "realm": "obodrites", "county": "", "region": "wendland", "kind": "royal seat", "landing": "beach", "size": 2, "from": 810, "to": 0, "note": "The Obodrite king's great fort in a marsh at the head of Wismar bay, where Reric was: captives and horses. A bishop about 1055 to 1066 (?)."},
  {"id": "liubice", "name": "Liubice", "modern": "Alt Lübeck", "lat": 53.908, "lon": 10.713, "realm": "obodrites", "county": "", "region": "wendland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 1055, "to": 0, "note": "Gottschalk's fort and church where the Schwartau meets the Trave, rebuilt in 1055; the short road from the Baltic to the Elbe."},
  {"id": "daugmale", "name": "Daugmale", "modern": "Daugmale, above Riga", "lat": 56.83, "lon": 24.38, "realm": "livs", "county": "", "region": "dvina", "kind": "fort", "landing": "riverbank", "size": 2, "from": 900, "to": 0, "note": "A hill fort and harbour on the Dvina a day's row from the sea (?): wax and furs weighed against silver. The lower gate of the Dvina road to Palteskja."},
  {"id": "eysysla", "name": "Eysýsla", "modern": "Tornimäe, Saaremaa (?)", "lat": 58.56, "lon": 23.07, "realm": "osilians", "county": "", "region": "estland", "kind": "market", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "The islanders' beach market in the sound behind Saaremaa, on the way to the Dvina: captives, iron and seal oil. They rob as readily as they trade."},
  {"id": "palteskja", "name": "Palteskja", "modern": "Polotsk", "lat": 55.486, "lon": 28.763, "realm": "polotsk", "county": "", "region": "dvina", "kind": "royal seat", "landing": "riverbank", "size": 3, "from": 860, "to": 0, "note": "The town on the upper Dvina with princes of its own: Rogvolod from over the sea, killed about 980; Vseslav from 1044, who builds a Holy Wisdom to match Kiev's."},
  {"id": "arkona", "name": "Arkona", "modern": "Cape Arkona, Rügen", "lat": 54.676, "lon": 13.437, "realm": "rani", "county": "", "region": "wendland", "kind": "fort", "landing": "beach", "size": 1, "from": 0, "to": 0, "note": "The Rani's holy place of Svantevit on a chalk cliff. In the herring weeks of late autumn ships gather under it, and a stranger gives the god his share first (said of the 12th century (?))."},
  {"id": "dierkow", "name": "Dierkow", "modern": "Rostock-Dierkow", "lat": 54.098, "lon": 12.155, "realm": "wilzi", "county": "", "region": "wendland", "kind": "market", "landing": "riverbank", "size": 2, "from": 0, "to": 850, "note": "A craft market on the lower Warnow, bead makers and comb makers, used by Northmen; its own name is lost (?)."},
  {"id": "pleskov", "name": "Pleskov", "modern": "Pskov", "lat": 57.822, "lon": 28.329, "realm": "rus", "county": "", "region": "ilmen", "kind": "town", "landing": "riverbank", "size": 2, "from": 860, "to": 0, "note": "The fort on the Velikaya above the long lake, reached by the Narva and its fall: the back door to Novgorod's country. Olga of Kiev came from here (?)."}
```

# Sources

Pages read on 6 Oct 2026:

- https://en.wikipedia.org/wiki/Wolin_(town)
- https://en.wikipedia.org/wiki/Ralswiek (coordinates only; the page is a stub)
- https://en.wikipedia.org/wiki/Gnezdovo
- https://en.wikipedia.org/wiki/Rurikovo_Gorodische
- https://en.wikipedia.org/wiki/Kołobrzeg
- https://en.wikipedia.org/wiki/History_of_Gdańsk
- https://en.wikipedia.org/wiki/Szczecin
- https://en.wikipedia.org/wiki/Polotsk
- https://de.wikipedia.org/wiki/Oldenburger_Wall
- https://de.wikipedia.org/wiki/Altes_Lager_(Menzlin)
- https://de.wikipedia.org/wiki/Wiskiauten
- https://de.wikipedia.org/wiki/Alt-Lübeck

Tried and not found (404): en.wikipedia.org/wiki/Menzlin, /Kaup_(Wiskiauten), /Daugmale;
de.wikipedia.org/wiki/Burgberg_Daugmale.

Written sources of the time (cited by chapter from memory; check against an edition before a player sees a number):

- Adam of Bremen, Gesta Hammaburgensis ecclesiae pontificum (about 1075), II.18-22, II.27-28, III.19-20, III.50-51,
  IV.18, scholia.
- Widukind of Corvey, Res gestae Saxonicae (about 968), III.69.
- Thietmar of Merseburg, Chronicon (1012-1018), IV.45, VII.72, VIII.32.
- Ibrahim ibn Yaqub (965), as kept by al-Bakri.
- Constantine VII, De Administrando Imperio (about 950), ch. 9.
- The Primary Chronicle (about 1113), entries 862 to 1067.
- Rimbert, Vita Anskarii (about 875), ch. 30.
- Wulfstan's voyage in the Old English Orosius (about 890).
- The first Life of Adalbert of Prague (about 999).
- Helmold, Chronica Slavorum (about 1170), and Ebo and Herbord, lives of Otto of Bamberg (1150s), and Saxo (about
  1200): all later than the game; used only where marked.

Public-domain translations that may be quoted in the game (BRIEF.md rule 4): Rimbert (Robinson 1921); the Orosius
voyages (Bosworth 1859). Adam, Thietmar, Helmold, the Primary Chronicle and De Administrando Imperio have no English
translation old enough: say what they say in our own words, as this page does.

Project research used: `Design/content/realms/wends_baltic.md`, `realms/rus.md`, `peoples/wendish.md`,
`peoples/baltic_east.md`.
