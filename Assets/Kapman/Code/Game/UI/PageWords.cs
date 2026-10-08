using System.Text.RegularExpressions;

namespace Kapman.Game
{
    /// <summary>
    /// THE PAGE'S RULE (Design/fixes/page_and_strings.md, 7 Oct 2026; the alpha's row 17, "a clean screen"): nothing
    /// reaches the screen that was written for the makers. A window says what is so in the world. It does not say
    /// what the game lacks, and it does not name a file, a key, a flag or a rule, or put a number in brackets after a
    /// word. <see cref="Fault"/> names what is wrong with a string, or null. Pure, so a test holds it: over the data
    /// the frame prints, and over every word on every screen the play tests show.
    /// </summary>
    public static class PageWords
    {
        static readonly (string name, Regex rx)[] Faults =
        {
            ("says what is to come",     new Regex(@"\bto come\)", RegexOptions.IgnoreCase)),
            ("says what the game lacks", new Regex(@"(?i:not (kept|counted|built|written|read|reckoned) yet|not in the game|no rule in the game|: not yet\b|not yet people)|^not yet$|^Not yet chosen$")),
            ("an unwritten key",         new Regex(@"\[write:|\(placeholder\)|\bTODO\b")),
            ("a number in brackets",     new Regex(@"\([^)]*\d[^)]*\)")),
            ("a word in brackets",       new Regex(@"\([a-z][a-z ]{0,20}\)")),
            ("a tag or key in brackets", new Regex(@"\[[A-Za-z0-9 \-]{1,24}\]")),
            ("a name in quote marks",    new Regex("\"[a-z0-9_.]+( [a-z0-9_.]+)*\"|'by [a-z_.]+'")),
            ("the sums joined",          new Regex(@"^Either: |; or: ")),
            ("an id",                    new Regex(@"\b[a-z]+(_[a-z0-9]+)+\b|\b(npc|hold|flag|text|me|port|crew)\.[a-z_.]+\b")),
            ("a file name",              new Regex(@"\.(json|cs|md|png|uss)\b")),
            ("a workshop word",          new Regex(@"\bpalette\b|\bpips?\b|\bflag\b|\bplaceholder\b|\b(panel|window)\b|\bA check\b|\bstats?\b", RegexOptions.IgnoreCase)),
            ("a sign of the sums",       new Regex(@"<=|>=|!=")),
        };

        /// <summary>
        /// What is wrong with a string the player would see, or null.
        /// `tooltip`: the interface is speaking, and the player asked: a key in square brackets and the words window
        /// and panel may stand there.
        /// `prose`: the words of a card, a talk or the log, where the world speaks: a window there is a hole in a wall
        /// and a flag is on a mast, and somebody may say a short word as an aside; the workshop's words, the short
        /// word in brackets and the name in quote marks (somebody quoting somebody) are let through.
        /// </summary>
        public static string Fault(string text, bool tooltip = false, bool prose = false)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            foreach (var (name, rx) in Faults)
            {
                if (tooltip && (name == "a tag or key in brackets" || name == "a workshop word"))
                    continue;
                if (prose && (name == "a workshop word" || name == "a word in brackets" || name == "a name in quote marks"))
                    continue;
                if (rx.IsMatch(text))
                    return name;
            }
            return null;
        }
    }
}
