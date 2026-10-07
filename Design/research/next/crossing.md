# The crossing that is worth its minutes

7 October 2026. For the ruling of the same day: no "Sail on", the clock keeps 1x, 2x and 4x and nothing faster, so
the leg has to be worth its real minutes. This page replaces stage 1 step 5 of `GAME_MAKER.md`.

**How I worked.** I read the book, both apprentices, both critics, `fate_faith_and_self.md`, `DECISIONS.md`,
layout G of `ui_mock/template3.html`, the depth and fleets research, and `research/next/popups_and_windows.md`
(written the same morning; this page keeps to its rules for floats and windows). I read `ChartBody.cs`,
`GameSpeed.cs`, `Sim/Travel.cs`, `Course.cs`, `LaneGraph.cs`, `Rest.cs`, `PanelClock.cs`, `Chest.cs` and the data
tables. I did not run Unity. I walked the baked sea lanes (`G:/kapman_unity/data/baked/world/lanes.bin`) with a
script that copies the router's costs and `Travel.Speed`. The session's web searches were already spent, so the
web facts come from sixteen pages fetched by name and from memory.

**Marks.** (?) not checked. (g) a first guess to tune. LATER: the source is later than 793 and is used as the
nearest witness. Words in quotes in the designs are placeholders: the wording is yours.

## 0. The short answer

1. **The sum.** Dorestat to Lundenwic is 415 to 433 km by the lanes, not 350. With a fair wind it is 50 to 55 game
   hours: **25 to 28 real minutes at 2x, 13 to 14 at 4x**. Against the usual south-west wind it is 111 to 123 game
   hours: an hour at 2x, half an hour at 4x. Each night lying up adds 3 minutes at 2x in June and 6 in October.
2. **Why loved slow games do not feel like waiting.** The traveller always has one small question he can answer
   by acting now, and the passing time pays something in that he keeps. Nine mechanics do it; section 2 names them.
3. **Four things fill the leg here**, all from what the game already is:
   - **The landfall line.** One sentence, always in sight, says when she will be in and what gate stands in the
     way: a tide, the dusk, a Sunday, a rival. The order's live switches move it, and each shows its cost first.
   - **Three verbs that harvest.** Look out, Sound and Hail each return a fact with an age. Sightings are
     tidings to tell in port. Soundings ink your own chart in Byzantium. A hailed ship out of your port has the
     freshest price you can get.
   - **One thing a watch.** Every four game hours (two real minutes at 2x) up to three offers light: a tale, a
     lesson in a tongue, a talk with a man, a riddle, a mend, a line over the side, the helm. You take one. They
     are done in the tiled windows, so the clock runs through them. They are only there at sea.
   - **Signs and cards**, as the book has them: four signs a game day, two cards a leg.
4. **The rhythm at 2x:** something new to look at every half minute to a minute, a choice with a cost about every
   two minutes, a stop about every five. Busy at both ends, quiet in the middle, like any passage.
5. **Three faults found on the way.** The planner cannot sail the book's way down the Lek: 4 km of lane at the
   Maas mouth is marked pending, so she goes by Traiectum and the Old Rhine. Quentovic, Ripa, Doccinga and Fordwic
   have no way at all for the player's boat. And `Rest` (1800) and the tide wait (3600) run faster than 4x, against
   the ruling.

## 1. The sum, from the code

**The rule in code.** `ChartBody.Update` moves her by `Travel.Speed` in real metres a second times the game
seconds of the frame. `LevelClock.rate` is 60 and the chart's scale is 60, so the two cancel: she makes her real
knots on a clock of one game minute a real second. `GameSpeed.Speeds` is 1, 2, 4 and sets `Time.timeScale`. So
**game hours = real km / (knots x 1.852)**, and one game hour is 60 real seconds at 1x, 30 at 2x, 15 at 4x.

**Her speed** (`ship_classes_793.json`, row `gislinge`: 4.8 knots on a beam reach in 8 m/s, no closer than 58
degrees, 6.5 at most, oars 3.5 for 6 hours in 12; she lies to over 17 m/s):

| Wind | Beating, made good | Close reach | Beam | Broad reach | Dead run |
|---|---|---|---|---|---|
| 3 m/s | 1.75 (the oars win) | 1.9 | 2.7 | 3.0 | 2.2 |
| 5 m/s | 1.75 (the oars win) | 2.6 | 3.6 | 4.1 | 3.0 |
| 7 m/s | 1.75 (the oars win) | 3.2 | 4.4 | 5.0 | 3.6 |
| 8 m/s | 1.8 | 3.5 | 4.8 | 5.4 | 3.9 |
| 12 m/s | 2.3 | 4.4 | 6.1 | 6.5 | 5.0 |

A calm is 1.75 knots at the oars. So a foul wind and a calm are the same thing to her: rowing pace.

**The way.** `Course.Plan` shuts every lane of kind `Pending`. Four kilometres of the lane at the Maas mouth
(near 51.89 N 4.32 E) are pending. So today she cannot go down the Lek to the sea. She goes past Traiectum, down
the Old Rhine to its mouth (52.23 N 4.38 E), 200 km over open sea, makes the Essex shore near 51.78 N 1.42 E and
comes up inside the sands. With that one link open she goes the book's way: the Lek, the Maas mouth, 41 km along
the coast to off Schouwen, 142 km of open sea to the Foreland, the north Kent shore, 15 km of flats, the Thames.

| Dorestat to Lundenwic | Game hours | 1x | 2x | 4x |
|---|---|---|---|---|
| **The way she sails today**, 433 km (river 113, open sea 199, sounds, coast and flats 121) | | | | |
| East, 12 m/s: the best there is | 41 | 41 min | 21 min | 10 min |
| East, 8 m/s: fair | 52 | 52 min | 26 min | 13 min |
| North-east, 7 m/s | 55 | 55 min | 28 min | 14 min |
| The mean of all wind directions at 8 m/s | 75 | 1 h 15 | 38 min | 19 min |
| South-west, 8 m/s: the common wind | 114 | 1 h 54 | 57 min | 29 min |
| West, 8 m/s | 123 | 2 h 3 | 61 min | 31 min |
| A calm, at the oars | 134 | 2 h 14 | 67 min | 33 min |
| **The book's way, by the Lek**, 415 km (if the pending link is opened) | | | | |
| North-east, 7 m/s | 50.5 | 51 min | 25 min | 13 min |
| The mean of all directions at 8 m/s | 72 | 1 h 12 | 36 min | 18 min |
| South-west, 8 m/s | 111 | 1 h 51 | 56 min | 28 min |

To add: 2 to 4 per cent for `ChartBody`'s slowing near each point of the course (she slows inside 25 game metres
of her goal, and the planner passes a point at 15). Each night lying up: 5.6 dark hours at midsummer at 52 N (the
Sun under 6 degrees, as `Rest.Dark` has it), so 5.6 minutes at 1x, 2.8 at 2x, 1.4 at 4x; about 13 hours in late
October. A gale lying to, and any wait for a tide. There is no stream in `ChartBody`: the ebb gives her nothing yet.

Other legs, in a wind of 8 m/s, fair to foul: Traiectum 30 km, 3.5 to 7.5 game hours (1.8 to 3.8 minutes at 2x).
By the Lek, with the link open: Witla 111 km, 15 to 25; Walichrum 166 km, 19 to 43. With every pending lane open: Fordwic 331 km,
Quentovic 371 km, Doccinga 374 km, Ripa 563 km (78 to 158 game hours: 39 to 79 minutes at 2x). **Under the
profile the player has today, those four have no way.**

