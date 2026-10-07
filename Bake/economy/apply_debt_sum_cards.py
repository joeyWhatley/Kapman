# -*- coding: utf-8 -*-
r"""
THE DEBT'S SUM IN THE CARDS AND THEIR WORDS (ruling 30 of 7 Oct 2026: "Every place that says 480 d or two pounds must
change with it"). The table and the code were changed on 7 Oct (trade_rules.json start.debt 1920, SimRules.minMargin
0.25, story/self_793.json, the frame's bar and panels). THE CARDS WERE NOT: the count's own chain still says 480
pennies and two pounds, and lets the player count out 480. They were left because the content pass was rewriting those
same files on the branch `content` that day, and two hands on one line is a merge that somebody has to untangle.

RUN THIS ONCE THE CONTENT BRANCH IS MERGED:
  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\economy\apply_debt_sum_cards.py [--apply]
Without --apply it only lists every line it would change, old and new, for a human to read first. It changes:
  encounters/chain_house_debt_793.json   the flag ch_house_debt_owed, the checks and costs (480 -> 1920; by weight
                                         367 -> 1468, the sum times 1.3 / 1.7), the labels and the notes
  writing/chain_house_debt.json          the words of the same cards
  writing.json                           the same texts where they are merged in (keys chain_house_debt.*), and
                                         chain_year_1066.end_house.narr_boat ("a debt of two pounds to a count")
  encounters/crew_793.json, encounters/npc_roles_793.json   two notes that name the sum
It does not touch any other 480 or "two pounds" (a bride-price, another man's goods, loaves of two pounds, rewards of
silver +480: those are other sums, or the pay cap's). The stick's own count is changed where it is said in words:
"forty dozen" (480) becomes "eight score dozen" (1,920), and "eighteen score and seven" (367, the old sum by weight)
becomes "seventy-three score and eight" (1,468). NOT changed, because they are parts and not the sum, and a writer
must say what they are now: the widow's surety of 120 pennies ("six score": a quarter of the old debt), the hawk's price.

THE CARDS ALSO DO NOT PAY THE GAME'S DEBT: their "cost": "silver 480" takes the silver and sets the chain's own flag
(ch_house_debt_owed), but the debt the top bar shows (Trade.Debt) stays as it was. Whoever rewrites the chain must
join the two, or the bar will go on saying Owed after the count has been paid.

STILL FOR A WRITER, not for a script: the story of the sum. Two pounds was the price of the boat ("480 pennies lent
for the boat"; ships_roster.md prices her at 600 d). Eight pounds is three or four boats: what the count lent it for
(the father's debts, the boat and her first lading, a hard count) is the user's to write. This script only makes the
numbers agree. Afterwards run the EditMode tests (the cards' lint) and read the chain once in the game.
"""
import io, json, os, re, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
APPLY = "--apply" in sys.argv
OLD, NEW = 480, 1920
OLD_W, NEW_W = 367, 1468                    # by weight: the sum times 1.3 / 1.7 (the old penny against the new)
assert int(round(NEW * 1.3 / 1.7)) == NEW_W and int(round(OLD * 1.3 / 1.7)) == OLD_W

# (pattern, replacement), in order; each is tried on a line that the file's own rule lets through
WORDS = [
    (r"\btwo pounds\b", "eight pounds"),
    (r"\bTwo pounds\b", "Eight pounds"),
    # the stick's own count: forty dozen is 480; eight score dozen is 1,920. By weight: eighteen score and seven is 367;
    # seventy-three score and eight is 1,468
    (r"\bForty dozen\b", "Eight score dozen"),
    (r"\bforty dozen\b", "eight score dozen"),
    (r"\bEighteen score and seven\b", "Seventy-three score and eight"),
    (r"\beighteen score and seven\b", "seventy-three score and eight"),
    (r"\b480 pennies\b", "1,920 pennies"),
    (r"\b480 of the new pennies\b", "1,920 of the new pennies"),
    (r"\b480 new pennies\b", "1,920 new pennies"),
    (r"\b367 new pennies\b", "1,468 new pennies"),
    (r"\b367 pennies\b", "1,468 pennies"),
    (r"\b480 d\b", "1,920 d"),
]
RULES = [
    (r"(ch_house_debt_owed = )480\b", r"\g<1>1920"),
    (r"(silver >= )480\b", r"\g<1>1920"),
    (r'("cost": "silver )480(")', r"\g<1>1920\g<2>"),
    (r"(silver >= )367\b", r"\g<1>1468"),
    (r'("cost": "silver )367(")', r"\g<1>1468\g<2>"),
    (r"(tally of )480( pennies)", r"\g<1>1,920\g<2>"),
]
problems, changed = [], []


