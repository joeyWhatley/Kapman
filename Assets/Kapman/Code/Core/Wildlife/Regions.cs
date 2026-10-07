using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>The climate the wildlife model runs on (regions.json "climate", decision 17): the warm index's ramp into the
    /// Medieval Climate Anomaly and the thresholds of a hard winter's die in cool and warm years.</summary>
    public sealed class Climate
    {
        /// <summary>The first year the year state is reckoned from (the crash-and-recovery of the herons runs from here).</summary>
        public int firstYear = 793;
        /// <summary>The warm index is 0 up to this year and 1 from warmTo, linear between.</summary>
        public int warmFrom = 900, warmTo = 950;
        /// <summary>A winter is hard (HW > 0) when the year's die passes this in a cool year, or hardWinterWarm in a warm one.</summary>
        public double hardWinterCold = 0.85, hardWinterWarm = 0.9;
        /// <summary>The die's ramp from 0 to a full hard winter (HW = 1).</summary>
        public double hardWinterWidth = 0.15;

        /// <summary>The warm index M(y): 0 in the cool years, 1 in the warm, linear between (decision 17).</summary>
        public double WarmIndex(int year)
        {
            if (year <= warmFrom)
                return 0.0;
            if (year >= warmTo || warmTo <= warmFrom)
                return 1.0;
            return (year - warmFrom) / (double)(warmTo - warmFrom);
        }
    }

    /// <summary>One wildlife region: a land region (ports and river places) or a sea (ocean levels), as a rough lat/lon
    /// polygon with the chronicle's finer region ids it answers to and its habitat shares.</summary>
    public sealed class Region
    {
        public string id = "", name = "";
        /// <summary>"land" or "sea".</summary>
        public string kind = "land";
        /// <summary>The corners, (latitude, longitude) in degrees.</summary>
        public readonly List<(double lat, double lon)> polygon = new List<(double lat, double lon)>();
        public readonly List<string> chronicleIds = new List<string>();
        public Dictionary<string, double> habitats = new Dictionary<string, double>();

        public bool IsSea => kind == "sea";

        /// <summary>The share (0-1) of a habitat in the region; 0 for one it does not name.</summary>
        public double Habitat(string habitat) => habitats.TryGetValue(habitat ?? "", out var v) ? v : 0.0;

        /// <summary>Does a chronicle entry's region list touch this region (its own id or one of its chronicle ids)?</summary>
        public bool MatchesChronicle(IEnumerable<string> ids)
        {
            if (ids == null)
                return false;
            foreach (var c in ids)
                if (c == id || chronicleIds.Contains(c))
                    return true;
            return false;
        }

        /// <summary>Is the point inside the polygon (even-odd rule on the lat/lon plane: good enough at this size)?</summary>
        public bool Contains(double lat, double lon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.lat > lat) != (b.lat > lat) && lon < (b.lon - a.lon) * (lat - a.lat) / (b.lat - a.lat) + a.lon)
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>The mean of the corners (the fallback when a point is in no polygon of its kind).</summary>
        public (double lat, double lon) Centre
        {
            get
            {
                double la = 0.0, lo = 0.0;
                foreach (var p in polygon)
                {
                    la += p.lat;
                    lo += p.lon;
                }
                int n = Math.Max(1, polygon.Count);
                return (la / n, lo / n);
            }
        }

        public override string ToString() => id;
    }

    /// <summary>
    /// THE REGIONS (regions.json; Design/wildlife_and_seasons.md 2.1): where a level or a port is, as the wildlife sees
    /// it. A level asks with its kind, because the land and the sea overlap on purpose (Katwijk is in the delta and in the
    /// southern North Sea): a river place or a port asks for land, an ocean level for sea.
    /// </summary>
    public sealed class RegionTable
    {
        public const string FileName = "regions.json";

        public readonly List<Region> regions = new List<Region>();
        public Climate climate = new Climate();

        public static RegionTable Load(string path) => Parse(File.ReadAllText(path));

        public static RegionTable Parse(string json)
        {
            var root = WildlifeJson.Root(json, FileName);
            var t = new RegionTable();
            var c = Json.Obj(root, "climate");
            if (c != null)
            {
                t.climate.firstYear = (int)Json.Num(c, "firstYear", t.climate.firstYear);
                t.climate.warmFrom = (int)Json.Num(c, "warmFrom", t.climate.warmFrom);
                t.climate.warmTo = (int)Json.Num(c, "warmTo", t.climate.warmTo);
                t.climate.hardWinterCold = Json.Num(c, "hardWinterCold", t.climate.hardWinterCold);
                t.climate.hardWinterWarm = Json.Num(c, "hardWinterWarm", t.climate.hardWinterWarm);
                t.climate.hardWinterWidth = Json.Num(c, "hardWinterWidth", t.climate.hardWinterWidth);
            }
            var list = Json.Arr(root, "regions") ?? throw new FormatException(FileName + ": 'regions' is missing");
            foreach (var o in list)
            {
                string id = WildlifeJson.Need(o, "id", FileName + " region");
                string what = FileName + " region " + id;
                var r = new Region
                {
                    id = id,
                    name = Json.Str(o, "name", id),
                    kind = Json.Str(o, "kind", "land"),
                    habitats = WildlifeJson.Numbers(o, "habitats"),
                };
                if (r.kind != "land" && r.kind != "sea")
                    throw new FormatException(what + ": kind must be land or sea, not " + r.kind);
                var poly = Json.Arr(o, "polygon");
                if (poly == null || poly.Count < 3)
                    throw new FormatException(what + ": a polygon of at least three [lat, lon] corners is needed");
                foreach (var corner in poly)
                    r.polygon.Add(WildlifeJson.Pair(corner, what + " polygon"));
                r.chronicleIds.AddRange(WildlifeJson.Strings(o, "chronicleIds"));
                if (t.Get(id) != null)
                    throw new FormatException(what + ": the id is used twice");
                t.regions.Add(r);
            }
            return t;
        }

        /// <summary>A region by id, or null.</summary>
        public Region Get(string id)
        {
            foreach (var r in regions)
                if (r.id == id)
                    return r;
            return null;
        }

        /// <summary>The region of a point: the first polygon of the kind ("land" or "sea") holding it, else the region of
        /// that kind whose centre is nearest (distance on the lat/lon plane, longitude shrunk by the latitude's cosine).</summary>
        public Region At(double lat, double lon, string kind)
        {
            foreach (var r in regions)
                if (r.kind == kind && r.Contains(lat, lon))
                    return r;
            Region best = null;
            double bestD = double.MaxValue, k = Math.Cos(lat * Math.PI / 180.0);
            foreach (var r in regions)
            {
                if (r.kind != kind)
                    continue;
                var c = r.Centre;
                double dLat = c.lat - lat, dLon = (c.lon - lon) * k;
                double d = dLat * dLat + dLon * dLon;
                if (d < bestD)
                {
                    bestD = d;
                    best = r;
                }
            }
            return best;
        }

        /// <summary>A level's region: its own override when it names a known region (the Thames mouth, the Limfjord), else
        /// by its latitude, longitude and kind. An override naming no region is an error, not a quiet fallback.</summary>
        public Region Resolve(string overrideId, double lat, double lon, string kind)
        {
            if (!string.IsNullOrEmpty(overrideId))
                return Get(overrideId) ?? throw new ArgumentException("No wildlife region '" + overrideId + "' in " + FileName);
            return At(lat, lon, kind);
        }
    }
}