**Two things in the code break the ruling of 7 October.** `Rest.Rate` is 1800 (half an hour a second) and
`voyage_rules.json` `waiting_rate` is 3600; `GameSpeed.Factor` steps aside for both. If nothing may run faster than
4x, both go, and a night ashore becomes real minutes that need filling. Section 4.5 fills them.

**What the sum says.** The critic's "2 to 4 days" was right. The measure's "3 to 4 days" is the fair-wind case
with two nights ashore. A player who casts off into a south-west wind sits an hour at 2x at rowing pace. So waiting
for a wind in port is not a nicety: it halves the leg. And the leg is long enough that it must be a sitting of play
in its own right, not a gap between ports.

## 2. Slow travel that is loved: what the hands and the head do

`research/fleets/one_ship.md` has how these games grow and how their late games feel. `voyage_depth.md` has why
their travel is a choice. This table is only the minute by minute: what is pressed, what is watched, what is
planned, what breaks in. The last column names the mechanic, not the mood.

| Game | Hands, every few seconds | Head, every few minutes | What breaks in | The mechanic |
|---|---|---|---|---|
| **Sailwind** | Sheet, halyard and tiller by hand; a touch of trim shows at once in her speed. Bail. Cast a line, cut and cook the fish | Where am I? No mark on the map: the Sun's height at noon, the compass, a pencil mark you make yourself. Food, water, sleep | A squall to reef for; a shoal; land where you did not expect it. Sleep runs time on while she sails and something may wake you (?) | **The position is yours to work out**, so landfall is a result you earned |
| **Euro Truck Simulator 2** | Steer, signal, mirrors, hold the speed limit | The route advisor: time to go against the delivery window; fuel; the rest timer. The next job and the garage | Fines, traffic, roadworks, tolls, a fuel stop, the forced sleep (?) | **A light, unbroken task with a small price for every lapse**, under a time of arrival you can see. The head is left free: the game streams real radio (checked) |
| **Elite Dangerous** | In supercruise: hold the throttle so the time to target stays near seven seconds, or overshoot (?). Each jump: line up, charge, skim the star for fuel without overheating, sound the scanner | The route: how many jumps, which stars give fuel. Galnet news in the cockpit (checked; read aloud since 2018 (?)) | Interdiction: a tug of war you can win, or submit to. Docking is a small test every time | **A ritual of 45 seconds with a small skill and a small find each time.** What you scanned is sold at port, and a first find carries your name |
| **Sunless Sea** | Steer; set the engine; lamp on (fuel burns, fear rises slowly) or off | Fuel, supplies and terror against the next light. Terror climbs away from shores and buoys | Beasts in the dark; a line of text for each new shore | **"Something Awaits You"**: only time at zee earns the token that opens a port's main story (?). A port report is written at each call and paid for at home |
| **Dredge** | Steer; a timing ring to haul a catch; fit each fish into the grid of the hold | The Sun dial: can I be home by dark? | A rock closes a cell of the hold and its fish is lost; things in the fog at night | **Time moves only when you move or act** (checked), and the sea is seeded so thickly that something to do is never more than seconds off |
| **Death Stranding** | Keep balance under the load with the two triggers; pulse the scanner, which paints the ground ahead by how bad the footing is | The line you drew on the map before leaving; weight and how it is stacked; boots, stamina | Timefall rots the cargo cases (checked); ghosts to creep past; a river to ford | **Every step reads the ground. The state of the cargo is the score.** A road you build makes the second trip a different trip. A song starts as the goal comes in sight |
| **The Long Dark** | Walk; watch the breath, the wind, the light left | Four needs falling at four speeds (checked): thirst in hours, hunger in half days, sleep daily, cold by the weather | A wolf; a blizzard that pins you indoors | **Dead time is turned into kept gain**: pinned by weather you read a skill book (hours each, needs light (?)), mend, sew, boil water. You draw your own map with charcoal |
| **Kenshi** | None: you click a place and the squad walks. Speeds are few, as ours | Who is weak, who carries what, who is fed. Shapes on the horizon: a patrol, a caravan to walk beside | An ambush; a wound, and the squad goes at the pace of the limping man: carry him, camp, or leave him | **The march itself trains**: skills rise by doing (checked); a heavy pack builds strength (?). Dangers are seen far off and close at a speed you can read |
| **Roadwarden** | Choose from text; now and then type a word (checked) | The hours left before night, of 40 days (checked). Five things to keep up, one of them how you look (checked) | Night on the road | **A day is a budget of hours**, and being dirty changes how people treat you |
| **80 Days** | On a leg: read, choose, tend your master, talk to a fellow traveller | Money, his health, the days. Which way on from the next city | A story beat that may reroute you | **You learn the roads ahead by asking while you travel** (?). The clock also runs while you shop. The legs are short: under a minute (?) |
| **FTL**, by contrast | None. A jump is one press | Which beacon next, on a map that shows a little | Everything happens at the beacon | **Travel is cut to nothing and the stake is moved into the choice of the next node.** The fleet behind you is the clock |

**Nine mechanics, and where each lands here.**

1. **A hand loop that reads the world** (the scanner pulse, the scanner "honk", the lamp). Here: **Look out** and
   **Sound**. A press returns a fact with an age.
2. **A meter the journey drains, and a gate at the far end** (home by dark, the delivery window, the fleet).
   Here: **the landfall line** against the tide, the dusk, Sunday, the age of your word, and the field.
3. **Dead time turned into kept gain, capped by the day** (the skill book in the blizzard, Something Awaits You,
   the march that trains). Here: **one thing a watch**.
4. **The road ahead is learned on the road** (asking fellow travellers, the route plotter). Here: **Hail**. A keel
   coming out of your port has its prices a day or two old.
5. **The map you make yourself** (the pencil mark, charcoal, the built road). Here: **your soundings and the
   verse of the water** ink the chart in Byzantium, and a known water frees the next crossing for other things.
6. **The needs of the body at different speeds** (thirst, hunger, sleep, cold). Here: **the watch bill**, weary
   men, bread, the bilge.
7. **Arrival is a test, then a prize** (the parking, the docking, the graded delivery). Here: **the sands on the
   tide**, then the reveal and the reckoning.
8. **Something for the ear or the eye that is not the task** (the radio, the news read aloud). Here: **the tale at
   dusk**, and the ship window.
9. **Or cut the travel** (FTL, 80 Days). Ruled out. So everything FTL does at a beacon must be possible at sea.

**The test that comes out of this.** Stop the game at any second of a crossing. The player must be able to say
three things: what the leg will next ask of me and about when; one thing worth doing now; what this hour is paying
in. If he cannot, that minute is waiting.

## 3. An open boat in 793: the day and the night

Nobody wrote down a day aboard a Frisian boat of 793. What follows is put together from the nearest witnesses.
Most are later, or Norse, or English, and are marked.

**The witnesses.**

- [W] Willibald, Life of Boniface, about 760: in 716 Boniface took passage at Lundenwic on a merchant ship and came
  with a fair wind to Dorestat. It is this very crossing, with a monk as a paying passenger (?: from memory).
- [O] Ohthere and Wulfstan in the Old English Orosius, about 890: waiting for a wind; a coast sailed by day and
  camped at night; Wulfstan "seven days and nights" under sail all the way (`voyage_depth.md` 2.2).