def swap(line, pairs):
    for pat, to in pairs:
        line = re.sub(pat, to, line)
    return line


def do_file(rel, pairs, keep=None):
    """Changes every line of a file by the pairs; `keep(i, lines)` says whether line i is one of the debt's."""
    path = os.path.join(DATA, rel.replace("/", os.sep))
    if not os.path.exists(path):
        problems.append("%s is not there" % rel)
        return
    text = io.open(path, encoding="utf-8", newline="").read()
    lines = text.split("\n")
    n = 0
    for i, line in enumerate(lines):
        if keep is not None and not keep(i, lines):
            continue
        new = swap(line, pairs)
        if new != line:
            changed.append((rel, i + 1, line.strip(), new.strip()))
            lines[i] = new
            n += 1
    if n == 0:
        problems.append("%s: nothing to change (done already, or the file has moved on: read it)" % rel)
        return
    out = "\n".join(lines)
    try:
        json.loads(out.lstrip("\ufeff"))
    except ValueError as e:
        problems.append("%s: the changed text is not JSON: %s" % (rel, e))
        return
    if APPLY:
        io.open(path, "w", encoding="utf-8", newline="").write(out)


def debt_key(i, lines):
    """writing.json: a text line whose key (the line before it) is one of the debt chain's, or the house's last page."""
    if '"text"' not in lines[i] or i == 0:
        return False
    key = lines[i - 1]
    return '"key": "chain_house_debt.' in key or '"key": "chain_year_1066.end_house.narr_boat"' in key


do_file("encounters/chain_house_debt_793.json", RULES + WORDS)
do_file("writing/chain_house_debt.json", WORDS)
do_file("writing.json", WORDS, debt_key)
do_file("encounters/crew_793.json", [(r"(trade_rules\.json start \()480 d( to the count\))", r"\g<1>1,920 d\g<2>")])
do_file("encounters/npc_roles_793.json", [(r"(holds the house's debt of )480 d\b", r"\g<1>1,920 d")])

# what still names the old sum in the chain's own files after the change (a new line the content pass wrote?)
left = []
for rel in ("encounters/chain_house_debt_793.json", "writing/chain_house_debt.json"):
    path = os.path.join(DATA, rel.replace("/", os.sep))
    if os.path.exists(path):
        for i, line in enumerate(io.open(path, encoding="utf-8")):
            line2 = swap(line, RULES + WORDS)
            if re.search(r"\b(480|367)\b|[Tt]wo pounds|[Ff]orty dozen|[Ee]ighteen score and seven", line2):
                left.append("%s %d: %s" % (rel, i + 1, line2.strip()[:160]))

print("THE DEBT'S SUM IN THE CARDS: %s, %d lines" % ("APPLIED" if APPLY and not problems else "checked only (add --apply)" if not problems else "SEE THE PROBLEMS", len(changed)))
for rel, n, old, new in changed:
    print("  %s %d" % (rel, n))
    print("    - %s" % old[:300])
    print("    + %s" % new[:300])
for x in left:
    print("  STILL NAMES THE OLD SUM, READ IT: " + x)
for x in problems:
    print("  PROBLEM: " + x)
sys.exit(1 if problems else 0)
