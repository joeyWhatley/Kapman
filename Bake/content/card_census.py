# -*- coding: utf-8 -*-
r"""
CARD CENSUS - reads every card file in Assets/Kapman/Game/Data/encounters and counts, by file:
  cards    all the cards in the file
  open     cards that could ever open in the first release: the twelve ports of Design/GAME_MAKER.md (stage 2), the
           years 793 to 805, one life (generation 0). A card is open when (a) its "when" can hold at one of the twelve
           ports (harbour cards), at sea (sea cards) or either (crew and long cards) in one of those years, (b) something
           can bring it (it comes on arrival, it has a weight or a rate, or an open card names it in "then" or
           "schedule", at a port of the twelve), and (c) every flag it asks for is set by some open card, or by no card
           at all (then the game's code is taken to set it). (c) is worked out round by round until nothing changes.
  real     open cards with a choice that costs or gains something hard: 10 d of silver or more, goods, a man, the law,
           the toll, an order, a hire.
  door     open cards with no hard choice but one that gives knowledge or a door: a text, a riddle, a lesson, a tongue,
           a passenger, a deed, news, a price shock, an unlock_ or changed_ flag.
  soft     open cards where every choice moves only heart, health, feelings and plain flags.
  keyed    open cards with a choice that only shows for who you are or what you hold (a text, a tongue, a riddle, a
           lesson, a person's feeling, a good aboard, an earlier flag, the port's people or faith).
  far      shut by place or year: the "when" cannot hold at the twelve ports in 793 to 805.
  cut      the "when" could hold, but the card that sets its flag, or that schedules it, is itself shut.
  nodoor   the "when" could hold, but nothing brings it (weight 0, no rate, and no card names it).
  nodraw   of the open cards: stage "long" with a rate. Scheduler.HourlyWeight gives stage long no weight today, so
           these wait for the engine.
Then the conditions and the effects most used, the flags, the twelve ports, and the sea's rates.
It is a count, not the game's loader: the EditMode tests are the last word on cards.
  & "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\card_census.py
"""
import collections, glob, io, json, math, os, re, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
ENC = os.path.join(DATA, "encounters")

# the twelve ports of the first release (Design/GAME_MAKER.md, stage 2) and its years (DECISIONS.md, 7 October)
RELEASE_PORTS = ["dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre",
                 "quentovic", "sarre", "fordwic", "ripa"]
FIRST_YEAR, LAST_YEAR, START_MONTH = 793, 805, 5      # (1 May 793 is the earliest start on the table)
HARD_SILVER = 10.0                                     # the book's "Price" mark: 10 d or more
DEAD_VERBS = ("passenger", "learn", "tongue", "encounter", "open_fetch")    # GameEffects returns null for these today
NO_MODEL = re.compile(r"^(tongue\..*|crew\..*_count|crew\.kind|crew_min|crew_full|days_since_paid|venture_ended|"
                      r"learned\.runes|chest\.rune_count|reform\.coin|fast|hold\.from_.*)$")   # EncounterGameVars: read 0

UNKNOWN = object()


def load(path):
    with io.open(path, "r", encoding="utf-8-sig") as f:
        return json.load(f)


# ---------------------------------------------------------------- the condition language (Cond.cs)

TOKEN = re.compile(r"\s*(\(|\)|<=|>=|!=|==|<|>|=|[-+]?\d[\d.]*|[A-Za-z_][A-Za-z0-9_.\-]*)")
OPS = {"<", "<=", "=", "!=", ">=", ">"}
TEXT_VARS = re.compile(r"^(port\.(id|kind|people|faith|realm)|level_kind|crew\.kind|npc\.[a-z0-9_]+\.last|riddle\..*)$")


def tokens(text):
    out, i = [], 0
    while i < len(text):
        m = TOKEN.match(text, i)
        if not m:
            if text[i:].strip() == "":
                break
            raise ValueError("unexpected %r in %r" % (text[i], text))
        t = m.group(1)
        out.append("=" if t == "==" else t)
        i = m.end()
    return out


