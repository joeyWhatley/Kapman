# -*- coding: utf-8 -*-
"""
MAKE READINGS - adds the long readings to the chest's texts table (Assets/Kapman/Game/Data/texts_793.json):
Beowulf in its 43 fitts, The Seafarer, The Wanderer, Deor, The Battle of Maldon, Hávamál stanzas, Ælfric's merchant,
the Old Saxon baptismal vow, Alcuin on Lindisfarne, the 793 annal, the Irish "Viking terror" quatrain.
Sources are the downloaded files in SRC (Gutenberg, Wikisource, the Internet Archive OCR, sacred-texts); each entry
records where it came from and who translated it. Run from Kapman2/Bake:  run.bat guide/make_readings.py [--print]
--print only shows the cut boundaries; without it the table is rewritten (new ids only; existing entries untouched).
"""
import io, json, os, re, sys

SRC = r"C:\Users\joeyw\AppData\Local\Temp\texts"
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Kapman", "Game", "Data", "texts_793.json")
PRINT_ONLY = "--print" in sys.argv


def read(name):
    with io.open(os.path.join(SRC, name), "r", encoding="utf-8", errors="replace") as f:
        return f.read().replace("\r\n", "\n").split("\n")


ROMAN = {}
def roman(n):
    vals = [(40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")]
    s = ""
    for v, r in vals:
        while n >= v:
            s += r
            n -= v
    return s


# ------------------------------------------------------------------------------------------------ Beowulf
def beowulf_hall():
    lines = read("beowulf_gummere.txt")          # (the file is J. Lesslie Hall's 1892 translation, Gutenberg #16328)
    heads = [i for i, l in enumerate(lines) if re.match(r"^[IVXL]+\.\s*$", l)]
    assert len(heads) == 43, len(heads)
    end = next(i for i, l in enumerate(lines) if i > heads[-1] and l.startswith("ADDENDA"))
    fitts = []
    for k, h in enumerate(heads):
        stop = heads[k + 1] if k + 1 < 43 else end
        title = next(l.strip() for l in lines[h + 1:stop] if l.strip())
        body = "\n".join(lines[h + 1:stop])
        body = body.replace(title, "", 1)
        body = re.sub(r"\{[^}]*\}", "", body, flags=re.S)           # the editor's glosses
        body = re.sub(r"\[\d+\]", "", body)                           # footnote marks
        out = []
        for l in body.split("\n"):
            l = re.sub(r"^\s*\d+\s", " ", l).strip()
            if l and not re.match(r"^[\d\s\.]+$", l):
                out.append(l)
        title = title.replace("_", "").replace(".--", ": ").replace("--", ": ").rstrip(".")
        title = " ".join(w[:1].upper() + w[1:].lower() for w in title.split(" "))
        title = re.sub(r": ([a-z])", lambda m: ": " + m.group(1).upper(), title).replace("(continued)", "(continued)")
        fitts.append((title, "\n".join(out)))
    return fitts


def beowulf_oe():
    lines = read("beowulf_oe_9701.txt")
    heads, want = [], 1
    for i, l in enumerate(lines):
        if i < 430 or i > 3800:
            continue
        m = re.match(r"^([IVXL]+)\. (.+)$", l)
        if m and m.group(1) == roman(want):
            heads.append(i)
            want += 1
    assert len(heads) == 43, len(heads)
    end = next(i for i, l in enumerate(lines) if i > heads[-1] and "LIST OF NAMES" in l)
    fitts = []
    for k, h in enumerate(heads):
        stop = heads[k + 1] if k + 1 < 43 else end
        out = []
        for l in lines[h + 1:stop]:
            l = re.sub(r"^\s*\d+\s+", "", l).rstrip()
            l = re.sub(r"^\s+", "", l)
            l = re.sub(r" {3,}", "  ", l)
            if l:
                out.append(l)
        fitts.append("\n".join(out))
    return fitts


# ------------------------------------------------------------------------------------------------ Cook & Tinker (OCR)
OCR_JUNK = re.compile(r"[■^\\{}|_*~`#@$%<>=+°]")

def clean_ocr_verse(lines, headers):
    """Cook & Tinker 1902 from the Internet Archive OCR: drops page headers and margin line numbers, mends the
    printer's wrapped lines (a continuation starts in lower case, or is short with no stop before it)."""
    out = []
    for l in lines:
        s = OCR_JUNK.sub("", l).strip()
        if not s or any(re.match(h, s) for h in headers):
            continue
        s = re.sub(r"^\d{1,3}\s+", "", s)                     # a line number at the start
        s = re.sub(r"\s+(\d{1,3}|lo|i\d|\d[oO])\s*$", "", s)   # at the end (OCR reads 10 as lo)
        s = re.sub(r"\s+,", ",", s).replace(" ;", ";").replace(" !", "!").replace(" ?", "?").replace(" :", ":")
        s = re.sub(r"\s{2,}", " ", s).strip()
        if not s:
            continue
        if out and (s[0].islower() or (len(s) <= 12 and not re.search(r"[.;!?:]$", out[-1]))):
            out[-1] = (out[-1] + " " + s).strip()
        else:
            out.append(s)
    return "\n".join(out)


def cook_tinker(first, last, start_after=None, headers=()):
    lines = read("cu31924059963672.txt")[first - 1:last]
    if start_after is not None:
        idx = next(i for i, l in enumerate(lines) if re.search(start_after, l))
        lines = lines[idx + 1:]
    hdr = [r"^THE SEAFARER\b", r"^THE WANDERER\b", r"^DEOR.S LAMENT\b", r"^THE BATTLE OF MALDON\b", r"^\d*\s*SECULAR LYRICS\b",
           r"^\d*\s*HEROIC POEMS\b", r"^\d+\s*$"] + list(headers)
    text = clean_ocr_verse(lines, hdr)
    return re.sub(r"^\(?yi\b", "I", text)                 # the OCR's reading of the opening "I"


# ------------------------------------------------------------------------------------------------ Wikisource (Bright's Reader)
def wiki_oe(text):
    text = re.sub(r"<noinclude>.*?</noinclude>", "", text, flags=re.S)
    text = re.sub(r"<ref>.*?</ref>", "", text, flags=re.S)
    text = re.sub(r"\{\{(ces|cesura)\}\}", "  ", text)
    text = re.sub(r"\{\{pline\|[^}]*\}\}", "", text)
    text = re.sub(r"\{\{(block center|lang block)[^}]*\}\}", "", text)
    text = re.sub(r"\{\{rh\|[^}]*\}\}", "", text)
    text = re.sub(r"<br\s*/?>", "\n", text)
    text = re.sub(r"\{\{ppoem\|", "", text)
    text = text.replace("}}", "")
    out = []
    for l in text.split("\n"):
        l = re.sub(r"^:+\s*", "", l).strip()
        l = re.sub(r" {3,}", "  ", l)
        if l and not l.startswith("{{") and not l.startswith("[[") and not l.startswith("----") and not l.startswith("|"):
            out.append(l)
    return "\n".join(out)


def wanderer_oe():
    text = ""
    for p in (246, 247, 248, 249, 250):
        text += "\n".join(read("wanderer_p%d.txt" % p)) + "\n"
    text = text[text.index("Oft him"):]
    return wiki_oe(text)


def maldon_oe():
    lines = read("maldon_oe.txt")
    i = next(k for k, l in enumerate(lines) if "{{ppoem|" in l)
    text = "\n".join(lines[i:])
    text = wiki_oe(text)
    return text.replace("[...] ", "... ")


# ------------------------------------------------------------------------------------------------ Hávamál
HAV = [1, 2, 3, 4, 5, 6, 7, 10, 11, 12, 15, 21, 23, 34, 35, 36, 38, 41, 43, 44, 47, 54, 68, 70, 76, 77, 81]

def havamal_on():
    lines = read("havamal_is.txt")
    st, cur, n = {}, [], None
    for l in lines + [":'''999.'''"]:
        m = re.match(r"^:'''(\d+)\.'''\s*(.*)$", l)
        if m:
            if n is not None:
                st[n] = "\n".join(x for x in cur if x)
            n, cur = int(m.group(1)), [m.group(2).strip()]
        elif n is not None and l.startswith(":"):
            cur.append(l[1:].strip())
    return st


def havamal_en():
    """Bellows (sacred-texts): each stanza is one <p>N. ...<br>...</p>; his notes are <p>[...]</p> blocks that can
    themselves start with a number; a stanza broken by a page runs on in an unnumbered <p>."""
    html = "\n".join(read("havamal_bellows_old.html"))
    st, cur, in_note = {}, None, False
    for p in re.findall(r"<p>(.*?)</p>", html, flags=re.S):
        p = re.sub(r"<br\s*/?>", "\n", p)
        t = re.sub(r"<[^>]+>", "", p).strip()
        if not t:
            continue
        if in_note:
            if t.endswith("]"):
                in_note = False
            continue
        if t.startswith("["):
            in_note = not t.endswith("]")
            continue
        if re.match(r"^p\. \d+$", t):
            continue
        m = re.match(r"^(\d+)\.\s+(.*)$", t, flags=re.S)
        lines = [x.strip() for x in (m.group(2) if m else t).split("\n") if x.strip()]
        if m:
            cur = int(m.group(1))
            if cur not in st:
                st[cur] = "\n".join(lines)
        elif cur is not None and cur in st and lines and lines[0][0].isupper():
            st[cur] += "\n" + "\n".join(lines)
    return st


# ------------------------------------------------------------------------------------------------ the short ones
def colloquy_merchant():
    lines = read("colloquy_ws.txt")
    def line(k):
        return re.sub(r"\{\{left sidenote\|[^}]*\}\}", "", lines[k - 1]).strip()
    return line(195) + "\n" + line(199)


def giles_793():
    lines = read("asc_giles.txt")
    i = next(k for k, l in enumerate(lines) if l.startswith("A.D. 793."))
    out = []
    for l in lines[i:]:
        if l.startswith("A.D. 794."):
            break
        out.append(l.strip())
    return re.sub(r"\s+", " ", " ".join(out)).replace("A.D. 793. ", "").replace("fiery, dragons", "fiery dragons").strip()


OWN = "the game's own draft from the original (for the user to edit)"

def entry(id, title, people, form, language, original, translation, translator, translated, died, where, written, told, credit, **extra):
    e = {"id": id, "title": title, "people": people, "form": form, "language": language, "script": "latin",
         "original": original, "translation": translation, "translator": translator, "translated": translated,
         "translatorDied": died, "where": where, "writtenDown": written, "toldLater": told, "credit": credit}
    e.update(extra)
    return e


def build():
    new = []
    # Beowulf
    hall, oe = beowulf_hall(), beowulf_oe()
    for n in range(43):
        title, eng = hall[n]
        new.append(entry(
            "beowulf_%02d" % (n + 1), "Beowulf, fitt %s: %s" % (roman(n + 1), title), "english", "book", "Old English",
            oe[n], eng, "John Lesslie Hall", 1892, 1928,
            {"editor": "James A. Harrison (d. 1911) and Robert Sharp (d. 1920)", "title": "Beówulf: an Anglo-Saxon Poem, 4th ed. (Boston: Ginn, 1894)",
             "where": "Project Gutenberg #9701; the fitt numbers are the manuscript's"},
            "c. 975-1025 (London, BL Cotton Vitellius A.xv); the poem itself older", False,
            "Old English: Harrison and Sharp (1894), Gutenberg #9701. English: John Lesslie Hall, Beowulf: An Anglo-Saxon Epic Poem (Boston: D. C. Heath, 1892), Gutenberg #16328. Hall's marginal glosses and footnotes are left out.",
            acquire={"how": "told", "who": "a crewman who has the song by heart: one fitt at a time (the reading in parts, 3 Oct)"},
            series={"id": "beowulf", "part": n + 1, "of": 43}))
    # The Seafarer
    new.append(entry(
        "seafarer", "The Seafarer", "english", "parchment", "Old English",
        "[The Old English of the Exeter Book (folios 81v-83r) is not transcribed here yet; Gollancz's EETS edition of 1895 is free and has it.]",
        cook_tinker(2339, 2602, start_after=r"^PART I"), "Lola La Motte Iddings", 1902, 0,
        {"editor": "Albert S. Cook (d. 1927) and Chauncey B. Tinker", "title": "Select Translations from Old English Poetry (Boston: Ginn, 1902), pp. 44-49", "where": "Internet Archive: cu31924059963672 (OCR, cleaned; check against the page images)"},
        "c. 975 (the Exeter Book)", False,
        "English: Lola La Motte Iddings, in Cook and Tinker, Select Translations from Old English Poetry (1902). Free in the US (1902); the translator's death year is not found, so check before shipping outside the US, or take another free translation.",
        rightsNote="translator's death year unknown; US-free by date"))
    # The Wanderer
    new.append(entry(
        "wanderer", "The Wanderer", "english", "parchment", "Old English",
        wanderer_oe(), cook_tinker(2603, 2869, start_after=r"Lament \(p\. 58\)", headers=(r"^THE WANDERER",)),
        "Emily H. Hickey", 1881, 1924,
        {"editor": "James W. Bright", "title": "Bright's Anglo-Saxon Reader (New York: Holt, 1891), XXIII The Wanderer, pp. 160-164", "where": "Wikisource, Page:Bright's Anglo-Saxon Reader.djvu/246-250; English: Cook and Tinker (1902) pp. 50-55, IA cu31924059963672 (OCR, cleaned)"},
        "c. 975 (the Exeter Book)", False,
        "Old English: Bright's Anglo-Saxon Reader (1891) by Wikisource. English: Emily H. Hickey (The Academy, 1881; in Cook and Tinker, 1902)."))
    # Deor
    new.append(entry(
        "deor", "Deor's Lament", "english", "parchment", "Old English",
        "[The Old English of the Exeter Book (folio 100r-v) is not transcribed here yet.]",
        cook_tinker(2943, 3184, start_after=r"free paraphrase"), "Francis A. Blackburn", 1902, 0,
        {"editor": "Cook and Tinker", "title": "Select Translations from Old English Poetry (1902), pp. 58-60", "where": "IA cu31924059963672 (OCR, cleaned)"},
        "c. 975 (the Exeter Book)", False,
        "English: Francis A. Blackburn, 'a free paraphrase', in Cook and Tinker (1902). Free in the US by date; the translator's death year is not found: check.",
        rightsNote="translator's death year unknown; US-free by date"))
    # Maldon
    new.append(entry(
        "maldon_991", "The Battle of Maldon (991)", "english", "parchment", "Old English",
        maldon_oe(), cook_tinker(1724, 2338, start_after=r"Development, New York, 1902", headers=(r"^THE BATTLE OF MALDON", r"^\d*\s*EPIC AND HISTORICAL PIECES")),
        "H. W. Lumsden", 1902, 0,
        {"editor": "James W. Bright", "title": "Bright's Anglo-Saxon Reader (1891), The Battle of Maldon", "where": "Wikisource, Bright's Anglo-Saxon Reader/The Battle of Maldon; English: Cook and Tinker (1902) pp. 31-43, IA cu31924059963672 (OCR, cleaned)"},
        "soon after 991 (Cotton Otho A.xii, burned 1731; Hearne's print of 1726)", False,
        "Old English: Bright's Anglo-Saxon Reader (1891) by Wikisource. English: H. W. Lumsden, in Cook and Tinker (1902). The translator's death year is not found: check before shipping outside the US.",
        rightsNote="translator's death year unknown; US-free by date"))
    # Hávamál
    on, en = havamal_on(), havamal_en()
    EN_OF = {76: 77, 77: 78}          # Bellows puts "Fitjung's sons" (Icelandic 78) before "Cattle die" (76-77)
    for n in HAV:
        b = EN_OF.get(n, n)
        if n not in on or b not in en:
            print("missing Havamal stanza", n, n in on, b in en)
            continue
        new.append(entry(
            "havamal_%03d" % n, "Hávamál, stanza %d" % n, "norse", "rune_stick", "Old Norse",
            on[n], en[b], "Henry Adams Bellows", 1923, 1939,
            {"title": "The Poetic Edda, translated by Henry Adams Bellows (New York: American-Scandinavian Foundation, 1923), Hovamol", "where": "sacred-texts.com/neu/poe/poe04.htm; Old Norse from is.wikisource.org/wiki/Hávamál (normalized modern Icelandic spelling: check against Bugge 1867 if the old spelling is wanted)"},
            "c. 1270 (Codex Regius, GKS 2365 4to); the sayings older", True,
            "Old Norse: Hávamál (is.wikisource, normalized). English: H. A. Bellows, The Poetic Edda (1923). Bellows' bracketed doubtful lines are kept in parentheses."))
    # Ælfric's merchant
    new.append(entry(
        "aelfric_merchant", "The merchant speaks (Ælfric's Colloquy)", "english", "parchment", "Old English",
        colloquy_merchant(),
        "I go aboard my ship with my cargo and row over the parts of the sea, and sell my things and buy precious things that are not produced in this land, and I bring them here to you with great danger over the sea, and sometimes I suffer shipwreck with the loss of all my things, barely escaping alive.\n"
        "Purple cloth and silk, precious gems and gold, strange garments and spices, wine and oil, ivory and brass, copper and tin, sulphur and glass, and many such things.",
        OWN, 2026, 0,
        {"title": "Ælfric's Colloquy, the Old English gloss (London, BL Cotton Tiberius A.iii)", "where": "en.wikisource.org/wiki/Ælfric's_Colloquy (the page gives no edition: check against Thorpe, Analecta Anglo-Saxonica, 1834, or Garmonsway's text)"},
        "c. 1000 (Latin by Ælfric); the Old English gloss 11th c.", False,
        "Old English: Ælfric's Colloquy via Wikisource. English: the game's own draft.", ownTranslation=True))
    # the Old Saxon baptismal vow
    new.append(entry(
        "saxon_vow", "The Saxon baptismal vow", "saxon", "parchment", "Old Saxon",
        "Forsachistu diobolae? - ec forsacho diabolae.\nend allum diobolgelde? - end ec forsacho allum diobolgeldae.\nend allum dioboles uuercum? - end ec forsacho allum dioboles uuercum and uuordum, Thunaer ende Uoden ende Saxnote ende allum them unholdum, the hira genotas sint.\ngelobistu in got alamehtigan fadaer? - ec gelobo in got alamehtigan fadaer.",
        "Do you forsake the devil? - I forsake the devil.\nAnd all offerings to devils? - And I forsake all offerings to devils.\nAnd all the devil's works? - And I forsake all the devil's works and words, Thunaer and Woden and Saxnot and all the fiends that are their companions.\nDo you believe in God the almighty Father? - I believe in God the almighty Father.",
        OWN, 2026, 0,
        {"title": "Interrogationes et responsiones baptismales (Vatican, Cod. Pal. lat. 577, 9th c.)", "where": "MGH Capitularia regum Francorum I (Boretius, 1883), pp. 222-223; Old Saxon text via Wikipedia's transcription (check against the MGH page)"},
        "late 8th c. (a 9th-c. copy)", False,
        "Old Saxon: the Utrecht/Mainz baptismal vow, MGH Capit. I pp. 222-223. English: the game's own draft.", ownTranslation=True))
    # Alcuin on Lindisfarne
    new.append(entry(
        "alcuin_793", "Alcuin to King Æthelred of Northumbria, 793", "latin-church", "parchment", "Latin",
        "Ecce trecentis et quinquaginta ferme annis, quod nos nostrique patres huius pulcherrimae patriae incolae fuimus, et numquam talis terror prius apparuit in Brittannia, veluti modo a pagana gente perpessi sumus, nec eiusmodi navigium fieri posse putabatur.",
        "Lo, it is nearly three hundred and fifty years that we and our fathers have dwelt in this most lovely land, and never before has such a terror appeared in Britain as we have now suffered from a heathen people, nor was it thought that such a voyage could be made.",
        OWN, 2026, 0,
        {"title": "Alcuin, Epistola 16 (to Æthelred), MGH Epistolae IV (Dümmler, 1895), p. 42", "where": "the Latin is quoted from memory of the MGH text: CHECK against dmgh.de before use"},
        "793", False,
        "Latin: Alcuin, ep. 16, MGH Epp. IV p. 42 (check). English: the game's own draft.", ownTranslation=True, check=True))
    # the 793 annal
    new.append(entry(
        "chronicle_793", "The Chronicle for 793", "english", "parchment", "Old English",
        "Her wæron reðe forebecna cumene ofer Norðhymbra land, ⁊ þæt folc earmlic bregdon: þæt wæron ormete þodenas ⁊ ligrescas, ⁊ fyrenne dracan wæron gesewene on þam lifte fleogende. Þam tacnum sona fyligde mycel hunger, ⁊ litel æfter þam, þæs ilcan geares on .vi. Idus Ianuarii, earmlice hæþenra manna hergunc adilegode Godes cyrican in Lindisfarnaee þurh hreaflac ⁊ mansliht.",
        giles_793(), "J. A. Giles", 1847, 1884,
        {"title": "The Anglo-Saxon Chronicle, translated by J. A. Giles (London: Bohn, 1847), the Peterborough text", "where": "Project Gutenberg #657; the Old English (Peterborough Chronicle, E) is quoted from memory: CHECK against Thorpe 1861 or Plummer 1892"},
        "the E text c. 1121 copying older annals; the D and E texts carry this entry", False,
        "Old English: the Peterborough Chronicle (E) for 793 (check). English: J. A. Giles (1847), Gutenberg #657.", check=True))
    # the Irish Viking terror
    new.append(entry(
        "irish_viking_terror", "Bitter is the wind tonight", "irish", "parchment", "Old Irish",
        "Is acher in gaíth in-nocht,\nfu-fuasna fairggae findfolt;\nní ágor réimm mora minn\ndond láechraid lainn úa Lothlind.",
        "Bitter is the wind to-night,\nIt tosses the ocean's white hair:\nTo-night I fear not the fierce warriors of Norway\nCoursing on the Irish Sea.",
        "Kuno Meyer", 1911, 1919,
        {"title": "Selections from Ancient Irish Poetry, translated by Kuno Meyer (London: Constable, 1911), 'The Viking Terror', p. 101", "where": "Internet Archive: selectionsfroman1911meye; Old Irish from the St Gall Priscian (Stiftsbibliothek 904, p. 112), as in Thesaurus Palaeohibernicus II (1903): quoted from memory, CHECK"},
        "c. 845-850 (a margin of the St Gall Priscian)", False,
        "Old Irish: St Gall Priscian margin, Thes. Pal. II (check). English: Kuno Meyer (1911).", check=True))
    return new


def main():
    new = build()
    if PRINT_ONLY:
        for e in new:
            o, t = e["original"], e["translation"]
            print("==", e["id"], "|", e["title"], "| original %d chars, english %d chars" % (len(o), len(t)))
            print("   O:", o[:160].replace("\n", " / "))
            print("   E:", t[:220].replace("\n", " / "))
        return
    with io.open(OUT, "r", encoding="utf-8", newline="") as f:
        raw = f.read()
    crlf = "\r\n" in raw
    data = json.loads(raw)
    have = {e["id"] for e in data["texts"]}
    added = 0
    for e in new:
        if e["id"] in have:
            print("skip (exists):", e["id"])
            continue
        data["texts"].append(e)
        added += 1
    data["about"] = data["about"].rstrip() + " Readings added 4 Oct 2026 by Bake/guide/make_readings.py: Beowulf (43 fitts), The Seafarer, The Wanderer, Deor, Maldon, Hávamál stanzas, Ælfric's merchant, the Saxon vow, Alcuin 793, the 793 annal, the Irish Viking quatrain; entries with check=true carry an original quoted from memory to be checked; ownTranslation=true marks the game's own drafts for the user to edit."
    s = json.dumps(data, ensure_ascii=False, indent=1)
    if crlf:
        s = s.replace("\n", "\r\n")
    with io.open(OUT, "w", encoding="utf-8", newline="") as f:
        f.write(s + ("\r\n" if crlf else "\n"))
    print("added", added, "entries; table now holds", len(data["texts"]))


if __name__ == "__main__":
    main()
