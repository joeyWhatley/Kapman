# -*- coding: utf-8 -*-
r"""
CHECK WIKI - looks over Bake/content/out/wiki_793.json, which make_wiki.py bakes (Design/wiki.md).
  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\check_wiki.py [file]

FAULTS (the script ends with 1 if there is any):
  - two pages share an id; a page lacks a field, or its kind or its 'opens' rule is not one the frame knows;
  - a link leads to no page;
  - a name leads to no page, or stands twice, or the names are not longest first;
  - a page of the first release is empty (no title, no brief, or no game text);
  - a 'later' piece has no date or no text; a How to play page does not say whether it is built.
LISTED, NOT FAULTS:
  - every word of part (b) of Design/content/lore/period_words.md (modern words that must not stand in a line of
    793 to 1066) found in a page's title, hint, brief, game, known or later text. Many are fine in the book's own
    voice (the frame's Hold window, the chart, the clock) and wrong only in a mouth: the list is for a person to read;
  - pages with nothing under 'known', by kind; how many names each kind has.
"""
import collections
import io
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
FILE = os.path.join(HERE, "out", "wiki_793.json")
WORDS = os.path.join(ROOT, "Design", "content", "lore", "period_words.md")
KINDS = ["how_to_play", "port", "site", "person", "good", "ship", "people", "faith", "day", "word", "event", "text"]
FIELDS = ["id", "kind", "title", "hint", "brief", "game", "known", "source", "links", "opens", "first_release"]
OPENS = re.compile(r"^(?:always|(?:port_heard|port_seen|good_seen|person_met|word_heard|text_held):[^&\s]+|date:\d{3,4}-\d\d-\d\d)$")


