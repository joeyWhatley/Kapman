# Ship upgrades: what a yard can do for her (6 Oct 2026)

The user asked for "upgrades". The data is `Assets/Kapman/Game/Data/story/ship_upgrades_793.json`: 40 things that can be
fitted to a ship or done for her, each with its evidence, years, where it is had, cost, days, which hulls it fits, what
it changes, its upkeep and what takes it away. Fitting one sets the flag `unlock_upgrade_<id>` on that one ship.
A draft, for the user to overwrite. Every number is a starting guess to tune.

## How it plays

- **At a yard, a smithy, a workshop or a beach**, with the clock held: `act:fit_upgrade` lists what this place can do.
  A beach can tar her and cut brushwood; only a smith forges an anchor; walrus rope is had only in Norse ports before 890.
- **Prices are for the first boat** (7.7 m). Twice for 9 to 12 m, four times for 13 to 17 m, eight times above. Small
  things (a lead, a chest, a bailer) cost the same on any hull. Many take a traded good in place of silver: sailcloth,
  tar, lead, bar iron, hides. So an upgrade is also a reason to carry a cargo.
- **Small and many, not a ladder.** No fitting makes a boat a ship. Together they can give at most a fifth in speed,
  8 degrees toward the wind and a quarter off her fear of a gale. The way to a better ship is another ship (`ships_roster.md`).
- **They wear and they are lost.** Tar and moss are done again each year. A sail is old in eight years. A wreck takes
  nearly all of it. A few things go with the men to the next hull: the lead, the chest, the vane, the boat astern.
- **Nothing the period did not have.** No compass, no rudder on the sternpost, no chart, no pump, no crow's nest. The
  sunstone is sold as a tale and does nothing sure.

## Speed (8)

| Id | What | Cost (d), days | What it feels like |
|---|---|---|---|
| `new_sail` | a new sail of better wool | 120 or 6 sailcloth, 1 | every leg a little shorter; she holds 2 degrees higher. Old again in 8 years |
| `sail_dressed` | the sail rubbed with fat and ochre | 6, 2 | a small gain, and no speed lost on a wet day. Every second spring |
| `beitass` | a tacking spar (from 850, Norse ports) | 10, 0 | the big one against a head wind: 6 degrees closer, if a man is spared for it |
| `reef_bands` | reef bands on the sail | 12, 1 | a third choice in a rising wind: go on slowly under a small sail |
| `more_oars` | another pair of oars and two spare | 8, 1 | quicker in a calm, with two more men; a broken oar costs nothing |
| `tar_coat` | hauled out, scraped and tarred | 10, 3 | the yearly chore that pays: a clean hull, half the leaks |
| `tow_gear` | a tow mast and a long line | 15, 1 | upriver at a walking pace in place of a crawl |
| `rollers_tackle` | rollers and hauling tackle | 10, 0 | a portage on the chart opens; hauled above the tide in an hour |

## Safety (14)

| Id | What | Cost (d), days | What it feels like |
|---|---|---|---|
| `bailers` | two good bailers and a bucket | 2, 0 | the cheapest life-saver: two men bail at once |
| `ballast` | ballast stones | 1, 0 | a switch: steady when light, a tenth less room. Thrown out for a heavy cargo |
| `caulking_wool` | new caulking of tarred wool | 25, 6 | fewer leaks, one man less at the bailer. Six years |
| `moss_laths` | new moss, laths and clamps (river craft) | 8, 3 | the barge's yearly chore: grain on the bottom stays dry |
| `spare_steering_oar` | a spare steering oar and withy | 12, 2 | the worst card of a following sea becomes an hour's work |
| `sounding_line` | a lead armed with tallow | 18 or 2 lead and 6, 0 | fog and night near a coast lose a step of their danger; her place on the chart is surer |
| `second_killick` | a second stone anchor and a long cable | 8, 0 | one free answer to a dragging anchor |
| `iron_anchor` | an iron anchor in place of the stone | 100 or 4 bar iron and 60, 4 | she can lie off a beach and wait for a tide outside a bar |
| `walrus_rope` | shrouds and stay of walrus hide | 2 walrus rope, 1 | the mast stays in her; sail carried a little longer |
| `mast_step` | a stouter mast step and partner | 40, 8 | one more step of wind before the rig is at risk |
| `washstrake` | a strake added (12 m or less) | 200, a winter | the first boat grown: a fifth more cargo, drier, heavier to row |
| `oar_port_shutters` | shutters for the oar ports (from 850) | 10, 2 | a Norse ship sailed hard without filling |
| `ships_boat` | a boat towed astern (ships of 12 m and more) | 120, 0 | beach landings open; in a wreck the men likely live |
| `new_rivets` | new rivets and roves (a hull of 12 years or more) | a tenth of her price, 10 | ten more years of life, once |

## Cargo (8)

