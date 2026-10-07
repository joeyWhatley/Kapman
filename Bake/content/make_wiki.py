# -*- coding: utf-8 -*-
r"""
MAKE WIKI - bakes the in-game wiki from the game's own data (Design/wiki.md; DECISIONS.md, 7 October 2026, rulings 3
and 6). It reads the data files, writes every page by rule, folds in the hand-written rules of wiki_rules.json, and
writes ONE file: Bake/content/out/wiki_793.json (outside Assets on purpose: the lead moves it to
Assets/Kapman/Game/Data/story when the panel is ready for it).

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\make_wiki.py
  ... make_wiki.py --sample C:\some\folder 50     also writes 50 pages of each kind as plain text, to read through

It can be run again whenever the data grows. A file that is missing or will not parse is skipped and named in the
report; nothing else stops for it. Nothing is invented here: every sentence is either a sentence of the data, or a
sentence built round a field of the data. Check the result with check_wiki.py.

WHAT IS IN THE FILE
  pages   id, kind, title, hint, brief, game, known, source, links, opens, first_release (and, where it applies:
          later, built, built_note, holds).
            game    what it is and does for the player.   known   what the record says, from the data's own notes.
                    Both may hold line breaks: one paragraph a line (ports and faiths do).
            source  where the data has it from, as the data gives it.
            later   pieces of the page's text that name a day after the first morning (21 June 793): each has the
                    date after which it may be shown, the part it belongs to, and the text. The wiki must not show a
                    day that has not come, so these are kept out of game and known and handed over apart.
            opens   when the player knows the page: always | port_heard:<port> (heard of, or seen) |
                    port_seen:<port> | good_seen:<good> | person_met:<person> | word_heard:<tongue.word> |
                    text_held:<text> | date:<yyyy-mm-dd> (that day has passed). Two rules joined by & must both hold.
  names   every word or phrase that becomes a link, the longest first, each with the page it leads to and, where
          two pages wanted the same word, a note of who else wanted it and why this one won ('also' lists them).
  shields phrases that hold a name inside them and must not link (New York is not York).
  HOW A NAME IS MATCHED (the baker finds each page's links this way, and the frame should too): exactly as written,
  capitals and all; the letter before it and the letter after it must not be letters; the longest name wins; the
  first use in a box becomes the link.
"""
import collections
import datetime
import glob
import io
import json
import os
import random
import re
import sys
import unicodedata

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
CONTENT = os.path.join(ROOT, "Design", "content")
OUT = os.path.join(HERE, "out", "wiki_793.json")
RULES = os.path.join(HERE, "wiki_rules.json")

TWELVE = ["dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre",
          "quentovic", "sarre", "fordwic", "ripa"]
START_YEAR, START_DAY, LIFE_END = 793, "793-06-21", 805
KINDS = ["how_to_play", "port", "site", "person", "good", "ship", "people", "faith", "day", "word", "event", "text"]
# when two pages want one name, the kind earlier in this list wins
KIND_RANK = {k: i for i, k in enumerate(["how_to_play", "port", "people", "faith", "day", "good", "ship", "person",
                                         "site", "text", "word", "event"])}
MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
          "November", "December"]

skipped = []      # (file, why): data that could not be read
remarks = []      # things found in the data on the way, for the report


def rel(path):
    try:
        return os.path.relpath(path, ROOT).replace("\\", "/")
    except ValueError:                                         # (another drive)
        return path.replace("\\", "/")


def load(path):
    """A JSON file as data, or None (and a line in the report) when it is missing or will not parse."""
    if not os.path.isfile(path):
        skipped.append((rel(path), "missing"))
        return None
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            return json.load(f)
    except Exception as e:
        skipped.append((rel(path), "will not parse: %s" % str(e)[:90]))
        return None


def read_text(path):
    if not os.path.isfile(path):
        skipped.append((rel(path), "missing"))
        return None
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            return f.read()
    except Exception as e:
        skipped.append((rel(path), "cannot be read: %s" % str(e)[:90]))
        return None


# ------------------------------------------------------------------------------------------------ words and sentences

def norm(s):
    return re.sub(r"\s+", " ", s or "").strip()


def stop(s):
    """The text as a finished sentence: a full stop at the end unless it has an end already."""
    s = norm(s)
    if not s:
        return ""
    if s[-1] in ".!?" or s.endswith((".)", '."', ".'", ".\u201d", ".\u2019", '!"', '?"')):
        return s
    return s.rstrip(";:,") + "."


def cap(s):
    s = norm(s)
    for i, c in enumerate(s):
        if c.isalpha():
            return s[:i] + c.upper() + s[i + 1:]
        if c.isdigit():
            break
    return s


def low_article(s):
    """'The Rhine delta' -> 'the Rhine delta': only a leading article is lowered."""
    s = norm(s)
    for a in ("The ", "A ", "An "):
        if s.startswith(a):
            return a.lower() + s[len(a):]
    return s


SMALL = set()     # every word the data itself writes small: 'wheat' is in it, 'rhenish' and 'frisia' are not
NEVER = set()     # the words of wiki_rules.json do_not_link, with a small and a large first letter


def low_plain(s):
    """Lowers the first letter unless the first word is a port's or a people's name ('About forty' -> 'about forty')."""
    s = norm(s)
    w = re.split(r"[\s,;:.!?)]", s, 1)[0] if s else ""
    if not w or w in W.proper or any(c.isupper() for c in w[1:]) or w == "I":
        return s
    return s[0].lower() + s[1:]


def low_first(s):
    """Lowers the first letter where the data itself writes that word small. A name keeps its capital:
    'The mint' -> 'the mint', 'Bar iron' -> 'bar iron', but 'Rhenish wine', 'Frisia', 'Offa's' stay."""
    s = norm(s)
    if not s:
        return s
    w = re.split(r"[\s,;:.!?)]", s, 1)[0]
    if w in ("I",) or any(c.isupper() for c in w[1:]) or not w[:1].isupper():
        return s
    if w.lower() not in SMALL:
        return s
    return s[0].lower() + s[1:]


def listing(items, last="and"):
    items = [norm(x) for x in items if norm(x)]
    if not items:
        return ""
    if len(items) == 1:
        return items[0]
    return ", ".join(items[:-1]) + " " + last + " " + items[-1]


def a_an(phrase):
    phrase = norm(phrase)
    if not phrase:
        return phrase
    return ("an " if phrase[0].lower() in "aeiou" else "a ") + phrase


def number(n):
    """A figure as the page prints it: 60, 0.7, 1,200."""
    try:
        f = float(n)
    except (TypeError, ValueError):
        return str(n)
    if abs(f - round(f)) < 1e-9:
        return "{:,}".format(int(round(f)))
    return ("%.2f" % f).rstrip("0").rstrip(".")


def fold(s):
    """A name in plain letters: Hleiðr -> Hleidr, Serræ -> Serrae, Skíringssalr -> Skiringssalr."""
    for a, b in (("ð", "d"), ("Ð", "D"), ("þ", "th"), ("Þ", "Th"), ("æ", "ae"), ("Æ", "Ae"), ("ø", "o"), ("Ø", "O"),
                 ("ß", "ss"), ("œ", "oe"), ("ł", "l"), ("ƀ", "b"), ("đ", "d")):
        s = s.replace(a, b)
    return "".join(c for c in unicodedata.normalize("NFKD", s) if not unicodedata.combining(c))


def slug(s):
    s = fold(s).lower()
    return re.sub(r"[^a-z0-9]+", "_", s).strip("_")


def strip_paren(s):
    """The text without its bracketed asides: 'Candlemas (the Purification)' -> 'Candlemas'."""
    prev = None
    while prev != s:
        prev = s
        s = re.sub(r"\s*\([^()]*\)", "", s)
    return norm(s)


_ABBR = {"c", "ca", "st", "no", "nos", "p", "pp", "ed", "tr", "vol", "d", "b", "cf", "esp", "fol", "ff", "capit",
         "ann", "chron", "ep", "epp", "al", "bk", "ch", "art", "tit", "ms", "mt", "lat", "ofris", "ar", "approx",
         "v", "ss", "cod", "add", "lib", "cap", "fr", "abp", "bp", "e.g", "i.e", "s.v", "etc", "viz", "nr"}


def sentences(text):
    """The text cut into sentences. Careful with 'c. 4', 'St. Martin', 'A. J. Grant' and '(?)'."""
    text = norm(text)
    out, start = [], 0
    for m in re.finditer(r"([.!?])([\"'\)\]\u201d\u2019]*)\s+", text):
        i = m.start(1)
        mark = m.group(1)
        nxt = text[m.end():m.end() + 1]
        if not nxt:
            break
        if mark == "?" and i > 0 and text[i - 1] == "(":
            continue                                           # (?) is a mark of doubt, not an end
        if mark == ".":
            before = re.search(r"([\w.]+)$", text[start:i])
            word = before.group(1) if before else ""
            if word.lower() in _ABBR or (len(word) == 1 and word.isalpha()):
                continue
            if word.isdigit() and nxt.islower():
                continue
        if not (nxt.isupper() or nxt.isdigit() or nxt in "\"'(\u201c\u2018[+"):
            continue
        out.append(text[start:m.end(2)].strip())
        start = m.end()
    tail = text[start:].strip()
    if tail:
        out.append(tail)
    return out


def first_sentences(text, enough=60, most=240):
    """One or two sentences for the box under the pointer."""
    out = ""
    for s in sentences(text):
        if out and len(out) + len(s) + 1 > most:
            break
        out = (out + " " + s).strip()
        if len(out) >= enough:
            break
    if len(out) > most:
        cut = max(out.rfind("; ", 0, most), out.rfind(": ", 0, most), out.rfind(", ", 0, most))
        out = out[:cut if cut > 80 else most].rstrip(" ,;:")
    return stop(out)


def date_words(s):
    """'793-06-08' -> '8 June 793'; '794-06' -> 'June 794'; '793' -> '793'."""
    parts = str(s).split("-")
    try:
        if len(parts) >= 3:
            return "%d %s %d" % (int(parts[2]), MONTHS[int(parts[1]) - 1], int(parts[0]))
        if len(parts) == 2:
            return "%s %d" % (MONTHS[int(parts[1]) - 1], int(parts[0]))
        return "%d" % int(parts[0])
    except (ValueError, IndexError):
        return str(s)


def md_words(s):
    """'06-08' -> '8 June'."""
    try:
        m, d = str(s).split("-")
        return "%d %s" % (int(d), MONTHS[int(m) - 1])
    except (ValueError, IndexError):
        return str(s)


def iso(y, m=12, d=31):
    return "%d-%02d-%02d" % (y, m, d)


# ------------------------------------------------------------------------------------------------ the day that has not come

_UNITS = (r"(?:d|p|g|kg|km|m|t|men|man|ships?|sails?|keels?|pennies|pence|deniers|pounds?|marks?|hides|graves|coins|"
          r"casks|sacks|paces|feet|ells|litres|years|days|hours|miles|head|souls|people|dead|monks|oars|tons|tonnes|"
          r"solidi|sceattas|dirhams|shillings|loaves|houses|plots|perches|sheep|cattle|horses|pigs|captives|slaves|"
          r"skins|pieces|leaves|lines|of them|in all|strong|by)\b")
_NUMBER = re.compile(r"(?<![\w.,/#])(\d{1,4})(s?)(?![\w/]|[.,]\d)")
_MONTH_BEFORE = re.compile(r"(?:(\d{1,2}) )?(" + "|".join(MONTHS) + r") $")
# a year that only dates a source is not a day of the story: 'the charters of 777 and 815', 'ARF 808', 'S 160 (804)'
_CITE = re.compile(
    r"(?:charters?|precepts?|councils?|synods?|capitular(?:y|ies)|edicts?|laws?|grants?|annals?|letters?|diplomas?|"
    r"privileges?|judgements?|vows?|oaths?|stones?|gloss(?:es)?|lists?|polyptychs?)\s+(?:of|for|dated)\s+(?:about\s+|c\.\s+)?$|"
    r"(?:ARF|RFA|ASC|AU|AB|AF|AX|AV|HR|AI|AFM|CS|AC|Sawyer|Birch|MGH|Asser|Thietmar|Regino|Flodoard|Nithard|Rimbert|"
    r"Dudo|Alpert|Notker|Astronomer|Einhard|Bede|Alcuin|Adam of Bremen|Chronicle|Annals)"
    r"(?:\s+\([A-Za-z,/ ]+\))?(?:,?\s+[IVXLC]+\.?\d*)?,?\s+(?:rev\.\s+|for\s+|under\s+|c\.\s+)?$|"
    r"\bS\s?\d+,?\s+\(?$")
_CHAIN = re.compile(r"^(?:\s|,|and|to|or|-|–|/|\(|\)|=|c\.|about)*$")
_DATE_ONLY = re.compile(r"^(?:about |c\. ?|ca\. ?)?\d{3,4}s?(?:\s?[-–/]\s?\d{1,4})?(?:\s?\(\?\))?$")


def future_dates(text, year0):
    """Every mention in the text of a day of the story after year0 (and up to 1066): [(start, end, iso date)].
    Left out: figures with a unit (800 d, 900 kg), and years that only date a source."""
    found = []
    brackets = [(m.start(), m.end()) for m in re.finditer(r"\(([^()]*)\)", text) if _DATE_ONLY.match(m.group(1).strip())]
    cited, prev_end = False, 0
    for m in _NUMBER.finditer(text):
        was_cited = cited
        cited = bool(_CITE.search(text[max(0, m.start() - 70):m.start()]))
        if not cited and was_cited and _CHAIN.match(text[prev_end:m.start()]):
            cited = True
        if not cited and any(a <= m.start() and m.end() <= b for a, b in brackets):
            cited = True
        prev_end = m.end()
        y = int(m.group(1))
        if cited or y <= year0 or y > 1066:
            continue
        if re.match(r"\s+" + _UNITS, text[m.end():]):
            continue
        back = _MONTH_BEFORE.search(text[max(0, m.start() - 16):m.start()])
        if back and not m.group(2):
            month = MONTHS.index(back.group(2)) + 1
            found.append((m.start(), m.end(), iso(y, month, int(back.group(1)) if back.group(1) else 28)))
        else:
            found.append((m.start(), m.end(), iso(y)))
    return found


