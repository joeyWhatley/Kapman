# Kapman art guide

For the one who draws the pictures. Each picture is a slot with a fixed name and size (index.html, art_list.csv).
This page says how each kind is meant to be drawn. It is copied into the Desktop folder as ART GUIDE.md every time
make_art_list.py runs. The source is Kapman2/Design/art_guide.md.

## 1. What holds for every picture

- **Size.** The game is drawn for a screen of 640 x 360 pixels and shown three times as large on 1920 x 1080 (twice
  as large on 960 x 540). So one pixel of yours is a 3 x 3 square on the screen. Draw at the slot's own size and
  never scale a picture by a part number.
- **Colours.** Only the 40 colours of palette_byzantium.gpl. No blur, no soft edge, no half-clear pixel: a pixel is
  one of the 40 or it is clear. The empty pictures in _templates have the colours loaded.
- **Light.** From the top left, in every picture. A raised edge is light on top and left, dark at the bottom and
  right. A sunk edge is the other way.
- **Shadow leans to purple, light to gold.** The dark end of every ramp is drawn toward the purple ramp and the
  light end toward warm white. Under a chin, under a roof, under a hull: plum or dark purple, not grey.
- **Byzantium is rare.** It means "this is yours" or "this is on": the chosen button, the band on your own sail, a
  king's cloak. If it is everywhere it says nothing.
- **No letters.** The game sets every word. A picture with writing in it cannot be turned into another tongue.
- **Judge it small.** Look at it at its real size and at three times that. Then fill it with black: if the outline
  still says what it is, it works.
- **Save** as PNG under the slot's own name in its own folder, at the same size. Leave out the placeholder's corner
  mark and name strip.

## 2. The 40 colours

Eight ramps, each for one kind of stuff.

| Ramp | Dark to light | For |
|---|---|---|
| Purple | night, dark purple, plum, byzantium, mauve, lilac | the frame, shadow, dusk, what is yours |
| Sea | abyss, deep sea, sea, shoal, shallows, wave light, foam | water |
| Land | wood shade, forest, grass, meadow, reed light | grass, marsh, reeds, woods |
| Earth | peat, wet earth, timber, dry earth, sand, pale sand | mud, hulls, timber, thatch, sand, skin, undyed wool |
| Stone | basalt, slate, stone, pale stone | stone, iron, grey cloth, a thing switched off |
| Warm | blood, rust, gold, pale gold | fire, blood, coin, what you can press or spend |
| Sky | dusk rose, dawn peach, morning gold, day sky | sky and its light on things |
| White | frost, snow, cool white, warm white | sails, foam, snow, the lightest light |

Use three or four steps of one ramp for one thing. A small icon needs two or three colours in all. A portrait
needs about six ramps and no more.

### Pairs that some players cannot tell apart

About one man in twelve sees red and green poorly. To such an eye these pairs are one colour. The figures are how
far apart the two look (under 5: one colour; under 10: told side by side, not named alone). They were worked out
on the 40 colours by Bake/art/check_art_colours.py, with the same sums as the research in
Design/research/next/art.md, part B.

| Pair | To normal sight | To red-blind or green-blind sight | What it means |
|---|---|---|---|
| byzantium and sea | 21.9 | 1.1 | each purple has a blue twin |
| plum and deep sea | 15.7 | 2.4 | |
| mauve and shoal | 27.4 | 2.3 | |
| lilac and shallows | 32.4 | 2.5 | |
| dark purple and deep sea | 12.6 | 3.5 | |
| grass and rust | 47.2 | 1.1 | each green has a red or brown twin |
| forest and wet earth, blood, timber | 27 to 43 | 2.6 to 2.8 | |
| wood shade and peat | 17.9 | 2.5 | |
| grass and dry earth | 28.4 | 3.8 | |
| timber and blood | 16.5 | 2.3 | reds and browns of one lightness |
| dry earth and rust | 15.8 | 3.3 | |
| wet earth and blood | 18.4 | 3.8 | |
| sand and dusk rose | 12.4 | 2.9 | pale warm colours |
| pale sand and dawn peach | 10.6 | 3.4 | |
| foam and cool white | 6.7 | 2.0 | pale cold colours |
| wave light and frost | 8.3 | 2.2 | |
| snow and cool white | 8.4 | 3.2 | |

