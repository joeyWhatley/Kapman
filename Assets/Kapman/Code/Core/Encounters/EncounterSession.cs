using System;
using System.Collections.Generic;
using System.Linq;

namespace Kapman.Encounters
{
    /// <summary>What a card needs from the world when it opens and when a choice is made.</summary>
    public sealed class EncounterWorld
    {
        public EncounterBook book;
        public EncounterState state;
        public NpcRegistry npcs;
        public IGameEffects game;
        /// <summary>The variables (the runner fills the live game's into them before a card opens).</summary>
        public EncounterVars vars;
        public string merchant = "player";
        /// <summary>The port of the call (people, faith, realm); an empty id at sea.</summary>
        public PortFacts port = new PortFacts();
        public int year = 793;
        /// <summary>Game days since the start.</summary>
        public double day;
        /// <summary>The dice for a chance: 0 (inclusive) to 1 (exclusive).</summary>
        public Func<double> roll = () => 0.5;
    }

    /// <summary>A line as it shows: who says it, the words (the user's, or the [write: ...] placeholder) and its tongue.</summary>
    public sealed class ShownLine
    {
        public LineDef def;
        public Npc who;
        public string text = "", tongue = "";
        /// <summary>The real line of the tongues file it comes up as before it turns into English ("" for none).</summary>
        public string turns = "";
        public bool written;
    }

    /// <summary>An option as it shows: its label, whether it can be chosen (else the rule, in words), its odds as a word
    /// where it has a chance (decision 18: sure, likely, doubtful), and whether Enter chooses it.</summary>
    public sealed class ShownOption
    {
        public OptionDef def;
        public string label = "";
        /// <summary>The tag its label began with in square brackets ("Tongue", "Frisian to Frisian"), shown before the
        /// words; "" for none. The label is the words without it.</summary>
        public string tag = "";
        public bool labelWritten, enabled, isDefault;
        public string rule = "", odds = "";
        public double chance = -1.0;

        /// <summary>"[Tongue] Ask him to wait till Lammas": the tag in its brackets and the words, as they are shown.</summary>
        public string Tagged => tag.Length > 0 ? "[" + tag + "] " + label : label;

        /// <summary>Takes a tag in square brackets off the front of a label: "[Tongue] Ask him" gives "Tongue" and
        /// "Ask him". A label with no such tag, or with nothing after it, is left as it is.</summary>
        public static void SplitTag(string text, out string tag, out string label)
        {
            tag = "";
            label = text ?? "";
            string s = label.TrimStart();
            if (s.Length < 3 || s[0] != '[')
                return;
            int end = s.IndexOf(']');
            if (end < 2)
                return;
            string rest = s.Substring(end + 1).Trim();
            string inside = s.Substring(1, end - 1).Trim();
            if (rest.Length == 0 || inside.Length == 0)
                return;
            tag = inside;
            label = rest;
        }
    }

    /// <summary>What came of a choice.</summary>
    public sealed class ChoiceResult
    {
        public bool ok;
        public string option = "", branch = "", then = "";
        public readonly List<string> facts = new List<string>();
        /// <summary>What the merchant did, as a sentence ("You sat low and listened."): the card's own line of what the
        /// people remember of the choice, else the answer given. "" for a choice that was not taken.</summary>
        public string told = "";
        /// <summary>What came of it as the writers told it (the choice's written end: Outcomes), or "" where none is
        /// written. The card shows it; the log keeps to the short telling.</summary>
        public string written = "";
        public string Text => told.Length > 0 ? (told + " " + string.Join(" ", facts)).Trim() : string.Join(" ", facts);
        /// <summary>What the effects did, in their own sentences ("" for none).</summary>
        public string Facts => string.Join(" ", facts);
    }

    /// <summary>
    /// A CARD IN PLAY - opened from its data: its people found or forged (a fixed role's holder at this port, else a new
    /// person), the lines whose "when" holds, the options with their checks (a failed one greyed, with its rule in words)
    /// and odds words; then the merchant's choice: the cost paid, the effects run, what the people remember written
    /// (the card's memory line for the choice and the effects' own words), and the next card put first in line ("then").
    /// Pure: the panel shows it, the tests play it.
    /// </summary>
    public sealed class EncounterSession
    {
        public readonly EncounterDef card;
        public readonly EncounterWorld world;
        public readonly Dictionary<string, Npc> actors = new Dictionary<string, Npc>();
        public readonly List<ShownLine> lines = new List<ShownLine>();
        public readonly List<ShownOption> options = new List<ShownOption>();
        /// <summary>The merchant has called here before: the default option is the one-key choice (Enter).</summary>
        public bool returning;
        public ChoiceResult result;

        EncounterSession(EncounterDef card, EncounterWorld world)
        {
            this.card = card;
            this.world = world;
        }

        public bool Chosen => result != null && result.ok;

        /// <summary>The title: the user's words for the title key, else the card's plain title.</summary>
        public string Title => world.book.Written(card.titleKey) ?? (card.title.Length > 0 ? card.title : world.book.Shown(card.titleKey));

        /// <summary>What this step is, in plain words: the user's for the intention key, else the card's plain intention.</summary>
        public string Intention => world.book.Written(card.intentionKey) ?? card.intention;

        /// <summary>The option Enter chooses (-1: none): the default, on a return, if it can be chosen.</summary>
        public int DefaultIndex
        {
            get
            {
                for (int i = 0; i < options.Count; i++)
                    if (options[i].isDefault)
                        return i;
                return -1;
            }
        }

