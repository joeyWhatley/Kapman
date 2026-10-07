# Kapman story schemas: the files the living world reads (6 Oct 2026)

Read `BRIEF.md`, `BRIEF_2.md` and `CHAINS.md` first: their rules hold here (period truth with sources, `(?)` for
what is not checked, plain wording, the Write tool for every file, the checker). The design these files serve is
`Design/living_world.md`. Where this page and a brief differ on a story file, this page wins.

| File | Holds | One file for |
|---|---|---|
| `Assets/Kapman/Game/Data/story/settlements/<region>.json` | the towns: people, bread, what helps, what opens | each trade region of `ports_793.json` (`rhine_delta`, `frisia`, `england_south` ...) |
| `Assets/Kapman/Game/Data/story/wars/<realm>.json` | who holds which port when; the real wars | each realm file of `Design/content/realms` (`denmark`, `england_south`, `francia`, `frisia_lotharingia`, `northumbria_york`, `norway`, `rus`, `sweden_gotland`, `wends_baltic`) |
| `Assets/Kapman/Game/Data/story/war_templates_793.json` | the shapes of wars the world makes for itself, and the grounds they need | the whole game |
| `Assets/Kapman/Game/Data/story/hinges/<age>.json` | real events a merchant can turn | each age id of `story/ages_793.json` (`age1` ... `age7`) |
| `Assets/Kapman/Game/Data/story/starts_793.json` | the ways a campaign can begin | the whole game |
| `Assets/Kapman/Game/Data/story/endings_793.json` | how the house and the world are told at the end | the whole game |
| `Assets/Kapman/Game/Data/story/storyteller_793.json` | the teller's numbers, its table, and the repeatable jobs | the whole game |

---

## 0. Rules for every file

1. **Shape.** Valid UTF-8 JSON, one-space or two-space indents, no trailing commas. The first key is `"about"`: one
   line saying what the file is, who wrote it and when. Every file ends with `"sources"`: full references, URLs where
   there are any, and `"needs"`: a list of plain lines for anything you needed that does not exist (an id, a chronicle
   entry, a port row). Do not add to the shared tables yourself.
2. **Ids** are lower-case letters, digits and underscores, and are those of the tables: ports of `ports_793.json`,
   goods of `goods_793.json`, realms of `ports_793.json` (plus those a wars file declares, section 2), roles and
   peoples of `encounters/npc_roles_793.json`, deeds of `reputation_793.json`, ship classes of
   `ship_classes_793.json`, work kinds of `story/works_793.json`, ages of `story/ages_793.json`, chronicle entries of
   `chronicle_793.json`, chains of `CHAINS_LIST.md`.
3. **Dates** are Julian, written `"793-06-21"`. A year alone is a number: `793`. `0` in `from` or `to` means "before
   the game" or "no end", as in `ports_793.json`.
4. **Every fact has a `source`.** A figure nobody has measured is given as a `low`, a `high` and a `best`, and the
   source says whose estimate it is. A guess of your own is marked `(?)` in its source.
5. **Words** in these files are plain text in the field (`text`, `name`, `then`), as in `works_793.json`, written as
   if they ship. A card that a story file names keeps its words in a writing file, as always.
6. **Conditions** (`when`, `turned`, `check`) are written in the grammar of `Cond.cs` and use only its names:
   `year`, `month`, `silver`, `debt`, `standing`, `standing_region`, `moored`, `port.id`, `port.kind`, `port.realm`,
   `hold.<good>`, `hold.value`, `hold.captives`, `hold.weapons`, `tongue.<name>`, `npc.<role>.feeling`,
   `text.<id>`, `chest.riddles_solved`, `generation`, `feast`, `flag.<name>` and the rest of its table. A word
   compares as there: `standing >= well_regarded`, `npc.reeve.feeling >= friendly`, `port.realm = franks`.
   **The state of the world is read as flags** (the table below). Do not invent a name.
7. **Effects** (`effects`) are the verbs of `Effects.cs` and nothing else: `silver`, `goods`, `health`, `morale`,
   `men`, `deed`, `news`, `price_shock`, `commission`, `give_text`, `give_riddle`, `open_fetch`, `hire`, `pay_off`,
   `passenger`, `encounter`, `law`, `learn`, `tongue`, `toll`, `flag`, `schedule`, `npc`, and the chance object.
   One form is new and is being added to the engine: `flag <name> +n` and `flag <name> -n` (add to a flag).
8. **World effects** (`world`) are a second list, beside `effects`, never mixed with it. They are what the ledgers
   do. Only these, one a string:

   | World effect | What it does |
   |---|---|
   | `people <port> +n` / `-n` | So many people come or come back (the wound falls; past nothing, the mark rises), or are lost. `+captives` lands every captive aboard as freed people of that port. `-n%` is a share of its heads. |
   | `mark <port> +f` / `-f` | What the house has added to the place for good, or cost it. Kept within -0.5 and +0.5. |
   | `store <port> +f` / `-f` | Months of bread in the place, 0 to 12. |
   | `heal <port>` | This year its wound heals at the helped rate (25 in 100, not 8). |
   | `state <port> <word> for <days>` | Sets a passing state: `besieged`, `sick`, `shut`, `warned`. |
   | `hold <port> <realm> [to <year>]` | The port is held by that realm from today. The next real row of a wars file's `holds` ends it. |
   | `tilt <port> +n` / `-n` | The house has helped (+) or hindered (-) the side that holds this port, in the war it is in. |
   | `war <template id> at <port>` | The teller starts a made war of that template with this port in its theatre, if the grounds allow. |
   | `job <job id> at <port>` | Posts that job there now. |
   | `open <port> <what>` / `close <port> <what>` | Sets or clears the flag `open_<port>_<what>`. |
   | `work <port> <work kind>` | The work is done there: its gift applies and `unlock_work_<port>_<kind>` is set. |
   | `drop <entry id>` | A later chronicle entry will not be told. |
   | `tell <entry id>` | A written `instead` entry is told today. |

   A port may be `here` (the port she lies at).
9. **Flags the world writes.** Read them; never set them. All are numbers; a bare flag is true when it is not 0.

   | Flag | Holds |
   |---|---|
   | `st_<port>_heads`, `st_<port>_households` | The town's people and households now |
   | `st_<port>_store` | Months of bread, rounded down |
   | `st_<port>_hungry` | 1 hungry (under two months of bread), 2 starving (none) |
   | `st_<port>_burned`, `st_<port>_sick`, `st_<port>_besieged`, `st_<port>_shut`, `st_<port>_warned`, `st_<port>_ruin` | 1 while it is so |
   | `st_<port>_trend` | Last year's change: 2 thriving, 1 growing, 0 steady, -1 thinning, -2 emptying |
   | `st_<port>_size` | Its market's size now, 0 to 7 |
   | `st_<port>_helped` | How many times this house has helped it (jobs kept there, works funded, warnings, people brought home) |
   | `st_<port>_war` | 1 while it lies in a war's theatre |
   | `here_heads`, `here_households`, `here_store`, `here_hungry`, `here_burned`, `here_sick`, `here_besieged`, `here_shut`, `here_warned`, `here_ruin`, `here_trend`, `here_size`, `here_helped`, `here_war` | The same, for the port she lies at; 0 at sea. Use these in cards written once for any port |
   | `here_phase` | The phase of the war this port is in: 1 muster, 2 a host or fleet moving, 3 siege, 4 sack or tribute, 5 peace; 0 none |
   | `here_tide` | That war's tide as the port's holder sees it, -3 (losing) to +3 (winning) |
   | `here_word_band`, `word_band_<port>` | 1 while the house carries word under five days old of a band or fleet bound for the port |
   | `news_<port>_days` | The age in days of the freshest word the house has of the port; 999 when it has none |
   | `war_<war id>` | The war's phase number while it runs; 0 before and after |
   | `war_<war id>_tide`, `war_<war id>_tilt` | Its tide, -3 to +3 for side a; what the house has done for side a (+) or side b (-) |
   | `age` | 1 to 7: the age of `ages_793.json` the year lies in |
   | `house_keels`, `house_plots`, `house_factors`, `house_crossings`, `house_stage` | The house's ships, plots, factors, sea crossings in a ship of its own, and its stage 1 to 5 (`research_scale_to_fleets.md`) |
   | `promises` | Jobs and orders the house has open, 0 to 3 |

10. **Flags writers set**, by the names of `CHAINS.md`: `ch_<id>_<n>`, `ch_<id>_end_<name>`, `unlock_<kind>_<what>`,
    `changed_<event>`. Added here: `open_<port>_<what>` (a threshold reached: set by the ledgers through `opens`),
    `st_end_<port>_<name>` (how a place ended), `war_<war id>_end_<name>` (how a war ended), `ending_<id>` (the house's
    ending), `start_<id>` (the start chosen). Letters, digits and underscores only.
11. **The checker.** Run it as `BRIEF_2.md` says. Today it only checks that a file in `story/` parses; it does not
    look inside `story/settlements`, `story/wars` or `story/hinges` yet. So check your own ids against the tables
    before you report, and say in your report which facts you could not check.