class Parser(object):
    def __init__(self, text):
        self.t, self.i = tokens(text), 0

    def peek(self):
        return self.t[self.i] if self.i < len(self.t) else ""

    def next(self):
        if self.i >= len(self.t):
            raise ValueError("ends too soon")
        self.i += 1
        return self.t[self.i - 1]

    def expr(self):
        parts = [self.conj()]
        while self.peek() == "or":
            self.i += 1
            parts.append(self.conj())
        return parts[0] if len(parts) == 1 else ("or", parts)

    def conj(self):
        parts = [self.term()]
        while self.peek() == "and":
            self.i += 1
            parts.append(self.term())
        return parts[0] if len(parts) == 1 else ("and", parts)

    def term(self):
        t = self.next()
        if t == "not":
            return ("not", self.term())
        if t == "(":
            inner = self.expr()
            if self.next() != ")":
                raise ValueError("')' expected")
            return inner
        if t == "true":
            return ("const", True)
        if t == "false":
            return ("const", False)
        if self.peek() not in OPS:
            return ("bare", t)
        op, right = self.next(), self.next()
        return ("cmp", t, op, right)


def parse(text):
    if text is None or not str(text).strip():
        return ("const", True)
    p = Parser(str(text))
    node = p.expr()
    if p.i < len(p.t):
        raise ValueError("unexpected %r" % p.peek())
    return node


def is_number(s):
    return bool(re.match(r"^[-+]?\d", s))


def names_of(node, into):
    k = node[0]
    if k == "not":
        names_of(node[1], into)
    elif k in ("and", "or"):
        for p in node[1]:
            names_of(p, into)
    elif k == "bare":
        into.append(node[1])
    elif k == "cmp":
        into.append(node[1])
        r = node[3]
        if not is_number(r) and ("." in r) and not TEXT_VARS.match(node[1]):
            into.append(r)                                   # a variable on the right
    return into


def value(name, S):
    if name in S:
        return S[name]
    if name.startswith("flag."):
        return UNKNOWN if S["_flags"] is None or name[5:] in S["_flags"] else 0.0
    return UNKNOWN


def holds(op, a, b):
    return {"<": a < b, "<=": a <= b, "=": abs(a - b) < 1e-9, "!=": abs(a - b) >= 1e-9, ">=": a >= b, ">": a > b}[op]


def ev(node, S):
    """Three-valued: True, False, or None (it depends on something the census does not know)."""
    k = node[0]
    if k == "const":
        return node[1]
    if k == "not":
        v = ev(node[1], S)
        return None if v is None else (not v)
    if k == "and":
        res = True
        for p in node[1]:
            v = ev(p, S)
            if v is False:
                return False
            if v is None:
                res = None
        return res
    if k == "or":
        res = False
        for p in node[1]:
            v = ev(p, S)
            if v is True:
                return True
            if v is None:
                res = None
        return res
    if k == "bare":
        v = value(node[1], S)
        if v is UNKNOWN:
            return None
        if isinstance(v, frozenset):
            return len(v) > 0
        return bool(v)
    name, op, right = node[1], node[2], node[3]
    a = value(name, S)
    if a is UNKNOWN:
        return None
    if isinstance(a, frozenset):                             # a port of two peoples: it may be either
        if op not in ("=", "!="):
            return None
        word = right.lower()
        same = True if a == frozenset([word]) else (None if word in a else False)
        return same if op == "=" or same is None else (not same)
    if isinstance(a, str):
        if op not in ("=", "!="):
            return None
        if right.startswith("npc.") or right.startswith("port."):
            b = value(right, S)
            if b is UNKNOWN or not isinstance(b, str):
                return None
            same = a.lower() == b.lower()
        else:
            same = a.lower() == right.lower()
        return same if op == "=" else (not same)
    if is_number(right):
        return holds(op, a, float(right))
    b = value(right, S)
    if b is UNKNOWN or isinstance(b, (str, frozenset)):
        return None                                          # a word (standing, feeling) or an unknown variable
    return holds(op, a, b)


