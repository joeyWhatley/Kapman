"""
make_scale_maps.py - the data for the scale comparison maps (the user, 4 Oct 2026, evening: "create several 2D maps
displaying different proportions and scales ... high res and zoom in to see river widths ... when you scale down cut the
less prominent towns"). Picker: 1:40, 1:80, 1:160 and a few more of different proportions; maps that shorten the open
sea harder; each with three river rules (true width, shrunk with the world, boat-scaled); the whole world.

Everything is written in REAL metres of the world map's projection (make_world_map.py's Lambert conformal); the page
divides by each map's scale. Maps of uneven scale get a warp grid (real -> real-equivalent at the land's scale).

  run.bat world\\make_scale_maps.py                 -> G:\\kapman_unity\\data\\scale_maps\\

Out:
  land.bin       the land (Natural Earth 10 m outside the Low Countries, the Vos c. 800 map inside): rings of int32
                 x, y in 10 m units, after a header (see write_rings)
  lakes.bin      the big lakes, the same way
  rivers.bin     every other river with an average flow of 30 m3/s or more (HydroRIVERS; Natural Earth west of 30 W):
                 polylines with their true width (m), from the flow (w = 8 sqrt(Q), a hydraulic-geometry estimate)
  maps.json      the maps (their scales and warps), the game rivers traced and eased at each scale and rule, the
                 places with their rank, the voyages, the credits
  warp_<id>.bin  a warp grid per uneven map: float32 header (nx, ny, x0, y0, dx, dy) then nx*ny (x', y')
"""
import json
import math
import os
import struct
import sys
import time

import numpy as np
import shapely
from osgeo import ogr
from scipy import fft as sfft
from scipy import ndimage

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import make_tiles as mt  # noqa: E402

OUT = r"G:\kapman_unity\data\scale_maps"
BBOX = (-68.0, 41.0, 42.0, 74.0)
PORTS = os.path.join(mt.ROOT, "Assets", "Kapman", "Game", "Data", "ports_793.json")
LEVELS = os.path.join(mt.ROOT, "Design", "levels_793.json")
SHIP_SPEED = 2.0            # m/s, about 4 knots: her average under sail and oar

T0 = time.time()


def log(s):
    print("%6.1f s  %s" % (time.time() - T0, s), flush=True)


class Args:
    simplify = 200.0          # real m: the coast's tolerance (5 game m at 1:40)
    segment = 1e12
    smooth = 0
    vos_channel = 300.0
    max_width = 300.0
    branch_width = 150.0
    bend = 3.0


# ---------------------------------------------------------------------------------------------------- the maps
# kind "uniform": one scale everywhere. kind "warp": a warp grid, then the land's scale.
# rivers: the scale whose traced game rivers the map uses.
MAPS = [
    dict(id="u1", name="The real world, 1:1", short="1:1", kind="uniform", scale=1, rivers=1,
         note="For reference: no shrinking at all. Zoom in anywhere to see the real river beside the boat."),
    dict(id="u32", name="1:32, history's days", short="1:32", kind="uniform", scale=32, rivers=32,
         note="Voyages take about as many game days as they took in history (Wulfstan, Hedeby to Truso, 7 days; "
              "Ohthere, Kaupang to Hedeby, 5): about 1:30 to 1:37 on the 1 s = 1 min clock."),
    dict(id="u40", name="1:40 everywhere", short="1:40", kind="uniform", scale=40, rivers=40,
         note="Today's choice. Every river town keeps its own stretch of water."),
    dict(id="u60", name="1:60, true speed", short="1:60", kind="uniform", scale=60, rivers=60,
         note="On the 1 s = 1 min clock the boat's speed reads true (4-5 knots by the game's clock), as Euro Truck "
              "Simulator matches its 1:19 world to its clock."),
    dict(id="u80", name="1:80 everywhere", short="1:80", kind="uniform", scale=80, rivers=80,
         note="Half the voyage times. River towns start to crowd."),
    dict(id="u160", name="1:160 everywhere", short="1:160", kind="uniform", scale=160, rivers=160,
         note="A quarter of the voyage times. Only the big places fit."),
    dict(id="m80", name="Land 1:40, open sea 1:80", short="1:40 / sea 1:80", kind="warp", scale=40, rivers=40,
         sea=1 / 4.0, band_km=25, note="The coasts and rivers as at 1:40; the empty sea between them squeezed twice as hard."),
    dict(id="m160", name="Land 1:40, open sea 1:160", short="1:40 / sea 1:160", kind="warp", scale=40, rivers=40,
         sea=1 / 16.0, band_km=25, note="Crossings four times shorter than the coasts around them."),
    dict(id="rivers", name="Rivers long, seas short", short="rivers 1:20, sea 1:160", kind="warp", scale=40, rivers=40,
         sea=1 / 16.0, band_km=25, corridor=4.0, corridor_km=12,
         note="The level idea in one map: river roads stretched to 1:20 so they feel like journeys, other land 1:40, "
              "open sea 1:160."),
    dict(id="home", name="Home waters", short="home 1:40, far 1:500", kind="radial", scale=40, rivers=40,
         centre=(5.343, 51.972), inner_km=600, outer_km=5200, outer_scale=500,
         note="The world as a Frisian saw it: the North Sea at 1:40, everything further off squeezed more and more, "
              "Vinland at about 1:500."),
    dict(id="sqrt", name="Square-root distances", short="hops 1:36, Atlantic 1:170", kind="sqrt", scale=40, rivers=40,
         note="Every place moved so a leg takes 1.45 minutes times the square root of its real km (Dorestad to London "
              "30 min): short hops stay near 1:36, the North Sea near 1:110, the Atlantic near 1:170. Coasts bend to fit."),
]
TRACE_SCALES = sorted({m["rivers"] for m in MAPS} | {20})


