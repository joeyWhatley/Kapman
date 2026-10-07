// ported 4 Oct: copied unchanged from the first build (Code/World/LaneGraph.cs). The sea lanes (Bake/lanes: lanes.bin) are
// pure data and routing, used by the simulation (Travel, WorldSim, Seamanship) and by Voyage/Course. The new runtime has no
// copy, so it lives here with the sim. If another owner brings one too, keep one and delete the other.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kapman.World
{
    /// <summary>
    /// THE SEA LANES - the routes on real water between the ports (Bake/lanes writes lanes.bin, "KLN1", layout in
    /// Bake/lanes/README.md): nodes on the open sea, along the coasts, in narrow channels, on the flats, along the rivers
    /// and at each port's berth; edges between them, each with its kind (sea, coastal, sound, river, pending river, flats,
    /// portage, approach), its length, its shallowest water and its exact course. Real metres in the world frame. Read once,
    /// shared (thread-safe once loaded); <see cref="LaneRouter"/> finds the passages.
    /// </summary>
    public sealed class LaneGraph
    {
        public const byte Sea = 0, Coastal = 1, Sound = 2, River = 3, Pending = 4, Flats = 5, Portage = 6, Approach = 7;
        public const byte FlagGate = 1, FlagUncertain = 2, FlagDry = 4;

        public int NodeCount, EdgeCount;
        public float[] nodeX, nodeZ;
        public byte[] nodeKind;
        public int[] edgeA, edgeB;
        public byte[] edgeKind, edgeFlags;
        public float[] edgeLength, edgeDepth;
        public int[] edgePoint, edgePoints;                          // first point and count in px/pz
        public float[] px, pz;
        /// <summary>Port id -> its berth node.</summary>
        public readonly Dictionary<string, int> berths = new Dictionary<string, int>();
        /// <summary>Gate id -> (toll id, its edges).</summary>
        public readonly Dictionary<string, (string toll, int[] edges)> gates = new Dictionary<string, (string, int[])>();
        // adjacency: for node i, adjEdge[adjStart[i] .. adjStart[i + 1]) are its edges, by edge index (deterministic)
        public int[] adjStart, adjEdge;

        public static LaneGraph Load(string path)
        {
            using var r = new BinaryReader(File.OpenRead(path));
            if (Encoding.ASCII.GetString(r.ReadBytes(4)) != "KLN1")
                throw new InvalidDataException(path + " is not a lanes file (KLN1)");
            int version = r.ReadInt32();
            if (version != 1)
                throw new InvalidDataException(path + ": lanes version " + version + ", expected 1");
            var g = new LaneGraph();
            int n = r.ReadInt32(), e = r.ReadInt32(), b = r.ReadInt32(), gc = r.ReadInt32(), p = r.ReadInt32();
            g.NodeCount = n;
            g.EdgeCount = e;
            g.nodeX = new float[n];
            g.nodeZ = new float[n];
            g.nodeKind = new byte[n];
            for (int i = 0; i < n; i++)
            {
                r.ReadInt32();
                g.nodeX[i] = r.ReadSingle();
                g.nodeZ[i] = r.ReadSingle();
                g.nodeKind[i] = r.ReadByte();
            }
            g.edgeA = new int[e];
            g.edgeB = new int[e];
            g.edgeKind = new byte[e];
            g.edgeFlags = new byte[e];
            g.edgeLength = new float[e];
            g.edgeDepth = new float[e];
            g.edgePoint = new int[e];
            g.edgePoints = new int[e];
            g.px = new float[p];
            g.pz = new float[p];
            int at = 0;
            for (int i = 0; i < e; i++)
            {
                g.edgeA[i] = r.ReadInt32();
                g.edgeB[i] = r.ReadInt32();
                g.edgeKind[i] = r.ReadByte();
                g.edgeFlags[i] = r.ReadByte();
                g.edgeLength[i] = r.ReadSingle();
                g.edgeDepth[i] = r.ReadSingle();
                int count = r.ReadInt32();
                g.edgePoint[i] = at;
                g.edgePoints[i] = count;
                for (int k = 0; k < count; k++)
                {
                    g.px[at] = r.ReadSingle();
                    g.pz[at] = r.ReadSingle();
                    at++;
                }
            }
            for (int i = 0; i < b; i++)
            {
                string id = Encoding.UTF8.GetString(r.ReadBytes(r.ReadByte()));
                g.berths[id] = r.ReadInt32();
            }
            for (int i = 0; i < gc; i++)
            {
                string id = Encoding.UTF8.GetString(r.ReadBytes(r.ReadByte()));
                string toll = Encoding.UTF8.GetString(r.ReadBytes(r.ReadByte()));
                int k = r.ReadInt32();
                var edges = new int[k];
                for (int j = 0; j < k; j++)
                    edges[j] = r.ReadInt32();
                g.gates[id] = (toll, edges);
            }
            g.Index();
            return g;
        }

        void Index()
        {
            var degree = new int[NodeCount + 1];
            for (int i = 0; i < EdgeCount; i++)
            {
                degree[edgeA[i]]++;
                degree[edgeB[i]]++;
            }
            adjStart = new int[NodeCount + 1];
            for (int i = 0; i < NodeCount; i++)
                adjStart[i + 1] = adjStart[i] + degree[i];
            adjEdge = new int[adjStart[NodeCount]];
            var fill = new int[NodeCount];
            for (int i = 0; i < EdgeCount; i++)                      // edges in index order: the lists are sorted
            {
                adjEdge[adjStart[edgeA[i]] + fill[edgeA[i]]++] = i;
                adjEdge[adjStart[edgeB[i]] + fill[edgeB[i]]++] = i;
            }
        }

        public int Other(int edge, int node) => edgeA[edge] == node ? edgeB[edge] : edgeA[edge];

        /// <summary>The node nearest a point (real metres) that has at least one edge a profile may use; -1 if none.</summary>
        public int Nearest(double x, double z, LaneRouter.Profile profile = null)
        {
            int best = -1;
            double bestD = double.MaxValue;
            for (int i = 0; i < NodeCount; i++)
            {
                double dx = nodeX[i] - x, dz = nodeZ[i] - z, d = dx * dx + dz * dz;
                if (d >= bestD)
                    continue;
                if (profile != null)
                {
                    bool usable = false;
                    for (int k = adjStart[i]; k < adjStart[i + 1] && !usable; k++)
                        usable = !float.IsInfinity(profile.cost[edgeKind[adjEdge[k]]]) && !profile.Avoids(this, adjEdge[k]);
                    if (!usable)
                        continue;
                }
                best = i;
                bestD = d;
            }
            return best;
        }

        /// <summary>Appends an edge's course to a list of points, travelled from `from` (skipping its first point if the list
        /// already ends there).</summary>
        public void AppendCourse(int edge, int from, List<(double x, double z)> points)
        {
            int start = edgePoint[edge], count = edgePoints[edge];
            bool forward = edgeA[edge] == from;
            for (int k = 0; k < count; k++)
            {
                int i = forward ? start + k : start + count - 1 - k;
                if (k == 0 && points.Count > 0)
                    continue;
                points.Add((px[i], pz[i]));
            }
        }
    }

    /// <summary>
    /// PASSAGES - the cheapest way between two berths over the lanes (A*, the straight line as its guide; ties to the lower
    /// node, so the same question always gets the same route). What a ship may use, and how much she dislikes it, is her
    /// profile: a sea-going trader keeps off hauls, a river barge off the open sea, everyone dislikes the flats and
    /// the dry (uncertain) legs. Real metres.
    /// </summary>
    public sealed class LaneRouter
    {
        /// <summary>Cost per real metre by edge kind (sea, coastal, sound, river, pending, flats, portage, approach);
        /// infinity: not allowed.</summary>
        public sealed class Profile
        {
            public float[] cost = { 1f, 1f, 1.05f, 1.1f, 1.3f, 1.5f, 4f, 1f };
            public float dry = 2f;
            /// <summary>Real metres of water she'd rather keep to (0: no rule). A lane over the sea, the coast, a sound, the
            /// flats or a berth's approach whose shallowest water (sampled when the lanes were baked) is less costs more,
            /// up to <see cref="shallow"/> times as much where it dries: the course goes round shallows wherever there's a
            /// way, and over them only where there's none. Rivers (cut to depth) and gates (unmeasured) don't count.</summary>
            public float minDepth;
            public float shallow = 1f;
            /// <summary>Real metres of water below which such a lane isn't taken at all (0: no rule). Rivers and gates don't
            /// count, as for <see cref="minDepth"/>.</summary>
            public float avoidBelow;
            public string key = "default";

            /// <summary>Is a lane too shallow for this profile to take at all (<see cref="avoidBelow"/>)?</summary>
            public bool Avoids(LaneGraph g, int edge)
            {
                if (avoidBelow <= 0f || (g.edgeFlags[edge] & LaneGraph.FlagGate) != 0)
                    return false;
                byte kind = g.edgeKind[edge];
                if (kind == LaneGraph.River || kind == LaneGraph.Pending || kind == LaneGraph.Portage)
                    return false;
                return g.edgeDepth[edge] < avoidBelow;
            }

            /// <summary>How much more a lane costs for its shallowest water (1: no more).</summary>
            public float ShallowFactor(LaneGraph g, int edge)
            {
                if (minDepth <= 0f || shallow <= 1f || (g.edgeFlags[edge] & LaneGraph.FlagGate) != 0)
                    return 1f;
                byte kind = g.edgeKind[edge];
                if (kind == LaneGraph.River || kind == LaneGraph.Pending || kind == LaneGraph.Portage)
                    return 1f;
                float depth = g.edgeDepth[edge];
                if (depth >= minDepth)
                    return 1f;
                return 1f + (shallow - 1f) * Math.Min(1f, (minDepth - depth) / minDepth);
            }

            public static Profile For(bool sea, bool coast, bool river, bool small)
            {
                var p = new Profile();
                if (!sea)
                {
                    p.cost[LaneGraph.Sea] = float.PositiveInfinity;
                    if (!coast)
                        p.cost[LaneGraph.Coastal] = p.cost[LaneGraph.Flats] = 8f;                 // river craft: only short hops along a coast
                }
                if (!river)
                    p.cost[LaneGraph.River] = p.cost[LaneGraph.Pending] = float.PositiveInfinity;
                if (!small)
                    p.cost[LaneGraph.Portage] = 12f;                                    // a big ship is rarely hauled
                p.key = (sea ? "S" : "s") + (coast ? "C" : "c") + (river ? "R" : "r") + (small ? "M" : "m");
                return p;
            }
        }

        readonly LaneGraph g;
        // the search's working arrays, reused (one router per thread)
        readonly double[] best;
        readonly int[] via, stamp;
        int stampNow;

        public LaneRouter(LaneGraph graph)
        {
            g = graph;
            best = new double[g.NodeCount];
            via = new int[g.NodeCount];
            stamp = new int[g.NodeCount];
        }

        /// <summary>The route between two ports' berths, as edges (in order) and its length (real m); false if none.</summary>
        public bool Route(string fromPort, string toPort, Profile profile, List<int> edges, out double metres)
        {
            edges.Clear();
            metres = 0.0;
            if (!g.berths.TryGetValue(fromPort, out int a) || !g.berths.TryGetValue(toPort, out int b))
                return false;
            return Route(a, b, profile, edges, out metres);
        }

        public bool Route(int from, int to, Profile profile, List<int> edges, out double metres)
        {
            edges.Clear();
            metres = 0.0;
            if (from == to)
                return true;
            stampNow++;
            var open = new SortedSet<(double f, int node)>();
            Touch(from);
            best[from] = 0.0;
            open.Add((Guess(from, to), from));
            while (open.Count > 0)
            {
                var top = open.Min;
                open.Remove(top);
                int n = top.node;
                if (n == to)
                    break;
                double here = best[n];
                for (int k = g.adjStart[n]; k < g.adjStart[n + 1]; k++)
                {
                    int e = g.adjEdge[k];
                    float per = profile.cost[g.edgeKind[e]];
                    if (float.IsInfinity(per) || profile.Avoids(g, e))
                        continue;
                    if ((g.edgeFlags[e] & LaneGraph.FlagDry) != 0)
                        per *= profile.dry;
                    per *= profile.ShallowFactor(g, e);
                    int m = g.Other(e, n);
                    double cost = here + g.edgeLength[e] * per;
                    if (stamp[m] == stampNow && cost >= best[m])
                        continue;
                    if (stamp[m] == stampNow)
                        open.Remove((best[m] + Guess(m, to), m));
                    Touch(m);
                    best[m] = cost;
                    via[m] = e;
                    open.Add((cost + Guess(m, to), m));
                }
            }
            if (stamp[to] != stampNow)
                return false;
            for (int n = to; n != from;)
            {
                int e = via[n];
                edges.Add(e);
                metres += g.edgeLength[e];
                n = g.Other(e, n);
            }
            edges.Reverse();
            return true;
        }

        void Touch(int n)
        {
            if (stamp[n] != stampNow)
            {
                stamp[n] = stampNow;
                best[n] = double.MaxValue;
                via[n] = -1;
            }
        }

        double Guess(int a, int b)
        {
            double dx = g.nodeX[a] - g.nodeX[b], dz = g.nodeZ[a] - g.nodeZ[b];
            return Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>The route's course as points (real metres), from the first port's berth to the second's.</summary>
        public void Course(int from, List<int> edges, List<(double x, double z)> points)
        {
            points.Clear();
            int n = from;
            foreach (int e in edges)
            {
                g.AppendCourse(e, n, points);
                n = g.Other(e, n);
            }
        }
    }
}