- [S] The Seafarer, Exeter Book (the manuscript is of the 10th century, the poem older): the anxious night watch
  at the prow as she tosses by cliffs, feet bound by frost, hunger, and for company the swan, the gannet, the
  curlew, the gull, the tern and the eagle (read in translation).
- [M] Maxims I, Exeter Book: the Frisian wife who washes the sea-stained clothes of her man when his keel comes
  home (?: from memory).
- [R] Rimbert, Life of Anskar: about 829 merchants bound for Birka fought pirates "vigorously and for a time
  successfully", lost ships and goods, and went on by foot (ch. 10, read). A king has lots cast in the open air to
  ask the gods (ch. 18, read).
- [B] Bede: the Frisian in Lundenwic who bought a captive (IV.22); oil given to calm a storm (III.15) (?).
- [N] The Voyage of Brendan, 8th or 9th century: monks at sea in a boat of hides.
- [G] The saga of Grettir, ch. 17, written about 1320 of about 1011, LATER: bailing with tubs, "one went down
  while the other came up"; every man works "in even shares"; one who will not is mocked in verses (read).
- [K] The Mirror of the King, about 1250, LATER: what a merchant should learn and carry (?: from memory).
- [L] Norse town and sea law of the 12th and 13th centuries, LATER: bailing by turns, named watches, the cook put
  ashore (?: from memory, after Falk and Jesch).
- [H] The Hurstwic summary of the sagas, LATER: the skin sleeping sack, cold food, water in skins, cooking only
  ashore, ships beached at dusk, oar shifts of about two hours (read).
- [A] Finds: the Gokstad ship, about 900, with a tent, a bed, three small boats and a gaming board (read); the
  Ballinderry board, with peg holes "possibly to allow for portability" (read); the Oseberg cauldron and its
  stand, 834 (?).
- [X] Trials: a hide boat of 36 feet crossed the Atlantic in 1976 and 1977 with four men (Severin; read). The Sea
  Stallion, 2007 (not read: (?)).

**The day, midsummer, 52 degrees north** (light from about three in the morning to nine at night; 5.6 dark hours).

| Hour | The four men | The skipper decides | Witness |
|---|---|---|---|
| First light, about 3 | The watch below is shaken out. The tent or the sail comes off the boat if she lay ashore; she is walked down the beach on the tide, or the stone is weighed. A prayer, or something poured | **Go or wait.** He reads the sky, the dew, the swell, the wind. Ohthere waited for his wind | O; H; (?) for what was said or poured |
| Sunrise to mid morning | Yard up: two at the halyard, one at the sheet, one at the steering oar on her right quarter. On a river: oars with the stream, or the rope on the bank against it. Cold food: bread, cheese, dried fish, butter; ale or water | Sail or oars. Which bank, which channel | H; a Frisian boat hauled up the Rhine by a rope, in a book of miracles of 839 (?) |
| Through the day | One steers. One tends sheet and brace and looks ahead. One bails: a clinker boat weeps, and in a seaway it is steady work, by turns. One sleeps, or mends | Who steers and who rests. Spells at the oar of about two hours in a calm | G; L; H |
| Noon | The Sun is south and at its height: the one sure bearing of the day. A guess at the run made. The main meal, cold | How far have we come? Stand on, or close the land to know | O; (?) for any measure of the height of the Sun |
| Afternoon | The idle hours: a seam of the sail sewn, a rope end whipped, a thole strap cut, the sail greased. A line over the side. Talk, a board with pegged pieces, a riddle, a boy taught his knots | What to mend now and what to leave for the yard | K (carry sailcloth, needles, thread, nails, tools) (?); A; G |
| Where the water shoals | The lead, or a pole. What sticks to the lead says where she is: sand, shell, mud | Which sand is this? Wait for water over it, or go round | The Bayeux Tapestry, about 1075, LATER; `voyage_depth.md` 2.2 (?) |
| An hour before dusk | Along a coast: find the beach or creek while it can still be seen. Haul her up, or lie to the stone with a line ashore. Fire. The cauldron: the one hot meal. Ale, a tale, sleep under the sail, two to a skin sack, one man awake | **Land, or stand on.** The largest choice of every day | O; H; A |
| Night at sea | Two awake: the helm, and a man in the bow listening for surf. Steered by the north star and by the wind on the cheek. No fire, no hot food. Cold feet. Bailing goes on | The watches. Less sail. How near the land he dares be | S; O (Wulfstan) |
| Any hour: wind and sea | All hands bail. The yard comes down part way, or all the way. Goods go over the side. A vow is made | Hold on, shorten, run for shelter | G; R for the vow (?) |
| Any hour: a sail | Look, and look again: her size, her course, whether she grows | Stand on, turn away, speak her, run. Merchants sailed in company and were still taken | R ch. 10 |
| Landfall and the river | Which land is it? Then the tide: up on the flood, lie still on the ebb. The toll. Where to beach | Which channel in. Who stays with the boat | O; (?) for the toll at Lundenwic in 793, known from charters of the 730s and 740s |

**What a skipper decided, and when.** Nine moments, in the order a passage brings them: the wind before casting
off; the bar and the tide at the river mouth; land or stand on at each dusk; the watches for the night; sail or
oars when the wind changes; a strange sail; which land this is; the tide and the channel at the far end; and among
his men, all day, who does what. Quarrels come from that last one: in the saga the man who will not bail is paid
in mocking verses. Each of the first eight is already a switch of the order or a sign in the book.
The day adds what the book lacks: **who steers and who sleeps, what is mended, and what the idle hours are spent
on.**

**What the witnesses do not give.** A length for a watch. Any words of a prayer said, or a thing poured, by a
Frisian at a river mouth. Whether a Frisian boat of 800 carried a lead (`voyage_depth.md`: likely, not proven).
Reefing. Songs at the oar. The game must say so where it shows them.

## 4. The leg, designed for this frame

The frame is layout G as ruled: eight function buttons that each do one thing, tiled windows under them that hold
any of eighteen panels, floats over the chart for talk and events, the portrait, the ship window.

### 4.1 The clock at sea: four rules

1. **At sea the tiled windows never hold the clock.** Reading the chest, a lesson, a talk with your own man, the
   reckoning: all are done in windows while she sails. This is rule 2 of `popups_and_windows.md`, and it changes
   the decision of 4 October for the sea only. A sign, a card, a ship alongside and a function button's own window
   hold it. Space holds it at will. In port nothing changes.
2. **Nothing runs faster than 4x.** `Rest.Rate` (1800) and `waiting_rate` (3600) go. Lying up for the night,
   waiting for the flood and waiting for a wind run at the speed chosen.
3. **Stops keep apart.** After a hold ends, no new sign or card for one game hour (`popups_and_windows.md` R1).
   Four signs a game day and two cards a leg at most, as the book has it.
4. **The speed is the player's.** 2x is the speed a crossing is tuned for (g). A warning drops her to 1x once
   (`GameSpeed.Slow` exists) and she goes back.

*Build:* `PanelClock.PanelOpen` reads "a float that asks is open" in place of four named panels. `GameSpeed.Factor`
loses its clause for `Rest.On` and `TideWait.On`; the two keep their logic and lose their rates.

### 4.2 The watch: the heartbeat

**Rule.** The day at sea is six watches of four game hours: the dawn watch (4 to 8), morning, afternoon, evening,
the first night watch, the dead watch (midnight to 4). The four men stand watch and watch: two on, two off. Of
the two on, one steers; the other looks out, tends the sheet and bails. You are the fifth. You stand no watch
unless you take one. No witness gives a length for a watch in 793: this is the game's own device, like the day
rule, and the credits should say so.

