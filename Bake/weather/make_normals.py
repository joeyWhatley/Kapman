"""
THE CLIMATE NORMALS - the measured weather of the Dutch coast and river land, 1991-2020, from KNMI's open station data,
written as the game's climate_normals.json (the weather engine's layer 1: Design/weather_and_sea_state.md 1.1, 4.2).

Run with the bake's Python (it needs only the standard library and the network):

    Bake\\run.bat weather\\make_normals.py              fetch what is missing into Bake/weather/raw, then compute
    Bake\\run.bat weather\\make_normals.py --offline    compute from Bake/weather/raw only (no network)

What it does:
  1. downloads, for the four stations that stand for the game's Dutch place classes, the daily series and the hourly
     series 1991-2020 from the KNMI data service (POST to daggegevens / uurgegevens, fmt=csv, as documented at
     https://www.knmi.nl/kennis-en-datacentrum/achtergrond/data-ophalen-vanuit-een-script), gzipped into
     Bake/weather/raw/ with a SHA-256 of each file in raw/SHA256SUMS (a file already there is kept, not fetched again);
  2. computes per station and month: the hourly wind's Beaufort shares (as measured, and with the day's cycle divided
     out, which is what the engine maps its wind onto before it puts the day's cycle back), the wind's day cycle, the
     gust factor, the days whose strongest hour reached force 6-10, the gale and storm days a year, temperature (mean,
     minimum, maximum, the day's cycle), frost and ice days, fog and mist days, rain days and amounts, cloud, pressure,
     the wind rose of the days, and the day-to-day persistence of wind, temperature and pressure;
  3. writes climate_normals.json ("provisional": false) to Assets/Kapman/Data/Weather/ (what the game reads) and to
     G:/kapman_unity/data/weather/ (the bake's copy), and the tables as Markdown to Bake/weather/tables.md, stamped
     "regenerated on <date>", for the weather document (Tables A-E) to take.

Licence: KNMI's open data are free to reuse with attribution: the credit line "Source: KNMI" goes with the data
(written into the json's credits). The sea temperatures are not from KNMI: they stay marked as a guess until the
Rijkswaterstaat series is read (week 2 of the weather plan).
"""
import csv
import datetime
import gzip
import hashlib
import io
import json
import math
import os
import sys
import time
import urllib.parse
import urllib.request
from collections import defaultdict

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = os.path.join(HERE, "raw")
PROJECT = os.path.normpath(os.path.join(HERE, "..", ".."))
OUT_GAME = os.path.join(PROJECT, "Assets", "Kapman", "Data", "Weather", "climate_normals.json")
OUT_BAKE = os.path.join("G:" + os.sep, "kapman_unity", "data", "weather", "climate_normals.json")
OUT_TABLES = os.path.join(HERE, "tables.md")

DAY_URL = "https://www.daggegevens.knmi.nl/klimatologie/daggegevens"
HOUR_URL = "https://www.daggegevens.knmi.nl/klimatologie/uurgegevens"
DAY_VARS = "DDVEC:FG:FHX:FXX:TG:TN:TX:RH:NG:VVN:PG"
HOUR_VARS = "DD:FH:FX:T:VV:M"
FIRST_YEAR, LAST_YEAR = 1991, 2020
DECADES = [(1991, 2000), (2001, 2010), (2011, 2020)]

# the place classes and their stations (weather document 1.1); north_sea is the open sea: Hoek's wind
STATIONS = {
    330: ("coast", "Hoek van Holland"),
    235: ("wadden", "De Kooy (Den Helder)"),
    310: ("channel_nl", "Vlissingen"),
    260: ("river_land", "De Bilt"),
}
CLASS_POS = {   # where each class's station stands (degrees), so the engine can tell which class a place is
    "coast": (51.99, 4.12), "wadden": (52.93, 4.78), "channel_nl": (51.44, 3.60), "river_land": (52.10, 5.18),
    "north_sea": (52.40, 3.40),
}
# lower bounds (m/s) of Beaufort forces 1..12 (https://en.wikipedia.org/wiki/Beaufort_scale), as the data service rounds
BEAUFORT = [0.3, 1.6, 3.4, 5.5, 8.0, 10.8, 13.9, 17.2, 20.8, 24.5, 28.5, 32.7]
MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
SECTORS = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"]
# a guess (the weather document 1.1: the coastal Dutch sea bottoms near 4-5 C in late February - early March and peaks
# near 18 C in August), to be replaced by Rijkswaterstaat's measured monthly means (open data, "Bron: Rijkswaterstaat")
SEA_TEMP_GUESS = [6.0, 4.8, 5.2, 8.0, 11.5, 15.0, 17.5, 18.5, 17.0, 14.0, 10.5, 7.8]


