"""REAL PICTURES FOR THE PORTS, FROM WIKIMEDIA COMMONS (the user, 7 Oct 2026: "use any wiki images of ports and places
of that period instead of AI drawn stuff").

Step 1 (this file, `search`): for each port a few searches on Commons; every picture found whose licence may be
shipped with a credit line (public domain, CC0, CC BY, CC BY-SA; nothing "NC" or "ND") is listed in
Bake/art/out/standin/candidates.json with its author, licence and page, and a small copy is fetched; sheets of small
copies are made to choose from (Bake/art/out/standin/sheet_NN.png: a row a port, each picture numbered).

Step 2 (`make`): the choices (Bake/art/standin_choices.json: port id -> the picture's number in its row, and where to
cut) are fetched large, put through the game's filter (standin_filter.py) into Art/standin/port_view, and listed with
their credit lines in Design/art_standins.md.

Run:  python standin_search.py search      then      python standin_search.py make
"""
import io, json, os, sys, time, urllib.parse, urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(HERE, "out", "standin")
API = "https://commons.wikimedia.org/w/api.php"
AGENT = "KapmanGameDev/0.1 (stand-in pictures for a game in development; contact joey.whatley99@outlook.com)"

# what to look for, port by port (the period name, the place today, what stands or was dug there)
QUERIES = {
    "traiectum": ["Traiectum castellum Utrecht reconstructie", "Utrecht vroege middeleeuwen", "Domplein Utrecht castellum maquette"],
    "walichrum": ["Domburg Walcheren vroege middeleeuwen", "Walcheren ringwalburg", "Domburg strand Nehalennia"],
    "witla": ["Maasmond vroege middeleeuwen kaart", "Brielle Maas mouth old map", "Rhine Meuse delta early medieval map"],
    "medemelacha": ["Medemblik middeleeuwen", "Medemblik Radboud kasteel prent", "Medemblik oude kaart"],
    "wijnaldum": ["Wijnaldum terp", "terp Friesland reconstructie", "Wijnaldum fibula"],
    "doccinga": ["Dokkum Bonifatius", "Dokkum oude kaart", "Dokkum terp"],
    "emuthon": ["Emden Wurt Mittelalter", "Emden alte Karte", "Emden Ratsdelft historisch"],
    "daventre": ["Deventer middeleeuwen", "Deventer Lebuinus oude prent", "Deventer oude kaart"],
    "noviomagus": ["Valkhof Nijmegen reconstructie", "Valkhof Nijmegen Karolingisch", "Valkhofburcht"],
    "quentovic": ["Quentovic", "Canche estuaire Etaples", "Montreuil-sur-Mer medieval"],
    "rotomagus": ["Rouen medieval plan", "Rouen Viking", "Rouen moyen age gravure"],
    "parisius": ["Siege of Paris 885", "Paris 9th century plan", "Saint-Denis abbaye carolingienne"],
    "ganda": ["Sint-Baafsabdij Gent", "Gent vroege middeleeuwen", "Ganda Gent abdij"],
    "andoverpis": ["Antwerpen burcht Steen middeleeuwen", "Antwerpen oude kaart burcht", "Het Steen Antwerpen oud"],
    "colonia": ["Köln Mittelalter Modell", "Köln 9. Jahrhundert", "Cologne medieval woodcut"],
    "mogontia": ["Mainz Mittelalter", "Mainz karolingisch", "Mainz Schedel"],
    "diusburg": ["Duisburg Königspfalz Modell", "Duisburg Mittelalter Stadtmodell", "Duisburg Corputius"],
    "lundenwic": ["Lundenwic", "Anglo-Saxon London", "Saxon London map"],
    "hamwic": ["Hamwic", "Saxon Southampton", "Southampton medieval walls"],
    "gipeswic": ["Ipswich Anglo-Saxon", "Gipeswic", "Ipswich ware"],
    "eoforwic": ["Jorvik reconstruction", "Jorvik Viking Centre", "York Anglian tower"],
    "sarre": ["Sarre Kent", "Wantsum Channel map", "Sarre brooch"],
    "fordwic": ["Fordwich", "Fordwich town hall river Stour"],
    "dofras": ["Dover St Mary in Castro", "Dover Anglo-Saxon", "Dover harbour old print"],
    "lindisfarena": ["Lindisfarne Priory", "Holy Island Lindisfarne harbour", "Lindisfarne old print"],
    "bebbanburg": ["Bamburgh Castle", "Bamburgh Castle old print"],
    "gyrwe": ["Jarrow monastery St Paul", "Bede's World Jarrow", "Jarrow Hall Anglo-Saxon farm"],
    "streonaeshalch": ["Whitby Abbey", "Whitby harbour old print"],
    "dommoc": ["Dunwich", "Dunwich medieval map", "Dunwich Greyfriars"],
    "legacaestir": ["Chester Roman walls", "Chester medieval map", "Chester Anglo-Saxon"],
    "hi": ["Iona Abbey", "Iona old print", "Iona St Martin's cross"],
    "duiblinn": ["Dublin Viking", "Dublinia", "Wood Quay Dublin Viking"],
    "ripa": ["Ribe Viking Center", "Ribe Vikinge marked", "Ribe old print"],
    "sliesthorp": ["Haithabu Wikinger Häuser", "Hedeby reconstruction", "Haithabu Hafen"],
    "reric": ["Reric", "Groß Strömkendorf", "Wismarer Bucht slawisch"],
    "skiringssal": ["Kaupang Viking", "Kaupang Vestfold", "Kaupang reconstruction"],
    "birka": ["Birka Viking", "Birka reconstruction", "Björkö Birka"],
    "helgo": ["Helgö", "Helgö Ekerö", "Helgö Buddha"],
    "uppsalir": ["Gamla Uppsala", "Gamla Uppsala högar", "Old Uppsala temple"],
    "paviken": ["Paviken", "Västergarn Gotland", "Gotland Viking harbour"],
    "ahus": ["Åhus Viking", "Åhus", "Åhus medieval"],
    "truso": ["Truso", "Janów Pomorski", "Truso Elbląg Viking"],
    "grobin": ["Grobiņa", "Grobina Viking", "Grobiņa pilskalns"],
    "aldeigjuborg": ["Staraya Ladoga fortress", "Staraya Ladoga", "Ladoga Volkhov kurgan"],
    "ogvaldsnes": ["Avaldsnes", "Avaldsnes Viking farm", "Avaldsnes church"],
    "hleidr": ["Lejre", "Sagnlandet Lejre", "Lejre kongehal"],
    "namnetis": ["Nantes moyen age", "Nantes medieval plan", "Nantes Vikings 843"],
    "herio": ["Noirmoutier abbaye", "Noirmoutier", "Saint Philbert Noirmoutier"],
    "burdigala": ["Bordeaux moyen age", "Bordeaux Roman walls", "Bordeaux Palais Gallien"],
}


