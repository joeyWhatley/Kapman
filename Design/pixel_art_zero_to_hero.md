# Pixel art from zero to hero: your plan, and the Byzantium palette

**The ask (4 Oct 2026):** "I just want to learn pixel art so if you could research a plan for that from zero to hero",
and "I want the game theme to be around the colour Byzantium".

Pixel art is the most learnable of the game arts, because every decision is visible: one pixel, one colour, on or off.
People who draw an hour a day are making usable tiles in two weeks, good tiles in two months, and characters that
move well in four. The plan below is twelve weeks of about an hour a day, each week with one skill, one exercise that
goes straight into the game, and the one resource to read. The game's palette is fixed first, so everything you make
from day one matches.

---

## 1. The Byzantium palette

Byzantium is the hex colour #702963: a deep red-purple, the colour of the imperial silk and the murex dye that came
to your merchant up the Rus rivers and the Rhine, worth more than gold a yard. In the game it is the **key colour**:
the colour of the thing most worth having, the UI frame, the dusk, the shadow in every ramp.

The palette (`Design/palette_byzantium.gpl`, 40 colours, swatch sheet in `palette_byzantium.png`; load it in
Aseprite with Palette > Load) is built on three rules:

1. **Shadows lean to purple, lights to gold.** Every ramp's dark end is pulled toward the purple ramp and its light end
   toward warm white. That is what makes a palette look like one world instead of a paintbox.
2. **One ramp per material**, four to seven steps: purple (6), sea (7), land green (5), earth and timber (6), stone (4),
   warm accents (4), sky (4), snow and white (4). Forty colours is plenty for a whole game; Celeste uses fewer.
3. **Byzantium itself is rare.** It appears on the cloth in the hold, the UI border, a king's cloak, the deepest band of
   dusk, and nowhere on the ground. Rarity is what makes a key colour read.

Where the colours go:

| On screen | Ramp |
|---|---|
| open sea, river, flats | sea ramp; its deepest step is `abyss` (near the purple ramp) so night water and the night sky meet |
| sand, mud, jetties, hulls, thatch | earth ramp |
| grass, marsh, reeds, woods | land ramp, with `wood shade` as the darkest green |
| dykes, querns, the church | stone ramp (purple-grey) |
| sails, foam, snow, frost, gull wings | the whites and snow |
| blood, rust, gold, fire | the warm accents |
| dawn and dusk | `dusk rose`, `dawn peach`, `morning gold`, then `byzantium` and `plum` as the last light goes |
| the UI | `night` glass, `byzantium` border, `warm white` text |
| seasons | spring: `meadow` up; summer: `grass`; autumn: swap `meadow` for `rust` and `gold`; winter: swap the greens for `frost` and `snow` |

Seasons and times of day are **palette swaps of the same tiles**, which is the oldest pixel-art trick and the reason
the ramps are kept separate.

## 2. The tools (one evening)

| Tool | For | Cost |
|---|---|---|
| **Aseprite** | everything: tiles, sprites, animation, palettes, export | $19.99 (Steam or itch.io) |
| **Lospec** (lospec.com) | palettes to study, the pixel-art tutorial list, the "Pixel Art Where to Start" page | free |
| **Tiled** or **LDtk** | laying out a hand-made area, importing into Unity (SuperTiled2Unity, LDtkToUnity) | free |
| **Laigter** | normal maps from your sprites for the game's lighting (week 11) | free |
| a cheap drawing tablet | optional; most pixel artists use a mouse | $30-60 |

Set Aseprite up once: a new file 320x180 for mockups, the Byzantium palette loaded and locked, grid 16x16 on,
pixel-perfect pencil on, Shift+click for lines. Save as .aseprite; export PNG at 1x only.

## 3. Learn: the twelve weeks

Each week: one skill, one thing drawn for the game, one thing to read. "Game" means I take what you drew and put it
on the real map that week, so you see it in place.

