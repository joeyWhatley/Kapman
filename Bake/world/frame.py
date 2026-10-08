# -*- coding: utf-8 -*-
"""The game's world frame outside the game (Code/Core/World/WorldFrame.cs): Lambert conformal conic on WGS84, standard
parallels 45N and 65N, centred on 56N 5E; x east, z north, real metres. And the sea lanes' file (Code/Game/Sim/LaneGraph.cs)."""
import math, struct

A = 6378137.0
F = 1 / 298.257223563
E = math.sqrt(F * (2 - F))
D = math.pi / 180


def _m(p):
    s = math.sin(p)
    return math.cos(p) / math.sqrt(1 - E * E * s * s)


def _t(p):
    s = math.sin(p)
    return math.tan(math.pi / 4 - p / 2) / ((1 - E * s) / (1 + E * s)) ** (E / 2)


_n = (math.log(_m(45 * D)) - math.log(_m(65 * D))) / (math.log(_t(45 * D)) - math.log(_t(65 * D)))
_af = A * _m(45 * D) / (_n * _t(45 * D) ** _n)
_rho0 = _af * _t(56 * D) ** _n


def to_world(lat, lon):
    rho = _af * _t(lat * D) ** _n
    th = _n * (lon - 5) * D
    return rho * math.sin(th), _rho0 - rho * math.cos(th)


def to_latlon(x, z):
    dy = _rho0 - z
    rho = math.hypot(x, dy)
    th = math.atan2(x, dy)
    t = (rho / _af) ** (1 / _n)
    phi = math.pi / 2 - 2 * math.atan(t)
    for _ in range(15):
        s = math.sin(phi)
        phi = math.pi / 2 - 2 * math.atan(t * ((1 - E * s) / (1 + E * s)) ** (E / 2))
    return phi / D, 5 + th / _n / D


LANE_KINDS = ["sea", "coastal", "sound", "river", "pending river", "flats", "portage", "approach"]


def lanes(path="G:/kapman_unity/data/baked/world/lanes.bin"):
    """Every point of every lane's course: a list of (x, z, kind)."""
    with open(path, "rb") as f:
        b = f.read()
    assert b[:4] == b"KLN1", "not a lanes file"
    at = 4
    version, n, e, berths, gates, p = struct.unpack_from("<6i", b, at)
    at += 24
    assert version == 1
    at += n * 13                                                     # a node: int, float, float, byte
    points = []
    for _ in range(e):
        _a, _b, kind, _flags, _length, _depth, count = struct.unpack_from("<iiBBffi", b, at)
        at += 22
        xs = struct.unpack_from("<%df" % (count * 2), b, at)
        at += count * 8
        for k in range(count):
            points.append((xs[2 * k], xs[2 * k + 1], kind))
    return points
