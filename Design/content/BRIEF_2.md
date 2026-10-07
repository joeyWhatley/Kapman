# Kapman content brief, part 2 (4 Oct 2026, 22:40)

Read `BRIEF.md` first. This part adds what the user asked for after it was written:
"bring in way more agents", "create event chains, story line chains", "extensive", "with good payouts",
"town interactivity, upgrades, different ships". Where the two parts differ, this one wins.

## How to work on this machine (read this: it saves you an hour)

- **Write every file with the Write tool, and change files with the Edit tool.** A shell heredoc FAILS here whenever the
  text contains an apostrophe ("unexpected EOF while looking for matching quote"), and prose always does. Bash genuinely
  cannot write these files. Use Bash only to list, search and read.
- The checker: run it with the PowerShell tool:
  `& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\check_content.py`
  It prints errors by file. Many writers are working at once, so other files may show errors: fix only your own, and
  finish when your own files are clean.
- **Searching the web is rationed.** Use at most 10 WebSearch calls. For everything else use WebFetch on pages you can
  name: Wikipedia in English, Dutch, German, Danish, Swedish, Norwegian and French, archive.org, the Avalon and
  Fordham sourcebooks, dmgh.de, rundata, runkartan.se, british-history.ac.uk. If a search or a fetch is refused, do not
  stop: work from what you know, and mark each fact you could not check with `(?)`.
- Do not run Unity. Do not run git. Touch only the files named as yours. Do not make documents anywhere except the
  files named as yours (no cloud documents, no artifacts).
- Research already done for you may lie in `Design/content/`: `spine.json` and `spine.md` (the shared cast and threads),
  `dossiers/<port>.md` (sourced research on one port), `peoples/`, `realms/`, `economy/`, `lore/`, `ships.md`. Read what
  exists for your task before you search. If a file is not there yet, go on without it.

## Places are now written one port to a file

`Assets/Kapman/Game/Data/places/<port id>.json`, in the format of BRIEF.md, with one port in `"ports"` and
`"region"` set to the region key. Three more things in each port, because the user asked for town interactivity,
upgrades and different ships:

```json
"sites": [ { "id": "dorestad.yard", "kind": "shipyard", "name": "...", "see": "...",
             "do": ["act:repair", "act:build_ship", "act:fit_upgrade", "person:dorestad.wright", "card:harbour.some.card"] } ],
"works": [ { "id": "dorestad.work.jetty", "name": "A jetty of your own", "what": "One or two sentences, period-true.",
             "cost_d": 600, "days": 90, "from": 793, "to": 863, "needs": "what must be true first, in plain words",
             "gives": "what it changes for the player, in plain words", "source": "..." } ],
"ships": [ { "class": "north_sea_merchant_ship", "how": "built to order over a winter | bought second-hand | hired",
             "from": 793, "to": 863, "note": "why here: the timber, the wrights, the trade", "source": "..." } ]
```

**What can be done at a site** is a list of strings in `do`. Use only these:

- `market` (the market panel), `person:<a person id in this file>`, `card:<a card id>`.
- `act:<id>`, where the id is one of:
  load, repair, careen, hire_porters, hire_pilot, wait_wind, ask_harbour, haggle, weigh_silver, sample, pay_toll,
  show_charter, change_coin, test_silver, alms, hear_mass, ask_news, read_text, copy_text, vow, sanctuary, bury,
  remember, offer, ask_omen, gift, feast, oath, audience, lawsuit, witness, hear_law, rumours, hire_crew, dice, tafl,
  song, riddles, drink, buy_tools, commission, buy_ship, build_ship, fit_upgrade, store, rent_store, buy_plot, lodge,
  heal, provision, buy_at_source, cross, fund_work.
  (What each act does is being written in `Data/story/town_actions_793.json`. Give a site the acts that are true of it:
  a beach market has no mint; a heathen hall takes no alms.)

Every port needs at least one thing to do at every site, and a large place should feel busy: eight to twelve sites,
most with a person and two or three acts. `works` are things the player can pay for that change the town or their
standing in it (a jetty, a storehouse, a bell for the church, a beacon, a well, a causeway, a hospice): two to five in
a large place, none or one in a landing, each period-true and sourced. `ships` lists which of the classes in
`Data/ship_classes_793.json` can be had here, by the class's id.

## A card can now name its port

The condition variable `port.id` holds the id of the port she lies at: `"when": "moored and port.id = dorestad"`.
(It is being added to the engine tonight; the checker already knows it.) Flag names are letters, digits and
underscores only, no dots: `flag ch_black_stones_3`, read as `flag.ch_black_stones_3`.

## Chains

See `CHAINS.md`: the format of an event chain, the payout scale, and the unlock flags.

## Pictures: say what the artist should draw (added 22:35)

The user will draw every picture in the game by hand and asked for a list of each one with a description. So:

- Every **person** gets `"looks"`: one line for the artist. Age, build, hair, dress and one thing they carry or wear,
  all true to their people and the year. Example: `"looks": "About fifty, heavy, grey beard cut short, a plain brown
  cloak with a bronze pin, a tally stick in his belt."`
- Every **site** already has `"see"`: write it so it can be drawn (what is in front, what is behind, the light).
- Every **story**, every **work** and every **ship** entry gets `"picture"`: one line of what its picture shows.
- Every **card** gets a writing entry with the key `art.<card id>` and one line of what the picture on the card
  shows: who, where, doing what, the time of day and the weather. (It is a writing entry so the card format stays as it is.)
- A chain page (`Design/content/chains/<id>.md`) lists a picture for each step and one for each ending.

## Stories have more than one ending (added 22:35)

The user asked for "different endings to stories". Every story in a places file names at least two ways it can end in
`"endings": [ { "id": "...", "how": "what the player did", "text": "what becomes of it", "reward": "..." } ]`.
Every chain has at least three endings (CHAINS.md), and what the player did is remembered by a flag.

## If one of your files is already there (added 5 Oct)

The writers were cut off once in mid-work. If a file named as yours already exists, an earlier writer began it: read it,
keep what is good, and finish it. Do not start again from nothing, and do not leave a half-written file beside a new one.

## The limit on agents (added 6 Oct)

The user has set a hard limit: no more than 30 agents at work at any time, across everything. You are one of them.
Do not start agents, subagents or workflows of your own, for any reason. Do the work yourself, and keep it lean:
read what is already on disk before you search, and stop when your own files are done and clean.
