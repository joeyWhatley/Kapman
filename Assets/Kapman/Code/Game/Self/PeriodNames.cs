using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// NAMES OF THE TIME, BY PEOPLE (the user, 7 Oct 2026: "you then type your name or generate a name based on a list
    /// of time accurate names for that culture"). Read from Resources/names_793.json, whose own note says how far the
    /// lists are checked. A people the file does not have takes the list of the people it falls back to, else the
    /// first list.
    /// </summary>
    public static class PeriodNames
    {
        public const string Resource = "names_793";

        static readonly Dictionary<string, (List<string> men, List<string> women)> lists =
            new Dictionary<string, (List<string>, List<string>)>(StringComparer.Ordinal);
        static readonly Dictionary<string, string> fallsBack = new Dictionary<string, string>(StringComparer.Ordinal);
        static bool loaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay() => loaded = false;

        /// <summary>Reads the lists from their JSON (the tests hand in their own).</summary>
        public static void Load(string json)
        {
            lists.Clear();
            fallsBack.Clear();
            loaded = true;
            if (string.IsNullOrEmpty(json))
                return;
            var o = Json.Parse(json);
            foreach (var kv in Json.Obj(o, "peoples") ?? new Dictionary<string, object>())
            {
                var men = new List<string>();
                var women = new List<string>();
                foreach (var n in Json.Arr(kv.Value, "men") ?? new List<object>())
                    if (n is string s && s.Trim().Length > 0)
                        men.Add(s.Trim());
                foreach (var n in Json.Arr(kv.Value, "women") ?? new List<object>())
                    if (n is string s && s.Trim().Length > 0)
                        women.Add(s.Trim());
                lists[kv.Key] = (men, women);
            }
            foreach (var kv in Json.Obj(o, "falls_back") ?? new Dictionary<string, object>())
                if (kv.Value is string to)
                    fallsBack[kv.Key] = to;
        }

        static void Ensure()
        {
            if (loaded)
                return;
            var asset = Resources.Load<TextAsset>(Resource);
            Load(asset != null ? asset.text : "");
        }

        /// <summary>The names of a people: men's or women's. Empty when the file has none at all.</summary>
        public static IReadOnlyList<string> Of(string people, bool women)
        {
            Ensure();
            string id = people ?? "";
            if (!lists.ContainsKey(id) && fallsBack.TryGetValue(id, out string to))
                id = to;
            if (!lists.TryGetValue(id, out var found))
            {
                foreach (var any in lists.Values)
                {
                    found = any;
                    break;
                }
            }
            var list = women ? found.women : found.men;
            return list ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        /// <summary>A name of a people, by the dice (0 inclusive to 1 exclusive); never the one given as `not` when
        /// there is another. "" when there are none.</summary>
        public static string Pick(string people, bool women, double roll, string not = null)
        {
            var list = Of(people, women);
            if (list.Count == 0)
                return "";
            int i = Mathf.Clamp((int)(roll * list.Count), 0, list.Count - 1);
            if (list.Count > 1 && list[i] == not)
                i = (i + 1) % list.Count;
            return list[i];
        }
    }
}
