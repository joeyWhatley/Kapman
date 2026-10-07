# -*- coding: utf-8 -*-
# One-off: teaches check_content.py the things BRIEF_2.md and CHAINS.md add (port.id, site acts, works, ships,
# scheduled cards that must exist, chains, the story files). Safe to run twice.
import io, os
p = os.path.join(os.path.dirname(os.path.abspath(__file__)), "check_content.py")
s = io.open(p, encoding="utf-8").read()
if "ACTS = {" in s:
    print("already patched"); raise SystemExit(0)

def rep(a, b):
    global s
    assert s.count(a) == 1, (s.count(a), a[:60])
    s = s.replace(a, b)

rep('"port.kind", "port.people",', '"port.id", "port.kind", "port.people",')
rep('TIES = {"kin", "friend", "rival", "owes", "serves", "feud", "loves", "taught"}',
    'TIES = {"kin", "friend", "rival", "owes", "serves", "feud", "loves", "taught"}\n'
    'ACTS = set("load repair careen hire_porters hire_pilot wait_wind ask_harbour haggle weigh_silver sample pay_toll show_charter change_coin "\n'
    '           "test_silver alms hear_mass ask_news read_text copy_text vow sanctuary bury remember offer ask_omen gift feast oath audience "\n'
    '           "lawsuit witness hear_law rumours hire_crew dice tafl song riddles drink buy_tools commission buy_ship build_ship fit_upgrade "\n'
    '           "store rent_store buy_plot lodge heal provision buy_at_source cross fund_work".split())\n'
    'ship_classes = set()\n'
    '_sc = load(os.path.join(DATA, "ship_classes_793.json")) or {}\n'
    'for _v in _sc.values() if isinstance(_sc, dict) else []:\n'
    '    if isinstance(_v, list):\n'
    '        ship_classes |= {x.get("id") for x in _v if isinstance(x, dict) and x.get("id")}\n'
    'scheduled = []')
# a scheduled card must exist
rep('            if verb == "give_riddle" and len(e.split()) > 1 and e.split()[1] not in riddles:',
    '            if verb == "schedule" and len(e.split()) > 1:\n'
    '                scheduled.append((path, where, e.split()[1]))\n'
    '            if verb == "flag" and len(e.split()) > 1 and not re.match(r"^[a-z0-9_]+$", e.split()[1]):\n'
    '                err(path, "%s: flag name \'%s\' must be letters, digits and underscores only" % (where, e.split()[1]))\n'
    '            if verb == "give_riddle" and len(e.split()) > 1 and e.split()[1] not in riddles:')
rep('# ------------------------------------------------------------------------------------------------ places',
    'for path, where, target in scheduled:\n'
    '    if target not in card_ids:\n'
    '        err(path, "%s: schedules the card \'%s\', which does not exist" % (where, target))\n'
    'chains = {}\n'
    'for cid, f in card_ids.items():\n'
    '    if f.startswith("chain_"):\n'
    '        chains[f] = chains.get(f, 0) + 1\n'
    'for f, n in sorted(chains.items()):\n'
    '    if n < 6:\n'
    '        err(os.path.join(DATA, "encounters", f), "a chain of only %d cards (CHAINS.md asks for eight to fourteen)" % n)\n'
    'for path in sorted(glob.glob(os.path.join(DATA, "story", "*.json"))):\n'
    '    load(path)\n\n'
    '# ------------------------------------------------------------------------------------------------ places')
rep('                if isinstance(act, str) and act.startswith("card:") and act[5:] not in card_ids:\n'
    '                    warn(path, "%s: card \'%s\' does not exist (yet)" % (sid, act[5:]))',
    '                if isinstance(act, str) and act.startswith("card:") and act[5:] not in card_ids:\n'
    '                    warn(path, "%s: card \'%s\' does not exist (yet)" % (sid, act[5:]))\n'
    '                elif isinstance(act, str) and act.startswith("act:") and act[4:] not in ACTS:\n'
    '                    err(path, "%s: \'%s\' is not an act in BRIEF_2.md" % (sid, act))\n'
    '                elif isinstance(act, str) and act.startswith("person:"):\n'
    '                    site_people.append((path, sid, act[7:]))\n'
    '                elif isinstance(act, str) and act != "market" and not act.startswith(("card:", "act:")):\n'
    '                    err(path, "%s: \'%s\' is not market, person:, card: or act:" % (sid, act))\n'
    '            if not s.get("do"):\n'
    '                warn(path, "%s: nothing to do at this site" % sid)')
rep('people, sites, stories, seen_ports = {}, {}, {}, {}', 'people, sites, stories, seen_ports = {}, {}, {}, {}\nsite_people, works = [], {}')
rep('        for st in p.get("stories", []):',
    '        for w in p.get("works", []):\n'
    '            wid = w.get("id") or "(no id)"\n'
    '            if wid in works:\n'
    '                err(path, "work id \'%s\' is also in %s" % (wid, works[wid]))\n'
    '            works[wid] = os.path.basename(path)\n'
    '            if not isinstance(w.get("cost_d"), int) or not w.get("name") or not w.get("what") or not w.get("gives") or not w.get("source"):\n'
    '                err(path, "%s: a work needs a name, what, an integer cost_d, gives and a source" % wid)\n'
    '        for sh in p.get("ships", []):\n'
    '            if ship_classes and sh.get("class") not in ship_classes:\n'
    '                err(path, "%s: ship class \'%s\' is not in ship_classes_793.json" % (pid, sh.get("class")))\n'
    '        for st in p.get("stories", []):')
rep('spine = load(os.path.join(ROOT, "Design", "content", "spine.json"))',
    'for path, sid, who in site_people:\n'
    '    if who not in people:\n'
    '        err(path, "%s: person \'%s\' is in no places file" % (sid, who))\n'
    'spine = load(os.path.join(ROOT, "Design", "content", "spine.json"))')
rep('print("checked: %d ports in places, %d sites, %d people, %d stories, %d cards, %d writing keys; %d files with errors" %\n'
    '      (len(seen_ports), len(sites), len(people), len(stories), len(card_ids), len(writing), len(errors)))',
    'print("checked: %d ports in places, %d sites, %d people, %d stories, %d works, %d cards, %d chains, %d writing keys; %d files with errors" %\n'
    '      (len(seen_ports), len(sites), len(people), len(stories), len(works), len(card_ids), len(chains), len(writing), len(errors)))')
io.open(p, "w", encoding="utf-8").write(s)
print("checker patched")
