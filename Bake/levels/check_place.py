r"""Compare two place bakes grid by grid (and their meta.json): proves make_place.py reproduces a bake.

Run: Bake\run.bat levels\check_place.py <folder A> <folder B>
     (e.g. G:\kapman_unity\data\levels\dorestad G:\kapman_unity\data\levels\_check_dorestad)
Prints, for each grid, the largest absolute difference and how many cells differ; for meta.json, the keys that
differ and the keys only one side has. Exit code 0 when every grid is identical.
"""
import json
import os
import sys

import numpy as np

GRIDS = [("height.f32", np.float32), ("water_sd.f32", np.float32), ("class.u8", np.uint8), ("cover.u8", np.uint8),
         ("wood.u8", np.uint8)]


def main(a, b):
    ma = json.load(open(os.path.join(a, "meta.json")))
    mb = json.load(open(os.path.join(b, "meta.json")))
    n = ma["n"]
    same = True
    for name, dt in GRIDS:
        x = np.fromfile(os.path.join(a, name), dt).reshape(n, n).astype(np.float64)
        y = np.fromfile(os.path.join(b, name), dt).reshape(n, n).astype(np.float64)
        d = np.abs(x - y)
        k = int((d > 0).sum())
        same &= k == 0
        print("%-13s max |diff| %-10.6g cells differing %d of %d%s" % (name, d.max(), k, n * n,
              "" if k == 0 else " (first at row %d col %d)" % tuple(np.argwhere(d > 0)[0])))
    print("meta.json:")
    for key in ma:
        if key not in mb:
            print("  only in A:", key)
        elif ma[key] != mb[key]:
            print("  differs:  ", key, ":", json.dumps(ma[key])[:120], "|", json.dumps(mb[key])[:120])
    for key in mb:
        if key not in ma:
            print("  only in B:", key)
    print("identical grids" if same else "GRIDS DIFFER")
    return 0 if same else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1], sys.argv[2]))
