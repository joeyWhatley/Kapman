using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;

namespace Kapman.Encounters
{
    /// <summary>The texts the loader reads (the game passes the files' contents; a test reads them off the disk).</summary>
    public sealed class EncounterSources
    {
        /// <summary>The card files: a name for the messages ("encounters/harbour_793.json") and the text.</summary>
        public readonly List<(string name, string json)> cards = new List<(string, string)>();
        /// <summary>encounters/npc_roles_793.json, encounters/names_placeholder.json, reputation_793.json, knowing_793.json, writing.json.</summary>
        public string roles, names, reputation, knowing, writing;
        /// <summary>Optional cross-check tables: goods_793.json, ports_793.json; traffic_793.json for the port factor.</summary>
        public string goods, ports, traffic;
        /// <summary>Which cards of a file are read (the file as named in <see cref="cards"/>, the id of the card); null: all.
        /// The game holds back what is not polished yet (Game/Content/ContentHold, 9 Oct 2026): a card not taken is
        /// not read at all, so the faults of a held card shut nothing.</summary>
        public Func<string, string, bool> take;
    }

    /// <summary>All the cards, checked, with the tables they need: roles, names, words, the writing, the ports' traffic.</summary>
    public sealed class EncounterBook
    {
        public readonly List<EncounterDef> cards = new List<EncounterDef>();
        readonly Dictionary<string, EncounterDef> byId = new Dictionary<string, EncounterDef>(StringComparer.Ordinal);
        public RoleTable roles = new RoleTable();
        public NamePool names = new NamePool();
        public WordTable words = new WordTable();
        /// <summary>The writing table: key to the user's text ("" or "[write: ...]" until written).</summary>
        public readonly Dictionary<string, string> writing = new Dictionary<string, string>(StringComparer.Ordinal);
        /// <summary>Faults that stop the cards being used (a bad condition, an unknown verb, a missing card).</summary>
        public readonly List<string> errors = new List<string>();
        /// <summary>Things to look at that do not stop the cards (a field the loader does not know, such as "art": it is
        /// skipped; a port.id that names no port in ports_793.json).</summary>
        public readonly List<string> warnings = new List<string>();
        /// <summary>Writing keys the cards use that writing.json has no entry for (shown as [write: key] in play).</summary>
        public readonly List<string> missingKeys = new List<string>();
        /// <summary>Hulls at each port's landing by month (traffic_793.json harbours.ports.&lt;id&gt;.present).</summary>
        public readonly Dictionary<string, double[]> present = new Dictionary<string, double[]>(StringComparer.Ordinal);

        /// <summary>The cards of the files read that were not taken (<see cref="EncounterSources.take"/>).</summary>
        public int heldBack;

        public bool Ok => errors.Count == 0;

        public EncounterDef Card(string id) => id != null && byId.TryGetValue(id, out var c) ? c : null;

        internal bool Add(EncounterDef c)
        {
            if (byId.ContainsKey(c.id))
                return false;
            c.number = cards.Count;
            cards.Add(c);
            byId[c.id] = c;
            return true;
        }

        /// <summary>
        /// m_port (npc_text_encounters.md 2.3): the port's hulls at the landing this month over 40, kept within 0.4..1.5
        /// (Dorestad in July 1.5, a middling market about 1, a village 0.4). A port the traffic table lacks counts as 1.
        /// </summary>
        public double PortFactor(string port, int month)
        {
            if (port == null || !present.TryGetValue(port, out var p) || p.Length < 12)
                return 1.0;
            return Math.Max(0.4, Math.Min(1.5, p[Math.Max(1, Math.Min(12, month)) - 1] / 40.0));
        }

        /// <summary>The user's words for a key, or null while unwritten (no entry, empty, or a "[write: ...]" placeholder).</summary>
        public string Written(string key)
        {
            if (key == null || !writing.TryGetValue(key, out var t) || string.IsNullOrWhiteSpace(t) || t.TrimStart().StartsWith("[write:", StringComparison.Ordinal))
                return null;
            return t;
        }

        /// <summary>What shows for a key: the user's words, else the placeholder (its hint if writing.json has one).</summary>
        public string Shown(string key)
        {
            var w = Written(key);
            if (w != null)
                return w;
            return key != null && writing.TryGetValue(key, out var t) && !string.IsNullOrWhiteSpace(t) ? t : $"[write: {key}]";
        }
    }

