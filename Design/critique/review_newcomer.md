<!-- 7 October 2026, about 11:20 to 12:10. One of three critics the user asked for. Looked at, read and counted; not
played. Unity and git were not run. There was no web search: what real reviews said is from memory, marked "as I
recall", and no review is quoted. Nothing in the project was changed but this file. (sum) = my own arithmetic from
the project's numbers. -->

# Kapman: the newcomer's review

## 1. Where I stand

I am the mainstream PC reviewer with ninety minutes and forty other games in the queue. I am not a history reader, and I do not forgive a game for being worthy.
I care about the first hour above all: can I tell what to do, is anything at stake, does the interface fight me, does it look like something I want to keep on my screen, would I tell a friend.
My nearest games are Sunless Sea and Sunless Skies, Dredge, 80 Days, Roadwarden, Citizen Sleeper, Pentiment and Cultist Simulator, and I score as PC Gamer does: a number out of 100 and one sentence.

### What I saw, and what I did not

**Saw.**

- All twenty-two pictures in `Captures/chart` (01 to 22, taken by the test at 11:32), each opened and looked at.
  Every "capture" below is one of them.
- Code, read and not run: the `Frame` folder (FrameRoot and its parts, PersonTalk, the Trade, Port, People, Tasks,
  House, Field and Self panels), `ChartWorld`, `Course`, `Effects`, `Cond`, `EncounterSession`,
  `KapmanGame.KnowHome`, `MainMenu.NewGame`. Tests: `ChartGameTests` whole, and the names of the rest (352 in all).
- Forty-five cards in full, with their words, choices and effects: ten of `sea_793`, ten of `rhine_frisia_793`, eight
  of `crew_793`, five of `england_south_east_793`, four of `chain_house_debt`, two of `chain_pilots_secret`, two of
  `danes_norway_793` (Ripa), and one each of `harbour_793`, `chain_storm_vow`, `chain_black_stones` and `long_793`.
- Dorestat's whole entry in `places/dorestad.json`: the eight eras, the ten sayings, all sixteen people (their lines,
  looks, wants and fears; their ties in part). Seven texts of `texts_793.json` and the first riddle.
- The plans: `GAME_MAKER.md`, `DECISIONS.md` and `QUEUE.md` whole; `wiring_plan.md` section 8; the data of layout G
  in `template3.html`; `steam_release.md`; `wiki.md`; `fate_faith_and_self.md` sections 1, 2 and 6a to 6c; the short
  answers of both ship view plans; the art list's counts and its read-me.
- What was found already: both critiques whole; `first_ten_minutes`, `hours_check`, `doubles_and_faults` whole; the
  short answers and the parts I cite of `money`, `crossing`, `cards`, `art`, `popups_and_windows` and `routes_fix`;
  the head of every file in `fixes` and of the four audits.

**Did not see.** The game running. Any sound. The title. The chest and its riddle board. The Tasks, House, Word,
Chronicle, Field, Crew, Ship and Sky windows on a screen (no picture of them exists). Any water but Dorestat to
Traiectum. Lundenwic. The 3D ship (not built). Any final art (none exists). Where a fault is read from code or data
and not seen in a picture, I say so.

**The ground moved while I read.** A builder was at work in the same hour. At 11:45 the fix for the four ports with
no way went into the code (`Sim/LaneOpenings.cs`), with a test that every home can reach every port. Before it, a
Dane or a Frank began in a port the boat could not leave. From 11:47 the files for meetings at sea were being
changed. I saw neither run. Both are left out of the cons below.

## 2. Pros

Eighteen. Each is one thing, where I saw it, and why it is good from where I stand. None repeats the two critics of
6 October; where I judge the same thing differently, I say so.

1. **The predicament in one line.** Capture 01, the top bar: "1 May 793 early morning", "86 d silver", "bread for 7
   days", "Owed 480 d, due in 365 days". Most games in my queue open on a menu. This opens on a sentence that is
   already trouble. I knew what I had, what I ate and what I owed before I clicked. (The player critic's complaint
   was about the mock-up, where the debt hid under the face. The build put it where it belongs.)

2. **Days said as a spread, in words, and the way drawn bend by bend.** Capture 04: "About 2 to 5 and a half days
   for your boat, by the wind. 434 km by water." Capture 08: "About half a day for your boat, by the wind: foul wind
   today", with the course laid in gold along every turn of the river. It tells me a passage is a bet, with no
   tutorial. 80 Days showed each road's days before you paid for it, as I recall, and it is the right unit here.
   Capture 08 is also the best the chart looks.

