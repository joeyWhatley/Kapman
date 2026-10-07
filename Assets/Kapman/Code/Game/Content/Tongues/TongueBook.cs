// 7 Oct 2026: THE TONGUES (Design/fate_faith_and_self.md section 6c; the user: "mostly English with the speaker's
// original language sprinkled in"; sometimes "you only see their original language and it does a cool animation
// translation as it appears and the foreign language disappears as English takes its place", "make it change
// quicker"). Reads Game/Data/story/tongues_793.json through ContentFiles: the tongues with their kin, the attested
// words, and the real lines with their pieces ([their words, the English for them]). Nothing here is invented: a line
// is only ever one the file has, and a line the file marks as not yet spoken in a year, or as quoted from memory, is
// not given out.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kapman.Game
{
    /// <summary>A tongue: its name, who speaks it, and how much of each kin tongue's speaker follows it (0 to 1).</summary>
    public sealed class TongueDef
    {
        public string id = "", name = "";
        public string[] spokenBy = Array.Empty<string>();
        public readonly Dictionary<string, double> kin = new Dictionary<string, double>(StringComparer.Ordinal);

        /// <summary>The name without what stands in brackets after it: "Norse (the Danish tongue)" is "Norse".</summary>
        public string Short
        {
            get
            {
                int at = name.IndexOf(" (", StringComparison.Ordinal);
                return at > 0 ? name.Substring(0, at) : name;
            }
        }
    }

    /// <summary>An attested word of a tongue: how it is written, how it is said, what it means, and its kind
    /// (greeting, oath, kin, trade...).</summary>
    public sealed class TongueWord
    {
        public string tongue = "", word = "", say = "", means = "", kind = "";
    }

    /// <summary>A real line of a real text: its pieces in order ([their words, the English for them]), the whole
    /// English, where it stands, and whether it may be given out (not a line quoted from memory).</summary>
    public sealed class TongueLine
    {
        public string id = "", tongue = "", original = "", english = "", source = "", englishBy = "";
        public readonly List<(string original, string english)> pairs = new List<(string, string)>();
        /// <summary>The original was quoted from memory: not to be shown until someone has checked it.</summary>
        public bool check;
        /// <summary>The year before which it is not spoken as a living line (0: any year).</summary>
        public int notBefore;

        /// <summary>Where it stands, short: "Hávamál 76", "Psalm 106:23-24".</summary>
        public string Where
        {
            get
            {
                string s = source ?? "";
                int cut = s.Length;
                foreach (string stop in new[] { " (", ". ", ", ", "; " })
                {
                    int at = s.IndexOf(stop, StringComparison.Ordinal);
                    if (at > 0 && at < cut)
                        cut = at;
                }
                return s.Substring(0, cut).TrimEnd('.');
            }
        }
    }

    /// <summary>A stretch of a line as it is shown: plain words, or a word of another tongue (set apart, with its
    /// meaning on hover).</summary>
    public struct TongueRun
    {
        public string text;
        public TongueWord word;
    }

    /// <summary>
    /// THE TONGUES' BOOK. Asked: what a tongue is called, which tongue a people speaks, how well a tongue is followed
    /// through its kin, a word by how it is written, a line by its id for a year, and where the words of other tongues
    /// stand in a line of English.
    /// </summary>
    public sealed class TongueBook
    {
        public const string File = "story/tongues_793.json";

        readonly List<TongueDef> tongues = new List<TongueDef>();
        readonly List<TongueWord> words = new List<TongueWord>();
        readonly Dictionary<string, TongueLine> lines = new Dictionary<string, TongueLine>(StringComparer.Ordinal);

        public bool FromFile { get; private set; }
        public IReadOnlyList<TongueDef> Tongues => tongues;
        public IReadOnlyList<TongueWord> Words => words;
        public IEnumerable<TongueLine> Lines => lines.Values;

        static readonly ContentCache<TongueBook> cache = new ContentCache<TongueBook>("the tongues", Load);
        public static TongueBook Current => cache.Value;

        public TongueDef Tongue(string id) => tongues.Find(t => t.id == id);

        /// <summary>A tongue's plain name ("Norse"); "" if the book has no such tongue.</summary>
        public string Name(string id) => Tongue(id)?.Short ?? "";

        /// <summary>The tongue a people speaks ("" if the book names none).</summary>
        public string Of(string people)
        {
            if (string.IsNullOrEmpty(people))
                return "";
            foreach (var t in tongues)
                if (Array.IndexOf(t.spokenBy, people) >= 0)
                    return t.id;
            return "";
        }

        /// <summary>How well a tongue is followed through its kin: the best of (how well a kin tongue is held) times
        /// (how much of this tongue that kin's speaker follows). 0 for a tongue with no kin held.</summary>
        public double Follow(string tongue, Func<string, double> held)
        {
            var t = Tongue(tongue);
            if (t == null || held == null)
                return 0.0;
            double best = 0.0;
            foreach (var kv in t.kin)
                best = Math.Max(best, held(kv.Key) * kv.Value);
            return Math.Min(1.0, best);
        }

        /// <summary>A word of a tongue by how it is written (the case does not matter); null if the book has none.</summary>
        public TongueWord Word(string tongue, string word)
        {
            foreach (var w in words)
                if (w.tongue == tongue && string.Equals(w.word, word, StringComparison.OrdinalIgnoreCase))
                    return w;
            return null;
        }

        /// <summary>The first greeting of a tongue (null: the book has none for it).</summary>
        public TongueWord Greeting(string tongue)
        {
            foreach (var w in words)
                if (w.tongue == tongue && w.kind == "greeting")
                    return w;
            return null;
        }

        /// <summary>A line that may be given out in a year: it is in the file, has pieces, is not marked as quoted from
        /// memory, and was spoken by then. Null otherwise.</summary>
        public TongueLine Line(string id, int year)
        {
            if (id == null || !lines.TryGetValue(id, out var l))
                return null;
            return l.pairs.Count > 0 && !l.check && (l.notBefore == 0 || l.notBefore <= year) ? l : null;
        }

        /// <summary>A line by its id whatever its marks (a test, a tool).</summary>
        public TongueLine LineRaw(string id) => id != null && lines.TryGetValue(id, out var l) ? l : null;

        /// <summary>What a word shows on hover: "heill, said hayl. Norse: whole, well, in luck."</summary>
        public string Tip(TongueWord w)
        {
            if (w == null)
                return "";
            string tongue = Name(w.tongue);
            return w.word + (w.say.Length > 0 ? ", said " + w.say : "") + ". " + (tongue.Length > 0 ? tongue + ": " : "") + w.means;
        }

        /// <summary>
        /// A line of English cut into runs: plain words, and the words of other tongues in it. The words looked for are
        /// the ones named (`marked`: written by the line's maker on purpose) and, of the tongue given, only those no
        /// English word could be taken for (a word with a letter English does not have: félagi, kāpmon, þing). So "wine"
        /// in an English line is never taken for the Old English word for a friend. The longest match wins.
        /// </summary>
        public List<TongueRun> Mark(string text, string tongue, IEnumerable<TongueWord> marked = null)
        {
            var runs = new List<TongueRun>();
            if (string.IsNullOrEmpty(text))
                return runs;
            var look = new List<TongueWord>();
            if (marked != null)
                foreach (var w in marked)
                    if (w != null && w.word.Length > 0)
                        look.Add(w);
            if (!string.IsNullOrEmpty(tongue))
                foreach (var w in words)
                    if (w.tongue == tongue && w.word.Length > 1 && NotEnglish(w.word) && !look.Contains(w))
                        look.Add(w);
            look.Sort((a, b) => b.word.Length.CompareTo(a.word.Length));
            int from = 0, i = 0;
            while (i < text.Length)
            {
                TongueWord hit = null;
                if (i == 0 || !char.IsLetter(text[i - 1]))
                    foreach (var w in look)
                    {
                        int end = i + w.word.Length;
                        if (end > text.Length || string.Compare(text, i, w.word, 0, w.word.Length, true, CultureInfo.InvariantCulture) != 0)
                            continue;
                        if (end < text.Length && char.IsLetter(text[end]))
                            continue;
                        hit = w;
                        break;
                    }
                if (hit == null)
                {
                    i++;
                    continue;
                }
                if (i > from)
                    runs.Add(new TongueRun { text = text.Substring(from, i - from) });
                runs.Add(new TongueRun { text = text.Substring(i, hit.word.Length), word = hit });
                i += hit.word.Length;
                from = i;
            }
            if (from < text.Length)
                runs.Add(new TongueRun { text = text.Substring(from) });
            return runs;
        }

        static bool NotEnglish(string word)
        {
            foreach (char c in word)
                if (c > 0x7f)
                    return true;
            return false;
        }

        // ------------------------------------------------------------------ reading

        static TongueBook Load()
        {
            var book = new TongueBook();
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return book;
            foreach (var x in ContentJson.Rows(root, "tongues"))
            {
                var t = new TongueDef { id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), spokenBy = ContentJson.Strings(x, "spoken_by") };
                foreach (var k in ContentJson.Rows(x, "kin"))
                {
                    string kin = ContentJson.Text(k, "tongue");
                    if (kin.Length > 0)
                        t.kin[kin] = Math.Max(0.0, Math.Min(1.0, ContentJson.Num(k, "follows")));
                }
                if (t.id.Length > 0)
                    book.tongues.Add(t);
            }
            foreach (var x in ContentJson.Rows(root, "words"))
            {
                var w = new TongueWord
                {
                    tongue = ContentJson.Text(x, "tongue"), word = ContentJson.Text(x, "word"), say = ContentJson.Text(x, "say"),
                    means = ContentJson.Text(x, "means"), kind = ContentJson.Text(x, "kind"),
                };
                if (w.tongue.Length > 0 && w.word.Length > 0)
                    book.words.Add(w);
            }
            var notBefore = Json.Obj(root, "not_before");
            foreach (var x in ContentJson.Rows(root, "lines"))
            {
                var l = new TongueLine
                {
                    id = ContentJson.Text(x, "id"), tongue = ContentJson.Text(x, "tongue"), original = ContentJson.Text(x, "original"),
                    english = ContentJson.Text(x, "english"), source = ContentJson.Text(x, "source"), englishBy = ContentJson.Text(x, "english_by"),
                    check = Json.Bool(x, "check"),
                };
                if (l.id.Length == 0)
                    continue;
                l.notBefore = ContentJson.Int(notBefore, l.id);
                var pairs = Json.Arr(x, "pairs");
                if (pairs != null)
                    foreach (object p in pairs)
                        if (p is List<object> two && two.Count >= 2 && two[0] is string theirs && two[1] is string ours && theirs.Length > 0)
                            l.pairs.Add((theirs, ours));
                book.lines[l.id] = l;
            }
            book.FromFile = book.tongues.Count > 0;
            return book;
        }
    }

    /// <summary>
    /// HOW A LINE TURNS (pure: the frame shows it, the tests reckon it). A line comes up whole in the speaker's tongue
    /// and turns into English piece by piece. How fast, and whether the hard pieces turn at all, follows the player's
    /// skill in that tongue (0 to 1):
    ///   - a tongue you know (0.9 and up): every piece turns, quickly, the whole in well under two seconds;
    ///   - a tongue you half know (0.2 to 0.9): slower, and the hardest pieces stay as they were (you get the gist and
    ///     miss the catch): the longest pieces you have not yet seen a few times;
    ///   - a tongue you do not know (under 0.2): nothing turns.
    /// A piece seen often enough (Self.StaysAfter) is English from the start, whatever the skill.
    /// </summary>
    public sealed class TurnPlan
    {
        public struct Piece
        {
            public string original, english;
            /// <summary>Seconds from the start at which it turns; 0: English from the start; under 0: it stays as it was.</summary>
            public float at;
        }

        public const double KnownFrom = 0.9, NoneBelow = 0.2;
        /// <summary>Seconds a piece takes to go out and come back as English.</summary>
        public const float Fade = 0.12f;
        /// <summary>A hard piece seen this often is no longer hard.</summary>
        public const int EasyAfter = 3;

        public readonly Piece[] pieces;
        public readonly double skill;
        /// <summary>Seconds until the last piece that turns has turned (0: nothing turns).</summary>
        public float Duration { get; private set; }
        /// <summary>How many pieces turn as it is watched, how many stay as they were, how many are English from the start.</summary>
        public int Turning { get; private set; }
        public int Staying { get; private set; }
        public int FromStart { get; private set; }

        TurnPlan(int n, double skill)
        {
            pieces = new Piece[n];
            this.skill = skill;
        }

        public static TurnPlan Make(IReadOnlyList<(string original, string english)> pairs, double skill, Func<string, int> seen)
        {
            int n = pairs != null ? pairs.Count : 0;
            var plan = new TurnPlan(n, skill);
            var open = new List<int>();                                    // the pieces not English from the start
            for (int i = 0; i < n; i++)
            {
                plan.pieces[i] = new Piece { original = pairs[i].original, english = pairs[i].english, at = -1f };
                if (seen != null && seen(pairs[i].original) >= Self.StaysAfter)
                {
                    plan.pieces[i].at = 0f;
                    plan.FromStart++;
                }
                else
                    open.Add(i);
            }
            if (skill >= NoneBelow && open.Count > 0)
            {
                var stay = new HashSet<int>();
                if (skill < KnownFrom)
                {
                    // the hard pieces: the longest ones not yet seen a few times, as many as the skill leaves out
                    int count = (int)Math.Round(n * (KnownFrom - skill) * 0.7, MidpointRounding.AwayFromZero);
                    count = Math.Max(n >= 2 ? 1 : 0, Math.Min(n - 1, count));
                    var hard = new List<int>();
                    foreach (int i in open)
                        if (seen == null || seen(pairs[i].original) < EasyAfter)
                            hard.Add(i);
                    hard.Sort((a, b) => pairs[b].original.Length != pairs[a].original.Length ? pairs[b].original.Length.CompareTo(pairs[a].original.Length) : a.CompareTo(b));
                    for (int k = 0; k < hard.Count && k < count; k++)
                        stay.Add(hard[k]);
                }
                bool known = skill >= KnownFrom;
                float first = known ? 0.2f : 0.45f;
                float step = known ? Math.Min(0.15f, 0.9f / n)
                                   : Math.Min(3.0f / n, (float)(0.5 + (0.2 - 0.5) * ((skill - NoneBelow) / (KnownFrom - NoneBelow))));
                int turned = 0;
                foreach (int i in open)
                {
                    if (stay.Contains(i))
                        continue;
                    plan.pieces[i].at = first + step * turned++;
                    plan.Duration = plan.pieces[i].at + Fade * 0.5f;
                }
                plan.Turning = turned;
            }
            plan.Staying = n - plan.Turning - plan.FromStart;
            return plan;
        }

        /// <summary>How far piece `i` is through its turn at `t` seconds: 0 as it was, 1 English.</summary>
        public float Phase(int i, float t)
        {
            float at = pieces[i].at;
            if (at < 0f)
                return 0f;
            if (at == 0f)
                return 1f;
            float p = (t - (at - Fade * 0.5f)) / Fade;
            return p < 0f ? 0f : p > 1f ? 1f : p;
        }

        /// <summary>What piece `i` reads as at `t` seconds.</summary>
        public string TextAt(int i, float t) => Phase(i, t) >= 0.5f ? pieces[i].english : pieces[i].original;

        /// <summary>How solid piece `i` is drawn at `t`: it goes out as it was and comes back as English.</summary>
        public float Opacity(int i, float t)
        {
            float p = Phase(i, t);
            return p <= 0f || p >= 1f ? 1f : Math.Abs(2f * p - 1f);
        }

        /// <summary>The whole line as it reads at `t` seconds.</summary>
        public string ReadAt(float t)
        {
            var parts = new string[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
                parts[i] = TextAt(i, t);
            return string.Join(" ", parts);
        }
    }

    /// <summary>
    /// WHO SAYS WHAT IN THEIR OWN TONGUE: Game/Data/story/talk_tongues_793.json gives the people of the ports a word to
    /// hail with and, where the tongues file has one that fits, one true line: by person, or by role and people. The
    /// first rule that fits gives the hail; the first that fits and has a line gives the line.
    /// </summary>
    public sealed class TalkTongues
    {
        public const string File = "story/talk_tongues_793.json";

        public sealed class Rule
        {
            public string person = "", hailTongue = "", hailWord = "", line = "", on = "";
            public string[] roles = Array.Empty<string>(), peoples = Array.Empty<string>();

            public bool Fits(PersonDef p)
            {
                if (p == null)
                    return false;
                if (person.Length > 0)
                    return person == p.id;
                return (roles.Length == 0 || Array.IndexOf(roles, p.role) >= 0) && (peoples.Length == 0 || Array.IndexOf(peoples, p.people) >= 0);
            }
        }

        public readonly List<Rule> rules = new List<Rule>();
        public bool FromFile { get; private set; }

        static readonly ContentCache<TalkTongues> cache = new ContentCache<TalkTongues>("the talk in tongues", Load);
        public static TalkTongues Current => cache.Value;

        /// <summary>The word a person hails with (null: none).</summary>
        public TongueWord Hail(PersonDef p)
        {
            foreach (var r in rules)
                if (r.hailWord.Length > 0 && r.Fits(p))
                    return TongueBook.Current.Word(r.hailTongue, r.hailWord);
            return null;
        }

        /// <summary>The true line a person has for a year, and the line key it is said at ("greet", "saying"): the rule's,
        /// or the greeting when the person has no such key. Null: none.</summary>
        public TongueLine LineOf(PersonDef p, int year, out string on)
        {
            on = "greet";
            foreach (var r in rules)
            {
                if (r.line.Length == 0 || !r.Fits(p))
                    continue;
                var line = TongueBook.Current.Line(r.line, year);
                if (line == null)
                    continue;
                on = r.on.Length > 0 && p.Has(r.on) ? r.on : "greet";
                return line;
            }
            return null;
        }

        static TalkTongues Load()
        {
            var talk = new TalkTongues();
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return talk;
            foreach (var x in ContentJson.Rows(root, "rules"))
            {
                var r = new Rule
                {
                    person = ContentJson.Text(x, "person"), line = ContentJson.Text(x, "line"), on = ContentJson.Text(x, "on"),
                    roles = ContentJson.Strings(x, "role"), peoples = ContentJson.Strings(x, "people"),
                };
                string[] hail = ContentJson.Strings(x, "hail");
                if (hail.Length >= 2)
                {
                    r.hailTongue = hail[0];
                    r.hailWord = hail[1];
                }
                talk.rules.Add(r);
            }
            talk.FromFile = talk.rules.Count > 0;
            return talk;
        }
    }
}