**Numbers.** A watch is 4 real minutes at 1x, 2 at 2x, 1 at 4x. Each man has one word: fresh, tired, weary,
spent. A watch on duty in the dark, at the oars or in a gale: one step down. A watch asleep: one step up. A night
ashore: fresh. Watch and watch by day holds a man where he is. A weary man speaks his sign late (a sail called at
0.4 of the sight range, not 0.6 (g)); his oar gives 0.7; the night risk is half as much again
(`voyage_depth.md` 3.8). A spent man leaves his seat empty, and `sea.watch.asleep` may open.

**The player decides.** The bill has the three settings of `hands.md` 2.4 (all hands, watch and watch, one man)
and one press more: **put a man below for a watch**. Rest the pilot in the morning and he is fresh for the sands
at dusk. This is the choice the day of 793 adds.

**In the frame.** At the turn: one log line ("The morning watch. Folkmar takes the oar from Aldgrim."), the four
heads under the portrait change seats, the ship window shows them change, and the offers are dealt anew (4.5). No
stop. Crew shows four rows: name, seat, his word, what he is doing.

*Build:* `Watch` (the index from the hour of the clock; an event `Turned`). Four `CrewMan` records: name, craft,
faith, tongue, weary, bond, want, his words. They are step 9 of the book. `WatchBill`.

### 4.3 The landfall line

**Rule.** At sea one sentence stands at the head of Voyage, and as a stretch on the day rule: when she will be in
at best and at worst, and the next gate. It is worked out again each game hour from where she is.

> "Lundenwic: Sunday morning at best, Tuesday at worst. Next: the Maas bar at half ebb, before dusk. The two
> Frisians: a day ahead."

**Numbers.** Best: the course walked from `Course.Next` with `Travel.Speed` in the wind as it blows. Worst: with
the wind foul on the sea part (the two counts of the measure, book 4.2). At cast off the two are 50 and 111 game
hours. Off the Foreland, with 124 km to go, they are 15 and 37. In the river they are an hour apart. A gate is one
of four things, each from the simulation: a tide (a bar, the sands: `voyage_depth.md` 3.4), the dusk, a shut day
at the port (a Sunday, a feast), a rival (the field).

**The levers** are the live switches of the order (sail, in a calm, at dusk, way), and Lie to and Wait. Each says
what it does to the line before the press: "Row: Sunday dawn, not Sunday noon. Two men weary by dusk."

**Why it fills the minutes.** It is the time of arrival against the delivery window, with five levers and no
wheel. The question "can I be at the Nore before the ebb?" is open for the whole crossing, and every sign, calm
and hail moves it.

*Build:* `LandfallLine` (two counts, the next gate, the sentence). The walker of the measure (book step 7) called
once a game hour: about 400 points. The tide at a place for the chart scene, and `gates_793.json`
(`voyage_depth.md` 3.4). *Test:* at any hour of a seeded crossing the true hour of mooring lies between the two
counts, in 9 runs of 10.

### 4.4 The eight buttons at sea

| Button | At sea it does | Numbers | Build |
|---|---|---|---|
| **Cast off**, which reads **Lie to** | She stops and drifts. The clock runs. It is needed to restow (an hour), to mend with the yard down, to lie alongside a ship, to fish a whole watch | Drift half a knot down the wind (g). The landfall line shows the cost first | A lying-to state in `ChartBody` |
| **Wait** | "Wait for": the flood at the next bar, first light, the wind to serve, a sail to pass. She lies to, or to her stone in less than 10 m (g), or lies up at a landing in reach, and goes on by herself | Each choice shows hours, bread, wages (1 d a day for the four) and, past a day, "every word a day older" | `TideWait`, `Rest.Dark`, `WorldSim.FairWind` (made public), at the chosen speed |
| **Measure** | From her to a place, or between two. At sea it adds two marks on her course: where dusk finds her, where dawn finds her. Two places: the run after this one, laid on the day rule against Michaelmas | Free. No wait | Book step 7 |
| **Look out** | What can be seen now. Up to five lines: sails (bearing, hull up or down, growing or not; kind and home once made out); land (a mark you know is a fix); the sky (the reading of the weather-wise man six hours on, with his sureness); water and birds (brown water, a tide rip, gannets working); fires by night. Each line has one key: Mark it, Hail, Sound here, Keep as a tiding | Once in 10 game minutes. Your look reaches the whole of `Sight.RangeKm`; the watch by himself calls a sail at 0.6 of it (g). From an eye at 1.8 m a mast of 10 m shows at 17 km and a cliff of 40 m at 30 km (`Sight.HorizonKm`). What you look at yourself is first hand. Six tidings kept a leg at most (g) | `LookOut.Now()` over `SeaWatch`, `Sight`, `WorldSim.WeatherAt` six hours on, `Wildlife`, `MapLand`. A `SeaTiding` is a `Known` fact with a worth at "What tidings?" (2 to 10 d by the book) |
| **Sound** | One cast of the lead: the depth in fathoms, and what comes up on the tallow. Or "keep the lead going". On a river, a pole. Every cast leaves a mark on the chart in Byzantium | A cast is 5 game minutes. Lead going: her way falls a tenth, touching ground falls to a quarter (`voyage_depth.md` 3.5). Twelve marks in one water (g) and its bottom is known: a cast there names the place and is a fix | `Sounding.Cast()`: depth from the lane's `edgeDepth` and the tide; the bottom from a small table by lane kind until a bottom map is baked (g). `SoundingMark`s saved with the house. A chart layer |
| **Hail** | A sail near your way, the shore, or your own men. A sail: the frame first says what closing her costs. Then a talk, a word for a word: you tell one thing she lacks; she tells the price at the port she left, as old as her passage, who was loading there, and what she saw. The shore: a pilot; the water on the bar. All hands: each man reads the sign by his craft and his faith, and you choose whose reading the boat goes by | A sail within 3 km of your way (g): half an hour to an hour. With nothing to tell: ale, 2 d (g). She may stand off, by your Name and the look of your sail. Once a ship; three hails a game day (g). A pilot: 4 d | `Encounter.CloseAndHail` is built. The swap goes through `Knowledge.Hear` and `Known.Spoke`. The council is `crew.council.wind` |
| **Rest** | All hands but one sleep for a watch. She lies to, lies to her stone, or lies up | In the open only with the wind under 8 m/s (g). Every man one step fresher. Ashore by a fire all are fresh by morning, and the hot meal mends heart | `Voyage.Rest(health, heart)` is built; tie it to `Watch` |
| **Offer** | An offering or a vow, by your faith: before the open sea, in danger, in a long calm, at landfall, on a feast at sea. A prayer; ale poured; alms at the next church; a vow with a day, which goes on Faith and days and on the day rule | It never moves the weather. It moves Troth, and the heart of each man by his own faith: a prayer said aloud lifts three and leaves Ubbi cold, unless he may pour too. Alms 4 d. Once a watch; one vow unpaid at a time (g) | `crew.faith.offering`, `crew.faith.vow` and the storm_vow chain hold the words. A small table of offerings by faith |

Look out, Sound and Hail are the hand loop. Each press returns something with an age that you keep: a tiding to
tell, a mark on your own chart, a fresher price. By a coast one of them is worth pressing every half minute at
2x. Far out, every two or three minutes.

### 4.5 One thing a watch: what the idle hours are for

