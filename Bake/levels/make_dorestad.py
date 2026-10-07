"""DORESTAD, THE FIRST RIVER PLACE - the level's ground, water and land cover at true size (1:1), from the c. 800 ground.

Reads the Netherlands c. 800 raster the first build made from AHN4 lidar (Bake/realground/nl_793.py in the first
build: today's dikes, canal, town and works taken off, the 800 Kromme Rijn cut along the Vos line) and the Vos 800 AD
map's inland water (the 800 Lek), and writes the level's grids for Unity:

  height.f32      float32 (N, N), metres on the bake's datum (AHN NAP minus ~0.17 m), row 0 = the south edge
  water_sd.f32    float32 (N, N), signed distance to the water's edge in metres (+ in the water, - on land)
  class.u8        uint8 (N, N): 1 river plain and peat, 3 the 800 Kromme Rijn, 4 sands kept, 10 the 800 Lek
  cover.u8        uint8 (N, N): the land cover c. 800 (COVER below): levee crest and flank, wet meadow, reed marsh,
                  alder carr, peat, the older sandy ridges, bank mud, sand bars, pools, levee woods
  wood.u8         uint8 (N, N): how thickly wooded (0..255), a continuous value so the woods thin out at their edges
  meta.json       the grid, the water level, the port, the Kromme Rijn's centre line, the cover classes, the sources
  preview.png     a check plot

The land cover comes from the real data, not from the game's needs: the lidar heights over the water (a few
decimetres decide wet from dry in this country), Cohen's channel belts (which were active c. 800, which were long
dead sandy ridges), the Vos 800 AD units (the Langbroek peat), the distance to the water and the shape of the bends
(point bars grow on the inside of a bend, mud and steep banks on the outside). The settlement's clearing and the
woods' patchiness are guesses, marked below.

The level's frame: x east, z north, metres from the box's south-west corner (RD New, EPSG:28992).
Run: Bake\\run.bat levels\\make_dorestad.py
"""
import json
import os

import numpy as np
from osgeo import gdal, osr
from scipy import ndimage as ndi

gdal.UseExceptions()

RG = r"G:\kapman_unity\data\work\realground"
CACHE = os.path.join(RG, "cache")
OUT = r"G:\kapman_unity\data\levels\dorestad"
H10 = os.path.join(RG, "nl_793_10m.tif")
I10 = os.path.join(RG, "nl_793_10m_info.tif")
MAPS10 = os.path.join(CACHE, "nl_maps_10m.tif")           # bands: 1 Vos unit, 2 Cohen belt active c. 800,
                                                           # 3 today's embanked floodplain, 4 WorldCover, 5 any belt
BELT10 = os.path.join(CACHE, "nl_cohen_belt_10m.tif")     # Cohen's belt numbers (STROOM1)
VOS = os.path.join(CACHE, "nl_vos800_rd.gpkg")
LINES = os.path.join(CACHE, "nl_793_channel_lines.json")

PORT_LATLON = (51.972, 5.343)            # ports_793.json: Dorestat
CENTER = (151700.0, 443400.0)            # RD: between the fork and the Hoogstraat
N = 2049                                 # Unity heightmap size (2^11 + 1)
CELL = 2.5                               # m
SIZE = (N - 1) * CELL                    # 5120 m
X0, Y0 = CENTER[0] - SIZE / 2, CENTER[1] - SIZE / 2
MARGIN = 600.0
INLAND = ("binnenwater", "dekzand_hol", "laag holoceen")
VOS_VEEN = 6                             # the Vos unit index of 'veen' (row of vos800_rules.csv + 1)

DEPTH_LEK = 4.5          # m at mid-river, the 800 Lek
DEPTH_KR = 2.8           # m at mid-river, the 800 Kromme Rijn
EDGE_DEPTH = 0.35        # m at the water's edge
DEPTH_RAMP = 30.0        # m from the edge to full depth
BANK_RISE = 12.0         # m: the bank stands over the water this far out
BANK_LIFT = 0.12         # m over the water at the edge, rising 4 cm a metre

