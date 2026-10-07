<!-- 6 Oct 2026. Decided from the eight studies in Design/research/depth, with ui_direction.md, scale_to_fleets_plan.md,
living_world.md, town_interactivity.md, CHAINS.md and ui_mock/template3.html. No web search was made for this page.
(D) = a number I judged. (?) = not checked against a source. Sizes are pixels on a 960x540 canvas (2x on a 1080p
screen). Every number is a first guess. Nothing was run in Unity. Read a second time the same day: see the last section. -->

# The depth design: the frame, and the game behind it

**In one line.** A thick frame of fixed places round the chart, and behind it four things that feed each other:
**the word** (what you know and how old it is), **days** (the only fuel), **ties** (few, named, public, inherited)
and **the name** (what the house kept its word on).

**What this overrules.** `ui_direction.md` banned a frame, buttons, tabs and a log. Your ask of 6 October brings
them back. Kept from that page: the lens (hold a thing and the chart writes about it), words before figures, a
hover that says what a thing is, Byzantium only for what is yours. Dates and hours are plain ("21 June 793,
morning"), because you called period labels "obnoxious"; a feast's name is in the tooltip.

**Numbers are for the code.** Every share, chance and point below is a line in a rules file. The player is shown a
word (sure, likely, doubtful; pays well, fair, thin), a mark, or a thing in the picture. No percentage, no point
of feeling and no chance is ever printed on the screen.

## 1. The HUD

### 1.1 The frame

Layout E of the mockup with "map shows through", in the thick rounded cut. The frame is drawn as pieces round each
thing that is there. Where nothing is, the chart runs to the rim and takes clicks. **The places are thick, the rim
is thin:** each place has a 6 px bevel and 10 px round corners, a sunk well a 4 px edge (D); the rim that joins
them is 4 px, so between two places you see map and not wood. Light from the top left: raised things have a bright
top and left edge, sunk wells a dark one. The wood and its edge take the colour scheme you pick in the mockup (the
plum `#3b2a3f` in the first). Text is Gentium, warm white. Pale gold is for silver and nothing else. Byzantium
`#702963` is only what is yours: her sail, your keels, today's notch.

| Place | Where | Size | Share | Holds |
|---|---|---|---|---|
| Rim | all four edges | 4 px, a boss at each corner | 2.3% | joins the places into one frame |
| **Settings block** | top left | 236x30 | 1.4% | menu, clock, silver, days of bread. Four things, no more |
| **Button rack** | under it | 236 wide, 1 to 4 rows of 24 | 1.1% a row | two named buttons a row |
| **Panel dock** | under the rack, to the foot | 236 wide, up to 398 high | 0 to 18% | no panel, one, or two stacked |
| Day stick | top centre | 472x16 | 1.5% | the date at its left end (112 px), then 45 days ahead with their signs |
| **Portrait block** | top right | 236x150 | 6.8% | face 96x112, name, two lines, the crew's four heads |
| Speech place | under the portrait | 236x222 | 0 or 10.1% | a talk or a card. Empty means chart |
| **Ship window** | bottom right | 236x156 | 7.1% | view 224x126, one caption line |
| **Log** | bottom centre | 472x52 | 4.7% | the hint line, then three lines |

The left column is **things** (what you own and know). The right column is **people and place** (who speaks, where
she is). Answers are always clicked under the face. The middle 472x456 (42% of the screen) is never covered, except
by the winter board and a text being read. On other screen shapes the places keep their pixels and the chart gets
the rest. The frame replaces `MapTable`'s top bar and outliner. The panels that exist (`MarketPanel`, `ChestPanel`,
`EncounterPanel`, `TextPanel`) are moved into its places, not written again.

### 1.2 The settings block and the day stick

**The block,** left to right: the **menu** button (save, load, options, quit, and two switches: "Show the whole
frame", "The men keep their counsel"); the **clock** (hold, 1x, 2x, 4x, as `GameSpeed` has them; absent until the
clock can first run); **silver aboard**; **days of bread**. The date is not here: 236 pixels cannot hold it too.

**The stick** is the calendar. Its left end reads the date and the hour as a word. Then the days ahead: a notch a
day, one sign a notch at most, a line under the days your bread lasts, a paler stretch for a course's days with
dark bars for its nights. The debt is a sign on its day and the first line of Tasks.

**The clock is held** by a card, a talk, the town, the market, the chest and a text, as you decided on 4 October;
"held" is then lit. This page reads that as every panel in the dock (question 7). Held, she does not move.

### 1.3 The buttons: two at the start, eight at the most

A button exists only after the thing it opens has happened once. It arrives under the last one, with one log line,
and wears a small white count until opened. Never a greyed button. At least 90 seconds lie between two arrivals.
An heir keeps the whole rack. Keys 1 to 8 follow the order of arrival; while a talk or a card is up the same keys
pick the reply, as the port menu's digits do today.