# ---------------------------------------------------------------- the tables

ports_doc = load(os.path.join(DATA, "ports_793.json"))
PORT = {p["id"]: p for p in ports_doc["ports"]}
roles_doc = load(os.path.join(ENC, "npc_roles_793.json"))
REALM = roles_doc.get("realms", {})
missing = [p for p in RELEASE_PORTS if p not in PORT]
if missing:
    print("WARNING: not in ports_793.json: %s" % ", ".join(missing))


def scene_port(p, year, month, flags):
    d = PORT.get(p, {})
    realm = d.get("realm", "")
    r = REALM.get(realm, {})
    return {"port.id": p, "port.realm": realm, "port.kind": d.get("kind", ""),
            "port.people": frozenset(x.lower() for x in r.get("peoples", [])), "port.faith": r.get("faith", ""),
            "moored": 1.0, "under_way": 0.0, "year": float(year), "month": float(month), "generation": 0.0, "_flags": flags}


def scene_sea(year, month, flags):
    return {"port.id": "", "port.realm": "", "port.kind": "", "port.people": frozenset(), "port.faith": "",
            "moored": 0.0, "year": float(year), "month": float(month), "generation": 0.0, "_flags": flags}


def times(names):
    """The years and months worth trying: all of them only if the condition reads them."""
    years = range(FIRST_YEAR, LAST_YEAR + 1) if "year" in names else [FIRST_YEAR + 1]
    months = range(1, 13) if "month" in names else [7]
    for y in years:
        for m in months:
            if y == FIRST_YEAR and m < START_MONTH and "month" in names:
                continue
            yield y, m


# ---------------------------------------------------------------- the cards

class Card(object):
    pass


def verbs_of(node, into):
    """Every verb string in a cost or an effects node (a chance's branches too)."""
    if node is None:
        return into
    if isinstance(node, list):
        for x in node:
            verbs_of(x, into)
    elif isinstance(node, dict):
        into.append("chance")
        verbs_of(node.get("success"), into)
        verbs_of(node.get("failure"), into)
    elif isinstance(node, str):
        into.append(node.strip())
    return into


def num(s):
    try:
        return float(s)
    except ValueError:
        return 0.0


FLAG_FAMILIES = ("ch_", "unlock_", "changed_", "sea_", "crew_", "long_", "st_", "here_", "war_", "act_", "feud_")


def family(name):
    """A variable's family, for the tallies."""
    if name.startswith("flag."):
        f = name[5:]
        for pre in FLAG_FAMILIES:
            if f.startswith(pre):
                return "flag." + pre + "*"
        return "flag.(other)"
    m = re.match(r"^npc\.[^.]+\.(\w+)$", name)
    if m:
        return "npc.<role>." + m.group(1)
    if name.startswith("hold.from_"):
        return "hold.from_<region>"
    if name.startswith("text."):
        return "text.<id>"
    if name.startswith("riddle."):
        return "riddle.<id>"
    if name.startswith("tongue."):
        return "tongue.<name>"
    if re.match(r"^crew\.\w+_best$", name):
        return "crew.<skill>_best"
    if re.match(r"^crew\.\w+_count$", name):
        return "crew.<kind>_count"
    if name.startswith("hold.") and name not in ("hold.weapons", "hold.captives", "hold.value"):
        return "hold.<good>"
    return name


def effect_family(v):
    t = v.split()
    if not t:
        return "(empty)"
    if t[0] == "flag" and len(t) > 1:
        return "flag " + family("flag." + t[1])[5:]
    if t[0] == "toll" and len(t) > 1:
        return "toll " + t[1]
    return t[0]


