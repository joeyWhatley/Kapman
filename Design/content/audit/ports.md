# Audit: the ports of 793 (`ports_793.json`)

Audited 6 Oct 2026. The table was not edited. 50 rows checked.

**What was checked, and where it lives.** `ports_793.json` holds only name, modern place, coordinates, realm, county,
region, kind, landing, size, years and note. Four things the brief asks about are not in it and are derived:

- **people** and **faith** and **mint**: `encounters/npc_roles_793.json`, block `"ports"` (`peoples`, `faith`, `has_mint`).
- **toll** and money: `trade_rules.json`, by the port's `region` -> zone (frankish 10 %, english 5 %, weight 0 %, dirham 10 %).
- the words' culture: `wording_793.json` (`culture_of_region`, `culture_of_port`).

So a row below is judged on all three files. **right** = agrees with the sources as far as I could check. **wrong** =
a correction with its source. **doubtful** = why. (?) = I could not open a source for it; it stands on memory.
Coordinates were checked against Wikipedia's (opened 6 Oct 2026 where a key is given, otherwise from memory); "right"
means within about a kilometre.

Source keys: [ARF] Annales regni Francorum; [AF] Annales Fuldenses; [AB] Annales Bertiniani; [ASC] Anglo-Saxon
Chronicle; [Bede] Historia ecclesiastica; [Rim] Rimbert, Vita Anskarii; [Altfrid] Vita Liudgeri; [Oht] Ohthere and
Wulfstan in the Old English Orosius; [S n] Sawyer charter n (esawyer.lib.cam.ac.uk); [D:x] the project's own dossier
`Design/content/dossiers/x.md` (sourced there); [W:x] Wikipedia article x, language given, opened 6 Oct 2026.

## 1. Every row

