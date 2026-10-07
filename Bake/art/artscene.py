# -*- coding: utf-8 -*-
"""
ARTSCENE - the bigger placeholders (make_art_list.py calls these): a site of a port as a scene, a port from the water,
a card's picture, an ending, a plate. Each is put together by rule from the data: the kind of site, the port's
landing and country, the card's stage and the plain words it uses (toll, fog, a sail, a church, silver ...).
"""
import math
import os
import re

from artkit import *          # noqa: F401,F403
from artkit import Cv, glyph, rng_of, placeholder_mark, label_strip
from artdraw import num, has_word, ship_traits, _hull_side, sign_for, ROLE_TOKEN, CLERGY, NUNS

SKIES = {"day": [SKY, SKY, FROST], "dawn": [SHOAL, DROSE, PEACH, MGOLD], "dusk": [DPURP, PLUM, DROSE, PEACH],
         "night": [NIGHT, NIGHT, DPURP], "fog": [PSTONE, FROST, CWHITE], "storm": [BASALT, SLATE, STONE],
         "winter": [FROST, SNOW, CWHITE]}
SEAS = {"day": [SHOAL, SEA, DEEP], "dawn": [SHALL, SHOAL, SEA], "dusk": [SEA, DEEP, ABYSS], "night": [DEEP, ABYSS, ABYSS],
        "fog": [WAVE, SHALL, SHOAL], "storm": [SEA, DEEP, ABYSS], "winter": [SHALL, SHOAL, SEA]}
SEA_LIGHT = {"day": WAVE, "dawn": PEACH, "dusk": MAUVE, "night": SHOAL, "fog": FOAM, "storm": FOAM, "winter": FOAM}
MOOD_WORDS = [
    ("night", ("night", "midnight", "moonlight", "moonlit", "starlight", "after dark", "by torch", "dark of")),
    ("storm", ("storm", "gale", "squall", "thunder", "driving rain", "heavy rain", "lashing", "heavy sea")),
    ("fog", ("fog", "mist(?![a-z])", "misty", "mists", "haze(?![a-z])", "hazy")),
    ("winter", ("snow", "frost", "icy", "frozen", "winter", "yule", "midwinter", "candlemas", "on the ice")),
    ("dusk", ("dusk", "evening", "sunset", "vespers", "twilight", "firelight", "last light", "nightfall", "late light")),
    ("dawn", ("dawn", "sunrise", "first light", "daybreak", "early morning", "matins", "sun-up")),
]
TERRAIN = {}
for _t, _regs in (("marsh", "rhine_delta zeeland frisia"), ("river", "rhineland meuse flanders neustria loire aquitaine saxony"),
                  ("down", "england_south england_east"), ("moor", "northumbria mercia_west cornwall wales ireland dalriata shetland"),
                  ("fjord", "vestfold norway_west norway_north"),
                  ("wood", "jutland schleswig danish_isles skane wendland prussia curonia finns svealand gotland ladoga rus_volga")):
    for _r in _regs.split():
        TERRAIN[_r] = _t


def mood_of(text, rng, default=None):
    """The light of a picture from the plain words of its text; (mood, True) if the text gave it."""
    for m, ws in MOOD_WORDS:
        if has_word(text, *ws):
            return m, True
    return (default or rng.choice(("day", "day", "day", "dawn", "dusk"))), False


