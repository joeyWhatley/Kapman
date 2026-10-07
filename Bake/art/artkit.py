# -*- coding: utf-8 -*-
"""
ARTKIT - what the placeholder drawers share (make_art_list.py, artdraw.py): the game's 40 colours, a small canvas that
only knows those colours, 3 x 5 letters, a steady random number from a slot's id, and the small 16 x 16 signs
(a coin, a cask, a cross, an oar ...) that say what a thing is about.
"""
import hashlib
import os
import random
import unicodedata

from PIL import Image, ImageDraw

# ------------------------------------------------------------------------------------------------ the 40 colours
_FALLBACK = [
    (11, 8, 16, "night"), (46, 18, 51, "dark purple"), (74, 28, 74, "plum"), (112, 41, 99, "byzantium"),
    (154, 74, 138, "mauve"), (199, 127, 176, "lilac"), (16, 19, 31, "abyss"), (27, 35, 64, "deep sea"),
    (39, 56, 94, "sea"), (60, 90, 128, "shoal"), (95, 138, 166, "shallows"), (158, 196, 204, "wave light"),
    (232, 238, 240, "foam"), (31, 42, 30, "wood shade"), (47, 74, 42, "forest"), (77, 112, 54, "grass"),
    (127, 154, 74, "meadow"), (182, 194, 106, "reed light"), (43, 29, 22, "peat"), (75, 51, 36, "wet earth"),
    (110, 75, 49, "timber"), (155, 110, 70, "dry earth"), (196, 154, 106, "sand"), (227, 203, 154, "pale sand"),
    (38, 33, 43, "basalt"), (74, 67, 80, "slate"), (122, 114, 129, "stone"), (170, 164, 174, "pale stone"),
    (143, 45, 45, "blood"), (201, 85, 58, "rust"), (217, 164, 65, "gold"), (240, 210, 122, "pale gold"),
    (208, 140, 116, "dusk rose"), (232, 180, 138, "dawn peach"), (242, 217, 168, "morning gold"),
    (143, 179, 201, "day sky"), (181, 199, 214, "frost"), (215, 227, 236, "snow"), (232, 226, 230, "cool white"),
    (246, 239, 224, "warm white"),
]

NIGHT, DPURP, PLUM, BYZ, MAUVE, LILAC = "night", "dark purple", "plum", "byzantium", "mauve", "lilac"
ABYSS, DEEP, SEA, SHOAL, SHALL, WAVE, FOAM = "abyss", "deep sea", "sea", "shoal", "shallows", "wave light", "foam"
WSHADE, FOREST, GRASS, MEADOW, REED = "wood shade", "forest", "grass", "meadow", "reed light"
PEAT, WEARTH, TIMBER, DEARTH, SAND, PSAND = "peat", "wet earth", "timber", "dry earth", "sand", "pale sand"
BASALT, SLATE, STONE, PSTONE = "basalt", "slate", "stone", "pale stone"
BLOOD, RUST, GOLD, PGOLD = "blood", "rust", "gold", "pale gold"
DROSE, PEACH, MGOLD, SKY = "dusk rose", "dawn peach", "morning gold", "day sky"
FROST, SNOW, CWHITE, WWHITE = "frost", "snow", "cool white", "warm white"

RGB = {}
ORDER = []


def load_palette(path=None):
    """Reads the GIMP palette; falls back on the 40 colours written above if the file is not there."""
    rows = []
    if path and os.path.exists(path):
        try:
            with open(path, "r", encoding="utf-8") as f:
                for line in f:
                    p = line.strip().split(None, 3)
                    if len(p) == 4 and p[0].isdigit() and p[1].isdigit() and p[2].isdigit():
                        rows.append((int(p[0]), int(p[1]), int(p[2]), p[3].strip()))
        except Exception:
            rows = []
    have = {r[3] for r in rows}
    if not rows or any(n not in have for (_, _, _, n) in _FALLBACK):
        rows = list(_FALLBACK)          # a palette with other names would break every drawer: use the 40 we know
    RGB.clear()
    del ORDER[:]
    for r, g, b, n in rows:
        RGB[n] = (r, g, b, 255)
        ORDER.append(n)
    return RGB


load_palette(None)


def col(c):
    if c is None:
        return (0, 0, 0, 0)
    return RGB[c]


# ------------------------------------------------------------------------------------------------ steady chance
def seed_of(s):
    return int(hashlib.md5(str(s).encode("utf-8")).hexdigest()[:12], 16)


def rng_of(s):
    return random.Random(seed_of(s))


def ascii_up(s):
    """Upper-case plain letters for the 3 x 5 letters: Æthelred -> AETHELRED."""
    s = (s or "")
    for a, b in (("Æ", "AE"), ("æ", "ae"), ("Þ", "Th"), ("þ", "th"), ("Ð", "D"), ("ð", "d"), ("Ø", "O"), ("ø", "o"),
                 ("ß", "ss"), ("Œ", "OE"), ("œ", "oe"), ("’", "'"), ("‘", "'"), ("–", "-"), ("—", "-")):
        s = s.replace(a, b)
    s = unicodedata.normalize("NFKD", s)
    s = "".join(ch for ch in s if not unicodedata.combining(ch))
    return "".join(ch if ch.upper() in FONT else " " for ch in s).upper()


# ------------------------------------------------------------------------------------------------ 3 x 5 letters
FONT = {
    "A": "010101111101101", "B": "110101110101110", "C": "011100100100011", "D": "110101101101110",
    "E": "111100110100111", "F": "111100110100100", "G": "011100101101011", "H": "101101111101101",
    "I": "111010010010111", "J": "001001001101010", "K": "101101110101101", "L": "100100100100111",
    "M": "101111111101101", "N": "110101101101101", "O": "010101101101010", "P": "110101110100100",
    "Q": "010101101110011", "R": "110101110101101", "S": "011100010001110", "T": "111010010010010",
    "U": "101101101101111", "V": "101101101101010", "W": "101101111111101", "X": "101101010101101",
    "Y": "101101010010010", "Z": "111001010100111",
    "0": "111101101101111", "1": "010110010010111", "2": "110001010100111", "3": "110001010001110",
    "4": "101101111001001", "5": "111100110001110", "6": "011100111101111", "7": "111001010010010",
    "8": "111101111101111", "9": "111101111001110",
    ".": "000000000000010", ",": "000000000010100", "-": "000000111000000", "_": "000000000000111",
    "'": "010010000000000", ":": "000010000010000", "/": "001001010100100", "?": "110001010000010",
    "!": "010010010000010", "(": "001010010010001", ")": "100010010010100", "+": "000010111010000",
    " ": "000000000000000",
}


