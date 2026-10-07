# The ship roster: the ladder a house climbs, 793 to 1066

The data is `Assets/Kapman/Game/Data/story/ships_793.json`: twenty craft a house can own, each a full class in the
format of `ship_classes_793.json`. The evidence is `Design/content/ships.md`. Marks: **(D)** derived or judged,
**(?)** not checked. No sale price for a merchant ship survives from these years: every price here is derived from
the hours the replicas took (ships.md 8.2).

**The money scale** (CHAINS.md): the debt is 480 d; an ordinary leg makes 10 to 40 d; a good voyage 100 to 300 d; a
chain's finale pays 600 to 2,500 d. "Voyages" below is the price in good voyages of 200 d.

## The ladder

Two truths shape it. Before about 900 a house grows by **more hulls**, because no trader found carries more than 6
or 7 t. After about 900 it grows by **size**, with the same handful of men. And the river and the sea are **two
fleets**: flat craft of oak with no iron, and keel ships that need iron, wool and a winter.

| Rung | From | Ship (id) | Cargo | Men | Price | Voyages | Upkeep a year | What the step opens |
|---|---|---|---|---|---|---|---|---|
| 1 | 793 | Six-oar boat (`gislinge`) | 0.66 t | 4 | 600 d | 3 | 15 d | The start. A little of everything: creeks, the flats, the coast, the sea on a settled day. |
| 2 sea | 793 | Coaster, 10 m (`coaster_minster_ship`) | 2.5 t | 4 to 6 | 1,200 d | 6 | 30 d | **A sea:** the crossing to Lundenwic as an ordinary summer voyage. Still lands on any beach. |
| 2 north | 793 | Norse carrier, 11 m (`norse_cargo_boat`) | 1.8 t | 5 | 1,100 d | 5 to 6 | 28 d | **A sea:** the Danish islands and the Baltic in summer. |
| 2 river | 793 | Small Utrecht-type freighter (`logboat_expanded`) | 2.5 t | 3 + haulers | 300 d | 1 to 2 | 12 d | **A river:** the Rhine to Cologne as a trade. Wine down, querns down. |
| 2 river | 793 | Barge, 15 m (`river_barge`) | 7 t | 3 + a gang | 600 d | 3 | 24 d | **A cargo:** bulk (stone, grain, wine in tuns). River only, ten years of life. |
| 3 | 793 | The keel, 14 m (`north_sea_merchant_ship`) | 6 t | 5 or 6 | 3,000 d | 15 | 75 d | **A cargo at sea:** ten times the boat. The first real fork is behind you; this is "the ship". |
| 3 north | 793 to 900 | Norse ship of 16 oars (`norse_allround_ship`) | 4 t | 12 | 3,000 d | 15 | 75 d | **A sea and a season:** open water in most summer weather, and oars to get out of trouble. Twelve mouths. |
| 3 river | 826 | Large barge, 20 m (`river_barge_large`) | 18 t | 3 + a gang | 800 d | 4 | 32 d | **A cargo:** whole loads for a sea ship to take on. |
| 3 flats | 870 | Early cog (`cog_early`), a reconstruction | 10 t | 6 | 3,500 d | 17 to 18 | 88 d | **The flats:** the Wadden, the Almere and the Limfjord at any tide; loads aground. |
| 3 north | 900 | Small sailing trader, 14 m (`small_sailing_trader`) | 4.6 t | 5 | 2,400 d | 12 | 60 d | **A crew saved:** the Baltic run with five men, not twelve. |
| 3 river | 950 | Utrecht ship, 17 m (`utrecht_ship`) | 13 t | 4 + haulers | 1,200 d | 6 | 48 d | **River and sheltered sea in one hull:** Cologne to the Frisian coast with no reloading. |
| 4 | 900 | Knarr, 16 m (`knarr`) | 20 t | 6 to 8 | 6,000 d | 30 | 150 d | **The open sea and a season:** three times the keel with the same crew; she rides out a gale. Needs a jetty or her boat. |
| 5 | 1025 | Great ship of Hedeby (`hedeby_great_ship`) | 50 t | 8 to 12 | 15,000 d | 75 | 375 d | **A season's trade in one voyage.** Only with partners, a factor and a jetty at both ends. |

Side roads, off the ladder: `west_small_boat` (150 d; a lighter, and the towed boat a knarr needs),
`norse_small_boat` (250 d; the cheap second hull in the north), `baltic_coastal_boat` (500 d, 2.5 t) and
`slav_trader` (1,500 d, 6 t, from 900): the amber coast, for a house with a Baltic tongue or a factor there.

## What it costs, read plainly

- **The first step is the debt over again.** A coaster is 1,200 d: two and a half debts, six good voyages. The river
  freighter is 300 d, so the cheap way up is the river. That is the first real choice: a coaster to Lundenwic or a
  river ship to Cologne (ships.md 10, stage 2).
