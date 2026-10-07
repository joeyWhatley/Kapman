# -*- coding: utf-8 -*-
r"""
MAKE GAZETTEER (the user, 4 Oct 2026: "Everything single play in the known trade routes and all settlements all ports
cities all locations everything" - read as: every single place on the known trade routes, and all settlements, ports,
cities, locations). The game has 50 ports (Assets/Kapman/Game/Data/ports_793.json). This asks Wikidata for the rest of
the world a merchant of 793-1066 moved through, and writes one list the game and the writers can read.

  run.bat gazetteer\make_gazetteer.py              ask what has not been asked yet, then merge, write, draw
  run.bat gazetteer\make_gazetteer.py --fresh      ask everything again
  run.bat gazetteer\make_gazetteer.py --fresh monastery,battle     ask these kinds again
  run.bat gazetteer\make_gazetteer.py --no-ask     only merge what is on disk (no network)
  run.bat gazetteer\make_gazetteer.py --no-domesday   leave out the English settlements found by their Domesday id

WHERE IT ASKS: Wikidata's SPARQL service (https://query.wikidata.org/sparql), one query at a time, with a User-Agent
that names the project. It waits and asks again on a 429 or a timeout; a query that stays too heavy is cut into four
boxes and each box asked on its own. Wikidata is CC0: free for commercial use. Nothing else is read.

WHAT IT ASKS, by kind (the window: longitude -11 to 33, latitude 43 to 69; alive some time between 750 and 1066):
  monastery    monasteries and abbeys founded before 1067
  cathedral    cathedrals founded before 1067; bishoprics founded before 1067 (at their own place, or at their seat)
  palace       the Pfalzen, royal vills and kings' farms (by class and by their words); palaces founded before 1067
  burh         Anglo-Saxon burhs (by class and by their words; the Burghal Hidage is in the list of names below)
  mint         mints (Wikidata has them without dates: kept when dated before 1067, or when on the list of early mints)
  emporium     emporia and Viking Age trading places (by their words; the known ones are in the list of names)
  fort         ring fortresses and other forts of the time (by class, by date, by their words)
  stronghold   Slavic strongholds (the class "gord", and by their words)
  irish        Irish monastic towns and longphorts (by their words and ids; the known ones are in the list of names)
  town         towns, cities and villages with an inception, or a first written record, before 1067
  roman_town   living places that carry an id of the ancient world (Pleiades, DARE, vici.org): Roman towns still lived in
  settlement   English places that carry an OpenDomesday id (the vills of 1066-86)
  site         anything tagged with the Viking Age, the Early Middle Ages, the Vendel era, the early Slavic period, or
               built in the Carolingian, Ottonian, Anglo-Saxon or pre-Romanesque style; and by their words
  thing        thing sites
  battle       battles and sieges of 793 to 1066
  ship_find    ship burials, boat graves, Viking ships and wrecks of the time
  stone        rune stones and picture stones: their own light layer ("stones" in the output, not "places")
  dated        everything else with an inception before 1067 (churches, castles, walls): kept when it is a place
  named        a list of well-known places, written below, each looked up in Wikidata by name and nearness. It gives the
               kind where Wikidata has no class for it (an emporium, a burh, a longphort), and brings in the famous
               places Wikidata has no date for. The place, its name and its coordinates still come from Wikidata.

WHAT IT WRITES
  G:\kapman_unity\data\gazetteer\raw_<kind>.json        the answers as they came (values only), with the query asked
  Assets/Kapman/Game/Data/gazetteer_793.json             the merged list, one place a line
  Design/content/gazetteer/README.md                     what was asked, the counts, the credit, what is thin
  Captures/map/gazetteer.png                             every place as a dot over the chart's overview

The curators' own places: Design/content/gazetteer/curated.json, if it is there, is merged last (see the README).
"""
import datetime, difflib, io, json, math, os, re, sys, time, unicodedata
import requests
from pyproj import CRS, Transformer

try:                                   # names in every tongue go to the console and the log
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
RAW = r"G:\kapman_unity\data\gazetteer"
PORTS = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data", "ports_793.json")
OUT = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data", "gazetteer_793.json")
DOCS = os.path.join(ROOT, "Design", "content", "gazetteer")
README = os.path.join(DOCS, "README.md")
CURATED = os.path.join(DOCS, "curated.json")
MAP_ART = os.path.join(ROOT, "MapArt", "world")
MAP_ART_SCRIPT = os.path.join(ROOT, "Bake", "world", "make_map_art.py")
PICTURE = os.path.join(ROOT, "Captures", "map", "gazetteer.png")

ENDPOINT = "https://query.wikidata.org/sparql"
AGENT = "KapmanGazetteer/1.0 (Kapman, a merchant-sailing game of the North Sea world 793-1066; Bake/gazetteer/make_gazetteer.py) python-requests"
ENTITY = "http://www.wikidata.org/entity/"
WEST, EAST, SOUTH, NORTH = -11.0, 33.0, 43.0, 69.0
YEAR_FROM, YEAR_TO = 750, 1066
PAUSE = 1.0                    # seconds between two queries
CREDIT = "Places, names, coordinates and dates: Wikidata (wikidata.org), CC0 1.0 Universal public domain dedication."


def log(*a):
    print(*a, flush=True)


# ------------------------------------------------------------------------------------------------ the world frame
def world_proj():
    """The game's world frame: read from the map art's script, so there is one truth."""
    fallback = "+proj=lcc +lat_0=56 +lon_0=5 +lat_1=45 +lat_2=65 +x_0=0 +y_0=0 +datum=WGS84 +units=m +no_defs"
    try:
        with io.open(MAP_ART_SCRIPT, "r", encoding="utf-8") as f:
            m = re.search(r'^WORLD_PROJ\s*=\s*"([^"]+)"', f.read(), re.M)
        return m.group(1) if m else fallback
    except IOError:
        return fallback


WORLD_PROJ = world_proj()
_to_world = Transformer.from_crs(CRS.from_epsg(4326), CRS.from_proj4(WORLD_PROJ), always_xy=True)


def to_world(lat, lon):
    x, z = _to_world.transform(lon, lat)
    return int(round(x)), int(round(z))


def km(lat1, lon1, lat2, lon2):
    p1, p2 = math.radians(lat1), math.radians(lat2)
    a = math.sin((p2 - p1) / 2) ** 2 + math.cos(p1) * math.cos(p2) * math.sin(math.radians(lon2 - lon1) / 2) ** 2
    return 12742.0 * math.asin(min(1.0, math.sqrt(a)))


def in_box(lat, lon):
    return SOUTH <= lat <= NORTH and WEST <= lon <= EAST


# ------------------------------------------------------------------------------------------------ asking
class Heavy(Exception):
    """The service ran out of time on this query."""


_session = requests.Session()
_last = [0.0]
_asked = [0]


def ask(query):
    """One query to the service. Waits and asks again on a 429, a timeout or a network error; a query that keeps
    timing out raises Heavy (the caller cuts its box)."""
    timeouts = 0
    for attempt in range(1, 8):
        wait = PAUSE - (time.time() - _last[0])
        if wait > 0:
            time.sleep(wait)
        _asked[0] += 1
        try:
            r = _session.post(ENDPOINT, data={"query": query},
                              headers={"User-Agent": AGENT, "Accept": "application/sparql-results+json"}, timeout=(30, 80))
        except requests.exceptions.Timeout:
            _last[0] = time.time()
            timeouts += 1
            if timeouts >= 2:
                raise Heavy()
            log("      timeout: waiting 20 s and asking again")
            time.sleep(20)
            continue
        except requests.exceptions.RequestException as e:
            _last[0] = time.time()
            log("      network: %s: waiting %d s" % (type(e).__name__, 15 * attempt))
            time.sleep(15 * attempt)
            continue
        _last[0] = time.time()
        if r.status_code == 200:
            try:
                return r.json()["results"]["bindings"]
            except ValueError:                       # the answer was cut off: the time ran out while it was sent
                timeouts += 1
                if timeouts >= 2:
                    raise Heavy()
                time.sleep(20)
                continue
        if r.status_code == 429:
            try:
                secs = float(r.headers.get("Retry-After", ""))
            except ValueError:
                secs = 30.0 * attempt
            log("      429 too many requests: waiting %.0f s" % (secs + 2))
            time.sleep(secs + 2)
            continue
        if r.status_code in (500, 502, 503, 504):
            if r.status_code == 504 or "timeout" in r.text.lower():
                timeouts += 1
                if timeouts >= 2:
                    raise Heavy()
                log("      timeout (%d): waiting 20 s and asking again" % r.status_code)
                time.sleep(20)
                continue
            log("      http %d: waiting %d s" % (r.status_code, 20 * attempt))
            time.sleep(20 * attempt)
            continue
        raise RuntimeError("http %d: %s" % (r.status_code, r.text[:400]))
    raise RuntimeError("gave up after 7 tries")


def simple(b):
    """One answer row, values only: an entity as its Q-number, a tagged text with its language beside it."""
    out = {}
    for k, v in b.items():
        val = v["value"]
        if v["type"] == "uri" and val.startswith(ENTITY):
            val = val[len(ENTITY):]
        out[k] = val
        if "xml:lang" in v:
            out[k + "@"] = v["xml:lang"]
    return out


def box_text(box):
    w, s, e, n = box
    return ("?item p:P625/psv:P625 ?cv . ?cv wikibase:geoLatitude ?lat ; wikibase:geoLongitude ?lon . "
            "FILTER(?lat >= %s && ?lat <= %s && ?lon >= %s && ?lon <= %s)" % (repr(s), repr(n), repr(w), repr(e)))


def ask_boxed(sparql, box=(WEST, SOUTH, EAST, NORTH), depth=0):
    """Ask a query for a box ({BOX} in the text). Too heavy: cut the box in four and ask each."""
    try:
        return [simple(b) for b in ask(sparql.replace("{BOX}", box_text(box)))], []
    except Heavy:
        if "{BOX}" not in sparql or depth >= 3:
            log("      too heavy, and no smaller box to try: left out")
            return [], [list(box)]
        w, s, e, n = box
        mx, my = (w + e) / 2.0, (s + n) / 2.0
        log("      too heavy: cutting the box %s in four" % (box,))
        rows, failed = [], []
        for part in ((w, s, mx, my), (mx, s, e, my), (w, my, mx, n), (mx, my, e, n)):
            r, f = ask_boxed(sparql, part, depth + 1)
            rows += r
            failed += f
        return rows, failed


# ------------------------------------------------------------------------------------------------ the queries
BEFORE = '"%04d-01-01T00:00:00Z"^^xsd:dateTime' % (YEAR_TO + 1)
BATTLE_FROM = '"0793-01-01T00:00:00Z"^^xsd:dateTime'


def wds(ids):
    return " ".join("wd:" + q for q in ids)


def of(roots):
    """An instance of one of these classes, or of a class under them. One branch a class: the service is quick on a path
    to one fixed class, and runs out of time on the same path to a VALUES list."""
    return " UNION ".join("{ ?item wdt:P31/wdt:P279* wd:%s }" % q for q in roots)


def q_class(roots):
    """Every instance of these classes, or of a class under them, in the box."""
    return "SELECT DISTINCT ?item ?lat ?lon WHERE { %s {BOX} }" % of(roots)


def q_dated(roots=None, prop="P571"):
    """Founded (P571) or first written of (P1249) before 1067, in the box. The date first, by its index."""
    tail = ""
    if roots:
        tail = "?item wdt:P31/wdt:P279* ?root . FILTER(?root IN (%s))" % ", ".join("wd:" + q for q in roots)
    return ('SELECT DISTINCT ?item ?lat ?lon ?from WHERE { hint:Query hint:optimizer "None" . '
            "?item wdt:%s ?from . hint:Prior hint:rangeSafe true . FILTER(?from < %s) {BOX} %s }" % (prop, BEFORE, tail))


def q_tagged(props, values):
    """Everything in the box that carries one of these values (a period, a culture, a style)."""
    return "SELECT DISTINCT ?item ?lat ?lon ?tag WHERE { VALUES ?tag { %s } ?item %s ?tag . {BOX} }" % (
        wds(values), "|".join("wdt:" + p for p in props))


def q_has(props, also=""):
    """Everything in the box that carries one of these properties (an id in some other list)."""
    return "SELECT DISTINCT ?item ?lat ?lon WHERE { ?item %s ?id . %s {BOX} }" % ("|".join("wdt:" + p for p in props), also)


def q_words(terms):
    """Everything in the box whose labels or descriptions hold these words (Wikidata's own search, through the service)."""
    parts = []
    for t in terms:
        esc = t.replace("\\", "\\\\").replace('"', '\\"')
        parts.append('{ SERVICE wikibase:mwapi { bd:serviceParam wikibase:endpoint "www.wikidata.org" ; wikibase:api "Search" ; '
                     'mwapi:srsearch "%s haswbstatement:P625" ; mwapi:srlimit "max" . ?item wikibase:apiOutputItem mwapi:title . } '
                     'BIND("%s" AS ?words) }' % (esc, esc))
    # no box here: the search with the box's value nodes ran out of time; the window is cut when the rows are read
    return "SELECT DISTINCT ?words ?item ?coord WHERE { %s ?item wdt:P625 ?coord }" % " UNION ".join(parts)


Q_SEE_AT_SEAT = ("SELECT DISTINCT ?item ?from ?loc ?loccoord WHERE { " + of(["Q665487", "Q3146899"]) + " "
                 "?item wdt:P571 ?from . FILTER(isLiteral(?from) && ?from < %s) FILTER NOT EXISTS { ?item wdt:P625 ?own } "
                 "?item wdt:P159|wdt:P36 ?loc . ?loc wdt:P625 ?loccoord }" % BEFORE)
Q_BATTLE = ("SELECT DISTINCT ?item ?date ?coord ?loc ?loccoord WHERE { " + of(["Q178561", "Q188055"]) + " "
            "?item wdt:P585|wdt:P580 ?date . FILTER(isLiteral(?date) && ?date >= %s && ?date < %s) "
            "OPTIONAL { ?item wdt:P625 ?coord } OPTIONAL { ?item wdt:P276 ?loc . ?loc wdt:P625 ?loccoord } }" % (BATTLE_FROM, BEFORE))
