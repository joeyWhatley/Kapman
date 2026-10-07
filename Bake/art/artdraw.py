# -*- coding: utf-8 -*-
"""
ARTDRAW - the small placeholders, each drawn by rule from the data (make_art_list.py calls these):
portraits, goods, ships (side view and chart mark), icons, leaves and sticks, port marks, interface pieces.
The bigger pictures (scenes, harbour views, cards, plates) are in artscene.py.
"""
import math
import re

from artkit import *          # noqa: F401,F403  (the colour names, Cv, glyph, rng_of ...)
from artkit import Cv, glyph, rng_of, placeholder_mark, label_strip, GLYPHS


def num(v, default=0.0):
    try:
        return float(v)
    except Exception:
        return default


def has_word(text, *words):
    t = (text or "").lower()
    return any(re.search(r"(?<![a-z])" + w, t) for w in words)


# ================================================================================================ portraits
PEOPLE_BG = {"frankish": PLUM, "romance": DPURP, "frisian": SEA, "saxon": WSHADE, "english": FOREST, "danish": DEEP,
             "norse": SLATE, "svea": ABYSS, "gute": BASALT, "wendish": WEARTH, "slavic": PEAT, "baltic": TIMBER,
             "finnic_rus": WSHADE, "irish": FOREST, "gaelic": SHOAL}
PEOPLE_CLOAK = {"frankish": RUST, "romance": DROSE, "frisian": SHALL, "saxon": DEARTH, "english": GRASS, "danish": BLOOD,
                "norse": SHOAL, "svea": MAUVE, "gute": STONE, "wendish": MEADOW, "slavic": REED, "baltic": SAND,
                "finnic_rus": FROST, "irish": MEADOW, "gaelic": GOLD}
PEOPLE_HAIR = {"frankish": (TIMBER, WEARTH, DEARTH), "romance": (PEAT, WEARTH, TIMBER), "frisian": (DEARTH, PGOLD, TIMBER),
               "saxon": (PGOLD, DEARTH, TIMBER), "english": (DEARTH, TIMBER, PGOLD, RUST), "danish": (PGOLD, DEARTH, RUST),
               "norse": (PGOLD, DEARTH, RUST), "svea": (PGOLD, DEARTH), "gute": (PGOLD, TIMBER), "wendish": (DEARTH, TIMBER),
               "slavic": (DEARTH, TIMBER, PGOLD), "baltic": (PGOLD, DEARTH), "finnic_rus": (TIMBER, PEAT, DEARTH),
               "irish": (RUST, PEAT, TIMBER), "gaelic": (RUST, PEAT, TIMBER)}
SHADE_OF = {PGOLD: GOLD, RUST: BLOOD, TIMBER: WEARTH, PEAT: NIGHT, PSTONE: STONE, CWHITE: PSTONE, DEARTH: TIMBER,
            WEARTH: PEAT, GOLD: DEARTH, SAND: DEARTH, STONE: SLATE, SLATE: BASALT, BASALT: NIGHT, BLOOD: DPURP,
            SHOAL: SEA, SHALL: SHOAL, GRASS: FOREST, MEADOW: GRASS, REED: MEADOW, MAUVE: BYZ, BYZ: PLUM, DROSE: RUST,
            FROST: PSTONE, FOREST: WSHADE, SEA: DEEP, DEEP: ABYSS, PSAND: SAND, WWHITE: PSAND, LILAC: MAUVE, PLUM: DPURP}
COLOUR_WORDS = [("brown", TIMBER), ("blue", SHOAL), ("red", BLOOD), ("green", FOREST), ("grey", STONE), ("gray", STONE),
                ("black", BASALT), ("white", CWHITE), ("undyed", SAND), ("dark", SLATE), ("purple", BYZ),
                ("yellow", GOLD), ("russet", RUST), ("madder", RUST), ("plain", SAND), ("faded", PSTONE)]
CLERGY = {"priest", "bishop", "abbot", "monk", "scribe", "schoolmaster", "clerk", "guest_master", "porter", "cellarer"}
NUNS = {"abbess", "nun"}
COVERED = {"alewife", "farm_wife", "widow", "weaver", "wise_woman", "abbess", "nun"}
HOODED = {"fisherman", "ferryman", "pilot", "sailor", "crewman", "herdsman", "carter", "beggar", "pilgrim", "lookout",
          "steersman", "survivor", "salter", "fowler", "hunter", "leech"}
ROLE_TOKEN = {
    "tollman": "tally", "moneyer": "coins", "reeve": "staff", "harbour_master": "mooring", "count": "sword",
    "steward": "key", "lords_man": "spear", "kings_man": "letter", "king": "circlet", "chief": "ring",
    "lawman": "staff", "priest": "cross", "bishop": "crosier", "abbot": "crosier", "abbess": "crosier", "monk": "book",
    "nun": "book", "guest_master": "bed", "porter": "gate", "cellarer": "cask", "scribe": "quill",
    "schoolmaster": "leaf", "clerk": "quill", "merchant": "scales", "factor": "store", "skipper": "sail",
    "master": "sail", "pilot": "pole", "steersman": "oar", "mate": "rope", "lookout": "eye", "crewman": "oar",
    "sailor": "hull", "survivor": "wreck", "shipwright": "axe", "smith": "hammer", "goldsmith": "ring",
    "craftsman": "comb", "rune_carver": "runestick", "weaver": "spindle", "salter": "salt", "fisherman": "fish",
    "hunter": "bow", "fowler": "bird", "farmer": "sheaf", "farm_wife": "basket", "herdsman": "crook", "carter": "cart",
    "ferryman": "ferry", "alewife": "cup", "host": "house", "widow": "key", "singer": "lyre", "skald": "lyre",
    "setter": "question", "interpreter": "speech", "pilgrim": "staff", "slave_dealer": "chain", "captive": "chain",
    "hostage": "chain", "beggar": "bowl", "child": None, "wise_woman": "herb", "leech": "herb",
    "raider_chief": "raider", "raider": "shield", "warrior": "spear", "envoy": "letter", "commander": "shield",
    "owner": "spade",
}
FAITH_SIGN = {"christian": "cross", "old_gods": "thor", "slavic_gods": "post", "baltic_gods": "tree"}


def _looks_colour(looks, *garments):
    for g in garments:
        m = re.search(r"([a-z-]+)\s+(?:wool(?:len)?\s+|linen\s+|sea\s+|travelling\s+)?" + g, looks)
        if m:
            for w, c in COLOUR_WORDS:
                if w in m.group(1):
                    return c
    return None