        /// <summary>"Thiadger, the toll man · easy": the person's name, role and how they feel toward the merchant, as a word.</summary>
        public string Speaker(Npc who)
        {
            if (who == null)
                return "";
            string title = world.book.roles.Role(who.role)?.title ?? who.role.Replace('_', ' ');
            double feeling = world.npcs.Feeling(who.id, world.merchant);
            string word = world.book.words.WordFor(WordTable.Feeling, feeling);
            return $"{(who.name.Length > 0 ? who.name + ", " : "")}{title}{(word.Length > 0 ? " · " + word : "")}";
        }

        /// <summary>Opens a card: its people, the lines, the options. The card counts as shown (its cool-down starts).</summary>
        public static EncounterSession Open(EncounterDef card, EncounterWorld world) => Open(card, world, null);

        /// <summary>As Open, with a cast already drawn (a saved game's open card comes back with the people it had): part
        /// (the actor's key) to the person's id. A part whose person is no longer known is drawn as usual.</summary>
        public static EncounterSession Open(EncounterDef card, EncounterWorld world, Dictionary<string, string> cast)
        {
            var s = new EncounterSession(card, world);
            foreach (var spec in card.actors)
            {
                Npc kept = cast != null && cast.TryGetValue(spec.key, out string id) ? world.npcs.Get(id) : null;
                // (one of the standing cast is the same person on every card; anybody else is whoever holds that trade here)
                s.actors[spec.key] = kept ?? (spec.CastIn(world.year) ? world.npcs.Cast(spec, world.port, world.year, world.day)
                                              : spec.fixedRole ? world.npcs.Holder(spec, world.port, world.year, world.day)
                                                               : world.npcs.Forge(spec, world.port, world.year));
            }
            var vars = world.vars;
            vars.actors = s.actors;
            vars.port = world.port.id;
            vars.day = world.day;
            vars.merchant = world.merchant;
            vars.state = world.state;
            vars.npcs = world.npcs;
            s.returning = world.state.VisitsBefore(world.port.id) > 0;
            foreach (var l in card.lines)
            {
                if (!l.when.Eval(vars))
                    continue;
                var who = l.who.Length > 0 && s.actors.TryGetValue(l.who, out var n) ? n : null;
                s.lines.Add(new ShownLine
                {
                    def = l, who = who, text = world.book.Shown(l.key), written = world.book.Written(l.key) != null,
                    tongue = l.tongue.Length > 0 ? l.tongue : who?.tongue ?? "", turns = l.turns,
                });
            }
            foreach (var o in card.options)
            {
                var shown = new ShownOption { def = o, enabled = o.check.Eval(vars) };
                string written = world.book.Written(o.labelKey);
                shown.labelWritten = written != null;
                ShownOption.SplitTag(written ?? (o.label.Length > 0 ? o.label : world.book.Shown(o.labelKey)), out shown.tag, out shown.label);
                if (!shown.enabled)
                    shown.rule = o.check.Describe(world.book.words);
                var chance = o.Chance;
                if (chance != null)
                {
                    shown.chance = chance.Odds(vars);
                    shown.odds = world.book.words.WordFor(WordTable.Odds, shown.chance);
                }
                shown.isDefault = o.isDefault && s.returning && shown.enabled;
                s.options.Add(shown);
            }
            world.state.Shown(card, world.day);
            return s;
        }

        /// <summary>
        /// The merchant chooses option `index` (from 0). A greyed option does nothing and says its rule. Otherwise: the cost
        /// is paid, the effects run, the people remember, they have met, and the "then" card goes first in line.
        /// </summary>
        public ChoiceResult Choose(int index)
        {
            var r = new ChoiceResult();
            if (Chosen)
            {
                r.facts.Add("Already chosen.");
                return r;
            }
            if (index < 0 || index >= options.Count)
            {
                r.facts.Add("No such choice.");
                return r;
            }
            var shown = options[index];
            r.option = shown.def.key;
            if (!shown.enabled)
            {
                r.facts.Add(shown.rule.Length > 0 ? shown.rule : "Not now.");
                return r;
            }
            var ctx = new EffectContext
            {
                game = world.game, state = world.state, npcs = world.npcs, words = world.book.words, vars = world.vars,
                merchant = world.merchant, port = world.port.id, day = world.day, card = card.id, option = shown.def.key,
                actors = actors, roll = world.roll,
            };
            Effects.Apply(shown.def.cost, ctx);
            Effects.Apply(shown.def.effects, ctx);
            // what the people remember of it: the card's line for the choice (with its branch), on what the effects wrote
            string remembered = ctx.Remembered;
            if (!card.memory.TryGetValue(remembered, out string note) && ctx.branch.Length > 0)
                card.memory.TryGetValue(shown.def.key, out note);
            foreach (var who in actors.Values)
            {
                if (!string.IsNullOrEmpty(note))
                    world.npcs.Note(who.id, world.merchant, world.day, card.id, remembered, note);
                world.npcs.MarkMet(who.id, world.merchant);
            }
            if (shown.def.then.Length > 0)
                world.state.Enqueue(shown.def.then, true);
            ctx.CloseFeelings();
            r.ok = true;
            r.branch = ctx.branch;
            r.then = shown.def.then;
            r.told = !string.IsNullOrEmpty(note) ? LogProse.YouDid(note) : LogProse.Answered(shown.label);
            r.written = Outcomes.Text(world.book, shown.def, ctx.branch);
            r.facts.AddRange(ctx.facts);
            if (r.facts.Count == 0 && r.written.Length == 0)
                r.facts.Add("Nothing more came of it.");
            result = r;
            return r;
        }

        /// <summary>The people of the card, in its actors' order.</summary>
        public IEnumerable<Npc> People => card.actors.Select(a => actors.TryGetValue(a.key, out var n) ? n : null).Where(n => n != null);
    }
}