| id | verdict | what | correction or reason, and source |
|---|---|---|---|
| dorestad | right | all fields; toll and mint | Toll: the tenth at Dorestad, Quentovic and the Alpine passes (Praeceptum negotiatorum, 828) (?). Mint: DORESTAT deniers of Charlemagne. Last raid 863 [AB 863]: `to: 863` is the last mention of it as a place worth robbing, not a recorded end. |
| traiectum | right | all fields | Bishop in 793 Hamacar (790-806), a name only [D:traiectum]. No mint: right. |
| walichrum | doubtful | name; `to: 850` | Walichrum / Walacria is the name of the island (Alcuin, Vita Willibrordi 14; AB 837), not of the beach market, whose own name is lost. Keep the name, say so in the note. The end "about 850" is the coin series fading, not a dated event (?). The ring fort at Domburg is of about 875 [W:en Domburg]. |
| witla | right | all fields | "Witlam emporium iuxta ostium Mosae fluminis" burned 836 [AF 836]. Place lost; the coordinates are a guess and are marked so. `to: 836` is the last mention [D:witla]. |
| medemelacha | doubtful | `landing: jetty` | No jetty is evidenced [D:medemelacha]; `beach` is safer. Rest right. |
| wijnaldum | wrong | `kind`, `name`, `to`, lat | Not a royal seat in 793: the gold is of about 630, Frisia west of the Lauwers is Frankish since 734, and there is no Frisian king [D:wijnaldum]. Set `kind: village`. "Winaldum" is not attested before the 13th century: add (?) in the note. The terp is lived on after 850: set `to: 0` or drop it as a port. Lat 53.195, lon 5.460 [W:en Wijnaldum] (table 53.21: 1.7 km north). |
| doccinga | right | all fields | Boniface killed 5 June 754. The landing is a tidal creek (the Dokkumer Ee), not open beach: fine as `beach`. |
| emuthon | wrong | `from`; name | Founded "about 800"; not before the mid 8th century [D:emuthon; W:de Emden]. Set `from: 800`. First sure name AMVTHON on pennies of 1020-1051; "Emuthon" is a later spelling [D:emuthon]: name it `Amuthon` and add (?). CAUTION already in the roles table: East Frisian rising 792-793. |
| daventre | doubtful | `county: salon` | The grant of 1046 puts Deventer in Hamaland; the border with Salland is unclear [D:daventre]. Leave, add (?). Mint only from about 1000: right. |
| noviomagus | right | all fields | Palace on the Valkhof; no mint 793-1066 [D:noviomagus]. Charles is not attested here in 793. |
| quentovic | wrong | lon/lat; `to` | 50.4825 N, 1.7214 E [W:en Quentovic] (table is 2 km west). Last text 864 (Edict of Pitres), last coin about 980, abandoned early 11th century [W:en Quentovic]: set `to: 1000`. Toll (Gervold, 787) and mint: right. |
| rotomagus | doubtful | `landing: quay`; realm later | No quay of 793 is proven (?); `riverbank` is safer. From 911 it is Rollo's: the table has no way to change `realm` by year (see change 6). |
| parisius | right | all fields | Saint-Denis fair from 9 October; the city and the abbey are 9 km apart [D:parisius]. |
| ganda | right | all fields | Saint Bavo's (Ganda) and Saint Peter's (Blandinium). Fleet inspected 811 [ARF 811]. |
| andoverpis | doubtful | `kind: fort`; people | "Fort" is only the charter's word (castrum, 726); rampart about 900 [D:andoverpis]. Frisians here rest on one 7th-century text: drop `frisian` from `peoples` or mark (?). Burned 836 [AF 836]. |
| colonia | wrong | note | In 793 a bishop's see. Hildebold is bishop from 787; Cologne becomes an archbishopric in 794/795 [W:de Hildebold von Köln]. Change "an archbishop's see" to "a bishop's see (an archbishop's from 794/5)". Mint: right. |
| mogontia | right | all fields | Archbishopric since about 780 (Lull). Frisian quarter burned 886 [AF 886]. The usual period spelling is Mogontia or Moguntia: fine. |
| diusburg | doubtful | `kind: market`; `from` | First named 883/884 (Regino: Diusburh). Market about 870-80, church and Frisians 893 [D:diusburg]. In 793 only a landing under a royal estate: set `kind: village`, `size: 1`, or `from: 870`. |
| lundenwic | right | all fields; mint | Mercian in 793; Offa's London mint. The wic is given up and the town moves inside the walls by 886 [W:en Anglo-Saxon London]: a later row (Lundenburh, from 886) is wanted, see change 6. |
| hamwic | doubtful | mint; `to` | Roles table says no mint; Wikipedia says a mint 786-858 and Series H sceattas struck here [W:en Hamwic]; the 786-858 claim is not well founded (?). Flourished about 700-850, then trade moves to Winchester: consider `to: 900` for the wic (?). |
| gipeswic | right | all fields | Mint probable, marked (?) in the roles table already. |
| eoforwic | right | all fields | The wic at Fishergate, where Foss meets Ouse. Northumbrian kings' and archbishops' coin: mint right. Taken 866 [ASC 867]. |
| sarre | wrong | `realm` | Kent has no king in 793: Offa rules it directly from 785 to 796; revolt 796-798; Mercian client kings to 825; then Wessex [W:en Kingdom of Kent; ASC 796, 823]. Set `realm: mercia` (see change 2). Name "ad Serrae" and the king's toll on ships: right [S 29]. |
| fordwic | wrong | `realm`; mint | Realm as sarre. `has_mint: true` is Canterbury's mint, an hour inland, not Fordwich's: the roles note says so; keep, but a card must not put a moneyer on the landing. Toll: right [S 29]. |
| dofras | wrong | `realm`; `kind` | Realm as sarre. No market, fair or toll is attested in 793; "minster and landing" is what the ground shows [D:dofras]: set `kind: monastery` or `village`. |
| lindisfarena | doubtful | `to` | The community leaves with Cuthbert's body in 875 and the house is not refounded until after 1066 (?): set `to: 875`. Raid 8 June 793: right (the Chronicle's "January" is a slip). |
| bebbanburg | right | all fields | King Æthelred 790-796 in the roles note: right. |
| gyrwe | doubtful | `to` | Deserted in the later 9th century, refounded 1074 (?): set `to: 870` (?). The 794 raid at "Donemuthe" is usually, not surely, Jarrow: marked. |
| streonaeshalch | doubtful | `to` | Wasted about 867, refounded about 1078; the name Whitby is the Danes' (?). Set `to: 867` (?). Identification with Whitby is the usual one. |
| dommoc | doubtful | place | Dunwich or Walton Castle (Felixstowe): already marked (?). The see ends about 870 (?): `to: 870`. |
| legacaestir | wrong | `kind: town`, `size` | In 793 mostly ruins: "a waste chester in Wirral" still in 893 [ASC 894]; restored 907; a busy mint only in the 10th century [D:legacaestir]. Set `kind: fort` or `village`, `size: 1` until 907. Name: Bede's Legacaestir: right. |
| hi | right | all fields | 795, 802, 806 raids (Annals of Ulster). Realm dalriata: right. |
| duiblinn | wrong | people id; years | Roles table gives `peoples: ["irish"]` here and `["gaelic"]` at hi; the wording table knows only the culture `gaelic`. One people, two ids: pick one (see change 8). The Norse town from 841 needs its own row or an era (change 6). `realm: irish` is no realm: Leinster (Laigin) (?). |
| ripa | right | all fields | Laid out about 705-710. No toll and no coin in the game: acceptable; the sceatta question is marked. |
| sliesthorp | doubtful | `landing: jetty`; toll | Sliesthorp 804, 808 [ARF]. Whether the name means Hedeby itself or the Schlei district is argued (?). Jetties are 9th-century: `beach` in 793 (?). Toll: see change 3. |
| reric | wrong | toll; coordinates (?) | The one northern port whose toll is in a text: Reric "by the paying of tolls was of great use to his kingdom" [ARF 808, my rendering]. The game gives it 0 % (zone `weight`). Dendro: settled 735, ends soon after 811 [W:de Reric]: `from: 735`, `to: 808` right. Coordinates within 2 km of Groß Strömkendorf, not checked closer (?). |
| skiringssal | wrong | lon; `from`, `to` | 59.031 N, 10.108 E [W:en Kaupang] (table 10.031: 4.4 km west, by Larvik). Founded "in the 780s" [W:en Kaupang] or about 800 (Skre) (?): `from: 780` can stay with its (?). Abandoned "early 10th century" [W:en Kaupang], about 930 (Skre) (?): `to: 960` is late, set `to: 930`. |
| birka | right | all fields | About 750 to about 975. King's prefect, thing [Rim 11, 19, 26-27]. |
| helgo | wrong | lat/lon | 59.2785 N, 17.680 E [W:en Helgö] (table is 4.7 km north). `to: 800`: a fair guess, no dated end. |
| uppsalir | doubtful | note "temple" | The temple is Adam of Bremen, about 1075: the roles note already says LATER. Change the table's note to "seat and great mounds". |
| paviken | wrong | lat/lon; `to` | 57.455 N, 18.141 E [W:sv Paviken] (table is 4.5 km south-east, inland). In use from the 700s to the 1000s, then the inlet silts and trade moves to Västergarn [W:sv Paviken]: set `to: 1000`. |
| ahus | doubtful | `to`; name; realm | The trading place (upstream of the town) is of the Vendel / early Viking age (?), over by about 850 (?): set `to: 850` (?). First name "Aos", 1296 [W:en Åhus]: "Áhús" is a back-formed spelling, mark (?). Skåne is Denmark's by Wulfstan, about 890 [Oht]; in 793 unknown (?). |
| truso | wrong | `to: 850` | The note itself has Wulfstan sailing here about 890. Abandoned in the second half of the 10th century, destroyed about 1000 [W:en Truso]. Set `to: 1000`. Founded late 7th / 8th century: `from: 750` can be `0`. Scandinavians in the centre, Balts at the edge [W:en Truso]: peoples right. |
| grobin | right | all fields | About 650-850 (the grave fields). Culture norse by `culture_of_port`: right. |
| aldeigjuborg | doubtful | `realm: rus`; toll | The name Rus is first written in 839 [AB 839]: in 793 nobody is called that. Keep the id, never show it as a word before 839. Zone `dirham` gives a 10 % toll: the tenth is the Khazars', on the Volga; none is known at Ladoga (see change 3). Oldest timbers 753: right. |
| ogvaldsnes | right | all fields | Old Norse Ǫgvaldsnes; the ö is a modern spelling: harmless. |
| hleidr | doubtful | coordinates; landing | The halls at Gammel Lejre are about 4 km inland from the head of the fjord (Lejre Vig) (?). A ship cannot lie at 55.605, 11.97: put the port on the shore and walk, or mark `landing` so the river bake does not look for water [D:hleidr]. |
| namnetis | right | all fields | No mint under Charlemagne: right for 793, wrong from Louis the Pious; Breton from 851 [D:namnetis]. Needs an era (change 6). |
| herio | doubtful | `to`; note | The monks leave for good in 836 (Ermentarius): set `to: 836` (?). Salt pans in 793 are supposed, not attested [D:herio]. The 799 raid is "the islands of Aquitaine" (Alcuin), usually read as Noirmoutier. |
| burdigala | doubtful | `landing: quay` | No quay of these years is known [D:burdigala]: `riverbank`. Mint (BVRDIGALA deniers of Charlemagne) (?): keep the (?). Taken 848: right. |