# The land cover classes (cover.u8). The heights are over the water level (2.44 m); the sources: Van der Tuuk ch. 2
# (levees 0.5-1.5 m over the basins, basins alder carr and reed marsh), Gouw-Bouman 2025 (the river area the most
# re-wooded part of the country AD 300-700; by 800 only the levees round the settlements cleared), Van Es & Verwers
# 2015 ch. VIII (the new land in the river bed low and wet; the old side channel behind the Hoogstraat levee),
# Cohen et al. 2012 (the belts), Vos et al. (the peat).
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
# The crest, flank and marsh thresholds come from the land's own height distribution, not from fixed metres: the
# lidar surface carries the post-870 flood clay and a millennium of levee growth, so c. 800's "a few decimetres
# decide wet from dry" (Van der Tuuk ch. 2) is a matter of rank, not of metres over today's water. Near the live
# rivers (within 600 m) the crest is the top fifth of the land, the flank the next quarter, the marsh the lowest
# fifth or so (the lowest parts only: the basins were mostly carr, Van der Tuuk ch. 2); the far land (the old
# ridges and the deep basins) takes its own ranks, so the dead sandy belts still
# stand out as ridges over their basins. The percentile choice is a guess tuned to the pollen (Gouw-Bouman 2025:
# two thirds to three quarters of the land wooded or marsh c. 800). The values found are printed and put in meta.
CREST_PCT_NEAR, FLANK_PCT_NEAR, MARSH_PCT_NEAR = 80, 55, 18
CREST_PCT_FAR, FLANK_PCT_FAR, MARSH_PCT_FAR = 85, 60, 20
NEAR_RIVER = 600.0       # m from the water: where the near ranks apply (blended to the far ranks over 300 m)
POOL_UNDER = -0.03       # m: a pool below this, if big enough
POOL_MIN_HA = 0.10
SETTLEMENT_ALONG = 2000.0    # m along the Kromme Rijn from the fork: the ribbon settlement (3 km in all; guess: the
SETTLEMENT_WIDE = 450.0      # level's town builder uses the first 1.65 km) and its clearing on the TOWN'S bank (a guess)
SETTLEMENT_WIDE_FAR = 120.0  # m: the clearing on the far bank, a landing strip only (a guess: the town lay on the
                             # west bank, ED4 ch. V 3; the far bank's new land was 'extremely wet', ch. VIII 1.1)
WOOD_BASIN_SHARE = 0.68      # share of the basin land (beyond the clearings) under carr: a guess from 'mostly
                             # impenetrable alder carr' (Van der Tuuk ch. 2) and the ~90% tree pollen
WOOD_HIGH_SHARE = 0.35       # share of the far high land under ash, elm and oak wood (a guess: re-clearance was
                             # under way by 800 but the big expansion of farmland came after 900, Gouw-Bouman 2025)
DE_GEER = (-600.0, 500.0, 250.0)   # the De Geer farms' clearing: offset from the port (m) and radius (a guess:
                                   # 'c. 500 m west of the harbour on an older levee', JALC 2010)
RIJSWIJK = (3600.0, 800.0, 300.0)  # the villa and castellum zone south across the Lek from the fork: level metres
                                   # and radius (a guess: the fort's site is lost to the river, ED4 ch. VIII)


def transform(src_epsg, dst_epsg):
    a = osr.SpatialReference()
    a.ImportFromEPSG(src_epsg)
    a.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
    b = osr.SpatialReference()
    b.ImportFromEPSG(dst_epsg)
    b.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
    return osr.CoordinateTransformation(a, b)


def rd_of(lat, lon):
    x, y, _ = transform(4326, 28992).TransformPoint(lon, lat)
    return x, y


def latlon_of(x, y):
    lon, lat, _ = transform(28992, 4326).TransformPoint(x, y)
    return lat, lon


def value_noise(scale_m, seed, n=N):
    """Smooth random noise in 0..1 (mean about 0.5) with features about scale_m across, on the level's grid."""
    rs = np.random.RandomState(seed)
    m = int(np.ceil(n * CELL / scale_m)) + 3
    grid = rs.random_sample((m, m)).astype(np.float32)
    z = ndi.zoom(grid, n / m + 1e-6, order=3)
    if z.shape[0] < n or z.shape[1] < n:
        z = np.pad(z, ((0, max(0, n - z.shape[0])), (0, max(0, n - z.shape[1]))), mode="edge")
    z = z[:n, :n]
    return np.clip((z - 0.5) * 1.6 + 0.5, 0.0, 1.0).astype(np.float32)