3. **Your people recolour the world while you choose.** Captures 12 and 13: click "Dane" and behind the window the
   sea goes teal, the land amber, and the face changes from a brown beard to a fair one. I know no other creation
   screen in this kind of game where a choice shows itself before I confirm it. (Cons 8 and 20 say what is under
   the paint.)

4. **Creation is one page and under a minute, and each pick says its price and its door.** Capture 12, the faith:
   "It asks: No loading, selling or carting on a Sunday; no court sits. It opens: Abbeys as hosts and as keepers of
   silver: a guest is to be received as Christ." Sunless Sea asks for a past and an ambition up front and, as I
   recall, says less about what they will cost you. This tells you both sides in one line.

5. **A past gives a thing, a person and a place to go.** Capture 15: "Your father's lead and line. A lead with a
   hollow for tallow, to bring up the bottom, on a line knotted at the fathom. The knots are his." Then: "Someone
   knows you: The pilot at Witla sailed with your father and taught you the bars." It is the only line on the first
   screens that gives me a reason to sail somewhere. And Witla is where the best small story I read begins (pro 11).

6. **Stats as words.** Capture 15: "halting", "unknown", "steady enough", "has heard things", "observant". I knew
   who I was without knowing what a pip is worth.

7. **A line in another tongue turns into yours, as far as you can follow it.** Captures 19 and 20: Ravn's saying
   comes up in Norse and turns piece by piece: "Cattle die, *deyja frændr, deyr sjalfr it sama,* But a noble name
   will never die, *hveim er sér góðan getr.*" Under it: "Norse. Hávamál 76. You half follow it: some of it is lost
   on you." Play a Dane (21, 22) and the host of Ripa's verse turns whole: "Fire is what he needs who has come in
   cold to the knee". It is the one moment in twenty-two pictures where the game does something with its hands and
   not only its words, and it is who you chose to be, shown in a sentence. (How it is undercut is con 16.)

8. **The first card advertises the game behind it.** Capture 03: under "Declare the cargo" sit "Hide the swords
   under the ballast" and "Show the charter", greyed. At minute ten I learn there is smuggling here, and papers that
   waive a toll. Citizen Sleeper shows its locked actions in the same way, as I recall. (How the lock is worded is
   con 14.)

9. **The card I will see most was written to be seen again.** `harbour.tollman.frankish`: the second time, "You
   again. As last time?"; after an honest call, "You hid nothing last time. I looked."; after being caught, "Last
   time there was more under your ballast than ballast. I tap everything of yours now. Stand back from the boards."
   Sunless Sea's reviews, as I recall, faulted ports that say the same thing on the fortieth visit. This card has
   six states.

10. **A price inside a scene, with a swindle you can catch.** `harbour.black_haired_merchant.dorestad`: wax at
    thirty, "and I am robbing myself. The church pays forty and complains." You may "Put the cakes on your own scale
    first" (six chances in ten). Buy blind and the card sets `rf_short_wax`, and he warns you off the man who will
    find out: "The prior at Traiectum is a suspicious old man." A tip, a trick and the next port in one card. This is
    trading as a short story, and it is what a market table cannot do.

11. **"The far mark."** `chain_pilots_secret` s02. The pilot of the Maas mouth is going blind and his daughter
    covers for him: "What a lot of room there is between the post and the ridge today." Then: "I talk too much.
    Everybody says so. It is a great help to him." You may say what you see "as if by chance", tell him plainly, or
    "sell the word in the ale-houses up river" for 30 d, and then it travels as news. Money against decency, nobody
    tells me which is right, and the wrong one is the one a trading game pays for. Best card I read. (The critics
    praised house_debt and storm_vow. This one was only on a list.)

12. **The crew is funny.** `sea.river.mist`: "A bird. Brown, the size of a hen, and you will never see it. It does
    that all spring to find a wife." And: "My mother said it was a drowned ox calling from under the water. My
    mother was not often right about birds." `crew.faith.lent_meat`: "My belly keeps no priest's calendar. The pig
    is already dead. It will not be less dead at Easter." The earlier critic heard one dry voice in every mouth. I
    heard jokes that land, and games about the year 793 are seldom funny.

13. **Riddles you carry with you.** At Sarre, Tidburh asks hers while she opens oysters: "You have the look of a
    man who has eaten the answer." Ravn's first, "What did I have yesterday that I want today?", is answered with
    ale, and `riddles_793.json` wakes it when you next see ale in a market (data; I did not see the chest). It is
    something to turn over in my head across a crossing: the one built thing I found that makes empty minutes at sea
    worth having.

