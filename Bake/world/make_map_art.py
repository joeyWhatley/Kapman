# -*- coding: utf-8 -*-
r"""
MAKE MAP ART (the user, 4 Oct 2026 late: "a placeholder 2D map with period accurate geography, full sized, high res,
something that can be replaced with my own hand drawn one"; picker: pixel-flat in the Byzantium palette, the sailing
world). Draws the chart's picture as plain PNG tiles the game shows under its marks (Game/Map/MapArt.cs) and the user
redraws in Aseprite, tile by tile, at the same size.

  run.bat world\make_map_art.py                          -> Kapman2/MapArt/world: t_<row>_<col>.png, overview.png,
                                                            overview_guide.png, map_art.json
  run.bat world\make_map_art.py --out <folder>           -> the same files in another folder (bake there, look, then
                                                            swap it for MapArt/world: the game never sees half a map)
  run.bat world\make_map_art.py --tiles 3,1 3,2 4,2      -> only these tiles (row,col); the overviews are put together
                                                            from them and from the tiles already in the folder
  run.bat world\make_map_art.py --tiles none             -> only the overviews and map_art.json, from the tiles there
  ... --cache <folder>                                   -> keeps today's land cover per tile there (for trying things)

The picture is cut from the world bake the chart already reads (G:\kapman_unity\data\baked\world, level 2: 120 m a
cell, in the game's world frame: Lambert conformal, 45N and 65N, centred 56N 5E), one art pixel a cell:
  6 x 6 tiles of 4096 px (491.52 km a tile), from Ireland to Kiev and Novgorod, from Bordeaux to the Lofoten.

ONE MAP, ONE WAY OF DRAWING EACH KIND OF GROUND (the user, the same night: "why is frisia weirdly hyper detailed but
everywhere else isnt", and after the first re-bake: "bro frisia still looks out of place we gotta fix it"). The
Netherlands comes from the Vos reconstruction of c. 800 (the bake's kinds 2 to 18), everywhere else from heights and
today's land cover. Both are first turned into the same few kinds of ground, and each kind is drawn one way on every
coast, in colours of the palette broken up by the same patch noise:
  - woodland and clearings (forest, meadow): all dry land. How much was cleared about AD 800 comes from KK10 (Kaplan
    and Krumhardt: the share of each 9 km cell under farming and grazing, band for 1150 BP), laid in patches on the
    ground that is farmed or grazed today (ESA WorldCover 2021). The Dutch sand country, river levees, push moraines,
    loess and the old dune country are dry land like any other: the same rule draws them;
  - peat, fen and marsh (grass, with patches of forest for the carr and flecks of timber and reed light): the Dutch
    peat bogs and reed marsh, today's wetlands everywhere, and the low basins behind every coast (below). Mid tones:
    the moor's own green, so peat is not a hole in the map;
  - salt marsh (meadow, reed light): the Dutch salt marsh, and elsewhere the low land within reach of tidal water;
  - tidal flats (pale stone, shallows, in patches): the bake's own, on every tidal coast;
  - beaches, bars and open dune sand (pale sand): bare ground by the water; of the 800 map's dunes and beach ridges
    the beach and a quarter in patches, the rest being wooded and lived on about 800 (the Old Dunes): dry land;
  - heath and moor, bare rock and snow as before (today's open uplands and the far north; by WorldCover and height).
THE LOW COASTS BEYOND THE NETHERLANDS, BY HEIGHT: land lower than the spring tide reaches (mean high water of spring
tides, from a table of round figures for about 150 harbours of these seas, plus 0.8 m) is drawn as it was before the
dykes: salt marsh within about 10 km of tidal water, peat and fen behind it. That carries Frisia on through East
Frisia, the Weser and Elbe marshes, Dithmarschen and the Danish Wadden coast, the Flemish plain, the Fens, the Humber
levels, Romney Marsh, the Somerset levels and the Thames marshes. On coasts without a tide (the Baltic) the low land
is fen.
NO EDGE TO THE DUTCH DATA, AND NO MORE DETAIL INSIDE IT THAN OUTSIDE:
  - the 800 map's polygon edges, and the edge of its data, are roughened by a few pixels of noise;
  - where its peat meets the edge of the data it runs on in thinning patches for some 18 km (the Bourtange bog did
    cross that border);
  - its rivers are drawn as rivers are elsewhere: water with the coast's one-pixel rims where a river is five cells
    wide or runs along one of the rivers the game sails (the Rhine's arms, the Kromme Rijn, the Vecht, the IJssel, the
    Maas), a one-pixel line for the middling streams, and the brooks one cell wide and the least ponds left out.
Beyond the 800 map the big rivers and the lakes are cut in as water with the same rims; small rivers are a line.
It is a model, not a survey: KK10 is itself a model, the tide table is round figures, and the great raised bogs of the
German and Danish sand country are not in it (no data). A placeholder to paint over.
"""
import argparse, io, json, os, struct, sys, zlib, glob
import numpy as np
from osgeo import gdal, osr
from PIL import Image, ImageDraw
from scipy import ndimage

gdal.UseExceptions()

BAKE = r"G:\kapman_unity\data\baked\world"
SCALE_MAPS = r"G:\kapman_unity\data\scale_maps"
KK10 = r"G:\kapman_unity\data\raw\landcover\kk10\KK10.nc"
KK10_YEAR_BP = 1150                        # AD 800
WORLDCOVER = r"G:\kapman_unity\data\raw\landcover\worldcover"
ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
OUT = os.path.join(ROOT, "MapArt", "world")
LEVEL, CELL, CELLS = 2, 120.0, 256
BAKE_WEST, BAKE_NORTH = -7864320.0, 3932160.0
COL0, ROW0 = 0, 0                          # the window's first bake tile (level 2): the whole world bake (6 Oct 2026:
                                           # "bake the whole world"; it was 220, 78 and 6 x 6 tiles, Ireland to Kiev)
TILES_X, TILES_Y = 30, 18                  # art tiles across and down: Vinland to the Caspian, the White Sea to the Sahara
TILES = TILES_X
PER = 16                                   # bake tiles a side in one art tile
PX = PER * CELLS                           # 4096
WEST = BAKE_WEST + COL0 * CELLS * CELL
NORTH = BAKE_NORTH - ROW0 * CELLS * CELL
SPAN = TILES_X * PX * CELL                 # metres across
SPAN_Y = TILES_Y * PX * CELL               # metres down
MB = CELLS                                 # the wide margin round a tile (one bake tile, 30.7 km): distances, smoothing
MW = 48                                    # the margin the land cover keeps (the clearings' 9 km mean), so tiles join

