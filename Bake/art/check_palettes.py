# -*- coding: utf-8 -*-
"""
CHECK_PALETTES - can everyone tell the chart's colours apart, and can the HUD's words be read?

The game has eight palettes of eight colours, one for each people (Assets/Kapman/Code/Game/Map/MapPalette.cs; the same
colours as .gpl in Design/palettes). This script reads them from the C# file, so it checks what the game shows, and:

  1. for each palette, measures every pair of its eight colours as seen with normal sight and with the three common
     colour-blindnesses, and lists the pairs that cannot be told apart;
  2. checks the marks drawn on the chart (her course in Byzantium, her mark in gold, labels in ink or white) against
     each of the eight grounds;
  3. checks the HUD's text colours against its panel colours (Design/ui_mock/template3.html and the eight UI colours
     of Design/research/ui/pixel_ui.md, A7);
  4. with --mend, looks for the smallest move of lightness, hue and strength that mends each palette while Byzantium
     (#702963) stays where it is.

How it sees:
  * colour-blindness: the matrices of Machado, Oliveira and Fernandes (2009) at full strength, and as a second opinion
    Vienot, Brettel and Mollon (1999) for red- and green-blindness and Brettel, Vienot and Mollon (1997) for
    blue-blindness (constants as in libDaltonLens). All are applied to linear light, not to the sRGB numbers.
    A pair's distance under a blindness is the smaller of the two opinions.
  * how far apart two colours are: CIEDE2000 (dE). Under 5 they look like one colour on a chart; under 10 they can be
    told apart side by side but not named on their own (map classes want 10 or more).
  * how well a word or a thin line stands on its ground: the WCAG contrast ratio (1 to 21). Body text wants 4.5
    (we ask 7 in a game that is all text), big or bold words 3, a line or a mark 3.

Run (PowerShell):  & "C:\\Program Files\\QGIS 3.44.12\\bin\\python-qgis-ltr.bat" Bake\\art\\check_palettes.py
                   add  --mend  to search for the smallest fix for all four sights,  --mend-light  for a fix that
                   moves lightness only (every hue kept),  --mend-rg  for normal, red-blind and green-blind
                   only,  --all  to print every pair, not only the bad ones.
It writes nothing: it only prints.
"""
import math
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
CS = os.path.join(ROOT, "Assets", "Kapman", "Code", "Game", "Map", "MapPalette.cs")
GPL_DIR = os.path.join(ROOT, "Design", "palettes")

ROLES = ["deep water", "sea", "shallows", "shore", "marsh", "cleared land", "woodland", "high ground"]
WATER = (0, 1, 2)
BYZ = "#702963"

SAME, HARD = 5.0, 10.0          # dE: under SAME one colour; under HARD not to be named on its own
LINE, BODY, BODY_WANT = 3.0, 4.5, 7.0

# the palettes as they stood on 7 Oct 2026, used only if the C# file cannot be read
FALLBACK = [
    ("frisian", "Tide", "#3b1540 #702963 #a8619b #e6c9a8 #7d8442 #b3b56a #4a5a2c #f1e8d6"),
    ("frankish", "Vineyard", "#2b1233 #5a2970 #9466ab #f0d27a #9a4a5a #cf9a6a #70293f #f6efe0"),
    ("english", "Minster", "#2e1233 #702963 #a070a6 #ece5cc #5f8a5c #9fbf7a #297036 #dfe8e0"),
    ("danish", "Amber", "#14323a #296370 #73a7ad #f3dfa8 #a07c2c #d9a441 #702963 #f6efe0"),
    ("norse", "Fjord", "#171a38 #34407c #8494c6 #cbbccb #702963 #5fa08a #1f4a44 #f4f1fa"),
    ("svea", "Silver", "#241a2e #4a3d5c #8f82a3 #e8e4ee #702963 #a9b08a #4f5a4a #faf8fc"),
    ("wendish", "Grove", "#2e1233 #702963 #b06aa0 #dccba0 #637029 #94a23c #414c1a #efe9d0"),
    ("irish", "Vellum", "#1f2a44 #2f5d7a #82b3ba #f3e7c4 #c9553a #d8c27a #3f7029 #702963"),
]


