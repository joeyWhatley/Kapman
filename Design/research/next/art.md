# Art that goes further: faces from parts, colours all can tell apart, text that can be read

7 October 2026. Read first: the book (`Design/GAME_MAKER.md`), `DECISIONS.md`, the mock-up (`ui_mock/template3.html`,
layout G), `pixel_art_zero_to_hero.md`, `research/ui/pixel_ui.md` and `unity_ui.md`, `research/depth/old_school_huds.md`
and `people_depth.md`, `MapPalette.cs`, the art list, the fifty place files, and the placeholder drawers in `Bake/art`.
Nothing here repeats those pages: where they already rule, this page points at them.

**What was run.** `Bake/art/check_palettes.py` (new, section B). A count of the art list and of the people, sites and
cards in the data. A measure of the game's own font file and of every letter the game's texts use (section C).
**What was not.** Unity and git were not run. The session's web-search budget was already spent, so no search was
made: pages I could name were fetched, and every game fact I hold only from memory is marked `(?)`. A number marked
`(g)` is a first guess to time and tune.

---

## The short of it

1. **Faces.** Nobody draws 328 portraits. Draw **14 whole faces in layers on one fixed 64 x 80 grid**, then let the
   game mix their parts. That is how Papers, Please was made. With about 115 small pieces the first release has every
   face it needs, each face ages a little every few winters and stays the same person, and an heir has his father's
   nose. A learner's time: about **130 hours** `(g)`, against about 385 for sixty-odd portraits by hand. The first
   eight whole faces are stage 1's "eight faces", so nothing is drawn twice.
2. **Colours.** Seven of the eight palettes have pairs that some players cannot tell apart, and **Vineyard does not
   hold Byzantium at all**. Seventeen colour values, moved a little, mend all eight for all four kinds of sight. Tide,
   the only palette of the first release, needs two small moves.
3. **Byzantium cannot be both the sea and "what is yours".** In Tide, Minster and Grove the sea *is* `#702963`, so her
   course drawn in Byzantium has a contrast of 1.0 on it: it is not there. Every mark on the chart must be two-toned
   (a light core, a dark edge). Then it reads on all 64 grounds, and the script proves it.
4. **Reading.** The mock-up already has it right: a smooth book face (Gentium Book Plus) for everything you read, a
   pixel font for signs only. But its sizes are too small: at 1920 x 1080 the rows are 17.7 px and the labels 16 px;
   the least a PC game should show is 18. Make body text **22 px at 1080p**, 18 at 720p, 44 at 4K, and let the player
   pick from four sizes.
5. **A fault found on the way:** the game's texts use the letter ⁊ (the Tironian "and") 85 times and two Old Slavic
   letters, and the font in the project has none of the three. They will show as empty boxes.

---

# A. Faces from parts

## A1. How other games do it