SCALE_PROJ = "+proj=lcc +lat_1=50 +lat_2=68 +lat_0=58 +lon_0=-5 +datum=WGS84 +units=m +no_defs"
WORLD_PROJ = "+proj=lcc +lat_0=56 +lon_0=5 +lat_1=45 +lat_2=65 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs"

PALETTE = {
    "night": "#0b0810", "dark purple": "#2e1233", "plum": "#4a1c4a", "byzantium": "#702963", "mauve": "#9a4a8a", "lilac": "#c77fb0",
    "abyss": "#10131f", "deep sea": "#1b2340", "sea": "#27385e", "shoal": "#3c5a80", "shallows": "#5f8aa6", "wave light": "#9ec4cc", "foam": "#e8eef0",
    "wood shade": "#1f2a1e", "forest": "#2f4a2a", "grass": "#4d7036", "meadow": "#7f9a4a", "reed light": "#b6c26a",
    "peat": "#2b1d16", "wet earth": "#4b3324", "timber": "#6e4b31", "dry earth": "#9b6e46", "sand": "#c49a6a", "pale sand": "#e3cb9a",
    "basalt": "#26212b", "slate": "#4a4350", "stone": "#7a7281", "pale stone": "#aaa4ae",
    "blood": "#8f2d2d", "rust": "#c9553a", "gold": "#d9a441", "pale gold": "#f0d27a",
    "dusk rose": "#d08c74", "dawn peach": "#e8b48a", "morning gold": "#f2d9a8", "day sky": "#8fb3c9",
    "frost": "#b5c7d6", "snow": "#d7e3ec", "cool white": "#e8e2e6", "warm white": "#f6efe0",
}


def rgb(name):
    h = PALETTE[name].lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


# ------------------------------------------------------------------------------------------------ the kinds of ground
# The bake's surface kinds, the Netherlands of c. 800 (Vos):
#   2 tidal flats (the bake has these on every tidal coast)   3 salt marsh and river plain   4 salt-marsh ridges
#   5 peat bog   6 dunes and beach ridges   7 beach plains and dune valleys   8 sandy uplands
#   9 river plains and brook valleys   10 push moraines   11 loess   12 river dunes   14 older ground
#   16 river marsh and reeds   17 sand and mud bars   18 cut-bank earth;   1, 13, 15 water;   0, 20.. beyond the 800 map
WATER_KINDS = (1, 13, 15)                  # sea, inland water, the rivers of 800
# One way of drawing each kind of ground, on every coast: (the base colour, the patches' colour, the patches' share)
WOOD, CLEARING, HEATH = "forest", "meadow", "grass"
FEN = ("grass", "forest", 0.26)            # peat bog, fen and marsh: moor, with carr woodland
FEN_PEAT = ("timber", 0.06)                # bare peat and bog pools in it
FEN_REED = ("reed light", 0.03)            # reed beds in it
GRAIN = 0.14                               # the patches' edges fray by this much of the noise, cell by cell
SALT = ("meadow", "reed light", 0.50)      # salt marsh
FLATS = ("pale stone", "shallows", 0.50)   # tidal flats: mud, and the water over it
SAND = "pale sand"                         # beaches, bars, and the open sand of the dunes
DUNE_SAND = 0.25                           # this share of the 800 map's dunes and beach ridges is open sand; the rest was
                                           # wooded and lived on about 800 (the Old Dunes), so it is dry land like any other
BEACH = 4.0                                # and a beach this wide runs along their seaward side (cells)
RISE = 0.8                                 # m above mean high water springs that a spring storm still wets
TIDAL = 0.5                                # m of high water above mean sea: under this the coast has no tide to speak of
SALT_REACH = 10000.0 / CELL                # how far from tidal water salt marsh runs, beyond the 800 map (cells)
VOS_SALT_REACH = 18000.0 / CELL            # the 800 map's "salt marsh and river plain" is salt marsh this near the sea
FLATS_REACH = 80                           # tidal flats lie within this of the sea (cells); farther in they are land
BLEED = 150.0                              # the 800 map's peat runs on this far past the edge of its data (cells)
WARP = 6.0                                 # the 800 map's polygon edges are roughened by this much (cells)
PIT = -12.0                                # land this far under the sea is an open-cast mine, not a fen
PIT_REACH = 161                            # and so is the ground within 9.6 km of it (cells across)
BROOK_BODY = 400                           # a river's wide stretch is at least this many cells; smaller is a brook's pool
POND = 6                                   # a pond of the 800 map under this many cells is left out (it would be a dot)
RIVER_WIDE = 5                             # a river of the 800 map this many cells wide is water; a narrower one is a line,
GUIDE_NEAR = 41                            # unless it runs along one of the rivers the game sails (within 2.4 km; cells across)
SHORE = 31                                 # bare ground within 1.8 km of water is a beach or a bar (cells across)
# ESA WorldCover classes
WC_TREE, WC_SHRUB, WC_GRASS, WC_CROP, WC_BUILT, WC_BARE, WC_SNOW, WC_WATER, WC_WETLAND, WC_MOSS = 10, 20, 30, 40, 50, 60, 70, 80, 90, 100

