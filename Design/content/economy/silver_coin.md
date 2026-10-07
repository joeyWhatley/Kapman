# The trade in silver: pennies, sceattas, dirhams, hacksilver, weights, mints and the reforms of the coin, 793 to 1066

Written 6 Oct 2026 for the economy folder. A draft for the user to overwrite.

**How to read the marks.** `[W: page]` = checked this session on that Wikipedia page. `[mem]` = from memory of the
named source, not re-checked today. `(?)` = I could not check it, or the scholars disagree: treat as a guess.
`(reckoned)` = my own arithmetic from sourced figures. Nothing here is quoted word for word from a modern
translation; law texts are given in my own plain rendering.

---

## 0. In short, for the game

1. **Silver is not a cargo. It is the purse.** No one shipped silver as a good for profit in the way wine was shipped.
   It moved as payment, tribute, toll, alms and loot. The whole of the player's debt (480 d) weighs 816 g and fits in
   one hand. So silver belongs in `trade_rules.json` (the money), not as a row of `goods_793.json`, with three
   exceptions that are real goods: **scales and weights**, **silver in the bar for a smith** (small), and **plate**.
2. **There are three ways to pay, and the border between them moves over the years.** By count (a king's coin, taken
   at face: Francia, England); by weight (any silver, on the scales: the north and east, from about 850 at full
   strength); and in kind (cloth, cattle, furs, salt: everywhere, always, beside the other two).
3. **The profit in silver is in the differences**: a foreign penny loses at a mint; an old penny loses at a
   recoinage; a heavy penny is worth more on a northern scale than a light one that passes at the same face value
   at home; a bad penny passes with a stranger and not with a moneyer. A merchant who knows which coin is about to
   be called in, and where, wins days and pennies. That is the "knowledge is the treasure" rule applied to money.
4. **Five dates change the money for the player's house**: 793/4 (the heavy denier), 864 (Pîtres: ten mints, old coin
   void), about 870–900 (the north goes over to weighed silver; dirhams in flood), 973 (Edgar: every English penny
   called in again and again), 990s–1010s (the dirhams stop; English and German pennies fill the north; the northern
   kings strike their own).

---

## 1. What "silver" was: the kinds in the purse

| Kind | Weight and fineness | When | Where it passed, and how | Source |
|---|---|---|---|---|
| **Sceat** (pl. sceattas; a modern name) | small thick coin, 0.8–1.3 g; early ones good silver, later ones copied and debased | primary series c. 680–720, secondary c. 710–750 | England, Frisia, the Rhine mouth; Frisian series D and E struck in Frisia, Dorestad among the places; current in Francia until Pippin's reform at Ver in 755. By 793 an old coin: in a hoard or a child's hand, not on a moneyer's bench. | [W: Sceat] |
| **Ribe's small pennies** (the "Wodan/monster" type, series X) | about 1 g (?) | 8th century, perhaps to c. 800–820 (?) | Found coin by coin in the market layers at Ribe, which is read as coin used by count on that one market. No mint has been found. | [W: Sceat; W: Viking coinage]; Feveile 2006 [mem] |
| **The old light denier** | about 1.3 g (?) | Pippin 755 to 793 | the Frankish realm. Pippin's rule at Ver: 22 shillings (264 pennies) struck from the pound, of which the moneyer keeps one shilling. | [W: Carolingian monetary system] for the 22 shillings; the weight and the moneyer's shilling [mem: capitulary of Ver 754/5 c. 5] |
| **The new heavy denier** | about 1.7 g, of pure silver; 12 to the shilling, 240 to the pound of about 408 g; about sixty mints | from 793/4; the Synod of Frankfurt (June 794) orders it taken everywhere | the whole Frankish realm, by count. The shilling and the pound are words for sums, not coins. A half-penny (obol) is struck too, rarely (?). | [W: Carolingian monetary system]; Frankfurt 794 c. 5 (in `chronicle_793.json`) |
| **Offa's penny** | broad thin penny; begun c. 765–770; a second reform "in 792 or 3" with a heavier, wider coin (about 1.3 g before, about 1.45 g after (?)) | to 796, and his successors' after | the English kingdoms south of the Humber, by count; struck at Canterbury, London and in East Anglia | [W: History of the English penny (c. 600–1066)]; weights [mem: Grierson & Blackburn 1986] |
| **Styca** (Northumbria) | small thick coin, in the end "almost no silver", struck in great numbers; the Hexham hoard held some 8,000 | c. 790s–867 | Northumbria only; worthless outside it. Replaced about 867 under the Scandinavian rulers of York by silver. | [W: History of the English penny] |
| **Alfred's and later English pennies** | heavier, finer penny from the mid 870s (about 1.6 g (?)); after c. 973 one design at a time across about forty mints, the weight varying type by type between about 1.0 and 1.7 g (?) | 870s–1066 | all England, by count; halves and quarters made by cutting the penny along the cross | [W: History of the English penny]; weights [mem: Petersson 1969, Metcalf 1998] |
| **Dirham** | the Caliphate's silver coin; the lawful weight 2.975 g; in practice 2.7–3.0 g for Abbasid coin, and 2.5 to over 4 g for the broad Samanid coin of the 900s (?), whose silver falls after c. 950 (?) | in the north from the 780s/800 to the 1010s | by weight in the Baltic and Scandinavia, whole and cut; by count perhaps on the Volga | [W: Dirham]; Noonan 1998 [mem] |
| **Hacksilver** | any silver cut to the sum: coins, rings, bars, brooches, plate | at full strength from c. 850/870 to c. 1000–1050; later in Gotland and the east Baltic | Scandinavia, the Baltic, the Danelaw, the Irish Sea | [W: Hacksilver]; Skre 2008 [mem] |
| **Rings and bars as money** | arm rings made to a weight: at Spillings a ring is "around 100 grams"; broad-band arm rings in Ireland c. 850–950 and plain "ring-money" in Scotland c. 950–1050 run to multiples of about 24–26 g (?) | 9th–11th c. | the same lands; a ring is worn, shown, and cut when needed | [W: Spillings Hoard]; Sheehan, Graham-Campbell [mem] |
| **Hedeby's pennies** | light pennies copying Charlemagne's Dorestad deniers; later thin "half-bracteates" (?) | from c. 825; again c. 900–980s (?) | Hedeby and its neighbourhood, by count there, by weight elsewhere; four lay in the Spillings hoard | [W: Spillings Hoard]; Malmer 1966, 2007 [mem] |
| **The northern kings' pennies** | Sven Forkbeard c. 995; Olaf Tryggvason 990s; Olof Skötkonung at Sigtuna 990s; Sihtric at Dublin 997 (in the chronicle). A Norwegian penny about 0.89 g, later mixed with copper. | from the 990s | copies of Æthelred's pennies, struck in part by English moneyers (?) | [W: Viking coinage]; `chronicle_793.json` 995, 997 |
| **German pennies** | Cologne, Mainz, Worms, Speyer and the Saxon mints; the Otto-Adelheid penny (struck "soon after 983", unchanged to the mid 11th c., 1.25–1.5 g) | 10th–11th c. | the German kingdom by count; the Baltic by weight, in huge numbers after c. 990 | [W: Otto-Adelheid-Pfennig] |
| **Gold** | the mancus (a gold weight of about 4.25 g and a sum of 30 pence, in England); the dinar; the Byzantine nomisma (about 4.4 g (?)); a gold solidus of Louis the Pious, copied in Frisia (?) | all through | gifts, wergilds, church payments, never the market. Gold to silver 12 to 1 by law in 864. | `Design/economy_and_trade.md` 1.1; [W fr: Édit de Pîtres]; Grierson [mem] |

**Sums of account a merchant reckons in.** Frankish: 12 d = 1 shilling (solidus), 20 shillings = 1 pound = 240 d
[W: Carolingian monetary system]. English: the pound of 240 pence; the mancus of 30 pence; the West Saxon shilling of
5 pence and the Mercian of 4 (?) [mem: the law codes]. Norse: the mark of 8 ounces (aurar, sing. eyrir), the ounce of
3 ørtugar; a mark about 200–215 g, an ounce about 24–27 g, an ørtug about 8–9 g (?) [mem: Kyhlberg 1980; Pedersen in
Skre 2008]. In the Danelaw the ora is reckoned at 16 pence, later 20 (?) [mem: Domesday]. So one mark of silver is
roughly 120–125 new deniers, and half a pound Frankish (reckoned).

