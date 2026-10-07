<!-- Research pass, 6 Oct 2026. Web search was not available in this session (the budget was spent), so everything here
comes from pages fetched by name: the games' wikis, Soren Johnson's designer notes, Wikipedia, Steam forum threads and
Steam's public review feed. Player quotes were pulled out of those pages by an automated reader: the links are given so
each can be checked against the page before it is reused. Their spelling is left as written. Rules I give from memory
and could not check on a page are marked (?). -->

# From one unit to many: how ten games grow, and what Kapman should take

**The ask (4 Oct):** "research a way for the game to scale from slow and methodical at the start to managing fleets of
traders, and how that end game will look from a gameplay look standpoint."

The first answer is `Design/research_scale_to_fleets.md`. It looked at trading and truck games. This paper goes deeper on
ten games that grow a *household*: Crusader Kings II and III, The Guild 2 and 3, Old World, Suzerain, Stardew Valley,
Wildermyth, Dwarf Fortress and RimWorld. For each: the rules of growing, what the hands do at the start, the middle and
the end, the end screen as it could be drawn, what players say, and whether the first hour survives.

**The short answer.** These ten games use five different ways to grow, and each has one known way of going wrong.

| Way of growing | Games | What it gets right | How it goes wrong |
|---|---|---|---|
| **A limit on your own hand.** You may hold only so much yourself; the rest must go to people. | Crusader Kings II and III | The many are people with wants, not units. | Past a certain size nothing can hurt you, and the game is waiting. |
| **A switch that says "run it for me".** | The Guild 2 and 3, Stardew Valley | The chore goes away. | What is left is collecting, or nothing; and you cannot see why your people did what they did. |
| **A fixed budget of attention each turn.** | Old World | The hundredth turn asks for about as many choices as the tenth. | The turn still gets longer, because there is more to look at before choosing. |
| **A list of standing rules, set once.** | Dwarf Fortress, RimWorld | Seven or three people become fifty without fifty times the clicks. | You stop knowing anyone by name, the machine slows down, and the danger that grows with your wealth feels like a fine for playing well. |
| **Lives that end.** | Wildermyth, Crusader Kings, Suzerain | The reset is built in. Growth is the house's, and each life starts small again. | If the next life is only a repeat, the magic goes. |

Kapman already has the best single idea none of these games has: your own ships are known only by letter, so there is
nothing to babysit. What these ten add is five rules about limits, attention, reports, death and danger. They are at the
end, each with one concrete proposal.

---

## 1. Crusader Kings II (2012) and III (2020)

### 1.1 How one becomes many

You are one person: a count with one county, a duke, or a king. The game is a map of Europe on a running clock with a
pause key. You grow by marriage, claims, war and inheritance. The whole design rests on two limits.

- **The domain limit: what you may hold yourself.** In CK3 it rises with rank (county +2, duchy +2, kingdom +3, empire
  +4), with your stewardship schooling (+1 to +3) and a few laws and perks. Over the limit, "newly controlled holdings
  will stop providing taxes and levies after a year passes". In CK2 the sum is rank (1 for a count or duke, 3 for a
  king, 4 for an emperor) plus 0.15 for each point of stewardship, yours and half your spouse's; over it, each extra
  holding costs 20% of your own income and levies and 10 opinion with every vassal. So an emperor of half Europe holds
  about as many castles himself as a strong duke does.
- **The vassal limit: how many people you may deal with directly.** Over it, CK3 takes 5% of all vassal taxes and
  levies for each extra vassal, down to almost nothing. The cure is to make dukes and kings under you, so the tree
  grows a level and you still talk to a handful.

Everything past the limit must be given to a person. That person has traits, an opinion of you from minus 100 to plus
100, and a contract. In CK3 the contract has five levels each for tax (0, 2.5, 10, 15, 25%) and levies (0, 10, 25, 35,
50%); a kinder contract buys opinion (+15 at "exempt"), a harder one costs it (-20 at "massive"), and raising one
without cause costs tyranny. Your strongest vassals (three for a count, four for a duke, five for a king) expect a seat
on your council and hold a 40-point grudge without one. Unhappy vassals join factions that hand you an ultimatum.

**The gates** are rank (enough counties make a duchy, enough duchies a kingdom), a claim before every war, and prestige,
piety and gold as the three purses. **Succession** is the fourth gate and the most important: under the early laws
(gavelkind in CK2, partition in CK3) your lands are divided among your children when you die. You then play the heir,
with a third of what you had and brothers who hold the rest.

**The dynasty layer** sits above the lives. In CK3 each living member with a title earns renown (a barony 0.12, a county
0.25, a duchy 0.5, a kingdom 1, an empire 2), and renown buys legacies that last for ever: "The first dynasty legacy
costs 250 Renown to unlock and each one afterwards costs 500 more than the previous one."

**The screens.** One map. A portrait of you in a corner. A strip of tabs: realm, military, council, court, intrigue,
factions, decisions. A window for any person: portrait, traits, skills, family, opinion of you. Events arrive as a
picture, a paragraph and two or three choices. There is no second kind of screen at any size.

### 1.2 What the hands do

- **Start (a count).** Find a wife with good stewardship. Pick a way of life. Send the bishop to forge a claim on the
  next county and wait a year or two with the clock running. Answer a card about a hunt or a quarrel. Raise two hundred
  men, sit through a siege. Ten minutes may pass with one decision in them, and each one matters because you have six
  hundred men and so does your neighbour.
- **Middle (a duke or a young king).** This is the game at its best. You are juggling people: which vassal gets the
  council seat, which daughter marries which ally, how to get the lands to one son and not three, which faction is one
  angry count short of an ultimatum. A death at the wrong moment can undo twenty years.
- **End (an emperor).** After each war a list of won titles must be handed out one by one, because you are over the
  limit. Pop-ups about people you do not know. Armies win by weight. Mostly you watch the date run and wait for the
  heir. The dangers left are inside the house: a bad succession, a child with the wrong traits.

### 1.3 The end screen, to draw it

A map of Europe drawn like a painted parchment relief, zoomed far out. One colour covers it from Ireland to the Volga,
with the realm's name in wide curved capitals across the whole of it; smaller realms are slivers at the edge. Bottom
left, a large round portrait of an old crowned man, and beside it three small round portraits: wife, heir, a friend.
Top right, a narrow bar of icons and numbers: gold 14k with "+212" under it, prestige, piety, renown, "Domain 9/8" in
red, soldiers "68k". Down the right edge a column of seven tab icons. Top centre, a row of small round warning pips with
a number on a banner: "5 issues" (a title to give away, an unmarried cousin, an empty council seat). In the middle left,
an event window: a painting of a feast, four lines of text, three wide buttons with trait icons. Bottom right, the date
and five speed pips. Nothing is moving on the map except one small army banner.

### 1.4 What players say

Praise is for the middle: the house, the heirs, the reset.

