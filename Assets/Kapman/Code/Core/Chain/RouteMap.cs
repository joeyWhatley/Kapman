using System;
using System.Collections.Generic;
using Kapman.World;

namespace Kapman.Chain
{
    public enum RouteLegKind { Sail, Join }

    /// <summary>One leg of a route: sailed inside a level (from one stop to the next), or a join's cut-out reach.</summary>
    public sealed class RouteLeg
    {
        public RouteLegKind kind;
        /// <summary>The level sailed (a sail leg), or the level the join leads into (a join leg).</summary>
        public ChainLevel level;
        public ChainAnchor from, to;
        /// <summary>The join crossed (join legs).</summary>
        public ChainJoin join;
        /// <summary>Where a sail leg starts and ends, in the level's metres (unset in a planned level).</summary>
        public ChainPose start, end;
        /// <summary>A sail leg in a river place: the channel parameter (0 at the channel's first point, 1 at its last) at
        /// the start and the end.</summary>
        public double uStart, uEnd;
        /// <summary>The level's metres sailed (sail legs).</summary>
        public double metres;
        /// <summary>The real kilometres the leg stands for.</summary>
        public double realKm;
        /// <summary>The real kilometres from the route's start to this leg's start.</summary>
        public double kmAtStart;
        /// <summary>A sail leg in a river place going with the stream.</summary>
        public bool downstream;
        /// <summary>A sail leg in a level with a channel, mapped along it.</summary>
        public bool onChannel;
    }

    /// <summary>
    /// THE CHART'S VIEW OF A ROUTE - a named route of chain.json as legs (sailed in a level, or a join's cut-out reach),
    /// with their real kilometres and where each point of them lies on the real map (gateways 2.7). Inside a river place
    /// progress is the channel's parameter u, and the mark sits at the same share of the level's real chart line; on an
    /// ocean level the level's frame gives the real position directly. At a join the player's mark JUMPS across the
    /// cut-out reach in one step, with the date (decision 9): there is no glide. A sim ship inside a reach (no body, no
    /// clock jump of her own) is drawn part way across it in a straight line by her share of the reach's hours.
    /// </summary>
    public sealed class RouteMap
    {
        public readonly ChainData data;
        public readonly ChainRoute route;
        public readonly List<RouteLeg> legs = new List<RouteLeg>();

        /// <summary>The route's real kilometres, end to end.</summary>
        public double TotalRealKm { get; }

        /// <summary>Builds the legs. Throws (FormatException) when two following stops are neither in one level nor the two
        /// sides of a join, or a sail leg cannot be measured.</summary>
        public RouteMap(ChainData data, ChainRoute route)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.route = route ?? throw new ArgumentNullException(nameof(route));
            if (route.stops.Count < 2)
                throw new FormatException($"chain.json: the route {route.id} needs two stops at least");
            double km = 0.0;
            RouteLeg previous = null;
            for (int i = 1; i < route.stops.Count; i++)
            {
                var from = route.stops[i - 1];
                var to = route.stops[i];
                RouteLeg leg;
                if (from.level == to.level)
                    leg = SailLeg(from, to, previous);
                else
                {
                    var join = data.JoinBetween(from, to)
                               ?? throw new FormatException($"chain.json: the route {route.id} goes from {from.Ref} to {to.Ref}, which no join links");
                    leg = new RouteLeg { kind = RouteLegKind.Join, level = to.level, from = from, to = to, join = join, realKm = join.realKm };
                }
                leg.kmAtStart = km;
                km += leg.realKm;
                legs.Add(leg);
                previous = leg;
            }
            TotalRealKm = km;
        }

        /// <summary>Builds the map of a route, or says why not.</summary>
        public static bool TryBuild(ChainData data, ChainRoute route, out RouteMap map, out string error)
        {
            try
            {
                map = new RouteMap(data, route);
                error = null;
                return true;
            }
            catch (FormatException e)
            {
                map = null;
                error = e.Message;
                return false;
            }
        }

