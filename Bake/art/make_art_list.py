# -*- coding: utf-8 -*-
r"""
MAKE ART LIST (the user, 4 Oct 2026: "need to generate placeholder art and description for that piece of placeholder art
in relation to character, event, location etc for me to replace with my own art. please compile the complete list of
this in a folder on the desktop").

Finds every picture the game needs in the game's own data, gives each a SLOT with an id that does not change, draws a
placeholder for it by rule in the game's 40 colours at the size the real picture should be, and writes for the artist
what the picture must show. Run it again whenever the data has grown: new slots are added, pictures you have already
redrawn on the Desktop are never overwritten.

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\art\make_art_list.py
      [--no-desktop]            only Kapman2/Art, not the Desktop folder
      [--desktop <folder>]      another folder than  C:/Users/joeyw/OneDrive/Desktop/Kapman Art
      [--only kind,kind]        draw only these kinds (the list is still written whole)
      [--sheets <folder>]       also write one contact sheet a kind there, to look the placeholders over
      [--root <Kapman2>]        another project folder (for trying things)

Writes
  Kapman2/Art/placeholder/<kind>/<slot id>.png     the placeholders
  Kapman2/Art/art_list.csv                         id, kind, name, where used, size, file, description, source data file
  Kapman2/Art/art_slots.json                       every slot ever issued (so an id is never used twice)
  Kapman2/Art/placeholder_manifest.json            what each placeholder looked like (so pull_art.py knows what you changed)
  Kapman2/Art/hud_assign.json                      where each piece of the frame goes, how it is cut, what shows it in the game today
  Kapman2/Art/person_slots.json                    the people of the places files whose portrait has another name than person.<id>
  <Desktop>/Kapman Art/<kind>/..., art_list.csv, index.html, DRAW FIRST.html, _pages/<kind>.html, READ ME FIRST.txt, the palette,
      ART GUIDE.md (a copy of Design/art_guide.md), _templates/sheets/<group>.png (each family of the frame at 8 times its size)

Sizes (pixel art for a 640 x 360 screen, shown x3 on 1920 x 1080, and for a 960 x 540 screen, shown x2; nothing is ever
scaled by a part number): see SIZES below. pull_art.py brings redrawn pictures back into Kapman2/Art/final.
"""
import csv
import datetime
import glob
import hashlib
import html
import io
import json
import math
import os
import re
import shutil
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

from PIL import Image                                                        # noqa: E402
import artkit                                                                # noqa: E402
from artkit import Cv, off_palette, rng_of                                   # noqa: E402
import artdraw                                                               # noqa: E402
import artscene                                                              # noqa: E402


def arg(name, default=None):
    if name in sys.argv:
        i = sys.argv.index(name)
        if i + 1 < len(sys.argv):
            return sys.argv[i + 1]
    return default


ROOT = os.path.normpath(arg("--root", os.path.join(HERE, "..", "..")))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
DESIGN = os.path.join(ROOT, "Design")
CONTENT = os.path.join(DESIGN, "content")
ART = os.path.join(ROOT, "Art")
DESK = None if "--no-desktop" in sys.argv else arg("--desktop", os.environ.get("KAPMAN_ART_DESKTOP", "C:/Users/joeyw/OneDrive/Desktop/Kapman Art"))
ONLY = set(filter(None, (arg("--only", "") or "").split(",")))
SHEETS = arg("--sheets")

# ------------------------------------------------------------------------------------------------ kinds and sizes
# kind: (width, height, heading, where the game shows it, what the size is for)
SIZES = {
    "portrait": (64, 80, "Portraits", "In the portrait block top right while the person speaks, in a floating window of talk, and beside the name in the People here window.",
                 "Head and shoulders. 64 x 80: a fifth of the 640 x 360 screen's height."),
    "scene": (320, 180, "Scenes: the sites of the ports", "The full window of a port, when one of its sites is opened: the picture above what can be done there.",
              "320 x 180: half the 640 x 360 screen each way, a third of the 960 x 540 screen."),
    "harbour": (640, 180, "Harbour views: each port from the water", "The top of a port's full window when she comes in, behind the port's name.",
                "640 x 180: the full width of the small screen and half its height; a wide view from the boat."),
    "title": (53, 106, "The game's name, a letter a file",
              "On the title, riding the sea: each letter is a picture of its own, so it can bob, roll and go under by itself.",
              "Each letter its own width by 106 high, all six standing on the same line. Shown about three and a half times this size with hard pixels."),
    "port_view": (640, 360, "Port views: each port from above",
                  "In the middle of the screen, in place of the chart, while she lies at the port. The game lays the hour's light over it (day, dusk, night); nothing else in it moves.",
                  "640 x 360: the whole small screen. Shown stretched over the big one with hard pixels."),
    "port_mark": (16, 16, "Port marks for the chart", "On the chart, at the port's place.", "16 x 16, with a dark edge so it reads on sea and land."),
    "card": (192, 108, "Card pictures", "At the top of a card (a floating window over the chart), above its words and choices.", "192 x 108 (16:9): three tenths of the small screen's width."),
    "story": (192, 108, "Story pictures", "Beside a story of a port or of an age when it is told or remembered (the chest, the port panel).",
              "192 x 108, the same as a card."),
    "ship_offer": (192, 108, "Ships to be had at a port", "The shipyard or the beach of a port, where a ship of that class is offered.", "192 x 108, the same as a card."),
    "ending": (320, 180, "Endings of the chains", "Full in the middle of the screen when a chain of cards ends.", "320 x 180: half the small screen each way."),
    "good": (32, 32, "Goods", "The Trade window, the Hold window, and The word window where a good's price is told.", "32 x 32 icon, clear background."),
    "ship_side": (160, 96, "Ships from the side", "The Ship window, the House window's list of boats, and the shipyard of a port.",
                  "160 x 96, every class at 6 pixels to the metre so they can be compared; bow to the right; clear sky."),
    "ship_mark": (32, 16, "Ships from above, for the chart", "On the chart: your boat, the house's keels, sails made out, raiders.",
                  "32 x 16 at 1.25 pixels to the metre, bow to the right (the game turns it); dark edge; clear background."),
    "upgrade": (32, 32, "Ship upgrades", "Your boat's panel and the shipyard: what is fitted to the ship.", "32 x 32 icon, clear background."),
    "unlock": (32, 32, "What the house can win", "The house's own list: charters, plots, factors, partners, pilots' knowledge, titles, relics, heirlooms, kin, tongues.", "32 x 32 icon, clear background."),
    "work": (64, 64, "Works in the towns", "The port panel, where a work can be paid for, and the house's list of what it has built.", "64 x 64: a small picture, not an icon."),
    "act": (32, 32, "Town acts", "The port panel: the buttons of what can be done at a site.", "32 x 32 icon, clear background."),
    "game": (32, 32, "Town games", "The port panel, where a game can be played, and on the game's own cards.", "32 x 32 icon, clear background."),
    "leaf": (48, 48, "Texts and riddles: leaves and sticks", "The chest: each text and riddle as a thing lying in it.",
             "48 x 48, clear background: a rune stick, a leaf of parchment, a book or a wax tablet."),
    "plate": (640, 360, "Plates: the ages, the start, the end", "The whole screen: when the game starts, when an age turns, when it ends.", "640 x 360: the whole small screen."),
    "hud": (0, 0, "The frame: windows, buttons, marks and icons",
            "The Command frame: the thick frame, the settings bar and its clock, the eight function buttons, the panels and their full windows, the floating windows, the chart tools, the wiki.",
            "Each piece has its own size; a piece cut in nine says its corner size."),
    "shipview": (160, 96, "The ship window", "Bottom right: the view of your ship, by where she is, the hour and the weather, until the live 3D view takes its place.",
                 "160 x 96, the same as a ship from the side, which is laid over it; the sun, the moon and the weather are small pieces of their own."),
    "ui": (0, 0, "The cursor and the marks on the chart", "See each piece.", "Each piece has its own size."),
    "map": (4096, 4096, "The chart's picture", "Under everything: the map the game is played on.", "4096 x 4096 a tile, 120 real metres a pixel."),
}
KIND_ORDER = ["title", "portrait", "scene", "harbour", "port_view", "port_mark", "card", "story", "ending", "plate", "good", "ship_side", "ship_mark",
              "ship_offer", "upgrade", "unlock", "work", "act", "game", "leaf", "hud", "shipview", "ui", "map"]

LOG = []
SKIPPED = []


def say(msg):
    LOG.append(msg)
    try:
        print(msg)
    except Exception:
        print(msg.encode("ascii", "replace").decode("ascii"))


def rel(path):
    try:
        return os.path.relpath(path, ROOT).replace("\\", "/")
    except Exception:
        return str(path).replace("\\", "/")


def jload(path):
    """A JSON file, or None if it is not there or does not parse (it is then skipped and named in the report)."""
    if not os.path.exists(path):
        return None
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            d = json.load(f)
        if not isinstance(d, dict):
            SKIPPED.append("%s: is not a JSON object at the top; skipped" % rel(path))
            return None
        return d
    except Exception as e:
        SKIPPED.append("%s: does not parse (%s)" % (rel(path), str(e)[:80]))
        return None


def tload(path):
    if not os.path.exists(path):
        return ""
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            return f.read()
    except Exception as e:
        SKIPPED.append("%s: cannot be read (%s)" % (rel(path), str(e)[:80]))
        return ""


def lst(v):
    return v if isinstance(v, list) else []


def dct(v):
    return v if isinstance(v, dict) else {}


def s(v):
    """A plain one-line string of anything."""
    if v is None:
        return ""
    if isinstance(v, (list, tuple)):
        return ", ".join(s(x) for x in v if x not in (None, ""))
    if isinstance(v, dict):
        return "; ".join("%s: %s" % (k, s(x)) for k, x in v.items())
    return re.sub(r"\s+", " ", str(v)).strip()


def dot(text):
    text = s(text)
    if text and text[-1] not in ".!?\"'":
        text += "."
    return text


def clip(text, n):
    text = s(text)
    if len(text) <= n:
        return text
    cut = text[:n]
    k = max(cut.rfind(". "), cut.rfind("; "))
    return (cut[:k + 1] if k > n * 0.5 else cut.rstrip(" ,;:") + " ...")


def safe_id(text):
    return re.sub(r"[^A-Za-z0-9._-]+", "_", str(text)).strip("_.") or "x"


def years(a, b):
    a, b = int(artdraw.num(a, 0)), int(artdraw.num(b, 0))
    if a and b:
        return "%d to %d" % (a, b)
    if a:
        return "from %d" % a
    if b:
        return "until %d" % b
    return ""


# ------------------------------------------------------------------------------------------------ the data
class World(object):
    pass


W = World()


def load_world():
    W.ports = {}
    pj = jload(os.path.join(DATA, "ports_793.json")) or {}
    for p in lst(pj.get("ports")):
        if isinstance(p, dict) and p.get("id"):
            W.ports[p["id"]] = dict(p)
    roles_doc = jload(os.path.join(DATA, "encounters", "npc_roles_793.json")) or {}
    W.roles = dct(roles_doc.get("roles"))
    W.realms = dct(roles_doc.get("realms"))
    W.peoples = dct(roles_doc.get("peoples"))
    wording = jload(os.path.join(DATA, "wording_793.json")) or {}
    cor, cop = dct(wording.get("culture_of_region")), dct(wording.get("culture_of_port"))
    for pid, p in W.ports.items():
        realm = dct(W.realms.get(p.get("realm")))
        p["faith"] = realm.get("faith") or "christian"
        p["culture"] = cop.get(pid) or cor.get(p.get("region")) or (lst(realm.get("peoples")) or [""])[0]
    names = jload(os.path.join(DATA, "encounters", "names_placeholder.json")) or {}
    W.name_sex = {}
    for n in lst(names.get("names")) + lst((jload(os.path.join(DATA, "encounters", "names_793.json")) or {}).get("names")):
        if isinstance(n, dict) and n.get("name"):
            W.name_sex[n["name"].lower()] = n.get("sex")
    # the words
    W.writing = {}
    for path in [os.path.join(DATA, "writing.json")] + sorted(glob.glob(os.path.join(DATA, "writing", "*.json"))):
        d = jload(path) or {}
        for e in lst(d.get("entries")):
            if isinstance(e, dict) and e.get("key") and isinstance(e.get("text"), str) and e["text"].strip() and "[write:" not in e["text"]:
                W.writing[e["key"]] = (e["text"].strip(), rel(path))
    # the spine
    W.spine_path = os.path.join(CONTENT, "spine.json")
    W.spine = jload(W.spine_path) or {}
    W.cast = [c for c in lst(W.spine.get("cast")) if isinstance(c, dict) and c.get("id")]
    W.people_words = dct(dct(W.spine.get("peoples")).get("ids"))
    # the places
    W.places = {}            # port id -> (entry, file)
    for path in sorted(glob.glob(os.path.join(DATA, "places", "*.json"))):
        d = jload(path) or {}
        for p in lst(d.get("ports")):
            if isinstance(p, dict) and p.get("port"):
                W.places.setdefault(p["port"], (p, path))
    # the story tables
    W.story_files = {os.path.basename(p): (jload(p) or {}, p) for p in sorted(glob.glob(os.path.join(DATA, "story", "*.json")))}
    W.acts = {}
    for fn, (d, path) in W.story_files.items():
        if "action" in fn or fn.startswith("town_acts"):
            for a in lst(d.get("acts")) + lst(d.get("actions")):
                if isinstance(a, dict) and a.get("id"):
                    W.acts[a["id"]] = (a, path)
    # dress and houses, in the peoples' own notes
    W.dress, W.houses, W.burial = {}, {}, {}
    for path in sorted(glob.glob(os.path.join(CONTENT, "peoples", "*.md"))):
        key = os.path.splitext(os.path.basename(path))[0].lower()
        text = tload(path)
        for store, word in ((W.dress, "Dress"), (W.houses, "House")):
            m = re.search(r"\*\*%s[.:]?\*\*[.:]?\s*(.+)" % word, text)
            if m:
                store[key] = (re.sub(r"\*", "", m.group(1)).strip(), rel(path))
        m = re.search(r"^##\s*\d*\.?\s*Burial\s*\n+(.+)", text, re.M)
        if m:
            W.burial[key] = (re.sub(r"\*", "", m.group(1)).strip(), rel(path))
    W.brief = tload(os.path.join(CONTENT, "BRIEF.md"))
    W.brief2 = tload(os.path.join(CONTENT, "BRIEF_2.md"))
    W.cards = {}             # card id -> (card, file)
    for path in sorted(glob.glob(os.path.join(DATA, "encounters", "*.json"))):
        if os.path.basename(path) in ("npc_roles_793.json", "names_placeholder.json", "names_793.json"):
            continue
        d = jload(path) or {}
        for c in lst(d.get("cards")):
            if isinstance(c, dict) and c.get("id"):
                W.cards.setdefault(c["id"], (c, path))
    cls = jload(os.path.join(DATA, "ship_classes_793.json")) or {}
    W.ship_classes = {}
    for c in lst(cls.get("classes")):
        if isinstance(c, dict) and c.get("id"):
            W.ship_classes[c["id"]] = (c, os.path.join(DATA, "ship_classes_793.json"))


def culture_of(people):
    m = re.search(r"culture\s+([a-z_]+)", s(W.people_words.get(people)))
    return m.group(1) if m else people


def people_note(store, people):
    """The peoples' notes on dress or houses: the people's own file, else the file of the culture it counts as."""
    if not people:
        return ""
    if people in store:
        return "%s (%s)" % (store[people][0], store[people][1])
    cult = culture_of(people)
    for key in (cult, {"norse": "norse", "danish": "norse", "svea": "norse", "gute": "norse"}.get(people, "")):
        if key and key in store:
            return "%s (%s: written of the %s, whose culture the %s count as in the game)" % (store[key][0], store[key][1], key, people)
    return ""


NEVER = [("horned helmets", "no horned helmets"), ("no buttons", "no buttons"), ("no chimneys", "no chimneys"),
         ("stave churches", "no stave churches before the 11th century"), ("no forks", "no forks"), ("no clocks", "no clocks"),
         ("no compass", "no compass"), ("no sea charts", "no sea charts in the world"), ("no paper", "no paper north of the Alps"),
         ("no potatoes", "no potatoes, maize or tobacco")]


def never(*keys):
    """The anachronisms BRIEF.md rule 2 forbids, as far as they bear on this kind of picture."""
    out = [words for key, words in NEVER if key in W.brief and any(k in key for k in keys)]
    return ("Never (BRIEF.md rule 2): " + ", ".join(out) + ".") if out else ""


NUM_WORDS = {"twenty": 20, "thirty": 30, "forty": 40, "fifty": 50, "sixty": 60, "seventy": 70, "eighty": 80, "ninety": 90}
UNITS = {"one": 1, "two": 2, "three": 3, "four": 4, "five": 5, "six": 6, "seven": 7, "eight": 8, "nine": 9}
TEENS = {"ten": 10, "eleven": 11, "twelve": 12, "thirteen": 13, "fourteen": 14, "fifteen": 15, "sixteen": 16, "seventeen": 17,
         "eighteen": 18, "nineteen": 19}
DECADES = {"teens": 16, "twenties": 25, "thirties": 35, "forties": 45, "fifties": 55, "sixties": 65, "seventies": 75, "eighties": 85}


def age_from_text(text):
    """An age said in plain words ('About forty-five', 'in his fifties', 'a boy of twelve'); (age, the words) or (None, '')."""
    t = (text or "").lower()
    m = re.search(r"in (?:his|her|their) (?:early |late |mid-?)?(%s)" % "|".join(DECADES), t)
    if m:
        return DECADES[m.group(1)], m.group(0)
    m = re.search(r"(?:about|around|nearly|past|perhaps|maybe|some|over|not yet|under|aged?|of)\s+(%s)(?:[- ](%s))?\b(?!\s+(?:ships|men|years ago|pounds|pence|days|miles))"
                  % ("|".join(NUM_WORDS), "|".join(UNITS)), t)
    if m:
        return NUM_WORDS[m.group(1)] + UNITS.get(m.group(2) or "", 0), m.group(0)
    m = re.search(r"(?:about|around|nearly|aged?|of)\s+(%s)\b" % "|".join(TEENS), t)
    if m:
        return TEENS[m.group(1)], m.group(0)
    m = re.search(r"(?:about|around|nearly|aged?)\s+(\d{2})\b(?!\d)", t)
    if m:
        return int(m.group(1)), m.group(0)
    for wds, a in ((("a boy", "a girl", "a child", "a lad"), 11), (("young",), 22), (("middle-aged", "middle years"), 45),
                   (("an old", "old man", "old woman", "aged", "elderly", "very old"), 68)):
        for wd in wds:
            if wd in t:
                return a, wd
    return None, ""


def person_facts(p, role_fallback=None):
    """Sex and age of a person, each with where it was taken from."""
    role = p.get("role") or role_fallback or ""
    rdef = dct(W.roles.get(role))
    looks = s(p.get("looks"))
    sex = (p.get("sex") or "").lower()[:1]
    sex_from = "the data" if sex in ("m", "f") else ""
    if sex not in ("m", "f"):
        ns = W.name_sex.get(s(p.get("name")).lower())
        if ns in ("m", "f"):
            sex, sex_from = ns, "the name table"
        elif rdef.get("sex") in ("m", "f"):
            sex, sex_from = rdef["sex"], "the role"
        elif re.search(r"\b(she|her|woman|girl|wife|widow|nun|abbess|mother|daughter)\b", looks.lower()):
            sex, sex_from = "f", "'looks'"
        else:
            sex, sex_from = "m", "a guess"
    age, age_from = age_from_text(looks)
    if age is not None:
        age_from = "'looks': %s" % age_from
    else:
        born = int(artdraw.num(p.get("born"), 0))
        if not born:
            m = re.search(r"born (?:about |c\. ?|around |in )?(\d{3,4})", s(p.get("dates")).lower())
            born = int(m.group(1)) if m else 0
        frm = int(artdraw.num(p.get("from"), 0)) or 793
        if born and 4 < frm - born < 95:
            age, age_from = frm - born, "born %d, met from %d" % (born, frm)
        elif isinstance(rdef.get("age"), list) and len(rdef["age"]) == 2:
            lo, hi = rdef["age"]
            age = int(lo + (hi - lo) * (artkit.seed_of(p.get("id")) % 100) / 100.0)
            age_from = "the role's span %s to %s; no age is given" % (lo, hi)
        else:
            age, age_from = 35, "no age is given"
    return sex, sex_from, age, age_from


