# -*- coding: utf-8 -*-
"""Merges every Assets/Kapman/Game/Data/writing/*.json into writing.json (the game reads only that). Run after writers finish."""
import io, json, glob, os
D = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Kapman", "Game", "Data"))
main = json.load(io.open(os.path.join(D, "writing.json"), encoding="utf-8-sig"))
have = {e["key"]: e for e in main["entries"]}
n = 0
for f in sorted(glob.glob(os.path.join(D, "writing", "*.json"))):
    try:
        d = json.load(io.open(f, encoding="utf-8-sig"))
    except Exception as e:
        print("skipped", os.path.basename(f), e); continue
    for e in d.get("entries", []):
        k, t = e.get("key"), e.get("text")
        if not k or not isinstance(t, str) or not t.strip() or "[write:" in t: continue
        if k in have: have[k]["text"] = t
        else:
            ne = {"key": k, "text": t}; main["entries"].append(ne); have[k] = ne; n += 1
json.dump(main, io.open(os.path.join(D, "writing.json"), "w", encoding="utf-8", newline=chr(10)), ensure_ascii=False, indent=1)
print("writing.json:", len(main["entries"]), "entries,", n, "new")
