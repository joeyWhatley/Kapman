using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>A kind of craft (Data/ship_classes_793.json): its size, crew, cargo and how she goes under sail and oars.</summary>
    public sealed class ShipClassDef
    {
        public string id, name;
        public float length, beam, draught, cargoT, crew, sailKn, maxKn, oarsKn, closestToWind, polar, galeFragility, rowingHours;
        /// <summary>Metres above the water: her sail top (0: no mast), her hull's highest point, a man standing aboard.</summary>
        public float sailTop, hullTop, lookoutEye;
        /// <summary>The height a lookout sees of her first: her sail top, or her hull if she has no mast.</summary>
        public float SeenHeight => sailTop > 0f ? sailTop : hullTop;
        public bool sea, coast, river, lake, hasSail;
        public string[] uses = new string[0];

        public bool Uses(string use) => System.Array.IndexOf(uses, use) >= 0;
        public float CargoKg => cargoT * 1000f;
    }

    /// <summary>The traders a home port sends out (Data/traffic_793.json, long_distance): how many, in which classes.</summary>
    public sealed class HomeFleet
    {
        public string port;
        public int traders;
        public List<(string cls, float share)> classes = new List<(string, float)>();
    }

    /// <summary>
    /// THE WORLD'S TABLES for the simulation: the kinds of craft, how many traders each port sends out and in what, and
    /// the sailing season by sea (the share of ships out each month). Read once; plain data the headless run and the
    /// game share.
    /// </summary>
    public sealed class SimData
    {
        public readonly Dictionary<string, ShipClassDef> classes = new Dictionary<string, ShipClassDef>();
        public readonly List<HomeFleet> fleets = new List<HomeFleet>();
        /// <summary>Trade region -> the share of ships out at sea in each month (1-12), from its sea or river profile.</summary>
        public readonly Dictionary<string, float[]> season = new Dictionary<string, float[]>();
        /// <summary>Port id -> the visiting sea-going and river craft at its landing at the July peak (traffic_793.json
        /// harbours): what its berths are sized from (Seamanship).</summary>
        public readonly Dictionary<string, float> berthPeak = new Dictionary<string, float>();

        /// <summary>One kind of craft from its row: a row of ship_classes_793.json "classes", or a row of the roster
        /// story/ships_793.json "ships", which is written in the same fields (7 Oct: so the ships a house can own are the
        /// same ShipClassDef the world's ships sail by; Content/Ships). Missing fields take the same defaults as ever.</summary>
        public static ShipClassDef ClassFrom(object o)
        {
            var waters = Json.Arr(o, "waters") ?? new List<object>();
            var c = new ShipClassDef
            {
                id = Json.Str(o, "id"), name = Json.Str(o, "name"),
                length = (float)Json.Num(o, "length_m", 8), beam = (float)Json.Num(o, "beam_m", 2),
                draught = (float)Json.Num(o, "draught_m", 0.4), cargoT = (float)Json.Num(o, "cargo_t", 1),
                crew = (float)Json.Num(o, "crew", 4), sailKn = (float)Json.Num(o, "speed_sail_kn", 0),
                maxKn = (float)Json.Num(o, "speed_max_kn", 5), oarsKn = (float)Json.Num(o, "speed_oars_kn", 2),
                closestToWind = (float)Json.Num(o, "closest_to_wind_deg", 60), polar = (float)Json.Num(o, "polar_factor", 1),
                galeFragility = (float)Json.Num(o, "gale_fragility", 0.5), rowingHours = (float)Json.Num(o, "rowing_hours", 4),
                sailTop = (float)Json.Num(o, "sail_top_m", 0), hullTop = (float)Json.Num(o, "hull_top_m", 1), lookoutEye = (float)Json.Num(o, "lookout_eye_m", 1.8),
                sea = waters.Contains("sea"), coast = waters.Contains("coast"), river = waters.Contains("river"), lake = waters.Contains("lake"),
                uses = (Json.Arr(o, "uses") ?? new List<object>()).ConvertAll(u => u as string ?? "").ToArray(),
            };
            c.hasSail = c.sailKn > 0f;
            return c;
        }

        public static SimData Load(TextAsset classesJson, TextAsset trafficJson)
        {
            var d = new SimData();
            var classes = Json.Parse(classesJson.text);
            foreach (var o in Json.Arr(classes, "classes") ?? new List<object>())
            {
                var c = ClassFrom(o);
                if (!string.IsNullOrEmpty(c.id))
                    d.classes[c.id] = c;
            }

            var traffic = Json.Parse(trafficJson.text);
            foreach (var kv in Json.Obj(Json.Obj(traffic, "long_distance"), "ports") ?? new Dictionary<string, object>())
            {
                var f = new HomeFleet { port = kv.Key, traders = (int)Json.Num(kv.Value, "traders") };
                foreach (var c in Json.Obj(kv.Value, "classes") ?? new Dictionary<string, object>())
                    if (c.Value is double share)
                        f.classes.Add((c.Key, (float)share));
                if (f.traders > 0 && f.classes.Count > 0)
                    d.fleets.Add(f);
            }
            d.fleets.Sort((a, b) => string.CompareOrdinal(a.port, b.port));   // (the same order every run)
            foreach (var kv in Json.Obj(Json.Obj(traffic, "harbours"), "ports") ?? new Dictionary<string, object>())
                d.berthPeak[kv.Key] = (float)(Json.Num(kv.Value, "sea", 0) + Json.Num(kv.Value, "river", 0));
            var seasonRoot = Json.Obj(traffic, "season");
            foreach (var group in new[] { "river_profiles", "sea_profiles" })   // a sea profile wins over a river one
                foreach (var kv in Json.Obj(seasonRoot, group) ?? new Dictionary<string, object>())
                {
                    var m = Json.Arr(kv.Value, "m");
                    if (m == null || m.Count != 12)
                        continue;
                    var months = m.ConvertAll(x => x is double v ? (float)v : 0f).ToArray();
                    foreach (var r in Json.Arr(kv.Value, "regions") ?? new List<object>())
                        if (r is string region)
                            d.season[region] = months;
                }
            return d;
        }

        /// <summary>The share of a region's ships out in a month (1 in summer, low in winter); 1 if the table hasn't it.</summary>
        public float Season(string region, int month) =>
            region != null && season.TryGetValue(region, out var m) ? m[Mathf.Clamp(month, 1, 12) - 1] : 1f;
    }
}
