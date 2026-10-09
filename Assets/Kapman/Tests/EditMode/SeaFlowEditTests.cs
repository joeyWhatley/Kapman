using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Graphs;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;

namespace Kapman.Tests
{
    /// <summary>
    /// WHAT THE SEA TELLS HER IS A FLOW, RUN BY A GRAPH (the user's ruling of 7 Oct 2026; put right on 9 Oct). Held here,
    /// without the screen:
    ///   - the graph "Voyage - the sea moves her" is there and the game's scene runs it; its event's words go to the
    ///     log, and the clock drops back behind a test of whether the moment calls for it, for the moment's own reason;
    ///   - the log is told by the graph what it is told by the code, for each of the five moments;
    ///   - the lender's men's two (their boat sighted, a quay telling of them) reach the log through their own graph.
    /// That the clock does drop, and that the played game goes by the graph, is held by SeaMovesHerTests and
    /// SeaReckoningTests (play).
    /// </summary>
    public class SeaFlowEditTests
    {
        ScriptGraphAsset graph;
        readonly List<string> heard = new List<string>();

        void Hear(string words) => heard.Add(words);

        [SetUp]
        public void Load()
        {
            graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.SeaGraphMade.Path);
            heard.Clear();
            Voyage.Notice += Hear;
        }

        [TearDown]
        public void Done()
        {
            Voyage.Notice -= Hear;
            SeaMoments.Flow = null;
            Collectors.Flow = null;
            Collectors.Reset();
        }

        [Test]
        public void TheGraphIsThereTheSceneRunsItAndItTellsTheLogAndDropsTheClock()
        {
            Assert.IsNotNull(graph, "no graph of the sea's moments (Kapman > Make physical > 3f. The graph of the sea's moments)");
            var on = graph.graph.units.OfType<OnTheSeaMovesHer>().ToList();
            Assert.AreEqual(1, on.Count, "the graph does not have just one \"On the sea moves her\"");
            var say = graph.graph.units.OfType<LogSay>().FirstOrDefault();
            Assert.IsNotNull(say, "the graph tells the log nothing");
            Assert.IsTrue(say.words.hasValidConnection && say.words.connection.source.unit == on[0] && say.words.connection.source.key == "words",
                "what the log is told is not the moment's own words");
            var drop = graph.graph.units.OfType<ClockDropBack>().FirstOrDefault();
            Assert.IsNotNull(drop, "the graph never drops the clock back");
            Assert.IsTrue(drop.why.hasValidConnection && drop.why.connection.source.unit == on[0] && drop.why.connection.source.key == "why",
                "the clock drops back for a reason that is not the moment's own");
            var test = graph.graph.units.OfType<If>().FirstOrDefault();
            Assert.IsNotNull(test, "the clock drops back for every moment, whether it calls for it or not");
            Assert.IsTrue(test.condition.hasValidConnection && test.condition.connection.source.key == "drops");
            Assert.IsTrue(drop.enter.hasValidConnection && drop.enter.validConnections.Any(c => c.source == test.ifTrue), "the clock does not drop back where the moment calls for it");
            string guid = AssetDatabase.AssetPathToGUID(Kapman.EditorTools.SeaGraphMade.Path);
            StringAssert.Contains(guid, System.IO.File.ReadAllText(Kapman.EditorTools.KapmanPhysical.GameScene),
                "the game's scene does not run the graph of the sea's moments (Kapman > Make physical > 3f)");
        }

        [Test]
        public void TheLogIsToldByTheGraphWhatItIsToldByTheCode()
        {
            Assert.IsNotNull(graph, "no graph of the sea's moments");
            var moments = new (string what, string words, ClockReason? drop)[]
            {
                ("off_line", Adrift.OffLineWords(9000.0, new Kapman.Game.Sim.WorldXZ(6000.0, 6000.0)), ClockReason.OffLine),
                ("lee_shore", Adrift.LeeShoreWords, ClockReason.LeeShore),
                ("lee_shore", Adrift.LeeShoreGaleWords, ClockReason.LeeShore),
                ("adrift", Adrift.DriftWords, null),
                ("lost", Adrift.LostWords("Dorestat"), ClockReason.OffLine),
                ("rough", "A tide race: the tide runs hard here, and the water boils.", ClockReason.RoughWater),
                ("nothing to say", "", null),
            };
            foreach (var m in moments)
            {
                heard.Clear();
                SeaMoments.Flow = null;
                int codes = SeaMoments.ByCode;
                SeaMoments.Raise(m.what, m.words, m.drop);
                Assert.AreEqual(codes + 1, SeaMoments.ByCode, m.what + ": not run by the code");
                var byCode = new List<string>(heard);
                heard.Clear();
                SeaMoments.Flow = () => SeaFlow.Run(graph);
                int graphs = SeaMoments.ByGraph;
                SeaMoments.Raise(m.what, m.words, m.drop);
                Assert.AreEqual(graphs + 1, SeaMoments.ByGraph, m.what + ": not run by the graph's flow");
                CollectionAssert.AreEqual(byCode, heard, m.what + ": the log is not told by the graph what it is told by the code");
                Assert.AreEqual(m.words.Length > 0 ? 1 : 0, heard.Count, m.what + ": the log was told " + heard.Count + " things");
                Assert.AreEqual((m.what, m.words, m.drop.HasValue), (SeaMoments.What, SeaMoments.Words, SeaMoments.Drops));
            }
            SeaMoments.Flow = null;
        }

        [Test]
        public void WordOfTheLendersMenReachesTheLogThroughTheirGraph()
        {
            var theirs = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.GoonsGraphMade.Path);
            Assert.IsNotNull(theirs, "no graph of the lender's men");
            foreach (string moment in new[] { "sighted", "word" })
            {
                var of = LendersMenEvent.Of(moment);
                Assert.IsNotNull(of, moment);
                Assert.AreEqual(1, theirs.graph.units.Count(u => u.GetType() == of), "the graph of the lender's men does not have just one event for \"" + moment + "\"");
                heard.Clear();
                typeof(Collectors).GetProperty(nameof(Collectors.Words)).GetSetMethod(true).Invoke(null, new object[] { "Word of them: " + moment + "." });
                Assert.IsTrue(LendersMenFlow.Run(theirs, moment));
                CollectionAssert.AreEqual(new[] { "Word of them: " + moment + "." }, heard, moment + ": the log was not told through their graph");
            }
        }
    }
}
