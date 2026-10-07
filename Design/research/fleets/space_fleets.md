<!-- Research pass, 6 Oct 2026. One of the deeper passes behind Design/research_scale_to_fleets.md.
How it was done: the web search budget for this session was already spent (0 searches made), so everything
here comes from pages fetched by name: Steam forum threads (found through each forum's own search page), the
Quarter to Three forum, wiki.gg and Wikipedia. Refused: the Starsector forum and blog (HTTP 402), every Fandom
wiki (402), Reddit, archive.org, Rock Paper Shotgun. What I could not check is marked (?).
Player quotes are short and were relayed by the fetch tool, which sometimes clips a sentence: treat them as
what players say, not statistics, and open the link before reprinting one anywhere. Screen descriptions are
from my own knowledge of the games, not from a fetched picture. -->

# How space games grow from one ship to many, and what Kapman should take

**The ask (4 Oct):** "research a way for the game to scale from slow and methodical at the start to managing fleets
of traders, and how that end game will look from a gameplay look standpoint."

This goes deeper than `Design/research_scale_to_fleets.md` on five games: X3, X4: Foundations, Starsector, Elite
Dangerous (fleet carriers) and Avorion. They are the nearest relatives Kapman has: each starts you in one hull with
a little money and ends with many hulls. They disagree sharply about how, and their players are loud about which
way worked.

**The short answer.** The five games show three ways to grow, and only one of them keeps the first hour.

| Way | Game | What happens to the player |
|---|---|---|
| You become a cursor on a map | X3, X4 | You own hundreds of ships and stop being in one. Players call the result babysitting. |
| Your ships become timers | Avorion 2.0 | Babysitting is gone, and so is the world. Players call it an idle game. |
| You stay one mark on the map, and the mark grows | Starsector, Elite's carrier | You are always somewhere, doing something by hand. What you built is behind you and calls you home. |

Kapman's design (the three-crossings rule, letters, one boat always yours) is already the third way. This paper says
why that is right, where the third way still fails, and five concrete things to add.

One correction to the first paper: the X4 quotes it gives ("you mostly play from the map ... babysitting your
traders", "The AI is entirely incapable of making a decent decision") are not in the thread it links
(steamcommunity.com/app/392160/discussions/0/2270321250034956811, which is a thread about trade between two
stations). The complaint itself is real and common; section 2 gives threads where it can be read.

---

## 1. X3 (Reunion 2005, Terran Conflict 2008, Albion Prelude 2011)

### 1.1 From one ship to many: the rules and the gates

There is no rule that says "now you may have a fleet". There is no cap either: Wikipedia's summary is that the
player may own ships and stations with no limit. The gates are three, and all are money and standing.

1. **Credits.** You start in one small ship. You trade by hand until you can afford a freighter (a "TS").
2. **Software bought per ship.** A ship does nothing by itself until you fit it. Trade Command Software lets it
   buy and sell at a distance on your order; the "MK3" package (about half a million credits (?)) puts a pilot
   aboard who trades by himself. He starts as a **Sector Trader** who works one sector, gains levels by trading,
   and at a higher level becomes a **Universe Trader** who roams. Players: "Sector traders level up. At some point
   you can reissue the command and tell them to cover more than one sector." (Zoran).
3. **Race standing.** Each people sells its better ships and factories only to those it ranks highly, and rank
   comes from trading with them and killing their enemies.

After traders come **factories**: bought at a shipyard, carried by a hired transport, dropped in space, then
joined with others into a **complex** by a kit. Then freighters assigned to the complex to fetch and sell. Then
warships to guard it.

The standard advice is a recipe: "Buy a TS, spend the money improving engines, look for opportunities ... Earn
your first Million", then "buy another TS, outfit it, train the trader to Universal Trader ... Keep doing this
until you have 10-15", then "you will have a steady income from the UTs" (raZOleg). Others run "35-50 STs with a
2 jump range" (insolent1), and one player speaks of "empires" with "hundreds of trading ships".

### 1.2 Minute to minute

| Stage | What you do |
|---|---|
| **Start (hours 0 to 10)** | Fly one ship between stations. Read each station's stock and price by docking. Carry energy cells one way and ore back. Run from pirates. This is slow, and it is the part people remember. |
| **Middle (10 to 60)** | Fly your own ship on missions while four to fifteen traders level up. Train each new trader in a safe, busy sector first. Go and rescue or replace the ones that die. Buy the first factory and feed it by hand until its freighters are set. |
| **End (60 and on)** | Sit in a parked ship with the time accelerator on (SETA, up to ten times), opening the property list, fixing what has stopped, placing the next factory. Fly for fun in a capital ship when you choose to. |

### 1.3 The end screen, so it could be drawn

A cockpit view of space, and over most of it a grey, square-cornered menu: **the property list**. One line a ship
or station, perhaps two hundred lines, scrolled by keyboard: a small icon for the class, the name you gave it, the
sector it is in, and its present order in a column at the right ("Universe Trader: buying Ore", "Orders: none").
Beside it the **sector map**: a flat grid seen from above with small coloured arrows and squares, yours green. A
line of text scrolls across the top of the screen when a pilot reports ("I have reached level 12"). There is no
picture of the empire as a whole. The empire is a list.

### 1.4 What players praise and hate

- Praise, the goal itself: "What I loved was the challenge of building an economic empire large enough to provide
  the required resources." (tronic, on the Hub plot)
- Hate, the tools: "What I hated was the sheer inadequacy of the game's UI in managing such an empire." And:
  "if you wanted to send 50 ships to mine an asteroid field for ore, you had to perform maybe 10 clicks and enter
  several numbers for each ship!" "Despite having completely identical orders, it would take 20-30 minutes to send
  50 ships." (tronic)
- Hate, the pilots: "a brand new lvl 6 sector trader is more incompitent than a rock and require constant baby
  sitting." "I'd suggest to train them up to lvl 10 (8 minimum) before releasing them into the universe."
  (Fedora01)
- Losses shrugged off once you are big: "my empires have hundreds of trading ships, sure one will die here and
  there, thats just life in space."
- Some refuse the last rung: "I don't have the patience" (for big complexes; from the mid-late advice thread).
- Asked what to do late, veterans answer only with war: "Destroy the Paranid home sector" (Flozem); "put 700
  lazer towers in a kaak sector and watch the Qs just pop" (rubyismycat).

### 1.5 Does it keep the first hour?

No, and nothing in the rules tries to. Hand trading stops paying once ten traders run. A lost ship stops being a
story and becomes a line missing from a list. The first hour is slow because space is big; the late game answers
that with a time accelerator, which is the plainest sign that the player has nothing left to do with his hands.

---

## 2. X4: Foundations (2018; version 9 by 2026 (?))

### 2.1 From one ship to many: the rules and the gates

X4's promise is that you are a person, not a ship: you stand up from the pilot's chair, walk the deck, and step
onto any ship you own. The gates:

1. **Credits**, as before.
2. **A hired pilot for every ship**, with skill stars (none to five). Stars decide which standing behaviours a
   pilot may be given and how far he ranges. The exact thresholds changed between versions (?): early versions
   wanted three stars for the free-roaming trader, which made the first automated trader a long wait; later
   versions loosened it.
3. **Standing behaviours.** Each ship has a default: trade in a range, mine in a sector, or "work for" a station
   or a fleet commander. This is the real unit of growth: you do not order a ship, you set what it is.
4. **Stations built from modules.** Buy a plot, buy blueprints (gated by standing with the seller), draw the
   station in an editor, hire a builder ship, appoint a **manager**. Miners and traders assigned to the station
   take their orders from the manager, and his stars set their range.
5. **Rules set once for everything**: blacklists (never enter these sectors), trade rules (sell only to these
   peoples), alert levels. A player explaining it: "Go into player information. Global Orders and create a new
   blacklist." (Ruges)
6. **Your own shipyard**, at which point ships are a product, not a purchase.
7. Research at your headquarters gives **teleporting** to any ship you own, so distance no longer costs the
   player anything.

No cap on ships. The practical cap is the frame rate: "Late game when empires are large I was getting just 10-15
fps while viewing the map screen" (jpinard); "map fps is 15 to 21" (Kero).

### 2.2 Minute to minute

| Stage | What you do |
|---|---|
| **Start (0 to 15 hours)** | One fighter or courier. Fly the highways, dock by hand, walk the station to hire a man, scan, do missions for a people to earn standing, buy the first miner and tell it to mine in one sector. |
| **Middle (15 to 60)** | Half in the chair, half in the map. Place a station and nurse it to profit. Ten to forty ships with standing behaviours. Fights you fly yourself beside a few ships you command. |
| **End (60 and on)** | The map, almost entirely. Queue orders, read the log, fix stuck ships, add modules, order ships from your own yard by the dozen. "I am sitting in my ship with SETA on managing my empire far more often than fighting" (Darksurgeon). |

### 2.3 The end screen, so it could be drawn

The map fills the screen: a dark field of flat **hexagons**, one a sector, joined by thin lines, tilted a little
so they read as plates in space. Each hexagon is tinted by its owner. On the plates, hundreds of tiny icons:
triangles for ships, squares for stations, yours green, with thin dotted lines running out of each to where it is
going. Down the left edge, a column of tab icons and beside it a tall panel: **Property Owned**, a folding tree
(Stations, Fleets, Unassigned ships), each line a name, a small hull bar, and the current order in grey. Down the
right edge another tall panel: the mission or the info of what is selected. Bottom left, a ticker of income lines
going by ("+ 43,112 Cr"). A right-click anywhere drops a context menu of fifteen or so orders. The player's own
cockpit is behind this, unseen.

### 2.4 What players praise and hate

- "Later the game is the map": "Or you have to babysit everything and play on the map view the entire time."
  (mcb0ny)
- The AI: "No matter how many destroyers you have, you have to babysit them." (Holeypaladin). "im sick of
  babysitting my larger ships." (DrwHem). "I got 40 des attack fleet but ai is idiot." (Kero). "they can't seem
  to figure out how to make AI not dumb as rocks." (Deeks)
- Nothing left to want: "There is no late game content." "Game too empty, there is no content to play other than
  throwing some stations and watching the moneyflow." (DYRT). "i got 70 des nobody can counter me" and "10b in my
  acc" (Kero). "Is too easy to roll over factions in this game" (Cyrill Echo).
- The first verb is still the best one: "it's kinda boring but piloting is fun" (Kero). "I love the visual and
  combat/dogfighting aspect of the game far more than i do the economy" (Darksurgeon).
- Players mend it themselves with rules of their own: "try to set some boundaries for yourself next time"
  (Cyrill Echo); "you have to write meaning on to what your doing yourself" (AhriFoxFIreXD); "I start over at that
  point, change my start and build my empire differently" (Triu).
- Praise for the building: "its kind of fun to set it up this way because you can pretty much make it do what you
  want it to." (Knightwolf). "let the Manager do his or her Job." (Sticky). When delegation works: "Set miner to
  'local automine' and forget it." (Holeypaladin)

### 2.5 Does it keep the first hour?

It keeps the *means* (you can always fly) and loses the *reason*. Nothing in the late game needs you in a cockpit,
and everything needs you in the map, so the cockpit becomes a treat you allow yourself. The lesson for Kapman is
exact: "you can always still sail" is not enough. The game must need you to.

---

## 3. Starsector (in development since 2011; colonies came in 0.9, late 2018)

### 3.1 From one ship to many: the rules and the gates

Starsector never lets you become a cursor. On the campaign map you are **one fleet, one mark**, from the first
minute to the last. Growth is that mark getting heavier. The gates are the most carefully built of the five:

1. **A hard cap of 30 ships** in the fleet (?: the fleet page would not open; the figure is from memory).
2. **Running costs for every hull, every day**: supplies, fuel and crew. A bigger fleet eats more and, because a
   fleet moves at the pace of its slowest ship, travels slower. Size is a weight you carry.
3. **Officers**: at most 8, each on one ship, each with a level, skills and one of five tempers (timid, cautious,
   steady, aggressive, reckless). Pay is 500 credits plus 400 a level each month. A ship without an officer still
   fights, but plainly.
4. **Skills that weaken past a threshold.** Several fleet-wide skills give their full effect only up to a fleet
   worth 240 deployment points and fade above it. A small picked fleet is a real choice, not a stage to leave.
5. **Battle size**: only part of a big fleet can be in a fight at once, so 30 ships is depth, not width.
6. **Colonies**, the second layer. Founding one takes "1000 Crew, 200 Supplies, and 100 Heavy Machinery" and a
   surveyed, unclaimed planet. A colony starts at size 3 and grows to 6; its size sets how many industries it can
   hold (one at size 3, four at size 6). You may govern only a few yourself before each one more costs stability,
   and you may hire up to three administrators (the wiki's figures).
7. **Trouble that scales with success.** Putting a colony in a claimed system brings expeditions. Selling what a
   great power sells brings their fleets. A meter of "colony crisis" points fills as you grow and sets off named
   crises at 600.

A colony is run from one screen and then left; it sends money monthly. It is not a thing you drive.

### 3.2 Minute to minute

| Stage | What you do |
|---|---|
| **Start (0 to 5 hours)** | Two frigates. Glide across the map toward a mark; turn the transponder off; run from anything bigger; buy low at one port and sell at the next; take a small bounty; fight a three-minute battle flying your own ship. Count supplies every day. |
| **Middle (5 to 30)** | A dozen ships with officers. Long hauls out to the empty edge to survey and salvage, with fuel and supplies as the clock. Battles flown from your flagship, with a few orders to the others. The first colony. |
| **End (30 and on)** | The same fleet mark crossing the same map, now out of your own ports. Colony screens visited now and then. Crises that pull you home. The hardest fights in the game, still flown by hand. |

A player's own telling of the middle, and it is a story, not a spreadsheet: "So began what I'm going to refer to as
the Fuel Wars. The Sindrian Diktat and the Persean League ... took exception to some upstart newcomer horning in
on their markets." "Paying for my fleet and for food for the temporarily port-free colonies led to not really
being able to pay for either, which led to rapidly-mounting debts, which led to crew abandoning ship in droves
whenever I touched down at a port." (Fishbreath, 2018)

### 3.3 The end screen, so it could be drawn

The same screen as the first hour. A top-down star system: a sun, rings of orbit, planets as painted discs, and
your fleet as a small cluster of ship sprites, the flagship in the middle and the others in a loose ring, a faint
circle round them for how far they can be seen. Other fleets are other clusters with a name tag. Along the bottom,
a short bar of ability buttons (go dark, burn hard, scan). Bottom left, four numbers with small icons: supplies,
fuel, crew, credits. A message panel at the left edge scrolls short notices. That is all. The empire lives one key
away, in a **colony screen**: a planet's portrait at left, a row of square industry tiles in the middle each with
its small picture and its in-and-out goods as icons, and a column at right with stability, growth, hazard and
monthly income as plain numbers.

### 3.4 What players praise and hate

- Praise, the second layer changes the first: "It's fascinating how differently the game plays when you're running
  colonies." "I spend most of my time now outside the core systems, venturing there only when I need to hire
  officers or administrators" (Fishbreath).
- Praise, it reaches back into the early game: "It gives you a solid late game activity and even enhances the
  start of a playthrough -- you've always keeping an eye on the systems you visit to see if they're a good place
  for a future colony" (DoubleG, 2025).
- Critics, per Wikipedia: Rock Paper Shotgun in 2018, "more than worth the money already"; Cubed3 found balance a
  "massive annoyance".
- Hate (?): I could not open the game's own forum or Reddit, so this is from memory and is not quoted: colony
  income makes trade and bounties pointless within a few game years; raids on colonies feel like chores once they
  cannot hurt; and many players restart when the fleet is "done" because the early scramble is the best part.
  Open fractalsoftworks.com/forum to check before leaning on it.

### 3.5 Does it keep the first hour?

Yes, better than any of the others, for three reasons that are rules and not luck: the screen never changes; the
player's own hands are needed in every fight; and size costs speed and supplies, so the feeling of counting days
of food never goes away. What it loses is fear: once the colonies pay, a lost ship is only money.

---

## 4. Elite Dangerous: fleet carriers (2020)

This is not one unit to many. It is one unit to **one very big thing**, and it is the cleanest test there is of
upkeep as a design tool.

### 4.1 The rules and the gates

1. **One gate: 5,000,000,000 credits.** Players confirm the figure: "Saving up the 5 billion it costs is
   challenging for most people" (fullarmor2). One carrier a commander.
2. **You cannot fly it.** You pick a system on the galaxy map, it counts down (about a quarter of an hour (?)),
   and it jumps, up to 500 light years (?), burning a fuel (tritium) that you must buy or mine and carry aboard.
3. **Weekly upkeep**, more for each service fitted (a shipyard, outfitting, repair, a bar). Unpaid debt ends in
   the carrier being taken away.
4. **It earns only what you make it earn**: your own trade orders, a tariff on other players who dock.
5. It holds all your ships and stores and (players' figure) 25,000 units of cargo.

Upkeep's history is the lesson. In the April 2020 beta the figures were: "51,000,000/week minimally equiped -
2.66 billion/year. 147,000,000/week fully equiped - 7.65 billion/year. No, just no, I'm out." (Mako). They were
cut before release. Players now give "between 7.5 and 32 million per week" (Doom, 2025) and "under 10M per week
which is nothing" (Sighman).

### 4.2 Minute to minute

| Stage | What you do |
|---|---|
| **Start** | One small ship, flown by hand in first person: request docking, land on the pad, read the market, jump, scoop fuel from a star. Every landing is a small act of skill. |
| **Middle** | Bigger ships, the same verbs. Hundreds of hours of earning, by mining, trade loops or exploring. |
| **End, with a carrier** | Exactly the same verbs, from a home that moves. You plot the carrier's jump, wait, then launch from its deck in the small ship and do what you did in the first hour. Once a week you look at its bank. |

### 4.3 The end screen, so it could be drawn

Two pictures. **The management screen:** black with orange lines, the game's cockpit style. A wireframe of the
carrier (a long slab with a notched deck) at the left. Across the top, tabs: Overview, Crew and services, Finance,
Tritium depot, Navigation. In the middle, a bank balance in large figures, the weekly upkeep under it, and a line
saying for how many weeks the balance will last. Each service is a row with a crew portrait, a weekly cost, and a
switch: active, suspended. **The view that matters more:** you, in a small ship, lifting off a landing pad on the
carrier's deck, the carrier's name in big letters on the hull, a ringed planet behind.

### 4.4 What players praise and hate

- Praise: "It feels like home." "after I did it felt REALLY good." (fullarmor2). "A mobile base where i can ship
  my entire fleet around." (Agony_Aunt). "it changed the way the game is played for me." (Astro the Cat).
- It is where the money finally goes: "after a point, what else you gonna spend your money on?" (1Nf@MOu$(). "You
  get to the point where credits are so easy to acquire that there's zero reason not to get a fleet carrier."
  (Holeypaladin)
- Hate, upkeep as a tax: "After all the hype we now have to pay a Upkeep cost or lose the Carrier." "These ships
  should NOT be a credit sink. They should at the least operate on a break even basis out of the box" (One Eye
  Jack). Fear of it, even now that it is small: "My main concern is buying one but then not being able to keep it
  running because of cost." (Kaned Dragon). A request the game never met: "Is there an option to just 'park' a
  carrier to get rid of upkeep costs?"
- Hate, the grind to the gate: "If you don't have 5 billion....how willing are you to grind??" (Reggit)
- Doubt: "I'm still trying to figure out if the damn thing has any practical use and I'm still not seeing it."
  (Doom)

### 4.5 Does it keep the first hour?

Entirely. The carrier adds no new verb that replaces an old one. What it changes is where you sleep. The cost is
that it is not really a late game: it is a house.

---

## 5. Avorion (1.0 in 2020; 2.0 in 2021 (?))

Avorion is the most useful of the five, because it tried both ways and its players compared them in public.

### 5.1 The rules and the gates

1. **Materials in rings.** Seven building materials, each found nearer the galaxy's centre than the last. What you
   may build is set by how far in you have gone. This is the game's ladder, and it is a ladder of place.
2. **A ship needs a captain** to do anything without you. Captains are hired at stations.
3. **Before 2.0: chained orders.** You told a captained ship, on the map, "jump here, mine, jump there, sell,
   loop". The ship then really did it, in sectors the game really simulated.
4. **From 2.0: captains with classes** (miner, merchant, smuggler, explorer, commodore and others (?)), levels and
   perks, and **map commands**. You pick a command (mine, salvage, trade, scout, an expedition), draw an area on
   the galaxy map, and are shown three things before you agree: how long it takes, what it should bring back, and
   the chance of an ambush. The ship then **leaves the world**. It is not simulated. It comes back when the time
   is up, with goods. "Your captains only gain XP by running the operations that you can assign to them via the
   galaxy map, during which their activity is 100% autonomous."

### 5.2 Minute to minute

| Stage | What you do |
|---|---|
| **Start** | A mining drone, then a ship you build block by block. Mine rocks, sell ore, add blocks. |
| **Middle** | Fly your own ship toward the centre. Two to eight captained ships sent out on commands and collected when they return. Stations founded. |
| **End** | Your own very large ship for the bosses near the centre; the map for the rest, re-issuing commands as their timers end. |

### 5.3 The end screen, so it could be drawn

The galaxy map: a square grid of small dots on black, tens of thousands of them, each a sector, with a ring drawn
round the centre where the barrier is. Your ships are small green markers on the dots. Select one and a card
opens at the right: the captain's portrait, name, class and level, and a row of round command icons. Choose a
command and a **shaded patch** spreads over the grid for the area, with a panel that says, in three lines, the
time ("2 h 30 min"), the expected yield, and "Ambush probability: 12%". Ships that are away show as a portrait
with a clock running down.

### 5.4 What players praise and hate

- For 2.0: "I tell them to do something and they do it." (Mathmatics). "NOTHING breaks my immersion more than being
  forced or encouraged to micromanage if the goal is an empire." "The current Captain's Mission is designed to
  help players free up their time." The old chains were "buggy and performance intensive" (SKEL).
- Against 2.0, and this is the heart of it: "They have no impact to the game world now. Before when I traded stuff
  the stations I was trading with would get resources and start making stuff." It feels like "using console
  commands to spawn money and resources", "an idle clicker game now" (ouch67). "it's just out of play tickers
  running down to chance based loot." (LoSboccacc). "So now if I sending mining ship to other sectors they are
  not actually flying there?... WTF is going on?" (gribelo). "2.0 ruined the game for me as well" (No Step On
  Snek).
- And it did not even remove the tending: "2.0 is a massive slab of micromanagement dumped on the game", "a huge
  time sink" (rickcarson), because a timer that ends is a thing to click again.
- The older complaint, the pilots: "small ships under your employ have a nasty habit of just perpetually bumping
  into asteroids" (Slimurgical). And the end: "the end game is really one of its weaker points" (El Destructo).
  A late-game player with stations asks for the one screen the game lacks: "Is there a way to adjust price rates
  on your stations/productions without having to visit every single one of them manually?" (Lera; no one
  answered).

### 5.5 Does it keep the first hour?

Your own ship stays worth flying, because the centre's fights need it. The fleet, though, stopped being ships.
Players forgave stupid pilots more easily than they forgave ships that are not there.

---

## 6. Side by side

| | X3 | X4 | Starsector | Elite carrier | Avorion 2.0 |
|---|---|---|---|---|---|
| What the player is at the end | A list | A map | One fleet mark | One pilot with a home | One ship and some timers |
| Cap on size | None | None (the frame rate) | 30 ships, 8 officers | One carrier | Soft |
| What gates growth | Money, standing | Money, standing, pilot stars | Upkeep, speed, officers, thresholds, crises | One price | Place (how far in) |
| How you give an order | Per ship, many clicks | Standing behaviour, set once | You do not: they are with you | A jump on the map | A command with shown odds |
| Are the delegated ships real? | Yes | Yes | Yes (they sail with you) | n/a | No |
| Is the player's hand still needed? | No | No | Yes, every fight | Yes, everything | Yes, the hard fights |
| What players say at the end | "sheer inadequacy of the game's UI" | "babysit everything and play on the map view" | "a solid late game activity" | "It feels like home." | "an idle clicker game now" |

---

## 7. Five lessons for Kapman, each with one proposal

### Lesson 1. "You can still sail" is worthless unless the game needs you to

X4 and X3 let you fly for ever and give you no reason. "it's kinda boring but piloting is fun." Starsector and
Elite never offer the other seat. In Kapman the danger is sharper than in any of these, because **the chart is
already the game**: there is no cockpit to retreat to. If the house's ships can be ordered from the chart, the
late game is X4's map with a nicer picture.

**Proposal: no orders at a distance, ever.** The player's own ships cannot be clicked into doing anything. Clicking
one of your marks opens her last letter and her standing orders, read only. Orders are given in two places only:
face to face in a port where you and the skipper both lie, and at the winter council. A letter can carry a change
of orders, and arrives when the ship carrying it does. This turns Starsector's rule (you are one mark) into a
Kapman rule (you are one man, and you are somewhere). It also gives late voyages a purpose: sailing to Lundenwic
*to speak to Folkmar* is a reason to make the crossing yourself.

### Lesson 2. A ship you sent must still be in the world

Avorion's players said it in one line: "They have no impact to the game world now." Yield on a timer reads as
"console commands to spawn money", however honest the odds shown. X3 and X4 players, for all their anger at the
pilots, never say their fleet feels fake.

**Proposal: a house ship is one of the 138.** A skipper's boat is an ordinary ship of the AI fleet with an owner
set: the same trader brain, the same lanes, wind and wintering, fenced in by your orders. She really buys the
casks at Lundenwic and the price there really moves. You can meet her at sea as a sail the lookout makes out, and
find her moored beside you. Her tally on return is the list of what she actually did, port by port and price by
price, never a rolled sum. This costs almost nothing to build (section 6 of the first paper already plans it) and
it is the single thing that most separates a living fleet from a clicker.

Keep one thing from Avorion, because players liked it: **the odds are shown before you agree**. When orders are
set at the council, the page says what Kapman's cards already say, in words: the run to Lundenwic and back by
Lammas is *likely*; with Ubbi as skipper, after Michaelmas, *doubtful*.

### Lesson 3. A man you cannot trust is a ship you must watch

The oldest complaint, from 2008 to 2026: "more incompitent than a rock and require constant baby sitting";
"Babysitting your ships is never the answer." Players do not mind a skipper who earns little. They mind one who
dies stupidly while they were looking elsewhere, and they mind having to train him in a safe sector first. The
delegation that players praise is the kind that is set once: "Set miner to 'local automine' and forget it."

**Proposal: a floor under every skipper, and training that is play.**
- *The floor.* Whatever his skill, a skipper never does a short list of fatal things by plain stupidity: he does
  not sail into a gale he could see, does not sell below what the cargo cost, does not cross after the sea closes.
  Skill moves the margin (how well he buys, how many days he loses), not survival. Loss comes only from named
  causes the world really holds: a storm that came on fast, raiders, the bar, sickness, a king's men. Every loss
  arrives as a letter or a tale that names the cause, and the cause was a thing you could have ordered against.
- *Training.* A skipper is made from a crewman who has sailed the route **with you**. The three crossings that
  open a route for handing over are also what teaches the man. There is no training sector and no level bar: the
  man who was in your boat for those crossings is the man who knows the bar at Lundenwic. This joins the first
  paper's rule 4.1 to its rule 4.3 and removes X3's worst chore.

### Lesson 4. Size should be a weight, counted in people, and it should bring trouble home

Starsector is the only one of the five that caps the fleet (30 ships, 8 officers), makes size cost speed and
supplies, and weakens its bonuses past a threshold so that a small picked fleet stays a real choice. It is also
the only one whose players do not complain of managing. And it makes growth dangerous: the Fuel Wars came because
a player sold what great powers sold. X4, with no cap and no pushback, ends in "i got 70 des nobody can counter
me" and players inventing their own limits.

**Proposal: the house is limited by trusted men, and it fits on one page.**
- The cap is not a number of keels but of **skippers you have made** (lesson 3), with a hard ceiling of eight.
  Each takes a season of your own sailing to make, so the house cannot grow faster than you can sail.
- **The winter council is one page with at most eight lines**, one a ship: skipper, route, three order slots, one
  rule, and the odds as a word. If it cannot be read at a glance it is too big. No list ever scrolls.
- Past four ships, each more adds a named cost the day stick shows: more wages at Martinmas, more notice from the
  count's toll-keeper, more for a raider to come for. A house of three well-run boats must be a good way to play
  to 1066, not a stage to get through.
- **A house crisis, Starsector's way:** the chronicle's great turns (834 at Dorestad, 865, 991) and rivals'
  moves land harder the more you hold in one place, show themselves a season ahead on the day stick, and can only
  be met by you in person: a ransom carried, a hall visited, a storehouse emptied in time. This is the call home.

### Lesson 5. The big purchase should be a home, and its keep should never feel like a tax

Elite's carrier is loved for one sentence: "It feels like home." It was nearly hated to death for upkeep that was
too high, came weekly whether you played or not, and bought nothing you could see: "These ships should NOT be a
credit sink." Cut to a small sum, upkeep stopped hurting and still frightens newcomers ("not being able to keep
it running"). Meanwhile every one of these games runs out of things to want: "after a point, what else you gonna
spend your money on?"; "10b in my acc".

**Proposal: the house at Dorestad is the thing you buy, and Martinmas is the one day you pay.**
- The first great purchase is not a second ship but **a plot with a house and a jetty** in the home port (the
  `buy_plot` and `fund_work` acts already exist). It is where your ships winter, your stores lie, your men's
  families live, and the winter council is held. Its panel is the picture of how far you have come: more boats
  hauled up on the bank each year, drawn. This is Kapman's carrier.
- **All keep is paid once a year, at Martinmas**, as one sum made of named parts: each skipper's share, each
  crew's wages, the factor's household, the plot's rent to the count. The day stick shows it from spring. It is
  the 480 pennies of the first year, grown up: the same notch, the same dread, a bigger number.
- **A ship can be laid up.** Hauled out on the bank she costs nothing but a little tar, and loses nothing. This is
  the "park it" that Elite's players asked for and never got, and it lets a player shrink the house after a bad
  year without shame.
- **Silver has an end other than more ships.** The sinks in the first paper's table (a bell, a dowry, a share, a
  charter) each change a line in the chronicle's ranking of your life. The last question the game asks is not how
  much you had but what the house was when you died.

---

## 8. What the end game looks like, with these five added

The first paper's picture stands (section 5 there). These change it in four places:

1. **Your marks on the chart are not buttons.** Solid, hollow or faint; a click gives the last letter. The only
   mark that takes a course is your own.
2. **The winter council is the whole of management:** one page, eight lines at most, the odds as words, and the
   Martinmas sum at the foot.
3. **The home port's panel has your house on it**, with the boats that are laid up drawn on the bank.
4. **The day stick carries the house's fears as well as its dates:** ships due, Martinmas, and a crisis seen a
   season off.

A late evening's play, by the clock: the council (ten minutes, clock held); then your own boat out of Dorestad on
a voyage only you can make; three to five legs of the same five beats as the first hour; two letters met on the
way, one of them bad; a red notch; a choice to turn for home or go on. Nothing in it is a list.

---

## Sources

All fetched 6 Oct 2026 unless marked.

**X3**
- "Which ship is my choice?" (2018): https://steamcommunity.com/app/201310/discussions/0/1693788202020993799/
- "Max Sector traders?" (2017): https://steamcommunity.com/app/201310/discussions/0/135512931362696726/
- "How do I protect buying trade ships?" (2017; search snippet only): https://steamcommunity.com/app/201310/discussions/0/1327844097131158241/
- "Mid-Late game Advice" (2022): https://steamcommunity.com/app/2820/discussions/0/3710432845022886218/
- "What can you do late game?" (2019): https://steamcommunity.com/app/2820/discussions/0/1629664544091912710/
- tronic on X3's late game and its interface (2016): https://steamcommunity.com/app/2870/discussions/0/357285398695661197/
- https://en.wikipedia.org/wiki/X3:_Terran_Conflict

**X4: Foundations**
- "Anyone find that the game is too slow/boring?" (2025): https://steamcommunity.com/app/392160/discussions/0/594012019028454845/
- "late game target/help" (2023): https://steamcommunity.com/app/392160/discussions/0/3787002916510270679/
- "Is this a worthy successor from X3?" (2018): https://steamcommunity.com/app/392160/discussions/0/1744479698810523722/
- "Mining" (2026): https://steamcommunity.com/app/392160/discussions/0/592939340207756525/
- "how many destroyers to safely kill a Xenon station" (search snippet only): https://steamcommunity.com/app/392160/discussions/0/591814056255682240/
- "flee at damage threshold global order?" (search snippet only): https://steamcommunity.com/app/392160/discussions/0/583930224335590490/
- "Its time to make a bad Review" (2023): https://steamcommunity.com/app/392160/discussions/0/6450905019307217379/
- "The game keeps getting better but the score keeps getting worse..." (2025): https://steamcommunity.com/app/392160/discussions/0/603040419439207026/
- "Need a simple way to create a two way trade between MY stations" (2020): https://steamcommunity.com/app/392160/discussions/0/2270321250034956811
- jpinard on the map's frame rate (2022): https://forum.quartertothree.com/t/157612/61
- https://en.wikipedia.org/wiki/X4:_Foundations

**Starsector**
- Colony rules (version 0.98a): https://starsector.wiki.gg/wiki/Colony
- Officers: https://starsector.wiki.gg/wiki/Officer ; skills and the 240-point threshold: https://starsector.wiki.gg/wiki/Skills
- Fishbreath (2018) and DoubleG (2025), Quarter to Three: https://forum.quartertothree.com/t/78242/222 ; https://forum.quartertothree.com/t/78242/1510
- https://en.wikipedia.org/wiki/Starsector
- Not reachable (402): https://fractalsoftworks.com/forum/ ; https://fractalsoftworks.com/2020/07/08/skills-and-story-points/ (the developer on fleet size and skills; the address is from memory (?))

**Elite Dangerous**
- "Fleet Carrier upkeep is absurd - So long and thanks for all the fish..." (April 2020, the beta): https://steamcommunity.com/app/359320/discussions/0/2264689848036322380/
- "Fleet Carrier upkeep cost!!! Really" (search snippet only): https://steamcommunity.com/app/359320/discussions/0/2149847049315545793/
- "Fleet carriers upkeep costs when being offline for longer?" (search snippet only): https://steamcommunity.com/app/359320/discussions/0/4068391194362574801/
- "Are fleet carriers worth it?" (2021): https://steamcommunity.com/app/359320/discussions/0/3109145319599501040/
- "Was getting a space carrier worth it for you?" (2024): https://steamcommunity.com/app/359320/discussions/0/6015206719888046826/
- "Are fleet carriers worth the purchase for casual players" (2025): https://steamcommunity.com/app/359320/discussions/0/846244042087864441/
- https://en.wikipedia.org/wiki/Elite_Dangerous
- Not checked (?): the jump range, the jump countdown, the release date of the carriers outside the beta. The Fandom wiki page refused.

**Avorion**
- "I really don't like the new captain orders thing..." (2021): https://steamcommunity.com/app/445220/discussions/0/3038230013039380005/
- "What the problem with captains?" (2021): https://steamcommunity.com/app/445220/discussions/0/3038230013029772659/
- "Can I make this game 1.0 but with captains?" (2021): https://steamcommunity.com/app/445220/discussions/0/3196990234722466551/
- "Captain Level 0%" (2025; search snippet only): https://steamcommunity.com/app/445220/discussions/0/777534074828932277/
- "Sending captains on missions in singleplayer." (search snippet only): https://steamcommunity.com/app/445220/discussions/0/572666820169980572/
- "What kind of empire can you build late in this game?" (2017): https://steamcommunity.com/app/445220/discussions/0/1480982338951487731/
- "Late Game Trouble" (2022): https://steamcommunity.com/app/445220/discussions/0/3271309684514820364/
- "End/Late Game?" (2025): https://steamcommunity.com/app/445220/discussions/0/594026628585140689/
- Not checked (?): the list of captain classes, the 2.0 release date, the exact wording on the command panel. The wikis refused.