def balance(s):
    """A cut can leave a bracket open or shut alone: mend it."""
    s = norm(s)
    opens, shuts = s.count("("), s.count(")")
    if opens > shuts:
        s = s[1:] if s.startswith("(") else s + ")"
    elif shuts > opens:
        i = s.rfind(")")
        s = s[:i] + s[i + 1:]
    return norm(s)


def _cut_piece(piece, year0):
    """A piece of a sentence with a day in it that has not come: what of it may stand now (or '')."""
    hits = future_dates(piece, year0)
    # (a) the day stands in brackets only: the brackets go, the rest stays
    spans = [(m.start(), m.end()) for m in re.finditer(r"\s*\([^()]*\)", piece)]
    inside = [s for s in spans if any(s[0] <= h[0] and h[1] <= s[1] for h in hits)]
    if inside and all(any(s[0] <= h[0] and h[1] <= s[1] for s in inside) for h in hits):
        kept, at = "", 0
        for s in inside:
            kept += piece[at:s[0]]
            at = s[1]
        kept = norm(kept + piece[at:])
        if len(kept.split()) >= 3:
            return kept
    # (b) the day stands in the last clause after a comma: the clause goes
    first = min(h[0] for h in hits)
    comma = piece.rfind(", ", 0, first)
    if comma > 0 and ", " not in piece[first:] and len(piece[:comma].split()) >= 4:
        return piece[:comma]
    return ""


def gate(text, year0=START_YEAR):
    """(what may be said now, [(iso date, the whole sentence, to be shown when that day has passed)]).
    A sentence that names a day not yet come is cut back to what can be said now (a bracket or a last clause
    dropped), or left out; the whole of it waits in the second list."""
    now, later = [], []
    for s in sentences(text):
        hits = future_dates(s, year0)
        if not hits:
            now.append(s)
            continue
        kept, waiting = [], []
        for piece in re.split(r";\s+", s):
            if not future_dates(piece, year0):
                kept.append(piece)
                continue
            k = _cut_piece(piece, year0)
            if k:
                kept.append(k)
            waiting.append(piece)
        if kept:
            now.append(stop(cap(balance("; ".join(x.rstrip(".") for x in kept)))))
            for piece in waiting:
                later.append((max(h[2] for h in future_dates(piece, year0)), stop(cap(balance(piece)))))
        else:
            later.append((max(h[2] for h in hits), stop(cap(balance(s)))))
    return " ".join(now), later


# ------------------------------------------------------------------------------------------------ the data's own notes, made readable

_DEV_PAREN = re.compile(r"\s*\((?:[^()]|\([^()]*\))*?(?:\.json|\.md\b|dossier|https?://|scratchpad|\.txt|\.cs\b|\.asset|"
                        r"unlock_|flag |chain_|spine|thread |class table|wikipedia|[a-z]{3,}_[a-z]+|"
                        r"\b\w+\.(?:nl|org|com|no|dk|de|se|uk|fr)\b)(?:[^()]|\([^()]*\))*?\)", re.I)
_DEV_SENTENCE = re.compile(r"\bspine\b|spine's|\bchain_|\bchains?\b(?= '|:)|\bflags?\b|unlock_|\bhooks?\b|dossier|\.json|"
                           r"\.md\b|CHAINS|BRIEF|\brepo\b|\bnpc\b|\bcard\b|\bcards\b|Design/|Research/|Plans/|\(D\)|"
                           r"\.asset\b|ShipClass|\bKapman\b|[a-z]{3,}_[a-z]{2,}_?[a-z]*\b(?!-)|the writers? of", re.I)
# a note written for the makers, not for the player: such a sentence is left off a page
_MAKERS = re.compile(r"first release|twelve ports|fate page|\(fate\b|\bsection \d|\.json|\.md\b|the engine|this file|"
                     r"\bme\.[a-z]+|\bport\.[a-z]+ ?[!=]|check=|\bflags?\b|in this life|THE HARD CASE|the player\b|"
                     r"kin numbers|each entry|entries here|words and lines here|[Ww]ords given here|a prime-signed player|"
                     r"VERY LITTLE|What is honest|No whole line|No words are given|in the credits|marked 'kin'|marked as such|"
                     r"no line turns|counts as making|economy design|STARTING GUESS")
ID_TOKEN = [None]     # set in read_world: a port's id used as a prefix ('wijnaldum.bernrad')


def clean_note(text):
    """A source note of the data as a page can print it: without file names, web addresses and code in brackets;
    a sentence that speaks only of the project's own files is left out."""
    t = norm(text)
    t = _DEV_PAREN.sub("", t)
    t = re.sub(r"\s*\[[^\[\]]{1,40}\]", "", t)                             # [S29], [VSM-Lang]
    t = re.sub(r",?\s*(?:by |at |see |as given at )?https?://\S+", "", t)    # web addresses
    t = re.sub(r"\s+([,.;:])", r"\1", t)
    t = re.sub(r"\(\s*\)", "", t)
    out = []
    for s in sentences(t):
        if _DEV_SENTENCE.search(s) or (ID_TOKEN[0] is not None and ID_TOKEN[0].search(s)):
            continue
        if len(re.findall(r"[^\W\d_]{2,}", s)) < 3 or re.match(r"^[Ss]ee \(", s):
            continue
        out.append(stop(cap(balance(s))))
    return " ".join(out)


def player_text(text):
    """A note of the data without the sentences that speak to the makers (the engine, this file, the first release)."""
    return " ".join(s for s in sentences(text) if not _MAKERS.search(s) and not (ID_TOKEN[0] is not None and ID_TOKEN[0].search(s)))


# ------------------------------------------------------------------------------------------------ pages and names

pages = []
by_id = {}
wants = []        # (name, page id, rank): 0 the title, 1 another form of it, 2 a head word, 3 a weak alias
order_of = {}


def add_page(p, names=(), weak=()):
    """Adds a page; `names` are forms of its title, `weak` are aliases that give way to any title."""
    pid = p["id"]
    if pid in by_id:
        remarks.append("two things in the data want the page id %s: the second is left out" % pid)
        return None
    for key in ("hint", "brief", "source"):
        p[key] = norm(p.get(key, ""))
    for key in ("game", "known"):                              # (these two may hold paragraphs, one a line)
        p[key] = "\n".join(norm(x) for x in (p.get(key) or "").split("\n") if norm(x))
    p.setdefault("links", [])
    p.setdefault("later", [])
    p["later"] = [x for x in p["later"] if x.get("text")]
    order_of[pid] = len(pages)
    pages.append(p)
    by_id[pid] = p
    want(p["title"], pid, 0)
    for n in names:
        want(n, pid, 1)
    for n in weak:
        want(n, pid, 3)
    return p


def want(name, pid, rank):
    name = norm(name)
    if len(name) >= 2 and any(c.isalpha() for c in name):
        wants.append((name, pid, rank))


def later_items(pairs, part):
    return [{"date": d, "part": part, "text": t} for d, t in pairs if t]


def cap_low_forms(name):
    """A common noun as it may stand in a sentence: 'Wheat' and 'wheat'."""
    out = [name]
    if name[:1].isupper() and not any(c.isupper() for c in name.split(" ")[0][1:]):
        out.append(name[0].lower() + name[1:])
    elif name[:1].islower():
        out.append(name[0].upper() + name[1:])
    return out


def in_years(frm, to, a=START_YEAR, b=LIFE_END):
    """Does a thing that lasts from `frm` to `to` (0: no bound) touch the years a to b?"""
    frm = frm or 0
    to = to or 0
    return frm <= b and (to == 0 or to >= a)


# ------------------------------------------------------------------------------------------------ the tables every kind leans on

class World(object):
    pass


W = World()

REALM_WORDS = {
    "franks": "among the Franks", "mercia": "among the Mercians", "wessex": "among the West Saxons",
    "eastanglia": "among the East Angles", "northumbria": "among the Northumbrians", "kent": "in Kent",
    "dalriata": "in Dal Riata", "irish": "among the Irish", "danes": "among the Danes",
    "obodrites": "among the Obodrites", "norse_vestfold": "in Vestfold", "norse_west": "on the west coast of the Northmen",
    "svear": "among the Svear", "gutes": "on Gotland", "prussians": "among the Prussians",
    "curonians": "among the Curonians", "rus": "at Ladoga",
}
PORT_KIND = {"emporium": "market", "market": "market", "town": "town", "monastery": "monastery",
             "royal seat": "king's seat", "fort": "fort", "village": "village"}
LANDING = {"beach": "Ships are run up on the beach.", "jetty": "Ships lie at jetties.", "quay": "Ships lie at a quay.",
           "riverbank": "Ships lie along the river bank."}
SITE_KIND = {"quay": "landing", "toll": "toll place", "tavern": "ale-house", "thing": "meeting place",
             "saltern": "salt works", "shipyard": "boat yard", "mound": "mound", "grove": "holy wood"}
ROLE_WORDS = {"tollman": "toll man", "lords_man": "lord's man", "kings_man": "king's man",
              "harbour_master": "harbour master", "slave_dealer": "dealer in people", "farm_wife": "farm wife",
              "wise_woman": "wise woman", "rune_carver": "rune carver", "guest_master": "guest master",
              "raider_chief": "raider chief"}
TONGUE_SHORT = {"OE": "Old English", "ON": "Old Norse", "OFris.": "Old Frisian", "OFris": "Old Frisian",
                "Ar.": "Arabic", "Ar": "Arabic", "OS": "Old Saxon"}
ATTESTED = {
    "period": "A law or a text of the time says so.",
    "later_law": "It was written down after 805, but describes older custom.",
    "later": "It rests on a source written long after: say 'it is said'.",
    "computed": "The day is reckoned by rule; no record gives it.",
    "game": "This is the game's own rule, not the record's.",
}


def realm_phrase(realm):
    if realm in REALM_WORDS:
        return REALM_WORDS[realm]
    name = cap((realm or "").replace("_", " "))
    if not name:
        return ""
    return ("among the " if name.endswith("s") else "in ") + name


def port_where(p):
    """'among the Danes', 'in Kent'; under the Frankish king, by the people of the place (wording_793.json):
    'among the Frisians' for Dorestat."""
    realm = p.get("realm", "")
    if realm == "franks":
        culture = W.culture_port.get(p.get("id")) or W.culture_region.get(p.get("region"))
        if culture and culture != "frankish" and culture in W.people_words:
            return "among " + low_article(people_title(culture))
    return realm_phrase(realm)


def region_label(rid):
    label = (W.regions.get(rid) or rid.replace("_", " ")).split(":")[0]
    return low_article(strip_paren(label))


def port_title(pid):
    p = W.port.get(pid)
    return p["name"] if p else cap(pid.replace("_", " "))


def good_title(gid):
    g = W.good.get(gid)
    return strip_paren(g["name"]) if g else gid.replace("_", " ")


def low_good(title):
    """A good's name as it stands inside a sentence: 'olive oil', 'ship's bread', but 'Rhenish wine'."""
    title = norm(title)
    w = title.split(" ")[0] if title else ""
    if not w or not is_common(w, SMALL):
        return title
    return title[0].lower() + title[1:]


def goods_words(ids, most=5):
    return listing([low_good(good_title(g)) for g in ids[:most]])


def with_port_names(text):
    """A note of the data that names ports by their ids ('omen at eoforwic') with their names put in."""
    def one(m):
        return port_title(m.group(0))
    ids = [p for p in W.port if len(p) >= 4 and p != port_title(p)]
    if not ids:
        return text
    return re.sub(r"(?<![\w.])(?:%s)(?![\w.])" % "|".join(sorted((re.escape(i) for i in ids), key=len, reverse=True)), one, text)


def expand_tongue_marks(s):
    def one(m):
        parts = [TONGUE_SHORT.get(x.strip(), x.strip()) for x in m.group(1).split(",")]
        return "(" + ", ".join(parts) + ")"
    return re.sub(r"\(((?:OE|ON|OFris\.?|Ar\.?|OS)(?:,\s*(?:OE|ON|OFris\.?|Ar\.?|OS))*)\)", one, s)


