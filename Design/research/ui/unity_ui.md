# Pixel-perfect UI in Unity 6 UI Toolkit, and a plan for the chart

6 October 2026. For Unity 6000.6.3f1 (the version in `ProjectSettings/ProjectVersion.txt`), HDRP 17.6, UI Toolkit runtime.

**How to read the marks.** "Checked" means I read it on the Unity 6.6 manual or script reference page named in Sources,
or in the files of the editor installed on this machine (`UnityEngine.UIElementsModule.xml` and `.dll`,
`Resources/CGIncludes/Internal/UnityUIE.cginc`). **(?)** means I could not check it: web searching was refused for this
run (the session's budget was already spent), so those points come from what I know of earlier Unity 6 versions.
Nothing here was run in Unity; every snippet needs one compile before it is trusted.

## 0. The short answer

1. **Scale by a whole number, in the panel.** Set the panel to Constant Pixel Size and set `PanelSettings.scale` from
   code to `floor(min(width / 640, height / 360))`. One point of UI is then 2 x 2, 3 x 3 or 4 x 4 screen pixels, always
   whole. This replaces the present Scale With Screen Size at 1920 x 1080, match 0.5, which gives 1.33 at 1440p and a
   fraction in every window size. That alone makes pixel art shimmer.
2. **Point filter on every texture**, set on the texture itself. USS has no `image-rendering` property; the UI shader
   samples each texture with that texture's own sampler (checked in `UnityUIE.cginc`).
3. **Pixel fonts are bitmap font assets**: render mode RASTER, sampling size equal to the font's design size, the atlas
   texture set to Point, `font-size` equal to the sampling size or a whole multiple of it.
4. **Move things with `translate`, not `left` and `top`.** The chart today sets `left`, `top`, `width` and `height` on
   every tile, port and ship each frame. That runs layout each frame, and each element is rounded to the pixel grid by
   itself. One container moved by one rounded `translate` fixes the speed and the seams together.
5. **Do not render the interface to a low-resolution texture.** It works (section 8), but it costs the pointer mapping
   and it forbids sharp reading text. Keep the render texture for the 3D window.
6. **Three fixed strips and one floating panel.** The day stick, the tally, the chest; and one panel at a time pinned
   beside the thing it is about (a port, a sail, her own mark), with a short tail to it. Section 10.

## 1. What the game has now

Read: `Assets/Kapman/Game/UI/*.uss` (4 sheets, 1612 lines; there is no UXML, every tree is built in C#),
`Assets/Kapman/Code/Game/UI/*.cs`, `Assets/Kapman/Code/Game/Map/MapView.cs`, `MapTable.cs`, `ChestPanel.cs`, `MapArt.cs`,
`Assets/Kapman/Game/UI/GamePanel.asset`, `Assets/Kapman/Editor/Game/GameSetup.cs`.

| Thing | How it is now | What it means for a pixel look |
|---|---|---|
| Panel | One `GamePanel.asset` for five UIDocuments (sorting 39 to 50). Scale With Screen Size, 1920 x 1080, match 0.5 | A fractional scale at every size but 1080p. Must change |
| Units | All USS in 1080p pixels: 15 px port names, a 1260 px market, a 440 px chronicle | Every number is redone at the 640 x 360 base |
| Font | Gentium Book Plus TTF through `-unity-font`, sizes 10 to 28 px; Noto Sans Runic | Smooth outline type. It does not sit on a pixel grid. A decision for you: section 4.5 |
| Look | `rgba(8, 9, 11, 0.6)` glass, 1 px black border, square corners, `text-shadow: 0 1px 3px` | Square corners and 1 px borders are already pixel-friendly. The blurred shadow is not |
| Chart tiles | `Image` elements, `ScaleMode.StretchToFill`, placed by `style.left/top/width/height` each frame in `Place()` | Layout every frame; each tile rounded alone, so hairline seams and one-pixel wobble |
| Tile textures | `MapArt.Read`: `FilterMode.Point` (good). `MapRenderer` tiles and the `Exploration` fog: `FilterMode.Bilinear` | The fog is a soft bilinear sheet over hard pixels: part of "the fog washes the picture out" |
| Zoom | Any value: `mpp * 1.18^n`, eased | A map texel is almost never a whole number of screen pixels |
| Ports | One element a port, `left/top` each frame, the note's string built each frame | Layout and garbage each frame |
| Ships | 138 marks, `left/top` and `style.rotate` each frame; her own mark the same | A rotated rectangle is drawn smooth at screen resolution: it breaks the grid |
| Course | `Painter2D` stroke, width 2, round joins, `MarkDirtyRepaint()` each frame | An anti-aliased vector line, rebuilt each frame |
| Cards | `map-card`, `map-ship-card`, the port card: fixed to screen corners. Market: a 1260 px box. Encounters and the port's menu: mid-screen | Nothing is pinned to the thing it is about. `chart_game.md` names this as its first known gap |
| Drag | `ChestPanel.Chip`: pointer capture, a ghost label, `worldBound.Contains` on the drop places | The right pattern. It lacks a `PointerCaptureOutEvent`, so a lost capture can leave the ghost on screen |
| Hover | `HoverCards`: one card on the panel root, a 50 ms tick | Fine. Keep it |
| Big quads | `PlaceCut` cuts the fog to the view, because a quad a million pixels wide failed an assertion | Keep that rule: never hand UI Toolkit a quad far larger than the screen |

## 2. Panel scale modes and whole-number scaling

### 2.1 The three modes (checked: Panel Settings reference and `PanelScaleMode`, 6.6)

| Mode | What it does | Whole-number? |
|---|---|---|
| Constant Pixel Size | "Elements stay the same size, in pixels, regardless of screen size." One point = `scale` screen pixels | Yes, if you keep `scale` whole |
| Constant Physical Size | The same physical size by DPI (`referenceDpi`, `fallbackDpi`) | No. DPI ratios are fractions |
| Scale With Screen Size | Grows and shrinks from `referenceResolution`, by Match Width Or Height, Shrink or Expand | Only by luck (a 1080p design on a 1080p screen) |

There is no integer-scale tick box in 6000.6 (checked: no such property on `PanelSettings`). You do it yourself:
Constant Pixel Size, and `scale` set from code.

### 2.2 The base size

Use **640 x 360**. It is the 16:9 base that fits every common screen whole: x2 = 1280 x 720, x3 = 1920 x 1080,
x4 = 2560 x 1440, x6 = 3840 x 2160. (480 x 270 gives x4 at 1080p and x8 at 4K, but 5.33 at 1440p.)
On other shapes the panel is simply larger than the base one way: 2560 x 1080 at x3 is 853.33 x 360 points;
1920 x 1200 at x3 is 640 x 400. Design every strip to hang from an edge or a corner, never to assume 640 wide.

### 2.3 The snippet

```csharp
using UnityEngine;
using UnityEngine.UIElements;

/// Whole-number UI scale: one UI point is k x k screen pixels. One of these in the scene, beside the first UIDocument.
[DefaultExecutionOrder(-500)]
public sealed class PixelPanelScale : MonoBehaviour
{
    public PanelSettings panel;                   // GamePanel.asset
    public Vector2Int baseSize = new Vector2Int(640, 360);
    [Range(0, 8)] public int force;               // 0: automatic; else this scale (an options-menu setting)
    public static int Scale { get; private set; } = 1;
    int w, h, forced = -1;

    void OnEnable() => Apply();

    void Update()
    {
        if (Screen.width != w || Screen.height != h || force != forced)
            Apply();
    }

    void Apply()
    {
        w = Screen.width; h = Screen.height; forced = force;
        int fit = Mathf.Max(1, Mathf.Min(w / baseSize.x, h / baseSize.y));   // integer division: the floor
        Scale = force > 0 ? Mathf.Min(force, fit) : fit;
        panel.scaleMode = PanelScaleMode.ConstantPixelSize;
        panel.scale = Scale;
    }
}
```

In `GameSetup.PanelSettings()` replace the three lines that set Scale With Screen Size with
`ps.scaleMode = PanelScaleMode.ConstantPixelSize; ps.scale = 3f;` so the asset is right before the script runs.
The asset exists already, so that function returns early: change the asset in the Inspector once as well.

### 2.4 Pitfalls

- **`PanelSettings` is an asset.** Setting `scale` in Play mode in the editor writes into the asset on disk, and it
  shows up as a changed file. Harmless; or work on a runtime copy (`Instantiate(asset)`) and give **the same copy** to
  all five documents, or they become five panels with five atlases.
- **Layout rounds to screen pixels, not to UI points.** UI Toolkit rounds each laid-out box to the device pixel grid
  (checked that `VisualElement.scaledPixelsPerPoint` and `AlignmentUtils.RoundToPanelPixelSize`,
  `FloorToPanelPixelSize`, `CeilToPanelPixelSize` exist in 6000.6; the rounding rule itself is from earlier
  versions (?)). At scale 3 a box at `left: 10.33px` lands on screen pixel 31: on the screen's grid, off the fat-pixel
  grid. So **whole numbers only in USS** for anything holding pixel art: no percentages, no `flex-grow` splits of odd
  widths, no centring an odd-width thing in an even-width box. In code round with `Mathf.Round`; `AlignmentUtils`
  rounds to screen pixels, which is the wrong grid here.
- **The odd strip at the edge.** At 2560 x 1080 the panel is 853.33 points wide. Give the root a whole size and let the
  last one or two screen pixels show the chart behind:
  `root.style.width = Mathf.Floor(Screen.width / (float)PixelPanelScale.Scale);` and the same for the height.
- **The Game view has its own zoom slider.** Anything but 1x resamples the whole picture and looks exactly like a
  filtering bug. Judge at 1x with a fixed resolution, or in a build.
- **A window dragged to an odd size** changes the scale in steps (x3 to x2 as it shrinks under 1920 x 1080). Set a
  minimum window size of 640 x 360 in Player settings, and let the options menu offer x2, x3, x4 by name.
- **Windows display scaling** should not matter in Constant Pixel Size, since DPI is only read in Constant Physical
  Size (?).

## 3. Point filtering

There is no USS switch (checked: the 6.6 USS properties reference has no `image-rendering` and no filter-mode
property; `filter` in 6.6 is a post-processing effect on an element, not texture sampling). Filtering belongs to the
texture, and `UnityUIE.cginc` declares `_Texture0` to `_Texture7` each with its own sampler, so the texture's setting
is what the UI draws with.

**Imported textures and sprites** (in the Inspector, or by the importer script below):

| Setting | Value | Why |
|---|---|---|
| Texture Type | Sprite (2D and UI) | Sprites carry the 9-slice border |
| Filter Mode | Point (no filter) | The whole matter |
| Compression | None | Block compression smears a small palette |
| Generate Mipmap | Off | A mip is a blur |
| Non-Power of 2 | None | "To nearest" resamples the picture at import |
| Max Size | At least the real size | A smaller cap rescales it |
| Pixels Per Unit | 100, the same as the panel's Reference Sprite Pixels Per Unit | Keeps one texel = one point (section 5) |
| Mesh Type | Full Rect | Needed for tiled slicing (checked); no tight-mesh cuts at the edges |

One script, by folder, so no texture is forgotten:

```csharp
using UnityEditor;
using UnityEngine;

/// Every texture under .../Kapman/Game/UI/Pixel/ is imported as pixel art. Editor folder.
sealed class PixelArtImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace("\\", "/").Contains("/Kapman/Game/UI/Pixel/"))
            return;
        var t = (TextureImporter)assetImporter;
        t.textureType = TextureImporterType.Sprite;
        t.spritePixelsPerUnit = 100f;
        t.filterMode = FilterMode.Point;
        t.mipmapEnabled = false;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.npotScale = TextureImporterNPOTScale.None;
        t.maxTextureSize = 8192;
        var s = new TextureImporterSettings();
        t.ReadTextureSettings(s);
        s.spriteMeshType = SpriteMeshType.FullRect;
        t.SetTextureSettings(s);
    }
}
```

**Textures made in code** (the chart): `filterMode = FilterMode.Point`, `wrapMode = TextureWrapMode.Clamp`, no mip
chain. `MapArt.Read` already does this. `MapRenderer.cs` line 110 and `Exploration.cs` line 45 say `Bilinear`: change
both if they stay on screen in the pixel look.

**The dynamic atlas.** UI Toolkit copies small textures into one shared atlas to save draw calls. In 6000.6 it keeps a
point page and a bilinear page (checked: the names `m_PointPage` and `m_BilinearPage` are in
`UnityEngine.UIElementsModule.dll`; the script reference says "On GLES3 (and later) devices, the atlas supports more
than one filter mode"). So a Point texture stays Point inside the atlas. What may enter (checked,
`DynamicAtlasFilters`): not readable from script, not larger than Max Sub Texture Size (64 in `GamePanel.asset`), a
plain format of 8 bits or less a channel, the atlas's colour space. `GamePanel.asset` has `m_ActiveFilters: 31`: all
five filters on, the Filter Mode one too. That is harmless for Point textures.

Pitfalls:

- **A small texture you change in code** keeps its old picture in the atlas until you call
  `RuntimePanelUtils.SetTextureDirty(panel, texture)` (checked). A readable texture (the fog) never enters, so it is
  not affected.
- **Bleeding inside the atlas.** If a sliced frame ever shows a neighbour's colour on its edge, keep it out of the
  atlas with `panel.dynamicAtlasSettings` and a `customFilter` (checked that the property exists), or put the UI
  sprites in a Sprite Atlas asset with padding 2 and Point filter. I have not seen this in 6000.6 and could not test (?).
- **Drawn smaller than it is**, a Point texture drops rows and columns. Never shrink pixel art: pick the coarser level
  of the chart's pyramid (`MapArt/world/pyramid.json` has four) and enlarge that by a whole number.
- **`ScaleMode.ScaleToFit` and `-unity-background-scale-mode: scale-to-fit`** work out a fractional size. Give the
  element the exact size (texels times a whole number) and stretch to fill.
- **`border-radius`, `Painter2D` strokes, `rotate` and `scale`** are all drawn smooth at screen resolution. In a pixel
  interface each of them is a sprite instead (section 6.4 for her heading, 6.5 for the course).
- **Tint is safe**: `-unity-background-image-tint-color` multiplies the colour and does not filter. One white sprite
  can serve the whole Byzantium palette.

## 4. Bitmap and pixel fonts without blur

### 4.1 Why text blurs

UI Toolkit draws text from a font asset: an atlas texture of glyphs. A TTF given through `-unity-font` (as now) gets a
dynamic **SDF** asset made for it: smooth edges at any size, which is right for Gentium and wrong for a pixel font.
The shader has a second path for bitmap glyphs, `uie_std_frag_bitmap_text`, which takes the alpha straight from the
atlas with the atlas texture's own sampler (checked in `UnityUIE.cginc`). So a pixel font is sharp when three things
are true: the glyphs were rasterised at their design size, the atlas is Point-filtered, and each glyph texel covers a
whole number of screen pixels.

### 4.2 Making the font asset

The manual (6.6, "Font assets") lists the bitmap render modes: SMOOTH and SMOOTH_HINTED (anti-aliased), RASTER and
RASTER_HINTED (not anti-aliased), COLOR; and it says for pixel art "any bitmap mode". For hard pixels that means
RASTER or RASTER_HINTED.

1. Right-click the TTF, Create, Text, Font Asset (the exact menu path moved between versions (?)), or use the Font
   Asset Creator.
2. In the font asset's Inspector, Generation Settings:
   - **Atlas Render Mode: RASTER_HINTED.** If any stem comes out a pixel off against the font's own specimen, try
     RASTER and keep whichever matches.
   - **Sampling Point Size: the font's design size**, the size at which one font pixel is one texel. For most pixel
     fonts that is 8, 10, 12 or 16; the font's page says which. A wrong value is the commonest cause of ragged glyphs.
   - **Padding: 2.** (The manual's 1:10 ratio is for SDF spread; a bitmap atlas only needs a gap.)
   - **Atlas 512 x 512**, Atlas Population Mode Dynamic.
3. **Set the atlas texture to Point.** Open the font asset in the Project window, select the atlas texture under it,
   set Filter Mode to Point (?: if the sub-asset's filter is not editable in 6000.6, set it in code as below).
4. Use it by `-unity-font-definition`, which takes precedence over `-unity-font` (checked).

```css
.px {
    -unity-font-definition: url("project://database/Assets/Kapman/Game/UI/Fonts/Pixel8.asset");
    font-size: 8px;              /* the sampling size, or a whole multiple: 16px, 24px */
    letter-spacing: 0;
    -unity-text-align: upper-left;
    -unity-text-generator: standard;
    color: rgb(236, 235, 230);
}
```

In code, for a font asset made at run time and to be sure of the filter on every atlas page:

```csharp
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public static class PixelFont
{
    /// A bitmap font asset from a TTF, at its design size, Point-filtered.
    public static FontAsset Make(Font ttf, int designSize)
    {
        // (?) the overload as I know it from Unity 6.0 to 6.3: font, samplingPointSize, atlasPadding, renderMode, width, height.
        // The method is in 6000.6 (found in UnityEngine.TextCoreTextEngineModule.dll) but has no doc entry: let the editor confirm it.
        var fa = FontAsset.CreateFontAsset(ttf, designSize, 2, GlyphRenderMode.RASTER_HINTED, 512, 512);
        Sharpen(fa);
        return fa;
    }

    /// Call after the asset loads, and again after a burst of new glyphs (a new atlas page starts Bilinear).
    public static void Sharpen(FontAsset fa)
    {
        foreach (var t in fa.atlasTextures)
            if (t != null)
                t.filterMode = FilterMode.Point;
    }

    public static void Use(VisualElement root, FontAsset fa, int size)
    {
        root.style.unityFontDefinition = new StyleFontDefinition(fa);
        root.style.fontSize = size;
    }
}
```

### 4.3 Pitfalls

- **`font-size` must be the sampling size times a whole number.** 8 px glyphs at `font-size: 10px` are resampled and
  no filter setting saves them. With the panel at scale 3, `font-size: 8px` is 24 screen pixels: whole. Good.
- **No outline, no soft shadow.** `-unity-text-outline` and a blurred `text-shadow` are SDF effects (the manual calls
  SDF fonts the "effects-capable" kind). For a bitmap font draw the shadow yourself: a dark slip behind the words (the
  port names have one already), or a second label one point down and right in a dark colour. A `text-shadow` with
  blur 0 may work on bitmap text; I could not test it (?).
- **Half-point positions.** A label is rounded to the screen grid, not the fat-pixel grid (2.4). Centred text in a box
  whose free space is odd sits half a point off: at scale 3 that is 1.5 screen pixels, and the glyphs come out uneven.
  Keep pixel text `upper-left` or `middle-left`, with whole paddings, and centre only where you control both widths.
- **Line height.** Face Info, Line Height, Ascent and Descent of the font asset must be whole numbers at the sampling
  size. If lines of a paragraph drift by a pixel, round them by hand in the font asset's Inspector.
- **Rich text bold and italic** are faked by dilating or slanting: both blur a bitmap font. Use a second font asset
  for the bold, or a colour, never `<b>` or `<i>`.
- **New atlas pages.** A dynamic font asset adds an atlas texture when the first is full; the new one may start
  Bilinear (?). Hence `Sharpen` above; or pre-fill the atlas with the characters the game uses and it never grows.
- **The characters you need.** The game writes ā, þ, ð, æ, ø, and runes. Most 8 px fonts have Latin-1 at best. Check
  the glyph table before choosing; set a fallback font asset for the missing ones (font asset Inspector, Fallback
  Font Assets), itself a bitmap asset, or the fallback glyphs will come out smooth among hard ones.
- **`-unity-text-generator: advanced`** switches to the newer shaping engine (checked that the property exists in
  6.6). Leave pixel text on `standard`: I do not know how the advanced generator treats bitmap atlases (?).
- **`-unity-editor-text-rendering-mode`** is for editor windows only; it does nothing in the game.
- **`-unity-text-auto-size: best-fit`** (checked, in 6.6) picks any size between two bounds. Never on pixel text.

### 4.4 Sizes at 640 x 360

An 8 px font gives 80 columns by about 36 lines on the whole screen. A panel of 200 x 120 points holds 12 lines of 25
letters: enough for a card (a fact, three choices, the odds as a word), tight for a market row with a word on it, and
too small for reading Beowulf. That is the real constraint on a pure pixel interface, and it leads to the choice below.

### 4.5 A decision for you: Gentium or a pixel face

You chose Gentium for being readable, and the chest holds 267 texts to read. Two honest roads:

| Road | What it is | For | Against |
|---|---|---|---|
| A. All pixel | One pixel font everywhere, 8 px for labels, 16 px for titles | One grid, one look; the smallest build | Long reading is tiring; needs a font with þ ð æ and runes |
| B. Pixel chrome, Gentium for reading | Frames, labels on the chart, numbers, buttons in the pixel font; the chest's texts and the cards' body in Gentium at screen resolution | Reads well; keeps your choice | Two resolutions on screen at once: smooth letters inside fat-pixel frames. Many pixel games do this; some players dislike it |

Road B is only possible with the whole-number panel scale of section 2 (a smooth font in the same panel simply draws
at screen resolution: `font-size: 5.33px` at scale 3 is a 16 px Gentium). It is impossible with a low-resolution
render target (section 8). That is the main reason I advise against the render target.

Candidate pixel faces, all to be checked for licence and glyphs before use; I could not open their pages in this run:
GNU Unifont (16 px, very wide coverage, said to include the runic block; OFL 1.1 since version 13 (?)), Pixel Operator
(CC0 (?)), Monogram (CC0 (?)), Departure Mono (OFL (?)), Silkscreen and Pixelify Sans (OFL, on Google Fonts (?)).
Whichever is used, its credit line goes in the credits file, as the rule for shippable data asks.

## 5. Nine-slice sprites in USS

### 5.1 The properties (checked: USS common properties and properties reference, 6.6)

```
-unity-slice-left: <integer>      -unity-slice-top: <integer>
-unity-slice-right: <integer>     -unity-slice-bottom: <integer>
-unity-slice-scale: <length>
-unity-slice-type: sliced | tiled
```

`sliced` stretches the centre and the sides; `tiled` repeats them, and "Image must be imported as a Sprite (2D and UI)
and have Mesh Type set to Full Rect" (checked, `SliceType.Tiled`). For a pixel frame with any texture on its edge
(rope, wood grain, a dither) use **tiled**: stretching a 1-texel dither into a bar is the classic mistake. A flat
one-colour edge can use `sliced`.

### 5.2 The frame for this game

The present look is a 1 px black line round dark glass. As one sprite: 8 x 8 texels, a 1 texel black edge, a 2 texel
inner bevel if wanted, the centre the glass colour with alpha. Border 3 on each side, set in the Sprite Editor.

```css
.px-panel {
    background-image: url("project://database/Assets/Kapman/Game/UI/Pixel/panel.png");
    -unity-slice-left: 3;
    -unity-slice-top: 3;
    -unity-slice-right: 3;
    -unity-slice-bottom: 3;
    -unity-slice-scale: 1px;
    -unity-slice-type: tiled;
    padding: 4px 5px;
    border-width: 0;
    border-radius: 0;
    background-color: rgba(0, 0, 0, 0);
}

.px-button         { background-image: url("project://database/Assets/Kapman/Game/UI/Pixel/button.png"); }
.px-button:hover   { background-image: url("project://database/Assets/Kapman/Game/UI/Pixel/button_hover.png"); }
.px-button:active  { background-image: url("project://database/Assets/Kapman/Game/UI/Pixel/button_down.png"); }
```

In code: `el.style.backgroundImage = new StyleBackground(sprite); el.style.unitySliceLeft = 3;` and so on, and
`el.style.unitySliceType = SliceType.Tiled;`.

### 5.3 Pitfalls

- **Pixels Per Unit changes the slice size.** The manual: "For sprites, Unity adjusts the `-unity-slice-scale` by the
  sprite's pixels-per-unit" against the panel's reference value (default 100); its example is a sprite at 16 with the
  panel at 100, where a 2 px scale becomes 0.32 px. A pixel-art pack imported at 16 PPU therefore draws its borders at
  a sixth of a point: invisible. **Keep every UI sprite at 100 PPU and the panel's Reference Sprite Pixels Per Unit at
  100** (it is, in `GamePanel.asset`), and the factor is 1.
- **Slices in USS or in the sprite, not both by accident.** A sprite's own border (Sprite Editor) is used when the USS
  slices are left unset (?); USS values, when given, are in texels of the image. Write them in USS and the sheet shows
  what is happening.
- **The element must be at least left + right wide and top + bottom high**, or the corners overlap. Give every sliced
  class a `min-width` and `min-height`.
- **Whole sizes.** With `tiled`, a side that is not a whole number of tiles long is cut at the end. Make the tile 1 or
  2 texels long, or size panels in steps of the tile.
- **A hover state that swaps the image** costs nothing if both images are in the atlas. A hover state that changes
  `border-width` or `padding` moves the layout: do not.
- **`background-size`, `background-repeat`, `background-position`** exist in 6.6 (checked) for un-sliced repeating
  fills such as a parchment or a wave pattern: `background-repeat: repeat; background-size: 16px 16px;` with a whole
  size, top-left position.
- **Nine-slice is not for the chart.** Tiles are plain stretched images at a whole magnification.

## 6. Labels and small panels pinned to places on a moving map

The chart is itself UI: no camera draws it. So "world to screen" is the game's own sum, in real metres:
`x_point = (worldX - viewWest) / mpp`, `y_point = (viewNorth - worldZ) / mpp`. (For the 3D window later, the built-in
helper is `RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPosition, camera)`; checked.)

### 6.1 One moving sheet instead of hundreds of moving elements

Now: every tile, port, ship and pin gets a new `left` and `top` each frame. Each write is a style change, then a
layout pass, then new geometry, and each box is rounded to the grid on its own (the seams).

Better: one element, the **sheet**, holds everything that lives on the chart, at positions that do not change while
the view only pans. Panning is one `translate` on the sheet.

```csharp
// Built once.
sheet = new VisualElement { pickingMode = PickingMode.Ignore, usageHints = UsageHints.GroupTransform };
sheet.style.position = Position.Absolute;          // size 0 x 0: children are absolute and overflow is visible
layer.Add(sheet);                                  // layer keeps overflow: hidden and takes the pointer

// A mark on the sheet: placed once per zoom level, never per frame.
static VisualElement Mark(VisualElement sheet, string cls)
{
    var m = new VisualElement { usageHints = UsageHints.DynamicTransform };   // set BEFORE it is added to a panel
    m.style.position = Position.Absolute;
    m.style.left = 0; m.style.top = 0;
    m.AddToClassList(cls);
    sheet.Add(m);
    return m;
}

// Each frame.
void PlaceSheet()
{
    // the sheet's origin is a fixed world point (originX, originZ); move it every few thousand points (see below)
    double px = (originX - viewWest) / mpp, py = (viewNorth - originZ) / mpp;
    sheet.style.translate = new Translate(Mathf.Round((float)px), Mathf.Round((float)py));
}

// When the zoom level changes, or a thing moves on the chart (ships, hourly):
void PlaceOnSheet(VisualElement m, double worldX, double worldZ)
{
    float x = Mathf.Round((float)((worldX - originX) / mpp)), y = Mathf.Round((float)((originZ - worldZ) / mpp));
    m.style.translate = new Translate(x, y);
}
```

Checked in 6.6: `usageHints` "can only be set when the VisualElement is not yet part of a Panel"; `DynamicTransform`
"optimizes rendering of a VisualElement for frequent position and transformation changes"; `GroupTransform` is for an
element that moves often and "has many descendants that have their hints set to DynamicTransform".

Why it is pixel-right: the sheet's translate is rounded to a whole point, every child sits at a whole point on the
sheet, so everything moves together in whole fat pixels and tiles cannot gap.

Pitfalls:

- **`translate` is not rounded for you.** Layout positions are snapped; transforms are not (?). Always round.
- **Rounding moves her in steps.** At scale 3 the chart pans in 3-pixel steps. At a ship's speed that is a slow, even
  tick, which is the look. If you would rather have the chart glide in single screen pixels under fat-pixel art (some
  pixel games do), round to `1f / PixelPanelScale.Scale` instead of to 1. Try both; it is one line.
- **Float precision.** The sailing world at the finest level is hundreds of thousands of points wide. A float is exact
  to a whole point up to 16 million, so it holds, but keep the sheet's origin near the view: when the view is more
  than 4096 points from the origin, move the origin there and re-place the children (once, not each frame).
- **Things far off the sheet still cost vertices.** Hide marks outside the view plus a margin with
  `style.display = DisplayStyle.None`, as `Ports()` already does, but only when the state changes, not each frame.
- **`overflow: hidden` on the layer** clips the sheet with a rectangle: cheap (a shader clip). Keep the layer's
  corners square; rounded corners turn the clip into a stencil mask and break batches (?).

### 6.2 Whole-number zoom

A map texel must cover a whole number of points. The pyramid has four levels (120 m a texel at level 0, finer above).
Allowed views: each level at x1, x2, x3, x4. The wheel steps through that list instead of multiplying by 1.18.

```csharp
// stops[i] = (level, magnification); metres per point = level.metresPerPixel / magnification
void OnWheel(WheelEvent e)
{
    int next = Mathf.Clamp(stop + (e.delta.y > 0 ? 1 : -1), 0, stops.Count - 1);
    if (next == stop) return;
    // keep the world point under the pointer where it is
    var local = layer.WorldToLocal(e.mousePosition);
    double wx = viewWest + local.x * mpp, wz = viewNorth - local.y * mpp;
    stop = next;
    mpp = stops[stop].metresPerPixel / stops[stop].magnification;
    viewWest = wx - local.x * mpp;
    viewNorth = wz + local.y * mpp;
    Relayout();                                   // tiles of the new level, every mark re-placed once
    e.StopPropagation();
}
```

Tile elements on the sheet are then `tilePixels * magnification` points square at
`(col * tilePixels * magnification, row * tilePixels * magnification)`: whole numbers, set once per zoom stop.
A short cross-fade between stops (opacity on the old and new tile groups, 120 ms) hides the jump. Do not ease `mpp`.

Check the tile size against `SystemInfo.maxTextureSize` and against the big-quad rule in section 1: a 4096-texel tile
at x4 is a 16384-point quad, 49152 screen pixels at scale 3. That is the kind of quad `PlaceCut` was written to
avoid. At high magnification cut each visible tile to the view with `Image.uv` exactly as `PlaceCut` does, with the
cut snapped to whole texels.

### 6.3 Labels

A port's name is a child of the sheet, placed once per zoom stop, with whole offsets from the dot. No per-frame work.

- **Which names show** is decided when the zoom stop or the known set changes, not each frame: sort by rank (where she
  is, where she is bound, ports with a word on a good aboard, then the rest), and hide a name whose box
  (`worldBound`, after the first `GeometryChangedEvent`) overlaps one already shown. Fifty ports: a trivial loop.
- **The note under a name** ("pays 12", "much wanted") is rebuilt when the picked good or the ledger changes. Today
  the string is built for every port every frame.
- **A slip behind the words**, as now, is right for bitmap text: no shadow is needed.

### 6.4 Marks that turn

`style.rotate` on her mark draws a smooth rotated box. In a pixel look a heading is a sprite frame: one strip of 16
hulls (22.5 degrees apart), or 8. Pick the frame by heading; no rotation at all.

```csharp
// heads: Sprite[16], frame 0 pointing north, clockwise.
static void Head(VisualElement mark, Sprite[] heads, float headingDegrees)
{
    int i = Mathf.RoundToInt(Mathf.Repeat(headingDegrees, 360f) / (360f / heads.Length)) % heads.Length;
    if (mark.userData is int was && was == i) return;      // only when the frame changes
    mark.userData = i;
    mark.style.backgroundImage = new StyleBackground(heads[i]);
}
```

All 16 frames are small, so they share the dynamic atlas: 139 ships are one batch.

### 6.5 The course line

`Painter2D` strokes are anti-aliased and cannot be made hard-edged (the manual has no switch for it (?)). A pixel
chart wants a dotted course: a 2 x 2 point square every 6 points along the lane. Draw the squares as plain quads in
`generateVisualContent`, in sheet coordinates, and call `MarkDirtyRepaint()` only when the course or the zoom stop
changes. Because the element is a child of the sheet it moves with the pan for free.

```csharp
sealed class CourseDots : VisualElement
{
    public readonly List<Vector2> dots = new List<Vector2>();   // whole-point positions on the sheet
    public Color32 ink = new Color32(20, 22, 26, 255);
    public CourseDots() { pickingMode = PickingMode.Ignore; generateVisualContent += Paint; }

    void Paint(MeshGenerationContext ctx)
    {
        int n = dots.Count;
        if (n == 0) return;
        var mesh = ctx.Allocate(n * 4, n * 6);                  // one call; under 65535 vertices
        for (int i = 0; i < n; i++)
        {
            float x = dots[i].x, y = dots[i].y;
            mesh.SetNextVertex(new Vertex { position = new Vector3(x, y, Vertex.nearZ), tint = ink });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x + 2, y, Vertex.nearZ), tint = ink });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x + 2, y + 2, Vertex.nearZ), tint = ink });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x, y + 2, Vertex.nearZ), tint = ink });
            ushort b = (ushort)(i * 4);
            mesh.SetNextIndex(b); mesh.SetNextIndex((ushort)(b + 1)); mesh.SetNextIndex((ushort)(b + 2));
            mesh.SetNextIndex(b); mesh.SetNextIndex((ushort)(b + 2)); mesh.SetNextIndex((ushort)(b + 3));
        }
    }
}
```

`MeshGenerationContext.Allocate` is checked in the 6.6 manual. The winding order (clockwise as written) is from
earlier versions (?): if nothing shows, swap the second and third index of each triangle.

### 6.6 A small panel pinned beside a thing

The panel is not on the sheet (it must stay whole on screen and above everything); it follows an anchor point.

```csharp
/// Keeps a panel beside a point of the chart: right of it if there is room, else left; never off the screen.
sealed class Pinned
{
    readonly VisualElement panel, tail, root;
    public System.Func<Vector2> anchor;            // the thing, in root points (sheet translate + its place)
    const float Gap = 6f;                          // whole points between the thing and the panel

    public Pinned(VisualElement root, VisualElement panel, VisualElement tail)
    {
        this.root = root; this.panel = panel; this.tail = tail;
        panel.usageHints = UsageHints.DynamicTransform;        // before root.Add(panel)
        panel.style.position = Position.Absolute;
        panel.style.left = 0; panel.style.top = 0;
        panel.RegisterCallback<GeometryChangedEvent>(_ => Place());   // its size is known only after the first layout
    }

    public void Place()
    {
        float w = panel.resolvedStyle.width, h = panel.resolvedStyle.height;
        float W = root.resolvedStyle.width, H = root.resolvedStyle.height;
        if (anchor == null || float.IsNaN(w) || float.IsNaN(W)) return;
        var a = anchor();
        bool right = a.x + Gap + w <= W - 2f;
        float x = right ? a.x + Gap : a.x - Gap - w;
        float y = Mathf.Clamp(a.y - 12f, 2f, Mathf.Max(2f, H - h - 2f));
        x = Mathf.Clamp(x, 2f, Mathf.Max(2f, W - w - 2f));
        panel.style.translate = new Translate(Mathf.Round(x), Mathf.Round(y));
        tail.EnableInClassList("left", !right);                 // the tail sprite flips; it is a child of the panel
        tail.style.top = Mathf.Round(Mathf.Clamp(a.y - y - 3f, 3f, h - 9f));
        bool off = a.x < 0 || a.y < 0 || a.x > W || a.y > H;    // the thing has left the view
        panel.EnableInClassList("adrift", off);                 // dim it, hide the tail
    }
}
```

Call `Place()` after `PlaceSheet()` each frame while the panel is open; it is two style writes on one element.

Pitfalls:

- **`resolvedStyle.width` is NaN until the first layout.** Hence the `GeometryChangedEvent`. Hide the panel with
  `visibility: hidden` (not `display: none`) for that first frame, or it flashes at 0, 0.
- **Flip with hysteresis.** If the thing sits near the middle line, the panel can flip sides every frame while she
  sails. Flip only when the present side is short by more than 8 points.
- **The clock is held while a panel is open** (`PanelClock`), so a pinned panel mostly follows a still thing; it moves
  when the player drags the chart. Let dragging the chart keep the panel (clamped at the edge, tail hidden) rather
  than close it.
- **Picking.** The panel takes the pointer; the tail and the sheet's labels should be `PickingMode.Ignore`. A click on
  the bare chart closes the panel; the existing `e.StopPropagation()` on port clicks stays.
- **Draw order** is tree order within a document and `sortingOrder` between documents. Put pinned panels in one "over"
  layer added after the map layer, and the dragged ghost in a last "drag" layer.
- **One at a time.** Two pinned panels overlap and fight for the same side. Opening one closes the other.

## 7. Drag and drop of items

UI Toolkit has no ready drag-and-drop at run time (`DragAndDrop` is the editor's). The manual's own example ("Create
a drag-and-drop UI inside a custom Editor window", 6.6) is the pattern: a `PointerManipulator` that captures the
pointer on down, moves the element on move, and on release looks for the slot under it. The chest's chips do the same
by hand. For the hold as a grid, the same again with snapping to cells:

```csharp
using UnityEngine;
using UnityEngine.UIElements;

/// Drag a cargo item between grids of cells. Cells are Cell points square; items are whole cells in size.
public sealed class ItemDrag : PointerManipulator
{
    public const int Cell = 12;                                  // points
    readonly VisualElement dragLayer;                            // last child of the root: always on top
    readonly System.Func<VisualElement, Vector2Int, bool> canDrop;   // (grid, cell) -> fits?
    readonly System.Action<VisualElement, Vector2Int> drop;
    VisualElement ghost, overGrid;
    Vector2 grab, start;
    Vector2Int overCell;
    bool dragging;

    public ItemDrag(VisualElement dragLayer, System.Func<VisualElement, Vector2Int, bool> canDrop,
                    System.Action<VisualElement, Vector2Int> drop)
    { this.dragLayer = dragLayer; this.canDrop = canDrop; this.drop = drop; }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(Down);
        target.RegisterCallback<PointerMoveEvent>(Move);
        target.RegisterCallback<PointerUpEvent>(Up);
        target.RegisterCallback<PointerCaptureOutEvent>(Lost);   // alt-tab, a panel closing under the pointer
        target.RegisterCallback<KeyDownEvent>(Key);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(Down);
        target.UnregisterCallback<PointerMoveEvent>(Move);
        target.UnregisterCallback<PointerUpEvent>(Up);
        target.UnregisterCallback<PointerCaptureOutEvent>(Lost);
        target.UnregisterCallback<KeyDownEvent>(Key);
    }

    void Down(PointerDownEvent e)
    {
        if (e.button != 0) return;
        start = e.position;
        grab = e.localPosition;                                  // where on the item it was taken
        target.CapturePointer(e.pointerId);
        e.StopPropagation();
    }

    void Move(PointerMoveEvent e)
    {
        if (!target.HasPointerCapture(e.pointerId)) return;
        if (!dragging)
        {
            if (((Vector2)e.position - start).sqrMagnitude < 9f) return;   // 3 points of slack: a click is not a drag
            dragging = true;
            ghost = new VisualElement { pickingMode = PickingMode.Ignore, usageHints = UsageHints.DynamicTransform };
            ghost.style.position = Position.Absolute;
            ghost.style.left = 0; ghost.style.top = 0;
            ghost.style.width = target.resolvedStyle.width;
            ghost.style.height = target.resolvedStyle.height;
            ghost.style.backgroundImage = target.resolvedStyle.backgroundImage;
            ghost.AddToClassList("item-ghost");
            dragLayer.Add(ghost);
            target.AddToClassList("item-lifted");                // dim the original, leave it in place
        }
        Vector2 p = dragLayer.WorldToLocal(e.position) - grab;
        // the grid under the pointer: Pick ignores the ghost because the ghost ignores picking
        var hit = target.panel.Pick(e.position);
        overGrid = hit;
        while (overGrid != null && !overGrid.ClassListContains("hold-grid"))
            overGrid = overGrid.parent;
        if (overGrid != null)
        {
            Vector2 g = overGrid.WorldToLocal((Vector2)e.position - grab + new Vector2(Cell / 2f, Cell / 2f));
            overCell = new Vector2Int(Mathf.FloorToInt(g.x / Cell), Mathf.FloorToInt(g.y / Cell));
            p = dragLayer.WorldToLocal(overGrid.LocalToWorld(new Vector2(overCell.x * Cell, overCell.y * Cell)));   // snap
            ghost.EnableInClassList("no", !canDrop(overGrid, overCell));
        }
        else
            ghost.RemoveFromClassList("no");
        ghost.style.translate = new Translate(Mathf.Round(p.x), Mathf.Round(p.y));
    }

    void Up(PointerUpEvent e)
    {
        if (!target.HasPointerCapture(e.pointerId)) return;
        bool ok = dragging && overGrid != null && canDrop(overGrid, overCell);
        var grid = overGrid; var cell = overCell;
        target.ReleasePointer(e.pointerId);                      // fires Lost, which cleans up
        if (ok) drop(grid, cell);                                // change the model; the view redraws from it
    }

    void Lost(PointerCaptureOutEvent e)
    {
        ghost?.RemoveFromHierarchy();
        ghost = null; overGrid = null; dragging = false;
        target.RemoveFromClassList("item-lifted");
    }

    void Key(KeyDownEvent e)
    {
        if (dragging && e.keyCode == KeyCode.Escape)
            target.ReleasePointers();                            // (?) or loop the captured ids; ends in Lost
    }
}
// use: item.AddManipulator(new ItemDrag(dragLayer, CanDrop, Drop));
```

Pitfalls:

- **Always handle `PointerCaptureOutEvent`.** Capture can be lost without a pointer-up. Clean up there and nowhere
  else, so there is one path. This is the gap in `ChestPanel.Chip`.
- **`e.position` is in panel points**, already divided by the panel scale. Never mix it with `Input.mousePosition` or
  `Mouse.current.position` (screen pixels, y from the bottom). To convert a screen position use
  `RuntimePanelUtils.ScreenToPanel(panel, new Vector2(x, Screen.height - y))` (checked; the y flip is in the 6.6 FAQ).
- **Drag a ghost, not the item.** Re-parenting the real element mid-drag changes its styles, its layout and its place
  in the picking order, and a lost capture leaves it orphaned. The model changes once, on a good drop.
- **The ghost ignores picking**, or `panel.Pick` returns the ghost every time.
- **The manual's example moves the element with `target.transform.position`.** That still compiles in 6000.6
  (`VisualElement.transform` is in the XML doc, not marked obsolete), but `style.translate` is the same thing through
  styles and is what the usage hints are written for. Use `translate`.
- **Keyboard focus for Esc**: the item needs `focusable = true` and focus, or register the key on the root instead.
- **Rotating an item** in a grid (R while dragging) swaps the ghost's width and height and its sprite; no `rotate`.
- **A drag from a panel to the chart** (a rune stick of a price onto a port, a thing onto a riddle) is the same code:
  the drop target is whatever `Pick` finds with the right class, a port mark included. That gives the game one verb
  for "use this on that".
- **Clicks after drags.** A `ClickEvent` can still fire on the item after a drag ends on it (?). Remember that a drag
  happened and swallow the next click.

## 8. A low-resolution render target for the whole interface

### 8.1 How it is done

`PanelSettings.targetTexture` (checked) makes the panel draw into a RenderTexture; the panel's size becomes the
texture's. You then show that texture on screen at a whole multiple, with Point filtering, and map the pointer back.

```csharp
using UnityEngine;
using UnityEngine.UIElements;

/// The whole interface drawn at base resolution, shown k times larger with hard pixels.
public sealed class LowResUi : MonoBehaviour
{
    public PanelSettings low;        // the game's panel: Constant Pixel Size, scale 1, Clear Color on, transparent
    public UIDocument presenter;     // a second document on its OWN PanelSettings (Constant Pixel Size, scale 1), sorted on top
    RenderTexture rt;
    Image view;
    int k, offX, offY;

    void OnEnable()
    {
        view = new Image { pickingMode = PickingMode.Ignore, scaleMode = ScaleMode.StretchToFill };
        view.style.position = Position.Absolute;
        presenter.rootVisualElement.pickingMode = PickingMode.Ignore;
        presenter.rootVisualElement.Add(view);
        low.SetScreenToPanelSpaceFunction(ScreenToLow);
        Build();
    }

    void OnDisable()
    {
        low.SetScreenToPanelSpaceFunction(null);                 // back to the default
        low.targetTexture = null;
        view?.RemoveFromHierarchy();
        if (rt != null) { rt.Release(); Destroy(rt); }
    }

    void Update()
    {
        if (rt == null || Screen.width / k != rt.width || Screen.height / k != rt.height || K() != k)
            Build();
    }

    static int K() => Mathf.Max(1, Mathf.Min(Screen.width / 640, Screen.height / 360));

    void Build()
    {
        k = K();
        int w = Screen.width / k, h = Screen.height / k;         // at least 640 x 360
        if (rt != null) { low.targetTexture = null; rt.Release(); Destroy(rt); }
        rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)   // 24: a stencil, for masks
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, useMipMap = false, antiAliasing = 1, name = "UI low" };
        rt.Create();
        low.targetTexture = rt;
        offX = (Screen.width - w * k) / 2; offY = (Screen.height - h * k) / 2;
        view.image = rt;
        view.style.left = offX; view.style.top = offY;           // the presenter panel is 1 point = 1 screen pixel
        view.style.width = w * k; view.style.height = h * k;
    }

    // screen position as UI Toolkit gives it (pixels) -> panel points of the low panel
    Vector2 ScreenToLow(Vector2 screen)
    {
        var p = new Vector2((screen.x - offX) / k, (screen.y - offY) / k);
        if (p.x < 0 || p.y < 0 || p.x >= rt.width || p.y >= rt.height)
            return new Vector2(float.NaN, float.NaN);            // outside: no pointer events
        return p;
    }
}
```

`SetScreenToPanelSpaceFunction` is checked in the 6.6 script reference: it "sets the function that handles the
transformation from screen space to panel space", and Unity's sample returns NaN for "no hit". Whether the position
handed in has y from the top or the bottom I could not settle from the page (Unity's sample flips y before a camera
ray, which suggests y from the top (?)). If clicks land mirrored top to bottom, use
`(Screen.height - screen.y - offY) / k` for y.

### 8.2 What it gives and what it costs

| | Whole-number panel scale (section 2) | Low-resolution render target |
|---|---|---|
| Sprites and borders on the fat grid | Yes, if USS numbers are whole | Yes, always, even with careless numbers |
| Rotation, curves, vector lines | Drawn smooth at screen resolution | Drawn at low resolution: chunky, and still anti-aliased against neighbours |
| Smooth Gentium for reading (road B) | Yes | **No**: all text is 360 lines high |
| Chart may glide by single screen pixels | Yes, if wanted | No |
| Pointer | Works | Your function; a wrong offset is a silent off-by-k bug |
| Extra cost | None | One texture and one full-screen quad: small |
| Extra things to break | None | Recreate on resize; two panels; tooltips, text fields and the IME all live in the low panel |

**Advice: do not use it for the interface.** It buys discipline you can have by keeping numbers whole, and it takes
away the readable text. Its one real use here:

### 8.3 Where the render texture belongs: the 3D window

"The 3D pixel world is a window." That is a camera drawing into a small RenderTexture (say 320 x 180, Point), shown in
an `Image` inside a framed panel on the chart at exactly x1 or x2 points:

```csharp
var rt = new RenderTexture(320, 180, 24) { filterMode = FilterMode.Point, antiAliasing = 1 };
windowCamera.targetTexture = rt;                 // HDRP: also turn off its post anti-aliasing, or the pixels soften
var img = new Image { image = rt, scaleMode = ScaleMode.StretchToFill };
img.style.width = 320; img.style.height = 180;   // points: 960 x 540 screen pixels at scale 3
windowPanel.Add(img);
```

The window is then one more small panel on the table, it can be pinned to her mark like any other, and the camera is
switched off when the window is shut. HDRP into a small target still pays its fixed per-camera costs; measure it.

## 9. What is slow

Ranked for this game as it stands. The first four are in `MapView.Draw()` today.

1. **Layout every frame.** Any write to `left`, `top`, `width`, `height`, `margin`, `padding`, `font-size`, `display`
   or the text of an auto-sized label makes the layout engine run, then new geometry is built for what moved.
   `Draw()` writes them for every tile, port, ship, pin and landmark each frame. Cure: section 6.1 (a sheet moved by
   `translate`; `DynamicTransform` on what moves alone).
2. **Repainting custom geometry each frame.** `courseLine.MarkDirtyRepaint()` runs every frame and re-tessellates the
   whole stroke. Cure: geometry in sheet coordinates, repainted only when the course changes (6.5).
3. **Strings each frame.** `Ports()` builds the note for every port every frame (`$"pays {...}"`, joins), and
   `Labels()` every 30 frames. Setting a label to an equal string is cheap (?), but building the string is garbage,
   and garbage is a hitch every few seconds. Cure: rebuild when the picked good, the ledger or the day changes.
4. **Textures per batch.** A batch can hold `textureSlotCount` textures (8 in `GamePanel.asset`; checked that the
   property is "the maximum number of textures that UI Toolkit can bind to the shader at once"). Each chart tile is
   its own texture; the atlas is one more; each font atlas one more. Nine tiles in view plus UI is two or three
   batches: fine. What hurts is interleaving: tile, label, tile, label. Keep all tiles in one layer and all marks in
   the next, as now.
5. **Big quads and fill.** The chart, the fog and every see-through panel are drawn over one another at full screen
   size; at 4K that is real fill cost on a laptop GPU. Keep to two full-screen layers (picture, fog). The hint
   `UsageHints.LargePixelCoverage` exists in 6000.6 for "a VisualElement that covers a large pixel area on screen"
   (checked); try it on the tile layer and measure, since the page says no more than that.
6. **Masks.** A rectangular `overflow: hidden` is a cheap clip. With rounded corners, or nested, it becomes a stencil
   mask that splits batches (?). The pixel look has square corners, so this mostly goes away; `MaskContainer` is the
   hint if nested masks are ever needed.
7. **USS `filter`.** Blur, tint and the like exist in 6.6 (the panel asset has blur and colour-effect shaders, and
   there is a `DynamicPostProcessing` hint). Each filtered element is drawn to a texture first. A frosted-glass blur
   behind every panel would be the dearest thing in the interface. The glass stays a plain alpha.
8. **Long lists built by hand.** The chronicle and the market rows are `ScrollView`s filled with one element a row.
   Fine to a few hundred rows; past that use `ListView`, which recycles rows (6.6 manual, "Best practices for
   managing elements"). 793 to 1066 will pass a few hundred chronicle entries.
9. **Making and destroying elements.** `Clear()` and rebuild on every refresh (the market does this on each change)
   is the commonest hitch in UI Toolkit. Keep the rows; change their text and classes.
10. **How you hide things** (6.6 manual, same page): `display: none` costs nothing to draw but changes layout around
    it; `visibility: hidden` keeps layout and frees the meshes; `opacity: 0` and a far `translate` still send
    vertices; `RemoveFromHierarchy()` frees all and is dear to bring back. For marks on the sheet (absolute, no
    neighbours): `display: none`. For a panel that reopens often: `display: none`. For a fade: `opacity`.
11. **Transitions on layout properties.** A USS `transition` on `width`, `height`, `left`, `top` is layout every frame
    while it runs. Animate `translate`, `opacity`, tint. (In a pixel look, most motion should be in steps anyway.)
12. **Several panels.** Every `PanelSettings` asset in use is a separate panel: its own atlas, batches and event pass.
    Five documents on one asset (as now) are one panel. Keep it so.
13. **The camera that draws nothing.** In `L_Chart` an HDRP camera still runs each frame under the chart. Its culling
    mask is empty, but HDRP has fixed costs per camera (sky, volumes, post). Measure with the Profiler; if it shows,
    strip its frame settings to the bone, or draw the UI with no camera at all (?: whether an overlay panel draws
    with every camera disabled in HDRP 17.6 needs a test).
14. **Tile memory.** `tilePixels` defaults to 4096 in `MapArt.cs`: a 4096 x 4096 RGBA32 tile is 64 MB uncompressed in
    video memory. Nine in view is 576 MB. Unload what leaves the view (`artImages` already tracks use), and consider
    a palette trick later (an R8 index texture plus a 256 x 1 palette needs a custom UI shader; not now).

How to see it: Window, Analysis, Profiler (the UI Toolkit markers for layout, style and render), the Frame Debugger
for batches and which texture broke one, and the UI Toolkit Debugger for the tree. Measure in a build: the editor
adds its own UI cost.

## 10. The plan: a few small panels anchored to things on the chart

The concept (research_indie_and_concept.md 4.1): the chart fills the screen; three things lie on it; "a port, a sail
or a card opens as a panel beside the chart, never over it". Below, "beside" is taken one step further: beside **the
thing**, with a tail to it, so the eye never leaves the chart.

### 10.1 What is on screen (640 x 360 points)

| Piece | Where | Size (points) | Holds | Replaces |
|---|---|---|---|---|
| Day stick | Top edge, centred, hangs down 14 | about 300 x 14 | Today, the season, notches ahead (the debt's day, a fair, the sea closing); speed as 3 small notches at its end | `map-topbar` date, speed buttons, alerts button |
| Tally | Bottom left corner | about 120 x 44 | Silver, the debt, days of stores, the hold as a 4-row grid (drag target) | Top bar figures, the outliner's hold |
| Chest | Bottom right corner | a 28 x 22 icon; opens to 220 x 200 | Texts, riddles, rune sticks of prices, notes, crew | `ChestPanel`, the ledger, the chronicle list |
| The pinned panel | Beside a thing, with a tail | up to 200 x 140 | One of: a port, a sail, a card, the market, a man | `map-card`, `map-ship-card`, port card, `MarketPanel` box, `EncounterPanel`, `TextPanel` |
| Marks on the sheet | On the chart | 5 to 12 | Ports with a name slip, sails, her mark, the dotted course, pins | The same, as sprites |
| Hover card | At the pointer | up to 160 wide | One or two lines | `HoverCards`, kept |

Nothing else. The foot bar goes: the scale bar becomes a small ruler on the chart's own corner (part of the picture
layer), Save and Load go to the Esc menu, the tools (dividers, pins) go into the chest as things you pick up.

### 10.2 The pinned panel's five faces

All five are the same element class, the same frame sprite, the same `Pinned` placer; only the content differs.

| Face | Pinned to | Opens when | Content (8 px font, 25 letters a line) |
|---|---|---|---|
| Port, from afar | The port's dot | Click a port | Name; "3 days by water (2 to 4)"; the month there; the word on each good aboard, one line each with its age mark; [Set course] |
| Port, alongside | The port's dot (she lies there) | She moors | The port's menu as 4 to 6 lines; "Market" turns the panel to the market face in place |
| Market | The port's dot | From the port face | Rows: good, price, the word mark; the hold grid of the tally is the other side of the trade: **drag** between them |
| Sail | The other ship's mark | Click a sail | Who, whence, what she is said to carry, how old the sight is |
| Card | Her own mark | The encounter engine | A fact; two or three choices; the odds as a word. Never mid-screen: beside her, where it is happening |

The market is the test of the whole plan: 1260 px wide now, 200 points then. It works if a row is "Wine 14 d  (said,
last week)" and the detail is a hover card; it does not work as a table of six columns. If it cannot be made to fit,
the market keeps a wider face (300 x 200) pinned to the port and the chart re-centres to leave it room, as
`centreFrac` already does.

### 10.3 Code shape

Keep the game's rules untouched (`Trade`, `Course`, `Known`, `Exploration`, `PanelClock`, the encounter engine). Only
the view changes.

```
Code/Game/UI/Pixel/
  PixelPanelScale.cs    section 2.3
  PixelFont.cs          section 4.2
  Sheet.cs              the moving sheet, origin shifts, PlaceOnSheet, zoom stops (from MapView.Draw, Tiles, ArtTiles, PlaceCut)
  SheetMarks.cs         ports, sails, her mark, pins: pooled sprites, placed on change
  CourseDots.cs         section 6.5
  Pinned.cs             section 6.6
  PinnedPanel.cs        one frame, a title, a body slot, a tail; Show(face, anchor), Close()
  Faces/PortFace.cs, MarketFace.cs, SailFace.cs, CardFace.cs
  DayStick.cs, Tally.cs, ChestIcon.cs
  ItemDrag.cs           section 7
Game/UI/Pixel/
  pixel.uss             one sheet: .px, .px-panel, .px-button, marks, strips; every number whole
  panel.png, button*.png, tail.png, marks.png, hulls16.png, Fonts/Pixel8.asset
```

`MapView` keeps its public face (`Current`, `IsOpen`, `ExploreTown`, `OpenAtSea`, `ChestOpen`) so `PanelClock`,
`ChartBuilder` and the tests keep working; its inside is handed to `Sheet` and `PinnedPanel` step by step. The old
3D-rise path (`Begin`, `LateUpdate`) is not in the chart scene and can stay as it is until the window is built.

### 10.4 Steps, each one playable and testable

| Step | What | Done when |
|---|---|---|
| 1 | `PixelPanelScale`; the panel asset to Constant Pixel Size. Divide every px in the four USS files by 3 and round | The game looks the same at 1080p (x3), and is sharp at 1440p (x4) and 720p (x2). `ChartGameTests` pass |
| 2 | The sheet: tiles as children at whole places, one rounded `translate`; zoom stops; fog and `MapRenderer` textures to Point | No seams while panning; Profiler shows no layout in `Draw()` while only panning |
| 3 | Marks on the sheet: port dot and slip, 16-frame hulls, dotted course; notes rebuilt on change | No `rotate`, no `Painter2D`, no per-frame strings |
| 4 | Font decision (4.5), the font asset, the frame and button sprites, `pixel.uss` | One panel (the port card) fully in the new look |
| 5 | `PinnedPanel` with the port face (afar), replacing the port card and the `map-card` | Click a port: the panel is beside it, flips at the edge, follows a drag of the chart |
| 6 | Sail face and card face (this is also "cards at sea in chart mode" from `chart_game.md`) | A card opens beside her mark; the clock holds |
| 7 | Day stick and tally as strips; the top bar and foot go | The three things of concept 4.1 are the whole fixed interface |
| 8 | Market face and the hold grid with `ItemDrag` | A cask is bought by dragging it into the hold |
| 9 | Chest as an icon that opens a panel; chronicle to `ListView` | The chest's drag uses `ItemDrag`; one drag code in the game |
| 10 | The 3D window as a framed RenderTexture panel (8.3) | Later, with the pixel demo |

Steps 1 to 3 change no design and remove the per-frame layout: they are worth doing even if the look changes again.
Steps 4 onward need two answers from you.

### 10.5 Questions for you

1. **The font** (4.5): all pixel, or pixel chrome with Gentium for reading?
2. **The pan**: the chart moving in whole fat pixels (strict), or gliding by single screen pixels under fat-pixel art?
3. **The market**: a narrow pinned panel with the detail on hover, or a wider one that the chart makes room for?
4. **The glass**: keep the see-through dark panels (they work as a flat sprite with alpha), or go to solid
   pixel-drawn frames in the Byzantium palette?
5. **Base size**: 640 x 360 (advised), or 480 x 270 for bigger pixels and less room?

## 11. Pitfalls, in one list

| Pitfall | Sign | Cure |
|---|---|---|
| Scale With Screen Size | Shimmer, uneven pixel widths | Constant Pixel Size, whole `scale` (2) |
| Game view zoom not 1x | Everything soft in the editor only | Set it to 1x, fixed resolution |
| Fractional USS numbers, percentages | One border thicker than its twin | Whole numbers; floor the root size (2.4) |
| Bilinear texture | Soft sprite | Point on the texture; the importer script (3) |
| Mipmaps or compression on | Soft or blotchy when small | Off, None (3) |
| Art drawn smaller than it is | Missing rows, crawling | Coarser pyramid level, whole enlargement (6.2) |
| Sprite at 16 PPU | 9-slice borders vanish | 100 PPU to match the panel (5.3) |
| Stretched 9-slice edge | Smeared dither | `-unity-slice-type: tiled` (5.1) |
| SDF font asset for a pixel font | Soft, rounded letters | RASTER, design size, Point atlas (4.2) |
| `font-size` not a multiple of the sampling size | Ragged letters | 8, 16, 24 for an 8 px font |
| Centred pixel text | Letters uneven by half a pixel | Left-align, whole paddings (4.3) |
| `<b>`, `<i>`, outline, blurred shadow on bitmap text | Blur or nothing | A second font, a slip behind (4.3) |
| `rotate`, `border-radius`, `Painter2D` | Smooth edges among hard ones | Sprite frames, square corners, quads (6.4, 6.5) |
| `left/top` each frame | Layout in the Profiler every frame; seams | `translate` on a sheet, rounded (6.1) |
| `usageHints` set after adding | An exception | Set it before `Add` (checked) |
| `resolvedStyle` before first layout | NaN; a panel flashing at 0, 0 | `GeometryChangedEvent` (6.6) |
| No `PointerCaptureOutEvent` | A ghost stuck on screen | One clean-up path there (7) |
| Screen pixels mixed with panel points | Drops land k times too far | `e.position`, or `ScreenToPanel` with the y flip (7) |
| Render target without the mapping function | Clicks land in the wrong place | `SetScreenToPanelSpaceFunction` (8.1) |
| A quad far larger than the screen | An assertion each frame (seen with the fog) | Cut to the view with `uv` (6.2) |
| `PanelSettings.scale` set in Play mode | The asset shows as changed | Accept, or use one runtime copy for all documents (2.4) |

## Sources

Unity 6.6 (6000.6) manual and script reference, read 6 October 2026:

- Panel Settings properties reference: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-Runtime-Panel-Settings.html
- `PanelSettings`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.PanelSettings.html
- `PanelSettings.SetScreenToPanelSpaceFunction` (with Unity's sample): https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.PanelSettings.SetScreenToPanelSpaceFunction.html
- USS properties reference: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-USS-Properties-Reference.html
- USS common properties (slices, the pixels-per-unit note, fonts, text): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-USS-SupportedProperties.html
- Font assets (render modes; "Pixel art: any bitmap mode"): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-font-asset.html
- Font Asset properties: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-font-asset-properties.html
- Font Asset Creator properties: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-font-creator-properties.html
- Panel Text Settings: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-text-setting-asset.html
- `UsageHints`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.UsageHints.html
- `DynamicAtlasFilters`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.DynamicAtlasFilters.html
- Control textures of the dynamic atlas: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-control-textures-of-the-dynamic-atlas.html
- `RuntimePanelUtils`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.RuntimePanelUtils.html
- `VisualElement` (usageHints, scaledPixelsPerPoint): https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.VisualElement.html
- Runtime UI performance (index): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-performance-consideration-runtime.html
- Best practices for managing elements (hiding, pooling, ListView): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-best-practices-for-managing-elements.html
- Create a drag-and-drop UI: https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-create-drag-and-drop-ui.html
- Generate 2D visual content (Painter2D, `Allocate`): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-generate-2d-visual-content.html
- FAQ for events and input (screen to panel, the y flip, `panel.Pick`): https://docs.unity3d.com/6000.6/Documentation/Manual/UIE-faq-event-and-input-system.html
- World Space UI: https://docs.unity3d.com/6000.6/Documentation/Manual/ui-systems/world-space-ui.html

Read in the editor installed on this machine (`C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Data`):

- `Managed\UnityEngine\UnityEngine.UIElementsModule.xml`: `PanelScaleMode`, `PanelScreenMatchMode`, `SliceType`
  (the Full Rect note), `DynamicAtlasFilters`, `DynamicAtlasSettings`, `AlignmentUtils`, `RuntimePanelUtils`, `IPanel.Pick`.
- `Managed\UnityEngine\UnityEngine.UIElementsModule.dll`: the names `m_PointPage`, `m_BilinearPage`, `RoundToPixelGrid`.
- `Managed\UnityEngine\UnityEngine.TextCoreFontEngineModule.xml`: the `GlyphRenderMode` values.
- `Resources\CGIncludes\Internal\UnityUIE.cginc`: one sampler per texture slot; `uie_std_frag_bitmap_text`; 1, 2, 4
  or 8 texture slots.

Not checked, and marked (?) in the text: the 6000.6 pages for usage hints and for USS filter (both returned "not
found" at the addresses I tried); the `FontAsset.CreateFontAsset` overload (present in the DLL, no doc entry); how
layout rounding and transforms treat the pixel grid in 6000.6; whether the font atlas sub-asset's filter can be set in
the Inspector; `text-shadow` on bitmap text; the licences and glyph tables of the candidate pixel fonts; the y
direction of the position given to the screen-to-panel function; an overlay panel with no camera in HDRP 17.6.
No web search could be made in this run.
