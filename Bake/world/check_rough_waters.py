# -*- coding: utf-8 -*-
"""THE ROUGH WATERS, CHECKED (Game/Data/rough_waters_793.json; the user, 9 Oct 2026: "... and rough ocean areas").
Each is a real place where the sea is worse than the wind alone makes it. This says of every one: is its place water
by the world bake and how deep; how near the nearest lane a ship like hers can use passes (none within its reach: she
never comes there); how hard the tide's stream runs there at its hardest, as the game reckons it from the tide model
(a race with no stream is no race); and whether the thing to find it is tied to is in discoveries_793.json and will
have been found by the time she is in the rough of it.   python check_rough_waters.py
Exit code 1 if anything is wrong."""
import io, json, math, os, struct, sys, zlib
import numpy as np
import frame

D = "G:/KAPMAN/Assets/Kapman/Game/Data/"
BAKE = "G:/kapman_unity/data/baked/world"

waters = json.loads(io.open(D + "rough_waters_793.json", encoding="utf-8").read())["waters"]
table = json.loads(io.open(D + "discoveries_793.json", encoding="utf-8").read())
finds = {t["id"]: t for t in table["things"]}
kinds = table["kinds"]

# ---------------------------------------------------------------------------- the bake: is it water, how deep
world = json.load(io.open(BAKE + "/world.json", encoding="utf-8"))
west, north = world["bounds"]["west"], world["bounds"]["north"]
names = {s["id"]: s["name"] for s in world["surfaces"]}
tiles = {}


def tile(level, col, row):
    key = (level, col, row)
    if key not in tiles:
        path = os.path.join(BAKE, "L%d" % level, str(row), "%d.kt" % col)
        if not os.path.exists(path):
            tiles[key] = None
        else:
            with open(path, "rb") as f:
                assert f.read(4) == b"KPT1"
                _lv, _c, _r, cells = struct.unpack("<4i", f.read(16))
                f.read(8)
                n = struct.unpack("<i", f.read(4))[0]
                h = np.frombuffer(zlib.decompress(f.read(n)), dtype="<f2").astype(np.float32).reshape(cells, cells)
                n = struct.unpack("<i", f.read(4))[0]
                s = np.frombuffer(zlib.decompress(f.read(n)), dtype=np.uint8).reshape(cells, cells)
            tiles[key] = (h, s)
    return tiles[key]


