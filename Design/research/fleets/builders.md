<!-- Research pass, 6 Oct 2026. Deepens Design/research_scale_to_fleets.md for eight builder and transport games.
How it was checked: the web search ration was already spent for this session (every search was refused), so nothing here
comes from a search. Checked pages: the Factorio wiki and two Factorio developer posts (read in full), the Satisfactory
wiki, the Captain of Industry wiki, the OpenTTD manual, Soren Johnson's Designer Notes, Wikipedia, and the public Steam
review pages of all eight games (about 200 reviews each, read as raw text, so the player quotes are word for word,
spelling included). Everything marked (?) is from memory and was not checked against a page. Player quotes are what
single players say, not statistics. -->

# How builder games grow from one unit to many, and what Kapman should take

**The ask (4 Oct):** "research a way for the game to scale from slow and methodical at the start to managing fleets of
traders, and how that end game will look from a gameplay look standpoint."

**The short answer from these eight games.** Every one of them grows the same way: the player makes a thing by hand,
then makes a *rule* that makes the thing, then copies the rule. The games players still love at hour 200 are the ones
where the rule is one object you set once (a blueprint, a line, a shared order list) and where the hand tool stays
useful. The games players leave are the ones where the late game is the first hour done fifty times, or a list of
alarms. None of the eight keeps the first hour's feel all the way. Kapman can, because it already has the one thing
they lack: a reason the owner must still go himself.

---

## 1. The eight at a glance

| Game | The one unit | The many | What turns one into many | Hours to the first hand-over | Feel of the first hour at the end |
|---|---|---|---|---|---|
| Factorio | You, with an axe and a furnace | A factory of thousands of machines, trains, robots | Research bought with what the factory makes; blueprints | 5 to 15 (?) to construction robots | Lost on purpose; the expansion gives it back by landing you on a new planet |
| Satisfactory | You, at a craft bench | The same, in first person | Milestones paid in parts; Space Elevator phases | 1 to the first miner; no builder robots, ever | Kept (you still place every machine), and that is the complaint |
| Anno 1404 | One ship and a warehouse | Ten to thirty islands, dozens of ships | Counts of people in each class | 2 to 4 (?) to the first route | Mostly kept; the island you started on is still the heart |
| Anno 1800 | One ship and a trading post | Five regions, dozens of islands, 50 to 150 ships (?) | Counts of people; influence points | 2 to 3 (?) | Lost under routes and alerts |
| Offworld Trading Company | One headquarters, a few tiles | 15 to 20 tiles (?), no more | Headquarters level gives land claims | None: nothing is ever handed over | Kept, because a match is 20 to 40 minutes (?) and ends |
| Captain of Industry | A wrecked ship, a handful of trucks | 100 to 200 vehicles (?), trains, cargo ships | Research; a vehicle cap raised by research (?) | None needed: the trucks decide for themselves from minute one | Kept in view, lost in pace |
| Transport Fever 2 | One line, two horse carts | 80 to 200 lines, hundreds of vehicles | Money only | 0.2: the line is the first thing you make | Lost: money stops mattering |
| OpenTTD | One train between two stations | 500 to 5,000 vehicles | Money only | 0.2 | Lost unless the player sets his own goals |

---

## 2. Factorio

**How one becomes many.** There is no menu for it. Each tool is a research the factory has to pay for with its own
output, and each one hands over a job you have been doing with your hands:

1. *By hand.* You mine ore by holding a button, feed coal into a stone furnace, and craft from your pack. The first
   research (Automation) gives the assembling machine. From here a machine does what your hands did.
2. *Belts and arms.* Belts and inserters move things between machines. You place every one.
3. *Trains.* A train has a schedule: a list of stops, each with a wait rule ("until full", "30 seconds"). The schedule
   is set once per train and can be copied.
4. *Construction robots.* The research costs 100 each of the first three science packs and gives the robot, the
   roboport, and one more thing: destroyed buildings "will leave behind ghosts" that robots rebuild. After this you
   stop placing buildings. You place a *blueprint* (a saved drawing of any part of the factory) and the robots carry
   and place every piece.
5. *The logistic system.* A fourth science pack buys requester chests: robots now fetch as well as build.
6. *The rocket*, and in the 2024 expansion, other planets.

The gate is always the same: you cannot have the tool until the factory can already make the thing by the slower way.

**Minute to minute.**
- *Start:* walk, mine, carry, craft, place one machine. One decision every few seconds, each tiny and physical.
- *Middle:* lay out a production block: count machines, run belts, find the bottleneck, walk to it.
- *End:* open the map, find the red number in the production table, paste a blueprint of a whole block next to the
  rail, wait for robots, check the number again.

**The screen at the end (to draw).** The map view, zoomed far out, almost black. A regular grid of thin grey rail
lines cuts it into squares. Inside each square a dense block of small coloured rectangles (machines) with bright
moving specks (trains) on the rails. Roboport cover shows as overlapping pale yellow and green squares. Top right, a
strip of small alert icons, each blinking, each a click to jump there. One window open over the map: the production
table, two columns of item icons with bars and numbers per minute, and a line graph under them. The engineer himself
is one pixel somewhere, and has not walked in an hour.

