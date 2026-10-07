# -*- coding: utf-8 -*-
r"""
CHECK CONTENT - lints the game's written content (Design/content/BRIEF.md): the places files, the encounter card files
and the writing files. Prints errors by file and exits 1 if there are any. It is a lint, not the game's own loader:
the EditMode tests (EncounterTests) are the last word on cards.
  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\check_content.py [--warn-ties]
"""
import glob, io, json, os, re, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
errors, warnings = {}, {}


def err(path, msg):
    errors.setdefault(os.path.relpath(path, ROOT), []).append(msg)


def warn(path, msg):
    warnings.setdefault(os.path.relpath(path, ROOT), []).append(msg)


def load(path):
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            return json.load(f)
    except Exception as e:
        err(path, "does not parse as JSON: %s" % e)
        return None


def ids_in(path, key="id", container=None):
    d = load(path)
    if d is None:
        return set()
    items = d.get(container, []) if container else d
    if isinstance(items, dict):
        return set(items.keys())
    return {x.get(key) for x in items if isinstance(x, dict)}


ports = ids_in(os.path.join(DATA, "ports_793.json"), container="ports")
goods = ids_in(os.path.join(DATA, "goods_793.json"), container="goods")
texts = ids_in(os.path.join(DATA, "texts_793.json"), container="texts")
riddles = ids_in(os.path.join(DATA, "riddles_793.json"), container="riddles")
roles_doc = load(os.path.join(DATA, "encounters", "npc_roles_793.json")) or {}
roles = set((roles_doc.get("roles") or {}).keys())
peoples = set((roles_doc.get("peoples") or {}).keys())
rep = load(os.path.join(DATA, "reputation_793.json")) or {}
deeds = set()
for k in ("deeds", "kinds"):
    v = rep.get(k)
    if isinstance(v, dict):
        deeds |= set(v.keys())
    elif isinstance(v, list):
        deeds |= {x.get("id") for x in v if isinstance(x, dict)}

VERBS = {"silver", "goods", "health", "morale", "men", "deed", "news", "price_shock", "commission", "give_text", "give_riddle", "open_fetch",
         "hire", "pay_off", "passenger", "encounter", "law", "learn", "tongue", "toll", "flag", "schedule", "npc"}
EXACT = {"silver", "debt", "debt_due_days", "standing", "standing_region", "standing_word", "crew", "crew_min", "crew_full", "health", "heart",
         "days_at_sea", "month", "year", "hour", "wind", "fog", "storm", "ice", "feast", "fast", "moored", "under_way", "level_kind",
         "port.id", "port.kind", "port.people", "port.faith", "port.realm", "port.has_mint", "port.toll", "port.visits", "port.best_feeling",
         "hold.weapons", "hold.captives", "hold.value", "crew.kind", "days_since_paid", "venture_ended", "learned.latin", "learned.runes",
         "chest.latin_count", "chest.riddles_solved", "chest.rune_count", "reform.coin", "generation", "true", "false", "and", "or", "not"}
FAMILY = re.compile(r"^(hold\.from_[a-z0-9_]+|hold\.[a-z0-9_]+|tongue\.[a-z0-9_]+|crew\.[a-z0-9_]+_(best|count)|"
                    r"npc\.[a-z0-9_]+\.(feeling|met|owed|generation|last)|text\.[a-z0-9_:.\-]+|riddle\.[a-z0-9_:.\-]+|flag\.[a-z0-9_]+)$")
STAGES = {"harbour", "sea", "crew", "long"}
SITE_KINDS = {"quay", "beach", "market", "toll", "mint", "church", "minster", "abbey", "hall", "thing", "smithy", "shipyard", "tavern", "well", "gate",
              "wall", "ferry", "shrine", "grove", "mound", "graveyard", "workshop", "storehouse", "field", "fishery", "saltern", "quarry", "bridge",
              "ford", "lodging"}