- "King dies / Everything I worked for gets split up into several counties/duchies" (CK2, a *recommending* review: the loss is the fun) https://steamcommunity.com/profiles/76561198071255385/recommended/203770/
- "A vassal who controls more territory than he should becomes a nagging thorn...eventually tries to rebel" (CK2, recommending) https://steamcommunity.com/profiles/76561197986346593/recommended/203770/
- "weeks into months, and one day you'll wake up from reuniting the kingdoms of Charlemagne, or uniting Scandinavia" (CK2) https://steamcommunity.com/profiles/76561198024962299/recommended/203770/

The complaints are all about the end.

- "As well you will inevitably 'win' most games and become so large you no longer face real threats." (CK3) https://steamcommunity.com/profiles/76561198007336547/recommended/1158310/
- "Unless you are mega invested in the RPG way of playing it like the Sims, you'll be producing 500+ Gold per month and everything is done." (CK3, a recommending review) https://steamcommunity.com/profiles/76561198376717640/recommended/1158310/
- "you're mostly just watching numbers fight each other while occasionally clicking buttons to pretend you're in control." (CK3) https://steamcommunity.com/profiles/76561199126901147/recommended/1158310/
- "The majority of events involve completely random characters that are generated on-the-fly for the event, and are then deleted" (CK3) https://steamcommunity.com/profiles/76561197989629512/recommended/1158310/
- "The technical state is still unacceptable — late-game lag and crashes remain a persistent problem." (CK3) https://steamcommunity.com/profiles/76561198002763579/recommended/1158310/
- GameSpot's review, as quoted on Wikipedia: "when the story engine isn't firing, your actions can feel rote and uninspired". https://en.wikipedia.org/wiki/Crusader_Kings_III

### 1.5 Is the first hour still there?

Half of it. The *verbs* never change: the emperor still marries off a daughter exactly as the count did, on the same
screen. That is the best thing in the design. What goes is the *stakes*. The count's marriage could save him; the
emperor's cannot matter. And the people stop being known: a count knows his three neighbours, an emperor gets cards
about strangers. Players who love the late game often give themselves a new small start: they hand the empire away and
play a small cousin (?).

**For Kapman:** the limit on the owner's own hand (lesson 1), the contract as two plain dials, division at death as the
reset (lesson 4), and a warning: never let the house grow past the point where a storm or a raid can hurt it.

---

## 2. The Guild 2 (2006) and The Guild 3 (2017 to 2022)

The closest of the ten to Kapman's own subject: a townsman of the late Middle Ages who starts with one workshop and
ends as a family with offices.

### 2.1 How one becomes many

- **One person, one trade.** You make a character with a class (in The Guild 2: patron, craftsman, scholar, rogue) and
  the class decides which workshops you may own. You begin with one small business and a house.
- **Workers.** Each workshop takes a few hired hands. You set what they make; a cart takes it to the market. At the
  start you do the buying and carting yourself.
- **The switch.** Every business has a switch that hands its running to the game: buying materials, making, selling.
  In The Guild 3 this became a set of rules for each business. This is the whole of the move from one to many: you
  click the switch and buy the next workshop.
- **Family.** You court and marry through a row of courtship actions; children are born, schooled and grow up. In The
  Guild 2 "the game progresses from a single character to controlling a dynasty of up to three family members"
  (Wikipedia). If the last of the family dies without an heir, the game is over.
- **Titles and offices.** Titles are bought, from commoner upward, and each opens bigger buildings and more of them
  (the exact counts (?)). Offices in the town are won by votes, and votes are won by favours, bribes and blackmail. An
  office gives a wage and a power: a judge's verdict, a say in the town's laws.
- **Rivals.** Other families do the same, and may sue, rob, slander or kill your people.

**The screens.** A 3D town you walk your people round in real time. Round portraits of your family members in a
corner. A bar of action buttons with timers on them. A column of small building icons at the screen's edge, one for
each thing you own. A message log.

### 2.2 What the hands do

- **Start.** You are at the bench. Buy flax, set two men to spin, walk the cart to market, watch the price, walk home.
  Pay court to a neighbour between loads. It is slow and you see everything.
- **Middle.** Two or three businesses on the switch. Your three people each have their own row of timers: one is
  campaigning for an office, one is at school, one is running the tavern. An election is the high point: you count
  heads and spend favours.
- **End.** A dozen icons down the edge of the screen, money coming in without you. You wait on timers, answer attacks
  you did not see coming, and sit in the council chamber. The 3D town you used to walk through is now a thing you fly
  over.

### 2.3 The end screen, to draw it

A low bird's view of a walled late-medieval town in soft 3D: red roofs, a market square with carts. Top left, three
round family portraits with small health and purse bars. Along the bottom, a wide bar of twelve square action buttons,
half of them greyed with a clock sweep. Down the right edge, a column of fourteen small building icons (a forge, a
farm, an inn, a warehouse), three of them flashing red. Top right, a purse with a large number and the family's title.
A message log on the left with a stack of unread lines: "Your employee was attacked." "Your reputation has fallen."
(The layout is from memory of both games (?); the parts are right, their corners may not be.)

### 2.4 What players say

- The best case, The Guild 3: "Once your automation rules are set, watching your medieval corporate empire run itself is incredibly satisfying." The same review: "the political system eventually feels like a spreadsheet." https://steamcommunity.com/profiles/76561198163771977/recommended/311260/
- "Within 1 generation, you'll likely have several successful businesses printing money." "Within 2 generations, you'll likely be sovereign with a mansion for a residence." https://steamcommunity.com/profiles/76561197986885297/recommended/311260/
- "For every building, and every skill its all the same. Just let it automate and sit back." https://steamcommunity.com/profiles/76561197992005733/recommended/311260/
- "You spend more time waiting for timers than making meaningful decisions." https://steamcommunity.com/profiles/76561197965725688/recommended/311260/
- On the helpers' sense: "relying on the brick headed AI seeing 59/60 wheat in this one farm and instead of waiting literally 1 second runs to the opposite side of the map" https://steamcommunity.com/profiles/76561199128471241/recommended/311260/
- The sharpest one, a forum thread called "Fun until": "Fun until you have tons of people and have to micromanager with terrible UI and hotkeys", and later "late game sucks for me because i dont know whats going on with all my guys getting attacked all the people hate me ive done nothing wrong the logs are jumbled garbage that dont really tell you whats going on". https://steamcommunity.com/app/311260/discussions/0/3766732914514712701/
- "I find it enjoyable but its just I feel like there should be so much more." https://steamcommunity.com/app/311260/discussions/0/1696040635922378499/
- The Guild 2, praise: "once you master businesses and making money there is an entire world of intrigue, rivalry, thuggery, and political domination to explore." https://steamcommunity.com/profiles/76561198076892404/recommended/39680/
- The Guild 2, GameSpot as quoted on Wikipedia: "Micromanagement and repetition really bog down this original but unfulfilling game". https://en.wikipedia.org/wiki/The_Guild_2