**What players praise.**
- "It starts out small, having you just manage a few mining drills that you need to refuel by hand as they slowly
  deliver you basic resources, but before you know it you've got a massive factory" (609 hours) [F1]
- "the one thing that always keeps the crown where it belongs is the blueprinting and my own personal robot army"
  (2,079 hours, has finished Factorio, Satisfactory and Dyson Sphere Program) [F2]

**What players hate.**
- "Just nothing, before bots it feels awful and once you get to bots its stand in place simulator." (194 hours) [F3]
- "As you go through the tech tree you don't find new challenges, you just increase the number of times you have to
  replicate solution you found hundreds of hours ago." (26 hours) [F4]
- "I like building the factory, I don't really care for copy-pasting massive repetitive chunks and having the robots
  build them." (77 hours) [F5]
- "I guarantee you more than 50% of my play time is standing around waiting for things I'm hand-crafting" (680 hours)
  [F6]

**The makers saw the same thing.** In January 2018 the developers wrote two posts on whether robots had spoiled their
own game. Twinsen: "Bot bases are usually less complex and less interesting to look at, manage and expand" and "When
you learn that bots exist and how they work, building bases with belts just seems tedious." [F7] A week later kovarex
gave the best single statement of the problem I have found:

> "It is kind of fun getting to the bots setup, and moving to the bot-only factory, but once it is achieved, the
> continued play feels very dull for me, because I know that this is it" [F8]

His answer was not to take the tool away. It was that the tool should clear the small things and leave the large
ones to the player: "They should still be a big and powerful advancement when they are researched ... It should solve
the little stuff that isn't that powerful", and "the strongest strategy would be to combine all types of transport,
each for the part where they are the strongest." [F8]

**First hour kept or lost.** Lost by design: nobody mines by hand at hour 50. The 2024 expansion, Space Age, is in
effect an admission that the first hours are the best ones: each new planet has its own rules and you arrive with
little, so the player gets a second, third and fourth early game inside one save (?).

---

## 3. Satisfactory

**How one becomes many.** The same ladder as Factorio, seen from eye height, with gates you can read off a list.
You build a HUB, and each *milestone* is bought by carrying parts to it. Tiers 1 and 2 are open after the tutorial;
tiers 3 and 4 open when the Space Elevator is sent Phase 1 (50 Smart Plating); 5 and 6 at Phase 2 (1,000 Smart
Plating, 1,000 Versatile Framework, 100 Automated Wiring); 7 and 8 at Phase 3; 9 at Phase 4. [S1] [S2] What each tier
hands over: the first miner at the end of the tutorial, splitters at tier 1, the assembler at 2, the tractor and
truck station at 3, the blueprint designer at 4, trains at 6, drones at 8. [S1]

Two things are different from Factorio and both matter to Kapman:

- **There are no builder robots.** To the end, the player places every machine and belt himself. The blueprint
  designer is a small box (four foundations square at first (?)) and whatever fits in it can be stamped down.
- **The Dimensional Depot.** A late research lets you feed parts into a building and then build from that stock
  anywhere on the map: "cloud storage" that starts at 15 items a minute and one stack per part. [S3] It removes the
  walk home for parts and nothing else.

**Minute to minute.**
- *Start:* pick up ore by hand, craft at a bench, carry plates to the HUB.
- *Middle:* stand on a platform, place eight machines in a row, wire and belt each, walk the line to see it run.
- *End:* a plan in a spreadsheet outside the game; then an hour or more of placing what the plan says.

**The screen at the end (to draw).** Still first person: your own gloved hand and a build gun in the lower right.
You stand on a concrete floor high in the air. In front, forty identical machines in two rows, belts between them in
stacked layers, orange and grey. Beyond the floor edge, alien cliffs and a purple forest far below, a monorail on
pylons curving off, a drone crossing. In the middle of the screen a pale blue hologram of the next machine, waiting
for a click. No map, no list, no numbers except a small part counter. It is the most beautiful end screen of the
eight and the one with the least information on it.

**What players praise.**
- "I actually stayed in the mid-game for hundreds of hours just because I felt like it." (815 hours) [S4]
- "The freedom to build, rebuild, and scale without pressure makes the experience both relaxing and deeply engaging."
  (905 hours) [S5]

**What players hate.**
- "I had a lot of fun for the first 40-50 hours of the game ... but now when I've gotten to the endgame I feel burnt
  out on doing the same basic menial tasks over and over." (86 hours) [S6]
- "Early game was nice, late game feels worse than actual work." (201 hours) [S7]
- "So much of the time you spend doesn't feel like you are spending it making decisions or planning your factory but
  rather just really slowly implementing a plan tediously for an hour." (27 hours) [S8]
- "It clicks at some times, but realizing that past a certain point you have to do whatever you did in the last 20-30
  hours but five times bigger, is tedious." (43 hours) [S9]
- "Unlike Factorio, blueprints are also very limited and the game doesn't give you good enough tools to counter the
  ever increasing player workload." (156 hours) [S10]
