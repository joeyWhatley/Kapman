# -*- coding: utf-8 -*-
r"""
CARD RULES - a side file for the rulings of 7 October 2026 (Design/DECISIONS.md, "later the same day", 5, 10, 12, 13).
It edits no card. It reads every card in Assets/Kapman/Game/Data/encounters and writes one row for each to
Bake/content/out/card_rules_793.json:

  urgency         "ordinary" or "pressing" (a raider, a squall, a man overboard)
  holds_ship      a pressing card that keeps her where she is until it is answered
  patience_hours  168 for an ordinary card (a week of running clock); a pressing card's own time, 1 to 72
  default         the option the card takes by itself when that time runs out: its index, key and label
  default_why     a few words
  fallbacks       the other options, in the order to try them if the default's check does not hold on the day
  speaker         who is speaking: a person's id (places file, spine, or people of weight) or else the role
  portrait        the art slot for that face: the person's own, else the role's stand-in, else null
  faces_by_port   for a card that comes at several ports: the person who holds the speaker's role at each
  speaker_likely  a person who probably is the speaker (the same role at the same port) where the card's own note
                  calls the speaker its own invention: shown as a stand-in until someone says they are one person
  picture         the card's own art slot; picture_stand_in is card/card.any.<stage>.png
  open            can it open in the first release at all (Bake/content/card_census.py decides)
  by              "hand" where a person read the card (Bake/content/card_rules_overrides.json), else "rule"

THE DEFAULT BY RULE is the option that does least. Each option is weighed from its cost and its effects: silver and
goods spent or gained, a deed set, a person or the crew harmed, a chain ended, a promise taken on. The lightest option
that can always be chosen (no check) wins. An option that puts the matter off (it sets a ..._wait flag or schedules
its own card again) and the option the card itself marks "default" weigh less; a gamble that hurts when it is lost
weighs more. A tie goes to the last option.
A PRESSING card is different: its default is what happens if the skipper freezes, so among the free options the rule
takes the one with the worst odds. The rule is a first cut. Every card that can open in the first release was read,
and where the rule was wrong the overrides file corrects it.

WHO SPEAKS. The speaker is the actor with the most lines (the earliest on a tie). A person is found, in this order:
the actor's note cites an id (cast.thiadbod, ripa.hroald); the note begins with the name of a person of the card's
ports; the actor's key is a person's name; a seat aboard (steersman, mate, lookout, and the Dane: the spine's first
crew); a fixed actor whose role one person holds at the card's port in its years.

  & "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\card_rules.py
  ... card_rules.py --sheet <file> [--all] [--only <text in the id>]   a reading sheet of the open cards
  ... card_rules.py --faults <file>                                    the faults a machine can find, by card id
"""
import collections, contextlib, glob, io, json, os, re, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
ENC = os.path.join(DATA, "encounters")
OUT = os.path.join(HERE, "out", "card_rules_793.json")
OVERRIDES = os.path.join(HERE, "card_rules_overrides.json")

ORDINARY_HOURS = 168                 # ruling 12: an ordinary card waits a week
PRESSING_MIN, PRESSING_MAX = 1, 72   # ruling 10: hours or a few days
FIRST_YEAR, LAST_YEAR = 793, 805
GOOD_VOYAGE = 300                    # Design/GAME_MAKER.md 4.1: "a leg makes 10 to 40 d and a good voyage 100 to 300 d"
WINE_CROSSING = 150                  # the same page: "a wine crossing clears about 150 d"
OWN_DEFAULT = 40                     # how much lighter the card's own "default" option weighs
PUT_OFF = 30                         # how much lighter an option weighs that puts the matter off


def load(path):
    with io.open(path, "r", encoding="utf-8-sig") as f:
        return json.load(f)


# ---------------------------------------------------------------- the census: which cards can open, and where

sys.path.insert(0, HERE)
with contextlib.redirect_stdout(io.StringIO()):
    import card_census as census
CENSUS = {c.id: c for c in census.cards}
RELEASE_PORTS = census.RELEASE_PORTS


def release_ports_of(cid):
    """The ports of the twelve where an open card can come (empty for a card at sea, or shut)."""
    c = CENSUS.get(cid)
    if c is None or not c.open or c.stage == "sea":
        return []
    if cid in census.where:
        return list(census.where[cid])
    if any(n.startswith("port.") for n in c.when_names):
        return [p for p in RELEASE_PORTS if census.can_hold(c, census.possible, p)]
    return []


# ---------------------------------------------------------------- the cards and their words

cards = []
for path in sorted(glob.glob(os.path.join(ENC, "*.json"))):
    doc = load(path)
    if isinstance(doc, dict) and isinstance(doc.get("cards"), list):
        for o in doc["cards"]:
            if isinstance(o, dict) and o.get("id"):
                o["_file"] = os.path.basename(path)
                cards.append(o)

WRITING = {}
for e in load(os.path.join(DATA, "writing.json")).get("entries", []):
    if e.get("key"):
        WRITING[e["key"]] = e.get("text") or ""
for path in sorted(glob.glob(os.path.join(DATA, "writing", "*.json"))):
    try:
        for e in load(path).get("entries", []):                # as merge_writing.py: a writer's file wins where it has words
            if e.get("key") and isinstance(e.get("text"), str) and e["text"].strip() and "[write:" not in e["text"]:
                WRITING[e["key"]] = e["text"]
    except Exception as ex:                                   # a writer's file that does not parse is not ours to mend
        print("writing/%s skipped: %s" % (os.path.basename(path), ex))


def written(key):
    """The user's words for a key, or None while unwritten (as EncounterBook.Written)."""
    t = WRITING.get(key or "")
    if t is None or not t.strip() or t.lstrip().startswith("[write:"):
        return None
    return t


def keys_of(card):
    """Every writing key the card shows: title, intention, lines, option labels."""
    out = [card.get("title_key"), card.get("intention_key")]
    out += [l.get("key") for l in card.get("lines") or []]
    out += [o.get("label_key") for o in card.get("options") or []]
    return [k for k in out if k]


def words_ready(card):
    ks = keys_of(card)
    return sum(1 for k in ks if written(k) is not None), len(ks)


# ---------------------------------------------------------------- weighing an option

