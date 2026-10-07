using System.Collections.Generic;
using Kapman.Encounters;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The live game as the encounter tests see it: every call written down (npc_text_encounters.md 4.1), the silver kept
    /// as a number so a cost can be checked, and nothing else changed.
    /// </summary>
    public sealed class FakeGame : IGameEffects
    {
        public readonly List<string> calls = new List<string>();
        public readonly List<(string kind, string port)> deeds = new List<(string, string)>();
        public readonly List<(string kind, double days, string port)> laws = new List<(string, double, string)>();
        public double silver = 100.0;

        static string Inv(System.FormattableString s) => System.FormattableString.Invariant(s);

        string Log(string call)
        {
            calls.Add(call);
            return call + ".";
        }

        public string Silver(double deniers)
        {
            silver += deniers;
            return Log(Inv($"silver {deniers:+0.##;-0.##}"));
        }
        public string Goods(string good, int count) => Log(Inv($"goods {good} {count:+0;-0}"));
        public string Health(double share) => Log(Inv($"health {share:+0.##;-0.##}"));
        public string Morale(double share) => Log(Inv($"morale {share:+0.##;-0.##}"));
        public string Men(int count) => Log(Inv($"men {count:+0;-0}"));
        public string Deed(string kind, string port)
        {
            deeds.Add((kind, port));
            return Log(Inv($"deed {kind} at {port}"));
        }
        public string News(string key, string port) => Log(Inv($"news {key} at {port}"));
        public string PriceShock(string port, string good, double factor, double days) => Log(Inv($"price_shock {port} {good} x{factor} for {days}"));
        public string Commission(string port, string good, int count, double days, double premium) => Log(Inv($"commission {good} {count} by {days} premium {premium} at {port}"));
        public string GiveText(string id, string from, string port) => Log(Inv($"give_text {id} from {from}"));
        public string GiveRiddle(string id, string from, string port) => Log(Inv($"give_riddle {id} from {from}"));
        public string OpenFetch(string id) => Log(Inv($"open_fetch {id}"));
        public string Hire(Npc npc) => Log(Inv($"hire {npc?.id}"));
        public string PayOff(Npc npc) => Log(Inv($"pay_off {npc?.id}"));
        public string Passenger(Npc npc, string port) => Log(Inv($"passenger {npc?.id} to {port}"));
        public string Encounter(string kind) => Log(Inv($"encounter {kind}"));
        public string Law(string kind, double days, string port)
        {
            laws.Add((kind, days, port));
            return Log(Inv($"law {kind} in {days} at {port}"));
        }
        public string Learn(string what, double amount) => Log(Inv($"learn {what} {amount:+0.##;-0.##}"));
        public string Tongue(string name, double level) => Log(Inv($"tongue {name} = {level}"));
    }

    /// <summary>The effects of a choice (npc_text_encounters.md 2.7): each verb does exactly what it says, through the fake
    /// game or the encounter layer's own state; chances take the branch the dice say; flags, the toll's ease and waivers
    /// lapse when they should; and the loader's grammar refuses what isn't in it.</summary>
    public class EffectsTests
    {
        static List<Effect> Parse(params object[] effects)
        {
            var errors = new List<string>();
            var list = Effects.ParseList(new List<object>(effects), errors, "test");
            Assert.IsEmpty(errors, string.Join("\n", errors));
            return list;
        }

        static (EffectContext ctx, FakeGame game) Context(double day = 1.0, double roll = 0.5)
        {
            var book = EncounterData.Book();
            var game = new FakeGame();
            var npcs = new NpcRegistry(793, book.roles, book.names);
            var port = book.roles.Facts("dorestad", "franks", "frisian");
            var tollman = npcs.Holder(new RoleSpec { key = "tollman", role = "tollman" }, port, 793, day);
            var vars = new EncounterVars();
            vars.nums["crew.trade_best"] = 3.0;
            var ctx = new EffectContext
            {
                game = game, state = new EncounterState(), npcs = npcs, words = book.words, vars = vars, port = "dorestad", day = day,
                card = "test.card", option = "test", roll = () => roll,
            };
            ctx.actors["tollman"] = tollman;
            return (ctx, game);
        }

        [Test]
        public void EachVerbDoesWhatItSays()
        {
            var (ctx, game) = Context();
            Effects.Apply(Parse("silver +12", "goods wine -2", "health -0.1", "morale +0.05", "men 1", "deed bribed_officer",
                                "news tollman.greet at here", "price_shock dorestad wine x1.5 for 10", "commission wine 4 by 30 premium 0.2",
                                "give_text alcuin_letter", "give_riddle exeter_7", "open_fetch silver_1", "hire tollman", "pay_off tollman",
                                "passenger tollman to quentovic", "encounter close_and_hail", "law hearing in 7", "learn latin +0.1",
                                "tongue old_norse = 0.5"), ctx);
            string tollId = ctx.actors["tollman"].id;
            CollectionAssert.AreEqual(new[]
            {
                "silver +12", "goods wine -2", "health -0.1", "morale +0.05", "men +1", "deed bribed_officer at dorestad",
                "news tollman.greet at dorestad", "price_shock dorestad wine x1.5 for 10", "commission wine 4 by 30 premium 0.2 at dorestad",
                "give_text alcuin_letter from " + ctx.actors["tollman"].name, "give_riddle exeter_7 from " + ctx.actors["tollman"].name,
                "open_fetch silver_1", "hire " + tollId, "pay_off " + tollId, "passenger " + tollId + " to quentovic",
                "encounter close_and_hail", "law hearing in 7 at dorestad", "learn latin +0.1", "tongue old_norse = 0.5",
            }, game.calls);
            Assert.AreEqual(112.0, game.silver, 1e-9);
            Assert.AreEqual(game.calls.Count, ctx.facts.Count, "every call is a fact in the outcome");
        }

        [Test]
        public void TheLayersOwnVerbsChangeItsState()
        {
            var (ctx, game) = Context(day: 10.0);
            string toll = ctx.actors["tollman"].id;
            Effects.Apply(Parse("npc tollman feeling +8 gift", "flag hid_goods_at_port", "flag debt_named = 3 for 5",
                                "schedule harbour.tollman.frankish in 2 at dorestad"), ctx);
            Assert.IsEmpty(game.calls, "none of these touch the live game");
            Assert.AreEqual(8.0, ctx.npcs.Feeling(toll, "player"), 1e-9);
            Assert.AreEqual("test", ctx.npcs.LastOption(toll, "player"));
            Assert.AreEqual(1.0, ctx.state.Flag("hid_goods_at_port", 400.0), "no 'for': it never lapses");
            Assert.AreEqual(3.0, ctx.state.Flag("debt_named", 14.9), 1e-9);
            Assert.AreEqual(0.0, ctx.state.Flag("debt_named", 15.0), "lapsed after five days");
            Assert.AreEqual(1, ctx.state.scheduled.Count);
            Assert.AreEqual(12.0, ctx.state.scheduled[0].day, 1e-9);
            Assert.AreEqual("dorestad", ctx.state.scheduled[0].port);
        }

        [Test]
        public void ChanceTakesTheBranchTheDiceSay()
        {
            var chance = new Dictionary<string, object>
            {
                ["chance"] = 0.75, ["by"] = "crew.trade_best",
                ["success"] = new List<object> { "flag hid_goods_at_port for 60" },
                ["failure"] = new List<object> { "law seizure", "deed goods_seized" },
            };
            var (won, wonGame) = Context(roll: 0.0);
            Effects.Apply(Parse(chance), won);
            Assert.AreEqual("success", won.branch);
            Assert.AreEqual(1.0, won.state.Flag("hid_goods_at_port", 2.0));
            Assert.IsEmpty(wonGame.calls);
            var (lost, lostGame) = Context(roll: 0.999);
            Effects.Apply(Parse(chance), lost);
            Assert.AreEqual("failure", lost.branch);
            Assert.AreEqual("test.failure", lost.Remembered);
            Assert.AreEqual(0.0, lost.state.Flag("hid_goods_at_port", 2.0));
            CollectionAssert.AreEqual(new[] { "law seizure in 0 at dorestad", "deed goods_seized at dorestad" }, lostGame.calls);
        }

        [Test]
        public void OddsMoveWithTheStatAndShowAsAWord()
        {
            var words = EncounterData.Book().words;
            var c = new ChanceEffect { chance = 0.75, by = "crew.trade_best" };
            var vars = new EncounterVars();
            vars.nums["crew.trade_best"] = 3.0;
            Assert.AreEqual(0.75, c.Odds(vars), 1e-9);
            Assert.AreEqual("likely", words.WordFor(WordTable.Odds, c.Odds(vars)));
            vars.nums["crew.trade_best"] = 5.0;
            Assert.AreEqual(0.85, c.Odds(vars), 1e-9);
            Assert.AreEqual("sure", words.WordFor(WordTable.Odds, c.Odds(vars)));
            vars.nums["crew.trade_best"] = 40.0;
            Assert.AreEqual(0.95, c.Odds(vars), 1e-9, "never surer than 0.95");
            vars.nums["crew.trade_best"] = -40.0;
            Assert.AreEqual(0.05, c.Odds(vars), 1e-9, "never less than 0.05");
            Assert.AreEqual("doubtful", words.WordFor(WordTable.Odds, c.Odds(vars)));
        }

        [Test]
        public void FlagForThirtyLapsesOnDayThirtyOne()
        {
            var (ctx, _) = Context(day: 1.0);
            Effects.Apply(Parse("flag hid_goods_at_port for 30"), ctx);
            Assert.AreEqual(1.0, ctx.state.Flag("hid_goods_at_port", 30.99));
            Assert.AreEqual(0.0, ctx.state.Flag("hid_goods_at_port", 31.0));
            ctx.state.ExpireFlags(31.0);
            Assert.AreEqual(0, ctx.state.FlagCount, "dropped once lapsed");
        }

        [Test]
        public void TollEasedHalvesForThirtyDaysAndNoneEndsWithTheCall()
        {
            var (ctx, _) = Context(day: 5.0);
            Assert.AreEqual(1.0, ctx.state.TollFactor("dorestad", 5.0));
            Effects.Apply(Parse("toll eased 30"), ctx);
            Assert.AreEqual(0.5, ctx.state.TollFactor("dorestad", 34.9));
            Assert.AreEqual(1.0, ctx.state.TollFactor("dorestad", 35.0));
            Assert.AreEqual(1.0, ctx.state.TollFactor("quentovic", 6.0), "only at that port");
            ctx.state.BeginCall("dorestad", 5.0);
            Effects.Apply(Parse("toll none"), ctx);
            Assert.AreEqual(0.0, ctx.state.TollFactor("dorestad", 6.0));
            ctx.state.EndCall();
            Assert.AreEqual(0.5, ctx.state.TollFactor("dorestad", 6.0), "the waiver ends with the call; the ease goes on");
            Effects.Apply(Parse("toll sales"), ctx);
            Assert.AreEqual(0.5, ctx.state.TollFactor("dorestad", 6.0), "declaring keeps an ease already earned");
        }

        [Test]
        public void CostIsPaid()
        {
            var errors = new List<string>();
            var cost = Effects.ParseCost("silver 12", errors, "test");
            Assert.IsEmpty(errors);
            var (ctx, game) = Context();
            Effects.Apply(cost, ctx);
            Assert.AreEqual(88.0, game.silver, 1e-9);
            Effects.ParseCost("silver -3", errors, "test");
            Effects.ParseCost("toll none", errors, "test");
            Assert.AreEqual(2, errors.Count, "a cost is a payment: 'silver n', 'men n' or 'goods <good> n'");
        }

        [Test]
        public void GrammarRefusesWhatIsNotInIt()
        {
            string[] bad =
            {
                "silver", "silver lots", "toll half", "toll eased", "flag", "flag x = ", "flag x for", "npc tollman likes +3",
                "schedule card 3", "news key here", "price_shock dorestad wine 1.5 for 10", "commission wine four by 30 premium 0.2",
                "encounter board", "law seizure in", "learn greek +0.1", "tongue old_norse 0.5", "teleport home", "",
            };
            foreach (var s in bad)
            {
                var errors = new List<string>();
                Assert.IsNull(Effects.Parse(s, errors, "test"), $"'{s}' should not parse");
                Assert.AreEqual(1, errors.Count, $"'{s}' gives one error");
            }
            var chanceErrors = new List<string>();
            Effects.Parse(new Dictionary<string, object> { ["chance"] = 0.5, ["success"] = new List<object>() }, chanceErrors, "test");
            Assert.IsNotEmpty(chanceErrors, "a chance needs both branches");
            chanceErrors.Clear();
            Effects.Parse(new Dictionary<string, object> { ["chance"] = 1.5, ["success"] = new List<object>(), ["failure"] = new List<object>() }, chanceErrors, "test");
            Assert.IsNotEmpty(chanceErrors, "a chance is 0 to 1");
        }
    }
}
