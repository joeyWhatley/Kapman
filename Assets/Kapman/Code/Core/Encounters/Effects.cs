using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Game;

namespace Kapman.Encounters
{
    /// <summary>Everything an effect needs while it runs: the live game, the encounter layer's own state, the people, the
    /// variables (for a chance's stat), the dice, and the facts it adds to the card's outcome.</summary>
    public sealed class EffectContext
    {
        public IGameEffects game;
        public EncounterState state;
        public NpcRegistry npcs;
        public WordTable words;
        public IVars vars;
        public string merchant = "player";
        /// <summary>The port of the call ("" at sea).</summary>
        public string port = "";
        /// <summary>Game days since the start (with the hours as a fraction).</summary>
        public double day;
        public string card = "", option = "";
        /// <summary>The card's people by actor key.</summary>
        public Dictionary<string, Npc> actors = new Dictionary<string, Npc>();
        /// <summary>The dice: 0 (inclusive) to 1 (exclusive).</summary>
        public Func<double> roll = () => 0.5;
        /// <summary>"success" or "failure" once a chance has been rolled.</summary>
        public string branch = "";
        /// <summary>What happened, as plain facts, in order.</summary>
        public readonly List<string> facts = new List<string>();

        /// <summary>The option as the people remember it: "hide.failure" after a failed chance, else "hide".</summary>
        public string Remembered => branch.Length > 0 ? option + "." + branch : option;

        public void Say(string fact)
        {
            if (!string.IsNullOrWhiteSpace(fact))
                facts.Add(fact.Trim());
        }

        /// <summary>Whose feeling toward the merchant the effects moved, and which way (gathered into one sentence at the
        /// end: DECISIONS 7 Oct, item 39: "one sentence for a card, not one line for each person").</summary>
        public readonly List<(string name, double n)> felt = new List<(string name, double n)>();

        /// <summary>The feelings moved so far, said as one fact and forgotten. Called once when the effects have run.</summary>
        public void CloseFeelings()
        {
            Say(LogProse.Felt(felt));
            felt.Clear();
        }
    }

    /// <summary>One effect of a choice: a verb with its arguments ("toll eased 30"), or a chance with two branches.</summary>
    public abstract class Effect
    {
        /// <summary>The text it was parsed from.</summary>
        public string source = "";

        public abstract void Apply(EffectContext ctx);

        /// <summary>Every effect inside it, itself included (a chance's branches too), for the loader's cross-checks.</summary>
        public abstract IEnumerable<VerbEffect> Verbs();
    }

    /// <summary>
    /// A CHANCE (npc_text_encounters.md 2.4, 2.7): <c>{"chance": 0.75, "by": "crew.trade_best", "success": [..],
    /// "failure": [..]}</c>, one roll, p = chance + 0.05 x (stat - 3), kept within 0.05..0.95. The panel shows it as a
    /// word (sure, likely, doubtful), never the number.
    /// </summary>
    public sealed class ChanceEffect : Effect
    {
        public double chance = 0.5;
        public string by = "";
        public List<Effect> success = new List<Effect>(), failure = new List<Effect>();

        /// <summary>The odds now, with the stat's say.</summary>
        public double Odds(IVars vars)
        {
            double p = chance;
            if (!string.IsNullOrEmpty(by) && vars != null)
                p += 0.05 * (vars.Num(by) - 3.0);
            return Math.Max(0.05, Math.Min(0.95, p));
        }

        public override void Apply(EffectContext ctx)
        {
            bool won = ctx.roll() < Odds(ctx.vars);
            ctx.branch = won ? "success" : "failure";
            Effects.Apply(won ? success : failure, ctx);
        }

        public override IEnumerable<VerbEffect> Verbs()
        {
            foreach (var e in success)
                foreach (var v in e.Verbs())
                    yield return v;
            foreach (var e in failure)
                foreach (var v in e.Verbs())
                    yield return v;
        }
    }

    /// <summary>One verb and its arguments, parsed once at load (the table in Effects).</summary>
    public sealed class VerbEffect : Effect
    {
        public string verb = "";
        /// <summary>The verb's words (a good, a port, a role, a flag, a card ...), in the grammar's order.</summary>
        public string a = "", b = "", c = "";
        /// <summary>The verb's numbers (an amount, a factor, a value).</summary>
        public double n, m;
        /// <summary>Days (a lapse, a delay); 0: none given.</summary>
        public double days;
        public TollMode toll;

        public override IEnumerable<VerbEffect> Verbs()
        {
            yield return this;
        }

