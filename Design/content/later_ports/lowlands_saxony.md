# Later ports: the Low Countries, the Rhine, the Weser and the Elbe

Written 6 October 2026 for Kapman. The port table (`Assets/Kapman/Game/Data/ports_793.json`) was made for 793 and has
fifteen places in this country: Dorestat, Traiectum (Utrecht), Walichrum, Witla, Medemelacha, Winaldum, Doccinga,
Emuthon, Daventre, Noviomagus, Ganda, Andoverpis, Colonia, Mogontia and Diusburg. This page gives 24 more that a
merchant of the house could have put in at in some year between 793 and 1066, ranked, each with a ready row.

## How to read this page

- **Rank.** Must have (10): the game is wrong without it after a certain year. Good to have (10): a true place with
  its own reason to go. Leave out (4): true, but a neighbour already in the table does its work.
- **The row** is in the exact format of `ports_793.json` and can be pasted into its `"ports"` list. All 24 rows are
  together at the foot of the page.
- **realm.** The table has one realm id for this whole country: `franks`. Every row here uses it, so the rows load
  today. Who really ruled, by period, is in each entry; when the realms table gets ids for the East Frankish kingdom,
  Saxony, Lotharingia and Flanders, change them there. Bruggas above all wants a `flanders` realm.
- **region** uses only keys that are already in `goods_793.json`: `rhine_delta`, `frisia`, `zeeland`, `flanders`,
  `rhineland`, `meuse`, `saxony`. (`meuse` and `saxony` are in the goods table but had no port until now.)
- **county** is an id from `lands_793.json` where one fits, else empty.
- **from / to** are the years the place trades; 0 = before the game / no end. Where a place was there in 793 but only
  became a port later, `from` is the year it starts to matter and the entry says so.
- **(?)** marks what I could not check against a source on the day. Latitude and longitude are for the old landing
  or the oldest church, to three decimals, from memory of the maps and not measured afresh: check each against the
  baked rivers before a port is placed (?).
- Nothing here is quoted from a modern translation. Latin words in quotation marks are the charter's or the
  annal's own.

## What this country does in the game's years

1. **To 834.** Dorestat is the one great place. Bardanwic and Magadoburg are the king's border posts (805), Brema and
   Hammaburg are mission churches, Treiectum on the Maas is full of merchants (Einhard), Anternacum ships the
   black stones.
2. **834 to 892, the raiding years.** Dorestat fails. Hammaburg burns in 845, Sanctos in 863, the whole Rhine and
   Maas in 881 to 883. The counts and bishops answer with ring forts: Bruggas, Mitthelburg, Sutfene.
3. **892 to 1000.** The trade of Dorestat goes to Tiele, Daventre and Utrecht. The Saxon kings make markets by
   charter: Brema 888 and 965, Magadoburg 965, Bardanwic. Luniburc boils salt (956).
4. **1000 to 1066.** Tiele is at its height and quarrels with the count at Flardinga (1018). The Frisian mints strike
   for the counts: Staverun, Liunvert, Doccinga. Bruggas is where English exiles wait. Hoium gets the first town
   charter north of the Alps (1066). Hammaburg burns again (1066).

---

# MUST HAVE

## 1. Tiele (Tiel)

- **Name then:** Tiele, Tiela (charters of the 10th century; "Tiela" in the Magdeburg privilege of 975).
- **Modern place:** Tiel, Gelderland, on the Waal where the Linge joined it.
- **Position:** 51.886 N, 5.429 E.
- **Active:** from about 890 (?) to 1174 as a place of far trade; a village with a church before that. In the game:
  from 890, no end.
- **Kind:** the king's toll and market; the heir of Dorestat.
- **Who ruled:** the East Frankish and then the German king (king Zwentibold's toll charter, 896); the land is the
  county of Teisterbant. In 972 Otto II gave Tiele among other places to his bride Theophanu as her morning gift (?).
  The church of Saint Walburg was the bishop of Utrecht's from the 10th century (?). A stone house was built before
  950 and served as the toll house.
- **Traded:** Rhenish wine, Pingsdorf pots and later Paffrath cooking pots, quern stones and tuff down from
  Cologne and Andernach; Frisian cloth, cheese, salt fish and herring up; English wool and tin, and everything of
  London, across the sea. Tiele struck the king's pennies from about 1000, and they turn up in the Baltic hoards.
- **How a boat comes in:** up the Waal from the Maas mouth (past Flardinga, which is the trouble of 1018), or down
  from Noviomagus. She lies on the bank at the Linge mouth; the digs found timbered banks of the 10th and 11th
  centuries with boats drawn up on them (?).
- **Why go:** after 863 this is where a Dorestat house would move. It is the toll every Rhine cargo passes, the
  best market for English goods on the river, and the home of the first merchants' sworn company known in these
  lands.
- **Evidence:** the toll charter of 896; the Magdeburg privilege of 975, which frees Magdeburg's merchants of toll
  everywhere except at Mainz, Cologne, Tiele and Bardanwic; Alpert of Metz (about 1020), who says the merchants of
  Tiele are hard men who judge by their own will and not by the law, claim a charter of the emperor for it, hold
  money in common and drink together at set feasts; the raid of 1006, when Northmen came up the Waal, took the
  stores and burned the place (Alpert; nothing of it has been found in the ground); the complaint of the Tiele men
  to Henry II about the count's toll (about 1015 to 1018); the move of the toll to Kaiserswerth in 1174. The digs at
  Tiel (Tol-Zuid, Koornmarkt) show a trading place of the 10th and 11th centuries tied closely to Cologne.
- **Sources:** Alpert of Metz, De diversitate temporum I.8-10 and II.20-21; MGH DD Otto II no. 112 (975) (?);
  https://nl.wikipedia.org/wiki/Tiel ; the project's `Design/content/economy/cloth_wool.md` and
  `pots_glass_luxuries.md`, which already send goods "by Tiel" and note that it has no port yet.
- **Cards it wants:** the guild ale (1020); the raid of 1006; the count's toll (1015-1018).

```json
  {"id": "tiele", "name": "Tiele", "modern": "Tiel", "lat": 51.886, "lon": 5.429, "realm": "franks", "county": "testerbant", "region": "rhine_delta", "kind": "emporium", "landing": "riverbank", "size": 4, "from": 890, "to": 0, "note": "The king's toll on the Waal at the Linge mouth and the heir of Dorestat; its merchants trade with England and drink as a sworn company (Alpert, c. 1020); burned by Northmen in 1006."},
```

## 2. Staverun (Stavoren)

- **Name then:** Staverun (Annals of Hildesheim, 991); STAVERUN, STAVERON on the coins of the 11th century.
- **Modern place:** Stavoren, Friesland, on the east shore of what was then the Vlie water and the Almere.
- **Position:** 52.884 N, 5.359 E. The old town and Saint Odulf's abbey lay west of the present one and are under
  the IJsselmeer (?).
- **Active:** a church place from about 830; a port from about 950 (?); at its height from 1040 into the 13th century.
  In the game: from 950, no end.
- **Kind:** a Frisian sea town on the inland road between the Rhine mouths and the north.
- **Who ruled:** the Frankish king's counts in Frisia; from about 1038 the Brunon counts (Bruno III, Egbert I,
  Egbert II), who struck pennies here; Egbert I gave the place town rights between 1058 and 1068, perhaps the first
  in what is now the Netherlands, confirmed by Henry V in 1108 (?).
- **Traded:** Frisian cloth, cheese, butter, hides and salt fish out; Rhenish wine, pots and querns in; from about
  1000 the northern goods (furs, wax, whetstones) that come by Sliesthorp and the Wadden shore. Its pennies lie in
  the hoards of the Baltic and of Russia in great numbers (?).
- **How a boat comes in:** from Utrecht by the Vecht and across the Almere, then along the Frisian shore; or from
  the sea through the Vlie. A shelving shore under a low clay cliff (the Rode Klif lies just south): beach her.
- **Why go:** it is the last good market before the open Wadden crossing, and after 1040 the place to change silver
  into the pennies the north takes.
- **Evidence:** Saint Odulf, a canon of Utrecht sent here by bishop Frederik (died about 835-838) to preach; the
  Annals of Hildesheim for 991, which say that pirates laid Staverun waste and other places on the shore; the
  Brunon coins; the town right of 1058-1068.
- **Sources:** Annales Hildesheimenses, 991; Vita Odulfi (?); https://nl.wikipedia.org/wiki/Stavoren ;
  `Design/content/economy/silver_coin.md` (the Frisian mints).