# ----------------------------------------------------------------------------------------------- fetching

def fetch(url, fields, path, tries=4):
    """POSTs a query to the data service and keeps the CSV gzipped; returns False when the network or the service fails."""
    if os.path.exists(path):
        return True
    data = urllib.parse.urlencode(fields).encode("ascii")
    for attempt in range(tries):
        try:
            req = urllib.request.Request(url, data=data, headers={"User-Agent": "kapman-bake/1.0"})
            with urllib.request.urlopen(req, timeout=300) as r:
                body = r.read()
            text = body.decode("utf-8", "replace")
            if "SOURCE: ROYAL NETHERLANDS METEOROLOGICAL INSTITUTE" not in text or text.count("\n") < 100:
                raise IOError("the reply is not a KNMI series (%d bytes)" % len(body))
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with gzip.open(path + ".part", "wb") as f:
                f.write(body)
            os.replace(path + ".part", path)
            print("  fetched", os.path.basename(path), "%.1f MB" % (len(body) / 1e6))
            return True
        except Exception as e:  # the network, a timeout, a refusal: try again, then give up on this file
            print("  could not fetch %s (try %d): %s" % (os.path.basename(path), attempt + 1, e))
            time.sleep(5 * (attempt + 1))
    return False


def day_file(stn):
    return os.path.join(RAW, "knmi_day_%d_%d-%d.csv.gz" % (stn, FIRST_YEAR, LAST_YEAR))


def hour_file(stn, a, b):
    return os.path.join(RAW, "knmi_hour_%d_%d-%d.csv.gz" % (stn, a, b))


def fetch_all():
    ok = True
    for stn in STATIONS:
        ok &= fetch(DAY_URL, {"stns": stn, "vars": DAY_VARS, "start": "%d0101" % FIRST_YEAR, "end": "%d1231" % LAST_YEAR,
                              "fmt": "csv"}, day_file(stn))
        for a, b in DECADES:
            ok &= fetch(HOUR_URL, {"stns": stn, "vars": HOUR_VARS, "start": "%d010101" % a, "end": "%d123124" % b,
                                   "fmt": "csv"}, hour_file(stn, a, b))
    return ok


def write_sums():
    lines = []
    for name in sorted(os.listdir(RAW)):
        if not name.endswith(".csv.gz"):
            continue
        h = hashlib.sha256()
        with open(os.path.join(RAW, name), "rb") as f:
            for chunk in iter(lambda: f.read(1 << 20), b""):
                h.update(chunk)
        lines.append("%s  %s" % (h.hexdigest(), name))
    with open(os.path.join(RAW, "SHA256SUMS"), "w", newline="\n") as f:
        f.write("\n".join(lines) + "\n")


# ----------------------------------------------------------------------------------------------- reading

def read_csv(path):
    """The rows of a KNMI CSV as dicts of ints (None where blank); the header is the last comment line naming STN."""
    with gzip.open(path, "rt", encoding="utf-8", errors="replace") as f:
        text = f.read()
    header, rows = None, []
    for line in text.splitlines():
        if line.startswith("#"):
            if "STN" in line and "YYYYMMDD" in line:
                header = [h.strip() for h in line.lstrip("#").split(",")]
            continue
        if not line.strip() or header is None:
            continue
        parts = [p.strip() for p in line.split(",")]
        row = {}
        for k, v in zip(header, parts):
            row[k] = int(v) if v not in ("",) and v.lstrip("-").isdigit() else None
        row["YYYYMMDD"] = parts[1]
        rows.append(row)
    return rows


def beaufort(ms):
    f = 0
    for b in BEAUFORT:
        if ms >= b:
            f += 1
    return f


def mean(a):
    return sum(a) / len(a) if a else float("nan")


def pct(a, b):
    return 100.0 * a / b if b else 0.0


