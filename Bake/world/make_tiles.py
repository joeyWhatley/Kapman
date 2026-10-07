# THE WORLD BAKER, DATA SIDE (4 Oct 2026). Cuts the open world (Design/DECISIONS.md, 4 Oct pm; open_world_isometric.md)
# into square tiles of game metres for a lat/lon box: per 2 m cell a signed distance to the waterline and a kind (deep sea,
# shallow sea, river, waterfront band, inland under fog). Lengths shrink by the scale; river widths stay true (capped).
#
#   The coast: Natural Earth 10 m land (public domain), replaced in the Netherlands by the Vos c. 800 map
#     (buitenwater + getijdenzone = sea, binnenwater = inland water, buitenland = "not covered, ask Natural Earth").
#   The rivers: true-width channels along their courses shrunk by the scale; courses traced through HydroRIVERS
#     (and the Vos 800 channel lines for the c. 800 Rhine branches HydroRIVERS lacks: the Kromme Rijn, the Oude Rijn to
#     Katwijk, the Vecht); widths from the water records (Assets/Kapman/Data/Water/water_793.json), capped at 300 m;
#     bends eased so no bend's radius is under about three widths; a river stops where the sea around it is wider than it.
#   Deep or shallow: EMODnet DTM 2024 depth (shallower than --shallow m = shallow); the Vos tidal flats always shallow.
#
# Run (QGIS Python):  Bake\run.bat world\make_tiles.py [--scale 40] [--box -0.6 51.0 7.3 53.7] [--tile 1024]
#   -> G:\kapman_unity\data\world\<scale>\index.json, rivers.json, tiles\<x>_<y>\{water_sd.f32, kind.u8, meta.json}
# The README beside this file has the file format, the arguments and the credit lines.
import argparse
import glob
import json
import math
import os
import shutil
import time

import numpy as np
import shapely
from osgeo import gdal, ogr, osr
from scipy import ndimage
from scipy.sparse import coo_matrix
from scipy.sparse.csgraph import connected_components, dijkstra
from scipy.spatial import cKDTree
from shapely.geometry import Point, Polygon

gdal.UseExceptions()
ogr.UseExceptions()
osr.UseExceptions()

ROOT = r"G:\kapman_unity\Kapman2"
RAW = r"G:\kapman_unity\data\raw"
OUT_ROOT = r"G:\kapman_unity\data\world"
WATER_JSON = os.path.join(ROOT, "Assets", "Kapman", "Data", "Water", "water_793.json")


def vsizip(path, inner):
    return "/vsizip/" + path.replace("\\", "/") + "/" + inner


NE = vsizip(RAW + r"\vectors\naturalearth\10m_physical.zip", "")
VOS_POLY = (vsizip(RAW + r"\history\vos_paleogeography\paleogeografischekrt2021_GIS_3van3_gdb_lyr_sld.zip",
                   "paleogeografischekrt2021_3van3.gdb"), "paleogeografie800nc")
VOS_LINES = (vsizip(RAW + r"\history\vos_paleogeography\paleogeografischekrt2021_GIS_1van3_gdb.zip",
                    "paleogeografischekrt2021_1van3.gdb"), "paleogeografie_waterlopen", "800nc")
HYDRO = vsizip(RAW + r"\hydro\hydrorivers\HydroRIVERS_v10_eu_shp.zip", "HydroRIVERS_v10_eu_shp/HydroRIVERS_v10_eu.shp")
EMOD_GLOB = RAW + r"\bathy\emodnet\*_2024\*.tif"
# The same frame as make_world_map.py: Lambert conformal over the North Atlantic; game metres = projected metres / scale.
PROJ = "+proj=lcc +lat_1=50 +lat_2=68 +lat_0=58 +lon_0=-5 +datum=WGS84 +units=m +no_defs"

DEEP, SHALLOW, RIVER, BAND, INLAND = 0, 1, 2, 3, 4
KIND_NAMES = ["deep sea", "shallow sea", "river", "band", "inland"]
VOS_SEA = {"buitenwater", "getijdenzone"}
VOS_TIDAL = {"getijdenzone"}
VOS_INLAND_WATER = {"binnenwater"}
VOS_OUTSIDE = {"buitenland"}

# ---- The rivers ------------------------------------------------------------------------------------------------------
# Upstream to downstream. Each point: (lat, lon, width, label[, leg]). width = a water-record id (its river.widthM, capped),
# a number (an estimate: no record), or None (interpolated). leg = how to reach the NEXT point: the river's net
# ("hydro" = HydroRIVERS, "vos" = the Vos 800 channel lines) or "line" (straight, then eased).
# The delta branches and side rivers: capped narrower (--branch-width, 150 m; the user, 4 Oct: at 300 m the Lek, Waal
# and Nederrijn ran together into one broad water round Dorestad). The trunks and estuaries keep --max-width.
BRANCHES = {"nederrijn", "lek", "waal", "ijssel", "kromme_rijn", "oude_rijn", "vecht", "maas"}