- "The game goal feels a bit hollow, and the time it takes to accomplish things can make it really draggy (late game
  in particular)." (2,267 hours, and still recommends it) [S11]

**First hour kept or lost.** Kept, and it is the cautionary tale. The verb never changes, the view never changes,
and the work grows by a factor at every tier. Keeping the hand tool is not enough: if the hand tool is the *only*
tool, the late game is the early game times fifty.

---

## 4. Anno 1404

**How one becomes many.** Not by buying units: by counting people. Houses of peasants become citizens, then
patricians, then noblemen, when their needs are met; in the Orient nomads become envoys. Each class at a set head
count unlocks the next buildings. A city cannot be all noblemen: "a number of peasants are required to support a
moderate number of citizens who in turn support a number of patricians and noblemen." [A1] The class above always
wants a good that does not grow on the home island (spice, then indigo, then quartz), so every step up forces a new
island and a new ship. The Orient is gated a second way, by honour spent with the Grand Vizier (?).

Ships come from your shipyard or as gifts for quests. A trade route is made on the sea chart: pick the warehouses in
order, and for each stop say what each cargo slot loads or unloads; then put ships on it (?).

**Minute to minute.**
- *Start:* place a fisher's hut, a market, ten houses; sail the one ship by hand to buy tools.
- *Middle:* settle a spice island; make the first route; watch the first load come home.
- *End:* tune twenty routes, feed a cathedral site, answer the Sultan's and the Lord's quests.

**The screen at the end (to draw).** A warm, painterly island seen from above at an angle: a walled town of red
roofs climbing to a cathedral still in scaffolding, a harbour with stone quays and a dozen cogs at anchor, fields in
strips. A round minimap bottom left; a build menu along the bottom; the class faces top with green or red bars under
each. Small paper notes slide in at the left: a ship arrived, a quest. It still looks like the first hour's town,
grown.

**What players say.**
- "Not overly complicated with micromanaging." (103 hours) [A2]
- "There is always another problem to solve, another island to settle, or another trade route to optimize, and
  that's what makes it so addictive." [A3]
- "It is a fun time but a bit micromanaging for everything," (85 hours) [A4]
- "I have a fleet of 460 ships, 99% were given for quests by the Sultan after building the Mosque." (338 hours) [A5]
- "Also, trading ships stay at ports a bit too long; that duration could be shorter." (59 hours) [A6]

**First hour kept or lost.** The best of the eight at keeping it. One reason: the world is one map and one screen
of sea, so the home island is always a click away and always the centre. A second: the ship count grows by gift and
by need, not by a purchase the player must tend. The 460-ship quote shows the other side: a fleet the player did not
choose is a fleet he does not know.

---

## 5. Anno 1800

**How one becomes many.** The same class ladder, longer: farmers, workers, artisans, engineers, investors; then the
New World with its own two classes; then, with the add-ons, the Arctic, Enbesa and more (?). The game has
"multisession gameplay": each region is its own map, running at the same time. [B1] New World goods are needed to
raise Old World classes, "necessitating established trade routes". [B1] A second budget, influence points, is spent
on ships, routes and other things, so fleet size trades against other powers (?).

The trade route screen (?): the region map fills the screen, routes listed down the left. A route is a row of stops;
under each stop, one column per cargo slot with an up or down arrow and a number of tons. Several ships can share one
route. A *charter* route rents an unseen ship for one good from A to B for a fee, the tool for the player who has no
ship to spare. A statistics screen shows each island's make and use of every good.

**Minute to minute.**
- *Start:* a pier, a market, farmers' houses, potatoes. One ship, moved by hand.
- *Middle:* the first rum and cotton routes across the ocean; the first shortfall at home.
- *End:* in the player's own words below.

**The screen at the end (to draw).** A dense, smoky Victorian city, magnificent in detail. But the eye does not
rest on it. Down the left edge, a stack of round notification icons. Top centre, a row of region tabs. A panel open
on the right: the route list, forty or more rows, each a coloured line with small ship icons and a warning triangle on
three of them. Over the city, a second window: the statistics table, goods in rows, islands in columns, red numbers.
A voice says a ship is under attack.

**What players praise.**
- "However, complexity is unlocked step by step and never feels overwhelming." (107 hours) [B2]
- "I've colonised multiple islands over three different regions, have established an extensive trade network with
  dozens of ships ... I'm hundreds of hours into my current playthrough and keep discovering new things." (456 hours)
  [B3]

**What players hate.**
- From the same 107-hour review that praised the pacing: "Five worlds are overwhelming!" and "The trade route system
  is one of the biggest pain points. The lack of a flag to disable ware overflow makes it practically impossible to
  use one ship for multiple wares." [B2]
- "You're endlessly jumping between cities, expeditions, quests, trade routes, notifications, rarely making meaningful
  strategic decisions, mostly just reacting." and "That's spreadsheet busywork disguised as gameplay." (58 hours) [B4]
- "Too much micromanagement ... constantly changing requirements/optimalizations in mid/late game, constant
  distractions." (29 hours) [B5]
- One player wrote the late game as a dialogue: "system: your ship is under attack. player: i know system: your ship
  is under attack. player: a moment system: your ship is under attack." (83 hours) [B6]