Count: right 18, wrong 14, doubtful 18 (50 rows; `sarre`, `fordwic` and `dofras` share one correction).

## 2. Things true of the whole table

- **Faith** is right everywhere for 793 at the level the game uses (christian in the Frankish, English and Gaelic
  lands; old gods in the north; own gods for Wends and Balts). Two cautions: Frisia east of the Lauwers was in revolt
  and its churches burned in 792-793 (emuthon, doccinga) [Altfrid I.22]; Saxons at daventre are baptised by force
  since 785, so "christian" there is thin.
- **People**: `romance` at quentovic is doubtful (the Canche in 793 is Frankish-speaking country with English and
  Frisian guests) (?). `danish` at truso rests on the digs: fine.
- **Names** are a fair mix of Latin, Old English and Old Norse as the header says. Three are not of the time:
  Winaldum (13th century), Emuthon (later than Amuthon, 11th century), Áhús (Aos, 1296).
- **Years**: ten monasteries and wics have `to: 0` though they stop being trading places inside the game's span
  (lindisfarena 875, gyrwe, streonaeshalch 867, herio 836, hamwic, quentovic, dommoc, paviken, ahus, lundenwic as a wic).

## 3. The ten changes that matter most

Written for the programmer. File paths are under `Assets/Kapman/Game/Data/`.