def role_title(role):
    return s(dct(W.roles.get(role)).get("title")) or s(role).replace("_", " ")


def bare(title):
    """'the toll man' -> 'toll man', for 'any toll man'."""
    return re.sub(r"^(the|a|an)\s+", "", s(title))


def an(word):
    """'an emporium', 'a market'."""
    w = s(word)
    return ("an " if w[:1].lower() in "aeiou" else "a ") + w


def port_name(pid):
    p = W.ports.get(pid)
    return ("%s (%s)" % (p.get("name"), p.get("modern"))) if p else s(pid)


SLOTS = []
_seen = {}


def slot(sid, kind, name, desc, source, draw=None, size=None, where=None, copy=None):
    sid = safe_id(sid)
    if sid in _seen:
        if _seen[sid] == source:
            return None
        sid = sid + "." + hashlib.md5(s(source).encode("utf-8")).hexdigest()[:4]
        if sid in _seen:
            return None
    _seen[sid] = source
    w, h = size or SIZES[kind][:2]
    sl = {"id": sid, "kind": kind, "name": s(name) or sid, "w": int(w), "h": int(h), "where": where or SIZES[kind][3],
          "desc": re.sub(r"\s+", " ", desc).strip(), "source": source, "draw": draw, "copy": copy, "file": "%s/%s.png" % (kind, sid)}
    SLOTS.append(sl)
    return sl


def size_line(kind, w=None, h=None):
    w0, h0 = SIZES[kind][:2]
    return "Size: %d x %d pixels." % (w or w0, h or h0)


def file_line(kind, sid):
    return "File to replace: %s/%s.png (same name, same size)." % (kind, safe_id(sid))


# ------------------------------------------------------------------------------------------------ people
def portrait_slot(sid, p, source, extra="", stand_in=False):
    role = p.get("role") or ""
    people = p.get("people") or ""
    sex, sex_from, age, age_from = person_facts(p)
    pp = dict(p, sex=sex, age=age)
    name = s(p.get("name")) or sid
    title = role_title(role)
    where_at = s(p.get("seat")) or (port_name(p.get("_port")) if p.get("_port") else (port_name(p.get("home")) if p.get("home") else ""))
    parts = []
    if stand_in:
        parts.append("Stand-in portrait for any %s who has no portrait of their own: head and shoulders, facing us." % bare(title))
        n_role = ROLE_COUNT.get(role, 0)
        parts.append(("In the twelve ports of the first release %d of the named people have this role, so this one picture is seen for each of them until theirs is drawn." % n_role)
                     if n_role else "None of the named people of the twelve ports has this role: it is met on cards and at sea.")
        parts.append("What the role is: %s" % dot(clip(dct(W.roles.get(role)).get("note"), 420)))
        parts.append("His people and faith are those of the port where he is met, so keep the dress plain and of no one people."
                     if sex == "m" else "Her people and faith are those of the port where she is met, so keep the dress plain and of no one people.")
        parts.append("%s, aged %s (the role's own span)." % ("A man" if sex == "m" else "A woman" if dct(W.roles.get(role)).get("sex") == "f" else "A man or a woman",
                                                            " to ".join(str(x) for x in lst(dct(W.roles.get(role)).get("age"))) or "not given"))
    else:
        parts.append("Portrait of %s, %s%s: head and shoulders, facing us." % (name, title, (", " + where_at) if where_at else ""))
        if p.get("looks"):
            parts.append("Looks: %s" % dot(p.get("looks")))
        else:
            parts.append("Looks: none written yet ('looks' is empty); draw from what follows.")
        who = "%s, about %d (%s)" % ("A man" if sex == "m" else "A woman", age, age_from)
        if sex_from not in ("the data",):
            who += "; that it is a %s is from %s" % ("man" if sex == "m" else "woman", sex_from)
        parts.append(dot(who))
        if people:
            parts.append("People: %s%s." % (people, (" (%s)" % clip(W.people_words.get(people), 90)) if W.people_words.get(people) else ""))
        if p.get("faith"):
            parts.append("Faith: %s." % s(p.get("faith")).replace("_", " "))
        if isinstance(p.get("historical"), bool):
            parts.append("A real person: %s" % dot(p.get("dates") or "dates in the source") if p["historical"] else "An invented person.")
        yrs = years(p.get("from"), p.get("to"))
        if yrs:
            parts.append("In the game %s." % yrs)
        if p.get("trait"):
            parts.append("Character, for the face: %s" % dot(p.get("trait")))
        if p.get("voice"):
            parts.append("Voice: %s" % dot(p.get("voice")))
    dress = people_note(W.dress, people)
    if dress:
        parts.append("Dress of the %s: %s" % (people, dot(dress)))
    parts.append(never("helmets", "buttons"))
    if extra:
        parts.append(extra)
    parts.append("Light and season: none given; a plain even light, no scene behind.")
    parts.append("The placeholder: ground colour by people, the corner sign is the faith, the boxed sign is the role.")
    parts.append(size_line("portrait"))
    parts.append(file_line("portrait", sid))
    slot(sid, "portrait", name if not stand_in else "Any %s" % bare(title), " ".join(x for x in parts if x), source,
         draw=lambda pp=pp: artdraw.draw_portrait(pp, *SIZES["portrait"][:2]))


ROLE_COUNT = {}
KIND_COUNT = {}
PERSON_SLOTS = {}


def count_first():
    """How many people of each role and how many sites of each kind the twelve ports hold: what a stand-in covers."""
    ROLE_COUNT.clear(); KIND_COUNT.clear()
    cast_by_place = dict((c.get("place_id"), c) for c in W.cast if c.get("place_id"))
    for pid in FIRST_PORTS:
        entry = W.places.get(pid, ({}, ""))[0]
        for m in lst(entry.get("people")):
            if isinstance(m, dict) and m.get("id"):
                role = s(dct(cast_by_place.get(m["id"])).get("role")) or s(m.get("role"))
                if role:
                    ROLE_COUNT[role] = ROLE_COUNT.get(role, 0) + 1
        for st in lst(entry.get("sites")):
            if isinstance(st, dict) and st.get("kind"):
                KIND_COUNT[s(st["kind"])] = KIND_COUNT.get(s(st["kind"]), 0) + 1


def collect_people():
    count_first()
    done = set()
    place_ids = {}
    for c in W.cast:
        if c.get("place_id"):
            place_ids[c["place_id"]] = c
    # which places file also has this cast member (for the lines and the port)
    place_people = {}
    for pid, (entry, path) in W.places.items():
        for m in lst(entry.get("people")):
            if isinstance(m, dict) and m.get("id"):
                place_people[m["id"]] = (m, pid, path)
    for c in W.cast:
        p = dict(c)
        extra = ""
        twin = place_people.get(c.get("place_id") or "")
        if twin:
            m, pid, path = twin
            if not p.get("looks") and m.get("looks"):
                p["looks"] = m["looks"]
            extra = "Also written at %s (%s) as %s." % (port_name(pid), rel(path), m.get("id"))
            done.add(m["id"])
            PERSON_SLOTS[m["id"]] = "portrait/%s.png" % safe_id("person." + c["id"])
            if pid in FIRST_PORTS and int(artdraw.num(c.get("from"), 0)) <= 805:
                (FIRST_HOUR if (pid == FIRST_PORTS[0] and s(c["id"]).startswith("cast.crew.")) else FIRST_IDS).add(safe_id("person." + c["id"]))
        if c.get("threads"):
            extra += " Threads: %s." % s(c.get("threads"))
        portrait_slot("person." + c["id"], p, rel(W.spine_path), extra.strip())
    for mid, (m, pid, path) in sorted(place_people.items()):
        if mid in done:
            continue
        p = dict(m, _port=pid)
        port = W.ports.get(pid) or {}
        if not p.get("people"):
            p["people"] = port.get("culture") or ""
        sites = [st.get("name") for st in lst(W.places[pid][0].get("sites")) if isinstance(st, dict) and ("person:" + mid) in lst(st.get("do"))]
        extra = ("Met at: %s." % s(sites)) if sites else ""
        portrait_slot("person." + mid, p, rel(path), extra)
    rpath = rel(os.path.join(DATA, "encounters", "npc_roles_793.json"))
    for role, rdef in W.roles.items():
        if not isinstance(rdef, dict):
            continue
        sex = rdef.get("sex") if rdef.get("sex") in ("m", "f") else "m"
        age = lst(rdef.get("age"))
        p = {"id": "role." + role, "name": role_title(role), "role": role, "sex": sex, "people": "", "faith": "",
             "looks": "about %d" % (int((age[0] + age[1]) / 2) if len(age) == 2 else 35)}
        portrait_slot("role." + role, p, rpath, "", True)


# ------------------------------------------------------------------------------------------------ ports and their sites
SITE_WORD = {"tavern": "ale-house"}


def do_words(do, pid):
    out = []
    entry = W.places.get(pid, ({}, ""))[0]
    people = {m.get("id"): m for m in lst(entry.get("people")) if isinstance(m, dict)}
    for d in lst(do):
        d = s(d)
        if d == "market":
            out.append("the market")
        elif d.startswith("act:"):
            a = W.acts.get(d[4:])
            out.append((s(a[0].get("name")) if a else d[4:].replace("_", " ")).lower())
        elif d.startswith("person:"):
            m = people.get(d[7:]) or next((c for c in W.cast if c.get("place_id") == d[7:] or c.get("id") == d[7:]), None)
            out.append("talk to %s (%s)" % (m.get("name"), role_title(m.get("role"))) if m else "talk to " + d[7:])
        elif d.startswith("card:"):
            c = W.cards.get(d[5:])
            out.append("the card '%s'" % (card_title(c[0]) if c else d[5:]))
    return out


def dossier(pid):
    path = os.path.join(CONTENT, "dossiers", "%s.md" % pid)
    text = tload(path)
    if not text:
        return "", ""
    m = re.search(r"^##\s*1\..*?\n(.*?)(?=^##\s)", text, re.S | re.M)
    sec = m.group(1) if m else ""
    paras = [re.sub(r"\s+", " ", x).strip() for x in re.split(r"\n\s*\n", sec) if x.strip()]
    seen_ = next((x for x in paras if re.search(r"skipper sees|is seen|from the water|sees\b", x)), paras[-1] if paras else "")
    m2 = re.search(r"^##\s*2\..*?\n(.*?)(?=^##\s)", text, re.S | re.M)
    stood = re.sub(r"\s+", " ", m2.group(1)).strip() if m2 else ""
    out = ""
    if seen_:
        out += "What a skipper sees (%s, section 1): %s " % (rel(path), dot(clip(seen_, 800)))
    if stood:
        out += "What stood there (section 2): %s" % dot(clip(stood, 700))
    return out.strip(), rel(path)


BAND_COLS = [artkit.BYZ, artkit.SHOAL, artkit.MEADOW, artkit.RUST, artkit.GOLD, artkit.BLOOD, artkit.MAUVE, artkit.SHALL, artkit.FOREST,
             artkit.STONE, artkit.LILAC, artkit.DROSE, artkit.SEA, artkit.REED, artkit.FROST, artkit.DEARTH, artkit.PLUM, artkit.GRASS]


def collect_opening():
    """The people of the game's beginning who are in no place's list (Showtime.cs)."""
    lender = {"id": "lender", "name": "Wursing, called the W\u014dkerer (the lender of the opening)", "role": "merchant", "people": "frisian",
              "faith": "christian", "sex": "m", "age": 58, "historical": False, "from": 793,
              "looks": "Lean, grey, clean-shaven or close-cropped; pale eyes that look at the tally in his hand and not at you. Good plain wool, well kept, no show: one silver pin, a bunch of split hazel tally sticks at his belt.",
              "trait": "Cold, aloof, uninterested. He cares for what he is owed and for nothing else about you. He is never angry and never warm.",
              "voice": "Short flat sentences. He does not raise his voice and does not repeat himself."}
    # the game's name, a letter a file (the title's sea: Showtime.cs reads title/title.letter.N.png)
    for i, ch in enumerate(artscene.TITLE_NAME):
        cv = artscene.draw_title_letter(ch, i)
        sid = "title.letter.%d" % (i + 1)
        FIRST_HOUR.add(safe_id(sid))
        slot(sid, "title", "The name's letter %d of 6: %s" % (i + 1, ch),
             "Letter %d of the game's name, Kāpman: \"%s\". On the title the six letters stand side by side in the middle of a stormy sea seen from above; each rises in by itself, rides the sea with its own bob and roll, and goes under by itself when the game is begun. " % (i + 1, ch)
             + "So each is a picture of its own, clear all round it. The user's colour for the name is the green 297036, far more faded, in place of white (7 Oct 2026), with a texture: the placeholder is the frame's bold letter cut small with hard edges, painted as a worn sign in that faded green with a lit edge, a shaded edge, grain, scratches, a dark purple line round it and its shadow. "
             + "Keep every letter's picture the same height and its foot on the same line (the placeholder's), or the name will not stand level. These are the only pieces whose colours need not be of the forty. "
             + "Size: %d x %d pixels. File to replace: title/%s.png (same name, same size)." % (cv.w, cv.h, sid),
             "Assets/Kapman/Code/Game/UI/Showtime.cs (the title)", size=(cv.w, cv.h),
             draw=lambda ch=ch, i=i: artscene.draw_title_letter(ch, i))
    FIRST_HOUR.add(safe_id("person.lender"))                  # the first face of the game
    portrait_slot("person.lender", lender, "Assets/Kapman/Code/Game/UI/Showtime.cs (the opening; his name and byname are placeholders: Trade.LenderName, Trade.LenderByname)",
                  extra="He is the first face of the game: he asks the new player's people, name, faith and past, and then says what is owed. The player's creditor wherever the game begins.")


def collect_ports():
    src_ports = rel(os.path.join(DATA, "ports_793.json"))
    realms = sorted({s(p.get("realm")) for p in W.ports.values()})
    for p in W.ports.values():
        p["_band"] = BAND_COLS[realms.index(s(p.get("realm"))) % len(BAND_COLS)]
    for pid, port in W.ports.items():
        entry, path = W.places.get(pid, ({}, ""))
        name = "%s (%s)" % (port.get("name"), port.get("modern"))
        kind, landing, size = s(port.get("kind")), s(port.get("landing")), int(artdraw.num(port.get("size"), 0))
        realm = s(port.get("realm"))
        text = " ".join(s(entry.get(k)) for k in ("summary", "seasons"))
        doss, dpath = dossier(pid)
        parts = ["%s seen from the water as she comes in: %s of the %s (size %d of 5), with a %s landing." % (name, an(kind), realm, size, landing)]
        parts.append(dot(port.get("note")))
        yrs = years(port.get("from"), port.get("to"))
        if yrs:
            parts.append("She trades %s." % yrs)
        if entry.get("summary"):
            parts.append("The place (%s): %s" % (rel(path), dot(entry.get("summary"))))
        eras = [era for era in lst(entry.get("eras")) if isinstance(era, dict) and era.get("text")]
        for era in eras[:1]:
            parts.append("One view is asked for, of her first years in the game. %s: %s" % (years(era.get("from"), era.get("to")) or "Then", dot(era.get("text"))))
        if len(eras) > 1:
            parts.append("(How she changes after that is in the places file, under 'eras': %d more spans of years. It is not for this picture.)" % (len(eras) - 1))
        if doss:
            parts.append(doss)
        houses = people_note(W.houses, port.get("culture"))
        if houses:
            parts.append("Houses of the %s: %s" % (port.get("culture"), dot(houses)))
        parts.append("Faith of the place: %s." % s(port.get("faith")).replace("_", " "))
        parts.append(never("chimneys", "stave", "clocks"))
        if entry.get("seasons"):
            parts.append("Light and season: %s Choose one season and say it with the light." % dot(entry.get("seasons")))
        else:
            parts.append("Light and season: not given in the data yet; the placeholder's light is by chance.")
        parts.append("The placeholder: the far country by her region, the buildings by her kind and size, the shore by her landing, your own stem in the corner.")
        parts.append(size_line("harbour"))
        parts.append(file_line("harbour", "harbour." + pid))
        srcs = ", ".join(x for x in (src_ports, rel(path) if path else "", dpath) if x)
        slot("harbour." + pid, "harbour", name, " ".join(x for x in parts if x), srcs,
             draw=lambda port=port, text=text: artscene.draw_harbour(port, text, *SIZES["harbour"][:2]))
        # the port from above: what stands in the chart's place while she lies there (the user, 7 Oct 2026)
        vparts = ["%s from above, as the picture that stands in the chart's place while she lies there: %s of the %s (size %d of 5), with a %s landing." % (name, an(kind), realm, size, landing)]
        vparts.append("Seen from high up and a little to one side, one fixed angle: the water at the foot of the picture, the landing, the houses as roofs, the one great building of the place, the lanes, and the fields and wood behind. Simple shapes, as an old console would draw a small 3D model with flat pictures stood in it.")
        vparts.append("Draw it in plain daylight with no cast shadows of the sun and no lit windows: the game lays the hour over it (dawn, day, dusk, night). Nothing in it moves.")
        if entry.get("summary"):
            vparts.append("The place (%s): %s" % (rel(path), dot(entry.get("summary"))))
        for era in eras[:1]:
            vparts.append("Her first years in the game. %s: %s" % (years(era.get("from"), era.get("to")) or "Then", dot(era.get("text"))))
        if houses:
            vparts.append("Houses of the %s: %s" % (port.get("culture"), dot(houses)))
        vparts.append(never("chimneys", "stave", "clocks"))
        vparts.append("The placeholder: the houses by her kind and size, the shore by her landing, the country by her region; her great building (a church in its precinct, a hall in its bank, a town's wall) by her kind.")
        vparts.append(size_line("port_view"))
        vparts.append(file_line("port_view", "portview." + pid))
        slot("portview." + pid, "port_view", name, " ".join(x for x in vparts if x), srcs,
             draw=lambda port=port: artscene.draw_port_view(port, "", *SIZES["port_view"][:2]))
        mparts = ["The mark of %s on the chart: %s of the %s, size %d of 5, faith %s." % (name, an(kind), realm, size, s(port.get("faith")).replace("_", " ")),
                  "One small sign that says what kind of place this is (the kinds in the data: emporium, market, town, monastery, royal seat, village, fort) and how big; it must read on both sea and land, so keep the dark edge.",
                  dot(port.get("note")),
                  "The placeholder: the sign is the kind, the gold dots beneath are the size, and the line of colour under the sign is the realm (%s: %s)." % (realm, port.get("_band")),
                  size_line("port_mark"), file_line("port_mark", "portmark." + pid)]
        slot("portmark." + pid, "port_mark", name, " ".join(x for x in mparts if x), src_ports,
             draw=lambda port=port: artdraw.draw_port_mark(port, *SIZES["port_mark"][:2]))
        # the sites
        for st in lst(entry.get("sites")):
            if not isinstance(st, dict) or not st.get("id"):
                continue
            kindw = SITE_WORD.get(s(st.get("kind")), s(st.get("kind")))
            sp = ["%s: the %s at %s, %s of the %s." % (s(st.get("name")) or kindw, kindw, name, an(kind), realm)]
            sp.append("What is seen: %s" % dot(st.get("see")) if st.get("see") else "What is seen: nothing written yet ('see' is empty).")
            yrs = years(st.get("from"), st.get("to"))
            if yrs:
                sp.append("It stands %s." % yrs)
            dw = do_words(st.get("do"), pid)
            if dw:
                sp.append("What is done here (so the picture can show it): %s." % "; ".join(dw))
            houses = people_note(W.houses, port.get("culture"))
            if houses and s(st.get("kind")) not in ("church", "minster", "abbey", "grove", "mound", "field"):
                sp.append("Houses of the %s: %s" % (port.get("culture"), dot(houses)))
            if s(st.get("kind")) in ("graveyard", "mound"):
                bur = people_note(W.burial, port.get("culture"))
                if bur:
                    sp.append("Burial among the %s: %s" % (port.get("culture"), dot(bur)))
            sp.append("Faith of the place: %s." % s(port.get("faith")).replace("_", " "))
            sp.append(never("chimneys", "stave", "clocks"))
            mood, given = artscene.mood_of(s(st.get("see")), rng_of("x"), "day")
            if given:
                sp.append("Light: the text gives it (%s)." % mood)
            elif entry.get("seasons"):
                sp.append("Light and season: not said for this site. How the year turns at this port: %s" % dot(entry.get("seasons")))
            else:
                sp.append("Light and season: not given in the data.")
            if st.get("source"):
                sp.append("Source: %s" % dot(clip(st.get("source"), 200)))
            sp.append("The placeholder: what stands in it is by the site's kind (%s)." % kindw)
            sp.append(size_line("scene"))
            sp.append(file_line("scene", "site." + st["id"]))
            slot("site." + st["id"], "scene", "%s, %s" % (s(st.get("name")) or kindw, port.get("name")), " ".join(x for x in sp if x), rel(path),
                 draw=lambda st=st, port=port: artscene.draw_scene(st, port, *SIZES["scene"][:2]))
        # the stories of the port
        for story in lst(entry.get("stories")):
            if isinstance(story, dict) and story.get("id"):
                story_slot(story, rel(path), port)
        # the works of the port
        for wk in lst(entry.get("works")):
            if isinstance(wk, dict) and wk.get("id"):
                work_slot(wk, rel(path), port)
        # the ships to be had here
        for sh in lst(entry.get("ships")):
            if isinstance(sh, dict) and sh.get("class"):
                cls = dict(W.ship_classes.get(sh["class"], ({"id": sh["class"]}, ""))[0])
                sid = "shipat.%s.%s" % (pid, sh["class"])
                sp = ["A %s as she can be had at %s: %s." % (s(cls.get("name")) or sh["class"], name, s(sh.get("how")) or "how is not said")]
                sp.append("Picture: %s" % dot(sh.get("picture")) if sh.get("picture") else "Picture: no line written yet ('picture' is empty).")
                if sh.get("note"):
                    sp.append("Why here: %s" % dot(sh.get("note")))
                if cls.get("look"):
                    sp.append("The ship: %s" % dot(cls.get("look")))
                yrs = years(sh.get("from"), sh.get("to"))
                if yrs:
                    sp.append("Offered %s." % yrs)
                sp.append("Light and season: only what the picture line says.")
                sp.append(size_line("ship_offer")); sp.append(file_line("ship_offer", sid))
                slot(sid, "ship_offer", "%s at %s" % (s(cls.get("name")) or sh["class"], port.get("name")), " ".join(x for x in sp if x), rel(path),
                     draw=lambda cls=cls, port=port, sh=sh: artscene.draw_ship_offer(cls, port, s(sh.get("picture")), *SIZES["ship_offer"][:2]))