def verbs_of(node, p=1.0, branch=""):
    """Every verb of an effects node with the chance it runs and its branch ('' sure, 'success', 'failure')."""
    if node is None:
        return
    if isinstance(node, list):
        for x in node:
            for v in verbs_of(x, p, branch):
                yield v
    elif isinstance(node, dict):
        ch = node.get("chance")
        ch = float(ch) if isinstance(ch, (int, float)) else 0.5
        for v in verbs_of(node.get("success"), p * ch, "success"):
            yield v
        for v in verbs_of(node.get("failure"), p * (1.0 - ch), "failure"):
            yield v
    elif isinstance(node, str) and node.strip():
        yield node.strip(), p, branch


def num(s):
    try:
        return float(s)
    except (TypeError, ValueError):
        return 0.0


def first_chance(node):
    if isinstance(node, list):
        for x in node:
            c = first_chance(x)
            if c is not None:
                return c
    elif isinstance(node, dict) and "chance" in node:
        return node
    return None


class Weighed(object):
    """What one option does, added up: the score is how much it changes."""

    def __init__(self):
        self.score = 0.0
        self.silver_out = self.silver_in = 0.0          # the most it can spend and the most it can gain, in pennies
        self.goods_out = self.goods_in = 0
        self.men = 0
        self.deeds, self.harm, self.ends, self.promises, self.gifts, self.far = [], [], [], [], [], []
        self.advances = False                           # sets a flag, schedules a card or names a "then"
        self.puts_off = False                           # sets a ..._wait flag or schedules its own card again
        self.law = False
        self.chance = None                              # the first chance's number, or None
        self.checked = False                            # has a check that may not hold
        self.empty = False                              # no cost, no effect, no "then"
        self.risky = False                              # a chance whose failure costs silver, goods, health, heart, a man or a friend

    def add(self, points):
        self.score += points


def hurts(t):
    """Does this verb (its words) take something away: silver, goods, health, heart, a man, the law, a friend?"""
    verb = t[0]
    if verb in ("silver", "health", "morale"):
        return num(t[1]) < 0
    if verb in ("goods", "men"):
        return num(t[-1]) < 0
    if verb == "npc":
        return len(t) > 3 and num(t[3]) <= -5
    return verb == "law"


RISK = 40               # how much heavier a gamble weighs on an ordinary card when losing it hurts


def weigh(option, card_id=""):
    w = Weighed()
    check = (option.get("check") or "").strip()
    w.checked = check not in ("", "true")
    cost = option.get("cost")
    cost = cost if isinstance(cost, list) else ([cost] if cost else [])
    effects = option.get("effects")
    ch = first_chance(effects)
    if ch is not None:
        w.chance = float(ch.get("chance", 0.5)) if isinstance(ch.get("chance"), (int, float)) else 0.5
        w.add(15)                                        # a gamble is never "doing least"
    listed = [(c, 1.0, "cost") for c in cost if isinstance(c, str)] + list(verbs_of(effects))
    w.empty = not listed and not option.get("then")
    sure_in = sure_out = 0.0
    for text, p, branch in listed:
        t = text.split()
        verb = t[0]
        if branch == "cost":                             # a cost is written positive and is paid
            if verb == "silver":
                sure_out += num(t[1]); w.add(num(t[1]))
            elif verb == "goods":
                w.goods_out += int(num(t[-1])); w.add(15 * num(t[-1]))
            elif verb == "men":
                w.men -= int(num(t[1])); w.add(150 * num(t[1])); w.harm.append("a man")
            continue
        if branch == "failure" and hurts(t):
            w.risky = True
        if verb == "silver":
            n = num(t[1])
            if n < 0:
                if p == 1.0:
                    sure_out += -n
                else:
                    w.silver_out = max(w.silver_out, -n)
            else:
                if p == 1.0:
                    sure_in += n
                else:
                    w.silver_in = max(w.silver_in, n)
            w.add(p * abs(n))
        elif verb == "goods":
            n = int(num(t[-1]))
            if n < 0:
                w.goods_out = max(w.goods_out, -n)
            else:
                w.goods_in = max(w.goods_in, n)
            w.add(p * 15 * abs(n))
        elif verb == "men":
            n = int(num(t[1]))
            w.men += n if p == 1.0 else 0
            w.add(p * 150 * abs(n))
            if n < 0:
                w.harm.append("a man")
        elif verb == "health":
            n = num(t[1])
            w.add(p * (300 * -n if n < 0 else 60 * n))
            if n < 0:
                w.harm.append("health")
        elif verb == "morale":
            n = num(t[1])
            w.add(p * (120 * -n if n < 0 else 40 * n))
            if n <= -0.1:
                w.harm.append("heart")
        elif verb == "deed":
            w.deeds.append(t[1] if len(t) > 1 else "?"); w.add(p * 40)
        elif verb == "law":
            w.law = True; w.add(p * 200); w.harm.append("the law")
        elif verb == "npc":
            n = num(t[3]) if len(t) > 3 else 0.0
            w.add(p * (3 * -n if n < 0 else 0.6 * n))
            if n <= -5:
                w.harm.append(t[1])
        elif verb == "flag":
            name = t[1] if len(t) > 1 else ""
            w.advances = True
            if re.match(r"^ch_.+_end(_|$)", name):
                w.ends.append(name); w.add(p * 300)
            elif name.startswith("unlock_") or name.startswith("changed_"):
                w.gifts.append(name); w.add(p * 25)
            elif re.search(r"_wait\d*$", name):
                w.puts_off = True; w.add(p * 2)
            elif name.startswith("ch_"):
                w.add(p * 4)
            else:
                w.add(p * 2)
        elif verb == "schedule":
            w.advances = True; w.add(p * 6)
            if len(t) > 1 and t[1] == card_id:
                w.puts_off = True
            if len(t) >= 6 and t[4] == "at" and t[5] not in ("here",) and t[5] not in RELEASE_PORTS:
                w.far.append(t[5])
        elif verb == "commission":
            w.promises.append("an order"); w.add(p * 60)
        elif verb in ("hire", "pay_off"):
            w.promises.append(verb); w.add(p * 80)
        elif verb == "passenger":
            w.promises.append("a passenger"); w.add(p * 50)
            if len(t) >= 4 and t[3] not in ("here",) and t[3] not in RELEASE_PORTS:
                w.far.append(t[3])
        elif verb in ("give_text", "give_riddle", "learn", "tongue", "open_fetch"):
            w.gifts.append(verb); w.add(p * 12)
        elif verb == "news":
            w.add(p * 3)
        elif verb == "price_shock":
            w.add(p * 30)
        elif verb == "toll":
            w.add(p * (25 if t[1:2] == ["none"] else 20 if t[1:2] == ["eased"] else 0))
        elif verb == "encounter":
            w.add(p * 15)
    w.silver_out += sure_out
    w.silver_in += sure_in
    if option.get("then"):
        w.advances = True
        w.add(6)
    if w.checked:
        w.add(8)
    return w