14. **A calendar you can plan against.** Capture 16: "The Lord's day ... in 4 days", "The Rogation days ... in 12
    days", each with what is "Not to be done". The player critic called Sunday a tax you cannot plan round. With the
    days counted down beside a measure in days, it becomes a puzzle I know how to play: get in on Saturday or lose a
    day. (Today it has no teeth, and its own panel says so: "nothing stops a sale on a feast". See con 10.)

15. **The comparison is on the row.** Captures 03 and 09, under the wine at Traiectum, with an eye beside it: "You
    saw Rhenish wine pay 34 d at Dorestat, this morning." Capture 01, with an ear: "A boatman at Dorestat said
    Noviomagus was paying poorly for Rhenish wine. A few days back." I did not need a ledger screen to know whether
    I had gained, and what I saw is told apart from what I heard by one small mark. The idea was praised before.
    What I praise is that it is built as a sentence under the good. (What happens when three of them stack is con 15.)

16. **The market moves under my hand and says so.** Capture 09: "Sold 1 cask of rhenish wine for 36 d, less 3.6 d
    toll: got 33 deniers." The silver on the top bar goes from 86 to 119, and the next cask is offered 29. One click
    taught me the toll and the glut.

17. **A person's looks in print stand in for the missing face, and work.** Capture 17, under Ravn's line: "About
    forty-five, lean, weathered, fair hair gone grey and tied back, a short beard; a plain dark wool tunic and sea
    cloak, a knife and a whetstone at his belt, a small leather bag of rune-cut sticks." Roadwarden ran its whole
    length on prose and small pictures. The grey shape beside this is not a hole while that sentence is under it.

18. **The world's people reach into each other's ports.** In `places/dorestad.json` Thiadbod "pays the Maas pilot
    by the year, so that others wait". At Witla the pilot says: "Thiadbod's ship is looked for. Until she is in I
    take nobody over. He pays me by the year for that. I never said I liked it." (`harbour.pilots_shed.witla`.) I
    met my rival's money a port away from my rival. That makes twelve ports one place.

Two smaller ones. The ship window tells the hour and the work: at the berth with the yard down under a morning sun
(01), under a striped sail in the afternoon (02), under stars and a moon at Traiectum (03, 09). It is the only thing
on screen that changes while I read, and my eye went to it in every picture. And the game photographs itself:
`ChartGameTests` plays a new game through the frame and saves numbered pictures on every run, which is the only
reason a critic who cannot run it can point at things.

## 3. Cons

Twenty-one. Each says how bad it is from where I stand (it would stop me playing; it would cost a point; it is a
scratch) and the smallest thing that would mend it. None repeats the earlier critics or today's studies. Where one
of them found the fact and I judge it differently, I say so.

### It would stop me playing

