"""REAL THINGS OF THE TIME FOR THE BUTTONS' SIGNS AND THE TOP BAR, FROM WIKIMEDIA COMMONS.

The user, 8 Oct 2026: "replace the word silver with a period accurate PNG of a coin"; "replace the bread with a pic of
a barrel from the assets we have"; "and replace the button icons with wiki searched stuff you find yourself, not made
yourself".

Step 1 (`search`): for each sign a few searches on Commons; every picture found whose licence may be shipped with a
credit line (public domain, CC0, CC BY, CC BY-SA; nothing "NC" or "ND") is listed in Bake/art/out/icons/candidates.json
with its author, licence and page; a small copy is fetched, and sheets of small copies are made to choose from
(Bake/art/out/icons/sheet_<sign>.png: each picture numbered).

Step 2 (`make`): the choices (Bake/art/standin_icon_choices.json: sign -> the picture's number, where to cut it, and
how it is turned into a sign) are fetched large and made into the game's pictures in Art/standin/hud, and listed with
their credit lines in Design/art_standins.md.
  A SIGN OF A BUTTON is one colour of ink with holes in it (white, its darkness in the alpha, in dots where it is half
  dark), 32 x 32: the style sheet gives it the colour of the words beside it, so it greys and lights with its button.
  THE COIN AND THE BARREL of the top bar keep their light and shade, in the tones of the game (standin_filter.tone).

Run:  python standin_icons.py search [sign ...]      then      python standin_icons.py make
"""
import io, json, os, sys, time
from standin_search import get, fetch, shippable, plain

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(HERE, "out", "icons")
CHOICES = os.path.join(HERE, "standin_icon_choices.json")

# what to look for, sign by sign: a thing of the time (about 800) that says what the button does
QUERIES = {
    "coin": ["Charlemagne denier Dorestad", "denier Charlemagne", "Carolingian denier Charlemagne monogram", "denarius Karel de Grote Dorestad", "Charlemagne denier obverse reverse"],
    "cast_off": ["Ladby anchor", "Oseberg ship anchor", "Anker Wikinger Museum", "medieval anchor iron", "anchor engraving drawing old", "Viking age iron anchor museum"],
    "voyage": ["Dorestad denier ship Louis the Pious", "Carolingian coin ship Dorestad", "Gotland picture stone ship", "Hedeby coin ship", "Viking ship picture stone sail"],
    "crew": ["Vendel helmet plate warriors drawing", "Torslunda plates", "Valsgarde helmet plate warriors", "Bayeux Tapestry Stothard engraving ship", "Oseberg tapestry drawing procession", "Lewis chessmen warders", "Viking warriors picture stone drawing"],
    "hold": ["Oseberg chest", "Mastermyr chest", "Viking age chest museum", "Anglo-Saxon wooden chest"],
    "trade": ["Viking scales Birka", "Viking folding scales weights", "balance scales Viking age", "Waage Haithabu Wikinger", "Viking scales bronze"],
    "alehouse": ["Tating ware jug", "Tatinger Kanne", "Anglo-Saxon claw beaker", "Frankish glass beaker", "Anglo-Saxon drinking horn"],
    "offer": ["St Cuthbert pectoral cross", "Anglo-Saxon pectoral cross", "Carolingian cross brooch", "Frankish cross pendant", "Wilton cross"],
    "rest": ["eldstål Historiska museet", "fire striker Viking age", "fire steel early medieval", "Feuerstahl Wikinger", "strike-a-light Anglo-Saxon", "Viking oil lamp", "Oseberg lamp", "soapstone lamp Viking"],
    "look_out": ["Soderala vane", "Viking weather vane", "Heggen vane", "Kallunge vane", "Viking ship vane gilded"],
    "watch": ["Sutton Hoo helmet", "Sutton Hoo helmet replica", "Vendel helmet", "Valsgarde helmet", "Coppergate helmet", "Gjermundbu helmet"],
    "sound": ["sounding lead", "sounding lead weight ship", "lead line sounding medieval", "Senkblei Schiff Lot"],
    "hail": ["blowing horn medieval", "signal horn Viking", "olifant horn ivory", "Blashorn Mittelalter", "cattle horn blowing horn"],
    "wares": ["Reliefbandamphore", "Badorf ware", "Badorfer Keramik", "Carolingian pottery Dorestad", "Mayen quern stone"],
    "journey": ["wax tablet stylus medieval", "Springmount bog tablets", "wax tablet", "Wachstafel Griffel"],
    "errands": ["tally stick medieval", "tally sticks", "Kerbholz", "split tally stick exchequer"],
    "book": ["St Cuthbert Gospel cover", "Stonyhurst Gospel binding", "Carolingian book cover ivory", "Lindisfarne Gospels cover", "early medieval codex binding"],
    "measure": ["dividers Roman bronze", "medieval dividers compass tool", "Zirkel Bronze roemisch", "compasses drafting ancient"],
}