def weigh_card(card):
    """Every option weighed; a chain step's dead end (nothing set, nothing promised) counts as ending the chain."""
    opts = card.get("options") or []
    ws = [weigh(o, card["id"]) for o in opts]
    c = CENSUS.get(card["id"])
    only_once = bool(card.get("once")) or (c is not None and not c.own_door)
    if card["_file"].startswith("chain_") and only_once and any(w.advances for w in ws):
        for w in ws:
            if not w.advances and not w.ends:
                w.ends.append("(a dead end: nothing set, nothing promised)")
                w.add(120)
    return ws


def plain_default(option, w):
    """The option the card itself marks "default" (Enter on a card seen before), if it is cheap and breaks nothing."""
    return bool(option.get("default")) and w.silver_out <= 12 and w.goods_out == 0 and not w.law and not w.ends and w.men >= 0


def order_by_rule(card, ws):
    """Option indexes, the one that does least first: unchecked before checked, then the lightest, then the last."""
    opts = card.get("options") or []

    def key(i):
        w = ws[i]
        light = w.score - (OWN_DEFAULT if plain_default(opts[i], w) else 0) - (PUT_OFF if w.puts_off and not w.ends else 0)
        light += RISK if w.risky else 0
        return (1 if w.checked else 0, round(light, 3), 0 if opts[i].get("default") else 1, -i)
    return sorted(range(len(opts)), key=key)


def order_when_frozen(card, ws):
    """A pressing card: among the free options (no cost, no check) the one with the worst odds comes first."""
    opts = card.get("options") or []

    def key(i):
        w = ws[i]
        free = not w.checked and not opts[i].get("cost")
        return (0 if free else 1, w.chance if w.chance is not None else 1.0, round(w.score, 3), -i)
    return sorted(range(len(opts)), key=key)


def why_by_rule(card, ws, i):
    w, others = ws[i], [x for j, x in enumerate(ws) if j != i]
    said = []
    if w.puts_off:
        said.append("It puts the matter off; the card comes again")
    elif (card.get("options") or [])[i].get("default"):
        said.append("The card's own usual answer")
    if w.silver_out == 0 and w.silver_in == 0 and w.goods_out == 0 and w.goods_in == 0:
        said.append("no silver or goods change hands")
    else:
        bits = []
        if w.silver_out:
            bits.append("spends %d d" % round(w.silver_out))
        if w.silver_in:
            bits.append("gains %d d" % round(w.silver_in))
        if w.goods_out:
            bits.append("gives %d of goods" % w.goods_out)
        if w.goods_in:
            bits.append("takes %d of goods" % w.goods_in)
        said.append("the least on the card (" + ", ".join(bits) + ")")
    if not w.deeds and any(x.deeds for x in others):
        said.append("no deed")
    if not w.harm and any(x.harm for x in others):
        said.append("no one harmed")
    if not w.ends and any(x.ends for x in others):
        said.append("the chain stays open")
    if w.ends:
        said.append("every choice ends this chain" if all(x.ends for x in others) else "it ends the chain")
    if w.chance is not None:
        said.append("a chance of %d in 100" % round(100 * w.chance))
    text = "; ".join(said) + "."
    return text[0].upper() + text[1:]


# ---------------------------------------------------------------- urgency by rule

PRESSING_WORDS = [                                       # (a word in a sea card's id, hours): a first cut
    ("overboard", 1), ("squall", 1), ("splits", 1), ("shifts", 1), ("gale", 1), ("surf", 2), ("bar", 2), ("storm", 2),
    ("raider", 3), ("raiders", 3), ("chase", 3), ("fog", 3), ("leak", 3), ("ice", 6), ("sick", 24), ("thirst", 24),
]


def urgency_by_rule(card):
    """(urgency, holds_ship, hours, why) from the card's stage, id and effects: a first cut only."""
    if card.get("rate") == "traffic":
        return "pressing", True, 2, "a sail within hail"
    for o in card.get("options") or []:
        for text, _, _ in verbs_of(o.get("effects")):
            if text.split()[0] == "encounter":
                return "pressing", True, 2, "a ship met at sea"
    if card.get("stage") == "sea":
        words = re.split(r"[^a-z]+", card["id"].lower())
        for word, hours in PRESSING_WORDS:
            if word in words:
                return "pressing", True, hours, "the word '%s' in its id" % word
    return "ordinary", False, ORDINARY_HOURS, ""


# ---------------------------------------------------------------- people: the places files, the spine, people of weight

ART = load(os.path.join(ROOT, "Art", "art_slots.json"))["slots"]
ART_FILES = set(v["file"] for v in ART.values() if not v.get("retired"))

PEOPLE = {}                                              # places id -> person
BY_PORT = collections.defaultdict(list)
for path in sorted(glob.glob(os.path.join(DATA, "places", "*.json"))):
    try:
        doc = load(path)
    except Exception as ex:
        print("places/%s skipped: %s" % (os.path.basename(path), ex))
        continue
    for port in doc.get("ports", []):
        pid = port.get("port") if isinstance(port.get("port"), str) else (port.get("port") or {}).get("id", "")
        for p in port.get("people", []):
            if p.get("id"):
                p["_port"] = pid
                PEOPLE[p["id"]] = p
                BY_PORT[pid].append(p)

CAST = {}                                                # cast id -> row of the spine
CAST_OF_PLACE = {}                                       # places id -> cast id
for c in load(os.path.join(ROOT, "Design", "content", "spine.json")).get("cast", []):
    CAST[c["id"]] = c
    if c.get("place_id"):
        CAST_OF_PLACE[c["place_id"]] = c["id"]