1. **The first thing at stake falls due fifteen real hours away.** The ruling of 7 October (no "Sail on", nothing
   faster than 4x) and the clock (a game minute a second) make a game day six minutes at best. Capture 01 says "due
   in 365 days": 36 hours of running clock (sum). The planned day, Michaelmas, is 151 days from 1 May: 15 hours.
   `money.md` states that figure (line 72) and passes on. Nobody has summed what follows. The book still says "A
   season is an evening" and "Twelve to fifteen hours for a life" (`GAME_MAKER.md` lines 194 and 416): both were
   written for the rule that was struck. By the same sum a life of 793 to 805 is something like 250 to 290 hours at
   4x (sum; winters skipped from Martinmas to 1 April, which no page fixes). The free demo that "ends at the
   Michaelmas reckoning" is a fifteen-hour demo. And the start moved from 21 June to 1 May, which took the book's
   first-hour deadline (40 d on St John's day) from three days off to fifty-four.
   *Smallest mend, inside your rule:* a first payment due about ten game days in, which is one hour at 4x, sized to
   about half of what the first four casks fetch, cut on the stick by the steward at minute 0 and shown on the top
   bar. And one page that sums the real hours of a season, a year and a life under the clock as ruled.

2. **After the money fix, an hour's trading buys nothing.** `money.md` found this and called it "the price". I call
   it the first thing a review would say. Its own figures for the tuned game: "A laden leg makes 13 d in the mean",
   "55 of 100 legs carry nothing", and "At 4x an hour is ten game days: 8 d in the middle hour". The first hour
   sells the first four casks and feels rich; hours two to ten earn eight pennies each against 480. The small
   things silver can become (gear at 12 to 20 d) are held back to stage 2, after the hour this queue is building.
   Dredge sold me two upgrades in its first hour, as I remember it. Numbers that do not go up, with nothing to
   spend them on, is where I stop.
   *Smallest mend:* bring three small buys into stage 1 (the book already prices reef bands at 12 d and tow gear
   at 15 d), each of which changes a leg, so that a thin week still buys something.

3. **In the measure's own picture, the names and the land do not match.** Capture 04. The names and the gold line
   have drawn back to show Lundenwic and Dorestat together, and the land under them has not. Medemelacha's name
   stands at the right of the screen while the lake it sits beside is still drawn in the middle. Lundenwic's mark
   lies on the Dutch coast, the line to Traiectum runs across Holland, and the greyed English names (Dommoc,
   Gipeswic, Dofras) lie on Dutch fields. In capture 08, drawn nearer, the names and the land agree. It may be the
   test taking its picture before the wider chart had come. I cannot tell without running it. If a player sees
   this once, the chart is not trusted again, and the chart is the key to everything.
   *Smallest mend:* look at it first. Then make the test wait for the picture and check that one point under the
   line that should be sea has the sea's colour.

### It would cost a point

4. **Three casts that never meet (two points: this is the selling point).** At Dorestat the toll man you can talk
   to is Aldgrim (capture 05: "Aldgrim, Tollman, at the king's toll post"). The toll man on the card is "Liudbrand"
   (03). The teller of a price is "The toll man at Dorestat" (08). The wiring plan knows the first two are not
   joined. The third is the one that matters. The named people tell no prices: `PersonTalk` gives a greeting, a
   rumour, a saying and a riddle, and nothing else. The prices come from a nameless row, "Talk to people", which
   hands over "Prices heard for 14 places, now in your ledger" in one click (07). So the promise on the first page
   of the book, "every price is something a named person told you last week", is kept by nobody with a name.
   *Smallest mend:* where the teller's role is held by a person of that port's file, print the name ("Aldgrim, the
   toll man at Dorestat, said ..."), and let "Ask what is said here" hand over one price from the board with his
   name on it.

5. **The opening card is written, and the build's own tests keep it from the opening.** `chain_house_debt` s01,
   "The tally", is the steward with the split hazel stick: "Hold your half against mine. They fit now, and they
   will fit at Michaelmas." The book opens on it. But a new game's call at home is made quiet, and two tests hold
   it so (`EncounterChartTests.ANewGamesCallAtHomeBringsNobody`; `ChartGameTests`: "a card opened at home on a new
   game: nobody comes down to a boat that has not arrived"). It can come only when she next moors at home. So the
   first card a new player meets is the toll man of the next town, and the debt is a label nobody has explained.
   The ten-minutes check asked for two log lines. I would not: the card is better than any log line.
   *Smallest mend:* a mark on a card ("opening") that the quiet call lets through, and one line changed in each test.

6. **The first card decides nothing, says its rule four times, and sets the scene last.** Capture 03. The toll man
   says the toll "is a tenth of what you sell here". The small print under him says "The king's tenth is taken on
   what you sell here". The Trade window beside the card says "The toll takes 10 in the hundred of what you sell
   here." I chose "Declare the cargo" and capture 11 answered: "The toll is taken on what you sell here. Liudbrand
   thinks better of you (easy)." The line that sets the scene ("The toll man comes down to the boat with his tally
   stick") comes after his three speeches, in the smallest type on the card. The one live fork, "Give him something
   (12 d)", does not say what it buys. It buys half toll for thirty days, and on four casks it costs 12 d to save
   about 7 (sum).
   *Smallest mend:* the scene line first, above the speech; a cost line under each choice ("12 d now; half toll
   here for 30 days"), which the book's law 4 already asks for; and no result line that repeats the rule.

7. **Talk is a vending machine.** Capture 17: "Ask what is said here / Ask for a saying / Ask for the riddle /
   Take your leave". Every person has the same buttons, the lines are fixed, nothing is given or taken, and
   capture 18 says how they feel about you "is not kept yet". The count will "Speak of the debt" ("I do not want
   your reasons. I want two pounds") and cannot be paid. After three people I would stop pressing Talk, and the
   people are the best writing in the game.
   *Smallest mend:* one thing a person does for each call (a price, a text, a riddle put in the chest, a job), and
   the button named for it: "Ask Aldswith who wants a berth", not "Ask what is said here".

8. **A Dane's game is a Frisian's game in other colours.** Captures 14, 21 and 22. The Dane at Ripa has the same
   boat, a Rhine cargo (Mayen quernstones), and "Owed 480 d" to a Frankish count he has never met. The market is
   priced "in g silver" while the purse reads "86 d". The row under the bread says "You saw ship's bread pay 4.3 d
   at Dorestat, this morning": the game begins every merchant at Dorestat behind the creation window and then moves
   him. And Ripa's own host greets its own Dane, on screen, with "A Frisian boat, and small. Good. Small boats pay
   what they owe. Whose guest are you on this bank?" The card study counted that no card asks who you are. This is
   what that looks like on the first morning.
   *Smallest mend:* for each start, one creditor line, one home cargo and one first card; clear what was "seen"
   when she is moved; a line for a guest of the host's own people.

9. **The first day moved and the words did not.** Read from the data, not seen on screen. The game now begins on
   1 May so that Lindisfarne (8 June) is news you live through; the test asserts it has not fallen yet. But the
   priest of Dorestat still greets you with "I have a letter from York, thirteen days old, and I wish I had not
   read it", and one of his rumours gives the day of the raid. The count still thanks you for pennies left "at
   midsummer". The tally card still sends for the count's day "in 100" days, which from 1 May is 9 August. The one
   piece of news the start was moved to stage is spoiled at the first Talk.
   *Smallest mend:* a `when` on a person's line (the engine has conditions; the lines have only years), and these
   three lines read again.

10. **The game keeps offering what it does not do.** Known already: Hail, Rest and Offer answer "not yet". New
    from the pictures and cards: every row of the Port window ends in "(to come)" (06), the one that works
    included ("Talk to people. News, prices elsewhere, the weather (to come)."). The log says "Night falls. [Z] to
    rest till dawn." (03, 09) while the Rest button says no hours are skipped. The card "Go or wait"
    (`crew.council.wind`) asks the game's own central question, and "Go now" does not cast off and "Wait a tide"
    does not wait: both move two men's feelings by two or three. And the first day is a feast, "Not to be done:
    work" (16), on which the game asks me to work and nothing follows. Each is small. Together they teach a new
    player in ten minutes that words here need not mean anything.
    *Smallest mend:* "Go now" presses Cast off and "Wait a tide" starts the wait. Take the [Z] line off the chart.
    Hide a row whose thing is not built, and take "(to come)" off the one that is. Start on 2 May, or let the toll
    man mind the feast.

11. **The windows are letterboxes.** Capture 01, the main screen. Three windows stand one over the other and each
    shows the top of a longer list: one good and half of the next in Trade ("Ship's bread" is cut through its
    second line), two choices of three in Port, three people of eleven in People here. The list I most want, the
    market, is the shortest of the three. In Columns (05) words break inside themselves: "Medemel/acha", "Walichru/m", "quernstone/s".
    The size study measured the letters. This is about what the eye lands on: three half-read lists.
    *Smallest mend:* in port, the market gets the height it needs and the other two share the rest. Never break
    inside a word. Leave Columns out of the first release.

12. **The best built answer to an empty sea is not on the screen.** `FieldPanel` is built and tested: "One of them
    is ahead of you: the market will have met her cargo first." It is in no workspace a new player has. Under way
    the Sea workspace shows Voyage and Hold (02, 10): a list of ports and a list of casks, neither of which changes
    until she moors. It also reads the true place of every ship, which is more than a merchant who "never sees a
    true price" should know.
    *Smallest mend:* Sea is Voyage, The field, and Wind and tide. The field shows only ships someone told you of.

13. **There is nobody to be.** The portrait says "You" (01). Nothing asks my name, my house's name or my boat's.
    She is "The Gislinge boat" (12), which is the name of a find, not of a boat anyone sailed. The hold says the
    casks are "Your own from the start" (02), where the plans call them the count's. The pitch is a house that
    outlives me, and the house has no name. One of the eight pasts is "The widow who kept the boat", under a
    bearded face, in a town that says "Shipman" and "In you come, son". The art list already has
    `self.frisian.woman`.
    *Smallest mend:* two words typed at the start (yours, the boat's), a man or woman chip, and the name in ten
    greetings.

14. **Machine words reach the player.** "the world has the colours of the Tide palette" (12). "Either: Only with
    weapons aboard; or: Only with captives aboard." (03). "Only with the writing "charter toll dorestad" in your
    chest." (03). "Liudbrand, the toll man · easy" and "thinks better of you (easy)" (03, 11): a mood that reads
    as a difficulty. "Your standing here: known (+25). In the region: known (+10)." (01). And money has three names
    in ten minutes: "34 d", "got 33 deniers", "3.4 g silver" (01, 09, 14). You said the game reads as made by AI
    when it is bland. These lines are worse than bland: they are the machine showing.
    *Smallest mend:* one test that fails on any text on screen with a number in brackets after a word, a quote
    mark round a rule, or "palette"; a hand-written line for each lock; one word for a penny.

15. **The game's best idea reads as a form letter when it stacks.** Capture 08, on picking Traiectum: "The toll man
    at Dorestat said Traiectum was paying poorly for Rhenish wine. A few days back." Then the same sentence for
    Mayen quernstones. Then the same for ship's bread, with "well". Capture 05 does it with "A boatman at Dorestat".
    One line under one good is the charm of the thing (pro 15). Three in a row with one word changed is a table
    wearing a costume, and it is the first thing I read when I choose where to sail.
    *Smallest mend:* when one teller says several things of one port, say it once: "The toll man at Dorestat, a few
    days back: Traiectum pays poorly for wine and querns, well for bread."

16. **The turning line gives back what it says you lost.** Capture 20. The verse stops half turned and says "You
    half follow it: some of it is lost on you." The line straight under it is the whole saying in plain English:
    "Cattle die, and kinsmen die, and so one dies one's self. A good name does not." Nothing was lost on me. The
    one mechanic that makes a tongue matter costs nothing where it is first shown.
    *Smallest mend:* when a verse stands over its own plain version, show the plain version only as far as the
    verse turned.

17. **The stake can be paid from a button three workspaces away, in any port.** Read from code. `TasksPanel`: "Pay
    20 d" and "Pay it all" show wherever there is a market, Ripa and Lundenwic included. The best scene in the
    debt's story, the two halves of the stick laid together in the count's hall (s03), can be skipped by a click on
    workspace 4.
    *Smallest mend:* the buttons show at home only, and open the count's card.

18. **The chest pays out worse words than the cards that lead to it.** Read from data. Solve Tidburh's oyster
    ("The sea fed me and the waves covered me. I have no feet.") and the prize is the version of 1842: "He who
    produced me, with a watery covering deck'd me ... afterwards burns me unboil'd". Beowulf comes, a fitt an
    evening for 43 evenings, as "Waxed 'neath the welkin, world-honor gained". The house rule for cards is no
    mock-archaic. The chest is real archaic, and I would stop opening it.
    *Smallest mend:* the game's own plain line first, the old translation behind "as printed in 1842". The file
    already holds one own draft (`alcuin_793`).

19. **The period-word pass will cost clarity.** A plan, not yet applied. `wording_fixes_ports.md` and `_sea.md`
    would change "skipper" to "steersman", when "the steersman" is already my own man on most of the sea and crew
    cards; "lands to port, lands to starboard" to "lands on the back side, lands on the steering side"; "the
    Baltic" to "the East Sea". This is 145 lines made truer and harder to read. It is the same bargain as the port
    names, made again.
    *Smallest mend:* keep the narrator's plain words. Mend only what a player would feel as wrong (small change in
    a world of weighed silver). Put the period's word in the wiki's hover.

20. **Too many kinds of picture, and one start gets the hardest chart to look at.** On screen now: a speckled
    eight-colour chart, smooth book type, a face in fat pixels, a flat drawn boat. Planned: a 3D boat in the look
    of a 1997 console, with "four or five PSX looks to flip between" (queue, step 4). Each part is handsome alone,
    and the plum frame holds them together better than it has a right to. But the chart is where it frays. The
    Frisian's (01) reads as land and sea. The Dane's (14) is amber and purple speckle on teal: it wins the
    thumbnail and tires the eye at full size. The palette study tested whether colours can be told apart. Nobody
    has asked whether a player wants to look at each one for forty hours.
    *Smallest mend:* one rule (type is the only smooth thing; every picture sits on the faces' pixel grid), one
    look for the boat, chosen, with no switch, and each people's chart looked at full size for a minute before it
    ships.

### Scratches

21. **Small frictions, each a minute to mend.** Look out opens a window with a "Go on" button to say "No sail in
    sight" (10), which the log says too. The talk window opens on top of the card's first three choices (07), and
    the portrait block prints the card's first lines again beside the card (03). "half a day" is the least the
    game can say, so 8 km, 17 km and 30 km all read the same (02, 04): the river legs of the first hour want "an
    hour or two" and "a morning". Riddles drop into the log by themselves on arrival: one at Traiectum "from the
    cathedral schoolmaster" before I had met him (03), four lines of them in the first minute at Ripa (14). That
    spends the cards' best prop. Chart names show in the gaps of the frame: "Mogontia" under the ship window in
    most of the pictures, half a name between the windows (03, 09). The stand-in boat has the same three roofs
    behind her at Dorestat, Traiectum and Ripa, and green hills on the Dutch Rhine (02). And the chart is silent:
    the audit found that. My judgement is on the rule beside it. No music is a fair choice (Pentiment got by on
    pens, bells and village noise, as I recall), but only if what happens has a sound. Inside CC0: a bell at the
    mooring, a horn at a sail made out, oars at the cast off.

## 4. The review

Written as it would read if the game shipped in the state the plans describe for the first release: one life, 793
to about 805, twelve ports, the frame as ruled, the money fix in, the crossing filled as `crossing.md` plans, your
own sprites. The manner is PC Gamer's. **(seen)** rests on today's pictures, code or data. **(plan)** rests on the
book, the rulings, the queue or a study, and has not been built. Where the plans disagree on the first day (the book
says 21 June, the code and the tests 1 May), I review the 1 May game, because that is the one in the build.

---

### KAPMAN review

*Every price is something somebody told you last week. The somebody is the best part. The week is the problem.*

**Need to know**
**What is it?** A merchant game of the North Sea in 793, run in words from a frame round a chart.
**Expect to pay:** not set.
**Developer:** one person, with AI help for code and draft text; the store page says so (plan).
**Reviewed on:** not run. See the note at the top of this file.
**Multiplayer:** no. **Out:** Early Access, spring to summer 2027 (plan).

I owe a count 480 silver pennies and I have bread for a week, and Kapman says so in one line across the top of the
screen before I have clicked anything (seen). Then his steward holds up half a split hazel stick: "A day has no
price, and that is what makes it dear." (The words: seen. That he opens the game: plan.) It is a fine first minute.
It is also the last time for a long while that the game is in a hurry.

You never steer. You run the boat from a thick plum frame: eight verbs top left, windows of text under them, the
face of whoever is speaking top right, a log along the foot, and the boat herself in a small window that looks like
a PlayStation game from 1997 (the frame: seen; that look: plan). The chart in the middle has a purple sea and
speckled olive land, and it is the first thing here I would know among forty thumbnails (seen). You pick a port,
read "2 to 5 and a half days, by the wind" (seen), and cast off. Then the clock runs, a minute of the game to a
second of yours, at one, two or four times and never faster (ruling). Things happen to you as cards.

The idea that holds it together is gossip. No screen shows what wine fetches in Lundenwic. Under the wine in your
own market a line says "A boatman at Dorestat said Noviomagus was paying poorly for Rhenish wine. A few days back."
(seen), and 138 other ships are moving goods and stale news on the same winds (code, read and not run). You are
always betting on something a week old. I know no other trading game built on this, and when the word names a man
you have drunk with and he turns out wrong (plan), it will be the thing people tell each other about.

The writing earns that. A toll man who remembers: "You hid nothing last time. I looked." A wax seller who robs
himself at thirty pennies a cake and would rather you did not own a scale. A pilot going blind while his daughter
says what he can no longer see: "I talk too much. Everybody says so. It is a great help to him." You can sell that
secret in the ale-houses for thirty pennies, and the game will carry it up the river as news (all seen, in the card
files). It is funny, which worthy games are not: "The pig is already dead. It will not be less dead at Easter."
And it has one good trick with its hands. A Northman on the dam gives you a proverb in Norse, and it turns into
English as far as your own tongue can follow, and stops (seen).

**The first hour**

Three choices make you: a people, a faith, a past. Pick the Dane and the sea turns teal behind the window, which
is the best thing on a creation screen I have seen in a while (seen). Then the tally, three talks, a hold that
takes four casks, and the question of where to carry them (plan). The answer is Lundenwic, 415 km off. At the
fastest speed that is thirteen minutes in a fair wind and half an hour in a foul one (plan: `crossing.md`'s sum).

The plan fills them. Every four game hours two or three small offers light: a lesson in Danish from the man off
watch, a riddle, a seam to stop, the lead to heave, and each pays in something you keep (plan). I have read one
such crossing written out minute by minute and it reads well. None of it is built, and all of it rests on my
wanting the thirtieth lamp as much as the first. My hour ended on the Strand with one sale made. Dredge was praised
for a loop that fits a lunch break and faulted for running short of ideas late on, as I recall. By the end of its
first hour it had sold me two upgrades and shown me something with too many eyes.

**What does not work**

Time. The debt falls due at Michaelmas, 151 days on. At four times speed that is fifteen hours of my life before
the first thing at stake is settled (sum). Sunless Sea was praised for its prose and faulted for a slow boat and
ports that repeat, as I recall, and its sequel was praised for getting on with it. Kapman has looked at that boat
and built a slower one. 80 Days got round the whole world in a couple of hours and was loved for it. Citizen
Sleeper was at its best while a clock was biting, and reviewers noticed the hour it stopped, as I recall.

Money. Once the other ships have worked the markets, a loaded leg clears thirteen pennies and an hour clears eight
(plan: `money.md`'s own figures). Past a few pennies of gear at a yard, the next thing to want is a boat at three
hundred (plan). For long stretches the numbers do not go up and there is nothing near enough to save for.

The frame. Nineteen panels, five workspaces, three ways to tile them, three sizes each, pop-ups over the chart
(plan: layout G). I spent the minutes arranging windows that other games have me spend arranging cargo. Cultist
Simulator split reviewers on whether its refusal to explain itself was mystery or chore, as I recall. This is the
same wager with more buttons.

And the names. Medemelacha, Walichrum, Witla. The modern name is in a window (seen), not on the chart.

**Who it is for**

Roadwarden was praised for its prose and its limit of days, and faulted for how much it asks you to read before it
moves, as I recall. Pentiment was praised for its pictures and its lettering and for making the past sound like
gossip over a fence, and faulted for being slow and light on things to do, again as I recall. If you finished
either, this is yours, and you will forgive it a great deal. But Pentiment only ever asked you to read. It never
asked you to wait. If you want Dredge's loop, twenty minutes out and back and a better engine at the end of it, you
will not see the second hour.

**THE VERDICT**

**68**

**KAPMAN**

The best-written trading game I have waited this long to get moving in: a real idea, a real voice, and a clock that
spends your evening before it spends the merchant's.

---

*The score.* PC Gamer scores out of 100 and, as I recall its own scale, calls the 60s above average and the 70s
good. 68 is the plans taken at their word, less what the clock and the thin money cost in a first hour. About ten
points of what is missing are the clock's.

## 5. The same review's verdict if it shipped as it is today

1. One loop runs: say who you are, pick a port from a list, cast off, watch, moor, answer the toll man, sell a
   cask. In twenty-two pictures it ran clean and looked like a game, and a week ago it did not exist.
2. Nothing can be won or lost. The debt is a label due in 365 days, which is 36 hours of clock; there is no winter,
   no wreck and no end; no card can cost an hour.
3. Half of what is on the screen says what is not there: three of the eight verbs answer "not yet", every row of
   the port says "(to come)", and four windows end on a line about what is not kept or not in the game.
4. The best of it is off the screen: the written ends of the cards are not shown, the opening card is kept back,
   the chart is silent, the sea in the pictures is empty, and every face but your own is a grey shape.
5. The words that do reach the screen were good enough that I read forty-five cards out of the files after the
   game had stopped asking me to.

**30.** That is low on PC Gamer's scale, as I recall it: the place for something that does not yet work as a game.
It is the honest place for a frame that went in today, and no judgement on what is in the files.

## 6. What would move my score most

Five changes, in order. The points are on the planned 68, each taken alone. They overlap: all five together are
worth about fourteen, which is the low 80s, where the reviews of Sunless Sea sat, as I recall. I have not asked for
the clock to change. You have ruled on it twice and I scored inside the rule. The first two are how I would win
back most of what it costs.

1. **Something at stake that falls due inside the first hour, said by the steward at minute 0.** Plus 6 (plus 10
   on today's 30). Let the tally card through the quiet call. Cut a first payment on the stick about ten game days
   out, sized to hurt. Show its day on the top bar beside the silver.

2. **Make the first hour a river hour: six short legs, not one crossing.** Plus 5. Dorestat, Traiectum, Witla and
   Walichrum are about two to fifteen minutes apart at 4x (sum, from the days in capture 05 and the Lek now open).
   Give each a want of its own, so that a short leg pays and teaches, and keep Lundenwic as the hour's last act.
   Put three things to buy at 12 to 20 d in those ports, each of which changes a leg.

3. **One cast.** Plus 4. The person in People here is the teller on the price row and the face on the card. His
   word is marked true or stale when you moor. "Aldgrim told me wine paid well at Lundenwic, and Aldgrim was a week
   behind" is the sentence I would say to a friend, and today no part of the game can produce it.

4. **Show the written end of a card, and let a choice do what it says.** Plus 4. The outcomes are the best prose
   in the files, and no code shows them. "Go now" casts off. "Wait a tide" waits. The safe choice costs hours.

5. **One fixed screen for the first hour, swept clean.** Plus 3. No workspaces, layouts or sizes until hour two.
   In port: the market tall, Voyage and People here under it. At sea: Voyage, The field, Wind and tide. No "(to
   come)", no "not yet", no number in brackets, no rule's name in quote marks, no half row cut by a window's edge.