Close to every eye: pale sand and morning gold (3.3), foam and snow (4.3), night and abyss (4.9), abyss and basalt
(6.6), dark purple and plum (6.8), deep sea and sea (7.5). Use each such pair as the shade of one thing, never as
two things.

What to do about it:

1. **Never let colour alone carry a meaning.** Two things that mean different things differ in shape or in
   lightness as well. On and off, good and bad, yours and theirs.
2. **Every mark on the chart has two tones:** a light middle and a dark edge. Byzantium on the sea is not there to
   a red-blind eye (1.1). Warm white reads on sea (10.1) and fails on sand (2.2). Night reads on sand (7.7) and
   fails on sea (1.7). Together they read on both.
3. **Good is reed light, bad is rust.** Not meadow against rust (5.2 to a green-blind eye). Blood is for pictures
   only: on night its contrast is 2.4 and a sign in it cannot be read. Give every such sign a shape of its own.
4. **A red thing on a green ground needs an edge** that is darker or lighter than both: a red cloak on grass, a
   rust sail before a wood.
5. **On a window (dark purple)** a mark that must be read is gold or lighter: warm white 14.7, pale gold 11.4,
   gold 7.5, pale stone 6.9, lilac 5.7. Stone (3.6) and mauve (3.0) are for edges and for things switched off.
   On a button that is on (byzantium) use pale gold (6.5), not gold (4.3).

## 3. Each kind of picture

### The frame: frames, bars and buttons (hud, 5 to 84 px)
- **For:** the Command frame that is on screen all evening: the thick frame, the bars, the windows, the buttons.
- **Size and why:** each piece is as small as it can be, because the game pulls it out to the size it needs. A
  piece "cut in nine" keeps its four corners and stretches its sides and its middle.
- **Colours:** the purple ramp. Fill dark purple, edge light mauve, edge dark night. Byzantium for a thing that is
  on. Gold only for the panel in focus and for what is pressed or spent.
- **Light:** top left. That is the whole look: light edge on top and left, dark edge at the bottom and right.
- **Edge:** 2 px on windows and function buttons, 1 px on small things. One outermost line of warm white on
  anything that lies over the chart, or it is lost on dark water.
- **Ground:** the middle of a window is one flat colour. A hollow frame is clear in the middle.
- **What spoils it:** a pattern or dither on a side that stretches (it smears). Two states of one button whose
  outlines differ by a pixel (it jumps when pressed). Texture in the fill (the words on it get hard to read).

### Marks (hud, 5 to 12 px)
- **For:** the arrows, crosses, pips, clock marks and the marks of how a price is known.
- **Colours:** one, or two. Pale gold or gold; soft grey (stone) for the cross and the lock.
- **Light:** none. They are flat.
- **Edge:** none. At this size the shape is the whole picture.
- **Ground:** clear.
- **What spoils it:** an outline round a 5 px shape. A hollow arrow. Marks of one set drawn with different
  weights of line.

### Icons of the frame (hud, 16 x 16)
- **For:** the eight function buttons, the nineteen panels, the five stats, the eight chart tools, the nine kinds
  of wiki page. 50 in all.
- **Size and why:** 16 x 16 is what fits on a button at this screen size. Keep the thing inside 12 x 12 with 2 px
  clear all round.
- **Colours:** two or three of the gold ramp for things you press. Quieter (lilac, pale stone) for the wiki's.
- **Light:** one lighter pixel row on the top left side of a mass, if any.
- **Edge:** none round the whole. A dark pixel only where two parts of the thing must be parted.
- **Ground:** clear.
- **What spoils it:** two icons with the same outline (several placeholders share a sign: each description says
  which). Too much in one: a boat and a man and waves. A clock face, a compass, a cross: none of them is of 793 or
  of every faith.