**Rule.** At each turn of the watch up to three offers light. An offer is a lamp on the title of its window and
one line in the log. You may take one a watch. It is done in its window while she sails. When the watch turns, an
offer not taken goes dark; most come again. Giving a man a job is an order, and is free.

They exist only at sea, or at a fire on the way, for a plain reason: in port the men are ashore and the clock is
held. And a crossing is the only time you have a passenger.

| Offer | Who, and when | What you do | What you keep | Cap (g) | In hand, and to build |
|---|---|---|---|---|---|
| **A tale** (Chest) | The man who knows the old songs, at dusk or at any fire | He tells the first third: it comes line by line. You read the rest (your wish of 3 October) | The text is yours to quote: `[Lore] Quote` takes only texts you have finished. Lore: one mark of four to a pip. The heart of the men | One a game day | 267 texts, 45,600 words of English; the 43 fitt cards become this. A fitt is three to four minutes of reading. Build: a Told page in `ChestPanel`; `finished` on the item; a leaf that keeps your place |
| **A riddle** (Chest) | Any man, when its thing is in sight: the fog riddle in fog, the anchor riddle as the stone goes down | One try a watch. Think as long as you like | Its reward (41 in `riddles_793.json`); Lore. Three wrong and it rests until you learn something that bears on it (built) | One try a watch | 137 riddles; `crew.riddle.*`; `Chest.Answer`. Build: the trigger from what the ship window shows |
| **A lesson** (Tongues) | A man whose tongue it is, off watch and awake, in a sea under 1.2 m | Six words. He names things on the boat and the water; the ship window lights the thing; you pick the meaning from three. Met again, you type its first letters (g) | A word right three times is yours and stays English in the turning line (`fate_faith_and_self.md` 6c). 25 words to a pip | One a watch, two a game day | `crew.tongue.*`. Build: the glossary of attested words (about 150 for stage 1, checked by someone who knows them); the `tongue` verb, which returns null today |
| **To teach** (Tongues) | The same man | Six lessons from you | He can speak for you at a quay in your tongue; with letters, he can read to you (`Chest.ReaderAboard`) | As a lesson | `Chest.LessonsForLatin` is 6 already |
| **A talk** (Bonds) | A man off watch and awake | One of his stories, and your reply: plain, warm, hard, or one with a tag | The bond moves. At each step he gives one word of his own: a price he knows, a name to say at a port, a landing, a line of a verse. Six words a man: 24 in the boat | One long talk a man a game day | The 31 crew cards counted as filler that are not the song are these: good stories with no stake. They stop being stops. Build: a route `talk` for cards tagged idle; `CrewMan.words` |
| **A mend** (Ship) | A man off watch; with your hands it takes half | Pick the job, the man, the stuff: a chafed sail foot, a weeping seam, a thole strap, water in the bilge | Silver not spent at a yard (12 to 40 d and a day). Left, a fault grows a step in two days of sea | One job a man a watch | Build: the verb `hurt` and four steps for sail and hull (book 4.3); a list of jobs; a mending kit, 6 d |
| **A line over the side** (Hold) | One man, under way near land; or a whole watch lying to | Order it | A day of bread for the boat; a hot meal at the next fire; the lawful food on a fast day | 3 in 10 a watch; 6 in 10 under working gannets, which Look out shows | `Wildlife.Presence` has fish and birds by season |
| **The helm** (Yourself) | Any watch | You take the steering oar | The helmsman sleeps: a fresh pilot when you need one | | `WatchBill` |
| **On deck at a mark** | The watch in which she passes a mark of the water: its bar, its landfall, its sands | Take no other offer in that watch | That line of the verse of the water is yours (book, idea 6). If you were at your letters, your helmsman has it, and it leaves with him | Three or four marks a water | Build: marks in `gates_793.json`; `PassageKnowledge` (`voyage_depth.md` 3.9) |
| **The reckoning** (The word, House, Measure) | Always | Every lot aboard against its port: the word, its age **at landfall**, who is ahead, the likely price | Change port. Set the sale: a floor for each good and a list to buy; on mooring, Trade opens with "As planned" as one press. Choose the tiding to tell first | Free | Build: `Known.Price` with the day of landfall in place of today |

**A passenger is a book that walks off at the quay.** A monk reads your Latin leaf aloud (`Chest.ReadToYou`) and
gives lessons in letters. A pilot sells the lines of his water, 4 to 12 d. A pilgrim knows a road. The fare is 2 to
6 d and his bread (g). He lights offers only until his port, so a long crossing with him is worth more than a
short one. Boniface crossed this way in 716. *Build:* the verb `passenger` (97 uses in the cards; it returns null).

**Reading takes your minutes, not hers.** At 4x a fitt lasts most of a night. The leaf keeps your place and the
caps are by the game day, so no speed earns more than another.

**The leg record counts it.** At the mooring, under the reckoning: "This leg: 9 soundings, 2 lines of the Thames
verse, 12 words of Danish, 2 tales, 3 tidings to tell, a seam mended." A crossing that lost on the wine still paid.

### 4.6 The eighteen windows, while she sails

| Window | What it gives you to do |
|---|---|
| Voyage | The landfall line. The live switches. Change course, Put in, Turn back. The keys a sign lights |
| Trade | The ledger of the port she is bound for. Set the sale |
| Hold | Restow (an hour lying to). First over the side. A cask that weeps. The fish. Bread as days against the way |
| People here | It reads "Aboard and alongside": the four, a passenger, a ship you have hailed |
| Crew | The watch bill. Put a man below. Rations. The word of each man, and what he is doing. Jobs |
| Tasks | Promises against the landfall line. Show measures the way to each. Which to break |
| Yourself | The five words, and under each what on this leg can raise it: "Lore: a tale tonight. A riddle rests." |
| Bonds | Each man and passenger. The talk. The words he has given you |
| The word | The reckoning. The tidings of this leg, and what each is worth |
| Faith and days | The duty of today: a fast, a feast, a Sunday. The prayer at dawn and dusk, one press. Vows owed. The faith of each man |
| Chronicle | The news you hold, with ages. What you saw this leg, first hand |
| Chest | The tale. The riddle. Your notes: pin one where she is now. The verse of this water. Things seen |
| House | The tally sentence, and the pace needed from here: "4.6 d a day" |
| The field | Who is bound where with what, as last seen or told: each with an age, and "gaining" or "dropping" |
| Wind and tide | The wind now. The reading of the weather-wise man, and his record. The tide at the next gate. The moon. Light left |
| Tongues | The lesson. Words learned this leg |
| Ship | Hull, sail and rig as words. The bilge. The jobs. Gear |
| Event | The card |

The Sea workspace of `popups_and_windows.md` 4.2 stands: Voyage; The field and Crew; Wind and tide and Hold. A lamp
on a tab that is out of view is a count on its workspace button.

### 4.7 What stops her, and what only tells

| Kind | Clock | How many | At 2x, about |
|---|---|---|---|
| A sign | Held | Four a game day; never the same one twice in six hours | One in 3 to 5 minutes |
| A card | Held | Two a leg; none on a leg under half a day | One in 10 to 15 minutes |
| A ship alongside | Held | Three hails a game day, and only if you press | Yours to choose |
| The window of your own button | Held while open | As you press | |
| A sighting (a notice) | Runs | 4 to 8 a game day by a coast, 2 to 4 far out (g) | One in 2 to 5 minutes |
| The turn of the watch | Runs | Six a game day | Every 2 minutes |
| An offer | Runs | Up to three lit a watch; one taken | Every 2 minutes |

