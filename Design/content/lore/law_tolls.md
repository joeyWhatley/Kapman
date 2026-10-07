# Law, tolls and the merchant, 750 to 1070

Lore for Kapman, written 6 October 2026. The companion table is `laws.json` in this folder
(`[{year, where, what, means, source}]`, 113 rows, sorted by year); section 17 compares all of it with
`Assets/Kapman/Game/Data/trade_rules.json`.

**How the sources are marked.** The web was mostly shut while this was written (no searches at all; a dozen pages
fetched). So every fact carries one of three marks:

- `[W: page]` : read today on the page named (the list is in section 19). The pages were read through a
  summarising tool, so a `[W]` means the fact was on the page, not that I saw the clause's own words.
- `[mem]` : from my memory of the edition or the standard books; the clause is real and I am sure of its gist, but
  the wording and the clause number were not checked today.
- `(?)` : I am not sure of this detail. Check it before it is shown to a player as a fact.

Nothing here is quoted from a modern translation. The few quoted lines are from Thorpe's *Ancient Laws and
Institutes of England* (1840; Benjamin Thorpe died 1870) as printed in Thatcher's *Library of Original Sources*
(1901) and read today on the Fordham sourcebook, and from Bellows's *Poetic Edda* (1923), already in
`texts_793.json`. Latin is quoted from memory and marked so.

---

## 1. The short of it: twelve things the game should know

1. **A merchant is a stranger, and a stranger has no kin to avenge him.** Everywhere the answer is the same: he
   stands under somebody's hand. In Francia the king's (the privilege of 828). In England the king's reeve and his
   host. In the North the town's peace, the king's man and his own partners. A player who has no protector in a port
   is fair game there. Standing in a port is, in law, *whose man you are there*.
2. **The Frankish tenth is real, and it is taken at very few places.** The sources name three: Quentovic, Dorestad
   and the Clusae (the Alpine passes). Everywhere else in the realm a privileged man goes free, and an unprivileged
   one pays small old tolls by the boat, the cart and the load.
3. **Toll is taken on trade, not on travel.** Goods carried for your own house, for the palace or for the army go
   free (805). So do pilgrims (c. 755, 796), unless they are found trading (796). "It is not for sale" is the oldest
   lie on the quay, and the toll man knows it.
4. **A charter is worth more than a cargo.** The great abbeys carried toll-free by royal charter. A merchant carrying
   for an abbey, with its letter aboard, is free too. This is the best reward the game can give.
5. **Three things may not leave Francia:** mail coats (779), weapons to the Slavs and Avars (805), and after 864, in
   the West Frankish kingdom, weapons and horses to the Northmen on pain of death. Slaves may not be sold over the
   border (779), and Christians not to heathens.
6. **Buy before witnesses, in the open, by day.** Every law of the age says so, Frankish and English. A thing bought
   without witnesses is a stolen thing until you prove otherwise, and the proof is an oath with helpers.
7. **Proof is not evidence. Proof is an oath, your helpers, or the ordeal.** A man with twelve friends in a port
   wins his case. A stranger with none goes to the hot iron, the kettle or the lots.
8. **Every man has a price.** A Frisian freeman is 53 solidi and a penny in his own law; an English ceorl 200
   shillings; a Dane and an Englishman alike eight half-marks of gold under Alfred and Guthrum. Kill a man, even by
   mischance at the oar, and this is the bill. The crew have prices too.
9. **The sea gives no title.** A wrecked ship and her goods belong, by the custom of most coasts, to the lord of the
   shore, and the men may be seized. The exceptions are treaties (994, 911) and decent neighbours (the Sembi).
10. **False measure is sin and crime both.** One weight, one measure, the king's: Soissons 744, the Admonitio of 789,
    Edgar, Pîtres. In the North there is no king's measure: each man brings his own scales, and both weigh.
11. **In the North the toll is a gift and the law is the assembly.** Godfred moved Reric's merchants to Sliesthorp
    in 808 for the taxes. At Birka the king could not decide alone: the assembly and the lots did.
12. **The law changes six times in the game's span:** 793/4 (the heavy penny), 828 (the merchants' privilege), 864
    (Pîtres), 878 to 890 (the Danish border in England), c. 1000 (London's tariff), 1027 (Cnut's roads to Rome).

---

## 2. Who the merchant is in law