### The cursor and the marks on the chart (ui, 5 to 24 px; ship_mark 32 x 16; port_mark 16 x 16)
- **For:** the pointer, the ring round your boat, the course, pins, sails, ports, landmarks.
- **Size and why:** small, so the chart under them stays readable. A port's mark is 16 x 16; nothing else on the
  chart is as large, so nothing else is taken for a port.
- **Colours:** two tones, always: a light middle (warm white, pale gold) and a dark edge (night). Then one colour
  to say whose or what.
- **Light:** none.
- **Edge:** a dark edge of 1 px all round. This is the one place where a full outline is right.
- **Ground:** clear.
- **What spoils it:** one flat colour (it vanishes on half the chart). Detail under 2 px. A ship's mark that does
  not show which way she heads.

### Portraits (64 x 80)
- **For:** the portrait block top right, and beside a person's words.
- **Size and why:** a fifth of the screen's height. Head and shoulders, facing us. The head is about 30 px wide.
- **Colours:** skin from the earth and sky ramps (dry earth, dusk rose, dawn peach, morning gold). Hair from earth
  or stone. Cloth by rank: undyed, brown and grey for the poor; blue and russet for craftsmen and crew; strong red,
  green and yellow for merchants and reeves; fur and gold for counts and bishops. Byzantium only on a bishop's
  silk, a king's cloak, and a thin band at the neck of your own house.
- **Light:** top left, plain and even. The row under the chin is plum.
- **Edge:** each part is edged with the dark step of its own colour. One pixel of night only where the figure
  meets the ground.
- **Ground:** full, and flat: one colour, or two that fall from light to dark. No scene.
- **What spoils it:** a black line round everything. A face that needs colour to say who it is. Teeth, lashes and
  cloth patterns: they turn to noise at this size.

### Harbours (640 x 180)
- **For:** the top of a port's window when she comes in. One view of each port from the water.
- **Size and why:** the full width of the screen and half its height: a wide low strip, as a shore looks from a boat.
- **Colours:** sky ramp above, sea ramp below, earth and land between. Roofs and timber from the earth ramp. Keep
  the far shore two steps lighter and greyer than the near one.
- **Light:** top left. Choose one season and one hour and say it with the sky.
- **Edge:** none. Shapes are parted by lightness.
- **Ground:** full.
- **What spoils it:** drawing every house (count the roofs to show size, then stop). People as detail: a man is 4
  to 6 px tall here. Buildings of her later years.

### Goods (32 x 32)
- **For:** the Trade and Hold windows: one unit of the good as it is traded, in a row of others.
- **Size and why:** an icon large enough for a cask's hoops or a sack's tie, small enough for twelve in a list.
- **Colours:** the ramp of what it is made of, three or four steps. It lies on a window's dark purple, so keep the
  body in the middle and light steps.
- **Light:** top left: a light side, a dark side, a plum shadow under it if it stands.
- **Edge:** the dark step of each colour. Not a black line all round.
- **Ground:** clear.
- **What spoils it:** two goods of one kind with one outline (wine and ale are both casks: change the shape, not
  only the colour). A table or floor under it. Letters or a brand.

### Ships from the side (160 x 96) and from above (32 x 16)
- **For:** the Ship window and the shipyard (side); the chart (above).
- **Size and why:** every class at 6 px to the metre from the side, so two ships can be set side by side and
  compared. A man aboard is about 10 px tall. From above, 1.25 px to the metre: your own boat is 10 x 4 px.
- **Colours:** hull from the earth ramp, darkest at the waterline (tar). Sail from the whites, or striped.
- **Light:** top left: the sail's left edge and the top strake catch it.
- **Edge:** the dark step of the hull; from above, a dark edge all round (it is a chart mark).
- **Ground:** clear, but for the line of the water from the side.
- **What spoils it:** a hull at another scale than her sisters. Rigging in lines thinner than a pixel can be.
  Bow to the left (it is always to the right).

### Plates (640 x 360), endings and scenes (320 x 180), cards and stories (192 x 108)
- **For:** the whole screen (title, the first morning); the middle of the screen (an ending, a site of a port);
  the top of a card.
