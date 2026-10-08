using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kapman.Encounters
{
    /// <summary>The variables a condition reads: the live game's (the runner fills them) or a test's.</summary>
    public interface IVars
    {
        /// <summary>A number variable's value now (0 when the game has none of it).</summary>
        double Num(string name);
        /// <summary>A text variable's value now ("" when the game has none of it).</summary>
        string Text(string name);
    }

    public enum VarType { Number, Text }

    /// <summary>
    /// THE NAMES a condition may use (npc_text_encounters.md 2.4). An unknown name is an error when the cards load, not
    /// in play. Some are whole names ("silver", "port.realm"), some families with a part from the data ("hold.&lt;good&gt;",
    /// "npc.&lt;role&gt;.feeling", "flag.&lt;name&gt;"). A number variable may carry a kind of word it compares with
    /// (feelings, standings). "port.id" is the id of the port she lies at, as in ports_793.json ("" at sea), so a card
    /// can say "moored and port.id = dorestad". A flag's name is the data's: letters, digits and underscores
    /// ("flag.ch_black_stones_3").
    /// </summary>
    public static class VarNames
    {
        static readonly Dictionary<string, (VarType type, string words)> exact = new Dictionary<string, (VarType, string)>
        {
            ["silver"] = (VarType.Number, null), ["debt"] = (VarType.Number, null), ["debt_due_days"] = (VarType.Number, null),
            ["debt_short"] = (VarType.Number, null), ["debt_weight_short"] = (VarType.Number, null), ["debt_quarter_short"] = (VarType.Number, null),
            ["standing"] = (VarType.Number, WordTable.Standing), ["standing_region"] = (VarType.Number, WordTable.Standing),
            ["standing_word"] = (VarType.Number, WordTable.Standing),
            ["crew"] = (VarType.Number, null), ["crew_min"] = (VarType.Number, null), ["crew_full"] = (VarType.Number, null),
            ["health"] = (VarType.Number, null), ["heart"] = (VarType.Number, null), ["days_at_sea"] = (VarType.Number, null),
            ["month"] = (VarType.Number, null), ["year"] = (VarType.Number, null), ["hour"] = (VarType.Number, null),
            ["wind"] = (VarType.Number, null), ["fog"] = (VarType.Number, null), ["storm"] = (VarType.Number, null),
            ["ice"] = (VarType.Number, null), ["feast"] = (VarType.Number, null), ["fast"] = (VarType.Number, null),
            ["moored"] = (VarType.Number, null), ["under_way"] = (VarType.Number, null), ["level_kind"] = (VarType.Text, null),
            ["port.id"] = (VarType.Text, null),
            ["port.kind"] = (VarType.Text, null), ["port.people"] = (VarType.Text, null), ["port.faith"] = (VarType.Text, null),
            ["port.realm"] = (VarType.Text, null), ["port.has_mint"] = (VarType.Number, null), ["port.toll"] = (VarType.Number, null),
            ["port.visits"] = (VarType.Number, null), ["port.best_feeling"] = (VarType.Number, WordTable.Feeling),
            ["hold.weapons"] = (VarType.Number, null), ["hold.captives"] = (VarType.Number, null), ["hold.value"] = (VarType.Number, null),
            ["crew.kind"] = (VarType.Text, null), ["days_since_paid"] = (VarType.Number, null), ["venture_ended"] = (VarType.Number, null),
            ["learned.latin"] = (VarType.Number, null), ["learned.runes"] = (VarType.Number, null),
            ["chest.latin_count"] = (VarType.Number, null), ["chest.riddles_solved"] = (VarType.Number, null),
            ["chest.rune_count"] = (VarType.Number, null), ["reform.coin"] = (VarType.Number, null),
            ["generation"] = (VarType.Number, null),
            // the boat (7 Oct 2026, Game/Voyage/Boat.cs): a gale where she is (1), the hours to one (99: none in sight),
            // her hull (1 sound .. 0), the water in her (litres), the men's weariness (0..1), the sail (whole, weak, split)
            ["gale"] = (VarType.Number, null), ["gale_in"] = (VarType.Number, null), ["boat.hull"] = (VarType.Number, null),
            ["boat.water"] = (VarType.Number, null), ["boat.weary"] = (VarType.Number, null), ["boat.sail"] = (VarType.Text, null),
            // the self (7 Oct 2026, Game/Self/Self.cs): the five stats in pips, 1 to 5, and the three picks
            ["me.tongue"] = (VarType.Number, null), ["me.name"] = (VarType.Number, null), ["me.nerve"] = (VarType.Number, null),
            ["me.lore"] = (VarType.Number, null), ["me.troth"] = (VarType.Number, null),
            // (8 Oct 2026: "me.<stat>" is the stat as it is now, marks and all; ".base" is what lies under them)
            ["me.tongue.base"] = (VarType.Number, null), ["me.name.base"] = (VarType.Number, null), ["me.nerve.base"] = (VarType.Number, null),
            ["me.lore.base"] = (VarType.Number, null), ["me.troth.base"] = (VarType.Number, null),
            ["me.people"] = (VarType.Text, null), ["me.faith"] = (VarType.Text, null), ["me.past"] = (VarType.Text, null),
        };

        // prefix, suffix: the part between is the data's (a good, a role, a flag's name); it holds no dot
        static readonly (string prefix, string suffix, VarType type, string words)[] families =
        {
            ("hold.from_", "", VarType.Number, null),
            ("hold.", "", VarType.Number, null),
            ("tongue.", "", VarType.Number, null),
            ("crew.", "_best", VarType.Number, null),
            ("crew.", "_count", VarType.Number, null),
            ("npc.", ".feeling", VarType.Number, WordTable.Feeling),
            ("npc.", ".met", VarType.Number, null),
            ("npc.", ".owed", VarType.Number, null),
            ("npc.", ".generation", VarType.Number, null),
            ("npc.", ".last", VarType.Text, null),
            ("text.", "", VarType.Number, null),
            ("riddle.", "", VarType.Text, null),
            ("flag.", "", VarType.Number, null),
            ("mark.", "", VarType.Number, null),                         // (is this mark on the player now: 1 or 0)
        };

        /// <summary>Is this a name a condition may use, and of what type (and kind of word)?</summary>
        public static bool TryType(string name, out VarType type, out string words)
        {
            type = VarType.Number;
            words = null;
            if (string.IsNullOrEmpty(name))
                return false;
            if (exact.TryGetValue(name, out var e))
            {
                type = e.type;
                words = e.words;
                return true;
            }
            foreach (var f in families)
            {
                if (!name.StartsWith(f.prefix, StringComparison.Ordinal) || !name.EndsWith(f.suffix, StringComparison.Ordinal))
                    continue;
                int len = name.Length - f.prefix.Length - f.suffix.Length;
                if (len <= 0)
                    continue;
                string part = name.Substring(f.prefix.Length, len);
                if (part.IndexOf('.') >= 0)
                    continue;
                type = f.type;
                words = f.words;
                return true;
            }
            return false;
        }

        /// <summary>The role in "npc.&lt;role&gt;.x" (null for any other name).</summary>
        public static string RoleOf(string name)
        {
            if (name == null || !name.StartsWith("npc.", StringComparison.Ordinal))
                return null;
            int dot = name.IndexOf('.', 4);
            return dot > 4 ? name.Substring(4, dot - 4) : null;
        }

        /// <summary>The good in "hold.&lt;good&gt;" (null for the whole names and the other families).</summary>
        public static string GoodOf(string name)
        {
            if (name == null || !name.StartsWith("hold.", StringComparison.Ordinal) || exact.ContainsKey(name) ||
                name.StartsWith("hold.from_", StringComparison.Ordinal))
                return null;
            return name.Substring(5);
        }
    }

    /// <summary>
    /// A CONDITION - the small language the cards' "when", the lines' "when" and the options' "check" are written in
    /// (npc_text_encounters.md 2.4), parsed once at load and then only evaluated:
    /// <code>
    /// expr  := and ("or" and)*          and   := term ("and" term)*        ("and" binds tighter than "or")
    /// term  := "not" term | "(" expr ")" | "true" | "false" | var | var op (number | word | var)
    /// op    := &lt; &lt;= = != &gt;= &gt;  (== is read as =)
    /// </code>
    /// A bare variable is true when it is not zero (a text variable when it is not empty). A word compares by the lowest
    /// value it stands for (WordTable); a text variable compares with a word by its spelling (= and != only).
    /// </summary>
    public abstract class Cond
    {
        /// <summary>The text it was parsed from.</summary>
        public string Source { get; private set; } = "true";

        public abstract bool Eval(IVars vars);

        /// <summary>Every variable name it reads (for the loader's cross-checks).</summary>
        public abstract void Names(List<string> into);

        /// <summary>Every text variable it compares with a word, and the word ("port.id", "dorestad"), for the loader's
        /// cross-checks (a port that is in no table would never be true).</summary>
        public virtual void TextWords(List<(string name, string word)> into) { }

        /// <summary>The rule in plain words, for a greyed option's tooltip ("The tollman must be easy with you.").</summary>
        public abstract string Describe(WordTable words);

        public static readonly Cond True = new Const(true);

        /// <summary>Parses a condition; an empty text is always true. Throws FormatException with the reason.</summary>
        public static Cond Parse(string text, WordTable words)
        {
            if (string.IsNullOrWhiteSpace(text))
                return True;
            var p = new Parser(text, words ?? new WordTable());
            var c = p.Or();
            if (!p.AtEnd)
                throw new FormatException($"'{text}': unexpected '{p.Peek}'");
            c.Source = text.Trim();
            return c;
        }

        // ------------------------------------------------------------------ the nodes

        sealed class Const : Cond
        {
            readonly bool value;
            public Const(bool value)
            {
                this.value = value;
                Source = value ? "true" : "false";
            }
            public override bool Eval(IVars vars) => value;
            public override void Names(List<string> into) { }
            public override string Describe(WordTable words) => value ? "" : "Never.";
        }

        sealed class Not : Cond
        {
            readonly Cond inner;
            public Not(Cond inner) => this.inner = inner;
            public override bool Eval(IVars vars) => !inner.Eval(vars);
            public override void Names(List<string> into) => inner.Names(into);
            public override void TextWords(List<(string name, string word)> into) => inner.TextWords(into);
            public override string Describe(WordTable words) => inner is Bare b ? b.DescribeNot() : NameOf != null ? "Not now." : "Not when: " + inner.Describe(words);
        }

        sealed class Join : Cond
        {
            readonly bool all;
            readonly List<Cond> parts;
            public Join(bool all, List<Cond> parts)
            {
                this.all = all;
                this.parts = parts;
            }
            public override bool Eval(IVars vars)
            {
                if (all)
                {
                    foreach (var p in parts)
                        if (!p.Eval(vars))
                            return false;
                    return true;
                }
                foreach (var p in parts)
                    if (p.Eval(vars))
                        return true;
                return false;
            }
            public override void Names(List<string> into)
            {
                foreach (var p in parts)
                    p.Names(into);
            }
            public override void TextWords(List<(string name, string word)> into)
            {
                foreach (var p in parts)
                    p.TextWords(into);
            }
            public override string Describe(WordTable words)
            {
                var said = new List<string>();
                foreach (var p in parts)
                {
                    string s = p.Describe(words).TrimEnd('.');
                    if (s.Length > 0)
                        said.Add(s);
                }
                if (said.Count == 0)
                    return "";
                if (!all && NameOf != null)
                {
                    // ("Only if you carry weapons, or captives."; where the parts do not begin alike: one, then "Or:" and the next)
                    const string carry = "Only if you carry ";
                    if (said.TrueForAll(x => x.StartsWith(carry, StringComparison.Ordinal)))
                        return carry + string.Join(", or ", said.ConvertAll(x => x.Substring(carry.Length))) + ".";
                    said.RemoveAll(x => x == "Not now");
                    return said.Count == 0 ? "Not now." : string.Join(". Or: ", said) + ".";
                }
                return all ? string.Join("; ", said) + "." : "Either: " + string.Join("; or: ", said) + ".";
            }
        }

        sealed class Bare : Cond
        {
            readonly string name;
            readonly VarType type;
            public Bare(string name, VarType type)
            {
                this.name = name;
                this.type = type;
            }
            public override bool Eval(IVars vars) => type == VarType.Text ? !string.IsNullOrEmpty(vars.Text(name)) : vars.Num(name) != 0.0;
            public override void Names(List<string> into) => into.Add(name);
            public override string Describe(WordTable words)
            {
                if (NameOf != null)
                {
                    // (the page's rule: a writing by its title, a good by its name; a flag is the makers' name for a
                    // thing that happened, and is never shown)
                    if (name.StartsWith("text.", StringComparison.Ordinal))
                    {
                        string title = NameOf(name);
                        return title != null ? $"Only with {title} in your chest." : "Only with a writing you have not got.";
                    }
                    if (name.StartsWith("hold.", StringComparison.Ordinal))
                        return $"Only if you carry {NameOf(name) ?? Human(name.Substring(5))}.";
                    return "Not now.";
                }
                if (name.StartsWith("text.", StringComparison.Ordinal))
                    return $"Only with the writing \"{Human(name.Substring(5))}\" in your chest.";
                if (name.StartsWith("flag.", StringComparison.Ordinal))
                    return $"Only after: {Human(name.Substring(5))}.";
                if (name.StartsWith("hold.", StringComparison.Ordinal))
                    return $"Only with {Human(name.Substring(5))} aboard.";
                return $"Only when {Human(name)}.";
            }
            public string DescribeNot()
            {
                if (NameOf != null)
                    return name.StartsWith("hold.", StringComparison.Ordinal) ? $"Not while you carry {NameOf(name) ?? Human(name.Substring(5))}." : "Not now.";
                if (name.StartsWith("flag.", StringComparison.Ordinal))
                    return $"Not after: {Human(name.Substring(5))}.";
                if (name.StartsWith("hold.", StringComparison.Ordinal))
                    return $"Not with {Human(name.Substring(5))} aboard.";
                return $"Not when {Human(name)}.";
            }
        }

        enum Op { Lt, Le, Eq, Ne, Ge, Gt }

        static bool Holds(Op op, double a, double b)
        {
            switch (op)
            {
                case Op.Lt: return a < b;
                case Op.Le: return a <= b;
                case Op.Eq: return Math.Abs(a - b) < 1e-9;
                case Op.Ne: return Math.Abs(a - b) >= 1e-9;
                case Op.Ge: return a >= b;
                default: return a > b;
            }
        }

        static string OpText(Op op)
        {
            switch (op)
            {
                case Op.Lt: return "<";
                case Op.Le: return "<=";
                case Op.Eq: return "=";
                case Op.Ne: return "!=";
                case Op.Ge: return ">=";
                default: return ">";
            }
        }

        sealed class NumCompare : Cond
        {
            readonly string name, word, wordKind;
            readonly Op op;
            readonly double value;
            public NumCompare(string name, Op op, double value, string word, string wordKind)
            {
                this.name = name;
                this.op = op;
                this.value = value;
                this.word = word;
                this.wordKind = wordKind;
            }
            public override bool Eval(IVars vars) => Holds(op, vars.Num(name), value);
            public override void Names(List<string> into) => into.Add(name);
            public override string Describe(WordTable words)
            {
                string shown = word != null ? words.Shown(wordKind, word) : value.ToString("0.##", CultureInfo.InvariantCulture);
                string role = VarNames.RoleOf(name);
                bool atLeast = op == Op.Ge || op == Op.Gt, below = op == Op.Lt || op == Op.Le;
                if (role != null && name.EndsWith(".feeling", StringComparison.Ordinal) && word != null)
                {
                    // (the role's title as the cards say it, "the toll man", where the game has given a name for it)
                    string who = (NameOf != null ? NameOf("role:" + role) : null) ?? "the " + Human(role);
                    if (atLeast)
                        return $"{char.ToUpperInvariant(who[0]) + who.Substring(1)} must be {(op == Op.Gt ? "better than" : "at least")} {shown} with you.";
                    if (below)
                        return $"Only while {who} is {(op == Op.Lt ? "less than" : "no more than")} {shown} with you.";
                }
                if (name == "port.best_feeling" && word != null && atLeast)
                    return $"You must know someone here who is {shown} with you.";
                if ((name == "standing" || name == "standing_word") && word != null && atLeast)
                    return $"Your standing here must be {shown} or better.";
                if ((name == "standing" || name == "standing_word") && word != null && below)
                    return $"Only while your standing here is {(op == Op.Lt ? "below" : "at most")} {shown}.";
                if (name == "silver" && atLeast)
                    return $"You need {shown} d of silver.";
                // (the debt for the boat, as the cards ask it: 8 Oct 2026, it read "Only when debt short = 0.")
                if (name.StartsWith("debt_", StringComparison.Ordinal) && name.EndsWith("_short", StringComparison.Ordinal) && value == 0.0 && word == null)
                {
                    if (op == Op.Eq || op == Op.Le)
                        return "You have not the silver for it aboard.";
                    if (op == Op.Gt || op == Op.Ne)
                        return "Only when the silver aboard falls short of it.";
                }
                if (name == "debt" && atLeast && word == null)
                    return op == Op.Gt && value == 0.0 ? "Only while something is owed for the boat." : $"Only while {shown} d or more is owed for the boat.";
                if (name.StartsWith("hold.", StringComparison.Ordinal) && op == Op.Gt && value == 0.0 && word == null)
                    return NameOf != null ? $"Only if you carry {NameOf(name) ?? Human(name.Substring(5))}." : $"Only with {Human(name.Substring(5))} aboard.";
                if (name.StartsWith("hold.", StringComparison.Ordinal) && (op == Op.Eq || op == Op.Le) && value == 0.0 && word == null)
                    return NameOf != null ? $"Only if you carry no {NameOf(name) ?? Human(name.Substring(5))}." : $"Only with no {Human(name.Substring(5))} aboard.";
                if (name == "year" && op == Op.Lt)
                    return $"Only before {shown}.";
                if (name == "year" && op == Op.Ge)
                    return $"Only from {shown}.";
                if (name.StartsWith("me.", StringComparison.Ordinal) && name.Length > 3)
                {
                    // one of the player's own stats: "Your Tongue must be 3 or more."
                    string stat = char.ToUpperInvariant(name[3]) + name.Substring(4);
                    // (by the level's own word where the game gives one: "Your Tongue must be ready or better.")
                    string level = NameOf != null && word == null && op == Op.Ge ? NameOf("level:" + name.Substring(3) + ":" + shown) : null;
                    if (!string.IsNullOrEmpty(level))
                        return $"Your {stat} must be {level} or better.";
                    if (atLeast)
                        return op == Op.Gt ? $"Your {stat} must be more than {shown}." : $"Your {stat} must be {shown} or more.";
                    if (below)
                        return $"Only while your {stat} is {(op == Op.Lt ? "below" : "at most")} {shown}.";
                }
                return NameOf != null ? "Not now." : $"Only when {Human(name)} {OpText(op)} {shown}.";
            }
        }

        sealed class TextCompare : Cond
        {
            readonly string name, word;
            readonly bool equal;
            public TextCompare(string name, bool equal, string word)
            {
                this.name = name;
                this.equal = equal;
                this.word = word;
            }
            public override bool Eval(IVars vars) => string.Equals(vars.Text(name) ?? "", word, StringComparison.OrdinalIgnoreCase) == equal;
            public override void Names(List<string> into) => into.Add(name);
            public override void TextWords(List<(string name, string word)> into) => into.Add((name, word));
            public override string Describe(WordTable words)
            {
                if (NameOf != null)
                {
                    // (a port by its name as it is spelt on the chart; a people and a faith are the player's own, and
                    // need no naming; anything else is the makers' and is not said)
                    if (name == "port.id")
                        return $"Only {(equal ? "at" : "away from")} {NameOf("port:" + word) ?? Human(word)}.";
                    return name == "me.people" ? "Not for your people." : name == "me.faith" ? "Not for your faith." : "Not now.";
                }
                return name == "port.id" ? $"Only {(equal ? "at" : "away from")} {Human(word)}."
                     : name.StartsWith("me.", StringComparison.Ordinal) ? $"Only if your {Human(name.Substring(3))} {(equal ? "is" : "is not")} {Human(word)}."
                     : $"Only where {Human(name)} {(equal ? "is" : "is not")} {Human(word)}.";
            }
        }

        sealed class VarCompare : Cond
        {
            readonly string a, b;
            readonly Op op;
            readonly VarType type;
            public VarCompare(string a, Op op, string b, VarType type)
            {
                this.a = a;
                this.op = op;
                this.b = b;
                this.type = type;
            }
            public override bool Eval(IVars vars)
            {
                if (type == VarType.Text)
                {
                    bool same = string.Equals(vars.Text(a) ?? "", vars.Text(b) ?? "", StringComparison.OrdinalIgnoreCase);
                    return op == Op.Eq ? same : !same;
                }
                return Holds(op, vars.Num(a), vars.Num(b));
            }
            public override void Names(List<string> into)
            {
                into.Add(a);
                into.Add(b);
            }
            public override string Describe(WordTable words) => NameOf != null ? "Not now." : $"Only when {Human(a)} {OpText(op)} {Human(b)}.";
        }

        /// <summary>
        /// THE GAME'S OWN NAME FOR A THING A RULE NAMES (the page's rule of 7 Oct 2026, Design/fixes/page_and_strings.md
        /// 2.3: an id is never shown). "text.charter_toll_dorestad" gives the writing's title, "hold.captives"
        /// "captives", "port:dorestad" "Dorestat", "role:tollman" "the toll man", "level:tongue:3" the level's word;
        /// null where it has none a player may read. Set by the game (the encounter runner). While it is null (a test
        /// of the rules alone) a lock is described by its parts as it always was, ids and all.
        /// </summary>
        public static Func<string, string> NameOf;

        /// <summary>"npc.tollman.feeling" as "tollman feeling", "hid_goods_at_port" as "hid goods at port".</summary>
        static string Human(string name)
        {
            string s = name ?? "";
            if (s.StartsWith("npc.", StringComparison.Ordinal))
                s = s.Substring(4);
            return s.Replace('_', ' ').Replace('.', ' ');
        }

        // ------------------------------------------------------------------ the parser

        sealed class Parser
        {
            readonly List<string> tokens = new List<string>();
            readonly string text;
            readonly WordTable words;
            int at;

            public Parser(string text, WordTable words)
            {
                this.text = text;
                this.words = words;
                Tokenise();
            }

            public bool AtEnd => at >= tokens.Count;
            public string Peek => at < tokens.Count ? tokens[at] : "";

            string Next()
            {
                if (at >= tokens.Count)
                    throw new FormatException($"'{text}': ends too soon");
                return tokens[at++];
            }

            void Tokenise()
            {
                int i = 0;
                while (i < text.Length)
                {
                    char c = text[i];
                    if (char.IsWhiteSpace(c))
                    {
                        i++;
                        continue;
                    }
                    if (c == '(' || c == ')')
                    {
                        tokens.Add(c.ToString());
                        i++;
                        continue;
                    }
                    if (c == '<' || c == '>' || c == '=' || c == '!')
                    {
                        int start = i++;
                        if (i < text.Length && text[i] == '=')
                            i++;
                        string op = text.Substring(start, i - start);
                        if (op == "!")
                            throw new FormatException($"'{text}': '!' alone means nothing (write 'not', or '!=')");
                        tokens.Add(op == "==" ? "=" : op);
                        continue;
                    }
                    if (char.IsDigit(c) || ((c == '-' || c == '+') && i + 1 < text.Length && char.IsDigit(text[i + 1])))
                    {
                        int start = i++;
                        while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.'))
                            i++;
                        tokens.Add(text.Substring(start, i - start));
                        continue;
                    }
                    if (char.IsLetter(c) || c == '_')
                    {
                        int start = i++;
                        while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_' || text[i] == '.' || text[i] == '-'))
                            i++;
                        tokens.Add(text.Substring(start, i - start));
                        continue;
                    }
                    throw new FormatException($"'{text}': unexpected '{c}'");
                }
            }

            public Cond Or()
            {
                var parts = new List<Cond> { And() };
                while (Peek == "or")
                {
                    at++;
                    parts.Add(And());
                }
                return parts.Count == 1 ? parts[0] : new Join(false, parts);
            }

            Cond And()
            {
                var parts = new List<Cond> { Term() };
                while (Peek == "and")
                {
                    at++;
                    parts.Add(Term());
                }
                return parts.Count == 1 ? parts[0] : new Join(true, parts);
            }

            Cond Term()
            {
                string t = Next();
                if (t == "not")
                    return new Not(Term());
                if (t == "(")
                {
                    var inner = Or();
                    if (Next() != ")")
                        throw new FormatException($"'{text}': ')' expected");
                    return inner;
                }
                if (t == "true")
                    return new Const(true);
                if (t == "false")
                    return new Const(false);
                if (t == ")" || t == "and" || t == "or" || IsOp(t) || IsNumber(t))
                    throw new FormatException($"'{text}': a variable expected, not '{t}'");
                if (!VarNames.TryType(t, out var type, out var wordKind))
                    throw new FormatException($"'{text}': unknown variable '{t}'");
                if (!IsOp(Peek))
                    return new Bare(t, type);
                var op = ToOp(Next());
                string right = Next();
                if (right == "(" || right == ")" || IsOp(right) || right == "and" || right == "or" || right == "not")
                    throw new FormatException($"'{text}': a number, a word or a variable expected after '{t}'");
                if (type == VarType.Text)
                {
                    if (op != Op.Eq && op != Op.Ne)
                        throw new FormatException($"'{text}': '{t}' is a text and compares with = or != only");
                    if (IsNumber(right))
                        throw new FormatException($"'{text}': '{t}' compares with a word, not '{right}'");
                    if (right.StartsWith("npc.", StringComparison.Ordinal) || right.StartsWith("port.", StringComparison.Ordinal))
                    {
                        if (!VarNames.TryType(right, out var rt, out _) || rt != VarType.Text)
                            throw new FormatException($"'{text}': '{right}' is not a text variable");
                        return new VarCompare(t, op, right, VarType.Text);
                    }
                    return new TextCompare(t, op == Op.Eq, WordTable.Normalise(right));
                }
                if (IsNumber(right))
                    return new NumCompare(t, op, double.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture), null, null);
                if (wordKind != null && words.TryGet(wordKind, right, out double threshold))
                    return new NumCompare(t, op, threshold, WordTable.Normalise(right), wordKind);
                if (VarNames.TryType(right, out var rightType, out _))
                {
                    if (rightType != VarType.Number)
                        throw new FormatException($"'{text}': '{right}' is a text, '{t}' a number");
                    return new VarCompare(t, op, right, VarType.Number);
                }
                if (wordKind != null)
                    throw new FormatException($"'{text}': '{right}' is not a {wordKind} word ({string.Join(", ", words.Tokens(wordKind))})");
                throw new FormatException($"'{text}': '{t}' is a number: '{right}' is neither a number nor a variable");
            }

            static bool IsOp(string t) => t == "<" || t == "<=" || t == "=" || t == "!=" || t == ">=" || t == ">";

            static bool IsNumber(string t) => t.Length > 0 && (char.IsDigit(t[0]) || ((t[0] == '-' || t[0] == '+') && t.Length > 1));

            static Op ToOp(string t)
            {
                switch (t)
                {
                    case "<": return Op.Lt;
                    case "<=": return Op.Le;
                    case "=": return Op.Eq;
                    case "!=": return Op.Ne;
                    case ">=": return Op.Ge;
                    default: return Op.Gt;
                }
            }
        }
    }
}