    /// <summary>
    /// THE LOADER - reads the card files with the project's Json parser and checks them hard (npc_text_encounters.md 4.4),
    /// so a fault shows when the game starts, not in play: every condition parses over the known variables and words; every
    /// effect is a known verb with its arguments as the grammar has them; chance objects have both branches; no option
    /// without a label key; at most one default per card; every "then" and "schedule" names a card; no card both on arrival
    /// and drawn; every speaker and every npc.&lt;role&gt; is one of the card's actors and every actor's role is in the roles
    /// table; deeds, goods and ports exist where the tables are given; ids and option keys are unique. A field the loader
    /// does not know ("art", or a misspelt "cooldown") is skipped and listed in the book's warnings: the card still loads,
    /// and the misspelling is not silent. A "port.id = x" that names no port is a warning too. Writing keys missing from
    /// writing.json are listed, not faults: they show as [write: key].
    /// </summary>
    public static class EncounterLoader
    {
        static readonly HashSet<string> CardFields = new HashSet<string>
        {
            "id", "stage", "arrival", "when", "weight", "rate", "once", "cooldown_days", "months", "wants", "actors",
            "title_key", "title", "intention_key", "intention", "lines", "options", "memory", "note",
        };
        static readonly HashSet<string> OptionFields = new HashSet<string>
        {
            "key", "label_key", "label", "check", "cost", "effects", "then", "default", "note",
            "out", "out_success", "out_failure",
        };
        static readonly HashSet<string> LineFields = new HashSet<string> { "who", "key", "tongue", "turns", "when", "note" };
        static readonly HashSet<string> ActorFields = new HashSet<string> { "role", "people", "faith", "tongue", "age", "fixed", "sex", "note", "cast", "name", "cast_from", "cast_to" };

        public static EncounterBook Load(EncounterSources src)
        {
            var book = new EncounterBook();
            var errors = book.errors;
            book.roles = RoleTable.Load(src.roles, errors);
            book.names = NamePool.Load(src.names, errors);
            book.words = WordTable.FromData(src.roles, src.reputation, src.knowing, errors);
            LoadWriting(book, src.writing);
            LoadTraffic(book, src.traffic, errors);
            var deeds = Keys(src.reputation, "deeds");
            var goods = Ids(src.goods, "goods");
            var ports = Ids(src.ports, "ports");

            foreach (var (name, json) in src.cards)
            {
                object root;
                try
                {
                    root = Json.Parse(json ?? "");
                }
                catch (FormatException e)
                {
                    errors.Add($"{name}: {e.Message}");
                    continue;
                }
                var list = Json.Arr(root, "cards");
                if (list == null)
                {
                    errors.Add($"{name}: no \"cards\" list.");
                    continue;
                }
                foreach (var o in list)
                {
                    if (src.take != null && o is Dictionary<string, object> raw && !src.take(name, Json.Str(raw, "id")))
                    {
                        book.heldBack++;
                        continue;
                    }
                    var card = ParseCard(o as Dictionary<string, object>, name, book, errors);
                    if (card != null && !book.Add(card))
                        errors.Add($"{name}: two cards with the id '{card.id}'.");
                }
            }

            // the checks that need every card: "then" and "schedule" targets, and the tables
            foreach (var c in book.cards)
            {
                string where = $"{c.file} {c.id}";
                if (goods != null)
                {
                    var names = new List<string>();
                    c.when.Names(names);
                    foreach (var l in c.lines)
                        l.when.Names(names);
                    foreach (var o in c.options)
                        o.check.Names(names);
                    foreach (var n in names.Distinct())
                    {
                        string good = VarNames.GoodOf(n);
                        if (good != null && !goods.Contains(good))
                            errors.Add($"{where}: '{n}' names no good '{good}' in goods_793.json.");
                    }
                }
                if (ports != null)
                {
                    var words = new List<(string name, string word)>();
                    c.when.TextWords(words);
                    foreach (var l in c.lines)
                        l.when.TextWords(words);
                    foreach (var o in c.options)
                        o.check.TextWords(words);
                    foreach (var (n, word) in words.Distinct())
                        if (n == "port.id" && !ports.Contains(word))
                            book.warnings.Add($"{where}: port.id = {word}: no port '{word}' in ports_793.json (the card never comes there).");
                }
                foreach (var o in c.options)
                {
                    if (o.then.Length > 0 && book.Card(o.then) == null)
                        errors.Add($"{where} option {o.key}: 'then' names no card '{o.then}'.");
                    foreach (var v in o.cost.Concat(o.effects).SelectMany(e => e.Verbs()))
                        CheckVerb(v, c, $"{where} option {o.key}", book, deeds, goods, ports, errors);
                }
                LeansByName(c, book);
            }
            return book;
        }

