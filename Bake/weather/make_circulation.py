"""
THE CIRCULATION CHAIN'S DATA - writes Assets/Kapman/Data/Weather/circulation.json: the day-to-day chain of circulation
types (after Lamb's types, simplified: Design/weather_and_sea_state.md 2.2) and the pressure systems each type places on
the map. Plain Python, no network:

    Bake\\run.bat weather\\make_circulation.py

How the twelve month matrices are made. Each type keeps itself from one day to the next with its persistence (the
document's starting values: a high or a block holds longest, a cyclonic day least); when it changes, it changes to the
others in proportion to a target share, and the shares are fitted (a fixed-point loop) so that the chain's stationary
distribution is the month's share of each type. The month's shares come from the measured wind rose of the days at Hoek
van Holland (Table C of the document, KNMI 1991-2020): the southerly sectors give S, half the south-west gives SW (the
storm track's days) and the other half with the west gives W, the north-west NW, the north N, the north-east and east E;
anticyclonic and cyclonic days take a fixed share first (more highs in the spring blocking season). These are starting
values: the fit loop of the thirty-year report (week 2) tunes them against the normals.
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Kapman", "Data", "Weather", "circulation.json"))

TYPES = ["A", "C", "W", "SW", "NW", "N", "E", "S"]
# persistence: the chance a type holds to the next day (weather document 2.2's starting matrix diagonal)
PERSIST = {"A": 0.55, "C": 0.35, "W": 0.45, "SW": 0.40, "NW": 0.40, "N": 0.45, "E": 0.55, "S": 0.40}

# Hoek van Holland, % of days by the day's vector-mean direction: N NE E SE S SW W NW (Table C, KNMI 1991-2020)
ROSE = [
    [4.8, 5.8, 11.2, 9.0, 17.2, 29.0, 15.5, 7.4],
    [5.5, 10.0, 10.1, 7.7, 15.7, 26.4, 15.2, 9.3],
    [9.7, 13.5, 10.1, 6.8, 10.3, 22.5, 18.0, 9.1],
    [16.9, 16.3, 9.5, 4.6, 10.7, 20.1, 12.3, 9.5],
    [19.5, 19.7, 9.8, 2.7, 7.1, 18.3, 12.4, 10.5],
    [19.3, 14.0, 4.1, 3.2, 5.8, 22.7, 16.3, 14.6],
    [15.2, 11.6, 4.7, 2.7, 4.4, 25.4, 22.6, 13.4],
    [11.9, 11.2, 5.5, 5.7, 8.4, 24.0, 20.0, 13.3],
    [11.8, 11.0, 9.8, 7.0, 12.6, 21.9, 15.3, 10.7],
    [5.6, 8.9, 12.4, 9.7, 20.9, 21.3, 14.2, 7.1],
    [5.7, 5.7, 8.8, 11.1, 25.3, 21.1, 13.0, 9.3],
    [5.2, 5.5, 13.0, 6.9, 20.6, 26.5, 15.7, 6.7],
]
# the share of anticyclonic days by month (more in the spring blocking season and late summer) and of cyclonic days
ANTICYCLONIC = [0.16, 0.16, 0.18, 0.22, 0.24, 0.20, 0.18, 0.18, 0.20, 0.17, 0.15, 0.15]
CYCLONIC = [0.10, 0.10, 0.09, 0.09, 0.08, 0.08, 0.09, 0.09, 0.09, 0.10, 0.11, 0.11]

# the systems each type places, given where they stand at the middle of their day: kind, latitude, longitude (degrees),
# depth (hPa against the season's mean field: + a high, - a low), radius (km), track bearing (degrees) and speed (km/h),
# the ridge behind a low (a share of its depth, as the model's own lows), and how far (km) and how much (share of the
# depth) a day's system may stray from the template
TEMPLATES = {
    "A": [{"kind": "high", "lat": 54.5, "lon": 4.0, "depth": 15.0, "radius": 1100, "bearing": 90, "speed": 8}],
    "C": [{"kind": "low", "lat": 55.0, "lon": 3.0, "depth": -15.0, "radius": 600, "bearing": 60, "speed": 20, "ridge": 0.2}],
    "W": [{"kind": "low", "lat": 61.0, "lon": 0.0, "depth": -18.0, "radius": 800, "bearing": 70, "speed": 45, "ridge": 0.4}],
    "SW": [{"kind": "low", "lat": 57.0, "lon": 1.0, "depth": -23.0, "radius": 750, "bearing": 55, "speed": 50, "ridge": 0.45}],
    "NW": [{"kind": "low", "lat": 60.0, "lon": 14.0, "depth": -16.0, "radius": 800, "bearing": 100, "speed": 35, "ridge": 0.0},
           {"kind": "high", "lat": 54.0, "lon": -14.0, "depth": 10.0, "radius": 1200, "bearing": 90, "speed": 10}],
    "N": [{"kind": "high", "lat": 57.0, "lon": -12.0, "depth": 14.0, "radius": 1200, "bearing": 100, "speed": 8},
          {"kind": "low", "lat": 62.0, "lon": 18.0, "depth": -12.0, "radius": 900, "bearing": 110, "speed": 20, "ridge": 0.0}],
    "E": [{"kind": "high", "lat": 62.0, "lon": 14.0, "depth": 16.0, "radius": 1200, "bearing": 90, "speed": 5}],
    "S": [{"kind": "high", "lat": 53.0, "lon": 20.0, "depth": 12.0, "radius": 1300, "bearing": 90, "speed": 6},
          {"kind": "low", "lat": 53.0, "lon": -14.0, "depth": -14.0, "radius": 800, "bearing": 30, "speed": 15, "ridge": 0.0}],
}
STRAY_KM = 300.0
STRAY_DEPTH = 0.25
# the air each type brings, C against the season's normal: [in midwinter, in midsummer] (weather document 2.3: E in winter
# -4, N -2, S +3, SW +2 in winter; the summer column a guess: continental E and S air hot, clear highs warm by day)
TYPE_TEMPERATURE = {"A": [-1.5, 1.5], "C": [0.0, -1.0], "W": [1.0, -0.5], "SW": [2.0, 0.0], "NW": [-1.0, -1.5],
                    "N": [-2.0, -2.0], "E": [-4.0, 2.0], "S": [3.0, 1.5]}


def shares(m):
    r = ROSE[m]
    s = {"S": r[4] + r[3], "SW": 0.5 * r[5], "W": 0.5 * r[5] + r[6], "NW": r[7], "N": r[0], "E": r[1] + r[2]}
    rest = 1.0 - ANTICYCLONIC[m] - CYCLONIC[m]
    total = sum(s.values())
    f = {k: v / total * rest for k, v in s.items()}
    f["A"] = ANTICYCLONIC[m]
    f["C"] = CYCLONIC[m]
    return [f[t] for t in TYPES]


def matrix(target, persist):
    """Off-diagonal targets g fitted so that the chain's stationary distribution is the target."""
    n = len(target)
    g = list(target)
    for _ in range(400):
        P = [[persist[i] if i == j else (1.0 - persist[i]) * g[j] / (1.0 - g[i]) for j in range(n)] for i in range(n)]
        pi = stationary(P)
        g = [max(1e-4, min(0.9, g[i] * (target[i] / pi[i]) ** 0.5)) for i in range(n)]
        total = sum(g)
        g = [x / total for x in g]
    P = [[persist[i] if i == j else (1.0 - persist[i]) * g[j] / (1.0 - g[i]) for j in range(n)] for i in range(n)]
    # each row's off-diagonal part renormalised so the row sums to 1 (before rounding)
    out = []
    for i, row in enumerate(P):
        off = sum(row[j] for j in range(n) if j != i)
        out.append([round(persist[i] if j == i else row[j] * (1.0 - persist[i]) / off, 4) for j in range(n)])
    return out, stationary(P)


