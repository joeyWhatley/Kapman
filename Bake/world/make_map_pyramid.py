# -*- coding: utf-8 -*-
r"""
MAKE MAP PYRAMID (the user, 4 Oct 2026 late, of the 120 m chart: "Why's the map so small I want you to make it
absolutely collosal full sized full detail full resolution"). The same picture as make_map_art.py, baked again at
finer levels the chart can zoom through, as far as the data honestly carries.

  run.bat world\make_map_pyramid.py                        -> every level, every tile still to do (stop it and start
                                                              it again: finished tiles are not done twice)
  run.bat world\make_map_pyramid.py --levels 2             -> only the 30 m level
  run.bat world\make_map_pyramid.py --ports dorestad witla -> only the tiles (of every level) round these ports
  run.bat world\make_map_pyramid.py --tiles 2:16,9 3:48,27 -> only these tiles, as level:row,col
  run.bat world\make_map_pyramid.py --redo ...             -> paint them again even if they are there
  run.bat world\make_map_pyramid.py --jobs 6 --jobs-fine 3 -> tiles painted at once (the 10 m level takes more memory)
  run.bat world\make_map_pyramid.py --plan                 -> only say what there is to do
  run.bat world\make_map_pyramid.py --index                -> only write pyramid.json from what is on disk
  run.bat world\make_map_pyramid.py --crop dorestad 5.343 51.972
                                                           -> a picture of every level round a point, one art pixel
                                                              a screen pixel, in Captures/map/pyramid (for looking)

THE LEVELS (all in Kapman2/MapArt/world, 4096-pixel tiles, row 0 the northernmost, col 0 the westernmost, every level
starting at the same north-west corner):
  level 0   120 m a pixel    6 x 6 tiles   t_<row>_<col>.png       make_map_art.py's own tiles, where they always were
  level 1    60 m a pixel   12 x 12        L1/t_<row>_<col>.png
  level 2    30 m a pixel   24 x 24        L2/t_<row>_<col>.png    the whole window: 98,304 pixels a side
  level 3    10 m a pixel   72 x 72        L3/t_<row>_<col>.png    only round the 50 ports of ports_793.json: the four
                                                                    tiles nearest each (82 km a side, the port at
                                                                    least 20 km from the edge)
A tile that is all open sea has no file (the chart shows the coarser level there); pyramid.json lists them.

WHY THESE LEVELS, FROM WHAT IS ON DISK (5 Oct 2026):
  - Height. The world bake holds 30 m (its level 0) only in a ribbon along the coasts and the sailed rivers (14,205 of
    the window's 147,456 tiles: every coast tile but 20), 60 m a little wider, 120 m everywhere. The raw heights
    (Copernicus GLO-30) are 30 m over all the land of the window; nothing on disk is finer, but for lidar in the
    middle of the Netherlands (AHN, 5 m) and on the tidal Thames (2 m), which the bake has already taken in at 30 m.
    So the shore is true to 30 m: that is the finest level that covers the window. On a flat map heights only decide
    which ground is low enough to be marsh and which is high enough to be moor, rock or snow; inland, where the bake
    has only 120 m, they are filled in between its cells (no line on the picture moves by it more than a pixel or two).
  - Land cover. ESA WorldCover is 10 m everywhere. At 30 m each pixel takes the commonest of its nine 10 m cells; at
    10 m the cells themselves. This is where the 10 m level's detail comes from, and today's water with it: rivers
    wider than a pixel or two, lakes, harbours and inlets at their true width.
  - The land of 800. In the Netherlands the kinds of ground are the Vos reconstruction, drawn for about 1:100,000:
    its edges are not better than a few hundred metres at any level (make_map_art roughens them on purpose). The 10 m
    level adds nothing to them. KK10's cleared share is one figure for 9 km: the clearings' outlines are a noise field
    whose finest feature is 360 m, the same field at every level (a clearing at 120 m is the same clearing at 10 m,
    cut finer where WorldCover says the ground is wood, field or water).
  - The rivers. The bake cuts the sailed rivers of the North Sea core for the boat of the 1:32 game: up to five times
    their true width (the Lek at Dorestad 1.2 km for 350 m). The 120 m map draws those. The finer levels do not:
    here every river has its true width. In the Netherlands: the Vos map's own inland water, and the bake's river
    network (work/rivers/rivers793.gpkg) as a ribbon of the real width it records at every point; the smaller Vos
    watercourses, whose width nobody knows, as a one-pixel line. Elsewhere: WorldCover's water, and the small rivers of
    the 120 m map as a line where WorldCover sees no water along them (their line comes from a 450 m flow grid: it
    wanders beside the true valley by a few hundred metres, and at 10 m that shows).
  Below 10 m there is nothing to draw from: no finer level is made.

HOW IT PAINTS: with make_map_art.py's own rules, imported: its colours, kinds of ground, tide table, clearing rule,
rims. The rules count some lengths in 120 m cells (how far salt marsh reaches, the 9 km mean); here the module is
loaded again for each level with its cell set to the level's pixel, and those lengths grown to match (see `rules`).
Lengths on the picture (a rim is one pixel) stay pixels. If make_map_art.py's paint() changes, this script says so
in its log until REVIEWED below is brought up to date: read the change, then run --fingerprint for the new value.

Stopping and starting: a tile is written whole (to .part, then renamed), sea tiles are noted in L<n>/sea.txt as they
are found, and both are skipped the next time. Several tiles are painted at once, each in its own process.
"""
import argparse, hashlib, importlib.util, inspect, io, json, math, os, sys, time
import numpy as np
from osgeo import gdal, ogr, osr
from PIL import Image, ImageDraw
from scipy import ndimage

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)
import make_map_art as art                      # the 120 m map: its window, palette, kinds of ground, painting rules

gdal.UseExceptions(); ogr.UseExceptions(); osr.UseExceptions()

OUT = art.OUT
ROOT = art.ROOT
CAPTURES = os.path.join(ROOT, "Captures", "map", "pyramid")
PORTS = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data", "ports_793.json")
NETWORK = r"G:\kapman_unity\data\work\rivers\rivers793.gpkg"            # the sailed rivers: real course, real width
VOS_MAP = r"G:\kapman_unity\data\work\layer1\vos800_world.gpkg"         # the 800 map's polygons, in the world frame
VOS_STREAMS = r"G:\kapman_unity\data\work\layer1\vos800_rivers_world.gpkg"   # its watercourses (lines)
PX = art.PX                                     # 4096
WEST, NORTH, SPAN = art.WEST, art.NORTH, art.SPAN
LEVELS = {1: 60.0, 2: 30.0, 3: 10.0}            # metres a pixel; level 0 is make_map_art's 120 m
BAKE_LEVEL = {1: 1, 2: 0, 3: 0}                 # the bake level each one reads first (L1 = 60 m, L0 = 30 m)
UP = {1: 1, 2: 1, 3: 3}                         # art pixels to one cell of that bake level
COVER_FROM = {1: (0, "mode"), 2: (None, "mode"), 3: (None, "near")}     # WorldCover: overview (None: its own 10 m), how
PORT_TILES = 2                                  # the 10 m level: this many tiles a side round each port
GAME_H = 32.0                                   # the 1:32 game the bake's rivers were cut for (kapman_bake/rivers793.py)
CARVE_KINDS = (16, 17, 18)                      # the bake's river banks: reeds, bars, cut bank (with the river, 15)
LINE_NEAR = 600.0                               # m: a drawn river line this near water for LINE_RUN m is that water's own
LINE_RUN = 1500.0
MIN_RIBBON = 1.5                                # pixels: a river narrower than this is a line
REVIEWED = ""                                   # sha1 of make_map_art.paint as last read against `rules` (--fingerprint)

