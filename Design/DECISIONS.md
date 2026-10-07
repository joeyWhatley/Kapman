# Decisions, 4 October 2026

The user's answers to the eighteen decisions in GAME_PLAN.md, given through the picker. They override the design
documents' defaults where they differ.

| No. | Decision | Answer |
|---|---|---|
| 1 | How long Dorestad takes to sail through | **Keep it long**: 15-19 minutes from the berth to the level's edge; the join to the sea sits at the bake's edge, not at the first bend. Voyages from home run longer than the GDD's ~30 minutes. |
| 2 | Passing from a river into the sea | A 3-5 s shot from the deck as the river opens to the sea, skippable after the first time. |
| 3 | Other ships' pace | They keep the player's pace (chain minutes, with the same date jumps as the player's). |
| 4 | Storm wave height | Full real height on the sea levels; half only if the boat's motion becomes unreadable. |
| 5 | First ship | The Gislinge, with Dorestat 1's numbers dropped in until a proper model exists. |
| 6 | Drift on the shortened sea | Accept the 150x drift for now; look again after testing. |
| 7 | Capsize in a harbour or river | Salvage in harbour and river (days and silver, cargo spoiled); loss at sea. |
| 8 | Wait in port | Yes: the days tick by in the panel; the clock's visible rate stays normal. |
| 9 | The date at a join | **The date jumps** by the cut-out stretch's real sailing time (Dorestad-London about 46 game hours, not 30); the chart's mark jumps with it. |
| 10 | Difficulty and the debt | Difficulty is the lord's terms only (easy: 480 d by Michaelmas; hard: 20 pounds); the debt is a fixed sum due on a day. |
| 11 | Position offshore | A fuzzy area on the chart: sun and stars fix the heading, landmarks the place. |
| 12 | Night | As a dark-adapted eye sees it: stars, outlines, no colour; a lantern resets it. |
| 13 | Human sound | Wordless (market murmur, rowers' breath, a shout at the sail); no music; a cowbell recording stands in for the hand bells for now. |
| 14 | Our own recordings | **No: CC0 only.** Gaps stay empty until a CC0 recording turns up. |
| 15 | Animals beyond birds | **Research realistic HDRP animal packs** and give a short priced list. Birds stay the only visible animals until then. |
| 16 | Wildlife and prices | Strong swings, 20-60%: the seasons are trade strategy. |
| 17 | The climate of 793 | Modern normals cooled by 0.8 C (more hard winters, the river freezing some years), warming into the Medieval Climate Anomaly by about 950. |
| 18 | Odds on encounter cards | Shown as a word: sure, likely, doubtful. |

Answered later the same morning: **the river current pushes the ship** through Dorestad (downstream quicker, drift when the oars stop, harder work coming back up): `pushesShips` true in chain.json and on the river's RiverCurrent.

## 4 October, afternoon: the game's shape (after playing Dorestad at about 20 fps)

- **Camera:** a strict isometric view while sailing near land (rivers, coasts, harbours); a cinematic and free camera only on the open ocean, away from coastlines and land.
- **World:** water ribbons only: rivers, coasts and harbours in detail; little or no inland detail.
- **Joining:** an open world, streamed along the rivers and coasts (replaces the cut-and-date-jump joins for river travel).
- **Scale (the same afternoon):** the world's lengths and distances at about 1:80 (Dorestad to London about 30 minutes at 1x); the ship, the buildings and the rivers' widths stay true size. (Correction: at true scale the ship moves at her real speed in real seconds, so 350 km would take about 39 real hours, not 38 minutes.)
- **Banks:** a waterfront band of about 60 m beside the water (banks, jetties and dams, the first row of houses and trees, reeds); beyond it a soft haze.
- **The camera near land:** chosen later from a small test scene (a simulated sea, wind and waves, several angles and views); the user wants it much closer and lower than the 50-degree view tried today.
- **Next:** start again from the top on the new world map.
- **The world (picker, the same afternoon):** the North Sea, Britain and Brittany, Norway, the Baltic and on to Kiev, Iceland, Greenland and Vinland, in one open world; the rivers up which she sails: the Rhine and its branches, the Thames and the English rivers, the Scheldt, the Canche and the Seine (and the river road to Kiev); the geography real and put back to c. 800 where the research knows; each port's waterfront scaled by its importance (Dorestad about 300 m, a market town about 150 m, a hamlet or monastery about 50 m).
- **River widths (the user: "the river width to be in scale with the boat, that's the important part"):** every river at its true width beside the true-size boat; only lengths shrink to 1:80. Waters wider than about 300 m (estuaries, broad lower rivers, firths) narrow to about 300 m, opening out to the sea at their mouths.
- **The scale (from the scale test, https://claude.ai/artifact/ThtrhAgEQt8M5UtLRa4uhb): 1:40.** At 1:80 and smaller the river towns run into each other (Dorestad and Traiectum, 18 km apart, would share one waterfront); at 1:40 each keeps its own water and the river still winds like a journey. Dorestad to London about 1.6 hours at 1x, 23 minutes at 4x. Whether the open seas are shortened harder (the recommendation: 1:80 or more) is still open.
- **The camera near land:** view B1 (the camera test, https://claude.ai/artifact/6Xsr4khCDoeziiJFUqyAN7) raised to 35 degrees down ("slightly higher ... looking down on an angle more"), about 35 m across, a soft lens bulge enlarging the boat; zoom out to the widest view of the water and its bands, then on through the clouds to the 2D map.

## 4 October, night: the chart is the game (picker answers)

- **The chart is the game.** You sail on the 2D map: set a course, her mark moves on the clock at a real ship's speed, ports open as panels, the fog lifts. The 3D pixel world is a window to open, not the place the game is played. Scene: `L_Chart` (Design/chart_game.md).
- **The map's picture** is plain PNG tiles in `MapArt/world` that the user redraws: pixel-flat, the Byzantium palette, the sailing world (Ireland to Kiev and Novgorod, Bordeaux to the Lofoten) at 120 m a pixel.
- **The placeholder map is brought up to one level of detail everywhere** (the user: "why is frisia weirdly hyper detailed but everywhere else isnt"): period land cover for all of it, not only the Netherlands' reconstruction.
- **A house with heirs.** A wreck costs the boat, the cargo and maybe a man; the house goes on and the heir takes the chart, the chest, one feud and one friend. This replaces "wreck = game over" (2 October, answer 7's loss at sea).
- **The clock is held while a panel is open** (the market, the chest, a talk, a card, the port's menu). Only sailing and waiting cost days. Resting and waiting for a tide still run it.
- **Next build: the word on the item** (Design/research_trade_seasons_and_hints.md, section 6).
- **Theme colour: Byzantium (#702963)**; palette in Design/palette_byzantium.gpl. The user is learning pixel art (Design/pixel_art_zero_to_hero.md).

## 7 October 2026: after the critics and the game maker (picker)

The book to follow is Design/GAME_MAKER.md (the game maker, with Design/maker/hands.md and heart.md, answering
Design/critique/player.md and designer.md). The user ruled on four of its six questions:

1. **No "Sail on".** The clock keeps 1x, 2x and 4x and nothing faster. Empty hours are not skipped. (The standing rule
   against time warps holds.) So the leg has to be worth its real minutes: the order and its signs, the field of other
   sails, cards with teeth, the ship window, the crew, and things to do on the chart and in the chest while she sails.
   Stage 1 step 5 of the book ("Sail on") is struck and must be replaced by this.
2. **The first release:** one life, 793 to about 805, on twelve ports, with the whole world dim on the chart. Later ages
   come as later releases.
3. **The portrait:** the player at rest, a year older each winter; whoever is speaking while they speak.
4. **The slave trade:** in the world and in the cards. The player may carry or refuse; word of it travels as news does,
   and the heirs bear it.

Not yet ruled, so the book's recommendation stands until the user says otherwise: the ship window is a drawn stand-in
first and 3D in stage 2; the wide wiring plan (Design/wiring_plan.md) is held, its foundation kept, and a loader is let
in only when its stage opens.

Earlier the same days: the interface is the Command frame (Design/ui_mock/template3.html, layout G), thick rounded,
map showing through, run from the frame with the map as a visual key, a measure in days; ink panels; five map colour
schemes on K; the heir is the next in the line of succession; agents never more than the user allows (30 at most,
and far fewer when usage is short).

## 7 October 2026, later the same day (rulings made while the builders were at work: builders, read these)

1. **The eight function buttons are apart from the windows.** "The panels must be separate function from the 8
   buttons above them." Buttons are verbs (cast off or lie to, wait, measure, look out, sound, hail, rest, offer);
   windows show and set. The note at the end of Design/wiring_plan.md has the detail.
2. **A window system like i3 or sway, and lots of pop-ups.** Workspaces; rows, columns or tabs; any window holds any
   panel. Talk and events are floating windows over the middle of the chart. "The game will be all in text basically,
   the map is more just visual cue."
3. **An in-game wiki** (Design/wiki.md). It is the nineteenth panel. The user chose by the picker: it FILLS AS YOU
   LEARN (the rules are open from day one; ports, goods, people and words come in when seen or heard of, each with how
   you know it), and the real account of a dated event shows ONLY AFTER that day has passed in the game. A linked
   word in any window or pop-up opens its page. To the builder of panels: register a Wiki panel; if you reach it, give
   it a first true version (the rules that are in the game, and a page for each port, good and person the player
   knows, from the data); if not, one honest line.
4. **Art: descriptions and placeholders, not made-up characters.** "Instead of creating the characters just make a
   description instead and a placeholder, for me to go over later and make the art for it, and for the UI and really
   everything I will need templates for and assigned correctly and organised so I can make the sprites myself."
   So: no face kit and no generated faces. Every picture is a slot with a fixed name, a size, a written description
   and a placeholder (Bake/art/make_art_list.py; the list is Art/art_list.csv; the user works in the Desktop folder
   Kapman Art and pull_art.py brings finished pictures to Art/final). The frame has its own slots now (kinds hud and
   shipview, from Bake/art/artframe.py) and Art/hud_assign.json says where each piece goes and how it is cut in nine.
   To the builders: where the game shows a face or an icon, load it by its slot through one small class (Art/final/
   <file> if it is there, else Art/placeholder/<file>, read from the project folder as MapArt reads chart tiles).
   A person of a places file has the slot portrait/person.<id>.png; a person of weight portrait/weight.<id>.png; the
   player portrait/self.<people>.<man or woman>.png. Skinning the whole frame with the hud pieces is a later step.
5. **Only a card stops the clock** (the picker, 7 Oct; this changes the rule of 4 October that any open panel pauses).
   Tiled windows never hold the clock, at sea or in port: the player reads, plans, trades and looks things up while she
   sails. Only a card or a talk that needs an answer holds time, and the player's own pause. It follows from the ruling
   of no time skipping that Rest and the tide wait must not run the clock faster than the chosen speed either (they
   run at 30x and 60x today: Design/research/next/crossing.md); they become waits at 1x, 2x or 4x.
6. **Everything links to the wiki** (the user, 7 Oct). Words and phrases of interest are a colour of their own anywhere
   on the screen; hover gives a brief overview; a click brings up the wiki on that page (Design/wiki.md, point 2; the
   mockup shows it: Design/ui_mock/template3.html, the functions linkBox, showTip and wikiGo).
   **Made exact by the user the same morning:** "It stops the clock but you can override it immediately if you want."
   So a card or a talk that needs an answer holds the clock when it opens, and the player may set the clock running
   again at once (the clock buttons, or Space) with the card still open: the card waits. Windows never hold it.
   Not ruled, so the plain default stands until the user says otherwise: a waiting card does nothing on its own; it
   is still there when the player turns back to it. (Whether a pressing card should answer itself after a time is a
   later question for the user.)
7. **The boat and the chart change places** (the user, 7 Oct). "The map and the 3D sailing boat can be changed focus with
   a simple button on the ... top left of the 3D sailing boat that switches place with the map and miniaturises the
   map." So the view of the ship has a button on its top left corner: pressed, she fills the screen behind the frame
   and the chart goes small in her window; pressed again, they change back.
8. **Chart things on the right, everything else on the left.** "The measure and all map related stuff will be on the
   right hand of the screen and all other stuff on the left." Right: the portrait, the chart tools (the measure in days,
   nearer, farther, find her, names, winds, sails, pins), the view of the ship. The measure is no longer one of the
   eight function buttons; Oars (out oars, or sail) has its place for now.
9. **Panels are brief, and open into full windows.** "The panels being modular and can change sizes, showing a brief
   summary of the panel's purpose and if you press it it brings up the full panel with expanded usage, say crew
   management can be put as a panel and it just shows brief stats for the crew management but you press it and it
   brings up the full window with full crew management and interaction ... do this for all panels." So every panel has
   two faces: a BRIEF face of one to four lines for the left column (the first line the one that matters now; sizes
   small, medium, large) and a FULL window with all its controls, which floats over the chart. To the builders: give
   the panel interface both faces; what is built so far is the full face.
10. **How pressing a card is.** "Depends on the urgency of the event, if it is indeed a raider then the player must
   choose an option ... a selected few days that pass; if they don't it'll just keep them there until it automatically
   presses an option." So each card has an urgency. An ordinary card waits for ever. A PRESSING card (a raider, a
   squall, a man overboard) holds the ship where she is until it is answered; it has a time of its own (hours or a few
   days, set by the card); if the clock is let run and that time passes, the card takes one of its options by itself
   (the card names which), and says so in the log.
11. **The 3D view of the ship** is to be built by the lead once the builders and the reviewer have finished (the user
   chose this by the picker), and the user allowed two agents to think it through first (Design/ship_view/). It is
   water only, with a simple dock when she is moored, sea, weather and wind simulated, the crew rowing, and "a few
   different shaders, make it psx style in a few different ways".
12. **An ordinary card waits a week** (the user, 7 Oct; this settles what item 5 left open and corrects item 10, which
   said an ordinary card waits for ever). "Ordinary cards wait a week for it to automatically press if the player
   pressed play without interacting with it." So: a card opens and holds the clock. If the player sets the clock
   running without answering, an ORDINARY card stays open for seven game days and then takes one of its options by
   itself (the card names which; if it names none, the one that changes least) and says so in the log. A PRESSING
   card holds the ship where she is and takes its option after its own, shorter time. The seven days count only
   while the clock runs.
13. **The art list is a catalogue, not a to-do list.** The user, on the count: "I have gotta draw 4720 slots??" No. The
   list names every picture the game could ever show; the game runs on placeholders. Each slot now says when to draw it
   (the last column of Art/art_list.csv, and DRAW FIRST.html in the Desktop folder): 1 the frame, 2 the first hour,
   3 stand-ins that cover many, 4 the first release one by one (optional), 5 later. For the loader in the game, the
   order of falling back is: a person's own portrait, else portrait/role.<role>.png; a card's own picture, else
   card/card.any.<stage>.png, else none; a site's own scene, else scene/scene.any.<kind>.png. Final art in Art/final
   wins over the placeholder of the same name at every step.
14. **The money fix: yes** (the user, 7 Oct, to Design/research/next/money.md). The world's traders work the markets
   from 1 April before the player's first morning; the famine of 793 falls on grain only; the English toll is a tenth;
   no card pays over 60 d and no chain over 240 d. The data part is ready (Bake/economy/apply_money_fix.py, checked,
   not yet applied); the pre-run is code; the card cap is a rewrite of amounts and words together, with a list for the
   user to read. All of it goes in when Unity is free, with the tests run.
15. **The palette mend: yes** (the user, 7 Oct). The seventeen colour moves of Design/research/next/art.md B3, so every
   ground can be told from its neighbours by every kind of sight; Byzantium stays in each palette and Vineyard gains
   it as its sea. Ready and checked (Bake/world/apply_palette_mend.py changes MapPalette.cs, Frame.uss, the preview
   script and the .gpl files together); applied when Unity is free. Note for the user: in Vineyard this replaces one of
   their own picked colours (#5A2970) with Byzantium and darkens the other (#70293F to #481224).
16. **Steam**: how it is done, what the game still needs before a page, a demo and a release, and an honest range of
   dates, are in Design/steam_release.md.
17. **"You do what you think needs to be done"** (the user, 7 Oct, about 11:15). The order of work is Design/QUEUE.md.
   Two choices made by the lead under it, for the 3D view of the ship (the planners asked): the boat is shown at her
   TRUE SIZE (the pack's longship shrunk to 7.7 m, with full-size men in her), because the view must not lie about the
   boat the player sails; and the oars follow the class table (six oars, four men: three pulling a pair each, one
   steering). The nine trap chains are left out of the first release, as the card study planned. The user may overrule
   any of these.
18. **Events move the stats** (the user, 7 Oct): "Stats can be effected by in game events with caps and dynamic
   decisions and what not like all the other games that have implemented such mechanics." A stat is a base (1 to 5)
   plus named marks that events put on it and take off, each with a size, a cause the player can read and an end; all
   marks together move a stat by no more than 2; choices may ask for a stat, offer a mark or cost one. The rule and
   the engine's part are in Design/fixes/stats_events.md.
19. **The ship view pulls from the sail scene** (the user, 7 Oct): "Remember we have the ship sail scene that you can
   pull from." The boat in the view is the Gislinge model (Assets/Kapman/Models/Ships/Gislinge, the same as in the look
   lab G:/kapman_unity/KapmanSail_dev), not the pack's longship shrunk (this corrects item 17); the sea, sky and
   weather take the look the user tuned in that lab; the pack gives the crew, the oars and the jetty's timber.
20. **Peoples and faiths that could be added** are listed in Design/peoples_and_faiths.md, with what I would add first.
21. **The boat in the ship view is the pack's ship** (the user, 7 Oct 11:35, after the Gislinge was offered): "Use the
   other ship model I think not the gislinge the other one works better." This overrules the boat part of item 19.
   The sea, sky and weather still come from the sail scene. Her size is chosen by eye so four men fit her; a picture
   at a true 7.7 m and one at the size judged best are made for the user to choose between.
22. **Which picture wins** (the lead, under the user's go-ahead; it settles a doubt the art loader's planner raised
   about ruling 13). Anything the user has drawn beats anything they have not: (1) the thing's own picture in
   Art/final; (2) its stand-in's picture in Art/final (the role's portrait, the card's stage, the site's kind);
   (3) its own placeholder; (4) the stand-in's placeholder. So a stand-in the user draws shows at once for everyone
   who has no drawn picture of their own.
23. **The first release is long, and that is right** (the user, 7 Oct, about 12:00). A critic reckoned that at the ruled
   clock (a game day is six real minutes at 4x) one sailing year is about 19 hours and 793 to 805 about 230 hours.
   Asked what the first release should cover, the user first said "Do it the same as ck3", then: "Actually I take that
   back. 230 is good at normal speed." So: the clock stays as ruled (1x, 2x, 4x; no faster speeds, no skipping), the
   first release stays one life from 793 to 805, and about 230 hours is its intended length. The book's "twelve to
   fifteen hours for a life" (Design/GAME_MAKER.md) is wrong by this ruling. What follows from it: the game must be
   worth 230 hours, so there must be far more to meet than the 420 cards that can open today, and the leg itself must
   carry the time (Design/research/next/crossing.md).
24. **Why the campaign is long, and a 3x speed** (the user, 7 Oct, just after item 23): "Because the player most likely
   will be playing at 2x or 3x and a grand campaign should take a really long time as the game goes on it'll become a
   multiple ship fleet manager trading sim." So the clock has four speeds, 1x, 2x, 3x and 4x (TO THE BUILDER OF PART 2
   OF THE QUEUE, numbers and look: add 3x to the clock buttons and keys; nothing faster), and the game is meant to
   grow from one boat into the running of many ships (Design/scale_to_fleets_plan.md).
25. **Your own company is a fellowship** (the user, 7 Oct): "Form your own 'company' or whatever term works best for the
   period." The term is the fellowship (Old Norse felag: goods laid together, partners called fellows); the sworn
   brotherhood of merchants across ports is a gild, which comes later; "the house" stays for kin and heirs. Founding it
   is an act at a quay with shares cut on a tally and an oath. Design/fellowship.md.
26. **THE CLOCK HAS FIVE SPEEDS, LIKE CK3, AND THE WHOLE TIMELINE IS ABOUT 230 HOURS** (the user, 7 Oct, about 12:15;
   this overrules the 3x of item 24 and corrects item 23, which rested on a misunderstanding). The user: "Wait what?
   793 to 805 will take 230 hours? I was thinking 230 hours for the entire timeline to the end date ... how do we fix
   that", then chose by the picker "Five speeds, like CK3". For 793 to 1066 to fit in about 230 hours a game day must
   average about fifteen seconds. So:
   - Pause, and five speeds. 1: one game minute a real second (a day in 24 minutes), for being aboard and watching.
     2: four times that (a day in 6 minutes). 3: a day in about a minute. 4: a day in about fifteen seconds.
     5: a day in about four seconds. Keys 1 to 5 are taken by the workspaces: use + and - (and the buttons), Space to
     pause, as CK3 does.
   - The clock drops back by itself when something needs the player: a card that needs an answer holds it (item 5);
     a sail sighted close, a pressing thing, nearing or making port, the debt's day drop it to a slow speed; each of
     these can be switched off in the settings; after it the player picks the speed again with one key.
   - Everything that counts time counts game time, so it holds at every speed: a card's week, a pressing card's hours,
     bread, wages, news. Nothing may be skipped or lost at speed 5 that would have happened at speed 1 (the world's
     ships, markets, weather, cards due): prove it by a test that runs the same seed at 1 and at 5.
   - The view of the ship does not race at high speed: it keeps its own calm motion and lets the sky and light pass.
   - The first release is still one life, 793 to 805: now about ten hours of sailing at speed 4, plus what the player
     stops to do.
   TO THE BUILDER OF PART 2 OF THE QUEUE (numbers and look): this is yours, in place of the 3x of item 24. The fuller
   write-up, if it is there by the time you start, is Design/fixes/clock_five_speeds.md.
27. **Alpha first, then content** (the user, 7 Oct, about 12:30): "I think we will get it to an alpha version with the
   basics all done then add more content." What the basics are, where each stands and what waits until after is
   Design/ALPHA.md. No new waves of cards, ports or chains until the alpha is reached; the content pass now running
   only fits the cards that exist to the rules (who speaks, what happened, who you are, dates, pay).
28. **The ship in the 3D view is 7.7 m with six oars** (the user, by the picker, 7 Oct 13:05, from the builder's
   side-by-side pictures): her true size, three men pulling a pair each and one at the steering oar.
29. **The 3D view shows no land** (the user, 7 Oct 13:10): "from the 3D view it should be very restrictive you cant see
   anything other than the boat and the water and just the port no land." Only the boat, the water, the sky, and when
   she is moored the jetty itself standing in the water. No banks, shore, hills or buildings, in any place or look;
   mist closes the distance.
30. **Trade that pays, and a debt of eight pounds** (the user, by the picker, 7 Oct 13:15, to
   Design/fixes/money_that_pays.md). Two numbers that go together or not at all: the world's traders leave more for
   others (SimRules.minMargin 0.12 to 0.25) and the starting debt is 1,920 d, eight pounds, not 480. By the model a
   laden leg then clears about 27 d and a sensible player pays in about half of seasons. Every place that says 480 d or
   two pounds must change with it (trade_rules.json, the debt chain's cards and words, the first payment of
   Design/fixes/first_hour.md, which was sized for 480 and scales by four, the wiki's rule page, the mock-up).
   TO THE BUILDER OF PART 2 OF THE QUEUE: this is yours, with the money already in your part. The numbers come from a
   model, not from play: they are to be looked at again once the alpha can be played.
31. **Winter is the player's choice each year** (the user, by the picker, 7 Oct 13:15). When the sailing season ends the
   player chooses: lay up, and one winter page settles it (repairs, what was spent, who died, the news, plans for
   spring), or stay out and play on day by day at their own risk. With winters laid up the whole timeline is about 220
   hours at speed 4. The winter page is a basic for the alpha only in its plainest form (the days pass, bread and wages
   are paid, the news arrives); the rest waits.
32. **No payment on the tenth day** (the user, 7 Oct: "On the tenth day? How is that even possible I don't think you'd be
   back from the first voyage?"). They are right: by the game's own figures Dorestat to Lundenwic is 2 to 5.5 days each
   way, so a first voyage is 4 to 11 days and more with time in port. Design/fixes/first_hour.md is wrong on this
   point and is NOT to be built as written. Instead (the lead's proposal, to be confirmed by the user): the first
   payment falls at St John's, 24 June, 55 days after the start, time for several voyages; it may be paid at home or
   to the creditor's man at any port of the realm, not only at Dorestat; its size is set by the money model, not by
   hand. With the five-speed clock St John's is still inside the first hours of play. The early stake in the first
   hour is then an order with a day (wine wanted at a named port), not a debt payment.
33. **The debt has a face: the creditor's man who keeps turning up** (the user, 7 Oct, in place of a fixed first
   payment): "Maybe it's someone on the debtors pay roll, a messenger or a debt collector and it's humorous in the way
   that he keeps appearing and demanding debt payments." So the first stake is not a date but a person: one man in the
   creditor's pay who carries the other half of the tally stick and finds the player wherever she is. This replaces
   the St John's payment of item 32 as the early stake; Michaelmas stays as the day of reckoning.
   How it works (the lead's sketch, for the builder of the debt and for the cards):
   - He is ONE person with a name, a face and a memory (a bond like any other), invented and marked so. The user
     names him.
   - He appears as a card, more often as Michaelmas nears: at home, on a far jetty at dawn, in the alehouse, at a
     feast, once in a boat alongside in fog. Where he could not possibly be is the joke; he never explains how he got
     there, and it is never magic: he has always just come on another ship.
   - Each time he asks for something on the stick. Any sum may be paid and he cuts the notch there and then (so the
     debt can be paid anywhere, in parts). Other answers: put him off (he comes sooner next time), feed him (he likes
     you and comes anyway), lie (a stat check; he remembers), hide (the crew find it funny or shaming, by how often).
   - The humour is in him staying the same while the places get stranger: polite, seasick, patient, fond of his food,
     never angry. No threats. What is at stake stays real: what the stick says at Michaelmas.
   - He is an ordinary card, never a pressing one: he will wait a week on the quay if he must, and says so.
   In the alpha: one card with a handful of places and lines, part payment working. More of him is content, after.
34. **A thin edge of land in the 3D view, made from where she is** (the user, 7 Oct about 14:00; this loosens item 29):
   "a super complex but simple land generation technique based on where the boat is at any time ... Not much just 2-5
   meters and the rest is a fog in PSX style." Where the chart says land is truly close on a side, its nearest 2 to 5
   metres stand at the fog's edge there, built by rule from the chart's own ground classes and the place's own number;
   beyond it only fog. No far shore, no hills, and nothing on open sea. The note for the builders is at the top of
   Design/ship_view/JOIN.md.
35. **A people is called what the one naming it would call it, in the spelling of the time** (the user, 7 Oct, after
   asking whether "English" should be "Anglo-Saxon"): "Let's call them what the people that you are playing as at the
   time or the person referencing them would call them, those who consider them English like maybe Alfred, and using
   spelling like they use to, 'Aenglish' or whatever it was."
   The rule:
   - In talk and on cards, a people is named by the SPEAKER's own word for them.
   - In the frame, the wiki and the character page, a people is named by the word of the PLAYER's people.
   - The spelling is the period's own, as attested (Englisc, not English). Every such name is a linked word: the box
     under the cursor says in plain words who is meant ("Englisc: the English; their own name for themselves").
   - Nothing is invented: a form goes in only with where it is attested; where the right tongue left nothing written
     for the time (Frisian in 793), the nearest attested form is used and marked.
   A first table, from the lead's memory, EVERY CELL TO BE CHECKED against a source before it is used (the tongues
   researcher's Design/content/lore/period_words.md part (c) is the place to start and to correct this):
   | the people | their own name | an Englishman says | a Northman says | a Frankish clerk writes | a Welshman or Irishman says |
   | the English | Englisc, Angelcynn; by kingdom: Mierce, Norþhymbre, Westseaxe, Cantware | (their own) | Englar | Angli; Angli Saxones | Saeson; Saxain |
   | the Danes | Danir | Dene | (their own) | Dani; Nordmanni | Gaill, "foreigners" |
   | the Northmen | Norðmenn | Norþmenn | (their own) | Nordmanni | Gaill; Lochlannaig |
   | the Franks | Franci | Francan | Frakkar | (their own) | Frainc |
   | the Frisians | Frisa (attested later) | Frisan, Fresan | Frísir | Frisiones, Fresones | - |
   | the old Saxons | Sahson | Ealdseaxe | Saxar | Saxones | - |
   | the Wends | by tribe: Obodriti, Wilzi | Winedas | Vindr | Sclavi, Winidi | - |
   | the Irish | Goídil | Scottas | Írar | Scotti | (their own) |
   | the Svear | Svíar | Sweon | (their own) | Sueones | - |
   Ids in the data do not change (the people "english" keeps its id); only what is shown.
36. **Other ships pass by, on the chart and in the 3D view** (the user, 7 Oct about 14:10): "With proximity ships passing
   by and you can see them passing on the map plus the 3D view." The world's other ships that are near her are drawn
   passing on the chart, and come out of the fog and pass in the view on the side and heading they truly have, the
   distance squeezed to fit the short view. The ship hailed, or the one that stops her, is one the player can see.
37. **Four starts for now; every people and every tongue in the world for everyone else** (the user, 7 Oct about
   14:20): "with 4 starts for now: Frisian, Saxon, Dane, Frank. But every single language and every single culture in
   the game for NPCs etc."
   - The player may start as a Frisian, a Saxon, a Dane or a Frank. "Saxon" is read here as the people till now called
     English in the data (id "english", home Lundenwic), whom their neighbours called Saxons; if the user meant the old
     Saxons of the continent, this line is to be corrected. Their name on screen follows item 35.
   - Everyone the player meets has their true people and their true tongue, from the whole list of the world
     (Design/peoples_and_faiths.md), not only from the four: a Welsh trader is Welsh and speaks Welsh, a Wend is a Wend.
     Each people needs, as data: what they call themselves and are called (item 35), names for their people, their
     tongue with attested words and lines (Data/story/tongues_793.json holds eleven tongues so far), their faiths.
   - For the alpha this holds for the peoples already in the data; the rest of the list is content, after the alpha.
38. **You speak one tongue; kin tongues are a struggle; tongues are learned or interpreted** (the user, 7 Oct about
   14:30): "Your character only speaks the language of your culture, Frisians struggle to understand Saxons and vice
   versa but can communicate like how it was historically, you can learn languages or have someone on your crew have a
   translator modifier based on their culture and where they're from."
   - AT THE START the player has one tongue, their people's, whole. Nothing else.
   - KIN TONGUES are followed in part, by how near they are (Data/story/tongues_793.json already holds "kin" with a
     share for each pair): Frisian and the Saxons' English very near; old Saxon near; the Franks' speech and the
     Northmen's with effort; Latin, Irish, Welsh and the Wends' speech not at all. (From memory of the scholarship:
     to be sourced.)
   - WHAT A HALF-KNOWN TONGUE DOES, beyond the turning line that is built: the sense comes through but not the detail.
     Choices that need fine words (a [Tongue] choice, a bargain, a lie) are harder or shut ("you lack the words"); what
     is heard of prices and news in that tongue is rougher ("dear", not "49 d"); a misunderstanding is possible and is
     a small story, never a punishment without warning.
   - LEARNING: by use (words heard and seen: built), by time among its speakers, by a man aboard who speaks it, by
     lessons where the data has them. Slow, shown in the Tongues panel, never bought with points.
   - AN INTERPRETER ABOARD: a man of the crew lends his tongues, by his people and his home (a Dane of Ripa has Danish
     and some Frisian from the trade; a Frisian of Dorestat some Frankish). Through him the player follows what is
     said, at one step worse than his own knowledge; stat choices made through another's mouth are harder; and he is a
     person: he may soften, sharpen or leave out, by how he feels about the player and the speaker. For this the crew
     must be people with a name, a people, a home and tongues (they are four unnamed men today).
   In the alpha: the first three points, on the tongues the data has. The interpreter comes when the crew are people.
39. **THE LOG MUST READ AS SENTENCES** (the user, 7 Oct, quoting the game's own log: "The lookout: lost to sight: the
   sail. Night falls. The evening song: Heorot: Into the chest: a text, from Torhtred. The crew's heart rises. Three
   kinds of silver: Tofi thinks better of you (easy). Bernric thinks worse of you (cool)" and "This makes no sense
   though"). They are right. The log glues a card's title to the engine's notes of what changed, with colons, and
   prints inner words (easy, cool) in brackets. TO WHICHEVER BUILDER OF THE QUEUE READS THIS FIRST AND TOUCHES CARDS
   OR THE LOG (part 3, 4 or 6): mend it.
   - A line of the log is one plain sentence a person could say, in the past or the present, with who did what:
     "The lookout has lost sight of the sail." "At the evening song Torhtred sang of Heorot. You have it in the chest
     now, and the crew are in better heart." "Tofi thinks better of you for it. Bernric thinks worse."
   - When a card is answered, the log prints the card's own written outcome (the content branch has one for every
     first-release choice, under three namings that must be read as one: Design/content/changes on branch content),
     then what changed, in words: never a bare title and a colon, never an id, never a number or a word in brackets
     that the engine uses inside (easy, cool, +5).
   - Changes of feeling are said as the person would show them, and gathered: one sentence for a card, not one line
     for each person.
   - Things the lookout, the weather and the clock say are sentences too: who, what, where ("to the south-east").
   - Test: no line of the log in a play-through holds two colons, a bracket, or an id.
   Where it is made: FrameLog and the notices it is fed from (Assets/Kapman/Code/Game/UI/Frame/FrameGame.cs, Alerts),
   the effects' own words (Core/Encounters/Effects.cs), SeaWatch's lines.
40. **The player may found the far settlements early** (the user, 7 Oct): "it'll be an in game thing to have the
   opportunity to kick start the settlement in those regions, if a player wants they should be able to just make it
   their mission to speed run Iceland settlement or Greenland or Vinland at difficulty." So Iceland, Greenland and
   Vinland are not locked behind their dates (874, 986, about 1000). The lands are there from 793, empty and unknown.
   A player who makes it their aim can find them and plant a settlement long before history did, and it is meant to
   be hard. If the player does nothing, history settles them at its own dates.
   The lead's sketch of what "at difficulty" is made of (to be designed with the settlements of Design/living_world.md,
   after the alpha):
   - KNOWING: nobody can sail to a place nobody has told of. The way is pieced together from word (Irish hermits knew
     an island in the north in the 790s: the data's own "thule_795" hour), from being blown off course, from a first
     landfall that is then lost again. A course is only as good as who gave it.
   - A SHIP AND A SEASON: an open boat of four cannot do it. It takes a sea-going hull, the right weeks of the year,
     and a crossing with no shore to follow, where a gale or fog is the whole danger.
   - PEOPLE AND STOCK: settlers who will go (and a reason for them: land, a feud, a faith), beasts, seed, iron,
     timber where there is none. Carrying them is the trade.
   - THE FIRST WINTERS: a settlement lives or dies in its first years by what the player brings back. It can fail.
   - WHAT IT CHANGES: a new port that grows, standing there that nobody else has, a line in the chronicle that history
     never wrote, and later events that now find the place already settled.
   It is a late aim: it wants the fellowship and more than one ship.
41. **Most ports are plain** (the user, 7 Oct): "not every single port needs a lot of content, they can be simple,
   another place to rest and trade etc, so we can add far more that way." So there are two kinds of port:
   - A PLAIN PORT: a name (then and now), a place on the chart joined to the lanes, its realm (which gives the toll and
     the tongue), its kind and size, and a market (a market's goods already come from its region, so trade works with
     no more data). She can moor, trade, rest, buy bread, mend, hear the common word. One wiki page. The shared
     stand-in pictures. No people of its own, no sites, no cards of its own; the cards that fit any port still come.
   - A FULL PORT: all that, and its own sites, people, stories and cards (the fifty there are today).
   A plain port can be made full later without anything changing for the player's saved game. With this the world can
   hold several hundred ports: the rows for many are already written in Design/content/later_ports/*.md. What each
   new port truly costs is its join to the lanes (a bake step) and a check that it can be reached.
42. **A plain port still has people** (the user, 7 Oct, to item 41): "maybe key people and NPCs you can meet at these
   locations doing their own trades tho no?" Yes. A plain port has no hand-written sites, stories or cards, but it is
   not empty:
   - ITS OFFICES are filled by people made by rule, of that port's own people and tongue, each with a name from that
     people's names and a greeting in their tongue: who takes the toll, who keeps the church or the holy place, who
     sells ale, who mends boats. The same person every visit, remembered by the game.
   - THE TRADERS WHO ARE THERE: the world's other ships really sail; when one lies in this port its shipmaster is in
     People here, doing his own trade. He can be talked to, asked for word of prices where he has been, hailed again
     at sea, and he remembers the player.
   - KEY PEOPLE OF HISTORY where and when the record puts them there (a king at his seat, a bishop at his see), from
     the people of weight (Data/story/weight_793.json and its like for later years).
   Nothing of this needs writing by hand for each port.
43. **The world's people go about their own business, as in Mount and Blade** (the user, 7 Oct): "in mount and blade
   they have NPCs just running around with their own objectives and goals etc? Bit like that." So the people of the
   world are not waiting in ports for the player. Each who matters has an aim of their own and moves to follow it,
   whether the player is near or not; the player sees those in sight on the chart and in the ship view, hears of the
   rest by word, and meets them where they truly are.
   - WHAT IS THERE ALREADY: the world's 138 ships sail with purses, wages, beliefs about prices and bankruptcies;
     raiders and the king's guard boats exist as systems (Assets/Kapman/Code/Game/Sim).
   - WHAT THIS ADDS: a named person aboard each ship that matters (a shipmaster with a people, a home, a temper and a
     memory of the player); aims beyond buying and selling: a rival building a house, a raider choosing where to
     strike, a bishop going round his see, pilgrims bound for a shrine, a lord's man gathering dues, the creditor's
     man looking for the player (item 33), great people moving between their seats by the season (the calendar data
     already has where the king kept Christmas and Easter each year); and their aims changing what happens: prices,
     who holds an office, where it is safe.
   - HOW THE PLAYER TOUCHES IT: meet, hail, follow, trade with, warn, hire, betray; be asked for help with their aim;
     be remembered for it.
   In the alpha: the traders that exist are shown as people who can be met (item 42). The rest is after it.
44. **Nobody in the world cheats, and anyone can fail or grow rich** (the user, 7 Oct): "make it so the ships/NPCs
   aren't cheating and can fail or succeed, so some games an NPC rival does really bad or sometimes they become
   extremely rich you know?"
   - THE SAME RULES FOR ALL: every other trader buys and sells in the same markets by the same dealing rule as the
     player, pays the same tolls, wages and bread, sails by the same wind, runs the same risk in a gale or from
     raiders, and KNOWS ONLY WHAT IT HAS SEEN OR BEEN TOLD, as old as the word is. No trader is given silver, goods,
     speed or knowledge from nowhere. (Much of this holds today: Trade/Deal.cs is shared, the traders' beliefs go
     stale, they go broke and are lost at sea.) Prove it with a test: over a played year every penny a trader has came
     from a deal, and every price it acts on came from a port it called at or a ship it met.
   - THEY CAN FAIL: a bad run ruins a trader; his boat is sold, and he may turn up as crew, a debtor, or not at all.
   - THEY CAN GROW: a trader who does well may do what the player may do: a better boat, a second boat, a partner, a
     store ashore, a fellowship. Nothing the player cannot also do.
   - EVERY GAME DIFFERS: a new game rolls its own seed, so who rises and who falls is not the same twice. In one game a
     named rival is ruined by his second summer; in another he is the richest man on the Rhine.
   - THE PLAYER CAN SEE IT, by word: who is doing well, whose boat was lost, who is buying up wine (The field, the
     chronicle, talk in port), never by a list the skipper could not know.
   In the alpha: the first point as a test on the traders that exist, and the seed rolled on a new game. Growing into
   houses and fellowships comes with the player's own (Design/fellowship.md).
45. **Things to strive for, for the player and for everyone else** (the user, 7 Oct): "I guess the player and NPCs need
   something more to strive for, more equipped crew, better and more ships, maybe a homestead to hoard gold."
   Three ladders, and the world's other traders climb the same ones by the same rules (item 44):
   - THE CREW: more men and better: a steersman who knows a coast, a man who speaks a tongue, a fighter; and their
     gear (sea cloaks, spears and shields, a spare sail, tools), each of which does something that can be felt: fewer
     days lost to cold, raiders who think twice, a mend at sea.
   - THE SHIP: fittings on the boat she has, then a better boat, then more than one (the fellowship). Much is written
     already and waits to be used: Data/story/ship_upgrades_793.json, ships_793.json and ship_classes_793.json,
     Design/ships_roster.md, ship_upgrades.md.
   - THE HOMESTEAD: a house and yard at the home port that is the player's own: where silver is kept (carried silver
     can be lost with the boat or taken by raiders; a hoard at home, even buried, is safer but can be found or burned),
     where goods wait for a better price, where kin and the winter are, and which grows: a store, a slip, a hall. It
     is what an heir inherits. Written already: Data/story/works_793.json, unlocks_793.json (plots, charters, factors),
     hoards_793.json, Design/works_and_upgrades.md, scale_to_fleets_plan.md part 5.
   What the player sees of others: a rival's new sail, his second boat, his hall going up at Witla.
   In the alpha: a few true things to buy for the crew and the boat with an effect each, so silver has a use beyond
   the debt. The homestead and the ladders in full come after it.
46. **The 3D view keeps its own pace; only the world's changes come quicker** (the user, 7 Oct, on the five-speed
   clock): "if we speed up the 3D view it'll just look crazy, I think the speed of the 3D view will remain the same
   physically, but the water / weather and day night cycle will just happen quicker dependent on the location at the
   time." So at every clock speed the boat, the oars, the waves and the wake move as they would at speed 1. What
   follows the game's clock is the state of things: the hour and so the light and the sky, the weather where she now
   is, how rough the sea is, whether she is on a river, at sea or at a jetty. At speed 5 a day's light passes in a few
   seconds over a boat that still rows at an oarsman's pace. Changes are eased so the picture never jumps.
   (This is how the view was built: the stage runs on real seconds and reads the game's date, hour, place and weather.)
47. **THE BEGINNING OF THE GAME, in the user's own words** (7 Oct, evening; built the same evening: Showtime.cs, test
   OpeningTests, pictures Captures/chart/open_*.png). Every word said in it is a placeholder for the user's own.
   - "A slow start like a loading psx, maybe have a rotating ship at the bottom right as a fake loader": black, the
     purple comes up slowly, a small boat turns in a little window at the bottom right over the word Loading. Nothing
     is loading. Any key goes straight to the title.
   - "The PSX style filter from those screenshots earlier but just the colour purple, with the title of the game in
     big bold text that appears and fades away": a backdrop of slow purple bands in five tones of Byzantium with a
     console's dots between; the game's name big and bold; the title's menu under it. On New game the name fades.
   - "Then the text box slides up from the bottom slowly to the middle of the screen and it expands, not slow but not
     fast, into the text rectangle, then it asks '...', next 'And, from where do you hail?', you select your
     culture. Next is 'And your name?', you then type your name or generate a name based on a list of time accurate
     names for that culture. Then another text for religion." The questions as built: "..." / "And from where do you
     hail?" (your people) / "And your name?" (typed, or given: Resources/names_793.json, written from memory and to be
     checked name by name) / "And by what do you swear?" (your faith: he needs your oath for the debt) / "And what were
     you, before this?" (your past; the user, asked: keep it, he asks it).
   - "Then it's revealed that it's the debtor, a cold aloof uninterested man, just cares about getting what he's
     owed. Whatever the frisian word for loanshark is, then that's his title. He then explains the deal and the
     terms" and praises a boat that is not worth it ("he screwed you royally with this deal, the ship is crappy and
     dilapidated and cost way too much").
     WORDS CORRECTED at the user's ask ("check my terminology and correct it where needed"): the man you owe is the
     CREDITOR, or the lender; the debtor is you. "Loan shark" is a word of the nineteenth century; the period's is
     usurer. His byname as built is "the Wokerer" with a long o, from Old Frisian woker, usury (the Dutch woeker):
     NOT YET CHECKED against a dictionary, and so a placeholder. His name as built, Wursing, is a placeholder too.
     A point of history that serves the story: in 793 lending at interest was forbidden to Christians, laymen
     included (the Admonitio generalis of 789), so he does not lend silver at interest. He sells a worn boat on a
     tally at twice its worth. That is the debt the game already has.
     WHO HE IS (the user asked: "who would this be most likely in a situation where he's loaning someone like this"):
     most likely an older, richer merchant and shipowner of the player's own port. So he is his own man, not the
     count's. A game begun through the opening carries the flag lender_opening: its debt is owed to him wherever the
     game says who is owed (Trade.CreditorSaid), the casks aboard are his, and the count's chain of cards
     (house_debt) sleeps in that game. That chain's future (rewritten to him, or kept for another start) is the
     user's to say.
   - "The player is then taken to the 3d view of the ship and just the ship, a rotating floating 3D model of the ship
     rotating like the old psx demos, no water, just the pink void": ShipStage.Showroom.
   - "Then you click past it and you're seemingly free to click around, but as you click something then you're
     interrupted with multiple !!! appearing. Then another text conversation with Ravn. Ravn is the loveable first
     crew mate with relatively bad stats but will be your guide for the most part and everyone's first crewmate. He
     will explain the game mechanics at this point, and explain the first objective, to gather up crew mates to set
     sail for the first voyage, maybe then discover that the sail is in poor condition and needs to be mended before
     they can set sail. He is also the info on the first good trade and makes a really good suggestion on what to buy
     and where to take it."
     As built: the first click or key of the game is answered by seven marks and then Ravn, in a floating window. He
     says who he is, what must be done, how the screen is worked, and one buy worked out from the markets as they
     stand that morning (Showtime.FirstTip: a good the home market sells that she can hold and pay for, and the
     nearest port that pays most over it).
   - THE FIRST JOBS ARE REAL RULES (the user, asked: "Real, simple"): FirstJobs.cs. She begins with one man, Ravn,
     and a worn sail. Hands are hired on the home quay (People here: four pennies of hand money a man) until she has
     four; the sail is mended at the port (Port: twelve pennies and six hours of the clock). Cast off refuses, in
     words, until both are done. The men hired are a count, as the crew is everywhere today: Ravn's "bad stats" and
     the crew as people wait for the crew system.
   - The show is off in a batch run, where the old creation window still asks the three choices: the other play tests
     are as they were.
48. **TALKING TEXT** (the user, 7 Oct: "a Pokemon style text reveal ... how it appears like they're actually talking
   but if you press a key it skips to the next set of dialogue"): TextReveal.cs. Wherever somebody speaks (a card, a
   talk, the opening) the lines come a letter at a time, with a breath at a comma and a longer one at a full stop,
   one line after another. Enter, or a click on the words, finishes the line being said and starts the next. A number
   key still picks a choice at any time. The game's own lists (the lookout, the creation window) stand whole.
49. **CUTS AND CARDS**: a cut through black for New game and Continue; a place's name and the day across the screen
   on the first morning and each time she comes in somewhere; a new floating window eases in.
50. **MUSIC, FOR NOW** (the user, 7 Oct: "add this song to start playing softly as soon as the game is launched";
   four tracks cut from an EP, "add them as OST for now"; "play them whenever you think they should be played with
   appropriate breaks like minecraft"): GameMusic.cs plays files from a folder BESIDE the game, Music/title.mp3 on
   the title and through the opening, Music/ost/* in the game: one at a time, never cut short, a quiet of four to
   nine minutes after each, the main one on the first morning, a farewell at casting off, a morning one at first
   light, a long one at sea, by the words in the files' names.
   THESE TRACKS ARE PUBLISHED RECORDINGS (Fan-Hsiu Kadesch, "Ewige Blumenkraft", 1993; Gilgareth, "Hinab in die
   Zwergenbinge", 2024) and the user's own stand-ins. The folder is in no commit and no build. They cannot ship
   without their owners' leave; the rule for sound that ships stays public domain or CC0 recordings only.
51. **NOTES WHILE PLAYING** (the user, 7 Oct: "create a mode where I can edit and note changes to be made on the go
   while playing"): F2 in the game. PlayNotes.cs; the notes are in Notes/ beside the game (not committed).
52. **THE TITLE IS A SEA** (the user, 7 Oct, late): "make the loading screen a top down of a stormy ocean with rain,
   with the title of the game animated in the middle, and it's click anywhere to play but it doesn't say that, just
   the title, then a cool animation where all the letters disappear, then it fades into the text"; and "let it cycle
   forever through all possible weather conditions for as long as the game isn't started".
   As built: after the fake loader the title is the 3D stage's sea seen from almost straight above (ShipStage.TitleSea),
   in the look of purple tones (ink3), with nothing on it; the name's letters rise in one by one and ride it, each
   with its own bob and roll. Nothing else is on the screen. A click anywhere or a key plays: the letters heel over
   and go under, one after another in no order; the sea fades into the purple, and the lender's box comes up. The
   weather goes round for ever, each for about half a minute and passing into the next: a gale with rain, the rain
   passing, light airs, fog, a rising wind and whitecaps, snow on a hard wind (ShipStage.TitleWeather).
   DECIDED BY ME, to be overruled: where there are saved games, the click brings the menu up instead (Continue stands
   first), so a game is never begun over one by a stray click; and Esc on the title brings the menu up or puts it
   away, because the sound's setting and Quit live there.
   KNOWN: the rain is drawn for a camera that looks along the water, so from above it falls mostly in the upper part
   of the picture; the weathers differ less in the picture than their names do; the light of the title's sea is its
   own, laid low, because under a high Sun every facet of the console's sea throws its glint straight up and the
   picture is one white sheet.
53. **THE MARKS SHAKE** (the user: "make the !!! animated and shakey like old nintendo games"): seven of them, bigger;
   they pop in one after another, then shake where they stand in steps (sixteen a second: a jump to a new place and a
   new lean, a hop now and then), and blink out.
54. **IN PORT THE PORT'S PICTURE STANDS IN THE CHART'S PLACE** (the user: "when you're at a port, instead of a map it
   just shows an artwork of the current port; if you can make a simple placeholder for each one then I'll redo it
   later myself; just a top down angle looking at the place, not animated except for lighting; I want it simple psx
   graphics, 3D with 2D elements as it's just looking at one angle").
   As built: a new kind of art slot, port_view (640 x 360, one for each of the fifty ports, in the art list and the
   Kapman Art folder with its description; the port's first-release ones are in the tier of things to draw early).
   The placeholder is made by rule from the port's kind, size, landing, country and faith: the water at the foot, the
   landing and its boats, the houses as roofs with a strip of wall under each, the one great building, lanes, fields
   and wood. PortView.cs shows the picture behind the frame while she lies at the port and lays the hour over it as
   a tint by the Sun's height there (day as drawn, warm, rose, blue, dark blue). Nothing in it moves.
   DECIDED BY ME, to be overruled: a small button at the top, Chart, puts the chart back while she lies there (and
   The port puts the picture back), because the chart is still wanted for planning; a course is set in the Voyage
   window either way. Not shown while the ship's 3D view has the middle of the screen. Off in a batch run with the
   rest of the show.
   For later: lit windows and fires at night; the picture as a small 3D model with flat pictures stood in it, as the
   user described the look, when the user makes them.
55. **THE FRAME IN THE LOOK OF THE WINDOWS XP YEARS, IN BYZANTIUM** (the user: "make all ui stylised winxp era type but
   make it the colour byzantium"; "the soft edges are bad"; of the first mock-up: "connect the entire left side into
   one piece all the way around the entire frame of the screen, not so many X's in the top right of them all"; of
   the second: "Yes, but thinner sides"; of the first build in the game, with the empty block down the right struck
   out in red: "Can you remove what I highlighted part of the UI").
   As built: one frame all the way round the screen: the bar across the top (Menu as its gold first button, the
   date, the silver, the bread, the debt, the clock's buttons), a side down the left a quarter of the screen wide
   with the eight orders and the windows in it as sections, a foot with the log, and a thin edge down the right. The
   portrait and the boat stand over the world in its right corners; there is no block down the right. Hard corners,
   thin dark lines, pale bodies with dark words, bars and buttons shaded from top to bottom (six small pictures in
   Game/UI/Xp). A section has no cross (N and X still open and close them); only a window that floats has a bar with
   its name and a cross. The menu and the chart's port names are dressed the same way. The typeface is DejaVu Sans
   Condensed for now (see 58).
   How: the old dress stays in Frame.uss and the new one is a block at its foot that wins; the code only gained the
   frame's own pieces (frame-pane, frame-viewport), a class on the Menu button and on a section's cross, and the bar
   of a floating window.
56. **A PARAGRAPH AT ANY ONE TIME** (the user: "you need to give far less text at one time, a paragraph at any one
   time"). A window somebody speaks in, and every card, shows one paragraph: it is said a letter at a time; Enter or
   a click finishes it, and it then stands and waits; Enter or a click again and the next takes its place. The
   choices come up with the last paragraph, and the number keys do nothing before that. The line under the window
   says "Enter, or a click: go on." while there is more. (TextReveal.OneAtATime; off in a batch run with the rest of
   the show, where the whole text stands for the tests to read.)
   NOT DONE: the windows of the game's own that have no speaker (a ship met at sea: what she is, her facts, what
   came of it) still stand whole; they are two or three short lines.
57. **THE NAME IS A THING, AFLOAT ON THE TITLE'S SEA** (the user: "can you 3d the Title text and have it float on the
   water"). Each letter's picture (title/title.letter.N.png: the user's to redraw) is made into a slab as the game
   starts: its painted face up, a side wherever the picture has an edge, in that edge's own colour. The six lie
   afloat in a row and each rides the water under it: lifted and dropped by the swell and leaning with its slope.
   They come up from under one after another, and at the click they turn over and go down. The flat letters are kept
   for a machine or a look where the stage is not up.
   With it: the title's look is the console one (it keeps colours), its sea is painted in the game's purple, the
   camera no longer goes round (the name has to be read), and the light stands behind the camera.
   TRIED AND DROPPED: the letters stood upright in the water. The tail of the p went under and the name read
   "Kanman".
   THE NAME'S COLOUR (the user: "change the white colour for the 297036 hex color"; and, of the first cut, which was
   washed out toward white: "You didn't change the white"): the letter's body is the green 297036 itself; only its
   head, its lit edge and its worn places are paler.
58. **A TYPEFACE OF THE USER'S, HEARTH** (the user: "ive also added a text font called hearth"; "if you can use that
   with everything other than the title screen"; "i might have to open unity and add it to the project to use").
   NOT DONE: no file of that name is on the machine yet (looked for on every drive, 7 Oct). When it is in the
   project it takes the place of DejaVu Sans Condensed in the five stylesheets (Frame, MainMenu, MapView,
   MarketPanel, TextPanel); the title keeps the letters it has. Its licence is to be read and its credit line
   recorded before it ships.
59. **THE FRAME'S COLOURS: THE GREEN 297036 FOR EVERY PALE GROUND, BYZANTIUM FOR EVERY WORD** (the user: "change the
   white colour and replace it with 297036"; "the white as in the UI color, not the font"; "the font colour Byzantium
   and the background is the other colour I said before"). The green is faded (a body 168 191 159, a row 203 213 189):
   at its full strength it is as dark as Byzantium and no word of that colour can be read on it. The bars stay
   Byzantium with white words. DECIDED BY ME, to be overruled: how far it is faded.
60. **THE TALL FOOT** (by the picker, after "they're a good addition just not well placed, neither are the panels"):
   nothing stands on the right; the portrait, the log and the boat's view stand side by side in one tall foot; the
   world has the whole width over it. Chart or Port, Swap and Look are buttons of the top bar. Texture and depth on
   every ground ("texture the UI colour so it's not so sleek and glossy"; "more flair, more depth, more style"): a
   grain over each, bars lit in dots, panels and buttons standing up, rows let in, a gold line and corner pieces
   round the world.
61. **THE TITLE FROM STRAIGHT ABOVE** ("maybe change the camera angle on the title screen to be looking directly down"),
   as bright as the picture of that evening the user called perfect ("this earlier picture brightness was perfect");
   the name's letters stay the faded green of that picture (the full green was a misreading of mine).
62. **WHAT IS SAID STANDS ONCE, IN THE FOOT** ("why is he speaking twice, once in the middle and once at the bottom;
   expand the bottom one for more wide text space for characters, add their personality type / stats, and when
   it's just focused back on the player's character just show name, stats, culture, religion"). A card, a talk or an
   alert is docked over the portrait's place and the log: who speaks with who they are, the words, the choices at
   the right when they come. NOTHING IS EXPLAINED UNDER A WINDOW ("remove the 'Enter, or a click, to go on', you
   don't have to explain everything like this"). People have no figures of their own yet: their people, their faith
   and their nature stand there, and figures when they have them.
63. **REAL PICTURES AS STAND-INS, NOT DRAWN ONES** ("use this as placeholder for Dorestad's view ... instead of AI drawn
   stuff"; "just apply a Byzantium / PSX filter to it"; "do the same for all these and then exchange them for all
   NPC portraits"; "use that second one for Alfred"; "filter these and use where fits"). Design/art_standins.md has
   every one with what may never ship. The searching of Wikimedia was called off by the user the same evening
   ("maybe forget about searching for images, you don't seem to be very good at it"; "just tell me what you need and
   I'll find it"): the user sends pictures, I filter and place them.
64. **THE BUTTONS: THREE ROWS OF FOUR** (by the picker, after "where's inventory button? what is offer? why is measure up
   there ... fix the button functions, what even is happening up there"): the ship (Cast off, Voyage, Crew, Hold);
   where she is (in port: Trade, Alehouse, Minster or Grove or Shrine by the port's own holy place, Rest; at sea:
   Look out, Sound, Hail, Rest); what is known (Wares, Journey, Errands, Book). The words are period-rooted English
   (by the picker). A button that names a window brings it up; one that names a place goes into it. Wait went into
   Rest; Offer into the holy place (its rule is still not in the game); Measure down to the chart's tools.
   THE PLACES (by the picker: the picture in the world's frame, the choices in the box at the foot; the pictures are
   the user's to send: scene/place.alehouse.png, place.minster.png, place.grove.png, place.camp.png): the alehouse
   is the port's own tavern or lodging of the places' files, with the shipmen for hire, the people found there and
   the other masters' talk (news and prices into the ledger); the holy place is its minster, abbey, church, grove or
   shrine; Rest is the fire on the bank or a watch below, with sleep till first light and the wait for a wind.
   NOT DONE: what each crewman says at the fire (the crew are a count, not people); offerings; a crew for hire
   after the first jobs.
65. **WARES, JOURNEY, THE BOOK, AND THE CHART'S READINGS.** Wares ("a market button with all known things about all
   known products ... you learn it and it gets added here, then you can further investigate"): every good seen sold,
   heard priced or carried, each with where it is wanted and where it is sold by the merchant's own notes. Journey
   ("a record of all interactions and knowledge gained"): what was learned (markets, people, places, news) and the
   days' log. The Book: the wiki's 2,600 baked pages, open by their own rules or when come upon. The chart's tools
   stand at the bottom right by the boat's view: Measure, and the readings Land, Realms, Faith, Peoples ("add alt
   map modes like borders, religion, culture maps"): for now they colour the ports' names, with a key; the land has
   no borders drawn yet.
   NOT DONE: the linked words all over the screen and the box under the pointer (next); conversations boiled down
   into the Journey.
66. **THE PLAYER'S OWN FACE IS CHOSEN FIRST** ("you gotta replace the AI image of the player and make it a choice at the
   very start out of all the choices"): every face of the pool in rows, before the first question. WURSING HAS A
   VOICE ("add mannerisms, and text quirks, don't make him sound so AI"): he counts while he talks and loses the
   count. "HAND" for a crewman is a word of the 1600s (the user asked): shipman, shipmen, hiring silver. MOORED, THE
   BOAT IS SEEN FROM THE LAND looking out over the water ("make it so it's looking away from the port").
   STATE: items 64 to 66 were tested in a second copy of the project (Kapman2_ship: the user's editor had the first
   one open) and committed on 7 Oct, night (b3e959d).
67. **ONE WINDOW ON THE SIDE** (the user, 7 Oct, night, after playing: "the text is far too crowded on the side so maybe
   just one panel"). The side holds one window, and the twelve buttons over it say which. At sea she comes up on
   Voyage, in port on Port. No bar of workspaces, nothing tiled (FrameWindows.OneWindow; the tiled windows are kept
   for their tests and for a day they are wanted again).
68. **SHE KEEPS TO HER CHARTED LINE** (the user: "the ship sailing is extremely buggy and the little map indicator just
   goes over land"). On a river she cut straight over the land between its bends: the planner passed a point of the
   line from 900 m off and looked for her leg among the next sixty, and a river's lane has a point every 300 m. On
   the chart she now steers for a point of the line itself, 300 m ahead of her own place on it, and does not slow at
   every point (Course.Aim, ChartBody.Through).
69. **THE MAP MUST NOT STAND STILL, AND THE FOG MUST LIVE** (the user: "the map also is extremely buggy with the map not
   updating on the fly fast enough, nor is the fog of war interactive, it's just static"). The chart's tiles (4096
   pixels a side) were read, unpacked and recoloured on the main thread, a tile a frame, the game stood still for
   each: two threads of their own read them now (MapPng, tested pixel for pixel against the engine's own reading) and
   the main thread only puts a finished tile on the card. The fog has three states: never seen; known but out of
   sight (a thin veil); in sight now, a clear round of 30 km that goes with her each game minute.
70. **ZOOMED ALL THE WAY IN, THE SHIP ON HER WATER** (the user: "if you zoom in all the way you can see a boat sailing on
   a simple simulated water animation"). Scrolling on in past the chart's closest brings the ship's own 3D view into
   the middle, from above; scrolling out over it gives the chart back.
   DECIDED BY ME, to be overruled: that it is the 3D view (whose sea is worked out from the wind where she is) and
   not a flat picture drawn on the chart itself.
71. **HEARTH** (the user: "i added the new text"). Every text but the title's name is in Hearth (Assets/HearthPixelFont,
   the user's pack), a pixel face of 8: so the frame's sizes at 1080p are 16 and 24 (they were 18, 22, 26, 30), where
   its pixels are whole. 16 of it stands as high as 19 of the face before. It has 161 letters and the game writes 210
   kinds it has not (thorn and eth some 4,000 times each, the long vowels, Cyrillic, runes): those are drawn from the
   old face behind it (FrameFonts). No bold: a name is told by its size and colour.
   TO DO: the pack's licence (the Asset Store's) and credit line into the credits list.
72. **KAPMAN, THE NEW PROJECT: RULED** (the user, 7 Oct, night: "Build what you can, start on the new project, move over
   to everything visual scripting etc., built like an actual Unity project. I'm happy with the vision we've made so
   let's move forward"). By the picker: the screens are UI Builder files (UXML); visual scripting goes as far as
   the rules themselves ("Rules as graphs too"), not only the flows; it lives at G:\KAPMAN (G:\kapman_unity\KAPMAN
   is the first build's own folder under another case) and starts from Kapman2 as it stands when tonight's fixes
   are committed; Kapman2 is then the archive. Design/KAPMAN_PLAN.md holds the plan as ruled.