        public override void Apply(EffectContext ctx)
        {
            var g = ctx.game;
            switch (verb)
            {
                case "silver": ctx.Say(g?.Silver(n)); break;
                case "goods": ctx.Say(g?.Goods(a, (int)n)); break;
                case "health": ctx.Say(g?.Health(n)); break;
                case "morale": ctx.Say(g?.Morale(n)); break;
                case "men": ctx.Say(g?.Men((int)n)); break;
                case "deed": ctx.Say(g?.Deed(a, ctx.port)); break;
                case "news": ctx.Say(g?.News(a, Port(b, ctx))); break;
                case "price_shock": ctx.Say(g?.PriceShock(Port(a, ctx), b, n, days)); break;
                case "commission": ctx.Say(g?.Commission(ctx.port, a, (int)n, days, m)); break;
                case "give_text": ctx.Say(g?.GiveText(a, FirstActorName(ctx), ctx.port)); break;
                case "give_riddle": ctx.Say(g?.GiveRiddle(a, FirstActorName(ctx), ctx.port)); break;
                case "open_fetch": ctx.Say(g?.OpenFetch(a)); break;
                case "hire": ctx.Say(g?.Hire(Actor(a, ctx))); break;
                case "pay_off": ctx.Say(g?.PayOff(Actor(a, ctx))); break;
                case "passenger": ctx.Say(g?.Passenger(Actor(a, ctx), Port(b, ctx))); break;
                case "encounter": ctx.Say(g?.Encounter(a)); break;
                case "law": ctx.Say(g?.Law(a, days, ctx.port)); break;
                case "learn": ctx.Say(g?.Learn(a, n)); break;
                case "tongue": ctx.Say(g?.Tongue(a, n)); break;
                // (8 Oct 2026: events move the stats. A mark on a stat for some days or until a flag is set; a mark
                // taken off; the base moved for good)
                case "mark": ctx.Say(g?.Mark(a, n, b, days, c)); break;
                case "unmark": ctx.Say(g?.Unmark(a)); break;
                case "stat": ctx.Say(g?.StatBase(a, n)); break;
                case "toll":
                    ctx.state?.SetToll(ctx.port, toll, ctx.day, days);
                    ctx.Say(toll == TollMode.None ? "No toll on your sales here this call."
                          : toll == TollMode.Eased ? $"Half the toll on your sales here for {Days(days)}."
                          : "The toll is taken on what you sell here.");
                    break;
                case "flag":
                    if (b == "add")
                        ctx.state?.AddFlag(a, n, ctx.day);                   // "flag <name> +n": a count; a lapsing flag keeps its day
                    else
                        ctx.state?.SetFlag(a, n, ctx.day, days);
                    break;
                case "schedule":
                    ctx.state?.Schedule(a, ctx.day + days, string.IsNullOrEmpty(b) ? "" : Port(b, ctx));
                    break;
                case "npc":
                {
                    var who = Actor(a, ctx);
                    if (who == null || ctx.npcs == null)
                        break;
                    ctx.npcs.Feel(who.id, ctx.merchant, n, ctx.day, ctx.card, ctx.Remembered, b);
                    if (n != 0.0)
                        ctx.felt.Add((Name(who), n));                        // (said at the end, all together: CloseFeelings)
                    break;
                }
                default:
                    throw new InvalidOperationException("Encounter effect: unknown verb '" + verb + "' (the loader should have refused it).");
            }
        }

        static string Days(double d) => d == 1.0 ? "a day" : d.ToString("0.#", CultureInfo.InvariantCulture) + " days";

        static string Port(string arg, EffectContext ctx) => string.IsNullOrEmpty(arg) || arg == "here" ? ctx.port : arg;

        static Npc Actor(string key, EffectContext ctx) => key != null && ctx.actors.TryGetValue(key, out var n) ? n : null;

        static string FirstActorName(EffectContext ctx)
        {
            foreach (var kv in ctx.actors)
                return Name(kv.Value);
            return "";
        }

        static string Name(Npc n) => string.IsNullOrEmpty(n.name) ? "The " + n.role.Replace('_', ' ') : n.name;
    }

