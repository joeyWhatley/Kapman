using Kapman.Chain;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The chart's view of a route: the real kilometres only grow along it, a level's channel maps onto its real
    /// line in order, and at a join the player's mark jumps exactly onto the next level's start (decision 9: no glide).</summary>
    public class RouteMapTests
    {
        [Test]
        public void Monotonic()
        {
            foreach (var d in new[] { ChainSamples.Real(), ChainSamples.BendChain() })
                foreach (var route in d.routes)
                {
                    var map = new RouteMap(d, route);
                    double lastKm = -1.0;
                    for (int i = 0; i < map.legs.Count; i++)
                    {
                        var leg = map.legs[i];
                        double lastU = leg.uStart, lastArc = -1.0;
                        for (int k = 0; k <= 40; k++)
                        {
                            double f = k / 40.0;
                            double km = map.RealKm(i, f);
                            Assert.That(km, Is.GreaterThanOrEqualTo(lastKm - 1e-9), $"{route.id} leg {i}: the real km never fall back");
                            lastKm = km;
                            if (leg.kind != RouteLegKind.Sail)
                                continue;
                            double u = map.LegU(i, f);
                            if (leg.uEnd >= leg.uStart)
                                Assert.That(u, Is.GreaterThanOrEqualTo(lastU - 1e-12), $"{route.id} leg {i}: u runs one way");
                            else
                                Assert.That(u, Is.LessThanOrEqualTo(lastU + 1e-12), $"{route.id} leg {i}: u runs one way");
                            lastU = u;
                            if (leg.onChannel)
                            {
                                // the mark's place along the level's real line follows u
                                double arc = ArcAlong(leg.level, map.ChartPoint(i, f));
                                double want = ArcAlong(leg.level, RouteMap.ChartAt(leg.level, u));
                                Assert.AreEqual(want, arc, 1.0, $"{route.id} leg {i}: the mark sits at u along the chart line");
                                if (lastArc >= 0.0)
                                    Assert.That(leg.uEnd >= leg.uStart ? arc - lastArc : lastArc - arc, Is.GreaterThanOrEqualTo(-1.0));
                                lastArc = arc;
                            }
                        }
                        Assert.AreEqual(map.RealKm(i, 0.0), leg.kmAtStart, 1e-9);
                        if (i + 1 < map.legs.Count)
                            Assert.AreEqual(map.RealKm(i + 1, 0.0), map.RealKm(i, 1.0), 1e-9, $"{route.id}: leg {i} ends where leg {i + 1} begins");
                        if (leg.kind == RouteLegKind.Join && i + 1 < map.legs.Count)
                        {
                            // the player's mark has already jumped: from the first moment of the join it is at the far start
                            var next = map.ChartPoint(i + 1, 0.0);
                            foreach (double f in new[] { 0.0, 0.3, 1.0 })
                            {
                                var mark = map.PlayerMark(i, f);
                                Assert.AreEqual(next.lat, mark.lat, 1e-12, $"{route.id}: the mark jumps across {leg.join.id}");
                                Assert.AreEqual(next.lon, mark.lon, 1e-12);
                            }
                        }
                    }
                    Assert.AreEqual(map.TotalRealKm, map.RealKm(map.legs.Count - 1, 1.0), 1e-9);
                }

            // the real chain's ends sit where they are on the map: Dorestad's berth by Wijk bij Duurstede, the sea's
            // arrival off the Rhine mouth, Lundenwic at the Strand
            var real = ChainSamples.Real();
            var home = new RouteMap(real, real.Route("dorestad_lundenwic"));
            var start = home.ChartPoint(0, 0.0);
            Assert.AreEqual(51.974, start.lat, 0.005);
            Assert.AreEqual(5.348, start.lon, 0.005);
            int sea = home.legs.FindIndex(l => l.level.id == "northsea_south" && l.kind == RouteLegKind.Sail);
            var offKatwijk = home.ChartPoint(sea, 0.0);
            Assert.AreEqual(52.10, offKatwijk.lat, 0.02);
            Assert.AreEqual(4.11, offKatwijk.lon, 0.02);
            var end = home.ChartPoint(home.legs.Count - 1, 1.0);
            Assert.AreEqual(51.511, end.lat, 0.002);
            Assert.AreEqual(-0.121, end.lon, 0.002);
        }

        /// <summary>How far along a level's chart line a point lies (m), by its nearest point on it.</summary>
        static double ArcAlong(ChainLevel level, (double lat, double lon) p)
        {
            const double M = 111195.0;
            var line = level.chartRoute;
            double best = double.MaxValue, bestArc = 0.0, arc = 0.0;
            for (int i = 1; i < line.Count; i++)
            {
                double c = System.Math.Cos(line[i].lat * System.Math.PI / 180.0);
                double ax = line[i - 1].lon * M * c, az = line[i - 1].lat * M, bx = line[i].lon * M * c, bz = line[i].lat * M;
                double px = p.lon * M * c, pz = p.lat * M, dx = bx - ax, dz = bz - az, len2 = dx * dx + dz * dz, len = System.Math.Sqrt(len2);
                double t = len2 > 0.0 ? System.Math.Max(0.0, System.Math.Min(1.0, ((px - ax) * dx + (pz - az) * dz) / len2)) : 0.0;
                double dd = (ax + dx * t - px) * (ax + dx * t - px) + (az + dz * t - pz) * (az + dz * t - pz);
                if (dd < best)
                {
                    best = dd;
                    bestArc = arc + t * len;
                }
                arc += len;
            }
            return bestArc;
        }
    }
}