# Mean high water of spring tides above mean sea level, in metres: half the mean spring range, in round figures from
# the tide tables as remembered (not checked harbour by harbour; a model's input). lon, lat, metres.
HIGH_WATER = [
    # the German Bight and Jutland
    (6.75, 53.56, 1.1), (7.15, 53.70, 1.2), (7.20, 53.35, 1.5), (8.15, 53.52, 1.9), (8.57, 53.55, 1.9), (8.80, 53.08, 1.5),
    (8.70, 53.87, 1.5), (9.97, 53.55, 1.9), (9.45, 53.80, 1.6), (8.85, 54.13, 1.6), (8.95, 54.32, 1.6), (9.05, 54.47, 1.7),
    (8.70, 54.96, 0.9), (8.40, 54.90, 0.9), (8.44, 55.47, 0.8), (8.30, 56.00, 0.4), (8.20, 56.70, 0.2), (8.60, 57.12, 0.2),
    (10.60, 57.72, 0.15), (9.92, 57.05, 0.15),
    # the Netherlands, Flanders and the French Channel coast
    (4.90, 53.30, 0.9), (5.90, 53.45, 1.1), (4.12, 51.98, 0.9), (3.60, 51.44, 2.0), (4.30, 51.30, 1.5), (2.92, 51.23, 2.1),
    (2.37, 51.05, 2.7), (1.85, 50.97, 3.2), (1.60, 50.73, 4.0), (1.60, 50.20, 4.6), (1.08, 49.93, 4.3), (0.10, 49.48, 3.7),
    (1.08, 49.44, 3.0), (-0.35, 49.30, 3.3), (-1.62, 49.65, 2.7), (-1.60, 48.84, 6.0), (-2.03, 48.64, 5.6), (-3.97, 48.72, 3.9),
    # Brittany to the Basque coast
    (-4.50, 48.38, 3.0), (-3.37, 47.75, 2.2), (-2.20, 47.27, 2.6), (-1.55, 47.20, 2.6), (-1.15, 46.15, 2.7), (-1.03, 45.62, 2.3),
    (-0.55, 44.85, 2.5), (-1.17, 44.66, 1.9), (-1.50, 43.50, 1.9),
    # the Mediterranean and the Adriatic
    (3.70, 43.40, 0.1), (5.35, 43.30, 0.1), (8.90, 44.40, 0.1), (12.30, 45.40, 0.45), (13.75, 45.65, 0.45), (14.40, 45.30, 0.2),
    # England and Wales, south and west
    (1.32, 51.12, 3.0), (0.95, 50.93, 3.3), (0.00, 50.78, 3.1), (-1.10, 50.80, 2.0), (-1.98, 50.71, 0.8), (-2.45, 50.60, 0.9),
    (-3.40, 50.62, 1.9), (-4.13, 50.37, 2.3), (-5.54, 50.10, 2.5), (-4.10, 51.20, 4.2), (-3.00, 51.22, 5.6), (-2.70, 51.50, 6.4),
    (-3.16, 51.45, 5.6), (-3.97, 51.57, 4.3), (-5.05, 51.70, 3.2), (-4.10, 52.40, 2.2), (-4.63, 53.31, 2.5), (-3.00, 53.40, 4.2),
    (-2.90, 54.05, 4.3), (-3.40, 54.87, 4.1),
    # Scotland
    (-5.10, 54.85, 1.7), (-4.90, 55.75, 1.6), (-5.50, 56.40, 1.7), (-6.40, 58.20, 2.1), (-3.10, 58.45, 1.4), (-1.14, 60.15, 0.8),
    (-2.95, 58.98, 1.3), (-4.20, 57.50, 1.9), (-2.08, 57.14, 1.8), (-3.17, 55.99, 2.4),
    # England, east
    (-1.40, 55.00, 2.1), (-1.15, 54.63, 2.3), (-0.60, 54.48, 2.3), (-0.19, 53.63, 3.2), (-0.90, 53.70, 3.3), (0.35, 53.10, 3.2),
    (0.40, 52.75, 3.3), (1.30, 52.93, 2.2), (1.73, 52.60, 1.0), (1.75, 52.47, 1.0), (1.29, 51.95, 1.8), (0.90, 51.75, 2.3),
    (0.74, 51.45, 2.7), (-0.08, 51.50, 3.3), (1.42, 51.33, 2.3),
    # Ireland
    (-6.22, 53.35, 1.8), (-6.14, 52.80, 0.5), (-6.45, 52.33, 0.7), (-7.10, 52.25, 2.0), (-8.30, 51.85, 1.9), (-9.90, 51.65, 1.6),
    (-8.63, 52.66, 2.9), (-9.05, 53.27, 2.3), (-8.50, 54.27, 1.8), (-7.30, 55.00, 1.2), (-5.90, 54.60, 1.6), (-6.30, 54.00, 2.3),
    # Norway, the far north, the islands
    (5.73, 58.97, 0.25), (5.30, 60.40, 0.6), (6.15, 62.47, 0.9), (10.40, 63.43, 1.3), (14.40, 67.28, 1.3), (14.60, 68.20, 1.4),
    (18.95, 69.65, 1.2), (23.70, 70.66, 1.3), (31.10, 70.37, 1.5), (33.05, 68.97, 1.8), (32.40, 67.15, 1.0),
    (8.00, 58.14, 0.15), (10.70, 59.90, 0.2), (11.95, 57.70, 0.15), (-6.77, 62.00, 0.2),
    (-21.90, 64.15, 2.0), (-18.10, 65.68, 0.6), (-14.00, 65.26, 0.7), (-15.20, 64.25, 1.0), (-20.30, 63.40, 1.2), (-23.10, 66.07, 1.0),
    # the Baltic and the Danish straits: no tide to speak of
    (9.45, 54.80, 0.1), (9.57, 54.52, 0.05), (9.95, 54.67, 0.05), (9.90, 54.47, 0.05), (10.15, 54.33, 0.1), (10.70, 53.87, 0.1),
    (11.20, 54.50, 0.1), (12.10, 54.10, 0.1), (9.75, 55.55, 0.2), (10.20, 56.15, 0.2), (10.40, 55.40, 0.15), (12.60, 55.70, 0.15),
    (14.55, 53.43, 0.05), (18.65, 54.35, 0.05), (21.00, 55.70, 0.05), (24.10, 57.00, 0.05), (18.07, 59.33, 0.05), (24.95, 60.17, 0.05),
    (30.30, 59.93, 0.05), (22.00, 65.60, 0.05), (16.40, 56.70, 0.05), (21.50, 61.50, 0.05),
]
_hw = np.asarray(HIGH_WATER, dtype=np.float64)


def high_water(lon, lat):
    """Mean high water springs above mean sea level at points (lon, lat): the table, weighted by nearness."""
    dx = (lon[..., None] - _hw[:, 0]) * np.cos(np.radians(lat[..., None])) * 111.3
    dy = (lat[..., None] - _hw[:, 1]) * 111.2
    w = 1.0 / (dx * dx + dy * dy + 25.0) ** 1.5
    return (w * _hw[:, 2]).sum(axis=-1) / w.sum(axis=-1)