def read_world():
    """Reads the tables the kinds share, each guarded: a missing one leaves an empty table."""
    d = load(os.path.join(DATA, "ports_793.json")) or {}
    W.ports = [p for p in d.get("ports", []) if isinstance(p, dict) and p.get("id")]
    W.port = {p["id"]: p for p in W.ports}

    d = load(os.path.join(DATA, "goods_793.json")) or {}
    W.goods = [g for g in d.get("goods", []) if isinstance(g, dict) and g.get("id")]
    W.good = {g["id"]: g for g in W.goods}
    W.regions = d.get("regions", {}) or {}
    W.good_sources = d.get("sources", {}) or {}

    d = load(os.path.join(DATA, "trade_rules.json")) or {}
    W.zone_of = {}
    W.coins = {c.get("id"): c for c in d.get("coins", []) if isinstance(c, dict)}
    W.coin_list = [c for c in d.get("coins", []) if isinstance(c, dict)]
    for z in d.get("zones", []):
        for r in z.get("regions", []):
            W.zone_of[r] = z

    d = load(os.path.join(DATA, "voyage_rules.json")) or {}
    rations = d.get("rations", {}) or {}
    W.food_goods = rations.get("food_goods", []) or []
    W.drink_goods = rations.get("drink_goods", []) or []

    W.places = {}          # port id -> the port's record in its places file
    W.place_file = {}
    for path in sorted(glob.glob(os.path.join(DATA, "places", "*.json"))):
        d = load(path)
        if not d:
            continue
        for port in d.get("ports", []):
            if isinstance(port, dict) and port.get("port"):
                W.places[port["port"]] = port
                W.place_file[port["port"]] = rel(path)

    if not W.places:
        skipped.append((rel(os.path.join(DATA, "places")), "no places file could be read: no sites and no people of the ports"))

    W.spine = load(os.path.join(CONTENT, "spine.json")) or {}
    W.weight = load(os.path.join(DATA, "story", "weight_793.json")) or {}
    W.self = load(os.path.join(DATA, "story", "self_793.json")) or {}
    W.calendar = load(os.path.join(DATA, "story", "calendar_793.json")) or {}
    W.tongues = load(os.path.join(DATA, "story", "tongues_793.json")) or {}
    W.chronicle = load(os.path.join(DATA, "chronicle_793.json")) or {}
    W.texts = load(os.path.join(DATA, "texts_793.json")) or {}
    W.classes = load(os.path.join(DATA, "ship_classes_793.json")) or {}
    W.roster = load(os.path.join(DATA, "story", "ships_793.json")) or {}
    wording = load(os.path.join(DATA, "wording_793.json")) or {}
    W.culture_region = wording.get("culture_of_region", {}) or {}
    W.culture_port = wording.get("culture_of_port", {}) or {}
    acts = load(os.path.join(DATA, "story", "town_actions_793.json")) or {}
    W.act_name = {a.get("id"): a.get("name") for a in acts.get("acts", []) if isinstance(a, dict)}

    W.rulers = []          # every ruler row of Design/content/realms/*.json
    for path in sorted(glob.glob(os.path.join(CONTENT, "realms", "*.json"))):
        d = load(path)
        if not d:
            continue
        rows = d.get("rulers")
        if isinstance(rows, list):
            for r in rows:
                if isinstance(r, dict):
                    r = dict(r)
                    r["_file"] = rel(path)
                    r["_realm"] = d.get("realm", "")
                    W.rulers.append(r)

    if not W.rulers:
        skipped.append((rel(os.path.join(CONTENT, "realms")), "no realms file could be read: no ruler is named on a port's page"))
    if not os.path.isdir(os.path.join(CONTENT, "dossiers")):
        skipped.append((rel(os.path.join(CONTENT, "dossiers")), "missing: the ports get no other spellings from the dossiers"))
    if not os.path.isdir(os.path.join(CONTENT, "peoples")):
        skipped.append((rel(os.path.join(CONTENT, "peoples")), "missing: the peoples get no line for 793"))

    W.rules = load(RULES) or {}
    NEVER.clear()
    for x in W.rules.get("do_not_link", []) or []:
        x = norm(x)
        if x:
            NEVER.update((x, x[0].upper() + x[1:], x[0].lower() + x[1:]))
    W.people_words = W.rules.get("peoples", {}) or {}
    W.faith_words = W.rules.get("faiths", {}) or {}
    W.self_people = {p["id"]: p for p in W.self.get("peoples", []) if isinstance(p, dict) and p.get("id")}
    W.self_faith = {p["id"]: p for p in W.self.get("faiths", []) if isinstance(p, dict) and p.get("id")}
    W.twelve_regions = sorted({W.port[p]["region"] for p in TWELVE if p in W.port})
    W.proper = set(p["name"] for p in W.ports)
    for v in W.people_words.values():
        W.proper.update(n.split(" ")[0] for n in v.get("names", []))

    # the words the data writes small in its own running prose, to tell a common noun from a name
    prose = []

    def walk(v, key=""):
        """Gathers the data's own running prose (what a writer wrote to be read), not ids, sources or conditions."""
        if isinstance(v, dict):
            for k, x in v.items():
                walk(x, k)
        elif isinstance(v, list):
            for x in v:
                walk(x, key)
        elif isinstance(v, str) and key in ("summary", "seasons", "text", "see", "see_early", "trait", "looks", "note",
                                            "season", "what", "rule", "risk", "knows", "asks", "prices", "want", "fear"):
            prose.append(v)

    walk([W.ports, W.goods, list(W.places.values()), W.calendar, W.self.get("faiths", [])])
    blob = " \n ".join(prose)
    W.blob = blob.lower()
    # words the prose writes with a capital in the middle of a sentence: names (Rhenish, Mayen, Frisian)
    W.caps = set(re.findall(r"(?<=[a-z,;:] )[A-Z][A-Za-z\-]+", blob))
    SMALL.clear()
    SMALL.update(("a", "an"))
    SMALL.update(re.findall(r"(?<![A-Za-z\u00c0-\u024f_.:/'\-])[a-z][a-z\-]+(?![A-Za-z\u00c0-\u024f_/])", blob))
    ids = sorted((re.escape(p) for p in W.port), key=len, reverse=True)
    ID_TOKEN[0] = re.compile(r"\b(?:%s)\.[a-z0-9_.]+" % "|".join(ids + ["cast", "wt"])) if ids else None


def one_of_people(pid):
    w = W.people_words.get(pid)
    if w and w.get("one"):
        return w["one"]
    return "one of the " + cap((pid or "").replace("_", " "))


def one_of_faith(fid):
    w = W.faith_words.get(fid)
    if w and w.get("one"):
        return w["one"]
    f = W.self_faith.get(fid)
    return "of the faith called " + (f["name"] if f else (fid or "").replace("_", " "))


def people_title(pid):
    w = W.people_words.get(pid)
    if w and w.get("title"):
        return w["title"]
    return "The " + cap((pid or "").replace("_", " "))


def faith_title(fid):
    f = W.self_faith.get(fid)
    return f["name"] if f else cap((fid or "").replace("_", " "))


def ruler_in_793(port):
    """Who rules a port in 793, by Design/content/realms: the rows that name the port, else those of its realm."""
    pid, realm = port["id"], port.get("realm", "")
    found = []
    for r in W.rulers:
        frm, to = r.get("from"), r.get("to")
        if not (isinstance(frm, (int, float)) and isinstance(to, (int, float)) and frm <= START_YEAR <= (to or 9999)):
            continue
        over = r.get("over") if isinstance(r.get("over"), list) else []
        if pid in over and r.get("line") in ("king", None):
            found.insert(0, r)
        elif r.get("realm") == realm or r.get("part") == realm or (not over and r.get("_realm") == realm):
            found.append(r)
    for r in found:
        name = norm(r.get("name", ""))
        if name and not name.startswith("(") and "no " not in name.lower()[:4] and not name.lower().startswith("not "):
            return r
    return None


# ------------------------------------------------------------------------------------------------ ports

def dossier_names(pid):
    """The names in a dossier's first line: '# Dorestad (Dorestat, Wijk bij Duurstede): dossier'."""
    path = os.path.join(CONTENT, "dossiers", pid + ".md")
    if not os.path.isfile(path):
        return []
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            head = f.readline().strip()
    except Exception:
        return []
    head = head.lstrip("# ").strip()
    head = re.sub(r"^Dossier:\s*", "", head)
    head = re.sub(r":\s*dossier\s*$", "", head)
    m = re.match(r"^([^()]+?)\s*(?:\(([^()]*)\))?$", head)
    if not m:
        return []
    out = [norm(m.group(1))]
    if m.group(2):
        first = norm(m.group(2).split(",")[0])
        if first[:1].isupper():
            out.append(first)
    return out


def port_names(p):
    title = p["name"]
    out = []
    cid = cap(p["id"].replace("_", " "))
    if len(cid) >= 4:
        out.append(cid)
    modern = norm(p.get("modern", ""))
    head = norm(re.split(r"[,(]", modern)[0])
    if head[:1].isupper():
        out.append(head)
    for m in re.finditer(r"\(([^()]+)\)", modern):
        inner = norm(m.group(1))
        if inner[:1].isupper() and " " not in inner:
            out.append(inner)
    out += dossier_names(p["id"])
    out += [fold(x) for x in [title] + out]
    seen, clean = {title}, []
    for n in out:
        if len(n) >= 3 and n not in seen:
            seen.add(n)
            clean.append(n)
    return clean


def port_trade_lines(p):
    """What the port's country makes and wants (goods_793.json), and its coin and toll (trade_rules.json)."""
    region = p.get("region", "")
    made, wanted = [], []
    for g in W.goods:
        if not in_years(g.get("from"), g.get("to"), START_YEAR, START_YEAR):
            continue
        m = (g.get("made") or {}).get(region, 0)
        w = (g.get("wanted") or {}).get(region, 0)
        if m >= 2:
            made.append((-m, len(made), g["id"]))
        if w >= 2:
            wanted.append((-w, len(wanted), g["id"]))
    out = []
    if made or wanted:
        bits = []
        if made:
            bits.append("made in its country, and so cheaper here: " + goods_words([x[2] for x in sorted(made)]))
        if wanted:
            bits.append("wanted here, and so dearer: " + goods_words([x[2] for x in sorted(wanted)]))
        out.append(stop(cap("; ".join(bits))))
    z = W.zone_of.get(region)
    if z:
        coin = W.coins.get(z.get("coin"), {}).get("name", "silver")
        toll = z.get("toll", 0) or 0
        if abs(toll - 0.1) < 1e-6:
            t = "the toll on what you sell is a tenth"
        elif abs(toll - 0.05) < 1e-6:
            t = "the toll on what you sell is a twentieth"
        elif toll <= 0:
            t = "no toll is taken on what you sell"
        else:
            t = "the toll on what you sell is %s in the hundred" % number(toll * 100)
        if z.get("coin") == "hacksilver":
            out.append("Silver goes by weight here, and %s." % t)
        else:
            out.append("Prices are in %s, and %s." % (coin, t))
    return out


def history_line(h):
    year, text = h.get("year"), norm(h.get("text", ""))
    if not text:
        return ""
    y = str(year)
    if text.startswith("This year "):
        return stop("In %s %s" % (y, text[len("This year "):]))
    if text.startswith("About this year "):
        return stop("About %s %s" % (y, text[len("About this year "):]))
    if text.startswith("In these years "):
        return stop("About %s %s" % (y, text[len("In these years "):]))
    if text.startswith("On "):
        return stop("In %s, on %s" % (y, text[3:]))
    return stop("%s: %s" % (y, text))


def build_ports():
    for p in W.ports:
        pid = p["id"]
        page_id = "port." + pid
        place = W.places.get(pid) or {}
        title = p["name"]
        kind_word = PORT_KIND.get(p.get("kind", ""), p.get("kind", "place") or "place")
        where = port_where(p)
        later = []

        # in the game: the places file's own summary and seasons, then the trade and the toll by rule
        game = []
        summary = " ".join(s for s in sentences(place.get("summary", "")) if not re.search(r"\byour home\b", s))
        now, lat = gate(summary)
        later += later_items(lat, "game")
        if now:
            game.append(now)
        else:
            game.append("%s is %s %s.%s" % (title, a_an(kind_word), where,
                                            (" " + LANDING[p["landing"]]) if p.get("landing") in LANDING else ""))
        now, lat = gate(place.get("seasons", ""))
        later += later_items(lat, "game")
        if now:
            game.append(now)
        trade = port_trade_lines(p)
        if trade:
            game.append(" ".join(trade))
        for era in place.get("eras", []) or []:
            frm, to, text = era.get("from") or 0, era.get("to") or 0, norm(era.get("text", ""))
            if not text:
                continue
            if frm <= START_YEAR <= (to or 9999):
                now, lat = gate(text)
                later += later_items(lat, "game")
                if now:
                    game.append("In these years: " + low_first(now) if not now[:1].isdigit() else now)
            elif frm > START_YEAR:
                span = "%d to %d" % (frm, to) if to and to != frm else "%d" % frm
                later.append({"date": iso(frm, 1, 1), "part": "game", "text": stop("From %s: %s" % (span, low_first(text)))})

        # what is known: the port table's note, the place today, who rules, the dated history up to the first year
        known, sources = [], []
        now, lat = gate(p.get("note", ""))
        later += later_items(lat, "known")
        note_now = now
        if now:
            known.append(now)
        modern = norm(p.get("modern", ""))
        if modern:
            known.append(("The place is now %s." if modern[:1].isupper() else "The place today: %s.") % modern.rstrip("."))
        r = ruler_in_793(p)
        if r:
            rname = strip_paren(norm(r.get("name", "")))
            rtitle, lat = gate(norm(r.get("title", "")))
            later += later_items(lat, "known")
            known.append(stop("In 793 it is under %s%s" % (rname, ", " + low_first(rtitle.rstrip(".")) if rtitle else "")))
            if r.get("source"):
                sources.append(norm(r["source"]) + " (" + r["_file"] + ")")
        frm, to = p.get("from") or 0, p.get("to") or 0
        if frm and frm <= START_YEAR:
            known.append("It has been a trading place since about %d." % frm)
        elif frm > START_YEAR:
            later.append({"date": iso(frm, 1, 1), "part": "known", "text": "It is a trading place from about %d." % frm})
        if to:
            later.append({"date": iso(to), "part": "known", "text": "As a trading place it ends about %d." % to})
        known = [" ".join(known)]
        past = []
        for h in place.get("history", []) or []:
            line = history_line(h)
            if not line:
                continue
            year = h.get("year") or 0
            if year <= START_YEAR:
                now, lat = gate(line)
                later += later_items(lat, "known")
                if now:
                    past.append(now)
                if h.get("source"):
                    sources.append(norm(h["source"]))
            else:
                later.append({"date": iso(int(year)), "part": "known", "text": line})
        if past:
            known.append(" ".join(past))
        srcs = [norm(s) for s in (place.get("sources") or [])[:1]] + sources
        if not srcs:
            srcs = ["Assets/Kapman/Game/Data/ports_793.json"]
        seen, source = set(), []
        for s in srcs:
            if s and s not in seen:
                seen.add(s)
                source.append(s)

        sites = ["site." + s["id"] for s in place.get("sites", []) or [] if s.get("id")]
        people = ["person." + s["id"] for s in place.get("people", []) or [] if s.get("id")]
        add_page({
            "id": page_id, "kind": "port", "title": title,
            "hint": cap("%s %s" % (a_an(kind_word), where)),
            "brief": first_sentences(note_now or game[0]),
            "game": "\n".join(game), "known": "\n".join(known), "source": "; ".join(source),
            "links": sites + people,
            "opens": "port_heard:" + pid,
            "first_release": pid in TWELVE,
            "later": later,
        }, names=port_names(p))


# ------------------------------------------------------------------------------------------------ sites and people of the ports

def port_source(pid):
    place = W.places.get(pid) or {}
    srcs = place.get("sources") or []
    return norm(srcs[0]) if srcs else W.place_file.get(pid, "")