```json
  {"id": "staverun", "name": "Staverun", "modern": "Stavoren", "lat": 52.884, "lon": 5.359, "realm": "franks", "county": "westrachia", "region": "frisia", "kind": "market", "landing": "beach", "size": 2, "from": 950, "to": 0, "note": "Frisian sea town on the Vlie road north, with Saint Odulf's church; laid waste by pirates in 991; the counts strike pennies here from about 1040; town rights 1058-1068."},
```

## 3. Brema (Bremen)

- **Name then:** Brema; Bremon, Bremun (937).
- **Modern place:** Bremen, on a dune ridge on the right bank of the Weser, about 60 km above the sea.
- **Position:** 53.076 N, 8.807 E.
- **Active:** from 787 as a bishop's seat; as a market from 888. In the game: from 0 (it is there in 793, as a
  wooden church among Saxons), no end.
- **Kind:** a bishop's town; from 848 the working seat of the archbishops of Hamburg and Bremen, who hold the
  mission to all the north.
- **Who ruled:** Charlemagne's bishop Willehad (bishop 787, died 789); the bishops under Cologne from 805; the
  archbishops from 848 (Ansgar, then Rimbert 865-888, Adaldag 937-988, Unwan 1013-1029, Adalbert 1043-1072); the
  Billung dukes of Saxony press on them from 961.
- **Traded:** Saxon rye, linen, honey and wax, horses and hides out; Rhenish wine for the altars, quern stones,
  pots, salt and Frisian cloth in. Under Adalbert, says Adam of Bremen, merchants came to Bremen from all parts with
  their wares, until the archbishop's stewards plundered them and they stayed away (?).
- **How a boat comes in:** up the Weser on the flood, past shifting sands; she lies in the Balge, a side arm of the
  river under the cathedral hill, on a sandy bank (?).
- **Why go:** it is the Weser's only market, the door to inner Saxony, and the place where news of the whole north
  is gathered, because every missionary and bishop of the Danes, Swedes and Norwegians is sent from here.
- **Evidence:** the Life of Willehad (first mention, 782); the charter of king Arnulf to Rimbert, 888, giving
  market, mint and toll; Otto I's protection of the market folk in 937 and his charter of 10 August 965 for market,
  mint and toll; the burning of the first cathedral by Danes about 850 or 858 (?); the Hungarian raid of 915; the
  walls strengthened against pirates in 994 and 1020; Adam of Bremen's whole book, written here about 1075.
- **Sources:** Vita Willehadi; MGH DD Arnulf no. 27 (888) (?); MGH DD Otto I no. 307 (965) (?); Adam of Bremen,
  Gesta Hammaburgensis ecclesiae pontificum, books I-III (no safe English: Tschan is 1959);
  https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Bremen .

```json
  {"id": "brema", "name": "Brema", "modern": "Bremen", "lat": 53.076, "lon": 8.807, "realm": "franks", "county": "wigmodia", "region": "saxony", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Willehad's see on the Weser dune (787), the archbishops' seat from 848; market, mint and toll by the king's charter in 888 and again in 965; the mission to the whole north is run from here."},
```

## 4. Hammaburg (Hamburg)

- **Name then:** Hammaburg (first written in 834).
- **Modern place:** Hamburg, the Domplatz, on a spur between the Alster and the Elbe marsh.
- **Position:** 53.549 N, 9.998 E.
- **Active:** a Saxon ring fort of the 8th century; the Franks' fort and church from about 811-822; burned 845;
  rebuilt larger about 900; burned by the Obodrites in 915 (?), 983, 1066 and 1072. In the game: from 811, no end,
  with the burnings as events.
- **Kind:** a border fort with a mission church and a landing; a small place with a great name.
- **Who ruled:** the Frankish king's count of the Nordalbingians; Ansgar as bishop from 831 and archbishop from 832
  or 834; after 848 the archbishops sit mostly at Bremen; from 961 the Billung dukes share the place with them
  (the archbishop's stone house and the duke's fort of the 11th century stand face to face).
- **Traded:** the Slav goods of the east bank and the Baltic road: wax, honey, furs, captives; Saxon grain and
  hides; Rhenish wine, pots and querns from the west. Archbishop Adaldag (937-988) is credited with a market here
  (?). It is the Elbe end of the short land road to Sliesthorp.
- **How a boat comes in:** up the Elbe about 100 km from the sea on the flood, into the Alster mouth. About 820 a
  ditch from the Alster (the later Reichenstrasse fleet) made a sheltered landing under the fort (?). Bank landing.
- **Why go:** for the Slav trade without sailing round Jutland; for the archbishop's letters and passengers going
  north; and because what is cheap here after a burning is dear a year later.
- **Evidence:** Rimbert's Life of Ansgar, chapter 16: in 845 a fleet of Northmen came up the Elbe by surprise, the
  count was away, the place was taken in an evening and burned with the church and the books, and Ansgar got away
  with the relics and little else. The Annals of St Bertin for 845 give king Horik 600 ships. The digs on the
  Domplatz of 2005-2006 (published 2014) found the fort: three phases, the last about 900. Adam of Bremen for 983,
  1066 and 1072.