def story_slot(story, source, port=None, age=None):
    sid = "story." + story["id"]
    title = s(story.get("title")) or story["id"]
    parts = ["Picture for the story '%s'%s." % (title, (" at " + port_name(port.get("id"))) if port else "")]
    if story.get("picture"):
        parts.append("Picture: %s" % dot(story.get("picture")))
    else:
        parts.append("Picture: no line written yet; draw from the story itself.")
    if story.get("summary"):
        parts.append("The story: %s" % dot(story.get("summary")))
    if story.get("text"):
        parts.append("The story: %s" % dot(clip(story.get("text"), 700)))
    if story.get("year"):
        parts.append("Year: %s." % s(story.get("year")))
    if story.get("where"):
        parts.append("Where: %s." % ", ".join(port_name(x) for x in lst(story.get("where"))))
    beats = [dot(b.get("text")) for b in lst(story.get("beats")) if isinstance(b, dict) and b.get("text")]
    if beats:
        parts.append("Its steps: %s" % " ".join(clip(b, 160) for b in beats[:5]))
    ends = [s(e.get("id")) + ": " + clip(e.get("text"), 120) for e in lst(story.get("endings")) if isinstance(e, dict)]
    if ends:
        parts.append("It can end: %s." % "; ".join(ends))
    if age:
        parts.append("A story of the age '%s' (%s)." % (s(age.get("name")), years(age.get("from"), age.get("to"))))
    if story.get("source"):
        parts.append("Source: %s" % dot(clip(story.get("source"), 200)))
    parts.append(never("helmets", "chimneys", "stave", "compass", "sea charts"))
    parts.append("Light and season: only what the lines above say.")
    parts.append(size_line("story")); parts.append(file_line("story", sid))
    texts = [(3, title), (2, s(story.get("picture"))), (1, s(story.get("summary")) + " " + s(story.get("text")))]
    wp = port or W.ports.get((lst(story.get("where")) or [None])[0])
    slot(sid, "story", title, " ".join(x for x in parts if x), source,
         draw=lambda: artscene.vignette(sid, "harbour", texts, (), wp, *SIZES["story"][:2], title=title)[0])


def work_slot(wk, source, port=None):
    sid = "work." + wk["id"]
    name = s(wk.get("name")) or wk["id"]
    parts = ["A work a house can pay for%s: %s." % ((" at " + port_name(port.get("id"))) if port else " (the kind of work, in any port where it is true)", name)]
    if wk.get("picture"):
        parts.append("Picture: %s" % dot(wk.get("picture")))
    if wk.get("what"):
        parts.append("What it is: %s" % dot(wk.get("what")))
    if wk.get("evidence"):
        parts.append("Evidence: %s" % dot(clip(wk.get("evidence"), 420)))
    if wk.get("where"):
        parts.append("Where it stands: %s." % s(wk.get("where")))
    yrs = years(wk.get("from"), wk.get("to"))
    if yrs:
        parts.append("Possible %s." % yrs)
    if wk.get("gives"):
        parts.append("What it gives: %s" % dot(clip(wk.get("gives"), 240)))
    parts.append(never("chimneys", "stave", "clocks"))
    parts.append("Light and season: not given.")
    parts.append(size_line("work")); parts.append(file_line("work", sid))
    slot(sid, "work", name + ((", " + s(port.get("name"))) if port else ""), " ".join(x for x in parts if x), source,
         draw=lambda: artdraw.draw_work(wk, *SIZES["work"][:2]))


# ------------------------------------------------------------------------------------------------ cards
def card_title(c):
    t = W.writing.get(s(c.get("title_key")))
    return t[0] if t else (s(c.get("title")) or s(c.get("id")))


def when_words(when):
    """The plain facts in a card's condition: where, which years, what weather."""
    w = s(when)
    out = []
    ports = re.findall(r"port\.id\s*=\s*([a-z0-9_]+)", w)
    if ports:
        out.append("Where: %s." % " or ".join(port_name(p) for p in dict.fromkeys(ports)))
    for name, word in (("port.realm", "realm"), ("port.kind", "kind of port"), ("port.faith", "faith of the port"), ("port.people", "people of the port")):
        m = re.findall(re.escape(name) + r"\s*(!?=)\s*([a-z0-9_ ]+?)(?=\s+and|\s+or|\)|$)", w)
        for op, val in m:
            out.append("%s %s %s." % (word.capitalize(), "is" if op == "=" else "is not", val.strip().replace("_", " ")))
    yrs = re.findall(r"\byear\s*(<=|>=|<|>|=)\s*(\d{3,4})", w)
    if yrs:
        out.append("Years: %s." % ", ".join({"<": "before ", "<=": "to ", ">": "after ", ">=": "from ", "=": "in "}[op] + y for op, y in yrs))
    mon = re.findall(r"\bmonth\s*(<=|>=|<|>|=)\s*(\d{1,2})", w)
    if mon:
        out.append("Months: %s." % ", ".join(op + " " + m for op, m in mon))
    for flag, word in (("storm", "in a storm"), ("fog", "in fog"), ("ice", "in ice"), ("feast", "on a feast day"), ("fast", "in a fast")):
        m = re.search(r"(?<!not )\b%s\b(?!\.)\s*(<=|>=|<|>|!=|=)?\s*([0-9.]+|false|no)?" % flag, w)
        if m and not re.search(r"not\s+%s\b" % flag, w):
            op, val = m.group(1), m.group(2)
            if op in ("<", "<=", "!=") or (op == "=" and val in ("0", "false", "no")):
                continue                                     # 'storm < 0.1' asks for no storm: say nothing
            out.append("It comes %s." % word)
    return " ".join(out), (ports[0] if ports else None)


def collect_cards():
    for cid, (c, path) in W.cards.items():
        title = card_title(c)
        stage = s(c.get("stage")) or "harbour"
        art = W.writing.get("art." + cid)
        intention = W.writing.get(s(c.get("intention_key")))
        intention = intention[0] if intention else s(c.get("intention"))
        roles = [s(dct(a).get("role")) for a in dct(c.get("actors")).values() if s(dct(a).get("role"))]
        ww, pid = when_words(c.get("when"))
        m = re.match(r"^[a-z]+\.chain_([a-z0-9_]+)\.", cid)
        chain = m.group(1) if m else ""
        md_pic = W.chain_step_pictures.get(cid) if not art else None
        parts = ["Picture on the card '%s' (a %s card%s)." % (title, stage, (" of the chain " + chain) if chain else "")]
        if art:
            parts.append("Picture: %s" % dot(art[0]))
        elif md_pic:
            parts.append("Picture (from the chain's page; no art.%s line yet): %s" % (cid, dot(md_pic[0])))
        else:
            parts.append("Picture: no line written yet (art.%s); made here from the card's title and what it is for." % cid)
        if intention:
            parts.append("What the card is: %s" % dot(intention))
        if roles:
            parts.append("People in it: %s." % ", ".join(role_title(r) for r in roles))
        if ww:
            parts.append(ww)
        if c.get("when"):
            parts.append("The card's own condition: \"%s\"." % s(c.get("when")))
        labels = [s(o.get("label")) for o in lst(c.get("options")) if isinstance(o, dict) and o.get("label")]
        if labels:
            parts.append("The choices: %s." % "; ".join(labels))
        if c.get("note"):
            parts.append("Note: %s" % dot(clip(c.get("note"), 300)))
        parts.append({"harbour": "Stage harbour: she lies at a port; the picture is ashore or at the landing.",
                      "sea": "Stage sea: she is under way; the picture is the open water as seen from or near the boat (7.7 m, four men, one square sail, at the start).",
                      "crew": "Stage crew: it is among the men aboard; the picture is in the boat.",
                      "long": "Stage long: it is a matter of the house over months; the picture is at home or in a hall."}.get(stage, ""))
        parts.append(never("helmets", "buttons", "chimneys", "stave", "compass", "sea charts", "clocks"))
        if not art and not md_pic:
            parts.append("Light and season: not given; only what the condition above says.")
        else:
            parts.append("Light and season: as the picture line says.")
        parts.append("The placeholder: the ground is the stage, the small figures are the card's people with the signs of their roles, the large signs are the plain words of the card.")
        parts.append(size_line("card")); parts.append(file_line("card", "card." + cid))
        srcs = rel(path) + ((", " + art[1]) if art else "") + ((", " + md_pic[1]) if md_pic else "")
        texts = [(3, title), (2, art[0] if art else (md_pic[0] if md_pic else "")), (1, intention), (0.5, s(c.get("note")) + " " + cid.replace("_", " ").replace(".", " "))]
        port = W.ports.get(pid) if pid else None
        slot("card." + cid, "card", title, " ".join(x for x in parts if x), srcs,
             draw=lambda cid=cid, stage=stage, texts=texts, roles=roles, port=port, title=title:
             artscene.vignette(cid, stage, texts, roles, port, *SIZES["card"][:2], title=title)[0])
    # the older table of things that happen (events_793.json): cards too
    epath = os.path.join(DATA, "events_793.json")
    for e in lst((jload(epath) or {}).get("events")):
        if not isinstance(e, dict) or not e.get("id"):
            continue
        eid = "event." + e["id"]
        title = s(e.get("title")) or e["id"]
        stage = "sea" if s(e.get("where")) == "sea" else "harbour"
        parts = ["Picture on the card '%s' (it happens %s)." % (title, "at sea" if stage == "sea" else "in port"),
                 "Picture: no line written; made here from the card's title and what it is for.",
                 "What the card is: %s" % dot(e.get("intention")),
                 "The choices: %s." % "; ".join(s(o.get("label") or o.get("text") or o.get("key")) for o in lst(e.get("options")) if isinstance(o, dict)),
                 never("helmets", "buttons", "chimneys", "compass", "sea charts"),
                 "Light and season: not given.", size_line("card"), file_line("card", eid)]
        texts = [(3, title), (1, s(e.get("intention")))]
        slot(eid, "card", title, " ".join(x for x in parts if x), rel(epath),
             draw=lambda eid=eid, stage=stage, texts=texts, title=title: artscene.vignette(eid, stage, texts, (), None, *SIZES["card"][:2], title=title)[0])


# ------------------------------------------------------------------------------------------------ chains: the pages and their endings
def split_row(line):
    return [c.strip() for c in line.strip().strip("|").split("|")]


def clean_md(text):
    text = re.sub(r"(?<![A-Za-z0-9])_{1,3}|_{1,3}(?![A-Za-z0-9])", "", re.sub(r"[`*]{1,3}", "", s(text)))
    return re.sub(r"\s+", " ", text).strip(" :;-")


def chain_endings(lines, stem):
    """The endings of a chain, one for each ending flag ch_<id>_end_<name> the page names. The name, what becomes of it
    and the picture are looked for wherever writers put them: a table under an Endings heading, a bold line with the
    flag, a step row that says 'Ending', and the lines of a Pictures section."""
    flag_re = re.compile(r"\bch_%s_end_([a-z0-9_]+)" % re.escape(stem))
    if not any(flag_re.search(l) for l in lines):
        flag_re = re.compile(r"\bch_[a-z0-9_]+?_end_([a-z0-9_]+)")
    keys = []
    for l in lines:
        for k in flag_re.findall(l):
            if k not in keys:
                keys.append(k)
    if not keys:
        return []
    # the page by sections
    sect, cur, header = [], "", None
    for l in lines:
        if l.startswith("#"):
            cur, header = clean_md(l.lstrip("#")).lower(), None
            sect.append((cur, l, None, None))
            continue
        row = l.strip().startswith("|")
        cells = split_row(l) if row else None
        if row and set("".join(cells)) <= set("-: "):
            continue
        if row and header is None:
            header = [clean_md(c).lower() for c in cells]
            continue
        if not row and not l.strip():
            header = None
        sect.append((cur, l, cells, header if row else None))
    out = {k: {"key": k, "name": "", "picture": "", "text": ""} for k in keys}

    def flags_in(text):
        return flag_re.findall(text)

    # 1. tables and bold lines under a heading that says 'ending'
    for k in keys:
        e, texts = out[k], []
        for n, (cur, l, cells, header) in enumerate(sect):
            fl = flags_in(l)
            if "ending" not in cur or k not in fl:
                continue
            if cells:
                flag_cols = [i for i, c in enumerate(cells) if flags_in(c)]
                name = clean_md(cells[0]) if not flags_in(cells[0]) else ""
                if fl[0] == k and name and not e["name"]:
                    e["name"] = name
                bits = []
                for i, c in enumerate(cells):
                    if i == 0 or i in flag_cols or not clean_md(c):
                        continue
                    h = header[i] if header and i < len(header) else ""
                    bits.append(("%s: %s" % (h.capitalize(), clean_md(c))) if h else clean_md(c))
                body = " ".join(dot(b) for b in bits)
                if name and (fl[0] != k or (e["name"] and name != e["name"])):
                    body = "(%s) %s" % (name, body)
                if body:
                    texts.append(body)
            elif fl[0] == k:
                bm = re.match(r"\s*(?:[-*]\s+)?\*\*(?:\d+\.\s*)?(.+?)\*\*", l)
                if bm and not e["name"]:
                    e["name"] = clean_md(bm.group(1)).rstrip(".")
                body = clean_md(re.sub(r"^\s*(?:[-*]|\d+\.)\s+", "", l))
                if bm and body:
                    for _, l2, c2, _h in sect[n + 1:]:              # the rest of the paragraph the line opens
                        if not l2.strip() or l2.startswith("#") or c2 or flags_in(l2):
                            break
                        body += " " + clean_md(l2)
                    texts.append(body)
        e["text"] = " ".join(texts)
    # 2. a step row that says 'Ending: the name'
    for k in keys:
        e = out[k]
        for cur, l, cells, header in sect:
            fl = flags_in(l)
            if fl and fl[0] == k and (not e["name"] or not e["text"]):
                m = re.search(r"(?i)\bending:\s*([^.|;]+)", clean_md(l))
                if m and not e["name"]:
                    e["name"] = m.group(1).strip()
                if not e["text"] and cells:
                    e["text"] = " ".join(dot(clean_md(c)) for c in cells if clean_md(c) and not flags_in(c))
        if not e["name"]:
            e["name"] = k.replace("_", " ")
    # 3. the picture: a line of a Pictures section (or a line that says 'picture:') that names the ending
    pic_lines, on, open_ = [], False, False                      # open_: the last picture line may run on to the next line
    for cur, l, cells, header in sect:
        if l.startswith("#"):
            on = open_ = False
            continue
        if not l.strip():
            open_ = False
            continue
        if re.search(r"(?i)\bfor each ending\b", l):
            on, open_ = True, False
            continue
        starts = cells is not None or re.match(r"\s*(?:[-*]\s|\d+\.\s|\*\*)", l)
        if open_ and not starts:
            pic_lines[-1] = (pic_lines[-1][0].rstrip() + " " + l.strip(), None, None)
            continue
        open_ = False
        if "picture" in cur or on or re.search(r"(?i)\bpicture\b\s*[:=]", l):
            pic_lines.append((l, cells, header))
            open_ = cells is None
    used = set()
    for k in sorted(keys, key=lambda x: -len(x)):
        e, words = out[k], [w for w in k.split("_") if w]
        name_l = re.sub(r"^(the|a|an)\s+", "", e["name"].lower())
        for i, (l, cells, header) in enumerate(pic_lines):
            if i in used:
                continue
            low = clean_md(l).lower()
            head = low[:90]
            by_flag = ("end_" + k) in low or re.search(r"[a-z0-9]_%s\b(?!_)" % re.escape(k), low)
            by_name = len(name_l) > 3 and name_l in low
            by_word = re.search(r"\bending\b", head) and all(re.search(r"\b%s" % re.escape(w[:6]), head) for w in words)
            if by_flag or by_name or by_word:
                used.add(i)
                if cells:                                         # a row of a table of pictures: the cell that is the picture
                    named = [clean_md(c) for h, c in zip(header or [], cells) if re.search(r"picture|shows|drawn|art\b", h)]
                    rest = [clean_md(c) for c in cells if not flags_in(c)]
                    pic = (named[0] if named and named[0] else max(rest, key=len)) if (named or rest) else ""
                else:
                    pic = clean_md(re.sub(r"^\s*(?:[-*]|\d+\.)\s+", "", l))
                    pic = re.sub(r"(?i)^picture\s*[:=]\s*", "", pic)
                    # a short label in front that only says which ending it is ('Ending, feud:', 's09_count:', 'Sold (s14_sold).')
                    m = re.match(r"(.{2,70}?)(?::|\)\.|\):)\s+(?=\S)", pic)
                    if m and len(pic) - m.end() > 30 and (re.search(r"(?i)\bending\b|\(|_", m.group(1)) or name_l in m.group(1).lower()):
                        pic = pic[m.end():]
                        pic = pic[:1].upper() + pic[1:]
                e["picture"] = pic
                break
    # an ending whose name only adds to another's (friend_hard to friend) is a finale of it
    for k in keys:
        e = out[k]
        parent = max((p for p in keys if p != k and k.startswith(p + "_")), key=len, default=None)
        e["of"] = parent or ""
        if parent:
            if not e["picture"] and out[parent]["picture"]:
                e["picture_of"] = out[parent]["picture"]
            if e["name"] == k.replace("_", " ") and out[parent]["name"]:
                e["name"] = "%s (%s)" % (out[parent]["name"], k[len(parent) + 1:].replace("_", " "))
    return [out[k] for k in keys]