- **The keel is a finale.** 3,000 d is more than one chain pays (600 to 2,500 d). A chain finale plus a season buys
  her; so does a third down and the rest at the launch (town_actions build_ship).
- **A tonne of carrying gets cheaper as you climb:** about 900 d a tonne in the boat, 500 in the keel, 350 in the
  cog, 300 in the knarr and the great ship, under 100 on the river. A house grows by moving down that column.
- **The knarr is an heir's ship.** 6,000 d is thirty good voyages, and she is not offered before 900. A rich
  player in 810 buys a second and third keel, a barge, a jetty, a storehouse and a charter instead. She is bought in
  shares (a quarter or a half) as ships.md reads Skuldelev 1.
- **Upkeep** is 2.5 per cent of the price a year for keel ships and 4 per cent for river craft (D), wages and food
  apart. Crew is the real running cost: the Norse ship of 793 feeds twelve for 4 t; the knarr feeds seven for 20 t.
- **Life:** 25 to 30 years for a clinker ship, with new rivets in year 15 for a tenth of her price; ten years for a
  barge. Second-hand: half price at ten to fifteen years, a quarter at twenty.

## Rewards and things bought

Every ship has a flag `unlock_ship_<id>` (CHAINS.md, the grand rewards). What the flag means differs.

| How she is had | Ships | Note |
|---|---|---|
| **Reward only** | `river_estate_ship`, `currach`, `snekke` | Never for sale. A bishop lends his river ship with cabins for a season. The monks of Hi have a hide boat made for a man who has served them (120 d of hides). A king or a shire lays a warship of 26 oars on a house of rank, late in the game: thirty men, almost no cargo, an honour and a duty (?). |
| **Bought, and a good reward** | `north_sea_merchant_ship`, `norse_allround_ship`, `knarr`, `hedeby_great_ship`, `cog_early`, `coaster_minster_ship`, `slav_trader`, `gislinge`, `norse_small_boat` | The flag gives the hull itself, or the right to order one where a stranger could not. The Dane sells only to a man he has eaten with; the great ship needs a plot and a partner at Sliesthorp; a Frisian district that owes a rowed cog may give the right to build one; a minster lends its coaster until about 870. |
| **Bought only** | `west_small_boat`, `logboat_expanded`, `river_barge`, `river_barge_large`, `utrecht_ship`, `norse_cargo_boat`, `small_sailing_trader`, `baltic_coastal_boat` | Working craft: silver, a wright and the standing to be trusted with timber. Too plain to end a chain on. |

Eight of these flags are new and not yet in `story/unlocks_793.json`: `west_small_boat`, `river_barge_large`,
`utrecht_ship`, `norse_cargo_boat`, `cog_early`, `small_sailing_trader`, `slav_trader`, `snekke`.

## For the programmer

- Eleven of the twenty ids are already rows in `ship_classes_793.json`; each lists its `changed_from_table` (mostly
  the corrections of ships.md 11.2: the Gislinge at 1.64 m beam and 0.66 t, hauling upstream at 10 to 12 km a day
  and not 18, the keel slower under oars and weaker in a gale). Nine are new rows. `knarr` and `hedeby_great_ship`
  use the ids `unlocks_793.json` already waits for.
- New fields beside the class fields: `built_at`, `cost_d`, `build_days`, `crew_min`, `crew_full`, `cargo_kg`,
  `can_go` (river, flats, coast, open_sea: yes, limited or no), `upkeep_d_a_year`, `life_years`, `how_had`,
  `unlock_flag`, `rung`, `what_it_is`, `why_want_it`, `source`, `judged`.
- `from` and `to` are real years here (793, 1066); the class table writes 0 for both.
- **Where this file and `unlocks_793.json` disagree** (one should give way): prices of `logboat_expanded` (300
  against 250), `river_barge` (600 against 900), `north_sea_merchant_ship` (3,000 against 2,400),
  `hedeby_great_ship` (15,000 against 12,000); the knarr from 900 at Norse yards, not 1000; the Baltic boat to
  1066, not 850; the coaster bought as well as lent.
- Ports missing from `ports_793.json` that the late ladder wants: Tiel (the Utrecht ship, the knarr of about
  1000), and a tenth-century Slav port (Wolin or Ralswiek) for `slav_trader`.

## What is weakest

- **The cog** has a name, a due and a shape on coins, and no hull before 1150: every number is a reconstruction.
- **The great ship** is one find and one hypothetical reconstruction (40 to 60 t).
- **The warship as a reward** rests on the levy of 1008 falling on a merchant house, which is not shown (?).
- **All prices** are derived. The Norse ship of 793 (3,000 d) and the Slav boats have no derivation in ships.md at all.
- Nothing was opened again for this file: it stands on ships.md and the class table, with their marks.