def get(params):
    url = API + "?" + urllib.parse.urlencode(params)
    req = urllib.request.Request(url, headers={"User-Agent": AGENT})
    for attempt in range(4):
        try:
            with urllib.request.urlopen(req, timeout=40) as r:
                return json.load(r)
        except Exception as e:                                          # a slow or busy server: wait and ask again
            time.sleep(2 + attempt * 3)
            err = e
    print("no answer:", err)
    return {}


def fetch(url, path):
    if os.path.exists(path) and os.path.getsize(path) > 0:
        return True
    req = urllib.request.Request(url, headers={"User-Agent": AGENT})
    for attempt in range(4):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path, "wb") as f:
                f.write(r.read())
            return True
        except Exception as e:
            time.sleep(2 + attempt * 3)
            err = e
    print("not fetched:", url, err)
    return False


def shippable(name):
    n = (name or "").lower()
    if not n or "nc" in n.replace("-", " ").split() or "nd" in n.replace("-", " ").split():
        return False
    return n.startswith(("public domain", "pd", "cc0", "cc by", "cc-by", "no restrictions", "attribution"))


def plain(html):
    import re
    return re.sub(r"\s+", " ", re.sub(r"<[^>]+>", "", html or "")).strip()


def search():
    os.makedirs(os.path.join(OUT, "small"), exist_ok=True)
    found = {}
    for port, queries in QUERIES.items():
        seen, rows = set(), []
        for q in queries:
            d = get({"action": "query", "generator": "search", "gsrsearch": q + " filetype:bitmap", "gsrnamespace": 6, "gsrlimit": 8,
                     "prop": "imageinfo", "iiprop": "url|size|extmetadata", "iiurlwidth": 320, "format": "json"})
            pages = sorted((d.get("query", {}).get("pages", {}) or {}).values(), key=lambda p: p.get("index", 0))
            for p in pages:
                ii = (p.get("imageinfo") or [{}])[0]
                em = ii.get("extmetadata", {})
                lic = em.get("LicenseShortName", {}).get("value", "")
                if p["title"] in seen or not shippable(lic) or ii.get("width", 0) < 700 or not ii.get("thumburl"):
                    continue
                seen.add(p["title"])
                rows.append({"title": p["title"], "query": q, "url": ii.get("url"), "thumb": ii.get("thumburl"), "page": ii.get("descriptionurl"),
                             "width": ii.get("width"), "height": ii.get("height"), "licence": lic,
                             "author": plain(em.get("Artist", {}).get("value", ""))[:160], "credit": plain(em.get("Credit", {}).get("value", ""))[:160],
                             "what": plain(em.get("ImageDescription", {}).get("value", ""))[:200]})
            time.sleep(0.4)
        rows = rows[:12]
        for i, r in enumerate(rows):
            r["n"] = i + 1
            r["small"] = os.path.join(OUT, "small", "%s_%02d.jpg" % (port, i + 1))
            fetch(r["thumb"], r["small"])
            time.sleep(0.15)
        found[port] = rows
        print(port, len(rows))
    with io.open(os.path.join(OUT, "candidates.json"), "w", encoding="utf-8") as f:
        json.dump(found, f, ensure_ascii=False, indent=1)
    sheets(found)


