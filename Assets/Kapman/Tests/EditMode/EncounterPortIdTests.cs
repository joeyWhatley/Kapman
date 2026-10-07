using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// A card knows a port by name: "port.id" is the id of the port she lies at, as in ports_793.json ("" at sea), so a
    /// card can say "moored and port.id = dorestad". A field the loader does not know ("art") is skipped with a warning
    /// and the card still loads. Flag names with digits and underscores (ch_black_stones_3, unlock_ship_gislinge) work in
    /// "flag" effects and in "flag.&lt;name&gt;" conditions. The tables here are written in the test, so a half-finished
    /// data file elsewhere cannot fail them.
    /// </summary>
    public class EncounterPortIdTests
    {
        const string Roles = "{\"roles\": {\"tollman\": {\"title\": \"toll man\", \"fixed\": true}}, " +
                             "\"feeling_words\": [[-100, \"hostile\"], [0, \"easy\"], [20, \"friendly\"]]}";
        const string Names = "{\"names\": [{\"name\": \"Radbod\", \"people\": \"frisian\", \"sex\": \"m\"}]}";
        const string Reputation = "{\"words\": [[0, \"ill thought of\"], [100, \"known\"]], \"deeds\": {}}";
        const string Knowing = "{\"words\": {\"sure\": 0.8, \"likely\": 0.55, \"doubtful\": 0.3}}";
        const string Writing = "{\"entries\": []}";
        const string Ports = "{\"ports\": [{\"id\": \"dorestad\"}, {\"id\": \"lundenwic\"}]}";

        static EncounterBook Load(string cards, string ports = Ports)
        {
            var src = new EncounterSources { roles = Roles, names = Names, reputation = Reputation, knowing = Knowing, writing = Writing, ports = ports };
            src.cards.Add(("encounters/test.json", cards));
            return EncounterLoader.Load(src);
        }

        static EncounterVars Vars(string port, bool moored)
        {
            var v = new EncounterVars { state = new EncounterState(), port = port, day = 1.0 };
            v.nums["moored"] = moored ? 1.0 : 0.0;
            return v;
        }

        // ------------------------------------------------------------------ port.id

        [Test]
        public void PortIdIsATextVariable()
        {
            Assert.IsTrue(VarNames.TryType("port.id", out var type, out var words));
            Assert.AreEqual(VarType.Text, type);
            Assert.IsNull(words);
        }

        [Test]
        public void ACardCanNameThePortSheLiesAt()
        {
            var c = Cond.Parse("moored and port.id = dorestad", new WordTable());
            Assert.IsTrue(c.Eval(Vars("dorestad", true)), "moored at Dorestad");
            Assert.IsFalse(c.Eval(Vars("lundenwic", true)), "moored somewhere else");
            Assert.IsFalse(c.Eval(Vars("", false)), "at sea: no port");
            Assert.IsFalse(c.Eval(Vars("dorestad", false)), "not moored");
            var names = new List<string>();
            c.Names(names);
            CollectionAssert.Contains(names, "port.id");
        }

        [Test]
        public void PortIdIsEmptyAtSeaAndComparesBothWays()
        {
            var words = new WordTable();
            Assert.AreEqual("", Vars("", false).Text("port.id"));
            Assert.AreEqual("", new EncounterVars { port = null }.Text("port.id"), "no port at all reads as empty, not null");
            Assert.AreEqual("dorestad", Vars("dorestad", true).Text("port.id"));
            Assert.IsFalse(Cond.Parse("port.id", words).Eval(Vars("", false)), "a bare port.id is false at sea");
            Assert.IsTrue(Cond.Parse("port.id", words).Eval(Vars("dorestad", true)), "and true in a port");
            Assert.IsTrue(Cond.Parse("port.id != dorestad", words).Eval(Vars("lundenwic", true)));
            Assert.IsFalse(Cond.Parse("port.id != dorestad", words).Eval(Vars("dorestad", true)));
            Assert.IsTrue(Cond.Parse("port.id == Dorestad", words).Eval(Vars("dorestad", true)), "== reads as =, and the case does not matter");
            Assert.IsTrue(Cond.Parse("port.id = dorestad or port.id = lundenwic", words).Eval(Vars("lundenwic", true)));
            Assert.IsTrue(Cond.Parse("not (port.id = dorestad)", words).Eval(Vars("", false)));
            Assert.Throws<System.FormatException>(() => Cond.Parse("port.id > dorestad", words), "a text compares with = or != only");
            Assert.Throws<System.FormatException>(() => Cond.Parse("port.id = 3", words), "and with a word, not a number");
            Assert.AreEqual("Only at dorestad.", Cond.Parse("port.id = dorestad", words).Describe(words));
        }

        [Test]
        public void TheGameSideAnswersPortIdToo()
        {
            var port = new PortDef { id = "dorestad", name = "Dorestat" };
            Assert.AreEqual("dorestad", EncounterGameVars.Text("port.id", port));
            Assert.AreEqual("", EncounterGameVars.Text("port.id", null), "at sea");
            // hooked to the live game, a card's variables still read the call's port
            var v = new EncounterVars { port = "dorestad" };
            EncounterGameVars.Attach(v, () => "lundenwic");
            Assert.AreEqual("dorestad", v.Text("port.id"));
        }

        [Test]
        public void ACardWithPortIdLoadsAndAnUnknownPortIsAWarning()
        {
            var book = Load("{\"cards\": [" +
                            "{\"id\": \"harbour.home\", \"stage\": \"harbour\", \"when\": \"moored and port.id = dorestad\", \"weight\": 1, " +
                            "\"title_key\": \"t.home\", \"options\": [{\"key\": \"ok\", \"label_key\": \"t.ok\"}]}," +
                            "{\"id\": \"harbour.typo\", \"stage\": \"harbour\", \"when\": \"port.id = dorstad\", \"weight\": 1, " +
                            "\"title_key\": \"t.typo\", \"options\": [{\"key\": \"ok\", \"label_key\": \"t.ok\", \"check\": \"port.id != lundenwic\"}]}]}");
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            Assert.AreEqual(2, book.cards.Count);
            Assert.IsTrue(book.Card("harbour.home").when.Eval(Vars("dorestad", true)));
            Assert.IsFalse(book.Card("harbour.home").when.Eval(Vars("", false)));
            Assert.AreEqual(1, book.warnings.Count, string.Join("\n", book.warnings));
            StringAssert.Contains("dorstad", book.warnings[0]);
            StringAssert.Contains("harbour.typo", book.warnings[0]);
        }

        // ------------------------------------------------------------------ a field the loader does not know

        [Test]
        public void ACardFileWithAnUnknownFieldStillLoads()
        {
            var book = Load("{\"about\": \"the file's own note\", \"art\": \"file.png\", \"cards\": [" +
                            "{\"id\": \"harbour.art\", \"stage\": \"harbour\", \"weight\": 1, \"art\": \"cards/tollman.png\", \"mood\": {\"light\": \"dusk\"}, " +
                            "\"title_key\": \"t.art\", " +
                            "\"actors\": {\"tollman\": {\"role\": \"tollman\", \"art\": \"faces/tollman.png\"}}, " +
                            "\"lines\": [{\"who\": \"tollman\", \"key\": \"t.line\", \"art\": \"x\"}], " +
                            "\"options\": [{\"key\": \"ok\", \"label_key\": \"t.ok\", \"art\": \"icons/ok.png\", \"effects\": [\"silver -1\"]}]}]}");
            Assert.IsTrue(book.Ok, "an unknown field is no fault:\n" + string.Join("\n", book.errors));
            var card = book.Card("harbour.art");
            Assert.IsNotNull(card, "the card is in the book");
            Assert.AreEqual(1, card.options.Count);
            Assert.AreEqual(1, card.options[0].effects.Count, "what the loader knows is read as before");
            Assert.AreEqual(1, card.lines.Count);
            Assert.AreEqual(1, card.actors.Count);
            // each is listed, so a misspelt field is not silent: two on the card, one each on the actor, the line and the option
            Assert.AreEqual(5, book.warnings.Count, string.Join("\n", book.warnings));
            Assert.IsTrue(book.warnings.Exists(w => w.Contains("\"art\"") && w.Contains("harbour.art")));
            Assert.IsTrue(book.warnings.Exists(w => w.Contains("\"mood\"")));
        }

        [Test]
        public void ARealFaultIsStillAFault()
        {
            var book = Load("{\"cards\": [{\"id\": \"harbour.bad\", \"stage\": \"harbour\", \"art\": \"x.png\", \"when\": \"port.nonsense = dorestad\", " +
                            "\"title_key\": \"t.bad\", \"options\": [{\"key\": \"ok\", \"label_key\": \"t.ok\", \"effects\": [\"fly away\"]}]}]}");
            Assert.IsFalse(book.Ok);
            Assert.IsTrue(book.errors.Exists(e => e.Contains("unknown variable 'port.nonsense'")), string.Join("\n", book.errors));
            Assert.IsTrue(book.errors.Exists(e => e.Contains("unknown verb 'fly'")), string.Join("\n", book.errors));
        }

        // ------------------------------------------------------------------ flag names with digits and underscores

        [Test]
        public void FlagNamesWithDigitsAndUnderscoresWorkInEffects()
        {
            var errors = new List<string>();
            var list = Effects.ParseList(new List<object>
            {
                "flag ch_black_stones_3", "flag unlock_ship_gislinge = 2", "flag ch_black_stones_4 = 1 for 30", "flag ch_2_done for 5",
            }, errors, "test");
            Assert.IsEmpty(errors, string.Join("\n", errors));
            Assert.AreEqual(4, list.Count);
            var state = new EncounterState();
            var ctx = new EffectContext { state = state, day = 1.0 };
            Effects.Apply(list, ctx);
            Assert.AreEqual(1.0, state.Flag("ch_black_stones_3", 1.0), 1e-9);
            Assert.AreEqual(2.0, state.Flag("unlock_ship_gislinge", 500.0), 1e-9, "no lapse given: it stays");
            Assert.AreEqual(1.0, state.Flag("ch_black_stones_4", 30.0), 1e-9);
            Assert.AreEqual(0.0, state.Flag("ch_black_stones_4", 31.0), 1e-9, "set on day 1 for 30: gone on day 31");
            Assert.AreEqual(1.0, state.Flag("ch_2_done", 5.9), 1e-9);
            Assert.AreEqual(0.0, state.Flag("ch_2_done", 6.0), 1e-9);
        }

        [Test]
        public void FlagNamesWithDigitsAndUnderscoresWorkInConditions()
        {
            var words = new WordTable();
            var state = new EncounterState();
            var vars = new EncounterVars { state = state, day = 2.0 };
            Assert.IsTrue(VarNames.TryType("flag.ch_black_stones_3", out var type, out _));
            Assert.AreEqual(VarType.Number, type);
            var bare = Cond.Parse("flag.ch_black_stones_3", words);
            var atLeast = Cond.Parse("flag.unlock_ship_gislinge >= 2", words);
            var both = Cond.Parse("flag.ch_black_stones_3 and not flag.ch_black_stones_4 and flag.unlock_ship_gislinge = 2", words);
            Assert.IsFalse(bare.Eval(vars), "not set yet");
            Assert.IsFalse(atLeast.Eval(vars));
            state.SetFlag("ch_black_stones_3", 1.0, 1.0, 0.0);
            state.SetFlag("unlock_ship_gislinge", 2.0, 1.0, 0.0);
            Assert.IsTrue(bare.Eval(vars));
            Assert.IsTrue(atLeast.Eval(vars));
            Assert.IsTrue(both.Eval(vars));
            state.SetFlag("ch_black_stones_4", 1.0, 1.0, 0.0);
            Assert.IsFalse(both.Eval(vars));
            var names = new List<string>();
            both.Names(names);
            CollectionAssert.AreEquivalent(new[] { "flag.ch_black_stones_3", "flag.ch_black_stones_4", "flag.unlock_ship_gislinge" }, names);
            Assert.AreEqual("Only after: ch black stones 3.", bare.Describe(words));
        }

        [Test]
        public void ACardSetsAndReadsSuchFlags()
        {
            var book = Load("{\"cards\": [{\"id\": \"harbour.stones\", \"stage\": \"harbour\", \"weight\": 1, " +
                            "\"when\": \"moored and port.id = dorestad and not flag.ch_black_stones_3\", \"title_key\": \"t.stones\", " +
                            "\"lines\": [{\"key\": \"t.line\", \"when\": \"flag.unlock_ship_gislinge\"}], " +
                            "\"options\": [" +
                            "{\"key\": \"take\", \"label_key\": \"t.take\", \"check\": \"flag.ch_black_stones_2 >= 1\", " +
                            "\"effects\": [\"flag ch_black_stones_3\", \"flag unlock_ship_gislinge = 1 for 60\"]}," +
                            "{\"key\": \"leave\", \"label_key\": \"t.leave\"}]}]}");
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            Assert.IsEmpty(book.warnings, string.Join("\n", book.warnings));
            var card = book.Card("harbour.stones");
            var state = new EncounterState();
            var vars = new EncounterVars { state = state, port = "dorestad", day = 3.0 };
            vars.nums["moored"] = 1.0;
            Assert.IsTrue(card.when.Eval(vars));
            Assert.IsFalse(card.Option("take").check.Eval(vars), "the chain's second step is not done");
            state.SetFlag("ch_black_stones_2", 1.0, 2.0, 0.0);
            Assert.IsTrue(card.Option("take").check.Eval(vars));
            Effects.Apply(card.Option("take").effects, new EffectContext { state = state, day = 3.0, port = "dorestad" });
            Assert.IsFalse(card.when.Eval(vars), "done once: the card does not come again");
            Assert.IsTrue(card.lines[0].when.Eval(vars));
            vars.day = 64.0;
            Assert.IsFalse(card.lines[0].when.Eval(vars), "the 60 days are over");
        }
    }
}