RIVERS = [
    dict(id="rhine", name="The Rhine", net="hydro", pts=[
        (50.938, 6.962, "rhine_colonia", "Colonia (Cologne)"),
        (51.435, 6.762, "rhine_lower_diusburg", "Diusburg (Duisburg)"),
        (51.829, 6.248, None, "Emmerich"),
        (51.871, 6.046, None, "The Pannerdensche Kop: the Waal and the Nederrijn part")]),
    dict(id="nederrijn", name="The Nederrijn to Dorestad", net="hydro", pts=[
        (51.871, 6.046, "lek_dorestad", "The Pannerdensche Kop"),
        (51.956, 5.958, None, "The IJsselkop: the IJssel parts"),
        (51.976, 5.904, None, "Arnhem"),
        (51.972, 5.343, "lek_dorestad", "Dorestad: the Kromme Rijn and the Lek part")]),
    dict(id="kromme_rijn", name="The Kromme Rijn", net="vos", pts=[
        (51.972, 5.343, "kromme_rijn_dorestad", "Dorestad"),
        (52.091, 5.121, "old_rhine_traiectum", "Traiectum (Utrecht): the Vecht parts")]),
    dict(id="oude_rijn", name="The Old Rhine to Katwijk", net="vos", pts=[
        (52.091, 5.121, "old_rhine_traiectum", "Traiectum (Utrecht)"),
        (52.086, 4.884, None, "Woerden"),
        (52.082, 4.750, None, "Bodegraven"),
        (52.129, 4.660, None, "Alphen"),
        (52.159, 4.493, None, "Leiden"),
        (52.204, 4.398, None, "Katwijk: the Rhine mouth", "line"),
        (52.215, 4.300, None, "The sea off Katwijk")]),
    dict(id="vecht", name="The Vecht to Almere", net="vos", pts=[
        (52.091, 5.121, 70, "Traiectum (Utrecht)"),
        (52.332, 5.069, None, "Muiden", "line"),
        (52.375, 5.100, None, "Almere")]),
    dict(id="lek", name="The Lek", net="hydro", pts=[
        (51.972, 5.343, "lek_dorestad", "Dorestad"),
        (51.946, 4.853, None, "Schoonhoven"),
        (51.884, 4.638, None, "Kinderdijk: the Lek meets the Merwede")]),
    dict(id="waal", name="The Waal", net="hydro", pts=[
        (51.871, 6.046, "waal_noviomagus", "The Pannerdensche Kop"),
        (51.853, 5.866, "waal_noviomagus", "Noviomagus (Nijmegen)"),
        (51.884, 5.427, None, "Tiel"),
        (51.830, 4.975, None, "Gorinchem"),
        (51.813, 4.690, None, "Dordrecht"),
        (51.884, 4.638, None, "Kinderdijk")]),
    dict(id="maas", name="The Maas (Meuse)", net="hydro", pts=[
        (50.849, 5.696, "meuse_traiectum_mosae", "Traiectum ad Mosam (Maastricht)"),
        (51.195, 5.980, None, "Roermond"),
        (51.370, 6.175, None, "Venlo"),
        (51.762, 5.736, None, "Grave"),
        (51.737, 5.140, None, "Heusden", "line"),
        (51.815, 4.995, None, "Woudrichem: the Maas meets the Waal")]),
    dict(id="maasmond", name="The Maas mouth", net="hydro", pts=[
        (51.884, 4.638, "maas_mouth_witla", "Kinderdijk"),
        (51.905, 4.480, None, "Rotterdam"),
        (51.898, 4.340, None, "Vlaardingen", "line"),
        (51.905, 4.165, None, "Brielle", "line"),
        (51.880, 4.000, None, "The sea off the Maas mouth")]),
    dict(id="ijssel", name="The IJssel", net="hydro", pts=[
        (51.956, 5.958, "ijssel_daventre", "The IJsselkop"),
        (52.013, 6.137, None, "Doesburg"),
        (52.141, 6.193, None, "Zutphen"),
        (52.252, 6.158, "ijssel_daventre", "Daventre (Deventer)"),
        (52.556, 5.912, None, "Kampen", "line"),
        (52.585, 5.780, None, "The IJssel mouth", "line"),
        (52.550, 5.560, None, "Almere")]),
    dict(id="thames", name="The Thames", net="hydro", pts=[
        (51.486, -0.607, 60, "Windsor (the non-tidal Thames, estimate)"),
        (51.411, -0.309, None, "Kingston"),
        (51.509, -0.118, "thames_lundenwic", "Lundenwic (the Strand)"),
        (51.486, -0.006, None, "Greenwich"),
        (51.447, 0.370, None, "Gravesend"),
        (51.490, 0.700, None, "Canvey", "line"),
        (51.500, 1.000, None, "The outer estuary")]),
    dict(id="scheldt", name="The Scheldt", net="hydro", pts=[
        (50.606, 3.389, "scheldt_upper_tornacum_valentianas", "Tornacum (Tournai)"),
        (50.846, 3.607, None, "Oudenaarde"),
        (51.054, 3.724, "scheldt_ganda", "Ganda (Ghent)"),
        (51.028, 4.101, None, "Dendermonde"),
        (51.222, 4.397, "scheldt_estuary_andoverpis", "Andoverpis (Antwerp)"),
        (51.360, 4.240, None, "Bath", "line"),
        (51.420, 4.000, None, "The Scheldt mouth")]),
    dict(id="seine", name="The Seine", net="hydro", pts=[
        (48.857, 2.352, 150, "Paris (no water record: estimate)"),
        (49.437, 1.100, None, "Rouen"),
        (49.430, 0.250, None, "Honfleur", "line"),
        (49.470, 0.000, None, "The Seine bay")]),
    # Not in the brief's list but in open_world_isometric.md's c. 800 coast: the Wantsum open, Thanet an island.
    dict(id="wantsum", name="The Wantsum channel", net="line", pts=[
        (51.395, 1.200, 100, "Reculver (estimate: no width record; 300 would swallow Thanet at 1:40)"),
        (51.335, 1.240, None, "Sarre"),
        (51.293, 1.333, None, "Richborough"),
        (51.310, 1.390, None, "Pegwell Bay")]),
]


# ---- Projection ------------------------------------------------------------------------------------------------------
class Frame:
    """Lon/lat <-> real projected metres <-> game metres. real_to_game is the one place a separate sea scale would go:
    today it divides by one scale; a sea scale would make it a warp (see the README)."""

    def __init__(self, scale, sea_scale):
        self.scale, self.sea_scale = float(scale), float(sea_scale)
        wgs = osr.SpatialReference()
        wgs.ImportFromEPSG(4326)
        wgs.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
        self.srs = osr.SpatialReference()
        self.srs.ImportFromProj4(PROJ)
        self.wgs = wgs
        self._fwd = osr.CoordinateTransformation(wgs, self.srs)
        self._inv = osr.CoordinateTransformation(self.srs, wgs)
        self._cts = {}

    def _tx(self, ct, c):
        c = np.asarray(c, dtype=float).reshape(-1, 2)
        if len(c) == 0:
            return c
        return np.asarray(ct.TransformPoints(c.tolist()), dtype=float)[:, :2]

    def ll_to_real(self, c):
        return self._tx(self._fwd, c)

    def real_to_ll(self, c):
        return self._tx(self._inv, c)

    def srs_to_real(self, srs, c):
        key = srs.ExportToWkt()
        if key not in self._cts:
            s = srs.Clone()
            s.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
            self._cts[key] = osr.CoordinateTransformation(s, self.srs)
        return self._tx(self._cts[key], c)

    def real_to_game(self, c):
        return np.asarray(c, dtype=float) / self.scale

    def game_to_real(self, c):
        return np.asarray(c, dtype=float) * self.scale

    def ll_to_game(self, c):
        return self.real_to_game(self.ll_to_real(c))

    def game_to_ll(self, c):
        return self.real_to_ll(self.game_to_real(c))


# ---- Geometry helpers ------------------------------------------------------------------------------------------------
def ogr_to_shapely(g):
    if g is None:
        return None
    if g.HasCurveGeometry():
        g = g.GetLinearGeometry()
    g = g.Clone()
    g.FlattenTo2D()
    return shapely.from_wkb(bytes(g.ExportToWkb()))


def chaikin_ring(c, n):
    c = c[:-1] if len(c) > 1 and np.allclose(c[0], c[-1]) else c
    for _ in range(n):
        nxt = np.roll(c, -1, axis=0)
        out = np.empty((2 * len(c), 2))
        out[0::2] = 0.75 * c + 0.25 * nxt
        out[1::2] = 0.25 * c + 0.75 * nxt
        c = out
    return np.vstack([c, c[:1]])


def smooth_polygons(geom, n):
    """Chaikin corner cutting on every ring (after segmentize, so a cut is never more than a few metres)."""
    if n <= 0 or geom.is_empty:
        return geom
    out = []
    for p in shapely.get_parts(geom):
        if p.geom_type != "Polygon" or p.is_empty:
            continue
        ext = chaikin_ring(np.asarray(p.exterior.coords)[:, :2], n)
        holes = [chaikin_ring(np.asarray(h.coords)[:, :2], n) for h in p.interiors if len(h.coords) >= 4]
        if len(ext) >= 4:
            out.append(Polygon(ext, holes))
    return shapely.make_valid(shapely.MultiPolygon(out)) if out else shapely.MultiPolygon()


def polys_only(geom):
    return [p for p in shapely.get_parts(geom) if p.geom_type == "Polygon" and not p.is_empty]