def search(only):
    os.makedirs(os.path.join(OUT, "small"), exist_ok=True)
    path = os.path.join(OUT, "candidates.json")
    found = json.load(io.open(path, encoding="utf-8")) if os.path.exists(path) else {}
    for sign, queries in QUERIES.items():
        if only and sign not in only:
            continue
        seen, rows = set(), []
        for q in queries:
            d = get({"action": "query", "generator": "search", "gsrsearch": q + " filetype:bitmap", "gsrnamespace": 6, "gsrlimit": 10,
                     "prop": "imageinfo", "iiprop": "url|size|extmetadata", "iiurlwidth": 320, "format": "json"})
            pages = sorted((d.get("query", {}).get("pages", {}) or {}).values(), key=lambda p: p.get("index", 0))
            for p in pages:
                ii = (p.get("imageinfo") or [{}])[0]
                em = ii.get("extmetadata", {})
                lic = em.get("LicenseShortName", {}).get("value", "")
                if p["title"] in seen or not shippable(lic) or ii.get("width", 0) < 300 or not ii.get("thumburl"):
                    continue
                seen.add(p["title"])
                rows.append({"title": p["title"], "query": q, "url": ii.get("url"), "thumb": ii.get("thumburl"), "page": ii.get("descriptionurl"),
                             "width": ii.get("width"), "height": ii.get("height"), "licence": lic,
                             "author": plain(em.get("Artist", {}).get("value", ""))[:160], "credit": plain(em.get("Credit", {}).get("value", ""))[:160],
                             "what": plain(em.get("ImageDescription", {}).get("value", ""))[:200]})
            time.sleep(0.4)
        rows = rows[:18]
        for i, r in enumerate(rows):
            r["n"] = i + 1
            r["small"] = os.path.join(OUT, "small", "%s_%02d.jpg" % (sign, i + 1))
            if os.path.exists(r["small"]):
                os.remove(r["small"])
            fetch(r["thumb"], r["small"])
            time.sleep(0.15)
        found[sign] = rows
        sheet(sign, rows)
        print(sign, len(rows))
    with io.open(path, "w", encoding="utf-8") as f:
        json.dump(found, f, ensure_ascii=False, indent=1)


def sheet(sign, rows):
    from PIL import Image, ImageDraw
    cell, cols = 250, 6
    lines = max(1, (len(rows) + cols - 1) // cols)
    im = Image.new("RGB", (cols * cell, lines * (cell + 4)), (30, 30, 34))
    d = ImageDraw.Draw(im)
    for i, r in enumerate(rows):
        try:
            t = Image.open(r["small"]).convert("RGB")
        except Exception:
            continue
        t.thumbnail((cell - 8, cell - 8))
        x, y = (i % cols) * cell + 4, (i // cols) * (cell + 4) + 2
        im.paste(t, (x, y))
        d.rectangle((x, y, x + 34, y + 20), fill=(0, 0, 0))
        d.text((x + 4, y + 3), str(r["n"]), fill=(255, 230, 120))
    im.save(os.path.join(OUT, "sheet_%s.png" % sign))


if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 else ""
    if what == "search":
        search(set(sys.argv[2:]))
    else:
        print(__doc__)