Q_SHIP = ("SELECT DISTINCT ?item ?coord ?loc ?loccoord ?from WHERE { " + of(["Q1145397", "Q124473037", "Q211969", "Q897611"]) +
          " OPTIONAL { ?item wdt:P625 ?coord } "
          "OPTIONAL { ?item wdt:P189|wdt:P276 ?loc . ?loc wdt:P625 ?loccoord } OPTIONAL { ?item wdt:P571 ?from } }")
Q_WRECK = ('SELECT DISTINCT ?item ?coord ?from WHERE { ?item wdt:P31/wdt:P279* wd:Q852190 ; wdt:P625 ?coord ; wdt:P571|wdt:P729|wdt:P746 ?from . '
           'FILTER(isLiteral(?from) && ?from >= "0700-01-01T00:00:00Z"^^xsd:dateTime && ?from < %s) }' % BEFORE)
Q_THING = ("SELECT DISTINCT ?item ?coord ?loc ?loccoord WHERE { ?item wdt:P31/wdt:P279* wd:Q387834 . OPTIONAL { ?item wdt:P625 ?coord } "
           "OPTIONAL { ?item wdt:P276|wdt:P159 ?loc . ?loc wdt:P625 ?loccoord } }")
Q_RING_PART = "SELECT DISTINCT ?item ?lat ?lon WHERE { VALUES ?whole { wd:Q64518271 wd:Q123555321 } ?item wdt:P361|wdt:P1435|wdt:P31 ?whole . {BOX} }"

# the classes
SETTLEMENT = ["Q486972", "Q15284", "Q515", "Q3957", "Q532", "Q15661340", "Q5770868"]
# periods, cultures and styles that mean "of our time" (P2348, P2596, P149)
PERIODS = ["Q213649", "Q202763", "Q1275225", "Q98271051", "Q8086", "Q8083", "Q8052", "Q7063", "Q12567", "Q32768"]
STYLES = ["Q708807", "Q9159129", "Q537454", "Q2337737", "Q1588393"]

WORDS = {   # by their words: each line is one search (all its words must be there; "..." is a phrase)
    "palace": ['Königspfalz', 'Kaiserpfalz', 'Karolingerpfalz', '"karolingische Pfalz"', '"ottonische Pfalz"', 'Pfalz karolingisch',
               'Pfalz ottonisch', '"royal vill"', '"villa regia"', '"Anglo-Saxon palace"', '"Carolingian palace"', '"palais carolingien"',
               '"Anglo-Saxon royal"', 'kongsgård vikingtid', '"royal estate" "Anglo-Saxon"', '"royal estate" Carolingian',
               'Königshof karolingisch', 'Königshof fränkisch', '"fränkischer Königshof"', '"karolingischer Königshof"',
               '"royal seat" Viking', 'kungsgård vikingatid'],
    "burh": ['"Anglo-Saxon burh"', '"Burghal Hidage"', '"Saxon burh"', 'burh "Anglo-Saxon"'],
    "emporium": ['"trading place" Viking', '"trading settlement" Viking', '"trading centre" Viking', '"trading center" Viking',
                 '"trading post" Viking', '"market place" Viking', '"trading town" Viking', 'emporium "early medieval"',
                 'emporium Carolingian', 'emporium "Anglo-Saxon"', 'emporium Viking', 'emporium Frisian', 'Handelsplatz wikingerzeitlich',
                 'Handelsplatz wikingerzeitlicher', 'Handelsplatz frühmittelalterlich', 'Handelsplatz frühmittelalterlicher',
                 'Seehandelsplatz', 'Handelsplatz slawisch', 'Handelsplatz slawischer', 'handelsplads vikingetid',
                 'handelsplads vikingetiden', 'handelsplats vikingatid', 'handelsplats vikingatida', 'handelsplass vikingtid',
                 'handelsplass vikingtiden', 'kaupang vikingtid', 'wic "Anglo-Saxon"'],
    "fort": ['"ring fortress"', 'ringborg', 'trelleborg vikingetid', 'ringwalburg', '"Viking fortress"', '"Viking fort"', '"Viking camp"',
             '"winter camp" Viking', 'Ungarnrefugium', 'Ungarnwall', '"karolingische Burg"', '"ottonische Burg"',
             '"frühmittelalterliche Burg"', '"frühmittelalterliche Befestigung"', '"frühmittelalterlicher Ringwall"',
             '"frühmittelalterliche Wallanlage"', '"frühmittelalterlicher Burgstall"', '"sächsische Wallburg"', '"château carolingien"',
             '"enceinte carolingienne"', '"Anglo-Saxon fort"', '"Pictish fort"', 'fornborg vikingatid', 'bygdeborg vikingtid'],
    "stronghold": ['"Slawischer Burgwall"', 'Slawenburg', 'Burgwall slawisch', 'Burgwall slawischer', 'Burgwall slawische',
                   '"slawische Burg"', '"slawische Burganlage"', '"slawischer Ringwall"', '"slawische Wallanlage"', '"Slavic stronghold"',
                   '"Slavic fort"', '"Slavic hillfort"', '"Slavic fortress"', '"Slavic gord"', '"Slavic settlement"', '"Slavic castle"',
                   '"Slavic burgwall"', 'gord Slavic', 'grodzisko wczesnośredniowieczne', 'gród wczesnośredniowieczny',
                   '"slovanské hradiště"', '"slovanské hradisko"', 'hradiště slovanské', '"древнерусское городище"',
                   '"Great Moravian"', 'velkomoravské hradiště'],
    "irish": ['longphort Viking', '"Viking longphort"', '"monastic site" Ireland', '"monastic settlement" Ireland',
              '"early Christian" monastery Ireland', '"early Christian" "monastic site"', '"Irish round tower"', '"round tower" Ireland',
              '"early monastic" Ireland', '"Celtic monastery"', '"early Christian" monastery Scotland', '"early Christian" monastery Wales'],
    "site": ['"Viking Age"', '"Viking era"', '"Viking period"', '"Viking settlement"', '"Viking burial"', '"Viking grave"',
             '"Viking cemetery"', '"Viking harbour"', '"Viking harbor"', '"Norse settlement"', '"Norse farmstead"', 'vikingatid',
             'vikingatida', 'vikingatiden', 'vikingetid', 'vikingetiden', 'vikingtid', 'vikingtiden', 'vikingtida', 'Wikingerzeit',
             'wikingerzeitlich', 'wikingerzeitliche', 'wikingerzeitlicher', 'wikingerzeitliches', '"Anglo-Saxon"', 'angelsächsisch',
             'angelsächsische', 'Carolingian', 'karolingisch', 'karolingische', 'karolingischer', 'karolingisches', 'carolingien',
             'carolingienne', 'Karolingisch', 'Ottonian', 'ottonisch', 'ottonische', '"early medieval"', '"Early Middle Ages"',
             'frühmittelalterlich', 'frühmittelalterliche', 'frühmittelalterlicher', 'frühmittelalterliches', 'Frühmittelalter',
             '"haut Moyen Âge"', 'vroegmiddeleeuws', 'vroegmiddeleeuwse', 'vendeltid', 'vendeltida', '"Vendel Period"', '"Vendel era"',
             'merovingertid', '"early Slavic"', 'frühslawisch', 'frühslawische', 'altslawisch', 'altslawische',
             'wczesnośredniowieczne', 'wczesnośredniowieczny', 'wczesnośredniowieczna', '"raně středověké"', '"yngre jernalder"',
             '"yngre järnålder"', 'Pictish', '"Hiberno-Norse"', '"Anglo-Scandinavian"', '"Anglo-Saxon cemetery"', '"Anglo-Saxon church"'],
    "thing": ['"thing site"', 'thingstead', 'tingsted vikingetid', 'tingsplats', 'tingplats vikingatid', 'tingstad', 'tinghaug',
              'tingshög', 'tingplass vikingtid', '"assembly site"', '"assembly mound"', '"moot hill"', '"Viking parliament"',
              '"Norse assembly"', '"Norse parliament"'],
    "ship_find": ['"Viking ship"', '"ship burial"', '"boat burial"', '"boat grave"', '"ship grave"', 'båtgrav', 'båtgravar', 'båtgravfält',
                  'skibsgrav', 'skipsgrav', 'skipsgraven', 'Schiffsgrab', 'Bootsgrab', 'Bootsgräber', 'vikingaskepp', 'vikingeskib',
                  'vikingskip', 'Wikingerschiff'],
}


def word_queries(kind, how):
    out, terms = [], WORDS[kind]
    for t in terms:                      # one search a query: six at once ran out of time
        out.append({"kind": kind, "name": "by their words: " + t, "sparql": q_words([t]), "gives": kind, "soft": True, "how": how})
    return out


def asks(domesday=True):
    """Every query, in the order asked. gives: the kind a hit is (None: decided by what the thing is); soft: found by
    words only, so checked harder; how: the words for the place's source line."""
    A = []

    def add(kind, name, sparql, gives, how, soft=False):
        A.append({"kind": kind, "name": name, "sparql": sparql, "gives": gives, "soft": soft, "how": how})
    add("monastery", "monasteries and abbeys founded before 1067", q_dated(["Q44613"]), "monastery", "class and date")
    add("cathedral", "cathedrals founded before 1067", q_dated(["Q2977"]), "cathedral", "class and date")
    add("cathedral", "bishoprics founded before 1067", q_dated(["Q665487", "Q3146899"]), "bishopric", "class and date")
    add("cathedral", "bishoprics founded before 1067, placed at their seat", Q_SEE_AT_SEAT, "bishopric", "class and date, at its seat")
    add("palace", "pfalzen, royal vills and kings' farms", q_class(["Q1796442", "Q17296484", "Q1788752"]), "palace", "class")
    add("palace", "palaces founded before 1067", q_dated(["Q16560", "Q53536964"]), "palace", "class and date")
    A.extend(word_queries("palace", "its words"))
    add("burh", "burhs", q_class(["Q88034"]), "burh", "class")
    A.extend(word_queries("burh", "its words"))
    add("mint", "mints", q_class(["Q464780"]), "mint", "class")
    A.extend(word_queries("emporium", "its words"))
    add("fort", "Viking ring fortresses", q_class(["Q586552"]), "ring_fort", "class")
    add("fort", "parts of the Viking Age ring fortresses", Q_RING_PART, "ring_fort", "class")
    add("fort", "circular ramparts", q_class(["Q1363360"]), "fort", "class")
    add("fort", "forts and castles founded before 1067", q_dated(["Q57821", "Q23413", "Q744099"]), "fort", "class and date")
    A.extend(word_queries("fort", "its words"))
    add("stronghold", "gords", q_class(["Q88598"]), "stronghold", "class")
    A.extend(word_queries("stronghold", "its words"))
    add("irish", "longphorts", q_class(["Q3259145"]), "longphort", "class")
    add("irish", "Monasticon Hibernicum ids", q_has(["P12272"]), "monastery", "Monasticon Hibernicum id")
    A.extend(word_queries("irish", "its words"))
    add("town", "towns, cities and villages with an inception before 1067", q_dated(SETTLEMENT), "town", "class and date")
    add("town", "places first written of before 1067", q_dated(None, "P1249"), None, "first written record")
    add("town", "ancient and medieval cities by class", q_class(["Q5770868"]), None, "class")
    add("roman_town", "living places with an id of the ancient world", q_has(["P1584", "P1936", "P1481"], "?item wdt:P1082 ?people ."), "roman_town", "lived in, with an id of the ancient world")
    if domesday:
        add("settlement", "places with an OpenDomesday settlement id", q_has(["P3118"]), "settlement", "OpenDomesday id")
    add("site", "tagged with a period or a culture of the time", q_tagged(["P2348", "P2596"], PERIODS), None, "period tag")
    add("site", "built in a style of the time", q_tagged(["P149"], STYLES), None, "style tag")
    A.extend(word_queries("site", "its words"))
    add("thing", "things", Q_THING, "thing", "class")
    A.extend(word_queries("thing", "its words"))
    add("battle", "battles and sieges of 793 to 1066", Q_BATTLE, "battle", "class and date")
    add("ship_find", "ship burials, boat graves and Viking ships", Q_SHIP, "ship_find", "class")
    add("ship_find", "wrecks of 700 to 1066", Q_WRECK, "ship_find", "class and date")
    A.extend(word_queries("ship_find", "its words"))
    add("stone", "rune stones and picture stones", q_class(["Q815241", "Q602202"]), "stone", "class")
    add("stone", "inscriptions with a Rundata id", q_has(["P1261"]), "stone", "Rundata id")
    add("dated", "everything with an inception before 1067", q_dated(None), None, "date")
    return A