# ------------------------------------------------------------------------------------------------ colour sums
def hex_rgb(h):
    h = h.strip().lstrip("#")
    return tuple(int(h[i:i + 2], 16) / 255.0 for i in (0, 2, 4))


def rgb_hex(rgb):
    return "#" + "".join("%02x" % max(0, min(255, int(round(v * 255)))) for v in rgb)


def to_lin(v):
    return v / 12.92 if v <= 0.04045 else ((v + 0.055) / 1.055) ** 2.4


def to_srgb(v):
    v = max(0.0, min(1.0, v))
    return v * 12.92 if v <= 0.0031308 else 1.055 * v ** (1 / 2.4) - 0.055


def lin(rgb):
    return tuple(to_lin(v) for v in rgb)


def unlin(l):
    return tuple(to_srgb(v) for v in l)


def lum(l):
    return 0.2126 * l[0] + 0.7152 * l[1] + 0.0722 * l[2]


def ratio(la, lb):
    a, b = lum(la), lum(lb)
    if a < b:
        a, b = b, a
    return (a + 0.05) / (b + 0.05)


XN, YN, ZN = 0.95047, 1.0, 1.08883


def lab_of(l):
    r, g, b = l
    x = (0.4124564 * r + 0.3575761 * g + 0.1804375 * b) / XN
    y = (0.2126729 * r + 0.7151522 * g + 0.0721750 * b) / YN
    z = (0.0193339 * r + 0.1191920 * g + 0.9503041 * b) / ZN

    def f(t):
        return t ** (1 / 3.0) if t > 216 / 24389.0 else (24389 / 27.0 * t + 16) / 116.0

    fx, fy, fz = f(x), f(y), f(z)
    return (116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz))


def lin_of_lab(lab):
    L, a, b = lab
    fy = (L + 16) / 116.0
    fx, fz = fy + a / 500.0, fy - b / 200.0

    def g(t):
        return t ** 3 if t ** 3 > 216 / 24389.0 else (116 * t - 16) * 27 / 24389.0

    x, y, z = g(fx) * XN, g(fy) * YN, g(fz) * ZN
    return (3.2404542 * x - 1.5371385 * y - 0.4985314 * z,
            -0.9692660 * x + 1.8760108 * y + 0.0415560 * z,
            0.0556434 * x - 0.2040259 * y + 1.0572252 * z)


def de2000(p, q):
    L1, a1, b1 = p
    L2, a2, b2 = q
    c1, c2 = math.hypot(a1, b1), math.hypot(a2, b2)
    cb = (c1 + c2) / 2.0
    g = 0.5 * (1 - math.sqrt(cb ** 7 / (cb ** 7 + 25.0 ** 7)))
    a1p, a2p = (1 + g) * a1, (1 + g) * a2
    c1p, c2p = math.hypot(a1p, b1), math.hypot(a2p, b2)
    h1 = math.degrees(math.atan2(b1, a1p)) % 360 if c1p else 0.0
    h2 = math.degrees(math.atan2(b2, a2p)) % 360 if c2p else 0.0
    dl, dc = L2 - L1, c2p - c1p
    if c1p * c2p == 0:
        dh = 0.0
    else:
        dh = h2 - h1
        if dh > 180:
            dh -= 360
        elif dh < -180:
            dh += 360
    dH = 2 * math.sqrt(c1p * c2p) * math.sin(math.radians(dh / 2.0))
    lb, cbp = (L1 + L2) / 2.0, (c1p + c2p) / 2.0
    if c1p * c2p == 0:
        hb = h1 + h2
    elif abs(h1 - h2) <= 180:
        hb = (h1 + h2) / 2.0
    elif h1 + h2 < 360:
        hb = (h1 + h2 + 360) / 2.0
    else:
        hb = (h1 + h2 - 360) / 2.0
    t = (1 - 0.17 * math.cos(math.radians(hb - 30)) + 0.24 * math.cos(math.radians(2 * hb))
         + 0.32 * math.cos(math.radians(3 * hb + 6)) - 0.20 * math.cos(math.radians(4 * hb - 63)))
    dth = 30 * math.exp(-((hb - 275) / 25.0) ** 2)
    rc = 2 * math.sqrt(cbp ** 7 / (cbp ** 7 + 25.0 ** 7))
    sl = 1 + 0.015 * (lb - 50) ** 2 / math.sqrt(20 + (lb - 50) ** 2)
    sc = 1 + 0.045 * cbp
    sh = 1 + 0.015 * cbp * t
    rt = -math.sin(math.radians(2 * dth)) * rc
    return math.sqrt((dl / sl) ** 2 + (dc / sc) ** 2 + (dH / sh) ** 2 + rt * (dc / sc) * (dH / sh))