class Cv(object):
    """A picture that only takes the game's colours by name. Coordinates are whole pixels, ends included."""

    def __init__(self, w, h, bg=None):
        self.w, self.h = int(w), int(h)
        self.im = Image.new("RGBA", (self.w, self.h), col(bg))
        self.d = ImageDraw.Draw(self.im)

    # shapes ------------------------------------------------------------------------------------
    def rect(self, x0, y0, x1, y1, c):
        x0, x1 = (x0, x1) if x0 <= x1 else (x1, x0)
        y0, y1 = (y0, y1) if y0 <= y1 else (y1, y0)
        self.d.rectangle([int(x0), int(y0), int(x1), int(y1)], fill=col(c))

    def box(self, x0, y0, x1, y1, c, w=1):
        for i in range(w):
            self.d.rectangle([int(x0) + i, int(y0) + i, int(x1) - i, int(y1) - i], outline=col(c))

    def px(self, x, y, c):
        x, y = int(x), int(y)
        if 0 <= x < self.w and 0 <= y < self.h:
            self.im.putpixel((x, y), col(c))

    def line(self, x0, y0, x1, y1, c, w=1):
        self.d.line([int(x0), int(y0), int(x1), int(y1)], fill=col(c), width=w)

    def lines(self, pts, c, w=1):
        self.d.line([(int(x), int(y)) for x, y in pts], fill=col(c), width=w)

    def poly(self, pts, c, outline=None):
        self.d.polygon([(int(round(x)), int(round(y))) for x, y in pts], fill=col(c) if c else None,
                       outline=col(outline) if outline else None)

    def ell(self, x0, y0, x1, y1, c, outline=None):
        self.d.ellipse([int(x0), int(y0), int(x1), int(y1)], fill=col(c) if c else None,
                       outline=col(outline) if outline else None)

    def arc(self, x0, y0, x1, y1, a0, a1, c, w=1):
        self.d.arc([int(x0), int(y0), int(x1), int(y1)], a0, a1, fill=col(c), width=w)

    def dither(self, x0, y0, x1, y1, c, phase=0, step=2):
        """Every other pixel in colour c: a half tone between c and what lies under it."""
        x0, y0, x1, y1 = int(x0), int(y0), int(x1), int(y1)
        k = col(c)
        put = self.im.putpixel
        for y in range(max(0, y0), min(self.h - 1, y1) + 1):
            for x in range(max(0, x0), min(self.w - 1, x1) + 1):
                if (x + y + phase) % step == 0:
                    put((x, y), k)

    def clear(self, x0, y0, x1, y1):
        self.d.rectangle([int(x0), int(y0), int(x1), int(y1)], fill=(0, 0, 0, 0))

    # letters -----------------------------------------------------------------------------------
    @staticmethod
    def text_w(s, scale=1):
        return max(0, len(s) * 4 * scale - scale)

    def text(self, x, y, s, c, scale=1, shadow=None):
        s = ascii_up(s)
        if shadow:
            self.text(x + scale, y + scale, s, shadow, scale)
        k = col(c)
        cx = int(x)
        for ch in s:
            bits = FONT.get(ch, FONT[" "])
            for i, b in enumerate(bits):
                if b == "1":
                    px, py = cx + (i % 3) * scale, int(y) + (i // 3) * scale
                    if scale == 1:
                        if 0 <= px < self.w and 0 <= py < self.h:
                            self.im.putpixel((px, py), k)
                    else:
                        self.d.rectangle([px, py, px + scale - 1, py + scale - 1], fill=k)
            cx += 4 * scale
        return cx

    def text_fit(self, x, y, s, c, width, scale=1, shadow=None):
        """Writes as much of s as fits in width pixels."""
        s = ascii_up(s)
        n = max(0, (int(width) + scale) // (4 * scale))
        if len(s) > n:
            s = s[:max(0, n - 1)].rstrip() + ("." if n > 1 else "")
        return self.text(x, y, s, c, scale, shadow)

    def text_mid(self, cx, y, s, c, scale=1, shadow=None):
        s = ascii_up(s)
        return self.text(int(cx - self.text_w(s, scale) / 2), y, s, c, scale, shadow)

    # putting pictures together ----------------------------------------------------------------------
    def blit(self, other, x, y):
        """Lays another picture over this one; what falls outside is cut off."""
        im = other.im if isinstance(other, Cv) else other
        x, y = int(x), int(y)
        x0, y0 = max(0, x), max(0, y)
        x1, y1 = min(self.w, x + im.size[0]), min(self.h, y + im.size[1])
        if x1 <= x0 or y1 <= y0:
            return
        part = im.crop((x0 - x, y0 - y, x1 - x, y1 - y))
        self.im.alpha_composite(part, (x0, y0))

    def scaled(self, k):
        out = Cv(self.w * k, self.h * k)
        out.im = self.im.resize((self.w * k, self.h * k), Image.NEAREST)
        out.d = ImageDraw.Draw(out.im)
        return out

    def flipped(self):
        out = Cv(self.w, self.h)
        out.im = self.im.transpose(Image.FLIP_LEFT_RIGHT)
        out.d = ImageDraw.Draw(out.im)
        return out

    def outline_all(self, c):
        """A one pixel edge in colour c round everything drawn so far (so a sign reads on any ground)."""
        src = self.im.copy()
        a = src.split()[3]
        k = col(c)
        w, h = self.w, self.h
        ap = a.load()
        put = self.im.putpixel
        for y in range(h):
            for x in range(w):
                if ap[x, y] == 0:
                    if ((x > 0 and ap[x - 1, y]) or (x < w - 1 and ap[x + 1, y]) or
                            (y > 0 and ap[x, y - 1]) or (y < h - 1 and ap[x, y + 1])):
                        put((x, y), k)
        return self


def off_palette(im):
    """How many solid pixels are not one of the game's colours (should be none)."""
    allowed = set(v[:3] for v in RGB.values())
    bad = 0
    for count, (r, g, b, a) in (im.getcolors(maxcolors=1 << 24) or []):
        if a == 0:
            continue
        if a != 255 or (r, g, b) not in allowed:
            bad += count
    return bad


def placeholder_mark(cv):
    """The sign that a picture is a placeholder: a purple and gold corner, top right."""
    w, h = cv.w, cv.h
    if w < 12 or h < 12:
        cv.px(w - 1, 0, PGOLD)
        return
    if w < 24 or h < 24:
        cv.px(w - 1, 0, PGOLD); cv.px(w - 2, 0, BYZ); cv.px(w - 1, 1, BYZ)
        return
    if w < 48 or h < 40:
        for i in range(5):
            cv.rect(w - 5 + i, 0, w - 1, i, BYZ)
        cv.px(w - 1, 0, PGOLD); cv.px(w - 2, 0, PGOLD); cv.px(w - 1, 1, PGOLD)
        return
    cv.rect(w - 7, 0, w - 1, 6, BYZ)
    cv.box(w - 7, 0, w - 1, 6, NIGHT)
    cv.text(w - 5, 1, "P", PGOLD)


def label_strip(cv, text, h=7):
    """A dark band along the bottom with the slot's name in small letters."""
    y = cv.h - h
    cv.rect(0, y, cv.w - 1, cv.h - 1, NIGHT)
    cv.text_fit(2, y + 1, text, PGOLD, cv.w - 4)


# ------------------------------------------------------------------------------------------------ the 16 x 16 signs
def _coin(c):
    c.ell(3, 3, 12, 12, GOLD, PEAT); c.line(5, 5, 7, 4, PGOLD); c.line(8, 6, 8, 10, DEARTH); c.line(6, 8, 10, 8, DEARTH)


def _coins(c):
    c.ell(1, 7, 8, 14, GOLD, PEAT); c.ell(7, 7, 14, 14, PSTONE, PEAT); c.ell(4, 2, 11, 9, GOLD, PEAT)
    c.line(6, 4, 8, 3, PGOLD)


def _scales(c):
    c.line(2, 4, 13, 4, PEAT); c.line(8, 2, 8, 13, PEAT); c.line(5, 14, 11, 14, PEAT); c.px(8, 1, GOLD)
    c.line(2, 4, 0, 9, SLATE); c.line(2, 4, 4, 9, SLATE); c.line(13, 4, 11, 10, SLATE); c.line(13, 4, 15, 10, SLATE)
    c.rect(0, 9, 4, 10, GOLD); c.rect(11, 10, 15, 11, GOLD)


def _tally(c):
    c.poly([(1, 11), (12, 3), (14, 5), (3, 13)], PSAND, PEAT)
    for x, y in ((5, 9), (7, 7), (9, 6), (11, 4)):
        c.line(x, y, x + 1, y + 2, PEAT)


def _purse(c):
    c.ell(3, 6, 12, 14, TIMBER, PEAT); c.rect(6, 3, 9, 6, TIMBER); c.line(5, 6, 10, 6, PGOLD); c.px(8, 10, GOLD)
    c.line(6, 2, 9, 2, PEAT)


def _cask(c):
    c.rect(4, 2, 11, 13, TIMBER); c.rect(3, 5, 12, 10, TIMBER); c.line(4, 2, 11, 2, PEAT); c.line(4, 13, 11, 13, PEAT)
    c.line(3, 5, 12, 5, PEAT); c.line(3, 10, 12, 10, PEAT); c.line(6, 3, 6, 12, DEARTH); c.line(9, 3, 9, 12, WEARTH)
    c.line(2, 6, 2, 9, PEAT); c.line(13, 6, 13, 9, PEAT)


def _sack(c):
    c.ell(2, 5, 13, 14, SAND, PEAT); c.rect(6, 2, 9, 5, SAND); c.line(5, 5, 10, 5, PEAT); c.line(5, 2, 10, 2, PEAT)
    c.line(5, 9, 6, 12, DEARTH); c.line(10, 8, 11, 11, PSAND)


def _crate(c):
    c.rect(2, 5, 13, 13, DEARTH); c.box(2, 5, 13, 13, PEAT); c.line(2, 9, 13, 9, TIMBER); c.line(6, 5, 6, 13, TIMBER)
    c.line(10, 5, 10, 13, TIMBER); c.rect(4, 3, 6, 5, PSAND); c.rect(9, 2, 11, 5, DROSE)


def _bolt(c):
    c.rect(3, 5, 13, 11, MAUVE); c.line(3, 5, 13, 5, PEAT); c.line(3, 11, 13, 11, PEAT); c.ell(0, 4, 5, 12, LILAC, PEAT)
    c.px(2, 8, BYZ); c.px(3, 7, BYZ); c.line(13, 5, 13, 11, PEAT); c.line(7, 7, 11, 7, BYZ); c.line(7, 9, 11, 9, BYZ)


def _pot(c):
    c.ell(3, 5, 12, 14, DROSE, PEAT); c.rect(6, 2, 9, 5, DROSE); c.line(5, 2, 10, 2, PEAT); c.line(5, 7, 10, 7, RUST)
    c.px(5, 9, PEACH)


def _sword(c):
    c.rect(7, 1, 8, 10, PSTONE); c.line(7, 1, 7, 10, CWHITE); c.rect(4, 10, 11, 11, GOLD); c.rect(7, 12, 8, 13, TIMBER)
    c.rect(6, 14, 9, 15, GOLD); c.px(7, 0, PSTONE)


def _spear(c):
    c.line(3, 14, 11, 5, TIMBER); c.poly([(10, 6), (14, 1), (12, 7)], PSTONE, SLATE)


def _axe(c):
    c.line(5, 14, 9, 2, TIMBER, 2); c.poly([(9, 2), (14, 2), (14, 8), (10, 6)], PSTONE, SLATE)


def _shield(c):
    c.ell(1, 1, 14, 14, TIMBER, PEAT); c.line(5, 2, 5, 13, DEARTH); c.line(10, 2, 10, 13, DEARTH)
    c.ell(5, 5, 10, 10, PSTONE, SLATE)


def _hammer(c):
    c.rect(7, 6, 8, 14, TIMBER); c.rect(3, 2, 12, 6, SLATE); c.line(3, 2, 12, 2, PSTONE)


def _anvil(c):
    c.rect(3, 5, 12, 8, SLATE); c.poly([(12, 5), (15, 6), (12, 8)], SLATE); c.rect(6, 9, 9, 11, SLATE)
    c.rect(4, 12, 11, 13, SLATE); c.line(3, 5, 12, 5, PSTONE); c.px(8, 3, GOLD); c.px(6, 2, RUST); c.px(10, 2, PGOLD)


def _cross(c):
    c.rect(7, 1, 8, 14, PGOLD); c.rect(3, 5, 12, 6, PGOLD); c.line(9, 1, 9, 14, GOLD); c.line(3, 7, 12, 7, GOLD)


def _crosier(c):
    c.rect(8, 6, 8, 15, TIMBER); c.arc(4, 1, 11, 8, 150, 90, GOLD, 2); c.px(8, 6, GOLD)


def _book(c):
    c.rect(2, 3, 13, 13, BLOOD); c.box(2, 3, 13, 13, PEAT); c.line(4, 3, 4, 13, PEAT)
    c.line(8, 5, 8, 11, GOLD); c.line(6, 7, 10, 7, GOLD); c.px(13, 8, GOLD); c.line(3, 14, 13, 14, PSAND)


def _leaf(c):
    c.rect(3, 1, 12, 14, PSAND); c.box(3, 1, 12, 14, DEARTH); c.rect(5, 3, 6, 5, BLOOD)
    for y in (4, 6, 8, 10, 12):
        c.line(8 if y < 6 else 5, y, 10, y, TIMBER)


def _quill(c):
    c.line(3, 13, 12, 3, CWHITE, 2); c.line(6, 8, 9, 9, PSTONE); c.line(8, 6, 11, 7, PSTONE); c.px(2, 14, PEAT)
    c.px(3, 13, PEAT); c.line(11, 2, 13, 1, CWHITE)


def _key(c):
    c.ell(1, 1, 8, 8, None, GOLD); c.ell(2, 2, 7, 7, None, GOLD); c.line(7, 7, 13, 13, GOLD, 2)
    c.line(11, 13, 11, 15, GOLD); c.line(13, 11, 15, 11, GOLD)


def _bell(c):
    c.poly([(6, 3), (9, 3), (13, 12), (2, 12)], GOLD, PEAT); c.rect(7, 1, 8, 2, PEAT); c.rect(7, 13, 8, 14, PEAT)
    c.line(6, 5, 5, 10, PGOLD)


def _candle(c):
    c.rect(7, 6, 8, 13, WWHITE); c.rect(5, 14, 10, 14, TIMBER); c.px(7, 5, PEAT); c.rect(7, 2, 8, 4, GOLD)
    c.px(7, 1, PGOLD); c.px(8, 4, RUST)


def _casket(c):
    c.rect(2, 8, 13, 13, GOLD); c.poly([(2, 8), (4, 4), (11, 4), (13, 8)], PGOLD, PEAT); c.box(2, 8, 13, 13, PEAT)
    c.px(5, 10, BLOOD); c.px(8, 10, BYZ); c.px(11, 10, BLOOD); c.rect(7, 2, 8, 3, GOLD)


def _thor(c):
    c.rect(7, 3, 8, 9, PSTONE); c.rect(3, 9, 12, 13, PSTONE); c.box(3, 9, 12, 13, SLATE); c.ell(6, 0, 9, 3, None, SLATE)
    c.px(5, 11, SLATE); c.px(10, 11, SLATE)


def _ring(c):
    c.ell(2, 3, 13, 12, None, GOLD); c.ell(3, 4, 12, 11, None, GOLD); c.rect(7, 11, 8, 13, None)
    c.px(6, 12, PGOLD); c.px(9, 12, PGOLD); c.line(4, 5, 6, 4, PGOLD)


def _post(c):
    c.rect(6, 2, 9, 14, TIMBER); c.line(6, 2, 6, 14, DEARTH); c.px(7, 5, PEAT); c.px(8, 5, PEAT); c.line(7, 7, 8, 7, PEAT)
    c.rect(5, 1, 10, 2, WEARTH); c.rect(4, 14, 11, 15, WEARTH)


def _tree(c):
    c.rect(7, 10, 8, 14, TIMBER); c.ell(2, 1, 13, 11, FOREST, WSHADE); c.ell(4, 2, 9, 6, GRASS)


def _oar(c):
    c.line(2, 13, 10, 5, TIMBER); c.poly([(9, 6), (11, 2), (14, 1), (13, 4), (11, 7)], DEARTH, PEAT)


def _sail(c):
    c.line(8, 1, 8, 12, TIMBER); c.rect(3, 3, 13, 9, PSAND); c.box(3, 3, 13, 9, DEARTH); c.line(2, 2, 14, 2, TIMBER)
    c.poly([(2, 12), (13, 12), (11, 14), (4, 14)], TIMBER, PEAT)


def _hull(c):
    c.poly([(1, 8), (14, 8), (11, 12), (4, 12)], TIMBER, PEAT); c.line(1, 8, 0, 5, PEAT); c.line(14, 8, 15, 5, PEAT)
    c.line(3, 10, 12, 10, DEARTH)


def _rope(c):
    c.ell(2, 4, 13, 13, None, SAND); c.ell(3, 5, 12, 12, None, DEARTH); c.ell(4, 6, 11, 11, None, SAND)
    c.ell(6, 8, 9, 9, None, DEARTH); c.line(12, 5, 14, 2, SAND)


def _fish(c):
    c.ell(1, 5, 11, 10, SHALL, DEEP); c.poly([(10, 7), (14, 4), (14, 11)], SHALL, DEEP); c.px(3, 7, NIGHT)
    c.line(5, 9, 8, 9, FOAM)


def _net(c):
    c.line(2, 2, 2, 14, TIMBER); c.line(13, 2, 13, 14, TIMBER); c.line(2, 3, 13, 3, TIMBER)
    for i in range(4, 13, 3):
        c.line(i, 4, i - 2, 12, PSTONE); c.line(i, 4, i + 2, 12, PSTONE)


def _loaf(c):
    c.ell(2, 5, 13, 16, DEARTH, PEAT); c.clear(0, 13, 15, 15); c.line(2, 12, 13, 12, PEAT)
    c.line(5, 7, 6, 9, SAND); c.line(8, 6, 9, 9, SAND); c.line(11, 7, 12, 9, SAND)


def _cup(c):
    c.poly([(3, 3), (12, 3), (10, 9), (5, 9)], TIMBER, PEAT); c.rect(7, 10, 8, 12, TIMBER); c.rect(5, 13, 10, 13, PEAT)
    c.line(4, 4, 11, 4, PGOLD)


def _horn(c):
    c.poly([(2, 3), (6, 2), (9, 6), (13, 12), (14, 14), (11, 13), (5, 9)], PSAND, PEAT); c.line(2, 3, 6, 2, GOLD, 2)
    c.px(13, 13, GOLD)


def _dice(c):
    c.rect(2, 7, 7, 12, CWHITE); c.box(2, 7, 7, 12, PEAT); c.px(4, 9, PEAT); c.px(5, 10, PEAT)
    c.rect(8, 3, 13, 8, CWHITE); c.box(8, 3, 13, 8, PEAT); c.px(10, 5, PEAT); c.px(11, 6, PEAT); c.px(10, 6, PEAT)


def _tafl(c):
    c.rect(2, 2, 13, 13, SAND); c.box(2, 2, 13, 13, PEAT)
    for i in (5, 8, 11):
        c.line(i, 3, i, 12, DEARTH); c.line(3, i, 12, i, DEARTH)
    c.rect(7, 7, 8, 8, GOLD); c.px(4, 7, PEAT); c.px(11, 8, PEAT); c.px(7, 4, PEAT); c.px(8, 11, PEAT)
    c.px(6, 7, CWHITE); c.px(9, 8, CWHITE)


def _lyre(c):
    c.rect(4, 2, 11, 13, TIMBER); c.rect(6, 4, 9, 9, None); c.box(4, 2, 11, 13, PEAT); c.line(6, 4, 6, 12, PGOLD)
    c.line(8, 4, 8, 12, PGOLD); c.line(9, 4, 9, 12, PGOLD); c.line(7, 4, 7, 12, PGOLD)


def _spindle(c):
    c.line(8, 1, 8, 14, TIMBER); c.ell(5, 10, 11, 13, STONE, SLATE); c.ell(5, 2, 11, 8, CWHITE, PSTONE)


def _comb(c):
    c.rect(2, 4, 13, 6, WWHITE); c.box(2, 4, 13, 6, DEARTH)
    for x in range(3, 13, 2):
        c.line(x, 7, x, 12, WWHITE)
    c.px(5, 5, RUST); c.px(8, 5, RUST); c.px(11, 5, RUST)


def _sheaf(c):
    for x in (4, 6, 8, 10, 12):
        c.line(8, 14, x, 3, GOLD)
    c.line(6, 9, 10, 9, RUST); c.line(3, 2, 5, 4, PGOLD); c.line(12, 2, 11, 4, PGOLD); c.line(8, 1, 8, 4, PGOLD)


def _crook(c):
    c.rect(8, 5, 8, 15, TIMBER); c.arc(3, 1, 9, 8, 180, 60, TIMBER, 1)


def _staff(c):
    c.line(6, 1, 6, 15, TIMBER); c.ell(7, 4, 13, 10, SAND, PEAT); c.line(6, 4, 9, 4, PEAT)


def _chain(c):
    c.ell(1, 1, 7, 6, None, STONE); c.ell(5, 5, 11, 10, None, SLATE); c.ell(9, 9, 15, 14, None, STONE)


def _bowl(c):
    c.ell(2, 3, 13, 14, TIMBER, PEAT); c.clear(0, 0, 15, 7); c.line(2, 8, 13, 8, PEAT); c.px(7, 6, GOLD)


def _herb(c):
    c.line(8, 14, 8, 4, GRASS); c.ell(3, 5, 7, 8, MEADOW, FOREST); c.ell(9, 3, 13, 6, MEADOW, FOREST)
    c.ell(9, 8, 13, 11, MEADOW, FOREST); c.ell(6, 1, 9, 4, REED, FOREST)


def _circlet(c):
    c.rect(2, 8, 13, 11, GOLD); c.box(2, 8, 13, 11, PEAT); c.rect(3, 6, 4, 7, GOLD); c.rect(7, 5, 8, 7, GOLD)
    c.rect(11, 6, 12, 7, GOLD); c.px(5, 10, BLOOD); c.px(10, 10, BLOOD); c.px(7, 10, BYZ)


def _fire(c):
    c.poly([(8, 1), (12, 7), (11, 12), (5, 12), (4, 7)], RUST); c.poly([(8, 5), (10, 9), (9, 12), (6, 12), (6, 9)], GOLD)
    c.px(8, 10, PGOLD); c.line(3, 13, 12, 14, WEARTH); c.line(4, 14, 13, 13, PEAT)


def _storm(c):
    c.ell(1, 2, 10, 8, SLATE); c.ell(6, 1, 14, 7, STONE); c.rect(3, 6, 12, 8, SLATE)
    c.lines([(8, 8), (6, 11), (9, 11), (7, 15)], PGOLD); c.line(3, 10, 2, 13, SHALL); c.line(12, 10, 11, 13, SHALL)


def _fog(c):
    for i, y in enumerate((3, 6, 9, 12)):
        c.line(1 + (i % 2) * 2, y, 12 + (i % 2) * 2, y, PSTONE, 1); c.line(3, y + 1, 10, y + 1, FROST)


def _snow(c):
    c.line(8, 1, 8, 14, CWHITE); c.line(2, 4, 14, 11, CWHITE); c.line(2, 11, 14, 4, CWHITE); c.px(8, 8, FROST)


def _sun(c):
    c.ell(4, 4, 11, 11, PGOLD, GOLD)
    for x0, y0, x1, y1 in ((7, 0, 8, 2), (7, 13, 8, 15), (0, 7, 2, 8), (13, 7, 15, 8), (2, 2, 3, 3), (12, 2, 13, 3),
                           (2, 12, 3, 13), (12, 12, 13, 13)):
        c.rect(x0, y0, x1, y1, GOLD)


def _moon(c):
    c.ell(3, 2, 13, 12, PGOLD); c.d.ellipse([6, 1, 15, 10], fill=(0, 0, 0, 0))


def _wind(c):
    c.line(1, 4, 10, 4, CWHITE); c.arc(8, 1, 13, 5, 270, 90, CWHITE); c.line(1, 8, 13, 8, FROST)
    c.line(3, 12, 9, 12, CWHITE); c.arc(7, 10, 12, 14, 270, 90, CWHITE)


def _tide(c):
    c.lines([(0, 11), (3, 9), (6, 11), (9, 9), (12, 11), (15, 9)], WAVE); c.lines([(0, 14), (3, 12), (6, 14), (9, 12), (12, 14), (15, 12)], SHALL)
    c.line(8, 1, 8, 7, FOAM); c.line(8, 1, 5, 4, FOAM); c.line(8, 1, 11, 4, FOAM)


def _speech(c):
    c.poly([(1, 4), (11, 2), (14, 4), (12, 9), (3, 11), (1, 9)], PSAND, DEARTH); c.line(4, 6, 10, 5, TIMBER)
    c.line(4, 8, 9, 7, TIMBER); c.poly([(3, 11), (2, 14), (6, 10)], PSAND, DEARTH)


def _eye(c):
    c.poly([(1, 8), (5, 4), (10, 4), (14, 8), (10, 12), (5, 12)], CWHITE, PEAT); c.ell(5, 5, 10, 10, SHOAL, PEAT)
    c.rect(7, 7, 8, 8, NIGHT)


def _heart(c):
    c.ell(2, 3, 8, 9, BLOOD); c.ell(7, 3, 13, 9, BLOOD); c.poly([(2, 7), (13, 7), (8, 13), (7, 13)], BLOOD)
    c.px(4, 5, RUST)


def _bed(c):
    c.rect(1, 9, 14, 11, TIMBER); c.rect(1, 5, 2, 13, TIMBER); c.rect(13, 8, 14, 13, TIMBER); c.rect(3, 7, 12, 9, PSAND)
    c.rect(3, 6, 5, 7, CWHITE)


def _house(c):
    c.rect(3, 8, 12, 14, SAND); c.poly([(1, 8), (8, 2), (14, 8)], DEARTH, PEAT); c.rect(7, 10, 9, 14, PEAT)
    c.line(3, 14, 12, 14, PEAT)


def _church(c):
    c.rect(3, 8, 10, 14, PSTONE); c.rect(11, 10, 14, 14, PSTONE); c.poly([(2, 8), (6, 4), (11, 8)], TIMBER, PEAT)
    c.rect(6, 0, 6, 4, PGOLD); c.rect(5, 1, 7, 1, PGOLD); c.rect(6, 11, 7, 14, PEAT); c.line(11, 9, 14, 9, TIMBER)


def _gate(c):
    c.rect(2, 3, 4, 14, TIMBER); c.rect(11, 3, 13, 14, TIMBER); c.rect(1, 2, 14, 3, WEARTH); c.rect(5, 6, 10, 14, PEAT)
    c.px(3, 1, TIMBER); c.px(12, 1, TIMBER)


def _well(c):
    c.rect(3, 9, 12, 14, STONE); c.box(3, 9, 12, 14, SLATE); c.line(3, 3, 3, 9, TIMBER); c.line(12, 3, 12, 9, TIMBER)
    c.line(2, 3, 13, 3, TIMBER); c.line(8, 3, 8, 7, SAND); c.rect(7, 7, 9, 8, DEARTH)


def _bridge(c):
    c.rect(0, 5, 15, 6, TIMBER); c.line(0, 4, 15, 4, DEARTH)
    for x in (2, 7, 12):
        c.rect(x, 7, x + 1, 13, WEARTH)
    c.line(0, 12, 15, 12, SHALL); c.line(0, 14, 15, 14, SHOAL)


def _cart(c):
    c.rect(2, 4, 11, 9, DEARTH); c.box(2, 4, 11, 9, PEAT); c.ell(3, 8, 9, 14, TIMBER, PEAT); c.px(6, 11, PEAT)
    c.line(11, 8, 15, 8, TIMBER)


def _bird(c):
    c.poly([(3, 4), (7, 3), (10, 6), (12, 12), (8, 12), (5, 9)], TIMBER, PEAT); c.poly([(3, 4), (1, 6), (4, 6)], GOLD)
    c.px(5, 5, NIGHT); c.line(8, 12, 8, 14, GOLD); c.line(4, 14, 12, 14, WEARTH)


def _mound(c):
    c.ell(1, 5, 14, 20, GRASS, FOREST); c.clear(0, 13, 15, 15); c.line(0, 13, 15, 13, FOREST); c.rect(7, 2, 8, 6, PSTONE)
    c.px(4, 10, MEADOW); c.px(10, 9, MEADOW)


def _runestick(c):
    c.poly([(1, 11), (12, 3), (14, 5), (3, 13)], SAND, PEAT)
    c.line(4, 9, 5, 12, PEAT); c.line(5, 10, 6, 9, PEAT); c.line(7, 7, 8, 10, PEAT); c.line(9, 6, 10, 9, PEAT)
    c.line(10, 7, 11, 6, PEAT); c.line(11, 5, 12, 7, PEAT)


def _plank(c):
    for i, y in enumerate((4, 7, 10)):
        c.rect(1 + i, y, 12 + i, y + 2, DEARTH if i % 2 == 0 else TIMBER); c.line(1 + i, y + 2, 12 + i, y + 2, PEAT)


def _ribs(c):
    c.arc(1, -4, 14, 12, 20, 160, TIMBER, 2)
    for x in (4, 7, 10):
        c.line(x, 5, x, 11, DEARTH)
    c.line(2, 14, 13, 14, WEARTH); c.line(4, 12, 3, 14, WEARTH); c.line(11, 12, 12, 14, WEARTH)


def _tar(c):
    c.poly([(3, 7), (12, 7), (11, 14), (4, 14)], TIMBER, PEAT); c.rect(4, 7, 11, 8, NIGHT); c.arc(3, 2, 12, 10, 180, 360, PEAT)
    c.line(12, 3, 15, 0, TIMBER); c.rect(11, 3, 12, 5, NIGHT)


def _stall(c):
    c.poly([(1, 6), (3, 2), (12, 2), (14, 6)], MAUVE, PEAT); c.line(2, 6, 2, 14, TIMBER); c.line(13, 6, 13, 14, TIMBER)
    c.rect(3, 10, 12, 11, DEARTH); c.rect(4, 8, 5, 9, GOLD); c.rect(7, 8, 8, 9, DROSE); c.rect(10, 8, 11, 9, MEADOW)


def _spade(c):
    c.line(8, 1, 8, 9, TIMBER); c.rect(6, 1, 10, 1, TIMBER); c.poly([(5, 9), (11, 9), (11, 13), (8, 15), (5, 13)], DEARTH, PEAT)


def _flag(c):
    c.rect(3, 1, 3, 15, WWHITE); c.rect(4, 1, 12, 7, BYZ); c.box(4, 1, 12, 7, LILAC)


def _chest(c):
    c.rect(2, 7, 13, 13, TIMBER); c.poly([(2, 7), (4, 3), (11, 3), (13, 7)], DEARTH, PEAT); c.box(2, 7, 13, 13, PEAT)
    c.rect(7, 7, 8, 9, GOLD); c.line(5, 3, 5, 13, WEARTH); c.line(10, 3, 10, 13, WEARTH)


def _wreck(c):
    c.poly([(1, 9), (6, 7), (7, 12), (3, 13)], TIMBER, PEAT); c.poly([(9, 6), (14, 9), (13, 13), (9, 12)], TIMBER, PEAT)
    c.line(8, 1, 6, 8, DEARTH); c.lines([(0, 13), (3, 14), (6, 13), (9, 14), (12, 13), (15, 14)], WAVE)
    c.line(2, 15, 13, 15, SHOAL)


def _ferry(c):
    c.poly([(1, 9), (14, 9), (12, 12), (3, 12)], TIMBER, PEAT); c.line(0, 4, 15, 3, SAND); c.rect(7, 4, 8, 8, SLATE)
    c.rect(7, 3, 8, 4, PEACH); c.line(0, 14, 15, 14, SHALL)


def _store(c):
    c.rect(2, 6, 13, 12, DEARTH); c.poly([(1, 6), (8, 1), (14, 6)], SAND, PEAT); c.rect(6, 8, 9, 12, PEAT)
    c.rect(2, 13, 3, 14, WEARTH); c.rect(12, 13, 13, 14, WEARTH); c.rect(7, 13, 8, 14, WEARTH)
    c.line(2, 12, 13, 12, PEAT)


def _plot(c):
    c.rect(2, 5, 13, 13, MEADOW)
    for x in range(2, 14, 2):
        c.line(x, 3, x, 6, TIMBER); c.line(x, 12, x, 14, TIMBER)
    c.line(2, 5, 13, 5, DEARTH); c.line(2, 13, 13, 13, DEARTH); c.line(2, 5, 2, 13, TIMBER); c.line(13, 5, 13, 13, TIMBER)


def _raider(c):
    c.rect(5, 2, 11, 8, BLOOD); c.box(5, 2, 11, 8, PEAT); c.line(8, 1, 8, 10, TIMBER)
    c.poly([(1, 10), (14, 10), (12, 13), (3, 13)], PEAT, NIGHT); c.line(1, 10, 0, 6, PEAT); c.line(14, 10, 15, 6, PEAT)
    for x in (4, 7, 10):
        c.rect(x, 10, x + 1, 11, GOLD)


def _letter(c):
    c.rect(2, 4, 13, 12, PSAND); c.box(2, 4, 13, 12, DEARTH); c.line(2, 4, 8, 9, DEARTH); c.line(13, 4, 8, 9, DEARTH)
    c.ell(6, 8, 9, 11, BLOOD, PEAT)


def _wheel(c):
    c.ell(1, 1, 14, 14, None, TIMBER); c.ell(2, 2, 13, 13, None, DEARTH); c.line(8, 2, 8, 13, TIMBER)
    c.line(2, 8, 13, 8, TIMBER); c.line(4, 4, 11, 11, TIMBER); c.line(4, 11, 11, 4, TIMBER); c.rect(7, 7, 8, 8, PEAT)


def _bow(c):
    c.arc(-4, 1, 9, 14, 290, 70, TIMBER, 1); c.line(7, 2, 7, 13, PSTONE); c.line(3, 8, 14, 8, DEARTH)
    c.poly([(14, 8), (11, 6), (11, 10)], PSTONE)


def _salt(c):
    c.poly([(1, 13), (6, 5), (9, 4), (14, 13)], CWHITE, PSTONE); c.line(6, 7, 8, 6, FOAM); c.line(1, 14, 14, 14, DEARTH)
    c.px(10, 10, FROST); c.px(5, 11, FROST)


def _basket(c):
    c.poly([(2, 6), (13, 6), (11, 14), (4, 14)], SAND, PEAT)
    for y in (8, 10, 12):
        c.line(3, y, 12, y, DEARTH)
    c.arc(4, 0, 11, 10, 180, 360, DEARTH)


def _person(c):
    c.rect(6, 1, 9, 4, PEACH); c.rect(6, 1, 9, 1, TIMBER); c.rect(4, 5, 11, 11, SHOAL); c.rect(5, 12, 6, 15, WEARTH)
    c.rect(9, 12, 10, 15, WEARTH)


def _quern(c):
    c.ell(1, 4, 14, 13, BASALT, NIGHT); c.ell(1, 2, 14, 11, SLATE, NIGHT); c.ell(6, 5, 9, 7, NIGHT)
    c.line(4, 4, 6, 3, STONE)


def _beads(c):
    c.arc(1, -6, 14, 12, 10, 170, DEARTH)
    for i, (x, y, k) in enumerate(((2, 6, GOLD), (4, 9, RUST), (7, 11, SHALL), (10, 10, GOLD), (12, 7, BYZ), (13, 4, RUST))):
        c.rect(x, y, x + 1, y + 1, k)


def _tusk(c):
    c.arc(-6, 1, 12, 20, 270, 360, WWHITE, 3); c.arc(-2, 3, 14, 22, 280, 360, PSAND, 2)


def _fur(c):
    c.poly([(4, 1), (11, 1), (13, 5), (12, 12), (14, 15), (9, 13), (6, 13), (1, 15), (3, 12), (2, 5)], TIMBER, PEAT)
    c.line(7, 3, 7, 11, WEARTH); c.px(5, 6, DEARTH); c.px(10, 8, DEARTH)


def _question(c):
    for i, b in enumerate(FONT["?"]):
        if b == "1":
            x, y = 3 + (i % 3) * 3, 0 + (i // 3) * 3
            c.rect(x, y, x + 2, y + 2, PGOLD)


def _mast(c):
    c.line(2, 14, 13, 1, DEARTH, 2); c.line(2, 15, 14, 2, TIMBER)


def _mooring(c):
    c.rect(6, 4, 9, 14, TIMBER); c.rect(5, 3, 10, 4, WEARTH); c.arc(2, 6, 13, 12, 0, 180, SAND); c.line(10, 9, 15, 11, SAND)


def _pole(c):
    c.line(4, 15, 11, 0, TIMBER)
    for i, (x, y) in enumerate(((5, 12), (7, 8), (9, 4))):
        c.rect(x, y, x + 1, y, CWHITE if i % 2 == 0 else BLOOD)
    c.lines([(0, 13), (4, 14), (8, 13), (12, 14), (15, 13)], WAVE)


GLYPHS = {
    "coin": _coin, "coins": _coins, "scales": _scales, "tally": _tally, "purse": _purse, "cask": _cask, "sack": _sack,
    "crate": _crate, "bolt": _bolt, "pot": _pot, "sword": _sword, "spear": _spear, "axe": _axe, "shield": _shield,
    "hammer": _hammer, "anvil": _anvil, "cross": _cross, "crosier": _crosier, "book": _book, "leaf": _leaf,
    "quill": _quill, "key": _key, "bell": _bell, "candle": _candle, "casket": _casket, "thor": _thor, "ring": _ring,
    "post": _post, "tree": _tree, "oar": _oar, "sail": _sail, "hull": _hull, "rope": _rope, "fish": _fish, "net": _net,
    "loaf": _loaf, "cup": _cup, "horn": _horn, "dice": _dice, "tafl": _tafl, "lyre": _lyre, "spindle": _spindle,
    "comb": _comb, "sheaf": _sheaf, "crook": _crook, "staff": _staff, "chain": _chain, "bowl": _bowl, "herb": _herb,
    "circlet": _circlet, "fire": _fire, "storm": _storm, "fog": _fog, "snow": _snow, "sun": _sun, "moon": _moon,
    "wind": _wind, "tide": _tide, "speech": _speech, "eye": _eye, "heart": _heart, "bed": _bed, "house": _house,
    "church": _church, "gate": _gate, "well": _well, "bridge": _bridge, "cart": _cart, "bird": _bird, "mound": _mound,
    "runestick": _runestick, "plank": _plank, "ribs": _ribs, "tar": _tar, "stall": _stall, "spade": _spade,
    "flag": _flag, "chest": _chest, "wreck": _wreck, "ferry": _ferry, "store": _store, "plot": _plot,
    "raider": _raider, "letter": _letter, "wheel": _wheel, "bow": _bow, "salt": _salt, "basket": _basket,
    "person": _person, "quern": _quern, "beads": _beads, "tusk": _tusk, "fur": _fur, "question": _question,
    "mast": _mast, "mooring": _mooring, "pole": _pole,
}
_cache = {}


def glyph(name, scale=1, edge=None):
    """A 16 x 16 sign as its own small picture, scaled by a whole number; edge gives it a one pixel rim."""
    key = (name, scale, edge)
    if key not in _cache:
        c = Cv(16, 16)
        fn = GLYPHS.get(name)
        if fn is None:
            c.text(2, 5, (name + "   ")[:3], PGOLD)
        else:
            fn(c)
        if edge:
            c.outline_all(edge)
        _cache[key] = c.scaled(scale) if scale != 1 else c
    return _cache[key]


def glyph_sheet(path, scale=4):
    """All the signs on one sheet, to look at."""
    names = sorted(GLYPHS)
    cols = 12
    rows = (len(names) + cols - 1) // cols
    cell = 16 * scale + 8
    sheet = Cv(cols * cell, rows * (cell + 8), SLATE)
    for i, n in enumerate(names):
        x, y = (i % cols) * cell + 4, (i // cols) * (cell + 8) + 2
        sheet.rect(x, y, x + 16 * scale - 1, y + 16 * scale - 1, DEEP if (i + i // cols) % 2 else PSAND)
        sheet.blit(glyph(n, scale), x, y)
        sheet.text_fit(x, y + 16 * scale + 1, n, PGOLD, cell - 2)
    sheet.im.save(path)
    return path