        /// <summary>
        /// A CARD WHOSE OWN WORDS NAME ONE OF ITS STANDING CAST leans on him too (9 Oct 2026: the lender's men can kill
        /// Ravn, and no card may show a dead man alive): its scene, another's line, a choice or its written end has his
        /// name in it. Unless every such text is written again for when he is gone, under its own key with
        /// ".without_ravn" after it (EncounterSession.Without): then the card comes, and reads those.
        /// </summary>
        static void LeansByName(EncounterDef c, EncounterBook book)
        {
            foreach (var spec in c.actors)
            {
                if (spec.cast.Length == 0 || spec.name.Length < 3 || c.leans.Contains(spec.cast))
                    continue;
                string tail = ".without_" + EncounterState.ShortName(spec.cast);
                var keys = new List<string> { c.titleKey, c.intentionKey };
                foreach (var l in c.lines)
                    if (l.who != spec.key)
                        keys.Add(l.key);                                 // (his own lines are simply left out when he is gone)
                foreach (var o in c.options)
                {
                    keys.Add(o.labelKey);
                    foreach (string branch in new[] { "", "success", "failure" })
                        keys.AddRange(Outcomes.Keys(o, branch));
                }
                foreach (string key in keys)
                {
                    string text = book.Written(key);
                    if (text != null && text.IndexOf(spec.name, StringComparison.Ordinal) >= 0 && book.Written(key + tail) == null)
                    {
                        c.leans.Add(spec.cast);
                        break;
                    }
                }
            }
        }

        static void CheckVerb(VerbEffect v, EncounterDef c, string where, EncounterBook book, HashSet<string> deeds, HashSet<string> goods,
                              HashSet<string> ports, List<string> errors)
        {
            bool Port(string p) => string.IsNullOrEmpty(p) || p == "here" || ports == null || ports.Contains(p);
            bool Good(string g) => goods == null || goods.Contains(g);
            switch (v.verb)
            {
                case "deed":
                    if (deeds != null && !deeds.Contains(v.a))
                        errors.Add($"{where}: no deed '{v.a}' in reputation_793.json.");
                    break;
                case "goods":
                    if (!Good(v.a))
                        errors.Add($"{where}: no good '{v.a}' in goods_793.json.");
                    break;
                case "price_shock":
                    if (!Port(v.a))
                        errors.Add($"{where}: no port '{v.a}'.");
                    if (!Good(v.b))
                        errors.Add($"{where}: no good '{v.b}'.");
                    break;
                case "commission":
                    if (!Good(v.a))
                        errors.Add($"{where}: no good '{v.a}'.");
                    break;
                case "news":
                    if (!Port(v.b))
                        errors.Add($"{where}: no port '{v.b}'.");
                    Key(book, v.a);
                    break;
                case "schedule":
                    if (book.Card(v.a) == null)
                        errors.Add($"{where}: 'schedule' names no card '{v.a}'.");
                    if (!Port(v.b))
                        errors.Add($"{where}: no port '{v.b}'.");
                    break;
                case "passenger":
                    if (!Port(v.b))
                        errors.Add($"{where}: no port '{v.b}'.");
                    if (c.Actor(v.a) == null)
                        errors.Add($"{where}: '{v.a}' is not one of the card's actors.");
                    break;
                case "npc":
                case "hire":
                case "pay_off":
                    if (c.Actor(v.a) == null)
                        errors.Add($"{where}: '{v.a}' is not one of the card's actors.");
                    break;
            }
        }