def self_test():
    """Four of Sharma's pairs: if these are right the sum is right."""
    for p, q, want in (((50.0, 2.6772, -79.7751), (50.0, 0.0, -82.7485), 2.0425),
                       ((50.0, 2.5, 0.0), (73.0, 25.0, -18.0), 27.1492),
                       ((60.2574, -34.0099, 36.2677), (60.4626, -34.1751, 39.4387), 1.2644),
                       ((2.0776, 0.0795, -1.1350), (0.9033, -0.0636, -0.5514), 0.9082)):
        got = de2000(p, q)
        if abs(got - want) > 0.0002:
            raise SystemExit("the CIEDE2000 sum is wrong: %.4f, not %.4f" % (got, want))


# ------------------------------------------------------------------------------------------------ the three blindnesses
MACHADO = {
    "red-blind": ((0.152286, 1.052583, -0.204868), (0.114503, 0.786281, 0.099216), (-0.003882, -0.048116, 1.051998)),
    "green-blind": ((0.367322, 0.860646, -0.227968), (0.280085, 0.672501, 0.047413), (-0.011820, 0.042940, 0.968881)),
    "blue-blind": ((1.255528, -0.076749, -0.178779), (-0.078411, 0.930809, 0.147602), (0.004733, 0.691367, 0.303900)),
}
VIENOT = {
    "red-blind": ((0.11238, 0.88762, 0.0), (0.11238, 0.88762, 0.0), (0.00401, -0.00401, 1.0)),
    "green-blind": ((0.29275, 0.70725, 0.0), (0.29275, 0.70725, 0.0), (-0.02234, 0.02234, 1.0)),
}
BRETTEL_BLUE = (((1.01277, 0.13548, -0.14826), (-0.01243, 0.86812, 0.14431), (0.07589, 0.80500, 0.11911)),
                ((0.93678, 0.18979, -0.12657), (0.06154, 0.81526, 0.12320), (-0.37562, 1.12767, 0.24796)),
                (0.03901, -0.02788, -0.01113))
SIGHTS = ["normal", "red-blind", "green-blind", "blue-blind"]
SHORT = {"normal": "norm", "red-blind": "red-b", "green-blind": "grn-b", "blue-blind": "blu-b"}


def mul(m, v):
    return tuple(max(0.0, min(1.0, m[i][0] * v[0] + m[i][1] * v[1] + m[i][2] * v[2])) for i in range(3))


def seen(l, sight):
    """Linear light as each opinion says that sight sees it: a list of one (normal) or two."""
    if sight == "normal":
        return [l]
    out = [mul(MACHADO[sight], l)]
    if sight in VIENOT:
        out.append(mul(VIENOT[sight], l))
    else:
        m1, m2, n = BRETTEL_BLUE
        out.append(mul(m1 if n[0] * l[0] + n[1] * l[1] + n[2] * l[2] >= 0 else m2, l))
    return out


class Col(object):
    """A colour, with what each sight makes of it worked out once."""

    def __init__(self, hx=None, linear=None):
        first = lin(hex_rgb(hx)) if linear is None else tuple(max(0.0, min(1.0, v)) for v in linear)
        self.hex = rgb_hex(unlin(first))
        self.lin = lin(hex_rgb(self.hex))                       # what a screen of 8 bits can show
        self.lab = lab_of(self.lin)
        self.views = dict((s, [(v, lab_of(v)) for v in seen(self.lin, s)]) for s in SIGHTS)


def dist(a, b, sight):
    return min(de2000(x[1], y[1]) for x, y in zip(a.views[sight], b.views[sight]))