| Key | Button | Arrives when | Usual time | What it holds |
|---|---|---|---|---|
| 1 | **Hold** | the start | 0:00 | *Cargo*: twelve lots, each with a grade notch and its age. *Boat*: what is hurt, and what mending costs |
| 2 | **Tasks** | the start | 0:00 | *Promises*: three and one vow, each eight words and a day. *Owed*: the debt, claims held on you, wergild parts |
| 3 | **Ledger** | the first word on a price | 0:05 | One list, no tabs: the word, who said it, its age as fading ink. With a good in hand it lists ports; with a port clicked, goods |
| 4 | **Crew** | a man first speaks on a card | 0:11 | *Men*: each man is his head and three sentences, never a grid of fields. *Bench* (in port only): who can be hired |
| 5 | **Chronicle** | the first news | 0:18 | *Heard* (with source and age), *The annals*, *Otherwise* (after a hinge has turned) |
| 6 | **Chest** | the first text | 0:25 | *Texts*, *Riddles*, *Years* (each past year's saga and its legs), *People* (all met, by port and bond; the dead) |
| 7 | **World** | a war or shut port first touches a course | 0:45 | One page, no tabs: who holds what, each war's phase. Click a realm and the chart shows it. Waters, gates and pilots are ink on the chart, not a list |
| 8 | **House** | the first Martinmas | hours 4 to 6 (D) | *Keels* (six at most, her sentence, the last word of her and its age), *Hearth* (eight faces at most), *Holdings*, *Stones* |

A panel opens in the dock; a second stacks under it; a third closes the oldest. Outside the market, a row in a
panel does one of two things when clicked: it turns the lens, or it puts that person in the portrait. **The House
panel shows; it does not command.** Orders are given only on the winter page or face to face (fleets plan 2.8).

**By where she is.** With no button pressed the dock is chart at sea and the **town** in port (its picture, its
sites as lines, "Cast off"). Tasks that can be done here come first. In winter the rack and dock are hidden and all
of it is on the board.

**The market, in three clicks.** It opens in the dock from a market site, with the Hold locked open under it.
*Buy:* click a good's row and one lot drops into the hold (Shift: all that fits). *Sell:* click a lot in the hold;
its one to three buyers show as lines under it ("The cellarer of Saint Martin: four casks, pays well"); click a
buyer and one lot is his. A buyer's line ends in "for silver" or "for wares", and a click swaps it (2.1). *Haggle*
(once a call) opens the Ledger at that good; click the note you want to name. This port's rows in the Ledger are
warm white: seen today. At sea a clicked lot is only the lens.

### 1.4 The portrait

It is a person and a button, and it is never empty.

| When | It shows | What can be done from it |
|---|---|---|
| At rest | you: face, name, age and health as words; from the first Martinmas, the word said of you (2.2) | click the face: the House panel (before it exists, your own slip). Hold still on the word: three lines of why |
| Always, under the face | four crew heads, 20x20, three patience pips each; an uneasy man looks away; an empty seat is empty | click a head: he takes the frame and Crew opens at him |
| A talk | the speaker; beside the face only his name, his feeling as a word, his bond marks and the tally stick of claims (top edge yours on him, bottom his on you) | in the speech place: his words; replies plain, warm or hard; a claim to call; a gift; pips for talks left. Click his face: People opens at him, with what he remembers and wants |
| At market | the buyer under the pointer: "He will draw a cup first" | the selling is done on his line in the dock |
| A card | the man the card is about, or you | the choices, in the speech place, the odds as a word |
| A sail sighted | a plain dark head until she is known | click: her slip on the chart |
| Winter | you, a year older; the heir once he is 14 | click the heir: his three traits |

Six faces (plain, glad, grim, weary, cold, hurt), a blink, a glance of 0.8 s toward what just happened, a hood in
rain, breath in frost. Faces age by decade and are built from a base head and layers.

### 1.5 The live ship window

**Rule: every pixel is driven by a number the simulation already holds. A fact with a picture here gets no icon
elsewhere, and no fact that kills lives only here.** The real 3D boat on a small stage: a 60 m sine sea, a sky
dome, three scrolling shore strips, drawn to a 224x126 point-filtered texture at 20 pictures a second, budget 1 ms
a frame. It is built from what this project has (`WaterWaves`, `SkyDome`, `SeaStateModel`, `LocalSky`, the boat
`SailingShip` shows); the tank's rig and its three camera views are copied over from the KapmanSail project. It is
where the ship's sound comes from.

| It shows | It tells you |
|---|---|
| The sky from the weather ahead of her: cirrus, a lowering sheet, a dark line to windward, swell with no wind | the early forecast, written nowhere: mares' tails come about 27 real minutes before the rain at 1x. The sign 12 hours out is always spoken by a man as well (2.3), so a shut window loses no ship |
| One of six places, read from the map each game hour: river, estuary, coast (land on the true side), open sea, port, ice | where she is, and how near a lee shore |
| How deep she rides, casks standing in her; the sail full, shaking, reefed or down; oars out | the load, and fair or foul, seen and not counted |
| Four figures: rowing, bailing, asleep, turned away, standing to point; an empty seat | the mood of the boat, and a man lost |
| A far sail at true size and bearing; a named sea-mark | whether she gains. Click a sea-mark: the blot on the chart shrinks to 2 km |
| The shot a card names (the shot kinds `ShotDirector` has), and its result before the card closes | a card needs no art of its own |
| Night as a dark-adapted eye; fog; snow on the thwarts; her on the bank in winter | the dark edge |

Clicks: a man, a far sail, a sea-mark (the nearest within 6 pixels), bare water (the next of three views). Click
the caption for 480x270 over the centre; W for the whole screen. Shut by its corner, that corner is chart and the
camera costs nothing. The caption is words: "The Kromme Rijn below Dorestat. West, a fair breeze."

### 1.6 The log, the hint line, tooltips

Everything that happens is one log line. Its **rank**, set in data, decides how loudly it is told.

| Rank | What | Told as | Clock | Limit |
|---|---|---|---|---|
| 1 | a fact: the wind veered, a price heard | a log line | runs | none |
| 2 | a thing to weigh: stores low, a task due in two days | a line, a count on its button, a ring on the chart | runs | 6 a game day |
| 3 | a thing coming: a gale building, the debt's eve | a short slip by her mark; the sound thins first | drops to 1x | 2 a game day |
| 4 | a choice | a card under the portrait | held | 1 to 3 a leg |

One `NoticeBus` replaces two things: the lines of `Voyage.Notice` become rank 1 and 2 (the fading `Notices` go),
an `Alert` becomes rank 3, an urgent one rank 4. Three lines show, each led by the hour as a plain word. A click
opens twelve; the wheel reads back the year, which later feeds the winter saga. A clicked line opens its panel at
its subject. **The hint line** is the log's top line: it names what is under the pointer and never says what a
click does. **Tooltips** grow out of `HoverCards` and go two deep and no further: hold still one second, or press
Alt, and the note locks; an underlined word in it opens one more. Costs always read in one order: silver, hours
or days, odds. A crewman gives a hint only after a stall, one in three minutes at most.

### 1.7 State by state

| State | Left: rack and dock | Right: portrait, speech, window | Centre and log | Covered |
|---|---|---|---|---|
| **The first minute** | Hold, Tasks | you; her at the dam heads | Dorestat a third in from the left | 25% |
| **At sea** | three rows; a panel or chart | you and four heads; her under way, the sky, the banks | her mark, the course, wind streaks, wakes | 27%; 35% with a panel |
| **In port** | the town panel | you; her moored, other hulls by the port's size | the reveal line is the newest log line | 41% |
| **In a talk** | the town panel stays | the speaker, his words, your replies | the hint line names what a reply costs | 51% |
| **At market** | the market, the hold locked under it | the buyer; she sinks a pixel for every three lots | the word on the row you point at | 46%; 56% haggling |
| **On a card** | as they were, one ink step paler, taking no clicks | the man, the fact, the choices; the card's shot | her mark ringed; the outcome becomes a log line | 37% |
| **In winter** | hidden | you a year older, the heir; window shut | the winter board, 800x460, on the frost chart; frost on the frame | 85% |
| **Late, with a fleet** | four rows; House at Keels | you; your own boat, as in the first hour | six Byzantium marks at most (solid, hollow with an age, faint); six due-notches under the day stick | 40% |

Nothing in the frame fades, glows or slides. A panel is put down in 3 frames with a wooden clip. A pressed button
goes in 1 pixel and its bevel swaps. A notch is cut in the day stick each midnight.

## 2. The mechanics behind the panels

**How they feed each other.** A talk gives **word**. Word chooses the port, and at the scales it is the haggle.
A sale well made and a gift given make a **tie**. A tie gives a fourth talk, a store that sells while you are
away, a pilot's water, a partner, a wife, an heir. All of it is paid for in **days**, and the voyage rules are how
days are won or lost. What the house kept its word on is cut on a stone; the stones are its **name**, which warms
the next stranger's first word.

### 2.1 Trade (Hold, Ledger, the market)

| Rule and numbers | True to the period | Adds |
|---|---|---|
| **Named buyers.** A good's appetite is split among one to three buyers (port size 1 to 2: one; 3: two; 4 to 5: three). The best pays 1.15 x for the first third, the second 1.0, the quay 0.80, then the old glut curve of `Market`. A served buyer is gone 20 days; a rival's sale strikes the best first | a sale was two men talking | every sale is a choice of whom; coming second costs a named thing |
| **The haggle is what you know.** Once a call you name a port from your `Ledger`. A note seen and under 14 days old moves the price a third of the way to it, 8% at most. Any other note under 30 days, 4%. Older, or none, is a bluff at 0.3, and failed it costs 5%. If his own port has newer word than yours, the claim fails and he tells you why: a free fact | a price was an argument, and the argument was news | fresh word pays twice: to choose the port, and at the scales |
| **One gift rule.** One gift a person a year counts: 5% of the cargo, at least the floor of his rank (6, 24, 60, 240 d); a fine lot counts half as much again. Feeling +8, or +12 for what he prizes. A return gift within the year, 0.5 to 2 times yours. At a hall only the quay deals with you until you have given | Ohthere's walrus teeth; Havamal on gift for gift | giving away the best cask can be the best trade |
| **The host keeps your goods.** The man who lodged you three nights is your guest-friend (2.2), one a port. He stores 10 lots a port size and sells a third of the appetite each 20 days at the fair price, a tenth to him. A raid empties it | the Kentish law: the host answers for his guest | a small boat beats the glut with time, not tonnage |
| **Three grades.** Poor, fair, fine: x 0.75, 1.0, 1.3. Unsampled it "looks fair". A maker sells 15% poor, a stranger 35%, a host none. A hall or church takes only fine | short cloth, sour casks, the king's measure of 794 | the Sample act and the trusted seller matter |
| **Paid in wares.** Sold "for wares", the price is held as credit with that buyer at the middle price. This port's goods are bought from the credit at the middle price too: no 20% gap. What is unspent at cast-off is paid in silver at the usual price. Before 860 in lands of weighed silver the best buyer pays in wares only (D) | Ohthere's dues; the Raffelstetten salt toll | the return cargo is chosen at the moment of sale |
| **A hungry town** (living world) pays half again for food; 11 sacks give a village of 120 two and a half months | the famine of 793 | the grain run has a face |

### 2.2 People and standing (the portrait; Chest: People)

| Rule and numbers | True to the period | Adds |
|---|---|---|
| **Feeling is a word with reasons.** The -100 to 100 of `NpcRegistry` stays in the code. It fades 10 in 100 a winter toward the bond's floor: the middle word for kin and a guest-friend, the lowest but one as a ceiling in a feud (D). Three memories are carved and never fade. A deed worth 10 points travels as news: friends half, rivals a quarter the other way | deeds were public and told | people quote you back to yourself |
| **Five bonds:** kin (by birth, marriage, fostering or the font), guest-friend, sworn partner, lord, feud. One guest-friend a port, one lord, twelve living bonds besides kin. Unvisited five years, a bond is cold until a gift renews it | kin, sworn friendship and lordship, made in public | a port with a tie trades differently |
| **Claims.** A favour done is a notch on a tally stick. Small: lapses in 7 years, spent once, three on one person; buys a fourth talk, surety to 120 d, 30 days' grace. Great: never lapses, once in 3 years; buys surety for the whole debt, an oath-helper, a winter's shelter | favours were counted and inherited | standing you can hold, spend and lose |
| **Three talks a call**, plain, warm (one thing more; he may ask a favour) or hard (only what you asked; he likes you less); one back each dawn in port | hearing was the trade | the rationed act all else starts from |
| **The feud ladder:** rival, at odds, feud. Up by a deed; down by a settlement: an arbiter friendly with both, wergild of 640 d (?) in three yearly parts on the day stick, a marriage or fostering to seal it | feud ended by settlement, not victory | an enemy with a way out, and a price |
| **The word said of you:** Unnoticed, Known, Well-off, Rich, A prize, from what can be seen (keels, stock, public gifts). One step lower for two years after a keel, a plot or the head is lost | envy followed seen wealth | danger you can read, that eases after a blow |

### 2.3 The voyage (the chart, the day stick, the window)

| Rule and numbers | True to the period | Adds |
|---|---|---|
| **The word, one sentence:** cast off [now / on the ebb / at first light / when the wind serves], go [inside / by the coast / straight across], at dusk [lie up / sail on]. Each way shows a short and a long count of days and at most three risk words | Ohthere "waited for a west wind"; he camped or sailed the night | the wind's threefold difference becomes your choice. You never steer |
| **A wind that serves.** The world ships' own test (`WorldSim.FairWind`, made public), checked every 6 hours. The wait runs fast, as `TideWait` does, and any key stops it. A day waited costs 1 d in wages, a day of bread, a day off every price word. Beating to Lundenwic costs 2.5 days more: two days of waiting pay, a third does not | the world's ships already wait | "fair" or "foul" is written by each known port: a reason to trade elsewhere |
| **Dusk.** Lie up within 8 km of a known landing, or sail on: along a coast each dark hour is 2% to touch ground with no moon, 0.5% under one, a quarter of that in a known water. A touch is an hour lost and a log line; on a falling tide it is `Aground` and its wait; in a gale it is a hull wound. Dark is 5 hours in June, 13 in October (?) | camp on a coast, sail through on open water | the season is felt with no extra rule |
| **Gates.** A stream of 1 to 1.5 knots; wind of 8 m/s against the ebb on a bar: 10% a lot lost, 3% a man hurt. One card: wait, cross, go round | she rows at 1.75 knots: the right tide is half a day | a deeper hull makes it worse |
| **Weather wounds before it wrecks.** A sign 12 hours ahead, said by a man, answered once: hold on, shorten sail, shelter. In a gale each hour: 6% a lot over the side, in the order you set once in the Hold (unset, the cheapest first); 2% sail or strake; 0.5% a man. The boat is lost only when already hurt with a lee shore within 10 km | Anskar's merchants lost nearly all, and lived | loss by rungs, each with a way out, and no card an hour |
| **The blot.** On open water over 40 km her mark is a blot: 8% of the distance since the last fix, doubled in murk, halved by a good helmsman. A sea-mark or a sounding fixes it | no chart; "hafvilla" was a word | the window is worth watching |
| **Company for one leg:** raiders close a third as often on three ships; you wait 0 to 2 days, sail a tenth slower, and lose 5 to 8% for each ship in ahead of you | merchants sailed together when they feared men | safe and late, or alone and first |
| **Known water.** Three passages: the long count shrinks a quarter, night risk halves, landings are inked. The heir keeps the ink | the way was known from men | the same count makes a run a steersman can be given |

### 2.4 The crew (Crew, the four heads)

| Rule and numbers | True to the period | Adds |
|---|---|---|
| **Four seats, four crafts:** helmsman (halves the blot), lookout (sees a third further), leadsman (holds each pilot's water), weather-wise. Craft 0 to 3: right 60% of the time unskilled, 90% skilled. Each sign is said by its man, with his sureness. The four are records in place of the count `Voyage.Men` | a boat this size carried about four (D) | the men are the instruments; an empty seat is a silence |
| **Trait, want, fear, and three notches of patience.** Overruled against his nature, one is cut. At the third, one card: he leaves, he fights, or 1 time in 4 he vows. Mended by a step toward his want, a feast at his home port, his share paid on the day | free men with their own ends | a want tied to a place is a route offered |
| **Marks, earned,** three at most, each with one effect. Storm-tried (three gales): his gale signs are a word surer. Sea-shy (a wreck): a notch is cut if you sail after Michaelmas. Scarred: slower at the oar, raiders' cards a word better. Close-mouthed: your deeds do not travel from him. Grey (50): one more water known | | an old hand is seen to be one, and is worth keeping |
| **Weary,** one word for the boat: +0.1 a night sailed, +0.15 a gale, -0.3 a night ashore. Over 0.6 the oars give 0.7 and night risk is half as much again | | lying up is rest; seen in how the men sit |
| **Pay, age, death.** 0.25 d a man a day (as now). From 55, 1 winter in 5 he comes ashore as your guest-friend or your man abroad. Dead at sea: 60 d to his kin and a visit within a year, or a kinsman holds a great claim on you | a man answered to kin | nobody vanishes; a death leaves a debt |

### 2.5 The house and its heirs (House, the winter board)

| Rule and numbers | True to the period | Adds |
|---|---|---|
| **The winter page has seven sticks.** New orders, a keel laid, a man placed, a marriage, a gift, a suit, a partner's terms: each breaks one. The rest runs as last year. All keep is one sum at Martinmas | the season closed; winter was the reckoning | the house never outgrows one page |
| **Men and shares, not a list of ships.** You always sail one boat. Six other keels at most, in shares adding to three hulls. Only kin, a man aboard for three crossings of the run, or a partner of one season may steer | ships were held in shares by sworn partners (felag); a merchant with two whole ships is not known to me (?) | a second boat is a person you know |
| **Word of your ships is as old as the ship that brought it:** Lundenwic 6 days, Hedeby 13, Birka 35. A notch that passes with no boat turns red, without a sound | there was no other way to know | the fleet is worry and news, not a screen to mind |
| **Silver is where it lies,** and earns nothing while it sits | no banks; lending at interest was forbidden | silver has to become something |
| **Marriage and children.** A morning-gift of 60 to 240 d; she brings three named kin and keeps the keys. A child 1 winter in 3 at home. At seven: home, aboard at twelve, fostered, or the minster. Three traits: born, raised, earned | marriage and fostering allied houses | the heir is someone you made |
| **The year together.** From 14 the heir may sail one year beside the head: each run counts as his one crossing, each person met keeps full feeling for him (not the half `NpcRegistry` gives an heir), the father gives one line on each card | sons learned the water aboard | the handover is played |
| **Death and the division.** From 45 a draw each winter, 2 in 100 rising by 2 a year, warned a winter ahead. Each man keeps the boat he steers and becomes a kin house. You play one of up to three heirs. Loose silver: two thirds. One heirloom is named. One friend you choose; the deepest feud comes too | inheritance was divided | each life starts smaller, in a wider world |

### 2.6 The long game (World, Chronicle; House: Stones)

| Rule and numbers | Adds |
|---|---|
| **Five chosen years a life.** Eight to ten lives to 1066; four to seven sailed years in about thirty. A full year is 3 to 4 hours: about thirty days under sail (6 minutes a day at 4x), the rest waited at the fast rate. A year ashore is two minutes. Staying ashore stops itself for a death, a war in reach, a hinge two years off, an heir of age, an age turn | 273 years in about a hundred hours, with no time warp |
| **Seven ages, one rule changed each.** 834: a port can be burned, and home must move by 863. 865: two purses, counted pennies and weighed silver. 900: in English lands, no sale over 20 d outside a walled town and its reeve (?). 973: in England old pennies lose part of their worth at each recoinage, on a known day (Edgar's reform; how often, ?). 991: a tribute year can take a ship (?). 1042: a strait can be shut (D). Each age lays three deeds on the winter table | last age's best run stops being the answer; nothing is taken from the house by rule |
| **Three promises and one vow.** A promise is a job or a chain step for a named person. A vow is one of three the winter page offers (fleets plan 5). A fourth promise asks which to break. A job pays 20 to 120 d over the market; a chain's finale 600 to 2,500 d and a grand reward | Tasks is never a list of chores |
| **The name: stones, not silver.** Each life leaves a stone with notches: a vow kept 1, an age deed 2, a town above its line 2, a hinge turned 3, a broken oath minus 2. The sum is a word: 5 known on the bank, 12 in the delta, 25 across the sea, 45 in three realms, 70 told of. It floors standing at a succession | a poor grandfather can be the greatest of the house |
| **Danger comes from the calendar.** The age sets how many bands sail; the word said of you only chooses where a blow lands | staying poor is never the best way to play |
| **The ladder of loss:** a cargo, a man, the boat (the six-oar boat is never sold), the home port (move: a winter and 480 d), the head, all heirs (a new man of the same bank, with deeds worth double) | a fall opens the best page |

### 2.7 Left out, and why

- **Steering, and the sail or the lead as orders from the window.** The word and the cards give orders. Two ways
  to do one thing is how the first mockup got heavy.
- **Selling poor goods as fair; scale pans dragged level.** A second haggle, and a second way to sell.
- **God-kin and fostering as bonds of their own; a fourth feud rung; stress; child deaths by a roll.**
- **Hired captains, a fleet screen with orders, live sight of far ships.** Ruled out by the fleets plan.
- **Price graphs, stock numbers, automatic routes, a second currency, skill trees, a battle screen.** As before.

## 3. The first hour, ten hours, fifty hours

**The first hour.** No tutorial mode: ordinary play, three starting tasks, one early debt day, buttons that
arrive. Real minutes; the clock is held in every panel.

| Minute | What happens | New on the frame |
|---|---|---|
| 0 to 5 | One board: who you are, what you owe. She lies at the dam heads; the town shows three sites. The reeve takes the portrait: 480 d by Michaelmas, and an earnest of 40 d by St John's day, three days off. The priest's man: four casks for Traiectum, 12 d | 21 June 793 on the stick; 86 d, 9 days of bread; Hold, Tasks; your face; her at the dam; a sign on the stick; three task lines |
| 5 to 9 | The market; each good carries its word. Click Traiectum: "half a day". Cast off | Ledger; the clock's speeds |
| 9 to 15 | Thirty km of river, the banks in the window. Minute 11, the first card: a ferry rope across the stream, three choices, none ruinous. Folkmar speaks | Crew |
| 15 to 25 | The reveal line; the wine handed over; your goods sold to a named buyer. Three talks: Lindisfarne is two weeks old. Evening: lodge, or sail by night | Chronicle; Chest, with the first lines of a poem |
| 25 to 50 | Home against the stream. Weather seen in the window before its card. Dorestad with all its sites. One haggle, naming Traiectum's price. A skipper from Lundenwic: wine pays well there | World |
| 50 to 60 | St John's day, the reeve's card: pay 40 d (the debt reads 440), ask a week for 10 d, or let it pass (528 d). A course west shows "2 to 4 days" | seven buttons |

Pass marks, with a new player and no help: cast off by minute 10, a sale by minute 20, every button opened once by
minute 60, the earnest paid or knowingly refused, no boat or man lost before St John's day.

**After ten hours** (the second winter near (D)) a player has: crossed to Lundenwic three times, so that water is
inked; waited two days for a wind and been glad of it; lost a cask on a bar; a guest-friend at Lundenwic with six
casks in his store; spent a small claim on a fourth talk; one man with a mark and one seat refilled; paid the
count, or won a year's grace with a great claim; read a winter back as a saga; the House button.

**After fifty hours** (the fourth head of the house, about 890 (D)) a player can say: "I paid my father's debt."
"I sailed a year beside my father, and then without him." "I lost the Gull off Texel and came home in the six-oar
boat." "Dorestad burned four times; the fourth time our store was empty, because I had word." "My brother's house
is at Hedeby and carries my word for nothing." "Three stones stand. My grandfather's has the most notches, and he
died poor." The frame is the one from the first minute, with eight buttons and six purple marks on the chart.

## 4. The build order

One step at a time; each leaves the game playable and `ChartGameTests` passing. Steps 1 to 6 are the frame you
asked for, on the game as it stands. Steps 7 and 8 are the two cheapest gains in depth. The living world's steps
(`living_world.md` section 10) run beside these. **A button's arrival is switched on only by the step that builds
its panel,** so no button ever opens nothing.

| # | Step | Nights (D) | Its test |
|---|---|---|---|
| 1 | `FrameRoot` (UI Toolkit, as every panel now): named slots on a 960x540 canvas at whole scale; `FrameSkin` nine-slices; `ChartSafeRect` for `MapView` | 1.5 | A click through an empty dock reaches the chart; her mark stays in the safe rect; lint fails on any frame opacity under 1 or any transition |
| 2 | Settings block and rack in place of `MapTable`'s bar and outliner: `RackRegistry`, flags `hud.<id>`. The market, ledger and chest moved into the dock. **Hold** (twelve lots, on `Hold`) and **Tasks** (on `Commissions`) built | 2.5 | A fresh game has two buttons; each condition adds one; a third panel closes the oldest; `PanelClock` holds for a dock panel |
| 3 | `NoticeBus` over `Voyage.Notice` and `Alerts`; the log, the hint line; `TooltipLayer` from `HoverCards` | 1.5 | Seven rank 2 notices in a day show six and fold one; a third tooltip level cannot open; an urgent alert still stops the game |
| 4 | Portrait block and speech place; `EncounterPanel` and talks open there; **cards at sea in the chart scene** | 2 | A sea card opens under the face, drops speed to 1x, holds the clock; each state fills each slot as section 1.7 says |
| 5 | The day stick: the date, notches, signs, the bread line, a course's days | 1 | A course that outruns the bread shows past the line |
| 6 | The first hour: stores, three tasks, the earnest (if you answer 3a), the first river card, hints on a stall; the **Chronicle** panel on `Chronicle` | 2 | 200 seeded first hours: no boat or man lost before St John's day; paying sets 440, passing sets 528 |
| 7 | Three buyers; the haggle from notes | 1.5 | The three steps sum to the old curve within 5%; a seen note never moves the price past the note |
| 8 | The word: `PassagePlan` on `Course` and `LaneRouter.Profile`, two day counts, the wind wait | 2 | A foul wind lengthens the long count; "when the wind serves" casts off only when the world ships' test passes |
| 9 | The ship window, first version: stage, sea and sky, sail and load, three places, sound | 3 | A gale on the chart is a gale in the window; twelve lots sink her. **Over 2 ms a frame: stop and decide** |
| 10 | `FeelBus`, ticks, the chart's sound bed, the purse count | 2 | Every event has a row; every clip is on disk with its credit line |
| 11 | Dusk (in place of `Rest`'s key) and `VoyageRisks`; weather answers and wounds | 3 | A seeded night roll repeats after a reload; 200 seasons: no boat lost that was not hurt on a lee shore |
| 12 | Four `CrewMan` records: seats, crafts, patience, weary; the heads, the **Crew** panel, figures in the window | 3 | Three overrulings draw the break card once; a lost man's line is not spoken and his seat is empty |
| 13 | The leg's log line on the `VoyageLog` that exists; known waters and `RunLog` as one counter | 1.5 | Three round trips ink a water, two do not; the line's pennies equal the change in the purse |
| 14 | People: fading, carved memories, bonds, claims, the gift rule; Chest: People | 3 | A claim is spent once; a cold bond gives nothing until a gift; no gift at a hall, no best buyer |
| 15 | The host's store; gates | 3 | Twelve casks left 60 days at appetite 12 sell twelve; a raid empties it; with the ebb she gains the stream |
| 16 | The winter board: saga, reckoning, seven sticks, marriage; `Life` and the death draw; a year ashore | 4 | The eighth change is refused; 1,000 lives end between 46 and 66, each warned; twenty years ashore hash the same twice |
| 17 | Grades; paid in wares | 2 | An old save loads as fair; a sale for wares spent in full changes no silver |
| 18 | One boat sent (fleets plan steps 2 to 6); the **House** panel | 6 | That plan's tests. Play it before going on |
| 19 | The blot, sea-marks and the fix; company; far sails and card shots in the window | 4 | The blot is 16 km after 200 km clear, 32 in murk; in company both arrive on the slower boat's hour |
| 20 | Succession: three offers, the division, the year together, stones and the name; the feud ladder; age turns | 6 | A staged death gives three offers, each with the keel, feud and friend it named; flags give the expected notches |

About 55 nights of code (D); steps 1 to 8 are about 14. The **World** panel comes with the living world's wars
(its step 10). The drawing runs beside: nine-slices, button pictures, six faces on a base head, twelve shore strip
sets, flat figures for the men. **To settle before step 20:** `winter_793.json` says the heir keeps every hull and
`unlocks_793.json` says eight keels; this page and the fleets plan say the division, and six.

## 5. Yours to decide

My recommendation is (a) each time.

1. **The frame between the places.** (a) A thin rim, thick places on it, the chart running to the rim. (b) No
   rim: islands only. (c) Solid columns, with empty wood where no panel is open. (d) As (a) in another cut.
2. **Buttons that arrive.** (a) Two at the start, the rest arriving with what they open; a switch shows all eight.
   (b) All eight from the first minute. (c) Four at the start, four arriving.
3. **A first debt day inside the first evening.** This changes your decision 10 (one fixed sum on one day).
   (a) An earnest of 40 d on St John's day; missing it raises the debt and ends nothing. (b) Michaelmas only.
4. **What the ship window may be clicked for.** (a) Looking and learning: a man, a far sail, a sea-mark, the next
   view. (b) Also the sail and the oars, as orders. (c) Nothing: it is only watched.
5. **The years between voyages.** (a) You choose which years to sail, four to seven a life. (b) Every year is
   sailed, kept short. (c) One great voyage a life.
6. **Whom you play at a death.** (a) One of up to three heirs, after the house divides. (b) Always the first in
   rank, with the division. (c) The first in rank, and he keeps every hull.
7. **The clock and a panel open at sea.** (a) Every panel holds it, as decided on 4 October: she waits while you
   read. (b) At sea, Hold, Tasks, Ledger, Crew and World only show, and she sails on, as the old outliner let her.

## What was changed on the second read

1. Settings block: six things in 236 px did not fit. Now four; the date is on the day stick.
2. The word said of you: moved to your portrait; its five words named.
3. Gold was "silver only" and also the button counts. The counts are white.
4. The frame had no thickness. Now thick places, a thin rim, Gentium, the scheme you pick.
5. Twenty-one tabs, now seventeen: Ledger and World are one page each; a crewman is sentences, not fields.
6. A talk put seven things beside a 96 px face. Now four; the rest opens on a click.
7. Buying and selling was told three ways. Now one paragraph, three clicks.
8. Paid in kind had no screen. Now "for wares" on the buyer's line; the dragged pans are left out.
9. Percentages read like screen text. A rule at the top keeps them in the code (your decision 18).
10. The clock rule was only implied. Now stated from your decision of 4 October; question 7 added.
11. The forecast lived only in the window, against the window's own rule. The 12-hour sign is spoken too.
12. A gale meant a card an hour to pick a lot. Now the order is set once, in the Hold.
13. Touching ground at night had no outcome. Now an hour, `Aground`, or a wound.
14. The haggle left notes of 14 to 30 days undefined and dropped "he has newer word". Both are in.
15. The host and the guest-friend were two rules for one man. Now one.
16. Crew marks did nothing. Each has one effect. Bond floors, talk tones and the vow are defined.
17. The wind wait had no pace. It runs as `TideWait` does (your decision 8).
18. Period: "no merchant owned two ships" is now a doubt. The walled-town and recoinage rules are English law
    and hold in English lands only; the recoinage is about 973, not 955.
19. The first Martinmas at "hour 3" did not fit the clock. Now hours 4 to 6; ten hours is near the second winter.
20. Code that exists was ignored: `MapTable`, `Notices`, `Alerts`, `HoverCards`, `PanelClock`, `EncounterPanel`,
    `VoyageLog`, `Ledger`, `Commissions`, `LaneRouter.Profile`, `ShotDirector`, `FairWind`, `Voyage.Men`. Each is
    now named where it is used or replaced. The window's rig is in KapmanSail and must be copied.
21. Hold, Tasks, Chronicle and World had no build step, and Crew could arrive before its panel. Fixed: 55 nights.
22. Shift and 1 to 4 for the crew is gone; the digit keys no longer clash with today's menus.

Least sure still: the wergild sums, the hours of dark, the English laws of ages 4 and 5, and every share of the
screen, which is arithmetic on a mockup and not a measured capture.