def parse_chain_page(path):
    """The endings of a chain and the picture of each step, read from the chain's page however it is laid out."""
    text = tload(path)
    stem = os.path.splitext(os.path.basename(path))[0]
    lines = text.splitlines()
    title = next((clean_md(l.lstrip("#")) for l in lines if l.startswith("# ")), stem)
    steps, endings, order = {}, {}, []
    header, section = None, ""
    card_re = re.compile(r"\b(?:harbour|sea|crew|long)\.chain_[a-z0-9_]+\.[a-z0-9_]+")
    flag_re = re.compile(r"\bch_[a-z0-9_]+?_end_([a-z0-9_]+)")
    for i, line in enumerate(lines):
        if line.startswith("#"):
            section, header = clean_md(line.lstrip("#")).lower(), None
            continue
        is_row = line.strip().startswith("|")
        cells = split_row(line) if is_row else []
        if is_row and set("".join(cells)) <= set("-: "):
            continue
        if is_row and header is None:
            header = [c.lower() for c in cells]
            continue
        if not is_row:
            header = None if not line.strip() else header
        pic = ""
        if is_row and header:
            for h, c in zip(header, cells):
                if "picture" in h or "art" == h.strip():
                    pic = clean_md(c)
        m = re.search(r"(?i)\bpicture\b\s*[:=]\s*(.+)", line)
        if not pic and m:
            pic = clean_md(re.split(r"(?i)\s+\b(flag|reward|payout|pays)\b\s*:", m.group(1))[0])
        cm = card_re.search(line)
        fm = flag_re.search(line)
        in_end = "ending" in section
        if cm and not fm and pic:
            steps.setdefault(cm.group(0), pic)
        if fm or (in_end and (is_row or re.match(r"\s*(?:[-*]|\d+\.)\s+", line))):
            if fm:
                key = fm.group(1)
            else:
                first = clean_md(cells[0] if is_row else re.sub(r"^\s*(?:[-*]|\d+\.)\s+", "", line).split(":")[0].split(".")[0])
                key = safe_id(first.lower())[:40]
                if not key or key in ("ending", "x"):
                    continue
            e = endings.setdefault(key, {"key": key, "name": "", "picture": "", "text": ""})
            if key not in order:
                order.append(key)
            if is_row:
                name = clean_md(cells[0]) if cells else ""
                if name and not flag_re.search(cells[0]) and not e["name"]:
                    e["name"] = name
                body = " | ".join(clean_md(c) for c in cells[1:] if c and not flag_re.fullmatch(c.strip("` ")))
            else:
                bm = re.search(r"\*\*(.+?)\*\*", line)
                if bm and not e["name"]:
                    e["name"] = clean_md(bm.group(1))
                body = clean_md(re.sub(r"^\s*(?:[-*]|\d+\.)\s+", "", line))
            if pic and not e["picture"]:
                e["picture"] = pic
            if body and len(body) > len(e["text"]):
                e["text"] = body
    # a "Pictures" section may name the endings by flag or by name on lines of their own
    for key in order:
        e = endings[key]
        if not e["name"]:
            e["name"] = key.replace("_", " ")
        if not e["picture"]:
            for line in lines:
                if re.search(r"(?i)picture", line) and (("end_" + key) in line or e["name"].lower() in line.lower()):
                    m = re.search(r"(?i)\bpicture\b\s*[:=]\s*(.+)", line)
                    if m:
                        e["picture"] = clean_md(m.group(1))
                        break
    summary = ""
    for i, line in enumerate(lines):
        if line.startswith("#") and re.search(r"(?i)story|summary|in five|what it is", line):
            body = []
            for l2 in lines[i + 1:]:
                if l2.startswith("#"):
                    break
                body.append(l2)
            summary = clean_md(" ".join(body))
            break
    if not summary:
        paras = [clean_md(x) for x in re.split(r"\n\s*\n", text) if x.strip() and not x.strip().startswith(("#", "|"))]
        summary = paras[0] if paras else ""
    return {"id": stem, "title": title, "summary": summary, "endings": chain_endings(lines, stem) or [endings[k] for k in order], "steps": steps}


def collect_chains():
    W.chain_step_pictures = {}
    W.chain_pages = []
    for path in sorted(glob.glob(os.path.join(CONTENT, "chains", "*.md"))):
        try:
            page = parse_chain_page(path)
        except Exception as e:
            SKIPPED.append("%s: could not be read as a chain page (%s)" % (rel(path), str(e)[:80]))
            continue
        W.chain_pages.append((page, path))
        for cid, pic in page["steps"].items():
            W.chain_step_pictures[cid] = (pic, rel(path))


def collect_endings():
    for page, path in W.chain_pages:
        if not page["endings"]:
            SKIPPED.append("%s: no endings found on the chain's page (looked for flags ch_<id>_end_<name> and an Endings heading)" % rel(path))
        for e in page["endings"]:
            sid = "ending.%s.%s" % (page["id"], e["key"])
            parts = ["Ending '%s' of the chain '%s'." % (e["name"], page["title"])]
            if e.get("of"):
                parts.append("It is a finale of the ending '%s' (flag ch_%s_end_%s) and needs a picture of its own only if the two should look different." % (
                    e["of"].replace("_", " "), page["id"], e["of"]))
            if e["picture"]:
                parts.append("Picture: %s" % dot(e["picture"]))
            elif e.get("picture_of"):
                parts.append("Picture: no line of its own on the chain's page. The picture line of '%s': %s" % (e["of"].replace("_", " "), dot(e["picture_of"])))
            else:
                parts.append("Picture: no line found on the chain's page; draw from what becomes of it.")
            if e["text"]:
                parts.append("What becomes of it: %s" % dot(clip(e["text"], 1100)))
            if page["summary"]:
                parts.append("The chain: %s" % dot(clip(page["summary"], 500)))
            parts.append("Flag: ch_%s_end_%s." % (page["id"], e["key"]))
            parts.append(never("helmets", "buttons", "chimneys", "stave", "compass", "sea charts", "clocks"))
            parts.append("Light and season: as the picture line says, else not given.")
            parts.append(size_line("ending")); parts.append(file_line("ending", sid))
            pic = e["picture"] or e.get("picture_of") or ""
            texts = [(3, e["name"]), (2, pic), (1, e["text"])]
            stage = "long" if re.search(r"(?i)\b(hall|inside|church|storehouse|firelight|hearth|lamp)\b", pic[:160]) else "harbour"
            parts.insert(-2, "The placeholder: %s; the two large signs are the plain words of the ending." % (
                "a hall by firelight, since the picture line is indoors" if stage == "long" else "a waterfront"))
            slot(sid, "ending", "%s: %s" % (page["title"], e["name"]), " ".join(x for x in parts if x), rel(path),
                 draw=lambda sid=sid, texts=texts, e=e, stage=stage: artscene.vignette(sid, stage, texts, (), None, *SIZES["ending"][:2],
                                                                          title=e["name"], frame=artkit.GOLD)[0])


# ------------------------------------------------------------------------------------------------ goods, ships, icons
def collect_goods():
    path = os.path.join(DATA, "goods_793.json")
    d = jload(path) or {}
    regions = dct(d.get("regions"))
    for g in lst(d.get("goods")):
        if not isinstance(g, dict) or not g.get("id"):
            continue
        sid = "good." + g["id"]
        yr0, yr1 = int(artdraw.num(g.get("from"), 0)), int(artdraw.num(g.get("to"), 0))
        traded = yr0 <= 805 and (yr1 == 0 or yr1 >= 793)
        if traded and (g["id"] in first_hold() or home_region() in dct(g.get("made"))):
            FIRST_HOUR.add(safe_id(sid))                     # aboard on the first morning, or made where the game starts
        elif traded and set(dct(g.get("made"))) & first_regions():
            FIRST_IDS.add(safe_id(sid))
        made = [k for k, v in sorted(dct(g.get("made")).items(), key=lambda kv: -artdraw.num(kv[1]))]
        parts = ["Icon of %s%s, as it is traded: one %s (%s kg)." % (s(g.get("name")), (" (then: %s)" % s(g.get("period"))) if g.get("period") else "",
                                                                       s(g.get("unit")) or "unit", s(g.get("kg")))]
        parts.append("Kind: %s." % s(g.get("cat")))
        if g.get("note"):
            parts.append(dot(g.get("note")))
        if made:
            parts.append("Made in: %s." % ", ".join(clip(regions.get(k, k), 60) for k in made[:3]))
        yrs = years(g.get("from"), g.get("to"))
        if yrs:
            parts.append("Traded %s." % yrs)
        if g.get("law"):
            parts.append("The law on it: %s." % s(g.get("law")))
        parts.append("It must read at a glance among %d goods and be told from the others of its kind; no letters on it." % len(lst(d.get("goods"))))
        parts.append("The placeholder: the shape is how it is traded (%s), the colours are its kind." % artdraw.good_shape(g))
        parts.append("Light and season: none. Clear background.")
        parts.append(size_line("good")); parts.append(file_line("good", sid))
        slot(sid, "good", s(g.get("name")), " ".join(x for x in parts if x), rel(path), draw=lambda g=g: artdraw.draw_good(g, *SIZES["good"][:2]))


def collect_ships():
    ships = dict(W.ship_classes)
    for fn, (d, path) in W.story_files.items():
        if "ship" in fn and "upgrade" not in fn:
            for key in ("ships", "classes"):
                for c in lst(d.get(key)):
                    if isinstance(c, dict) and c.get("id"):
                        base = dict(ships.get(c.get("class") or c.get("ship_class") or "", ({}, ""))[0])
                        base.update(c)
                        ships[c["id"]] = (base, path)
    for fn, (d, path) in W.story_files.items():
        for u in lst(d.get("unlocks")):
            if isinstance(u, dict) and u.get("kind") == "ship":
                cid = s(u.get("ship_class")) or re.sub(r"^unlock_ship_", "", s(u.get("id")))
                if cid and cid not in ships:
                    c = {"id": cid, "name": s(u.get("name")), "look": s(u.get("what_it_is")), "from": u.get("from"), "to": u.get("to"), "mast": True,
                         "sail": "square", "_thin": True}
                    m = re.search(r"(\d+(?:\.\d+)?)\s*m\b", s(u.get("what_it_is")))
                    if m:
                        c["length_m"] = float(m.group(1))
                        c["sail_top_m"] = c["length_m"] * 0.65
                    ships[cid] = (c, path)
    for pid in FIRST_PORTS:                                  # the classes that can be had in the twelve ports by 805
        for sh in lst(W.places.get(pid, ({}, ""))[0].get("ships")):
            if isinstance(sh, dict) and sh.get("class") and int(artdraw.num(sh.get("from"), 0)) <= 805:
                FIRST_IDS.add(safe_id("ship." + s(sh["class"]))); FIRST_IDS.add(safe_id("shipmark." + s(sh["class"])))
    for cid, (c, path) in ships.items():
        t = artdraw.ship_traits(c)
        common = []
        common.append("Then called: %s." % s(c.get("period_name")) if c.get("period_name") else "")
        nums = []
        for key, word in (("length_m", "long"), ("beam_m", "beam"), ("freeboard_m", "freeboard amidships"), ("hull_top_m", "to the top of the stems"),
                          ("sail_top_m", "to the masthead"), ("draught_m", "draught")):
            if c.get(key) is not None:
                nums.append("%s m %s" % (s(c.get(key)), word))
        if nums:
            common.append("Measures: %s." % ", ".join(nums))
        if c.get("sail") is not None or c.get("oars") is not None:
            common.append("Sail: %s. Oars: %s. Crew: %s%s." % (s(c.get("sail")) or "none", s(c.get("oars")) or "none", s(c.get("crew")) or "not given",
                                                             (" (%s)" % s(c.get("crew_range"))) if c.get("crew_range") else ""))
        if c.get("look"):
            common.append("How she looks: %s" % dot(c.get("look")))
        if c.get("picture"):
            common.append("Picture: %s" % dot(c.get("picture")))
        if c.get("cultures"):
            common.append("Whose: %s." % s(c.get("cultures")).replace("_", " "))
        yrs = years(c.get("from"), c.get("to"))
        if yrs:
            common.append("On the water %s." % yrs)
        for ev in lst(c.get("evidence"))[:1]:
            common.append("Evidence: %s" % dot(clip(ev, 330)))
        for u in lst(c.get("uncertain"))[:2]:
            common.append("Not certain: %s" % dot(clip(u, 200)))
        if c.get("_thin"):
            common.append("Her class is not yet in ship_classes_793.json: the placeholder guesses her from the words above.")
        common.append(never("compass", "helmets"))
        common = " ".join(x for x in common if x)
        name = s(c.get("name")) or cid
        a = "%s from the side, bow to the right, lying on calm water, at 6 pixels to the metre like every other class, so that she can be set beside them. Men aboard give the scale. Clear sky: nothing behind her but the waterline." % name
        slot("ship." + cid, "ship_side", name, " ".join([a, common, "Light and season: plain daylight.", size_line("ship_side"), file_line("ship_side", "ship." + cid)]),
             rel(path), draw=lambda c=c, cid=cid: artdraw.draw_ship_side(c, *SIZES["ship_side"][:2], label=cid))
        b = "%s seen from straight above for the chart, bow to the right (the game turns it to her course), at 1.25 pixels to the metre: %.0f x %.0f pixels of hull in the 32 x 16 picture. Hull, the line of the yard and sail if she has one, nothing else; a dark edge so she reads on the sea." % (
            name, max(6, t["L"] * 1.25), max(3, t["B"] * 1.25 + 1.5))
        slot("shipmark." + cid, "ship_mark", name, " ".join([b, common, size_line("ship_mark"), file_line("ship_mark", "shipmark." + cid)]),
             rel(path), draw=lambda c=c: artdraw.draw_ship_mark(c, *SIZES["ship_mark"][:2]))


def icon_slot(sid, kind, name, parts, source, ident, sign_name, text, plate=None, tag=None):
    parts = [x for x in parts if x]
    parts.append("An icon: one clear thing, no letters, readable at 32 x 32 on the paper and on the ink panel.")
    main, minor = artdraw.sign_for(ident, sign_name, text)
    parts.append("The placeholder: the sign of %s%s%s." % (main or "nothing fitting (its first letters instead)", (" with %s" % minor) if minor else "",
                                                           (", and the letters %s in the corner to tell it from the others with the same sign (the real icon has no letters)" % artkit.ascii_up(tag)) if tag else ""))
    parts.append(never("compass", "clocks", "buttons", "forks"))
    parts.append(size_line(kind)); parts.append(file_line(kind, sid))
    slot(sid, kind, name, " ".join(x for x in parts if x), source,
         draw=lambda: artdraw.draw_icon(ident, sign_name, text, *SIZES[kind][:2], plate=plate, tag=tag))


def unlock_tags():
    """Three letters for each thing the house can win, different within its kind: Colonia's charter is COL."""
    by_kind = {}
    for fn, (d, path) in W.story_files.items():
        for u in lst(d.get("unlocks")):
            if isinstance(u, dict) and u.get("id"):
                kind = s(u.get("kind"))
                ident = re.sub(r"^unlock_(?:%s)_" % re.escape(kind), "", u["id"]) if kind else u["id"]
                by_kind.setdefault(kind, set()).add((u["id"], re.sub(r"^unlock_", "", ident)))
    tags = {}
    for kind, items in by_kind.items():
        used = set()
        for uid, ident in sorted(items):
            words = [w for w in re.split(r"[^a-z0-9]+", ident.lower()) if w] or ["x"]
            first, last = words[0], words[-1]
            cands = [last[:3], (first[0] + last[:2]) if len(words) > 1 else "", last[:2] + last[-1], last[0] + last[-2:], "".join(words)[:3]]
            cands += [last[:2] + str(i) for i in range(1, 100)]
            tag = next(c for c in cands if c and c not in used)
            used.add(tag)
            tags[uid] = tag
    return tags


def collect_story_tables():
    acts_seen = set()
    tags = unlock_tags()
    first_acts = set()                                       # the acts the sites of the twelve ports offer
    for pid in FIRST_PORTS:
        for st in lst(W.places.get(pid, ({}, ""))[0].get("sites")):
            for d_ in lst(dct(st).get("do")):
                if s(d_).startswith("act:"):
                    first_acts.add(s(d_)[4:])
    for a in first_acts:
        FIRST_IDS.add(safe_id("act." + a))
    for fn, (d, path) in W.story_files.items():
        # town acts
        for a in lst(d.get("acts")) + lst(d.get("actions")):
            if isinstance(a, dict) and a.get("id"):
                acts_seen.add(a["id"])
                parts = ["Icon of the act '%s': the button a site shows for it." % s(a.get("name")),
                         "What it was: %s" % dot(a.get("what")) if a.get("what") else "",
                         "At: %s." % s(a.get("at")) if a.get("at") else "",
                         "Done with: %s." % role_title(a.get("who")) if s(a.get("who")) not in ("", "none") else "",
                         "As the screen says it: %s" % dot(dct(a.get("words")).get("offer")) if dct(a.get("words")).get("offer") else "",
                         "Evidence: %s" % dot(clip(a.get("evidence"), 300)) if a.get("evidence") else ""]
                icon_slot("act." + a["id"], "act", s(a.get("name")) or a["id"], parts, rel(path), a["id"], s(a.get("name")), s(a.get("what")))
        # town games
        for g in lst(d.get("games")):
            if isinstance(g, dict) and g.get("id"):
                parts = ["Icon of the game '%s'." % s(g.get("name")),
                         "Its picture, which the icon should be a small part of: %s" % dot(g.get("picture")) if g.get("picture") else "",
                         "On screen: %s." % s(g.get("screen")) if g.get("screen") else "",
                         "Played with: %s." % ", ".join(role_title(x) for x in lst(g.get("with"))) if g.get("with") else "",
                         "Among: %s." % s(g.get("peoples")) if g.get("peoples") else "",
                         "Years: %s." % years(g.get("from"), g.get("to")) if years(g.get("from"), g.get("to")) else "",
                         "Evidence: %s" % dot(clip((lst(g.get("evidence")) or [""])[0], 300)) if g.get("evidence") else "",
                         "Not known: %s" % dot(clip(g.get("not_known"), 240)) if g.get("not_known") else ""]
                if s(g.get("act")) in first_acts and int(artdraw.num(g.get("from"), 0)) <= 805:
                    FIRST_IDS.add(safe_id("game." + g["id"]))
                icon_slot("game." + g["id"], "game", s(g.get("name")) or g["id"], parts, rel(path), s(g.get("act")) or g["id"],
                          s(g.get("name")) + " " + g["id"], s(g.get("picture")))
        # ship upgrades in a table of their own
        if "upgrade" in fn:
            for u in lst(d.get("upgrades")) + lst(d.get("items")):
                if isinstance(u, dict) and u.get("id"):
                    parts = ["Icon of the ship's upgrade '%s'." % (s(u.get("name")) or u["id"]),
                             "Picture: %s" % dot(u.get("picture")) if u.get("picture") else "",
                             "What it is: %s" % dot(u.get("what") or u.get("what_it_is")) if (u.get("what") or u.get("what_it_is")) else "",
                             "Years: %s." % years(u.get("from"), u.get("to")) if years(u.get("from"), u.get("to")) else ""]
                    at = lst(dct(u.get("where")).get("ports"))
                    if int(artdraw.num(u.get("from"), 0)) <= 805 and (not at or set(at) & set(FIRST_PORTS)):
                        FIRST_IDS.add(safe_id("upgrade." + re.sub(r"^unlock_upgrade_", "", u["id"])))
                    icon_slot("upgrade." + re.sub(r"^unlock_upgrade_", "", u["id"]), "upgrade", s(u.get("name")) or u["id"], parts, rel(path),
                              u["id"], s(u.get("name")), s(u.get("what") or u.get("what_it_is")))
        # everything the house can win
        for u in lst(d.get("unlocks")):
            if not isinstance(u, dict) or not u.get("id"):
                continue
            kind = s(u.get("kind"))
            if kind in ("ship", "text", "riddle_reward"):
                continue                                     # ships have their views, texts and riddles their leaves
            how = dct(u.get("how_won"))
            parts = ["Icon of %s: %s." % ({"upgrade": "a thing fitted to the ship", "charter": "a charter of toll freedom", "plot": "a plot of ground",
                                           "factor": "a factor, the house's man in a far port", "partner": "a sworn partner", "pilot": "the knowledge of a water",
                                           "work": "a work done or granted", "title": "a standing among men", "relic": "a relic", "heirloom": "an heirloom",
                                           "kin": "kin won", "tongue": "a tongue learned", "letters": "letters learned", "memorial": "a memorial"}.get(kind, "a thing the house can win (%s)" % kind),
                                          s(u.get("name")) or u["id"]),
                     "What it is: %s" % dot(u.get("what_it_is")) if u.get("what_it_is") else "",
                     "How it is won: %s" % dot(clip(how.get("plain"), 260)) if how.get("plain") else "",
                     "Where: %s." % ", ".join(port_name(x) for x in lst(how.get("where"))) if how.get("where") else "",
                     "Years: %s." % years(u.get("from"), u.get("to")) if years(u.get("from"), u.get("to")) else "",
                     "Source: %s" % dot(clip(u.get("source"), 200)) if u.get("source") else ""]
            ident = re.sub(r"^unlock_(?:%s)_" % re.escape(kind), "", u["id"]) if kind else u["id"]
            if set(lst(how.get("where"))) & set(FIRST_PORTS) and int(artdraw.num(u.get("from"), 0)) <= 805:
                FIRST_IDS.add(safe_id(("upgrade." + ident) if kind == "upgrade" else ("unlock." + re.sub(r"^unlock_", "", u["id"]))))
            fallback = {"charter": "letter", "plot": "plot", "factor": "store", "partner": "ring", "pilot": "pole", "work": "house", "title": "circlet",
                        "relic": "casket", "heirloom": "chest", "kin": "person", "tongue": "speech", "letters": "quill", "memorial": "mound"}.get(kind)
            main, _ = artdraw.sign_for(ident, s(u.get("name")), "")
            ident2 = ident if (main and kind in ("upgrade", "work", "heirloom", "relic", "memorial")) else (fallback or ident)
            if kind == "upgrade":
                icon_slot("upgrade." + ident, "upgrade", s(u.get("name")) or u["id"], parts, rel(path), ident2, s(u.get("name")), s(u.get("what_it_is")), tag=tags.get(u["id"]))
            else:
                icon_slot("unlock." + re.sub(r"^unlock_", "", u["id"]), "unlock", s(u.get("name")) or u["id"], parts, rel(path), ident2, s(u.get("name")), "",
                          tag=tags.get(u["id"]))
        # the catalogue of works
        if "work" in fn:
            for wk in lst(d.get("works")):
                if isinstance(wk, dict) and wk.get("id"):
                    work_slot(wk, rel(path))
    # acts the briefs name that no table describes yet, and acts the places use
    named = set()
    m = re.search(r"where the id is one of:\s*(.+?)\n\s*\(", W.brief2, re.S)
    if m:
        named |= set(re.findall(r"[a-z_]+", m.group(1)))
    for pid, (entry, path) in W.places.items():
        for st in lst(entry.get("sites")):
            for d_ in lst(dct(st).get("do")):
                if s(d_).startswith("act:"):
                    named.add(s(d_)[4:])
    for a in sorted(named - acts_seen):
        if a in ("and", "or", "the", "id"):
            continue
        icon_slot("act." + a, "act", a.replace("_", " "), ["Icon of the act '%s': the button a site shows for it." % a.replace("_", " "),
                                                         "No table says yet what this act is (Data/story/town_actions_793.json has no entry): only its name is known."],
                  "Design/content/BRIEF_2.md", a, a, "")