def worst(a, b, sights=None):
    return min(dist(a, b, s) for s in (sights or SIGHTS))


def con(a, b, sight="normal"):
    return min(ratio(x[0], y[0]) for x, y in zip(a.views[sight], b.views[sight]))


def con_worst(a, b):
    return min(con(a, b, s) for s in SIGHTS)


def over(top_hex, alpha, under_hex):
    """A see-through colour laid on another (as the screen mixes them: on the sRGB numbers)."""
    t, u = hex_rgb(top_hex), hex_rgb(under_hex)
    return rgb_hex(tuple(alpha * a + (1 - alpha) * b for a, b in zip(t, u)))


# ------------------------------------------------------------------------------------------------ reading the palettes
def read_palettes():
    pals, note = [], ""
    try:
        with open(CS, "r", encoding="utf-8") as f:
            src = f.read()
        for m in re.finditer(r'P\(\s*"(\w+)"\s*,\s*"([^"]+)"\s*,\s*"[^"]+"\s*,((?:\s*"#[0-9a-fA-F]{6}"\s*,?){8})\)', src):
            pals.append((m.group(1), m.group(2), [h.lower() for h in re.findall(r"#[0-9a-fA-F]{6}", m.group(3))]))
    except (IOError, OSError):
        pass
    if len(pals) < 1:
        pals = [(p, n, h.split()) for p, n, h in FALLBACK]
        note = "MapPalette.cs could not be read: these are the colours of 7 Oct 2026."
    return pals, note


def check_gpl(pals):
    """The .gpl files the user loads in Aseprite must hold the same colours as the game."""
    bad = []
    if not os.path.isdir(GPL_DIR):
        return bad
    for people, name, hexes in pals:
        path = os.path.join(GPL_DIR, "%s_%s.gpl" % (people, name.lower()))
        if not os.path.exists(path):
            bad.append("%s: no file %s" % (people, os.path.basename(path)))
            continue
        got = []
        with open(path, "r", encoding="utf-8") as f:
            for line in f:
                p = line.split()
                if len(p) >= 3 and p[0].isdigit() and p[1].isdigit() and p[2].isdigit():
                    got.append("#%02x%02x%02x" % (int(p[0]), int(p[1]), int(p[2])))
        if got != hexes:
            bad.append("%s: %s differs from MapPalette.cs" % (people, os.path.basename(path)))
    return bad


# ------------------------------------------------------------------------------------------------ 1. the pairs
def pair_rows(cols):
    rows = []
    for i in range(8):
        for j in range(i + 1, 8):
            d = [dist(cols[i], cols[j], s) for s in SIGHTS]
            rows.append((min(d), i, j, d, con(cols[i], cols[j])))
    rows.sort()
    return rows


def word(d):
    return "ONE COLOUR" if d < SAME else "hard" if d < HARD else "ok"


def report_pairs(people, name, hexes, show_all):
    cols = [Col(h) for h in hexes]
    rows = pair_rows(cols)
    bad = [r for r in rows if r[0] < HARD]
    print("")
    print("== %s (%s)   %s" % (name, people, " ".join(hexes)))
    print("   lightness L*: " + "  ".join("%s %.0f" % (ROLES[i].split()[0], cols[i].lab[0]) for i in range(8)))
    print("   nearest pair by sight: " + "; ".join(
        "%s %.1f" % (SHORT[s], min(dist(cols[i], cols[j], s) for i in range(8) for j in range(i + 1, 8))) for s in SIGHTS))
    if not bad:
        print("   every pair is %.0f or more apart to every sight." % HARD)
    if bad or show_all:
        print("   %-28s %6s %6s %6s %6s  %5s" % ("pair", "norm", "red-b", "grn-b", "blu-b", "ratio"))
    for mn, i, j, d, c in (rows if show_all else bad):
        tag = "coast" if (i in WATER) != (j in WATER) else ("water" if j in WATER else "land")
        fails = ", ".join("%s" % SHORT[s] for s, v in zip(SIGHTS, d) if v < HARD)
        print("   %-28s %6.1f %6.1f %6.1f %6.1f  %5.2f  %-10s %s%s" % (
            "%s / %s" % (ROLES[i], ROLES[j]), d[0], d[1], d[2], d[3], c, word(mn), tag, (" (" + fails + ")") if fails else ""))
    return cols, bad


