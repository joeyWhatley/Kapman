# cap_order_premiums.py - writes "premium 0.5" into every card order whose premium is over a half.
# Design/fixes/orders.md, rule 3. Run with no argument to see what it would do; with --apply to do it.
# It changes nothing but the number after "premium" inside "commission ..." effects: no other byte of a file moves.
import glob, io, json, os, re, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.environ.get("KAPMAN_DATA") or os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
CAP = 0.5
RX = re.compile(r'("commission\s+[a-z_0-9]+\s+\d+\s+by\s+[\d.]+\s+premium\s+)([\d.]+)(\s*")')

def cut_text(text):
    """The text with every order over a half cut to a half; how many orders it holds; how many were cut."""
    count = {"seen": 0, "cut": 0}
    def cut(m):
        count["seen"] += 1
        if float(m.group(2)) <= CAP + 1e-9:
            return m.group(0)
        count["cut"] += 1
        return m.group(1) + "0.5" + m.group(3)
    return RX.sub(cut, text), count["seen"], count["cut"]

def main():
    apply = "--apply" in sys.argv
    files = sorted(glob.glob(os.path.join(DATA, "encounters", "*.json"))) + [os.path.join(DATA, "events_793.json")]
    seen = changed = 0
    for path in files:
        with io.open(path, "r", encoding="utf-8", newline="") as f:      # (newline="": line ends are kept as they are)
            text = f.read()
        new, n, cut = cut_text(text)
        seen += n
        if cut == 0:
            continue
        json.loads(new.lstrip("﻿"))                                  # still a JSON file, or stop here
        changed += cut
        print("%-52s %3d of %3d orders cut to a half" % (os.path.basename(path), cut, n))
        if apply:
            with io.open(path, "w", encoding="utf-8", newline="") as f:
                f.write(new)
    print("%d orders seen, %d over a half%s" % (seen, changed, ": written" if apply else ": nothing written (run with --apply)"))

if __name__ == "__main__":
    main()