- **Size and why:** the whole screen, half of it each way, and three tenths of its width.
- **Colours:** all eight ramps may meet here, but one ramp should rule a picture: the sea ramp at sea, earth in
  a hall, purple at dusk.
- **Light:** top left, and the hour the words give.
- **Edge:** none.
- **Ground:** full.
- **What spoils it:** telling the whole story (a card's picture sets a place and a mood; the words tell). Faces in
  detail: a head is 6 to 8 px on a card. No quiet part for the game's words on a plate.

### The rest, when you come to them
- **Icons of acts, games, upgrades and what the house can win (32 x 32):** as goods. One thing, clear ground.
- **Works (64 x 64):** a small full picture of one thing built. Light top left.
- **Leaves (48 x 48):** a rune stick, a leaf, a book or a wax tablet, on a clear ground. No letters that can be read.
- **Ships to be had (192 x 108):** as a card: the ship on her beach.
- **The ship window's views (160 x 96):** a stand-in. The window is to become a live 3D view. Do not draw them first.
- **Chart tiles (4096 x 4096):** the map itself. They have their own colours by people and their own rules.

## 4. A first week: ten pictures

Your course (Design/pixel_art_zero_to_hero.md) spends week 1 on pixel control (straight lines, clean curves, no
doubled pixels) and week 2 on shapes and two-step shading. These ten need exactly those two skills. All are small.
All are on screen in the first minute of the game. And after them the look of the frame is decided, so the hundred
pieces that follow copy choices you have already made.

| Day | File | Size | Why this one |
|---|---|---|---|
| 1 | hud/hud.pip_full.png | 5 x 5 | The smallest round shape there is. Two colours. If this dot is clean, your curves are. Draw hud.pip_empty beside it. |
| 1 | hud/hud.arrow_right.png | 5 x 7 | A solid shape with two slopes. Flip it for hud.arrow_left, turn it for hud.arrow_pick. |
| 2 | hud/hud.win_close.png | 7 x 7 | Two diagonal lines of 1 px that cross. Then hud.win_add: the same with straight lines. |
| 2 | hud/hud.small_button.png | 16 x 10 | Your first piece cut in nine: round ends, a 1 px edge, a flat middle. It is the commonest button in the game. Make hud.small_button_on from it by changing colours only. |
| 3 | hud/hud.frame_window.png | 24 x 24 | The bevel: light on top and left, dark at the bottom and right. Every panel wears it. Here you choose the corner's rounding and the edge colours for the whole frame. |
| 4 | hud/hud.fn_button.png | 36 x 24 | The same bevel on a button. Then press it (hud.fn_button_down: the edge colours change sides) and switch it off (hud.fn_button_off: grey, flat). |
| 5 | hud/hud.fn.sound.png | 16 x 16 | Your first icon: a weight on a line going into water. A line, a block, two marks of water. Test it filled with black. |
| 5 | port_mark/portmark.dorestad.png | 16 x 16 | Your home port on the chart. The two-tone rule: a light middle, a dark edge. Try it on sea and on sand. |
| 6 | ship_mark/shipmark.gislinge.png | 32 x 16 | Your own boat from above: a hull of 10 x 4 px and a line for the yard. It is on the chart all evening. |
| 7 | good/good.rhenish_wine.png | 32 x 32 | Your first thing with a round form: a cask lit from the top left, a plum shadow below. It lies in your hold on the first morning. |

How to work on them:

- Open the family's sheet in _templates/sheets beside your drawing (frames, buttons, marks, function icons). It
  shows every piece of the family at eight times its size, so you see what yours must sit beside.
- For a piece cut in nine, look at its picture in _templates/cut_in_nine: the gold lines are the cuts.
- Start from the empty picture of the right size in _templates, or from the placeholder itself.
- When a piece is done, save it over the placeholder. To see it among its family, make the sheets anew (quick; it
  never overwrites a picture you have changed):
  `& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\art\make_art_list.py --only hud`
- pull_art.py (the command is in READ ME FIRST.txt) brings what you drew into the game.

After the ten: finish the frames and buttons first (they are one set), then the marks, then the icons a family at
a time. DRAW FIRST.html keeps the order.