| Thing | What it was | Source |
|---|---|---|
| The word | *negotiator*, *mercator* in Latin; *ciepa*, *ceapman*, *mangere* in English; *kaupmaðr* in Norse; *kapman* in Frisian and Low German (the game's name). | [mem] |
| His standing | A free man, usually, of his own people's law. Law was personal in Francia: a Frisian was judged by Frisian law wherever he stood, a Frank by Frankish. The first question in a Frankish court is "by what law do you live?" | [mem]: the principle of personality of law; Capitulare missorum 802 and the Lex Ribuaria 35 (31) |
| A stranger's price | In Ribuarian land (Cologne): a foreign Frank 200 solidi, a foreign Burgundian 160, a foreign Roman 100, **a foreign Alaman, Frisian, Bavarian or Saxon 160**. | Lex Ribuaria 40 (36) [mem] |
| A stranger's kin | Where he has no kin, the king is his kin: the king takes his wergild, or two thirds of it. | Ine 23 [mem]; II Cnut 40 (?) |
| Collective blame | When a Frisian merchant killed a count's son at York (c. 773), all the Frisians left the city in fear, Liudger with them. One man's deed fell on his whole people in a port. | Altfrid, *Vita Liudgeri* I.11-12 [mem] |
| Partners | The Norse *félag*: men who lay their goods together, share ship, profit and loss, and are each other's heir abroad. Runestones name *félagi*. | [mem]; rune stones of the 10th and 11th centuries |
| Sworn clubs | Forbidden in Francia in 779 as *gildonia*: men may agree to help each other in alms, fire and **shipwreck**, but may not swear to it. | Herstal 779 c. 16 [mem] |
| The first merchants' law of their own | The merchants of Tiel, c. 1020: they judge "not by law but by their own will", say the emperor gave them a charter for it, clear debts by oath, and drink together on feast days from a common fund. | Alpert of Metz, *De diversitate temporum* II.20 [mem]; chronicle `1020-tiel-merchants` |
| Frisians abroad | A Frisian quarter at Mainz, "the best part of the town", burned in 886. Frisians at Worms, Cologne, Duisburg (893), York, London, Birka, Sigtuna (rune stones U 379 and U 391, "the Frisians' guild-brothers", 11th century). | Annals of Fulda 886 [mem]; Prüm polyptych 893 [mem]; U 379, U 391 [mem] |

**In play.** The game already has standing by port (`reputation_793.json`). Law gives it a floor and a ceiling:
*no protector* (a stranger: any quarrel goes against you), *a host* (he answers for you, and you for him), *the
king's man* (the 828 privilege, an abbey's letter, a reeve's friendship). The York story is a card: a countryman's
crime closes a port to every Frisian for a season.

---

## 3. Tolls

### 3.1 The words

A Frankish charter of toll freedom lists what its holder need not pay. The list is the best picture we have of what
a boatman did pay [mem: the formula of Carolingian immunity charters, for example Saint-Denis 753 and 814,
Saint-Germain 779]:

| Word | For what |
|---|---|
| *teloneum* | the toll proper: on goods brought to sell, and on the sale |
| *ripaticum* | for lying at a bank or landing (Dorestad, 777) |
| *portaticum* | for entering a harbour or a gate |
| *pontaticum* | for passing a bridge |
| *rotaticum* | by the wheel, on carts |
| *pulveraticum* | "dust money", on pack beasts and droves |
| *cespitaticum* | for treading the turf of the towpath |
| *salutaticum* | a gift of greeting to the officer |
| *laudaticum* | for leave to trade (?) |
| *mutaticum* | for changing boats or beasts (?) |
| *navigium*, *barganaticum* | by the boat, by the barge (?) |

Three things follow for the game. A toll is **small fixed sums at many places** as often as a share at one. The
**greeting gift** to the officer is in the law's own list. And a boatman on the Rhine met a rope across the river
more than once between Dorestad and Mainz: the capitulary of 805 names it.

### 3.2 Dorestad

| Year | What | Source |
|---|---|---|
| before 753 | Pippin III and Carloman give the church of St Martin at Utrecht a tenth of the royal income in the land: of estates, of tolls and of trade. Pippin confirms it in 753; Charlemagne in 769; Louis the Pious in 815. | DD Kar. 1 nos. 4, 5, 56 (?) [mem] |
| 8 June 777 | Charlemagne, at Nijmegen, gives St Martin's the estate of Leusden, and at Dorestad the church called Upkirika and the bank toll on the Lek (*ripaticum*), with an island. | DD Kar. 1 no. 117 [W: nl Dorestad, for the ripaticum and the year; the rest mem] |
| 779 | Saint-Germain-des-Prés is freed from toll at Rouen, Quentovic, Amiens, Maastricht and Dorestad. | [W: en Quentovic] |
| 793 to c. 830 | The mint strikes the king's pennies with the town's name and, under Louis, a ship. A royal officer (a *procurator* or *praefectus*) holds the toll. The rate is the tenth. | the tenth: the privilege of 828 [mem]; the rest: coins |
| 828 | The merchants' privilege: toll-free everywhere, "except at Quentovic, Dorestad and the Clusae, where the tenth is taken for our use". | Formulae imperiales 37 [mem] |
| 831 | Louis the Pious frees the church of Strasbourg's men from toll through the whole realm, with the same three exceptions. | BM2 890 [mem] |
| 834 to 837 | Dorestad raided four years running. In 837 the Danes "took tribute" there. | Annals of St-Bertin [mem] |
| 841, 850 | Lothar gives Walcheren to Harald; Rorik takes Dorestad and Lothar accepts him as his man, to hold it and keep other Danes out. The toll is now a Dane's. | Annals of St-Bertin and of Fulda 850 [mem]; chronicle `850-rorik-dorestad` |
| 863 | The last raid. The merchants flee. | Annals of St-Bertin 863 [mem] |
| 896 | King Zwentibold confirms to the church of Utrecht its old rights; the toll once taken at Dorestad is now taken at Tiel (and Deventer). | [W: en Tiel, for a toll right at Tiel in 896]; the wording (?) |
| 975 | Otto II frees the merchants of Magdeburg from toll everywhere in the realm **except at Mainz, Cologne, Tiel and Bardowick**. Tiel is now what Dorestad was. | DD O II 112 [mem] |
| 1018 | Count Dirk III takes a toll of his own on the Merwede at Vlaardingen from the Tiel men going to England. The emperor's army sent against him is beaten, 29 July. | Alpert II.20-21; Thietmar VIII [mem]; chronicle `1018-07-29-vlaardingen` |

**In play.** Dorestad's tenth is right as the game has it. What is missing is *who takes it*: the king's man to
850, Rorik's man to c. 863, nobody after. And the church of Utrecht has a tenth of the tenth: the bishop's man
stands beside the toll man at the dam head. A player carrying for St Martin's of Utrecht is among friends at the
toll house.

### 3.3 Quentovic

| Year | What | Source |
|---|---|---|
| 779 | Saint-Germain-des-Prés free of toll there. | [W: en Quentovic] |
| c. 787 to 806 | **Gervold**, abbot of Saint-Wandrille, is for many years the king's *procurator* of trade, "collecting the tolls and dues in the various ports and towns, above all at Quentovic". He is Offa's friend and goes to him on the king's errands. | *Gesta abbatum Fontanellensium* 12 (16) [mem]; the office and the man [W: en Quentovic] |
| c. 790 | The quarrel over the marriages. Charlemagne shuts the ports of Gaul to English ships. Gervold talks him out of the worst of it. | *Gesta abb. Font.* [mem]; [W: en Offa of Mercia] |
| at its height | The royal toll is 10 % on each exchange. | [W: en Quentovic] |
| 828, 831 | One of the three places where even privileged men pay the tenth. | as above |
| 842 | Raided at dawn by a fleet from England; held only for a payment. | Annals of St-Bertin 842 [mem]; [W: en Quentovic] |
| c. 858 to 868 | **Grippo**, *praefectus* of the emporium of Quentovic, goes to the English kings on Charles the Bald's business and is saved from shipwreck by St Wandrille. | *Miracula S. Wandregisili* [mem] |
| 864 | One of the ten mints the edict of Pîtres leaves standing. | Pîtres c. 12 [mem] |

**In play.** Gervold is a real man a player can meet at Quentovic between 793 and 806: an abbot who is also the
chief toll officer of the Channel coast. He is the person who can close or open England to you. Grippo is his
heir in the office sixty years on.

### 3.4 The rest of the realm

- **Pilgrims go free.** Pippin, c. 755: no toll from pilgrims to Rome at the passes, bridges or ferries. [mem:
  Capit. I no. 13 c. 4]
- **Old tolls only.** Herstal 779 c. 18; Thionville 805 c. 13 (section 4). A new toll is unlawful, and a player can
  refuse it, if he dares and knows the law.
- **No toll where no service.** 805 and again 809 (?) and 820 (?): none where a rope is stretched over the river,
  none under a bridge where the boat is given no help, none in open country where there is no bridge or ferry.
  [mem]
- **Not on your own goods.** 805: none from men who carry their own without meaning to trade, from house to house,
  to the palace or to the army. [mem]
- **The fair of Saint-Denis** (9 October, for weeks): Pippin's judgment of 753 gives all its tolls to the abbey and
  names the Saxons (the English) and Frisians who come to buy wine and honey (?). The king's officers at Paris had
  been taking four pennies a head from the merchants; the abbey won. [mem: DD Kar. 1 no. 6]

### 3.5 England

| Year | What | Source |
|---|---|---|
| 733 to c. 765 | The kings of Mercia and Kent free one ship, or two, of a church from toll at London (Rochester, the bishops of London and Worcester, Minster in Thanet), at Fordwich and at Sarre (Minster, Reculver). One says: if the ship is wrecked or worn out, the freedom passes to the one built in her place. | Sawyer 86-88, 91, 98, 29, 1612, 1788 (?) [mem]. The numbers of ships by charter (?) |
| c. 680 | In Lundenwic a Kentishman buys before two or three honest men or the king's wic-reeve, and vouches his seller at the king's hall in the town. | Hlothhere and Eadric 16 [W: Fordham] |
| 857 | Burgred of Mercia grants the bishop of Worcester a plot in London by the west gate, with the free use there of **the scale, the weights and the measures**, as is the custom in the port. | Sawyer 208 [mem] |
| 889 | Alfred and Æthelred grant the bishop of Worcester a plot at Hwætmund's stone in London: what is sold in the courtyard pays its toll to the bishop; what is sold in the street or on **the trading shore** pays to the king. | Sawyer 346 [mem] |
| 898/9 | The meeting at Chelsea on the rebuilding of London: plots at Æthelred's hythe (Queenhithe), where the holders' ships may lie. | Sawyer 1628 [mem] |
| c. 1000 | **IV Æthelred**: the London tariff (section 7.4). | [mem]; the outline [W: Early English Laws] |
| 1023 (?) | Cnut gives Christ Church, Canterbury, the port of Sandwich and its tolls, on both shores, as far inland as a small axe can be thrown from a ship afloat at high tide. | Sawyer 959, of doubtful genuineness (?) [mem] |
| before 1066 | Domesday's customs: Chester, Lewes, Dover (section 7.5). | Domesday Book [mem] |

There is **no English rate for 793**. The toll was real (the charters prove it); its size is unknown. The first
rates are flat sums by the ship (c. 1000).

---

## 4. The capitularies

The Frankish kings' orders, read out by the counts and the *missi*. Latin from memory.

### 4.1 Herstal, March 779 (MGH Capit. I no. 20)

[W: en Capitulary of Herstal, for the date, the 23 chapters and the three trade matters: tolls, slave sales under
a bishop's or count's eye, arms exports. Chapter numbers and wording mem.]

| c. | What it says | In play |
|---|---|---|
| 10 | A perjurer loses his hand. | A false oath at a toll house or in a suit is not a small thing. |
| 16 | No sworn *gildonia*. Men may agree to help each other in alms, fire and shipwreck; nobody may swear to it. | A wreck club among the Dorestad men is lawful if it is a promise and not an oath. A good early "work". |
| 18 | Tolls that were forbidden before stay forbidden; none is taken except where it was of old. | The player may know which tolls are old. |
| 19 | Slaves are sold in the presence of the bishop or the count, the archdeacon or the hundred-man, or before good witnesses. **Nobody sells a slave beyond the march.** | A sale of a slave in Francia needs an officer present; export is unlawful from 779. |
| 20 | *De brunias*: nobody sells a mail coat out of our realm. | Already in `trade_rules.json`. |

### 4.2 Between 789 and 803

| Year | Text | What it says | Source |
|---|---|---|---|
| 789 | Admonitio generalis c. 74 | Equal and just measures and just weights in the towns and the monasteries, for giving and for taking, "as the Lord's law commands" (Proverbs 20:10). | Capit. I no. 22 [mem] |
| 789 | c. 5 | No usury for anybody, cleric or lay. | [mem] |
| 789 | c. 75 | Strangers, pilgrims and the poor are to be taken in. | [mem] |
| 789 | c. 81 | No work on Sunday; no markets (?). | [mem] |
| 794 | Frankfurt c. 4 | Nobody sells grain dearer than the public bushel at: oats 1 d, barley 2 d, rye 3 d, wheat 4 d. Bread: twelve wheat loaves of two pounds for 1 d, fifteen of rye, twenty of barley, twenty-five of oats. | Capit. I no. 28 [mem] |
| 794 | c. 5 | The new pennies are to be taken everywhere, if they bear the king's name and are of pure silver and full weight. Who refuses them: a free man pays 15 solidi. | [mem]; see `economy/silver_coin.md` |
| 797 | Capitulare Saxonicum c. 11 | What a solidus is in kind, for paying fines in Saxony: a yearling ox; among the Bortrini 40 bushels of oats or 20 of rye, among the northern Saxons 30 of oats or 15 of rye; one and a half *siclae* of honey (?). In silver, twelve pennies. | Capit. I no. 27 [mem]; chronicle `797-10-28-capitulare-saxonicum` |
| 802 | Capitulare missorum generale c. 27 | Nobody, rich or poor, denies a roof, a fire and water to a pilgrim or any traveller. | Capit. I no. 33 [mem]; chronicle `802-10-aachen-laws-802` |
| 803 (?) | Capitulare missorum c. 7 | Arm-rings (*bauga*) and mail coats are not given to merchants. | Capit. I no. 40 [mem] |
| c. 803 to 806 | "On trading by night" | Nobody trades at night in gold or silver vessels, slaves, gems, horses or beasts, **except food and fodder for men on a journey**. By day, before everybody and before witnesses. | Capit. I no. 55 c. 2 (?) [mem] |

### 4.3 Thionville (Diedenhofen), late 805 (MGH Capit. I nos. 43-44)

[W: de Diedenhofener Kapitular, for the date, the nine places and the arms ban. The officers, the penalty and the
other chapters mem.] Chronicle: `805-12-thionville-border-posts`, `-arms`, `-tolls`, `-famine`, `-coin`.

| c. | What it says | In play |
|---|---|---|
| 4 | In a famine nobody sells food out of the empire. | A famine shock in Francia closes grain export. |
| 5 | Nobody carries arms within the land: shield, lance, mail coat. | A merchant crew goes unarmed ashore in Francia, by law. |
| 7 | Merchants going to the Slavs and Avars go no farther than **Bardowick** (under Hredi), **Schezla** (Madalgaud), **Magdeburg** (Aito), **Erfurt** (Madalgaud), **Hallstadt** (Madalgaud), **Forchheim, Premberg and Regensburg** (Audulf) and **Lorch** (Warnar). They carry no arms or mail to sell. If they do, all they carry is forfeit: half to the palace, half shared by the officers and the man who found it. | The eastern border is a line of nine named markets with a named officer each. A searcher earns by finding. |
| 13 | Old and just tolls are taken from merchants, at bridges, at ferries and at markets. New and unjust ones are not: where ropes are stretched, where ships pass under bridges, and the like, where no help is given to the traveller. Nor from men carrying their own goods from one house to another, to the palace or to the army, without meaning to trade. | The player can claim "my own goods". Doubtful, if the hold is full of wine. |
| 18 (?) | False coin: pennies struck only at the palace, unless the king orders otherwise. | See `silver_coin.md`. |

### 4.4 Nijmegen 806, the cloak prices of 808, Aachen 809, Boulogne 811

| Year | What it says | Source |
|---|---|---|
| 806 | Usury is "where more is asked back than was given". *Turpe lucrum* (foul gain): to buy grain or wine at harvest for 2 d the bushel and hold it till it sells for 4 or 6. *Negotium* (honest trade) is buying what one needs and selling the rest. | Capit. I no. 46 cc. 11-17 [mem] |
| 808 | The best double cloak not dearer than 20 solidi, a single 10; a coat lined with marten or otter 30, with *sismus* (?) 10. Buyer and seller who break it pay 40 solidi, and 20 to the informer. | Capit. I no. 52 c. 5 [mem]; in `economy/cloth_wool.md` |
| 809 | No toll where there is no bridge or ferry to help. Markets are not held on Sunday, except where they are of old. | Capit. I nos. 61-62 (?) [mem]; chronicle `809-sunday-markets` |
| 811 | Boulogne: no bishop, abbot or abbess gives or sells a mail coat or a sword to an outsider; whoever holds a benefice keeps a ship ready (?) | Capit. I no. 74 c. 10 [mem]; chronicle `811-10-boulogne-capitulary` |

**In play.** The debt of 480 d: by the law of 789 and 806 the count may not take interest. He takes it another
way: a pledge, a share of the cargo, or a price fixed in his favour. That is why the game's debt is "a fixed sum due
on a day" (DECISIONS no. 10), and it is right.

### 4.5 The privilege of the merchants, 828 (Formulae imperiales 37)

A model letter from Louis the Pious's chancery, the *praeceptum negotiatorum*: the form by which the emperor took
named merchants under his hand. [mem throughout; the German Wikipedia page for it was missing today.] It says:

- These men are under our protection. Nobody is to disturb them, seize their ships for the public service, or
  demand army-fine or the other public burdens of them (?).
- They come to the palace **each year, or every second year, in the middle of May**, and serve our chamber
  faithfully with their trade as well as their own.
- They trade freely through the realm and pay **no toll anywhere, except at Quentovic, Dorestad and the Clusae,
  where the tenth is taken for our use.**
- If a suit is brought against them that cannot be settled at home without heavy loss, it waits until it comes
  before us, or before **the master we have set over them and the other merchants**.

**In play.** This is the game's great charter, from 828. It makes a house "the emperor's merchants". It costs a
journey to the palace at Aachen (or Ingelheim, Nijmegen: where the court is) in mid May with gifts, and service
after: the chamber's commissions come first. It gives: freedom from every toll but the three tenths; the right to
have a suit moved to the king; the ship safe from seizure. An heir must renew it under a new king.

### 4.6 The edict of Pîtres, 25 June 864 (MGH Capit. II no. 273)

[W: en and fr pages, for the date, the new penny of 1.75 g with the monogram, the 12 : 1 rate, the arms ban, death
for selling horses, the private castles to be pulled down by 1 August, the bridges, flogging or fines for mixed
metal. The clause numbers and the rest mem.] It binds **Charles the Bald's kingdom only**: the west. Dorestad,
Tiel, Cologne and the Rhine are Lothar II's and Louis the German's, and Pîtres is not law there.

| c. | What it says | In play |
|---|---|---|
| 8-11 | New pennies from 1 July; after Martinmas (11 November) only the new are taken. Sworn men in every market see to it. | The moneyer's bench is crowded before Martinmas (`silver_coin.md`). |
| 12 | Ten mints only: the palace, Quentovic, Rouen, Reims, Sens, Paris, Orléans, Chalon, Melle, Narbonne. (The English page says the mints were cut to three: it is wrong or means something else.) | Coin can be changed at Quentovic, Rouen and Paris among the game's ports. |
| 15-18 | Who refuses a good penny: a free man pays the king's ban; an unfree man is flogged (sixty strokes (?)). | A seller in the west may not refuse the new coin. |
| 19 | Each count lists the markets of his county: those from the grandfather's time, the father's, and the new ones, and by whose leave. The king will decide which stay. | A new beach market can be shut by the count. |
| 20 | Measures are made true to the palace's; who gives with a small measure and takes with a great one is punished: a free man fined, an unfree man flogged. Bread and meat by the penny's worth, wine by the just sester. | False measure in the west has a named penalty. |
| 23-24 | Gold and silver are sold pure; a pound of refined gold for twelve pounds of new pennies. | A sourced gold rate. |
| 25 | **Whoever, after 1 July, gives a mail coat, any weapons or a horse to the Northmen, for any reason or for any ransom, shall lose his life without pardon, as a traitor to his country who hands Christendom over to the heathen.** | Already in `trade_rules.json`. Note "for any ransom": not even to buy back your own kin. |
| 31 | Men who fled the Northmen from the coast may stay where they are and go home when they can; nobody enslaves them. | Refugees as passengers and hires. |
| 37 (?) | The works on the Seine: the fortified bridge at Pîtres (Pont-de-l'Arche). | From the late 860s the Seine is barred above Pîtres. |

---

## 5. The Lex Frisionum and its neighbours

### 5.1 What it is

The law of the Frisians, written down for Charlemagne about 802, perhaps for the assembly at Aachen that October;
it may be a working draft and was never finished. It survives only in Herold's print of 1557. Twenty-two titles of
fines, then the "additions of the wise men" Wlemar and Saxmund. It is the player's own law. [W: en and de Lex
Frisionum, for the date, the 1557 print, the two parts, the four ranks, the three lands, the kettle ordeal and the
two wise men. The sums and clause numbers below are mem.]

**Three lands, three scales.** Middle Frisia between the Vlie and the Lauwers is the base. West Frisia between the
Zwin (*Sincfal*) and the Vlie, where Dorestad's hinterland lies, and East Frisia between the Lauwers and the Weser
have their own figures.

**Four ranks:** the noble (*nobilis*), the free man (*liber*), the half-free (*litus*) and the slave (*servus*).

### 5.2 What a man costs

| | Wergild (Tit. I, middle Frisia) | Note |
|---|---|---|
| Noble | 80 solidi | |
| Free man | 53 solidi and 1 penny | |
| Litus | 27 solidi less 1 penny to his lord, and 9 solidi less a third of a penny to his kin | |
| Slave | his worth, as his owner swears it | he is goods |

The proportion is 3 : 2 : 1. In another place (Tit. XV) the same is given by weight: a noble 11 pounds, a free man
5 and a half, a litus 2 pounds 9 ounces, a slave 1 pound 4 and a half ounces (?). The Frisian solidus is a small
one, of two, two and a half or three "pennies" by land, where the "penny" is an old gold piece (?): do not turn
these into the game's deniers without a decision. As a rule of thumb for play: **a free Frisian is worth about as
much as the player's whole debt, and a noble half as much again** (?).

A wergild is paid threefold for a man killed in a church or in the duke's court, and ninefold for one killed in the
army's camp (Tit. XVII) [mem].

**Women** have the same wergild as men of their rank [W: de Lex Frisionum].

**Who may be killed without payment** (Tit. V): a hired champion; a man who falls in battle; an adulterer; a thief
found in the pit he is digging under a house; a man caught with a torch at another's house; **a breaker of a
shrine**; and a newborn child put out by its mother. [mem]

### 5.3 What touches a merchant

| Title | What it says | In play |
|---|---|---|
| I | An accused man clears himself by his oath with helpers of his own rank: a noble accused of killing a noble swears with eleven; a man of lower rank needs more, of higher rank fewer. | The number of friends needed scales with whom you wronged. |
| III | Theft: twofold or more back, and the peace-money to the king. A man accused of theft who cannot find helpers goes to **the boiling water**. | See section 6. |
| XIV | A man killed in a crowd, and nobody knows by whom: the accuser may name up to seven men. Each swears with eleven helpers. Then they go to the church and **lots** are laid on the altar (or on relics, where there is no church): two sticks, one cut with a cross, wrapped in clean wool; a priest or an innocent boy draws. If the cross comes up, the seven are clean. If not, each makes his own stick with his own mark, and they are drawn until one is left. | A killing on a crowded dam head at Dorestad is settled exactly so. A whole card. |
| XVII | "This is the ban." Who starts a fight in the army pays ninefold. Who kills a man in the duke's court, or the duke's messenger, pays ninefold (?). **Who sells a slave to heathen peoples pays his own wergild to the king** (?). | A Frisian law against the slave trade to the Danes, in 802. |
| XVIII | Work on a Sunday: a free man pays 12 solidi beyond the Lauwers and 4 elsewhere (?); a slave is beaten or his lord pays. | No loading on Sunday in Frisia. |
| XXI | *De plagio.* Who sells a free man out of the country pays for him as if he had killed him. | Kidnapping for sale is a wergild matter. |
| Additio XI | "Who breaks into a shrine and takes holy things from it is led to the sea, and on the sand that the tide covers his ears are slit, he is gelded, and he is offered to the gods whose temple he dishonoured." | A heathen law left standing in a Christian king's book. The most quoted line in Frisian law, and a true easter egg: a monk copied it. |

Lesser oaths were sworn on one's own clothing or on a coin or a beast; great ones on relics (?). [mem: Tit. III and
XII]

