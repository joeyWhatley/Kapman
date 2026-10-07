using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kapman.Game
{
    /// <summary>
    /// A small JSON reader for the game's tables (Unity's own JsonUtility can't read objects used as maps, like a
    /// good's "made": {"frisia": 3, ...}). Objects come back as Dictionary&lt;string, object&gt;, arrays as
    /// List&lt;object&gt;, numbers as double, and true/false/null as themselves.
    /// </summary>
    public static class Json
    {
        public static object Parse(string text)
        {
            if (text == null)
                return null;
            int i = 0;
            var v = Value(text, ref i);
            return v;
        }

        // ---- reading values back out, forgivingly (a missing or odd field gives the fallback)

        public static Dictionary<string, object> Obj(object o, string key) =>
            o is Dictionary<string, object> d && d.TryGetValue(key, out var v) ? v as Dictionary<string, object> : null;

        public static List<object> Arr(object o, string key) =>
            o is Dictionary<string, object> d && d.TryGetValue(key, out var v) ? v as List<object> : null;

        public static string Str(object o, string key, string fallback = "") =>
            o is Dictionary<string, object> d && d.TryGetValue(key, out var v) && v is string s ? s : fallback;

        public static double Num(object o, string key, double fallback = 0.0) =>
            o is Dictionary<string, object> d && d.TryGetValue(key, out var v) && v is double n ? n : fallback;

        public static bool Bool(object o, string key, bool fallback = false) =>
            o is Dictionary<string, object> d && d.TryGetValue(key, out var v) && v is bool b ? b : fallback;

        // ---- writing (saved games): Dictionary<string, object>, lists, strings, numbers (exact: "R"), bools, null

        public static string Write(object value)
        {
            var sb = new StringBuilder();
            Emit(sb, value);
            return sb.ToString();
        }

        static void Emit(StringBuilder sb, object v)
        {
            switch (v)
            {
                case null: sb.Append("null"); return;
                case string s: Quote(sb, s); return;
                case bool b: sb.Append(b ? "true" : "false"); return;
                case double d: sb.Append(double.IsNaN(d) || double.IsInfinity(d) ? "null" : d.ToString("R", CultureInfo.InvariantCulture)); return;
                case float f: sb.Append(float.IsNaN(f) || float.IsInfinity(f) ? "null" : ((double)f).ToString("R", CultureInfo.InvariantCulture)); return;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); return;
                case long l: sb.Append(l.ToString(CultureInfo.InvariantCulture)); return;
                case Dictionary<string, object> d:
                {
                    sb.Append('{');
                    bool first = true;
                    foreach (var kv in d)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Quote(sb, kv.Key);
                        sb.Append(':');
                        Emit(sb, kv.Value);
                    }
                    sb.Append('}');
                    return;
                }
                case System.Collections.IEnumerable e:
                {
                    sb.Append('[');
                    bool first = true;
                    foreach (var x in e)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Emit(sb, x);
                    }
                    sb.Append(']');
                    return;
                }
                default:
                    sb.Append(Convert.ToString(v, CultureInfo.InvariantCulture));
                    return;
            }
        }

        static void Quote(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            sb.Append('"');
        }

        /// <summary>Floats packed as base64 (a saved price list), and back.</summary>
        public static string Floats(float[] a)
        {
            if (a == null)
                return "";
            var bytes = new byte[a.Length * 4];
            Buffer.BlockCopy(a, 0, bytes, 0, bytes.Length);
            return Convert.ToBase64String(bytes);
        }

        public static float[] Floats(string s)
        {
            var bytes = Convert.FromBase64String(s ?? "");
            var a = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, a, 0, a.Length * 4);
            return a;
        }

        // ---- the parser

        static object Value(string s, ref int i)
        {
            Space(s, ref i);
            if (i >= s.Length)
                throw new FormatException("JSON: unexpected end");
            char c = s[i];
            if (c == '{') return Object(s, ref i);
            if (c == '[') return Array(s, ref i);
            if (c == '"') return String(s, ref i);
            if (c == 't' && Word(s, ref i, "true")) return true;
            if (c == 'f' && Word(s, ref i, "false")) return false;
            if (c == 'n' && Word(s, ref i, "null")) return null;
            return Number(s, ref i);
        }

        static Dictionary<string, object> Object(string s, ref int i)
        {
            var d = new Dictionary<string, object>();
            i++;                                                     // {
            Space(s, ref i);
            if (i < s.Length && s[i] == '}')
            {
                i++;
                return d;
            }
            while (true)
            {
                Space(s, ref i);
                string key = String(s, ref i);
                Space(s, ref i);
                if (i >= s.Length || s[i] != ':')
                    throw new FormatException("JSON: ':' expected at " + i);
                i++;
                d[key] = Value(s, ref i);
                Space(s, ref i);
                if (i < s.Length && s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (i < s.Length && s[i] == '}')
                {
                    i++;
                    return d;
                }
                throw new FormatException("JSON: ',' or '}' expected at " + i);
            }
        }

        static List<object> Array(string s, ref int i)
        {
            var list = new List<object>();
            i++;                                                     // [
            Space(s, ref i);
            if (i < s.Length && s[i] == ']')
            {
                i++;
                return list;
            }
            while (true)
            {
                list.Add(Value(s, ref i));
                Space(s, ref i);
                if (i < s.Length && s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (i < s.Length && s[i] == ']')
                {
                    i++;
                    return list;
                }
                throw new FormatException("JSON: ',' or ']' expected at " + i);
            }
        }

        static string String(string s, ref int i)
        {
            if (s[i] != '"')
                throw new FormatException("JSON: string expected at " + i);
            i++;
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                char c = s[i++];
                if (c == '"')
                    return sb.ToString();
                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }
                char e = s[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16));
                        i += 4;
                        break;
                    default: sb.Append(e); break;                      // \" \\ \/
                }
            }
            throw new FormatException("JSON: unterminated string");
        }

        static double Number(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0)
                i++;
            if (i == start)
                throw new FormatException("JSON: value expected at " + i);
            return double.Parse(s.Substring(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        static bool Word(string s, ref int i, string word)
        {
            if (string.CompareOrdinal(s, i, word, 0, word.Length) != 0)
                return false;
            i += word.Length;
            return true;
        }

        static void Space(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i]))
                i++;
        }
    }
}
