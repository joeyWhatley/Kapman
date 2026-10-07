using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kapman.Chain
{
    /// <summary>One thing the checker found: an error breaks the chain; a note is worth knowing (a join to a level not built).</summary>
    public struct ChainFinding
    {
        public bool error;
        public string rule;
        public string message;

        public override string ToString() => (error ? "ERROR " : "note ") + rule + ": " + message;
    }

    /// <summary>
    /// THE CHAIN'S RULES - what a sound chain keeps (gateways 2.2 and 2.12), checked on the data alone, as a list of
    /// findings for the tests and the editor's Kapman > Chain > Check. The rules that need the bakes or the scenes (the
    /// depth under a corridor, an anchor on land, the corridor heights matching) are the editor checker's.
    ///   ids        every level, join and route id once; anchor ids once per level; known kinds
    ///   ends       a join's anchors are join anchors of two different levels; no anchor in two joins
    ///   poses      a built level's anchors have poses; a planned level has its planned metres
    ///   corridor   a two-way corridor join has a corridor on both built sides
    ///   scale      a bend, narrows, reeds or strait join has one realMetresPerUnit on both sides
    ///   strait     a strait between ocean levels: frames unturned, no turn across it, the shift a whole number of wave patches
    ///   prompt     a mouth or a haul has a prompt on both sides; a deck beat lasts 3 to 5 s (decision 2)
    ///   tide       every anchor's tideShare 0 or 1 (0 where the level has no tide); the tide reaching the two anchors of
    ///              a corridor join equal (the level's tide times the anchor's share), so the water never steps at a cut
    ///   current    a bend, narrows or reeds join: equal currentMps, pushesShips and preset
    ///   layer      Level0 or Level1, different across every join (unless hidden by weather)
    ///   connected  every level reachable from home
    ///   ports      no port in two levels
    ///   route      every route's stops make legs; its real km within 5 % of its stated length; its played minutes within
    ///              20 % of its budget at the chain's pace
    ///   years      from before to
    /// </summary>
    public static class ChainCheck
    {
        public const double RouteKmTolerance = 0.05;
        public const double RouteMinutesTolerance = 0.20;
        public const double MinBeatSeconds = 3.0, MaxBeatSeconds = 5.0;

        static readonly string[] LevelKinds = { "river_place", "ocean" };
        static readonly string[] States = { "generated", "hand_finished", "planned" };
        static readonly string[] AnchorKinds = { "join", "berth" };
        static readonly string[] Layers = { "Level0", "Level1" };
        static readonly string[] JoinKinds = { "bend", "narrows", "reeds", "mouth", "strait", "haul", "weather" };

        public static List<ChainFinding> Check(ChainData d)
        {
            var found = new List<ChainFinding>();
            void Error(string rule, string message) => found.Add(new ChainFinding { error = true, rule = rule, message = message });
            void Note(string rule, string message) => found.Add(new ChainFinding { error = false, rule = rule, message = message });

            // ---- ids and kinds
            Unique(d.levels.ConvertAll(l => l.id), "level", Error);
            Unique(d.joins.ConvertAll(j => j.id), "join", Error);
            Unique(d.routes.ConvertAll(r => r.id), "route", Error);
            foreach (var l in d.levels)
            {
                Unique(l.anchors.ConvertAll(a => a.id), "anchor of " + l.id, Error);
                if (Array.IndexOf(LevelKinds, l.kind) < 0)
                    Error("ids", $"the level {l.id} is of an unknown kind '{l.kind}'");
                if (Array.IndexOf(States, l.state) < 0)
                    Error("ids", $"the level {l.id} is in an unknown state '{l.state}'");
                foreach (var a in l.anchors)
                    if (Array.IndexOf(AnchorKinds, a.kind) < 0)
                        Error("ids", $"the anchor {a.Ref} is of an unknown kind '{a.kind}'");
                if (l.from > 0 && l.to > 0 && l.from > l.to)
                    Error("years", $"the level {l.id} is open from {l.from} to {l.to}");
            }
            foreach (var j in d.joins)
            {
                if (Array.IndexOf(JoinKinds, j.kind) < 0)
                    Error("ids", $"the join {j.id} is of an unknown kind '{j.kind}'");
                if (Array.IndexOf(ChainJoin.Hides, j.hide) < 0)
                    Error("ids", $"the join {j.id} has an unknown hide '{j.hide}'");
                if (j.from > 0 && j.to > 0 && j.from > j.to)
                    Error("years", $"the join {j.id} is open from {j.from} to {j.to}");
                if (j.realKm < 0.0)
                    Error("ids", $"the join {j.id} has a negative reach ({j.realKm} km)");
            }

            // ---- the ends of the joins
            var used = new Dictionary<ChainAnchor, ChainJoin>();
            foreach (var j in d.joins)
            {
                if (j.a.level == j.b.level)
                    Error("ends", $"the join {j.id} has both ends in {j.a.level.id}");
                foreach (var side in new[] { j.a, j.b })
                {
                    if (side.IsBerth)
                        Error("ends", $"the join {j.id} ends at the berth {side.Ref}");
                    if (used.TryGetValue(side, out var other))
                        Error("ends", $"the anchor {side.Ref} is in two joins ({other.id} and {j.id})");
                    else
                        used[side] = j;
                }
                if (!j.Built)
                    Note("ends", $"the join {j.id} leads to a level not built yet ({(j.a.level.Built ? j.b.level.id : j.a.level.id)})");
            }

            // ---- poses
            foreach (var l in d.levels)
            {
                if (l.Built)
                {
                    foreach (var a in l.anchors)
                        if (!a.hasPose)
                            Error("poses", $"the anchor {a.Ref} of the built level {l.id} has no pos");
                }
                else if (l.plannedMetres <= 0.0)
                    Error("poses", $"the planned level {l.id} has no plannedMetres");
                if (l.realMetresPerUnit <= 0.0)
                    Error("poses", $"the level {l.id} has realMetresPerUnit {l.realMetresPerUnit}");
            }

            // ---- per join: corridor, scale, strait, prompt, tide, current, layer
            foreach (var j in d.joins)
            {
                var la = j.a.level;
                var lb = j.b.level;
                if (j.IsCorridor)
                {
                    if (j.twoWay)
                        foreach (var side in new[] { j.a, j.b })
                            if (side.level.Built && side.corridor.Count < 2)
                                Error("corridor", $"the corridor join {j.id} has no corridor at {side.Ref}");
                    if (j.kind != "weather" && Math.Abs(la.realMetresPerUnit - lb.realMetresPerUnit) > 1e-9)
                        Error("scale", $"the {j.kind} join {j.id} crosses a change of scale (1:{Num(la.realMetresPerUnit)} to 1:{Num(lb.realMetresPerUnit)}): it needs a deck beat");
                    // the tide that reaches each anchor: a level that crosses its river's tidal limit is tidal with a
                    // share of 0 at its river end, which meets a tideless neighbour evenly
                    double tideA = la.water.tide ? j.a.tideShare : 0.0, tideB = lb.water.tide ? j.b.tideShare : 0.0;
                    if (Math.Abs(tideA - tideB) > 1e-9)
                        Error("tide", $"the corridor join {j.id} meets the tide unequally ({Num(tideA)} at {j.a.Ref}, {Num(tideB)} at {j.b.Ref}): the water would step at the cut");
                }
                if (j.kind == "bend" || j.kind == "narrows" || j.kind == "reeds")
                {
                    if (Math.Abs(la.water.currentMps - lb.water.currentMps) > 1e-9)
                        Error("current", $"the {j.kind} join {j.id} changes the stream ({Num(la.water.currentMps)} to {Num(lb.water.currentMps)} m/s)");
                    if (la.water.pushesShips != lb.water.pushesShips)
                        Error("current", $"the {j.kind} join {j.id} has the stream pushing ships on one side only");
                    if (la.water.preset != lb.water.preset)
                        Error("current", $"the {j.kind} join {j.id} changes the water's look ({la.water.preset} to {lb.water.preset})");
                }
                if (j.kind == "strait" && la.IsOcean && lb.IsOcean && j.Built)
                {
                    if (la.frame == null || lb.frame == null || Math.Abs(la.frame.yaw) > 1e-9 || Math.Abs(lb.frame.yaw) > 1e-9)
                        Error("strait", $"the strait {j.id} joins seas whose frames are turned (or missing)");
                    var t = ChainData.Transform(j, j.a);
                    if (Math.Abs(ChainPose.Turn(0.0, t.yaw)) > 1e-6)
                        Error("strait", $"the strait {j.id} turns the sea by {Num(t.yaw)} degrees");
                    double patch = Math.Max(la.water.patchMetres, lb.water.patchMetres);
                    if (patch > 0.0 && (!WholeMultiple(t.shiftX, patch) || !WholeMultiple(t.shiftZ, patch)))
                        Error("strait", $"the strait {j.id} shifts the sea by ({Num(t.shiftX)}, {Num(t.shiftZ)}) m, not a whole number of {Num(patch)} m wave patches");
                }
                if (j.IsRePose)
                {
                    if (string.IsNullOrEmpty(j.promptA) || string.IsNullOrEmpty(j.promptB))
                        Error("prompt", $"the {j.kind} join {j.id} needs a prompt at both anchors");
                    if (j.kind == "mouth" && j.hide != "deck_beat")
                        Error("prompt", $"the mouth {j.id} is hidden by '{j.hide}', not the deck beat");
                }
                if (j.hide == "deck_beat")
                {
                    if (j.beat == null)
                        Error("prompt", $"the join {j.id} is hidden by the deck beat but has no beat");
                    else if (j.beat.seconds < MinBeatSeconds || j.beat.seconds > MaxBeatSeconds)
                        Error("prompt", $"the deck beat of {j.id} lasts {Num(j.beat.seconds)} s, not {Num(MinBeatSeconds)}-{Num(MaxBeatSeconds)}");
                }
                if (Array.IndexOf(Layers, la.layer) >= 0 && la.layer == lb.layer && j.hide != "weather")
                    Error("layer", $"the join {j.id} links {la.id} and {lb.id}, both on {la.layer}");
            }

            // ---- per level: tideShare and layer names
            foreach (var l in d.levels)
            {
                if (Array.IndexOf(Layers, l.layer) < 0)
                    Error("layer", $"the level {l.id} is on '{l.layer}', not Level0 or Level1");
                foreach (var a in l.anchors)
                {
                    if (a.tideShare != 0.0 && a.tideShare != 1.0)
                        Error("tide", $"the anchor {a.Ref} has tideShare {Num(a.tideShare)}: an anchor sits where the tide is still or full");
                    if (!l.water.tide && a.tideShare != 0.0)
                        Error("tide", $"the anchor {a.Ref} has tideShare {Num(a.tideShare)} in a level without tide");
                }
            }

            // ---- the graph
            var home = d.Level(d.home);
            if (home == null)
                Error("connected", $"the home level '{d.home}' is not in the chain");
            else
            {
                var reach = d.Reachable(home);
                foreach (var l in d.levels)
                    if (!reach.Contains(l))
                        Error("connected", $"the level {l.id} cannot be reached from {home.id}");
            }
            var portLevel = new Dictionary<string, ChainLevel>();
            foreach (var l in d.levels)
                foreach (var p in l.ports)
                {
                    if (portLevel.TryGetValue(p, out var other) && other != l)
                        Error("ports", $"the port {p} is in both {other.id} and {l.id}");
                    portLevel[p] = l;
                }

            // ---- the routes
            var pace = ChainShip.Player(d);
            foreach (var r in d.routes)
            {
                if (!RouteMap.TryBuild(d, r, out var map, out var why))
                {
                    Error("route", why);
                    continue;
                }
                if (r.realKm > 0.0 && Math.Abs(map.TotalRealKm - r.realKm) > RouteKmTolerance * r.realKm)
                    Error("route", $"the route {r.id} sums to {Num(map.TotalRealKm)} real km against its {Num(r.realKm)} (more than {Num(RouteKmTolerance * 100.0)} % off)");
                double minutes = ChainSim.PlayMinutes(map, pace);
                if (r.budgetMinutes > 0.0 && Math.Abs(minutes - r.budgetMinutes) > RouteMinutesTolerance * r.budgetMinutes)
                    Error("route", $"the route {r.id} takes {Num(minutes)} minutes against its budget of {Num(r.budgetMinutes)} (more than {Num(RouteMinutesTolerance * 100.0)} % off)");
            }
            return found;
        }

        /// <summary>The errors alone.</summary>
        public static List<ChainFinding> Errors(ChainData d) => Check(d).FindAll(f => f.error);

        static void Unique(List<string> ids, string what, Action<string, string> error)
        {
            var seen = new HashSet<string>();
            foreach (var id in ids)
                if (!seen.Add(id))
                    error("ids", $"the {what} id '{id}' is used twice");
        }

        static bool WholeMultiple(double value, double step)
        {
            double k = value / step;
            return Math.Abs(k - Math.Round(k)) * step < 0.01;
        }

        static string Num(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
