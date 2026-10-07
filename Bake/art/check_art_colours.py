# -*- coding: utf-8 -*-
r"""
CHECK ART COLOURS: which of the game's 40 colours some players cannot tell apart, and how well a mark stands on a ground.

The figures in Design/art_guide.md come from here. It uses the sums of check_palettes.py (CIEDE2000 distance as normal,
red-blind, green-blind and blue-blind sight see the two colours, at full strength; the WCAG contrast ratio) on the 40
colours of Design/palette_byzantium.gpl. It prints and writes nothing.

  & "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\art\check_art_colours.py [limit]

limit: print the pairs nearer than this to any sight (default 5: "one colour"; 10: "told side by side, not named alone").
"""
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
LIMIT = float(sys.argv[1]) if len(sys.argv) > 1 else 5.0
sys.argv = sys.argv[:1]
import check_palettes as cp                                                   # noqa: E402

GPL = os.path.join(HERE, "..", "..", "Design", "palette_byzantium.gpl")
# a mark or a word, and the ground it stands on: what the frame and the chart really pair
PAIRS = [("warm white", "dark purple"), ("pale gold", "dark purple"), ("gold", "dark purple"), ("pale stone", "dark purple"),
         ("lilac", "dark purple"), ("stone", "dark purple"), ("mauve", "dark purple"), ("pale gold", "plum"), ("gold", "plum"),
         ("pale gold", "byzantium"), ("gold", "byzantium"), ("warm white", "night"), ("reed light", "night"), ("rust", "night"),
         ("mauve", "night"), ("blood", "night"), ("warm white", "sea"), ("night", "sea"), ("warm white", "sand"), ("night", "sand"),
         ("warm white", "grass"), ("night", "foam")]


def main():
    cols = []
    for line in io.open(GPL, encoding="utf-8"):
        m = re.match(r"\s*(\d+)\s+(\d+)\s+(\d+)\s+(.+)", line)
        if m:
            cols.append((m.group(4).strip(), cp.Col("#%02x%02x%02x" % tuple(int(m.group(i)) for i in (1, 2, 3)))))
    print("%d colours. Pairs nearer than %.0f to any sight (under 5: one colour; under 10: told side by side, not named alone)" % (len(cols), LIMIT))
    out = []
    for i in range(len(cols)):
        for j in range(i + 1, len(cols)):
            ds = [cp.dist(cols[i][1], cols[j][1], s) for s in cp.SIGHTS]
            if min(ds) < LIMIT:
                out.append((min(ds), cols[i][0], cols[j][0], ds))
    for d, a, b, ds in sorted(out):
        print("  %-12s %-13s normal %5.1f   red-blind %5.1f   green-blind %5.1f   blue-blind %5.1f" % (a, b, ds[0], ds[1], ds[2], ds[3]))
    byname = dict(cols)
    print("\nContrast of a mark on its ground, normal sight (a mark or a line wants 3, words want 4.5)")
    for a, b in PAIRS:
        print("  %-11s on %-12s %5.1f" % (a, b, cp.con(byname[a], byname[b])))


if __name__ == "__main__":
    main()
