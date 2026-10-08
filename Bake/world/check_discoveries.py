# -*- coding: utf-8 -*-
"""THE WATERS AND LANDMARKS TO FIND, CHECKED AGAINST THE SEA LANES (Game/Data/discoveries_793.json; the user, 9 Oct 2026:
"add waters to discover and land marks ect points of interest"). A thing is found when she comes within its reach. She
only ever sails the lanes, so a thing whose reach no lane enters can never be found: this says which, and how near the
nearest lane a ship like hers can use passes. It also says what is off the painted chart, what lies in the same place as
a port, and which two things are so near each other that one would do.   python check_discoveries.py
Exit code 1 if anything cannot be found."""
import io, json, math, sys
import frame

D = "G:/KAPMAN/Assets/Kapman/Game/Data/"
table = json.loads(io.open(D + "discoveries_793.json", encoding="utf-8").read())
kinds = table["kinds"]
things = table["things"]
art = json.loads(io.open("G:/KAPMAN/MapArt/world/map_art.json", encoding="utf-8-sig").read())
west, north = art["west"], art["north"]
size = art["metresPerPixel"] * art["tilePixels"]
east, south = west + art["cols"] * size, north - art["rows"] * size
ports = json.loads(io.open(D + "ports_793.json", encoding="utf-8-sig").read())["ports"]

# the lanes a ship can use (not the rivers that are not built, nor a haul over land), in a grid of 10 km squares
USABLE = {0, 1, 2, 3, 5, 7}
CELL = 10000.0
grid = {}
for x, z, kind in frame.lanes():
    if kind in USABLE:
        grid.setdefault((int(x // CELL), int(z // CELL)), []).append((x, z, kind))


def nearest(x, z, most=80000.0):
    best = (1e18, -1)
    r = int(most // CELL) + 1
    cx, cz = int(x // CELL), int(z // CELL)
    for dx in range(-r, r + 1):
        for dz in range(-r, r + 1):
            for px, pz, kind in grid.get((cx + dx, cz + dz), ()):
                d = math.hypot(px - x, pz - z)
                if d < best[0]:
                    best = (d, kind)
    return best


bad = 0
seen = set()
rows = []
for t in things:
    assert t["id"] not in seen, "twice: " + t["id"]
    seen.add(t["id"])
    assert t["kind"] in kinds, t["id"] + ": no such kind " + t["kind"]
    for key in ("name", "note", "today"):
        assert t.get(key, "").strip(), t["id"] + " has no " + key
    x, z = frame.to_world(t["lat"], t["lon"])
    t["x"], t["z"] = x, z
    reach = t.get("reach_km", kinds[t["kind"]]["reach_km"])
    d, kind = nearest(x, z)
    said = []
    if not (west <= x <= east and south <= z <= north):
        said.append("OFF THE PAINTED CHART")
        bad += 1
    if d / 1000.0 > reach * 0.85:
        said.append("CANNOT BE FOUND: the nearest lane passes %.0f km off, its reach is %d km" % (d / 1000.0, reach))
        bad += 1
    for p in ports:
        px, pz = frame.to_world(p["lat"], p["lon"])
        if math.hypot(px - x, pz - z) < 2500.0:
            said.append("in the same place as the port " + p["name"])
    rows.append((t, reach, d, kind, said))
for i, a in enumerate(things):
    for b in things[i + 1:]:
        if math.hypot(a["x"] - b["x"], a["z"] - b["z"]) < 6000.0:
            print("NEAR EACH OTHER (%.1f km): %s and %s" % (math.hypot(a["x"] - b["x"], a["z"] - b["z"]) / 1000.0, a["id"], b["id"]))
print("%-18s %-9s %-34s %5s %7s  %s" % ("id", "kind", "name", "reach", "lane", "the lane's kind"))
for t, reach, d, kind, said in rows:
    print("%-18s %-9s %-34s %3d km %4.0f km  %-9s %s" % (t["id"], t["kind"], t["name"][:34], reach, d / 1000.0,
                                                      frame.LANE_KINDS[kind] if kind >= 0 else "none within 80 km", "   <<< " + "; ".join(said) if said else ""))
by = {}
for t in things:
    by[t["kind"]] = by.get(t["kind"], 0) + 1
print("%d things: %s" % (len(things), ", ".join("%d %s" % (n, k) for k, n in sorted(by.items()))))
print("FAULTS: %d" % bad if bad else "every one of them can be found.")
sys.exit(1 if bad else 0)