CAST_TAIL = {cid.split(".")[-1]: cid for cid in CAST}

WEIGHT = {}                                              # wt.<name> -> row
try:
    for p in load(os.path.join(DATA, "story", "weight_793.json")).get("people", []):
        if p.get("id"):
            WEIGHT[p["id"]] = p
except Exception as ex:
    print("weight_793.json skipped: %s" % ex)

SEATS = {}                                               # a seat's role -> the cast member who sits in it on the first day
for cid, c in CAST.items():
    if cid.startswith("cast.crew.") and c.get("role") in ("steersman", "mate", "lookout"):
        SEATS[c["role"]] = cid
THE_DANE = "cast.crew.ravn" if "cast.crew.ravn" in CAST else ""     # crew_793.json "about": the seat 'dane' (role crewman)

ROLES = load(os.path.join(ENC, "npc_roles_793.json")).get("roles", {})


def person_id(any_id):
    """One id for a person however the card cites them: the places id where there is one, else the cast or wt id."""
    if any_id in CAST:
        pid = CAST[any_id].get("place_id") or ""
        return pid if pid in PEOPLE else any_id
    if any_id in WEIGHT:
        w = WEIGHT[any_id]
        if w.get("place_id") in PEOPLE:
            return w["place_id"]
        if w.get("cast") in CAST:
            return person_id(w["cast"])
    return any_id


def ids_of(pid):
    """Every id a person goes by."""
    out = [pid]
    if pid in CAST_OF_PLACE:
        out.append(CAST_OF_PLACE[pid])
    if pid in CAST and CAST[pid].get("place_id"):
        out.append(CAST[pid]["place_id"])
    for w in WEIGHT.values():
        if w["id"] == pid or (w.get("place_id") and w["place_id"] in out) or (w.get("cast") and w["cast"] in out):
            out.append(w["id"])
    return out


def portrait_of_person(pid):
    """The art slot of a person, by any of the ids the person goes by; None if no slot is listed."""
    tries = ids_of(pid)
    for t in tries:
        f = "portrait/person.%s.png" % t
        if f in ART_FILES:
            return f
    for t in tries:
        if t.startswith("wt."):
            f = "portrait/weight.%s.png" % t[3:]
            if f in ART_FILES:
                return f
    return None


def portrait_of_role(role):
    f = "portrait/role.%s.png" % role
    return f if f in ART_FILES else None


def name_of(pid):
    for t in ids_of(pid):
        for table in (PEOPLE, CAST, WEIGHT):
            if t in table and table[t].get("name"):
                return table[t]["name"]
    return ""


def years_of(card):
    """The years the card's "when" allows, read off its plain year terms (a first cut: an 'or' is not followed)."""
    lo, hi = 793, 1066
    when = card.get("when") or ""
    if " or " in when:
        return lo, hi
    for op, n in re.findall(r"\byear\s*(>=|<=|>|<|=)\s*(\d{3,4})", when):
        n = int(n)
        if op == ">=":
            lo = max(lo, n)
        elif op == ">":
            lo = max(lo, n + 1)
        elif op == "<=":
            hi = min(hi, n)
        elif op == "<":
            hi = min(hi, n - 1)
        else:
            lo, hi = max(lo, n), min(hi, n)
    return lo, hi


def ports_named(card):
    """The ports the card's "when" names (port.id = x)."""
    return sorted(set(re.findall(r"\bport\.id\s*=\s*([a-z_]+)", card.get("when") or "")))


def holders(port, role, lo, hi):
    """The people of a places file who hold a role at a port in some year of lo..hi."""
    return [p for p in BY_PORT.get(port, []) if p.get("role") == role and p.get("from", 0) <= hi and p.get("to", 9999) >= lo]


CITED = re.compile(r"\b(cast\.crew\.[a-z_]+|cast\.[a-z_]+|wt\.[a-z_]+|[a-z_]+\.[a-z_]+)\b")
OWN = re.compile(r"\b(invented|forged|unnamed|of the forge|the forge names)\b", re.IGNORECASE)
NAME_FIRST = re.compile(r"^\s*(?:the\s+)?([A-ZÆ][a-zæðþ]{2,}(?:\s[A-ZÆ][a-zæðþ]{2,})?)\b")
NOT_NAMES = {"The", "King", "Saint", "Frisian", "Frank", "Dane", "English", "Kentish", "Mercian", "Saxon", "Thiadbod's"}


