using System;
using System.Collections.Generic;
using Kapman.World;

namespace Kapman.Game.Sim
{
    /// <summary>How ships get from port to port: a passage of points in real metres, and its length.</summary>
    public interface IRoutes
    {
        /// <summary>Fills `path` with the passage from one port to another for a kind of craft (real metres), and `kinds`
        /// (if given) with each leg's kind of water (LaneGraph.Sea ... Approach); false if there's none she can take.</summary>
        bool Passage(int from, int to, ShipClassDef cls, List<WorldXZ> path, List<byte> kinds, out double km);
    }

    /// <summary>Until the sea lanes are baked: straight from berth to berth (they cross land; a stand-in only).</summary>
    public sealed class StraightRoutes : IRoutes
    {
        public bool Passage(int from, int to, ShipClassDef cls, List<WorldXZ> path, List<byte> kinds, out double km)
        {
            path.Clear();
            kinds?.Clear();
            var a = GameData.Ports[from];
            var b = GameData.Ports[to];
            var (ax, az) = WorldFrame.ToWorld(a.lat, a.lon);
            var (bx, bz) = WorldFrame.ToWorld(b.lat, b.lon);
            path.Add(new WorldXZ(ax, az));
            path.Add(new WorldXZ(bx, bz));
            kinds?.Add(LaneGraph.Sea);
            km = WorldXZ.Distance(path[0], path[1]) / 1000.0 * 1.25;      // (a sailing passage is longer than the line)
            return true;
        }
    }

    /// <summary>
    /// PASSAGES ON THE SEA LANES (Bake/lanes: lanes.bin) - the real routes on water: down the rivers, along the coasts,
    /// through the narrows, over the flats, across the open sea; each kind of craft by what she can take (a river barge
    /// keeps off the open sea; a sea-going ship isn't hauled overland). Routes are kept once found (per kind of craft).
    /// </summary>
    public sealed class LaneRoutes : IRoutes
    {
        readonly LaneGraph graph;
        readonly LaneRouter router;
        readonly Dictionary<string, (List<WorldXZ> path, List<byte> kinds, double km)> cache =
            new Dictionary<string, (List<WorldXZ>, List<byte>, double)>();
        readonly List<int> edges = new List<int>();
        readonly List<(double x, double z)> points = new List<(double, double)>();

        public LaneRoutes(LaneGraph graph)
        {
            this.graph = graph;
            router = new LaneRouter(graph);
        }

        public LaneGraph Graph => graph;

        public static LaneRouter.Profile ProfileOf(ShipClassDef c) =>
            LaneRouter.Profile.For(sea: c.sea, coast: c.coast || c.sea, river: c.river, small: c.length <= 12f);

        public bool Passage(int from, int to, ShipClassDef cls, List<WorldXZ> path, List<byte> kinds, out double km)
        {
            path.Clear();
            kinds?.Clear();
            km = 0.0;
            var profile = ProfileOf(cls);
            string a = GameData.Ports[from].id, b = GameData.Ports[to].id, key = a + ">" + b + ":" + profile.key;
            if (!cache.TryGetValue(key, out var found))
            {
                if (!graph.berths.TryGetValue(a, out int na) || !router.Route(a, b, profile, edges, out double metres))
                    found = (null, null, -1.0);
                else
                {
                    // the course, edge by edge, each leg marked with its edge's kind of water
                    var list = new List<WorldXZ>();
                    var legKinds = new List<byte>();
                    int n = na;
                    foreach (int e in edges)
                    {
                        points.Clear();
                        graph.AppendCourse(e, n, points);
                        for (int k = 0; k < points.Count; k++)
                        {
                            if (k == 0 && list.Count > 0)
                                continue;                                       // (the joint is already there)
                            list.Add(new WorldXZ(points[k].x, points[k].z));
                            if (list.Count > 1)
                                legKinds.Add(graph.edgeKind[e]);
                        }
                        n = graph.Other(e, n);
                    }
                    found = (list, legKinds, metres / 1000.0);
                }
                cache[key] = found;
            }
            if (found.path == null || found.path.Count < 2)
                return false;
            path.AddRange(found.path);
            kinds?.AddRange(found.kinds);
            km = found.km;
            return true;
        }
    }

