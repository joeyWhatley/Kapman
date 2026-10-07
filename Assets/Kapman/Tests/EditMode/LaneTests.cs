using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The sea lanes (Bake/lanes, lanes.bin): every port has a berth, the routes the bake's checker passed come
    /// out the same in the game, and a river barge can't cross the open sea.</summary>
    public class LaneTests
    {
        const string File = "G:/kapman_unity/data/baked/world/lanes.bin";
        LaneGraph g;

        [SetUp]
        public void Load()
        {
            if (!System.IO.File.Exists(File))
                Assert.Ignore("no lanes.bin: run Bake/lanes/bake_lanes.py");
            g = LaneGraph.Load(File);
        }

        [Test]
        public void EveryPortHasABerth() => Assert.GreaterOrEqual(g.berths.Count, 50);

        [Test]
        public void DorestadToLundenwic()
        {
            var router = new LaneRouter(g);
            var sea = LaneRouter.Profile.For(sea: true, coast: true, river: true, small: false);
            var edges = new List<int>();
            Assert.IsTrue(router.Route("dorestad", "lundenwic", sea, edges, out double m));
            Assert.That(m / 1000.0, Is.InRange(380.0, 480.0), "km");
            var again = new List<int>();
            router.Route("dorestad", "lundenwic", sea, again, out _);
            CollectionAssert.AreEqual(edges, again, "the same query gives the same route");
            var course = new List<(double x, double z)>();
            router.Course(g.berths["dorestad"], edges, course);
            Assert.Greater(course.Count, 10);
            Assert.AreEqual(g.nodeX[g.berths["lundenwic"]], course[course.Count - 1].x, 1.0, "ends at Lundenwic's berth");
        }

        [Test]
        public void TheMerchantsCourseGoesByWaterAndOnlyWhereTheWorldHasIt()
        {
            GameData.LoadPorts(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            var dorestad = GameData.Port("dorestad");
            var (x, z) = WorldFrame.ToWorld(dorestad.lat, dorestad.lon);
            var line = new List<Kapman.Game.Sim.WorldXZ>();
            Assert.IsTrue(Course.Plan("lundenwic", new Kapman.Game.Sim.WorldXZ(x, z), "dorestad", line, out double km), "Dorestad to Lundenwic");
            Assert.That(km, Is.InRange(350.0, 560.0), "km by water");
            var london = GameData.Port("lundenwic");
            var (lx, lz) = WorldFrame.ToWorld(london.lat, london.lon);
            Assert.AreEqual(lx, line[line.Count - 1].x, 1.0, "it ends at Lundenwic");
            Assert.AreEqual(lz, line[line.Count - 1].z, 1.0);
            Assert.Greater(Course.Days(km), 2.0, "days at sea");
            // a port up a river whose water isn't in the world yet: no course there
            int unreachable = 0;
            foreach (var p in GameData.Ports)
                if (!Course.Plan(p.id, new Kapman.Game.Sim.WorldXZ(x, z), "dorestad", line, out _))
                    unreachable++;
            Assert.Greater(unreachable, 0, "some ports wait for their rivers");
            Assert.Less(unreachable, GameData.Ports.Count - 10, "but most are reachable");
        }

        [Test]
        public void TheCourseKnowsItsFlatsAndWitlaHasNoWayInButOverThem()
        {
            GameData.LoadPorts(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            var dorestad = GameData.Port("dorestad");
            var (x, z) = WorldFrame.ToWorld(dorestad.lat, dorestad.lon);
            var from = new Kapman.Game.Sim.WorldXZ(x, z);
            var line = new List<Kapman.Game.Sim.WorldXZ>();
            var water = new List<float>();
            // every leg has its water; Witla's approach crosses the flats (the user, 2026-10-03: courses avoid them where
            // there's a way; where there's none, she waits for the tide before them)
            Assert.IsTrue(Course.Plan("witla", from, "dorestad", line, water, out _), "Dorestad to Witla");
            Assert.AreEqual(line.Count, water.Count, "a depth for every point");
            Assert.IsTrue(float.IsInfinity(water[0]) && float.IsInfinity(water[water.Count - 1]), "no leg ends at the first point; the berth isn't judged");
            Assert.IsTrue(water.Exists(w => w < Course.KeepOffBelowM), "Witla's way has flats on it");
            // and when it's set, the run of flats is found ahead of her, with a distance to it
            Assert.IsTrue(Course.SetTo("witla", from, "dorestad"));
            try
            {
                // (from Dorestad the flats are far down the river: she's taken to the point before them first)
                int flats = Course.Depths.FindIndex(w => w < Course.KeepOffBelowM);
                Assert.Greater(flats, 0);
                var before = Course.Points[flats - 1];
                Course.Advance(before, 1.0);
                Assert.IsTrue(TideWait.ShallowRunAhead(before, out int first, out int last, out float shallowest, out double metresTo), "a run ahead from just before it");
                Assert.AreEqual(flats, first);
                Assert.GreaterOrEqual(last, first);
                Assert.Less(shallowest, Course.KeepOffBelowM);
                Assert.Less(metresTo, TideWait.LookAheadM);
            }
            finally
            {
                Course.Clear();
            }
            // some port has a way with no flats at all
            int clear = 0;
            foreach (var p in GameData.Ports)
                if (p.id != "dorestad" && Course.Plan(p.id, from, "dorestad", line, water, out _) && !water.Exists(w => w < Course.KeepOffBelowM))
                    clear++;
            Assert.Greater(clear, 0, "some ports are reached without crossing flats");
        }

        [Test]
        public void TheMerchantsCourseKeepsToDeepWaterWhereItCan()
        {
            var deep = Course.Profile();
            var any = Course.Profile();
            any.minDepth = 0f;
            // a lane that dries costs the most, one over deep water no more; rivers and gates aren't judged by it
            int drying = -1, deepSea = -1;
            for (int e = 0; e < g.EdgeCount && (drying < 0 || deepSea < 0); e++)
            {
                if (g.edgeKind[e] == LaneGraph.Flats && (g.edgeFlags[e] & LaneGraph.FlagGate) == 0 && g.edgeDepth[e] <= 0f && drying < 0)
                    drying = e;
                if (g.edgeKind[e] == LaneGraph.Sea && g.edgeDepth[e] > 10f && deepSea < 0)
                    deepSea = e;
            }
            Assert.GreaterOrEqual(drying, 0, "a lane over flats that dry");
            Assert.AreEqual(Course.ShallowCost, deep.ShallowFactor(g, drying), 1e-4, "drying flats");
            Assert.AreEqual(1f, deep.ShallowFactor(g, deepSea), 1e-6, "deep sea");
            Assert.AreEqual(1f, any.ShallowFactor(g, drying), 1e-6, "no rule, no cost");
            // from Dorestad to every port she can reach: taken together, the routes go over less shallow water (weighted by
            // how shallow) than without the rule - so the rule changes some of them - and they're not much longer
            var router = new LaneRouter(g);
            var a = new List<int>();
            var b = new List<int>();
            double Shallowness(List<int> edges)
            {
                double m = 0.0;
                foreach (int e in edges)
                    m += g.edgeLength[e] * (deep.ShallowFactor(g, e) - 1f);
                return m;
            }
            double shallowAny = 0.0, shallowDeep = 0.0, lengthAny = 0.0, lengthDeep = 0.0;
            int changed = 0;
            foreach (var port in g.berths.Keys)
            {
                if (port == "dorestad" || !router.Route("dorestad", port, any, a, out double ma) || !router.Route("dorestad", port, deep, b, out double mb))
                    continue;
                shallowAny += Shallowness(a);
                shallowDeep += Shallowness(b);
                lengthAny += ma;
                lengthDeep += mb;
                if (!a.SequenceEqual(b))
                    changed++;
            }
            Assert.Greater(changed, 0, "some routes keep off shallows now");
            Assert.Less(shallowDeep, shallowAny, "less shallow water, weighted by how shallow");
            Assert.Less(lengthDeep / lengthAny, 1.15, "how much further, all together");
        }

        [Test]
        public void SmallBoatsReachHedebyAndRiverCraftKeepOffTheSea()
        {
            var router = new LaneRouter(g);
            var edges = new List<int>();
            var small = LaneRouter.Profile.For(sea: true, coast: true, river: true, small: true);
            Assert.IsTrue(router.Route("ripa", "sliesthorp", small, edges, out _), "Ribe to Hedeby");
            var barge = LaneRouter.Profile.For(sea: false, coast: false, river: true, small: false);
            if (router.Route("dorestad", "birka", barge, edges, out _))
                foreach (int e in edges)
                    Assert.AreNotEqual(LaneGraph.Sea, g.edgeKind[e], "a river barge on the open sea");
        }
    }
}
