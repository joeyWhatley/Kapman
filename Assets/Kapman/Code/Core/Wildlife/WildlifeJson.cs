using System;
using System.Collections.Generic;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>The small readers the wildlife tables share, on top of the game's Json: maps of numbers, lists of
    /// strings, (day, value) curves and (from, to) windows. A field the table must have and lacks is an error naming the
    /// table and the record, never a silent zero.</summary>
    static class WildlifeJson
    {
        public static Dictionary<string, object> Root(string json, string what)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new FormatException(what + ": the file is empty");
            if (!(Json.Parse(json) is Dictionary<string, object> root))
                throw new FormatException(what + ": the file is not a JSON object");
            return root;
        }

        public static string Need(object o, string key, string what)
        {
            string s = Json.Str(o, key);
            if (string.IsNullOrEmpty(s))
                throw new FormatException(what + ": '" + key + "' is missing");
            return s;
        }

        public static bool Has(object o, string key) => o is Dictionary<string, object> d && d.ContainsKey(key) && d[key] != null;

        /// <summary>{"a": 1.0, "b": 0.5} as a map; an absent field gives an empty map.</summary>
        public static Dictionary<string, double> Numbers(object o, string key)
        {
            var map = new Dictionary<string, double>();
            var obj = Json.Obj(o, key);
            if (obj == null)
                return map;
            foreach (var kv in obj)
                if (kv.Value is double v)
                    map[kv.Key] = v;
            return map;
        }

        /// <summary>["a", "b"] as a list; an absent field gives an empty list.</summary>
        public static List<string> Strings(object o, string key)
        {
            var list = new List<string>();
            var arr = Json.Arr(o, key);
            if (arr == null)
                return list;
            foreach (var x in arr)
                if (x is string s)
                    list.Add(s);
            return list;
        }

        /// <summary>A pair of numbers from a two-element array, e.g. a window [from, to] or a corner [lat, lon].</summary>
        public static (double a, double b) Pair(object item, string what)
        {
            if (item is List<object> l && l.Count >= 2 && l[0] is double a && l[1] is double b)
                return (a, b);
            throw new FormatException(what + ": a pair of numbers expected");
        }

        /// <summary>[[x, y], ...] as a list of pairs, sorted by x; null when the field is absent.</summary>
        public static List<(double x, double y)> Curve(List<object> arr, string what)
        {
            if (arr == null)
                return null;
            var list = new List<(double x, double y)>();
            foreach (var item in arr)
                list.Add(Pair(item, what));
            list.Sort((p, q) => p.x.CompareTo(q.x));
            return list;
        }

        /// <summary>A window [from, to] of whole days.</summary>
        public static (int from, int to) Window(object o, string key, string what)
        {
            var arr = Json.Arr(o, key);
            if (arr == null)
                throw new FormatException(what + ": '" + key + "' [from, to] is missing");
            var (a, b) = Pair(arr, what + " " + key);
            return ((int)Math.Round(a), (int)Math.Round(b));
        }
    }
}
