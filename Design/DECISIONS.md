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
73. **THE CHART HAS PAPER** (the user, 8 Oct: "Can you give the map more texture on the colours so they're not so hard").
   One clear tile of specks, soft patches in an ordered dither and fibres lies over all the colours of the chart and
   goes with the land (Game/UI/Map/paper.png, made by Bake/art/make_map_textures.py); and a picture of the chart shown
   smaller than it is is drawn smoothed, not every fourth pixel alone.
74. **RAVN POINTS AT WHAT HE SPEAKS OF** (the user, 8 Oct: "When Ravn is explaining what to do and how to play can you
   highlight what he's talking about"; and of its colour: "that green tinge colour we had earlier"). A line of a talk
   carries marks, [[>names]], naming parts of the frame (their names in Frame.uxml); as the words come the frame's
   pointer stands round those parts, in the 297036 green. Any talk can use it (FloatSpec.Say).
75. **THE MARKS OF THE FIRST MORNING** (the user, 8 Oct: "make them the same colour and style as the title screen";
   "they start out like that and need to travel back down to Ravn's portrait"). The "!!!" are a picture painted as the
   name's letters are (title/title.mark.png, a slot of the art list); after they have shaken they fly down to where
   Ravn's face is about to stand, and then he speaks.
76. **THE SAIL, IN THE USER'S WORDS**: "Sails tattered.... / Needs to be repaired." and the cost next to it (the cost
   is the button).