---

## 2. Where it was made and by whom

### 2.1 The mines

| Mine | What it gave | Years | Source |
|---|---|---|---|
| **Melle in Poitou** (Metallum, on the coins METULLO / METALLVM) | The Carolingians' one great silver mine: silver parted from lead ore (galena), the rock broken by setting fires against it. It had its own mint; Melle deniers and obols of Charlemagne and Charles the Bald are among the commonest Carolingian coins. The lead went to church roofs. | worked from about 600 to the late 10th century (?); the place was burned by the Northmen in 848 | Annals of St-Bertin under 848 [mem]; Téreygeol's excavations [mem]. The Wikipedia page could not be fetched: all of this row is (?) for detail. |
| **The Rammelsberg above Goslar** (the Harz) | "Veins of silver" opened under Otto the Great, first written of about 968 by Widukind of Corvey; Goslar first named 979; the king's palace built there in 1005. This silver feeds the Saxon and Rhenish pennies that reach the Baltic after c. 990. | from c. 968 | [W: Rammelsberg] |
| **The mines of the east** | The Samanid emirs' silver: the Panjshir valley (Banjahir) in the Hindu Kush and the mines of Ilaq near Tashkent (al-Shash) (?). The mints named on dirhams in northern hoards are chiefly al-Shash, Samarkand, Balkh and Andaraba for the 10th century; Baghdad (Madinat al-Salam), Wasit, al-Muhammadiyya (Rayy) and North Africa for the 9th (?). The Spillings coins were struck at 69 places in 15 lands. | 8th–10th c. | [W: Spillings Hoard] for the 69 mints; Noonan 1998 [mem] |
| **English and Welsh lead** | Lead from the Peak (Wirksworth) and Mendip carries little silver (?). England's coin silver was mostly other people's coin melted down: Frankish in Offa's day, German in Æthelred's (?). | all through | Sawyer 2013 [mem] |
| **Old silver** | Everywhere the largest "mine" was the melting pot: old coin, church plate, Roman plate, loot. The Vale of York cup was Frankish church plate of the mid 9th century in a Northman's hoard of 927/8. | all through | [W: Vale of York Hoard] |

### 2.2 The mints and the moneyers

