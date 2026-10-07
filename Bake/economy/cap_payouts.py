# cap_payouts.py - PROPOSES only. Reads the cards and their words, changes no data.
# Ruling (7 Oct 2026): no card pays over 60 d and no chain over 240 d.
# Out: Bake/economy/out/pay_cap_proposed.json
import json, re, os, glob, sys
from collections import OrderedDict, defaultdict

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
ENC = os.path.join(DATA, "encounters")
OUT = os.path.join(ROOT, "Bake", "economy", "out", "pay_cap_proposed.json")
CARD_CAP = 60
CHAIN_CAP = 240
GAIN = re.compile(r"^\s*silver\s*\+\s*(\d+)\b")

ONES = ["zero","one","two","three","four","five","six","seven","eight","nine","ten","eleven","twelve",
        "thirteen","fourteen","fifteen","sixteen","seventeen","eighteen","nineteen"]
TENS = ["","","twenty","thirty","forty","fifty","sixty","seventy","eighty","ninety"]

def words(n, land=True):
    if n < 20: return ONES[n]
    if n < 100:
        return TENS[n // 10] + ("-" + ONES[n % 10] if n % 10 else "")
    if n < 1000:
        r = n % 100
        s = ONES[n // 100] + " hundred"
        if r: s += (" and " if land else " ") + words(r)
        return s
    r = n % 1000
    s = words(n // 1000) + " thousand"
    if r: s += (" and " if (land and r < 100) else " ") + words(r, land)
    return s

def load(path):
    with open(path, "r", encoding="utf-8-sig") as f:
        return json.load(f)

# ---------- words ----------
WORDS = defaultdict(list)   # key -> [(file, text)]
def read_writing():
    files = [os.path.join(DATA, "writing.json")] + sorted(glob.glob(os.path.join(DATA, "writing", "*.json")))
    bad = []
    for p in files:
        try:
            d = load(p)
        except Exception as e:
            bad.append(os.path.basename(p) + ": " + str(e)[:80]); continue
        ents = d.get("entries", []) if isinstance(d, dict) else d
        rel = os.path.relpath(p, DATA).replace("\\", "/")
        for e in ents:
            if isinstance(e, dict) and "key" in e and isinstance(e.get("text"), str):
                WORDS[e["key"]].append((rel, e["text"]))
    return bad

def forms(n):
    # (regex, kind) ways the number n can stand in a line
    out = [(r"(?<![\d.,])" + str(n) + r"(?![\d])", "digits")]
    for land in (True, False):
        w = words(n, land)
        out.append((r"(?<![A-Za-z-])" + re.escape(w).replace(r"\ ", r"\s+") + r"(?![A-Za-z-])", "words"))
    if n >= 240 and n % 240 == 0:
        k = n // 240
        if k == 1:
            out.append((r"\b(?:a|one) pound\b", "pounds"))
        else:
            out.append((r"\b(?:" + str(k) + "|" + words(k) + r") pounds?\b", "pounds"))
    if n == 120: out.append((r"\bhalf a pound\b", "pounds"))
    if n == 60: out.append((r"\ba quarter of a pound\b", "pounds"))
    if n >= 12 and n % 12 == 0:
        k = n // 12
        out.append((r"\b(?:" + str(k) + "|" + words(k) + r") shillings?\b", "shillings"))
    return out

def change(text, old, new):
    # returns (new_text, kind, count) or None
    for rx, kind in forms(old):
        ms = list(re.finditer(rx, text, flags=re.I))
        if not ms: continue
        def rep(m):
            g = m.group(0)
            if kind == "digits": return str(new)
            if kind == "words":
                w = words(new)
            else:
                w = words(new) + " pennies"
            return w[0].upper() + w[1:] if g[0].isupper() else w
        return re.sub(rx, rep, text, flags=re.I), kind, len(ms)
    return None

# ---------- cards ----------
def walk(node, path, out):
    if isinstance(node, dict):
        for k, v in node.items(): walk(v, path + [k], out)
    elif isinstance(node, list):
        for i, v in enumerate(node): walk(v, path + [i], out)
    elif isinstance(node, str):
        m = GAIN.match(node)
        if m: out.append((path, int(m.group(1)), node))

def chain_of(card_id, fname):
    m = re.search(r"chain_([a-z0-9_]+?)(?:\.|$)", card_id or "")
    if m: return m.group(1)
    m = re.match(r"chain_(.+?)(?:_793)?\.json$", fname)
    if m: return m.group(1)
    return None

def main():
    bad = read_writing()
    gains = []   # dicts
    for p in sorted(glob.glob(os.path.join(ENC, "*.json"))):
        fname = os.path.basename(p)
        try:
            d = load(p)
        except Exception as e:
            bad.append(fname + ": " + str(e)[:80]); continue
        cards = d.get("cards", []) if isinstance(d, dict) else d
        if not isinstance(cards, list): continue
        for c in cards:
            if not isinstance(c, dict): continue
            found = []
            walk(c, [], found)
            for path, n, raw in found:
                opt = None; lst = "effects"; choice = "/".join(str(x) for x in path[:-1])
                if len(path) >= 3 and path[0] == "options" and isinstance(path[1], int):
                    opt = c["options"][path[1]]
                    choice = str(opt.get("key", path[1])) if isinstance(opt, dict) else str(path[1])
                    lst = str(path[-2]) if len(path) >= 2 else "effects"
                gains.append({"card": c.get("id", "?"), "file": fname, "choice": choice, "list": lst,
                              "old": n, "effect": raw, "chain": chain_of(c.get("id", ""), fname),
                              "_card": c, "_opt": opt if isinstance(opt, dict) else None})

    # ---------- chains: richest path = each card's richest choice, summed ----------
    def path_sum(gs, val):
        per_card = defaultdict(lambda: defaultdict(lambda: defaultdict(int)))
        for g in gs: per_card[g["card"]][g["choice"]][g["list"]] += val(g)
        tot = 0
        for card, ch in per_card.items():
            best = 0
            for choice, lists in ch.items():
                base = lists.get("effects", 0)
                rest = [v for k, v in lists.items() if k != "effects"]
                best = max(best, base + (max(rest) if rest else 0))
            tot += best
        return tot

    for g in gains: g["new"] = min(g["old"], CARD_CAP); g["why"] = "card over 60" if g["old"] > CARD_CAP else ""
    chains = defaultdict(list)
    for g in gains:
        if g["chain"]: chains[g["chain"]].append(g)
    chain_rep = []
    for name, gs in sorted(chains.items()):
        old_sum = path_sum(gs, lambda g: g["old"])
        cap_sum = path_sum(gs, lambda g: min(g["old"], CARD_CAP))
        new_sum = cap_sum
        if cap_sum > CHAIN_CAP:
            lo, hi = 0.0, 1.0
            f = lambda g, k: max(1, min(CARD_CAP, int(g["old"] * k)))
            for _ in range(40):
                mid = (lo + hi) / 2
                if path_sum(gs, lambda g: f(g, mid)) <= CHAIN_CAP: lo = mid
                else: hi = mid
            for g in gs:
                nv = f(g, lo)
                if nv != g["old"]:
                    g["why"] = "chain over 240" + (" and card over 60" if g["old"] > CARD_CAP else "")
                g["new"] = nv
            new_sum = path_sum(gs, lambda g: g["new"])
        chain_rep.append({"chain": name, "files": sorted(set(g["file"] for g in gs)),
                          "cards_with_gains": len(set(g["card"] for g in gs)),
                          "richest_path_old": old_sum, "richest_path_after_card_cap": cap_sum,
                          "richest_path_new": new_sum, "over_240": old_sum > CHAIN_CAP,
                          "scaled": cap_sum > CHAIN_CAP})

    # ---------- words ----------
    rows = []; by_file = defaultdict(lambda: {"gains_changed": 0, "words_changed": 0, "number_not_found": 0})
    word_files = defaultdict(int)
    for g in gains:
        if g["new"] == g["old"]: continue
        c, opt = g["_card"], g["_opt"]
        by_file[g["file"]]["gains_changed"] += 1
        base = {"card": g["card"], "file": g["file"], "choice": g["choice"], "list": g["list"],
                "effect": g["effect"], "old": g["old"], "new": g["new"], "why": g["why"], "chain": g["chain"]}
        prefix = None
        for k in ("title_key", "intention_key"):
            if isinstance(c.get(k), str) and "." in c[k]: prefix = c[k].rsplit(".", 1)[0]; break
        cand = []
        if opt:
            if isinstance(opt.get("label_key"), str): cand.append(opt["label_key"])
            for v in opt.values():
                if isinstance(v, str):
                    for m in re.finditer(r"[A-Za-z0-9_]+(?:\.[A-Za-z0-9_]+)+", v):
                        if m.group(0) in WORDS: cand.append(m.group(0))
            if prefix:
                ok = str(opt.get("key", ""))
                for key in WORDS:
                    if key.startswith(prefix + "."):
                        slot = key[len(prefix) + 1:]
                        if re.match(r"(?:o|out|l)_" + re.escape(ok) + r"(?:_|$)", slot): cand.append(key)
        seen = set(); hits = []
        def try_keys(keys, scope):
            for key in keys:
                if key in seen: continue
                seen.add(key)
                for wf, text in WORDS.get(key, []):
                    r = change(text, g["old"], g["new"])
                    if r:
                        hits.append({"words_key": key, "words_file": wf, "old_words": text, "new_words": r[0],
                                     "number_form": r[1], "times_in_line": r[2], "scope": scope})
        try_keys(cand, "this choice")
        if not hits and prefix:
            try_keys(sorted(k for k in WORDS if k.startswith(prefix + ".")), "elsewhere in this card: check by eye")
        # words kept inside the card file itself
        inline = []
        if opt and isinstance(opt.get("label"), str):
            r = change(opt["label"], g["old"], g["new"])
            if r: inline.append({"words_key": "(card file) options." + g["choice"] + ".label", "words_file": "encounters/" + g["file"],
                                 "old_words": opt["label"], "new_words": r[0], "number_form": r[1], "times_in_line": r[2], "scope": "card file label"})
        if not hits and not inline:
            row = dict(base); row["words_key"] = None; row["old_words"] = None; row["new_words"] = None
            row["words"] = "number not found in words"
            rows.append(row); by_file[g["file"]]["number_not_found"] += 1
        for h in hits + inline:
            row = dict(base); row.update(h)
            if h["number_form"] in ("pounds", "shillings") or h["times_in_line"] > 1 or h["scope"].startswith("elsewhere"):
                row["check"] = True
            rows.append(row); by_file[g["file"]]["words_changed"] += 1; word_files[h["words_file"]] += 1

    over60 = [g for g in gains if g["old"] > CARD_CAP]
    summary = OrderedDict()
    summary["ruling"] = "no card pays over 60 d and no chain over 240 d"
    summary["note"] = "Proposal only. No data was changed. Richest path of a chain = each card's richest choice, summed (cards taken once)."
    summary["silver_gains_seen"] = len(gains)
    summary["gains_over_60"] = len(over60)
    summary["cards_with_a_gain_over_60"] = len(set((g["file"], g["card"]) for g in over60))
    summary["chains_seen"] = len(chain_rep)
    summary["chains_over_240"] = sum(1 for c in chain_rep if c["over_240"])
    summary["chains_still_over_240_after_card_cap_so_scaled"] = sum(1 for c in chain_rep if c["scaled"])
    summary["gains_to_change"] = sum(v["gains_changed"] for v in by_file.values())
    summary["word_lines_to_change"] = sum(v["words_changed"] for v in by_file.values())
    summary["gains_with_number_not_found_in_words"] = sum(v["number_not_found"] for v in by_file.values())
    summary["rows_to_check_by_eye"] = sum(1 for r in rows if r.get("check"))
    summary["files_not_read"] = bad
    out = OrderedDict()
    out["summary"] = summary
    out["count_by_card_file"] = OrderedDict(sorted(by_file.items()))
    out["count_by_words_file"] = OrderedDict(sorted(word_files.items()))
    out["chains"] = chain_rep
    out["proposed"] = rows
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(out, f, ensure_ascii=False, indent=1)
    print(json.dumps(summary, indent=1))
    for k, v in sorted(by_file.items()): print(k, v)
    print("wrote", OUT)

if __name__ == "__main__":
    main()