def prep_polys(geom, frame_fn, simplify, segment, smooth):
    """Lon/lat (or source CRS) geometry -> list of game-space polygons, simplified and optionally smoothed."""
    g = shapely.transform(geom, frame_fn)
    g = shapely.make_valid(g)
    if simplify > 0:
        g = shapely.simplify(g, simplify, preserve_topology=True)
    if smooth > 0:
        g = smooth_polygons(shapely.segmentize(g, segment), smooth)
    return polys_only(g)


_MEMV = None


def mem_vector_driver():
    global _MEMV
    if _MEMV is None:
        _MEMV = ogr.GetDriverByName("MEM") if gdal.GetDriverByName("MEM").GetMetadataItem("DCAP_VECTOR") else ogr.GetDriverByName("Memory")
    return _MEMV


def rasterize(geoms, x0, y0, nx, ny, res):
    """Burn polygons into a bool grid whose row 0 is the SOUTH edge (y0) and column 0 the west edge (x0)."""
    ds = gdal.GetDriverByName("MEM").Create("", nx, ny, 1, gdal.GDT_Byte)
    ds.SetGeoTransform((x0, res, 0.0, y0 + ny * res, 0.0, -res))
    geoms = [g for g in geoms if g is not None and not g.is_empty]
    if geoms:
        vds = mem_vector_driver().CreateDataSource("r")
        lyr = vds.CreateLayer("g", geom_type=ogr.wkbUnknown)
        defn = lyr.GetLayerDefn()
        for g in geoms:
            f = ogr.Feature(defn)
            f.SetGeometry(ogr.CreateGeometryFromWkb(shapely.to_wkb(g)))
            lyr.CreateFeature(f)
        gdal.RasterizeLayer(ds, [1], lyr, burn_values=[1])
    a = ds.GetRasterBand(1).ReadAsArray()
    return a[::-1].astype(bool)


class Group:
    """A set of game-space polygons with a spatial index; clip() returns the pieces inside a rectangle."""

    def __init__(self, polys):
        self.polys = list(polys)
        self.tree = shapely.STRtree(self.polys) if self.polys else None

    def clip(self, x0, y0, x1, y1):
        if self.tree is None:
            return []
        idx = self.tree.query(shapely.box(x0, y0, x1, y1))
        out = []
        for i in idx:
            g = shapely.clip_by_rect(self.polys[i], x0, y0, x1, y1)
            if not g.is_empty:
                out.append(g)
        return out


# ---- Sources ---------------------------------------------------------------------------------------------------------
def load_natural_earth(frame, llbox, args):
    lon0, lat0, lon1, lat1 = llbox
    geoms = []
    for name in ("ne_10m_land.shp", "ne_10m_minor_islands.shp"):
        ds = ogr.Open(NE + name)
        lyr = ds.GetLayer(0)
        lyr.SetSpatialFilterRect(lon0, lat0, lon1, lat1)
        for f in lyr:
            g = ogr_to_shapely(f.GetGeometryRef())
            if g is None:
                continue
            g = shapely.clip_by_rect(g, lon0, lat0, lon1, lat1)
            if not g.is_empty:
                geoms.append(g)
        ds = None
    land = shapely.union_all(geoms) if geoms else shapely.MultiPolygon()
    return prep_polys(land, frame.ll_to_game, args.simplify, args.segment, args.smooth)


def load_vos(frame, llbox, args):
    ds = ogr.Open(VOS_POLY[0])
    lyr = ds.GetLayerByName(VOS_POLY[1])
    srs = lyr.GetSpatialRef()
    fn = lambda c: frame.real_to_game(frame.srs_to_real(srs, c))
    groups = {"domain": [], "sea": [], "inland": [], "tidal": []}
    classes = {}
    dropped = []
    for f in lyr:
        cls = f.GetField("legenda")
        g = ogr_to_shapely(f.GetGeometryRef())
        if g is None or g.is_empty:
            continue
        classes[cls] = classes.get(cls, 0) + 1
        if cls in VOS_OUTSIDE:
            continue
        polys = prep_polys(g, fn, args.simplify, 0, 0)       # no smoothing: the classes must keep their shared edges
        groups["domain"] += polys
        if cls in VOS_SEA:
            groups["sea"] += polys
        if cls in VOS_TIDAL:
            groups["tidal"] += polys
        if cls in VOS_INLAND_WATER:
            # The river channels (narrow on average: the Rhine-Waal-Lek-IJssel-Maas network is one 240 km2 piece
            # 260 m wide on average) shrink to slivers beside the true-width rivers that stand in for them: dropped.
            # Lakes and broad pools stay.
            mean_w = 2.0 * g.area / max(g.length, 1.0)          # source CRS metres (RD New)
            if mean_w < args.vos_channel:
                dropped.append(round(g.area / 1e6, 1))
                continue
            groups["inland"] += polys
    ds = None
    classes["binnenwater dropped as channels (km2)"] = dropped
    # Sea pieces cut off from the sea (a few tidal-flat polygons far inland, e.g. in South Limburg, under 1 km2) become
    # land: keep the parts of the sea classes of 2 km2 or more, or within 2 km (real) of a big one.
    s2 = frame.scale ** 2
    parts = polys_only(shapely.union_all(groups["sea"])) if groups["sea"] else []
    big = [p for p in parts if p.area * s2 >= 50e6]
    big_u = shapely.union_all(big) if big else None
    keep = [p for p in parts if p.area * s2 >= 2e6 or (big_u is not None and p.distance(big_u) * frame.scale <= 2000.0)]
    stray = [p for p in parts if not any(p is k for k in keep)]
    where = frame.game_to_ll([(p.centroid.x, p.centroid.y) for p in stray]) if stray else []
    classes["sea pieces dropped as stray (km2 at lon, lat)"] = [[round(p.area * s2 / 1e6, 2), round(float(w[0]), 3), round(float(w[1]), 3)]
                                                              for p, w in zip(stray, where)]
    groups["sea"] = keep
    return groups, classes


def load_vos_lines(frame, llbox):
    ds = ogr.Open(VOS_LINES[0])
    lyr = ds.GetLayerByName(VOS_LINES[1])
    srs = lyr.GetSpatialRef()
    lines = []
    for f in lyr:
        if f.GetField("jaar") != VOS_LINES[2]:
            continue
        g = ogr_to_shapely(f.GetGeometryRef())
        for part in shapely.get_parts(g):
            c = np.asarray(part.coords)[:, :2]
            if len(c) >= 2:
                lines.append(frame.srs_to_real(srs, c))
    ds = None
    return lines, [1.0] * len(lines), [0.0] * len(lines)


def load_hydro(frame, llbox):
    lon0, lat0, lon1, lat1 = llbox
    ds = ogr.Open(HYDRO)
    lyr = ds.GetLayer(0)
    lyr.SetSpatialFilterRect(lon0, lat0, lon1, lat1)
    lyr.SetAttributeFilter("UPLAND_SKM >= 50")
    lines, mults, ups = [], [], []
    for f in lyr:
        g = f.GetGeometryRef()
        c = np.asarray(g.GetPoints(), dtype=float)[:, :2]
        if len(c) < 2:
            continue
        up = f.GetField("UPLAND_SKM") or 0.0
        lines.append(frame.ll_to_real(c))
        ups.append(up)
        mults.append(1.0 if up >= 5000 else 1.5 if up >= 1000 else 3.0 if up >= 200 else 6.0)   # prefer the big rivers
    ds = None
    return lines, mults, ups