# ---------------------------------------------------------------------------------------------------- writing
def write_rings(path, rings, extra=None):
    """Header: magic 'KRNG', count (u32), then count u32 lengths (points), then count u16 widths if extra, then
    all points as int32 x, y in 10 m units."""
    with open(path, "wb") as f:
        f.write(b"KRNG")
        f.write(struct.pack("<II", len(rings), 1 if extra is not None else 0))
        f.write(np.asarray([len(r) for r in rings], dtype="<u4").tobytes())
        if extra is not None:
            f.write(np.asarray(extra, dtype="<u2").tobytes())
            if len(extra) % 2:
                f.write(b"\0\0")                                   # (keep the int32s aligned)
        for r in rings:
            f.write(np.round(np.asarray(r) / 10.0).astype("<i4").tobytes())
    return os.path.getsize(path)


def poly_rings(polys, min_area=0.0):
    rings = []
    for p in polys:
        if p.area < min_area:
            continue
        rings.append(np.asarray(p.exterior.coords)[:, :2])
        for h in p.interiors:
            if shapely.Polygon(h).area >= min_area:
                rings.append(np.asarray(h.coords)[:, :2])
    return rings


# ---------------------------------------------------------------------------------------------------- sources
def land_and_lakes(frame):
    args = Args()
    ne = mt.load_natural_earth(frame, BBOX, args)
    log("Natural Earth land: %d polygons, %d points" % (len(ne), sum(shapely.get_num_coordinates(ne))))
    vos, _ = mt.load_vos(frame, BBOX, args)
    dom = shapely.union_all(vos["domain"])
    wet = shapely.union_all(vos["sea"] + vos["inland"])
    ne_u = shapely.union_all(ne)
    land = shapely.union_all([shapely.difference(ne_u, dom), shapely.difference(dom, wet)])
    land = shapely.make_valid(shapely.simplify(land, Args.simplify, preserve_topology=True))
    polys = mt.polys_only(land)
    log("land with the Low Countries c. 800: %d polygons, %d points" % (len(polys), sum(shapely.get_num_coordinates(polys))))
    # big lakes (Natural Earth), 20 km2 and up
    lakes = []
    ds = ogr.Open(mt.NE + "ne_10m_lakes.shp")
    lyr = ds.GetLayer(0)
    lyr.SetSpatialFilterRect(*BBOX)
    for f in lyr:
        g = mt.ogr_to_shapely(f.GetGeometryRef())
        if g is None or g.is_empty:
            continue
        g = shapely.make_valid(shapely.simplify(shapely.transform(g, frame.ll_to_real), Args.simplify, preserve_topology=True))
        lakes += [p for p in mt.polys_only(g) if p.area >= 20e6]
    ds = None
    log("lakes: %d" % len(lakes))
    return polys, lakes, dom