KEY_VARS = re.compile(r"^(text\.|riddle\.|tongue\.|learned\.|chest\.|npc\.[^.]+\.(feeling|met|owed|last)|flag\.|hold\.|"
                      r"port\.(people|faith)|port\.best_feeling|standing)")


def option_kind(verbs, cost_verbs):
    """hard, door or soft, from what a choice costs and does."""
    hard = door = False
    for v in verbs + cost_verbs:
        t = v.split()
        if not t or t[0] == "chance":
            continue
        verb = t[0]
        if verb == "silver" and len(t) > 1 and abs(num(t[1])) >= HARD_SILVER:
            hard = True
        elif verb in ("goods", "men") and abs(num(t[-1])) > 0:
            hard = True
        elif verb in ("law", "commission", "hire", "pay_off"):
            hard = True
        elif verb == "toll" and len(t) > 1 and t[1] in ("none", "eased"):
            hard = True
        elif verb in ("give_text", "give_riddle", "open_fetch", "learn", "tongue", "passenger", "deed", "news", "price_shock"):
            door = True
        elif verb == "flag" and len(t) > 1 and (t[1].startswith("unlock_") or t[1].startswith("changed_")):
            door = True
    return "hard" if hard else "door" if door else "soft"


cards, by_id, faults = [], {}, []
files = []
for path in sorted(glob.glob(os.path.join(ENC, "*.json"))):
    try:
        doc = load(path)
    except Exception as e:
        faults.append("%s: does not parse (%s)" % (os.path.basename(path), e))
        continue
    if not isinstance(doc, dict) or not isinstance(doc.get("cards"), list):
        continue
    fname = os.path.basename(path)
    files.append(fname)
    for o in doc["cards"]:
        if not isinstance(o, dict):
            continue
        c = Card()
        c.file, c.id, c.stage = fname, o.get("id", ""), o.get("stage", "")
        c.arrival, c.once = bool(o.get("arrival")), bool(o.get("once"))
        c.weight = o.get("weight") or 0.0
        rate = o.get("rate")
        c.by_sim = rate == "traffic"
        c.rate = float(rate) if isinstance(rate, (int, float)) else 0.0
        c.cooldown = o.get("cooldown_days") or 0.0
        c.months = o.get("months") or []
        try:
            c.when = parse(o.get("when"))
        except ValueError as e:
            faults.append("%s %s: when: %s" % (fname, c.id, e))
            c.when = ("const", True)
        c.when_names = set(names_of(c.when, []))
        c.cond_names = list(names_of(c.when, []))            # every condition of the card, for the tallies
        for l in o.get("lines") or []:
            if isinstance(l, dict) and l.get("when"):
                try:
                    names_of(parse(l["when"]), c.cond_names)
                except ValueError as e:
                    faults.append("%s %s: line when: %s" % (fname, c.id, e))
        c.options, c.sets, c.calls, c.verbs = [], set(), [], []
        c.keyed = False
        for op in o.get("options") or []:
            if not isinstance(op, dict):
                continue
            check_names = []
            if op.get("check"):
                try:
                    names_of(parse(op["check"]), check_names)
                except ValueError as e:
                    faults.append("%s %s option %s: check: %s" % (fname, c.id, op.get("key"), e))
            c.cond_names += check_names
            if any(KEY_VARS.match(n) for n in check_names):
                c.keyed = True
            ev_verbs, cost_verbs = verbs_of(op.get("effects"), []), verbs_of(op.get("cost"), [])
            c.verbs += ev_verbs + ["cost:" + v for v in cost_verbs]
            c.options.append(option_kind(ev_verbs, cost_verbs))
            if op.get("then"):
                c.calls.append((op["then"], ""))
            for v in ev_verbs:
                t = v.split()
                if len(t) >= 2 and t[0] == "flag":
                    c.sets.add(t[1])
                elif len(t) >= 4 and t[0] == "schedule":
                    c.calls.append((t[1], t[5] if len(t) >= 6 and t[4] == "at" else ""))
        c.kind = "hard" if "hard" in c.options else "door" if "door" in c.options else "soft"
        cards.append(c)
        by_id[c.id] = c

