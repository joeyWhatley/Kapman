"""ANY RIVER PLACE - the level's ground, water and land cover at true size (1:1), from the c. 800 ground.

The same bake as make_dorestad.py, for any river place, driven by a config file:

    Bake\\run.bat levels\\make_place.py <id>                  reads levels\\places\\<id>.json
    Bake\\run.bat levels\\make_place.py <id> --out <folder>   writes there instead of the config's "out"
    Bake\\run.bat levels\\make_place.py <id> --no-preview     skips preview.png

It writes, to the config's "out" folder (G:\\kapman_unity\\data\\levels\\<id> by default):

  height.f32      float32 (N, N), metres on the source's datum, row 0 = the south edge
  water_sd.f32    float32 (N, N), signed distance to the water's edge in metres (+ in the water, - on land)
  class.u8        uint8 (N, N): the source's own classes on land, the river's class code on the water (meta
                  class_names lists them)
  cover.u8        uint8 (N, N): the land cover c. 800 (meta cover_classes: 0-11 the river land as at Dorestad, and
                  on a tidal or sea place 12 beach, 13 dunes, 14 salt marsh, 15 tidal flat)
  wood.u8         uint8 (N, N): how thickly wooded (0..255)
  meta.json       the grid, the water level, the port, the channel's centre line, the anchors, the cover classes,
                  the sources
  preview.png     a check plot

The sources ("source" in the config):
  nl_793      G:\\kapman_unity\\data\\work\\realground\\nl_793_10m.tif (+ _info): the Netherlands c. 800 from AHN4 lidar
              (RD New, EPSG:28992), with its caches (the Vos 800 units, Cohen's belts, the 800 channel lines)
  thames_793  G:\\kapman_unity\\data\\work\\realground\\thames_793_10m.tif (+ _info): the tidal Thames c. 793 from the
              EA lidar (British National Grid, EPSG:27700), with GRWL's centre lines for the channel

With "dorestad" this reproduces make_dorestad.py's grids exactly (the config places\\dorestad.json holds every number
make_dorestad.py has as a constant; check_place.py compares the two outputs).

The level's frame: x east, z north, metres from the box's south-west corner (in the source's CRS).
"""
import copy
import glob
import json
import math
import os
import sys

import numpy as np
from osgeo import gdal, ogr, osr
from scipy import ndimage as ndi

gdal.UseExceptions()

HERE = os.path.dirname(os.path.abspath(__file__))
PLACES = os.path.join(HERE, "places")
CHAIN = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Kapman", "Data", "Chain", "chain.json"))
RG = r"G:\kapman_unity\data\work\realground"
CACHE = os.path.join(RG, "cache")
LEVELS = r"G:\kapman_unity\data\levels"
RAW = r"G:\kapman_unity\data\raw"
EMODNET = os.path.join(RAW, "bathy", "emodnet")
# folders another job owns: a bake never writes there unless asked to in so many words (--write-protected)
PROTECTED = [os.path.normcase(os.path.join(LEVELS, "dorestad"))]

# ---------------------------------------------------------------------------------------------- the cover classes
# 0-11 are make_dorestad.py's, word for word (a config may reword them for its own country with "cover_names").
COVER = {
    0: "water",
    1: "levee crest: the dry sandy top of the Kromme Rijn's and the Lek's levees (settlement, fields, hay)",
    2: "levee flank and dry basin margin: hay meadow and pasture",
    3: "flood basin: wet meadow on heavy clay, pasture and hay, under water in the floods",
    4: "reed marsh and sedge: the lowest basin land, the old gullies and oxbow scars, wet all year",
    5: "alder carr: the wet wilderness wood of the basins (alder and willow, birch on the peat)",
    6: "peat (veen): the Langbroek bog's edge, moss and birch",
    7: "older channel-belt ridge: sandy, dry, long dead (fields, oak and hazel on the driest parts)",
    8: "bank mud: the wet shore within a few metres of the water (wide where the shore is flat)",
    9: "sand bar: a point bar on the inside of a bend, just over the water",
    10: "pool: open water left in the lowest basin hollows after the floods",
    11: "levee wood: ash, elm and oak on the high dry land away from the settlement",
}
# the coast's classes: only on a place with a tide or a sea, so a river place's meta stays as Dorestad's
COAST_COVER = {
    12: "coast, beach: bare sand from the low water mark to the foot of the dunes",
    13: "coast, dunes: dry sand hills behind the beach, marram grass, sea buckthorn and scrub",
    14: "coast, salt marsh: grass, sea lavender and samphire that the spring tides flood, cut by creeks",
    15: "coast, tidal flat: mud and sand under the water at the mean level and bare at low tide",
}
COVER_COLOURS = ["#3e5c8e", "#d9c98a", "#b8c47a", "#8fae6a", "#6f8f5a", "#2f5a34", "#6c6844", "#cdb97a", "#7a6248",
                 "#e8dcb0", "#4f7fa8", "#3f7040", "#f2e6c2", "#c9b77e", "#7f9c84", "#8a8470"]

# ---------------------------------------------------------------------------------------------- config defaults
# Every number make_dorestad.py had as a constant, with its value there. A place's config overrides any of them.
DEFAULTS = {
    "n": 2049,                      # Unity heightmap size (2^11 + 1)
    "cell": 2.5,                    # m
    "margin": 600.0,                # m of the source read round the box (the distance fields need it)
    "noise_seed": 0,                # added to the noise seeds (0 = Dorestad's patches)
    "water_mask": "masks",          # "masks": the rivers' own masks are the water; "below_level": the ground under
                                    # the water level that is joined to a river's mask (or the sea's)
    "water_level": {"rule": "bank_ring", "pct": 20, "ring_m": [10.0, 60.0]},
    "shore": {
        "edge_depth": 0.35,         # m at the water's edge
        "depth_ramp": 30.0,         # m from the edge to full depth (a river may have its own "ramp_m")
        "bank_rise": 12.0,          # m: the bank stands over the water this far out (0 or null: no bank lift)
        "bank_lift": 0.12,          # m over the water at the edge ...
        "bank_slope": 0.04,         # ... rising this much a metre
        "sd_blur": 2.0,             # grid cells: the shoreline's 10 m steps rounded off
        "height_blur": 1.0,         # grid cells: the heights smoothed between the two shapings
    },
    "cover": {
        "crest_pct_near": 80, "flank_pct_near": 55, "marsh_pct_near": 18,
        "crest_pct_far": 85, "flank_pct_far": 60, "marsh_pct_far": 20,
        "near_river_m": 600.0, "near_blend_m": 300.0,
        "pool_under": -0.03, "pool_min_ha": 0.10,
        "mud_flat_hw": [0.4, 1.4], "mud_width": [26.0, 2.0], "mud_under": 0.7,
        "bend_radius": 150.0, "bend_bar": 0.8, "bar_width": [50.0, 90.0], "bar_under": 0.9,
        "peat_vos": 6,              # the Vos unit index of 'veen' in the nl_maps band 1 (null: no peat class)
    },
    "settlement": {"along_m": 2000.0, "wide_m": 450.0, "far_m": 120.0, "fade_m": 400.0, "clearings": []},
    "woods": {"basin_share": 0.68, "high_share": 0.35, "shore_bare_m": 25.0},
    "coast": {
        "saltmarsh_over_hw": 0.5,   # m: salt marsh up to this over high water (thames_793: SALTMARSH_TOP)
        "beach_width_m": 250.0,     # m from the sea: the beach (a guess)
        "beach_top": 1.5,           # m over high water: the beach's top, the dunes' foot (a guess)
        "dune_reach_m": 3000.0,     # m from the sea: dunes only this near it
        "dune_wood": 0.25,          # the dunes' wood density is cut to this share (scrub only)
    },
}


def deep_merge(base, over):
    out = copy.deepcopy(base)
    for k, v in over.items():
        if isinstance(v, dict) and isinstance(out.get(k), dict):
            out[k] = deep_merge(out[k], v)
        else:
            out[k] = copy.deepcopy(v)
    return out


def load_config(place_id):
    path = place_id if place_id.lower().endswith(".json") else os.path.join(PLACES, place_id + ".json")
    with open(path, encoding="utf-8") as f:
        cfg = json.load(f)
    cfg = deep_merge(DEFAULTS, cfg)
    cfg["_path"] = os.path.abspath(path)
    cfg.setdefault("out", os.path.join(LEVELS, cfg["id"]))
    return cfg


# ---------------------------------------------------------------------------------------------- coordinates
def transform(src_epsg, dst_epsg):
    a = osr.SpatialReference()
    a.ImportFromEPSG(src_epsg)
    a.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
    b = osr.SpatialReference()
    b.ImportFromEPSG(dst_epsg)
    b.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
    return osr.CoordinateTransformation(a, b)


def srs_of(epsg):
    s = osr.SpatialReference()
    s.ImportFromEPSG(epsg)
    s.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
    return s