def other_rivers(frame, vos_domain):
    """HydroRIVERS (Europe) with 30 m3/s or more, outside the Vos domain (the c. 800 rivers stand in there); Natural
    Earth's river lines west of 30 W (Iceland is in HydroRIVERS Europe; Greenland and Vinland are not)."""
    lines, widths = [], []
    ds = ogr.Open(mt.HYDRO)
    lyr = ds.GetLayer(0)
    lyr.SetSpatialFilterRect(*BBOX)
    lyr.SetAttributeFilter("DIS_AV_CMS >= 30")
    dom_prep = None if vos_domain.is_empty else vos_domain
    if dom_prep is not None:
        shapely.prepare(dom_prep)
    for f in lyr:
        q = f.GetField("DIS_AV_CMS") or 0.0
        c = np.asarray(f.GetGeometryRef().GetPoints(), dtype=float)[:, :2]
        if len(c) < 2:
            continue
        r = frame.ll_to_real(c)
        if dom_prep is not None and shapely.contains_xy(dom_prep, r[len(r) // 2][0], r[len(r) // 2][1]):
            continue
        ls = shapely.simplify(shapely.LineString(r), 150.0)
        lines.append(np.asarray(ls.coords))
        widths.append(int(min(4000, round(8.0 * math.sqrt(q)))))
    ds = None
    n_h = len(lines)
    ds = ogr.Open(mt.NE + "ne_10m_rivers_lake_centerlines.shp")
    if ds is not None:
        lyr = ds.GetLayer(0)
        lyr.SetSpatialFilterRect(BBOX[0], BBOX[1], -30.0, BBOX[3])
        for f in lyr:
            g = mt.ogr_to_shapely(f.GetGeometryRef())
            if g is None:
                continue
            rank = f.GetField("scalerank") or 6
            for part in shapely.get_parts(g):
                c = np.asarray(part.coords)[:, :2]
                if len(c) >= 2:
                    lines.append(np.asarray(shapely.simplify(shapely.LineString(frame.ll_to_real(c)), 150.0).coords))
                    widths.append(int(max(40, 400 - 50 * rank)))
        ds = None
    log("other rivers: %d HydroRIVERS reaches, %d Natural Earth lines, %d points" % (n_h, len(lines) - n_h, sum(len(l) for l in lines)))
    return lines, widths


# ---------------------------------------------------------------------------------------------------- the game rivers
def trace_raw(rdef, nets, frame, records):
    """make_tiles.trace_river's course-finding, kept in real metres, with each station's true (uncapped) width."""
    pts = rdef["pts"]
    wp = frame.ll_to_real([(p[1], p[0]) for p in pts])
    seq, st_idx = [wp[0]], [0]
    for i in range(len(pts) - 1):
        leg = pts[i][4] if len(pts[i]) > 4 else rdef["net"]
        if leg == "line" or leg not in nets:
            seq.append(wp[i + 1])
        else:
            net = nets[leg]
            s, _ = net.snap(wp[i])
            t, _ = net.snap(wp[i + 1])
            path = net.path(s, t)
            straight = float(np.hypot(*(wp[i + 1] - wp[i])))
            plen = float(np.hypot(*np.diff(net.xy[path], axis=0).T).sum()) if path is not None and len(path) > 1 else 0.0
            if path is None or plen > max(2.5 * straight, straight + 5000.0):
                seq.append(wp[i + 1])
            else:
                seq.extend(net.xy[path])
                if i + 1 == len(pts) - 1:
                    seq.append(wp[i + 1])
        st_idx.append(len(seq) - 1)
    seq = np.asarray(seq)
    keep = np.ones(len(seq), bool)
    keep[1:] = np.hypot(*np.diff(seq, axis=0).T) > 1e-6
    remap = np.cumsum(keep) - 1
    seq = seq[keep]
    st_idx = [int(remap[k]) for k in st_idx]
    s = np.concatenate([[0.0], np.cumsum(np.hypot(*np.diff(seq, axis=0).T))])
    st_s, st_w = [], []
    for k, p in zip(st_idx, pts):
        w, _ = mt.station_width(p[2], records, 1e9)
        if w is not None:
            st_s.append(s[k])
            st_w.append(w)
    W = np.interp(s, st_s, st_w)
    return dict(id=rdef["id"], name=rdef["name"], branch=rdef["id"] in mt.BRANCHES, P=seq, W=W, pins=st_idx)


def rule_width(w_real, rule, scale, branch):
    if rule == "true":
        return np.minimum(w_real, Args.branch_width if branch else Args.max_width)
    if rule == "boat":                                  # the 1 Oct rule: 2.15 real^0.604, at least 15 m and real/scale
        return np.maximum(np.maximum(15.0, 2.15 * np.power(w_real, 0.604)), w_real / scale)
    return w_real / scale                               # shrunk with the world


def game_rivers(frame):
    records = mt.load_records()
    la = [p[0] for r in mt.RIVERS for p in r["pts"]]
    lo = [p[1] for r in mt.RIVERS for p in r["pts"]]
    hbox = (min(lo) - 0.3, min(la) - 0.3, max(lo) + 0.3, max(la) + 0.3)
    nets = {"hydro": mt.Net(*mt.load_hydro(frame, hbox), snap_m=1500.0), "vos": mt.Net(*mt.load_vos_lines(frame, hbox), snap_m=1000.0)}
    raws = [trace_raw(r, nets, frame, records) for r in mt.RIVERS]
    log("game rivers traced: %s" % ", ".join("%s %.0f km" % (r["id"], np.hypot(*np.diff(r["P"], axis=0).T).sum() / 1000) for r in raws))
    out = {}
    for scale in TRACE_SCALES:
        for rule in ("true", "boat", "shrunk"):
            key = "%d_%s" % (scale, rule)
            rivs = []
            for r in raws:
                G = r["P"] / scale
                W = rule_width(r["W"], rule, scale, r["branch"])
                if rule == "shrunk":
                    P2, W2 = G, W                         # a few metres wide: the real course fits as it is
                else:
                    P2, W2, _, _, _, _ = mt.ease_bends(G, W, Args.bend, pins=G[r["pins"]])
                rivs.append(dict(id=r["id"], name=r["name"], branch=r["branch"],
                                 p=np.round(P2 * scale / 10.0).astype(int).ravel().tolist(),       # back to real, 10 m units
                                 w=np.round(W2, 1).tolist()))
            out[key] = rivs
        log("game rivers eased at 1:%d (true, boat, shrunk)" % scale)
    return out, raws


# ---------------------------------------------------------------------------------------------------- the warps
def grid_for(land_polys, lakes):
    """A density grid over the world box padded by a third each side: land and lakes."""
    xs = [p.bounds for p in land_polys]
    x0 = min(b[0] for b in xs); y0 = min(b[1] for b in xs); x1 = max(b[2] for b in xs); y1 = max(b[3] for b in xs)
    pad_x, pad_y = 0.33 * (x1 - x0), 0.33 * (y1 - y0)
    gx0, gy0, gx1, gy1 = x0 - pad_x, y0 - pad_y, x1 + pad_x, y1 + pad_y
    res = 6000.0
    nx, ny = int(math.ceil((gx1 - gx0) / res)), int(math.ceil((gy1 - gy0) / res))
    land = mt.rasterize(land_polys, gx0, gy0, nx, ny, res)
    return dict(x0=gx0, y0=gy0, nx=nx, ny=ny, res=res, land=land)


def density(g, spec, corridor_lines):
    land = g["land"]
    dist_km = ndimage.distance_transform_edt(~land) * g["res"] / 1000.0          # distance to land, km
    band = np.clip(1.0 - (dist_km - spec["band_km"]) / spec["band_km"], 0.0, 1.0)  # 1 near the coast, 0 two bands out
    rho = spec["sea"] + (1.0 - spec["sea"]) * band
    rho[land] = 1.0
    if spec.get("corridor"):
        mask = np.zeros_like(land)
        for line in corridor_lines:
            c = ((line[:, 0] - g["x0"]) / g["res"]).astype(int)
            r = ((line[:, 1] - g["y0"]) / g["res"]).astype(int)
            ok = (c >= 0) & (c < g["nx"]) & (r >= 0) & (r < g["ny"])
            mask[r[ok], c[ok]] = True
        near = ndimage.distance_transform_edt(~mask) * g["res"] / 1000.0
        rho = np.where(near <= spec["corridor_km"], spec["corridor"], rho)
    return ndimage.gaussian_filter(rho, 1.5)


def flow_cartogram(rho, res, steps=160):
    """Gastner, Seguy & More (2018): a flow that carries the density rho to its mean. With rho_t = (1-t) rho + t mean,
    the flux J = -grad(psi), lap(psi) = mean - rho (cosine transform: no flux through the edges), constant in time;
    each point moves with v = J / rho_t. Returns the final position of every grid node (x, y in cells)."""
    ny, nx = rho.shape
    mean = rho.mean()
    src = sfft.dctn(mean - rho, type=2, norm="ortho")
    ky = np.pi * np.arange(ny) / ny
    kx = np.pi * np.arange(nx) / nx
    lap = -(2 - 2 * np.cos(ky))[:, None] - (2 - 2 * np.cos(kx))[None, :]
    lap[0, 0] = 1.0
    psi_h = src / lap
    psi_h[0, 0] = 0.0
    psi = sfft.idctn(psi_h, type=2, norm="ortho")
    jy, jx = np.gradient(-psi)
    yy, xx = np.mgrid[0:ny, 0:nx].astype(float)
    px, py = xx.copy(), yy.copy()

    def vel(px, py, t):
        coords = [np.clip(py, 0, ny - 1), np.clip(px, 0, nx - 1)]
        r = (1 - t) * ndimage.map_coordinates(rho, coords, order=1, mode="nearest") + t * mean
        vx = ndimage.map_coordinates(jx, coords, order=1, mode="nearest") / r
        vy = ndimage.map_coordinates(jy, coords, order=1, mode="nearest") / r
        return vx, vy

    dt = 1.0 / steps
    for i in range(steps):
        t = i * dt
        vx1, vy1 = vel(px, py, t)
        vx2, vy2 = vel(px + 0.5 * dt * vx1, py + 0.5 * dt * vy1, t + 0.5 * dt)
        px += dt * vx2
        py += dt * vy2
    return px, py


def land_scale(g, px, py):
    """The mean linear scale the warp gives the land (sqrt of the mean area factor over land cells)."""
    dxdx = np.gradient(px, axis=1); dxdy = np.gradient(px, axis=0)
    dydx = np.gradient(py, axis=1); dydy = np.gradient(py, axis=0)
    det = np.abs(dxdx * dydy - dxdy * dydx)
    return float(math.sqrt(det[g["land"]].mean())), det


def write_warp(path, g, X, Y, step=3):
    """Every step-th node from the middle of each step x step block (so the grid's corner stays put): 18 km nodes, about
    2.4 MB a map (the page's files must stay under 15 MB; the warps are smooth at this spacing)."""
    Xs, Ys = X[step // 2::step, step // 2::step], Y[step // 2::step, step // 2::step]
    with open(path, "wb") as f:
        f.write(struct.pack("<6f", Xs.shape[1], Xs.shape[0], g["x0"], g["y0"], g["res"] * step, g["res"] * step))
        f.write(np.stack([Xs, Ys], axis=-1).astype("<f4").tobytes())


def radial_warp(g, frame, spec):
    c = frame.ll_to_real([spec["centre"]])[0]
    xs = g["x0"] + (np.arange(g["nx"]) + 0.5) * g["res"]
    ys = g["y0"] + (np.arange(g["ny"]) + 0.5) * g["res"]
    X, Y = np.meshgrid(xs, ys)
    dx, dy = X - c[0], Y - c[1]
    d = np.hypot(dx, dy)
    # scale N(d): the map's own scale inside inner_km, rising as a power to outer_scale at outer_km
    inner, outer = spec["inner_km"] * 1000.0, spec["outer_km"] * 1000.0
    k = math.log(spec["outer_scale"] / spec["scale"]) / math.log(outer / inner)
    s = np.linspace(0, d.max() + 1, 20000)
    N = np.where(s < inner, spec["scale"], spec["scale"] * np.power(np.maximum(s, inner) / inner, k))
    gd = np.concatenate([[0.0], np.cumsum(np.diff(s) / N[1:])]) * spec["scale"]     # real-equivalent at the map scale
    r2 = np.interp(d, s, gd)
    f = np.where(d > 0, r2 / np.maximum(d, 1e-6), 1.0)
    return c[0] + dx * f, c[1] + dy * f


def sqrt_warp(g, frame, pl, raws):
    """Places laid out by stress majorization (SMACOF) so each pair's distance is 195.3 m x sqrt(real km) of game
    (1.447 min x sqrt(km) at 2.25 m/s), then a thin-plate spline carries the whole grid along. Real-equivalent at 1:40."""
    from scipy.interpolate import RBFInterpolator
    pts = [q for q in pl if q["rank"] >= 2.5 or q["x"] * 10.0 < -1.2e6]                 # the main ports and the far west
    P = np.array([[q["x"] * 10.0, q["y"] * 10.0] for q in pts])
    # a ring of anchors round the edge, on the same law from the middle, so the spline does not run wild out there
    mid = P.mean(axis=0)
    xs = g["x0"] + np.array([0, 0.5, 1]) * g["nx"] * g["res"]
    ys = g["y0"] + np.array([0, 0.5, 1]) * g["ny"] * g["res"]
    ring = np.array([[x, y] for x in xs for y in ys if not (x == xs[1] and y == ys[1])])
    A = np.vstack([P, ring])
    D = np.hypot(*(A[:, None, :] - A[None, :, :]).transpose(2, 0, 1))
    T = 40.0 * 195.3 * np.sqrt(D / 1000.0)                                            # target, real-equivalent at 1:40
    W = np.where(D > 0, 1.0 / np.maximum(T, 1.0) ** 2, 0.0)
    X = mid + (A - mid) * (np.median(T[D > 0] / D[D > 0]))
    n = len(A)
    V = -W.copy()
    np.fill_diagonal(V, W.sum(axis=1))
    Vp = np.linalg.pinv(V)
    for it in range(400):                                                              # SMACOF (Guttman transform)
        Dx = np.hypot(*(X[:, None, :] - X[None, :, :]).transpose(2, 0, 1))
        B = np.where(Dx > 1e-9, -W * T / np.maximum(Dx, 1e-9), 0.0)
        np.fill_diagonal(B, 0.0)
        np.fill_diagonal(B, -B.sum(axis=1))
        X = Vp @ (B @ X)
    Dx = np.hypot(*(X[:, None, :] - X[None, :, :]).transpose(2, 0, 1))
    m = D > 0
    log("sqrt map: %d places + %d anchors; distance error median %.0f%%, worst 10%% over %.0f%%" % (
        len(P), len(ring), 100 * np.median(np.abs(Dx[m] / T[m] - 1)), 100 * np.quantile(np.abs(Dx[m] / T[m] - 1), 0.9)))
    # keep the map's centre where it was (Dorestad roughly in place)
    X = X - X[:len(P)].mean(axis=0) + P.mean(axis=0)
    rbf = RBFInterpolator(A, X, kernel="thin_plate_spline", smoothing=1e3)
    gx = g["x0"] + (np.arange(g["nx"]) + 0.5) * g["res"]
    gy = g["y0"] + (np.arange(g["ny"]) + 0.5) * g["res"]
    GX, GY = np.meshgrid(gx, gy)
    out = np.empty((g["ny"], g["nx"], 2))
    flat = np.column_stack([GX.ravel(), GY.ravel()])
    for i in range(0, len(flat), 200000):
        out.reshape(-1, 2)[i:i + 200000] = rbf(flat[i:i + 200000])
    return out[..., 0], out[..., 1]


# ---------------------------------------------------------------------------------------------------- places, voyages
RANK_BY_KIND = {"emporium": 4.0, "royal seat": 3.0, "market": 2.5, "town": 2.0, "monastery": 1.5, "fort": 1.5, "village": 1.0}


def places(frame):
    p793 = {p["id"]: p for p in json.load(open(PORTS, encoding="utf-8"))["ports"]}
    seen, out = set(), []
    lv = json.load(open(LEVELS, encoding="utf-8"))["levels"]
    allp = list(p793.values()) + [p for L in lv for p in L.get("ports", [])]
    for p in allp:
        if p["id"] in seen or "lat" not in p:
            continue
        seen.add(p["id"])
        rich = p793.get(p["id"], p)
        rank = float(rich.get("size", 0)) or RANK_BY_KIND.get(rich.get("kind", ""), 1.0)
        x, y = frame.ll_to_real([(rich["lon"], rich["lat"])])[0]
        out.append(dict(id=p["id"], name=rich.get("name", p["id"]), modern=rich.get("modern", ""), kind=rich.get("kind", ""),
                        rank=rank, x=round(x / 10.0), y=round(y / 10.0)))
    out.sort(key=lambda q: -q["rank"])
    log("places: %d (%s)" % (len(out), ", ".join("%g: %d" % (r, sum(1 for q in out if q["rank"] == r)) for r in sorted({q["rank"] for q in out}, reverse=True))))
    return out


def along(raw, a_ll, b_ll, frame):
    """The part of a traced river between the points nearest two lon/lats (real metres), in that order."""
    P = raw["P"]
    a, b = frame.ll_to_real([a_ll, b_ll])
    i = int(np.argmin(np.hypot(*(P - a).T)))
    j = int(np.argmin(np.hypot(*(P - b).T)))
    return P[i:j + 1] if i <= j else P[j:i + 1][::-1]


def voyages(frame, raws):
    R = {r["id"]: r for r in raws}

    def sea(*ll):
        return frame.ll_to_real([(lo, la) for la, lo in ll])

    out = []
    dl = np.vstack([along(R["kromme_rijn"], (5.343, 51.972), (5.121, 52.091), frame),
                    along(R["oude_rijn"], (5.121, 52.091), (4.30, 52.215), frame),
                    sea((52.10, 3.60), (51.62, 2.20), (51.42, 1.62), (51.50, 1.00)),
                    along(R["thames"], (1.00, 51.50), (-0.118, 51.509), frame)])
    out.append(dict(id="lundenwic", name="Dorestad to Lundenwic (London)", via="down the Rhine, across, up the Thames", p=dl))
    out.append(dict(id="haithabu", name="Dorestad to Haithabu (Hedeby)", via="the Vecht, the Vlie, the Frisian coast, the Eider, a portage",
                    p=np.vstack([along(R["kromme_rijn"], (5.343, 51.972), (5.121, 52.091), frame),
                                 along(R["vecht"], (5.121, 52.091), (5.10, 52.375), frame),
                                 sea((52.80, 5.30), (53.30, 5.05), (53.62, 6.30), (53.86, 7.90), (54.25, 8.62), (54.38, 9.05),
                                     (54.46, 9.33), (54.49, 9.57))])))
    out.append(dict(id="birka", name="Haithabu to Birka", via="the Schlei, the Danish sounds, the Swedish coast",
                    p=sea((54.49, 9.57), (54.67, 10.03), (54.55, 11.10), (55.00, 12.60), (55.38, 12.85), (56.10, 14.90),
                          (56.60, 16.45), (57.80, 16.85), (58.75, 17.90), (59.17, 17.62), (59.335, 17.54))))
    out.append(dict(id="kiev", name="Birka to Kiev (Kænugarðr)", via="the Gulf of Finland, the Neva, Ladoga, the Volkhov, the Lovat, a portage, the Dnieper",
                    p=sea((59.335, 17.54), (59.70, 19.20), (60.05, 20.80), (59.85, 23.50), (60.05, 26.50), (59.95, 29.20),
                          (59.93, 30.30), (60.20, 31.10), (60.12, 32.30), (59.99, 32.30), (59.30, 31.95), (58.52, 31.27),
                          (58.25, 31.40), (57.90, 31.35), (57.30, 31.25), (56.80, 30.95), (56.34, 30.53), (55.70, 31.20),
                          (54.78, 31.88), (54.20, 30.90), (53.90, 30.33), (53.10, 30.10), (52.40, 30.45), (51.60, 30.55),
                          (50.45, 30.52))))
    out.append(dict(id="iceland", name="Dorestad to Iceland", via="the Rhine mouth, the North Sea, Shetland, the Faroes",
                    p=np.vstack([along(R["kromme_rijn"], (5.343, 51.972), (5.121, 52.091), frame),
                                 along(R["oude_rijn"], (5.121, 52.091), (4.30, 52.215), frame),
                                 sea((53.50, 3.80), (56.00, 2.50), (59.50, -0.80), (60.15, -1.15), (62.00, -6.80),
                                     (63.40, -16.00), (63.80, -18.00), (63.80, -21.00), (64.15, -21.95))])))
    out.append(dict(id="vinland", name="Iceland to Vinland", via="Snaefellsnes, Cape Farewell, the Eastern Settlement, Labrador",
                    p=sea((64.15, -21.95), (64.85, -24.20), (63.00, -33.00), (60.80, -42.00), (59.75, -43.90), (60.90, -45.60),
                          (61.50, -48.50), (60.00, -55.00), (56.50, -60.50), (53.80, -56.50), (51.60, -55.53))))
    for v in out:
        v["real_km"] = round(float(np.hypot(*np.diff(v["p"], axis=0).T).sum()) / 1000.0, 1)
        v["p"] = np.round(v["p"] / 10.0).astype(int).ravel().tolist()
    log("voyages: %s" % ", ".join("%s %.0f km" % (v["id"], v["real_km"]) for v in out))
    return out


GOTO = [("The whole world", None, None, 0), ("The North Sea", 3.0, 55.0, 1300),
        ("Dorestad", 5.343, 51.972, 12), ("Lundenwic (London)", -0.118, 51.509, 12), ("Colonia (Cologne)", 6.962, 50.938, 12),
        ("Rouen on the Seine", 1.10, 49.437, 12), ("Andoverpis (Antwerp)", 4.397, 51.222, 15), ("Haithabu (Hedeby)", 9.57, 54.49, 25),
        ("Birka", 17.54, 59.335, 40), ("Novgorod", 31.27, 58.52, 40), ("Kiev", 30.52, 50.45, 40),
        ("Iceland", -19.0, 64.9, 600), ("Vinland", -55.53, 51.6, 200)]


def goto(frame):
    out = []
    for name, lon, lat, km in GOTO:
        if lon is None:
            out.append(dict(name=name))
        else:
            x, y = frame.ll_to_real([(lon, lat)])[0]
            out.append(dict(name=name, x=round(x / 10.0), y=round(y / 10.0), km=km))
    return out


# ---------------------------------------------------------------------------------------------------- main
def main():
    os.makedirs(OUT, exist_ok=True)
    frame = mt.Frame(1.0, 1.0)
    land, lakes, dom = land_and_lakes(frame)
    sizes = {"land.bin": write_rings(os.path.join(OUT, "land.bin"), poly_rings(land, 1e6)),
             "lakes.bin": write_rings(os.path.join(OUT, "lakes.bin"), poly_rings(lakes))}
    lines, widths = other_rivers(frame, dom)
    sizes["rivers.bin"] = write_rings(os.path.join(OUT, "rivers.bin"), lines, widths)
    rivers, raws = game_rivers(frame)
    g = grid_for(land, lakes)
    log("warp grid %d x %d at %.0f km" % (g["nx"], g["ny"], g["res"] / 1000))
    corridor = [r["P"] for r in raws] + [l for l, w in zip(lines, widths) if w >= 120]
    maps = []
    pl = places(frame)
    for spec in MAPS:
        m = {k: v for k, v in spec.items() if k not in ("centre",)}
        if spec["kind"] == "warp":
            rho = density(g, spec, corridor)
            px, py = flow_cartogram(rho, g["res"])
            ls, det = land_scale(g, px, py)
            X = g["x0"] + (px + 0.5) * g["res"]
            Y = g["y0"] + (py + 0.5) * g["res"]
            cx, cy = X[g["land"]].mean(), Y[g["land"]].mean()
            X, Y = cx + (X - cx) / ls, cy + (Y - cy) / ls                # the land back at its own scale
            sea_lin = float(np.sqrt(np.median(det[(rho < 0.3) & ~g["land"]]))) / ls if (rho < 0.3).any() else 1.0
            m["warp"] = "warp_%s.bin" % spec["id"]
            m["measured"] = dict(land=1.0, sea=round(sea_lin, 3))
            write_warp(os.path.join(OUT, m["warp"]), g, X, Y)
            log("map %s: warped; the open sea at %.2f of the land's scale (1:%.0f)" % (spec["id"], sea_lin, spec["scale"] / max(sea_lin, 1e-6)))
        elif spec["kind"] == "sqrt":
            X, Y = sqrt_warp(g, frame, pl, raws)
            m["warp"] = "warp_%s.bin" % spec["id"]
            write_warp(os.path.join(OUT, m["warp"]), g, X, Y)
            log("map %s: square-root warp" % spec["id"])
        elif spec["kind"] == "radial":
            X, Y = radial_warp(g, frame, spec)
            m["warp"] = "warp_%s.bin" % spec["id"]
            write_warp(os.path.join(OUT, m["warp"]), g, X, Y)
            log("map %s: radial warp" % spec["id"])
        maps.append(m)
    data = dict(
        about="Kapman scale maps (4 Oct 2026): real metres of the world map's Lambert conformal projection (%s), in 10 m units "
              "in the .bin files and the lists here; each map divides by its scale (game metres), after its warp if it has one."
              % mt.PROJ,
        ship=dict(length=7.7, beam=1.5, speed=SHIP_SPEED, name="the Gislinge"),
        maps=maps, rivers=rivers, places=pl, voyages=voyages(frame, raws), goto=goto(frame),
        credits=[s["credit"] for s in mt.SOURCES if "EMODnet" not in s["name"]],
        files=sizes)
    with open(os.path.join(OUT, "maps.json"), "w", encoding="utf-8") as f:
        json.dump(data, f, separators=(",", ":"))
    log("done: %s, maps.json %.1f MB" % (", ".join("%s %.1f MB" % (k, v / 1e6) for k, v in sizes.items()),
                                          os.path.getsize(os.path.join(OUT, "maps.json")) / 1e6))


if __name__ == "__main__":
    main()