# make_map_art.py counts in 120 m cells. Every number it keeps at the top of the file is named in one of these lists,
# and every window it hands scipy in WINDOWS, so that a finer level means the same thing on the ground:
GROUND_CELLS = ("SALT_REACH", "VOS_SALT_REACH", "FLATS_REACH", "BLEED", "WARP", "BEACH")     # lengths on the ground: x k
GROUND_WINDOWS = ("PIT_REACH", "GUIDE_NEAR", "SHORE")                                         # windows on the ground: x k, odd
PICTURE_CELLS = ("RIVER_WIDE", "POND", "BROOK_BODY")           # lengths on the picture (what is too small to draw): pixels
PIXEL_GRAIN = ("GRAIN",)                                       # the fray of a patch's edge, a pixel or two wide: / k
NOT_LENGTHS = ("KK10_YEAR_BP", "LEVEL", "CELL", "CELLS", "BAKE_WEST", "BAKE_NORTH", "COL0", "ROW0", "TILES", "PER", "PX", "WEST",
               "NORTH", "SPAN", "MB", "MW", "RISE", "TIDAL", "PIT", "DUNE_SAND", "WC_TREE", "WC_SHRUB", "WC_GRASS", "WC_CROP",
               "WC_BUILT", "WC_BARE", "WC_SNOW", "WC_WATER", "WC_WETLAND", "WC_MOSS")
# windows written as plain numbers in its paint(): (function, size in 120 m cells) -> on the ground or on the picture
WINDOWS = {("uniform_filter", 5): "ground", ("uniform_filter", 75): "ground",
           ("maximum_filter", 3): "picture", ("maximum_filter", 5): "picture", ("maximum_filter", 7): "picture"}
PASS_THROUGH = ("distance_transform_edt", "map_coordinates", "binary_opening", "binary_dilation", "binary_erosion", "label")

CREDITS = [
    "Copernicus DEM GLO-30 and GLO-90 (c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space GmbH 2014-2018, provided under COPERNICUS by the European Union and ESA; all rights reserved",
    "DeltaDTM v1.1 (Pronk et al. 2024), Deltares, CC BY 4.0",
    "EMODnet Bathymetry Consortium (2024): EMODnet Digital Bathymetry (DTM 2024), CC BY 4.0",
    "GEBCO Compilation Group (2025) GEBCO 2025 Grid",
    "Paleogeografische kaarten van Nederland, P.C. Vos & S. de Vries (Deltares/TNO), Rijksdienst voor het Cultureel Erfgoed, CC BY 4.0",
    "AHN4 (Rijkswaterstaat and the water boards), CC0 1.0",
    "Contains Environment Agency information (c) Environment Agency and/or database right 2022. Licensed under the Open Government Licence v3.0.",
    "HydroRIVERS (Lehner & Grill 2013), HydroSHEDS licence",
    "Allen, G. H. & Pavelsky, T. M. (2018), Global extent of rivers and streams (GRWL), Science 361, CC BY 4.0",
    "Kaplan, J.O. and Krumhardt, K.M. (2011): The KK10 Anthropogenic Land Cover Change scenario, PANGAEA, CC BY 3.0",
    "(c) ESA WorldCover project 2021 / Contains modified Copernicus Sentinel data (2021) processed by ESA WorldCover consortium, CC BY 4.0",
    "Made with Natural Earth. Free vector and raster map data @ naturalearthdata.com.",
]


def tiles_a_side(level):
    return 6 if level == 0 else int(round(SPAN / (PX * LEVELS[level])))


def tile_path(level, tr, tc, out=None):
    out = out or OUT
    return os.path.join(out, "t_%d_%d.png" % (tr, tc)) if level == 0 else os.path.join(out, "L%d" % level, "t_%d_%d.png" % (tr, tc))


# ---------------------------------------------------------------------------- make_map_art's rules, at a finer cell
def _odd(v):
    v = int(round(v))
    return v + 1 - v % 2


class GroundWindows(object):
    """scipy.ndimage as make_map_art's rules call it, when their cell is k times smaller than the 120 m they were
    written for: a window that stands for a length on the ground grows k times, one that stands for pixels stays.
    A call this script has not been told about stops the bake: someone must say which of the two it is."""

    def __init__(self, k, grown):
        self.k = k
        self.grown = set(grown)                                      # the named windows, grown already

    def _size(self, name, size):
        if size in self.grown:
            return size
        kind = WINDOWS.get((name, size))
        if kind == "ground":
            return _odd(size * self.k)
        if kind == "picture":
            return size
        raise RuntimeError("make_map_art.py now calls ndimage.%s(size=%r): say in make_map_pyramid.py's WINDOWS whether that window is a "
                           "length on the ground or on the picture" % (name, size))

    def uniform_filter(self, a, size=3, **kw):
        return ndimage.uniform_filter(a, size=self._size("uniform_filter", size), **kw)

    def minimum_filter(self, a, size=3, **kw):
        return ndimage.minimum_filter(a, size=self._size("minimum_filter", size), **kw)

    def maximum_filter(self, a, size=3, **kw):
        return ndimage.maximum_filter(a, size=self._size("maximum_filter", size), **kw)

    def __getattr__(self, name):
        if name in PASS_THROUGH:
            return getattr(ndimage, name)
        raise RuntimeError("make_map_art.py now calls ndimage.%s: say in make_map_pyramid.py how it scales (GroundWindows)" % name)


def check_rules():
    """Stops if make_map_art.py keeps a number this script has not been told the meaning of."""
    known = set(GROUND_CELLS + GROUND_WINDOWS + PICTURE_CELLS + PIXEL_GRAIN + NOT_LENGTHS)
    new = sorted(nm for nm, v in vars(art).items() if nm.isupper() and isinstance(v, (int, float)) and not isinstance(v, bool) and nm not in known)
    if new:
        raise RuntimeError("make_map_art.py has numbers make_map_pyramid.py does not know: %s. Name each in GROUND_CELLS (a length on the ground, "
                           "in 120 m cells), GROUND_WINDOWS, PICTURE_CELLS (a length in pixels) or NOT_LENGTHS, near the top of make_map_pyramid.py." % ", ".join(new))
    gone = sorted(nm for nm in GROUND_CELLS + GROUND_WINDOWS + PICTURE_CELLS + PIXEL_GRAIN if not hasattr(art, nm))
    if gone:
        raise RuntimeError("make_map_art.py no longer has: %s. Take them out of make_map_pyramid.py's lists." % ", ".join(gone))


def quick_value_noise(hash01):
    """make_map_art's _value_noise, worked the quick way for a grid whose x runs along the columns and y down the
    rows (every grid _grid makes): the lattice's numbers are drawn once with its own _hash01, then filled in along x
    and along y. The same numbers, a hundred times sooner on a fine grid (checked against the original in `rules`)."""
    def value_noise(gx, gy, cell):
        x = np.asarray(gx[0, :], dtype=np.float64) / cell
        y = np.asarray(gy[:, 0], dtype=np.float64) / cell
        x0 = np.floor(x); y0 = np.floor(y)
        fx = (x - x0).astype(np.float32); fy = (y - y0).astype(np.float32)
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy)
        ix = x0.astype(np.int64); iy = y0.astype(np.int64)
        lx = np.arange(ix.min(), ix.max() + 2); ly = np.arange(iy.min(), iy.max() + 2)
        LX, LY = np.meshgrid(lx + 100000, ly + 100000)
        L = hash01(LX, LY)
        jx = ix - lx[0]; jy = iy - ly[0]
        A = L[:, jx] * (1 - fx) + L[:, jx + 1] * fx                  # along x, for each lattice row
        return A[jy] * (1 - fy)[:, None] + A[jy + 1] * fy[:, None]
    return value_noise