def speaker_of(card, is_open):
    """
    Who speaks, and how we know. Returns a dict: actor (the card's key), role, person (an id or ""), how, by_port,
    likely (an id or ""), own_name (a name the card's note gives to a person of its own).
    """
    actors = card.get("actors") or {}
    said = collections.Counter()
    order = []
    for l in card.get("lines") or []:
        w = l.get("who") or ""
        if w and w in actors:
            said[w] += 1
            if w not in order:
                order.append(w)
    out = {"actor": "", "role": "", "person": "", "how": "no actor on the card", "by_port": {}, "likely": "", "own_name": ""}
    if order:
        actor = max(order, key=lambda k: (said[k], -order.index(k)))
    elif actors:
        actor = next(iter(actors))
    else:
        return out
    spec = actors[actor] or {}
    role = spec.get("role") or actor
    note = spec.get("note") or ""
    fixed = spec.get("fixed", True) is not False
    out.update({"actor": actor, "role": role, "how": ""})
    lo, hi = years_of(card)
    if is_open:
        lo, hi = max(lo, FIRST_YEAR), min(hi, LAST_YEAR)
        ports = release_ports_of(card["id"]) if card.get("stage") != "sea" else []
    else:
        ports = ports_named(card)

    def alive(pid):
        p = PEOPLE.get(pid)
        return p is None or (p.get("from", 0) <= hi and p.get("to", 9999) >= lo)

    # 1. the note cites an id (cast.thiadbod; ripa.hroald; "cast.wighelm at Lundenwic; dofras.reeve at Dofras")
    cited = []
    for m in CITED.finditer(note):
        t = m.group(1)
        if t in CAST or t in PEOPLE or t in WEIGHT:
            pid = person_id(t)
            if pid not in cited:
                cited.append(pid)
    if cited:
        here = [pid for pid in cited if PEOPLE.get(pid, {}).get("_port") in ports] if len(cited) > 1 and ports else cited
        here = [pid for pid in (here or cited) if alive(pid)]
        cited = here
    if cited:
        if len(here) == 1 or not ports:
            out["person"], out["how"] = here[0], "the card's note names " + here[0]
            return out
        for pid in here:
            if PEOPLE.get(pid, {}).get("_port"):
                out["by_port"][PEOPLE[pid]["_port"]] = pid
        out["how"] = "the card's note names a person for each port"
        return out

    # 2. the note begins with a name: a person of the card's ports, a person of weight, or the card's own person
    m = NAME_FIRST.match(note)
    first = m.group(1) if m and m.group(1).split()[0] not in NOT_NAMES else ""
    if first:
        pool = [p for port in ports for p in BY_PORT.get(port, [])]
        hit = [p for p in pool if p.get("name") in (first, first.split()[0]) and alive(p["id"])]
        if len(hit) == 1:
            out["person"], out["how"] = hit[0]["id"], "the card's note names %s" % hit[0]["name"]
            return out
        wt = [w for w in WEIGHT.values() if w.get("name") and (w["name"] == first or w["id"][3:] in [x.lower() for x in first.split()])]
        if not wt:                                               # "Eadberht, later called Praen; historical": a byname further on
            head = note.split(";")[0]
            tails = set(w.lower() for w in re.findall(r"\b([A-ZÆ][a-zæðþ]{2,})\b(?!'s)", head))
            wt = [w for w in WEIGHT.values() if w["id"][3:] in tails]
        if len(wt) == 1:
            out["person"], out["how"] = person_id(wt[0]["id"]), "the card's note names %s, a person of weight" % wt[0]["name"]
            return out
        out["own_name"] = first

    # 3. the actor's key is a person's name
    if actor not in ROLES and not out["own_name"]:
        if actor in CAST_TAIL:
            out["person"], out["how"] = person_id(CAST_TAIL[actor]), "the actor's key is the cast's " + CAST_TAIL[actor]
            return out
        if ("wt." + actor) in WEIGHT:
            out["person"], out["how"] = person_id("wt." + actor), "the actor's key is a person of weight"
            return out
        pool = [p for port in ports for p in BY_PORT.get(port, [])]
        hit = [p for p in pool if (p["id"].split(".")[-1] == actor or p.get("name", "").lower() == actor) and alive(p["id"])]
        if len(hit) == 1:
            out["person"], out["how"] = hit[0]["id"], "the actor's key is that person"
            return out

    # 4. a seat aboard: the spine's first crew (crew_793.json, "THE SEATS")
    aboard = card.get("stage") in ("crew", "sea") or card["id"].startswith(("crew.", "sea."))
    if aboard and not out["own_name"] and not OWN.search(note):
        seat = SEATS.get(role, "")
        if role == "crewman" and THE_DANE and (actor in ("dane", "ravn") or "Dane" in note):
            seat = THE_DANE
        if seat:
            out["person"] = person_id(seat)
            out["how"] = "a seat aboard: %s sits there on the first day (another man if he is paid off)" % CAST[seat]["name"]
            return out

    # 5. a fixed actor whose role one person holds at the card's port in its years
    if fixed and not out["own_name"]:
        found = {}
        for port in ports:
            hs = holders(port, role, lo, hi)
            if len(hs) > 1:
                cast = [p for p in hs if p["id"] in CAST_OF_PLACE]
                hs = cast if len(cast) == 1 else sorted(hs, key=lambda p: p.get("from", 0))[:1]
            if len(hs) == 1:
                found[port] = hs[0]["id"]
        if found and len(ports) == 1:
            pid = found[ports[0]]
            if OWN.search(note):
                out["likely"] = pid
                out["how"] = "the card's note calls the speaker its own; %s holds this role at %s" % (name_of(pid), ports[0])
            else:
                out["person"], out["how"] = pid, "the %s at %s in those years" % (role.replace("_", " "), ports[0])
            return out
        if found:
            if OWN.search(note):
                out["how"] = "the card's note calls the speaker its own"
            else:
                out["by_port"] = found
                out["how"] = "the %s of each port" % role.replace("_", " ")
            return out
    if out["own_name"]:
        out["how"] = "the card's own person, %s: no one of the places files" % out["own_name"]
    elif not fixed:
        out["how"] = "a passing %s, a new face each time" % role.replace("_", " ")
    else:
        out["how"] = "no person of the places files holds this role here"
    return out


# ---------------------------------------------------------------- faults a machine can find

