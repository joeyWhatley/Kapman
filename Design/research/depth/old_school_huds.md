<!-- 6 Oct 2026. For the frame of ui_mock/template3.html, layout E, "map shows through". No web search was left
this session; only named pages were fetched. [n] = read today. (?) = from memory, not checked. (D) = a number I
judged. Sizes are pixels on a 960x540 canvas (2x on a 1080p screen). Nothing here was run in Unity. -->

# Old-school full-frame HUDs, and the frame for Kapman

**In one line.** The old frames felt solid because **no part of them was ever empty**, each thing had one place for
the whole game, and one small region changed with what you were doing. Kapman's frame should be a thin rim with five
fixed islands on it, a quarter of the screen at sea and half in port, with the middle of the chart never covered.

This follows the ask of 6 October. It goes against section 4 of `ui_direction.md` ("never a frame, a log, a gear")
and keeps that paper's rules of the lens, words before figures, and purple only for what is yours.

## 1. Findings

### 1.1 The games, region by region

| Game | The frame | View window | Portrait | Buttons | Messages |
|---|---|---|---|---|---|
| **Eye of the Beholder** 1991, 320x200 | Stone all round, every slot filled | 176x120, one third of the screen (?) | Party faces at right, each with two hands, a life bar and a food bar (?) | Six move arrows and a compass under the view (?) | Three lines of text along the bottom (?) |
| **Uncharted Waters: New Horizons** 1993 | Sea or town view at left, a column at right | About two thirds (?) | Only in a talk: a face beside a text box (?) | No button rack: nested word menus (?) | The text box itself |
| **Colonization** 1994, 320x200 | Menu bar on top, a column at right, a quarter of the width (?) | The map, three quarters | None on the map | None: menus and keys | Pop-up boxes |
| **Heroes II and III** 1996, 1999 | Right column 25% wide, two thin bars at the foot (?) | The map, about two thirds of the pixels (?) | Hero faces as a list of five, a click picks one (?) | Ten in HoMM3: eight in a 2x4 block, then Next Hero and End Turn (?) | One hint line; the info box, bottom right |
| **Settlers II** 1996 | A thin ornate rim and four buttons at the bottom centre (?) | Nearly all of the screen | None | Four, the rest in windows "modeled on Windows 95" [3] | A post bird, read when you choose (?) |
| **Baldur's Gate** 1998 | Three bars round the view [6] | About two thirds (?) | Six down the right with life, order and effects [6] | Left bar: map, journal, records, inventory, spells, clock [6]. Bottom bar: actions that change with who is picked [6] | A text window at the foot, three heights (?) |
| **Age of Empires II** 1999 | Thin bar on top, deep panel at the foot | About three quarters (?) | The picked unit, centre of the foot | 5x3 grid at the foot left, filled by context (?) | Lines over the map that fade (?) |
| **Pirates!** 1987 and 2004 | 1987: none. A picture and a short menu of lines [2] | The whole screen | 2004: the faces you talk to (?) | 2004: a drawn keypad, lower right, "dynamically showing each key's current function" [5] | A box, one at a time |
| **Ultima VII** 1992 | None. The world takes the whole screen; earlier Ultimas gave much of it to interface [4] | All of it | On the gumps you open | None: you drag the thing itself [4] | Speech over heads (?) |
| **Loop Hero** 2021 | Bar on top, a full column at right, cards below (?) | About 60% (?) | None: a hero and his gear grid | Three or four, top bar (?) | None |
| **Dwarf Fortress** on Steam, 2022 | No rim. Bars and islands on the map | Nearly all | In windows | Foot left: eight for people and things. Centre: eleven tools. Right: army and world [7] | Alerts at the top; a strip with name, mood, food, date [7] |
| **Songs of Conquest** 2022, **Stoneshard** 2020 | Islands in the corners, the map between (?) | Nearly all | Hero faces as a short list (?) | Six to eight, in one corner (?) | Pop-ups; a log, lower left (?) |

Not in the table: **Darklands** (1992) has no frame, only a painting with a text menu over it (?). **Patrician II**
(2000) keeps one side column with a small map, the date, silver and lists (?).

### 1.2 What the frames have in common

1. **Nothing in the frame is empty.** In Eye of the Beholder every slot holds a face, a hand, an arrow or a word.
   A frame looks dead when it keeps room for panels that are not open: the "solid columns" switch in template3.
2. **One place per thing, for the whole game.** The frames never move a thing to make room for another.
3. **One region changes, the rest hold still.** HoMM3's info box shows the day, then the picked hero, then the town
   (?). Pirates 2004 redraws one keypad [5]. Baldur's Gate keeps the fixed bar and the changing bar apart [6].
4. **Buttons are few and in blocks.** Four (Settlers II), 2x4 plus two (HoMM3), eight, eleven and two with gaps
   between (Dwarf Fortress) [7]. The heaviest acts (End Turn, the clock) sit apart and are larger.