def read_tile(col, row):
    path = os.path.join(BAKE, "L%d" % LEVEL, str(row), "%d.kt" % col)
    if not os.path.exists(path):
        return None, None
    with open(path, "rb") as f:
        assert f.read(4) == b"KPT1", path
        _lv, _c, _r, cells = struct.unpack("<4i", f.read(16))
        f.read(8)
        n = struct.unpack("<i", f.read(4))[0]
        heights = np.frombuffer(zlib.decompress(f.read(n)), dtype="<f2").astype(np.float32).reshape(cells, cells)
        n = struct.unpack("<i", f.read(4))[0]
        surf = np.frombuffer(zlib.decompress(f.read(n)), dtype=np.uint8).reshape(cells, cells)
    return heights, surf


def window(col, row, cols, rows):
    """Heights and kinds for a window of bake tiles (missing tiles: deep sea)."""
    H = np.full((rows * CELLS, cols * CELLS), -3000.0, dtype=np.float32)
    S = np.ones((rows * CELLS, cols * CELLS), dtype=np.uint8)
    for r in range(rows):
        for c in range(cols):
            h, s = read_tile(col + c, row + r)
            if h is not None:
                H[r * CELLS:(r + 1) * CELLS, c * CELLS:(c + 1) * CELLS] = h
                S[r * CELLS:(r + 1) * CELLS, c * CELLS:(c + 1) * CELLS] = s
    return H, S


# ------------------------------------------------------------------------------------------------ the frame's helpers
_world = osr.SpatialReference(); _world.ImportFromProj4(WORLD_PROJ)
_wgs = osr.SpatialReference(); _wgs.ImportFromEPSG(4326); _wgs.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
_to_ll = osr.CoordinateTransformation(_world, _wgs)


def lonlat_grid(west, north, n, step=64):
    """Longitude, latitude and the spring tide's high water at every pixel centre of an n x n window (worked on a
    coarse grid and filled in)."""
    k = np.arange(0, n + step, step, dtype=np.float64)
    gx, gy = np.meshgrid(west + (k + 0.5) * CELL, north - (k + 0.5) * CELL)
    pts = np.asarray(_to_ll.TransformPoints(np.column_stack([gx.ravel(), gy.ravel()]).tolist()), dtype=np.float64)[:, :2]
    lon_c, lat_c = pts[:, 0].reshape(gx.shape), pts[:, 1].reshape(gx.shape)
    hw_c = high_water(lon_c, lat_c)
    zoom = (np.arange(n, dtype=np.float64)) / step
    yy, xx = np.meshgrid(zoom, zoom, indexing="ij")
    lon = ndimage.map_coordinates(lon_c, [yy, xx], order=1)
    lat = ndimage.map_coordinates(lat_c, [yy, xx], order=1)
    hw = ndimage.map_coordinates(hw_c, [yy, xx], order=1).astype(np.float32)
    return lon, lat, hw


class Kk10:
    """The cleared share of the land about AD 800, over Europe (read once)."""

    def __init__(self):
        ds = gdal.Open(KK10)
        band = 8001 - KK10_YEAR_BP                         # band 1 is 8000 BP
        gt = ds.GetGeoTransform()
        self.lon0, self.lat0, self.d = -30.0, 76.0, gt[1]
        x0 = int(round((self.lon0 - gt[0]) / gt[1])); y0 = int(round((gt[3] - self.lat0) / gt[1]))
        nx = int(round(90.0 / gt[1])); ny = int(round(40.0 / gt[1]))
        a = ds.GetRasterBand(band).ReadAsArray(x0, y0, nx, ny).astype(np.float32)
        a[a < 0] = 0.0
        self.f = a * 1e-4
        ds = None
        print("KK10: band %d (%d BP), Europe mean cleared share %.3f, max %.2f" % (band, KK10_YEAR_BP, float(self.f.mean()), float(self.f.max())), flush=True)

    def at(self, lon, lat):
        x = (lon - self.lon0) / self.d - 0.5
        y = (self.lat0 - lat) / self.d - 0.5
        return ndimage.map_coordinates(self.f, [y, x], order=1, mode="nearest")


class WorldCover:
    """Today's land cover, warped to a window of the art (from the tiles' overviews)."""

    def __init__(self):
        self.tiles = {}
        for p in glob.glob(os.path.join(WORLDCOVER, "*_Map.tif")):
            tag = os.path.basename(p).split("_")[5]            # N45W063
            lat = int(tag[1:3]) * (1 if tag[0] == "N" else -1)
            lon = int(tag[4:7]) * (1 if tag[3] == "E" else -1)
            self.tiles[(lat, lon)] = p
        print("WorldCover: %d tiles" % len(self.tiles), flush=True)

    def window(self, west, north, n, lon, lat):
        lo0, lo1, la0, la1 = float(lon.min()), float(lon.max()), float(lat.min()), float(lat.max())
        srcs = []
        for (tlat, tlon), p in self.tiles.items():
            if tlon + 3 < lo0 or tlon > lo1 or tlat + 3 < la0 or tlat > la1:
                continue
            srcs.append(gdal.OpenEx(p, open_options=["OVERVIEW_LEVEL=1"]))      # 9000 px a tile: about 40 m
        out = np.zeros((n, n), dtype=np.uint8)
        if not srcs:
            return out
        dst = gdal.GetDriverByName("MEM").Create("", n, n, 1, gdal.GDT_Byte)
        dst.SetGeoTransform((west, CELL, 0.0, north, 0.0, -CELL))
        dst.SetProjection(_world.ExportToWkt())
        gdal.Warp(dst, srcs, resampleAlg="mode", multithread=True, warpOptions=["NUM_THREADS=ALL_CPUS"])
        out = dst.GetRasterBand(1).ReadAsArray()
        dst = None
        return out


def _hash01(ix, iy):
    h = (ix.astype(np.uint64) * np.uint64(374761393) + iy.astype(np.uint64) * np.uint64(668265263)) & np.uint64(0xFFFFFFFF)
    h = ((h ^ (h >> np.uint64(13))) * np.uint64(1274126177)) & np.uint64(0xFFFFFFFF)
    h = h ^ (h >> np.uint64(16))
    return (h & np.uint64(0xFFFFFF)).astype(np.float32) / np.float32(0x1000000)


def _value_noise(gx, gy, cell):
    x = gx / cell; y = gy / cell
    x0 = np.floor(x); y0 = np.floor(y)
    fx = (x - x0).astype(np.float32); fy = (y - y0).astype(np.float32)
    fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy)
    x0 = x0.astype(np.int64) + 100000; y0 = y0.astype(np.int64) + 100000
    a = _hash01(x0, y0); b = _hash01(x0 + 1, y0); c = _hash01(x0, y0 + 1); d = _hash01(x0 + 1, y0 + 1)
    return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy


_cdf = None

def _grid(west, north, n, shift):
    gx0 = int(round((west - WEST) / CELL)) + shift[0]; gy0 = int(round((NORTH - north) / CELL)) + shift[1]
    return np.meshgrid(np.arange(gx0, gx0 + n, dtype=np.float64), np.arange(gy0, gy0 + n, dtype=np.float64))


def patch_noise(west, north, n, shift=(0, 0)):
    """Smooth noise in 0..1, evenly spread, on the whole map's own pixel grid (so tiles join): the shapes of the
    clearings, and (shifted, so it is another field) of every other kind of ground's patches."""
    global _cdf
    gx, gy = _grid(west, north, n, shift)
    v = 0.55 * _value_noise(gx, gy, 26.0) + 0.3 * _value_noise(gx + 977, gy + 311, 9.0) + 0.15 * _value_noise(gx + 55, gy + 4099, 3.0)
    if _cdf is None:
        sx, sy = np.meshgrid(np.arange(0, 3000, 3, dtype=np.float64), np.arange(0, 3000, 3, dtype=np.float64))
        s = 0.55 * _value_noise(sx, sy, 26.0) + 0.3 * _value_noise(sx + 977, sy + 311, 9.0) + 0.15 * _value_noise(sx + 55, sy + 4099, 3.0)
        _cdf = np.sort(s.ravel())
    return np.searchsorted(_cdf, v.ravel()).reshape(v.shape).astype(np.float32) / float(len(_cdf))


def broad_noise(west, north, n, shift=(0, 0)):
    """Slow noise in 0..1 (about 8 km a swell): how far the salt marsh reaches, where the peat runs on."""
    gx, gy = _grid(west, north, n, shift)
    return 0.7 * _value_noise(gx, gy, 70.0) + 0.3 * _value_noise(gx + 431, gy + 2203, 22.0)


# ------------------------------------------------------------------------------------------------ painting
def two_tone(img, mask, pair, noise):
    """A kind of ground: its base colour, with patches of its second colour where the noise is low."""
    img[mask] = rgb(pair[0])
    img[mask & (noise < pair[2])] = rgb(pair[1])


