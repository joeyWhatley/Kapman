"""THE PEOPLE OF THE RECORD, FROM PICTURES OF THEM (the user, 7 Oct 2026: "any other characters that have media
representation use a portrait of them, otherwise use anything from the wikis or found online for all the significant
characters. Like historical characters.").

For each person of the cast and of the people of weight whom the record names, Wikimedia Commons is searched for a
picture of them that may be shipped with a credit line (their own coin, a manuscript, a mosaic, a statue, an old
engraving). `search` lists what is found (Bake/art/out/standin_people/candidates.json) and makes sheets to choose from;
`make` takes the choices (Bake/art/standin_people_choices.json: slot id -> the picture's number and where the head is),
cuts each to head and shoulders, puts it through the game's filter and writes Art/standin/portrait/<slot>.png, over the
stand-in face that slot had from the pool. The credit lines go to Bake/art/out/standin_people/credits_rows.md, for
Design/art_standins.md.

Run:  python standin_people.py search      then      python standin_people.py make
"""
import io, json, os, sys, time

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import standin_search as S
import standin_filter

ROOT = S.ROOT
OUT = os.path.join(HERE, "out", "standin_people")

# slot id -> what to look for
QUERIES = {
    "weight.charles": ["Charlemagne denier", "Karl der Große Reiterstatuette Louvre", "Charlemagne Dürer", "Charlemagne mosaic Lateran"],
    "weight.offa": ["Offa of Mercia penny", "Offa king Mercia manuscript", "Offa rex coin portrait"],
    "weight.cynethryth": ["Cynethryth penny", "Cynethryth coin"],
    "weight.cenwulf": ["Coenwulf mancus", "Coenwulf of Mercia penny"],
    "weight.ecgberht": ["Egbert of Wessex", "Ecgberht king of Wessex penny"],
    "weight.beorhtric": ["Beorhtric of Wessex"],
    "weight.ecgfrith": ["Ecgfrith of Mercia"],
    "weight.aethelberht": ["Æthelberht II of East Anglia", "Saint Ethelbert Hereford Cathedral"],
    "weight.eadwald": ["Eadwald of East Anglia penny"],
    "weight.cuthred": ["Cuthred of Kent penny"],
    "weight.praen": ["Eadberht Praen penny"],
    "weight.wulfred": ["Wulfred archbishop penny"],
    "weight.leo": ["Pope Leo III mosaic", "Leo III Triclinium Lateran", "Leo III Charlemagne coronation miniature"],
    "weight.fastrada": ["Fastrada"],
    "weight.liutgard": ["Luitgard Charlemagne wife"],
    "weight.charles_younger": ["Charles the Younger Charlemagne son", "Karl der Jüngere"],
    "weight.angilbert": ["Angilbert Saint-Riquier", "Angilbert abbot"],
    "weight.hildebold": ["Hildebold Köln Erzbischof", "Hildebold of Cologne"],
    "weight.eadburh": ["Eadburh Wessex queen"],
    "weight.sigfred": ["Sigfred Danish king", "Sigfred konge"],
    "person.cast.alcuin": ["Alcuin manuscript Raban Maur", "Alcuin of York", "Alkuin Fulda Handschrift"],
    "person.cast.anskar": ["Ansgar Hamburg statue", "Saint Ansgar painting", "Ansgar Bernt Notke"],
    "person.cast.cnut": ["Cnut Liber Vitae New Minster", "Canute king miniature", "Cnut penny"],
    "person.cast.godfred": ["Gudfred Danish king", "Godfred king of the Danes", "Kong Godfred"],
    "person.cast.horik": ["Horik I Danish king", "Kong Horik"],
    "person.cast.liudger": ["Liudger Vita secunda miniature", "Ludger Münster statue", "Saint Ludger"],
    "person.cast.ohthere": ["Ohthere of Hålogaland", "Ottar fra Hålogaland"],
    "person.cast.rollo": ["Rollo statue Rouen", "Rollon Falaise statue", "Rollo duke of Normandy miniature"],
    "person.cast.rorik": ["Rorik of Dorestad", "Rorik Koekkoek 1912"],
    "person.cast.thorkell": ["Thorkell the Tall"],
    "person.cast.higbald": ["Higbald Lindisfarne"],
    "person.cast.aethelheard": ["Æthelhard archbishop Canterbury penny", "Aethelheard archbishop"],
    "person.cast.eanbald": ["Eanbald archbishop York coin"],
    "person.cast.gervold": ["Gervold Fontenelle abbot"],
}


