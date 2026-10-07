# Bake/world: the open world

Scripts for the one open world of `Design/open_world_isometric.md` and `Design/DECISIONS.md` (4 Oct 2026, afternoon).
All run with QGIS's Python (`Bake\run.bat`), which has GDAL/OGR, numpy, scipy, shapely and Pillow.

- `make_world_map.py`: the whole world at 1:80 as game kilometres for the world-map page (`world_map.json`).
- `make_tiles.py`: the water tiles the game streams (this section).
- `preview_tiles.py`: a PNG overview of a tile bake, for checking by eye.

## The water tiles: make_tiles.py and preview_tiles.py (4 Oct 2026)

For a lat/lon box and a scale, `make_tiles.py` cuts the world into square tiles of game metres. Each 2 m cell gets a
signed distance to the waterline and a kind. Lengths shrink by the scale. River widths stay true size, capped at 300 m.
The scale is 1:40 (the user, 4 Oct). 1:80 bakes as well and is kept for comparison.

### Run

```
cd G:\kapman_unity\Kapman2\Bake
run.bat world\make_tiles.py --scale 40                 (the North Sea core, about 30 s)
run.bat world\make_tiles.py --scale 80                 (about 15 s)
run.bat world\make_tiles.py --scale 40 --box 3.0 50.5 5.0 52.0 --name test40     (another box, its own folder)
run.bat world\preview_tiles.py --scale 40                         -> data\world\40\preview.png
run.bat world\preview_tiles.py --scale 40 --px 2 --crop 16600 -16200 18800 -14600 --rivers --ports
```

`--crop` takes game metres. `--rivers` draws the river centre lines and `--ports` marks the ports of `ports_793.json`.
`--pool min` lets water win inside a pixel, so thin channels show at overview scale. When passing `--out` through
`cmd`, use forward slashes (`G:/...`): backslashes are lost on the way.

### What comes out