- "it was very complex to manage, especially once you get to the New World." (15 hours) [B7]
- GameSpot's headline for its review: "The Prettiest Spreadsheet". [B1]

**First hour kept or lost.** Lost. Three causes, each one a thing Kapman can avoid: the world is split into maps
you must jump between; the game speaks to you in a stream of alerts you did not ask for; and a route breaks silently
(a full warehouse, a ship that carries the wrong mix) so the player must patrol his own list.

---

## 6. Offworld Trading Company

**How one becomes many.** It mostly does not, and that is the lesson. You scan a patch of Mars, found one
headquarters, and receive a handful of land claims. Each headquarters upgrade (four of them) costs more goods and
gives more claims; a few more come from auctions and the black market. Each claimed tile holds one building. That is
the whole growth: from about four tiles to perhaps fifteen or twenty (?). There are no units to command at all. The
designer's rule sheet says it flatly:

> "Units cannot be controlled directly by the players and are instead generated automatically based on player
> actions." [O1]

Goods move by themselves; power sells itself; food and air buy themselves if you run short, on credit, and "the
automatic purchase increases the company's debt, which has a very negative effect on its stock price." [O1] The game
ends when one company has bought the others' shares, and a player is warned "when an opposing player earns enough
money to buy them out". [O2]

**Minute to minute.**
- *Start:* scan, choose the founding tile (the most important choice of the match), claim three deposits.
- *Middle:* watch thirteen prices; move a building from the cheap good to the dear one; buy a share.
- *End:* a race of seconds: sell everything, buy the last block of a rival's stock before he buys yours.

**The screen at the end (to draw).** A rust-red hex map, the player's fifteen tiles outlined in his colour with a
small glowing building on each and thin rail lines between them. Down the left side, the whole game: a column of
thirteen goods, each with a price and a small green or red arrow, flickering. Down the right, the rival companies as
portraits with a share price and a row of ten share boxes filling with colours. Bottom centre, a short row of
building buttons. Nothing else. The first minute's screen had the same parts.

**What players say.**
- "The game replaces physical territorial warfare with pure economic warfare; watching your rivals fail because you
  successfully cornered the global market on water or electronics provides a rush of intellectual satisfaction" [O3]
- "Early matches can feel overwhelming" and "There are no large-scale battles or dramatic confrontations, and much of
  the action takes place within menus and data displays." (13 hours) [O4]

I found no player complaining of a tedious late game here, and I think the reason is structural: there is no late
game to be tedious. The claim cap is the governor, and the buyout ends the match at the moment one side has won.

**First hour kept or lost.** Kept, at the price of having no hour 100 of a single game. For Kapman the useful part
is the hard cap that makes every slot a choice, and the fact that the market, not a unit list, is what the player
watches.

---

## 7. Captain of Industry

**How one becomes many.** You start with the many, small. A wrecked ship, a few workers, a few trucks and an
excavator. The trucks are never given orders: they see that a building needs or offers something and go. One player
put it exactly: "Factorio but you start with both construction bots and logistic bots, all buildings are
automatically connected to logistic network, but it balances out with more realistic recipes and fact everything uses
diesel and/or electricity, spare parts and manpower (so food)." (181 hours) [C1]

So the growth is not in handing over. It is in *feeding* what you already handed over. Bigger trucks, excavators and
trains come by research and are built at a vehicle depot [C2]; the number you may run is capped and research raises
the cap (?).

At sea the game has two kinds of ship, and this is the nearest thing in the eight to Kapman's plan:

- **The Ship.** One, yours, the wreck you arrived in, repaired. You send it by hand over a world map to find
  villages, mines and oil rigs, and arm it against pirates.
- **Cargo ships.** Found damaged on the world map and repaired (the nearest for 240 iron), or bought from villages.
  A cargo ship is fixed to one cargo depot, and its size is set by the depot. [C3] It runs a *contract*: a standing
  swap with a village, set up once for a one-time fee and a monthly fee in Unity, the game's goodwill currency (6 to
  24 to open, 0.1 to 0.4 a month). [C4] After that it sails back and forth unseen.

**Minute to minute.**
- *Start:* place a farm, a furnace, a fuel tank; watch four trucks do it.
- *Middle:* dig a mountain away for room; find why maintenance has run out; add fuel; add food.
- *End:* balance by-products across a dozen linked chains; launch rockets.

**The screen at the end (to draw).** An island seen from above at a steep angle, half of it terraced flat by
digging, the spoil dumped into the sea as new grey land. Dozens of yellow trucks in files on dirt roads like ants.
Refinery towers, pipes in bundles, a container port at the shore with one large ship and stacked boxes. Top bar: a
long row of small counters (people, food, fuel, maintenance, Unity) each with a tiny plus or minus. Bottom: a tool
row. No list of vehicles is open, because nobody needs one.

**What players praise.**
- "Truck-based transportation dominates in the early game which presents some interesting challenges and also a great
  'colony of ants' visual presentation." and later research means "you don't need to scale your basic builds
  horizontally as much as in, say, Factorio, where you might simply copy-paste your iron smelter setup 10 times" (185
  hours) [C5]
