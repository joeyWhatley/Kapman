# -*- coding: utf-8 -*-
r"""
MAKE TRACE MAPS (the user, 4 Oct 2026 late: "create a simple traceable map I can go over to make my pixel art map"):
clean black-and-white sheets of the real world shapes, to open in Aseprite as a faint reference layer and trace in pixels.
Reads the scale-map bake (G:\kapman_unity\data\scale_maps: land.bin, lakes.bin, rivers.bin, maps.json; 10 m units in the
world map's Lambert projection). Writes PNGs to Kapman2/Design/trace_maps/.
  run.bat world\make_trace_map.py
Sheets: the whole world; the North Sea (1300 km); the home waters (London to Cologne, the Channel to the Danish border);
each with a km grid, the places of 793 ranked, the game rivers heavier than the rest.
"""
import io, json, os, struct
import numpy as np
import shapely
from shapely.strtree import STRtree
from PIL import Image, ImageDraw, ImageFont

SRC = r"G:\kapman_unity\data\scale_maps"
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Design", "trace_maps")
FONT = r"C:\Windows\Fonts\segoeui.ttf"
FONT_B = r"C:\Windows\Fonts\segoeuib.ttf"

SEA = (255, 255, 255)
LAND = (236, 232, 234)
COAST = (0, 0, 0)
RIVER = (39, 56, 94)
GAME_RIVER = (112, 41, 99)          # Byzantium: the rivers the game sails
LAKE = (255, 255, 255)
GRID = (205, 200, 203)
INK = (26, 11, 30)


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
        pts = np.frombuffer(f.read(), dtype="<i4").reshape(-1, 2).astype(np.float64) * 10.0   # metres
    rings, i = [], 0
    for L in lens:
        rings.append(pts[i:i + L])
        i += L
    return rings, extra


class Sheet:
    def __init__(self, name, title, cx, cy, km, width_px, grid_km, place_rank, label_rank, note):
        self.name, self.title, self.note = name, title, note
        self.cx, self.cy, self.km, self.W = cx, cy, km, width_px
        self.grid_km, self.place_rank, self.label_rank = grid_km, place_rank, label_rank

    def frame(self):
        half = self.km * 500.0
        return self.cx - half, self.cy - half, self.cx + half, self.cy + half


def main():
    os.makedirs(OUT, exist_ok=True)
    land, _ = read_rings(os.path.join(SRC, "land.bin"))
    lakes, _ = read_rings(os.path.join(SRC, "lakes.bin"))
    rivers, widths = read_rings(os.path.join(SRC, "rivers.bin"))
    with io.open(os.path.join(SRC, "maps.json"), "r", encoding="utf-8") as f:
        maps = json.load(f)
    places = maps["places"]
    game_rivers = maps["rivers"].get("1_true", [])
    print("land rings %d, lakes %d, rivers %d, places %d, game rivers %d" % (len(land), len(lakes), len(rivers), len(places), len(game_rivers)))

    # ring parity: a ring inside another is a hole (water), inside two is land again
    polys = [shapely.Polygon(r) for r in land if len(r) >= 4]
    polys = [p if p.is_valid else p.buffer(0) for p in polys]
    areas = np.array([p.area for p in polys])
    big = [i for i in range(len(polys)) if areas[i] > 1e6]
    tree = STRtree([polys[i] for i in big])
    depth = np.zeros(len(polys), dtype=int)
    reps = [p.representative_point() for p in polys]
    for i, rp in enumerate(reps):
        hits = tree.query(rp, predicate="within")
        depth[i] = sum(1 for h in hits if big[h] != i)
    order = np.argsort(depth, kind="stable")

    allx = np.concatenate([r[:, 0] for r in land])
    ally = np.concatenate([r[:, 1] for r in land])
    wx0, wy0, wx1, wy1 = allx.min(), ally.min(), allx.max(), ally.max()
    wcx, wcy = (wx0 + wx1) / 2.0, (wy0 + wy1) / 2.0
    wkm = max(wx1 - wx0, wy1 - wy0) / 1000.0

    sheets = [
        Sheet("trace_world", "The sailing world of Kapman, 793-1066: Ireland to Novgorod, Lofoten to Bordeaux (2900 km; real shapes)", 925000, -25000, 2900, 4096, 200, 3.0, 4.0,
              "Trace the coast in black and the rivers in blue. Purple rivers are the ones the game sails."),
        Sheet("trace_north_sea", "The North Sea (1300 km across)", 505830, -299930, 1300, 4096, 100, 3.0, 3.5,
              "Dorestad, London, Quentovic, Ribe, Hedeby and Kaupang are the big dots."),
        Sheet("trace_home_waters", "Home waters: London to Cologne, the Channel to the Danish border (520 km)", 550000, -520000, 520, 4096, 50, 0.0, 2.0,
              "The Rhine, Maas and Scheldt mouths at true width; Frisia; the Thames. Every place of 793 is marked."),
    ]
    for s in sheets:
        render(s, land, polys, order, depth, lakes, rivers, widths, game_rivers, places)


