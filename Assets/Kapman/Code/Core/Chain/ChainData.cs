using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Kapman.Game;

namespace Kapman.Chain
{
    /// <summary>A place and a facing in a level's own frame: x east and z north in Unity metres, heading in degrees from north.</summary>
    public struct ChainPose
    {
        public double x, z, heading;

        public ChainPose(double x, double z, double heading)
        {
            this.x = x;
            this.z = z;
            this.heading = heading;
        }

        /// <summary>The unit vector of a heading: (east, north).</summary>
        public static (double x, double z) Dir(double heading)
        {
            double r = heading * Math.PI / 180.0;
            return (Math.Sin(r), Math.Cos(r));
        }

        /// <summary>A heading folded into [0, 360).</summary>
        public static double Wrap(double heading)
        {
            double h = heading % 360.0;
            return h < 0.0 ? h + 360.0 : h;
        }

        /// <summary>The smallest turn from one heading to another (degrees, -180 to 180).</summary>
        public static double Turn(double from, double to)
        {
            double d = Wrap(to - from);
            return d > 180.0 ? d - 360.0 : d;
        }

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0:0.##}, {1:0.##}) facing {2:0.#}", x, z, heading);
    }

    /// <summary>A level's water, as the chain's rules need it (the corridor rules of gateways 2.2).</summary>
    public sealed class ChainWater
    {
        /// <summary>The water's resting height (m).</summary>
        public double baseLevel;
        /// <summary>Whether the level's water moves with the tide.</summary>
        public bool tide;
        /// <summary>The river's stream (m/s; 0 on the sea).</summary>
        public double currentMps;
        /// <summary>Whether the stream pushes ships (RiverCurrent.pushesShips; Dorestad: on, decided 4 Oct).</summary>
        public bool pushesShips;
        /// <summary>The water's look (colour and ripple): equal on both sides of a corridor.</summary>
        public string preset = "";
        /// <summary>The level's record in Data/Water/water_793.json (its colour, clarity, stream by season, tide): "" = none yet.</summary>
        public string profile = "";
        /// <summary>The sea's largest wave patch (m), where a strait's translation must be a whole multiple of it; 0 = not given.</summary>
        public double patchMetres;
    }

    /// <summary>Where a level's local metres sit in the real world: the CRS, the corner's real coordinates and a turn.</summary>
    public sealed class ChainFrame
    {
        public string crs = "";
        public double originX, originZ, yaw;
    }

    /// <summary>One anchor: where a join meets a level, or a berth (a port's mooring, where routes begin and end).</summary>
    public sealed class ChainAnchor
    {
        public ChainLevel level;
        public string id = "";
        /// <summary>"join" or "berth".</summary>
        public string kind = "join";
        public string port = "";
        /// <summary>The anchor's pose; heading OUTWARD (the way a ship faces leaving the level by it). Unset on a planned level.</summary>
        public ChainPose pose;
        public bool hasPose;
        /// <summary>Where a ship coming in by this anchor is put at a re-posing join (heading inward). Without one: the anchor
        /// itself, facing in.</summary>
        public ChainPose arrive;
        public bool hasArrive;
        public double waterLevel;
        /// <summary>How much of the level's tide reaches this anchor: 0 (still water) or 1 (fully tidal).</summary>
        public double tideShare;
        /// <summary>The prompt's reach around the anchor (m).</summary>
        public double promptRadius;
        /// <summary>The corridor's centre line in the level's metres (corridor joins), empty otherwise.</summary>
        public readonly List<(double x, double z)> corridor = new List<(double x, double z)>();

        /// <summary>"level/anchor".</summary>
        public string Ref => level.id + "/" + id;
        public bool IsBerth => kind == "berth";

        /// <summary>The pose a ship is given on coming in by this anchor at a re-posing join (a mouth, a haul).</summary>
        public ChainPose ArrivalPose
        {
            get
            {
                if (hasArrive)
                    return arrive;
                return new ChainPose(pose.x, pose.z, ChainPose.Wrap(pose.heading + 180.0));
            }
        }