# ------------------------------------------------------------------------------------------------ the list of names
# kind ; name (or names, | between) ; latitude ; longitude  - about where it lies: the place itself comes from Wikidata,
# the nearest thing of that name within 30 km. A name Wikidata does not have is left out and listed in the README.
NAMED = u"""
# ---- emporia and trading places
emporium ; Dorestad ; 51.97 ; 5.34
emporium ; Quentovic ; 50.48 ; 1.72
emporium ; Hamwic|Southampton ; 50.90 ; -1.40
emporium ; Lundenwic|London ; 51.51 ; -0.12
emporium ; Ipswich ; 52.06 ; 1.16
emporium ; York ; 53.96 ; -1.08
emporium ; Hedeby|Haithabu ; 54.49 ; 9.57
emporium ; Ribe ; 55.33 ; 8.76
emporium ; Birka ; 59.34 ; 17.55
emporium ; Kaupang ; 59.03 ; 10.11
emporium ; Truso ; 54.15 ; 19.40
emporium ; Wolin ; 53.84 ; 14.62
emporium ; Reric ; 53.95 ; 11.47
emporium ; Groß Strömkendorf ; 53.96 ; 11.48
emporium ; Staraya Ladoga ; 60.00 ; 32.30
emporium ; Helgö ; 59.28 ; 17.68
emporium ; Åhus ; 55.92 ; 14.30
emporium ; Uppåkra ; 55.67 ; 13.17
emporium ; Sebbersund ; 56.96 ; 9.56
emporium ; Löddeköpinge ; 55.76 ; 13.01
emporium ; Menzlin ; 53.89 ; 13.63
emporium ; Ralswiek ; 54.47 ; 13.45
emporium ; Bardowick ; 53.29 ; 10.39
emporium ; Domburg ; 51.56 ; 3.50
emporium ; Medemblik ; 52.77 ; 5.11
emporium ; Tiel ; 51.89 ; 5.43
emporium ; Deventer ; 52.25 ; 6.16
emporium ; Stavoren ; 52.88 ; 5.36
emporium ; Emden ; 53.37 ; 7.21
emporium ; Fordwich ; 51.30 ; 1.13
emporium ; Sarre ; 51.34 ; 1.24
emporium ; Sandwich ; 51.27 ; 1.34
emporium ; Dover ; 51.13 ; 1.31
emporium ; Gnezdovo|Gnyozdovo ; 54.78 ; 31.88
emporium ; Rurikovo Gorodische|Rurikovo Gorodishche|Rurik's Hillfort ; 58.49 ; 31.30
emporium ; Veliky Novgorod|Novgorod ; 58.52 ; 31.27
emporium ; Fröjel ; 57.33 ; 18.19
emporium ; Köpingsvik ; 56.88 ; 16.71
emporium ; Sigtuna ; 59.62 ; 17.72
emporium ; Västergarn ; 57.44 ; 18.15
emporium ; Visby ; 57.64 ; 18.30
emporium ; Grobiņa ; 56.54 ; 21.17
emporium ; Wiskiauten ; 54.93 ; 20.46
emporium ; Apuolė ; 56.25 ; 21.68
emporium ; Oldenburg in Holstein|Starigard ; 54.29 ; 10.89
emporium ; Alt Lübeck|Liubice ; 53.91 ; 10.71
emporium ; Hollingstedt ; 54.46 ; 9.34
emporium ; Schleswig ; 54.52 ; 9.57
emporium ; Stade ; 53.60 ; 9.47
emporium ; Hamburg ; 53.55 ; 10.00
emporium ; Bremen ; 53.08 ; 8.81
emporium ; Meols ; 53.40 ; -3.16
emporium ; Whithorn ; 54.73 ; -4.42
emporium ; Aarhus ; 56.16 ; 10.21
emporium ; Kołobrzeg ; 54.18 ; 15.58
emporium ; Szczecin ; 53.43 ; 14.55
emporium ; Gdańsk ; 54.35 ; 18.65
emporium ; Kamień Pomorski ; 53.97 ; 14.77
emporium ; Usedom ; 53.87 ; 13.92
# ---- towns on the routes
town ; Utrecht ; 52.09 ; 5.12
town ; Nijmegen ; 51.84 ; 5.86
town ; Dokkum ; 53.33 ; 6.00
town ; Wijnaldum ; 53.20 ; 5.47
town ; Wijk bij Duurstede ; 51.97 ; 5.34
town ; Antwerp ; 51.22 ; 4.40
town ; Ghent ; 51.05 ; 3.72
town ; Bruges ; 51.21 ; 3.22
town ; Tournai ; 50.61 ; 3.39
town ; Maastricht ; 50.85 ; 5.69
town ; Huy ; 50.52 ; 5.24
town ; Dinant ; 50.26 ; 4.91
town ; Namur ; 50.47 ; 4.87
town ; Liège ; 50.64 ; 5.57
town ; Verdun ; 49.16 ; 5.38
town ; Metz ; 49.12 ; 6.18
town ; Reims ; 49.26 ; 4.03
town ; Arras ; 50.29 ; 2.78
town ; Cambrai ; 50.18 ; 3.24
town ; Saint-Omer ; 50.75 ; 2.25
town ; Boulogne-sur-Mer ; 50.73 ; 1.61
town ; Rouen ; 49.44 ; 1.10
town ; Amiens ; 49.89 ; 2.30
town ; Paris ; 48.86 ; 2.35
town ; Saint-Denis ; 48.94 ; 2.36
town ; Bayeux ; 49.28 ; -0.70
town ; Chartres ; 48.45 ; 1.49
town ; Nantes ; 47.22 ; -1.55
town ; Angers ; 47.47 ; -0.55
town ; Tours ; 47.39 ; 0.69
town ; Orléans ; 47.90 ; 1.91
town ; Poitiers ; 46.58 ; 0.34
town ; Saintes ; 45.75 ; -0.63
town ; Bordeaux ; 44.84 ; -0.58
town ; Noirmoutier-en-l'Île|Noirmoutier|Île de Noirmoutier ; 47.00 ; -2.25
town ; Rennes ; 48.11 ; -1.68
town ; Vannes ; 47.66 ; -2.76
town ; Cologne ; 50.94 ; 6.96
town ; Mainz ; 50.00 ; 8.27
town ; Duisburg ; 51.43 ; 6.76
town ; Xanten ; 51.66 ; 6.45
town ; Worms ; 49.63 ; 8.36
town ; Speyer ; 49.32 ; 8.43
town ; Strasbourg ; 48.58 ; 7.75
town ; Trier ; 49.76 ; 6.64
town ; Würzburg ; 49.79 ; 9.93
town ; Magdeburg ; 52.13 ; 11.63
town ; Erfurt ; 50.98 ; 11.03
town ; Hallstadt ; 49.93 ; 10.88
town ; Forchheim ; 49.72 ; 11.06
town ; Regensburg ; 49.02 ; 12.10
town ; Enns|Lorch ; 48.21 ; 14.48
town ; Münster ; 51.96 ; 7.63
town ; Osnabrück ; 52.27 ; 8.05
town ; Minden ; 52.29 ; 8.92
town ; Hildesheim ; 52.15 ; 9.95
town ; Halberstadt ; 51.90 ; 11.05
town ; Verden|Verden an der Aller ; 52.92 ; 9.23
town ; Soest ; 51.57 ; 8.11
town ; Bamberg ; 49.89 ; 10.89
town ; Norwich ; 52.63 ; 1.30
town ; Thetford ; 52.41 ; 0.75
town ; Lincoln ; 53.23 ; -0.54
town ; Stamford ; 52.65 ; -0.48
town ; Nottingham ; 52.95 ; -1.15
town ; Derby ; 52.92 ; -1.48
town ; Leicester ; 52.63 ; -1.13
town ; Chester ; 53.19 ; -2.89
town ; Bristol ; 51.45 ; -2.59
town ; Canterbury ; 51.28 ; 1.08
town ; Rochester ; 51.39 ; 0.50
town ; Dunwich ; 52.28 ; 1.63
town ; Whitby ; 54.49 ; -0.61
town ; Jarrow ; 54.98 ; -1.47
town ; Durham ; 54.78 ; -1.58
town ; Ripon ; 54.14 ; -1.52
town ; Beverley ; 53.84 ; -0.43
town ; Peterborough ; 52.57 ; -0.24
town ; Ely ; 52.40 ; 0.26
town ; Bury St Edmunds ; 52.25 ; 0.71
town ; St Albans ; 51.75 ; -0.34
town ; Glastonbury ; 51.15 ; -2.71
town ; Dorchester ; 50.71 ; -2.44
town ; Guildford ; 51.24 ; -0.57
town ; New Romney ; 50.99 ; 0.94
town ; Hythe ; 51.07 ; 1.08
town ; Carlisle ; 54.89 ; -2.93
town ; St Andrews ; 56.34 ; -2.80
town ; Kirkwall ; 58.98 ; -2.96
town ; Tórshavn ; 62.01 ; -6.77
town ; Kirkjubøur ; 61.95 ; -6.79
town ; Aalborg ; 57.05 ; 9.92
town ; Viborg ; 56.45 ; 9.40
town ; Odense ; 55.40 ; 10.39
town ; Roskilde ; 55.64 ; 12.08
town ; Lund ; 55.70 ; 13.19
town ; Ringsted ; 55.44 ; 11.79
town ; Trondheim ; 63.43 ; 10.40
town ; Oslo ; 59.91 ; 10.75
town ; Tønsberg ; 59.27 ; 10.41
town ; Sarpsborg ; 59.28 ; 11.11
town ; Skara ; 58.39 ; 13.44
town ; Lödöse ; 58.03 ; 12.15
town ; Kungahälla ; 57.87 ; 11.98
town ; Pskov ; 57.82 ; 28.33
town ; Izborsk ; 57.71 ; 27.86
town ; Polotsk ; 55.49 ; 28.80
town ; Smolensk ; 54.78 ; 32.05
town ; Vitebsk ; 55.19 ; 30.20
town ; Kyiv|Kiev ; 50.45 ; 30.52
town ; Chernihiv ; 51.49 ; 31.30
town ; Turov|Turaŭ ; 52.07 ; 27.74
town ; Tartu ; 58.38 ; 26.72
town ; Prague ; 50.09 ; 14.42
town ; Kraków ; 50.06 ; 19.94
town ; Gniezno ; 52.54 ; 17.60
town ; Poznań ; 52.41 ; 16.93
town ; Wrocław ; 51.11 ; 17.03
town ; Opole ; 50.67 ; 17.92
town ; Płock ; 52.55 ; 19.70
town ; Olomouc ; 49.59 ; 17.25
town ; Nitra ; 48.31 ; 18.09
# ---- royal palaces and estates
palace ; Palace of Aachen|Aachener Königspfalz ; 50.78 ; 6.08
palace ; Ingelheim Imperial Palace|Ingelheimer Kaiserpfalz|Kaiserpfalz Ingelheim ; 49.98 ; 8.07
palace ; Valkhof ; 51.85 ; 5.87
palace ; Pfalz Paderborn|Kaiserpfalz Paderborn ; 51.72 ; 8.76
palace ; Königspfalz Frankfurt ; 50.11 ; 8.68
palace ; Thionville ; 49.36 ; 6.17
palace ; Herstal ; 50.66 ; 5.63
palace ; Königspfalz Attigny|Attigny ; 49.48 ; 4.58
palace ; Compiègne ; 49.42 ; 2.83
palace ; Quierzy ; 49.57 ; 3.14
palace ; Ponthion ; 48.76 ; 4.71
palace ; Verberie ; 49.31 ; 2.73
palace ; Düren ; 50.80 ; 6.48
palace ; Pfalz Trebur|Trebur ; 49.92 ; 8.41
palace ; Royal palace of Werla|Königspfalz Werla|Werla ; 52.04 ; 10.55
palace ; Tilleda royal palace|Pfalz Tilleda ; 51.42 ; 11.14
palace ; Memleben ; 51.27 ; 11.50
palace ; Quedlinburg ; 51.79 ; 11.15
palace ; Pöhlde ; 51.62 ; 10.32
palace ; Pfalz Grona ; 51.55 ; 9.91
palace ; Allstedt ; 51.40 ; 11.39
palace ; Merseburg ; 51.36 ; 11.99
palace ; Dortmund ; 51.51 ; 7.47
palace ; Imperial Palace of Goslar|Kaiserpfalz Goslar ; 51.90 ; 10.43
palace ; Yeavering ; 55.57 ; -2.12
palace ; Cheddar Palace ; 51.27 ; -2.78
palace ; Rendlesham ; 52.13 ; 1.42
palace ; Kingston upon Thames ; 51.41 ; -0.30
palace ; Bamburgh ; 55.61 ; -1.71
palace ; Benson ; 51.62 ; -1.11
palace ; Chippenham ; 51.46 ; -2.12
palace ; Wantage ; 51.59 ; -1.43
palace ; Calne ; 51.44 ; -2.00
palace ; Amesbury ; 51.17 ; -1.78
palace ; Cookham ; 51.56 ; -0.71
palace ; Faversham ; 51.32 ; 0.89
palace ; Bosham ; 50.83 ; -0.86
palace ; Northampton ; 52.24 ; -0.90
palace ; Old Windsor ; 51.46 ; -0.58
palace ; Dunadd ; 56.09 ; -5.48
palace ; Dumbarton Castle|Dumbarton Rock ; 55.94 ; -4.56
palace ; Scone ; 56.42 ; -3.40
palace ; Forteviot ; 56.34 ; -3.53
palace ; Dunkeld ; 56.56 ; -3.59
palace ; Hill of Tara|Tara ; 53.58 ; -6.61
palace ; Rock of Cashel|Cashel ; 52.52 ; -7.89
palace ; Knowth ; 53.70 ; -6.49
palace ; Grianan of Aileach|Grianán of Aileach ; 55.02 ; -7.43
palace ; Aberffraw ; 53.19 ; -4.46
palace ; Lejre ; 55.60 ; 11.97
palace ; Jelling ; 55.76 ; 9.42
palace ; Gamla Uppsala|Old Uppsala ; 59.90 ; 17.63
palace ; Avaldsnes ; 59.36 ; 5.28
palace ; Hovgården ; 59.36 ; 17.53
palace ; Husaby ; 58.53 ; 13.38
palace ; Prague Castle ; 50.09 ; 14.40
palace ; Vyšehrad ; 50.06 ; 14.42
# ---- burhs (the Burghal Hidage, and the burhs of Mercia and of Edward and Aethelflaed)
burh ; Winchester ; 51.06 ; -1.31
burh ; Wallingford ; 51.60 ; -1.13
burh ; Wareham ; 50.69 ; -2.11
burh ; Hastings ; 50.86 ; 0.57
burh ; Lewes ; 50.87 ; 0.01
burh ; Burpham ; 50.87 ; -0.52
burh ; Chichester ; 50.84 ; -0.78
burh ; Portchester ; 50.84 ; -1.12
burh ; Wilton ; 51.08 ; -1.86
burh ; Chisbury ; 51.39 ; -1.60
burh ; Shaftesbury ; 51.01 ; -2.20
burh ; Christchurch ; 50.73 ; -1.78
burh ; Bridport ; 50.73 ; -2.76
burh ; Exeter ; 50.72 ; -3.53
burh ; Halwell ; 50.37 ; -3.72
burh ; Lydford ; 50.64 ; -4.11
burh ; Pilton ; 51.09 ; -4.06
burh ; Barnstaple ; 51.08 ; -4.06
burh ; Totnes ; 50.43 ; -3.69
burh ; Watchet ; 51.18 ; -3.33
burh ; Axbridge ; 51.29 ; -2.82
burh ; Lyng ; 51.06 ; -2.95
burh ; Langport ; 51.04 ; -2.83
burh ; Bath ; 51.38 ; -2.36
burh ; Malmesbury ; 51.58 ; -2.10
burh ; Cricklade ; 51.64 ; -1.86
burh ; Oxford ; 51.75 ; -1.26
burh ; Buckingham ; 52.00 ; -0.99
burh ; Eashing ; 51.19 ; -0.65
burh ; Southwark ; 51.50 ; -0.09
burh ; Worcester ; 52.19 ; -2.22
burh ; Warwick ; 52.28 ; -1.59
burh ; Tamworth ; 52.63 ; -1.69
burh ; Stafford ; 52.81 ; -2.12
burh ; Bridgnorth ; 52.53 ; -2.42
burh ; Runcorn ; 53.34 ; -2.73
burh ; Thelwall ; 53.38 ; -2.53
burh ; Hertford ; 51.80 ; -0.08
burh ; Bedford ; 52.14 ; -0.47
burh ; Towcester ; 52.13 ; -0.99
burh ; Maldon ; 51.73 ; 0.68
burh ; Colchester ; 51.89 ; 0.90
burh ; Huntingdon ; 52.33 ; -0.18
burh ; Cambridge ; 52.21 ; 0.12
burh ; Manchester ; 53.48 ; -2.24
burh ; Bakewell ; 53.21 ; -1.68
burh ; Gloucester ; 51.86 ; -2.24
burh ; Hereford ; 52.06 ; -2.72
burh ; Shrewsbury ; 52.71 ; -2.75
burh ; Witham ; 51.80 ; 0.64
burh ; Rhuddlan ; 53.29 ; -3.47
# ---- ring fortresses and other forts
ring_fort ; Trelleborg ; 55.39 ; 11.27
ring_fort ; Fyrkat ; 56.62 ; 9.77
ring_fort ; Aggersborg ; 57.00 ; 9.25
ring_fort ; Nonnebakken ; 55.39 ; 10.39
ring_fort ; Borgring ; 55.47 ; 12.12
ring_fort ; Trelleborgen ; 55.38 ; 13.15
ring_fort ; Middelburg ; 51.50 ; 3.61
ring_fort ; Oost-Souburg ; 51.46 ; 3.60
ring_fort ; Veurne ; 51.07 ; 2.66
ring_fort ; Bergues ; 50.97 ; 2.43
ring_fort ; Bourbourg ; 50.95 ; 2.20
ring_fort ; Oostburg ; 51.33 ; 3.49
fort ; Danevirke ; 54.48 ; 9.49
fort ; Burghead ; 57.70 ; -3.49
fort ; Camp de Péran ; 48.46 ; -2.80
fort ; Zutphen ; 52.14 ; 6.20
site ; Repton ; 52.84 ; -1.55
site ; Torksey ; 53.30 ; -0.74
site ; Lindholm Høje ; 57.08 ; 9.91
site ; Jarlshof ; 59.87 ; -1.29
site ; Brough of Birsay ; 59.14 ; -3.33
site ; Borre mound cemetery|Borrehaugene ; 59.38 ; 10.47
site ; Anundshög ; 59.63 ; 16.65
# ---- Slavic and Baltic strongholds
stronghold ; Mikulčice ; 48.80 ; 17.09
stronghold ; Pohansko ; 48.73 ; 16.89
stronghold ; Devín Castle|Devín ; 48.17 ; 16.98
stronghold ; Levý Hradec ; 50.17 ; 14.37
stronghold ; Stará Boleslav ; 50.20 ; 14.67
stronghold ; Libice nad Cidlinou ; 50.13 ; 15.18
stronghold ; Kouřim ; 50.00 ; 14.98
stronghold ; Ostrów Lednicki ; 52.53 ; 17.38
stronghold ; Giecz ; 52.32 ; 17.36
stronghold ; Kruszwica ; 52.67 ; 18.33
stronghold ; Santok ; 52.74 ; 15.41
stronghold ; Wiślica ; 50.35 ; 20.67
stronghold ; Sandomierz ; 50.68 ; 21.75
stronghold ; Przemyśl ; 49.78 ; 22.77
stronghold ; Brandenburg an der Havel ; 52.41 ; 12.56
stronghold ; Havelberg ; 52.83 ; 12.07
stronghold ; Dorf Mecklenburg|Burg Mecklenburg|Mecklenburg Castle ; 53.84 ; 11.47
stronghold ; Schwerin ; 53.63 ; 11.41
stronghold ; Ratzeburg ; 53.70 ; 10.77
stronghold ; Lenzen ; 53.09 ; 11.47
stronghold ; Jaromarsburg|Cape Arkona|Kap Arkona ; 54.68 ; 13.44
stronghold ; Garz/Rügen|Garz ; 54.32 ; 13.35
stronghold ; Groß Raden ; 53.74 ; 11.87
stronghold ; Meissen|Meißen ; 51.16 ; 13.47
stronghold ; Bautzen ; 51.18 ; 14.42
stronghold ; Lebus ; 52.42 ; 14.53
stronghold ; Slawenburg Raddusch|Raddusch ; 51.81 ; 14.03
stronghold ; Vyshhorod ; 50.58 ; 30.49
stronghold ; Pereiaslav ; 50.07 ; 31.46
stronghold ; Liubech ; 51.70 ; 30.66
stronghold ; Korosten ; 50.95 ; 28.63
stronghold ; Daugmale ; 56.83 ; 24.42
stronghold ; Iru hillfort|Iru linnamägi|Iru ; 59.46 ; 24.90
stronghold ; Varbola Stronghold|Varbola ; 59.03 ; 24.50
stronghold ; Otepää ; 58.06 ; 26.50
# ---- longphorts and the Norse towns of Ireland
longphort ; Dublin ; 53.35 ; -6.26
longphort ; Waterford ; 52.26 ; -7.11
longphort ; Wexford ; 52.34 ; -6.46
longphort ; Cork ; 51.90 ; -8.47
longphort ; Limerick ; 52.66 ; -8.63
longphort ; Linn Duachaill ; 53.89 ; -6.36
longphort ; Annagassan ; 53.88 ; -6.34
longphort ; Woodstown ; 52.25 ; -7.21
longphort ; Arklow ; 52.80 ; -6.16
longphort ; Wicklow ; 52.98 ; -6.04
longphort ; Carlingford ; 54.04 ; -6.19
longphort ; Youghal ; 51.95 ; -7.85
# ---- the monastic towns of Ireland, and the island monasteries
monastic_town ; Armagh ; 54.35 ; -6.65
monastic_town ; Kells ; 53.73 ; -6.88
monastic_town ; Clonmacnoise ; 53.33 ; -7.99
monastic_town ; Glendalough ; 53.01 ; -6.33
monastic_town ; Kildare ; 53.16 ; -6.91
monastic_town ; Clonard Abbey|Clonard ; 53.45 ; -7.02
monastic_town ; Bangor Abbey|Bangor ; 54.66 ; -5.67
monastic_town ; Durrow Abbey|Durrow ; 53.33 ; -7.52
monastic_town ; Monasterboice ; 53.78 ; -6.42
monastic_town ; Lismore ; 52.14 ; -7.93
monastic_town ; Emly ; 52.46 ; -8.35
monastic_town ; Clonfert ; 53.24 ; -8.06
monastic_town ; Devenish Island|Devenish ; 54.37 ; -7.66
monastic_town ; Inis Cealtra|Holy Island ; 52.92 ; -8.45
monastic_town ; Skellig Michael ; 51.77 ; -10.54
monastic_town ; Nendrum Monastery|Nendrum ; 54.50 ; -5.65
monastic_town ; Movilla Abbey|Movilla ; 54.60 ; -5.67
monastic_town ; Downpatrick ; 54.33 ; -5.72
monastic_town ; Derry ; 55.00 ; -7.32
monastic_town ; Tallaght ; 53.29 ; -6.37
monastic_town ; Swords ; 53.46 ; -6.22
monastic_town ; Lusk ; 53.53 ; -6.17
monastic_town ; Clondalkin ; 53.32 ; -6.39
monastic_town ; Ferns ; 52.59 ; -6.50
monastic_town ; Aghaboe ; 52.92 ; -7.51
monastic_town ; Birr ; 53.10 ; -7.91
monastic_town ; Roscrea ; 52.95 ; -7.80
monastic_town ; Terryglass ; 53.05 ; -8.20
monastic_town ; Lorrha ; 53.09 ; -8.12
monastic_town ; Killaloe ; 52.81 ; -8.44
monastic_town ; Ardfert ; 52.33 ; -9.78
monastic_town ; Tuam ; 53.51 ; -8.85
monastic_town ; Cong ; 53.54 ; -9.28
monastic_town ; Inishmurray ; 54.43 ; -8.66
monastic_town ; Kilmacduagh ; 53.05 ; -8.89
monastic_town ; Clones ; 54.18 ; -7.23
monastic_town ; Louth ; 53.95 ; -6.54
monastic_town ; Duleek ; 53.66 ; -6.42
monastic_town ; Slane ; 53.71 ; -6.54
monastic_town ; Trim ; 53.55 ; -6.79
monastic_town ; Fore Abbey|Fore ; 53.68 ; -7.23
monastic_town ; Castledermot ; 52.91 ; -6.84
monastic_town ; Old Leighlin|Leighlin ; 52.74 ; -7.02
monastic_town ; St Mullin's|St Mullins ; 52.49 ; -6.93
monastic_town ; Ardmore ; 51.95 ; -7.72
monastic_town ; Cloyne ; 51.86 ; -8.12
monastic_town ; Rosscarbery ; 51.58 ; -9.04
monastic_town ; Scattery Island|Inis Cathaigh ; 52.61 ; -9.52
monastery ; Iona ; 56.33 ; -6.39
monastery ; Lindisfarne ; 55.67 ; -1.80
monastery ; St Davids ; 51.88 ; -5.27
monastery ; Llantwit Major ; 51.41 ; -3.49
monastery ; Portmahomack ; 57.84 ; -3.83
monastery ; Govan ; 55.86 ; -4.31
monastery ; Peel ; 54.22 ; -4.69
# ---- thing sites
thing ; Tynwald Hill|Tynwald ; 54.20 ; -4.64
thing ; Gulating|Gulatinget ; 61.00 ; 5.10
thing ; Frostating|Frostatinget ; 63.59 ; 10.70
thing ; Thynghowe ; 53.20 ; -1.09
thing ; Tingwall ; 60.19 ; -1.25
thing ; Law Ting Holm ; 60.20 ; -1.25
thing ; Dingwall ; 57.60 ; -4.43
thing ; Thingwall ; 53.35 ; -3.09
thing ; Tinganes ; 62.01 ; -6.77
thing ; Arkils tingstad|Arkels tingstad ; 59.44 ; 18.05
thing ; Mora stenar|Stones of Mora|Mora stone ; 59.79 ; 17.77
# ---- ship finds
ship_find ; Oseberghaugen|Oseberg Mound|Oseberg mound ; 59.31 ; 10.44
ship_find ; Gokstadhaugen|Gokstad Mound|Gokstad mound ; 59.14 ; 10.25
ship_find ; Ladby ship|Ladbyskibet ; 55.44 ; 10.62
ship_find ; Skuldelev ships|Skuldelevskibene ; 55.80 ; 12.06
ship_find ; Salme ships ; 58.17 ; 22.25
ship_find ; Balladoole ; 54.08 ; -4.68
ship_find ; Scar boat burial ; 59.30 ; -2.57
ship_find ; Port an Eilean Mhòir boat burial|Port an Eilean Mhòir ship burial ; 56.74 ; -6.13
ship_find ; Myklebust Ship|Myklebustskipet ; 61.91 ; 5.99
ship_find ; Storhaug ; 59.37 ; 5.29
ship_find ; Valsgärde ; 59.90 ; 17.63
ship_find ; Vendel ; 60.16 ; 17.60
ship_find ; Ultuna ; 59.82 ; 17.65
ship_find ; Graveney boat ; 51.33 ; 0.95
ship_find ; Utrecht ship|Utrechtse schip ; 52.09 ; 5.12
"""