1. **Truso lives to 1000.** `ports_793.json`, id `truso`: `"to": 850` -> `"to": 1000`. As it stands the port closes
   forty years before Wulfstan's voyage of about 890, which the game's own text (`texts_793.json` wulfstan) describes.
2. **Kent is Mercian in 793.** `ports_793.json`, ids `sarre`, `fordwic`, `dofras`: `"realm": "kent"` -> `"mercia"`.
   If realms can change by year (see 6): mercia 785-796, kent 796-798, mercia 798-825, wessex from 825. Until then
   a card that reads `port.realm = kent` in 793 names a kingdom with no king.
3. **Tolls: one rate per zone is wrong in three places.** In `trade_rules.json` the zone decides the toll. Add an
   optional `"toll"` number to a port row in `ports_793.json` that overrides the zone (PortDef gets `public float toll = -1;`,
   and the place that reads the zone's toll uses the port's when it is >= 0). Then set: `reric` 0.10 (the king's toll,
   ARF 808); `aldeigjuborg` 0 (no toll known; the Khazar tenth is on the Volga); and for the Frankish zone keep 0.10
   only at `dorestad` and `quentovic` (the tenth is taken there and at the Alpine passes, Praeceptum negotiatorum 828 (?)),
   with a small landing due (0.02-0.04, a guess to tune) at the other Frankish ports. The English 0.05 has no source for
   its rate: mark it a guess in the zone's note.
4. **Four coordinates are 2 to 5 km off; fix before the chart is drawn.** `skiringssal` lon 10.031 -> 10.108 (lat
   59.031). `helgo` 59.32, 17.7 -> 59.2785, 17.680. `paviken` 57.42, 18.18 -> 57.455, 18.141. `quentovic` 50.475, 1.695
   -> 50.4825, 1.7214. Also `wijnaldum` lat 53.21 -> 53.195.
