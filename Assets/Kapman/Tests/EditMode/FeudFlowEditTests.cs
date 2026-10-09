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
    /// WHAT SHE DOES ABOUT RAVN'S DEATH IS A FLOW, RUN BY A GRAPH (the user's ruling of 7 Oct 2026; put right on 9 Oct,
    /// after "Btw is this all being built like I asked"). Held here, without the screen:
    ///   - the graph "Feud - what she does about Ravn's death" is there, the game's scene runs it, and it has a row
    ///     for every answer the feud's cards can order;
    ///   - what each answer does by the graph is what it does by the code: the same game played twice, the last
    ///     answer once by the code's own order of the parts and once by the graph's flow run straight from its file.
    /// That the played game goes by the graph is held by FeudTests (play).
    /// </summary>
    public class FeudFlowEditTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        EncounterState flags;
        ScriptGraphAsset graph;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            RuleGraphs.Install();
            graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.FeudGraphMade.Path);
        }

        [TearDown]
        public void Done()
        {
            Feud.AnswerFlow = null;
            Collectors.Flow = null;
            Collectors.Reset();
            Feud.Reset();
            Self.Reset();
            ContentFlags.Use(null);
            Markets.Clear();
            GameRules.Graph = null;
        }

        /// <summary>A game in which the lender's men have just killed Ravn, his scene is past, and she stands over him:
        /// four men were aboard, so much silver in the purse, so many casks of wine in the hold.</summary>
        void RavnIsDead(double silver, int casks, int men)
        {
            Markets.Clear();
            Collectors.Reset();
            Feud.Reset();
            Self.Reset();
            Self.SetStat("nerve", 3.0);
            Self.SetStat("name", 3.0);
            flags = new EncounterState();
            ContentFlags.Use(flags, () => 0.0);
            flags.SetFlag(Collectors.OpeningFlag, 1.0, 0.0, 0.0);
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Voyage.SetMen(4);
            Voyage.Affect(2.0, 2.0);
            var home = GameData.Port(Collectors.Rules.home);
            var (x, z) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            Collectors.Refused(2);
            Collectors.PutOut(3, new WorldXZ(x, z), 2.0, Collectors.Rules.home);
            Collectors.Law("goons_teeth");
            Assert.AreEqual(FeudStage.Scene, Feud.Stage, "the third time she gave them nothing, and Ravn lives");
            Feud.PastScene();
            Trade.Purse.PayUpTo(Trade.Purse.Silver(TradeRules.Current), TradeRules.Current);
            if (silver > 0.0)
                Trade.Purse.Add("denier", silver);
            foreach (var (good, count) in new List<(GoodDef good, int count)>(Trade.Hold.Contents))
                Trade.Hold.Remove(good, count);
            if (casks > 0)
                Trade.Hold.Add(GameData.Good("rhenish_wine"), casks, 0.0, 0.0);
            Voyage.SetMen(men);
        }

        string Shot(string said, int deedsBefore)
        {
            var rules = TradeRules.Current;
            string hold = string.Join(",", Trade.Hold.Contents.Where(l => l.count > 0).OrderBy(l => l.good.id, System.StringComparer.Ordinal).Select(l => l.good.id + "x" + l.count));
            string deeds = string.Join(",", Deeds.Log.Skip(deedsBefore).Select(d => d.deed));
            return $"{Feud.Stage} ended {Feud.Ended} sworn {Feud.IsSworn} at law {Feud.IsAtLaw} can swear {Feud.CanSwear} outlaw {Feud.IsOutlaw} aboard {Feud.LiesAboard} buried {Feud.Var("feud.buried")} " +
                   $"killer alive {Feud.KillerAlive} lender dead {Feud.LenderDead} quiet {flags.Flag(Feud.Quiet, 0.0)} works {flags.Flag(Trade.WorkFlag, 0.0)} ceiling {Feud.HeartCeiling:0.###} " +
                   $"mark {Self.HasMark(Feud.Mark)} owed {Trade.Debt:0.##} silver {Trade.Purse.Silver(rules):0.##} hold [{hold}] men {Voyage.Men} heart {Voyage.Morale:0.###} " +
                   $"temper {Collectors.Temper:0.###} deeds [{deeds}]\n   said: {said}";
        }

        /// <summary>One game: the answers before the last by the code, the last by the code's own order or by the graph's flow.</summary>
        string Play(bool byGraph, string[] before, string last, double silver, int casks, int men)
        {
            Feud.AnswerFlow = null;
            if (last == "feud_swear_with_nobody_dead")
            {
                // (nobody is dead: an answer does nothing)
                Markets.Clear();
                Collectors.Reset();
                Feud.Reset();
                Self.Reset();
                flags = new EncounterState();
                ContentFlags.Use(flags, () => 0.0);
                Trade.Begin(TradeRules.Current, 900f, 0.0);
                last = "feud_swear";
            }
            else
                RavnIsDead(silver, casks, men);
            foreach (string law in before)
            {
                if (law == "works_for_him")
                    flags.SetFlag(Trade.WorkFlag, 1.0, 0.0, 0.0);
                else
                    Feud.Law(law);
            }
            int deeds = Deeds.Log.Count, graphs = Feud.ByGraph, codes = Feud.ByCode;
            Feud.AnswerFlow = byGraph ? (System.Func<bool>)(() => FeudFlow.Run(graph)) : null;
            string said = Feud.Law(last);
            if (byGraph)
                Assert.AreEqual((graphs + 1, codes), (Feud.ByGraph, Feud.ByCode), last + ": the answer was not run by the graph's flow");
            else
                Assert.AreEqual((graphs, codes + 1), (Feud.ByGraph, Feud.ByCode), last + ": the answer was not run by the code");
            Feud.AnswerFlow = null;
            return Shot(said, deeds);
        }

        [Test]
        public void TheGraphIsThereTheSceneRunsItAndEveryAnswerHasItsRow()
        {
            Assert.IsNotNull(graph, "no graph of the feud (Kapman > Make physical > 3e. The graph of the feud)");
            Assert.AreEqual(1, graph.graph.units.Count(u => u is OnRavnsDeathIsAnswered), "the graph does not have just one \"On Ravn's death is answered\"");
            var which = graph.graph.units.OfType<SwitchOnString>().FirstOrDefault();
            Assert.IsNotNull(which, "the graph does not switch on the answer");
            CollectionAssert.AreEquivalent(Kapman.EditorTools.FeudGraphMade.Answers, which.options, "the graph's rows are not the answers the code knows");
            // every answer a card can order has its row
            var ordered = new HashSet<string>();
            foreach (var card in EncounterData.Book().cards)
                foreach (var option in card.options)
                    foreach (var effect in option.cost.Concat(option.effects))
                        foreach (var verb in effect.Verbs())
                            if (verb.verb == "law" && verb.a.StartsWith("feud_", System.StringComparison.Ordinal))
                                ordered.Add(verb.a.Substring("feud_".Length));
            Assert.Greater(ordered.Count, 8, "the cards order nothing of the feud");
            foreach (string answer in ordered)
                CollectionAssert.Contains(Kapman.EditorTools.FeudGraphMade.Answers, answer, "a card orders \"law feud_" + answer + "\", and the graph has no row for it");
            // and its machine is on an object of the game's scene (the scene's file names the graph)
            string guid = AssetDatabase.AssetPathToGUID(Kapman.EditorTools.FeudGraphMade.Path);
            StringAssert.Contains(guid, System.IO.File.ReadAllText(Kapman.EditorTools.KapmanPhysical.GameScene),
                "the game's scene does not run the graph of the feud (Kapman > Make physical > 3e)");
        }

        [Test]
        public void EachAnswerDoesByTheGraphWhatItDoesByTheCode()
        {
            Assert.IsNotNull(graph, "no graph of the feud");
            string[] none = { }, sworn = { "feud_swear" }, atLaw = { "feud_law" }, quiet = { "feud_quiet" }, aboard = { "feud_swear", "feud_bury_aboard" },
                     lenderDead = { "feud_swear", "works_for_him", "feud_lender" };
            // the answers before, the answer, silver in the purse, casks of wine, men aboard
            var cases = new (string[] before, string last, double silver, int casks, int men)[]
            {
                (none, "feud_swear", 300.0, 4, 3), (sworn, "feud_swear", 300.0, 4, 3), (atLaw, "feud_swear", 300.0, 4, 3), (none, "feud_law", 300.0, 4, 3), (sworn, "feud_law", 300.0, 4, 3),
                (none, "feud_quiet", 300.0, 4, 3), (none, "feud_swear_with_nobody_dead", 300.0, 4, 3),
                (sworn, "feud_bury_priest", 300.0, 4, 3), (sworn, "feud_bury_water", 300.0, 4, 3), (sworn, "feud_bury_aboard", 300.0, 4, 3), (aboard, "feud_pyre", 300.0, 4, 3),
                (sworn, "feud_blood", 300.0, 4, 3), (sworn, "feud_blood", 0.0, 0, 3), (quiet, "feud_blood", 300.0, 4, 3),
                (sworn, "feud_blood_lost", 300.0, 4, 3), (sworn, "feud_blood_lost", 300.0, 4, 1),
                (sworn, "feud_lender_lost", 300.0, 4, 3), (sworn, "feud_lender", 300.0, 4, 3), (new[] { "feud_swear", "works_for_him" }, "feud_lender", 300.0, 4, 3),
                (atLaw, "feud_price", 300.0, 4, 3),
                (lenderDead, "feud_outlaw_seized", 300.0, 4, 3), (lenderDead, "feud_outlaw_seized", 300.0, 0, 3),
                (lenderDead, "feud_outlaw_paid", 300.0, 4, 3), (lenderDead, "feud_outlaw_paid", 2000.0, 4, 3),
                (quiet, "feud_let_lie", 300.0, 4, 3), (sworn, "feud_no_such_answer", 300.0, 4, 3),
            };
            var told = new System.Text.StringBuilder();
            foreach (var c in cases)
            {
                string byCode = Play(false, c.before, c.last, c.silver, c.casks, c.men);
                string byGraph = Play(true, c.before, c.last, c.silver, c.casks, c.men);
                string what = $"{string.Join(", ", c.before)} then {c.last}, {c.silver} d, {c.casks} casks, {c.men} men";
                Assert.AreEqual(byCode, byGraph, what + ": the graph \"Feud - what she does about Ravn's death\" and the code (Feud.AnsweredInCode) do not do the same. " +
                    "The graph is what the game goes by; bring the code's order in line with it, or delete the graph to have it made again.");
                told.AppendLine(what + "\n   " + byGraph);
            }
            string all = told.ToString();
            // (the cases came to every end)
            foreach (string must in new[] { "ended 1", "ended 2", "ended 3", "ended 4", "outlaw True", "outlaw no longer", "outlaw still", "take the cargo", "find nothing", "does not get up again",
                                            "thrown", "off your stick", "nothing is owed", "took blood for her man", "killed the lender", "aboard True", "buried 1", "buried 2", "buried 3" })
                StringAssert.Contains(must, all.Replace("throw you", "thrown"), "the cases never came to: " + must);
        }
    }
}