# ------------------------------------------------------------------------------------------------ texts and riddles
def collect_leaves():
    tpath = os.path.join(DATA, "texts_793.json")
    texts = {}
    for t in lst((jload(tpath) or {}).get("texts")):
        if isinstance(t, dict) and t.get("id"):
            texts[t["id"]] = t
    form_word = {"rune_stick": "a stick cut with runes", "parchment": "a single leaf of parchment", "book": "a bound book", "wax_tablet": "a pair of wax tablets"}
    for tid, t in texts.items():
        sid = "text." + tid
        form = s(t.get("form"))
        parts = ["'%s' as a thing lying in the chest: %s." % (s(t.get("title")), form_word.get(form, form or "a leaf")),
                 "Language: %s; people: %s; script in the data: %s." % (s(t.get("language")), s(t.get("people")), s(t.get("script"))),
                 "Written down: %s." % clip(t.get("writtenDown"), 160) if t.get("writtenDown") else "",
                 "Told later than the game's years: the thing in the chest is the game's own copy." if t.get("toldLater") else "",
                 "Too small for real words: show the form (the stick, the leaf, the book, the tablets), a few strokes for the writing, and one thing that tells it from the others of its set.",
                 never("no paper"),
                 "The placeholder: the form is from 'form'; the number is the text's own; the cord's colour is the people.",
                 "Light and season: none. Clear background.", size_line("leaf"), file_line("leaf", sid)]
        slot(sid, "leaf", s(t.get("title")) or tid, " ".join(x for x in parts if x), rel(tpath), draw=lambda t=t: artdraw.draw_leaf(t, *SIZES["leaf"][:2]))
    rpath = os.path.join(DATA, "riddles_793.json")
    rd = jload(rpath) or {}
    for r_ in lst(rd.get("riddles")):
        if not isinstance(r_, dict) or not r_.get("id"):
            continue
        t = texts.get(s(r_.get("text"))) or {"id": r_["id"], "title": r_["id"], "form": "rune_stick"}
        sid = "riddle." + r_["id"]
        acq = dct(r_.get("acquire"))
        wh = ", ".join(port_name(dct(x).get("port")) for x in lst(acq.get("where")) if dct(x).get("port"))
        parts = ["The riddle %s ('%s') as a thing in the chest, unsolved: %s." % (r_["id"], s(t.get("title")), form_word.get(s(t.get("form")), "a stick or a leaf")),
                 "It must NOT show the answer or anything that gives it away.",
                 "How hard: %s of 5." % s(r_.get("difficulty")) if r_.get("difficulty") else "",
                 "How it is had: %s%s%s." % (s(acq.get("how")) or "not said", (" from " + s(acq.get("who"))) if acq.get("who") else "", (" at " + wh) if wh else ""),
                 "It is the same thing as the text %s with a mark that it is a riddle still to be answered." % s(r_.get("text")) if r_.get("text") else "",
                 "The placeholder: the question sign, and one gold pip for each step of hardness.",
                 "Light and season: none. Clear background.", size_line("leaf"), file_line("leaf", sid)]
        slot(sid, "leaf", "Riddle: %s" % s(t.get("title") or r_["id"]), " ".join(x for x in parts if x), rel(rpath),
             draw=lambda t=t, r_=r_: artdraw.draw_leaf(dict(t, id=r_["id"]), *SIZES["leaf"][:2], riddle=True, difficulty=artdraw.num(r_.get("difficulty"), 0)))
    for v in lst(rd.get("verses")):
        if not isinstance(v, dict) or not v.get("id"):
            continue
        sid = "verse." + v["id"]
        t = texts.get(s(v.get("text"))) or {"id": v["id"], "title": v["id"], "form": "rune_stick", "people": "norse"}
        acq = dct(v.get("acquire"))
        parts = ["The skald's verse %s as a thing in the chest, its hidden names still unread." % v["id"],
                 "What is asked of the player: %s" % dot(v.get("hint")) if v.get("hint") else "",
                 "It must NOT show the answer.",
                 "How it is had: %s%s." % (s(acq.get("how")) or "not said", (" from " + s(acq.get("who"))) if acq.get("who") else ""),
                 "The placeholder: the form of its text, with the lyre for a verse.",
                 "Light and season: none. Clear background.", size_line("leaf"), file_line("leaf", sid)]
        slot(sid, "leaf", "Verse: %s" % v["id"], " ".join(x for x in parts if x), rel(rpath),
             draw=lambda t=t, v=v: artdraw.draw_leaf(dict(t, id=v["id"]), *SIZES["leaf"][:2], verse=True))


# ------------------------------------------------------------------------------------------------ plates
def find_lists(d, *words):
    """Lists of things under a key whose name holds one of the words (ages, starts, endings), wherever they lie."""
    out = []
    for k, v in dct(d).items():
        if any(w in k.lower() for w in words):
            if isinstance(v, list):
                out += [x for x in v if isinstance(x, dict)]
            elif isinstance(v, dict):
                out += [dict(x, id=x.get("id") or kk) for kk, x in v.items() if isinstance(x, dict)]
    return out


def collect_plates():
    tr_path = os.path.join(DATA, "trade_rules.json")
    ch_path = os.path.join(DATA, "chronicle_793.json")
    tr, ch = jload(tr_path) or {}, jload(ch_path) or {}
    start = s(ch.get("start")) or "793-06-21"
    span = lst(ch.get("span")) or [793, 1066]
    gis = dct(W.ship_classes.get("gislinge", ({}, ""))[0])
    dor = W.ports.get("dorestad") or {}
    debt = dct(tr.get("start")).get("debt")
    # the title
    parts = ["The title plate: the game's name, Kapman, over the North Sea world of %s to %s." % (span[0], span[-1]),
             "Nothing else is given in the data: a merchant's boat and the sea are enough. The words are set by the game; leave room for the name across the top third.",
             never("helmets", "compass", "sea charts"), "Light and season: yours to choose.", size_line("plate"), file_line("plate", "plate.title")]
    slot("plate.title", "plate", "The title", " ".join(x for x in parts if x), rel(ch_path),
         draw=lambda: artscene.draw_plate("plate.title", "KAPMAN", "%s - %s" % (span[0], span[-1]), "sea", [(1, "a merchant boat under sail")], None, *SIZES["plate"][:2]))
    # the start
    parts = ["The opening plate: %s, the day the game begins (chronicle_793.json 'start')." % start,
             "Where: %s. %s" % (port_name("dorestad"), dot(dor.get("note"))) if dor else "",
             "The boat: %s. %s" % (s(gis.get("name")), dot(gis.get("look"))) if gis else "",
             "The house owes %s pennies for her (trade_rules.json 'start')." % s(debt) if debt else "",
             "Season: midsummer by the date; the light is not given.",
             never("helmets", "chimneys", "stave", "compass", "sea charts"), size_line("plate"), file_line("plate", "plate.start.793")]
    slot("plate.start.793", "plate", "The start: %s" % start, " ".join(x for x in parts if x), ", ".join((rel(ch_path), rel(tr_path))),
         draw=lambda: artscene.draw_plate("plate.start.793", "THE START", start, "harbour", [(1, "a merchant boat at the landing at dawn, casks of wine")], dor, *SIZES["plate"][:2]))
    # the end
    last = [e for e in lst(ch.get("entries")) if isinstance(e, dict) and s(e.get("date")).startswith(str(span[-1]))]
    parts = ["The closing plate: %s, the last year of the game (chronicle_793.json 'span')." % span[-1],
             "What the chronicle has for that year: %s." % "; ".join(clip(e.get("title"), 110) for e in last[:6]) if last else "",
             "How the house stands at the end is the player's doing and is not in the data: draw the year, not one fate.",
             never("helmets", "chimneys", "compass", "sea charts"), "Light and season: not given.", size_line("plate"), file_line("plate", "plate.end.%s" % span[-1])]
    slot("plate.end.%s" % span[-1], "plate", "The end: %s" % span[-1], " ".join(x for x in parts if x), rel(ch_path),
         draw=lambda: artscene.draw_plate("plate.end.%s" % span[-1], "THE END", str(span[-1]), "sea", [(1, " ".join(s(e.get("title")) for e in last[:6]) + " fleet dusk")], None, *SIZES["plate"][:2]))
    # the ages, the starts and the endings, wherever the writers put them
    sources = [(W.spine, W.spine_path)] + [(d, p) for fn, (d, p) in W.story_files.items()]
    for d, path in sources:
        fn = os.path.basename(path).lower()
        for a in find_lists(d, "ages") + ([x for x in lst(d.get("eras")) if isinstance(x, dict)] if "age" in fn else []):
            if not a.get("id"):
                continue
            sid = "plate.age." + s(a["id"])
            name = s(a.get("name") or a.get("title")) or s(a["id"])
            ports = dct(a.get("ports"))
            parts = ["The plate of the age '%s', %s: shown when the age begins." % (name, years(a.get("from"), a.get("to"))),
                     "Picture: %s" % dot(a.get("picture")) if a.get("picture") else "",
                     "The world: %s" % dot(a.get("world")) if a.get("world") else "",
                     "The trade: %s" % dot(clip(a.get("trade"), 420)) if a.get("trade") else "",
                     "The danger: %s" % dot(clip(a.get("danger"), 320)) if a.get("danger") else "",
                     "New in these years: %s." % "; ".join("%s %s" % (s(n.get("year")), clip(n.get("what"), 90)) for n in lst(a.get("new"))[:6] if isinstance(n, dict)) if a.get("new") else "",
                     "Gone: %s." % "; ".join("%s %s" % (s(n.get("year")), clip(n.get("what"), 90)) for n in lst(a.get("gone"))[:4] if isinstance(n, dict)) if a.get("gone") else "",
                     "Ports that rise: %s." % ", ".join(port_name(x) for x in lst(ports.get("rise"))) if ports.get("rise") else "",
                     "Ports that fall: %s." % ", ".join(port_name(x) for x in lst(ports.get("fall"))) if ports.get("fall") else "",
                     "One picture for the whole age: what a merchant of those years would know it by.",
                     never("helmets", "chimneys", "stave", "compass", "sea charts", "clocks"),
                     "Light and season: not given.", size_line("plate"), file_line("plate", sid)]
            texts = [(3, name), (2, s(a.get("picture"))), (1, s(a.get("world"))), (0.5, s(a.get("danger")))]
            stage = "sea" if re.search(r"raid|army|fleet|tribute", (name + " " + s(a.get("danger"))[:200]).lower()) else "harbour"
            slot(sid, "plate", "%s (%s)" % (name, years(a.get("from"), a.get("to"))), " ".join(x for x in parts if x), rel(path),
                 draw=lambda sid=sid, name=name, a=a, texts=texts, stage=stage: artscene.draw_plate(sid, name, years(a.get("from"), a.get("to")), stage, texts, dor, *SIZES["plate"][:2]))
            for story in lst(a.get("stories")):
                if isinstance(story, dict) and story.get("id"):
                    story_slot(story, rel(path), None, a)
        for word, kindname, sidp in (("starts", "A way the game can start", "plate.start."), ("endings", "A way the game can end", "plate.ending.")):
            for a in find_lists(d, word):
                if not a.get("id") or "chain" in fn:
                    continue
                sid = sidp + s(a["id"])
                name = s(a.get("name") or a.get("title")) or s(a["id"])
                parts = ["%s: '%s'." % (kindname, name),
                         "Picture: %s" % dot(a.get("picture")) if a.get("picture") else "",
                         dot(clip(a.get("text") or a.get("summary") or a.get("what") or a.get("how"), 700)),
                         "Year: %s." % s(a.get("year") or a.get("from")) if (a.get("year") or a.get("from")) else "",
                         never("helmets", "chimneys", "stave", "compass", "sea charts", "clocks"),
                         "Light and season: only what the lines above say.", size_line("plate"), file_line("plate", sid)]
                texts = [(3, name), (2, s(a.get("picture"))), (1, s(a.get("text") or a.get("summary")))]
                slot(sid, "plate", name, " ".join(x for x in parts if x), rel(path),
                     draw=lambda sid=sid, name=name, texts=texts, a=a: artscene.draw_plate(sid, name, s(a.get("year") or ""), "harbour", texts, None, *SIZES["plate"][:2]))
    if not any(sl["id"].startswith("plate.age.") for sl in SLOTS):
        SKIPPED.append("the seven ages: no list of ages found (looked in Data/story/*.json and the spine)")


# ------------------------------------------------------------------------------------------------ the interface
def parse_ui_direction(text):
    """Pieces named with a size in Design/ui_direction.md: table rows and list lines like 'Close mark, 9 x 9: ...'."""
    out = []
    header = None
    for line in text.splitlines():
        m = re.search(r"(\d{1,4})\s*(?:x|×|by)\s*(\d{1,4})(?:\s*(?:px|pixels))?", line)
        if line.strip().startswith("|"):
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if set("".join(cells)) <= set("-: "):
                continue
            if header is None:
                header = cells
                continue
            if m and cells:
                out.append((clean_md(cells[0]), int(m.group(1)), int(m.group(2)), clean_md(" | ".join(cells[1:]))))
        else:
            header = None if not line.strip() else header
            lm = re.match(r"\s*(?:[-*]|\d+\.)\s+(.*)", line)
            if lm and m:
                body = lm.group(1)
                name = clean_md(re.split(r"[:(,]|\d+\s*(?:x|×|by)\s*\d+", body)[0])
                if name:
                    out.append((name, int(m.group(1)), int(m.group(2)), clean_md(body)))
    return [o for o in out if 1 <= o[1] <= 1024 and 1 <= o[2] <= 1024]


# the ink-slip look's own frames and buttons: the Command frame (kind hud, artframe.py) has taken their place
SUPERSEDED = {"frame_paper", "frame_ink", "button", "button_on", "button_hover", "close", "face_frame",
              # 7 Oct, later: the clock's marks and the marks of how a price is known are pieces of the frame now (hud.clock.*, hud.word.*)
              "clock_pause", "clock_run", "clock_fast", "word_seen", "word_heard", "word_none"}
# what of the older list the Command frame and its chart still show: id -> (where it is seen now, what must read)
UI_NOW = {
    "cursor": ("Everywhere: it takes the place of the system pointer.",
               "A pointer. Its tip is the top left pixel: that pixel is where it clicks. A light body with a dark edge of 1 px, so it reads "
               "on the chart, on a window and on a picture."),
    "cursor_hand": ("Over anything that can be pressed: a port, a sail, a line of a list, a button, a linked word.",
                    "A hand, or a second pointer shape; the tip of the finger is where it clicks. The same weight and colours as ui.cursor, "
                    "so the change is felt and does not startle."),
    "her_ring": ("On the chart, round your own boat, so she is told from the other sails.",
                 "A ring, clear inside: her own mark (ship_mark, 32 x 16) shows through. Two tones, a light line with a dark one beside "
                 "it, so it reads on dark sea and on pale shallows."),
    "port_selected": ("On the chart, round the port you are bound for or have clicked.",
                      "Four corners that stand round a 16 x 16 port mark, clear in the middle and along the sides. Two tones, as every "
                      "mark on the chart."),
    "course_dash": ("On the chart: the course laid for your boat.",
                    "One dash and its gap, 5 px on and 3 px clear; the game lays it end to end and turns it. It must not be taken for the "
                    "measure (hud.measure_dash, gold): draw the course warm white over night."),
    "wind_streak": ("Scattered over the sea on the chart when the chart tool Winds is on, turned to the wind.",
                    "A thin pale line with a small head, pointing right; the game turns it. Faint: it lies over the sea and must not "
                    "fight the marks."),
    "sail": ("On the chart, where a sail has been made out and her kind is not yet known; shown when the chart tool Sails is on.",
             "A small pale sail with a dark edge, far off, no hull to speak of. A ship whose kind is known has her own mark (ship_mark)."),
    "note": ("On the chart where you set a pin of your own; shown when the chart tool Pins is on.",
             "A small pin or leaf, light with a dark edge. Smaller in feel than a port's mark, so it is never taken for a port."),
    "crew_head": ("In the portrait block top right, under your name: one for each man of the crew, his first letter set on it by the game.",
                  "A small disc with a dark edge and a flat middle: a ground for one letter."),
}
UI_OWN_SIZE = {"sail"}                           # their size is their own: a row of ui_direction.md with a like name is another thing
INK_NOTE = ("Of the ink-slip look of 5 October. The Command frame took its place on 7 October and the game does not show this piece now: "
            "leave it until it is asked for.")
CHART_LOOK = "The letters are the game's, never drawn into a piece."


