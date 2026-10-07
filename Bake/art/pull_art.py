# -*- coding: utf-8 -*-
r"""
PULL ART - brings the pictures you have drawn back from the Desktop folder into the game's project.

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\art\pull_art.py
      [--dry]                 only look and list, copy nothing
      [--map]                 also put redrawn chart tiles into Kapman2/MapArt/world (the game reads them from there)
      [--desktop <folder>]    another folder than  C:/Users/joeyw/OneDrive/Desktop/Kapman Art
      [--root <Kapman2>]      another project folder

A picture counts as yours when it is no longer the placeholder that make_art_list.py wrote (it keeps the SHA-1 of every
placeholder in Kapman2/Art/placeholder_manifest.json). Each such picture is checked against the list of slots
(Kapman2/Art/art_slots.json): the name must be a slot's, the size must be the slot's size. A picture that passes is
copied to Kapman2/Art/final/<kind>/<slot id>.png if it is new or has changed since the last pull.

Writes
  Kapman2/Art/final/<kind>/<slot id>.png   your pictures
  Kapman2/Art/final/art_final.csv          every picture in final: id, kind, file, size, when it was pulled
  Kapman2/Art/final/pull_report.txt        what this run did
Nothing in the Desktop folder is ever changed or removed, and nothing is removed from final.
"""
import csv
import datetime
import hashlib
import io
import json
import os
import shutil
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def arg(name, default=None):
    if name in sys.argv:
        i = sys.argv.index(name)
        if i + 1 < len(sys.argv):
            return sys.argv[i + 1]
    return default