### 2.5 Is the first hour still there?

No, and this is the lesson. The first hour is a craftsman's day: bench, cart, market. The switch removes all of it
and puts nothing in its place but a number. The game's answer is to change subject, from trade to politics. Players
who like politics stay; players who came for the workshop say the game has no late game. And when something goes
wrong among the many, the game cannot tell you what or why.

**For Kapman:** two rules. What you hand over must report back in words you can follow (lesson 3). And handing over must
not delete the craft: the owner still sails.

---

## 3. Old World (2021)

A turn-based game of ancient kingdoms by Soren Johnson, who led the design of Civilization IV. It is here for one
idea, which is the cleanest fix anyone has made for "more things means more clicks".

### 3.1 How one becomes many

You start with one city, a settler, a few units, and one ruler who will die. Cities can be founded only on marked
city sites, so there is a known number of them. Each turn is one year. The ruler ages; an heir must be raised and
schooled; three families hold your cities and each has an opinion of you.

**The rule: orders.** Every action by anything costs one order from a single pool: moving a unit a step, having a
worker build, sending an envoy. The pool is refilled each turn. It grows with your legitimacy and with certain
buildings, officers and laws (the exact sums (?)); a new realm has about a dozen, a great one several times that (?).
A unit may move more than once, until it tires. So with thirty units and twenty orders you cannot move them all. You
choose.

The designer's reasons, in his own words (https://www.designer-notes.com/old-world-designer-notes-1-orders/):

- "The problem with every unit moves in 4X games is that it only creates the illusion of tactical and strategic decision making."
- "Each turn, the player is evaluating the most effective single move for each unit, which is often very straightforward and often even boring."
- "These decisions quickly become rote as the number of units in the game grows."
- With orders: "Not only were we making actual guns-or-butter decisions every turn, but the strategic space was blown open so wide."