# the mints that struck before 1067 (Carolingian, Ottonian and Salian, Anglo-Saxon, Viking Age): a mint item of Wikidata
# with one of these names is kept, when it lies by a place already in the list
EARLY_MINTS = u"""Dorestad Quentovic Melle Haithabu Hedeby Tilleda Trier Cologne Köln Mainz Worms Speyer Strasbourg Straßburg Regensburg
Magdeburg Würzburg Verdun Metz Tours Lyon Vienne Arles Marseille Narbonne Toulouse Bourges Orléans Paris Rouen Reims Sens
Chalon-sur-Saône Utrecht Deventer Tiel Stavoren Dokkum Leeuwarden Bolsward Emden Jever Bardowick Stade Bremen Duisburg Dortmund
Soest Xanten Andernach Koblenz Aachen Maastricht Liège Lüttich Huy Namur Dinant Visé Brussels Ghent Bruges Arras Saint-Omer
Amiens Hildesheim Goslar Quedlinburg Halberstadt Merseburg Erfurt Fulda Corvey Paderborn Münster Osnabrück Minden Konstanz
Zürich Basel Chur Augsburg Passau Salzburg Freising Eichstätt Nabburg Cham Esslingen Breisach Lorsch Toul Cambrai Tournai
Nijmegen Groningen Lüneburg Gittelde Meissen Meißen Prague London Canterbury York Winchester Lincoln Lund Roskilde Viborg Ribe
Sigtuna Dublin Milan Pavia Verona Venice Lucca Luca Kyiv Kiev Kraków Gniezno Poznań""".split()


def named_list():
    out = []
    for line in NAMED.splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        kind, names, lat, lon = [p.strip() for p in line.split(";")]
        out.append({"kind": kind, "names": [n.strip() for n in names.split("|")], "lat": float(lat), "lon": float(lon)})
    return out


SEED_LANGS = ["en", "mul", "de", "fr", "nl", "da", "sv", "nb", "pl", "ru", "ga", "cs", "lt", "lv", "et", "uk", "be", "it"]
SEED_SETTLE = ["Q486972", "Q15284"]
SEED_SITE = ["Q839954", "Q57821", "Q23413", "Q44613", "Q24398318", "Q16560", "Q34023", "Q2593777", "Q11446", "Q1145397",
             "Q4989906", "Q1363360", "Q88598", "Q744099"]
SEED_ISLAND = ["Q23442"]


def q_named(seeds):
    lits = []
    for s in seeds:
        for n in s["names"]:
            for lang in SEED_LANGS:
                lits.append('"%s"@%s' % (n.replace("\\", "\\\\").replace('"', '\\"'), lang))

    def flag(var, roots):
        return "BIND(EXISTS { ?item wdt:P31/wdt:P279* ?r%s . FILTER(?r%s IN (%s)) } AS ?%s)" % (var, var, ", ".join("wd:" + q for q in roots), var)
    return ("SELECT ?name ?item ?coord ?links ?settle ?site ?island WHERE { VALUES ?name { %s } "
            "{ ?item rdfs:label ?name } UNION { ?item skos:altLabel ?name } ?item wdt:P625 ?coord . "
            "OPTIONAL { ?item wikibase:sitelinks ?links } %s %s %s }" % (" ".join(lits), flag("settle", SEED_SETTLE), flag("site", SEED_SITE), flag("island", SEED_ISLAND)))