def collect_ui():
    dpath = os.path.join(DESIGN, "ui_direction.md")
    mpath = os.path.join(DESIGN, "ui_mock", "template2.html")
    dtext, mtext = tload(dpath), tload(mpath)
    named = parse_ui_direction(dtext) if dtext else []
    used = set()
    if not dtext and not mtext:
        SKIPPED.append("the interface: neither Design/ui_direction.md nor Design/ui_mock/template2.html is there")
        return
    for piece in artscene.UI_PIECES:
        pid, name, w, h, find, fn, where, what = piece
        if pid in SUPERSEDED:
            continue
        in_mock = (find in mtext) if mtext else False
        hit = None
        for i, (nm, nw, nh, body) in enumerate(named):
            key = pid.replace("_", " ")
            if key in nm.lower() or nm.lower() in name.lower() or all(part in nm.lower() for part in key.split()):
                hit = i
                break
        if dtext and hit is None and not in_mock and not re.search(re.escape(pid.split("_")[0]), dtext.lower()):
            continue                                         # the direction does not have it and the mock-up does not either
        if not dtext and not in_mock:
            continue
        note = ""
        if hit is not None:
            used.add(hit)
            if pid not in UI_OWN_SIZE:
                nm, nw, nh, body = named[hit]
                w, h = nw, nh
                note = "As Design/ui_direction.md has it: %s " % dot(body)
        src = rel(dpath) if hit is not None else (rel(mpath) if in_mock else rel(dpath))
        p2 = (pid, name, w, h, find, fn, where, what)
        if pid in UI_NOW:
            where = UI_NOW[pid][0]
            parts = ["%s. %s" % (name, UI_NOW[pid][1]), CHART_LOOK,
                     "Size: %d x %d pixels." % (w, h), file_line("ui", "ui." + pid)]
        else:
            parts = ["%s. %s" % (name, what), note,
                     "In the mock-up this is '%s' (Design/ui_mock/template2.html)." % find if in_mock else "", INK_NOTE,
                     "Size: %d x %d pixels." % (w, h), file_line("ui", "ui." + pid)]
        slot("ui." + pid, "ui", name, " ".join(x for x in parts if x), src, size=(w, h), where=where,
             draw=lambda p2=p2: artscene.draw_ui(p2))
    for i, (nm, nw, nh, body) in enumerate(named):
        if i in used:
            continue
        pid = safe_id(nm.lower().replace(" ", "_"))[:40]
        parts = ["%s. %s" % (nm, dot(body)), "Named in Design/ui_direction.md; no placeholder rule knows it, so the placeholder is a plain box with its name.",
                 INK_NOTE, "Size: %d x %d pixels." % (nw, nh), file_line("ui", "ui." + pid)]

        def box(nm=nm, nw=nw, nh=nh):
            cv = Cv(nw, nh)
            cv.rect(0, 0, nw - 1, nh - 1, artkit.BASALT); cv.box(0, 0, nw - 1, nh - 1, artkit.BYZ)
            cv.text_fit(2, max(1, nh // 2 - 2), nm, artkit.PGOLD, nw - 4)
            artkit.placeholder_mark(cv)
            return cv
        slot("ui." + pid, "ui", nm, " ".join(parts), rel(dpath), size=(nw, nh), where="See Design/ui_direction.md.", draw=box)


HUD_ASSIGN = {}


def collect_hud():
    """The Command frame's own pieces and the ship window's views (artframe.PIECES)."""
    import artframe
    src = rel(os.path.join(DESIGN, "ui_mock", "template3.html"))
    for piece in artframe.PIECES:
        pid, group, name, w, h, corner, fn, where, what = piece
        kind = artframe.kind_of(piece)
        sid = kind + "." + pid
        desc = " ".join([artframe.describe(piece), "Size: %d x %d pixels." % (w, h), file_line(kind, sid)])
        sl = slot(sid, kind, name, desc, src, size=(w, h), where=where, draw=lambda piece=piece: artframe.draw(piece))
        if sl:
            HUD_ASSIGN[sl["id"]] = {"file": sl["file"], "group": group, "name": name, "w": w, "h": h,
                                    "slice": [corner] * 4 if corner else None, "where": where,
                                    "today": artframe.USS.get(pid, "")}


def collect_weight():
    """The people of weight (story/weight_793.json) who have no portrait yet, and the player's own face for each people."""
    wpath = os.path.join(DATA, "story", "weight_793.json")
    have = set(sl["id"] for sl in SLOTS if sl["kind"] == "portrait")
    for p in lst(dct(jload(wpath)).get("people")):
        if not isinstance(p, dict) or not p.get("id"):
            continue
        if (p.get("cast") and "person." + s(p["cast"]) in have) or (p.get("place_id") and "person." + s(p["place_id"]) in have):
            continue
        q = dict(p, role=p.get("role") or "", trait=s(p.get("traits")))
        if not q.get("people") and p.get("seat"):
            q["people"] = dct(W.ports.get(p["seat"])).get("culture") or ""
        if not q.get("sex") and re.search(r"\b(queen|abbess|wife|widow|daughter|lady|nun|mother|sister)\b", s(p.get("office")).lower()):
            q["sex"] = "f"
        temper = dct(p.get("temper"))
        extra = "Office: %s" % dot(p.get("office"))
        if temper.get("start"):
            extra += " Temper when the game starts: %s%s. This is the first face; the same person in another temper may be drawn later." % (
                s(temper.get("start")), (" (%s)" % clip(temper.get("why"), 160)) if temper.get("why") else "")
        if p.get("source"):
            extra += " Source: %s" % dot(clip(p.get("source"), 200))
        portrait_slot("weight." + s(p["id"]).replace("wt.", ""), q, rel(wpath), extra)
    spath = os.path.join(DATA, "story", "self_793.json")
    for pe in lst(dct(jload(spath)).get("peoples")):
        if not isinstance(pe, dict) or not pe.get("id"):
            continue
        people = s(pe["id"])
        for sex, word in (("m", "man"), ("f", "woman")):
            pp = {"id": "self.%s.%s" % (people, word), "name": "You", "role": "", "people": people, "sex": sex, "age": 31, "faith": "", "looks": ""}
            parts = ["Your own portrait when you play a %s %s: head and shoulders, facing us." % (s(pe.get("name")), word),
                     "A merchant with one small boat and a debt, about thirty in 793: weathered, not rich. The player sees this face more than any other.",
                     "No faith shows in it (the faith is chosen apart from the people), and no trade but the sea.",
                     ("Home: %s." % port_name(pe.get("home"))) if pe.get("home") else ""]
            dress = people_note(W.dress, people)
            if dress:
                parts.append("Dress of the %s: %s" % (people, dot(dress)))
            parts += [never("helmets", "buttons"), "Light and season: a plain even light, no scene behind.",
                      "The placeholder: ground colour by people.", size_line("portrait"), file_line("portrait", pp["id"])]
            slot(pp["id"], "portrait", "You: %s %s" % (s(pe.get("name")), word), " ".join(x for x in parts if x), rel(spath),
                 where="The portrait block top right, whenever nobody else is speaking.",
                 draw=lambda pp=pp: artdraw.draw_portrait(pp, *SIZES["portrait"][:2]))


def write_templates(folder):
    """An empty picture of the right size for each kind, the same with guide lines, and where the frame's pieces are cut in nine."""
    import artframe
    os.makedirs(os.path.join(folder, "cut_in_nine"), exist_ok=True)
    flat = [0, 0, 0]
    for name in artkit.ORDER:
        flat += list(artkit.RGB[name][:3])
    lines = ["TEMPLATES", "",
             "<kind>_<size>.png        an empty picture of the right size with the game's 40 colours already loaded (colour 0 is clear).",
             "                         Open it, draw, and save it under the slot's own name in the slot's own folder.",
             "<kind>_<size>_guide.png  the same size with guide lines. Lay it under your drawing as a layer; never save it over a slot.",
             "sheets/                  each family of the frame side by side, eight times its size, with the file names: to look at while you draw.",
             "cut_in_nine/             each piece of the frame that is cut in nine, eight times its size, with the cuts in gold.",
             "                         The four corners stay as you draw them; the game stretches the sides and fills the middle.", "",
             "THE GUIDE LINES"]
    for k in KIND_ORDER:
        w, h = SIZES[k][:2]
        if not w or w > 700:
            continue
        name = "%s_%dx%d" % (k, w, h)
        blank = os.path.join(folder, name + ".png")
        if not os.path.exists(blank):
            try:
                im = Image.new("P", (w, h), 0)
                im.putpalette(flat + [0] * (768 - len(flat)))
                im.save(blank, transparency=0)
            except Exception:
                Image.new("RGBA", (w, h), (0, 0, 0, 0)).save(blank)
        artframe.kind_guide(k, w, h).im.save(os.path.join(folder, name + "_guide.png"))
        lines.append("  %-22s %s" % (name, artframe.GUIDE_WORDS.get(k, artframe.GUIDE_WORDS["scene"] if w >= 160 else "plum box: keep the thing inside it; gold dot: the middle")))
    n = 0
    for piece in artframe.PIECES:
        if piece[5]:
            artframe.slice_guide(piece).im.save(os.path.join(folder, "cut_in_nine", "hud.%s_x8.png" % piece[0]))
            n += 1
    lines += ["", "PIECES CUT IN NINE: %d, each with its corner size in its description (index.html, the page of the frame)." % n]
    with io.open(os.path.join(folder, "READ ME.txt"), "w", encoding="utf-8-sig", newline="") as f:
        f.write("\r\n".join(lines) + "\r\n")


SHEET_GROUPS = ["frames", "buttons", "marks", "function icons", "panel icons", "stat icons", "chart tool icons", "wiki icons"]


def write_sheets(folder, desk, k=8):
    """One picture a family of the frame: every piece at 8 times its size on a neutral ground with its file name under it, to have
    open beside the piece being drawn. It shows what lies in the Desktop folder now (yours where you have drawn it, else the
    placeholder). To look at only: each piece is still saved by hand under its own name."""
    import artframe
    from PIL import ImageDraw, ImageFont
    os.makedirs(folder, exist_ok=True)
    font = None
    for fname in ("consola.ttf", "cour.ttf", "arial.ttf"):
        try:
            font = ImageFont.truetype(os.path.join(os.environ.get("WINDIR", "C:/Windows"), "Fonts", fname), 13)
            break
        except Exception:
            continue
    font = font or ImageFont.load_default()
    ground, cell_ground, ink, soft = (52, 54, 58, 255), (84, 88, 92, 255), (240, 240, 236, 255), (196, 198, 202, 255)
    check = (96, 100, 104, 255)                              # with cell_ground: two greys that are none of the 40 colours
    by_id = dict((sl["id"], sl) for sl in SLOTS)
    made = []
    for group in SHEET_GROUPS:
        pieces = [p for p in artframe.PIECES if p[1] == group and ("hud." + p[0]) in by_id]
        if not pieces:
            continue
        cells = []
        for p in pieces:
            sl = by_id["hud." + p[0]]
            im = None
            path = os.path.join(desk, sl["file"])
            if os.path.exists(path):
                try:
                    im = Image.open(path).convert("RGBA")
                    if im.size != (sl["w"], sl["h"]):
                        im = None                                # a wrong size would mislead: show the placeholder
                except Exception:
                    im = None
            if im is None:
                im = artframe.draw(p).im
            label = ["%s.png" % sl["id"], "%d x %d%s" % (sl["w"], sl["h"], ("  cut %d" % p[5]) if p[5] else "")]
            tw = max(int(font.getlength(t)) for t in label)
            big = Image.new("RGBA", (im.size[0] * k, im.size[1] * k), cell_ground)
            bd = ImageDraw.Draw(big)
            for yy in range(im.size[1]):
                for xx in range(yy % 2, im.size[0], 2):
                    bd.rectangle([xx * k, yy * k, xx * k + k - 1, yy * k + k - 1], fill=check)
            big.alpha_composite(im.resize((im.size[0] * k, im.size[1] * k), Image.NEAREST))
            cells.append((big, label, max(im.size[0] * k, tw) + 16, im.size[1] * k))
        width = 1500
        rows, row, x = [], [], 12
        for c in cells:
            if row and x + c[2] > width:
                rows.append(row); row, x = [], 12
            row.append(c); x += c[2] + 12
        if row:
            rows.append(row)
        height = 40 + sum(max(c[3] for c in r) + 16 + 40 + 14 for r in rows)
        sheet = Image.new("RGBA", (width, height), ground)
        d = ImageDraw.Draw(sheet)
        d.text((12, 10), "%s: %d pieces, each %d times its size. hud/<name>. To look at; save each piece by hand under its own name." % (group, len(cells), k), fill=ink, font=font)
        y = 40
        for r in rows:
            rh = max(c[3] for c in r)
            x = 12
            for im, label, cw, ch in r:
                sheet.alpha_composite(im, (x + (cw - im.size[0]) // 2, y + 8 + (rh - ch) // 2))
                d.text((x, y + rh + 20), label[0], fill=ink, font=font)
                d.text((x, y + rh + 36), label[1], fill=soft, font=font)
                x += cw + 12
            y += rh + 16 + 40 + 14
        name = group.replace(" ", "_") + ".png"
        sheet.convert("RGB").save(os.path.join(folder, name))
        made.append("%s (%d)" % (name, len(cells)))
    with io.open(os.path.join(folder, "READ ME.txt"), "w", encoding="utf-8-sig", newline="") as f:
        f.write("\r\n".join([
            "SHEETS: the families of the frame, side by side",
            "",
            "One picture a family: " + ", ".join(made) + ".",
            "Every piece is shown at %d times its size on a checked grey ground, with its file name and its size under it" % k,
            "('cut 8' means it is cut in nine with corners of 8 pixels). One square of the ground is one pixel of the piece;",
            "where the squares show through, the piece is clear.",
            "",
            "They are to look at while you draw, so that a whole family is of one hand: the same edge, the same weight of line,",
            "the same colours. They show what lies in the hud folder now: your own piece where you have drawn it, else the placeholder.",
            "Run make_art_list.py again and they are made anew.",
            "",
            "Do not draw on a sheet. Each piece is its own small file in hud/, at its own size, saved by hand under its own name."]) + "\r\n")


# ------------------------------------------------------------------------------------------------ what to draw first
# The user, 7 Oct 2026, on seeing the count: "I've gotta draw 4720 slots??" No. The list is every picture the whole game
# could show, 793 to 1066. The first release is one life, 793 to 805, on twelve ports (Design/GAME_MAKER.md stage 2; the
# same twelve as Bake/content/card_census.py), and most of that can stand on a few shared pictures.
FIRST_PORTS = ["dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic", "ripa"]
FIRST_IDS = set()
FIRST_HOUR = set()
FIRST_PEOPLES = ["frisian", "frankish", "english", "danish"]     # story/self_793.json, 'about': the four peoples that can start on the twelve ports
TIERS = [
    (1, "The frame", "The interface itself, the cursor and the marks on the chart. Every screen shows them, so they are what makes the game look like yours. Most pieces are 16 pixels or smaller."),
    (2, "The first hour", "The title, your own face, your first shipmate, your own boat, what lies in your hold on the first morning, and the twelve ports of the first release from the water and on the chart."),
    (3, "Stand-ins that cover many", "One picture the game shows for every person of a role, every card of a stage and every site of a kind that has none of its own. With these drawn, nothing in the first release is bare."),
    (4, "The first release, one by one", "The named people, sites, stories, works, goods, ships, town acts and ship's gear of the twelve ports, 793 to 805, and your own face for the other three peoples that can start. Draw the ones you care about, in any order."),
    (5, "Later", "Everything else: the other ports, the later years, a picture for each card, the texts in the chest, the views behind the ship (a live 3D view is to take their place) and the pieces of the older ink-slip interface. None of it is needed for the first release."),
]


def first_regions():
    return set(s(dct(W.ports.get(pid)).get("region")) for pid in FIRST_PORTS) - {""}


def home_region():
    return s(dct(W.ports.get(FIRST_PORTS[0])).get("region"))


_hold = []


def first_hold():
    """What a new game starts with aboard, read from the game's own code (Code/Game/Chart/ChartWorld.cs)."""
    if not _hold:
        text = tload(os.path.join(ROOT, "Assets", "Kapman", "Code", "Game", "Chart", "ChartWorld.cs")) or ""
        m = re.search(r"foreach \(var \(id, n\) in new\[\] \{([^}]*)\}", text)
        _hold.extend(re.findall(r'\("([a-z_]+)",\s*\d+\)', m.group(1)) if m else [])
        if not _hold:
            SKIPPED.append("the first hold: ChartWorld.cs no longer says what a new game starts with; the four goods of 7 October are used")
            _hold.extend(["rhenish_wine", "quernstones", "ships_bread", "ale"])
    return _hold


def tier_of(sl):
    k, i = sl["kind"], sl["id"]
    low = (i + " " + s(sl["source"])).lower()
    here = any(("." + pid + ".") in low or ("/" + pid + ".json") in low or i.lower().endswith("." + pid) for pid in FIRST_PORTS)
    if k in ("hud", "title"):
        return 1
    if k == "ui":                                            # the cursor and the chart's marks; the rest is the ink-slip look, not shown now
        return 1 if i[3:] in UI_NOW else 5
    if k == "shipview":                                      # a stand-in until the live 3D view of the ship is built (DECISIONS, 7 Oct, ruling 11)
        return 5
    if (i.startswith("self.%s." % FIRST_PEOPLES[0]) or i in FIRST_HOUR or i in ("plate.title", "plate.start.793")
            or (k in ("ship_side", "ship_mark") and "gislinge" in i) or (k in ("harbour", "port_view", "port_mark") and here)):
        return 2
    if i.startswith("role.") or ".any." in i:
        return 3
    if i in FIRST_IDS or any(i.startswith("self.%s." % p) for p in FIRST_PEOPLES[1:]):
        return 4
    if here and k in ("portrait", "scene", "story", "work", "ship_offer"):
        m = re.search(r"(?:In the game|It stands|Offered|Possible)\s+(?:from\s+)?(\d{3,4})|Year: (\d{3,4})", sl["desc"])
        year = int(m.group(1) or m.group(2)) if m else 0
        return 4 if year <= 805 else 5
    return 5


# what an artist needs before the data: what the picture is, what must read at its size, what to leave out
LEADS = {
    "portrait": "In short: one person, head and shoulders, 64 x 80 px. Must read: who this is from the outline of head, hair or headgear and "
                "beard, from where the eyes look, and from one thing of the trade or rank at the shoulder; the head is about 30 px wide. "
                "Leave out: a scene behind (one flat colour, or two that fall from light to dark), cloth patterns finer than 2 px, teeth, "
                "lashes, anything under 2 px.",
    "scene": "In short: one place in a port, 320 x 180 px. Must read: what kind of place it is, from its biggest shape, at the first "
             "glance; and where a person would stand to do what is done there. A standing figure is about 12 to 16 px tall here. "
             "Leave out: named people, faces, writing, anything of a later century.",
    "harbour": "In short: the port as a skipper first sees it from the water, a wide strip of 640 x 180 px. Must read: the line of the shore "
               "and the kind of landing, how big the place is by the count of its roofs, and the one thing only this port has (it is in "
               "the notes below). A man on the bank is about 4 to 6 px tall here. Leave out: crowds, single faces, buildings of her later years.",
    "port_mark": "In short: the port's mark on the chart, 16 x 16 px. Must read: the kind of place and its size from the outline alone, on "
                 "dark sea and on pale land: a light middle, a dark edge all round, a clear ground, two or three colours. Leave out: detail "
                 "under 2 px, letters, the name (the game writes it beside the mark).",
    "card": "In short: the picture at the top of a card, 192 x 108 px. Must read: where we are and what is at stake, at one glance, before "
            "the words are read. Leave out: more than two or three figures, faces in detail (a head is 6 to 8 px here), writing.",
    "story": "In short: the picture beside a story, 192 x 108 px. Must read: the one moment the story turns on. Leave out: more than two or "
             "three figures, faces in detail (a head is 6 to 8 px here), writing.",
    "ship_offer": "In short: a ship where she can be had, on a beach or at a yard, 192 x 108 px. Must read: her kind from her outline, and "
                  "the place from the ground she lies on. Leave out: lines thinner than 1 px, crowds.",
    "ending": "In short: the last picture of a chain of cards, 320 x 180 px, full in the middle of the screen. Must read: how it ended, "
              "without the words. Leave out: writing, anything the chain did not have.",
    "plate": "In short: the whole screen, 640 x 360 px. Must read: one large shape that holds from across a room; the game sets its words "
             "over the picture, so leave a quiet part for them. Leave out: writing, small things scattered about.",
    "good": "In short: one unit of the good as it is traded, 32 x 32 px, seen in a row of other goods. Must read: what it is from its "
            "outline, and its kind from its main colour. A clear ground and a dark edge. Leave out: letters, a table or ground under it, "
            "more than three or four colours.",
    "ship_side": "In short: the ship from the side, 160 x 96 px, at 6 px to the metre. Must read: her outline: the sheer, the stems, the "
                 "mast and the yard. A man aboard is about 10 px tall, and he gives the scale. Leave out: lines thinner than 1 px, a sky, a shore.",
    "ship_mark": "In short: the ship from straight above for the chart, 32 x 16 px, bow to the right. Must read: that she is a boat, and "
                 "which way she heads. A light middle and a dark edge, so she reads on dark sea and on pale shallows. Leave out: all but "
                 "hull, yard and sail.",
    "upgrade": "In short: an icon, 32 x 32 px, in a list. Must read: one thing, known by its outline. A clear ground. Leave out: letters, a "
               "ground, more than three or four colours.",
    "work": "In short: a small picture of a thing built, 64 x 64 px. Must read: what was built, from one building or one thing. Leave out: "
            "people in detail, writing.",
    "leaf": "In short: a text or a riddle as a thing lying in the chest, 48 x 48 px, clear ground. Must read: what kind of thing it is: a "
            "rune stick, a leaf, a book, a wax tablet. Leave out: letters that can be read.",
}
for _k in ("unlock", "act", "game"):
    LEADS[_k] = LEADS["upgrade"].replace("in a list", "on a button or in a list")


def lead_of(sl):
    lead = LEADS.get(sl["kind"], "")
    if not lead:
        return ""
    if sl["id"].startswith("role."):
        lead += " It stands for many people: no scar, no sign of one people or one faith, an age in the middle of the role's span."
    elif sl["id"].startswith("self."):
        lead += " The player looks at this face all evening: calm, eyes level, nothing that tires."
    elif ".any." in sl["id"]:
        lead += " It stands for many: mood before detail, and nothing that fits only one port or one season."
    return lead + " The notes from the data: "


def collect_standins():
    """One picture for every card of a stage and every site of a kind that has none of its own (people have theirs by role already)."""
    src = rel(os.path.join(DATA, "encounters"))
    stages = [("harbour", "at a quay or a beach, among moored boats", artkit.TIMBER), ("sea", "under way, open water all round", artkit.SEA),
              ("crew", "aboard, among your own men", artkit.PEAT), ("long", "a thing that runs over many days: a road, a season, a waiting", artkit.DPURP)]
    for stage, what, ground in stages:
        def box(stage=stage, ground=ground):
            cv = Cv(*SIZES["card"][:2])
            cv.rect(0, 0, cv.w - 1, cv.h - 1, ground)
            cv.text_mid(cv.w // 2, cv.h // 2 - 8, "ANY CARD", artkit.PGOLD, 2, artkit.NIGHT)
            cv.text_mid(cv.w // 2, cv.h // 2 + 8, stage, artkit.WWHITE, 2, artkit.NIGHT)
            artkit.label_strip(cv, "stand-in: any card, " + stage)
            artkit.placeholder_mark(cv)
            return cv
        sid = "card.any." + stage
        n_cards = sum(1 for cid in W.cards if s(cid).split(".")[0] == stage)
        slot(sid, "card", "Any card: %s" % stage,
             "Stand-in for every card of the stage '%s' that has no picture of its own: %s. %d cards of this stage are in the data. "
             "No named person and no one port in it, so it fits them all; "
             "mood before detail. %s %s" % (stage, what, n_cards, size_line("card"), file_line("card", sid)), src,
             where="At the top of any card of this stage that has no picture of its own.", draw=box)
    kinds, example = {}, {}
    for pid, (entry, path) in sorted(W.places.items()):
        for st in lst(entry.get("sites")):
            if isinstance(st, dict) and st.get("kind"):
                kinds.setdefault(s(st["kind"]), []).append(s(st.get("name")))
    for pid in FIRST_PORTS:                                  # one site of each kind as the data writes it: the twelve ports first
        for st in lst(W.places.get(pid, ({}, ""))[0].get("sites")):
            if isinstance(st, dict) and st.get("kind") and st.get("see") and int(artdraw.num(st.get("from"), 0)) <= 805:
                example.setdefault(s(st["kind"]), (s(st.get("name")), dct(W.ports.get(pid)).get("name"), s(st.get("see"))))
    done_at = {}
    for aid, (a, apath) in W.acts.items():                   # in the table's own order: the commonest acts come first there
        at = a.get("at")
        for kd in (lst(at) if isinstance(at, list) else re.findall(r"[a-z_]+", s(at))):
            done_at.setdefault(s(kd), []).append(s(a.get("name")).lower())
    for kind, names in sorted(kinds.items()):
        def box(kind=kind):
            cv = Cv(*SIZES["scene"][:2])
            cv.rect(0, 0, cv.w - 1, 107, artkit.SKY); cv.rect(0, 108, cv.w - 1, cv.h - 1, artkit.DEARTH)
            cv.text_mid(cv.w // 2, 70, "ANY", artkit.NIGHT, 3)
            cv.text_mid(cv.w // 2, 96, kind.replace("_", " "), artkit.NIGHT, 3)
            artkit.label_strip(cv, "stand-in: any " + kind.replace("_", " "))
            artkit.placeholder_mark(cv)
            return cv
        sid = "scene.any." + safe_id(kind)
        ex = example.get(kind)
        more = ""
        if KIND_COUNT.get(kind):
            more += (" In the twelve ports of the first release %d sites are of this kind." % KIND_COUNT[kind]) if KIND_COUNT[kind] != 1 else \
                " In the twelve ports of the first release one site is of this kind."
        if ex:
            more += " As one of them is written in the data (%s, %s): %s Take from it what any such place has and leave what is only its own." % (ex[0], ex[1], dot(clip(ex[2], 420)))
        if done_at.get(kind):
            more += " What is done at such a place, so the picture can leave room for it: %s." % ", ".join(list(dict.fromkeys(done_at[kind]))[:9])
        slot(sid, "scene", "Any %s" % kind.replace("_", " "),
             "Stand-in for every site of the kind '%s' that has no scene of its own (%d sites in the data, such as %s). Of no one port and no one people: "
             "the plainest true picture of such a place in the north about 800.%s %s %s %s"
             % (kind.replace("_", " "), len(names), ", ".join(sorted(set(n for n in names if n))[:3]), more, never("chimneys", "stave", "clocks"),
                size_line("scene"), file_line("scene", sid)),
             rel(os.path.join(DATA, "places")), where="The full window of a port, when a site of this kind with no scene of its own is opened.", draw=box)


def first_page(desk):
    """DRAW FIRST.html: the short list, tier by tier, each placeholder beside what the real picture must show."""
    out = []
    for n, title, why in TIERS[:4]:
        ks = [sl for sl in SLOTS if sl["tier"] == n]
        if n == 1:                                           # the frame a family at a time, as the sheets have it
            fam = lambda sl: SHEET_GROUPS.index(dct(HUD_ASSIGN.get(sl["id"])).get("group")) if dct(HUD_ASSIGN.get(sl["id"])).get("group") in SHEET_GROUPS else 99
            order = dict((sl["id"], i) for i, sl in enumerate(ks))
            ks.sort(key=lambda sl: (fam(sl), order[sl["id"]]))
        if n == 3:                                           # the stand-in that covers most of the twelve ports comes first
            ks.sort(key=lambda sl: (KIND_ORDER.index(sl["kind"]), -(ROLE_COUNT.get(sl["id"][5:], 0) if sl["id"].startswith("role.") else
                                                                    KIND_COUNT.get(sl["id"][10:], 0) if sl["id"].startswith("scene.any.") else 0), sl["id"]))
        rows = []
        for sl in ks:
            zoom = max(1, min(4, 320 // max(1, sl["w"]), 240 // max(1, sl["h"])))
            rows.append('<tr><td class="p"><a href="%s"><img loading="lazy" decoding="async" src="%s" width="%d" height="%d" alt=""></a></td>'
                        '<td><b>%s</b><br><code>%s</code> &middot; %d x %d<p>%s</p><p class="s">Shown: %s</p></td></tr>'
                        % (html.escape(sl["file"]), html.escape(sl["file"]), sl["w"] * zoom, sl["h"] * zoom, html.escape(sl["name"]),
                           html.escape(sl["file"]), sl["w"], sl["h"], html.escape(sl["desc"]), html.escape(sl["where"])))
        out.append('<h1 id="t%d" style="margin-top:34px">%d. %s (%d)</h1><p class="lede">%s</p><details%s><summary>show the %d pictures</summary><table>%s</table></details>'
                   % (n, n, html.escape(title), len(ks), html.escape(why), " open" if n == 2 else "", len(ks), "\n".join(rows)))
    counts = [sum(1 for sl in SLOTS if sl["tier"] == n) for n, _, _ in TIERS]
    head = ('<!doctype html><meta charset="utf-8"><title>Kapman art: draw first</title><style>' + CSS + ' summary{cursor:pointer;color:#c77fb0;margin:8px 0}</style><main>'
            '<p><a href="index.html">&lsaquo; the whole list</a></p><h1>What to draw first</h1>'
            '<p class="lede">The whole list is %d pictures because it names everything the game could ever show, from 793 to 1066. You do not have to draw them. '
            'The game runs on the placeholders, and it is a game of words: a card or a site with no picture still works. '
            'This page is the short list, in the order that changes the most for the least drawing. Stop wherever you like. '
            'Before you start: <a href="ART%%20GUIDE.md">ART GUIDE.md</a> says, for each kind of picture, its size, its colours, where the light comes from '
            'and what spoils it, and gives a first week of ten pictures. The families of the frame are laid out side by side, eight times their size, in '
            '<a href="_templates/sheets/">_templates/sheets</a>. In part 3 the stand-ins that cover most of the twelve ports come first.</p>'
            '<p class="lede">1. The frame: <b>%d</b> &middot; 2. The first hour: <b>%d</b> &middot; 3. Stand-ins that cover many: <b>%d</b> &middot; '
            '4. The first release, one by one: <b>%d</b> (optional) &middot; later: %d (not needed for the first release)</p>' % ((len(SLOTS),) + tuple(counts)))
    with io.open(os.path.join(desk, "DRAW FIRST.html"), "w", encoding="utf-8") as f:
        f.write(head + "\n".join(out) + "</main>")
    return counts


# ------------------------------------------------------------------------------------------------ the chart's tiles
def lcc(lat, lon):
    """The game's world frame (Lambert conformal, parallels 45N and 65N, centred 56N 5E), in metres east and north."""
    a, f = 6378137.0, 1 / 298.257223563
    e = math.sqrt(2 * f - f * f)

    def m(phi):
        return math.cos(phi) / math.sqrt(1 - (e * math.sin(phi)) ** 2)

    def t(phi):
        return math.tan(math.pi / 4 - phi / 2) / ((1 - e * math.sin(phi)) / (1 + e * math.sin(phi))) ** (e / 2)

    p1, p2, p0, l0 = math.radians(45), math.radians(65), math.radians(56), math.radians(5)
    n = (math.log(m(p1)) - math.log(m(p2))) / (math.log(t(p1)) - math.log(t(p2)))
    F = m(p1) / (n * t(p1) ** n)
    rho, rho0 = a * F * t(math.radians(lat)) ** n, a * F * t(p0) ** n
    th = n * (math.radians(lon) - l0)
    return rho * math.sin(th), rho0 - rho * math.cos(th)


def collect_map():
    mdir = os.path.join(ROOT, "MapArt", "world")
    meta = jload(os.path.join(mdir, "map_art.json"))
    if not meta:
        SKIPPED.append("the chart's tiles: MapArt/world/map_art.json is not there")
        return
    tp = int(artdraw.num(meta.get("tilePixels"), 4096))
    mpp = artdraw.num(meta.get("metresPerPixel"), 120.0)
    west, north = artdraw.num(meta.get("west")), artdraw.num(meta.get("north"))
    cols, rows = int(artdraw.num(meta.get("cols"), 6)), int(artdraw.num(meta.get("rows"), 6))
    in_tile = {}
    for pid, p in W.ports.items():
        try:
            x, y = lcc(float(p.get("lat")), float(p.get("lon")))
        except Exception:
            continue
        cx, ry = (x - west) / (tp * mpp), (north - y) / (tp * mpp)
        if 0 <= cx < cols and 0 <= ry < rows:
            fx, fy = cx - int(cx), ry - int(ry)
            where = ("top" if fy < 0.34 else "bottom" if fy > 0.66 else "middle") + " " + ("left" if fx < 0.34 else "right" if fx > 0.66 else "centre")
            in_tile.setdefault((int(ry), int(cx)), []).append("%s (%s)" % (p.get("name"), where))
    legend = "; ".join("%s = %s" % (k, v) for k, v in dct(meta.get("legend")).items())
    pattern = s(meta.get("tiles")) or "t_{row}_{col}.png"
    for r_ in range(rows):
        for c_ in range(cols):
            fn = pattern.replace("{row}", str(r_)).replace("{col}", str(c_))
            path = os.path.join(mdir, fn)
            if not os.path.exists(path):
                continue
            sid = "map." + os.path.splitext(fn)[0]
            km = tp * mpp / 1000.0
            parts = ["Tile row %d, column %d of the chart (row 0 is the northernmost, column 0 the westernmost): %.0f km a side at %.0f real metres a pixel." % (r_, c_, km, mpp),
                     "Ports in it: %s." % ", ".join(sorted(in_tile.get((r_, c_), []))) if in_tile.get((r_, c_)) else "No port of the table lies in it.",
                     "The placeholder is the tile the game shows now, made from real coasts (%s)." % clip(meta.get("made"), 200),
                     "Colours now: %s." % legend if legend else "",
                     "Keep the coast where it is: ports, sea lanes and fog are placed by real coordinates, not by the picture. Draw on a layer above it in the game's palette.",
                     "The game reads tiles from Kapman2/MapArt/world: pull_art.py puts a redrawn tile into Art/final/map, and with --map also into MapArt/world (then run Bake\\run.bat world\\make_map_overview.py).",
                     "Size: %d x %d pixels." % (tp, tp), file_line("map", sid)]
            slot(sid, "map", "Chart tile %d, %d" % (r_, c_), " ".join(x for x in parts if x), "MapArt/world/map_art.json, " + rel(path), size=(tp, tp), copy=path)
    over = os.path.join(mdir, s(meta.get("overview")) or "overview.png")
    if os.path.exists(over):
        op = int(artdraw.num(meta.get("overviewPixels"), 4096))
        parts = ["The whole chart at one sixth the size, shown from far out and under tiles still loading.",
                 "It is made from the tiles (Bake\\run.bat world\\make_map_overview.py): redraw the tiles first; redraw this one only if the far-out view should differ.",
                 "overview_guide.png beside it in MapArt/world has the places of 793 marked, for drawing; the game does not show it.",
                 "Size: %d x %d pixels." % (op, op), file_line("map", "map.overview")]
        slot("map.overview", "map", "The whole chart, far out", " ".join(parts), "MapArt/world/map_art.json, " + rel(over), size=(op, op), copy=over)


# ------------------------------------------------------------------------------------------------ writing it all out
def sha(data):
    return hashlib.sha1(data).hexdigest()


def file_sha(path):
    h = hashlib.sha1()
    with open(path, "rb") as f:
        for block in iter(lambda: f.read(1 << 20), b""):
            h.update(block)
    return h.hexdigest()


def has_mark(path):
    """True when a picture still carries the placeholder's own corner mark (the 7 x 7 box with the P, top right): then it is a
    placeholder and not a picture of yours. Only for pictures large enough to have the whole mark."""
    try:
        im = Image.open(path).convert("RGBA")
        w, h = im.size
        if w < 48 or h < 40:
            return False
        ref = Cv(w, h)
        artkit.placeholder_mark(ref)
        box = (w - 7, 0, w, 7)
        return list(im.crop(box).getdata()) == list(ref.im.crop(box).getdata())
    except Exception:
        return False


def write_if_changed(path, data):
    if os.path.exists(path):
        try:
            with open(path, "rb") as f:
                if f.read() == data:
                    return False
        except Exception:
            pass
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)
    return True


def render(sl):
    """The placeholder's PNG bytes, and how many of its pixels are off the palette."""
    if sl["copy"]:
        return None, 0
    cv = sl["draw"]()
    im = cv.im if isinstance(cv, Cv) else cv
    if im.size != (sl["w"], sl["h"]):
        fixed = Image.new("RGBA", (sl["w"], sl["h"]), (0, 0, 0, 0))
        fixed.paste(im, (0, 0))
        im = fixed
    bad = off_palette(im)
    buf = io.BytesIO()
    im.save(buf, format="PNG", optimize=True)
    return buf.getvalue(), bad


def page_html(kind, slots, desk):
    w0, h0, heading, where, why = SIZES[kind]
    rows = []
    for sl in slots:
        zoom = max(1, min(4, 320 // max(1, sl["w"]), 240 // max(1, sl["h"])))
        src = "../" + sl["file"]
        if sl["w"] > 700 or sl["h"] > 400:
            src, dw, dh = "thumb/" + sl["id"] + ".png", 384, 384
        else:
            dw, dh = sl["w"] * zoom, sl["h"] * zoom
        rows.append(
            '<tr><td class="p"><a href="../%s"><img loading="lazy" decoding="async" src="%s" width="%d" height="%d" alt=""></a></td>'
            '<td><b>%s</b><br><code>%s</code> &middot; %d x %d &middot; <code>%s</code><p>%s</p><p class="s">Shown: %s<br>From: %s</p></td></tr>'
            % (html.escape(sl["file"]), html.escape(src), dw, dh, html.escape(sl["name"]), html.escape(sl["id"]), sl["w"], sl["h"],
               html.escape(sl["file"]), html.escape(sl["desc"]), html.escape(sl["where"]), html.escape(sl["source"])))
    return PAGE % {"title": html.escape(heading), "n": len(slots), "why": html.escape(why), "where": html.escape(where), "rows": "\n".join(rows)}


CSS = """
body{background:#0b0810;color:#f6efe0;font:16px/1.45 Georgia,'Times New Roman',serif;margin:0;padding:24px 16px 60px}
main{max-width:1180px;margin:0 auto}
h1{font:600 26px/1.2 'Courier New',monospace;color:#f0d27a;margin:0 0 6px}
a{color:#c77fb0} .lede{color:#aaa4ae;max-width:80ch;margin:4px 0 14px}
table{border-collapse:collapse;width:100%} td{vertical-align:top;padding:12px 14px 12px 0;border-top:1px solid #2e1233}
td.p{width:330px;text-align:center} img{image-rendering:pixelated;image-rendering:crisp-edges;background:#4a4350;border:1px solid #2e1233;max-width:100%;height:auto}
code{font:13px 'Courier New',monospace;color:#f0d27a} p{margin:6px 0;max-width:86ch} .s{color:#aaa4ae;font-size:14px}
input{font:15px Georgia,serif;background:#150f1c;color:#f6efe0;border:1px solid #702963;padding:5px 8px;width:320px;max-width:100%}
.k{display:grid;grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:12px;margin-top:14px}
.k a{display:block;border:1px solid #702963;padding:10px 12px;text-decoration:none;color:#f6efe0;background:#150f1c}
.k a b{color:#f0d27a;font:600 17px 'Courier New',monospace} .k a span{display:block;color:#aaa4ae;font-size:14px}
"""
PAGE = """<!doctype html><meta charset="utf-8"><title>Kapman art: %(title)s</title><style>""" + CSS.replace("%", "%%") + """</style>
<main><p><a href="../index.html">&lsaquo; all kinds</a></p><h1>%(title)s (%(n)d)</h1>
<p class="lede">%(why)s<br>Shown: %(where)s</p>
<p><input id="q" placeholder="find by name, id or a word" oninput="f(this.value)"></p>
<table id="t">
%(rows)s
</table></main>
<script>function f(v){v=v.toLowerCase();for(const r of document.getElementById('t').rows)r.style.display=r.textContent.toLowerCase().includes(v)?'':'none'}</script>
"""
INDEX = """<!doctype html><meta charset="utf-8"><title>Kapman art list</title><style>""" + CSS.replace("%", "%%") + """</style>
<main><h1>Kapman: every picture the game could show</h1>
<p class="lede" style="border:1px solid #702963;padding:10px 12px;background:#150f1c"><b style="color:#f0d27a">You do not have to draw all of these.</b>
The game runs on the placeholders. Start with <a href="DRAW FIRST.html"><b>DRAW FIRST</b></a>: %(first)s
Read <a href="ART%%20GUIDE.md"><b>ART GUIDE.md</b></a> before you draw.</p>
<p class="lede">%(n)d pictures in %(k)d kinds, each a placeholder drawn by rule with what the real one must show beside it.
Made %(when)s from the game's data. To replace one: draw it at the same size, save it under the same name in the same folder
(see READ ME FIRST.txt). The list as a table: <a href="art_list.csv">art_list.csv</a>.</p>
<div class="k">
%(cards)s
</div>
<p class="lede" style="margin-top:22px">%(notes)s</p></main>
"""


def readme(counts, total, drawn, gone):
    lines = []
    lines.append("KAPMAN ART: every picture the game needs, as placeholders for you to replace")
    lines.append("Made %s by Kapman2/Bake/art/make_art_list.py from the game's own data." % datetime.datetime.now().strftime("%d %B %Y, %H:%M"))
    lines.append("")
    lines.append("DO I HAVE TO DRAW ALL OF THEM?")
    lines.append("  No. The list names everything the game could ever show, 793 to 1066, so that nothing is ever without a name.")
    lines.append("  The game runs on the placeholders, and a card or a site with no picture still works. Open DRAW FIRST.html:")
    for n, title, why in TIERS:
        lines.append("  %d. %-30s %5d   %s" % (n, title, sum(1 for sl in SLOTS if sl.get("tier") == n), why))
    lines.append("  The last column of art_list.csv ('draw when') says which of these each picture is.")
    lines.append("")
    lines.append("WHAT IS HERE")
    lines.append("  index.html         open this: every placeholder beside what the real picture must show, one page a kind")
    lines.append("  art_list.csv       the same list as a table (id, kind, name, where used, size, file, description, source data file)")
    lines.append("  <kind>/<id>.png    the placeholders, one folder a kind")
    lines.append("  palette_byzantium.gpl / .png   the game's 40 colours (Aseprite: Palette > Load)")
    lines.append("  ART GUIDE.md       read this before you draw: for each kind of picture its size, its colours, where the light comes from,")
    lines.append("                     what spoils it; and a first week of ten pictures")
    lines.append("  DRAW FIRST.html    the short list: the frame, the first hour, the stand-ins that cover many")
    lines.append("  _templates/        an empty picture of the right size for each kind with the colours loaded, the same with guide lines,")
    lines.append("                     and where each piece of the frame is cut in nine (see READ ME.txt in it)")
    lines.append("  _templates/sheets/ each family of the frame (frames, buttons, marks, the five sets of icons) side by side at 8 times its size")
    lines.append("  hud/  shipview/    the Command frame: frames, buttons, marks, icons; and the views behind your ship")
    lines.append("")
    lines.append("HOW TO REPLACE A PICTURE")
    lines.append("  1. Open the placeholder (or start new) at the SAME SIZE in pixels. The size is in index.html and art_list.csv.")
    lines.append("  2. Draw it in the game's 40 colours. Icons, marks, ships and leaves keep a clear (transparent) background.")
    lines.append("  3. Save it as PNG under the SAME NAME in the SAME FOLDER, over the placeholder.")
    lines.append("  4. Leave out the placeholder's own marks: the purple and gold corner with the P, and the dark name strip along the bottom.")
    lines.append("     They are only there to say 'this is a placeholder'; your picture uses the whole size.")
    lines.append("")
    lines.append("HOW TO BRING THE FOLDER BACK INTO THE GAME")
    lines.append("  Run, in PowerShell:")
    lines.append('    & "C:\\Program Files\\QGIS 3.44.12\\bin\\python-qgis-ltr.bat" G:\\kapman_unity\\Kapman2\\Bake\\art\\pull_art.py')
    lines.append("  It finds every picture here that is no longer the placeholder, copies it to Kapman2/Art/final/<kind>/ and lists it.")
    lines.append("  It tells you if a picture has the wrong size or a name the list does not know. Add --dry to only look.")
    lines.append("  Chart tiles: add --map to put redrawn tiles into Kapman2/MapArt/world as well (the game reads them from there).")
    lines.append("")
    lines.append("WHEN THE LIST IS MADE AGAIN")
    lines.append("  The game's data is still being written. Running make_art_list.py again adds the new pictures and refreshes the")
    lines.append("  descriptions. It never overwrites a picture you have changed here, and a slot's id and file name never change.")
    lines.append("  A slot whose thing has left the data is listed in Kapman2/Art/art_list_retired.csv; its file stays where it is.")
    lines.append("")
    lines.append("SIZES (pixel art for a 640 x 360 screen shown x3, and a 960 x 540 screen shown x2; always whole numbers)")
    for k in KIND_ORDER:
        if k in counts:
            w, h, heading, where, why = SIZES[k]
            lines.append("  %-11s %5d   %-9s  %s" % (k, counts[k], ("%d x %d" % (w, h)) if w else "various", why))
    lines.append("  %-11s %5d" % ("in all", total))
    lines.append("")
    lines.append("HOW TO READ A PLACEHOLDER (they are drawn by rule, so that each is of its thing and can be told from its neighbours)")
    lines.append("  portrait    ground colour = the person's people; corner sign = faith (cross: Christian, hammer: the old gods,")
    lines.append("              post: Slavic gods, tree: Baltic gods); boxed sign = role; build, hair, beard, hood and cloak from")
    lines.append("              the people, the role, the age and any plain word in 'looks' (grey, bald, heavy, a blue cloak).")
    lines.append("  scene       what stands in it = the site's kind (a quay is not a church); the far country = the port's region;")
    lines.append("              the light = any plain word in 'see' (dusk, fog, snow), else by chance.")
    lines.append("  harbour     buildings = the port's kind and size; the shore = her landing; your own stem in the corner.")
    lines.append("  card        the ground = the card's stage (harbour, sea, crew, long); small figures = its people with the signs")
    lines.append("              of their roles; large signs = the plain words of the card (toll, silver, a sail, fire, a church).")
    lines.append("  good        the shape = how it is traded (cask, sack, bolt, bundle of skins); the colours = its kind.")
    lines.append("  ship        every class at the same scale, from her length, freeboard, stems, mast, sail and oars.")
    lines.append("  leaf        rune stick, leaf, book or wax tablet by the text's form; riddles carry the question sign and pips.")
    lines.append("  hud         a piece 'cut in nine' keeps its corners and stretches its sides: the corner size is in its description and")
    lines.append("              _templates/cut_in_nine shows the cuts in gold. Kapman2/Art/hud_assign.json tells the game where each goes.")
    lines.append("  shipview    sky and water by the hour, the shore by where she is; the horizon is at the same height in all twelve.")
    lines.append("              A stand-in: the ship window is to become a live 3D view, so these are not among the pictures to draw first.")
    lines.append("  ui          the cursor and the marks that lie on the chart. The rest of that folder is of the older ink-slip look, which the")
    lines.append("              Command frame replaced: each such piece says so, and is marked 'later'.")
    lines.append("")
    lines.append("WHAT THE DESCRIPTIONS ARE")
    lines.append("  Every line of a description is taken from the game's data (the file is named beside it). Where the data says")
    lines.append("  nothing (the light, a face, a season), the description says so and leaves it to you. Nothing is invented.")
    if gone:
        lines.append("")
        lines.append("RETIRED THIS RUN (the thing has left the data; the file is left alone): %d, see Kapman2/Art/art_list_retired.csv" % len(gone))
    return "\r\n".join(lines) + "\r\n"


def contact_sheet(kind, slots, folder):
    """Some placeholders of a kind on one sheet, to look them over."""
    if not slots:
        return
    w, h = slots[0]["w"], slots[0]["h"]
    if w > 700:
        return
    zoom = max(1, min(4, 256 // max(w, 1), 192 // max(h, 1)))
    n = min(len(slots), max(4, min(60, (1400 // (w * zoom + 6)) * max(1, 900 // (h * zoom + 6)))))
    step = max(1, len(slots) // n)
    pick = slots[::step][:n]
    cols = max(1, min(len(pick), 1400 // (max(sl["w"] for sl in pick) * zoom + 6)))
    cw = max(sl["w"] for sl in pick) * zoom + 6
    chh = max(sl["h"] for sl in pick) * zoom + 6
    rows = (len(pick) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * cw, rows * chh), (74, 67, 80, 255))
    for i, sl in enumerate(pick):
        p = os.path.join(ART, "placeholder", sl["file"])
        if os.path.exists(p):
            im = Image.open(p).convert("RGBA")
            im = im.resize((im.size[0] * zoom, im.size[1] * zoom), Image.NEAREST)
            sheet.alpha_composite(im, ((i % cols) * cw + 3, (i // cols) * chh + 3))
    os.makedirs(folder, exist_ok=True)
    sheet.save(os.path.join(folder, "sheet_%s.png" % kind))


def main():
    artkit.load_palette(os.path.join(DESIGN, "palette_byzantium.gpl"))
    say("MAKE ART LIST  root %s" % ROOT)
    load_world()
    collect_chains()
    steps = [("people", collect_people), ("the opening", collect_opening), ("ports, sites, stories, works", collect_ports), ("cards", collect_cards), ("endings", collect_endings),
             ("goods", collect_goods), ("ships", collect_ships), ("icons", collect_story_tables), ("texts and riddles", collect_leaves),
             ("plates", collect_plates), ("interface", collect_ui), ("the frame and the ship window", collect_hud),
             ("people of weight, your own face", collect_weight), ("stand-ins", collect_standins), ("chart tiles", collect_map)]
    for name, fn in steps:
        n0 = len(SLOTS)
        try:
            fn()
        except Exception as e:                                # one bad table must not stop the list
            import traceback
            SKIPPED.append("%s: stopped on an error (%s); what it had found is kept" % (name, repr(e)[:160]))
            say(traceback.format_exc())
        say("  %-30s %5d slots" % (name, len(SLOTS) - n0))
    SLOTS.sort(key=lambda sl: (KIND_ORDER.index(sl["kind"]) if sl["kind"] in KIND_ORDER else 99, sl["id"]))
    counts = {}
    for sl in SLOTS:
        counts[sl["kind"]] = counts.get(sl["kind"], 0) + 1
        sl["tier"] = tier_of(sl)
        sl["desc"] = lead_of(sl) + sl["desc"]
    tier_names = dict((n, "%d %s" % (n, t)) for n, t, _ in TIERS)

    # ---- the registry of slots: an id is issued once and is never given to another thing
    os.makedirs(ART, exist_ok=True)
    reg_path = os.path.join(ART, "art_slots.json")
    reg = dct((jload(reg_path) or {}).get("slots"))
    today = datetime.date.today().isoformat()
    now_ids = {sl["id"] for sl in SLOTS}
    for sl in SLOTS:
        r = reg.setdefault(sl["id"], {"kind": sl["kind"], "first": today})
        r.update({"kind": sl["kind"], "last": today, "file": sl["file"], "size": "%dx%d" % (sl["w"], sl["h"]), "name": sl["name"]})
        r.pop("retired", None)
    gone = sorted(i for i in reg if i not in now_ids)
    for i in gone:
        reg[i].setdefault("retired", today)

    # ---- the placeholders
    man_path = os.path.join(ART, "placeholder_manifest.json")
    manifest = dct((jload(man_path) or {}).get("files"))
    new_manifest = {}
    drawn = kept = fresh = bad_total = failed = 0
    desk_ok = False
    if DESK:
        try:
            os.makedirs(DESK, exist_ok=True)
            desk_ok = True
        except Exception as e:
            SKIPPED.append("the Desktop folder %s could not be made (%s)" % (DESK, e))
    for sl in SLOTS:
        if ONLY and sl["kind"] not in ONLY:
            if sl["file"] in manifest:
                new_manifest[sl["file"]] = manifest[sl["file"]]
            continue
        target = os.path.join(ART, "placeholder", sl["kind"], sl["id"] + ".png")
        try:
            if sl["copy"]:
                digest = file_sha(sl["copy"])
                if not os.path.exists(target) or file_sha(target) != digest:
                    os.makedirs(os.path.dirname(target), exist_ok=True)
                    shutil.copyfile(sl["copy"], target)
                data = None
            else:
                data, bad = render(sl)
                bad_total += bad
                if bad:
                    say("  ! %s has %d pixels off the palette" % (sl["id"], bad))
                digest = sha(data)
                write_if_changed(target, data)
            drawn += 1
        except Exception as e:
            failed += 1
            SKIPPED.append("%s: could not be drawn (%s)" % (sl["id"], repr(e)[:160]))
            continue
        new_manifest[sl["file"]] = digest
        if desk_ok:
            dt = os.path.join(DESK, sl["kind"], sl["id"] + ".png")
            try:
                if os.path.exists(dt):
                    cur = file_sha(dt)
                    if cur == digest:
                        continue
                    if cur != manifest.get(sl["file"]):
                        kept += 1                                 # yours: never overwritten
                        continue
                os.makedirs(os.path.dirname(dt), exist_ok=True)
                if sl["copy"]:
                    shutil.copyfile(sl["copy"], dt)
                else:
                    with open(dt, "wb") as f:
                        f.write(data)
                fresh += 1
            except Exception as e:
                SKIPPED.append("%s: could not be written to the Desktop (%s)" % (sl["file"], repr(e)[:120]))
    # placeholders of slots that are gone: take the generated one away, leave anything the user changed
    def write_whole(path, write):
        """Write beside the file and swap it in, so a stop in mid-write never leaves half a file."""
        tmp = path + ".tmp"
        with io.open(tmp, "w", encoding="utf-8") as f:
            write(f)
        os.replace(tmp, path)

    try:
        write_whole(reg_path, lambda f: json.dump(
            {"about": "Every art slot ever issued by Bake/art/make_art_list.py. An id is never reused for another thing.", "slots": reg},
            f, ensure_ascii=False, indent=1, sort_keys=True))
        write_whole(man_path, lambda f: json.dump(
            {"about": "SHA-1 of each placeholder as last made: pull_art.py and the next run use it to tell a placeholder from a picture you drew.",
             "made": datetime.datetime.now().isoformat(timespec="seconds"), "files": new_manifest}, f, indent=0, sort_keys=True))
    except Exception as e:
        SKIPPED.append("the list of slots or of placeholders could not be written (%s): run again before drawing over anything" % repr(e)[:160])
    for relf, digest in manifest.items():
        if relf in new_manifest:
            continue
        p = os.path.join(ART, "placeholder", relf)
        try:
            if os.path.exists(p):
                os.remove(p)
        except Exception:
            pass
        if desk_ok:
            dp = os.path.join(DESK, relf)
            try:
                if os.path.exists(dp) and file_sha(dp) == digest:
                    os.remove(dp)
            except Exception:
                pass
    # a retired slot whose picture on the Desktop still has the placeholder's corner mark: a left-over placeholder, taken away
    if desk_ok:
        for i in gone:
            dp = os.path.join(DESK, s(reg[i].get("file")))
            try:
                if reg[i].get("file") and os.path.exists(dp) and has_mark(dp):
                    os.remove(dp)
            except Exception:
                pass
    # a superseded interface piece whose placeholder an earlier run left on the Desktop: taken away if it is still exactly the
    # placeholder (drawn again at its registered size and compared); a picture you changed stays
    if desk_ok and (not ONLY or "ui" in ONLY):
        for piece in artscene.UI_PIECES:
            dp = os.path.join(DESK, "ui", "ui.%s.png" % piece[0])
            r = reg.get("ui." + piece[0])
            if piece[0] not in SUPERSEDED or not r or not os.path.exists(dp):
                continue
            try:
                w, h = [int(x) for x in s(r.get("size")).split("x")]
                buf = io.BytesIO()
                artscene.draw_ui((piece[0], piece[1], w, h) + tuple(piece[4:])).im.save(buf, format="PNG", optimize=True)
                if file_sha(dp) == sha(buf.getvalue()):
                    os.remove(dp)
            except Exception:
                pass

    # ---- the list
    def write_csv(path):
        with io.open(path, "w", encoding="utf-8-sig", newline="") as f:
            wr = csv.writer(f)
            wr.writerow(["id", "kind", "name", "where used", "size", "file", "description", "source data file", "draw when"])
            for sl in SLOTS:
                wr.writerow([sl["id"], sl["kind"], sl["name"], sl["where"], "%dx%d" % (sl["w"], sl["h"]), sl["file"], sl["desc"], sl["source"], tier_names[sl["tier"]]])

    try:
        write_csv(os.path.join(ART, "art_list.csv"))
        with io.open(os.path.join(ART, "art_list_retired.csv"), "w", encoding="utf-8-sig", newline="") as f:
            wr = csv.writer(f)
            wr.writerow(["id", "kind", "name", "file", "first listed", "retired"])
            for i in gone:
                wr.writerow([i, reg[i].get("kind"), reg[i].get("name"), reg[i].get("file"), reg[i].get("first"), reg[i].get("retired")])
    except Exception as e:
        SKIPPED.append("Art/art_list.csv could not be written (%s): is it open in another program? Close it and run again" % repr(e)[:160])

    if PERSON_SLOTS:
        write_if_changed(os.path.join(ART, "person_slots.json"), json.dumps({
            "about": "People of the places files (Data/places/<port>.json) whose portrait is not portrait/person.<id>.png: they are of the cast of "
                     "Design/content/spine.json and their slot was issued under the cast id, which never changes. id of the places file -> file. "
                     "Everyone else of a places file has portrait/person.<id>.png. Made by Bake/art/make_art_list.py.",
            "people": PERSON_SLOTS}, indent=1, ensure_ascii=False, sort_keys=True).encode("utf-8"))
    if HUD_ASSIGN:
        write_if_changed(os.path.join(ART, "hud_assign.json"), json.dumps({
            "about": "Where each piece of the Command frame goes and how it is cut. slice = left, top, right, bottom in pixels (null: not stretched). "
                     "today = the class of Assets/Kapman/Game/UI/Frame.uss, or the sign drawn in code, that shows this piece in the game now "
                     "(empty: nothing shows it yet). The game takes Art/final/<file> when it is there, else Art/placeholder/<file>.",
            "pieces": HUD_ASSIGN}, indent=1, ensure_ascii=False).encode("utf-8"))

    # ---- the Desktop folder: the list, the pages, the palette
    if desk_ok:
        try:
            write_csv(os.path.join(DESK, "art_list.csv"))
            for fn in ("palette_byzantium.gpl", "palette_byzantium.png"):
                if os.path.exists(os.path.join(DESIGN, fn)):
                    shutil.copyfile(os.path.join(DESIGN, fn), os.path.join(DESK, fn))
            write_templates(os.path.join(DESK, "_templates"))
            write_sheets(os.path.join(DESK, "_templates", "sheets"), DESK)
            guide = os.path.join(DESIGN, "art_guide.md")
            if os.path.exists(guide):
                shutil.copyfile(guide, os.path.join(DESK, "ART GUIDE.md"))
            else:
                SKIPPED.append("Design/art_guide.md is not there, so the Desktop folder has no ART GUIDE.md")
            tier_counts = first_page(DESK)
            pages = os.path.join(DESK, "_pages")
            os.makedirs(os.path.join(pages, "thumb"), exist_ok=True)
            cards = []
            for k in KIND_ORDER:
                ks = [sl for sl in SLOTS if sl["kind"] == k]
                if not ks:
                    continue
                for sl in ks:
                    if sl["w"] > 700 or sl["h"] > 400:
                        tp = os.path.join(pages, "thumb", sl["id"] + ".png")
                        src = os.path.join(DESK, sl["file"])
                        if os.path.exists(src) and (not os.path.exists(tp) or os.path.getmtime(tp) < os.path.getmtime(src)):
                            try:
                                im = Image.open(src)
                                im.draft("RGB", (512, 512))
                                im.convert("RGB").resize((384, 384), Image.NEAREST).save(tp, optimize=True)
                            except Exception as e:
                                SKIPPED.append("thumbnail of %s: %s" % (sl["file"], repr(e)[:80]))
                with io.open(os.path.join(pages, k + ".html"), "w", encoding="utf-8") as f:
                    f.write(page_html(k, ks, DESK))
                w, h, heading, where, why = SIZES[k]
                cards.append('<a href="_pages/%s.html"><b>%s</b><span>%d pictures &middot; %s</span><span>%s</span></a>'
                             % (k, html.escape(heading), len(ks), ("%d x %d" % (w, h)) if w else "each its own size", html.escape(where)))
            notes = "Still to come as the data grows: " + "; ".join(html.escape(x) for x in SKIPPED[:12]) if SKIPPED else ""
            with io.open(os.path.join(DESK, "index.html"), "w", encoding="utf-8") as f:
                f.write(INDEX % {"n": len(SLOTS), "k": len(counts), "when": datetime.datetime.now().strftime("%d %B %Y, %H:%M"), "cards": "\n".join(cards), "notes": notes,
                                 "first": "the frame (%d), the first hour (%d), stand-ins that cover many (%d); then, only if you wish, the first release one by one (%d)." % tuple(tier_counts[:4])})
            with io.open(os.path.join(DESK, "READ ME FIRST.txt"), "w", encoding="utf-8-sig", newline="") as f:
                f.write(readme(counts, len(SLOTS), drawn, gone))
        except Exception as e:
            import traceback
            SKIPPED.append("the Desktop pages: stopped on an error (%s)" % repr(e)[:160])
            say(traceback.format_exc())
    if SHEETS:
        for k in KIND_ORDER:
            contact_sheet(k, [sl for sl in SLOTS if sl["kind"] == k], SHEETS)

    say("")
    say("SLOTS BY KIND")
    for k in KIND_ORDER:
        if k in counts:
            w, h = SIZES[k][:2]
            say("  %-11s %5d   %s" % (k, counts[k], ("%d x %d" % (w, h)) if w else "various"))
    say("  %-11s %5d" % ("in all", len(SLOTS)))
    say("DRAW WHEN")
    for n, title, why in TIERS:
        say("  %d %-32s %5d" % (n, title, sum(1 for sl in SLOTS if sl["tier"] == n)))
    say("drawn %d, off-palette pixels %d, could not draw %d, retired %d" % (drawn, bad_total, failed, len(gone)))
    if desk_ok:
        say("Desktop: %s  (%d written or refreshed, %d of yours left alone)" % (DESK, fresh, kept))
    if SKIPPED:
        say("NOT THERE YET OR SKIPPED")
        for x in SKIPPED:
            say("  - " + x)
    with io.open(os.path.join(ART, "art_list_report.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(LOG) + "\n")
    return 0


if __name__ == "__main__":
    try:
        code = main()
    except Exception:
        import traceback
        print(traceback.format_exc())                        # on the ordinary output, so it is seen and logged
        code = 1
    sys.exit(code)