| Id | What | Cost (d), days | What it feels like |
|---|---|---|---|
| `dunnage` | brushwood under the cargo | 1, 0 | no bilge-water spoilage; stone and iron do the hull no harm. Each spring |
| `gangplank` | a gangplank | 3, 0 | loading in half the time, a porter less |
| `hold_cover` | greased hides over the hold | 20 or 2 hides, 1 | spray and rain spoil half as much: cloth, fur and grain become safe to carry |
| `half_deck` | decks fore and aft (from 990, ships) | 75, 10 | a fifth of the hold takes no harm at all |
| `lock_chest` | a lock chest | 45, 0 | thieves in harbour fail more often; letters and a charter survive a wreck |
| `stock_hurdles` | hurdles and a ramp for beasts | 6, 0 | sheep, cattle and horses can be carried |
| `river_cabin` | a cabin on a river ship | 60, 8 | a bishop or a count's man as a passenger, at double pay |
| `ice_chest` | a chest of cold (Truso, a reward only) | 60, 0 | one unit of fish or butter keeps twice as long, until midsummer |

## Crew (5)

| Id | What | Cost (d), days | What it feels like |
|---|---|---|---|
| `tilt` | a tent for the nights | 40 or 2 sailcloth, 1 | a night on a beach is as good as a lodging, and free |
| `kettle_hearth` | a kettle, a tripod and a hearth box | 24, 0 | hot food ashore and at anchor: morale holds, stores go further |
| `sea_chests` | a sea chest for each man | 5 a man, 0 | men trade a little for themselves, stay longer and ask less |
| `skin_bags` | skin sleeping bags | 4 for two men, 0 | the season is a week longer at each end |
| `water_casks` | water casks and a dipper | 8, 0 | six days at sea without landing, where she had two |

## Show (5)

| Id | What | Cost (d), days | What it feels like |
|---|---|---|---|
| `carved_stem` | a carved stem | 30, 6 | she has a face: known in every port, the crew prouder. A beast's head up in a Christian harbour is a cold greeting |
| `striped_sail` | a sail of two colours | 30, 5 | known from as far as she is seen, by friends and by those with a grudge |
| `shield_rail` | shields along the rail | 4 and the shields, 0 | she looks armed: a lone raider thinks twice, a toll man likes her less |
| `weather_vane` | a gilded vane (from 1000, great ships) | 480, 20 | a lord's mark: kings notice her, and so do thieves |
| `sunstone` | a sunstone (a tale) | 30, 0 | nothing sure. Men who believe in it are calmer in fog. A good gift |

Each of the show fittings cuts both ways on purpose: to be known is good in a port where the house is liked and bad on a
sea where it is hunted.

## Rewards of chains

Bought ones are bought. These can also come as a chain's or a card's pay, by the flag `unlock_upgrade_<id>`:

| Flag | Given by | State |
|---|---|---|
| `unlock_upgrade_walrus_rope` | the finale of the chain `walrus_ivory` | set today |
| `unlock_upgrade_ice_chest` | finale E2 of the chain `amber_road`, at Truso; had no other way | set today |
| `unlock_upgrade_new_sail` | the winter card `harbour.winter.weaving` (the house's own looms) | set today |
| `unlock_upgrade_washstrake` | the winter card `harbour.winter.rebuild` | set today |
| `unlock_upgrade_beitass` | shown by a Norse steersman after a voyage in his company | proposed |
| `unlock_upgrade_iron_anchor` | a smith's chain: Rhine iron carried north | proposed |
| `unlock_upgrade_sounding_line` | a pilot's parting gift, or taken from a wreck | proposed |
| `unlock_upgrade_ships_boat` | left to the house by a skipper it saved | proposed |
| `unlock_upgrade_lock_chest` | found at a chain's end with its key lost, and something in it | proposed |
| `unlock_upgrade_river_cabin` | built at a bishop's cost for the house that carries him | proposed |
| `unlock_upgrade_carved_stem` | a carver's thanks: the house's own beast | proposed |
| `unlock_upgrade_striped_sail` | colours granted by a lord or by sworn merchants | proposed |
| `unlock_upgrade_weather_vane` | a king's gift, after 1000 only | proposed |
| `unlock_upgrade_sunstone` | the small prize of a northern chain, given with a straight face | proposed |

## What is sure and what is not

- **Sure (read on 6 Oct 2026):** the Oseberg ship's iron anchor, bailer, gangplank and steering oar (buried 834); the
  Gokstad oar-hole shutters, tent and three small boats (timber felled about 890); the Soderala vane of gilt bronze,
  about 1050; sounding with a tallow-armed lead (antiquity, and the Bayeux Tapestry); the sunstone as a late tale only;
  Rimbert's two cabins (826); the Roskilde replicas' sail, dressing, caulking, rivets and tar (from `content/ships.md`).
- **From memory, marked (?) in the data:** the Gokstad shields, the Oseberg kettle, chests and mended mast partner, the
  Klaastad brushwood, the Mastermyr chest, the picture-stone sails, the Encomium Emmae, the Vorsaa rudder, the tar pits.
- **Guesses of mine, marked:** the year 850 for the tacking spar and the shutters, 990 for decks, reef bands (never
  found), a fire box aboard (no find: offered for harbour and anchor only), skin bags (the word is later).

## For whoever wires it

- `unlocks_793.json` and `town_actions_793.json` both say the fittings move to the new file. The nine old ids keep their
  meaning. Two prices differ and are noted in the data (`ships_boat`, `half_deck`).
- The engine reads only the flag today. The `effect.changes` are in the fields of `ship_classes_793.json` (`by` adds,
  `times` multiplies); the `plain` lines are rules still to be coded.
- `act:careen` should set `tar_coat` for the year.
