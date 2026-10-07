# -*- coding: utf-8 -*-
r"""
APPLY THE PALETTE MEND (the user said yes on 7 Oct 2026: "apply the seventeen small colour shifts for colour-blind players").

The moves are those of Design/research/next/art.md, section B3, found by Bake/art/check_palettes.py --mend: seventeen
colour values moved a little so that every pair of grounds in every palette can be told apart by every kind of sight.
Byzantium (#702963) is never moved, and Vineyard, which had none, gets it as its sea.

The palettes live in four places that must change together:
  Assets/Kapman/Code/Game/Map/MapPalette.cs        the game
  Assets/Kapman/Game/UI/Frame.uss                  the frame's edge (a palette's sea) and accent (from its shallows);
                                                   the test TheStylesheetHoldsEveryPalettesColours holds them in step
  Bake/world/recolour_map.py                       the previews
  Design/palettes/<people>_<name>.gpl              the files for the artist

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\world\apply_palette_mend.py [--apply]
Without --apply it only checks that every old value is where it should be and says what would change.
Run the EditMode tests afterwards (FrameWindowsTests, SelfTests) and look at the chart.
"""
import colorsys
import io
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
APPLY = "--apply" in sys.argv
ROLES = ["deep water", "sea", "shallows", "shore", "marsh and heath", "cleared land", "woodland", "high and bare ground"]
# people, palette name, role index, old, new
MOVES = [
    ("frisian", "Tide", 0, "#3b1540", "#37113c"), ("frisian", "Tide", 7, "#f1e8d6", "#faf1df"),
    ("frankish", "Vineyard", 1, "#5a2970", "#702963"), ("frankish", "Vineyard", 6, "#70293f", "#481224"),
    ("english", "Minster", 3, "#ece5cc", "#efe5c2"),
    ("danish", "Amber", 1, "#296370", "#38717e"), ("danish", "Amber", 3, "#f3dfa8", "#f7dda8"),
    ("norse", "Fjord", 1, "#34407c", "#585896"), ("norse", "Fjord", 2, "#8494c6", "#9490c2"), ("norse", "Fjord", 6, "#1f4a44", "#204a44"),
    ("svea", "Silver", 1, "#4a3d5c", "#463c43"), ("svea", "Silver", 3, "#e8e4ee", "#cbcde2"),
    ("wendish", "Grove", 7, "#efe9d0", "#f8f2d9"),
    ("irish", "Vellum", 0, "#1f2a44", "#202638"), ("irish", "Vellum", 1, "#2f5d7a", "#2c678f"), ("irish", "Vellum", 4, "#c9553a", "#ce4f48"),
    ("irish", "Vellum", 6, "#3f7029", "#4c6e1f"),
]
problems = []


def rgb(h):
    return int(h[1:3], 16), int(h[3:5], 16), int(h[5:7], 16)


def accent(shallows):
    """FrameColours.Accent: the hue of the shallows, lightness 0.76, a little more colour."""
    r, g, b = [v / 255.0 for v in rgb(shallows)]
    h, l, s = colorsys.rgb_to_hls(r, g, b)
    return tuple(int(v * 255 + 0.5) for v in colorsys.hls_to_rgb(h, 0.76, min(1.0, s * 1.1)))


def read(rel):
    with io.open(os.path.join(ROOT, rel), "r", encoding="utf-8") as f:
        return f.read()


def write(rel, text):
    if APPLY:
        with io.open(os.path.join(ROOT, rel), "w", encoding="utf-8", newline="") as f:
            f.write(text)


def in_line(text, starts, old, new, what):
    """Changes old to new on the one line that starts a palette (the line holding `starts`)."""
    lines = text.split("\n")
    hits = [i for i, ln in enumerate(lines) if starts in ln]
    if len(hits) != 1:
        problems.append("%s: %d lines hold %s" % (what, len(hits), starts))
        return text
    i = hits[0]
    if new.lower() in lines[i].lower() and old.lower() not in lines[i].lower():
        return text                                          # already done
    if lines[i].lower().count(old.lower()) != 1:
        problems.append("%s: %s is on the line of %s %d times" % (what, old, starts, lines[i].lower().count(old.lower())))
        return text
    lines[i] = re.sub(re.escape(old), new, lines[i], flags=re.I)
    return "\n".join(lines)


cs = "Assets/Kapman/Code/Game/Map/MapPalette.cs"
py = "Bake/world/recolour_map.py"
uss = "Assets/Kapman/Game/UI/Frame.uss"
cs_t, py_t, uss_t = read(cs), read(py), read(uss)