def lag1(values):
    """Lag-1 autocorrelation of a series with gaps (None), as anomalies from the series' own monthly means."""
    pairs = [(values[i - 1], values[i]) for i in range(1, len(values)) if values[i] is not None and values[i - 1] is not None]
    if len(pairs) < 10:
        return float("nan")
    mx = mean([p for p, q in pairs])
    my = mean([q for p, q in pairs])
    sxy = sum((p - mx) * (q - my) for p, q in pairs)
    sxx = sum((p - mx) ** 2 for p, q in pairs)
    syy = sum((q - my) ** 2 for p, q in pairs)
    return sxy / math.sqrt(sxx * syy) if sxx > 0 and syy > 0 else float("nan")


def station_normals(stn):
    days = read_csv(day_file(stn))
    hours = []
    for a, b in DECADES:
        hours += read_csv(hour_file(stn, a, b))
    years = LAST_YEAR - FIRST_YEAR + 1

    bym = defaultdict(list)
    for r in days:
        bym[int(r["YYYYMMDD"][4:6]) - 1].append(r)
    hbym = defaultdict(list)
    for r in hours:
        hbym[int(r["YYYYMMDD"][4:6]) - 1].append(r)

    months = []
    for m in range(12):
        rs, hs = bym[m], hbym[m]
        fg = [r["FG"] / 10.0 for r in rs if r["FG"] is not None]
        fhx = [r["FHX"] / 10.0 for r in rs if r["FHX"] is not None]
        tg = [r["TG"] / 10.0 for r in rs if r["TG"] is not None]
        tn = [r["TN"] / 10.0 for r in rs if r["TN"] is not None]
        tx = [r["TX"] / 10.0 for r in rs if r["TX"] is not None]
        vvn = [r["VVN"] for r in rs if r["VVN"] is not None]
        rh = [r["RH"] for r in rs if r["RH"] is not None]
        ng = [r["NG"] for r in rs if r["NG"] is not None and r["NG"] <= 8]
        pg = [r["PG"] / 10.0 for r in rs if r["PG"] is not None]

        # the hourly wind: its day cycle (the mean by UT hour over the month's mean), the shares by force as measured and
        # with the day's cycle divided out, the gust factor in a fresh wind
        fh_by_hour = defaultdict(list)
        t_by_hour = defaultdict(list)
        for r in hs:
            if r["FH"] is not None:
                fh_by_hour[r["HH"] - 1].append(r["FH"] / 10.0)
            if r["T"] is not None:
                t_by_hour[r["HH"] - 1].append(r["T"] / 10.0)
        fh_all = [v for h in range(24) for v in fh_by_hour[h]]
        fh_mean = mean(fh_all)
        wind_by_hour = [mean(fh_by_hour[h]) / fh_mean if fh_by_hour[h] else 1.0 for h in range(24)]
        t_all = [v for h in range(24) for v in t_by_hour[h]]
        t_mean_h = mean(t_all) if t_all else float("nan")
        temp_by_hour = [mean(t_by_hour[h]) - t_mean_h if t_by_hour[h] else 0.0 for h in range(24)]
        shares = [0] * 13
        steady = [0] * 13
        for h in range(24):
            for v in fh_by_hour[h]:
                shares[beaufort(v)] += 1
                steady[beaufort(v / wind_by_hour[h])] += 1
        n = max(1, len(fh_all))
        gust = [r["FX"] / r["FH"] for r in hs if r["FH"] and r["FX"] and r["FH"] >= 50]

        months.append({
            "windMean": round(fh_mean, 2),
            "dailyWindMean": round(mean(fg), 2),
            "beaufort": [round(pct(c, n), 2) for c in shares],
            "beaufortNoDay": [round(pct(c, n), 2) for c in steady],
            "windByHour": [round(x, 3) for x in wind_by_hour],
            "gustFactor": round(mean(gust), 2) if gust else 1.55,
            "daysF6": round(pct(sum(1 for x in fhx if x >= 10.8), len(fhx)), 1),
            "daysF7": round(pct(sum(1 for x in fhx if x >= 13.9), len(fhx)), 1),
            "daysF8": round(pct(sum(1 for x in fhx if x >= 17.2), len(fhx)), 2),
            "daysF9": round(pct(sum(1 for x in fhx if x >= 20.8), len(fhx)), 2),
            "daysF10": round(pct(sum(1 for x in fhx if x >= 24.5), len(fhx)), 2),
            "tMean": round(mean(tg), 2),
            "tMin": round(mean(tn), 2),
            "tMax": round(mean(tx), 2),
            "tempByHour": [round(x, 2) for x in temp_by_hour],
            "frostDays": round(sum(1 for x in tn if x < 0) / years, 1),
            "iceDays": round(sum(1 for x in tx if x < 0) / years, 1),
            "fogDays": round(sum(1 for v in vvn if v < 10) / years, 1),
            "mistDays": round(sum(1 for v in vvn if 10 <= v < 20) / years, 1),
            "rainDays": round(sum(1 for x in rh if x >= 10) / years, 1),
            "rainMm": round(sum(max(x, 0) for x in rh) / 10.0 / years, 1),
            "cloudOctas": round(mean(ng), 2) if ng else None,
            "clearDays": round(pct(sum(1 for x in ng if x <= 2), len(ng)), 1) if ng else None,
            "dullDays": round(pct(sum(1 for x in ng if x >= 7), len(ng)), 1) if ng else None,
            "pressure": round(mean(pg), 1) if pg else None,
            "rose": rose(rs),
        })

    by_year = defaultdict(lambda: [0, 0, 0])
    for r in days:
        if r["FHX"] is None:
            continue
        y = r["YYYYMMDD"][:4]
        by_year[y][0] += r["FHX"] >= 172
        by_year[y][1] += r["FHX"] >= 208
        by_year[y][2] += r["FHX"] >= 245
    ys = sorted(by_year)
    month_mean = {m: {} for m in range(12)}

    def anomalies(key, scale):
        mm = {}
        for m in range(12):
            v = [r[key] / scale for r in bym[m] if r[key] is not None]
            mm[m] = mean(v)
        return [(r[key] / scale - mm[int(r["YYYYMMDD"][4:6]) - 1]) if r[key] is not None else None for r in days]

    pg = sorted(r["PG"] / 10.0 for r in days if r["PG"] is not None)
    year = {
        "galeDays": round(mean([by_year[y][0] for y in ys]), 2),
        "stormDays": round(mean([by_year[y][1] for y in ys]), 2),
        "f10Days": round(mean([by_year[y][2] for y in ys]), 3),
        "lag1Wind": round(lag1(anomalies("FG", 10.0)), 2),
        "lag1Temp": round(lag1(anomalies("TG", 10.0)), 2),
        "lag1Pressure": round(lag1(anomalies("PG", 10.0)), 2) if pg else None,
        "pressureP1": round(pg[len(pg) // 100], 1) if pg else None,
        "pressureP99": round(pg[len(pg) * 99 // 100], 1) if pg else None,
        "hours": len(hours),
        "days": len(days),
    }
    return months, year


def rose(rs):
    cnt = [0] * 8
    n = 0
    for r in rs:
        d = r["DDVEC"]
        if d is None or d == 0 or d >= 990:
            continue
        cnt[int(((d + 22.5) % 360) // 45)] += 1
        n += 1
    return [round(pct(c, n), 1) for c in cnt]


# ----------------------------------------------------------------------------------------------- writing

def tables_md(classes, stamp):
    out = ["# Climate normals 1991-2020 (KNMI)", "", "Regenerated on %s by Bake/weather/make_normals.py from the raw files in" % stamp,
           "Bake/weather/raw (SHA256SUMS). Source: KNMI. These are the weather document's Tables A-E as measured.", ""]
    for cid, c in classes.items():
        if c.get("derivedFrom"):
            continue
        out += ["## %s: %s (station %d)" % (cid, c["station"], c["stationId"]), ""]
        out += ["| Month | hourly mean m/s | daily mean | %days F6+ / F7+ / F8+ / F9+ | hours by force F0-1 F2 F3 F4 F5 F6 F7 F8+ | "
                "mean / min / max C | frost / ice d | fog / mist d | rain d / mm | NG, %clear, %dull | rose N NE E SE S SW W NW |",
                "|---|---|---|---|---|---|---|---|---|---|---|"]
        for m, r in enumerate(c["months"]):
            b = r["beaufort"]
            force = [b[0] + b[1]] + b[2:8] + [sum(b[8:])]
            out.append("| %s | %.2f | %.2f | %.0f / %.0f / %.1f / %.1f | %s | %.1f / %.1f / %.1f | %.1f / %.1f | %.1f / %.1f | %.1f / %.0f | %s, %s, %s | %s |" % (
                MONTHS[m], r["windMean"], r["dailyWindMean"], r["daysF6"], r["daysF7"], r["daysF8"], r["daysF9"],
                " ".join("%.1f" % x for x in force), r["tMean"], r["tMin"], r["tMax"], r["frostDays"], r["iceDays"],
                r["fogDays"], r["mistDays"], r["rainDays"], r["rainMm"], r["cloudOctas"], r["clearDays"], r["dullDays"],
                " ".join("%.0f" % x for x in r["rose"])))
        y = c["year"]
        out += ["", "A year: %.1f gale days (strongest hour F8+), %.2f storm days (F9+), %.3f F10+ days; lag-1 day "
                    "autocorrelation wind %.2f, temperature %.2f, pressure %s." % (y["galeDays"], y["stormDays"], y["f10Days"],
                                                                                 y["lag1Wind"], y["lag1Temp"], y["lag1Pressure"]), ""]
    return "\n".join(out) + "\n"


def main():
    offline = "--offline" in sys.argv
    os.makedirs(RAW, exist_ok=True)
    if not offline:
        print("Fetching the KNMI series (kept in Bake/weather/raw)...")
        if not fetch_all():
            print("Some series could not be fetched: the normals are NOT written; the provisional file stays as it is.")
            return 2
    missing = [p for stn in STATIONS for p in [day_file(stn)] + [hour_file(stn, a, b) for a, b in DECADES] if not os.path.exists(p)]
    if missing:
        print("Missing raw files (run without --offline):", ", ".join(os.path.basename(p) for p in missing))
        return 2
    write_sums()

    classes = {}
    for stn, (cid, name) in STATIONS.items():
        print("Computing", name, "...")
        months, year = station_normals(stn)
        lat, lon = CLASS_POS[cid]
        classes[cid] = {"station": name, "stationId": stn, "lat": lat, "lon": lon, "months": months, "year": year}
    # the open sea: Hoek van Holland's measured wind and weather (the windiest mainland station, on the sea wall), with
    # the sea's own temperature (the engine damps the day's cycle over water)
    sea = json.loads(json.dumps(classes["coast"]))
    sea["derivedFrom"] = "coast"
    sea["lat"], sea["lon"] = CLASS_POS["north_sea"]
    classes["north_sea"] = sea

    stamp = datetime.date.today().isoformat()
    doc = {
        "provisional": False,
        "regenerated": stamp,
        "period": "%d-%d" % (FIRST_YEAR, LAST_YEAR),
        "about": "Monthly climate normals by place class, measured. Made by Bake/weather/make_normals.py from KNMI's open "
                 "station data (raw files and SHA-256 sums in Bake/weather/raw). Wind in m/s at 10 m; beaufort = % of hours "
                 "at force 0..12 (beaufortNoDay: the same with the day's cycle divided out); windByHour and tempByHour by "
                 "UT hour 0-23 (the hour ending at 1..24 UT); days* = % of days whose strongest hourly mean reached the force; "
                 "temperatures C; days a month; rose = % of days by the day's vector-mean wind direction (N NE E SE S SW W NW).",
        "beaufortLowerBounds": BEAUFORT,
        "classes": classes,
        "seaTemp": {"months": SEA_TEMP_GUESS, "source": "guess (weather document 1.1); to be replaced by Rijkswaterstaat's monthly means"},
        "yearOffset": {
            "about": "The user's decision 17 (4 Oct 2026): the climate of 793 is the modern normals cooled by 0.8 C, warming "
                     "into the Medieval Climate Anomaly by about 950 (summers 0.3 C over the modern normals, winters as now); "
                     "Eldgja's haze cooled 939-942 (weather document 1.2).",
            "earlyOffset": -0.8, "earlyUntil": 900, "warmBy": 950, "mcaSummer": 0.3, "mcaWinter": 0.0,
            "events": [{"id": "939-eldgja", "from": 939.5, "to": 942.5, "offset": -1.2}],
        },
        "credits": [
            {"what": "station series 330, 235, 310, 260 (1991-2020)", "licence": "KNMI open data, free reuse with attribution",
             "credit": "Source: KNMI", "url": "https://www.daggegevens.knmi.nl/"},
        ],
    }
    text = json.dumps(doc, indent=1, ensure_ascii=False)
    for path in (OUT_GAME, OUT_BAKE):
        try:
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, "w", encoding="utf-8", newline="\n") as f:
                f.write(text + "\n")
            print("Wrote", path)
        except OSError as e:
            print("Could not write", path, ":", e)
    with open(OUT_TABLES, "w", encoding="utf-8", newline="\n") as f:
        f.write(tables_md(classes, stamp))
    print("Wrote", OUT_TABLES)
    return 0


if __name__ == "__main__":
    sys.exit(main())