def search():
    os.makedirs(os.path.join(OUT, "small"), exist_ok=True)
    found = {}
    for slot, queries in QUERIES.items():
        seen, rows = set(), []
        for q in queries:
            d = S.get({"action": "query", "generator": "search", "gsrsearch": q + " filetype:bitmap", "gsrnamespace": 6, "gsrlimit": 8,
                       "prop": "imageinfo", "iiprop": "url|size|extmetadata", "iiurlwidth": 320, "format": "json"})
            pages = sorted((d.get("query", {}).get("pages", {}) or {}).values(), key=lambda p: p.get("index", 0))
            for p in pages:
                ii = (p.get("imageinfo") or [{}])[0]
                em = ii.get("extmetadata", {})
                lic = em.get("LicenseShortName", {}).get("value", "")
                if p["title"] in seen or not S.shippable(lic) or ii.get("width", 0) < 300 or not ii.get("thumburl"):
                    continue
                seen.add(p["title"])
                rows.append({"title": p["title"], "query": q, "url": ii.get("url"), "thumb": ii.get("thumburl"), "page": ii.get("descriptionurl"),
                             "width": ii.get("width"), "height": ii.get("height"), "licence": lic,
                             "author": S.plain(em.get("Artist", {}).get("value", ""))[:160], "credit": S.plain(em.get("Credit", {}).get("value", ""))[:160],
                             "what": S.plain(em.get("ImageDescription", {}).get("value", ""))[:200]})
            time.sleep(0.4)
        rows = rows[:12]
        key = slot.replace(".", "_")
        for i, r in enumerate(rows):
            r["n"] = i + 1
            r["small"] = os.path.join(OUT, "small", "%s_%02d.jpg" % (key, i + 1))
            S.fetch(r["thumb"], r["small"])
            time.sleep(0.15)
        found[slot] = rows
        print(slot, len(rows))
    with io.open(os.path.join(OUT, "candidates.json"), "w", encoding="utf-8") as f:
        json.dump(found, f, ensure_ascii=False, indent=1)
    sheets(found)


def sheets(found):
    from PIL import Image, ImageDraw
    slots = list(found.keys())
    tw, th, per = 150, 190, 4
    for s in range(0, len(slots), per):
        group = slots[s:s + per]
        sheet = Image.new("RGB", (12 * tw + 120, len(group) * (th + 16)), (30, 20, 34))
        dr = ImageDraw.Draw(sheet)
        for row, slot in enumerate(group):
            y = row * (th + 16)
            dr.text((4, y + 4), slot.split(".")[-1][:18], fill=(255, 255, 255))
            for r in found[slot]:
                try:
                    im = Image.open(r["small"]).convert("RGB")
                except Exception:
                    continue
                im.thumbnail((tw - 4, th))
                x = 120 + (r["n"] - 1) * tw
                sheet.paste(im, (x, y + 14))
                dr.rectangle((x, y, x + 22, y + 13), fill=(240, 210, 122))
                dr.text((x + 3, y + 1), str(r["n"]), fill=(0, 0, 0))
        sheet.save(os.path.join(OUT, "sheet_%02d.png" % (s // per + 1)))
    print("sheets:", (len(slots) + per - 1) // per)


def make():
    from PIL import Image
    import standin_portraits as P
    found = json.load(io.open(os.path.join(OUT, "candidates.json"), encoding="utf-8"))
    choices = json.load(io.open(os.path.join(HERE, "standin_people_choices.json"), encoding="utf-8"))
    os.makedirs(os.path.join(OUT, "large"), exist_ok=True)
    out = os.path.join(ROOT, "Art", "standin", "portrait")
    os.makedirs(out, exist_ok=True)
    lines = []
    for slot, c in choices.items():
        if not c or not c.get("n"):
            continue
        r = next((x for x in found.get(slot, []) if x["n"] == c["n"]), None)
        if r is None:
            print("no such picture:", slot, c)
            continue
        d = S.get({"action": "query", "titles": r["title"], "prop": "imageinfo", "iiprop": "url", "iiurlwidth": 1200, "format": "json"})
        page = next(iter((d.get("query", {}).get("pages", {}) or {}).values()), {})
        ii = (page.get("imageinfo") or [{}])[0]
        large = os.path.join(OUT, "large", slot.replace(".", "_") + ".jpg")
        if not S.fetch(ii.get("thumburl") or r["url"], large):
            continue
        im = Image.open(large).convert("RGB")
        standin_filter.tone(P.cut(im, tuple(c.get("box", (0.5, 0.4, 0.8))))).save(os.path.join(out, slot + ".png"))
        lines.append("| %s | %s | %s | %s | %s |" % (slot, r["title"].replace("File:", ""), r["author"] or r["credit"] or "(not given)", r["licence"], r["page"]))
        print(slot, r["title"])
        time.sleep(0.5)
    with io.open(os.path.join(OUT, "credits_rows.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    print("made", len(lines))


if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 else ""
    if what == "search":
        search()
    elif what == "sheets":
        sheets(json.load(io.open(os.path.join(OUT, "candidates.json"), encoding="utf-8")))
    elif what == "make":
        make()
    else:
        print(__doc__)