**The other gate: the end.** The game stops at two hundred turns (?), and can be won long before by fulfilling ten
ambitions (?): goals offered three at a time and chosen as you go. From the last designer note
(https://www.designer-notes.com/old-world-designer-notes-11-the-end/): "The hardest part of a 4X to design is the
ending. Truthfully, no one has ever done this well". Ambitions mean "the player would need to achieve many tasks to
win, but these tasks could be chosen on the fly". And a player who is far ahead is let go early: "Double victory helps
Old World avoid the worst of a common problem once victory is certain but the player still needs to push End Turn
fifty more times".

**The screens.** One hex map. The orders left this turn are a number at the top with the other stocks. The ruler, the
heir, the spouse and the council are portraits; events are a picture, text and choices, as in Crusader Kings.

### 3.2 What the hands do

- **Start.** Eight to twelve orders (?). Walk a scout, set one worker to a quarry, move the settler. Every order is felt.
- **Middle.** Thirty-odd orders, six cities, a war on one border. You spend most of the pool on the war and the
  workers at home stand idle, and that is a real choice, not a failing.
- **End.** A big pool, but far more things that want it. You still give only so many commands. What has grown is the
  looking: twelve cities' build lists, a long row of characters.

### 3.3 The end screen, to draw it

A hex map of dry hills and a blue inland sea, painted in warm ochre. Twelve walled cities with name banners in one
colour. Along a mountain border, two lines of units facing each other, each with a small row of pips under it for its
tiredness. Across the top, a bar of stocks: food, wood, stone, iron, coin, and among them a figure with its own icon:
"Orders 23 / 61". Top left, the ruler's portrait, grey-haired, with age "58" and smaller portraits of heir and spouse.
Down the left, a short list of things asking for a decision. Bottom right, a large round "End turn" seal. Most of the
units on the map have not moved and will not. (Corners from memory (?).)

### 3.4 What players say

- "Old World has a brilliant solution to this with the ORDERS system. You only get a limited number of actions per turn." https://steamcommunity.com/profiles/76561198115203200/recommended/597180/
- "The problem is: I can only move 10 of them this turn... Which one gets my attention? That's an actual strategic decision." (the same reviewer)
- "Because of this limitation the Old World does not have the same carpet of doom issue that Civ 5 has, and the endgame unit management is not a great chore every turn." https://steamcommunity.com/profiles/76561198041492419/recommended/597180/
- But: "There are still the long endgame turns prevalent in the genre as well as declining performance." https://steamcommunity.com/profiles/76561197967191844/recommended/597180/
- On winning by ambitions: "I find it to be both an unsatisfying and anticlimactic way to finish a game." https://steamcommunity.com/profiles/76561198307188683/recommended/597180/
- A player who did not stay: "I find that instead of getting immersed in the experience, I just want it to end... the experience begins to grate on me." https://steamcommunity.com/profiles/76561198171636533/recommended/597180/

### 3.5 Is the first hour still there?

More than in any other game of its kind. The feeling of the first turns, too few orders for what you want, is the
feeling of the last. The cost is that the designer feared "we were adding complexity that would be mandatory to
understand to play the game", and that the turn still grows longer because there is more to read.

**For Kapman:** a fixed number of house matters each winter, however big the house (lesson 2). And goals chosen as you
go, three offered at a time, are a better end than a score.

---

## 4. Suzerain (2020)

Suzerain is the odd one here: it does not go from one to many at all. It is in the list because it makes a whole
country feel large with one man, a map and text, which is very near Kapman's own means.

### 4.1 How the scale grows

You are Anton Rayne, new president of Sordland, for one term. The screen is a map of the country. On it lie tokens:
a scene to play, a bill to sign or veto, a report, a newspaper. You click one; a conversation opens beside the map
with your ministers, and you choose from a few replies. Wikipedia counts 450,000 words and says a term can be played
in under twelve hours. There is one budget, decided early, and a personal purse.

Nothing is added to your hands from first turn to last. What grows is the *weight carried forward*: a promise made to
a general early on decides whether there is a coup near the end. The later Rizia expansion adds a layer of resources
and a playable war on the map.

### 4.2 What the hands do

- **Start.** Read. Meet the cabinet one by one. Learn who wants what.
- **Middle.** Read, and choose with a growing list in your head of what you have promised to whom. Sign and veto.
- **End.** The same, faster, as everything you set going arrives at once: the reform vote, the war or the peace, the
  election. Then a results screen sets out what you did.

### 4.3 The end screen, to draw it

A flat political map of an invented country in muted greens and greys, its regions outlined, cities as dots with
names. Eight or nine round tokens lie on it: a speech bubble, a pen, a folded newspaper, a red exclamation on the
border. On the right, a tall panel: a painted portrait of a minister at the top, his words under it in a serif face,
and three numbered replies. Along the top, a thin bar: the turn, the government's budget, your own wealth. No units,
no numbers moving.

### 4.4 What players say

- "through every meeting, conversation, and signature the world conforms around what YOU did personally, and you are held accountable for all of it." https://steamcommunity.com/profiles/76561199084619478/recommended/1207650/
- "Once it clicks, the Codex, reports and news updates become strangely addictive to sift through, and you start seeing how your earlier choices ripple forwards." https://steamcommunity.com/profiles/76561198067270512/recommended/1207650/
- "coming from paradoxesque strategy games, Suzerain gave me a refreshing, dialogue-based way to manage a nation." https://steamcommunity.com/profiles/76561199127490096/recommended/1207650/
- On the shape of it: "I'd say the large middle section of the game is more interesting that the slow beginning, or the chaotic home stretch." https://steamcommunity.com/profiles/76561198870742416/recommended/1207650/
- On being led: "You can't take action on any of them until one of your advisors, by the story's script, decides to give you a liberal lecture about that specific issue." https://steamcommunity.com/profiles/76561198293161796/recommended/1207650/
- On hidden recipes: "In order to get the good capitalist (or communist or dictatorial or democratic and so on) ending you need to take decisions X, Y, and Z on turns 3, 4, and 6." https://steamcommunity.com/profiles/76561199087525672/recommended/1207650/
- "At the beginning of your 4 YEAR TERM, you are given exactly one chance to decide where your budget will go." https://steamcommunity.com/profiles/76561198884635774/recommended/1207650/

### 4.5 Is the first hour still there?

Entirely: the last hour is the first hour with more remembered. That is its strength and the limit of it. The
complaints are that you can only act when the script lets you, and that the end comes in a rush.

**For Kapman:** a big house can be felt through remembered promises and letters on a map, with no new screens. And the
warning: the player must be able to *start* things (send a ship, write first), not only answer.

---

## 5. Stardew Valley (2016)

One farmer, and never anyone else. Stardew grows from one pair of hands to a farm of hundreds of machines without a
single employee. It is the clearest case of what tools do to a slow first hour.

### 5.1 How one becomes many

Day one: fifteen parsnip seeds, a hoe, a watering can, a bar of energy that a morning's watering empties. A day is
about fourteen real minutes (?) and ends whether you are done or not. Everything that follows is a way to buy your
mornings back.

- **Sprinklers**, the great gate. Three kinds, each a crafting recipe opened by farming skill: at level 2 one that
  "waters the 4 adjacent tiles every morning", at level 6 eight tiles, at level 9 twenty-four. The best needs iridium,
  which lies at the bottom of the second mine. So the watering chore ends only for the player who has gone and done
  the rest of the game.
- **Animals** look after themselves in steps: a silo and the larger barns feed them; a late machine collects from them.
- **Junimo huts**, the only helpers: 20,000 gold and nine starfruit each, bought from the wizard after his quest.
  "Junimos will harvest crops around the hut for you", in a square of seventeen tiles a side, not in winter or rain.
  The player gets no farming experience for what they pick.
- **Kegs and jars.** The real money is in turning crops into wine and pickles. A machine takes one item and is
  emptied and filled by hand. A rich farm has hundreds.
- **The gates** are skill levels (recipes), the mine's depth (ores), the community centre's bundles (the greenhouse, the
  bus to the desert), and the calendar: the grandfather's judgement comes at the start of the third year.

**The screens.** The farm itself is the only management screen. There is no list of machines, no overview: you know
the kegs are ready because you walk past and see the bubble over them.

### 5.2 What the hands do

- **Start.** Clear a few tiles, plant, water each tile by hand, run to town before the shop shuts, fall into bed. You
  have more to do than day or energy, and choosing what to leave is the game.
- **Middle (the second spring).** Sprinklers do the watering. The morning is a round: pet the animals, pick the ripe
  rows, load the kegs. The afternoon is yours: the mine, a birthday, the bundles. This is the stretch people remember.
- **End (year three on).** The round has become the whole day, or has shrunk to nothing. Either you walk a shed of two
  hundred kegs pressing one button, or the farm runs and you look for something to want: the island, the last
  collection, redecorating.

### 5.3 The end screen, to draw it

A top-down pixel farm in high summer, sixteen-pixel tiles. The screen is filled edge to edge with a grid: square beds
of blue-green starfruit, each bed centred on a small purple sprinkler, with a scarecrow at every fourth crossing. Three
round leaf-roofed huts stand among them, and tiny bright stick-figures carry fruit to their doors. On the left, a big
shed with its roof cut away showing rows of barrels, forty wide, each with a small bubble over it showing a purple
bottle. One small farmer stands in the aisle. Top right, a round dial with the day, the hour and the weather, and under
it the purse: eight digits. Bottom centre, a toolbar of twelve slots. Bottom right, a thin green energy bar, full.

### 5.4 What players say

- The classic complaint, a thread called "Late Game Is Boring And Repetitive": "I wake up, check my tree farm, check my kegs, check my pickle jars, check my green house, THAN go back to bed". A reply: "While capable of infinite play, it wasnt designed to be different past grandpas assessment." https://steamcommunity.com/app/413150/discussions/0/357284131788423175/
- "late game for many players is mostly to harvest crops and then fill hundreds of kegs and jars" https://steamcommunity.com/app/413150/discussions/0/1742264052613006539/
- "i get bored with late game farms anyways where all i have left to do is just make wine" https://steamcommunity.com/app/413150/discussions/0/844005624123555694/
- "It starts nicely but soon becomes a game of nonsensical accumulation, just another extension of the life you were running away from." https://steamcommunity.com/profiles/76561199000213367/recommended/413150/
- The start has its own haters: "core gameplay loop for farming of water-sleep-water-sleep to get to be able to experience any of that is just exhausting and gets old quick." https://steamcommunity.com/profiles/76561198374182789/recommended/413150/
- And a player who names all three ages: "Early game is intensive, slow, and confusing. Mid game you start to get a hang of everything...End game is kinda odd and confusing." https://steamcommunity.com/profiles/76561198128163562/recommended/413150/
- Praise for the end as it is now: "Especially after the massive 1.6 update, the endgame and progression feel unmatched...you can customize your farm to your exact liking, maximize your millions, or simply focus on building your dream life." https://steamcommunity.com/profiles/76561198256897471/recommended/413150/
- "don't get frustrated with your first sprinkler or with fishing. It really does get better later on" https://steamcommunity.com/profiles/76561199219915809/recommended/413150/

### 5.5 Is the first hour still there?

The place is; the pressure is not. The first hour's feeling is "too much to do before dark". Sprinklers end it, which
is the reward, and then the kegs bring back a chore with no choice in it. The players who stay happiest are the ones
who find a new small thing to care about: a person, a room, the island. The town and its people, which never scale,
carry the late game.

**For Kapman:** when a job is handed over it must be gone, not turned into a round of collecting (lesson 3). And the
things that do not scale (a man's wedding at Ribe, a riddle, a fitt of Beowulf) are what the late game will live on.

---

## 6. Wildermyth (2021)

A game of a small band of heroes across a lifetime, told like a picture book. It is here because it is the best of
the ten at keeping every person a person, and at making age and death a gift.

### 6.1 How one becomes many

- **Three farmers.** A campaign starts with three young people: a warrior, a hunter, a mystic.
- **Recruiting.** Towns on the map hold recruits. Hiring costs legacy points, a small purse earned by clearing the
  land. A band ends a long campaign with perhaps eight to twelve heroes (?).
- **Chapters and years of peace.** A campaign is three or five chapters. Between chapters years pass in a line of
  text: the heroes age, marry, have children who may join later, and at last retire.
- **The overland map.** The land is a board of tiles. You split the band and set each group a job, measured in
  hero-days. From the wiki: "if a job would take 60 hero-days, then that's about how long it takes one hero to do it.
  Two heroes take about 30 days, three heroes take about 20 days". Scouting a tile is 40, building a bridge 120,
  cutting a pass 240. Classes matter: "Hunters are better at Scouting, Mystics are better at Securing sites, Warriors
  are better at preparing Defenses." The clock runs only while jobs run; meanwhile the enemy's deck turns up
  calamities that make every later fight harder.
- **The legacy.** When a campaign ends, its heroes go into a lasting roster. They rise through five named ranks (Folk
  Hero, Local Legend, Fabled Adventurer, Balladsung Hero, Mythwalker), one or two ranks a campaign. In a later
  campaign a town may offer an old hero, young again, keeping one remembered ability for each rank, and costing more
  legacy points the greater they are.

**The screens.** The board of tiles, with paper-doll heroes standing on it. A fight on a small grid. A comic-book
panel for each event, with your own heroes drawn into it.

### 6.2 What the hands do

- **Start.** Three people, one fight, one comic. You read every word because there are only three of them.
- **Middle.** Six or seven heroes in two groups. On the map you weigh days: split up to scout faster, or stay together
  for the fight that is coming. Somebody has lost an arm and grown a crow's wing in its place.
- **End.** The first three are grey. Their children are in the second group. Three groups work the map at once and
  the last incursion is days away. Then the ending: each hero's life is read back, and you choose who is remembered.

### 6.3 The end screen, to draw it

A board of irregular paper tiles in storybook colours: pine green forest, tan hills, a blue river with two plank
bridges, laid on a dark table. Flat cut-paper figures with white outlines stand on it in three groups: three grey-haired
ones by a walled town, four at a mountain pass with a small progress ring over them, two on a tower. One has a stag's
antlers; one has an arm of stone. A red arrow of small monster cut-outs points from the map's edge at the town, with
"11 days" on a ribbon. Top left, a ribbon: "Chapter 5". Along the top, a row of small face cards for the band, twelve
of them, two edged in black. Top right, a short stack of calamity cards and a coin with a number: the legacy points.

### 6.4 What players say

- "The winding paths your heroes take, the struggles and the aging and the injuries." https://steamcommunity.com/profiles/76561198122506475/recommended/763890/
- "your adventurers can die or retire - their appearance changes as they age and from some choices you make." and "The surviving heroes entered into a legacy mode where they're basically reborn without all their abilities." https://steamcommunity.com/profiles/76561198030764539/recommended/763890/
- "It's only when playing through the third and later campaigns that you get a chance to see how complex and interesting the tactical part of the game is." But the same player: "the magic goes away once you finish the story campaigns and play generic, randomized ones." https://steamcommunity.com/profiles/76561198043840291/recommended/763890/
- "After multiple campaigns, you hit a point where you recognize the event from the opening." https://steamcommunity.com/profiles/76561198080597444/recommended/763890/
- Critics, as quoted on Wikipedia: "One of the best video game narratives of the year" (Washington Post). https://en.wikipedia.org/wiki/Wildermyth
- I found no player complaint about managing the larger band or the map in the reviews I could read; the complaints are about repeated stories. I could not reach the forum to look further.

### 6.5 Is the first hour still there?

Yes, and it gets better, which is rare. Three reasons. The band never passes about a dozen, so every face is known.
The people change where you can see it. And the end of a campaign is a return to three farmers, with one old friend
waiting in a town. What wears out is the supply of stories, not the structure.

**For Kapman:** a hard cap on named people (lesson 3); the division of the house as a new small start with old friends
in it (lesson 4). Kapman's concept already borrows the tales: "the best crewmen become tales that later crews tell".

---

## 7. Dwarf Fortress (2006; Steam edition 2022)

### 7.1 How one becomes many

- **Seven dwarves and a wagon.** You choose their skills and their supplies.
- **Migrants.** More arrive in waves, drawn by the wealth you have made, up to a cap you may set (two hundred by
  default (?)). You do not choose them.
- **You never command a dwarf.** You mark what should be done: dig here, build a bed, brew. Dwarves take the jobs
  their labours allow.
- **Work details** are the first handing-over. The labour screen (key y) lists groups on the left (Miners,
  Woodcutters, Planters, Haulers and so on) and every citizen on the right with a tick box. Each group has three
  settings: "Only selected do this", "Everybody does this", "Nobody does this".
- **The manager** is the second, and it is forced. At twenty dwarves "work orders will not be performed until they
  are validated by the manager", and the manager "absolutely requires an office". A work order is a standing
  instruction for the whole fortress, and it can carry a condition, such as brewing whenever drink falls below a
  count (?). This is the fortress learning to run itself.
- **Rank.** The settlement is named by size and wealth: hamlet at 20 dwarves, village at 50, town at 80, city at 110,
  metropolis at 140, each needing a sum of made or exported wealth as well. Rank brings nobles, who make demands, and
  at city rank the monarch may come.
- **The danger grows with you.** Wealth and numbers draw thieves, then sieges, then great beasts (thresholds (?)).

**The screens.** One cross-section of the earth, one level at a time. A strip of buttons at the bottom for marking
work. Lists: citizens, labour, work orders, nobles, stocks. A scrolling line of announcements.

### 7.2 What the hands do

- **Start.** Pause. Mark a stair and a few rooms. Unpause and watch seven named dwarves dig. Place a carpenter's shop,
  a still, a field. You know who the brewer is.
- **Middle (30 to 80).** Stockpiles, a dining hall, a squad. You write work orders and stop placing single jobs. A
  migrant wave of fourteen arrives and you tick boxes for ten minutes. Something goes wrong in a way you did not
  foresee, and that is the story you tell afterwards.
- **End (150 and more).** The fortress feeds itself. You are an architect: a great hall, a tower, a pump stack to bring
  magma up. Or you go looking for trouble, because none is coming. The clock is running at a fraction of its speed.

### 7.3 The end screen, to draw it

A top-down slice through rock in small square tiles, earth-brown and slate. A grid of carved rooms fills it: a long
dining hall with forty tables, a block of sixty identical bedrooms like a honeycomb, a row of workshops each three
tiles square, stockpiles striped by kind. Hundreds of tiny bearded figures, each a few pixels, clog a central stair.
Top left, a small panel: the date, the fortress's name and rank ("Metropolis"), population 203, and a row of stock
counts. Top right, a number in small type: 14 FPS. Down the left side, a column of round alert badges: a siege horn, a
crying face, a crown with a demand. Along the bottom, a bar of square tool icons. An open list window at the right
shows work orders: twenty rows, each with an item, a count, and a small condition.

### 7.4 What players say

- The clearest statement of the empty end I found: "Usually by year 2 or 3 once you have around 3-4 squads, over 150+ dwarves and 1000+ food and drink each slowly climbing you basically have nothing more to look forward to." https://steamcommunity.com/profiles/76561197980128392/recommended/975370/
- "the FPS will slowly decline, and you'll never get to play a game for very long...IDK what they're doing code-wise bc my CPU is running at under 20% utilization yet the game is crawling at 4 FPS" https://steamcommunity.com/profiles/76561198069038623/recommended/975370/
- "this game doesnt make good use of multi threads CPU and even with a monster machine still ends up beeing able to lagout almost every game" https://steamcommunity.com/profiles/76561198162971507/recommended/975370/
- The game's own wiki agrees on the cause: "the vast majority of processing time in Dwarf Fortress is taken up by units taking their turns, over 60% in larger forts", and its first advice is "Limit the number of dwarves by setting a population cap." https://dwarffortresswiki.org/index.php/Maximizing_framerate
- The same wiki sends players of large forts to an outside tool for labour, which says what the built-in screen is like at two hundred. https://dwarffortresswiki.org/index.php/Labor
- Praise for Dwarf Fortress is for its stories, and the stories players tell are of small forts and of falls. I did not collect quotes for that; it is common knowledge of the game and marked here as mine (?).

### 7.5 Is the first hour still there?

No. The first hour is seven known dwarves in a hole. At two hundred the dwarves are a crowd and you are a town
planner. The game is honest about it: its motto is that losing is fun, and the cure for a finished fortress is a new
one. The machine also sets a limit of its own: the simulation of every person slows the clock until the fort is
abandoned.

**For Kapman:** the manager's order with a condition is exactly "orders in words", and proof that it works. The
warnings: a crowd is not a crew (lesson 3), and safety is the end of the game (lesson 5).

---

## 8. RimWorld (2018)

### 8.1 How one becomes many

- **Three survivors** of a crash (the standard start), each with skills, traits, a past and things they will not do.
- **New people come by events**, not by purchase: a prisoner talked round, a refugee, a wanderer. The storyteller that
  deals the events wants a colony of a certain size. From the wiki: "The odds based on population decrease until the
  20th colonist", after which the chance of a joiner sits at a floor (lower for the two steady storytellers than for
  the random one). So the game leans against growth past about twenty without ever forbidding it.
- **You do not command, you rank.** The Work tab is a grid: colonists down the side, about twenty-eight kinds of
  work across the top. In the simple mode each box is ticked or not, and "tasks on the left rank more important than
  tasks on the right". In the manual mode each box holds a number from 1 to 4, and "all available work of one priority
  will be done before the next priority". A bright outline shows who is good at what.
- **Standing rules.** A bill at a workbench can say "do until you have ten". A schedule sets each person's hours. Zones
  say where things go. You can still take any one colonist by the hand, and in a fight you always do.
- **The danger is a sum.** Raid points = (wealth points + people points) x difficulty x two smaller factors, between
  35 and 10,000. Wealth gives nothing below 14,000 and 2,400 points at 400,000. Each colonist adds 15 points in a poor
  colony and 140 in a rich one. A colony that has just lost people is eased for a while.
- **The ends.** Build a ship and hold out while it warms up. Or, in a later expansion, sell the whole colony and walk
  on with a handful of people to start again, three times over (?).

**The screens.** The colony from above. A row of colonist heads along the top. Tabs along the bottom: architect, work,
schedule, assign, animals, research. Letters arrive as coloured envelopes down the right side.

### 8.2 What the hands do

- **Start.** Three people, a wooden hut, a patch of rice. You draft them by hand when a mad squirrel attacks. You know
  that one of them will not haul and one of them is in love with the third.
- **Middle (8 to 12).** The Work grid is set and mostly left alone. You plan rooms, queue bills, and step in by hand
  for surgery and for raids. A caravan goes out. This is where most stories happen.
- **End (20 and more).** Raids of a hundred, fought at walls built for the purpose. Between raids: fiddling with
  bills, packing caravans, waiting on research. The clock stutters.

### 8.3 The end screen, to draw it

A top-down view of a walled compound in a dry valley, drawn with flat, round-headed, limbless people. Inside the wall:
a block of bedrooms, a long freezer, a workshop hall, a hospital, a field of crops under sun lamps, a row of
generators. Outside one gate, a zigzag corridor of sandbags and turrets facing a scorched field. Across the top
centre, a row of twenty-two small portrait heads, three with a red mood bar, one flashing. Down the right edge, a
stack of envelopes: two blue, one yellow, one red. Bottom left, a list of selected items; along the whole bottom edge a
bar of tab names. Bottom right, the date, the weather, the temperature and four speed buttons. Open in the middle: the
Work grid, twenty-two rows by twenty-eight columns of small numbered boxes.

### 8.4 What players say

- "I have over 700 hours in this game, yet I enjoy every single playthrough" https://steamcommunity.com/profiles/76561198052397636/recommended/294100/
- "RimWorld has so many ways to kick your ass if you're getting too comfy, unless you're playing on peaceful/builder. Some of the late game random events are brutally unfair unless you know specifically how to prepare for them." https://news.ycombinator.com/item?id=36752688
- On danger that grows with wealth: "If you play well, your 'reward' is just scaled punishment. There is no playing well." https://steamcommunity.com/profiles/76561198402938696/recommended/294100/
- "Rimworld has crippling performance issues that make running a late game colony impossible." https://steamcommunity.com/profiles/76561198162827737/recommended/294100/
- "it is IMPOSSIBLE to run a lategame colony without powerpoint FPS slowing to a crawl." https://steamcommunity.com/profiles/76561197988874042/recommended/294100/
- "Most the time, just setting up a caravan and finding all the junk in all the inventories became the biggest part..." https://steamcommunity.com/profiles/76561198100797497/recommended/294100/
- On helpers' sense: "Making sure your guy cleans a room at a time because if you highlight everything it'll decide to go from one corner, open 5 doors, clean another spot..." https://steamcommunity.com/profiles/76561197974017105/recommended/294100/
- "Every colony has the same indistinct pawns, the same resources, the same handful of events and indistinct rival factions." https://steamcommunity.com/profiles/76561198014958492/recommended/294100/

### 8.5 Is the first hour still there?

Better than in Dwarf Fortress, because the game pushes back against size, and because you can always take one person
by the hand. But past twenty the faces blur, the wealth sum sends armies, and players learn to keep their colony poor
on paper to keep it safe. A hidden sum that punishes success teaches players to play the sum.

**For Kapman:** a soft lean against growth is better than a hard wall; one person can always be taken by the hand (your
own boat). Danger that grows with wealth must be in the open and must come for a thing, not from a multiplier
(lesson 5).

---

## 9. The ten side by side

| Game | You start with | You end with | How work is handed over | What limits the many | The reset | First hour kept? |
|---|---|---|---|---|---|---|
| Crusader Kings II, III | One person, one county | An empire of hundreds of title-holders | Titles given to people with opinions and contracts | Domain limit, vassal limit | Death; lands divided among sons | The verbs yes, the stakes no |
| The Guild 2, 3 | One craftsman, one workshop | A family, a dozen businesses, offices | A switch on each business | Titles (building counts); three family members in hand | Death without an heir ends the game | No: the craft is deleted |
| Old World | One city, one ruler | A dozen cities, forty units | Not handed over: rationed | Orders each turn; city sites | The ruler dies, the heir rules; a turn limit | Yes: scarcity of orders never ends |
| Suzerain | One president | One president | Nothing is | The script | The term ends | Yes, wholly |
| Stardew Valley | One farmer, 15 seeds | One farmer, hundreds of machines | Tools, and three small helpers per hut | The day's length; the farm's size | None | The place yes, the pressure no |
| Wildermyth | Three heroes | About a dozen, across three generations | Jobs on the map in hero-days | Legacy points; a small roster | Each campaign ends; heroes return young | Yes, and it deepens |
| Dwarf Fortress | Seven dwarves | Two hundred | Work details, then a manager with standing orders | A population cap; the machine | The fortress falls or is left | No: a crew becomes a crowd |
| RimWorld | Three colonists | Twenty or more | A grid of priorities; standing bills | The storyteller leans against growth; wealth draws raids | Leave by ship; or sell up and start again | Partly: one person can always be taken by the hand |

Three things stand out.

1. **Every game that keeps its first hour keeps the number of people you deal with small.** Wildermyth a dozen, Old
   World a pool of orders, Crusader Kings a handful of direct vassals, Suzerain one cabinet. The games that lose it
   (The Guild, Dwarf Fortress, late RimWorld) let the list grow without a tree above it.
2. **Every game that players forgive for its end has a death in it.** Where nothing ends (Stardew, The Guild 3, a safe
   fortress), the complaint is always the same sentence: nothing left to look forward to.
3. **Nobody complains about too few things to command.** The complaints are about not knowing why something happened,
   about collecting, about safety, and about being punished by a hidden sum.

---

## 10. Five lessons for Kapman, and one proposal each

These add to the first paper; they do not replace it. The three-crossings rule, letters, orders in words, the winter
council and "you always have a boat" all stand, and all ten games support them.

### Lesson 1. Limit the owner's own hand, and grow by a tree of people

*From Crusader Kings' domain and vassal limits, and from The Guild's failure to have any.* An emperor holds no more
castles than a duke; he holds dukes. The number of people the player deals with stays about the same at every size,
and that is why the count's screen still serves the emperor.

**Proposal: the reach.** The owner gives orders in words to at most **three** skippers besides his own boat. A fourth
keel and beyond must sail under a **partner**: a senior man, himself a person with a want and a fear, who takes up to
four boats and gives them their orders. You give the partner one sentence, with two dials, as a Crusader Kings
contract has two:

> **Folkmar** sails for **a half share**, in **the Frisian sea**.

- His share: a third, a half, two thirds. A kinder share buys his loyalty and his care; a hard one loses both.
- His freedom: this route only; this sea; where he sees fit. More freedom earns more and tells you less.

With the cap of eight keels proposed in the first paper this gives: your boat, three skippers, and one partner with
four. You never deal with more than four people about ships. The partnership is the period's own félag; a share
agreed in thirds or halves is my assumption and should be checked before it is called period practice (?).

### Lesson 2. Give the house a fixed budget of attention

*From Old World's orders, against the title lists of late Crusader Kings and the keg sheds of Stardew.* The length of
a turn should depend on the player's interest, never on the size of the house. Johnson: such decisions "quickly become
rote as the number of units in the game grows".

**Proposal: seven matters.** At the winter council the owner has **seven tally sticks**. Every change costs one: new
orders for a ship, a keel laid, a factor placed, a marriage, a gift, a suit at law, a partner's terms. Whatever is not
touched runs as it did last year. One boat and a debt will not use seven; eight keels, four ports and three children
want twenty, and the choosing is the winter's game. The number does not grow with rank. (One more may be earned by
keeping a clerk, which is one of the church's uses in the first paper's table of sinks.) In the sailing season the
same rule already exists: three talks a call. A reply to a letter should cost one of the three.