def land_cover(H, SD, wl, maps, channel, port):
    """The land cover c. 800 (COVER) and the wood density (0..1) on the level's grid.

    maps: dict of the 10 m bands resampled to the grid: 'vos' (Vos unit), 'active' (Cohen belt active c. 800),
    'anybelt', 'belt' (Cohen's belt numbers). channel: the Kromme Rijn's centre line from the fork (level metres)."""
    hw = H - wl
    land = SD <= 0
    dist = np.clip(-SD, 0, None)                      # m from the water's edge, on land
    n90, n400, n13 = value_noise(90.0, 11), value_noise(400.0, 12), value_noise(13.0, 13)
    wn90, wn400, wn13 = value_noise(90.0, 21), value_noise(400.0, 22), value_noise(13.0, 23)

    # how flat the shore is: the height 25-35 m inland from the water (the bake's own bank lift ends at 12 m)
    ring = land & (dist >= 25) & (dist <= 35)
    idx = ndi.distance_transform_edt(~ring, return_distances=False, return_indices=True)
    hw30 = hw[tuple(idx)]
    mud_w = np.interp(hw30, [0.4, 1.4], [26.0, 2.0]).astype(np.float32) * (0.55 + 0.9 * n90)
    mud = land & (dist < mud_w) & (hw < 0.7)

    # the inside of a bend: less water in a 300 m box round the cell than a straight channel of the local width would
    # give (a point bar grows there); the outside has more (the bank is cut)
    R = 150.0
    k = int(2 * R / CELL)
    water = (SD > 0).astype(np.float32)
    share = ndi.uniform_filter(water, size=k, mode="nearest")
    width = 2.0 * ndi.maximum_filter(np.clip(SD, 0, None), size=k, mode="nearest")
    expected = np.clip(width / (2 * R), 0.06, 1.0)
    bend = share / expected                            # ~1 on a straight reach, < 1 inside a bend, > 1 outside
    bar_w = 50.0 + 90.0 * n90
    bar = land & maps["active"] & ~mud & (bend < 0.8) & (dist < bar_w) & (hw < 0.9 + 0.3 * n13)

    # pools: the lowest hollows of the basins, below the water level and away from the river (the lidar's old gullies
    # and oxbow scars); the small ones stay reed marsh
    low = land & (hw < POOL_UNDER) & (dist > 40)
    lab, nlab = ndi.label(low)
    if nlab:
        area = ndi.sum(np.ones_like(lab, np.float32), lab, index=np.arange(nlab + 1)) * CELL * CELL / 1e4
        pool = (area[lab] >= POOL_MIN_HA) & low
    else:
        pool = np.zeros_like(low)

    # the thresholds from the land's own ranks (see the constants): one set for the land near the live rivers, one
    # for the far land, blended between 600 and 900 m from the water
    near_river = dist < NEAR_RIVER
    far_land = land & ~near_river
    crest_n, flank_n, marsh_n = np.percentile(hw[land & near_river], [CREST_PCT_NEAR, FLANK_PCT_NEAR, MARSH_PCT_NEAR])
    crest_f, flank_f, marsh_f = np.percentile(hw[far_land], [CREST_PCT_FAR, FLANK_PCT_FAR, MARSH_PCT_FAR])
    print("cover thresholds, m over the water: near the rivers crest %.2f flank %.2f marsh %.2f (p%d/p%d/p%d of the land"
          " within %.0f m); far land crest %.2f flank %.2f marsh %.2f (p%d/p%d/p%d)"
          % (crest_n, flank_n, marsh_n, CREST_PCT_NEAR, FLANK_PCT_NEAR, MARSH_PCT_NEAR, NEAR_RIVER,
             crest_f, flank_f, marsh_f, CREST_PCT_FAR, FLANK_PCT_FAR, MARSH_PCT_FAR))
    blend = np.clip((dist - NEAR_RIVER) / 300.0, 0.0, 1.0).astype(np.float32)
    crest_t = crest_n + (crest_f - crest_n) * blend + 0.2 * (n400 - 0.5) + 0.1 * (n13 - 0.5)
    flank_t = flank_n + (flank_f - flank_n) * blend + 0.2 * (n400 - 0.5) + 0.1 * (n13 - 0.5)
    marsh_t = marsh_n + (marsh_f - marsh_n) * blend + 0.25 * (n90 - 0.5) + 0.1 * (n13 - 0.5)
    marsh = land & ~mud & ~bar & ~pool & (hw < marsh_t)
    high = hw >= crest_t
    mid = (hw >= flank_t) & ~high
    thresholds = {"near": [round(float(v), 3) for v in (crest_n, flank_n, marsh_n)],
                  "far": [round(float(v), 3) for v in (crest_f, flank_f, marsh_f)]}

    cover = np.zeros(H.shape, np.uint8)
    cover[land] = 3                                                     # wet basin meadow by default
    cover[land & mid] = 2
    cover[land & high & (maps["active"] | near_river)] = 1              # the live rivers' levees
    cover[land & high & ~maps["active"] & ~near_river & maps["anybelt"]] = 7   # dead sandy belts
    cover[land & high & ~maps["active"] & ~near_river & ~maps["anybelt"]] = 2  # high basin margin: dry meadow
    cover[land & (maps["vos"] == VOS_VEEN) & ~high] = 6                 # the Langbroek peat
    cover[marsh] = 4
    cover[bar] = 9
    cover[mud] = 8
    cover[pool] = 10

    # the settlement's clearing: along the Kromme Rijn's first 2 km from the fork, wide on the town's bank (the
    # port's side of the line) and a landing strip only on the far bank, plus De Geer and Rijswijk (guesses)
    clear = np.zeros(H.shape, bool)
    line_t = np.zeros((2, N, N), np.float32)          # the line's direction at each of its cells
    if channel:
        c = np.asarray(channel, np.float64)
        s = np.concatenate([[0.0], np.cumsum(np.hypot(*np.diff(c, axis=0).T))])
        c = c[s <= SETTLEMENT_ALONG]
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
        vx, vz = jj - idx[1] * CELL, ii - idx[0] * CELL          # from the nearest line cell to the cell
        cross = tx * vz - tz * vx                                 # > 0: left of the line's direction
        # the port's side of the line decides which bank is the town's
        pi_, pj_ = min(N - 1, max(0, int(port[1] / CELL))), min(N - 1, max(0, int(port[0] / CELL)))
        town_sign = 1.0 if cross[pi_, pj_] >= 0 else -1.0
        town_side = np.sign(cross) == town_sign
        width = np.where(town_side, SETTLEMENT_WIDE, SETTLEMENT_WIDE_FAR).astype(np.float32)
    else:
        d_line = np.full(H.shape, 1e9, np.float32)
        width = np.full(H.shape, SETTLEMENT_WIDE, np.float32)
    d_geer = np.hypot(jj - (port[0] + DE_GEER[0]), ii - (port[1] + DE_GEER[1]))
    d_rijs = np.hypot(jj - RIJSWIJK[0], ii - RIJSWIJK[1])
    beyond = np.minimum(d_line - width, np.minimum(d_geer - DE_GEER[2], d_rijs - RIJSWIJK[2]))
    cleared = np.clip(beyond / 400.0, 0.0, 1.0).astype(np.float32)   # 0 in a clearing, 1 beyond its 400 m fade

    # the woods: the basins mostly carr (Gouw-Bouman 2025: tree pollen ~90% AD 300-700, grassland turned to alder
    # carr), the high land cleared round the settlement, patchy woods of ash, elm and oak further off; the shore
    # bare (willow scrub is planted by the level builder from the distance to the water). The wood bonus of each
    # class is set so that, beyond the clearings, WOOD_BASIN_SHARE of the basin land and WOOD_HIGH_SHARE of the high
    # land pass the canopy threshold (the shares are guesses; the patch shapes come from the noise).
    potential = 0.55 * wn400 + 0.30 * wn90 + 0.15 * wn13
    basin = np.isin(cover, (3, 4, 6))
    highland = np.isin(cover, (1, 2, 7))
    canopy = 0.42 + 0.45 * 0.3                                          # the potential that makes density 0.45
    open_land = cleared > 0.9
    def bonus(mask, share):
        sel = mask & open_land
        if sel.sum() < 100:
            return 0.0
        return float(canopy - np.percentile(potential[sel], 100.0 * (1.0 - share)))
    basin_bonus = bonus(np.isin(cover, (3, 6)), WOOD_BASIN_SHARE)       # the reed marsh never closes to carr
    high_bonus = bonus(highland, WOOD_HIGH_SHARE)
    print("wood bonus: basin %+.3f (for %.0f%% carr), high land %+.3f (for %.0f%% wood)"
          % (basin_bonus, 100 * WOOD_BASIN_SHARE, high_bonus, 100 * WOOD_HIGH_SHARE))
    wood = np.where(basin, potential + basin_bonus, potential + high_bonus)
    wood = wood * cleared
    wood[np.isin(cover, (8, 9, 10, 0))] = 0.0
    wood[cover == 4] *= 0.35                                           # reed marsh: only scattered alder
    wood[dist < 25] = 0.0
    density = np.clip((wood - 0.42) / 0.3, 0.0, 1.0).astype(np.float32)  # 0 open, 1 closed canopy
    density = ndi.gaussian_filter(density, 2.0)
    cover[(density > 0.45) & np.isin(cover, (3, 4, 6))] = 5
    cover[(density > 0.45) & np.isin(cover, (1, 2, 7))] = 11
    cover[~land] = 0
    carr_able = np.isin(cover, (3, 5, 6)) & open_land                  # the basin land that can close to carr
    wooded_basin = float(((density > 0.45) & carr_able).sum() / max(1, carr_able.sum()))
    print("wood: %.0f%% of the open-country basin land under carr, %.0f%% of the open-country high land under wood"
          % (100 * wooded_basin, 100 * float(((density > 0.45) & highland & open_land).sum() / max(1, (highland & open_land).sum()))))
    thresholds["wood_bonus"] = [round(basin_bonus, 3), round(high_bonus, 3)]
    return cover, density, bend, thresholds


