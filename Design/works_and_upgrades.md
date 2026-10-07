# Works and upgrades: what silver turns into (4 Oct 2026)

The user asked for "upgrades". In 793 a merchant does not upgrade; he builds, buys a share, or gives. The catalogue is
`Assets/Kapman/Game/Data/story/works_793.json`: 39 kinds of work a house of 793 to 1066 could really pay for, each with
its evidence, cost, needs, what it gives, its upkeep and what can take it away. Three kinds: a **house work** (yours:
19), a **town work** (everyone's, with your name on it: 10), a **share** (a part of something that pays each year: 10).
A port's own works (`places/<port id>.json`, `"works"`) are instances of these kinds: `dorestad.work.jetty`.

## 1. How silver turns into something

Silver is the score (concept 4.2), and in the chest it does nothing but wait for a raider, a toll-man or a wreck.
A work is how it stops being silver. It becomes one of seven things, and every work is one or two of them:

| Silver becomes | Because | Works |
|---|---|---|
| **Days** | no lighterage, no waiting for light, a dry boat in spring | jetty, causeway, well, boat_shed, beacon |
| **Seasons** | goods wait for the dear season (wine 37 d in November, 91 d in June) | store_corner, storehouse, cellar, factor_house |
| **Goods every year** | they wait at a place on a feast day, so they make routes | flock_share, fish_weir, mill_share, salt_pan, vineyard_share, ship_share, weaving_shed, quern_shed, booth |
| **Rights** | no weigh-fee, no bridge toll, a first-day place, brothers who swear for you | scales, fair_booth, ferry_right, bridge_share, guild_seat |
| **Standing** | the town and the church remember who paid | altar_cloth, bell, book, hospice, standing_cross, thing_stead, church_rebuilt, causeway |
| **Safety** | the flood, the raid and the fire take half, or nothing | terp, wall_share, bell, gate_and_bell_house, cellar, well |
| **A name that lasts** | the heir starts higher, with one more friend | hall, chapel, grave_stone, rune_stone, book_of_life |

The rules, for whoever codes it:

- **Buying.** At a site, clock held: `act:rent_store` (store_corner), `act:buy_plot` (plot), `act:fund_work` (town works),
  or through the site's person. Or as the one thing to build over winter at the reckoning (concept 4.4).
  `days` is the builders' time; the house sails on and finds it done. Goods the house brings (oak, copper, a book, silk) cut the cost.
- **Upkeep** falls due at the winter reckoning. Two winters unpaid and the work is lost. A full ladder at home costs about 160 d a year: one good voyage.
- **Loss.** `can_be_lost` names what takes it, in the chronicle's own kinds (raid, burning, storm, flood, winter, murrain, war),
  plus a lord's whim, a wreck, a bad partner and the river moving. The chronicle is real, so the danger can be read ahead:
  Dorestad is raided four years running from 834 and gone by 863. Every work there goes with it, and the house must make
  a new home. That is the great turn of the first three lifetimes, and the reason to build abroad before it comes.
- **Yields wait where they are made** (three casks in the press-house at Mogontia at Martinmas). Fetch them, or they are sold there cheap.
- **Returns.** A share pays itself back in six to twelve years (a Domesday mill was worth 10 shillings a year). Works the
  house must feed (wool for the looms, blanks for the quern-dresser) pay faster. Nothing pays like a good voyage: works are for heirs.
- **Heirs** inherit every work, its upkeep, and the standing it earned.

## 2. The ladder: from a rented corner to a hall with a chapel

The rungs are not locks. Only the `needs` are: a plot before a storehouse or a jetty, a hall before a chapel, both before a gate.

| Rung | The house is | Works (cost in d) | What changes |
|---|---|---|---|
| 1 | **A guest** | store_corner 6 (and 12 a year), fair_booth 120, altar_cloth 240 | six units wait for the dear season; a first-day place at a fair; one church knows your name |
| 2 | **A plot-holder** | plot 480, well 40, storehouse 360, booth 90 | thirty units under your own roof; free lodging; house works open at this port |
| 3 | **A man with a waterfront** | jetty 600, boat_shed 240, scales 120, cellar 240, weaving_shed 180, quern_shed 150, causeway 300, terp 300 | a day saved each call; the boat winters dry; no weigh-fee; the house makes cloth and querns |
| 4 | **A man with shares** | ship_share 300, flock_share 200, fish_weir 240, mill_share 300, salt_pan 360, factor_house 400, ferry_right 480, bridge_share 600, vineyard_share 900 | goods and pennies come each year without the boat; a man of yours in a far port |
| 5 | **A householder** | hall 1,200, grave_stone 120, rune_stone 300, book_of_life 240, guild_seat 240, wall_share 500, beacon 200 | feasts, a crew that winters with you, sworn partners; the dead are remembered |
| 6 | **A patron** | chapel 1,500, gate_and_bell_house 600, bell 480, book 900, hospice 900, standing_cross 720, thing_stead 400, church_rebuilt 1,800 | the town and the church owe the house; among the English a thegn's rank is in reach |

The straight road at home (corner, plot, well, storehouse, jetty, boat shed, hall, chapel) is 4,426 d. The top of the ladder
is the period's own: Archbishop Wulfstan (1002-1023) wrote that a man with a hall, a church, a bell-house and a gate was
worthy of a thegn's rank, and so was a merchant who had crossed the open sea three times at his own cost.

## 3. The money (CHAINS.md's scale)

| What | d | Beside it |
|---|---|---|
| A corner of a storehouse | 6, and 12 a year | half an ordinary leg (10 to 40 d) |
| A plot | 480 | the debt. A real price: a London plot in 857 cost 60 shillings and 12 pence a year (charter S 208) |
| A jetty | 600 | a small boat (about 600 d) |
| A hall | 1,200 | two small boats, or four good voyages (100 to 300 d each) |
| A chapel, a burned church made new | 1,500, 1,800 | a chain's finale (600 to 2,500 d) |
| The whole road to a hall with a chapel | 4,426 | a sea-going trader (several thousand) |

So each winter the house chooses between a bigger ship and a deeper root, and a raid can undo the root.

## 4. Works as rewards of chains

The flag is `unlock_work_<port id>_<work id>` (CHAINS.md), for example `unlock_work_sliesthorp_bell`. It gives the work
**finished**: no cost, no days, no needs asked. Upkeep and loss begin at once, so a gift can be a burden (CHAINS.md: "a
reward may carry a cost that lasts"). A work at the home port takes the home port's id. Each work lists `aliases`, the
other words a chain may have used (`wharf`, `pier` and `hithe` all mean `jetty`).

- **Only a grant gives them** (`got_by: grant`: silver alone cannot): **scales** (the reeve's: London 857 and 889),
  **ferry_right** (the lord of the bank), **fair_booth** (the abbot of Saint-Denis). And in some places: a **bell** at
  Sliesthorp (King Horik's leave, 854), a **chapel** in a heathen king's port (Herigar at Birka, about 830), a
  **standing_cross** in the German kingdom after 958 (the lord's market right), a **guild_seat** by `unlock_title_<name>`.
- **Turn rewards** (worth 80 to 250 d): well, booth, scales, fair_booth, grave_stone, quern_shed, weaving_shed, flock_share,
  beacon, boat_shed, fish_weir, cellar, altar_cloth, book_of_life, guild_seat.
- **Between the two** (300 to 480 d: a strong turn, or half a finale): causeway, mill_share, salt_pan, thing_stead, bell,
  and ferry_right by grant.
- **Finale rewards** (worth 600 to 2,500 d): jetty, bridge_share, standing_cross, vineyard_share, book, hospice, hall, chapel,
  church_rebuilt; a plot with its storehouse through `unlock_plot_<port id>`; a factor_house with `unlock_factor_<port id>`;
  a ship_share with `unlock_partner_<name>`.
- **Not rewards, always bought:** store_corner, terp, rune_stone, wall_share, gate_and_bell_house.

## 5. Open points

- **Home.** `where: home` needs a rule for which port is home and for moving it when Dorestad ends in 863.
- **`unlock_title_thegn`** is named here (gate_and_bell_house) and is in no table yet. Standing numbers in `needs` are guesses.
- **Left out on purpose:** a mint or a toll (the king's), a guild before 1000, dykes in Frisia (after about 1200), the
  Frisians' house at Rome (off the chart). **Not checked against a page:** everything marked `(?)` in the catalogue.