        static EncounterDef ParseCard(Dictionary<string, object> o, string file, EncounterBook book, List<string> errors)
        {
            if (o == null)
            {
                errors.Add($"{file}: a card is not an object.");
                return null;
            }
            var c = new EncounterDef { id = Json.Str(o, "id"), file = file };
            string where = $"{file} {(c.id.Length > 0 ? c.id : "(no id)")}";
            if (c.id.Length == 0)
                errors.Add($"{where}: a card needs an id.");
            foreach (var k in o.Keys)
                if (!CardFields.Contains(k))
                    book.warnings.Add($"{where}: unknown field \"{k}\" (skipped).");
            switch (Json.Str(o, "stage"))
            {
                case "harbour": c.stage = Stage.Harbour; break;
                case "sea": c.stage = Stage.Sea; break;
                case "crew": c.stage = Stage.Crew; break;
                case "long": c.stage = Stage.Long; break;
                default: errors.Add($"{where}: stage must be harbour, sea, crew or long."); break;
            }
            c.arrival = Json.Bool(o, "arrival");
            c.when = Condition(Json.Str(o, "when"), book, errors, where + " when");
            c.weight = Json.Num(o, "weight");
            if (o.TryGetValue("rate", out var rate))
            {
                if (rate is double r)
                    c.rate = r;
                else if (rate as string == "traffic")
                    c.bySim = true;
                else
                    errors.Add($"{where}: rate is a number per game hour, or \"traffic\".");
            }
            c.once = Json.Bool(o, "once");
            c.cooldownDays = Json.Num(o, "cooldown_days");
            foreach (var m in Json.Arr(o, "months") ?? new List<object>())
                if (m is double month && month >= 1 && month <= 12)
                    c.months.Add((int)month);
                else
                    errors.Add($"{where}: months are 1 to 12.");
            c.wants = Json.Str(o, "wants");
            if (c.wants != "" && c.wants != "good_name" && c.wants != "bad_name")
                errors.Add($"{where}: wants is good_name or bad_name.");
            if (c.arrival && c.weight > 0.0)
                errors.Add($"{where}: a card is either on arrival or drawn (weight), not both.");
            if (c.weight < 0.0 || c.rate < 0.0)
                errors.Add($"{where}: weight and rate can't be below 0.");
            if (c.arrival && c.stage != Stage.Harbour)
                errors.Add($"{where}: only a harbour card comes on arrival.");
            c.titleKey = Json.Str(o, "title_key");
            c.title = Json.Str(o, "title");
            c.intentionKey = Json.Str(o, "intention_key");
            c.intention = Json.Str(o, "intention");
            if (c.titleKey.Length == 0)
                errors.Add($"{where}: a card needs a title_key.");
            Key(book, c.titleKey);
            Key(book, c.intentionKey);

            foreach (var kv in Json.Obj(o, "actors") ?? new Dictionary<string, object>())
            {
                var a = kv.Value as Dictionary<string, object>;
                if (a == null)
                {
                    errors.Add($"{where}: actor '{kv.Key}' is not an object.");
                    continue;
                }
                foreach (var k in a.Keys)
                    if (!ActorFields.Contains(k))
                        book.warnings.Add($"{where} actor {kv.Key}: unknown field \"{k}\" (skipped).");
                var spec = new RoleSpec
                {
                    key = kv.Key,
                    role = Json.Str(a, "role", kv.Key),
                    people = Json.Str(a, "people", "port"),
                    faith = Json.Str(a, "faith", "port"),
                    tongue = Json.Str(a, "tongue", "port"),
                    sex = Json.Str(a, "sex"),
                    fixedRole = Json.Bool(a, "fixed", true),
                    cast = Json.Str(a, "cast"),
                    name = Json.Str(a, "name"),
                    castFrom = (int)Json.Num(a, "cast_from"),
                    castTo = (int)Json.Num(a, "cast_to"),
                };
                // (a named person needs the name; their people may be the port's own, as for anybody of the place)
                if (spec.cast.Length > 0 && spec.name.Length == 0)
                    errors.Add($"{where} actor {kv.Key}: a named person (\"cast\") needs a \"name\".");
                if (spec.cast.Length == 0 && (spec.name.Length > 0 || spec.castFrom != 0 || spec.castTo != 0))
                    errors.Add($"{where} actor {kv.Key}: \"name\", \"cast_from\" and \"cast_to\" belong to a named person (\"cast\").");
                var age = Json.Arr(a, "age");
                if (age != null)
                {
                    if (age.Count == 2 && age[0] is double a0 && age[1] is double a1 && a0 <= a1)
                    {
                        spec.ageMin = (int)a0;
                        spec.ageMax = (int)a1;
                    }
                    else
                        errors.Add($"{where} actor {kv.Key}: age is [from, to].");
                }
                if (book.roles.Role(spec.role) == null)
                    errors.Add($"{where} actor {kv.Key}: no role '{spec.role}' in npc_roles_793.json.");
                c.actors.Add(spec);
            }

            foreach (var lo in Json.Arr(o, "lines") ?? new List<object>())
            {
                var l = lo as Dictionary<string, object>;
                if (l == null)
                {
                    errors.Add($"{where}: a line is not an object.");
                    continue;
                }
                foreach (var k in l.Keys)
                    if (!LineFields.Contains(k))
                        book.warnings.Add($"{where} line: unknown field \"{k}\" (skipped).");
                var line = new LineDef { who = Json.Str(l, "who"), key = Json.Str(l, "key"), tongue = Json.Str(l, "tongue"), turns = Json.Str(l, "turns") };
                line.when = Condition(Json.Str(l, "when"), book, errors, $"{where} line {line.key}");
                if (line.key.Length == 0)
                    errors.Add($"{where}: a line needs a key.");
                if (line.who.Length > 0 && c.Actor(line.who) == null)
                    errors.Add($"{where} line {line.key}: '{line.who}' is not one of the card's actors.");
                Key(book, line.key);
                c.lines.Add(line);
            }
            // (9 Oct 2026) the standing cast this card cannot do without: who speaks half its spoken lines or more
            int spoken = 0;
            foreach (var l in c.lines)
                if (l.who.Length > 0)
                    spoken++;
            foreach (var spec in c.actors)
            {
                if (spec.cast.Length == 0 || spoken == 0)
                    continue;
                int his = 0;
                foreach (var l in c.lines)
                    if (l.who == spec.key)
                        his++;
                if (his * 2 >= spoken)
                    c.leans.Add(spec.cast);
            }

            int defaults = 0;
            foreach (var oo in Json.Arr(o, "options") ?? new List<object>())
            {
                var op = oo as Dictionary<string, object>;
                if (op == null)
                {
                    errors.Add($"{where}: an option is not an object.");
                    continue;
                }
                var opt = new OptionDef
                {
                    key = Json.Str(op, "key"),
                    labelKey = Json.Str(op, "label_key"),
                    label = Json.Str(op, "label"),
                    then = Json.Str(op, "then"),
                    isDefault = Json.Bool(op, "default"),
                    index = c.options.Count,
                    outKey = Json.Str(op, "out"),
                    outSuccess = Json.Str(op, "out_success"),
                    outFailure = Json.Str(op, "out_failure"),
                };
                string ow = $"{where} option {(opt.key.Length > 0 ? opt.key : "#" + (opt.index + 1))}";
                foreach (var k in op.Keys)
                    if (!OptionFields.Contains(k))
                        book.warnings.Add($"{ow}: unknown field \"{k}\" (skipped).");
                if (opt.key.Length == 0)
                    errors.Add($"{ow}: an option needs a key.");
                else if (c.Option(opt.key) != null)
                    errors.Add($"{ow}: two options with the key '{opt.key}'.");
                if (opt.labelKey.Length == 0)
                    errors.Add($"{ow}: an option needs a label_key.");
                Key(book, opt.labelKey);
                foreach (string named in new[] { opt.outKey, opt.outSuccess, opt.outFailure })
                    if (named.Length > 0)
                        Key(book, named);
                opt.check = Condition(Json.Str(op, "check"), book, errors, ow + " check");
                op.TryGetValue("cost", out var cost);
                opt.cost = Effects.ParseCost(cost, errors, ow + " cost");
                op.TryGetValue("effects", out var fx);
                opt.effects = Effects.ParseList(fx, errors, ow);
                if (opt.isDefault)
                    defaults++;
                c.options.Add(opt);
            }
            if (c.options.Count == 0)
                errors.Add($"{where}: a card needs at least one option.");
            if (c.options.Count > 9)
                errors.Add($"{where}: at most nine options (keys 1-9).");
            if (defaults > 1)
                errors.Add($"{where}: at most one default option.");

            foreach (var kv in Json.Obj(o, "memory") ?? new Dictionary<string, object>())
            {
                string optionKey = kv.Key, branch = "";
                int dot = kv.Key.IndexOf('.');
                if (dot > 0)
                {
                    optionKey = kv.Key.Substring(0, dot);
                    branch = kv.Key.Substring(dot + 1);
                }
                var opt = c.Option(optionKey);
                if (opt == null)
                    errors.Add($"{where} memory '{kv.Key}': no option '{optionKey}'.");
                else if (branch.Length > 0 && (opt.Chance == null || (branch != "success" && branch != "failure")))
                    errors.Add($"{where} memory '{kv.Key}': only an option with a chance has .success and .failure.");
                if (!(kv.Value is string note) || note.Length == 0)
                    errors.Add($"{where} memory '{kv.Key}': the note is a text.");
                else
                    c.memory[kv.Key] = note;
            }

            // every npc.<role> a condition reads is one of the card's actors
            var names = new List<string>();
            c.when.Names(names);
            foreach (var l in c.lines)
                l.when.Names(names);
            foreach (var opt in c.options)
                opt.check.Names(names);
            foreach (var opt in c.options)
                foreach (var ch in opt.effects.OfType<ChanceEffect>())
                    if (ch.by.Length > 0)
                        names.Add(ch.by);
            foreach (var n in names.Distinct())
            {
                string role = VarNames.RoleOf(n);
                if (role != null && c.Actor(role) == null)
                    errors.Add($"{where}: '{n}' reads '{role}', not one of the card's actors.");
            }
            return c;
        }

