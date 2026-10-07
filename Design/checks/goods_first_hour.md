# Goods in the first hour (May-June 793)

Ports: dorestad, traiectum, witla, walichrum, lundenwic. A new game begins at Dorestat on 1 May 793.
Read: goods_793.json, trade_rules.json, seasons_793.json, trade_year_793.json, ports_793.json, Art/art_list.csv, Code/Game/Trade/Market.cs (and GameData.MadeFactor / WantedFactor). Nothing here is invented; every number is worked from those files by the rule in Market.cs.

## How a market decides what it holds (Market.cs)

- Every market lists every good traded in the year (GoodDef.Traded: the good's from/to). What differs is stock and price.
- A port's goods come from its REGION, not the port: made = good.made[region], wanted = good.wanted[region]. So dorestad, traiectum and witla (all rhine_delta) make and want the same goods at the same usual price; only size differs. walichrum is zeeland; lundenwic is england_south.
- Usual stock = size x made x stock_value (150 d) / value, held between 1 and 120 units; 0 if the region does not make the good ("it only has what's brought"). On offer only when stock >= 1. Sizes: dorestad 5, traiectum 3, witla 2, walichrum 3, lundenwic 5.
- Day one the stock is 75 to 125 % of the usual (a fixed hash of port and good, the same every game).
- Price = value x made factor x wanted factor x season (cheap months 0.85, dear months 1.2, blended over 20 days at the months' edges) x (usual / stock) ^ 0.4, held between 0.6 and 2.2. A good the region neither makes nor wants is paid at 0.6 of its value. Selling more than the market's appetite (size x wanted x 80 d / value) gluts it and the price paid falls.
- The market asks 10 % over its price and pays 10 % under (spread 0.2). Stock settles back 5 % of the gap a day.
- Grain (wheat, rye, barley, oats, malt, flour), wine and herring have a season of making (seasons_793.json): where made, the stock settles toward a share of the usual. In May-June: grain 0.64 rising to 0.75 (the hungry spring: low stock, dear), wine 1.0 falling toward 0.85 (old wine running low), herring 0.65 to 0.70 (few barrels).
- trade_year_793.json moves no price; it is the word on the item. May: rhine_delta "comes" bar_iron and ships_bread, "wanted" cattle; zeeland "wanted" badorf_ware. June: rhine_delta and zeeland have neither.
- Money (trade_rules.json): you start with 40 deniers (and 25 pennies, 6 dirhams, 30 g hacksilver) and a debt of 480 d to the count, due in 365 days. Frankish zone (the four home ports): toll 0.1, foreign coin loses 0.1. English zone (lundenwic): pennies (1 p = 0.706 d), toll 0.05, foreign coin loses 0.1. All prices below are in deniers.
- Laws that touch these ports: arms (no export from the Frankish realm since 779), captives (no export, Frankish and English), sacred (no sale).

## The table

One row per good that the port's region makes (the market holds stock: you can buy) or wants (it pays dear: you sell). Price = the usual price in deniers (Market.UsualPrice: value x made factor x wanted factor x season), May / June. You pay 10 % over it and get 10 % under it (spread 0.2). Stock = the usual stock at the port's size (the day-one stock is 75 to 125 % of it).

| good | port | sold or bought there | made | wanted | usual price May / June (d) | usual stock | unit | kg | art slot good.<id> | word of the month (trade_year) |
|---|---|---|---|---|---|---|---|---|---|---|
| Wheat (wheat) | lundenwic | sold (made here) | 2 |  | 4.7 / 4.7 | 120 | sack (about a modius and a half) | 60 | yes |  |
| Ship's bread (ships_bread) | dorestad | sold (made here) | 1 |  | 2.4 / 2.4 | 120 | sack of hard loaves | 20 | yes | comes May |
| Ship's bread (ships_bread) | traiectum | sold (made here) | 1 |  | 2.4 / 2.4 | 120 | sack of hard loaves | 20 | yes | comes May |
| Ship's bread (ships_bread) | witla | sold (made here) | 1 |  | 2.4 / 2.4 | 100 | sack of hard loaves | 20 | yes | comes May |
| Ship's bread (ships_bread) | lundenwic | sold (made here) | 1 |  | 2.4 / 2.4 | 120 | sack of hard loaves | 20 | yes |  |
| Rhenish wine (rhenish_wine) | dorestad | sold (made here) | 2 |  | 39.0 / 46.8 | 25 | cask (about 100 litres) | 115 | yes |  |
| Rhenish wine (rhenish_wine) | traiectum | sold (made here) | 2 |  | 39.0 / 46.8 | 15 | cask (about 100 litres) | 115 | yes |  |
| Rhenish wine (rhenish_wine) | witla | sold (made here) | 2 |  | 39.0 / 46.8 | 10 | cask (about 100 litres) | 115 | yes |  |
| Rhenish wine (rhenish_wine) | lundenwic | bought dear (wanted) |  | 2 | 84.0 / 100.8 | 0 (only what ships bring) | cask (about 100 litres) | 115 | yes | wanted June |
| Frankish wine (Seine and Loire) (frankish_wine) | lundenwic | bought dear (wanted) |  | 3 | 85.0 / 102.0 | 0 (only what ships bring) | cask | 115 | yes | wanted June |
| Wine of Aquitaine (aquitaine_wine) | lundenwic | bought dear (wanted) |  | 2 | 63.0 / 75.6 | 0 (only what ships bring) | cask | 115 | yes |  |
| Bay salt (sea_salt) | walichrum | sold (made here) | 1 |  | 2.9 / 2.9 | 120 | sack | 50 | yes |  |
| Droitwich salt (brine_salt) | lundenwic | bought dear (wanted) |  | 1 | 4.6 / 4.6 | 0 (only what ships bring) | sack | 50 | yes |  |
| Salt herring (herring) | dorestad | bought dear (wanted) |  | 1 | 13.8 / 13.8 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Salt herring (herring) | traiectum | bought dear (wanted) |  | 1 | 13.8 / 13.8 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Salt herring (herring) | witla | bought dear (wanted) |  | 1 | 13.8 / 13.8 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Eels (eels) | lundenwic | bought dear (wanted) |  | 1 | 13.8 / 13.8 | 0 (only what ships bring) | barrel (salted or smoked) | 80 | yes |  |
| Salt salmon (salmon) | lundenwic | bought dear (wanted) |  | 1 | 20.7 / 17.6 | 0 (only what ships bring) | barrel | 90 | yes |  |
| Cheese (cheese) | walichrum | sold (made here) | 2 |  | 3.1 / 2.6 | 120 | wheel | 10 | yes |  |
| Butter (butter) | lundenwic | bought dear (wanted) |  | 1 | 6.9 / 5.9 | 0 (only what ships bring) | crock (salted) | 20 | yes |  |
| Salt pork and bacon (salt_pork) | lundenwic | sold (made here) | 1 |  | 11.2 / 13.4 | 54 | barrel | 90 | yes |  |
| Honey (honey) | lundenwic | sold (made here) | 1 |  | 23.0 / 23.0 | 31 | crock | 20 | yes |  |
| Olive oil (olive_oil) | lundenwic | bought dear (wanted) |  | 2 | 56.0 / 56.0 | 0 (only what ships bring) | jar | 30 | yes |  |
| Pepper (pepper) | lundenwic | bought dear (wanted) |  | 2 | 67.2 / 67.2 | 0 (only what ships bring) | pound (about 400 g) | 0.4 | yes |  |
| Cinnamon (cinnamon) | lundenwic | bought dear (wanted) |  | 2 | 84.0 / 84.0 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Cloves (cloves) | lundenwic | bought dear (wanted) |  | 2 | 112.0 / 112.0 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Ginger (ginger) | lundenwic | bought dear (wanted) |  | 1 | 46.0 / 46.0 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Cumin (cumin) | lundenwic | bought dear (wanted) |  | 1 | 13.8 / 13.8 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Dates, figs and raisins (dates_figs) | lundenwic | bought dear (wanted) |  | 1 | 17.2 / 17.2 | 0 (only what ships bring) | basket | 15 | yes |  |
| Hazelnuts (hazelnuts) | lundenwic | sold (made here) | 1 |  | 2.9 / 2.9 | 120 | sack | 30 | yes |  |
| Cattle (cattle) | dorestad | bought dear (wanted) |  | 1 | 33.1 / 27.6 | 0 (only what ships bring) | head | 300 | yes | wanted May |
| Cattle (cattle) | traiectum | bought dear (wanted) |  | 1 | 33.1 / 27.6 | 0 (only what ships bring) | head | 300 | yes | wanted May |
| Cattle (cattle) | witla | bought dear (wanted) |  | 1 | 33.1 / 27.6 | 0 (only what ships bring) | head | 300 | yes | wanted May |
| Sheep (sheep) | dorestad | bought dear (wanted) |  | 1 | 6.9 / 6.9 | 0 (only what ships bring) | head | 40 | yes |  |
| Sheep (sheep) | traiectum | bought dear (wanted) |  | 1 | 6.9 / 6.9 | 0 (only what ships bring) | head | 40 | yes |  |
| Sheep (sheep) | witla | bought dear (wanted) |  | 1 | 6.9 / 6.9 | 0 (only what ships bring) | head | 40 | yes |  |
| Pigs (pigs) | lundenwic | sold (made here) | 1 |  | 8.0 / 8.0 | 75 | head | 80 | yes |  |
| Frisian cloth (frisian_cloth) | lundenwic | sold (made here) | 1 |  | 38.4 / 38.4 | 16 | bolt | 6 | yes |  |
| Woollen cloth (wool_cloth) | lundenwic | sold (made here) | 2 |  | 11.7 / 11.7 | 83 | bolt | 8 | yes |  |
| Linen (linen) | lundenwic | bought dear (wanted) |  | 1 | 27.6 / 27.6 | 0 (only what ships bring) | bolt | 4 | yes |  |
| Fine linen (fine_linen) | lundenwic | bought dear (wanted) |  | 1 | 69.0 / 69.0 | 0 (only what ships bring) | bolt | 3 | yes |  |
| Silk (silk) | lundenwic | bought dear (wanted) |  | 3 | 612.0 / 612.0 | 0 (only what ships bring) | bolt | 1.5 | yes |  |
| Wool (raw_wool) | lundenwic | sold (made here) | 2 |  | 5.2 / 4.4 | 120 | sack | 30 | yes | comes June |
| Flax (flax) | lundenwic | bought dear (wanted) |  | 1 | 6.9 / 6.9 | 0 (only what ships bring) | bundle | 20 | yes |  |
| Woad (woad) | lundenwic | bought dear (wanted) |  | 1 | 11.5 / 11.5 | 0 (only what ships bring) | balls (a basket) | 10 | yes |  |
| Madder (madder) | walichrum | sold (made here) | 1 |  | 9.6 / 9.6 | 38 | sack | 20 | yes |  |
| Kermes red (kermes) | lundenwic | bought dear (wanted) |  | 1 | 69.0 / 69.0 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Squirrel pelts (vair) (squirrel) | lundenwic | bought dear (wanted) |  | 2 | 28.6 / 33.6 | 0 (only what ships bring) | timber (40 skins) | 2 | yes |  |
| Marten (marten) | lundenwic | bought dear (wanted) |  | 3 | 173.4 / 204.0 | 0 (only what ships bring) | timber (40 skins) | 3 | yes |  |
| Fox pelts (fox) | lundenwic | bought dear (wanted) |  | 2 | 35.7 / 42.0 | 0 (only what ships bring) | bundle of 10 | 5 | yes |  |
| Ermine (ermine) | lundenwic | bought dear (wanted) |  | 2 | 238.0 / 280.0 | 0 (only what ships bring) | timber (40 skins) | 1 | yes |  |
| Bear skins (bear_skins) | lundenwic | bought dear (wanted) |  | 2 | 67.2 / 67.2 | 0 (only what ships bring) | skin | 8 | yes |  |
| Cattle hides (cattle_hides) | lundenwic | bought dear (wanted) |  | 1 | 17.2 / 17.2 | 0 (only what ships bring) | bundle of 5 | 30 | yes |  |
| Tanned leather (leather) | lundenwic | sold (made here) | 1 |  | 19.2 / 19.2 | 31 | bundle | 20 | yes |  |
| Walrus-hide rope (walrus_rope) | lundenwic | bought dear (wanted) |  | 1 | 34.5 / 34.5 | 0 (only what ships bring) | coil (60 ells) | 8 | yes |  |
| Walrus ivory (walrus_ivory) | lundenwic | bought dear (wanted) |  | 3 | 153.0 / 153.0 | 0 (only what ships bring) | pair of tusks | 2 | yes |  |
| Whalebone (whalebone) | lundenwic | bought dear (wanted) |  | 1 | 34.5 / 34.5 | 0 (only what ships bring) | piece | 10 | yes |  |
| Mayen quernstones (quernstones) | lundenwic | bought dear (wanted) |  | 2 | 16.8 / 16.8 | 0 (only what ships bring) | pair | 90 | yes |  |
| Millstones (millstones) | lundenwic | bought dear (wanted) |  | 2 | 84.0 / 84.0 | 0 (only what ships bring) | stone | 500 | yes |  |
| Whetstones (whetstones) | dorestad | bought dear (wanted) |  | 1 | 9.2 / 9.2 | 0 (only what ships bring) | bundle of 20 | 10 | yes |  |
| Whetstones (whetstones) | traiectum | bought dear (wanted) |  | 1 | 9.2 / 9.2 | 0 (only what ships bring) | bundle of 20 | 10 | yes |  |
| Whetstones (whetstones) | witla | bought dear (wanted) |  | 1 | 9.2 / 9.2 | 0 (only what ships bring) | bundle of 20 | 10 | yes |  |
| Whetstones (whetstones) | lundenwic | bought dear (wanted) |  | 1 | 9.2 / 9.2 | 0 (only what ships bring) | bundle of 20 | 10 | yes |  |
| Rhenish pottery (badorf_ware) | walichrum | bought dear (wanted) |  | 2 | 8.4 / 8.4 | 0 (only what ships bring) | crate of 10 | 20 | yes | wanted May |
| Rhenish pottery (badorf_ware) | lundenwic | bought dear (wanted) |  | 1 | 6.9 / 6.9 | 0 (only what ships bring) | crate of 10 | 20 | yes |  |
| Ipswich ware (ipswich_ware) | lundenwic | bought dear (wanted) |  | 1 | 4.6 / 4.6 | 0 (only what ships bring) | crate of 10 | 20 | yes |  |
| Glass vessels (glass_vessels) | lundenwic | bought dear (wanted) |  | 2 | 100.8 / 100.8 | 0 (only what ships bring) | crate of 6 | 4 | yes |  |
| Jet (jet) | lundenwic | bought dear (wanted) |  | 1 | 27.6 / 27.6 | 0 (only what ships bring) | sack | 2 | yes |  |
| Bar iron (bar_iron) | dorestad | sold (made here) | 2 |  | 6.5 / 6.5 | 120 | bundle of 10 bars | 10 | yes | comes May |
| Bar iron (bar_iron) | traiectum | sold (made here) | 2 |  | 6.5 / 6.5 | 90 | bundle of 10 bars | 10 | yes | comes May |
| Bar iron (bar_iron) | witla | sold (made here) | 2 |  | 6.5 / 6.5 | 60 | bundle of 10 bars | 10 | yes | comes May |
| Lead (lead) | lundenwic | sold (made here) | 1 |  | 12.0 / 12.0 | 50 | pig | 25 | yes | comes May, comes June |
| Swords (swords) | dorestad | sold (made here) | 1 |  | 67.2 / 67.2 | 9 | sword, hilted, in its scabbard | 1.5 | yes |  |
| Swords (swords) | traiectum | sold (made here) | 1 |  | 67.2 / 67.2 | 5 | sword, hilted, in its scabbard | 1.5 | yes |  |
| Swords (swords) | witla | sold (made here) | 1 |  | 67.2 / 67.2 | 4 | sword, hilted, in its scabbard | 1.5 | yes |  |
| Books (books) | lundenwic | bought dear (wanted) |  | 2 | 504.0 / 504.0 | 0 (only what ships bring) | book | 5 | yes |  |
| Parchment (parchment) | lundenwic | sold (made here) | 2 |  | 39.0 / 39.0 | 25 | bundle of 100 leaves | 8 | yes |  |
| Frankincense (incense) | lundenwic | bought dear (wanted) |  | 2 | 50.4 / 50.4 | 0 (only what ships bring) | pound | 0.4 | yes |  |
| Relics (relics) | lundenwic | bought dear (wanted) |  | 2 | 336.0 / 336.0 | 0 (only what ships bring) | relic in its reliquary | 2 | yes |  |
| Church plate (church_plate) | lundenwic | sold (made here) | 1 |  | 192.0 / 192.0 | 3 | piece | 1.5 | yes |  |
| Oak timber (oak_timber) | dorestad | bought dear (wanted) |  | 2 | 42.0 / 42.0 | 0 (only what ships bring) | load of 10 planks | 300 | yes |  |
| Oak timber (oak_timber) | traiectum | bought dear (wanted) |  | 2 | 42.0 / 42.0 | 0 (only what ships bring) | load of 10 planks | 300 | yes |  |
| Oak timber (oak_timber) | witla | bought dear (wanted) |  | 2 | 42.0 / 42.0 | 0 (only what ships bring) | load of 10 planks | 300 | yes |  |
| Oak timber (oak_timber) | walichrum | bought dear (wanted) |  | 2 | 42.0 / 42.0 | 0 (only what ships bring) | load of 10 planks | 300 | yes |  |
| Oak timber (oak_timber) | lundenwic | sold (made here) | 1 |  | 24.0 / 24.0 | 25 | load of 10 planks | 300 | yes |  |
| Yew staves (yew_staves) | lundenwic | sold (made here) | 1 |  | 16.0 / 16.0 | 38 | bundle | 10 | yes |  |
| Pine tar (tar) | dorestad | bought dear (wanted) |  | 1 | 23.0 / 23.0 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Pine tar (tar) | traiectum | bought dear (wanted) |  | 1 | 23.0 / 23.0 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Pine tar (tar) | witla | bought dear (wanted) |  | 1 | 23.0 / 23.0 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Pine tar (tar) | lundenwic | bought dear (wanted) |  | 1 | 23.0 / 23.0 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Hunting falcons (falcons) | lundenwic | bought dear (wanted) |  | 2 | 201.6 / 168.0 | 0 (only what ships bring) | bird | 1 | yes | wanted May |
| Hounds (hounds) | lundenwic | sold (made here) | 1 |  | 28.8 / 28.8 | 21 | hound | 40 | yes |  |
| Eider down (eider_down) | lundenwic | bought dear (wanted) |  | 1 | 46.0 / 46.0 | 0 (only what ships bring) | sack | 5 | yes |  |
| Whale oil (whale_oil) | dorestad | bought dear (wanted) |  | 1 | 18.4 / 18.4 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Whale oil (whale_oil) | traiectum | bought dear (wanted) |  | 1 | 18.4 / 18.4 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Whale oil (whale_oil) | witla | bought dear (wanted) |  | 1 | 18.4 / 18.4 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Whale oil (whale_oil) | lundenwic | bought dear (wanted) |  | 1 | 18.4 / 18.4 | 0 (only what ships bring) | barrel | 100 | yes |  |
| Beeswax (wax) | lundenwic | bought dear (wanted) |  | 2 | 56.0 / 56.0 | 0 (only what ships bring) | cake | 10 | yes |  |
| Sulphur (sulphur) | lundenwic | bought dear (wanted) |  | 1 | 11.5 / 11.5 | 0 (only what ships bring) | sack | 20 | yes |  |

Rows: 99. Goods in the table: 69 of 135 traded in 793 (140 in goods_793.json). Goods in the table with no art slot: none.
All goods in goods_793.json without a slot good.<id> in art_list.csv: none.
Not traded in 793 (from/to): hops, narwhal, pingsdorf_ware, stockfish, sword_blades.
Law on goods in the table: church_plate: sacred; relics: sacred; swords: arms.

## Best carries between the three regions (May)

Bought at the usual price + 10 % where it is made, sold at the usual price - 10 % in the other region; tolls and coin loss not taken off. Only goods the first region makes (so there is stock). Sorted by gain per unit.

| good | from region | to region | buy (d) | sell (d) | gain per unit (d) | gain per 100 kg (d) | units for the 40 d you start with | usual stock at the biggest port of the region |
|---|---|---|---|---|---|---|---|---|
| Rhenish wine (rhenish_wine) | rhine_delta | england_south | 42.9 | 75.6 | 32.7 | 28.4 | 0 | 25 |
| Oak timber (oak_timber) | england_south | rhine_delta | 26.4 | 37.8 | 11.4 | 3.8 | 1 | 25 |
| Oak timber (oak_timber) | england_south | zeeland | 26.4 | 37.8 | 11.4 | 3.8 | 1 | 25 |

Carries that pay: 3.

made_factor: {"3": 0.5, "2": 0.65, "1": 0.8, "0.5": 1.4}; wanted_factor: {"3": 1.7, "2": 1.4, "1": 1.15}.

## The six goods that make the first voyages, and why

FINDING FIRST: between these five ports only TWO goods pay at the usual prices (the carries table above: 3 carries, 2 goods). dorestad, traiectum and witla share one region, so nothing pays between them at usual prices (same price, minus the 20 % spread); only a day's stock (75 to 125 % of usual) moves it. The other four goods below are the ones the data puts in front of a new player in May and June, with what is missing for each.

1. Rhenish wine (rhenish_wine): the one real first voyage. Made in rhine_delta (2): 39 d in May at dorestad, traiectum, witla (usual stock 25 / 15 / 10 casks); wanted at lundenwic (2): 84 d. Buy 42.9, sell 75.6: 32.7 d a cask. BUT a cask costs 42.9 d and a new player has 40 deniers: he cannot buy one cask on day one at the usual price (only if dorestad's day-one stock is above usual, or by changing his pennies, dirhams or hacksilver at the 10 % loss). In June it is a dear month (46.8 at home, 100.8 in London; trade_year marks it "wanted June" at lundenwic). 115 kg a cask.
2. Oak timber (oak_timber): the only cargo home. Made in england_south, wanted in rhine_delta and zeeland: buy 26.4 at lundenwic, sell 37.8 at dorestad, traiectum, witla or walichrum: 11.4 d a unit, but only 3.8 d per 100 kg (heavy). trade_year has it "wanted" in rhine_delta and zeeland only from July.
3. Ship's bread (ships_bread): 2.4 d a sack of 20 kg at dorestad, traiectum, witla and lundenwic (stock 100 to 120), the word "comes May" in rhine_delta. No market among the five wants it: it is the voyage's food (goods note: about a day's food for 10 men a sack), not a trade.
4. Bar iron (bar_iron): made in rhine_delta (2), the word "comes May" at the three home ports, 10 kg a bundle, value 10: the cheap light good a 40 d purse can fill up on. Wanted in frisia, jutland, england_east, ireland, ladoga: NOT at any of the five ports. Its first buyer is north (doccinga, medemelacha if they are frisia: not checked here).
5. Bay salt (sea_salt) and 6. Cheese (cheese): what walichrum sells (salt 2.9 d a 50 kg sack; cheese 3.1 d in May, 2.6 in June, a 10 kg wheel; stock 120 each). Neither is wanted at any of the five ports (salt: the north; cheese: rhineland, neustria, norway_west), and both are in a dear month in May (salt also June). So walichrum gives a new player something to buy and nowhere among the five to sell it.

Also in front of the player with no source among the five: cattle ("wanted May" at the three home ports, 33.1 d; made in frisia, saxony, jutland, ireland) and Badorf ware ("wanted May" at walichrum, also wanted at lundenwic; made only in rhineland).

## What this means for the first hour (from the data, not a design)

- With 40 deniers and these five ports the first hour has one paying run (wine to London, timber back) and the purse is 2.9 d short of the first cask. Worth a decision: the start purse, or a cheaper first cargo with a buyer inside the five.
- walichrum and witla have no paying carry of their own at usual prices.
- Every good in the table has its art slot good.<id> (see the count line under the table).

## Not done / not checked

- Tolls (0.1 Frankish, 0.05 English) and coin exchange are not taken off the gains: how the code applies them was not read.
- Day-one stock per port and good (the Hash in Market.cs) was not worked out; the table gives the usual stock and the 75 to 125 % range.
- Usual prices use the whole-month season (Market.UsualPrice); the live price blends at the months' edges and reads scarcity (grain, wine, herring stock under their usual in spring makes them dearer where made).
- Places, encounters and story files were not read (cards or people that hand out goods or prices are not in this check).
- The other seven first-release ports (medemelacha, doccinga, daventre, quentovic, sarre, fordwic, ripa) are outside this task; several of the goods above find their buyers there.
