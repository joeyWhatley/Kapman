"""FETCH CC0 ASSETS - downloads the free (CC0) models and ground textures listed in cc0_assets.csv, from Poly Haven
and ambientCG, into G:\\kapman_unity\\data\\raw\\assets\\<source>\\<slug>\\. No account needed. CC0 means free for any
use, a paid game included, with no credit required (the list keeps where each came from anyway).

  models     Poly Haven FBX at 2K with its textures (LODs where Poly Haven has them)
  textures   Poly Haven 2K JPGs: colour, normal (OpenGL), ARM (ambient occlusion, roughness, metal), displacement;
             ambientCG 2K-JPG zips (unpacked)

The csv also says which Build mode drawer and category each goes in. Resumable: files already there are skipped.
Run:  Bake\\run.bat ..\\Tools\\Assets\\fetch_cc0_assets.py          (from the Kapman folder: any Python 3 with the
      standard library will do)
"""
import csv
import json
import os
import shutil
import sys
import urllib.request
import zipfile
from concurrent.futures import ThreadPoolExecutor

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = r"G:\kapman_unity\data\raw\assets"
AGENT = {"User-Agent": "Kapman asset fetcher (CC0 assets for a game in development)"}
TEXTURE_MAPS = ("Diffuse", "nor_gl", "arm", "Displacement")


def get_json(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=AGENT), timeout=60) as r:
        return json.loads(r.read().decode("utf-8"))


def download(url, path):
    if os.path.exists(path):
        return 0
    os.makedirs(os.path.dirname(path), exist_ok=True)
    part = path + ".part"
    with urllib.request.urlopen(urllib.request.Request(url, headers=AGENT), timeout=300) as r, open(part, "wb") as f:
        shutil.copyfileobj(r, f, 1 << 20)
    os.replace(part, path)
    return os.path.getsize(path)


def polyhaven_jobs(slug, kind):
    files = get_json("https://api.polyhaven.com/files/" + slug)
    base = os.path.join(OUT, "polyhaven", slug)
    jobs = []
    if kind == "model":
        fbx = files["fbx"]["2k"]["fbx"]
        jobs.append((fbx["url"], os.path.join(base, os.path.basename(fbx["url"]))))
        for rel, item in (fbx.get("include") or {}).items():
            jobs.append((item["url"], os.path.join(base, rel.replace("/", os.sep))))
    else:
        for m in TEXTURE_MAPS:
            if m in files and "2k" in files[m] and "jpg" in files[m]["2k"]:
                url = files[m]["2k"]["jpg"]["url"]
                jobs.append((url, os.path.join(base, os.path.basename(url))))
    return jobs


def ambientcg_fetch(slug):
    base = os.path.join(OUT, "ambientcg", slug)
    marker = os.path.join(base, ".done")
    if os.path.exists(marker):
        return 0
    try:
        return ambientcg_fetch_inner(slug, base, marker)
    except Exception as e:
        print("  skipped %s: %s" % (slug, e), flush=True)
        return 0


def ambientcg_fetch_inner(slug, base, marker):
    zpath = os.path.join(base, slug + "_2K-JPG.zip")
    size = download("https://ambientcg.com/get?file=%s_2K-JPG.zip" % slug, zpath)
    with zipfile.ZipFile(zpath) as z:
        z.extractall(base)
    os.remove(zpath)
    open(marker, "w").close()
    return size


def main():
    with open(os.path.join(HERE, "cc0_assets.csv"), encoding="utf-8-sig") as f:
        rows = list(csv.DictReader(f))
    jobs, total = [], 0
    for r in rows:
        if r["source"] == "polyhaven":
            try:
                jobs += polyhaven_jobs(r["slug"], r["kind"])
            except Exception as e:
                print("  skipped %s: %s" % (r["slug"], e), flush=True)
    print("%d Poly Haven files for %d assets, and %d ambientCG sets" % (
        len(jobs), sum(1 for r in rows if r["source"] == "polyhaven"), sum(1 for r in rows if r["source"] == "ambientcg")), flush=True)
    with ThreadPoolExecutor(6) as pool:
        for i, size in enumerate(pool.map(lambda j: download(*j), jobs), 1):
            total += size
            if i % 50 == 0 or i == len(jobs):
                print("  %d / %d files, %.0f MB so far" % (i, len(jobs), total / 1e6), flush=True)
        for size in pool.map(ambientcg_fetch, [r["slug"] for r in rows if r["source"] == "ambientcg"]):
            total += size
    # the list, with licences, next to the files
    with open(os.path.join(OUT, "ASSETS.md"), "w", encoding="utf-8") as f:
        f.write("# Free assets (CC0: any use, a paid game included, no credit required)\n\n")
        f.write("Downloaded by Kapman/Tools/Assets/fetch_cc0_assets.py from the list in cc0_assets.csv.\n\n")
        f.write("| Source | Asset | Kind | Build mode drawer | Category | Note |\n|---|---|---|---|---|---|\n")
        for r in rows:
            url = ("https://polyhaven.com/a/" + r["slug"]) if r["source"] == "polyhaven" else ("https://ambientcg.com/view?id=" + r["slug"])
            f.write("| %s | [%s](%s) | %s | %s | %s | %s |\n" % (r["source"], r["slug"], url, r["kind"], r["drawer"], r["category"], r["note"]))
    print("done: %.0f MB downloaded -> %s" % (total / 1e6, OUT), flush=True)


if __name__ == "__main__":
    sys.exit(main())