ROOT = os.path.normpath(arg("--root", os.path.join(HERE, "..", "..")))
ART = os.path.join(ROOT, "Art")
FINAL = os.path.join(ART, "final")
DESK = arg("--desktop", os.environ.get("KAPMAN_ART_DESKTOP", "C:/Users/joeyw/OneDrive/Desktop/Kapman Art"))
DRY = "--dry" in sys.argv
MAP = "--map" in sys.argv
PICTURES = (".png",)
OTHER_PICTURES = (".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".aseprite", ".ase", ".psd", ".kra")

LOG = []


def say(msg=""):
    LOG.append(msg)
    try:
        print(msg)
    except Exception:
        print(msg.encode("ascii", "replace").decode("ascii"))


def file_sha(path):
    h = hashlib.sha1()
    with open(path, "rb") as f:
        for block in iter(lambda: f.read(1 << 20), b""):
            h.update(block)
    return h.hexdigest()


def jload(path):
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            return json.load(f)
    except Exception:
        return None


def png_size(path):
    """Width and height from the PNG's own header (no picture library needed)."""
    try:
        with open(path, "rb") as f:
            head = f.read(24)
        if head[:8] != b"\x89PNG\r\n\x1a\n" or head[12:16] != b"IHDR":
            return None
        return int.from_bytes(head[16:20], "big"), int.from_bytes(head[20:24], "big")
    except Exception:
        return None


def palette():
    cols = set()
    try:
        with io.open(os.path.join(ROOT, "Design", "palette_byzantium.gpl"), "r", encoding="utf-8") as f:
            for line in f:
                p = line.split()
                if len(p) >= 3 and all(x.isdigit() for x in p[:3]):
                    cols.add((int(p[0]), int(p[1]), int(p[2])))
    except Exception:
        pass
    return cols


def off_palette(path, cols):
    """How many of the picture's colours are not among the game's 40 (None if it cannot be told)."""
    if not cols:
        return None
    try:
        from PIL import Image
        im = Image.open(path).convert("RGBA")
        if im.size[0] * im.size[1] > 2048 * 2048:
            return None                                        # the chart's tiles are not held to the palette here
        got = im.getcolors(maxcolors=1 << 20) or []
        return len({(r, g, b) for _, (r, g, b, a) in got if a > 0 and (r, g, b) not in cols})
    except Exception:
        return None


def main():
    say("PULL ART  %s" % datetime.datetime.now().strftime("%d %B %Y, %H:%M") + ("   (--dry: nothing is copied)" if DRY else ""))
    say("from  %s" % DESK)
    say("to    %s" % FINAL)
    if not os.path.isdir(DESK):
        say("The Desktop folder is not there. Run make_art_list.py first, or name the folder with --desktop.")
        return 1
    reg = (jload(os.path.join(ART, "art_slots.json")) or {}).get("slots") or {}
    manifest = (jload(os.path.join(ART, "placeholder_manifest.json")) or {}).get("files") or {}
    if not reg:
        say("Kapman2/Art/art_slots.json is not there or is empty. Run make_art_list.py first.")
        return 1
    by_file = {}
    for sid, r in reg.items():
        if isinstance(r, dict) and r.get("file"):
            by_file[r["file"].replace("\\", "/").lower()] = (sid, r)
    cols = palette()
    old = {}
    final_csv = os.path.join(FINAL, "art_final.csv")
    if os.path.exists(final_csv):
        try:
            with io.open(final_csv, "r", encoding="utf-8-sig", newline="") as f:
                for row in csv.DictReader(f):
                    old[row.get("id")] = row
        except Exception:
            pass

    new, changed, same, wrong_size, unknown, not_png, colours, failed = [], [], [], [], [], [], [], []
    placeholders = 0
    now = datetime.datetime.now().isoformat(timespec="seconds")
    for kind in sorted(os.listdir(DESK)):
        kdir = os.path.join(DESK, kind)
        if not os.path.isdir(kdir) or kind.startswith("_"):
            continue
        for fn in sorted(os.listdir(kdir)):
            path = os.path.join(kdir, fn)
            if not os.path.isfile(path):
                continue
            relf = "%s/%s" % (kind, fn)
            ext = os.path.splitext(fn)[1].lower()
            if ext not in PICTURES:
                if ext in OTHER_PICTURES:
                    not_png.append(relf)
                continue
            try:
                digest = file_sha(path)
            except Exception as e:
                failed.append("%s (%s)" % (relf, e))
                continue
            if manifest.get(relf) == digest:
                placeholders += 1
                continue
            hit = by_file.get(relf.lower())
            if not hit:
                unknown.append(relf)
                continue
            sid, r = hit
            size = png_size(path)
            want = tuple(int(x) for x in str(r.get("size") or "0x0").split("x")[:2])
            if size != want:
                wrong_size.append("%s is %s, the slot is %d x %d" % (relf, ("%d x %d" % size) if size else "not a PNG inside", want[0], want[1]))
                continue
            target = os.path.join(FINAL, kind, fn)
            if os.path.exists(target) and file_sha(target) == digest:
                same.append(relf)
                old.setdefault(sid, {"id": sid, "kind": kind, "file": relf, "size": "%dx%d" % want, "pulled": ""})
                continue
            bad = off_palette(path, cols)
            if bad:
                colours.append("%s: %d colours that are not in the palette" % (relf, bad))
            was = os.path.exists(target)
            if not DRY:
                try:
                    os.makedirs(os.path.dirname(target), exist_ok=True)
                    shutil.copyfile(path, target)
                    if MAP and kind == "map":
                        mt = os.path.join(ROOT, "MapArt", "world", fn[4:] if fn.startswith("map.") else fn)
                        if os.path.isdir(os.path.dirname(mt)):
                            shutil.copyfile(path, mt)
                except Exception as e:
                    failed.append("%s (%s)" % (relf, e))
                    continue
                old[sid] = {"id": sid, "kind": kind, "file": relf, "size": "%dx%d" % want, "pulled": now}
            (changed if was else new).append(relf)

    def block(title, items):
        if items:
            say("")
            say("%s (%d)" % (title, len(items)))
            for x in items:
                say("  " + x)

    verb = "WOULD BE COPIED" if DRY else "COPIED"
    block("NEW, %s to Art/final" % verb, new)
    block("CHANGED SINCE THE LAST PULL, %s to Art/final" % verb, changed)
    block("WRONG SIZE, not copied (draw at the slot's size)", wrong_size)
    block("NAME NOT IN THE LIST, not copied (a picture must keep its placeholder's name and folder)", unknown)
    block("NOT A PNG, not copied (save as .png under the placeholder's name)", not_png)
    block("COLOURS OUTSIDE THE GAME'S 40 (copied all the same; for you to judge)", colours)
    block("COULD NOT BE READ OR WRITTEN", failed)
    if MAP and any(x.startswith("map/") for x in new + changed) and not DRY:
        say("")
        say("Chart tiles were put into MapArt/world: run Bake\\run.bat world\\make_map_overview.py so the far-out view matches.")
    say("")
    total = len(reg) - sum(1 for r in reg.values() if isinstance(r, dict) and r.get("retired"))
    say("In all: %d new, %d changed, %d already pulled and the same, %d still placeholders, %d not copied." % (
        len(new), len(changed), len(same), placeholders, len(wrong_size) + len(unknown) + len(not_png) + len(failed)))
    done = len(old) if not DRY else len(same) + len(new) + len(changed)
    say("Pictures of yours in Art/final: %d of %d slots." % (done, total))
    if not DRY:
        os.makedirs(FINAL, exist_ok=True)
        with io.open(final_csv, "w", encoding="utf-8-sig", newline="") as f:
            wr = csv.writer(f)
            wr.writerow(["id", "kind", "file", "size", "pulled"])
            for sid in sorted(old):
                row = old[sid]
                wr.writerow([row.get("id"), row.get("kind"), row.get("file"), row.get("size"), row.get("pulled")])
        with io.open(os.path.join(FINAL, "pull_report.txt"), "w", encoding="utf-8") as f:
            f.write("\n".join(LOG) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
