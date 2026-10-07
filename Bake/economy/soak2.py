# -*- coding: utf-8 -*-
"""
MONEY THAT PAYS  (Bake/economy/soak2.py)  -  soak.py of 7 October 2026 with three things added, for
Design/fixes/money_that_pays.md. Everything below "THE MONEY, RECKONED" is soak.py's own header and still holds.
With min_margin at 0.12 and the orders off it gives soak.py's numbers to the last digit on the same seeds.

  1. min_margin  The least margin the world's traders will carry for (SimRules.minMargin, WorldSim.cs line 16) is a
                 rule here, read from the C# as it stands; soak.py had it as a constant, 0.12.   --set min_margin=0.25
  2. orders      The orders of commissions_793.json, as Commissions.Daily posts them at the twelve ports (soak.py
                 left them out): posted by the port's kind, paid at the market's price at posting times 1 + premium,
                 filled by "another merchant" four days in a hundred. The perfect and the sensible player take one at
                 the port that posts it when the boat can carry it and it should pay; they go for the goods when that
                 leg beats their trade, and deliver. (They keep orders badly: read the gain as a floor.)
                 --set orders=1   order_tolled=1   order_premium_max=0.5   order_extra_max=60   (Design/fixes/orders.md)
                 --set order_premium_all=1.5       a what-if: every posted order at the premium the cards ask
  3. the report  --config pays prints the tables of money_that_pays.md. Every summary also has the sum that is paid
                 in half the seasons (the debt to ask for), what the other traders did, and what the orders brought.

  soak2.py --config pays                       the whole report of money_that_pays.md (300 seeds, 13 to 25 minutes)
  soak2.py --seeds 100 --config pays --brief   the proposed set, one line a player
  soak2.py --seeds 100 --config approved --brief --set min_margin=0.25 --set orders=1
  configs: before (the code and data of 7 October), approved (ruling 14: the money fix; soak.py's "tuned" with the
           margin pinned at 0.12), pays (the proposal: approved, the margin 0.25, the debt 1920), tuned, data

THE MONEY, RECKONED  (Bake/economy/soak.py)

A model of one merchant with the starting boat trading among the twelve ports of the first release
through one season (1 May to Michaelmas, 29 September 793), on the game's own tables:

  Assets/Kapman/Game/Data: goods_793.json, ports_793.json, trade_rules.json, seasons_793.json,
  voyage_rules.json, ship_classes_793.json, traffic_793.json, chronicle_793.json (the famine's regions)
  G:/kapman_unity/data/baked/world/lanes.bin (the real ways by water; straight lines x 1.25 if it is missing)

What is ported from the C# as it stands (7 October 2026):
  Market.cs      price = value x made_factor x wanted_factor x season x scarcity x glut x shock; asks +10 %, pays -10 %;
                 a unit bought lowers the stock, a unit sold raises it and gluts a market that does not make the good;
                 stock, glut and shock settle a share a day; the hashed starting stock
  Deal.cs        unit by unit; the zone's toll off a sale
  Seasons.cs     the cheap and dear months blended over 20 days; the harvest, vintage and herring stock curves
  Hold.cs        room = kg x bulk; the boat's 900 kg less four 60 l water casks = 660
  Voyage.cs      four men at 0.25 d a day, 0.7 kg of food a man a day, water free
  Travel.cs      the polar, beating, rowing, sheltering in a gale; true distance over true speed (the chart is 1:60)
  TraderBrain.cs the other traders: sell near the best they believe, weigh every port in reach, softmax over the best
                 five, buy in lots of five while a margin of 12 % is left, a fifth of the silver kept back; ballast runs
  WorldSim.cs    the fleets of traffic_793.json for these twelve ports (56 of the 138), their silver, their dwell
  Shocks.cs      the great famine of 793 (Food twice as dear in the Frankish ports all season); Fairs.cs: Ripa

What is NOT the game's code and is this script's own guess (marked (?) in the report):
  the wind (one wind for the whole water, a westerly and an easterly spell, log-normal strength, a 6-hour step);
  the twelve ports as a closed world (the 56 traders trade only among them; no ship comes in from outside);
  the three players:
    perfect   sees the true price and stock of every market this hour and the true wind ahead, sails through the
              dark, wants no margin, keeps the law (the upper bound: nobody plays like this);
    sensible  sees the port he lies in, remembers what he saw, gets three talks a call (the board's word of three
              ports of his choice, as old as the ship that brought it), else what everyone knows (Lore); expects a
              price to sag as he sells; buys only what should fetch 15 % over what he pays; lies up in the dark
              near land; counts the long way home;
    careless  what everyone knows and nothing else; never looks at a glut; a day idle in every port; counts the
              short way home.
  the debt is paid when she lies at Dorestat at noon on the day with the sum in silver (the hold is sold there first).

Run (PowerShell):  & "C:\\Program Files\\QGIS 3.44.12\\bin\\python-qgis-ltr.bat" soak.py
      soak.py                                  the whole report: before, the steps, tuned, both starts (300 seeds, ~10 min)
      soak.py --seeds 60 --config before --brief            one line a player, the code as it stands
      soak.py --seeds 60 --config tuned --kinds sensible --brief --set toll.english=0.075 --set door_fee=20
      soak.py --seeds 1 --config tuned --kinds sensible --trace     one season, deal by deal
  --set keys: any key of base_rules(), dotted: toll.frankish, made.frankish_wine.neustria, value.rhenish_wine,
              start=6-21, world_from=4-1, start_cargo=rhenish_wine:2,ships_bread:2, famine_goods=wheat,rye
"""
import argparse, heapq, json, math, os, random, re, struct, sys, tempfile, time
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
DATA = os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
LANES = "G:/kapman_unity/data/baked/world/lanes.bin"

PORTS = ["dorestad", "traiectum", "witla", "walichrum", "medemelacha", "doccinga", "daventre", "quentovic",
         "lundenwic", "sarre", "fordwic", "ripa"]
SHORT = ["Dorestat", "Traiectum", "Witla", "Walichrum", "Medemelacha", "Doccinga", "Daventre", "Quentovic",
         "Lundenwic", "Sarre", "Fordwic", "Ripa"]
HOME = 0
LUNDENWIC = 8
FIRST = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334]
INF = float("inf")
NAN = float("nan")


def load(name):
    with open(os.path.join(DATA, name), encoding="utf-8-sig") as f:
        return json.load(f)


def doy(md):
    return FIRST[md[0] - 1] + md[1]


def month_of_index(i):
    m = 11
    while m > 0 and FIRST[m] > i:
        m -= 1
    return m + 1


def interp(table, x):
    ks = sorted(table)
    if x <= ks[0]:
        return table[ks[0]]
    for i in range(1, len(ks)):
        if x <= ks[i]:
            a, b = ks[i - 1], ks[i]
            return table[a] + (table[b] - table[a]) * (x - a) / (b - a)
    return table[ks[-1]]


def fnv(a, b):
    h = 2166136261
    for c in a + "|" + b:
        h = ((h ^ ord(c)) * 16777619) & 0xFFFFFFFF
    return (h & 0xFFFF) / 65535.0


# ---------------------------------------------------------------------------------------------- the rules

def sim_rule(name, default):
    """A number of SimRules as the C# has it today (Assets/Kapman/Code/Game/Sim/WorldSim.cs)."""
    try:
        with open(os.path.join(ROOT, "Assets", "Kapman", "Code", "Game", "Sim", "WorldSim.cs"), encoding="utf-8-sig") as f:
            m = re.search(r"\b%s\s*=\s*([0-9.]+)f?\s*[;,]" % name, f.read())
        return float(m.group(1)) if m else default
    except Exception:
        return default


def base_rules():
    """Every number as the code and the data have it today."""
    tr = load("trade_rules.json")
    vr = load("voyage_rules.json")
    gd = load("goods_793.json")
    m = tr["market"]
    coins = {c["id"]: c["silver_g"] / 1.7 for c in tr["coins"]}
    silver = sum(n * coins[c] for c, n in tr["start"]["coins"].items())
    water = vr["water"]["casks"] * vr["water"]["litres_per_cask"] * vr["water"]["kg_per_litre"]
    return {
        "spread": m["spread"], "stock_value": m["stock_value"], "stock_min": m["stock_min"], "stock_max": m["stock_max"],
        "regen": m["regen"], "elasticity": m["elasticity"], "price_min": m["price_min"], "price_max": m["price_max"],
        "appetite_value": m["appetite_value"], "glut_elasticity": m["glut_elasticity"], "glut_fade": m["glut_fade"],
        "unwanted": m["unwanted"], "shock_fade": m["shock_fade"], "cheap": m["cheap"], "dear": m["dear"],
        "made_factor": {str(float(k)): v for k, v in gd["made_factor"].items()},
        "wanted_factor": {str(float(k)): v for k, v in gd["wanted_factor"].items()},
        "toll": {z["id"]: z["toll"] for z in tr["zones"]},
        "men": vr["crew"]["men"], "pay": vr["crew"]["pay_per_man_day"], "food_kg": vr["rations"]["food_kg_per_man_day"],
        "debt": tr["start"]["debt"], "start_silver": round(silver, 1),
        "start_cargo": [["rhenish_wine", 4], ["quernstones", 2], ["ships_bread", 1], ["ale", 1]],   # ChartWorld.cs
        "hold": 900.0 - water,
        "start": [5, 1], "due": [9, 29],
        "famine": 1, "field": 1.0,
        "famine_goods": "food",                      # shocks_793.json "793-great-famine": "food" (the whole category) or ids
        "warmup": 0,                                 # days the other traders have been at work before day one (0: as the code)
        "world_from": None,                          # or [month, day]: the other traders start then (sets warmup from the start)
        "door_fee": 0.0, "door_days": 30.0,          # the reeve at Lundenwic (GAME_MAKER 4.1): not in the code yet
        "call_fee": 0.0,                             # a landing due at every call: not in the code
        "port_hours": {"perfect": 0.0, "sensible": 0.0, "careless": 24.0},
        "value": {}, "made": {}, "wanted": {},       # overrides by good (tuning)
        "sens_kmday": 105.0, "sens_long_kmday": 70.0, "careless_kmday": 150.0,
        "empty_km": 150.0,                           # the sensible player sails no further than this empty without a fresh word
        "safety": 1.15, "talks": 3,
        "smuggle": 0,                                # 1: the perfect player also carries what the law forbids
        # ---- soak2
        "min_margin": sim_rule("minMargin", 0.12),   # SimRules.minMargin: the world's traders buy only while this is left
        "orders": 0,                                 # 1: the orders of commissions_793.json are posted, taken and filled
        "order_premium_max": 99.0,                   # the most an order pays over the market (no cap in the code today)
        "order_extra_max": 1e9,                      # the most deniers an order pays over the market in all (no cap today)
        "order_premium_all": None,                   # a what-if: every order posted at this premium (the cards post up to 2.0)
        "order_tolled": 0,                           # 1: the zone's toll comes off an order's pay (today: none)
        "order_safety": 1.15,                        # the sensible player takes an order that should pay this over its cost
        "order_most": 2,                             # orders he holds at one time
    }


def with_overrides(R, sets):
    """sets: {"toll.frankish": 0.15, "made_factor.3.0": 0.6, "value.rhenish_wine": 50, "start": [6, 21] ...}"""
    R = json.loads(json.dumps(R))
    for k, v in sets.items():
        parts = k.split(".")
        if k == "start_cargo" and isinstance(v, str):            # rhenish_wine:2,ships_bread:2
            R[k] = [[x.split(":")[0], int(x.split(":")[1])] for x in v.split(",") if x]
            continue
        if k in ("start", "due", "world_from"):
            if isinstance(v, str):                               # 6-21
                v = [int(x) for x in v.split("-")]
        if parts[0] in ("made_factor", "wanted_factor") and len(parts) >= 2:
            R[parts[0]][str(float(".".join(parts[1:])))] = v
            continue
        node = R
        for p in parts[:-1]:
            node = node.setdefault(p, {})
        node[parts[-1]] = v
    return R


# ---------------------------------------------------------------------------------------------- the ways by water