- "You start with simple mechanics and fuel production and end up, after many hours of play, with full automation,
  nuclear power, advanced chemical processes and global transport via container ships." [C6]

**What players hate.**
- "Everything has an upkeep cost in maintenance and fuel consumption, so your production of maintenance and fuel needs
  constant expansion as your truck fleet expands. By the mid-game, I felt like every time I expanded my factory by a
  little bit, I'd have a lot of tedious work getting the support to catch up afterward." (346 hours) [C7]
- "I think the midgame can be quite slow when you need to wait to finish terraforming a bunch of land" (601 hours) [C8]
- On the two kinds of ship: "ships in game are almost useless. you cannot use your own ships on your own maps ... you
  just simply cannot use them outside of 'contracts or world mines'." (461 hours) [C9]
- "One thing I think needs an overhaul is the world map and 'the ship'" (399 hours) [C10]
- "Only annoying part is getting started, because the first hour of a new island is just a single build-order." (34
  hours) [C11]

**First hour kept or lost.** The view is kept: you watch the same ants. The pace is lost: upkeep makes every step
forward cost two steps of support. And the warning for Kapman is in the last three quotes. The game has "one ship of
your own plus hired cargo ships", the very shape Kapman plans, and players find it the weakest part, because the
owner's ship does little that matters and the cargo ships do nothing that can be seen.

---

## 8. Transport Fever 2

**How one becomes many.** By money alone. The game runs from 1850 to today [T1] and nothing is locked but vehicles
that have not been invented yet. The unit is the *line*: click the stops in order, and the line exists; buy vehicles
at a depot and assign them to it. More vehicles on a line shorten its interval. A player who knows both games: "I
think the devs played around with OpenTTD and worked out how they could simplify and streamline some of the game
mechanics, for example you build a linked-route and apply vehicles to routes, if you need to replace or add more
vehicles to a route then you can do that incredibly quickly and easily." (28 hours) [T2]

**Minute to minute.**
- *Start:* a loan, two towns, a cart line for bread; ride along in the cab to watch.
- *Middle:* the first long railway; signals; a line that loses money and why.
- *End:* open the line list, sort by age, replace vehicles; add capacity where a number is red.

**The screen at the end (to draw).** A wide green landscape in near-photographic 3D, a city of towers in the middle
distance, three double-track railways and a motorway converging on it, a jet on approach. Over the right third, the
line manager: a scrolling list of 80 rows, each with a colour swatch, a name, a vehicle count, an interval in
minutes, and a profit figure; one row expanded to show twelve identical trains with small condition bars, several
orange. The landscape is why people play; the list is where the late game is spent.

**What players praise.**
- "There's something weirdly relaxing about just watching your trains, trucks, and ships do their thing once
  everything is set up properly." (24 hours) [T3]
- "To me its like building a model railroad, but still having a strategic side of finances, trade routes, and time
  tables." (225 hours) [T4]

**What players hate.**
- "The only thing that really bothers me about it is the lack of an auto-replace feature for vehicles that've reached
  the end of their lifespan- it leads to a situation where I spend more time micro managing maintenance levels across
  80 lines as opposed to playing the rest of the game." (790 hours, recommends it) [T5]
- "Money becomes completely meaningless within the first hour of play." (32 hours) [T6]
- "Cluttered UI for Fleet Management: Replacing, modifying, or monitoring dozens of lines and hundreds of individual
  vehicles becomes a massive chore" and the verdict "fun for the first 15 hours, but ruined by ... repetitive
  late-game gameplay." (49 hours) [T7]
- "Other than that, your just focusing on expanding and maintaining your fleet of vehicles." (48 hours) [T8]

**First hour kept or lost.** Lost twice over. The first hour is a tight puzzle (a loan, one line that must pay).
Once it pays, money never binds again, so there is no puzzle; and age wears every vehicle out, so there is a chore.
The good half: the line as the unit is right, and riding along in the cab is this game's version of Kapman's window.

---

## 9. OpenTTD

**How one becomes many.** Money alone again, from 1950 to 2050 by default, about a day of play. [P1] The unit is
the vehicle with an order list: go to this station, "Full load", go to that one, "Unload all". [P2] The game's whole
answer to scale is four tools, all added by the community over the years on top of the 1995 game:

1. **Shared orders.** Hold a key when copying and the new vehicle does not get a copy of the list, it gets *the*
   list: "When you modify the orders, all linked vehicles update simultaneously." There is no master vehicle. [P2]
2. **Order checking.** The game itself looks for vehicles with too few orders, a deleted station, or a station that
   cannot take that vehicle, and tells you. [P2]
3. **Groups and autoreplace (?).** Vehicles are put in named groups; a rule says "replace every engine of type A in
   this group with type B when it next visits a depot", and autorenew swaps a worn-out vehicle for a new one of the
   same kind without asking.
4. **Conditional orders and timetables.** "If load is under 50 per cent, skip to order 4." [P2]

The company's vehicle count is capped by a setting, by default some hundreds of each kind, at most 5,000 (?).