5. **Hover text has one home.** HoMM3 writes one line at the foot of the map for what is under the pointer (?).
6. **Messages come in three weights.** A line that passes (the log), a thing that waits (the Settlers post bird),
   a box that stops the clock (Colonization).
7. **The view window is small and it is loved.** Eye of the Beholder plays in a third of the screen, sunk into the
   frame like a window in a wall. Settlers II lets you open small live windows on far places (?).
8. **The portrait is a person and a button.** In Baldur's Gate the face shows life, order and effects [6], and a
   click picks the man (?).
9. **Solid is drawn, not coloured.** One light, from the top left. Raised and sunk are told apart by which edge is
   bright. Ornament only at corners and joins. Nothing in the frame fades, glows or slides.
10. **The frame is worn by the world.** HoMM2 has a good and an evil frame; Age of Empires II skins its bars by
    people (?).
11. **The moderns kept the islands and dropped the columns.** Songs of Conquest, Stoneshard and Dwarf Fortress put
    bevelled islands in the corners and let the map run between (?): "map, not empty frame".
12. **No frame at all has its own cost.** Settlers II hid nearly all in windows, and "the player had to work really
    hard to get into the game" [3]. Pirates swaps the screen for a picture and a menu [2] and loses the map.

## 2. Proposals for Kapman

### P1. A rim and five islands

**Rule.** The frame is a 4 px bevelled rim round the screen, with five fixed islands on it. Between the islands is
chart. The middle 480x470 is never covered, except by the winter board and a text being read.

| Region | Where | Size | Holds |
|---|---|---|---|
| Rim | All four edges | 4 px | Joins the islands; a boss at each corner |
| System block | Top left | 236x30 | Menu, the day, the clock, silver, days of bread |
| Button rack | Under it | 236 wide, 24 a row, 2 to 4 rows | Two labelled buttons a row, 116x22 |
| Panel dock | Under the rack | 236 wide, to the foot | 0, 1 or 2 panels. Empty means chart |
| Portrait block | Top right | 236x150 | Face 96x112 sunk, name, two lines, crew heads |
| Ship window | Bottom right | 236x156 | View 224x126 sunk, a caption line |
| Log | Bottom centre | 472x52 | Hint line, then three lines |

**Covered:** 26% at sea with six buttons and no panel, 33% with one panel (236x170), 41% with two, 50% in the
market (D).

**The player sees** a heavy frame at the corners and the chart running to the rim between them.

**Build.** A `FrameRoot` UI Toolkit document with seven named slots on the 960x540 canvas at whole-number scale.
Empty slots use `pickingMode = Ignore`, so the chart under them takes clicks. A `ChartSafeRect` (240, 8, 480, 470)
that `MapView` uses to centre her mark. Nine-slices for raised, sunk and rim; the five cuts of template3 (round,
carved, arch, stud, leaf) are sprite sets behind one `FrameSkin` asset, so the choice is a swap.

### P2. Fixed islands, one changing pair

**Rule.** The system block, rack and log never change their place or size. Only the portrait block and the ship
window change what they show, and both change by one rule: **the portrait is who is speaking; the window is where
she is.** Section 3 is the whole table.

**The player sees** the same screen in every state, with the right-hand column telling him where he stands.

**Build.** A `FrameState` enum (Sea, Port, Talk, Trade, Card, Winter) and one `FrameStateTable` asset: a row per
state, a column per slot, each cell a view name. `ChartGame` sets the state; nothing else may place a thing in a
slot. One test per state checks that each slot holds what the table says.

### P3. The rack: at most eight, in pairs, earned

**Rule.** Buttons come in pairs by subject, one pair a row: **Hold, Crew** (the boat); **Ledger, Chest** (what you
know); **Tasks, Chronicle** (time); **House, World** (the fleet stages). The game starts with two rows. Tasks and
Chronicle come with the first task. House and World come at stage 3 of `scale_to_fleets_plan.md`. Never more than
eight. A button with something new wears a count. Keys 1 to 8 follow the rack's order.

**The player sees** four buttons on day one. Each new row arrives with the thing it is for, and the log names it.

**Build.** A `RackRegistry`: id, label, icon, pair, an unlock test and a count source. The rack is rebuilt only when
an unlock fires. A panel opens in the dock; a third panel closes the oldest.

### P4. One hint line, three weights of message

**Rule.** (a) What is under the pointer is named on the hint line, the top line of the log. No box follows the
mouse. (b) Things that pass are log lines, stamped with the hour (Prime, Terce), the newest brightest. Three show;
a click opens thirty. (c) A thing that waits (a letter, a debt day near) is a count on its button. (d) A thing that
needs an answer is a card and holds the clock. Nothing else may stop the clock.

**The player sees** one calm strip of words at the foot, and knows a card is always worth reading.

**Build.** A `Hint.Set(text)` service called by every hoverable thing. A `Log` ring of 200 lines with hour, kind
and text, saved with the game. A router that takes each game event and sends it to exactly one of the three.