# the palettes as the game has them now, to check the stylesheet's sums before anything moves
now = {}
for m in re.finditer(r'P\("([a-z]+)", "([A-Za-z]+)", "[a-z_]+", ((?:"#[0-9a-fA-F]{6}",? ?){8})\)', cs_t):
    now[m.group(1)] = [m.group(2)] + re.findall(r"#[0-9a-fA-F]{6}", m.group(3))
if len(now) != 8:
    problems.append("MapPalette.cs: found %d palettes, not 8" % len(now))
for people, v in sorted(now.items()):
    mm = re.search(r"\.frame-root\.pal-%s \{ --frame-edge: rgb\((\d+), (\d+), (\d+)\); --frame-accent: rgb\((\d+), (\d+), (\d+)\);" % people, uss_t)
    if not mm:
        problems.append("Frame.uss: no class pal-%s" % people)
        continue
    got = tuple(int(x) for x in mm.groups())
    want = rgb(v[2]) + accent(v[3])
    if any(abs(a - b) > 1 for a, b in zip(got, want)):
        problems.append("Frame.uss pal-%s holds %s but my sum of the game's rule gives %s: the sum is wrong, do not apply" % (people, got, want))

for people, name, role, old, new in MOVES:
    cs_t = in_line(cs_t, 'P("%s", "%s"' % (people, name), '"%s"' % old, '"%s"' % new, cs)
    py_t = in_line(py_t, "('%s', '%s'" % (people, name), old, new, py) if py_t.count("\n") > 20 else py_t
    # recolour_map.py keeps all eight on one line: change within that palette's own tuple
    gpl = "Design/palettes/%s_%s.gpl" % (people, name.lower())
    g = read(gpl)
    r0, g0, b0 = rgb(old)
    r1, g1, b1 = rgb(new)
    pat = re.compile(r"^\s*%d\s+%d\s+%d\t%s %s\s*$" % (r0, g0, b0, re.escape(ROLES[role]), re.escape(old)), re.M | re.I)
    if pat.search(g):
        write(gpl, pat.sub("%3d %3d %3d\t%s %s" % (r1, g1, b1, ROLES[role], new), g))
    elif new.lower() not in g.lower():
        problems.append("%s: no line for %s %s" % (gpl, ROLES[role], old))

# recolour_map.py: one long line of tuples
if py_t.count("\n") <= 20 or "PALETTES = [" in py_t:
    def fix_tuple(m):
        people = m.group(1)
        body = m.group(0)
        for p2, name, role, old, new in MOVES:
            if p2 == people:
                if body.lower().count(old.lower()) == 1:
                    body = re.sub(re.escape(old), new, body, flags=re.I)
                elif new.lower() not in body.lower():
                    problems.append("%s: %s not in the tuple of %s" % (py, old, people))
        return body
    py_t = re.sub(r"\('([a-z]+)', '[A-Za-z]+', '[a-z_]+', '[^']+'\)", fix_tuple, read(py))

# the frame's stylesheet, from the palettes as they will be
after = {}
for m in re.finditer(r'P\("([a-z]+)", "([A-Za-z]+)", "[a-z_]+", ((?:"#[0-9a-fA-F]{6}",? ?){8})\)', cs_t):
    after[m.group(1)] = re.findall(r"#[0-9a-fA-F]{6}", m.group(3))
for people, cols in sorted(after.items()):
    e, a = rgb(cols[1]), accent(cols[2])
    line = ".frame-root.pal-%s { --frame-edge: rgb(%d, %d, %d); --frame-accent: rgb(%d, %d, %d); --frame-rule: rgba(%d, %d, %d, 0.3); }" % ((people,) + e + a + a)
    uss_t, n = re.subn(r"^\.frame-root\.pal-%s \{[^}]*\}" % people, line, uss_t, flags=re.M)
    if n != 1:
        problems.append("Frame.uss: pal-%s replaced %d times" % (people, n))
    if cols.count("#702963") != 1 and [c.lower() for c in cols].count("#702963") != 1:
        problems.append("%s holds Byzantium %d times after the mend" % (people, [c.lower() for c in cols].count("#702963")))

print("PALETTE MEND: %d moves, %s" % (len(MOVES), "APPLIED" if APPLY and not problems else "checked only (add --apply)" if not problems else "NOT APPLIED"))
for people, cols in sorted(after.items()):
    was = now.get(people, [""] * 9)[1:]
    ch = ["%s %s -> %s" % (ROLES[i], was[i], cols[i]) for i in range(8) if i < len(was) and was[i].lower() != cols[i].lower()]
    print("  %-9s %s" % (people, "; ".join(ch) if ch else "no change"))
if problems:
    print("PROBLEMS")
    for x in problems:
        print("  - " + x)
    sys.exit(1)
write(cs, cs_t)
write(py, py_t)
write(uss, uss_t)
