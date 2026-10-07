# THE NEW WORLD AT 1:80 (the user, 4 Oct 2026 pm): the real coast, lakes and the chosen rivers from Natural Earth 10 m
# (public domain), in a Lambert conformal projection centred on the North Atlantic, divided by 80 and written in game
# kilometres for the world map page; with the ports of the chosen levels from Design/levels_793.json.
# Run: Bake\run.bat world\make_world_map.py   ->   Bake/world/world_map.json
import json, os
from osgeo import ogr, osr

ROOT = r"G:\kapman_unity\Kapman2"
NE = r"G:\kapman_unity\data\raw\vectors\naturalearth\10m_physical.zip"
OUT = os.path.join(ROOT, "Bake", "world", "world_map.json")
SCALE = 80.0
BBOX = (-68.0, 41.0, 42.0, 74.0)          # lon min, lat min, lon max, lat max: Vinland to Kiev, Brittany to Greenland
PROJ = "+proj=lcc +lat_1=50 +lat_2=68 +lat_0=58 +lon_0=-5 +datum=WGS84 +units=m +no_defs"
TOL_M = 4000.0                            # simplify (real metres): 50 m in the game, plenty for the overview

RIVERS = {"rhine": "Rhine", "rhein": "Rhine", "rijn": "Rhine", "waal": "Waal", "ijssel": "IJssel", "lek": "Lek",
          "maas": "Meuse", "meuse": "Meuse", "schelde": "Scheldt", "scheldt": "Scheldt", "escaut": "Scheldt",
          "seine": "Seine", "thames": "Thames", "ouse": "Ouse", "trent": "Trent", "humber": "Humber",
          "neva": "Neva", "volkhov": "Volkhov", "lovat": "Lovat", "dnieper": "Dnieper", "dnepr": "Dnieper", "dnipro": "Dnieper",
          "desna": "Desna", "main": "Main", "moselle": "Moselle", "mosel": "Moselle", "canche": "Canche"}

LEVELS_OUT = ("O14", "O15", "O16", "O17", "O18", "O19", "O20")
COASTS_OUT = {"Asturias and Galicia", "The coast of Portugal", "The Algarve and the Gulf of C\u00e1diz", "The Strait of Gibraltar",
              "The coast of al-Andalus and the Balearics", "Catalonia and the Ebro", "The Gulf of Lion and Provence",
              "Liguria and Tuscany", "Rome and the Tiber", "Campania", "Sicily and Ifriqiya", "Venetia and the Po",
              "The Dardanelles and the Bosporus", "The Bulgar coast and the Danube", "The Dnieper mouth", "Crimea",
              "The Sea of Azov and the Don", "The Anatolian coast", "The Volga delta", "The western shore", "The southern shore",
              "The White Sea", "The Loire mouth", "The Charente and the Gironde"}
ROADS_IN = {"The Rhine road", "The Meuse road", "The Scheldt road", "The Seine road", "The road to the Greeks"}

src = osr.SpatialReference()
src.ImportFromEPSG(4326)
src.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
dst = osr.SpatialReference()
dst.ImportFromProj4(PROJ)
to_game = osr.CoordinateTransformation(src, dst)


def layer(name):
    return ogr.Open("/vsizip/" + NE.replace("\\", "/") + "/" + name)


def clip_geom():
    ring = ogr.Geometry(ogr.wkbLinearRing)
    x0, y0, x1, y1 = BBOX
    for x, y in ((x0, y0), (x1, y0), (x1, y1), (x0, y1), (x0, y0)):
        ring.AddPoint_2D(x, y)
    poly = ogr.Geometry(ogr.wkbPolygon)
    poly.AddGeometry(ring)
    return poly


CLIP = clip_geom()


def km(g):
    """A projected, simplified geometry's rings or lines in game km (rounded to 10 m)."""
    out = []

    def walk(geom):
        t = ogr.GT_Flatten(geom.GetGeometryType())
        if t in (ogr.wkbLineString, ogr.wkbLinearRing):
            pts = [(round(geom.GetX(i) / SCALE / 1000.0, 2), round(geom.GetY(i) / SCALE / 1000.0, 2)) for i in range(geom.GetPointCount())]
            dedup = [p for i, p in enumerate(pts) if i == 0 or p != pts[i - 1]]
            if len(dedup) >= 2:
                out.append(dedup)
        else:
            for i in range(geom.GetGeometryCount()):
                walk(geom.GetGeometryRef(i))

    walk(g)
    return out


