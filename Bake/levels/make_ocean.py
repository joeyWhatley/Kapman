"""THE SOUTHERN NORTH SEA, AN OCEAN LEVEL - the sea from the Thames and Kent to the Rhine's mouth and Zeeland, shortened
hard (about 1:150), from the first build's whole-map bake of the coast as it was about 793 (data\\baked\\world, level 3,
240 m cells: the Vos 800 AD coast, the tidal flats, the 793 sea level).

Writes, for Unity (Kapman2's OceanBuilder):
  height.f32     float32 (N, N), real metres over the 793 sea level, row 0 = the south edge
  surface.u8     uint8 (N, N), the bake's surface types (1 sea, 2 tidal flats, 3 salt marsh, 6 dunes ...)
  meta.json      the box, the shortening, and where the real places fall in the level (game metres): the towns
                 ("places"), the sea marks and chalk coasts ("marks"), the great woods ("woods": centre and half-sizes)
                 and the weir grounds, all in the level's frame
  preview.png    a check plot

The level's frame: x east, z north, game metres from the south-west corner; 1 game metre = SCALE real metres across.
Run: Bake\\run.bat levels\\make_ocean.py
"""
import json
import math
import os
import sys

import numpy as np
from scipy import ndimage as ndi

sys.path.insert(0, r"G:\kapman_unity\Kapman\Bake")
from kapman_bake import tiles, world  # noqa: E402

STORE = r"G:\kapman_unity\data\baked\world"
OUT = r"G:\kapman_unity\data\levels\northsea_south"
BOX = (50.70, 53.40, -0.70, 5.20)       # lat min, lat max, lon min, lon max
SCALE = 150.0                           # real metres per game metre across
LEVEL = 3                               # 240 m cells
N = 2049

PLACES = {
    "Lundenwic": (51.511, -0.115),
    "The Thames mouth": (51.50, 0.95),
    "Dover": (51.122, 1.316),
    "Walichrum": (51.565, 3.497),
    "The Rhine mouth at Katwijk": (52.205, 4.395),
    "The Maas mouth": (51.95, 4.05),
    "Quentovic": (50.47, 1.67),
}

# Landmarks the ocean builder sets at the real spots, and the hints it paints by. The prefix says what each is for:
#   (none)     a sea mark standing in 793: a church, a Roman wall, a lighthouse (OceanBuilder makes it)
#   "Chalk: "  a chalk coast: the steep sea faces within about 9 km of the point are painted as chalk cliffs
#   "Weirs: "  a fishing ground: fish weirs stand on the tidal flats round the point
#   "Birds: "  great flats and marshes where the geese and gulls are put
MARKS = {
    # sea marks standing in 793 (sure, except where said)
    "Reculver": (51.379, 1.199),       # the minster church of 669 inside the Roman fort Regulbium, on the Thames approaches
    "Richborough": (51.293, 1.332),    # the Roman fort Rutupiae: its walls stood (and stand) on the Wantsum channel
    "Bradwell": (51.735, 0.938),       # St Peter-on-the-Wall (654) in the Roman fort Othona, at the Blackwater's mouth
    "Boulogne": (50.726, 1.605),       # Gesoriacum's Roman lighthouse (the Tour d'Ordre) on the cliff; it stood till 1644
    "Londinium": (51.513, -0.090),     # the Roman walled city east of the wic, St Paul's (604) inside, mostly empty in 793
    # the chalk coasts: Dover-Folkestone, Thanet, the Seven Sisters and Beachy Head, Brighton's cliffs, Cap Blanc-Nez
    "Chalk: Dover": (51.13, 1.35),
    "Chalk: Thanet": (51.38, 1.40),
    "Chalk: Beachy Head": (50.74, 0.25),
    "Chalk: Brighton": (50.80, -0.05),
    "Chalk: Blanc-Nez": (50.92, 1.72),
    # fishing grounds: the Essex weirs of the 7th-10th c. (Blackwater: Collins Creek, Sales Point) are excavated; the
    # Dutch and Flemish ones are a guess from the many later medieval weirs on the same flats
    "Weirs: the Blackwater": (51.74, 0.92),
    "Weirs: the Maas mouth": (51.93, 4.08),
    "Weirs: the Scheldt": (51.45, 3.75),
    "Weirs: the Wadden": (53.22, 5.00),
    # the birds' grounds
    "Birds: the Wadden flats": (53.25, 5.05),
    "Birds: the Essex marshes": (51.70, 0.85),
    "Birds: the Holland fens": (52.05, 4.35),
}

# The great woods of 793 as ellipses: centre, half-width east-west and half-height north-south (km). The Weald
# (Andredesweald) between the North and South Downs was the biggest wood in southern England (Anglo-Saxon Chronicle 893:
# 120 miles long, 30 broad); the Boulonnais hills behind Boulogne were wooded. Sizes are rounded guesses.
WOODS = {
    "The Weald": (51.05, 0.45, 48.0, 15.0),
    "The Boulonnais": (50.75, 1.85, 12.0, 10.0),
}