`G:\kapman_unity\data\world\<scale>\` (the scale's number, e.g. `40`; `--name` overrides it):

| File | What |
|---|---|
| `index.json` | The bake's settings, the sources and credit lines, timings, a summary of each river, and **every tile** of the box: `{x, y, files, counts[5], sd[min,max], rivers[], fill?}`. A tile with `files: false` is uniform and has no folder; `fill` gives its `sd` and `kind` (open sea: `+cap`, kind 0; deep inland: `-cap`, kind 4). |
| `tiles\<x>_<y>\water_sd.f32` | float32 little-endian, 512 x 512, row-major. **Row 0 is the south edge and column 0 the west edge.** Cell (r, c) is centred at `origin + ((c + 0.5), (r + 0.5)) x 2 m`, where `origin = (x, y) x tile size`. The value is the signed distance in game metres to the waterline: positive in water, clamped to +/- `cap` (256 m). |
| `tiles\<x>_<y>\kind.u8` | uint8, same layout: 0 deep sea, 1 shallow sea, 2 river (all inland water), 3 band (land within 60 m of water), 4 inland (under fog). Kinds 0-2 are exactly the cells with `sd >= 0`, and kind 3 exactly those with `-60 <= sd < 0`. |
| `tiles\<x>_<y>\meta.json` | The tile's origin, size, cell, cap, band, counts per kind, sd range, the rivers that touch it, and its corners in lon/lat. |
| `rivers.json` | Each river's centre line in game metres, upstream first, every half width or so. `w` is the width at each point. The file also holds the width sources, the tightest bend and how far each waypoint lies from the line. For the river current. |
| `bake_log.txt` | The run's log: the source counts, each river's length, widths, easing and trim, the straight-line fallbacks, and the timings. |
| `preview*.png` | Overviews and check crops from `preview_tiles.py`. |

Game metres are the Lambert conformal projection of `make_world_map.py`
(`+proj=lcc +lat_1=50 +lat_2=68 +lat_0=58 +lon_0=-5 +datum=WGS84`) divided by the scale: x east, y north. Tile (x, y)
covers `[x*1024, (x+1)*1024) x [y*1024, (y+1)*1024)`. The grid is global, so other boxes baked at the same scale line up
with these tiles. The North Sea core lies far from the projection's origin (x 7-21 km, y -19 to -9 km at 1:40), so the
game needs its floating origin.

### How it is built

1. **The coast.**
   - Outside the Netherlands: Natural Earth 10 m `land` + `minor_islands`, projected and simplified at 1 game metre.
     Corners are rounded by two Chaikin passes after cutting the segments to at most 20 m, so a 300 m Natural Earth
     segment does not read as a straight ruler line at 1:40.
   - Inside the Vos c. 800 map's coverage: `buitenwater` and `getijdenzone` are sea, `binnenwater` is inland water, and
     every other class is land. `buitenland` means "not covered", and Natural Earth decides there. The seams with
     Natural Earth, on the Flemish coast and at the Ems, close cleanly.
   - Two corrections to the Vos layer:
     - The `binnenwater` river network (one 240 km² piece about 260 m wide on average: the Rhine, Waal, Lek, IJssel and
       Maas c. 800) and other narrow channels are left out (`--vos-channel 300`). Shrunk 40 times they became 5-10 m
       slivers beside the true-width rivers that replace them. The eight lakes stay.
     - Sea pieces under 2 km² lying more than 2 km from the big sea are dropped as stray: three tidal-flat polygons in
       South Limburg, 0.3-0.6 km².
2. **The rivers** (the `RIVERS` table at the top of the script: waypoints, upstream first).
   - Each leg between waypoints is the shortest path through HydroRIVERS (`hydro`) or through the Vos 800 channel lines
     (`vos`), or a straight line (`line`).
   - A leg that finds no path, or a path more than 2.5 times the straight distance, falls back to a straight line and
     is logged. HydroRIVERS is a tree, so a distributary's head is not tied to its parent; the Vos channel lines have
     gaps.
   - The course is projected and shrunk. Widths come from the water records at their waypoints, are interpolated in
     between and capped at 300 m.
   - Bends are eased (Laplacian smoothing where the radius is under 3 widths) while each waypoint must stay within a
     quarter width of the line, so the ports stay on the water. Where the real course turns too hard for that (the
     Scheldt at Antwerp, the Nederrijn at Arnhem) the bend stays tighter than three widths, and the log says how much.
   - Rivers end where, after a stretch on land, their whole width first lies in sea or lake water, plus one width of
     overlap. A river therefore keeps its width through an estuary that is narrower than it, and opens into it where
     the estuary is wider.
   - Each river is then a ribbon polygon (offset centre line plus round end caps). Branches start on their parent's
     waypoint, so junctions overlap.
3. **The distance field.** Per tile, the water mask is rasterized at 1 m (supersample 2) with a margin of 260 m.
   Euclidean distance transforms run both ways, and the result is averaged to 2 m and clamped. The tiles are seamless:
   neighbouring cells differ by at most 2 m inside tiles and across tile edges (checked on both bakes).
4. **Deep or shallow.** Sea is shallow where the EMODnet DTM 2024 depth is under 10 m (`--shallow`), where the Vos map
   has tidal flats, or where Vos c. 800 water is land today (no EMODnet value, so silted up since).

### The rivers of the North Sea core

| id | Course | Width (m, game = real) |
|---|---|---|
| rhine | HydroRIVERS, Colonia - Diusburg - Emmerich - Pannerdensche Kop | 300 (records rhine_colonia 350, rhine_lower_diusburg 350, capped) |
| nederrijn | HydroRIVERS, Pannerdensche Kop - IJsselkop - Arnhem - Dorestad | 300 (lek_dorestad 350, capped) |
| kromme_rijn | Vos 800 lines, Dorestad - Traiectum | 100 to 70 (kromme_rijn_dorestad, old_rhine_traiectum) |
| oude_rijn | Vos 800 lines, Traiectum - Woerden - Bodegraven - Alphen; straight to Leiden and Katwijk | 70 (old_rhine_traiectum) |
| vecht | Vos 800 lines, Traiectum - Muiden; straight into Almere | 70 (**estimate**: no record; the Old Rhine's) |
| lek | HydroRIVERS, Dorestad - Schoonhoven - Kinderdijk | 300 (lek_dorestad 350, capped) |
| waal | Straight from the Kop to Noviomagus (HydroRIVERS detours), then HydroRIVERS to Dordrecht; straight to Kinderdijk | 300 (waal_noviomagus 400, capped) |
| maas | HydroRIVERS, Traiectum ad Mosam - Roermond - Venlo - Grave - Heusden; straight to Woudrichem | 130 (meuse_traiectum_mosae) |
| maasmond | HydroRIVERS, Kinderdijk - Rotterdam - Vlaardingen; straight past Brielle to the sea | 300 (maas_mouth_witla 800, capped) |
| ijssel | Straight from the IJsselkop to Doesburg, then HydroRIVERS by Zutphen and Daventre to Kampen; straight into Almere | 120 (ijssel_daventre) |
| thames | HydroRIVERS, Windsor - Kingston - Lundenwic - Greenwich - Gravesend; straight down the estuary | 60 (**estimate**, the non-tidal Thames) to 300 (thames_lundenwic 350, capped) |
| scheldt | HydroRIVERS, Tornacum - Oudenaarde - Ganda - Dendermonde - Andoverpis; straight by Bath to the mouth | 30 to 50 to 300 (scheldt_upper_tornacum_valentianas, scheldt_ganda, scheldt_estuary_andoverpis) |
| wantsum | Straight lines, Reculver - Sarre - Richborough - Pegwell Bay (not in the brief; open_world_isometric.md's c. 800 coast) | 100 (**estimate**: no record; 300 would swallow Thanet at 1:40) |
| seine | HydroRIVERS, Paris - Rouen; straight to the bay (outside the default box, so not baked yet) | 150 (**estimate**: no water record) |

### Arguments (make_tiles.py)

| Argument | Default | Meaning |
|---|---|---|
| `--box LON0 LAT0 LON1 LAT1` | -0.6 51.0 7.3 53.7 | The region: the Rhine delta, Holland and the Frisian coast to the Ems, the Thames estuary. Every tile touching the projected box is baked. |
| `--scale` | 40 | Real lengths per game length. |
| `--sea-scale` | = scale | A harder shortening for the open sea. **Designed for, not built** (see below): any other value stops the bake. |
| `--tile`, `--cell`, `--supersample` | 1024, 2, 2 | Tile size and cell size (game m); sub-cells per side for the distance field. |
| `--cap`, `--band` | 256, 60 | The distance clamp; the waterfront band (game m). |
| `--shallow` | 10 | Sea shallower than this (real m) is kind 1. |
| `--max-width`, `--bend` | 300, 3 | River width cap; the smallest bend radius in widths. |
| `--simplify`, `--smooth`, `--segment` | 1, 2, 20 | Coast simplification (game m), Chaikin passes on the Natural Earth coast, the longest segment before smoothing. |
| `--vos-channel` | 300 | Vos inland water narrower than this on average (real m) is a river channel the true-width rivers replace; 0 keeps it all. |
| `--name`, `--tiles X0 Y0 X1 Y1` | | Output folder name; bake only these tile indices (for tests). |

### A second scale for the open sea (designed, not built)

Every coordinate passes through `Frame.real_to_game`, which today divides by the one scale. A sea scale turns it into a
warp:
- Near land and rivers it shrinks by the river scale.
- Far out at sea it shrinks by the sea scale.
- Between them it blends by distance from the coast. A coarse real-world distance-to-land raster, smoothed, drives a
  blend factor; game positions are the integral of the local scale along the projected axes, or a precomputed
  displacement grid.
- The ribbons, ports and tiles are all built after that function, so only it and its inverse (`game_to_real`, used for
  the bathymetry and the lon/lat corners) need to change.

Open question before building it: do coast-hugging stretches (the Holland coast, the Wadden) count as "near land"?

### Results, 4 Oct 2026

| Scale | Tiles | With files | Empty (sea / inland) | Size | Time |
|---|---|---|---|---|---|
| 1:40 | 116 (14 x 10 minus the box's corners) | 84 | 32 (20 / 12) | 110 MB | about 26-30 s |
| 1:80 | 36 | 31 | 5 (3 / 2) | 41 MB | about 14 s |

At 1:40 the share of cells by kind is: deep 38%, shallow 8%, river 4%, band 6%, inland 45%.

### Known limits and open questions

- **True widths on shrunk lengths.** At 1:40 the Lek and the Waal (both 300 m) run only about 250-350 m apart, centre
  to centre, so they nearly touch. The Rhine junctions (Pannerden, Arnhem, Dorestad) become one broad water. At 1:80 the
  whole delta from Colonia to the Maas mouth is one water body, and the Thames reads as a lake. This follows from the
  rule; a narrower cap for the delta branches (e.g. 150-200 m) would open them out.
- **Sea channels shrink with the sea.** The Vos tidal channels (Zeeland, the Vlie, the IJ) and Natural Earth's narrow
  estuaries (the Ems towards Leer, the Orwell, the Swale) are only as wide as the scale makes them: a 1 km channel is
  25 m at 1:40. The river ribbons carry the navigable width through the Thames, Maas and Scheldt mouths, but the other
  channels are not widened to true size.
- **Straight legs** (logged in `bake_log.txt`): Pannerdensche Kop to Noviomagus, Dordrecht to Kinderdijk, the IJsselkop
  to Doesburg, Alphen to Leiden to Katwijk, Gravesend down the estuary, Antwerp to the mouth. At 1:40 these are 200-700 m
  of gently eased channel.
- **River ends inside the box's outer tiles.** The Rhine starts at Colonia, the Maas at Traiectum ad Mosam and the
  Thames at Windsor. The next region's bake continues them.
- **c. 800 outside the Netherlands** is today's coast: the Flemish coast, the Wantsum (added as a channel), East Anglia
  and the Thames marshes are as Natural Earth has them.
- **Not modelled:** Natural Earth lakes outside the Vos map, other rivers (the Ems, the Medway, the Stour to Fordwic, the
  Orwell to Gipeswic, the Zwin), and depth inside rivers. The shallow/deep split is modern EMODnet depth.

### Sources, licences and credit lines

All the data used are commercially shippable. The credit lines below are also written into each `index.json`
(`sources`).

| Source | Licence | Credit line |
|---|---|---|
| Natural Earth 10 m physical: land, minor islands (`data\raw\vectors\naturalearth\10m_physical.zip`) | Public domain | Made with Natural Earth. Free vector and raster map data @ naturalearthdata.com. |
| Paleogeografische kaarten van Nederland, 800 n.Chr., Vos 2021 release (`paleogeografischekrt2021_GIS_3van3` layer `paleogeografie800nc`; `_GIS_1van3` layer `paleogeografie_waterlopen`, 800nc) | CC BY 4.0 (data.overheid.nl dataset 62601; recorded in `Research/region_textures.md`). **The zips carry no licence file.** | Paleogeografische kaarten van Nederland, P.C. Vos & S. de Vries (Deltares/TNO), Rijksdienst voor het Cultureel Erfgoed, CC BY 4.0 |
| HydroRIVERS v1.0, Europe (`data\raw\hydro\hydrorivers`) | HydroSHEDS licence: "free for non-commercial and commercial use" (TechDoc v1.0 in the zip); HydroSHEDS is CC BY 4.0 | Lehner, B., Grill G. (2013): Global river hydrography and network routing: baseline data and new approaches to study the world's large river systems. Hydrological Processes, 27(15): 2171-2186. Data available at www.hydrosheds.org. |
| EMODnet Digital Bathymetry DTM 2024, tiles D4, D5, E4, E5 (`data\raw\bathy\emodnet`) | CC BY 4.0 | EMODnet Bathymetry Consortium (2024): EMODnet Digital Bathymetry (DTM 2024), emodnet.ec.europa.eu, CC BY 4.0 |
| Kapman water records (`Assets\Kapman\Data\Water\water_793.json`) | Own research | River widths; each record carries its own sources. |

GEBCO and GRWL are not used by this bake. The first file of the Vos download, `vos_800AD_shp.zip`, is a broken export:
its 2,770 features are polygon boundaries saved as line strings, and the class field sits on features that have no
geometry. The bake reads the same map from the 2021 GDB release beside it instead.