# ------------------------------------------------------------------------------------------------ 2. marks on the chart
MARKS = [("her course (byzantium)", "#702963", LINE), ("her mark (gold)", "#d9a441", LINE),
         ("a label in ink (night)", "#0b0810", BODY), ("a label in warm white", "#f6efe0", BODY)]


# a mark of two colours is read by whichever of the two stands out more from the ground under it
CASED = [("her course: byzantium with a warm-white edge", ("#702963", "#f6efe0"), LINE),
         ("her mark: gold with an ink edge (as in the mock-up)", ("#d9a441", "#150f1c"), LINE),
         ("any mark: warm white with an ink edge", ("#f6efe0", "#0b0810"), LINE),
         ("the slip against the chart: ink fill, byzantium border", ("#110b17", "#702963"), LINE),
         ("the slip against the chart: ink fill, light bevel #8c6a8e", ("#110b17", "#8c6a8e"), LINE),
         ("a label: warm white with an ink edge (or ink on a warm-white plate)", ("#f6efe0", "#0b0810"), BODY)]


def report_marks(pals):
    print("")
    print("== MARKS ON THE CHART: the contrast ratio of each mark on each ground, worst of the four sights")
    print("   (a line or a mark wants 3, a label wants 4.5; ' !' is under what it wants)")
    for mname, mhex, want in MARKS:
        m = Col(mhex)
        print("   %s  %s" % (mname, mhex))
        for people, name, hexes in pals:
            cells = []
            for i, h in enumerate(hexes):
                r = con_worst(m, Col(h))
                cells.append("%s %4.1f%s" % (ROLES[i].split()[0][:5], r, " !" if r < want else "  "))
            print("     %-9s %s" % (name, " ".join(cells)))
    print("")
    print("   marks of two colours (the better of the two on each ground): the weakest ground in each palette")
    for mname, pair, want in CASED:
        a, b = Col(pair[0]), Col(pair[1])
        fails, low = [], (99.0, "")
        for people, name, hexes in pals:
            for i, h in enumerate(hexes):
                g = Col(h)
                r = max(con_worst(a, g), con_worst(b, g))
                if r < low[0]:
                    low = (r, "%s %s" % (name, ROLES[i]))
                if r < want:
                    fails.append("%s %s %.1f" % (name, ROLES[i], r))
        print("   %-68s weakest %4.1f on %s (wants %.1f)" % (mname, low[0], low[1], want))
        if fails:
            print("       under: " + "; ".join(fails))