**Minute to minute.**
- *Start:* "The first few hours will probably see you running out of money while trying to build a long train route
  and spending a few brain-scratching hours figuring out how train signals works." (129 hours) [P3]
- *Middle:* join lines into a network; first junctions; first jam.
- *End:* rebuild a main line from two tracks to four without stopping the trains on it; chase the one jam.

**The screen at the end (to draw).** Small bright pixel art, seen from the classic angle. The map is zoomed out
until towns are grey patches; a lattice of rail in the company colour runs between them, four tracks wide on the
trunks, with trains as short coloured dashes nose to tail. Six small grey windows float over it, each with a title
bar in the company colour: a vehicle list (number, a coloured coin for profit, this year and last year), a group
list, an order list of twelve lines, a station rating window, the company graph, and a small view locked on one
junction. Along the bottom a yellow news line: a new locomotive is on offer.

**What players say.**
- "Great game, hundreds of hours always be spamming path signals." (359 hours) [P4]
- "when I first tried it, I got overwhelmed by the map and interface and had no idea what to do." (203 hours, now
  recommends it) [P5]

I did not find a quotable late-game complaint in the 200 reviews I read, and could not search the forums. From long
acquaintance (?): the common ones are that money is beyond counting by 1970, that the built-in rivals offer no
contest, and that the game after that is whatever the player sets himself. The many who stay are playing a different
game from the first hour: network design, where the trains are a flow and no single one is ever looked at.

**First hour kept or lost.** Lost, by a change of subject that players welcome. The lesson is narrow and valuable:
of all eight, this game has the most complete kit for managing many units, and every piece of it is a way of *not
looking at the single unit*. One list for many vehicles; the game checks the lists itself; replacement is a rule.

---

## 10. What the eight agree on

| Finding | Seen in | Against it |
|---|---|---|
| The unit of management is a rule, not a vehicle (a blueprint, a line, a shared order list) | Factorio, Transport Fever 2, OpenTTD, Anno | Satisfactory has no such rule and pays for it |
| A hand-over tool that solves everything empties the game | Factorio (its own makers), Transport Fever 2 | |
| No hand-over tool at all makes the late game the early game times fifty | Satisfactory | |
| Growth with no rising cost makes money meaningless early | Transport Fever 2, OpenTTD | Captain of Industry, where upkeep is the game |
| Upkeep that grows as fast as the fleet turns growth into a chore | Captain of Industry | |
| Things that break silently force the player to patrol his lists | Anno 1800, Transport Fever 2 | OpenTTD checks orders itself |
| Unasked alerts are the most hated part of a large game | Anno 1800 | Offworld, which shows one warning that matters |
| A hard cap on slots keeps every slot a choice | Offworld | Anno 1404, 460 gift ships |
| A fresh start inside the same game restores the first hour | Factorio Space Age (?), each new Anno region | Anno 1800 has too many at once |
| The owner's own unit, if kept, must do something the hired ones cannot | Captain of Industry gets this wrong | |

---

## 11. Five lessons for Kapman, each with one proposal

### Lesson 1. The hand-over should clear the small things and leave the large ones (Factorio)

kovarex's test: a good tool gives the feeling of power and takes away the fiddling, but must not be "the one
solution for everything", or "this is it". The three-crossings rule in the first report says *when* a route may be
handed over. It does not yet say *which* routes may never be, or how well a skipper does.

**Proposal: the skipper's ceiling.** A skipper sails a handed-over route to *your own middle result*, never your
best. The game keeps your three tallies for the route; his season is rolled around the middle one, moved by his own
skill and luck. And three kinds of voyage cannot be handed over at all, by rule, shown in the House tab as a short
list titled "Yours to sail": a first call at any port; any open-sea crossing longer than the longest the skipper has
made *with you aboard*; and a fair (the dear price of the year). So the house's routine is done for you and the
house's best silver is always in your own hold. Build cost: one stored number per route (the three tallies) and one
per skipper (longest crossing sailed under you).

### Lesson 2. Manage routes, never ships; set once; the game checks them itself (OpenTTD, Transport Fever 2)

Shared orders, the line, and the order checker are the reason those games are playable at 500 vehicles. Anno 1800's
most quoted pain is the opposite: a route that goes wrong without saying so.