**Three signs more, for the sea.** The book has five. (1) A man is spent: the bill. (2) Water gains: bail with
two, lie to, or run for a beach. (3) Landfall, "which land is it?": two or three readings by the men, each with a
word for how sure; you choose. Right, and you have a fix. Wrong, and it costs 2 to 10 hours (`voyage_depth.md`
3.5). All three come from the simulation.

**Quiet is allowed.** No watch passes with nothing new lit, and never more than three offers are lit. A dead
watch with all asleep is two quiet minutes at 2x. That is what 4x is for, and what the ship window is for.

### 4.8 Watching pays

**The ship window.**

1. **The sky is the forecast** (book, idea 4). The first trace of a front is in the window 2 to 6 game hours before
   the weather sign (`ship_window.md` P1). Flip the sail switch before the sign and the log says "Shortened in good
   time": the sign does not come, and no harm is rolled for that front (g).
2. **Things are in the window before they are called.** A sail is drawn from the whole sight range; the man
   calls it at 0.6. Brown water, working birds, a far fire. Click the thing: a look at it alone, with no wait, and
   first hand. `ship_window.md` P5 allows five things to click; the sighted thing is one of the five.
3. **The men are the meters.** Who sits where, who sleeps, who bails with both hands, who sits turned away. Weary
   and sour are seen a watch before they are said.
4. **Things seen.** About forty rare sights (`fate_faith_and_self.md` 5): the sea burning in her wake, the lights
   in the north, a whale. They last minutes. If you were watching and clicked: "Seen" in the Chest, first hand,
   yours to tell, and a line of the chronicle in Byzantium. If not, a man tells you in the morning, and it is his
   tale.

**The chart.**

1. Her mark, the pale fan to where dusk finds her, the course with its night bars and dawn ticks.
2. The landfall stretch on the day rule, narrowing as she goes.
3. **Other sails stand where they were last seen, with an age.** They do not move until you see them or are told
   of them again. The gap to a rival is a thing you know, not a thing you are given: it changes when you look out
   or hail.
4. Wind streaks ahead of her: a shift lies on the chart before it reaches her.
5. Tide arrows at the gates, turning on their hours.
6. **Your own soundings and pins in Byzantium.** The chart fills with your hand as she goes, and it passes to the
   heir. With the blot (decision 11) her mark spreads offshore and snaps small at a fix.

### 4.9 The rhythm of one crossing

By the book's way, in a north-east wind of 7 m/s, sailed through both nights: 50.5 game hours.

| Stretch | Game hours | At 2x | How busy | What it is for |
|---|---|---|---|---|
| The river: the Lek, 98 km | 12.5 | 6 min | Light | The first talk and lesson. Craft to hail for the wind outside. Banks to see |
| The mouth and the bar, 9 km | 1 | 30 s | Dense | The tide sign. The offering. The first sounding. A mark |
| The coast toward dusk, 42 km | 5 | 2.5 min | Middling | Fires ashore. The bill for the night. The tale |
| The open sea, 142 km | 17 | 8.5 min | Quiet, with one busy hour | Night watches. The dawn look. Fish. A hail and the reckoning. One card. Soundings from noon. Landfall and its mark |
| The sands, 106 km | 12.5 | 6 min | Dense | The lead going. The tide gates. The choice at dusk. The flat. Other keels closing on one river |
| The river and the strand, 19 km | 2.5 | 1 min | Middling | The hulls on the beach counted. The sale set. The tidings chosen |

Busy at both ends and quiet in the middle. The quiet middle is where the long things go: the tale, the lessons,
the talk, the mending.

## 5. One crossing at 2x, minute by minute

Friday 21 June 793 (a Friday by my sum from the Julian calendar; the 23rd is a Sunday). The book's way, with the
pending link open. Wind north-east 7 m/s, a calm before dawn on Saturday, then north 8, then north-east again.
The speeds are `Travel.Speed`'s. The tide hours are made up for the script: `TideModel` will give the true ones.
One real minute is two game hours. The men are the mock's: Aldgrim the steersman and Ubbi the Dane in one watch,
Folkmar the mate and Liafwin, who knows the old songs, in the other. **(held)** marks the clock stopped.

At the quay, clock held: the talks, four casks, the order. "By the Lek and straight across to Lundenwic, sailing
on at dusk." The measure: 2 to 5 days. Bread for 7.