---

## 1. Settlements: `story/settlements/<region>.json`

One file a trade region, one entry for each port of that region in `ports_793.json`. The game works a town out as

    heads = line(year) x (1 + mark) - wound        households = heads / household.size

The **line** is history's own course for the town, read from `people`. The writer gives the line and what bends it;
the game keeps `wound`, `mark` and `store`.

```json
{
 "about": "THE TOWNS OF THE RHINE DELTA, 793-1066: people, bread, what helps and what opens. Worked example for SCHEMAS.md, 6 Oct 2026. Figures are estimates: (?) marks our own guesses.",
 "region": "rhine_delta",
 "settlements": [
  {
   "port": "dorestad",
   "people": [
    { "age": "age1", "year": 793, "low": 2000, "high": 3000, "best": 2500, "source": "Design/content/dossiers/dorestad.md [N, T9]: an estimated 2,000 to 3,000 souls", "note": "In the graves two men to each woman: a town of shippers and incomers." },
    { "age": "age2", "year": 834, "low": 1800, "high": 3000, "best": 2400, "source": "our guess: the town before the first sack (?)" },
    { "age": "age2", "year": 838, "low": 1000, "high": 2000, "best": 1500, "source": "our guess after four sacks in four years, AB 834 to 837 (?)" },
    { "age": "age2", "year": 850, "low": 800, "high": 1600, "best": 1200, "source": "our guess: burned 846 (Ann. Xant.), held by Rorik from 850 (AB) (?)" },
    { "age": "age2", "year": 864, "low": 200, "high": 700, "best": 400, "source": "our guess: last named as a trading place in 863 (AB) (?)" },
    { "age": "age3", "year": 865, "low": 100, "high": 500, "best": 300, "source": "our guess (?)" },
    { "age": "age4", "year": 900, "low": 0, "high": 200, "best": 100, "source": "our guess: a farm village; a charter of Otto I in 948 speaks of the place once called Dorestad and now Wik (?)" },
    { "age": "age5", "year": 955, "low": 50, "high": 150, "best": 80, "source": "our guess (?)" },
    { "age": "age6", "year": 991, "low": 50, "high": 150, "best": 80, "source": "our guess (?)" },
    { "age": "age7", "year": 1042, "low": 50, "high": 150, "best": 80, "source": "our guess (?)" }
   ],
   "household": { "size": 6, "low": 5, "high": 8, "source": "Design/content/dossiers/medemelacha.md [W61 6.3]: households of six to ten with a grandparent or servants; fewer here, where many are single men (?)" },
   "bread": {
    "by_keel": 0.25, "after_harvest": 9,
    "eats": ["rye", "barley", "wheat", "oats", "flour"],
    "source": "our guess: the farms of the Kromme Rijn bank fed most of it; a quarter came down the river and from the Betuwe by boat (?)"
   },
   "makes": [
    { "good": "combs", "a_year": 400, "season": "all year", "source": "bone and antler working in the harbour quarter: van Es and Verwers, Excavations at Dorestad 1 (1980) (?)" },
    { "good": "iron_tools", "a_year": 120, "season": "all year", "source": "smithing waste on the plots (?)" }
   ],
   "needs": [
    { "good": "rye", "a_year": 2600, "why": "bread for a quarter of the town", "source": "0.7 kg a head a day (voyage_rules.json), 60 kg a sack" },
    { "good": "oak_timber", "a_year": 150, "why": "the jetties are lengthened as the river moves east", "source": "the harbour's causeways, dendro-dated to c. 675 to 825 (?)" },
    { "good": "antler", "a_year": 200, "why": "the comb makers", "source": "as under makes" },
    { "good": "sea_salt", "a_year": 300, "why": "meat and fish for winter", "source": "our guess (?)" }
   ],
   "stores": [
    { "good": "rhenish_wine", "months": 4, "where": "dorestad.quay", "note": "Casks from Mainz and Cologne wait here for the sea crossing.", "source": "wine barrels reused as well linings at Dorestad, the oak from the Mainz region" },
    { "good": "quernstones", "months": 8, "where": "dorestad.quay", "note": "Mayen lava querns, roughed out up the river and finished here.", "source": "Parkhouse 1997 (?)" }
   ],
   "grows": [
    { "cause": "calls", "by": 0.01, "note": "The toll and the mint draw every ship of the Rhine and the coast.", "source": "the charter of 777; Design/content/dossiers/dorestad.md" },
    { "cause": "works", "by": 0.002, "note": "Each town work standing, to five.", "source": "living_world.md 1" }
   ],
   "fails": [
    { "cause": "raid", "years": [834, 835, 836, 837, 863], "turns": true, "source": "AB 834 to 837, 863" },
    { "cause": "burning", "years": [846], "turns": true, "source": "Ann. Xant. 846; AB 846" },
    { "cause": "silting", "years": [850, 900], "turns": false, "note": "The Kromme Rijn leaves the town; the trade goes to Tiel and Daventre.", "source": "Design/content/dossiers/dorestad.md" },
    { "cause": "flood", "years": [838, 864], "turns": false, "source": "AB 839 (the flood of 26 December 838); a flood about 864 is told in later writers (?)" }
   ],
   "fixed": [
    { "what": "The river leaves Dorestad", "from": 850, "to": 900, "text": "The old Rhine is a ditch a man can wade. No wall and no silver brings the water back.", "source": "Design/content/dossiers/dorestad.md" }
   ],
   "help": [
    {
     "id": "dorestad.help.feed", "by": "sale", "what": "Sell grain or flour here while the town is hungry.",
     "when": "flag.st_dorestad_hungry >= 1", "cost": "", "effects": [], "world": [],
     "then": "The store rises by what is landed (the game reckons it from bread.by_keel). The dear price is the pay."
    },
    {
     "id": "dorestad.help.warn", "by": "card", "card": "harbour.town.warn_reeve", "what": "Tell the count's man that a fleet is coming.",
     "when": "flag.here_word_band and not flag.st_dorestad_warned", "cost": "",
     "effects": ["npc reeve feeling +10 warned"], "world": ["state dorestad warned for 10"],
     "then": "The stock is hidden and the boats are sent up the river. A raid within ten days takes no captives and half as much."
    },
    {
     "id": "dorestad.help.mend", "by": "job", "job": "mend_burned", "what": "Bring oak, iron and tar after a burning.",
     "when": "flag.st_dorestad_burned", "cost": "", "effects": [], "world": [],
     "then": "As the job mend_burned says: the town heals at 25 in 100 that year."
    },
    {
     "id": "dorestad.help.wall", "by": "work", "work": "dorestad.work.wall_share", "what": "Pay for a stretch of ditch and bank, and men to watch it.",
     "when": "standing >= known and year >= 834", "cost": "silver 500",
     "effects": ["deed gave_alms"], "world": ["work dorestad wall_share", "mark dorestad +0.05"],
     "gift": { "defence": 60, "raid_takes_x": 0.5 },
     "then": "A band must be stronger to try the town. A stretch paid each year for three years counts as the wall of the hinge dorestad_863."
    },
    {
     "id": "dorestad.help.ransom", "by": "card", "card": "harbour.town.bring_home", "what": "Bring its people home from the raiders' market.",
     "when": "hold.captives > 0 and flag.st_dorestad_heads < 2000", "cost": "",
     "effects": ["deed bought_captives", "npc priest feeling +15 ransomed"], "world": ["people dorestad +captives"],
     "then": "They are set down free. Kin pay what they can; the church pays for those with none."
    }
   ],
   "asks": [
    { "job": "feed_hungry", "when": "flag.st_dorestad_hungry >= 1", "goods": ["rye", "barley", "flour"], "units": [20, 60], "premium": [0.4, 0.8], "days": [20, 40] },
    { "job": "mend_burned", "when": "flag.st_dorestad_burned", "goods": ["oak_timber", "bar_iron", "tar"], "units": [6, 20], "premium": [0.3, 0.5], "days": [30, 60] }
   ],
   "opens": [
    {
     "id": "dorestad.open.great_ships", "what": "great_ships",
     "when": "flag.st_dorestad_households >= 400 and flag.unlock_work_dorestad_slip",
     "opens": ["ship:north_sea_merchant_ship", "act:build_ship@dorestad.yard"], "keeps": false,
     "text": "The wrights of the bank will lay the keel of a sea-going trader, if the oak comes.",
     "lost_text": "The wrights have gone down the river to Tiel. Nobody here builds more than a boat now.",
     "source": "the Utrecht-type hulls of the delta; ships.md (?)"
    },
    {
     "id": "dorestad.open.mint_closed", "what": "mint_closed",
     "when": "flag.st_dorestad_households < 150", "opens": [], "closes": ["act:change_coin@dorestad.mint", "site:dorestad.mint"], "keeps": true,
     "text": "The moneyers have gone. Old pennies are silver by weight here now.",
     "source": "the last Dorestad pennies are Lothar's, about 850 (?)"
    }
   ],
   "endings": [
    {
     "id": "toll_town", "at": 864, "real": false, "when": "flag.changed_dorestad_863",
     "text": "The Danes did not come a last time. The river went all the same, a hand's breadth a year, and Dorestat grew small behind its bank: a toll, a church, a ferry, and men who remembered whose silver had paid for the wall.",
     "world": ["mark dorestad +0.1"], "effects": ["flag st_end_dorestad_toll_town"],
     "picture": "A low earth bank and a gate above a narrow river; one boat at a short jetty; a wooden church behind."
    },
    {
     "id": "moved", "at": 864, "real": false, "when": "not flag.changed_dorestad_863 and flag.st_dorestad_heads >= 600 and flag.st_dorestad_helped >= 3",
     "text": "Before the last fleet came the people of the bank had gone, with their looms, their scales and their dead men's names, to Daventre and to the bishop's town. The Danes burned empty houses.",
     "world": ["people daventre +300", "people traiectum +200"], "effects": ["flag st_end_dorestad_moved"],
     "picture": "River barges piled with household goods going down a grey river in rain; smoke far astern."
    },
    {
     "id": "ashes", "at": 864, "real": true, "when": "true",
     "text": "This year the heathen men came to Dorestat once more and went on up the Rhine. After this it is not named as a place of trade.",
     "world": [], "effects": ["flag st_end_dorestad_ashes"],
     "picture": "Burned jetty posts standing in mud at low water; reeds; no boats."
    }
   ],
   "sources": [
    "Design/content/dossiers/dorestad.md and its references",
    "Annales Bertiniani 834 to 837, 846, 850, 863; Annales Xantenses 834, 846"
   ]
  }
 ],
 "sources": ["as under each settlement"],
 "needs": ["A town work 'granary' in story/works_793.json (a common store of grain): the table has the house's storehouse only."]
}
```