def stationary(P):
    n = len(P)
    pi = [1.0 / n] * n
    for _ in range(2000):
        pi = [sum(pi[i] * P[i][j] for i in range(n)) for j in range(n)]
    s = sum(pi)
    return [x / s for x in pi]


def main():
    persist = [PERSIST[t] for t in TYPES]
    months = []
    for m in range(12):
        target = shares(m)
        P, pi = matrix(target, persist)
        months.append({"shares": [round(x, 4) for x in target], "matrix": P})
        print("month %2d target %s stationary %s" % (m + 1, " ".join("%.3f" % x for x in target), " ".join("%.3f" % x for x in pi)))
    doc = {
        "about": "The circulation chain (weather document 2.2): one type a day from a month's 8x8 matrix (rows: today's type, "
                 "columns: tomorrow's, in the order of 'types'), interpolated between months by the day; the systems each "
                 "type places (where they stand at the middle of their day). Made by Bake/weather/make_circulation.py; "
                 "starting values, to be fitted by the thirty-year report.",
        "types": TYPES,
        "persistence": persist,
        "months": months,
        "templates": TEMPLATES,
        "strayKm": STRAY_KM,
        "strayDepth": STRAY_DEPTH,
        "typeTemperature": TYPE_TEMPERATURE,
        "dayKernelHours": 48.0,
        "winterDeepening": 0.25,
        "ownSystemsWeight": 0.6,
        "calibrationYears": 3,
        "calibrationPerDay": 8,
    }
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(doc, indent=1) + "\n")
    print("Wrote", OUT)


if __name__ == "__main__":
    main()