- **Sources:** Rimbert, Vita Anskarii 12-16 (Robinson's English of 1921 is safe to quote); Annales Bertiniani 845;
  Adam of Bremen II and III; https://de.wikipedia.org/wiki/Hammaburg ;
  https://de.wikipedia.org/wiki/Geschichte_Hamburgs .

```json
  {"id": "hammaburg", "name": "Hammaburg", "modern": "Hamburg (Domplatz)", "lat": 53.549, "lon": 9.998, "realm": "franks", "county": "sturmarii", "region": "saxony", "kind": "fort", "landing": "riverbank", "size": 2, "from": 811, "to": 0, "note": "The Franks' fort and Ansgar's church where the Alster meets the Elbe; burned by a Danish fleet in 845 and by the Obodrites in 983, 1066 and 1072; the Elbe door to the Slav trade."},
```

## 5. Bardanwic (Bardowick)

- **Name then:** Bardaenowic (capitulary of 805); Bardanwic, Bardonwic (975).
- **Modern place:** Bardowick, Lower Saxony, on the Ilmenau, about 12 km above its mouth in the Elbe.
- **Position:** 53.294 N, 10.388 E.
- **Active:** from before 795 to 26-28 October 1189, when Henry the Lion destroyed it. In the game: from 0, no end.
- **Kind:** the chief market of the Saxons on the lower Elbe and the king's post for the trade with the Slavs.
- **Who ruled:** Charlemagne camped in the Bardengau in 795 and 798; in 805 his officer Hredi oversees the
  merchants here (?); from 951 Hermann Billung and the Billung dukes of Saxony; a church of the bishop of Verden.
- **Traded:** to the Slavs: Frankish cloth, salt, wine, iron tools, and, against the law, swords and mail. From the
  Slavs: wax, honey, furs, horses, captives. From 956 the salt of Luniburc goes down the Ilmenau past its quay.
  A mint from about 965 (?).
- **How a boat comes in:** up the Elbe past Hammaburg, into the Ilmenau and a short way up a small quiet river.
  Bank landing below the churches.
- **Why go:** it is as far as the law lets a merchant go toward the Slavs, so the Slavs come here. Arms fetch the
  most and cost the most if found.
- **Evidence:** the capitulary of Thionville (Diedenhofen), 805, chapter 7: merchants going to the Slavs and Avars
  may go as far as nine named posts, of which Bardaenowic is the first, and may carry no arms or mail to sell; a
  load found with them is forfeit, half to the palace and half shared between the officer and the finder. The
  privilege of 975 names Bardonwic with Mainz, Cologne and Tiele as the four places where even Magdeburg's
  merchants pay toll. The project's `stone_metal_arms.md` already has a rumour line set here.
- **Sources:** MGH Capitularia I no. 44, c. 7; Annales regni Francorum 795, 798; MGH DD Otto II no. 112 (?);
  https://de.wikipedia.org/wiki/Bardowick .

```json
  {"id": "bardanwic", "name": "Bardanwic", "modern": "Bardowick", "lat": 53.294, "lon": 10.388, "realm": "franks", "county": "bardengau", "region": "saxony", "kind": "market", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The king's post for trade with the Slavs, on the Ilmenau above the Elbe: by the law of 805 a merchant goes no further east and carries no swords or mail to sell."},
```

## 6. Bruggas (Bruges)

- **Name then:** Bruggas, Bruccia (on pennies of Charles the Bald, before 875); Bricge, Brycge in the English
  Chronicle.
- **Modern place:** Bruges, West Flanders, the Burg square.
- **Position:** 51.209 N, 3.225 E.
- **Active:** from about 865 as the count's fort; a port by 1000. In the game: from 865, no end.
- **Kind:** the fort and then the chief town of the counts of Flanders, with a tidal landing.
- **Who ruled:** Baldwin I Iron Arm (count from 862, fort about 865, died 879); Baldwin II (879-918), who married
  Aelfthryth, king Alfred's daughter; Arnulf I (918-965); Baldwin IV (988-1035); Baldwin V (1035-1067). All hold of
  the West Frankish king, and from 1012 hold Walcheren of the emperor.
- **Traded:** Flemish cloth from about 1000, cheese and salt of the sea marsh; English wool in (a trickle before
  1066); Rhine wine by sea from Tiele and Cologne; the goods of the north by Danish ships in Cnut's time.
- **How a boat comes in:** from the open coast up a tidal creek through the salt marsh to the foot of the fort. The
  creek silts through the 11th century and by about 1050 only small craft reach the town (?), which is no hardship
  for a boat of 7.7 m. (The storm that opened the Zwin is 1134, after the game.) She lies on a mud bank at low
  water.
- **Why go:** it is the nearest safe mainland harbour to England that is not the king of England's and not the
  duke of Normandy's. So it is full of English exiles with silver, news and errands.
- **Evidence:** pennies with BRVGGAS MON of Charles the Bald (864-875); the fort under the Burg; the Praise of
  Queen Emma (written 1041-1042 at Saint-Omer), which says Emma came to Bruges in 1037, that the town is a Flemish
  settlement famous for the coming of merchants and for the plenty of all that men prize, and that count Baldwin
  kept her well. The English Chronicle sends there Swein Godwinson (1047-1049), earl Godwin and his family in 1051
  ("to Baldwin's land"), and Tostig in 1065. Harthacnut joined his mother there in 1039-1040 with a fleet.
- **Sources:** Encomium Emmae Reginae III.7 (paraphrased; Campbell's English of 1949 is not safe to quote);
  Anglo-Saxon Chronicle 1037, 1039, 1047-1051, 1065 (Giles 1847 and Ingram 1823 are safe to quote);
  https://nl.wikipedia.org/wiki/Geschiedenis_van_Brugge .

```json
  {"id": "bruggas", "name": "Bruggas", "modern": "Bruges", "lat": 51.209, "lon": 3.225, "realm": "franks", "county": "flandria", "region": "flanders", "kind": "town", "landing": "beach", "size": 3, "from": 865, "to": 0, "note": "The fort of the counts of Flanders at the head of a tidal creek; pennies before 875; from 1037 the harbour where queen Emma, Godwin and other English exiles wait for better days."},
```

## 7. Flardinga (Vlaardingen)

- **Name then:** Flardinga (?); Flaridingun in the chronicles of 1018 (?).
- **Modern place:** Vlaardingen, South Holland, the church hill, where a creek met the Merwede (the main stream of
  the Maas mouth then).
- **Position:** 51.907 N, 4.342 E.
- **Active:** a church from the 8th century; the count's fort and toll from about 1000. In the game: from 1000, no
  end.
- **Kind:** a count's fort with a toll that the king never granted.
- **Who ruled:** the West Frisian counts later called of Holland: Dirk III (993-1039), Dirk IV (1039-1049), Floris I
  (1049-1061). The emperor says the land is the bishop of Utrecht's.
- **Traded:** little of its own (fish, cattle, cheese). What it has is the river: every boat between Tiele and
  England passes its bank.
- **How a boat comes in:** from the sea into the Maas mouth and up the Merwede on the flood; a small creek harbour
  under a round fort on a mound (a ring 27 m across was found by radar under the church hill in 2007).
- **Why go:** you do not choose to. You pay the count's toll here, or you run it. After 1018 it is also the place
  to be on good terms with the house that won.
- **Evidence:** Alpert of Metz and Thietmar of Merseburg: the merchants of Tiele complained to Henry II that
  count Dirk had built a fort in the marsh and took heavy toll from everyone sailing to the sea. At Easter 1018 at
  Nijmegen the emperor ordered duke Godfrey and bishop Adalbold of Utrecht to throw him out. The fleet went down
  from Tiele; on 29 July 1018 the army landed among the ditches, panicked and was cut down at the boats; the duke
  was taken. Bishop Balderic of Liege died the same day on the way. In 1047 Henry III took the fort and lost men
  getting out again. The digs found a settlement of some seventeen timber houses of the 11th century (?).
- **Sources:** Alpert, De diversitate temporum II.20-21; Thietmar, Chronicon VIII.27-28 (?); Gesta episcoporum
  Cameracensium III; https://en.wikipedia.org/wiki/Battle_of_Vlaardingen .

```json
  {"id": "flardinga", "name": "Flardinga", "modern": "Vlaardingen", "lat": 51.907, "lon": 4.342, "realm": "franks", "county": "masaland", "region": "rhine_delta", "kind": "fort", "landing": "riverbank", "size": 1, "from": 1000, "to": 0, "note": "Count Dirk's fort on the Merwede, taking a toll the king never granted from every boat bound to sea; the emperor's army was cut down here on 29 July 1018."},
```

## 8. Anternacum (Andernach)

- **Name then:** Anternacum, Antunnacum; Andernacha (?).
- **Modern place:** Andernach, on the left bank of the Rhine below Koblenz.
- **Position:** 50.440 N, 7.402 E.
- **Active:** from Roman times without a break. In the game: from 0, no end.
- **Kind:** a royal estate inside old Roman walls, and the harbour of the Eifel stone.
- **Who ruled:** the king (a royal house since the Merovingians); in the middle kingdom from 843, East Frankish
  after the battle of 876; the archbishop of Cologne only from 1167.
- **Traded:** hand querns and millstones of Mayen lava, carted about 25 km from the quarries (?); tuff building
  stone from the Brohl valley and Weibern, which builds the 11th-century churches of Utrecht, Deventer and Tiele (?);
  Mayen pots; wine. This is where the game's black stones are loaded.
- **How a boat comes in:** up the Rhine from Cologne, about 90 km, towed or under sail with a north wind; a quiet
  reach below the Andernach narrows. A hard bank under the wall: the stones are stacked on it.
- **Why go:** querns at the source are the cheapest heavy cargo that every house in Frisia, England and Denmark
  must buy. In 793 the house can buy them at Dorestat; after 863 it must come here or to Cologne.
- **Evidence:** the quarries of Mayen and the quern finds from Dorestat, Ribe, Hedeby, London and York; the
  battle of 8 October 876, where Louis the Younger beat his uncle Charles the Bald before the town; the Northmen
  burned it in 882-883; the battle of 2 October 939, where the rebel dukes Eberhard and Giselbert were caught at
  the crossing and Giselbert drowned in the Rhine. The project's `stone_metal_arms.md` has the trade.
- **Sources:** Annales Fuldenses 876, 883; Regino 876; Widukind II.26 and Liudprand for 939 (?);
  https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Andernach ; Mangartz, Roemischer Basaltlava-Abbau zwischen
  Eifel und Rhein (2008) (?).

```json
  {"id": "anternacum", "name": "Anternacum", "modern": "Andernach", "lat": 50.44, "lon": 7.402, "realm": "franks", "county": "", "region": "rhineland", "kind": "royal seat", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The king's estate in Roman walls and the Rhine harbour of the Eifel: quern stones of Mayen lava, tuff and pots are loaded here; battles before the town in 876 and 939."},
```

## 9. Magadoburg (Magdeburg)

- **Name then:** Magadoburg (805).
- **Modern place:** Magdeburg, the cathedral hill on the left bank of the middle Elbe.
- **Position:** 52.125 N, 11.634 E.
- **Active:** from 805 as a border post; a great place from 937. In the game: from 805, no end.
- **Kind:** the king's post on the Slav border, then Otto the Great's own city and an archbishop's see.
- **Who ruled:** Charlemagne's officer Aito (805); the Saxon dukes who became kings in 919; Otto I, who founded
  Saint Maurice's abbey here in 937, made it an archbishopric in 968 and is buried here (973); the archbishops
  after him (Adalbert 968-981, Giselher, Tagino, Gero).
- **Traded:** Slav captives, wax, honey, furs and horses from across the river; salt from Halle; Saxon silver
  after about 968; and going east cloth, wine, weapons by leave, and church goods for the new sees.
- **How a boat comes in:** this is far: about 300 km up the Elbe above Hammaburg, against a strong stream, with
  sandbars at low water in late summer. Two to three weeks up, towing and poling; a few days down. Bank landing
  below the cathedral rock.
- **Why go:** for a merchant of the Rhine mouth it is the far end of the German world. Slav goods are cheapest here,
  and from 965 the market is under the king's peace. A voyage for a house that has grown.
- **Evidence:** the capitulary of 805 (Aito oversees the merchants at Magadoburg); a fort built across the Elbe
  from it in 806; Otto I's charter of 965 giving Saint Maurice's the market, mint and toll, and the ban over "the
  Jews and the other merchants living there"; Otto II's privilege of 975, which frees the merchants of Magdeburg
  from toll in the whole kingdom except at Mainz, Cologne, Tiele and Bardanwic; Ibrahim ibn Yaqub, who came to
  Otto's court about 965 and wrote down the roads and prices of the Slav lands (?); the Slav rising of 983, which
  took Havelberg and Brandenburg and stopped at the Elbe.
- **Sources:** MGH Capitularia I no. 44; Annales regni Francorum 806; MGH DD Otto I no. 300 (965) (?); MGH DD
  Otto II no. 112 (975) (?); Thietmar, Chronicon II-III; https://de.wikipedia.org/wiki/Magdeburg .

```json
  {"id": "magadoburg", "name": "Magadoburg", "modern": "Magdeburg", "lat": 52.125, "lon": 11.634, "realm": "franks", "county": "ostfalahi", "region": "saxony", "kind": "town", "landing": "riverbank", "size": 4, "from": 805, "to": 0, "note": "The king's border post on the middle Elbe (805), then Otto the Great's city: abbey 937, market and mint 965, archbishop 968; Slav wax, furs and captives; three weeks upstream from Hammaburg."},
```

## 10. Treiectum (Maastricht)

- **Name then:** Treiectum (Einhard); Traiectum; TRIECTO on coins. (The table already has a Traiectum, which is
  Utrecht: keep this spelling apart.)
- **Modern place:** Maastricht, at the old Roman bridge over the Maas.
- **Position:** 50.849 N, 5.689 E.
- **Active:** from Roman times without a break. In the game: from 0, no end.
- **Kind:** a bridge town with a famous saint, a mint and a toll.
- **Who ruled:** the Frankish king; Saint Servatius's abbey is the king's own, and Alcuin and then Einhard hold it
  as lay abbots; the middle kingdom from 843, Lotharingia, the German king from 925; in 908 king Louis the Child
  gives mint and toll to bishop Stephen of Liege; after that the town is shared between king and bishop.
- **Traded:** the Maas country's goods: brass and copper work, iron, grain, wine of the valley, building stone and
  slates coming down from the Ardennes; salt, fish, cloth and English goods coming up from Witla and Dorestat.
- **How a boat comes in:** up the Maas from the delta, about 250 km of a river that is shallow in summer and wild in
  flood. The stone bridge stops a mast; the landing is the gravel bank below it on the west side.
- **Why go:** it opens the `meuse` region, which the goods table already has and no port serves. Brass kettles and
  blades bought here sell in every northern port.
- **Evidence:** Einhard, in his account of carrying the relics of Marcellinus and Peter (about 830), calls it a
  place with a great many people living in it, most of all merchants; the mint of the 7th century (a dozen
  moneyers) and Carolingian pennies; the Northmen who wintered at Asselt plundered it in 881; the charter of 908.
- **Sources:** Einhard, Translatio et miracula SS. Marcellini et Petri IV.13 (?); Annales Fuldenses 881;
  Regino 881; https://nl.wikipedia.org/wiki/Geschiedenis_van_Maastricht .

```json
  {"id": "treiectum", "name": "Treiectum", "modern": "Maastricht", "lat": 50.849, "lon": 5.689, "realm": "franks", "county": "hasbania", "region": "meuse", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The bridge town of Saint Servatius on the Maas, thick with merchants in Einhard's day; mint and toll; plundered by the Northmen in 881."},
```

---

# GOOD TO HAVE

## 11. Sutfene (Zutphen)

- **Name then:** Sutfene, Sutfeno (a Corvey record of about 1053-1071); earlier forms are not known (?).
- **Modern place:** Zutphen, Gelderland, on a river dune where the Berkel runs into the IJssel.
- **Position:** 52.139 N, 6.196 E.
- **Active:** a settlement since the 4th century; a count's fort from about 890. In the game: from 890, no end.
- **Kind:** a count's seat inside a ring fort, 15 km upstream of Daventre.
- **Who ruled:** the counts of Hamaland (the house of Wichman and Meginhard in the 9th and 10th centuries) (?); the
  emperor Henry III gave the place to the church of Utrecht in 1046; a palace and the great church of Saint Walburg
  were built about 1050; counts of Zutphen by name from 1101.
- **Traded:** the cattle, hides, rye and timber of the Saxon east, coming down the Berkel; Rhine wine and pots in.
  It lives beside Daventre and feeds it.
- **How a boat comes in:** up the IJssel from Daventre, or down from the Rhine fork above Arnhem. She lies in the
  Berkel mouth under the wall, on the bank.
- **Why go:** a count at home is a buyer of wine, cloth and arms, and a court where a debt can be pleaded. In the
  raid years it is the nearest wall to Daventre.
- **Evidence:** the digs on the Gravenhof (1997 and after): a burned house with the bones of people and animals
  killed in it, laid to the Northmen who took Daventre in 882; then a ring wall with a U-shaped ditch 20 m wide,
  whose line the market streets still follow; a stone hall of the 11th century, 54 m long (?).
- **Sources:** Annales Fuldenses 882; M. Groothedde, Een vorstelijke palts te Zutphen? (2013) (?);
  https://nl.wikipedia.org/wiki/Geschiedenis_van_Zutphen ; `Design/content/dossiers/daventre.md`.

```json
  {"id": "sutfene", "name": "Sutfene", "modern": "Zutphen", "lat": 52.139, "lon": 6.196, "realm": "franks", "county": "hamaland", "region": "rhine_delta", "kind": "royal seat", "landing": "riverbank", "size": 2, "from": 890, "to": 0, "note": "The counts' ring fort at the Berkel mouth on the IJssel, raised after the Northmen's raid of 882; given to the church of Utrecht in 1046; a palace about 1050."},
```

## 12. Sanctos (Xanten)

- **Name then:** ad Sanctos, Sanctos ("at the Saints"); Xanctum (967); Troia on the archbishops' coins of
  1036-1056.
- **Modern place:** Xanten, on the lower Rhine; the church of Saint Victor.
- **Position:** 51.662 N, 6.454 E.
- **Active:** from the 8th century. In the game: from 0, no end.
- **Kind:** a church of canons over the graves of martyrs, with a market at its gate.
- **Who ruled:** the provost and canons under the archbishop of Cologne; in the middle kingdom from 843, the
  German king from 925; under archbishop Brun, the king's brother, from 953.
- **Traded:** wine and wax for the church, Rhine pots, the grain and cattle of the lower Rhine; pilgrims. A mint
  of the archbishops in the 11th century (the SCA TROIA pennies).
- **How a boat comes in:** two days below Cologne. The Rhine then ran close under the place (?) and has moved
  since; bank landing. The old Roman town a little north is a quarry of cut stone.
- **Why go:** it is the one stop of note on the long reach between Diusburg and Noviomagus, and it has a story a
  card can carry.
- **Evidence:** the Annals of Xanten: in the winter of 863-864 the heathen came up the Rhine in a great flood,
  burned the church of Saint Victor and took what was in it, and the provost fled by night to Cologne with the
  saint's bones; the Northmen lay on an island in the river nearby. The battle of Birten, 939, a short walk
  away, where Otto I's men beat his brother Henry while the king watched from the other bank.
- **Sources:** Annales Xantenses 864; Widukind II.17; https://de.wikipedia.org/wiki/Xanten ; the realm file
  `Design/content/realms/frisia_lotharingia.json` (the annals of Xanten are used there).

```json
  {"id": "sanctos", "name": "Sanctos", "modern": "Xanten", "lat": 51.662, "lon": 6.454, "realm": "franks", "county": "hattuaria", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The canons' church of Saint Victor on the lower Rhine, with a market at its gate; burned by the Northmen in the winter of 863-864, the saint's bones carried to Cologne by night."},
```

## 13. Wormatia (Worms)

- **Name then:** Wormatia, Wormacia.
- **Modern place:** Worms, on the left bank of the upper Rhine, 60 km above Mainz.
- **Position:** 49.630 N, 8.360 E.
- **Active:** from Roman times. In the game: from 0, no end.
- **Kind:** a bishop's city with a royal palace; a Frisian merchants' quarter by the river.
- **Who ruled:** the king (Charlemagne kept winter here often before the palace burned in the winter of 790-791);
  assemblies of the realm in 829 and 926; the Salian counts until bishop Burchard (1000-1025) bought them out and
  had their tower pulled down (?); the bishop after that. Cathedral hallowed in 1018.
- **Traded:** wine above all; grain; and the Frisians bring up cloth, fish and the goods of the north. Jews are
  known here from about 960, with a synagogue in 1034.
- **How a boat comes in:** above Mainz the Rhine is broad, braided and easy but slow: three or four days up from
  Mainz with a tow. To come this far she has passed the rocks at Bingen. Bank landing at the Rhine gate.
- **Why go:** wine is cheaper here than at Mainz, and a Frisian finds his own people, his own tongue and credit.
- **Evidence:** the wall-building order of Worms (about 900, laid to bishop Thietlach) shares out the upkeep of
  the town wall: the Frisians hold the stretch from the "Frisian point" to the Rhine gate. On 18 January 1074
  Henry IV freed the men of Worms from toll at the king's toll places (after the game, but it shows what they
  had become).
- **Sources:** Annales regni Francorum 790-791; the Wormser Mauerbauordnung (Quellen zur Geschichte der Stadt
  Worms, ed. Boos, III) (?); Vita Burchardi; https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Worms .

```json
  {"id": "wormatia", "name": "Wormatia", "modern": "Worms", "lat": 49.63, "lon": 8.36, "realm": "franks", "county": "", "region": "rhineland", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Bishop's city and king's palace on the upper Rhine; the Frisian merchants keep their own stretch of the town wall (c. 900); wine at the source."},
```

## 14. Confluentes (Koblenz)

- **Name then:** Confluentes; Cobolenza (?).
- **Modern place:** Koblenz, where the Moselle meets the Rhine; the church of Saint Castor on the point.
- **Position:** 50.362 N, 7.604 E.
- **Active:** from Roman times. In the game: from 0, no end.
- **Kind:** a royal estate and church at the meeting of two rivers, with the best-known toll on the Rhine.
- **Who ruled:** the king; Saint Castor's was hallowed in 836; the envoys of Charlemagne's three grandsons met in
  it in October 842 to share out the realm, and the kings made peace there in June 860; Henry II gave the estate
  to the archbishop of Trier in 1018; the archbishop gave the toll to Saint Simeon's at Trier in 1042 (?).
- **Traded:** Moselle wine comes out here; everything on the Rhine passes. The toll list tells what was on the
  boats.
- **How a boat comes in:** a day above Anternacum. A calm landing in the Moselle mouth behind the point; the
  Rhine itself runs hard past it.
- **Why go:** Moselle wine without going up the Moselle; and above here the gorge begins, so it is the last easy
  water. A cautious skipper sells here and turns round.
- **Evidence:** the Koblenz toll list, written down in a charter of 1104 from an older custom of the 11th
  century (?). It names the home ports of the boats and takes the toll in each town's own goods: the men of the
  Maas towns (Huy, Dinant, Namur, Liege) give brass kettles and basins; the men of the lower river (Tiele,
  Utrecht, Daventre, Antwerp, Flanders) give cheese, fish and wine by measure; a man selling swords gives one
  sword in ten (?); four pennies are due on each bought slave (?). The Northmen laid the place waste in April 882.
- **Sources:** Mittelrheinisches Urkundenbuch I no. 409 (1104) (?); Annales Fuldenses 842, 860, 882;
  https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Koblenz .
- **Note for the checker:** the payments by town are from memory and every one wants checking before a card
  quotes it.

```json
  {"id": "confluentes", "name": "Confluentes", "modern": "Koblenz", "lat": 50.362, "lon": 7.604, "realm": "franks", "county": "", "region": "rhineland", "kind": "royal seat", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The king's estate and Saint Castor's church where the Moselle meets the Rhine; a toll taken in each town's own goods; the archbishop of Trier's from 1018."},
```

## 15. Stethu (Stade)

- **Name then:** Stethu (Thietmar, for 994).
- **Modern place:** Stade, Lower Saxony, on the Schwinge, 5 km from the Elbe, below Hamburg on the south bank.
- **Position:** 53.602 N, 9.477 E.
- **Active:** a fort with a landing 4 km upstream from about 650 to 800; the present place from about 800, with a
  fort about 900 and a built harbour about 1000. In the game: from 900, no end.
- **Kind:** a count's fort and harbour at the first firm ground on the lower Elbe.
- **Who ruled:** the counts of Stade (the Udonids: Henry the Bald, died 976; Henry the Good; Siegfried); in 1038
  the emperor Conrad II let the archbishop of Hamburg and Bremen set up a market with mint and toll on church
  ground here.
- **Traded:** the same Elbe goods as Hammaburg, on the safer bank: grain, cattle, hides, salt, and what comes
  across from the Slavs and down from Bardanwic.
- **How a boat comes in:** up the Elbe on the flood and into the Schwinge, a narrow tidal creek; the harbour is a
  basin at the foot of the fort hill. Bank and, from about 1000, timber quay.
- **Why go:** when Hammaburg is burned, and it often is, Stethu is where the Elbe trade lies.
- **Evidence:** Thietmar of Merseburg, whose mother was the counts' sister: in 994 pirates came up the Elbe; the
  counts Henry, Udo and Siegfried met them in boats on 23 June and lost; Udo was killed, the others taken and held
  for a great ransom; Siegfried got his guards drunk and slipped away, and the pirates then cut the noses, ears
  and hands off the hostages and threw them on the shore at Stethu. Thietmar himself, a boy, was to have gone as
  a hostage for his uncle. The digs found the early fort and the harbour.
- **Sources:** Thietmar, Chronicon IV.23-25; MGH DD Konrad II no. 278 (1038) (?); https://de.wikipedia.org/wiki/Stade .

```json
  {"id": "stethu", "name": "Stethu", "modern": "Stade", "lat": 53.602, "lon": 9.477, "realm": "franks", "county": "", "region": "saxony", "kind": "fort", "landing": "riverbank", "size": 2, "from": 900, "to": 0, "note": "The counts' fort and creek harbour on the south bank of the lower Elbe; pirates beat the counts here in 994 and maimed their hostages; market, mint and toll in 1038."},
```

## 16. Cruoninga (Groningen)

- **Name then:** villa Cruoninga (1040).
- **Modern place:** Groningen, on the north tip of the Hondsrug sand ridge, between the Aa and the Hunze.
- **Position:** 53.219 N, 6.568 E.
- **Active:** a village with a wooden church from about 800, a stone one about 925-950; a market by 1040. In the
  game: from 1000, no end.
- **Kind:** a market village where the sand meets the clay.
- **Who ruled:** the king's estate in the land of Drenthe; on 21 May 1040 (?) Henry III gave it to the church of
  Utrecht, with a mint; the bishop's prefect after that.
- **Traded:** the cattle, butter, cheese and cloth of the Frisian clay against the rye, timber and peat of the
  sand; Rhine pots and querns in.
- **How a boat comes in:** from the Wadden Sea up the winding tidal Hunze, about 30 km; only a small boat does it
  easily. Bank landing.
- **Why go:** it is the one market where a boat reaches the Drenthe farmers, and it fills the long gap in the
  table between Doccinga and Emuthon.
- **Evidence:** the charter of 1040; the two old churches (Saint Martin, Saint Walburg); finds of coins and
  pots of the 9th to 11th centuries under the Grote Markt.
- **Sources:** MGH DD Heinrich III no. 45 (1040) (?); https://nl.wikipedia.org/wiki/Geschiedenis_van_Groningen .

```json
  {"id": "cruoninga", "name": "Cruoninga", "modern": "Groningen", "lat": 53.219, "lon": 6.568, "realm": "franks", "county": "thrianta", "region": "frisia", "kind": "market", "landing": "riverbank", "size": 2, "from": 1000, "to": 0, "note": "A market village on the tip of the sand ridge, reached from the Wadden by the tidal Hunze; the king's estate, given with a mint to the church of Utrecht in 1040."},
```

## 17. Luniburc (Lueneburg)

- **Name then:** Luniburc (956); perhaps the Hliuni of the royal annals for 795 (?).
- **Modern place:** Lueneburg, on the Ilmenau, 6 km above Bardowick.
- **Position:** 53.249 N, 10.401 E.
- **Active:** from about 950. In the game: from 956, no end.
- **Kind:** a duke's castle on a chalk hill, a monastery, and a salt spring.
- **Who ruled:** Hermann Billung (margrave from 936, in charge of Saxony from 951 or 961, died 973) and the Billung
  dukes after him: Bernhard I, Bernhard II, Ordulf (1059-1072).
- **Traded:** boiled brine salt, the only great salt source between the Rhineland and the Baltic. In the goods
  table this is `brine_salt`.
- **How a boat comes in:** up the Ilmenau past Bardanwic; the last kilometres are shallow and are poled. Many
  buyers stop at Bardanwic and let the salt come down to them.
- **Why go:** salt at the pan. Every fish barrel of the Elbe and, later, of the Baltic herring is filled with it.
- **Evidence:** on 13 August 956 (?) Otto I gave the toll of the salt works at Luniburc to the monastery of
  Saint Michael on the castle hill ("teloneum ad Luniburc"). The town itself only passes Bardowick after 1189.
- **Sources:** MGH DD Otto I no. 183 (956) (?); https://de.wikipedia.org/wiki/L%C3%BCneburg ;
  `Design/content/economy/wine_salt.md`.

```json
  {"id": "luniburc", "name": "Luniburc", "modern": "Lueneburg", "lat": 53.249, "lon": 10.401, "realm": "franks", "county": "bardengau", "region": "saxony", "kind": "market", "landing": "riverbank", "size": 2, "from": 956, "to": 0, "note": "The Billung dukes' castle on the chalk hill and the brine spring below it; the king gave the salt toll to Saint Michael's in 956; the salt goes down the Ilmenau past Bardanwic."},
```

## 18. Hoium (Huy)

- **Name then:** Hoius vicus (7th century); Hoium, Hoio; Hogge in the English toll list.
- **Modern place:** Huy, on the Maas between Namur and Liege, under a rock.
- **Position:** 50.519 N, 5.240 E.
- **Active:** from the 7th century. In the game: from 0, no end.
- **Kind:** a river town of metalworkers and far-going merchants.
- **Who ruled:** the king; a county of Huy in the 10th century, given to bishop Notger of Liege in 985 (?); the
  bishops of Liege after that. On 26 August 1066 (?) bishop Theoduin sold the townsmen a charter of freedoms,
  the oldest town charter known north of the Alps, in return for half and then a third of their movable goods to
  rebuild the church of Our Lady.
- **Traded:** brass and copper kettles and basins, tin-bronze, iron; wine of the valley; wool cloth later. They
  buy copper in Saxony (from the Harz, after about 970) and tin in England (?).
- **How a boat comes in:** 30 km above Liege, a week or more above Treiectum on a shallow river with gravel bars;
  towing all the way. Bank landing below the bridge.
- **Why go:** kettles and basins are light for their price and wanted everywhere from London to Birka.
- **Evidence:** a Merovingian mint; the London toll rules of king Aethelred (about 1000) name the men of Huy,
  Liege and Nivelles, who pay a fee for showing their goods and then the toll; the Koblenz toll takes the Huy
  men's due in kettles (?); the charter of 1066, which falls in the game's last summer.
- **Sources:** IV Aethelred 2 (Liebermann, Gesetze der Angelsachsen I); the charter of 1066 (text lost, known
  from a later copy) (?); https://en.wikipedia.org/wiki/Huy ; A. Joris, La ville de Huy au moyen age (1959) (?).

```json
  {"id": "hoium", "name": "Hoium", "modern": "Huy", "lat": 50.519, "lon": 5.24, "realm": "franks", "county": "hasbania", "region": "meuse", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "A town of brass and copper workers under a rock on the Maas; its men pay toll at London about 1000; the bishop of Liege sells it a charter of freedoms in 1066."},
```

## 19. Werda (Kaiserswerth)

- **Name then:** Werda, Werid ("the island"); Saint Suitbert's island.
- **Modern place:** Kaiserswerth, in Duesseldorf, then an island in the Rhine.
- **Position:** 51.300 N, 6.733 E.
- **Active:** from about 700. In the game: from 0, no end.
- **Kind:** a monastery on a river island; from 1045 an emperor's palace.
- **Who ruled:** the monks and then canons of Saint Suitbert (the English missionary who died here in 713), under
  the king's protection from 877; Henry III built a palace about 1045-1050; the Rhine toll of Tiele was moved here
  in 1174.
- **Traded:** an abbey's needs: wine, wax, cloth, fish. A stopping place, not a market.
- **How a boat comes in:** a short day below Cologne, a short day above Diusburg. Land on the island's lee side,
  on the bank.
- **Why go:** for the one great scene. In April 1062 archbishop Anno of Cologne came to the palace by ship, asked
  the boy king Henry IV, eleven years old, aboard to see the fine new boat, and had the oars put out. The boy
  jumped into the Rhine and count Ekbert pulled him out. Anno took him to Cologne and ruled in his name. A
  merchant boat lying at the island that morning sees it.
- **Evidence:** Bede on Suitbert (Ecclesiastical History V.11); the charter of protection of 877; Lampert of
  Hersfeld for 1062.
- **Sources:** Bede, Historia ecclesiastica V.11; Lampert of Hersfeld, Annales, 1062;
  https://de.wikipedia.org/wiki/Kaiserswerth .

```json
  {"id": "werda", "name": "Werda", "modern": "Kaiserswerth, Duesseldorf", "lat": 51.3, "lon": 6.733, "realm": "franks", "county": "", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 1, "from": 0, "to": 0, "note": "Saint Suitbert's monastery on an island in the Rhine; the emperor's palace from about 1045, where archbishop Anno carried off the boy king by ship in 1062."},
```

## 20. Mitthelburg (Middelburg)

- **Name then:** Mitthelburg ("the middle fort"); "Mitthelburgensis portus" in 1103.
- **Modern place:** Middelburg, Walcheren; the abbey stands inside the old ring.
- **Position:** 51.500 N, 3.614 E.
- **Active:** the ring fort from about 880-890; a trading place in the 10th century. In the game: from 890, no end.
- **Kind:** one of the round refuge forts of the Zeeland coast (with Domburg, Oost-Souburg, Oostburg, Burgh),
  which became the island's market when Walichrum on the dunes was given up.
- **Who ruled:** the Frankish and then German king; the emperor made it the seat of Walcheren and Beveland soon
  after 1000; from 1012 the count of Flanders (Baldwin IV) holds the islands of the emperor.
- **Traded:** the island's salt (boiled from peat ash), cheese, wool and madder (?); English and Flemish goods.
- **How a boat comes in:** from the Scheldt mouth up the Arne, a tidal creek, to the foot of the ring wall. Mud
  bank at low water.
- **Why go:** it takes over from Walichrum, which the table ends at 850. Without it Zeeland has no port for two
  centuries.
- **Evidence:** the ring wall, about 220 m across, still in the street plan; the dig at the sister fort of
  Oost-Souburg; the church of Saint Martin of the second half of the 10th century; Thiofrid of Echternach, who in
  1103 calls it a harbour.
- **Sources:** Thiofrid, Vita Willibrordi (1103); R. van Heeringen and others, Vroeg-Middeleeuwse ringwalburgen
  in Zeeland (1995) (?); https://nl.wikipedia.org/wiki/Geschiedenis_van_Middelburg ;
  `Design/content/dossiers/walichrum.md`.

```json
  {"id": "mitthelburg", "name": "Mitthelburg", "modern": "Middelburg, Walcheren", "lat": 51.5, "lon": 3.614, "realm": "franks", "county": "walacria", "region": "zeeland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 890, "to": 0, "note": "The middle one of Walcheren's round refuge forts, raised against the Northmen about 890; the island's market after Walichrum, reached by the tidal Arne."},
```

---

# LEAVE OUT (true places, but a neighbour does their work)

## 21. Bunna (Bonn)

- **Name then:** Bunna, vicus Bunnensis; the church quarter is "villa Basilica", later Verona.
- **Modern place:** Bonn; the minster of Saints Cassius and Florentius.
- **Position:** 50.733 N, 7.100 E.
- **Active:** from Roman times. From 0, no end.
- **Kind:** a church of canons with a small merchants' street.
- **Who ruled:** the provost of Saint Cassius under the archbishop of Cologne.
- **Traded:** wine; the potteries of Badorf and Pingsdorf lie between here and Cologne, but their pots are sold
  at Cologne.
- **How a boat comes in:** half a day above Cologne; bank.
- **Why go / why not:** nothing here that Colonia, 25 km down, does not do better. Burned by the Northmen in 882
  and 883 and held by them in 891 (?): use it as a line in the chronicle.
- **Sources:** Annales Fuldenses 881-883; Regino 881, 892; https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Bonn .

```json
  {"id": "bunna", "name": "Bunna", "modern": "Bonn", "lat": 50.733, "lon": 7.1, "realm": "franks", "county": "ripuaria", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The canons' church of Saint Cassius with a merchants' street beside it, half a day above Cologne; burned by the Northmen in 882 and 883."},
```

## 22. Niusa (Neuss)

- **Name then:** Niusa, Niusi; Novesium in the learned.
- **Modern place:** Neuss, on the left bank where the Erft meets the Rhine.
- **Position:** 51.198 N, 6.692 E.
- **Active:** from Roman times. From 0, no end.
- **Kind:** an old fort with a convent; a toll of the archbishop of Cologne in the 11th century (?).
- **Who ruled:** the archbishop of Cologne. The relics of Saint Quirinus came from Rome in 1050 (?).
- **Traded:** grain, cattle; a crossing place.
- **How a boat comes in:** half a day below Cologne; bank.
- **Why go / why not:** it lies between Colonia and Werda and adds nothing to either. The Northmen wintered on an
  island by the fort in 863 (?) and burned it in 881.
- **Sources:** Annales Bertiniani 863; Annales Fuldenses 881; https://de.wikipedia.org/wiki/Neuss (not fetched) (?).

```json
  {"id": "niusa", "name": "Niusa", "modern": "Neuss", "lat": 51.198, "lon": 6.692, "realm": "franks", "county": "", "region": "rhineland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "An old Roman fort and convent at the Erft mouth, the archbishop of Cologne's; burned by the Northmen in 881 (?)."},
```

## 23. Liunvert (Leeuwarden)

- **Name then:** villa Lintarwde (a Fulda record, 8th or 9th century); LIVNVERT on pennies of count Bruno III
  (1038-1057).
- **Modern place:** Leeuwarden; three terps (Oldehove, Nijehove, Hoek) on the east shore of the Middelzee.
- **Position:** 53.203 N, 5.790 E.
- **Active:** terps since Roman times; a mint about 1040-1060. From 1000, no end.
- **Kind:** terp villages with a church of Corvey abbey (Saint Vitus) and a count's mint.
- **Who ruled:** the Brunon counts in the 11th century; Corvey holds land.
- **Traded:** cattle, cheese, cloth; its pennies go to the Baltic.
- **How a boat comes in:** into the Middelzee from the Wadden and up to the terp foot on the tide; beach.
- **Why go / why not:** Doccinga, 20 km away and already in the table, is the same kind of place with the same
  mint and a better story. Add Liunvert only if the Middelzee is drawn.
- **Sources:** https://nl.wikipedia.org/wiki/Geschiedenis_van_Leeuwarden ; `Design/content/economy/silver_coin.md`.

```json
  {"id": "liunvert", "name": "Liunvert", "modern": "Leeuwarden", "lat": 53.203, "lon": 5.79, "realm": "franks", "county": "austrachia", "region": "frisia", "kind": "village", "landing": "beach", "size": 1, "from": 1000, "to": 0, "note": "Three terps on the shore of the Middelzee with a church of Corvey; count Bruno strikes pennies here about 1040-1057."},
```

## 24. Spira (Speyer)

- **Name then:** Spira; Nemetum in the learned.
- **Modern place:** Speyer, on the left bank of the upper Rhine, 40 km above Worms.
- **Position:** 49.317 N, 8.442 E.
- **Active:** from Roman times; great only after 1030. From 0, no end.
- **Kind:** a bishop's city; from 1030 the burial church of the Salian emperors.
- **Who ruled:** the bishop (market and mint 946, immunity 969); the emperors Conrad II and Henry III build the
  cathedral from 1030 (hallowed 1061).
- **Traded:** wine; a builders' market for stone, lead, timber and lime while the cathedral rises.
- **How a boat comes in:** two days above Worms; bank. The Rhine harbour is first named in 1084.
- **Why go / why not:** too far up for a boat from the sea, and Wormatia has the Frisians. Its great days (the
  Jews invited in 1084, the harbour) fall after 1066.
- **Sources:** https://de.wikipedia.org/wiki/Geschichte_der_Stadt_Speyer .

```json
  {"id": "spira", "name": "Spira", "modern": "Speyer", "lat": 49.317, "lon": 8.442, "realm": "franks", "county": "", "region": "rhineland", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Bishop's city on the upper Rhine; the emperors build their burial church here from 1030; wine, and stone and lead for the builders."},
```

---

# Looked at and not written up

These are true trading or landing places of the country in the game's years. They are named here so nobody has to
look again; none has a row.

| Place | Then | Why not now |
|---|---|---|
| Zaltbommel | Bomela | A toll given to Utrecht in 999 (?); Tiele, 15 km away, does its work. |
| Heerewaarden | Herwerde | A toll place between Maas and Waal, named with Tiele in 972 (?). |
| Arnhem, Rhenen, Muiden | Arneym, Hreni, Amuthon | Estates and fish tolls of the Utrecht church (lists of about 870-953); landings only. |
| Rijnsburg, Egmond | Rinasburg, Hecmunde | The Holland counts' fort and abbey; no market known before 1066. |
| Dordrecht | Thuredrith | First named for 1049, when count Dirk IV was killed there; a town only after 1120. |
| Oost-Souburg, Domburg, Oostburg | Sutburg, Duinburg, Osburg | Sister ring forts of Mitthelburg; refuges, not markets. |
| Jever | Gefri | Billung pennies of the 11th century; Emuthon is near. |
| Esesfelth (Itzehoe) | Esesfelth | Charlemagne's fort on the Stoer, 810; a fort only. |
| Schezla | Schezla | The second post of 805; nobody knows where it lay. |
| Verden, Minden | Fardium, Mimida | Bishops' seats on the Aller and upper Weser; Minden has market right 977 (?); too far up a hard river. |
| Corvey and Hoexter | Corbeia nova | The first market and mint charter east of the Rhine (833) (?); 250 km up the Weser. |
| Soest, Dortmund | Sosat, Throtmanni | On the Hellweg road, not on water. |
| Trier | Treveris | The Moselle city; market cross 958; Confluentes sells its wine. |
| Frankfurt | Franconofurd | The king's palace on the Main; Charlemagne held the great synod there in 794. Worth a row if the Main is baked. |
| Strasbourg | Strazburg | Its bishop's men had toll freedom everywhere but Quentovic, Dorestat and Sluis (775, 831); too far for the map (?). |
| Liege, Dinant, Namur | Leodium, Deonant, Namucum | The other Maas towns. Hoium stands for them; add them if the Maas is baked above Treiectum. |
| Tournai, Valenciennes, Saint-Omer, Arras | Tornacum, Valentianas, Sithiu, Atrebatis | Scheldt and Flanders ports; they belong with the Flanders and Francia page, not this one. |

# What the shared tables would need

- Realm ids for the years after 843: `east_franks` (from 843, 870 and 925 by stages), `lotharingia` (843-925),
  `saxony` as a duchy, `flanders` (from 862), and `holland` or `west_frisia` for Flardinga from about 1000.
- Tiele wants `to` on its toll, not on the port: the toll leaves in 1174, after the game.
- Events with a port id: the burnings of Hammaburg (845, 983, 1066), Sanctos (863-864), Treiectum and the Rhine
  towns (881-883), Staverun (991), Stethu (994), Tiele (1006); the battle at Flardinga (1018); the charter of
  Hoium (1066); the carrying off of the king at Werda (1062).
- Three rows change an old one: Mitthelburg follows Walichrum (ends 850); Tiele follows Dorestat (ends 863);
  Sutfene shelters Daventre after 882.

# All 24 rows

Paste into the `"ports"` list of `ports_793.json` (the last row here ends with a comma: take it off if it becomes the
last in the list).

```json
  {"id": "tiele", "name": "Tiele", "modern": "Tiel", "lat": 51.886, "lon": 5.429, "realm": "franks", "county": "testerbant", "region": "rhine_delta", "kind": "emporium", "landing": "riverbank", "size": 4, "from": 890, "to": 0, "note": "The king's toll on the Waal at the Linge mouth and the heir of Dorestat; its merchants trade with England and drink as a sworn company (Alpert, c. 1020); burned by Northmen in 1006."},
  {"id": "staverun", "name": "Staverun", "modern": "Stavoren", "lat": 52.884, "lon": 5.359, "realm": "franks", "county": "westrachia", "region": "frisia", "kind": "market", "landing": "beach", "size": 2, "from": 950, "to": 0, "note": "Frisian sea town on the Vlie road north, with Saint Odulf's church; laid waste by pirates in 991; the counts strike pennies here from about 1040; town rights 1058-1068."},
  {"id": "brema", "name": "Brema", "modern": "Bremen", "lat": 53.076, "lon": 8.807, "realm": "franks", "county": "wigmodia", "region": "saxony", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Willehad's see on the Weser dune (787), the archbishops' seat from 848; market, mint and toll by the king's charter in 888 and again in 965; the mission to the whole north is run from here."},
  {"id": "hammaburg", "name": "Hammaburg", "modern": "Hamburg (Domplatz)", "lat": 53.549, "lon": 9.998, "realm": "franks", "county": "sturmarii", "region": "saxony", "kind": "fort", "landing": "riverbank", "size": 2, "from": 811, "to": 0, "note": "The Franks' fort and Ansgar's church where the Alster meets the Elbe; burned by a Danish fleet in 845 and by the Obodrites in 983, 1066 and 1072; the Elbe door to the Slav trade."},
  {"id": "bardanwic", "name": "Bardanwic", "modern": "Bardowick", "lat": 53.294, "lon": 10.388, "realm": "franks", "county": "bardengau", "region": "saxony", "kind": "market", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The king's post for trade with the Slavs, on the Ilmenau above the Elbe: by the law of 805 a merchant goes no further east and carries no swords or mail to sell."},
  {"id": "bruggas", "name": "Bruggas", "modern": "Bruges", "lat": 51.209, "lon": 3.225, "realm": "franks", "county": "flandria", "region": "flanders", "kind": "town", "landing": "beach", "size": 3, "from": 865, "to": 0, "note": "The fort of the counts of Flanders at the head of a tidal creek; pennies before 875; from 1037 the harbour where queen Emma, Godwin and other English exiles wait for better days."},
  {"id": "flardinga", "name": "Flardinga", "modern": "Vlaardingen", "lat": 51.907, "lon": 4.342, "realm": "franks", "county": "masaland", "region": "rhine_delta", "kind": "fort", "landing": "riverbank", "size": 1, "from": 1000, "to": 0, "note": "Count Dirk's fort on the Merwede, taking a toll the king never granted from every boat bound to sea; the emperor's army was cut down here on 29 July 1018."},
  {"id": "anternacum", "name": "Anternacum", "modern": "Andernach", "lat": 50.44, "lon": 7.402, "realm": "franks", "county": "", "region": "rhineland", "kind": "royal seat", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The king's estate in Roman walls and the Rhine harbour of the Eifel: quern stones of Mayen lava, tuff and pots are loaded here; battles before the town in 876 and 939."},
  {"id": "magadoburg", "name": "Magadoburg", "modern": "Magdeburg", "lat": 52.125, "lon": 11.634, "realm": "franks", "county": "ostfalahi", "region": "saxony", "kind": "town", "landing": "riverbank", "size": 4, "from": 805, "to": 0, "note": "The king's border post on the middle Elbe (805), then Otto the Great's city: abbey 937, market and mint 965, archbishop 968; Slav wax, furs and captives; three weeks upstream from Hammaburg."},
  {"id": "treiectum", "name": "Treiectum", "modern": "Maastricht", "lat": 50.849, "lon": 5.689, "realm": "franks", "county": "hasbania", "region": "meuse", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "The bridge town of Saint Servatius on the Maas, thick with merchants in Einhard's day; mint and toll; plundered by the Northmen in 881."},
  {"id": "sutfene", "name": "Sutfene", "modern": "Zutphen", "lat": 52.139, "lon": 6.196, "realm": "franks", "county": "hamaland", "region": "rhine_delta", "kind": "royal seat", "landing": "riverbank", "size": 2, "from": 890, "to": 0, "note": "The counts' ring fort at the Berkel mouth on the IJssel, raised after the Northmen's raid of 882; given to the church of Utrecht in 1046; a palace about 1050."},
  {"id": "sanctos", "name": "Sanctos", "modern": "Xanten", "lat": 51.662, "lon": 6.454, "realm": "franks", "county": "hattuaria", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The canons' church of Saint Victor on the lower Rhine, with a market at its gate; burned by the Northmen in the winter of 863-864, the saint's bones carried to Cologne by night."},
  {"id": "wormatia", "name": "Wormatia", "modern": "Worms", "lat": 49.63, "lon": 8.36, "realm": "franks", "county": "", "region": "rhineland", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Bishop's city and king's palace on the upper Rhine; the Frisian merchants keep their own stretch of the town wall (c. 900); wine at the source."},
  {"id": "confluentes", "name": "Confluentes", "modern": "Koblenz", "lat": 50.362, "lon": 7.604, "realm": "franks", "county": "", "region": "rhineland", "kind": "royal seat", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The king's estate and Saint Castor's church where the Moselle meets the Rhine; a toll taken in each town's own goods; the archbishop of Trier's from 1018."},
  {"id": "stethu", "name": "Stethu", "modern": "Stade", "lat": 53.602, "lon": 9.477, "realm": "franks", "county": "", "region": "saxony", "kind": "fort", "landing": "riverbank", "size": 2, "from": 900, "to": 0, "note": "The counts' fort and creek harbour on the south bank of the lower Elbe; pirates beat the counts here in 994 and maimed their hostages; market, mint and toll in 1038."},
  {"id": "cruoninga", "name": "Cruoninga", "modern": "Groningen", "lat": 53.219, "lon": 6.568, "realm": "franks", "county": "thrianta", "region": "frisia", "kind": "market", "landing": "riverbank", "size": 2, "from": 1000, "to": 0, "note": "A market village on the tip of the sand ridge, reached from the Wadden by the tidal Hunze; the king's estate, given with a mint to the church of Utrecht in 1040."},
  {"id": "luniburc", "name": "Luniburc", "modern": "Lueneburg", "lat": 53.249, "lon": 10.401, "realm": "franks", "county": "bardengau", "region": "saxony", "kind": "market", "landing": "riverbank", "size": 2, "from": 956, "to": 0, "note": "The Billung dukes' castle on the chalk hill and the brine spring below it; the king gave the salt toll to Saint Michael's in 956; the salt goes down the Ilmenau past Bardanwic."},
  {"id": "hoium", "name": "Hoium", "modern": "Huy", "lat": 50.519, "lon": 5.24, "realm": "franks", "county": "hasbania", "region": "meuse", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "A town of brass and copper workers under a rock on the Maas; its men pay toll at London about 1000; the bishop of Liege sells it a charter of freedoms in 1066."},
  {"id": "werda", "name": "Werda", "modern": "Kaiserswerth, Duesseldorf", "lat": 51.3, "lon": 6.733, "realm": "franks", "county": "", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 1, "from": 0, "to": 0, "note": "Saint Suitbert's monastery on an island in the Rhine; the emperor's palace from about 1045, where archbishop Anno carried off the boy king by ship in 1062."},
  {"id": "mitthelburg", "name": "Mitthelburg", "modern": "Middelburg, Walcheren", "lat": 51.5, "lon": 3.614, "realm": "franks", "county": "walacria", "region": "zeeland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 890, "to": 0, "note": "The middle one of Walcheren's round refuge forts, raised against the Northmen about 890; the island's market after Walichrum, reached by the tidal Arne."},
  {"id": "bunna", "name": "Bunna", "modern": "Bonn", "lat": 50.733, "lon": 7.1, "realm": "franks", "county": "ripuaria", "region": "rhineland", "kind": "monastery", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "The canons' church of Saint Cassius with a merchants' street beside it, half a day above Cologne; burned by the Northmen in 882 and 883."},
  {"id": "niusa", "name": "Niusa", "modern": "Neuss", "lat": 51.198, "lon": 6.692, "realm": "franks", "county": "", "region": "rhineland", "kind": "fort", "landing": "riverbank", "size": 2, "from": 0, "to": 0, "note": "An old Roman fort and convent at the Erft mouth, the archbishop of Cologne's; burned by the Northmen in 881 (?)."},
  {"id": "liunvert", "name": "Liunvert", "modern": "Leeuwarden", "lat": 53.203, "lon": 5.79, "realm": "franks", "county": "austrachia", "region": "frisia", "kind": "village", "landing": "beach", "size": 1, "from": 1000, "to": 0, "note": "Three terps on the shore of the Middelzee with a church of Corvey; count Bruno strikes pennies here about 1040-1057."},
  {"id": "spira", "name": "Spira", "modern": "Speyer", "lat": 49.317, "lon": 8.442, "realm": "franks", "county": "", "region": "rhineland", "kind": "town", "landing": "riverbank", "size": 3, "from": 0, "to": 0, "note": "Bishop's city on the upper Rhine; the emperors build their burial church here from 1030; wine, and stone and lead for the builders."},
```