SEED_WANTS = {   # which kind of thing a named place most likely is: settle, site, island (higher wins)
    "town": (3, 2, 1), "burh": (3, 2, 1), "emporium": (3, 2, 1), "longphort": (3, 2, 1), "palace": (3, 3, 1), "mint": (3, 2, 1),
    "monastic_town": (2, 3, 2), "monastery": (2, 3, 2), "ring_fort": (1, 3, 1), "fort": (1, 3, 1), "stronghold": (2, 3, 1),
    "thing": (1, 3, 1), "ship_find": (1, 3, 1), "site": (1, 3, 1),
}


def point(text):
    m = re.match(r"Point\(([-0-9.eE]+) ([-0-9.eE]+)\)", text or "")
    return (float(m.group(2)), float(m.group(1))) if m else None


def pick_named(seed, rows):
    """The best of Wikidata's things of that name: near, of the right sort, and the best known."""
    best, best_score = None, None
    for r in rows:
        if r.get("name") not in seed["names"]:
            continue
        p = point(r.get("coord"))
        if not p:
            continue
        d = km(seed["lat"], seed["lon"], p[0], p[1])
        if d > 30.0:
            continue
        w = SEED_WANTS[seed["kind"]]
        pref = max(w[0] if r.get("settle") == "true" else 0, w[1] if r.get("site") == "true" else 0, w[2] if r.get("island") == "true" else 0)
        if pref == 0 and seed["kind"] not in ("thing", "ship_find", "site"):
            continue
        score = pref * 1000 + min(int(r.get("links") or 0), 300) - 3.0 * d
        if best is None or score > best_score:
            best, best_score = {"qid": r["item"], "lat": p[0], "lon": p[1], "km": round(d, 1)}, score
    return best


# ------------------------------------------------------------------------------------------------ raw files
def raw_path(kind):
    return os.path.join(RAW, "raw_%s.json" % kind)


