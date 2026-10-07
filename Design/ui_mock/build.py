# -*- coding: utf-8 -*-
# Builds the UI mockup page: crops the real overview map, places the ports by their real coordinates, fills the template.
import base64, io, json, os
from osgeo import osr
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
OVER = r"G:\kapman_unity\Kapman2\MapArt\world\overview.png"
WEST, NORTH, MPP = -1105920.0, 1536000.0, 2949120.0 / 4096.0
X0, Z1, WKM, HKM = -690.0, -95.0, 1152.0, 648.0          # the crop: west, north (km), width, height

wgs = osr.SpatialReference(); wgs.ImportFromEPSG(4326); wgs.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
wf = osr.SpatialReference(); wf.ImportFromProj4("+proj=lcc +lat_0=56 +lon_0=5 +lat_1=45 +lat_2=65 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs")
ct = osr.CoordinateTransformation(wgs, wf)


def pos(lat, lon):
    x, z, _ = ct.TransformPoint(lon, lat)
    return {"x": round((x / 1000.0 - X0) / WKM * 100.0, 2), "y": round((Z1 - z / 1000.0) / HKM * 100.0, 2)}


im = Image.open(OVER).convert("RGB")
px0 = int(round((X0 * 1000.0 - WEST) / MPP)); py0 = int(round((NORTH - Z1 * 1000.0) / MPP))
crop = im.crop((px0, py0, px0 + 1600, py0 + 900)).quantize(colors=48, dither=Image.NONE)
buf = io.BytesIO(); crop.save(buf, format="PNG", optimize=True)
uri = "data:image/png;base64," + base64.b64encode(buf.getvalue()).decode("ascii")
print("map crop %d KB" % (len(buf.getvalue()) // 1024))

P = [
    # id, name, modern, kind, realm, faith, standing, big, lat, lon, km, days, market, church, word class, word, trade
    ("dorestad", "Dorestat", "Wijk bij Duurstede", "emporium", "Franks", "Christian", "trusted", True, 51.972, 5.343, 0, "here",
     "Wine, querns, pots and glass from up the Rhine. Short of grain till harvest.", "The church stands above the dam heads. The priest has letters from York.",
     "seen", "You are known here. The count's man counts your casks himself.",
     {"Wine": ["fair", "asks 49 d"], "Wool": ["good", "pays well"], "Salt": ["fair", "fair"], "Grain": ["good", "pays well"]}),
    ("traiectum", "Traiectum", "Utrecht", "church town", "Franks", "Christian", "known", False, 52.09, 5.12, 30, "half a day",
     "Small: iron, wax for the minsters, fish in Lent.", "St Martin's and St Salvator's. The mission's school is here.",
     "seen", "You saw bar iron fetch a good price here. A week back.",
     {"Wine": ["fair", "fair"], "Wool": ["none", "no word"], "Salt": ["fair", "fair"], "Grain": ["fair", "fair"]}),
    ("witla", "Witla", "at the Maas mouth", "market", "Franks", "Christian", "known", False, 51.92, 4.25, 85, "a day",
     "Salt and fish. The last water before the open sea.", "A wooden chapel on the dune.",
     "heard", "A pilot said the bar is bad on the ebb. Yesterday.",
     {"Wine": ["fair", "fair"], "Wool": ["none", "no word"], "Salt": ["bad", "cheap here"], "Grain": ["fair", "fair"]}),
    ("walichrum", "Walichrum", "Domburg, Walcheren", "beach market", "Franks", "Christian", "a stranger", False, 51.57, 3.50, 140, "1 to 2 days",
     "A beach market: salt, cloth, what the Frisians bring.", "Old altars to Nehalennia lie in the dunes.",
     "none", "No word of this place since spring.",
     {"Wine": ["none", "no word"], "Wool": ["fair", "fair"], "Salt": ["bad", "cheap here"], "Grain": ["none", "no word"]}),
    ("lundenwic", "Lundenwic", "London, the Strand", "emporium", "Mercia", "Christian", "a stranger", True, 51.51, -0.12, 430, "2 to 4 days",
     "Wool, cloth, lead and tin. Wants wine, querns and glass.", "St Paul's inside the old walls; the bishop's men buy wine.",
     "heard", "A Frisian skipper said Lundenwic was paying well for wine. Last week.",
     {"Wine": ["good", "pays well"], "Wool": ["bad", "cheap here"], "Salt": ["fair", "fair"], "Grain": ["none", "no word"]}),
    ("gipeswic", "Gipeswic", "Ipswich", "emporium", "East Angles", "Christian", "known", False, 52.06, 1.16, 330, "2 to 3 days",
     "Pots by the thousand. Wants querns and wine.", "A church by the quay.",
     "seen", "You saw querns fetch 14 d here. In May.",
     {"Wine": ["good", "paid 80 d in May"], "Wool": ["fair", "fair"], "Salt": ["none", "no word"], "Grain": ["fair", "fair"]}),
    ("quentovic", "Quentovic", "on the Canche", "emporium", "Franks", "Christian", "a stranger", True, 50.48, 1.65, 370, "2 to 4 days",
     "The king's tenth on all that is sold. Wine from the Seine, English cloth.", "The abbot of Saint-Wandrille's men keep the toll.",
     "heard", "They say the toll-keeper turns English ships away again. Old talk.",
     {"Wine": ["bad", "cheap here"], "Wool": ["good", "pays well"], "Salt": ["fair", "fair"], "Grain": ["bad", "cheap here"]}),
    ("colonia", "Colonia", "Cologne", "city", "Franks", "Christian", "known", True, 50.94, 6.96, 260, "8 to 12 days upstream",
     "Wine at its source, Badorf pots, glass, querns from Mayen.", "The archbishop is the king's own chaplain.",
     "seen", "The vintage is not in. Wine is dear here till Martinmas.",
     {"Wine": ["bad", "dear to buy"], "Wool": ["good", "pays well"], "Salt": ["good", "pays well"], "Grain": ["fair", "fair"]}),
    ("eoforwic", "Eoforwic", "York", "city", "Northumbria", "Christian", "a stranger", True, 53.96, -1.08, 640, "4 to 6 days",
     "Hungry this year. Wants grain above all.", "The minster's library is the best in the north. The Frisians left twenty years ago.",
     "heard", "A priest from York said there is famine in Northumbria. Last month.",
     {"Wine": ["fair", "fair"], "Wool": ["fair", "fair"], "Salt": ["none", "no word"], "Grain": ["good", "famine: pays dear"]}),
    ("sliesthorp", "Sliesthorp", "Hedeby", "market", "Danes", "the old gods", "a stranger", True, 54.49, 9.57, 560, "5 to 7 days",
     "Furs, amber, hides, slaves. Silver by weight, not by coin.", "No church. An oath here is sworn on a ring.",
     "none", "No word of this place since spring.",
     {"Wine": ["none", "no word"], "Wool": ["none", "no word"], "Salt": ["none", "no word"], "Grain": ["none", "no word"]}),
    ("hamwic", "Hamwic", "Southampton", "emporium", "Wessex", "Christian", "a stranger", False, 50.90, -1.40, 600, "4 to 6 days",
     "Wool and hides. Wants wine and pots.", "St Mary's by the shore.",
     "none", "No word of this place since spring.",
     {"Wine": ["none", "no word"], "Wool": ["none", "no word"], "Salt": ["none", "no word"], "Grain": ["none", "no word"]}),
    ("medemelacha", "Medemelacha", "Medemblik", "landing", "Frisians", "Christian", "known", False, 52.77, 5.10, 110, "1 to 2 days",
     "Frisian cloth, cattle in autumn. Wants timber and grain.", "A chapel on the terp.",
     "heard", "A drover said the cloth is ready. Two days back.",
     {"Wine": ["good", "pays well"], "Wool": ["bad", "cheap here"], "Salt": ["fair", "fair"], "Grain": ["good", "pays well"]}),
]
ports = []
for (pid, name, modern, kind, realm, faith, standing, big, lat, lon, km, days, market, church, w, word, trade) in P:
    d = pos(lat, lon)
    d.update(lft=pid in ("traiectum", "witla", "walichrum"), id=pid, name=name, modern=modern, kind=kind, realm=realm, faith=faith, standing=standing, big=big, km=km, days=days,
             market=market, church=church, w=w, word=word, trade=trade)
    ports.append(d)
    print("%-12s %5.1f %5.1f" % (pid, d["x"], d["y"]))

def path(pts):
    return [[pos(a, b)["x"], pos(a, b)["y"]] for a, b in pts]

data = {
    "ports": ports,
    "course": path([(51.972, 5.343), (51.95, 4.9), (51.93, 4.5), (51.95, 4.05), (51.85, 3.2), (51.6, 2.0), (51.48, 1.0), (51.47, 0.45), (51.51, -0.12)]),
    "atSea": pos(51.78, 2.9),
    "lanes": [path([(51.95, 4.05), (52.4, 3.9), (53.2, 4.6), (53.6, 6.0), (53.9, 7.8), (54.3, 8.4)]),
              path([(51.85, 3.2), (51.2, 2.4), (50.9, 1.6), (50.48, 1.65)]),
              path([(51.6, 2.0), (51.95, 1.5), (52.06, 1.16)]),
              path([(51.6, 2.0), (52.6, 1.9), (53.6, 0.4), (53.75, -0.3), (53.96, -1.08)])],
    "winds": [dict(pos(a, b), a=ang) for a, b, ang in [(53.5, 2.5, 20), (52.6, 3.0, 15), (51.9, 2.6, 25), (51.2, 1.9, 30), (53.9, 5.0, 10), (54.4, 6.6, 8), (52.9, 1.0 + 1.4, 22), (50.4, 0.2, 35), (53.0, 0.9, 18)]],
    "tides": [dict(pos(51.97, 3.9), t="Rhine bar: cross on the flood, two hours on"), dict(pos(51.48, 0.9), t="Thames: the ebb runs till dusk"), dict(pos(53.0, 0.3), t="The Wash: flats dry at low water")],
    "news": [dict(pos(53.96, -1.6), t="Famine in Northumbria. 32 days old", k="Raids", bad=True),
             dict(pos(54.85, -0.9), t="Lindisfarne sacked by heathen men. 13 days old", k="Raids", bad=True),
             dict(pos(49.75, 1.9), t="Saint-Denis fair from 9 October", k="Fairs", bad=False),
             dict(pos(51.972, 5.9), t="St John's fires tomorrow night", k="Fairs", bad=False)],
    "notes": [dict(pos(51.99, 4.0), t="Your note: the bar; cross on the flood"), dict(pos(50.48, 1.9), t="Your note: the toll-keeper takes a cask kindly")],
    "sails": [pos(52.3, 3.4), pos(51.7, 2.1), pos(53.4, 4.9), pos(51.3, 1.6)],
    "men": [
        {"n": "Aldgrim", "home": "Wijnaldum", "faith": "baptised", "trait": "Careful with the steering oar", "want": "home for his sister's wedding at Lammas", "heart": 3},
        {"n": "Folkmar", "home": "Dorestat", "faith": "baptised", "trait": "Counts every penny twice", "want": "a share, not a wage", "heart": 2},
        {"n": "Ubbi", "home": "Ripa", "faith": "keeps to Thor", "trait": "Says less than he means", "want": "to see Sliesthorp again", "heart": 2},
        {"n": "Liafwin", "home": "Traiectum", "faith": "a deacon's son", "trait": "Knows the old songs by heart", "want": "a book of his own", "heart": 3},
    ],
    "goods": [
        {"n": "Rhenish wine", "c": "good", "t": "worth carrying west", "w": "heard", "word": "A Frisian skipper said Lundenwic was paying well for wine. Last week.", "here": "asks 49 d a cask"},
        {"n": "Wheat", "c": "good", "t": "wanted here", "w": "seen", "word": "You saw the granaries low this morning. The harvest is six weeks off.", "here": "pays 8 d a sack"},
        {"n": "Mayen querns", "c": "fair", "t": "steady", "w": "seen", "word": "You saw querns fetch 14 d at Gipeswic. In May.", "here": "asks 6 d"},
        {"n": "Frisian cloth", "c": "fair", "t": "steady", "w": "heard", "word": "A drover said the cloth is ready at Medemelacha. Two days back.", "here": "asks 30 d a bolt"},
        {"n": "Salt", "c": "bad", "t": "wait for Martinmas", "w": "none", "word": "The slaughter is months off. Nobody wants salt before it.", "here": "asks 3 d a measure"},
    ],
}
data["house"] = {
    "rhine": path([(51.972, 5.343), (51.97, 5.9), (51.86, 6.1), (51.66, 6.45), (51.43, 6.76), (50.94, 6.96)]),
    "north": path([(51.972, 5.343), (51.95, 4.9), (51.93, 4.5), (51.95, 4.05), (52.4, 3.9), (53.2, 4.6), (53.6, 6.0), (53.9, 7.8), (54.6, 8.1), (55.2, 8.2)]),
    "west": path([(51.972, 5.343), (51.95, 4.9), (51.93, 4.5), (51.95, 4.05), (51.85, 3.2), (51.2, 2.4), (50.9, 1.6), (50.48, 1.65)]),
    "aldgrim": pos(53.3, 4.5), "folkmarDue": pos(51.62, 2.1), "gerbrandDue": pos(51.15, 2.3), "thiadricDue": pos(51.75, 6.3),
}
tpl = io.open(os.path.join(HERE, "template2.html"), encoding="utf-8").read()
out = tpl.replace("__MAP__", uri).replace("__DATA__", json.dumps(data, ensure_ascii=False))
dst = os.path.join(HERE, "kapman_table.html")
io.open(dst, "w", encoding="utf-8").write(out)
print("wrote", dst, "%d KB" % (len(out.encode("utf-8")) // 1024))
