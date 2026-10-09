using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHARTED COURSE - where the merchant has chosen to sail, and the line his ship follows to get there (the user's
    /// rule: no steering; she follows her charted line). Set in the map by picking a port. The line goes by the sea lanes
    /// (Bake/lanes) the way a boat like hers can go in the 3D world now - along the coasts, over the sea, up the rivers
    /// that have their water, never up a river that hasn't yet nor over a haul - and ends at the port's own berth.
    /// Points are real metres in the world frame. The voyage planner (VoyagePlanner) hands her helmsman one point at
    /// a time.
    /// ported 4 Oct: unchanged. The line, its water, its kinds, its km and days are the chart's business and stay in the
    /// world frame (WorldFrame; the lanes.bin of Bake/lanes, read by SimRoutes through Kapman.World.LaneGraph, which the
    /// rebuild must carry over as it was: it's plain C#). The one place this touches a scene is BerthOf (a placed port's
    /// 3D berth -> world metres through FloatingOrigin): the 3D hand-off, as in VoyagePlanner.
    /// </summary>
    public static class Course
    {
        /// <summary>The port she's bound for (its id), or null: no course.</summary>
        public static string Destination { get; private set; }
        public static bool IsSet => Destination != null;
        /// <summary>The line, from where she was when it was charted to the berth.</summary>
        public static readonly List<WorldXZ> Points = new List<WorldXZ>();
        /// <summary>For each point of the line, the shallowest water (real metres under mean sea level) of the lane the
        /// leg ENDING there runs over; infinity where the lane isn't judged by its water (a river cut to depth, a gate),
        /// for the first point (no leg) and the last (the berth). The flats show as legs with little or none: the
        /// voyage waits for the tide before them (TideWait).</summary>
        public static readonly List<float> Depths = new List<float>();
        /// <summary>For each point of the line, the kind of lane (LaneGraph.Sea, River...) the leg ending there runs over;
        /// Sea for the first and last. On a river she rows along the channel (the user, 2026-10-03: under sail "she keeps
        /// looping out banging into river beds either side").</summary>
        public static readonly List<byte> Kinds = new List<byte>();
        /// <summary>The next point she's making for.</summary>
        public static int Next { get; private set; }
        /// <summary>Real km of the whole line.</summary>
        public static double Km { get; private set; }
        /// <summary>Raised when a course is set, cleared or reached.</summary>
        public static event Action Changed;

        /// <summary>
        /// A COURSE TO A PLACE POINTED AT ON THE CHART (the user, 9 Oct 2026, by the picker: "Click and sail ... Click any
        /// water on the chart, also in the dark, and she sails there by water"). Its end is not a port: Destination is
        /// <see cref="PlaceId"/> and <see cref="Place"/> is what was pointed at. She goes by the lanes to their own point
        /// nearest the place, and on to the place itself where open water lies between (the world bake says); else she
        /// lies to at the lanes' nearest point: as near the place as the water lets her.
        /// </summary>
        public const string PlaceId = "@place";
        /// <summary>The place last pointed at (kept when the course is given up, so a course laid by for a wait or a
        /// lying-to is taken up again: <see cref="SetTo"/> with <see cref="PlaceId"/>).</summary>
        public static WorldXZ Place { get; private set; }
        public static bool ToPlace => Destination == PlaceId;
        /// <summary>What a place pointed at is called. PLACEHOLDER WORDS, the user's to write.</summary>
        public const string PlaceWords = "the place you pointed at";
        /// <summary>Where she is bound, in words ("" with no course): a port's name, or the words for a place pointed at.</summary>
        public static string BoundName => !IsSet ? "" : ToPlace ? PlaceWords : GameData.Port(Destination)?.name ?? Destination;
        /// <summary>Real metres beyond the lanes' nearest point that she will stand on to a place over open water (the
        /// lanes leave no open sea farther than some 32 km from one of them).</summary>
        public const double BeyondLanesM = 45000.0;

        /// <summary>Knots she makes good on average over a passage (for the days a course will take).</summary>
        public const double PlanKnots = 3.5;

        /// <summary>Real metres of water the charted course keeps to where it can (the lanes' depths were sampled on 120 m
        /// cells, so a bank's crest can be shallower than its lane says: well over what she draws).</summary>
        public const float DeepWaterM = 2f;
        /// <summary>How many times more a lane over water that dries costs her course (less for water a little short of
        /// <see cref="DeepWaterM"/>): she goes round a bank unless that's this many times further.</summary>
        public const float ShallowCost = 8f;

        /// <summary>Real metres of water a lane (over the sea, the coast, a sound, the flats, an approach) must have for the
        /// course to take it at all, when there's a way that keeps to such water (the user, 2026-10-03: "Courses avoid
        /// flats": they dry at low tide and she grounds there).</summary>
        public const float KeepOffBelowM = 1.5f;

        /// <summary>The lanes a ship like hers can use in the 3D world now: no unbuilt rivers, no hauls, wary of the dry,
        /// and keeping to deep water where there's a way (the user, 2026-10-02: the course should keep off the sands).
        /// `offFlats`: no flats lanes and nothing shallower than <see cref="KeepOffBelowM"/> at all (tried first).</summary>
        public static LaneRouter.Profile Profile(bool offFlats = false)
        {
            var p = LaneRouter.Profile.For(sea: true, coast: true, river: true, small: true);
            p.cost[LaneGraph.Pending] = float.PositiveInfinity;          // (rivers not opened for the chart game: see LaneOpenings)
            p.cost[LaneGraph.Portage] = float.PositiveInfinity;
            p.dry = 6f;
            p.minDepth = DeepWaterM;
            p.shallow = ShallowCost;
            p.key = "player";
            if (offFlats)
            {
                p.cost[LaneGraph.Flats] = float.PositiveInfinity;
                p.avoidBelow = KeepOffBelowM;
                p.key = "player-offflats";
            }
            return p;
        }

        /// <summary>Works out a course from a point (real m) to a port without setting it: its line and real km.
        /// `fromPort` (if she lies at one) starts it at that port's berth. False if there's no way there by water.</summary>
        public static bool Plan(string portId, WorldXZ from, string fromPort, List<WorldXZ> line, out double km) =>
            Plan(portId, from, fromPort, line, null, out km);

        /// <summary>As <see cref="Plan(string, WorldXZ, string, List{WorldXZ}, out double)"/>, and the water of each leg
        /// (as <see cref="Depths"/>) if `depths` is given.</summary>
        public static bool Plan(string portId, WorldXZ from, string fromPort, List<WorldXZ> line, List<float> depths, out double km) =>
            Plan(portId, from, fromPort, line, depths, null, out km);

        /// <summary>As above, and the kind of lane of each leg (as <see cref="Kinds"/>) if `kinds` is given.</summary>
        public static bool Plan(string portId, WorldXZ from, string fromPort, List<WorldXZ> line, List<float> depths, List<byte> kinds, out double km)
        {
            line.Clear();
            depths?.Clear();
            kinds?.Clear();
            km = 0.0;
            var def = GameData.Port(portId);
            if (def == null)
                return false;
            var end = BerthOf(def);
            var g = SimRoutes.Graph();
            line.Add(from);
            depths?.Add(float.PositiveInfinity);
            kinds?.Add(LaneGraph.Sea);
            if (g == null)
            {
                line.Add(end);                                           // (no lanes baked: straight there)
                depths?.Add(float.PositiveInfinity);
                kinds?.Add(LaneGraph.Sea);
                km = WorldXZ.Distance(from, end) / 1000.0;
                return true;
            }
            if (!g.berths.TryGetValue(portId, out int b))
                return false;
            var router = new LaneRouter(g);
            var edges = new List<int>();
            // off the flats where there's a way; over them only where there's none
            int a = -1;
            bool found = false;
            foreach (bool offFlats in new[] { true, false })
            {
                var profile = Profile(offFlats);
                a = fromPort != null && g.berths.TryGetValue(fromPort, out int fb) ? fb : g.Nearest(from.x, from.z, profile);
                if (a >= 0 && router.Route(a, b, profile, edges, out _))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
            var pts = new List<(double x, double z)>();
            int n = a;
            foreach (int e in edges)                                     // (as LaneRouter.Course, with each leg's water)
            {
                int before = pts.Count;
                g.AppendCourse(e, n, pts);
                n = g.Other(e, n);
                for (int k = before; k < pts.Count; k++)
                {
                    depths?.Add(LaneWater(g, e));
                    kinds?.Add(g.edgeKind[e]);
                }
            }
            if (pts.Count == 0)
            {
                pts.Add((g.nodeX[a], g.nodeZ[a]));
                depths?.Add(float.PositiveInfinity);
                kinds?.Add(LaneGraph.Sea);
            }
            foreach (var (x, z) in pts)
                line.Add(new WorldXZ(x, z));
            line.Add(end);
            depths?.Add(float.PositiveInfinity);
            kinds?.Add(LaneGraph.Sea);
            km = 0.0;
            for (int i = 0; i + 1 < line.Count; i++)
                km += WorldXZ.Distance(line[i], line[i + 1]) / 1000.0;
            return true;
        }

        /// <summary>The water a lane is judged by (as <see cref="Depths"/>): its shallowest, or infinity where it isn't
        /// judged (a river cut to depth, a pending river, a haul, a gate).</summary>
        public static float LaneWater(LaneGraph g, int edge)
        {
            if ((g.edgeFlags[edge] & LaneGraph.FlagGate) != 0)
                return float.PositiveInfinity;
            byte kind = g.edgeKind[edge];
            if (kind == LaneGraph.River || kind == LaneGraph.Pending || kind == LaneGraph.Portage)
                return float.PositiveInfinity;
            return g.edgeDepth[edge];
        }

        /// <summary>The water of the leg ending at a point of the line (<see cref="Depths"/>); infinity if unknown.</summary>
        public static float LegWater(int point) => point >= 0 && point < Depths.Count ? Depths[point] : float.PositiveInfinity;

        /// <summary>Is the leg ending at a point of the line a river lane (she rows it)?</summary>
        public static bool LegIsRiver(int point) => point >= 0 && point < Kinds.Count && Kinds[point] == LaneGraph.River;

        /// <summary>Charts a course to a port from where she is; false (and the old course kept) if there's no way.</summary>
        public static bool SetTo(string portId, WorldXZ from, string fromPort)
        {
            if (portId == PlaceId)
                return SetToPlace(Place, from, fromPort);              // (a course to a place, laid by and taken up again)
            var line = new List<WorldXZ>();
            var water = new List<float>();
            var kinds = new List<byte>();
            if (!Plan(portId, from, fromPort, line, water, kinds, out double km))
                return false;
            Points.Clear();
            Points.AddRange(line);
            Depths.Clear();
            Depths.AddRange(water);
            Kinds.Clear();
            Kinds.AddRange(kinds);
            Destination = portId;
            Next = 1;
            Km = km;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Charts a course to a place pointed at; false (and the old course kept) if no water she can sail leads
        /// towards it.</summary>
        public static bool SetToPlace(WorldXZ place, WorldXZ from, string fromPort)
        {
            var line = new List<WorldXZ>();
            var water = new List<float>();
            var kinds = new List<byte>();
            if (!PlanToPlace(place, from, fromPort, line, water, kinds, out double km))
                return false;
            Points.Clear();
            Points.AddRange(line);
            Depths.Clear();
            Depths.AddRange(water);
            Kinds.Clear();
            Kinds.AddRange(kinds);
            Destination = PlaceId;
            Place = place;
            Next = 1;
            Km = km;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Works out a course to a place that is not a port, without setting it. From her berth if she lies at a port
        /// (`fromPort`), else from the lanes' own point nearest her (not their nearest crossing: at sea that can lie
        /// thirty km astern, and a short hop would begin by sailing back to it); along the lanes to their point nearest
        /// the place; and on to the place itself where open water lies between. False if the lanes do not join the two.
        /// </summary>
        public static bool PlanToPlace(WorldXZ place, WorldXZ from, string fromPort, List<WorldXZ> line, List<float> depths, List<byte> kinds, out double km)
        {
            line.Clear();
            depths?.Clear();
            kinds?.Clear();
            km = 0.0;
            var g = SimRoutes.Graph();
            line.Add(from);
            depths?.Add(float.PositiveInfinity);
            kinds?.Add(LaneGraph.Sea);
            if (g == null)
            {
                line.Add(place);                                         // (no lanes baked: straight there)
                depths?.Add(float.PositiveInfinity);
                kinds?.Add(LaneGraph.Sea);
                km = WorldXZ.Distance(from, place) / 1000.0;
                return true;
            }
            var router = new LaneRouter(g);
            foreach (bool offFlats in new[] { true, false })
            {
                var profile = Profile(offFlats);
                // where it begins: her berth, or the lanes' own point nearest her
                int berth = -1, startLane = -1, startAt = -1;
                if (fromPort != null && g.berths.TryGetValue(fromPort, out int fb))
                    berth = fb;
                else if (!NearestOnLanes(g, profile, from, null, out startLane, out startAt))
                    continue;
                // where the way ends: the point nearest the place of the lanes SHE CAN COME TO (a pond's or a brook's own
                // lane may lie nearer and join nothing: found by the first test of a place on dry land, where the
                // nearest water was a river of the heath that no boat reaches)
                var reached = Reach(g, profile, berth >= 0 ? berth : g.edgeA[startLane]);
                if (!NearestOnLanes(g, profile, place, reached, out int endLane, out int endAt))
                    continue;
                var pts = new List<(double x, double z)>();
                var ptWater = new List<float>();
                var ptKind = new List<byte>();
                void Lane(int lane, int a, int b)                        // (along one lane from its point a to its point b)
                {
                    int step = b >= a ? 1 : -1;
                    for (int i = a; ; i += step)
                    {
                        if (pts.Count == 0 || pts[pts.Count - 1].x != g.px[i] || pts[pts.Count - 1].z != g.pz[i])
                        {
                            pts.Add((g.px[i], g.pz[i]));
                            ptWater.Add(LaneWater(g, lane));
                            ptKind.Add(g.edgeKind[lane]);
                        }
                        if (i == b)
                            break;
                    }
                }
                int First(int lane, int node) => g.edgeA[lane] == node ? g.edgePoint[lane] : g.edgePoint[lane] + g.edgePoints[lane] - 1;
                if (berth < 0 && startLane == endLane)
                    Lane(startLane, startAt, endAt);                     // (the same lane: along it, never round by its end)
                else
                {
                    // by which ends of the two lanes: the shortest of the ways
                    double least = double.MaxValue;
                    List<int> way = null;
                    int outBy = -1, inBy = -1;
                    var starts = berth >= 0 ? new[] { berth } : new[] { g.edgeA[startLane], g.edgeB[startLane] };
                    foreach (int a in starts)
                        foreach (int b in new[] { g.edgeA[endLane], g.edgeB[endLane] })
                        {
                            var edges = new List<int>();
                            double m = 0.0;
                            if (a != b && !router.Route(a, b, profile, edges, out m))
                                continue;
                            double total = m + (berth >= 0 ? 0.0 : Along(g, startAt, First(startLane, a))) + Along(g, First(endLane, b), endAt);
                            if (total < least)
                            {
                                least = total;
                                way = edges;
                                outBy = a;
                                inBy = b;
                            }
                        }
                    if (way == null)
                        continue;
                    if (berth < 0)
                        Lane(startLane, startAt, First(startLane, outBy));
                    int n = outBy;
                    foreach (int e in way)
                    {
                        int before = pts.Count;
                        g.AppendCourse(e, n, pts);
                        n = g.Other(e, n);
                        for (int k = before; k < pts.Count; k++)
                        {
                            ptWater.Add(LaneWater(g, e));
                            ptKind.Add(g.edgeKind[e]);
                        }
                    }
                    Lane(endLane, First(endLane, inBy), endAt);
                }
                for (int k = 0; k < pts.Count; k++)
                {
                    line.Add(new WorldXZ(pts[k].x, pts[k].z));
                    depths?.Add(ptWater[k]);
                    kinds?.Add(ptKind[k]);
                }
                // and on to the place itself, where open water lies between the lanes and it
                var last = line[line.Count - 1];
                double off = WorldXZ.Distance(last, place);
                if (off > 50.0 && off <= BeyondLanesM && OpenWater(last, place))
                {
                    line.Add(place);
                    depths?.Add(float.PositiveInfinity);
                    kinds?.Add(LaneGraph.Sea);
                }
                for (int i = 0; i + 1 < line.Count; i++)
                    km += WorldXZ.Distance(line[i], line[i + 1]) / 1000.0;
                return true;
            }
            return false;
        }

        /// <summary>The crossings of the lanes that can be come to from one of them, by the lanes a profile may use.</summary>
        static bool[] Reach(LaneGraph g, LaneRouter.Profile profile, int from)
        {
            var reached = new bool[g.NodeCount];
            var open = new Stack<int>();
            reached[from] = true;
            open.Push(from);
            while (open.Count > 0)
            {
                int u = open.Pop();
                for (int k = g.adjStart[u]; k < g.adjStart[u + 1]; k++)
                {
                    int e = g.adjEdge[k];
                    if (float.IsInfinity(profile.cost[g.edgeKind[e]]) || profile.Avoids(g, e))
                        continue;
                    int v = g.Other(e, u);
                    if (!reached[v])
                    {
                        reached[v] = true;
                        open.Push(v);
                    }
                }
            }
            return reached;
        }

        /// <summary>The point of the lanes nearest a place, of the lanes a profile may use (and, with `reached`, only of
        /// those that join the crossings marked there): the lane, and the point's index among all the lanes' points.
        /// False if there is none.</summary>
        static bool NearestOnLanes(LaneGraph g, LaneRouter.Profile profile, WorldXZ place, bool[] reached, out int lane, out int at)
        {
            lane = at = -1;
            double nearest = double.MaxValue;
            for (int e = 0; e < g.EdgeCount; e++)
            {
                if (float.IsInfinity(profile.cost[g.edgeKind[e]]) || profile.Avoids(g, e))
                    continue;
                if (reached != null && !reached[g.edgeA[e]])
                    continue;
                for (int k = g.edgePoint[e], end = k + g.edgePoints[e]; k < end; k++)
                {
                    double dx = g.px[k] - place.x, dz = g.pz[k] - place.z, d = dx * dx + dz * dz;
                    if (d < nearest)
                    {
                        nearest = d;
                        lane = e;
                        at = k;
                    }
                }
            }
            return lane >= 0;
        }

        /// <summary>Real metres along a lane between two of its points (their indices among all the lanes' points).</summary>
        static double Along(LaneGraph g, int a, int b)
        {
            double m = 0.0;
            for (int i = Math.Min(a, b); i < Math.Max(a, b); i++)
            {
                double dx = g.px[i + 1] - g.px[i], dz = g.pz[i + 1] - g.pz[i];
                m += Math.Sqrt(dx * dx + dz * dz);
            }
            return m;
        }

        static bool[] waterSurface;
        static MapLand waterOf;

        /// <summary>Is the straight water between two places open: every 300 m of it sea or inland water, by the world
        /// bake (MapLand)? False where the bake is not there to ask: then she keeps to the lanes.</summary>
        public static bool OpenWater(WorldXZ a, WorldXZ b)
        {
            MapLand land;
            try
            {
                land = MapLand.Shared();
            }
            catch (Exception)
            {
                land = null;
            }
            if (land == null)
                return false;
            if (waterSurface == null || !ReferenceEquals(waterOf, land))
            {
                waterSurface = new bool[256];
                for (int i = 0; i < 256; i++)
                {
                    string name = land.SurfaceNames[i] ?? "";
                    waterSurface[i] = name == "Sea" || name == "Inland water";
                }
                waterOf = land;
            }
            int steps = Math.Max(1, (int)Math.Ceiling(WorldXZ.Distance(a, b) / 300.0));
            try
            {
                for (int i = 0; i <= steps; i++)
                {
                    double k = (double)i / steps;
                    if (!waterSurface[land.SurfaceAt(a.x + (b.x - a.x) * k, a.z + (b.z - a.z) * k)])
                        return false;
                }
            }
            catch (Exception)
            {
                return false;                                            // (a tile that cannot be read: keep to the lanes)
            }
            return true;
        }

        /// <summary>The course as plain data (a saved game): null if none.</summary>
        public static Dictionary<string, object> Capture()
        {
            if (!IsSet)
                return null;
            var pts = new List<object>();
            foreach (var p in Points)
                pts.Add(new List<object> { p.x, p.z });
            var water = new List<object>();
            foreach (var d in Depths)
                water.Add(float.IsInfinity(d) ? 9999.0 : (double)d);        // (JSON has no infinity: "plenty")
            // (7 Oct 2026: the kind of each leg too. Before, a loaded course had none, so no leg was a river: she sailed
            // where she should row, and the chart read "at sea" on the Rhine, until a new course was set)
            var kinds = new List<object>();
            foreach (byte k in Kinds)
                kinds.Add((double)k);
            var saved = new Dictionary<string, object>
            {
                ["dest"] = Destination, ["next"] = Next, ["km"] = Km, ["points"] = pts, ["water"] = water, ["kinds"] = kinds,
            };
            if (ToPlace)
                saved["place"] = new List<object> { Place.x, Place.z };
            return saved;
        }

        public static void Restore(Dictionary<string, object> d)
        {
            Destination = null;
            Points.Clear();
            Depths.Clear();
            Kinds.Clear();
            Next = 0;
            Km = 0.0;
            if (d != null)
            {
                foreach (var x in Json.Arr(d, "points") ?? new List<object>())
                    if (x is List<object> l && l.Count >= 2)
                        Points.Add(new WorldXZ((double)l[0], (double)l[1]));
                foreach (var x in Json.Arr(d, "water") ?? new List<object>())
                    if (x is double w)
                        Depths.Add(w >= 9999.0 ? float.PositiveInfinity : (float)w);
                if (Depths.Count != Points.Count)
                    Depths.Clear();                                      // (an older save: no water known, no waiting)
                foreach (var x in Json.Arr(d, "kinds") ?? new List<object>())
                    if (x is double k)
                        Kinds.Add((byte)Math.Max(0.0, Math.Min(255.0, k)));
                if (Kinds.Count != Points.Count)
                    Kinds.Clear();                                       // (an older save: worked out again below)
                if (Points.Count >= 2)
                {
                    Destination = Json.Str(d, "dest", null);
                    if (Json.Arr(d, "place") is List<object> pl && pl.Count >= 2 && pl[0] is double plx && pl[1] is double plz)
                        Place = new WorldXZ(plx, plz);
                    else if (Destination == PlaceId)
                        Place = Points[Points.Count - 1];
                    Next = Math.Max(1, Math.Min(Points.Count - 1, (int)Json.Num(d, "next", 1)));
                    Km = Json.Num(d, "km");
                    if (Kinds.Count == 0)
                        RebuildKinds();
                }
                else
                {
                    Points.Clear();
                    Depths.Clear();
                    Kinds.Clear();
                }
            }
            Changed?.Invoke();
        }

        /// <summary>A save from before the kinds were kept: the same course is planned again from its first point, and
        /// its kinds (and its water, if that was missing too) are taken when the line comes out with the same number of
        /// points. If it does not (the lanes were baked again since), the kinds stay empty, as they were: no leg is
        /// taken for a river until a new course is set.</summary>
        static void RebuildKinds()
        {
            if (Destination == null || Destination == PlaceId || Points.Count < 2)
                return;
            try
            {
                var line = new List<WorldXZ>();
                var water = new List<float>();
                var kinds = new List<byte>();
                if (!Plan(Destination, Points[0], null, line, water, kinds, out _) || kinds.Count != Points.Count)
                    return;
                Kinds.AddRange(kinds);
                if (Depths.Count == 0)
                    Depths.AddRange(water);
            }
            catch (Exception)
            {
                Kinds.Clear();                                           // (no tables or lanes to plan by: as it was)
            }
        }

        // 7 Oct 2026: a new play begins with no course. Play is entered without a domain reload, and a new game from the
        // menu clears the state the same way: without this the course of the game before stood in the new one. She lay
        // at home "bound for" a port of the last game, the watch took her for under way, and when she was made fast it
        // was an arrival: the toll man came down to a boat that had gone nowhere.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            Destination = null;
            Points.Clear();
            Depths.Clear();
            Kinds.Clear();
            Next = 0;
            Km = 0.0;
        }

        public static void Clear()
        {
            if (Destination == null)
                return;
            Destination = null;
            Points.Clear();
            Depths.Clear();
            Kinds.Clear();
            Next = 0;
            Km = 0.0;
            Changed?.Invoke();
        }

        /// <summary>Moves on along the line: she makes for the end of the leg of the line nearest her (looking a little
        /// way ahead, or along all the rest if she's been carried well off it), never back; and on past a point once
        /// she's within `pass` real metres of it. True when the point she's making for is the last (the berth).</summary>
        public static bool Advance(WorldXZ ship, double pass)
        {
            int last = Points.Count - 1;
            if (last < 1)
                return true;
            int seg = NearestLeg(ship, Math.Max(1, Next), Math.Min(last, Math.Max(1, Next) + 60), out double d);
            if (d > 5000.0)
                seg = NearestLeg(ship, Math.Max(1, Next), last, out d);
            Next = Math.Max(Next, seg);
            while (Next < last && WorldXZ.Distance(ship, Points[Next]) < pass)
                Next++;
            return Next >= last;
        }

        /// <summary>
        /// THE POINT TO STEER FOR ON THE CHART: the point of the line `ahead` real metres on from her own place on it.
        /// (The user, 7 Oct 2026, night, after sailing Dorestat to Traiectum: "the ship sailing is extremely buggy and the
        /// little map indicator just goes over land".) <see cref="Advance"/> was made for the first build's 3D levels: it
        /// passes a point of the line from 900 m off and looks for her leg among the next sixty, and a river's lane has a
        /// point every 300 m and loops back on itself, so on the chart she made for points far down the river, straight
        /// over the land between its bends. This keeps her on the line: her leg is looked for only a little way along it
        /// from the one she was on (never across the neck of a loop), and she steers for a point of the line itself, a
        /// few ship's lengths ahead, so she rounds every bend. `last`: that point is the berth (the line's end is nearer
        /// than `ahead`): she comes in and stops there as before.
        /// </summary>
        public static WorldXZ Aim(WorldXZ ship, double ahead, out bool last)
        {
            int end = Points.Count - 1;
            last = true;
            if (end < 1)
                return end == 0 ? Points[0] : ship;
            int from = Math.Min(end, Math.Max(1, Next));
            // the legs she can be on: from the one she was on, as far along the line as a few minutes take her
            double window = Math.Max(1200.0, ahead * 3.0), run = 0.0;
            int to = from;
            while (to < end && run < window)
            {
                to++;
                run += WorldXZ.Distance(Points[to - 1], Points[to]);
            }
            int seg = NearestLeg(ship, from, to, out double d);
            if (d > 5000.0)
                seg = NearestLeg(ship, from, end, out d);                 // (carried well off it: anywhere along the rest)
            Next = Math.Max(Next, seg);
            // her own place on that leg, and from it `ahead` metres on along the line
            int i = Math.Min(end, Math.Max(1, Next));
            var a = Points[i - 1];
            var b = Points[i];
            double ax = b.x - a.x, az = b.z - a.z, ll = ax * ax + az * az;
            double t = ll > 0.0 ? Math.Max(0.0, Math.Min(1.0, ((ship.x - a.x) * ax + (ship.z - a.z) * az) / ll)) : 1.0;
            double at = t * Math.Sqrt(ll) + Math.Max(0.0, ahead);
            for (; ; i++)
            {
                a = Points[i - 1];
                b = Points[i];
                double len = WorldXZ.Distance(a, b);
                if (at < len)
                {
                    double k = at / len;
                    last = false;
                    return new WorldXZ(a.x + (b.x - a.x) * k, a.z + (b.z - a.z) * k);
                }
                if (i >= end)
                    return b;                                             // (the berth is nearer than `ahead`)
                at -= len;
            }
        }

        /// <summary>The leg (by its end point's index) of the line from `from` to `to` nearest a point, and how far.</summary>
        static int NearestLeg(WorldXZ ship, int from, int to, out double best)
        {
            best = double.MaxValue;
            int seg = from;
            for (int i = from; i <= to; i++)
            {
                var a = Points[i - 1];
                var b = Points[i];
                double ax = b.x - a.x, az = b.z - a.z, ll = ax * ax + az * az;
                double t = ll > 0.0 ? Math.Max(0.0, Math.Min(1.0, ((ship.x - a.x) * ax + (ship.z - a.z) * az) / ll)) : 0.0;
                double dx = a.x + ax * t - ship.x, dz = a.z + az * t - ship.z, d = Math.Sqrt(dx * dx + dz * dz);
                if (d < best)
                {
                    best = d;
                    seg = i;
                }
            }
            return seg;
        }

        /// <summary>Real km still to go from a point along the rest of the line.</summary>
        public static double KmLeft(WorldXZ ship)
        {
            if (!IsSet || Next >= Points.Count)
                return 0.0;
            double m = WorldXZ.Distance(ship, Points[Next]);
            for (int i = Next; i + 1 < Points.Count; i++)
                m += WorldXZ.Distance(Points[i], Points[i + 1]);
            return m / 1000.0;
        }

        /// <summary>Game days a passage of this many real km takes at PlanKnots.</summary>
        public static double Days(double km) => km * 1000.0 / (SimUnits.RealMetresPerGameHour(PlanKnots * SimUnits.KnotMs) * 24.0);

        /// <summary>A port's berth in the world frame: its 3D berth if the port is in the scene, else its place.
        /// 3D HAND-OFF (ported 4 Oct): a placed port's berth is a scene position; FloatingOrigin.ToWorld puts it in the
        /// world frame only when the level scene carries a FloatingOrigin placed at the level's world position with
        /// horizontalScale = LevelInfo.realMetresPerUnit (see VoyagePlanner). Until it does, a placed port's berth would
        /// come out wrong here, so it's taken only when the scene has that frame; else the port's own lat/lon.</summary>
        public static WorldXZ BerthOf(PortDef def)
        {
            bool framed = UnityEngine.Object.FindAnyObjectByType<FloatingOrigin>() != null;
            foreach (var p in Port.All)
                if (framed && p.id == def.id && p.Placed)
                {
                    var w = FloatingOrigin.ToWorld(p.berth != null ? p.berth.position : p.transform.position);
                    return new WorldXZ(w.x, w.z);
                }
            var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
            return new WorldXZ(x, z);
        }
    }
}