class Window:
    """The source's 10 m window round the level's box (make_dorestad.py's c0, r0, w, h)."""

    def __init__(self, ds, x0, y0, size, margin):
        gt = ds.GetGeoTransform()
        self.gt = gt
        self.cell = gt[1]
        self.c0 = int((x0 - margin - gt[0]) // self.cell)
        self.r0 = int((gt[3] - (y0 + size + margin)) // self.cell)
        self.w = int((size + 2 * margin) // self.cell) + 2
        self.h = self.w
        self.wx0, self.wy1 = gt[0] + self.c0 * self.cell, gt[3] - self.r0 * self.cell
        self.projection = ds.GetProjection()
        self.bounds = (self.wx0, self.wy1 - self.h * self.cell, self.wx0 + self.w * self.cell, self.wy1)

    def read(self, band, fill):
        """The window from a band; cells off the raster get `fill` (inside it, exactly ReadAsArray)."""
        xs, ys = band.XSize, band.YSize
        if self.c0 >= 0 and self.r0 >= 0 and self.c0 + self.w <= xs and self.r0 + self.h <= ys:
            return band.ReadAsArray(self.c0, self.r0, self.w, self.h)
        a0, b0 = max(0, self.c0), max(0, self.r0)
        a1, b1 = min(xs, self.c0 + self.w), min(ys, self.r0 + self.h)
        probe = band.ReadAsArray(0, 0, 1, 1)
        out = np.full((self.h, self.w), fill, dtype=probe.dtype)
        if a1 > a0 and b1 > b0:
            out[b0 - self.r0:b1 - self.r0, a0 - self.c0:a1 - self.c0] = band.ReadAsArray(a0, b0, a1 - a0, b1 - b0)
        return out

    def mem(self, dtype=gdal.GDT_Byte):
        m = gdal.GetDriverByName("MEM").Create("", self.w, self.h, 1, dtype)
        m.SetGeoTransform((self.wx0, self.cell, 0, self.wy1, 0, -self.cell))
        m.SetProjection(self.projection)
        return m

    def warp(self, src, resample="bilinear", nodata=None, dtype=gdal.GDT_Float32):
        """Any raster (path or list of paths) warped onto the window's grid."""
        kw = dict(format="MEM", dstSRS=self.projection, outputBounds=self.bounds, xRes=self.cell, yRes=self.cell,
                  resampleAlg=resample, outputType=dtype)
        if nodata is not None:
            kw["dstNodata"] = nodata
        ds = gdal.Warp("", src, **kw)
        return ds.GetRasterBand(1).ReadAsArray()

    def line_mask(self, lines, half_width):
        """Polylines (source CRS) buffered by half_width (m) and burnt onto the window."""
        drv = ogr.GetDriverByName("MEM") or ogr.GetDriverByName("Memory")
        mds = drv.CreateDataSource("")
        srs = osr.SpatialReference()
        srs.ImportFromWkt(self.projection)
        lyr = mds.CreateLayer("lines", srs, ogr.wkbPolygon)
        for ln in lines:
            g = ogr.Geometry(ogr.wkbLineString)
            for p in ln:
                g.AddPoint_2D(float(p[0]), float(p[1]))
            f = ogr.Feature(lyr.GetLayerDefn())
            f.SetGeometry(g.Buffer(float(half_width)))
            lyr.CreateFeature(f)
        m = self.mem()
        gdal.RasterizeLayer(m, [1], lyr, burn_values=[1])
        return m.GetRasterBand(1).ReadAsArray() > 0


# ---------------------------------------------------------------------------------------------- the sources
class Source:
    """What a bake needs from a country's c. 800 raster. Each method returns arrays on the 10 m window."""
    name = "?"
    epsg = 0
    heights = info = ""
    class_names = {}
    credits = []

    def __init__(self, cfg):
        self.cfg = cfg
        self.ds = gdal.Open(self.heights)
        self.inf = gdal.Open(self.info)

    def window(self, x0, y0, size, margin):
        return Window(self.ds, x0, y0, size, margin)

    def read(self, win):
        """(heights with NaN where the raster has nothing, class band, weight 0..1)"""
        h10 = win.read(self.ds.GetRasterBand(1), np.nan).astype(np.float32)
        cls = win.read(self.inf.GetRasterBand(1), 0)
        wt = win.read(self.inf.GetRasterBand(3), 0).astype(np.float32) / 255.0
        return h10, cls, wt

    def mask(self, spec, win, cls):
        """A water body's mask from its spec: {"info_class": [..]}, {"line": [[x, y], ..], "width_m": w},
        {"lines": "<name>", "near": [x, y], "width_m": w} (a named line source picked by nearness)."""
        m = np.zeros((win.h, win.w), bool)
        if "info_class" in spec:
            for c in spec["info_class"]:
                m |= cls == c
        if "line" in spec:
            m |= win.line_mask([spec["line"]], spec.get("width_m", 60.0) / 2.0)
        return m

    def maps(self, win, cls):
        """The cover's own maps: 'vos' (unit index), 'active' (a river active c. 800), 'anybelt' (an older ridge),
        'belt' (a belt number)."""
        z = np.zeros((win.h, win.w), np.uint8)
        return {"vos": z, "active": z.astype(bool), "anybelt": z.astype(bool), "belt": z.astype(np.uint16)}

    def coast_maps(self, win, cls):
        """'tidal_marsh' (land the tides may reach), 'sandy' (a sandy coast), 'dunes' (dune ground)."""
        z = np.zeros((win.h, win.w), bool)
        return {"tidal_marsh": z, "sandy": z, "dunes": z.copy()}

    def channel_lines(self, spec, box):
        return []


class NL793(Source):
    """The Netherlands c. 800 (nl_793_design.md): class 1 river plain and peat, 3 the 800 channel, 4 ground kept."""
    name = "nl_793"
    epsg = 28992
    heights = os.path.join(RG, "nl_793_10m.tif")
    info = os.path.join(RG, "nl_793_10m_info.tif")
    MAPS10 = os.path.join(CACHE, "nl_maps_10m.tif")        # bands: 1 Vos unit, 2 Cohen belt active c. 800,
                                                            # 3 today's embanked floodplain, 4 WorldCover, 5 any belt
    BELT10 = os.path.join(CACHE, "nl_cohen_belt_10m.tif")   # Cohen's belt numbers (STROOM1)
    VOS = os.path.join(CACHE, "nl_vos800_rd.gpkg")
    LINES = os.path.join(CACHE, "nl_793_channel_lines.json")
    AHN = os.path.join(CACHE, "nl_ahn4_10m.tif")            # today's ground, AHN4 DTM mean, m NAP (band 1)
    class_names = {1: "river plain and peat (kwelder, beekdal, veen)", 3: "the 800 channel",
                   4: "ground kept (sands, moraines, donken, old dunes)"}
    credits = [
        "The Netherlands c. 800 from AHN4 lidar (CC0), the first build's Bake/realground/nl_793.py",
        "Vos et al., paleogeographic map 800 AD",
        "Cohen et al. 2012, Rhine-Meuse delta channel belts (CC BY 4.0)",
    ]

    def vos_mask(self, units, win):
        mem = win.mem()
        where = "legenda IN (%s)" % ",".join("'%s'" % u for u in units)
        gdal.Rasterize(mem, self.VOS, layers=["vos800"], burnValues=[1], where=where)
        return mem.GetRasterBand(1).ReadAsArray() > 0

    def mask(self, spec, win, cls):
        m = super().mask(spec, win, cls)
        if "vos_units" in spec:
            m |= self.vos_mask(spec["vos_units"], win)
        if "lines" in spec:
            lines = self.channel_lines({"lines": spec["lines"]}, win.bounds)
            near = spec.get("near")
            if near and lines:
                lines = [min(lines, key=lambda a: np.hypot(a[:, 0] - near[0], a[:, 1] - near[1]).min())]
            m |= win.line_mask(lines, spec.get("width_m", 60.0) / 2.0)
        return m

    def maps(self, win, cls):
        maps_ds = gdal.Open(self.MAPS10)
        belt_ds = gdal.Open(self.BELT10)
        return {"vos": win.read(maps_ds.GetRasterBand(1), 0), "active": win.read(maps_ds.GetRasterBand(2), 0),
                "anybelt": win.read(maps_ds.GetRasterBand(5), 0), "belt": win.read(belt_ds.GetRasterBand(1), 0)}

    def coast_maps(self, win, cls):
        c = self.cfg.get("coast", {})
        dunes = self.vos_mask(c.get("dune_units", ["duin", "strandwal"]), win)
        marsh = self.vos_mask(c.get("marsh_units", ["getijdenzone", "kwelderrug"]), win)
        reach = int(c.get("sandy_reach_m", 600.0) / win.cell)
        sandy = ndi.binary_dilation(dunes, iterations=max(1, reach)) if dunes.any() else dunes
        return {"tidal_marsh": marsh, "sandy": sandy, "dunes": dunes}

    def channel_lines(self, spec, box):
        if spec.get("lines", "nl_793_channel_lines") != "nl_793_channel_lines":
            return []
        with open(self.LINES) as f:
            return [np.asarray(ln, dtype=np.float64) for ln in json.load(f)]

    def outer(self, win, spec):
        """Today's ground (AHN4) on the window, moved to the raster's datum (minus D, nl_793_design.md 4)."""
        ds = gdal.Open(self.AHN)                        # held: a band outlives no dataset
        a = win.read(ds.GetRasterBand(1), np.nan).astype(np.float32)
        return a + float(spec.get("shift_m", -0.2))


class Thames793(Source):
    """The tidal Thames c. 793 (the first build's Bake/realground/README.md): heights in m ODN; class 1 marsh (793
    high water + relief), 2 foreshore (outside today's walls), 3 river channel and creeks, 4 ground kept."""
    name = "thames_793"
    epsg = 27700
    heights = os.path.join(RG, "thames_793_10m.tif")
    info = os.path.join(RG, "thames_793_10m_info.tif")
    LIDAR = os.path.join(CACHE, "thames_2m.vrt")            # today's ground, EA LIDAR Composite DTM 2022 2 m, m ODN
    GRWL = ["/vsizip/" + os.path.join(RAW, "hydro", "grwl", "GRWL_vector_V01.01.zip").replace("\\", "/")
            + "/GRWL_vector_V01.01/%s.shp" % t for t in ("NM30", "NM31")]
    # thames_793.py's 793 frame: high water c. 793 (m ODN) and today's mean low water by easting, which the raster
    # keeps where it is (README: "low water stays where it is, and today's mean high water comes down to HW_793")
    HW_793 = 1.25
    TIDE_NOW = [(525000, 3.1, -3.0), (532700, 3.0, -2.9), (564000, 2.8, -2.6), (588000, 2.4, -2.3), (605000, 2.4, -2.3)]
    LAT_ODN = [(525000, -3.2), (532700, -3.2), (564000, -3.1), (588000, -2.9), (605000, -2.9)]
    class_names = {1: "marsh (793 high water + relief)", 2: "foreshore (outside today's walls)",
                   3: "river channel and creeks", 4: "ground kept (terraces, islands, the 1 km round)"}
    credits = [
        "The tidal Thames c. 793 from the EA LIDAR Composite DTM 2022 (Contains Environment Agency information (c) "
        "Environment Agency and/or database right 2022, OGL v3), the first build's Bake/realground/thames_793.py",
        "EMODnet Bathymetry Consortium (2024): EMODnet Digital Bathymetry (DTM 2024) (CC BY 4.0)",
        "Allen, G. H. & Pavelsky, T. M. (2018), Global extent of rivers and streams, Science 361 (GRWL, CC BY 4.0)",
    ]

    def low_water(self, e):
        return float(np.interp(e, [t[0] for t in self.TIDE_NOW], [t[2] for t in self.TIDE_NOW]))

    def maps(self, win, cls):
        z = np.zeros((win.h, win.w), np.uint8)
        # the whole tidal floodplain is live ground (bars may grow anywhere in it); the terraces and islands kept
        # are the old high ground (they take the 'older ridge' class far from the river)
        return {"vos": z, "active": np.isin(cls, (1, 2, 3)), "anybelt": cls == 4, "belt": z.astype(np.uint16)}

    def coast_maps(self, win, cls):
        z = np.zeros((win.h, win.w), bool)
        return {"tidal_marsh": np.isin(cls, (1, 2)), "sandy": z, "dunes": z.copy()}

    def mask(self, spec, win, cls):
        m = super().mask(spec, win, cls)
        if spec.get("lines") == "grwl":
            lines = self.channel_lines({"lines": "grwl"}, win.bounds)
            m |= win.line_mask(lines, spec.get("width_m", 60.0) / 2.0)
        return m

    def channel_lines(self, spec, box):
        if spec.get("lines", "grwl") != "grwl":
            return []
        return grwl_lines(self.GRWL, self.epsg, box)

    def outer(self, win, spec):
        """Today's ground from the 2 m lidar, averaged to 10 m (ODN, the raster's own datum)."""
        a = win.warp(self.LIDAR, resample="average", nodata=-9999.0)
        a = a.astype(np.float32)
        a[a < -9000] = np.nan
        return a + float(spec.get("shift_m", 0.0))


SOURCES = {"nl_793": NL793, "thames_793": Thames793}


def grwl_lines(paths, epsg, box, join_m=40.0):
    """GRWL's centre lines (rivers and tidal rivers) in the box, joined end to end into long polylines."""
    dst = srs_of(epsg)
    q = ogr.CreateGeometryFromWkt("POLYGON((%f %f,%f %f,%f %f,%f %f,%f %f))" % (
        box[0], box[1], box[2], box[1], box[2], box[3], box[0], box[3], box[0], box[1]))
    pieces = []
    for p in paths:
        src = ogr.Open(p)
        if src is None:
            continue
        sl = src.GetLayer(0)
        srs = sl.GetSpatialRef()
        srs.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
        qq = q.Clone()
        qq.AssignSpatialReference(dst)
        qq.TransformTo(srs)
        sl.SetSpatialFilter(qq)
        sl.SetAttributeFilter("lakeFlag = 0 OR lakeFlag = 2")
        to = osr.CoordinateTransformation(srs, dst)
        for f in sl:
            g = f.GetGeometryRef().Clone()
            g.Transform(to)
            parts = [g] if g.GetGeometryCount() == 0 else [g.GetGeometryRef(i) for i in range(g.GetGeometryCount())]
            for part in parts:
                pts = np.asarray(part.GetPoints(), dtype=np.float64)[:, :2]
                if len(pts) >= 2:
                    pieces.append(pts)
        src = None
    # join the pieces end to end (GRWL's segments meet at their ends)
    lines = []
    while pieces:
        cur = pieces.pop(0)
        grown = True
        while grown:
            grown = False
            for i, pc in enumerate(pieces):
                if np.hypot(*(pc[0] - cur[-1])) < join_m:
                    cur = np.vstack([cur, pc[1:]])
                elif np.hypot(*(pc[-1] - cur[-1])) < join_m:
                    cur = np.vstack([cur, pc[::-1][1:]])
                elif np.hypot(*(pc[-1] - cur[0])) < join_m:
                    cur = np.vstack([pc[:-1], cur])
                elif np.hypot(*(pc[0] - cur[0])) < join_m:
                    cur = np.vstack([pc[::-1][:-1], cur])
                else:
                    continue
                pieces.pop(i)
                grown = True
                break
        lines.append(cur)
    return lines


def emodnet_on(win, epsg):
    """EMODnet DTM 2024 on the window (its own datum, mostly LAT), NaN off it."""
    lon_lat = transform(epsg, 4326)
    xs = [win.bounds[0], win.bounds[2]]
    ys = [win.bounds[1], win.bounds[3]]
    corners = [lon_lat.TransformPoint(x, y)[:2] for x in xs for y in ys]
    lo0, lo1 = min(c[0] for c in corners), max(c[0] for c in corners)
    la0, la1 = min(c[1] for c in corners), max(c[1] for c in corners)
    tiles = []
    for t in sorted(glob.glob(os.path.join(EMODNET, "*", "*.tif"))):
        d = gdal.Open(t)
        gt = d.GetGeoTransform()
        tx0, ty1 = gt[0], gt[3]
        tx1, ty0 = gt[0] + gt[1] * d.RasterXSize, gt[3] + gt[5] * d.RasterYSize
        if tx0 < lo1 and tx1 > lo0 and ty0 < la1 and ty1 > la0:
            tiles.append(t)
    if not tiles:
        return np.full((win.h, win.w), np.nan, np.float32), []
    e = win.warp(tiles, resample="bilinear", nodata=-9999.0).astype(np.float32)
    e[(e <= -9990.0) | (e < -200.0) | (e > 200.0)] = np.nan
    return e, [os.path.basename(t) for t in tiles]


# ---------------------------------------------------------------------------------------------- the land cover
def value_noise(scale_m, seed, n, cell):
    """Smooth random noise in 0..1 (mean about 0.5) with features about scale_m across, on the level's grid."""
    rs = np.random.RandomState(seed)
    m = int(np.ceil(n * cell / scale_m)) + 3
    grid = rs.random_sample((m, m)).astype(np.float32)
    z = ndi.zoom(grid, n / m + 1e-6, order=3)
    if z.shape[0] < n or z.shape[1] < n:
        z = np.pad(z, ((0, max(0, n - z.shape[0])), (0, max(0, n - z.shape[1]))), mode="edge")
    z = z[:n, :n]
    return np.clip((z - 0.5) * 1.6 + 0.5, 0.0, 1.0).astype(np.float32)


def land_cover(H, SD, wl, maps, channel, port, cfg):
    """make_dorestad.py's land_cover with its constants from the config (see there for the reasoning)."""
    N, CELL = cfg["n"], cfg["cell"]
    C = cfg["cover"]
    S = cfg["settlement"]
    W = cfg["woods"]
    s0 = int(cfg["noise_seed"])
    hw = H - wl
    land = SD <= 0
    dist = np.clip(-SD, 0, None)
    n90, n400, n13 = value_noise(90.0, 11 + s0, N, CELL), value_noise(400.0, 12 + s0, N, CELL), value_noise(13.0, 13 + s0, N, CELL)
    wn90, wn400, wn13 = value_noise(90.0, 21 + s0, N, CELL), value_noise(400.0, 22 + s0, N, CELL), value_noise(13.0, 23 + s0, N, CELL)

    ring = land & (dist >= 25) & (dist <= 35)
    if ring.any():
        idx = ndi.distance_transform_edt(~ring, return_distances=False, return_indices=True)
        hw30 = hw[tuple(idx)]
    else:
        hw30 = hw
    mud_w = np.interp(hw30, C["mud_flat_hw"], C["mud_width"]).astype(np.float32) * (0.55 + 0.9 * n90)
    mud = land & (dist < mud_w) & (hw < C["mud_under"])

    R = C["bend_radius"]
    k = int(2 * R / CELL)
    water = (SD > 0).astype(np.float32)
    share = ndi.uniform_filter(water, size=k, mode="nearest")
    width = 2.0 * ndi.maximum_filter(np.clip(SD, 0, None), size=k, mode="nearest")
    expected = np.clip(width / (2 * R), 0.06, 1.0)
    bend = share / expected
    bar_w = C["bar_width"][0] + C["bar_width"][1] * n90
    bar = land & maps["active"] & ~mud & (bend < C["bend_bar"]) & (dist < bar_w) & (hw < C["bar_under"] + 0.3 * n13)

    low = land & (hw < C["pool_under"]) & (dist > 40)
    lab, nlab = ndi.label(low)
    if nlab:
        area = ndi.sum(np.ones_like(lab, np.float32), lab, index=np.arange(nlab + 1)) * CELL * CELL / 1e4
        pool = (area[lab] >= C["pool_min_ha"]) & low
    else:
        pool = np.zeros_like(low)

    near_river = dist < C["near_river_m"]
    far_land = land & ~near_river
    near_sel = hw[land & near_river]
    far_sel = hw[far_land] if far_land.sum() >= 100 else near_sel
    if near_sel.size < 100:
        near_sel = far_sel if far_sel.size >= 100 else np.array([0.0, 1.0, 2.0], np.float32)
        far_sel = near_sel if far_sel.size < 100 else far_sel
    crest_n, flank_n, marsh_n = np.percentile(near_sel, [C["crest_pct_near"], C["flank_pct_near"], C["marsh_pct_near"]])
    crest_f, flank_f, marsh_f = np.percentile(far_sel, [C["crest_pct_far"], C["flank_pct_far"], C["marsh_pct_far"]])
    print("cover thresholds, m over the water: near the rivers crest %.2f flank %.2f marsh %.2f (p%d/p%d/p%d of the land"
          " within %.0f m); far land crest %.2f flank %.2f marsh %.2f (p%d/p%d/p%d)"
          % (crest_n, flank_n, marsh_n, C["crest_pct_near"], C["flank_pct_near"], C["marsh_pct_near"], C["near_river_m"],
             crest_f, flank_f, marsh_f, C["crest_pct_far"], C["flank_pct_far"], C["marsh_pct_far"]))
    blend = np.clip((dist - C["near_river_m"]) / C["near_blend_m"], 0.0, 1.0).astype(np.float32)
    crest_t = crest_n + (crest_f - crest_n) * blend + 0.2 * (n400 - 0.5) + 0.1 * (n13 - 0.5)
    flank_t = flank_n + (flank_f - flank_n) * blend + 0.2 * (n400 - 0.5) + 0.1 * (n13 - 0.5)
    marsh_t = marsh_n + (marsh_f - marsh_n) * blend + 0.25 * (n90 - 0.5) + 0.1 * (n13 - 0.5)
    marsh = land & ~mud & ~bar & ~pool & (hw < marsh_t)
    high = hw >= crest_t
    mid = (hw >= flank_t) & ~high
    thresholds = {"near": [round(float(v), 3) for v in (crest_n, flank_n, marsh_n)],
                  "far": [round(float(v), 3) for v in (crest_f, flank_f, marsh_f)]}

    cover = np.zeros(H.shape, np.uint8)
    cover[land] = 3
    cover[land & mid] = 2
    cover[land & high & (maps["active"] | near_river)] = 1
    cover[land & high & ~maps["active"] & ~near_river & maps["anybelt"]] = 7
    cover[land & high & ~maps["active"] & ~near_river & ~maps["anybelt"]] = 2
    if C.get("peat_vos") is not None:
        cover[land & (maps["vos"] == C["peat_vos"]) & ~high] = 6
    cover[marsh] = 4
    cover[bar] = 9
    cover[mud] = 8
    cover[pool] = 10

    # the settlement's clearing along the channel from its start, wide on the port's bank, and the extra clearings
    clear = np.zeros(H.shape, bool)
    line_t = np.zeros((2, N, N), np.float32)
    if channel and S["along_m"] > 0:
        c = np.asarray(channel, np.float64)
        s = np.concatenate([[0.0], np.cumsum(np.hypot(*np.diff(c, axis=0).T))])
        if S.get("around_port"):
            # a town by the water: the clearing runs along_m along the channel, centred on the port's nearest point
            s_port = s[int(np.argmin(np.hypot(c[:, 0] - port[0], c[:, 1] - port[1])))]
            c = c[(s >= s_port - S["along_m"] / 2) & (s <= s_port + S["along_m"] / 2)]
        else:
            c = c[s <= S["along_m"]]
        for i in range(len(c) - 1):
            a, b = c[i], c[i + 1]
            seg = (b - a) / max(1e-6, np.hypot(*(b - a)))
            for t in np.linspace(0, 1, max(2, int(np.hypot(*(b - a)) / CELL) + 1)):
                p = a + (b - a) * t
                xi, zi = int(p[0] / CELL), int(p[1] / CELL)
                if 0 <= xi < N and 0 <= zi < N:
                    clear[zi, xi] = True
                    line_t[:, zi, xi] = seg
    jj, ii = np.meshgrid(np.arange(N) * CELL, np.arange(N) * CELL)
    if clear.any():
        d_line, idx = ndi.distance_transform_edt(~clear, return_distances=True, return_indices=True)
        d_line = d_line * CELL
        tx, tz = line_t[0][tuple(idx)], line_t[1][tuple(idx)]
        vx, vz = jj - idx[1] * CELL, ii - idx[0] * CELL
        cross = tx * vz - tz * vx
        pi_, pj_ = min(N - 1, max(0, int(port[1] / CELL))), min(N - 1, max(0, int(port[0] / CELL)))
        town_sign = 1.0 if cross[pi_, pj_] >= 0 else -1.0
        town_side = np.sign(cross) == town_sign
        width = np.where(town_side, S["wide_m"], S["far_m"]).astype(np.float32)
    else:
        d_line = np.full(H.shape, 1e9, np.float32)
        width = np.full(H.shape, S["wide_m"], np.float32)
    rest = None
    for cl in reversed(S["clearings"]):
        if "from_port" in cl:
            cx, cz = port[0] + cl["from_port"][0], port[1] + cl["from_port"][1]
        else:
            cx, cz = cl["at"][0], cl["at"][1]
        d = np.hypot(jj - cx, ii - cz) - cl["r"]
        rest = d if rest is None else np.minimum(d, rest)
    beyond = d_line - width if rest is None else np.minimum(d_line - width, rest)
    cleared = np.clip(beyond / S["fade_m"], 0.0, 1.0).astype(np.float32)

    potential = 0.55 * wn400 + 0.30 * wn90 + 0.15 * wn13
    basin = np.isin(cover, (3, 4, 6))
    highland = np.isin(cover, (1, 2, 7))
    canopy = 0.42 + 0.45 * 0.3
    open_land = cleared > 0.9

    def bonus(mask, share):
        sel = mask & open_land
        if sel.sum() < 100:
            return 0.0
        return float(canopy - np.percentile(potential[sel], 100.0 * (1.0 - share)))
    basin_bonus = bonus(np.isin(cover, (3, 6)), W["basin_share"])
    high_bonus = bonus(highland, W["high_share"])
    print("wood bonus: basin %+.3f (for %.0f%% carr), high land %+.3f (for %.0f%% wood)"
          % (basin_bonus, 100 * W["basin_share"], high_bonus, 100 * W["high_share"]))
    wood = np.where(basin, potential + basin_bonus, potential + high_bonus)
    wood = wood * cleared
    wood[np.isin(cover, (8, 9, 10, 0))] = 0.0
    wood[cover == 4] *= 0.35
    wood[dist < W["shore_bare_m"]] = 0.0
    density = np.clip((wood - 0.42) / 0.3, 0.0, 1.0).astype(np.float32)
    density = ndi.gaussian_filter(density, 2.0)
    cover[(density > 0.45) & np.isin(cover, (3, 4, 6))] = 5
    cover[(density > 0.45) & np.isin(cover, (1, 2, 7))] = 11
    cover[~land] = 0
    carr_able = np.isin(cover, (3, 5, 6)) & open_land
    wooded_basin = float(((density > 0.45) & carr_able).sum() / max(1, carr_able.sum()))
    print("wood: %.0f%% of the open-country basin land under carr, %.0f%% of the open-country high land under wood"
          % (100 * wooded_basin, 100 * float(((density > 0.45) & highland & open_land).sum() / max(1, (highland & open_land).sum()))))
    thresholds["wood_bonus"] = [round(basin_bonus, 3), round(high_bonus, 3)]
    return cover, density, bend, thresholds


def coast_cover(cover, wood, H, SD, wl, sea, cmaps, tide, cfg):
    """The coast's classes over the river cover: salt marsh where the spring tides reach the tidal marsh, tidal flat
    under the water down to low water, and on a sandy coast the beach and the dunes. Returns (cover, wood, notes)."""
    K = cfg["coast"]
    CELL = cfg["cell"]
    land = SD <= 0
    hw = H - wl
    notes = []
    if tide:
        top = tide["hw"] - wl + K["saltmarsh_over_hw"]
        salt = land & cmaps["tidal_marsh"] & (hw < top)
        cover[salt] = 14
        wood[salt] = 0.0
        flat = (SD > 0) & (H >= tide["lw"])
        cover[flat] = 15
        notes.append("salt marsh: the tidal marsh under %.2f m (high water %.2f + %.2f); tidal flat: the water's bed "
                     "over low water (%.2f m)" % (tide["hw"] + K["saltmarsh_over_hw"], tide["hw"], K["saltmarsh_over_hw"], tide["lw"]))
    if sea is not None and sea.any():
        dsea = ndi.distance_transform_edt(~sea) * CELL
        top_hw = (tide["hw"] if tide else wl) - wl
        dunes = land & cmaps["dunes"] & (dsea < K["dune_reach_m"])
        beach = land & cmaps["sandy"] & (dsea < K["beach_width_m"]) & (hw < top_hw + K["beach_top"])
        cover[dunes & ~beach] = 13
        wood[dunes & ~beach] *= K["dune_wood"]
        cover[beach] = 12
        wood[beach] = 0.0
        notes.append("beach: sandy coast within %.0f m of the sea and under %.1f m over high water; dunes: the dune "
                     "units within %.0f m of the sea, scrub only" % (K["beach_width_m"], K["beach_top"], K["dune_reach_m"]))
    return cover, wood, notes


# ---------------------------------------------------------------------------------------------- the anchors
def arc(c):
    """Arc length at each vertex of a polyline."""
    return np.concatenate([[0.0], np.cumsum(np.hypot(*np.diff(c, axis=0).T))])


def point_at(c, s, t):
    """The point at arc length t (clamped to the line)."""
    t = min(max(t, 0.0), s[-1])
    return np.array([np.interp(t, s, c[:, 0]), np.interp(t, s, c[:, 1])])


def smooth_line(a, step, sigma_m):
    """A polyline resampled every `step` metres and smoothed with a gaussian of sigma_m (its ends held)."""
    s = arc(a)
    t = np.arange(0.0, s[-1] + 1e-9, step)
    p = np.stack([np.interp(t, s, a[:, 0]), np.interp(t, s, a[:, 1])], axis=1)
    if sigma_m > 0 and len(p) > 3:
        p = np.stack([ndi.gaussian_filter1d(p[:, 0], sigma_m / step, mode="nearest"),
                      ndi.gaussian_filter1d(p[:, 1], sigma_m / step, mode="nearest")], axis=1)
    return p


def norm_tide(t):
    """A tide as {"hw", "lw"} (the source's datum) from {"hw", "lw"}, {"hw", "range"} or {"mean", "range"}."""
    if not t:
        return None
    t = dict(t)
    if "lw" not in t:
        if "hw" in t:
            t["lw"] = t["hw"] - t["range"]
        else:
            t["hw"], t["lw"] = t["mean"] + t["range"] / 2.0, t["mean"] - t["range"] / 2.0
    elif "hw" not in t:
        t["hw"] = t["lw"] + t["range"]
    t["hw"], t["lw"] = round(float(t["hw"]), 3), round(float(t["lw"]), 3)
    return t


def deep_path(H, SD, wl, cell, a, b, coarse=8, shallow=1.0):
    """The channel between two points (level metres) through the water, preferring deep water: the cheapest
    8-neighbour path on a coarse grid where a step costs its length x (1 + shallow / depth). For estuaries and
    coasts where no centre line exists. Returns a polyline in level metres (empty if the points are not joined)."""
    from scipy.sparse import coo_matrix
    from scipy.sparse.csgraph import dijkstra
    n = H.shape[0] // coarse
    Hc = H[:n * coarse, :n * coarse].reshape(n, coarse, n, coarse).mean(axis=(1, 3))
    Wc = (SD[:n * coarse, :n * coarse] > 0).reshape(n, coarse, n, coarse).mean(axis=(1, 3)) > 0.5
    depth = np.clip(wl - Hc, 0.1, None)
    cost = np.where(Wc, 1.0 + shallow / depth, np.inf)
    idx = np.arange(n * n).reshape(n, n)
    rows, cols, vals = [], [], []
    for di, dj in ((0, 1), (1, 0), (1, 1), (1, -1)):
        i0, i1 = max(0, -di), n - max(0, di)
        j0, j1 = max(0, -dj), n - max(0, dj)
        A = idx[i0:i1, j0:j1]
        B = idx[i0 + di:i1 + di, j0 + dj:j1 + dj]
        c = 0.5 * (cost[i0:i1, j0:j1] + cost[i0 + di:i1 + di, j0 + dj:j1 + dj]) * math.hypot(di, dj)
        ok = np.isfinite(c)
        rows += [A[ok], B[ok]]
        cols += [B[ok], A[ok]]
        vals += [c[ok], c[ok]]
    G = coo_matrix((np.concatenate(vals), (np.concatenate(rows), np.concatenate(cols))), shape=(n * n, n * n)).tocsr()
    cc = cell * coarse

    def node(p):
        i, j = int(p[1] / cc), int(p[0] / cc)
        i, j = min(n - 1, max(0, i)), min(n - 1, max(0, j))
        if not Wc[i, j]:                                # the nearest water node
            ii, jj = np.nonzero(Wc)
            k = int(np.argmin((ii - i) ** 2 + (jj - j) ** 2))
            i, j = ii[k], jj[k]
        return idx[i, j]
    s, t = node(a), node(b)
    dist, pred = dijkstra(G, indices=s, return_predecessors=True)
    if not np.isfinite(dist[t]):
        return []
    path = [t]
    while path[-1] != s:
        path.append(pred[path[-1]])
    path = path[::-1]
    return [[(k % n + 0.5) * cc, (k // n + 0.5) * cc] for k in path]


def heading_of(dx, dz):
    return (math.degrees(math.atan2(dx, dz)) + 360.0) % 360.0


def anchors_for(cfg, channel, H, SD, wl, X0, Y0, epsg, port):
    """The anchors (where the chain joins the place), in level metres, from chain.json and the config. A config
    anchor may give "pos" (level metres), "src" (the source CRS), "latlon", or "at": "channel_start" /
    "channel_end" with "inset_m" (metres along the channel inside its end); "heading": a number (degrees from
    north, OUTWARD as chain.json has it) or "auto" (along the channel, out of the place)."""
    N, CELL = cfg["n"], cfg["cell"]
    found = {}
    if cfg.get("anchors_from_chain"):
        try:
            with open(CHAIN, encoding="utf-8") as f:
                chain = json.load(f)
            for lv in chain.get("levels", []):
                if lv.get("id") == cfg["anchors_from_chain"]:
                    for a in lv.get("anchors", []):
                        if "pos" in a:
                            found[a["id"]] = {k: a[k] for k in ("id", "kind", "pos", "heading", "arrive", "port") if k in a}
                            found[a["id"]]["from"] = "chain.json"
        except OSError as e:
            print("chain.json not read:", e)
    ch = np.asarray(channel, np.float64) if channel else None
    for a in cfg.get("anchors", []):
        out = dict(found.get(a["id"], {}))
        out.update({k: v for k, v in a.items() if k not in ("src", "latlon", "at", "inset_m")})
        out["from"] = "config"
        hdg = a.get("heading")
        if "src" in a:
            out["pos"] = [a["src"][0] - X0, a["src"][1] - Y0]
        elif "latlon" in a:
            x, y, _ = transform(4326, epsg).TransformPoint(a["latlon"][1], a["latlon"][0])
            out["pos"] = [x - X0, y - Y0]
        elif a.get("at") == "nearest_water":
            # the nearest water at least min_depth deep to the port (or to "near": level metres): a berth
            near = a.get("near", port)
            deep = (SD > 0) & ((wl - H) >= float(a.get("min_depth", 1.5)))
            if deep.any():
                jj_, ii_ = np.nonzero(deep.T)
                k = int(np.argmin(np.hypot(jj_ * CELL - near[0], ii_ * CELL - near[1])))
                out["pos"] = [float(jj_[k] * CELL), float(ii_[k] * CELL)]
        elif a.get("at") in ("channel_start", "channel_end") and ch is not None and len(ch) > 1:
            s_ch = arc(ch)
            inset = float(a.get("inset_m", 60.0))
            s_at = inset if a["at"] == "channel_start" else s_ch[-1] - inset
            out["pos"] = [float(v) for v in point_at(ch, s_ch, s_at)]
            if hdg in (None, "auto"):
                hdg = "to_start" if a["at"] == "channel_start" else "to_end"
        if "pos" not in out:
            print("anchor %s has no position: left out" % a["id"])
            continue
        if hdg in ("auto", "to_start", "to_end") and ch is not None and len(ch) > 1:
            # along the channel over a chord of chord_m, toward its start or end ('auto': the nearer end, outward)
            s_ch = arc(ch)
            p = np.asarray(out["pos"])
            s_at = s_ch[int(np.argmin(np.hypot(ch[:, 0] - p[0], ch[:, 1] - p[1])))]
            if hdg == "auto":
                hdg = "to_end" if s_ch[-1] - s_at < s_at else "to_start"
            half = float(a.get("chord_m", 80.0)) / 2
            d = point_at(ch, s_ch, s_at + half) - point_at(ch, s_ch, s_at - half)
            if hdg == "to_start":
                d = -d
            hdg = heading_of(d[0], d[1])
        if isinstance(hdg, (int, float)):
            out["heading"] = round(float(hdg), 1)
        found[a["id"]] = out
    res = []
    for a in found.values():
        x, z = a["pos"]
        j, i = min(N - 1, max(0, int(round(x / CELL)))), min(N - 1, max(0, int(round(z / CELL))))
        a["pos"] = [round(float(x), 2), round(float(z), 2)]
        a["waterLevel"] = round(wl, 3)
        a["depth"] = round(float(wl - H[i, j]), 2)
        a["on_water"] = bool(SD[i, j] > 0)
        a["water_sd"] = round(float(SD[i, j]), 1)
        if not a["on_water"]:
            print("WARNING: anchor %s is on land (%.1f m from the water)" % (a["id"], -SD[i, j]))
        res.append(a)
    return res


# ---------------------------------------------------------------------------------------------- the bake
def bake(cfg, out, preview=True):
    src = SOURCES[cfg["source"]](cfg)
    EPSG = src.epsg
    N, CELL = int(cfg["n"]), float(cfg["cell"])
    SIZE = (N - 1) * CELL
    CENTER = tuple(cfg["centre"])
    X0, Y0 = CENTER[0] - SIZE / 2, CENTER[1] - SIZE / 2
    MARGIN = float(cfg["margin"])
    SH = cfg["shore"]
    rivers = cfg["rivers"]
    sea_cfg = cfg.get("sea")
    tide = norm_tide(cfg.get("tide"))
    names = [r["name"] for r in rivers]
    os.makedirs(out, exist_ok=True)

    win = src.window(X0, Y0, SIZE, MARGIN)
    wx0, wy1, cs = win.wx0, win.wy1, win.cell
    h10, cls, wt10 = src.read(win)
    maps10 = src.maps(win, cls)

    # the water bodies' own masks at 10 m (the sea, if any, is one more body, last but one in priority: after the
    # rivers named before it in "rivers", before the default)
    masks = [src.mask(r["mask"], win, cls) for r in rivers]
    sea_idx = next((k for k, r in enumerate(rivers) if r.get("kind") == "sea"), None)
    notes = []

    # land the raster has nothing for: today's ground from the outer source (a guess, capped), then the sea's bed
    nan0 = ~np.isfinite(h10)
    h_today = None
    if cfg.get("outer") and nan0.any():
        o = cfg["outer"]
        h_out = src.outer(win, o)
        h_today = h_out.copy()                          # kept for the sea's datum ("match_outer")
        if o.get("median_m"):
            k = max(3, int(o["median_m"] / cs) | 1)
            good = np.isfinite(h_out)
            fill = h_out.copy()
            if (~good).any() and good.any():
                idx = ndi.distance_transform_edt(~good, return_distances=False, return_indices=True)
                fill = h_out[tuple(idx)]
            h_out = np.where(good, ndi.median_filter(fill, size=k), np.nan).astype(np.float32)
        if o.get("cap_m") is not None:
            h_out = np.minimum(h_out, float(o["cap_m"]))
        if sea_idx is not None:
            h_out[masks[sea_idx]] = np.nan                  # the 800 sea takes the bed, not today's beach
        take = nan0 & np.isfinite(h_out)
        h10 = np.where(take, h_out, h10).astype(np.float32)
        bl = float(o.get("blend_m", 200.0))
        if bl > 0 and take.any():
            d_in = ndi.distance_transform_edt(~take) * cs   # metres into the raster's own ground
            a = np.clip(d_in / bl, 0.0, 1.0)
            mix = (~take) & np.isfinite(h_out) & np.isfinite(h10) & (a < 1)
            h10 = np.where(mix, a * h10 + (1 - a) * h_out, h10).astype(np.float32)
        notes.append("%.0f%% of the window from today's ground (%s)%s" % (
            100 * take.mean(), o.get("about", "the outer source"),
            ", capped at %.1f m" % o["cap_m"] if o.get("cap_m") is not None else ""))
    if sea_cfg:
        e, tiles = emodnet_on(win, EPSG)
        dat = sea_cfg.get("datum", {"rule": "fixed", "m": 0.0})
        if dat["rule"] == "fixed":
            off = float(dat["m"])
        elif dat["rule"] == "thames_lat_odn":
            xs = wx0 + (np.arange(win.w) + 0.5) * cs
            off = np.interp(xs, [t[0] for t in Thames793.LAT_ODN], [t[1] for t in Thames793.LAT_ODN]).astype(np.float32)[None, :]
        elif dat["rule"] == "match_outer":
            # EMODnet's datum is LAT where the tide is large and near MSL elsewhere: match it to today's ground
            # (moved to the raster's datum) where both have data and the ground is between band[0] and band[1] m
            ref = h_today if h_today is not None else h10
            lo, hi = dat.get("band", [0.0, 3.0])
            both = np.isfinite(e) & np.isfinite(ref) & (ref >= lo) & (ref < hi)
            off = float(np.median(ref[both] - e[both])) if both.sum() >= 50 else float(dat.get("m", 0.0))
            dat = dict(dat, m=round(off, 2), cells=int(both.sum()))
            print("sea datum: EMODnet + %.2f m (median over %d cells of today's ground %.1f..%.1f m)" % (off, both.sum(), lo, hi))
        else:
            raise ValueError("sea datum rule %r" % dat["rule"])
        e = e + off
        nan_now = ~np.isfinite(h10)
        where_sea = nan_now & np.isfinite(e)
        if sea_cfg.get("only_in_sea", True) and sea_idx is not None:
            where_sea &= masks[sea_idx]
        h10 = np.where(where_sea, e, h10).astype(np.float32)
        notes.append("the sea's bed from EMODnet DTM 2024 (%s), its datum moved by %s m, on %.0f%% of the window"
                     % (", ".join(tiles) or "no tile", dat.get("m", "the LAT-ODN table"), 100 * where_sea.mean()))

    nan = ~np.isfinite(h10)
    idx = ndi.distance_transform_edt(nan, return_distances=False, return_indices=True)
    hf = h10[tuple(idx)]

    # the water: the bodies' masks, or the ground under the level joined to them
    def signed(water):
        din = ndi.distance_transform_edt(water) * cs
        dout = ndi.distance_transform_edt(~water) * cs
        return np.where(water, din - cs / 2, -(dout - cs / 2)).astype(np.float32)

    WL = cfg["water_level"]
    if cfg["water_mask"] == "masks":
        water = masks[0]
        for m in masks[1:]:
            water = water | m
        sd10 = signed(water)
    if WL["rule"] == "bank_ring":
        if cfg["water_mask"] != "masks":
            raise ValueError("water_level 'bank_ring' needs water_mask 'masks'")
        ring = (sd10 < -WL["ring_m"][0]) & (sd10 > -WL["ring_m"][1]) & ~nan
        wl = float(np.percentile(h10[ring], WL["pct"]))
        print("bank ring: p5 %.2f p20 %.2f p50 %.2f p80 %.2f m" % tuple(np.percentile(h10[ring], [5, 20, 50, 80])))
    elif WL["rule"] == "fixed":
        wl = float(WL["m"])
    elif WL["rule"] == "tide_mean":
        wl = float((tide["hw"] + tide["lw"]) / 2.0)
    else:
        raise ValueError("water_level rule %r" % WL["rule"])
    if cfg["water_mask"] == "below_level":
        seeds = masks[0]
        for m in masks[1:]:
            seeds = seeds | m
        lab, _ = ndi.label(hf < wl)
        keep = np.unique(lab[seeds & (lab > 0)])
        water = seeds | (np.isin(lab, keep[keep > 0]))
        # each new water cell belongs to the body whose own mask is nearest
        owner = np.full(water.shape, -1, np.int16)
        for k, m in enumerate(masks):
            owner[m & (owner < 0)] = k
        if (owner >= 0).any():
            idx2 = ndi.distance_transform_edt(owner < 0, return_distances=False, return_indices=True)
            owner = owner[tuple(idx2)]
        masks = [water & (owner == k) for k in range(len(masks))]
        sd10 = signed(water)
    for k, m in enumerate(masks):
        if (m & ~nan).any():
            print("%s bed p10/p50/p90 %.2f %.2f %.2f m" % ((names[k],) + tuple(np.percentile(h10[m & ~nan], [10, 50, 90]))))
    print("water level %.2f m" % wl)

    # to the level's grid (row 0 south)
    jj, ii = np.meshgrid(np.arange(N), np.arange(N))
    x = X0 + jj * CELL
    y = Y0 + ii * CELL
    cx = (x - wx0) / cs - 0.5
    cy = (wy1 - y) / cs - 0.5
    H = ndi.map_coordinates(hf, [cy, cx], order=3, mode="nearest").astype(np.float32)
    SD = ndi.map_coordinates(sd10, [cy, cx], order=1, mode="nearest").astype(np.float32)
    # which body each water cell is: the first in "rivers" whose mask covers it, else the last
    PRI = [ndi.map_coordinates(m.astype(np.float32), [cy, cx], order=1, mode="nearest") > 0.5 for m in masks[:-1]]
    CLS = ndi.map_coordinates(cls.astype(np.float32), [cy, cx], order=0, mode="nearest").astype(np.uint8)
    SD = ndi.gaussian_filter(SD, SH["sd_blur"])

    def nearest(a):
        return ndi.map_coordinates(a.astype(np.float32), [cy, cx], order=0, mode="nearest")
    maps = {
        "vos": nearest(maps10["vos"]).astype(np.uint8),
        "active": nearest(maps10["active"]) > 0.5,
        "anybelt": nearest(maps10["anybelt"]) > 0.5,
        "belt": nearest(maps10["belt"]).astype(np.uint16),
    }

    def per_body(key, default=None):
        v = np.full(SD.shape, rivers[-1].get(key, default))
        for k in reversed(range(len(rivers) - 1)):
            v = np.where(PRI[k], rivers[k].get(key, default), v)
        return v
    DEPTH = per_body("depth")
    ramps = {float(r.get("ramp_m", SH["depth_ramp"])) for r in rivers}
    RAMP = ramps.pop() if len(ramps) == 1 else per_body("ramp_m", SH["depth_ramp"]).astype(np.float32)
    EDGE = SH["edge_depth"]
    RISE = SH["bank_rise"] or 0.0

    def shape(H):
        inside = SD > 0
        t = np.clip(SD / RAMP, 0, 1)
        t = t * t * (3 - 2 * t)
        bed = wl - (EDGE + (DEPTH - EDGE) * t)
        H = np.where(inside, np.minimum(H, bed), H)
        if RISE <= 0:
            return H
        near = (SD <= 0) & (SD > -RISE)
        bank = wl + SH["bank_lift"] + SH["bank_slope"] * (-SD)
        return np.where(near, np.maximum(H, bank), H)

    H = shape(H)
    H = ndi.gaussian_filter(H, SH["height_blur"])
    H = shape(H).astype(np.float32)
    water_f = SD > 0
    CLS = np.where(water_f, per_body("class"), CLS).astype(np.uint8)

    # the port and the channel's centre line, in the level's frame
    port_cfg = cfg.get("port")
    if port_cfg:
        px, py, _ = transform(4326, EPSG).TransformPoint(port_cfg["latlon"][1], port_cfg["latlon"][0])
    else:                                           # a place with no port: the centre stands in, and no settlement
        px, py = CENTER
        cfg["settlement"]["along_m"] = 0.0
    port = [px - X0, py - Y0]
    chc = cfg["channel"]
    if "line" in chc:
        lines = [np.asarray(chc["line"], np.float64)]
    elif "path" in chc:
        # the deep-water path between two points (source CRS), when no centre line exists (estuaries, coasts)
        pa = [chc["path"][0][0] - X0, chc["path"][0][1] - Y0]
        pb = [chc["path"][1][0] - X0, chc["path"][1][1] - Y0]
        dp = deep_path(H, SD, wl, CELL, pa, pb)
        lines = [np.asarray(dp, np.float64) + [X0, Y0]] if dp else []
        print("deep-water path: %d nodes" % len(dp))
    else:
        lines = src.channel_lines(chc, (X0, Y0, X0 + SIZE, Y0 + SIZE))
    near_pt = (px, py) if chc.get("pick", "nearest_port") == "nearest_port" else tuple(chc["pick_near"])
    best = None
    for a in lines:
        inb = (a[:, 0] > X0) & (a[:, 0] < X0 + SIZE) & (a[:, 1] > Y0) & (a[:, 1] < Y0 + SIZE)
        if not inb.any():
            continue
        a = a[inb]
        d = np.hypot(a[:, 0] - near_pt[0], a[:, 1] - near_pt[1]).min()
        if best is None or d < best[0]:
            best = (d, a)
    channel = []
    if best is not None:
        a = best[1]
        if chc.get("smooth_m"):                     # a stepped line (GRWL's 30 m pixels) resampled and smoothed
            a = smooth_line(a, chc.get("min_step_m", 10.0), float(chc["smooth_m"]))
        keep = [0]
        for k in range(1, len(a)):
            if np.hypot(*(a[k] - a[keep[-1]])) >= chc.get("min_step_m", 10.0):
                keep.append(k)
        channel = [[round(float(p[0] - X0), 2), round(float(p[1] - Y0), 2)] for p in a[keep]]
        print("%s line: %d points, nearest %.0f m from the port" % (chc.get("name", "channel"), len(channel), best[0]))
    else:
        print("WARNING: no channel line in the box")

    # the settlement walks the channel from its start: the end nearer a named body ("near_body"), or the port
    from_start = list(channel)
    wf = chc.get("walk_from", {})
    if from_start and wf:
        if "near_body" in wf:
            k = names.index(wf["near_body"])
            body = PRI[k] if k < len(PRI) else water_f & ~np.any(PRI, axis=0) if PRI else water_f
            d_b = ndi.distance_transform_edt(~body) * CELL
        else:
            d_b = np.hypot(jj * CELL - port[0], ii * CELL - port[1])

        def dl(p):
            return d_b[min(N - 1, max(0, int(p[1] / CELL))), min(N - 1, max(0, int(p[0] / CELL)))]
        if dl(from_start[-1]) < dl(from_start[0]):
            from_start.reverse()
    COVER_U8, WOOD, bend, thresholds = land_cover(H, SD, wl, maps, from_start, port, cfg)

    cover_names = dict(COVER)
    coastal = bool(tide) or sea_idx is not None
    if coastal:
        cover_names.update(COAST_COVER)
        cm10 = src.coast_maps(win, cls)
        cmaps = {k: nearest(v) > 0.5 for k, v in cm10.items()}
        sea_grid = None
        if sea_idx is not None:
            if sea_idx < len(PRI):
                sea_grid = water_f & PRI[sea_idx] & ~np.any(PRI[:sea_idx], axis=0) if sea_idx else water_f & PRI[0]
            else:
                sea_grid = water_f & ~np.any(PRI, axis=0) if PRI else water_f
        COVER_U8, WOOD, cnotes = coast_cover(COVER_U8, WOOD, H, SD, wl, sea_grid, cmaps, tide, cfg)
        notes += cnotes
    for k, v in cfg.get("cover_names", {}).items():
        cover_names[int(k)] = v

    land = SD <= 0
    hw_land = (H - wl)[land]
    if hw_land.size:
        print("land over the water: p10 %.2f p25 %.2f p50 %.2f p75 %.2f p90 %.2f m" % tuple(np.percentile(hw_land, [10, 25, 50, 75, 90])))
    cover_stats = {}
    for k, name in cover_names.items():
        share = float((COVER_U8 == k).mean())
        cover_stats[str(k)] = round(share, 4)
        print("  cover %2d %5.1f%%  %s" % (k, 100 * share, name.split(":")[0]))
    if land.any():
        print("wood: %.1f%% of the land over 0.5 density" % (100 * float((WOOD[land] > 0.5).mean())))

    H.tofile(os.path.join(out, "height.f32"))
    SD.astype(np.float32).tofile(os.path.join(out, "water_sd.f32"))
    CLS.tofile(os.path.join(out, "class.u8"))
    COVER_U8.tofile(os.path.join(out, "cover.u8"))
    np.clip(WOOD * 255.0 + 0.5, 0, 255).astype(np.uint8).tofile(os.path.join(out, "wood.u8"))
    clon, clat, _ = transform(EPSG, 4326).TransformPoint(*CENTER)
    S = cfg["settlement"]
    C = cfg["cover"]
    W = cfg["woods"]
    guesses = []
    if channel and S["along_m"] > 0:
        guesses.append("the settlement's clearing: %.0f m along the %s from the %s, %.0f m wide on the town's bank, %.0f m "
                       "on the far bank, fading out over %.0f m" % (S["along_m"], chc.get("name", "channel"),
                                                                    chc.get("start_name", "start"), S["wide_m"], S["far_m"], S["fade_m"]))
    for cl in S["clearings"]:
        if "from_port" in cl:
            guesses.append("%s: %.0f m radius at (%.0f, %.0f) m from the port" % (cl["label"], cl["r"], cl["from_port"][0], cl["from_port"][1]))
        else:
            guesses.append("%s: %.0f m radius at (%.0f, %.0f) m" % (cl["label"], cl["r"], cl["at"][0], cl["at"][1]))
    guesses += [
        "the woods' patch shapes and shares (%.0f%% of the basin land beyond the clearings under carr, %.0f%% of the far high land under wood)" % (100 * W["basin_share"], 100 * W["high_share"]),
        "the crest/flank/marsh thresholds as ranks of the land's height over the water: p%d/p%d/p%d within %.0f m of the rivers = %.2f/%.2f/%.2f m, p%d/p%d/p%d of the far land = %.2f/%.2f/%.2f m (with noise)"
        % (C["crest_pct_near"], C["flank_pct_near"], C["marsh_pct_near"], C["near_river_m"], *thresholds["near"], C["crest_pct_far"], C["flank_pct_far"], C["marsh_pct_far"], *thresholds["far"]),
        "mud width 2-26 m by the shore's flatness; sand bars inside bends in the active belts; pools in hollows over %.1f ha" % C["pool_min_ha"],
    ]
    guesses += cfg.get("guesses", [])
    meta = {
        "name": cfg["name"],
        "about": cfg["about"],
        "crs": "EPSG:%d" % EPSG,
        "origin_rd" if EPSG == 28992 else "origin": [X0, Y0],
        "cell": CELL,
        "n": N,
        "size": SIZE,
        "water_level": round(wl, 3),
        "height_min": round(float(H.min()), 3),
        "height_max": round(float(H.max()), 3),
        "center_latlon": [round(clat, 6), round(clon, 6)],
        "port": [round(port[0], 2), round(port[1], 2)],
        "channel": channel,
        "water_share": round(float(water_f.mean()), 4),
        "cover_file": "cover.u8: uint8 (n, n), row 0 the south edge, the classes in cover_classes",
        "cover_classes": {str(k): v for k, v in cover_names.items()},
        "cover_share": cover_stats,
        "cover_thresholds": thresholds,
        "wood_file": "wood.u8: uint8 (n, n), how thickly wooded, 0 open to 255 a closed canopy (continuous: the woods thin at their edges)",
        "cover_guesses": guesses,
        "sources": cfg.get("sources") or src.credits,
    }
    # what make_place.py adds to make_dorestad.py's meta
    meta["place"] = cfg["id"]
    meta["origin"] = [X0, Y0]
    meta["source"] = src.name
    meta["class_names"] = {str(k): v for k, v in src.class_names.items()}
    meta["water_bodies"] = [{"name": r["name"], "kind": r.get("kind", "river"), "class": r["class"], "depth": r["depth"]}
                            for r in rivers]
    for r in rivers:
        meta["class_names"][str(r["class"])] = "water: " + r["name"]
    meta["water_level_rule"] = WL
    if tide:
        meta["tide"] = tide
    meta["anchors"] = anchors_for(cfg, channel, H, SD, wl, X0, Y0, EPSG, port)
    meta["bake_notes"] = notes
    meta["bake"] = {"script": "Bake/levels/make_place.py", "config": os.path.relpath(cfg["_path"], os.path.join(HERE, "..", ".."))}
    with open(os.path.join(out, "meta.json"), "w") as f:
        json.dump(meta, f, indent=1)
    print("height %.2f..%.2f m, water %.1f%% of the level" % (H.min(), H.max(), 100 * water_f.mean()))
    for a in meta["anchors"]:
        print("anchor %-8s (%7.1f, %7.1f) heading %s, depth %.2f m, %s" % (a["id"], a["pos"][0], a["pos"][1],
              a.get("heading", "-"), a["depth"], "on the water" if a["on_water"] else "ON LAND"))
    for n_ in notes:
        print("note:", n_)

    if not preview:
        print("written to", out)
        return
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from matplotlib.colors import LightSource, ListedColormap, BoundaryNorm
    from matplotlib.patches import Patch
    fig, ax = plt.subplots(2, 2, figsize=(20, 19))
    ls = LightSource(azdeg=315, altdeg=35)
    rgb = ls.shade(H, cmap=plt.cm.gist_earth, vert_exag=6, blend_mode="soft", vmin=wl - 5, vmax=max(wl + 4, float(np.percentile(H, 98))))
    ax[0, 0].imshow(rgb, origin="lower", extent=(0, SIZE, 0, SIZE))
    ax[0, 0].contour(np.arange(N) * CELL, np.arange(N) * CELL, SD, levels=[0], colors="cyan", linewidths=0.6)
    if channel:
        c = np.asarray(channel)
        ax[0, 0].plot(c[:, 0], c[:, 1], "r-", lw=0.8)
    if port_cfg:
        ax[0, 0].plot(*port, "y*", ms=14)
    for a in meta["anchors"]:
        ax[0, 0].plot(*a["pos"], "m^", ms=10)
        if "heading" in a:
            r = math.radians(a["heading"])
            ax[0, 0].arrow(a["pos"][0], a["pos"][1], 150 * math.sin(r), 150 * math.cos(r), color="m", width=12)
        ax[0, 0].annotate(a["id"], a["pos"], color="m", fontsize=10)
    ax[0, 0].set_title("%s level: ground c. 800 (hillshade x6), water edge (cyan), %s line (red), port, anchors (magenta)"
                       % (cfg["name"], chc.get("name", "channel")))
    im = ax[0, 1].imshow(H - wl, origin="lower", extent=(0, SIZE, 0, SIZE), cmap="RdYlBu_r", vmin=-5, vmax=4)
    plt.colorbar(im, ax=ax[0, 1], shrink=0.8, label="m over the water level (%.2f m)" % wl)
    ax[0, 1].set_title("height over the water level")
    ncls = max(cover_names) + 1
    cm = ListedColormap(COVER_COLOURS[:ncls])
    norm = BoundaryNorm(np.arange(-0.5, ncls + 0.5, 1), cm.N)
    ax[1, 0].imshow(COVER_U8, origin="lower", extent=(0, SIZE, 0, SIZE), cmap=cm, norm=norm, interpolation="nearest")
    if port_cfg:
        ax[1, 0].plot(*port, "k*", ms=12)
    ax[1, 0].legend(handles=[Patch(color=COVER_COLOURS[k], label="%d %s" % (k, cover_names[k].split(":")[0])) for k in cover_names],
                    loc="lower left", fontsize=8, framealpha=0.85)
    ax[1, 0].set_title("land cover c. 800 (cover.u8)")
    im = ax[1, 1].imshow(np.where(land, WOOD, np.nan), origin="lower", extent=(0, SIZE, 0, SIZE), cmap="Greens", vmin=0, vmax=1)
    plt.colorbar(im, ax=ax[1, 1], shrink=0.8, label="wood density (wood.u8 / 255)")
    ax[1, 1].contour(np.arange(N) * CELL, np.arange(N) * CELL, bend, levels=[cfg["cover"]["bend_bar"]], colors="orange", linewidths=0.5)
    if port_cfg:
        ax[1, 1].plot(*port, "k*", ms=12)
    ax[1, 1].set_title("woods (green), and the inside of the bends (orange: where the bars grow)")
    fig.tight_layout()
    fig.savefig(os.path.join(out, "preview.png"), dpi=80)
    print("written to", out)


def main(argv):
    if len(argv) < 2 or argv[1] in ("-h", "--help"):
        print(__doc__)
        return 1
    cfg = load_config(argv[1])
    out = cfg["out"]
    if "--out" in argv:
        out = argv[argv.index("--out") + 1]
    if os.path.normcase(os.path.abspath(out)) in PROTECTED and "--write-protected" not in argv:
        print("refused: %s belongs to another job (pass --write-protected to write it on purpose, or --out elsewhere)" % out)
        return 2
    print("place %s (%s) from %s -> %s" % (cfg["id"], cfg["name"], cfg["source"], out))
    bake(cfg, out, preview="--no-preview" not in argv)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