flags_set = set()
for c in cards:
    flags_set |= c.sets
flags_read = collections.Counter()
for c in cards:
    for n in c.cond_names:
        if n.startswith("flag."):
            flags_read[n[5:]] += 1
outside = set(flags_read) - flags_set                  # read by a card, set by no card: the code's, or nobody's


def can_hold(c, flags, port=None):
    """Can the card's "when" hold in the release? flags: the flags that can be set (None: any). port: only there."""
    in_port = c.stage in ("harbour", "crew", "long")
    at_sea = c.stage in ("sea", "crew", "long") and port is None
    for y, m in times(c.when_names | ({"month"} if c.months else set())):
        if c.months and m not in c.months:
            continue
        if at_sea and ev(c.when, scene_sea(y, m, flags)) is not False:
            return True
        if in_port:
            for p in ([port] if port else RELEASE_PORTS):
                if ev(c.when, scene_port(p, y, m, flags)) is not False:
                    return True
    return False


for c in cards:
    c.static = can_hold(c, None)                             # by place and year alone
    c.own_door = c.arrival or c.by_sim or (c.stage == "harbour" and c.weight > 0) or (c.stage in ("sea", "crew", "long") and c.rate > 0)
    c.open = False
    c.called_by = []

for c in cards:
    for target, port in c.calls:
        if target in by_id:
            by_id[target].called_by.append((c, port))

# round by round: a card opens when its "when" can hold with the flags the open cards can set, and something brings it
changed = True
possible = set(outside)
while changed:
    changed = False
    for c in cards:
        if c.open or not c.static:
            continue
        brought = c.own_door or any(src.open and (port in ("", "here") or port in RELEASE_PORTS) for src, port in c.called_by)
        if brought and can_hold(c, possible):
            c.open = True
            changed = True
    new = set(outside)
    for c in cards:
        if c.open:
            new |= c.sets
    if new != possible:
        possible = new
        changed = True

for c in cards:
    if c.open:
        c.why = ""
    elif not c.static:
        c.why = "far"
    elif not c.own_door and not c.called_by:
        c.why = "nodoor"
    else:
        c.why = "cut"
    c.nodraw = c.open and c.stage == "long" and c.rate > 0 and not any(src.open for src, _ in c.called_by)

# ---------------------------------------------------------------- the table by file

COLS = ("cards", "open", "real", "door", "soft", "keyed", "far", "cut", "nodoor", "nodraw")


def row_of(group):
    r = collections.OrderedDict((k, 0) for k in COLS)
    for c in group:
        r["cards"] += 1
        if c.open:
            r["open"] += 1
            r[{"hard": "real", "door": "door", "soft": "soft"}[c.kind]] += 1
            r["keyed"] += 1 if c.keyed else 0
            r["nodraw"] += 1 if c.nodraw else 0
        else:
            r[c.why] += 1
    return r


def short(name):
    return name.replace("_793.json", "").replace(".json", "")


def print_table(title, rows):
    print("")
    print(title)
    print("| file | " + " | ".join(COLS) + " |")
    print("|---|" + "---:|" * len(COLS))
    for name, r in rows:
        print("| %s | %s |" % (name, " | ".join(str(r[k]) for k in COLS)))


print("CARD CENSUS, the first release: %d ports (%s), %d to %d, one life." % (len(RELEASE_PORTS), ", ".join(RELEASE_PORTS), FIRST_YEAR, LAST_YEAR))
print("%d card files, %d cards." % (len(files), len(cards)))
for f in faults:
    print("FAULT: " + f)

rows = [(short(f), row_of([c for c in cards if c.file == f])) for f in files]
print_table("BY FILE", rows)


def deck(c):
    if c.file.startswith("chain_"):
        return "the chains (chain_*)"
    if c.file in ("sea_793.json", "crew_793.json", "long_793.json", "harbour_793.json"):
        return short(c.file)
    return "the region decks"


