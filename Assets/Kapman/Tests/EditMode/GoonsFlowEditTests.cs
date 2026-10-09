using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Graphs;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE MEETING WITH THE LENDER'S MEN IS A FLOW, RUN BY A GRAPH (the user's ruling of 7 Oct 2026; put right on 9 Oct,
    /// after "Btw is this all being built like I asked"). Held here, without the screen:
    ///   - the graph "Encounter - the lender's men" is there, the game's scene runs it, and each of the four moments
    ///     has its event in it;
    ///   - what each answer does by the graph is what it does by the code (the same game, played twice: once with the
    ///     code's own order of the parts, once with the graph's flow run straight from its asset), over every answer
    ///     and over every rung of what they do when they are given nothing;
    ///   - the lender's day to send, and their giving up, by the graph as by the code.
    /// That the played game goes by the graph and never by the code is held by GoonsTests and FeudTests (play).
    /// </summary>
    public class GoonsFlowEditTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        EncounterState flags;
        ScriptGraphAsset graph;
        WorldXZ quay;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            RuleGraphs.Install();                                         // (their rules by their graphs too, as in the played game)
            graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.GoonsGraphMade.Path);
            var home = GameData.Port(Collectors.Rules.home);
            var (hx, hz) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            quay = new WorldXZ(hx, hz);
            Fresh();
        }

        [TearDown]
        public void Done()
        {
            Collectors.Flow = null;
            Collectors.Reset();
            Feud.Reset();
            Self.Unmark(Feud.Mark);
            ContentFlags.Use(null);
            Markets.Clear();
            GameRules.Graph = null;                                       // (the other tests of this run go by the code, as before)
        }

        /// <summary>As a new game has it: nothing of the lender's men or of Ravn's death, the crew whole and in good heart.</summary>
        void Fresh()
        {
            Markets.Clear();
            Collectors.Reset();
            Feud.Reset();
            Self.Unmark(Feud.Mark);
            flags = new EncounterState();
            ContentFlags.Use(flags, () => 0.0);
            flags.SetFlag(Collectors.OpeningFlag, 1.0, 0.0, 0.0);
            Trade.Begin(TradeRules.Current, 900f, 0.0);
        }

        static void Set(string property, object value) =>
            typeof(Collectors).GetProperty(property).GetSetMethod(true).Invoke(null, new[] { value });

        string Shot(string said)
        {
            var rules = TradeRules.Current;
            string hold = string.Join(",", Trade.Hold.Contents.Where(l => l.count > 0).OrderBy(l => l.good.id, System.StringComparer.Ordinal).Select(l => l.good.id + "x" + l.count));
            string used = string.Join(",", new[] { "hide", "lie", "drink", "plead", "run" }.Select(t => t + Collectors.Used(t)));
            return $"{Collectors.State} visit {Collectors.Visit} temper {Collectors.Temper:0.###} unpaid {Collectors.Unpaid} fooled {Collectors.Fooled} used {used} does {Collectors.WhatTheyDo} " +
                   $"owed {Trade.Debt:0.##} silver {Trade.Purse.Silver(rules):0.##} hold [{hold}] men {Voyage.Men} health {Voyage.Health:0.###} heart {Voyage.Morale:0.###} " +
                   $"ravn dead {Collectors.RavnGone} feud {Feud.Stage}\n   said: {said}";
        }

        /// <summary>One answer in one game: by the code's own order of the parts, or by the graph's flow.</summary>
        string Play(bool byGraph, string kind, double temper, int unpaid, double silver, int casks, int men, out int does)
        {
            Fresh();
            Voyage.SetMen(men);
            Voyage.Affect(2.0, 2.0);
            // (the purse as wanted: what a new game gives is taken out first)
            Trade.Purse.PayUpTo(Trade.Purse.Silver(TradeRules.Current), TradeRules.Current);
            if (silver > 0.0)
                Trade.Purse.Add("denier", silver);
            if (casks > 0)
                Trade.Hold.Add(GameData.Good("rhenish_wine"), casks, 0.0, 0.0);
            Collectors.PutOut(2, quay, temper, Collectors.Rules.home);
            Collectors.Refused(unpaid);
            Collectors.Flow = byGraph ? (System.Func<string, bool>)(moment => LendersMenFlow.Run(graph, moment)) : null;
            int graphs = Collectors.ByGraph, codes = Collectors.ByCode;
            string said = Collectors.Law(kind);
            if (byGraph)
                Assert.AreEqual((graphs + 1, codes), (Collectors.ByGraph, Collectors.ByCode), kind + ": the answer was not run by the graph's flow");
            else
                Assert.AreEqual((graphs, codes + 1), (Collectors.ByGraph, Collectors.ByCode), kind + ": the answer was not run by the code");
            Collectors.Flow = null;
            does = Collectors.WhatTheyDo;
            return Shot(said);
        }

        [Test]
        public void TheGraphIsThereTheSceneRunsItAndEachMomentHasItsEvent()
        {
            Assert.IsNotNull(graph, "no graph of the lender's men (Kapman > Make physical > 3d. The graph of the lender's men)");
            foreach (string moment in new[] { "due", "caught", "answered", "gave_up" })
            {
                var of = LendersMenEvent.Of(moment);
                Assert.IsNotNull(of, moment);
                Assert.AreEqual(1, graph.graph.units.Count(u => u.GetType() == of), "the graph does not have just one event for the moment \"" + moment + "\"");
            }
            Assert.IsNull(LendersMenEvent.Of("no such moment"));
            // every answer the cards can order has its row: the switch on the answer names each
            var which = graph.graph.units.OfType<SwitchOnString>().FirstOrDefault();
            Assert.IsNotNull(which, "the graph does not switch on the answer");
            CollectionAssert.AreEquivalent(Kapman.EditorTools.GoonsGraphMade.Answers, which.options, "the graph's rows are not the answers the code knows");
            var book = EncounterData.Book();
            var ordered = new HashSet<string>();
            foreach (var card in book.cards)
                foreach (var option in card.options)
                    foreach (string law in LawsOf(option))
                        if (law.StartsWith("goons_", System.StringComparison.Ordinal))
                            ordered.Add(law.Substring("goons_".Length));
            Assert.Greater(ordered.Count, 6, "the cards order nothing of the lender's men");
            foreach (string kind in ordered)
            {
                string answer = kind == "teeth" || kind == "teeth_angry" ? "nothing" : kind.StartsWith("fooled_", System.StringComparison.Ordinal) ? "fooled" : kind;
                CollectionAssert.Contains(Kapman.EditorTools.GoonsGraphMade.Answers, answer, "a card orders \"law goons_" + kind + "\", and the graph has no row for it");
            }
            // and its machine is on an object of the game's scene (the scene's file names the graph)
            string guid = AssetDatabase.AssetPathToGUID(Kapman.EditorTools.GoonsGraphMade.Path);
            StringAssert.Contains(guid, System.IO.File.ReadAllText(Kapman.EditorTools.KapmanPhysical.GameScene),
                "the game's scene does not run the graph of the lender's men (Kapman > Make physical > 3d)");
        }

        /// <summary>Every law an option of a card can order (its costs, its effects, and both ends of a chance).</summary>
        static IEnumerable<string> LawsOf(OptionDef option)
        {
            foreach (var effect in option.cost.Concat(option.effects))
                foreach (var verb in effect.Verbs())
                    if (verb.verb == "law")
                        yield return verb.a;
        }

        [Test]
        public void EachAnswerDoesByTheGraphWhatItDoesByTheCode()
        {
            Assert.IsNotNull(graph, "no graph of the lender's men");
            // kind, the lender's temper, visits unpaid before, silver in the purse, casks of wine, men aboard
            var cases = new (string kind, double temper, int unpaid, double silver, int casks, int men)[]
            {
                ("goons_paid", 3.0, 2, 300.0, 4, 4), ("goons_paid_well", 3.0, 2, 600.0, 4, 4), ("goons_done", 4.0, 2, 300.0, 0, 4),
                ("goons_cargo", 3.0, 1, 0.0, 8, 4), ("goons_cargo", 2.0, 0, 50.0, 0, 4),
                ("goons_astray", 1.0, 0, 300.0, 4, 4), ("goons_drunk", 2.0, 0, 300.0, 4, 4), ("goons_drunk", 2.0, 0, 300.0, 0, 4),
                ("goons_poured", 2.0, 0, 300.0, 4, 4), ("goons_poured", 2.0, 0, 300.0, 0, 4), ("goons_shaken", 2.0, 1, 300.0, 4, 4), ("goons_routed", 2.0, 2, 300.0, 4, 3),
                ("goons_fooled_hide", 1.0, 0, 300.0, 4, 4), ("goons_fooled_plead", 3.0, 1, 0.0, 0, 2), ("goons_no_such_answer", 2.0, 0, 300.0, 4, 4),
                // given nothing: every rung
                ("goons_teeth", 1.0, 0, 300.0, 4, 4),          // the lender easy: they only talk
                ("goons_teeth", 2.0, 0, 300.0, 4, 4),          // they take silver
                ("goons_teeth", 2.0, 0, 60.0, 8, 4),           // silver short: cargo for the rest
                ("goons_teeth", 2.0, 0, 0.0, 0, 4),            // nothing aboard: nothing taken
                ("goons_teeth", 3.0, 0, 0.0, 0, 4),            // nothing aboard, and he is angry: a man is beaten
                ("goons_teeth", 4.0, 0, 0.0, 0, 4),            // past patience: a man is killed
                ("goons_teeth", 4.0, 0, 0.0, 0, 1),            // never the last man
                ("goons_teeth", 2.0, 2, 300.0, 4, 4),          // the third time: Ravn
                ("goons_teeth", 2.0, 2, 300.0, 4, 1),          // the third time, and he is her last man: not Ravn
                ("goons_teeth_angry", 1.0, 0, 300.0, 4, 4), ("goons_teeth_angry", 2.0, 0, 300.0, 4, 4), ("goons_teeth_angry", 3.0, 0, 0.0, 0, 4), ("goons_teeth_angry", 3.0, 1, 600.0, 4, 4),
            };
            var rungs = new HashSet<int>();
            var told = new System.Text.StringBuilder();
            foreach (var c in cases)
            {
                string byCode = Play(false, c.kind, c.temper, c.unpaid, c.silver, c.casks, c.men, out int doesCode);
                string byGraph = Play(true, c.kind, c.temper, c.unpaid, c.silver, c.casks, c.men, out int doesGraph);
                string what = $"{c.kind}, temper {c.temper}, {c.unpaid} unpaid, {c.silver} d, {c.casks} casks, {c.men} men";
                Assert.AreEqual(byCode, byGraph, what + ": the graph \"Encounter - the lender's men\" and the code (Collectors.AnsweredInCode) do not do the same. " +
                    "The graph is what the game goes by; bring the code's order in line with it, or delete the graph to have it made again.");
                told.AppendLine(what + "\n   " + byGraph);
                if (c.kind.StartsWith("goons_teeth", System.StringComparison.Ordinal))
                    rungs.Add(doesGraph);
            }
            // (every rung was climbed: they talk, take, beat, kill, and kill Ravn)
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, rungs, "the cases never came to one of the five things they do:\n" + told);
            StringAssert.Contains("looks at Ravn", told.ToString(), "Ravn is never killed in the cases");
            StringAssert.Contains("They sleep.", told.ToString());
            StringAssert.Contains("goes on the stick", told.ToString());
        }

        [Test]
        public void TheLendersDayAndGivingUpByTheGraphAsByTheCode()
        {
            Assert.IsNotNull(graph, "no graph of the lender's men");
            // his day to send, and she has paid him lately: he cools, and sends nobody
            Fresh();
            Set(nameof(Collectors.Sent), 1);
            Set(nameof(Collectors.PaidLately), true);
            Set(nameof(Collectors.Temper), 3.0);
            Assert.IsTrue(LendersMenFlow.Run(graph, "due"));
            Assert.AreEqual(2.0, Collectors.Temper, 1e-9, "she has paid him lately, and the graph does not cool him by one (as the code does)");
            Assert.AreEqual(GoonState.Home, Collectors.State, "she has paid him lately, and the graph sends his men all the same");
            // she has not: his men set out
            Fresh();
            Set(nameof(Collectors.Sent), 1);
            Set(nameof(Collectors.PaidLately), false);
            Assert.IsTrue(LendersMenFlow.Run(graph, "due"));
            Assert.AreNotEqual(GoonState.Home, Collectors.State, "his day to send has come unpaid, and the graph sends nobody");
            Assert.AreEqual(1, Collectors.Visit);
            Assert.AreEqual(Collectors.Rules.temperStart, Collectors.Temper, 1e-9, "sending his men moves his temper");
            // they give up: he is the angrier by one, as by the code
            Fresh();
            Collectors.PutOut(1, quay, 2.0, Collectors.Rules.home);
            Assert.IsTrue(LendersMenFlow.Run(graph, "gave_up"));
            Assert.AreEqual(3.0, Collectors.Temper, 1e-9, "they give up, and the graph does not make him the angrier by one (as the code does)");
            // no such moment, no such graph
            Assert.IsFalse(LendersMenFlow.Run(graph, "no such moment"));
            Assert.IsFalse(LendersMenFlow.Run(null, "due"));
        }
    }
}