| Min | Game time | Where | The frame | You |
|---|---|---|---|---|
| 0 | Fri 04:00 | The Lek below Dorestat | Log: "Sunrise. Cast off for Lundenwic." Landfall line: "Sunday morning at best, Tuesday at worst. Next: the Maas bar, before dusk. The two Frisians: a day ahead." Ship window: reed banks, the sail filling, thin high streaks in the west. Aldgrim steers, Ubbi is forward | **Cast off.** The dawn prayer, one press. **Look out**: "A ferry. The smoke of Dorestat astern. High streaks in the west." |
| 1 | 06:00 | The Lek | Three lamps. Bonds: "Folkmar is splicing a sheet. He would talk." Chest: a riddle from Liafwin. Ship: "The foot of the sail is chafed. One man, one watch." | The talk with Folkmar: his story, your warm reply. He gives a word of his own: a name to say on the Strand. Liafwin is put to the sail: an order, free |
| 2 | 08:00 | The Lek | The turn: "The morning watch. Folkmar takes the oar." Notice: "A keel coming up under tow, six men on the rope." | **Hail** her; she is against the bank, no time lost. **(held)** You tell: wheat is dear at Dorestat. She tells: north-east and steady outside; two long ships off Schouwen three days ago, standing north. A dark sail marked "3 d" goes on the chart |
| 3 | 10:00 | The Lek | Lamp on Tongues: "Ubbi is off watch. He would teach you words of the sea." | The lesson: sail, oar, fair wind, bailing, weather, to steer. Six picks, forty seconds. "Danish: 6 words. 19 to a pip." Then The word: wine at Lundenwic, "6 days old; 9 at landfall". **Measure** two places: Lundenwic home with cloth, back by 1 July |
| 4 | 12:00 | The Lek | The turn. Log: "Noon. Bread and cheese." Lamp on Chest: Liafwin asks of the big one that wades in the deep and tugs against the earth (it is in `texts_793.json`) | One try: the anchor. Right. Lore: one mark. Log: "The sail is sound." |
| 5 | 14:00 | The lower Lek; salt in the air | Ship window: the streaks have thickened to a sheet. Wind and tide: "Folkmar: it will back north by morning. Likely." | Nothing asks. You look at Hold: bread 7 days against a way of 2 to 5. Crew: watch and watch for the night |
| 6 | 16:00 | The Maas mouth; Witla on its beach | 16:30 **Sign (held)**, Aldgrim: "The bar at half ebb. Water enough for her as she is loaded. A short sea on it." Keys: Cross now (likely fine). Wait at Witla for slack (2 hours). A pilot from the shore (4 d). Voyage: "A mark of the Maas." | **Cross now.** **Offer**: a prayer before the open sea; Ubbi may pour (a measure of ale; four hearts lift). **Sound** on the bar: "Two fathoms. Hard sand." The first mark in Byzantium. You were on deck: a line of the Maas verse |
| 7 | 18:00 | The coast under Goeree | Lamp on Tongues: a second lesson | You let it pass: this watch was for the bar. **Look out**: "Dunes two leagues to port. No sail. A red Sun: fair tomorrow, Folkmar says." |
| 8 | 20:00 | Off Schouwen. Dusk at 21:10 | The first night watch. Lamp on Chest: "Liafwin has the first fitt." Notice: "Two fires on the beach." | The tale: he tells a third, you read on. You click the fires in the ship window: "Two keels drawn up. No long ships." Kept as a tiding |
| 9 | 22:00 | 22:40: the land drops astern | Stars; the north star on her right quarter. The tale ends: "Lore: two marks of four." The field: "Radbod, by your guess, lies at Witla tonight." | The dusk prayer. You finish reading |
| 10 | Sat 00:00 | The open sea | The dead watch: Folkmar and Liafwin. 00:40 her wake burns green. 01:30 **Sign (held)**, Folkmar: "The wind is dying." Keys: Row, as ordered (two men weary by dawn). Drift (two hours; the landfall line moves an hour) | You click the wake: "Seen: the sea burning." First hand. **Drift**, and **Rest**: all but one sleep |
| 11 | 02:00 | Becalmed. First light at 02:50 | A quiet minute. 03:30 log: "A breeze from the north at sunrise. Five knots." Wind and tide: "Folkmar: right once of once." | The dawn prayer. **Look out**: "No land. No sail. Gannets working to the south-west." A line over the side: an order |
| 12 | 04:00 | Out of sight of land | The dawn watch. Log: "Six mackerel." Ship window: Liafwin bails more than he did. Lamps. Ship: "A seam weeps forward." Bonds: Liafwin. Yourself: the helm | Liafwin to the seam, with tarred wool. You take **the helm** and put Aldgrim below: the pilot will be fresh for the sands |
| 13 | 06:00 | The open sea | You have the oar. A long swell from the north. 07:20 a speck in the ship window, to the west | You click it before Ubbi calls it: "A sail, hull down, standing toward you." |
| 14 | 08:00 | The open sea | The turn. "A Frisian keel, deep, out of the Thames by her course. 2 km south of your way: half an hour." | **Hail. (held)** You tell: no long ships off Schouwen on Friday. She tells: she left Lundenwic on Thursday; wine paid well; two Frisian keels were at the Nore, bound in; cloth is cheap on the Strand; the reeve turns Frankish keels away without a gift. The word: "Lundenwic, wine: pays well, 2 days old. Two keels in before you: about a tenth less." |
| 15 | 10:00 | The open sea | The reckoning, in The word: Lundenwic on Sunday, about 52 d for the four after the reeve; Sarre on Saturday at dusk, a word 20 days old; Gipeswic, no word. 10:30 **Card (held)**, `sea.pilgrims.adrift`: a boat with no oars, three men, one a monk. Take them aboard (an hour; "bread: 7 days, then 5"). Bread and a bearing (half an hour; a day of bread). Stand on (Troth falls; Liafwin will mind) | You hold for Lundenwic. You take them aboard. People here: "Aboard: the four, three pilgrims." |
| 16 | 12:00 | The open sea, shoaling | The afternoon watch; Aldgrim, rested, at the oar. Lamps. Chest: "The monk will read your psalter leaf." Tongues: letters. Voyage: "The Foreland should lift this watch: a mark of the Thames." | You take neither and stay on deck. **Sound**: "Twenty fathoms. Fine grey sand." 13:30 **Sound**: "Fourteen. Sand and broken shell." Aldgrim: "Shell. The long sand is south of us." A fix |
| 17 | 14:00 | 15:00: land | Notice: "Land. White, low in the south-west." Aldgrim: "The Foreland. Sure." A line of the Thames verse. Landfall line: "Sunday morning at best. Sunday: no market." | **Look out**: "A big sail far astern, gaining slowly. Not made out." You mark it. The field: "Radbod?" |
| 18 | 16:00 | Closing the Foreland; round it at 18:00 | The evening watch. Lamp on Chest: the monk and the leaf | You take it. Psalm 106, "they that go down to the sea in ships", turns from Latin as he reads. The leaf is open to you from now |
| 19 | 18:00 | Under Thanet, then the north shore of Kent | **Sound**: "Four fathoms. Mud." The lead is kept going. 19:50 **Sign (held)**, Aldgrim: "Dusk in an hour. The beach under the old wall at Raculf is close. Beyond are the flats, in the dark, on a falling tide." Keys: Lie up till first light (6 hours; "Tomorrow is Sunday: no market. You lose only your place ahead of Radbod"). Sail on, as ordered (five dark hours on the sands: doubtful; likely with all hands and the lead). Lie to her stone offshore | **Lie up.** |
| 20 | 20:00 | 20:50: the beach at Raculf | Ship window: the boat drawn up, a fire, the cauldron. Log: "Mackerel, hot." Landfall line: "Sunday, mid afternoon." | **Wait**: till first light. **Offer**: thanks for the crossing, 4 d of alms at the minster; the monk speaks for you there |
| 21 | 22:00 | By the fire | Lamp on Chest: the second fitt. 23:10 notice, from the man at the fire: "A big sail went by to the west, under the moon." The field: "Radbod: passed Raculf by night. Ahead of you." | The tale |
| 22 | Sun 00:00 | Raculf | Ubbi has the fire. Lamp on Bonds | A talk with Ubbi: why he sits apart from the monk. Then a quiet minute |
| 23 | 02:00 | Raculf. First light at 02:50 | All four fresh. 03:00 log: "Launched on the young flood." | The dawn prayer. The lead going |
| 24 | 04:00 | Across the mouth of the estuary | The dawn watch. **Sound**, three casts: mud, mud, sand. 06:45 **Sign (held)**, Aldgrim: "The flat. Half a fathom on it, and making. Over it saves two hours. Round by the deep is two more. She draws a foot." | **Over.** |
| 25 | 06:00 | The flat | "One fathom. Hard sand." 07:30 notice: "A big keel at her stone off the deep, waiting for water." You are on deck: a second line of the Thames verse | **Look out**: "Radbod of Witla. Twelve casks." Kept as a tiding. The field: "Radbod: behind you, by two hours." |
| 26 | 08:00 | Sea Reach | The morning watch. Lamp on Tongues: letters, from the monk | The first lesson of six toward Latin |
| 27 | 10:00 | The river narrows | Wind and tide: the ebb is against her; the wind holds her at four knots. Trade: the ledger for Lundenwic | You set the sale: no cask under 70 d; cloth if it is under 30. In The word you choose the tiding to tell first. Not the twelve casks astern |
| 28 | 12:00 | 13:15: the minster at Barking | The afternoon watch. Lamp on Bonds: the monk, before he goes | A last talk. He gives a word: who buys wax at his house |
| 29 | 14:00 | 15:30: the Strand | **Look out**: "Two Frisian keels on the beach." Moored. The reveal: "Two Frisians beached on Saturday with wine. It pays a tenth less than the word you bought on. The keel you hailed told you true." The pilgrims go ashore | "What tidings?" |

**The leg, under the reveal:** "Dorestat to Lundenwic: 59 hours, two nights. 8 soundings marked. A line of the
Maas verse and two of the Thames. 6 words of Danish. A lesson in letters. Two tales and a riddle. Three talks. A
sail and a seam mended. Three tidings to tell. One thing seen."