- **Francia.** After 793/4 about sixty mints strike the king's denier [W: Carolingian monetary system]. In 805
  (Thionville) the king orders coin struck only at the palace, because of false coin (in the chronicle:
  `805-12-thionville-coin`). Under Louis the Pious the coin is called in and re-struck more than once: a type with the
  mint's name (816 (?)), and from 822/3 the "Christiana religio" penny with a temple and no mint name; in 825
  Martinmas is the last day for old pennies (?) [mem: Admonitio of 825 c. 20; Coupland 1990]. In 864 Charles the Bald
  cuts the West Frankish mints to ten [W fr: Édit de Pîtres]: the palace, Quentovic, Rouen, Reims, Sens, Paris,
  Orléans, Chalon, Melle and Narbonne [mem: Edict of Pîtres c. 12]. (The English Wikipedia page says nine cut to
  three; the edict's own list has ten, and the French page says ten. Trust ten.) In practice far more than ten struck
  the new type within a few years (?).
- **Dorestad.** A mint under Charlemagne, Louis the Pious and Lothar I, at its busiest in the 820s–840s; the model for
  Hedeby's first pennies; its striking ends in the 850s (?) [mem: Coupland 1988, "Dorestad in the ninth century: the
  numismatic evidence"]. The moneyer Madelinus's gold coins of the 7th century were still being copied in Frisia in
  the 8th (?). The game's port table ends Dorestad in 863, which fits.
- **England.** Offa's moneyers work at Canterbury, London and in East Anglia, and sign their coins with their own
  names [W: History of the English penny]. Æthelstan's law at Grately (c. 926–930) sets "a single currency" and lists
  the moneyers for each town: London 8, Canterbury 7 (4 the king's, 2 the bishop's, 1 the abbot's), Winchester 6,
  Rochester 3, Lewes 2, Southampton 2, Wareham 2, Exeter 2, Shaftesbury 2, Hastings 1, Chichester 1, and one in each
  other borough [W for the code and its date; the list from II Æthelstan 14, mem, agreeing with
  `town_actions_793.json` for London and Canterbury]. Edgar's reform of c. 973: one design at "around forty mints",
  the dies cut centrally. By 1066 Harold's pennies come from 48 mints, and some seventy places struck at one time or
  another [W: History of the English penny]. In 1066 a moneyer at Hereford paid 18 shillings for his dies when the
  coinage changed and 20 more within the month (?) [mem: Domesday Book, Herefordshire, customs "in the time of King
  Edward"].
- **The Danelaw.** From about 895 the Scandinavian kings at York and in East Anglia strike pennies of their own:
  the St Edmund memorial pennies (about 1,800 in the Cuerdale hoard), the York pennies of Cnut and Siefred (about
  3,000 at Cuerdale), later St Peter pennies with a sword and a hammer (?) [W: Cuerdale Hoard; the St Peter type mem].
  Coin dies and lead trial strikings were dug up at Coppergate in York (?) [mem: Pirie 1986].
- **The north.** Hedeby from c. 825 [W]; then nothing royal until the 990s: Sven in Denmark c. 995, Olaf in Norway,
  Olof at Sigtuna, Sihtric at Dublin 997 [W: Viking coinage; chronicle]. Cnut (1018–1035) gives Denmark a row of
  mints (Lund, Roskilde, Viborg and others) on the English pattern (?). Harald Hardrada (1047–1066) strikes a
  Norwegian penny for use by count, and thins it with copper [W: Viking coinage, in general terms; the saga story of
  his bad pay-silver, "Harald's striking", is from Morkinskinna, mem (?)].
- **The German kingdom and Frisia after Dorestad.** Cologne's "Sancta Colonia" penny is the Rhine's coin in the 10th
  century (?). In the 11th century small Frisian mints strike for the counts and bishops: Tiel, Deventer, Utrecht,
  Dokkum, Leeuwarden, Stavoren, Bolsward, Emden (?), and their pennies are among the commonest German coins in
  Baltic hoards (?) [mem: Ilisch 1997–2014].

**Who a moneyer is.** A man of standing in a town, working for the king, the bishop or the count under oath; he takes
your silver, tests it, melts it, and gives back fewer new pennies than you gave weight, the difference being the
king's share and his own. Under Pippin that share was one shilling in 22 (about 4.5 %) [mem]. The game's 10 % is
high for an ordinary change and about right for a recoinage (?).

---

## 3. Where it was wanted and why

| Who | Why they want it | In what form | Source |
|---|---|---|---|
| **Frankish and English kings** | Tolls, fines and rents come in as coin; a recoinage is a tax. A king wants all foreign and old silver in his realm to pass through his moneyers. | their own penny only | Frankfurt 794; Pîtres 864; II Æthelstan 14 |
| **The Church** | Tithes, alms, Peter's pence to Rome (a penny a hearth from England, from the 9th century (?)); plate for the altar; ransoms for captives. Frideburg of Birka left her silver to be given to the poor and the churches at Dorestad, and her daughter Catla carried it there. | coin; plate; bullion for the goldsmith | Rimbert, Life of Anskar c. 20 [mem; Robinson 1921 is the safe translation] |
| **Danish, Norse and Svea chiefs** | Silver is rank: rings given to followers, a hoard under the floor, a woman's neck rings. Ibn Fadlan (922) says a Rus wife wears one neck ring for every ten thousand dirhams her husband owns. Silver also pays men and buys peace. | rings, bars, any coin by weight | Ibn Fadlan [mem; cited in `goods_793.json` as `ibnfadlan`] |
| **The Rus and the river traders** | They sell furs and slaves on the Volga and will take nothing but coin for them, which they tie into their belts (Ibn Rusta, c. 903–913 (?)). | dirhams | Ibn Rusta [mem] (?) |
| **Northern craftsmen** | Silver to cast: rings, brooches, pendants, the wire on a sword hilt. | bars and scrap | Kaupang, Birka, Hedeby workshop finds [mem: Skre 2008] |
| **Everyone who owes** | Rent days and tribute days pull coin out of every purse at once. In 866 the West Frankish tribute was raised at 6 d from each free holding, 3 d from each servile one, a tenth of what merchants had, and so on. | the king's coin | Annals of St-Bertin under 866 [mem] |
| **The player** | 480 d owed to the count, in the count's coin. Not in dirhams, not in rings. | deniers | `trade_rules.json` start |

**Where silver is scarce and so buys most** (reasoning from the hoards, not a sourced price): in 793 the whole north
has little; silver's buying power there is high and falls as the dirhams arrive (after c. 800, strongly after c. 900)
and again as the English tributes arrive (991–1018). In England after 1018 coin is short. `Design/economy_and_trade.md`
section 2.4 already models this as a regional silver level; this file agrees with it.

---

## 4. The routes and the ships

1. **The eastern stream (dirhams).** From the mints of Iraq, Iran and later Central Asia, north by caravan to the
   Khazars at the Volga mouth and to the Volga Bulgars; up the Volga and over the portages to Ladoga
   (Aldeigjuborg); by ship across the Gulf of Finland to Gotland, Birka and the south Baltic markets (Truso, Reric,
   later Wolin and Hedeby). The earliest dirhams reach the Baltic "from at least 786" and the stream stops around
   1009–1015 (`Design/economy_and_trade.md` 1.1, checked there). Two waves: Abbasid coin c. 800–870, then the
   Samanid flood c. 900–960 through the Bulgars, with a lean stretch between (c. 875–900 (?), Noonan's "first silver
   crisis") [mem: Noonan 1998]. The carriers are river boats and small sea-going clinker ships of the Svear and
   Gutes; nothing special is built for silver. It goes in the skipper's chest.
2. **West out of the Baltic.** Dirhams get as far as Hedeby, Kaupang, the Danelaw and Ireland as cut fragments:
   over 120 pieces at the great army's winter camp at Torksey (872–3) (?), Kufic coins in the Cuerdale hoard, and
   a Samarkand piece in the Vale of York hoard [W: Cuerdale Hoard; W: Vale of York Hoard; Torksey mem: Hadley &
   Richards 2016]. Very few reach Francia; Ibrahim ibn Yaqub is surprised to see Samarkand dirhams (of 913–915 (?))
   at Mainz in 965 (`chronicle_793.json` 965).
3. **The Rhine stream (deniers).** Down the Rhine with the wine to Dorestad, and out with the Frisians to England,
   Ribe, Hedeby and Birka. Carolingian coin is rare in the north as coin (it was melted or worn as pendants), but
   Dorestad's penny is the pattern for Hedeby's [mem: Coupland 2007]. The ships are the Frisians' river and coast
   boats: the player's own trade.
4. **The Channel.** English and Frankish pennies do not mix: each side's finds hold very little of the other's coin,
   because each king had foreign coin melted at the port (?) [mem: Metcalf; Coupland]. So across the Channel silver
   changes its face at Quentovic, Rouen, Dover (dofras), Sarre, Fordwich, London and Hamwic.
5. **The tribute stream (west and south to north).** Frisia 100 pounds in 810; Paris 7,000 pounds in 845; in the
   860s the Seine and Somme fleets 3,000, 5,000, 6,000 and 4,000 pounds (?); 5,000 in 877 and 12,000 in 884 (?);
   England 10,000 pounds in 991, 16,000 in 994, 24,000 in 1002, 36,000 in 1007, 48,000 in 1012, 72,000 plus London's
   10,500 in 1018 [W: Danegeld for 810, 991, 994, 1007, 1012, 1018; the Frankish figures after 845 mem: Annals of
   St-Bertin and St-Vaast; 1002 from the chronicle]. It went home in the raiders' own warships. The Swedish rune
   stones remember men who "took geld in England" [W: Danegeld].
6. **The German stream.** After c. 970–990 pennies of the Harz silver go down the Elbe and by Hedeby, and from the
   Rhine and Frisia by sea, to the Baltic. Sweden's Viking-age coin finds come to about 259,524 coins, two thirds of
   them on Gotland; about 35 % of the coin entering Scandinavia was eastern [W: Viking coinage]. The rest is mostly
   German and English (?).
7. **Rome.** English silver goes south every year as alms and Peter's pence; a hoard of English pennies of c. 945
   was found in the Forum (?) [mem]. Off the game's chart, but a reason coin is short in England at Easter.

---

## 5. The season of silver

Silver does not ripen, but coin comes and goes with the year. (The feast dates are the period's own; the reading of
"cheap" and "dear" is mine: coin is *dear* when everyone needs it and goods must be sold low to get it.)

| Time | What happens to coin | Where | Source or reasoning |
|---|---|---|---|
| Thaw to midsummer | The river boats go down the Volga and Dnieper; no new dirhams yet. Silver at its scarcest in the Baltic. | Ladoga, Birka, Gotland | the ice season (`research_trade_seasons_and_hints.md` 2.15) |
| Midsummer to the first frost | The eastern boats come back. New silver at Ladoga, then Gotland and Birka. Northern buyers pay best now. | Ladoga, Birka, Gotland, Hedeby | the same; Ibn Fadlan met the Rus on the Volga in spring–summer 922 |
| Spring (Lent to Whitsun) | Tributes are demanded and raised: Paris fell at Easter 845; the English gelds of 1007 and 1012 were paid in spring (?). Coin drains from the paying land; the raiders' markets fill with silver and loot. | Seine, Thames; then Denmark, Norway | chronicle entries 845, 1007, 1012 |
| After harvest (August–October) | Corn and wool are sold; coin is in country purses; the fairs (Saint-Denis from 9 October) turn it over. Coin easiest. | Francia, England | `fairs_793.json`; the fair date [mem] |
| Vintage to Martinmas (October–11 November) | The wine fleet buys on the Rhine: coin flows up-river. | Rhineland, Dorestad | goods table, `rhenish_wine` |
| **Martinmas (11 November)** | The day rents fall due, and twice the day old coin dies: 825 (?) and 864. Before it, the moneyer's bench is crowded and coin is dear. | Francia | Pîtres 864 c. 10–11 [mem]; `town_actions_793.json` change_coin |
| Michaelmas and Easter | English rent and render days (?); Peter's pence due by St Peter's day (1 August) (?) | England | the law codes [mem] |
| Winter | The sea is shut. Hoards go into the ground. Silver sits. | everywhere | — |
| A recoinage year (England: 973, then about every six years, later every two or three) | Every old penny must be changed. For some months the old coin is refused or taken at a loss; a stranger with old pennies is stuck. | England | [W: History of the English penny]: six types under Æthelred, fourteen between 1035 and 1066 "lasting only two or three years each" |

---

## 6. Units, packing, and what a boat of 660 kg could carry

**How it was carried.** In a leather or linen purse at the belt (Ibn Rusta's Rus) (?); in a small locked chest
under the skipper's thwart (iron-bound chests and their locks are common finds; a whole tool chest survives from
Mästermyr on Gotland (?)); in a lead box or lead sheet in the ground (Cuerdale, Vale of York) [W]; inside a silver cup
(Vale of York) [W]; under the floor of a house in wooden chests (?) (Spillings) [W gives the building, not the
chests]; and on the body, as arm rings. Rings were also tied in bundles to a round weight (?) (Spillings).
Bars were cast in open moulds cut in soapstone or sandstone; a bar is nicked to show it is silver through.

**The measures.**

| Measure | In deniers of 1.7 g | In grams |
|---|---|---|
| 1 denier | 1 | 1.7 |
| 1 Frankish shilling (a sum) | 12 | 20.4 |
| 1 Frankish pound (a sum and a weight) | 240 | 408 |
| 1 dirham at full weight | 1.75 | 2.975 |
| 1 Norse ounce (eyrir) | about 14–16 (?) | about 24–27 (?) |
| 1 Norse mark | about 115–125 (?) | about 200–215 (?) |
| 1 Spillings arm ring | about 59 | about 100 |
| the player's debt | 480 | 816 |
| 1 kg of silver | 588 | 1,000 |

**What 660 kg means** (all reckoned):

- 660 kg of silver is 388,000 deniers, or 1,617 Frankish pounds. No merchant in this game's world ever had it.
- The tribute of Paris in 845 (7,000 pounds, about 2,860 kg) would fill the player's boat four and a third times.
- The geld of 1018 (72,000 pounds; the Danegeld page reckons it at 26,900 kg) would take 41 such boats.
- The Spillings hoard (67 kg of silver) is a tenth of the boat's burden and about 39,000 deniers' worth: one very rich
  Gotland farm's saving of a lifetime or three.
- The Cuerdale hoard (32–40 kg) is one man's lift in two chests.
- A full cargo of Rhenish wine (five casks at the goods table's 60 d) is worth 300 d: 510 g of silver. **The whole
  boat-load of the game's richest bulk cargo is paid for with a purse that weighs as much as a loaf.** This is why
  silver must stay money and not become a cargo row: at 588 d a kilo it would break every price in the table.
- What a small boat really carries of silver: the skipper's purse (some tens to hundreds of pennies), the crew's
  own, a sum entrusted by a church or a lord (Frideburg's alms), and now and then a bar or a few rings for a smith.

**The tools that are goods.** A folding balance of bronze in a tinned case, with a set of weights: first lead
weights of many shapes; from about 860–880 the two standard kinds borrowed from the Caliphate, small brass cubes with
the corners cut off (marked with dots) and flattened iron balls cased in brass (?) [mem: Steuer 1987; Pedersen in
Skre 2008; `town_actions_793.json` weigh_silver says the same with a (?)]. Truso has given over a thousand weights
and Kaupang some four hundred (?) [mem]. `town_actions_793.json` already sells a balance and weights for 12 d
through `buy_tools`.

---

## 7. Prices and rates, where there is any evidence at all

Silver has no price in silver. What can be said is what a penny bought, what one silver cost in another, and what
changing cost.

| Evidence | Figure | In new deniers (reckoned) | Source |
|---|---|---|---|
| Frankfurt 794: a modius of oats, barley, rye, wheat | 1, 2, 3, 4 d; 12 two-pound wheat loaves 1 d | the same | already in `goods_793.json` (`frankfurt794`) |
| Gold against silver, Pîtres 864 | 1 pound of refined gold for 12 pounds of silver | 12 : 1 | [W fr: Édit de Pîtres] |
| The moneyer's share, Ver 755 | 1 shilling in 22 | 4.5 % | [mem] |
| London, Æthelstan's time (VI Æthelstan, c. 930–940): the worth of stolen stock | a horse half a pound (120 pence), an ox a mancus (30), a cow 20, a pig 10, a sheep a shilling (5) | at about 1.5 g a penny (?): 106, 26, 18, 9, 4.4 d | [mem: VI Æthelstan 6] (?) |
| Edgar's law: the price of wool | a wey of wool for half a pound (120 pence), no dearer | about 100–110 d (?) | [mem: III Edgar 8] (?) |
| The same law | "one coinage over all the king's realm, and one measure and one weight, as at London and Winchester" | — | [mem: III Edgar 8] (?) |
| Birka's ransom, c. 840s | 100 pounds of silver asked by Anund; the townsmen paid | about 24,000 d if Frankish pounds (?) | Rimbert, Life of Anskar c. 19 [mem] |
| The Curonians' ransom at Apulia, c. 850s | half a pound of silver a man | 120 d a man | Rimbert c. 30 [mem] |
| A marten skin among the Volga Bulgars | 2½ dirhams | about 4.4 d | Ibn Rusta [mem] (?) |
| A Rus wife's neck ring | one for each 10,000 dirhams of her husband's | 17,500 d (72 pounds) | Ibn Fadlan [mem] |
| Cloth as small money at Prague, 965 | ten thin kerchiefs to a penny | — | Ibrahim ibn Yaqub [mem] (?) |
| The Seine tribute of 866 | 4,000 pounds, weighed "by the Northmen's balance" | — | Annals of St-Bertin 866 [mem] |
| London tolls, c. 1000 (IV Æthelred 2) | a small boat ½ penny, a larger boat with a sail 1 penny, a keel or hulk 4 pence if she lies there; the Emperor's men at Christmas and Easter two grey cloths and a brown, ten pounds of pepper, five pairs of gloves, two casks of vinegar | the player's boat: ½ or 1 penny | [mem: IV Æthelred 2; Thorpe 1840 / Robertson 1925] (?) |
| A balance and weights | 12 d | — | a game guess already in `town_actions_793.json` |

**Rates between silvers, for the game** (reckoned from the weights; history gives no table):

- By weight, 1 full dirham = 1.75 new deniers = about 2 of Offa's heavy pennies = about 2.3 of Offa's light pennies.
- On a northern scale all of these are just grams. A heavy coin is worth carrying north; a light coin that passes
  at face at home (a late-type English penny of 1.0 g) is worth spending at home and never north.
- At a mint, foreign silver loses the king's share: a twentieth in a quiet year (?), a tenth or more at a recoinage (?).

---

## 8. Tolls, bans and laws about the coin

| Year | Law | What it says, plainly | For the game | Source |
|---|---|---|---|---|
| 755 | Pippin at Ver | 22 shillings to the pound; one to the moneyer | history only | [W; mem] |
| 793/4 | Charlemagne's new denier; Frankfurt c. 5 (June 794) | The new pennies with the king's name, if of pure silver and full weight, are to be taken in every place, town and market. Who refuses: a free man pays 15 shillings; an unfree man loses what he was selling or is flogged at the post (?). | From 794 old light deniers are refused in Frankish markets until changed. **The game starts on 21 June 793, a year before this.** | chronicle `794-06-frankfurt-new-denier`; the penalties mem (?) |
| 805 | Thionville | Because of false coin, pennies are to be struck nowhere but at the palace, unless the king orders otherwise; good pennies already out stay good. | a year when a mint may be shut | chronicle `805-12-thionville-coin` |
| 818/9 (?) | Louis the Pious | A free man who refuses a good penny pays the king's fine of 60 shillings; an unfree man takes 60 strokes (?). | the fine for refusing coin | [mem] (?) |
| 825 | Louis the Pious, Admonitio c. 20 | After Martinmas only the new penny is good (?). | old coin void: a dated card | [mem] (?); `town_actions_793.json` |
| 861 (?) | Charles the Bald at Quierzy | Whoever refuses a good penny is flogged; a repeat offender is marked on the forehead with a hot penny (?). | colour for a card | [mem] (?) |
| 864 | Edict of Pîtres | New pennies (1.75 g, the king's monogram, "by the grace of God king", the mint's name round a cross) from ten mints only; from Martinmas only the new coin is taken; each count is given five pounds of silver from the king's chest to start his mint (?); a moneyer swears to strike pure and true; a false moneyer loses his hand (?); no one may sell gold or silver that is mixed; gold 12 to 1; no arms or horses to the Northmen on pain of death. | the one great Frankish recoinage after 825: old deniers void, fewer mints, a long bench | [W fr: Édit de Pîtres] for the ten mints, 1.75 g, the design, 12:1; [W: Edict of Pistres] for arms and horses; the rest mem (?) |
| c. 926–930 | Æthelstan at Grately (II Æthelstan 12, 14) | One coinage over all the king's land; no one to strike but in a port (a town); a moneyer found false has the hand that did it struck off and set up over the mint; no buying of goods worth over 20 pence outside a port. | English mints only in towns; big deals only in towns | [W]; chronicle 928; the hand and the 20 pence mem |
| c. 973 | Edgar's reform | Every penny called in; one design from about forty mints; after this the type is changed again and again. | recoinage events: 973, then about 979, 985, 991, 997, 1003, 1009, 1017, 1023, 1029, 1035 (?), then every two or three years to 1066 | [W]; chronicle `973-edgar-coinage`; the type dates mem: Dolley (?) |
| c. 1000 | IV Æthelred 5–9 | Merchants who bring false or underweight coin to the port must clear themselves or pay; those who watch the ports answer for bad coin passing; moneyers who work in woods or in secret forfeit their lives (?). | port reeves test strangers' coin | [mem] (?) |
| 1018 | Cnut at Oxford | (the peace; the last and greatest geld) | silver out of England | chronicle `1018-heregeld` |

**Tolls and silver.** Tolls in the coin lands are paid in the king's coin or in kind; a toll-keeper is the first man
to refuse a foreign penny. The Frankish tenth at Dorestad and Quentovic is in `trade_rules.json` already. In the
lands of weighed silver the king's man takes a gift, not a toll (`Design/economy_and_trade.md` 1.5). The London list
above is the only place where the player's own boat has a sourced toll in pennies: a halfpenny or a penny.

**There was no ban on carrying silver out of a realm** in this period that I know of. The control was at the other
end: what came in had to be re-struck.

---

## 9. Frauds, and how silver was tested

**The frauds.**

1. **Plated coin**: a copper or lead core under a silver skin. A forged dirham in the name of the Samanid
   Isma'il ibn Ahmad (903–907/8) was found at Coppergate in York [W: Dirham, footnote]. The Spillings hoard holds
   "imitations and fakes", though of good silver, struck by others than the caliph's mints (Khazar and Bulgar copies,
   among them the "Moses is the messenger of God" coin of about 800) [W: Spillings Hoard].
2. **Thinned silver**: alloyed with copper, or with lead, which keeps the colour better. The stycas of York and
   Harald Hardrada's pennies are kings doing it; Pîtres forbids selling "mixed" gold or silver.
3. **Light coin**: clipped at the edge, or struck light by the moneyer. The late English types fall in weight within
   each issue (?), which a moneyer knows and a stranger does not.
4. **False weights and balances**: a hollow weight, a weight shaved underneath, a balance with unequal arms. Weights
   were made hard to tamper with on purpose: the brass skin on an iron ball shows any filing, and the cubes carry
   their dots on every face (?) [mem: Steuer; Sperber 1996].
5. **False bars and rings**: a bar with a base core; a ring of the right look and the wrong weight.
6. **Passing dead coin**: old pennies after a recoinage, another realm's pennies, stycas outside Northumbria.

**The tests.**

1. **The nick and the peck.** A knife cut in the edge of a coin or bar, or a jab of the point into its face, shows
   the metal under the skin and how soft it is (good silver is soft; a copper mix is hard). Pecks appear on coins in
   the Danelaw in the last decades of the 9th century (the Cuerdale coins are pecked), spread through Scandinavia in
   the 10th, and die away in the 11th as kings' coin comes to be taken at face (?) [mem: Archibald 1990 and 2011;
   `town_actions_793.json` test_silver says the same with a (?)]. Some coins carry a dozen or more pecks: one for each
   hand they passed through.
2. **Bending.** A good coin bends and stays bent; a plated one cracks. Many hoard coins are bent (?) [mem].
3. **The scales.** Both parties weigh; each brings his own weights and watches the other's.
4. **The touchstone.** A dark fine stone on which the silver is rubbed beside a rub of known silver; touchstones are
   claimed among the small "whetstones" in graves and towns (?) [mem: Ježek 2013].
5. **The fire.** A moneyer or goldsmith melts a sample with lead on a bone-ash hearth (cupellation): the lead and
   base metal sink away and the silver is left. Hearth-lining with silver and lead in it has been found at mint and
   workshop sites (?) [mem]. Domesday records payments in 1066 as "weighed and burnt" beside payments "by count" (?).
6. **The face.** In a coin land the moneyer's own name is on the penny (England), or the mint's (Francia after 864):
   a bad penny has a man who answers for it with his hand.

The game's `test_silver`, `weigh_silver` and `change_coin` acts already carry tests 1 and 3 and the mint. Tests 2, 4
and 5 are not in the game; 5 is what "a moneyer could tell more by the stone and the fire" stands for.

---

## 10. How the trade changed, age by age

| Years | The Frankish side | The English side | The north and east | What a Frisian skipper lives |
|---|---|---|---|---|
| **793–c. 830** | The heavy denier (793/4); sixty mints; Dorestad's mint among the busiest; coin by count, firmly. Recoinages 794, (816?), 822/3, 825. | Offa's heavy penny (792/3); then Coenwulf, the Kentish and East Anglian kings; Northumbria goes its own way into brass. | Almost no silver in 793. Ribe counts small pennies (?). The first dirhams reach Ladoga (780s) and the Baltic (c. 800). Hedeby copies Dorestad's penny (c. 825). Payment mostly in kind: cloth, beads, iron, cattle. | Deniers at home, pennies over the sea, goods for goods in the north. Change at every king's port. |
| **c. 830–c. 870** | Dorestad raided 834–837 and yet strikes on (?); the realm split 843; tribute begins (845); Melle burned 848; Dorestad's mint ends in the 850s (?); Pîtres 864. | Wessex and Mercia strike alike from the 860s [W]; the great army lands 865, York falls 866; the stycas end. | The first dirham wave. Birka rich enough to pay 100 pounds. Rings and bars cast from melted coin. Spillings buried after 870. Hoards on Wieringen (Westerklief, c. 850 and c. 880 (?)) show Danes living in Frisia with rings, bars, deniers and dirhams together. | Frankish silver flows north as loot and tribute. The north now has scales. |
| **c. 870–c. 930** | West Frankish coin slips into the hands of counts and bishops; the "immobilised" types begin (?). In the east, few mints: Cologne, Mainz, Strasbourg (?). | Alfred's good penny (mid 870s); the Danelaw strikes its own (from c. 895); two economies side by side in the east: coin and bullion (Torksey 872–3, Cuerdale c. 905, Vale of York 927/8). | The standard weights spread (c. 860–880 (?)); pecking begins; a lean stretch for dirhams (c. 875–900 (?)), then the Samanid flood from c. 900. Dublin (from 841, refounded 917) deals in rings by weight. | The same purse must be counted in London and weighed in York. |
| **c. 930–c. 990** | Normandy (911) strikes at Rouen (?). Otto's Saxon silver from c. 968; Cologne's penny rules the Rhine (?). Tiel and Deventer take Dorestad's place. | Grately (c. 928): one coinage, mints in towns. Edgar (c. 973): forty mints, one type, recoinages. | The dirham flood peaks c. 930–950, thins and worsens after c. 960, and is nearly over by 990 (?). Hedeby strikes again. Samarkand dirhams seen at Mainz, 965. | England is now the best-run coin land in Europe; a stranger's coin is melted on the quay. |
| **c. 990–1066** | German pennies by the hundred thousand go to the Baltic (?). Small Frisian mints. | The gelds, 991–1018; six types under Æthelred; Cnut's three; fourteen types 1035–1066; 48 mints for Harold. | The dirhams stop (by c. 1015). English and German pennies fill the hoards. The kings strike their own (990s): Sigtuna, Lund, Dublin, Nidaros (?). Denmark and Norway go over to coin by count in the 1040s–1060s (?); Gotland and the east Baltic keep weighing. | The heirs deal in pennies nearly everywhere west of Gotland. The scales are for the east. |

---

## 11. The finds that prove it

| Find | Where | Buried | What it held | What it proves | Source |
|---|---|---|---|---|---|
| **Spillings** | Othem, Gotland (found 16 July 1999) | after 870/1 | 67 kg of silver in two lots, and over 20 kg of bronze; 14,295 coins of which 14,200 dirhams, 23 Persian, 4 from Hedeby, 1 Byzantine; earliest 539, latest 870; 486 arm rings and pieces, near 60 % of the find, each ring about 100 g; coins from 69 mints in 15 lands | the size of the eastern stream; rings as weighed units; forged and copied dirhams | [W: Spillings Hoard] |
| **Cuerdale** | on the Ribble, Lancashire (found 15 May 1840) | 903–910 | more than 8,600 pieces, 32–40 kg, in a lead box; over 7,000 coins: Danelaw (about 3,000 of Cnut of York, about 1,800 St Edmund), Wessex (about 850 of Alfred, 50 of Edward), and foreign (Kufic, Byzantine, Papal, Italian, Carolingian, many from Aquitaine) in about 5 : 1 : 1; about 1,000 ounces of bars | coin and bullion in one chest; Aquitanian (Melle?) silver in northern hands; pecking | [W: Cuerdale Hoard] |
| **Vale of York** | near Harrogate (found 6 January 2007) | 927–928 | 617 coins and 65 other pieces in a gilt Frankish cup, in a lead box; things from Samarkand, Afghanistan, North Africa, Russia, Ireland, Scandinavia | how far one purse reached; church plate as a strongbox | [W: Vale of York Hoard] |
| **Hexham** | Northumberland (1832) | c. 845 (?) | some 8,000 stycas in a bronze bucket (?) | a coinage with no silver in it | [W: History of the English penny] |
| **Torksey** | the great army's winter camp, Lincolnshire | 872–3 | over 350 weights, more than 120 cut dirhams, stycas, cut silver and gold, lead gaming pieces (?) | an army that weighs | [mem: Hadley & Richards 2016] (?) |
| **Westerklief I and II** | Wieringen, North Holland (1996, 1999) | c. 850 and c. 880 (?) | I: about 1.7 kg, rings, bars, 78 Carolingian coins in a pot (?); II: bars, cut silver, about 95 dirhams and Carolingian pennies (?) | Danes settled in Frisia, weighing silver within a day's sail of Medemblik | [mem: Besteman 1999, 2004] (?) |
| **Ribe, the market layers; the Damhus hoard (2018)** | Jutland | 8th c.; c. 825 (?) | single small pennies lost across the plots; a hoard of some 250 pennies with ship and stag types (?) | coin by count in a Danish market before any hacksilver | [mem: Feveile 2006, 2019] (?) |
| **Kaupang** | Vestfold | c. 800–930 | about a hundred coins, most of them cut dirhams; cut silver; some 400 weights; moulds for bars (?) | the change from goods to weighed silver about 850–875 | [mem: Skre 2008, already a source of the goods table] |
| **Truso (Janów Pomorski)** | by Elbląg | c. 800–850 | about a thousand coins, mostly cut dirhams, and over a thousand weights (?) | the earliest weighed-silver market on the south Baltic | [mem: Jagodziński, Bogucki] (?) |
| **Coppergate** | York | 10th c. | iron coin dies, lead trial strikings, a plated false dirham | a mint at work in a Norse town; a forgery in trade | [W: Dirham] for the dirham; the rest mem (?) |
| **The Sigtuna and Lund coin dies, and the English pennies in Sweden** | — | c. 995–1030 | more of Æthelred's pennies lie in Scandinavian museums than in English ones (?) | where the gelds went | [mem] (?); [W: Danegeld] "some sixty million pence" paid in all |
| **The game's own table** | `Assets/Kapman/Game/Data/hoards_793.json` | — | real hoards with grams and clues | use it for the riddles; add any above that are missing | — |

---

## 12. Twenty-five words on the item

The line the game shows under silver in the purse or under a coin kind at a port: what was heard, from whom, how
long ago. Sureness is in the verb ("you saw", "he said", "they say"), age in words, dates by feasts. Each is true to
a place and a season; the `when` column is for the scheduler. Port ids are those of `ports_793.json`.

| # | The word | Where | When | Basis |
|---|---|---|---|---|
| 1 | "The toll-keeper says a heavier penny is coming from the palace. He does not know the day. He would not hoard the old ones." | dorestad | 793, summer | the reform of 793/4, before Frankfurt |
| 2 | "Word from the king's synod at Frankfurt, a month old: the new penny must be taken in every market. A free man who refuses it pays fifteen shillings." | dorestad, colonia, mogontia | 794, after midsummer | Frankfurt 794 c. 5 |
| 3 | "A moneyer at the Thames strand told you yesterday: King Offa's new pennies are broader and heavier. Frankish coin he takes only to melt." | lundenwic | 793–796, sailing season | Offa's second reform, 792 or 3 |
| 4 | "A Dane said at the last market: at Ribe they count the small thick pennies with the beast on them. Nobody there puts them on a scale." | ripa | 793–820 (?), the summer market | Ribe's single finds |
| 5 | "A Svea trader, home from the east since the last moon, showed you white coins with writing on both sides and no king's face. He says they go by weight." | birka, helgo | 800–830, late summer | the first dirham wave |
| 6 | "A priest from York says the pennies there are more brass than silver. Take them in York and spend them in York." | eoforwic | 830–866 | the stycas |
| 7 | "A Frisian skipper saw it at the spring market: they strike pennies at Sliesthorp now, after the pattern of Dorestad's." | sliesthorp, dorestad | 825–850 | Hedeby's coins, c. 825 |
| 8 | "The moneyer says old pennies are dead after Martinmas. His bench is crowded already. Go early or lose a day." | dorestad, quentovic, parisius | 825, from Michaelmas to Martinmas (?) | Admonitio 825 |
| 9 | "A priest who was at Birka says King Anund asked the town for a hundred pounds of silver, and the town found it." | sliesthorp, dorestad | c. 845–855, any season | Life of Anskar c. 19 |
| 10 | "They say seven thousand pounds of silver went down the Seine to the Northmen before Easter. A monk says the churches' plate went into the scales." | rotomagus, parisius, quentovic | 845, spring | chronicle `845-03-28-paris-sacked` |
| 11 | "A salt skipper from the Bay heard it a fortnight ago: Melle is burned, where the king's silver is dug." | namnetis, herio, burdigala | 848, summer | Annals of St-Bertin 848 |
| 12 | "The king's edict from Pîtres was read at the quay last week. Ten mints only, this one among them. After Martinmas the old pennies buy nothing." | quentovic, rotomagus, parisius | 864, midsummer to Martinmas | Edict of Pîtres c. 10–12 |
| 13 | "The Northmen on the Seine took four thousand pounds this year and weighed it on their own balance. Every merchant paid a tenth of what he had." | rotomagus, parisius | 866, summer | Annals of St-Bertin 866 |
| 14 | "A Frisian who wintered among the army says they pay in cut silver and take the same. Bring scales, and watch his." | eoforwic, gipeswic | 867–880, spring | Torksey; the Danelaw's two economies |
| 15 | "The Gute nicked your penny with his knife before he took it. They all do it now. Nobody takes offence." | paviken | from c. 880, summer | pecking |
| 16 | "The boats go down river when the ice breaks and come home with the silver before the first frost. A man who sells cloth here in spring is paid in promises." | aldeigjuborg | 800–1000, spring | the river season |
| 17 | "A Norse skipper said last month: in Dublin a man pays with a ring off his arm, weighed. Coin they throw in the same pan." | duiblinn | 850–950 | broad-band arm rings; Cuerdale |
| 18 | "King Athelstan's law, read out at the folk-moot: one coinage in all his land, and no penny struck outside a town. A false moneyer loses the hand, and it is nailed over his door." | lundenwic, hamwic | c. 928–939 | II Æthelstan 14 |
| 19 | "A spice seller at Mainz was paid in coins from the far end of the world. A traveller from Cordoba read the writing and said: Samarkand." | mogontia | 965, spring | Ibrahim ibn Yaqub |
| 20 | "A Cologne moneyer says they have opened veins of silver in the Saxon hills. He expects pennies enough for everyone. He said the same of wine once." | colonia | 968–980 | Widukind, c. 968 |
| 21 | "King Edgar has called in every penny in England. The new ones are all of one face, from forty towns. Your old ones go to the moneyer at his price." | lundenwic, gipeswic, eoforwic | 973, from Whitsun | Edgar's reform |
| 22 | "Sixteen thousand pounds for the fleet, they say. The moneyers have worked every day since Michaelmas, and the pennies come out thin." | lundenwic | 994, autumn | chronicle `994-09-08-london-attack` |
| 23 | "A Gute says no white coins have come from the east in years. What comes now is the emperor's pennies and the English king's." | paviken, birka (to c. 975), sliesthorp | c. 1000–1020 | the end of the dirhams |
| 24 | "They say King Olof strikes pennies at Sigtuna with his own name on them. The man who cut the dies is English." | uppsalir, paviken | 995–1022 | chronicle `995-sigtuna-coins` |
| 25 | "A Norwegian says King Harald's pennies are half copper. In Norway you must take them. Nowhere else will you be asked to." | ogvaldsnes, sliesthorp | 1050–1066 | Harald's debased penny (?) |

Notes for whoever turns these into data: 1–3 belong to the first summer and teach the player that coin changes;
8, 12 and 21 should fire as the "hard news" rule (rule 1 in `research_trade_seasons_and_hints.md` 6.1) with a
tilt of *down* on old coin; 15 and 17 are the "you saw" kind; the rest are "he said" or "they say". Dorestad lines
need `year <= 863`; Dublin `year >= 841`; Sigtuna is not a port in the table (the line is heard at Uppsala or on
Gotland); Birka is abandoned about 975 (?).

---

## 13. Compared with `goods_793.json` and `trade_rules.json`: what they lack or have wrong

### 13.1 `trade_rules.json`, the coins

1. **The penny is too light and too vague.** `"penny"` is 1.2 g and is described as "Offa's broad pennies and the
   older sceattas (about 1–1.3 g)". On the start day (21 June 793) Offa's heavier penny of 792/3 is new (about
   1.45 g (?)); sceattas went out of use forty years before and are a different thing (thick, small, often debased).
   Either split them (a `sceat` kind that only the weight lands and a moneyer will take, as scrap) or drop the
   sceattas from the note. `unit_d` 0.706 follows from the 1.2 g and moves with it (1.45 g gives 0.85).
2. **No coin changes with the years.** Every `silver_g` is one number for 273 years. Needed, at least: denier 1.3 g
   before the reform reaches a mint and 1.7 after (1.75 from 864); English penny about 1.3 (to 792), 1.45 (793–c. 875),
   1.6 (Alfred to Edgar), then by type 1.0–1.7 (973–1066) (?); dirham 2.9–3.0 to c. 950 and worse after; and the
   stycas of York at next to nothing. A small `coin_ages` list (coin, from, to, silver_g, fine) would do it.
3. **The start is a year before the money it is priced in.** The file's `about` and the goods table reckon in "a
   denier of Charlemagne's new standard of 793/4"; Frankfurt is June 794 (the chronicle has it). On 21 June 793 the
   40 deniers in the player's purse are most likely old light ones. This is a gift to the design (the first lesson
   in changing coin) but the data should say which they are.
4. **Six dirhams in a Dorestad purse in 793 is not likely.** The first dirhams reach Ladoga in the 780s and the
   Baltic about 800; none to speak of reach the Rhine. Start with none, or one as a curiosity with a line about it.
5. **Hacksilver in 793 is early.** The cut-silver economy with scales and standard weights belongs to c. 850/870
   onward. In 793 a Frisian might carry a few scraps for a smith, but the "30 g" should not read as a currency he
   uses at Ribe. Ribe in 793 probably counts its own small pennies (?).
6. **There is no gold.** The mancus (30 pence), the dinar and the nomisma are the money of gifts, wergilds and
   church dues, and Pîtres gives the rate (12 : 1). One `gold` kind by weight would cover them.
7. **No halfpenny.** The Frankish obol and the English cut halfpenny and farthing (common after 973 [W]) are how a
   loaf or a toll of a halfpenny (London, c. 1000) is paid. Not needed as a coin kind, but prices under 1 d need a way
   to be said.
8. **The dirham is 2.9 g; the lawful weight is 2.975 g** [W: Dirham]. Small; `unit_d` 1.706 becomes 1.75.

### 13.2 `trade_rules.json`, the zones

9. **The zones never change, and the money map changed more than anything else in the period.** Needed `from`/`to`
   on zones or per-region overrides:
   - `northumbria`: English penny zone only in name; stycas to 867, then the York kings' coin *and* weighed silver
     together to 954, then English.
   - `england_east`: coin and weighed silver together from 869 to c. 920.
   - `jutland` (Ribe): coin by count on the market in 793 (?); `schleswig` (Hedeby): its own penny from c. 825.
   - `danish_isles`, `skane`, `jutland`, `schleswig`: a king's coin from c. 995, by count from the 1040s (?).
   - `svealand`: Sigtuna's pennies 995–c. 1030, then weight again (?). `norway_west`, `vestfold`: 995, then c. 1047.
   - `ireland`: Dublin's pennies from 997 (the chronicle has it; the zone does not).
   - `neustria` (Rouen) after 911: Norman, with its own pennies (?).
   - The German kingdom after 843/925: `rhineland`, `saxony`, `frisia`, `rhine_delta` are no longer in one coin
     zone with `neustria`, `loire` and `aquitaine`. A West Frankish denier at Cologne is foreign coin from then on.
10. **`weight` zone, "No coin of their own"** is right for 850–990 and wrong at both ends (Ribe and Hedeby before;
    the kings' mints after).
11. **Ireland, Wales, Cornwall, Dál Riata and Shetland in 793 are not lands of weighed silver.** They are lands of
    no silver: cattle, cloth and kind. Weighed silver comes to the Irish Sea with the Northmen (from the 840s).
    A `kind` zone (barter only, silver taken as a rare good at a keen price) would be truer.
12. **`dirham` zone**: Spain (the emirs of Cordoba strike their own dirhams) is lumped with the Volga under a note
    about the Khazars' tenth. Ladoga probably weighs as much as it counts (?). Split or re-note.
13. **`byzantine` zone uses hacksilver** as a stand-in. Marked "to come" in the file; noted here so it is not
    forgotten: the nomisma (gold) and the miliaresion (silver, about 2–3 g (?)).
14. **`foreign_loss` is flat.** History gives: about 4.5 % as the moneyer's share in a quiet year (Ver 755, mem);
    much more at a recoinage, when a realm's *own* old coin loses too (794, 825, 864; England 973 and every few
    years after). The data has no way to make the player's own deniers go stale. `town_actions_793.json` already
    reads a `reform.coin` flag; `trade_rules.json` needs the list of recoinage years by zone that sets it.
15. **`foreign_loss` 0.03 in the weight lands** is fair for honest scales. Add the other half: in the weight lands a
    *heavy* coin gains and a *light* coin loses against its face, which is the whole point of weighing. If coins
    are converted through `silver_g` this happens by itself, so long as `silver_g` is right for the year (see 2).
16. **Mints are not in the port table.** BRIEF.md rule 7 says a port's mint must agree with `ports_793.json`, but
    that file has no mint field (Dorestad's is only in its note); `change_coin` reads `port.has_mint` from
    `npc_roles_793.json`. A dated list is needed. From this file: dorestad (to c. 855), quentovic (to c. 980),
    rotomagus, parisius, colonia, mogontia, traiectum (11th c.), daventre (10th–11th c.), lundenwic, hamwic (few
    coins; Winchester takes over), gipeswic (from the 970s (?)), eoforwic, fordwic/Canterbury, dofras (10th c. (?)),
    legacaestir (from c. 900), sliesthorp (c. 825; c. 900–980), duiblinn (997), burdigala and namnetis
    (Carolingian), and Melle, which is not a port. Each needs a `from` and `to`; most of mine carry a (?).

### 13.3 `trade_rules.json`, the laws

17. **There is not one coin law.** The `laws` list has arms, horses, captives and sacred things. Missing, all dated
    and all in the chronicle or section 8 above: 794 (the new denier must be taken; the fine), 805 (palace only),
    825 (old coin void at Martinmas (?)), 864 (ten mints; old coin void at Martinmas; no mixed silver; gold 12 : 1),
    c. 928 (England: one coinage, mints only in towns, the hand; no deal over 20 pence outside a town), 973 (Edgar),
    c. 1000 (IV Æthelred: strangers' bad coin, the port-watchers). A `"law": "coin"` kind with `rule` values such
    as `must accept`, `old coin void`, `mint only`, `town only` would carry them.
18. **No penalty for bad silver.** Passing false coin, refusing good coin and using false weights all had penalties
    (15 shillings, 60 shillings, flogging, the hand). `test_silver` and `lawsuit` exist as acts; the law table has
    nothing for them to read.
19. **The 864 arms law note** is right; add to the same year that Pîtres is also the coin law, so one chronicle
    entry sets both.
20. **The English toll of 0.05** is a guess with nothing behind it. The one sourced English toll on a boat like the
    player's is flat: a halfpenny or a penny a call at London about 1000 (IV Æthelred 2 (?)). For 793 the evidence
    is the kings' charters freeing a church's ships from toll at London, Fordwich and Sarre (8th century), which
    proves a toll and gives no rate.

### 13.4 `goods_793.json`

21. **Silver is not a good, and that is right. But three silver things that are goods are missing:**
    - **A balance and weights** (cat. Crafts; made: `caliphate` 2, `rus_volga` 1, `svealand` 1, `schleswig` 1 from
      c. 860; wanted: all the weight lands). It is in `town_actions_793.json` as a 12 d purchase, not in the table.
    - **Silver in the bar** for the smith (unit: a bar of about a mark, 200 g, about 120 d; made: `caliphate`,
      `ladoga` 0.5, `gotland`, `svealand`; from c. 968 `saxony` 2; wanted: `ireland`, `norway_west`, `vestfold`,
      `jutland`). Only worth having if kept tiny, as a craftsman's need, with stock of one or two.
    - **Silver arm rings** (unit: a ring of about 100 g, about 59 d by weight and more as a gift; made: `gotland`,
      `svealand`, `ireland` from c. 850; wanted: wherever a gift is owed). The best gift good in the game; the
      `gift` act would use it.
22. **`church_plate`** is there, with the right law. Its `made` list should add `byzantium` and `italy`; and its
    risk line should say what the Vale of York cup shows: north of the coin lands it is just silver by weight
    (1.5 kg is about 880 d melted, against the table's 240 d a piece: one of the two numbers is wrong, and I think
    the weight is: a chalice is nearer 300–500 g (?)).
23. **`lead`**: the note gives roofs and gutters. Add: lead is what silver is refined with and parted from, and what
    hoards are boxed in (Cuerdale, Vale of York); Melle's lead was a by-product of its silver. `made` should add the
    region that holds Melle (see 25) and, from c. 968, `saxony`.
24. **`brooches`, `silk`, `glass_beads`, `carnelian`**: the carnelian note already ties the beads to the dirham
    stream (source `noonan1998`), which is right. No change; only a cross-reference to this file.
25. **The regions have no Poitou.** Melle, the one Carolingian silver mine, sits between `loire` and `aquitaine`
    and belongs to neither description. Put it in `aquitaine`'s line ("Aquitaine: Bordeaux, the Garonne, Poitou's
    silver at Melle") rather than add a region.
26. **The regions have no Harz.** `saxony` is "Westphalia, Engern, Eastphalia"; from 968 it should also read "the
    Harz silver at Goslar", because that is what changes the Baltic's money after 990.
27. **The `about` line dates every value to "deniers of about 1.7 g after Charlemagne's reform of 793/4".** True as
    a yardstick; but see 3: the yardstick is a year younger than the start day.
28. **Sources.** `skre2008`, `noonan1998` and `coupland` are listed and are the right books for this subject. The
    table lacks: Grierson & Blackburn, *Medieval European Coinage 1* (1986); Coupland, *Carolingian Coinage and the
    Vikings* (2007); Graham-Campbell & Williams (eds.), *Silver Economy in the Viking Age* (2007); Graham-Campbell,
    Sindbæk & Williams (eds.), *Silver Economies, Monetisation and Society in Scandinavia AD 800–1100* (2011);
    Naismith, *Money and Power in Anglo-Saxon England* (2012) and *Medieval European Coinage 8* (2017); Kilger on
    the dirham waves (in Skre 2008).

### 13.5 The acts that touch silver (`town_actions_793.json`), for whoever owns that file

- `change_coin` says Frankish and English loss is 0.10 and a friendly moneyer takes 0.07. Fine as a game rule; see 14.
- `weigh_silver` says "from about 860–880 small standard weights copied from the Caliphate's spread over the whole
  north (?)": this file agrees and cannot remove the (?).
- `test_silver` puts pecking in "the later 9th century (?)": this file agrees. Before about 870 the test in the
  north should be the scale and the bend, not the knife.
- Nothing lets the player *lose* by holding the wrong coin over a recoinage day except the market refusing it. A
  line from section 12 (8, 12, 21) a month ahead is the fair warning.

### 13.6 What is needed and not mine to add (the `needs` list)

- `trade_rules.json`: dated coin weights; dated zones; a `coin` law kind; a recoinage list by zone; gold.
- `ports_793.json` or `npc_roles_793.json`: mint `from`/`to` by port.
- `goods_793.json`: a balance and weights; silver arm rings; perhaps bar silver; the two region lines (25, 26).
- `wording_793.json`: the 25 lines of section 12 as wording, if the user wants them.
- `chronicle_793.json`: entries for 825 (old coin void), 848 (Melle burned), 864 as a coin law as well as an arms
  law, c. 968 (the Rammelsberg), if they are not there (I found coin entries for 794, 805, 973, 995 and 997 named in
  `Design/economy_and_trade.md`; I did not read the whole chronicle).

---

## 14. Sources

**Checked this session (Wikipedia, 6 Oct 2026):** Carolingian monetary system; Edict of Pistres (en) and Édit de
Pîtres (fr); Sceat; History of the English penny (c. 600–1066); Coinage in Anglo-Saxon England; Viking coinage;
Dirham; Hacksilver; Spillings Hoard; Cuerdale Hoard; Vale of York Hoard; Danegeld; Rammelsberg;
Otto-Adelheid-Pfennig; Ibrahim ibn Yaqub (the page has nothing on Mainz: that fact rests on the project's own
chronicle entry for 965).

**Not found (404), so everything from them is from memory and marked:** Mines of Melle (en) and Mines d'argent de
Melle (fr); Hedeby coins; Silver Age (Viking).

**Primary texts named (none quoted word for word):** the capitulary of Ver (754/5); the Synod of Frankfurt (794),
MGH Capit. I no. 28; Thionville (805); Louis the Pious's Admonitio (825); the Edict of Pîtres (864), MGH Capit. II
no. 273; the Annals of St-Bertin (845, 848, 866); Rimbert, Life of Anskar, c. 19, 20, 30 (Robinson 1921 is the
public-domain translation); II and VI Æthelstan, III Edgar, IV Æthelred (Thorpe 1840 is the public-domain
translation); the Anglo-Saxon Chronicle for the gelds (Giles 1847, Ingram 1823); Widukind of Corvey (c. 968);
Ibn Fadlan (922); Ibn Rusta (c. 903–913); Ibrahim ibn Yaqub (965); Domesday Book (1086, for customs of 1066).

**Modern works relied on from memory:** Grierson & Blackburn 1986; Coupland 1988, 1990, 2007; Metcalf 1998;
Naismith 2012, 2017; Noonan 1998; Skre (ed.) 2008 (Pedersen on the weights, Kilger on the dirhams, Blackburn on the
coins); Steuer 1987; Sperber 1996; Kyhlberg 1980; Malmer 1966, 2007; Archibald 1990, 2011; Hadley & Richards 2016;
Besteman 1999, 2004; Feveile 2006; Graham-Campbell & Williams 2007; Sheehan on the Irish arm rings; Ježek 2013;
Ilisch on the Frisian mints; Téreygeol on Melle.

**What I could not check and would check first:** the chapter numbers and penalties of Frankfurt, 818/9, 825, 861
and Pîtres against the MGH text; the list of Æthelstan's moneyers beyond London and Canterbury; IV Æthelred's
toll figures; the weights of Offa's and Alfred's pennies; the Westerklief, Torksey, Truso, Kaupang and Damhus
counts; the end date of Dorestad's mint; the dates of the standard weights and of pecking; Harald Hardrada's
copper; all of Melle.