    /// <summary>
    /// THE EFFECTS (npc_text_encounters.md 2.7): one effect per string, <c>verb arg*</c>, a good id or a key one token; a
    /// chance is an object. The loader refuses any verb not in this table, and any verb whose arguments are not as the
    /// grammar has them:
    /// <code>
    /// silver ±n | goods &lt;good&gt; ±n | health ±f | morale ±f | men ±n
    /// toll sales | toll none | toll eased &lt;days&gt;
    /// deed &lt;kind&gt;                                  npc &lt;role&gt; feeling ±n [memory word]
    /// flag &lt;name&gt; [= value] [for &lt;days&gt;]            schedule &lt;card&gt; in &lt;days&gt; [at &lt;port&gt;]
    /// flag &lt;name&gt; +n | flag &lt;name&gt; -n               (adds to the flag's value: a count)
    /// news &lt;key&gt; at &lt;port&gt;                         price_shock &lt;port&gt; &lt;good&gt; x&lt;f&gt; for &lt;days&gt;
    /// commission &lt;good&gt; &lt;n&gt; by &lt;days&gt; premium &lt;f&gt;
    /// give_text &lt;id&gt; | give_riddle &lt;id&gt; | open_fetch &lt;id&gt;
    /// hire &lt;role&gt; | pay_off &lt;role&gt; | passenger &lt;role&gt; to &lt;port&gt;
    /// encounter close_and_hail|heave_to|run       law &lt;kind&gt; [in &lt;days&gt;]
    /// learn latin|runes +f                        tongue &lt;name&gt; = f
    /// </code>
    /// A port may be "here" (the call's). An option's cost is written "silver 12" (or "goods &lt;good&gt; 2", "men 1") and is paid.
    /// </summary>
    public static class Effects
    {
        public static readonly string[] Verbs =
        {
            "silver", "goods", "health", "morale", "men", "toll", "deed", "npc", "flag", "schedule", "news", "price_shock",
            "commission", "give_text", "give_riddle", "open_fetch", "hire", "pay_off", "passenger", "encounter", "law", "learn",
            "tongue", "mark", "unmark", "stat",
        };

        /// <summary>The five stats a mark may lie on (the game's Self.StatIds; kept here too so that a card with a stat
        /// that is none of them is refused when the cards are read).</summary>
        public static readonly string[] Stats = { "tongue", "name", "nerve", "lore", "troth" };

        static bool IsStat(string s) => Array.IndexOf(Stats, s) >= 0;

        static bool Signed(string s, out double v, int most)
        {
            v = 0.0;
            return !string.IsNullOrEmpty(s) && (s[0] == '+' || s[0] == '-') && Number(s, out v) && v != 0.0 && Math.Abs(v) <= most && v == Math.Round(v);
        }

        static readonly string[] SeaOrders = { "close_and_hail", "heave_to", "run" };

        /// <summary>Runs effects in order.</summary>
        public static void Apply(List<Effect> list, EffectContext ctx)
        {
            if (list == null)
                return;
            foreach (var e in list)
                e.Apply(ctx);
        }

        /// <summary>A list of effects from the data (a list, or one string or chance); errors go in the list with where.</summary>
        public static List<Effect> ParseList(object node, List<string> errors, string where)
        {
            var list = new List<Effect>();
            if (node == null)
                return list;
            if (node is List<object> items)
            {
                foreach (var item in items)
                {
                    var e = Parse(item, errors, where);
                    if (e != null)
                        list.Add(e);
                }
                return list;
            }
            var one = Parse(node, errors, where);
            if (one != null)
                list.Add(one);
            return list;
        }

        /// <summary>An option's cost: "silver 12" (paid), "goods &lt;good&gt; 2", "men 1"; a list of such, or one.</summary>
        public static List<Effect> ParseCost(object node, List<string> errors, string where)
        {
            var list = new List<Effect>();
            if (node == null)
                return list;
            var items = node as List<object> ?? new List<object> { node };
            foreach (var item in items)
            {
                if (!(item is string s))
                {
                    errors.Add($"{where}: a cost is written as text (\"silver 12\").");
                    continue;
                }
                var t = Tokens(s);
                if (t.Length == 2 && t[0] == "silver" && Number(t[1], out double d) && d >= 0)
                    list.Add(new VerbEffect { source = s, verb = "silver", n = -d });
                else if (t.Length == 2 && t[0] == "men" && Whole(t[1], out int men) && men >= 0)
                    list.Add(new VerbEffect { source = s, verb = "men", n = -men });
                else if (t.Length == 3 && t[0] == "goods" && Whole(t[2], out int units) && units >= 0)
                    list.Add(new VerbEffect { source = s, verb = "goods", a = t[1], n = -units });
                else
                    errors.Add($"{where}: the cost '{s}' is not 'silver n', 'men n' or 'goods <good> n'.");
            }
            return list;
        }

        /// <summary>One effect from the data: a string or a chance object. Null (and an error) if it doesn't parse.</summary>
        public static Effect Parse(object node, List<string> errors, string where)
        {
            if (node is Dictionary<string, object> obj)
                return ParseChance(obj, errors, where);
            if (node is string s)
            {
                string why = null;
                var e = ParseVerb(s, ref why);
                if (e == null)
                    errors.Add($"{where}: '{s}': {why}");
                return e;
            }
            errors.Add($"{where}: an effect is a string or a chance object.");
            return null;
        }