class Net:
    """An undirected graph of river lines in real metres; endpoints left dangling are tied to the nearest other line."""

    def __init__(self, lines, mults, ups, snap_m):
        n_per = [len(l) for l in lines]
        allp = np.vstack(lines)
        keys = np.round(allp).astype(np.int64)
        uniq, inv = np.unique(keys, axis=0, return_inverse=True)
        inv = inv.reshape(-1)
        self.xy = uniq.astype(float)
        n = len(self.xy)
        self.up = np.zeros(n)
        a, b, w = [], [], []
        off = 0
        for li, l in enumerate(lines):
            ids = inv[off:off + n_per[li]]
            off += n_per[li]
            seg = np.hypot(*np.diff(l, axis=0).T)
            a.append(ids[:-1])
            b.append(ids[1:])
            w.append(seg * mults[li])
            np.maximum.at(self.up, ids, ups[li])
        a, b, w = np.concatenate(a), np.concatenate(b), np.concatenate(w)
        deg = np.bincount(np.concatenate([a, b]), minlength=n)
        if snap_m > 0:
            # Tie each dangling end to the nearest node of ANOTHER connected piece (a gap in the source lines).
            _, comp = connected_components(coo_matrix((np.ones(len(a)), (a, b)), shape=(n, n)), directed=False)
            tree = cKDTree(self.xy)
            ends = np.unique(np.concatenate([[inv[o], inv[o + k - 1]] for o, k in zip(np.cumsum([0] + n_per[:-1]), n_per)]))
            ea, eb, ew = [], [], []
            for e in ends:
                if deg[e] > 1:
                    continue
                d, j = tree.query(self.xy[e], k=64, distance_upper_bound=snap_m)
                for dd, jj in zip(np.atleast_1d(d), np.atleast_1d(j)):
                    if np.isfinite(dd) and jj < n and comp[jj] != comp[e]:
                        ea.append(e)
                        eb.append(jj)
                        ew.append(max(dd, 1.0) * 3.0)          # a tie costs more than the river itself
                        break
            a, b, w = np.concatenate([a, ea]).astype(int), np.concatenate([b, eb]).astype(int), np.concatenate([w, ew])
        keep = a != b
        a, b, w = a[keep], b[keep], np.maximum(w[keep], 1e-3)
        lo, hi = np.minimum(a, b), np.maximum(a, b)
        order = np.lexsort((w, hi, lo))
        lo, hi, w = lo[order], hi[order], w[order]
        first = np.ones(len(lo), bool)
        first[1:] = (lo[1:] != lo[:-1]) | (hi[1:] != hi[:-1])
        self.G = coo_matrix((w[first], (lo[first], hi[first])), shape=(n, n)).tocsr()
        self.tree = cKDTree(self.xy)

    def snap(self, p):
        """The node nearest to p, preferring the biggest river among those within 500 m of the nearest."""
        d, j = self.tree.query(p, k=40)
        ok = d <= d[0] + 500.0
        cand = j[ok]
        best = cand[np.argmax(self.up[cand])] if self.up[cand].max() > 0 else cand[0]
        return int(best), float(np.hypot(*(self.xy[best] - p)))

    def path(self, s, t):
        if s == t:
            return [s]
        dist, pred = dijkstra(self.G, directed=False, indices=s, return_predecessors=True)
        if not np.isfinite(dist[t]):
            return None
        out = [t]
        while out[-1] != s:
            out.append(pred[out[-1]])
        return out[::-1]


def load_records():
    recs = json.load(open(WATER_JSON, encoding="utf-8"))["records"]
    return {r["id"]: r for r in recs}


def station_width(w, records, cap):
    """-> (width capped, source text)."""
    if w is None:
        return None, None
    if isinstance(w, str):
        r = records.get(w)
        wm = ((r or {}).get("river") or {}).get("widthM")
        if wm is None:
            raise SystemExit("water record %s has no river.widthM" % w)
        return min(float(wm), cap), "record %s (%s m)" % (w, wm)
    return min(float(w), cap), "estimate %s m (no water record)" % w


# ---- River courses ---------------------------------------------------------------------------------------------------
def resample(P, W, ds):
    seg = np.hypot(*np.diff(P, axis=0).T)
    s = np.concatenate([[0.0], np.cumsum(seg)])
    n = max(2, int(math.ceil(s[-1] / ds)) + 1)
    t = np.linspace(0.0, s[-1], n)
    return np.column_stack([np.interp(t, s, P[:, 0]), np.interp(t, s, P[:, 1])]), np.interp(t, s, W)


def circumradius(A, B, C):
    a = np.hypot(*(B - C).T)
    b = np.hypot(*(A - C).T)
    c = np.hypot(*(A - B).T)
    cross = np.abs((B[:, 0] - A[:, 0]) * (C[:, 1] - A[:, 1]) - (B[:, 1] - A[:, 1]) * (C[:, 0] - A[:, 0]))
    with np.errstate(divide="ignore", invalid="ignore"):
        R = a * b * c / (2.0 * cross)
    R[~np.isfinite(R)] = 1e12
    return R


def ease_bends(P, W, factor, pins=(), keep=0.25, max_iter=30000):
    """Laplacian smoothing, only where a bend is tighter than factor x width, until none is. The ends stay put and the
    course must keep passing within keep x width of every pin (the waypoints: the ports stay on the water), so the
    river cannot drift off its course. Where the real course turns too sharply near a pin (the Scheldt at Antwerp) the
    bend stays tighter than asked: the loop stops when it stops improving and reports the tightest bend and the share
    of the length that is still tighter than factor x width."""
    ds = float(np.clip(W.min() / 4.0, 2.0, 40.0))
    pins = np.asarray(pins, dtype=float).reshape(-1, 2)

    def hold(P, W):
        fixed = np.zeros(len(P), bool)
        fixed[0] = fixed[-1] = True
        for q in pins:
            d = np.hypot(P[:, 0] - q[0], P[:, 1] - q[1])
            i = int(np.argmin(d))
            tol = keep * W[i]
            if d[i] > tol:
                P[i] = q + (P[i] - q) * (tol / d[i])
                fixed[i] = True
        return fixed

    P, W = resample(P, W, ds)
    fixed = hold(P, W)
    it, best, best_it = 0, None, 0
    for it in range(max_iter):
        if it % 20 == 0 and it:
            P, W = resample(P, W, ds)
            fixed = hold(P, W)
        if len(P) < 3:
            break
        R = circumradius(P[:-2], P[1:-1], P[2:])
        bad = R < factor * W[1:-1]
        n_bad = int(bad.sum())
        if n_bad == 0:
            break
        if best is None or n_bad < best:
            best, best_it = n_bad, it
        elif it - best_it > 2000:
            break
        m = ndimage.binary_dilation(bad, iterations=3) & ~fixed[1:-1]
        lap = 0.5 * (P[:-2] + P[2:]) - P[1:-1]
        inner = P[1:-1]
        inner[m] += 0.5 * lap[m]
    # A short finish without the pins irons out single-vertex kinks the pins left (a few metres of drift at most).
    P, W = resample(P, W, ds)
    for _ in range(300):
        R = circumradius(P[:-2], P[1:-1], P[2:]) if len(P) >= 3 else np.array([1e12])
        bad = R < 1.0 * W[1:-1]           # under one width the ribbon folds over itself
        if not bad.any():
            break
        m = ndimage.binary_dilation(bad, iterations=2)
        lap = 0.5 * (P[:-2] + P[2:]) - P[1:-1]
        inner = P[1:-1]
        inner[m] += 0.5 * lap[m]
    P, W = resample(P, W, ds)
    if len(P) >= 3:
        ratio = circumradius(P[:-2], P[1:-1], P[2:]) / W[1:-1]
        worst, tight = float(ratio.min()), float((ratio < factor).mean())
    else:
        worst, tight = 1e9, 0.0
    return P, W, ds, it, worst, tight