        RouteLeg SailLeg(ChainAnchor from, ChainAnchor to, RouteLeg previous)
        {
            var level = from.level;
            var leg = new RouteLeg { kind = RouteLegKind.Sail, level = level, from = from, to = to };
            if (!level.Built)
            {
                if (level.plannedMetres <= 0.0)
                    throw new FormatException($"chain.json: the planned level {level.id} has no plannedMetres for the route {route.id}");
                leg.metres = level.plannedMetres;
                leg.realKm = leg.metres * level.realMetresPerUnit / 1000.0;
                // a planned level's chart line runs from its join anchor (u 0) to its berth (u 1)
                leg.uStart = from.IsBerth ? 1.0 : 0.0;
                leg.uEnd = to.IsBerth ? 1.0 : 0.0;
                if (leg.uStart == leg.uEnd)
                    leg.uEnd = 1.0 - leg.uStart;
                leg.downstream = true;
                return leg;
            }
            if (!from.hasPose || !to.hasPose)
                throw new FormatException($"chain.json: the route {route.id} sails {level.id} from {from.Ref} to {to.Ref}, but an anchor has no pose");
            // she starts where she was put: at the far side's arrival point after a mouth or a haul, else at the stop itself
            leg.start = previous != null && previous.kind == RouteLegKind.Join && previous.join.IsRePose ? from.ArrivalPose : from.pose;
            leg.end = to.pose;
            if (level.channel.Count >= 2)
            {
                double length = ChannelLength(level);
                double s0 = ChannelS(level, leg.start.x, leg.start.z), s1 = ChannelS(level, leg.end.x, leg.end.z);
                leg.onChannel = true;
                leg.uStart = s0 / length;
                leg.uEnd = s1 / length;
                leg.metres = Math.Abs(s1 - s0);
                leg.downstream = (s1 >= s0) == level.channelFlowsToEnd;
            }
            else
            {
                leg.metres = Math.Sqrt(Sq(leg.end.x - leg.start.x) + Sq(leg.end.z - leg.start.z));
                leg.downstream = true;
            }
            leg.realKm = leg.metres * level.realMetresPerUnit / 1000.0;
            return leg;
        }

        // ---- real kilometres and the chart

        /// <summary>The real kilometres from the route's start at a share (0-1) of a leg.</summary>
        public double RealKm(int leg, double fraction)
        {
            var l = legs[leg];
            return l.kmAtStart + Clamp01(fraction) * l.realKm;
        }

        /// <summary>The channel parameter at a share of a sail leg.</summary>
        public double LegU(int leg, double fraction)
        {
            var l = legs[leg];
            return l.uStart + (l.uEnd - l.uStart) * Clamp01(fraction);
        }

        /// <summary>The level's metres at a share of a sail leg (along the channel in a river place, straight on the sea).</summary>
        public (double x, double z) LegPoint(int leg, double fraction)
        {
            var l = legs[leg];
            if (l.kind != RouteLegKind.Sail || !l.level.Built)
                throw new InvalidOperationException($"Leg {leg} of {route.id} is not sailed in a built level");
            if (l.onChannel)
                return ChannelPoint(l.level, LegU(leg, fraction));
            double f = Clamp01(fraction);
            return (l.start.x + (l.end.x - l.start.x) * f, l.start.z + (l.end.z - l.start.z) * f);
        }

        /// <summary>Where a point at a share of a leg lies on the real map (latitude, longitude). A join leg is drawn straight
        /// across its reach (for sim ships inside it; the player's mark uses <see cref="PlayerMark"/>).</summary>
        public (double lat, double lon) ChartPoint(int leg, double fraction)
        {
            var l = legs[leg];
            if (l.kind == RouteLegKind.Join)
            {
                var a = leg > 0 && legs[leg - 1].kind == RouteLegKind.Sail ? ChartPoint(leg - 1, 1.0) : StopChart(l.from);
                var b = leg + 1 < legs.Count && legs[leg + 1].kind == RouteLegKind.Sail ? ChartPoint(leg + 1, 0.0) : StopChart(l.to);
                double f = Clamp01(fraction);
                return (a.lat + (b.lat - a.lat) * f, a.lon + (b.lon - a.lon) * f);
            }
            if (l.onChannel || !l.level.Built)
                return ChartAt(l.level, LegU(leg, fraction));
            var (x, z) = LegPoint(leg, fraction);
            return LocalToLatLon(l.level, x, z);
        }

        /// <summary>The player's mark: as ChartPoint inside a level; at a join it has already jumped to the far side.</summary>
        public (double lat, double lon) PlayerMark(int leg, double fraction)
        {
            var l = legs[leg];
            if (l.kind == RouteLegKind.Join)
                return leg + 1 < legs.Count && legs[leg + 1].kind == RouteLegKind.Sail ? ChartPoint(leg + 1, 0.0) : StopChart(l.to);
            return ChartPoint(leg, fraction);
        }

        (double lat, double lon) StopChart(ChainAnchor stop)
        {
            if (stop.level.Built && stop.hasPose && (stop.level.channel.Count < 2 || stop.level.chartRoute.Count < 2))
                return LocalToLatLon(stop.level, stop.pose.x, stop.pose.z);
            if (stop.level.Built && stop.hasPose)
                return ChartAt(stop.level, ChannelS(stop.level, stop.pose.x, stop.pose.z) / ChannelLength(stop.level));
            return ChartAt(stop.level, stop.IsBerth ? 1.0 : 0.0);
        }