        static Effect ParseChance(Dictionary<string, object> obj, List<string> errors, string where)
        {
            var c = new ChanceEffect { source = Json.Write(obj) };
            if (!(obj.TryGetValue("chance", out var p) && p is double chance) || chance < 0.0 || chance > 1.0)
            {
                errors.Add($"{where}: a chance object needs \"chance\" between 0 and 1.");
                return null;
            }
            c.chance = chance;
            c.by = Json.Str(obj, "by");
            if (c.by.Length > 0 && (!VarNames.TryType(c.by, out var type, out _) || type != VarType.Number))
                errors.Add($"{where}: the chance's \"by\" '{c.by}' is not a number variable.");
            if (!obj.ContainsKey("success") || !obj.ContainsKey("failure"))
                errors.Add($"{where}: a chance object needs both \"success\" and \"failure\" (an empty list for nothing).");
            c.success = ParseList(Json.Arr(obj, "success"), errors, where + " (success)");
            c.failure = ParseList(Json.Arr(obj, "failure"), errors, where + " (failure)");
            foreach (var key in obj.Keys)
                if (key != "chance" && key != "by" && key != "success" && key != "failure")
                    errors.Add($"{where}: a chance object has no field \"{key}\".");
            return c;
        }

        /// <summary>A verb string by the grammar; null with the reason if it doesn't fit.</summary>
        public static VerbEffect ParseVerb(string s, ref string why)
        {
            var t = Tokens(s);
            if (t.Length == 0)
            {
                why = "empty";
                return null;
            }
            var e = new VerbEffect { source = s, verb = t[0] };
            int argc = t.Length - 1;
            switch (t[0])
            {
                case "silver":
                case "health":
                case "morale":
                    if (argc == 1 && Number(t[1], out e.n))
                        return e;
                    why = $"'{t[0]}' takes one number (+12, -0.1)";
                    return null;
                case "men":
                    if (argc == 1 && Whole(t[1], out int men))
                    {
                        e.n = men;
                        return e;
                    }
                    why = "'men' takes a whole number";
                    return null;
                case "goods":
                    if (argc == 2 && Whole(t[2], out int units))
                    {
                        e.a = t[1];
                        e.n = units;
                        return e;
                    }
                    why = "'goods' takes a good and a whole number";
                    return null;
                case "toll":
                    if (argc == 1 && t[1] == "sales")
                    {
                        e.toll = TollMode.Sales;
                        return e;
                    }
                    if (argc == 1 && t[1] == "none")
                    {
                        e.toll = TollMode.None;
                        return e;
                    }
                    if (argc == 2 && t[1] == "eased" && Number(t[2], out e.days) && e.days > 0)
                    {
                        e.toll = TollMode.Eased;
                        return e;
                    }
                    why = "'toll' is 'toll sales', 'toll none' or 'toll eased <days>'";
                    return null;
                case "deed":
                case "give_text":
                case "give_riddle":
                case "open_fetch":
                case "hire":
                case "pay_off":
                    if (argc == 1)
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = $"'{t[0]}' takes one id";
                    return null;
                case "encounter":
                    if (argc == 1 && Array.IndexOf(SeaOrders, t[1]) >= 0)
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'encounter' takes close_and_hail, heave_to or run";
                    return null;
                case "npc":
                    // npc <role> feeling ±n [memory word]
                    if ((argc == 3 || argc == 4) && t[2] == "feeling" && Number(t[3], out e.n))
                    {
                        e.a = t[1];
                        e.b = argc == 4 ? t[4] : "";
                        return e;
                    }
                    why = "'npc' is 'npc <role> feeling <+n> [memory word]'";
                    return null;
                case "flag":
                {
                    // flag <name> [= value] [for <days>]   |   flag <name> +n   |   flag <name> -n
                    if (argc < 1)
                        break;
                    e.a = t[1];
                    if (argc == 2 && (t[2][0] == '+' || t[2][0] == '-') && Number(t[2], out double by))
                    {
                        // 7 Oct (SCHEMAS.md rule 7; living_world.md step 1): add to a flag, for the counts the world keeps
                        e.n = by;
                        e.b = "add";
                        return e;
                    }
                    e.n = 1.0;
                    int i = 2;
                    if (i < t.Length && t[i] == "=")
                    {
                        if (i + 1 >= t.Length || !Number(t[i + 1], out e.n))
                            break;
                        i += 2;
                    }
                    if (i < t.Length && t[i] == "for")
                    {
                        if (i + 1 >= t.Length || !Number(t[i + 1], out e.days) || e.days <= 0)
                            break;
                        i += 2;
                    }
                    if (i == t.Length)
                        return e;
                    break;
                }
                case "schedule":
                    // schedule <card> in <days> [at <port>]
                    if ((argc == 3 || argc == 5) && t[2] == "in" && Number(t[3], out e.days) && e.days >= 0 && (argc == 3 || t[4] == "at"))
                    {
                        e.a = t[1];
                        e.b = argc == 5 ? t[5] : "";
                        return e;
                    }
                    why = "'schedule' is 'schedule <card> in <days> [at <port>]'";
                    return null;
                case "news":
                    if (argc == 3 && t[2] == "at")
                    {
                        e.a = t[1];
                        e.b = t[3];
                        return e;
                    }
                    why = "'news' is 'news <key> at <port>'";
                    return null;
                case "price_shock":
                    // price_shock <port> <good> x<f> for <days>
                    if (argc == 5 && t[4] == "for" && Factor(t[3], out e.n) && Number(t[5], out e.days) && e.days > 0)
                    {
                        e.a = t[1];
                        e.b = t[2];
                        return e;
                    }
                    why = "'price_shock' is 'price_shock <port> <good> x<f> for <days>'";
                    return null;
                case "commission":
                    // commission <good> <n> by <days> premium <f>
                    if (argc == 6 && t[3] == "by" && t[5] == "premium" && Whole(t[2], out int count) && Number(t[4], out e.days) && Number(t[6], out e.m))
                    {
                        e.a = t[1];
                        e.n = count;
                        return e;
                    }
                    why = "'commission' is 'commission <good> <n> by <days> premium <f>'";
                    return null;
                case "passenger":
                    if (argc == 3 && t[2] == "to")
                    {
                        e.a = t[1];
                        e.b = t[3];
                        return e;
                    }
                    why = "'passenger' is 'passenger <role> to <port>'";
                    return null;
                case "law":
                    // law <kind> [in <days>]
                    if (argc == 1 || (argc == 3 && t[2] == "in" && Number(t[3], out e.days) && e.days >= 0))
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'law' is 'law <kind> [in <days>]'";
                    return null;
                case "learn":
                    if (argc == 2 && (t[1] == "latin" || t[1] == "runes") && Number(t[2], out e.n))
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'learn' is 'learn latin|runes +f'";
                    return null;
                case "tongue":
                    if (argc == 3 && t[2] == "=" && Number(t[3], out e.n) && e.n >= 0.0 && e.n <= 1.0)
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'tongue' is 'tongue <name> = f' (0 to 1)";
                    return null;
                case "mark":
                    // mark <stat> <+n|-n> <id> for <days>   |   mark <stat> <+n|-n> <id> until <flag>
                    if (argc == 5 && IsStat(t[1]) && Signed(t[2], out e.n, 2))
                    {
                        e.a = t[1];
                        e.b = t[3];
                        if (t[4] == "for" && Number(t[5], out e.days) && e.days > 0)
                            return e;
                        if (t[4] == "until")
                        {
                            e.c = t[5];
                            return e;
                        }
                    }
                    why = "'mark' is 'mark <stat> <+1|-1|+2|-2> <id> for <days>' or '... until <flag>' (a stat is tongue, name, nerve, lore or troth)";
                    return null;
                case "unmark":
                    if (argc == 1)
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'unmark' is 'unmark <id>'";
                    return null;
                case "stat":
                    if (argc == 2 && IsStat(t[1]) && Signed(t[2], out e.n, 1))
                    {
                        e.a = t[1];
                        return e;
                    }
                    why = "'stat' is 'stat <stat> +1' or '-1': the base, moved for good (a stat is tongue, name, nerve, lore or troth)";
                    return null;
                default:
                    why = $"unknown verb '{t[0]}' (known: {string.Join(", ", Verbs)})";
                    return null;
            }
            why = "'flag' is 'flag <name> [= value] [for <days>]', or 'flag <name> +n' to add to it";
            return null;
        }

        static string[] Tokens(string s) => (s ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        static bool Number(string s, out double v) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);

        static bool Whole(string s, out int v) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);

        /// <summary>"x1.5", "×1.5" or "*1.5".</summary>
        static bool Factor(string s, out double v)
        {
            v = 0.0;
            if (string.IsNullOrEmpty(s) || (s[0] != 'x' && s[0] != '×' && s[0] != '*'))
                return false;
            return Number(s.Substring(1), out v) && v > 0.0;
        }
    }
}
