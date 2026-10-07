# -*- coding: utf-8 -*-
"""Bakes the whole-world chart picture (make_map_art.py, 30 x 18 tiles at 120 m) one tile to a process, a few at once,
skipping tiles already on disk, so it can be stopped and started again. Then puts the overviews together.
  python-qgis-ltr.bat bake_whole_world.py [jobs]      -> Kapman2/MapArt/world_whole"""
import os, subprocess, sys, time
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "..", "MapArt", "world_whole"))
PY = r"C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat"
COLS, ROWS = 30, 18
jobs = int(sys.argv[1]) if len(sys.argv) > 1 else 3
os.makedirs(OUT, exist_ok=True)
# nearest the old sailing world first, so the part that matters most is there soonest
todo = [(r, c) for r in range(ROWS) for c in range(COLS) if not os.path.exists(os.path.join(OUT, "t_%d_%d.png" % (r, c)))]
todo.sort(key=lambda t: (t[0] - 8) ** 2 + (t[1] - 16) ** 2)
print("%d tiles to bake, %d at once" % (len(todo), jobs), flush=True)
t0 = time.time(); running = []; done = failed = 0
def reap(block):
    global done, failed
    while True:
        for p, t in list(running):
            if p.poll() is not None:
                running.remove((p, t))
                ok = os.path.exists(os.path.join(OUT, "t_%d_%d.png" % t))
                done += ok; failed += (not ok)
                left = len(todo) + len(running)
                print("%s tile %d,%d  (%d done, %d failed, %d left, %.0f min so far, about %.0f min to go)" %
                      ("ok  " if ok else "FAIL", t[0], t[1], done, failed, left, (time.time() - t0) / 60,
                       (time.time() - t0) / 60 / max(1, done + failed) * left), flush=True)
        if not block or len(running) < jobs:
            return
        time.sleep(1)
while todo or running:
    while todo and len(running) < jobs:
        t = todo.pop(0)
        log = open(os.path.join(OUT, "bake_%d_%d.log" % t), "w")
        running.append((subprocess.Popen([PY, os.path.join(HERE, "make_map_art.py"), "--out", OUT, "--tiles", "%d,%d" % t, "--no-overview"],
                                         stdout=log, stderr=subprocess.STDOUT, cwd=HERE), t))
    reap(True)
    if not todo:
        while running:
            reap(False); time.sleep(1)
subprocess.call([PY, os.path.join(HERE, "make_map_art.py"), "--out", OUT, "--tiles", "none"], cwd=HERE)
for f in os.listdir(OUT):
    if f.startswith("bake_") and f.endswith(".log") and os.path.exists(os.path.join(OUT, "t_" + f[5:-4] + ".png")):
        os.remove(os.path.join(OUT, f))
print("finished: %d baked, %d failed, %.0f minutes" % (done, failed, (time.time() - t0) / 60), flush=True)