### 5.4 The neighbours' laws, in one table

| Law | When | A free man's wergild | Notes for a merchant | Source |
|---|---|---|---|---|
| Lex Salica (the Franks) | c. 510; Charlemagne's text c. 802 | 200 solidi; a king's sworn follower 600; a Roman 100 | In the Carolingian reckoning a solidus is 12 d: 200 solidi are 2,400 d, ten pounds. | [W: en Weregild for 200; the rest mem] |
| Lex Ribuaria (Cologne) | 7th century, rewritten c. 802 | 200; **a foreign Frisian or Saxon 160** | Title 40 (36) lets a wergild be paid in kind: an ox 2 solidi, a cow 1, a sword with scabbard 7, a mail coat 12, a helmet 6. | [mem]; `Design/economy_and_trade.md` 1.2 |
| Ewa ad Amorem (the Franks of Hamaland: Deventer, the IJssel) | c. 802 | a *homo Francus* 600, a free man 200, a litus 100, a slave 50 | The law at Deventer. | [mem] |
| Lex Saxonum | 802 | a noble 1,440 solidi; a litus 120 | The Saxon solidus is in cattle (4.2, 797). | [mem] |
| Capitulatio de partibus Saxoniae | c. 782 | | Death for burning a body in the heathen way, for eating meat in Lent out of scorn, for hiding from baptism. Softened in 797. | [mem] |
| English laws | 600 to 1030 | a ceorl 200 shillings (Wessex and Mercia); a thegn 1,200; a Welshman with a hide of land 120 | A West Saxon shilling is 5 d, a Mercian 4 d: a ceorl is 1,000 or 800 pence. | [W: en Weregild; Fordham for the Mercian 200 and "six times as much"] |
| Northumbria ("the North People's law") | c. 920 to 1020 | a ceorl 266 thrymsas; an archbishop or ætheling 15,000; the king 30,000 | A thrymsa is 3 d. | [W: Fordham] |
| Alfred and Guthrum | 878 to 890 | Dane and Englishman alike **8 half-marks of pure gold**; a ceorl on rented land and a Danish freedman 200 shillings | Section 7.2. | [W: en Treaty of Alfred and Guthrum] |
| II Æthelred | 994 | an Englishman or a Dane, free: **25 pounds**; a slave 1 pound | Section 7.3. | [mem] |
| Norse | written 12th century | Norway (Gulathing): a *hauldr* 96 ells' worth by old reckoning (?); Iceland: a ring-payment to four degrees of kin (*Baugatal*) | Too late to be sure for 793. Use: a free man's price is "a hundred of silver" in the sagas (?). | (?) |
| Irish | 7th and 8th centuries | Not wergild alone but honour-price by rank: a small farmer 3 *séoit*; a king of a people 7 *cumala*. A *cumal* is a slave woman, reckoned at three milch cows. | **A stranger from over the sea (*cú glas*, "grey dog") has no honour-price of his own**: he has what his host or wife gives him. | *Críth Gablach*, *Bretha Crólige* [mem] (?) |