### P5. The portrait works

**Rule.** At rest it is you: face, name, age, one word for health, the debt. The face ages by decade and changes
with the heir. Under it sit the crew, four heads at 20x20; a man who is uneasy looks away. Click a head for the
man; click your own face for the house. When someone speaks, his face takes the well and his words go beside it.

**The player sees** a person, top right, always. He learns to glance there to know who he is dealing with.

**Build.** `PortraitView` with a `Show(person)` call and a stack, so a talk inside a port returns to the right
face. Faces are 96x112 sprites built from a base head and layers (hair, beard, cap, age lines), so the cast in
`content/peoples` can be drawn from parts.

### P6. The ship window is real, small, and can grow

**Rule.** It shows the 3D boat where she is: river banks, coast, open sea or quay, with the chart's own weather
and hour. The caption gives place and wind in words. Click it once for 480x270 over the centre; W for the whole
screen. In port it shows her at the quay. In winter it shows her hauled out under snow.

**The player sees** his own boat alive in the corner, and the weather before any word says it.

**Build.** A second camera drawing to a 224x126 `RenderTexture`, point filtered, at 15 frames a second (D), and
stopped while a board is open. A `ShipScene` that builds a small set from five things the chart gives it: water
kind, bank kind, wind, weather, hour. It does not stream the world.

### P7. Make it solid, and let it wear the world

**Rule.** Light from the top left on every piece. Raised: bright top and left. Sunk: dark top and left. Two
materials, dark wood and worn bronze. Fill is solid. Ornament only on the four corner bosses and the two wells.
Nothing moves but the clock and a new log line. All sizes are even; all gaps are 4 or 8. The edge colour comes
from the map's scheme, as in template3. Three skins: plain, frost for winter, and one for Miklagard.

**The player sees** a made object. In winter the frame itself has frost on it.

**Build.** The `FrameSkin` asset of P1 with three sets; a season hook swaps it. A lint test that fails on any frame
element with opacity under 1, a size that is not even, or a transition.

## 3. What each region holds in each state

| Region | At sea | In port | Talking | Trading | A card | Winter |
|---|---|---|---|---|---|---|
| System block | Day, clock running, silver, bread | Same, clock held | Same | Same | Same | Day reads "Winter", the clock gone |
| Button rack | All unlocked | All | All, greyed | Hold lit and locked open | All, greyed | Hidden |
| Panel dock | What you opened | The town: its picture, its sites, Cast off | The same town panel, dimmed | The market: rows of goods, full height | What you had open, dimmed | Hidden |
| Portrait | You, and the crew heads | You, and who is on the quay as small heads | The speaker, large; his words beside | The trader; his mood in one word | The man the card is about, or you | You, a year older, and the heir |
| Ship window | Her under way; weather and hour | Her at the quay | Her at the quay | Her deck from above: the hold fills as you buy | The scene of the card: fog, a strange sail, a bar | Her on the bank under snow |
| Log | Hint line and three lines | Same | Your replies, as lines to click | The word on the row you point at, and its age | The choices, as lines to click | Hidden |
| Centre of the chart | Her mark and the course | The port, a third in from the left | The same | The same | Her mark, ringed | The winter board, 800x460, on the frost chart |

Two things follow. **Replies and choices go in the log's place**, so an answer is always clicked at the bottom
centre. And **the ship window is the card's picture**, so a card needs no art of its own.

**Open for the user:** a rim all round or none between the islands; labelled buttons or icons only; solid fill or
the dark glass of 5 October.

## Sources

1. `Design/ui_mock/template3.html`, `Design/ui_direction.md`, `Design/scale_to_fleets_plan.md` (this project).
2. The Digital Antiquarian, "Pirates!", https://www.filfre.net/2015/07/pirates/ (menu-driven; Meier on scenes).
3. Wikipedia, "The Settlers II", https://en.wikipedia.org/wiki/The_Settlers_II (windows; the help complaint).
4. Wikipedia, "Ultima VII: The Black Gate", https://en.wikipedia.org/wiki/Ultima_VII:_The_Black_Gate (full screen, gumps).
5. Wikipedia, "Sid Meier's Pirates! (2004)", https://en.wikipedia.org/wiki/Sid_Meier%27s_Pirates!_(2004_video_game) (the keypad).
6. Wikipedia, "Baldur's Gate", https://en.wikipedia.org/wiki/Baldur%27s_Gate_(video_game) (the three bars).
7. Dwarf Fortress Wiki, "Interface", https://dwarffortresswiki.org/index.php/Interface (toolbar groups, status strip).
8. Wikipedia, "Songs of Conquest", "Dungeon Master", "Sid Meier's Colonization", "Uncharted Waters" (lineage only).

Not reached: the fandom wikis (refused) and any page on the HoMM3 frame, Stoneshard, Loop Hero or Patrician II.
Check every (?) against a screenshot before it is quoted elsewhere.