def site_kind_word(kind):
    return SITE_KIND.get(kind, (kind or "place").replace("_", " "))


def role_word(role):
    return ROLE_WORDS.get(role, (role or "person").replace("_", " "))


def distinct_site_name(name):
    """Is a site's name its own (a capital after the first word), not 'The mint' or 'The market', and not a
    bare 'Saint Paul's', which stands in many ports?"""
    words = name.split(" ")
    if re.match(r"^(?:Saints?|St) \S+(?: and (?:Saint )?\S+)?(?: (?:church|minster|abbey))?$", name):
        return False
    return len(words) >= 2 and any(w[:1].isupper() for w in words[1:])


def build_sites():
    counts = collections.Counter(norm(s.get("name", "")) for place in W.places.values() for s in place.get("sites", []) or [])
    for pid, place in sorted(W.places.items()):
        ptitle = port_title(pid)
        people_here = {x.get("id"): x for x in place.get("people", []) or []}
        for s in place.get("sites", []) or []:
            sid = s.get("id")
            name = norm(s.get("name", ""))
            if not sid or not name:
                continue
            kind_word = site_kind_word(s.get("kind"))
            frm, to = s.get("from") or 0, s.get("to") or 0
            later = []
            see, lat = gate(s.get("see", ""))
            later += later_items(lat, "game")
            game = ["%s at %s." % (cap(a_an(kind_word)), ptitle)]
            early = norm(s.get("see_early", ""))
            if early:
                e_now, e_lat = gate(early)
                later += later_items(e_lat, "game")
                later.append({"date": iso(800, 1, 1), "part": "game", "text": see})
                if e_now:
                    game.append(e_now)
            elif see:
                game.append(see)
            acts, persons, links = [], [], ["port." + pid]
            for d in s.get("do", []) or []:
                k, _, v = str(d).partition(":")
                if k == "act":
                    act = norm(W.act_name.get(v) or v.replace("_", " "))
                    acts.append(act[:1].lower() + act[1:] if act[1:2].islower() else act)
                elif k == "person":
                    links.append("person." + v)
                    who = people_here.get(v)
                    if who and in_years(who.get("from"), who.get("to"), max(frm, START_YEAR), max(frm, START_YEAR) + 12):
                        persons.append(norm(who.get("name", "")))
            if acts:
                shown = acts[:8]
                game.append(stop("What is done here: " + listing(shown) + (", and more" if len(acts) > 8 else "")))
            if persons:
                game.append(stop("Found here: " + listing(persons)))
            known, lat = gate(clean_note(s.get("source", "")))
            later += later_items(lat, "known")
            if to and to < 1066:
                later.append({"date": iso(to), "part": "known", "text": "It stands until about %d." % to})
            opens = "port_seen:" + pid
            if frm > START_YEAR:
                opens += "&date:" + iso(frm, 1, 1)
            title = name if counts[name] == 1 else "%s, %s" % (name, ptitle)
            names = []
            if counts[name] == 1 and distinct_site_name(name):
                names = cap_low_forms(name)[1:] if name.startswith(("The ", "A ", "An ")) else []
            page = add_page({
                "id": "site." + sid, "kind": "site", "title": title,
                "hint": cap(a_an(kind_word)) + ", not yet seen",
                "brief": first_sentences("%s at %s. %s" % (cap(a_an(kind_word)), ptitle, early or see), 90),
                "game": " ".join(game), "known": known, "source": port_source(pid),
                "links": links, "opens": opens,
                "first_release": pid in TWELVE and in_years(frm, to),
                "later": later,
            }, names=names)
            if page is not None and not (counts[name] == 1 and distinct_site_name(name)):
                # a plain name ('The mint') must not link from everywhere: only its full title with the port does
                wants[:] = [w for w in wants if not (w[1] == page["id"] and w[0] == name)]


def gate_segments(text, sep=";"):
    """A line of dated segments ('born 748 (?); king 768; died 28 January 814'): those of days come, and the rest."""
    now, later = [], []
    for seg in [norm(x) for x in (text or "").split(sep)]:
        if not seg:
            continue
        hits = future_dates(seg, START_YEAR)
        if hits:
            later.append((max(h[2] for h in hits), stop(cap(seg))))
        else:
            now.append(seg)
    return now, later


def build_people_of_ports():
    cast = {c.get("id"): c for c in W.spine.get("cast", []) or [] if isinstance(c, dict) and c.get("id")}
    weight = {w.get("id"): w for w in W.weight.get("people", []) or [] if isinstance(w, dict) and w.get("id")}
    cast_by_place = {c.get("place_id"): c for c in cast.values() if c.get("place_id")}
    weight_by_place = {w.get("place_id"): w for w in weight.values() if w.get("place_id")}
    weight_by_cast = {w.get("cast"): w for w in weight.values() if w.get("cast")}
    tempers = W.weight.get("tempers", {}) or {}
    used_cast, used_weight = set(), set()

    name_count = collections.Counter()
    for place in W.places.values():
        for x in place.get("people", []) or []:
            name_count[norm(x.get("name", ""))] += 1

    def weight_lines(w, name):
        """The lines a person of weight adds: the office, the seat, the reach, the temper at the start."""
        game, known, later = [], [], []
        now, lat = gate_segments(w.get("office", ""))
        later += later_items(lat, "game")
        if now:
            game.append(stop("%s is %s" % (name, "; ".join(now))))
        seat = w.get("seat")
        if seat and seat in W.port:
            note, lat = gate_segments(w.get("seat_note", ""))
            later += later_items(lat, "game")
            game.append(stop("The seat nearest you is %s%s" % (port_title(seat), (": " + "; ".join(note)) if note else "")))
        reach = norm(w.get("reach", ""))
        if reach.startswith("twelve"):
            game.append("You may meet this person in one of the twelve ports.")
        elif reach.startswith("dim"):
            game.append("This person sits in a port beyond the twelve.")
        elif reach.startswith("word"):
            game.append("You know this person only by word.")
        temper = w.get("temper") or {}
        if temper.get("start") and temper["start"] in tempers:
            game.append(stop("Temper at the start: %s. That is: %s" % (temper["start"], tempers[temper["start"]])))
        for mv in temper.get("moves", []) or []:
            when, to, by = str(mv.get("when", "")), mv.get("to", ""), norm(mv.get("by", ""))
            parts = when.split("-")
            try:
                date = iso(int(parts[0]), int(parts[1]) if len(parts) > 1 else 12, int(parts[2]) if len(parts) > 2 else 28)
            except (ValueError, IndexError):
                continue
            later.append({"date": date, "part": "game", "text": stop("%s: %s. The temper turns to %s" % (date_words(when), by, to))})
        now, lat = gate_segments(w.get("dates", ""))
        later += later_items(lat, "known")
        if now:
            known.append(stop(cap("; ".join(now))))
        if norm(temper.get("why", "")).lower().startswith("invented"):
            known.append("The temper is the game's own reading.")
        elif temper.get("why"):
            why, lat = gate(temper["why"])
            later += later_items(lat, "known")
            if why:
                known.append("Why that temper: " + low_first(why))
        return game, known, later

    # the people of the places files, each with what the cast and the people of weight add
    for pid, place in sorted(W.places.items()):
        ptitle = port_title(pid)
        folk = place.get("people", []) or []
        sites = place.get("sites", []) or []
        for x in folk:
            xid, name = x.get("id"), norm(x.get("name", ""))
            if not xid or not name:
                continue
            c = cast_by_place.get(xid)
            w = weight_by_place.get(xid) or (weight_by_cast.get(c.get("id")) if c else None)
            if c:
                used_cast.add(c["id"])
            if w:
                used_weight.add(w["id"])
            role = role_word(x.get("role"))
            frm, to = x.get("from") or 0, x.get("to") or 0
            same = [y for y in folk if y.get("role") == x.get("role") and in_years(y.get("from"), y.get("to"), frm or START_YEAR, to or 9999)]
            art = "the " + role if len(same) <= 1 else a_an(role)
            later = []
            who = [one_of_people(x.get("people"))] if x.get("people") else []
            if x.get("faith"):
                who.append(one_of_faith(x.get("faith")))
            game = [stop(cap("%s is %s at %s%s" % (name, art, ptitle, (", " + listing(who)) if who else "")))]
            at = [norm(s.get("name", "")) for s in sites if ("person:" + xid) in (s.get("do") or [])]
            if at:
                game.append(stop("Found at: " + listing([low_article(n) for n in at[:3]])))
            trait, lat = gate(x.get("trait", ""))
            later += later_items(lat, "game")
            if trait:
                game.append(trait)
            looks, lat = gate(x.get("looks", ""))
            later += later_items(lat, "game")
            if looks:
                game.append("To look at: " + low_plain(looks))
            known = []
            historical = bool(x.get("historical"))
            if historical:
                known.append("A real person.")
                if (x.get("born") or 0) > 0 and x["born"] <= START_YEAR:
                    known.append("Born about %d." % x["born"])
                if (x.get("died") or 0) > 0:
                    later.append({"date": iso(int(x["died"])), "part": "known", "text": "Died in %d." % x["died"]})
            else:
                known.append("Not in the record: the game's own person, set in a real place.")
            note = re.sub(r"^\s*[Ii]nvented(?:\s*\([^)]*\))?\s*[;:.,]?\s*", "", norm(x.get("source", "")))
            note, lat = gate(clean_note(note))
            later += later_items(lat, "known")
            if note:
                known.append(note)
            if w:
                wg, wk, wl = weight_lines(w, name)
                game[1:1] = wg[:1]
                game += [g for g in wg[1:] if g.startswith("Temper at the start")]
                known += wk
                later += wl
            links = ["port." + pid] + ["site." + s["id"] for s in sites if ("person:" + xid) in (s.get("do") or []) and s.get("id")]
            title = name if name_count[name] == 1 else "%s of %s" % (name, ptitle)
            opens = "person_met:" + xid
            add_page({
                "id": "person." + xid, "kind": "person", "title": title,
                "hint": cap(a_an(role)) + ", not yet met",
                "brief": first_sentences("%s, %s at %s. %s" % (cap(name), art, ptitle, trait), 70),
                "game": " ".join(game), "known": " ".join(known), "source": port_source(pid),
                "links": links, "opens": opens,
                "first_release": pid in TWELVE and in_years(frm, to),
                "later": later,
            }, names=[name] if title != name else [])

    # the cast that stands in no places file
    for cid, c in cast.items():
        if cid in used_cast:
            continue
        name = norm(c.get("name", ""))
        if not name:
            continue
        w = weight_by_cast.get(cid)
        if w:
            used_weight.add(w["id"])
        home = c.get("home")
        role = role_word(c.get("role"))
        later = []
        who = [one_of_people(c.get("people"))] if c.get("people") else []
        if c.get("faith"):
            who.append(one_of_faith(c.get("faith")))
        where = (" at " + port_title(home)) if home in W.port else ""
        game = [stop("%s is %s%s%s" % (name, a_an(role), where, (", " + listing(who)) if who else ""))]
        trait, lat = gate(c.get("trait", ""))
        later += later_items(lat, "game")
        if trait:
            game.append(trait)
        looks, lat = gate(c.get("looks", ""))
        later += later_items(lat, "game")
        if looks:
            game.append("To look at: " + low_plain(looks))
        known = ["A real person." if c.get("historical") else "Not in the record: the game's own person."]
        now, lat = gate_segments(c.get("dates", ""))
        later += later_items(lat, "known")
        if now and c.get("historical"):
            known.append(stop(cap("; ".join(now))))
        note = re.sub(r"^\s*[Ii]nvented(?:\s*\([^)]*\))?\s*[;:.,]?\s*", "", norm(c.get("source", "")))
        note, lat = gate(clean_note(note))
        later += later_items(lat, "known")
        if note:
            known.append(note)
        if w:
            wg, wk, wl = weight_lines(w, name)
            game[1:1] = wg[:1]
            known += wk
            later += wl
        met = [m for m in (c.get("met") or []) if m in W.port]
        add_page({
            "id": "person." + cid, "kind": "person", "title": name,
            "hint": cap(a_an(role)) + ", not yet met",
            "brief": first_sentences("%s, %s%s. %s" % (name, a_an(role), where, trait), 70),
            "game": " ".join(game), "known": " ".join(known),
            "source": "Design/content/spine.json (the cast): " + norm(c.get("source", "")),
            "links": (["port." + home] if home in W.port else []) + ["port." + m for m in met],
            "opens": "person_met:" + cid,
            "first_release": (home in TWELVE or any(m in TWELVE for m in met)) and in_years(c.get("from"), c.get("to")),
            "later": later,
        })

    # the people of weight who stand nowhere else
    for wid, w in weight.items():
        if wid in used_weight:
            continue
        name = norm(w.get("name", ""))
        if not name:
            continue
        game, known, later = weight_lines(w, name)
        traits, lat = gate(w.get("traits", ""))
        later += later_items(lat, "game")
        if traits:
            game.insert(1, traits)
        known.insert(0, "A real person." if w.get("historical") else "Not in the record: the game's own person in a real office.")
        office_now, _ = gate_segments(w.get("office", ""))
        seat = w.get("seat")
        add_page({
            "id": "person." + wid, "kind": "person", "title": name,
            "hint": cap(office_now[0]) if office_now else "A person of weight",
            "brief": first_sentences("%s, %s. %s" % (name, office_now[0] if office_now else "a person of weight", traits), 70),
            "game": " ".join(game), "known": " ".join(known),
            "source": norm(w.get("source", "")),
            "links": ["port." + seat] if seat in W.port else [],
            "opens": "person_met:" + wid,
            "first_release": True,
            "later": later,
        })


# ------------------------------------------------------------------------------------------------ goods

def keeps_words(days):
    if not days:
        return "This good does not spoil."
    if days >= 330:
        halves = int(round(days / 182.5))
        words = {2: "a year", 3: "a year and a half", 4: "two years", 5: "two years and a half", 6: "three years"}
        return "This good keeps about %s." % words.get(halves, "%s years" % number(round(days / 365.0)))
    if 170 <= days <= 190:
        return "This good spoils in about half a year."
    if 25 <= days <= 35:
        return "This good spoils in about a month."
    return "This good spoils in about %s days." % number(days)