MODERN = [                                               # Design/content/lore/period_words.md, part (b): whole words
    "hello", "hi", "hey", "goodbye", "bye", "please", "sorry", "okay", "ok", "yeah", "nope", "wow", "cool", "awesome",
    "sir", "madam", "mister", "guys", "folks", "kid", "boss", "buddy", "actually", "basically", "literally", "obviously",
    "definitely", "absolutely", "honestly", "thee", "thou", "thy", "prithee", "mayhap", "methinks", "verily",
    "minute", "minutes", "o'clock", "hour", "hours", "decade", "century", "weekend", "holiday",
    "vacation", "medieval", "money", "cash", "funds", "profit", "profits", "margin", "business", "company", "firm",
    "enterprise", "investment", "capital", "stock", "interest", "loan", "credit", "banker", "customer", "client",
    "customs", "tariff", "tax", "taxes", "salary", "wages", "payroll", "contract", "paperwork", "receipt", "invoice",
    "licence", "license", "permit", "passport", "signature", "paper", "percent", "average", "zero", "economy",
    "inflation", "import", "export", "wholesale", "retail", "smuggling", "smuggler", "smugglers", "captain", "skipper",
    "starboard", "deck", "cabin", "galley", "helm", "compass", "chart", "map", "latitude", "bearing", "degrees", "knots",
    "mile", "miles", "kilometres", "lighthouse", "telescope", "spyglass", "logbook", "navy", "marines", "admiral",
    "flagship", "drakkar", "longboat", "cog", "galleon", "cannon", "gunpowder", "musket", "hammock", "rum", "grog",
    "scurvy", "viking", "vikings", "england", "france", "french", "germany", "germans", "netherlands", "dutch", "belgium",
    "norway", "sweden", "denmark", "scotland", "russia", "russians", "byzantium", "byzantines", "charlemagne", "europe",
    "nation", "national", "patriot", "citizen", "foreigner", "immigrant", "ethnic", "soldier", "soldiers",
    "troops", "officer", "uniform", "knight", "castle", "feudal", "serf", "peasant", "police", "prison", "jail", "judge",
    "lawyer", "trial", "evidence", "crime", "murder", "rights", "equality", "democracy", "vote", "pagan", "paganism",
    "religion", "christianity", "catholic", "cathedral", "parish", "vicar", "bible", "rosary", "crusade", "luck", "lucky",
    "destiny", "magic", "spell", "germs", "infection", "virus", "hygiene", "doctor", "medicine", "surgery", "stress",
    "trauma", "depressed", "mental", "ego", "personality", "career", "job", "profession", "hobby", "team", "staff",
    "colleague", "manager", "interesting", "amazing", "fun", "problem", "situation", "option", "issue", "focus",
    "information", "data", "breakfast", "lunch", "dinner", "potato", "potatoes", "sugar", "tea", "coffee", "pocket",
    "pockets", "button", "buttons", "clock", "chimney",
]
MODERN_RE = re.compile(r"(?<![A-Za-z'])(%s)(?![A-Za-z])" % "|".join(re.escape(w) for w in MODERN), re.IGNORECASE)
# words of part (b) with an older sense as well ("a second boat", "the river bank", "to judge"): not flagged by machine
TWO_SENSES = {"knots", "staff", "interest", "stock", "firm", "company", "bearing", "trial", "spell", "focus", "issue", "fun", "paper",
              "map", "chart", "judge", "average", "capital", "credit", "evidence", "option", "job", "tea", "cool", "hi",
              "thy", "hey", "ok", "kid", "team", "data", "sir", "murder", "rights", "vote", "castle", "deck", "bye",
              "helm", "problem", "situation", "pocket", "pockets", "button", "buttons", "clock", "galley", "import",
              "export", "cash", "funds", "margin", "permit", "contract", "zero", "nation", "magic", "medicine", "doctor"}


def faults_of(card, ws):
    """Faults a machine can see on one card: (kind, words)."""
    out = []
    opts = card.get("options") or []
    for i, o in enumerate(opts):
        if ws[i].empty:
            out.append(("a choice with no effect", "'%s' (%s) costs nothing and does nothing" % (o.get("key"), o.get("label", ""))))
        if ws[i].silver_in >= GOOD_VOYAGE:
            out.append(("pays more than a good voyage", "'%s' can pay %d d (a good voyage is 100 to 300 d)" % (o.get("key"), round(ws[i].silver_in))))
        elif ws[i].silver_in >= WINE_CROSSING:
            out.append(("pays as much as a crossing", "'%s' can pay %d d (a wine crossing clears about 150 d)" % (o.get("key"), round(ws[i].silver_in))))
        for port in sorted(set(ws[i].far)):
            out.append(("leads out of the twelve ports", "'%s' sends the next step to %s" % (o.get("key"), port)))
    if opts and all(w.checked for w in ws):
        out.append(("no choice is always open", "every choice has a check: on a bad day none can be taken"))
    missing = [k for k in keys_of(card) if written(k) is None]
    if missing:
        out.append(("words missing", "%d of %d: %s" % (len(missing), len(keys_of(card)), ", ".join(missing[:6]) + (" ..." if len(missing) > 6 else ""))))
    hits = collections.OrderedDict()
    for k in keys_of(card):
        t = written(k)
        if not t:
            continue
        for m in MODERN_RE.finditer(t):
            word = m.group(1).lower()
            if word not in TWO_SENSES:
                hits.setdefault(word, []).append(k)
    for word, where in hits.items():
        out.append(("a modern word", "'%s' in %s" % (word, ", ".join(sorted(set(where))[:4]))))
    return out


# ---------------------------------------------------------------- put together

