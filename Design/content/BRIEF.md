# PAUSED BY THE USER (7 Oct 2026, 07:10): READ THIS BEFORE ANYTHING ELSE

The user is short of usage and has asked for new story work to stop for now, without losing work already begun.

- **If your task is to write or check an event chain, quests, settlements, wars, war templates, hinges, starts, endings,
  the storyteller, the gazetteer (curating or merging it), or trade routes, and you have not yet written anything:**
  do NOT do the task. Read nothing more. Write no file. Report at once, and put the single word KAPMAN_PAUSED in every
  text field of your report (notes, corrections, anything you must fill). This costs the user almost nothing, and your
  task will be given again later.
- **If you have already begun writing your files:** ignore this and finish your work as normal.
- **If your task is to CHECK a region of ports (the period-accuracy editor of a region) and you have not yet changed
  anything:** the same: do not do it, and report at once with KAPMAN_PAUSED in every text field.
- **If your task is to weave a region of ports, to write code, to wire the game, or to criticise it:** this does not
  apply to you. Carry on.

# Kapman content brief (4 Oct 2026, night)

The user asked: "research and write all dialogue and interactivity for the game, all scripts, all interactions, all
unlockables, all visitable locations, everything", then: "do your research, take inspirations, tell stories, period
accurate jokes, inspiration from saxon chronicles, events, seasons, gods, culture, characters, interconnecting stories,
riddles, references, easter eggs", and: "research every single visitable place in game, characters of interest, personal
relationships with each other, wars, economy, history, everything period accurate".

This reverses an older rule (the game's words were the user's to write). Everything written here is therefore a
**draft in the game's own files, for the user to overwrite**. Write it as if it ships.

## The game in one paragraph

Kapman: you are a merchant of Dorestad with one small boat (a Gislinge boat: 7.7 m, four men, 660 kg of cargo) and a
debt of 480 pennies to the count, starting 21 June 793, thirteen days after Lindisfarne. The game runs to 1066 through
your heirs. The chart is the game: you sail a 2D map of the North Sea world at a real ship's speed on a clock of one
game minute a second; ports open as panels; things that happen come as cards (a fact, two or three choices, the odds as
a word). Days are the only fuel: the clock stands still while a panel is open. Knowledge is the treasure: prices,
news and rumours are as old as the ship that brought them. No steering, no battle screen: raiders are cards and a
chase on the chart. A wreck costs the boat and cargo; the house goes on. Read `Design/research_indie_and_concept.md`
(sections 4 and 5), `Design/chart_game.md` and `Design/DECISIONS.md` (bottom) before writing.

## Hard rules

1. **Period accuracy.** 793-1066, the North Sea, Channel, Baltic and their rivers. Every historical person, event, law,
   coin, good and place is real, dated and sourced. Where the sources are silent, invent ordinary people only, mark them
   `"historical": false`, and build their names from attested name parts of their people
   (`Assets/Kapman/Game/Data/encounters/names_placeholder.json` shows how). Never give an invented person a known
   historical person's name and role. Mark anything unverified with `(?)` in its `source` or `note` field, as the
   project does everywhere.
2. **No anachronisms.** No horned helmets, no "Vikings" as a people's own word (they are Northmen, Danes, heathen men;
   "wicing" only for raiders at sea), no stave churches before the 11th century, no Danegeld before 991, no Danelaw
   before 878, no Hanse, no guilds by that name before c. 1000 (Tiel 1020), no compass, no sea charts in the world
   (the game's chart is the player's tool), no chimneys, no potatoes, maize, tobacco, no sugar as a common thing, no
   forks, no buttons, no stirrups as a novelty in Francia after 750 (?), no clocks, no paper north of the Alps, no
   coinage in Scandinavia before c. 825 (Hedeby) except as silver by weight, pennies (deniers) in Francia and England,
   dirhams in the Baltic from c. 800. Wine is the lord's and the Mass's drink; ale and mead are everyone's. Dates are
   said by feasts and moons, not by month numbers, when a person speaks.