def region_ports(regions, most=4):
    """The ports of some trade regions, those of the first release first."""
    found = [p["id"] for p in W.ports if p.get("region") in regions and in_years(p.get("from"), p.get("to"), START_YEAR, START_YEAR)]
    found.sort(key=lambda x: (x not in TWELVE, regions.index(W.port[x]["region"])))
    return found[:most]


def plural_like(name):
    last = name.split(" ")[-1]
    return " and " in name or (last.endswith("s") and not last.endswith(("ss", "us", "is")))


def build_goods():
    for g in W.goods:
        gid = g["id"]
        name = norm(g.get("name", gid))
        title = strip_paren(name)
        cat = norm(g.get("cat", ""))
        later, game, links = [], [], []
        if cat and cat.lower() != title.lower():
            game.append("%s %s counted among %s." % (title, "are" if plural_like(title) else "is", cat.lower()))
        unit = norm(g.get("unit", ""))
        kg = g.get("kg")
        if unit:
            first = unit.split(" ")[0].rstrip(",")
            u = unit if first.endswith("s") else a_an(unit)
            game.append(stop("One unit is %s%s" % (u, (", of %s kg" % number(kg)) if kg else "")))
        bulk = g.get("bulk") or 0
        if bulk >= 2:
            game.append("This good is light and bulky: a unit takes about %s times its weight in room." % number(bulk))
        elif 0 < bulk <= 0.8:
            game.append("This good is dense: a unit takes less room than its weight.")
        if g.get("value"):
            game.append("Where it is neither made nor wanted, a unit is worth about %s pennies." % number(g["value"]))
        if (g.get("keeps") or 0) or cat not in ("Livestock", "Captives"):
            game.append(keeps_words(g.get("keeps") or 0))
        made = sorted((g.get("made") or {}).items(), key=lambda kv: -kv[1])
        wanted = sorted((g.get("wanted") or {}).items(), key=lambda kv: -kv[1])
        if made:
            top = [r for r, _ in made[:4]]
            ports = region_ports(top)
            links += ["port." + p for p in ports]
            game.append(stop("Where it is made: %s%s" % (listing([region_label(r) for r, _ in made[:4]]),
                                                          ("; ports there: " + listing([port_title(p) for p in ports])) if ports else "")))
        if wanted:
            top = [r for r, _ in wanted[:4]]
            ports = region_ports(top)
            links += ["port." + p for p in ports]
            game.append(stop("Where it is wanted: %s%s" % (listing([region_label(r) for r, _ in wanted[:4]]),
                                                            ("; ports there: " + listing([port_title(p) for p in ports])) if ports else "")))
        season, lat = gate(g.get("season", ""))
        later += later_items(lat, "game")
        if season:
            game.append(stop(season))
        risk, lat = gate(g.get("risk", ""))
        later += later_items(lat, "game")
        if risk:
            game.append(risk)
        if gid in W.food_goods:
            game.append("This is one of the foods the crew eat aboard.")
        if gid in W.drink_goods:
            game.append("This is one of the drinks the crew take aboard.")
        frm, to = g.get("from") or 0, g.get("to") or 0
        known = []
        note, lat = gate(player_text(g.get("note", "")))
        later += later_items(lat, "known")
        if note:
            known.append(note)
        period = norm(g.get("period", ""))
        if period:
            known.append(stop("Names of the time: " + expand_tongue_marks(period)))
        if to and to >= START_YEAR:
            later.append({"date": iso(to), "part": "known", "text": "It is no longer traded after about %d." % to})
        src = [W.good_sources.get(s, s) for s in g.get("src", []) or []]
        opens = "good_seen:" + gid
        if frm > START_YEAR:
            opens += "&date:" + iso(frm, 1, 1)
        first = in_years(frm, to) and any(r in W.twelve_regions for r in list(g.get("made") or {}) + list(g.get("wanted") or {}))
        page = add_page({
            "id": "good." + gid, "kind": "good", "title": title,
            "hint": "A good not yet seen" + ((": " + cat.lower()) if cat else ""),
            "brief": first_sentences(" ".join(game[:2]) + " " + note, 90),
            "game": " ".join(game), "known": " ".join(known),
            "source": "; ".join(src) if src else "Assets/Kapman/Game/Data/goods_793.json (no source is given for this good)",
            "links": links, "opens": opens, "first_release": bool(first), "later": later,
        })
        if page is None:
            continue
        page["_good"] = {"title": title, "id": gid, "period": period}


def good_name_forms(corpus_words):
    """The linking forms of each good, made when every page's text is known (so that a plural is only added where
    the data itself uses it)."""
    def singular(word):
        if re.search(r"(ches|shes|sses)$", word):
            return word[:-2]
        if word.endswith("s") and not word.endswith(("ss", "us", "is")) and len(word) > 3:
            return word[:-1]
        return None

    def plural(word):
        if word.endswith(("s", "x", "ch", "sh")):
            return word + "es"
        return word + "s"

    def number_forms(phrase):
        out = [phrase]
        words = phrase.split(" ")
        last = words[-1]
        s = singular(last)
        if s:
            out.append(" ".join(words[:-1] + [s]))
        else:
            pl = " ".join(words[:-1] + [plural(last)])
            if (pl.lower() in corpus_words) if len(words) == 1 else (pl.lower() in W.blob):
                out.append(pl)
        return out

    for p in pages:
        info = p.pop("_good", None)
        if not info:
            continue
        title, gid = info["title"], info["id"]
        plain = " and " not in title and "," not in title
        forms = []
        family = number_forms(title) if plain else [title]
        if any(f in NEVER for f in family):
            family = [title]                                    # (the title itself is refused by the list anyway)
        for f in family:
            forms += cap_low_forms(f) if is_common(f.split(" ")[0], corpus_words) else [f]
        for f in list(forms):
            forms.append(f + ("'" if f.endswith("s") else "'s"))
        for f in forms:
            want(f, p["id"], 1 if f != title else 0)
        if plain and " " in title:
            head = title.split(" of ")[0].split(" ")[-1] if " of " in title else title.split(" ")[-1]
            head = head.lower()
            family = number_forms(head)
            if len(head) >= 3 and head != title.lower() and not any(f in NEVER for f in family):
                for f in family:
                    for v in cap_low_forms(f):
                        want(v, p["id"], 2)
        alias = gid.replace("_", " ")
        if alias != title.lower() and all(w in corpus_words for w in alias.split(" ")) and alias not in NEVER:
            for v in cap_low_forms(alias):
                want(v, p["id"], 3)
        for piece in re.split(r",\s*(?![^()]*\))", info["period"]):
            w = strip_paren(piece).strip(" '")
            if w and "+" not in w and (len(w) >= 6 or any(ord(c) > 127 for c in w)) and w.lower() != title.lower():
                want(w, p["id"], 3)


def is_common(word, corpus_words):
    """Is a word a common noun here? It is, unless the data's prose writes it with a capital inside a sentence and
    never small (Rhenish, Mayen, Frisian), or it is a port's or a people's name."""
    word = word.strip("'")
    if word in W.proper:
        return False
    return word.lower() in corpus_words or word not in W.caps


# ------------------------------------------------------------------------------------------------ ships

def ship_title(name):
    t = norm(name)
    t = t.split(":")[0]
    t = strip_paren(t)
    t = re.sub(r",\s*\d[^,]*$", "", t)
    return norm(t)


def can_go_words(can):
    names = {"river": "the rivers", "flats": "the tidal flats", "coast": "the coast", "open_sea": "the open sea"}
    yes = [names[k] for k in ("river", "flats", "coast", "open_sea") if (can or {}).get(k) == "yes"]
    some = [names[k] for k in ("river", "flats", "coast", "open_sea") if (can or {}).get(k) == "limited"]
    no = [names[k] for k in ("river", "flats", "coast", "open_sea") if (can or {}).get(k) == "no"]
    bits = []
    if yes:
        bits.append("She can go on " + listing(yes))
    if some:
        bits.append(("on " if bits else "She can go on ") + listing(some) + " only with care")
    if no:
        bits.append("never on " + listing(no))
    return stop("; ".join(bits)) if bits else ""


def build_ships():
    roster = [s for s in W.roster.get("ships", []) or [] if isinstance(s, dict) and s.get("id")]
    classes = [s for s in W.classes.get("classes", []) or [] if isinstance(s, dict) and s.get("id")]
    owned = {s["id"] for s in roster}
    for s in roster + [c for c in classes if c["id"] not in owned]:
        sid = s["id"]
        mine = sid in owned
        title = ship_title(s.get("name", sid))
        frm, to = s.get("from") or 0, s.get("to") or 0
        game, links, later = [], [], []
        if mine:
            if s.get("what_it_is"):
                game.append(stop(s["what_it_is"]))
            if s.get("why_want_it"):
                game.append(stop(s["why_want_it"]))
            go = can_go_words(s.get("can_go"))
            if go:
                game.append(go)
            yards = [y for y in s.get("built_at", []) or [] if y in W.port]
            yards.sort(key=lambda y: y not in TWELVE)
            links += ["port." + y for y in yards[:6]]
            how = s.get("how_had", "")
            buy = []
            if s.get("cost_d"):
                buy.append("new, with her gear, she costs %s pennies" % number(s["cost_d"]))
            if s.get("build_days"):
                buy.append("takes %s days to build" % number(s["build_days"]))
            line = cap(" and ".join(buy)) if buy else ""
            if yards:
                line += (", at " if line else "She is built at ") + listing([port_title(y) for y in yards[:6]], "or")
            if line:
                game.append(stop(line))
            if how == "reward":
                game.append("She is not sold: a house comes by her only as a reward.")
            elif how == "both":
                game.append("She can be bought, and she can come as a reward.")
            if s.get("crew_min") and s.get("crew_full"):
                least = "1 man" if s["crew_min"] == 1 else "%s men" % number(s["crew_min"])
                game.append("She needs %s at the least and is meant for %s." % (least, number(s["crew_full"])))
        else:
            look = " ".join(x for x in sentences(clean_note(s.get("look", "")))
                            if not re.search(r"\bmodel\b|^Reads as|scaled down|mast_share|hull_top", x))
            if look:
                game.append(look)
            size = []
            if s.get("oars"):
                size.append("%s oars" % number(s["oars"]))
            if s.get("crew"):
                size.append("%s men" % number(s["crew"]))
            if s.get("cargo_t"):
                size.append("about %s tonnes of cargo" % number(s["cargo_t"]))
            if s.get("length_m"):
                game.append(stop("She is %s metres long%s" % (number(s["length_m"]), (", with " + listing(size)) if size else "")))
            elif size:
                game.append(stop("She has " + listing(size)))
            uses = [u.replace("_", " ") for u in s.get("uses", []) or []]
            if uses:
                game.append(stop("Used for: " + listing(uses)))
            game.append("She is other people's craft: you meet her on the water, and a house does not own her.")
        known = []
        if s.get("period_name"):
            known.append(stop("What she was called then: " + norm(s["period_name"])))
        for e in (s.get("evidence") or [])[:3]:
            e = clean_note(e)
            if e:
                known.append(e)
        doubt = [clean_note(u).rstrip(".") for u in (s.get("uncertain") or [])[:3]]
        doubt = [d for d in doubt if d]
        if doubt:
            known.append(stop("Not certain: " + "; ".join(doubt)))
        opens = "always"
        if frm > START_YEAR:
            opens = "date:" + iso(frm, 1, 1)
        if mine:
            first = in_years(frm, to) and any(y in TWELVE for y in s.get("built_at", []) or [])
        else:
            first = in_years(frm, to) and any(c in ("frisian", "frankish", "english", "danish") for c in s.get("cultures", []) or [])
        names = [low_first(title)] if low_first(title) != title else []
        add_page({
            "id": "ship." + sid, "kind": "ship", "title": title,
            "hint": "A kind of craft",
            "brief": first_sentences(game[0] if game else title, 60),
            "game": " ".join(game), "known": " ".join(known),
            "source": norm(s.get("source", "")) or "Assets/Kapman/Game/Data/ship_classes_793.json",
            "links": links, "opens": opens, "first_release": bool(first), "later": later,
        }, names=names)


# ------------------------------------------------------------------------------------------------ peoples and faiths

def md_793(pid):
    """The '793' line of a people's page in Design/content/peoples, where a file is named for the people."""
    for path in sorted(glob.glob(os.path.join(CONTENT, "peoples", "*.md"))):
        stem = os.path.splitext(os.path.basename(path))[0]
        if pid != stem and pid not in stem.split("_"):
            continue
        text = read_text(path)
        if not text:
            continue
        for line in text.splitlines():
            m = re.match(r"^-\s+\*\*793[^*]*\*\*:?\s*(.+)$", line.strip())
            if m:
                body = re.sub(r"\(`[^`]*`\)", "", m.group(1))
                body = re.sub(r"[`*]", "", body)
                body = re.sub(r"\s*\((?:ASC|ARF|RFA|AU)[^()]*\)", "", body)
                return clean_note(body), rel(path)
    return "", ""


