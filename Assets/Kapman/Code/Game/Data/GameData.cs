using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>A trading place of the time: a port the ship can put in at (Data/ports_793.json).</summary>
    [Serializable]
    public class PortDef
    {
        [Tooltip("Short id, used everywhere (e.g. dorestad).")]
        public string id;
        [Tooltip("The name then, as its people or its charters had it.")]
        public string name;
        [Tooltip("The place today.")]
        public string modern;
        public double lat, lon;
        [Tooltip("Whose it is in 793: a realm id (Data/realms_793.json, to come) or a people.")]
        public string realm;
        [Tooltip("The county or land it lies in (an id from Build/Guide/lands_793.json).")]
        public string county;
        [Tooltip("Its trade region: what it makes and wants (Data/goods_793.json), and its money, tolls and laws (Data/trade_rules.json).")]
        public string region;
        [Tooltip("emporium, town, monastery, royal seat, fort, market, village.")]
        public string kind;
        [Tooltip("How a ship comes in: beach, jetty, quay, riverbank.")]
        public string landing;
        [Tooltip("1 a landing place ... 5 a great emporium: how many buildings its blockout gets.")]
        public int size = 2;
        [Tooltip("Years it's a trading place (0 = always / no end).")]
        public int from, to;
        public string note;

        /// <summary>Its place in the ports table (the same every run: the world simulation's arrays use it).</summary>
        [NonSerialized] public int index;

        public bool Exists(int year) => (from == 0 || from <= year) && (to == 0 || to >= year);
        public string Title => string.IsNullOrEmpty(modern) ? name : $"{name} ({modern})";
    }

    [Serializable]
    class PortFile
    {
        public string about;
        public PortDef[] ports = new PortDef[0];
    }

    /// <summary>A good of the time (Data/goods_793.json): what it is, how it's traded, where it's made and wanted.</summary>
    public sealed class GoodDef
    {
        public string id, name, period, cat, unit, season, risk, note, law;
        /// <summary>One unit's weight (kg), and hold space per kg (1 dense ... 4 light and bulky).</summary>
        public float kg, bulk;
        /// <summary>Deniers for one unit where it's neither made nor wanted.</summary>
        public float value;
        /// <summary>Days before it spoils (0: it keeps); years it's traded (0: always / no end).</summary>
        public int keeps, from, to;
        /// <summary>Trade regions that make it and want it, each with a strength (made 3 the main source ... 0.5
        /// brought in; wanted 3 keenly ... 1).</summary>
        public readonly Dictionary<string, float> made = new Dictionary<string, float>(), wanted = new Dictionary<string, float>();
        /// <summary>The months (1-12) it's cheapest and dearest.</summary>
        public int[] cheap = new int[0], dear = new int[0];
        public string[] sources = new string[0];

        /// <summary>Its place in the goods table (the same every run).</summary>
        public int index;

        public bool Traded(int year) => (from == 0 || from <= year) && (to == 0 || to >= year);
        /// <summary>Hold space one unit takes (kg x bulk).</summary>
        public float Space => kg * Mathf.Max(0.1f, bulk);
        public float MadeIn(string region) => region != null && made.TryGetValue(region, out var s) ? s : 0f;
        public float WantedIn(string region) => region != null && wanted.TryGetValue(region, out var s) ? s : 0f;
    }

    /// <summary>
    /// GAME DATA - the tables the game reads (ports, and as they come: goods, realms, events, texts). Loaded once
    /// from the JSON files the Game object holds; read anywhere (graphs read them through these lists).
    /// </summary>
    public static class GameData
    {
        static readonly List<PortDef> ports = new List<PortDef>();
        static readonly Dictionary<string, PortDef> portById = new Dictionary<string, PortDef>();
        static readonly List<GoodDef> goods = new List<GoodDef>();
        static readonly Dictionary<string, GoodDef> goodById = new Dictionary<string, GoodDef>();
        static readonly Dictionary<string, string> regions = new Dictionary<string, string>();
        static readonly Dictionary<string, string> sources = new Dictionary<string, string>();

        public static IReadOnlyList<PortDef> Ports => ports;
        public static IReadOnlyList<GoodDef> Goods => goods;
        /// <summary>Trade region id -> what it is ("The Rhine delta: Dorestad, Utrecht...").</summary>
        public static IReadOnlyDictionary<string, string> Regions => regions;
        /// <summary>Source id -> its reference (the goods' "src").</summary>
        public static IReadOnlyDictionary<string, string> Sources => sources;

        public static PortDef Port(string id) => id != null && portById.TryGetValue(id, out var p) ? p : null;
        public static GoodDef Good(string id) => id != null && goodById.TryGetValue(id, out var g) ? g : null;

        /// <summary>Reads the goods table (Data/goods_793.json).</summary>
        public static void LoadGoods(TextAsset json)
        {
            goods.Clear();
            goodById.Clear();
            regions.Clear();
            sources.Clear();
            if (json == null)
                return;
            var root = Json.Parse(json.text);
            madeFactor = Factors(Json.Obj(root, "made_factor"), madeFactor);
            wantedFactor = Factors(Json.Obj(root, "wanted_factor"), wantedFactor);
            foreach (var kv in Json.Obj(root, "regions") ?? new Dictionary<string, object>())
                regions[kv.Key] = kv.Value as string ?? "";
            foreach (var kv in Json.Obj(root, "sources") ?? new Dictionary<string, object>())
                sources[kv.Key] = kv.Value as string ?? "";
            foreach (var o in Json.Arr(root, "goods") ?? new List<object>())
            {
                var g = new GoodDef
                {
                    id = Json.Str(o, "id"), name = Json.Str(o, "name"), period = Json.Str(o, "period"),
                    cat = Json.Str(o, "cat"), unit = Json.Str(o, "unit"), season = Json.Str(o, "season"),
                    risk = Json.Str(o, "risk"), note = Json.Str(o, "note"), law = Json.Str(o, "law"),
                    kg = (float)Json.Num(o, "kg", 1), bulk = (float)Json.Num(o, "bulk", 1), value = (float)Json.Num(o, "value", 1),
                    keeps = (int)Json.Num(o, "keeps"), from = (int)Json.Num(o, "from"), to = (int)Json.Num(o, "to"),
                };
                if (string.IsNullOrEmpty(g.id) || goodById.ContainsKey(g.id))
                    continue;
                foreach (var kv in Json.Obj(o, "made") ?? new Dictionary<string, object>())
                    if (kv.Value is double v) g.made[kv.Key] = (float)v;
                foreach (var kv in Json.Obj(o, "wanted") ?? new Dictionary<string, object>())
                    if (kv.Value is double v) g.wanted[kv.Key] = (float)v;
                g.cheap = Months(Json.Arr(o, "cheap"));
                g.dear = Months(Json.Arr(o, "dear"));
                var src = Json.Arr(o, "src");
                if (src != null)
                    g.sources = src.ConvertAll(s => s as string ?? "").ToArray();
                g.index = goods.Count;
                goods.Add(g);
                goodById[g.id] = g;
            }
        }

        // a good's price where it's made / wanted, by strength (the goods table's made_factor and wanted_factor)
        static SortedList<float, float> madeFactor = new SortedList<float, float> { { 0.5f, 1.4f }, { 1f, 0.8f }, { 2f, 0.65f }, { 3f, 0.5f } };
        static SortedList<float, float> wantedFactor = new SortedList<float, float> { { 1f, 1.15f }, { 2f, 1.4f }, { 3f, 1.7f } };

        /// <summary>A good's price where it's made this strongly (0: not made there), times its value.</summary>
        public static float MadeFactor(float strength) => strength <= 0f ? 1f : Interpolate(madeFactor, strength);
        /// <summary>A good's price where it's wanted this keenly (0: not wanted there), times its value.</summary>
        public static float WantedFactor(float strength) => strength <= 0f ? 1f : Interpolate(wantedFactor, strength);

        static float Interpolate(SortedList<float, float> table, float x)
        {
            var keys = table.Keys;
            if (x <= keys[0]) return table.Values[0];
            for (int i = 1; i < keys.Count; i++)
                if (x <= keys[i])
                    return Mathf.Lerp(table.Values[i - 1], table.Values[i], (x - keys[i - 1]) / (keys[i] - keys[i - 1]));
            return table.Values[keys.Count - 1];
        }

        static SortedList<float, float> Factors(Dictionary<string, object> obj, SortedList<float, float> fallback)
        {
            if (obj == null || obj.Count == 0)
                return fallback;
            var t = new SortedList<float, float>();
            foreach (var kv in obj)
                if (kv.Value is double v && float.TryParse(kv.Key, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var k))
                    t[k] = (float)v;
            return t.Count > 0 ? t : fallback;
        }

        static int[] Months(List<object> list)
        {
            if (list == null)
                return new int[0];
            var m = new List<int>();
            foreach (var o in list)
                if (o is double d) m.Add((int)d);
            return m.ToArray();
        }

        public static void LoadPorts(TextAsset json)
        {
            ports.Clear();
            portById.Clear();
            if (json == null)
                return;
            var file = JsonUtility.FromJson<PortFile>(json.text);
            foreach (var p in file?.ports ?? new PortDef[0])
            {
                if (string.IsNullOrEmpty(p.id) || portById.ContainsKey(p.id))
                    continue;
                p.index = ports.Count;
                ports.Add(p);
                portById[p.id] = p;
            }
        }
    }
}