    /// <summary>The routes the world's ships take: the sea lanes when they're baked, else straight lines (a stand-in).</summary>
    public static class SimRoutes
    {
        public const string DefaultLanes = "G:/kapman_unity/data/baked/world/lanes.bin";
        static LaneGraph cached;
        static string cachedPath;
        static System.DateTime cachedTime;
        static LaneRoutes shared;

        /// <summary>The routes on the lanes. One set for every world made on the same lanes: a route once found is kept
        /// (it depends on the lanes and the kind of craft only), so a second world, a new game, a test's next run do not
        /// search for it again. (7 Oct 2026: each world had its own, and finding them all again cost about seven
        /// seconds a world.)</summary>
        public static IRoutes Load(string lanesFile)
        {
            var g = Graph(lanesFile);
            if (g == null)
                return new StraightRoutes();
            if (shared == null || shared.Graph != g)
                shared = new LaneRoutes(g);
            return shared;
        }

        /// <summary>The sea lanes (read once, again when the file changes), or null if they're not baked.</summary>
        public static LaneGraph Graph(string lanesFile = DefaultLanes)
        {
            try
            {
                if (!string.IsNullOrEmpty(lanesFile) && System.IO.File.Exists(lanesFile))
                {
                    var time = System.IO.File.GetLastWriteTimeUtc(lanesFile);
                    if (cached == null || cachedPath != lanesFile || cachedTime != time)
                    {
                        // (loaded whole, then the few rivers the chart game sails are opened, then shared: LaneOpenings)
                        var loaded = LaneGraph.Load(lanesFile);
                        LaneOpenings.Apply(loaded);
                        cached = loaded;
                        cachedPath = lanesFile;
                        cachedTime = time;
                    }
                    return cached;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning("Kapman: the sea lanes couldn't be read (" + ex.Message + "); ships sail straight lines.");
            }
            return null;
        }
    }
    /// <summary>
    /// HOW FAST A SHIP GOES this hour, from the wind where she is (the weather model, the same the player sails in) and her
    /// class: her polar (the Gislinge boat's curve times her class's factor), beating against the wind at about 2 knots
    /// made good, rowing in a calm when she has oars, and sheltering in a gale. Real m/s.
    /// </summary>
    public static class Travel
    {
        /// <summary>The share of her beam-reach speed at a true wind angle (degrees off the wind): the Gislinge curve's shape.</summary>
        public static double Polar(double twa, double closest)
        {
            twa = Math.Abs(twa);
            if (twa < closest)
                return 0.0;
            if (twa < 90.0)
                return 0.72 + 0.28 * (twa - closest) / Math.Max(1.0, 90.0 - closest);
            if (twa < 135.0)
                return 1.0 + 0.12 * (twa - 90.0) / 45.0;                   // a broad reach is her best
            return 1.12 - 0.3 * (twa - 135.0) / 45.0;                      // dead before the wind: less
        }

        /// <summary>Her speed made good along her course (m/s), and whether she's sheltering from a gale.</summary>
        public static double Speed(ShipClassDef c, double courseDeg, double windFrom, double windSpeed, out bool sheltering)
        {
            sheltering = windSpeed > 17.0;                                // a gale: she runs for shelter or heaves to
            if (sheltering)
                return 0.0;
            double sail = 0.0;
            if (c.hasSail && windSpeed > 1.5)
            {
                double twa = Math.Abs(((courseDeg - windFrom) % 360.0 + 540.0) % 360.0 - 180.0);   // 0 = into the wind
                double strength = Math.Pow(windSpeed / 8.0, 0.6);
                double through = c.sailKn * c.polar * strength;
                if (twa >= c.closestToWind)
                    sail = through * Polar(twa, c.closestToWind);
                else                                                      // beating: tacking at her closest, made good along the course
                    sail = through * Polar(c.closestToWind, c.closestToWind) * Math.Cos(c.closestToWind * Math.PI / 180.0);
                sail = Math.Min(sail, c.maxKn);
            }
            // oars: in a calm, or when rowing beats tacking (the crew rests some hours: a day's mean)
            double oars = c.oarsKn * Math.Min(1.0, c.rowingHours / 12.0);
            return Math.Max(sail, oars) * SimUnits.KnotMs;
        }

        /// <summary>The compass bearing (degrees) from one point to another (x east, z north).</summary>
        public static double Bearing(WorldXZ a, WorldXZ b) => (Math.Atan2(b.x - a.x, b.z - a.z) * 180.0 / Math.PI + 360.0) % 360.0;
    }
}