# ------------------------------------------------------------------------------------------------ 3. the HUD
INK = "#110b17"          # the slip's fill in the mock-up: rgba(17,11,23,.94)
HUD = [
    # words, ground, what it is, wanted ratio
    ("#f1e8d6", "ink", "paper text on a slip (body)", BODY_WANT),
    ("#a79fae", "ink", "mute text on a slip (second rank, hints)", BODY),
    ("#dba3cb", "ink", "lilac labels and window names", BODY),
    ("#f0d27a", "ink", "gold numbers and headings", BODY),
    ("#f0d27a", "#702963", "gold on byzantium: the chosen button or tab", BODY),
    ("#f1e8d6", "#702963", "paper on byzantium", BODY),
    ("#a79fae", "#2a1c2e", "mute on a tab not chosen", BODY),
    ("#a79fae", "#150f1c", "mute on a settings button", BODY),
    ("#f1e8d6", "#3b2a3f", "paper on a key button", BODY),
    ("#150f1c", "#f0d27a", "dark on gold: a count badge", BODY),
    ("#150f1c", "#dba3cb", "dark on lilac: the clock's chosen speed", BODY),
    ("#a79fae", "row", "mute small text in a list row (ink darkened by 22%)", BODY),
    ("#702963", "ink", "the byzantium border of a slip against its own fill (a line)", LINE),
    ("rule", "ink", "the thin rule between rows (lilac at 28%) (a line)", LINE),
    # the eight UI colours of pixel_ui.md A7
    ("#f6efe0", "#0b0810", "kit: warm white text on night", BODY_WANT),
    ("#aaa4ae", "#0b0810", "kit: pale stone quiet text on night", BODY),
    ("#d9a441", "#0b0810", "kit: gold (yours) on night", BODY),
    ("#f6efe0", "#2e1233", "kit: warm white on a well (dark purple)", BODY_WANT),
    ("#aaa4ae", "#2e1233", "kit: pale stone on a well", BODY),
    ("#f6efe0", "#4a1c4a", "kit: warm white on plum", BODY),
    ("#aaa4ae", "#4a1c4a", "kit: pale stone on plum (a disabled thing)", LINE),
    ("#f6efe0", "#702963", "kit: warm white on the byzantium edge", BODY),
    ("#4a1c4a", "#0b0810", "kit: a plum rule on night (a line)", LINE),
    ("#702963", "#0b0810", "kit: the byzantium edge on night (a line)", LINE),
    ("#7f9a4a", "#0b0810", "kit: good (meadow) on night", BODY),
    ("#c9553a", "#0b0810", "kit: warning (rust) on night", BODY),
    ("#8f2d2d", "#0b0810", "kit: bad (blood) on night", BODY),
]
SIGNALS = [("good (meadow)", "#7f9a4a"), ("warning (rust)", "#c9553a"), ("bad (blood)", "#8f2d2d"),
           ("yours (gold)", "#d9a441"), ("quiet (pale stone)", "#aaa4ae")]


def report_hud(pals):
    grounds = [h for _, _, hexes in pals for h in hexes]
    light = max(grounds, key=lambda h: lum(lin(hex_rgb(h))))
    dark = min(grounds, key=lambda h: lum(lin(hex_rgb(h))))
    print("")
    print("== THE HUD: words on panels. The slip is 94% ink, so the chart shows through: it is tried over the")
    print("   lightest ground of any palette (%s) and the darkest (%s). Ratio is the worst of the four sights." % (light, dark))
    print("   %-62s %6s %6s" % ("what", "ratio", "wants"))
    for fg, bg, what, want in HUD:
        worst_r, where = 99.0, ""
        for under in (light, dark):
            if bg == "ink":
                b = over(INK, 0.94, under)
            elif bg == "row":
                b = over("#000000", 0.22, over(INK, 0.94, under))
            else:
                b = bg
            f = over("#c77fb0", 0.28, b) if fg == "rule" else fg
            r = con_worst(Col(f), Col(b))
            if r < worst_r:
                worst_r, where = r, "%s on %s" % (f, b)
        print("   %-62s %6.2f %6.1f  %s  %s" % (what, worst_r, want, "ok  " if worst_r >= want else "FAIL", where))
    print("")
    print("   the signal colours against each other (dE by sight; they must never be the only sign):")
    for i in range(len(SIGNALS)):
        for j in range(i + 1, len(SIGNALS)):
            a, b = Col(SIGNALS[i][1]), Col(SIGNALS[j][1])
            d = [dist(a, b, s) for s in SIGHTS]
            mn = min(d)
            print("   %-20s / %-20s norm %5.1f  red-b %5.1f  grn-b %5.1f  blu-b %5.1f  %s" % (
                SIGNALS[i][0], SIGNALS[j][0], d[0], d[1], d[2], d[3], word(mn)))


# ------------------------------------------------------------------------------------------------ 4. the smallest mend
def lch(lab):
    return lab[0], math.hypot(lab[1], lab[2]), math.degrees(math.atan2(lab[2], lab[1])) % 360


def col_lch(L, c, h):
    L = max(2.0, min(98.0, L))
    c = max(0.0, c)
    l = (0.0, 0.0, 0.0)
    for _ in range(40):                                        # pull the strength in until a screen can show it
        l = lin_of_lab((L, c * math.cos(math.radians(h)), c * math.sin(math.radians(h))))
        if all(-0.0005 <= v <= 1.0005 for v in l):
            break
        c *= 0.94
    return Col(linear=l)


MEND_SIGHTS = list(SIGHTS)       # --mend-rg leaves blue-blindness out (it is some hundreds of times rarer)