def build():
    over = load(OVERRIDES) if os.path.exists(OVERRIDES) else {}
    overrides = over.get("cards", {})
    read = set(over.get("read_and_right", []))
    known = set(c["id"] for c in cards)
    problems = ["overrides: no card '%s'" % k for k in list(overrides) + sorted(read) if k not in known]
    rows = []
    for card in cards:
        cid = card["id"]
        opts = card.get("options") or []
        keys = [o.get("key") for o in opts]
        ws = weigh_card(card)
        is_open = bool(CENSUS.get(cid) and CENSUS[cid].open)
        urgency, holds, hours, urgency_why = urgency_by_rule(card)
        ov = overrides.get(cid, {})
        by = "hand" if (ov or cid in read) else "rule"
        if "urgency" in ov:
            urgency = ov["urgency"]
            hours = hours if hours != ORDINARY_HOURS else 6
        if urgency == "pressing":
            hours = max(PRESSING_MIN, min(PRESSING_MAX, ov.get("patience_hours", hours)))
            holds = bool(ov.get("holds_ship", True))
        else:
            hours, holds = ORDINARY_HOURS, False
        order = order_when_frozen(card, ws) if urgency == "pressing" else order_by_rule(card, ws)
        pick = order[0] if order else -1
        why = why_by_rule(card, ws, pick) if pick >= 0 else ""
        if urgency == "pressing" and by == "rule" and pick >= 0:
            why = "By rule, not read: the free choice with the worst odds." if ws[pick].chance is not None else "By rule, not read: the free choice that does least."
        if "default" in ov:
            if ov["default"] in keys:
                pick = keys.index(ov["default"])
                rest = [keys.index(k) for k in ov.get("fallbacks", []) if k in keys and keys.index(k) != pick]
                order = [pick] + rest + [i for i in order if i != pick and i not in rest]
                why = ov.get("why") or why_by_rule(card, ws, pick)
            else:
                problems.append("overrides: %s has no option '%s'" % (cid, ov["default"]))
            for k in ov.get("fallbacks", []):
                if k not in keys:
                    problems.append("overrides: %s has no option '%s' (fallbacks)" % (cid, k))
        elif "why" in ov:
            why = ov["why"]
        sp = speaker_of(card, is_open)
        if "speaker" in ov:
            s = ov["speaker"]
            if s.startswith("role:"):
                sp.update({"person": "", "likely": "", "by_port": {}, "role": s[5:], "how": "by hand: no one of the places files"})
            else:
                sp.update({"person": person_id(s), "likely": "", "by_port": {}, "how": "by hand"})
                if person_id(s) not in PEOPLE and s not in CAST and s not in WEIGHT:
                    problems.append("overrides: %s names an unknown speaker '%s'" % (cid, s))
        person = sp["person"]
        portrait = portrait_of_person(person) if person else None
        face = "named" if portrait else "none"
        if not portrait:
            portrait = portrait_of_role(sp["role"]) if sp["role"] else None
            face = "stand-in" if portrait else "none"
        by_port = collections.OrderedDict()
        for port, pid in sorted(sp["by_port"].items()):
            by_port[port] = collections.OrderedDict([("person", pid), ("name", name_of(pid)), ("portrait", portrait_of_person(pid) or portrait)])
        own = "card/card.%s.png" % cid
        row = collections.OrderedDict()
        row["id"] = cid
        row["open"] = is_open
        row["urgency"] = urgency
        row["holds_ship"] = holds
        row["patience_hours"] = hours
        row["default"] = None if pick < 0 else collections.OrderedDict(
            [("index", pick), ("key", opts[pick].get("key", "")), ("label", opts[pick].get("label", ""))])
        row["default_why"] = why
        row["fallbacks"] = order[1:]
        row["speaker"] = person or sp["role"] or None
        row["speaker_name"] = name_of(person) if person else sp["own_name"]
        row["speaker_role"] = sp["role"] or None
        row["speaker_actor"] = sp["actor"] or None
        row["speaker_how"] = sp["how"]
        row["portrait"] = portrait
        row["face"] = face
        if sp["likely"]:
            row["speaker_likely"] = collections.OrderedDict([("person", sp["likely"]), ("name", name_of(sp["likely"])), ("portrait", portrait_of_person(sp["likely"]))])
        if by_port:
            row["faces_by_port"] = by_port
        row["picture"] = own if own in ART_FILES else None
        row["picture_stand_in"] = "card/card.any.%s.png" % card.get("stage", "harbour")
        row["by"] = by
        if ov.get("note"):
            row["note"] = ov["note"]
        rows.append((card, ws, row, urgency_why))
    return rows, problems


def counts(rows):
    lines = []
    for label, pick in (("ALL %d CARDS" % len(rows), lambda r: True), ("THE CARDS THAT CAN OPEN IN THE FIRST RELEASE", lambda r: r["open"])):
        rs = [r for _, _, r, _ in rows if pick(r)]
        n = len(rs)
        pressing = [r for r in rs if r["urgency"] == "pressing"]
        lines.append(label if label.startswith("ALL") else "%s: %d" % (label, n))
        lines.append("  ordinary %d, pressing %d (of the pressing, %d hold the ship)" % (n - len(pressing), len(pressing), sum(1 for r in pressing if r["holds_ship"])))
        named = [r for r in rs if r["face"] == "named"]
        standin = [r for r in rs if r["face"] == "stand-in"]
        lines.append("  face: a named person %d, a role stand-in %d, none %d" % (len(named), len(standin), sum(1 for r in rs if r["face"] == "none")))
        how = collections.Counter()
        for r in named:
            h = r["speaker_how"]
            how["the card's note names the person" if h.startswith("the card's note names") else
                "a seat aboard (the first crew)" if h.startswith("a seat aboard") else
                "the actor's key is a name" if h.startswith("the actor's key") else
                "by hand" if h.startswith("by hand") else "the role at the card's one port"] += 1
        lines.append("    named, by: " + ", ".join("%s %d" % kv for kv in how.most_common()))
        lines.append("    of the stand-ins: %d have a named person at some of their ports (faces_by_port), %d have a likely person (speaker_likely), %d are the card's own named person" % (
            sum(1 for r in standin if r.get("faces_by_port")), sum(1 for r in standin if r.get("speaker_likely")),
            sum(1 for r in standin if r["speaker_how"].startswith("the card's own person"))))
        lines.append("  default: read by hand %d, by rule only %d; the default is the card's own 'default' option on %d of the %d cards that mark one" % (
            sum(1 for r in rs if r["by"] == "hand"), sum(1 for r in rs if r["by"] == "rule"),
            sum(1 for c, _, r, _ in rows if pick(r) and r["default"] and (c["options"][r["default"]["index"]].get("default"))),
            sum(1 for c, _, r, _ in rows if pick(r) and any(o.get("default") for o in c.get("options") or []))))
        lines.append("  a picture of its own (a slot in art_slots.json): %d" % sum(1 for r in rs if r["picture"]))
    return lines


def short_effects(node):
    """Effects on one line for the sheet: a chance as {70%: ... | else: ...}; feelings and flags shortened."""
    if node is None:
        return "-"
    if isinstance(node, str):
        t = node.split()
        if t[0] == "npc" and len(t) > 3:
            return "%s%s" % (t[1], t[3])
        if t[0] == "flag":
            name = t[1] if len(t) > 1 else ""
            keep = re.match(r"^ch_.+_end(_|$)", name) or name.startswith(("unlock_", "changed_")) or re.search(r"_wait\d*$", name)
            return "f:" + " ".join(t[1:]) if keep else "f"
        if t[0] == "schedule":
            return "sched " + " ".join(t[1:]).replace("harbour.", "h.").replace("chain_", "")
        return node
    if isinstance(node, dict):
        ch = node.get("chance")
        return "{%s%%: %s | else: %s}" % (round(100 * ch) if isinstance(ch, (int, float)) else "?", short_effects(node.get("success")), short_effects(node.get("failure")))
    out = []
    for x in node:
        s = short_effects(x)
        if s == "f" and out and out[-1].startswith("f") and not out[-1].startswith("f:"):
            continue
        out.append(s)
    return "; ".join(out) if out else "-"


