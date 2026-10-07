<!-- Research pass, 4 Oct 2026, late evening. Web searches done inline in one session (about 25 searches). -->

# Kapman: a 3D boat and water over a pixel-art world, and how you learn to draw it

**The ask (4 Oct, evening):** "research ideas on how I can incorporate the 3D simplified but aesthetically pleasing boat
and water but a pixel art map and land environment from a top down perspective", and "research work flow for me to
create this pixel art map from scratch at a high detail from knowing nothing to easily creating the pixel art map and
characters from scratch".

**The short answer.** It has been done, in both directions. Dave the Diver puts pixel people in a 3D sea. Abandon Ship
puts 3D ships on a painted 2D sea. Octopath Traveler puts pixel sprites in a 3D world with modern water. Nobody has
done exactly your mix (3D boat and live water, pixel land, 793 AD), which is a good sign. The way to make it hold
together is to push the boat and water through the same pixel grid as the land, live, every frame. The land is
then drawn by you, tile by tile, and laid out by the bake from the real terrain data. You paint tiles, not maps.

Two things follow that you should decide early:

1. **Pipeline.** Every pixel-art tool for Unity lives in URP. HDRP has no 2D renderer, no 2D lights and no Pixel
   Perfect Camera, and ray tracing and DLSS mean nothing once the frame is 640 pixels wide. The pixel direction means
   URP. The LiveWater sim, the sky and time code, the weather and the sound carry over; the HDRP water surface and the
   ray tracing stay in the tank as a reference.
2. **Zoom.** Pixel art only scales by whole numbers. The close view and the far view become two or three fixed steps,
   not a free zoom. The 2D chart stays the far view.

---

## 1. What other games did

| Game | Mix | What to take |
|---|---|---|
| **Dave the Diver** (Mint Rocket, 2023) | Pixel characters and small fish; 3D boats, buildings, big bosses and the sea | Pixel and 3D can sit side by side if the 3D is flat-shaded and the palette is shared. 3D for the things that must move smoothly (your boat, the water). |
| **Abandon Ship** (Fireblade, 2018) | 3D ships with a painterly shader on a 2D painted sea and map | The ship is the only 3D thing and it carries the whole scene. Weather and water were done as effects on the 2D plane. |
| **Octopath Traveler / II, Dragon Quest III HD-2D** (Square Enix, UE4) | Pixel sprites in 3D sets with real lighting, depth of field, water with reflections | The opposite of your mix, but it proves the rule: one lighting model over everything, and the camera held to angles where the pixel art does not break. |
| **Sid Meier's Pirates!** (2004) | A 3D ship on a stylized, oversized map, seen from above | The sailing map works with a small 3D ship on a map that is not to scale. The ship's size sells the scene, not the map's. |
| **Sea of Stars, Eastward** | Pure pixel art with dynamic light and normal maps | Pixel land can take a real sun, a real night and firelight if the tiles carry normal maps. |
| **Dead Cells** | 3D models rendered to pixel sprite sheets | The cheap way out. It loses the live water, so not for your boat. |

Sources: Dave the Diver on Game Developer and Nintendo World Report; Abandon Ship's Cubebrush article and Killscreen;
Octopath Traveler II's Unreal Engine interview; the Wikipedia pages for HD-2D and Pirates! (links at the end).

## 2. Three ways to marry the boat and the land

### A. One pixel grid for everything (recommended)

Render the whole frame, boat and water included, at a low internal resolution (for example 640x360, shown 3x on a
1080p screen), with the camera snapped to the world pixel grid. The boat's 3D model becomes pixel art live: a
limited palette with dither, a one-pixel outline, flat lit faces. The water sim shows as pixel water, with its
waves, foam and the boat's wake still coming from the physics. The pixel land tiles sit in the same grid, so
nothing looks pasted on.

- **How:** the t3ssel8r technique (the camera snapped to the pixel grid, with a sub-pixel offset so motion is smooth);
  **ProPixelizer** ($30, URP only, per-object pixel size, outlines, palettes and dither, mixed pixel and non-pixel
  objects) or **UPixelator**; or our own: one low-res render target, a palette lookup and an outline pass. About
  two days to build ourselves, a day with ProPixelizer.