def shortfall(cols, target):
    s = 0.0
    for i in range(8):
        for j in range(i + 1, 8):
            s += max(0.0, target - worst(cols[i], cols[j], MEND_SIGHTS))
    return s


STEPS = [(dl, dh, dc) for dl in (0, 2, -2, 4, -4, 7, -7) for dh in (0, 5, -5, 10, -10, 18, -18) for dc in (0, 6, -6)
         if (dl, dh, dc) != (0, 0, 0)]
ALL_STEPS = list(STEPS)
LIGHT_STEPS = [(dl, 0, 0) for dl in (1, -1, 2, -2, 4, -4, 7, -7, 11, -11)]      # --mend-light: every hue is kept


def step_cost(dl, dh, dc):
    return abs(dl) + abs(dh) * 0.35 + abs(dc) * 0.5 + 0.2      # lightness and hue first; strength last


def mend(hexes, target):
    """Moves the seven free colours in the smallest steps that bring every pair to the target under every sight."""
    orig = [Col(h) for h in hexes]
    fixed = [i for i, h in enumerate(hexes) if h.lower() == BYZ]
    cols = list(orig)
    sf = shortfall(cols, target)
    guard = 0
    while sf > 1e-6 and guard < 300:
        guard += 1
        best = None
        failing = set()
        for i in range(8):
            for j in range(i + 1, 8):
                if worst(cols[i], cols[j], MEND_SIGHTS) < target:
                    failing.add(i)
                    failing.add(j)
        for k in sorted(failing):
            if k in fixed:
                continue
            L, c, h = lch(cols[k].lab)
            for dl, dh, dc in STEPS:
                cand = col_lch(L + dl, c + dc, h + dh)
                if cand.hex == cols[k].hex:
                    continue
                trial = cols[:k] + [cand] + cols[k + 1:]
                if any(trial[w].lab[0] >= trial[w + 1].lab[0] - 4 for w in (0, 1)):    # deep < sea < shallows stays
                    continue
                gain = sf - shortfall(trial, target)
                if gain <= 1e-6:
                    continue
                far = de2000(orig[k].lab, cand.lab) - de2000(orig[k].lab, cols[k].lab)
                score = gain / (step_cost(dl, dh, dc) + max(0.0, far))
                if best is None or score > best[0]:
                    best = (score, k, cand)
        if best is None:
            break
        cols[best[1]] = best[2]
        sf = shortfall(cols, target)
    # walk each moved colour back toward where it was, as far as the pairs allow
    for _ in range(3):
        for k in range(8):
            if cols[k].hex == orig[k].hex:
                continue
            lo, hi = 0.0, 1.0                                  # 0 = as it was, 1 = as moved
            a, b = orig[k].lab, cols[k].lab
            for _ in range(7):
                mid = (lo + hi) / 2.0
                lab = tuple(a[n] + (b[n] - a[n]) * mid for n in range(3))
                cand = Col(linear=lin_of_lab(lab))
                trial = cols[:k] + [cand] + cols[k + 1:]
                if shortfall(trial, target) <= 1e-6:
                    hi = mid
                else:
                    lo = mid
            lab = tuple(a[n] + (b[n] - a[n]) * hi for n in range(3))
            cand = Col(linear=lin_of_lab(lab))
            if shortfall(cols[:k] + [cand] + cols[k + 1:], target) <= 1e-6:
                cols[k] = cand
    return orig, cols, shortfall(cols, target)