TIES = {"kin", "friend", "rival", "owes", "serves", "feud", "loves", "taught"}
ACTS = set("load repair careen hire_porters hire_pilot wait_wind ask_harbour haggle weigh_silver sample pay_toll show_charter change_coin "
           "test_silver alms hear_mass ask_news read_text copy_text vow sanctuary bury remember offer ask_omen gift feast oath audience "
           "lawsuit witness hear_law rumours hire_crew dice tafl song riddles drink buy_tools commission buy_ship build_ship fit_upgrade "
           "store rent_store buy_plot lodge heal provision buy_at_source cross fund_work".split())
ship_classes = set()
_sc = load(os.path.join(DATA, "ship_classes_793.json")) or {}
for _v in _sc.values() if isinstance(_sc, dict) else []:
    if isinstance(_v, list):
        ship_classes |= {x.get("id") for x in _v if isinstance(x, dict) and x.get("id")}
scheduled = []


def lint_condition(path, where, text):
    if not isinstance(text, str) or not text.strip():
        return
    for name in re.findall(r"[A-Za-z_][A-Za-z0-9_.]*", re.sub(r"'[^']*'|\"[^\"]*\"", " ", text)):
        left_of_op = re.search(re.escape(name) + r"\s*(=|!=|<|>|<=|>=|$|\)|\s+(and|or))", text)
        if name in EXACT or FAMILY.match(name):
            continue
        # a bare word on the right of a comparison is a value (franks, wary, declare, hide.failure ...): let it pass
        if re.search(r"(=|!=|<|>|<=|>=)\s*" + re.escape(name) + r"\b", text):
            continue
        if left_of_op:
            err(path, "%s: unknown name '%s' in condition \"%s\"" % (where, name, text))


def lint_effects(path, where, effects):
    for e in effects or []:
        if isinstance(e, str):
            verb = e.split()[0] if e.split() else ""
            if verb not in VERBS:
                err(path, "%s: unknown effect verb '%s' in \"%s\"" % (where, verb, e))
            if verb == "deed" and deeds and len(e.split()) > 1 and e.split()[1] not in deeds:
                err(path, "%s: deed '%s' is not in reputation_793.json" % (where, e.split()[1]))
            if verb == "goods" and len(e.split()) > 1 and e.split()[1] not in goods:
                err(path, "%s: good '%s' is not in goods_793.json" % (where, e.split()[1]))
            if verb == "give_text" and len(e.split()) > 1 and e.split()[1] not in texts:
                err(path, "%s: text '%s' is not in texts_793.json" % (where, e.split()[1]))
            if verb == "schedule" and len(e.split()) > 1:
                scheduled.append((path, where, e.split()[1]))
            if verb == "flag" and len(e.split()) > 1 and not re.match(r"^[a-z0-9_]+$", e.split()[1]):
                err(path, "%s: flag name '%s' must be letters, digits and underscores only" % (where, e.split()[1]))
            if verb == "give_riddle" and len(e.split()) > 1 and e.split()[1] not in riddles:
                err(path, "%s: riddle '%s' is not in riddles_793.json" % (where, e.split()[1]))
        elif isinstance(e, dict):
            if "chance" in e:
                lint_effects(path, where + " (success)", e.get("success"))
                lint_effects(path, where + " (failure)", e.get("failure"))
            else:
                err(path, "%s: an effect object without 'chance': %s" % (where, json.dumps(e)[:80]))


# ------------------------------------------------------------------------------------------------ writing
writing = {}
main = load(os.path.join(DATA, "writing.json")) or {}
for e in main.get("entries", []):
    writing[e.get("key")] = ("writing.json", e.get("text", ""))
for path in sorted(glob.glob(os.path.join(DATA, "writing", "*.json"))):
    d = load(path)
    if d is None:
        continue
    for e in d.get("entries", []):
        k, t = e.get("key"), e.get("text", "")
        if not k:
            err(path, "an entry without a key")
            continue
        if not isinstance(t, str) or not t.strip():
            err(path, "'%s' has no text" % k)
        elif "[write:" in t:
            err(path, "'%s' is still a placeholder" % k)
        if k in writing and writing[k][0] != "writing.json":
            err(path, "'%s' is also in %s" % (k, writing[k][0]))
        writing[k] = (os.path.basename(path), t)