def sky(cv, x0, y0, x1, y1, mood, rng):
    bands = SKIES[mood]
    n, h, w = len(bands), y1 - y0 + 1, x1 - x0 + 1
    for i, c in enumerate(bands):
        ya, yb = y0 + h * i // n, y0 + h * (i + 1) // n - 1
        cv.rect(x0, ya, x1, yb, c)
        if i > 0:
            cv.dither(x0, ya - max(1, h // 24), x1, ya - 1, c)
    if mood in ("day", "dawn", "dusk"):
        rad = max(2, h // 9)
        sx, sy = x0 + int(w * rng.uniform(0.15, 0.85)), y0 + int(h * (0.3 if mood == "day" else 0.8))
        cv.ell(sx - rad, sy - rad, sx + rad, sy + rad, WWHITE if mood == "day" else PGOLD)
    if mood == "night":
        for _ in range(max(6, w * h // 300)):
            cv.px(rng.randint(x0, x1), rng.randint(y0, y1), CWHITE if rng.random() < .7 else LILAC)
        rad = max(2, h // 10)
        sx, sy = x0 + int(w * rng.uniform(0.2, 0.8)), y0 + int(h * 0.3)
        cv.ell(sx - rad, sy - rad, sx + rad, sy + rad, PGOLD)
        cv.ell(sx - rad + rad // 2 + 1, sy - rad - 1, sx + rad + rad // 2 + 1, sy + rad - 1, bands[0])
    if mood in ("day", "dawn", "dusk", "storm", "winter"):
        cc = {"day": CWHITE, "dawn": MGOLD, "dusk": MAUVE, "storm": BASALT, "winter": CWHITE}[mood]
        for _ in range(max(1, w // 120) + (2 if mood == "storm" else 0)):
            cw = rng.randint(max(6, w // 10), max(8, w // 4))
            cx_, cy = rng.randint(x0, max(x0, x1 - cw)), y0 + rng.randint(h // 10, max(h // 10 + 1, h * 5 // 10))
            ch = max(2, h // 14)
            cv.rect(cx_, cy, cx_ + cw, cy + ch, cc); cv.rect(cx_ + cw // 5, cy - ch // 2 - 1, cx_ + cw * 4 // 5, cy, cc)
            cv.rect(cx_ - cw // 8, cy + ch // 2, cx_ + cw // 2, cy + ch + 1, cc)


def sea(cv, x0, y0, x1, y1, mood, rng, calm=False):
    bands = SEAS[mood]
    n, h, w = len(bands), y1 - y0 + 1, x1 - x0 + 1
    if h <= 0:
        return
    for i, c in enumerate(bands):
        ya, yb = y0 + h * i // n, y0 + h * (i + 1) // n - 1
        cv.rect(x0, ya, x1, yb, c)
        if i > 0:
            cv.dither(x0, ya - max(1, h // 20), x1, ya - 1, c)
    lite = SEA_LIGHT[mood]
    for _ in range(w * h // (240 if calm else 80)):
        x, y = rng.randint(x0, x1), rng.randint(y0, y1)
        ln = rng.randint(1, 3 + (y - y0) * 6 // max(1, h))
        band = bands[max(0, min(n - 1, (y - y0) * n // h) - 1)]
        cv.line(x, y, min(x1, x + ln), y, lite if rng.random() < 0.45 else band)


def weather(cv, x0, y0, x1, y1, mood, rng):
    """Rain, fog or falling snow over a finished picture."""
    w, h = x1 - x0 + 1, y1 - y0 + 1
    if mood == "storm":
        for _ in range(w * h // 220):
            x, y = rng.randint(x0, x1), rng.randint(y0, y1 - 5)
            cv.line(x, y, max(x0, x - 2), y + 5, SHALL)
    elif mood == "fog":
        for i in range(h // 14):
            y = y0 + rng.randint(h // 4, h - 2)
            xa = rng.randint(x0 - w // 4, x1 - w // 4); xb = xa + rng.randint(w // 4, w * 3 // 4)
            cv.dither(max(x0, xa), y, min(x1, xb), min(y1, y + rng.randint(2, 5)), FROST if i % 2 else CWHITE)
    elif mood == "winter":
        for _ in range(w * h // 240):
            cv.px(rng.randint(x0, x1), rng.randint(y0, y1), CWHITE)


def trap(cv, xl0, xr0, y0, xl1, xr1, y1, c, shade=None, every=3):
    """A roof: a four-sided shape from (xl0..xr0 at y0) down to (xl1..xr1 at y1), with thatch lines."""
    cv.poly([(xl0, y0), (xr0, y0), (xr1, y1), (xl1, y1)], c)
    if shade and y1 > y0:
        for y in range(int(y0) + 1, int(y1), every):
            f = (y - y0) / float(y1 - y0)
            cv.line(xl0 + (xl1 - xl0) * f + 1, y, xr0 + (xr1 - xr0) * f - 1, y, shade)
        cv.line(xl1, y1, xr1, y1, shade)


def backdrop(cv, x0, x1, hz, terrain, mood, rng, hmax):
    """The far country behind a place: flat marsh, hills, forest or mountains, by the port's region."""
    w = x1 - x0 + 1
    dark = mood in ("night", "dusk", "storm")

    def ridge(height, c, f, jag=0.0, cap=None):
        p1, p2, p3 = rng.uniform(0, 6.28), rng.uniform(0, 6.28), rng.uniform(0, 6.28)
        for x in range(x0, x1 + 1):
            t = (x - x0) / float(w)
            v = 0.5 + 0.28 * math.sin(t * f * 6.28 + p1) + 0.16 * math.sin(t * f * 15.1 + p2) + 0.08 * math.sin(t * f * 37 + p3)
            if jag:
                v += jag * (((x * 7919) % 13) / 13.0 - 0.5)
            hh = int(max(0, height * v))
            cv.line(x, hz - hh, x, hz, c)
            if cap and hh > height * 0.62:
                cv.line(x, hz - hh, x, hz - hh + max(1, int((hh - height * 0.62) * 0.9)), cap)

    if terrain == "fjord":
        ridge(hmax, SLATE if dark else STONE, 1.6, 0.25, FROST if dark else SNOW)
        ridge(hmax * 0.42, WSHADE if dark else FOREST, 2.3, 0.35)
    elif terrain == "wood":
        ridge(hmax * 0.45, WSHADE if dark else FOREST, 2.0, 0.55)
    elif terrain == "moor":
        ridge(hmax * 0.6, WSHADE if dark else FOREST, 1.3)
        ridge(hmax * 0.3, FOREST if dark else GRASS, 2.1)
    elif terrain == "down":
        ridge(hmax * 0.4, FOREST if dark else GRASS, 1.2)
    elif terrain == "river":
        ridge(hmax * 0.5, FOREST if dark else GRASS, 1.5)
        ridge(hmax * 0.22, WSHADE if dark else FOREST, 3.0, 0.4)
    else:
        ridge(max(3, hmax * 0.1), FOREST if dark else GRASS, 3.0, 0.7)


def ground_cols(terrain, mood):
    if mood == "winter":
        return SNOW, FROST
    if mood == "night":
        return FOREST, WSHADE
    if mood in ("dusk", "storm"):
        return GRASS, FOREST
    return {"marsh": (MEADOW, REED), "river": (GRASS, MEADOW), "down": (MEADOW, GRASS), "moor": (GRASS, FOREST),
            "fjord": (GRASS, STONE), "wood": (GRASS, FOREST)}.get(terrain, (MEADOW, GRASS))


def ground(cv, x0, y0, x1, y1, g1, g2, rng, dense=160):
    if y1 < y0:
        return
    cv.rect(x0, y0, x1, y1, g1)
    for _ in range(max(1, (x1 - x0 + 1) * (y1 - y0 + 1) // dense)):
        x, y = rng.randint(x0, x1), rng.randint(y0, y1)
        cv.line(x, y, min(x1, x + rng.randint(1, 4)), y, g2)


def house(cv, x, yb, w, h, wall=SAND, wsh=DEARTH, roof=PSAND, rsh=SAND, door=True, lit=False):
    rh = max(3, int(h * 0.55))
    wh = h - rh
    cv.rect(x, yb - wh + 1, x + w - 1, yb, wall)
    for xx in range(x + 2, x + w - 1, 5):
        cv.line(xx, yb - wh + 2, xx, yb, wsh)
    ins = max(2, w // 8)
    trap(cv, x + ins, x + w - 1 - ins, yb - h + 1, x - 2, x + w + 1, yb - wh + 1, roof, rsh)
    if door:
        dw = max(1, w // 9)
        cv.rect(x + w // 2 - dw, yb - max(2, int(wh * 0.8)), x + w // 2 + dw, yb, GOLD if lit else PEAT)


def gable(cv, x, yb, w, h, wall=SAND, wsh=DEARTH, roof=PSAND, rsh=SAND, lit=False):
    """A house seen end on."""
    wh = int(h * 0.5)
    cv.rect(x, yb - wh + 1, x + w - 1, yb, wall)
    cv.poly([(x - 2, yb - wh + 1), (x + w // 2, yb - h + 1), (x + w + 1, yb - wh + 1)], wall)
    cv.line(x - 2, yb - wh + 1, x + w // 2, yb - h + 1, roof, 2); cv.line(x + w + 1, yb - wh + 1, x + w // 2, yb - h + 1, rsh, 2)
    dw = max(1, w // 6)
    cv.rect(x + w // 2 - dw, yb - int(wh * 0.85), x + w // 2 + dw, yb, GOLD if lit else PEAT)
    cv.line(x, yb - wh + 2, x, yb, wsh); cv.line(x + w - 1, yb - wh + 2, x + w - 1, yb, wsh)


def hall(cv, x, yb, w, h, roof=PSAND, rsh=SAND, wall=DEARTH, wsh=TIMBER, lit=False):
    """A long hall with a bowed roof."""
    wh = max(3, int(h * 0.3))
    cv.rect(x, yb - wh + 1, x + w - 1, yb, wall)
    for xx in range(x + 3, x + w - 1, 7):
        cv.line(xx, yb - wh + 1, xx, yb, wsh)
    pts = [(x - 3, yb - wh + 1)]
    for i in range(0, w + 1, max(1, w // 16)):
        t = i / float(w)
        pts.append((x + i, yb - wh - (h - wh) * (0.55 + 1.8 * t * (1 - t)) + 1))
    pts.append((x + w + 2, yb - wh + 1))
    cv.poly(pts, roof)
    for k in range(1, 6):
        yy = yb - wh - (h - wh) * k // 7
        cv.line(x + 4 + k * 2, yy, x + w - 5 - k * 2, yy, rsh)
    cv.line(x - 3, yb - wh + 1, x + w + 2, yb - wh + 1, rsh)
    dw = max(2, w // 18)
    cv.rect(x + w // 2 - dw, yb - wh + 1, x + w // 2 + dw, yb, GOLD if lit else PEAT)
    cv.line(x + w // 2 - dw - 1, yb - wh - 1, x + w // 2 - dw - 1, yb, wsh); cv.line(x + w // 2 + dw + 1, yb - wh - 1, x + w // 2 + dw + 1, yb, wsh)


def church(cv, x, yb, w, h, stone=True, cross=True, porch=False):
    """A small church of the time: a nave, a lower chancel to the east (right), a cross on the west gable."""
    wall, wsh = (PSTONE, STONE) if stone else (DEARTH, TIMBER)
    roof, rsh = (TIMBER, WEARTH) if stone else (PSAND, SAND)
    nave_w, nave_h = int(w * 0.68), int(h * 0.55)
    rh = h - nave_h
    ch = int(nave_h * 0.72)
    cv.rect(x + nave_w, yb - ch + 1, x + w - 1, yb, wall)
    trap(cv, x + nave_w - 1, x + w - 4, yb - ch - int(rh * 0.6) + 1, x + nave_w - 1, x + w + 1, yb - ch + 1, roof, rsh)
    cv.rect(x, yb - nave_h + 1, x + nave_w - 1, yb, wall)
    trap(cv, x + 4, x + nave_w - 5, yb - h + 1, x - 2, x + nave_w + 1, yb - nave_h + 1, roof, rsh)
    if stone:
        for yy in range(yb - nave_h + 4, yb, 4):
            for xx in range(x + 2 + (yy % 8), x + nave_w - 2, 9):
                cv.line(xx, yy, xx + 3, yy, wsh)
    else:
        for xx in range(x + 2, x + w - 1, 4):
            cv.line(xx, yb - ch + 2, xx, yb, wsh)
    nwin = max(2, nave_w // 18)
    for i in range(nwin):
        wx = x + 6 + i * (nave_w - 12) // nwin + 4
        cv.rect(wx, yb - nave_h + 4, wx + 1, yb - nave_h + 4 + max(3, nave_h // 4), NIGHT)
    dw = max(2, nave_w // 12)
    cv.rect(x + 5, yb - max(5, int(nave_h * 0.6)), x + 5 + dw * 2, yb, PEAT)
    cv.px(x + 5, yb - max(5, int(nave_h * 0.6)), wall); cv.px(x + 5 + dw * 2, yb - max(5, int(nave_h * 0.6)), wall)
    if porch:
        pw = max(8, w // 6)
        cv.rect(x - pw, yb - int(nave_h * 1.25), x - 1, yb, wall); cv.line(x - pw, yb - int(nave_h * 1.25), x - 1, yb - int(nave_h * 1.25), wsh)
        trap(cv, x - pw + 2, x - 3, yb - int(nave_h * 1.25) - pw // 2, x - pw - 1, x, yb - int(nave_h * 1.25), roof, rsh)
        cv.rect(x - pw // 2 - 1, yb - int(nave_h * 0.9), x - pw // 2, yb - int(nave_h * 0.6), NIGHT)
    if cross:
        cxp = x + 4 if not porch else x - max(8, w // 6) // 2
        cyp = yb - h + 1 if not porch else yb - int(nave_h * 1.25) - max(8, w // 6) // 2
        s = max(1, h // 30)
        cv.rect(cxp, cyp - 6 * s, cxp + s - 1, cyp, PGOLD); cv.rect(cxp - 2 * s, cyp - 4 * s, cxp + 3 * s - 1, cyp - 4 * s + s - 1, PGOLD)


def palisade(cv, x0, x1, yb, h, c=TIMBER, sh=WEARTH):
    for x in range(int(x0), int(x1) + 1, 3):
        cv.rect(x, yb - h, x + 1, yb, c); cv.px(x, yb - h - 1, c); cv.line(x + 2, yb - h + 1, x + 2, yb, sh)


def stone_wall(cv, x0, x1, yb, h, c=PSTONE, sh=STONE):
    cv.rect(x0, yb - h, x1, yb, c)
    for yy in range(yb - h + 3, yb, 4):
        cv.line(x0, yy, x1, yy, sh)
        for xx in range(int(x0) + (yy % 8), int(x1), 10):
            cv.line(xx, yy - 3, xx, yy, sh)
    for xx in range(int(x0), int(x1), 9):
        cv.rect(xx, yb - h - 3, xx + 4, yb - h, c)


def tree(cv, x, yb, h, kind="leaf", dark=False):
    c1, c2 = (WSHADE, FOREST) if dark else (FOREST, GRASS)
    if kind == "pine":
        cv.rect(x, yb - h // 4, x + 1, yb, WEARTH)
        for k in range(3):
            wy = yb - h // 4 - k * h // 4
            cv.poly([(x - h // 4 + k * 2, wy), (x + 1, wy - h // 3), (x + h // 4 + 1 - k * 2, wy)], WSHADE if dark else FOREST)
    else:
        cv.rect(x, yb - h // 2, x + max(1, h // 12), yb, WEARTH)
        cv.ell(x - h // 3, yb - h, x + h // 3 + 1, yb - h // 3, c1)
        cv.ell(x - h // 4, yb - h + 1, x + h // 10, yb - h // 2 - 1, c2)


def reeds(cv, x0, x1, yb, rng, c=REED, h=6):
    for x in range(int(x0), int(x1), 2):
        hh = rng.randint(h // 2, h)
        cv.line(x, yb - hh, x, yb, c if rng.random() < 0.7 else MEADOW)
        if rng.random() < 0.25:
            cv.px(x, yb - hh - 1, TIMBER)


def smoke(cv, x, y, h, c=PSTONE):
    for i in range(h):
        dx = int(round(math.sin(i / 3.0) * (1 + i / 8.0)))
        cv.rect(x + dx, y - i, x + dx + (1 if i > h // 3 else 0), y - i, c)


def fire(cv, x, yb, s=1):
    cv.poly([(x - 3 * s, yb), (x, yb - 7 * s), (x + 3 * s, yb)], RUST); cv.poly([(x - 1 * s, yb), (x, yb - 4 * s), (x + 2 * s, yb)], GOLD)
    cv.rect(x - 4 * s, yb, x + 4 * s, yb, WEARTH)


def person(cv, x, yb, s=1, cloak=SHOAL, hair=TIMBER, skin=PEACH, legs=WEARTH, hood=False, female=False):
    """A small standing figure, feet at (x, yb): 5 wide and 11 tall at s = 1."""
    cv.rect(x - 1 * s, yb - 11 * s + 1, x + 2 * s - 1, yb - 8 * s, skin)
    cv.rect(x - 1 * s, yb - 11 * s + 1, x + 2 * s - 1, yb - 10 * s, cloak if hood else hair)
    if hood:
        cv.rect(x - 2 * s, yb - 10 * s, x - 1 * s - 1, yb - 8 * s, cloak); cv.rect(x + 2 * s, yb - 10 * s, x + 3 * s - 1, yb - 8 * s, cloak)
    low = yb if female else yb - 3 * s
    cv.rect(x - 2 * s, yb - 8 * s + 1, x + 3 * s - 1, low, cloak)
    if not female:
        cv.rect(x - 1 * s, yb - 3 * s + 1, x - 1, yb, legs); cv.rect(x + 1 * s, yb - 3 * s + 1, x + 2 * s - 1, yb, legs)
    else:
        cv.rect(x - 1 * s, yb - 11 * s + 1, x + 2 * s - 1, yb - 10 * s, CWHITE)


CLOAKS = (SHOAL, RUST, MAUVE, DEARTH, STONE, MEADOW, BLOOD, SLATE, GOLD, SHALL, TIMBER, BYZ)
HAIRS = (TIMBER, PGOLD, PEAT, DEARTH, RUST, PSTONE)


def crowd(cv, x0, x1, yb, n, rng, s=1, jitter=3, cloaks=CLOAKS):
    for i in range(n):
        x = int(x0 + (x1 - x0) * (i + 0.5) / max(1, n)) + rng.randint(-jitter, jitter)
        person(cv, x, yb + rng.randint(-jitter, jitter), s, rng.choice(cloaks), rng.choice(HAIRS), hood=rng.random() < 0.2,
               female=rng.random() < 0.25)


def boat(cv, x, yw, L, mast=True, sail=False, hullc=TIMBER, stem=None, furled=True, shade=WEARTH):
    """A clinker boat from the side on the waterline yw, x its stern end."""
    L = int(L)
    hh = max(2, L // 9)
    st = max(1, L // 10) if stem is None else stem
    for i in range(L):
        u = abs(2.0 * i / max(1, L - 1) - 1.0)
        top = yw - hh - int(round(st * u ** 3))
        low = yw - int(round((hh + st) * max(0.0, (u - 0.88) / 0.12) ** 1.5))
        if low >= top:
            cv.line(x + i, top, x + i, low, hullc)
            cv.px(x + i, top, PEAT)
            if hh >= 4 and top + hh // 2 <= low:
                cv.px(x + i, top + hh // 2, shade)
    if mast:
        mx = x + L // 2
        mh = int(L * 0.62)
        cv.line(mx, yw - hh - mh, mx, yw - hh, WEARTH)
        if sail:
            sw_ = int(L * 0.6); sh_ = int(mh * 0.6)
            cv.rect(mx - sw_ // 2, yw - hh - mh + 2, mx + sw_ // 2, yw - hh - mh + 2 + sh_, PSAND)
            cv.box(mx - sw_ // 2, yw - hh - mh + 2, mx + sw_ // 2, yw - hh - mh + 2 + sh_, DEARTH)
            for xx in range(mx - sw_ // 2 + 4, mx + sw_ // 2, 5):
                cv.line(xx, yw - hh - mh + 3, xx, yw - hh - mh + 1 + sh_, SAND)
        elif furled:
            cv.line(mx - L // 4, yw - hh - mh + 3, mx + L // 4, yw - hh - mh + 3, WEARTH)
            cv.line(mx - L // 4 + 1, yw - hh - mh + 4, mx + L // 4 - 1, yw - hh - mh + 4, PSAND)


def beached(cv, x, yb, L, lean=1, hullc=TIMBER):
    """A boat drawn up on the shore, leaning on her bilge."""
    L = int(L)
    hh = max(3, L // 8)
    for i in range(L):
        u = abs(2.0 * i / max(1, L - 1) - 1.0)
        top = yb - hh - int(round(max(1, L // 10) * u ** 3)) + int(lean * (i - L / 2) * 0.06)
        low = yb - int(round(hh * max(0.0, (u - 0.75) / 0.25) ** 1.4))
        if low >= top:
            cv.line(x + i, top, x + i, low, hullc); cv.px(x + i, top, PEAT)
            if (top + low) // 2 > top:
                cv.px(x + i, (top + low) // 2, WEARTH)
    cv.line(x + L // 3, yb + 1, x + L // 3 - 3, yb - hh // 2, WEARTH)


def cask(cv, x, yb, s=1):
    cv.rect(x, yb - 7 * s, x + 5 * s, yb, TIMBER); cv.rect(x - s, yb - 5 * s, x + 6 * s, yb - 2 * s, TIMBER)
    cv.line(x - s, yb - 5 * s, x + 6 * s, yb - 5 * s, PEAT); cv.line(x - s, yb - 2 * s, x + 6 * s, yb - 2 * s, PEAT)
    cv.line(x, yb - 7 * s, x + 5 * s, yb - 7 * s, DEARTH)


def sack(cv, x, yb, s=1, c=SAND):
    cv.ell(x, yb - 6 * s, x + 7 * s, yb, c, DEARTH); cv.rect(x + 2 * s, yb - 8 * s, x + 4 * s, yb - 6 * s, c)


def stall(cv, x, yb, w, h, awning, rng):
    cv.line(x, yb - h, x, yb, TIMBER); cv.line(x + w, yb - h, x + w, yb, TIMBER)
    trap(cv, x + 2, x + w - 2, yb - h - max(3, h // 3), x - 2, x + w + 2, yb - h, awning, SHADE.get(awning))
    cv.rect(x + 1, yb - h // 2, x + w - 1, yb - h // 2 + 1, DEARTH)
    for i in range(max(2, w // 7)):
        gx = x + 3 + i * 7
        if gx < x + w - 3:
            cv.rect(gx, yb - h // 2 - 3, gx + 3, yb - h // 2 - 1, rng.choice((GOLD, DROSE, CWHITE, MEADOW, RUST, SHALL, PSAND)))


SHADE = {SAND: DEARTH, PSAND: SAND, MAUVE: BYZ, RUST: BLOOD, SHOAL: SEA, MEADOW: GRASS, GOLD: DEARTH, DROSE: RUST,
         PSTONE: STONE, DEARTH: TIMBER, SHALL: SHOAL, BLOOD: DPURP, CWHITE: PSTONE, LILAC: MAUVE, STONE: SLATE}


def jetty(cv, x0, x1, y, rng, deck=DEARTH):
    """A plank landing on posts, seen from the side."""
    cv.rect(x0, y, x1, y + 2, deck); cv.line(x0, y, x1, y, SAND); cv.line(x0, y + 3, x1, y + 3, PEAT)
    for x in range(int(x0) + 3, int(x1), 12):
        cv.rect(x, y + 3, x + 1, y + 12, WEARTH); cv.px(x, y - 1, WEARTH)


# ================================================================================================ scenes: one site of a port
class Ctx(object):
    pass


def _row(c, yb, x0, x1, n, hmin=14, hmax=22, lit=False):
    r = c.rng
    for i in range(n):
        w = r.randint(18, 34)
        x = int(x0 + (x1 - x0 - w) * (i + r.uniform(0.1, 0.9)) / max(1, n))
        if r.random() < 0.3:
            gable(c.cv, x, yb, w * 2 // 3, r.randint(hmin, hmax), c.wall, c.wsh, c.roof, c.rsh, lit)
        else:
            house(c.cv, x, yb, w, r.randint(hmin, hmax), c.wall, c.wsh, c.roof, c.rsh, True, lit)


def _water(c, y0, y1=None, calm=True):
    sea(c.cv, 0, y0, c.W - 1, c.H - 1 if y1 is None else y1, c.mood, c.rng, calm)


def s_quay(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, 98, 0, W, 7, 14, 24, c.lit)
    cv.rect(0, 99, W - 1, 126, DEARTH)
    for y in range(102, 126, 5):
        cv.line(0, y, W - 1, y, TIMBER)
    for x in range(8, W, 23):
        cv.line(x, 99, x + r.randint(-2, 2), 126, TIMBER)
    cv.rect(0, 127, W - 1, 142, WEARTH); cv.line(0, 127, W - 1, 127, SAND)
    for x in range(4, W, 14):
        cv.rect(x, 127, x + 2, 146, PEAT); cv.line(x + 3, 128, x + 3, 142, TIMBER)
    _water(c, 143)
    boat(cv, 150, 164, 140, True, False, TIMBER)
    cv.line(165, 152, 150, 127, SAND); cv.line(275, 152, 292, 127, SAND)
    for i in range(5):
        cask(cv, 14 + i * 11 + r.randint(0, 2), 120 - (i % 2) * 2, 2 if i % 2 else 1)
    for i in range(4):
        sack(cv, 84 + i * 10, 122 - (i % 2) * 3, 1, r.choice((SAND, PSAND, DEARTH)))
    cv.rect(300, 108, 303, 126, WEARTH); cv.rect(120, 110, 123, 126, WEARTH)
    crowd(cv, 130, 300, 122, 5, r, 2)


def s_beach(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    snow = c.mood == "winter"
    cv.rect(0, c.hz + 14, W - 1, H - 1, PSAND if not snow else SNOW)
    for _ in range(140):
        x, y = r.randint(0, W - 1), r.randint(c.hz + 14, H - 1)
        cv.line(x, y, x + r.randint(1, 5), y, SAND if not snow else FROST)
    for x in range(0, W, 3):                                  # dune grass along the back
        cv.line(x, c.hz + 14 - r.randint(0, 5), x, c.hz + 15, MEADOW if r.random() < 0.6 else GRASS)
    _row(c, c.hz + 12, 10, W - 10, 4, 10, 16, c.lit)
    cv.poly([(0, 150), (W - 1, 138), (W - 1, H - 1), (0, H - 1)], SEAS[c.mood][0])
    sea(cv, 0, 152, W - 1, H - 1, c.mood, r, True)
    cv.poly([(0, 150), (W - 1, 138), (W - 1, 140), (0, 152)], FOAM)
    beached(cv, 20, 138, 96, 1); beached(cv, 130, 128, 84, -1); beached(cv, 226, 124, 78, 1, WEARTH)
    cv.line(60, 100, 60, 136, WEARTH); cv.line(170, 96, 172, 126, WEARTH)
    for x in (112, 214, 300):
        cv.rect(x, 126, x + 1, 138, WEARTH)
    crowd(cv, 40, 290, 146, 4, r, 2)
    fire(cv, 206, 148, 1); smoke(cv, 206, 140, 26, PSTONE)


def s_market(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.rect(0, c.hz + 12, W - 1, H - 1, SAND if c.mood != "winter" else SNOW)
    for _ in range(160):
        x, y = r.randint(0, W - 1), r.randint(c.hz + 12, H - 1)
        cv.line(x, y, x + r.randint(1, 4), y, DEARTH if c.mood != "winter" else FROST)
    _row(c, c.hz + 16, 0, W, 7, 14, 22, c.lit)
    aw = [MAUVE, RUST, SHOAL, MEADOW, GOLD, DROSE, PSTONE]
    r.shuffle(aw)
    for i in range(5):
        stall(cv, 10 + i * 62 + r.randint(-3, 3), 124, 40, 16, aw[i % len(aw)], r)
    crowd(cv, 12, 308, 136, 9, r, 1, 3)
    for i in range(4):
        stall(cv, 16 + i * 78 + r.randint(-4, 4), 162, 54, 22, aw[(i + 3) % len(aw)], r)
    crowd(cv, 30, 300, 170, 6, r, 2, 2)
    cask(cv, 6, 170, 2); sack(cv, 296, 170, 2)


def s_toll(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 18, 120, W, 4, 14, 20, c.lit)
    cv.poly([(0, c.hz + 10), (110, c.hz + 10), (70, H - 1), (0, H - 1)], SEAS[c.mood][0])
    sea(cv, 0, c.hz + 30, 80, H - 1, c.mood, r, True)
    cv.poly([(110, c.hz + 10), (70, H - 1), (76, H - 1), (116, c.hz + 10)], DEARTH)
    boat(cv, 4, 150, 86, True, False)
    for i in range(3):
        cask(cv, 28 + i * 9, 143, 1)
    house(cv, 150, 132, 96, 46, c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
    cv.rect(118, 118, 121, 150, WEARTH); cv.rect(300, 118, 303, 150, WEARTH); cv.line(121, 124, 300, 124, TIMBER, 2)
    cv.rect(196, 146, 236, 148, DEARTH); cv.rect(198, 149, 200, 160, TIMBER); cv.rect(232, 149, 234, 160, TIMBER)
    cv.blit(glyph("scales", 2), 200, 114)
    person(cv, 180, 162, 3, BYZ, PSTONE)
    cv.blit(glyph("tally", 1, NIGHT), 162, 128)
    person(cv, 262, 164, 3, SHOAL, TIMBER, hood=True)
    cv.blit(glyph("coins", 1, NIGHT), 240, 134)


def s_mint(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 16, 0, 90, 2, 14, 20, c.lit); _row(c, c.hz + 16, 240, W, 2, 14, 20, c.lit)
    wall, wsh = (PSTONE, STONE) if c.stone else (c.wall, c.wsh)
    cv.rect(70, 84, 250, 150, wall); trap(cv, 86, 234, 52, 62, 258, 84, c.roof, c.rsh)
    cv.rect(96, 98, 224, 150, NIGHT)
    cv.dither(130, 104, 190, 150, RUST, 0, 3)
    fire(cv, 204, 148, 2); cv.rect(196, 148, 212, 150, SLATE)
    cv.rect(140, 134, 156, 150, TIMBER); cv.rect(144, 128, 152, 134, SLATE)
    person(cv, 166, 150, 3, DEARTH, TIMBER)
    cv.line(172, 122, 160, 110, TIMBER, 2); cv.rect(154, 104, 162, 110, SLATE)
    cv.rect(104, 138, 128, 140, DEARTH)
    for i in range(7):
        cv.rect(105 + i * 3, 136 - (i % 2), 106 + i * 3, 137 - (i % 2), GOLD if i % 3 else PSTONE)
    cv.blit(glyph("chest", 2, NIGHT), 24, 132)
    person(cv, 268, 162, 3, BLOOD, PGOLD); cv.line(278, 126, 278, 162, TIMBER); cv.poly([(276, 126), (278, 118), (280, 126)], PSTONE)


def s_church(c, big=False, abbey=False):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    if not c.christian:
        return s_shrine(c)
    _row(c, c.hz + 12, 0, 70, 2, 10, 16, c.lit)
    tree(cv, 290, 112, 46, "leaf", c.dark); tree(cv, 22, 120, 34, "leaf", c.dark)
    w_, h_ = (190, 82) if big else (150, 66)
    church(cv, (W - w_) // 2 + (16 if big else 0), 138, w_, h_, c.stone, True, big)
    if big:
        church(cv, 250, 132, 60, 30, c.stone, True, False)
        stone_wall(cv, 0, 110, 160, 10) if c.stone else palisade(cv, 0, 110, 160, 10)
        stone_wall(cv, 150, W - 1, 160, 10) if c.stone else palisade(cv, 150, W - 1, 160, 10)
    else:
        for x in range(6, W - 6, 9):
            cv.rect(x, 150, x, 158, TIMBER)
        cv.line(6, 153, W - 6, 153, TIMBER)
    for i in range(5):
        gx = 24 + i * 22 + r.randint(-3, 3)
        cv.rect(gx, 142, gx, 148, PSAND); cv.rect(gx - 1, 144, gx + 1, 144, PSAND)
    crowd(cv, 130, 300, 166, 3, r, 2, 2, (SLATE, STONE, DEARTH, SHOAL, PSTONE))


def s_minster(c):
    return s_church(c, True)


def s_abbey(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    if not c.christian:
        return s_shrine(c)
    for i in range(6):                                         # strip fields beyond
        cv.poly([(i * 60 - 20, c.hz + 4), (i * 60 + 30, c.hz + 4), (i * 60 + 10, c.hz + 22), (i * 60 - 50, c.hz + 22)], GOLD if i % 2 else MEADOW)
    church(cv, 104, 128, 130, 58, c.stone, True, False)
    house(cv, 20, 134, 80, 28, c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
    house(cv, 240, 132, 60, 24, c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
    for i in range(3):
        gable(cv, 250 + i * 22, 150, 14, 16, c.wall, c.wsh, c.roof, c.rsh)
    cv.arc(-40, 128, W + 40, 210, 180, 360, DEARTH, 5); cv.arc(-40, 130, W + 40, 212, 180, 360, GRASS if c.mood != "winter" else FROST, 2)
    cv.rect(64, 96, 66, 132, PSTONE); cv.rect(60, 102, 70, 104, PSTONE); cv.ell(61, 99, 69, 107, None, PSTONE)
    crowd(cv, 90, 260, 164, 5, r, 2, 2, (BASALT, SLATE, BASALT, STONE))
    smoke(cv, 60, 104, 18, PSTONE)


def s_hall(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 0, 80, 2, 12, 16, c.lit); _row(c, c.hz + 14, 250, W, 2, 12, 16, c.lit)
    hall(cv, 50, 142, 220, 76, c.roof, c.rsh, DEARTH, TIMBER, True)
    smoke(cv, 160, 60, 34, PSTONE)
    palisade(cv, 0, 44, 156, 12); palisade(cv, 276, W - 1, 156, 12)
    for x, k in ((140, BLOOD), (184, SHOAL)):
        person(cv, x, 160, 3, k, r.choice(HAIRS)); cv.line(x + 10, 122, x + 10, 160, TIMBER); cv.poly([(x + 8, 122), (x + 10, 114), (x + 12, 122)], PSTONE)
    cv.blit(glyph("shield", 1, NIGHT), 118, 136); cv.blit(glyph("shield", 1, NIGHT), 192, 136)
    crowd(cv, 220, 300, 168, 2, r, 2)


def s_thing(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.ell(-40, c.hz + 16, W + 40, H + 120, c.g2)
    cv.ell(-20, c.hz + 24, W + 20, H + 120, c.g1)
    tree(cv, 26, 112, 40, "leaf", c.dark); tree(cv, 296, 108, 34, "leaf", c.dark)
    cv.rect(156, 96, 163, 134, PSTONE); cv.line(156, 96, 156, 134, STONE); cv.rect(154, 134, 165, 136, STONE)
    n = 15
    for i in range(n):
        a = math.pi * (0.06 + 0.88 * i / (n - 1))
        x, y = 160 + int(128 * math.cos(a)), 124 + int(44 * math.sin(a))
        person(cv, x, y, 2, r.choice(CLOAKS), r.choice(HAIRS), hood=r.random() < 0.15)
        if i % 3 == 0:
            cv.line(x + 6, y - 34, x + 6, y, TIMBER); cv.poly([(x + 4, y - 34), (x + 6, y - 41), (x + 8, y - 34)], PSTONE)
    person(cv, 178, 134, 3, BYZ, PSTONE); cv.line(190, 96, 190, 134, TIMBER)


def s_smithy(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 0, W, 5, 12, 18, c.lit)
    for x in (70, 130, 190, 250):
        cv.rect(x, 96, x + 3, 152, WEARTH)
    cv.rect(72, 98, 252, 152, NIGHT)
    trap(cv, 90, 232, 62, 58, 264, 96, c.roof, c.rsh)
    cv.dither(150, 104, 250, 152, RUST, 0, 3); cv.dither(190, 112, 250, 152, GOLD, 1, 4)
    cv.rect(210, 132, 244, 152, STONE); cv.rect(214, 124, 240, 132, SLATE); fire(cv, 227, 126, 2)
    cv.blit(glyph("anvil", 3), 118, 108)
    person(cv, 182, 154, 3, WEARTH, PEAT); cv.line(190, 126, 176, 112, TIMBER, 2); cv.rect(168, 106, 178, 112, SLATE)
    smoke(cv, 228, 60, 40, SLATE); smoke(cv, 232, 58, 30, STONE)
    for i in range(4):
        cv.line(84 + i * 4, 150, 90 + i * 4, 116, SLATE)
    cv.rect(40, 150, 62, 160, TIMBER); cv.rect(42, 152, 60, 154, SHALL)


def s_shipyard(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 12, 0, 110, 3, 12, 16, c.lit)
    cv.rect(0, c.hz + 16, W - 1, H - 1, SAND if c.mood != "winter" else SNOW)
    sea(cv, 0, 156, W - 1, H - 1, c.mood, r, True); cv.line(0, 155, W - 1, 155, FOAM)
    x0, L, yb = 56, 210, 140
    cv.arc(x0, yb - 70, x0 + L, yb + 6, 20, 160, TIMBER, 3)                                  # the keel and stems
    cv.line(x0 + 10, yb - 8, x0 + 4, yb - 44, TIMBER, 2); cv.line(x0 + L - 10, yb - 8, x0 + L - 4, yb - 44, TIMBER, 2)
    for i in range(1, 12):
        x = x0 + i * L // 12
        u = abs(2.0 * i / 12 - 1.0)
        cv.line(x, yb - 2 - int(6 * (1 - u * u)) + 6, x, yb - 30 - int(10 * u), DEARTH, 2)
    for k in range(3):
        cv.arc(x0 + 4, yb - 60 - k * 6, x0 + L - 4, yb + 2 - k * 6, 25, 155, DEARTH, 2)
    for x in (x0 + 30, x0 + L // 2, x0 + L - 30):
        cv.line(x, yb + 4, x - 8, yb + 14, WEARTH, 2); cv.line(x, yb + 4, x + 8, yb + 14, WEARTH, 2)
    for i in range(4):
        cv.rect(8 + i * 3, 148 - i * 4, 52 + i * 3, 150 - i * 4, DEARTH if i % 2 else TIMBER)
    for _ in range(40):
        cv.px(r.randint(60, 270), r.randint(142, 153), PSAND)
    person(cv, 96, 152, 2, DEARTH, TIMBER); cv.blit(glyph("axe", 1, NIGHT), 100, 124)
    person(cv, 214, 150, 2, STONE, PGOLD); person(cv, 282, 152, 2, RUST, PEAT)
    cv.blit(glyph("tar", 1, NIGHT), 288, 134); fire(cv, 300, 152, 1)


def s_tavern(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 150, W, 4, 12, 18, True)
    house(cv, 16, 140, 150, 60, c.wall, c.wsh, c.roof, c.rsh, True, True)
    smoke(cv, 90, 80, 30, PSTONE)
    cv.rect(180, 142, 290, 145, DEARTH); cv.rect(184, 146, 187, 160, TIMBER); cv.rect(283, 146, 286, 160, TIMBER)
    cv.rect(176, 154, 296, 156, TIMBER)
    for i, x in enumerate((196, 222, 248, 272)):
        person(cv, x, 150, 2, r.choice(CLOAKS), r.choice(HAIRS)); cv.rect(x + 5, 138, x + 7, 141, PSAND)
    for i in range(3):
        cask(cv, 26 + i * 14, 160, 2)
    cv.blit(glyph("cup", 2, NIGHT), 126, 132)
    person(cv, 110, 160, 3, MAUVE, TIMBER, female=True)


def s_well(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 18, 0, W, 6, 16, 26, c.lit)
    cv.rect(0, 150, W - 1, H - 1, DEARTH if c.mood != "winter" else FROST)
    cv.dither(0, 146, W - 1, 150, DEARTH if c.mood != "winter" else FROST)
    cv.ell(128, 136, 196, 160, WEARTH); cv.ell(134, 134, 190, 152, TIMBER); cv.ell(140, 137, 184, 149, NIGHT)
    for x in range(136, 190, 6):
        cv.line(x, 142, x, 154, PEAT)
    cv.rect(206, 96, 209, 156, WEARTH); cv.line(120, 122, 250, 86, TIMBER, 2); cv.line(124, 121, 124, 140, SAND)
    cv.rect(120, 140, 128, 148, DEARTH); cv.rect(246, 84, 254, 92, STONE)
    person(cv, 100, 160, 3, MAUVE, TIMBER, female=True); person(cv, 236, 162, 3, SHOAL, PGOLD, female=True)
    cv.rect(84, 150, 92, 158, DEARTH); cv.rect(250, 152, 258, 160, DEARTH)


def s_gate(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 20, W - 20, 5, 10, 16, c.lit)
    if c.stone:
        stone_wall(cv, 0, 126, 138, 34); stone_wall(cv, 194, W - 1, 138, 34)
        cv.rect(118, 86, 134, 138, PSTONE); cv.rect(186, 86, 202, 138, PSTONE); cv.rect(118, 86, 202, 98, PSTONE)
        cv.line(118, 98, 202, 98, STONE)
    else:
        cv.rect(0, 126, W - 1, 138, c.g2); palisade(cv, 0, 124, 128, 26); palisade(cv, 196, W - 1, 128, 26)
        cv.rect(120, 84, 127, 138, WEARTH); cv.rect(193, 84, 200, 138, WEARTH); cv.rect(116, 84, 204, 91, TIMBER)
        cv.rect(120, 74, 200, 84, DEARTH); trap(cv, 126, 194, 64, 114, 206, 74, c.roof, c.rsh)
    cv.poly([(134, 138), (186, 138), (250, H - 1), (70, H - 1)], SAND if c.mood != "winter" else FROST)
    cv.rect(134, 100, 186, 138, c.g1 if not c.dark else WSHADE)
    person(cv, 110, 150, 3, BLOOD, PGOLD); cv.line(120, 112, 120, 150, TIMBER); cv.poly([(118, 112), (120, 104), (122, 112)], PSTONE)
    cv.blit(glyph("cart", 2, NIGHT), 150, 126)
    person(cv, 206, 160, 2, DEARTH, TIMBER, hood=True)


def s_wall(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 16, 0, W, 6, 14, 22, c.lit)
    if c.stone:
        stone_wall(cv, 0, W - 1, 132, 38)
        cv.rect(120, 80, 170, 132, PSTONE); cv.ell(120, 72, 170, 92, PSTONE); cv.line(120, 86, 120, 132, STONE); cv.line(170, 86, 170, 132, STONE)
        for yy in range(92, 132, 6):
            cv.line(122, yy, 168, yy, STONE)
        cv.poly([(230, 94), (250, 104), (262, 94), (262, 90), (230, 90)], SKIES[c.mood][-1])   # a breach, long fallen
        for _ in range(26):
            x = r.randint(0, W - 1); cv.line(x, 92, x, 92 - r.randint(1, 3), GRASS)
    else:
        cv.poly([(0, 132), (0, 112), (W - 1, 112), (W - 1, 132)], c.g2)
        cv.poly([(0, 114), (W - 1, 114), (W - 1, 110), (0, 110)], DEARTH)
        palisade(cv, 0, W - 1, 112, 22)
    cv.rect(0, 133, W - 1, 146, WEARTH if c.mood != "winter" else FROST); cv.dither(0, 133, W - 1, 146, PEAT, 0, 3)
    cv.rect(0, 140, W - 1, 144, SEAS[c.mood][0])
    person(cv, 60, 164, 3, STONE, TIMBER); person(cv, 270, 162, 2, DEARTH, PEAT, hood=True)


def s_ferry(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 12, 0, W, 4, 10, 14, c.lit)
    sea(cv, 0, c.hz + 20, W - 1, 152, c.mood, r, True)
    reeds(cv, 0, W - 1, c.hz + 22, r, REED, 6)
    cv.rect(0, 153, W - 1, H - 1, c.g1); cv.dither(0, 151, W - 1, 154, c.g1)
    cv.line(40, c.hz + 16, 280, 150, SAND)
    cv.rect(36, c.hz + 4, 38, c.hz + 20, WEARTH); cv.rect(280, 132, 283, 156, WEARTH)
    cv.poly([(110, 128), (220, 128), (210, 138), (120, 138)], TIMBER); cv.line(110, 128, 220, 128, PEAT); cv.line(120, 138, 210, 138, PEAT)
    person(cv, 128, 128, 2, DEARTH, TIMBER, hood=True); cv.line(134, 100, 126, 140, TIMBER)
    cv.blit(glyph("cart", 2, NIGHT), 150, 96)
    person(cv, 200, 128, 2, MAUVE, PGOLD, female=True)
    jetty(cv, 230, 300, 152, r)
    crowd(cv, 244, 296, 152, 3, r, 2, 2)


def s_shrine(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    tree(cv, 50, 130, 70, "leaf", c.dark); tree(cv, 270, 126, 60, "leaf", c.dark); tree(cv, 300, 136, 40, "pine", c.dark)
    cv.ell(90, 132, 230, 172, c.g2)
    if c.christian:
        cv.rect(156, 70, 163, 150, PSTONE); cv.rect(138, 86, 181, 93, PSTONE); cv.ell(146, 76, 173, 103, None, PSTONE)
        cv.ell(147, 77, 172, 102, None, STONE); cv.line(156, 70, 156, 150, STONE); cv.rect(148, 150, 171, 156, STONE)
        for yy in range(104, 148, 8):
            cv.line(158, yy, 161, yy + 3, STONE)
    else:
        for x, hh in ((140, 62), (160, 78), (182, 58)):
            cv.rect(x, 150 - hh, x + 7, 150, TIMBER); cv.line(x, 150 - hh, x, 150, WEARTH)
            cv.rect(x + 1, 150 - hh + 6, x + 2, 150 - hh + 7, NIGHT); cv.rect(x + 5, 150 - hh + 6, x + 6, 150 - hh + 7, NIGHT)
            cv.line(x + 2, 150 - hh + 12, x + 5, 150 - hh + 12, NIGHT); cv.rect(x - 1, 150 - hh - 2, x + 8, 150 - hh, WEARTH)
        cv.rect(150, 152, 178, 158, STONE); cv.blit(glyph("bowl", 1, NIGHT), 156, 138)
    for i in range(6):
        cv.rect(120 + i * 16 + r.randint(-3, 3), 160 + r.randint(-2, 3), 122 + i * 16, 162, r.choice((GOLD, CWHITE, RUST, LILAC)))
    person(cv, 214, 164, 3, r.choice(CLOAKS), r.choice(HAIRS), hood=True)


def s_grove(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.rect(0, c.hz - 30, W - 1, c.hz + 30, WSHADE)
    for i in range(9):
        x = 10 + i * 38 + r.randint(-6, 6)
        cv.rect(x, c.hz - 10, x + 5, 140, PEAT); cv.ell(x - 26, c.hz - 60 + r.randint(-6, 6), x + 32, c.hz + 10, WSHADE)
        cv.ell(x - 16, c.hz - 54, x + 14, c.hz - 20, FOREST)
    cv.ell(60, 118, 260, 176, c.g1); cv.ell(90, 126, 230, 170, MEADOW if not c.dark else GRASS)
    cv.rect(156, 96, 163, 146, TIMBER); cv.line(156, 96, 156, 146, WEARTH); cv.rect(154, 92, 165, 96, WEARTH)
    cv.rect(158, 104, 159, 105, NIGHT); cv.rect(161, 104, 162, 105, NIGHT)
    for i in range(7):
        x = 70 + i * 30 + r.randint(-5, 5); y = c.hz + r.randint(0, 20)
        cv.line(x, y, x, y + 8, SAND); cv.rect(x - 1, y + 8, x + 1, y + 10, r.choice((GOLD, CWHITE, RUST, PSTONE)))
    for x in (136, 180):
        tree(cv, x - 80 if x < 160 else x + 80, 150, 58, "leaf", True)
    cv.rect(148, 148, 172, 152, STONE)


def s_mound(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.ell(40, c.hz - 6, 200, c.hz + 120, c.g2); cv.ell(48, c.hz, 192, c.hz + 120, c.g1)
    cv.ell(190, c.hz + 10, 310, c.hz + 110, c.g2); cv.ell(196, c.hz + 15, 304, c.hz + 110, c.g1)
    cv.rect(0, 136, W - 1, H - 1, c.g1)
    for _ in range(120):
        x, y = r.randint(0, W - 1), r.randint(136, H - 1); cv.line(x, y, x + r.randint(1, 4), y, c.g2)
    for i in range(11):                                         # a setting of stones round the foot
        a = math.pi * i / 10.0
        x, y = 120 + int(92 * math.cos(a)), 146 + int(16 * math.sin(a))
        cv.rect(x - 2, y - 6, x + 2, y, PSTONE); cv.line(x - 2, y - 6, x - 2, y, STONE)
    cv.rect(118, c.hz - 16, 121, c.hz + 2, PSTONE)
    for x, y in ((250, 156), (270, 162), (232, 164)):
        cv.ell(x, y - 6, x + 12, y, CWHITE, PSTONE); cv.rect(x - 2, y - 6, x + 1, y - 3, PEAT); cv.rect(x + 2, y, x + 3, y + 3, PEAT)
        cv.rect(x + 8, y, x + 9, y + 3, PEAT)
    tree(cv, 300, 124, 30, "leaf", c.dark)


def s_graveyard(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    if c.christian:
        church(cv, 180, 122, 130, 54, c.stone, True)
        for row in range(3):
            for i in range(7):
                x, y = 20 + i * 24 + row * 8 + r.randint(-2, 2), 128 + row * 16
                if x < 176 or row > 0:
                    cv.ell(x - 5, y - 2, x + 9, y + 3, c.g2)
                    cv.rect(x + 1, y - 10, x + 2, y, PSAND if (i + row) % 3 else PSTONE); cv.rect(x - 1, y - 7, x + 4, y - 6, PSAND if (i + row) % 3 else PSTONE)
        cv.rect(150, 96, 155, 150, PSTONE); cv.rect(140, 108, 165, 113, PSTONE); cv.line(150, 96, 150, 150, STONE)
    else:
        for i in range(5):
            x, y = 30 + i * 60 + r.randint(-8, 8), 124 + (i % 2) * 22
            cv.ell(x - 24, y - 14, x + 24, y + 14, c.g2); cv.ell(x - 20, y - 12, x + 20, y + 12, c.g1)
            cv.rect(x - 1, y - 20, x + 1, y - 12, PSTONE)
        for i in range(14):                                     # stones set in the shape of a ship
            t = i / 13.0
            x = 90 + int(140 * t); dy = int(12 * math.sin(math.pi * t))
            cv.rect(x, 160 - dy - 3, x + 2, 160 - dy, PSTONE); cv.rect(x, 160 + dy - 3, x + 2, 160 + dy, PSTONE)
    for x in range(4, W - 4, 10):
        cv.rect(x, 164, x, 171, TIMBER)
    cv.line(4, 167, W - 5, 167, TIMBER)
    tree(cv, 16, 118, 40, "leaf", c.dark)


def s_workshop(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 0, W, 5, 12, 18, c.lit)
    for x in (50, 150, 262):
        cv.rect(x, 94, x + 3, 150, WEARTH)
    cv.rect(52, 96, 264, 150, PEAT); trap(cv, 70, 246, 60, 38, 278, 94, c.roof, c.rsh)
    cv.rect(70, 132, 240, 136, DEARTH); cv.rect(74, 137, 77, 150, TIMBER); cv.rect(234, 137, 237, 150, TIMBER)
    cv.blit(glyph("comb", 2, None), 84, 102); cv.blit(glyph("beads", 2, None), 196, 100)
    for i in range(6):
        x = 90 + i * 22
        cv.rect(x, 128, x + 6, 130, r.choice((WWHITE, GOLD, RUST, SHALL, PSAND)))
    person(cv, 158, 150, 3, DEARTH, PSTONE)
    for i in range(5):                                          # a heap of antler by the door
        x = 14 + i * 6
        cv.line(x, 162, x + 10, 146 - i * 2, WWHITE, 2); cv.line(x + 5, 154 - i, x + 12, 152 - i * 3, WWHITE)
    for _ in range(30):
        cv.px(r.randint(60, 260), r.randint(152, 160), PSAND)
    person(cv, 290, 162, 2, MAUVE, TIMBER, female=True)


def s_storehouse(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 12, 0, 60, 1, 12, 16, c.lit); _row(c, c.hz + 12, 270, W, 1, 12, 16, c.lit)
    cv.rect(60, 90, 260, 142, DEARTH)
    for x in range(64, 260, 8):
        cv.line(x, 90, x, 142, TIMBER)
    trap(cv, 80, 240, 54, 50, 270, 90, c.roof, c.rsh)
    for x in (64, 110, 160, 210, 252):
        cv.rect(x, 143, x + 4, 152, WEARTH)
    cv.rect(130, 100, 190, 142, NIGHT); cv.rect(124, 100, 130, 142, TIMBER); cv.rect(190, 100, 196, 142, TIMBER)
    for i in range(4):
        cask(cv, 136 + i * 13, 140, 2)
    cv.poly([(130, 143), (190, 143), (204, 160), (116, 160)], TIMBER); cv.line(116, 160, 204, 160, PEAT)
    for i in range(5):
        cask(cv, 20 + i * 15, 166 - (i % 2) * 3, 2)
    for i in range(5):
        sack(cv, 226 + i * 14, 168 - (i % 2) * 4, 2, r.choice((SAND, PSAND, DEARTH)))
    person(cv, 210, 160, 3, DEARTH, TIMBER, hood=True); sack(cv, 204, 134, 2, SAND)
    cv.blit(glyph("key", 1, NIGHT), 198, 108)


def s_field(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 10, 200, W, 3, 10, 14, c.lit)
    cols = [GOLD, MEADOW, DEARTH, PGOLD, GRASS, SAND] if c.mood != "winter" else [SNOW, FROST, CWHITE, FROST, SNOW, PSTONE]
    for i in range(8):
        xa, xb = -60 + i * 56, -4 + i * 56
        k = cols[i % len(cols)]
        cv.poly([(100 + i * 18, c.hz + 14), (118 + i * 18, c.hz + 14), (xb, H - 1), (xa, H - 1)], k)
        for j in range(1, 4):
            f = j / 4.0
            cv.line(100 + i * 18 + 18 * f, c.hz + 14, xa + (xb - xa) * f, H - 1, SHADE.get(k, DEARTH))
    for x in range(0, W, 4):
        cv.line(x, c.hz + 14 - r.randint(1, 4), x, c.hz + 14, FOREST)
    for x in (120, 146):                                        # the plough team
        cv.rect(x, 128, x + 20, 138, TIMBER if x == 120 else PSAND); cv.rect(x + 18, 124, x + 26, 132, TIMBER if x == 120 else PSAND)
        cv.rect(x + 2, 139, x + 3, 146, PEAT); cv.rect(x + 16, 139, x + 17, 146, PEAT); cv.line(x + 24, 122, x + 28, 118, WWHITE)
    cv.line(96, 132, 120, 132, WEARTH, 2); cv.line(96, 132, 88, 144, WEARTH, 2); cv.line(88, 144, 80, 122, WEARTH)
    person(cv, 76, 146, 3, DEARTH, TIMBER, hood=True)
    person(cv, 250, 158, 3, SAND, PGOLD); cv.blit(glyph("basket", 1, NIGHT), 258, 132)
    for _ in range(12):
        cv.px(r.randint(264, 290), r.randint(140, 160), GOLD)


def s_fishery(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.rect(0, c.hz + 10, W - 1, 118, PSAND if c.mood != "winter" else SNOW)
    _row(c, c.hz + 16, 0, 120, 2, 12, 16, c.lit)
    sea(cv, 0, 119, W - 1, H - 1, c.mood, r, True); cv.line(0, 118, W - 1, 118, FOAM)
    for i in range(16):                                         # a weir: two fences of stakes closing to a trap
        t = i / 15.0
        cv.rect(60 + int(100 * t), 126 + int(34 * t), 61 + int(100 * t), 134 + int(36 * t), WEARTH)
        cv.rect(260 - int(96 * t), 126 + int(34 * t), 261 - int(96 * t), 134 + int(36 * t), WEARTH)
    cv.line(60, 130, 160, 164, TIMBER); cv.line(260, 130, 164, 164, TIMBER)
    cv.ell(154, 158, 172, 172, SAND, DEARTH); cv.line(156, 162, 170, 168, DEARTH); cv.line(156, 168, 170, 162, DEARTH)
    for k in range(3):                                          # racks of fish drying
        x = 140 + k * 56
        cv.rect(x, 84, x + 1, 112, TIMBER); cv.rect(x + 40, 84, x + 41, 112, TIMBER); cv.line(x, 86, x + 41, 86, TIMBER); cv.line(x, 96, x + 41, 96, TIMBER)
        for i in range(8):
            cv.rect(x + 3 + i * 5, 87, x + 4 + i * 5, 92, SHALL if i % 2 else PSTONE); cv.rect(x + 3 + i * 5, 97, x + 4 + i * 5, 102, PSTONE if i % 2 else SHALL)
    boat(cv, 10, 150, 60, False)
    person(cv, 30, 144, 2, DEARTH, TIMBER, hood=True)
    cv.blit(glyph("net", 2, NIGHT), 96, 80)


def s_saltern(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    cv.rect(0, c.hz + 8, W - 1, H - 1, SAND if c.mood != "winter" else FROST)
    sea(cv, 0, c.hz + 2, W - 1, c.hz + 14, c.mood, r, True)
    for row in range(3):
        for i in range(5):
            x, y = 12 + i * 44 + row * 10, 110 + row * 20
            cv.poly([(x, y), (x + 36, y), (x + 40, y + 14), (x - 4, y + 14)], WAVE if (i + row) % 3 else FOAM)
            cv.poly([(x, y), (x + 36, y), (x + 40, y + 14), (x - 4, y + 14)], None, DEARTH)
            if (i + row) % 3 == 0:
                cv.dither(x, y + 2, x + 34, y + 12, CWHITE)
    house(cv, 226, 108, 70, 30, c.wall, c.wsh, c.roof, c.rsh, True, True)
    smoke(cv, 262, 78, 36, PSTONE); smoke(cv, 266, 76, 28, CWHITE)
    for x in (238, 262, 286):
        cv.poly([(x - 10, 168), (x, 150), (x + 10, 168)], CWHITE, PSTONE)
    person(cv, 120, 150, 2, DEARTH, TIMBER, hood=True); cv.line(128, 126, 140, 150, TIMBER); cv.line(136, 150, 146, 150, TIMBER)
    person(cv, 200, 162, 2, STONE, PEAT)


def s_quarry(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    dark_rock = c.port.get("region") in ("rhineland",)
    rock, rsh = (SLATE, BASALT) if dark_rock else (STONE, SLATE)
    cv.poly([(100, 28), (W - 1, 14), (W - 1, 150), (60, 150), (80, 110), (70, 70)], rock)
    for yy in range(40, 150, 18):
        cv.line(76 + (yy % 30), yy, W - 1, yy - 4, rsh)
    for xx in range(110, W, 30):
        cv.line(xx, 30, xx + r.randint(-6, 6), 150, rsh)
    cv.rect(0, 150, W - 1, H - 1, DEARTH if c.mood != "winter" else FROST)
    for i in range(5):
        x = 120 + i * 36
        if dark_rock:
            cv.ell(x, 148, x + 24, 162, SLATE, NIGHT); cv.ell(x + 9, 153, x + 15, 157, NIGHT)
        else:
            cv.rect(x, 146, x + 22, 160, PSTONE); cv.box(x, 146, x + 22, 160, STONE)
    person(cv, 100, 150, 3, DEARTH, TIMBER); cv.line(108, 120, 122, 110, TIMBER, 2); cv.rect(120, 106, 126, 110, SLATE)
    person(cv, 40, 166, 3, STONE, PEAT, hood=True)
    cv.rect(16, 162, 70, 166, TIMBER); cv.rect(30, 152, 56, 162, rock)
    tree(cv, 30, 100, 40, "leaf", c.dark)


def s_bridge(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 10, 0, W, 5, 10, 14, c.lit)
    sea(cv, 0, c.hz + 16, W - 1, H - 1, c.mood, r, True)
    cv.poly([(0, c.hz + 14), (50, c.hz + 14), (30, H - 1), (0, H - 1)], c.g1); cv.poly([(W - 1, c.hz + 14), (270, c.hz + 14), (292, H - 1), (W - 1, H - 1)], c.g1)
    cv.rect(20, 112, 300, 118, TIMBER); cv.line(20, 112, 300, 112, SAND); cv.line(20, 119, 300, 119, PEAT)
    for x in range(40, 290, 28):
        cv.rect(x, 119, x + 3, 160, WEARTH); cv.line(x - 8, 150, x + 3, 122, WEARTH); cv.line(x + 12, 150, x, 122, WEARTH)
    cv.line(20, 100, 300, 100, TIMBER)
    for x in range(22, 300, 14):
        cv.line(x, 100, x, 112, TIMBER)
    cv.blit(glyph("cart", 2, NIGHT), 130, 82)
    person(cv, 110, 112, 2, DEARTH, TIMBER, hood=True); person(cv, 220, 112, 2, MAUVE, PGOLD, female=True)
    boat(cv, 150, 164, 70, False)


def s_ford(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 10, 0, 120, 2, 10, 14, c.lit)
    tree(cv, 280, c.hz + 16, 36, "leaf", c.dark); tree(cv, 250, c.hz + 14, 26, "leaf", c.dark)
    cv.rect(0, c.hz + 18, W - 1, 150, SHALL if c.mood not in ("night", "dusk") else SEA)
    for _ in range(26):
        x, y = r.randint(0, W - 20), r.randint(c.hz + 22, 146)
        cv.ell(x, y, x + r.randint(10, 30), y + r.randint(2, 4), PSAND if c.mood not in ("night", "dusk") else SHOAL)
    for _ in range(60):
        x, y = r.randint(0, W - 1), r.randint(c.hz + 20, 148); cv.line(x, y, x + 3, y, WAVE)
    cv.rect(0, 151, W - 1, H - 1, c.g1); cv.poly([(110, 151), (200, 151), (240, H - 1), (60, H - 1)], SAND if c.mood != "winter" else FROST)
    cv.poly([(140, c.hz + 18), (176, c.hz + 18), (170, c.hz + 8), (150, c.hz + 8)], SAND if c.mood != "winter" else FROST)
    for x, y in ((120, 110), (196, 112), (126, 140), (204, 142)):
        cv.rect(x, y - 14, x + 1, y, WEARTH)
    cv.blit(glyph("cart", 3, NIGHT), 134, 90)
    person(cv, 124, 134, 2, DEARTH, TIMBER, hood=True); person(cv, 210, 130, 2, MAUVE, PEAT, female=True)


def s_lodging(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 14, 170, W, 3, 12, 18, True)
    house(cv, 30, 144, 170, 64, c.wall, c.wsh, c.roof, c.rsh, True, True)
    smoke(cv, 110, 80, 30, PSTONE)
    cv.rect(214, 136, 290, 139, TIMBER); cv.rect(218, 140, 220, 150, TIMBER); cv.rect(284, 140, 286, 150, TIMBER)
    cv.blit(glyph("bed", 2, NIGHT), 50, 110)
    person(cv, 150, 160, 3, DEARTH, PSTONE); person(cv, 236, 164, 3, SHOAL, TIMBER, hood=True)
    sack(cv, 250, 164, 2, SAND); cv.line(246, 124, 246, 164, TIMBER)
    cask(cv, 12, 160, 2)


def s_generic(c):
    cv, r, W, H = c.cv, c.rng, c.W, c.H
    _row(c, c.hz + 16, 0, W, 6, 14, 24, c.lit)
    _row(c, 150, 20, W - 20, 3, 30, 44, c.lit)
    crowd(cv, 30, 290, 166, 4, r, 2)


SCENES = {"quay": s_quay, "beach": s_beach, "market": s_market, "toll": s_toll, "mint": s_mint, "church": s_church,
          "minster": s_minster, "abbey": s_abbey, "hall": s_hall, "thing": s_thing, "smithy": s_smithy,
          "shipyard": s_shipyard, "tavern": s_tavern, "well": s_well, "gate": s_gate, "wall": s_wall, "ferry": s_ferry,
          "shrine": s_shrine, "grove": s_grove, "mound": s_mound, "graveyard": s_graveyard, "workshop": s_workshop,
          "storehouse": s_storehouse, "field": s_field, "fishery": s_fishery, "saltern": s_saltern, "quarry": s_quarry,
          "bridge": s_bridge, "ford": s_ford, "lodging": s_lodging}
STONE_KINDS = ("town",)


def _ctx(cv, ident, text, port, W, H, hz, default_mood=None):
    c = Ctx()
    c.cv, c.W, c.H, c.hz = cv, W, H, hz
    c.rng = rng_of("scene:" + str(ident))
    c.mood, c.mood_given = mood_of(text, c.rng, default_mood)
    c.port = port or {}
    c.terrain = TERRAIN.get(c.port.get("region") or "", "marsh")
    c.christian = (c.port.get("faith") or "christian") == "christian"
    c.dark = c.mood in ("night", "dusk", "storm")
    c.lit = c.mood in ("night", "dusk")
    c.stone = has_word(text, "stone wall", "roman wall", "stone church", "of stone", "roman") or (c.port.get("kind") in STONE_KINDS and not has_word(text, "timber", "wooden", "wattle"))
    c.g1, c.g2 = ground_cols(c.terrain, c.mood)
    if c.terrain == "fjord" or has_word(text, "turf roof", "turf-roofed"):
        c.roof, c.rsh = GRASS, FOREST
    elif c.mood == "winter":
        c.roof, c.rsh = SNOW, FROST
    else:
        c.roof, c.rsh = PSAND, SAND
    c.wall, c.wsh = (DEARTH, TIMBER) if c.terrain in ("fjord", "wood") else (SAND, DEARTH)
    return c


def draw_scene(site, port, w=320, h=180):
    """One site of a port. The kind of site decides what stands in the picture; the port's country and landing, the
    faith of the place and the plain words of 'see' (dusk, fog, snow, stone, timber) decide the rest."""
    cv = Cv(w, h)
    big = Cv(320, 180)
    H = 173
    text = " ".join(str(site.get(k) or "") for k in ("see", "name"))
    c = _ctx(big, site.get("id"), text, port, 320, H, 78)
    sky(big, 0, 0, 319, c.hz, c.mood, c.rng)
    backdrop(big, 0, 319, c.hz, c.terrain, c.mood, c.rng, 46)
    ground(big, 0, c.hz + 1, 319, H - 1, c.g1, c.g2, c.rng)
    SCENES.get((site.get("kind") or "").lower(), s_generic)(c)
    weather(big, 0, 0, 319, H - 1, c.mood, c.rng)
    label_strip(big, "%s: %s" % (c.port.get("name") or c.port.get("id") or "", site.get("name") or site.get("kind") or ""), 7)
    if (w, h) == (320, 180):
        cv = big
    else:
        cv.blit(big, 0, 0)
    placeholder_mark(cv)
    return cv


# ================================================================================================ a port from the water
def draw_harbour(port, text="", w=640, h=180):
    """A port as she is seen coming in: the far country by her region, the town by her kind and size, the shore by
    her landing (beach, jetty, quay, riverbank), ships by her size."""
    cv = Cv(w, h)
    H = h - 7
    c = _ctx(cv, "harbour:" + str(port.get("id")), text, port, w, H, 62)
    r = c.rng
    kind = (port.get("kind") or "").lower()
    landing = (port.get("landing") or "beach").lower()
    size = int(max(1, min(5, num(port.get("size"), 2))))
    shore = 104
    sky(cv, 0, 0, w - 1, c.hz, c.mood, r)
    backdrop(cv, 0, w - 1, c.hz, c.terrain, c.mood, r, 44)
    ground(cv, 0, c.hz + 1, w - 1, shore, c.g1, c.g2, r)
    sea(cv, 0, shore + 1, w - 1, H - 1, c.mood, r, landing == "riverbank")
    span0, span1 = w // 2 - (60 + size * 46), w // 2 + (60 + size * 46)
    yb = shore - 4
    if kind == "fort":
        cv.poly([(w // 2 - 120, shore), (w // 2 - 80, shore - 40), (w // 2 + 70, shore - 46), (w // 2 + 130, shore)], STONE)
        cv.dither(w // 2 - 100, shore - 40, w // 2 + 100, shore, SLATE, 0, 3)
        palisade(cv, w // 2 - 78, w // 2 + 68, shore - 44, 12)
        hall(cv, w // 2 - 40, shore - 46, 80, 26, c.roof, c.rsh, DEARTH, TIMBER, c.lit)
    elif kind == "royal seat":
        cv.ell(w // 2 - 150, shore - 26, w // 2 + 150, shore + 30, c.g2); cv.rect(w // 2 - 150, shore + 1, w // 2 + 150, shore + 30, SEAS[c.mood][0])
        hall(cv, w // 2 - 70, shore - 16, 140, 42, c.roof, c.rsh, DEARTH, TIMBER, c.lit)
        palisade(cv, w // 2 - 130, w // 2 - 76, shore - 12, 10); palisade(cv, w // 2 + 76, w // 2 + 130, shore - 12, 10)
        smoke(cv, w // 2, shore - 58, 24, PSTONE)
    elif kind == "monastery":
        church(cv, w // 2 - 70, yb, 120, 50, True, True)
        for i in range(4 + size):
            gable(cv, w // 2 + 60 + i * 20, yb, 13, 14, c.wall, c.wsh, c.roof, c.rsh, c.lit)
        house(cv, w // 2 - 150, yb, 70, 22, c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
        cv.rect(w // 2 - 96, yb - 40, w // 2 - 94, yb, PSTONE); cv.rect(w // 2 - 101, yb - 34, w // 2 - 89, yb - 32, PSTONE)
    n = 2 + size * 4 if kind not in ("fort", "royal seat", "monastery") else size * 2
    if kind == "town":
        stone_wall(cv, span0, span1, yb + 2, 16)
    for i in range(n):
        ww = r.randint(18, 34)
        x = int(span0 + (span1 - span0 - ww) * (i + r.uniform(0.1, 0.9)) / max(1, n))
        if kind in ("fort", "royal seat", "monastery") and abs(x - w // 2) < 150:
            continue
        up = 14 if kind == "town" else 0
        if r.random() < 0.35:
            gable(cv, x, yb - up, ww * 2 // 3, r.randint(14, 22), c.wall, c.wsh, c.roof, c.rsh, c.lit)
        else:
            house(cv, x, yb - up, ww, r.randint(13, 20), c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
    if c.christian and kind in ("emporium", "town", "market", "village") and (size >= 3 or kind == "town"):
        church(cv, w // 2 + r.randint(-80, 40), yb - (16 if kind == "town" else 2), 70 if kind == "town" else 52,
               34 if kind == "town" else 26, kind == "town" or c.stone, True)
    if kind == "market":
        for i in range(3 + size):
            stall(cv, span0 + 20 + i * 44, yb + 2, 26, 12, r.choice((MAUVE, RUST, SHOAL, MEADOW, GOLD)), r)
    if kind in ("emporium", "town", "market") and size >= 4:
        for i in range(size):
            smoke(cv, span0 + r.randint(20, span1 - span0 - 20), yb - 26, r.randint(10, 20), PSTONE)
    # the shore, by the landing
    ships = size + (1 if kind == "emporium" else 0)
    if landing == "beach":
        cv.rect(0, shore - 3, w - 1, shore + 5, PSAND if c.mood != "winter" else SNOW); cv.line(0, shore + 6, w - 1, shore + 6, FOAM)
        for i in range(ships + 1):
            beached(cv, span0 + 10 + i * (span1 - span0 - 60) // max(1, ships), shore + 4, r.randint(34, 56), r.choice((-1, 1)))
    elif landing == "jetty":
        cv.line(0, shore + 1, w - 1, shore + 1, FOAM)
        for i in range(2 + size):
            x = span0 + 14 + i * (span1 - span0 - 28) // (1 + size)
            cv.poly([(x - 5, shore - 2), (x + 5, shore - 2), (x + 10, shore + 36), (x - 10, shore + 36)], DEARTH)
            for yy in range(shore, shore + 36, 4):
                cv.line(x - 5 - (yy - shore) // 8, yy, x + 5 + (yy - shore) // 8, yy, TIMBER)
            cv.rect(x - 11, shore + 30, x - 10, shore + 42, WEARTH); cv.rect(x + 10, shore + 30, x + 11, shore + 42, WEARTH)
            if i % 2 == 0:
                boat(cv, x + 14, shore + 30, r.randint(36, 54), True, False)
    elif landing == "quay":
        cv.rect(span0, shore - 4, span1, shore + 6, DEARTH); cv.line(span0, shore - 4, span1, shore - 4, SAND)
        for x in range(span0, span1, 10):
            cv.rect(x, shore - 3, x + 1, shore + 10, WEARTH)
        for i in range(ships):
            boat(cv, span0 + 12 + i * (span1 - span0 - 70) // max(1, ships), shore + 16, r.randint(44, 66), True, False)
    else:                                                    # a river bank
        reeds(cv, 0, span0 + 30, shore + 2, r, REED if c.mood != "winter" else PSAND, 8)
        reeds(cv, span1 - 30, w - 1, shore + 2, r, REED if c.mood != "winter" else PSAND, 8)
        cv.line(0, shore + 3, w - 1, shore + 3, WEARTH)
        for i in range(ships + 1):
            boat(cv, span0 + 24 + i * (span1 - span0 - 80) // max(1, ships), shore + 10, r.randint(30, 50), r.random() < 0.6, False,
                 r.choice((TIMBER, DEARTH)))
    for i in range(max(1, size - 1)):                          # ships lying off or standing in
        boat(cv, r.randint(20, w - 110), r.randint(shore + 46, H - 8), r.randint(46, 80), True, r.random() < 0.6)
    # your own stem in the corner: she is seen from the boat
    cv.poly([(0, H - 1), (0, H - 34), (10, H - 50), (16, H - 50), (30, H - 14), (70, H - 1)], TIMBER)
    cv.line(10, H - 50, 30, H - 14, PEAT); cv.line(0, H - 22, 44, H - 6, WEARTH); cv.line(0, H - 12, 30, H - 2, WEARTH)
    weather(cv, 0, 0, w - 1, H - 1, c.mood, r)
    label_strip(cv, "%s (%s) %s, %s" % (port.get("name") or port.get("id") or "", port.get("modern") or "", kind, landing), 7)
    placeholder_mark(cv)
    return cv


# ================================================================================================ cards, stories, endings, plates
ABOUT = [  # the plain words of a card and the sign each brings into its picture
    (r"toll|tenth|teloneum", "tally"), (r"charter|writ\b|seal", "letter"), (r"riddle", "runestick"),
    (r"letter|gospel|psalter|parchment|book", "book"), (r"weigh|scales|hacksilver|short weight", "scales"),
    (r"silver|penn|denier|dirham|coin|purse|debt|hoard|tribute|ransom|pounds", "coins"),
    (r"wine|cask|ale\b|mead|barrel", "cask"), (r"sword|blade|byrnie|arms\b|weapon", "sword"),
    (r"raid|heathen|northmen|wicing|pirate|long ?ship|fleet|army", "raider"), (r"fire|burn|sack(ed|ing)?\b|ashes", "fire"),
    (r"wreck|aground|sandbank|\bbar\b|shoal|strand", "wreck"), (r"thor|odin|woden|blot|grove|old gods|idol", "thor"),
    (r"church|mass\b|priest|bishop|abbot|monk|saint|relic|altar|minster|abbey|bapti", "cross"),
    (r"bell", "bell"), (r"oath|sworn|swear|arm-ring", "ring"), (r"king|emperor|court\b|count\b|reeve|lord", "circlet"),
    (r"thing|law\b|judg|lawsuit|witness|inquiry", "staff"), (r"feast|wedding|marriage|bride|horn", "horn"),
    (r"sick|fever|wound|leech|plague|sickness", "herb"), (r"dead|death|grave|bury|burial|mound|barrow|killed", "mound"),
    (r"fish|herring|eel|whale", "fish"), (r"cloth|cloak|wool|silk|loom|weav", "bolt"), (r"quern|black stones", "quern"),
    (r"amber|bead", "beads"), (r"walrus|ivory|tusk", "tusk"), (r"fur|pelt|marten|skins", "fur"),
    (r"captive|slave|hostage|sold", "chain"), (r"pilgrim|rome", "staff"), (r"wright|launch|keel|strake|leak|hull", "hull"),
    (r"wind|becalm|calm", "wind"), (r"tide|ebb|flood", "tide"), (r"news|rumour|word of|message", "speech"),
    (r"grain|wheat|bread|famine|hunger|harvest|corn", "sheaf"), (r"salt", "salt"), (r"dice|tafl|game|wager", "dice"),
    (r"song|harp|sing|skald|lay\b|lyre", "lyre"), (r"horse|cart|stallion", "cart"), (r"hawk|falcon|crane", "bird"),
    (r"market|fair\b|stall|booth", "stall"), (r"thie(f|ves)|theft|steal|stolen", "purse"), (r"passenger|passage", "person"),
    (r"lookout|watch|sail to|a sail", "eye"), (r"star|comet|portent|omen|eclipse", "sun"), (r"rope|rigging|sail\b", "sail"),
    (r"water|casks spoil|thirst", "cask"), (r"grumbl|pay\b|wage|share", "coins"), (r"heir|son\b|daughter|child|kin", "person"),
]


def about_signs(texts, n=2):
    """The signs a picture should carry, from its words: the title counts most, then the picture line, then the rest."""
    score = {}
    for wgt, t in texts:
        t = (t or "").lower()
        for i, (pat, g) in enumerate(ABOUT):
            m = re.search(r"(?<![a-z])(" + pat + ")", t)
            if m:
                s = wgt * 100 - m.start() * 0.01 - i * 0.001
                if s > score.get(g, -1e9):
                    score[g] = s
    return [g for g, _ in sorted(score.items(), key=lambda kv: -kv[1])[:n]]


ROLE_CLOAK = {"king": GOLD, "count": BYZ, "reeve": BYZ, "tollman": BYZ, "steward": MAUVE, "kings_man": BYZ, "lords_man": BLOOD,
              "chief": RUST, "merchant": SHOAL, "skipper": SHOAL, "master": SHOAL, "raider": BLOOD, "raider_chief": BLOOD,
              "warrior": BLOOD, "beggar": WEARTH, "captive": STONE, "pilgrim": DEARTH, "widow": SLATE}
WOMEN = {"abbess", "nun", "weaver", "farm_wife", "alewife", "widow", "wise_woman"}


def vignette(ident, stage, texts, roles=(), port=None, w=192, h=108, signs=None, title=None, frame=None, strip=True, mark=True):
    """A card's picture: the stage sets the place (harbour: a quay; sea: open water and your boat; crew: aboard, looking
    forward; long: a hall by firelight), the people of the card stand in it, and two large signs say what it is about."""
    cv = Cv(w, h)
    H = h - (7 if strip else 0)
    k = max(1, int(round(w / 192.0)))                            # how much bigger than a card
    alltext = " ".join(t for _, t in texts if t)
    c = _ctx(cv, "card:" + str(ident), alltext, port, w, H, int(H * 0.42),
             "dusk" if stage == "long" else None)
    r = c.rng
    signs = signs if signs is not None else about_signs(texts)
    if stage == "sea" and c.mood == "day" and "storm" in signs:
        c.mood = "storm"
    if "fog" in alltext.lower() and c.mood == "day":
        c.mood = "fog"
    stage = stage if stage in ("harbour", "sea", "crew", "long") else "harbour"
    fs = 2 * k + (1 if w >= 300 and k == 1 else 0)                # figure scale
    gs = 2 * k                                                   # sign scale
    if stage == "long":
        cv.rect(0, 0, w - 1, H - 1, PEAT)
        for x in range(w // 10, w, w // 5):
            cv.rect(x, 0, x + 2 * k, H - 1, WEARTH)
        cv.rect(0, 0, w - 1, 4 * k, WEARTH); cv.rect(0, int(H * 0.72), w - 1, H - 1, WEARTH)
        cv.dither(0, int(H * 0.72), w - 1, H - 1, PEAT, 0, 3)
        fx, fy = w // 2, int(H * 0.86)
        cv.dither(fx - 40 * k, fy - 34 * k, fx + 40 * k, fy + 6 * k, RUST, 0, 3); cv.dither(fx - 22 * k, fy - 22 * k, fx + 22 * k, fy + 4 * k, GOLD, 1, 3)
        fire(cv, fx, fy, 2 * k)
        cv.rect(4 * k, int(H * 0.66), w // 2 - 44 * k, int(H * 0.69), DEARTH); cv.rect(w // 2 + 44 * k, int(H * 0.66), w - 4 * k, int(H * 0.69), DEARTH)
        base = int(H * 0.7)
        cv.rect(w - 30 * k, 10 * k, w - 14 * k, 34 * k, SKIES["winter" if c.mood == "winter" else "night"][1])
        cv.box(w - 31 * k, 9 * k, w - 13 * k, 35 * k, TIMBER)
    elif stage == "sea":
        sky(cv, 0, 0, w - 1, c.hz, c.mood, r)
        sea(cv, 0, c.hz + 1, w - 1, H - 1, c.mood, r, "wind" in signs or "becalm" in alltext.lower())
        if "raider" in signs or "eye" in signs:
            cv.blit(glyph("raider", k, None), int(w * 0.78), c.hz - 12 * k)
        bx, by = int(w * 0.2), int(H * 0.74)
        boat(cv, bx, by, int(w * 0.38), True, "wreck" not in signs, TIMBER)
        for i in range(4):
            hx = bx + int(w * 0.38 * (0.2 + 0.18 * i))
            cv.rect(hx, by - int(w * 0.38) // 9 - 3 * k, hx + 2 * k - 1, by - int(w * 0.38) // 9 - 1, PEACH)
        base = H - 3 * k
        if "wreck" in signs:
            for i in range(5):
                x = int(w * 0.6) + i * 12 * k
                cv.poly([(x, H - 1), (x + 6 * k, H - 16 * k - (i % 2) * 8 * k), (x + 14 * k, H - 1)], SLATE)
            cv.line(int(w * 0.58), H - 12 * k, w - 1, H - 14 * k, FOAM)
    elif stage == "crew":
        sky(cv, 0, 0, w - 1, c.hz, c.mood, r)
        sea(cv, 0, c.hz + 1, w - 1, H - 1, c.mood, r)
        cv.poly([(w // 2 - 8 * k, c.hz - 6 * k), (w // 2 + 8 * k, c.hz - 6 * k), (w - 10 * k, H - 1), (10 * k, H - 1)], TIMBER)
        cv.poly([(w // 2 - 5 * k, c.hz - 2 * k), (w // 2 + 5 * k, c.hz - 2 * k), (w - 26 * k, H - 1), (26 * k, H - 1)], DEARTH)
        cv.line(w // 2, c.hz - 6 * k, w // 2, c.hz - 20 * k, WEARTH, k)
        for i in range(1, 5):
            y = c.hz + (H - c.hz) * i // 5
            half = 5 * k + (w // 2 - 26 * k - 5 * k) * (y - c.hz + 2 * k) // max(1, H - c.hz + 2 * k)
            cv.rect(w // 2 - half, y, w // 2 + half, y + k, TIMBER)
        cv.rect(w // 2 - k, 2 * k, w // 2 + k, int(H * 0.7), WEARTH)
        cv.rect(w // 2 - 30 * k, 4 * k, w // 2 + 30 * k, int(H * 0.36), PSAND); cv.box(w // 2 - 30 * k, 4 * k, w // 2 + 30 * k, int(H * 0.36), DEARTH)
        base = H - 2 * k
    else:
        sky(cv, 0, 0, w - 1, c.hz, c.mood, r)
        backdrop(cv, 0, w - 1, c.hz, c.terrain, c.mood, r, int(H * 0.2))
        landing = (c.port.get("landing") or "jetty")
        ground(cv, 0, c.hz + 1, w - 1, c.hz + 9 * k, c.g1, c.g2, r)
        for i in range(5):
            ww = r.randint(12, 20) * k
            x = int(i * w / 5.0 + r.randint(0, 8 * k))
            if i == 2 and c.christian and "cross" in signs:
                church(cv, x, c.hz + 7 * k, 30 * k, 18 * k, c.stone, True)
            else:
                house(cv, x, c.hz + 7 * k, ww, r.randint(9, 14) * k, c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
        if "fire" in signs:
            for i in range(3):
                fire(cv, int(w * (0.2 + 0.25 * i)), c.hz + 2 * k, 2 * k); smoke(cv, int(w * (0.2 + 0.25 * i)), c.hz - 6 * k, 14 * k, SLATE)
        sea(cv, 0, c.hz + 10 * k, w - 1, int(H * 0.68), c.mood, r, True)
        boat(cv, int(w * 0.56), int(H * 0.66), int(w * 0.34), True, False)
        if "raider" in signs:
            cv.blit(glyph("raider", k, None), int(w * 0.1), c.hz + 8 * k)
        if landing == "beach":
            cv.rect(0, int(H * 0.68), w - 1, H - 1, PSAND if c.mood != "winter" else SNOW)
            cv.dither(0, int(H * 0.68), w - 1, int(H * 0.72), SAND if c.mood != "winter" else FROST)
            cv.line(0, int(H * 0.68), w - 1, int(H * 0.68), FOAM)
        else:
            cv.rect(0, int(H * 0.68), w - 1, H - 1, DEARTH)
            for y in range(int(H * 0.68) + 2 * k, H, 5 * k):
                cv.line(0, y, w - 1, y, TIMBER)
            cv.line(0, int(H * 0.68), w - 1, int(H * 0.68), SAND)
            for x in range(6 * k, w, 22 * k):
                cv.rect(x, int(H * 0.68) - 3 * k, x + 2 * k - 1, int(H * 0.68) + k, WEARTH)
        base = H - 3 * k
    # the people of the card
    roles = [x for x in (roles or []) if x][:3]
    n = len(roles)
    for i, role in enumerate(roles):
        x = int(w * (0.16 + 0.2 * i)) if stage != "long" else int(w * (0.14 + 0.2 * i) + (w * 0.22 if i >= 1 else 0))
        cloak = ROLE_CLOAK.get(role) or (BASALT if role in ("monk", "nun", "abbot", "abbess", "porter", "cellarer", "guest_master") else
                                         (SLATE if role in CLERGY else r.choice((DEARTH, SHOAL, STONE, MEADOW, RUST, TIMBER))))
        person(cv, x, base, fs, cloak, r.choice(HAIRS), hood=role in ("monk", "pilgrim", "beggar", "fisherman", "crewman", "sailor", "pilot"),
               female=role in WOMEN)
        tok = ROLE_TOKEN.get(role)
        if tok:
            cv.blit(glyph(tok, k, NIGHT), x - 8 * k, base - 11 * fs - 17 * k)
    # what it is about
    spots = [(int(w * 0.78) - 8 * gs, base - 16 * gs + 2 * k), (int(w * 0.56) - 8 * gs, base - 16 * gs + 2 * k)]
    if n == 0:
        spots = [(int(w * 0.3) - 8 * gs, base - 16 * gs + 2 * k), (int(w * 0.7) - 8 * gs, base - 16 * gs + 2 * k)]
    drawn = 0
    for g in signs:
        if g in ("raider",) and stage in ("sea", "harbour"):
            continue
        if drawn >= len(spots):
            break
        cv.blit(glyph(g, gs, NIGHT), spots[drawn][0], spots[drawn][1])
        drawn += 1
    if stage != "long":                                          # no rain or fog under a roof
        weather(cv, 0, 0, w - 1, H - 1, c.mood, r)
    if frame:
        cv.box(0, 0, w - 1, H - 1, NIGHT); cv.box(1, 1, w - 2, H - 2, frame); cv.box(2, 2, w - 3, H - 3, NIGHT)
        if k >= 2:
            cv.box(3, 3, w - 4, H - 4, frame)
    if strip:
        label_strip(cv, title or str(ident), 7)
    placeholder_mark(cv)
    return cv, c


def draw_plate(ident, title, sub, stage, texts, port=None, w=640, h=360, frame=GOLD):
    """A full plate (an age, a start, an ending): a card's picture made large, in a frame, with its name across the top."""
    body, c = vignette(ident, stage, texts, (), port, w - 24, h - 60, None, None, None, False, False)
    cv = Cv(w, h, NIGHT)
    cv.rect(0, 0, w - 1, h - 1, DPURP); cv.box(0, 0, w - 1, h - 1, NIGHT); cv.box(3, 3, w - 4, h - 4, frame); cv.box(5, 5, w - 6, h - 6, BYZ)
    for x, y in ((6, 6), (w - 15, 6), (6, h - 15), (w - 15, h - 15)):
        cv.rect(x, y, x + 8, y + 8, frame); cv.rect(x + 2, y + 2, x + 6, y + 6, BYZ); cv.rect(x + 4, y + 4, x + 4, y + 4, PGOLD)
    cv.blit(body, 12, 44)
    cv.box(11, 43, w - 12, h - 16, frame)
    scale = 3 if Cv.text_w(ascii_up(title or ""), 3) <= w - 60 else 2
    cv.text_mid(w // 2, 12, title or str(ident), PGOLD, scale, NIGHT)
    if sub:
        cv.text_mid(w // 2, 12 + 6 * scale + 3, sub, LILAC, 2 if scale == 3 else 1, NIGHT)
    cv.text_fit(14, h - 12, str(ident), MAUVE, w - 40)
    placeholder_mark(cv)
    return cv


def draw_ship_offer(cls, port, text="", w=192, h=108, title=None):
    """A ship as she can be had at a port: the hull of her class on the shore with the wrights about her."""
    cv = Cv(w, h)
    H = h - 7
    c = _ctx(cv, "shipat:%s:%s" % (port.get("id"), cls.get("id")), text, port, w, H, 40)
    r = c.rng
    sky(cv, 0, 0, w - 1, c.hz, c.mood, r)
    backdrop(cv, 0, w - 1, c.hz, c.terrain, c.mood, r, 20)
    ground(cv, 0, c.hz + 1, w - 1, c.hz + 10, c.g1, c.g2, r)
    for i in range(4):
        house(cv, 6 + i * 48 + r.randint(0, 8), c.hz + 8, r.randint(14, 22), r.randint(9, 13), c.wall, c.wsh, c.roof, c.rsh, True, c.lit)
    cv.rect(0, c.hz + 11, w - 1, H - 1, PSAND if c.mood != "winter" else SNOW)
    sea(cv, 0, H - 12, w - 1, H - 1, c.mood, r, True); cv.line(0, H - 13, w - 1, H - 13, FOAM)
    t = ship_traits(cls)
    ppm = min(6.0, (w - 30) / max(4.0, t["L"]))
    Lp = int(t["L"] * ppm)
    x0 = (w - Lp) // 2
    yw = H - 18
    if t["mast"]:
        mx = x0 + Lp // 2
        cv.line(mx, max(3, yw - int(t["sail_top"] * ppm)), mx, yw, WEARTH)
    _hull_side(cv, x0, yw, Lp, t, ppm, PEAT if t["kind"] == "hide" else TIMBER, WEARTH)
    for x in (x0 + Lp // 5, x0 + Lp // 2, x0 + Lp * 4 // 5):
        cv.line(x, yw + 1, x - 4, yw + 6, WEARTH); cv.line(x, yw + 1, x + 4, yw + 6, WEARTH)
    person(cv, max(8, x0 - 10), H - 16, 2, DEARTH, TIMBER); cv.blit(glyph("axe", 1, NIGHT), max(0, x0 - 26), H - 52)
    person(cv, min(w - 10, x0 + Lp + 10), H - 16, 2, SHOAL, PGOLD)
    weather(cv, 0, 0, w - 1, H - 1, c.mood, r)
    label_strip(cv, title or "%s at %s" % (cls.get("id"), port.get("id")), 7)
    placeholder_mark(cv)
    return cv


# ================================================================================================ the interface
def _slip(cv, x0, y0, x1, y1, ink=False):
    fill, edge, sh = (BASALT, BYZ, NIGHT) if ink else (PSAND, NIGHT, DPURP)
    cv.rect(x0 + 3, y0 + 3, x1, y1, sh)
    cv.rect(x0, y0, x1 - 3, y1 - 3, fill)
    cv.box(x0, y0, x1 - 3, y1 - 3, edge, 2)
    if not ink:
        cv.line(x0 + 2, y0 + 2, x1 - 5, y0 + 2, WWHITE)
    else:
        cv.line(x0 + 2, y0 + 2, x1 - 5, y0 + 2, SLATE)


def _keel(cv, x, y, fill, edge):
    pts = [(x, y + 2), (x + 13, y), (x + 19, y + 5), (x + 13, y + 10), (x, y + 8)]
    cv.poly(pts, fill, edge)


def ui_frame_paper(cv):
    _slip(cv, 0, 0, cv.w - 1, cv.h - 1, False)


def ui_frame_ink(cv):
    _slip(cv, 0, 0, cv.w - 1, cv.h - 1, True)


def ui_button(cv):
    cv.rect(0, 0, cv.w - 1, cv.h - 1, BASALT); cv.box(0, 0, cv.w - 1, cv.h - 1, PLUM); cv.text_mid(cv.w // 2, cv.h // 2 - 2, "BUTTON", PSTONE)


def ui_button_on(cv):
    cv.rect(0, 0, cv.w - 1, cv.h - 1, BYZ); cv.box(0, 0, cv.w - 1, cv.h - 1, PLUM); cv.text_mid(cv.w // 2, cv.h // 2 - 2, "BUTTON", PGOLD)


def ui_button_hover(cv):
    cv.rect(0, 0, cv.w - 1, cv.h - 1, DPURP); cv.box(0, 0, cv.w - 1, cv.h - 1, MAUVE); cv.text_mid(cv.w // 2, cv.h // 2 - 2, "BUTTON", WWHITE)


def ui_close(cv):
    cv.line(1, 1, 7, 7, STONE, 1); cv.line(7, 1, 1, 7, STONE, 1); cv.line(2, 1, 8, 7, SLATE); cv.line(6, 1, 0, 7, SLATE)


def ui_cursor(cv):
    cv.poly([(1, 1), (1, 12), (4, 9), (7, 14), (9, 13), (6, 8), (10, 8)], WWHITE, NIGHT)


def ui_cursor_hand(cv):
    cv.rect(5, 1, 6, 8, WWHITE); cv.rect(3, 6, 12, 13, WWHITE); cv.rect(7, 4, 8, 6, WWHITE); cv.rect(9, 5, 10, 6, WWHITE)
    cv.rect(11, 6, 12, 7, WWHITE); cv.rect(2, 8, 3, 10, WWHITE); cv.outline_all(NIGHT)


def ui_focus(cv):
    cv.box(0, 0, cv.w - 1, cv.h - 1, PGOLD); cv.box(1, 1, cv.w - 2, cv.h - 2, GOLD)


def ui_port_selected(cv):
    cv.box(0, 0, cv.w - 1, cv.h - 1, PGOLD, 2)
    for x, y in ((0, 0), (cv.w - 2, 0), (0, cv.h - 2), (cv.w - 2, cv.h - 2)):
        cv.rect(x, y, x + 1, y + 1, NIGHT)
    cv.rect(8, 0, cv.w - 9, 1, None); cv.rect(8, cv.h - 2, cv.w - 9, cv.h - 1, None)
    cv.rect(0, 8, 1, cv.h - 9, None); cv.rect(cv.w - 2, 8, cv.w - 1, cv.h - 9, None)


def ui_her_ring(cv):
    for a in range(0, 360, 30):
        cv.arc(1, 1, cv.w - 2, cv.h - 2, a, a + 18, PGOLD, 1)


def ui_her_ring_open(cv):
    cv.ell(1, 1, cv.w - 2, cv.h - 2, None, PGOLD); cv.ell(2, 2, cv.w - 3, cv.h - 3, None, GOLD)


def ui_sail(cv):
    cv.poly([(4, 0), (7, 7), (0, 7)], WWHITE, NIGHT)


def ui_wind_streak(cv):
    cv.line(0, 2, 11, 2, WAVE); cv.poly([(11, 0), (15, 2), (11, 4)], WAVE)


def ui_wind_vane(cv):
    cv.ell(1, 1, 22, 22, BASALT, WWHITE); cv.line(4, 12, 19, 12, PGOLD, 2); cv.line(19, 12, 15, 8, PGOLD, 2); cv.line(19, 12, 15, 16, PGOLD, 2)


def ui_keel_solid(cv):
    _keel(cv, 0, 0, LILAC, BASALT)


def ui_keel_hollow(cv):
    _keel(cv, 0, 0, BASALT, LILAC)


def ui_keel_faint(cv):
    for i in range(0, 20, 3):
        cv.px(i, 2 - (1 if i > 6 else 0), LILAC); cv.px(i, 8 + (1 if i > 6 else 0), LILAC)
    cv.px(19, 5, LILAC); cv.px(17, 3, LILAC); cv.px(17, 7, LILAC); cv.px(0, 5, LILAC)


def ui_keel_late(cv):
    _keel(cv, 0, 0, BASALT, RUST)


def ui_flag(cv):
    cv.rect(0, 0, 0, 11, WWHITE); cv.rect(1, 0, 8, 5, BYZ); cv.box(1, 0, 8, 5, LILAC)


def ui_year_bar(cv):
    w = cv.w
    for a, b, k in ((0, 0.22, MEADOW), (0.22, 0.45, GOLD), (0.45, 0.76, DEARTH), (0.76, 1.0, STONE)):
        cv.rect(int(w * a), 0, int(w * b) - 1, cv.h - 1, k)


def _diamond(cv, fill):
    cv.poly([(3, 0), (6, 3), (3, 6), (0, 3)], fill, WWHITE)


def ui_year_mark_good(cv):
    _diamond(cv, GRASS)


def ui_year_mark_gold(cv):
    _diamond(cv, GOLD)


def ui_year_mark_bad(cv):
    _diamond(cv, BLOOD)


def ui_year_mark_plain(cv):
    _diamond(cv, BASALT)


def ui_year_today(cv):
    cv.rect(0, 0, 1, cv.h - 1, NIGHT)


def ui_clock_pause(cv):
    cv.box(0, 0, 11, 9, STONE); cv.rect(3, 2, 4, 7, PSTONE); cv.rect(7, 2, 8, 7, PSTONE)


def ui_clock_run(cv):
    cv.box(0, 0, 11, 9, STONE); cv.poly([(4, 2), (8, 5), (4, 7)], PSTONE)


def ui_clock_fast(cv):
    cv.box(0, 0, 11, 9, STONE); cv.poly([(2, 2), (5, 5), (2, 7)], PSTONE); cv.poly([(6, 2), (9, 5), (6, 7)], PSTONE)


def ui_heart_full(cv):
    cv.rect(1, 1, 3, 3, BLOOD); cv.rect(4, 1, 6, 3, BLOOD); cv.rect(1, 3, 6, 4, BLOOD); cv.rect(2, 5, 5, 5, BLOOD); cv.rect(3, 6, 4, 6, BLOOD)
    cv.px(2, 2, RUST)


def ui_heart_empty(cv):
    ui_heart_full(cv); cv.rect(2, 2, 2, 3, None); cv.rect(5, 2, 5, 3, None); cv.rect(3, 4, 4, 4, None)
    cv.rect(2, 3, 5, 3, None)


def ui_keels_full(cv):
    cv.rect(0, 0, 9, 5, BYZ)


def ui_keels_hollow(cv):
    cv.box(0, 0, 9, 5, BYZ)


def ui_keels_lost(cv):
    cv.box(0, 0, 9, 5, BLOOD)


def _icon12(name):
    def f(cv):
        g = glyph(name, 1, None)
        cv.blit(g.im.resize((12, 12), 0), 0, 0)
    return f


def ui_hold_cell(cv):
    cv.rect(0, 0, 23, 23, SAND); cv.box(0, 0, 23, 23, NIGHT); cv.blit(glyph("cask", 1), 4, 4)


def ui_hold_cell_empty(cv):
    for i in range(0, 24, 3):
        cv.px(i, 0, NIGHT); cv.px(i, 23, NIGHT); cv.px(0, i, NIGHT); cv.px(23, i, NIGHT)


def ui_hold_cell_on(cv):
    cv.rect(0, 0, 23, 23, SAND); cv.box(0, 0, 23, 23, BYZ, 2); cv.blit(glyph("cask", 1), 4, 4)


def ui_crew_head(cv):
    cv.ell(0, 0, 11, 11, PSAND, NIGHT); cv.text(5, 3, "1", NIGHT)


def ui_crew_head_on(cv):
    cv.ell(0, 0, 11, 11, BYZ, NIGHT); cv.text(5, 3, "1", PSAND)


def ui_boat_plan(cv):
    w, h = cv.w, cv.h
    pts = [(4, h // 2)]
    for i in range(0, 21):
        t = i / 20.0
        pts.append((4 + (w - 8) * t, h // 2 - (h // 2 - 6) * math.sin(math.pi * t) ** 0.7))
    for i in range(20, -1, -1):
        t = i / 20.0
        pts.append((4 + (w - 8) * t, h // 2 + (h // 2 - 6) * math.sin(math.pi * t) ** 0.7))
    cv.poly(pts, SAND, NIGHT)
    for x in range(40, w - 40, 30):
        cv.line(x, 12, x, h - 12, DEARTH)
    cv.ell(w // 2 - 3, h // 2 - 3, w // 2 + 3, h // 2 + 3, WEARTH, NIGHT)


def ui_word_seen(cv):
    cv.rect(0, 0, 2, cv.h - 1, GRASS)


def ui_word_heard(cv):
    cv.rect(0, 0, 2, cv.h - 1, GOLD)


def ui_word_none(cv):
    cv.rect(0, 0, 2, cv.h - 1, STONE)


def ui_price_good(cv):
    cv.poly([(4, 0), (8, 5), (0, 5)], REED, NIGHT); cv.rect(3, 6, 5, 8, REED)


def ui_price_fair(cv):
    cv.rect(0, 3, 8, 5, PGOLD); cv.box(0, 2, 8, 6, NIGHT)


def ui_price_bad(cv):
    cv.poly([(4, 8), (8, 3), (0, 3)], DROSE, NIGHT); cv.rect(3, 0, 5, 2, DROSE)


def ui_tide(cv):
    cv.blit(glyph("tide", 1, NIGHT), 0, 0)


def ui_news_bad(cv):
    cv.blit(glyph("speech", 1, NIGHT), 0, 0); cv.rect(11, 0, 15, 4, BLOOD); cv.box(11, 0, 15, 4, NIGHT)


def ui_news_fair(cv):
    cv.blit(glyph("speech", 1, NIGHT), 0, 0); cv.rect(11, 0, 15, 4, GOLD); cv.box(11, 0, 15, 4, NIGHT)


def ui_note(cv):
    cv.blit(glyph("leaf", 1, NIGHT), 0, 0)


def ui_course_dash(cv):
    cv.rect(0, 0, 4, 1, PGOLD)


def ui_face_frame(cv):
    cv.box(0, 0, cv.w - 1, cv.h - 1, NIGHT); cv.box(1, 1, cv.w - 2, cv.h - 2, BYZ); cv.box(2, 2, cv.w - 3, cv.h - 3, NIGHT)


def ui_title(cv):
    w, h = cv.w, cv.h
    cv.text_mid(w // 2, 2, "KAPMAN", PGOLD, 6, NIGHT)


# id, name, w, h, what to look for in the mock-up, drawer, where the game shows it, what it must show
UI_PIECES = [
    ("frame_paper", "The slip: the panel's frame, paper look", 48, 48, ".slip", ui_frame_paper,
     "Round every panel (the date slip, your boat, the place you clicked, what is said, the council) in the paper look.",
     "A frame cut in nine: the four corners are 8 px, the sides stretch, the middle fills. Warm paper, a hard dark edge 2 px wide, square corners, a hard shadow 3 px down and right. No tabs, no rounded corners (ui_mock/template2.html: 'slips: the only kind of panel there is')."),
    ("frame_ink", "The slip: the panel's frame, ink look", 48, 48, ".screen.ink", ui_frame_ink,
     "Round every panel in the ink look (dark, see-through).",
     "The same frame in the ink look: near-black fill, a byzantium edge 2 px wide, square corners, a hard black shadow. Cut in nine like the paper frame."),
    ("button", "A button, at rest", 48, 16, ".seg button", ui_button, "Every button with words on it.",
     "A plain button cut in three (6 px ends, the middle stretches): dark fill, a thin plum edge. The words are set by the game: leave the middle empty."),
    ("button_on", "A button, chosen", 48, 16, "button.on", ui_button_on, "A button that is on or held down.",
     "The same button lit: byzantium fill, the words in gold. Leave the middle empty."),
    ("button_hover", "A button, under the cursor", 48, 16, ":hover", ui_button_hover, "A button the cursor is over.",
     "The same button a shade lighter with a lighter edge. Leave the middle empty."),
    ("close", "The close mark", 9, 9, ".slip .x", ui_close, "Top right of a panel.", "A small cross, one colour, soft grey: it closes the panel."),
    ("cursor", "The cursor", 16, 16, "cursor", ui_cursor, "Everywhere: it replaces the system pointer.",
     "A pointer whose tip is the top left pixel. Light with a dark edge so it reads on sea, land and paper."),
    ("cursor_hand", "The cursor over something you can click", 16, 16, "cursor: pointer", ui_cursor_hand,
     "Over a port, a keel, a row or a button.", "A hand or a second pointer shape; the tip is where the finger points. Light with a dark edge."),
    ("focus", "The mark round the thing the keys are on", 16, 16, "focus-visible", ui_focus, "Round a button reached by the keyboard.",
     "A gold outline cut in nine (4 px corners): two pixels wide, empty inside."),
    ("port_selected", "The ring round a chosen port", 24, 24, ".port.sel", ui_port_selected, "On the chart, round the port you clicked.",
     "Four gold corners that sit round a 16 x 16 port mark; empty in the middle and along the sides."),
    ("her_ring", "The ring round your own boat", 24, 24, ".her::after", ui_her_ring, "On the chart, round your boat.",
     "A dashed gold ring, empty inside: 'this one is you'."),
    ("her_ring_open", "The ring round your boat, panel open", 24, 24, ".her.open", ui_her_ring_open, "On the chart while your boat's panel is open.",
     "The same ring drawn solid."),
    ("sail", "A sail made out", 8, 8, ".sail", ui_sail, "On the chart, where the lookout has made out another ship.",
     "A small pale sail with a dark edge: far off, no detail."),
    ("wind_streak", "A streak of wind", 16, 5, ".streak", ui_wind_streak, "Scattered over the sea on the chart, turned to the wind.",
     "A thin pale line with a small head, pointing right (the game turns it). Faint: it lies over the sea."),
    ("wind_vane", "The wind mark", 24, 24, ".vane", ui_wind_vane, "Top right of the screen, beside the wind's words.",
     "A dark disc with a pale rim and a gold arrow pointing right (the game turns the arrow to the wind)."),
    ("keel_solid", "One of your keels, seen today", 20, 11, ".keel", ui_keel_solid, "On the chart: a boat of your house that is in sight.",
     "A boat seen from above, bow to the right, filled lilac with a dark edge."),
    ("keel_hollow", "One of your keels, last heard of", 20, 11, ".keel.hollow", ui_keel_hollow, "On the chart: where a boat of your house was when word last came.",
     "The same shape hollow: dark inside, a lilac edge."),
    ("keel_faint", "Where a keel should be by now", 20, 11, ".keel.faint", ui_keel_faint, "On the chart: where the boat ought to be, by her orders.",
     "The same shape as a dotted outline only."),
    ("keel_late", "A keel that is overdue", 20, 11, ".keel.late", ui_keel_late, "On the chart: a boat of your house that is late.",
     "The hollow shape with a red edge."),
    ("flag", "A plot of your house", 10, 12, ".flag", ui_flag, "On the chart, at a port where your house holds a plot.",
     "A thin pale staff with a small byzantium flag edged lilac. No device on the flag."),
    ("year_bar", "The year stick", 120, 4, ".year", ui_year_bar, "In the date slip: the year from spring to winter.",
     "A bar in four lengths: spring green, summer gold, autumn brown, winter grey (22, 23, 31 and 24 parts in a hundred)."),
    ("year_mark_good", "A mark on the year stick: something good", 7, 7, ".year i.good", ui_year_mark_good, "On the year stick, at a feast or a good day.",
     "A small diamond, green, pale edge."),
    ("year_mark_gold", "A mark on the year stick: something due", 7, 7, ".year i.gold", ui_year_mark_gold, "On the year stick, at a fair or a day goods are due.",
     "A small diamond, gold, pale edge."),
    ("year_mark_bad", "A mark on the year stick: something to fear", 7, 7, ".year i.bad", ui_year_mark_bad, "On the year stick, at a debt day or a danger.",
     "A small diamond, red, pale edge."),
    ("year_mark_plain", "A mark on the year stick: a day to know", 7, 7, ".year i", ui_year_mark_plain, "On the year stick, at any other marked day.",
     "A small diamond, dark, pale edge."),
    ("year_today", "Today on the year stick", 2, 10, ".year u", ui_year_today, "On the year stick: where today is.", "A dark upright line that stands above and below the bar."),
    ("clock_pause", "The clock: held", 12, 10, ".clock", ui_clock_pause, "In the date slip.", "Two upright bars in a thin box, one colour (the game lights the one that is on)."),
    ("clock_run", "The clock: running", 12, 10, ".clock", ui_clock_run, "In the date slip.", "One arrowhead to the right in a thin box, one colour."),
    ("clock_fast", "The clock: fast", 12, 10, ".clock", ui_clock_fast, "In the date slip.", "Two arrowheads to the right in a thin box, one colour."),
    ("heart_full", "A crewman's heart, whole", 8, 8, ".hearts", ui_heart_full, "In the date slip and beside each man.", "A small red heart (the mock-up uses a red square)."),
    ("heart_empty", "A crewman's heart, lost", 8, 8, ".hearts i.o", ui_heart_empty, "In the date slip and beside each man.", "The same heart as an outline only."),
    ("keels_full", "The count of keels: one with you", 10, 6, ".keels", ui_keels_full, "In the date slip, once the house has more than one boat.", "A small filled bar, byzantium."),
    ("keels_hollow", "The count of keels: one away", 10, 6, ".keels i.h", ui_keels_hollow, "In the date slip.", "The same bar as an outline."),
    ("keels_lost", "The count of keels: one overdue", 10, 6, ".keels i.l", ui_keels_lost, "In the date slip.", "The same bar as a red outline."),
    ("icon_coin", "Silver aboard", 12, 12, "coin:", _icon12("coin"), "In the date slip and your boat's panel, before the pennies you carry.", "One coin, gold on any ground. No king's head is needed at this size."),
    ("icon_loaf", "Food aboard", 12, 12, "loaf:", _icon12("loaf"), "In the date slip, before the days of food.", "One round loaf."),
    ("icon_hull", "The hold", 12, 12, "hull:", _icon12("hull"), "In the date slip, before the weight in the hold.", "A small hull seen from the side."),
    ("icon_stick", "The tally stick: what is owed", 12, 12, "stick:", _icon12("tally"), "In your boat's panel, before the debt.", "A notched tally stick."),
    ("icon_chest", "The chest", 12, 12, "chest:", _icon12("chest"), "In your boat's panel, before the count of texts and riddles.", "A small wooden chest with a rounded lid."),
    ("hold_cell", "A place in the hold, filled", 24, 24, ".hull .cell", ui_hold_cell, "In your boat's panel: one lot of cargo.", "A square place in the hold with a dark edge; the good's own icon is laid on it by the game, so draw the empty square."),
    ("hold_cell_empty", "A place in the hold, empty", 24, 24, ".cell.empty", ui_hold_cell_empty, "In your boat's panel.", "The same square as a dashed outline."),
    ("hold_cell_on", "A place in the hold, chosen", 24, 24, ".cell.on", ui_hold_cell_on, "In your boat's panel: the lot you clicked.", "The same square with a thicker byzantium edge."),
    ("crew_head", "A crewman's mark", 12, 12, ".hull .head", ui_crew_head, "In your boat's panel: one man at his place.", "A small disc with a dark edge; the game writes his letter on it."),
    ("crew_head_on", "A crewman's mark, chosen", 12, 12, ".head.on", ui_crew_head_on, "In your boat's panel: the man you clicked.", "The same disc filled byzantium."),
    ("boat_plan", "Your boat from above", 256, 64, ".boat .hull", ui_boat_plan, "In your boat's panel: the plan the cargo and the men are laid on.",
     "The Gislinge boat seen from above, bow to the right: a slender double-ended hull, wood colour, a dark edge, the thwarts as lines, the mast step in the middle. Left empty for the cargo squares and the four men."),
    ("word_seen", "The mark of a thing you saw yourself", 3, 16, ".word.seen", ui_word_seen, "Left edge of a line of news or a price you saw with your own eyes.", "A green bar down the left of the words."),
    ("word_heard", "The mark of a thing you were told", 3, 16, ".word.heard", ui_word_heard, "Left edge of a line of news or a price that was told to you.", "A gold bar down the left of the words."),
    ("word_none", "The mark of no word", 3, 16, ".word", ui_word_none, "Left edge of a line where nothing is known.", "A grey bar down the left of the words."),
    ("price_good", "A known price: it pays well", 9, 9, "em.good", ui_price_good, "On the chart beside a port, when you ask where a good pays.", "A small green sign that says 'good': the mock-up uses the colour alone."),
    ("price_fair", "A known price: fair", 9, 9, "em.fair", ui_price_fair, "On the chart beside a port.", "A small gold sign that says 'fair'."),
    ("price_bad", "A known price: cheap here", 9, 9, "em.bad", ui_price_bad, "On the chart beside a port.", "A small red sign that says 'bad'."),
    ("tide", "A tide note", 16, 16, "tides", ui_tide, "On the chart at a bar or a channel with a tide to know.", "Waves with an arrow: a place where the tide matters."),
    ("news_bad", "News on the chart: ill", 16, 16, "news", ui_news_bad, "On the chart where bad news is known (a raid, a famine).", "A scroll of speech with a red mark."),
    ("news_fair", "News on the chart: fair", 16, 16, "news", ui_news_fair, "On the chart where good news is known (a fair, a feast).", "A scroll of speech with a gold mark."),
    ("note", "A note of your own", 16, 16, "notes", ui_note, "On the chart where you wrote a note.", "A small leaf with a line or two on it."),
    ("course_dash", "The course line", 8, 2, "D.course", ui_course_dash, "On the chart: the course laid for your boat, as a dashed gold line.", "One dash and its gap (5 on, 3 off); the game repeats it along the course."),
    ("face_frame", "The frame round a face", 68, 84, ".talk .face", ui_face_frame, "In the panel of what is said, round the speaker's portrait.", "A thin frame for a 64 x 80 portrait: two pixels all round, byzantium between dark lines, empty inside."),
]


def draw_ui(piece):
    pid, name, w, h, find, fn, where, what = piece
    cv = Cv(w, h)
    fn(cv)
    if w >= 12 and h >= 12 and pid not in ("cursor", "cursor_hand"):
        placeholder_mark(cv)
    return cv


# ================================================================================================ a port from above
def _roof_block(cv, x, y, bw, bd, wh, roof, rsh, wall, wsh, lit=False):
    """A house seen from above and a little to the south: its roof (the far slope in light, the near one in shade,
    the ridge between) and under the eaves a strip of its south wall. (x, y) is the left end of the eaves' line."""
    cv.rect(x + 2, y + wh + 1, x + bw + 3, y + wh + 3, WSHADE)          # its shadow on the ground
    cv.rect(x, y - bd, x + bw, y, roof)
    cv.rect(x, y - bd // 2, x + bw, y, rsh)
    cv.line(x, y - bd // 2, x + bw, y - bd // 2, wsh)
    cv.rect(x, y + 1, x + bw, y + wh, wall)
    cv.rect(x + bw - 1, y + 1, x + bw, y + wh, wsh)
    if bw >= 12:
        cv.rect(x + bw // 2 - 1, y + wh - 2, x + bw // 2, y + wh, GOLD if lit else WEARTH)   # the door


def _boat_above(cv, x, y, L, c=TIMBER, mast=True):
    """A boat from above, lying east and west."""
    cv.ell(x - L // 2, y - 3, x + L // 2, y + 3, c)
    cv.line(x - L // 2 + 3, y, x + L // 2 - 3, y, WEARTH)
    for t in range(-L // 2 + 6, L // 2 - 4, 6):
        cv.line(x + t, y - 2, x + t, y + 2, DEARTH)
    if mast:
        cv.rect(x, y - 1, x + 1, y, PSAND)


def _tree_above(cv, x, y, s, dark=False):
    cv.ell(x - s, y - s + 2, x + s, y + s + 2, WSHADE)
    cv.ell(x - s, y - s, x + s, y + s, FOREST if dark else GRASS)
    cv.ell(x - s + 1, y - s, x + s // 2, y + s // 3, GRASS if dark else MEADOW)


def draw_port_view(port, text="", w=640, h=360):
    """A PORT FROM ABOVE (the user, 7 Oct 2026: "when you're at a port, instead of a map it just shows an artwork of
    the current port ... a top down angle looking at the place, not animated except for lighting ... simple psx
    graphics"). Seen from high up and a little to the south, in plain day (the game lays the hour's light over it):
    the water at the foot of the picture, her landing, her houses as roofs with a strip of wall under each, her one
    great building by her kind, the lanes between, and fields and wood behind. By rule from the data: her kind, her
    size, her landing, her country, her faith."""
    cv = Cv(w, h)
    H = h - 7
    c = _ctx(cv, "portview:" + str(port.get("id")), "", port, w, H, 0, "day")
    r = c.rng
    kind = (port.get("kind") or "").lower()
    landing = (port.get("landing") or "beach").lower()
    size = int(max(1, min(5, num(port.get("size"), 2))))
    g1, g2 = ground_cols(c.terrain, "day")
    roof, rsh = (GRASS, FOREST) if c.terrain == "fjord" else (PSAND, SAND)
    wall, wsh = (DEARTH, TIMBER) if c.terrain in ("fjord", "wood") else (SAND, DEARTH)
    sea1, sea2, sea3 = SEAS["day"]
    cx = w // 2 + r.randint(-40, 40)
    river = landing == "riverbank"
    shore = int(H * (0.60 if river else 0.64))
    span = 90 + size * 52

    # ---- the land, and what is behind the place
    cv.rect(0, 0, w - 1, H - 1, g1)
    cv.dither(0, 0, w - 1, H - 1, g2, 0, 4)
    for i in range(5 + size * 2):                                   # strips of field, ploughed and fallow
        fw, fh = r.randint(46, 110), r.randint(16, 30)
        fx, fy = r.randint(0, w - fw), r.randint(4, max(8, shore - 150))
        col = r.choice((PSAND, MEADOW, SAND, g2))
        cv.rect(fx, fy, fx + fw, fy + fh, col)
        for yy in range(fy + 2, fy + fh, 4):
            cv.line(fx + 1, yy, fx + fw - 1, yy, DEARTH if col in (PSAND, SAND) else GRASS)
    wooded = c.terrain in ("wood", "fjord", "moor")
    for i in range(18 + (30 if wooded else 0)):                     # wood, thickest at the edges
        tx = r.choice((r.randint(0, w // 5), r.randint(w - w // 5, w - 1), r.randint(0, w - 1)))
        ty = r.randint(2, shore - 30)
        if abs(tx - cx) < span + 20 and ty > shore - 150:
            continue
        _tree_above(cv, tx, ty, r.randint(5, 9), wooded and r.random() < 0.6)

    # ---- the water at her foot
    if river:
        far = shore + 62 + size * 4
        cv.rect(0, shore, w - 1, far, sea1)
        cv.dither(0, shore + 10, w - 1, far - 8, sea2, 0, 3)
        cv.rect(0, far, w - 1, H - 1, g1)
        cv.dither(0, far, w - 1, H - 1, g2, 1, 4)
        for x in range(0, w, 7):                                    # reeds along both banks
            if abs(x - cx) > span:
                cv.line(x, shore - 2, x, shore + 1, REED)
            cv.line(x + 3, far - 1, x + 3, far + 2, REED)
        for i in range(6):
            _tree_above(cv, r.randint(0, w - 1), r.randint(far + 12, H - 6), r.randint(5, 8), wooded)
        water0, water1 = shore + 6, far - 6
    else:
        cv.rect(0, shore, w - 1, H - 1, sea1)
        cv.rect(0, shore + 34, w - 1, H - 1, sea2)
        cv.dither(0, shore + 22, w - 1, shore + 46, sea2, 0, 2)
        cv.rect(0, shore + 84, w - 1, H - 1, sea3)
        cv.dither(0, shore + 70, w - 1, shore + 96, sea3, 1, 2)
        for i in range(40):                                         # the light on the water
            wx, wy = r.randint(0, w - 12), r.randint(shore + 8, H - 4)
            cv.line(wx, wy, wx + r.randint(4, 10), wy, WAVE)
        water0, water1 = shore + 10, H - 12
    if landing == "beach":
        cv.rect(0, shore - 5, w - 1, shore + 3, PSAND)
        cv.dither(0, shore - 5, w - 1, shore - 2, SAND, 0, 3)
        cv.line(0, shore + 4, w - 1, shore + 4, FOAM)
    else:
        cv.line(0, shore, w - 1, shore, FOAM if not river else WEARTH)

    # ---- lanes: one along the water, one up from it, one behind each row
    rows = 2 + (size + 1) // 2
    cv.rect(cx - span, shore - 14, cx + span, shore - 9, PSAND)
    cv.rect(cx - 3, shore - 14 - rows * 34, cx + 3, shore - 9, PSAND)
    for j in range(rows):
        yl = shore - 44 - j * 34
        cv.rect(cx - span + j * 22, yl + 16, cx + span - j * 22, yl + 19, PSAND)

    # ---- the wall or the bank round her, by her kind
    top = shore - 34 - rows * 34
    if kind == "town":
        cv.box(cx - span - 10, top, cx + span + 10, shore - 4, PSTONE, 3)
        cv.box(cx - span - 10, top, cx + span + 10, shore - 4, STONE, 1)
        cv.rect(cx - 6, shore - 6, cx + 6, shore - 2, g1)            # the water gate
    elif kind in ("fort", "royal seat"):
        cv.ell(cx - span // 2 - 40, top + 6, cx + span // 2 + 40, shore - 6, g2, TIMBER)
        cv.ell(cx - span // 2 - 37, top + 9, cx + span // 2 + 37, shore - 9, g1, WEARTH)
        cv.rect(cx - 6, shore - 12, cx + 6, shore - 5, PSAND)

    # ---- the houses, row behind row
    big_done = False
    for j in range(rows):
        yb = shore - 24 - j * 34
        n = max(2, 3 + size * 2 - j * 2)
        x0, x1 = cx - span + j * 22, cx + span - j * 22
        for i in range(n):
            bw, bd = r.randint(14, 26), r.randint(12, 18)
            x = int(x0 + (x1 - x0 - bw) * (i + r.uniform(0.15, 0.85)) / n)
            if abs(x + bw // 2 - cx) < 8:
                continue                                            # the lane up from the water is kept clear
            if kind in ("fort", "royal seat", "monastery") and j >= 1 and abs(x + bw // 2 - cx) < 70:
                continue                                            # room for the great building
            _roof_block(cv, x, yb - r.randint(0, 5), bw, bd, r.randint(3, 5), roof, rsh, wall, wsh)
    # the one great building
    gy = shore - 24 - 34 * min(rows - 1, 1) - 8
    if kind == "monastery":
        cv.box(cx - 66, gy - 50, cx + 66, gy + 14, PSTONE, 2)        # the precinct's wall
        cv.rect(cx - 40, gy - 30, cx + 22, gy - 12, PSTONE)          # the church: nave, in stone
        cv.rect(cx - 40, gy - 21, cx + 22, gy - 12, STONE)
        cv.rect(cx - 12, gy - 40, cx + 2, gy - 2, PSTONE)            # and transept: a cross from above
        cv.rect(cx - 12, gy - 21, cx + 2, gy - 2, STONE)
        cv.ell(cx + 20, gy - 30, cx + 32, gy - 12, PSTONE)           # the apse
        cv.box(cx - 58, gy - 8, cx - 30, gy + 10, SAND, 2)           # the cloister
        big_done = True
    elif kind in ("fort", "royal seat"):
        _roof_block(cv, cx - 50, gy - 6, 100, 24, 6, roof, rsh, DEARTH, TIMBER, True)
        cv.rect(cx - 2, gy - 34, cx + 1, gy - 30, PSTONE)            # the smoke hole, and its smoke
        cv.dither(cx - 6, gy - 52, cx + 8, gy - 34, PSTONE, 0, 2)
        big_done = True
    if c.christian and not big_done and (size >= 3 or kind == "town"):
        ex = cx + r.choice((-1, 1)) * r.randint(40, span - 40)
        ey = shore - 24 - 34 - 4
        cv.rect(ex - 20, ey - 14, ex + 14, ey, PSTONE if kind == "town" else roof)
        cv.rect(ex - 20, ey - 7, ex + 14, ey, STONE if kind == "town" else rsh)
        cv.rect(ex - 6, ey - 20, ex, ey + 6, PSTONE if kind == "town" else roof)
        cv.rect(ex - 6, ey - 7, ex, ey + 6, STONE if kind == "town" else rsh)
        cv.box(ex - 28, ey - 26, ex + 22, ey + 12, WEARTH, 1)        # the churchyard's fence
    if kind in ("emporium", "market", "town"):                      # the market place, and its stalls
        mx = cx + (span // 2 if cx < w // 2 else -span // 2)
        cv.rect(mx - 34, shore - 40, mx + 34, shore - 16, PSAND)
        for i in range(3 + size):
            sx, sy = mx - 30 + (i % 4) * 16, shore - 38 + (i // 4) * 11
            cv.rect(sx, sy, sx + 10, sy + 6, r.choice((MAUVE, RUST, SHOAL, MEADOW, GOLD)))
            cv.line(sx, sy + 7, sx + 10, sy + 7, WSHADE)

    # ---- the landing, and the boats at it
    ships = size + (2 if kind == "emporium" else 1)
    if landing == "jetty" or (river and kind in ("emporium", "town", "market")):
        for i in range(2 + size):
            x = cx - span + 16 + i * (2 * span - 32) // (1 + size)
            cv.rect(x - 3, shore - 4, x + 3, shore + 34, DEARTH)
            for yy in range(shore - 2, shore + 34, 4):
                cv.line(x - 3, yy, x + 3, yy, TIMBER)
            cv.rect(x - 4, shore + 30, x - 4, shore + 36, WEARTH); cv.rect(x + 4, shore + 30, x + 4, shore + 36, WEARTH)
            if i < ships:
                _boat_above(cv, x + 22, shore + 20 + (i % 2) * 8, r.randint(26, 40), r.choice((TIMBER, DEARTH)))
    elif landing == "quay":
        cv.rect(cx - span, shore - 6, cx + span, shore + 5, DEARTH)
        for x in range(cx - span, cx + span, 8):
            cv.line(x, shore - 6, x, shore + 5, TIMBER)
        for i in range(ships):
            _boat_above(cv, cx - span + 30 + i * (2 * span - 60) // max(1, ships - 1 if ships > 1 else 1), shore + 13, r.randint(30, 46), r.choice((TIMBER, DEARTH)))
    else:                                                           # drawn up on the beach or the bank
        for i in range(ships):
            _boat_above(cv, cx - span + 24 + i * (2 * span - 48) // max(1, ships), shore + (1 if landing == "beach" else 8), r.randint(24, 38), r.choice((TIMBER, DEARTH)), False)
    for i in range(max(1, size - 1)):                               # and one or two lying off
        _boat_above(cv, r.randint(40, w - 40), r.randint(water0 + 26, max(water0 + 28, water1)), r.randint(30, 48), TIMBER)
    label_strip(cv, "%s (%s) from above: %s, %s" % (port.get("name") or port.get("id") or "", port.get("modern") or "", kind, landing), 7)
    placeholder_mark(cv)
    return cv


# ================================================================================================ the name's letters
TITLE_NAME = "Kāpman"
TITLE_FONT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Kapman", "Game", "UI", "Fonts",
                                           "GentiumBookPlus-Bold.ttf"))
_BAYER4 = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5]
TITLE_GREEN = (0x29, 0x70, 0x36)                           # the user's colour for the name; it is drawn far more faded


def draw_title_letter(ch, index=0, size=62):
    """ONE LETTER OF THE GAME'S NAME, WITH A TEXTURE (the user, 7 Oct 2026: "the title font needs to have texture to
    it"). The letter of the frame's own bold face, cut small with hard edges so that shown four times its size it has
    a console's pixels, and painted as a worn sign: warm white at its head going down through pale sand to sand in
    dots, a lit edge up and to the left, a shaded edge down and to the right, grain, a few scratches and chips, a dark
    purple line round it and its shadow on the water. Every letter's picture is the same height and stands on the same
    line, so the six go side by side as they are."""
    from PIL import Image, ImageDraw, ImageFont
    font = ImageFont.truetype(TITLE_FONT, size)
    asc, desc = font.getmetrics()
    adv = int(round(font.getlength(ch)))
    pad = 5
    w, h = adv + pad * 2 + 4, asc + desc + pad * 2 + 4
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).text((pad, pad), ch, font=font, fill=255)
    px = mask.load()
    r = rng_of("title:%s:%d" % (ch, index))

    def inside(x, y):
        return 0 <= x < w and 0 <= y < h and px[x, y] >= 128

    ys = [y for y in range(h) for x in range(w) if inside(x, y)]
    top, foot = (min(ys), max(ys)) if ys else (0, h - 1)
    # scratches: short slanting lines where the paint is gone, and chips out of the edge
    worn = set()
    for _ in range(4):
        x, y = r.randint(pad, w - pad), r.randint(top, foot)
        dx = r.choice((-1, 1))
        for k in range(r.randint(5, 12)):
            worn.add((x + dx * k, y + k // 2))
    chips = set()
    for _ in range(5):
        x, y = r.randint(pad, w - pad), r.randint(top, foot)
        if inside(x, y) and not (inside(x - 2, y) and inside(x + 2, y) and inside(x, y - 2) and inside(x, y + 2)):
            for cx in range(-1, 2):
                for cy in range(-1, 2):
                    if r.random() < 0.7:
                        chips.add((x + cx, y + cy))

    def solid(x, y):
        return inside(x, y) and (x, y) not in chips

    cv = Cv(w, h)
    # THE NAME'S OWN COLOUR (the user, 7 Oct 2026: "change the white colour for the 297036 hex color", "but far more
    # faded"): that green, washed out toward warm white. These five are the only colours in the game's art that are not
    # of the forty: the name is the one piece that has its own.
    def faded(k, dark=0.0):
        g, wh = TITLE_GREEN, (246, 239, 224)
        c = [g[i] + (wh[i] - g[i]) * k for i in range(3)]
        return tuple(int(round(v * (1.0 - dark))) for v in c) + (255,)
    # (7 Oct, late: for a few hours the body was the green itself, on a misreading: the white the user wanted changed
    # was the frame's and its windows', "not the font". Of the faded cut, on the title: "this earlier picture
    # brightness was perfect". So the faded cut stands.)
    tones = [faded(0.70), faded(0.52), faded(0.34)]
    lit, shaded, scratch = faded(0.86), faded(0.18, 0.12), faded(0.0, 0.25)
    put = cv.im.putpixel
    for y in range(h):
        for x in range(w):
            if solid(x, y):
                t = (y - top) / float(max(1, foot - top))
                v = t * 2.0 + (_BAYER4[(y & 3) * 4 + (x & 3)] + 0.5) / 16.0 - 0.5
                i = max(0, min(2, int(v)))
                if r.random() < 0.05:
                    i = min(2, i + 1)                                # grain
                c = tones[i]
                if not solid(x - 1, y - 1):
                    c = lit                                          # the lit edge
                elif not solid(x + 1, y + 1):
                    c = shaded                                       # the shaded edge
                if (x, y) in worn:
                    c = scratch
                put((x, y), c)
            elif any(solid(x + ax, y + ay) for ax in (-1, 0, 1) for ay in (-1, 0, 1)):
                cv.px(x, y, DPURP)                                   # the line round it
            elif solid(x - 2, y - 3) or solid(x - 3, y - 3) or solid(x - 2, y - 4):
                cv.px(x, y, NIGHT)                                   # its shadow, down and to the right
    return cv