def build_peoples():
    ids = list((W.spine.get("peoples", {}) or {}).get("ids", {}) or {})
    for pid in W.self_people:
        if pid not in ids:
            ids.append(pid)
    spine_line = (W.spine.get("peoples", {}) or {}).get("ids", {}) or {}
    tongues = {t.get("id"): t for t in W.tongues.get("tongues", []) or [] if isinstance(t, dict)}
    faith_says = {f.get("people"): f for f in (W.spine.get("gods_and_faith", {}) or {}).get("peoples", []) or [] if isinstance(f, dict)}
    seen_people = collections.Counter()
    for port, place in W.places.items():
        if port in TWELVE:
            for x in place.get("people", []) or []:
                if in_years(x.get("from"), x.get("to")):
                    seen_people[x.get("people")] += 1
    for pid in ids:
        me = W.self_people.get(pid)
        title = people_title(pid)
        later, game, known, source, links = [], [], [], [], []
        line = norm(str(spine_line.get(pid, "")).split(";")[0])
        if line:
            now, lat = gate(stop("These are " + ("" if line.startswith("the ") else "the ") + line))
            later += later_items(lat, "game")
            if now:
                game.append(now)
        tongue = None
        if me:
            home = me.get("home")
            if home in W.port:
                links.append("port." + home)
            game.append(stop("You may begin as %s%s" % (one_of_people(pid), (": your home is then " + port_title(home)) if home in W.port else "")))
            tongue = tongues.get(me.get("first_tongue"))
            half = [strip_paren(tongues[t]["name"]) for t in (me.get("half_known") or {}) if t in tongues]
            if tongue:
                game.append(stop("Their tongue is %s%s" % (strip_paren(tongue.get("name", "")),
                                                          ("; one of them half follows " + listing(half)) if half else "")))
            faiths = [low_first(faith_title(f)) for f in me.get("usual_faiths", []) or []]
            links += ["faith." + f for f in me.get("usual_faiths", []) or []]
            if faiths:
                game.append(stop("The faiths usual among them: " + "; ".join(faiths)))
            hard, lat = gate(player_text(me.get("hard", "")))
            later += later_items(lat, "game")
            if hard:
                game.append("What is hard about beginning as one of them: " + low_first(hard))
            source += [norm(s) for s in me.get("sources", []) or []]
        else:
            for t in tongues.values():
                if pid in (t.get("spoken_by") or []):
                    tongue = t
                    game.append(stop("Their tongue is %s" % strip_paren(t.get("name", ""))))
                    break
            game.append("You cannot begin as one of them in this game.")
        body, path = md_793(pid)
        if body:
            now, lat = gate(body)
            later += later_items(lat, "known")
            if now:
                known.append(now)
            source.append(path)
        says = faith_says.get(pid)
        if says and says.get("says") and not says.get("later"):
            now, lat = gate(says["says"])
            later += later_items(lat, "known")
            if now:
                known.append(now)
            if says.get("source"):
                source.append(norm(says["source"]))
        if tongue:
            if tongue.get("written_then"):
                known.append(stop("Was their tongue written then? " + cap(norm(tongue["written_then"]))))
            now, lat = gate(" ".join(sentences(player_text(tongue.get("note", "")))[:3]))
            later += later_items(lat, "known")
            if now:
                known.append(now)
        if not source:
            source.append("Design/content/spine.json (peoples)")
        first = bool(me and me.get("in_first_release")) or seen_people.get(pid, 0) > 0
        add_page({
            "id": "people." + pid, "kind": "people", "title": title,
            "hint": title,
            "brief": first_sentences(game[0] if game else title, 40),
            "game": " ".join(game), "known": " ".join(known), "source": "; ".join(source),
            "links": links, "opens": "always", "first_release": first, "later": later,
        }, names=(W.people_words.get(pid) or {}).get("names", []))


def asks_line(lead, items, key):
    """'It asks, by the day: a; b; c.' from a list of {asks|what, law}; days not yet come are held back."""
    now, later = [], []
    for it in items or []:
        text = player_text(norm(it.get(key, "") if isinstance(it, dict) else it)).rstrip(".")
        if not text:
            continue
        hits = future_dates(text, START_YEAR)
        if hits:
            later.append((max(h[2] for h in hits), stop(cap(text))))
        else:
            now.append(low_first(text))
    return (stop(lead + " " + "; ".join(now)) if now else ""), later


def build_faiths():
    for f in W.self.get("faiths", []) or []:
        if not isinstance(f, dict) or not f.get("id"):
            continue
        fid, title = f["id"], norm(f.get("name", f["id"]))
        later, game, known = [], [], []
        game.append("This is one of the faiths a merchant may keep%s." % (
            "" if f.get("in_first_release") else "; it cannot be chosen in the first game"))
        for lead, key, field in (("By the day it asks:", "by_calendar", "asks"), ("Of your cargo it asks:", "by_cargo", "asks"),
                                 ("It opens:", "opens", "what")):
            line, lat = asks_line(lead, f.get(key), field)
            later += later_items(lat, "game")
            if line:
                game.append(line)
        up = [norm(x) for x in f.get("troth_up", []) or []]
        down = [norm(x) for x in f.get("troth_down", []) or []]
        if up:
            game.append(stop("Troth rises for one who " + "; ".join(up)))
        if down:
            game.append(stop("Troth falls for one who " + "; ".join(down)))
        for key, law in (("swears", "swears_law"), ("buries", "buries_law")):
            now, lat = gate(f.get(key, ""))
            later += later_items(lat, "known")
            if now:
                lead = "How its people swear: " if key == "swears" else "How its dead are laid: "
                known.append(lead + low_first(now))
        laws = []
        for key in ("by_calendar", "by_cargo", "opens"):
            for it in f.get(key, []) or []:
                if isinstance(it, dict) and it.get("law"):
                    laws.append(norm(it["law"]))
        for key in ("swears_law", "buries_law"):
            if f.get(key):
                laws.append(norm(f[key]))
        source = [norm(s) for s in f.get("sources", []) or []] + laws
        add_page({
            "id": "faith." + fid, "kind": "faith", "title": title,
            "hint": title,
            "brief": first_sentences(" ".join(game[:2]), 60, 260),
            "game": "\n".join(game), "known": "\n".join(known), "source": "; ".join(source),
            "links": [], "opens": "always", "first_release": bool(f.get("in_first_release")), "later": later,
        }, names=(W.faith_words.get(fid) or {}).get("names", []))


# ------------------------------------------------------------------------------------------------ days

def day_title(name):
    return norm(strip_paren(name).split(":")[0])


def day_aliases(name, did):
    """(other names of a day from its brackets, a weak name from its id)."""
    out, weak = [], []
    for m in re.finditer(r"\(([^()]*)\)", name):
        for piece in m.group(1).split(";"):
            piece = norm(piece)
            if piece and len(piece.split()) <= 3:
                out.append(piece)
                if piece.startswith("the "):
                    out.append(cap(piece[4:]))
    if "_" not in did and not strip_paren(name).startswith("Saint"):
        weak.append(cap(did))
    return out, weak


def faiths_line(ids):
    names = [(W.faith_words.get(f) or {}).get("many") or faith_title(f) for f in ids or []]
    return stop("Kept by: " + listing(names)) if names else ""


def price_line(e):
    bits = []
    if e.get("dear"):
        bits.append("dearer for it: " + goods_words(e["dear"], 6))
    if e.get("cheap"):
        bits.append("cheaper: " + goods_words(e["cheap"], 6))
    if not bits:
        return ""
    lead = "For %s days before, " % number(e["x_days_before"]) if e.get("x_days_before") else ""
    return stop(cap(lead + "; ".join(bits)))


def may_not_line(e):
    items = [norm(x).rstrip(".") for x in e.get("may_not", []) or [] if norm(x)]
    now = [x for x in items if not future_dates(x, START_YEAR)]
    later = [(max(h[2] for h in future_dates(x, START_YEAR)), stop(cap(x))) for x in items if future_dates(x, START_YEAR)]
    return (stop("A keeper may not: " + "; ".join(now)) if now else ""), later


def easter_table():
    return {int(y): v for y, v in (W.calendar.get("easter_by_year", {}) or {}).items() if str(y).isdigit() and isinstance(v, dict)}


def movable_dates(offset):
    out = []
    for y, row in sorted(easter_table().items()):
        e = row.get("easter")
        if not e:
            continue
        try:
            yy, mm, dd = [int(x) for x in e.split("-")]
            d = datetime.date(yy, mm, dd) + datetime.timedelta(days=int(offset))   # (793 to 806: the same leap years in both calendars)
            out.append("%d %s in %d" % (d.day, MONTHS[d.month - 1], y))
        except (ValueError, OverflowError):
            continue
    return out


def add_day(did, name, game, known, source, links, first, later, extra_names=()):
    title = day_title(name)
    alias, weak = day_aliases(name, did)
    add_page({
        "id": "day." + did, "kind": "day", "title": title,
        "hint": title,
        "brief": first_sentences(" ".join(game[:2]), 50),
        "game": " ".join(g for g in game if g), "known": " ".join(k for k in known if k), "source": norm(source),
        "links": links, "opens": "always", "first_release": first, "later": later,
    }, names=alias + list(extra_names), weak=weak)


def day_first(e):
    faiths = e.get("faiths")
    if faiths is not None and not any((W.self_faith.get(f) or {}).get("in_first_release") for f in faiths):
        return False
    port = e.get("port")
    if port and port not in TWELVE and not any(p in TWELVE for p in (e.get("heard_at") or []) + (e.get("also") or [])):
        return False
    return True


