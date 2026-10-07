using Kapman.World;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE OPENED RIVERS (7 Oct 2026) - the few pending lanes (LaneGraph.Pending: a river that had no water in the old 3D
    /// world) the chart game sails as rivers: the gates of four ports that have no other way in (Quentovic, Ripa,
    /// Doccinga, Fordwic), and the three gap legs at the Maas mouth that join the Lek to the sea. Run once on the lanes
    /// after they are read (SimRoutes.Graph). Every other pending river stays pending, and so shut for the player
    /// (Course.Profile): opening them all would open 31,556 km of rivers at once.
    /// Take a name out of here when gates_793.csv of the bake gives that gate the kind "river"
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
        /// <summary>How many edges the Maas rule finds in the lanes of 2 Oct 2026.</summary>
        public const int MaasExpected = 3;

        /// <summary>Re-kinds the opened edges from Pending to River. Safe to call twice (the second call finds none).
        /// Returns how many edges it changed (12 on the lanes of 2 Oct 2026).</summary>
        public static int Apply(LaneGraph g)
        {
            if (g == null || g.edgeKind == null)
                return 0;
            // the Maas legs first, while they are still pending and so can be told by their place
            var (mx, mz) = WorldFrame.ToWorld(MaasLat, MaasLon);
            int maas = 0;
            for (int e = 0; e < g.EdgeCount; e++)
                if (IsMaasMouth(g, e, mx, mz))
                {
                    g.edgeKind[e] = LaneGraph.River;
                    maas++;
                }
            if (maas != 0 && maas != MaasExpected)
                UnityEngine.Debug.LogWarning("Kapman lanes: the Maas mouth rule opened " + maas + " edges, expected " + MaasExpected + ".");
            int changed = maas;
            foreach (string id in Gates)
            {
                if (!g.gates.TryGetValue(id, out var gate) || gate.edges == null)
                {
                    UnityEngine.Debug.LogWarning("Kapman lanes: no gate '" + id + "' to open (the lanes were baked again?).");
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
            var (mx, mz) = WorldFrame.ToWorld(MaasLat, MaasLon);
            return IsMaasMouth(g, e, mx, mz);
        }

        static bool IsMaasMouth(LaneGraph g, int e, double mx, double mz)
        {
            if (g.edgeKind[e] != LaneGraph.Pending || (g.edgeFlags[e] & LaneGraph.FlagGate) != 0)
                return false;
            return Near(g, g.edgeA[e], mx, mz) && Near(g, g.edgeB[e], mx, mz);
        }

        static bool Near(LaneGraph g, int node, double x, double z)
        {
            double dx = g.nodeX[node] - x, dz = g.nodeZ[node] - z;
            return dx * dx + dz * dz <= MaasReachM * MaasReachM;
        }
    }
}