def paint(H, S, hw, lat, wc, cleared_share, west, north, river_mask, lake_mask):
    """Heights and kinds (with the wide margin MB), the high water, land cover, cleared share and the vector masks
    (with the margin MW; west and north are that window's corner) -> an RGB picture of the tile without margins, and
    where its land lies beyond the 800 map (for the small rivers' lines)."""
    o = MB - MW
    n = PX + 2 * MW
    cut = (slice(o, o + n), slice(o, o + n))
    # ---- on the wide window: how far the sea is, and the land's height without the height model's speckle
    sea = S == 1
    d_sea = ndimage.distance_transform_edt(~sea).astype(np.float32) if sea.any() else np.full(S.shape, 1e9, np.float32)
    flats = (S == 2) & (d_sea <= FLATS_REACH)            # the bake's tidal flats; a stray one far inland is land
    del d_sea
    body = ndimage.binary_opening(sea | flats, structure=np.ones((3, 3), bool))      # and so is a speck of one or two cells
    flats &= ndimage.binary_dilation(body, structure=np.ones((3, 3), bool), iterations=2)
    del body
    tidal_water = sea | flats
    d_tide = (ndimage.distance_transform_edt(~tidal_water).astype(np.float32) if tidal_water.any() else np.full(S.shape, 1e9, np.float32))[cut]
    raw_land = ~np.isin(S, WATER_KINDS) & ~flats
    Hl = np.where(raw_land, H, np.float32(0.0))
    Hs = (ndimage.uniform_filter(Hl, size=5, mode="nearest") / np.maximum(ndimage.uniform_filter(raw_land.astype(np.float32), size=5, mode="nearest"), 1e-3))[cut]
    pit = (ndimage.minimum_filter(Hl, size=PIT_REACH, mode="nearest") < PIT)[cut]
    peat = S == 5
    d_peat = ndimage.distance_transform_edt(~peat).astype(np.float32)[cut] if peat.any() else None
    del Hl, raw_land, tidal_water, peat, sea
    H, S, flats, hw = H[cut], S[cut].copy(), flats[cut], hw[cut]

    # ---- water: the big rivers and the lakes cut into the land beyond the 800 map (the bake holds the Netherlands' own)
    S[(S == 2) & ~flats] = 20
    beyond = (S == 0) | (S >= 20)
    vector_river = beyond & river_mask
    S[beyond & lake_mask] = 13
    S[vector_river] = 15
    water = np.isin(S, WATER_KINDS)
    # The 800 map draws every brook, and draws its streams wide. Elsewhere only the great rivers are water and the
    # middling ones a line, so the same is done here: water where a river is RIVER_WIDE cells wide, or runs along one
    # of the rivers the game sails (the Kromme Rijn, the Vecht); the rest is land with a line on it.
    wet = water | flats
    core = ndimage.binary_opening(wet, structure=np.ones((RIVER_WIDE, RIVER_WIDE), bool))
    labels, count = ndimage.label(core)
    if count:                                             # a pool in a stream is not a wide river: the stretch stays a line
        core &= (np.bincount(labels.ravel()) >= BROOK_BODY)[labels]
    del labels
    core = ndimage.binary_dilation(core, structure=np.ones((3, 3), bool), iterations=2) & wet
    vos_river = (S == 15) & ~vector_river
    along = ndimage.uniform_filter(river_mask.astype(np.float32), size=GUIDE_NEAR, mode="nearest") > 0.0
    core |= along & ndimage.binary_opening(vos_river, structure=np.ones((2, 2), bool))
    brook = vos_river & ~core
    # the line: the stream's middle. A brook one cell wide is left out, as the least rivers are elsewhere
    mid3 = ndimage.binary_erosion(vos_river, structure=np.ones((3, 3), bool))
    mid2 = ndimage.binary_erosion(vos_river, structure=np.ones((2, 2), bool))
    brook_line = brook & (mid3 | (mid2 & ~ndimage.binary_dilation(mid3, structure=np.ones((3, 3), bool))))
    del mid3, mid2, along
    gone = brook
    labels, count = ndimage.label(water & ~brook)
    if count:                                             # the 800 map's least ponds go the same way
        small = np.bincount(labels.ravel()) < POND
        small[0] = False
        vos_ground = ((S >= 3) & (S <= 18) & ~water).astype(np.uint8)
        gone = brook | (small[labels] & (ndimage.maximum_filter(vos_ground, size=3) > 0))
    del labels
    if gone.any():
        ground = np.where(water | flats, 0, S).astype(np.uint8)          # the ground a brook runs through: its neighbours'
        for size in (3, 5, 7):
            near = ndimage.maximum_filter(ground, size=size)
            fill = gone & np.isin(S, WATER_KINDS) & (near > 0)
            S[fill] = near[fill]
        S[gone & np.isin(S, WATER_KINDS)] = 20
        water = np.isin(S, WATER_KINDS)
    land = ~water & ~flats
    vos = land & (S >= 3) & (S <= 18)                    # land of the 800 map
    beyond = land & ~vos

    # ---- noise: all on the map's own grid, so tiles join
    noise = patch_noise(west, north, n)                   # the clearings
    gx, gy = _grid(west, north, n, (0, 0))
    grain = (_hash01(gx.astype(np.int64) + 100000, gy.astype(np.int64) + 100000) - 0.5) * GRAIN
    del gx, gy
    tex = patch_noise(west, north, n, (3001, 1777)) + grain       # every other kind's patches, their edges frayed
    fleck = patch_noise(west, north, n, (7013, 5231)) + 0.4 * grain
    swell = broad_noise(west, north, n, (1213, 877))
    # the 800 map's polygon edges, roughened, and the edge of its data with them: each cell of land looks a few cells
    # aside for its kind (the water stays put)
    K = S
    if vos.any():
        dx = np.rint((patch_noise(west, north, n, (911, 4877)) - 0.5) * 2.0 * WARP).astype(np.int32)
        dy = np.rint((patch_noise(west, north, n, (5519, 263)) - 0.5) * 2.0 * WARP).astype(np.int32)
        yy, xx = np.indices(S.shape, dtype=np.int32)
        ys, xs = np.clip(yy + dy, 0, n - 1), np.clip(xx + dx, 0, n - 1)
        del yy, xx, dx, dy
        K = np.where(land & land[ys, xs], S[ys, xs], S)
        del ys, xs
        vos = land & (K >= 3) & (K <= 18)
        beyond = land & ~vos

    # ---- the kinds of ground
    reach = 0.6 + 0.8 * swell
    tidal = hw >= TIDAL
    wetland = beyond & (wc == WC_WETLAND)
    low = beyond & (Hs < hw + RISE) & ~pit                # lower than a spring tide reaches: marsh or fen before the dykes
    salt = (low | wetland) & tidal & (d_tide < SALT_REACH * reach)
    if d_peat is not None:                                # the 800 map's peat runs on past the edge of its data, thinning
        run_on = np.clip(1.0 - d_peat / BLEED, 0.0, 1.0) ** 1.5
        peat_on = beyond & (Hs < 60.0) & (patch_noise(west, north, n, (2377, 6101)) < run_on)
        salt &= ~peat_on
        wetland |= peat_on
    fen = (low | wetland) & ~salt
    salt |= vos & ((K == 4) | ((K == 3) & (H < 1.2) & (d_tide < VOS_SALT_REACH * (0.7 + 0.6 * swell))))
    fen |= vos & ((K == 5) | (K == 16))
    dune = vos & ((K == 6) | (K == 7)) & ((d_tide < BEACH) | ((K == 6) & (tex < DUNE_SAND)))
    bar = vos & (K == 17) & (ndimage.uniform_filter((water | flats).astype(np.float32), size=5, mode="nearest") > 0.0)
    dry = land & ~salt & ~fen & ~dune & ~bar              # woodland and clearings: the same rule on all dry land
    heath = dry & ((wc == WC_MOSS) | (((wc == WC_GRASS) | (wc == WC_SHRUB)) & ((H >= 350.0) | (lat >= 63.0)))
                   | ((wc == WC_BARE) & (H >= 200.0)))              # bare hill ground away from the water: open moor
    suitable = dry & ~heath & ((wc == WC_CROP) | (wc == WC_GRASS) | (wc == WC_BUILT))
    # the cleared share of a 9 km cell falls on its suitable land: the chance a suitable pixel is cleared
    local = ndimage.uniform_filter(suitable.astype(np.float32), size=75, mode="nearest")
    chance = np.clip(cleared_share / np.maximum(local, 0.06), 0.0, 1.0)
    cleared = suitable & (noise < chance)

    # ---- colours
    img = np.zeros(S.shape + (3,), dtype=np.uint8)
    depth = -H                                            # the sea by its depth; inland water and rivers as shallows
    img[water] = rgb("deep sea")
    img[water & (depth < 200)] = rgb("sea")
    img[water & (depth < 40)] = rgb("shoal")
    img[water & (depth < 10)] = rgb("shallows")
    img[(S == 13) | (S == 15)] = rgb("shallows")
    img[dry] = rgb(WOOD)
    img[heath] = rgb(HEATH)
    img[cleared] = rgb(CLEARING)
    two_tone(img, fen, FEN, tex)
    img[fen & (fleck < FEN_PEAT[1])] = rgb(FEN_PEAT[0])
    img[fen & (fleck > 1.0 - FEN_REED[1])] = rgb(FEN_REED[0])
    two_tone(img, salt, SALT, tex)
    img[dune | bar] = rgb(SAND)
    bare = beyond & (wc == WC_BARE)
    # beaches, dunes, river bars: bare ground by the water, and the far north's sands; a bare patch inland is a quarry now
    shore = (ndimage.uniform_filter((water | flats).astype(np.float32), size=SHORE, mode="nearest") > 0.0) | (lat >= 60.0)
    img[bare & shore & (H < 600.0)] = rgb(SAND)
    img[beyond & ((H >= 1200.0) | (bare & (H >= 600.0)))] = rgb("stone")
    img[beyond & ((wc == WC_SNOW) | (H >= 2200.0))] = rgb("snow")
    two_tone(img, flats, FLATS, tex)
    # the coast: a dark rim on the land's last pixel, a light one on the water's first (4-neighbours)
    solid = land
    wet = ~solid
    near_water = np.zeros_like(solid); near_land = np.zeros_like(solid)
    near_water[1:, :] |= wet[:-1, :]; near_water[:-1, :] |= wet[1:, :]; near_water[:, 1:] |= wet[:, :-1]; near_water[:, :-1] |= wet[:, 1:]
    near_land[1:, :] |= solid[:-1, :]; near_land[:-1, :] |= solid[1:, :]; near_land[:, 1:] |= solid[:, :-1]; near_land[:, :-1] |= solid[:, 1:]
    img[solid & near_water] = rgb("peat")
    img[water & near_land] = rgb("wave light")
    img[brook_line] = rgb("shoal")
    return img[MW:-MW, MW:-MW], beyond[MW:-MW, MW:-MW]


