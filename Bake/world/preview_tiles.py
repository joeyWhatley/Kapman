# A PNG OVERVIEW OF A WORLD BAKE (Bake/world/make_tiles.py), for checking by eye: water blue (deep, shallow, river in
# three blues), the waterfront band green, inland grey; empty tiles drawn from their fill; faint tile lines; the river
# centre lines and the ports of c. 793 marked if asked.
# Run: Bake\run.bat world\preview_tiles.py [--scale 40] [--px 8] [--crop GX0 GY0 GX1 GY1] [--rivers] [--ports]
#   -> G:\kapman_unity\data\world\<scale>\preview.png (or preview_<crop>.png)
import argparse
import json
import math
import os

import numpy as np
from osgeo import osr
from PIL import Image, ImageDraw

osr.UseExceptions()

OUT_ROOT = r"G:\kapman_unity\data\world"
PORTS = r"G:\kapman_unity\Kapman2\Assets\Kapman\Game\Data\ports_793.json"
COLOURS = np.array([(28, 62, 120),     # 0 deep sea
                    (66, 128, 196),    # 1 shallow sea
                    (40, 170, 235),    # 2 river
                    (96, 160, 72),     # 3 band
                    (150, 150, 150)],  # 4 inland
                   dtype=np.uint8)
OUTSIDE = (40, 40, 40)


def main():
    ap = argparse.ArgumentParser(description="PNG overview of a world bake")
    ap.add_argument("--scale", default="40", help="the bake's folder under data\\world (default 40)")
    ap.add_argument("--px", type=float, default=None, help="game metres per pixel (default: fit about 3000 px wide)")
    ap.add_argument("--crop", type=float, nargs=4, default=None, metavar=("GX0", "GY0", "GX1", "GY1"),
                    help="only this game-metre rectangle")
    ap.add_argument("--pool", choices=("centre", "min"), default="centre",
                    help="centre = sample each pixel's centre cell; min = water wins (shows thin channels)")
    ap.add_argument("--rivers", action="store_true", help="draw the river centre lines from rivers.json")
    ap.add_argument("--ports", action="store_true", help="mark the ports of ports_793.json")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    base = os.path.join(OUT_ROOT, args.scale)
    idx = json.load(open(os.path.join(base, "index.json"), encoding="utf-8"))
    T, cell, nc = idx["tileSize"], idx["cell"], idx["cellsPerSide"]
    gx0, gy0, gx1, gy1 = args.crop or idx["gameBounds"]
    px = args.px or max(cell, math.ceil((gx1 - gx0) / 3000.0 / cell) * cell)
    step = max(1, int(round(px / cell)))
    px = step * cell
    W, H = int(math.ceil((gx1 - gx0) / px)), int(math.ceil((gy1 - gy0) / px))
    img = np.empty((H, W, 3), np.uint8)
    img[:] = OUTSIDE
    tiles = {(t["x"], t["y"]): t for t in idx["tiles"]}
    n_read = 0
    for (tx, ty), t in tiles.items():
        x0, y0 = tx * T, ty * T
        if x0 >= gx1 or x0 + T <= gx0 or y0 >= gy1 or y0 + T <= gy0:
            continue
        # The pixel centres that fall in this tile, in tile cells.
        cx = np.arange(W) * px + px / 2 + gx0
        cy = np.arange(H) * px + px / 2 + gy0
        ix = np.nonzero((cx >= x0) & (cx < x0 + T))[0]
        iy = np.nonzero((cy >= y0) & (cy < y0 + T))[0]
        if len(ix) == 0 or len(iy) == 0:
            continue
        if not t["files"]:
            img[np.ix_(H - 1 - iy, ix)] = COLOURS[t["fill"]["kind"]]
            continue
        kind = np.fromfile(os.path.join(base, "tiles", "%d_%d" % (tx, ty), "kind.u8"), np.uint8).reshape(nc, nc)
        n_read += 1
        cols = np.clip(((cx[ix] - x0) / cell).astype(int), 0, nc - 1)
        rows = np.clip(((cy[iy] - y0) / cell).astype(int), 0, nc - 1)
        if args.pool == "min" and step > 1:
            k = kind[: (nc // step) * step, : (nc // step) * step].reshape(nc // step, step, nc // step, step).min(axis=(1, 3))
            sub = k[np.clip(rows // step, 0, k.shape[0] - 1)][:, np.clip(cols // step, 0, k.shape[1] - 1)]
        else:
            sub = kind[rows][:, cols]
        img[np.ix_(H - 1 - iy, ix)] = COLOURS[sub]
    im = Image.fromarray(img)
    dr = ImageDraw.Draw(im)

    def to_px(x, y):
        return (x - gx0) / px, H - (y - gy0) / px

    # Faint tile lines.
    for tx in range(int(math.floor(gx0 / T)), int(math.ceil(gx1 / T)) + 1):
        x, _ = to_px(tx * T, 0)
        dr.line([(x, 0), (x, H)], fill=(0, 0, 0), width=1)
    for ty in range(int(math.floor(gy0 / T)), int(math.ceil(gy1 / T)) + 1):
        _, y = to_px(0, ty * T)
        dr.line([(0, y), (W, y)], fill=(0, 0, 0), width=1)
    if args.rivers and os.path.exists(os.path.join(base, "rivers.json")):
        for r in json.load(open(os.path.join(base, "rivers.json"), encoding="utf-8"))["rivers"]:
            pts = [to_px(x, y) for x, y in r["points"]]
            dr.line(pts, fill=(255, 255, 255), width=1)
            dr.text(pts[len(pts) // 2], r["id"], fill=(255, 255, 255))
    if args.ports and os.path.exists(PORTS):
        wgs = osr.SpatialReference()
        wgs.ImportFromEPSG(4326)
        wgs.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
        dst = osr.SpatialReference()
        dst.ImportFromProj4(idx["projection"])
        ct = osr.CoordinateTransformation(wgs, dst)
        for p in json.load(open(PORTS, encoding="utf-8"))["ports"]:
            x, y, _ = ct.TransformPoint(p["lon"], p["lat"])
            u, v = to_px(x / idx["scale"], y / idx["scale"])
            if 0 <= u < W and 0 <= v < H:
                dr.ellipse((u - 3, v - 3, u + 3, v + 3), outline=(255, 60, 60), width=2)
                dr.text((u + 5, v - 6), p.get("name", p["id"])[:24], fill=(255, 230, 230))
    out = args.out or os.path.join(base, "preview.png" if not args.crop else
                                   "preview_%d_%d_%d_%d.png" % tuple(int(v) for v in args.crop))
    im.save(out)
    print("preview %s: %d x %d px at %g m/px, %d tiles read -> %s" % (args.scale, W, H, px, n_read, out))


if __name__ == "__main__":
    main()