77. **THE COURSE IN SPACED STROKES** (the user, 8 Oct: "make the map trail for the ship icon to be spaced lines like
   -  -  -").
78. **THE TOP BAR** (the user, 8 Oct: "replace the word silver with a period accurate PNG of a coin, use period
   accurate date keeping. Replace the bread with a pic of a barrel from the assets we have and next to it 'supplies'
   or another word that works better and it just says the days left").
   - The coin: a silver denier of Charlemagne struck at Dorestad before the new coinage of 793/4, the side that
     names the town (Bibliotheque nationale de France, public domain).
   - The date: a day is named by its feast, else reckoned by the Kalends, Nones and Ides, the Roman way every clerk
     used (PeriodDate): 1 May 793 is "Kalends of May, 793", 2 May "vi Nones of May". The plain date is in the tip.
     The debt's day is said by its name alone ("by Michaelmas"). The log and the windows still say "2 May 793".
   - The barrel: the Vikings pack's own, drawn by the editor; the word is "stores"; then the days.
   DECIDED BY ME, to be overruled: the word "stores"; lower-case Roman numbers for the count ("vi Nones"); the year
   in plain figures; that only the bar changes for now.
79. **THE BUTTONS' SIGNS ARE REAL THINGS** (the user, 8 Oct: "replace the button icons with wiki searched stuff you
   find yourself not made yourself"). Sixteen pictures from Wikimedia Commons, each one colour of ink, 32 pixels,
   tinted by the style sheet: a ship under sail (Cast off), the vane of Soderala (Voyage), a Torslunda fighting man
   (Crew), the Mastermyr chest (Hold), a balance (Trade), a Tating jug (Alehouse), St Cuthbert's cross (Minster), a
   fire steel (Rest), the Sutton Hoo helmet (Look out), a lead and line (Sound), an ivory horn (Hail), a Frisian pot
   (Wares), a wax tablet (Journey), tally sticks (Errands), the St Cuthbert Gospel (Book), dividers (Measure).
   Sources, authors and licences: Design/art_standins.md. They are stand-ins: the user's own drawing of the same
   name in Art/final/hud wins. Not all are of 793 (the vane and the horn are of about 1000 to 1100, the tablet a
   replica, the lead from a book of 1890); better ones can be put in by changing one line of
   Bake/art/standin_icon_choices.json.
80. **KAPMAN: WHERE IT STANDS** is the table at the foot of Design/KAPMAN_PLAN.md. Done on 8 Oct: the frame, its
   repeated parts, the menu and the opening's screens are UI Builder files; four rules of the merchant's are graphs
   (the toll, the crew's pay, health and heart), each held to the code's sum by a test; the opening is a flow in a
   graph on the Showtime object.
81. **THE FIRST TEN MINUTES** (the user, 8 Oct, by the picker, out of the critic's list of that day,
   Design/critique/critic_playthrough_2026-10-08.md: "First ten minutes"). Five things, items 82 to 86. The rest of
   the critic's list is not ruled on.
82. **BEFORE SHE SAILS: A LIST THAT TICKS OFF** (the critic's B1: "after Ravn stops, nothing on screen holds what he
   said"). From the first morning until she first casts off, a list stands under the buttons: the sail (the user's
   own words, and the cost as its button), the crew ("Crew: 1 of 4", a button to the alehouse), a cargo (Ravn's word
   on what to buy and where to take it, a button to Trade), a course (a button to Voyage), cast off (what she still
   wants, then the button itself). A row done is ticked and shrinks to one line. It is laid out in Frame.uxml
   ("jobs"); the rules are FirstJobs (Listed, Tip, Wanting); the code is FrameRoot.Jobs.cs. Ravn's last words point
   at it row by row. The good and the port he named are tagged "Ravn's word" in the Trade and Voyage windows.
   The sail's line is no longer in the Port window, and "Before she sails" is no longer in Errands: said once.
   Every text sends the player to the alehouse for men (none says "People here" or "Look under Port" any more).
   DECIDED BY ME, to be overruled: that a cargo is a row though it never stops her; that the crew and the course
   rows have no small print; that the list goes when she casts off, not when she lands.
83. **WHY SHE MAY NOT CAST OFF IS WRITTEN UNDER THE BUTTONS** (the critic's B1: "Cast off is grey and no reason stands
   by it"). In a port, whenever Cast off is grey and the list is not there to say it, the reason is a line under the
   buttons ("rack-why" in Frame.uxml): the same words the button gives when the pointer rests on it.
84. **A LANDING IS MARKED** (the critic's B4: "the first voyage ends and nothing marks it"). When she makes fast after
   a passage the log says one framed line: "Dorestat to Traiectum, half a day. The purse holds 12 d. What she carries
   cost 50 d and would fetch about 122 d here. Owed 1920 d, with 150 days left." At the first landing of a game begun
   through the opening Ravn adds a line. The landings are kept (Landings; saved with the game) and listed in the
   Journey window under "Landings", each closed when she casts off again with what the stay came to in the purse
   ("Came with 12 d, left with 152 d").
   DECIDED BY ME, to be overruled: the critic asked for "Left with 86. Have 152." at the landing, but at the landing
   nothing is sold yet and the purse is at its lowest, so the slip says what the cargo cost and what it would fetch
   here, and the sum of the stay is told when it is over; her own bread and ale are not counted as cargo; Ravn
   speaks at the first landing only.
85. **THE WINE IS YOURS TO SELL, AND OWED FOR** (the critic's B3: "whose is the wine, and whose is the silver?"). The
   four casks came with the boat and are on the lender's stick with her, so they are the player's to sell and the
   silver is the player's to spend. Ravn says it once on the first morning. The sentence "what it fetched is his
   stake and not your gain" is gone from every sale; the hold says "Came with the boat. Yours to sell, and owed for
   already."; the log says "that came with the boat". What they fetch is still not told as gain.
86. **ONE RAVN** (the critic's D1: "there are two Ravns"; the user, 8 Oct, by the picker: "Warm, talks too much").
   The Ravn of the opening is the real one. His sheet (nature, voice, looks) and his lines on the quay at Dorestad
   (places/dorestad.json) are written again in that voice; his riddle is a thing he half remembers; "Go to your
   count" is gone. Placeholders for the user's pen. NOT DONE: his lines in the story chains and at other ports
   (about a hundred, in some fifty files under Game/Data) are still the dry Ravn's.
87. **ONE PANEL WITH TABS** (the user, 8 Oct, of the list as a panel of its own between the buttons and the window:
   "Maybe make it one panel with a tab system"; by the picker: "List is a tab"). This overrules "under the buttons"
   in item 82. The side is one panel. While the list stands, that panel's head is a bar of two tabs: "To do", which
   shows the list in the window, and the window's own name with its arrows. The list is the tab shown first, and the
   one shown while Ravn points at it; a button that brings a window up turns to that window's tab; a click on
   "To do" comes back. When she first casts off the tab goes and the head is as it always was. The tab is in
   Parts/Window.uxml ("todo", and "window-tab" round the name and arrows); the list is still laid out in Frame.uxml
   ("jobs") and the code moves it into the window (FrameRoot.Jobs.cs). With several windows on the side, which is
   not a player's layout, it stands over them as a strip.
   Also: the reason under the buttons (item 83) is shown only while the port's menu waits, not for the moment an
   errand or a talk greys the button.
   DECIDED BY ME, to be overruled: the name "To do"; that there are two tabs only, the other windows still being
   chosen by the buttons; that the two-tab head is there on the first morning only.
88. **THE PLAYER'S FACE WHEN NONE IS CHOSEN** (the user, 8 Oct, of a picture from a test game that skips the opening:
   "Why can I see the old ai generated portrait"; then, of the face I took from the pool by the people's name: "Use
   one of the portraits I gave the one you were using earlier was fine"). A game with no face chosen shows the FIRST
   face of the pool the opening offers (Art/final/portrait_pool, else the pictures the user gave in
   Art/standin/portrait_pool), which is the one the opening's pictures had shown. The drawn portrait of the people
   stands only on a machine with no pool at all (FrameArt.SelfUnchosen).
89. **RAVN'S LINES ON THE STORY CARDS, IN THE ONE VOICE** (the user, 8 Oct, asked whether I should write the rest of
   his lines again in the warm voice: "Yes"). This finishes item 86. His 75 spoken lines on the cards of eleven story
   files (the old gods, the storm vow, the riddle trail, the barrow, the dirham hoard, the coast watch of 810, Reric,
   the silver road, the pilgrim road, the sword smuggling, and the first question at Dorestad) are written again:
   every fact, name, number, piece of advice and quoted riddle or verse kept, the scenes as grave as they were, the
   man warmer and saying a little too much. Four narration texts and two sentences of the places that made him a
   quiet man by nature are turned by a word or two ("for once"). Placeholders for the user's pen, as all of it is.
   HOW IT WAS DONE: the session had ultracode on, so three writer agents, a checker for each (seven lines fixed), a
   reader of the places and one editor over all (one line changed) did the writing from a voice brief of mine; I
   read every line, trimmed one, and put them in by script (the scratchpad's ravn folder). The old and new lines
   side by side: Design/content/ravn_one_voice_2026-10-08.md. One line was not yet written and still is not
   (rhine_frisia.dorestad.godfred_is_dead.which).
   LEFT AS FOUND, and noted by the checkers: on the wine ending of the storm vow nothing goes into the tarn, yet
   his last line there speaks of a thing in the tarn; the barrow's cards 8 and 9 disagree on whether he has spoken
   of the player's word since; two sentences at Birka say he "knew the answers and would not say them", which sits
   oddly beside his having answered too much at Hleidr.
90. **RAVN LOOKS LIKE HIS PORTRAIT** (the user, 8 Oct, by the picker, asked whether he is as the stand-in portrait
   shows or as first written: "Like the portrait"). About forty-five, broad and weathered, a full dark beard going
   grey at the chin, thick hair tied back, a ready grin. I had changed his sheet to this in the morning without
   asking; now it is ruled. Brought into line: nine picture descriptions that drew him lean and grey-haired (in
   the writing and in Art/art_list.csv), his portrait's note in the art list, the Book's page, and the design
   spine (Design/content/spine.md and spine.json, with his nature and voice of item 86).
91. **THE SCRIPT CRITIC'S REPORT, APPLIED** (the user, 8 Oct, evening: "the script report is good apply suggestions
   and put it together"; by the picker, the scope: "Everything it names", done by me and not by agents). The report
   is Design/critique/script_critic_2026-10-08.md. Items 92 to 98 are what was done. NOT DONE, and why, is item 99.
92. **WURSING, WRITTEN AGAIN** (the user: "I think wursing needs a re-script"; by the picker, out of three: "Cold,
   and eating"). The user's first brief of 7 Oct for him: "a cold aloof uninterested man, just cares about getting
   what he's owed". He does not look up from his food, makes no joke, and calls a wreck a good boat. His questions
   are plain: "Where are you from?", "Your name?", "What do you swear by?", "And before this? What were you?" (two
   of the old ones were the user's own words of 7 Oct, changed on the report). The plate under the boat no longer
   repeats that she floats: that is Ravn's joke alone. All of it placeholders for the user's pen (Showtime.cs).
93. **HIS BYNAME, SAID ONCE** (the user, twice: "I don't know what that other word means"; by the picker: "Keep it,
   said once"). "The Wokerer" stays, and he says it once with what it means: "On the quay they call me the Wokerer.
   The usurer. I do not lend. I sell boats." CHECKED on 8 Oct: Old Frisian woker (gain, interest, usury) is a
   recorded word, beside Dutch woeker and German Wucher; the ending that makes it a man is the game's own. And his
   trade (the user: "is his role really illegal in this year?"): lending was lawful; taking any price for a loan
   was forbidden to laymen too from the assembly at Aachen in 789, as a rule of the Church with the king behind
   it. So he does not lend: he sells a worn boat on a tally for twice her worth, and the quay calls him the usurer
   all the same. The Book's page for the word says so.
94. **WHO IS OWED: WURSING ALONE** (the report's first change; told to the user as taken for settled, by the user's
   own opening). 1,920 pennies at Michaelmas, on his stick. The count Folcbert and his steward Hartwin no longer
   claim the boat (their "Speak of the debt" lines are gone, and the steward's 480 pennies with them); the widow,
   the rival, the alewife, the story of the stick at Dorestad, and the cards and ports that said "the count's day"
   say the Wokerer's. STILL ASLEEP in a game begun through the opening: the old chain of cards of the count's debt
   (house_debt), and what a count could grant (an errand in place of silver, a year's delay): those stories are
   worded for the Wokerer now but their rules were not looked at.
95. **THE OPENING, CUT** (the report's third change). Ravn's first morning is himself, the sail, the men, the wine,
   his one word on what to buy, and the list. Gone: the paragraph that read out the buttons and the clock ("Space
   stops it", from a Northman of 793), the saying over of the five jobs, and "Talks a great deal and means well"
   under his name (it is "A Northman" now). Prices are said in words ("twelve pennies", Spoken.cs), never "12 d".
96. **RAVN IS ONE MAN, WITH TWO GEARS** (the report's second change and its answer on the rural Ravn; the user had
   asked: "Would it be worth portraying him as somewhat rural, with wording like 'ere?", and said not to do it but
   to show it: Design/content/ravn_rural_trial_2026-10-08.md). No dropped h and no dialect words. At ease he
   shortens his words (I'll, don't, that's), the only one in the game who does, and runs on. In earnest (a grave, a
   vow, a storm, the last question) he says every word whole and says little. He loses "I only say", "I say it
   once" and telling the player what he is like. The old sayings and the riddles are his alone, given word for
   word. "The Dane" of the crew cards and "the old hand" of the sea cards ARE him (70 lines, written as his; the
   teller calls him Ravn). The Dane who asks to be hired is a stranger.
97. **WHO IS SPEAKING ON A CARD** (found while doing item 96, put to the user, who chose by the picker: "Fix the
   cards first"). Two faults the critic could not see, because the copy it read had each line labelled:
   - A card's speakers were made up afresh at every port by their trade, so the crew's steersman had another name
     at each quay and Ravn spoke under a stranger's. Now an actor of a card may be ONE OF THE STANDING CAST
     ("cast": "cast.crew.ravn", "name": "Ravn"): one person, the same on every card (NpcRegistry.Cast). The four
     of the crew are bound so on 370 actors: Ravn, Aldger the steersman, Brunger the mate, Beornwine the lookout
     (who is English at sea too now). Their portraits are their own.
   - A card showed every line under the face and name of its first speaker. Now each line carries who says it,
     and the face, the name and the nature in the talk window change as the lines come; the teller has no face
     (FloatSpec.voices). Anybody not of the cast has the portrait of their trade.
   OPEN, for the user: the three hands hired on the first morning have other names (FirstJobs makes them up),
   and then the cards speak of Aldger, Brunger and Beornwine. And the people of the ports are still made up
   afresh on cards (the moneyer of Dorestad is Sigimar in the port and "Madalger" on his card).
98. **THE LINES THE REPORT NAMES** (its "Fix these first", "Where a player is lost" and "Slips").
   - The ten lines to fix first, in the plainer words it gave or near them.
   - The words in brackets on choices are gone (195: "(sure)" told nothing, and the frame says the odds itself
     where there is a chance).
   - 31 first greetings in the ten home ports no longer ask a question the player has no way to answer.
   - The toll man says what he is looking for before he asks what she carries.
   - The riddle cards no longer say that the riddle is about what lies in front of the crew.
   - One name for a place in what is said and told: Dorestat (not Dorestad), Traiectum (not Utrecht), Ganda (not
     Ghent); "pennies" (not "pence"); "alehouse" (not "ale-house"). Only in the cards' words and the people's
     lines: the ports' own histories, sources and the Book keep the names their books use.
   - Slips: six oars, not four; half a day to Traiectum; the "worse" joke told once, not three times; one
     craftsman's line was another's; "the emperor" before there was one; memorial stones two hundred years early;
     Ravn sewing a sail he had said he could not sew.
99. **OF THE REPORT, NOT DONE**
   - THE HABITS (its heart: the dry last word on every line, one breath for everybody, one mark in many mouths,
     the teller's smirk, stock hellos and goodbyes, the footnote in a mouth, threes with a joke third). They are in
     about 400,000 words. The user chose "Everything it names" over "Also rewrite against the habits" (3 to 5
     million tokens of agents). They are rules for the user's own rewrite.
   - "Print the riddles and let the player choose the answer": the riddles are read and answered in the Chest, and
     a card's answer is open only when its riddle is solved there. The critic's copy did not show the Chest. Only
     the one unclear choice it named was reworded ("Say his pupil is standing behind you").
   - The great mound on the Deben can still be opened: that is the game's own marked change of history (the flag
     changed_sutton_hoo_opened, and three endings rest on it), not a slip.
   - Names used twice in different ports (four Gerbrands) and three Thiad- names at Dorestad: not changed. Kin did
     share a name's first half, and a renaming touches every story that names them.
   - The quoted verses keep "o'er" and "call'd": they are other people's translations, word for word.
100. **WHAT IS SAID STANDS APART FROM WHAT IS TOLD** (the user, 8 Oct, evening, of the talk window: "What they're saying
   and the description beneath it needs to be more clearly defined from each other its hard to tell what's actually
   dialogue"). In a talk window a line somebody says stands in a pale block with a bar of Byzantium down its left, in
   the full ink. What the teller tells (a line of narration on a card, and the description of the scene under the
   lines) is plain on the window's ground and paler, and the description is set off by a thin line. The player's own
   choice, told back after it is made, is plain too. (Frame.uss: .frame-float-line.spoken, .teller, .frame-float-small;
   the classes are given in FrameRoot.Floats.cs.) DECIDED BY ME, to be overruled: the block and the bar as the mark
   of speech, and that the description stays under what is said.
101. **WHAT CAME OF A CHOICE, AS IT IS WRITTEN** (8 Oct, night; the user: "Do what you think needs to be done";
   the alpha's row 7: "each choice has a written end"). FOUND: the script holds a paragraph of what came of it for
   1,990 of the cards' 4,456 choices, and for 233 chances one for each way they can fall (2,429 texts in all), and
   NO code had ever shown them: a card answered said "You did X." and the effects' own facts. Now a card answered
   tells the choice back small, then its written end in the teller's voice, then the facts in the let-in box; the
   log keeps to the short telling. The ends are found by the key of the choice's label, as the writers named them
   (Code/Core/Encounters/Outcomes.cs: "x.opt_k" -> "x.out_k", "x.o.k" -> "x.out.k", "x" -> "x.done", and for a
   chance "_success"/"_failure", "_ok"/"_fail"/"_bad", ".success"/".failure"); a choice may name its end outright
   ("out", "out_success", "out_failure": six do). STILL WITHOUT A WRITTEN END: 2,466 choices, among them all of
   Dorestat's own harbour cards and the toll man. They say what you did and the facts, as before: writing them is
   content, and the user's.
102. **THE PEOPLE OF THE CARDS HAVE THEIR OWN NAMES** (8 Oct, night; the user: "Do what you think needs to be done";
   it is item 97's open point, and the alpha's row 7: "the speaker has a name and a face"). The words of a card call
   its people by name ("Folkmar rolls four pairs down to your boat") while the window made up another for each
   ("Meinbod, the craftsman"), and the moneyer of Dorestat's own page, Sigimar, was "Madalger" on his card. Now:
   - An actor whom the designers' note ties to a person IS that person: 751 by the id of one of the places' people
     in the note, 83 more by a name the note gives where the card's own port has somebody of that name, and 365 by
     a name the note gives and nothing else ("Folkmar, a quern cutter of the vicus; invented"). With the crew's 370
     that is 1,569 of the cards' 2,328 actors. The other 759 have no name in any note and are made up as before.
   - One of the places' people is that person only in the years the places give them ("cast_from", "cast_to"):
     after Sigimar's last year the mint's card is met with whoever holds the mint then.
   - A card that wants the holder of an office at a port (the toll man, the moneyer, the count, the bishop, the
     abbot, the abbess, the reeve, the harbour master, the king) meets the one the port's page names for that year
     (PlaceBook.Holder): the toll man of Dorestat is Aldgrim on his card as in the port. A priest, a skipper, a
     merchant are many: a card's is its own.
   - A named person has their own portrait where there is one (one of the story's cast under the cast's id).
   NINE notes name somebody ELSE than the speaker ("a man of the count's (cast.folcbert)") and were left alone.
   WHAT THIS DOES NOT DO: the port's people and the cards' people still keep two memories (talk in the port does
   not use the cards' feelings); and a named person who is in the words of a card after their years is still named
   there.
103. **THE MEN ON THE QUAY ARE THE CREW OF THE CARDS** (the same night; item 97's other open point, as I had
   advised). On the first morning five made-up names stood on the quay and three were hired, and from the first
   card on the crew were Aldger, Brunger and Beornwine, whom nobody had hired. Now those three are the men who
   look for a boat, each with a line of who he is from his sheet (FirstJobs.Crew; placeholders). There is no
   choosing among them any more: before, the choice changed nothing but a name.
104. **THE DEBT HAS AN END** (8 Oct, night; the user: "Do what you think needs to be done"; the alpha's row 6: "a
   debt with a day and an end (paid, or what follows if not)"). FOUND: in a game begun through the opening the
   count's chain of debt cards sleeps (item 50), so nothing at all followed the day. The bar turned to "overdue"
   and stayed so. The lender's man of item 33 (the user's own idea) had never been built. Now, in such a game
   (Data/encounters/wokerer_793.json, eight cards; words Data/writing/wokerer.json):
   - THE LENDER'S MAN (item 33, as the user sketched it). One man with a name and a memory, who carries the other
     half of the stick. From the tenth day he is there when she moors: at home on the dam head, anywhere else on a
     jetty he could not have reached, once in a boat alongside in fog. Seldom while Michaelmas is far (24 days
     between), oftener as it nears (12, then 6), sooner after he was put off (10, 6, 3). ANY SUM MAY BE PAID to him,
     wherever he finds her: a pound, sixty pennies, or all of it, and he cuts the notch there. Or: feed him (he likes
     you and comes anyway), put him off, lie to him (a chance by your Tongue; he remembers), hide under the sail
     (the men find it funny, and each time less so). He is polite, patient, seasick, fond of his food, never angry,
     and never says how he got there. HIS NAME, "Bubo", IS A PLACEHOLDER (a Frisian name of the 730s from the game's
     own list): THE USER NAMES HIM. His face is the stand-in of his trade until the user draws him: a picture named
     portrait/person.dorestad.wokerers_man.png under Art/final is shown at once.
   - THE DAY. From Michaelmas on, when she lies at Dorestat with the stick not clean, the lender is on the dam head
     himself with two men and a rope, as he said in the opening. He asks for all of it. A week past the day his man
     brings the rope to wherever she lies.
   - PAID: the stick is given up ("A clean stick"). She is the merchant's own. The deed is known on the bank
     (paid_the_stick).
   - NOT PAID: **DECIDED BY ME, AND THE USER'S TO OVERRULE.** What follows a missed day had never been ruled
     (Design/fixes/debt_day.md: "what the count does on Michaelmas is still to be ruled and written"). I chose the
     mildest thing that is still a real loss and that the period's law allows. The lender may ask back no more
     than the sum (the king's law of 789, item 93), so a late debtor pays in other coin, and the old chain's own
     page names the coins: "the boat's work, a pledge, a gift, or himself". So: SHE WORKS FOR HIM. From the day he
     says so, half of everything she sells goes to the stick before it reaches the purse
     (trade_rules.json "work_share": 0.5; Trade.WorkShare), his man sails with her and counts, and the bank hears
     of it (the deed missed_the_day). Nothing is added to the debt. He does not take her purse. The game does not
     end, and there is no dead end: every sale still pays her the other half, and silver may be paid on the stick
     at any market as before. When the stick is clean he gives it up and his man goes ashore.
     WHAT I DID NOT CHOOSE, and why: taking the boat (the game has no way on from there yet: the house with heirs
     of item "A house with heirs" is not built, nor is any end of the game); adding to the debt (forbidden, and
     he says himself that he does not lend); the widow's surety or Thiadbod buying the stick (the old chain's ways
     out: they want rules of their own, and the user has not said what becomes of that chain).
   - Errands says what follows: before the day the lender's own word ("The day after, he comes with two men and a
     rope": the critic's B5), after it what she owes of each sale. The bar says "she works for the Wokerer".
   - Wursing is a person of Dorestat now (places/dorestad.json), with a line when the debt is spoken of. Dorestat's
     own page no longer says the count holds the debt.
   NOT DONE: the boat's work as a graph (the plan's step 3d: it is a sum in code, as every rule's fallback is);
   the count's old chain is as it was; a home port other than Dorestat (the man's "home" card is Dorestat's).
105. **A NAMED PERSON'S PORTRAIT** (with item 102). A picture named portrait/person.<their id>.png is theirs: the
   id is the one their cards give ("dorestad.sigimar", "named.chain_black_stones.folkmar", "dorestad.wokerers_man").
   The art list does not yet issue slots for the people the story cards name or for the lender's man: when it is
   next made (Bake/art/make_art_list.py) they should be added to it with a description each.
106. **THE ROAD TO RELEASE** (the user, 8 Oct, night: "create an extensive and indepth analysis on where the game is
   and every single step from here to alpha to beta to full release in percentage"; then: "DO WHAT YOU CAN FOR
   NOW"). The paper is Design/ROADMAP.md, made by Bake/plan/roadmap.py: 100 is the first release as ruled (one life,
   793 to about 805, twelve ports); the count of that night was 42, the alpha at 49, the beta at 93, in 67 steps.
   The weights are my estimates. A step done is put into the model's DONE list and the paper is made again. I work
   down the alpha's steps in the paper's order, each tested and committed, and leave the ones that want a ruling.
107. **THE FIVE REGIONS' WORDS ARE IN THE GAME** (the road's step 1). 5,259 written texts (133,588 words: the Baltic,
   the Danes and Norway, south-east England, Northumbria and the Irish Sea, part of the Rhine and Frisia) lay in
   their source files and had never been put into writing.json, the one file the game reads: 408 of the 1,400 cards
   showed "[write: key]" where each line should be, 66 of them in the first release. They are ADDED (never merged
   over: Bake/content/merge_writing.py writes the sources over writing.json and would undo any mend made only
   there), after the passes the rest had on the critic's report: Dorestat, Traiectum, pennies, alehouse (92 texts);
   her six oars; "the king" in three lines that can be said before 800. Their choices' ends are found by their own
   naming ("x.o_gift" and "x.do_pay" -> "x.out_gift", "x.out_pay"): 2,573 of the 4,484 choices now have a written
   end (it was 1,990). STILL WITHOUT WORDS: the 14 cards of the saga of the Rhineland and Flanders (879 to 886),
   whose words were never written. Not touched: "the Dane" in these regions' cards is each time another Dane, not
   Ravn.
108. **EVENTS MOVE THE STATS, BUILT** (the road's step 7, the alpha's row 10; the user's ruling is item 18 of 7 Oct;
   the rule is Design/fixes/stats_events.md). A stat is a base (1 to 5, grown by use as before) and the MARKS that
   lie on it. A mark has a size (one, rarely two, up or down), a cause the player can read and an end (a count of
   days, or a flag set somewhere: "until the stick is clean"). The same mark twice starts its time again. All marks
   together move a stat by no more than two; a stat never reads under 1 or over 5. What a choice asks for and what a
   chance leans on is the stat as it is now ("me.nerve"); the base is "me.nerve.base"; "mark.<id>" asks whether a
   mark lies on the player.
   - Cards: three new verbs: "mark <stat> <+1|-1|+2|-2> <id> for <days>", "mark ... until <flag>", "unmark <id>",
     and "stat <stat> <+1|-1>" for the base, for good (rare). The cause's words are the writing under "mark.<id>".
   - Deeds: a deed that is known puts its mark by a table (story/self_793.json "deed_marks", 18 rows): fought
     raiders off, Nerve up for a month; goods seized, Name down for two months; the missed day, Name down until the
     stick is clean. Selling captives and digging hallowed ground cost a Christian's Troth for a year, and a
     heathen's not at all. THE SIZES AND DAYS ARE MY FIRST GUESSES, AND THE CAUSES' WORDS PLACEHOLDERS.
   - The body: no bread aboard lies on Nerve, sickness aboard on Tongue, each until it is mended.
   - Shown: in Yourself the pips are the stat as it is now, and under a stat stands each mark with its cause and how
     long it has yet. The portrait's line and the Faith and House windows show the stat as it is now.
   NOT DONE of the write-up: marks from dress and gifts, from company aboard, from drink, from a season in a
   strange tongue's land; a greyed choice that says what would raise the stat; the lint in check_content.py.
109. **CARDS THAT WAIT, BUILT** (the road's step 5, the alpha's row 7; the user's rulings are items 10 and 12 of
   7 Oct; the plan is Design/fixes/card_clock.md). A card opens and holds the clock, as before. If the player lets
   the clock run without answering:
   - an ORDINARY card waits 168 running game hours, then takes one of its own options, closes, and the log says so
     ("You gave ... no answer, so it went this way: ..."). The time counts only while the clock runs, in the clock's
     own step, so it is the same game minute at every speed;
   - a PRESSING card (44 cards: a squall, a man overboard, fog, a leak) waits its own hours, 1 to 72; 36 of them
     HOLD HER where she is meanwhile: she keeps her course and her order and makes no way. The clock may not run
     fast while one waits (the clock's own rule, which was waiting for this);
   - which option a card takes is in Game/Data/card_rules_793.json, by the options' keys, in the order to try
     them; the first that can be chosen on the day is taken. The file is written by Bake/content/card_rules.py
     from the rows a person read (all 428 cards that can open in the first release are read; the 8 cards of the
     debt added today); edit Bake/content/card_rules_overrides.json and run the script, never the file;
   - left to wait, the card says under itself, in a row of its own, how long it will wait yet and what it then
     does ("She is held here. It waits 1 hour more. Then: ..."). Nothing is said while the clock is held: no time
     passes then. PLACEHOLDER WORDS;
   - the hours a card has waited are saved with the game.
   DECIDED BY ME, the user's to overrule:
   - a HARBOUR card still unanswered when she casts off goes its own way at once (whoever stood on the quay does
     not follow her out). Before, the toll man's card would have sailed with her for a week;
   - a new card holds the clock anew, though the last one had been let wait;
   - only the card on screen waits: one in line behind it does not count its time yet;
   - Esc still shuts an unanswered card that is not an arrival card, with no option taken;
   - a rest or a wait for wind is running clock: a card can answer itself meanwhile.
110. **THE OARS, BUILT** (the road's step 2, the alpha's row 3; the user's ruling 8 of 7 Oct names the order:
   "Oars (out oars, or sail)". Which of the three rules of Design/fixes/chart_tools.md part 4 it should be was
   never answered: I TOOK (b), THE HARD PULL, the user's to overrule).
   - Left to herself she goes as she always has: by the sail, and where the sail gives less the men take an easy
     stroke by themselves. Nobody who never gives the order is worse off, and the other ships go the same way.
   - OUT OARS: the sail comes down and the men row hard. Her class's full speed under oars (3.5 knots for the first
     boat; the easy stroke is half that) whatever the wind: nothing lost with the wind abeam or aft, up to half
     lost dead into a gale's wind, and in a gale she lies to. It lasts as long as the strength in the men: her
     class's rowing hours, six for the first boat.
   - It costs bread and strength. A full pull eats half a day's food on top of the day's. The hours come back only
     with rest: whole again after 18 hours without hard rowing. Spent, the oars come in by themselves, it is said,
     and their heart is a little the less. The order is refused with less than an hour in them, and says how long.
   - Made fast the oars are in. The order and the men's strength are saved with the game.
   - WHERE THE ORDER IS: beside her course in the Voyage window ("Out oars" / "Ship oars", and a line that says
     what the men have left in them). NOT a button of the rack: the three rows of four the user chose by the picker
     on 7 Oct have no place free. If it should be one of the twelve, say which it takes the place of.
   - The view of the ship shows her under oars while the order stands.
   What it is for: a foul wind or a calm, a day or a tide to make, a sail that is closing (the raiders close on
   where she really is). THE NUMBERS ARE FIRST GUESSES (voyage_rules.json "oars").
111. **ORDERS THAT WORK, BUILT** (the road's step 6, the alpha's row 9; the plan and its drafts are
   Design/fixes/orders.md, written 7 Oct and never run until now: all its tests pass).
   - OFFERED, THEN TAKEN OR REFUSED. A card does not take an order for the player any more. It offers one; when
     the card has closed the game asks, with the terms: what, for where, by what day, at how much a unit and in
     all, the toll, how many loads it is for her boat. Refusing costs nothing. Shut unanswered, the offer waits in
     Errands seven days and then goes to another. A port's own posted order is as before.
   - TAKEN, it stands in Errands with its day, what is brought and what is aboard.
   - IN PARTS. Where it is wanted she hands over what she has aboard, in Errands; each part is paid as it lands.
     On arrival the log says what is aboard for an order there; nothing is handed over for her.
   - THE PAY is so much a unit, fixed the day the order is given: what that market pays a seller that day, never
     more than it usually pays, plus the premium asked cut to a half, and the part over the market never more
     than 60 d for the whole order (my reading of ruling 14). 126 of the cards' 220 orders asked more than a
     half: their premiums are written down to 0.5 (Bake/economy/cap_order_premiums.py).
   - TOLLED like any sale, by the graphs' own rule. While she works for the lender his share comes off it too.
   - WARNED: seven days before its day and on the last day the log says what is still to bring, and the clock
     drops back by itself (two new reasons in the settings, like the debt's).
   - ITS DAY PASSED: nothing brought, it is broken (-5 there); part brought, it is short (-2, a new deed), and
     what was landed stays paid. No late delivery, no handing back.
   CHANGED FROM THE DRAFT, by me: the ask comes when the card has closed (so what came of the card is read first),
   one ask at a time; it is plain words, not somebody speaking; goods keep their capitals ("Rhenish wine"); in the
   narrow window the buttons stand under an order's line.
   THE DRAFT'S OWN CHOICES STAND, the user's to overrule (orders.md part 9): the 60 d cap; the lower of today's
   price and the usual; every card's order is asked once; no late delivery; handing over is the player's act; a
   short order costs 2 where a broken one costs 5. Four of the twelve ports (Sarre, Fordwic, Medemelacha,
   Doccinga) still post no orders of their own: their kind has no row in commissions_793.json.
112. **DEALS BY THE LOT, AND THE WORD SAID AGAINST WHERE SHE LIES, BUILT** (the road's step 4, the alpha's row 5;
   the plan and its drafts are Design/fixes/word_and_lots.md, written 7 Oct and never run until now).
   - THE LOT. A good's line keeps its "Buy" and "Sell" (one unit). Under the line, a row for selling and a row for
     buying: half of what can go and all that can go, each button with its count and its sum on it ("Sell 2 for
     73 d", "all 4 for 130 d"). "All" is all aboard, or all that the stock, the room and the silver let her buy.
     Each button is one deal and one settlement.
   - THE PURSE no longer rounds to the whole coin: what is under one coin is weighed out in cut silver, in change
     and in takings. (It is the same purse for every ship of the world, so a seeded run plays a little
     differently than before.)
   - THE WORD on a good is said against the price she has in front of her: where she lies, or at sea where she
     last saw that good priced. Five words: far more, more, much the same, less, far less. "Far more" begins where
     carrying the good there would pay, the market's gap and the toll counted. How exactly it is said goes by how
     she knows it: her own eyes within the week, the figure and the difference; a man who saw it, or her own
     older sight, "about" and a rough figure; talk through more mouths, a word only. A figure is in the reckoning
     of where she lies.
   - BY WHOM. Whoever on a waterfront told a price is one of that port's own people, by name and trade ("Hartwin
     the steward at Dorestat said ..."): the toll man, the reeve, the moneyer, the ale-wife, the host, a
     shipmaster, a pilot, the ferryman, a steward, the cellarer. The same one for the same telling. Not a
     merchant, and so not the lender. Where a port's page names nobody of such a trade the old words stand.
   - Under a good that is only wanted here (none aboard, none on offer) no word is said: it was the same
     sentence a dozen times over.
   ONE RULE CHANGED, THE USER'S TO OVERRULE: a man who saw a market himself now gives a rough figure ("about
   60 d"). Before, a number was only for what she saw herself. One switch puts that back: "told_figure" in
   knowing_793.json.
   NOT TAKEN FROM THE PLAN: the ledger (goods down, ports across, a wide window of its own: the Wares window does
   that work today and the side window is narrow); the top bar's silver as what it buys here (the trade window
   says both figures in words); asking a named person for prices, and holding a teller's word against the market
   when she moors (Design/fixes/one_cast.md, parts 4 and 6).
113. **A STAKE INSIDE THE FIRST HOUR, AND A CLEAN SCREEN, BUILT** (the road's step 8, the alpha's row 17).
   - A STAKE WITH A DAY. On the first morning, when Ravn has had his say, an order is asked: two casks of the wine
     that came with the boat, for Traiectum, inside ten days, at what that market pays and a little over. Taken,
     it stands in Errands; refused, nothing is lost. Ravn says it in one line and points at Errands; whoever wants
     it is that port's own cellarer (Hadda). The numbers are in trade_rules.json "start.first_order". RAVN'S LINE
     IS A PLACEHOLDER, the user's to write. (Design/fixes/first_hour.md, the steward's first payment of 120 d, was
     written before the user's own opening and is NOT built: the lender and Michaelmas stand as the user wrote
     them.)
   - THE PAGE'S RULE (Design/fixes/page_and_strings.md): nothing reaches the screen that was written for the
     makers. PageWords.Fault names what is wrong with a string. The play tests read every word on every screen
     they picture and fail on a fault. 43 texts were found and mended:
     - what the game lacks: five windows no longer say what is not built (Ship, Crew, Bonds, Faith, House); the
       Port window's 21 lines that ended "(to come)" say what the row does; a row whose errand is not built is
       not shown;
     - numbers and words in brackets: a standing is its word ("known", no "+25"); an order is "at 46 d each, 91 d
       in all"; a choice's price is an aside by commas ("Give him something, 12 d": 398 choices of the script,
       changed where they are shown, not in the script); "Saved: 1 May 793"; the talk of the waterfront and of a
       ship met says its ages in words;
     - ids and sums: a choice that cannot be taken says why in the world's words ("Only if you carry weapons, or
       captives."; "Only at Dorestat."; a writing by its title; a role by its title), and "Not now." where the
       reason is the makers' own (a flag, a sum);
     - workshop words: no window names another window; the stats line reads "These grow by use ...".
   - NOTHING COVERS ANYTHING, at full size, in the layouts a player has: the play tests measure every text
     against its box and fail on one that runs out or breaks a word. Mended: a tab's word is not broken; the
     boat's caption has room (and says the wind in few words: "light breeze, south-west"); the top bar's figures
     have a rule between them and the stores are told in whole days.
   NOT DONE, AND LISTED BY THE TESTS: a screen of 720 lines (55 texts run out or break there: the rack's words
   above all); the tiled layouts of columns and tabs, which only the tests can reach. Of page_and_strings.md:
   parts 3 to 6 (line length, a text-size setting, the chest, captives), modern measures (km, kg, knots, m/s),
   and the talk's keys offered as questions.
114. **THE BOAT CAN BE HURT AND LOST LIKE ANY OTHER, BUILT** (the road's step 3, the alpha's row 4; the plan and
   its drafts are Design/fixes/risk_is_real.md, written 7 Oct, compiled and its 22 tests run outside Unity, and
   now run in it: all pass).
   - ONE BOAT (Voyage/Boat.cs). She has a hull (sound, working, a strake started, opening, going), water in her,
     a sail (whole, a seam gone, split) set all of it, shortened or down, an order for a gale (lie to, run
     shortened, carry sail), men who grow weary, and whether they sail on shares. Saved with the game.
   - EACH GAME HOUR (BoatWatch) the weather where she is and for twelve hours ahead is read from the world's own
     model: the hull works in a gale, water comes in by the laps and over the gunwale and the men bail it, the men
     tire or rest, a weak sail may split.
   - THE SAME DICE as every other ship (GaleDice), times the state she is in: 1 for a sound boat lying to with a
     full crew; more for running (2.5), carrying sail (8), a started strake (2), an opened hull (5), water to
     half her depth (2), weary men, a short crew; less lashed down, and a tenth on a river.
   - WARNED FIRST. Three signs: the swell twelve hours off, the sky six, white water within the hour. Each is
     told in the log and drops the clock (two new reasons with their switches); the second and third bring the
     swell's card and the gale's card. No dice are thrown in a gale she had no sign of, nor in hours the clock
     was jumped over.
   - WOUNDED BEFORE SHE IS LOST. The first sea that strikes starts a strake; the next takes the sail; the third,
     the boat. She is also lost if her strakes open altogether or she fills.
   - LOST IS THE END (the user, 2 Oct): the last page says how, with the last of the log; the clock stands; no
     save is taken; "Load the last saved game" brings her back, or "To the title".
   - THE CARDS ARE THE BOAT. Nine of the deck's flags (the leak, the weak sail, the reef, the lashing, the spar,
     after a gale, shares, bundles, tar) are read from her and written to her; the deck keeps no second copy. A
     new verb "boat ..." and the names gale, gale_in, boat.hull, boat.water, boat.weary, boat.sail. 18 lines of
     eight sea and crew cards were changed as the plan lists (7.2): the gale card's three choices ARE the order.
   - HER SPEED is the world's ships' own rule through her sail, her water and her men. The hard pull at the oars
     (item 110) goes through her water and her men too. The men tire little under sail, a little more at their
     own stroke in a calm, and much at the hard pull or holding her head to a gale; a mooring rests them.
   - SHARES: half the wage each day and a tenth of the season's gain at Martinmas (the plan's 12.2, as drafted).
   - THE SHIP WINDOW shows her as she is, takes the orders at sea (the sail, the order for a gale, run for the
     nearest port), and made fast gives a yard's work on her seams (8 to 28 d) and her sail (3 or 12 d). THE CREW
     WINDOW shows weariness, who bails, and what they are paid by.
   FOUND AND MENDED WHILE TESTING IT IN THE GAME:
   - the draft refused a lost boat both the save AND the load, so "Load the last saved game" could never have
     worked. A lost boat is not saved, and is loaded from (a test holds both);
   - the Ship window was built again every game hour (the water in her moves each hour), which at the fast
     speeds loses the click. It is built only when its lines change; what they say changes in place;
   - a calm day at the men's own stroke wore them out (a third weary by evening). Their own stroke now tires
     them little; the hard pull and holding her head to a gale tire them much;
   - while the opening's sail is still to mend, the Ship window says "tattered" (it said "whole").
   DECIDED BY ME, the user's to overrule (the plan's part 12):
   - what four pressing cards take when nobody answers is now the SAFE thing (shorten and stow; lie to; beach
     her; sew the sail), since a gale now costs more than time. The swell's card no longer holds her where she
     is and waits two hours, not six: the warning is time to use;
   - how she would fare in a gale is said as a WORD ("she would ride it" ... "she would not live long in it"),
     never as "1 in 350": the cards show odds as words, by the user's own rule;
   - the other ships still die on the first hit and do not wear (not changed);
   - the table's chance for the whole fleet is left as it was.
   NOT DONE: the brief faces; the 3D view does not show a shortened or split sail; the written ends of the gale
   card still tell the whole gale at once ("it has blown itself out") though the gale now plays hour by hour:
   the user's to rewrite; "crew.pay.martinmas" still pays its fixed sums beside the real tenth; she cannot take
   the ground on the chart, so no grounding hurts her yet. THE NUMBERS ARE FIRST GUESSES (boat_rules.json).
115. **THE PLAY-THROUGH, A PICTURE AT EACH STEP, DONE** (the road's step 9, the alpha's row 18; 9 Oct, small hours).
   - THE WALK. Tests/PlayMode/PlayThroughTests.cs plays the game through the real buttons from the title to the
     Michaelmas reckoning: the opening, the first jobs by their own buttons (the sail's row, the alehouse), and a
     season of trade by a plain trader (it sells what it brought, buys what the next port pays most for, keeps
     bread aboard, rests the men, has a yard mend her, answers every card, is home for the day). It is long (a
     season in about a quarter of an hour) and is not in the suites: run it by its name. KAPMAN_PT_DAYS shortens
     it. Pictures, a journal of every step and the page's faults go to Captures/playthrough.
   - THE FIRST WALK: 151 days, 62 calls at 16 ports, 166 cards, 549 pictures; the debt paid on 13 August, the
     clean stick on 24 September. I looked at every picture. The record is Design/checks/playthrough_1.md, with
     the first walk's journal and faults beside it.
   - THE SECOND WALK, with the mends in: 55 calls, 171 cards, raiders alongside on 18 June; on Michaelmas 784 d
     in the purse against 1920 d owed. The page's rule found 31 faults on the first walk's pictures, 6 on the
     second's. Its journal and faults are kept too.
   - THE THIRD WALK, with the lender's day mended: 43 calls, 165 cards; on Michaelmas at first light his card
     came to her at home, with 1580 d against 1920 d, and she works for him. Three walks, three ends: the debt
     can be paid and can be missed.
   - MENDED AT ONCE, because they would have spoiled the user's own play (the record's list 1, eighteen
     points): the lender never came for a boat that lay at home on the day (his card met her only as she
     moored: the day now comes to her where she lies, from first light); she was made fast at a port she only
     passed; a split sail had no way to be mended at sea; a
     gale's first sign came within the hour; old happenings doubled the boat and its cards; a sighting opened
     over a card; the debt's words were cut in the bar; Errands kept old words; a yard was offered on a sound
     hull; a tight boat was wet; stones of the 900s were given in 793; "Not now." with no reason; "lost heart"
     for a hundredth; the Book's people never opened; and small words. Held by PlayThroughMendsTests (5).
   DECIDED BY ME, the user's to overrule:
   - A CARD HOLDS THE CLOCK UNTIL IT IS GONE ON FROM (what came of it too; a happening's end the same). Until
     now the clock ran again from the choice, and at the fastest clock a day passed while the end was read. The
     player may still set the clock running under it. My reading of item 5 ("it stops the clock but you can
     override it").
   - A PORT BESIDE HER WAY IS PASSED: under way on a course for another port she is not made fast there. To
     call, chart the course to it.
   - THREE OLD HAPPENINGS ARE OUT of events_793.json (She's taking water, A gale, The water turns): the boat
     and the cards tell those now. The other nine still come; whether they stay is asked in the record.
   - A sound hull's weep is scooped out as it comes (10 litres an hour, boat_rules.json "scoopAnHour").
   NOT MENDED, listed in the record (list 2): a fifth man on crew cards ("Uffo, the sailor": one_cast.md parts
   4 and 6 want a ruling on who may fall sick or leave); the small ship view black or white at the fastest
   clock; news, markets and tongues learned in play do not reach Journey and Tongues; brackets in the
   calendar's texts. FOR THE USER (list 3, thirteen points): the season is winnable with 47 days to spare; the
   debt is paid by a button far from the lender; the men are paid twice; and ten more.
116. **THE CHART'S ZOOM AND ITS FOG OF WAR, MENDED** (the user, 9 Oct: "Can you fix the map zoom and the fog of
   war"). I looked first: a test zoomed the chart a notch at a time and sailed three days, a picture at each
   step (Tests/PlayMode/MapLookTests.cs, pictures in Captures/maplook).
   FOUND:
   - THE FOG'S PICTURE NEVER MOVED OR SCALED WITH THE CHART. It stood on the screen as it was first drawn. The
     clear round was not on the ship (at sea it lay 84 km west of her and twice too large), nor the known waters
     round home. The cause: the fog is one picture larger than the view on every side, so it is cut to the view
     and its place on the screen never changes, only the part of it shown; and UI Toolkit's Image is not drawn
     again when only that part (its uv) changes. This is also why the fog was "just static" on 7 October: what I
     built then (known, remembered, in sight) was right and could not be seen.
   - THE WHEEL RAN OUT PAST THE PAINTED CHART, to 9 km a pixel: the chart a small square in the grey world of
     the first build's bake, with no fog on the grey.
   - EACH NOTCH WAS A JUMP of 18 in the hundred, and under the follow the view lurched off her by 40 pixels
     and slid back.
   - FAR OUT THE PORTS' NAMES LAY IN A HEAP.
   MENDED (Code/Game/Map/MapView.cs):
   - the fog's picture is drawn again whenever the part of it shown changes. Measured on the screen itself:
     two marked squares of the fog land within a third of a pixel of their place at every scale;
   - the wheel names a scale and the view closes on it in about a fifth of a second. While she is followed she
     keeps her place on the screen; when the chart has been dragged, the place under the pointer stays under it;
   - the view never leaves the painted chart: its widest is where the chart still fills what the frame leaves
     free (about 2 km a pixel on a 1080 screen), and the hand cannot drag it off the edge;
   - no port's name lies over another. Every port keeps its dot; names are written in this order: where she
     lies, where she is bound, home, the ports she has seen, the ports only heard of. A port without its name
     says it when the pointer rests on its dot.
   DECIDED BY ME, the user's to overrule:
   - THE WIDEST ZOOM STOPS AT THE PAINTED CHART (it showed 13,000 km across; it shows the chart's whole width,
     2,970 km, now). The grey world beyond is no longer reachable on the chart.
   - A NOTCH OF THE WHEEL IS A QUARTER (it was 18 in the hundred: 36 notches from the widest to the closest;
     now 27).
   NOT CHANGED: the fog's own rules (8 km squares, 30 km of sight, 160 km known round home, the fog 55 in the
   hundred dark, what is remembered 24); the closest zoom (5 m a pixel) and the ship's own view past it.
   SEEN AND NOT MENDED: at the closest zoom her mark stands some 500 m from the painted river at Dorestat (the
   port's place against the picture); the ship's own view from above is all but black on a moonless night.
   Tests: MapZoomTests (PlayMode; in the suite from now on), which holds all four mends on the real chart.
117. **THE FOG HIDES WHAT SHE HAS NOT SEEN** (the user, 9 Oct, of item 116's pictures: "Why are we doing such a
   subtle fog? Shouldn't it be you can't see anything you haven't been?"). A RULING.
   - WHY IT WAS SUBTLE. The first build's fog was 94 in the hundred dark. On 3 October the user, zoomed all the
     way out, saw "a thick film ... I can't see anything except a tiny little circle", and I thinned it to 55,
     so that the land showed through. That was the wrong lesson: the trouble was that the map zoomed out to
     13,000 km across, where the known waters are a dot. The zoom stops at the painted chart now (item 116).
   - NOW (Code/Game/Map/Exploration.cs, FogAlpha 1): nothing of the chart shows through the fog over what she
     has never seen. It is one plain dark, the frame's dark purple (30, 16, 38), with a soft edge two squares
     wide. What she HAS seen stays on the chart for good, under the thin veil of what is remembered (24 in the
     hundred) when it is out of her sight, and clear in the round she sees now (30 km).
   - STILL SHOWN IN THE DARK, by my choice, the user's to overrule: the dots and names of ports she has only
     HEARD of (faded, as before: 35 of them on the first morning, from the waterfront's talk), since she may
     set a course for them; and her charted course, whose strokes have a pale edge now so that they read on the
     dark.
   - NOT CHANGED: home waters known from the start (160 km round home), her sight (30 km), the squares (8 km).
     At the chart's widest the known waters of a new game are a small round in the dark: that is what the
     ruling asks for.
   - TRIED AND NOT KEPT: the chart's paper grain laid over the dark. Its soft patches stood out as a lattice of
     smudges, a tile apart.
   Tests: ExplorationTests (EditMode: whole fog over the unseen, the veil over the remembered, clear in sight,
   and it stays lifted behind her); MapZoomTests reads the screen: far from home the land and the sea are the
   same dark, the fog's own, and the sea off her own coast is not.

118. **THE CHART: A PLACE IS ITS DOT, A NARROW START, WATERS AND LANDMARKS TO FIND, AND CLOSER** (the user,
   9 Oct, four asks in a row after item 117's pictures). RULINGS, with what I chose inside each.
   - NAMES ("Make it so you just simply can't see the name of somewhere"; "hide the name of somewhere unless
     you're hovering over the dot and let it linger for a few seconds when the mouse is removed say 7 seconds
     then it fades away again to just a dot"). On the chart a place is its dot. Its name stands beside the dot
     only while the pointer is on it, stays 7 seconds after the pointer has left (MapView.nameLingerSeconds)
     and fades over 0.8 s, by the clock on the wall. MINE: this is for every place, the ports she knows as
     well as those only heard of (I read "somewhere" as anywhere; if only the unseen were meant, the ports she
     knows get their names back); the name stands right of its dot, left of it at the chart's right edge; a
     name still lingering goes at once when the name under the pointer would lie over it; the names lie in a
     layer of their own over every mark and never take the pointer; under a name stands a second slip where
     there is something to say: what the good picked in the market fetches there, or "heard of, not seen"
     (placeholder words). The thinning of names of item 116 is gone with the names.
   - THE START ("narrow the starting fog area, just Dorestead and the closest port"). On the first morning she
     knows her home port as far as she sees from it (30 km) and the port nearest it by water, 20 km round
     (Dorestat: Traiectum; a Dane of Ripa: Emuthon). Some 3,300 square km, where it was 80,000.
     KapmanGame.KnowStart; a new game whose people start elsewhere puts back what was lifted round the first
     home. NOT CHANGED: the ports heard of on the waterfront (35 on the first morning) are hollow dots in the
     dark, since a course can be set for them; she sees 30 km.
   - WATERS AND LANDMARKS ("add waters to discover and land marks ect points of interest").
     Game/Data/discoveries_793.json: 82 of them (23 waters, 18 rivers and river mouths, 16 islands, 10
     headlands, 5 shoals, 5 landmarks, 4 holy places, a shore) from the Gironde to the Neva, each with a place,
     a reach (9 to 28 km) and one or two sentences. A thing is FOUND when she comes within its reach, once: the
     log says "New to you: <name>. <its sentences>", a small mark stands on the chart from then on (a pale bar
     for a water, a diamond for land the sea shows her, a square for a thing built; its name and kind under the
     pointer as a port's), and Journey lists them ("Waters and landmarks: 9 of 82"). What lies in the waters she
     knows on the first morning is known without a word (the Rhine, the Lek, the Vecht, the Waal). Saved with
     the game; a save of before today is not told a heap of old news. MINE, ALL OF IT: which 82, their names (a
     name of the time where one is written, the source beside it in the file; a plain one where none is), every
     sentence (PLACEHOLDERS, the user's to write), the reaches, and that nothing but the log, the mark and the
     list comes of a find (no silver, no standing). Bake/world/check_discoveries.py checks each against the sea
     lanes; DiscoveriesTests walks her real courses (Dorestat to Ripa: the old mouth of the Rhine, the Kinnem
     shore, Wiron, Texla, the Fli, the deeps off Ripa). Seven more (the great lake, six on the Rhine above the
     delta) wait in the file for the day their rivers can be sailed.
   - CLOSER ("it needs to be more zoomed in otherwise the map is overwhelming and cluttered"). The chart opens
     at 120 m to a pixel (it was 400): one pixel of the painted chart to one of the screen; 174 by 93 km show,
     and the round she sees takes two thirds of the chart's height. MINE: the figure. The wheel still goes out
     to the whole painted chart. SEEN, NOT MENDED: at this scale the edge of what is known is blocky (the fog
     is kept in squares of 8 km); a finer fog wants a smaller save.
   - THE NEWS OF ICELAND, GREENLAND AND VINLAND (the user asked: "have we got the Iceland settlement event? And
     Greenland / Vinland?"). They are in the chronicle (874 the settlement, 930 the Althing, 985 Erik the Red's
     Greenland, 1000 Iceland made Christian, about 1000 Leif's Vinland; and as dates from the ground, never
     told as news, the Eldgja fire of 939 and the camp at L'Anse aux Meadows of 1021). But an entry is told at
     the ports of its regions, and Iceland and Greenland have none: only the settlement (also filed under
     western Norway) could ever be heard. MENDED: chronicle_793.json "news_lands_at" names where the news of a
     land with no port comes ashore (Iceland and Greenland: Ogvaldsnes), and it spreads from there with the
     ships. Only news: no market feels it. NOT PLACES: the chart ends at about 20 degrees west; the east of
     Iceland is on its edge, Greenland and Vinland are far off it. OPEN, FOR A RULING: 229 of the chronicle's
     582 entries are still told at no port (Byzantium 58, Italy 48, no region 38, Spain 27, the Caliphate 27,
     Saxony 25, the Meuse 17, Wales 13, Provence 7, Wendland 6, Khazaria 3, the Volga 2, Cornwall 1): where
     does each land's news come ashore?
   Tests: DiscoveriesTests (7), ChronicleNewsTests (3), ExplorationTests (the fog forgotten); MapZoomTests
   reads the screen (the first morning's dark 58 km east of home; a name up under the pointer, whole for its
   time, fading, gone; a port heard of and a water named the same way; the name left of its dot at the edge;
   no name up at the widest). A fault found on the way and mended: the chart's picture was destroyed the wrong
   way outside play (MapArt). EditMode 557 (555 pass, 2 skipped), PlayMode 26 of 26.
119. **WHAT IS IN THE GAME FOR NOW, AND WHAT IS HELD BACK** (the user, 9 Oct: "I think for now we don't include
   any fully fleshed out quests/ cards until they're polished, only keep in the good polished stuff for now and
   we will add in the other content when it's all done"). A RULING, and the cut by the picker the same day.
   - HOW. Game/Data/content_hold.json names the card files that are IN. A card file it does not name is not read
     into the game (Code/Game/Content/ContentHold.cs; EncounterRunner.LoadBook): its quests and cards never come
     up. A file may be in whole, or by a list of its cards. NOTHING IS DELETED: the files lie in
     Game/Data/encounters as before, and the card tests go on reading every one of them off the disk
     (EncounterData), so what waits is still checked. To let a file in when it is polished, name it in the
     table. A new card file is held until it is named.
   - THE CUT (the user, by the picker: "Sea cards back in"). IN, 72 cards: the toll at a Frankish port (1), the
     lender's reminders, his day and the clean stick (8), and the sea's 63 (calms, swell and gales, squalls,
     fog, wrecks, whales, ice, sickness, a quarrel at the oars; they are my drafts, not polished by the user).
     HELD, 1,328 cards: the 43 quests (595 cards), the ports' own cards (548), the dated cards (73), the crew's
     (112). I had first built it with only the 13 cards the game's own rules raise (the toll, the lender, the
     boat's four); the user asked "So do we also have events that just happen while sailing?" and took the
     sea's cards back in.
   - NOT TOUCHED: the nine simple happenings (events_793.json), the chest's riddles and readings, the ports'
     people and talk, the Book.
   - THE TESTS. HeldContentTests (play) plays a new game as it is now: 72 cards in its book, a held card cannot
     be asked for, two days down the Lek and every card that comes is one that is in. OpeningTests shows the
     cards' machinery on the ports' cards and a quest, so it has the whole book (ContentHold.AllIn, a test's
     switch that a new game clears). ContentHoldTests (edit, 4): the table names real files and cards; the
     game's book is sound with the rest held and no file had to be shut; the cards the rules raise by name
     are in.
120. **THE DEBT FALLS DUE AT THE SECOND MICHAELMAS** (the user, 9 Oct, asked "Do you think that debt repayment is
   too short?"; my answer: a little: of three walks the same plain trader paid it once, and stood 340 d and
   1136 d short in the others, and that with the whole delta known from the first morning; by the picker:
   "A second summer"). A RULING.
   - trade_rules.json "due_skip": 1: not the next Michaelmas after the start but the one after it, 29 September
     794 for a game begun 1 May 793: 516 days (it was 151). The bar says the year when the day is not this
     year's: "Owed 1920 d by Michaelmas 794: 516 days left". Michaelmas 794 is a Monday, so the Sunday warning
     of 793 is gone with it.
   - NOT CHANGED, AND NOW OUT OF STEP: the lender's man first comes within 140 days of the day (his cards), so
     for a whole year nobody asks; and the first life now crosses a winter that no play-through has walked.
     PlayThroughTests still walks to the debt's day, which is now three and a half times as far.
   - THE NEXT RULING IS ALREADY ASKED FOR (the user, the same hour): "I want the debt mechanic to be more of a
     constant cat and mouse game, you CAN just play the game and try and pay the debt asap", "Or you could idk
     try different methods and comedic ways around it". Not built: a proposal comes first.
121. **CLICK AND SAIL, AND THE RINGS OF HEARSAY** (the user, 9 Oct: "We need a more intuitive sailing system for
   navigating unknown waters", "Cause right now the path is just set straight to the location and weaves ect",
   "Maybe don't even show dots for unfound locations, needs to be more vague"; by the picker: "Click and sail"
   and "A wide hazy ring"). RULINGS, with what I chose inside them.
   - POINT AND SHE SAILS. A click on the open chart, in known water or in the dark, points at a place. The
     Voyage window shows it at its head ("The place you pointed at": which way; the km and the days where the
     whole way is known, else "Through waters you do not know") with "Set this course"; a second click on the
     same place sets it at once. She goes by the lanes to their own point nearest the place, of the lanes she
     can come to, and on to the place itself where open water lies between (the world bake says); else she lies
     to as near as the water lets her. Come there, she lies to, the log says so, and the clock drops back.
     Course.PlanToPlace / SetToPlace; a short hop at sea no longer begins by sailing back to the lanes' last
     crossing.
   - NOTHING IS DRAWN THROUGH THE DARK. Her course, a planned course and the measure are drawn only where the
     water is known to her. A place pointed at has her own small cross, seen in the dark too. The measure
     says "Part of that way lies in waters you do not know: it cannot be reckoned."
   - A PORT HAS ITS DOT WHEN SHE HAS FOUND IT (seen where it lies). One only heard of is a ring some 24 km
     across in the dark (it was 80 km as first built; the user, on seeing it: "make the rings much smaller
     pls"), three rings of pale strokes each fainter than the last: the place lies inside it,
     between a quarter and two thirds of the way out from the middle, never in the middle, the same in every
     game (by the port's name: Hearsay.RingMiddle). With the pointer in a ring a slip says whose it is. The
     Voyage window lists the ports found under "Where to" with their ways, and those only heard of under "By
     hearsay" with a rough bearing to the ring and rough days ("to the west, half a day to 2 days off, they
     say"), and no way: she is sent to look by pointing at the water in the ring.
   - A HARBOUR COMES IN SIGHT. When the fog lifts from a port's place: "A harbour in sight to the west: Witla,
     that you had heard of.", its dot stands on the chart, its ring is gone, and the clock drops back to speed 1
     (a new reason, with its switch in the settings; so does a water or landmark found, and coming to a place
     pointed at). Found, a port is charted by its name as before.
   - THE DRAWN COURSE has lost its smallest jogs: points that stand off the straight by less than six pixels
     of the chart as shown (less along a coast, next to nothing on a river) are left out of the drawing. She
     sails the lanes as before.
   - MINE, the user's to overrule: every word of it (placeholders); how far off its ring's middle a port may
     lie; that the fog knows land from water when she is sent on beyond the lanes; that "run for the nearest
     port" in a gale now runs only for a port she has found.
   - A FAULT OF MY OWN MAKING, FOUND AND MENDED: since a port's mark is its dot alone (item 118), a trader
     lying at the quay lay over it, and the dot could not be pointed at or clicked. The other ships' marks
     lie under the ports' dots now.
   - NOT DONE: PlayThroughTests still picks ports only heard of by their names in the Voyage window, which is
     gone: the long walk must learn to point. The lanes still turn where they turn: a course over known water
     is the lanes' way, kinks larger than six pixels and all.
   Tests: ClickAndSailTests (edit, 5), SailByPointingTests (play: a click in the dark, the course only where
   she knows, Witla sighted, the clock dropped, lying to at the place, Witla charted by name), MapZoomTests
   (the ring on the screen, its word, no dot for a port only heard of).
122. **THE SEA MOVES HER: WIND, TIDE, THE STEERSMAN'S RECKONING, ROUGH WATERS** (the user, 9 Oct: "We need an
   intelligent realistic current and weather system that can blow and push the ship off course and rough ocean
   areas ect"; by the picker: wind and tide before the lender; "Always adrift" ("She never quite holds a line
   anywhere. Every passage wanders and you correct it by pointing again. More to do on every voyage"); a shore:
   "Strained and held off"). RULINGS, with what I chose inside them.
   - WHAT CARRIES HER, each game minute, besides her own way (Voyage/SeaPush.cs):
     the TIDE'S STREAM, reckoned from the tide model the game has had since the first build (it held the height
     of the tide only). Each constituent's wave travels the way its high water comes later, and the water under
     it runs that way at the height times the square root of g over the depth, with the wave above mean level
     and against it below; where the tide stands and does not travel the stream is let fall to nothing. So it
     turns with the real tide of the day, runs hardest in shallow water with a large range, and hardly in the
     Baltic. As the game reckons it: the narrows off Dover 2.7 knots at their hardest, off the Maas mouth 1.5,
     the middle of the southern North Sea 0.7, the Baltic none. It is no survey: a true narrows (the Picts'
     firth) runs far harder than this says.
     the WIND'S DRIFT: under sail her leeway, sideways to leeward (10 degrees beating, 6 on a reach, 2 running,
     at 8 m/s of wind; more in more wind); with no sail drawing (oars, lying to) she is blown bodily downwind
     at 2 to 3.5 hundredths of the wind's speed.
     the TOP OF THE SEA, which a steady wind sets running at 1.5 hundredths of its speed, a little to its right.
   - NOBODY STEERS AGAINST IT ON OPEN WATER (Voyage/Adrift.cs). The steersman keeps the headings of her course
     as it was laid, from where he RECKONS her to be; she is truly somewhere else by all that has carried her.
     What he knows of it:
       on a river nothing carries her (the banks);
       in a sound, over the flats, on the way in to a berth he steers by eye, and the stream with her or against
         her is her speed;
       along a coast he steers by reckoning and finds out half of his error each hour by the land (a tenth by
         night, none in thick fog);
       on the open sea by reckoning alone;
       when the end of her open water (his landfall: a river's mouth, a sound, the berth) comes in sight, he
         knows where he is, and her course is laid again from there by itself. In sight is as far as the chart
         shows her what is round her, 30 km (two fifths of that by night; no further than the air lets him
         see): he sees what the player sees. (It was 12 km as first built: the picture of a lost reckoning
         showed the haven plain on the chart under the words "should be in sight, and it is not".)
       when he has run his distance and cannot make out his landfall he has LOST HIS RECKONING: "By the
         steersman's reckoning she should be off Walichrum by now, and he cannot make it out. She lies to.",
         and the clock drops back. The Voyage window says "The reckoning is lost" and lays the course again.
     A place pointed at on open water has nothing to see: she comes to it by his reckoning, and the log says so
     when that is not where it is.
   - THE PLAYER SETS HER RIGHT BY POINTING AGAIN. Eight km off her line the log says "Wind and tide have set
     her 8 km to the north of her line." and the clock drops back (a new reason, with its switch). The Voyage
     window says how far and which way, with "Lay the course again": a new course to where she is bound, from
     where she truly is. Any course set or place pointed at does the same.
   - ON THE CHART, by her own mark and only at sea: a short arrow, the way she is carried now and longer as it
     is harder; and when she is more than a km and a half from where he reckons her, a small ring at that place
     (where her drawn course begins) with a thin line from it to her.
   - LYING TO with no course she is carried too ("The water is too deep to anchor in: lying to, she goes where
     wind and tide carry her."), unless the water is 12 m deep or less and it blows no gale: then she lies to
     her anchor. The ship's caption says which.
   - A SHORE. She is never put on the land (the world bake says what is water; the tidal flats are not). Where
     the push would set her on a shore she is held off it and slides along it. When she has not the way on to
     keep clear (lying to, a gale, a stream faster than she goes) it works her hull: a hundredth an hour, three
     in a gale. The log says so and the clock drops back. In a gale on a lee shore that is how she is lost, by
     the rules a gale loses her by already. Out oars, and she keeps clear.
   - ROUGH WATERS (Voyage/RoughWaters.cs, Game/Data/rough_waters_793.json): 33 real places where the sea is
     worse than the wind makes it. A RACE is rough while the stream runs, worst with the wind against the tide,
     quiet at slack water (the Picts' firth, the narrows, the race of Sena, Brecan's cauldron); a BAR with the
     wind blowing on to it (the Maas mouth, the Fli, the bar of the Quantia below Quentovic); a SHOAL in any
     fresh wind (the long sands, the Horn reef); an open CAPE in a fresh wind (the Naze, the Stad). In it she
     takes water over the gunwale, her hull works as in a gale, and she keeps six tenths of her speed. The log
     names it when she comes into the rough of it, the clock drops back, and the Wind and tide window says how
     rough. Each is tied to a thing to find; seven of those are new (89 things to find now), with a new kind
     "tide race" that has the mark of a water.
   - THE WIND AND TIDE WINDOW says the stream where she is, the wind's drift, how the steersman knows her place,
     how far she is off her line, and the rough water she is in.
   - A COURSE LAID FROM OPEN WATER (she is off the lanes most of the time now) takes up the lanes at their own
     nearest point that open water leads to, of the lanes that lead somewhere, and goes straight for a later
     point of its way where open water lies between: it does not begin by sailing back.
   - EVERY NUMBER IS IN Game/Data/sea_push.json (24 of them; "on": false and nothing carries her, as before).
   - MINE, the user's to overrule: every word of it (placeholders); every number; that the chart shows where
     she truly is and not where he reckons her; that he finds his error along a coast and not on the open sea;
     that she anchors in 12 m; the 33 places and how rough each is; the seven new finds' names and words (the
     race off the long bill, the race of Sena, the south roost, Brecan's cauldron, the race off Cenn Tire, the
     Stad, the bar of the Quantia); that resting and waiting for light, wind or tide she is not carried.
   - FAULTS OF MY OWN MAKING, FOUND BY THE TESTS AND THE PICTURES, AND MENDED: a course laid again at sea began
     by sailing 13 km back to the lanes' nearest point (seen in the first picture of it); driven into the old
     Rhine mouth behind its spit of dunes, her course was laid again and its first stretch took her across the
     spit for three minutes (her own way is trusted only on a lane of her course now); whether it was night
     was read from the last frame drawn, so the same game ended a metre apart at speed 1 and at speed 5 (the
     test that holds the two together caught it: it is reckoned from the clock and her place now); the
     weather watch's look ahead moved her on along her line (it asks without moving her now).
   - NOT DONE: the other ships are not carried (they keep to the lanes as they did). The stream does not run up
     the rivers' tidal reaches. Resting at sea in deep water she is not carried. The cards do not yet know of
     any of it (no card for a lee shore or a race). The long play-through walk has not sailed with it.
   Tests: SeaMovesHerEditTests (edit, 11), SeaMovesHerTests and SeaReckoningTests (play, 3: carried off her
   line, the log, the clock, the chart's marks, the two windows, the course laid again; the landfall in sight;
   the reckoning lost and the course laid again from the window; carried lying to in deep water, at anchor in
   shallow; rough water on the long sands; held off a shore and never on the land). Bake/world/
   check_rough_waters.py checks every rough water against the world bake, the lanes and the tide model.
123. **THE LENDER SENDS HIS MEN** (the user, 9 Oct: "I want the debt mechanic to be more of a constant cat and mouse
   game, you CAN just play the game and try and pay the debt asap", "Or you could idk try different methods and
   comedic ways around it"; by the picker: "Sends his goons to collect", "A handful of times", "Ports and at sea",
   "Fools first, sharper later", and of what they do when she does not pay: "Dependant"; then, while it was being
   built: "It'll be dynamic you know", "Maybe you can pay them off in installments, maybe you can give them cargo
   maybe they kill a crew member ect", "It'll be comedic but also shockingly brutal at times", "Just clueless
   brutes", "Maybe it you don't pay up for the 3rd time they kill Ravn?"). RULINGS, with what I chose inside them.
   - A HANDFUL OF TIMES. Four sendings in the life of the debt (game days 21, 130, 335, 445; Michaelmas 794 is
     day 516). On its day the lender sends a boat of his men from Dorestat, UNLESS she has paid a pound on the
     stick since the last one: then he sends nobody and his temper cools. So she can simply pay as she goes (the
     Errands window takes a pound now) and never see them.
   - THEY FOLLOW THE WORD OF HER (Voyage/Collectors.cs). They sail to the port she last made, ask round the quay
     six hours, and go on to the next she made after it, or to where she said she was bound. If she lingers they
     find her at her berth. If they see her at sea by day (6 km; less by night and in fog; not up a river, and not
     with land between) they row for her, and alongside they hold her. They give up when their days are out (40
     to 50) and go home with nothing. A quay she comes back to tells her they were there, how long since, and
     where they went on to; their boat has its own mark on the chart while she has it in sight, the log says so,
     and the clock drops back (a new reason, with its switch).
   - CAUGHT, A CARD (the quay's, or alongside at sea). She may pay a pound, two, or all of it; give cargo off
     the hold at their price (six tenths of its worth); or try a way round them: say there is no silver aboard
     (it is under the ballast stones), send them down the coast after nothing, broach a cask for them, plead a
     bad season, and at sea pull away. Each way is carried by one of her stats, is a quarter less likely for each
     time it has fooled them before ("He said look under the stones this time"), and a little less for each
     later sending.
   - WHAT THEY DO WHEN THEY ARE GIVEN NOTHING DEPENDS: on the lender's temper (it starts at 1, falls by one when
     she pays a pound and by two for two, rises by one each time they come home with nothing or fooled), on what
     there is aboard, and on whether she has just made fools of them. At 1 they only talk. From 2 they turn out
     the purse and then the hold for an instalment (more as his temper climbs), a quarter of it for their
     trouble. When there is little to take, or a way round them has just failed, a man is beaten from 3 and
     killed from 4. Never the last man aboard.
   - THE THIRD TIME SHE DOES NOT PAY, THEY KILL RAVN (the user's own idea; "ravnAtRefusal": 3 in the table, 0 for
     never). Counted: every visit that ended with nothing paid of her own will, fooled or not, less one for each
     pound she has paid since. It is said before it is done: the Errands window says "Twice now you have paid
     his men nothing of your own will. The next time, they kill Ravn.", and on that visit's card the rope man
     says it himself. It is told flat, in one sentence.
   - THE DEAD IN THE CARDS (Core/Encounters). One of the standing cast can be dead: a flag says so
     ("gone_cast_crew_ravn"). He does not speak; a card that leans on him does not come (he speaks half of it or
     more, or its own words name him); and a text may be written for when he is gone, under its own key with
     ".without_ravn" after it. Of the 63 sea cards in the game, those that lean on Ravn stop coming when he is
     dead. The clean stick's card still comes, with its end written for it.
   - THE CAST. The men are the two with the rope who already stand behind the lender on the day, and his man
     Bubo comes with them to count. The rope men have no names and no faces: both are the user's. In the user's
     words they are just clueless brutes: they say what they were told to say as far as they have kept it.
   - THE LENDER'S MAN'S OWN THREE CARDS ARE GONE (at home, away, alongside in fog: he kept turning up inside the
     last 140 days). His words are kept in the writing. The day itself and the clean stick are as they were.
   - EVERY NUMBER IS IN Game/Data/collectors_793.json. EVERY WORD IS A PLACEHOLDER (43 new texts).
   - MINE, the user's to overrule: the four days; that a pound paid stops a sending; how they follow her; the
     ways round them and which stat carries each; the whole ladder of his temper; that a visit she fooled them
     on counts toward the third; that Ravn is not killed while he is the only man aboard; that a card which
     names a dead man does not come unless it is written again; every word.
   - NOT DONE: nothing of this is in the opening's own talk (the lender does not say he will send men). Ravn's
     death has no card of its own, no burial, no word from the crew after. The other 1,300 held cards will need
     their ".without_ravn" texts, or they will not come once he is dead. The long play-through walk has not met
     them. The Errands window says his temper in one word and nothing of what they took last time.
   Tests: CollectorsTests (edit, 7), WokererTests (their card, what it orders), GoonsTests (play: the first
   sending at the quay, given nothing three times, Ravn killed and the cards that lean on him shut; their boat
   in sight at sea, alongside, the ship held).
