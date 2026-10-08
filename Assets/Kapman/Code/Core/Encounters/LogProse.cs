using System;
using System.Collections.Generic;

namespace Kapman.Encounters
{
    /// <summary>
    /// THE LOG IN SENTENCES (DECISIONS 7 Oct 2026, item 39: the user, of the log's own lines: "This makes no sense
    /// though"). A line of the log is one or more plain sentences a person could say, with who did what: never a card's
    /// title and a colon, never a word the engine uses inside put in brackets. Pure: the tests read it.
    /// </summary>
    public static class LogProse
    {
        /// <summary>What the people remember of a choice ("sat low and listened", the card's memory line, which is
        /// written without its subject) as a sentence of the merchant's own: "You sat low and listened."</summary>
        public static string YouDid(string remembered)
        {
            string s = (remembered ?? "").Trim().TrimEnd('.', ' ');
            if (s.Length == 0)
                return "";
            if (s.StartsWith("was ", StringComparison.Ordinal))
                s = "were " + s.Substring(4);
            else if (s.StartsWith("wasn't ", StringComparison.Ordinal))
                s = "weren't " + s.Substring(7);
            else if (s.StartsWith("is ", StringComparison.Ordinal))
                s = "are " + s.Substring(3);
            else if (s.StartsWith("has ", StringComparison.Ordinal))
                s = "have " + s.Substring(4);
            // ("kept his word on the men's pay" is written of the merchant: "You kept your word ...")
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\b(kept|broke|gave|went back on|kept to) his (word|oath|promise)\b", "$1 your $2");
            return "You " + s + ".";
        }

        /// <summary>A choice the card has no memory line for, by the words on its button: Your answer was "Walk on".</summary>
        public static string Answered(string label)
        {
            string s = (label ?? "").Trim().TrimEnd('.', ' ');
            return s.Length == 0 ? "" : "Your answer was \"" + s + "\".";
        }

        /// <summary>Feelings moved by one card, as one sentence: "Tofi thinks better of you for it; Bernric thinks worse."
        /// A person moved both ways counts by the sum; "" when nobody's feeling moved.</summary>
        public static string Felt(IList<(string name, double n)> felt)
        {
            if (felt == null || felt.Count == 0)
                return "";
            var order = new List<string>();
            var sum = new Dictionary<string, double>();
            foreach (var (name, n) in felt)
            {
                if (string.IsNullOrEmpty(name))
                    continue;
                if (!sum.ContainsKey(name))
                {
                    sum[name] = 0.0;
                    order.Add(name);
                }
                sum[name] += n;
            }
            var better = new List<string>();
            var worse = new List<string>();
            foreach (string name in order)
                if (sum[name] > 0.0)
                    better.Add(name);
                else if (sum[name] < 0.0)
                    worse.Add(name);
            if (better.Count > 0 && worse.Count > 0)
                return Names(better) + (better.Count == 1 ? " thinks" : " think") + " better of you for it; " +
                       Names(worse) + (worse.Count == 1 ? " thinks" : " think") + " worse.";
            if (better.Count > 0)
                return Names(better) + (better.Count == 1 ? " thinks" : " think") + " better of you for it.";
            if (worse.Count > 0)
                return Names(worse) + (worse.Count == 1 ? " thinks" : " think") + " worse of you for it.";
            return "";
        }

        /// <summary>"Tofi", "Tofi and Bernric", "Tofi, Bernric and Ada".</summary>
        public static string Names(IList<string> names)
        {
            if (names == null || names.Count == 0)
                return "";
            if (names.Count == 1)
                return names[0];
            var head = new List<string>(names);
            string last = head[head.Count - 1];
            head.RemoveAt(head.Count - 1);
            return string.Join(", ", head) + " and " + last;
        }
    }
}