3. **Plain wording.** The user found period labels "obnoxious": no "thee" and "thou", no mock-archaic. Short plain
   sentences a clever person of any time would say. Dry humour over jokes with punchlines. Jokes must be the period's
   own kind: riddles with a double meaning (the Exeter Book's), a monk's marginal complaint, a proverb turned, a Frank
   mocking a Frisian's short cloak (Notker), a Northman's understatement. No modern idiom, no winking at the player.
4. **Words you may quote.** Only texts already in `Assets/Kapman/Game/Data/texts_793.json`, or public-domain
   translations (translator died before 1956, printed before 1931), credited in a `source` field. Never copy from a
   modern novel, game or translation. The Anglo-Saxon Chronicle (Giles 1847, Ingram 1823), Bede, Alcuin, Einhard and
   Notker (Grant 1907), Rimbert (Robinson 1921), Hávamál (Bellows 1923), the Exeter riddles (Tupper 1910) are safe.
   "Inspiration from the Saxon chronicles" means the Chronicle's own flat, dated voice ("This year...").
5. **Facts in data, words in writing.** A card's logic (when it comes, what it costs, what it does) is data; its words
   are keyed text. Follow the engine: `Design/npc_text_encounters.md` sections 2.1-2.8 and 4.2 (the card, scheduling,
   the condition grammar, NPC memory, tongues, the effect verbs), with `Assets/Kapman/Game/Data/encounters/harbour_793.json`
   as the working example. Condition variables are those in `Assets/Kapman/Code/Core/Encounters/Cond.cs`; effect verbs
   are those in `Assets/Kapman/Code/Core/Encounters/Effects.cs` (silver, goods, health, morale, men, deed, news,
   price_shock, commission, give_text, give_riddle, open_fetch, hire, pay_off, passenger, encounter, law, learn, tongue,
   toll, flag, schedule, npc). Stages: harbour, sea, crew, long. Do not invent variables, verbs, deeds or roles: use
   what exists (roles: `encounters/npc_roles_793.json`; deeds: `reputation_793.json`; goods: `goods_793.json`; ports:
   `ports_793.json`; texts and riddles: `texts_793.json`, `riddles_793.json`; chronicle entries: `chronicle_793.json`).
   If you need one that does not exist, list it under `needs` in your report; do not add it yourself (the spine owns
   the shared tables).
6. **Every card:** a fact, two or three options, each option's outcome a fact; odds are a word (sure, likely,
   doubtful), never a percentage in text; at least one option that costs nothing but time or pride; no card that only
   punishes. Never the same text twice: give each card a `cooldown_days` and each person several lines by `when`.
7. **Agree with the tables.** A port's goods, toll, mint, people and faith must agree with `ports_793.json`,
   `goods_793.json`, `trade_rules.json`. Prices in pennies (d) agree with `Design/economy_and_trade.md`. The year a
   thing exists must be right: a card about Hedeby's mint needs `year >= 825`, Dorestad is gone after 863, Dublin
   exists from 841, Normandy from 911.
8. **Files.** Each writer owns only the files named in its task. Never edit another writer's file, any `.cs` file,
   `writing.json`, `wording_793.json`, or the shared tables. Do not run Unity. Do not commit. Valid UTF-8 JSON, two-space
   or one-space indents, no trailing commas.
9. **Check your work** before you report: `"C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\check_content.py`
   (run it from PowerShell; it lints every content file and prints errors by file). Fix every error in your own files.

## The files

### A places file: `Assets/Kapman/Game/Data/places/<region>.json`