With it, take Old World's ending too: each winter the house is offered **three wishes** and takes one: a plot at
Hedeby, a daughter married at Ribe, three crossings to York. A life is ranked in the chronicle by wishes kept, not
by silver.

### Lesson 3. What is handed over must vanish from the hands and come back as one line with its reason

*From The Guild 3 ("the logs are jumbled garbage that dont really tell you whats going on"; the helper who "runs to the
opposite side of the map"), Stardew's kegs, and Dwarf Fortress's crowd.* Three separate failures: a report nobody can
follow, a chore turned into collecting, and people who have become a count.

**Proposal: the line home.** Each voyage by one of your ships returns **exactly one line** to the house's log, in the
Chronicle's voice, and the line carries the skipper's reason, taken from the same beliefs his trader brain used:

> Folkmar sold the wine at Quentovic and not at Lundenwic: a Frisian at Walcheren told him Lundenwic was full. 61 d
> after wages. He was nine days late: he lay four days at the Maas mouth for wind.

Three rules go with it.

- **No collecting.** The silver is in the tally where the ship lies. There is nothing to click to receive it.
- **Harm names itself.** Any loss to the house says who, where, why and how old the word is, or says plainly that it is
  not known: "No word of the Gull since Lammas."
- **Twelve names.** The house has at most twelve named people: the four in your boat and eight more (skippers,
  factors, a partner, kin). Other crews are a number, "five men". Wildermyth keeps a dozen and every one is known;
  Dwarf Fortress keeps two hundred and none is.

### Lesson 4. Let death divide the house, so every life starts small

*From Crusader Kings' partition (which players complain of and love), Wildermyth's heroes who return young, and
RimWorld's selling-up.* The reset is what lets a game run for 273 years without an end like The Guild 3's.

**Proposal: the division.** When the owner dies, a card divides the house.

- Each grown child and each partner **keeps the boat he sails**.
- The heir you choose to play keeps the home plot, the chart with its lifted fog, the chest, **one boat**, one feud and
  one friend (as already decided on 4 October).
- The others become **kin houses**: named houses in the world that carry your letters without charge, give you their
  word on prices, and sometimes ask for help. A brother at Hedeby is worth more than a storehouse there.
- A route the father had sailed three times needs **one** crossing by the heir before it can be handed over, not three.
- You may also choose the youngest child and start again with one boat and a famous name.
- In any winter an old owner may **hand over** and play the heir from then on.

So each life begins at about the second or third rung of the first paper's ladder, with a wider world and more people
who know your name. That lands and goods were divided among sons is well known for Frankish kings (Verdun, 843);
whether a Frisian merchant's boats were divided so is not something I could check (?).

### Lesson 5. Danger must grow in the open, come for a thing, and never run out

*From RimWorld ("your 'reward' is just scaled punishment"), Dwarf Fortress ("nothing more to look forward to") and
Crusader Kings ("so large you no longer face real threats").* Two opposite failures: a hidden sum that fines success,
and a size past which nothing can touch you. The first paper's section 4.6 already says the dangers grow with you;
this is how to do it without either failure.

**Proposal: what is said of you.** One word stands on the House tab and beside the tally, and the player can always
read why:

| The word | What makes it | What it draws |
|---|---|---|
| **Unnoticed** | One small boat | Nothing but the weather and the toll |
| **Known** | Two or three keels; a name at two ports | Rivals learn your routes; a reeve expects his cask |
| **Well-off** | A plot and a storehouse; a factor | The toll-keeper weighs harder; kin ask for loans |
| **Rich** | Stock lying in three ports; a big ship | Raiders choose where your silver lies; the lord asks a ship for his levy |
| **A prize** | Eight keels, silver in store, a house with rank | A king wants you as his man or wants what you have |

Rules: only what can be seen counts (ships in harbour, goods in a storehouse, gifts given in public). The danger
arrives as the game's dangers already do, hours or days ahead and with a way out. It strikes a place or a ship, not
the house as a whole. And the player may choose to be smaller: silver buried in a pot is not seen (hoards of the age
are found all round these seas); silver given to a church becomes standing; stock spread over ports is a smaller
prize in each. A storm sinks a great ship as surely as a small one, so the sea itself never becomes safe.

---

## 11. What this does to the end game's look

The first paper's picture stands: the chart, the marks solid, hollow or faint by the age of the word, the day stick's
second row. These lessons add four small things and no new screen.

- **The reach, as four faces.** On the House tab, a short row of square portraits in thin black frames: never more than
  four, the people you give orders to. Under each, a line in Gentium: "Folkmar. Half share, the Frisian sea. Last word
  nine days old." A partner's own boats are a count beside his face, not faces.
- **The word.** Beside the tally, one word in Byzantium: "Rich". Hover or click, and three plain lines say why.
- **The winter council's margin.** Seven notched sticks down the left edge of the council page. Each matter you settle
  breaks one. When the seventh is broken the page can only be closed.
- **The log.** One line a voyage, newest at the top, each with its ship's small mark; a line with harm in it has a
  thin red rule beside it.

The late-game screen, drawn: the pixel chart from the Humber to the Elbe. Your own mark, solid, off Texel. Three
hollow marks in your colour with small ages by them, "6 d", "9 d", "31 d", the last one faint. One thin Byzantium
thread runs from the Rhine mouth to the Thames. By Hedeby, a small flag that is not yours but carries your house's
sign with a bar across it: a brother's. On the black glass rail at the right, four faces. At the bottom left, the
tally and the word "Rich". On the day stick, a second row of five notches, one of them red. Beside the chart, one
letter card. It is the first hour's screen with eleven more things on it.

## 12. Your decisions

1. **The reach:** three skippers and one partner, or some other number.
2. **Seven matters** each winter, the same at every size of house.
3. **Twelve named people** at most in the house.
4. **The division at death:** each son and partner keeps his boat and becomes a kin house; the heir starts smaller.
5. **The word said of you,** shown openly, with ways to be smaller.
6. **Three wishes a winter** as the measure of a life, in place of silver.

## Sources

Rules and numbers:

- Crusader Kings III: https://ck3.paradoxwikis.com/Domain ; https://ck3.paradoxwikis.com/Vassals ; https://ck3.paradoxwikis.com/Dynasty ; https://en.wikipedia.org/wiki/Crusader_Kings_III
- Crusader Kings II: https://ck2.paradoxwikis.com/Demesne
- The Guild 2: https://en.wikipedia.org/wiki/The_Guild_2
- Old World: https://www.designer-notes.com/old-world-designer-notes-1-orders/ ; https://www.designer-notes.com/old-world-designer-notes-11-the-end/ ; https://en.wikipedia.org/wiki/Old_World_(video_game)
- Suzerain: https://en.wikipedia.org/wiki/Suzerain_(video_game)
- Stardew Valley: https://stardewvalleywiki.com/Sprinklers ; https://stardewvalleywiki.com/Junimo_Hut
- Wildermyth: https://wildermyth.com/wiki/Legacy ; https://wildermyth.com/wiki/Job ; https://en.wikipedia.org/wiki/Wildermyth
- Dwarf Fortress: https://dwarffortresswiki.org/index.php/Labor ; https://dwarffortresswiki.org/index.php/Manager ; https://dwarffortresswiki.org/index.php/Population ; https://dwarffortresswiki.org/index.php/Maximizing_framerate
- RimWorld: https://rimworldwiki.com/wiki/Work ; https://rimworldwiki.com/wiki/Raid_points ; https://rimworldwiki.com/wiki/AI_Storytellers

Players' words: each quote has its link beside it. Steam reviews are linked by the reviewer's profile and the game's
number; forum threads by their own address.

## What I could not check

- Web search was closed for this session, and the Paradox, Bay 12 and Reddit forums refused the reader. Steam's forum
  search answered for Stardew Valley and The Guild 3 only. So the players' words come mostly from Steam reviews; the
  forums would give longer and angrier ones.
- Every quote was copied out by an automated reader, not by eye. Check the wording at the link before a quote is used
  anywhere outside this paper.
- From memory and marked (?): the layout of each end screen; The Guild's title and building counts; Old World's order
  sums, turn limit and number of ambitions; the length of a Stardew day; Wildermyth's roster size; Dwarf Fortress's
  default cap, order conditions and siege thresholds; RimWorld's sell-up ending; that Crusader Kings players restart
  small on purpose.
- No page was found for The Guild 3's rules (its Wikipedia address led elsewhere), so its rules are from reviews and
  memory.
- I found no quoted praise for Dwarf Fortress's stories and no complaint about Wildermyth's late campaign in what
  could be reached.
- Two period points in the proposals are unchecked: shares in thirds and halves between partners, and the division of
  a merchant's boats among sons.