        static Cond Condition(string text, EncounterBook book, List<string> errors, string where)
        {
            try
            {
                return Cond.Parse(text, book.words);
            }
            catch (FormatException e)
            {
                errors.Add($"{where}: {e.Message}");
                return Cond.True;
            }
        }

        /// <summary>A writing key a card uses: noted as missing if writing.json has no entry for it.</summary>
        static void Key(EncounterBook book, string key)
        {
            if (string.IsNullOrEmpty(key) || book.writing.ContainsKey(key) || book.missingKeys.Contains(key))
                return;
            book.missingKeys.Add(key);
        }

        static void LoadWriting(EncounterBook book, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                book.errors.Add("writing.json: missing.");
                return;
            }
            object o;
            try
            {
                o = Json.Parse(json);
            }
            catch (FormatException e)
            {
                book.errors.Add("writing.json: " + e.Message);
                return;
            }
            foreach (var e in Json.Arr(o, "entries") ?? new List<object>())
            {
                string key = Json.Str(e, "key");
                if (key.Length > 0)
                    book.writing[key] = Json.Str(e, "text");
            }
        }

        static void LoadTraffic(EncounterBook book, string json, List<string> errors)
        {
            if (string.IsNullOrEmpty(json))
                return;
            object o;
            try
            {
                o = Json.Parse(json);
            }
            catch (FormatException e)
            {
                errors.Add("traffic_793.json: " + e.Message);
                return;
            }
            foreach (var kv in Json.Obj(Json.Obj(o, "harbours"), "ports") ?? new Dictionary<string, object>())
            {
                var p = Json.Arr(kv.Value, "present");
                if (p == null || p.Count != 12)
                    continue;
                var months = new double[12];
                for (int i = 0; i < 12; i++)
                    months[i] = p[i] is double d ? d : 0.0;
                book.present[kv.Key] = months;
            }
        }

        /// <summary>The keys of an object in a table (the deed kinds), or null without the table.</summary>
        static HashSet<string> Keys(string json, string field)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                var o = Json.Obj(Json.Parse(json), field);
                return o == null ? null : new HashSet<string>(o.Keys);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>The ids in a table's list (goods, ports), or null without the table.</summary>
        static HashSet<string> Ids(string json, string field)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                var list = Json.Arr(Json.Parse(json), field);
                if (list == null)
                    return null;
                var ids = new HashSet<string>();
                foreach (var x in list)
                {
                    string id = Json.Str(x, "id");
                    if (id.Length > 0)
                        ids.Add(id);
                }
                return ids;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