# ------------------------------------------------------------------------------------------------ cards
card_ids = {}
for path in sorted(glob.glob(os.path.join(DATA, "encounters", "*.json"))):
    if os.path.basename(path) in ("npc_roles_793.json", "names_placeholder.json", "names_793.json"):
        continue
    d = load(path)
    if d is None:
        continue
    for c in d.get("cards", []):
        cid = c.get("id") or "(no id)"
        if cid in card_ids:
            err(path, "card id '%s' is also in %s" % (cid, card_ids[cid]))
        card_ids[cid] = os.path.basename(path)
        if c.get("stage") not in STAGES:
            err(path, "%s: stage '%s' is not one of %s" % (cid, c.get("stage"), sorted(STAGES)))
        lint_condition(path, cid, c.get("when"))
        for name, a in (c.get("actors") or {}).items():
            if isinstance(a, dict) and a.get("role") and roles and a["role"] not in roles:
                err(path, "%s: role '%s' is not in npc_roles_793.json" % (cid, a["role"]))
        for field in ("title_key", "intention_key"):
            k = c.get(field)
            if k and k not in writing:
                err(path, "%s: %s '%s' has no writing entry" % (cid, field, k))
        for ln in c.get("lines", []):
            if ln.get("key") not in writing:
                err(path, "%s: line key '%s' has no writing entry" % (cid, ln.get("key")))
            lint_condition(path, cid + " line", ln.get("when"))
        opts = c.get("options", [])
        if not (1 <= len(opts) <= 4):
            err(path, "%s: %d options (a card has two or three; one or four at most)" % (cid, len(opts)))
        for o in opts:
            if o.get("label_key") and o["label_key"] not in writing:
                err(path, "%s: label_key '%s' has no writing entry" % (cid, o["label_key"]))
            if not o.get("label"):
                err(path, "%s: option '%s' has no plain label" % (cid, o.get("key")))
            lint_condition(path, cid + " option " + str(o.get("key")), o.get("check"))
            lint_effects(path, cid + " option " + str(o.get("key")), o.get("effects"))

for path, where, target in scheduled:
    if target not in card_ids:
        err(path, "%s: schedules the card '%s', which does not exist" % (where, target))
chains = {}
for cid, f in card_ids.items():
    if f.startswith("chain_"):
        chains[f] = chains.get(f, 0) + 1
for f, n in sorted(chains.items()):
    if n < 6:
        err(os.path.join(DATA, "encounters", f), "a chain of only %d cards (CHAINS.md asks for eight to fourteen)" % n)
for path in sorted(glob.glob(os.path.join(DATA, "story", "*.json"))):
    load(path)