**Proposal: the route is the row.** The House tab lists *routes*, one row each, in the words of the order ("Wine to
Lundenwic, wool home, back by Lammas"), with the keels on it shown as small marks at the end of the row. A boat lost
or sold is replaced by dragging another mark onto the row; the orders are not written again. At the winter council,
and only there, the game reads each route against what it knows and writes one plain line under any that cannot
work: "No wine at Dorestad before the vintage ships come", "Skipper Folkmar has never crossed in October". That is
OpenTTD's order check in the game's own voice. And the cap on keels (the first report suggests eight) is set by a
drawing rule, not a balance number: **the route list never scrolls.**

### Lesson 3. The owner's boat must do what no hired boat can (Captain of Industry, Satisfactory)

Captain of Industry has exactly Kapman's shape, one ship of your own and cargo ships on contracts, and its players
call the owner's ship the part that "needs an overhaul" and the cargo ships "almost useless", because the first does
little that matters and the second cannot be seen. Satisfactory shows the other failure: the hand tool kept, with no
difference between what you do and what a machine could.

**Proposal: only your own eyes lift the fog.** Three powers belong to the boat you are aboard, and to no other in
the house: she alone lifts fog on the chart; she alone makes a price note marked "seen", where a skipper's note is
"heard from Folkmar" and ages as any other word does; and she alone can open a port to the house (first call, first
host, first toll-keeper known by name). A skipper may be sent *only where your chart is already clear*. So the size
of the house's world is exactly the wake of your own boat, across all the heirs, and every late-game year has a
reason to go somewhere new by hand. The cargo ships are made visible in the cheapest honest way: when your course
crosses one of your own boats, she is a card ("Your own sail. Folkmar hails you."), with news, a swap of cargo, or a
change of orders by word of mouth, the only way to change orders in mid-season.

### Lesson 4. Give growth a cost that binds, and a fresh start inside the same game (Transport Fever 2, Captain of Industry, Factorio Space Age)

Money that stops mattering "within the first hour" kills the puzzle. Upkeep that grows in step with the fleet kills
the pleasure. The best fix in the eight is neither: it is Factorio's new planets and Anno's new regions, where your
wealth does not follow you and you are small again.

**Proposal: silver is where it lies, and each sea is a new beginning.** The first report already shows the house's
silver by place (aboard, in the storehouse, lent, owed). Make that a hard rule: you can spend only what is in the
hull under you or in a storehouse in the port you stand in. Then the first voyage into each new sea (the Channel,
the Jutland coast, the Baltic, the Irish Sea, the north) is the first hour again: one boat, the silver you dared
carry, no price notes, no host, no known toll-keeper, four men. It is true to the period (coin and hack-silver
travel in the boat) and it needs no new system. For the cost that binds at home, keep it to two that are paid once a
year at the winter council and never in between: each keel's wages and mending, and each idle keel's loss. A boat
with no route is not free.

### Lesson 5. The game must not talk to the owner faster than a ship can (Anno 1800)

"system: your ship is under attack. player: i know". The most hated thing in the largest of these games is not the
size but the stream of alerts and the jumping between places to answer them. Kapman's letters rule already forbids
telepathy. It needs a budget as well, so that eight keels do not mean eight times the cards.

**Proposal: the letter bag.** Word of the house reaches you only in port, only when a ship has in fact come in
from that way, and it comes as one bundle: a single card, "The bag from Dorestad: three letters", read in any order
or left for the evening. At most one letter in a bag may ask for a decision; the rest are for knowing. Anything else
that needs your word waits for the winter council. Nothing about the house ever interrupts a leg at sea, except
meeting your own sail (lesson 3). On the day stick the due-notches stay as they are in the first report: a notch
turning from gold to red is the only alarm in the game, it makes no sound, and there can never be more of them than
there are keels.

---

## 12. What this changes in the first report

Nothing is overturned. Four things are added to `research_scale_to_fleets.md`:

1. Section 4.1 gains the skipper's ceiling and the "Yours to sail" list.
2. Section 5's House tab lists routes, not ships, and never scrolls.
3. Section 4.7 gains its teeth: fog, "seen" notes and first calls belong to your own boat alone.
4. Sections 4.2 and 4.5 gain the letter bag and the rule that silver is spent only where it lies.

The end game as a picture, after these: the same chart as the first hour, clear now from the Thames to the Baltic
along the lines the house has sailed and dark everywhere else. Up to eight thin threads in the house's colour, each
with a hollow mark and an age. One solid mark, yours, at the far edge of the clear part, with fog in front of her.

---

## Sources

Steam review links have the form `steamcommunity.com/profiles/<player>/recommended/<game>/`. Hours are the
reviewer's hours in the game as shown on the review page on 6 Oct 2026.

**Factorio**
- [F1] https://steamcommunity.com/profiles/76561198210974989/recommended/427520/
- [F2] https://steamcommunity.com/profiles/76561198105062687/recommended/427520/
- [F3] https://steamcommunity.com/profiles/76561198105478123/recommended/427520/
- [F4] https://steamcommunity.com/profiles/76561198152386484/recommended/427520/
- [F5] https://steamcommunity.com/profiles/76561197991391471/recommended/427520/
- [F6] https://steamcommunity.com/profiles/76561197975504815/recommended/427520/
- [F7] Friday Facts 224, "Bots versus belts", Twinsen, 5 Jan 2018: https://www.factorio.com/blog/post/fff-224
- [F8] Friday Facts 225, "Bots versus belts (part 2)", 12 Jan 2018: https://www.factorio.com/blog/post/fff-225
- Research costs and unlocks: https://wiki.factorio.com/Construction_robotics_(research) ; https://wiki.factorio.com/Logistic_system_(research) ; https://wiki.factorio.com/Construction_robot
- Overview: https://en.wikipedia.org/wiki/Factorio

**Satisfactory**
- [S1] https://satisfactory.wiki.gg/wiki/Milestones
- [S2] https://satisfactory.wiki.gg/wiki/Space_Elevator
- [S3] https://satisfactory.wiki.gg/wiki/Dimensional_Depot
- [S4] https://steamcommunity.com/profiles/76561198333732615/recommended/526870/
- [S5] https://steamcommunity.com/profiles/76561198015413820/recommended/526870/
- [S6] https://steamcommunity.com/profiles/76561198027845400/recommended/526870/
- [S7] https://steamcommunity.com/profiles/76561198382273889/recommended/526870/
- [S8] https://steamcommunity.com/profiles/76561198027291764/recommended/526870/
- [S9] https://steamcommunity.com/profiles/76561198084070386/recommended/526870/
- [S10] https://steamcommunity.com/profiles/76561198054218057/recommended/526870/
- [S11] https://steamcommunity.com/profiles/76561198047931463/recommended/526870/
- Overview: https://en.wikipedia.org/wiki/Satisfactory

**Anno 1404** (reviews are of the History Edition)
- [A1] https://en.wikipedia.org/wiki/Anno_1404
- [A2] https://steamcommunity.com/profiles/76561198130828669/recommended/1281630/
- [A3] https://steamcommunity.com/profiles/76561198132931055/recommended/1281630/
- [A4] https://steamcommunity.com/profiles/76561198197731030/recommended/1281630/
- [A5] https://steamcommunity.com/profiles/76561199558131761/recommended/1281630/
- [A6] https://steamcommunity.com/profiles/76561198376103146/recommended/1281630/

**Anno 1800**
- [B1] https://en.wikipedia.org/wiki/Anno_1800
- [B2] https://steamcommunity.com/profiles/76561197970778311/recommended/916440/
- [B3] https://steamcommunity.com/profiles/76561197974844621/recommended/916440/
- [B4] https://steamcommunity.com/profiles/76561198277972159/recommended/916440/
- [B5] https://steamcommunity.com/profiles/76561197993209652/recommended/916440/
- [B6] https://steamcommunity.com/profiles/76561198111545477/recommended/916440/
- [B7] https://steamcommunity.com/profiles/76561198955576696/recommended/916440/
- Not reached: the Anno wiki's trade route page (refused) and the Anno Union developer post on routes (not found). The route screen is described from memory.

**Offworld Trading Company**
- [O1] Soren Johnson, "Offworld Rules Circa June 2014" (an early rule sheet; details changed before release): https://www.designer-notes.com/offworld-rules-circa-june-2014/
- [O2] https://en.wikipedia.org/wiki/Offworld_Trading_Company
- [O3] https://steamcommunity.com/profiles/76561198974039891/recommended/271240/
- [O4] https://steamcommunity.com/profiles/76561198367471798/recommended/271240/
- Also read: https://www.designer-notes.com/offworld-trading-company-early-access-postmortem/ ; the talk "An RTS Without Guns" is video only: https://www.designer-notes.com/offworld-trading-company-an-rts-without-guns/

**Captain of Industry**
- [C1] https://steamcommunity.com/profiles/76561198025490621/recommended/1594320/
- [C2] https://wiki.coigame.com/Vehicles
- [C3] https://wiki.coigame.com/Cargo_Ship
- [C4] https://wiki.coigame.com/Contracts
- [C5] https://steamcommunity.com/profiles/76561197961653983/recommended/1594320/
- [C6] https://steamcommunity.com/profiles/76561197971724802/recommended/1594320/
- [C7] https://steamcommunity.com/profiles/76561197976834325/recommended/1594320/
- [C8] https://steamcommunity.com/profiles/76561198018988803/recommended/1594320/
- [C9] https://steamcommunity.com/profiles/76561199112787812/recommended/1594320/
- [C10] https://steamcommunity.com/profiles/76561197975018661/recommended/1594320/
- [C11] https://steamcommunity.com/profiles/76561198032715989/recommended/1594320/

**Transport Fever 2**
- [T1] https://en.wikipedia.org/wiki/Transport_Fever_2
- [T2] https://steamcommunity.com/profiles/76561198288512024/recommended/1066780/
- [T3] https://steamcommunity.com/profiles/76561198159896966/recommended/1066780/
- [T4] https://steamcommunity.com/profiles/76561198011831637/recommended/1066780/
- [T5] https://steamcommunity.com/profiles/76561198131092271/recommended/1066780/
- [T6] https://steamcommunity.com/profiles/76561198410867578/recommended/1066780/
- [T7] https://steamcommunity.com/profiles/76561198282319243/recommended/1066780/
- [T8] https://steamcommunity.com/profiles/76561198042891769/recommended/1066780/
- Not reached: the game's own wiki page on lines (permission denied).

**OpenTTD**
- [P1] https://en.wikipedia.org/wiki/OpenTTD
- [P2] https://wiki.openttd.org/en/Manual/Orders
- [P3] https://steamcommunity.com/profiles/76561198114054713/recommended/1536610/
- [P4] https://steamcommunity.com/profiles/76561198070289261/recommended/1536610/
- [P5] https://steamcommunity.com/profiles/76561198025976395/recommended/1536610/
- Not reached: the manual pages on autoreplace, groups and vehicle limits (addresses not found).