| Week | Skill | You draw for the game | Read or watch |
|---|---|---|---|
| **1** | Pixel control: straight and curved lines, pixel-perfect curves, no doubles, no jaggies; the palette | 16x16 plain tiles: grass, sand, water, mud; three variants each | Saint11 "Lines", "Curves"; Lospec "Pixel Art: Where to Start"; Pixelblog 1 (palettes) |
| **2** | Shapes and clusters: thinking in masses, not outlines; selective outlines; shading with two steps | the water-sand and sand-grass dual-grid sets (15 tiles each) | Pixelblog 20 (Top Down Tiles), Saint11 "Shading", Brandon James Greer "Why pixel art looks like this" |
| **3** | Texture and dither: when to dither (rarely), noise versus pattern; reading tiles at a distance | grass-marsh, grass-forest, marsh-water sets; reeds; a willow, an alder, an oak (Pixelblog 44) | Pixelblog 43 (Top Down Tiles 2), 44 (Top Down Trees), Saint11 "Dithering" |
| **4** | Light: one light direction (top-left) for everything; form shadow versus cast shadow; hue shift in the shadow | a plank hall, a wattle house, a boat shed, a jetty, a well, fences, from the Dorestad notes | Pixelblog 21 (Top Down Objects), 45 (Bricks, Walls, Doors), Saint11 "Light" |
| **5** | The 3/4 view: how roofs, walls and the ground meet; consistent tile heights; depth cues | Dorestad's waterfront block, laid out in Tiled over the generated map | Pixelblog 35 (Top Down Interiors) for the view logic; AdamCYounis "Top-down tilesets" |
| **6** | Character design small: silhouette first; 16x24 and 32x32 bodies; faces in six pixels; clothing as colour blocks | a merchant, a crewman, a Frisian woman, a monk, a Dane: standing, facing down, up and side | Pixelblog 22 (Top Down Character Sprites), Saint11 "Character design", Pixel Logic ch. on characters |
| **7** | Animation 1: idle and the 4-frame walk; sub-pixel motion; the bob | the merchant's walk in three directions (side mirrored) | Pixelblog 8 (Intro to Animation), 55 (Top Down Character Animation), Saint11 "Walk cycle" |
| **8** | Animation 2: anticipation and follow-through; 6-8 frame actions; looping | rowing (a crewman at an oar), hauling a line, a man coiling rope; water edge, smoke, a flag, a gull | Pixelblog 56, 58; Saint11 "Smoke", "Fire", "Water" |
| **9** | Big tiles and props: 32x32 and 48x48 objects; breaking the grid with overlays | a Frankish wine cask stack, querns, a bale of Frisian cloth, a fish rack, a cart, cattle, sheep, pigs, a horse | Pixelblog 34 (On The Farm), 51 (City Builder) |
| **10** | Palette craft: seasons and hours as swaps; limited-colour discipline; the Byzantium accents | the autumn, winter, snow and frost swaps of every tile; the dusk ramp over the waterfront | Pixelblog 1 again, Lospec palette studies, Pixel Logic on colour |
| **11** | Light in the engine: normal maps for sprites, so the game's sun and fires light your art | normal maps for the houses and trees with Laigter; a firelit jetty at night | Laigter docs; the SpriteIlluminator tutorial for the idea |
| **12** | Finish: polish pass, consistency check across everything; the eight-direction merchant if you want him | a full screen of Dorestad at dawn in spring and at dusk in winter, as the game's first look | Pixelblog 62 (Landscape Backgrounds); your own eye |

After week 12 you draw the world one terrain pair and one town at a time, and every tile you add appears across the
whole map by itself.

## 4. The resources, ranked

1. **Pedro Medeiros (Saint11)**, saint11.org: one-image animated tutorials on every fundamental. Free. Start here.
2. **Slynyrd's Pixelblog**, slynyrd.com/pixelblog-catalogue: a free 62-part course by Raymond Schlitter. The numbers
   above are its entries.
3. **Pixel Logic** by Michafrar (michafrar.gumroad.com, about $10): the one book; everything in one place with examples.
4. **Lospec** (lospec.com): palettes, the tutorial index, and the "Pixel Art Where to Start" guide.
5. **YouTube:** AdamCYounis (long Aseprite sessions, tilesets, lighting), MortMort (short, sharp tips), Brandon James
   Greer (why pixel art reads), Pixel Pete (tiles), Mislav Majdandžić (characters).
6. **Reference, not tutorials:** Stardew Valley, Eastward, Sea of Stars, Owlboy, Hyper Light Drifter, Celeste, Kingdom
   Two Crowns, Moonlighter, Unsighted. Study one screen of each at 1x and ask what each pixel is doing.
7. **Period reference for what to draw:** Research/dorestad_c800_notes.md, the Oseberg and Gokstad finds, the
   Utrecht Psalter's drawings (c. 830, people and boats of your own decade), the Bayeux Tapestry (ships, halls, horses,
   sails), Hedeby's reconstructions.

## 5. The rules that save months

- Fix the pixel size and the palette on day one and never change them.
- Draw at 1x; zoom to work, but judge at 1x and at the game's 2x or 3x only.
- One light direction for everything you ever draw.
- Fewer colours than you think: a tile in three colours, a character in six.
- Outlines are a choice, not a default: selective outlines, darker versions of the fill, never black lines on everything.
- Draw tiles, variants and objects. Never draw a map by hand: the bake lays the map.
- Mirror side-facing sprites for left and right until the game proves it needs more.
- Keep one .aseprite per tileset, one per character, the palette locked, layers named.
- Export sheets with Aseprite's "Export Sprite Sheet", 1x, no padding; Unity slices them.
- Show your work at 1x to someone who does not draw. If they can tell what it is in a second, it works.

## 6. How this meets the pixel-map plan

Weeks 1-5 produce exactly what Design/research_pixel_map_3d_boat.md section 5 needs: the terrain-pair tilesets the
autotiler lays on the real Rhine delta, and the hand-made waterfront. Weeks 6-8 give the people on the jetties and the
crew on deck. Weeks 10-11 give the seasons and the light that the weather and clock already drive. The 3D boat and
water are mine; the land and the people are yours; the palette is shared.
