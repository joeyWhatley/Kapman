# Pixel-art interfaces, and how you draw the game's own kit

**Written 6 Oct 2026.** Two parts. Part A is how ten loved pixel games build their interface and what the rules
behind them are. Part B is your plan: every sprite the kit needs, its size in pixels, the order to draw them in, and
the method for each, in Aseprite.

A mark `(?)` means I could not check that fact this session (web search was used up; I could only fetch pages I could
name). Everything without a mark is either checked against the source listed at the end or is plain arithmetic.

**The short version.**

1. Draw the whole kit at **1x on a 640x360 screen**, in **eight colours** of the Byzantium palette, with a **1 px hard
   border** and **square corners**. That is your "ink slip" look, and it is also the easiest frame there is to draw.
2. The kit is about **200 small sprites before the goods**, most of them 16x16 or smaller. The goods are 132 more icons,
   drawn family by family over time. Nothing in the kit is bigger than 48x48 except the wordmark.
3. Draw in this order: the slip, the well, the button, the cursor, four tally icons. After those five you have a
   working interface, and the game can wear it the same night.
4. Three things to decide are listed at the very end.

---

## Part A. How the loved games do it

### A1. Ten games, one line each on what to take

| Game | The frame | The text | What to take for Kapman |
|---|---|---|---|
| **Stardew Valley** | Wood and parchment boxes, built from a small sheet of corner, edge and fill pieces. The world is 16x16 tiles shown at 4x (one tile is 64 screen pixels at 100% zoom). | Its own bitmap fonts, brown ink on cream. The font's pixels are finer than the world's `(?)`. | One frame used for everything. The inventory is 12 slots a row, each slot holds one 16x16 icon; the same slot is used in the toolbar, chests and shops. Warning: the UI scale option runs from 75% to 150% in steps of 5%, so at most settings the pixels are uneven. Offer whole steps only. |
| **Loop Hero** | Heavy double-line frames on black, like a 1980s home computer. A very small palette. | A chunky pixel font, with an option to switch to a plainer font for people who cannot read it `(?)`. | A dark UI with a few strong colours reads well around a busy middle. Cards sit in a row at the bottom and are played onto the map: the closest cousin to our cards on the chart. Give a readable-font option from day one. |
| **Moonlighter** | Warm, chunky parchment and leather panels with thick outlines. | Large pixel text, few words per panel. | The bag is a small grid (five across, four down `(?)`) and things happen inside the cells (arrows show what an item does to its neighbours). A grid cell can carry meaning, not only an icon: good for the hold. Selection is a bracket that jumps from cell to cell, so it plays the same on keys, pad and mouse. |
| **Eastward** | Rounded speech boxes and device-like menus (a fridge, a television, a slot machine) in the style of 1990s Japanese RPGs. | A custom rounded pixel font. | Menus are objects from the world. Our day stick, tally and chest are the same idea. |
| **Hyper Light Drifter** | Hardly any frames. A few pips in a corner. 480x270 `(?)`. | No words: conversations are pictures. | Pips instead of numbers where a count is small (days of stores, a man's mood). The less frame, the more the picture is the game. |
| **Dave the Diver** | Pixel characters over a 3D sea, with a **clean high-resolution interface** on top (a phone with apps). | A smooth modern font. | Mixing can work when it is a rule: the world is pixels, the interface is not, and nothing sits in between. |
| **Sea of Stars** | Thin light lines on deep blue panels, very little ornament, small portraits. | Small, neat pixel text `(?)`. | A thin frame makes rich art look richer. The team spent the first six months on lighting that matched the pixel art; the frames were kept quiet so the art could carry it. |
| **Octopath Traveler** | Pixel sprites in a 3D world; the interface is smooth: hairline gold rules, paper texture, dark see-through panels. | A fine serif font, high resolution. | The same lesson as Dave the Diver, done with a bookish serif. This is the nearest thing to "Gentium over a pixel chart". |
| **Shovel Knight** | A black bar across the top with pips and numbers; plain black dialogue boxes with a simple border. Virtual screen 400x240, tiles 16x16, most sprites 4 or 5 colours. | One 8x8 font, capitals. | A strict small rulebook, kept everywhere, looks more "designed" than any ornament. They broke the old rules only where it helped the player (a wider screen, four extra colours). |
| **Wargroove** | Bright bevelled buttons, ribbons, thick dark outlines on everything. | Big outlined pixel text. | Thick outlines make a UI readable over any map. Our slips sit over a bright chart, so the border must be the darkest colour we have. |

Two more worth knowing: **Celeste** draws its world at 320x180 and puts a smooth high-resolution font over it, on
purpose; **Dredge** (not pixel art, but our model for the clock) keeps one panel style for the whole game.

### A2. The frame: what a panel is made of

Every game in the table builds every box from **one small sprite cut into nine pieces**. This is the nine-slice (also
"nine-patch").

```
+----+--------+----+
| TL |  top   | TR |     the four corners are never stretched
+----+--------+----+
| L  | centre | R  |     the four edges stretch or repeat along one direction
+----+--------+----+
| BL | bottom | BR |     the centre stretches or repeats both ways
+----+--------+----+
```

- You draw **one tiny square**, for example 12x12 pixels with a 4 px margin on every side. The game makes a box of any
  size from it.
- **Stretch or tile.** A flat fill and a plain line can be stretched: a stretched line of one colour is still that line.
  Anything with a pattern in it (wood grain, a rope border, a dither) must be **tiled** (repeated), or it smears.
  Rule for you: keep the edges plain and they can stretch. Put all the ornament in the corners.
- **The margin** is the number of pixels from each side that belong to the corner. It has to be at least as big as
  the biggest thing in the corner.
- **The smallest box** the game can make is two margins wide and high. A 4 px margin gives a smallest box of 8x8.
- **Three-slice** is the same thing in one direction: left cap, middle, right cap. Bars, sliders and the day stick
  are three-slices.

The frame styles you will meet, from least to most work:

| Style | What it is | Who | Work |
|---|---|---|---|
| **Flat with a line** | one fill colour, one 1 px border | Shovel Knight, Sea of Stars, our ink slips | minutes |
| **Line plus inner line** | border, then a second line one step lighter or darker inside it, which reads as a bevel | Loop Hero, most RPG boxes | half an hour |
| **Hard drop shadow** | the same box again in a dark colour, moved 1 or 2 px down and right, no blur | many; your mockup's slips already have it | minutes |
| **Corner ornament** | a knot, a rivet or a leaf in each corner, plain edges | Moonlighter, Octopath | an evening |
| **Material** | wood, parchment, leather, with a tiled texture | Stardew, Wargroove | days, and it must be tiled |

You chose black see-through glass, a thin hard black line, square corners. In pixel terms that is **flat with a line,
plus a hard drop shadow**. It is the easiest row, and the one the quiet, well-loved UIs use.

### A3. Pixel fonts and their sizes

A pixel font is named by its grid: "5x7" means capitals are 5 pixels wide and 7 tall.

| Capital height | Typical name | Line height | Reads as | Use |
|---|---|---|---|---|
| 5 px | 3x5, 4x5 | 6 to 7 | capitals only, cramped | a count on an icon, a key cap. Never sentences. |
| **7 px** | **5x7** | **10 to 11** | **the workhorse: lower case works** | **labels, numbers, card text, the market** |
| 8 px | 8x8 | 8 to 10 | the old console look, wide | titles in a retro UI; too wide for long text |
| 9 to 10 px | 6x10, 7x9 | 13 to 14 | comfortable, room for serifs | long reading, headings |
| 12 to 16 px | display | 16 to 20 | a wordmark | the title, a card's name |

Rules that every game above keeps:

- **A pixel font has one true size.** It is drawn for one height and is sharp only at that height and at whole
  multiples of it (2x, 3x). A font file may tell you the size to ask for: m5x7 says "use 16, 32, 48". Any other size
  gives uneven strokes.
- **No smoothing.** Anti-aliasing puts grey pixels round every letter. Turn it off in Aseprite's text tool and in
  the engine.
- **Text sits on whole pixels.** A label at x = 10.5 is drawn blurred or with a wobbling letter. See A7.
- **Numbers the same width.** In a price list every digit must take the same room, or the columns jump. Check the
  font's "1".
- **Two sizes are enough.** One for text, one for titles. A third size (5 px) only for counts on icons.
- **Proportional, not monospace, for sentences.** "i" and "l" take 1 or 2 px, "m" and "w" take 5. Monospace wastes a
  third of the line.
- **The letters you need.** Kapman needs more than English: the macron in Kāpman, and for the old texts æ, þ, ð, ø,
  å, ö, ü, é, and the long vowels ā ē ī ō ū. Check a font for these before you fall for it.

Free fonts worth trying first (you will draw your own last, see B5):

| Font | Grid | Licence | Note |
|---|---|---|---|
| **m5x7** by Daniel Linssen | 5x7, ask for size 16 | CC0 | The standard small font of indie pixel games. Many accented letters. Check for ā, þ, ð `(?)`. |
| **monogram** by datagoblin | 5 px wide, monospace | CC0 | Wide language cover (the extended version has Greek and Cyrillic). Monospace, so better for tables than for prose. |
| **m6x11** by Daniel Linssen | 6x11 | free `(?)` | A taller sister of m5x7, good for titles. |
| **Pixelify Sans** | scalable | SIL OFL `(?)` | Already in your mockup. Note: it is a smooth outline font drawn to look like pixels. Its "pixels" land on the screen grid only at certain sizes, so at most sizes it is a sub-pixel font (see A7). Use a true bitmap font in the game. |

### A4. Icon grids

- **16x16 is the unit** for almost every game above: Stardew's items, Shovel Knight's tiles, Moonlighter's bag `(?)`.
  At 640x360 a 16 px icon is 4.4% of the screen's height, about the size of a thumbnail on a 1080p screen.
- **Draw inside 14x14.** Leave a 1 px empty rim so two icons side by side never touch.
- **The slot is bigger than the icon.** A 16 px icon sits in an 18 px or 20 px cell (the cell's own 1 px line, and
  sometimes 1 px of air).
- **8x8 for marks**, not pictures: a notch on the day stick, a mark on a list row, a pip.
- **One light, one outline rule.** Light from the top left on every icon. Either every icon has a dark outline or none
  has. On our dark slips, icons need **no** outline on the slip but **do** need one on the chart. So chart marks
  get a 1 px `night` outline and slip icons do not.
- **Silhouette first.** If the icon cannot be told in one colour, colour will not save it.
- **Three or four colours an icon**, from one ramp, plus at most one accent.
- **A row is a grid.** Slots line up on a pitch (18 or 20 px). The hold, the market and the crew all use the same pitch.

### A5. The cursor

- A pixel game draws its own pointer. The system arrow, smooth and white, over a pixel chart is the first thing a
  player sees that is wrong.
- **Size:** drawn in a 16x16 cell; the arrow itself is about 11x16. Shown at the same scale as the rest of the UI.
- **Two-tone:** a `night` outline round a `warm white` fill, so it shows on dark slips and on the bright sea alike.
- **The hotspot** is one named pixel: the tip of the arrow (0,0), the fingertip of the hand.
- **A few shapes, each meaning one thing:** arrow (nothing here), hand (you can press this), course (a port you can
  sail to), hold (dragging cargo), wait (the clock is running).
- **For keys and pad:** not a pointer but a **selection bracket**, four corner pieces round the chosen thing, pulsing
  in two frames. Moonlighter and Stardew both do this.

### A6. How big things are at 640x360 and at 960x540

Both sizes are sixteen by nine. The difference is how they fit real screens.

| Screen | 640x360 shows at | 960x540 shows at |
|---|---|---|
| 1280x720 | 2x, exact | 1.33x: **not whole** |
| 1366x768 (cheap laptops) | 2x with a small border | not whole |
| 1280x800 (Steam Deck) | 2x with bars top and bottom | not whole |
| 1920x1080 | 3x, exact | 2x, exact |
| 2560x1440 | 4x, exact | 2.67x: **not whole** |
| 3840x2160 | 6x, exact | 4x, exact |

640x360 lands on a whole number on every common screen. 960x540 is exact only on 1080p and 4K. That alone is the
reason most pixel games sit at 640x360 or below (Celeste 320x180, Shovel Knight 400x240).

The same sprite at the two sizes, on a 1920x1080 screen:

| Thing | Drawn (1x) | On screen at 640x360 (3x) | On screen at 960x540 (2x) |
|---|---|---|---|
| Capital letter, 5x7 font | 7 px high | 21 px | 14 px |
| One line of text | 11 px | 33 px | 22 px |
| Icon | 16x16 | 48x48 | 32x32 |
| Mark | 8x8 | 24x24 | 16x16 |
| Button | 16 px high | 48 px | 32 px |
| Hold cell | 18x18 | 54x54 | 36x36 |
| Crew portrait | 32x32 | 96x96 | 64x64 |
| Border line | 1 px | 3 px | 2 px |

How much fits:

| | 640x360 | 960x540 |
|---|---|---|
| Lines of text down the whole screen (11 px a line) | 32 | 49 |
| Letters across the whole screen (about 6 px a letter) | about 105 | about 160 |
| Letters a line in a slip one third of the screen wide | about 33 | about 50 |
| 16 px icons across the whole screen at an 18 px pitch | 35 | 53 |

What that means for Kapman:

- **640x360** gives big, calm text that reads on a Steam Deck and from a sofa. A slip one third of the screen wide
  holds about 33 letters a line, which is short: fine for a card (a fact and three choices), tight for a page of
  Beowulf. The reading slip must be wider, about half the screen (300 px, about 48 letters a line; books aim for 45
  to 75).
- **960x540** gives half as much again in every direction. It suits a desk and a text-heavy game, and it is too small
  on a handheld (a capital is 14 screen pixels on 1080p and under 10 on a Steam Deck, where it would not even scale
  cleanly).
- **You do not have to choose before you draw.** The sprites are the same pixels either way. Only the number the game
  multiplies them by changes, and the slips are laid out by rule, not by hand. I recommend we build for 640x360 and
  let a player with a big monitor pick the finer step in the options, in whole steps only.

**The chart under the UI.** The map's art is 120 m a pixel and the player zooms it. The zoom must go in whole steps of
the UI's pixel (one map pixel = 1, 2, 3, 4 UI pixels). Zoomed out past 1, the game should show a smaller drawing of
the map, not shrink the big one live. Otherwise the map's pixels and the slips' pixels are different sizes on the same
screen, which is the first item in A8.

### A7. Palette limits

| Game | The limit they kept |
|---|---|
| Shovel Knight | the NES's colours plus four of their own; 4 or 5 colours a sprite |
| Loop Hero | a very small dark palette, a few bright accents `(?)` |
| Hyper Light Drifter | a wide palette, but the UI uses two or three of its loudest colours only |
| Stardew Valley | no fixed limit; the UI stays inside one brown-and-cream ramp |

The pattern: **the UI uses far fewer colours than the world, all from the world's palette.** For Kapman, eight, all
already in `Design/palette_byzantium.gpl`:

| Role | Name | Hex | Where |
|---|---|---|---|
| Slip fill | night | `#0B0810` | the body of every slip (the game makes it see-through; you draw it solid) |
| Line | night, solid | `#0B0810` | the 1 px border and the drop shadow, never see-through |
| Well | dark purple | `#2E1233` | sunk places: hold cells, a pressed button, a list row under the pointer |
| Rule | plum | `#4A1C4A` | dividers, the inner line, disabled things |
| Edge | byzantium | `#702963` | the one accent on a frame: the chosen thing, the card's frame. Rare. |
| Quiet text | pale stone | `#AAA4AE` | second-rank text, units, ages of news |
| Text | warm white | `#F6EFE0` | all main text and icon lights |
| Yours | gold | `#D9A441` | what is yours: your silver, your ship, your port |

Signals, used only with a word or an icon beside them, never alone: good `#7F9A4A` (meadow), warning `#C9553A`
(rust), bad `#8F2D2D` (blood). About one man in twelve cannot tell red from green, so the odds on a card are
always the word ("sure", "likely", "doubtful") and the colour is extra.

Icons may use the whole 40-colour palette, three or four colours each.

### A8. What never to do

1. **Mixed pixel sizes.** A slip drawn at 3x beside an icon at 2x beside a map at 2.5x. The eye reads it at once as
   "pasted together". One screen, one pixel size. The only allowed break is a whole, declared layer (Octopath's smooth
   UI over a pixel world), never a mixture inside a layer.
2. **Scaling by a fraction.** 1.5x makes every other pixel two wide and the rest one wide. Lines thicken and thin as
   things move. Whole numbers only; fill the leftover screen with a border or a wider view of the chart.
3. **Soft shadows and glows.** A blurred shadow is hundreds of in-between colours and no hard edge. Use a hard shadow:
   the same shape in `night`, moved 1 or 2 px down and right.
4. **Sub-pixel text.** Text at half-pixel places, at a size that is not the font's own, or with smoothing on. Each
   one makes grey fringes.
5. **Gradients.** A smooth fade does not exist in pixel art. Use two or three flat bands, or a dither if you must.
6. **Rotating or resizing a sprite in the engine.** A turned sprite grows jagged, off-grid pixels. Draw each
   direction (or only use quarter turns and flips, which are exact).
7. **Stretching a patterned edge.** Tile it, or keep edges plain.
8. **Smooth movement of a panel at fractions of a pixel.** Slips should snap, or move in whole pixels.
9. **Colours from outside the palette.** One off-palette grey in a frame shows.
10. **Black outlines on everything by default.** Outlines are for things that sit on the chart.
11. **Saving as JPEG, or letting the engine compress or filter the image.** PNG only; point filtering; no compression.
12. **Tiny text to fit more in.** If it does not fit at 7 px capitals, cut words. That suits your own rule: plain,
    short labels.
13. **Round corners made by the engine** (a "radius" setting). A round corner is drawn, pixel by pixel, or it is square.
14. **A partly see-through border.** The fill may be see-through glass; the line stays solid.

---

## Part B. Your kit

### B1. Set up once (one evening)

**The master file.** `Art/UI/kit.aseprite`, 640x360, colour mode Indexed (Sprite > Color Mode > Indexed), the
Byzantium palette loaded. This file is a **mock screen**, not a sheet:

1. Bottom layer: a screenshot of the chart from `Captures/chart`, shrunk to 640x360 with nearest-neighbour
   (Sprite > Sprite Size, Method: Nearest-neighbor). Lock the layer.
2. Above it: grey boxes where the five slips go (the date, the wind, your boat, the place, what is said), from
   `Design/ui_mock/template2.html`.
3. Everything you draw is pasted in here to be judged, at 100% zoom, against the real chart.

**The sheets.** One file a family, so nothing is ever cut by hand:

| File | Holds |
|---|---|
| `ui_frames.aseprite` | every nine-slice and three-slice |
| `ui_controls.aseprite` | buttons, boxes, scroll parts, pointer, bracket |
| `ui_icons_16.aseprite` | every 16x16 icon except goods, on a 16 px grid |
| `ui_marks_8.aseprite` | every 8x8 mark |
| `ui_chart.aseprite` | ship marks, port marks, course, hazards |
| `ui_goods_16.aseprite` | the goods, on a 16 px grid, in the order of `goods_793.json` |
| `ui_faces_32.aseprite` | the crew |

**Settings.** Grid 8x8 (View > Grid > Grid Settings), pixel-perfect ticked on the pencil, the palette's eight UI
colours dragged to the front. Save as `.aseprite`; export PNG at 100% only, never bigger.

**Names.** Use the Slice tool (Shift+C), draw a box round each sprite and name it (`slip`, `button_normal`,
`icon_silver`). For a nine-slice, double-click the slice, tick 9-slices and drag the inner box to the margin. Export
with File > Export Sprite Sheet, with JSON data and Slices ticked. The names and the margins travel in the JSON, so I
read them and you never type a number for me.

### B2. Every sprite, with its size

Sizes are pixels at 1x. "9s 12x12 m4" means a nine-slice drawn 12x12 with a 4 px margin on each side. "3s" is a
three-slice. Stage is the order (B3).

**Frames**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 1 | Slip (the one panel) | 9s 12x12 m4 | 1 | 1 |
| 2 | Slip shadow (hard, drawn 2 px down and right by the game) | 9s 12x12 m4 | 1 | 1 |
| 3 | Well (sunk box: hold cell, list, text field) | 9s 12x12 m4 | 1 | 1 |
| 4 | Row highlight (the list row under the pointer) | 3s 12x12 m4 | 1 | 2 |
| 5 | Divider, plain | 8x1 tile | 1 | 1 |
| 6 | Divider with a middle knot | 24x5 | 1 | 8 |
| 7 | Tooltip box | 9s 9x9 m3 | 1 | 2 |
| 8 | Tooltip tail | 5x3 | 1 | 2 |
| 9 | Card frame (the slip with byzantium edge and drawn corners) | 9s 48x48 m16 | 1 | 8 |
| 10 | Card picture window | 9s 12x12 m4 | 1 | 8 |
| 11 | Odds chip (behind "sure", "likely", "doubtful") | 9s 9x11 m3 | 3 | 8 |
| 12 | Key cap (behind a key's letter in a hint) | 9s 11x12 m4 | 1 | 9 |

**Controls**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 13 | Button: normal, pointed at, pressed, off | 9s 16x16, margins 4 4 4 5 | 4 | 2 |
| 14 | Selection bracket (keys and pad), two frames | 9s 12x12 m4 | 2 | 2 |
| 15 | Close cross: normal, pointed at, pressed | 9x9 | 3 | 3 |
| 16 | Tick box: empty, ticked | 10x10 | 2 | 3 |
| 17 | Round choice: empty, chosen | 10x10 | 2 | 3 |
| 18 | Scroll track (upright) | 3s 6x12 m4 | 1 | 3 |
| 19 | Scroll thumb: normal, held | 3s 6x12 m4 | 2 | 3 |
| 20 | Slider track | 3s 12x4 m4 | 1 | 3 |
| 21 | Slider knob | 7x10 | 1 | 3 |
| 22 | Meter frame and fill | 3s 12x6 m3, fill 1x4 | 2 | 3 |
| 23 | Pips: full, half, empty | 5x5 | 3 | 3 |
| 24 | Arrows: up, down, left, right | 7x7 | 4 | 3 |
| 25 | Speed: hold, 1, 2, 4 (normal and chosen) | 12x12 | 8 | 6 |

**The pointer**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 26 | Arrow (hotspot 0,0) | in 16x16 | 1 | 2 |
| 27 | Hand, pointing (hotspot at the fingertip) | in 16x16 | 1 | 2 |
| 28 | Course (over a port you can sail to) | in 16x16 | 1 | 5 |
| 29 | Hold: open hand, closed hand (dragging cargo) | in 16x16 | 2 | 7 |
| 30 | Wait, four frames (the clock is running) | in 16x16 | 4 | 6 |

**The tally (bottom left)**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 31 | Silver (a penny) | 16x16 | 1 | 4 |
| 32 | Debt | 16x16 | 1 | 4 |
| 33 | Stores (days aboard) | 16x16 | 1 | 4 |
| 34 | Crew | 16x16 | 1 | 4 |
| 35 | Hold cell: empty, full, cannot go here | 18x18 | 3 | 7 |
| 36 | Hull outline behind the hold grid (the Gislinge from above) | about 112x36 | 1 | 7 |
| 37 | The chest: shut, open | 24x24 | 2 | 6 |

**The day stick (top)**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 38 | Stick: left end, right end | 8x16 | 2 | 6 |
| 39 | Stick: one day (tiles sideways) | 8x16 | 1 | 6 |
| 40 | Stick: a seventh day, with a deeper notch | 8x16 | 1 | 6 |
| 41 | Stick: a day already cut (past) | 8x16 | 1 | 6 |
| 42 | Today (a thread tied round the stick) | 9x18 | 1 | 6 |
| 43 | Day marks: debt due, fair, feast, sea closes, your own note | 8x8 | 5 | 6 |
| 44 | Moon: eight phases | 8x8 | 8 | 6 |
| 45 | Seasons: spring, summer, autumn, winter | 16x16 | 4 | 6 |

**Wind and weather (top right)**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 46 | Wind arrow, 8 directions (draw 2, turn and flip the rest) | 16x16 | 2 drawn, 8 used | 5 |
| 47 | Wind strength pips, 5 steps (reuse the pips, #23) | 5x5 | 0 new | 5 |
| 48 | Weather: clear, cloud, rain, fog, gale, thunder, snow, frost | 16x16 | 8 | 6 |
| 49 | Tide: rising, falling, slack | 16x16 | 3 | 6 |
| 50 | Sea: calm, chop, rough, high | 16x16 | 4 | 6 |

**On the chart**

Marks that sit on a point have an **odd** size, so there is a true middle pixel. Each has a 1 px `night` outline.

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 51 | Your ship, 8 headings (draw 3, flip the rest), 2 frames each | 15x15 | 6 drawn, 16 used | 5 |
| 52 | Other sails: trader, stranger, raider, king's man | 9x9 | 4 | 5 |
| 53 | Port: landing, town, great market | 5x5, 7x7, 9x9 | 3 | 5 |
| 54 | Port states: not yet seen, heard of, visited, yours (a palette swap of #53) | as #53 | 0 new | 5 |
| 55 | Chosen ring, two frames | 13x13 | 2 | 5 |
| 56 | Course dot and course dash | 1x1, 3x1 | 2 | 5 |
| 57 | Journey's end (a small flag) | 7x9 | 1 | 5 |
| 58 | Hazards: shoal, bar, wreck, ice | 7x7 | 4 | 6 |
| 59 | Fog edge, four strengths (tiles) | 16x16 | 4 | 9 |
| 60 | News mark on a port: fresh, old, stale | 5x5 | 3 | 6 |

**In port**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 61 | What to do: town and market, talk, cast off, toll, exchange, church, rest, wait for the tide, mend | 16x16 | 9 | 7 |
| 62 | Ways to speak: plain, warm, hard | 16x16 | 3 | 7 |
| 63 | Buy, sell, weigh | 16x16 | 3 | 7 |
| 64 | Price going up, going down, steady | 7x7 | 3 | 7 |
| 65 | Goods, family icons: grain, drink, salt, fish and meat, spice, beasts, cloth, wool and dye, fur and hide, bone and horn, stone, pots and glass, gems, metal, arms, church things, ship's stores, other | 16x16 | 18 | 7 |
| 66 | Goods, one each (see B4) | 16x16 | 132 | 10 |

**People**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 67 | Face frame (a well with room for a 32 px face) | 36x36 | 1 | 9 |
| 68 | A face: you, four crew, and one base head to build others from | 32x32 | 6 | 9 |
| 69 | Mood: content, uneasy, at the end of his patience | 7x7 | 3 | 9 |

**The front of the game**

| # | Sprite | Size | Count | Stage |
|---|---|---|---|---|
| 70 | Wordmark "Kāpman" | about 160x40 | 1 | 11 |
| 71 | Mouse: left, right, wheel | 11x15 | 3 | 9 |
| 72 | Small digits for counts on icons (0 to 9, x, the dot) | 3x5 each | 12 | 4 |
| 73 | Your own font (see B5) | 5x7 capitals in a 16 px line | about 140 letters | 12 |

**The count.** Without the goods and the font: about 200 sprites drawn (frames 14, controls 35, pointers 9, tally 10,
day stick 23, wind and weather 17, chart 29, port 36, people 10, front 16). Stages 1 to 4 (a working interface) are
about 50 of them, most a few pixels each, and 12 of those are the small digits.

### B3. The order, and why

Each stage needs only the skill of the stage before it, and each ends with something the game can show.

| Stage | You draw | Skill it teaches | Evenings | The game after it |
|---|---|---|---|---|
| **0** | the mock screen with grey boxes | none: layout before art | 1 | nothing yet, but every later choice is judged here |
| **1** | slip, shadow, well, divider | straight lines, the nine-slice | 1 | every panel wears your frame |
| **2** | button in 4 states, bracket, row highlight, tooltip, arrow and hand pointers | states: the same thing, changed by one step | 2 | it can be clicked, with your pointer |
| **3** | the small machine parts: cross, tick, scroll, slider, meter, pips, arrows | drawing at 5 to 10 px, where every pixel is a choice | 2 | lists scroll, options work |
| **4** | the four tally icons, the small digits | the 16x16 icon; silhouette first | 2 | the tally strip is yours |
| **5** | ship marks, sails, ports, ring, course, flag, wind arrows, course pointer | outlines over a busy ground; turning a shape | 3 | the chart's marks are yours |
| **6** | the day stick and its marks, moons, seasons, weather, tide, sea, hazards, chest, speed, wait pointer | tiling; a family of icons that match | 4 | the whole table of section 4.1 is yours |
| **7** | port icons, ways to speak, buy and sell, the 18 goods families, hold cells, hull, hold pointers | objects with a material: cloth, wood, metal, in 4 colours | 4 | the market and the hold are yours |
| **8** | card frame, picture window, odds chips, knot divider | ornament in a corner; restraint | 2 | cards are yours |
| **9** | face frame, six faces, moods, key cap, mouse, fog tiles | faces at 32x32 | 4 | the crew have faces |
| **10** | the goods, a family at a time | stamina, and a house style | ongoing | each family shows up as it is done |
| **11** | the wordmark | lettering | 2 | the title |
| **12** | your own font | spacing; the hardest thing in the kit | 5 or more | every word is yours |

How this sits beside the twelve-week plan in `Design/pixel_art_zero_to_hero.md`: stages 0 to 4 need only week 1 (clean
lines) and can run beside weeks 1 and 2 as the warm-up of each sitting. Stage 7 wants week 4 (light). Stage 9 is the
same work as week 6 (faces in a few pixels), so do them together.

### B4. The method for each kind

**A nine-slice (the slip, #1).** Ten minutes.

1. New sprite, 12x12. Fill it with `night`.
2. That is nearly it: the border is also `night`, so against the chart the slip is a dark box with a hard edge.
   To keep the edge visible where slip meets dark sea, draw the inner line: one pixel in from the edge, a 10x10
   outline in `plum`.
3. Leave the four corner pixels of the inner line empty (`night`). That one missing pixel softens a square corner
   without rounding it.
4. Slice tool (Shift+C): box the whole 12x12, name it `slip`, tick 9-slices, set the inner box 4 px in on every side.
5. Paste it into the mock screen as a 200x120 box (draw it by hand once, to see it) over the chart. Look at 100%.
6. The shadow (#2) is the same 12x12 filled solid with `night` and nothing else. The game draws it first, 2 px down
   and right.

The well (#3) is the slip turned inside out: fill `dark purple`, a `night` line along the top and left (the shadow
of the rim), a `plum` line along the bottom and right.

**A button in four states (#13).**

1. Draw normal: 16x16, fill `dark purple`, 1 px `night` border, a `plum` line under the top border (the lit lip),
   and the bottom two rows `night` (the button's thickness).
2. Copy it three times, side by side.
3. Pointed at: change the lip from `plum` to `byzantium`. Nothing else.
4. Pressed: remove the thickness (one `night` row at the bottom instead of two) and move the face 1 px down. That is why
   the bottom margin is 5 and not 4: the game moves the label down with it.
5. Off: fill `night`, border `plum`, no lip.
6. Test: flip between the four with the frame keys. If the change is easy to miss, it is right for the pointed-at
   state and wrong for pressed; pressed must be felt.

**A 16x16 icon (the silver, #31).**

1. Thumbnails first: six tries on paper or at 16x16 in one colour, two minutes each. Pick the one a stranger names
   first.
2. Block the silhouette in a middle colour inside the 14x14 area.
3. Light from the top left: one lighter colour on the upper left edge, one darker on the lower right.
4. One accent or highlight pixel, at most two.
5. No outline (it lives on a dark slip). If it is lost against `night`, lighten the darkest colour by one step of the
   ramp instead of adding an outline.
6. Paste it on the mock screen beside its number. Look at 100% from an arm's length.
7. Period check for this one: the penny of 793 carries the king's name, CARLVS REX FR, on one face and the mint, for
   example DORESTADO, on the other. At 16x16 that is a pale disc with a ring of dots for the lettering. Do not
   draw a head on it, and do not draw a stack of gold coins.

**A set of icons that match (weather, #48).** Draw all eight as silhouettes before colouring any. Keep one thing
the same in all of them (the same cloud shape, the same horizon line). Colour them in one sitting with the same three
colours where you can.

**A mark on the chart (your ship, #51).**

1. 15x15, so there is a middle pixel; the game puts that pixel on her position.
2. Draw north first: a hull seen from above, a square sail across it. Then north-east, then east. These three are the
   only drawings.
3. The other five: flip north-east sideways for north-west; turn east a quarter for south (Edit > Rotate > 90,
   which is exact), and so on. Never turn by 45 degrees in the program: draw the north-east by hand.
4. Outline every one with 1 px of `night` (Edit > FX > Outline, Shift+O, then tidy by hand).
5. Hull `timber`, sail `warm white`, one `gold` pixel at the masthead: gold is "yours".
6. Second frame: the sail one pixel fuller. Two frames at about two a second is enough life.
7. Test on three grounds in the mock screen: open sea, the pale flats, dark forest.

**Ports (#53, #54).** Three sizes of one plain shape (a square set on its corner reads at 5x5; a circle does not).
The four states are the same pixels in four colourings: not yet seen (`slate`), heard of (`pale stone`), visited
(`warm white`), yours (`gold`). Draw one colouring; the game swaps the rest.

**A strip that tiles (the day stick, #38 to #42).**

1. Sprite 24x16: left end, one day, right end, each 8 px wide.
2. Turn on View > Tiled Mode > Tile in X while you draw the day piece, so you see it repeat as you work.
3. The stick: `timber` body 8 px thick in the middle of the 16 px height, a lighter top edge, a darker under
   edge. The notch is a 1 px wide, 3 px deep cut in `peat` at the same x in every day.
4. The seventh day: the notch 5 px deep. The cut (past) day: the same tile one step darker.
5. No wood grain that runs across the join. If you want grain, two or three short dashes that stay inside the tile.
6. Today's thread (#42) is drawn over the stick in `byzantium`: your key colour, used once on the whole screen.
7. This is a device of the game, not a find of 793 (the concept says so already): keep it plain, with no carved runes.

**The card frame (#9).**

1. 48x48, margin 16. Edges plain: `night` border, a `byzantium` line inside it.
2. All the drawing goes in the 16x16 corners. Draw one corner, then copy and flip it into the other three
   (Edit > Flip Horizontal, Flip Vertical).
3. Corner idea: one knot or one small leaf, three colours. Look at page borders in the Utrecht Psalter and other books
   of the decade for shapes; do not copy a famous page.
4. Check the smallest card the game will make (a title and two choices) and the biggest. The frame must not crowd the
   small one.

**A face at 32x32 (#68).**

1. Draw the base head once: three-quarter view, the head fills the square from the brow to the collar, light from
   the top left, five colours of skin and shadow.
2. Save it. Every other face starts as a copy.
3. Change only three things a man: the hair and beard shape, one piece of clothing at the neck (a hood, a cloak pin,
   a cap), and the brow line. That is how a crew of four reads as four men.
4. Eyes are two dark pixels each with one light pixel. Do not draw a white of the eye at this size.
5. Test them in a row at 100%. If two could be brothers, change a silhouette, not a colour.

**The goods (#66), 132 of them.** Not a list to finish before the game works. The game shows the family icon (#65)
until a good has its own.

1. Draw the 18 families first (stage 7).
2. Then the goods of the first twelve ports: the ones a player sees in the first hour (wheat, Rhenish wine, sea salt,
   herring, Frisian cloth, quernstones, Badorf ware, glass vessels, bar iron, whetstones, honey, amber, furs).
3. Then one family a sitting. Within a family keep the container the same and change the mark: every wine is the same
   cask with a different band; every cloth is the same bolt in a different colour. That is 132 icons from about 30
   drawings.
4. Things that look alike (three salts, five spices) are told apart by colour and by the word beside them. Do not try
   to make cumin and cloves different shapes at 16x16.

**The pointer (#26).**

1. 16x16, clear ground. The tip is the top-left pixel.
2. Draw the arrow's edge in `night`: straight down the left side for 11 px, a clean diagonal on the right, a short
   tail.
3. Fill with `warm white`.
4. Export the pointers at 2x, 3x and 4x as well (this is the one place you export bigger, with nearest-neighbour):
   the system draws the pointer itself and will not scale it for us.

### B5. Your own font, last

Until stage 12 the game uses a free bitmap font (A3). When you are ready:

1. **The grid.** Capitals 5 wide, 7 tall. Small letters 5 tall with 2 px above for b, d, h, k, l and 2 px below for
   g, j, p, q, y. Line height 11 (7 + 2 below + 2 of air). One pixel between letters, three for a space.
2. **Draw in this order:** `n o H O` first: they fix the widths and the roundness of everything else. Then
   `a e s`, the hardest three at this size. Then the rest of the small letters, the capitals, the digits (all 5 wide,
   the 1 as well), then the marks.
3. **The extra letters Kapman needs:** ā ē ī ō ū (a 1 px bar on top: this is why there are 2 px above the small
   letters), æ Æ þ Þ ð Ð ø å ö ü é, the long dash, both kinds of quote marks, the multiplication sign for "x4".
4. **Test words, not letters.** Type "Dorestad", "Kāpman", "480 pennies", "likely", "Hæthum" in the mock screen. Fix
   the pairs that touch or gape.
5. **Make it a font.** Draw the letters as one PNG sheet in Aseprite, then turn the sheet into a .ttf with a pixel
   font converter (YellowAfterlife's free Pixel Font Converter does exactly this `(?)`), or draw it straight in
   FontStruct or BitFontMaker2. I take the .ttf from there.
6. A second, taller face (capitals 9 or 10) for titles and long reading is a later job, built from the first.

### B6. What I do with your files (so you know what not to worry about)

- Import: point filter, no compression, no mipmaps, one pixels-per-unit number for every UI image.
- The nine-slice margins come from your slices. The game's UI system (UI Toolkit) takes them as
  `-unity-slice-left`, `-right`, `-top`, `-bottom`, with `-unity-slice-scale` for the UI's pixel size, and
  `-unity-slice-type: tiled` for a patterned edge. One trap is mine to mind: for sprites, Unity rescales the slice by
  the sprite's pixels-per-unit against the panel's reference (100 by default).
- Scale: the panel is laid out at 640x360 and multiplied by the biggest whole number that fits the window. Leftover
  screen becomes more chart, not bigger pixels.
- Text: the font is rendered as a bitmap at its own size, with no smoothing, and placed on whole pixels.
- The slip's see-through glass is one flat alpha on the fill. The border and the shadow stay solid.
- The pointer: your exported sizes, picked to match the scale.
- Colour swaps (port states, seasons) are done by the game from one drawing.

---

## Your three decisions

1. **The UI's pixel size: 640x360 or 960x540.** I recommend 640x360 (whole numbers on every screen, readable on a
   handheld), with the finer step as an option on big monitors. The drawing is the same either way.
2. **The reading font.** Today the plan is Pixelify Sans for labels and Gentium for reading. On a pixel chart, Gentium
   is a smooth font over pixels: allowed only as a declared second layer (Octopath, Celeste, Dave the Diver do it), and
   then it should be the font for **all** text, with pixel icons and frames. The other road is a bitmap font
   everywhere, and a "plain font" switch in the options for long texts, as Loop Hero has `(?)`. I recommend the
   bitmap font everywhere for the table, and trying both for the chest's long texts side by side before you
   choose, because you turned down one hard-to-read font already.
3. **How much ornament.** The kit above is plain: line, inner line, hard shadow, ornament only on the card's corners
   and the knot divider. That fits "ink slips" and is the fastest to draw. If you want wood or parchment later, only
   the frames file changes.

## Sources

Checked this session:

- Shovel Knight (400x240 virtual screen, 16x16 tiles, four added colours, 4 to 5 colours a sprite, sprites drawn over
  the HUD): Yacht Club Games, "Breaking the NES", https://www.yachtclubgames.com/blog/breaking-the-nes
- Stardew Valley (UI scale 75% to 150% and zoom 75% to 200%, in 5% steps): https://stardewvalleywiki.com/Options
- Hyper Light Drifter (no dialogue, pictures in its place, 8-bit and 16-bit homage, GameMaker):
  https://en.wikipedia.org/wiki/Hyper_Light_Drifter
- Sea of Stars (first six months on lighting that matched the pixel art): https://en.wikipedia.org/wiki/Sea_of_Stars
- Loop Hero (GameMaker; praised for its 1980s look): https://en.wikipedia.org/wiki/Loop_Hero
- m5x7 (CC0; use sizes 16, 32, 48): https://managore.itch.io/m5x7
- monogram (CC0; 5 px wide, monospace; extended language cover): https://datagoblin.itch.io/monogram
- Aseprite slices (Shift+C, 9-slice inner box, pivot, exported in the JSON): https://www.aseprite.org/docs/slices/
- Unity UI Toolkit slice properties, and the pixels-per-unit note for sprites:
  https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html
- Unity 2D Pixel Perfect (one pixels-per-unit for all sprites, point filter, no compression, reference resolution):
  https://docs.unity3d.com/Packages/com.unity.2d.pixel-perfect@5.0/manual/index.html
- The penny of 794 (CARLVS REX FR on one face, the mint, for example DORESTADO, on the other; about 1.7 g of silver):
  https://en.wikipedia.org/wiki/Carolingian_monetary_system
- Dorestad's mint: https://en.wikipedia.org/wiki/Dorestad

From this project: `Design/palette_byzantium.gpl` (all hex values above), `Design/pixel_art_zero_to_hero.md`,
`Design/research_indie_and_concept.md` section 4.1 (the day stick, the tally, the chest),
`Design/research_pixel_map_3d_boat.md` section 4 (the two screen sizes), `Design/ui_mock/template2.html` (the five
slips), `Assets/Kapman/Game/Data/goods_793.json` (132 goods).

Not checked this session, marked `(?)` in the text: the screen size of Hyper Light Drifter; that Stardew's font
pixels are finer than its world's; Loop Hero's palette and its plain-font option; Moonlighter's bag size and icon
size; Sea of Stars's text; the licences of m6x11 and Pixelify Sans; whether m5x7 has ā, þ and ð; the Pixel Font
Converter. What is said of Eastward, Dave the Diver, Octopath Traveler, Wargroove and Celeste is from my own
knowledge of the games and was not checked against a page this session.
