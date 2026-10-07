"""A REAL PICTURE AS A STAND-IN, THROUGH THE GAME'S OWN FILTER.

The user, 7 Oct 2026: "I want you to use this as placeholder for Dorestad's view, and use any wiki images of ports and
places of that period instead of AI drawn stuff"; "just apply a Byzantium / PSX filter to it".

A picture is cut to the slot's shape, made small (an old console's picture), its light and dark stretched, and turned
into the tones of Byzantium, eight of them from night to cream, in dots where one tone meets the next (an ordered
dither, as the title's sea has). It is written to Art/standin/<kind>/<slot>.png, which the game shows before the drawn
placeholder and after a drawing of the user's own (FrameArt.Load). Art/standin is not in the repository and nothing in
it ships without its licence being cleared: each file's source and licence is listed in Design/art_standins.md.

Use:  python standin_filter.py <picture> <kind> <slot id> [--lines 180] [--cut 0.5,0.5]
      e.g. python standin_filter.py dorestad.jpg port_view portview.dorestad
"""
import os, sys
from PIL import Image, ImageOps

HERE = os.path.dirname(os.path.abspath(__file__))
ART = os.path.normpath(os.path.join(HERE, "..", "..", "Art"))
SIZES = {"port_view": (640, 360), "scene": (320, 180), "harbour": (320, 180), "card": (320, 180)}
BAYER4 = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5]
# night, dark purple, plum, byzantium, and four lighter steps to cream (the first four are the palette's own)
RAMP = [(22, 10, 28), (46, 18, 51), (74, 28, 74), (112, 41, 99), (150, 76, 136), (190, 124, 170), (226, 184, 198), (246, 232, 214)]


def tone(im):
    """A picture, at its own size, in the eight tones of Byzantium with dots where one meets the next."""
    grey = ImageOps.autocontrast(ImageOps.grayscale(im), cutoff=1)
    out = Image.new("RGB", im.size)
    gp, op = grey.load(), out.load()
    n = len(RAMP) - 1
    for y in range(im.size[1]):
        for x in range(im.size[0]):
            v = (gp[x, y] / 255.0) ** 0.92 * n
            i = min(n - 1, int(v))
            t = (BAYER4[(y & 3) * 4 + (x & 3)] + 0.5) / 16.0
            op[x, y] = RAMP[i + 1] if v - i > t else RAMP[i]
    return out


def filtered(src, size, lines=180, cut=(0.5, 0.5)):
    w, h = size
    im = Image.open(src).convert("RGB")
    im = ImageOps.fit(im, (w, h), Image.LANCZOS, centering=cut)
    small = (max(1, int(round(lines * w / float(h)))), lines)
    return tone(im.resize(small, Image.LANCZOS)).resize((w, h), Image.NEAREST)


def main(argv):
    if len(argv) < 4:
        print(__doc__)
        return 2
    src, kind, slot = argv[1], argv[2], argv[3]
    lines, cut = 180, (0.5, 0.5)
    for i, a in enumerate(argv):
        if a == "--lines":
            lines = int(argv[i + 1])
        if a == "--cut":
            cut = tuple(float(v) for v in argv[i + 1].split(","))
    size = SIZES.get(kind, (640, 360))
    out = os.path.join(ART, "standin", kind)
    os.makedirs(out, exist_ok=True)
    path = os.path.join(out, slot + ".png")
    filtered(src, size, lines, cut).save(path)
    print(path, size)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