def load_json(path, default=None):
    if not os.path.exists(path):
        return default
    with io.open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def save_json(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    tmp = path + ".tmp"
    with io.open(tmp, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=None, separators=(",", ":"))
    os.replace(tmp, path)


def now():
    return datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def find(fresh, no_ask, domesday):
    """Ask every kind's queries (or read them back from disk). -> {kind: [query entries with rows]}"""
    by_kind = {}
    for a in asks(domesday):
        by_kind.setdefault(a["kind"], []).append(a)
    out = {}
    for kind, entries in by_kind.items():
        old = load_json(raw_path(kind), {"queries": []})
        have = {q["name"]: q for q in old.get("queries", [])}
        done, changed = [], False
        for a in entries:
            q = have.get(a["name"])
            if q and q.get("sparql") == a["sparql"] and not q.get("failed") and not (kind in fresh or "all" in fresh):
                done.append(dict(a, rows=q["rows"], asked=q.get("asked"), seconds=q.get("seconds"), failed=[]))
                continue
            if no_ask:
                log("  %-11s %-55s not on disk (and --no-ask): left out" % (kind, a["name"][:55]))
                continue
            t0 = time.time()
            rows, failed = ask_boxed(a["sparql"])
            log("  %-11s %-55s %6d rows  %5.1f s%s" % (kind, a["name"][:55], len(rows), time.time() - t0, "  (boxes left out: %d)" % len(failed) if failed else ""))
            done.append(dict(a, rows=rows, asked=now(), seconds=round(time.time() - t0, 1), failed=failed))
            changed = True
        if changed or not os.path.exists(raw_path(kind)):
            save_json(raw_path(kind), {"kind": kind, "endpoint": ENDPOINT, "credit": CREDIT,
                                       "about": "The answers of Wikidata's SPARQL service to the queries of Bake/gazetteer/make_gazetteer.py, values only (an entity as its Q-number).",
                                       "queries": [{k: d[k] for k in ("name", "sparql", "gives", "soft", "how", "asked", "seconds", "failed", "rows")} for d in done]})
        out[kind] = done
    return out


def find_named(fresh, no_ask):
    seeds = named_list()
    old = load_json(raw_path("named"), {"queries": []})
    have = {q["sparql"]: q for q in old.get("queries", [])}
    queries, changed = [], False
    for i in range(0, len(seeds), 15):
        part = seeds[i:i + 15]
        sparql = q_named(part)
        q = have.get(sparql)
        if q and not ("named" in fresh or "all" in fresh):
            queries.append(q)
            continue
        if no_ask:
            continue
        t0 = time.time()
        try:
            rows = [simple(b) for b in ask(sparql)]
        except Heavy:
            log("  named       names %d-%d: too heavy, left out" % (i + 1, i + len(part)))
            continue
        log("  named       names %3d-%3d %-43s %6d rows  %5.1f s" % (i + 1, i + len(part), "", len(rows), time.time() - t0))
        queries.append({"names": [s["names"][0] for s in part], "sparql": sparql, "asked": now(), "rows": rows})
        changed = True
    if changed:
        save_json(raw_path("named"), {"kind": "named", "endpoint": ENDPOINT, "credit": CREDIT,
                                      "about": "The list of names in make_gazetteer.py, looked up in Wikidata by label: every thing of that name with coordinates.",
                                      "queries": queries})
    rows = [r for q in queries for r in q["rows"]]
    found, missing = [], []
    for s in seeds:
        hit = pick_named(s, rows)
        if hit:
            found.append(dict(s, **hit))
        else:
            missing.append(s)
    return found, missing


LABEL_LANGS = ["en", "mul", "de", "fr", "nl", "da", "sv", "nb", "nn", "fi", "et", "lv", "lt", "pl", "cs", "sk", "ru", "be", "uk", "ga",
               "cy", "gd", "gv", "it", "es", "ca", "hu", "ro", "sl", "hr", "sr", "bs", "bg", "fo", "lb", "fy", "br"]


def find_details(qids, fresh, no_ask):
    """What each thing is: coordinates, dates, classes, country, labels. Asked 300 at a time; kept on disk."""
    path = raw_path("details")
    data = load_json(path, None)
    if data is None or "details" in fresh or "all" in fresh:
        data = {"kind": "details", "endpoint": ENDPOINT, "credit": CREDIT,
                "about": "For every thing found: coordinates, dates and sitelinks (facts), classes, country codes, native labels and ancient-world ids (marks), labels (labels).",
                "done": [], "facts": [], "marks": [], "labels": []}
    done = set(data["done"])
    todo = sorted(q for q in qids if q not in done)
    if no_ask:
        todo = []
    langs = ", ".join('"%s"' % l for l in LABEL_LANGS)
    for i in range(0, len(todo), 300):
        part = todo[i:i + 300]
        vals = wds(part)
        t0 = time.time()
        q1 = ("SELECT ?item (SAMPLE(?c) AS ?coord) (MIN(?i) AS ?inc) (MAX(?d) AS ?dis) (MIN(?r) AS ?rec) (MIN(?s) AS ?st) (MAX(?e) AS ?en) "
              "(MIN(?p) AS ?pit) (SAMPLE(?l) AS ?links) WHERE { VALUES ?item { %s } OPTIONAL { ?item wdt:P625 ?c } "
              "OPTIONAL { ?item wdt:P571 ?i . FILTER(isLiteral(?i)) } OPTIONAL { ?item wdt:P576 ?d . FILTER(isLiteral(?d)) } "
              "OPTIONAL { ?item wdt:P1249 ?r . FILTER(isLiteral(?r)) } OPTIONAL { ?item wdt:P580 ?s . FILTER(isLiteral(?s)) } "
              "OPTIONAL { ?item wdt:P582 ?e . FILTER(isLiteral(?e)) } OPTIONAL { ?item wdt:P585 ?p . FILTER(isLiteral(?p)) } "
              "OPTIONAL { ?item wikibase:sitelinks ?l } } GROUP BY ?item" % vals)
        q2 = ('SELECT ?item ?k ?v WHERE { VALUES ?item { %s } { ?item wdt:P31 ?v . BIND("c" AS ?k) } UNION '
              '{ ?item wdt:P17 ?land . ?land wdt:P297 ?v . BIND("iso" AS ?k) } UNION { ?item wdt:P1705 ?v . BIND("nat" AS ?k) } UNION '
              '{ ?item wdt:P1584|wdt:P1936|wdt:P1481 ?x . BIND("anc" AS ?k) BIND("1" AS ?v) } }' % vals)
        q3 = "SELECT ?item ?l WHERE { VALUES ?item { %s } ?item rdfs:label ?l . FILTER(lang(?l) IN (%s)) }" % (vals, langs)
        try:
            facts = [simple(b) for b in ask(q1)]
            marks = [simple(b) for b in ask(q2)]
            labels = [simple(b) for b in ask(q3)]
        except Heavy:
            log("  details     %d-%d: too heavy, left out" % (i + 1, i + len(part)))
            continue
        data["facts"] += facts
        data["marks"] += marks
        data["labels"] += labels
        data["done"] += part
        log("  details     %5d-%5d of %5d   %5.1f s" % (i + 1, i + len(part), len(todo), time.time() - t0))
        if (i // 300) % 10 == 9:
            save_json(path, data)
    if todo:
        save_json(path, data)
    # a country with no ISO code of its own in Wikidata (the Netherlands, Q55, had none on 6 Oct 2026): ask for the
    # country itself, and read its code from LAND_ISO
    have_iso = set(r["item"] for r in data["marks"] if r["k"] in ("iso", "land"))
    asked_land = set(data.get("land_done", []))
    todo = [] if no_ask else sorted(q for q in data["done"] if q not in have_iso and q not in asked_land)
    for i in range(0, len(todo), 300):
        part = todo[i:i + 300]
        try:
            rows = [simple(b) for b in ask('SELECT ?item ?k ?v WHERE { VALUES ?item { %s } ?item wdt:P17 ?v . BIND("land" AS ?k) }' % wds(part))]
        except Heavy:
            continue
        data["marks"] += rows
        data.setdefault("land_done", []).extend(part)
        log("  countries   %5d-%5d of %5d" % (i + 1, i + len(part), len(todo)))
    if todo:
        save_json(path, data)
    return data


LAND_ISO = {"Q55": "NL", "Q29999": "NL"}


# what a thing is, by the classes it stands under
ROOTS = {
    "settlement": SETTLEMENT, "monastery": ["Q44613"], "cathedral": ["Q2977"], "bishopric": ["Q665487", "Q3146899"],
    "church": ["Q16970", "Q24398318"], "palace": ["Q16560", "Q53536964", "Q1796442", "Q17296484", "Q1788752"],
    "castle": ["Q23413", "Q57821", "Q744099", "Q1363360"], "gord": ["Q88598"], "ringfort": ["Q586552"], "burh": ["Q88034"],
    "mint": ["Q464780"], "thing": ["Q387834"], "battle": ["Q178561", "Q188055"],
    "shipfind": ["Q1145397", "Q124473037", "Q211969", "Q897611", "Q852190"], "runestone": ["Q815241"], "picturestone": ["Q602202"],
    "runic": ["Q7379880"], "notstone": ["Q208820", "Q848960"],
    "site": ["Q839954", "Q34023", "Q39614", "Q2593777", "Q4989906", "Q1081138"], "structure": ["Q811979"],
    "bad": ["Q5", "Q33506", "Q386724", "Q13442814", "Q4167410", "Q16521", "Q79007", "Q55488", "Q3305213", "Q27686", "Q3914",
            "Q4830453", "Q1076486", "Q483110", "Q7075", "Q11707", "Q133215", "Q131734", "Q180958", "Q1307276"],
    "realm": ["Q7275", "Q3024240"], "admin": ["Q56061"], "water": ["Q23397"],
}
ROOT_OF = {}
for _name, _ids in ROOTS.items():
    for _q in _ids:
        ROOT_OF.setdefault(_q, set()).add(_name)


def find_classes(classes, fresh, no_ask):
    path = raw_path("classes")
    data = load_json(path, None)
    roots = sorted(ROOT_OF)
    if data is None or data.get("roots") != roots or "classes" in fresh or "all" in fresh:
        data = {"kind": "classes", "endpoint": ENDPOINT, "credit": CREDIT, "about": "Which of the script's root classes each class found stands under (P279).",
                "roots": roots, "done": [], "rows": []}
    done = set(data["done"])
    todo = sorted(c for c in classes if c not in done and re.match(r"Q[0-9]+$", c))   # "unknown value" is no class
    if no_ask:
        todo = []
    for i in range(0, len(todo), 150):
        part = todo[i:i + 150]
        q = "SELECT ?c ?root WHERE { VALUES ?c { %s } ?c wdt:P279* ?root . FILTER(?root IN (%s)) }" % (wds(part), ", ".join("wd:" + r for r in roots))
        t0 = time.time()
        try:
            rows = [simple(b) for b in ask(q)]
        except Heavy:
            log("  classes     %d-%d: too heavy, left out" % (i + 1, i + len(part)))
            continue
        data["rows"] += rows
        data["done"] += part
        log("  classes     %5d-%5d of %5d   %5.1f s" % (i + 1, i + len(part), len(todo), time.time() - t0))
    if todo:
        save_json(path, data)
    out = {}
    for r in data["rows"]:
        out.setdefault(r["c"], set()).update(ROOT_OF.get(r["root"], ()))
    return out


# ------------------------------------------------------------------------------------------------ the merge
LOCAL = {   # the tongue(s) a place's local name is given in, by country
    "GB": ["en"], "IE": ["ga", "en"], "IM": ["gv", "en"], "FR": ["fr"], "BE": ["nl", "fr"], "NL": ["nl"], "LU": ["lb", "fr", "de"], "DE": ["de"],
    "DK": ["da"], "NO": ["nb", "nn"], "SE": ["sv"], "FI": ["fi", "sv"], "EE": ["et"], "LV": ["lv"], "LT": ["lt"], "PL": ["pl"], "CZ": ["cs"],
    "SK": ["sk"], "AT": ["de"], "CH": ["de", "fr", "it"], "LI": ["de"], "IT": ["it"], "ES": ["es", "ca"], "AD": ["ca"], "MC": ["fr"], "SM": ["it"],
    "RU": ["ru"], "BY": ["be", "ru"], "UA": ["uk"], "MD": ["ro"], "HU": ["hu"], "RO": ["ro"], "SI": ["sl"], "HR": ["hr"], "RS": ["sr"],
    "BA": ["bs", "hr", "sr"], "BG": ["bg"], "FO": ["fo", "da"], "ME": ["sr"], "JE": ["en", "fr"], "GG": ["en", "fr"], "AX": ["sv"],
}
COUNTRY = {"GB": "United Kingdom", "IE": "Ireland", "IM": "Isle of Man", "FR": "France", "BE": "Belgium", "NL": "Netherlands", "LU": "Luxembourg",
           "DE": "Germany", "DK": "Denmark", "NO": "Norway", "SE": "Sweden", "FI": "Finland", "EE": "Estonia", "LV": "Latvia", "LT": "Lithuania",
           "PL": "Poland", "CZ": "Czechia", "SK": "Slovakia", "AT": "Austria", "CH": "Switzerland", "LI": "Liechtenstein", "IT": "Italy", "ES": "Spain",
           "AD": "Andorra", "MC": "Monaco", "SM": "San Marino", "RU": "Russia", "BY": "Belarus", "UA": "Ukraine", "MD": "Moldova", "HU": "Hungary",
           "RO": "Romania", "SI": "Slovenia", "HR": "Croatia", "RS": "Serbia", "BA": "Bosnia and Herzegovina", "BG": "Bulgaria", "FO": "Faroe Islands",
           "ME": "Montenegro", "JE": "Jersey", "GG": "Guernsey", "AX": "Åland", "": "not known (at sea, or no country in Wikidata)"}
PRIORITY = ["emporium", "longphort", "ring_fort", "burh", "palace", "thing", "mint", "monastic_town", "cathedral", "bishopric", "monastery",
            "stronghold", "ship_find", "battle", "roman_town", "town", "fort", "church", "site", "settlement"]
KIND_MEANS = {
    "emporium": "an emporium or trading place", "longphort": "a longphort, or a Norse town of Ireland", "ring_fort": "a ring fortress",
    "burh": "an Anglo-Saxon burh", "palace": "a royal palace, Pfalz, royal vill or king's farm", "thing": "a thing site", "mint": "a mint",
    "monastic_town": "a monastic town of Ireland", "cathedral": "a cathedral", "bishopric": "a bishop's seat (the bishopric)",
    "monastery": "a monastery or abbey", "stronghold": "a Slavic or Baltic stronghold", "ship_find": "a ship find", "battle": "a battle or siege",
    "roman_town": "a Roman town still lived in", "town": "a town, city or village known before 1067", "fort": "a fort, castle or rampart",
    "church": "a church", "site": "an archaeological site or other remains of the time", "settlement": "an English vill of Domesday (1066-86)",
    "runestone": "a rune stone", "picture_stone": "a picture stone", "runic_inscription": "a runic inscription (a Rundata id)",
}
KNOWN = {"emporium", "longphort", "ring_fort", "burh", "palace", "thing", "mint", "monastic_town", "cathedral", "bishopric"}   # known by name, when sure
LINKS_FOR_TIER_2 = {"town": 20, "roman_town": 20, "monastery": 8, "stronghold": 8, "fort": 10, "church": 15, "settlement": 25, "site": 15,
                    "battle": 9999, "ship_find": 9999}
GOOD = {"settlement", "monastery", "cathedral", "bishopric", "church", "palace", "castle", "gord", "ringfort", "burh", "mint", "thing", "site",
        "shipfind", "battle"}


def year(text):
    m = re.match(r"^(-?\d+)-", text or "")
    return int(m.group(1)) if m else 0


def root_kind(roots, frm, anc):
    for root, kind in (("mint", "mint"), ("burh", "burh"), ("ringfort", "ring_fort"), ("gord", "stronghold"), ("thing", "thing"),
                       ("palace", "palace"), ("cathedral", "cathedral"), ("bishopric", "bishopric"), ("monastery", "monastery"),
                       ("battle", "battle"), ("shipfind", "ship_find"), ("castle", "fort"), ("church", "church")):
        if root in roots:
            return kind
    if "settlement" in roots:
        return "roman_town" if (anc or (frm and frm <= 476)) else "town"
    if "site" in roots or "structure" in roots:
        return "site"
    return None


def norm(s):
    s = s.lower().replace("ø", "o").replace("æ", "ae").replace("ð", "d").replace("þ", "th").replace("ß", "ss").replace("ł", "l").replace("đ", "d")
    s = "".join(c for c in unicodedata.normalize("NFKD", s) if not unicodedata.combining(c))
    return re.sub(r"[^a-z0-9]+", " ", s).strip()


def slug(s):
    return norm(s).replace(" ", "-")


def merge(found, named, details, class_roots, domesday):
    """-> places, stones (before the ports and the ids)."""
    # 1. what the queries found, by thing
    cand = {}

    def see(qid, gives, how, soft, lat=None, lon=None, when=None):
        c = cand.setdefault(qid, {"ev": [], "lat": None, "lon": None, "when": 0, "seed": None})
        c["ev"].append((gives, how, soft))
        if lat is not None and c["lat"] is None:
            c["lat"], c["lon"] = lat, lon
        if when and not c["when"]:
            c["when"] = when
    for kind, entries in found.items():
        if kind == "settlement" and not domesday:
            continue
        for a in entries:
            for r in a["rows"]:
                lat = lon = None
                if "lat" in r:
                    lat, lon = float(r["lat"]), float(r["lon"])
                else:
                    p = point(r.get("coord")) or point(r.get("loccoord"))
                    if p:
                        lat, lon = p
                how = a["how"] + (": " + r["words"].replace('"', "") if a["soft"] and r.get("words") else "")
                see(r["item"], a["gives"], how, a["soft"], lat, lon, year(r.get("from") or r.get("date")))
    for s in named:
        c = cand.setdefault(s["qid"], {"ev": [], "lat": None, "lon": None, "when": 0, "seed": None})
        if c["seed"] is None or PRIORITY.index(s["kind"]) < PRIORITY.index(c["seed"]):
            c["seed"] = s["kind"]
        if c["lat"] is None:
            c["lat"], c["lon"] = s["lat"], s["lon"]

    # 2. what each thing is
    D = {}
    for r in details["facts"]:
        D[r["item"]] = {"coord": point(r.get("coord")), "inc": year(r.get("inc")), "dis": year(r.get("dis")), "rec": year(r.get("rec")),
                        "st": year(r.get("st")), "en": year(r.get("en")), "pit": year(r.get("pit")), "links": int(r.get("links") or 0),
                        "classes": set(), "iso": set(), "nat": "", "anc": False, "labels": {}}
    for r in details["marks"]:
        d = D.get(r["item"])
        if d is None:
            continue
        if r["k"] == "c":
            d["classes"].add(r["v"])
        elif r["k"] == "iso":
            d["iso"].add(r["v"])
        elif r["k"] == "land" and r["v"] in LAND_ISO:
            d["iso"].add(LAND_ISO[r["v"]])
        elif r["k"] == "nat" and not d["nat"]:
            d["nat"] = r["v"]
        elif r["k"] == "anc":
            d["anc"] = True
    for r in details["labels"]:
        d = D.get(r["item"])
        if d is not None:
            d["labels"][r.get("l@", "")] = r["l"]

    mint_names = set(norm(n) for n in EARLY_MINTS)
    places, stones, mints_waiting, dropped = [], [], [], {}

    def drop(why):
        dropped[why] = dropped.get(why, 0) + 1
    for qid, c in cand.items():
        d = D.get(qid)
        if d is None:
            drop("no details")
            continue
        pos = d["coord"] or ((c["lat"], c["lon"]) if c["lat"] is not None else None)
        if not pos or not in_box(pos[0], pos[1]):
            drop("outside the window, or no coordinates")
            continue
        roots = set()
        for cl in d["classes"]:
            roots |= class_roots.get(cl, set())
        is_battle = any(g == "battle" for g, _h, _s in c["ev"])
        frm = min([y for y in (d["inc"], d["rec"]) if y] or [0]) or d["st"] or (d["pit"] if is_battle else 0) or c["when"]
        to = d["dis"] or d["en"] or (d["pit"] if is_battle else 0)
        if is_battle:
            frm = frm or c["when"]
            to = to or frm
        labels = d["labels"]
        iso = sorted(i for i in d["iso"] if i in LOCAL)
        iso = iso[0] if iso else ""
        name = labels.get("en") or labels.get("mul") or ""
        local = d["nat"]
        if not local:
            for lang in LOCAL.get(iso, []):
                if labels.get(lang):
                    local = labels[lang]
                    break
        if not name:
            name = local or next((labels[l] for l in LABEL_LANGS if labels.get(l)), "")
        if not name:
            drop("no name in any of the languages read")
            continue
        rec = {"qid": qid, "name": name, "name_local": local or name, "lat": round(pos[0], 5), "lon": round(pos[1], 5), "from": frm, "to": to,
               "links": d["links"], "country": iso, "labels": set(norm(v) for v in labels.values()) | {norm(local or name)}}

        # the stones: their own layer
        stone = [e for e in c["ev"] if e[0] == "stone"]
        if stone and not c["seed"]:
            if "notstone" in roots or ("bad" in roots and not (roots & {"runestone", "picturestone", "runic"})):
                drop("a Rundata id on something that is not a stone")
                continue
            if frm and frm > 1150:
                drop("a stone or inscription dated after 1150")
                continue
            rec["kind"] = "picture_stone" if "picturestone" in roots else "runestone" if "runestone" in roots else "runic_inscription"
            rec["tier"], rec["source"], rec["also"] = 3, "wikidata (%s)" % stone[0][1], []
            stones.append(rec)
            continue

        bad = "bad" in roots and not (roots & GOOD)
        plain = "settlement" in roots and not (roots & (GOOD - {"settlement"}))
        rk = root_kind(roots, frm, d["anc"])
        kinds = []
        if c["seed"]:
            kinds.append((-1, c["seed"], "named", True))
            if frm > YEAR_TO:                # a town's later charter is not its beginning
                rec["from"] = frm = 0
        for gives, how, soft in c["ev"]:
            if gives == "stone":
                continue
            if bad:
                continue
            if not c["seed"] and ((frm and frm > YEAR_TO) or (to and to < YEAR_FROM)):
                continue
            k = gives
            if k is None:
                k = rk or ("site" if "tag" in how else None)
            elif soft:
                if k in ("stronghold", "fort", "ring_fort", "ship_find", "thing") and plain:
                    continue
                if k == "site":
                    k = rk if rk and rk not in ("town", "roman_town") else "site"
                if k == "irish":
                    k = "longphort" if "longphort" in how else "monastery"
            else:
                if k == "town" and frm and frm <= 476:
                    k = "roman_town"
                if k == "roman_town" and "settlement" not in roots:
                    continue
                if k == "fort" and rk in ("ring_fort", "stronghold", "palace"):
                    k = rk
            if k == "mint" and not c["seed"]:
                if not (frm and frm <= YEAR_TO):
                    words = set(norm(rec["name"]).split()) | set(norm(rec["name_local"]).split())
                    if words & mint_names:
                        mints_waiting.append(rec)
                    continue
            if k:
                kinds.append((PRIORITY.index(k), k, how, not soft))
        if not kinds:
            drop("a person, a work, a museum" if bad else "not of the time (founded after 1066, or gone before 750)" if ((frm and frm > YEAR_TO) or (to and to < YEAR_FROM)) else "dated, but not a place (a realm, an office, a people)" if rk is None else "found by words only, on a plain town or village")
            continue
        kinds.sort(key=lambda k: k[0])
        rec["kind"] = kinds[0][1]
        rec["sure"] = kinds[0][3]
        rec["named"] = bool(c["seed"])
        rec["source"] = "wikidata (%s)" % kinds[0][2]
        rec["also"] = sorted(set(k[1] for k in kinds[1:]) - {rec["kind"]}, key=PRIORITY.index)
        places.append(rec)

    # 3. the early mints: kept when they lie by a place already in the list
    grid = {}
    for p in places:
        grid.setdefault((int(p["lat"] * 10), int(p["lon"] * 10)), []).append(p)

    def near(lat, lon, reach):
        out = []
        n = int(math.ceil(reach / 7.0))
        for a in range(int(lat * 10) - n, int(lat * 10) + n + 1):
            for b in range(int(lon * 10) - 2 * n, int(lon * 10) + 2 * n + 1):
                for p in grid.get((a, b), ()):
                    dist = km(lat, lon, p["lat"], p["lon"])
                    if dist <= reach:
                        out.append((dist, p))
        return sorted(out, key=lambda t: t[0])
    for m in mints_waiting:
        if near(m["lat"], m["lon"], 5.0):
            m.update(kind="mint", sure=True, named=False, source="wikidata (class; on the list of early mints)", also=[])
            places.append(m)
        else:
            drop("a mint with an early name, but by no place of the time")

    # 4. the same place twice (two things of one name and kind within 2 km): the better known stays
    places.sort(key=lambda p: (-p["links"], p["qid"]))
    kept, seen = [], {}
    for p in places:
        key = (norm(p["name"]), p["kind"])
        twin = next((o for o in seen.get(key, ()) if km(p["lat"], p["lon"], o["lat"], o["lon"]) < 2.0), None)
        if twin:
            drop("the same place twice")
            continue
        seen.setdefault(key, []).append(p)
        kept.append(p)
    places = kept

    # 5. a country for what has none: the nearest place that has one, within 80 km
    grid = {}
    for p in places:
        if p["country"]:
            grid.setdefault((int(p["lat"] * 10), int(p["lon"] * 10)), []).append(p)
    for p in places + stones:
        if not p["country"]:
            hit = near(p["lat"], p["lon"], 80.0)
            if hit:
                p["country"] = hit[0][1]["country"]

    # 6. the tiers 2 and 3
    for p in places:
        k = p["kind"]
        known = (p["named"] and k not in ("ship_find", "site", "battle")) or (p["sure"] and k in KNOWN) or p["links"] >= LINKS_FOR_TIER_2.get(k, 10)
        p["tier"] = 2 if known else 3
    return places, stones, dropped


PORT_KIND = {"emporium": "emporium", "market": "emporium", "town": "town", "royal seat": "palace", "monastery": "monastery", "village": "town", "fort": "fort"}
PORT_PREFERS = ["emporium", "town", "roman_town", "burh", "longphort", "monastic_town", "palace", "monastery", "settlement"]


def match_ports(places):
    """Tier 1: the 50 ports, each matched to the place of its name that lies nearest. A port with no such place is
    written in from ports_793.json itself."""
    ports = load_json(PORTS)["ports"]
    grid = {}
    for p in places:
        grid.setdefault((int(p["lat"] * 5), int(p["lon"] * 5)), []).append(p)
    used, table = set(), []
    for port in ports:
        words = [port["name"], port["id"].replace("_", " ")] + [w for w in re.split(r"[,()/]", port["modern"]) if w.strip() and "lost" not in w and "?" not in w]
        words = [norm(w) for w in words if norm(w)]
        best, best_key = None, None
        a0, b0 = int(port["lat"] * 5), int(port["lon"] * 5)
        for a in range(a0 - 2, a0 + 3):
            for b in range(b0 - 3, b0 + 4):
                for p in grid.get((a, b), ()):
                    if p["qid"] in used:
                        continue
                    dist = km(port["lat"], port["lon"], p["lat"], p["lon"])
                    if dist > 30.0:
                        continue
                    exact = any(w in p["labels"] for w in words)
                    close = exact or max(difflib.SequenceMatcher(None, w, l).ratio() for w in words for l in (p["labels"] or {""})) >= 0.84
                    if not close or (not exact and dist > 12.0):
                        continue
                    key = (0 if exact else 1, PORT_PREFERS.index(p["kind"]) if p["kind"] in PORT_PREFERS else len(PORT_PREFERS), dist)
                    if best is None or key < best_key:
                        best, best_key = p, key
        if best:
            used.add(best["qid"])
            best["tier"], best["port"] = 1, port["id"]
            table.append((port["id"], best["name"], best["qid"], best["kind"], round(km(port["lat"], port["lon"], best["lat"], best["lon"]), 1)))
        else:
            modern = [w.strip() for w in re.split(r"[,()/]", port["modern"]) if w.strip() and "lost" not in w and "?" not in w]
            places.append({"qid": "", "name": modern[0] if modern else port["name"], "name_local": port["name"], "lat": port["lat"], "lon": port["lon"],
                           "from": port.get("from", 0), "to": port.get("to", 0), "links": 0, "country": "", "labels": set(), "kind": PORT_KIND.get(port["kind"], "town"),
                           "tier": 1, "port": port["id"], "source": "ports_793.json (Wikidata has no place of this name here)", "also": [], "sure": True, "named": True})
            table.append((port["id"], port["name"], "", "", None))
    return table


def curated(places, stones):
    """The curators' own places, merged last: a record with a qid or an id already in the list changes that place
    (only the fields given); any other is added. "drop": qids or ids to leave out."""
    data = load_json(CURATED, None)
    if not data:
        return 0, 0
    by_qid = {p["qid"]: p for p in places + stones if p["qid"]}
    by_id = {p["id"]: p for p in places + stones}
    changed = added = 0
    for r in data.get("places", []):
        p = by_qid.get(r.get("qid") or "?") or by_id.get(r.get("id") or "?")
        if p:
            p.update({k: v for k, v in r.items() if v not in ("", None)})
            changed += 1
        else:
            r = dict({"qid": "", "name_local": r.get("name", ""), "from": 0, "to": 0, "tier": 3, "kind": "site", "source": "curated", "country": ""}, **r)
            r["id"] = r.get("id") or slug(r["name"])
            places.append(r)
            added += 1
    gone = set(data.get("drop", []))
    places[:] = [p for p in places if p["qid"] not in gone and p["id"] not in gone]
    stones[:] = [p for p in stones if p["qid"] not in gone and p["id"] not in gone]
    for p in places + stones:
        p["x"], p["z"] = to_world(p["lat"], p["lon"])
    return changed, added


def give_ids(places, stones):
    taken = set(p["port"] for p in places if p.get("port"))
    count = {}
    for p in places + stones:
        if not p.get("port"):
            s = slug(p["name"]) or p["qid"].lower()
            p["_slug"] = s
            count[s] = count.get(s, 0) + 1
    for p in places + stones:
        if p.get("port"):
            p["id"] = p["port"]
        else:
            s = p.pop("_slug")
            p["id"] = s if count[s] == 1 and s not in taken else "%s-%s" % (s, p["qid"].lower())
        p["x"], p["z"] = to_world(p["lat"], p["lon"])


FIELDS = ["id", "qid", "name", "name_local", "lat", "lon", "x", "z", "kind", "from", "to", "tier", "source", "country", "also"]


def line(p):
    r = {k: p.get(k, "") for k in FIELDS}
    if not r["also"]:
        del r["also"]
    return json.dumps(r, ensure_ascii=False)


def write_out(places, stones):
    ports = [p["id"] for p in load_json(PORTS)["ports"]]
    order = {pid: i for i, pid in enumerate(ports)}
    places.sort(key=lambda p: (p["tier"], order.get(p["id"], 0) if p["tier"] == 1 else 0, norm(p["name"]), p["id"]))
    stones.sort(key=lambda p: (norm(p["name"]), p["id"]))
    about = ("THE GAZETTEER OF 793-1066 - every place of the game's world that Wikidata knows to have been there some time between 750 and 1066 "
             "(longitude -11 to 33, latitude 43 to 69), for the map, the writers and the curators. name: today's name (English where there is one); "
             "name_local: the name in the land's own tongue; lat, lon: degrees; x, z: metres in the game's world frame (Lambert conformal, parallels 45N "
             "and 65N, centred 56N 5E; x east, z north); kind: " + "; ".join("%s = %s" % (k, KIND_MEANS[k]) for k in PRIORITY) +
             "; from, to: the years it is known from and to (0 = not known; a year below 0 is BC; a date known only to the century is that century's first year); "
             "tier: 1 = one of the 50 ports of ports_793.json (its id is the port's id), 2 = a place a merchant would know by name, 3 = the rest; "
             "source: where the record comes from and how it was found; country: today's country (ISO code), for sorting; also: what else the place is. "
             "stones: rune stones, picture stones and runic inscriptions, a light layer of their own, same fields. "
             "Made by Bake/gazetteer/make_gazetteer.py: do not edit by hand, put changes in Design/content/gazetteer/curated.json (see the README there).")
    with io.open(OUT, "w", encoding="utf-8", newline="\n") as f:
        f.write("{\n \"about\": %s,\n \"credit\": %s,\n \"places\": [\n" % (json.dumps(about, ensure_ascii=False), json.dumps(CREDIT, ensure_ascii=False)))
        f.write(",\n".join("  " + line(p) for p in places))
        f.write("\n ],\n \"stones\": [\n")
        f.write(",\n".join("  " + line(p) for p in stones))
        f.write("\n ]\n}\n")


# ------------------------------------------------------------------------------------------------ the picture
GROUPS = [   # three colours on the big map (validated together, any two side by side); one panel each on the right
    ("Towns and trading places", "#3987e5", ["emporium", "longphort", "burh", "town", "roman_town", "settlement"]),
    ("Monasteries, churches and bishops' seats", "#d95926", ["monastic_town", "monastery", "cathedral", "bishopric", "church"]),
    ("Halls, forts, things, finds and battles", "#199e70", ["palace", "mint", "thing", "ring_fort", "fort", "stronghold", "site", "ship_find", "battle"]),
]
PANELS = [
    ("Emporia, towns and longphorts", ["emporium", "longphort", "town", "roman_town"]),
    ("Burhs and the vills of Domesday", ["burh", "settlement"]),
    ("Monasteries and churches", ["monastic_town", "monastery", "church"]),
    ("Cathedrals and bishops' seats", ["cathedral", "bishopric"]),
    ("Palaces, mints and thing sites", ["palace", "mint", "thing"]),
    ("Ring fortresses, forts and strongholds", ["ring_fort", "fort", "stronghold"]),
    ("Sites, ship finds and battles", ["site", "ship_find", "battle"]),
    ("Rune stones and picture stones", None),
]
SEA, LAND, INK, INK2, EDGE = (26, 26, 25), (46, 46, 44), (255, 255, 255), (195, 194, 183), (56, 56, 53)


def picture(places, stones):
    import numpy as np
    from PIL import Image, ImageDraw, ImageFont
    meta = load_json(os.path.join(MAP_ART, "map_art.json"))
    over = Image.open(os.path.join(MAP_ART, meta.get("overview", "overview.png"))).convert("RGB")
    n = over.size[0]
    mpp = meta["metresPerPixel"] * meta["tilePixels"] * meta["cols"] / float(n)
    west, north = meta["west"], meta["north"]
    a = np.asarray(over).astype(np.int16)
    water = (a[:, :, 2] > a[:, :, 0] + 10) & (a[:, :, 2] >= a[:, :, 1])          # the chart's blues: sea, rivers, lakes
    base = np.where(water[:, :, None], np.array(SEA, dtype=np.uint8), np.array(LAND, dtype=np.uint8)).astype(np.uint8)
    big = Image.fromarray(base)

    def font(size, bold=False):
        try:
            return ImageFont.truetype(r"C:\Windows\Fonts\%s" % ("segoeuib.ttf" if bold else "segoeui.ttf"), size)
        except Exception:
            return ImageFont.load_default()

    def px(p, scale=1.0):
        return (p["x"] - west) / mpp * scale, (north - p["z"]) / mpp * scale
    colour = {}
    for _title, hexa, kinds in GROUPS:
        for k in kinds:
            colour[k] = tuple(int(hexa[i:i + 2], 16) for i in (1, 3, 5))
    d = ImageDraw.Draw(big)
    for tier, r, ring in ((3, 3, 1), (2, 5, 2)):
        for p in places:
            if p["tier"] != tier:
                continue
            x, y = px(p)
            if 0 <= x < n and 0 <= y < n:
                d.ellipse([x - r - ring, y - r - ring, x + r + ring, y + r + ring], fill=SEA)
                d.ellipse([x - r, y - r, x + r, y + r], fill=colour.get(p["kind"], colour["site"]))
    f_port, boxes = font(26), []
    for p in places:
        if p["tier"] != 1:
            continue
        x, y = px(p)
        if not (0 <= x < n and 0 <= y < n):
            continue
        d.ellipse([x - 11, y - 11, x + 11, y + 11], fill=SEA)
        d.ellipse([x - 9, y - 9, x + 9, y + 9], fill=INK)
        d.ellipse([x - 5, y - 5, x + 5, y + 5], fill=colour.get(p["kind"], colour["site"]))
    for p in places:
        if p["tier"] != 1:
            continue
        x, y = px(p)
        if not (0 <= x < n and 0 <= y < n):
            continue
        text = p["name"]
        w, h = d.textbbox((0, 0), text, font=f_port)[2:]
        for ox, oy in ((14, -h // 2 - 4), (-14 - w, -h // 2 - 4), (-w // 2, -h - 18), (-w // 2, 12), (14, -h - 12), (14, 8), (-14 - w, -h - 12), (-14 - w, 8)):
            box = (x + ox - 2, y + oy - 2, x + ox + w + 2, y + oy + h + 8)
            if box[0] < 0 or box[2] > n or box[1] < 0 or box[3] > n:
                continue
            if any(not (box[2] < b[0] or box[0] > b[2] or box[3] < b[1] or box[1] > b[3]) for b in boxes):
                continue
            boxes.append(box)
            d.text((x + ox, y + oy), text, font=f_port, fill=INK, stroke_width=3, stroke_fill=SEA)
            break
    # the title and the key, on the open sea of the north-west
    d.text((60, 50), "The gazetteer of 793-1066", font=font(64, True), fill=INK)
    d.text((60, 132), "%d places and %d stones from Wikidata (CC0)" % (len(places), len(stones)), font=font(34), fill=INK2)
    y = 200
    for title, hexa, kinds in GROUPS:
        c = tuple(int(hexa[i:i + 2], 16) for i in (1, 3, 5))
        d.ellipse([64, y + 8, 88, y + 32], fill=c)
        d.text((104, y), "%s  (%d)" % (title, sum(1 for p in places if p["kind"] in kinds)), font=font(30), fill=INK)
        y += 46
    y += 14
    d.ellipse([62, y + 6, 90, y + 34], fill=INK)
    d.ellipse([70, y + 14, 82, y + 26], fill=LAND)
    d.text((104, y), "one of the 50 ports, named  (%d)" % sum(1 for p in places if p["tier"] == 1), font=font(30), fill=INK)
    y += 46
    d.ellipse([69, y + 13, 83, y + 27], fill=INK2)
    d.text((104, y), "a place a merchant would know by name  (%d)" % sum(1 for p in places if p["tier"] == 2), font=font(30), fill=INK)
    y += 46
    d.ellipse([72, y + 16, 80, y + 24], fill=INK2)
    d.text((104, y), "the rest  (%d)" % sum(1 for p in places if p["tier"] == 3), font=font(30), fill=INK)

    # the panels: one group of kinds each, in one colour, so the thin ones can be seen
    side = n // 4
    small = Image.fromarray(base).resize((side, side), Image.NEAREST)
    sheet = Image.new("RGB", (n + 2 * side, n), EDGE)
    sheet.paste(big, (0, 0))
    blue = colour["town"]
    for i, (title, kinds) in enumerate(PANELS):
        pts = stones if kinds is None else [p for p in places if p["kind"] in kinds]
        im = small.copy()
        dd = ImageDraw.Draw(im)
        r = 3 if len(pts) <= 2500 else 2
        for p in pts:
            x, y = px(p, side / float(n))
            if 0 <= x < side and 0 <= y < side:
                dd.ellipse([x - r, y - r, x + r, y + r], fill=blue)
        dd.text((22, 16), title, font=font(30, True), fill=INK)
        dd.text((22, 56), "%d" % len(pts), font=font(28), fill=INK2)
        col, row = i % 2, i // 2
        sheet.paste(im.crop((0, 0, side - 2, side - 2)), (n + col * side + 2, row * side + (2 if row else 0)))
    os.makedirs(os.path.dirname(PICTURE), exist_ok=True)
    sheet.save(PICTURE, optimize=True)
    return sheet.size


# ------------------------------------------------------------------------------------------------ the README
def table(rows, head):
    out = ["| " + " | ".join(head) + " |", "|" + "|".join("---" if i == 0 else "---:" for i in range(len(head))) + "|"]
    for r in rows:
        out.append("| " + " | ".join(str(c) for c in r) + " |")
    return "\n".join(out)


def write_readme(places, stones, dropped, missing, port_table, found, asked_when, pic_size, curated_counts, domesday):
    kinds = [k for k in PRIORITY if any(p["kind"] == k for p in places)]
    by_kind = [(k, KIND_MEANS[k], sum(1 for p in places if p["kind"] == k), sum(1 for p in places if p["kind"] == k and p["tier"] == 1),
                sum(1 for p in places if p["kind"] == k and p["tier"] == 2), sum(1 for p in places if p["kind"] == k and p["tier"] == 3),
                sum(1 for p in places if p["kind"] == k and p["from"])) for k in kinds]
    by_kind.append(("**all places**", "", len(places), sum(1 for p in places if p["tier"] == 1), sum(1 for p in places if p["tier"] == 2),
                    sum(1 for p in places if p["tier"] == 3), sum(1 for p in places if p["from"])))
    stone_kinds = [(k, KIND_MEANS[k], sum(1 for p in stones if p["kind"] == k)) for k in ("runestone", "picture_stone", "runic_inscription")]
    lands = sorted(set(p["country"] for p in places + stones), key=lambda c: -sum(1 for p in places if p["country"] == c))
    by_land = []
    for c in lands:
        mine = [p for p in places if p["country"] == c]
        dated = sum(1 for p in mine if p["from"])
        by_land.append((COUNTRY.get(c, c), len(mine), sum(1 for p in mine if p["tier"] <= 2), dated, "%d%%" % round(100.0 * dated / len(mine)) if mine else "-",
                        sum(1 for p in stones if p["country"] == c)))
    queries = []
    for kind, entries in found.items():
        for a in entries:
            queries.append((kind, a["name"], len(a["rows"])))
    thin_dates = [COUNTRY.get(c, c) for c in lands if c and sum(1 for p in places if p["country"] == c) >= 25 and
                  sum(1 for p in places if p["country"] == c and p["from"]) < 0.25 * sum(1 for p in places if p["country"] == c)]
    unmatched = [t[0] for t in port_table if not t[2]]
    T = []
    T.append("# The gazetteer of 793-1066\n")
    T.append("Asked for by the user on 4 Oct 2026: \"Everything single play in the known trade routes and all settlements all ports cities all "
             "locations everything\" - read as every single place on the known trade routes, and all settlements, ports, cities and locations. "
             "The game has 50 ports (`Assets/Kapman/Game/Data/ports_793.json`). This is the rest of the world around them.\n")
    T.append("- The list: `Assets/Kapman/Game/Data/gazetteer_793.json` - **%d places** and, as a light layer of their own, **%d stones**." % (len(places), len(stones)))
    T.append("- The script: `Bake/gazetteer/make_gazetteer.py` (run it with `Bake\\run.bat gazetteer\\make_gazetteer.py`; it only asks what is not yet on disk).")
    T.append("- The raw answers: `G:\\kapman_unity\\data\\gazetteer\\raw_<kind>.json`, each with the query that was asked.")
    T.append("- The picture: `Captures/map/gazetteer.png` (%d x %d): every place as a dot over the chart's overview, and one small map for each group of kinds." % pic_size)
    T.append("- Asked on %s. Window: longitude %g to %g, latitude %g to %g. Time: alive some time between %d and %d.\n" % (asked_when, WEST, EAST, SOUTH, NORTH, YEAR_FROM, YEAR_TO))
    T.append("## The credit line\n")
    T.append("> " + CREDIT + "\n")
    T.append("Wikidata is the only source read. CC0 asks for no credit, but the project records one for every source, so this line goes in the credits. "
             "Nothing with a share-alike or non-commercial licence was used: no OpenStreetMap, no Wikipedia text, no Pleiades or OpenDomesday data.\n")
    T.append("One thing to know: three layers are found by an **id that Wikidata holds** for another list - the English vills by their OpenDomesday id, "
             "the Roman towns by a Pleiades, DARE or vici.org id, the inscriptions by a Rundata id. Only the fact that Wikidata's item carries such an id is used, "
             "and the id itself is not kept; the place, its name and its coordinates are Wikidata's. If that is too near the other lists for comfort, "
             "`--no-domesday` leaves the English vills out, and the kinds `roman_town` and `runic_inscription` can be dropped by kind.\n")
    T.append("## What a record holds\n")
    T.append("`id` (a slug; for a port, the port's id), `qid` (Wikidata), `name` (today's), `name_local` (in the land's own tongue), `lat`, `lon`, "
             "`x`, `z` (metres in the game's world frame: the Lambert conformal projection `WORLD_PROJ` of `Bake/world/make_map_art.py`; x east, z north), "
             "`kind`, `from`, `to` (years; 0 = not known), `tier`, `source`. Two more for convenience: `country` (today's, ISO code) and `also` (what else the place is).\n")
    T.append("- **Tier 1**: the 50 ports, matched by name and nearness (%d matched to a Wikidata place%s). Their `lat`/`lon` here are Wikidata's; the game keeps its own in `ports_793.json`."
             % (len(port_table) - len(unmatched), "; written in from the ports file itself: " + ", ".join(unmatched) if unmatched else ""))
    T.append("- **Tier 2**: a place a merchant would know by name - the places on the script's list of names, every emporium, burh, palace, ring fortress, longphort, "
             "mint, thing site, cathedral and bishop's seat found by class, and any other place well known today (by its count of Wikipedia articles: "
             "a town 20, a monastery or stronghold 8, a fort 10, a church or site 15, a Domesday vill 25).")
    T.append("- **Tier 3**: the rest.\n")
    T.append("## The counts by kind\n")
    T.append(table(by_kind, ["kind", "what it is", "places", "tier 1", "tier 2", "tier 3", "with a date"]) + "\n")
    T.append("The stones (their own layer, `stones` in the file, all tier 3):\n")
    T.append(table(stone_kinds, ["kind", "what it is", "stones"]) + "\n")
    T.append("## The counts by country (today's borders)\n")
    T.append(table(by_land, ["country", "places", "tier 1 and 2", "with a date", "dated", "stones"]) + "\n")
    T.append("## What is thin\n")
    T.append("Wikidata is broad and free, and it is uneven. Read the map with this in mind:\n")
    T.append("- **Dates are few.** Only %d of the %d places have a year. A place without one is in the list because of what it is (a burh, a gord, a Domesday vill), "
             "not because Wikidata says when. %s" % (sum(1 for p in places if p["from"]), len(places),
             ("Under a quarter of the places are dated in: " + ", ".join(thin_dates) + ".") if thin_dates else ""))
    T.append("- **A date is often not the beginning.** Wikidata's inception of a town is sometimes its charter, and a town whose charter is later than 1066 is left out "
             "even if it was there (Winchester, Lejre and Hedeby have no date at all). The famous ones are on the script's list of names; the small ones are missing.")
    T.append("- **England looks full, the rest looks empty.** The %d Domesday vills are one list for one kingdom. No such list exists in Wikidata for Francia, Frisia, "
             "Saxony, Denmark, Norway, Sweden, Ireland or the Slavic lands, so their villages are missing, not absent." % sum(1 for p in places if p["kind"] == "settlement"))
    T.append("- **Scandinavia, the Baltic and the east are the thinnest**: few dated towns, few monasteries (there were few), and the Viking Age sites are mostly "
             "untagged. Sweden's weight is in the stones layer.")
    T.append("- **Kinds Wikidata has no class for.** Burhs, royal vills, longphorts and emporia have almost no items of their own class (burh: 3, royal vill: 0, longphort: 0), "
             "so these kinds are mostly the script's list of names: the Burghal Hidage, the known emporia, the Norse towns of Ireland.")
    T.append("- **Mints** have no dates in Wikidata. Only mints named on the script's list of early mints are kept, and most Anglo-Saxon mints have no item.")
    T.append("- **Irish monastic sites**: only the larger houses have a founding date; the hundreds of small early churches are missing.")
    T.append("- **Thing sites and ship finds** are a handful each.")
    T.append("- **Found by words only** (`source` says \"its words\"): these were found by a phrase in a label or description (\"Slawischer Burgwall\", \"Viking Age\") "
             "and are right more often than not, but not checked one by one. They are tier 3 unless well known.")
    T.append("- **Roman towns still lived in** are living places that carry an id of the ancient world. Most were lived in through our time; a few may have lain empty.")
    T.append("- **The window is a box**, so it holds the north of Spain and Italy and the north Balkans too, which the chart does not show.\n")
    if missing:
        T.append("Names on the script's list that Wikidata has no placed item for (within 30 km): " + ", ".join(m["names"][0] for m in missing) + ".\n")
    T.append("Left out in the merge: " + "; ".join("%s: %d" % (k, v) for k, v in sorted(dropped.items(), key=lambda t: -t[1])) + ".\n")
    T.append("## How the curators fill it\n")
    T.append("The script never loses hand work, because hand work does not go in the output file. It goes in `Design/content/gazetteer/curated.json`, "
             "which the script merges last on every run:\n")
    T.append("```json\n{\n \"places\": [\n  {\"qid\": \"Q172157\", \"from\": 648, \"source\": \"wikidata; date: Anglo-Saxon Chronicle s.a. 648 (public domain)\"},\n"
             "  {\"id\": \"witla\", \"name\": \"Witla\", \"name_local\": \"Witla\", \"lat\": 51.89, \"lon\": 4.22, \"kind\": \"emporium\", \"from\": 0, \"to\": 836, \"tier\": 2,\n"
             "   \"source\": \"Annales Fuldenses s.a. 836 (public domain)\"}\n ],\n \"drop\": [\"Q12345\"]\n}\n```\n")
    T.append("- A record with a `qid` or an `id` that is already in the list **changes** that place (only the fields given). Any other record is **added**. `drop` leaves places out.")
    T.append("- Fill in this order: (1) dates and period names for tier 1 and 2; (2) the places a port's dossier names that are missing here "
             "(`Design/content/dossiers/<port>.md`); (3) the thin lands above, region by region; (4) wrong kinds and wrong hits among the places found by words.")
    T.append("- **Sources**: only ones free for commercial use, with the credit line written in `source`. Good: the period texts themselves (annals, charters, sagas, "
             "itineraries - public domain), Wikidata (CC0), national heritage registers under CC0 or CC BY. Not allowed: OpenStreetMap (share-alike), Wikipedia text "
             "(share-alike), anything non-commercial. A fact not checked gets `(?)` in its source, as in the ports file.")
    T.append("- To fix a thing at the root, fix it in Wikidata (add the inception, the class, the coordinates) and run the script with `--fresh`.")
    if curated_counts[0] or curated_counts[1]:
        T.append("- In this run `curated.json` changed %d places and added %d." % curated_counts)
    T.append("\n## The ports and their places\n")
    T.append(table([(t[0], t[1], t[2] or "-", t[3] or "-", "-" if t[4] is None else t[4]) for t in port_table], ["port", "place", "qid", "kind", "km apart"]) + "\n")
    T.append("## What was asked, and what came back\n")
    T.append(table(queries, ["raw file", "query", "rows"]) + "\n")
    T.append("Every query is in its raw file word for word. One query at a time, a second between two, waiting on a 429 or a timeout; "
             "a query still too heavy is cut into four boxes.\n")
    os.makedirs(DOCS, exist_ok=True)
    with io.open(README, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(T))


# ------------------------------------------------------------------------------------------------ main
def main():
    args = sys.argv[1:]
    fresh = set()
    if "--fresh" in args:
        i = args.index("--fresh")
        fresh = set(args[i + 1].split(",")) if i + 1 < len(args) and not args[i + 1].startswith("--") else {"all"}
    no_ask = "--no-ask" in args
    domesday = "--no-domesday" not in args
    os.makedirs(RAW, exist_ok=True)
    log("world frame: %s" % WORLD_PROJ)
    log("1. asking Wikidata, kind by kind")
    found = find(fresh, no_ask, domesday)
    named, missing = find_named(fresh, no_ask)
    log("   named: %d found, %d not in Wikidata under that name" % (len(named), len(missing)))
    qids = set(s["qid"] for s in named)
    for entries in found.values():
        for a in entries:
            for r in a["rows"]:
                if "lat" in r:
                    ok = in_box(float(r["lat"]), float(r["lon"]))
                else:
                    p = point(r.get("coord")) or point(r.get("loccoord"))
                    ok = bool(p) and in_box(p[0], p[1])
                if ok:
                    qids.add(r["item"])
    log("2. what each of the %d things is" % len(qids))
    details = find_details(qids, fresh, no_ask)
    classes = set(r["v"] for r in details["marks"] if r["k"] == "c")
    log("3. the %d classes they stand under" % len(classes))
    class_roots = find_classes(classes, fresh, no_ask)
    log("4. the merge")
    places, stones, dropped = merge(found, named, details, class_roots, domesday)
    port_table = match_ports(places)
    give_ids(places, stones)
    cur = curated(places, stones)
    write_out(places, stones)
    log("   %d places, %d stones -> %s" % (len(places), len(stones), OUT))
    for k in PRIORITY:
        c = sum(1 for p in places if p["kind"] == k)
        if c:
            log("     %-14s %6d" % (k, c))
    log("     tiers: 1: %d, 2: %d, 3: %d" % tuple(sum(1 for p in places if p["tier"] == t) for t in (1, 2, 3)))
    for t in port_table:
        log("     port %-16s -> %s" % (t[0], "%s (%s, %s, %.1f km)" % (t[1], t[2], t[3], t[4]) if t[2] else "no place in Wikidata: written in from the ports file"))
    log("   left out: " + "; ".join("%s: %d" % kv for kv in sorted(dropped.items(), key=lambda t: -t[1])))
    if missing:
        log("   names not found: " + ", ".join(m["names"][0] for m in missing))
    log("5. the picture")
    size = picture(places, stones)
    log("   %s (%d x %d)" % (PICTURE, size[0], size[1]))
    asked_when = max([a.get("asked") or "" for entries in found.values() for a in entries] or [now()])[:10]
    write_readme(places, stones, dropped, missing, port_table, found, asked_when, size, cur, domesday)
    log("6. %s" % README)
    log("queries asked in this run: %d" % _asked[0])


if __name__ == "__main__":
    main()
