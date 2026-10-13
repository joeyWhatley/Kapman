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
    /// THE COURSE PULLED WITH THE MOUSE (the user, 13 Oct 2026: "you should want to be able to drag and correct the course
    /// on the map with your mouse"; by the picker: pull the line anywhere, by the lanes through your point, a point on
    /// land to the nearest water, in the dark too). Held here, without the screen:
    ///   - a course pulled through a knot goes by water through it and on to the same end, and is the longer for it;
    ///   - a knot pulled again moves, and a knot taken off leaves the course as it was;
    ///   - a point let go on land is taken to the water nearest it;
    ///   - a knot she has passed drops away;
    ///   - the knots are saved, and a course laid again to the same end keeps them; a new end drops them;
    ///   - the graph "Chart - the course is pulled" is there, the scene runs it, and it lays the course and tells the log
    ///     as the code does.
    /// What the chart shows of this, and the hand itself, is held by PullTheCourseTests (play).
    /// </summary>
    public class PulledCourseTests
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
            CoursePulls.Flow = null;
            heard.Clear();
            Voyage.Notice += Hear;
        }

        [TearDown]
        public void Done()
        {
            Voyage.Notice -= Hear;
            CoursePulls.Flow = null;
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

        /// <summary>The sea off the coast of Holland, well north of the way from Dorestat to Witla.</summary>
        static WorldXZ NorthSea => At(52.6, 4.35);

        /// <summary>How far the nearest point of her line is from a place.</summary>
        static double OffLine(WorldXZ place) => Course.Points.Min(p => WorldXZ.Distance(p, place));

        static double DirectCourse()
        {
            Course.Clear();                                              // (a course laid again to the same end keeps its knots: begin with none)
            Assert.IsTrue(Course.SetTo("witla", Of("dorestad"), "dorestad"), "no course from Dorestat to Witla");
            Assert.AreEqual(0, Course.Knots.Count, "a fresh course has knots");
            return Course.Km;
        }

        [Test]
        public void ACoursePulledGoesThroughTheKnotAndOnToTheSameEnd()
        {
            NeedLanes();
            double direct = DirectCourse();
            var end = Course.Points[Course.Points.Count - 1];
            int codes = CoursePulls.ByCode;
            CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            Assert.AreEqual(codes + 1, CoursePulls.ByCode, "the pull was not run by the code");
            Assert.IsTrue(CoursePulls.Laid, "the course was not laid again: " + CoursePulls.Words);
            Assert.AreEqual(1, Course.Knots.Count, "the pull left no knot");
            Assert.AreEqual(1, Course.KnotAt.Count);
            Assert.AreEqual("witla", Course.Destination, "the pull changed her end");
            Assert.Less(WorldXZ.Distance(Course.Points[Course.Points.Count - 1], end), 1.0, "the course no longer ends at Witla's berth");
            Assert.Less(WorldXZ.Distance(Course.Points[Course.KnotAt[0]], Course.Knots[0]), 1.0, "the knot is not a point of the line");
            Assert.Less(WorldXZ.Distance(Course.Knots[0], NorthSea), Course.BeyondLanesM, "the knot is nowhere near where it was let go");
            Assert.Greater(Course.Km, direct + 30.0, $"the course through the north sea is not the longer for it: {Course.Km:0} km against {direct:0} km");
            Assert.AreEqual(1, Course.Next, "a course laid again does not begin at its first point");
            Assert.AreEqual(1, heard.Count, "the log was told " + heard.Count + " things: " + string.Join(" | ", heard));
            StringAssert.Contains("knot", heard[0]);
            StringAssert.Contains($"{Course.Km:0} km", heard[0]);
            Debug.Log($"The course pulled: Dorestat to Witla through the sea off Holland: {Course.Km:0} km against {direct:0} km direct; the knot {WorldXZ.Distance(Course.Knots[0], NorthSea) / 1000.0:0.0} km from where it was let go. Said: {heard[0]}");
        }

        [Test]
        public void AKnotPulledAgainMovesAndAKnotTakenOffLeavesTheCourseAsItWas()
        {
            NeedLanes();
            double direct = DirectCourse();
            CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            Assert.AreEqual(1, Course.Knots.Count);
            double north = Course.Km;
            // pulled again, to the sea off the Maas mouth: nearer the way
            var maas = At(52.05, 3.85);
            CoursePulls.Raise("moved", maas, 0, -1, Of("dorestad"), "dorestad");
            Assert.IsTrue(CoursePulls.Laid, CoursePulls.Words);
            Assert.AreEqual(1, Course.Knots.Count, "a knot moved is a knot more or less");
            Assert.Less(WorldXZ.Distance(Course.Knots[0], maas), Course.BeyondLanesM, "the knot did not move to where it was pulled");
            Assert.Greater(WorldXZ.Distance(Course.Knots[0], NorthSea), 20000.0, "the knot stayed where it was");
            Assert.Less(Course.Km, north, "the course through the Maas mouth is not shorter than through the sea off Holland");
            StringAssert.Contains("moved", heard[heard.Count - 1]);
            // taken off: the course as it was
            CoursePulls.Raise("off", default, 0, -1, Of("dorestad"), "dorestad");
            Assert.IsTrue(CoursePulls.Laid, CoursePulls.Words);
            Assert.AreEqual(0, Course.Knots.Count, "the knot was not taken off");
            Assert.AreEqual(direct, Course.Km, 0.5, "with the knot off, the course is not what it was");
            StringAssert.Contains("taken off", heard[heard.Count - 1]);
        }

        [Test]
        public void AKnotLetGoOnLandIsTakenToTheWaterNearestIt()
        {
            NeedLanes();
            if (!Course.KnowsWater)
                Assert.Ignore("no world bake to tell water from land");
            DirectCourse();
            // the dunes of Holland, a little inland of the sea
            var dunes = At(52.45, 4.62);
            Assert.IsFalse(Course.IsWater(dunes.x, dunes.z), "the dunes are water in the bake");
            CoursePulls.Raise("pulled", dunes, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            Assert.IsTrue(CoursePulls.OnLand, "the point let go was not known to be on land");
            Assert.IsTrue(Course.IsWater(CoursePulls.Where.x, CoursePulls.Where.z), "the knot was not taken to the water");
            Assert.Less(WorldXZ.Distance(CoursePulls.Where, dunes), Course.ToWaterM, "the water found is too far from the point let go");
            Assert.IsTrue(CoursePulls.Laid, CoursePulls.Words);
            Assert.AreEqual(1, Course.Knots.Count);
            StringAssert.Contains("on the land", heard[heard.Count - 1]);
            Debug.Log($"A knot let go on the dunes went {WorldXZ.Distance(CoursePulls.Where, dunes) / 1000.0:0.0} km to the water; the course is {Course.Km:0} km. Said: {heard[heard.Count - 1]}");
        }

        [Test]
        public void AKnotSheHasPassedDropsAway()
        {
            NeedLanes();
            DirectCourse();
            CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            Assert.AreEqual(1, Course.Knots.Count);
            int at = Course.KnotAt[0];
            // she comes up to the point before the knot: the knot stands
            Course.Advance(Course.Points[at - 1], 100.0);
            Assert.AreEqual(1, Course.Knots.Count, "the knot dropped before she reached it");
            // and on past it
            int beyond = System.Math.Min(Course.Points.Count - 1, at + 3);
            Course.Advance(Course.Points[beyond], 100.0);
            Assert.AreEqual(0, Course.Knots.Count, "the knot did not drop when she passed it");
            Assert.AreEqual(0, Course.KnotAt.Count);
            Assert.AreEqual("witla", Course.Destination);
        }

        [Test]
        public void TheKnotsAreSavedAndACourseLaidAgainToTheSameEndKeepsThem()
        {
            NeedLanes();
            DirectCourse();
            CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
            var knot = Course.Knots[0];
            int at = Course.KnotAt[0];
            double km = Course.Km;
            // saved and loaded
            var saved = Course.Capture();
            Course.Clear();
            Assert.AreEqual(0, Course.Knots.Count);
            Course.Restore(saved);
            Assert.AreEqual(1, Course.Knots.Count, "the knot was not saved");
            Assert.Less(WorldXZ.Distance(Course.Knots[0], knot), 0.01);
            Assert.AreEqual(at, Course.KnotAt[0]);
            Assert.AreEqual(km, Course.Km, 1e-6);
            // laid again to the same end from another place (the steersman's own laying again): through the knot still
            var sea = At(52.3, 4.3);
            Assert.IsTrue(Course.SetTo("witla", sea, null), "no course from the sea to Witla");
            Assert.AreEqual(1, Course.Knots.Count, "a course laid again to the same end dropped the knot");
            Assert.Less(OffLine(knot), 2000.0, "the course laid again does not pass the knot");
            // a new end: the knots go
            Assert.IsTrue(Course.SetTo("dorestad", sea, null), "no course from the sea to Dorestat");
            Assert.AreEqual(0, Course.Knots.Count, "a course to a new end kept the knots");
            Assert.AreEqual(0, Course.KnotAt.Count);
        }

        [Test]
        public void TheGraphIsThereAndTheSceneRunsIt()
        {
            var graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.PullGraphMade.Path);
            Assert.IsNotNull(graph, "no graph of the course pulled (Kapman > Make physical > 3g. The graph of the course pulled)");
            var on = graph.graph.units.OfType<OnTheCourseIsPulled>().ToList();
            Assert.AreEqual(1, on.Count, "the graph does not have just one \"On the course is pulled\"");
            var water = graph.graph.units.OfType<KnotToWater>().FirstOrDefault();
            Assert.IsNotNull(water, "the graph never takes a knot to the water");
            var lay = graph.graph.units.OfType<LayThroughKnots>().FirstOrDefault();
            Assert.IsNotNull(lay, "the graph never lays the course again");
            var say = graph.graph.units.OfType<CourseSay>().FirstOrDefault();
            Assert.IsNotNull(say, "the graph tells the log nothing");
            Assert.IsTrue(say.words.hasValidConnection && say.words.connection.source.unit == lay && say.words.connection.source.key == "words",
                "what the log is told is not the laying's own words");
            var test = graph.graph.units.OfType<If>().FirstOrDefault();
            Assert.IsNotNull(test, "the graph takes every knot to the water, on land or not");
            Assert.IsTrue(test.condition.hasValidConnection && test.condition.connection.source.key == "onLand");
            Assert.IsTrue(water.enter.hasValidConnection && water.enter.validConnections.Any(c => c.source == test.ifTrue), "a knot on land is not taken to the water");
            Assert.IsTrue(lay.enter.validConnections.Any(c => c.source == test.ifFalse), "a knot on the water is not laid through");
            Assert.IsTrue(lay.enter.validConnections.Any(c => c.source == water.exit), "a knot taken to the water is not laid through");
            string guid = AssetDatabase.AssetPathToGUID(Kapman.EditorTools.PullGraphMade.Path);
            StringAssert.Contains(guid, System.IO.File.ReadAllText(Kapman.EditorTools.KapmanPhysical.GameScene),
                "the game's scene does not run the graph of the course pulled (Kapman > Make physical > 3g)");
        }

        [Test]
        public void TheCourseIsLaidByTheGraphAsByTheCode()
        {
            NeedLanes();
            var graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Kapman.EditorTools.PullGraphMade.Path);
            Assert.IsNotNull(graph, "no graph of the course pulled");
            var dunes = At(52.45, 4.62);
            var moments = new (string what, WorldXZ where, int knot)[]
            {
                ("pulled", NorthSea, -1),
                ("pulled", dunes, -1),
                ("moved", At(52.05, 3.85), 0),
                ("off", default, 0),
            };
            foreach (var m in moments)
            {
                // by the code
                DirectCourse();
                if (m.what != "pulled")
                    CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
                heard.Clear();
                CoursePulls.Flow = null;
                int codes = CoursePulls.ByCode;
                CoursePulls.Raise(m.what, m.where, m.knot, Course.Points.Count / 2, Of("dorestad"), "dorestad");
                Assert.AreEqual(codes + 1, CoursePulls.ByCode, m.what + ": not run by the code");
                var byCode = new List<string>(heard);
                var codePoints = new List<WorldXZ>(Course.Points);
                var codeKnots = new List<WorldXZ>(Course.Knots);
                double codeKm = Course.Km;
                // by the graph, from the same beginning
                DirectCourse();
                if (m.what != "pulled")
                    CoursePulls.Raise("pulled", NorthSea, -1, Course.Points.Count / 2, Of("dorestad"), "dorestad");
                heard.Clear();
                CoursePulls.Flow = () => PullFlow.Run(graph);
                int graphs = CoursePulls.ByGraph;
                CoursePulls.Raise(m.what, m.where, m.knot, Course.Points.Count / 2, Of("dorestad"), "dorestad");
                CoursePulls.Flow = null;
                Assert.AreEqual(graphs + 1, CoursePulls.ByGraph, m.what + ": not run by the graph's flow");
                CollectionAssert.AreEqual(byCode, heard, m.what + ": the log is not told by the graph what it is told by the code");
                Assert.AreEqual(codeKm, Course.Km, 1e-6, m.what + ": the graph lays a course of another length");
                Assert.AreEqual(codePoints.Count, Course.Points.Count, m.what + ": the graph lays a course of other points");
                Assert.AreEqual(codeKnots.Count, Course.Knots.Count, m.what + ": the graph leaves other knots");
                for (int i = 0; i < codeKnots.Count; i++)
                    Assert.Less(WorldXZ.Distance(codeKnots[i], Course.Knots[i]), 0.01, m.what + ": knot " + i + " lies elsewhere by the graph");
            }
        }
    }
}