def ground(x, z):
    for level in (0, 1, 2):
        cell = 30.0 * (1 << level)
        c, r = int((x - west) // cell), int((north - z) // cell)
        t = tile(level, c // 256, r // 256)
        if t is not None:
            return float(t[0][r % 256, c % 256]), names.get(int(t[1][r % 256, c % 256]), "?")
    return 0.0, "?"


# ---------------------------------------------------------------------------- the lanes a ship can use
USABLE = {0, 1, 2, 3, 5, 7}
CELL = 10000.0
grid = {}
for x, z, kind in frame.lanes():
    if kind in USABLE:
        grid.setdefault((int(x // CELL), int(z // CELL)), []).append((x, z))


def nearest_lane(x, z, most=120000.0):
    best = 1e18
    r = int(most // CELL) + 1
    cx, cz = int(x // CELL), int(z // CELL)
    for dx in range(-r, r + 1):
        for dz in range(-r, r + 1):
            for px, pz in grid.get((cx + dx, cz + dz), ()):
                best = min(best, math.hypot(px - x, pz - z))
    return best


# ---------------------------------------------------------------------------- the tide's stream, as TideModel.StreamAt
b = open(BAKE + "/tides.bin", "rb").read()
n, = struct.unpack_from("<i", b, 4)
tw, ts, step = struct.unpack_from("<3d", b, 8)
nx, ny = struct.unpack_from("<2i", b, 32)
tnames = [b[40 + 8 * i: 48 + 8 * i].decode("ascii").rstrip("\0") for i in range(n)]
off = 40 + 8 * n
RE, IM = [], []
for c in range(n):
    RE.append(np.frombuffer(b, dtype="<f2", count=nx * ny, offset=off).astype(float).reshape(ny, nx)); off += nx * ny * 2
    IM.append(np.frombuffer(b, dtype="<f2", count=nx * ny, offset=off).astype(float).reshape(ny, nx)); off += nx * ny * 2
SPEED = {"M2": 1.405189e-4, "S2": 1.454441e-4, "N2": 1.378797e-4, "K2": 1.458423e-4, "2N2": 1.352405e-4, "K1": 7.292117e-5, "O1": 6.759774e-5,
         "P1": 7.252295e-5, "Q1": 6.495854e-5, "M4": 2.810378e-4}


def stream_most(lat, lon, depth):
    fx, fy = (lon - tw) / step, (lat - ts) / step
    if fx < 0 or fy < 0 or fx > nx - 1 or fy > ny - 1:
        return 0.0
    x0, y0 = min(nx - 2, int(fx)), min(ny - 2, int(fy))
    tx, ty = fx - x0, fy - y0
    h = max(4.0, min(400.0, depth)); cel = math.sqrt(9.81 * h); per = math.sqrt(9.81 / h)
    mx = step * 111320.0 * math.cos(math.radians(lat)); my = step * 110574.0
    most = 0.0
    for k in range(0, 150):
        t = k * 600.0
        e = nn = 0.0
        for c, name in enumerate(tnames):
            w = SPEED[name]; r, q = RE[c], IM[c]
            a00, a10, a01, a11 = r[y0, x0], r[y0, x0 + 1], r[y0 + 1, x0], r[y0 + 1, x0 + 1]
            b00, b10, b01, b11 = q[y0, x0], q[y0, x0 + 1], q[y0 + 1, x0], q[y0 + 1, x0 + 1]
            if any(u == 0 and v == 0 for u, v in ((a00, b00), (a10, b10), (a01, b01), (a11, b11))):
                continue
            a = (a00 * (1 - tx) + a10 * tx) * (1 - ty) + (a01 * (1 - tx) + a11 * tx) * ty
            bb = (b00 * (1 - tx) + b10 * tx) * (1 - ty) + (b01 * (1 - tx) + b11 * tx) * ty
            amp2 = a * a + bb * bb
            if amp2 < 1e-6:
                continue
            ax = ((a10 - a00) * (1 - ty) + (a11 - a01) * ty) / mx; ay = ((a01 - a00) * (1 - tx) + (a11 - a10) * tx) / my
            bx = ((b10 - b00) * (1 - ty) + (b11 - b01) * ty) / mx; by = ((b01 - b00) * (1 - tx) + (b11 - b10) * tx) / my
            gx = (a * bx - bb * ax) / amp2; gy = (a * by - bb * ay) / amp2
            g = math.hypot(gx, gy)
            if g < 1e-12:
                continue
            run = (a * math.cos(w * t) + bb * math.sin(w * t)) * per * min(1.0, g * cel / w)
            e += run * gx / g; nn += run * gy / g
        most = max(most, math.hypot(e, nn))
    return most


# ---------------------------------------------------------------------------- the check
bad = 0
seen = set()
for w in waters:
    wid, kind, lat, lon, reach = w["id"], w["kind"], w["lat"], w["lon"], w["reach_km"]
    said = []
    if wid in seen:
        said.append("TWICE")
        bad += 1
    seen.add(wid)
    if kind not in ("race", "bar", "shoal", "cape"):
        said.append("NO SUCH KIND")
        bad += 1
    if kind == "bar" and "open_to" not in w:
        said.append("A BAR THAT DOES NOT SAY WHICH WAY IT LIES OPEN")
        bad += 1
    x, z = frame.to_world(lat, lon)
    h, surf = ground(x, z)
    lane = nearest_lane(x, z) / 1000.0
    most = stream_most(lat, lon, max(4.0, -h))
    if surf not in ("Sea", "Inland water", "Rivers of 800 AD"):
        said.append("its middle is " + surf)
    if lane > reach * 0.9:
        said.append("NO LANE COMES WITHIN ITS REACH (%.0f km off)" % lane)
        bad += 1
    if kind == "race" and most < 0.15:
        said.append("THE TIDE MODEL RUNS NO STREAM HERE (%.2f m/s)" % most)
        bad += 1
    find = w.get("find")
    if not find:
        said.append("TIED TO NO THING TO FIND")
        bad += 1
    elif find not in finds:
        said.append("NO SUCH FIND " + find)
        bad += 1
    else:
        fx, fz = frame.to_world(finds[find]["lat"], finds[find]["lon"])
        freach = finds[find].get("reach_km", kinds[finds[find]["kind"]]["reach_km"])
        apart = math.hypot(fx - x, fz - z) / 1000.0
        if apart + reach * 0.5 > freach:
            said.append("she may be in it before she has found %s (%.0f km apart, its reach %.0f)" % (find, apart, freach))
    print("%-14s %-6s %7.3f %8.3f  reach %2d  rough %.1f  ground %7.1f %-24s lane %5.1f km  stream most %.2f m/s  %s" %
          (wid, kind, lat, lon, reach, w["rough"], h, surf, lane, most, "; ".join(said)))
print("%d rough waters, %d wrong" % (len(waters), bad))
sys.exit(1 if bad else 0)