- **Why it wins:** one look. The boat and water you already simulate stay live and reactive ("the water answers
  anything") and still read as pixel art.
- **Cost:** URP. The tank's HDRP surface and ray tracing are left behind.

### B. Mixed resolution (the Dave the Diver way)

The land is chunky pixel tiles (each art pixel is 3 or 4 screen pixels). The boat and water are drawn at full screen
resolution with a flat, banded, outlined shader in the land's palette. The boat holds smooth motion and fine waves.

- **Why:** keeps every wave. Fits HDRP if we accept hand-rolled integer scaling and unlit sprites with no 2D lights.
- **Risk:** the two resolutions fight. Dave the Diver gets away with it because its 3D is big and its pixel art is
  small; your boat is small on a big land. Reviewers call Dave's mix "somehow" balanced, which means it took a lot
  of tuning.

### C. Pre-rendered boat sprites (the Dead Cells way)

Render the Gislinge to sprite sheets (16 headings x sail states x heel) and draw her as a sprite over a pixel sea.
Cheapest to run, no live water. Not what you asked for; listed so you know it exists.

**Recommendation:** A, with B's banded shader on the boat so her faces stay readable at 48 pixels long.

## 3. The camera and the perspective

"Top-down" pixel games are almost all 3/4 views: you see roofs and the south walls, as in Zelda and Stardew Valley.
A true 90-degree look-down makes a boat a plank with a stick. So:

- An orthographic camera pitched about 55-60 degrees from the horizontal, never rotating (pixel art breaks when the
  camera turns; Octopath found the angles it could allow and locked them).
- The boat as a 3D model under the same camera. Her hull, sail and oars show as a 3/4 view, which matches the tiles.
- North up. Rivers and coasts wind on the screen; the boat turns, the world does not.

## 4. Scale and pixel density

The boat sets the pixel. Some options at a 1080p screen:

| Internal frame | Scale on 1080p | 1 art pixel is | The Gislinge (12 m) | A 16-px tile | Screen shows |
|---|---|---|---|---|---|
| 640x360 | 3x | 25 cm | 48 px | 4 m | 160 x 90 m |
| 640x360 | 3x | 12.5 cm | 96 px | 2 m | 80 x 45 m |
| 960x540 | 2x | 25 cm | 48 px | 4 m | 240 x 135 m |
| 960x540 | 2x | 12.5 cm | 96 px | 2 m | 120 x 68 m |

- 640x360 at 3x is the classic chunky look (Celeste, Hyper Light Drifter). Fewer pixels per tile, so each tile is
  quick to draw.
- 960x540 at 2x is "hi-bit" (Owlboy, Eastward): more detail, more drawing per tile, and it suits a game that wants
  to show a real place.
- A 300 m wide Rhine at 25 cm per pixel is 1200 pixels, two screens. That is the river at 1:1 width, which is what you
  asked for, and it reads as big water.
- Zoom steps: 1x (the close view above), 2x out (the river bend), then the 2D chart. Each step is a whole number so
  the art stays sharp.

This does not settle the world scale (the 11 maps). It does say the scale maps' "true width" rule is the one that
looks right in pixel art, because a pixel river shrunk to a stream next to a 48-px boat looks wrong at once.

## 5. The land: paint tiles, not maps

The bake already knows, for every metre of the world, whether it is water, sand, marsh, grass, heath, forest, town or
tidal flat, from the real terrain data. So:

1. **The bake writes a class grid** (one class per tile cell, 4 m or 2 m) and the river and coast lines.
2. **Autotiling lays your tiles.** Use the dual grid (Oskar Stalberg's method): tiles sit on the corners where four
   cells meet, so each pair of terrains (water-sand, sand-grass, grass-forest...) needs only 15 tiles, not 47. Unity's
   Rule Tiles in the 2D Tilemap Extras package do the same with a bigger set; a dual-grid tile is a short script.
3. **You draw the tilesets:** 15 transition tiles per terrain pair, plus 3-6 plain variants per terrain so fields do not
   repeat, plus seasonal palettes (spring, summer, autumn, winter, snow, frost) as colour swaps of the same tiles.
4. **You paint the detail by hand where the player looks:** jetties, halls, fences, boats, reeds, the Dorestad
   waterfront. In Tiled or LDtk (both free, both import into Unity), or in the game's Build mode with the brushes.
5. **Objects as sprites** on top: trees, houses, people, animals. Drawn by you or bought (section 7).

The whole Rhine delta is then generated and correct, and you spend your drawing on the forty or so tiles and the
places that matter.

## 6. Light, weather and the day over pixel land

- **One sun for both.** The land tiles carry normal maps (made from the art with Laigter, free, or Sprite DLight /
  SpriteIlluminator, paid) so the same sun and moon that light the boat light the roofs and trees, and fires at night
  cast light on the ground. In URP this is the 2D lights system or a lit sprite material under the 3D camera.
- **Time of day as a colour grade.** A palette ramp per hour tints the whole frame: pixel art has always done dusk this
  way and it looks right.
- **Weather on the land:** rain and snow as pixel particle layers; fog as a soft overlay that the boat's 3D fog matches;
  tide shown by the water-sand tiles swapping along the shore (the bake knows the tide line).
- **The water** stays the live sim, shown through the same pixel grid: waves, foam, the wake, rain rings.

## 7. Tiles and sprites you can use or buy

Pixel art is not covered by the no-home-made rule for foliage, because here the drawing is the point. Still, for
speed and to learn from, these are commercially licensed:

| Pack | What | Licence | Price |
|---|---|---|---|
| Kenney Roguelike/RPG pack (1,700 tiles, 16x16), Tiny Town, Tiny Farm, Roguelike Characters | Top-down tiles and people | CC0 | Free |
| Viking Village RPG Tileset (carbonova, itch.io) | Great hall, huts, forest in spring and autumn | Commercial allowed | Name your price |
| Viking Fantasy Tileset (mariaparragames, itch.io) | 16x16 Viking houses, doors, windows | Commercial allowed | $6 |
| Top-Down Medieval Town 16x16 (pekinczyk, itch.io) | Buildings, walls, ground, props | Commercial allowed | $15 |
| Pixelart Village Tiles 32x32 (dreamypixelart, itch.io) | Terrain variants, 30+ props | Commercial allowed | $3.99 |
| Tile Rivers, Coasts, Seas, Terrain (dgbaumgart, itch.io) | 122 coast tiles, 38 river tiles | Check on purchase | Paid |

Caution: "Viking" packs are 10th-11th century fantasy. Dorestad c. 800 is Frisian and Carolingian: wattle and plank
halls, thatch, no stave churches, no horned anything. Use the packs for terrain and props and draw the buildings
yourself from the Dorestad notes in Research/dorestad_c800_notes.md.

## 8. Your workflow: from nothing to drawing the map and the people

### Tools (one evening to set up)

- **Aseprite** ($19.99, Steam or itch.io). The standard. Animation timeline, onion skin, tilemap mode, palettes,
  sprite sheet export. Free alternatives if you want to try first: Pixelorama or LibreSprite.
- **Lospec** (lospec.com/palette-list) for a palette. Pick one and stay with it: Endesga 32, Resurrect 64 or Apollo
  are common; 32-64 colours is enough for a whole game and it makes every tile match.
- **Tiled** or **LDtk** (free) for laying out hand-made areas; SuperTiled2Unity / LDtkToUnity bring them into Unity.
- **Laigter** (free) for normal maps later.

### Learn (free, in this order)

1. **Pedro Medeiros (Saint11)** animated one-image tutorials: pixel lines, anti-aliasing, dithering, palettes, light.
   Half an hour each, free. saint11.org.
2. **Slynyrd's Pixelblog** (slynyrd.com): a free 62-part course. Read, in this order:
   1 Colour Palettes; 11 Landscape Pixeling; 20 Top Down Tiles; 43 Top Down Tiles Part 2; 21 Top Down Objects;
   44 Top Down Trees; 10 Water in Motion; 22 Top Down Character Sprites; 8 Intro to Animation; 55, 56, 58 Top Down
   Character Animation; 34 On the Farm; 45 Bricks, Walls, Doors.
3. **Pixel Logic** (Michafrar, about $10, PDF): the one book worth buying; covers everything above with examples.
4. **YouTube:** AdamCYounis (long form, Aseprite), MortMort (short tips), Brandon James Greer (why pixels read).

### Six weeks, an hour or two a day

| Week | You draw | What I do with it |
|---|---|---|
| 1 | Aseprite basics. Lines, curves, a 16x16 grass tile, a sand tile, a water tile. A palette chosen. | Set up URP pixel rendering in the tank: the Gislinge and the live water through a 640x360 grid. |
| 2 | The water-sand and sand-grass dual-grid sets (15 tiles each) plus 3 variants of each plain tile. | The bake writes the class grid for the Rhine mouth; your tiles are laid on the real delta the same day. |
| 3 | Grass-marsh, grass-forest, marsh-water. Reeds, a willow, an alder, an oak (Slynyrd 44). | Seasons as palette swaps; the tide line swapping tiles. |
| 4 | Dorestad's buildings: a plank hall, a wattle house, a jetty, a boat shed, a well, fences (from the Dorestad notes). | Objects placed from the Dorestad site plan; the waterfront laid out for you to touch up in Tiled. |
| 5 | A person: 16x24 or 32x32, facing up, down and to the side (mirror for the other side). Idle, then a 4-6 frame walk. | People on the jetties and in the market; crew on deck as sprites or as 3D under the pixel grid. |
| 6 | Animation: water edge, smoke, a flag, a gull. Eight directions for the main person if you want them. | Night light with normal maps; firelight; rain and snow layers. |

After that you draw the rest of the world one terrain pair and one town at a time, and every tile you make shows up
across the whole map by itself.

### Rules that save months

- Pick the pixel size (section 4) and the palette first, and never change them.
- Draw tiles, variants and objects, never a whole map by hand. The bake lays the map.
- One light direction for all tiles (top-left is the convention), so they can be mixed.
- Keep everything in one Aseprite file per tileset with the palette locked.
- Export at 1x. Scaling happens in the game, never in the art.
- Mirror the side-facing character for left and right until the game needs more.

## 9. What this means for the open decisions

- **Engine (research question 3):** URP, in a pixel-render branch of the tank first. HDRP, ray tracing and DLSS do
  not survive a 640-pixel frame.
- **Scale:** rivers at true width, land shrunk around them; the pixel grid makes the shrunk-river rule look wrong.
  The 11 maps are still to be compared for the land and sea scale.
- **Our own water (stages 2-4):** stage 2 (our own spectrum and depth-aware waves) matters more than before, because
  the look now depends on wave shapes that read at 25 cm per pixel. Stage 3 (our own shader) becomes a pixel shader
  with a banded palette, which is smaller than planned. Stage 4 (hull forces) is unchanged.
- **The three modes:** 3D voyage becomes the pixel 3/4 view; the 2D chart stays; the isometric ship view can be the
  same camera zoomed in one step.

## 9b. The layers: your question of 4 Oct, "the 3D river system, simulated oceans, weather etc. layered on top of a 2D pixel map"

Yes, and that is the whole idea of design A. One 3D scene, one orthographic 3/4 camera, one pixel pass at the end.
From the bottom up:

| Layer | What it is | 3D or 2D | Comes from |
|---|---|---|---|
| 1. The ground | the pixel land as flat textured quads on the ground plane (a tilemap laid by the autotiler on the bake's class grid); cliffs and dykes as a second tile row with a height | 2D art in the 3D scene | the bake + your tiles |
| 2. The water | our own simulated surface drawn only where the class grid says water: the rivers at true width, the flats, the sea; depth from the bake drives the waves (they slow and steepen in the shallows, the flats dry with the tide); LiveWater adds every body's wake and rings | 3D sim | LiveWater + the stage-2 spectrum |
| 3. The edge | the autotiler's shore tiles have a transparent water side, so the sim's foam and wet sand meet the pixel sand; the tide line swaps tiles as the water rises | 2D over 3D | the bake's tide line |
| 4. Things standing up | trees, houses, people, jetties as upright "paper" sprites facing the camera (the Octopath trick), sorted by depth with the boat, so the boat passes behind a headland or a hall | 2D sprites with depth | your art + the Dorestad site plan |
| 5. The boat and the ships | 3D models on the simulated water, heeling and pitching from the sim | 3D | the Gislinge and the AI fleet |
| 6. The weather | the weather engine as it stands: rain and snow as particles, fog as a volume, cloud shadows moving over land and water alike, the sun and moon lighting both (tiles carry normal maps so roofs and trees take the same light as the hull) | 3D over everything | the tank's sky and weather |
| 7. The pixel pass | the whole frame rendered at 640x360 or 960x540, camera snapped to the pixel grid, palette and dither, one-pixel outlines on the 3D things | the finish | ProPixelizer or our own |
| 8. The UI | your black glass panels, drawn after the pixel pass at full resolution so the text stays sharp | 2D | as now |

What this buys: the rivers, the sea, the tides, the storms, the fog and the day are all one simulation, the same one
the boat sails on, and the land is drawn by you. Nothing is painted twice. The risk to watch is the seam at layer 3:
a 3D shoreline meeting a pixel shoreline. The test in the tank is where we find out whether the tile edge hides it or
the sim's foam has to do the work.

## 10. Where to start

A one-week test in the tank: the Gislinge and the live water through a 640x360 pixel grid, a Kenney CC0 tileset
laid by the bake on the real Rhine mouth, one sun over both, filmed at dawn, noon, dusk and night. Then you judge it
against the HDRP tank films. If it holds, the pixel direction is set and your week-1 drawing exercises already have
somewhere to go.

## Sources

- HD-2D: https://en.wikipedia.org/wiki/HD-2D and the Octopath Traveler II interview
  https://www.unrealengine.com/en-us/developer-interviews/octopath-traveler-ii-builds-a-bigger-bolder-world-in-its-stunning-hd-2d-style
- Dave the Diver: https://www.gamedeveloper.com/design/dave-the-diver and https://nintendoworldreport.com/review/65313/dave-the-diver-switch-review
- Abandon Ship: https://cubebrush.co/blog/abandon-ship--developing-a-game-from-concept-to-execution and https://www.killscreen.com/upcoming-game-sea-battles-made-resemble-oil-paintings/
- Sid Meier's Pirates!: https://en.wikipedia.org/wiki/Sid_Meier%27s_Pirates!_(2004_video_game)
- ProPixelizer: https://assetstore.unity.com/packages/slug/177877 ; UPixelator docs: https://github.com/Radivarig/UPixelator_Documentation
- Pixel-art 3D camera snapping and the texel-splatting paper: https://arxiv.org/abs/2603.14587 ; 80.lv on 3D pixel-art animation: https://80.lv/articles/tips-on-convincing-animations-for-3d-pixel-art-games
- Unity 2D in URP (2D renderer, 2D lights, Pixel Perfect): https://docs.unity3d.com/6/Documentation/Manual/2d-urp-landing.html and https://docs.unity3d.com/Packages/com.unity.2d.pixel-perfect@2.0/manual/index.html
- Autotiling and the dual grid: https://www.redblobgames.com/articles/autotile/claude/ ; Godot dual-grid implementation for reference: https://github.com/pablogila/TileMapDual
- Unity Rule Tiles: https://uhiyama-lab.com/en/notes/unity/unity-tilemap-basics/
- Resolution and tile size: https://pixnote.net/en/learn/resolution and https://www.wayline.io/learn/pixel-art/0
- Sprite normal maps: https://www.codeandweb.com/spriteilluminator/tutorials/how-to-create-dynamically-lit-2d-scenes-for-unity
- Pixel water shaders for reference: https://estivalbloom.itch.io/pixel-water and https://thomasgamboa.itch.io/pixel-art-water-shader-stylized-water-with-foam-reflections-and-waves-urp
- Tools: https://www.pcgamer.com/best-pixel-art-maker-tools/ ; https://alternativeto.net/software/aseprite/
- Learning: https://www.slynyrd.com/pixelblog-catalogue ; https://www.slynyrd.com/blog/2019/10/21/pixelblog-22-top-down-character-sprites ; https://creativebloq.com/news/artist-makes-animated-gif-tutorials-to-teach-you-pixel-art ; https://beatcopgame.com/best-pixel-art-tutorials/
- Tilesets: https://opengameart.org/content/roguelikerpg-pack-1700-tiles ; https://carbonova.itch.io/viking-village-rpg-tileset ; https://mariaparragames.itch.io/viking-fantasy-tileset-pixel-art-game-asset ; https://pekinczyk.itch.io/top-down-medieval-town ; https://dreamypixelart.itch.io/pixelart-village-tiles ; https://dgbaumgart.itch.io/tile-rivers-coasts-seas-terrain