        /// <summary>Half the corridor's length along its line (m), or the design's default (150 m on a river, 300 m on the sea).</summary>
        public double CorridorHalfLength
        {
            get
            {
                if (corridor.Count >= 2)
                {
                    double len = 0.0;
                    for (int i = 1; i < corridor.Count; i++)
                        len += Math.Sqrt(Sq(corridor[i].x - corridor[i - 1].x) + Sq(corridor[i].z - corridor[i - 1].z));
                    return len * 0.5;
                }
                return level.IsOcean ? 300.0 : 150.0;
            }
        }

        static double Sq(double v) => v * v;
    }

    /// <summary>One level of the chain: a river place at true size or an ocean level shortened hard.</summary>
    public sealed class ChainLevel
    {
        public string id = "", name = "", kind = "river_place", state = "generated", scene = "", bake = "", layer = "";
        public double realMetresPerUnit = 1.0, latitude, longitude;
        /// <summary>A planned level's sailed metres from its join anchor to its berth (the design's estimate).</summary>
        public double plannedMetres;
        public ChainFrame frame;
        public readonly ChainWater water = new ChainWater();
        public readonly List<string> ports = new List<string>();
        public readonly List<ChainAnchor> anchors = new List<ChainAnchor>();
        /// <summary>The channel's centre line in the level's metres (river places), first point upstream when channelFlowsToEnd.</summary>
        public readonly List<(double x, double z)> channel = new List<(double x, double z)>();
        public bool channelFlowsToEnd = true;
        /// <summary>The real line the level stands for, (latitude, longitude) along the channel (river places and planned levels).</summary>
        public readonly List<(double lat, double lon)> chartRoute = new List<(double lat, double lon)>();
        /// <summary>The years the level is open (0 = no bound).</summary>
        public int from, to;

        public bool IsOcean => kind == "ocean";
        /// <summary>False for a level that is on the chain but has no scene yet.</summary>
        public bool Built => state != "planned";

        public ChainAnchor Anchor(string anchorId)
        {
            foreach (var a in anchors)
                if (a.id == anchorId)
                    return a;
            return null;
        }
    }

    /// <summary>The short shot from the deck that hides a change of scale (decision 2).</summary>
    public sealed class ChainBeat
    {
        public double seconds = 4.0, skipSeconds = 1.0;
        public bool skippableAfterFirst = true;
    }

    /// <summary>One join: two anchors, how the cut between them is hidden, and the real reach it stands for.</summary>
    public sealed class ChainJoin
    {
        public string id = "", kind = "", hide = "", graftIn = "", promptA = "", promptB = "";
        public ChainAnchor a, b;
        public bool twoWay = true;
        /// <summary>The cut-out reach (real km): what the chart's mark and the date jump across (decision 9).</summary>
        public double realKm;
        public double maxTonnes;
        public ChainBeat beat;
        public int from, to;
        public readonly List<string> closedWhen = new List<string>();

        /// <summary>The kinds whose two sides share a corridor of the same metres: the cut maps anchor onto anchor.</summary>
        public static readonly string[] CorridorKinds = { "bend", "narrows", "reeds", "strait", "weather" };
        /// <summary>The kinds that re-pose the ship at the far side's arrival point (the deck beat or the haul's scene hides it).</summary>
        public static readonly string[] RePoseKinds = { "mouth", "haul" };
        public static readonly string[] Hides = { "none", "bend", "narrows", "reeds", "deck_beat", "scene", "weather" };

        public bool IsCorridor => Array.IndexOf(CorridorKinds, kind) >= 0;
        public bool IsRePose => Array.IndexOf(RePoseKinds, kind) >= 0;
        /// <summary>Both levels exist as scenes.</summary>
        public bool Built => a != null && b != null && a.level.Built && b.level.Built;

        /// <summary>The anchor on the other side.</summary>
        public ChainAnchor Other(ChainAnchor side) => side == a ? b : side == b ? a : null;

        /// <summary>The prompt shown at an anchor of this join, with "(not built yet)" when the far side has no scene.</summary>
        public string PromptAt(ChainAnchor side)
        {
            string text = side == a ? promptA : side == b ? promptB : "";
            var other = Other(side);
            if (other != null && !other.level.Built && !string.IsNullOrEmpty(text))
                text += " (not built yet)";
            return text;
        }

        /// <summary>The play seconds the hide itself costs at 1x (the deck beat; nothing for a corridor, which is sailed).</summary>
        public double BeatSeconds => hide == "deck_beat" && beat != null ? beat.seconds : 0.0;
    }

