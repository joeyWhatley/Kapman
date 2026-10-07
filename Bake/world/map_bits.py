# -*- coding: utf-8 -*-
r"""
THE CHART IN 1, 2, 3 AND 4 BITS, built round Byzantium (#702963). The user, 7 Oct 2026: "different colour palettes made
around the Byzantium colour, 1bit, 2bit, 3bit, 4Bit ... also do the same for the world map no hud".

Each pixel of the chart's picture is first read as a kind of ground (the same eight kinds as the game's palettes:
deep water, sea, shallows, shore, marsh, cleared land, woodland, high ground: Bake/world/recolour_map.py), then drawn:
  1 bit   2 colours: Byzantium and paper; the kinds are told apart by how thickly they are dotted
  2 bit   4 colours: a dark plum, Byzantium, a light mauve, paper; dotted between
  3 bit   8 colours: the Tide palette of the game, one colour a kind of ground
  4 bit  16 colours: Tide with a lighter and a darker shade of each kind, by the lie of the land in the picture

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" map_bits.py <picture.png> <out folder> [width in pixels, default 1280]
Writes <name>_1bit.png ... <name>_4bit.png and <name>_bits.png (the four side by side).
"""
import colorsys, os, sys
import numpy as np
from PIL import Image
Image.MAX_IMAGE_PIXELS = None
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from recolour_map import role, hx, PALETTES

BYZ, PAPER, DARK, MAUVE = "#702963", "#f1e8d6", "#2e1233", "#c77fb0"
TIDE = [p for p in PALETTES if p[0] == "frisian"][0][3].split()
#        deep  sea   shallows shore marsh cleared wood  high      how light each kind is drawn where colours are few
LIGHT1 = [0.00, 0.10, 0.38, 0.97, 0.70, 0.90, 0.55, 1.00]
LIGHT2 = [0.04, 0.33, 0.52, 0.94, 0.68, 0.86, 0.50, 1.00]
BAYER = (np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) + 0.5) / 16.0


def shade(h, k):
    r, g, b = [v / 255.0 for v in hx(h)]
    hh, l, s = colorsys.rgb_to_hls(r, g, b)
    return tuple(int(round(v * 255)) for v in colorsys.hls_to_rgb(hh, max(0.0, min(1.0, l * k)), s))


def main(src, dst, width):
    os.makedirs(dst, exist_ok=True)
    im = Image.open(src).convert("RGB")
    w = int(width)
    h = int(round(im.size[1] * w / im.size[0]))
    im = im.resize((w, h), Image.BOX)
    pal = im.convert("P", palette=Image.ADAPTIVE, colors=64)
    idx = np.array(pal)
    p = np.array(pal.getpalette()[:768], dtype=np.float64).reshape(-1, 3) / 255.0
    roles = np.array([role(*c) for c in p])[idx]
    light = np.array([colorsys.rgb_to_hls(*c)[1] for c in p])[idx]
    dots = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    out = {}
    # 1 bit
    v = np.array(LIGHT1)[roles]
    two = np.array([hx(BYZ), hx(PAPER)], dtype=np.uint8)
    out["1bit"] = two[(v > dots).astype(int)]
    # 2 bit
    v = np.array(LIGHT2)[roles] * 3.0
    lo = np.floor(v).astype(int)
    t = np.clip(lo + ((v - lo) > dots).astype(int), 0, 3)
    four = np.array([hx(DARK), hx(BYZ), hx(MAUVE), hx(PAPER)], dtype=np.uint8)
    out["2bit"] = four[t]
    # 3 bit
    eight = np.array([hx(c) for c in TIDE], dtype=np.uint8)
    out["3bit"] = eight[roles]
    # 4 bit: each kind in two shades, the lighter where the picture itself is lighter than that kind's middle
    sixteen = np.zeros((16, 3), dtype=np.uint8)
    for i, c in enumerate(TIDE):
        sixteen[i * 2] = shade(c, 0.86) if i != 1 else shade(c, 0.88)
        sixteen[i * 2 + 1] = shade(c, 1.10) if i != 1 else hx(c)          # Byzantium itself stays in the picture
    mid = np.zeros(8)
    for i in range(8):
        m = roles == i
        mid[i] = light[m].mean() if m.any() else 0.5
    up = ((light - mid[roles]) * 6.0 + 0.5 > dots).astype(int)
    out["4bit"] = sixteen[roles * 2 + up]
    base = os.path.splitext(os.path.basename(src))[0]
    name = os.path.basename(os.path.dirname(os.path.abspath(src))) + "_" + base
    sheet = Image.new("RGB", (w * 2 + 12, h * 2 + 12), hx("#150f1c"))
    for k, (bits, arr) in enumerate(out.items()):
        pic = Image.fromarray(arr, "RGB")
        pic.save(os.path.join(dst, "%s_%s.png" % (name, bits)), optimize=True)
        sheet.paste(pic, ((k % 2) * (w + 12), (k // 2) * (h + 12)))
        print(bits, len(set(map(tuple, arr.reshape(-1, 3)[::97]))), "colours seen")
    sheet.save(os.path.join(dst, "%s_bits.png" % name), optimize=True)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else 1280)
