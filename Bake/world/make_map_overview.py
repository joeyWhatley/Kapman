# -*- coding: utf-8 -*-
r"""
MAKE MAP OVERVIEW - after redrawing any of the chart's tiles (Kapman2/MapArt/world/t_<row>_<col>.png), run this to
rebuild overview.png from them (the small picture the chart shows from far out and under tiles still loading).
  run.bat world\make_map_overview.py
Tiles that are missing are left as deep sea. Nothing else is touched: your tiles are only read.
"""
import io, json, os
from PIL import Image

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
ART = os.path.join(ROOT, "MapArt", "world")


def main():
    with io.open(os.path.join(ART, "map_art.json"), "r", encoding="utf-8") as f:
        meta = json.load(f)
    cols, rows, px, over_px = int(meta["cols"]), int(meta["rows"]), int(meta["tilePixels"]), int(meta.get("overviewPixels", 4096))
    over = Image.new("RGB", (over_px, over_px), (27, 35, 64))
    cell_w, cell_h = over_px / float(cols), over_px / float(rows)
    done = 0
    for r in range(rows):
        for c in range(cols):
            path = os.path.join(ART, meta["tiles"].replace("{row}", str(r)).replace("{col}", str(c)))
            if not os.path.exists(path):
                continue
            tile = Image.open(path).convert("RGB")
            if tile.size != (px, px):
                print("warning: %s is %dx%d, not %dx%d: the chart stretches it to fit" % (os.path.basename(path), tile.size[0], tile.size[1], px, px))
            x0, y0 = int(round(c * cell_w)), int(round(r * cell_h))
            x1, y1 = int(round((c + 1) * cell_w)), int(round((r + 1) * cell_h))
            over.paste(tile.resize((x1 - x0, y1 - y0), Image.NEAREST), (x0, y0))
            done += 1
    over.save(os.path.join(ART, meta.get("overview", "overview.png")), optimize=True)
    print("overview.png rebuilt from %d of %d tiles" % (done, cols * rows))


if __name__ == "__main__":
    main()
