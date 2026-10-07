# The interface direction

6 October 2026. Built on `Design/research/ui/` (handmade, map_first, pixel_ui, unity_ui), the three mockups in
`Design/ui_mock`, the palette, `town_interactivity.md` and `research_scale_to_fleets.md` (there is no
`scale_to_fleets_plan.md`). Sizes are pixels at 1x. **(?)** marks what is not checked. Nothing here was run in Unity.

## 1. The one idea

**The chart lies on a board, and the skipper's own things lie on it. You click the thing itself.**

There are no menus to learn, because there is no menu: a port is opened by clicking the port, the hold by clicking the
boat, a man by clicking the man in the boat, a day by clicking its notch. The test for every element is "would this lie
on that board?" A close cross would not. A tab would not.

**The layers.** At most one thing is open on each, and Esc or a click on the bare chart takes off the top one.

| Layer | What it is | How many | Clock |
|---|---|---|---|
| 0 | The chart, and writing on it: names, her mark, sails, the course, a price | always | runs |
| 1 | Her three things and the chest: day stick, boat with purse, chest | always, 3.7% of the screen | runs |
| 2 | One slip, pinned by a short tail to the thing you clicked | one | held |
| 2a | The slip turned in place: port, then a site, then its market or a talk. Two turns deep, never three. The way back is written at its head ("Dorestat, the dam heads") and is clicked | same slip | held |
| 3 | A card. It takes the slip's place beside her mark; the slip comes back after | one | held |
| 4 | The board: winter, and a long text being read. The only large screens | one | held |

**What you hold is the lens.** This replaces map layers, chips and legends. Click a cask in the boat and the chart
writes what you know of wine at each port, paler the older the word. Click a man and his home port and his want are
ringed. Click a notch and the port where it falls is ringed. Click away and the chart is bare again.

**Your third mockup** (settings top left, buttons, module panels, a portrait top right, a ship window bottom right) is
kept, but each part moves onto the thing it is about. If you would rather keep the row of buttons, that is decision 1.

| You asked for | Here it is |
|---|---|
| Settings, top left | The bored end of the day stick, top left. Click it, or Esc: save, load, options, quit |
| Ledger, Tasks | Prices are written on the chart by the lens. Tasks are notches on the stick and a small mark on their port |
| Crew, Hold | The boat, lower left: the men sit in her and the cargo lies in her |
| Chest, Chronicle | The chest, lower right. The newest news is one line when you open it |
| House, World | Your ships and flags are on the chart. The plan for the house is the winter board |
| Portrait, top right | A drawn face on the corner of the slip, only while someone is speaking |
| Ship window, bottom right | Above the chest, shut until you open it (key W, or click her mark twice) |

## 2. Everything that can be on screen

Fixed things are the same pixels at both sizes; only the written slips grow. On a 1080p screen 640x360 shows at 3x and
960x540 at 2x. Margins are uneven on purpose (5, 7, 4): put down, not snapped.

| Thing | 640x360 | 960x540 | Where it sits |
|---|---|---|---|
| Day stick | 304x16 (38 days) | 464x16 (58 days) | Top edge, 2 down, running off the left edge of the screen; today's thread 64 in |
| Clock cord | 20x12 | same | Hangs from the stick's right end: three knots for 1x, 2x, 4x. Click the thread to hold (Space) |
| Boat, standing | 96x24 | same | Lower left, 5 in, cut 2 by the bottom edge. No frame |
| Purse | about 44x16 | same | At her stern: one penny and one number. The debt shows beside it only in the last 30 days |
| Chest | 24x24 | same | Lower right, 7 in, 4 up |
| Her mark; wind | 15x15; 16x16 | same | On the chart. The wind arrow stands by her when it changes or a course is being chosen |
| Port; name | 5, 7 or 9 square | same | On the chart. Names are written on the sea, no boxes |
| Hover line | up to 160 wide, 2 lines | 240 | At the pointer. Says what the thing is, never what a click does |
| Port from afar | 168x74 | 216x86 | Pinned to the port's dot |
| Town, site, market, talk | 200x148 | 264x196 | Pinned to the port; the chart shifts so the port sits a third in from the left |
| Sail | 152x60 | 200x72 | Pinned to the other ship |
| Card | 208x132 | 264x150 | Pinned to her mark, where it is happening |
| Boat, open | 224x56 | same | Grows in place, lower left. Still no frame |
| A man | 168x96 | 216x110 | Pinned to him in the open boat |
| Chest, open | 220x200 | 300x260 | Rises from the chest |
| Reading board | 304x320 | 440x480 | Right side; the chart stays in view on the left |
| Winter board | 560x312 | 800x460 | Centre, once a year |
| Ship window | 160x90 | 320x180 | Above the chest |
| Esc board | 160x120 | same | Centre |