_rules = {}

def rules(level):
    """make_map_art, loaded again with its cell set to this level's pixel: its own paint(), noise and tide table,
    with the lengths it keeps in cells grown to the same metres, and its noise fields the same fields on the ground."""
    if level in _rules:
        return _rules[level]
    check_rules()
    mpp = LEVELS[level]
    k = art.CELL / mpp
    spec = importlib.util.spec_from_file_location("make_map_art_at_%dm" % int(mpp), art.__file__)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    for name in GROUND_CELLS:
        setattr(m, name, getattr(m, name) * k)
    for name in GROUND_WINDOWS:
        setattr(m, name, _odd(getattr(m, name) * k))
    for name in PIXEL_GRAIN:
        setattr(m, name, getattr(m, name) / k)
    m.MB, m.MW = int(round(m.MB * k)), int(round(m.MW * k))
    m.CELL = mpp
    corner = (m.WEST, m.NORTH)

    def grid(west, north, n, shift, _k=k, _mpp=mpp):
        if sys._getframe(1).f_code.co_name == "paint":
            # paint() asks for the grid itself only to fray the patches' edges cell by cell: here pixel by pixel
            x = round((west - corner[0]) / _mpp) + np.arange(n, dtype=np.float64)
            y = round((corner[1] - north) / _mpp) + np.arange(n, dtype=np.float64)
            return np.meshgrid(x, y)
        # the noise fields: a pixel's place in the 120 m map's own pixel count (fine pixel j sits at (j + 0.5) / k - 0.5)
        x = (west - corner[0]) / art.CELL + (np.arange(n, dtype=np.float64) + 0.5) / _k - 0.5 + shift[0]
        y = (corner[1] - north) / art.CELL + (np.arange(n, dtype=np.float64) + 0.5) / _k - 0.5 + shift[1]
        return np.meshgrid(x, y)

    m._grid = grid
    m.ndimage = GroundWindows(k, [getattr(m, name) for name in GROUND_WINDOWS])
    # its value noise the quick way, if that still gives its own numbers
    quick = quick_value_noise(m._hash01)
    tx, ty = np.meshgrid(np.arange(-40.3, 211.0, 0.37), np.arange(977.1, 1100.0, 0.41))
    if all(float(np.abs(quick(tx, ty, c) - m._value_noise(tx, ty, c)).max()) < 1e-5 for c in (3.0, 26.0)):
        m._value_noise = quick
    else:
        print("make_map_art's _value_noise has changed: using it as it is (slow at this level)", flush=True)
    _rules[level] = m
    return m


def fingerprint():
    return hashlib.sha1(inspect.getsource(art.paint).encode("utf-8")).hexdigest()[:12]


# ---------------------------------------------------------------------------- the bake, at its finest
def read_kt(level, col, row):
    art.LEVEL = level
    try:
        return art.read_tile(col, row)
    finally:
        art.LEVEL = 2


def finer(H, S, up):
    """Heights and kinds on a grid `up` times finer: the kinds as they are (each cell cut into up x up), the heights
    filled in between the cells, the land's from land and the water's from water (so no shore is pulled under)."""
    S2 = np.repeat(np.repeat(S, up, axis=0), up, axis=1)
    water = np.isin(S, (1, 2, 13, 15))
    if not water.any() or water.all():
        return ndimage.zoom(H, up, order=1, grid_mode=True, mode="nearest"), S2
    out = np.empty(S2.shape, dtype=np.float32)
    for mask in (water, ~water):
        wt = ndimage.zoom(mask.astype(np.float32), up, order=1, grid_mode=True, mode="nearest")
        hv = ndimage.zoom(np.where(mask, H, np.float32(0.0)), up, order=1, grid_mode=True, mode="nearest")
        big = np.repeat(np.repeat(mask, up, axis=0), up, axis=1)
        out[big] = (hv[big] / np.maximum(wt[big], 1e-3))
        del wt, hv, big
    return out, S2