class Lanes:
    def __init__(self, path):
        b = open(path, "rb").read()
        if b[:4] != b"KLN1":
            raise ValueError("not a lanes file")
        o = 4
        ver, n, e, nb, gc, p = struct.unpack_from("<6i", b, o)
        o += 24
        self.nx = [0.0] * n
        self.nz = [0.0] * n
        for i in range(n):
            _, x, z, _k = struct.unpack_from("<iffB", b, o)
            o += 13
            self.nx[i] = x
            self.nz[i] = z
        self.eA, self.eB, self.eK, self.eF, self.eL, self.eD, self.eP = [], [], [], [], [], [], []
        for i in range(e):
            a, bb, k, f, L, d, c = struct.unpack_from("<iiBBffi", b, o)
            o += 22
            pts = struct.unpack_from("<%df" % (2 * c), b, o)
            o += 8 * c
            self.eA.append(a); self.eB.append(bb); self.eK.append(k); self.eF.append(f)
            self.eL.append(L); self.eD.append(d); self.eP.append(pts)
        self.berths = {}
        for i in range(nb):
            ln = b[o]; o += 1
            s = b[o:o + ln].decode("utf-8"); o += ln
            self.berths[s] = struct.unpack_from("<i", b, o)[0]; o += 4
        self.adj = [[] for _ in range(n)]
        for i in range(e):
            self.adj[self.eA[i]].append(i)
            self.adj[self.eB[i]].append(i)

    def cost(self, i, prof):
        k = self.eK[i]
        per = prof["cost"][k]
        if per == INF:
            return INF
        judged = not (self.eF[i] & 1) and k not in (3, 4, 6)
        if prof.get("avoid", 0) > 0 and judged and self.eD[i] < prof["avoid"]:
            return INF
        if self.eF[i] & 4:
            per *= prof.get("dry", 2.0)
        md = prof.get("min_depth", 0)
        if md > 0 and judged and self.eD[i] < md:
            per *= 1 + (prof["shallow"] - 1) * min(1.0, (md - self.eD[i]) / md)
        return self.eL[i] * per

    def from_source(self, src, prof):
        best = {src: 0.0}
        via = {}
        h = [(0.0, src)]
        while h:
            d, u = heapq.heappop(h)
            if d > best.get(u, INF):
                continue
            for i in self.adj[u]:
                c = self.cost(i, prof)
                if c == INF:
                    continue
                v = self.eB[i] if self.eA[i] == u else self.eA[i]
                nd = d + c
                if nd < best.get(v, INF):
                    best[v] = nd
                    via[v] = (i, u)
                    heapq.heappush(h, (nd, v))
        return best, via

    def chunks(self, src, dst, via):
        """The way as stretches of about 8 km: (bearing in 5-degree steps, km, open sea or not)."""
        edges = []
        v = dst
        while v != src:
            i, u = via[v]
            edges.append((i, u))
            v = u
        edges.reverse()
        pts = []          # (x, z, sea)
        total = 0.0
        for i, u in edges:
            total += self.eL[i]
            p = self.eP[i]
            line = [(p[2 * k], p[2 * k + 1]) for k in range(len(p) // 2)]
            a = (self.nx[u], self.nz[u])
            other = self.eB[i] if self.eA[i] == u else self.eA[i]
            b = (self.nx[other], self.nz[other])
            if line:
                d0 = (line[0][0] - a[0]) ** 2 + (line[0][1] - a[1]) ** 2
                d1 = (line[-1][0] - a[0]) ** 2 + (line[-1][1] - a[1]) ** 2
                if d1 < d0:
                    line.reverse()
            line = [a] + line + [b]
            sea = self.eK[i] == 0
            for q in line:
                pts.append((q[0], q[1], sea))
        out = []
        dx = dz = ln = sealn = 0.0
        for k in range(1, len(pts)):
            sx = pts[k][0] - pts[k - 1][0]
            sz = pts[k][1] - pts[k - 1][1]
            seg = math.hypot(sx, sz)
            if seg <= 0:
                continue
            dx += sx; dz += sz; ln += seg
            if pts[k][2]:
                sealn += seg
            if ln >= 8000.0:
                out.append([dx, dz, ln, sealn])
                dx = dz = ln = sealn = 0.0
        if ln > 0:
            out.append([dx, dz, ln, sealn])
        drawn = sum(c[2] for c in out) or 1.0
        scale = total / drawn
        res = []
        for dx, dz, ln, sealn in out:
            bearing = (math.degrees(math.atan2(dx, dz)) + 360.0) % 360.0
            res.append([int(round(bearing / 5.0)) % 72, ln * scale / 1000.0, 1 if sealn > 0.5 * ln else 0])
        return total / 1000.0, res


PROFILES = {
    # Course.Profile: the player's line (off the flats where there is a way, over them where there is none; a river
    # whose water is not in the 3D world yet only when there is no other way: the chart sails them)
    "player": [{"cost": [1, 1, 1.05, 1.1, INF, INF, INF, 1], "dry": 6.0, "min_depth": 2.0, "shallow": 8.0, "avoid": 1.5},
               {"cost": [1, 1, 1.05, 1.1, INF, 1.5, INF, 1], "dry": 6.0, "min_depth": 2.0, "shallow": 8.0},
               {"cost": [1, 1, 1.05, 1.1, 1.3, 1.5, INF, 1], "dry": 6.0, "min_depth": 2.0, "shallow": 8.0}],
    # LaneRouter.Profile.For: the world's ships
    "sea": [{"cost": [1, 1, 1.05, 1.1, 1.3, 1.5, 4, 1], "dry": 2.0}],
    "coast": [{"cost": [INF, 1, 1.05, 1.1, 1.3, 1.5, 4, 1], "dry": 2.0}],
    "river": [{"cost": [INF, 8, 1.05, 1.1, 1.3, 8, 4, 1], "dry": 2.0}],
}


def haversine(a, b):
    la1, lo1, la2, lo2 = map(math.radians, (a["lat"], a["lon"], b["lat"], b["lon"]))
    h = math.sin((la2 - la1) / 2) ** 2 + math.cos(la1) * math.cos(la2) * math.sin((lo2 - lo1) / 2) ** 2
    return 6371.0 * 2 * math.asin(math.sqrt(h))


_ROUTES = None


def routes(ports):
    """{profile: {(i, j): (km, chunks)}}, kept in the temp folder between runs."""
    global _ROUTES
    if _ROUTES is not None:
        return _ROUTES
    stamp = os.path.getmtime(LANES) if os.path.exists(LANES) else 0
    cache = os.path.join(tempfile.gettempdir(), "kapman_soak_routes_v3.json")
    if os.path.exists(cache):
        try:
            with open(cache) as f:
                d = json.load(f)
            if d.get("stamp") == stamp:
                _ROUTES = {p: {tuple(map(int, k.split(","))): (v[0], [tuple(c) for c in v[1]]) for k, v in t.items()}
                           for p, t in d["routes"].items()}
                return _ROUTES
        except Exception:
            pass
    out = {p: {} for p in PROFILES}
    if stamp:
        g = Lanes(LANES)
        for name, tries in PROFILES.items():
            for i, pid in enumerate(PORTS):
                for prof in tries:
                    if all((i, j) in out[name] for j in range(len(PORTS)) if j != i):
                        break
                    best, via = g.from_source(g.berths[pid], prof)
                    for j, qid in enumerate(PORTS):
                        if j == i or (i, j) in out[name] or g.berths[qid] not in best:
                            continue
                        out[name][(i, j)] = g.chunks(g.berths[pid], g.berths[qid], via)
    else:                                   # StraightRoutes: the line x 1.25
        for name in PROFILES:
            for i, a in enumerate(ports):
                for j, b in enumerate(ports):
                    if i == j:
                        continue
                    km = haversine(a, b) * 1.25
                    y = math.sin(math.radians(b["lon"] - a["lon"])) * math.cos(math.radians(b["lat"]))
                    x = math.cos(math.radians(a["lat"])) * math.sin(math.radians(b["lat"])) - \
                        math.sin(math.radians(a["lat"])) * math.cos(math.radians(b["lat"])) * math.cos(math.radians(b["lon"] - a["lon"]))
                    bearing = (math.degrees(math.atan2(y, x)) + 360) % 360
                    n = max(1, int(km / 10))
                    out[name][(i, j)] = (km, [(int(round(bearing / 5)) % 72, km / n, 1)] * n)
    try:
        with open(cache, "w") as f:
            json.dump({"stamp": stamp, "routes": {p: {"%d,%d" % k: v for k, v in t.items()} for p, t in out.items()}}, f)
    except Exception:
        pass
    _ROUTES = out
    return out


# ---------------------------------------------------------------------------------------------- the tables

class Tables:
    pass


def build(R):
    T = Tables()
    T.R = R
    gd = load("goods_793.json")
    tr = load("trade_rules.json")
    se = load("seasons_793.json")
    pt = load("ports_793.json")
    year = 793
    goods = [g for g in gd["goods"] if (not g.get("from") or g["from"] <= year) and (not g.get("to") or g["to"] >= year)]
    G = len(goods)
    T.G = G
    T.goods = goods
    T.gid = [g["id"] for g in goods]
    T.gi = {g["id"]: i for i, g in enumerate(goods)}
    pm = {p["id"]: p for p in pt["ports"]}
    T.ports = [pm[i] for i in PORTS]
    zone_of = {}
    for z in tr["zones"]:
        for r in z["regions"]:
            zone_of[r] = z["id"]
    T.zone = [zone_of[p["region"]] for p in T.ports]
    T.toll = [R["toll"][z] for z in T.zone]
    value = np.array([float(R["value"].get(g["id"], g["value"])) for g in goods])
    T.value = value
    T.space = np.array([g["kg"] * max(0.1, g["bulk"]) for g in goods])
    T.space_l = T.space.tolist()
    T.keeps = np.array([float(g.get("keeps", 0) or 0) for g in goods])
    if R["famine_goods"] == "food":
        T.food = np.array([g.get("cat", "").lower() == "food" for g in goods])
    else:
        ids = set(R["famine_goods"].split(",")) if isinstance(R["famine_goods"], str) else set(R["famine_goods"])
        T.food = np.array([g["id"] in ids for g in goods])
    T.bread = T.gi["ships_bread"]
    laws = {}
    for l in tr["laws"]:
        if (not l.get("from") or l["from"] <= year) and (not l.get("to") or l["to"] >= year):
            for z in l["zones"]:
                laws.setdefault(l["law"], set()).add(z)
    T.lawful = np.ones((len(PORTS), G), dtype=bool)
    for pi in range(len(PORTS)):
        for gi, g in enumerate(goods):
            if g.get("law") and T.zone[pi] in laws.get(g["law"], ()):
                T.lawful[pi, gi] = False
    T.base = np.zeros((len(PORTS), G))
    T.usual = np.zeros((len(PORTS), G))
    T.app = np.ones((len(PORTS), G))
    T.made = np.zeros((len(PORTS), G))
    T.wanted = np.zeros((len(PORTS), G))
    T.stock0 = np.zeros((len(PORTS), G))
    MF = {float(k): v for k, v in R["made_factor"].items()}
    WF = {float(k): v for k, v in R["wanted_factor"].items()}
    for pi, p in enumerate(T.ports):
        reg = p["region"]
        size = max(1, int(p["size"]))
        for gi, g in enumerate(goods):
            made = dict(g.get("made") or {})
            made.update(R["made"].get(g["id"], {}))
            wanted = dict(g.get("wanted") or {})
            wanted.update(R["wanted"].get(g["id"], {}))
            md = float(made.get(reg, 0) or 0)
            wt = float(wanted.get(reg, 0) or 0)
            b = value[gi] * (interp(MF, md) if md > 0 else 1.0) * (interp(WF, wt) if wt > 0 else 1.0)
            worth = max(0.5, value[gi])
            us = min(max(size * md * R["stock_value"] / worth, R["stock_min"]), R["stock_max"]) if md > 0 else 0.0
            if us <= 0 and md <= 0 and wt <= 0:
                b *= R["unwanted"]
            T.base[pi, gi] = b
            T.usual[pi, gi] = us
            T.app[pi, gi] = max(1.0, size * (wt if wt > 0 else 0.5) * R["appetite_value"] / worth)
            T.made[pi, gi] = md
            T.wanted[pi, gi] = wt
            T.stock0[pi, gi] = round(us * (0.75 + 0.5 * fnv(p["id"], g["id"]))) if us > 0 else 0.0
    T.gluton = (T.wanted <= 0) | (T.usual <= 0)          # Market.GotOne: a unit sold here gluts it from the first
    # the season's price by day, and the month as a whole (Lore)
    edge = float(se.get("edge_days", 20))
    SP = np.ones((G, 365))
    T.SM = np.ones((13, G))
    for gi, g in enumerate(goods):
        cheap, dear = g.get("cheap") or [], g.get("dear") or []
        if not cheap and not dear:
            continue
        month = [1.0] * 13
        for m in range(1, 13):
            month[m] = R["cheap"] if m in cheap else R["dear"] if m in dear else 1.0
            T.SM[m, gi] = month[m]
        t = [month[month_of_index(d)] for d in range(365)]
        half = edge * 0.5
        reach = int(math.ceil(half))
        for m in range(1, 13):
            a, b = month[12 if m == 1 else m - 1], month[m]
            if abs(a - b) < 1e-6:
                continue
            e = FIRST[m - 1]
            for k in range(-reach, reach):
                x = min(1.0, max(0.0, (k + 0.5 + half) / edge))
                t[(e + k + 365) % 365] = a + (b - a) * (1 - math.cos(math.pi * x)) * 0.5
        SP[gi] = t
    T.SPt = np.ascontiguousarray(SP.T)
    # the seasons of making
    MK = np.ones((G, 365))
    smooth = int(se.get("smooth_days", 21))
    for c in se.get("make", []):
        pts = []
        for key, v in c["points"].items():
            mm, dd = key.split("-")
            pts.append((min(364, FIRST[int(mm) - 1] + int(dd) - 1), max(0.0, float(v))))
        pts.sort()
        n = len(pts)
        raw = [0.0] * 365
        for d in range(365):
            after = 0
            while after < n and pts[after][0] <= d:
                after += 1
            a = n - 1 if after == 0 else after - 1
            b = 0 if after == n else after
            da = pts[a][0] - (365 if after == 0 else 0)
            db = pts[b][0] + (365 if after == n else 0)
            tt = (d - da) / (db - da) if db > da else 0.0
            raw[d] = pts[a][1] + (pts[b][1] - pts[a][1]) * tt
        reach = max(0, int(c.get("smooth_days", smooth)) // 2)
        sm = [sum(raw[(d + k) % 365] for k in range(-reach, reach + 1)) / (2 * reach + 1) for d in range(365)]
        for gid in c["goods"]:
            if gid in T.gi:
                MK[T.gi[gid]] = sm
    T.MKt = np.ascontiguousarray(MK.T)
    T.month_of = [month_of_index(i) for i in range(365)]
    # the famine of 793: Food twice as dear in the regions the chronicle names, held all season (Shocks.cs)
    T.famine = [False] * len(PORTS)
    if R["famine"]:
        regs = None
        try:
            for e in load("chronicle_793.json")["entries"]:
                if e.get("id") == "793-great-famine":
                    regs = set(e.get("regions") or [])
        except Exception:
            regs = None
        if regs is None:
            regs = {"rhine_delta", "zeeland", "frisia", "neustria"}
        T.famine = [p["region"] in regs for p in T.ports]
    T.held = np.zeros((len(PORTS), G))
    for pi in range(len(PORTS)):
        if T.famine[pi]:
            T.held[pi, T.food] = 1.0
    # what every merchant knows (Lore): the usual price by month, and this year's famine with it for the player
    sp = R["spread"]
    T.UPpays = [[None] * 13 for _ in PORTS]
    T.UPasks = [[None] * 13 for _ in PORTS]
    T.LOpays = [[None] * 13 for _ in PORTS]
    T.LOasks = [[None] * 13 for _ in PORTS]
    for pi in range(len(PORTS)):
        for m in range(1, 13):
            up = np.maximum(0.05, T.base[pi] * T.SM[m])
            T.UPpays[pi][m] = up * (1 - sp / 2)
            T.UPasks[pi][m] = np.where(T.usual[pi] >= 1, up * (1 + sp / 2), NAN)
            T.LOpays[pi][m] = T.UPpays[pi][m] * (1 + T.held[pi])
            T.LOasks[pi][m] = T.UPasks[pi][m] * (1 + T.held[pi])
    # the ways, the craft, the fleets
    T.routes = routes(T.ports)
    T.pkm = [[(T.routes["player"][(i, j)][0] if i != j else 0.0) for j in range(len(PORTS))] for i in range(len(PORTS))]
    sc = load("ship_classes_793.json")
    T.cls = {}
    for c in sc["classes"]:
        waters = c.get("waters") or []
        T.cls[c["id"]] = {
            "id": c["id"], "cargo": float(c.get("cargo_t") or 0) * 1000.0, "cargo_t": float(c.get("cargo_t") or 0),
            "crew": int(c.get("crew") or 1), "sail": c.get("speed_sail_kn") is not None and (c.get("polar_factor") or 0) > 0,
            "sail_kn": float(c.get("speed_sail_kn") or 0), "max_kn": float(c.get("speed_max_kn") or 0),
            "oars_kn": float(c.get("speed_oars_kn") or 0), "row_h": float(c.get("rowing_hours") or 0),
            "closest": float(c.get("closest_to_wind_deg") or 180), "polar": float(c.get("polar_factor") or 0),
            "profile": "sea" if "sea" in waters else "coast" if "coast" in waters else "river",
            "long": "trade_long" in (c.get("uses") or []), "small": float(c.get("cargo_t") or 0) <= 2.5,
        }
    tf = load("traffic_793.json")["long_distance"]["ports"]
    T.fleets = [(PORTS.index(k), int(v.get("traders", 0)), v.get("classes", {})) for k, v in tf.items()
                if k in PORTS and isinstance(v, dict) and v.get("traders", 0) > 0]
    T.line_km = [[haversine(a, b) for b in T.ports] for a in T.ports]
    T._lore_q = {}
    # the orders (Commissions.cs): by the kind of port
    T.order_kinds, T.order_fill = {}, 0.04
    try:
        cm = load("commissions_793.json")
        T.order_fill = float(cm.get("ai_fill_per_day", 0.04))
        for kind, v in cm.get("by_kind", {}).items():
            ids = list(v.get("goods", {}).keys())            # the order of the file, as Json.Obj keeps it
            T.order_kinds[kind] = {"chance": float(v.get("chance_per_month", 0)), "premium": float(v.get("premium", 0.3)),
                                   "days": float(v.get("days", 40)), "ids": ids, "goods": v.get("goods", {})}
    except Exception:
        pass
    T.port_kind = [p.get("kind") or "" for p in T.ports]
    return T


_TABLES = {}


def tables(sets):
    key = json.dumps(sets, sort_keys=True)
    if key not in _TABLES:
        _TABLES[key] = build(with_overrides(base_rules(), sets))
    return _TABLES[key]


# ---------------------------------------------------------------------------------------------- a market

class Market:
    def __init__(self, T, pi, hashed=True):
        R = T.R
        self.T = T
        self.pi = pi
        self.base = T.base[pi]
        self.usual = T.usual[pi]
        self.usual_l = T.usual[pi].tolist()
        self.app = T.app[pi]
        self.app_l = T.app[pi].tolist()
        self.base_l = T.base[pi].tolist()
        self.gluton_l = T.gluton[pi].tolist()
        self.makes = self.usual > 0
        self.stock = (T.stock0[pi] if hashed else T.usual[pi]).copy()
        self.glut = np.zeros(T.G)
        self.dearer = np.zeros(T.G)
        self.held = T.held[pi]
        self.has_held = bool(self.held.any())
        self.el, self.pmin, self.pmax = R["elasticity"], R["price_min"], R["price_max"]
        self.ge, self.sp = R["glut_elasticity"], R["spread"]
        self.regen, self.gfade, self.sfade = R["regen"], R["glut_fade"], R["shock_fade"]
        if self.has_held:                                    # the backstory replayed at the start (Shocks.Apply)
            if hashed:
                self.stock[T.food] *= 0.55
            self.dearer = np.maximum(self.dearer, self.held)

    def settle(self, di):
        """One day: the shock and the glut fade, the stock settles toward the day's usual; a held shock stays."""
        self.dearer *= (1 - self.sfade)
        self.glut *= (1 - self.gfade)
        self.stock += (self.usual * self.T.MKt[di] - self.stock) * self.regen
        if self.has_held:
            np.maximum(self.dearer, self.held, out=self.dearer)

    def pricevec(self, di):
        scar = np.ones(self.T.G)
        m = self.makes
        scar[m] = np.clip((self.usual[m] / np.maximum(self.stock[m], 0.5)) ** self.el, self.pmin, self.pmax)
        return np.maximum(0.05, self.base * self.T.SPt[di] * scar * (1 + self.glut / self.app) ** (-self.ge) * (1 + self.dearer))

    def price1(self, g, di):
        p = self.base_l[g] * self.T.SPt[di, g]
        u = self.usual_l[g]
        if u > 0:
            f = (u / max(self.stock[g], 0.5)) ** self.el
            p *= self.pmin if f < self.pmin else self.pmax if f > self.pmax else f
        p *= (1 + self.glut[g] / self.app_l[g]) ** (-self.ge) * (1 + self.dearer[g])
        return p if p > 0.05 else 0.05

    def asks1(self, g, di):
        return self.price1(g, di) * (1 + self.sp / 2)

    def pays1(self, g, di):
        return self.price1(g, di) * (1 - self.sp / 2)

    def buy(self, g, n, cash, di):
        """Buys up to n units while there is stock and silver: (units, deniers). Each makes the next dearer."""
        st = float(self.stock[g])
        u = self.usual_l[g]
        k = self.base_l[g] * self.T.SPt[di, g] * (1 + self.glut[g] / self.app_l[g]) ** (-self.ge) * (1 + self.dearer[g]) * (1 + self.sp / 2)
        total = 0.0
        got = 0
        while got < n and st - got >= 1.0:
            f = 1.0
            if u > 0:
                f = (u / max(st - got, 0.5)) ** self.el
                f = self.pmin if f < self.pmin else self.pmax if f > self.pmax else f
            each = max(0.05, k * f)
            if total + each > cash + 1e-6:
                break
            total += each
            got += 1
        self.stock[g] = st - got
        return got, total

    def sell(self, g, n, di, commit=True):
        """What the market pays for n units, before the toll. Each makes the next cheaper."""
        st = float(self.stock[g])
        gl = float(self.glut[g])
        u = self.usual_l[g]
        ap = self.app_l[g]
        on = self.gluton_l[g]
        k = self.base_l[g] * self.T.SPt[di, g] * (1 + self.dearer[g]) * (1 - self.sp / 2)
        total = 0.0
        for _ in range(n):
            f = 1.0
            if u > 0:
                f = (u / max(st, 0.5)) ** self.el
                f = self.pmin if f < self.pmin else self.pmax if f > self.pmax else f
            total += max(0.05, k * f * (1 + gl / ap) ** (-self.ge))
            st += 1.0
            if on or gl > 0:
                gl += 1.0
        if commit:
            self.stock[g] = st
            self.glut[g] = gl
        return total


# ---------------------------------------------------------------------------------------------- the world

AI_ARRIVE, AI_READY, P_ARRIVE, P_READY = 0, 1, 2, 3
# the world traders' numbers (WorldSim.cs, SimRules)
BELIEF_HALF = 30.0
KEEP_BACK = 0.2
MIN_MARGIN = 0.12
LOT_CAP = 60
STOCK_SHARE = 0.5
LOT_STEP = 5
SELL_NEAR_BEST = 0.85
KEEP_MAN_DAY = 0.3
CONSIDER_TOP = 5
TEMPERATURE = 0.25
WAGE_MAN_DAY = 0.25
SILVER_BASE, SILVER_TONNE = 60.0, 50.0
REACH = {"river": 260.0, "coast": 450.0, "long": 1500.0}
WIND_MEDIAN = {4: 6.0, 5: 5.6, 6: 5.2, 7: 5.2, 8: 5.5, 9: 6.3, 10: 7.0, 11: 7.5}       # m/s (?): this script's guess
PLAYER_CLASS = "gislinge"


class Ship:
    __slots__ = ("i", "cls", "home", "at", "dest", "silver", "hold", "cap", "owed", "tlast", "knows", "reach", "rng")


class Player:
    pass


class World:
    def __init__(self, T, seed, kind):
        R = T.R
        self.T, self.R, self.kind, self.seed = T, R, kind, seed
        self.warm = int(R["warmup"])
        if R["world_from"]:
            self.warm = max(0, doy(R["start"]) - doy(R["world_from"]))
        self.season = doy(R["due"]) - doy(R["start"])
        self.start_doy = doy(R["start"]) - self.warm            # the world's day 0 (the player's is day `warm`)
        self.D = self.warm + self.season
        self.tend = self.D + 0.5                                 # noon on the day
        self.weather(random.Random(seed * 7919 + 101))
        self.mk = [Market(T, i) for i in range(len(PORTS))]
        self.board = [dict() for _ in PORTS]
        self.ev = []
        self.seq = 0
        self.day_done = 0
        self.rng = random.Random(seed * 104729 + 7)
        self.mm = float(R["min_margin"])
        self.orders = []                                         # open orders: dicts (port, g, n, pay, deadline, mine)
        self.orders_posted = 0
        self.ai = [0, 0, 0.0, 0]                                 # sailings, waits for want of a cargo, silver laid out, laden sailings
        self.first = None                                        # the watched prices on the player's first morning
        self.orng = random.Random(seed * 911 + 3)
        self.ships = []
        self.watch = None
        self.pl = None
        self.trace = None
        self.fleet()
        if kind != "none":
            self.player()

    def di(self, d):
        return (self.start_doy - 1 + d) % 365

    # ---- the wind and the light (this script's own: the game's weather model is not ported)
    def weather(self, r):
        nd = self.D + 45
        nb = nd * 4
        wdir = np.zeros(nb)
        wspd = np.zeros(nb)
        west = r.random() < 0.625
        dev = 0.0
        x = 0.0
        for b in range(nb):
            if west and r.random() < 1 / 20.0:
                west = False
            elif not west and r.random() < 1 / 12.0:
                west = True
            centre, sd = (245.0, 35.0) if west else (50.0, 45.0)
            dev = 0.8 * dev + r.gauss(0, sd * 0.6)
            wdir[b] = (centre + dev) % 360.0
            x = 0.85 * x + r.gauss(0, 0.45 * math.sqrt(1 - 0.85 ** 2))
            month = self.T.month_of[self.di(b // 4)]
            wspd[b] = WIND_MEDIAN.get(month, 6.0) * math.exp(x)
        self.wdir, self.wspd = wdir, wspd
        course = np.arange(72) * 5.0
        twa = np.abs(((course[None, :] - wdir[:, None]) % 360.0 + 540.0) % 360.0 - 180.0)
        self.SPD = {}
        for cid, c in self.T.cls.items():
            oars = c["oars_kn"] * min(1.0, c["row_h"] / 12.0)
            if c["sail"]:
                through = c["sail_kn"] * c["polar"] * (wspd / 8.0) ** 0.6
                cl = c["closest"]
                pol = np.where(twa < 90.0, 0.72 + 0.28 * (twa - cl) / max(1.0, 90.0 - cl),
                               np.where(twa < 135.0, 1.0 + 0.12 * (twa - 90.0) / 45.0, 1.12 - 0.3 * (twa - 135.0) / 45.0))
                pol = np.where(twa < cl, 0.72 * math.cos(math.radians(cl)), pol)
                sail = np.minimum(through[:, None] * pol, c["max_kn"])
                sail = np.where(wspd[:, None] > 1.5, sail, 0.0)
                v = np.maximum(sail, oars)
            else:
                v = np.full((nb, 72), oars)
            v = np.where(wspd[:, None] > 17.0, 0.0, v) * 1.852            # a gale: she shelters; km an hour
            self.SPD[cid] = v.tolist()
        self.dawn, self.dusk = [], []
        lat = math.radians(52.0)
        for d in range(nd + 2):
            n = self.start_doy + d + 4                                   # the Julian calendar ran four days behind the sun in 793
            decl = math.radians(23.44) * math.sin(2 * math.pi * (284 + n) / 365.0)
            c = (math.sin(math.radians(-6.0)) - math.sin(lat) * math.sin(decl)) / (math.cos(lat) * math.cos(decl))
            h = math.degrees(math.acos(max(-1.0, min(1.0, c)))) / 15.0
            self.dawn.append(12.0 - h)
            self.dusk.append(12.0 + h)

    def sail(self, chunks, t, spd, liesup):
        """The hour she comes in: stretch by stretch at Travel.Speed in the wind of the hour; lying up in the dark
        near land when she does that; sheltering in a gale."""
        dawn, dusk = self.dawn, self.dusk
        last = len(spd) - 1
        for bb, km, sea in chunks:
            rem = km
            while rem > 1e-6:
                day = int(t)
                lim = INF
                if liesup and not sea:
                    hod = (t - day) * 24.0
                    if hod < dawn[day]:
                        t = day + dawn[day] / 24.0 + 1e-6
                        continue
                    if hod >= dusk[day]:
                        t = day + 1 + dawn[day + 1] / 24.0 + 1e-6
                        continue
                    lim = day + dusk[day] / 24.0
                blk = int(t * 4.0)
                bend = (blk + 1) / 4.0
                v = spd[blk if blk < last else last][bb]
                end = bend if bend < lim else lim
                if v <= 0.0:
                    t = end + 1e-6
                    continue
                can = v * (end - t) * 24.0
                if can >= rem:
                    t += rem / (v * 24.0)
                    rem = 0.0
                else:
                    rem -= can
                    t = end + 1e-6
        return t

    # ---- the clock
    def push(self, t, typ, obj):
        self.seq += 1
        heapq.heappush(self.ev, (t, self.seq, typ, obj))

    def advance(self, t):
        while self.day_done < int(t):
            self.day_done += 1
            di = self.di(self.day_done)
            for m in self.mk:
                m.settle(di)
            if di + 1 == 145:                                    # the summer market at Ripa (fairs_793.json)
                m = self.mk[PORTS.index("ripa")]
                m.stock *= 1.4
                np.minimum(m.dearer, -0.05, out=m.dearer)
            if self.R["orders"]:
                self.orders_daily(self.day_done, di)
            if self.watch is not None:
                self.watch_day(di)
            if self.pl is not None:
                self.p_worth(self.day_done)

    # ---- the orders (Commissions.Daily, for the twelve ports)
    def orders_daily(self, d, di):
        T, R, r = self.T, self.R, self.orng
        keep = []
        for o in self.orders:
            if o["mine"]:
                if d > o["deadline"]:
                    self.pl.orders_broken += 1
                    self.pl.orders.remove(o)
                else:
                    keep.append(o)
            elif not (d > o["deadline"] or r.random() < T.order_fill):
                keep.append(o)
        self.orders = keep
        for p in range(len(PORTS)):
            kind = T.order_kinds.get(T.port_kind[p])
            if not kind or not kind["ids"]:
                continue
            if r.random() >= kind["chance"] / 30.0 or any(o["port"] == p and not o["mine"] for o in self.orders):
                continue
            gid = kind["ids"][r.randrange(len(kind["ids"]))]
            if gid not in T.gi:
                continue
            g = T.gi[gid]
            lo, hi = kind["goods"][gid][0], kind["goods"][gid][1]
            n = r.randint(int(lo), int(hi))
            pays_ = self.mk[p].pays1(g, di)
            prem = kind["premium"] if R["order_premium_all"] is None else float(R["order_premium_all"])
            each = pays_ * (1.0 + min(prem, R["order_premium_max"], R["order_extra_max"] / max(0.01, pays_ * n)))
            self.orders.append({"port": p, "g": g, "n": n, "pay": float(round(each * n)), "deadline": d + kind["days"],
                                "mine": False, "posted": d})
            if d > self.warm:
                self.orders_posted += 1

    def order_cost(self, S, g, n, di):
        """What n units ask at port S now, unit by unit (nothing is bought); None when there are not so many."""
        m = self.mk[S]
        st = float(m.stock[g])
        if st - n < 0.0 or not self.T.lawful[S, g]:
            return None
        u = m.usual_l[g]
        k = m.base_l[g] * self.T.SPt[di, g] * (1 + m.glut[g] / m.app_l[g]) ** (-m.ge) * (1 + m.dearer[g]) * (1 + m.sp / 2)
        total = 0.0
        for i in range(n):
            f = 1.0
            if u > 0:
                f = (u / max(st - i, 0.5)) ** m.el
                f = m.pmin if f < m.pmin else m.pmax if f > m.pmax else f
            total += max(0.05, k * f)
        return total

    def order_source(self, o, S, t, di, month):
        """What the player reckons the goods of order o cost at port S (None: not to be had there, as far as he knows)."""
        pl, T = self.pl, self.T
        g, n = o["g"], o["n"]
        if pl.kind == "perfect":
            return self.order_cost(S, g, n, di)
        if T.usual[S, g] < n + 1 or not T.lawful[S, g]:
            return None
        a_ = float(self.believed_asks(pl.knows, S, t, month, lore=True)[g])
        if a_ != a_:
            return None
        return n * a_ * (1.0 + 0.4 * n / (2.0 * T.usual[S, g]))      # the price rises as he buys

    def p_orders(self, t, di):
        """In port: deliver what was ordered here, then look at what is wanted here and take it or not."""
        pl, T, R = self.pl, self.T, self.R
        if not R["orders"] or pl.kind == "careless":
            return
        P = pl.at
        m = self.mk[P]
        for o in list(pl.orders):
            g, n = o["g"], o["n"]
            if o["port"] != P or t > o["deadline"] or g not in pl.hold or pl.hold[g][0] < n:
                continue
            lot = pl.hold[g]
            cost = lot[1] * n / lot[0]
            toll = o["pay"] * T.toll[P] if R["order_tolled"] else 0.0
            plain = m.sell(g, n, di, commit=False) * (1 - T.toll[P])
            pl.silver += o["pay"] - toll
            pl.tolls += toll
            gain = o["pay"] - toll - cost
            pl.gain += gain
            pl.order_gain += gain
            pl.order_extra += o["pay"] - toll - plain
            pl.order_pay += o["pay"] - toll
            pl.orders_done += 1
            if self.trace is not None:
                self.trace.append("    ORDER delivered: %d %s for %.0f, toll %.0f, cost %.0f: %+.0f (the market would have paid %.0f)"
                                  % (n, T.gid[g], o["pay"], toll, cost, gain, plain))
            lot[0] -= n
            lot[1] -= cost
            if lot[0] <= 0:
                del pl.hold[g]
            pl.orders.remove(o)
            self.orders.remove(o)
        if len(pl.orders) >= int(R["order_most"]):
            return
        month = T.month_of[di]
        wage = R["men"] * R["pay"]
        left = self.tend - t
        for o in self.orders:
            if o["mine"] or o["port"] != P or len(pl.orders) >= int(R["order_most"]):
                continue
            g, n = o["g"], o["n"]
            if o.get("seen") is None:
                o["seen"] = 1
                pl.orders_seen += 1
                if n * T.space_l[g] > pl.cap - 60.0:
                    pl.orders_toobig += 1
            if n * T.space_l[g] > pl.cap - 60.0:                 # more than she carries with two sacks of bread aboard
                continue
            net = o["pay"] * (1 - (T.toll[P] if R["order_tolled"] else 0.0))
            ok = False
            for S in range(len(PORTS)):
                if S == P:
                    continue
                cost = self.order_source(o, S, t, di, month)
                if cost is None:
                    continue
                trip = (T.pkm[P][S] + T.pkm[S][P]) / R["sens_long_kmday"] + 1.0
                if t + trip > o["deadline"] or trip > left - (T.pkm[P][HOME] / R["sens_long_kmday"] + 1.0):
                    continue
                if net >= cost * R["order_safety"] and net - cost > wage * trip * 0.5:
                    ok = True
                    pl.order_hope += net - cost
                    break
            if ok:
                o["mine"] = True
                pl.orders.append(o)
                pl.orders_taken += 1
                if self.trace is not None:
                    self.trace.append("    ORDER taken at %s: %d %s for %.0f by day %.0f" % (SHORT[P], n, T.gid[g], o["pay"], o["deadline"] - self.warm))

    def run(self):
        while self.ev:
            t, _, typ, obj = heapq.heappop(self.ev)
            if t > self.tend:
                break
            self.advance(t)
            if typ == AI_ARRIVE:
                self.ai_arrive(obj, t)
            elif typ == AI_READY:
                self.ai_ready(obj, t)
            elif typ == P_ARRIVE:
                self.p_arrive(t)
            else:
                self.p_ready(t)
        self.advance(self.tend)
        return self.result()

    # ---- what is known: (day, pays, asks) of a market as someone saw it
    def snapshot(self, p, t, di):
        pv = self.mk[p].pricevec(di)
        sp = self.R["spread"]
        return (t, pv * (1 - sp / 2), np.where(self.mk[p].stock >= 1, pv * (1 + sp / 2), NAN))

    def swap(self, knows, p):
        b = self.board[p]
        for q, sn in list(knows.items()):
            if q not in b or b[q][0] < sn[0]:
                b[q] = sn
        for q, sn in b.items():
            if q not in knows or knows[q][0] < sn[0]:
                knows[q] = sn

    def believed_pays(self, knows, q, t, month, lore=False):
        usual = (self.T.LOpays if lore else self.T.UPpays)[q][month]
        sn = knows.get(q)
        if sn is None:
            return usual
        w = 0.5 ** (max(0.0, t - sn[0]) / BELIEF_HALF)
        return w * sn[1] + (1 - w) * usual

    def believed_asks(self, knows, q, t, month, lore=False):
        usual = (self.T.LOasks if lore else self.T.UPasks)[q][month]
        sn = knows.get(q)
        if sn is None:
            return usual
        w = 0.5 ** (max(0.0, t - sn[0]) / BELIEF_HALF)
        seen = sn[2]
        if lore and t - sn[0] <= 10.0:                           # the player: what he saw or heard lately stands as it was
            return seen
        mix = w * seen + (1 - w) * usual
        return np.where(np.isnan(seen), usual, np.where(np.isnan(usual), seen if w >= 0.5 else NAN, mix))

    # ---- the other traders
    def fleet(self):
        T, R = self.T, self.R
        n = 0
        for home, traders, classes in T.fleets:
            count = traders * R["field"]
            count = int(count) + (1 if self.rng.random() < count - int(count) else 0)
            ids = sorted(classes)
            for _ in range(count):
                r = self.rng.random() * sum(classes.values())
                cid = ids[-1]
                for c in ids:
                    r -= classes[c]
                    if r <= 0:
                        cid = c
                        break
                c = T.cls[cid]
                s = Ship()
                s.i = n
                n += 1
                s.cls = c
                s.home = s.at = home
                s.dest = home
                s.silver = SILVER_BASE + SILVER_TONNE * c["cargo_t"]
                s.hold = {}
                s.cap = c["cargo"]
                s.owed = 0.0
                s.tlast = 0.0
                s.knows = {}
                s.rng = random.Random(self.seed * 1009 + s.i * 31 + 5)
                reach = REACH["long"] if c["long"] else REACH["coast"] if c["profile"] != "river" else REACH["river"]
                rt = T.routes[c["profile"]]
                near = [q for q in range(len(PORTS)) if q == home or (T.line_km[home][q] <= reach and (home, q) in rt and (q, home) in rt)]
                s.reach = [q for q in near if all((a, q) in rt for a in near if a != q)]
                self.ships.append(s)
                self.push(s.rng.uniform(0.0, 3.0), AI_READY, s)

    def ai_wages(self, s, t):
        s.owed += s.cls["crew"] * WAGE_MAN_DAY * (t - s.tlast)
        s.tlast = t

    def ai_arrive(self, s, t):
        T = self.T
        self.ai_wages(s, t)
        s.at = s.dest
        di = self.di(int(t))
        month = T.month_of[di]
        s.knows[s.at] = self.snapshot(s.at, t, di)
        self.swap(s.knows, s.at)
        self.ai_sell(s, t, di, month, False)
        pay = min(s.owed, s.silver)
        s.silver -= pay
        s.owed -= pay
        if s.owed > 0.01 and s.hold:
            self.ai_sell(s, t, di, month, True)
            pay = min(s.owed, s.silver)
            s.silver -= pay
            s.owed -= pay
        self.push(t + s.rng.uniform(0.5, 2.0), AI_READY, s)

    def ai_sell(self, s, t, di, month, must):
        if not s.hold:
            return
        T = self.T
        m = self.mk[s.at]
        toll = T.toll[s.at]
        bestv = None
        if not must:
            for q in s.reach:
                if q == s.at:
                    continue
                v = self.believed_pays(s.knows, q, t, month) * (1 - T.toll[q])
                bestv = v if bestv is None else np.maximum(bestv, v)
        for g in sorted(s.hold):
            if not T.lawful[s.at, g]:
                continue
            n, cost = s.hold[g]
            each = cost / n
            best = float(bestv[g]) if bestv is not None else 0.0
            left = n
            while left > 0:
                if must:
                    if s.silver >= s.owed:
                        break
                else:
                    net = m.pays1(g, di) * (1 - toll)
                    if not (net >= SELL_NEAR_BEST * best or (net >= each and s.at == s.home) or net >= each * 1.15):
                        break
                k = min(LOT_STEP, left)
                s.silver += m.sell(g, k, di) * (1 - toll)
                left -= k
            if left <= 0:
                del s.hold[g]
            else:
                s.hold[g] = [left, cost * left / n]

    def ai_ready(self, s, t):
        T = self.T
        self.ai_wages(s, t)
        di = self.di(int(t))
        month = T.month_of[di]
        dest = self.ai_plan(s, t, di, month, False)
        if dest < 0 and s.at != s.home and s.rng.random() < 0.3:
            dest = self.ai_plan(s, t, di, month, True)
        if dest < 0:
            wait = 3.0 * (1.0 + s.rng.random()) if s.at == s.home else 1.0 + 2.0 * s.rng.random()
            if t >= self.warm:
                self.ai[1] += 1
            self.push(t + wait, AI_READY, s)
            return
        if t >= self.warm:
            self.ai[0] += 1
            self.ai[3] += 1 if s.hold else 0
        c = s.cls
        km, chunks = T.routes[c["profile"]][(s.at, dest)]
        s.dest = dest
        self.push(self.sail(chunks, t, self.SPD[c["id"]], c["small"]), AI_ARRIVE, s)

    def ai_days(self, s, a, b):
        c = s.cls
        knots = max(2.0, c["sail_kn"] * 0.65) if c["sail"] else max(1.0, c["oars_kn"] * 0.6)
        return self.T.routes[c["profile"]][(a, b)][0] / (knots * 1.852 * 24.0)

    def ai_plan(self, s, t, di, month, go_home):
        T = self.T
        m = self.mk[s.at]
        a = s.at
        crew = s.cls["crew"]
        spend = max(0.0, s.silver * (1 - KEEP_BACK) - s.owed - 14.0 * crew * WAGE_MAN_DAY)
        pv = m.pricevec(di)
        ask = pv * (1 + m.sp / 2)
        pays_here = pv * (1 - m.sp / 2) * (1 - T.toll[a])
        onoffer = (m.stock >= 1) & T.lawful[a]
        used = sum(n * T.space_l[g] for g, (n, _c) in s.hold.items())
        free = s.cap - used
        opts = []
        plans = {}
        bels = {}
        for p in s.reach:
            if p == a or (go_home and p != s.home):
                continue
            days = self.ai_days(s, a, p)
            bel = self.believed_pays(s.knows, p, t, month) * (1 - T.toll[p])
            profit = 0.0
            for g, (n, _c) in s.hold.items():
                if not T.lawful[p, g]:
                    continue
                here = pays_here[g] if T.lawful[a, g] else 0.0
                if bel[g] > here:
                    profit += n * (bel[g] - here) * 0.8
            margin = bel - ask
            ok = onoffer & T.lawful[p] & (margin > self.mm * ask) & ((T.keeps <= 0) | (days <= T.keeps * 0.8))
            idx = np.nonzero(ok)[0]
            plan = []
            if len(idx):
                order = idx[np.argsort(-(margin[idx] / T.space[idx]), kind="stable")]
                room, cash = free, spend
                for g in order.tolist():
                    a_ = float(ask[g])
                    units = min(int(room // T.space_l[g]), int(cash // max(0.01, a_)), int(m.stock[g] * STOCK_SHARE), LOT_CAP)
                    if units <= 0:
                        continue
                    plan.append((g, units))
                    profit += units * float(margin[g]) * 0.8
                    room -= units * T.space_l[g]
                    cash -= units * a_
            score = profit - days * crew * KEEP_MAN_DAY
            opts.append((score, p))
            plans[p] = plan
            bels[p] = bel
        if not opts:
            return -1
        opts.sort(key=lambda o: (-o[0], o[1]))
        if go_home:
            choice = s.home
        else:
            if opts[0][0] <= 0.0:
                look = not s.hold and (a == s.home or s.rng.random() < 0.25)
                return self.ai_ballast(s, t, month, spend) if look else -1
            n = min(CONSIDER_TOP, len(opts))
            temp = max(1.0, TEMPERATURE * opts[0][0])
            w = [math.exp((opts[i][0] - opts[0][0]) / temp) if opts[i][0] > 0 else 0.0 for i in range(n)]
            r = s.rng.random() * sum(w)
            choice = opts[0][1]
            for i in range(n):
                r -= w[i]
                if r <= 0:
                    choice = opts[i][1]
                    break
        bel = bels[choice]
        for g, units in plans[choice]:
            sells = float(bel[g])
            bought = 0
            while bought < units and m.asks1(g, di) < sells * (1 - self.mm):
                want = min(LOT_STEP, units - bought)
                k, cost = m.buy(g, want, s.silver, di)
                if k == 0:
                    break
                s.silver -= cost
                if t >= self.warm:
                    self.ai[2] += cost
                lot = s.hold.setdefault(g, [0, 0.0])
                lot[0] += k
                lot[1] += cost
                bought += k
                if k < want:
                    break
        return choice

    def ai_ballast(self, s, t, month, spend):
        T = self.T
        best, best_score = -1, 0.0
        crew = s.cls["crew"]
        for p in s.reach:
            if p == s.at:
                continue
            back = s.home if s.home != p else s.at
            go, ret = self.ai_days(s, s.at, p), self.ai_days(s, p, back)
            ask = self.believed_asks(s.knows, p, t, month)
            with np.errstate(invalid="ignore"):
                margin = self.believed_pays(s.knows, back, t, month) * (1 - T.toll[back]) - ask
                ok = T.lawful[p] & T.lawful[back] & (margin > self.mm * ask) & ((T.keeps <= 0) | (ret <= T.keeps * 0.8))
            idx = np.nonzero(ok)[0]
            if not len(idx):
                continue
            order = idx[np.argsort(-(margin[idx] / T.space[idx]), kind="stable")]
            profit, room, cash = 0.0, s.cap, spend
            for g in order.tolist():
                a_ = float(ask[g])
                units = min(int(room // T.space_l[g]), int(cash // max(0.01, a_)), LOT_CAP)
                if units <= 0:
                    continue
                profit += units * float(margin[g]) * 0.6
                room -= units * T.space_l[g]
                cash -= units * a_
            score = profit - (go + ret) * crew * KEEP_MAN_DAY
            if score > best_score:
                best, best_score = p, score
        return best

    # ---- a day's prices, for the report (a run with no player)
    WATCH = [("rhenish_wine", 0, 8), ("rhenish_wine", 0, 11), ("frankish_wine", 7, 8), ("frisian_cloth", 4, 7),
             ("glass_vessels", 7, 8), ("honey", 8, 4), ("wheat", 8, 5), ("cheese", 4, 7), ("bar_iron", 0, 4),
             ("books", 7, 8), ("wool_cloth", 8, 0), ("quernstones", 0, 8), ("rhenish_wine", 0, 9), ("frankish_wine", 7, 9)]

    def watch_day(self, di):
        T = self.T
        if self.first is None and self.day_done >= max(1, self.warm):
            self.first = [(self.mk[a].asks1(T.gi[gid], di), self.mk[b].pays1(T.gi[gid], di) * (1 - T.toll[b])) for gid, a, b in self.WATCH]
        for k, (gid, a, b) in enumerate(self.WATCH):
            g = T.gi[gid]
            self.watch[k][0] += self.mk[a].asks1(g, di)
            self.watch[k][1] += self.mk[b].pays1(g, di) * (1 - T.toll[b])
            self.watch[k][2] += 1 if self.mk[a].stock[g] >= 1 else 0
            self.watch[k][3] += 1

    # ---- the player
    def player(self):
        T, R = self.T, self.R
        pl = Player()
        self.pl = pl
        pl.kind = self.kind
        pl.at = HOME
        pl.dest = HOME
        pl.silver = float(R["start_silver"])
        pl.cap = float(R["hold"])
        pl.hold = {}                     # good -> [units, cost, bought at]
        pl.sacks = 0
        pl.open_kg = 0.0
        pl.owed = 0.0
        pl.tlast = self.warm + 0.25
        pl.knows = {}
        pl.liesup = pl.kind != "perfect"
        pl.lawful = not (pl.kind == "perfect" and R["smuggle"])
        pl.rng = random.Random(self.seed * 2003 + 11)
        pl.gain = pl.wages = pl.bread = pl.tolls = pl.fees = 0.0
        pl.hungry = 0.0
        pl.legs = []
        pl.leg = None
        pl.gain_good = {}
        pl.gain_route = {}
        pl.worth_day = [0.0] * (self.season + 1)
        pl.first_clear = None
        pl.door_paid = -INF
        pl.sea_days = 0.0
        pl.km = 0.0
        pl.late = False
        pl.idle = 0
        pl.prev = -1
        pl.worth0 = None
        pl.orders = []
        pl.orders_seen = pl.orders_toobig = pl.orders_taken = pl.orders_done = pl.orders_broken = 0
        pl.order_gain = pl.order_extra = pl.order_pay = pl.order_hope = 0.0
        self.push(self.warm + 0.25, P_READY, None)                       # six in the morning of the first day

    def p_begin(self, t):
        """The first morning: what is aboard (ChartWorld.cs), reckoned at what it would fetch at home today."""
        pl, T, R = self.pl, self.T, self.R
        room = pl.cap
        di = self.di(int(t))
        m = self.mk[HOME]
        for gid, n in R["start_cargo"]:
            g = T.gi[gid]
            take = min(int(n), int(room // T.space_l[g] + 1e-6))
            if take <= 0:
                continue
            room -= take * T.space_l[g]
            if g == T.bread:
                pl.sacks += take
            else:
                pl.hold[g] = [take, m.sell(g, take, di, commit=False) * (1 - T.toll[HOME]), HOME]
        pl.worth0 = pl.silver + sum(v[1] for v in pl.hold.values())
        pl.worth_day[0] = pl.worth0
        pl.knows[HOME] = self.snapshot(HOME, t, di)
        if pl.kind == "sensible":
            self.p_talks(t, di)

    def p_worth(self, d):
        pl = self.pl
        d -= self.warm
        if 0 < d <= self.season and pl.worth0 is not None:
            w = pl.silver - pl.owed + sum(v[1] for v in pl.hold.values())
            pl.worth_day[d] = w
            if pl.first_clear is None and w >= self.R["debt"]:
                pl.first_clear = d

    def p_tick(self, t):
        pl, R = self.pl, self.R
        dt = t - pl.tlast
        if dt <= 0:
            return
        pl.tlast = t
        w = R["men"] * R["pay"] * dt
        pl.owed += w
        pl.wages += w
        need = R["men"] * R["food_kg"] * dt
        while need > 1e-9:
            if pl.open_kg <= 1e-9:
                if pl.sacks <= 0:
                    pl.hungry += need / (R["men"] * R["food_kg"])
                    break
                pl.sacks -= 1
                pl.open_kg = 20.0
            eat = min(need, pl.open_kg)
            pl.open_kg -= eat
            need -= eat

    def p_sell(self, g, n, m, di):
        pl, T = self.pl, self.T
        lot = pl.hold[g]
        n = min(n, lot[0])
        gross = m.sell(g, n, di)
        toll = gross * T.toll[pl.at]
        cost = lot[1] * n / lot[0]
        pl.silver += gross - toll
        pl.tolls += toll
        gain = gross - toll - cost
        if self.trace is not None:
            self.trace.append("    sold %d %s for %.0f, toll %.0f, cost %.0f: %+.0f" % (n, T.gid[g], gross, toll, cost, gain))
        pl.gain += gain
        pl.gain_good[g] = pl.gain_good.get(g, 0.0) + gain
        key = (lot[2], pl.at)
        pl.gain_route[key] = pl.gain_route.get(key, 0.0) + gain
        lot[0] -= n
        lot[1] -= cost
        if lot[0] <= 0:
            del pl.hold[g]

    def p_arrive(self, t):
        pl, T, R = self.pl, self.T, self.R
        self.p_tick(t)
        pl.at = pl.dest
        di = self.di(int(t))
        pay = min(pl.owed, pl.silver)
        pl.silver -= pay
        pl.owed -= pay
        fee = R["call_fee"]
        if pl.at == LUNDENWIC and R["door_fee"] > 0 and t - pl.door_paid > R["door_days"]:
            fee += R["door_fee"]
            pl.door_paid = t
        fee = min(fee, pl.silver)
        pl.silver -= fee
        pl.fees += fee
        pl.knows[pl.at] = self.snapshot(pl.at, t, di)
        if pl.kind == "sensible":
            self.p_talks(t, di)
        self.p_orders(t, di)
        if pl.kind == "careless":
            m = self.mk[pl.at]
            for g in list(pl.hold):
                self.p_sell(g, pl.hold[g][0], m, di)
        self.push(t + R["port_hours"][pl.kind] / 24.0, P_READY, None)

    def p_talks(self, t, di, n=None):
        """Three talks a call (one more each dawn she lies there): the board's word of the ports that by what everyone
        knows pay best from here, where the board is at least a day fresher than what he holds."""
        pl, T = self.pl, self.T
        month = T.month_of[di]
        b = self.board[pl.at]
        q = self.lore_onward(month)
        rank = sorted((p for p in b if p != pl.at), key=lambda p: -q["pair"][pl.at][p])
        left = int(self.R["talks"]) if n is None else n
        for p in rank:
            if left <= 0:
                break
            if p not in pl.knows or pl.knows[p][0] < b[p][0] - 1.0:
                pl.knows[p] = b[p]
                left -= 1

    def lore_onward(self, month):
        """What everyone knows of the best next leg from each port (static by month): for the sensible player."""
        T = self.T
        if month not in T._lore_q:
            n = len(PORTS)
            T._lore_q[month] = self.onward([T.LOasks[p][month] for p in range(n)],
                                           [T.LOpays[p][month] * (1 - T.toll[p]) for p in range(n)],
                                           [T.usual[p] * 0.5 for p in range(n)], INF, 0.5, self.R["sens_kmday"], T.lawful)
        return T._lore_q[month]

    def onward(self, asks, paysnet, avail, cash, trust, kmday, lawful):
        """A quick guess of the best leg from each port: {q: [gain], d: [days], pair: [[gain]]}."""
        T, R = self.T, self.R
        n = len(PORTS)
        wage = R["men"] * R["pay"]
        Q, D = [0.0] * n, [1.0] * n
        pair = [[0.0] * n for _ in range(n)]
        for b in range(n):
            best_rate = 0.0
            for c in range(n):
                if c == b:
                    continue
                with np.errstate(invalid="ignore"):
                    mg = paysnet[c] - asks[b]
                    ok = (avail[b] >= 1) & lawful[b] & lawful[c] & (mg > 0)
                ok[T.bread] = False
                idx = np.nonzero(ok)[0]
                if not len(idx):
                    continue
                order = idx[np.argsort(-(mg[idx] / T.space[idx]))][:6]
                room, csh, pr = R["hold"], cash, 0.0
                for g in order.tolist():
                    a_ = float(asks[b][g])
                    u = min(room // T.space_l[g], float(avail[b][g]), float(T.app[c, g]), csh // a_)
                    if u <= 0:
                        continue
                    pr += u * float(mg[g]) * trust
                    room -= u * T.space_l[g]
                    csh -= u * a_
                d = T.pkm[b][c] / kmday
                v = pr - wage * d
                pair[b][c] = v
                if v / d > best_rate:
                    best_rate, Q[b], D[b] = v / d, v, d
        return {"q": Q, "d": D, "pair": pair}

    def bread_need(self, days, dest):
        """Sacks to buy before this leg, by the player's habit."""
        pl, R, T = self.pl, self.R, self.T
        have = (pl.sacks * 20.0 + pl.open_kg) / (R["men"] * R["food_kg"])
        if pl.kind == "careless":
            return 1 if have < 2.0 else 0
        want = days + (1.0 if pl.kind == "perfect" else 5.0)
        if T.usual[dest, T.bread] < 1:
            want += 5.0
        return max(0, int(math.ceil((want - have) * R["men"] * R["food_kg"] / 20.0)))

    def fill(self, cands, ask, sale, kept, cash, room, safety, cap, lawful_b):
        """The hold that pays best: unit by unit, the dearest gain for its room first (and, tried too, for its silver)."""
        space = self.T.space_l
        best_plan, best_profit = {}, 0.0
        for mode in (0, 1):
            heap = []
            c, r, profit, plan = cash, room, 0.0, {}
            for g in cands:
                if not lawful_b[g] or space[g] > r:
                    continue
                a0 = ask(g, 0)
                if a0 is None or a0 > c:
                    continue
                s0 = sale(g, kept.get(g, 0))
                if s0 < a0 * safety:
                    continue
                mg = s0 - a0
                heap.append((-(mg / space[g]) if mode == 0 else -(mg / a0), g, 0, a0, s0))
            heapq.heapify(heap)
            while heap:
                _, g, k, a_, s_ = heapq.heappop(heap)
                if a_ > c or space[g] > r:
                    continue
                c -= a_
                r -= space[g]
                profit += s_ - a_
                plan[g] = k + 1
                k += 1
                if k >= cap[g]:
                    continue
                a2 = ask(g, k)
                if a2 is None:
                    continue
                s2 = sale(g, kept.get(g, 0) + k)
                if s2 < a2 * safety:
                    continue
                mg = s2 - a2
                heapq.heappush(heap, (-(mg / space[g]) if mode == 0 else -(mg / a2), g, k, a2, s2))
            if profit > best_profit:
                best_profit, best_plan = profit, plan
        return best_plan, best_profit

    def decide(self, t):
        """Where next, what to sell here and what to buy.
        perfect:  the true price and stock of every market this hour, the true wind ahead, sails through the dark;
        sensible: his own eyes, three talks a call, else what everyone knows; expects a price to sag as he sells; wants
                  15 % over what he pays; counts the long way home;
        careless: what everyone knows and nothing else; does not think of the glut; counts the short way home."""
        pl, T, R = self.pl, self.T, self.R
        perfect = pl.kind == "perfect"
        careless = pl.kind == "careless"
        a = pl.at
        Ma = self.mk[a]
        di = self.di(int(t))
        month = T.month_of[di]
        sp, el, ge = R["spread"], R["elasticity"], R["glut_elasticity"]
        pmin, pmax = R["price_min"], R["price_max"]
        left = self.tend - t
        wage = R["men"] * R["pay"]
        n = len(PORTS)
        all_ok = [True] * T.G
        lawful_a = T.lawful[a].tolist() if pl.lawful else all_ok
        stock_a = Ma.stock.tolist()
        usual_a = Ma.usual_l
        ka = (Ma.base * T.SPt[di] * (1 + Ma.dearer) * (1 + Ma.glut / Ma.app) ** (-ge) * (1 + sp / 2)).tolist()

        def ask(g, k):
            st = stock_a[g] - k
            if st < 1.0:
                return None
            u = usual_a[g]
            if u > 0:
                f = (u / max(st, 0.5)) ** el
                return ka[g] * (pmin if f < pmin else pmax if f > pmax else f)
            return ka[g]

        vhere = {g: Ma.sell(g, lot[0], di, commit=False) * (1 - T.toll[a]) for g, lot in pl.hold.items()}
        cands = [g for g in range(T.G) if stock_a[g] >= 1.0 and lawful_a[g] and g != T.bread]
        if perfect:
            pvs = [m.pricevec(di) for m in self.mk]
            on = self.onward([pv * (1 + sp / 2) for pv in pvs], [pvs[p] * (1 - sp / 2) * (1 - T.toll[p]) for p in range(n)],
                             [m.stock * 0.5 for m in self.mk], max(50.0, pl.silver + sum(vhere.values())), 0.7, 150.0,
                             T.lawful if pl.lawful else np.ones_like(T.lawful))
            spd = self.SPD[PLAYER_CLASS]
        elif careless:
            on = self.lore_onward(month)
        else:                                                    # from what he has seen and heard, else what everyone knows
            asks_b = [np.where(Ma.stock >= 1, Ma.pricevec(di) * (1 + sp / 2), NAN) if p == a else
                      self.believed_asks(pl.knows, p, t, month, lore=True) for p in range(n)]
            on = self.onward(asks_b, [self.believed_pays(pl.knows, p, t, month, lore=True) * (1 - T.toll[p]) for p in range(n)],
                             [T.usual[p] * 0.5 + (Ma.stock if p == a else 0.0) for p in range(n)],
                             max(50.0, pl.silver + sum(vhere.values())), 0.5, R["sens_kmday"], T.lawful)
        best = None
        back_empty = None
        home_opt = None
        sensible = pl.kind == "sensible"
        feasible_any = False
        for b in range(n):
            if b == a:
                continue
            km = T.pkm[a][b]
            chunks = T.routes["player"][(a, b)][1]
            if perfect:
                eta = self.sail(chunks, t, spd, False)
                days = eta - t
                back = eta if b == HOME else self.sail(T.routes["player"][(b, HOME)][1], eta, spd, False)
                if back > self.tend:
                    continue
            elif careless:
                days = km / R["careless_kmday"]
                if (km + (T.pkm[b][HOME] if b != HOME else 0.0)) / R["careless_kmday"] > left:
                    continue
            else:
                days = km / R["sens_kmday"] + 0.2
                need = (km + (T.pkm[b][HOME] if b != HOME else 0.0)) / R["sens_long_kmday"] + (1.0 if b != HOME else 0.5)
                if need > left:
                    continue
            feasible_any = True
            Mb = self.mk[b]
            toll_b = T.toll[b]
            app_b = Mb.app_l
            if perfect:
                kb = (Mb.base * T.SPt[di] * (1 + Mb.dearer) * (1 - sp / 2) * (1 - toll_b)).tolist()
                stock_b, glut_b, usual_b, gluton_b = Mb.stock.tolist(), Mb.glut.tolist(), Mb.usual_l, Mb.gluton_l

                def sale(g, k, kb=kb, stock_b=stock_b, glut_b=glut_b, usual_b=usual_b, gluton_b=gluton_b, app_b=app_b):
                    u = usual_b[g]
                    f = 1.0
                    if u > 0:
                        f = (u / max(stock_b[g] + k, 0.5)) ** el
                        f = pmin if f < pmin else pmax if f > pmax else f
                    gl = glut_b[g] + (k if (gluton_b[g] or glut_b[g] > 0) else 0)
                    return kb[g] * f * (1 + gl / app_b[g]) ** (-ge)
                cap = [10 ** 9] * T.G
                safety = 1.0
            elif careless:
                bp = (T.LOpays[b][month] * (1 - toll_b)).tolist()

                def sale(g, k, bp=bp):
                    return bp[g]
                cap = [10 ** 9] * T.G
                safety = 1.0
            else:
                bp = (self.believed_pays(pl.knows, b, t, month, lore=True) * (1 - toll_b)).tolist()

                def sale(g, k, bp=bp, app_b=app_b):
                    return bp[g] * (1 + k / app_b[g]) ** (-ge)
                cap = [int(math.ceil(x)) for x in app_b]
                safety = R["safety"]
            lawful_b = T.lawful[b].tolist() if pl.lawful else all_ok
            od = {}
            for o in pl.orders:                                  # an order held for b: its goods fetch the order's price there
                if o["port"] != b or t + days > o["deadline"]:
                    continue
                g_, n_ = o["g"], o["n"]
                have = pl.hold[g_][0] if g_ in pl.hold else 0
                if have >= n_ or (lawful_a[g_] and stock_a[g_] - (n_ - have) >= 1.0):
                    od[g_] = (n_, o["pay"] * (1 - (toll_b if R["order_tolled"] else 0.0)) / n_)
            if od:
                def sale(g, k, plain=sale, od=od):
                    x = od.get(g)
                    if x is None:
                        return plain(g, k)
                    return x[1] if k < x[0] else plain(g, k - x[0])
                cap = list(cap)
                for g_ in od:
                    cap[g_] += od[g_][0]
            sacks = self.bread_need(days, b)
            fees = (R["door_fee"] if b == LUNDENWIC and t + days - pl.door_paid > R["door_days"] else 0.0) + R["call_fee"]
            cash = pl.silver - 3.5 * sacks - fees
            room = pl.cap - (pl.sacks + sacks) * 30.0
            kept = {}
            carry = 0.0
            for g, lot in pl.hold.items():
                vb = sum(sale(g, k) for k in range(lot[0])) if lawful_b[g] else 0.0
                if vb > vhere[g] * (1.0 if perfect else 1.1):
                    kept[g] = lot[0]
                    room -= lot[0] * T.space_l[g]
                    carry += vb - vhere[g]
                else:
                    cash += vhere[g]
            plan, profit = self.fill(cands, ask, sale, kept, max(0.0, cash), room, safety, cap, lawful_b)
            net = profit + carry - (wage + 0.4) * days - fees
            score = (net + on["q"][b]) / (days + on["d"][b])
            pulled = False
            for o in pl.orders:                                  # an order held for elsewhere: b has its goods
                P_ = o["port"]
                if P_ == b or (o["g"] in pl.hold and pl.hold[o["g"]][0] >= o["n"]):
                    continue
                dP = T.pkm[b][P_] / (150.0 if perfect else R["sens_kmday"]) + 0.2
                if t + days + dP > o["deadline"] or dP > left - days:
                    continue
                cost_ = self.order_source(o, b, t, di, month)
                if cost_ is None or cost_ > pl.silver + sum(vhere.values()):
                    continue
                qo = o["pay"] * (1 - (T.toll[P_] if R["order_tolled"] else 0.0)) - cost_ - wage * dP
                if qo > 0 and (net + qo) / (days + dP) > score:
                    score = (net + qo) / (days + dP)
                    pulled = True
            if b == HOME:
                home_opt = (score, b, kept, plan, net, days)
            if not pulled and sensible and not plan and not kept and km > R["empty_km"] and (b not in pl.knows or t - pl.knows[b][0] > 10.0):
                continue                                         # not far and empty on what everyone knows alone
            if not pulled and not plan and not kept and b == pl.prev:           # not straight back, empty, to the port she just left
                back_empty = (score, b, kept, plan, net, days)   # (unless there is nowhere else: the way home)
                continue
            if best is None or score > best[0]:
                best = (score, b, kept, plan, net, days)
        if best is None and back_empty is not None and a != HOME and not sensible:
            best = back_empty
        if best is None and sensible and a != HOME and feasible_any:
            if T.pkm[a][HOME] / R["sens_long_kmday"] + 1.5 < left - 1.0:
                return None, {}, [], 0.0                         # he lies here another day
            best = home_opt if home_opt is not None else back_empty
        if best is None:
            if a != HOME and not feasible_any:
                pl.late = True                                   # too far to be home on the day: she goes anyway
                return HOME, {g: lot[0] for g, lot in pl.hold.items()}, [], T.pkm[a][HOME] / R["sens_kmday"]
            return None, {g: lot[0] for g, lot in pl.hold.items()}, [], 0.0
        if sensible and best[0] <= 0.0 and a != HOME:
            # nothing worth casting off for today: he lies here another day, unless the long way home says go now
            if T.pkm[a][HOME] / R["sens_long_kmday"] + 1.5 < left - 1.0:
                return None, {}, [], 0.0
            if home_opt is not None:
                best = home_opt
        score, b, kept, plan, net, days = best
        if score <= 0.0 and a == HOME:
            return None, {}, [], 0.0
        sells = {g: lot[0] for g, lot in pl.hold.items() if g not in kept}
        return b, sells, sorted(plan.items()), days

    def p_ready(self, t):
        pl, T, R = self.pl, self.T, self.R
        if pl.worth0 is None:
            self.p_begin(t)
        self.p_tick(t)
        m = self.mk[pl.at]
        di = self.di(int(t))
        dest, sells, buys, days = self.decide(t)
        for g, k in sells.items():
            self.p_sell(g, k, m, di)
        if pl.leg is not None:                                   # the leg is reckoned from cast off to cast off
            leg = pl.leg
            profit = (pl.gain - leg["gain0"]) - (pl.wages - leg["wages0"]) - (pl.bread - leg["bread0"]) - (pl.fees - leg["fees0"])
            pl.legs.append((leg["frm"], leg["to"], leg["days"], profit, leg["km"], leg["laden"]))
            pl.leg = None
        if dest is None:
            if pl.kind == "sensible":                            # a day in port: he sees the market again, and one talk
                pl.knows[pl.at] = self.snapshot(pl.at, t, di)
                self.p_talks(t, di, 1)
                self.p_orders(t, di)
                pl.idle += 1
            self.push(t + 1.0, P_READY, None)
            return
        for _ in range(self.bread_need(days, dest)):
            if (pl.cap - self.p_used()) < 30.0:
                break
            k, cost = m.buy(T.bread, 1, pl.silver, di)
            if k == 0:
                break
            pl.sacks += 1
            pl.silver -= cost
            pl.bread += cost
        for g, units in buys:
            room = int((pl.cap - self.p_used()) // T.space_l[g] + 1e-6)
            k, cost = m.buy(g, min(units, room), pl.silver, di)
            if k > 0:
                if self.trace is not None:
                    self.trace.append("    bought %d %s for %.0f" % (k, T.gid[g], cost))
                pl.silver -= cost
                lot = pl.hold.setdefault(g, [0, 0.0, pl.at])
                lot[2] = pl.at
                lot[0] += k
                lot[1] += cost
        km, chunks = T.routes["player"][(pl.at, dest)]
        arr = self.sail(chunks, t, self.SPD[PLAYER_CLASS], pl.liesup)
        if self.trace is not None:
            self.trace.append("day %6.2f  %s to %s, %.0f km, in on day %.2f; silver %.0f, bread %d sacks" % (t - self.warm, SHORT[pl.at], SHORT[dest], km, arr - self.warm, pl.silver, pl.sacks))
        pl.leg = {"frm": pl.at, "to": dest, "days": arr - t, "km": km, "gain0": pl.gain, "wages0": pl.wages,
                  "bread0": pl.bread, "fees0": pl.fees, "laden": 1 if pl.hold else 0}
        pl.sea_days += min(arr, self.tend) - t
        pl.km += km
        pl.prev = pl.at
        pl.dest = dest
        pl.at = -1
        self.push(arr, P_ARRIVE, None)

    def p_used(self):
        pl = self.pl
        return pl.sacks * 30.0 + sum(lot[0] * self.T.space_l[g] for g, lot in pl.hold.items())

    # ---- the reckoning of a run
    def result(self):
        fleet = [s.silver - s.owed + sum(v[1] for v in s.hold.values()) for s in self.ships]
        ai = self.ai + [len(self.ships), mean(fleet) if fleet else 0.0, mean([SILVER_BASE + SILVER_TONNE * s.cls["cargo_t"] for s in self.ships]) if fleet else 0.0]
        if self.pl is None:
            return {"kind": "none", "watch": self.watch, "first": self.first, "ai": ai}
        pl, T, R = self.pl, self.T, self.R
        home = pl.at == HOME
        self.p_tick(self.tend)
        if home:
            m = self.mk[HOME]
            di = self.di(self.D)
            for g in list(pl.hold):
                self.p_sell(g, pl.hold[g][0], m, di)
            pay = min(pl.owed, pl.silver)
            pl.silver -= pay
            pl.owed -= pay
            if pl.leg is not None:
                leg = pl.leg
                profit = (pl.gain - leg["gain0"]) - (pl.wages - leg["wages0"]) - (pl.bread - leg["bread0"]) - (pl.fees - leg["fees0"])
                pl.legs.append((leg["frm"], leg["to"], leg["days"], profit, leg["km"], leg["laden"]))
        worth_end = pl.silver - pl.owed + sum(v[1] for v in pl.hold.values())
        return {
            "kind": pl.kind, "seed": self.seed, "home": home, "paid": bool(home and pl.silver >= R["debt"]),
            "silver": pl.silver, "worth_end": worth_end, "worth0": pl.worth0, "legs": pl.legs,
            "gain_good": {T.gid[g]: v for g, v in pl.gain_good.items()},
            "gain_route": {"%d>%d" % k: v for k, v in pl.gain_route.items()},
            "gain": pl.gain, "wages": pl.wages, "tolls": pl.tolls, "bread": pl.bread, "fees": pl.fees,
            "hungry": pl.hungry, "worth_day": pl.worth_day, "first_clear": pl.first_clear,
            "sea_days": pl.sea_days, "km": pl.km, "days": self.season, "idle": pl.idle,
            "orders": [pl.orders_seen, pl.orders_toobig, pl.orders_taken, pl.orders_done, pl.orders_broken + len(pl.orders),
                       pl.order_gain, pl.order_extra, pl.order_pay, self.orders_posted, pl.order_hope],
            "ai": ai,
        }


def run_one(job):
    sets, seed, kind = job[:3]
    T = tables(sets)
    w = World(T, seed, kind)
    if len(job) > 3:
        w.trace = []
        r = w.run()
        r["trace"] = w.trace
        return r
    if kind == "none":
        w.watch = [[0.0, 0.0, 0, 0] for _ in World.WATCH]
    return w.run()


# ---------------------------------------------------------------------------------------------- the report

def q(xs, p):
    if not xs:
        return float("nan")
    s = sorted(xs)
    k = (len(s) - 1) * p
    lo = int(math.floor(k))
    hi = min(len(s) - 1, lo + 1)
    return s[lo] + (s[hi] - s[lo]) * (k - lo)


def mean(xs):
    return sum(xs) / len(xs) if xs else float("nan")


def summarise(rs, R):
    n = len(rs)
    D = rs[0]["days"]
    legs = [l for r in rs for l in r["legs"]]
    out = {
        "n": n, "paid": 100.0 * sum(r["paid"] for r in rs) / n, "home": 100.0 * sum(r["home"] for r in rs) / n,
        "silver": [q([r["silver"] for r in rs], p) for p in (0.1, 0.5, 0.9)],
        "sum": [q([r["silver"] if r["home"] else 0.0 for r in rs], p) for p in (0.2, 0.5, 0.8)],
        "made": [q([r["worth_end"] - r["worth0"] for r in rs], p) for p in (0.1, 0.5, 0.9)],
        "per_day": mean([(r["worth_end"] - r["worth0"]) / D for r in rs]),
        "legs": mean([len(r["legs"]) for r in rs]),
        "leg": [mean([l[3] for l in legs]), q([l[3] for l in legs], 0.5), q([l[3] for l in legs], 0.75), q([l[3] for l in legs], 0.9)],
        "leg_days": mean([l[2] for l in legs]), "leg_km": mean([l[4] for l in legs]),
        "loss_legs": 100.0 * sum(1 for l in legs if l[3] < 0) / max(1, len(legs)),
        "laden": 100.0 * sum(l[5] for l in legs) / max(1, len(legs)),
        "laden_days": 100.0 * sum(l[2] for l in legs if l[5]) / max(1e-9, sum(l[2] for l in legs)),
        "laden_leg": [mean([l[3] for l in legs if l[5]]), q([l[3] for l in legs if l[5]], 0.5), q([l[3] for l in legs if l[5]], 0.75),
                      q([l[3] for l in legs if l[5]], 0.9), mean([l[2] for l in legs if l[5]])],
        "laden_loss": 100.0 * sum(1 for l in legs if l[5] and l[3] < 0) / max(1, sum(l[5] for l in legs)),
        "wages": mean([r["wages"] for r in rs]), "tolls": mean([r["tolls"] for r in rs]),
        "bread": mean([r["bread"] for r in rs]), "fees": mean([r["fees"] for r in rs]), "gain": mean([r["gain"] for r in rs]),
        "hungry": mean([r["hungry"] for r in rs]), "sea_days": mean([r["sea_days"] for r in rs]),
        "idle": mean([r.get("idle", 0) for r in rs]),
        "clear": [q([r["first_clear"] for r in rs if r["first_clear"] is not None], p) for p in (0.25, 0.5, 0.75)],
        "never": 100.0 * sum(1 for r in rs if r["first_clear"] is None) / n,
        "worth": {d: [q([r["worth_day"][d] for r in rs], p) for p in (0.1, 0.5, 0.9)] for d in range(0, D + 1) if d % 25 == 0 or d == D},
    }
    ten = [r["worth_day"][d + 10] - r["worth_day"][d] for r in rs for d in range(0, D - 10, 5)]
    out["ten"] = [q(ten, p) for p in (0.5, 0.75, 0.9, 0.98)]
    out["ten_mean"] = mean(ten)
    out["ten_lost"] = 100.0 * sum(1 for x in ten if x < 0) / max(1, len(ten))
    out["ten_all"] = ten
    out["orders"] = [mean([r["orders"][i] for r in rs]) for i in range(10)]
    out["ai"] = [mean([r["ai"][i] for r in rs]) for i in range(7)]
    out["home_silver"] = sorted((r["silver"] if r["home"] else 0.0) for r in rs)
    out["worth_all"] = [r["worth_day"] for r in rs]
    out["home_each"] = [(r["silver"] if r["home"] else 0.0) for r in rs]
    out["laden_n"] = sum(l[5] for l in legs) / float(n)
    # in doubt: on day d, how many seasons does the plain pace (what was made so far, carried on) call wrongly?
    doubt = {}
    for d in (25, 50, 75, 100, 125):
        if d >= D:
            continue
        wrong = 0
        for r in rs:
            pace = r["worth0"] + (r["worth_day"][d] - r["worth0"]) * D / d
            wrong += (pace >= R["debt"]) != r["paid"]
        doubt[d] = 100.0 * wrong / n
    out["doubt"] = doubt
    out["open"] = {d: 100.0 * sum(1 for r in rs if r["worth_day"][d] < R["debt"]) / n for d in (25, 50, 75, 100, 125) if d < D}
    good, route = {}, {}
    for r in rs:
        for k, v in r["gain_good"].items():
            good[k] = good.get(k, 0.0) + v / n
        for k, v in r["gain_route"].items():
            route[k] = route.get(k, 0.0) + v / n
    out["goods"] = sorted(good.items(), key=lambda kv: -kv[1])
    out["routes"] = sorted(route.items(), key=lambda kv: -kv[1])
    legc = {}
    for l in legs:
        k = (l[0], l[1])
        c = legc.setdefault(k, [0, 0.0, 0.0])
        c[0] += 1
        c[1] += l[3]
        c[2] += l[2]
    out["legtable"] = sorted(((k, c[0] / n, c[1] / c[0], c[2] / c[0]) for k, c in legc.items()), key=lambda x: -x[1])
    return out


def static_table(T):
    """Free money in the tables: one good, one way, a full hold, markets at their usual stock, 1 July, no other ship."""
    R = T.R
    di = doy((7, 1)) - 1
    mk = [Market(T, i, hashed=False) for i in range(len(PORTS))]
    rows = []
    for a in range(len(PORTS)):
        for b in range(len(PORTS)):
            if a == b:
                continue
            for g in range(T.G):
                if mk[a].usual[g] < 1 or g == T.bread:
                    continue
                law = not (T.lawful[a, g] and T.lawful[b, g])
                st0, gl0 = float(mk[b].stock[g]), float(mk[b].glut[g])
                sa0 = float(mk[a].stock[g])
                n, cost, rev = 0, 0.0, 0.0
                room = R["hold"]
                first = None
                while room >= T.space_l[g]:
                    k, c = mk[a].buy(g, 1, INF, di)
                    if k == 0:
                        break
                    s = mk[b].sell(g, 1, di) * (1 - T.toll[b])
                    if first is None:
                        first = (c, s)
                    if s <= c:
                        break
                    n += 1
                    cost += c
                    rev += s
                    room -= T.space_l[g]
                mk[a].stock[g] = sa0
                mk[b].stock[g] = st0
                mk[b].glut[g] = gl0
                if n > 0:
                    rows.append((rev - cost, T.gid[g], a, b, n, cost, first[0], first[1], T.pkm[a][b], law))
    rows.sort(key=lambda r: -r[0])
    return rows


def sail_table(T, seeds=12):
    """Days from Dorestat to each port: straight through, and lying up in the dark near land."""
    acc = {b: [[], []] for b in range(1, len(PORTS))}
    for s in range(seeds):
        w = World(T, 9000 + s, "none")
        spd = w.SPD[PLAYER_CLASS]
        for d in range(0, w.season - 20, 5):
            for b in acc:
                ch = T.routes["player"][(HOME, b)][1]
                acc[b][0].append(w.sail(ch, d + 0.25, spd, False) - d - 0.25)
                acc[b][1].append(w.sail(ch, d + 0.25, spd, True) - d - 0.25)
    return acc


def print_config(name, R, out):
    out("### %s" % name)
    out("start %02d-%02d, due %02d-%02d (day %d); debt %.0f d; purse %.1f d; cargo %s; hold %.0f kg of room"
        % (R["start"][0], R["start"][1], R["due"][0], R["due"][1], doy(R["due"]) - doy(R["start"]), R["debt"], R["start_silver"],
           ", ".join("%d %s" % (n, g) for g, n in R["start_cargo"]), R["hold"]))
    out("spread %.2f; toll Frankish %.2f, English %.2f, weighed silver %.2f; wage %.2f d a man a day (%d men); "
        "appetite_value %.0f; stock_value %.0f; regen %.2f; glut_fade %.2f"
        % (R["spread"], R["toll"]["frankish"], R["toll"]["english"], R["toll"]["weight"], R["pay"], R["men"],
           R["appetite_value"], R["stock_value"], R["regen"], R["glut_fade"]))
    out("made_factor %s; wanted_factor %s" % (json.dumps(R["made_factor"], sort_keys=True), json.dumps(R["wanted_factor"], sort_keys=True)))
    extra = []
    if R["door_fee"]:
        extra.append("the reeve at Lundenwic %.0f d, eased %.0f days" % (R["door_fee"], R["door_days"]))
    if R["call_fee"]:
        extra.append("landing due %.1f d a call" % R["call_fee"])
    if R["value"]:
        extra.append("value " + json.dumps(R["value"], sort_keys=True))
    if R["made"] or R["wanted"]:
        extra.append("made " + json.dumps(R["made"], sort_keys=True) + " wanted " + json.dumps(R["wanted"], sort_keys=True))
    warm = max(0, doy(R["start"]) - doy(R["world_from"])) if R["world_from"] else R["warmup"]
    extra.append("famine %s (%s); other traders x%.2f, at work %d days before day one; talks a call %d"
                 % ("on" if R["famine"] else "off", R["famine_goods"] if isinstance(R["famine_goods"], str) else ",".join(R["famine_goods"]),
                    R["field"], warm, R["talks"]))
    out("; ".join(extra))
    out("")


def print_summary(T, sums, out, top=8):
    R = T.R
    kinds = [k for k in ("perfect", "sensible", "careless") if k in sums]
    D = doy(R["due"]) - doy(R["start"])
    out("%-36s" % "" + "".join("%12s" % k for k in kinds))

    def row(label, f):
        out("%-36s" % label + "".join("%12s" % f(sums[k]) for k in kinds))
    row("seasons", lambda s: "%d" % s["n"])
    row("PAYS %.0f d on the day, %%" % R["debt"], lambda s: "%.0f" % s["paid"])
    row("home on the day, %", lambda s: "%.0f" % s["home"])
    row("silver on the day: 1 in 10 under", lambda s: "%.0f" % s["silver"][0])
    row("silver on the day: middle", lambda s: "%.0f" % s["silver"][1])
    row("silver on the day: 1 in 10 over", lambda s: "%.0f" % s["silver"][2])
    row("the sum paid in 8 seasons of 10", lambda s: "%.0f" % s["sum"][0])
    row("the sum paid in 5 seasons of 10", lambda s: "%.0f" % s["sum"][1])
    row("the sum paid in 2 seasons of 10", lambda s: "%.0f" % s["sum"][2])
    row("made in the season (middle), d", lambda s: "%.0f" % s["made"][1])
    row("made a day, d", lambda s: "%.1f" % s["per_day"])
    row("legs a season", lambda s: "%.1f" % s["legs"])
    row("a leg: days", lambda s: "%.1f" % s["leg_days"])
    row("a leg: km", lambda s: "%.0f" % s["leg_km"])
    row("made a leg: mean, d", lambda s: "%.0f" % s["leg"][0])
    row("made a leg: middle, d", lambda s: "%.0f" % s["leg"][1])
    row("made a leg: 1 in 4 over, d", lambda s: "%.0f" % s["leg"][2])
    row("made a leg: 1 in 10 over, d", lambda s: "%.0f" % s["leg"][3])
    row("legs that lost silver, %", lambda s: "%.0f" % s["loss_legs"])
    row("legs with goods aboard, %", lambda s: "%.0f" % s["laden"])
    row("  their share of the days, %", lambda s: "%.0f" % s["laden_days"])
    row("  a laden leg: days", lambda s: "%.1f" % s["laden_leg"][4])
    row("  made a laden leg: mean, d", lambda s: "%.0f" % s["laden_leg"][0])
    row("  made a laden leg: middle, d", lambda s: "%.0f" % s["laden_leg"][1])
    row("  made a laden leg: 1 in 4 over", lambda s: "%.0f" % s["laden_leg"][2])
    row("  made a laden leg: 1 in 10 over", lambda s: "%.0f" % s["laden_leg"][3])
    row("  laden legs that lost silver, %", lambda s: "%.0f" % s["laden_loss"])
    row("made in ten days: middle, d", lambda s: "%.0f" % s["ten"][0])
    row("made in ten days: 1 in 4 over, d", lambda s: "%.0f" % s["ten"][1])
    row("made in ten days: 1 in 10 over, d", lambda s: "%.0f" % s["ten"][2])
    row("made in ten days: 1 in 50 over, d", lambda s: "%.0f" % s["ten"][3])
    row("gain on goods a season, d", lambda s: "%.0f" % s["gain"])
    row("  tolls paid (already off), d", lambda s: "%.0f" % s["tolls"])
    row("  wages, d", lambda s: "%.0f" % s["wages"])
    row("  bread, d", lambda s: "%.0f" % s["bread"])
    row("  the reeve and landing dues, d", lambda s: "%.0f" % s["fees"])
    row("days under way", lambda s: "%.0f" % s["sea_days"])
    row("days lying in port for want of a cargo", lambda s: "%.0f" % s["idle"])
    row("days with no bread", lambda s: "%.1f" % s["hungry"])
    if R["orders"]:
        row("orders posted in the twelve ports", lambda s: "%.1f" % s["orders"][8])
        row("  seen at a call (too big for her)", lambda s: "%.1f (%.1f)" % (s["orders"][0], s["orders"][1]))
        row("  taken / delivered / broken", lambda s: "%.1f/%.1f/%.1f" % (s["orders"][2], s["orders"][3], s["orders"][4]))
        row("  gain on orders a season, d", lambda s: "%.0f" % s["orders"][5])
        row("  of it over the market's price, d", lambda s: "%.0f" % s["orders"][6])
    row("tally first reads %.0f: 1 in 4 by day" % R["debt"], lambda s: "%.0f" % s["clear"][0])
    row("tally first reads %.0f: middle day" % R["debt"], lambda s: "%.0f" % s["clear"][1])
    row("tally first reads %.0f: 3 in 4 by day" % R["debt"], lambda s: "%.0f" % s["clear"][2])
    row("tally never reads %.0f, %%" % R["debt"], lambda s: "%.0f" % s["never"])
    out("")
    out("In hand and in the hold (silver, less wages owed, plus the hold at cost): 1 in 10 under / middle / 1 in 10 over")
    for d in sorted(sums[kinds[0]]["worth"]):
        out("  day %3d   " % d + "   ".join("%-9s %5.0f /%5.0f /%5.0f" % (k, sums[k]["worth"][d][0], sums[k]["worth"][d][1], sums[k]["worth"][d][2]) for k in kinds))
    out("")
    out("Still short of %.0f on the day (%% of seasons), and how often the plain pace calls the ending wrongly (%%)" % R["debt"])
    for d in (25, 50, 75, 100, 125):
        if d < D:
            out("  day %3d   " % d + "   ".join("%-9s short %3.0f, pace wrong %3.0f" % (k, sums[k]["open"][d], sums[k]["doubt"][d]) for k in kinds))
    out("")

    def nm(key):
        a, b = key.split(">")
        return "%s to %s" % (SHORT[int(a)], SHORT[int(b)])
    for k in kinds:
        s = sums[k]
        tot = sum(v for _, v in s["goods"] if v > 0) or 1.0
        out("%s: goods by share of the gain: " % k + "; ".join("%s %.0f%% (%.0f d)" % (g, 100 * v / tot, v) for g, v in s["goods"][:top]))
        worst = [x for x in s["goods"] if x[1] < -0.5][-3:]
        if worst:
            out("%s: goods that lost: " % k + "; ".join("%s %.0f d" % (g, v) for g, v in worst))
        tot = sum(v for _, v in s["routes"] if v > 0) or 1.0
        out("%s: bought at, sold at, by share of the gain: " % k + "; ".join("%s %.0f%%" % (nm(r), 100 * v / tot) for r, v in s["routes"][:top]))
        out("%s: legs sailed most (times a season, made a leg, days): " % k + "; ".join(
            "%s to %s %.1fx %.0f d %.1f days" % (SHORT[a], SHORT[b], c, p, d) for (a, b), c, p, d in s["legtable"][:top]))
        out("")


def brief(kinds, sums, out):
    for k in kinds:
        s = sums[k]
        w100 = s["worth"].get(100, s["worth"][max(s["worth"])])
        out("%-9s pays %3.0f%% home %3.0f%% silver %5.0f/%5.0f/%5.0f  a day %5.1f  legs %4.1f  a leg %4.0f (mid %4.0f, 1in4 %4.0f, 1in10 %4.0f)  clear %s/%s/%s never %3.0f%%  d100 %4.0f/%4.0f/%4.0f  short@100 %3.0f%% wrong@100 %s"
            % (k, s["paid"], s["home"], s["silver"][0], s["silver"][1], s["silver"][2], s["per_day"], s["legs"], s["leg"][0], s["leg"][1], s["leg"][2], s["leg"][3],
               "%.0f" % s["clear"][0], "%.0f" % s["clear"][1], "%.0f" % s["clear"][2], s["never"], w100[0], w100[1], w100[2],
               s["open"].get(100, float("nan")), "%.0f" % s["doubt"].get(100, float("nan"))) + "  ten %.0f/%.0f/%.0f  laden %.0f%% of legs: %.0f/%.0f/%.0f/%.0f" % (s["ten"][0], s["ten"][1], s["ten"][2], s["laden"], s["laden_leg"][0], s["laden_leg"][1], s["laden_leg"][2], s["laden_leg"][3]))
        out("          half the seasons pay %.0f d (4 in 5: %.0f; 1 in 5: %.0f)  ten days mean %.0f, lose %.0f%%  laden legs lose %.0f%%  orders posted %.1f seen %.1f too big %.1f taken %.1f done %.1f broken %.1f gain %.0f (over the market %.0f)"
            % (debt_for(s), debt_for(s, 0.8), debt_for(s, 0.2), s["ten_mean"], s["ten_lost"], s["laden_loss"], s["orders"][8], s["orders"][0], s["orders"][1], s["orders"][2], s["orders"][3], s["orders"][4], s["orders"][5], s["orders"][6]))


def debt_for(s, share=0.5):
    """The debt that is paid in this share of the seasons: so much silver, at home on the day."""
    xs = s["home_silver"]
    return xs[max(0, min(len(xs) - 1, int(round((1.0 - share) * len(xs)))))]


def against(s, debt):
    """A summary read against another debt (the player does not look at the debt, so no new run is needed):
    pays %, the day the tally first reads it, never %, still short on day 100 and on day 125 %."""
    xs = s["home_silver"]
    paid = 100.0 * sum(1 for x in xs if x >= debt) / len(xs)
    first, short100, short125, n = [], 0, 0, len(s["worth_all"])
    wrong = {50: 0, 75: 0, 100: 0, 125: 0}
    for w, hs in zip(s["worth_all"], s["home_each"]):
        D = len(w) - 1
        f = next((d for d in range(1, D + 1) if w[d] >= debt), None)
        if f is not None:
            first.append(f)
        short100 += w[min(100, D - 1)] < debt
        short125 += w[min(125, D - 1)] < debt
        for d in wrong:
            if d < D:
                pace = w[0] + (w[d] - w[0]) * D / float(d)
                wrong[d] += (pace >= debt) != (hs >= debt)
    return {"paid": paid, "first": q(first, 0.5), "first25": q(first, 0.25), "never": 100.0 * (n - len(first)) / n,
            "short100": 100.0 * short100 / n, "short125": 100.0 * short125 / n,
            "wrong": {d: 100.0 * v / n for d, v in wrong.items()}}


def run_config(name, sets, seeds, kinds, pool, out, quiet=False):
    T = tables(sets)
    jobs = [(sets, s, k) for k in kinds for s in seeds]
    t0 = time.time()
    res = pool.map(run_one, jobs, chunksize=4) if pool else [run_one(j) for j in jobs]
    sums = {k: summarise([r for r in res if r["kind"] == k], T.R) for k in kinds}
    if not quiet:
        print_config(name, T.R, out)
        print_summary(T, sums, out)
        out("(%d runs in %.0f s)" % (len(jobs), time.time() - t0))
        out("")
    return sums


def watch_table(sets, seeds, pool, out):
    T = tables(sets)
    rows = {}
    labels = ("no other ship", "with the other traders")
    for label, field in zip(labels, (0.0, T.R["field"])):
        s2 = dict(sets)
        s2["field"] = field
        jobs = [(s2, s, "none") for s in seeds]
        res = pool.map(run_one, jobs) if pool else [run_one(j) for j in jobs]
        acc = [[0.0, 0.0, 0, 0] for _ in World.WATCH]
        for r in res:
            for k, w in enumerate(r["watch"]):
                for i in range(4):
                    acc[k][i] += w[i]
        rows[label] = acc
    out("%-15s %-24s %30s   %30s" % ("good", "bought at, sold at", labels[0], labels[1]))
    out("%-15s %-24s %30s   %30s" % ("", "", "asks / pays net / on offer", "asks / pays net / on offer"))
    for k, (gid, a, b) in enumerate(World.WATCH):
        cells = []
        for label in labels:
            w = rows[label][k]
            n = max(1, w[3])
            cells.append("%7.1f / %7.1f / %3.0f%%" % (w[0] / n, w[1] / n, 100.0 * w[2] / n))
        out("%-15s %-24s %30s   %30s" % (gid, "%s > %s" % (SHORT[a], SHORT[b]), cells[0], cells[1]))
    out("")


def ladder(title, rows, seeds, pool, out, kind="sensible"):
    """One line a setting: the same seeds, one kind of player."""
    out(title)
    out("%-58s %5s %5s %17s %6s %15s %11s %14s %6s %7s" % ("", "pays", "home", "silver on the day", "a day", "a laden leg", "ten days", "tally reads it", "never", "short"))
    out("%-58s %5s %5s %17s %6s %15s %11s %14s %6s %7s" % ("", "%", "%", "low / mid / high", "d", "mean / 1 in 10", "mid / good", "by day (mid)", "%", "day100"))
    for label, sets in rows:
        T = tables(sets)
        jobs = [(sets, s_, kind) for s_ in seeds]
        res = pool.map(run_one, jobs, chunksize=4) if pool else [run_one(j) for j in jobs]
        s = summarise(res, T.R)
        out("%-58s %5.0f %5.0f %5.0f /%5.0f /%5.0f %6.1f %7.0f /%5.0f %5.0f /%4.0f %14s %6.0f %7s"
            % (label, s["paid"], s["home"], s["silver"][0], s["silver"][1], s["silver"][2], s["per_day"], s["laden_leg"][0], s["laden_leg"][3],
               s["ten"][0], s["ten"][2], ("%.0f" % s["clear"][1]) if s["clear"][1] == s["clear"][1] else "-", s["never"],
               ("%.0f%%" % s["open"][100]) if 100 in s["open"] else "-"))
    out("")


# BEFORE: the code as it stands. One free talk gives every price the port's board holds (PortLife.Talk).
BEFORE = {"talks": 12}
# TUNED: the smallest set of changed numbers this script found (see Design/research/next/money.md):
#   the world's traders at work from 1 April (a pre-run before the player's first morning);
#   the famine of 793 on grain only (shocks_793.json), not on the whole Food category;
#   the English toll a tenth, as the Frankish (trade_rules.json).
# And the book's rule of three talks a call (GAME_MAKER 4.1), which is a rule of hearing, not a price.
GRAIN = "wheat,rye,barley,oats,malt,flour"
TUNED = {"world_from": [4, 1], "famine_goods": GRAIN, "toll.english": 0.10, "talks": 3}
# APPROVED: the tuned set as the user approved it (ruling 14), with the margin pinned at what the code had on 7 October,
# so that this stays the "before" of money_that_pays.md after a builder has changed WorldSim.cs.
APPROVED = dict(TUNED, min_margin=0.12)
CONFIGS = {"before": dict(BEFORE, min_margin=0.12), "tuned": TUNED, "approved": APPROVED, "data": {}}


# PAYS: the proposal of Design/fixes/money_that_pays.md, on top of the approved set: the world's traders leave a
# quarter (SimRules.minMargin 0.12 -> 0.25) and the count is owed eight pounds (trade_rules.json start.debt 480 -> 1920).
PAYS = dict(APPROVED, min_margin=0.25, debt=1920)
CONFIGS["pays"] = PAYS
POUND = 240.0


def plus2(base, **kw):
    d = dict(base)
    for k, v in kw.items():
        d[k.replace("__", ".")] = v
    return d


def one(sets, seeds, pool, kind="sensible"):
    T = tables(sets)
    jobs = [(sets, s_, kind) for s_ in seeds]
    res = pool.map(run_one, jobs, chunksize=4) if pool else [run_one(j) for j in jobs]
    return summarise(res, T.R)


def prices_table(rows, seeds, pool, out):
    """What the world's traders leave: asked where bought, paid where sold after the toll; the season's mean and the
    player's first morning. One block for each setting."""
    for label, sets in rows:
        jobs = [(sets, s, "none") for s in seeds]
        res = pool.map(run_one, jobs) if pool else [run_one(j) for j in jobs]
        out("%s" % label)
        out("  %-15s %-24s %28s %28s" % ("good", "bought at, sold at", "the season: asks > pays, gain", "first morning: asks > pays, gain"))
        for k, (gid, a, b) in enumerate(World.WATCH):
            sa = sum(r["watch"][k][0] for r in res) / max(1, sum(r["watch"][k][3] for r in res))
            sb = sum(r["watch"][k][1] for r in res) / max(1, sum(r["watch"][k][3] for r in res))
            fa = mean([r["first"][k][0] for r in res if r["first"]])
            fb = mean([r["first"][k][1] for r in res if r["first"]])
            out("  %-15s %-24s %10.1f > %5.1f: %+6.1f (%+3.0f%%) %10.1f > %5.1f: %+6.1f (%+3.0f%%)"
                % (gid, "%s > %s" % (SHORT[a], SHORT[b]), sa, sb, sb - sa, 100 * (sb - sa) / sa, fa, fb, fb - fa, 100 * (fb - fa) / fa))
        ai = [mean([r["ai"][i] for r in res]) for i in range(7)]
        out("  the %d other traders, from the first morning to Michaelmas: %.1f sailings each, %.0f%% of them laden; %.1f times each found nothing worth carrying; %.0f d laid out on goods each; worth %.0f d at the end (%.0f d at the start)"
            % (ai[4], ai[0] / max(1, ai[4]), 100 * ai[3] / max(1, ai[0]), ai[1] / max(1, ai[4]), ai[2] / max(1, ai[4]), ai[5], ai[6]))
        out("")


def pays_report(seeds, pool, out, extra):
    n = len(seeds)
    half = seeds[:max(1, n * 2 // 3)]
    ap_ = dict(APPROVED)
    pr = dict(PAYS)
    pr.update(extra)
    debt = float(pr["debt"])
    out("# MONEY THAT PAYS: soak2.py, %d seeded seasons for each kind of player (%d where a table says so)" % (n, len(half)))
    out("")
    out("## 1. What the world's traders leave open, by the least margin they will carry for (24 seeds, no player)")
    prices_table([("the approved set, margin 0.12", ap_), ("margin 0.20", plus2(ap_, min_margin=0.20)), ("margin 0.25 (proposed)", pr),
                  ("margin 0.30", plus2(ap_, min_margin=0.30))], seeds[:min(24, n)], pool, out)

    out("## 2. The margin, a step at a time (the sensible player, 1 May to Michaelmas, %d seasons each; all else as approved)" % len(half))
    out("%-8s %14s %19s %7s %12s %22s %6s %22s %6s" % ("margin", "half the", "silver on the day", "made", "laden legs", "a laden leg makes", "lose", "ten days make", "lose"))
    out("%-8s %14s %19s %7s %12s %22s %6s %22s %6s" % ("", "seasons pay", "low / mid / high", "a day", "of all legs", "mean/mid/1 in 4/1 in 10", "%", "mid/1 in 4/1 in 10", "%"))
    ladder_s = {}
    for mm in (0.12, 0.15, 0.18, 0.20, 0.22, 0.25, 0.30, 0.35):
        s = one(plus2(ap_, min_margin=mm), half, pool)
        ladder_s[mm] = s
        h = debt_for(s)
        out("%-8.2f %6.0f (%4.1f lb) %5.0f /%5.0f /%5.0f %7.1f %5.1f of %4.1f %5.0f /%3.0f /%4.0f /%4.0f %6.0f %6.0f /%5.0f /%5.0f %6.0f"
            % (mm, h, h / POUND, s["silver"][0], s["silver"][1], s["silver"][2], s["per_day"], s["laden_n"], s["legs"],
               s["laden_leg"][0], s["laden_leg"][1], s["laden_leg"][2], s["laden_leg"][3], s["laden_loss"], s["ten"][0], s["ten"][1], s["ten"][2], s["ten_lost"]))
    out("")

    out("## 3. BEFORE: the approved set (ruling 14), the debt 480 d")
    sb = run_config("approved, 1 May to Michaelmas", ap_, seeds, ["perfect", "sensible", "careless"], pool, out)
    out("## 4. AFTER: the margin 0.25 and the debt %.0f d (%.1f pounds)" % (debt, debt / POUND))
    sa = run_config("proposed, 1 May to Michaelmas", pr, seeds, ["perfect", "sensible", "careless"], pool, out)

    out("## 5. The lord's terms at the margin 0.25: what each sum would mean (the same %d seasons; the players do not look at the debt)" % n)
    s = sa["sensible"]
    out("the sum the sensible player holds at home on the day in 9 seasons of 10: %.0f d; in 8: %.0f; in 2 of 3: %.0f; in half: %.0f; in 1 of 3: %.0f; in 2 of 10: %.0f; in 1 of 10: %.0f"
        % (debt_for(s, 0.9), debt_for(s, 0.8), debt_for(s, 2 / 3.0), debt_for(s, 0.5), debt_for(s, 1 / 3.0), debt_for(s, 0.2), debt_for(s, 0.1)))
    out("%-20s %9s %22s %7s %14s %14s %19s %9s %12s %9s" % ("the debt", "sensible", "tally first reads it", "never", "short, day 100", "short, day 125", "pace wrong, day", "perfect", "perfect first", "careless"))
    out("%-20s %9s %22s %7s %14s %14s %19s %9s %12s %9s" % ("", "pays %", "1 in 4 / middle day", "%", "%", "%", "75 / 100 / 125 %", "pays %", "reads it, day", "pays %"))
    for lb in (4, 5, 6, 7, 7.5, 8, 8.5, 9, 10, 12):
        d = lb * POUND
        a_, p_, c_ = against(s, d), against(sa["perfect"], d), against(sa["careless"], d)
        out("%-20s %9.0f %12.0f / %-7.0f %7.0f %14.0f %14.0f %7.0f /%4.0f /%4.0f %9.0f %12.0f %9.0f"
            % ("%4.1f pounds (%4.0f d)" % (lb, d), a_["paid"], a_["first25"], a_["first"], a_["never"], a_["short100"], a_["short125"],
               a_["wrong"][75], a_["wrong"][100], a_["wrong"][125], p_["paid"], p_["first"], c_["paid"]))
    out("")
    out("the same for the approved set (margin 0.12), for comparison")
    s0 = sb["sensible"]
    for d in (480.0,):
        a_, p_, c_ = against(s0, d), against(sb["perfect"], d), against(sb["careless"], d)
        out("%-20s %9.0f %12.0f / %-7.0f %7.0f %14.0f %14.0f %7.0f /%4.0f /%4.0f %9.0f %12.0f %9.0f"
            % ("%4.1f pounds (%4.0f d)" % (d / POUND, d), a_["paid"], a_["first25"], a_["first"], a_["never"], a_["short100"], a_["short125"],
               a_["wrong"][75], a_["wrong"][100], a_["wrong"][125], p_["paid"], p_["first"], c_["paid"]))
    out("")
    out("the tally sentence, proposed: in hand and in the hold, 1 in 10 under / middle / 1 in 10 over; and ten days' gain by where in the season")
    for dd in sorted(s["worth"]):
        out("  day %3d   %5.0f /%5.0f /%5.0f" % (dd, s["worth"][dd][0], s["worth"][dd][1], s["worth"][dd][2]))
    out("")

    out("## 6. How firm it is (the sensible player, %d seasons each): each row against the debt %.0f d" % (len(half), debt))
    out("%-62s %8s %14s %19s %16s %12s" % ("", "pays %", "half the", "silver on the day", "a laden leg", "ten days"))
    out("%-62s %8s %14s %19s %16s %12s" % ("", "", "seasons pay", "low / mid / high", "mean / 1 in 10", "middle"))
    rows = [
        ("THE PROPOSED SET", pr),
        ("he wants 10 % over what he pays, not 15 %", plus2(pr, safety=1.10)),
        ("he wants 20 %", plus2(pr, safety=1.20)),
        ("he wants 30 %", plus2(pr, safety=1.30)),
        ("other traders x0.75 (42 ships, not 56)", plus2(pr, field=0.75)),
        ("other traders x1.25 (70 ships)", plus2(pr, field=1.25)),
        ("no talks at all", plus2(pr, talks=0)),
        ("every price free at each call (the code today)", plus2(pr, talks=12)),
        ("the reeve at Lundenwic takes 20 d, eased 30 days", plus2(pr, door_fee=20)),
        ("a landing due of 1 d at every call (stands for rounding)", plus2(pr, call_fee=1.0)),
        ("the famine off", plus2(pr, famine=0)),
        ("NOT APPLIED: the famine still on all food", plus2(pr, famine_goods="food")),
        ("NOT APPLIED: the English toll still 0.05", plus2(pr, toll__english=0.05)),
        ("NOT BUILT: no pre-run (the traders start on the first morning)", plus2(pr, world_from=None)),
        ("the margin 0.22", plus2(pr, min_margin=0.22)),
        ("the margin 0.28", plus2(pr, min_margin=0.28)),
        ("the start on 21 June (100 days)", plus2(pr, start=[6, 21])),
    ]
    for label, sets in rows:
        s_ = one(sets, half, pool)
        a_ = against(s_, debt)
        h = debt_for(s_)
        out("%-62s %8.0f %6.0f (%4.1f lb) %5.0f /%5.0f /%5.0f %8.0f /%5.0f %12.0f"
            % (label, a_["paid"], h, h / POUND, s_["silver"][0], s_["silver"][1], s_["silver"][2], s_["laden_leg"][0], s_["laden_leg"][3], s_["ten"][0]))
    out("")

    out("## 7. The orders of commissions_793.json at the twelve ports (the sensible player, %d seasons each, the proposed set)" % len(half))
    out("%-58s %7s %6s %8s %7s %10s %8s %12s %14s %12s" % ("", "posted", "seen", "too big", "taken", "delivered", "broken", "gain, d", "hoped for, d", "half pay"))
    for label, sets in [
        ("as the code pays them (no toll, the table's premiums)", plus2(pr, orders=1)),
        ("tolled, premium at most 0.5, at most 60 d over the market", plus2(pr, orders=1, order_tolled=1, order_premium_max=0.5, order_extra_max=60.0)),
        ("WHAT IF the table paid the cards' premium of 1.5, no toll", plus2(pr, orders=1, order_premium_max=99.0, order_safety=1.15, **{"order_premium_all": 1.5})),
        ("the same, tolled and capped", plus2(pr, orders=1, order_tolled=1, order_premium_max=0.5, order_extra_max=60.0, **{"order_premium_all": 1.5})),
    ]:
        s_ = one(sets, half, pool)
        o = s_["orders"]
        out("%-58s %7.1f %6.1f %8.1f %7.1f %10.1f %8.1f %12.0f %14.0f %12.0f" % (label, o[8], o[0], o[1], o[2], o[3], o[4], o[5], o[9], debt_for(s_)))
    out("")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--seeds", type=int, default=300)
    ap.add_argument("--first", type=int, default=1)
    ap.add_argument("--config", default="report", help="pays (the report of money_that_pays.md) | report (soak.py's own) | before | approved | tuned | data")
    ap.add_argument("--kinds", default="perfect,sensible,careless")
    ap.add_argument("--set", action="append", default=[], help="key=value over the chosen config, e.g. toll.frankish=0.15")
    ap.add_argument("--jobs", type=int, default=0)
    ap.add_argument("--brief", action="store_true")
    ap.add_argument("--trace", action="store_true", help="one season, the first seed and kind, deal by deal")
    a = ap.parse_args()
    extra = {}
    for s in a.set:
        k, v = s.split("=", 1)
        try:
            extra[k] = json.loads(v)
        except ValueError:
            extra[k] = v
    seeds = list(range(a.first, a.first + a.seeds))
    kinds = [k for k in a.kinds.split(",") if k]
    jobs = a.jobs or max(1, min(8, (os.cpu_count() or 2) - 1))
    pool = None
    if jobs > 1:
        try:
            import multiprocessing as mp
            pool = mp.Pool(jobs)
        except Exception:
            pool = None

    def out(s=""):
        print(s)
        sys.stdout.flush()

    T0 = tables({})
    if a.trace:
        sets = dict(CONFIGS.get(a.config, {}))
        sets.update(extra)
        r = run_one((sets, seeds[0], kinds[0], True))
        for line in r["trace"]:
            out(line)
        out("paid %s, home %s, silver %.0f" % (r["paid"], r["home"], r["silver"]))
        return
    if a.config == "pays" and not a.brief:
        pays_report(seeds, pool, out, extra)
    elif a.config == "report":
        out("# THE MONEY, RECKONED: soak.py, %d seeded seasons for each kind of player" % a.seeds)
        out("")
        out("## 1. The water: km by the lanes from Dorestat, and days in this script's wind (?)")
        acc = sail_table(T0)
        out("%-12s %6s %22s %32s" % ("to", "km", "straight through", "lying up in the dark near land"))
        out("%-12s %6s %22s %32s" % ("", "", "mean (1 in 10 over)", "mean (1 in 10 over)"))
        for b in range(1, len(PORTS)):
            out("%-12s %6.0f %14.1f (%4.1f) %22.1f (%4.1f)" % (SHORT[b], T0.pkm[HOME][b], mean(acc[b][0]), q(acc[b][0], 0.9),
                                                               mean(acc[b][1]), q(acc[b][1], 0.9)))
        out("")
        out("## 2. Free money in the tables as they stand: one good, one way, a full hold of 660 kg of room")
        out("(markets at their usual stock, 1 July, the famine on, no other ship; the gain is after the spread and the toll;")
        out(" 'law' marks a good the capitularies forbid: the world's traders will not carry it, the player may)")
        rows = static_table(T0)
        out("%-15s %-24s %5s %7s %7s  %-12s %6s %s" % ("good", "bought at, sold at", "units", "outlay", "gain", "first unit", "km", ""))
        for r in rows[:26]:
            out("%-15s %-24s %5d %7.0f %7.0f  %4.0f > %-5.0f %6.0f %s" % (r[1], "%s > %s" % (SHORT[r[2]], SHORT[r[3]]), r[4], r[5], r[0],
                                                                         r[6], r[7], r[8], "law" if r[9] else ""))
        out("pairs of good and way that gain at all: %d; over 100 d a hold: %d; over 300 d: %d"
            % (len(rows), sum(1 for r in rows if r[0] > 100), sum(1 for r in rows if r[0] > 300)))
        out("")
        ns = min(24, a.seeds)
        out("## 3. What the other traders do to prices (mean over the season, %d seeds)" % ns)
        watch_table({}, seeds[:ns], pool, out)
        out("## 4. BEFORE: the code and the data as they stand")
        b1 = dict(BEFORE)
        b1.update(extra)
        run_config("before, 1 May to Michaelmas", b1, seeds, kinds, pool, out)
        b2 = dict(b1)
        b2["start"] = [6, 21]
        run_config("before, 21 June to Michaelmas (the start in the book and in the code)", b2, seeds, kinds, pool, out)
        t1 = dict(TUNED)
        t1.update(extra)
        t2 = dict(t1)
        t2["start"] = [6, 21]
        half = seeds[:max(1, len(seeds) * 2 // 3)]

        def plus(base, **kw):
            d = dict(base)
            for k, v in kw.items():
                d[k.replace("__", ".")] = v
            return d
        out("## 5. Where the gain goes: one change at a time (the sensible player, %d seasons each)" % len(half))
        ladder("From the code as it stands to the tuned set, a step at a time (1 May to Michaelmas)", [
            ("the code as it stands (every price free at each call)", b1),
            ("+ three talks a call (the rule in the book)", plus(b1, talks=3)),
            ("+ the other traders at work from 1 April", plus(b1, talks=3, world_from=[4, 1])),
            ("+ the famine on grain only", plus(b1, talks=3, world_from=[4, 1], famine_goods=GRAIN)),
            ("+ the English toll a tenth  = THE TUNED SET", t1),
        ], half, pool, out)
        ladder("Other levers, each alone on top of: from 1 April, three talks, famine on grain (English toll still 0.05)", [
            ("(none)", plus(t1, toll__english=0.05)),
            ("the reeve at Lundenwic, 20 d, eased 30 days", plus(t1, toll__english=0.05, door_fee=20)),
            ("wages 0.375 d a man a day", plus(t1, toll__english=0.05, pay=0.375)),
            ("wages 0.5 d a man a day", plus(t1, toll__english=0.05, pay=0.5)),
            ("the spread 0.30", plus(t1, toll__english=0.05, spread=0.3)),
            ("Frankish wine: made 2 at Quentovic, wanted 2 in England", plus(t1, toll__english=0.05, made__frankish_wine__neustria=2, wanted__frankish_wine__england_south=2)),
            ("two casks aboard at the start, not four", plus(t1, toll__english=0.05, start_cargo="rhenish_wine:2,quernstones:1,ships_bread:2")),
            ("a landing due of 1 d at every call", plus(t1, toll__english=0.05, call_fee=1.0)),
            ("a toll of 0.05 in the lands of weighed silver (Ripa)", plus(t1, toll__english=0.05, toll__weight=0.05)),
        ], half, pool, out)
        out("## 6. AFTER: the tuned set")
        run_config("tuned, 1 May to Michaelmas", t1, seeds, kinds, pool, out)
        run_config("tuned, 21 June to Michaelmas (the start in the book and in the code)", t2, seeds, kinds, pool, out)
        out("## 7. How firm the tuned set is (the sensible player, %d seasons each)" % len(half))
        ladder("What this script had to guess, moved either way (1 May to Michaelmas)", [
            ("THE TUNED SET", t1),
            ("other traders x0.75 (42 ships, not 56)", plus(t1, field=0.75)),
            ("other traders x1.25 (70 ships)", plus(t1, field=1.25)),
            ("no talks at all", plus(t1, talks=0)),
            ("every price free at each call (the code today)", plus(t1, talks=12)),
            ("he wants 10 % over what he pays, not 15 %", plus(t1, safety=1.10)),
            ("he wants 20 % over what he pays", plus(t1, safety=1.20)),
            ("the famine off", plus(t1, famine=0)),
        ], half, pool, out)
        ladder("The dial to turn when another rule is built: the English toll", [
            ("1 May, no reeve, toll 0.10  = THE TUNED SET", t1),
            ("1 May, no reeve, toll 0.075", plus(t1, toll__english=0.075)),
            ("1 May, the reeve takes 20 d, toll 0.10", plus(t1, door_fee=20)),
            ("1 May, the reeve takes 20 d, toll 0.075", plus(t1, door_fee=20, toll__english=0.075)),
            ("1 May, the reeve takes 20 d, toll 0.05", plus(t1, door_fee=20, toll__english=0.05)),
            ("21 June, no reeve, toll 0.10", plus(t2)),
            ("21 June, no reeve, toll 0.075", plus(t2, toll__english=0.075)),
            ("21 June, no reeve, toll 0.05", plus(t2, toll__english=0.05)),
            ("21 June, the reeve takes 20 d, toll 0.05", plus(t2, door_fee=20, toll__english=0.05)),
        ], half, pool, out)
        ladder("The perfect player and the law (the tuned set)", [
            ("keeps the law", t1),
            ("carries swords, mail and helmets too", plus(t1, smuggle=1)),
        ], half[:100], pool, out, kind="perfect")
    else:
        sets = dict(CONFIGS.get(a.config, {}))
        sets.update(extra)
        sums = run_config(a.config, sets, seeds, kinds, pool, out, quiet=a.brief)
        if a.brief:
            brief(kinds, sums, out)
    if pool:
        pool.close()


if __name__ == "__main__":
    main()
