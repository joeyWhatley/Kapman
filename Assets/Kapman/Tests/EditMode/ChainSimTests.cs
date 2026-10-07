using System.Collections.Generic;
using Kapman.Chain;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The world's ships on the chain: they keep the player's pace (decision 3), take the same date jumps at the
    /// joins (decision 9), and their real kilometres are derived from their chain progress.</summary>
    public class ChainSimTests
    {
        /// <summary>The player's voyage as the player lives it: each level sailed at 1x, and at each join the deck beat
        /// played and the clock jumped by the reach (the tick ladder replays the hours). Game hours from casting off.</summary>
        static double PlayerArrives(RouteMap map, ChainShip player)
        {
            double clock = 0.0;
            foreach (var leg in map.legs)
            {
                if (leg.kind == RouteLegKind.Sail)
                    clock += leg.metres / ChainSim.SpeedIn(leg, player) / 60.0;    // a real second at 1x is a game minute
                else
                {
                    clock += leg.join.BeatSeconds / 60.0;
                    clock += ChainSim.JumpHours(leg.join, player.reachMps);        // the date jumps at the cut
                }
            }
            return clock;
        }

        /// <summary>A sim ship stepped on as the world's sim does (by the hour, or finer), until she arrives: the game hours
        /// from casting off to the end of the step she arrived in. Her real km after each step go in the list if one is given.</summary>
        static double SimArrives(RouteMap map, ChainShip ship, double step, List<double> kmEachStep = null)
        {
            var p = new ChainProgress();
            double hours = 0.0;
            while (!p.arrived)
            {
                ChainSim.Advance(map, ship, ref p, step);
                hours += step;
                kmEachStep?.Add(ChainSim.RealKm(map, ship, p));
                Assert.Less(hours, 10000.0, "she never arrived");
            }
            return hours;
        }

        [Test]
        public void TraderKeepsPace()
        {
            var d = ChainSamples.Real();
            var map = new RouteMap(d, d.Route("dorestad_lundenwic"));
            var player = ChainShip.Player(d);

            // a trader of the player's class leaving Dorestad in the same hour makes Lundenwic within the hour
            var twin = player.Scaled("frisian_trader", 1.0);
            double playerHours = PlayerArrives(map, player);
            double traderHours = SimArrives(map, twin, 0.05);
            Assert.AreEqual(playerHours, traderHours, 1.0, "the trader and the player arrive together");

            // a slower trader (3.5 knots against the player's 5) arrives later by the speeds' ratio (the deck beats'
            // few minutes aside), less a little where the stream carries both alike: leg by leg, her own speeds
            var slow = player.Scaled("slow_trader", 3.5 / 5.0);
            double slowHours = SimArrives(map, slow, 0.05);
            double beats = 0.0;
            foreach (var leg in map.legs)
                if (leg.kind == RouteLegKind.Join)
                    beats += leg.join.BeatSeconds / 60.0;
            Assert.AreEqual(PlayerArrives(map, slow), slowHours, 0.1, "the slow trader keeps her own pace leg by leg");
            Assert.That((slowHours - beats) / (traderHours - beats), Is.InRange(1.3, 5.0 / 3.5 + 0.01), "about the speeds' ratio");

            // and overhauled where the player expects: half way through the player's voyage, both are at one place
            var p = new ChainProgress();
            var q = new ChainProgress();
            ChainSim.Advance(map, player, ref p, playerHours * 0.5);
            ChainSim.Advance(map, twin, ref q, playerHours * 0.5);
            Assert.AreEqual(ChainSim.RealKm(map, player, p), ChainSim.RealKm(map, twin, q), 1e-6);
        }

        [Test]
        public void RealKmIsDerived()
        {
            var d = ChainSamples.Real();
            foreach (var route in d.routes)
            {
                var map = new RouteMap(d, route);
                var ship = ChainShip.Player(d).Scaled("trader", 0.9);
                var km = new List<double>();
                SimArrives(map, ship, 1.0, km);
                for (int i = 1; i < km.Count; i++)
                    Assert.That(km[i], Is.GreaterThanOrEqualTo(km[i - 1] - 1e-9), $"{route.id}: hour {i}: the real km never fall back");
                Assert.AreEqual(map.TotalRealKm, km[km.Count - 1], 1e-6, "she ends at the route's real length");

                // at each anchor (the start of each leg) her real km is the sum of the legs' real km before it
                double sum = 0.0;
                for (int i = 0; i < map.legs.Count; i++)
                {
                    var p = new ChainProgress();
                    double hours = 0.0;
                    for (int k = 0; k < i; k++)
                        hours += ChainSim.LegHours(map.legs[k], ship);
                    ChainSim.Advance(map, ship, ref p, hours + 1e-9);
                    Assert.AreEqual(sum, ChainSim.RealKm(map, ship, p), 1e-3, $"{route.id}: at the start of leg {i}");
                    sum += map.legs[i].realKm;
                }
                Assert.AreEqual(sum, map.TotalRealKm, 1e-9);
            }

            // she has a body only where she is sailing a built level, at a point inside it
            var home = new RouteMap(d, d.Route("dorestad_lundenwic"));
            var trader = ChainShip.Player(d);
            var at = new ChainProgress();
            ChainSim.Advance(home, trader, ref at, 5.0);
            Assert.IsTrue(ChainSim.LocalPoint(home, trader, at, out var level, out var x, out var z));
            Assert.AreEqual("dorestad", level.id);
            Assert.That(x, Is.InRange(0.0, 5120.0));
            Assert.That(z, Is.InRange(0.0, 5120.0));
            ChainSim.Advance(home, trader, ref at, home.legs[0].metres / trader.riverMps / 60.0);   // into the Rhine's reach
            Assert.AreEqual(RouteLegKind.Join, home.legs[at.leg].kind);
            Assert.IsFalse(ChainSim.LocalPoint(home, trader, at, out _, out _, out _), "no body in a cut-out reach");
        }

        [Test]
        public void JumpHoursAddTheCutReach()
        {
            var d = ChainSamples.Real();
            var mouth = d.Join("rhine_mouth");

            // decision 9: the date jumps by the cut-out reach's real sailing time
            Assert.AreEqual(mouth.realKm / (2.572 * 3.6), ChainSim.JumpHours(mouth, 2.572), 1e-9);
            Assert.AreEqual(2.0 * ChainSim.JumpHours(mouth, 2.572), ChainSim.JumpHours(mouth, 1.286), 1e-9, "half the speed, twice the hours");
            Assert.AreEqual(0.0, ChainSim.JumpHours(new ChainJoin { id = "a_strait", kind = "strait", realKm = 0.0 }, 2.572), "a strait in both seas cuts nothing");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ChainSim.JumpHours(mouth, 0.0));

            // the 430 km to London at 5 knots is about 46 hours (the user's figure)
            Assert.AreEqual(46.0, 430.0 / (2.572 * 3.6), 0.5);
            // the whole voyage's date: what is played plus what jumps; the jumps alone are the cut-out reaches at 5 knots
            var map = new RouteMap(d, d.Route("dorestad_lundenwic"));
            var player = ChainShip.Player(d);
            double jumps = ChainSim.RouteJumpHours(map, player);
            double cutKm = 0.0;
            foreach (var leg in map.legs)
                if (leg.kind == RouteLegKind.Join)
                    cutKm += leg.join.realKm;
            Assert.AreEqual(cutKm / (player.reachMps * 3.6), jumps, 1e-9);
            Assert.That(cutKm, Is.GreaterThan(150.0), "the Rhine and the Thames below their places are cut out");
            Assert.AreEqual(ChainSim.PlayMinutes(map, player) + jumps, ChainSim.RouteHours(map, player), 1e-9,
                            "a minute of play is a game hour (1 s = 1 game minute): the voyage's hours are the minutes played and the jumps");
            // the date at Lundenwic lands within a day of the real voyage under way (46 hours) and later than the played 33
            Assert.That(ChainSim.RouteHours(map, player), Is.InRange(46.0 - 24.0, 46.0 + 24.0));
            Assert.That(ChainSim.RouteHours(map, player), Is.GreaterThan(ChainSim.PlayMinutes(map, player)));
        }
    }
}