def sheets(found):
    from PIL import Image, ImageDraw
    ports = list(found.keys())
    tw, th, per = 200, 130, 5
    for s in range(0, len(ports), per):
        group = ports[s:s + per]
        sheet = Image.new("RGB", (12 * tw + 90, len(group) * (th + 16)), (30, 20, 34))
        dr = ImageDraw.Draw(sheet)
        for row, port in enumerate(group):
            y = row * (th + 16)
            dr.text((4, y + 4), port[:13], fill=(255, 255, 255))
            for r in found[port]:
                try:
                    im = Image.open(r["small"]).convert("RGB")
                except Exception:
                    continue
                im.thumbnail((tw - 4, th))
                x = 90 + (r["n"] - 1) * tw
                sheet.paste(im, (x, y + 14))
                dr.rectangle((x, y, x + 22, y + 13), fill=(240, 210, 122))
                dr.text((x + 3, y + 1), str(r["n"]), fill=(0, 0, 0))
        sheet.save(os.path.join(OUT, "sheet_%02d.png" % (s // per + 1)))
    print("sheets:", (len(ports) + per - 1) // per)


def make():
    sys.path.insert(0, HERE)
    import standin_filter
    found = json.load(io.open(os.path.join(OUT, "candidates.json"), encoding="utf-8"))
    choices = json.load(io.open(os.path.join(HERE, "standin_choices.json"), encoding="utf-8"))
    os.makedirs(os.path.join(OUT, "large"), exist_ok=True)
    out = os.path.join(ROOT, "Art", "standin", "port_view")
    os.makedirs(out, exist_ok=True)
    lines = []
    for port, c in choices.items():
        if not c or not c.get("n"):
            continue
        r = next((x for x in found.get(port, []) if x["n"] == c["n"]), None)
        if r is None:
            print("no such picture:", port, c)
            continue
        d = get({"action": "query", "titles": r["title"], "prop": "imageinfo", "iiprop": "url", "iiurlwidth": 1600, "format": "json"})
        page = next(iter((d.get("query", {}).get("pages", {}) or {}).values()), {})
        ii = (page.get("imageinfo") or [{}])[0]
        large = os.path.join(OUT, "large", port + os.path.splitext(r["url"])[1].lower())
        if not fetch(ii.get("thumburl") or r["url"], large):
            continue
        cut = tuple(c.get("cut", (0.5, 0.5)))
        standin_filter.filtered(large, (640, 360), c.get("lines", 270), cut).save(os.path.join(out, "portview.%s.png" % port))
        lines.append("| portview.%s | %s | %s | %s | %s |" % (port, r["title"].replace("File:", ""), r["author"] or r["credit"] or "(not given)", r["licence"], r["page"]))
        print(port, r["title"])
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