def render(s, land, polys, order, depth, lakes, rivers, widths, game_rivers, places):
    x0, y0, x1, y1 = s.frame()
    W = s.W
    H = int(round(W * (y1 - y0) / (x1 - x0)))
    m_per_px = (x1 - x0) / W
    margin = 160
    img = Image.new("RGB", (W + 2 * margin, H + 2 * margin), SEA)
    d = ImageDraw.Draw(img)
    f_small = ImageFont.truetype(FONT, 18)
    f_place = ImageFont.truetype(FONT, 22)
    f_big = ImageFont.truetype(FONT_B, 26)
    f_title = ImageFont.truetype(FONT_B, 40)

    def to_px(pts):
        xs = margin + (pts[:, 0] - x0) / m_per_px
        ys = margin + (y1 - pts[:, 1]) / m_per_px            # y up in the world, down on the sheet
        return list(zip(xs.tolist(), ys.tolist()))

    def visible(pts):
        return not (pts[:, 0].max() < x0 or pts[:, 0].min() > x1 or pts[:, 1].max() < y0 or pts[:, 1].min() > y1)

    # land and its holes, by parity
    for i in order:
        r = np.asarray(polys[i].exterior.coords)[:, :2]
        if len(r) < 3 or not visible(r):
            continue
        d.polygon(to_px(r), fill=LAND if depth[i] % 2 == 0 else SEA)
    for r in lakes:
        if len(r) >= 3 and visible(r):
            d.polygon(to_px(r), fill=LAKE)
    # the grid
    g = s.grid_km * 1000.0
    gx = np.arange(np.floor(x0 / g) * g, x1 + g, g)
    gy = np.arange(np.floor(y0 / g) * g, y1 + g, g)
    for k, X in enumerate(gx):
        px = margin + (X - x0) / m_per_px
        if margin <= px <= margin + W:
            d.line([(px, margin), (px, margin + H)], fill=GRID, width=1)
            d.text((px + 4, margin - 24), str(int(round((X - gx[0]) / g))), fill=GRID, font=f_small)
    for k, Y in enumerate(gy):
        py = margin + (y1 - Y) / m_per_px
        if margin <= py <= margin + H:
            d.line([(margin, py), (margin + W, py)], fill=GRID, width=1)
            d.text((margin - 40, py - 10), chr(65 + (k % 26)), fill=GRID, font=f_small)
    # coast lines
    for i in order:
        r = np.asarray(polys[i].exterior.coords)[:, :2]
        if len(r) < 3 or not visible(r):
            continue
        d.line(to_px(r), fill=COAST, width=1)
    for r in lakes:
        if len(r) >= 3 and visible(r):
            d.line(to_px(r), fill=RIVER, width=1)
    # rivers: HydroRIVERS, heavier when wider
    wmax = float(np.percentile(widths, 97)) if widths is not None and len(widths) else 1.0
    for r, w in zip(rivers, widths if widths is not None else [1] * len(rivers)):
        if len(r) < 2 or not visible(r):
            continue
        px_w = 1 if w < 0.35 * wmax else 2
        d.line(to_px(r), fill=RIVER, width=px_w)
    # the game's rivers, traced at true width
    for gr in game_rivers:
        p = np.asarray(gr["p"], dtype=np.float64).reshape(-1, 2) * 10.0
        if visible(p):
            d.line(to_px(p), fill=GAME_RIVER, width=3 if not gr.get("branch") else 2)
    # places
    for pl in sorted(places, key=lambda q: -q["rank"]):
        if pl["rank"] < s.place_rank:
            continue
        x, y = pl["x"] * 10.0, pl["y"] * 10.0
        if not (x0 <= x <= x1 and y0 <= y <= y1):
            continue
        px, py = margin + (x - x0) / m_per_px, margin + (y1 - y) / m_per_px
        rad = 3 + int(pl["rank"])
        d.ellipse([px - rad, py - rad, px + rad, py + rad], fill=INK if pl["rank"] >= 4 else SEA, outline=INK, width=2)
        if pl["rank"] >= s.label_rank and not pl["name"].startswith("("):
            d.text((px + rad + 3, py - 12), pl["name"].split(",")[0], fill=INK, font=f_place if pl["rank"] >= 4 else f_small)
    # frame, title, scale bar, note
    d.rectangle([margin, margin, margin + W, margin + H], outline=INK, width=2)
    d.text((margin, 40), s.title, fill=INK, font=f_title)
    bar_km = s.grid_km
    bar_px = bar_km * 1000.0 / m_per_px
    bx, by = margin, margin + H + 50
    d.rectangle([bx, by, bx + bar_px, by + 12], fill=INK)
    d.text((bx, by + 18), "%d km   (1 px = %.0f m; the Gislinge is 7.7 m long)" % (bar_km, m_per_px), fill=INK, font=f_big)
    d.text((bx, by + 56), s.note + "  Grid %d km. Black: coast. Blue: rivers and lakes. Purple: the game's rivers. Dots: places of 793, bigger is bigger."
           % s.grid_km, fill=INK, font=f_small)
    d.text((bx, by + 82), "In Aseprite: open this as a layer, lower its opacity to 30 %, lock it, draw on a layer above at your canvas size. Made from Natural Earth, Vos c. 800 (CC BY 4.0) and HydroRIVERS.",
           fill=GRID, font=f_small)
    path = os.path.join(OUT, s.name + ".png")
    img.save(path, optimize=True)
    print("wrote", path, img.size)


if __name__ == "__main__":
    main()