def draw_portrait(p, w=64, h=80):
    """A head and shoulders. Build, hair, hood and cloak follow the person's people, faith, role, age and id,
    and any plain word in 'looks' (grey, bald, hood, beard, heavy, a blue cloak ...)."""
    cv = Cv(w, h)
    r = rng_of("portrait:" + str(p.get("id")))
    H = h - 8
    cx = w // 2
    looks = (p.get("looks") or "").lower()
    role = p.get("role") or ""
    people = p.get("people") or ""
    sex = p.get("sex") or "m"
    age = int(num(p.get("age"), 35))
    female = sex == "f"
    child = age < 14

    def has(*ws):
        return has_word(looks, *ws)

    b = 1 if has("heavy", "stout", "broad", "fat", "big", "thick", "burly", "bull", "large") else (
        -1 if has("lean", "thin", "spare", "slight", "small", "gaunt", "wiry", "narrow", "frail", "slender") else r.choice((0, 0, 0, 1, -1)))
    skin, shade = r.choice(((PEACH, DROSE), (MGOLD, PEACH), (PSAND, SAND), (PEACH, DROSE)))
    if has("weathered", "tanned", "sunburn", "red-faced", "ruddy", "wind-burnt"):
        skin, shade = PEACH, DROSE
    elif has("pale", "pallid", "white-faced", "sallow"):
        skin, shade = MGOLD, PEACH

    hair = r.choice(PEOPLE_HAIR.get(people, (TIMBER, DEARTH, PEAT)))
    if age >= 62:
        hair = CWHITE
    elif age >= 48:
        hair = PSTONE
    if has("white hair", "white-haired", "white beard", "hair white", "snow-white"):
        hair = CWHITE
    elif has("grey", "gray", "grizzled", "greying", "iron-grey"):
        hair = PSTONE
    elif has("red hair", "red-haired", "red beard", "ginger", "red-bearded", "foxy"):
        hair = RUST
    elif has("fair", "blond", "yellow hair", "flaxen", "straw"):
        hair = PGOLD
    elif has("black hair", "dark hair", "dark-haired", "black beard", "black-haired", "black-bearded"):
        hair = PEAT
    elif has("brown hair", "brown beard", "brown-haired"):
        hair = TIMBER
    hsh = SHADE_OF.get(hair, PEAT)

    clergy = role in CLERGY
    nun = role in NUNS or has("veil", "wimple")
    hood = has("hood", "hooded", "cowl") or (role in HOODED and r.random() < 0.45)
    if nun:
        style = "veil"
    elif female:
        if role in COVERED or has("head-cloth", "headcloth", "head cloth", "kerchief", "covered", "linen cap") or (age >= 22 and r.random() < 0.6):
            style = "headcloth"
        else:
            style = "braids" if (has("braid", "plait") or r.random() < 0.6) else "long"
    elif clergy or has("tonsure"):
        style = "tonsure"
    elif has("bald", "shaven head", "shaved head", "hairless"):
        style = "bald"
    elif has("tied back", "long hair", "hair loose", "to his shoulders", "braid", "hair long"):
        style = "long"
    elif people == "saxon":
        style = "long"
    elif people in ("norse", "danish", "svea", "gute", "wendish", "slavic", "baltic"):
        style = r.choice(("long", "short", "long"))
    else:
        style = "short"
    if female or child:
        beard = "none"
    elif has("clean-shaven", "beardless", "no beard", "shaven chin", "smooth-faced", "smooth-shaven", "shaven"):
        beard = "none"
    elif has("moustache") and not has("beard"):
        beard = "moustache"
    elif has("long beard", "great beard", "full beard", "beard to his", "forked beard", "big beard", "thick beard"):
        beard = "full"
    elif has("short beard", "beard cut short", "trimmed", "stubble", "cropped beard", "clipped beard", "neat beard"):
        beard = "short"
    elif has("beard"):
        beard = r.choice(("short", "full"))
    elif clergy:
        beard = "none" if r.random() < 0.7 else "short"
    elif people in ("frankish", "romance"):
        beard = r.choice(("moustache", "moustache", "short", "none"))
    elif age < 22:
        beard = "none"
    else:
        beard = r.choice(("short", "short", "full", "none")) if age < 40 else r.choice(("short", "full", "full"))
    helmet = has("helmet", "helm ")
    mail = has("mail", "byrnie")
    circlet = (role == "king" or has("circlet", "crown", "diadem", "gold band")) and not helmet

    cloak = _looks_colour(looks, "cloak", "mantle", "cape", "shawl")
    tunic = _looks_colour(looks, "tunic", "habit", "gown", "dress", "kirtle", "shirt", "robe")
    if clergy or nun:
        cloak = cloak or (BASALT if role in ("monk", "nun", "porter", "cellarer", "guest_master", "abbot", "abbess") else SLATE)
        tunic = tunic or (PSTONE if role in ("priest", "bishop", "scribe", "clerk", "schoolmaster") else SLATE)
    if role == "beggar" and not cloak:
        cloak = WEARTH
    cloak = cloak or PEOPLE_CLOAK.get(people, DEARTH)
    tunic = tunic or r.choice((SAND, PSAND, DEARTH, PSTONE, WWHITE))
    if cloak == tunic:
        tunic = SAND if cloak != SAND else DEARTH
    csh = SHADE_OF.get(cloak, PEAT)
    bg = PEOPLE_BG.get(people, SLATE)
    if bg == cloak:
        bg = SHADE_OF.get(bg, NIGHT)

    # ground and the sign of the faith ------------------------------------------------------------
    cv.rect(0, 0, w - 1, H - 1, bg)
    cv.dither(0, H - 26, w - 1, H - 1, NIGHT, step=3)
    cv.dither(0, 0, w - 1, 12, SHADE_OF.get(bg, NIGHT), step=3)
    sign = FAITH_SIGN.get(p.get("faith") or "")
    if sign:
        cv.blit(glyph(sign, 1, NIGHT), 1, 1)

    # sizes ----------------------------------------------------------------------------------------
    hw = (18 if child else 22) + 2 * b
    hh = 20 if child else 26
    top = 28 if child else (12 if not has("tall") else 10)
    if female and not child:
        hw -= 2
    sy = top + hh + 3                                   # where the shoulders begin
    sw = (30 if child else 46) + 6 * b
    x0, x1 = cx - hw // 2, cx + hw // 2 - 1
    ey = top + (10 if child else 11)

    # what lies behind the head: long hair, a hood, a veil ------------------------------------------
    if hood and style != "veil":
        cv.ell(x0 - 5, top - 5, x1 + 5, top + hh + 6, cloak, csh)
    elif style == "veil":
        cv.ell(x0 - 5, top - 4, x1 + 5, top + hh + 14, cloak, csh)
    elif style == "headcloth":
        cv.ell(x0 - 4, top - 3, x1 + 4, top + hh + 10, CWHITE, PSTONE)
    elif style in ("long", "braids"):
        cv.rect(x0 - 3, top + 4, x1 + 3, sy + (10 if style == "long" else 4), hair)
        cv.ell(x0 - 3, top - 3, x1 + 3, top + 12, hair)
        cv.line(x0 - 3, top + 6, x0 - 3, sy + 8, hsh); cv.line(x1 + 3, top + 6, x1 + 3, sy + 8, hsh)

    # shoulders, tunic, cloak ---------------------------------------------------------------------
    cv.poly([(cx - sw // 2, H), (cx - sw // 2 + 5, sy + 2), (cx - 6, sy - 1), (cx + 6, sy - 1), (cx + sw // 2 - 5, sy + 2),
             (cx + sw // 2, H)], tunic)
    if mail:
        cv.rect(cx - sw // 2 + 4, sy + 3, cx + sw // 2 - 4, H - 1, SLATE)
        cv.dither(cx - sw // 2 + 4, sy + 3, cx + sw // 2 - 4, H - 1, PSTONE)
    if role == "smith":
        cv.rect(cx - 9, sy + 6, cx + 9, H - 1, WEARTH); cv.line(cx - 9, sy + 6, cx - 4, sy, WEARTH); cv.line(cx + 9, sy + 6, cx + 4, sy, WEARTH)
    elif not child or r.random() < 0.5:
        # the cloak hangs from both shoulders and is pinned on the wearer's right (our left)
        cv.poly([(cx - sw // 2 - 2, H), (cx - sw // 2 + 3, sy + 2), (cx - 7, sy - 2), (cx - 2, sy + 6), (cx - 9, H)], cloak)
        cv.poly([(cx + sw // 2 + 2, H), (cx + sw // 2 - 3, sy + 2), (cx + 7, sy - 2), (cx + 5, sy + 5), (cx + 13, H)], cloak)
        cv.line(cx - 2, sy + 6, cx - 9, H - 1, csh); cv.line(cx + 5, sy + 5, cx + 13, H - 1, csh)
        cv.line(cx - sw // 2 + 3, sy + 2, cx - sw // 2 - 2, H - 1, csh)
        if not clergy and not nun and role not in ("beggar", "captive", "child"):
            cv.rect(cx - 9, sy + 1, cx - 7, sy + 3, GOLD if role in ("king", "count", "chief", "merchant", "bishop", "reeve", "kings_man", "raider_chief", "widow", "goldsmith") else PSTONE)
            cv.px(cx - 8, sy + 2, PEAT)
    if clergy and role in ("bishop", "abbot", "priest"):
        cv.rect(cx, sy + 8, cx + 1, sy + 14, PGOLD); cv.rect(cx - 2, sy + 10, cx + 3, sy + 11, PGOLD)
    if has("hammer amulet", "thor's hammer", "hammer at his neck", "hammer on a thong"):
        cv.rect(cx, sy + 6, cx, sy + 10, PSTONE); cv.rect(cx - 2, sy + 10, cx + 2, sy + 12, PSTONE)
    if has("beads", "oval brooches", "strap dress"):
        for i, k in enumerate((GOLD, RUST, SHALL, GOLD, BYZ, RUST, GOLD)):
            cv.rect(cx - 9 + i * 3, sy + 7 + (2 if 1 < i < 5 else 0), cx - 8 + i * 3, sy + 8 + (2 if 1 < i < 5 else 0), k)

    # neck and head --------------------------------------------------------------------------------
    cv.rect(cx - 4, top + hh - 3, cx + 3, sy + 1, shade)
    cv.rect(x0, top + 3, x1, top + hh - 6, skin)
    cv.rect(x0 + 2, top, x1 - 2, top + hh - 2, skin)
    cv.rect(x0 + 1, top + 1, x1 - 1, top + hh - 4, skin)
    cv.rect(x0 + 4, top + hh - 1, x1 - 4, top + hh, skin)
    cv.line(x1 - 1, top + 6, x1 - 1, top + hh - 6, shade); cv.line(x1, top + 5, x1, top + hh - 7, shade)
    cv.line(x0 + 4, top + hh, x1 - 4, top + hh, shade)
    if style not in ("veil", "headcloth") and not hood:
        cv.rect(x0 - 2, ey, x0 - 1, ey + 4, skin); cv.rect(x1 + 1, ey, x1 + 2, ey + 4, shade)
    # the face
    for ex in (cx - 6, cx + 3):
        cv.rect(ex, ey, ex + 2, ey + 1, CWHITE); cv.rect(ex + 1, ey, ex + 2, ey + 1, PEAT)
        cv.line(ex - 1, ey - 3, ex + 3, ey - 3, hsh if hair not in (CWHITE, PSTONE) else STONE)
    cv.line(cx, ey + 1, cx, ey + 6, shade); cv.line(cx - 2, ey + 7, cx + 1, ey + 7, shade)
    my = ey + (9 if child else 11)
    cv.line(cx - 3, my, cx + 2, my, WEARTH)
    cv.line(cx - 2, my + 1, cx + 1, my + 1, DROSE if skin != DROSE else RUST)
    if age >= 50:
        cv.line(cx - 5, top + 6, cx + 4, top + 6, shade); cv.line(cx - 7, ey + 3, cx - 4, ey + 3, shade)
        cv.line(cx + 3, ey + 3, cx + 6, ey + 3, shade); cv.px(cx - 5, my - 1, shade); cv.px(cx + 4, my - 1, shade)
    if has("scar"):
        cv.line(cx + 4, ey - 5, cx + 7, ey + 5, RUST)
    if has("blind", "one eye", "one-eyed"):
        cv.rect(cx + 3, ey, cx + 5, ey + 1, shade if has("blind") else PEAT)
        if has("blind"):
            cv.rect(cx - 6, ey, cx - 4, ey + 1, shade)

    # hair in front, beard -------------------------------------------------------------------------
    if style in ("short", "long", "braids"):
        cv.rect(x0 + 1, top - 2, x1 - 1, top + 3, hair); cv.rect(x0 - 1, top + 1, x0 + 1, ey + (2 if style == "short" else 6), hair)
        cv.rect(x1 - 1, top + 1, x1 + 1, ey + (2 if style == "short" else 6), hair)
        cv.rect(x0 + 3, top - 3, x1 - 3, top - 2, hair)
        cv.line(x0 + 3, top + 4, x0 + 7, top + 4, hair); cv.line(x1 - 8, top + 4, x1 - 3, top + 4, hair)
        cv.line(x0 + 4, top - 1, x1 - 6, top - 1, hsh)
        if style == "braids":
            for bx in (x0 - 3, x1 + 1):
                for i in range(sy + 16 - (top + 10)):
                    cv.rect(bx, top + 10 + i, bx + 2, top + 10 + i, hair if (i // 2) % 2 == 0 else hsh)
    elif style == "tonsure":
        cv.rect(x0 - 1, top + 3, x0 + 1, ey + 2, hair); cv.rect(x1 - 1, top + 3, x1 + 1, ey + 2, hair)
        cv.rect(x0 + 1, top + 2, x1 - 1, top + 4, hair); cv.line(x0 + 3, top + 5, x1 - 3, top + 5, hsh)
        cv.rect(x0 + 4, top - 1, x1 - 4, top + 1, skin); cv.line(x0 + 6, top - 1, x1 - 8, top - 1, WWHITE if skin != WWHITE else PSAND)
    elif style == "bald":
        cv.rect(x0 - 1, ey - 3, x0, ey + 2, hair); cv.rect(x1, ey - 3, x1 + 1, ey + 2, hair)
        cv.line(x0 + 5, top + 1, x0 + 9, top + 1, WWHITE)
    if beard == "moustache":
        cv.rect(cx - 5, my - 2, cx + 4, my - 1, hair); cv.line(cx - 6, my - 1, cx - 6, my + 3, hair); cv.line(cx + 5, my - 1, cx + 5, my + 3, hair)
    elif beard in ("short", "full"):
        low = top + hh + (0 if beard == "short" else 7)
        cv.rect(x0 + 1, my - 2, x1 - 1, top + hh - 2, hair)
        cv.rect(x0 + 3, top + hh - 2, x1 - 3, low, hair)
        if beard == "full":
            cv.rect(x0 + 5, low, x1 - 5, low + 2, hair)
        cv.rect(x0, ey + 4, x0 + 1, my, hair); cv.rect(x1 - 1, ey + 4, x1, my, hair)
        cv.line(cx - 3, my, cx + 2, my, WEARTH)
        cv.line(cx - 1, my + 3, cx - 1, low, hsh); cv.line(cx + 3, my + 4, cx + 3, low - 1, hsh)
        cv.line(x0 + 3, my + 2, x0 + 3, low - 2, hsh)

    # what is worn on the head -----------------------------------------------------------------------
    if style == "veil":
        cv.rect(x0 - 2, top - 1, x1 + 2, top + 4, CWHITE); cv.rect(x0 - 2, top + 4, x0, top + hh, CWHITE)
        cv.rect(x1, top + 4, x1 + 2, top + hh, CWHITE); cv.rect(x0, top + hh - 1, x1, top + hh + 4, CWHITE)
        cv.line(x0 - 2, top + 5, x1 + 2, top + 5, PSTONE)
        cv.rect(x0 - 3, top - 4, x1 + 3, top - 1, cloak)
    elif style == "headcloth":
        cv.rect(x0 - 1, top - 2, x1 + 1, top + 4, CWHITE); cv.rect(x0 - 2, top + 3, x0, top + hh + 2, CWHITE)
        cv.rect(x1, top + 3, x1 + 2, top + hh + 2, CWHITE); cv.line(x0, top + 5, x1, top + 5, PSTONE)
        cv.line(x0 + 3, top + 6, x0 + 6, top + 6, hair); cv.line(x1 - 6, top + 6, x1 - 3, top + 6, hair)
    elif hood:
        cv.rect(x0 - 1, top - 3, x1 + 1, top + 2, cloak); cv.rect(x0 - 3, top, x0, top + hh - 2, cloak)
        cv.rect(x1, top, x1 + 3, top + hh - 2, cloak); cv.line(x0, top + 3, x1, top + 3, csh)
        cv.line(x0 + 1, top + 3, x0 + 1, top + hh - 4, csh); cv.line(x1 - 1, top + 3, x1 - 1, top + hh - 4, csh)
        if style in ("short", "long") and hair:
            cv.line(x0 + 3, top + 4, x1 - 3, top + 4, hair)
    if helmet:
        hc = GOLD if has("gilded", "gilt", "gold helmet") else PSTONE
        cv.poly([(x0 - 1, top + 5), (x0 + 2, top - 3), (cx, top - 7), (x1 - 2, top - 3), (x1 + 1, top + 5)], hc, SLATE)
        cv.rect(cx - 1, top + 5, cx, ey + 4, hc); cv.line(x0 - 1, top + 5, x1 + 1, top + 5, SLATE)
        cv.line(cx - 4, top - 2, cx - 6, top + 3, CWHITE if hc == PSTONE else PGOLD)
    if circlet:
        cv.rect(x0 - 1, top + 2, x1 + 1, top + 4, GOLD); cv.line(x0 - 1, top + 5, x1 + 1, top + 5, DEARTH)
        cv.px(cx - 5, top + 3, BLOOD); cv.px(cx, top + 3, BYZ); cv.px(cx + 4, top + 3, BLOOD)
        cv.rect(cx - 1, top, cx, top + 1, GOLD)

    # the sign of what he does ----------------------------------------------------------------------
    tok = ROLE_TOKEN.get(role)
    if not tok:
        for wds, g in ((("tally",), "tally"), (("book", "psalter"), "book"), (("keys", "key "), "key"), (("harp", "lyre"), "lyre"),
                       (("staff",), "staff"), (("axe",), "axe"), (("sword",), "sword"), (("spear",), "spear"),
                       (("scales",), "scales"), (("rune",), "runestick"), (("purse",), "purse")):
            if has(*wds):
                tok = g
                break
    if tok:
        cv.rect(w - 19, H - 19, w - 2, H - 2, NIGHT); cv.box(w - 19, H - 19, w - 2, H - 2, PGOLD)
        cv.blit(glyph(tok), w - 18, H - 18)
    label_strip(cv, p.get("name") or p.get("id") or "", 8)
    placeholder_mark(cv)
    return cv


# ================================================================================================ goods
CAT_COL = {  # main, shade, accent
    "Food": (SAND, DEARTH, PGOLD), "Drink": (TIMBER, WEARTH, BLOOD), "Provisions": (DEARTH, TIMBER, SAND),
    "Spices": (TIMBER, WEARTH, RUST), "Textiles": (MAUVE, BYZ, LILAC), "Fibres": (SAND, DEARTH, CWHITE),
    "Dyes": (SAND, DEARTH, SHOAL), "Furs": (TIMBER, WEARTH, DEARTH), "Hides": (WEARTH, PEAT, DEARTH),
    "Ship stores": (TIMBER, PEAT, NIGHT), "Ivory and bone": (WWHITE, PSAND, SAND), "Beads and gems": (SAND, DEARTH, GOLD),
    "Crafts": (SAND, DEARTH, WWHITE), "Stone": (STONE, SLATE, PSTONE), "Pottery": (DROSE, RUST, PEACH),
    "Glass": (WAVE, SHALL, FOAM), "Metals": (SLATE, BASALT, PSTONE), "Weapons": (PSTONE, SLATE, GOLD),
    "Books and church goods": (BYZ, PLUM, GOLD), "Animals": (TIMBER, WEARTH, PSAND), "Livestock": (TIMBER, WEARTH, PSAND),
    "Timber": (DEARTH, TIMBER, SAND), "Captives": (STONE, SLATE, PEACH),
}
GOOD_COL = {  # by a word in the id: (main, shade, accent); None keeps the category's
    "wheat": (None, None, GOLD), "rye": (None, None, DEARTH), "barley": (None, None, PGOLD), "oats": (None, None, PSAND),
    "malt": (None, None, TIMBER), "flour": (None, None, WWHITE), "salt": (None, None, CWHITE), "almond": (None, None, DEARTH),
    "rice": (None, None, WWHITE), "hazel": (None, None, TIMBER), "hops": (None, None, MEADOW), "mustard": (None, None, GOLD),
    "rhenish_wine": (None, None, BLOOD), "frankish_wine": (None, None, RUST), "aquitaine_wine": (None, None, BYZ),
    "mead": (None, None, GOLD), "ale": (None, None, SAND), "herring": (None, None, SHALL), "salmon": (None, None, DROSE),
    "eels": (None, None, SLATE), "whale_meat": (None, None, DROSE), "salt_pork": (None, None, DROSE),
    "whale_oil": (None, None, PGOLD), "seal_oil": (None, None, PSAND), "tar": (None, None, NIGHT), "pitch": (None, None, PEAT),
    "honey": (DEARTH, TIMBER, GOLD), "butter": (DEARTH, TIMBER, PGOLD), "castoreum": (WEARTH, PEAT, RUST),
    "soapstone": (STONE, SLATE, PSTONE), "pepper": (None, None, PEAT), "cinnamon": (None, None, RUST),
    "cloves": (None, None, WEARTH), "ginger": (None, None, SAND), "cumin": (None, None, DEARTH), "kermes": (None, None, BLOOD),
    "incense": (None, None, PGOLD), "pearls": (None, None, CWHITE), "coral": (None, None, RUST), "pigments": (None, None, SHOAL),
    "frisian_cloth": (SHOAL, SEA, SHALL), "wool_cloth": (DEARTH, TIMBER, SAND), "wadmal": (STONE, SLATE, PSTONE),
    "fine_linen": (WWHITE, PSAND, FOAM), "linen": (PSAND, SAND, WWHITE), "silk": (BYZ, PLUM, GOLD),
    "cotton": (CWHITE, PSTONE, FOAM), "sailcloth": (SAND, DEARTH, PSAND), "raw_wool": (None, None, CWHITE),
    "fleeces": (CWHITE, PSTONE, WWHITE), "flax": (REED, MEADOW, PSAND), "hemp": (SAND, DEARTH, REED),
    "madder": (None, None, RUST), "alum": (None, None, FROST), "woad": (SAND, DEARTH, SHOAL),
    "squirrel": (PSTONE, STONE, CWHITE), "marten": (TIMBER, WEARTH, DEARTH), "sable": (PEAT, NIGHT, WEARTH),
    "beaver": (WEARTH, PEAT, TIMBER), "fox": (RUST, BLOOD, CWHITE), "otter": (WEARTH, PEAT, DEARTH),
    "ermine": (CWHITE, PSTONE, NIGHT), "wolf": (STONE, SLATE, PSTONE), "bear": (PEAT, NIGHT, WEARTH),
    "reindeer": (SAND, DEARTH, PSAND), "seal_skins": (STONE, SLATE, PSTONE), "cattle_hides": (TIMBER, WEARTH, PSAND),
    "leather": (DEARTH, TIMBER, SAND), "amber": (None, None, GOLD), "jet": (None, None, NIGHT), "raw_glass": (None, None, SHALL),
    "potash": (None, None, PSTONE), "sulphur": (None, None, PGOLD), "eider": (None, None, CWHITE), "feathers": (None, None, FROST),
    "copper": (RUST, BLOOD, DROSE), "brass": (GOLD, DEARTH, PGOLD), "tin": (PSTONE, STONE, CWHITE), "lead": (STONE, SLATE, PSTONE),
    "bar_iron": (SLATE, BASALT, STONE), "crucible": (SLATE, BASALT, PSTONE), "quicksilver": (DROSE, RUST, CWHITE),
    "quern": (SLATE, BASALT, STONE), "millstone": (STONE, SLATE, PSTONE), "whetstone": (STONE, SLATE, PSTONE),
    "badorf": (PSAND, SAND, RUST), "tating": (BASALT, NIGHT, PSTONE), "pingsdorf": (PSAND, SAND, BLOOD),
    "ipswich": (STONE, SLATE, PSTONE), "glass_vessels": (WAVE, SHALL, FOAM), "olive": (DROSE, RUST, MEADOW),
    "cheese": (PGOLD, GOLD, MGOLD), "wax": (GOLD, DEARTH, PGOLD), "sugar": (CWHITE, PSTONE, FOAM),
    "dates": (SAND, DEARTH, WEARTH), "ships_bread": (None, None, DEARTH), "walrus_rope": (WEARTH, PEAT, DEARTH),
    "rope": (SAND, DEARTH, PSAND), "rivets": (TIMBER, WEARTH, SLATE), "iron_tools": (DEARTH, TIMBER, SLATE),
    "glass_beads": (SHALL, SHOAL, GOLD), "carnelian": (RUST, BLOOD, CWHITE), "amber_beads": (GOLD, DEARTH, PGOLD),
    "oak": (DEARTH, TIMBER, SAND), "masts": (SAND, DEARTH, PSAND), "yew": (RUST, TIMBER, SAND), "musk": (WEARTH, PEAT, TIMBER),
}


def _g_sack(c, m, s, a):
    c.ell(6, 9, 26, 29, m, PEAT); c.rect(13, 4, 19, 10, m); c.line(12, 4, 20, 4, PEAT); c.line(12, 4, 12, 9, PEAT)
    c.line(20, 4, 20, 9, PEAT); c.line(12, 10, 20, 10, s); c.line(11, 11, 21, 11, PEAT)
    c.line(10, 16, 12, 25, s); c.line(21, 14, 23, 23, s); c.line(16, 14, 16, 27, s)
    c.poly([(1, 30), (5, 25), (9, 24), (13, 30)], a, PEAT); c.px(6, 27, s)


def _g_cask(c, m, s, a):
    c.rect(8, 4, 23, 28, m); c.rect(6, 9, 25, 23, m); c.line(8, 4, 23, 4, PEAT); c.line(8, 28, 23, 28, PEAT)
    c.line(5, 10, 5, 22, PEAT); c.line(26, 10, 26, 22, PEAT); c.line(6, 8, 7, 5, PEAT); c.line(25, 8, 24, 5, PEAT)
    c.line(6, 24, 7, 27, PEAT); c.line(25, 24, 24, 27, PEAT)
    for y in (7, 12, 20, 25):
        c.line(6 if 8 < y < 24 else 8, y, 25 if 8 < y < 24 else 23, y, PEAT)
    for x in (11, 15, 19):
        c.line(x, 5, x, 27, s)
    c.rect(14, 15, 17, 17, a); c.box(13, 14, 18, 18, PEAT); c.line(16, 18, 16, 22, a)


def _g_barrel(c, m, s, a):
    c.rect(7, 9, 24, 28, m); c.line(7, 28, 24, 28, PEAT); c.line(6, 10, 6, 27, PEAT); c.line(25, 10, 25, 27, PEAT)
    for y in (13, 23):
        c.line(7, y, 24, y, PEAT)
    for x in (11, 15, 20):
        c.line(x, 10, x, 27, s)
    c.ell(6, 5, 25, 12, a, PEAT)


def _g_fish_top(c, k):
    for x in (9, 14, 19):
        c.poly([(x, 7), (x + 2, 1), (x + 5, 3), (x + 3, 8)], k, DEEP)


def _g_bolt(c, m, s, a):
    for i, y in enumerate((17, 7)):
        x = 4 + i * 3
        c.rect(x + 3, y, x + 22, y + 9, m); c.line(x + 3, y, x + 22, y, PEAT); c.line(x + 3, y + 9, x + 22, y + 9, PEAT)
        c.line(x + 22, y, x + 22, y + 9, PEAT); c.ell(x, y, x + 6, y + 9, a, PEAT); c.px(x + 3, y + 4, s); c.px(x + 4, y + 5, s)
        c.line(x + 9, y + 3, x + 20, y + 3, s); c.line(x + 9, y + 6, x + 20, y + 6, s)


def _g_skins(c, m, s, a):
    for i, y in enumerate((22, 17, 12, 7)):
        k = m if i % 2 == 0 else s
        c.poly([(3, y + 5), (5, y), (27, y), (29, y + 5), (26, y + 6), (22, y + 4), (16, y + 6), (10, y + 4), (6, y + 6)], k, PEAT)
    c.line(11, 7, 11, 28, a); c.line(21, 7, 21, 28, a); c.rect(15, 5, 17, 6, a)


def _g_sticks(c, m, s, a):
    for i in range(6):
        c.line(3 + i, 25 - i * 3, 24 + i, 9 - i * 2 + (i % 2), m if i % 2 == 0 else s, 2)
    c.line(9, 12, 14, 27, a, 2); c.line(19, 6, 24, 21, a, 2)
    c.line(3, 26, 9, 27, PEAT)


def _g_fishbundle(c, m, s, a):
    for i, x in enumerate((5, 10, 15, 20)):
        c.poly([(x, 6), (x + 5, 6), (x + 6, 20), (x + 3, 29), (x, 20)], m if i % 2 == 0 else s, PEAT)
        c.poly([(x + 1, 6), (x + 3, 2), (x + 5, 6)], s, PEAT); c.px(x + 2, 22, NIGHT)
    c.line(3, 9, 28, 9, a, 2)


def _g_pouch(c, m, s, a):
    c.ell(8, 11, 24, 28, m, PEAT); c.poly([(12, 12), (10, 5), (22, 5), (20, 12)], m, PEAT); c.line(11, 11, 21, 11, a)
    c.line(21, 11, 25, 15, a); c.line(13, 16, 14, 24, s)
    for x, y in ((3, 27), (6, 29), (5, 25), (27, 28), (25, 30), (2, 30)):
        c.rect(x, y, x + 1, y + 1, a)


def _g_pot(c, m, s, a):
    c.ell(6, 9, 26, 29, m, PEAT); c.rect(11, 5, 21, 10, m); c.line(10, 5, 22, 5, PEAT); c.line(10, 5, 10, 9, PEAT)
    c.line(22, 5, 22, 9, PEAT); c.ell(11, 3, 21, 7, a, PEAT); c.line(9, 15, 12, 24, s); c.line(8, 14, 23, 14, s)


def _g_jar(c, m, s, a):
    c.poly([(13, 3), (19, 3), (19, 8), (24, 13), (22, 24), (17, 30), (15, 30), (10, 24), (8, 13), (13, 8)], m, PEAT)
    c.line(12, 3, 20, 3, PEAT); c.arc(5, 6, 13, 14, 90, 270, PEAT); c.arc(19, 6, 27, 14, 270, 90, PEAT)
    c.line(11, 14, 13, 24, s); c.rect(14, 1, 18, 2, a)


def _g_flask(c, m, s, a):
    c.ell(9, 13, 23, 29, m, PEAT); c.rect(14, 5, 18, 14, m); c.line(13, 5, 13, 13, PEAT); c.line(19, 5, 19, 13, PEAT)
    c.rect(13, 3, 19, 5, TIMBER); c.ell(12, 18, 17, 24, a)


def _g_crate(c, m, s, a):
    c.rect(4, 13, 27, 28, DEARTH); c.box(4, 13, 27, 28, PEAT); c.line(4, 20, 27, 20, TIMBER)
    for x in (10, 16, 22):
        c.line(x, 13, x, 28, TIMBER)
    for i, x in enumerate((6, 13, 20)):
        c.ell(x, 5, x + 6, 14, m, PEAT); c.rect(x + 2, 3, x + 4, 6, m); c.line(x + 1, 3, x + 5, 3, PEAT); c.line(x + 1, 9, x + 5, 9, a)
    c.line(5, 14, 26, 14, PSAND)


def _g_basket(c, m, s, a):
    c.poly([(4, 13), (27, 13), (24, 29), (7, 29)], SAND, PEAT)
    for y in (17, 21, 25):
        c.line(5, y, 26, y, DEARTH)
    for x in (9, 14, 19, 23):
        c.line(x, 14, x, 28, DEARTH)
    for x, y in ((7, 9), (12, 7), (17, 8), (22, 9), (10, 11), (15, 11), (20, 11)):
        c.ell(x, y, x + 4, y + 4, a, PEAT)


def _g_wheel(c, m, s, a):
    c.ell(3, 15, 28, 28, s, PEAT); c.ell(3, 11, 28, 24, m, PEAT); c.ell(5, 4, 24, 15, s, PEAT); c.ell(5, 1, 24, 12, m, PEAT)
    c.line(9, 6, 14, 4, a)


def _g_cone(c, m, s, a):
    c.poly([(16, 2), (25, 28), (7, 28)], m, PEAT); c.line(14, 8, 10, 26, a); c.line(18, 10, 21, 26, s)
    c.line(7, 29, 25, 29, PEAT)


def _g_cakes(c, m, s, a):
    for y in (21, 14, 7):
        c.ell(4 + (y % 3), y, 27 - (y % 2), y + 9, m, PEAT); c.line(9, y + 3, 15, y + 2, a); c.line(8, y + 7, 22, y + 7, s)


def _g_discs(c, m, s, a):
    c.ell(2, 15, 22, 29, s, NIGHT); c.ell(2, 12, 22, 26, m, NIGHT); c.ell(10, 17, 14, 20, NIGHT)
    c.ell(11, 5, 30, 19, s, NIGHT); c.ell(11, 2, 30, 16, m, NIGHT); c.ell(18, 7, 22, 10, NIGHT)
    c.line(6, 16, 9, 14, a); c.line(15, 6, 18, 4, a)


def _g_disc1(c, m, s, a):
    c.ell(3, 2, 28, 29, m, NIGHT); c.ell(13, 13, 18, 18, NIGHT); c.arc(6, 5, 25, 26, 180, 300, a)
    for ang in range(0, 360, 45):
        c.line(16 + 5 * math.cos(math.radians(ang)), 16 + 5 * math.sin(math.radians(ang)),
               16 + 11 * math.cos(math.radians(ang)), 16 + 11 * math.sin(math.radians(ang)), s)


def _g_bars(c, m, s, a):
    for i, y in enumerate((22, 16, 10)):
        x = 3 + i * 3
        c.poly([(x, y + 5), (x + 3, y), (x + 23, y), (x + 20, y + 5)], m, NIGHT); c.line(x + 4, y + 1, x + 21, y + 1, a)
        c.line(x + 1, y + 4, x + 19, y + 4, s)


def _g_ingot(c, m, s, a):
    c.ell(5, 9, 27, 25, m, NIGHT); c.ell(8, 11, 18, 17, a); c.line(9, 22, 22, 22, s)


def _g_pig(c, m, s, a):
    c.poly([(3, 24), (7, 12), (25, 12), (29, 24)], m, NIGHT); c.line(8, 13, 24, 13, a); c.line(5, 23, 27, 23, s)
    c.rect(12, 16, 20, 19, s)


def _g_tusks(c, m, s, a, n=2):
    c.arc(-8, 2, 24, 44, 270, 350, m, 4); c.arc(-9, 1, 25, 45, 270, 350, PEAT, 1)
    if n > 1:
        c.arc(0, 6, 30, 46, 270, 352, m, 4); c.arc(-1, 5, 31, 47, 270, 352, PEAT, 1)
    c.rect(6, 4, 9, 6, s)


def _g_narwhal(c, m, s, a):
    c.line(3, 28, 28, 3, m, 3); c.line(2, 28, 27, 3, PEAT)
    for i in range(0, 24, 3):
        c.line(5 + i, 27 - i, 6 + i, 24 - i, s)


def _g_piece(c, m, s, a):
    c.poly([(5, 24), (9, 8), (23, 6), (27, 22), (16, 28)], m, PEAT); c.ell(12, 9, 20, 14, s, PEAT); c.line(8, 22, 10, 12, a)


def _g_antler(c, m, s, a):
    c.line(6, 28, 16, 14, m, 3); c.line(16, 14, 26, 6, m, 2); c.line(12, 19, 6, 10, m, 2); c.line(18, 12, 14, 3, m, 2)
    c.line(22, 9, 28, 14, m, 2); c.line(9, 25, 20, 27, s, 2); c.line(20, 27, 27, 22, s, 2); c.px(6, 9, PEAT)


def _g_comb(c, m, s, a):
    c.rect(3, 8, 28, 13, m); c.box(3, 8, 28, 13, PEAT)
    for x in range(5, 28, 2):
        c.line(x, 14, x, 24, m)
    for x in (8, 14, 20, 25):
        c.px(x, 10, RUST); c.px(x, 11, RUST)
    c.line(3, 14, 28, 14, s)


def _g_brooches(c, m, s, a):
    for x in (2, 16):
        c.ell(x, 9, x + 13, 22, GOLD, PEAT); c.ell(x + 4, 13, x + 9, 18, PGOLD, DEARTH); c.px(x + 6, 15, BLOOD)
        c.px(x + 7, 16, BLOOD)
    c.line(9, 6, 22, 6, DEARTH)


def _g_string(c, m, s, a):
    c.arc(2, -14, 29, 22, 15, 165, DEARTH)
    pts = [(3, 9), (6, 14), (10, 18), (14, 20), (18, 20), (22, 18), (26, 14), (28, 9)]
    cols = [m, a, m, s, m, a, m, s]
    for (x, y), k in zip(pts, cols):
        c.ell(x - 2, y - 2, x + 2, y + 2, k, PEAT)
    c.ell(13, 23, 19, 29, a, PEAT)


def _g_coil(c, m, s, a):
    for i, k in enumerate((m, s, m, s)):
        c.ell(3 + i * 3, 7 + i * 2, 28 - i * 3, 28 - i * 2, None, k); c.ell(4 + i * 3, 8 + i * 2, 27 - i * 3, 27 - i * 2, None, k)
    c.ell(3, 7, 28, 28, None, PEAT); c.line(24, 9, 29, 3, m, 2)


def _g_planks(c, m, s, a):
    for i, y in enumerate((23, 18, 13, 8)):
        x = 2 + i * 2
        c.rect(x, y, x + 22, y + 4, m if i % 2 == 0 else a); c.box(x, y, x + 22, y + 4, PEAT)
        c.line(x + 3, y + 2, x + 14, y + 2, s)


def _g_pole(c, m, s, a):
    c.line(2, 28, 28, 4, m, 4); c.line(1, 27, 27, 3, PEAT); c.line(4, 30, 30, 6, s)
    c.ell(25, 2, 30, 7, a, PEAT)


def _g_sword(c, m, s, a):
    c.poly([(14, 2), (18, 2), (18, 21), (16, 23), (14, 21)], m, SLATE); c.line(16, 3, 16, 20, CWHITE)
    c.rect(10, 21, 22, 23, a); c.box(10, 21, 22, 23, PEAT); c.rect(15, 24, 17, 27, TIMBER); c.rect(13, 28, 19, 30, a)
    c.box(13, 28, 19, 30, PEAT)


def _g_blade(c, m, s, a):
    c.poly([(14, 1), (18, 1), (18, 24), (16, 26), (14, 24)], m, SLATE); c.line(16, 2, 16, 23, CWHITE)
    c.rect(15, 26, 17, 30, s); c.line(15, 9, 17, 9, SLATE); c.line(15, 11, 17, 11, SLATE); c.line(15, 13, 17, 13, SLATE)


def _g_spearheads(c, m, s, a):
    for i, x in enumerate((4, 11, 18)):
        c.poly([(x + 4, 3), (x + 8, 14), (x + 5, 20), (x + 5, 28), (x + 3, 28), (x + 3, 20), (x, 14)], m, SLATE)
        c.line(x + 4, 5, x + 4, 18, CWHITE)
    c.line(2, 24, 29, 24, DEARTH, 2)


def _g_axes(c, m, s, a):
    for i, x in enumerate((2, 12)):
        c.line(x + 6, 29, x + 9, 4, TIMBER, 2); c.poly([(x + 9, 4), (x + 17, 3), (x + 18, 13), (x + 10, 10)], m, SLATE)
        c.line(x + 16, 4, x + 17, 12, CWHITE)


def _g_mail(c, m, s, a):
    c.poly([(3, 9), (10, 5), (22, 5), (29, 9), (27, 16), (23, 14), (23, 29), (9, 29), (9, 14), (5, 16)], SLATE, NIGHT)
    c.dither(10, 6, 22, 28, m); c.dither(5, 9, 9, 14, m); c.dither(23, 9, 27, 14, m)
    c.poly([(13, 5), (19, 5), (16, 9)], NIGHT)


def _g_helmet(c, m, s, a):
    c.poly([(5, 20), (7, 10), (16, 3), (25, 10), (27, 20)], m, SLATE); c.rect(4, 20, 28, 22, s)
    c.rect(15, 20, 17, 28, m); c.line(16, 4, 16, 19, SLATE); c.line(10, 9, 8, 18, CWHITE); c.box(4, 20, 28, 22, NIGHT)


def _g_shield(c, m, s, a):
    c.ell(2, 2, 29, 29, TIMBER, NIGHT); c.ell(4, 4, 27, 27, DEARTH, TIMBER)
    for x in (10, 16, 22):
        c.line(x, 5, x, 26, TIMBER)
    c.ell(11, 11, 20, 20, m, SLATE); c.ell(14, 14, 17, 17, CWHITE)


def _g_bows(c, m, s, a):
    c.arc(-10, 2, 16, 29, 290, 70, TIMBER, 2); c.line(11, 4, 11, 27, PSTONE)
    for x in (18, 22, 26):
        c.line(x, 5, x, 28, DEARTH); c.poly([(x - 2, 8), (x, 3), (x + 2, 8)], PSTONE, SLATE); c.line(x - 1, 26, x + 1, 28, CWHITE)


def _g_person(c, m, s, a):
    c.rect(13, 3, 18, 9, a); c.rect(13, 3, 18, 4, TIMBER); c.poly([(9, 11), (22, 11), (24, 24), (7, 24)], m, NIGHT)
    c.rect(11, 25, 14, 30, s); c.rect(17, 25, 20, 30, s); c.rect(12, 16, 19, 19, a); c.line(11, 17, 20, 17, SAND)
    c.line(11, 19, 20, 19, SAND); c.line(20, 18, 28, 14, SAND)


def _g_book(c, m, s, a):
    c.poly([(5, 6), (24, 4), (27, 26), (8, 29)], m, NIGHT); c.poly([(24, 4), (27, 5), (29, 26), (27, 26)], PSAND, PEAT)
    c.poly([(8, 29), (27, 26), (29, 26), (10, 30)], PSAND, PEAT); c.line(8, 7, 10, 28, s)
    c.rect(15, 10, 17, 22, a); c.rect(12, 13, 21, 15, a); c.px(25, 15, a)


def _g_leaves(c, m, s, a):
    for i in range(4):
        c.rect(6 + i * 2, 9 - i * 2, 23 + i * 2, 29 - i * 2, PSAND if i < 3 else WWHITE); c.box(6 + i * 2, 9 - i * 2, 23 + i * 2, 29 - i * 2, DEARTH)
    for y in (8, 11, 14, 17, 20):
        c.line(15, y, 26, y, TIMBER)
    c.rect(15, 5, 17, 7, BLOOD)


def _g_roll(c, m, s, a):
    c.rect(6, 10, 26, 22, PSAND); c.line(6, 10, 26, 10, DEARTH); c.line(6, 22, 26, 22, DEARTH)
    c.ell(2, 9, 9, 23, WWHITE, DEARTH); c.ell(23, 9, 30, 23, SAND, DEARTH); c.ell(4, 13, 7, 19, DEARTH)
    for y in (13, 16, 19):
        c.line(11, y, 22, y, TIMBER)
    c.line(14, 23, 12, 29, a); c.line(17, 23, 19, 29, a)


def _g_reliquary(c, m, s, a):
    c.rect(4, 15, 27, 27, GOLD); c.poly([(4, 15), (8, 6), (23, 6), (27, 15)], PGOLD, PEAT); c.box(4, 15, 27, 27, PEAT)
    c.rect(15, 2, 16, 6, GOLD); c.rect(13, 3, 18, 4, GOLD)
    for x, k in ((8, BLOOD), (15, BYZ), (22, BLOOD)):
        c.rect(x, 19, x + 2, 22, k)
    c.line(9, 10, 22, 10, GOLD); c.rect(6, 28, 8, 30, DEARTH); c.rect(23, 28, 25, 30, DEARTH)


def _g_chalice(c, m, s, a):
    c.poly([(7, 4), (24, 4), (21, 14), (10, 14)], GOLD, PEAT); c.rect(14, 15, 17, 22, GOLD); c.ell(12, 16, 19, 20, PGOLD, PEAT)
    c.poly([(9, 28), (13, 22), (18, 22), (22, 28)], GOLD, PEAT); c.line(9, 6, 22, 6, PGOLD); c.px(15, 10, BLOOD)


def _g_pod(c, m, s, a):
    c.ell(9, 8, 23, 26, m, NIGHT); c.line(16, 3, 16, 8, s); c.line(12, 12, 14, 22, a); c.px(19, 14, a)


def _g_skin(c, m, s, a):
    c.poly([(11, 2), (20, 2), (22, 7), (29, 6), (26, 14), (27, 22), (30, 29), (22, 27), (16, 30), (9, 27), (1, 29), (4, 22),
            (5, 14), (2, 6), (9, 7)], m, NIGHT)
    c.line(16, 5, 16, 26, s); c.line(9, 12, 12, 20, a); c.line(22, 12, 20, 20, a); c.px(13, 4, a); c.px(18, 4, a)


def _g_cattle(c, m, s, a):
    c.rect(6, 11, 25, 21, m); c.rect(2, 9, 8, 15, m); c.rect(7, 22, 9, 29, s); c.rect(21, 22, 23, 29, s)
    c.rect(11, 22, 12, 28, s); c.rect(18, 22, 19, 28, s); c.line(2, 8, 0, 5, a); c.line(6, 8, 8, 5, a); c.px(3, 11, NIGHT)
    c.rect(13, 12, 19, 16, a); c.line(26, 12, 28, 20, s)


def _g_sheep(c, m, s, a):
    c.ell(6, 9, 27, 23, CWHITE, PSTONE); c.ell(8, 7, 16, 14, CWHITE, PSTONE); c.ell(15, 7, 25, 15, CWHITE, PSTONE)
    c.rect(1, 11, 7, 17, PEAT); c.px(3, 13, CWHITE); c.rect(9, 23, 10, 29, PEAT); c.rect(21, 23, 22, 29, PEAT)
    c.rect(13, 23, 14, 28, PEAT); c.rect(18, 23, 19, 28, PEAT)


def _g_pigbeast(c, m, s, a):
    c.ell(5, 11, 27, 24, DROSE, RUST); c.rect(1, 14, 7, 19, DROSE); c.rect(0, 16, 1, 18, RUST); c.px(4, 15, NIGHT)
    c.poly([(5, 11), (7, 8), (9, 12)], RUST)
    for x in (9, 13, 19, 23):
        c.rect(x, 24, x + 1, 29, RUST)
    c.line(27, 14, 30, 12, RUST); c.line(10, 11, 22, 11, WEARTH)


def _g_horse(c, m, s, a):
    c.rect(8, 12, 25, 20, m); c.poly([(8, 12), (3, 3), (7, 2), (12, 12)], m); c.rect(1, 3, 6, 7, m); c.px(3, 4, NIGHT)
    c.line(6, 2, 11, 11, NIGHT, 1); c.line(7, 1, 12, 10, s)
    for x in (9, 13, 20, 24):
        c.rect(x, 21, x + 1, 29, s)
    c.line(26, 12, 29, 22, s, 2)


def _g_falcon(c, m, s, a):
    c.poly([(11, 4), (17, 3), (21, 9), (23, 22), (17, 24), (12, 18)], m, NIGHT); c.poly([(11, 5), (7, 8), (12, 9)], GOLD)
    c.px(14, 6, NIGHT); c.line(16, 11, 20, 21, a); c.line(18, 10, 22, 19, s); c.rect(16, 25, 17, 27, GOLD); c.rect(20, 25, 21, 27, GOLD)
    c.rect(9, 28, 26, 30, DEARTH); c.poly([(11, 3), (13, 1), (16, 2)], WEARTH)


def _g_hound(c, m, s, a):
    c.rect(8, 13, 24, 19, m); c.poly([(8, 13), (5, 6), (9, 5), (12, 13)], m); c.rect(1, 6, 6, 10, m); c.px(3, 7, NIGHT)
    c.poly([(6, 5), (8, 2), (9, 6)], s)
    for x in (9, 12, 21, 24):
        c.rect(x, 20, x, 29, s)
    c.line(25, 13, 29, 8, m); c.rect(7, 11, 10, 12, a)


GOOD_SHAPES = {
    "sack": _g_sack, "cask": _g_cask, "barrel": _g_barrel, "bolt": _g_bolt, "skins": _g_skins, "sticks": _g_sticks,
    "fishbundle": _g_fishbundle, "pouch": _g_pouch, "pot": _g_pot, "jar": _g_jar, "flask": _g_flask, "crate": _g_crate,
    "basket": _g_basket, "wheel": _g_wheel, "cone": _g_cone, "cakes": _g_cakes, "discs": _g_discs, "disc1": _g_disc1,
    "bars": _g_bars, "ingot": _g_ingot, "pig": _g_pig, "tusks": _g_tusks, "narwhal": _g_narwhal, "piece": _g_piece,
    "antler": _g_antler, "comb": _g_comb, "brooches": _g_brooches, "string": _g_string, "coil": _g_coil,
    "planks": _g_planks, "pole": _g_pole, "sword": _g_sword, "blade": _g_blade, "spearheads": _g_spearheads,
    "axes": _g_axes, "mail": _g_mail, "helmet": _g_helmet, "shield": _g_shield, "bows": _g_bows, "person": _g_person,
    "book": _g_book, "leaves": _g_leaves, "roll": _g_roll, "reliquary": _g_reliquary, "chalice": _g_chalice,
    "pod": _g_pod, "skin": _g_skin, "cattle": _g_cattle, "sheep": _g_sheep, "pigbeast": _g_pigbeast, "horse": _g_horse,
    "falcon": _g_falcon, "hound": _g_hound,
}


def good_shape(g):
    """Which shape a good's icon takes: from how it is traded (its unit), then its kind."""
    gid = (g.get("id") or "").lower()
    unit = (g.get("unit") or "").lower()
    cat = g.get("cat") or ""
    by_id = (("cattle", "cattle"), ("sheep", "sheep"), ("pigs", "pigbeast"), ("horse", "horse"), ("falcon", "falcon"),
             ("hound", "hound"), ("antler", "antler"), ("comb", "comb"), ("brooch", "brooches"), ("narwhal", "narwhal"),
             ("stockfish", "fishbundle"), ("dried_fish", "fishbundle"), ("church_plate", "chalice"), ("bows", "bows"),
             ("spearhead", "spearheads"), ("axes", "axes"), ("millstone", "disc1"), ("mast", "pole"))
    for k, sh in by_id:
        if k in gid:
            return sh
    by_unit = (("cask", "cask"), ("keg", "cask"), ("barrel", "barrel"), ("bolt", "bolt"), ("timber (", "skins"), ("skin", "skin"),
               ("sack", "sack"), ("pouch", "pouch"), ("pound", "pouch"), ("crock", "pot"), ("pot", "pot"), ("jar", "jar"),
               ("flask", "flask"), ("crate", "crate"), ("basket", "basket"), ("wheel", "wheel"), ("loaf", "cone"),
               ("cake", "cakes"), ("pair of tusks", "tusks"), ("tusk", "narwhal"), ("pair", "discs"), ("stone", "disc1"),
               ("ingot", "ingot"), ("pig", "pig"), ("bar", "bars"), ("string", "string"), ("coil", "coil"),
               ("plank", "planks"), ("mast", "pole"), ("sword", "sword"), ("blade", "blade"), ("mail", "mail"),
               ("helmet", "helmet"), ("shield", "shield"), ("person", "person"), ("book", "book"), ("leaves", "leaves"),
               ("roll", "roll"), ("relic", "reliquary"), ("pod", "pod"), ("piece", "piece"), ("dozen", "comb"),
               ("head", "cattle"), ("bird", "falcon"), ("hound", "hound"))
    for k, sh in by_unit:
        if k in unit:
            return sh
    if "bundle" in unit:
        return "skins" if cat in ("Furs", "Hides", "Fibres") and "flax" not in gid and "hemp" not in gid else "sticks"
    return {"Food": "sack", "Drink": "cask", "Provisions": "sack", "Spices": "pouch", "Textiles": "bolt", "Fibres": "sack",
            "Dyes": "sack", "Furs": "skins", "Hides": "skins", "Ship stores": "coil", "Ivory and bone": "piece",
            "Beads and gems": "string", "Crafts": "crate", "Stone": "disc1", "Pottery": "crate", "Glass": "crate",
            "Metals": "bars", "Weapons": "sword", "Books and church goods": "book", "Animals": "falcon",
            "Livestock": "cattle", "Timber": "planks", "Captives": "person"}.get(cat, "crate")


def draw_good(g, w=32, h=32):
    """A good's icon: the shape of how it is traded (a cask, a sack, a bolt, a bundle of skins), coloured by its kind."""
    cv = Cv(32, 32)
    gid = (g.get("id") or "").lower()
    m, s, a = CAT_COL.get(g.get("cat") or "", (SAND, DEARTH, GOLD))
    best = ""
    for k in GOOD_COL:
        if k in gid and len(k) > len(best):
            best = k
    if best:
        m2, s2, a2 = GOOD_COL[best]
        m, s, a = m2 or m, s2 or s, a2 or a
    shape = good_shape(g)
    fn = GOOD_SHAPES[shape]
    if shape == "tusks":
        fn(cv, m, s, a, 2)
    else:
        fn(cv, m, s, a)
    if shape == "barrel" and any(k in gid for k in ("herring", "salmon", "eel", "fish")):
        _g_fish_top(cv, a)
    if (w, h) != (32, 32):
        k = max(1, min(w // 32, h // 32))
        big = Cv(w, h)
        big.blit(cv.scaled(k), (w - 32 * k) // 2, (h - 32 * k) // 2)
        cv = big
    placeholder_mark(cv)
    return cv


# ================================================================================================ ships
def ship_traits(c):
    """What the drawing needs from a ship class, read from its numbers and the plain words of its 'look'."""
    look = " ".join(str(c.get(k) or "") for k in ("look", "name", "id", "sail", "picture")).lower()
    L = num(c.get("length_m"), 10.0) or 10.0
    B = num(c.get("beam_m"), max(1.2, L / 4.5)) or max(1.2, L / 4.5)
    fb = num(c.get("freeboard_m"), 0.5) or 0.5
    top = num(c.get("hull_top_m"), fb + 0.6) or (fb + 0.6)
    kind = "clinker"
    if "raft" in look:
        kind = "raft"
    elif "logboat" in look or "dugout" in look or "hollowed" in look:
        kind = "log"
    elif c.get("flat_bottom") and ("barge" in look or "punt" in look or "ferry" in look or "box" in look):
        kind = "barge"
    elif "hide" in look or "currach" in look:
        kind = "hide"
    mast = bool(c.get("mast")) and c.get("sail_top_m") is not None
    sail = str(c.get("sail") or "").lower()
    return {"L": L, "B": B, "fb": fb, "top": top, "kind": kind, "mast": mast, "sail_top": num(c.get("sail_top_m"), L * 0.65),
            "square": "square" in sail and "towing" not in sail, "towing": "towing" in sail,
            "oars": int(num(c.get("oars"), 0)), "crew": int(num(c.get("crew"), 2)),
            "shields": "shields along" in look and "no shields" not in look, "cabin": "cabin" in look or "awning" in look,
            "cargo": "cargo heaped" in look or "cart" in look, "war": "warship" in look or "raider" in look or "war," in look}


def _hull_side(cv, x0, yw, Lp, t, ppm, hullc, shade, detail=True):
    """One hull from the side, bow to the right, on the waterline yw."""
    fbp = max(2, int(round(t["fb"] * ppm)))
    stem = max(0, int(round((t["top"] - t["fb"]) * ppm)))
    kind = t["kind"]
    if kind == "raft":
        cv.rect(x0, yw - 2, x0 + Lp - 1, yw, hullc)
        for x in range(x0, x0 + Lp, 4):
            cv.px(x, yw - 1, shade)
        cv.line(x0, yw - 3, x0 + Lp - 1, yw - 3, SAND)
        return fbp, 0
    cols = []
    for i in range(Lp):
        u = abs(2.0 * i / max(1, Lp - 1) - 1.0)
        if kind in ("barge", "log"):
            rise = 0
            cut = int(round(fbp * max(0.0, (u - 0.86) / 0.14)))
        else:
            rise = int(round(stem * u ** 3.2))
            cut = int(round((fbp + rise) * max(0.0, (u - 0.9) / 0.1) ** 1.6))
        cols.append((yw - fbp - rise, yw - cut))
    for i, (ya, yb) in enumerate(cols):
        if yb >= ya:
            cv.line(x0 + i, ya, x0 + i, yb, hullc)
    if detail and kind in ("clinker", "barge"):
        for k in range(2, fbp + 1, 2):                       # strakes follow the sheer
            for i, (ya, yb) in enumerate(cols):
                y = ya + k
                if y <= yb:
                    cv.px(x0 + i, y, shade)
    for i, (ya, yb) in enumerate(cols):                      # the gunwale
        if yb >= ya:
            cv.px(x0 + i, ya, PEAT)
    if kind in ("clinker", "hide") and stem > 0:
        cv.line(x0, cols[0][0], x0, cols[0][0] - 1, PEAT); cv.line(x0 + Lp - 1, cols[-1][0], x0 + Lp - 1, cols[-1][0] - 1, PEAT)
    return fbp, stem


def draw_ship_side(c, w=160, h=96, ppm=6.0, label=None):
    """A ship from the side, bow to the right, every class at the same scale so they can be compared."""
    cv = Cv(w, h)
    t = ship_traits(c)
    Lp = int(max(12, min(w - 8, round(t["L"] * ppm))))
    yw = h - 14
    x0 = (w - Lp) // 2
    hullc, shade = (PEAT, NIGHT) if t["kind"] == "hide" else ((DEARTH, TIMBER) if t["kind"] in ("raft", "log") else (TIMBER, WEARTH))
    # the water
    cv.rect(0, yw + 1, w - 1, h - 1, SEA); cv.rect(0, yw + 1, w - 1, yw + 2, SHOAL)
    r = rng_of("ship:" + str(c.get("id")))
    for _ in range(w // 6):
        x, y = r.randint(0, w - 6), r.randint(yw + 3, h - 2)
        cv.line(x, y, x + r.randint(2, 5), y, SHALL if y < yw + 8 else SHOAL)
    mx = x0 + (Lp // 2 if not t["towing"] else int(Lp * 0.72))
    fbp = max(2, int(round(t["fb"] * ppm)))
    if t["mast"]:
        mh = int(round(t["sail_top"] * ppm))
        mtop = max(3, yw - mh)
        if t["square"]:
            sw_ = int(Lp * (0.62 if t["L"] > 12 else 0.7)); sh_ = max(6, int((yw - fbp - mtop) * 0.62))
            sx = mx - sw_ // 2
            cv.rect(sx, mtop + 3, sx + sw_ - 1, mtop + 3 + sh_, PSAND)
            for x in range(sx + 6, sx + sw_ - 1, 7):
                cv.line(x, mtop + 4, x, mtop + 2 + sh_, SAND)
            cv.box(sx, mtop + 3, sx + sw_ - 1, mtop + 3 + sh_, DEARTH)
            cv.line(sx - 2, mtop + 2, sx + sw_ + 1, mtop + 2, WEARTH)
            cv.line(sx, mtop + 3 + sh_, x0 + 3, yw - fbp - 1, DEARTH); cv.line(sx + sw_ - 1, mtop + 3 + sh_, x0 + Lp - 4, yw - fbp - 1, DEARTH)
        elif t["towing"]:
            cv.line(mx, mtop, w - 1, yw - fbp - 6, SAND)
        cv.line(mx, mtop, mx, yw - 1, WEARTH); cv.line(mx + 1, mtop + 2, mx + 1, yw - 1, TIMBER)
        cv.line(mx, mtop, x0 + Lp - 2, yw - fbp - 2, PEAT) if t["square"] else None
    fbp, stem = _hull_side(cv, x0, yw, Lp, t, ppm, hullc, shade)
    top_mid = yw - fbp
    if t["cabin"]:
        cx0 = x0 + int(Lp * 0.25)
        cv.rect(cx0, top_mid - 8, cx0 + int(Lp * 0.3), top_mid - 1, SAND); cv.box(cx0, top_mid - 8, cx0 + int(Lp * 0.3), top_mid - 1, PEAT)
        cv.line(cx0 - 1, top_mid - 9, cx0 + int(Lp * 0.3) + 1, top_mid - 9, DEARTH)
    if t["cargo"]:
        cx0 = x0 + int(Lp * 0.35)
        cv.poly([(cx0, top_mid - 1), (cx0 + 4, top_mid - 6), (cx0 + int(Lp * 0.22), top_mid - 6), (cx0 + int(Lp * 0.26), top_mid - 1)], SAND, PEAT)
    # oar ports or oars
    n_side = t["oars"] // 2
    if n_side and t["kind"] == "clinker":
        a, bq = x0 + int(Lp * 0.2), x0 + int(Lp * 0.8)
        for k in range(n_side):
            ox = a + int((bq - a) * (k + 0.5) / n_side)
            if t["shields"]:
                cv.ell(ox - 2, top_mid - 1, ox + 2, top_mid + 3, r.choice((GOLD, BLOOD, PSTONE, DEARTH)), PEAT)
            else:
                cv.px(ox, top_mid + 2, NIGHT)
                if n_side <= 8:
                    cv.line(ox, top_mid + 2, ox - 5, yw + 4, DEARTH)
    elif n_side:
        for k in range(min(n_side, 3)):
            ox = x0 + int(Lp * (0.3 + 0.2 * k))
            cv.line(ox, top_mid, ox - 4, yw + 4, DEARTH)
    # men
    heads = min(max(1, t["crew"]), max(1, Lp // 9), 9)
    for k in range(heads):
        hx = x0 + int(Lp * (0.16 + 0.68 * (k + 0.5) / heads))
        if t["mast"] and abs(hx - mx) < 3:
            hx += 4
        cv.rect(hx, top_mid - 5, hx + 2, top_mid - 3, PEACH); cv.rect(hx, top_mid - 6, hx + 2, top_mid - 6, r.choice((TIMBER, PGOLD, PEAT, DEARTH)))
        cv.rect(hx - 1, top_mid - 2, hx + 3, top_mid - 1, r.choice((SHOAL, DEARTH, STONE, RUST, MEADOW)))
    if t["kind"] in ("clinker", "hide", "barge"):               # the side rudder, on the quarter
        rx = x0 + int(Lp * 0.1)
        cv.line(rx, top_mid - 2, rx - 2, yw + 5, WEARTH); cv.line(rx - 1, yw, rx - 3, yw + 5, WEARTH)
    cv.line(max(0, x0 - 2), yw + 1, min(w - 1, x0 + Lp + 1), yw + 1, WAVE)
    # a rule in metres
    cv.line(4, h - 3, 4 + int(5 * ppm), h - 3, FOAM); cv.line(4, h - 4, 4, h - 2, FOAM); cv.line(4 + int(5 * ppm), h - 4, 4 + int(5 * ppm), h - 2, FOAM)
    cv.text(4 + int(5 * ppm) + 3, h - 6, "5 M", FOAM)
    if label:
        cv.text_fit(4 + int(5 * ppm) + 20, h - 6, label, PGOLD, w - (4 + int(5 * ppm) + 24))
    placeholder_mark(cv)
    return cv


def draw_ship_mark(c, w=32, h=16, ppm=1.25):
    """A ship from above for the chart, bow to the right (the game turns it)."""
    cv = Cv(w, h)
    t = ship_traits(c)
    Lp = int(max(6, min(w - 2, round(t["L"] * ppm))))
    Bp = int(max(3, min(h - 6, round(t["B"] * ppm + 1.5))))
    if Bp % 2 == 0:
        Bp += 1
    x0, cy = (w - Lp) // 2, h // 2
    hullc = PEAT if t["kind"] == "hide" else (DEARTH if t["kind"] in ("raft", "log") else TIMBER)
    half = Bp // 2
    for i in range(Lp):
        u = abs(2.0 * i / max(1, Lp - 1) - 1.0)
        if t["kind"] in ("raft", "barge"):
            hw_ = half if u < 0.9 else max(0, half - 1)
        else:
            hw_ = int(round(half * (1 - u ** 2.2)))
        cv.line(x0 + i, cy - hw_, x0 + i, cy + hw_, hullc)
    inner = PSAND if t["kind"] != "raft" else SAND
    if Lp >= 10 and half >= 1 and t["kind"] != "raft":
        for i in range(2, Lp - 2):
            u = abs(2.0 * i / max(1, Lp - 1) - 1.0)
            hw_ = int(round(half * (1 - u ** 2.2))) - 1
            if hw_ >= 0:
                cv.line(x0 + i, cy - hw_, x0 + i, cy + hw_, inner if hw_ > 0 or half == 1 else hullc)
        if half == 1:
            cv.line(x0 + 2, cy, x0 + Lp - 3, cy, inner)
    if t["oars"] >= 12:
        for i in range(x0 + 3, x0 + Lp - 3, 3):
            cv.px(i, cy - half - 1, DEARTH); cv.px(i, cy + half + 1, DEARTH)
    if t["mast"]:
        mx = x0 + (Lp // 2 if not t["towing"] else int(Lp * 0.72))
        if t["square"]:
            cv.line(mx, max(0, cy - half - 2), mx, min(h - 1, cy + half + 2), CWHITE)
            cv.line(mx + 1, max(1, cy - half - 1), mx + 1, min(h - 2, cy + half + 1), PSAND)
        cv.px(mx, cy, WEARTH)
    if t["shields"]:
        for i in range(x0 + 3, x0 + Lp - 3, 2):
            cv.px(i, cy - half, GOLD if (i // 2) % 2 else BLOOD); cv.px(i, cy + half, GOLD if (i // 2) % 2 else BLOOD)
    cv.px(x0 + Lp - 1, cy, PEAT)
    cv.outline_all(NIGHT)
    placeholder_mark(cv)
    return cv


# ================================================================================================ icons (acts, upgrades, games, works)
ICON_OF = {  # an act's sign, and a smaller second sign where two acts would look alike
    "load": ("crate", None), "repair": ("hammer", "hull"), "careen": ("hull", "tar"), "hire_porters": ("sack", "person"),
    "hire_pilot": ("pole", "person"), "wait_wind": ("wind", None), "ask_harbour": ("mooring", "speech"),
    "haggle": ("scales", "speech"), "weigh_silver": ("scales", "coin"), "sample": ("cup", "eye"), "pay_toll": ("tally", "coin"),
    "show_charter": ("letter", None), "change_coin": ("coins", None), "test_silver": ("coin", "eye"), "alms": ("bowl", "coin"),
    "hear_mass": ("cross", None), "ask_news": ("speech", None), "read_text": ("book", "eye"), "copy_text": ("quill", "leaf"),
    "vow": ("candle", None), "sanctuary": ("church", None), "bury": ("mound", "cross"), "remember": ("candle", "heart"),
    "offer": ("post", "bowl"), "ask_omen": ("bird", None), "gift": ("ring", None), "feast": ("horn", None),
    "oath": ("ring", "thor"), "audience": ("circlet", None), "lawsuit": ("staff", "scales"), "witness": ("eye", None),
    "hear_law": ("staff", "speech"), "rumours": ("speech", "cup"), "hire_crew": ("oar", "person"), "dice": ("dice", None),
    "tafl": ("tafl", None), "song": ("lyre", None), "riddles": ("runestick", "question"), "drink": ("cup", None),
    "buy_tools": ("axe", "coin"), "commission": ("tally", "letter"), "buy_ship": ("sail", "coin"),
    "build_ship": ("ribs", None), "fit_upgrade": ("hull", "hammer"), "store": ("store", None),
    "rent_store": ("store", "coin"), "buy_plot": ("plot", None), "lodge": ("bed", None), "heal": ("herb", None),
    "provision": ("loaf", None), "buy_at_source": ("sack", "coin"), "cross": ("ferry", None), "fund_work": ("coins", "house"),
    "market": ("stall", None),
}
ICON_WORDS = [  # for things with no sign of their own: the first word found in the id, then the name, then the text
    ("sail", "sail"), ("mast", "mast"), ("oar", "oar"), ("rudder", "oar"), ("steer", "oar"), ("rope", "rope"), ("rigging", "rope"),
    ("anchor", "mooring"), ("tar", "tar"), ("caulk", "tar"), ("pitch", "tar"), ("plank", "plank"), ("strake", "plank"),
    ("keel", "ribs"), ("rib", "ribs"), ("frame", "ribs"), ("hull", "hull"), ("deck", "plank"), ("awning", "stall"),
    ("tent", "stall"), ("chest", "chest"), ("lock", "key"), ("shield", "shield"), ("spear", "spear"), ("sword", "sword"),
    ("axe", "axe"), ("bow", "bow"), ("mail", "shield"), ("pump", "well"), ("bail", "bowl"), ("cask", "cask"), ("water", "cask"),
    ("bread", "loaf"), ("oven", "fire"), ("hearth", "fire"), ("fire", "fire"), ("beacon", "fire"), ("light", "candle"),
    ("lantern", "candle"), ("bell", "bell"), ("cross", "cross"), ("relic", "casket"), ("altar", "cross"), ("church", "church"),
    ("chapel", "church"), ("hospice", "bed"), ("lodging", "bed"), ("bed", "bed"), ("jetty", "bridge"), ("causeway", "bridge"),
    ("bridge", "bridge"), ("quay", "bridge"), ("storehouse", "store"), ("store", "store"), ("granary", "store"),
    ("well", "well"), ("wall", "gate"), ("gate", "gate"), ("palisade", "gate"), ("mint", "coins"), ("market", "stall"),
    ("stall", "stall"), ("booth", "stall"), ("hall", "house"), ("house", "house"), ("plot", "plot"), ("school", "book"),
    ("book", "book"), ("ferry", "ferry"), ("road", "cart"), ("cart", "cart"), ("mill", "quern"), ("quern", "quern"),
    ("dice", "dice"), ("tafl", "tafl"), ("board", "tafl"), ("riddle", "runestick"), ("song", "lyre"), ("harp", "lyre"),
    ("lyre", "lyre"), ("drink", "horn"), ("ale", "cup"), ("wrestl", "person"), ("race", "hull"), ("wager", "coins"),
    ("bet", "coins"), ("lot", "runestick"), ("flyting", "speech"), ("boast", "speech"), ("knuckle", "dice"),
    ("hnefatafl", "tafl"), ("merels", "tafl"), ("pilot", "pole"), ("sounding", "pole"), ("lead", "pole"),
    ("lookout", "eye"), ("net", "net"), ("fish", "fish"), ("line", "rope"), ("silver", "coins"), ("coin", "coin"),
    ("scale", "scales"), ("weigh", "scales"), ("toll", "tally"), ("charter", "letter"), ("letter", "letter"),
    ("ship", "sail"), ("boat", "hull"), ("crew", "person"), ("bench", "plank"), ("thwart", "plank"), ("iron", "anvil"),
    ("rivet", "hammer"), ("smith", "anvil"), ("wool", "bolt"), ("cloth", "bolt"), ("salt", "salt"), ("grave", "mound"),
    ("mound", "mound"), ("tree", "tree"), ("grove", "tree"), ("shrine", "post"), ("idol", "post"), ("horse", "cart"),
    ("field", "sheaf"), ("grain", "sheaf"), ("flag", "flag"), ("banner", "flag"), ("weather", "wind"), ("vane", "wind"),
]


def sign_for(ident, name="", text=""):
    """The sign that best says what a thing is: its own, else the first known word in its id, name or text."""
    i = (ident or "").lower().split(".")[-1]
    if i in ICON_OF:
        return ICON_OF[i]
    if i in GLYPHS:
        return (i, None)
    for src in (i, (name or "").lower(), (text or "").lower()):
        for wd, g in ICON_WORDS:
            if wd in src:
                return (g, None)
    return (None, None)


def draw_icon(ident, name="", text="", w=32, h=32, plate=None, tag=None):
    """A 32 x 32 sign for an act, an upgrade or a game: one large sign, a small second one to tell it from its twin."""
    cv = Cv(w, h)
    main, minor = sign_for(ident, name, text)
    k = max(1, min(w, h) // 16)
    if plate:
        cv.rect(1, 1, w - 2, h - 2, plate); cv.box(0, 0, w - 1, h - 1, NIGHT)
    if main:
        cv.blit(glyph(main, k, None if plate else NIGHT), (w - 16 * k) // 2, (h - 16 * k) // 2)
        if minor:
            cv.rect(w - 16 - 1, h - 16 - 1, w - 1, h - 1, NIGHT)
            cv.blit(glyph(minor, 1), w - 16, h - 16)
            cv.box(w - 17, h - 17, w - 1, h - 1, PGOLD)
    else:
        r = rng_of("icon:" + str(ident))
        cv.rect(1, 1, w - 2, h - 2, r.choice((DEEP, PLUM, WSHADE, WEARTH, SLATE))); cv.box(0, 0, w - 1, h - 1, PGOLD)
        letters = (ident or "?").split(".")[-1].replace("_", "")[:3]
        cv.text_mid(w // 2, h // 2 - 5, letters, PGOLD, 2)
    if tag:                                                  # letters that tell it from the others with the same sign
        tag = ascii_up(tag)[:5]
        cv.rect(0, h - 7, 4 * len(tag) + 1, h - 1, NIGHT)
        cv.text(1, h - 6, tag, PGOLD)
    placeholder_mark(cv)
    return cv


def draw_work(wk, w=64, h=64):
    """A town work (a jetty, a bell, a well): its sign, large, on a patch of ground."""
    cv = Cv(w, h)
    r = rng_of("work:" + str(wk.get("id")))
    text = " ".join(str(wk.get(k) or "") for k in ("picture", "what", "gives"))
    main, minor = sign_for(str(wk.get("id") or ""), str(wk.get("name") or ""), text)
    cv.rect(0, 0, w - 1, h - 9, r.choice((SKY, FROST, PEACH, MGOLD)))
    cv.rect(0, h - 24, w - 1, h - 9, r.choice((MEADOW, GRASS, SAND, REED)))
    cv.dither(0, h - 26, w - 1, h - 24, GRASS)
    cv.blit(glyph(main or "house", 3, NIGHT), (w - 48) // 2, 4)
    if minor:
        cv.blit(glyph(minor, 1, NIGHT), w - 18, h - 26)
    label_strip(cv, wk.get("name") or wk.get("id") or "", 8)
    placeholder_mark(cv)
    return cv


# ================================================================================================ leaves and sticks
PEOPLE_CORD = {"norse": BLOOD, "english": FOREST, "latin-church": BYZ, "saxon": DEARTH, "irish": MEADOW, "frankish": RUST,
               "frisian": SHOAL}


def draw_leaf(t, w=48, h=48, riddle=False, difficulty=0, verse=False):
    """A text as it lies in the chest: a rune stick, a leaf of parchment, a book or a wax tablet, by its 'form'."""
    cv = Cv(w, h)
    tid = str(t.get("id") or "")
    r = rng_of("leaf:" + tid)
    form = (t.get("form") or "parchment").lower()
    cord = PEOPLE_CORD.get((t.get("people") or "").lower(), GOLD)
    title = t.get("title") or tid
    m = re.search(r"(\d+)\s*$", tid) or re.search(r"(\d+)", title)
    numb = m.group(1) if m else ""
    initial = (ascii_up(title).strip() or "?")[0]
    if "stick" in form or "rune" in form:
        wood, wsh = r.choice(((SAND, DEARTH), (PSAND, SAND), (DEARTH, TIMBER)))
        cv.poly([(3, 37), (40, 8), (45, 14), (8, 43)], wood, PEAT)
        cv.line(5, 38, 40, 10, PSAND if wood != PSAND else WWHITE)
        n = 7 + r.randint(0, 3)
        for i in range(n):
            x = 9 + i * (30 // n) + r.randint(0, 1)
            y = 36 - int((x - 6) * 0.78)
            k = r.randint(0, 3)
            cv.line(x, y - 2, x + 2, y + 4, PEAT)
            if k == 0:
                cv.line(x + 1, y - 1, x + 3, y, PEAT)
            elif k == 1:
                cv.line(x + 1, y + 1, x + 3, y - 1, PEAT); cv.line(x + 3, y - 1, x + 2, y + 2, PEAT)
            elif k == 2:
                cv.line(x - 1, y + 1, x + 1, y, PEAT)
        cv.line(37, 10, 43, 16, cord, 2); cv.line(43, 16, 46, 22, cord); cv.line(42, 17, 41, 24, cord)
    elif "book" in form:
        cover, csh = r.choice(((BLOOD, DPURP), (BYZ, PLUM), (FOREST, WSHADE), (TIMBER, WEARTH), (SEA, DEEP)))
        cv.poly([(8, 7), (36, 4), (40, 40), (12, 44)], cover, NIGHT)
        cv.poly([(36, 4), (40, 6), (44, 40), (40, 40)], PSAND, PEAT)
        cv.poly([(12, 44), (40, 40), (44, 40), (15, 46)], PSAND, PEAT)
        cv.line(11, 8, 15, 43, csh); cv.line(12, 8, 16, 43, csh)
        cv.rect(21, 12, 33, 13, GOLD); cv.rect(22, 33, 34, 34, GOLD)
        cv.text(23, 17, initial, PGOLD, 3)
        cv.rect(39, 20, 41, 23, GOLD)
    elif "wax" in form or "tablet" in form:
        cv.rect(4, 8, 22, 40, TIMBER); cv.rect(25, 8, 43, 40, TIMBER); cv.box(4, 8, 22, 40, PEAT); cv.box(25, 8, 43, 40, PEAT)
        cv.rect(6, 10, 20, 38, BASALT); cv.rect(27, 10, 41, 38, BASALT)
        for y in range(13, 37, 4):
            cv.line(8, y, 8 + r.randint(6, 11), y, PSTONE); cv.line(29, y, 29 + r.randint(5, 11), y, PSTONE)
        cv.rect(22, 14, 25, 15, cord); cv.rect(22, 32, 25, 33, cord)
        cv.line(30, 46, 46, 30, PSTONE, 2); cv.px(29, 47, SLATE)
    else:                                                    # a leaf of parchment
        sheet = r.choice((PSAND, WWHITE, MGOLD))
        cv.poly([(9, 3), (38, 3), (40, 38), (34, 45), (8, 44)], sheet, DEARTH)
        cv.poly([(34, 45), (35, 39), (40, 38)], SAND, DEARTH)
        cv.text(12, 7, initial, BLOOD if r.random() < 0.6 else BYZ, 2)
        for i, y in enumerate(range(8, 42, 3)):
            xa = 21 if y < 18 else 12
            xb = 36 - r.randint(0, 5) - (4 if y > 37 else 0)
            if xb > xa:
                cv.line(xa, y, xb, y, TIMBER if i % 4 else WEARTH)
        cv.line(9, 3, 8, 44, SAND)
    if numb:
        tw = Cv.text_w(numb)
        cv.rect(0, h - 8, tw + 3, h - 1, NIGHT); cv.text(2, h - 7, numb, PGOLD)
    if riddle:
        cv.blit(glyph("question", 1, NIGHT), 0, 0)
        for i in range(max(0, min(5, int(difficulty)))):
            cv.rect(w - 4 - i * 4, h - 4, w - 2 - i * 4, h - 2, GOLD); cv.box(w - 5 - i * 4, h - 5, w - 1 - i * 4, h - 1, NIGHT)
    if verse:
        cv.blit(glyph("lyre", 1, NIGHT), 0, 0)
    placeholder_mark(cv)
    return cv


# ================================================================================================ port marks
def draw_port_mark(p, w=16, h=16):
    """The mark of a port on the chart: its kind as a tiny sign, its size as dots beneath."""
    cv = Cv(w, h)
    kind = (p.get("kind") or "").lower()
    size = int(num(p.get("size"), 2))
    christian = (p.get("faith") or "christian") == "christian"
    if p.get("_band"):                                       # the ground under the sign: the realm's colour
        cv.rect(1, 12, 14, 12, p["_band"])
    if kind == "monastery":
        cv.rect(4, 6, 10, 11, PSTONE); cv.poly([(3, 6), (7, 2), (11, 6)], TIMBER); cv.rect(7, 0, 7, 3, PGOLD); cv.rect(6, 1, 8, 1, PGOLD)
        cv.rect(11, 8, 13, 11, PSTONE); cv.rect(7, 9, 8, 11, NIGHT)
    elif kind == "royal seat":
        cv.rect(2, 7, 13, 11, SAND); cv.poly([(1, 7), (4, 3), (11, 3), (14, 7)], GOLD); cv.rect(7, 8, 8, 11, NIGHT)
        cv.px(3, 2, GOLD); cv.px(12, 2, GOLD)
    elif kind == "fort":
        for x in range(2, 14, 2):
            cv.rect(x, 5, x, 11, TIMBER); cv.px(x + 1, 6, WEARTH); cv.rect(x + 1, 7, x + 1, 11, WEARTH)
        cv.rect(6, 2, 9, 11, STONE); cv.rect(7, 8, 8, 11, NIGHT); cv.px(6, 1, STONE); cv.px(9, 1, STONE)
    elif kind == "town":
        cv.rect(1, 8, 14, 11, PSTONE); cv.rect(1, 6, 2, 7, PSTONE); cv.rect(13, 6, 14, 7, PSTONE); cv.rect(7, 9, 8, 11, NIGHT)
        cv.rect(5, 4, 10, 7, SAND); cv.poly([(4, 4), (7, 1), (11, 4)], RUST)
        if christian:
            cv.px(7, 0, PGOLD)
    elif kind == "emporium":
        cv.rect(1, 6, 5, 10, SAND); cv.poly([(0, 6), (3, 3), (6, 6)], DEARTH)
        cv.rect(6, 5, 10, 10, PSAND); cv.poly([(5, 5), (8, 2), (11, 5)], TIMBER)
        cv.rect(11, 7, 14, 10, SAND); cv.poly([(10, 7), (12, 4), (15, 7)], DEARTH)
        cv.rect(2, 11, 3, 12, WEARTH); cv.rect(7, 11, 8, 12, WEARTH); cv.rect(12, 11, 13, 12, WEARTH)
    elif kind == "market":
        cv.poly([(2, 6), (4, 2), (11, 2), (13, 6)], MAUVE); cv.rect(3, 7, 3, 11, TIMBER); cv.rect(12, 7, 12, 11, TIMBER)
        cv.rect(4, 9, 11, 10, DEARTH); cv.px(6, 8, GOLD); cv.px(9, 8, RUST)
    else:                                                    # a village, or a kind we do not know
        cv.rect(4, 7, 11, 11, SAND); cv.poly([(3, 7), (7, 3), (8, 3), (12, 7)], DEARTH); cv.rect(7, 9, 8, 11, NIGHT)
    for i in range(max(1, min(5, size))):
        cv.rect(8 - max(1, min(5, size)) * 3 // 2 + i * 3, 14, 9 - max(1, min(5, size)) * 3 // 2 + i * 3, 14, PGOLD)
    cv.outline_all(NIGHT)
    if (w, h) != (16, 16):
        k = max(1, min(w // 16, h // 16)); big = Cv(w, h); big.blit(cv.scaled(k), (w - 16 * k) // 2, (h - 16 * k) // 2); cv = big
    placeholder_mark(cv)
    return cv