**In play.** A table `wergild` by people and rank lets every killing, maiming or kidnapping card name its price as a
fact. The engine has `silver` and `deed`: a wergild paid is silver gone and a feud closed; a wergild refused is a
feud opened (the heir inherits one feud: DECISIONS, the house with heirs).

---

## 6. Oaths, oath-helpers and the ordeal

### 6.1 The oath

A suit in this age is not about what happened. The court decides **who shall swear**, and the oath decides the
case. The accused usually has the right to swear himself clean. His helpers do not witness the facts: they swear
that his oath is clean.

| Where | How many | Source |
|---|---|---|
| Frisia | Eleven of his own rank and himself the twelfth, for a killing; fewer for less. | Lex Fris. I, XIV [mem] |
| The Franks of Cologne | 6, 12, 36 or 72 helpers by the size of the charge. | Lex Ribuaria [mem] |
| England | Oaths are weighed: "A twelve-hynde man's oath stands for six ceorls' oaths." | [W: Fordham, Thorpe's wording] |
| Alfred and Guthrum | A king's thegn accused of killing clears himself with twelve king's thegns; a lesser man with eleven of his equals and one king's thegn. | c. 3 [W: en Treaty of Alfred and Guthrum] |
| The Danelaw, 997 | The twelve leading thegns go out with the reeve and swear on relics to accuse no innocent man and hide no guilty one. | III Æthelred 3.1 (Wantage) [mem] |
| Iceland, Norway | Panels of neighbours (*kviðr*) and oaths of 3, 6 and 12. | *Grágás*; the Gulathing law [mem], written later (?) |

**On what one swears.**

- Christians: on relics, on the altar, on a gospel book. In Frisia small oaths on one's clothing or on a coin (?).
- Danes: on a holy ring. In 876 the army at Wareham swore peace to Alfred on the holy ring, "which they would not
  before to any nation", says the Chronicle, and broke it. [mem: Anglo-Saxon Chronicle 876]
- Rus: on their weapons, by Perun and Volos, laying down shields and swords and gold rings (907, 944). [mem:
  Primary Chronicle]
- Perjury: the hand (Herstal 779 c. 10). In England a perjurer is never again oath-worthy: only the ordeal is left
  to him (I Edward 3) [mem].

**In play.** The act `oath` exists. Its odds should be a count of people: *how many in this port will swear with
you*. Friends, host, crew of the same people, a priest you gave alms to. Twelve is sure, six likely, fewer
doubtful. A stranger has none, and is offered the ordeal.

### 6.2 The ordeal

For the man with no helpers, the perjurer, the stranger and the slave. Always in the Church's hands by 800.

| Kind | How | Where and when | Source |
|---|---|---|---|
| **The lots** | Two sticks on the altar, one marked with a cross. | Frisia, 802. Also how Radbod judged Willibrord's men on Fositesland c. 700 (three casts a day for three days), and how the Svear asked their gods at Birka. | Lex Fris. XIV [mem]; Alcuin, *Vita Willibrordi* 10-11 [mem]; Rimbert, *Vita Anskarii* 19, 27, 30 [mem] |
| **The kettle** (hot water) | A stone is taken from boiling water: to the wrist for a single charge, to the elbow for a threefold one. The hand is bound and sealed, and looked at on the third day. | Frisia (theft); the Franks since the Lex Salica; England. | [W: en Lex Frisionum]; [W: Fordham, the Athelstan-age ordeal dooms] |
| **The iron** | A glowing iron of one pound (three for a threefold charge) is carried nine feet. Bound, sealed, the third day. | England, 10th century; the Franks; Denmark after c. 965. | [W: Fordham for the weights and the third day]; nine feet [mem] |
| **Cold water** | The bound man is let down into blessed water on a rope. If he sinks he is clean. | The Franks. Louis the Pious forbade it in 829; Hincmar defended it in 860; it went on. | Capit. II no. 192 c. 12 [mem] |
| **The cross** | Both parties stand with arms stretched out before a cross; the first to drop them loses. | The Franks to 818/9, when Louis forbade it as mocking Christ's passion. | Capit. I no. 138 c. 27 [mem] |
| **The morsel** (*corsnæd*) | A blessed ounce of bread and cheese: the guilty man chokes. | England, for clerics above all; 11th century. | VIII Æthelred 22, I Cnut 5 [mem] |
| **The duel** | The parties or their hired champions fight. | Franks, Frisians (the champion has no wergild), Saxons, Lombards. **Not the English before 1066.** The Norse *hólmganga*, ended in Iceland c. 1006 and Norway c. 1014 by the sagas' telling (?). | Lex Fris. V, XIV [mem] |

The procedure in England (II Athelstan 23, the dooms on the ordeal) [mem, and W: Fordham in part]: the man comes to
the priest three days before and lives on bread, water, salt and herbs; he hears Mass each day; on the day he takes
the sacrament and swears he is guiltless; no more than twelve men of each side come into the church, all fasting.

**The famous one.** About 965 the priest Poppo carried a great glowing iron before King Harald Bluetooth to prove
Christ the one God, and showed his hand unhurt. The king was baptised. [mem: Widukind III.65] A Dane's reason to
trust the iron after that.

**In play.** A card: accused of selling stolen goods at Hamwic, no witnesses to the purchase, no helpers. Pay the
price twice over; or find helpers (a day, and favours); or go to the iron (three days fasting, then *doubtful*, and
a hand that is no use for a month either way). No card that only punishes: the ordeal passed clears the name for
good in that port.

---

## 7. England

### 7.1 Buying and selling before witnesses, 680 to 1023

| Year | Law | What it says | Source |
|---|---|---|---|
| c. 680 | Hlothhere and Eadric 15 | Who keeps a stranger three nights in his own home, a trader or another come over the border, and feeds him with his own food, answers for the harm he does. | [mem] |
| c. 680 | 16 | "If any Kentish-man buy a chattel in Lundenwic, let him then have two or three true men to witness, or the king's wic-reeve." | [W: Fordham] (Thorpe 1840) |
| c. 690 | Ine 20; Wihtred 28 | A man from afar, or a stranger, who leaves the road through the wood and neither shouts nor blows a horn is taken for a thief: to be killed or ransomed. | [mem] |
| c. 690 | Ine 25 | A trader who buys among the people up-country does it before witnesses. If stolen goods are found with him and he did not, he swears clear to the value of the fine or pays 36 shillings. | [mem] |
| c. 890 | Alfred 34 | Traders bring the men they take up-country with them before the king's reeve at the folk-moot and say how many they are; and take only men they can bring to justice afterwards. | [mem] |
| c. 900 to 924 | I Edward 1 | "No man buy out of port, but have the port-reeve's witness, or that of other unlying men whom one may believe." | [W: Fordham] (Thorpe 1840) |
| c. 928 | II Athelstan 12 (Grately) | "No man buy any property out of port over twenty pence; but let him buy there within, on the witness of the portreeve." | [W: Fordham] (Thorpe 1840); chronicle `928-grately-laws` |
| c. 928 | II Athelstan 14 | "There be one money over all the king's dominion, and that no man mint except within port." Moneyers: Canterbury 7, Rochester 3, London 8, Winchester 6, Lewes 2, Hastings 1, Chichester 1, Hamwic 2, Wareham 2, Exeter 2, Shaftesbury 2, each other borough 1. A false moneyer loses the hand, and it is set up over the mint. | quote [W: Fordham]; the list [mem] |
| c. 928 | II Athelstan 18 | Nobody sends a horse over the sea, unless he gives it as a gift. | [mem] |
| c. 930 to 939 | VI Athelstan (the peace-guild of London) | The bishops and reeves of London and their men form a guild against thieves: four pence a year each; they ride after stolen beasts. What is stolen is paid for at: a horse half a pound, an ox a mancus (30 d), a cow 20 d, a pig 10 d, a sheep a shilling, a slave half a pound. | [mem] |
| c. 962 | IV Edgar 3-11 | Sworn witnesses: thirty-six in every borough, twelve in a small borough and in every hundred. Every sale is made before two or three of them. Who rides out to buy tells his neighbours first what he is going for. | [mem] |
| c. 962 | III Edgar 8 | One coinage in all the king's realm; **one measure and one weight, as is kept at London and at Winchester**; the wey of wool goes for 120 pence, and nobody sells it cheaper. | [mem]; `cloth_wool.md` |
| 1008 | V Æthelred 2; again VI 9, VII 5, II Cnut 3 | Christian men and innocent men are not sold out of the land, least of all to heathen people. | [mem] |
| 1008 | V Æthelred 24 | False weights and wrongful measures are to be put away. | [mem] |
| c. 1020 | II Cnut 8-9, 24 | One coinage; weights and measures made right; nobody buys anything over four pence, living or lying, without four true witnesses, in borough or up-country. | [mem] |

**In play.** England is the land of the witnessed sale. Three rules by year: from the start, *buy in the port
before the reeve*; from c. 928 nothing over 20 d outside a port (a beach sale of anything dearer is unlawful, and
the goods can be claimed as stolen); from c. 962 the sworn witnesses. Horses do not leave England from c. 928.

### 7.2 Alfred and Guthrum, between 878 and 890

[W: en Treaty of Alfred and Guthrum, for all five points. The wording there is a modern translation and is not
quoted.] The peace between "all the English council" and "all the people that are in East Anglia".

1. **The border:** up the Thames, up the Lea to its source, straight to Bedford, up the Ouse to Watling Street.
2. **One price:** a man killed, Englishman or Dane, is paid for with eight half-marks of pure gold; except the
   ceorl on rented land and the Danes' freedmen, at 200 shillings each.
3. **One oath:** a king's thegn accused of killing clears himself with twelve king's thegns.
4. **Warranty:** every man knows his warrantor when he buys slaves, horses or oxen.
5. **Crossing:** no slave or free man goes over to the other army without leave. If they need to trade, in cattle
   and in goods, it is allowed when **hostages are given** as a pledge of peace and as proof that a man goes with a
   clean back.

**In play.** From 878 the Lea and Watling Street are a border the chart should show. A Frisian is neither English
nor Dane, which is his luck: but the rule on warranty binds him on both sides, and the hostage rule is the model
for a bond paid at the line (`Design/economy_and_trade.md` already guesses 20 d and a day).

### 7.3 II Æthelred: the treaty with the fleet, 994

Æthelred's peace with Olaf Tryggvason, Jostein and Guthmund after the tribute of 22,000 pounds (?). [mem throughout]
It is the best **sea law** of the age:

- Every merchant ship that comes into a river mouth has peace, even if she is not of the lands in the peace.
- If she is driven ashore, and the men get themselves and what they carry into a borough, they and their goods have
  peace.
- A man of the king's peace who comes into a land outside it, and the fleet comes there: his ship and all his goods
  have peace. If he has drawn his ship up, or built a hut or pitched a tent, he and his goods have peace there.
  If he has carried his goods into a house among the goods of men outside the peace, he loses the goods and keeps
  his life, if he makes himself known.
- An Englishman who kills a Dane, or a Dane an Englishman, free man for free man, pays **25 pounds** or the killer
  is given up. A slave is paid for with a pound.
- Eight men killed is a breach of the peace; fewer are paid for man by man.
- No side takes in the other's slave, thief or foe.

**In play.** A flag `kings_peace` on the ship from 994 in English waters: a raider of Olaf's fleet who meets her
owes her peace. And the rule for a wreck near a borough: reach the walls with what you can carry and it is yours.

### 7.4 IV Æthelred: the tolls of London, c. 1000

[W: Early English Laws, for the outline: Latin only, in the *Quadripartitus*; the date argued over (some put it
under Cnut); the gates, Billingsgate, the three vessel rates, the cloth days, Rouen, Flanders, Ponthieu, Normandy,
the Emperor's men and their dues; the coin clauses after. That page's summary was thin, so the details below are
mem, and agree with what `Design/economy_and_trade.md` and the chronicle entry `1000-london-tolls` already hold.]

| Who or what | The toll |
|---|---|
| Aldersgate and Cripplegate | The guards keep them. |
| A small boat at Billingsgate | a halfpenny |
| A larger boat with a sail | a penny |
| A keel or a hulk, if she lies there | four pence |
| A ship with planks | one plank from the load |
| Cloth | toll on three days of the week: Sunday, Tuesday and Thursday |
| A boat with fish come to the bridge | a halfpenny; a larger one a penny |
| The men of **Rouen**, with wine or blubber-fish (*craspois*) | six shillings for a great ship, and a twentieth of the fish |
| The men of **Flanders, Ponthieu, Normandy and Francia** | show their goods and pay toll |
| The men of **Huy, Liège and Nivelles**, passing through | pay for the showing, and toll |
| **The Emperor's men**, who come in their own ships | "worthy of good laws, as we are". They may buy wool that is unloaded, and melted fat, and three live pigs for their ship. They may not forestall the townsmen. They pay toll, and at Christmas two grey cloths and one brown, ten pounds of pepper, five pairs of men's gloves and two little horse-kegs of vinegar; and the same at Easter. |
| Hampers of hens | one hen from the hamper |
| Hampers of eggs | five eggs |
| The butter and cheese women | a penny a fortnight before Christmas, and another a week after |

A man who holds back the toll and is found out pays it and a fine to the king (five pounds (?)). Then the coin
clauses: merchants who bring false or light coin to the port clear themselves or pay; moneyers who work in woods
forfeit their lives; **every weight is stamped to the weight by which the king's money is taken, fifteen ores to
the pound**; three moneyers in every chief port, one in every other (?).

**In play.** Two things. From c. 1000 the player's Gislinge boat pays **a halfpenny or a penny** at London, not a
share: nearly nothing. And a Tiel or Cologne house (the player's heirs, when Dorestad is gone) can be "the Emperor's
men" at London: good law, wool, three pigs, and a twice-yearly due in kind worth a great deal more than a penny.
That due is a commission in itself: find ten pounds of pepper by Christmas.

### 7.5 The customs Domesday remembers (as they stood before 1066)

[mem throughout; only the Lewes figures are in the project already.]

| Where | The custom | In play |
|---|---|---|
| **Chester** | A ship that comes or goes without the king's leave: 40 shillings from every man aboard. A ship that comes against the king's peace and his ban: ship, men and goods are forfeit to king and earl. With leave: they sell in peace, and on leaving pay four pence for each load (*lesth*). **Marten skins:** the king's reeve has the first right to buy; nobody sells one before he has seen it, on pain of 40 shillings. False measure: four shillings. Bad ale: the dung-stool, or four shillings. | The Irish Sea fur trade under a reeve's eye. |
| **Lewes** | The seller of a horse gives the reeve a penny and the buyer another; of an ox a halfpenny; **of a man four pence**, wherever in the rape he buys. | A toll by the head on a slave. |
| **Dover** | The burgesses give the king twenty ships for fifteen days a year, twenty-one men in each; for it they are quit of toll through all England. The king's messenger pays three pence for his horse's passage in winter and two in summer. | The Cinque Ports before the name. Ship service buys toll freedom. |

Also the **heregeld**, the army-tax, paid from 1012 to 1051 to keep the hired fleet; Edward ended it. [mem:
Anglo-Saxon Chronicle 1051/2]

---

## 8. The North: gift, peace and assembly

No law book from the North is as old as the game's years. The Norwegian and Icelandic laws were written from
c. 1100, the Danish and Swedish from c. 1200. What follows is what writers of the time saw, and what the later laws
say was old.

| Year | Where | What | Source |
|---|---|---|---|
| 808 | Reric, Sliesthorp | Godfred destroys the Abodrite market of Reric, "which brought his kingdom great gain by the payment of taxes", carries the merchants off to Sliesthorp and orders the Danevirke. | Royal Frankish Annals 808 [mem]; chronicle `808-summer-reric-destroyed` |
| 811 | the Eider | Peace sworn on the river by twelve men of each side, Franks and Danes. | RFA 811 [mem]; chronicle `811-spring-eider-peace` |
| c. 830, 852 | Birka | The king (Björn, later Olaf) cannot let the priests preach on his own word: "it is the custom among them that every public matter lies more in the will of the whole people than in the king's power." The lots are cast, the matter goes to the assembly, then to a second assembly in another part of the realm. Hergeir is *praefectus* of the place and the king's counsellor. | Rimbert, *Vita Anskarii* 11, 19, 26-27 [mem] (Robinson 1921 is the public-domain translation) |
| c. 850 | Sliesthorp | Horik the elder gives leave for a church at the port, "where merchants came together from all sides". Afterwards merchants from Dorestad and from Hamburg and Bremen went there freely and without fear, and the place had plenty of everything. | *Vita Anskarii* 24 [mem] |
| c. 854 to 860 | Ripa | Horik the younger gives ground for a church at Ribe. He had the bell at Sliesthorp silenced first, then let it ring (?). | *Vita Anskarii* 31-32 [mem] |
| 873 | the Eider | King Sigifrid's men ask Louis the German for peace on the border, "so that merchants of either realm, going and coming with their wares, might buy and sell in peace". Granted. | Annals of Fulda 873 [mem] |
| c. 890 | Sciringes heal, Hæthum | Ohthere's account names no toll at either port. He pays none; he is a chieftain. | the Old English Orosius [mem] |
| c. 900 (by later telling) | Norway | **Landaurar**, the "land-pence": every man sailing from Norway to Iceland owes the king a fee; Harald Finehair set it at five ounces (?), says Ari. | Ari, *Íslendingabók* 1 [mem] |
| c. 1022 | Norway, Iceland | **Olaf Haraldsson's law for the Icelanders**: in Norway they have a *hauldr's* rights. They pay landaurar on landing: **six cloaks and six ells of homespun, or half a mark of silver**. They pay nothing if driven to Norway by storm on the way to Greenland or elsewhere. They may take wood and water in the king's forests. They must help defend the land if an army comes while they are there. A dead Icelander's goods are kept for his heir or partner. | sworn to again c. 1083 and written into *Grágás* [mem] |
| 989 | Dublin | Máel Sechnaill takes the town and lays on it an ounce of gold from every garth, each Christmas night for ever. | Annals of Tigernach 989 [mem] (?) |

### 8.1 The law of Birka: Bjarkeyjarréttr

[W: en Bjarkey laws.] "Bjarkey law" is the name in Norway and Sweden for the law of a trading place, as against the
law of the land. The name is commonly taken from Birka (Björkö, "birch island"), and some have tied its beginning
to the king's assembly there c. 832; that is debated. **No text from the game's years survives.** What survives:
fragments of the law of Nidaros (written 12th and 13th century), the Bergen town law accepted in 1276, and the
Swedish *Bjärköarätt* (late 13th or early 14th century, oldest book c. 1345, for Stockholm and Lödöse).

What the Norwegian text says of itself is the useful part [mem]: Bjarkey law holds **not only in the town but at
every fishing station, at the herring grounds and on trading voyages**. It is the law of men away from home: of a
ship's crew, a booth, a beach. Its sea chapter (*Farmannalög*) has the rules of a ship's company: who bails, who
may be put ashore, how freight is shared, when cargo may be thrown overboard.

For 793 to 1066 the game can say, safely:

1. A trading place has **its own peace**, from the day the market opens to the day it closes. A killing inside it
   costs more than outside.
2. The **king's man** (Hergeir at Birka; the *comes vici* at Sliesthorp, a Hovi is named c. 850) keeps it, and
   takes his lord's due in gifts.
3. Disputes between crews are settled **there and then by the men present**, not sent home.
4. A dead stranger's goods are kept for his **partner or heir**; if none comes, they are the king's (?).
5. Everybody weighs silver **on his own scales**; the weights are a shared standard (section 13).

**In play.** A `birk` rule: inside a Norse or Svea trading place in the season, no raider card and no feud killing;
the king's man expects a gift (the 2 % in kind that `economy_and_trade.md` 2.8 already guesses); a suit is heard the
same day by the crews in harbour, so the odds are the count of ships in port that are your friends.

---

## 9. East and south, where the player's goods come from

### 9.1 The Raffelstetten toll inquest, 903 to 906 (MGH Capit. II no. 253)

[W: en and de pages, for the date, the king, Margrave Aribo, the complaint of the Bavarian bishops, abbots and
counts about unjust tolls, the 41 named men, Rosdorf, Linz, the half-drachma and the three bushels, the Bohemian
forest, the wax of the Slavs, the Moravian market at a solidus a ship, and the Jews and other merchants paying the
toll of the old kings' time. The per-head rates are mem and were marked (?) in the project before.]

It is the only full toll tariff of the age, and it shows how a toll station worked.

- It is an **inquest**: the king orders the margrave to ask sworn local men what the toll was "in the days of Louis
  and Carloman". Law is what the old men remember.
- **Ships from the west**, once past the Passau forest, landing at Rosdorf or anywhere: half a drachma, that is one
  *scoti*. Going on down to Linz: three half-measures, that is three bushels, of salt from each ship. On slaves and
  other goods they pay nothing there, and may then land and trade as far as the Bohemian forest.
- **A Bavarian carrying salt for his own house** pays nothing, if the steersman swears to it (?).
- **Salt ships** going further pay three bushels again at each station (the Enns, Eparesburg, Mautern), and then
  have a free market.
- **Slavs from the Rugi (the Rus?) or the Bohemians** who come to trade: on a horse-load of wax two measures worth
  a *scoti* each; on a man's load one. On **a slave woman one *tremissa*, the same on a stallion; on a male slave
  one *saiga*, the same on a mare** (?).
- **The Moravians' market:** one solidus from each ship.
- **Jews and other merchants**, from wherever they come, pay the just toll on slaves and on other goods, as in the
  time of the earlier kings.
- Who slips past the station loses ship and cargo (?).

**In play.** A model for a river toll card: the toll man asks three things in order: *whose man are you*, *what do
you carry*, *is it for sale or for your own house*. The answer to the third is an oath.

### 9.2 Further east and south, in brief

| Year | What | Source |
|---|---|---|
| c. 850 | The Rus merchants pay a tenth to the emperor of the Romans on the Black Sea and a tenth to the Khazar king on the Volga; in Baghdad they say they are Christians and pay the poll-tax. | Ibn Khurradadhbih [mem] |
| 922 | The king of the Bulghars takes one slave in ten from every Rus ship. | Ibn Fadlan [mem] |
| 907, 911 | The Rus at Constantinople: lodged at St Mamas; entered in a list; in by one gate, unarmed, fifty at a time, with an imperial officer; bread, wine, meat, fish and fruit for six months, and sails, ropes and anchors for the way home; no toll. **If a ship is cast ashore, the other side helps her and brings her to safety**; a killer dies or pays; a thief pays threefold. | Primary Chronicle [mem]; chronicle `911-09-02-rus-treaty` |
| 944 | Tightened: the Rus bring a charter from their prince saying how many ships; no man buys silk above 50 gold pieces, and it is sealed by the officer; no wintering at the Dnieper's mouth. | Primary Chronicle [mem]; chronicle `944-igor-treaty` |
| 840 | The *Pactum Lotharii* with Venice: no Christian is bought or sold; fugitives are returned. | [mem] |
| c. 912 | The Book of the Eparch: the guilds of Constantinople; foreign merchants stay three months; forbidden silks. | [mem] |

---

## 10. Cnut at Rome, 1027

[W: en Cnut, for the journey, Conrad II's crowning at Easter, the pallium fees and the tolls.] Cnut was in Rome for
the emperor's crowning on Easter day, 26 March 1027. He wrote home to the English, and the letter is kept in Latin
by William of Malmesbury and John of Worcester [mem]. In it he says that he spoke with the emperor, the pope and
the princes who were there, about the needs of all his peoples, English and Danes: that they should have fairer law
and safer peace on the road to Rome, and not be hemmed in by so many barriers on the way nor worn down by unjust
tolls. The emperor agreed, and King Rudolf of Burgundy, who holds most of those barriers; and all the princes
confirmed by their edicts that his men, **merchants as well as those who travel to pray**, should go to Rome and
come back in firm peace and under just law, free of the barriers and the toll-takers. And the archbishops are no
longer to be squeezed for great sums when they come for the pallium.

**In play.** From 1027 an English or Danish house pays no toll at the Clusae and on the Rhône and Burgundy roads.
For a North Sea game the worth of it is at home: it is the first time a northern king bargains for his merchants by
name, and a player's house that is "Cnut's men" in 1027 has the best protector on the sea. Chronicle:
`1027-03-26-cnut-rome`.

---

## 11. Charlemagne and Offa, 790 and 796

**The embargo, c. 789 to 791.** Charlemagne asked for a daughter of Offa for his son Charles; Offa asked for
Charlemagne's daughter Bertha for his son Ecgfrith. The king was angry and ordered that nobody from Britain and the
English people should land on the coast of Gaul to trade. Abbot Gervold, the toll officer, talked him round.
Alcuin wrote in 790 that he might be sent to make peace and that ships were stopped on both sides. [W: en Offa of
Mercia; *Gesta abbatum Fontanellensium* and Alcuin's letter to Colcu mem]

**The letter, spring 796** (Alcuin's hand; MGH Epistolae IV no. 100) [W: en Offa of Mercia for the matters; the
detail mem]. Charles, king of the Franks and Lombards, to his dearest brother Offa:

1. **Pilgrims** to Rome go in peace, with what they need for the road.
2. **But** some have been found among them who go for gain, not for God: those pay the set tolls at the proper
   places.
3. **Merchants** have our protection and patronage in our realm, lawfully, by our order. If they are wronged
   anywhere, they may appeal to us or to our judges, and we will see justice done. **Likewise ours with you.**
4. **The black stones** you asked for: send a man to say what kind, and we will gladly have them found and help
   with the carrying. But as you say what length the stones should be, so our people ask about **the length of the
   cloaks**: have them made as they used to come to us.
5. Gifts: to Offa a belt, a Hunnish sword and two silk cloaks, from the Avar treasure.

**In play.** Already in the brief's picture of a good opening (the reeve turning Frankish ships away). Law gives it
three states for the Channel: *shut* (789 to 791: no English ship lands in Gaul, no Frankish ship in Offa's ports;
a Frisian is the emperor's man and is shut out too); *open by custom* (791 to 796); *open by letter* (from 796: a
wronged merchant can appeal to the other king, which is the act `audience` and the act `lawsuit` with a reason to
exist). Chronicle: `796-spring-offa-letter`.

---

## 12. Wreck

No king of the game's years wrote a general law of wreck that we have. What there is:

| Where, when | The rule | Source |
|---|---|---|
| The old Roman rule | A wrecked ship stays her owner's; to plunder her is theft. Known to churchmen, not kept on the coasts. | the Digest [mem] |
| Most northern coasts | **The custom of the strand:** what the sea throws up belongs to the lord of the shore; men thrown up with it can be seized and sold. If nobody lives, there is nobody to claim. | [W: de Strandrecht, for the custom and for its curbing beginning only in the 12th century] |
| Francia, 779 | Men may make agreements to help each other after shipwreck, without an oath. So the loss was common enough to plan for, and the law gave no other help. | Herstal c. 16 [mem] |
| Frisia, 802 | Nothing in the Lex Frisionum. | |
| The Sembi (Sambia) | "Most humane men, who go out to help those in peril on the sea or attacked by pirates." | Adam of Bremen IV.18 [mem], written c. 1075 |
| Heligoland (Fositesland) | A holy island: no raider dares take anything from it, however small; hermits live there. In Willibrord's day nobody touched its beasts or drew from its spring but in silence. | Adam of Bremen IV.3 [mem]; Alcuin, *Vita Willibrordi* 10 [mem] |
| Rus and Greeks, 911 | A ship cast ashore is helped with her cargo and brought to a safe place by the other side; goods sold from her are accounted for. | Primary Chronicle [mem] |
| England, 994 | A ship driven ashore: if the men get into a borough with what they carry, they and it have peace. | II Æthelred 2 [mem] |
| England, 11th century | "Wreck" appears as a right the king gives away with a shore: to Ramsey, to Christ Church at Sandwich (?). So by then it was the king's. | writs of Edward the Confessor (?) [mem] |
| Iceland, Norway | Drift rights (*reki*) belong to the owner of the shore and are bought and sold apart from the land: timber, whales. A ship's men keep their own if they live (?). | *Grágás* [mem], written 12th century |
| Ireland | The sea-judgments (*Muirbretha*): what is cast up belongs to the owner of the shore up to a set value and to the king above it; a stranded crew is fed by the people of that shore (?). | [mem] (?) |
| After the game | The Church forbids the plunder of wrecks at the Lateran council of 1179; the emperors and Hanse towns follow. | [W: de Strandrecht, "from the 12th century"] |

**In play.** The game says a wreck costs the boat and the cargo and the house goes on. Law says *where* matters:

- on a lord's shore with no friends: cargo gone, the men held for ransom (silver, or a commission owed);
- near a borough in England after 994, or on a coast where the player has a host or a charter: what the crew can
  carry is saved;
- on the Sembian coast: helped;
- on Heligoland: safe, and nothing may be taken off the island.

And the lawful wreck club of 779 is a "work" at Dorestad: pay in each year, and a lost boat is half made good.

---

## 13. Strangers and hosts

| Rule | Where, when | Source |
|---|---|---|
| A roof, a fire and water may be refused to no traveller. | Francia, 802 (and 789) | Capit. I no. 33 c. 27 [mem] |
| Who harms a stranger pays 160 solidi to the king's chest besides. | Bavaria, 8th century | Lex Baiuvariorum IV.30-31 [mem] |
| **Three nights make a host.** Feed a stranger three nights under your roof and you answer for him. | Kent, c. 680; England after | Hlothhere and Eadric 15 [mem]; [W: en Anglo-Saxon law, in outline] |
| Off the road, shout or blow a horn. | Wessex, Kent, c. 690 | Ine 20, Wihtred 28 [mem] |
| A trader shows his men to the reeve. | Wessex, c. 890 | Alfred 34 [mem] |
| A stranger with no kin: the king takes his price. | Wessex, c. 690 | Ine 23 [mem] |
| A stranger from overseas has no honour-price but his host's. | Ireland | [mem] (?) |
| A guest's needs: "Fire he needs who with frozen knees has come from the cold without; food and clothes must the farer have, the man from the mountains come." | the North | *Hávamál* 3, Bellows 1923; in `texts_793.json` |
| Foreign merchants lodge in a named quarter, are listed, and have a term. | Constantinople, 907 to 944 | Primary Chronicle [mem] |
| The emperor's Jewish and Christian merchants have houses at the palace of Aachen; the officers search them for hidden wrongdoers. | c. 820 | *Capitulare de disciplina palatii Aquisgranensis* [mem] (?) |
| A merchant has safe conduct by the king's letter. | Francia | Formulae; the letter of 796; the privilege of 828 [mem] |

**In play.** The act `lodge` makes a host after three nights (England) and gives the player one sure oath-helper
there; it also makes the player answerable for his own crew's doings ashore. The horn is a small true card for a
crewman sent inland for water in Wessex.

---

## 14. Slaves and their sale

Fuller in `economy/furs_amber_ivory_slaves.md`. The law, by date:

| Year | Where | The rule | Source |
|---|---|---|---|
| c. 690 | Wessex | Who sells his own countryman over the sea, slave or free, even a guilty one, pays his own wergild. | Ine 11 [mem] |
| 779 | Francia | Sold only before the bishop, the count or their men, or good witnesses. Not beyond the march. | Herstal c. 19 [mem]; outline [W] |
| 802 | Frisia | Who sells a slave to heathens pays his own wergild to the king (?). Who sells a free man abroad pays as for a killing. | Lex Fris. XVII.5, XXI [mem] |
| c. 803 to 806 | Francia | Not sold by night. | 4.2 above |
| c. 825 | Francia | Louis the Pious's letters for the Jewish merchants Donatus and Samuel, David and Joseph of Lyon, and Abraham of Saragossa: they may **buy foreign slaves and sell them within the empire**; nobody baptises their slaves to take them away; they pay no toll (?). Bishop Agobard of Lyon writes against it. | Formulae imperiales 30, 31, 52 [mem] |
| 840 | Venice | No Christian is bought or sold. | *Pactum Lotharii* [mem] |
| 845 | West Francia | The bishops at Meaux ask that heathen slaves led through the realm be bought by Christians and not taken on to the infidel. The king does not make it law. | Council of Meaux c. 76 (?) [mem] |
| c. 870 | Sliesthorp, Nordalbingia | Rimbert sells church plate, and gives his horse, to buy back Christians at Hedeby. Anskar shames the Nordalbingians who had caught escaped Christian captives and sold them on, some to heathens. | *Vita Rimberti* 17-18; *Vita Anskarii* 38 [mem] |
| 878 to 890 | England | Know your warrantor when you buy a slave. | Alfred and Guthrum 4 [W] |
| 903 to 906 | the Danube | A toll by the head. | 9.1 |
| 922 | Koblenz | To sell a Christian is to be guilty of his killing. | synod of Koblenz c. 7 [mem]; in the project already |
| 1008 | England | No Christian sold out of the land, least of all to heathens. | V Æthelred 2 [mem] |
| before 1066 | Lewes | Four pence toll on a man. | Domesday [mem] |
| c. 1060 to 1090 | Bristol | Bishop Wulfstan preaches the Bristol men out of selling English slaves to Ireland. | William of Malmesbury, *Vita Wulfstani* II.20 [mem] |

Prices in law: a slave half a pound (London, c. 935), a pound (994); an Irish *cumal* three cows.

The Church's own rule: freeing a slave is a good work, done at the altar or at the crossroads; a slave who works on
Sunday at his lord's order goes free (Ine 3) [mem].

**In play.** `trade_rules.json` has the law as "from 0". It should have dates and teeth: from 779 in Francia a
slave is sold only before an officer and never abroad; from 802 a Frisian (the player) who sells to heathens owes
his own wergild; from 864 nothing changes; from 1008 England. And the reverse trade is lawful and praised: buying
Christians back at Sliesthorp (the deed for it exists or should: see `needs`).

---

## 15. Weights, measures and false measure

| Year | Where | The rule | Source |
|---|---|---|---|
| 744 | Francia | Every town has a lawful market and lawful measures, by the plenty of the season. | Soissons c. 6, Capit. I no. 12 [mem] |
| 789 | Francia | Equal measures and just weights everywhere. | Admonitio generalis c. 74 [mem] |
| 793/4 | Francia | The new, larger public bushel and the heavier penny and pound (about 408 g: 240 pennies of 1.7 g). | Frankfurt c. 4-5 [mem] |
| 857 | London | The right to use the port's scale, weights and measures is a thing a king grants. | Sawyer 208 [mem] |
| 864 | West Francia | Measures true to the palace's; the cheat fined or flogged; pure metal only. | Pîtres c. 20, 23 [mem]; flogging and fines for mixed metal [W: fr] |
| c. 962 | England | One measure and one weight, London's and Winchester's. | III Edgar 8 [mem] |
| c. 1000 | London | Every weight stamped to the king's; fifteen ores to the pound. | IV Æthelred 9 [mem] |
| 1008, c. 1020 | England | False weights and wrongful measures put away. | V Æthelred 24, II Cnut 9 [mem] |
| before 1066 | Chester | False measure: four shillings. | Domesday [mem] |
| from c. 870 | the North and the Baltic | No king's measure. Folding scales in every merchant's purse, and two families of weights made to one standard from Dublin to the Volga: the iron ball with flat poles cased in bronze, and the small bronze cube with cut corners. The units: the *ertog* (about 8 g), the *øre* (about 24 to 26 g), the mark (about 200 g). Silver is nicked with a knife to see it is not plated. | finds at Birka, Hedeby, Kaupang, Torksey [mem]; `economy/silver_coin.md` |

**In play.** `weigh_silver`, `test_silver` and `sample` are acts already. Law adds: in a coin land a false measure
is the reeve's business and a fine; in a weight land it is the seller's word against yours, and the cure is to weigh
on your own scales. A set of true weights is a thing to own.

---

## 16. Markets and mints by grant

A market, a mint and a toll went together, and from the 830s kings gave all three away. Every grant made a new
place where the player could sell and where somebody new took the toll.

| Year | Where | The grant | Source |
|---|---|---|---|
| 744 | Francia | A lawful market in every town. | Soissons [mem] |
| 753 | Saint-Denis | The fair's tolls to the abbey. | DD Kar. 1 no. 6 [mem] |
| 833 | Corvey | Louis the Pious gives the abbey **a mint**, "because that region lacked a place of trade". The first mint given to a church east of the Rhine. | BM2 922 [mem] |
| 861 | Prüm | Lothar II: a market and a mint at Rommersheim (?). | [mem] (?) |
| 864 | West Francia | The counts list every market; the king keeps or shuts them. | Pîtres c. 19 [mem] |
| 888 | Bremen | Arnulf gives Archbishop Rimbert **market, mint and toll at Bremen**, as Hamburg had had. | DD Arn. 27 [mem] |
| 889 | London | A bishop's market court within a king's port. | Sawyer 346 [mem] |
| 896 | Tiel | The toll of Dorestad is at Tiel. | [W: en Tiel] |
| c. 928 | England | Mints only in ports; the list of moneyers by town. | II Athelstan 14 [W, mem] |
| 937, 965 | Magdeburg | Otto I gives St Maurice's the toll and then the market, mint and toll. | DD O I [mem] |
| 965 | Bremen | Otto I: a market for the archbishop; the merchants living there are under the king's protection and have the rights of merchants in the other royal towns. | DD O I 307 [mem] |
| 973 | England | Edgar's new coinage: one type, recalled and restruck every few years. | chronicle `973-edgar-coinage`; `silver_coin.md` |
| 975 | Magdeburg | Its merchants toll-free in the whole realm but for four places. | DD O II 112 [mem] |
| c. 825; c. 975; 995; 997 | Sliesthorp; Denmark; Sigtuna; Dublin | The North's own coins: the Hedeby pennies; Harald Bluetooth's; Olof Skötkonung's; Sihtric's. A king's coin is a claim to a king's toll. | chronicle `995-sigtuna-coins`, `997-dublin-coins`; `silver_coin.md` |

---

## 17. Compared with `trade_rules.json`

What the file has, against the law above. None of this was changed: the file is a shared table.

### 17.1 Tolls

| The file says | The sources say | Proposed |
|---|---|---|
| `frankish.toll` 0.10 on every sale in twelve regions | The tenth is named at **three places**: Quentovic, Dorestad, the Clusae (828, 831). Rouen, Amiens, Maastricht had tolls (779) of unknown rate. Elsewhere: fixed small sums by the boat and the load. | Keep 0.10 at `dorestad` and `quentovic`. Elsewhere in the zone a flat landing sum (1 to 4 d a call (?)) at toll stations and nothing at villages. A per-port `toll` override is all it needs. |
| The Frankish toll never changes hands | Dorestad's toll is Rorik's from 850, gone after 863, at Tiel by 896, Dirk's on the Merwede in 1018. | A `toll_holder` by years on the port, so the toll man's people and tongue change. `economy_and_trade.md` 2.5 already plans `danish_gift`. |
| `english.toll` 0.05 | No rate before c. 1000. Then flat: a halfpenny, a penny, four pence by the vessel, and dues in kind from the Emperor's men. | Before 1000 keep a small share but mark it a guess; from 1000 at `lundenwic` a flat 1 p for the player's boat, 4 p for a keel or hulk. A `by_ship` toll kind. |
| `weight.toll` 0.0, "to come" | Reric paid Godfred taxes (808). The king's man took gifts (Birka, Sliesthorp). Landaurar in Norway: half a mark of silver or six cloaks a ship (c. 1022; older by Ari). Dublin: an ounce of gold a house from 989. | A `gift` toll kind: a fixed gift in kind per call (2 % was the design's guess), refused at a cost in standing. For `norway_*` from c. 900 a flat landing fee of half a mark (about 100 g, 60 d) on ships bound to or from Iceland (?). |
| `dirham.toll` 0.10 | Right: the Khazar tenth and the Bulghar tenth (one slave in ten). | No change. Source it: Ibn Khurradadhbih, Ibn Fadlan. |
| `byzantine.toll` 0.10 | Right for the *kommerkion*; but the Rus by treaty paid none (907 to 944). | A treaty exemption by people. |
| No exemptions | Charters of toll freedom are the commonest royal document of the age. | `Chest.TollFreeAt` exists (a riddle's reward). Add charter kinds: an abbey's letter (that cargo, that realm); the privilege of 828 (all Francia but the three tenths; from 828); the Emperor's men at London (from c. 1000); Cnut's men (from 1027). |
| Toll on every sale | Toll on goods **for sale**, not on one's own stores, a pilgrim's needs, or goods for the palace or the army (805, 796, 755). | A choice at the toll card: declare "my own goods" (an oath: a deed if it is false). The verb `toll none` already exists. |
| No tolls by the head | Slaves and horses by the head at Raffelstetten; a man four pence at Lewes. | A `per_unit_d` on goods of kind captive and horse (already asked for in `furs_amber_ivory_slaves.md` 6). |

### 17.2 Laws

| The file's law | Check | Proposed |
|---|---|---|
| `arms`, frankish, from 779, "no export" (Herstal; Thionville 805) | Right. Herstal names mail coats only; 805 adds weapons, and only toward the Slavs and Avars, at nine named posts. | Split: `byrnie` no export from 779; `arms` no export east from 805. |
| `arms`, frankish, from 864, "death"; `horses`, frankish, from 864 | Right in substance. But **Pîtres is West Frankish only**: it does not bind Dorestad, Tiel, Cologne, the Rhine or Saxony. And it bans *giving to the Northmen*, for sale or ransom, not export as such. | Limit to the regions `neustria`, `loire`, `aquitaine` and `flanders` (Charles the Bald's kingdom), or give laws a `regions` list beside `zones`. |
| `captives`, frankish and english, from 0 | The principle is old, but the laws have dates: 779 (Francia: before an officer, not over the march), 802 (Frisia), 1008 (England; Ine 11 from c. 690 for Wessex). | Date them. Add the witness rule. |
| `sacred`, from 0 | Right. | Add the heathen side for Frisia: breaking a shrine is death on the tide line (Lex Fris. Add. XI); and Heligoland's rule. |
| nothing on horses in England | No horse over the sea except as a gift, from c. 928. | `horses`, english, from 928, "no export". |
| nothing on grain | No food sold out of the empire in a famine (805). Grain price ceilings (794, 806). | `grain`, frankish, "no export" while a famine shock holds; from 805. |
| nothing on night sales | No trade by night in slaves, horses, beasts, gold and silver, gems (c. 803 to 806). | A rule on the market panel: at night only food and fodder are sold in Francia. |
| nothing on where one may buy | England: in a port, before the reeve; nothing over 20 d outside a port from c. 928; sworn witnesses from c. 962. | `witness`, english: a beach or village sale over 20 d is unlawful from 928 (the goods can be claimed). |
| nothing on Sunday | No Sunday markets except old ones (809); no Sunday work in Frisia (802) or England. | Markets shut on Sunday in the frankish and english zones, except fairs. |
| nothing on measures | 744, 789, 794, 864, 962, 1008. | A `measure` law for the acts `weigh_silver` and `sample`. |
| nothing on coin | In `silver_coin.md` 8 and its proposals 18-19. | See there. |
| `start.debt` 480, due in 365 days | Usury is forbidden (789, 806), so a fixed sum with no interest is right. A Frisian free man's wergild is in the same range as the debt. | Say in the creditor's words that the count takes no interest "because the king forbids it" and takes his gain in the first refusal of the cargo. |

### 17.3 Small things

- The note on the Frankish zone says "the king's toll (the tenth) is taken at Dorestad, Quentovic and the river
  tolls". Better: "the tenth at Dorestad and Quentovic; old fixed tolls on the rivers".
- The 864 arms note quotes "sells arms or horses"; the text is "gives, for any reason or any ransom".
- `LawDef.Holds` keys on zone and year only. Pîtres, Alfred and Guthrum, Raffelstetten and the Bjarkey peace are
  all narrower than a money zone. Laws need `regions` or `ports`.

---

## 18. What this means in play: a short list for the card writers

These are proposals in the engine's own verbs. Nothing here was added to a shared table.

| Card or act | Fact it rests on | Verbs |
|---|---|---|
| The toll man asks: for sale, or your own? | 805 c. 13 | `toll none` on an oath; `deed` if it was false |
| A new toll at a rope across the Lek | 779 c. 18; 805 | refuse (doubtful), pay (`silver`), or name the capitulary (`learn` first) |
| The abbey's letter | 779 Saint-Germain; 831 Strasbourg | `commission`, then `toll none` for that cargo |
| To the palace in mid May | the privilege of 828 | `flag`, `toll eased`, a yearly `schedule` |
| The searcher at Bardowick | 805 c. 7 | `goods` lost if arms aboard; half to the finder |
| A ransom asked in horses | Pîtres c. 25 | a choice with no lawful way out |
| The lots on the altar | Lex Fris. XIV | the act `oath`; odds by helpers |
| The shrine-breaker on the tide line | Lex Fris. Add. XI | `news`, `give_text` |
| A Frisian kills a man at York | *Vita Liudgeri* | every Frisian leaves: `flag`, the port shut for a season |
| Three nights under a roof | Hlothhere and Eadric 15 | `npc` (a host), the act `lodge` |
| Blow the horn | Ine 20 | a crew card ashore |
| Bought on the beach, over 20 pence | II Athelstan 12 | the goods claimed; `oath` or lose them |
| The hostage at the Lea | Alfred and Guthrum 5 | `silver` as a bond, back on return |
| Ten pounds of pepper by Christmas | IV Æthelred 2 | `commission` |
| The reeve sees the martens first | Chester | sell to him at his price, or hide them (`deed`) |
| The king cannot say yes alone | *Vita Anskarii* 26-27 | wait for the assembly: days |
| Six cloaks or half a mark | the Icelanders' law | `goods` or `silver` on landing in Norway |
| Driven ashore under a borough | II Æthelred 2 | cargo saved if the walls are reached |
| The Sembi come out in boats | Adam IV.18 | help, not wreckers |
| A wreck club without an oath | 779 c. 16 | a `work` at Dorestad |

**Needs** (not added; for the spine's owner to decide):

1. Laws by `regions` or `ports`, not only by money zone.
2. Toll kinds beside the share: `by_ship`, `gift`, `per_unit_d`, and a per-port override.
3. A `wergild` table by people and rank.
4. A charter as a thing in the chest with a scope (realm, port, cargo) and years.
5. Deeds, if they do not exist: `sold_to_heathens`, `ransomed_christians`, `false_oath`, `kept_oath`,
   `passed_ordeal`.
6. A count of oath-helpers in a port, for the acts `oath` and `lawsuit`.

---

## 19. Sources

### Read today (6 October 2026)

| Mark | Page | Used for |
|---|---|---|
| [W: en Raffelstetten] | en.wikipedia.org/wiki/Raffelstetten_customs_regulations | date, Aribo, the 41, Rosdorf, Linz, rates in salt, MGH edition |
| [W: de Raffelstetten] | de.wikipedia.org/wiki/Raffelstettener_Zollordnung | the stations, the Bavarians' freedom, the Slavs' wax, the Moravian market, the Jews |
| [W: en Lex Frisionum], [W: de Lex Frisionum] | en. and de.wikipedia.org/wiki/Lex_Frisionum | date, lands, ranks, kettle ordeal, Herold 1557, structure, women's wergild |
| [W: en Treaty of Alfred and Guthrum] | en.wikipedia.org/wiki/Treaty_of_Alfred_and_Guthrum | all five clauses |
| [W: en Edict of Pistres], [W: fr Édit de Pîtres] | en.wikipedia.org/wiki/Edict_of_Pistres, fr.wikipedia.org/wiki/Édit_de_Pîtres | date, penny, 12 : 1, arms and horses, castles, bridges, penalties for mixed metal |
| [W: en Quentovic] | en.wikipedia.org/wiki/Quentovic | the 10 %, Gervold, Saint-Germain 779, the raid of 842 |
| [W: nl Dorestad], [W: en Dorestad] | nl. and en.wikipedia.org/wiki/Dorestad | the ripaticum of 777; Rorik |
| [W: en Capitulary of Herstal] | en.wikipedia.org/wiki/Capitulary_of_Herstal | date, 23 chapters, tolls, slave sales, arms |
| [W: de Diedenhofener Kapitular] | de.wikipedia.org/wiki/Diedenhofener_Kapitular | date, the nine places, the arms ban, Boretius 1883 |
| [W: Fordham] | sourcebooks.fordham.edu/source/560-975dooms.asp | the quoted dooms (Thorpe's wording), the ordeal weights, the northern and Mercian wergilds |
| [W: Early English Laws] | earlyenglishlaws.ac.uk/laws/texts/iv-atr/ | IV Æthelred in outline |
| [W: en Bjarkey laws] | en.wikipedia.org/wiki/Bjarkey_laws | all of 8.1 but the "fishing station" line |
| [W: en Offa of Mercia] | en.wikipedia.org/wiki/Offa_of_Mercia | the quarrel, the letter's matters |
| [W: en Cnut] | en.wikipedia.org/wiki/Cnut | Rome 1027 |
| [W: en Weregild] | en.wikipedia.org/wiki/Weregild | the Salic 200, the English 200 and 1,200, the Welsh scale |
| [W: de Strandrecht] | de.wikipedia.org/wiki/Strandrecht | the custom; its curbing from the 12th century |
| [W: en Tiel] | en.wikipedia.org/wiki/Tiel | the toll right of 896 |
| [W: en Anglo-Saxon law], [W: en Trial by ordeal] | en.wikipedia.org | outline only |

Pages that failed today: de *Praeceptum negotiatorum* (404), fr *Capitulaire de Thionville* (404), en *Cnut's
letter of 1027* (404), the Avalon text of Alfred and Guthrum (404), la.wikisource *Lex Frisionum* (404), keesn.nl
(the Lex Frisionum in English: certificate error), the Fordham London page (500). All web searches were refused
(the session's budget was spent before this task began).

### Editions, cited from memory

- *Capitularia regum Francorum* I (Boretius 1883) and II (Boretius and Krause 1897), Monumenta Germaniae Historica:
  nos. 12, 13, 20, 22, 27, 28, 33, 40, 43-44, 46, 52, 55, 61-62, 74, 138, 192, 253, 273.
- *Formulae imperiales* 30, 31, 37, 52, in MGH Formulae (Zeumer 1886).
- *Lex Frisionum*, ed. K. A. and A. Eckhardt, MGH Fontes iuris 12 (1982); first printed by Herold, Basel 1557.
- *Lex Ribuaria*, MGH LL nat. Germ. III 2 (1954); *Lex Saxonum* and the *Ewa ad Amorem*, MGH Fontes iuris 4, 6.
- F. Liebermann, *Die Gesetze der Angelsachsen* (1903-16); B. Thorpe, *Ancient Laws and Institutes of England*
  (1840), the public-domain English.
- Sawyer, *Anglo-Saxon Charters* (the S numbers).
- Alcuin's letters, MGH Epistolae IV (no. 100: Charles to Offa). An old English version is in Giles's translation
  of William of Malmesbury (1847) (?).
- *Gesta abbatum Fontanellensium*; *Miracula S. Wandregisili*; Altfrid, *Vita Liudgeri*; Alcuin, *Vita
  Willibrordi*; Rimbert, *Vita Anskarii* (C. H. Robinson's translation, 1921, is public domain) and the *Vita
  Rimberti*.
- The Royal Frankish Annals; the Annals of St-Bertin; the Annals of Fulda; the Anglo-Saxon Chronicle (Giles 1847,
  Ingram 1823); Widukind; Thietmar; Alpert of Metz; Adam of Bremen.
- Ari, *Íslendingabók*; *Grágás*; the Gulathing law; the Nidaros *Bjarkeyjarréttr*.
- The Russian Primary Chronicle; Ibn Khurradadhbih; Ibn Fadlan.
- Domesday Book: Cheshire, Sussex (Lewes), Kent (Dover).

### To check before any of it is shown as fact (the worst first)

1. The whole text of the privilege of 828, above all the army-fine clause and "each year or every second year".
2. IV Æthelred clause by clause: the plank, the three cloth days, the six shillings, the five pounds, the kegs.
3. The Lex Frisionum's figures: the three lands' wergilds, the weights in Title XV, what a solidus is in pennies,
   XVII.5 (sale to heathens) and the Sunday fines.
4. Raffelstetten's rates by the head, and the forfeit for slipping past.
5. The Utrecht tenth: which charters, which years.
6. The English toll charters of 733 to 765: Sawyer numbers and how many ships.
7. Pîtres: the clause numbers; sixty strokes; chapter 37.
8. The capitulary on trading by night: its number and year.
9. Ine 23 and the king as a stranger's kin; the Irish *cú glas*; the Irish sea-judgments.
10. Cnut's Sandwich charter and the wreck writs.
11. The landaurar under Harald Finehair; the Dublin tax of 989.
12. Zwentibold's charter of 896: what exactly it says of Dorestad's toll.