| Game | The layers | Age | People, rank, faith | How he stays the same man | What Kapman takes |
|---|---|---|---|---|---|
| **Crusader Kings II** (checked: the CK2 wiki's portrait modding page) | Fixed order: background, hair behind, base head, neck, chin, cheeks, clothes, mouth, nose, eyes, beard, hair, headgear, then scars, boils, a blindfold, prison bars. Each layer is a strip of frames | Three sets of the same strips: young 16 to 29, middle 30 to 49, old 50 and over. A child is one whole picture | Clothes and headgear are picked by culture, faith and rank from a table. The face parts come in one set for each culture group | A "DNA" word of 11 letters, fixed at birth: neck, chin, mouth, nose, cheeks, eyes, ears, hair colour, eye colour. A second word of up to 14 letters holds what can change: hair, beard, clothes, headgear, scars. A child takes each DNA letter from one parent | The split into **what is fixed for life and what changes**. The child's letters from the parents. Dress, not skull, says who you are |
| **Papers, Please** (checked: Lucas Pope's devlog, "Timelapse: Drawing Faces", Dec 2012 to Mar 2013) | He drew whole faces, four women and four men at a sitting, then marked the pieces. "Once the separate pieces are highlighted, the game randomly mixes them to make new faces" | None | A stamp and a dress, not the face | One seed makes the face at the booth and on the paper | **Draw whole faces, then cut.** A learner cannot judge an eye alone on a blank canvas. He can judge a face |
| **Dwarf Fortress** on Steam (checked: the DF wiki's graphics tokens) | Layers with conditions written as text: `CONDITION_CHILD`, `CONDITION_ITEM_WORN`, hair by `TISSUE_MIN_LENGTH` and shaping, `BP_SCARRED`, `CONDITION_GHOST`. Portrait sets (`LAYER_SET:PORTRAIT`) came in v50.13 | Child or not; hair and beard by their true length | By what is worn and by caste | The creature's own body record picks the layers | **Palettes swap whole ramps** (`USE_PALETTE`, a row of colours for a row). And the rules are data, not code |
| **RimWorld** `(?)` | A body, a head (a handful of shapes), hair, clothes. Drawn grey and tinted | Hair greys | Clothes | Head shape, hair and skin tone are kept | Tinting. A face needs very few features if the outline and the hair differ |
| **Wildermyth** `(?)` | Paper figures: skin, face, hair, clothes by calling, and limbs that a story can replace | Hair greys and lines come with the years; the old retire | Clothes by calling | The face parts are chosen once. Time and events only add: grey, scars, a changed arm | **History is added on top**, never by redrawing |
| **Darkest Dungeon** `(?)` | Not parts. One drawing for each calling, in a few colour sets, and a name | None | The calling is the drawing | The name and the quirks do it, in words | Words carry a person. Kapman has a written "looks" line for all 477. Show it |
| **Battle Brothers** `(?)` | Busts: body, armour, head, hair, beard, helmet, wounds on top | Hair colour | Background by dress | Head and hair kept; wounds and gear change | A hurt shows on the face |
| **Mii, and avatar makers such as the LPC generator** `(?)` | Every part is a clear picture the full size of the canvas, laid in a fixed order | Parts only | Parts only | A list of part numbers | **Each part is drawn in place, canvas-sized.** Nothing needs lining up. And where a part sits (eyes a pixel higher, wider apart) is a face of its own for no drawing |

Four rules come out of it, and the kit below is built on them:

- **Fixed and changing are kept apart.** Bone, eyes, nose, mouth, marks: fixed at birth. Hair colour, lines, hairline,
  dress: they move with years and fortune.
- **Dress says the people, not the skull.** These are all north-west Europeans. A Dane is not told from a Frisian by
  his jaw, and it would be false to try. He is told by his hair, his beard, his pin and his cloth, which is how the
  finds tell them apart too.
- **Draw whole faces and cut them.** Each one is a finished portrait on the day it is drawn.
- **Everything is canvas-sized and drawn in place.** The game only lays one picture on another.

## A2. What the data holds

- The art list has 328 portrait slots at **64 x 80**: 258 named people and 70 "a man of this trade" pictures.
- The fifty place files hold **477** people (the brief says 444; the files have grown). Each has `people`, `faith`,
  `role`, `born`, `from`, `to` and `looks`. Only 54 have `sex`: the rest is read from the role, as the art list does.
- 15 peoples, 5 faiths, 62 roles. The `looks` lines are about 170 letters long. Words in them, counted: cloak 184,
  grey 141, beard 116, short 116, tunic 104, apron 55, keys 49, tally 43, cross 40, staff 40, brooch 37, moustache 32,
  stooped 31, tonsure 30, head-cloth 25, beads 23, hammer 20, scar 14.
- **The twelve ports of the first release** (Dorestat, Traiectum, Walichrum, Lundenwic, Witla, Medemelacha, Doccinga,
  Daventre, Quentovic, Sarre, Fordwic, Ripa) hold 117 people, of whom **77 are alive between 793 and 805**. The book's
  audits will cut that to about sixty. Of the 77: Frisian 33, English 22, Frankish 11, Danish 7, Saxon 3, Norse 1.
  Christian 69, old gods 8. Sixteen are clergy. About twenty are women. Thirty-three roles.
- In 793, 44 of them are 30 to 44 and 25 are 45 to 59; the oldest is 58 and will be 70 when the life ends. So the
  first release needs the old face as much as the young one.
- `Bake/art/artdraw.py` already reads a `looks` line and picks build, hair, beard, hood, cloak colour, scar and token
  for the placeholder. **The picking is written. Only the drawing is missing.**

## A3. The kit: one grid, seventeen layers

**The grid (64 wide, 80 high, facing us, light from top left).** Lock it as a guide layer in Aseprite.

| Line | Where |
|---|---|
| Middle of the face | between x = 31 and x = 32 |
| Top of the skull | y = 12 (hair may rise to y = 4, a hood to y = 2) |
| Temples | x = 20 and x = 43: the skull is 24 wide |
| Brow / eyes / nose end / mouth | y = 24 / 27 / 33 / 38 |
| Chin | y = 44 (a long jaw 46, a round one 43) |
| Neck | y = 45 to 50, 10 wide |
| Shoulders | y = 50 at the neck, falling to y = 56 at x = 6 and x = 57 |
| The hand and what it holds | x = 42 to 61, y = 56 to 77 |

**Every adult head shares one skull above the eye line.** Heads differ only in the jaw, the cheeks and the neck. So
every hair, head-cloth and hood fits every head, and a beard hides the jaw it sits on. This one rule is what keeps the
kit small. A child has a skull of his own.

**The layers, from the back.** "First" is the first release; "Full" is the whole cast, 793 to 1066.

| # | Layer | What it holds | First | Full | Picked by |
|---|---|---|---|---|---|
| 1 | Ground | A flat two-tone field: day sky, hall dark, church candle, night lamp | 4 | 8 | where you meet him (the site's kind) |
| 2 | Hair, behind | Long hair, plaits, the fall of a veil or hood | 5 | 10 | goes with its front piece |
| 3 | Body | Shoulders and the neck of the garment: man's tunic, woman's gown, monk's habit with cowl, cleric's long tunic, bishop's chasuble with the Y band, strap dress with two oval brooches and beads, seaman's hooded smock, smith's and ale-wife's apron | 11 | 26 | role, then people and sex |
| 4 | Over it | Cloak pinned on his right shoulder, a woman's shawl, a fur collar, a woven band at the neck | 5 | 10 | wealth, season, `looks` |
| 5 | Pin | Disc brooch, equal-armed brooch, ring pin, plain bone pin | 4 | 9 | **people**: the pin is the passport |
| 6 | At the neck | A small cross, a string of beads, a key on a thong | 3 | 7 | faith and `looks` |
| 7 | Head | Jaw shapes on the one skull: square, long, heavy, a woman's oval, a woman's round; one child | 6 | 10 | fixed at birth; `looks` (broad, lean, big, spare) |
| 8 | Lines | One strip of 8 steps for each adult head: step n holds lines 1 to n | 5 | 8 | age |
| 9 | Mouth | 5 shapes and one glad | 6 | 10 | fixed at birth |
| 10 | Nose | straight, long, broad, hooked, broken | 5 | 9 | fixed at birth |
| 11 | Eyes | Whites and lids, 5 x 3 pixels each. The dark of the eye is a layer of its own, so the game can move it | 8 | 14 | fixed at birth |
| 12 | Brows | 3 shapes, each plain and stern | 6 | 10 | shape fixed; plain or stern by how you stand with him |
| 13 | Marks | Scar on the brow, scar on the cheek, pox, a blind eye | 4 | 8 | `looks`, and what happens in play |
| 14 | Beard | Stubble, moustache, short, full, long | 5 | 9 | people, age, `looks` |
| 15 | Hair, in front | Men: cropped, bowl-cut, to the shoulder, tied back, receding, bald with a fringe, tonsure, tonsure gone thin. Women bare-headed: plaits, a knot, loose. Two for the child | 13 | 28 | people, role, age, `looks` |
| 16 | On the head | Linen head-cloth (two ways), a nun's veil, a hood up, a felt cap | 5 | 17 | role, faith, `looks` |
| 17 | In the hand | A fist and a thing: tally stick, scales, keys, crosier, book, pen, ladle, steering oar, sounding pole, spindle, hammer, adze, staff, sickle, purse and so on | 20 | 50 | role (the table `ROLE_TOKEN` in `artdraw.py`) |
| | **Pieces** | | **115** | **243** | |

**Free faces, for no drawing.** The game may set the eyes at y = 26, 27 or 28, the gap between them at 5, 6 or 7, the
nose one pixel longer, and the mouth at y = 37, 38 or 39. That is 54 placings of the same pieces. With 5 jaws, 8 eyes,
5 noses, 5 mouths and 3 brows there are 3,000 bare faces before placing, hair or beard.

**What a player can tell at a glance** is the outline: hair, beard, head-cloth, and the colour of the cloth. So the
rule for a port is on the outline: **no two people of one port share hair, beard, headwear and cloak colour.** The bake
checks it and rolls again.

**What the kit does not try to show.** A burn on a forearm, a limp, a voice. The `looks` line says those. Show it:
under the name the first time you meet someone, the frame prints the first sentence of `looks` in the quiet colour.
The game is text. The face is the cue.

## A4. Colour rules of the kit

- **Paint with the real colours** of `palette_byzantium.gpl`. No false colours. You must be able to judge the face as
  you draw it.
- **Each layer has one ramp that the game may swap**, and you paint that layer's cloth, hair or skin with exactly that
  ramp. Everything else on the layer stays as painted.

| Layer | Paint with (the ramp that swaps) | The game swaps it to |
|---|---|---|
| Head, neck, hand | fair skin: dry earth, dusk rose, dawn peach, morning gold | **weathered**: blood, rust, dusk rose, dawn peach. **sallow**: timber, dry earth, sand, pale sand. **dark**: peat, wet earth, timber, dry earth |
| Hair, beard, brows | brown: wet earth, timber, dry earth | **flaxen**: dry earth, gold, pale gold. **red**: blood, rust, dusk rose. **dark**: peat, wet earth, timber. **black**: night, basalt, slate. **greying**: the brown with only its light step turned to pale stone. **grey**: slate, stone, pale stone. **white**: stone, pale stone, cool white |
| Body | undyed wool: dry earth, sand, pale sand | brown, grey, blue (woad), russet and red (madder), green, yellow (weld), near-black |
| Over it | blue: deep sea, sea, shoal | the same list |
| Pin | bronze: dry earth, gold, pale gold | iron: basalt, slate, stone. silver: stone, pale stone, cool white. gold stays |

- **Dye is rank.** Four steps of wealth, set by the role. 1 (beggar, captive, herdsman): undyed, brown, grey; an iron
  or bone pin. 2 (craftsman, ferryman, ale-wife, crew): adds blue and russet; bronze. 3 (merchant, moneyer, reeve,
  steward, skipper): adds strong red, green and yellow, a woven band; silver. 4 (count, bishop, abbot, king): adds a
  fur collar in winter and gold. The people leans the hue, as `PEOPLE_CLOAK` does now.
- **Byzantium is not in the list.** It is on a bishop's silk, a king's cloak, and one more place: **a thin band at the
  neck of your own house.** You wear it, your heir wears it, your factor wears it. That keeps the book's rule
  ("Byzantium means yours") inside the portrait.
- **Shadow leans to purple** (the palette's first rule): the row under the chin is `plum`. **Outlines are the dark
  step of what they bound.** One pixel of `night` goes only where the figure meets the ground, so that he stands off
  all four grounds.
- **Nothing is said by hue alone.** Faith, people and standing are also in words on the name line. (See B: about one
  man in twelve does not see red against green.)

## A5. How a person's data picks the pieces

The bake turns each person into a **recipe**: a short record the game reads.

```
"dorestad.adalwin": { "jaw": "heavy", "eyes": 3, "nose": 1, "mouth": 4, "brows": 2, "place": [27, 6, 0, 38],
  "skin": "weathered", "hair": "brown", "hairFamily": "cropped", "beard": "short", "headwear": "",
  "body": "tunic", "over": "cloak", "clothA": "brown", "clothB": "blue", "pin": "disc", "wealth": 2,
  "neck": "", "token": "tally", "marks": [], "grey": 44, "wear": 2, "fixed": false }
```

| From | Comes |
|---|---|
| the id | A steady seed (`rng_of("portrait:" + id)`, as now). It picks jaw, eyes, nose, mouth, brows, the placing, the natural hair colour among those of his people, and two numbers: `grey` (the year of life his hair turns, 40 to 54) and `wear` (-4 to +4 years on his lines) |
| `born` and the year | His age: which head (child under 14), the step of lines, the hair ramp, the hairline (A6) |
| `people` | The pin. The hair and beard he is likely to wear. The lean of his cloth's colour. For Danish, Norse and Svea women, the strap dress |
| `faith` | With a clergy role: the tonsure and the habit. A lay Christian: a cross at the neck one time in four. Old gods: beads or nothing. No sign is forced: the name line says his faith once you know it |
| `role` | Body, wealth, the thing in his hand, hood or head-cloth by trade. The tables `CLERGY`, `NUNS`, `COVERED`, `HOODED` and `ROLE_TOKEN` in `artdraw.py` are kept as they are |
| `looks` | **It overrules all of the above.** Broad or lean picks the jaw. Grey, white, red, fair, dark pick the hair. Bald, tonsure, beard, moustache, clean-shaven, hood, head-cloth, scar, blind, "a blue cloak", apron, keys, tally stick, cross: each picks its piece. A `looks` line is dated ("forty-five in 793"), so if it says grey at 45, his `grey` number is set so that it is true that year |
| sex | From `sex` where it is given, else from the role and the words of `looks`. The bake lists everyone it guessed |

**The bake also writes the drawing list.** When a `looks` line asks for something there is no piece for, the bake counts
it: "wanted: fur cap, 3 people; plaited beard, 2". The piece most asked for is the next one worth drawing.

A recipe with `"fixed": true` is left alone by the bake. That is how you correct a face by hand without drawing it.

## A6. Growing old and staying the same man

**Fixed for life:** jaw, eyes, nose, mouth, brow shape, the placing, skin, natural hair colour, the hair's family,
marks got before the game. **Moving:** everything in this table.

| What moves | Rule |
|---|---|
| Lines | Step 0 to 8. Step = (age - 34 + wear) / 4.5, rounded, kept in 0 to 8. A seaman, a ferryman or a "weathered" man has 4 more years of wear. The first line comes at about 37, all eight by about 70 |
| Beard colour | Natural until `grey` - 4. Then greying. Grey from `grey` + 2. White from `grey` + 12. The beard goes before the hair, as in life |
| Hair colour | The same, four years later |
| Hairline | One man in three: at `grey` + 6 his hair turns to the receding piece of the same family, at `grey` + 18 to the bald one |
| Stoop | From 62, or if `looks` says stooped: the whole head sits 1 pixel lower. From 70, 2 |
| Marks | A card that hurts him adds a mark. It stays |

The **lines strip** is the cheap part. Draw the old face's lines once over a head, then make the earlier steps by
rubbing lines out. Eight frames, about an hour and a half a head `(g)`.

**You, a year older each winter** (ruled 7 October). Your own face takes exactly one step at each winter board, from
this list: first line, beard greying, second line, hair greying, third line, beard grey, fourth line, the hairline,
fifth line, hair grey, sixth line, seventh line. Twelve winters, twelve changes. The winter board shows last year's
face beside this year's for the length of the board. Nobody else needs that care: they follow the table.

**The heir.** For each of jaw, eyes, nose, mouth, brows, skin and natural hair, the heir takes his father's or his
mother's, one or the other, by his own seed. If the mother is not in the data, one piece in four is rolled new. He
wears the house's Byzantium band. On the day of the handover the frame holds the old face a moment and then the young
one with the same eyes. Nothing has to be written to say it.

**The feeling on a face.** The brows are stern if he thinks ill of you (standing -2 or worse), raised one pixel with
the glad mouth if he thinks well (+2 or better), plain between. A crewman who is uneasy looks away: the game moves the
darks of his eyes one pixel. You see how you stand before you read a word. The feeling word stays on the name line,
because a face must never be the only place a fact is told.

## A7. A portrait drawn by hand

The road already exists: `pull_art.py` copies a picture you drew to `Art/final/portrait/person.<id>.png` when it has
the slot's name and size. **If that file is there, the game shows it and leaves the kit alone.**

- Ages, if you want them: `person.<id>.y.png` (young), `.p.png` (prime), `.e.png` (from 45), `.o.png` (from 60). The
  game shows the nearest one at or under his age. One file alone serves every age.
- Draw it on the kit's grid if you can. Then the lines strip and the marks still fit over it.
- Who is worth it in the first release: the count's steward (the first face of the game, with half a split hazel
  stick), the count, your mate, your rival, the loud voice of Dorestat. Five. About 5 hours each `(g)`. None is
  needed for the game to run: the kit's face stands until yours replaces it.
- Yourself: the book asks four drawings for each head of the house. With the kit you need none. Draw them last.

## A8. What the player sees in the frame

- **Top right, the well:** the face at 64 x 80 art pixels, shown at the frame's whole scale (2 at 720p, 3 at 1080p,
  4 at 1440p, 6 at 4K: so 192 x 240 screen pixels at 1080p). Layout G's right column is 24% of the width, which is
  154 art pixels: the face, and 80 art pixels beside it for the name line, the first sentence of `looks`, and the
  feeling word.
- **Pop-ups do not repeat the face.** The well shows whoever speaks, as ruled. Each pop-up's title bar carries a
  32 x 40 cut of the same picture (x = 16 to 47, y = 6 to 45: the head and hair), so that with three pop-ups open you
  see whose words each is.
- **The crew strip** is four of the same 32 x 40 cuts. No second set of small heads is drawn.
- The ground behind a face changes with the place: sky on the strand, dark in the ale-house, candle in the church.

## A9. What a programmer must build

About four nights `(g)`.

1. `Bake/art/make_face_recipes.py`. Lift the picking out of `artdraw.draw_portrait` into `face_recipe(person, year)`.
   Write `Assets/Kapman/Game/Data/faces_793.json`: a recipe for each person and for each of the 70 trades. Write a
   contact sheet, `Art/built/faces_sheet.png`: each port's people in a row, in 793 and twelve years on. Print the
   people whose sex was guessed, the ports where two people look alike, and the drawing list (A5).
2. `Art/kit/face/<layer>.png`: one strip for each layer, cells of 64 x 80. And `face_kit.json`: the order of the
   layers, the names of the cells, the ramp each layer swaps, which front hair needs which back, what hides what (a
   hood hides the front hair; a head-cloth hides all hair).
3. In the game: `FaceKit` (reads the strips once into pixel arrays), `FaceRecipe` (kept in the save for the people of
   your house), `FaceBuilder.Build(recipe, age, feeling)` which returns a 64 x 80 texture, point-filtered. It lays 17
   cells and swaps ramps by exact colour. Keep the last forty built. `PortraitView.Show(person)` asks first for a
   hand-drawn file, then for the builder.
4. The heir's recipe from the parents' (A6). The yearly step for your own face.
5. Tests. Every recipe builds with no piece missing. A person at 30 and at 60 differs only in lines, hair and beard
   ramp, hairline and stoop. No two alike in a port. A hand-drawn file wins.
6. `make_art_list.py`: 115 new rows of a kind `face_part` (name, 64 x 80, the grid numbers, what it must fit), so the
   list, the Desktop folder and `pull_art.py` serve the kit as they serve everything else. The 328 portrait rows get
   the words "the kit draws this until you do".

## A10. The hours, honestly

You are in week 1 of the twelve-week plan. A face at 64 x 80 is past its end: week 6 teaches "faces in six pixels".
Add **three weeks of portrait study** after week 12 (about 15 hours): copy three faces from the Utrecht Psalter, and
study, without copying into the game, a few of the 96 x 80 portraits of the Game Boy Advance Fire Emblem games `(?)`.

| Work | Pieces | Each `(g)` | Hours `(g)` |
|---|---|---|---|
| The first whole face (the grid, the skin, the light; expect to draw it twice) | 1 | 10 h | 10 |
| Thirteen more whole faces, in layers | 13 | 4.5 h | 58 |
| More hair than the faces gave (7), beards (2), headwear (3), bodies (3) | 15 | 50 min | 13 |
| Pins and things at the neck | 7 | 15 min | 2 |
| Things in the hand | 20 | 30 min | 10 |
| Marks and grounds | 8 | 15 min | 2 |
| Lines strips | 5 | 1.5 h | 8 |
| Mixing every part on the contact sheet and mending what does not fit | | a quarter more | 26 |
| **The kit for the first release** | **115** | | **about 130** |
| The same sixty-odd faces one at a time, by hand, with no ageing, no strangers and no heir | 77 | 5 h | 385 |
| **The rest of the kit for the whole cast** (12 more whole faces for the other peoples and the later years; 128 more pieces) | 128 | | **about 125 more** |
| The whole cast by hand | 477 | 4 h | 1,900 |

The fourteen whole faces hold about 60 of the 115 pieces; the other rows are the 55 drawn on their own. At an hour a
day the first kit is four to five months. At ten hours a week it is three. **These are guesses. Time the first face
and the second, then multiply.**

It is more than the book's "about 60 pieces a stage", though the pieces are small. Spread it:

| Stage | Draw | Hours `(g)` | The game then has |
|---|---|---|---|
| 1 | Eight whole faces (five men, three women) | 40 | The book's "eight faces", and already 4,000 mixes of them |
| 2 | Six more whole faces, the extra hair, beards, headwear and bodies, the pins | 55 | Every person of the twelve ports, the crew, strangers |
| 3 | The lines strips, the things in the hand, marks, grounds | 35 | Ageing, the heir, a trade in every hand |

## A11. The order of work

1. Set the grid (A3) as a locked layer. Load the palette. Make the layers of A3 once, in order, and save the empty
   file as your stencil.
2. Draw face 1 whole: a Frisian man of forty, cropped brown hair, short beard, tunic and cloak. Every part on its own
   layer. Paint hair only with the brown ramp, skin only with the fair ramp, the tunic with the undyed ramp, the cloak
   with the blue ramp.
3. Draw face 2 on a copy of the stencil with the head layer of face 1 switched on underneath as a guide: a different
   jaw, the same skull.
4. After each face, send it in. The contact sheet shows every new part on every old head. Mend where a hair leaves a
   gap at the temple or a beard does not meet the ear.
5. The eight of stage 1, so that they cover the first hour: the count's steward; a Frisian skipper; an ale-wife in a
   head-cloth; a priest with a tonsure; a toll man; an English reeve; a Frisian farm wife; a Dane with long hair and a
   full beard.

Period notes for the drawing, to check against `content/peoples` before any is drawn: no horned helmets and no buttons
(the art list's own rule); a cloak pins on the man's right shoulder; bishops wear no mitre before about 1000 `(?)`;
Thor's hammers at the neck are mostly of the tenth century `(?)`, yet `looks` has the word 20 times: the audit should
look at the years of those people.

## A12. Scenes: a kit of stamps

The art list has 335 site scenes at 320 x 180. The data has 610 sites of 30 kinds. The twelve ports have 149 sites of
29 kinds, which the audits will cut to about fifty. A learner draws a 320 x 180 scene in 8 to 12 hours `(g)`: fifty of
them is a year.

**`artscene.py` already lays every scene by rule** from a few things it draws itself: house, gable, hall, church,
palisade, stone wall, tree, reeds, smoke, fire, a person, a crowd, a boat afloat, a boat on the beach, cask, sack,
stall, jetty. There is a layout for each kind of site (`s_quay`, `s_market`, `s_toll` and so on) and seven moods of
sky and water (day, dawn, dusk, night, fog, storm, winter).

**So you draw the stamps, and the bake keeps laying.**

| Stamps | First release | Each `(g)` |
|---|---|---|
| Buildings: wattle house, plank house, gable end, hall, timber church, boat shed | 6 | 2 h |
| Edges: palisade, stone wall (the old Roman work at Traiectum and Lundenwic), fence, jetty, bank | 5 | 1 h |
| Growth: willow, alder, oak, reeds | 4 | 1 h |
| People, 16 x 24, standing: merchant, crewman, woman, monk, Dane, child, toll man, smith | 8 | 1 h (they are week 6 of your plan) |
| Boats: afloat, on the beach, in frame on the slip | 3 | 2 h |
| Things: cask, sack, stall, pot crate, hides on pegs, fish rack, loom, anvil, cross, well, quern, cart | 12 | 30 min |
| Skies and water | none: the moods are palette swaps of one sky | |
| **In all** | **38** | **about 45 h** |

- **To build:** in `artscene.py`, each drawing function first looks for `Art/kit/scene/<name>.png` and stamps it,
  recoloured for the mood by ramp swap, else draws as now. Two nights `(g)`. A hand-drawn scene overrides the kit by
  the same road as a portrait.
- **A fault in the sizes.** In layout G the chart shows between 30% and 75% of the width: 288 art pixels. A scene is
  320 wide, a harbour picture 640, a ship from the side 160 (the ship window is 154). None fits where it is meant to
  go. Either keep everything that matters in a scene inside its middle 256 pixels, or change the slots to 256 x 144.
  The harbour picture has no place in layout G at all, except as a band across the whole screen for the moment she
  comes in. This is yours to rule before any scene is drawn.

## A13. Card pictures

508 slots at 192 x 108: 233 harbour, 122 crew, 80 sea, 61 long. The first release lets in about 150 cards. Each row
asks for its own small scene. At 4 hours each that is 600 hours for the first release.

**Do not draw card pictures for the first release.** The critics say the cards have no teeth. A picture gives a card
no teeth. A card shows two things you have already drawn:

1. **Who speaks:** his face from the kit, in the well.
2. **Where it is:** one of eight backs at 192 x 108, picked by the card's deck and the weather: open sea calm, open sea
   rough, fog, river, quay, beach, hall by firelight, the deck close by. About 3 hours each `(g)`: 24 hours for every
   card. The moods are ramp swaps.

Later, a card that has earned it gets a **subject**: one stamp of 48 to 96 pixels (a sail on the skyline, dice on a
thwart, a tally stick) laid on its back. About an hour each `(g)`. A card earns it by being played and kept through a
soak. `vignette()` in `artscene.py` already lays a back and signs: it needs the same look-for-a-stamp step.

---

# B. Colours all can tell apart

## B1. The script

`Bake/art/check_palettes.py`. It reads the eight palettes out of `MapPalette.cs`, so it checks what the game shows.

```
& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\art\check_palettes.py
      --all          every pair, not only the bad ones
      --mend         the smallest fix for all four kinds of sight
      --mend-light   a fix that moves lightness only (every hue kept)
      --mend-rg      a fix for normal, red-blind and green-blind sight only
```

It prints and writes nothing. It checks its own sums against four pairs of the standard's test table first.

- **The sights.** Normal, and the three colour-blindnesses at full strength: red-blind, green-blind, blue-blind. Two
  opinions each: the matrices of Machado, Oliveira and Fernandes (2009), and those of Viénot, Brettel and Mollon
  (1999; 1997 for blue). Both are applied to linear light. A pair's distance is the smaller of the two. Full strength
  is the worst case: a palette that passes it passes the milder, commoner kinds.
- **How far apart** two colours are: CIEDE2000 (dE). Under 5 they are one colour on a chart. Under 10 they can be
  told side by side but not named alone. Classes on a map want 10 or more `(?)` (I hold that figure from Brychtová and
  Çöltekin's map studies and could not check it).
- **How well a word or a line stands on its ground:** the WCAG contrast ratio, 1 to 21. A line or a mark wants 3.
  Text wants 4.5; for the text you read all evening this page asks 7.
- **Who it is for.** About 1 man in 12 and 1 woman in 200 has a red-green weakness; about 1 man in 50 has none of one
  kind at all `(?)`. Blue-blindness is some hundreds of times rarer `(?)`.

## B2. What it found: the pairs that cannot be told apart

| Palette (people) | Pair | Normal | Red-blind | Green-blind | Blue-blind | |
|---|---|---|---|---|---|---|
| **Tide** (Frisian) | shore / high ground | 10.3 | 8.9 | 8.7 | 10.3 | hard |
| | deep water / sea | 11.8 | 8.9 | 11.2 | 13.2 | hard |
| **Vineyard** (Frankish) | none | | | | | **but it holds no Byzantium** |
| **Minster** (English) | shore / high ground | 8.7 | 7.1 | 8.0 | 9.4 | hard to all |
| **Amber** (Danish) | sea / woodland | 33.8 | 14.3 | **5.3** | 42.0 | hard: water against land |
| | shore / high ground | 12.1 | 12.3 | 12.0 | 8.6 | hard |
| **Fjord** (Norse) | sea / woodland | 26.9 | 21.7 | 18.4 | **1.2** | one colour: water against land |
| | shallows / cleared land | 29.0 | 25.9 | 21.1 | **4.1** | one colour: water against land |
| | sea / marsh | 19.7 | **5.0** | 9.5 | 37.8 | hard: water against land |
| | marsh / woodland | 30.9 | 18.4 | 10.0 | 36.4 | hard |
| **Silver** (Svea) | sea / marsh | 12.9 | **4.2** | **2.0** | 18.8 | one colour: water against land |
| | shore / high ground | **5.1** | 4.8 | 4.6 | 4.0 | one colour, to all |
| | sea / woodland | 26.4 | 22.7 | 20.4 | 8.3 | hard: water against land |
| **Grove** (Wendish) | shore / high ground | 9.1 | 9.0 | 8.7 | 8.7 | hard to all |
| **Vellum** (Irish) | marsh / woodland | 51.4 | **3.9** | 13.2 | 38.4 | one colour |
| | sea / high ground | 31.6 | 10.9 | **5.7** | 41.4 | hard: water against land |
| | sea / woodland | 38.5 | 37.4 | 36.9 | 6.6 | hard: water against land |
| | deep water / high ground | 22.3 | 7.1 | 10.4 | 32.6 | hard: water against land |

There are three kinds of fault.

1. **Byzantium beside a blue of the same lightness.** A red-blind or green-blind eye does not see the red in a
   red-purple, so `#702963` falls onto any blue or teal as dark as itself. That is Silver's sea and marsh, Fjord's sea
   and marsh, Amber's sea and woods, Vellum's sea and high ground. Byzantium may not move, so the blue must move away
   from it in lightness.
2. **Sand beside snow.** Shore and high ground are both pale creams, in five palettes, to every eye. In Silver they are
   5.1 apart with normal sight.
3. **Rust beside green** in Vellum: the classic red-green pair. To a red-blind eye the marsh and the woods are one.

## B3. The smallest mends

The script moves the seven free colours in the smallest steps that bring every pair to 10 for every sight, then walks
each one back toward where it was as far as the pairs allow. Byzantium is never moved.

| Palette | Ground | Now | Mended | What moved | Moved by |
|---|---|---|---|---|---|
| **Tide** | deep water | `#3b1540` | `#37113c` | 1.7 darker | 1.1 dE |
| | high ground | `#f1e8d6` | `#faf1df` | 3.1 lighter | 1.9 |
| **Vineyard** | sea | `#5a2970` | `#702963` | **Byzantium put in**: 16 degrees redder | 7.5 |
| | woodland | `#70293f` | `#481224` | 12.7 darker, the same wine hue | 9.3 |
| **Minster** | shore | `#ece5cc` | `#efe5c2` | a little more yellow | 3.1 |
| **Amber** | sea | `#296370` | `#38717e` | 5.6 lighter | 5.1 |
| | shore | `#f3dfa8` | `#f7dda8` | 5 degrees warmer (for blue-blind only) | 2.1 |
| **Fjord** | sea | `#34407c` | `#585896` | 11 lighter | 9.2 |
| | shallows | `#8494c6` | `#9490c2` | 13 degrees toward violet (for blue-blind only) | 6.0 |
| | woodland | `#1f4a44` | `#204a44` | a rounding | 0.2 |
| **Silver** | sea | `#4a3d5c` | `#463c43` | 27 degrees warmer and greyer | 10.2 |
| | shore | `#e8e4ee` | `#cbcde2` | 8.3 darker, a little bluer | 7.2 |
| **Grove** | high ground | `#efe9d0` | `#f8f2d9` | 3.1 lighter | 1.9 |
| **Vellum** | deep water | `#1f2a44` | `#202638` | a little greyer | 3.5 |
| | sea | `#2f5d7a` | `#2c678f` | 4 lighter, bluer | 4.4 |
| | marsh | `#c9553a` | `#ce4f48` | 9 degrees toward rose | 5.7 |
| | woodland | `#3f7029` | `#4c6e1f` | 9 degrees toward yellow | 3.7 |

- Seventeen values in all; fifteen if the two marked "for blue-blind only" are left. No colour moves more than about
  10 dE, and most under 6: you would have to hold old and new side by side to see it.
- After the mend the nearest pair in every palette is 10.0 or more to every sight.
- **Tide is the only palette of the first release**, since you begin a Frisian of Dorestat. It needs the first two
  rows and nothing else.
- Vineyard's woodland comes from `--mend-light`, which keeps the hue. `--mend` would make it a strong crimson
  (`#7f0026`) instead. Silver cannot be mended by lightness alone: its sea must change hue.
- With `--mend-rg` (blue-blindness left out) Fjord's sea need only go to `#3c5290`, Silver's sea to `#4a3541` and its
  shore to `#cfcbd5`, and Vineyard's woodland can stay. The cost of mending for all four is small, so I would do all.

## B4. Vineyard has no Byzantium

`MapPalette.cs` says every palette holds `#702963`. Vineyard does not: its sea is `#5a2970` and its woods `#70293f`.
`self_793.json` even says so ("the exact #702963 is not among its eight colours"). The cheaper place for it is the
sea (7.5 dE; the woods would cost more in other moves). Then four peoples have a Byzantium sea.

**The palettes are written down in four places:** `MapPalette.cs`, `Bake/world/recolour_map.py`,
`Design/palettes/*.gpl` and `Data/story/self_793.json`. Any change must be made in all four, or they drift.

## B5. Byzantium cannot be both the sea and "yours"

The book's rule 5: "Byzantium means yours": your sail, your course, your known waters, your pins. The palettes of the
same day put Byzantium in the ground: the sea in Tide, Minster and Grove (and Vineyard, once mended), the woods in
Amber, the marsh in Fjord and Silver, the high ground in Vellum.

The script's count for her course drawn as a plain Byzantium line (a line wants 3):

| | deep | sea | shallows | shore | marsh | cleared | woods | high |
|---|---|---|---|---|---|---|---|---|
| Tide | 1.5 | **1.0** | 2.2 | 5.8 | 2.2 | 4.1 | 1.2 | 7.4 |

On the sea she sails, in the palette of the first release, it is not there. Her mark in gold fails on five of Tide's
eight grounds (1.0 on cleared land). No single colour can stand on eight grounds that run from near-black to
near-white.

**The rule that does: every mark on the chart is two-toned.** A light core with a dark edge, or the other way. Any
ground stands off from near-white or from near-black. The script tried warm white with an ink edge on all 64 grounds:
the weakest is 3.6. Then colour says *whose*, and the two tones say *where*.

| Mark | Drawn as |
|---|---|
| Her mark | Hull and sail in warm white, an ink edge of one pixel, a Byzantium band across the sail |
| Her course | A warm-white line of one pixel with an ink pixel each side. It is the only course on the chart, so it needs no colour to say whose. A Byzantium bead with a white rim marks each day's run along it, which is the measure in days made visible |
| Other sails | Warm white with an ink edge. No band |
| Pins and port marks | As the art list already asks ("keep the dark edge"), with a light core |
| Names on the chart | Never bare. Warm white with an ink edge reaches only 3.6 to 4.2 on the middle tones (Vellum's marsh, Tide's and Vineyard's shallows), under the 4.5 that text wants. Every name sits on its own small ink plate (17 to 1) |
| Known waters | Not a purple wash, which is nothing on a purple sea. Known is inked and full; heard-of is dim, as the book already has it |

**The frame itself has the fault.** A slip is ink with a Byzantium border. On dark water both vanish: 1.3 on Vellum's
deep water, 1.5 on Tide's. The thick frame's pale bevel (`#8c6a8e`) reaches only 1.9 to 2.9 on the dark grounds. Give
every frame and floating window one outermost line of warm white, one art pixel wide. Then no window is lost on any
sea.

## B6. The HUD's words on its panels

The slip is 94% ink, so the chart shows through. The script tries each pair over the lightest ground of any palette
(`#faf8fc`) and the darkest, and gives the worst of the four sights.

| Words on ground | Ratio | Wants | |
|---|---|---|---|
| paper `#f1e8d6` on a slip (body text) | 13.9 | 7 | passes |
| gold `#f0d27a` on a slip (numbers, headings) | 11.0 | 4.5 | passes |
| lilac `#dba3cb` on a slip (labels) | 7.7 | 4.5 | passes |
| mute `#a79fae` on a slip (hints, the age of news) | 6.7 | 4.5 | passes; not for long text |
| gold on Byzantium (the chosen button) | 6.2 | 4.5 | passes |
| mute on a tab not chosen `#2a1c2e` | 6.3 | 4.5 | passes |
| dark on gold, dark on lilac (badges, the chosen speed) | 12.1, 8.4 | 4.5 | passes |
| the kit's warm white, pale stone, gold on night | 17.2, 8.1, 8.1 | | passes |
| the Byzantium border against the slip's own fill | 1.6 | 3 | **fails**: a line that only decorates |
| the thin rule between rows (lilac at 28%) | 1.5 | 3 | **fails**: rows are parted by space, not by this line |
| the kit's "warning" rust `#c9553a` as words on night | 3.6 | 4.5 | **fails** as text; passes as a sign |
| the kit's "bad" blood `#8f2d2d` as words on night | 1.9 | 4.5 | **fails**: cannot be read |

**Every word in the mock-up can be read by every eye.** The lines cannot be relied on, and two signal colours cannot
carry words.

The three signals of `pixel_ui.md` (good meadow, warning rust, bad blood) against each other: good and warning are
5.2 apart to a green-blind eye. Good and your own gold are 7.1 apart to a red-blind eye.

**Mend:** good is `reed light #b6c26a` (10.2 on night). Bad is `rust #c9553a`, never as running words: only as a
sign, or behind warm-white words. The two differ by 25 in lightness, so every eye parts them (17.5 at the worst).
Warning has no colour of its own: it is the sign "!" in gold. `blood` is for pictures. And as `pixel_ui.md` already
rules, a signal always has its word or its shape beside it: "sure", "likely", "doubtful".

## B7. What a programmer must build

One night `(g)`.

1. Put the mended values of B3 into the four places of B4. Better: make `recolour_map.py` and the `.gpl` files read
   them out of `MapPalette.cs`, as `check_palettes.py` does (`read_palettes()` can be shared), so there is one place.
2. A test in the game's suite: every palette holds `#702963` exactly once, and deep water is darker than sea, and sea
   darker than shallows. It would have caught Vineyard.
3. The two-tone rule for every mark on the chart (B5). The warm-white outer line on the frame and on every pop-up.
4. The signal colours of B6.
5. Run `check_palettes.py` whenever a palette changes. It takes seconds.

---

# C. Text that can be read

## C1. What was measured

**The font in the project** (`Assets/Kapman/Game/UI/Fonts`: Gentium Book Plus Regular and Bold, and Noto Sans Runic),
read with fontTools:

| | Of the font size |
|---|---|
| Height of a small letter (x) | 0.454 |
| Height of a capital | 0.615 |
| From the top of an h to the foot of a g | 1.00: so "22 px" is 22 screen pixels in the measure Xbox's guideline uses |
| The font's own line | 1.465 |
| A letter of the game's own text, spaces counted | 0.41 wide: a line holds **width / (0.41 x size)** letters |
| The digits | all the same width (986 units): a column of prices lines up as it is |

**The letters the game's texts use** (198 data files, 25.7 million characters): 253 different characters beyond plain
English. þ 3,823 times, ð 3,299, æ 2,867, ö 2,782, ē 2,665, ā 1,755, ǣ 1,242. Also 72 Cyrillic letters, 9 Greek, 43
runes.

- Gentium has 207 of them. Noto Sans Runic has the 43 runes.
- **Three are in neither font:** ⁊ (U+204A, the Tironian "and" of Old English and Irish writing), used **85 times**;
  and the Old Slavic ѧ and ѹ, three times between them. They will show as boxes.
- There is **no italic file**. The mock-up sets the speaker's own tongue in italic; Unity will fake it by slanting.

**The mock-up's sizes.** Its text is 1% of the screen's width: 12.8 px at 1280 x 720, 19.2 at 1920 x 1080. Rows are
0.92 of that, hints 0.88, labels 0.85, badges 0.7.

| At 1920 x 1080 | Size | Xbox's least for a PC (checked: guideline 101) |
|---|---|---|
| base text | 19.2 px | 18 |
| rows in a window | 17.7 px | 18: under |
| labels | 16.3 px | 18: under |
| badges | 13.4 px | 18: under |

At 1280 x 720 the base is 12.8 px and a small letter is 5.8 pixels high. Game Accessibility Guidelines ask 28 px at
1080p for a screen across a room (checked).

## C2. The rules

**One rule of scale.** The frame, the icons, the faces and the pixel font sit on a 640 x 360 grid shown at a whole
multiple (`pixel_ui.md` A6, `unity_ui.md` 2): 2 at 720p, 3 at 1080p, 4 at 1440p, 6 at 4K. **Text you read is not on
that grid.** It is set in true screen pixels. In the panel that is the size in screen pixels divided by the multiple
(22 / 3 = 7.33 panel pixels).

| | 1280 x 720 | 1920 x 1080 | 3840 x 2160 |
|---|---|---|---|
| The frame's multiple | 2 | 3 | 6 |
| **Body text** (cards, talk, readings, the chronicle, any sentence in a window): Gentium Book Plus Regular | **18 px** | **22 px** | **44 px** |
| Small letter / capital, in screen pixels | 8 / 11 | 10 / 13.5 | 20 / 27 |
| **Quiet text** (the age of a word, a source, a hint): Gentium, the mute colour | 16 px | 18 px | 36 px |
| Nothing is ever smaller than | 16 px | 18 px | 36 px |
| **Numbers in rows**: Gentium's own digits, set to the right, the unit ("d") after in mute | 18 px | 22 px | 44 px |
| **Signs** (the eight verbs, window names, the day rule, the date and the purse on the top bar): the pixel font, capitals 7 art pixels | 14 px capitals | 21 px capitals | 42 px capitals |
| Counts on an icon: 3 x 5 pixel digits | 10 px | 15 px | 30 px |
| Line height, sentences / rows | 1.45 / 1.25 | the same | the same |
| Left column of layout G (29% wide, 174 art pixels inside): letters a line | 47 | 57 | 57 |
| A pop-up with a card back (192 art pixels wide, the text as wide as the picture) | 52 | 64 | 64 |
| Contrast, body / quiet / signs | 7 / 4.5 / 4.5 at least (paper on a slip is 13.9, mute 6.7) | | |

- **Line length.** Aim at 45 to 75 letters. Never over 80 (the Xbox guideline's limit). A window of sentences is not
  let narrower than 36 letters: the tiler refuses the split, or turns the window to its rows-only form (a label and a
  number). A window of rows may go down to 24.
- **A card is as wide as its picture.** 192 art pixels gives 64 letters a line at 1080p: no measuring needed.
- **Set to the left, ragged on the right.** No centred sentences, none set in capitals, none justified.
- **Italic** is only for words in another tongue, and never more than two lines of it.
- **Names of people and ports are always in Gentium**, also on a button. So the pixel font never has to spell
  Ögvaldsnes, Hæthum or Kāpman.
- **A sign is at most 16 letters and never a sentence.**
- A Steam Deck is 1280 x 800 on a 7-inch glass. Text of 18 px is small there: on a Deck the game starts at "Large".
  (Valve asks that no letter be under 9 pixels high, and likes 12 `(?)`.)

## C3. What a pixel font is good for, and bad for, in this game

**Good**

- It is art. It sits on the frame's own grid and is sharp at every multiple.
- Short, big, few: a verb on a button, a window's name, a date, a sum. Its digits are plain and all one width.
- You can draw one yourself (`pixel_ui.md` B5): the 95 plain characters, perhaps 12 hours `(g)`. For signs that is all
  it needs.

**Bad**

- **Room.** On the grid a 5 x 7 font takes about 6 pixels a letter. The left column of layout G is 174 pixels inside:
  **29 letters a line**. Gentium at 22 px in the same column at 1080p: **57**. Twice the words in the same window. With
  eighteen panels and windows that tile, that decides whether two windows fit side by side.
- **Letters.** Small letters are 5 pixels high: a, e, s and g are five-by-five blots. There is no true italic, and bold
  closes the letters up.
- **Marks.** 253 characters beyond English. ǣ, ȳ and Ē need a mark on top of a tall letter: 7 pixels and 2 of air do not
  hold it. The free 5 x 7 fonts I know do not have them `(?)`. Drawing about 350 letters is 40 hours and more `(g)`.
- **Size.** A pixel font is sharp only at its own size and its doubles. "Large" would have to be twice: 14 letters a
  line. So the player's choice of text size cannot be offered at all.
- **Long reading.** The chest holds 267 readings and the chronicle about 13,600 words. You turned down one
  hard-to-read font already (`pixel_ui.md`, decision 2).

**So:** the pixel font paints the signs. Gentium writes everything that is read. This is the "second layer" that
`pixel_ui.md` allows when it is the font for *all* running text. The mock-up already does it. Change one thing:
Pixelify Sans is a smooth font drawn to look like pixels, and is sharp at few sizes (`pixel_ui.md` A3). Use a true
bitmap font for the signs.

## C4. The options a player should have

Five, on one page of the settings. All take effect at once, with a card of sample text beside them.

| Option | Choices | What it changes |
|---|---|---|
| **Text size** | Small, **Normal**, Large, Largest: 18, 22, 28, 36 px at 1080p (16, 18, 23, 30 at 720p; 36, 44, 56, 72 at 4K) | Body, quiet text and numbers. Windows flow again; the tiler keeps its least widths, so at Largest the left column grows to hold 36 letters: from 29% of the width to 30% at 1080p and to 37% at 720p. Signs do not change: at 21 px capitals they are already as big as Largest. Largest is twice Xbox's least, which is what its guideline asks |
| **Font** | **The game's**, Plain | Plain sets every word, signs too, in one plain sans face (Atkinson Hyperlegible, free to ship `(?)`; check that it has þ, ð and ā before it goes in, else it falls back to Gentium for those). Xbox's guideline asks for one sans face and one unstyled face: this is both |
| **Turning of lines** | **As spoken**, Slow, At once | A line in another tongue comes up in his words and turns to yours word by word. As spoken: 80 to 170 ms a word, as the mock-up has it, slower where you half know the tongue. Slow: two and a half times that. At once: your words are there at once, with his in quiet text beneath. In all three, any key ends the turn, and the words you do not know stay in gold. If the machine asks for less motion, the game starts on At once |
| **Solid windows** | **Off**, On | Slips become 100% ink: no chart shows through the words |
| **Strong edges** | **On**, Off | The warm-white line round windows and the two-tone edge on chart marks (B5). On by default: it is what makes the chart readable to every eye |

And three things that are not options, because they are always so:

- **Reading costs no days.** The clock is held while a window of words is open (ruled 4 October; `hands.md` rule 6).
  A slow reader loses nothing.
- **A signal is never colour alone.** A word or a shape goes with it.
- **No word is drawn into a picture.** Every word on screen is text, so that size, font and, one day, a reading voice
  can reach it.

## C5. What a programmer must build

Two nights `(g)`, on top of `unity_ui.md` section 4.

1. One number, `TextScale` (0.82, 1, 1.27, 1.64), and one rule: body = the larger of 18 and 22 x the screen's
   height / 1080, times `TextScale`, never under 16; quiet = 0.82 of body, never under 16. Set in the panel as that
   size divided by the frame's multiple. Remove every `em` smaller than 0.82 from the styles.
2. Gentium as a smooth font asset; the pixel font as a bitmap asset at its own size (`unity_ui.md` 4.2). At 720p,
   look at Gentium at 16 and 18 px in a capture before trusting it: if it is soft, make a hinted bitmap asset for
   those two sizes `(?)`.
3. Add `GentiumBookPlus-Italic.ttf`. Add a fall-back font that has ⁊, ѧ and ѹ (Junicode is the medievalist's face and
   should `(?)`; check with the three-line fontTools test before it goes in).
4. A test: every character in every data file is in the font chain. It would have caught the three.
5. The tiler's least widths in letters (36 for sentences, 24 for rows), worked out from the size.
6. The five options, saved, and the sample card beside them.

---

## Yours to rule

1. **Byzantium: the sea, or "yours"?** By hue alone it cannot be both. I propose: the world stays purple, and "yours"
   is a Byzantium band inside a two-toned mark (B5).
2. **Mend for all four sights (17 values) or for red and green only (13)?** I propose all. For the first release it is
   two values in Tide either way.
3. **Faces by the kit, drawn as whole faces first (A3, A11)?** If yes, the first eight are stage 1's eight.
4. **The scene, harbour and ship slots do not fit layout G (A12).** Keep the sizes and a safe middle of 256, or change
   the slots?
5. **No card pictures in the first release (A13)?** Eight backs and the speaker's face instead.
6. **Body text 22 px at 1080p, four sizes, Gentium for all that is read (C2, C4)?**

## Not checked

- No web search was possible. From memory only, marked `(?)` above: how RimWorld, Wildermyth, Darkest Dungeon, Battle
  Brothers, the Mii and the LPC generator build faces; the Fire Emblem portrait size; the figure of 10 dE for classes
  on a map; how many people have each colour-blindness; Valve's rule for the Deck; what Atkinson Hyperlegible and
  Junicode contain; the dates of mitres and of Thor's hammers.
- Whether the brown, blue and undyed ramps of A4 are each used for nothing else on their layer once real faces are
  drawn. The contact sheet will show a wrong swap at once.
- Every hour in A10, A12 and A13 is a guess for a learner. Time the first pieces.
- How Gentium looks at 16 and 18 px in Unity. Unity was not run.
- The mends were checked by the script's own sums, not by eye on the chart. Recolour one tile and look before the
  values go in.
- CK2's portrait size, and whether its child picture is one image in every version: the wiki page gave the layers and
  ages, not those.

## Sources

- Crusader Kings II wiki, "Portrait modding": https://ck2.paradoxwikis.com/Portrait_modding (fetched)
- Lucas Pope, Papers, Please devlog, "Timelapse: Drawing Faces" (13 Dec 2012) and "Even More Faces" (21 Mar 2013):
  https://dukope.com/devlogs/papers-please/timelapse-faces-1/ (fetched)
- Dwarf Fortress wiki, "Graphics token": https://dwarffortresswiki.org/index.php/Graphics_token (fetched)
- Machado, Oliveira and Fernandes (2009), "A physiologically-based model for simulation of color vision deficiency":
  https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html (fetched: the matrices)
- libDaltonLens, for the constants of Viénot 1999 and Brettel 1997:
  https://github.com/DaltonLens/libDaltonLens (fetched)
- Sharma, Wu and Dalal (2005), the CIEDE2000 test pairs (four are in the script's own check)
- WCAG 2.x, contrast ratio: 4.5 for text, 3 for large text and for lines and marks
- Xbox Accessibility Guideline 101, text display:
  https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101 (fetched: 18 px at 1080p
  and 36 at 4K for a PC, 26 and 52 for a console; scale to 200%; a sans face and an unstyled face; lines of 80 at most)
- Game Accessibility Guidelines, "Use an easily readable default font size":
  https://gameaccessibilityguidelines.com/use-an-easily-readable-default-font-size/ (fetched: 28 px at 1080p)
- In the project: `Bake/art/artdraw.py`, `artscene.py`, `pull_art.py`; `Art/art_list.csv`;
  `Assets/Kapman/Game/Data/places/*.json`; `Assets/Kapman/Code/Game/Map/MapPalette.cs`;
  `Assets/Kapman/Game/UI/Fonts`; `Design/ui_mock/template3.html`