5. **Wijnaldum is not a royal seat.** `ports_793.json`, id `wijnaldum`: `"kind": "royal seat"` -> `"village"`,
   `"to": 850` -> `0`, note -> "A great terp in Westergo; its lords' gold is a hundred and fifty years old." The kind
   drives the blockout and the market title, so a hall and a king's man would be built where there was neither.
6. **Let a port change with the years.** The table has one `realm`, `kind`, `size` and name per port for 273 years. Add
   an optional `"eras": [ { "from": 841, "realm": "norse_dublin", "kind": "emporium", "size": 3, "name": "Dyflin" } ]`
   to a row; the newest era whose `from` <= year overrides the row's fields. First uses: `duiblinn` 841 (longphort,
   norse), `rotomagus` 911 (Rollo), `lundenwic` 886 (inside the walls, Lundenburh), `eoforwic` 866 (the Danes' Jórvík),
   `sliesthorp` 808 (size 4, emporium; mint from 825), `legacaestir` 907 (restored: town, size 2), `colonia` 795
   (archbishop), `namnetis` 851 (Breton). `has_mint` in `npc_roles_793.json` needs the same: at least a `mint_from`
   year (sliesthorp 825, duiblinn 997, daventre 1000, namnetis 814).
7. **End years for places that stop trading.** `ports_793.json`: `quentovic` `to: 1000`; `paviken` `to: 1000`;
   `lindisfarena` `to: 875`; `herio` `to: 836`; `streonaeshalch` `to: 867` (?); `gyrwe` `to: 870` (?); `skiringssal`
   `to: 960` -> `930`. And one start year: `emuthon` `from: 0` -> `800`.
8. **One id for the Irish.** `encounters/npc_roles_793.json`: `duiblinn` has `"peoples": ["irish"]`, `hi` has
   `["gaelic"]`, the realm `irish` maps to `irish` and `dalriata` to `gaelic`; `wording_793.json` has only the culture
   `gaelic`; `names_placeholder.json` has names only for `irish`. Choose `irish` for people everywhere (keep `gaelic`
   as the wording culture), or a card at Iona finds no names to draw.
9. **Places that are less than the table says in 793.** `legacaestir`: `kind: town` -> `fort`, `size: 2` -> `1`
   (ruins until 907). `diusburg`: `kind: market` -> `village`, `size: 1` (first named 883). `dofras`: `kind: market`
   -> `monastery`. `andoverpis`: keep `fort`, drop `frisian` from its `peoples`. `colonia` note: "an archbishop's
   see" -> "a bishop's see (an archbishop's from 794/5)". `uppsalir` note: drop "and temple".
10. **Names and landings to mark.** Add (?) to the notes of `wijnaldum` (Winaldum: 13th century), `emuthon` (rename
    `Amuthon`, the coin spelling of 1020-1051, still (?) for 793), `ahus` (Aos, 1296), `walichrum` (the island's name,
    the market's is lost). Landings with no evidence for the built thing: `medemelacha` jetty -> beach, `sliesthorp`
    jetty -> beach (jetties come with the era of 808), `rotomagus` and `burdigala` quay -> riverbank. `hleidr`: move
    the point to the shore of Lejre Vig (about 55.64 N, 11.98 E (?)) or flag it as a walk inland.

## 4. What I could not check

- The Praeceptum negotiatorum (828) wording on the tenth at Dorestad and Quentovic: from memory.
- The end dates of Jarrow, Whitby and Dunwich as living places; the Åhus trading place's dates; Skre's dates for Kaupang.
- The exact site coordinates of Reric (Groß Strömkendorf), Truso (Wikipedia gives only arc-minutes: 54.07 N, 19.45 E,
  which is the lake; the table's 54.103, 19.48 is nearer the village) and the Lejre halls.
- Hamwic's mint: Wikipedia's "786-858" against the roles table's "none sure"; I trust the roles table.
- Carolingian deniers of Bordeaux under Charlemagne.