def build_days():
    cal = W.calendar
    if not cal:
        return

    def common(e):
        later, game, known = [], [], []
        line, lat = may_not_line(e)
        later += later_items(lat, "game")
        tail = [faiths_line(e.get("faiths")), line, price_line(e)]
        for key in ("note", "what", "rule", "prices"):
            now, lat = gate(player_text(e.get(key, "")))
            later += later_items(lat, "game")
            if now:
                tail.append(now)
        if e.get("excused"):
            tail.append(stop("Excused: " + norm(e["excused"])))
        if e.get("penalty"):
            now, lat = gate(e["penalty"])
            later += later_items(lat, "known")
            if now:
                known.append("What it cost to break it: " + low_first(now))
        if e.get("what_is_sure"):
            known.append("What is sure: " + low_first(stop(e["what_is_sure"])))
        if e.get("attested") in ATTESTED:
            known.append(ATTESTED[e["attested"]])
        links = ["good." + g for g in (e.get("dear") or []) + (e.get("cheap") or []) if g in W.good]
        links += ["faith." + f for f in e.get("faiths", []) or [] if f in W.self_faith]
        return game, tail, known, links, later

    for e in cal.get("week", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        kind = e.get("kind", "")
        lead = "%s comes every week" % day_title(e.get("name", ""))
        if e.get("weekday"):
            lead += ", on %s" % cap(e["weekday"])
        game = [stop(lead)]
        if kind == "fast":
            game.append("It is a fast.")
        if e.get("from"):
            game.append(stop("It runs from: " + norm(e["from"])))
        game += tail
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    for e in cal.get("fixed", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        try:
            when = "%d %s" % (int(e.get("day")), MONTHS[int(e.get("month")) - 1])
        except (TypeError, ValueError, IndexError):
            when = ""
        kind = e.get("kind", "day")
        span = " and the %s days after" % number(e["days"] - 1) if isinstance(e.get("days"), (int, float)) and e["days"] > 1 else ""
        game = [stop("%s is %s, kept on %s%s every year" % (day_title(e.get("name", "")), a_an(kind), when, span))] + tail
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    for e in cal.get("movable", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        off = e.get("easter_offset") or 0
        if off == 0:
            when = "It moves from year to year"
        elif abs(off) == 1:
            when = "It moves with Easter: the day %s it" % ("before" if off < 0 else "after")
        else:
            when = "It moves with Easter: %s days %s it" % (number(abs(off)), "before" if off < 0 else "after")
        kind = e.get("kind", "day")
        game = [stop("%s is %s. %s" % (day_title(e.get("name", "")), a_an(kind), when))] + tail
        dates = movable_dates(off)
        if dates:
            game.append(stop("Its day, year by year: " + "; ".join(dates)))
            known.append("The days are reckoned by the rule of Easter as Bede taught it; no record gives them one by one.")
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    by_name = {x.get("id"): day_title(x.get("name", "")) for sec in ("fixed", "movable") for x in cal.get(sec, []) or []}
    for e in cal.get("fasts", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        frm, to = e.get("from"), e.get("to")
        lead = "This is a fast" if frm and to else "This is one of the rules of fasting"
        if frm and to:
            a = by_name.get(frm) or (md_words(frm) if re.match(r"^\d\d-\d\d$", str(frm)) else norm(str(frm)).replace("11-11", "11 November").replace("11-30", "30 November"))
            b = by_name.get(to) or (md_words(to) if re.match(r"^\d\d-\d\d$", str(to)) else norm(str(to)))
            lead += ", kept from %s to %s" % (a, b)
            links += ["day." + x for x in (frm, to) if x in by_name]
        game = [stop(lead)] + tail
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    for e in cal.get("heathen", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        when, lat = gate(stop(cap(norm(e.get("when", "")))))
        later += later_items(lat, "game")
        game = [stop("%s is a day of those who keep the old gods" % day_title(e.get("name", "")))]
        if when:
            game.append("When: " + low_first(when))
        game += tail
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    for e in cal.get("fairs_and_things", []) or []:
        if not e.get("id"):
            continue
        game, tail, known, links, later = common(e)
        kind = e.get("kind", "gathering")
        port = e.get("port")
        what = {"fair": "a fair", "feast market": "a market held at a feast", "court": "a matter of the court",
                "assembly": "an assembly", "synod": "a meeting of bishops", "note": "a note on what the record lacks"}.get(kind, a_an(kind))
        lead = "This is " + what
        if port in W.port:
            lead += " at " + port_title(port)
            links.append("port." + port)
        also = [p for p in e.get("also", []) or [] if p in W.port]
        if also:
            lead += ", and at " + listing([port_title(p) for p in also])
            links += ["port." + p for p in also]
        if e.get("from") and e.get("to"):
            lead += ", from %s to %s every year" % (md_words(e["from"]), md_words(e["to"]))
        elif e.get("from") and e.get("days"):
            lead += ", for %s days from %s every year" % (number(e["days"]), md_words(e["from"]))
        game = [stop(lead)]
        if e.get("when"):
            now, lat = gate(stop(cap(norm(e["when"]))))
            later += later_items(lat, "game")
            if now:
                game.append("When: " + low_first(now))
        game += tail
        heard = [p for p in e.get("heard_at", []) or [] if p in W.port]
        if heard:
            game.append(stop("It is talked of at " + listing([port_title(p) for p in heard])))
            links += ["port." + p for p in heard]
        for y, text in sorted((e.get("by_year") or {}).items()):
            if str(y).isdigit() and norm(text):
                later.append({"date": iso(int(y)), "part": "known", "text": stop("In %s: %s" % (y, norm(text)))})
        for it in e.get("dated", []) or []:
            when = str(it.get("date", ""))
            parts = when.split("-")
            try:
                date = iso(int(parts[0]), int(parts[1]) if len(parts) > 1 else 12, int(parts[2]) if len(parts) > 2 else 28)
            except (ValueError, IndexError):
                continue
            later.append({"date": date, "part": "known",
                          "text": stop("%s, at %s: %s" % (date_words(when), norm(it.get("place", "")), norm(it.get("what", ""))))})
        add_day(e["id"], e.get("name", e["id"]), game, known, e.get("source", ""), links, day_first(e), later)

    # the days of the Jewish year: the rules, and each year's day
    jew = cal.get("jewish_by_year", {}) or {}
    jewish_first = bool((W.self_faith.get("jewish") or {}).get("in_first_release"))
    for key, rule in (jew.get("rules", {}) or {}).items():
        if not isinstance(rule, dict):
            continue
        name = cap(key.replace("_", " "))
        e = dict(rule)
        e["faiths"] = ["jewish"]
        game, tail, known, links, later = common(e)
        lead = "%s is a day of the Jewish year" % name
        if isinstance(rule.get("days"), (int, float)) and rule["days"] > 1:
            lead += ", kept for %s days" % number(rule["days"])
        game = [stop(lead)] + tail
        dates = []
        for y, row in sorted(jew.items()):
            if str(y).isdigit() and isinstance(row, dict) and row.get(key):
                dates.append("%s in %s" % (md_words(row[key][-5:]), y))
        if dates:
            game.append(stop("Its day, year by year: " + "; ".join(dates)))
            known.append("The days are reckoned by the fixed calendar of the rabbis; no record gives them one by one.")
        add_day("jewish_" + key, name, game, known, rule.get("source", "") or jew.get("source", ""), links, jewish_first, later)

    # the sailing year, one page
    spans = [s for s in cal.get("sailing_year", []) or [] if isinstance(s, dict) and s.get("name")]
    if spans:
        game = ["The old rule of the sea cuts the year in four."]
        for s in spans:
            game.append(stop("From %s to %s: %s" % (md_words(s.get("from", "")), md_words(s.get("to", "")), norm(s["name"]))))
        srcs = sorted({norm(s.get("source", "")) for s in spans if s.get("source")})
        add_day("sailing_year", "The sailing year", game,
                ["This is the rule of a Roman book that every clerk could read; how far the north kept it is not recorded here."],
                "; ".join(srcs), [], True, [], extra_names=["the sailing year", "the seas are shut", "safe sailing", "doubtful sailing"])


# ------------------------------------------------------------------------------------------------ words

WORD_KIND = {"greeting": "a greeting", "farewell": "a farewell", "thanks": "a word of thanks", "oath": "a word of oaths",
             "curse": "a curse", "kin": "a word of kin", "trade": "a word of trade", "ship": "a word of ships",
             "sea": "a word of the sea", "faith": "a word of faith", "law": "a word of law", "food": "a word of food and drink",
             "rank": "a word of rank"}


def build_words():
    tongues = {t.get("id"): t for t in W.tongues.get("tongues", []) or [] if isinstance(t, dict)}
    first_tongues = set()
    for p in W.self_people.values():
        if p.get("in_first_release"):
            first_tongues.add(p.get("first_tongue"))
            first_tongues.update(k for k, v in (p.get("half_known") or {}).items() if v)
    for tid, t in tongues.items():
        if not any(s in W.self_people or s in ((W.spine.get("peoples", {}) or {}).get("ids", {}) or {}) for s in t.get("spoken_by", []) or []):
            first_tongues.add(tid)                              # (a tongue of no one people: Latin)
    used = collections.Counter()
    for w in W.tongues.get("words", []) or []:
        if not isinstance(w, dict) or not norm(w.get("word", "")):
            continue
        word, tid = norm(w["word"]), w.get("tongue", "")
        t = tongues.get(tid, {})
        tname = strip_paren(t.get("name", tid.replace("_", " ")))
        key = "%s.%s" % (tid, slug(word))
        used[key] += 1
        if used[key] > 1:
            key += "_%d" % used[key]
        kind = WORD_KIND.get(w.get("kind", ""), "a word")
        game = [stop("%s is %s in the %s tongue" % (cap(word) if False else "\u201c%s\u201d" % word, kind, tname))]
        if w.get("means"):
            game.append(stop("It means: " + norm(w["means"])))
        if w.get("say"):
            game.append("Say it: %s." % norm(w["say"]))
        if w.get("use"):
            game.append("As it may be heard in the game: \u201c%s\u201d" % norm(w["use"]))
        known = []
        where = norm(re.sub(r"\s*\((?:Design|Research|Plans)/[^()]*\)", "", w.get("where_attested", "")))
        if where:
            known.append(stop("Where it is found: " + where))
            if "(?)" in where:
                known.append("The (?) says this was given from knowledge of the dictionary or the text and was not read again for the game.")
        add_page({
            "id": "word." + key, "kind": "word", "title": word,
            "hint": "A word of the %s tongue, not yet heard" % tname,
            "brief": stop("%s: %s" % (tname, norm(w.get("means", "")))),
            "game": " ".join(game), "known": " ".join(known),
            "source": "Assets/Kapman/Game/Data/story/tongues_793.json; the pages read are named in Design/content/lore/tongues_notes.md",
            "links": [], "opens": "word_heard:" + key, "first_release": tid in first_tongues, "later": [],
        })


# ------------------------------------------------------------------------------------------------ events

def event_when(e):
    date, precision = str(e.get("date", "")), e.get("precision", "year")
    year = date.split("-")[0]
    if precision == "day":
        return "On " + date_words(date)
    if precision == "month":
        return "In " + date_words(date)
    if precision == "season" and e.get("season"):
        return "In the %s of %s" % (e["season"], year)
    return "In " + year


def expand_sources(srcs, abbr):
    keys = sorted(abbr, key=len, reverse=True)
    used = []
    for s in srcs:
        for k in keys:
            if s.startswith(k):
                if k not in used:
                    used.append(k)
                break
    out = "; ".join(srcs)
    if used:
        out += ". " + " ".join(stop("%s: %s" % (k, norm(abbr[k]))) for k in used)
    return out


def build_events():
    ch = W.chronicle
    abbr = ch.get("abbreviations", {}) or {}
    kinds = ch.get("kinds", {}) or {}
    sure = {"sure": "The sources are sure of it.", "likely": "It is likely, not certain.", "uncertain": "It is uncertain."}
    for e in ch.get("entries", []) or []:
        if not isinstance(e, dict) or not e.get("id") or not e.get("fire"):
            continue
        eid, fire = e["id"], str(e["fire"])
        try:
            year = int(fire.split("-")[0])
        except ValueError:
            continue
        later, game, known, links = [], [], [], []
        title = norm(e.get("title", eid))
        ports = [p for p in e.get("ports", []) or [] if p in W.port]
        links += ["port." + p for p in ports]
        effect = with_port_names(norm(e.get("effect", "")))
        if effect:
            game.append(stop("In the game: " + effect))
        if ports:
            game.append(stop("It happens at or near " + listing([port_title(p) for p in ports])))
        regions = [r for r in e.get("regions", []) or []]
        if regions:
            game.append(stop("It is felt in: " + listing([region_label(r) for r in regions[:6]]) +
                             (", and %d more lands" % (len(regions) - 6) if len(regions) > 6 else "")))
        else:
            game.append("It is news only: it moves no price and shuts no port.")
        if e.get("end"):
            end = str(e["end"])
            parts = end.split("-")
            try:
                date = iso(int(parts[0]), int(parts[1]) if len(parts) > 1 else 12, int(parts[2]) if len(parts) > 2 else 28)
                later.append({"date": date, "part": "game", "text": "It lasted until %s." % date_words(end)})
            except (ValueError, IndexError):
                pass
        if kinds.get(e.get("kind")):
            game.append(stop("In the chronicle it stands under “%s”: %s" % (e["kind"], norm(kinds[e["kind"]]))))
        place = norm(e.get("place", ""))
        known.append(stop("%s: %s" % (event_when(e), title)))
        if place and place.split(",")[0].split(" (")[0].lower() not in title.lower():
            known.append(stop("The place: " + place))
        note, lat = gate(e.get("note", ""), max(year, START_YEAR))
        later += later_items(lat, "known")
        if note:
            known.append(note)
        if e.get("certainty") in sure:
            known.append(sure[e["certainty"]])
        add_page({
            "id": "event." + eid, "kind": "event", "title": title,
            "hint": "A day that has not come",
            "brief": first_sentences(stop("%s: %s" % (event_when(e), title)), 40),
            "game": " ".join(game), "known": " ".join(known),
            "source": expand_sources([norm(s) for s in e.get("sources", []) or []], abbr),
            "links": links, "opens": "date:" + fire, "first_release": year <= LIFE_END, "later": later,
        })
        wants[:] = [w for w in wants if w[1] != "event." + eid]      # (an event's title is a sentence, not a name)


# ------------------------------------------------------------------------------------------------ texts

FORM_WORDS = {"rune_stick": "a rune stick", "parchment": "a leaf of parchment", "book": "a book", "wax_tablet": "a wax tablet"}


def build_texts():
    for t in W.texts.get("texts", []) or []:
        if not isinstance(t, dict) or not t.get("id"):
            continue
        tid, title = t["id"], norm(t.get("title", t["id"]))
        game = [stop("This is %s in %s: %s" % (FORM_WORDS.get(t.get("form"), "a writing"),
                                               norm(t.get("language", "its own tongue")), title))]
        series = t.get("series") or {}
        if isinstance(series, dict) and series.get("part") and series.get("of"):
            game.append("It is part %s of %s." % (number(series["part"]), number(series["of"])))
        game.append("Once you have it, it lies in your chest and is read there; this page says only what it is.")
        known = []
        if t.get("writtenDown"):
            known.append(stop("Written down: " + norm(t["writtenDown"])))
        if t.get("toldLater"):
            known.append("It tells of the time, but was written down later.")
        where = t.get("where", "")
        if isinstance(where, dict):
            where = "; ".join(norm(str(where[k])) for k in ("title", "editor") if where.get(k))
        where = "; ".join(seg for seg in [norm(x) for x in str(where).split(";")]
                          if seg and not re.search(r"scratchpad|\.txt|\bIA |https?://|Gutenberg #|cpb\d|Wikisource|\.djvu|checked against|lines \d{4,}", seg))
        if where:
            known.append(stop("Where it stands: " + balance(where)))
        if t.get("ownTranslation"):
            known.append("The English is the game's own draft.")
        elif t.get("translator") and not re.match(r"^[a-z]+\d{4}$", norm(str(t["translator"]))):
            known.append(stop("The English is by %s%s" % (norm(t["translator"]), (", %s" % t["translated"]) if t.get("translated") else "")))
        if t.get("check"):
            known.append("The original was quoted from memory and is still to be checked.")
        acq = t.get("acquire") or {}
        places = [w.get("port") for w in acq.get("where", []) or [] if isinstance(w, dict)]
        first = any(p in TWELVE for p in places) if places else t.get("people") != "irish"
        names = []
        bare = strip_paren(title)
        if bare != title:
            names.append(bare)
        if isinstance(series, dict) and series.get("part") == 1 and "," in title:
            names.append(norm(title.split(",")[0]))
        add_page({
            "id": "text." + tid, "kind": "text", "title": title,
            "hint": "A writing not yet in your chest",
            "brief": first_sentences(game[0], 30),
            "game": " ".join(game), "known": " ".join(known), "source": norm(t.get("credit", "")),
            "links": ["port." + p for p in places if p in W.port],
            "opens": "text_held:" + tid, "first_release": bool(first), "later": [],
        }, names=names)


# ------------------------------------------------------------------------------------------------ how to play (by hand)

def rule_tokens():
    stats = []
    for s in W.self.get("stats", []) or []:
        if isinstance(s, dict) and s.get("name"):
            stats.append("%s is %s" % (s["name"], low_first(norm(s.get("what", "")).split(":")[0].rstrip("."))))
    pasts = [low_first(norm(p.get("name", ""))) for p in W.self.get("pasts", []) or [] if isinstance(p, dict) and p.get("name")]
    coins = [stop("%s: %s" % (cap(c.get("name", "")), norm(c.get("note", "")))) for c in W.coin_list if c.get("note")]
    return {
        "{stats_what}": "; ".join(stats),
        "{pasts}": "; ".join(pasts),
        "{food_goods}": listing([low_good(good_title(g)) for g in W.food_goods]),
        "{drink_goods}": listing([low_good(good_title(g)) for g in W.drink_goods], "or"),
        "{coin_notes}": " ".join(coins),
    }


def build_rules():
    tokens = rule_tokens()
    for r in W.rules.get("pages", []) or []:
        if not isinstance(r, dict) or not r.get("id") or not r.get("title"):
            continue

        def fill(text):
            text = text or ""
            for k, v in tokens.items():
                text = text.replace(k, v)
            return text

        known, lat = gate(fill(r.get("known", "")))
        page = add_page({
            "id": "rule." + r["id"], "kind": "how_to_play", "title": norm(r["title"]),
            "hint": norm(r["title"]),
            "brief": fill(r.get("brief", "")),
            "game": fill(r.get("game", "")), "known": known, "source": r.get("source", ""),
            "links": list(r.get("see", []) or []), "opens": "always", "first_release": True,
            "later": later_items(lat, "known"),
        }, names=r.get("names", []) or [])
        if page is not None:
            page["built"] = bool(r.get("built"))
            page["built_note"] = norm(r.get("built_note", ""))
            page["holds"] = norm(r.get("holds", ""))


# ------------------------------------------------------------------------------------------------ names and links

def is_letter(c):
    return bool(c) and (c.isalpha())


class Matcher(object):
    """Finds names in a text: as written, between two characters that are not letters, the longest first.
    A shield is matched like a name and makes no link: it only keeps the name inside it from linking."""

    def __init__(self, names, shields=()):
        self.by_first = collections.defaultdict(list)
        for name, pid in names:
            self.by_first[name[0]].append((name, pid))
        for s in shields:
            if s:
                self.by_first[s[0]].append((s, None))
        for k in self.by_first:
            self.by_first[k].sort(key=lambda x: -len(x[0]))

    def find(self, text):
        out, i, n = [], 0, len(text)
        while i < n:
            c = text[i]
            if c in self.by_first and (i == 0 or not is_letter(text[i - 1])):
                for name, pid in self.by_first[c]:
                    if text.startswith(name, i) and not is_letter(text[i + len(name):i + len(name) + 1]):
                        if pid is not None:
                            out.append((name, pid, i))
                        i += len(name) - 1
                        break
            i += 1
        return out


# the known part of these kinds is a citation (an edition, a dig, a stone): its names are not the story's
CITED_KINDS = ("text", "word", "ship", "event", "site", "person")


def page_text(p):
    return " ".join([p.get("brief", ""), p.get("game", ""), p.get("known", "")])


def link_text(p):
    """The text a page's links are read from."""
    if p["kind"] in CITED_KINDS:
        return " ".join([p.get("brief", ""), p.get("game", "")])
    return page_text(p)


def settle_names():
    """Who gets each name. Returns (names for the file, names left out, the matcher)."""
    # the forms of the goods need the data's own small words
    good_name_forms(SMALL)
    for x in W.rules.get("extra_names", []) or []:
        if isinstance(x, dict) and x.get("name") and x.get("page"):
            if x["page"] in by_id:
                want(x["name"], x["page"], 1)
            else:
                remarks.append("wiki_rules.json extra_names: no page %s for the name '%s'" % (x["page"], x["name"]))
    never = NEVER
    shields = [norm(x) for x in W.rules.get("shields", []) or [] if norm(x)]
    by_name = collections.defaultdict(list)
    for name, pid, rank in wants:
        if pid in by_id:
            by_name[name].append((pid, rank))
    left_out, chosen = [], {}
    for name, cands in by_name.items():
        best = {}
        for pid, rank in cands:
            if pid not in best or rank < best[pid]:
                best[pid] = rank
        if name in never:
            left_out.append({"name": name, "page": sorted(best)[0], "why": "on the list of words not to link (wiki_rules.json)"})
            continue
        ranked = sorted(best.items(), key=lambda kv: (1 if kv[1] >= 3 else 0, KIND_RANK.get(by_id[kv[0]]["kind"], 99),
                                                    kv[1], 0 if by_id[kv[0]].get("first_release") else 1, order_of[kv[0]]))
        win = ranked[0][0]
        note = ""
        if len(ranked) > 1:
            others = [pid for pid, _ in ranked[1:]]
            w, o = by_id[win], by_id[others[0]]
            if (1 if ranked[0][1] >= 3 else 0) != (1 if ranked[1][1] >= 3 else 0):
                why = "it is that page's own name, and only a byname of the other"
            elif w["kind"] != o["kind"]:
                why = "%s comes before %s" % (w["kind"].replace("_", " "), o["kind"].replace("_", " "))
            elif ranked[0][1] != ranked[1][1]:
                why = "it is nearer that page's own title"
            elif w.get("first_release") and not o.get("first_release"):
                why = "it belongs to the first release"
            else:
                why = "it stands first in the data"
            note = "also wanted by %s%s; this page wins: %s" % (", ".join(others[:4]), " and %d more" % (len(others) - 4) if len(others) > 4 else "", why)
        chosen[name] = (win, note, [pid for pid, _ in ranked[1:]])

    # a foreign word that is also a plain English word here must not link from everywhere
    counts = collections.Counter()
    probe = Matcher([(n, c[0]) for n, c in chosen.items() if by_id[c[0]]["kind"] == "word"])
    for p in pages:
        if p["kind"] == "word":
            continue
        for name, pid, _ in probe.find(page_text(p)):
            counts[name] += 1
    for name in list(chosen):
        pid = chosen[name][0]
        if by_id[pid]["kind"] == "word" and counts[name] > 12 and re.match(r"^[A-Za-z]+$", name):
            left_out.append({"name": name, "page": pid, "why": "a plain word in the pages' own English (%d uses): it would link everywhere" % counts[name]})
            del chosen[name]

    names = []
    for n, (pid, note, others) in chosen.items():
        row = collections.OrderedDict([("name", n), ("page", pid)])
        if note:
            row["note"] = note
            row["also"] = others[:8]
        names.append(row)
    names.sort(key=lambda x: (-len(x["name"]), x["name"]))
    cands = {n: set([c[0]] + c[2]) for n, c in chosen.items()}
    return names, sorted(left_out, key=lambda x: x["name"]), Matcher([(x["name"], x["page"]) for x in names], shields), cands, shields


def settle_links(matcher, cands):
    """Each page's links: those its maker gave it, then the pages its text names. Where two pages share a name
    and the page already leads to one of them (a site to its own Gerbrand), the name's winner is not added."""
    uses = collections.Counter()
    for p in pages:
        seen, links = set(), []
        for pid in p.get("links", []):
            if pid in by_id and pid != p["id"] and pid not in seen:
                seen.add(pid)
                links.append(pid)
        given = set(seen)
        found_here = set()
        for name, pid, _ in matcher.find(link_text(p)):
            if pid == p["id"]:
                continue
            others = cands.get(name, ())
            if p["id"] in others or (pid not in given and any(o in given for o in others)):
                continue
            if pid not in seen:
                seen.add(pid)
                links.append(pid)
            if name not in found_here:
                found_here.add(name)
                uses[name] += 1
        p["links"] = links
    return uses


# ------------------------------------------------------------------------------------------------ the run

SAMPLE = {}


def write_samples(folder, n):
    random.seed(793)
    os.makedirs(folder, exist_ok=True)
    for kind in KINDS:
        mine = [p for p in pages if p["kind"] == kind]
        first = [p for p in mine if p.get("first_release")]
        rest = [p for p in mine if not p.get("first_release")]
        pick = random.sample(first, min(len(first), (n * 3) // 4))
        pick += random.sample(rest, min(len(rest), n - len(pick)))
        with io.open(os.path.join(folder, "wiki_sample_%s.txt" % kind), "w", encoding="utf-8") as f:
            for p in pick:
                f.write("=== %s  [%s]  opens: %s  first: %s\n" % (p["title"], p["id"], p["opens"], p["first_release"]))
                f.write("HINT   %s\nBRIEF  %s\nGAME   %s\nKNOWN  %s\nSOURCE %s\n" % (
                    p["hint"], p["brief"], p["game"].replace("\n", "\n       / "), p["known"].replace("\n", "\n       / "), p["source"][:300]))
                for x in p.get("later", [])[:4]:
                    f.write("LATER  %s (%s) %s\n" % (x["date"], x["part"], x["text"]))
                if len(p.get("later", [])) > 4:
                    f.write("LATER  ... and %d more\n" % (len(p["later"]) - 4))
                f.write("LINKS  %s\n\n" % ", ".join(p["links"][:14]))
    with io.open(os.path.join(folder, "wiki_sample_names.txt"), "w", encoding="utf-8") as f:
        f.write("NAMES TWO PAGES WANTED\n")
        for x in SAMPLE.get("names", []):
            if x.get("note"):
                f.write("  %-34s -> %-40s %s\n" % (x["name"], x["page"], x["note"]))
        f.write("\nLEFT OUT\n")
        for x in SAMPLE.get("left_out", []):
            f.write("  %-24s %-40s %s\n" % (x["name"], x["page"], x["why"]))
        f.write("\nTHE NAMES THAT LINK FROM THE MOST PAGES\n")
        for n, c in SAMPLE.get("uses", collections.Counter()).most_common(260):
            f.write("  %-28s %5d  -> %s\n" % (n, c, SAMPLE["page_of"].get(n, "")))
        f.write("\nFOREIGN WORDS THAT ARE ALSO SMALL WORDS IN THE DATA (look these over)\n")
        for x in SAMPLE.get("names", []):
            if x["page"].startswith("word.") and x["name"].lower() in SMALL:
                f.write("  %-18s %4d uses -> %s\n" % (x["name"], SAMPLE["uses"].get(x["name"], 0), x["page"]))
        f.write("\nEVERY NAME, BY KIND OF PAGE\n")
        per = collections.defaultdict(list)
        for x in SAMPLE.get("names", []):
            per[x["page"].split(".")[0]].append(x["name"])
        for k, v in sorted(per.items()):
            f.write("%s (%d): %s\n\n" % (k, len(v), " | ".join(sorted(v))))


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    read_world()
    for build in (build_rules, build_ports, build_sites, build_people_of_ports, build_goods, build_ships,
                  build_peoples, build_faiths, build_days, build_words, build_events, build_texts):
        try:
            build()
        except Exception as e:                                   # one kind failing must not lose the rest
            remarks.append("%s stopped: %s: %s" % (build.__name__, type(e).__name__, e))
            import traceback
            traceback.print_exc()
    names, left_out, matcher, cands, shields = settle_names()
    uses = settle_links(matcher, cands)
    SAMPLE.update({"names": names, "left_out": left_out, "uses": uses, "page_of": {x["name"]: x["page"] for x in names}})

    counts = collections.OrderedDict()
    for k in KINDS:
        mine = [p for p in pages if p["kind"] == k]
        counts[k] = {"pages": len(mine), "first_release": sum(1 for p in mine if p.get("first_release")),
                     "with_later": sum(1 for p in mine if p.get("later"))}
    out_pages = []
    for p in pages:
        q = collections.OrderedDict()
        for key in ("id", "kind", "title", "hint", "brief", "game", "known", "source", "links", "opens", "first_release"):
            q[key] = p.get(key)
        if p.get("later"):
            q["later"] = sorted(p["later"], key=lambda x: tuple(int(n) for n in x["date"].split("-")))
        for key in ("built", "built_note", "holds"):
            if key in p:
                q[key] = p[key]
        out_pages.append(q)
    out = collections.OrderedDict()
    out["about"] = ("THE WIKI, baked from the game's own data by Bake/content/make_wiki.py with the hand-written rules of "
                    "Bake/content/wiki_rules.json (Design/wiki.md; DECISIONS.md, 7 October 2026, rulings 3 and 6). Do not edit: "
                    "change the data or wiki_rules.json and bake again. pages: id, kind, title, hint (what it is called before it "
                    "is known), brief (the box under the pointer), game (what it is and does for the player), known (what the "
                    "record says, from the data's own notes; game and known may hold line breaks between paragraphs), source (as "
                    "the data gives it), links (the pages its text names, and "
                    "for a port its sites and people), opens, first_release. later: text that names a day after the first morning "
                    "(21 June 793), each piece with the date after which it may be shown and the part (game or known) it belongs "
                    "to: show a piece only when that day has passed in the game. opens: always | port_heard:<port> (heard of, or "
                    "seen) | port_seen:<port> | good_seen:<good> | person_met:<person> | word_heard:<tongue.word> | "
                    "text_held:<text> | date:<yyyy-mm-dd> (the day has passed); two rules joined by & must both hold. A How to play "
                    "page also has built (false: ruled but not in the game, hold the page back), built_note and holds, which are "
                    "for the builders, not the player. names: every word or phrase that links, the longest first; a note says who "
                    "else wanted the word and why this page won, and 'also' lists the others, so that the frame may choose by "
                    "where it stands (on a port's own pages, that port's Gerbrand). A name is matched exactly as written, "
                    "capitals and all, where the letters before and after it are not letters; the longest wins; the first use "
                    "in a box becomes the link. shields: phrases that hold a name and must not link (New York is not York): "
                    "match a shield like a name and make no link of it. The baker read each page's links from its brief and "
                    "game text, and from its known text too for ports, goods, peoples, faiths, days and rules; the known text "
                    "of the other kinds and every source line are citations, and are better left unlinked. left_out: words "
                    "that would have linked and were kept back, with the reason. (?) in a text marks what the record leaves "
                    "in doubt or what was not checked; LATER marks a source written long after.")
    out["made"] = datetime.date.today().isoformat()
    out["first_release"] = {"ports": TWELVE, "from": START_YEAR, "to": LIFE_END, "first_morning": START_DAY}
    out["kinds"] = KINDS
    out["counts"] = counts
    out["skipped"] = [{"file": f, "why": w} for f, w in skipped]
    out["pages"] = out_pages
    out["names"] = names
    out["shields"] = shields
    out["left_out"] = left_out
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with io.open(OUT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(out, f, ensure_ascii=False, indent=1)
        f.write("\n")

    print("WIKI BAKED: %s" % rel(OUT))
    print("  %d pages, %d names, %d words left out" % (len(pages), len(names), len(left_out)))
    print("  %-12s %6s %14s %11s" % ("kind", "pages", "first release", "with later"))
    for k, c in counts.items():
        print("  %-12s %6d %14d %11d" % (k, c["pages"], c["first_release"], c["with_later"]))
    rules = [p for p in pages if p["kind"] == "how_to_play"]
    print("  how to play: %d built, %d ruled but not built (held back)" % (
        sum(1 for p in rules if p.get("built")), sum(1 for p in rules if not p.get("built"))))
    print("  names two pages wanted: %d" % sum(1 for x in names if x.get("note")))
    print("  pieces held back until their day: %d" % sum(len(p.get("later", [])) for p in pages))
    if skipped:
        print("SKIPPED (missing or will not parse):")
        for f, w in skipped:
            print("  %s: %s" % (f, w))
    else:
        print("  no file was skipped")
    for r in remarks:
        print("NOTE: " + r)
    busy = uses.most_common(25)
    print("the names that link from the most pages: " + ", ".join("%s (%d)" % (n, c) for n, c in busy))

    if "--sample" in sys.argv:
        i = sys.argv.index("--sample")
        folder = sys.argv[i + 1] if len(sys.argv) > i + 1 else os.path.join(HERE, "out")
        n = int(sys.argv[i + 2]) if len(sys.argv) > i + 2 and sys.argv[i + 2].isdigit() else 50
        write_samples(folder, n)
        print("samples: %s" % folder)
    return 0


if __name__ == "__main__":
    sys.exit(main())
