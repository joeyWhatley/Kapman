# One cast

Made 7 October 2026 for the user's order "We need to get the review to 100/100, 10/10 and 5/5 asap". It answers one
complaint, made by all three critics: `Design/critique/review_newcomer.md` point 4 ("Three casts that never meet",
two points, and its change 3, "One cast", plus 4), `review_words.md` point 11 ("The word on a good is a form letter",
a point), and `review_systems.md`, which calls the told price the reason to buy the game.

Two files: this one, and `Design/checks/cast_793.json` (the cast itself, 207908 bytes, made by a script from the
game's own data; nothing in it is invented).

What a builder can do at once is in part 5: two new C# files and 33 small edits in 12 files, each given
with the lines it replaces. I compiled the whole of Kapman.Core and Kapman.Game with them outside Unity and both
compile. I did not run Unity, the tests or the game.

## 1. The finding, in numbers

All counts are from the data as it stood at about 12:40 on 7 October, by the script described in part 8.

| | |
|---|---:|
| cards in `Data/encounters/*.json` | 1392 |
| of them can open in the first release (twelve ports, 793 to 805; the census of `Bake/content/card_census.py`) | 420 |
| people of the twelve ports' places files who are there in 793 to 805 | 77 |
| of them, people an open card should speak through | 73 |
| of them, people who should tell prices (by their trade) | 66 |
| offices (a port, a fixed role, a person of the places file who holds it) that an open card speaks through, where the game today forges somebody else | 48 |
| card parts those offices speak (a part: one actor of one card at one port) | 184 |
| card parts that go through the cast in all (offices, people a card's note names, the by-hand rows) | 263 |
| card parts at an office nobody of the places files holds | 28 |
| card parts of a card's own person (its note describes somebody no places file has) | 83 |
| card parts of the seats aboard (steersman, mate, lookout, the Dane) | 299 |

How the person behind a card part was found, over the open cards at the twelve ports: an id in the actor's note 187; a name at the start of the note 2; the actor's key 0; the holder of the role at the port 74; nobody (a gap) 111.

So the critic's case (Aldgrim, Liudbrand, "the toll man at Dorestat") is not one slip. It is how the game is built:
there are **four lists of people and no join between them**, and a fifth and sixth that the art and the story use.

## 2. Where each is decided in code today

Line numbers are as I read the files between 12:10 and 12:40 on 7 October. Six builders were changing files under
`Assets` in that time, so trust the quoted words over the numbers.

**(a) Who speaks on a card.** `Code/Core/Encounters/EncounterSession.cs`, `Open`, lines 146 to 151: for each actor
of the card, a fixed one is `world.npcs.Holder(spec, world.port, world.year, world.day)` and any other is
`world.npcs.Forge(...)`. `Code/Core/Encounters/NpcRegistry.cs`: `Holder` (lines 254 to 267) returns the living
holder of "port/role" or forges one; `Forge` (277 to 308) makes a person with the id `<port>.<role>.<number>`
(line 291) and a name drawn from `Data/encounters/names_placeholder.json` by `NamePool.Pick` (line 301), seeded by
the game's seed, the port, the role and the decade (line 283). It never looks at a places file. So the toll man of
the card `harbour.tollman.frankish` at Dorestat is `dorestad.tollman.0` with a pool name. The critic's capture shows
"Liudbrand"; which name is drawn depends on the seed, and I did not work the draw out again. The line on screen
("Liudbrand, the toll man · easy") is `EncounterSession.Speaker`, lines 128 to 136; the window takes it in
`Code/Game/UI/Frame/FrameRoot.Floats.cs`, `CardSpec` (the lines `spec.who = s.Speaker(first); spec.face = first.id;`).

**(b) Who is in People here.** `Code/Game/UI/Frame/Panels/PeoplePanel.cs`, `Fill`, lines 46 to 55:
`PlaceBook.Current.People(here.id, year)`, the people of `Data/places/<port>.json` who are there that year
(`Code/Game/Content/Places/PlaceBook.cs`, lines 103 to 112). The talk is `Code/Game/UI/Frame/PersonTalk.cs`: a
greeting, a rumour, a saying, a riddle and their own lines; no price. The line under the name ("Tollman, at the
king's toll post") is `PersonTalk.Where`, lines 69 to 75, which prints the role's id with a capital.

**(c) Who told a price.** A told price is a `PriceNote` (`Code/Game/Known/PriceNotes.cs`, lines 16 to 31) with two
words about its teller: `teller` ("skipper" or "quay") and `tellerOf` (a port). They are set in three places:
`Code/Game/KapmanGame.cs`, `KnowHome`, lines 374 to 376 (the first morning: every price the home waterfront knows,
all as "quay" of Dorestat); `Code/Game/Ports/PortLife.cs`, `Talk`, lines 73 to 78 (the row "Talk to people": a
master in port who saw them is "skipper", the rest "quay"; one click, every market on the board); and
`Code/Game/Sim/Encounter.cs`, lines 209 to 210 (a ship spoken at sea: "skipper"). The words come from
`Code/Game/Known/Word.cs`, `Person`, lines 268 to 282: for "quay" it picks one of four phrases by a hash of the note
(line 279) from `Data/wording_793.json`, `word`, `person.quay` (lines 652 to 657: "the toll man at {place}", "a
boatman at {place}", "a porter at {place}", "an ale-wife at {place}"; the Northmen's own four at lines 265 to 270).
No person is behind any of the four. At Lundenwic, which has a reeve and no toll man, it still says "the toll man
at Lundenwic".

**(d) Whose portrait shows.** `Code/Game/UI/Frame/FrameDrawn.cs`, `FrameFace.Who`, lines 121 to 132: a slot is set
only for "you"; every other speaker gets the empty shape. `Code/Game/UI/Frame/FrameArt.cs` line 26 can name a
person's slot (`portrait/person.<id>.png`) and nothing calls it. For 17 people of the twelve it would name a slot
that is not in `Art/art_slots.json`: their pictures are listed under the spine's id (part 3, clash 9).

**(e) Two more lists.** `Design/content/spine.json` `cast` (44 people, ids `cast.x`, each with the `place_id` of
their places row) and `Data/story/weight_793.json` (people of weight, ids `wt.x`). The art list and the card rules
(`Bake/content/out/card_rules_793.json`) use all three kinds of id. `card_rules_793.json` already says, for each
card's chief speaker, which person of the places files that is (`speaker`, `faces_by_port`); the cast file here
uses the same reading, for every actor and not only the chief one, and adds the years. I held the two against each other: of the 274 cases where the card rules name a person of the twelve ports as an open card's chief speaker, the cast file has the card under that same person in 170 as a row of `cast`, and in 104 more under `aboard` (a seat of the ship) or `visitors` (a person met away from their own port); they disagree nowhere.

## 3. The clashes, and which name stands

The rule for all of them: **the places files stand.** They are what the user reads and rewrites, they are what
People here shows, and each person in them has looks, a voice, ties and lines. A forged name has none of that.

| # | The clash | Which stands | Why |
|---|---|---|---|
| 1 | The toll man at Dorestat: Aldgrim (`dorestad.tollman`, places file, 793 to 810), a forged man on the card (Liudbrand in the critic's capture), "the toll man at Dorestat" on the price row | **Aldgrim** | He is the places file's; his own line there already answers the card ("The small boat. Last time wine, six casks, and you counted five.") |
| 2 | The same in every office: 48 offices of the twelve ports where a person of the places file holds the role and the card forges another (184 card parts) | the places person, each time | as 1. The rows are in `cast_793.json` (`office` true, `alias` false, `cards` not empty) |
| 3 | The forge's pool holds the cast's own names: 75 of the 233 names in `names_placeholder.json` are the name of a person of some places file. Liudbrand is the priest at Medemelacha (`medemelacha.liudbrand`); Aldgrim is in the pool too, so a stranger anywhere in Frisia can be a second Aldgrim | the places person keeps the name; the forge draws again | the file's own "about" says none of its names is used for a named person, and that is no longer true |
| 4 | Two at once in one office: the pilot at Witla is Gerlef (793 to 812) and Gerbrand (800 to 836); the craftsman at Ripa is Toki the bead maker (793 to 812) and Gudmund the well-wright (793 to 815) | **Gerlef** to 812, then Gerbrand; **Toki** to 812, then Gudmund | the spine's cast first (Gerlef is `cast.gerlef`), then the earlier year, then the file's order. The other is still in People here and still tells prices; only a card that names him speaks through him |
| 5 | One office, two words: the cards ask for the "tollman" at Fordwic and Quentovic and for the "reeve" at Sarre; the places files call those men reeve (Tilred), clerk (Hildric) and tollman (Dunwalh) | **Tilred**, **Hildric**, **Dunwalh** (by hand, the reasons are in the file) | each places file says in its own words that this man takes the toll. At Walichrum I gave the toll to **Reginbert** the steward, marked (?): he is the only king's man on the island. The user may strike it |
| 6 | A card names a person in another part than the places file gives: the bishop Hamacar as "priest", Thiadbod the skipper as "merchant" and "steersman", the count Folcbert as "lords_man", Gerbrand of Witla as "child" and "sailor" (nine such pairs of person and part; three are on cards that can open: Thiadbod as steersman on one, Gerbrand as child on four and as sailor on one) | the person; his title is the places file's | the card's part is only a slot. On screen he is "Hamacar, the bishop", whatever the slot is called |
| 7 | Gerbrand of Witla is on four open cards from 793 as the pilot's boy, and the places file has him at Witla from 800 | **the cards**: set his `from` to 793 in `places/witla.json` | he was born in 785 by the same file. Until then People here will not list a boy the cards speak through. (A change to a data file: for the content pass, not made here) |
| 8 | One name, two people in the release: Gerbrand (the shipwright at Dorestat, the pilot's boy at Witla); Radward (the skipper at Witla, the toll man at Medemelacha) | both stand | they are different men of a common name. The price row must say which: "Radward the toll man at Medemelacha" (part 5 does) |
| 9 | Seventeen people have their picture listed under the spine's id (`portrait/person.cast.folcbert.png`) and the code would ask for the places id (`portrait/person.dorestad.folcbert.png`): Ravn, Folcbert, Hartwin, Thiadbod, Thiadburg, Sigimar, Rorik, Eystein, Hamacar, Wighelm, Thorkell, Cnut, Gerlef, Aldger, Brunger, Gervold, Æthelheard | **the art list's file name** | the user draws from that list; the code must not rename the user's files. Each row of the cast file carries the slot (`portrait`) |
| 10 | Three words for one office on one screen: "Tollman" (People here), "the toll man" (the card), "The toll man at Dorestat" (the price row) | **the title of `npc_roles_793.json`** ("the toll man", "the ale-wife", "the reeve") | it is the one table of titles; each cast row carries it (`title`) |
| 11 | The crew: the cards aboard have a steersman, a mate, a lookout and a Dane, forged afresh (299 open card parts), while the spine seats four named people there (Aldger of Doccinga 95 parts, Brunger of Daventre 31, Beornwine of Gyrwe 97, Ravn of Dorestat 73), and People here lists Ravn on the dam, Aldger at Doccinga and Brunger at Daventre as if they were ashore | **the spine's four** | they are written people with lines. Not built here: the crew are "not yet people with names" (`CrewPanel.cs`), and that comes with hiring. The seats are in the file (`aboard`) for whoever builds it. Until then People here should leave out a person who is aboard |
| 12 | The spine and the places file disagree on a year for two people of later ages: Rorik (spine from 841, places from 850), Cnut (1014, 1016) | the places file | the game reads the places file; neither is in the first release |
| 13 | A spelling: `weight_793.json` has "Aethelheard", the places file "Æthelheard" | **Æthelheard** | it is the one on screen in People here |
| 14 | The cards' own people: 83 open card parts where the card's note describes or names somebody no places file has ("Folkmar, a quern cutter of the vicus; invented"). The forge names him something else than the note | not settled here | either the person is written into the places file, or the card's actor gets a `name`. They are listed by port in the file (`gaps`, `own_person_cards`) for the content pass |
| 15 | Offices nobody holds: 28 open card parts ask for a fixed role at a port where the places file has nobody in it. The ones a new player meets: **the toll man at Traiectum, at Doccinga and at Daventre** (the card `harbour.tollman.frankish` comes at every Frankish port with a toll, and Traiectum is in the first hour). The rest: a priest at Traiectum, Walichrum and Quentovic, a pilot at Quentovic, a steward at Traiectum, a law man at Dorestat, a wise woman at Ripa, and a weaver at nine ports for the three marriage cards | **for the user to write** (three toll men first), or the toll card is kept from those three ports | I will not invent a person. Until one is written the forge's man stays: he is at least the same man every call and is saved; with clash 3 mended he no longer bears a neighbour's name |

## 4. The cast of the first release

From `cast_793.json`, rows with years in 793 to 805. "holds it" is the span the person holds the part. (trade): a
role several people follow at once, so only a card whose own note names the person speaks through them. (card names
them): the places file calls them something else and a card's note puts them in this part. (by hand): clash 5.
"open cards" counts the cards that can open in the release and speak through the person in that part; the ids are
in the file.

**Dorestat**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman | Aldgrim (`dorestad.tollman`) | 793-810 | 1 | all goods, as "the toll man" |
| moneyer | Sigimar (`dorestad.sigimar`) | 793-815 | 3 | nothing |
| count | Folcbert (`dorestad.folcbert`) | 793-812 | 11 | nothing |
| steward | Hartwin (`dorestad.hartwin`) | 793-808 | 12 | 10 goods |
| lords_man (card names them) | Folcbert (`dorestad.folcbert`) | 793-812 | 0 | nothing |
| priest | Alfward (`dorestad.priest`) | 793-822 | 12 | 5 goods |
| merchant (trade) | Hrotberct (`dorestad.hrotberct`) | 793-803 | 2 | 10 goods |
| merchant (card names them) | Thiadbod (`dorestad.thiadbod`) | 793-825 | 0 | all goods, as "a boatman" |
| skipper (trade) | Thiadbod (`dorestad.thiadbod`) | 793-825 | 16 | all goods, as "a boatman" |
| steersman (card names them) | Thiadbod (`dorestad.thiadbod`) | 793-825 | 1 | all goods, as "a boatman" |
| crewman (trade) | Ravn (`dorestad.ravn`) | 793-815 | 3 | nothing |
| shipwright | Gerbrand (`dorestad.gerbrand`) | 793-833 | 3 | 10 goods, as "a ship's carpenter" |
| alewife | Aldswith (`dorestad.aldswith`) | 793-826 | 2 | 11 goods, as "an ale-wife" |
| widow | Thiadburg (`dorestad.thiadburg`) | 793-815 | 18 | all goods |

Nobody holds: lawman (chain_house_rival.end_feud); weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 14 open card parts.

**Traiectum**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| priest (card names them) | Hamacar (`traiectum.hamacar`) | 793-806 | 0 | nothing |
| bishop | Hamacar (`traiectum.hamacar`) | 793-806 | 4 | nothing |
| guest_master (by hand) | Hadda (`traiectum.cellarer`) | 793-801 | 1 | 32 goods |
| cellarer | Hadda (`traiectum.cellarer`) | 793-801 | 4 | 32 goods |
| schoolmaster | Radger (`traiectum.schoolmaster`) | 793-813 | 6 | 4 goods |
| ferryman | Aldlef (`traiectum.aldlef`) | 793-822 | 2 | all goods, as "a boatman" |
| alewife | Hildeburg (`traiectum.hildeburg`) | 793-818 | 1 | 11 goods, as "an ale-wife" |

Nobody holds: priest (chain_house_marriage.s05_frankish_table, chain_house_marriage.s06_frankish_deed, years_famine.corn_805); steward (chain_house_marriage.s05_frankish_table, chain_house_marriage.s06_frankish_deed); tollman (tollman.frankish); weaver (chain_house_marriage.s05_frankish_table, chain_house_marriage.s06_frankish_deed, chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 10 open card parts.

**Walichrum**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman (by hand) | Reginbert (`walichrum.reginbert`) | 793-816 | 1 | all goods, as "the toll man" |
| steward | Reginbert (`walichrum.reginbert`) | 793-816 | 0 | all goods, as "the toll man" |
| craftsman | Wilbraht (`walichrum.wilbraht`) | 793-828 | 2 | 11 goods |
| farm_wife (trade) | Bernhild (`walichrum.bernhild`) | 793-812 | 8 | 15 goods |
| alewife | Wiltrud (`walichrum.wiltrud`) | 793-820 | 2 | 11 goods, as "an ale-wife" |

Nobody holds: priest (years_famine.corn_805); weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 7 open card parts.

**Lundenwic**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| moneyer | Ibba (`lundenwic.moneyer`) | 793-800 | 1 | nothing |
| reeve | Wighelm (`lundenwic.wighelm`) | 793-810 | 9 | all goods, as "the toll man" |
| priest | Eanwulf (`lundenwic.eanwulf`) | 793-812 | 4 | 5 goods |
| merchant (trade) | Cynred (`lundenwic.woolman`) | 793-816 | 1 | 7 goods |
| craftsman | Hunfrith (`lundenwic.hunfrith`) | 793-818 | 1 | 5 goods |
| weaver (trade) | Leofrun (`lundenwic.cloakmaker`) | 793-820 | 3 | 16 goods |
| alewife | Wynflaed (`lundenwic.wynflaed`) | 793-822 | 1 | 11 goods, as "an ale-wife" |
| host | Tatberht (`lundenwic.tatberht`) | 793-815 | 5 | 11 goods, as "an ale-wife" |
The cards' own people here (a note describes them, no places row): 5 open card parts.

**Witla**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman | Folcrad (`witla.folcrad`) | 793-811 | 1 | all goods, as "the toll man" |
| priest | Bernrad (`witla.bernrad`) | 793-822 | 2 | 5 goods |
| skipper (trade) | Radward (`witla.radward`) | 793-819 | 1 | all goods, as "a boatman" |
| pilot | Gerlef (`witla.gerlef`) | 793-812 | 9 | all goods, as "a boatman" |
| sailor (card names them) | Gerbrand (`witla.gerbrand`) | 800-836 | 1 | all goods, as "a boatman" |
| weaver (trade) | Thiadlint (`witla.thiadlint`) | 793-824 | 2 | 16 goods |
| alewife | Wilsuit (`witla.wilsuit`) | 793-828 | 7 | 11 goods, as "an ale-wife" |
| child (card names them) | Gerbrand (`witla.gerbrand`) | 800-836 | 4 | all goods, as "a boatman" |
The cards' own people here (a note describes them, no places row): 3 open card parts.

**Medemelacha**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman | Radward (`medemelacha.radward`) | 793-812 | 4 | all goods, as "the toll man" |
| priest | Liudbrand (`medemelacha.liudbrand`) | 793-826 | 2 | 5 goods |
| skipper (trade) | Eilbern (`medemelacha.eilbern`) | 793-822 | 1 | all goods, as "a boatman" |
| craftsman | Tado (`medemelacha.tado`) | 793-815 | 1 | 4 goods |
| host | Meinburg (`medemelacha.meinburg`) | 793-818 | 2 | 11 goods, as "an ale-wife" |

Nobody holds: weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 2 open card parts.

**Doccinga**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| priest | Meinward (`doccinga.meinward`) | 793-822 | 4 | 5 goods |
| pilot | Radbrand (`doccinga.radbrand`) | 793-818 | 0 | all goods, as "a boatman" |
| steersman (trade) | Aldger (`doccinga.aldger`) | 793-820 | 3 | nothing |
| smith | Meinbod (`doccinga.smith`) | 793-819 | 0 | 17 goods |
| weaver (trade) | Meinhild (`doccinga.meinhild`) | 793-815 | 2 | 16 goods |
| farm_wife (trade) | Adelsuit (`doccinga.adelsuit`) | 793-836 | 2 | 15 goods |
| widow | Liafhild (`doccinga.liafhild`) | 793-812 | 1 | 10 goods |

Nobody holds: tollman (tollman.frankish).
The cards' own people here (a note describes them, no places row): 2 open card parts.

**Daventre**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| priest | Liudbod (`daventre.liudbod`) | 793-818 | 4 | 5 goods |
| mate (trade) | Brunger (`daventre.brunger`) | 793-815 | 1 | nothing |
| smith | Einbert (`daventre.einbert`) | 793-812 | 2 | 17 goods |
| ferryman | Thancric (`daventre.thancric`) | 793-816 | 1 | all goods, as "a boatman" |
| alewife | Hathugard (`daventre.hathugard`) | 793-806 | 1 | 11 goods, as "an ale-wife" |

Nobody holds: tollman (tollman.frankish); weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 4 open card parts.

**Quentovic**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman (by hand) | Hildric (`quentovic.hildric`) | 793-826 | 1 | all goods, as "the toll man" |
| moneyer | Gerwin (`quentovic.gerwin`) | 793-815 | 1 | nothing |
| abbot | Gervold (`quentovic.gervold`) | 787-806 | 0 | nothing |
| monk (trade) | Meginbert (`quentovic.meginbert`) | 793-818 | 2 | 4 goods |
| guest_master | Osmund (`quentovic.osmund`) | 793-812 | 5 | 6 goods |
| clerk (trade) | Hildric (`quentovic.hildric`) | 793-826 | 3 | all goods, as "the toll man" |
| skipper (trade) | Leofstan (`quentovic.leofstan`) | 793-812 | 1 | all goods, as "a boatman" |
| host | Wulfgifu (`quentovic.wulfgifu`) | 793-822 | 4 | 11 goods, as "an ale-wife" |

Nobody holds: pilot (quentovic.sails_off_the_bar); priest (quentovic.pilgrims_grave, years_famine.corn_805); weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 6 open card parts.

**Serrae (sarre)**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman | Dunwalh (`sarre.tollman`) | 793-812 | 3 | all goods, as "the toll man" |
| reeve (by hand) | Dunwalh (`sarre.tollman`) | 793-812 | 1 | all goods, as "the toll man" |
| nun (trade) | Cwenburh (`sarre.cwenburh`) | 793-815 | 4 | 16 goods |
| farmer (trade) | Bynna (`sarre.bynna`) | 793-830 | 2 | 15 goods |
| ferryman | Dudda (`sarre.ferryman`) | 793-825 | 2 | all goods, as "a boatman" |
| alewife | Tidburh (`sarre.alewife`) | 793-820 | 2 | 11 goods, as "an ale-wife" |

Nobody holds: weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 14 open card parts.

**Fordwic**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| tollman (by hand) | Tilred (`fordwic.reeve`) | 793-815 | 2 | all goods, as "the toll man" |
| moneyer | Eoba (`fordwic.eoba`) | 793-800 | 2 | nothing |
| reeve | Tilred (`fordwic.reeve`) | 793-815 | 3 | all goods, as "the toll man" |
| bishop | Æthelheard (`fordwic.aethelheard`) | 793-805 | 1 | nothing |
| scribe (trade) | Eadhun (`fordwic.scribe`) | 793-831 | 4 | 4 goods |
| farmer (trade) | Wicga (`fordwic.wicga`) | 793-818 | 1 | 15 goods |
| farm_wife (trade) | Cwengyth (`fordwic.cwengyth`) | 793-822 | 1 | 15 goods |

Nobody holds: weaver (chain_house_marriage.s07_morning_gift).
The cards' own people here (a note describes them, no places row): 15 open card parts.

**Ripa**

| part | person | holds it | open cards | tells prices of |
|---|---|---|---:|---|
| skipper (trade) | Roulf (`ripa.skipper`) | 793-825 | 2 | all goods, as "a boatman" |
| goldsmith | Soti (`ripa.goldsmith`) | 793-822 | 2 | 12 goods |
| craftsman | Toki (`ripa.bead_maker`) | 793-812 | 2 | 9 goods |
| carter (trade) | Alli (`ripa.carter`) | 793-820 | 2 | 8 goods, as "a porter" |
| alewife | Tola (`ripa.tola`) | 793-826 | 2 | 11 goods, as "an ale-wife" |
| host | Hroald (`ripa.hroald`) | 793-818 | 10 | 24 goods, as "a boatman" |

Nobody holds: weaver (chain_house_marriage.s05_danish_hearth, chain_house_marriage.s06_danish_handclasp, chain_house_marriage.s07_morning_gift); wise_woman (chain_house_marriage.s05_danish_hearth, chain_house_marriage.s06_danish_handclasp).
The cards' own people here (a note describes them, no places row): 1 open card parts.


Of the 77 people, four have no open card that speaks through them: Radbrand the
pilot and Meinbod the smith at Doccinga, Gervold the abbot at Quentovic, and Gudmund the well-wright at Ripa. Eleven
tell no prices: the four moneyers (they tell the worth of coin, which is not a good), the count, the two bishops and
the abbot (their stewards, cellarers and clerks do), and Ravn, Aldger and Brunger (they are aboard).

**Who tells prices, and of what.** By trade, from each person's own `knows` lines where they have one and else from
their role. Those who hear of every good: the man who takes the toll (toll man, reeve, the clerk at Quentovic), the
boatmen (pilot, ferryman, a skipper of the place) and the widow Thiadburg, who keeps other men's goods. The rest tell
only their own: an ale-wife or a host drink, malt and provisions; a shipwright ship stores, timber and iron; a smith
metals and weapons; a weaver cloth, wool and dye; a farm wife grain, cheese, butter and beasts; a priest wax,
incense, books; Hrotberct wax, honey and spices; Cynred wool and hides; Hunfrith antler and combs; Tado stones. The
ids are in each row (`tells_goods`). Every one of the twelve ports has at least one person who hears of every good.
These lists are my reading of the files and the weakest part of the cast: they are for the user to correct.

## 5. What the engine must do

One book, `CastBook`, reads the cast file. The four places of part 2 ask it. Nothing else is needed for a card's
speaker, the People here list, the teller on a price and the portrait to be the same person:

1. **A card's actor is the cast's person.** `NpcRegistry` gets a hook (`named`); the director sets it to
   `CastBook.Find`. A fixed actor is the holder of that office that year; any actor is the person whose row lists
   the card (a person the card's own note names). The person enters the registry **under the places id**
   (`dorestad.tollman`, not `dorestad.tollman.0`), so what he feels and remembers of the merchant hangs on the same
   id People here uses. Where nobody is written the forge still makes a man, and no longer gives him the name of
   anybody in a places file.
2. **People here** needs no change to list the same people: it already reads the places files. It gains each
   person's record beside the name ("Their word has held 3 times in 4.").
3. **The teller on a price is a person's id.** Where a price was "quay" talk, it is now told by one of the port's
   people who hear of every good, the same one every time for the same note. `Known.Person` then says "Aldgrim the
   toll man", and "Aldgrim the toll man at Dorestat" when she does not lie there. A note with no person (an older
   save, a skipper met at sea) reads as before.
4. **A person can be asked for prices.** The talk gains "Ask what the markets pay": one asking hands over one
   market from the waterfront, only the goods of that person's trade, with their name on it, said in one line
   ("Lundenwic, last week: well for Rhenish wine and wax, poorly for quernstones."). This is the critic's smallest
   mend, and the one line answers `review_words.md` point 11.
5. **The portrait** of whoever speaks is the slot of their cast row; a forged man gets the stand-in of his role.
6. **A told word is put to the proof at the mooring** (part 6).

**Before the code goes in:** copy `Design/checks/cast_793.json` to `Assets/Kapman/Game/Data/story/cast_793.json`
(the path `CastBook.File` reads). With no file there the book is empty and the game behaves exactly as today.

**The two new files.** Compiled as they stand here.

`Assets/Kapman/Code/Game/Content/Places/CastBook.cs`

```csharp
// 7 Oct 2026: THE ONE CAST (Design/fixes/one_cast.md). Reads Game/Data/story/cast_793.json: for each port, role and
// span of years the one person of the places files who is that role there. A card's speaker, the People here list, the
// teller on a price and the portrait all ask this book, so they are the same person.
using System;
using System.Collections.Generic;
using Kapman.Encounters;

namespace Kapman.Game
{
    /// <summary>One row of the cast: a person of a places file in a part (a role) at a port for a span of years.</summary>
    public sealed class CastRow
    {
        public string port = "", role = "", person = "", name = "", title = "", placesRole = "", portrait = "", standIn = "", tellsAs = "";
        /// <summary>The years the person holds the part, and the years they are at the port at all.</summary>
        public int from, to, atFrom, atTo;
        /// <summary>office: one person at a time holds the role (a card's fixed actor of it is this person). alias: the
        /// places file calls them something else.</summary>
        public bool office, alias, tellsPrices, tellsAll;
        public readonly HashSet<string> tellsGoods = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> cards = new HashSet<string>(StringComparer.Ordinal);

        public bool Holds(int year) => ContentJson.InYears(from, to, year);
        public bool AtPort(int year) => ContentJson.InYears(atFrom, atTo, year);
        /// <summary>Does this person tell the price of this good (by their trade)?</summary>
        public bool Tells(string good) => tellsPrices && (tellsAll || tellsGoods.Contains(good ?? ""));
    }

    public sealed class CastBook
    {
        public const string File = "story/cast_793.json";

        readonly List<CastRow> rows = new List<CastRow>();
        readonly Dictionary<string, List<CastRow>> byPart = new Dictionary<string, List<CastRow>>(StringComparer.Ordinal);    // "port/role"
        readonly Dictionary<string, List<CastRow>> byPerson = new Dictionary<string, List<CastRow>>(StringComparer.Ordinal);
        readonly Dictionary<string, List<CastRow>> byPort = new Dictionary<string, List<CastRow>>(StringComparer.Ordinal);

        static readonly ContentCache<CastBook> cache = new ContentCache<CastBook>("the cast", Load);
        public static CastBook Current => cache.Value;

        public IReadOnlyList<CastRow> Rows => rows;

        /// <summary>The first row of a person (null: not of the cast).</summary>
        public CastRow Row(string person) => person != null && byPerson.TryGetValue(person, out var l) && l.Count > 0 ? l[0] : null;

        /// <summary>Who holds an office at a port that year (null: nobody written).</summary>
        public CastRow Holder(string port, string role, int year)
        {
            if (!byPart.TryGetValue(port + "/" + role, out var l))
                return null;
            foreach (var r in l)
                if (r.office && r.Holds(year) && r.AtPort(year))
                    return r;
            return null;
        }

        /// <summary>
        /// Who a card's actor is: the row of that part at that port that lists the card (a person the card's own note
        /// names, whatever their trade); else, for a fixed actor, the holder of the office that year; else nobody (a
        /// stranger: the forge makes one).
        /// </summary>
        public CastRow Speaker(string card, string port, string role, int year, bool fixedRole)
        {
            if (!string.IsNullOrEmpty(card) && byPart.TryGetValue(port + "/" + role, out var l))
                foreach (var r in l)
                    if (r.cards.Contains(card) && r.AtPort(year))
                        return r;
            return fixedRole ? Holder(port, role, year) : null;
        }

        /// <summary>The people of a port who tell prices that year, one row a person, in the file's order.</summary>
        public List<CastRow> Tellers(string port, int year)
        {
            var list = new List<CastRow>();
            if (port == null || !byPort.TryGetValue(port, out var l))
                return list;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in l)
                if (r.tellsPrices && r.AtPort(year) && seen.Add(r.person))
                    list.Add(r);
            return list;
        }

        /// <summary>
        /// Who at a port tells the prices of another market on a day: one of the port's tellers, the same one every time
        /// for the same note (nothing is drawn). Those who hear everything (the toll man, a boatman) are picked twice as
        /// often as those who know their own trade. Null: nobody written of at that port tells prices that year.
        /// </summary>
        public CastRow Teller(string port, int year, string ofPort, double day, bool allOnly = false)
        {
            var all = Tellers(port, year);
            if (allOnly)
                all.RemoveAll(r => !r.tellsAll);                         // (only those who hear of every good: the toll man, a boatman)
            if (all.Count == 0)
                return null;
            int weight = 0;
            foreach (var r in all)
                weight += r.tellsAll ? 2 : 1;
            int pick = (int)(Known.Hash01(port + "/" + ofPort, day, 0, 23) * weight);
            foreach (var r in all)
            {
                pick -= r.tellsAll ? 2 : 1;
                if (pick < 0)
                    return r;
            }
            return all[all.Count - 1];
        }

        /// <summary>
        /// The picture's slot for whoever speaks: a person of the cast has their own (the art list's, which for a person
        /// of the spine is person.cast.x and not person.&lt;places id&gt;); a forged person ("traiectum.tollman.0") has
        /// the stand-in of their role; "" for anyone else.
        /// </summary>
        public string Portrait(string who)
        {
            var row = Row(who);
            if (row != null)
                return row.portrait.Length > 0 ? row.portrait : row.standIn;
            string[] parts = (who ?? "").Split('.');
            return parts.Length == 3 && int.TryParse(parts[2], out _) ? "portrait/role." + parts[1] + ".png" : "";
        }

        /// <summary>"Aldgrim the toll man", and "at Dorestat" when she does not lie there.</summary>
        public string Said(string person, string lyingAt)
        {
            var row = Row(person);
            if (row == null)
                return "";
            string who = row.title.Length > 0 ? row.name + " " + row.title : row.name;
            var port = GameData.Port(row.port);
            return port != null && row.port != lyingAt ? who + " at " + port.name : who;
        }

        /// <summary>
        /// The person of the data for a card's actor, as the encounters know people (NpcRegistry.named): their id is the
        /// places file's id, so what they feel and remember hangs on the same person the People here list shows.
        /// </summary>
        public static Npc Find(RoleTable roles, string card, string port, string role, int year, bool fixedRole)
        {
            var row = Current.Speaker(card, port, role, year, fixedRole);
            var p = row != null ? PlaceBook.Current.Person(row.person) : null;
            if (p == null || !p.Here(year))
                return null;
            int ends = p.to > 0 ? p.to : 9999;
            if (p.died > 0 && p.died < ends)
                ends = p.died;
            return new Npc
            {
                id = p.id, name = p.name, people = p.people, faith = p.faith, tongue = roles != null ? roles.TongueOf(p.people) : "",
                role = p.role, homePort = p.port, sex = roles?.Role(p.role)?.sex == "f" ? "f" : "m",
                bornYear = p.born, diesYear = ends, named = true,
            };
        }

        /// <summary>Is this the name of a person of any places file? (The forge asks, so that a stranger is not given it.)</summary>
        public static bool NameTaken(string name)
        {
            var book = Current;
            if (book.names == null)
            {
                book.names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var place in PlaceBook.Current.Places)
                    foreach (var p in place.people)
                        if (p.name.Length > 0)
                            book.names.Add(p.name);
            }
            return !string.IsNullOrEmpty(name) && book.names.Contains(name);
        }

        HashSet<string> names;

        static CastBook Load()
        {
            var book = new CastBook();
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return book;
            foreach (var x in ContentJson.Rows(root, "cast"))
            {
                var r = new CastRow
                {
                    port = ContentJson.Text(x, "port"), role = ContentJson.Text(x, "role"), person = ContentJson.Text(x, "person"),
                    name = ContentJson.Text(x, "name"), title = ContentJson.Text(x, "title"), placesRole = ContentJson.Text(x, "places_role"),
                    portrait = ContentJson.Text(x, "portrait"), standIn = ContentJson.Text(x, "portrait_stand_in"), tellsAs = ContentJson.Text(x, "tells_as"),
                    from = ContentJson.Int(x, "from"), to = ContentJson.Int(x, "to"),
                    office = Json.Bool(x, "office"), alias = Json.Bool(x, "alias"), tellsPrices = Json.Bool(x, "tells_prices"),
                };
                if (r.port.Length == 0 || r.role.Length == 0 || r.person.Length == 0)
                    continue;
                if (PlaceBook.Current.Person(r.person) == null)
                {
                    ContentFiles.Warn("cast:" + r.person, $"{File} names {r.person}, and no places file has that person: the row is skipped");
                    continue;
                }
                r.atFrom = r.from;
                r.atTo = r.to;
                if (x.TryGetValue("at_port", out object at) && at is List<object> span && span.Count == 2 && span[0] is double a && span[1] is double b)
                {
                    r.atFrom = (int)a;
                    r.atTo = (int)b;
                }
                if (r.to >= 9999)
                    r.to = 0;
                if (r.atTo >= 9999)
                    r.atTo = 0;
                if (x.TryGetValue("tells_goods", out object goods))
                {
                    r.tellsAll = goods is string s && s == "all";
                    if (goods is List<object> ids)
                        foreach (object id in ids)
                            if (id is string g)
                                r.tellsGoods.Add(g);
                }
                foreach (string c in ContentJson.Strings(x, "cards"))
                    r.cards.Add(c);
                foreach (string c in ContentJson.Strings(x, "cards_later"))
                    r.cards.Add(c);
                book.rows.Add(r);
                Put(book.byPart, r.port + "/" + r.role, r);
                Put(book.byPerson, r.person, r);
                Put(book.byPort, r.port, r);
            }
            return book;
        }

        static void Put(Dictionary<string, List<CastRow>> into, string key, CastRow r)
        {
            if (!into.TryGetValue(key, out var l))
                into[key] = l = new List<CastRow>();
            l.Add(r);
        }
    }
}
```

`Assets/Kapman/Code/Game/Known/Told.cs`

```csharp
// 7 Oct 2026: A TELLER'S WORD, TRUE OR STALE (Design/fixes/one_cast.md, part 6). When the merchant sees a market, every
// price a named person told of it is held against what it pays now, once, and the teller's record is kept.
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>What came of one told word when the market was seen.</summary>
    public sealed class ToldVerdict
    {
        /// <summary>Who told it (a places file's person id), the market it was of, the good.</summary>
        public string person = "", port = "", good = "";
        /// <summary>How the market was said to pay and how it paid when seen: +1 well, 0 the usual, -1 poorly.</summary>
        public int told, found;
        /// <summary>The day the price was true, the day it was told, the day the market was seen (game days).</summary>
        public double trueDay, toldDay, seenDay;
        public bool Held => told == found;
    }

    public static partial class Known
    {
        const int VerdictsKept = 200, GoodsJudged = 3;

        static readonly List<ToldVerdict> verdicts = new List<ToldVerdict>();
        static readonly Dictionary<string, (int held, int stale)> trust = new Dictionary<string, (int, int)>(StringComparer.Ordinal);

        /// <summary>The verdicts so far, oldest first (the last 200).</summary>
        public static IReadOnlyList<ToldVerdict> Verdicts => verdicts;

        /// <summary>How often a person's word has held and how often it was stale when the market was seen.</summary>
        public static (int held, int stale) Trust(string person) => person != null && trust.TryGetValue(person, out var t) ? t : (0, 0);

        /// <summary>"His word has held 3 times in 4." ("" while nothing of theirs has been put to the proof).</summary>
        public static string TrustLine(string person)
        {
            var (held, stale) = Trust(person);
            int all = held + stale;
            if (all == 0)
                return "";
            return all == 1 ? (held == 1 ? "Their word held the one time you put it to the proof." : "Their word was stale the one time you put it to the proof.")
                            : $"Their word has held {held} times in {all}.";
        }

        /// <summary>
        /// The market is seen: each note of it that a NAMED person told, not yet judged and not older than the days a
        /// word is kept (knowing_793.json "known_days"), is held against what the market pays now. Only what the word
        /// was for is judged: the goods of it she has aboard, else the goods it leaned on (well or poorly), three at
        /// most, and only goods of the teller's own trade. The word HELD if the market pays as it was said to (well,
        /// the usual, poorly); else it was STALE. Each note is judged once. Counts game days only, so it is the same at
        /// every speed of the clock. Returns the verdicts of this look (the caller says them in the log).
        /// </summary>
        public static List<ToldVerdict> JudgeTold(Market m, double day, int month)
        {
            var now = new List<ToldVerdict>();
            if (m == null || m.port == null || !Trade.Started)
                return now;
            var cast = CastBook.Current;
            foreach (var n in Trade.Ledger.History(m.port.id))
            {
                if (n.judged || n.how == How.Seen || n.how == How.Own || n.learned > day)
                    continue;
                var teller = cast.Row(n.teller);
                if (teller == null)
                    continue;                                            // (talk with no name to it is not put to the proof)
                n.judged = true;
                if (day - n.day >= Rules.wordKnownDays)
                    continue;
                var mine = new List<ToldVerdict>();
                var leaning = new List<ToldVerdict>();
                foreach (var g in m.Goods)
                {
                    if (!n.prices.TryGetValue(g.good.id, out var was) || !teller.Tells(g.good.id))
                        continue;
                    PaysKey(was.pays, g.good, out int told);
                    PaysKey(m.Pays(g, month), g.good, out int found);
                    var v = new ToldVerdict
                    {
                        person = teller.person, port = m.port.id, good = g.good.id, told = told, found = found,
                        trueDay = n.day, toldDay = n.learned, seenDay = day,
                    };
                    if (Trade.Hold.Count(g.good.id) > 0)
                        mine.Add(v);
                    else if (told != 0)
                        leaning.Add(v);
                }
                var judged = mine.Count > 0 ? mine : leaning;
                for (int i = 0; i < judged.Count && i < GoodsJudged; i++)
                {
                    var v = judged[i];
                    now.Add(v);
                    verdicts.Add(v);
                    var t = Trust(v.person);
                    trust[v.person] = v.Held ? (t.held + 1, t.stale) : (t.held, t.stale + 1);
                }
            }
            while (verdicts.Count > VerdictsKept)
                verdicts.RemoveAt(0);
            if (now.Count > 0)
                Version++;
            return now;
        }

        /// <summary>
        /// A verdict in words: "Aldgrim the toll man at Dorestat said Lundenwic paid well for Rhenish wine. It pays the
        /// usual. That was last week." The words are wording_793.json "word" keys told.held and told.stale (the lines
        /// here stand in until the writer has them).
        /// </summary>
        public static string Say(ToldVerdict v, string lyingAt)
        {
            var port = GameData.Port(v.port);
            var good = GameData.Good(v.good);
            string who = CastBook.Current.Said(v.person, lyingAt);
            string Pays(int tilt) => Wording.WordText(port, tilt > 0 ? "pays.well" : tilt < 0 ? "pays.poorly" : "pays.fair");
            string line = Wording.WordText(port, v.Held ? "told.held" : "told.stale");
            if (line.Length == 0)
                line = v.Held ? "{Person} said {port} paid {pays} for {good}, and it does."
                              : "{Person} said {port} paid {pays} for {good}. It pays {now}. That was {age}.";
            return Wording.Fill(line, ("person", who), ("port", port != null ? port.name : ""), ("pays", Pays(v.told)), ("now", Pays(v.found)),
                                ("good", GoodName(good)), ("age", Wording.WordText(port, AgeKey(v.trueDay, v.seenDay))));
        }

        /// <summary>
        /// ONE ASKING OF ONE PERSON ("Ask what the markets pay"): the freshest market on this port's waterfront that
        /// the merchant has no word of as new, told by this person, and only of the goods of their trade. It goes into
        /// the price notes with their id as the teller. Returns what they said in one line ("Lundenwic, last week: well
        /// for Rhenish wine and wax, poorly for quernstones."), or "" when they have nothing new to tell.
        /// </summary>
        public static string AskPrices(PersonDef p)
        {
            var game = KapmanGame.Current;
            var w = game != null ? game.World : null;
            var port = p != null ? GameData.Port(p.port) : null;
            var row = p != null ? CastBook.Current.Row(p.id) : null;
            if (w == null || port == null || row == null || !row.tellsPrices || !Trade.Started)
                return "";
            Kapman.Game.Sim.PriceSnapshot best = null;
            foreach (var snap in w.Board(port.index).All)
            {
                if (snap.port == port.index || snap.port < 0 || snap.port >= GameData.Ports.Count)
                    continue;
                var latest = Trade.Ledger.Latest(GameData.Ports[snap.port].id);
                if (latest != null && latest.day >= snap.day - 1e-6)
                    continue;                                            // (she has word as new as this, or newer)
                if (best == null || snap.day > best.day)
                    best = snap;
            }
            if (best == null)
                return "";
            var of = GameData.Ports[best.port];
            if (!Trade.Ledger.Hear(best, "told by " + p.name + " at " + port.name, How.Talk, 1, game.Day, p.id, port.id,
                                   row.tellsAll ? null : (Func<string, bool>)row.Tells))
                return "";
            HearOf(of);
            var note = Trade.Ledger.Latest(of.id);
            var well = new List<string>();
            var poorly = new List<string>();
            foreach (var kv in note.prices)
            {
                var good = GameData.Good(kv.Key);
                PaysKey(kv.Value.pays, good, out int tilt);
                if (tilt > 0 && well.Count < 3)
                    well.Add(GoodName(good));
                else if (tilt < 0 && poorly.Count < 3)
                    poorly.Add(GoodName(good));
            }
            string age = Wording.WordText(of, AgeKey(note.day, game.Day));
            if (well.Count == 0 && poorly.Count == 0)
                return $"{of.name}, {age}: the usual for all I know of.";
            string said = well.Count > 0 ? "well for " + string.Join(" and ", well) : "";
            if (poorly.Count > 0)
                said += (said.Length > 0 ? ", " : "") + "poorly for " + string.Join(" and ", poorly);
            return $"{of.name}, {age}: {said}.";
        }

        /// <summary>The verdicts and the tellers' records as plain data (a saved game).</summary>
        public static Dictionary<string, object> CaptureTold()
        {
            var list = new List<object>();
            foreach (var v in verdicts)
                list.Add(new List<object> { v.person, v.port, v.good, (double)v.told, (double)v.found, v.trueDay, v.toldDay, v.seenDay });
            var kept = new Dictionary<string, object>();
            foreach (var kv in trust)
                kept[kv.Key] = new List<object> { (double)kv.Value.held, (double)kv.Value.stale };
            return new Dictionary<string, object> { ["verdicts"] = list, ["trust"] = kept };
        }

        /// <summary>A loaded game's verdicts (null, an older save: none yet).</summary>
        public static void RestoreTold(object saved)
        {
            verdicts.Clear();
            trust.Clear();
            foreach (object o in Json.Arr(saved, "verdicts") ?? new List<object>())
                if (o is List<object> l && l.Count >= 8 && l[0] is string person && l[1] is string port && l[2] is string good)
                    verdicts.Add(new ToldVerdict
                    {
                        person = person, port = port, good = good, told = (int)(double)l[3], found = (int)(double)l[4],
                        trueDay = (double)l[5], toldDay = (double)l[6], seenDay = (double)l[7],
                    });
            foreach (var kv in Json.Obj(saved, "trust") ?? new Dictionary<string, object>())
                if (kv.Value is List<object> t && t.Count >= 2)
                    trust[kv.Key] = ((int)(double)t[0], (int)(double)t[1]);
        }
    }
}
```

**The edits.** Each is the old lines as they stand (they are in the file once and only once: the check refuses an
edit that does not fit), then the new lines. The line number is where the old lines begin in the file as it stood.

`Assets/Kapman/Code/Core/Encounters/NpcRegistry.cs`

Edit 1, line 16: a mark on a person of the data.

```csharp
// old
        public bool alive = true;
        public double lastSeenDay;
```

```csharp
// new
        public bool alive = true;
        /// <summary>A person of the places files (the one cast), not a forged one: their years are the data's.</summary>
        public bool named;
        public double lastSeenDay;
```

Edit 2, line 254: the holder of an office is the cast's person where there is one.

```csharp
// old
        public Npc Holder(RoleSpec spec, PortFacts port, int year, double day)
        {
            string key = port.id + "/" + spec.role;
```

```csharp
// new
        public Npc Holder(RoleSpec spec, PortFacts port, int year, double day, string card = null)
        {
            var cast = Named(spec, port, year, day, card);
            if (cast != null)
                return cast;
            string key = port.id + "/" + spec.role;
```

Edit 3, line 269: the hook and the method.

```csharp
// old
        Npc Seen(Npc n, string port, double day)
        {
```

```csharp
// new
        /// <summary>THE ONE CAST: who of the data is this part of this card at this port this year (card id, port, role,
        /// year, is the actor fixed), or null. Set by the game (CastBook.Find); null in a test without one.</summary>
        public Func<string, string, string, int, bool, Npc> named;

        /// <summary>Is this the name of a person of the data (the forge then draws again)? Set by the game; null: no.</summary>
        public Func<string, bool> nameTaken;

        /// <summary>The person of the data for a card's actor, or null (nobody written: the forge makes one). The first
        /// time they are met they are taken into the registry under their own id; for a fixed role they become its
        /// holder, and a forged man who held it before (an older save, a year nobody was written for) hands over what
        /// was felt toward him.</summary>
        public Npc Named(RoleSpec spec, PortFacts port, int year, double day, string card = null)
        {
            var who = named != null ? named(card ?? "", port.id, spec.role, year, spec.fixedRole) : null;
            if (who == null)
                return null;
            bool met = npcs.TryGetValue(who.id, out var have);
            if (met)
                who = have;
            else
                npcs[who.id] = who;
            if (spec.fixedRole)
            {
                string key = port.id + "/" + spec.role;
                Npc was = holders.TryGetValue(key, out var id) ? Get(id) : null;
                if (was != null && was.id != who.id && !was.named)
                {
                    was.alive = false;
                    if (!met)
                        Inherit(was, who);
                }
                holders[key] = who.id;
            }
            return Seen(who, port.id, day);
        }

        Npc Seen(Npc n, string port, double day)
        {
```

Edit 4, line 414: saved.

```csharp
// old
["generation"] = n.generation, ["alive"] = n.alive, ["seen"] = n.lastSeenDay, ["seen_at"] = n.lastSeenPort,
```

```csharp
// new
["generation"] = n.generation, ["alive"] = n.alive, ["seen"] = n.lastSeenDay, ["seen_at"] = n.lastSeenPort, ["named"] = n.named,
```

Edit 5, line 455: loaded.

```csharp
// old
alive = Json.Bool(o, "alive", true), lastSeenDay = Json.Num(o, "seen"), lastSeenPort = Json.Str(o, "seen_at"),
```

```csharp
// new
alive = Json.Bool(o, "alive", true), lastSeenDay = Json.Num(o, "seen"), lastSeenPort = Json.Str(o, "seen_at"), named = Json.Bool(o, "named"),
```

Edit 6, line 301: the forge leaves the cast's names alone.

```csharp
// old
            npc.name = names.Pick(people, npc.sex, ref rnd);
```

```csharp
// new
            npc.name = names.Pick(people, npc.sex, ref rnd);
            // (a forged person does not bear the name of a person of the places files: 75 of the pool's 233 names are theirs)
            for (int tries = 0; tries < 8 && nameTaken != null && nameTaken(npc.name); tries++)
                npc.name = names.Pick(people, npc.sex, ref rnd);
```

`Assets/Kapman/Code/Core/Encounters/EncounterSession.cs`

Edit 7, line 149: the card's id goes with the asking, and a passing part the card's note names is that person.

```csharp
// old
                s.actors[spec.key] = kept ?? (spec.fixedRole ? world.npcs.Holder(spec, world.port, world.year, world.day)
                                                             : world.npcs.Forge(spec, world.port, world.year));
```

```csharp
// new
                s.actors[spec.key] = kept ?? (spec.fixedRole ? world.npcs.Holder(spec, world.port, world.year, world.day, card.id)
                                                             : world.npcs.Named(spec, world.port, world.year, world.day, card.id)
                                                               ?? world.npcs.Forge(spec, world.port, world.year));
```

`Assets/Kapman/Code/Game/Encounters/EncounterDirector.cs`

Edit 8, line 212: the game gives the registry its cast.

```csharp
// old
            this.npcs = npcs ?? new NpcRegistry(seed, book.roles, book.names);
```

```csharp
// new
            this.npcs = npcs ?? new NpcRegistry(seed, book.roles, book.names);
            if (this.npcs.named == null)                                  // THE ONE CAST (Design/fixes/one_cast.md)
                this.npcs.named = (card, port, role, year, fixedRole) => CastBook.Find(book.roles, card, port, role, year, fixedRole);
            if (this.npcs.nameTaken == null)
                this.npcs.nameTaken = CastBook.NameTaken;
```

Edit 9, line 504: an effect that names a role names the same person.

```csharp
// old
                                actors[v.a] = npcs.Holder(new RoleSpec { key = v.a, role = v.a }, facts, Year, Day);
```

```csharp
// new
                                actors[v.a] = npcs.Holder(new RoleSpec { key = v.a, role = v.a }, facts, Year, Day, id);
```

`Assets/Kapman/Code/Game/Known/Word.cs`

Edit 10, line 270: the price row names the person.

```csharp
// old
            if (n == null)
                return Wording.WordText(null, "person.someone");
            var of = GameData.Port(n.tellerOf);
```

```csharp
// new
            if (n == null)
                return Wording.WordText(null, "person.someone");
            // THE ONE CAST: a teller noted by their id is that person, by name ("Aldgrim the toll man", "... at Dorestat")
            string named = CastBook.Current.Said(n.teller, Port.LyingId());
            if (named.Length > 0)
                return named;
            var of = GameData.Port(n.tellerOf);
```

`Assets/Kapman/Code/Game/Known/PriceNotes.cs`

Edit 11, line 29: a told note is judged once.

```csharp
// old
        public string teller, tellerOf;
```

```csharp
// new
        public string teller, tellerOf;
        /// <summary>Held against the market when it was seen (Known.JudgeTold): once.</summary>
        public bool judged;
```

Edit 12, line 83: a teller may tell only the goods of their trade.

```csharp
// old
        public bool Hear(Kapman.Game.Sim.PriceSnapshot snap, string source, How how, int hops, double learned, string teller = null, string tellerOf = null)
```

```csharp
// new
        public bool Hear(Kapman.Game.Sim.PriceSnapshot snap, string source, How how, int hops, double learned, string teller = null, string tellerOf = null,
                         Func<string, bool> only = null)
```

Edit 13, line 99: the goods of the trade.

```csharp
// old
                if (float.IsNaN(snap.pays[i]))
                    continue;
```

```csharp
// new
                if (float.IsNaN(snap.pays[i]) || (only != null && !only(GameData.Goods[i].id)))
                    continue;
```

Edit 14, line 105: an empty note is not kept.

```csharp
// old
            Insert(list, note);
            return list[list.Count - 1] == note;
```

```csharp
// new
            if (note.prices.Count == 0)
                return false;                                            // (nothing of their trade is dealt in there)
            Insert(list, note);
            return list[list.Count - 1] == note;
```

Edit 15, line 185: saved.

```csharp
// old
                    if (!string.IsNullOrEmpty(n.tellerOf))
                        d["teller_of"] = n.tellerOf;
```

```csharp
// new
                    if (!string.IsNullOrEmpty(n.tellerOf))
                        d["teller_of"] = n.tellerOf;
                    if (n.judged)
                        d["judged"] = true;
```

Edit 16, line 204: loaded.

```csharp
// old
                    teller = Json.Str(x, "teller", null), tellerOf = Json.Str(x, "teller_of", null),
```

```csharp
// new
                    teller = Json.Str(x, "teller", null), tellerOf = Json.Str(x, "teller_of", null), judged = Json.Bool(x, "judged"),
```

`Assets/Kapman/Code/Game/Ports/PortLife.cs`

Edit 17, line 77: Talk to people: each price has a named teller.

```csharp
// old
                    if (Trade.Ledger.Hear(snap, (witness ? "told by the master who saw them, at " : "talk on the waterfront at ") + port.name,
                                          witness ? How.Told : How.Talk, witness ? 0 : 1, today, witness ? "skipper" : "quay", tellerOf))
```

```csharp
// new
                    // THE ONE CAST: talk on the waterfront is somebody's: one of the port's people who hear everything
                    var who = witness ? null : CastBook.Current.Teller(port.id, game.Year, GameData.Ports[snap.port].id, snap.day, true);
                    if (Trade.Ledger.Hear(snap, (witness ? "told by the master who saw them, at " : who != null ? "told by " + who.name + " at " : "talk on the waterfront at ") + port.name,
                                          witness ? How.Told : How.Talk, witness ? 0 : 1, today, witness ? "skipper" : who != null ? who.person : "quay", tellerOf))
```

`Assets/Kapman/Code/Game/KapmanGame.cs`

Edit 18, line 374: the first morning's prices have named tellers.

```csharp
// old
            foreach (var snap in board.All)
                if (Trade.Ledger.Hear(snap, "talk on the waterfront at " + home.name, How.Talk, 1, Day, "quay", home.id))
                    Known.HearOf(GameData.Ports[snap.port]);
```

```csharp
// new
            foreach (var snap in board.All)
            {
                // THE ONE CAST: what the home waterfront knows was told by somebody: one of its people who hear everything
                var who = snap.port >= 0 && snap.port < GameData.Ports.Count ? CastBook.Current.Teller(home.id, Year, GameData.Ports[snap.port].id, snap.day, true) : null;
                if (Trade.Ledger.Hear(snap, who != null ? "told by " + who.name + " at " + home.name : "talk on the waterfront at " + home.name,
                                      How.Talk, 1, Day, who != null ? who.person : "quay", home.id))
                    Known.HearOf(GameData.Ports[snap.port]);
            }
```

`Assets/Kapman/Code/Game/Known/Known.cs`

Edit 19, line 468: the verdicts are saved.

```csharp
// old
            return new Dictionary<string, object> { ["next"] = nextId, ["facts"] = list, ["spoken"] = spokenList, ["heard_of"] = heard, ["standing"] = stand };
```

```csharp
// new
            return new Dictionary<string, object> { ["next"] = nextId, ["facts"] = list, ["spoken"] = spokenList, ["heard_of"] = heard, ["standing"] = stand,
                                                    ["told"] = CaptureTold() };
```

Edit 20, line 473: and loaded.

```csharp
// old
            Clear();
            if (d == null)
                return;
            nextId = (int)Json.Num(d, "next");
```

```csharp
// new
            Clear();
            if (d == null)
                return;
            RestoreTold(Json.Obj(d, "told"));
            nextId = (int)Json.Num(d, "next");
```

Edit 21, line 538: and forgotten with the rest.

```csharp
// old
            standing.Clear();
            nextId = 0;
            Version++;
```

```csharp
// new
            standing.Clear();
            RestoreTold(null);
            nextId = 0;
            Version++;
```

`Assets/Kapman/Code/Game/UI/Frame/FrameDrawn.cs`

Edit 22, line 130: whoever speaks has their slot.

```csharp
// old
                Slot = who == "you" ? FrameArt.Self(people != null ? people.people : "frisian", false) : "";
```

```csharp
// new
                Slot = who == "you" ? FrameArt.Self(people != null ? people.people : "frisian", false) : CastBook.Current.Portrait(who);
```

`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`

Edit 23, line 396: the proof at the mooring.

```csharp
// old
        void Workspace()
        {
```

```csharp
// new
        /// <summary>She has moored: the market is seen. Every price a named person told of it is held against it, once,
        /// and what came of each is said in the log; then its prices go in the notes as seen.</summary>
        void ProveTold()
        {
            var here = FrameGame.PortHere;
            var game = KapmanGame.Current;
            if (here == null || game == null || !Trade.Started)
                return;
            var market = Markets.Of(here, game.Year, game.Day);
            if (market == null)
                return;
            foreach (var v in Known.JudgeTold(market, game.Day, game.Month))
                FrameLog.Add(Known.Say(v, here.id));
            Trade.Ledger.Note(market, game.Day, game.Month);
        }

        void Workspace()
        {
```

Edit 24, line 407: called where the frame sees she has moored.

```csharp
// old
            lyingKnown = true;
            wasLying = lying;
            pick = -1;
```

```csharp
// new
            lyingKnown = true;
            wasLying = lying;
            if (lying)
                ProveTold();
            pick = -1;
```

`Assets/Kapman/Code/Game/UI/Frame/Panels/PeoplePanel.cs`

Edit 25, line 24: the list is drawn again when a word has been proved.

```csharp
// old
            return FrameUi.Mix(h, Self.Version);
```

```csharp
// new
            return FrameUi.Mix(FrameUi.Mix(h, Self.Version), Known.Version);
```

Edit 26, line 53: each person's record beside their name.

```csharp
// old
                string small = PersonTalk.Where(p, book.SiteOf(p, year)) + (met != null ? ". You have spoken." : "");
```

```csharp
// new
                string trust = Known.TrustLine(p.id);
                string small = PersonTalk.Where(p, book.SiteOf(p, year)) + (met != null ? ". You have spoken." : "") + (trust.Length > 0 ? " " + trust : "");
```

`Assets/Kapman/Code/Game/UI/Frame/PersonTalk.cs`

Edit 27, line 62: a person who tells prices can be asked for them.

```csharp
// old
            foreach (string k in p.keys)
                if (!IsGreeting(k) && k != "farewell" && !keys.Contains(k))
                    keys.Add(k);
            return keys;
```

```csharp
// new
            foreach (string k in p.keys)
                if (!IsGreeting(k) && k != "farewell" && !keys.Contains(k))
                    keys.Add(k);
            var cast = CastBook.Current.Row(p.id);
            if (cast != null && cast.tellsPrices)
                keys.Insert(0, PricesKey);                                // (what the markets pay: the first thing a merchant asks)
            return keys;
```

Edit 28, line 26: its key.

```csharp
// old
        static readonly Dictionary<string, int> picks = new Dictionary<string, int>(StringComparer.Ordinal);
```

```csharp
// new
        /// <summary>The asking that is no written line: one market's prices from the waterfront, by this person's trade.</summary>
        public const string PricesKey = "prices";

        static readonly Dictionary<string, int> picks = new Dictionary<string, int>(StringComparer.Ordinal);
```

Edit 29, line 20: its words.

```csharp
// old
            ("rumour", "Ask what is said here"),
```

```csharp
// new
            (PricesKey, "Ask what the markets pay"),
            ("rumour", "Ask what is said here"),
```

Edit 30, line 92: what they told, when prices were asked for.

```csharp
// old
                                     int year = 793, bool metBefore = false)
```

```csharp
// new
                                     int year = 793, bool metBefore = false, string told = null)
```

Edit 31, line 109: the line is the price told.

```csharp
// old
            string line = p.Line(key, pick);
```

```csharp
// new
            string line = key == PricesKey ? (told == null || told.Length == 0 ? "Nothing you have not heard." : told) : p.Line(key, pick);
```

Edit 32, line 129: prices can be asked for again: the next market.

```csharp
// old
                if (k == key && p.lines[k].Length < 2)
                    continue;
```

```csharp
// new
                if (k == key && k != PricesKey && p.lines[k].Length < 2)
                    continue;
```

Edit 33, line 175: asking hands over one market's prices with the person's name on them.

```csharp
// old
            frame.OpenFloat(Spec(p, PlaceBook.Current.SiteOf(p, year), GameData.Port(p.port), key, n,
                                 k => Ask(frame, p, k), () => frame.CloseFloat(FloatId(p)), year, true));
```

```csharp
// new
            string told = key == PricesKey ? Known.AskPrices(p) : null;
            if (!string.IsNullOrEmpty(told))
                FrameLog.Add($"{(p.name.Length > 0 ? p.name : "The " + p.Role)}: {told}");
            frame.OpenFloat(Spec(p, PlaceBook.Current.SiteOf(p, year), GameData.Port(p.port), key, n,
                                 k => Ask(frame, p, k), () => frame.CloseFloat(FloatId(p)), year, true, told));
```


**What will change in the tests** (I could not run them): `Assets/Kapman/Tests/EditMode/EncounterRunnerTests.cs`
line 90 expects the id `dorestad.tollman.0`; with the cast file in place it is `dorestad.tollman`, and his name is
Aldgrim. `KnownWordTests.cs` (lines 198 to 215, 420 to 421) tells prices as "quay" and still passes: a note with no
person reads as before.

**Not in the draft, said plainly:**

- The line "Prices heard for 14 places, now in your ledger" (`PortLife.cs` line 85) still gives every market in one
  click. With the draft each of those prices has a named teller, but the click is the same. The critic's wish is
  that prices come one asking at a time from a person. That is a choice about how much a player learns per call, so
  it is the user's: either the row goes (the port's graph, `Code/Graphs/PortNodes.cs` lines 150 to 166, and the
  action `act:ask_news` in the places files) and "Ask what the markets pay" is the only way, or it stays.
- The speaker's line on a card reads "Aldgrim, the toll man · easy" with the draft. The words critic dislikes "easy"
  there; that is the card wording pass's.
- An alias shows the person's own title: at Sarre a card that asks for the reeve shows "Dunwalh, the toll man".
- People here still lists Ravn, Aldger and Brunger ashore (clash 11).
- If the art loader of the queue (`Design/art_loader/PLAN.md` step 1) is in first, give it
  `CastBook.Current.Portrait(id)` where it asks for a speaker's slot, in place of the edit to `FrameDrawn.cs`.

## 6. How a named teller's word is marked true or stale when she moors

**The rule.** A told price is never a number: the player was told "well", "the usual" or "poorly"
(`Known.PaysKey`: at or over 1.15 of the good's table value, at or under 0.85, between; `knowing_793.json`, `word`).
When she moors, the market is seen. Each note of that market that a **named** person told, and that has not been
judged, is held against what the market pays that day:

- the word **held** if the market pays as it was said to (the same of the three words);
- the word was **stale** if it does not.

Only what the word was for is judged: of the goods in the note, those she has aboard; if she has none of them, the
goods the word leaned on (well or poorly); three goods at most, and only goods of the teller's own trade. Each note
is judged once (`PriceNote.judged`, saved). A note older than 90 game days is marked judged and not counted.

**What the player sees.**

- A line in the log at the mooring, one for each good judged. Held: "Aldgrim the toll man at Dorestat said
  Lundenwic paid well for Rhenish wine, and it does." Stale: "Aldgrim the toll man at Dorestat said Lundenwic paid
  well for Rhenish wine. It pays the usual. That was last week." The last sentence is the age of the price on the
  day she moored, in the age words the game already has (`Known.AgeKey`). That is the critic's sentence ("Aldgrim
  told me wine paid well at Lundenwic, and Aldgrim was a week behind"), made by the game. The two lines are wording
  keys (`told.held`, `told.stale` under `word` in `wording_793.json`) with stand-ins in the code until the user
  writes them.
- A record on the person, kept and saved: how often their word held and how often it was stale. People here shows
  it beside the name. So a player learns whom to believe: the toll man hears everything a week late, a master who
  saw the market himself is fresh.
- The mark on the row. Once she has seen the market the row there reads "You saw ..." (rule 2 of the word on the
  item), so the told line is gone from that row; the verdict lives in the log and on the person. In the ledger
  window (`Panels/WordPanel.cs`, about line 82, where a note's kind is written) a judged note should read "held" or
  "stale" after its kind. I have not drafted that line.

**Where it happens.** `FrameRoot.Workspace` (`Code/Game/UI/Frame/FrameRoot.cs`, about line 397) is where the frame
sees that she has moored or cast off. The draft calls `ProveTold` there: it judges, says the lines, and then notes
the market's prices as seen. Loading a game is not a mooring (the frame already tells the two apart), so nothing is
judged twice.

**The clock (ruling 26).** The critics wrote before the five speeds. "A week behind" was 2.8 real hours at the old
1x; at speed 4 a week is about 105 seconds, and a told word (14 game days by `told_days`) lasts three and a half
minutes of real time. That is right as it stands, because everything here counts game days and nothing counts real
time: the ages, the 14 and 90 days, the day a note was true. It also means the proof must ask nothing of the
player: it happens by itself at the mooring, it is a line in the log and never a card, so it does not hold the
clock (ruling 5), and the clock has already dropped for making port (ruling 26). The ruling's test, the same seed
run at speed 1 and at speed 5, should compare `Known.Verdicts` as well: the same notes must be judged the same.

**Tests to write** (none written):

1. With the cast file, the card `harbour.tollman.frankish` at Dorestat in 793 has the actor `dorestad.tollman`,
   named Aldgrim; in 811 (after his years) a forged man who has taken on half of what was felt toward Aldgrim.
2. The same card at Traiectum has a forged man whose name is no places person's.
3. A new game's first prices each have a teller who is a person of Dorestat there in 793, and the same person
   after a save and a load.
4. A note told by `dorestad.tollman` of Lundenwic, wine "well"; she moors at Lundenwic with wine aboard while it
   pays the usual: one verdict, stale; the note is judged; a second mooring adds none.
5. "Ask what the markets pay" of a weaver gives a note with cloth, wool and dye only, and nothing when she already
   has word as new.
6. Every row of the cast file names a person of a places file, and no two rows of one office overlap in years.

## 7. What a builder does, in order

1. Copy `Design/checks/cast_793.json` to `Assets/Kapman/Game/Data/story/cast_793.json`.
2. Add the two new files; make the edits of part 5; compile; run the tests; change the expectation of
   `EncounterRunnerTests.cs` line 90.
3. Play: moor at Dorestat, open the toll card (Aldgrim, with his slot's placeholder), open People here (Aldgrim),
   ask him what the markets pay, read a price row (Aldgrim the toll man said ...), sail to Traiectum, read the log.
4. For the user to read: the by-hand rows (clash 5), the lists of who tells what (part 4), the three toll men to
   write (clash 15), Gerbrand's year (clash 7).

## 8. How the cast file was made, and what I could not check

`cast_793.json` was written by a script kept outside the project. It imports `Bake/content/card_rules.py` (read
only: nothing under `Bake` was written) for which cards can open in the release and at which ports, and reads the
places files, `npc_roles_793.json`, `goods_793.json`, the spine and `Art/art_slots.json`. For each actor of each
card at each port it finds the person in this order: an id in the actor's note (`dorestad.ravn`, `cast.thiadbod`);
a name at the start of the note that is a person of that port; an actor's key that is a person; for a fixed actor,
the holder of the office in the card's years. What the file holds: `cast` (131 rows, 86 of them in 793
to 805), `gaps`, `aboard`, `visitors` (a named person met away from home), `strangers`, `clashes`, `counts`.

Not checked, or weak:

- I did not run Unity, the tests or the game. The compile was done outside Unity with Unity's own compiler and the
  argument files of its last compile, against the sources as they stood at about 12:40; builders have changed files
  since my first reading and may change them again.
- Which name the forge draws for a given office (I took "Liudbrand" from the critic's capture).
- A card's years are read from its plain `year` terms; a card whose `when` has an "or" is taken to span all years
  (the rule of `card_rules.py`).
- "Can open in the release" is the census's reckoning, not a play-through.
- The lists of goods each person tells of are my reading of their `knows` lines and roles.
- The content pass now at work on the cards (who speaks, starts by people) may move a card from one person to
  another; the script should be run again after it. The cast's rows for offices do not depend on it; the `cards`
  lists do.