def read_rings(path):
    with open(path, "rb") as f:
        assert f.read(4) == b"KRNG"
        n, has_extra = struct.unpack("<II", f.read(8))
        lens = np.frombuffer(f.read(4 * n), dtype="<u4")
        extra = None
        if has_extra:
            extra = np.frombuffer(f.read(2 * n), dtype="<u2").copy()
            if n % 2:
                f.read(2)
        pts = np.frombuffer(f.read(), dtype="<i4").reshape(-1, 2).astype(np.float64) * 10.0
    rings, i = [], 0
    for L in lens:
        rings.append(pts[i:i + L])
        i += L
    return rings, extra


def to_world_frame():
    a = osr.SpatialReference(); a.ImportFromProj4(SCALE_PROJ)
    ct = osr.CoordinateTransformation(a, _world)
    def fn(pts):
        pts = np.asarray(pts, dtype=float).reshape(-1, 2)
        if len(pts) == 0:
            return pts
        return np.asarray(ct.TransformPoints(pts.tolist()), dtype=float)[:, :2]
    return fn


def load_vectors():
    """Rivers (points in the world frame, width in art pixels), lakes, and the places for the guide."""
    fn = to_world_frame()
    out = []
    rivers, widths = read_rings(os.path.join(SCALE_MAPS, "rivers.bin"))
    cut = float(np.percentile(widths, 60)); mid = float(np.percentile(widths, 90)); big = float(np.percentile(widths, 97))
    for r, w in zip(rivers, widths):
        if len(r) < 2 or w < cut:
            continue
        out.append((fn(r), 3 if w >= big else 2 if w >= mid else 1))
    with io.open(os.path.join(SCALE_MAPS, "maps.json"), "r", encoding="utf-8") as f:
        maps = json.load(f)
    for gr in maps["rivers"].get("1_true", []):
        p = np.asarray(gr["p"], dtype=np.float64).reshape(-1, 2) * 10.0
        out.append((fn(p), 2 if gr.get("branch") else 3))
    lakes, _ = read_rings(os.path.join(SCALE_MAPS, "lakes.bin"))
    lake_polys = [fn(r) for r in lakes if len(r) >= 3]
    places = [dict(p, wx=float(q[0]), wz=float(q[1])) for p, q in zip(maps["places"], fn([(p["x"] * 10.0, p["y"] * 10.0) for p in maps["places"]]))]
    return out, lake_polys, places


def vector_masks(west, north, n, rivers, lakes):
    """Wide rivers and lakes as masks, small rivers as a list of pixel lines, for an n x n window."""
    x1, z0 = west + n * CELL, north - n * CELL
    def px(p):
        return list(zip(((p[:, 0] - west) / CELL).tolist(), ((north - p[:, 1]) / CELL).tolist()))
    def seen(p):
        return not (p[:, 0].max() < west or p[:, 0].min() > x1 or p[:, 1].max() < z0 or p[:, 1].min() > north)
    wide = Image.new("L", (n, n), 0); dw = ImageDraw.Draw(wide)
    lake = Image.new("L", (n, n), 0); dl = ImageDraw.Draw(lake)
    thin = []
    for p, width in rivers:
        if not seen(p):
            continue
        if width >= 2:
            dw.line(px(p), fill=255, width=width)
        else:
            thin.append(px(p))
    for p in lakes:
        if seen(p):
            dl.polygon(px(p), fill=255)
    return np.asarray(wide) > 0, np.asarray(lake) > 0, thin


def bake_tile(tr, tc, rivers, lakes, kk, wcov, cache):
    """One art tile's picture (PX x PX, RGB)."""
    col, row = COL0 + tc * PER, ROW0 + tr * PER
    H, S = window(col - 1, row - 1, PER + 2, PER + 2)       # one bake tile of margin all round (MB cells)
    o = MB - MW
    n = PX + 2 * MW
    if not ((S[o:o + n, o:o + n] != 1).any()):
        art = np.zeros((PX, PX, 3), dtype=np.uint8)
        depth = -H[MB:-MB, MB:-MB]
        art[:] = rgb("deep sea"); art[depth < 200] = rgb("sea"); art[depth < 40] = rgb("shoal"); art[depth < 10] = rgb("shallows")
        return art
    west_b, north_b = WEST + tc * PX * CELL - MB * CELL, NORTH - tr * PX * CELL + MB * CELL     # the wide margin's corner
    west, north = west_b + o * CELL, north_b - o * CELL                                         # the land cover's
    lon, lat, hw = lonlat_grid(west_b, north_b, PX + 2 * MB)
    lon, lat = lon[o:o + n, o:o + n], lat[o:o + n, o:o + n]
    wc = None
    cached = os.path.join(cache, "wc_%d_%d_m%d.npy" % (tr, tc, MW)) if cache else None
    if cached and os.path.exists(cached):
        wc = np.load(cached)
    if wc is None:
        wc = wcov.window(west, north, n, lon, lat)
        if cached:
            np.save(cached, wc)
    share = kk.at(lon, lat).astype(np.float32)
    river_mask, lake_mask, thin = vector_masks(west, north, n, rivers, lakes)
    art, beyond = paint(H, S, hw, lat, wc, share, west, north, river_mask, lake_mask)
    # the small rivers: a line over the land beyond the 800 map
    im = Image.fromarray(art); d = ImageDraw.Draw(im)
    for line in thin:
        d.line([(x - MW, y - MW) for x, y in line], fill=rgb("shoal"), width=1)
    return np.where(beyond[:, :, None], np.asarray(im), art)