def main():
    os.makedirs(OUT, exist_ok=True)
    meta_w = json.load(open(os.path.join(STORE, "world.json")))
    west, north = meta_w["bounds"]["west"], meta_w["bounds"]["north"]
    T, c = world.tile_size(LEVEL), world.cell(LEVEL)
    w, s, e, n = world.extent_of(*BOX, snap_level=LEVEL)
    side = max(e - w, n - s)
    cx, cz = (w + e) / 2.0, (s + n) / 2.0
    w, e, s, n = cx - side / 2, cx + side / 2, cz - side / 2, cz + side / 2
    col0, col1 = int(math.floor((w - west) / T)), int(math.floor((e - west) / T))
    row0, row1 = int(math.floor((north - n) / T)), int(math.floor((north - s) / T))
    store = tiles.TileStore(STORE)
    H = np.full(((row1 - row0 + 1) * 256, (col1 - col0 + 1) * 256), np.nan, np.float32)
    S = np.zeros(H.shape, np.uint8)
    found = 0
    for r in range(row0, row1 + 1):
        for cc in range(col0, col1 + 1):
            t = store.read(LEVEL, cc, r)
            if t is None:
                continue
            found += 1
            y0, x0 = (r - row0) * 256, (cc - col0) * 256
            H[y0:y0 + 256, x0:x0 + 256] = t[0]
            S[y0:y0 + 256, x0:x0 + 256] = t[1]
    print("level %d tiles: %d of %d" % (LEVEL, found, (row1 - row0 + 1) * (col1 - col0 + 1)))
    nan = ~np.isfinite(H)
    if nan.any():
        idx = ndi.distance_transform_edt(nan, return_distances=False, return_indices=True)
        H = H[tuple(idx)]
    mx0, mz1 = west + col0 * T, north - row0 * T              # the mosaic's west and north edges
    k = np.arange(N) * (side / (N - 1))
    X, Z = np.meshgrid(w + k, s + k)
    cols = (X - mx0) / c - 0.5
    rows = (mz1 - Z) / c - 0.5
    HH = ndi.map_coordinates(H, [rows, cols], order=1, mode="nearest").astype(np.float32)
    SS = ndi.map_coordinates(S.astype(np.float32), [rows, cols], order=0, mode="nearest").astype(np.uint8)
    HH.tofile(os.path.join(OUT, "height.f32"))
    SS.tofile(os.path.join(OUT, "surface.u8"))

    size = side / SCALE

    def in_level(table):
        """Each named point in game metres, those outside the level left out."""
        out = {}
        for name, (lat, lon) in table.items():
            x, z = world.to_world(lat, lon)
            gx, gz = (x - w) / SCALE, (z - s) / SCALE
            if 0 <= gx <= size and 0 <= gz <= size:
                out[name] = [round(gx, 2), round(gz, 2)]
        return out

    places = in_level(PLACES)
    marks = in_level(MARKS)
    woods = {}
    for name, (lat, lon, rx_km, rz_km) in WOODS.items():
        x, z = world.to_world(lat, lon)
        gx, gz = (x - w) / SCALE, (z - s) / SCALE
        if 0 <= gx <= size and 0 <= gz <= size:
            woods[name] = [round(gx, 2), round(gz, 2), round(rx_km * 1000.0 / SCALE, 2), round(rz_km * 1000.0 / SCALE, 2)]
    from osgeo import osr
    t = osr.CoordinateTransformation(world.srs(), world.wgs84())
    lon_c, lat_c, _ = t.TransformPoint(cx, cz)
    meta = {
        "name": "The southern North Sea",
        "about": "An ocean level: the sea from the Thames and Kent to the Rhine's mouth, shortened about 1:%d" % SCALE,
        "real_extent_world": [w, s, e, n],
        "real_side_m": side,
        "scale": SCALE,
        "size": round(size, 3),
        "n": N,
        "height_min": round(float(HH.min()), 2),
        "height_max": round(float(HH.max()), 2),
        "center_latlon": [round(lat_c, 5), round(lon_c, 5)],
        "places": places,
        "marks": marks,
        "woods": woods,
        "source": "the first build's whole-map bake (793 coast: Vos 800 AD in the Netherlands, GLO-90/30, EMODnet, GEBCO)",
    }
    json.dump(meta, open(os.path.join(OUT, "meta.json"), "w"), indent=1)
    print("side %.0f km real -> %.0f m in the game; heights %.0f..%.0f m; places: %s; marks: %s; woods: %s"
          % (side / 1000, size, HH.min(), HH.max(), ", ".join(places), ", ".join(marks), ", ".join(woods)))

    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig, ax = plt.subplots(figsize=(10, 10))
    ax.imshow(np.clip(HH, -40, 60), origin="lower", extent=(0, size, 0, size), cmap="gist_earth", vmin=-40, vmax=60)
    ax.contour(k / SCALE, k / SCALE, HH, levels=[0], colors="k", linewidths=0.5)
    for name, (gx, gz) in places.items():
        ax.plot(gx, gz, "r*")
        ax.text(gx, gz, " " + name, fontsize=8)
    for name, (gx, gz) in marks.items():
        ax.plot(gx, gz, "m.", markersize=4)
        ax.text(gx, gz, " " + name, fontsize=5, color="m")
    from matplotlib.patches import Ellipse
    for name, (gx, gz, rx, rz) in woods.items():
        ax.add_patch(Ellipse((gx, gz), 2 * rx, 2 * rz, fill=False, edgecolor="g", linewidth=0.8))
        ax.text(gx, gz, name, fontsize=6, color="g", ha="center")
    ax.set_title("The southern North Sea c. 793, shortened 1:%d (game metres)" % SCALE)
    fig.savefig(os.path.join(OUT, "preview.png"), dpi=90)


if __name__ == "__main__":
    main()
