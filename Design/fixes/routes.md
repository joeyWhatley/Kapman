# Routes fix (code): open four gates and the Maas mouth, nothing else

7 October 2026. Draft only: nothing under Assets was changed, Unity was not run, the code below was not compiled.
Source: `Design/research/next/routes_fix.md` (fix A, second form: re-kind once after the load).

## 1. What it does

After `lanes.bin` is read, 12 edges change kind from Pending (4) to River (3):

| Set | How found (no edge numbers in code) | Edges today |
|---|---|---|
| Gate `canche` (Quentovic) | `g.gates["canche"].edges`, those of kind Pending | 47937, 47938 |
| Gate `ribe_a` (Ripa) | same | 69283, 69341 |
| Gate `dokkumer_ee` (Doccinga) | same | 59476, 59664 |
| Gate `stour_fordwic` (Fordwic) | same | 46748, 46749, 47013 |
| Maas mouth | kind Pending, no gate flag, both ends within 3 km of 51.897 N 4.297 E | 51866, 51872, 51883 |

`Course.cs` line 64 stays as it is (`p.cost[LaneGraph.Pending] = float.PositiveInfinity;`): all other pending
rivers stay shut for the player. Flags, lengths, depths and courses are not touched.

## 2. Why River is safe for these edges (read in the code)

- `LaneGraph.cs` `Profile.Avoids` (line 196 on) and `ShallowFactor` (line 207 on) skip River just as they skip
  Pending, and skip any gate edge. So the gate depth of -1 is never read.
- `Course.LaneWater` (`Course.cs` line 163) gives infinity for River as for Pending: no tide wait on them.
- The dry flag stays: 5 of the 9 gate edges are dry, so they cost 6 times their length in the player search
  (`p.dry = 6f`, `Course.cs` line 66). Each is the only way to its port, so a way is still found.
  `km` comes from the lengths of the line (`Course.cs` lines 155 to 157), not from the cost.

## 3. Side effects to know

1. The ships of the world pay 1.1 a metre on these 12 edges, not 1.3 (`LaneGraph.cs` line 182). Their cached routes
   (`LaneRoutes.cache`) are per `LaneRoutes` object, built after the graph loads, so none is stale.
2. `Course.LegIsRiver` (`Course.cs` line 177) is now true on them: she rows there. `ChartWorld.cs` line 184
   already counted pending as river, so nothing changes on the chart.
3. `Course.Kinds` holds 3, not 4, on these legs. Anything that draws pending legs another way will draw them as river.
4. The Maas rule is by place. If the lanes are baked again and the hole is closed, it finds nothing and does nothing.
   If it finds some number other than 3, it writes a warning and still re-kinds what it found.
5. Not saved anywhere: `lanes.bin` is not written. A saved course holds points only, so old saves are fine.

## 4. The class (new file)