**The count.** 30 minutes of clock and about 2 of holds. Seven holds: four signs, one card, two hails, the hails
by your own press. Fifteen watches: an offer taken in thirteen, three of them the deck at a mark; about thirty lit.
About twenty presses of Look out, Sound and Measure. Every one of the eight buttons was used. Three quiet minutes:
5, 11 and 22. At 4x it is 15 minutes with the same events. At 1x it is an hour, and the ship window is the
reason to choose it.

**What made the stops decisions.** The bar was safe because she was loaded as she was. The calm cost an hour
because the men were kept fresh. The pilot was fresh at the flat because you steered at dawn. Raculf cost
nothing because the next day was a Sunday, and the day rule had said so since Friday. The flat was open to a boat
that draws a foot and shut to a keel that draws a metre: the one thing the small boat is better at.

## 6. What to build, in order

This replaces step 5 of stage 1 ("Sail on"). Steps 1 to 3 make a crossing readable. Steps 4 and 5 make it worth
its minutes. Nights are guesses.

| # | Step | Nights (g) | The test |
|---|---|---|---|
| 1 | The clock at sea (4.1); `Watch` and its turn; open the 4 km link at the Maas mouth, and find why four ports have no way | 1 | A bot crossing at 4x: the clock never runs faster than 4x; a log line at each turn; `Course.Plan` finds a way to each of the twelve ports |
| 2 | Four `CrewMan` records, `WatchBill`, weary; the rows in Crew; the four heads (the book's step 9, brought forward) | 2 | Two nights at sea with all hands: four weary men. A night ashore: four fresh |
| 3 | The landfall line and its gates, on the walker of the measure (the book's step 7); the tide at a place | 2 | The true hour of mooring lies between the two counts in 9 seeded runs of 10 |
| 4 | Look out, Sound, Hail with a word for a word; `SeaTiding`; `SoundingMark` and its chart layer | 3 | A tiding kept at sea is offered at "What tidings?". Twelve casts name a water. The price a hailed ship gives is as old as her passage |
| 5 | The offers: the dealer at the turn; the tale (the Told page); the riddle by what is in sight; the talk route for idle crew cards; jobs (needs `hurt`); the line over the side | 3 | No watch with nothing lit. Never more than three. One taken. The caps hold at 1x and at 4x |
| 6 | Lessons and the glossary; `passenger` | 2, and the glossary | A word right three times stays English. The monk is gone at the quay |
| 7 | The three sea signs; the click on a sighted thing in the ship window; things seen | 2 | A weather sign does not come when the sail was shortened first |
| 8 | The leg tally under the reckoning; the bot's crossing | 1 | The longest stretch at 2x with nothing new lit is under 130 real seconds. Four to nine holds. Dorestat to Lundenwic: 10 to 35 real minutes at 4x |

About 16 nights (g). The words are yours: about 40 offer lines, 24 words of the four men, 150 glossary words.

**The test by a person.** The three questions of section 2, asked at five random seconds of a crossing. And the
book's own last pass mark, moved to the sea: does the tester cast off on a second crossing unasked?

## 7. Yours to rule

1. **At sea the windows do not hold the clock.** Yes? (`popups_and_windows.md` asks the same.) Without it,
   nothing in section 4.5 fills a minute.
2. **Rest and the tide wait lose their fast rates** (half an hour and an hour a second), by your ruling of
   7 October. A June night ashore is then about 3 minutes at 2x. Yes?
3. **One offer a watch, with caps by the game day.** Or all offers free, and only the caps?
4. **The lesson as six words picked in a window.** Or tongues learned only by talk and the turning line?
5. **The line of a verse is lost to you when you were not on deck at the mark.** Or only late: the helmsman
   gives it for a talk?
6. **May a Christian skipper let Ubbi pour to Thor at the river mouth?** I have it cost ale and nothing in Troth,
   since you said your own. The other reading: it costs Troth and buys his heart.
7. **The watch of four hours as the game's own device,** named in plain words and owned in the credits.
8. **The way to the sea.** Open the Lek at the Maas mouth, as the book and the place files assume? Today she goes
   by Traiectum and the Old Rhine.

## 8. Not checked

- No run in Unity. The minutes come from my walk of `lanes.bin` with the router's costs (a plain shortest-way
  search, not the game's A-star: same costs, ties may fall otherwise) and a copy of `Travel.Speed` in a steady wind.
  The game's weather is not steady.
- Why the 4 km at the Maas mouth are pending, and whether opening them is safe. Why Quentovic, Ripa, Doccinga and
  Fordwic have no way: I saw only that with pending lanes open they do (371, 563, 374 and 331 km).
- `SeaWatch`'s real share of the sight range at which a sail is called (I wrote 0.6). Whether the world's ships
  wait for a tide, as the script has Radbod do.
- The weekday of 21 June 793 and the moon (about 8 days old, half lit, by a mean month): my sums.
- Games, from memory: the seven-second rule and the news read aloud in Elite; Something Awaits You in Sunless
  Sea, in its details; sleep in Sailwind; the hours of a skill book in The Long Dark; strength from a heavy pack
  in Kenshi; talk on a leg that opens routes in 80 Days. Four wiki pages refused the fetch.
- The period, from memory: Willibald on Boniface's crossing; Maxims I; Bede III.15 and IV.22; the Mirror of the
  King; the Norse sea law; the Rhine miracle of 839; the toll charters of Lundenwic; Raculf as the name of
  Reculver in 793; whether its beach was a landing. Nothing here gives a Frisian a prayer or a libation in words:
  none survives.

## Sources

- In the project: `Assets/Kapman/Code/Game/Chart/ChartBody.cs`, `UI/GameSpeed.cs`, `Sim/Travel.cs`, `Sim/LaneGraph.cs`, `Voyage/Course.cs`, `Voyage/Rest.cs`, `Voyage/VoyagePlanner.cs`, `UI/PanelClock.cs`, `Chest/Chest.cs`, `Core/World/WorldFrame.cs`; `Game/Data/ship_classes_793.json`, `voyage_rules.json`, `ports_793.json`, `texts_793.json`, `riddles_793.json`, `encounters/sea_793.json`, `crew_793.json`; `G:/kapman_unity/data/baked/world/lanes.bin`.
- Fetched, games: https://en.wikipedia.org/wiki/Dredge_(video_game) ; https://en.wikipedia.org/wiki/Euro_Truck_Simulator_2 ; https://en.wikipedia.org/wiki/Death_Stranding ; https://en.wikipedia.org/wiki/The_Long_Dark ; https://en.wikipedia.org/wiki/Kenshi_(video_game) ; https://en.wikipedia.org/wiki/Roadwarden ; https://en.wikipedia.org/wiki/80_Days_(2014_video_game) ; https://en.wikipedia.org/wiki/Elite_Dangerous
- Fetched, the period: Rimbert, Life of Anskar https://sourcebooks.fordham.edu/basis/anskar.asp ; the saga of Grettir https://sagadb.org/grettis_saga.en ; The Seafarer https://oldenglishpoetry.camden.rutgers.edu/the-seafarer/ and https://en.wikipedia.org/wiki/The_Seafarer_(poem) ; Hurstwic on Norse ships http://www.hurstwic.org/history/articles/manufacturing/text/norse_ships.htm ; https://en.wikipedia.org/wiki/Gokstad_ship ; https://en.wikipedia.org/wiki/Hnefatafl ; https://en.wikipedia.org/wiki/Brendan_the_Navigator ; https://en.wikipedia.org/wiki/Tim_Severin
- Not reached: the Sunless Sea, Dredge, Sailwind and Long Dark wikis; Larson's Mirror of the King; the Sea Stallion's own pages.