def trace_river(rdef, nets, frame, records, args, log):
    pts = rdef["pts"]
    wp_real = frame.ll_to_real([(p[1], p[0]) for p in pts])
    seq = [wp_real[0]]
    station_idx = [0]
    notes = []
    for i in range(len(pts) - 1):
        leg = pts[i][4] if len(pts[i]) > 4 else rdef["net"]
        if leg == "line" or leg not in nets:
            seq.append(wp_real[i + 1])
        else:
            net = nets[leg]
            s, ds_ = net.snap(wp_real[i])
            t, dt_ = net.snap(wp_real[i + 1])
            path = net.path(s, t)
            straight = float(np.hypot(*(wp_real[i + 1] - wp_real[i])))
            plen = float(np.hypot(*np.diff(net.xy[path], axis=0).T).sum()) if path is not None and len(path) > 1 else 0.0
            if path is None or plen > max(2.5 * straight, straight + 5000.0):
                why = "no %s path" % leg if path is None else "the %s path detours (%.0f km for %.0f km)" % (leg, plen / 1000, straight / 1000)
                log("   ! %s: %s from %s to %s; a straight line instead" % (rdef["id"], why, pts[i][3], pts[i + 1][3]))
                notes.append("%s from %s to %s: straight" % (why, pts[i][3], pts[i + 1][3]))
                seq.append(wp_real[i + 1])
            else:
                for d, lab in ((ds_, pts[i][3]), (dt_, pts[i + 1][3])):
                    if d > 2500:
                        log("   ! %s: %s is %.1f km from the %s network" % (rdef["id"], lab, d / 1000, leg))
                seq.extend(net.xy[path])
                if i + 1 == len(pts) - 1:
                    seq.append(wp_real[i + 1])     # the exact end, so branches meet their parents
        station_idx.append(len(seq) - 1)
    seq = np.asarray(seq)
    keep = np.ones(len(seq), bool)
    keep[1:] = np.hypot(*np.diff(seq, axis=0).T) > 1e-6
    remap = np.cumsum(keep) - 1
    seq = seq[keep]
    station_idx = [int(remap[k]) for k in station_idx]
    G = frame.real_to_game(seq)
    s = np.concatenate([[0.0], np.cumsum(np.hypot(*np.diff(G, axis=0).T))])
    st_s, st_w, sources = [], [], []
    for k, p in zip(station_idx, pts):
        cap = args.branch_width if rdef["id"] in BRANCHES else args.max_width
        w, src = station_width(p[2], records, cap)
        if w is not None:
            st_s.append(s[k])
            st_w.append(w)
            sources.append({"at": p[3], "widthM": w, "source": src})
    if not st_w:
        raise SystemExit("river %s has no width" % rdef["id"])
    W = np.interp(s, st_s, st_w)
    real_len = float(np.hypot(*np.diff(seq, axis=0).T).sum())
    pins = G[station_idx]
    P, W, ds, iters, worst, tight = ease_bends(G, W, args.bend, pins=pins)
    return dict(id=rdef["id"], name=rdef["name"], P=P, W=W, ds=ds, iters=iters, worst=worst, tight=tight, widths=sources,
                notes=notes, real_km=real_len / 1000.0,
                waypoints=[(p[3], frame.ll_to_game([(p[1], p[0])])[0]) for p in pts])


def trim_at_sea(river, sd_grid, gx0, gy0, res):
    """Cut the river where, after a stretch on land, its whole width first lies in sea or lake water (plus one width
    of overlap): an estuary narrower than the river keeps the river going until it opens out."""
    P, W = river["P"], river["W"]
    col = (P[:, 0] - gx0) / res - 0.5
    row = (P[:, 1] - gy0) / res - 0.5
    sd = ndimage.map_coordinates(sd_grid, [row, col], order=1, mode="constant", cval=-1.0)
    inside = sd >= W / 2.0
    land_before = np.concatenate([[False], np.logical_or.accumulate(~inside)[:-1]])
    hits = np.nonzero(inside & land_before)[0]
    if len(hits) == 0:
        river["trimmed_m"] = 0.0
        return
    j = int(hits[0])
    cut = min(len(P) - 1, j + int(math.ceil(W[j] / river["ds"])) + 1)
    river["trimmed_m"] = float((len(P) - 1 - cut) * river["ds"])
    river["P"], river["W"] = P[:cut + 1], W[:cut + 1]


def ribbon(P, W):
    T = np.gradient(P, axis=0)
    T /= np.maximum(np.hypot(T[:, 0], T[:, 1]), 1e-9)[:, None]
    N = np.column_stack([-T[:, 1], T[:, 0]])
    L = P + N * (W / 2.0)[:, None]
    R = P - N * (W / 2.0)[:, None]
    poly = shapely.make_valid(Polygon(np.vstack([L, R[::-1]])))
    caps = [Point(P[0]).buffer(W[0] / 2.0, 16), Point(P[-1]).buffer(W[-1] / 2.0, 16)]
    return shapely.MultiPolygon(polys_only(shapely.union_all([poly] + caps)))


# ---- Bathymetry ------------------------------------------------------------------------------------------------------
def load_bathy(frame, llbox, gbounds, res_game):
    """EMODnet elevation (m, negative = below the sea) warped into the game frame at res_game, gaps filled by nearest."""
    lon0, lat0, lon1, lat1 = llbox
    tifs = []
    for p in sorted(glob.glob(EMOD_GLOB)):
        try:
            ds = gdal.Open(p)
        except RuntimeError:
            continue
        gt = ds.GetGeoTransform()
        x0, y1 = gt[0], gt[3]
        x1, y0 = x0 + gt[1] * ds.RasterXSize, y1 + gt[5] * ds.RasterYSize
        if x1 > lon0 and x0 < lon1 and y1 > lat0 and y0 < lat1:
            tifs.append(p)
        ds = None
    gx0, gy0, gx1, gy1 = gbounds
    nx, ny = int(math.ceil((gx1 - gx0) / res_game)), int(math.ceil((gy1 - gy0) / res_game))
    if not tifs:
        return np.full((ny, nx), -100.0, np.float32), np.ones((ny, nx), bool), []
    vrt = gdal.BuildVRT("/vsimem/kapman_emod.vrt", tifs)
    s = frame.scale
    out = gdal.Warp("", vrt, format="MEM", dstSRS=PROJ, outputBounds=(gx0 * s, gy0 * s, (gx0 + nx * res_game) * s,
                    (gy0 + ny * res_game) * s), width=nx, height=ny, resampleAlg="average", srcNodata="nan",
                    dstNodata="nan", outputType=gdal.GDT_Float32)
    a = out.GetRasterBand(1).ReadAsArray()[::-1].astype(np.float32)     # row 0 = south
    out = None
    vrt = None
    gdal.Unlink("/vsimem/kapman_emod.vrt")
    nan = ~np.isfinite(a)
    if nan.all():
        a[:] = -100.0
    elif nan.any():
        idx = ndimage.distance_transform_edt(nan, return_distances=False, return_indices=True)
        a = a[tuple(idx)]
    return a, nan, [os.path.basename(t) for t in tifs]