    /// <summary>A named route: the stops (anchors) a voyage passes, the played minutes it should take and its real length.</summary>
    public sealed class ChainRoute
    {
        public string id = "";
        public readonly List<ChainAnchor> stops = new List<ChainAnchor>();
        public double budgetMinutes, realKm;
    }

    /// <summary>The player's ship's speeds the chain is timed by (m/s).</summary>
    public sealed class ChainPace
    {
        public double riverMps = 1.6, seaMps = 2.8, reachMps = 2.572;
    }

    /// <summary>
    /// THE LEVEL CHAIN'S DATA - Assets/Kapman/Data/Chain/chain.json read into plain objects: the levels with their anchors,
    /// water and real lines, the joins between them, the named routes and the player's pace. Look-ups by id and the graph
    /// (neighbours, what is reachable). Pure C#: the builders, the runtime, the chart, the fleets and the tests all read
    /// it. A malformed file throws (FormatException) naming what is wrong; the rules a sound chain keeps are ChainCheck's.
    /// </summary>
    public sealed class ChainData
    {
        /// <summary>The file in the project (relative to the project folder).</summary>
        public const string ProjectPath = "Assets/Kapman/Data/Chain/chain.json";

        public int version;
        public string home = "";
        public readonly ChainPace pace = new ChainPace();
        public readonly List<ChainLevel> levels = new List<ChainLevel>();
        public readonly List<ChainJoin> joins = new List<ChainJoin>();
        public readonly List<ChainRoute> routes = new List<ChainRoute>();

        /// <summary>Reads the chain from a file.</summary>
        public static ChainData Load(string path) => Parse(File.ReadAllText(path));

        /// <summary>Reads the chain from its JSON text.</summary>
        public static ChainData Parse(string json)
        {
            if (!(Json.Parse(json) is Dictionary<string, object> root))
                throw new FormatException("chain.json: the top level is not an object");
            var d = new ChainData
            {
                version = (int)Json.Num(root, "version", 1),
                home = Json.Str(root, "home"),
            };
            var pace = Json.Obj(root, "pace");
            if (pace != null)
            {
                d.pace.riverMps = Json.Num(pace, "riverMps", d.pace.riverMps);
                d.pace.seaMps = Json.Num(pace, "seaMps", d.pace.seaMps);
                d.pace.reachMps = Json.Num(pace, "reachMps", d.pace.reachMps);
            }
            foreach (var o in Json.Arr(root, "levels") ?? new List<object>())
                d.levels.Add(ReadLevel(o));
            foreach (var o in Json.Arr(root, "joins") ?? new List<object>())
                d.joins.Add(ReadJoin(d, o));
            foreach (var o in Json.Arr(root, "routes") ?? new List<object>())
                d.routes.Add(ReadRoute(d, o));
            return d;
        }

        // ---- look-ups

        public ChainLevel Level(string id)
        {
            foreach (var l in levels)
                if (l.id == id)
                    return l;
            return null;
        }

        /// <summary>An anchor by "level/anchor", or null.</summary>
        public ChainAnchor Anchor(string reference)
        {
            int slash = reference != null ? reference.IndexOf('/') : -1;
            if (slash <= 0)
                return null;
            return Level(reference.Substring(0, slash))?.Anchor(reference.Substring(slash + 1));
        }

        public ChainJoin Join(string id)
        {
            foreach (var j in joins)
                if (j.id == id)
                    return j;
            return null;
        }

        public ChainRoute Route(string id)
        {
            foreach (var r in routes)
                if (r.id == id)
                    return r;
            return null;
        }

        /// <summary>The join that meets at an anchor, or null (a berth, or an anchor no join names).</summary>
        public ChainJoin JoinAt(ChainAnchor anchor)
        {
            foreach (var j in joins)
                if (j.a == anchor || j.b == anchor)
                    return j;
            return null;
        }

        /// <summary>The join between two anchors (either way round), or null.</summary>
        public ChainJoin JoinBetween(ChainAnchor x, ChainAnchor y)
        {
            foreach (var j in joins)
                if ((j.a == x && j.b == y) || (j.a == y && j.b == x))
                    return j;
            return null;
        }

        /// <summary>The joins with a side in a level.</summary>
        public List<ChainJoin> JoinsOf(ChainLevel level)
        {
            var list = new List<ChainJoin>();
            foreach (var j in joins)
                if (j.a.level == level || j.b.level == level)
                    list.Add(j);
            return list;
        }