def sheet(rows, path, everything, only):
    """A reading sheet: each card in a few lines, with the script's answer, for correcting by hand."""
    out = []
    for card, ws, row, urgency_why in rows:
        if not everything and not row["open"]:
            continue
        if only and only not in card["id"]:
            continue
        ready, total = words_ready(card)
        ports = release_ports_of(card["id"])
        head = "## %s  [%s] %s%s%s%s  words %d/%d" % (
            card["id"], card.get("stage"), card["_file"].replace("_793.json", ""),
            "  ARRIVAL" if card.get("arrival") else "", "  w=%s" % card.get("weight") if card.get("weight") else "",
            "  rate=%s" % card.get("rate") if card.get("rate") else "", ready, total)
        out.append(head + ("  once" if card.get("once") else "") + ("  cd=%s" % card.get("cooldown_days") if card.get("cooldown_days") else "") +
                   ("  ports=" + ",".join(ports) if 0 < len(ports) < 12 else "  ports=all twelve" if ports else ""))
        out.append("when: " + (card.get("when") or ""))
        intent = written(card.get("intention_key")) or card.get("intention", "")
        out.append("T: %s | %s" % (written(card.get("title_key")) or card.get("title", ""), intent if len(intent) <= 300 else intent[:300] + "..."))
        for i, o in enumerate(card.get("options") or []):
            w = ws[i]
            check = (o.get("check") or "").strip()
            cost = o.get("cost")
            out.append("  %d%s [%s] %s%s%s%s => %s  (%.0f)" % (
                i, "*" if o.get("default") else " ", o.get("key"), written(o.get("label_key")) or o.get("label", ""),
                "  {if %s}" % check if check not in ("", "true") else "", "  cost %s" % json.dumps(cost) if cost else "",
                "  then %s" % o["then"] if o.get("then") else "", short_effects(o.get("effects")), w.score))
        d = row["default"]
        out.append("=> %s%s  default=%s%s  [%s]  speaker=%s (%s: %s) face=%s%s%s" % (
            row["urgency"], " %dh hold=%s %s" % (row["patience_hours"], row["holds_ship"], "(%s)" % urgency_why if urgency_why else "") if row["urgency"] == "pressing" else "",
            "%d %s" % (d["index"], d["key"]) if d else "-", " then %s" % row["fallbacks"] if row["fallbacks"] else "", row["by"],
            row["speaker"], row["speaker_actor"], row["speaker_how"], row["face"],
            "  likely=" + row["speaker_likely"]["person"] if row.get("speaker_likely") else "",
            "  by_port=" + ",".join("%s:%s" % (k, v["person"]) for k, v in row.get("faces_by_port", {}).items()) if row.get("faces_by_port") else ""))
        fs = faults_of(card, ws)
        if fs:
            out.append("FAULTS: " + " | ".join("%s: %s" % f for f in fs))
        out.append("")
    with io.open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out))
    return len(out)


def faults_file(rows, path):
    by_kind = collections.OrderedDict()
    for card, ws, row, _ in rows:
        if not row["open"]:
            continue
        for kind, words in faults_of(card, ws):
            by_kind.setdefault(kind, []).append("%s: %s" % (card["id"], words))
    with io.open(path, "w", encoding="utf-8", newline="\n") as f:
        for kind, items in by_kind.items():
            f.write("## %s (%d)\n" % (kind, len(items)))
            f.write("\n".join(items) + "\n\n")
    return collections.OrderedDict((k, len(v)) for k, v in by_kind.items())


def main(argv):
    rows, problems = build()
    doc = collections.OrderedDict()
    doc["about"] = (
        "CARD RULES for the rulings of 7 October 2026 (Design/DECISIONS.md 5, 10, 12, 13). Written by Bake/content/card_rules.py "
        "from the card files, the census (card_census.py), the places files, the spine, weight_793.json and Art/art_slots.json; "
        "hand corrections are kept in Bake/content/card_rules_overrides.json. Do not edit this file: edit the overrides and run "
        "the script. One row a card. urgency: ordinary (waits 168 running hours, then takes its default) or pressing (holds the "
        "ship, takes its default after patience_hours). default: the option taken when the time runs out, with its index in the "
        "card's options, its key and its plain label; fallbacks: the other options in the order to try them if the default's "
        "check does not hold on the day (if none can be taken, the card goes on waiting). speaker: a person's id (a places "
        "file's, the spine's cast.<id>, or wt.<id>) or else the role; speaker_actor is the card's own key for that actor; "
        "speaker_name is the person's name, or the name the card's note gives to a person of its own. portrait: the art slot to "
        "show; face says whether it is a named person's, a role's stand-in, or none. faces_by_port: for a card that comes at "
        "several ports, the person who holds the role at each. speaker_likely: a person who probably is the speaker, not shown "
        "until someone confirms it. picture: the card's own slot; picture_stand_in: the slot to fall back to. open: can the card "
        "open in the first release (twelve ports, 793 to 805). by: 'hand' where a person read the card and set or confirmed the "
        "row, 'rule' where only the rule has spoken (every card shut in the first release is by rule)."
    )
    doc["ordinary_hours"] = ORDINARY_HOURS
    doc["cards"] = [row for _, _, row, _ in rows]
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with io.open(OUT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print("card_rules_793.json: %d cards written to %s" % (len(rows), OUT))
    for line in counts(rows):
        print(line)
    for p in problems:
        print("PROBLEM: " + p)
    press = [(r["patience_hours"], r["id"], r["default"]["key"] if r["default"] else "-", r["by"]) for _, _, r, _ in rows if r["open"] and r["urgency"] == "pressing"]
    print("THE PRESSING CARDS THAT CAN OPEN (%d), by their time:" % len(press))
    for hours, cid, key, by in sorted(press):
        print("  %3d h  %s  -> %s  [%s]" % (hours, cid, key, by))
    if "--sheet" in argv:
        path = argv[argv.index("--sheet") + 1]
        only = argv[argv.index("--only") + 1] if "--only" in argv else ""
        print("sheet: %d lines to %s" % (sheet(rows, path, "--all" in argv, only), path))
    if "--faults" in argv:
        path = argv[argv.index("--faults") + 1]
        print("faults among the open cards: " + ", ".join("%s %d" % kv for kv in faults_file(rows, path).items()))


if __name__ == "__main__":
    main(sys.argv[1:])
