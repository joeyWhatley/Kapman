<!-- Research pass, 4 Oct 2026, late evening. Builds on economy_and_trade.md (446 lines) and goods_793.json (140 goods, 40 regions). Inline web searches plus my own knowledge; (guess) and (check) mark what was not verified today. -->

# Kapman: the trading year, region by region, and how the player learns it without numbers

**The ask (4 Oct, evening):** "research the most accurate season by season market changing trade economy of all the
regions ... all things traded, all knowledge had by the people of the time, the needs and desires, the real logistics
and economics of the time", and "incorporated in a smart intuitive way that's not just numbers, maybe the item has an
attached note about the most recent information received on that product, a hint perhaps".

**Where we already are.** More is built than you may remember:

- `goods_793.json`: 140 goods, each with its period name, unit and weight, where it is made and wanted across 40 trade
  regions, the months it is cheap and dear, how long it keeps, the law on it, and its sources (Frankfurt 794, the
  Dorestad excavations, Ohthere, Ibn Fadlan, Barrett on stockfish).
- `economy_and_trade.md`: the Frankfurt price caps, the tolls (Dorestad's riverbank tax of 777, Quentovic's tenth,
  London's tolls c. 1000, Raffelstetten), the routes with real passage times, the ice table, the chronicle's shocks
  year by year, the silver flows (dirhams, tributes, Rammelsberg), and a price model with seasons by good.
- `PriceNotes.cs` and `knowing_793.json`: the ledger as rune sticks in the chest: every price seen or told, with its
  day, who said it, how many mouths it passed through, and a sureness that fades (seen 1.0, told 0.85, talk 0.7,
  rumour 0.45).
- `fairs_793.json`, `commissions_793.json`, the AI traders with beats, news carried by ships.

So this report does not restart the economy. It fills four gaps and designs the thing you asked for last:

1. **Seasons by place, not only by good**: a month-by-month trading year for each region (section 2).
2. **Needs and desires by people**: what each coast lacked and craved (section 3).
3. **What the people knew**: the knowledge a merchant carried in his head (section 4), and the logistics (section 5).
4. **The note on the item** (section 6): the hint, in words, from what the merchant has actually heard.

---

## 1. The shape of the year everyone lived by

The whole North Sea world ran on one farming year and one church year, with the sea closed in the middle of winter.

| Month | The land | The church and the feasts | The sea |
|---|---|---|---|
| January | threshing; stock on hay; the hungry stores begin to show | Epiphany 6th; Candlemas 2 Feb needs **wax** (every candle in the realm) | closed; Baltic and Mälaren frozen; furs trapped in the north |
| February | ploughing starts in the south when the ground thaws | Lent begins (Feb-Mar): **fish** wanted everywhere for 40 days, no meat; the Saint-Mathias fair at Saint-Denis 24 Feb; the Svear's Disting at Uppsala (a thing and a market, probably old: check) | closed; ships hauled up, planks replaced, hulls tarred |
| March | spring sowing of oats and barley; lambing | Lent; Lady Day 25 Mar | the first coasting; Rhine traffic resumes when the ice and floods allow |
| April | the hungry gap begins: last year's grain running out | Easter (Mar 22-Apr 25): the great feast, **wine** for every church, new clothes, the end of the fast | **the season opens**: the Norse launch after Easter; the Frisians cross to England on the spring easterlies |
| May | cattle to the summer pastures; dairying starts | Pentecost, Rogation processions | the Frankish and Danish fleets out; the raiding season opens (c. 1 May) |
| June | hay; **sheep shearing**: the new wool; eider down gathered in the north; cheese and butter at their best | St John 24 June (midsummer fires) | long days, light winds, fog on the North Sea; the summer markets at Ribe, Kaupang, Birka in full swing; walrus hunts in the White Sea |
| July | the hungry gap at its worst: grain dearest; flax pulled | | the best sailing; the Icelandic Althing (late June) |
| August | **harvest**: Lammas 1 Aug (the first loaf); grain prices break; honey taken | Assumption 15 Aug | the herring shoals off the English east coast; captives come to market after the summer's raids |
| September | harvest in the north; the **vintage** starts in the Rhineland; nuts; pigs to the woods | Michaelmas 29 Sept: rents and reckonings | autumn gales begin; the last Baltic crossings; stockfish and furs come south from Hålogaland |
| October | ploughing and winter sowing; the new wine racked; **the Saint-Denis fair** 9 Oct to 29 Nov: Frisians and Saxons sell wool and cloth, buy wine and honey | All Saints 1 Nov | the Norse raiders home by the end of the month; the Baltic traders home; late gales |
| November | **slaughter month** (Bede's Blotmonath; Martinmas 11 Nov): salt pork and beef laid down; **salt** at its dearest; hides and tallow plentiful; the new wine comes down the Rhine | Advent fast: fish again; St Andrew 30 Nov | the sea closes in practice: a few Rhine boats and the Channel short crossings; the Baltic ice begins in the east |
| December | spinning and weaving by firelight (the winter's cloth); amber gathered after the storms | Christmas: twelve days of feasting; Yule in the north: ale, pork, the midwinter blót | closed |

The rule the model needs: **grain is cheapest in September and dearest in June-July; wine is dearest at Easter and
cheapest in November-December; salt peaks in November; fish peaks in Lent and Advent; wool is new in June and cloth is
new in March; furs and stockfish arrive in late summer and are freshest then; captives are cheapest in September;
wax is dearest in January.** The goods table already carries most of these by good; the per-region table below
puts them by place.

## 2. The trading year by region

For each region: what it put on the quay, month by month; what it was short of; the fixed dates; when its water
closed. Items from the sources are plain; my inferences are marked (guess).

### 2.1 The Rhine delta: Dorestad, Utrecht (the player's home)

- **Offers:** Rhenish wine (from south of Mainz) all year from stock, freshest and cheapest **Nov-Dec** when the new
  wine comes down; Badorf and Tating pottery, Mayen querns (roughouts finished on the spot), Rhenish glass, bar iron
  and Rhenish metalwork **all year**, with the stock built up over winter and shipped from **March**; Frankish swords
  under the counter (banned 779, 805, death 864).
- **Wants:** English cloth and lead, Frisian cloth, furs and walrus ivory from the north, amber, hides, honey and wax
  for Utrecht's churches, salt from Zeeland, fish in Lent, grain in the hungry months (the delta fed itself only
  partly: guess).
- **Dates:** the riverbank toll of 777 all year; the mint; the Frankish assembly in **May** when the king's men buy
  horses and gifts (guess); the Rhine's floods in **Feb-Mar** stop the upstream traffic (guess from the river's
  regime).
- **Closed:** only in the chronicle's hard winters (810/11, 813/14, 821/22, 859/60).

### 2.2 Frisia: the terp coast from West Frisia to the Ems

- **Offers:** **Frisian cloth** (pallia fresonica), the famous export, new in **March-April** after the winter's
  weaving; cattle and oxen driven to market in **Oct-Nov**; hides, butter and cheese through the **summer**;
  peat-salt (burning salt-soaked peat) made in **summer**, sold **Sept-Nov**; wool in **June**; fish and eels in
  the **autumn** runs; the Frisians were above all **carriers**: the ships and the men.
- **Wants:** everything that does not grow on a terp: **timber** (no trees), **grain** (the clay pastures grew
  little), wine, querns, pottery, iron.
- **Dates:** the spring crossing to England on the easterlies **(Mar-Apr)** and the return on the westerlies
  **(Aug-Oct)**: a Frisian ship made two round trips a season (guess).

### 2.3 Zeeland and Flanders: Walcheren, Ghent, the Scheldt

- **Offers:** sea salt from the Zeeland pans in **Sept-Nov**; Flemish wool and the first Flemish cloth; eels; sheep.
- **Wants:** wine, grain in bad years, the Rhine goods.
- **Dates:** Walcheren in Danish hands from 841 (Harald's grant): the toll a gift in kind.

### 2.4 The Rhineland and the Meuse: Cologne, Mainz, Maastricht, Huy

- **Offers:** the **vintage** in **Sept-Oct**, new wine racked by **Martinmas**; querns from Mayen and pottery from
  the Vorgebirge **all year**; glass; brass from the Meuse (Huy, Dinant) and swords from the Rhine smiths; books and
  parchment from the monasteries; relics for the new churches of the north; pepper and silk that came up from the
  Alps and the Rhône in **summer**.
- **Wants:** furs, slaves (sold on to Verdun and Spain), amber, walrus ivory, wax and honey, English cloth and lead,
  Frisian cloth, horses.
- **Dates:** the Frankfurt price caps of 794; the Thionville (805) and Nijmegen (806) famine rules; the Rhine frozen
  for thirty days in 821/22.

### 2.5 Neustria: Quentovic, Rouen, Paris, Saint-Denis

- **Offers:** Seine and Paris wine; grain (Neustria was the breadbasket); **the Saint-Denis fair, 9 Oct to 29 Nov**,
  where the abbey sold its grain and surplus wine and Frisians and Saxons sold wool and cloth and bought wine and
  honey; a second fair at **Saint-Mathias, 24 Feb**; Quentovic's mint and its tenth.
- **Wants:** English wool and cloth; furs; slaves; tin and lead; honey.
- **Closed:** the Channel short crossing ran most of the year; the Seine froze in 821/22.

### 2.6 The Loire and Aquitaine: Nantes, Noirmoutier, Bordeaux

- **Offers:** **Noirmoutier salt** made in **summer**, shipped **Aug-Oct** (the Noirmoutier monks fled the Norse
  every summer from 819 and left in 836); Loire and Bordeaux wine after the **Sept-Oct** vintage; the Irish wine
  trade from here.
- **Wants:** hides, wool, slaves, tin from Cornwall.

### 2.7 Southern England: Lundenwic, Hamwic, Kent

- **Offers:** **wool** in **June**, cloth and the long English cloaks over winter, sold **spring**; **lead** from the
  Mendips and **tin** from Cornwall by Hamwic; hides; hounds and horses; silver pennies (the best coin in the north
  after Offa); captives in the early period.
- **Wants:** Rhenish wine for every minster and hall (dearest at **Easter** and **Christmas**), Mayen querns (the
  "black stones" of 796), Badorf pottery, Rhenish glass, Frankish swords, silk, pepper and incense for the church,
  oil for the lamps.
- **Dates:** London's tolls by the ship (c. 1000); the Danegeld years drain the silver (991, 994, 1002, 1007, 1012,
  1018).

### 2.8 East Anglia and Northumbria: Ipswich, York, Whitby, Lindisfarne

- **Offers:** Ipswich ware pottery **all year**; the **herring** off the east coast **Aug-Oct**; the Fenland **eels**
  in **Sept-Nov**; Whitby **jet**; Northumbrian wool and hides; York's Frisian quarter sold the Rhine goods inland.
- **Wants:** wine, querns, glass, Rhenish pottery; after 866 the Danish York wants the Norse goods: soapstone,
  whetstones, furs, and ties to Dublin.

### 2.9 Ireland and the Isles: Dublin (from 841), Iona, the Orkneys

- **Offers:** hides and wool; **captives** in **autumn** after the raiding season (Dublin "where captives were traded
  northwards"; 871: 200 ships of captives); Orkney and Shetland: wool, dried fish, eider down.
- **Wants:** wine from the Loire and Bordeaux (the Irish church's wine came by sea); salt; iron; Norse goods.

### 2.10 Jutland and Schleswig: Ribe, Hedeby

- **Offers:** Ribe's **summer market** (seasonal in the 8th century: tents and workshops, no houses; permanent
  later): glass beads made from imported Frankish tesserae, antler combs, amber worked from the Baltic raw, bronze
  brooches; Hedeby (permanent, year-round, the hub between the Baltic and the west): everything from both seas, the
  **portage** at Hollingstedt (under 15 km) saving the Skagen passage; Jutland horses and cattle in **autumn**;
  rye.
- **Wants:** Rhenish wine, pottery and glass; querns; Frankish swords; silver; salt for the autumn fish.
- **Dates:** the market open **May-Sept**; Hedeby's festival with feasting and sacrifice (Ibrahim ibn Yaqub, 960s),
  probably **midwinter** or **autumn** (guess); the king's man takes his due in gifts.

### 2.11 Vestfold and western Norway: Kaupang, Avaldsnes

- **Offers:** **soapstone** vessels and **Eidsborg whetstones** (the Klåstad ship, c. 990, sank with a cargo of
  whetstones), **bar iron** smelted from bog ore in **winter**, antler and combs, furs from the inland, falcons,
  **tar** and timber, dried fish; Kaupang's market **summer** (it may have stayed seasonal).
- **Wants:** grain in every bad year, fine cloth, wine, glass, Frankish swords and pottery, salt, silver.
- **Dates:** the Gulathing in **summer**; ships launched **after Easter**, hauled up in **October**.

### 2.12 Hålogaland and the far north (Ohthere's country)

- **Offers:** the **Sámi tribute**, collected over **winter** and shipped **spring-summer**: marten, reindeer, bear
  and otter skins, feathers by the amber-measure, ship ropes of whale and seal hide 60 ells long; **walrus ivory and
  hide rope** from the White Sea hunts in **summer**; whale; from about 1000, **stockfish**: cod caught **Jan-Apr**
  off Lofoten, dried on racks **Feb-May**, shipped south from **June**.
- **Wants:** grain, cloth, iron, silver, everything.
- **Dates:** "one could not sail to Skiringssal in one month if one camped at night" (Ohthere): the northern cargo
  reaches Kaupang in **July-Aug** and Hedeby in **Aug-Sept**.

### 2.13 The Svear and Gotland: Birka, Gotland

- **Offers:** **furs** from the Sámi, Finns and the Russian forests (squirrel above all, marten, beaver, fox; prime
  pelts are **winter** pelts, trapped Nov-Mar and sold at the **summer** market; Birka also had a **winter market**
  reached by sled and skate: this is the one place the game can trade in winter); eastern silver (dirhams), beads,
  silk pieces, honey and wax from the east; iron; Gotland as the Baltic's bank.
- **Wants:** Frankish pottery, glass, wine, cloth; swords; salt.
- **Closed:** Mälaren **Dec-Mar** by ice (guess at the months); the Baltic crossing **Nov-Apr**.

### 2.14 The eastern Baltic: Truso, Grobina, the Finns

- **Offers:** **amber**, gathered on the beaches after the **autumn and winter storms** and sold in **spring**;
  furs, wax, honey; captives.
- **Wants:** salt (none made here), cloth, swords, silver, iron.
- **Dates:** Wulfstan's seven days and nights from Hedeby to Truso; the Baltic ice **Nov-Apr** (the Gulf of Finland
  from late November to late April).

### 2.15 Ladoga and the Rus rivers: Aldeigjuborg, Novgorod, Kiev

- **Offers:** the eastern **furs** (sable, black fox, beaver, squirrel), **wax and honey**, **captives**, sold for
  **dirhams**, silk, beads and glass; the Rus merchants pray to their idols for "a merchant with many dinars and
  dirhams who will buy from me" (Ibn Fadlan, 922).
- **Wants:** swords (the Rus sold Frankish swords on to the Caliphate), silver, amber, Frankish goods.
- **Dates:** the spring floods open the rivers in **May**; the Dnieper convoys leave Kiev in **June** for
  Constantinople (Constantine VII); Ladoga's shores freeze in **December**, the ice breaks in **April-May**; the
  dirham stream runs **800-1015** with its peak **900-950**.

### 2.16 The far ends: Byzantium, the Caliphate, al-Andalus, Rome

Silk (capped at 50 gold coins a man in 944), pepper, incense, oil, relics, dinars; they want furs, slaves, amber,
walrus ivory, swords, falcons. In the game these reach the north through the hubs marked 0.5 in the goods table,
with the news months old.

## 3. Needs and desires: what each people craved

Not every want is hunger. The table the market needs is three deep:

| Layer | Who | What they wanted, and why |
|---|---|---|
| **Survival** | every farmer, every winter | salt (no slaughter without it), iron (every tool and nail), querns (every loaf), grain in a bad year, fish in Lent, cloth against the cold |
| **The hall** | kings, counts, jarls, their men | wine (not ale: wine was the drink of lords and of the Mass), Frankish swords and byrnies, glass drinking vessels, Tating pitchers, fine cloaks, furs for trim, falcons and hounds, horses, silver by weight, silk strips sewn on a tunic, walrus and elephant ivory, amber, gold brooches |
| **The church** | every minster, abbey and new mission (Utrecht, Echternach, Werden, Hamburg, Birka's chapel) | wax (candles), wine and oil (the Mass and the lamps), incense, pepper and spices for the fast days, parchment and pigments, bells and bronze, relics, books, silk vestments, fish in Lent |

Cultures differed in what they reached for:

- **Franks and English** wanted the north's raw goods (furs, ivory, amber, wax, slaves, hides) and the south's
  luxuries, and paid in made things and coin.
- **Frisians** wanted what a terp cannot give (timber, grain, wine) and lived by carrying.
- **Norse and Danes** wanted grain, cloth, wine, glass and swords, and above all **silver by weight**: hacksilver, arm
  rings, dirhams; a Frankish coin was silver to them, not money.
- **Svear and Rus** wanted the east's silver and the west's swords, and sold furs, wax and people.
- **The Church everywhere** wanted the same three things in every port: wax, wine, oil.
- **Everyone** wanted salt in November and fish in March.

## 4. What the people knew

A merchant of 800 carried his knowledge in his head, in a few marks on a stick, and in other men's talk. The game's
sureness system already models this; these are the facts it should rest on.

- **Time by feasts and the moon.** Nobody said "15 October"; they said "the week after Saint Denis" or "the new moon
  before Martinmas". The year ran Easter to Easter or from Christmas. Market days were feast days.
- **Tides by the moon.** Bede (725) had written down that the tide follows the moon and that high water comes at a
  different hour at each port; a skipper knew his own ports' "establishment" by heart and the springs at full and new
  moon. The Rhine mouths and the Thames estuary are tide-bound; Dorestad itself is not.
- **The way by marks, not charts.** No charts existed. Sailing directions were spoken: Ohthere's "Denmark to port and
  open sea to starboard for three days"; the Landnámabók's "from Hernar west, so far north of Shetland that you can
  just see it in clear weather, so far south of the Faroes that the sea is right in between the distant mountains".
  Birds told the distance to land; whales marked the banks; the colour and smell of the water told the river mouths.
- **The lead and line.** Lead sounding weights are found in Viking-age harbours and the sounding pole is on the Bayeux
  Tapestry. The North Sea is shallow: a skipper knew the bottom by the sand on the tallow.
- **Weather lore.** A swell before the wind; the sky at dusk; the halo round the moon; gulls inland. The season's
  rules were hard: no open-sea passage from Martinmas to Easter unless need or greed drove it.
- **Weights and measures.** The Carolingian pound (about 408 g, 240 pennies of 1.7 g) from 793/4; the modius, newly
  fixed in 794; the ell for cloth and rope; the north weighed silver in the øre (about 26.6 g) and the mark (8 øre),
  on folding scales every trader carried, with a touchstone; the English penny was trusted for its silver.
- **Credit and trust.** Partnerships (the Norse félag: men pooling a cargo), pledges and sureties, hostages for
  peace (the Alfred-Guthrum treaty), the king's protection bought with a gift, the merchants of Tiel who "swear and
  drink together" (1020). A factor left in a port, a letter by the next ship.
- **Languages.** Frisian, Old English, Old Saxon, Old Norse and Frankish were close enough to trade in; Latin was the
  priests' and the charters'; Arabic and Greek came as coin.
- **News.** As fast as a ship and no faster: a day and a half from London to Dorestad, weeks to Birka, months to
  Constantinople. A tavern knew the last three ports in; a reeve knew the king's letters; a priest knew the calendar.

## 5. Logistics and the real numbers

| Thing | Figure | Standing |
|---|---|---|
| The Gislinge boat (the player's first) | about 7.7 m, 0.9 t of cargo, 4 men | the game's data |
| Skuldelev 3 (coaster, c. 1040) | 14 m, under 5 t, crew 5-8 | verified |
| Klåstad (c. 990, Vestfold) | about 21 m, carried whetstones; roughly 13 t (check) | memory |
| Skuldelev 1 (ocean trader, c. 1030) | 16 m, 24 t at 40 % freeboard, crew 6-8, 5-7 knots average, 13 knots best | verified |
| Hedeby 3 (Baltic trader) | 16 m, about 60 t | verified |
| Passage speeds | 5-7 knots under sail with a fair wind; a day's sail about 100-150 km with daylight only | verified for the ships above |
| Dorestad to London | 430 km real; 2-4 days with weather and tide | the economy doc |
| Kaupang to Hedeby | 5 days (Ohthere) | verified |
| Hedeby to Truso | 7 days and nights under sail (Wulfstan) | verified |
| The Rhine, Cologne to Dorestad (about 300 km of river) | 3-5 days down; 10-15 days up, rowed and towed | guess from the current |
| Bread | the Frankish 2-pound loaf (794); about a loaf a man a day; ale or water 3-4 litres | the loaf verified, the ration a guess |
| Tolls | a tenth of the sale at the Frankish ports; by the ship at London (½ d a small boat, 1 d a sailing ship, 4 d a keel); in kind to the king's man in the north | the economy doc |
| Ship upkeep | hauled up Oct-Mar; re-tarred, re-caulked, a few planks and a new sail every few years | guess |
| The round trip | a Frisian made two England runs in a season, or one Baltic run; the northern cargo reached Hedeby once a year, in late summer | guess |

The consequence for the game: at 24 real minutes a game day, a season is a long evening of play, and the year's
rhythm (section 1) is what the player feels, not the day's price. That is why the hint below is built on the season
and the news, not on the number.

## 6. The note on the item: how the player learns the market without numbers

Your idea, made exact: **every good carries one line of the merchant's own knowledge, in words, from what he has
actually seen or heard, with how old and how sure it is.** The number is there only if he saw it himself, and even then
it is a memory with a date, not a quote.

### 6.1 What the line is made of

The game already holds all the ingredients: the ledger (seen and told prices with day, source and hops), the news
(chronicle entries with region and day), the season tables (make_season and the cheap and dear months), the AI
fleet (who sailed where), the laws, and the port boards. The line is built from them, in this order, and the first
rule that fires wins:

| Rule | Example line | From |
|---|---|---|
| 1. Hard news that touches this good's market | "Walcheren was burned in June. Nobody there is buying." | chronicle entry in a wanted region, under 60 days old |
| 2. A price seen with your own eyes, recent | "You saw wine pay 91 d at Lundenwic, two days back." | ledger, seen, under 7 days |
| 3. A price told, recent | "A Frisian skipper said Lundenwic was paying well for wine. Last week." | ledger, told, under 14 days; hops 0-1 |
| 4. Talk and rumour | "Tavern talk at Quentovic: Rouen's wine is cheap this year." | ledger or news at sureness under 0.7 |
| 5. The season | "The vintage is in. New wine comes down the Rhine from Martinmas, and cheap." | make_season and the dear and cheap months |
| 6. Who sailed ahead | "Three Frisian ships left for London before you, deep-laden." | AI fleet departures from this port, last 3 days |
| 7. The law | "Swords may not leave the realm. The reeve at Quentovic searches." | trade_rules |
| 8. Nothing known | "No word on this since spring." | ledger empty or older than 90 days |

Age is said in words, never in days: *this morning, yesterday, two days back, last week, a fortnight ago, last month,
since spring, old talk*. Sureness is said by the verb: *you saw* (sure), *he said* (likely), *they say* (talk),
*there is a rumour* (doubtful). The source is a person, not a label: *a Frisian skipper*, *the toll-keeper*, *your
factor's letter*, *a priest from York*.

### 6.2 What the player sees

- **In the hold and the market:** each good has its name, its period name under it, a short plain line of what it is
  (from the goods table's note), and under that **the word**: the one hint line, in italics, with a small eye (seen)
  or ear (heard) mark and a tilt: up (worth carrying there), down, or none. No arrows on numbers; the tilt is the
  merchant's gut from the rule that fired.
- **On the chart:** a port shows the same line for the two or three goods you are carrying or have asked about.
- **In the chest:** tap the line to open the rune sticks: the full history of that good at that port, every note with
  its day and who said it, as already built in `PriceNotes.cs`.
- **In the tavern and on the quay:** asking about a good gives a new line and adds a stick.

### 6.3 Why this is better than a price list

- It is **true to the period**: no merchant of 800 had a price table; he had what he saw, what he was told and the
  season in his bones.
- It is **play, not reading**: the line is a bet. "A skipper said" is worth less than "you saw", and the player learns
  to go and look, to buy news with a cask, to keep a factor.
- It **never lies and never tells all**: a told price carries its small error (built: 3 % plus 2 % a hop), and the
  line says "well", not "91".
- It gives the user's **words** room: every line is a wording template in `wording_793.json` with slots (good, port,
  person, age), which they can rewrite.

### 6.4 Build plan (two nights)

1. `seasons_793.json` (new): region x month: offers (goods with a strength), wants, fairs and things, closed water,
   and a short plain line per month ("slaughter month: salt dear"). Section 2 as data. The port tick reads it for
   `usual_t`, and the hint reads it for rule 5.
2. `Known.Word(goodId, portId)` in the Known code: runs the eight rules over the ledger, the news, the season table,
   the fleet log and the laws; returns a wording key, its slots, the mark and the tilt. Deterministic, no
   randomness, under a millisecond per good.
3. Wording keys in `wording_793.json`: one per rule and age band, placeholders for the user's words.
4. UI: the line under each good in the market panel and the hold; the eye and ear marks; tap to the sticks.
5. Tests: eight rules, age bands, the "nothing known" fall-through, a told price never shows a number.

## 7. What is still thin, and where I would look next (no new searches needed)

- **Hard prices beyond 794.** Only Frankfurt gives a list. The rest is inference from wergilds and tolls. Lex Frisionum
  (802) and Ine's laws give values in cattle and shillings that could anchor livestock and slaves; the Raffelstetten
  tariff (903-906) anchors salt and slaves per head.
- **Fair dates in the north.** Ribe's summer market is archaeological, not dated; Birka's winter market is inferred;
  the Disting is attested late. Keep them as the game's own dates, marked.
- **Volumes.** How many ships a season, how many casks: unknown. The Dorestad wells (about 650 wine casks reused) are
  the best hint of the scale of the wine trade.
- **Wages.** Nothing survives for a sailor's pay; the félag share is the period's answer.

## Sources

- Saint-Denis fairs (9 Oct-29 Nov; 24 Feb): https://archeologie.culture.gouv.fr/saint-denis/en/saint-denis-fair ; https://archeologie.culture.gouv.fr/saint-denis/en/saint-deniss-markets-and-fairs
- Ribe seasonal in the 8th century, Kaupang, Birka, Hedeby: https://fjorntheskald.com/2016/09/09/viking-age-settlements-early-towns/ ; https://arca.unive.it/retrieve/e4239ddb-413b-7180-e053-3705fe0a3322/07_feveile.pdf ; https://whc.unesco.org/document/166146
- Ohthere's tribute list and voyages: https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland ; https://germanic.ge/en/ang/record-trans/ohthere%E2%80%99s-account-en/
- Frisian cloth and trade: https://frisiacoasttrail.blog/2020/10/16/haute-couture-from-the-salt-marshes/ ; https://books.ub.uni-heidelberg.de/propylaeum/catalog/view/1681/2913/129487
- Ships: https://en.wikipedia.org/wiki/Skuldelev_ships ; https://canadianmysteries.ca/sites/vinland/vikinglife/ships/4145en.html
- Navigation: https://www.vikingeskibsmuseet.dk/en/professions/education/knowledge-of-sailing/instrument-navigation-in-the-viking-age ; https://www.sciencenordic.com/denmark-history-society--culture/how-vikings-navigated-the-world/1377436 ; https://canadianmysteries.ca/sites/vinland/vikinglife/navigation/4571en.html
- Estates and renders: https://en.wikipedia.org/wiki/Capitulare_de_villis ; https://stars.library.ucf.edu/oerhistory-all/69/
- Furs and Birka's markets: https://en.wikipedia.org/wiki/Birka ; https://vikingage.org/wiki/wiki/Fur_%26_Feathers ; https://www.medievalists.net/2022/07/vikings-beaver-furs/
- Ine's laws: https://constitution.org/1-History/sech/sech_004.htm
- Everything else: the existing economy_and_trade.md and its sources, and goods_793.json.