def bake_window(b, cx0, cy0, w, h):
    """Heights and kinds for cells [cx0, cx0 + w) x [cy0, cy0 + h) of the bake's level b grid: from level b where it is
    baked, from the coarser levels where it is not (kinds as they are, heights filled in between the coarser cells)."""
    C = art.CELLS
    if b >= 2:
        H = np.full((h, w), -3000.0, dtype=np.float32)
        S = np.ones((h, w), dtype=np.uint8)
    else:
        px0, py0 = cx0 // 2 - 1, cy0 // 2 - 1
        pw, ph = (cx0 + w + 1) // 2 + 1 - px0, (cy0 + h + 1) // 2 + 1 - py0
        Hc, Sc = bake_window(b + 1, px0, py0, pw, ph)
        ox, oy = cx0 - 2 * px0, cy0 - 2 * py0
        H, S = finer(Hc, Sc, 2)
        H, S = np.ascontiguousarray(H[oy:oy + h, ox:ox + w]), np.ascontiguousarray(S[oy:oy + h, ox:ox + w])
        del Hc, Sc
    for r in range(cy0 // C, (cy0 + h - 1) // C + 1):
        for c in range(cx0 // C, (cx0 + w - 1) // C + 1):
            th, ts = read_kt(b, c, r)
            if th is None:
                continue
            x, y = c * C - cx0, r * C - cy0
            xa, ya, xb, yb = max(x, 0), max(y, 0), min(x + C, w), min(y + C, h)
            H[ya:yb, xa:xb] = th[ya - y:yb - y, xa - x:xb - x]
            S[ya:yb, xa:xb] = ts[ya - y:yb - y, xa - x:xb - x]
    return H, S


# ---------------------------------------------------------------------------- the rivers at their true width
class Network(object):
    """The sailed rivers of the North Sea core (rivers793.gpkg): points in the world frame, the real width at each
    (Z) and the width the bake cut there for the boat (M, game metres)."""

    def __init__(self):
        self.rivers = []
        self.box = None
        if not os.path.exists(NETWORK):
            print("no river network at %s: the bake's rivers stay as they are" % NETWORK, flush=True)
            return
        ds = ogr.Open(NETWORK)
        for f in ds.GetLayer(0):
            g = f.GetGeometryRef()
            n = g.GetPointCount()
            if n < 2:
                continue
            p = np.array([(g.GetX(i), g.GetY(i), g.GetZ(i), g.GetM(i)) for i in range(n)], dtype=np.float64)
            self.rivers.append((p[:, :2], p[:, 2], p[:, 3], int(f.GetField("carve") or 0)))
        allp = np.vstack([r[0] for r in self.rivers])
        self.box = (allp[:, 0].min(), allp[:, 1].min(), allp[:, 0].max(), allp[:, 1].max())
        print("river network: %d rivers, %.0f km" % (len(self.rivers), sum(np.hypot(*np.diff(r[0], axis=0).T).sum() for r in self.rivers) / 1000.0), flush=True)

    def near(self, west, north, w, h, cell, pad=3000.0):
        if self.box is None:
            return False
        return not (self.box[2] < west - pad or self.box[0] > west + w * cell + pad or self.box[3] < north - h * cell - pad or self.box[1] > north + pad)

    def seen(self, west, north, w, h, cell, pad=3000.0):
        x1, z0 = west + w * cell + pad, north - h * cell - pad
        for p, real, cut, carve in self.rivers:
            if p[:, 0].max() < west - pad or p[:, 0].min() > x1 or p[:, 1].max() < z0 or p[:, 1].min() > north + pad:
                continue
            yield p, real, cut, carve


def _runs(width_px):
    """Stretches of a line that are drawn with one width: (first point, last point, width)."""
    out, a = [], 0
    for i in range(1, len(width_px) + 1):
        if i == len(width_px) or width_px[i] != width_px[a]:
            out.append((a, min(i, len(width_px) - 1), int(width_px[a])))
            a = i
    return out


def draw_ribbon(d, xy, width_px, fill=255):
    """A line whose width changes along it. Wide stretches are a disc at every point (the points lie closer than a
    pixel or two, so the discs' edge is the river's bank, smooth); narrow ones a plain line of their width."""
    for a, b, wd in _runs(np.minimum(width_px, 4)):
        if b <= a or wd <= 0:
            continue
        pts = [tuple(v) for v in xy[a:b + 1].tolist()]
        if wd < 4:
            d.line(pts, fill=fill, width=wd, joint="curve")
            continue
        seg = np.hypot(*np.diff(xy[a:b + 1], axis=0).T)
        if len(seg) and seg.max() > 1.5:                             # sparse points: fill in between them
            along = np.concatenate([[0.0], np.cumsum(seg)])
            t = np.arange(0.0, along[-1] + 0.5, 1.0)
            x = np.interp(t, along, xy[a:b + 1, 0]); y = np.interp(t, along, xy[a:b + 1, 1])
            r = np.interp(t, along, width_px[a:b + 1]) / 2.0
        else:
            x, y, r = xy[a:b + 1, 0], xy[a:b + 1, 1], width_px[a:b + 1] / 2.0
        for px, py, pr in zip(x.tolist(), y.tolist(), r.tolist()):
            d.ellipse([px - pr, py - pr, px + pr, py + pr], fill=fill)


def _fill_from_nearest(arrays, gone, source):
    """Cells in `gone` take, in each of the arrays, the value of the nearest cell in `source`."""
    if not gone.any() or not source.any():
        return
    h, w = gone.shape
    ys, xs = np.nonzero(gone)
    pad = 64
    while True:                                                      # a box round them wide enough to hold a source
        y0, y1, x0, x1 = max(ys.min() - pad, 0), min(ys.max() + pad + 1, h), max(xs.min() - pad, 0), min(xs.max() + pad + 1, w)
        if source[y0:y1, x0:x1].any() or (y0 == 0 and x0 == 0 and y1 == h and x1 == w):
            break
        pad *= 4
    sub = gone[y0:y1, x0:x1]
    iy, ix = ndimage.distance_transform_edt(~source[y0:y1, x0:x1], return_distances=False, return_indices=True)
    iy, ix = iy[sub], ix[sub]
    for a in arrays:
        part = a[y0:y1, x0:x1]
        part[sub] = part[iy, ix]


def uncarve(H, S, west, north, cell, net):
    """Takes the boat's rivers out of a window of the bake (cells of `cell` metres, corner west/north). Along the
    rivers the bake cut for the boat, the cut (15), its banks (16, 17, 18) and the hair-thin sea its sweep left take
    the kind and height of the nearest dry land: what the bake cut away was land (it kept the sea and the flats).
    The 800 map's other watercourses (15, cut 90 m wide by rule) take whatever lies nearest. The true rivers are
    drawn in afterwards."""
    h, w = S.shape
    river = S == 15
    zone = None
    if net.near(west, north, w, h, cell) and (river.any() or np.isin(S, CARVE_KINDS).any()):
        im = Image.new("L", (w, h), 0)
        d = ImageDraw.Draw(im)
        for p, real, cut, carve in net.seen(west, north, w, h, cell):
            if not carve:
                continue
            xy = np.column_stack([(p[:, 0] - west) / cell, (north - p[:, 1]) / cell])
            # the cut's own width on the ground, its wandering edge and banks, and some room
            wd = np.maximum(4, np.ceil((cut * GAME_H * 1.5 + 400.0) / cell / 4.0) * 4)
            draw_ribbon(d, xy, wd)
        zone = np.asarray(im) > 0
        if not zone.any():
            zone = None
    if zone is not None:
        sea = S == 1
        body = ndimage.binary_opening(sea, structure=np.ones((3, 3), bool))
        body = ndimage.binary_dilation(body, structure=np.ones((3, 3), bool), iterations=2) & sea
        cut = zone & (river | np.isin(S, CARVE_KINDS) | (sea & ~body))
        _fill_from_nearest((H, S), cut, ~cut & ~river & ~np.isin(S, (1, 2, 13)))
        river = S == 15
    _fill_from_nearest((H, S), river, ~river)


class Streams(object):
    """The 800 map's inland water (polygons) and watercourses (lines), in the world frame."""

    def __init__(self):
        self.lines = []
        self.box = None
        self.water = None
        if os.path.exists(VOS_STREAMS):
            ds = ogr.Open(VOS_STREAMS)
            for f in ds.GetLayer(0):
                g = f.GetGeometryRef()
                for i in range(max(1, g.GetGeometryCount())):
                    part = g.GetGeometryRef(i) if g.GetGeometryCount() else g
                    p = np.asarray(part.GetPoints(), dtype=np.float64)[:, :2]
                    if len(p) >= 2:
                        self.lines.append(p)
        if os.path.exists(VOS_MAP):
            self.water = gdal.OpenEx(VOS_MAP, gdal.OF_VECTOR)
            x0, x1, y0, y1 = self.water.GetLayer(0).GetExtent()
            self.box = (x0, y0, x1, y1)
        print("the 800 map: %d watercourse lines%s" % (len(self.lines), "" if self.water else "; no polygons at " + VOS_MAP), flush=True)

    def inland_water(self, west, north, n, mpp):
        """The 800 map's inland water as a mask of an n x n window (None when the window is off the map)."""
        if self.water is None or self.box[2] < west or self.box[0] > west + n * mpp or self.box[3] < north - n * mpp or self.box[1] > north:
            return None
        dst = gdal.GetDriverByName("MEM").Create("", n, n, 1, gdal.GDT_Byte)
        dst.SetGeoTransform((west, mpp, 0.0, north, 0.0, -mpp))
        dst.SetProjection(art._world.ExportToWkt())
        ly = self.water.GetLayer(0)
        ly.SetAttributeFilter("legenda IN ('binnenwater', 'dekzand_hol', 'laag holoceen')")
        ly.SetSpatialFilterRect(west, north - n * mpp, west + n * mpp, north)
        gdal.RasterizeLayer(dst, [1], ly, burn_values=[1])
        ly.SetAttributeFilter(None); ly.SetSpatialFilter(None)
        a = dst.GetRasterBand(1).ReadAsArray() > 0
        return a if a.any() else None


def ground(level, X0, Y0, N, net, streams):
    """Heights and kinds for the art pixels [X0, X0 + N) x [Y0, Y0 + N) of a level (counted from the window's
    north-west corner): the bake at its finest, without the boat's rivers, with the true ones. Also the small rivers
    of the network that are lines at this level (pixel coordinates in this window)."""
    b, up, mpp = BAKE_LEVEL[level], UP[level], LEVELS[level]
    cell = mpp * up
    per = int(round(art.CELL / cell))                               # bake cells of level b to one 120 m cell
    gx0, gy0 = art.COL0 * art.CELLS * per, art.ROW0 * art.CELLS * per
    ax0, ay0 = X0 // up, Y0 // up                                    # floor, also when negative
    aw, ah = -(-(X0 + N) // up) - ax0, -(-(Y0 + N) // up) - ay0
    H, S = bake_window(b, gx0 + ax0, gy0 + ay0, aw, ah)
    west_c, north_c = WEST + ax0 * cell, NORTH - ay0 * cell
    uncarve(H, S, west_c, north_c, cell, net)
    if up > 1:
        ox, oy = X0 - ax0 * up, Y0 - ay0 * up
        H, S = finer(H, S, up)
        H, S = np.ascontiguousarray(H[oy:oy + N, ox:ox + N]), np.ascontiguousarray(S[oy:oy + N, ox:ox + N])
    west, north = WEST + X0 * mpp, NORTH - Y0 * mpp
    thin = []
    # the 800 map's own inland water (the bake holds it too, but under the boat's rivers it was lost)
    vos_water = streams.inland_water(west, north, N, mpp)
    if vos_water is not None:
        S[vos_water & (S >= 3) & (S <= 18)] = 13
    del vos_water
    # the network's rivers at the real width they record
    if net.near(west, north, N, N, mpp):
        wide = Image.new("L", (N, N), 0)
        d = ImageDraw.Draw(wide)
        any_wide = False
        for p, real, cut, carve in net.seen(west, north, N, N, mpp):
            xy = np.column_stack([(p[:, 0] - west) / mpp, (north - p[:, 1]) / mpp])
            wpx = real / mpp
            ribbon = wpx >= MIN_RIBBON
            wd = np.where(ribbon, np.where(wpx < 3.5, np.maximum(2.0, np.rint(wpx)), wpx), 0.0)
            if ribbon.any():
                draw_ribbon(d, xy, wd)
                any_wide = True
            for a, e, v in _runs(ribbon.astype(np.int64)):
                if v == 0 and e > a:
                    thin.append(xy[a:e + 1])
        if any_wide:
            ribbon = np.asarray(wide) > 0
            S[ribbon & (S != 1) & (S != 2)] = 13
            del ribbon
    return H, S, thin


# ---------------------------------------------------------------------------- the frame, the land cover
def fill_grid(C, n, step):
    """A field known at every `step`-th pixel centre (from the first), filled in for all n x n (float32)."""
    t = np.arange(n, dtype=np.float64) / step
    i0 = np.minimum(t.astype(np.int64), C.shape[1] - 2)
    f = (t - i0).astype(np.float32)
    C = C.astype(np.float32)
    rows = C[:, i0] * (1.0 - f) + C[:, i0 + 1] * f                   # along x, for each coarse row
    j0 = np.minimum(t.astype(np.int64), C.shape[0] - 2)
    g = (t - j0).astype(np.float32)[:, None]
    return rows[j0] * (1.0 - g) + rows[j0 + 1] * g


def frame_fields(m, kk, west, north, n, mpp):
    """For an n x n window: latitude, the spring tide's high water and KK10's cleared share at every pixel, and the
    window's box in longitude and latitude. Worked every 1.9 km and filled in."""
    step = max(8, int(round(1920.0 / mpp)))
    q = np.arange(0, n + step, step, dtype=np.float64)
    gx, gy = np.meshgrid(west + (q + 0.5) * mpp, north - (q + 0.5) * mpp)
    pts = np.asarray(art._to_ll.TransformPoints(np.column_stack([gx.ravel(), gy.ravel()]).tolist()), dtype=np.float64)[:, :2]
    lon_c, lat_c = pts[:, 0].reshape(gx.shape), pts[:, 1].reshape(gx.shape)
    box = (float(lon_c.min()), float(lon_c.max()), float(lat_c.min()), float(lat_c.max()))
    lat = fill_grid(lat_c - 56.0, n, step) + np.float32(56.0)
    hw = fill_grid(m.high_water(lon_c, lat_c), n, step)
    share = fill_grid(kk.at(lon_c, lat_c), n, step) if kk is not None else None
    return lat, hw, share, box


class Cover(object):
    """Today's land cover (ESA WorldCover 2021, 10 m), warped to a window of the art at the level's own pixel."""

    def __init__(self):
        self.tiles = art.WorldCover().tiles

    def window(self, level, west, north, n, box):
        lo0, lo1, la0, la1 = box
        ov, how = COVER_FROM[level]
        mpp = LEVELS[level]
        srcs = []
        for (tlat, tlon), p in self.tiles.items():
            if tlon + 3 < lo0 - 0.05 or tlon > lo1 + 0.05 or tlat + 3 < la0 - 0.05 or tlat > la1 + 0.05:
                continue
            srcs.append(gdal.Open(p) if ov is None else gdal.OpenEx(p, open_options=["OVERVIEW_LEVEL=%d" % ov]))
        if not srcs:
            return np.zeros((n, n), dtype=np.uint8)
        dst = gdal.GetDriverByName("MEM").Create("", n, n, 1, gdal.GDT_Byte)
        dst.SetGeoTransform((west, mpp, 0.0, north, 0.0, -mpp))
        dst.SetProjection(art._world.ExportToWkt())
        gdal.Warp(dst, srcs, resampleAlg=how, multithread=True, warpOptions=["NUM_THREADS=2"], warpMemoryLimit=512 * 1024 * 1024)
        out = dst.GetRasterBand(1).ReadAsArray()
        dst = None
        return out


# ---------------------------------------------------------------------------- the small rivers: lines
def line_mask(lines, n, off=0.0):
    """Pixel lines (arrays of x, y) as a one-pixel mask of an n x n picture."""
    im = Image.new("L", (n, n), 0)
    d = ImageDraw.Draw(im)
    for xy in lines:
        if len(xy) >= 2:
            d.line([(x - off, y - off) for x, y in xy.tolist()], fill=255, width=1)
    return np.asarray(im) > 0


def not_along_water(lines, dist, near_px, run_px):
    """The stretches of each line that do not run along water: a stretch that stays within near_px of water for
    run_px or more is that water's own river, drawn already; a short one is a stream reaching it and stays."""
    n = dist.shape[0]
    out = []
    for xy in lines:
        if len(xy) < 2:
            continue
        seg = np.hypot(*np.diff(xy, axis=0).T)
        along = np.concatenate([[0.0], np.cumsum(seg)])
        if along[-1] < 1.0:
            continue
        t = np.arange(0.0, along[-1] + 1.0, 2.0)
        x = np.interp(t, along, xy[:, 0]); y = np.interp(t, along, xy[:, 1])
        ix = np.clip(np.rint(x).astype(np.int64), 0, n - 1); iy = np.clip(np.rint(y).astype(np.int64), 0, n - 1)
        inside = (x >= 0) & (x < n) & (y >= 0) & (y < n)
        near = (dist[iy, ix] < near_px) & inside
        keep = np.ones(len(t), dtype=bool)
        a = 0
        while a < len(t):
            if not near[a]:
                a += 1
                continue
            e = a
            while e < len(t) and near[e]:
                e += 1
            if t[e - 1] - t[a] >= run_px:
                keep[a:e] = False
            a = e
        a = 0
        while a < len(t):
            if not keep[a]:
                a += 1
                continue
            e = a
            while e < len(t) and keep[e]:
                e += 1
            if e - a >= 2:
                out.append(np.column_stack([x[a:e], y[a:e]]))
            a = e
    return out


def clip_lines(polys, west, north, n, mpp, pad=64):
    """World-frame lines that touch an n x n window, in its pixel coordinates."""
    x1, z0 = west + (n + pad) * mpp, north - (n + pad) * mpp
    out = []
    for p in polys:
        if p[:, 0].max() < west - pad * mpp or p[:, 0].min() > x1 or p[:, 1].max() < z0 or p[:, 1].min() > north + pad * mpp:
            continue
        out.append(np.column_stack([(p[:, 0] - west) / mpp, (north - p[:, 1]) / mpp]))
    return out


# ---------------------------------------------------------------------------- one tile
_palette_keys = None

def to_palette(img):
    """An RGB picture in the palette's colours -> an indexed one (None if it holds any other colour)."""
    global _palette_keys
    if _palette_keys is None:
        cols = np.array([art.rgb(nm) for nm in art.PALETTE], dtype=np.uint32)
        keys = (cols[:, 0] << 16) | (cols[:, 1] << 8) | cols[:, 2]
        order = np.argsort(keys)
        _palette_keys = (keys[order], order.astype(np.uint8), cols.astype(np.uint8))
    keys, order, cols = _palette_keys
    v = (img[:, :, 0].astype(np.uint32) << 16) | (img[:, :, 1].astype(np.uint32) << 8) | img[:, :, 2]
    i = np.minimum(np.searchsorted(keys, v), len(keys) - 1)
    if not (keys[i] == v).all():
        return None
    im = Image.fromarray(order[i], "P")
    im.putpalette(cols.ravel().tolist())
    return im


def peak_memory_mb():
    try:
        import ctypes
        from ctypes import wintypes

        class PMC(ctypes.Structure):
            _fields_ = [("cb", wintypes.DWORD), ("PageFaultCount", wintypes.DWORD), ("PeakWorkingSetSize", ctypes.c_size_t),
                        ("WorkingSetSize", ctypes.c_size_t), ("QuotaPeakPagedPoolUsage", ctypes.c_size_t), ("QuotaPagedPoolUsage", ctypes.c_size_t),
                        ("QuotaPeakNonPagedPoolUsage", ctypes.c_size_t), ("QuotaNonPagedPoolUsage", ctypes.c_size_t),
                        ("PagefileUsage", ctypes.c_size_t), ("PeakPagefileUsage", ctypes.c_size_t)]
        pmc = PMC(); pmc.cb = ctypes.sizeof(PMC)
        k32 = ctypes.WinDLL("kernel32"); psapi = ctypes.WinDLL("psapi")
        k32.GetCurrentProcess.restype = wintypes.HANDLE
        psapi.GetProcessMemoryInfo.argtypes = [wintypes.HANDLE, ctypes.POINTER(PMC), wintypes.DWORD]
        if psapi.GetProcessMemoryInfo(k32.GetCurrentProcess(), ctypes.byref(pmc), pmc.cb):
            return pmc.PeakWorkingSetSize / 1e6
    except Exception:
        pass
    return 0.0


_ctx = None

def context():
    """What every tile needs, read once in each process."""
    global _ctx
    if _ctx is None:
        hydro, _lakes, _places = art.load_vectors()
        _ctx = {"kk": art.Kk10(), "cover": Cover(), "net": Network(), "streams": Streams(), "hydro": [p for p, _w in hydro]}
    return _ctx


def unmodern(wc, mpp):
    """Tree rows along the roads and canals of today, and the roads cut through the woods of today, were not there in 800.
    At the fine levels they show as straight strokes. Woodland narrower than about 70 m becomes the open ground beside
    it, and a gap in woodland narrower than that closes. Blocks of wood keep their shape."""
    if mpp > 30.0:
        return
    r = max(1, int(round(35.0 / mpp)))
    st = np.ones((2 * r + 1, 2 * r + 1), dtype=bool)
    tree = wc == art.WC_TREE
    body = ndimage.binary_opening(tree, structure=st)
    thin = tree & ~body
    _fill_from_nearest((wc,), thin, ~tree & (wc != art.WC_WATER) & (wc != 0))
    tree = wc == art.WC_TREE
    gaps = ndimage.binary_closing(tree, structure=st) & ~tree & (wc != art.WC_WATER) & (wc != 0)
    wc[gaps] = art.WC_TREE


def paint_tile(level, tr, tc, out=None, rgb_png=False):
    """Paints one tile and writes it. Returns a dict: what became of it ('tile' or 'sea'), seconds, bytes."""
    t0 = time.time()
    cx = context()
    m = rules(level)
    mpp = LEVELS[level]
    MB, MW = m.MB, m.MW
    N, n, o = PX + 2 * MB, PX + 2 * MW, MB - MW
    X0, Y0 = tc * PX - MB, tr * PX - MB
    H, S, net_thin = ground(level, X0, Y0, N, cx["net"], cx["streams"])
    if not (S[MB - 1:MB + PX + 1, MB - 1:MB + PX + 1] != 1).any():
        return {"level": level, "row": tr, "col": tc, "what": "sea", "seconds": time.time() - t0, "bytes": 0, "peak": peak_memory_mb()}
    west_b, north_b = WEST + X0 * mpp, NORTH - Y0 * mpp              # the wide margin's corner
    west, north = west_b + o * mpp, north_b - o * mpp                # the land cover's
    _lat, hw, _share, _box = frame_fields(m, None, west_b, north_b, N, mpp)
    del _lat
    lat, _hw, share, box = frame_fields(m, cx["kk"], west, north, n, mpp)
    del _hw
    wc = cx["cover"].window(level, west, north, n, box)
    unmodern(wc, mpp)
    S_cut = S[o:o + n, o:o + n]
    # today's water on the land of the 800 map (canals, pits, the new lakes) was not there then: it takes the land
    # cover beside it. (Beyond the 800 map today's water is the only water known, and is drawn.)
    stray = (S_cut >= 3) & (S_cut <= 18) & (wc == art.WC_WATER)
    _fill_from_nearest((wc,), stray, (wc != art.WC_WATER) & (wc != 0))
    del stray
    vos_land = ((S_cut >= 3) & (S_cut <= 18) & (S_cut != 13) & (S_cut != 15))[MW:-MW, MW:-MW].copy()
    none = np.zeros((n, n), dtype=bool)
    img, beyond = m.paint(H, S, hw, lat, wc, share, west, north, none, wc == art.WC_WATER)
    del H, S, hw, lat, share, none, S_cut
    img = np.ascontiguousarray(img)
    # ---- the small rivers, as lines on the land
    west_t, north_t = west + MW * mpp, north - MW * mpp
    shoal = np.array(art.rgb("shoal"), dtype=np.uint8)
    water_cols = [art.rgb(nm) for nm in ("deep sea", "sea", "shoal", "shallows", "wave light")]
    key = (img[:, :, 0].astype(np.uint32) << 16) | (img[:, :, 1].astype(np.uint32) << 8) | img[:, :, 2]
    water = np.isin(key, np.array([(r << 16) | (g << 8) | b for r, g, b in water_cols], dtype=np.uint32))
    del key
    land = beyond | vos_land
    net_lines = [xy - MB for xy in net_thin]                          # the wide window's pixels -> the tile's
    drawn = line_mask(net_lines, PX) if net_lines else np.zeros((PX, PX), dtype=bool)
    other = clip_lines(cx["hydro"], west_t, north_t, PX, mpp)
    vos_lines = clip_lines(cx["streams"].lines, west_t, north_t, PX, mpp) if vos_land.any() else []
    if other or vos_lines:
        wet = water | drawn
        dist = ndimage.distance_transform_edt(~wet).astype(np.float32) if wet.any() else np.full((PX, PX), 1e9, np.float32)
        near_px, run_px = LINE_NEAR / mpp, LINE_RUN / mpp
        if other:
            drawn |= line_mask(not_along_water(other, dist, near_px, run_px), PX) & beyond
        if vos_lines:
            drawn |= line_mask(not_along_water(vos_lines, dist, near_px, run_px), PX) & vos_land
        del dist, wet
    img[drawn & land & ~water] = shoal
    # ---- write it, whole or not at all
    path = tile_path(level, tr, tc, out)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    im = None if rgb_png else to_palette(img)
    if im is None:
        im = Image.fromarray(img)
    im.save(path + ".part", format="PNG", compress_level=6)
    os.replace(path + ".part", path)
    return {"level": level, "row": tr, "col": tc, "what": "tile", "seconds": time.time() - t0, "bytes": os.path.getsize(path), "peak": peak_memory_mb()}


def _job(args):
    level, tr, tc, out, rgb_png = args
    try:
        return paint_tile(level, tr, tc, out, rgb_png)
    except Exception as e:                                           # one bad tile must not end the night's bake
        import traceback
        return {"level": level, "row": tr, "col": tc, "what": "failed", "seconds": 0.0, "bytes": 0, "peak": 0.0,
                "error": "%s: %s\n%s" % (type(e).__name__, e, traceback.format_exc())}


# ---------------------------------------------------------------------------- which tiles
def load_ports():
    with io.open(PORTS, "r", encoding="utf-8") as f:
        ports = json.load(f)["ports"]
    to_world = osr.CoordinateTransformation(art._wgs, art._world)
    out = []
    for p in ports:
        x, z, _ = to_world.TransformPoint(float(p["lon"]), float(p["lat"]))
        out.append((p["id"], p.get("name", p["id"]), x, z))
    return out


def port_box(x, z, level=3):
    """The tiles of a level round a port: PORT_TILES a side, the port as near their middle as the grid lets it be."""
    size = PX * LEVELS[level]
    T = tiles_a_side(level)
    fx, fy = (x - WEST) / size, (NORTH - z) / size
    c0 = int(round(fx - PORT_TILES / 2.0)); r0 = int(round(fy - PORT_TILES / 2.0))
    return [(r, c) for r in range(r0, r0 + PORT_TILES) for c in range(c0, c0 + PORT_TILES) if 0 <= r < T and 0 <= c < T]


def level_tiles(level, ports):
    """Every tile a level should have (before the sea is taken out), and for the port level which port wants which."""
    T = tiles_a_side(level)
    if level < 3:
        return [(r, c) for r in range(T) for c in range(T)], {}
    want, by_port = set(), {}
    for pid, _name, x, z in ports:
        box = port_box(x, z, level)
        by_port[pid] = box
        want.update(box)
    return sorted(want), by_port


def touching(level, boxes3):
    """The tiles of a level that touch any of the given 10 m tiles."""
    s3, s = PX * LEVELS[3], PX * LEVELS[level]
    out = set()
    for r, c in boxes3:
        x0, x1, y0, y1 = c * s3, (c + 1) * s3, r * s3, (r + 1) * s3
        for rr in range(int(y0 // s), int(math.ceil(y1 / s))):
            for cc in range(int(x0 // s), int(math.ceil(x1 / s))):
                out.add((rr, cc))
    return out


def read_sea(level, out=None):
    p = os.path.join(out or OUT, "L%d" % level, "sea.txt")
    if not os.path.exists(p):
        return set()
    with open(p, "r") as f:
        return set(tuple(int(v) for v in line.split(",")) for line in f if "," in line)


def note_sea(level, tr, tc, out=None):
    d = os.path.join(out or OUT, "L%d" % level)
    os.makedirs(d, exist_ok=True)
    with open(os.path.join(d, "sea.txt"), "a") as f:
        f.write("%d,%d\n" % (tr, tc))


def write_index(ports, out=None, note=None):
    """pyramid.json: what the chart needs to choose a level and find a tile."""
    out = out or OUT
    levels = []
    total_bytes = total_tiles = 0
    for level in (0, 1, 2, 3):
        T = tiles_a_side(level)
        mpp = art.CELL if level == 0 else LEVELS[level]
        want, by_port = level_tiles(level, ports) if level else ([(r, c) for r in range(T) for c in range(T)], {})
        sea = read_sea(level, out) if level else set()
        have = [t for t in want if os.path.exists(tile_path(level, t[0], t[1], out))]
        size = sum(os.path.getsize(tile_path(level, r, c, out)) for r, c in have)
        pending = [t for t in want if t not in sea and t not in set(have)]
        total_bytes += size; total_tiles += len(have)
        e = {"level": level, "metresPerPixel": mpp, "cols": T, "rows": T, "tileMetres": PX * mpp, "pixelsAcross": T * PX,
             "tiles": "t_{row}_{col}.png" if level == 0 else "L%d/t_{row}_{col}.png" % level,
             "covers": "the whole window" if level < 3 else "the %d x %d tiles nearest each port (%.1f km a side)" % (PORT_TILES, PORT_TILES, PORT_TILES * PX * mpp / 1000.0),
             "wanted": len(want), "painted": len(have), "megabytes": round(size / 1e6, 1),
             "have": [[r, c] for r, c in have], "sea": [[r, c] for r, c in sorted(sea & set(want))], "pending": [[r, c] for r, c in pending]}
        if level == 3:
            e["ports"] = {pid: [[r, c] for r, c in box] for pid, box in sorted(by_port.items())}
        levels.append(e)
    meta = {
        "about": "The chart's picture as a pyramid: the same window at four sizes of pixel. Level 0 is make_map_art.py's 120 m map (map_art.json, t_<row>_<col>.png, "
                 "untouched); levels 1 to 3 are made by Bake/world/make_map_pyramid.py. Every level starts at the same north-west corner (west, north) and counts "
                 "tiles from there: tile (row, col) of a level covers x from west + col * tileMetres, z down from north - row * tileMetres. Row 0 is the northernmost, "
                 "col 0 the westernmost. A tile in 'sea' is all open water and has no file: show the next coarser level there. A tile in 'pending' is not baked yet. "
                 "Level 3 exists only round the ports: 'have' lists its tiles. Tiles of levels 1 to 3 are 8-bit indexed PNGs in the 40 colours of "
                 "Design/palette_byzantium.gpl (a redrawn tile may be any PNG of 4096 x 4096).",
        "frame": "the game's world frame: Lambert conformal, parallels 45N and 65N, centred 56N 5E; x east, z north, real metres (" + art.WORLD_PROJ + ")",
        "west": WEST, "north": NORTH, "spanMetres": SPAN, "tilePixels": PX,
        "levels": levels,
        "totalTiles": total_tiles, "totalMegabytes": round(total_bytes / 1e6, 1),
        "palette": "Kapman2/Design/palette_byzantium.gpl",
        "truth": {
            "shore": "the world bake at 30 m along every coast and sailed river (level 0 of data/baked/world), 60 m and 120 m behind it; at 10 m the shore is still the 30 m one",
            "landCover": "ESA WorldCover 2021: at 60 m and 30 m the commonest class of each pixel's 10 m cells, at 10 m the cells themselves; turned into the land of about 800 by make_map_art.py's rule (KK10's cleared share)",
            "netherlands": "the Vos reconstruction of c. 800, drawn for about 1:100,000: no finer than a few hundred metres at any level",
            "rivers": "true widths: the 800 map's inland water and the river network's recorded real widths in the North Sea core, WorldCover's water elsewhere; rivers narrower than a pixel and a half are a line. "
                      "Level 0 (120 m) still draws the bake's boat-scaled rivers, wider than life.",
            "inlandHeights": "30 m only along coasts and rivers; elsewhere filled in from 60 m and 120 m (they only decide marsh, moor, rock and snow)",
        },
        "made": "Bake/world/make_map_pyramid.py, with the painting rules of Bake/world/make_map_art.py",
        "credits": CREDITS,
    }
    if note:
        meta["note"] = note
    tmp = os.path.join(out, "pyramid.json.part")
    with io.open(tmp, "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=1)
    os.replace(tmp, os.path.join(out, "pyramid.json"))
    return meta


# ---------------------------------------------------------------------------- looking at it
def crop(name, lon, lat, w=1200, h=800, out=None):
    """A picture of each level round a point, one art pixel a screen pixel (missing tiles grey, sea tiles the sea's
    colour), in Captures/map/pyramid."""
    os.makedirs(CAPTURES, exist_ok=True)
    x, z, _ = osr.CoordinateTransformation(art._wgs, art._world).TransformPoint(lon, lat)
    written = []
    for level in (0, 1, 2, 3):
        mpp = art.CELL if level == 0 else LEVELS[level]
        sea = read_sea(level, out) if level else set()
        px, py = int((x - WEST) / mpp) - w // 2, int((NORTH - z) / mpp) - h // 2
        im = Image.new("RGB", (w, h), (90, 90, 90))
        found = False
        for tr in range(py // PX, (py + h - 1) // PX + 1):
            for tc in range(px // PX, (px + w - 1) // PX + 1):
                p = tile_path(level, tr, tc, out)
                at = (tc * PX - px, tr * PX - py)
                if os.path.exists(p):
                    im.paste(Image.open(p).convert("RGB"), at); found = True
                elif (tr, tc) in sea:
                    im.paste(Image.new("RGB", (PX, PX), art.rgb("deep sea")), at)
        if found:
            p = os.path.join(CAPTURES, "%s_L%d_%dm.png" % (name, level, int(mpp)))
            im.save(p)
            written.append(p)
    return written


# ---------------------------------------------------------------------------- the run
def main():
    ap = argparse.ArgumentParser(description="The chart's picture at finer levels (see the comment at the top of this file).")
    ap.add_argument("--levels", nargs="*", type=int, default=[1, 2, 3], help="which levels (1: 60 m, 2: 30 m, 3: 10 m round the ports)")
    ap.add_argument("--ports", nargs="*", default=None, help="only the tiles round these ports (ids of ports_793.json)")
    ap.add_argument("--tiles", nargs="*", default=None, help="only these tiles, as level:row,col")
    ap.add_argument("--jobs", type=int, default=6, help="tiles painted at once at 60 m and 30 m")
    ap.add_argument("--jobs-fine", type=int, default=3, help="tiles painted at once at 10 m")
    ap.add_argument("--redo", action="store_true", help="paint tiles again even if they are there")
    ap.add_argument("--rgb", action="store_true", help="write 24-bit PNGs, not indexed ones")
    ap.add_argument("--out", default=OUT, help="the folder to write (default: Kapman2/MapArt/world)")
    ap.add_argument("--plan", action="store_true", help="only say what there is to do")
    ap.add_argument("--index", action="store_true", help="only write pyramid.json from what is on disk")
    ap.add_argument("--fingerprint", action="store_true", help="print the fingerprint of make_map_art.paint (for REVIEWED)")
    ap.add_argument("--crop", nargs=3, metavar=("NAME", "LON", "LAT"), default=None, help="pictures of every level round a point, in Captures/map/pyramid")
    ap.add_argument("--crop-size", nargs=2, type=int, default=[1200, 800])
    args = ap.parse_args()
    out = os.path.abspath(args.out)
    if args.fingerprint:
        print(fingerprint())
        return
    if args.crop:
        for p in crop(args.crop[0], float(args.crop[1]), float(args.crop[2]), args.crop_size[0], args.crop_size[1], out):
            print(p)
        return
    ports = load_ports()
    if args.index:
        meta = write_index(ports, out)
        print("pyramid.json: %d tiles, %.1f MB" % (meta["totalTiles"], meta["totalMegabytes"]))
        return
    if REVIEWED and fingerprint() != REVIEWED:
        print("WARNING: make_map_art.paint has changed since make_map_pyramid.py was read against it (fingerprint %s, reviewed %s). "
              "Check GROUND_CELLS and GROUND_WINDOWS still name every length it keeps in cells." % (fingerprint(), REVIEWED), flush=True)
    only = None
    if args.tiles is not None:
        only = set()
        for t in args.tiles:
            lv, rc = t.split(":")
            r, c = rc.split(",")
            only.add((int(lv), int(r), int(c)))
    near = None
    if args.ports is not None:
        ids = dict((p[0], p) for p in ports)
        boxes = set()
        for pid in args.ports:
            assert pid in ids, "no port %s in ports_793.json" % pid
            boxes.update(port_box(ids[pid][2], ids[pid][3]))
        near = dict((lv, touching(lv, boxes)) for lv in (1, 2, 3))
    os.makedirs(out, exist_ok=True)
    plan = []
    for level in sorted(args.levels):
        assert level in LEVELS, "no level %d (1: 60 m, 2: 30 m, 3: 10 m)" % level
        want, _by = level_tiles(level, ports)
        sea = read_sea(level, out)
        todo = []
        for r, c in want:
            if only is not None and (level, r, c) not in only:
                continue
            if near is not None and (r, c) not in near[level]:
                continue
            if not args.redo and ((r, c) in sea or os.path.exists(tile_path(level, r, c, out))):
                continue
            todo.append((level, r, c, out, args.rgb))
        # nearest the middle of the sailing world first (Dorestad): what the player sees first is done first
        mid = ((ports[0][2] - WEST) / (PX * LEVELS[level]), (NORTH - ports[0][3]) / (PX * LEVELS[level]))
        todo.sort(key=lambda j: (j[2] + 0.5 - mid[0]) ** 2 + (j[1] + 0.5 - mid[1]) ** 2)
        plan.append((level, len(want), len(sea), todo))
        print("level %d (%g m): %d tiles wanted, %d known sea, %d on disk, %d to do" %
              (level, LEVELS[level], len(want), len(sea), sum(os.path.exists(tile_path(level, r, c, out)) for r, c in want), len(todo)), flush=True)
    if args.plan:
        return
    import multiprocessing as mp
    t_start = time.time()
    done = failed = 0
    grand = sum(len(p[3]) for p in plan)
    for level, _n_want, _n_sea, todo in plan:
        if not todo:
            continue
        jobs = max(1, min(args.jobs_fine if level == 3 else args.jobs, len(todo)))
        t_level = time.time()
        n_tile = n_sea = 0
        size = 0
        print("level %d: %d tiles, %d at once, started %s" % (level, len(todo), jobs, time.strftime("%H:%M:%S")), flush=True)
        with mp.Pool(processes=jobs, maxtasksperchild=12) as pool:
            for res in pool.imap_unordered(_job, todo, chunksize=1):
                done += 1
                if res["what"] == "sea":
                    note_sea(level, res["row"], res["col"], out)
                    n_sea += 1
                elif res["what"] == "failed":
                    failed += 1
                    print("FAILED level %d tile %d,%d: %s" % (level, res["row"], res["col"], res["error"]), flush=True)
                    continue
                else:
                    n_tile += 1
                    size += res["bytes"]
                k = n_tile + n_sea
                spent = time.time() - t_level
                left = (len(todo) - k) * spent / max(k, 1)
                print("L%d t_%d_%d  %-4s %5.0f s  %6.0f KB  peak %4.0f MB   %d/%d of the level, %d/%d in all; the level ends about %s" %
                      (level, res["row"], res["col"], res["what"], res["seconds"], res["bytes"] / 1024.0, res["peak"], k, len(todo), done, grand,
                       time.strftime("%H:%M", time.localtime(time.time() + left))), flush=True)
                if k % 25 == 0:
                    write_index(ports, out, note="bake running since %s" % time.strftime("%Y-%m-%d %H:%M", time.localtime(t_start)))
        print("level %d done: %d tiles (%.1f MB), %d sea, %.1f minutes" % (level, n_tile, size / 1e6, n_sea, (time.time() - t_level) / 60.0), flush=True)
        write_index(ports, out)
    meta = write_index(ports, out)
    print("done: %d tiles on disk, %.1f MB, %d failed, %.1f minutes; pyramid.json written" %
          (meta["totalTiles"], meta["totalMegabytes"], failed, (time.time() - t_start) / 60.0), flush=True)


if __name__ == "__main__":
    main()
