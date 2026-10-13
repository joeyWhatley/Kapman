using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Graphs;
using Kapman.World;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// YOUR HAND ON THE HELM (the user, 13 Oct 2026: "need a way to control while in 3d mode"; by the picker: "Steer her
    /// yourself in 3D"). Held here, without the screen:
    ///   - the helm taken, the helm over, let go, and given back: the moments in their order, and what the log is told;
    ///   - given back with a course set, the steersman lays her course again from where she is, through her knots, on
    ///     to the same end; with no course, or in thick air, she lies to;
    ///   - the graph "Voyage - the helm is yours" is there, the scene runs it, and it lays the course and tells the log
    ///     as the code does.
    /// The turning under the hand, the keys and the mark are held by HelmPlayTests (play); the two rules by
    /// RuleGraphTests.
    /// </summary>
    public class HelmEditTests
    {
        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + name);

        readonly List<string> heard = new List<string>();
        void Hear(string words) => heard.Add(words);

        [SetUp]
        public void Begin()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            Exploration.Clear();
            Course.Clear();
            HandHelm.Clear();
            HandHelm.Flow = null;
            CoursePulls.Flow = null;
            heard.Clear();
            Voyage.Notice += Hear;
        }

        [TearDown]
        public void Done()
        {
            Voyage.Notice -= Hear;
            HandHelm.Flow = null;
            HandHelm.Clear();
            Course.Clear();
        }

        static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }

        static WorldXZ Of(string port)
        {
            var p = GameData.Port(port);
            return At(p.lat, p.lon);
        }

        static void NeedLanes()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
        }

        /// <summary>The sea off the coast of Holland.</summary>
        static WorldXZ NorthSea => At(52.6, 4.35);
        /// <summary>Open water off the Maas mouth: where she is when the helm is given back.</summary>
        static WorldXZ OffMaas => At(52.0, 3.9);

        [Test]
        public void TheHelmIsTakenPutOverLetGoAndGivenBack()
        {
            NeedLanes();
            Assert.IsTrue(Course.SetTo("witla", Of("dorestad"), "dorestad"), "no course from Dorestat to Witla");
            CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            Assert.AreEqual(1, Course.Knots.Count);
            heard.Clear();
            int codes = HandHelm.ByCode;
            Assert.IsTrue(HandHelm.CanTake, "the helm may not be taken with no game running");
            Assert.IsTrue(HandHelm.Take(), "the helm was not taken");
            Assert.IsTrue(HandHelm.Held);
            Assert.AreEqual(0f, HandHelm.Rudder);
            Assert.AreEqual("taken", HandHelm.What);
            Assert.AreEqual(codes + 1, HandHelm.ByCode, "the taking was not run by the code");
            Assert.AreEqual(1, heard.Count, "the log was told " + heard.Count + " things: " + string.Join(" | ", heard));
            StringAssert.Contains("helm", heard[0]);
            Assert.IsTrue(Course.IsSet, "the course was given up when the helm was taken: it should wait");
            Assert.IsFalse(HandHelm.Take(), "the helm was taken twice");
            // over, and let go: nothing said
            heard.Clear();
            HandHelm.Steer(1f);
            Assert.AreEqual(1f, HandHelm.Rudder);
            HandHelm.Steer(0.5f);
            Assert.AreEqual(0, heard.Count, "the helm over says something");
            HandHelm.Steer(0f);
            Assert.AreEqual("let_go", HandHelm.What);
            Assert.AreEqual(0, heard.Count, "the helm let go says something: " + string.Join(" | ", heard));
            // given back out on the water: her course laid again from there, through her knot, on to Witla
            heard.Clear();
            Assert.IsTrue(HandHelm.GiveBack(OffMaas, null, false), "the helm was not given back");
            Assert.IsFalse(HandHelm.Held);
            Assert.AreEqual("given_back", HandHelm.What);
            Assert.IsTrue(HandHelm.Laid, "her course was not laid again: " + HandHelm.Words);
            Assert.AreEqual("witla", Course.Destination);
            Assert.Less(WorldXZ.Distance(Course.Points[0], OffMaas), 1.0, "the course laid again does not begin where she is");
            Assert.AreEqual(1, Course.Knots.Count, "the course laid again dropped her knot");
            Assert.AreEqual(1, heard.Count, "the log was told " + heard.Count + " things: " + string.Join(" | ", heard));
            StringAssert.Contains("steersman", heard[0]);
            StringAssert.Contains($"{Course.Km:0} km", heard[0]);
            Assert.IsFalse(HandHelm.GiveBack(OffMaas, null, false), "the helm was given back twice");
            Debug.Log($"The helm given back off the Maas: her course laid again, {Course.Km:0} km through her knot to Witla. Said: {heard[0]}");
        }

        [Test]
        public void GivenBackWithNoCourseOrInThickAirSheLiesTo()
        {
            NeedLanes();
            // no course
            Assert.IsTrue(HandHelm.Take());
            heard.Clear();
            Assert.IsTrue(HandHelm.GiveBack(OffMaas, null, false));
            Assert.IsFalse(HandHelm.Laid);
            Assert.IsFalse(Course.IsSet);
            Assert.AreEqual(1, heard.Count);
            StringAssert.Contains("lies to", heard[0]);
            // thick air: the course waits still, but he cannot lay it again
            Assert.IsTrue(Course.SetTo("witla", Of("dorestad"), "dorestad"));
            var before = new List<WorldXZ>(Course.Points);
            Assert.IsTrue(HandHelm.Take());
            heard.Clear();
            Assert.IsTrue(HandHelm.GiveBack(OffMaas, null, true));
            Assert.IsFalse(HandHelm.Laid, "in thick air he laid her course again");
            Assert.IsTrue(Course.IsSet, "the course was given up in thick air");
            Assert.AreEqual(before.Count, Course.Points.Count, "the course changed in thick air");
            StringAssert.Contains("thick", heard[heard.Count - 1]);
            // stuck fast by your hand: the helm is let go
            Assert.IsTrue(HandHelm.Take());
            heard.Clear();
            HandHelm.RanAground();
            Assert.IsFalse(HandHelm.Held);
            Assert.AreEqual("aground", HandHelm.What);
            StringAssert.Contains("aground", heard[heard.Count - 1]);
        }

        [Test]
        public void TheGraphIsThereAndTheSceneRunsIt()
        {
            var graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.HelmGraphMade.Path);
            Assert.IsNotNull(graph, "no graph of the helm (Kapman > Make physical > 3h. The graph of the helm)");
            var on = graph.graph.units.OfType<OnTheHelm>().ToList();
            Assert.AreEqual(1, on.Count, "the graph does not have just one \"On the helm\"");
            var lay = graph.graph.units.OfType<HelmLayAgain>().FirstOrDefault();
            Assert.IsNotNull(lay, "the graph never lays her course again");
            var says = graph.graph.units.OfType<HelmSay>().ToList();
            Assert.AreEqual(2, says.Count, "the graph should tell the log after the laying, and for the other moments");
            Assert.IsTrue(says.Any(s => s.words.hasValidConnection && s.words.connection.source.unit == lay && s.words.connection.source.key == "words"),
                "what the log is told after the laying is not the laying's own words");
            Assert.IsTrue(says.Any(s => s.words.hasValidConnection && s.words.connection.source.unit == on[0] && s.words.connection.source.key == "words"),
                "what the log is told of the other moments is not the moment's own words");
            var test = graph.graph.units.OfType<If>().FirstOrDefault();
            Assert.IsNotNull(test, "the graph lays her course again at every moment");
            Assert.IsTrue(test.condition.hasValidConnection && test.condition.connection.source.key == "givenBack");
            Assert.IsTrue(lay.enter.hasValidConnection && lay.enter.validConnections.Any(c => c.source == test.ifTrue), "the helm given back does not lay her course again");
            string guid = AssetDatabase.AssetPathToGUID(Kapman.EditorTools.HelmGraphMade.Path);
            StringAssert.Contains(guid, System.IO.File.ReadAllText(Kapman.EditorTools.KapmanPhysical.GameScene),
                "the game's scene does not run the graph of the helm (Kapman > Make physical > 3h)");
        }

        [Test]
        public void TheHelmGoesByTheGraphAsByTheCode()
        {
            NeedLanes();
            var graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.HelmGraphMade.Path);
            Assert.IsNotNull(graph, "no graph of the helm");
            foreach (string moment in new[] { "taken", "let_go", "given_back", "given_back_no_course", "given_back_thick", "aground" })
            {
                var byCode = Run(moment, null, out var codePoints, out double codeKm);
                var byGraph = Run(moment, () => HelmFlow.Run(graph), out var graphPoints, out double graphKm);
                CollectionAssert.AreEqual(byCode, byGraph, moment + ": the log is not told by the graph what it is told by the code");
                Assert.AreEqual(codeKm, graphKm, 1e-6, moment + ": the graph lays a course of another length");
                Assert.AreEqual(codePoints, graphPoints, moment + ": the graph lays a course of other points");
            }
        }

        /// <summary>One moment of the helm from a fresh beginning, by the code (no flow) or by the graph: what the log
        /// was told, and the course after.</summary>
        List<string> Run(string moment, System.Func<bool> flow, out int points, out double km)
        {
            Course.Clear();
            HandHelm.Clear();
            HandHelm.Flow = null;
            if (!moment.EndsWith("no_course"))
                Assert.IsTrue(Course.SetTo("witla", Of("dorestad"), "dorestad"));
            Assert.IsTrue(HandHelm.Take());
            if (moment != "taken")
                HandHelm.Steer(1f);
            heard.Clear();
            HandHelm.Flow = flow;
            int codes = HandHelm.ByCode, graphs = HandHelm.ByGraph;
            switch (moment)
            {
                case "taken": HandHelm.Clear(); HandHelm.Take(); break;
                case "let_go": HandHelm.Steer(0f); break;
                case "aground": HandHelm.RanAground(); break;
                default: HandHelm.GiveBack(OffMaas, null, moment.EndsWith("thick")); break;
            }
            HandHelm.Flow = null;
            if (flow == null)
                Assert.AreEqual(codes + 1, HandHelm.ByCode, moment + ": not run by the code");
            else
                Assert.AreEqual(graphs + 1, HandHelm.ByGraph, moment + ": not run by the graph's flow");
            points = Course.Points.Count;
            km = Course.Km;
            return new List<string>(heard);
        }
    }
}