        /// <summary>The levels one join away.</summary>
        public List<ChainLevel> Neighbours(ChainLevel level)
        {
            var list = new List<ChainLevel>();
            foreach (var j in JoinsOf(level))
            {
                var other = j.a.level == level ? j.b.level : j.a.level;
                if (!list.Contains(other))
                    list.Add(other);
            }
            return list;
        }

        /// <summary>Every level reachable from one (itself included), over the joins either way.</summary>
        public HashSet<ChainLevel> Reachable(ChainLevel start)
        {
            var seen = new HashSet<ChainLevel>();
            if (start == null)
                return seen;
            var todo = new Queue<ChainLevel>();
            seen.Add(start);
            todo.Enqueue(start);
            while (todo.Count > 0)
                foreach (var n in Neighbours(todo.Dequeue()))
                    if (seen.Add(n))
                        todo.Enqueue(n);
            return seen;
        }

        /// <summary>
        /// The rigid transform of the cut, leaving by one anchor of a join: at a corridor join the leaving anchor maps onto
        /// the far anchor (heading turned to face in); at a mouth or a haul onto the far side's arrival pose. Throws when
        /// either side has no pose (a planned level).
        /// </summary>
        public static JoinTransform Transform(ChainJoin join, ChainAnchor leaving)
        {
            var other = join.Other(leaving) ?? throw new ArgumentException($"{leaving?.Ref} is not a side of the join {join.id}");
            if (!leaving.hasPose || !other.hasPose)
                throw new InvalidOperationException($"The join {join.id} is not built: {(leaving.hasPose ? other.Ref : leaving.Ref)} has no pose");
            var to = join.IsRePose ? other.ArrivalPose : new ChainPose(other.pose.x, other.pose.z, ChainPose.Wrap(other.pose.heading + 180.0));
            return JoinTransform.Between(leaving.pose, to);
        }

        // ---- reading

        static ChainLevel ReadLevel(object o)
        {
            var l = new ChainLevel
            {
                id = Need(o, "id", "a level"),
                name = Json.Str(o, "name"),
                kind = Json.Str(o, "kind", "river_place"),
                state = Json.Str(o, "state", "generated"),
                scene = Json.Str(o, "scene"),
                bake = Json.Str(o, "bake"),
                layer = Json.Str(o, "layer"),
                realMetresPerUnit = Json.Num(o, "realMetresPerUnit", 1.0),
                latitude = Json.Num(o, "latitude"),
                longitude = Json.Num(o, "longitude"),
                plannedMetres = Json.Num(o, "plannedMetres"),
                channelFlowsToEnd = Json.Bool(o, "channelFlowsToEnd", true),
                from = (int)Json.Num(o, "from"),
                to = (int)Json.Num(o, "to"),
            };
            var frame = Json.Obj(o, "frame");
            if (frame != null)
            {
                var origin = Pair(Json.Arr(frame, "origin"), $"the level {l.id}'s frame origin");
                l.frame = new ChainFrame { crs = Json.Str(frame, "crs"), originX = origin.x, originZ = origin.z, yaw = Json.Num(frame, "yaw") };
            }
            var water = Json.Obj(o, "water");
            if (water != null)
            {
                l.water.baseLevel = Json.Num(water, "base");
                l.water.tide = Json.Bool(water, "tide");
                l.water.currentMps = Json.Num(water, "currentMps");
                l.water.pushesShips = Json.Bool(water, "pushesShips");
                l.water.preset = Json.Str(water, "preset");
                l.water.profile = Json.Str(water, "profile");
                l.water.patchMetres = Json.Num(water, "patchMetres");
            }
            foreach (var p in Json.Arr(o, "ports") ?? new List<object>())
                if (p is string s)
                    l.ports.Add(s);
            foreach (var p in Json.Arr(o, "channel") ?? new List<object>())
                l.channel.Add(Pair(p as List<object>, $"the level {l.id}'s channel"));
            foreach (var p in Json.Arr(o, "chartRoute") ?? new List<object>())
            {
                var ll = Pair(p as List<object>, $"the level {l.id}'s chart route");
                l.chartRoute.Add((ll.x, ll.z));
            }
            foreach (var a in Json.Arr(o, "anchors") ?? new List<object>())
                l.anchors.Add(ReadAnchor(l, a));
            return l;
        }