# ---- The bake --------------------------------------------------------------------------------------------------------
def main():
    ap = argparse.ArgumentParser(description="Bake the open world's water tiles for a lat/lon box.")
    ap.add_argument("--box", type=float, nargs=4, default=[-0.6, 51.0, 7.3, 53.7], metavar=("LON0", "LAT0", "LON1", "LAT1"),
                    help="the region, lon/lat (default: the North Sea core)")
    ap.add_argument("--scale", type=float, default=40.0, help="real lengths per game length (default 40)")
    ap.add_argument("--sea-scale", type=float, default=None, help="a harder shortening for the open sea (not built yet: "
                    "must equal --scale)")
    ap.add_argument("--tile", type=float, default=1024.0, help="tile size, game metres")
    ap.add_argument("--cell", type=float, default=2.0, help="cell size, game metres")
    ap.add_argument("--supersample", type=int, default=2, help="sub-cells per cell side for the distance field")
    ap.add_argument("--cap", type=float, default=256.0, help="the signed distance is clamped to +/- this (game m)")
    ap.add_argument("--band", type=float, default=60.0, help="the waterfront band's width (game m)")
    ap.add_argument("--shallow", type=float, default=10.0, help="sea shallower than this (real m) is 'shallow'")
    ap.add_argument("--max-width", type=float, default=300.0, help="the cap on river widths (game m = real m)")
    ap.add_argument("--branch-width", type=float, default=150.0, help="the cap on the delta branches and side rivers (BRANCHES)")
    ap.add_argument("--bend", type=float, default=3.0, help="no bend's radius under this many widths")
    ap.add_argument("--vos-channel", type=float, default=300.0, help="Vos inland water narrower than this on average "
                    "(real m) is a river channel the true-width rivers replace; 0 keeps all")
    ap.add_argument("--simplify", type=float, default=1.0, help="coast simplification tolerance (game m)")
    ap.add_argument("--smooth", type=int, default=2, help="Chaikin passes on the Natural Earth coast (0 = none)")
    ap.add_argument("--segment", type=float, default=20.0, help="longest coast segment before smoothing (game m)")
    ap.add_argument("--name", default=None, help="output folder under data\\world (default: the scale, e.g. 40)")
    ap.add_argument("--tiles", type=int, nargs=4, default=None, metavar=("X0", "Y0", "X1", "Y1"),
                    help="only bake these tile indices (inclusive), for tests; index.json then lists only them")
    args = ap.parse_args()
    if args.sea_scale is None:
        args.sea_scale = args.scale
    if args.sea_scale != args.scale:
        raise SystemExit("--sea-scale different from --scale is designed for (Frame.real_to_game) but not built yet")
    if args.tile % args.cell or (args.cell / args.supersample) <= 0:
        raise SystemExit("--tile must be a whole number of cells")

    t_all = time.time()
    timings = {}
    name = args.name or ("%g" % args.scale)
    out_dir = os.path.join(OUT_ROOT, name)
    tiles_dir = os.path.join(out_dir, "tiles")
    log_lines = []

    def log(msg):
        print(msg, flush=True)
        log_lines.append(msg)

    frame = Frame(args.scale, args.sea_scale)
    T, cell, ss = args.tile, args.cell, args.supersample
    nc = int(round(T / cell))

    # The region: the lon/lat box in the game frame, and the tiles it touches.
    lon0, lat0, lon1, lat1 = args.box
    edge = np.linspace(0, 1, 60)
    ring = np.vstack([np.column_stack([lon0 + (lon1 - lon0) * edge, np.full(60, lat0)]),
                      np.column_stack([np.full(60, lon1), lat0 + (lat1 - lat0) * edge]),
                      np.column_stack([lon1 - (lon1 - lon0) * edge, np.full(60, lat1)]),
                      np.column_stack([np.full(60, lon0), lat1 - (lat1 - lat0) * edge])])
    region = Polygon(frame.ll_to_game(ring))
    rx0, ry0, rx1, ry1 = region.bounds
    tiles = []
    for iy in range(int(math.floor(ry0 / T)), int(math.ceil(ry1 / T))):
        for ix in range(int(math.floor(rx0 / T)), int(math.ceil(rx1 / T))):
            if args.tiles and not (args.tiles[0] <= ix <= args.tiles[2] and args.tiles[1] <= iy <= args.tiles[3]):
                continue
            if region.intersects(shapely.box(ix * T, iy * T, (ix + 1) * T, (iy + 1) * T)):
                tiles.append((ix, iy))
    if not tiles:
        raise SystemExit("no tiles in the box")
    tx0 = min(t[0] for t in tiles) * T
    ty0 = min(t[1] for t in tiles) * T
    tx1 = (max(t[0] for t in tiles) + 1) * T
    ty1 = (max(t[1] for t in tiles) + 1) * T
    margin = math.ceil((args.cap + 2 * cell) / cell) * cell
    gb = (tx0 - margin, ty0 - margin, tx1 + margin, ty1 + margin)          # game bounds of everything baked
    gring = np.array([(gb[0] + (gb[2] - gb[0]) * u, gb[1]) for u in edge] + [(gb[2], gb[1] + (gb[3] - gb[1]) * u) for u in edge] +
                     [(gb[2] - (gb[2] - gb[0]) * u, gb[3]) for u in edge] + [(gb[0], gb[3] - (gb[3] - gb[1]) * u) for u in edge])
    ll = frame.game_to_ll(gring)
    src_box = (ll[:, 0].min() - 0.1, ll[:, 1].min() - 0.1, ll[:, 0].max() + 0.1, ll[:, 1].max() + 0.1)
    log("Kapman world bake: scale 1:%g, box %s, %d tiles of %g m (%d x %d cells of %g m), game bounds %s" % (
        args.scale, args.box, len(tiles), T, nc, nc, cell, [round(v) for v in gb]))

    # Sources.
    t = time.time()
    ne_polys = load_natural_earth(frame, src_box, args)
    timings["natural_earth_s"] = round(time.time() - t, 1)
    log("  Natural Earth land: %d polygons, %d vertices (%.1f s)" % (len(ne_polys), sum(shapely.get_num_coordinates(ne_polys)), timings["natural_earth_s"]))
    t = time.time()
    vos, vos_classes = load_vos(frame, src_box, args)
    timings["vos_s"] = round(time.time() - t, 1)
    log("  Vos c. 800: %s polygons (domain %d, sea %d, inland water %d, tidal %d) (%.1f s)" % (
        sum(v for v in vos_classes.values() if isinstance(v, int)), len(vos["domain"]), len(vos["sea"]), len(vos["inland"]), len(vos["tidal"]), timings["vos_s"]))

    # Rivers.
    t = time.time()
    records = load_records()
    rdefs = []
    for r in RIVERS:
        la = [p[0] for p in r["pts"]]
        lo = [p[1] for p in r["pts"]]
        if max(lo) >= lon0 and min(lo) <= lon1 and max(la) >= lat0 and min(la) <= lat1:
            rdefs.append(r)
    nets = {}
    if rdefs:
        la = [p[0] for r in rdefs for p in r["pts"]]
        lo = [p[1] for r in rdefs for p in r["pts"]]
        hbox = (min(lo) - 0.3, min(la) - 0.3, max(lo) + 0.3, max(la) + 0.3)
        if any(r["net"] == "hydro" for r in rdefs):
            nets["hydro"] = Net(*load_hydro(frame, hbox), snap_m=1500.0)
        if any(r["net"] == "vos" for r in rdefs):
            nets["vos"] = Net(*load_vos_lines(frame, hbox), snap_m=1000.0)
    log("  river networks: %s (%.1f s)" % ({k: len(v.xy) for k, v in nets.items()}, time.time() - t))
    rivers = []
    for r in rdefs:
        rv = trace_river(r, nets, frame, records, args, log)
        rivers.append(rv)

    # Polygon water at a coarse cell for trimming the rivers where the sea swallows them.
    land_g, dom_g, sea_g, in_g, tid_g = Group(ne_polys), Group(vos["domain"]), Group(vos["sea"]), Group(vos["inland"]), Group(vos["tidal"])
    cres = 8.0
    cnx, cny = int(math.ceil((gb[2] - gb[0]) / cres)), int(math.ceil((gb[3] - gb[1]) / cres))
    c_land = rasterize(ne_polys, gb[0], gb[1], cnx, cny, cres)
    c_dom = rasterize(vos["domain"], gb[0], gb[1], cnx, cny, cres)
    c_sea = rasterize(vos["sea"], gb[0], gb[1], cnx, cny, cres)
    c_in = rasterize(vos["inland"], gb[0], gb[1], cnx, cny, cres)
    c_pw = np.where(c_dom, c_sea | c_in, ~c_land)
    c_sd = (ndimage.distance_transform_edt(c_pw) * cres).astype(np.float32)
    for rv in rivers:
        trim_at_sea(rv, c_sd, gb[0], gb[1], cres)
        rv["ribbon"] = ribbon(rv["P"], rv["W"])
        line = shapely.LineString(rv["P"])
        rv["offsets"] = [(lab, round(float(line.distance(Point(q))), 1)) for lab, q in rv["waypoints"]]
        worst_off = max(rv["offsets"][:-1] or rv["offsets"], key=lambda o: o[1])
        log("  river %-12s %6.1f km real -> %5.0f m game, width %3.0f-%3.0f m, eased in %d steps (tightest bend %.1f widths, %.0f%% of it under %g), "
            "trimmed %.0f m at the sea; farthest waypoint %.0f m off (%s)" % (
                rv["id"], rv["real_km"], (len(rv["P"]) - 1) * rv["ds"], rv["W"].min(), rv["W"].max(), rv["iters"], rv["worst"],
                100 * rv["tight"], args.bend, rv["trimmed_m"], worst_off[1], worst_off[0]))
    riv_g = Group([rv["ribbon"] for rv in rivers])
    riv_ids = [rv["id"] for rv in rivers]
    del c_land, c_dom, c_sea, c_in, c_pw, c_sd
    timings["rivers_s"] = round(time.time() - t, 1)

    # Bathymetry.
    t = time.time()
    bres = 8.0
    bathy, bathy_nan, bathy_files = load_bathy(frame, src_box, gb, bres)
    timings["bathymetry_s"] = round(time.time() - t, 1)
    log("  EMODnet: %s -> %s grid at %g m (%.1f s)" % (bathy_files, bathy.shape, bres, timings["bathymetry_s"]))

    # Tiles.
    if os.path.isdir(tiles_dir):
        shutil.rmtree(tiles_dir)
    os.makedirs(tiles_dir, exist_ok=True)
    t = time.time()
    f = cell / ss
    mc = int(round(margin / cell))          # margin in output cells
    nf = int(round((T + 2 * margin) / f))   # fine cells per side, margin included
    entries = []
    bytes_written = 0
    centres = (np.arange(nc) + 0.5) * cell
    for k, (ix, iy) in enumerate(tiles):
        x0, y0 = ix * T, iy * T
        ex0, ey0, ex1, ey1 = x0 - margin, y0 - margin, x0 + T + margin, y0 + T + margin
        clip = lambda grp: grp.clip(ex0, ey0, ex1, ey1)
        land = rasterize(clip(land_g), ex0, ey0, nf, nf, f)
        dom = rasterize(clip(dom_g), ex0, ey0, nf, nf, f)
        vsea = rasterize(clip(sea_g), ex0, ey0, nf, nf, f)
        vin = rasterize(clip(in_g), ex0, ey0, nf, nf, f)
        tid = rasterize(clip(tid_g), ex0, ey0, nf, nf, f)
        riv = rasterize(clip(riv_g), ex0, ey0, nf, nf, f)
        sea = np.where(dom, vsea, ~land)
        water = sea | (dom & vin) | riv
        if water.all():
            sdf = np.full(water.shape, args.cap, np.float32)
        elif not water.any():
            sdf = np.full(water.shape, -args.cap, np.float32)
        else:
            d_in = ndimage.distance_transform_edt(water)
            d_out = ndimage.distance_transform_edt(~water)
            sdf = (np.where(water, d_in - 0.5, 0.5 - d_out) * f).astype(np.float32)
        fm = mc * ss
        core = slice(fm, fm + nc * ss)

        def pool(a):
            a = a[core, core].astype(np.float32)
            return a.reshape(nc, ss, nc, ss).mean(axis=(1, 3))

        sd = np.clip(pool(sdf), -args.cap, args.cap).astype(np.float32)
        seaf, watf, tidf, domf = pool(sea & water), pool(water), pool(tid & dom & sea), pool(dom)
        rows = (y0 + centres - gb[1]) / bres - 0.5
        cols = (x0 + centres - gb[0]) / bres - 0.5
        rr, cc = np.meshgrid(rows, cols, indexing="ij")
        elev = ndimage.map_coordinates(bathy, [rr, cc], order=1, mode="nearest")
        dry_today = ndimage.map_coordinates(bathy_nan.astype(np.float32), [rr, cc], order=1, mode="nearest") >= 0.5
        kind = np.full((nc, nc), INLAND, np.uint8)
        isw = sd >= 0
        kind[(~isw) & (sd >= -args.band)] = BAND
        seaflag = isw & (seaf >= watf - seaf)
        kind[isw & ~seaflag] = RIVER
        # Shallow: EMODnet says so, or the Vos tidal flats, or Vos c. 800 water that is land today (no EMODnet depth:
        # silted up since, so shallow then).
        shallow = (elev > -args.shallow) | (tidf >= 0.5) | (dry_today & (domf >= 0.5))
        kind[seaflag & shallow] = SHALLOW
        kind[seaflag & ~shallow] = DEEP
        counts = np.bincount(kind.ravel(), minlength=5).tolist()
        tile_rivers = [riv_ids[i] for i in (riv_g.tree.query(shapely.box(x0, y0, x0 + T, y0 + T), predicate="intersects")
                                             if riv_g.tree is not None else [])]
        entry = {"x": ix, "y": iy, "files": True, "counts": counts, "sd": [round(float(sd.min()), 2), round(float(sd.max()), 2)],
                 "rivers": sorted(tile_rivers)}
        if counts[DEEP] == nc * nc and sd.min() >= args.cap:
            entry.update(files=False, fill={"sd": args.cap, "kind": DEEP})
        elif counts[INLAND] == nc * nc and sd.max() <= -args.cap:
            entry.update(files=False, fill={"sd": -args.cap, "kind": INLAND})
        if entry["files"]:
            tdir = os.path.join(tiles_dir, "%d_%d" % (ix, iy))
            os.makedirs(tdir, exist_ok=True)
            sd.astype("<f4").tofile(os.path.join(tdir, "water_sd.f32"))
            kind.tofile(os.path.join(tdir, "kind.u8"))
            corners = frame.game_to_ll([(x0, y0), (x0 + T, y0), (x0 + T, y0 + T), (x0, y0 + T)])
            meta = {"tile": [ix, iy], "origin": [x0, y0], "size": T, "cell": cell, "cells": [nc, nc], "scale": args.scale,
                    "seaScale": args.sea_scale, "cap": args.cap, "band": args.band,
                    "files": {"water_sd.f32": "float32 little-endian, %d x %d, row-major; row 0 = the south edge, column 0 = "
                                              "the west edge; cell (row r, col c) is centred at origin + ((c + 0.5), (r + 0.5)) "
                                              "x cell; game metres to the waterline, + in water, clamped to +/- cap" % (nc, nc),
                              "kind.u8": "uint8, same layout: 0 deep sea, 1 shallow sea, 2 river (inland water), "
                                         "3 band (land within band of water), 4 inland (under fog)"},
                    "counts": dict(zip(KIND_NAMES, counts)), "sdRange": entry["sd"], "rivers": entry["rivers"],
                    "cornersLonLat": {"sw": corners[0].round(5).tolist(), "se": corners[1].round(5).tolist(),
                                      "ne": corners[2].round(5).tolist(), "nw": corners[3].round(5).tolist()}}
            json.dump(meta, open(os.path.join(tdir, "meta.json"), "w", encoding="utf-8"), indent=1)
            bytes_written += sum(os.path.getsize(os.path.join(tdir, n)) for n in os.listdir(tdir))
        entries.append(entry)
        if (k + 1) % 20 == 0 or k + 1 == len(tiles):
            log("  tiles %d/%d (%.0f s)" % (k + 1, len(tiles), time.time() - t))
    timings["tiles_s"] = round(time.time() - t, 1)

    # The index and the rivers.
    timings["total_s"] = round(time.time() - t_all, 1)
    n_files = sum(1 for e in entries if e["files"])
    index = {
        "about": "Kapman's open world, baked by Bake/world/make_tiles.py on %s. Game metres: x east, y north, in the "
                 "Lambert conformal frame of make_world_map.py divided by the scale. Each listed tile with files has "
                 "tiles/<x>_<y>/water_sd.f32, kind.u8 and meta.json; a tile without files is uniform: 'fill' gives its "
                 "signed distance and kind." % time.strftime("%Y-%m-%d %H:%M"),
        "scale": args.scale, "seaScale": args.sea_scale, "projection": PROJ, "box": args.box,
        "tileSize": T, "cell": cell, "cellsPerSide": nc, "supersample": ss, "cap": args.cap, "band": args.band,
        "shallowDepthM": args.shallow, "maxRiverWidth": args.max_width, "branchWidth": args.branch_width, "bendWidths": args.bend,
        "kinds": KIND_NAMES,
        "format": {"water_sd.f32": "float32 little-endian, row-major, row 0 = south, col 0 = west",
                   "kind.u8": "uint8, same layout"},
        "tileRange": [min(t[0] for t in tiles), min(t[1] for t in tiles), max(t[0] for t in tiles), max(t[1] for t in tiles)],
        "gameBounds": [tx0, ty0, tx1, ty1],
        "tilesWithFiles": n_files, "tilesEmpty": len(entries) - n_files, "bytes": bytes_written,
        "timings": timings,
        "sources": SOURCES, "vosClasses": vos_classes,
        "rivers": [{"id": rv["id"], "name": rv["name"], "realKm": round(rv["real_km"], 1),
                    "gameM": round((len(rv["P"]) - 1) * rv["ds"], 1), "widths": rv["widths"], "notes": rv["notes"]} for rv in rivers],
        "tiles": entries,
    }
    os.makedirs(out_dir, exist_ok=True)
    json.dump(index, open(os.path.join(out_dir, "index.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=1)
    rv_out = {"about": "River centre lines in game metres (x east, y north), upstream first, every ~half width; w = the "
                       "true width (capped) at each point. From Bake/world/make_tiles.py.", "scale": args.scale,
              "rivers": []}
    for rv in rivers:
        step = max(1, int(round(rv["W"].min() / 2.0 / rv["ds"])))
        idx = list(range(0, len(rv["P"]), step))
        if idx[-1] != len(rv["P"]) - 1:
            idx.append(len(rv["P"]) - 1)
        rv_out["rivers"].append({"id": rv["id"], "name": rv["name"],
                                 "points": [[round(float(rv["P"][i, 0]), 1), round(float(rv["P"][i, 1]), 1)] for i in idx],
                                 "w": [round(float(rv["W"][i]), 1) for i in idx],
                                 "tightestBendWidths": round(rv["worst"], 2), "shareTighterThanBend": round(rv["tight"], 4), "widths": rv["widths"],
                                 "waypointOffsetsM": [{"at": lab, "m": d} for lab, d in rv["offsets"]]})
    json.dump(rv_out, open(os.path.join(out_dir, "rivers.json"), "w", encoding="utf-8"), ensure_ascii=False)
    with open(os.path.join(out_dir, "bake_log.txt"), "w", encoding="utf-8") as fh:
        fh.write("\n".join(log_lines) + "\n")
    log("done: %d tiles (%d with files, %d empty), %.1f MB, %.1f s -> %s" % (
        len(entries), n_files, len(entries) - n_files, bytes_written / 1e6, timings["total_s"], out_dir))


SOURCES = [
    {"name": "Natural Earth 10 m physical (land, minor islands)", "licence": "public domain",
     "credit": "Made with Natural Earth. Free vector and raster map data @ naturalearthdata.com."},
    {"name": "Paleogeografische kaarten van Nederland, 800 n.Chr. (paleogeografischekrt2021 GDB: paleogeografie800nc, "
             "paleogeografie_waterlopen 800nc)", "licence": "CC BY 4.0",
     "credit": "Paleogeografische kaarten van Nederland, P.C. Vos & S. de Vries (Deltares/TNO), Rijksdienst voor het "
               "Cultureel Erfgoed, CC BY 4.0"},
    {"name": "HydroRIVERS v1.0 (Europe)", "licence": "CC BY 4.0",
     "credit": "Lehner, B., Grill G. (2013): Global river hydrography and network routing: baseline data and new "
               "approaches to study the world's large river systems. Hydrological Processes, 27(15): 2171-2186. "
               "Data available at www.hydrosheds.org (CC BY 4.0)."},
    {"name": "EMODnet Digital Bathymetry (DTM 2024)", "licence": "CC BY 4.0",
     "credit": "EMODnet Bathymetry Consortium (2024): EMODnet Digital Bathymetry (DTM 2024), emodnet.ec.europa.eu, CC BY 4.0"},
    {"name": "Kapman water records (Assets/Kapman/Data/Water/water_793.json)", "licence": "own research",
     "credit": "river widths; each record carries its own sources"},
]

if __name__ == "__main__":
    main()