# ------------------------------------------------------------------------------------------------ places
people, sites, stories, seen_ports = {}, {}, {}, {}
site_people, works = [], {}
ties = []
for path in sorted(glob.glob(os.path.join(DATA, "places", "*.json"))):
    d = load(path)
    if d is None:
        continue
    for p in d.get("ports", []):
        pid = p.get("port")
        if pid not in ports:
            err(path, "port '%s' is not in ports_793.json" % pid)
        if pid in seen_ports:
            err(path, "port '%s' is also in %s" % (pid, seen_ports[pid]))
        seen_ports[pid] = os.path.basename(path)
        for f in ("summary", "seasons"):
            if not isinstance(p.get(f), str) or len(p.get(f, "")) < 20:
                err(path, "%s: '%s' is missing or too short" % (pid, f))
        if len(p.get("history", [])) < 2:
            err(path, "%s: fewer than two dated facts in 'history'" % pid)
        for h in p.get("history", []):
            if not isinstance(h.get("year"), int) or not h.get("text") or not h.get("source"):
                err(path, "%s: a history entry needs an integer year, text and source: %s" % (pid, json.dumps(h, ensure_ascii=False)[:80]))
        if len(p.get("sites", [])) < 3:
            err(path, "%s: fewer than three sites" % pid)
        for s in p.get("sites", []):
            sid = s.get("id") or "(no id)"
            if sid in sites:
                err(path, "site id '%s' is also in %s" % (sid, sites[sid]))
            sites[sid] = os.path.basename(path)
            if s.get("kind") not in SITE_KINDS:
                err(path, "%s: site kind '%s' is not in the brief's list" % (sid, s.get("kind")))
            if not s.get("see") or not s.get("name"):
                err(path, "%s: a site needs a name and what is seen" % sid)
            for act in s.get("do", []):
                if isinstance(act, str) and act.startswith("card:") and act[5:] not in card_ids:
                    warn(path, "%s: card '%s' does not exist (yet)" % (sid, act[5:]))
                elif isinstance(act, str) and act.startswith("act:") and act[4:] not in ACTS:
                    err(path, "%s: '%s' is not an act in BRIEF_2.md" % (sid, act))
                elif isinstance(act, str) and act.startswith("person:"):
                    site_people.append((path, sid, act[7:]))
                elif isinstance(act, str) and act != "market" and not act.startswith(("card:", "act:")):
                    err(path, "%s: '%s' is not market, person:, card: or act:" % (sid, act))
            if not s.get("do"):
                warn(path, "%s: nothing to do at this site" % sid)
        if len(p.get("people", [])) < 3:
            err(path, "%s: fewer than three people" % pid)
        for m in p.get("people", []):
            mid = m.get("id") or "(no id)"
            if mid in people:
                err(path, "person id '%s' is also in %s" % (mid, people[mid]))
            people[mid] = os.path.basename(path)
            for f in ("name", "role", "people", "faith", "trait", "want", "fear", "source"):
                if not m.get(f):
                    err(path, "%s: no '%s'" % (mid, f))
            if not isinstance(m.get("historical"), bool):
                err(path, "%s: 'historical' must be true or false" % mid)
            if not isinstance(m.get("lines"), dict) or not m["lines"].get("greet"):
                err(path, "%s: no lines (at least 'greet')" % mid)
            for t in m.get("ties", []):
                if t.get("tie") not in TIES:
                    err(path, "%s: tie '%s' is not one of %s" % (mid, t.get("tie"), sorted(TIES)))
                ties.append((path, mid, t.get("to")))
        for w in p.get("works", []):
            wid = w.get("id") or "(no id)"
            if wid in works:
                err(path, "work id '%s' is also in %s" % (wid, works[wid]))
            works[wid] = os.path.basename(path)
            if not isinstance(w.get("cost_d"), int) or not w.get("name") or not w.get("what") or not w.get("gives") or not w.get("source"):
                err(path, "%s: a work needs a name, what, an integer cost_d, gives and a source" % wid)
        for sh in p.get("ships", []):
            if ship_classes and sh.get("class") not in ship_classes:
                err(path, "%s: ship class '%s' is not in ship_classes_793.json" % (pid, sh.get("class")))
        for st in p.get("stories", []):
            sid = st.get("id") or "(no id)"
            if sid in stories:
                err(path, "story id '%s' is also in %s" % (sid, stories[sid]))
            stories[sid] = os.path.basename(path)
            if not st.get("beats"):
                err(path, "%s: a story needs beats" % sid)
            for b in st.get("beats", []):
                lint_condition(path, sid + " beat", b.get("when"))

for path, sid, who in site_people:
    if who not in people:
        err(path, "%s: person '%s' is in no places file" % (sid, who))
spine = load(os.path.join(ROOT, "Design", "content", "spine.json")) if os.path.exists(os.path.join(ROOT, "Design", "content", "spine.json")) else None
cast = {c.get("id") for c in (spine or {}).get("cast", [])}
for path, who, to in ties:
    if to not in people and to not in cast:
        (warn if "--strict-ties" not in sys.argv else err)(path, "%s: tie to '%s', who is in no places file and not in the spine's cast" % (who, to))

for bucket, label in ((errors, "ERRORS"), (warnings, "warnings")):
    for f in sorted(bucket):
        print("%s in %s:" % (label, f))
        for m in bucket[f][:60]:
            print("   " + m)
        if len(bucket[f]) > 60:
            print("   ... and %d more" % (len(bucket[f]) - 60))
print("checked: %d ports in places, %d sites, %d people, %d stories, %d works, %d cards, %d chains, %d writing keys; %d files with errors" %
      (len(seen_ports), len(sites), len(people), len(stories), len(works), len(card_ids), len(chains), len(writing), len(errors)))
sys.exit(1 if errors else 0)