        static ChainAnchor ReadAnchor(ChainLevel level, object o)
        {
            var a = new ChainAnchor
            {
                level = level,
                id = Need(o, "id", $"an anchor of {level.id}"),
                kind = Json.Str(o, "kind", "join"),
                port = Json.Str(o, "port"),
                waterLevel = Json.Num(o, "waterLevel", level.water.baseLevel),
                tideShare = Json.Num(o, "tideShare"),
                promptRadius = Json.Num(o, "promptRadius", 60.0),
            };
            var pos = Json.Arr(o, "pos");
            if (pos != null)
            {
                var p = Pair(pos, $"the anchor {a.Ref}'s pos");
                a.pose = new ChainPose(p.x, p.z, ChainPose.Wrap(Json.Num(o, "heading")));
                a.hasPose = true;
            }
            var arrive = Json.Obj(o, "arrive");
            if (arrive != null)
            {
                var p = Pair(Json.Arr(arrive, "pos"), $"the anchor {a.Ref}'s arrival pos");
                a.arrive = new ChainPose(p.x, p.z, ChainPose.Wrap(Json.Num(arrive, "heading")));
                a.hasArrive = true;
            }
            foreach (var p in Json.Arr(o, "corridor") ?? new List<object>())
                a.corridor.Add(Pair(p as List<object>, $"the anchor {a.Ref}'s corridor"));
            return a;
        }

        static ChainJoin ReadJoin(ChainData d, object o)
        {
            string id = Need(o, "id", "a join");
            var j = new ChainJoin
            {
                id = id,
                kind = Json.Str(o, "kind"),
                hide = Json.Str(o, "hide"),
                graftIn = Json.Str(o, "graftIn"),
                twoWay = Json.Bool(o, "twoWay", true),
                realKm = Json.Num(o, "realKm"),
                maxTonnes = Json.Num(o, "maxTonnes"),
                from = (int)Json.Num(o, "from"),
                to = (int)Json.Num(o, "to"),
            };
            string aRef = Need(o, "a", $"the join {id}"), bRef = Need(o, "b", $"the join {id}");
            j.a = d.Anchor(aRef) ?? throw new FormatException($"chain.json: the join {id} names {aRef}, which is not an anchor of a level");
            j.b = d.Anchor(bRef) ?? throw new FormatException($"chain.json: the join {id} names {bRef}, which is not an anchor of a level");
            var prompt = Json.Obj(o, "prompt");
            if (prompt != null)
            {
                j.promptA = Json.Str(prompt, "a");
                j.promptB = Json.Str(prompt, "b");
            }
            var beat = Json.Obj(o, "beat");
            if (beat != null)
                j.beat = new ChainBeat
                {
                    seconds = Json.Num(beat, "seconds", 4.0),
                    skipSeconds = Json.Num(beat, "skipSeconds", 1.0),
                    skippableAfterFirst = Json.Bool(beat, "skippableAfterFirst", true),
                };
            foreach (var c in Json.Arr(o, "closedWhen") ?? new List<object>())
                if (c is string s)
                    j.closedWhen.Add(s);
            return j;
        }

        static ChainRoute ReadRoute(ChainData d, object o)
        {
            var r = new ChainRoute
            {
                id = Need(o, "id", "a route"),
                budgetMinutes = Json.Num(o, "budgetMinutes"),
                realKm = Json.Num(o, "realKm"),
            };
            foreach (var s in Json.Arr(o, "stops") ?? new List<object>())
            {
                string reference = s as string;
                r.stops.Add(d.Anchor(reference) ?? throw new FormatException($"chain.json: the route {r.id} stops at {reference}, which is not an anchor of a level"));
            }
            return r;
        }

        static string Need(object o, string key, string what)
        {
            string s = Json.Str(o, key);
            if (string.IsNullOrEmpty(s))
                throw new FormatException($"chain.json: {what} has no \"{key}\"");
            return s;
        }

        static (double x, double z) Pair(List<object> list, string what)
        {
            if (list == null || list.Count < 2 || !(list[0] is double x) || !(list[1] is double z))
                throw new FormatException($"chain.json: {what} is not a pair of numbers");
            return (x, z);
        }
    }
}