def period_words(path):
    """The words of part (b): the first cell of each table row, and the list of things that are not there."""
    with io.open(path, "r", encoding="utf-8-sig") as f:
        text = f.read()
    a = text.find("## (b)")
    b = text.find("## (c)")
    if a < 0:
        return []
    part = text[a:b if b > a else len(text)]
    out = []          # (the word, in what sense it is forbidden: '' when it is forbidden outright)

    def add(term):
        sense = ""
        m = re.search(r"\(([^()]*)\)", term)
        if m:
            sense = m.group(1).strip()                         # (as sixty minutes)
        term = re.sub(r"\([^()]*\)", " ", term)
        cut = re.split(r"\s+as\s+", term, 1)                   # fate as "destiny" in the modern soft sense
        if len(cut) > 1 and not sense:
            sense = "as " + cut[1].strip().replace('"', "'")
        term = cut[0]
        term = term.replace("*", "").replace('"', "").replace("\u201c", "").replace("\u201d", "")
        term = re.sub(r"\s+", " ", term).strip(" .!?:;'")
        term = re.sub(r"^(?:the|a|an) ", "", term)
        if not term or re.search(r"\d", term) or len(term) < 2 or len(term.split()) > 4:
            return
        if term not in [t for t, _ in out]:
            out.append((term, sense))

    for line in part.splitlines():
        line = line.strip()
        if line.startswith("|") and not line.startswith("|---") and "Do not write" not in line:
            cell = line.strip("|").split("|")[0]
            cell = re.sub(r"\(([^()]*,[^()]*)\)", " ", cell)   # a bracket with commas in it would cut wrongly
            for term in cell.split(","):
                m = re.match(r"^(.*?)\(s\)(.*)$", term.strip())
                if m:                                          # Viking(s)
                    add(m.group(1) + m.group(2))
                    add(m.group(1) + "s" + m.group(2))
                else:
                    add(term)
    m = re.search(r"\*\*Things that are not there\*\*\s*(.+?)(?:\n\s*\n)", part, re.S)
    if m:
        para = re.sub(r"\([^()]*\)", " ", m.group(1))
        for sentence in re.split(r"\.\s+", para.replace("\n", " ")):
            head = sentence.split(":")[0]
            for term in re.split(r",| and ", head):
                term = term.strip()
                if term and len(term.split()) <= 2:
                    add(term[0].lower() + term[1:])
    m = re.search(r"\*\*Famous lines that are not real\.\*\*(.+?)(?:\n---|\Z)", part, re.S)
    if m:
        for line in m.group(1).splitlines():
            if line.strip().startswith("- "):
                head = line.split(":")[0]
                for x in re.findall(r"\*([^*]+)\*", head) + re.findall(r'"([^"]+)"', head):
                    add(x)
    return out


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    path = sys.argv[1] if len(sys.argv) > 1 else FILE
    try:
        with io.open(path, "r", encoding="utf-8-sig") as f:
            data = json.load(f)
    except Exception as e:
        print("CHECK WIKI: cannot read %s: %s" % (path, e))
        return 1
    pages = data.get("pages", [])
    names = data.get("names", [])
    faults = collections.OrderedDict()

    def fault(kind, msg):
        faults.setdefault(kind, []).append(msg)

    # ids, fields, kinds, rules
    ids = collections.Counter(p.get("id") for p in pages)
    for pid, n in ids.items():
        if n > 1:
            fault("two pages share an id", "%s (%d times)" % (pid, n))
    by_id = {p.get("id"): p for p in pages}
    for p in pages:
        pid = p.get("id", "(no id)")
        for f in FIELDS:
            if f not in p:
                fault("a page lacks a field", "%s: %s" % (pid, f))
        if p.get("kind") not in KINDS:
            fault("a kind the frame does not know", "%s: %s" % (pid, p.get("kind")))
        if not isinstance(p.get("first_release"), bool):
            fault("first_release is not true or false", pid)
        for part in str(p.get("opens", "")).split("&"):
            if not OPENS.match(part):
                fault("an 'opens' rule the frame does not know", "%s: %s" % (pid, p.get("opens")))
        for x in p.get("later", []) or []:
            if not re.match(r"^\d{3,4}-\d\d-\d\d$", str(x.get("date", ""))) or not x.get("text") or x.get("part") not in ("game", "known"):
                fault("a 'later' piece without a date, a part or a text", pid)
        if p.get("kind") == "how_to_play" and not isinstance(p.get("built"), bool):
            fault("a How to play page does not say whether it is built", pid)

    # links
    for p in pages:
        for link in p.get("links", []) or []:
            if link not in by_id:
                fault("a link leads to no page", "%s -> %s" % (p.get("id"), link))
            elif link == p.get("id"):
                fault("a page links to itself", p.get("id"))

    # names
    seen = collections.Counter(x.get("name") for x in names)
    for n, c in seen.items():
        if c > 1:
            fault("a name stands twice", "%s (%d times)" % (n, c))
    for x in names:
        if x.get("page") not in by_id:
            fault("a name leads to no page", "%s -> %s" % (x.get("name"), x.get("page")))
        if not x.get("name") or not any(ch.isalpha() for ch in x.get("name", "")):
            fault("a name with no letters", repr(x.get("name")))
        for o in x.get("also", []) or []:
            if o not in by_id:
                fault("a name's 'also' leads to no page", "%s -> %s" % (x.get("name"), o))
    lengths = [len(x.get("name", "")) for x in names]
    if any(lengths[i] < lengths[i + 1] for i in range(len(lengths) - 1)):
        fault("the names are not longest first", "sort them by length, longest first")

    # the first release
    for p in pages:
        if p.get("first_release") and not (p.get("title") and p.get("brief") and p.get("game")):
            lack = [f for f in ("title", "brief", "game") if not p.get(f)]
            fault("a page of the first release is empty", "%s lacks %s" % (p.get("id"), ", ".join(lack)))

    try:
        shown = os.path.relpath(path, ROOT).replace("\\", "/")
    except ValueError:                                         # (another drive)
        shown = path
    print("CHECK WIKI: %s" % shown)
    print("  %d pages, %d names" % (len(pages), len(names)))
    print("  %-12s %6s %14s %12s %8s %7s" % ("kind", "pages", "first release", "known empty", "links", "names"))
    name_kind = collections.Counter(by_id[x["page"]]["kind"] for x in names if x.get("page") in by_id)
    for k in KINDS:
        mine = [p for p in pages if p.get("kind") == k]
        print("  %-12s %6d %14d %12d %8d %7d" % (
            k, len(mine), sum(1 for p in mine if p.get("first_release")), sum(1 for p in mine if not p.get("known")),
            sum(len(p.get("links") or []) for p in mine), name_kind.get(k, 0)))
    rules = [p for p in pages if p.get("kind") == "how_to_play"]
    held = [p["id"] for p in rules if p.get("built") is False]
    print("  how to play: %d built; %d ruled but not built, to be held back: %s" % (
        len(rules) - len(held), len(held), ", ".join(x.replace("rule.", "") for x in held)))
    unlinked = [p["id"] for p in pages if not p.get("links")]
    reached = set(l for p in pages for l in p.get("links") or [])
    named = set(x.get("page") for x in names)
    lonely = [p["id"] for p in pages if p["id"] not in reached and p["id"] not in named and p.get("kind") != "event"]
    print("  pages that lead nowhere: %d; pages (events apart: they have no names) that no link and no name leads to: %d%s" % (
        len(unlinked), len(lonely), (" (" + ", ".join(lonely[:10]) + (" ..." if len(lonely) > 10 else "") + ")") if lonely else ""))

    if faults:
        print("FAULTS:")
        for kind, rows in faults.items():
            print("  %s: %d" % (kind, len(rows)))
            for r in rows[:12]:
                print("    " + r)
            if len(rows) > 12:
                print("    ... and %d more" % (len(rows) - 12))
    else:
        print("NO FAULTS: every link leads to a page; no two pages share an id; every name leads to one page; "
              "no page of the first release is empty.")

    # the modern words of period_words.md, part (b)
    if not os.path.isfile(WORDS):
        print("period_words.md is missing: the modern words were not looked for (%s)" % WORDS)
    else:
        terms = period_words(WORDS)
        found = collections.OrderedDict()
        texts = []
        for p in pages:
            body = " \n ".join([p.get("title", ""), p.get("hint", ""), p.get("brief", ""), p.get("game", ""), p.get("known", "")] +
                               [x.get("text", "") for x in p.get("later", []) or []])
            texts.append((p, body))
        for term, sense in terms:
            tail = "" if " " in term or term.endswith("s") or len(term) < 4 else "s?"
            rx = re.compile(r"(?<![^\W\d_])" + re.escape(term) + tail + r"(?![^\W\d_])", re.I)
            hand, made, n = [], [], 0
            for p, body in texts:
                c = len(rx.findall(body))
                if c:
                    n += c
                    (hand if p.get("kind") == "how_to_play" else made).append(p["id"])
            if n:
                found[term] = (n, hand, made, sense)
        print("MODERN WORDS OF period_words.md PART (b) FOUND IN PAGES (%d of %d words; a list to read, not a fault):" % (len(found), len(terms)))
        rows = sorted(found.items(), key=lambda kv: -kv[1][0])

        def show(term, sense):
            return term + ((" (%s)" % sense) if sense else "")

        print("  in the hand-written How to play pages:")
        any_hand = False
        for term, (n, hand, made, sense) in rows:
            if hand:
                any_hand = True
                print("    %-34s %s" % (show(term, sense), ", ".join(x.replace("rule.", "") for x in hand[:8]) + (" ..." if len(hand) > 8 else "")))
        if not any_hand:
            print("    none")
        for title, want_sense in (("forbidden outright", False), ("forbidden only in the sense given in brackets", True)):
            print("  in the pages made from the data, %s (uses, pages, the first few):" % title)
            for term, (n, hand, made, sense) in rows:
                if made and bool(sense) == want_sense:
                    print("    %-34s %5d uses in %4d pages: %s" % (show(term, sense), n, len(made), ", ".join(made[:3]) + (" ..." if len(made) > 3 else "")))
    return 1 if faults else 0


if __name__ == "__main__":
    sys.exit(main())
