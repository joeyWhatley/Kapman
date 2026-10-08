using System.Collections.Generic;

namespace Kapman.Encounters
{
    /// <summary>
    /// THE WRITTEN END OF A CHOICE (8 Oct 2026). The script holds, for some two thousand choices, a paragraph of what
    /// came of it. Until now no code showed them: a card answered said only what you did and the effects' own facts.
    /// The words are found by the key of the choice's label, as the writers named them:
    ///   "x.opt_heir"   -> "x.out_heir"          (the chains)
    ///   "x.o.carry"    -> "x.out.carry"         (the Rhineland's cards)
    ///   "x.y"          -> "x.y.done"            (the crew's and the sea's cards)
    /// and where the choice was a chance, the end of the branch that fell: "..._success" / "..._failure" (also "_ok",
    /// "_fail", "_bad") on the first two, ".success" / ".failure" on the label's own key. A choice may name its end
    /// outright in the card's data ("out", "out_success", "out_failure") where the writers named it otherwise.
    /// A branch with no end of its own has the plain one. A choice with none written has none: the facts stand alone.
    /// </summary>
    public static class Outcomes
    {
        static readonly string[] Won = { "_success", "_ok" }, Lost = { "_failure", "_fail", "_bad" };

        /// <summary>The key the end of a choice is written under by the writers' habit of naming, from the key of its
        /// label; null where the label's key has no such shape.</summary>
        public static string Stem(string labelKey)
        {
            if (string.IsNullOrEmpty(labelKey))
                return null;
            int dot = labelKey.LastIndexOf('.');
            if (dot <= 0 || dot == labelKey.Length - 1)
                return null;
            string head = labelKey.Substring(0, dot), last = labelKey.Substring(dot + 1);
            if (last.StartsWith("opt_", System.StringComparison.Ordinal) && last.Length > 4)
                return head + ".out_" + last.Substring(4);
            if (head.EndsWith(".o", System.StringComparison.Ordinal))
                return head.Substring(0, head.Length - 2) + ".out." + last;
            return null;
        }

        /// <summary>The keys the end of a choice may be written under, the likeliest first. `branch`: "success" or
        /// "failure" after a chance, else "".</summary>
        public static IEnumerable<string> Keys(OptionDef o, string branch)
        {
            if (o == null)
                yield break;
            string stem = Stem(o.labelKey);
            if (branch == "success" || branch == "failure")
            {
                bool won = branch == "success";
                string own = won ? o.outSuccess : o.outFailure;
                if (!string.IsNullOrEmpty(own))
                    yield return own;
                if (o.labelKey.Length > 0)
                    yield return o.labelKey + "." + branch;
                if (stem != null)
                    foreach (string tail in won ? Won : Lost)
                        yield return stem + tail;
            }
            if (!string.IsNullOrEmpty(o.outKey))
                yield return o.outKey;
            if (o.labelKey.Length > 0)
                yield return o.labelKey + ".done";
            if (stem != null)
                yield return stem;
        }

        /// <summary>The written end of a choice, or "" where none is written.</summary>
        public static string Text(EncounterBook book, OptionDef o, string branch)
        {
            if (book == null || o == null)
                return "";
            foreach (string key in Keys(o, branch ?? ""))
            {
                string t = book.Written(key);
                if (t != null)
                    return t;
            }
            return "";
        }
    }
}
