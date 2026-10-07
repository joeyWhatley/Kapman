using System.Collections.Generic;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE OPENED RIVERS (7 Oct 2026, Design/research/next/routes_fix.md): four ports of the first release had no way
    /// in for the player (Quentovic, Ripa, Doccinga, Fordwic: each joined to the lanes only by a "pending" gate), and
    /// the Maas mouth was shut, so the way from Dorestat to the sea went the long way round. LaneOpenings re-kinds
    /// twelve edges to river after the lanes are read, and nothing else. Each test reads its own copy of the lanes; all
    /// are skipped, not failed, on a machine without lanes.bin.
    /// </summary>
    public class LaneOpeningsTests
    {
        /// <summary>The twelve ports of the first release.</summary>
        static readonly string[] FirstRelease =
        {
            "dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic", "ripa",
        };

        static LaneGraph Fresh()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
            return LaneGraph.Load(SimRoutes.DefaultLanes);
        }

        /// <summary>The way of the player between two berths as Course.Plan finds it: off the flats first, then over them.</summary>
        static bool Way(LaneGraph g, string from, string to, List<int> edges)
        {
            edges.Clear();
            if (!g.berths.TryGetValue(from, out int a) || !g.berths.TryGetValue(to, out int b))
                return false;
            var router = new LaneRouter(g);
            foreach (bool offFlats in new[] { true, false })
                if (router.Route(a, b, Course.Profile(offFlats), edges, out _))
                    return true;
            edges.Clear();
            return false;
        }

        static int Count(LaneGraph g, byte kind)
        {
            int n = 0;
            for (int e = 0; e < g.EdgeCount; e++)
                if (g.edgeKind[e] == kind)
                    n++;
            return n;
        }

        static double Km(LaneGraph g, List<int> edges)
        {
            double m = 0.0;
            foreach (int e in edges)
                m += g.edgeLength[e];
            return m / 1000.0;
        }

        /// <summary>How many nodes the player can reach from a berth (any water she may use at all: over the flats too).</summary>
        static int Reach(LaneGraph g, string from)
        {
            var profile = Course.Profile(false);
            var seen = new bool[g.NodeCount];
            var todo = new Stack<int>();
            int start = g.berths[from], n = 0;
            seen[start] = true;
            todo.Push(start);
            while (todo.Count > 0)
            {
                int at = todo.Pop();
                n++;
                for (int k = g.adjStart[at]; k < g.adjStart[at + 1]; k++)
                {
                    int e = g.adjEdge[k];
                    if (float.IsInfinity(profile.cost[g.edgeKind[e]]) || profile.Avoids(g, e))
                        continue;
                    int other = g.Other(e, at);
                    if (seen[other])
                        continue;
                    seen[other] = true;
                    todo.Push(other);
                }
            }
            return n;
        }

        [Test]
        public void Opens_twelve_edges_and_no_more()
        {
            var g = Fresh();
            int pendingBefore = Count(g, LaneGraph.Pending), riverBefore = Count(g, LaneGraph.River);
            int reachBefore = Reach(g, "dorestad");
            int changed = LaneOpenings.Apply(g);
            Assert.AreEqual(12, changed, "four gates (2 + 2 + 2 + 3) and three legs at the Maas mouth");
            Assert.AreEqual(pendingBefore - 12, Count(g, LaneGraph.Pending), "every other pending river stays pending");
            Assert.AreEqual(riverBefore + 12, Count(g, LaneGraph.River));
            Assert.AreEqual(0, LaneOpenings.Apply(g), "a second call finds nothing");
            int more = Reach(g, "dorestad") - reachBefore;
            UnityEngine.Debug.Log($"Lanes: the player reaches {reachBefore} nodes before the fix and {reachBefore + more} after; {pendingBefore - 12} edges stay pending.");
            Assert.Greater(more, 0, "the opened gates reach nothing new");
            Assert.Less(more, 60, "opening four gates and the Maas mouth should add a few tens of nodes, not the 688 of every pending river");
        }

        [TestCase("quentovic")]
        [TestCase("ripa")]
        [TestCase("doccinga")]
        [TestCase("fordwic")]
        public void A_course_is_found_from_dorestad(string port)
        {
            var g = Fresh();
            var edges = new List<int>();
            Assert.IsTrue(g.berths.ContainsKey("dorestad"), "dorestad has a berth in the lanes");
            Assert.IsTrue(g.berths.ContainsKey(port), port + " has a berth in the lanes");
            Assert.IsFalse(Way(g, "dorestad", port, edges), port + " was shut before the fix (lanes baked again? then take it out of LaneOpenings.Gates)");
            LaneOpenings.Apply(g);
            Assert.IsTrue(Way(g, "dorestad", port, edges), "no course from dorestad to " + port);
            Assert.Greater(edges.Count, 0);
            foreach (int e in edges)
                Assert.AreNotEqual(LaneGraph.Pending, g.edgeKind[e], "the course to " + port + " uses a pending river");
            UnityEngine.Debug.Log($"Lanes: dorestad to {port}: {Km(g, edges):0} km by water.");
        }

        [Test]
        public void Every_port_of_the_first_release_is_reached_from_every_home()
        {
            var g = Fresh();
            LaneOpenings.Apply(g);
            var edges = new List<int>();
            foreach (string from in FirstRelease)
                Assert.IsTrue(g.berths.ContainsKey(from), from + " has no berth in the lanes");
            var said = new System.Text.StringBuilder("Lanes, km by water from dorestad:");
            foreach (string to in FirstRelease)
            {
                if (to == "dorestad")
                    continue;
                Assert.IsTrue(Way(g, "dorestad", to, edges), "no course from dorestad to " + to);
                said.Append(' ').Append(to).Append(' ').Append(Km(g, edges).ToString("0"));
                Assert.IsTrue(Way(g, to, "dorestad", edges), "no course home from " + to);
            }
            UnityEngine.Debug.Log(said.ToString());
            // the other three homes (a Frank, an Englishman, a Dane) reach every port too
            foreach (string home in new[] { "quentovic", "lundenwic", "ripa" })
                foreach (string to in FirstRelease)
                    if (to != home)
                        Assert.IsTrue(Way(g, home, to, edges), "no course from " + home + " to " + to);
        }

        [Test]
        public void The_way_to_the_sea_goes_down_the_lek_and_out_by_the_maas_mouth()
        {
            var g = Fresh();
            // the three Maas mouth legs, marked before they are re-kinded
            var maas = new HashSet<int>();
            for (int e = 0; e < g.EdgeCount; e++)
                if (LaneOpenings.IsMaasMouth(g, e))
                    maas.Add(e);
            Assert.AreEqual(LaneOpenings.MaasExpected, maas.Count, "the Maas mouth legs in the lanes");

            var edges = new List<int>();
            Assert.IsTrue(Way(g, "dorestad", "walichrum", edges), "a way to walichrum before the fix");
            double before = Km(g, edges);
            foreach (int e in edges)
                Assert.IsFalse(maas.Contains(e), "before the fix the Maas mouth is shut");
            Assert.IsTrue(Way(g, "dorestad", "lundenwic", edges), "a way to lundenwic before the fix");
            double londonBefore = Km(g, edges);
            Assert.IsTrue(Way(g, "dorestad", "daventre", edges), "a way to daventre before the fix");
            double deventerBefore = Km(g, edges);

            LaneOpenings.Apply(g);
            Assert.IsTrue(Way(g, "dorestad", "walichrum", edges), "no course from dorestad to walichrum");
            int through = 0;
            foreach (int e in edges)
                if (maas.Contains(e))
                    through++;
            Assert.AreEqual(maas.Count, through, "the course runs down the Lek and out by the Maas mouth");
            double after = Km(g, edges);
            Assert.Less(after, before, "the Lek way is the shorter one");

            Assert.IsTrue(Way(g, "dorestad", "lundenwic", edges), "no course from dorestad to lundenwic");
            double london = Km(g, edges);
            through = 0;
            foreach (int e in edges)
                if (maas.Contains(e))
                    through++;
            Assert.AreEqual(maas.Count, through, "the course to Lundenwic runs out by the Maas mouth");
            Assert.Less(london, londonBefore, "Lundenwic is nearer by the Lek");
            Assert.AreEqual(413.0, london, 25.0, "about 411 to 415 km to Lundenwic (routes_fix.md: a plain walk without the shallow-water rule)");

            Assert.IsTrue(Way(g, "dorestad", "daventre", edges), "no course from dorestad to daventre");
            Assert.AreEqual(deventerBefore, Km(g, edges), 0.5, "the way to Daventre is not changed by the fix");
            UnityEngine.Debug.Log($"Lanes: dorestad to walichrum {before:0} km before, {after:0} km after; to lundenwic {londonBefore:0} km before, {london:0} km after; " +
                                  $"to daventre {deventerBefore:0} km.");
        }

        [Test]
        public void The_shared_lanes_are_opened_when_they_are_read()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
            var g = SimRoutes.Graph();
            Assert.IsNotNull(g);
            foreach (string id in LaneOpenings.Gates)
            {
                Assert.IsTrue(g.gates.ContainsKey(id), "no gate " + id);
                foreach (int e in g.gates[id].edges)
                    Assert.AreEqual(LaneGraph.River, g.edgeKind[e], "the gate " + id + " is not opened in the lanes the game shares");
            }
            Assert.AreEqual(0, LaneOpenings.Apply(g), "the shared lanes were not opened when they were read");
        }
    }
}