```json
{
 "about": "one line: which ports, who wrote it, when",
 "region": "<a name for this group of ports>",
 "ports": [
  {
   "port": "dorestad",
   "summary": "Three plain sentences: what the place is in 793 and what a merchant comes for.",
   "seasons": "One or two sentences: how the year turns here (the fair, the ice, the herring, the flood).",
   "history": [ { "year": 777, "text": "A dated fact.", "source": "where it is from" } ],
   "eras": [ { "from": 793, "to": 834, "text": "What the place is like in these years (so the game can change it)." } ],
   "sites": [
    { "id": "dorestad.quay", "kind": "quay", "name": "The dam heads", "from": 0, "to": 863,
      "see": "Two or three sentences of what is seen, heard and smelt there.",
      "do": ["card:harbour.tollman.frankish", "person:dorestad.tollman", "market"], "source": "..." }
   ],
   "people": [
    { "id": "dorestad.aldgrim", "name": "Aldgrim", "historical": false, "role": "tollman", "people": "frisian", "faith": "christian",
      "born": 750, "died": 0, "from": 793, "to": 810,
      "trait": "one line", "want": "one line", "fear": "one line", "voice": "how he talks, one line",
      "knows": ["what he can tell: prices here, the law, who is in harbour"],
      "ties": [ { "to": "traiectum.liudger", "tie": "kin|friend|rival|owes|serves|feud|loves|taught", "note": "one line" } ],
      "lines": { "greet": "...", "greet_again": "...", "rumour": ["...", "..."], "saying": "...", "farewell": "..." },
      "source": "invented; the office from the charter of 777" }
   ],
   "stories": [
    { "id": "dorestad.black_stones", "title": "The black stones", "thread": "<a thread id from the spine, or empty>",
      "summary": "Two sentences.",
      "beats": [ { "at": "dorestad.quay", "when": "year < 797", "text": "What is learned or happens here.", "card": "an encounter card id, if one carries it" } ],
      "links": ["lundenwic.offas_reeve"], "reward": "what the player gains: a text, a charter, a price word, a friend" }
   ],
   "sayings": [ { "text": "A proverb or a joke as said here.", "source": "Hávamál 76 (Bellows 1923) | Durham Proverbs | invented in the period's manner" } ],
   "easter_eggs": [ { "text": "A small true thing a curious player finds.", "where": "dorestad.quay", "source": "..." } ],
   "unlocks": [ { "id": "dorestad.charter_toll", "what": "a toll-free charter for Dorestad", "how": "what must be done", "gives": "the game effect, in the engine's verbs if it has one" } ],
   "sources": ["full references, URLs where there are any"]
  }
 ]
}
```

Site kinds: quay, beach, market, toll, mint, church, minster, abbey, hall, thing, smithy, shipyard, tavern (an ale-house:
say "ale-house"), well, gate, wall, ferry, shrine, grove, mound, graveyard, workshop, storehouse, field, fishery, saltern,
quarry, bridge, ford, lodging. A small landing has three sites; an emporium eight to twelve. Every port has at least:
where she lies, where goods are sold, where news is heard, where a god or God is honoured.

People: three to eight a port. Historical persons where a merchant could really have met them (a bishop, an abbot, a
reeve, a king at his hall on a feast), with their real dates in `from`/`to`; ordinary people otherwise. Give ties between
people, across ports and across regions (use the spine's cast ids). Lines are what they say; keep each under 30 words.

### A cards file: `Assets/Kapman/Game/Data/encounters/<name>_793.json`

The engine's own format (`{"about": "...", "cards": [ ... ]}`), exactly as `harbour_793.json`. The game loads every
`.json` in that folder. Each card id is `<stage>.<what>.<where or variant>`, unique across all files.

### A writing file: `Assets/Kapman/Game/Data/writing/<name>.json`

```json
{ "about": "...", "entries": [ { "key": "tollman.greet", "text": "The words." } ] }
```

Every `*_key`, `lines[].key` and `label_key` your cards use must have an entry here, with real words (never
`[write: ...]`). Keys are unique across all writing files; prefix yours with your file's name where they could clash.
The integrator merges these into `writing.json`.

## Tone guide

- The Chronicle's voice for facts: "This year the heathen men first lay over winter on Sheppey."
- People talk like people: short, concrete, about weather, prices, kin and God or the gods.
- A Frisian is dry. A Frank is proud of his king. An English monk is homesick for books. A Dane says less than he
  means. A Svea trader counts everything twice. A Wend trusts oaths sworn on water (?). Use this lightly.
- Faith is everywhere and ordinary: feast days, fasts, relics, a hammer amulet under a shirt next to a cross.
- The sea is work. Nobody says "adventure".

## What good looks like

A player lies at Dorestad in June 793. The toll man remembers them. A priest has a letter from York about
Lindisfarne and wants a book carried to Utrecht. A skipper in from Lundenwic says the king's reeve is turning Frankish
ships away again (true: Charlemagne and Offa's quarrel, c. 790). A blind man sings of old kings for a cup of ale
(Bernlef, a real Frisian singer). Each of those leads somewhere else on the chart, and somebody there knows somebody here.