decks = collections.OrderedDict()
for c in cards:
    decks.setdefault(deck(c), []).append(c)
drows = [(k, row_of(v)) for k, v in decks.items()]
drows.append(("ALL", row_of(cards)))
print_table("BY DECK", drows)

def ends(c):
    """Does a choice on this card set one of its chain's ending flags (ch_<id>_end_<name>)?"""
    return any(re.match(r"^ch_.+_end(_|$)", f) for f in c.sets)


chain_rows = []
for f in files:
    if not f.startswith("chain_"):
        continue
    group = [c for c in cards if c.file == f]
    n_open = sum(1 for c in group if c.open)
    chain_rows.append((n_open, len(group), short(f).replace("chain_", ""),
                       sum(1 for c in group if c.open and ends(c)), sum(1 for c in group if ends(c))))
whole = [n for n in chain_rows if n[0] == n[1]]
part = [n for n in chain_rows if 0 < n[0] < n[1]]
none = [n for n in chain_rows if n[0] == 0]
print("")
print("THE CHAINS: %d files. Every card can open: %d. Some can: %d. None can: %d." % (len(chain_rows), len(whole), len(part), len(none)))
print("| chain | cards open | cards that end it, open | of |")
print("|---|---:|---:|---:|")
for n in sorted(whole + part, key=lambda n: (-n[0], n[2])):
    print("| %s | %d of %d | %d | %d |" % (n[2], n[0], n[1], n[3], n[4]))
print("  can start but can never end in the release: " + (", ".join(n[2] for n in whole + part if n[3] == 0) or "none"))
print("  none open: " + ", ".join(n[2] for n in none))

# ---------------------------------------------------------------- conditions and effects


def tally(title, counter_all, counter_open, top):
    print("")
    print(title)
    print("| name | all cards | open cards |")
    print("|---|---:|---:|")
    for name, n in counter_all.most_common(top):
        print("| %s | %d | %d |" % (name, n, counter_open.get(name, 0)))


cond_all, cond_open, eff_all, eff_open = (collections.Counter() for _ in range(4))
no_model_all, no_model_open, dead_all, dead_open = (collections.Counter() for _ in range(4))
for c in cards:
    for n in c.cond_names:
        fam = family(n)
        cond_all[fam] += 1
        if NO_MODEL.match(n):
            no_model_all[fam] += 1
        if c.open:
            cond_open[fam] += 1
            if NO_MODEL.match(n):
                no_model_open[fam] += 1
    for v in c.verbs:
        fam = effect_family(v[5:]) + " (as a cost)" if v.startswith("cost:") else effect_family(v)
        eff_all[fam] += 1
        if c.open:
            eff_open[fam] += 1
        verb = fam.split()[0]
        if verb in DEAD_VERBS:
            dead_all[verb] += 1
            if c.open:
                dead_open[verb] += 1

tally("CONDITIONS MOST USED (every 'when' and 'check', counted each time a name is read)", cond_all, cond_open, 28)
tally("EFFECTS MOST USED (every option's cost and effects, a chance's branches too)", eff_all, eff_open, 30)

print("")
print("EFFECTS THE GAME DOES NOTHING WITH TODAY (GameEffects returns null): " +
      ", ".join("%s %d (open %d)" % (k, dead_all[k], dead_open.get(k, 0)) for k in DEAD_VERBS))
print("CONDITIONS THE GAME HAS NO MODEL FOR (they read 0): " +
      ", ".join("%s %d (open %d)" % (k, n, no_model_open.get(k, 0)) for k, n in no_model_all.most_common()))
print("me.* (people, faith, past, tongue, name, nerve, lore, troth): %d uses. The grammar has no such name yet."
      % sum(n for k, n in cond_all.items() if k.startswith("me.")))

# ---------------------------------------------------------------- how the open cards come