Covered by interface: 3.7% sailing, 16.5% with a town open, 21% in the market with the boat open (the first mockup:
38% to 61%). A slip one third wide holds about 31 letters a line and 11 lines.

## 3. How each thing appears

**The day stick.** Notches are days; every seventh is deeper; cut days are darker and run off the left edge. Signs are
cut on the days that matter (the count's day, a fair, a feast, the sea closing). Two things are laid on it and replace
two meters: a **sand line under the days your bread lasts**, and, when a port is clicked, a **paler stretch for the days
the course takes**, shortest to longest. If the stretch runs past the sand you will go hungry, and no word says so.
Pointing at a notch gives its name the period's way: "four days before Michaelmas". No date in figures.

**A port from afar** (first click; nothing has happened yet). The dotted course is drawn, "2 to 4 days" is written at
its far end, the stick shows the stretch. The slip: the name, one word for how the town stands, the word on each good
aboard with its age, and "Set course". The second click commits.

**A town** (she lies there). The slip's head is one picture of the waterfront, 198x56, with the sites named on it. You
click the place itself. Until the picture is drawn the sites are plain lines, and a landing shows three lines, not
seven empty ones. Under it: the reveal line, and "Cast off". A site turns the slip in place: what is seen there, who is
there, and its acts as lines with price and hours ("Mend the boat. 8 d. 4 hours."). An act that is shut stays in its
place, paler, with its reason. An act's hours show on the stick before you choose.

**The market.** A turn of the same slip, and the boat opens beside it. Rows of 16: the good's picture, its name, the
price. Click a row: one lot drops into her hold. Click a lot in her hold: it is sold. Dragging does the same; Shift
moves all that fits. No quantity box, no "are you sure". Two lines at the slip's foot carry the word on the row you
point at ("A Frisian skipper said Lundenwic was paying well for wine. Last week.").

**A talk.** A turn of the same slip, with the speaker's face (32x32) set on its top left corner, half off the slip.
His words, then your replies as plain lines: plain, warm, hard. Talks left are three pips. A hard word (sceatta, wic) is
underlined and its note with its source shows at the foot.

**A card.** Beside her mark. A fact in two or three sentences; two or three choices, one line each, the odds as a word
at the line's right end. A choice you have because of a man aboard or a thing carried is in Byzantium and names him or
it. A shut choice shows what it lacks.

**The boat and her hold.** The hull from above is the hold: twelve lots in two rows of six (a lot is 55 kg, a first
guess), each an 18x18 cell with the good's picture. Standing, the same twelve are 8x8 marks, so "nearly laden" is seen,
not counted. No kilograms anywhere.

**The crew.** Four heads at their places in the open boat: bow, two thwarts, helm. A man who is uneasy sits turned
away. Click him: his face, and three lines for his trait, his want and his fear, and one word for his mood. An empty
seat is an empty seat. There is no crew screen.

**The chest.** Things lie in it, each a small picture with a count: the bundle of rune sticks (prices heard), the
texts, the riddles, your notes, the log, the dividers and pins. Click one and the slip turns to its list. A text opens
on the reading board in Gentium. Under the lid, one line: the newest news and how old it is.

**The winter.** The one large and most drawn screen. The chart behind it is redrawn in the frost colours with the
closed sea. Three unequal parts: on the left the year read back as a short saga in your own log lines; in the middle the
reckoning, five lines and one sum; on the right what you choose, as things: the boat on the slipway with her empty
seats to fill, and one thing to build. Spring is one line at the foot.

**The fleet.** There is no fleet screen. Your ships are on the chart with a Byzantium sail: solid where seen today,
hollow with an age where the last word put her, dashed where her orders should have her now. A second, lower row of
notches on the stick says when each is due. A letter is a card pinned to the port it came from. Orders are one sentence
with holes ("Carry **wine** to **Lundenwic**, bring **wool** home, be back by **Lammas**"), filled on the winter board,
or on her slip when you both lie in one port. A factor is a small flag on his port.

## 4. What is never on screen

Tabs, a close cross, pause and play triangles, a gear. A frame or bar round the chart. Two slips at once. A minimap, a
legend, a quest list, a message log. Labels in capitals over numbers. Coloured pills, hearts, a letter in a shape for a
face. Metres a second, kilograms, percentages, a date in figures, numbers for mood or standing. A line that says what a
click does. Key hints (they live on the Esc board). Glows, blur, gradients, veils, round corners, anything turned or
resized by the engine. Green for good and red for bad.

## 5. The kit you draw, and the fonts

Eight colours for frames, three or four an icon, light from the top left, a night outline only on things that sit on
the chart. Draw in this order; each group ends with something the game can wear. "9s 12x12 m4" is a nine-slice.

| Group | Sprites and sizes |
|---|---|
| 1. The slip | Slip 9s 12x12 m4; its hard shadow 9s 12x12 m4; tail 6x5 (left and right); well 9s 12x12 m4; rule 8x1; row under the pointer 3s 12x12 m4 |
| 2. The pointer | Arrow, hand, course, open hand, closed hand: each in 16x16. Selection bracket 9s 12x12 m4, 2 frames |
| 3. On the chart | Her mark 15x15, 3 headings drawn, 2 frames each; other sails 9x9 x4; ports 5x5, 7x7, 9x9; chosen ring 13x13, 2 frames; course dot 2x2; journey's end 7x9; wind arrow 16x16, 2 drawn; hazards 7x7 x4; factor's flag 7x9 |
| 4. The day stick | Bored end 8x16; a day, a seventh day, a cut day, right end: 8x16 each; thread 9x18; cord with three knots 20x12; day signs 8x8 x5; moon 8x8 x8; sand line 8x2; course stretch 8x3 |
| 5. The boat | Hull standing 96x24; hull open 224x56 (drawn a little broad so the hold fits); hold cell 18x18 x3 states; lot mark 8x8; a man's head from above 12x12 x4, and one turned away; penny 16x16; small digits 3x5 x12 |
| 6. The port | Site signs 16x16 x9; plain, warm, hard 16x16 x3; pips 5x5 x3; price rising, falling, steady 7x7; goods families 16x16 x18; the chest 24x24, shut and open; chest things 16x16 x6 |
| 7. The card and faces | Card frame 9s 48x48 m16, the drawing in the corners; face frame 36x36; faces 32x32: you, four men, one base head; sentence hole 3s 12x11 m4 |
| 8. Slow work | Town pictures 198x56, one a port, the first twelve first; goods 16x16 x132, a family at a time; weather 16x16 x8; winter board pieces (slipway 120x64, snow tiles 16x16); wordmark about 160x40; your own font last |

About 150 sprites before the slow work; groups 1 to 5 are about 70 and give the whole sailing screen.

**Fonts.** Three, each with one job. (1) **m5x7** by Daniel Linssen, CC0, at its true size 16, lines 11 apart: every
word on the chart and in a slip. Check it has ā þ ð æ before use (?). (2) **m6x11**, same hand, licence to check (?):
a port's name at the head of its slip, a card's first line, the wordmark until you draw one. (3) **Gentium Book Plus**,
OFL, already in the project: only on the reading board and for the winter saga, drawn sharp at the screen's own pixels,
as a declared second layer. Runes stay Noto Sans Runic, on the reading board only. Your own 5x7 replaces (1) when you
are ready.

## 6. The colours, by name

| Colour | Its one job |
|---|---|
| night | The slip's fill (at 92%, the dark glass you chose on 5 Oct), every line and hard shadow (solid) |
| dark purple | Wells, hold cells, the row under the pointer |
| plum | The inner line, rules, the card's edge |
| **byzantium** | **Only what is yours**: her sail, your ships, your flag, today's thread, a choice you have because of your man or your thing |
| gold, pale gold | Silver only: the penny, a price, a sum |
| warm white | Writing; a thing seen with your own eyes |
| pale stone | A thing heard; second-rank writing |
| stone | Stale word; a shut act |
| slate | A port not yet seen |
| timber, dry earth, peat | The stick and the hull: light edge, body, the cut |
| sand | The bread line on the stick |
| rust | A warning, always with a word beside it |
| blood | A death; a debt past its day |
| frost, snow | The winter board and its chart |

Purple stops being the colour of every edge and becomes the colour of your house. Seen, heard and stale are one ramp
of three greys, so the age of a word is read as ink fading, with no code to learn.

## 7. What keeps it from looking machine-made

1. Three materials, three shapes: a long stick of wood, a round silver penny, a hull. None is a box.
2. One box only, the slip, and each use of it has a different head: a picture, a face, a drawn corner, nothing.
3. Uneven by need. A landing has three lines. The winter is large. The newest news is one line.
4. Things run off the screen: the stick off the left, the hull under the bottom edge, a face half off its slip.
5. Each fact is on the thing it is a fact about: bread on the days, cargo in the hull, mood in how a man sits.
6. Words as 793 says them: "fair for the river", "nearly laden", "Midsummer eve". Figures for silver, lots, days.
7. No screen speaks about itself. Notes for you go in this folder.
8. Nothing is a program's control: close by clicking away, hold time by taking the thread.
9. Fewer colours used harder, each with one job (section 6).
10. Care is spent on the stick, the boat and the slip. The Esc board is plain on purpose.
11. The test: cover the chart with your hand. What is left must not fit another game.

## 8. The order to build it in UI Toolkit

One step at a time; each leaves the game playable and `ChartGameTests` passing. `MapView` keeps its public face.

| Step | What | Done when |
|---|---|---|
| 1 | `PixelPanelScale`: Constant Pixel Size, scale = floor(min(w/640, h/360)). Every px in the four USS sheets divided by 3 and rounded | Same look at 1080p; sharp at 720p and 1440p |
| 2 | The sheet: tiles as children at whole places, one rounded `translate`; zoom in whole stops; fog texture to Point | No seams while panning; no layout in `Draw()` |
| 3 | Marks as sprites on the sheet: ports, her mark by heading frame, `CourseDots`; names placed once a zoom stop | No `rotate`, no `Painter2D`, no strings built each frame |
| 4 | The bitmap font asset (RASTER, Point atlas), the slip nine-slice, `pixel.uss`, the pointer | One slip in the new look |
| 5 | `Pinned` slip with its tail, and `Layers`: one slip, peel on Esc or a chart click. The port from afar | Click a port: the slip is beside it, flips at the edge |
| 6 | The day stick: tiled days, signs from the calendar, the course stretch, the sand line, the cord | The top bar is gone |
| 7 | The boat, the purse and the chest standing | The foot bar is gone; 3.7% standing |
| 8 | The town: sites as lines, the site turn, acts (steps 1 to 4 of `town_interactivity.md`) | A call at Dorestad is played from the slip |
| 9 | The market turn, the open boat, `ItemDrag` | A cask is bought with one click, or by dragging |
| 10 | The talk turn and the card beside her (also "cards at sea") | A card opens by her mark; the clock holds |
| 11 | The lens: a held good, man or notch writes on the chart | Click wine: its prices stand at the ports |
| 12 | The chest slip, the reading board in Gentium, the log as a `ListView` | A text is read without covering her |
| 13 | The winter board | A year ends on one screen |
| 14 | The house: your ships' three marks, the due row on the stick, letters, the order sentence | A second boat is sent and heard from |
| 15 | The ship window as a small Point render texture | Later, with the pixel demo |

Steps 1 to 3 change no design and are worth doing whatever you decide below.

## 9. Your decisions

1. **How do you reach your own things?**
   (a) Recommended: click the thing itself, as above, with keys 1 to 4 for boat, chest, stick and window.
   (b) Things as above, plus a row of four small picture buttons under the stick for the first hours, which can be put away.
   (c) The row of named buttons and module panels of the third mockup.
2. **The type.**
   (a) Recommended: a pixel face everywhere, Gentium only on the reading board and the winter saga.
   (b) A pixel face everywhere, with a "plain font" switch in the options.
   (c) Gentium for all writing, pixels for pictures and frames only.
3. **What the things are made of.**
   (a) Recommended: the ink slip you chose, and the stick, penny and hull drawn as wood, silver and timber.
   (b) Ink only: the stick and the boat are slips too. Fastest to draw, nearest to the second mockup.
   (c) The bevelled old-school frame of the third mockup on everything.
4. **Purple.**
   (a) Recommended: Byzantium only for what is yours; gold only for silver.
   (b) Byzantium as the edge of every slip, as in the mockups; gold for what is yours.
5. **The base size.**
   (a) Recommended: 640x360, with a finer step in the options for large monitors.
   (b) 960x540: half as much again fits, but it is whole only at 1080p and 4K and small on a handheld.