### The fields

| Field | Rule |
|---|---|
| `region` | The trade region, as in `ports_793.json`. Every port of the region has one entry in `settlements`, in the table's order. |
| `port` | A port id. |
| `people` | The line. **The first row is 793** (or the port's first year, if it begins later) and must give `low`, `high`, `best` and a `source`. Then **one row at least for each age the port exists in**, with `age` (an id of `ages_793.json`) and a `year` inside that age, and more rows at the sharp turns (the year before and after a sack, a move, a founding). Years rise down the list. Between two rows the game draws a straight line; after the last row the figure holds. `best` lies within `low` and `high`. A port that ends (`to` in the ports table) needs a row for its last year and what is left after. These are **people in the place**, not its district: monks, lay brothers and servants for a minster; the wic, not the shire. |
| `household` | `size` is the people to a household (5 to 8; a minster may use 1 with a note); `low`, `high`, `source`. The game shows households as heads / size. |
| `bread` | `by_keel`: the share of the town's bread that comes by water in a common year (a village 0.05 to 0.1, a town 0.1 to 0.2, an emporium 0.2 to 0.35, an island minster up to 0.5). It decides how much one cargo helps: months of bread from a cargo = `sacks x 60 / (heads x 21 x by_keel)`. `after_harvest`: months of bread in store after a common harvest (6 to 10). `eats`: the goods that count as its bread, from the Food goods. |
| `makes`, `needs` | What the place itself makes and lacks: `good`, `a_year` (the good's own units a year in 793, a guess to tune), `season` or `why`, `source`. They must agree with what the port's region makes and wants in `goods_793.json`; they add the place's own detail. Three to eight each for a town, one to three for a landing. |
| `stores` | What lies here between seasons: `good`, `months` (how long a common stock lasts), `where` (a site id of the port's places file, or empty), `note`, `source`. |
| `grows` | What makes it grow beyond its line: `cause` is one of `calls`, `works`, `fair`, `mint`, `see`, `court`, `peace`, `incomers`, `trade_moved_here`; `by` is the share of its heads a year the cause can add at the most (0.002 to 0.02); `note`, `source`. Leave a cause out to take the game's default. |
| `fails` | What made it fail in history: `cause` is one of `raid`, `burning`, `famine`, `epidemic`, `flood`, `siege`, `silting`, `tribute`, `lord_gone`, `trade_moved_away`, `toll_raised`; `years` (the years it struck, or a `[from, to]` pair for a slow cause); `turns`: true if a merchant could bear on it (then a hinge or a help row should exist), false if not; `source`. |
| `fixed` | What no merchant can turn, said plainly, so the game can say it: `what`, `from`, `to`, `text` (as a person there would say it), `source`. |
| `help` | **What a player can do for it, and what follows.** `id` is `<port>.help.<what>`. `by` is one of `sale` (it happens by trading; no card), `job` (a job id of section 5 in `job`), `card` (an encounter card id in `card`: say in `needs` if the card is not written), `work` (a work id of the port's places file in `work`), `act` (an act of `BRIEF_2.md` in `act`). `when`: when it is open. `cost`: the engine's cost words, or empty. `effects` and `world`: what it does (rules 7 and 8). `gift`: for a work, what it gives the town, from this list only: `defence` (fighting men added), `raid_takes_x` (a factor on what a raid takes), `warning_days`, `sickness_x`, `carry_store` (months of bread kept over a bad harvest), `berths`. `then`: one or two plain sentences of what follows. Three to six rows a town, one or two a landing. At least one row costs nothing but time. |
| `asks` | The jobs this town posts: `job` (a job id of section 5), `when`, and the town's own `goods`, `units` `[low, high]`, `premium` `[low, high]` (the share over the market), `days` `[low, high]`. |
| `opens` | **Thresholds.** `id` is `<port>.open.<what>`; `what` names the flag `open_<port>_<what>`. `when`: the condition, on the town's flags and any other. `opens` and `closes`: lists of `site:<site id>`, `work:<work id>`, `ship:<class id>`, `act:<act>@<site id>`, `job:<job id>`, `card:<card id>`. `keeps`: true if it stays once reached; false if it is lost when the condition fails (then give `lost_text`). `text`: one line said when it happens. `source`: why this size, this town. Two to five for a town; a landing may have none. Use round household counts a player can aim at. |
| `endings` | **Three at least**, tried in order at the year `at`; the first whose `when` holds is told. The last has `"when": "true"` and `"real": true`: what happened. `text` is in the Chronicle's voice, under 60 words. `effects` set `st_end_<port>_<id>`. `world` may move people. `picture`: one line for the artist. A port that lives to 1066 has `"at": 1066`. |

---

## 2. Holds and the real wars: `story/wars/<realm>.json`

One file for each realm file of `Design/content/realms`, built from it and from the chronicle. Three lists.

```json
{
 "about": "FRISIA AND LOTHARINGIA, 793-1066: who holds its ports, and its wars. Worked example for SCHEMAS.md, 6 Oct 2026.",
 "realm": "frisia_lotharingia",
 "realms": [
  { "id": "rorik", "name": "Rorik's lordship", "under": "franks", "people": "danish", "faith": "old_gods", "from": 850, "to": 873,
    "toll": "as franks", "law": "as franks", "note": "A Danish lord holding Frankish counties for the emperor: the king's dues are still taken.", "source": "AB 850; Ann. Fuld. 850, 857, 873" }
 ],
 "holds": [
  { "port": "dorestad", "realm": "rorik", "lord": "Rorik", "historical": true, "from": "850-07-01", "to": "873-12-31", "entry": "850-rorik-dorestad",
    "note": "He is to pay the king's dues and keep pirates off.", "source": "AB 850; Ann. Fuld. 850" },
  { "port": "witla", "realm": "rorik", "lord": "Rorik", "historical": true, "from": "850-07-01", "to": "873-12-31", "entry": "850-rorik-dorestad",
    "note": "Among the 'other counties' (?).", "source": "AB 850 (?)" }
 ],
 "wars": [
  {
   "id": "delta_834",
   "name": "The sacks of Dorestad and the delta",
   "real": true,
   "from": "834-07-01", "to": "863-12-31",
   "sides": [
    { "side": "a", "realm": "danes", "who": "Danish fleets, at first while the emperor's sons fight him", "bands_from": ["danes"] },
    { "side": "b", "realm": "franks", "who": "The counts of the coast, then Rorik for the emperor", "bands_from": [] }
   ],
   "theatre": ["dorestad", "traiectum", "witla", "walichrum", "andoverpis", "noviomagus"],
   "cause": "A rich market with no wall, and a realm at war with itself.",
   "levers": { "bands": [2, 4], "dear": { "goods": "food,horses,oak_timber", "x": 1.3 }, "patrols_x": 2.0, "levy": true },
   "phases": [
    { "n": 1, "kind": "blow", "on": "834-07-01", "entry": "834-dorestad-sacked", "ports": ["dorestad", "traiectum"], "tide": 1,
      "jobs": ["ransom_captives", "mend_burned"], "text": "A fleet comes by Utrecht to Dorestat and wastes it.", "source": "AB 834; Ann. Xant. 834" },
    { "n": 2, "kind": "blow", "on": "835-06-15", "entry": "835-dorestad-again", "ports": ["dorestad"], "tide": 1,
      "jobs": ["ransom_captives"], "text": "They come again.", "source": "AB 835" },
    { "n": 3, "kind": "burning", "on": "836-07-01", "entry": "836-witla-antwerp-burned", "ports": ["witla", "andoverpis", "dorestad"], "tide": 2,
      "jobs": ["mend_burned", "carry_refuge"], "text": "Antwerp and Witla are burned, and Dorestat is raided a third time.", "source": "AB 836" },
    { "n": 4, "kind": "tribute", "on": "837-06-17", "entry": "837-06-17-walcheren-raid", "ports": ["walichrum", "dorestad"], "tide": 2,
      "jobs": ["ransom_captives", "carry_envoy"], "text": "The count is killed on Walcheren and tribute is taken from Dorestat. The emperor holds an inquiry at Nijmegen.", "source": "AB 837" },
    { "n": 5, "kind": "burning", "on": "846-07-01", "entry": "846-dorestad-burned", "ports": ["dorestad", "noviomagus"], "tide": 2,
      "jobs": ["mend_burned"], "text": "Dorestat is burned while the emperor looks on from Nijmegen.", "source": "Ann. Xant. 846; AB 846" },
    { "n": 6, "kind": "peace", "on": "850-07-01", "entry": "850-rorik-dorestad", "ports": ["dorestad"], "tide": 3,
      "jobs": [], "text": "Lothar gives Dorestat to Rorik, who had taken it.", "source": "AB 850" },
    { "n": 7, "kind": "blow", "on": "863-03-01", "entry": "863-dorestad-last", "ports": ["dorestad", "colonia"], "tide": 3,
      "jobs": ["carry_refuge"], "text": "The last sack. The fleet goes on up the Rhine.", "source": "AB 863" }
   ],
   "plays": ["supply", "ransom", "refuge", "envoy", "profit", "ruin"],
   "chains": ["dorestad_834", "rorik_850"],
   "hinges": ["dorestad_863"],
   "endings": [
    { "id": "held", "real": false, "when": "flag.changed_dorestad_863", "text": "The delta is held. The fleets go to the Seine.", "effects": ["flag war_delta_834_end_held"], "world": [] },
    { "id": "lost", "real": true, "when": "true", "text": "Dorestat is not named again. The trade goes to Tiel and Daventre.", "effects": ["flag war_delta_834_end_lost"], "world": [] }
   ],
   "what_it_did_to_trade": "Thirty years in which the richest market of the north is the most dangerous berth on the Rhine: stock kept short, silver buried, and the careful houses moving up the river.",
   "sources": ["Annales Bertiniani 834 to 863", "Annales Xantenses 834, 846", "Design/content/realms/frisia_lotharingia.md"]
  }
 ],
 "sources": ["as under each entry"],
 "needs": ["The realm id 'rorik' in encounters/npc_roles_793.json 'realms', so cards may say port.realm = rorik."]
}
```

### The fields

| Field | Rule |
|---|---|
| `realm` | The file's name, as in `Design/content/realms`. |
| `realms` | Realm ids this file adds to the 17 of `ports_793.json`, for lordships that begin after 793 (Rorik's, the Danes at York, Rollo's Rouen, one English kingdom, the Rus of Kiev). `id`, `name`, `under` (the realm whose king they answer to, or empty), `people`, `faith` (as in the roles table and the spine), `from`, `to`, `toll` and `law`: "as <realm id>" or one plain line, `source`. Use an existing id wherever one fits. |
| `holds` | **Who holds a port, by date.** One row each time a port of this file's lands changes hands: `port`, `realm`, `lord` (his name) with `historical` true or false, `from`, `to` (a date; empty for "until the next row"), `entry` (the chronicle entry that tells it, or empty), `note`, `source`. In 793 a port is held as `ports_793.json` says: write rows only for changes. `port.realm` reads this table. Only real changes: a made war writes its own, and yours always win. |
| `wars[].id` | Short and dated: `<where>_<first year>`. Unique across all wars files. A war between two realms is written once, in the file of side a. |
| `real` | Always true in these files. |
| `from`, `to` | Its first and last day. A long run of raids is one war if a merchant of the time would have called it one trouble. |
| `sides` | Two: `side` a and b, `realm`, `who` (plain), and `bands_from`: origin ids of `raiders_793.json` whose bands sail for that side (may be empty). Side a is the one that begins it. |
| `theatre` | The ports it touches, by id: where prices, king's men and bands change while it runs. Three to eight. |
| `cause` | One plain line. |
| `levers` | `bands`: `[low, high]` bands a season sent at the other side's theatre ports. `dear`: `goods` (ids joined by commas, or `food`) and `x`, the factor on their price in the theatre. `patrols_x`: how much stricter the king's men are. `levy`: true if a lord may call up the house's hull (a warning card always comes first). |
| `phases` | **In date order, each bound to the chronicle.** `n` from 1. `kind`: one of `muster`, `moving`, `blow`, `burning`, `siege`, `tribute`, `battle`, `peace`, `lull`. `on`: its day. `entry`: the chronicle entry it is (its `fire` day must be `on`); if the chronicle has none, leave `entry` empty, give the `source`, and list the missing entry under `needs`. `ports`: where it falls. `tide`: where history puts the tide after it, -3 to +3 for side a. `jobs`: job ids (section 5) the phase posts at its ports. `until` (for a siege): its last day. `text`: one line, as news. A battle is a day with an outcome: it comes as news and moves the tide; nothing is fought on screen. |
| `plays` | Which of the seven plays its cards may offer here: `supply`, `blockade`, `envoy`, `ransom`, `refuge`, `profit`, `ruin`. Leave out what is not true of this war (no blockade where no river was shut). |
| `chains`, `hinges` | Chain ids of `CHAINS_LIST.md` and hinge ids of section 4 that belong to it. |
| `endings` | Two at least, tried in order on the day `to`; the last is `"real": true, "when": "true"`. Each sets `war_<id>_end_<name>` and may carry `world` effects (a `hold`). |
| `what_it_did_to_trade` | Two sentences a merchant would say. The realm files already have this line for many wars: use it. |

---

## 3. Wars the world makes: `story/war_templates_793.json`

A template is the shape of a small war. The teller fills it in: a ground (who, where, which years), ports, invented
leaders, days. **A made war never names a king, never kills or moves a real person, never changes a crown, and is
shown as news, not among the annals.** Its leaders are invented men with attested names (`names_placeholder.json`).

In a template's conditions and effects, write `{war}` for the made war's id (the teller makes one: `m1`, `m2` ...),
`{port}` for the port a phase falls on, `{a}` and `{b}` for the two realms. After they are filled in, every condition
is plain `Cond.cs` and every effect plain `Effects.cs`.

```json
{
 "about": "WAR TEMPLATES, 793-1066: the shapes of the small wars the world makes for itself, and the grounds they need. Worked example for SCHEMAS.md, 6 Oct 2026. Numbers are guesses to tune.",
 "templates": [
  {
   "id": "sea_kings_summer",
   "name": "A sea-king's summer",
   "what": "A chief with a few ships takes an island or a river mouth for a season, raids from it, and is paid off, beaten or stays the winter.",
   "from": 834, "to": 1066, "months": [5, 6, 7, 8, 9],
   "sides": { "a": { "kind": "band", "leader_role": "raider_chief", "ships": [2, 6] }, "b": { "kind": "holder", "leader_role": "count" } },
   "theatre": { "ports": [2, 4], "within_km": 200, "kinds": ["emporium", "market", "town", "monastery", "village"] },
   "levers": { "bands": [2, 6], "dear": { "goods": "food", "x": 1.25 }, "patrols_x": 1.5, "levy": false },
   "phases": [
    { "n": 1, "kind": "moving", "after_days": [0, 0], "heat": 2, "jobs": ["find_out_fleet"],
      "text": "Ships have been seen off {port}: more than a trader's company, and no cargo." },
    { "n": 2, "kind": "blow", "after_days": [10, 30], "heat": 3, "draw": true, "jobs": ["ransom_captives", "carry_refuge"],
      "win": { "world": ["people {port} -3%", "store {port} -2"], "tide": 1, "text": "They have struck {port} and carried people off." },
      "lose": { "world": [], "tide": -1, "text": "They came to {port} and found the bank manned. They went off with nothing." } },
    { "n": 3, "kind": "siege", "after_days": [20, 40], "heat": 3, "draw": true, "lasts_days": [30, 90], "when": "flag.war_{war}_tide >= 1", "jobs": ["feed_besieged", "carry_envoy"],
      "win": { "world": ["state {port} besieged for 60"], "tide": 1, "text": "They lie in the river below {port} and let nothing by." },
      "lose": { "world": [], "tide": -1, "text": "The count's men have burned two of their ships on the beach." } },
    { "n": 4, "kind": "peace", "after_days": [30, 60], "heat": 1, "jobs": [],
      "text": "Word is that it is over, one way or the other." }
   ],
   "plays": ["supply", "blockade", "ransom", "refuge", "profit", "ruin"],
   "endings": [
    { "id": "paid_off", "when": "flag.war_{war}_tide >= 2", "text": "{b} has paid them silver to go, and they have gone, for this year.",
      "effects": ["flag war_{war}_end_paid_off"], "world": ["store {port} -1"] },
    { "id": "beaten", "when": "flag.war_{war}_tide <= -2", "text": "They were caught at the river mouth on a falling tide. Few got home.",
      "effects": ["flag war_{war}_end_beaten"], "world": [] },
    { "id": "wintered", "when": "true", "text": "They have built huts on the island and mean to stay the winter.",
      "effects": ["flag war_{war}_end_wintered"], "world": [] }
   ],
   "precedent": "Noirmoutier as a winter camp from 843; Sheppey 855; Walcheren 837; Asselt 882.",
   "source": "AB 843; ASC 855; AB 837; Ann. Fuld. 882"
  }
 ],
 "grounds": [
  { "id": "danes_frisian_coast", "a": "danes", "b": "franks", "from": 834, "to": 885, "templates": ["sea_kings_summer"],
    "ports": ["walichrum", "witla", "dorestad", "medemelacha", "wijnaldum", "emuthon", "andoverpis"],
    "why": "Danish fleets on the Frisian coast almost yearly; the coast guard failed again and again.", "source": "AB 834 to 863; Ann. Fuld. 882, 885" }
 ],
 "sources": ["as under each entry"],
 "needs": []
}
```

### The fields

| Field | Rule |
|---|---|
| `templates[].id`, `name`, `what` | A plain name and two sentences. Six templates are wanted: `sea_kings_summer`, `two_lords_feud` (a toll taken twice), `border_and_tribute`, `rising`, `siege_of_one_town`, `levy_year`. |
| `from`, `to`, `months` | The years the shape is true to, and the months it may begin in. |
| `sides` | For a and b: `kind` is `band` (ships from a raiders' origin), `holder` (the lord who holds the theatre's ports that year), `claimant` (an invented kinsman of a lord), `townsmen`; `leader_role`: a role of the roles table for the invented leader (`raider_chief`, `count`, `reeve`, `chief`, `commander`); `ships` for a band. |
| `theatre` | How the teller picks ports: `ports` `[low, high]`, `within_km` of the first, and the port `kinds` it may fall on. It takes them from the ground's `ports`, and only ports the house has news of. |
| `levers` | As in section 2. |
| `phases` | `n`; `kind` (the list of section 2); `after_days` `[low, high]` after the phase before; `heat` 0 to 3 for the teller; `when` (optional: the phase is skipped unless it holds); `jobs`; and either `text` (it simply happens) or `"draw": true` with `win` and `lose` (side a wins the draw with `p = sA / (sA + sB) + 0.07 x tilt`). `win` and `lose` each have `world`, `tide` (what is added) and `text`. Texts are news lines, under 30 words, with the slots only. Give each phase **three variants** as `text`, `text_2`, `text_3` once the first is right, so a dozen made wars do not read alike. |
| `endings` | Three at least, tried in order when the tide reaches 3 or -3 or the last phase is done; the last has `"when": "true"`. A `hold {port} {a}` is allowed only where the ground says `"may_change_hands": true`, and only between lords below a king. |
| `precedent`, `source` | The real events the shape is drawn from. A template without a real precedent is not wanted. |
| `grounds` | **Where and when a made war is allowed.** `a` and `b`: realm ids that really were at odds in `from` to `to`; `templates`: which shapes fit; `ports`: where it may fall; `may_change_hands` (optional, default false); `why`; `source`. No ground, no war: the teller never sets two realms at each other without one. Twenty to forty are wanted, most after 825, drawn from the realm files' `wars` and `raids`. |

---

## 4. Hinges: `story/hinges/<age>.json`

A hinge is a real entry of the chronicle with a test. On the entry's day the game asks `turned`. If it holds, the
entry is not told and what you write here happens instead. One file for each age (`age1.json` ... `age7.json`), a
hinge in the file of the age its entry falls in.

```json
{
 "about": "HINGES OF THE RAIDING YEARS (age2, 834-864): real events a merchant can turn, and what follows. Worked example for SCHEMAS.md, 6 Oct 2026.",
 "age": "age2",
 "hinges": [
  {
   "id": "dorestad_863",
   "name": "The last sack of Dorestat",
   "class": "region",
   "entry": "863-dorestad-last",
   "on": "863-03-01",
   "really": "In 863 a Danish fleet sacked Dorestat once more and went up the Rhine towards Cologne. The place is not named again as a market; its trade went to Tiel and Deventer.",
   "sources": ["Annales Bertiniani 863", "Design/content/dossiers/dorestad.md"],
   "could_a_merchant": "Yes, in part. The town had no wall, its lord Rorik was away as often as not, and its people were leaving. Silver for a bank and a watch, a town still worth defending, and the lord's friendship are each things a house could bring. The river is not.",
   "herald": { "days_before": 400, "card": "harbour.hinge.dorestad_863_herald", "news": "Men at Dorestat say the bank ought to be ditched before another fleet comes, and ask who will pay." },
   "turned": "(flag.unlock_work_dorestad_wall_share and flag.st_dorestad_heads >= 1200) or (flag.unlock_work_dorestad_wall_share and flag.ch_rorik_850_end_his_man) or (flag.st_dorestad_heads >= 1200 and flag.ch_rorik_850_end_his_man)",
   "turned_words": "Two of three: the wall share and watch paid for; 1,200 people or more in the town; Rorik's man a friend of the house.",
   "softens": [
    { "entry": "846-dorestad-burned", "when": "flag.st_dorestad_warned or flag.unlock_work_dorestad_wall_share", "wound_x": 0.5, "text": "The stretch by the church held, and the boats had gone up the river the night before." }
   ],
   "instead": [
    {
     "id": "863-dorestad-stands", "date": "863", "precision": "month", "fire": "863-03-01", "kind": "raid",
     "title": "A Danish fleet comes up the Lek, finds Dorestat ditched and watched, and goes on up the Rhine",
     "place": "Dorestad", "ports": ["colonia"], "regions": ["rhine_delta", "rhineland"],
     "certainty": "otherwise", "effect": "no raid on dorestad; the Rhine above it is raided as the annals tell",
     "note": "Otherwise than the annals tell. In AB 863 Dorestat is sacked first.",
     "sources": ["this telling; against Annales Bertiniani 863"]
    },
    {
     "id": "870-dorestad-toll-kept", "date": "870", "precision": "year", "fire": "870-07-01", "kind": "trade",
     "title": "The king's toll is still taken at Dorestat, though the great ships now lie at Tiel",
     "place": "Dorestad", "ports": ["dorestad"], "regions": ["rhine_delta"],
     "certainty": "otherwise", "effect": "dorestad trades on as a small toll town",
     "note": "Otherwise than the annals tell.",
     "sources": ["this telling"]
    }
   ],
   "drops": [],
   "holds": [],
   "line": { "port": "dorestad", "people": [ { "year": 864, "best": 1100 }, { "year": 900, "best": 700 }, { "year": 955, "best": 500 }, { "year": 1066, "best": 500 } ],
             "to": 0, "size_cap": 3, "note": "The river still leaves it: a toll, a church and a ferry, not the great market." },
   "effects": ["flag changed_dorestad_863", "news hinge.dorestad_863.turned at traiectum"],
   "world": ["mark dorestad +0.1"],
   "later": [
    { "year": 885, "what": "Godfrid's fall (885-06-godfrid-killed) is told as it was; Dorestat is named in it as a place still standing." },
    { "year": 1006, "what": "The fleet that burns Tiel (1006-tiel-burned) passes a town with a bank; Utrecht's people do not burn their own quarter if the house brought the news (hinge utrecht_1006)." }
   ],
   "pays": { "tier": "finale", "silver": [600, 2500], "grand": ["unlock_charter_dorestad", "unlock_title_doomsman_dorestad"], "by": "the chain dorestad_834's ending, or the card harbour.hinge.dorestad_863_after" },
   "chain": "dorestad_834",
   "war": "delta_834",
   "picture": "A ditch and a raw earth bank with a timber gate; men with spears on it at dawn; longships passing in the river mist without landing.",
   "note": "A gift to the player: the river left Dorestat too. The line and the fixed row say so."
  }
 ],
 "fixed": [
  { "what": "The Kromme Rijn silts up", "years": [850, 900], "why": "A river's course. No merchant, lord or wall could hold it.", "says": "settlements/rhine_delta.json, dorestad, fixed", "source": "Design/content/dossiers/dorestad.md" }
 ],
 "sources": ["as under each hinge"],
 "needs": ["Cards harbour.hinge.dorestad_863_herald and harbour.hinge.dorestad_863_after; the writing key hinge.dorestad_863.turned.", "The ending flag of the chain rorik_850 for 'his man' (written here as ch_rorik_850_end_his_man)."]
}
```

### The fields

| Field | Rule |
|---|---|
| `id` | `<place or event>_<year>`. It names the flag: `changed_<id>`. If a chain already sets a `changed_` flag for this event, use the chain's name. |
| `class` | `place` (one boat, a season, news in time: 1 to 3 entries), `region` (a work, a town kept alive, a factor: 3 to 8 entries, a few holds), `realm` (a house of ships over two lives). **Write no realm hinge except 1066** until the user says so. |
| `entry`, `on` | The real chronicle entry and its `fire` day. If the chronicle lacks the entry (Harold's fleet sent home on 8 September 1066), write it under `entry_new` in the chronicle's own format and list it under `needs`. |
| `really`, `sources` | What really happened, in two or three sentences, with the sources. |
| `could_a_merchant` | Why this is a hinge: what a merchant's food, silver, warning, wall or ship could really bear on. If the honest answer is no, it is a `fixed` row. A hinge is never turned by who wins a fight. |
| `herald` | **Every hinge warns.** `days_before`, and a `card` or a `news` line that tells the player something is coming and what would bear on it. |
| `turned` | The test, in `Cond.cs` grammar on flags: works (`flag.unlock_work_<port>_<kind>`), the town (`flag.st_<port>_heads`), chains (`flag.ch_<id>_end_<name>`), wars (`flag.war_<id>_tilt`), the house (`flag.house_keels`). It is asked on the day `on`, wherever the ship is, so do not use `port.id`, `moored`, `hold.` or `npc.` here: let a card on the spot set a flag and test the flag. |
| `turned_words` | The same test in one plain sentence, for the chest. |
| `softens` | Optional. Earlier or other entries that are not turned but made lighter: `entry`, `when`, `wound_x` (a factor on the people lost), `text`. |
| `instead` | **What happens instead**, as entries in the chronicle's own format (`id`, `date`, `precision`, `fire`, `kind`, `title`, `place`, `ports`, `regions`, `effect`, `note`, `sources`), with `"certainty": "otherwise"`. The first fires on the day `on`. One to three for a place, three to eight for a region. Titles in the Chronicle's flat voice. The game shows them apart, under "Otherwise than the annals tell". An entry here may carry a shock as any entry does (its `kind` decides). |
| `drops` | Later real entries that cannot be told now, by id. Only entries that truly rested on this one. |
| `holds` | Rows as in section 2 that replace the real ones from the day `on`. |
| `line` | A second line for a town when the hinge is turned: `port`, `people` (rows of `year` and `best`), `to` (its new last year, 0 for none), `size_cap` (optional), `note`. The game moves from the old line to this one. |
| `effects`, `world` | What is done on the day, wherever the ship is: so `effects` may only be `flag`, `news`, `schedule` and `price_shock`. The first effect is `flag changed_<id>`. |
| `later` | **What later events it changes**, as a list a reader can check: `year`, `what`, naming the entry ids touched. Everything not written here is left to the ledgers. |
| `pays` | The payout, on the scale of `CHAINS.md`: `tier` (`finale`), `silver` `[low, high]`, `grand` (one or two grand-reward flags), `by` (the card or chain ending that hands it over: a hinge itself pays nothing at sea). |
| `chain`, `war` | The chain and the war it belongs to, or empty. |
| `fixed` | **What cannot be turned**, so nobody writes a hinge for it and the game can say why: `what`, `years`, `why`, `says` (where the game says it), `source`. |

---

## 5. Quests: jobs (repeatable) and chains

There is no quest file. A **chain** is written as `CHAINS.md` says. A **job** is a row of `jobs` in
`story/storyteller_793.json` (section 8), which a town's `asks` (section 1) or a war's phase (sections 2 and 3)
posts.

| | A job | A chain |
|---|---|---|
| Written as | One row of `jobs`, with lines by cause, people and age | Eight to fourteen cards, a writing file, a page |
| Comes | Whenever the world has cause: a town is hungry, burned or besieged; a war musters; a person is taken | Once a house (or once a life, if its page says so), where and when its start card can come |
| Is | A promise with a day, of one of three shapes: `bring` (goods to a port), `carry` (a person or a letter to a port), `find_out` (word of a port no older than so many days) | A story with steps, branches and three endings |
| Remembers | Nothing but the town's ledger, the house's standing and `st_<port>_helped` | Flags `ch_<id>_<n>`, `ch_<id>_end_<name>`, `changed_<event>` |
| Pays | Step or Turn tier of `CHAINS.md`: 20 to 120 d over the market for a town, 200 to 800 d in a war; never a grand reward | Finale tier and a grand reward |
| Fails | It lapses: the deed `broke_commission`, the stake falls on the town; another ship may fill it first | Never with nothing: it goes on by another road |
| Never | Names a real person, sets a `ch_` or `changed_` flag, or says the same line twice running | Repeats |

The house holds three promises at the most (`flag.promises`). A job shows its day and its pay before it is taken.

```json
{
 "id": "feed_hungry",
 "name": "Bread for a hungry town",
 "shape": "bring",
 "cause": "hungry",
 "lane": "place",
 "heat": 1,
 "asker": "reeve",
 "when": "flag.here_hungry >= 1 and not flag.here_besieged",
 "goods": ["rye", "barley", "wheat", "oats", "flour"],
 "units": [10, 60], "units_a_100_heads": 2,
 "premium": [0.4, 0.8],
 "days": [20, 45],
 "repeat_days": 30, "open_at_a_port": 1,
 "kept": { "effects": ["deed kept_commission", "npc reeve feeling +10 fed_us"], "world": ["heal here", "mark here +0.01"] },
 "late": { "effects": ["deed kept_commission"], "world": [] },
 "broken": { "effects": ["deed broke_commission", "npc reeve feeling -15 failed_us"], "world": [] },
 "unfilled": { "world": ["people here -1%"] },
 "lines": {
  "offer": [
   { "when": "port.kind = monastery", "text": "The cellarer has counted the bins twice. He needs {units} of {good} by {day} and will pay {pay} for them." },
   { "when": "flag.here_hungry >= 2", "text": "They are eating the seed corn. {units} of {good} by {day}: {pay}, and they will not forget who came." },
   { "when": "true", "text": "The harvest was thin. The reeve wants {units} of {good} by {day} and offers {pay}." }
  ],
  "kept": [ { "when": "true", "text": "The sacks go up the bank on men's backs. There is bread tonight." } ],
  "broken": [ { "when": "true", "text": "The day passed. Somebody else's boat came, or nobody's." } ]
 },
 "source": "Famine relief by purchase: the Frankfurt capitulary of 794 fixed the price of grain and bread after the famine of 792 to 793 (MGH Capit. I no. 28 c. 4)."
}
```

### The fields of a job

| Field | Rule |
|---|---|
| `id`, `name` | The id is what `asks`, a phase's `jobs` and `job <id> at <port>` name. Wanted at the start: `feed_hungry`, `feed_besieged`, `mend_burned`, `supply_muster`, `ransom_captives`, `carry_refuge`, `carry_envoy`, `carry_letter`, `find_out_port`, `find_out_fleet`. |
| `shape` | `bring`, `carry` or `find_out`. |
| `cause` | `hungry`, `burned`, `besieged`, `sick`, `muster`, `captive`, `envoy`, `refuge`, `want` (a person's). |
| `lane` | `place`, `world` or `house`: the teller's lane it fills. `heat`: 0 to 3. |
| `asker` | A role of the roles table: who asks and who remembers. |
| `when` | When it may be posted, on the `here_` flags, so one row serves every port. |
| For `bring` | `goods` (a town's `asks` may narrow them), `units` `[low, high]`, `units_a_100_heads` (optional: the size follows the town), `premium` `[low, high]` over the market, `days` `[low, high]`. |
| For `carry` | `who` (a role, or `letter`), `to` (how the port is chosen: `nearest_kind:<port kind>`, `holder_seat`, `raiders_sale`, `home`, or a port id), `pay_d` `[low, high]`, `days`. |
| For `find_out` | `about` (how the port is chosen, as `to`), `fresh_days` (the word must be no older), `pay_d`, `days`. The proof is the house's own news of that port: knowledge is the treasure. |
| `repeat_days`, `open_at_a_port` | How soon the same job may be posted again at a port, and how many may stand there at once. |
| `kept`, `late`, `broken` | What follows: `effects` and `world`. `late` is delivery after the day while the need still stands: paid at the market's price, no premium. |
| `unfilled` | What the world does if nobody fills it by the day (`world` only). Other merchants fill jobs too, as they fill orders now. |
| `lines` | `offer`, `kept`, `broken`: each a list of `{ "when", "text" }`, the first that holds is said, the last has `"when": "true"`. Slots: `{units}`, `{good}`, `{day}` (said as a feast or a count of days), `{pay}`, `{port}`, `{who}`. At least three `offer` lines by kind of place, people or age, so the job never reads the same twice running. Under 30 words each. |
| `source` | What in the period makes this a true thing to be asked. |

---

## 6. Starts: `story/starts_793.json`

```json
{
 "about": "THE STARTS, 793-1066: the ways a campaign can begin. Worked example for SCHEMAS.md, 6 Oct 2026.",
 "starts": [
  {
   "id": "debtor_793",
   "name": "The debtor",
   "date": "793-06-21", "hour": 6,
   "port": "dorestad",
   "who": { "people": "frisian", "faith": "christian", "tongues": { "frisian": 1.0, "frankish": 0.6, "english": 0.4 }, "is": "A Frisian of Dorestat with one small boat and a name not yet known beyond the Lek." },
   "ship": { "class": "gislinge", "owned": true, "name_from": "frisian", "state": 0.9 },
   "crew": { "men": 4, "cast": ["cast.crew.ravn"] },
   "coins": { "denier": 40, "penny": 25, "dirham": 6, "hacksilver": 30 },
   "debt": { "d": 480, "to": "count", "due": "793-09-29", "chain": "house_debt" },
   "hold": [],
   "stores_days": 9,
   "standing": { "ports": { "dorestad": 20 }, "regions": { "rhine_delta": 10 } },
   "known": { "ports": ["dorestad", "traiectum", "witla", "noviomagus", "daventre"], "news_reach_km": 700 },
   "texts": [],
   "flags": ["start_debtor_793"],
   "first_card": "harbour.chain_house_debt.s01",
   "burden": "480 pennies owed to the count by Michaelmas.",
   "age": "age1",
   "opens": { "when": "true", "words": "From the first." },
   "ready": { "places": true, "chronicle": true, "raiders": true, "note": "The built game." },
   "picture": "A small clinker boat at a timber jetty in early light; four men; a tally stick in the steersman's belt.",
   "source": "Design/research_indie_and_concept.md 4; chains/house_debt.md"
  },
  {
   "id": "moved_man_808",
   "name": "The moved man",
   "date": "808-08-10", "hour": 12,
   "port": "sliesthorp",
   "who": { "people": "wendish", "faith": "old_gods", "tongues": { "wendish": 1.0, "danish": 0.5, "saxon": 0.4 }, "is": "A merchant of Reric, set down at Sliesthorp from King Godfred's ships with what he could carry." },
   "ship": { "class": "baltic_coastal_boat", "owned": true, "name_from": "wendish", "state": 0.6 },
   "crew": { "men": 5, "cast": [] },
   "coins": { "dirham": 20, "hacksilver": 120 },
   "debt": { "d": 0, "to": "", "due": "", "chain": "" },
   "hold": [ { "good": "wax", "units": 3 }, { "good": "squirrel", "units": 2 } ],
   "stores_days": 4,
   "standing": { "ports": { "sliesthorp": 0, "reric": 30 }, "regions": { "wendland": 20 } },
   "known": { "ports": ["sliesthorp", "reric", "ahus", "truso", "paviken"], "news_reach_km": 500 },
   "texts": [],
   "flags": ["start_moved_man_808", "plot_is_the_kings"],
   "first_card": "harbour.start.moved_man_808",
   "burden": "No home, and a plot the king gave and can take back.",
   "age": "age1",
   "opens": { "when": "year >= 808 or flag.ch_reric_808_end_went_with_them", "words": "A campaign has reached the year 808, or the chain of Reric has ended at Sliesthorp." },
   "ready": { "places": false, "chronicle": true, "raiders": true, "note": "The places files of sliesthorp, reric, ahus, truso and paviken must be true for 808." },
   "picture": "Merchants and bundles on a raw new bank below a long earth wall; Danish ships drawn up; a king's man counting heads.",
   "source": "ARF 808: Godfred destroys Reric and moves its merchants to Sliesthorp; chains/reric_808.md"
  }
 ],
 "sources": ["as under each start"],
 "needs": ["The card harbour.start.moved_man_808; the ending flag of the chain reric_808 (written here as ch_reric_808_end_went_with_them)."]
}
```

### The fields

| Field | Rule |
|---|---|
| `id`, `name` | `<what>_<year>`; a plain name of two or three words. The flag `start_<id>` is set for the whole campaign. |
| `date`, `hour` | The first day and hour. The world is **set** at that day: each town at its line, each port held as the holds say, the chronicle with everything before it as backstory, the markets and the other ships made for the year. So a start needs no history played. |
| `port` | Where she lies. It must exist in that year. |
| `who` | `people` and `faith` (roles table, spine), `tongues` (name: 0 to 1), `is`: one sentence. Ordinary, invented people only: never a historical person. |
| `ship` | `class` (a class id that existed then), `owned` (true, or a line such as "the abbot's, sailed for a third"), `name_from` (a people, for the name forge), `state` 0 to 1. |
| `crew` | `men`, and `cast` ids of the spine who are aboard from the first day. |
| `coins`, `debt`, `hold`, `stores_days` | `coins`: the purse by kind of coin, as `start.coins` in `trade_rules.json` (`denier`, `penny`, `dirham`, and `hacksilver` in pennyweights): the coin must exist in that year and place (no pennies in the Baltic; dirhams there from about 800). `debt.d` in pennies, `to` a role, `due` a date, `chain` the chain that tells it. `hold`: rows of `good` and `units`; keep the first hold small or empty. |
| `standing` | By port and by region, -100 to 100. |
| `known` | `ports` whose waters are on the chart from the first; `news_reach_km`: how far the first day's news reaches. |
| `texts`, `flags` | Text ids in the chest from the first; flags set at the start. |
| `first_card` | The card that opens the game; `burden`: the start's one hard thing, in a line. |
| `age` | The age it begins in. |
| `opens` | `when`: asked of the **house book** (every flag any campaign of this player has set, and `year` as the latest year any of them reached). `"true"` is open from the first. `words`: the same, plainly, shown on the locked start. |
| `ready` | An honest note for the builders: are this start's places, chronicle years and raiders written for its year? A start with `places` false is not offered. |
| `picture`, `source` | One line for the artist; the real ground under the start. |

Wanted first: `debtor_793`, `minsters_man_793` (Eoforwic, a layman sailing an abbot's ship), `moved_man_808`.

---

## 7. Endings: `story/endings_793.json`

Three parts. `house`: what the house became. `worlds`: which world it leaves. `places`: how the places it did most
for are told. A life's own ending is already in `story/winter_793.json` (`remembered`): do not write it again.

```json
{
 "about": "THE ENDINGS, 793-1066: how the house and the world are told at the end. Worked example for SCHEMAS.md, 6 Oct 2026.",
 "when_weighed": { "last_day": "1066-12-25", "early": "flag.house_line_ended", "note": "At Christmas 1066, or on the day a life ends with no heir." },
 "house": [
  {
   "id": "thegn", "rank": 8, "name": "Thegn-worthy",
   "when": "flag.unlock_title_thegn and flag.house_crossings >= 3 and flag.house_plots >= 1",
   "text": "This year {name} the merchant, who had three times crossed the wide sea at his own cost, was held worthy of a thegn's right. His sons sold the ships, all but one.",
   "effects": ["flag ending_thegn"],
   "book": { "page": "The thegn", "opens": ["start:townsman_1000"] },
   "picture": "A timber hall with a bell house and a gate; a ship's prow kept under the eaves.",
   "source": "Gethynctho (early 11th century, among Archbishop Wulfstan's writings): the merchant who crossed the wide sea three times at his own cost (?)"
  },
  {
   "id": "town_that_stands", "rank": 6, "name": "The town that stands",
   "when": "flag.changed_dorestad_863 and flag.st_dorestad_heads >= 400",
   "text": "And Dorestat, which the annals of other men say was wasted, stood in this year behind its bank, and the toll was taken there as before.",
   "effects": ["flag ending_town_that_stands"],
   "book": { "page": "The town that stands", "opens": [] },
   "picture": "A small walled river town in winter sun, one ship at the jetty.",
   "source": "this telling"
  },
  {
   "id": "line_ends", "rank": 1, "name": "The line ends",
   "when": "true",
   "text": "This year died {name}, and no man of his blood took to the steering oar after him. His boat was sold at {port}.",
   "effects": ["flag ending_line_ends"],
   "book": { "page": "A name nobody carries", "opens": [] },
   "picture": "A boat drawn up on a bank under snow, her mast unstepped.",
   "source": "the Chronicle's manner of an obit"
  }
 ],
 "worlds": [
  { "id": "williams_england", "real": true, "when": "true", "text": "And on midwinter's day Archbishop Ealdred hallowed Duke William king at Westminster.", "source": "ASC (D) 1066, retold" },
  { "id": "harolds_england", "real": false, "when": "flag.changed_fleet_1066", "text": "And King Harold's ships kept the sea to Michaelmas and past it, for they had bread; and the duke did not cross that year.", "source": "this telling; against ASC (C) 1066" }
 ],
 "places": {
  "how_many": 3,
  "chosen_by": "the ports with the highest flag st_<port>_helped, ties to the older tie",
  "told_as": "each port's ending text from its settlements file (the one reached), or, if it has not ended, one line from 'still': thriving, steady, thinning",
  "still": {
   "thriving": "{port} had in this year more hearths than any man there remembered.",
   "steady": "{port} stood as it had stood.",
   "thinning": "{port} was smaller than it had been, and its people said so."
  }
 },
 "sources": ["as under each ending"],
 "needs": ["A flag house_line_ended set by the succession when no heir is found.", "The hinge fleet_1066 and its flag changed_fleet_1066."]
}
```

### The fields

| Field | Rule |
|---|---|
| `when_weighed` | Do not change it: the day, and the flag that ends the house early. |
| `house` | The highest `rank` whose `when` holds is the house's ending. The lowest has `"when": "true"`. Eight are wanted, each a rung the period really had: `thegn`, `sworn_brother` (Tiel or Sigtuna: `flag.unlock_title_tiel_brother or flag.unlock_title_frisian_guild`), `town_that_stands` (any `changed_` flag of a town and that town above its line), `house_of_ships` (`flag.house_keels`, `flag.house_factors`), `churchs_house` (`chest.riddles_solved`, texts, alms), `far_farers` (plots and standing in Ireland, Iceland or the east), `sea_wolves` (captives sold, raiders paid, standing low in Christian ports), `line_ends`. |
| `house[].when` | `Cond.cs` grammar. It is asked on the last day wherever the ship is: use flags, `silver`, `debt`, `generation`, `chest.` names; not `port.id` or `hold.`. |
| `house[].text` | The Chronicle's flat voice, under 60 words, with the slots `{name}`, `{home}`, `{port}`, `{heir}`, `{year}`. Our own words: no quotation. Write **a second and third variant** as `text_2`, `text_3` for the rungs most players reach. |
| `house[].book` | The house book: the `page` the ending fills, and what it `opens` for later campaigns: `start:<start id>`, `heirloom:<unlock id>`. |
| `worlds` | Which world the house leaves: tried in order, the first whose `when` holds; the last is `"real": true, "when": "true"`. One for each outcome of a realm hinge, and the real one. |
| `places` | A rule, not a list: fill only `still` (one line for each word a town can wear). |

---

## 8. The storyteller's table: `story/storyteller_793.json`

The teller never moves, hides or invents a chronicle entry. It sets flags, schedules cards, posts jobs, starts made
wars, and gives the built draw one more factor. This file is its numbers (`heat`, `stages`, `leg`, `pulse`,
`season`, `lanes`, `patience`, `caps`), its **table** of what it may start, and the **jobs** (section 5).

```json
{
 "about": "THE STORYTELLER, 793-1066: its numbers, what it may start, and the repeatable jobs. Worked example for SCHEMAS.md, 6 Oct 2026. Every number is a guess to tune in the headless harness.",
 "heat": {
  "about": "0 colour or relief, 1 a choice, 2 a cost or danger, 3 a crisis. The first rule whose pattern matches a card's id gives its heat.",
  "default": 1,
  "rules": [
   { "match": "sea.raider.*", "heat": 3 },
   { "match": "harbour.tollman.*", "heat": 1 },
   { "match": "harbour.winter.*", "heat": 1 },
   { "match": "crew.song.*", "heat": 0 },
   { "match": "*.chain_*.s*_finale", "heat": 2 }
  ]
 },
 "stages": [
  { "stage": 1, "keels": [1, 1], "leg_band": [2, 5], "season_floor": 4, "season_ceiling": 12, "lanes": ["place", "house"] },
  { "stage": 3, "keels": [2, 3], "leg_band": [3, 6], "season_floor": 6, "season_ceiling": 14, "lanes": ["place", "house", "world"] },
  { "stage": 4, "keels": [4, 8], "leg_band": [4, 8], "season_floor": 8, "season_ceiling": 16, "lanes": ["place", "house", "world"] }
 ],
 "leg": { "cards_max": 3, "after_card_hours": 6, "after_card_x": 0.2, "dry_hours": 12, "dry_x": 3.0, "under_band_x": 2.0, "over_band_x": 0.25, "legs_summed": 3 },
 "pulse": {
  "losses": { "man": 0.3, "cargo": 0.3, "third_of_silver": 0.3, "home_struck": 0.3, "boat": 0.6 },
  "half_life_days": 20, "high": 0.6, "quiet_days_after_crisis": 5
 },
 "season": {
  "counted_on": "03-25",
  "reach": { "news_days": 60, "plots": true },
  "weights": { "war": 3, "raid": 2, "burning": 2, "famine": 2, "chain_step": 2, "promise": 1, "other": 1 }
 },
 "lanes": [
  { "lane": "place", "fill_after_days": 20, "from": "the nearest known port whose trend is not steady" },
  { "lane": "house", "fill_after_days": 20, "from": "a crewman's want, then a letter, then kin" },
  { "lane": "world", "fill_after_days": 20, "from": "a war's next phase in reach, then a hinge's herald, then a made war" }
 ],
 "patience": { "job_days": [20, 45], "war_phase_days": 60, "chain_step_days": 90 },
 "caps": { "made_wars_in_reach": 1, "years_clear_of_a_real_war": 2, "chains_open": 2, "promises": 3, "crises_open": 1 },
 "table": [
  {
   "id": "start_sea_kings_summer", "lane": "world", "what": "war", "ref": "sea_kings_summer",
   "when": "year >= 834 and month >= 4 and month <= 6 and flag.house_stage >= 2",
   "weight": 4, "heat": 3, "cooldown_days": 720, "ages": ["age2", "age3", "age4", "age6"],
   "note": "Under the season's floor only. The ground decides where.", "source": "living_world.md 2, 7"
  },
  {
   "id": "hungry_spring", "lane": "place", "what": "trouble", "ref": "store -3",
   "when": "month >= 3 and month <= 5",
   "weight": 3, "heat": 2, "cooldown_days": 360, "ages": [],
   "note": "A thin store at one known port: its job follows by itself.", "source": "the hungry months before the harvest: Bede, the Chronicle's famine years"
  },
  {
   "id": "wreck_cargo", "lane": "house", "what": "card", "ref": "sea.windfall.wreck_cargo",
   "when": "flag.house_stage >= 1",
   "weight": 1.5, "heat": 0, "cooldown_days": 400, "ages": [],
   "note": "A windfall when the pulse is high.", "source": "wreck law: the chain wreckers"
  }
 ],
 "jobs": [
  { "id": "feed_hungry", "see": "the full row in SCHEMAS.md section 5" }
 ],
 "sources": ["Design/living_world.md 7"],
 "needs": ["The card sea.windfall.wreck_cargo."]
}
```

(The one-line `jobs` row above stands for the full job of section 5: in the real file every job is written out.)

### The fields

| Field | Rule |
|---|---|
| `heat.rules` | `match`: a card id with `*` for any run of characters; `heat` 0 to 3. Order matters: the first match wins. Write a rule for every card family there is, the narrowest first; chains may leave their steps at the default and mark only their dangers and finales. |
| `stages` | One row for each band of keels: `leg_band` (the heat wanted over the last `legs_summed` legs), the season's `floor` and `ceiling`, and which lanes push. Stages as in `research_scale_to_fleets.md`. |
| `leg`, `pulse`, `season`, `patience`, `caps` | Numbers only, with the names shown. Do not add keys: ask under `needs`. |
| `lanes` | Three, in this order of names. `from` is a plain line for the programmer. |
| `table` | **What the teller may start when a season is under its floor or a lane is empty.** `id`; `lane`; `what`: one of `war` (`ref` a template id), `job` (`ref` a job id), `card` (`ref` a card id, scheduled at the next port or leg), `chain_start` (`ref` a chain's first card), `news` (`ref` a writing key, posted as a rumour), `trouble` (`ref` one world effect without its port: the teller picks a known port in reach). `when`: `Cond.cs` grammar on flags, `year`, `month`. `weight`: its share in the draw. `heat`. `cooldown_days`. `ages`: age ids it belongs to (empty: all). `note`, `source`. Thirty to fifty rows are wanted, spread over the three lanes and the seven ages, more of them after 825, where the chronicle is thin. A row that invents history is refused: a `trouble` or a made war is always small and nameless. |
| `jobs` | Every job, in full, as section 5. |

---

## 9. Before you report

1. Every id is in its table (ports, goods, roles, deeds, ship classes, work kinds, ages, chronicle entries, chains).
2. Every `when`, `turned` and `check` uses only names of `Cond.cs`; the world's state is read as the flags of rule 9.
3. Every `effects` string is a verb of `Effects.cs`; everything the ledgers do is in `world`, from the list of rule 8.
4. Every figure has a `low`, a `high`, a `best` and a `source`; every guess of your own carries `(?)`.
5. Every list that is tried in order ends with `"when": "true"`, and where history is told, that last row is `"real": true`.
6. Every hinge warns (`herald`), says why a merchant could turn it (`could_a_merchant`) and what it changes later (`later`).
7. Every made thing is small and nameless: no invented king, no real person moved or killed, no crown changed.
8. Your file parses. Run the checker; then report the cards, writing keys, entries and ids you needed under `needs`.