def report_mend(pals, target):
    print("")
    print("== THE SMALLEST MEND%s: every pair %.0f or more apart to %s, Byzantium not moved" % (
        " BY LIGHTNESS ALONE" if STEPS == LIGHT_STEPS else "", target,
        "every sight" if len(MEND_SIGHTS) == 4 else "normal, red-blind and green-blind sight"))
    for people, name, hexes in pals:
        print("")
        print("   %s (%s)" % (name, people))
        if BYZ in hexes:
            orig, cols, left = mend(hexes, target)
        else:
            # no Byzantium in it: put it in the place of each of the two colours nearest to it, and keep the cheaper
            first = [Col(h) for h in hexes]
            b = Col(BYZ)
            near = sorted(range(8), key=lambda k: de2000(first[k].lab, b.lab))[:2]
            tries = []
            for k in near:
                o, c, l = mend(hexes[:k] + [BYZ] + hexes[k + 1:], target)
                cost = sum(de2000(first[n].lab, c[n].lab) for n in range(8))
                tries.append((l > 1e-6, cost, k, c, l))
                print("     Byzantium in the place of %s (%s): all the moves come to %.1f dE%s" % (
                    ROLES[k], hexes[k], cost, "" if l <= 1e-6 else ", not fully mended"))
            tries.sort(key=lambda t: (t[0], t[1]))
            orig, cols, left = first, tries[0][3], tries[0][4]
        moved = [k for k in range(8) if cols[k].hex != orig[k].hex]
        if not moved:
            print("     nothing to move.")
            continue
        for k in moved:
            L0, c0, h0 = lch(orig[k].lab)
            L1, c1, h1 = lch(cols[k].lab)
            dh = ((h1 - h0 + 180) % 360) - 180
            print("     %-13s %s -> %s   lightness %+5.1f   hue %+5.1f deg   strength %+5.1f   (moved %.1f dE)" % (
                ROLES[k], orig[k].hex, cols[k].hex, L1 - L0, dh, c1 - c0, de2000(orig[k].lab, cols[k].lab)))
        near = min((worst(cols[i], cols[j], MEND_SIGHTS), i, j) for i in range(8) for j in range(i + 1, 8))
        print("     now: %s" % " ".join(c.hex for c in cols))
        print("     all the moves come to %.1f dE; nearest pair now %.1f (%s / %s)%s" % (
            sum(de2000(orig[k].lab, cols[k].lab) for k in range(8)), near[0], ROLES[near[1]], ROLES[near[2]],
            "" if left <= 1e-6 else "   NOT FULLY MENDED: %.1f short" % left))
        if len(MEND_SIGHTS) < 4:
            blue = [(dist(cols[i], cols[j], "blue-blind"), i, j) for i in range(8) for j in range(i + 1, 8)]
            blue = [t for t in blue if t[0] < target]
            if blue:
                print("     still under %.0f to blue-blind sight: %s" % (target, "; ".join(
                    "%s / %s %.1f" % (ROLES[i], ROLES[j], d) for d, i, j in sorted(blue))))
        m = Col(BYZ)
        low = [(con_worst(m, cols[i]), ROLES[i]) for i in range(8) if cols[i].hex != BYZ]
        print("     her course in Byzantium is weakest on %s (ratio %.2f)" % (min(low)[1], min(low)[0]))


# ------------------------------------------------------------------------------------------------ the run
def main(argv):
    self_test()
    show_all = "--all" in argv
    pals, note = read_palettes()
    print("KAPMAN: THE CHART'S EIGHT PALETTES AND THE HUD, CHECKED FOR EVERY SIGHT")
    print("dE under %.0f = one colour; under %.0f = hard (told apart side by side, not named alone)." % (SAME, HARD))
    if note:
        print(note)
    for line in check_gpl(pals):
        print("NOTE: " + line)
    for people, name, hexes in pals:
        if BYZ not in hexes:
            print("NOTE: %s has no Byzantium (%s)" % (name, BYZ))
    total = {}
    for people, name, hexes in pals:
        cols, bad = report_pairs(people, name, hexes, show_all)
        total[name] = (len([b for b in bad if b[0] < SAME]), len(bad))
    print("")
    print("== THE COUNT: pairs of 28 that fail, by palette")
    for people, name, hexes in pals:
        print("   %-9s one colour: %d   hard or worse: %d" % (name, total[name][0], total[name][1]))
    report_marks(pals)
    report_hud(pals)
    if "--mend" in argv:
        report_mend(pals, HARD)
    if "--mend-light" in argv:
        STEPS[:] = LIGHT_STEPS
        report_mend(pals, HARD)
        STEPS[:] = ALL_STEPS
    if "--mend-rg" in argv:
        MEND_SIGHTS[:] = ["normal", "red-blind", "green-blind"]
        report_mend(pals, HARD)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
