using System;
using System.Collections.Generic;
using Kapman.Game;

namespace Kapman.Encounters
{
    /// <summary>
    /// THE WORDS - the plain words the game shows instead of numbers, each with the lowest number that earns it: how a
    /// person feels toward the merchant (npc_roles_793.json "feeling_words": shunned ... fast friend), his standing in a
    /// place (reputation_793.json "words": shunned ... trusted) and the odds of a chance (knowing_793.json "words": sure,
    /// likely, doubtful). A condition may compare a number with a word of its own kind ("npc.tollman.feeling >= easy"
    /// means the feeling is at least easy's lowest value); a word is written in a condition as one token, lower case,
    /// with a leading "a " dropped and spaces as underscores ("a stranger" is "stranger", "fast friend" "fast_friend").
    /// </summary>
    public sealed class WordTable
    {
        public const string Feeling = "feeling", Standing = "standing", Odds = "odds";

        sealed class Band
        {
            public double from;
            public string token, shown;
        }

        readonly Dictionary<string, List<Band>> bands = new Dictionary<string, List<Band>>();

        /// <summary>A word of a kind, earned from this value up (until the next word's value).</summary>
        public void Add(string category, string word, double from)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrWhiteSpace(word))
                return;
            if (!bands.TryGetValue(category, out var list))
                bands[category] = list = new List<Band>();
            list.RemoveAll(b => b.token == Normalise(word));
            list.Add(new Band { from = from, token = Normalise(word), shown = word.Trim() });
            list.Sort((a, b) => a.from.CompareTo(b.from));
        }

        /// <summary>True when the table has words of this kind.</summary>
        public bool Has(string category) => category != null && bands.ContainsKey(category);

        /// <summary>The lowest value a word stands for (a condition's threshold).</summary>
        public bool TryGet(string category, string word, out double from)
        {
            from = 0.0;
            if (category == null || !bands.TryGetValue(category, out var list))
                return false;
            string token = Normalise(word);
            foreach (var b in list)
                if (b.token == token)
                {
                    from = b.from;
                    return true;
                }
            return false;
        }

        /// <summary>The word for a value ("easy" for a feeling of 5); below the lowest word's value, the lowest word.</summary>
        public string WordFor(string category, double value)
        {
            if (category == null || !bands.TryGetValue(category, out var list) || list.Count == 0)
                return "";
            string word = list[0].shown;
            foreach (var b in list)
                if (value >= b.from)
                    word = b.shown;
            return word;
        }

        /// <summary>How a word token is shown ("fast_friend" is "fast friend").</summary>
        public string Shown(string category, string token)
        {
            if (category != null && bands.TryGetValue(category, out var list))
                foreach (var b in list)
                    if (b.token == Normalise(token))
                        return b.shown;
            return (token ?? "").Replace('_', ' ');
        }

        /// <summary>The words of a kind, as condition tokens, lowest first.</summary>
        public List<string> Tokens(string category)
        {
            var all = new List<string>();
            if (category != null && bands.TryGetValue(category, out var list))
                foreach (var b in list)
                    all.Add(b.token);
            return all;
        }

        /// <summary>"A stranger" becomes "stranger", "fast friend" "fast_friend", "Ill-regarded" "ill-regarded".</summary>
        public static string Normalise(string word)
        {
            string w = (word ?? "").Trim().ToLowerInvariant();
            if (w.StartsWith("a "))
                w = w.Substring(2).Trim();
            return w.Replace(' ', '_');
        }

        /// <summary>
        /// The three tables from the game's data: the feeling words from npc_roles_793.json (pairs of [from, word]), the
        /// standing words from reputation_793.json (pairs of [below, word]: a word holds from the number before it) and the
        /// odds words from knowing_793.json ({word: from}). A file that is missing or has no words is an error in the list.
        /// </summary>
        public static WordTable FromData(string rolesJson, string reputationJson, string knowingJson, List<string> errors)
        {
            var t = new WordTable();
            var roles = Parse(rolesJson, "npc_roles_793.json", errors);
            foreach (var pair in Json.Arr(roles, "feeling_words") ?? new List<object>())
                if (pair is List<object> l && l.Count >= 2 && l[0] is double from && l[1] is string w)
                    t.Add(Feeling, w, from);
            var rep = Parse(reputationJson, "reputation_793.json", errors);
            double lowest = -100.0;
            foreach (var pair in Json.Arr(rep, "words") ?? new List<object>())
                if (pair is List<object> l && l.Count >= 2 && l[0] is double below && l[1] is string w)
                {
                    t.Add(Standing, w, lowest);
                    lowest = below;
                }
            var knowing = Parse(knowingJson, "knowing_793.json", errors);
            foreach (var kv in Json.Obj(knowing, "words") ?? new Dictionary<string, object>())
                if (kv.Value is double from)
                    t.Add(Odds, kv.Key, from);
            if (!t.Has(Feeling))
                errors?.Add("npc_roles_793.json: no feeling_words.");
            if (!t.Has(Standing))
                errors?.Add("reputation_793.json: no words.");
            if (!t.Has(Odds))
                errors?.Add("knowing_793.json: no words.");
            return t;
        }

        static object Parse(string json, string name, List<string> errors)
        {
            if (string.IsNullOrEmpty(json))
            {
                errors?.Add(name + ": missing.");
                return null;
            }
            try
            {
                return Json.Parse(json);
            }
            catch (FormatException e)
            {
                errors?.Add(name + ": " + e.Message);
                return null;
            }
        }
    }
}