def main():
    os.makedirs(OUT, exist_ok=True)
    ds = gdal.Open(H10)
    gt = ds.GetGeoTransform()
    c0 = int((X0 - MARGIN - gt[0]) // 10)
    r0 = int((gt[3] - (Y0 + SIZE + MARGIN)) // 10)
    w = int((SIZE + 2 * MARGIN) // 10) + 2
    h = w
    wx0, wy1 = gt[0] + c0 * 10, gt[3] - r0 * 10            # the window's west and north edges
    h10 = ds.GetRasterBand(1).ReadAsArray(c0, r0, w, h).astype(np.float32)
    inf = gdal.Open(I10)
    cls = inf.GetRasterBand(1).ReadAsArray(c0, r0, w, h)
    maps_ds = gdal.Open(MAPS10)
    vos10 = maps_ds.GetRasterBand(1).ReadAsArray(c0, r0, w, h)
    active10 = maps_ds.GetRasterBand(2).ReadAsArray(c0, r0, w, h)
    anybelt10 = maps_ds.GetRasterBand(5).ReadAsArray(c0, r0, w, h)
    belt_ds = gdal.Open(BELT10)
    belt10 = belt_ds.GetRasterBand(1).ReadAsArray(c0, r0, w, h)

    mem = gdal.GetDriverByName("MEM").Create("", w, h, 1, gdal.GDT_Byte)
    mem.SetGeoTransform((wx0, 10, 0, wy1, 0, -10))
    mem.SetProjection(ds.GetProjection())
    where = "legenda IN (%s)" % ",".join("'%s'" % u for u in INLAND)
    gdal.Rasterize(mem, VOS, layers=["vos800"], burnValues=[1], where=where)
    lek = mem.GetRasterBand(1).ReadAsArray() > 0
    kr = cls == 3
    water = lek | kr

    # signed distance to the water's edge at 10 m (+ inside)
    din = ndi.distance_transform_edt(water) * 10.0
    dout = ndi.distance_transform_edt(~water) * 10.0
    sd10 = np.where(water, din - 5.0, -(dout - 5.0)).astype(np.float32)

    # heights: NaN (the inland water, where the c. 800 raster has no weight) filled from the nearest ground
    nan = ~np.isfinite(h10)
    idx = ndi.distance_transform_edt(nan, return_distances=False, return_indices=True)
    hf = h10[tuple(idx)]

    # the water level: low on the banks, so the levees and most banks stand over it
    ring = (sd10 < -10) & (sd10 > -60) & ~nan
    wl = float(np.percentile(h10[ring], 20))
    kr_bed = h10[kr & ~nan]
    print("bank ring: p5 %.2f p20 %.2f p50 %.2f p80 %.2f m" % tuple(np.percentile(h10[ring], [5, 20, 50, 80])))
    print("water level %.2f m; Kromme Rijn bed p10/p50/p90 %.2f %.2f %.2f m"
          % ((wl,) + tuple(np.percentile(kr_bed, [10, 50, 90]))))

    # to the level's grid (2.5 m, row 0 south)
    jj, ii = np.meshgrid(np.arange(N), np.arange(N))
    x = X0 + jj * CELL
    y = Y0 + ii * CELL
    cx = (x - wx0) / 10.0 - 0.5
    cy = (wy1 - y) / 10.0 - 0.5
    H = ndi.map_coordinates(hf, [cy, cx], order=3, mode="nearest").astype(np.float32)
    SD = ndi.map_coordinates(sd10, [cy, cx], order=1, mode="nearest").astype(np.float32)
    LEK = ndi.map_coordinates(lek.astype(np.float32), [cy, cx], order=1, mode="nearest") > 0.5
    CLS = ndi.map_coordinates(cls.astype(np.float32), [cy, cx], order=0, mode="nearest").astype(np.uint8)
    SD = ndi.gaussian_filter(SD, 2.0)          # round the 10 m steps off the shoreline

    def nearest(a):
        return ndi.map_coordinates(a.astype(np.float32), [cy, cx], order=0, mode="nearest")
    maps = {
        "vos": nearest(vos10).astype(np.uint8),
        "active": nearest(active10) > 0.5,
        "anybelt": nearest(anybelt10) > 0.5,
        "belt": nearest(belt10).astype(np.uint16),
    }

    def shape(H):
        inside = SD > 0
        depth = np.where(LEK, DEPTH_LEK, DEPTH_KR)
        t = np.clip(SD / DEPTH_RAMP, 0, 1)
        t = t * t * (3 - 2 * t)
        bed = wl - (EDGE_DEPTH + (depth - EDGE_DEPTH) * t)
        H = np.where(inside, np.minimum(H, bed), H)
        near = (SD <= 0) & (SD > -BANK_RISE)
        bank = wl + BANK_LIFT + 0.04 * (-SD)
        return np.where(near, np.maximum(H, bank), H)

    H = shape(H)
    H = ndi.gaussian_filter(H, 1.0)
    H = shape(H).astype(np.float32)
    water_f = SD > 0
    CLS = np.where(water_f, np.where(LEK, 10, 3), CLS).astype(np.uint8)

    # the port and the Kromme Rijn's centre line, in the level's frame
    px, py = rd_of(*PORT_LATLON)
    port = [px - X0, py - Y0]
    lines = json.load(open(LINES))
    best = None
    for ln in lines:
        a = np.asarray(ln, dtype=np.float64)
        inb = (a[:, 0] > X0) & (a[:, 0] < X0 + SIZE) & (a[:, 1] > Y0) & (a[:, 1] < Y0 + SIZE)
        if not inb.any():
            continue
        a = a[inb]
        d = np.hypot(a[:, 0] - px, a[:, 1] - py).min()
        if best is None or d < best[0]:
            best = (d, a)
    channel = []
    if best is not None:
        a = best[1]
        keep = [0]
        for k in range(1, len(a)):
            if np.hypot(*(a[k] - a[keep[-1]])) >= 10.0:
                keep.append(k)
        channel = [[round(float(p[0] - X0), 2), round(float(p[1] - Y0), 2)] for p in a[keep]]
        print("Kromme Rijn line: %d points, nearest %.0f m from the port" % (len(channel), best[0]))

    # the land cover: the channel line is walked from the fork (the end nearer the Lek)
    from_fork = list(channel)
    if from_fork:
        d_lek = ndi.distance_transform_edt(~LEK) * CELL
        def dl(p):
            return d_lek[min(N - 1, max(0, int(p[1] / CELL))), min(N - 1, max(0, int(p[0] / CELL)))]
        if dl(from_fork[-1]) < dl(from_fork[0]):
            from_fork.reverse()
    COVER_U8, WOOD, bend, thresholds = land_cover(H, SD, wl, maps, from_fork, port)
    land = SD <= 0
    hw_land = (H - wl)[land]
    print("land over the water: p10 %.2f p25 %.2f p50 %.2f p75 %.2f p90 %.2f m" % tuple(np.percentile(hw_land, [10, 25, 50, 75, 90])))
    cover_stats = {}
    for k, name in COVER.items():
        share = float((COVER_U8 == k).mean())
        cover_stats[str(k)] = round(share, 4)
        print("  cover %2d %5.1f%%  %s" % (k, 100 * share, name.split(":")[0]))
    print("wood: %.1f%% of the land over 0.5 density" % (100 * float((WOOD[land] > 0.5).mean())))

    H.tofile(os.path.join(OUT, "height.f32"))
    SD.astype(np.float32).tofile(os.path.join(OUT, "water_sd.f32"))
    CLS.tofile(os.path.join(OUT, "class.u8"))
    COVER_U8.tofile(os.path.join(OUT, "cover.u8"))
    np.clip(WOOD * 255.0 + 0.5, 0, 255).astype(np.uint8).tofile(os.path.join(OUT, "wood.u8"))
    clat, clon = latlon_of(*CENTER)
    meta = {
        "name": "Dorestad",
        "about": "The first river place: Dorestad at the fork of the Kromme Rijn and the Lek, c. 800, at true size",
        "crs": "EPSG:28992",
        "origin_rd": [X0, Y0],
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
        "cover_classes": {str(k): v for k, v in COVER.items()},
        "cover_share": cover_stats,
        "cover_thresholds": thresholds,
        "wood_file": "wood.u8: uint8 (n, n), how thickly wooded, 0 open to 255 a closed canopy (continuous: the woods thin at their edges)",
        "cover_guesses": [
            "the settlement's clearing: %.0f m along the Kromme Rijn from the fork, %.0f m wide on the town's bank, %.0f m on the far bank, fading out over 400 m" % (SETTLEMENT_ALONG, SETTLEMENT_WIDE, SETTLEMENT_WIDE_FAR),
            "De Geer's clearing: %.0f m radius at (%.0f, %.0f) m from the port" % (DE_GEER[2], DE_GEER[0], DE_GEER[1]),
            "Rijswijk's clearing (the villa and the lost castellum south of the Lek): %.0f m radius at (%.0f, %.0f) m" % (RIJSWIJK[2], RIJSWIJK[0], RIJSWIJK[1]),
            "the woods' patch shapes and shares (%.0f%% of the basin land beyond the clearings under carr, %.0f%% of the far high land under wood)" % (100 * WOOD_BASIN_SHARE, 100 * WOOD_HIGH_SHARE),
            "the crest/flank/marsh thresholds as ranks of the land's height over the water: p%d/p%d/p%d within %.0f m of the rivers = %.2f/%.2f/%.2f m, p%d/p%d/p%d of the far land = %.2f/%.2f/%.2f m (with noise)"
            % (CREST_PCT_NEAR, FLANK_PCT_NEAR, MARSH_PCT_NEAR, NEAR_RIVER, *thresholds["near"], CREST_PCT_FAR, FLANK_PCT_FAR, MARSH_PCT_FAR, *thresholds["far"]),
            "mud width 2-26 m by the shore's flatness; sand bars inside bends in the active belts; pools in hollows over %.1f ha" % POOL_MIN_HA,
        ],
        "sources": [
            "The Netherlands c. 800 from AHN4 lidar (CC0), the first build's Bake/realground/nl_793.py",
            "Vos et al., paleogeographic map 800 AD (inland water: the 800 Lek; veen: the Langbroek peat)",
            "Cohen et al. 2012, Rhine-Meuse delta channel belts (CC BY 4.0), via the 800 channel; active belts c. 800 and the dead sandy ridges",
            "Van der Tuuk, Dorestad onthuld ch. 2; Gouw-Bouman 2025 Veget Hist Archaeobot 34; Van Es & Verwers 2015 (NO 18) ch. VIII: what grew where",
        ],
    }
    json.dump(meta, open(os.path.join(OUT, "meta.json"), "w"), indent=1)
    print("height %.2f..%.2f m, water %.1f%% of the level" % (H.min(), H.max(), 100 * water_f.mean()))

    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from matplotlib.colors import LightSource, ListedColormap, BoundaryNorm
    from matplotlib.patches import Patch
    fig, ax = plt.subplots(2, 2, figsize=(20, 19))
    ls = LightSource(azdeg=315, altdeg=35)
    rgb = ls.shade(H, cmap=plt.cm.gist_earth, vert_exag=6, blend_mode="soft", vmin=wl - 5, vmax=wl + 4)
    ax[0, 0].imshow(rgb, origin="lower", extent=(0, SIZE, 0, SIZE))
    ax[0, 0].contour(np.arange(N) * CELL, np.arange(N) * CELL, SD, levels=[0], colors="cyan", linewidths=0.6)
    if channel:
        c = np.asarray(channel)
        ax[0, 0].plot(c[:, 0], c[:, 1], "r-", lw=0.8)
    ax[0, 0].plot(*port, "y*", ms=14)
    ax[0, 0].set_title("Dorestad level: ground c. 800 (hillshade x6), water edge (cyan), Kromme Rijn line (red), port")
    im = ax[0, 1].imshow(H - wl, origin="lower", extent=(0, SIZE, 0, SIZE), cmap="RdYlBu_r", vmin=-5, vmax=4)
    plt.colorbar(im, ax=ax[0, 1], shrink=0.8, label="m over the water level (%.2f m)" % wl)
    ax[0, 1].set_title("height over the water level")
    colours = ["#3e5c8e", "#d9c98a", "#b8c47a", "#8fae6a", "#6f8f5a", "#2f5a34", "#6c6844", "#cdb97a", "#7a6248",
               "#e8dcb0", "#4f7fa8", "#3f7040"]
    cm = ListedColormap(colours)
    norm = BoundaryNorm(np.arange(-0.5, len(colours) + 0.5, 1), cm.N)
    ax[1, 0].imshow(COVER_U8, origin="lower", extent=(0, SIZE, 0, SIZE), cmap=cm, norm=norm, interpolation="nearest")
    ax[1, 0].plot(*port, "k*", ms=12)
    ax[1, 0].legend(handles=[Patch(color=colours[k], label="%d %s" % (k, COVER[k].split(":")[0])) for k in COVER],
                    loc="lower left", fontsize=8, framealpha=0.85)
    ax[1, 0].set_title("land cover c. 800 (cover.u8)")
    im = ax[1, 1].imshow(np.where(land, WOOD, np.nan), origin="lower", extent=(0, SIZE, 0, SIZE), cmap="Greens", vmin=0, vmax=1)
    plt.colorbar(im, ax=ax[1, 1], shrink=0.8, label="wood density (wood.u8 / 255)")
    ax[1, 1].contour(np.arange(N) * CELL, np.arange(N) * CELL, bend, levels=[0.8], colors="orange", linewidths=0.5)
    ax[1, 1].plot(*port, "k*", ms=12)
    ax[1, 1].set_title("woods (green), and the inside of the bends (orange: where the bars grow)")
    fig.tight_layout()
    fig.savefig(os.path.join(OUT, "preview.png"), dpi=80)
    print("written to", OUT)


if __name__ == "__main__":
    main()