        // ---- a level's own mapping (static: the runtime's chart uses them without a route)

        /// <summary>A point of a level's metres on the real map: along the chart line in a river place, by the frame on the sea.</summary>
        public static (double lat, double lon) LocalToLatLon(ChainLevel level, double x, double z)
        {
            if (level.channel.Count >= 2 && level.chartRoute.Count >= 2)
                return ChartAt(level, ChannelS(level, x, z) / ChannelLength(level));
            if (level.frame != null && level.frame.crs == "kapman_world")
            {
                var f = level.frame;
                double r = f.yaw * Math.PI / 180.0, c = Math.Cos(r), s = Math.Sin(r);
                double k = level.realMetresPerUnit;
                double wx = f.originX + (x * c + z * s) * k, wz = f.originZ + (-x * s + z * c) * k;
                return WorldFrame.ToLatLon(wx, wz);
            }
            throw new InvalidOperationException($"The level {level.id} has neither a channel with a chart line nor a kapman_world frame");
        }

        /// <summary>The real point at a share (0-1) of a level's chart line, by length (an equirectangular measure).</summary>
        public static (double lat, double lon) ChartAt(ChainLevel level, double u)
        {
            var line = level.chartRoute;
            if (line.Count == 0)
                return (level.latitude, level.longitude);
            if (line.Count == 1)
                return line[0];
            double total = 0.0;
            var seg = new double[line.Count];
            for (int i = 1; i < line.Count; i++)
            {
                seg[i] = ChartMetres(line[i - 1], line[i]);
                total += seg[i];
            }
            double want = Clamp01(u) * total;
            for (int i = 1; i < line.Count; i++)
            {
                if (want <= seg[i] || i == line.Count - 1)
                {
                    double t = seg[i] > 0.0 ? Clamp01(want / seg[i]) : 0.0;
                    return (line[i - 1].lat + (line[i].lat - line[i - 1].lat) * t, line[i - 1].lon + (line[i].lon - line[i - 1].lon) * t);
                }
                want -= seg[i];
            }
            return line[line.Count - 1];
        }

        /// <summary>The channel's length (m).</summary>
        public static double ChannelLength(ChainLevel level)
        {
            double len = 0.0;
            for (int i = 1; i < level.channel.Count; i++)
                len += Math.Sqrt(Sq(level.channel[i].x - level.channel[i - 1].x) + Sq(level.channel[i].z - level.channel[i - 1].z));
            return len;
        }

        /// <summary>How far along the channel (m from its first point) a point lies, by its nearest point on the line.</summary>
        public static double ChannelS(ChainLevel level, double x, double z)
        {
            var ch = level.channel;
            double best = double.MaxValue, bestS = 0.0, s = 0.0;
            for (int i = 1; i < ch.Count; i++)
            {
                double ax = ch[i - 1].x, az = ch[i - 1].z, dx = ch[i].x - ax, dz = ch[i].z - az;
                double len2 = dx * dx + dz * dz, len = Math.Sqrt(len2);
                double t = len2 > 0.0 ? Clamp01(((x - ax) * dx + (z - az) * dz) / len2) : 0.0;
                double d = Sq(ax + dx * t - x) + Sq(az + dz * t - z);
                if (d < best)
                {
                    best = d;
                    bestS = s + t * len;
                }
                s += len;
            }
            return bestS;
        }

        /// <summary>The channel's point at a share (0-1) of its length, in the level's metres.</summary>
        public static (double x, double z) ChannelPoint(ChainLevel level, double u)
        {
            var ch = level.channel;
            double want = Clamp01(u) * ChannelLength(level);
            for (int i = 1; i < ch.Count; i++)
            {
                double len = Math.Sqrt(Sq(ch[i].x - ch[i - 1].x) + Sq(ch[i].z - ch[i - 1].z));
                if (want <= len || i == ch.Count - 1)
                {
                    double t = len > 0.0 ? Clamp01(want / len) : 0.0;
                    return (ch[i - 1].x + (ch[i].x - ch[i - 1].x) * t, ch[i - 1].z + (ch[i].z - ch[i - 1].z) * t);
                }
                want -= len;
            }
            return ch[ch.Count - 1];
        }

        static double ChartMetres((double lat, double lon) a, (double lat, double lon) b)
        {
            const double MetresPerDegree = 111195.0;
            double c = Math.Cos((a.lat + b.lat) * 0.5 * Math.PI / 180.0);
            return Math.Sqrt(Sq((b.lat - a.lat) * MetresPerDegree) + Sq((b.lon - a.lon) * MetresPerDegree * c));
        }

        static double Sq(double v) => v * v;
        static double Clamp01(double v) => v < 0.0 ? 0.0 : v > 1.0 ? 1.0 : v;
    }
}
