using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The three doors from the content to the game (Design/wiring_plan.md): the flags (Content/ContentFlags.cs, over the
    /// encounter state, which can now list its flags and says when they changed), the rules of a data row
    /// (Content/ContentRules.cs: a condition, a cost and effects parsed by the cards' own engine and run on the game), and
    /// the saved game (Content/ContentSave.cs). The engine here has the game's real tables of roles, names and words and
    /// no cards at all, so a card file a writer is in the middle of cannot fail these tests.
    /// </summary>
    public class ContentSeamTests
    {
        /// <summary>A director lying at Dorestad on day 10 with the fake game (100 d of silver, four men, one unit of lead).</summary>
        static (EncounterDirector d, FakeGame game) AtDorestad()
        {
            var (d, game) = ContentTestKit.AtPort("dorestad");
            d.vars.nums["hold.lead"] = 1.0;
            return (d, game);
        }

        /// <summary>No game is running (a batch test run; in the editor a game played without a domain reload may have left
        /// its director behind, and then the "no game" checks are skipped).</summary>
        static bool NoGame => EncounterRunner.Director == null;

        [TearDown]
        public void LeaveNothingBehind() => ContentTestKit.Done();

        // ------------------------------------------------------------------ the flags

        [Test]
        public void TheStateListsItsFlagsAndSaysWhenTheyChange()
        {
            var s = new EncounterState();
            int v0 = s.FlagVersion;
            s.SetFlag("unlock_ship_knarr", 1.0, 0.0, 0.0);
            s.SetFlag("wintering", 1.0, 0.0, 10.0);                          // lapses on day 10
            s.SetFlag("ch_black_stones_2", 1.0, 0.0, 0.0);
            s.SetFlag("cleared", 0.0, 0.0, 0.0);
            Assert.AreEqual(v0 + 4, s.FlagVersion, "each set moves the version");

            CollectionAssert.AreEqual(new[] { "ch_black_stones_2", "unlock_ship_knarr", "wintering" }, s.Flags(5.0).Select(kv => kv.Key).ToArray(),
                                      "the flags that hold, in the names' order; one set to 0 is not among them");
            CollectionAssert.AreEqual(new[] { "ch_black_stones_2", "unlock_ship_knarr" }, s.Flags(10.0).Select(kv => kv.Key).ToArray(),
                                      "a lapsed flag is not listed");
            Assert.AreEqual(1.0, s.Flags(5.0)[0].Value);

            int v1 = s.FlagVersion;
            s.ExpireFlags(5.0);
            Assert.AreEqual(v1, s.FlagVersion, "nothing lapsed yet: no change");
            s.ExpireFlags(11.0);
            Assert.Greater(s.FlagVersion, v1, "a flag dropped moves the version");

            int v2 = s.FlagVersion;
            var saved = s.Capture();
            var back = new EncounterState();
            int v3 = back.FlagVersion;
            back.Restore(saved);
            Assert.Greater(back.FlagVersion, v3, "a load moves the version");
            Assert.AreEqual(v2, s.FlagVersion);
            CollectionAssert.AreEqual(s.Flags(11.0), back.Flags(11.0));
        }

        [Test]
        public void TheFlagsAreReadAndSetThroughOneDoor()
        {
            ContentRules.Use(null);
            ContentFlags.Use(null);
            if (NoGame)
            {
                Assert.IsFalse(ContentFlags.Ready, "no game is running in an edit-mode test");
                Assert.IsFalse(ContentFlags.Has("unlock_ship_knarr"));
                Assert.IsFalse(ContentFlags.Set("unlock_ship_knarr"), "nothing to set a flag on");
                Assert.AreEqual(0, ContentFlags.All().Count);
            }

            var state = new EncounterState();
            double day = 3.0;
            ContentFlags.Use(state, () => day);
            Assert.IsTrue(ContentFlags.Ready);
            int version = ContentFlags.Version;

            Assert.IsTrue(ContentFlags.Set("unlock_ship_knarr"));
            Assert.IsTrue(ContentFlags.Has("unlock_ship_knarr"));
            Assert.AreEqual(1.0, ContentFlags.Value("unlock_ship_knarr"));
            Assert.AreEqual(1.0, state.Flag("unlock_ship_knarr", day), "it is the encounter layer's own flag: a card reads it as flag.unlock_ship_knarr");
            Assert.AreNotEqual(version, ContentFlags.Version, "a reader that kept a list knows to make it again");

            Assert.IsTrue(ContentFlags.Set("wintering", 1.0, 100.0));
            Assert.IsTrue(ContentFlags.Has("wintering"));
            day = 104.0;
            Assert.IsFalse(ContentFlags.Has("wintering"), "a flag set for days lapses by the day the door is on");
            Assert.IsTrue(ContentFlags.Has("unlock_ship_knarr"));

            Assert.IsTrue(ContentFlags.Add("st_dorestad_helped", 2.0));
            Assert.IsTrue(ContentFlags.Add("st_dorestad_helped", 1.0));
            Assert.AreEqual(3.0, ContentFlags.Value("st_dorestad_helped"));
            Assert.IsTrue(ContentFlags.Set("unlock_plot_dorestad"));
            CollectionAssert.AreEqual(new[] { "unlock_plot_dorestad", "unlock_ship_knarr" },
                                      ContentFlags.WithPrefix("unlock_").Select(kv => kv.Key).ToArray());
            Assert.AreEqual(3, ContentFlags.All().Count);
            Assert.IsTrue(ContentFlags.Clear("unlock_plot_dorestad"));
            Assert.IsFalse(ContentFlags.Has("unlock_plot_dorestad"));
            Assert.AreEqual(1, ContentFlags.WithPrefix("unlock_").Count);

            Assert.IsFalse(ContentFlags.Set("dorestad.charter"), "a dot: no condition could read it back");
            Assert.IsFalse(ContentFlags.Set(""));
            Assert.IsFalse(ContentFlags.ValidName("two words"));
            Assert.IsTrue(ContentFlags.ValidName("ch_black_stones_3h"));

            int withState = ContentFlags.Version;
            ContentFlags.Use(null);
            Assert.AreNotEqual(withState, ContentFlags.Version, "another game's flags: the lists are made again");
        }

        // ------------------------------------------------------------------ the rules of a data row

        [Test]
        public void AConditionParsesOnceAndAFaultyOneNeverHolds()
        {
            ContentRules.Use(null);
            ContentFiles.SetRoot(null);
            Assert.AreEqual(!NoGame, ContentRules.Ready);
            Assert.AreSame(Cond.True, ContentRules.ParseWhen("", "test"));
            Assert.AreSame(Cond.True, ContentRules.ParseWhen(null, "test"));
            Assert.IsTrue(ContentRules.Holds(null), "no condition at all holds");
            Assert.IsTrue(ContentRules.Holds(ContentRules.ParseWhen("true", "test")));
            Assert.IsFalse(ContentRules.Holds(ContentRules.Never));

            // the words of standing and feeling come from the game's tables even with no game running
            var known = ContentRules.ParseWhen("moored and standing >= known and npc.reeve.feeling >= friendly", "test");
            Assert.AreNotSame(ContentRules.Never, known, "a condition with the tables' words parses before the game starts");
            var nothingYet = ContentRules.ParseWhen("not flag.unlock_plot_dorestad and silver < 1", "test");
            if (NoGame)
            {
                Assert.IsFalse(ContentRules.Holds(known), "with no game every number is 0: not moored");
                Assert.IsTrue(ContentRules.Holds(nothingYet));
            }
            Assert.AreEqual(0, ContentFiles.Warnings.Count, "nothing to warn of so far");

            // a line of prose under "when" (the winter reckoning's years), and a name the engine does not know
            Assert.IsFalse(ContentRules.TryParseWhen("The year's gain after wages, bread, tolls and gifts is 240 d or more.", out Cond prose));
            Assert.IsFalse(ContentRules.Holds(prose));
            Assert.AreEqual(0, ContentFiles.Warnings.Count, "asking whether it is a condition warns of nothing");
            var faulty = ContentRules.ParseWhen("moored and sunday_best > 2", "story/town_actions_793.json act test");
            Assert.AreSame(ContentRules.Never, faulty);
            Assert.IsFalse(ContentRules.Holds(faulty), "a faulty row is shut, not open to all");
            ContentRules.ParseWhen("moored and sunday_best > 2", "story/town_actions_793.json act test");
            Assert.AreEqual(1, ContentFiles.Warnings.Count, "one warning, naming the row");
            StringAssert.Contains("story/town_actions_793.json act test", ContentFiles.Warnings[0]);

            Assert.AreEqual("", ContentRules.Why(null));
            Assert.AreEqual("You need 24 d of silver.", ContentRules.Why(ContentRules.ParseWhen("silver >= 24", "test")));
        }

        [Test]
        public void TheWordsAreFoundFromATestsOwnFolder()
        {
            string dir = ContentTestKit.UseFolder();
            try
            {
                ContentRules.Use(null);
                ContentTestKit.Put(dir, "story/ships_793.json", "{ \"ships\": [] }");
                Assert.IsTrue(ContentFiles.Exists("story/ships_793.json"));
                Assert.IsFalse(ContentFiles.Exists("reputation_793.json"), "a loader's test folder has only its own fixtures");
                Assert.IsNotNull(ContentFiles.ReadDefault("reputation_793.json"), "the shared tables are still within reach");
                Assert.IsFalse(ContentFiles.RootIsDefault, "and the root stays where the test put it");
                Assert.AreNotSame(ContentRules.Never, ContentRules.ParseWhen("standing >= known and npc.reeve.feeling >= friendly", "a fixture"),
                                  "so a fixture's conditions may use the words of standing and feeling");
                Assert.AreEqual(0, ContentFiles.Warnings.Count);

                // and the engine can be had while the content points at the fixtures (its tables come from the project)
                var (d, game) = ContentTestKit.AtPort("dorestad");
                Assert.IsFalse(ContentFiles.RootIsDefault);
                Assert.AreSame(d, ContentRules.Director);
                Assert.AreEqual(100.0, game.silver);
                Assert.IsTrue(ContentRules.Holds(ContentRules.ParseWhen("moored and port.id = dorestad and hour >= 6 and hour < 18 and crew = 4", "a fixture")));
            }
            finally
            {
                ContentTestKit.Done(dir);
            }
            Assert.IsTrue(ContentFiles.RootIsDefault, "Done puts the content back on the project's folder");
            if (NoGame)
                Assert.IsNull(ContentRules.Director, "and leaves no test's director behind");
        }

        [Test]
        public void CostsAndEffectsParseAndACostInProseIsRefused()
        {
            ContentRules.Use(null);
            ContentFiles.SetRoot(null);

            Assert.IsTrue(ContentRules.ParseCost(null, "test", out var cost));
            Assert.AreEqual(0, cost.Count);
            Assert.IsTrue(ContentRules.ParseCost("none", "test", out cost));
            Assert.AreEqual(0, cost.Count);
            Assert.IsTrue(ContentRules.ParseCost("", "test", out cost));
            Assert.AreEqual(0, cost.Count);

            Assert.IsTrue(ContentRules.ParseCost("silver 12", "test", out cost));
            Assert.AreEqual(1, cost.Count);
            var silver = (VerbEffect)cost[0];
            Assert.AreEqual("silver", silver.verb);
            Assert.AreEqual(-12.0, silver.n, "a cost is paid");

            Assert.IsTrue(ContentRules.ParseCost(new List<object> { "goods lead 2", "silver 6", "none" }, "test", out cost));
            Assert.AreEqual(2, cost.Count);
            Assert.AreEqual(0, ContentFiles.Warnings.Count);

            Assert.IsFalse(ContentRules.ParseCost("silver 400 to 1200, by the port", "story/unlocks_793.json unlock_x", out cost),
                           "a cost in plain words is not understood: the caller keeps the thing shut");
            Assert.AreEqual(0, cost.Count);
            Assert.AreEqual(1, ContentFiles.Warnings.Count);

            Assert.IsTrue(ContentRules.ParseEffects(new List<object> { "silver +5", "flag unlock_plot_dorestad", "npc reeve feeling +10 warned" }, "test", out var effects));
            Assert.AreEqual(3, effects.Count);
            Assert.IsTrue(ContentRules.ParseEffects(null, "test", out effects));
            Assert.AreEqual(0, effects.Count);
            Assert.IsTrue(ContentRules.ParseEffects("deed gave_alms", "test", out effects), "one effect written alone");
            Assert.AreEqual(1, effects.Count);
            Assert.IsFalse(ContentRules.ParseEffects(new List<object> { "silver +5", "people dorestad +300" }, "story/settlements/x.json help", out effects),
                           "a world effect is not one of the engine's verbs");
            Assert.AreEqual(1, effects.Count, "what parsed is kept");
            Assert.AreEqual(2, ContentFiles.Warnings.Count);
        }

        [Test]
        public void ARowRunsOnTheCardsEngine()
        {
            var (d, game) = AtDorestad();
            ContentRules.Use(d);
            Assert.IsTrue(ContentRules.Ready);
            Assert.IsTrue(ContentFlags.Ready, "the flags are the director's");
            Assert.AreEqual(10.0, ContentFlags.Day);

            var when = ContentRules.ParseWhen("moored and port.id = dorestad and year >= 793 and not flag.act_clean_hull", "act repair");
            Assert.IsTrue(ContentRules.ParseCost("silver 8", "act repair", out var cost));
            Assert.IsTrue(ContentRules.ParseEffects(new List<object> { "morale +0.05", "flag act_clean_hull for 300", "npc shipwright feeling +2 paid_fairly" },
                                                    "act repair", out var effects));
            Assert.IsTrue(ContentRules.Holds(when));
            Assert.IsNull(ContentRules.Lacks(cost));

            var done = ContentRules.Run("act.repair", when, cost, effects);
            Assert.IsTrue(done.ok, done.why);
            Assert.AreEqual(92.0, game.silver, "the cost was paid through the game's own seam");
            CollectionAssert.Contains(game.calls, "silver -8");
            CollectionAssert.Contains(game.calls, "morale +0.05");
            Assert.IsTrue(done.facts.Count >= 2, "the facts of what happened: " + done.Text);
            Assert.IsTrue(ContentFlags.Has("act_clean_hull"), "the effect's flag is read back through the flags' door");
            var wright = d.npcs.All.FirstOrDefault(n => n.role == "shipwright" && n.homePort == "dorestad");
            Assert.IsNotNull(wright, "the role the effects name is a person of this port, as on a card");
            Assert.AreEqual(2.0, d.npcs.Feeling(wright.id, "player"));
            Assert.IsTrue(d.npcs.Met(wright.id, "player"));
            Assert.AreEqual(0, d.vars.actors.Count, "nobody is left standing in the variables afterwards");

            // done once: the condition now refuses it, and says why
            var again = ContentRules.Run("act.repair", when, cost, effects);
            Assert.IsFalse(again.ok);
            Assert.IsTrue(again.why.Length > 0);
            Assert.AreEqual(92.0, game.silver, "nothing was paid");

            // what the merchant lacks is said plainly, and nothing is done
            Assert.IsTrue(ContentRules.ParseCost("silver 500", "act build_ship", out var dear));
            var poor = ContentRules.Run("act.build_ship", Cond.True, dear, effects);
            Assert.IsFalse(poor.ok);
            Assert.AreEqual("You need 500 d of silver.", poor.why);
            Assert.AreEqual(92.0, game.silver);
            Assert.IsTrue(ContentRules.ParseCost(new List<object> { "goods lead 2" }, "act fit_upgrade", out var lead));
            StringAssert.StartsWith("You need 2 of ", ContentRules.Lacks(lead));
            Assert.IsTrue(ContentRules.ParseCost("men 4", "test", out var men));
            Assert.IsNotNull(ContentRules.Lacks(men), "the last man is not spent");

            // elsewhere: the rule in plain words
            var away = ContentRules.Run("act.repair", ContentRules.ParseWhen("port.id = lundenwic", "test"), cost, effects);
            Assert.IsFalse(away.ok);
            Assert.AreEqual("Only at lundenwic.", away.why);

            // the same man the second time: a fixed role's holder, as a card would bring
            d.state.SetFlag("act_clean_hull", 0.0, d.Day, 0.0);
            Assert.IsTrue(ContentRules.Run("act.repair", when, cost, effects).ok);
            Assert.AreEqual(4.0, d.npcs.Feeling(wright.id, "player"));
            Assert.AreEqual(1, d.npcs.All.Count(n => n.role == "shipwright" && n.homePort == "dorestad"));

            // with no game nothing runs
            ContentRules.Use(null);
            if (NoGame)
            {
                var none = ContentRules.Run("act.repair", Cond.True, cost, effects);
                Assert.IsFalse(none.ok);
                Assert.AreEqual("The game is not ready.", none.why);
            }
        }

        [Test]
        public void ARowAtSeaRunsWithNobodyThere()
        {
            var (d, _) = ContentTestKit.AtSea(20.0, 800, 3);
            Assert.IsTrue(ContentRules.ParseEffects(new List<object> { "flag changed_dorestad_863", "npc reeve feeling +10 warned" }, "hinge", out var effects));
            var done = ContentRules.Run("hinge.dorestad_863", ContentRules.ParseWhen("year >= 800 and not moored", "hinge"), null, effects);
            Assert.IsTrue(done.ok, done.why);
            Assert.IsTrue(ContentFlags.Has("changed_dorestad_863"));
            Assert.AreEqual(0, d.npcs.Count, "no person is made for a role at sea");
        }

        [Test]
        public void AnEffectCanAddToAFlag()
        {
            var (d, _) = AtDorestad();
            ContentRules.Use(d);
            Assert.IsTrue(ContentRules.ParseEffects(new List<object> { "flag st_dorestad_helped = 2", "flag st_dorestad_helped +3", "flag st_dorestad_helped -1" },
                                                    "test", out var count));
            Assert.IsTrue(ContentRules.Run("test.count", null, null, count).ok);
            Assert.AreEqual(4.0, ContentFlags.Value("st_dorestad_helped"), "a flag at 2, +3, -1 reads 4");

            Assert.IsTrue(ContentRules.ParseEffects(new List<object> { "flag promises = 1 for 30", "flag promises +1" }, "test", out var lapsing));
            Assert.IsTrue(ContentRules.Run("test.lapsing", null, null, lapsing).ok);
            Assert.AreEqual(2.0, d.state.Flag("promises", d.Day + 29.0));
            Assert.AreEqual(0.0, d.state.Flag("promises", d.Day + 30.0), "a lapsing flag keeps its day");

            Assert.IsTrue(ContentFlags.Add("house_crossings", 1.0), "a count never set starts from nothing");
            Assert.AreEqual(1.0, ContentFlags.Value("house_crossings"));
            Assert.IsTrue(ContentFlags.Add("promises", -1.0));
            Assert.AreEqual(1.0, d.state.Flag("promises", d.Day + 29.0));
            Assert.AreEqual(0.0, d.state.Flag("promises", d.Day + 30.0));

            Assert.IsFalse(ContentRules.ParseEffects(new List<object> { "flag x + 3" }, "test", out _), "the sign is written on the number");
            Assert.IsFalse(ContentRules.ParseEffects(new List<object> { "flag x +many" }, "test", out _));
        }

        [Test]
        public void ACardCanBeAskedForAtASite()
        {
            var src = ContentTestKit.Tables();
            src.cards.Add(("test_cards.json",
                "{ \"cards\": [ { \"id\": \"harbour.test.ale_house\", \"stage\": \"harbour\", \"when\": \"moored and port.id = dorestad\", \"cooldown_days\": 30," +
                " \"title_key\": \"test.ale.title\", \"title\": \"The ale-house\"," +
                " \"options\": [ { \"key\": \"drink\", \"label_key\": \"test.ale.drink\", \"label\": \"Buy a round\", \"cost\": \"silver 2\"," +
                " \"effects\": [ \"morale +0.02\" ] } ] } ] }"));
            var book = EncounterLoader.Load(src);
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            var d = new EncounterDirector(book, 793, new FakeGame());
            d.portInfo = port => ("franks", "frisian");
            d.SetTime(10.0, 793, 6, 240);
            const string Card = "harbour.test.ale_house";

            Assert.IsFalse(d.CanOffer(Card), "at sea its condition does not hold");
            Assert.IsFalse(d.Offer(Card));
            d.BeginCall("dorestad");
            Assert.AreEqual(0, d.state.queue.Count, "a card with no weight is never drawn by itself");
            Assert.IsFalse(d.CanOffer("harbour.no.such_card"));
            Assert.IsTrue(d.CanOffer(Card));
            Assert.IsTrue(d.Offer(Card));
            Assert.IsTrue(d.Offer(Card), "asked for twice: it is in line once");
            Assert.AreEqual(1, d.state.queue.Count);

            var session = d.OpenNext();
            Assert.IsNotNull(session);
            Assert.AreEqual(Card, session.card.id);
            Assert.IsTrue(d.Choose(0).ok);
            Assert.IsTrue(d.Continue());
            Assert.IsFalse(d.CanOffer(Card), "shown today: its thirty days have begun");
            d.SetTime(41.0, 793, 7, 984);
            Assert.IsTrue(d.CanOffer(Card), "and are over");
        }

        // ------------------------------------------------------------------ the saved game

        [Test]
        public void TheContentIsSavedByItsParts()
        {
            object gotLedger = "untouched", gotFleet = "untouched";
            try
            {
                ContentSave.Register("test_ledger",
                    () => new Dictionary<string, object> { ["dorestad"] = new Dictionary<string, object> { ["wound"] = 120.0, ["mark"] = 0.05 } },
                    piece => gotLedger = piece);
                ContentSave.Register("test_fleet", () => new List<object> { "gislinge", "river_barge" }, piece => gotFleet = piece);
                ContentSave.Register("test_broken", () => throw new InvalidOperationException("a part with a fault"),
                                     piece => throw new InvalidOperationException("a part with a fault"));
                Assert.IsTrue(ContentSave.Registered("test_ledger"));
                CollectionAssert.IsSubsetOf(new[] { "test_broken", "test_fleet", "test_ledger" }, ContentSave.Keys.ToList());

                var saved = ContentSave.Capture();
                Assert.IsTrue(saved.ContainsKey("test_ledger"));
                Assert.IsTrue(saved.ContainsKey("test_fleet"));
                Assert.IsFalse(saved.ContainsKey("test_broken"), "a part that throws is left out; the save goes on");

                // through the save file's own text and back
                var loaded = Json.Parse(Json.Write(saved)) as Dictionary<string, object>;
                ContentSave.Restore(loaded);
                Assert.AreEqual(120.0, Json.Num(Json.Obj(gotLedger, "dorestad"), "wound"));
                Assert.AreEqual(0.05, Json.Num(Json.Obj(gotLedger, "dorestad"), "mark"));
                CollectionAssert.AreEqual(new object[] { "gislinge", "river_barge" }, (List<object>)gotFleet);

                // a save from before the part existed: each part is told so, and starts afresh
                ContentSave.Restore(null);
                Assert.IsNull(gotLedger);
                Assert.IsNull(gotFleet);
                ContentSave.Restore(new Dictionary<string, object> { ["test_fleet"] = new List<object>() });
                Assert.IsNull(gotLedger);
                Assert.IsNotNull(gotFleet);
            }
            finally
            {
                ContentSave.Unregister("test_ledger");
                ContentSave.Unregister("test_fleet");
                ContentSave.Unregister("test_broken");
            }
            Assert.IsFalse(ContentSave.Registered("test_ledger"));
        }
    }
}