Put at `Assets/Kapman/Code/Game/Sim/LaneOpenings.cs` (beside `LaneGraph.cs`; plain C#, no scene).

```csharp
using System;
using Kapman.World;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE OPENED RIVERS - the few pending lanes (LaneGraph.Pending: a river that had no water in the old 3D world) the
    /// chart game sails as rivers: the gates of four ports that have no other way in, and the three gap legs at the
    /// Maas mouth that join the Lek to the sea. Run once on the lanes after they are read (SimRoutes.Graph). Every
    /// other pending river stays pending, and so shut for the player (Course.Profile).
    /// Remove a name here when gates_793.csv of the bake gives that gate kind "river"
    /// (Design/research/next/routes_fix.md, fix B).
    /// </summary>
    public static class LaneOpenings
    {
        /// <summary>Gates (ids in lanes.bin) whose pending edges become river: Quentovic, Ripa, Doccinga, Fordwic.</summary>
        public static readonly string[] Gates = { "canche", "ribe_a", "dokkumer_ee", "stour_fordwic" };

        /// <summary>The Maas mouth: the middle of the three gap legs between the carved Lek and the carved river to Witla.</summary>
        public const double MaasLat = 51.897, MaasLon = 4.297;
        /// <summary>Real metres round it within which BOTH ends of a pending, ungated edge must lie.</summary>
        public const double MaasReachM = 3000.0;
        /// <summary>How many edges the Maas rule is expected to find in the lanes of 2 Oct 2026.</summary>
        public const int MaasExpected = 3;

        /// <summary>Re-kinds the opened edges from Pending to River. Safe to call twice (the second call finds none).
        /// Returns how many edges it changed (12 on the lanes of 2 Oct 2026).</summary>
        public static int Apply(LaneGraph g)
        {
            if (g == null)
                return 0;
            // the Maas legs first, while they are still pending and so can be told from their place
            int maas = 0;
            for (int e = 0; e < g.EdgeCount; e++)
                if (IsMaasMouth(g, e))
                {
                    g.edgeKind[e] = LaneGraph.River;
                    maas++;
                }
            if (maas != 0 && maas != MaasExpected)
                Warn("lanes: the Maas mouth rule opened " + maas + " edges, expected " + MaasExpected);
            int changed = maas;
            foreach (string id in Gates)
            {
                if (!g.gates.TryGetValue(id, out var gate))
                {
                    Warn("lanes: no gate " + id + " to open");
                    continue;
                }
                foreach (int e in gate.edges)
                    if (e >= 0 && e < g.EdgeCount && g.edgeKind[e] == LaneGraph.Pending)
                    {
                        g.edgeKind[e] = LaneGraph.River;
                        changed++;
                    }
            }
            return changed;
        }

        /// <summary>Is this an edge the Maas rule takes: pending, not of a gate, both ends within MaasReachM of the mouth?
        /// (False once it has been re-kinded.)</summary>
        public static bool IsMaasMouth(LaneGraph g, int e)
        {
            if (g.edgeKind[e] != LaneGraph.Pending || (g.edgeFlags[e] & LaneGraph.FlagGate) != 0)
                return false;
            var (mx, mz) = WorldFrame.ToWorld(MaasLat, MaasLon);
            return Near(g, g.edgeA[e], mx, mz) && Near(g, g.edgeB[e], mx, mz);
        }

        static bool Near(LaneGraph g, int node, double x, double z)
        {
            double dx = g.nodeX[node] - x, dz = g.nodeZ[node] - z;
            return dx * dx + dz * dz <= MaasReachM * MaasReachM;
        }

        static void Warn(string text) => UnityEngine.Debug.LogWarning(text);
    }
}
```

Notes on the draft:
- `WorldFrame.ToWorld(double lat, double lon)` returns `(double x, double z)`:
  `Assets/Kapman/Code/Core/World/WorldFrame.cs` line 53, namespace `Kapman.World`.
- `g.gates` is `Dictionary<string, (string toll, int[] edges)>` (`LaneGraph.cs` line 34).
- `IsMaasMouth` calls `WorldFrame.ToWorld` once per edge (116,596 times, once per load). If that shows in the
  load time, work the point out once in `Apply` and pass it in.
- The 3 km reach: by the table in routes_fix.md the ends of the three edges lie at most about 1.8 km from the centre.
  NOT CHECKED against the file: that no other pending, ungated edge has both ends inside 3 km. The warning and
  the count of 12 in the test catch it.
- NOT CHECKED: whether the assembly of `Kapman.Game.Sim` sees `UnityEngine` (`Course.cs` in the same code tree does
  use `UnityEngine.Object`). If the Sim folder has its own asmdef without it, drop `Warn`.

## 5. Where to call it: one place

`Assets/Kapman/Code/Game/Sim/Travel.cs`, in `SimRoutes.Graph`, line 127. This is the only call of `LaneGraph.Load`
in the code (grep: `LaneGraph.cs` 38 is its definition, `Travel.cs` 127 the one call). Every user goes through it:
`Course.Plan` (`Course.cs` line 100), `SimRoutes.Load` (`KapmanGame.cs` 297), `ChartWorld.cs` 163, `FramePassage.cs` 119.

Now (lines 125 to 130):

```csharp
                    if (cached == null || cachedPath != lanesFile || cachedTime != time)
                    {
                        cached = LaneGraph.Load(lanesFile);
                        cachedPath = lanesFile;
                        cachedTime = time;
                    }
```

Change to:

```csharp
                    if (cached == null || cachedPath != lanesFile || cachedTime != time)
                    {
                        var loaded = LaneGraph.Load(lanesFile);
                        LaneOpenings.Apply(loaded);                      // (four gates + the Maas mouth sail as rivers)
                        cached = loaded;
                        cachedPath = lanesFile;
                        cachedTime = time;
                    }
```

(`loaded` first, then `cached`: the graph is shared once set, so it must not be seen half done.)

Do NOT put it in `LaneGraph.Load`: that file is marked "copied unchanged from the first build" and is a plain reader.

Also the comment only, `Course.cs` line 64:

```csharp
            p.cost[LaneGraph.Pending] = float.PositiveInfinity;          // (rivers not opened for the chart game: see LaneOpenings)
```

## 6. EditMode test (new file)

Put at `Assets/Kapman/Tests/EditMode/LaneOpeningsTests.cs` (`Kapman.Tests.EditMode.asmdef` is in that folder).
It uses the graph and the router with the player profile, as `Course.Plan` lines 119 to 128 do (off the flats first,
then over them), so it needs no scene and no `GameData`. Each test reads its own copy of the lanes: the shared one
is not touched. It is skipped, not failed, on a machine without `lanes.bin`.

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.World;

namespace Kapman.Tests
{
    public class LaneOpeningsTests
    {
        static LaneGraph Fresh()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
            return LaneGraph.Load(SimRoutes.DefaultLanes);
        }

        /// <summary>The way of the player between two berths as Course.Plan finds it.</summary>
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

        [Test]
        public void Opens_twelve_edges_and_no_more()
        {
            var g = Fresh();
            int pendingBefore = Count(g, LaneGraph.Pending), riverBefore = Count(g, LaneGraph.River);
            int changed = LaneOpenings.Apply(g);
            Assert.AreEqual(12, changed, "four gates (2 + 2 + 2 + 3) and three legs at the Maas mouth");
            Assert.AreEqual(pendingBefore - 12, Count(g, LaneGraph.Pending), "every other pending river stays pending");
            Assert.AreEqual(riverBefore + 12, Count(g, LaneGraph.River));
            Assert.AreEqual(0, LaneOpenings.Apply(g), "a second call finds nothing");
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
            Assert.IsFalse(Way(g, "dorestad", port, edges), port + " was shut before the fix (lanes baked again? then drop it from LaneOpenings.Gates)");
            LaneOpenings.Apply(g);
            Assert.IsTrue(Way(g, "dorestad", port, edges), "no course from dorestad to " + port);
            Assert.Greater(edges.Count, 0);
            foreach (int e in edges)
                Assert.AreNotEqual(LaneGraph.Pending, g.edgeKind[e], "the course to " + port + " uses a pending river");
        }

        [Test]
        public void Dorestad_to_walichrum_goes_by_the_lek()
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
            double before = Metres(g, edges);
            foreach (int e in edges)
                Assert.IsFalse(maas.Contains(e), "before the fix the Maas mouth is shut");

            LaneOpenings.Apply(g);
            Assert.IsTrue(Way(g, "dorestad", "walichrum", edges), "no course from dorestad to walichrum");
            int through = 0;
            foreach (int e in edges)
                if (maas.Contains(e))
                    through++;
            Assert.AreEqual(maas.Count, through, "the course runs down the Lek and out by the Maas mouth");
            double after = Metres(g, edges);
            Assert.Less(after, before, "the Lek way is the shorter one");
            Assert.AreEqual(166.0, after / 1000.0, 10.0, "about 166 km (routes_fix.md: a plain walk, no shallow rule)");
        }

        static int Count(LaneGraph g, byte kind)
        {
            int n = 0;
            for (int e = 0; e < g.EdgeCount; e++)
                if (g.edgeKind[e] == kind)
                    n++;
            return n;
        }

        static double Metres(LaneGraph g, List<int> edges)
        {
            double m = 0.0;
            foreach (int e in edges)
                m += g.edgeLength[e];
            return m;
        }
    }
}
```

## 7. Not checked (do before the commit)

1. Nothing compiled, no test run. `LaneRouter.Route(int, int, Profile, List<int>, out double)` is at `LaneGraph.cs` line 262.
2. The port ids `dorestad, quentovic, ripa, doccinga, fordwic, walichrum` as keys of `g.berths`: taken from the task,
   not read from `lanes.bin`. If the ids differ the tests fail at the "has a berth" lines.
3. The 166 km (plus or minus 10) for Walichrum comes from the research walk, which had no shallow-water rule. If the
   router of the game goes a little longer, widen it or drop that one assert; "through the three Maas legs" is the real one.
4. Whether `Kapman.Tests.EditMode.asmdef` references the assembly that holds `Kapman.Game` and `Kapman.World`.
5. The "was shut before the fix" asserts pin the lanes of today. After a new bake (fix B) they should fail: then
   remove the names from `LaneOpenings.Gates` and those asserts.
6. On the chart: look at the four gates and the Maas mouth. The courses of the gates are "course approximate" in
   `gates_793.csv`; her mark may run over drawn land (routes_fix.md section 5, point 7).
7. The second user of the player profile, `UI/Frame/FramePassage.cs` line 129, gets the fix with no change (same graph).
8. The routes_fix.md test list (section 7 there) also asks: Dorestad to Daventre still 179 km, and the nodes the
   player reaches rise from 25,354 by a few tens, not by 688. Not drafted here; worth adding once the above passes.