open_cards = [c for c in cards if c.open]
print("")
print("HOW THE %d OPEN CARDS COME" % len(open_cards))
kinds = collections.Counter()
for c in open_cards:
    if c.arrival:
        kinds["on arrival (every call where it holds)"] += 1
    elif c.by_sim:
        kinds["from the sea sim (a sail within hail)"] += 1
    elif c.stage == "harbour" and c.weight > 0:
        kinds["drawn per port call (weight)"] += 1
    elif c.stage in ("sea", "crew") and c.rate > 0:
        kinds["drawn per hour (%s, rate)" % c.stage] += 1
    elif c.stage == "long" and c.rate > 0:
        kinds["stage long with a rate (no draw in the scheduler today)"] += 1
    else:
        kinds["only as a follow-up (then or schedule)"] += 1
for k, n in kinds.most_common():
    print("  %4d  %s" % (n, k))
cooled = sorted(c.cooldown for c in open_cards if c.cooldown > 0)
print("  %4d  come once only ('once')" % sum(1 for c in open_cards if c.once))
print("  %4d  have a cool-down; the middle one is %s days" % (len(cooled), cooled[len(cooled) // 2] if cooled else 0))
print("  %4d  are a chain step (file chain_*), of which %d are drawn by weight and %d only follow" % (
    sum(1 for c in open_cards if c.file.startswith("chain_")),
    sum(1 for c in open_cards if c.file.startswith("chain_") and c.weight > 0),
    sum(1 for c in open_cards if c.file.startswith("chain_") and not c.own_door)))

# ---------------------------------------------------------------- the flags

print("")
print("FLAGS: %d set by cards, %d read by cards, %d read but set by no card (the code's, or nobody's)." % (len(flags_set), len(flags_read), len(outside)))
print("  the most read of those: " + ", ".join("%s %d" % (f, flags_read[f]) for f in sorted(outside, key=lambda f: (-flags_read[f], f))[:14]))
never_read = flags_set - set(flags_read)
fam = collections.Counter(family("flag." + f)[5:] for f in never_read)
print("  set but read by no card: %d (%s)" % (len(never_read), ", ".join("%s %d" % kv for kv in fam.most_common(8))))

# ---------------------------------------------------------------- the twelve ports

print("")
print("THE TWELVE PORTS (open harbour cards; 'own' can come at three of the twelve or fewer)")
print("| port | own | of them real | chain steps | any-port cards | sum of weights there | a visit card comes |")
print("|---|---:|---:|---:|---:|---:|---:|")
harbour_open = [c for c in open_cards if c.stage == "harbour"]
where = {}
for c in harbour_open:
    where[c.id] = [p for p in RELEASE_PORTS if can_hold(c, possible, p)]
for p in RELEASE_PORTS:
    own = [c for c in harbour_open if p in where[c.id] and len(where[c.id]) <= 3]
    wide = [c for c in harbour_open if p in where[c.id] and len(where[c.id]) > 3]
    lam = sum(c.weight for c in harbour_open if p in where[c.id] and not c.arrival)
    print("| %s | %d | %d | %d | %d | %.1f | %.0f in 100 |" % (
        p, len(own), sum(1 for c in own if c.kind == "hard"), sum(1 for c in own if c.file.startswith("chain_")),
        len(wide), lam, 100.0 * (1.0 - math.exp(-lam))))
print("(The sum takes every flag as set and every year as right, so it is a ceiling. Over 3, a card comes on every")
print(" call, and which one is by weight alone: the scheduler has no budget and no order of need.)")

# a first call: no flag set yet (a new game, or a port never seen), so only starts and loose cards can be drawn
print("")
print("A CALL WITH NO FLAG SET (what the visit draw can pick from before any card has been answered, in some year of")
print("the release: chain starts and loose cards only)")
print("| port | cards in the draw | sum of weights | the heaviest three (weight) |")
print("|---|---:|---:|---|")
drawn_open = [c for c in harbour_open if c.weight > 0 and not c.arrival]
for p in RELEASE_PORTS:
    first = [c for c in drawn_open if can_hold(c, set(), p)]
    first.sort(key=lambda c: (-c.weight, c.id))
    print("| %s | %d | %.1f | %s |" % (p, len(first), sum(c.weight for c in first),
                                       "; ".join("%s (%g)" % (c.id.replace("harbour.", ""), c.weight) for c in first[:3])))
bands = collections.Counter()
for c in drawn_open:
    bands["under 1" if c.weight < 1 else "1 to 5" if c.weight <= 5 else "over 5 to 20" if c.weight <= 20 else "over 20"] += 1
print("Weights of the %d open cards drawn per call: %s. The draw is one Poisson process (Scheduler.Draw): a card comes with"
      % (len(drawn_open), ", ".join("%s: %d" % (k, bands[k]) for k in ("under 1", "1 to 5", "over 5 to 20", "over 20"))))
print("1 - exp(-sum), and is card i with weight_i / sum. One call draws one card at most.")

sea = [c for c in open_cards if c.stage == "sea" and c.rate > 0]
crew = [c for c in open_cards if c.stage == "crew" and c.rate > 0]
print("")
print("THE SEA: %d open sea cards, rates summing to %.2f an hour; %d crew cards, %.2f an hour (a ceiling: most are"
      % (len(sea), sum(c.rate for c in sea), len(crew), sum(c.rate for c in crew)))
print(" shut by weather, hour or flag at any one time). Real stake: sea %d of %d, crew %d of %d." % (
    sum(1 for c in sea if c.kind == "hard"), len(sea), sum(1 for c in crew if c.kind == "hard"), len(crew)))

# one day under way, hour by hour, as the hourly draw would see it with no flag set: how many cards the scheduler
# alone would send (cool-downs, "once" and the director's gap of hours not counted)


def day_at_sea(label, weather):
    low_total = high_total = 0.0
    low_n = high_n = 0
    for h in range(24):
        S = scene_sea(794, 7, set())
        S.update({"under_way": 1.0, "level_kind": "ocean", "hour": float(h), "days_at_sea": 1.0, "crew": 4.0,
                  "health": 0.9, "heart": 0.7, "silver": 100.0, "debt": 480.0, "feast": 0.0, "fast": 0.0})
        S.update(weather)
        low = high = 0.0
        for c in sea + crew:
            v = ev(c.when, S)
            if c.months and 7 not in c.months:
                continue
            if v is True:
                low += c.rate
                high += c.rate
                if h == 12:
                    low_n += 1
                    high_n += 1
            elif v is None:
                high += c.rate
                if h == 12:
                    high_n += 1
        low_total += 1.0 - math.exp(-low)
        high_total += 1.0 - math.exp(-high)
    print("| %s | %d to %d | %.1f to %.1f |" % (label, low_n, high_n, low_total, high_total))


print("")
print("A DAY UNDER WAY, NO FLAG SET (July 794, the open sea, four men, a day out; low: only cards whose 'when' surely")
print("holds; high: also those that hang on something the census cannot know, such as the hold)")
print("| the day | cards open at noon | cards the hourly draw sends in 24 hours |")
print("|---|---:|---:|")
day_at_sea("fair: wind 6 m/s, no fog, no storm", {"wind": 6.0, "fog": 0.0, "storm": 0.0, "ice": 0.0})
day_at_sea("calm: wind 1 m/s", {"wind": 1.0, "fog": 0.0, "storm": 0.0, "ice": 0.0})
day_at_sea("fog: wind 3 m/s, fog 1", {"wind": 3.0, "fog": 1.0, "storm": 0.0, "ice": 0.0})
day_at_sea("gale: wind 20 m/s, storm 0.8", {"wind": 20.0, "fog": 0.0, "storm": 0.8, "ice": 0.0})
print("(EncounterRunner.chartSeaGapHours is 4 today: at most 6 drawn cards in a day at sea, whatever these sums say.)")