def collect(name, keep=None, simplify=TOL_M, min_area_km2=0.0):
    ds = layer(name)
    lyr = ds.GetLayer(0)
    rings = []
    for f in lyr:
        if keep is not None and not keep(f):
            continue
        g = f.GetGeometryRef()
        if g is None:
            continue
        g = g.Intersection(CLIP)
        if g is None or g.IsEmpty():
            continue
        g = g.Clone()
        g.Transform(to_game)
        if min_area_km2 > 0 and g.GetArea() / 1e6 < min_area_km2:
            continue
        g = g.SimplifyPreserveTopology(simplify)
        for r in km(g):
            rings.append((f, r))
    return rings


def field(f, *names):
    for n in names:
        i = f.GetFieldIndex(n)
        if i >= 0 and f.GetField(i):
            return f.GetField(i)
    return ""


land = [r for _, r in collect("ne_10m_land.shp", min_area_km2=30.0)]
islands = [r for _, r in collect("ne_10m_minor_islands.shp", min_area_km2=5.0)]
lakes = [r for _, r in collect("ne_10m_lakes.shp", keep=lambda f: (f.GetField("scalerank") or 9) <= 6, min_area_km2=150.0)]

rivers = {}
for lname in ("ne_10m_rivers_lake_centerlines.shp", "ne_10m_rivers_europe.shp"):
    for f, r in collect(lname, simplify=1500.0):
        words = str(field(f, "name", "name_en")).lower().replace("-", " ").split()
        key = next((v for k, v in RIVERS.items() if k in words), None)
        if key:
            rivers.setdefault(key, []).append(r)


def pt(lat, lon):
    x, y, _ = to_game.TransformPoint(lon, lat)
    return round(x / SCALE / 1000.0, 2), round(y / SCALE / 1000.0, 2)


levels = json.load(open(os.path.join(ROOT, "Design", "levels_793.json"), encoding="utf-8"))["levels"]
ports, places = {}, []
for L in levels:
    kind, name = L.get("kind"), L.get("name")
    if kind == "ocean":
        if L.get("level") in LEVELS_OUT:
            continue
        places.append({"kind": "ocean", "name": name, "id": L.get("level")})
        continue
    if kind == "coast_stretch" and (name in COASTS_OUT or L.get("ocean") in LEVELS_OUT):
        continue
    if kind == "river_road" and name not in ROADS_IN:
        continue
    places.append({"kind": kind, "name": name})
    for p in L.get("ports", []):
        lat, lon = p.get("lat"), p.get("lon")
        if lat is None or lon is None or not (BBOX[0] <= lon <= BBOX[2] and BBOX[1] <= lat <= BBOX[3]):
            continue
        x, y = pt(lat, lon)
        ports[p["id"]] = {"id": p["id"], "name": p.get("name", p["id"]), "kind": p.get("kind", ""), "x": x, "y": y,
                          "from": p.get("from", 0), "to": p.get("to", 0), "place": name, "modern": p.get("modern", "")}

p793 = json.load(open(os.path.join(ROOT, "Assets", "Kapman", "Game", "Data", "ports_793.json"), encoding="utf-8"))["ports"]
for p in p793:
    if not (BBOX[0] <= p["lon"] <= BBOX[2] and BBOX[1] <= p["lat"] <= BBOX[3]):
        continue
    x, y = pt(p["lat"], p["lon"])
    q = ports.setdefault(p["id"], {"id": p["id"], "name": p["name"], "kind": p.get("kind", ""), "x": x, "y": y,
                                   "from": p.get("from", 0), "to": p.get("to", 0), "place": p.get("region", ""),
                                   "modern": p.get("modern", "")})
    q["size"] = p.get("size", 0)

xs = [x for r in land for x, _ in r]
ys = [y for r in land for _, y in r]
out = {"about": "Kapman's new world at 1:80 (4 Oct 2026): game kilometres (x east, y north) in a Lambert conformal "
                "projection (" + PROJ + ") divided by 80; Natural Earth 10 m (public domain) coast, lakes and rivers "
                "simplified at 4 km real; ports from Design/levels_793.json and ports_793.json.",
       "scale": SCALE, "bounds": [min(xs), min(ys), max(xs), max(ys)],
       "land": land, "islands": islands, "lakes": lakes, "rivers": rivers, "ports": list(ports.values()), "places": places}
json.dump(out, open(OUT, "w", encoding="utf-8"), ensure_ascii=False, separators=(",", ":"))
print("land rings", len(land), "islands", len(islands), "lakes", len(lakes), "rivers", {k: len(v) for k, v in rivers.items()},
      "ports", len(ports), "bounds", out["bounds"], "bytes", os.path.getsize(OUT))
