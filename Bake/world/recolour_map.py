# -*- coding: utf-8 -*-
"""Recolours the chart picture into the eight 3-bit palettes of the game (Assets/Kapman/Code/Game/Map/MapPalette.cs):
eight colours, one for each kind of ground, every palette holding Byzantium (#702963). One palette a people.
  python-qgis-ltr.bat recolour_map.py <picture.png> <out folder>            -> one PNG per palette
"""
import colorsys, os, sys
import numpy as np
from PIL import Image
Image.MAX_IMAGE_PIXELS = None
PALETTES = [('frisian', 'Tide', 'dorestad', '#37113c #702963 #a8619b #e6c9a8 #7d8442 #b3b56a #4a5a2c #faf1df'), ('frankish', 'Vineyard', 'quentovic', '#2b1233 #702963 #9466ab #f0d27a #9a4a5a #cf9a6a #481224 #f6efe0'), ('english', 'Minster', 'lundenwic', '#2e1233 #702963 #a070a6 #efe5c2 #5f8a5c #9fbf7a #297036 #dfe8e0'), ('danish', 'Amber', 'ripa', '#14323a #38717e #73a7ad #f7dda8 #a07c2c #d9a441 #702963 #f6efe0'), ('norse', 'Fjord', 'ogvaldsnes', '#171a38 #585896 #9490c2 #cbbccb #702963 #5fa08a #204a44 #f4f1fa'), ('svea', 'Silver', 'birka', '#241a2e #463c43 #8f82a3 #cbcde2 #702963 #a9b08a #4f5a4a #faf8fc'), ('wendish', 'Grove', 'reric', '#2e1233 #702963 #b06aa0 #dccba0 #637029 #94a23c #414c1a #f8f2d9'), ('irish', 'Vellum', 'hi', '#202638 #2c678f #82b3ba #f3e7c4 #ce4f48 #d8c27a #4c6e1f #702963')]
def hx(h): return tuple(int(h[i:i + 2], 16) for i in (1, 3, 5))
def role(r, g, b):
    h, l, s = colorsys.rgb_to_hls(r, g, b)
    if b > r and b >= g * 0.95 and s > 0.08: return 0 if l < 0.22 else 1 if l < 0.32 else 2
    if s < 0.12 or (l > 0.80 and s < 0.35): return 7
    if g >= r and g > b: return 6 if l < 0.30 else 4 if l < 0.40 else 5
    return 3 if l > 0.55 else 4
def recolour(im, colours):
    pal = im.convert("P", palette=Image.ADAPTIVE, colors=64) if im.mode != "P" else im
    p = np.array(pal.getpalette()[:768], dtype=np.float64).reshape(-1, 3) / 255.0
    out = []
    for r, g, b in p:
        out.extend(hx(colours[role(r, g, b)]))
    res = pal.copy(); res.putpalette(out + [0] * (768 - len(out))); return res
if __name__ == "__main__":
    src, dst = sys.argv[1], sys.argv[2]
    os.makedirs(dst, exist_ok=True)
    im = Image.open(src)
    base = os.path.splitext(os.path.basename(src))[0]
    for people, name, home, colours in PALETTES:
        recolour(im, colours.split()).save(os.path.join(dst, "%s_%s_%s.png" % (base, people, name.lower())), optimize=True)
        print(people, name, flush=True)
