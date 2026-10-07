# -*- coding: utf-8 -*-
r"""
APPLY THE MONEY FIX, THE DATA PART (the user said yes on 7 Oct 2026). From Design/research/next/money.md: as the numbers
stood a sensible player cleared the 480 d debt 99 times in 100 and had it in hand by day 12.

This script makes the two changes that are data only:
  1. the famine of 793 falls on grain only, not on all food   (Assets/Kapman/Game/Data/shocks_793.json)
  2. the English toll is a tenth, not a twentieth             (Assets/Kapman/Game/Data/trade_rules.json)
Still to do in code and content, by hand, with the tests run (the same ruling):
  3. the world's traders work the markets from 1 April, before the player's first morning (a pre-run at a new game)
  4. no card pays over 60 d and no chain over 240 d: the amounts in the cards AND in their words must change together
     (217 lines of the cards' words name a sum), so this is a rewrite with a list to read, not a clamp
  5. the debt falls due on a day (Michaelmas), and the count's four casks are marked as his

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\economy\apply_money_fix.py [--apply]
Without --apply it only checks and says what would change. Run the EditMode tests afterwards (EconomyTests).
"""
import io, json, os, re, sys
ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
APPLY = "--apply" in sys.argv
GRAIN = ["wheat", "rye", "barley", "oats", "malt", "flour"]
problems, changes = [], []

goods = json.load(io.open(os.path.join(DATA, "goods_793.json"), encoding="utf-8-sig"))
ids = set(g.get("id") for g in goods.get("goods", []) if isinstance(g, dict))
missing = [g for g in GRAIN if g not in ids]
if missing:
    problems.append("goods_793.json has no good called: " + ", ".join(missing))
grain = [g for g in GRAIN if g in ids]

p = os.path.join(DATA, "shocks_793.json")
t = io.open(p, encoding="utf-8").read()
m = re.search(r'("793-great-famine": \{ "goods": ")([^"]+)(")', t)
if not m:
    problems.append("shocks_793.json: the famine of 793 was not found")
elif m.group(2) == ",".join(grain):
    changes.append("the famine already falls on grain only")
else:
    changes.append("famine of 793: goods %s -> %s" % (m.group(2), ",".join(grain)))
    t2 = t[:m.start(2)] + ",".join(grain) + t[m.end(2):]
    json.loads(t2.lstrip("\ufeff"))
    if APPLY and not problems:
        io.open(p, "w", encoding="utf-8", newline="").write(t2)

p = os.path.join(DATA, "trade_rules.json")
t = io.open(p, encoding="utf-8").read()
m = re.search(r'(\{"id": "english",[^\n]*?"toll": )([0-9.]+)', t)
if not m:
    problems.append("trade_rules.json: the English toll was not found")
elif abs(float(m.group(2)) - 0.10) < 1e-9:
    changes.append("the English toll is already a tenth")
else:
    changes.append("English toll: %s -> 0.10" % m.group(2))
    t2 = t[:m.start(2)] + "0.10" + t[m.end(2):]
    json.loads(t2.lstrip("\ufeff"))
    if APPLY and not problems:
        io.open(p, "w", encoding="utf-8", newline="").write(t2)

print("MONEY FIX (data): %s" % ("APPLIED" if APPLY and not problems else "checked only (add --apply)" if not problems else "NOT APPLIED"))
for c in changes: print("  " + c)
for x in problems: print("  PROBLEM: " + x)
sys.exit(1 if problems else 0)