def main():
    ap = argparse.ArgumentParser(description="The chart's picture, as tiles (see the comment at the top of this file).")
    ap.add_argument("--out", default=OUT, help="the folder to write (default: Kapman2/MapArt/world)")
    ap.add_argument("--tiles", nargs="*", default=None, help="only these tiles, as row,col row,col ...; 'none': only the overviews and the index")
    ap.add_argument("--no-overview", action="store_true", help="bake the tiles named and stop (bake_whole_world.py puts the overviews together at the end)")
    ap.add_argument("--cache", default=None, help="a folder that keeps today's land cover per tile (for trying things out)")
    args = ap.parse_args()
    out = os.path.abspath(args.out)
    todo = set((r, c) for r in range(TILES_Y) for c in range(TILES_X))
    if args.tiles is not None:
        todo = set()
        for t in args.tiles:
            if t.lower() == "none":
                continue
            r, c = (int(v) for v in t.split(","))
            assert 0 <= r < TILES_Y and 0 <= c < TILES_X, "no tile %s (rows run 0..%d, columns 0..%d)" % (t, TILES_Y - 1, TILES_X - 1)
            todo.add((r, c))
    os.makedirs(out, exist_ok=True)
    if args.cache:
        os.makedirs(args.cache, exist_ok=True)
    rivers, lakes, places = load_vectors()
    print("rivers %d, lakes %d, places %d" % (len(rivers), len(lakes), len(places)), flush=True)
    kk = wcov = None
    if todo:
        kk = Kk10()
        wcov = WorldCover()
    over_px = 4096
    over_py = int(round(over_px * TILES_Y / float(TILES_X)))
    over = Image.new("RGB", (over_px, over_py), rgb("deep sea"))
    total = have = 0
    for tr in range(TILES_Y):
        for tc in range(TILES_X):
            path = os.path.join(out, "t_%d_%d.png" % (tr, tc))
            if (tr, tc) in todo:
                tile = Image.fromarray(bake_tile(tr, tc, rivers, lakes, kk, wcov, args.cache))
                tile.save(path + ".part", format="PNG", optimize=True)     # whole or not at all: the game may be reading
                os.replace(path + ".part", path)
                print("tile %d,%d  %.0f KB" % (tr, tc, os.path.getsize(path) / 1024.0), flush=True)
            elif os.path.exists(path) and not args.no_overview:
                tile = Image.open(path).convert("RGB")
            else:
                continue
            have += 1
            total += os.path.getsize(path)
            x0, y0 = int(round(tc * over_px / TILES_X)), int(round(tr * over_py / TILES_Y))
            x1, y1 = int(round((tc + 1) * over_px / TILES_X)), int(round((tr + 1) * over_py / TILES_Y))
            over.paste(tile.resize((x1 - x0, y1 - y0), Image.NEAREST), (x0, y0))
    if args.no_overview:
        return
    over.save(os.path.join(out, "overview.png"), optimize=True)
    # a guide the same size as the overview: the places of 793, for the redraw (not shown in the game)
    guide = over.copy()
    gd = ImageDraw.Draw(guide)
    mpp_over = SPAN / over_px
    try:
        from PIL import ImageFont
        font = ImageFont.truetype(r"C:\Windows\Fonts\segoeui.ttf", 15)
    except Exception:
        font = None
    for p in sorted(places, key=lambda q: -q["rank"]):
        x, y = (p["wx"] - WEST) / mpp_over, (NORTH - p["wz"]) / mpp_over
        if not (0 <= x < over_px and 0 <= y < over_py):
            continue
        r = 2 + int(p["rank"])
        gd.ellipse([x - r, y - r, x + r, y + r], fill=rgb("byzantium"), outline=rgb("warm white"))
        if p["rank"] >= 3 and not p["name"].startswith("("):
            gd.text((x + r + 3, y - 9), p["name"].split(",")[0], fill=rgb("warm white"), font=font)
    guide.save(os.path.join(out, "overview_guide.png"), optimize=True)
    meta = {
        "about": "The chart's picture, in tiles the game shows under its marks (Game/Map/MapArt.cs). Redraw any tile at the same size and the game shows yours. "
                 "The frame is the game's world frame (Lambert conformal, parallels 45N and 65N, centred 56N 5E; x east, z north, real metres). "
                 "Tile t_<row>_<col>.png: row 0 is the northernmost, col 0 the westernmost. overview.png is the whole window at one sixth the size, shown when zoomed far out. "
                 "overview_guide.png has the places of 793 marked: for drawing, not shown in the game.",
        "west": WEST, "north": NORTH, "metresPerPixel": CELL, "tilePixels": PX, "cols": TILES_X, "rows": TILES_Y,
        "tiles": "t_{row}_{col}.png", "overview": "overview.png", "overviewPixels": over_px,
        "palette": "Kapman2/Design/palette_byzantium.gpl",
        "legend": {"forest": "woodland; carr in the fen", "meadow": "cleared land: fields and pasture about 800; the salt marsh's ground",
                   "grass": "heath and moor; the ground of peat and fen",
                   "grass with patches of forest and flecks of timber and reed light": "peat bog, fen and marsh",
                   "meadow with patches of reed light": "salt marsh",
                   "pale stone with patches of shallows": "tidal flats",
                   "pale sand": "beaches, bars and open dune sand",
                   "stone": "bare rock and mountain", "snow": "snow and ice",
                   "shoal, one pixel": "small rivers and streams"},
        "made": "Bake/world/make_map_art.py from the world bake (level 2, 120 m), KK10 (1150 BP), ESA WorldCover 2021 and the scale-map bake's rivers and lakes",
        "credits": ["Made with Natural Earth. Free vector and raster map data @ naturalearthdata.com.",
                    "Paleogeografische kaarten van Nederland, P.C. Vos & S. de Vries (Deltares/TNO), Rijksdienst voor het Cultureel Erfgoed, CC BY 4.0",
                    "HydroRIVERS (Lehner & Grill 2013), HydroSHEDS licence",
                    "Kaplan, J.O. and Krumhardt, K.M. (2011): The KK10 Anthropogenic Land Cover Change scenario, PANGAEA, CC BY 3.0",
                    "© ESA WorldCover project 2021 / Contains modified Copernicus Sentinel data (2021) processed by ESA WorldCover consortium, CC BY 4.0"],
    }
    with io.open(os.path.join(out, "map_art.json"), "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=1)
    print("done: %d tiles baked, %d of %d in %s, %.1f MB; window x %.0f..%.0f km, z %.0f..%.0f km" %
          (len(todo), have, TILES_X * TILES_Y, out, total / 1e6, WEST / 1000, (WEST + SPAN) / 1000, (NORTH - SPAN_Y) / 1000, NORTH / 1000), flush=True)


if __name__ == "__main__":
    main()
