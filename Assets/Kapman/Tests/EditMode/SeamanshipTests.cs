using System.Collections.Generic;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Seamanship (the user: "Make sure the boats react to each other ... and don't collide"): head-on, crossing,
    /// overtaking, a narrow river and a crowded landing - no two ships ever closer than their passing room, every ship
    /// gets where she's going, and a full landing queues its newcomers at anchor, each in her own place.</summary>
    public class SeamanshipTests
    {
        SimData data;
        SeamanshipRules rules;
        ShipClassDef knarr, barge;
        int portA, portB;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Markets.Clear();
            rules = SeamanshipRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/seamanship_793.json"));
            data = new SimData();
            knarr = new ShipClassDef { id = "t_knarr", name = "test knarr", length = 16f, beam = 4.5f, crew = 6, cargoT = 20, sailKn = 5,
                                       maxKn = 8, hasSail = true, sea = true, coast = true, uses = new[] { "trade_long" } };
            barge = new ShipClassDef { id = "t_barge", name = "test barge", length = 12f, beam = 2.5f, crew = 4, cargoT = 5, sailKn = 3,
                                       maxKn = 5, hasSail = true, river = true, uses = new[] { "trade_long" } };
            data.classes[knarr.id] = knarr;
            data.classes[barge.id] = barge;
            portA = GameData.Port("dorestad").index;
            portB = GameData.Port("lundenwic").index;
        }

        WorldSim World(params double[] speeds)
        {
            var w = new WorldSim(data, new SimRules { dwellMin = 5, dwellMax = 5 }, new StraightRoutes(), new Kapman.Weather.WeatherModel(793),
                                 1, 793, 172, 10.0, rules);
            w.speedOverride = s => speeds[s.index];
            return w;
        }

        SimShip Ship(WorldSim w, ShipClassDef cls, int at, int dest, byte kind, params (double x, double z)[] pts)
        {
            var s = new SimShip
            {
                index = w.ships.Count, id = "test." + w.ships.Count, cls = cls, home = at, homeRegion = "", at = at,
                rng = SimRandom.For(1, "test." + w.ships.Count), purse = new Purse(), hold = new Hold { capacity = cls.CargoKg },
            };
            w.ships.Add(s);
            var path = new List<WorldXZ>();
            var kinds = new List<byte>();
            foreach (var (x, z) in pts)
                path.Add(new WorldXZ(x, z));
            for (int k = 0; k + 1 < path.Count; k++)
                kinds.Add(kind);
            w.SetSail(s, path, kinds, dest);
            return s;
        }

        /// <summary>Runs hours, checking every pair under way or at anchor over each hour; returns the least of
        /// (closest approach / passing room) seen.</summary>
        double Run(WorldSim w, int hours, System.Action<WorldSim> eachHour = null)
        {
            double worst = double.MaxValue;
            for (int h = 0; h < hours; h++)
            {
                w.Hour();
                eachHour?.Invoke(w);
                for (int i = 0; i < w.ships.Count; i++)
                    for (int j = i + 1; j < w.ships.Count; j++)
                    {
                        var a = w.ships[i];
                        var b = w.ships[j];
                        if (!Out(a) || !Out(b) || (a.state == ShipState.Anchored && b.state == ShipState.Anchored))
                            continue;                                       // (an anchorage packs closer than ships under way pass)
                        double room = rules.PassM(Seamanship.KindOf(a), Seamanship.KindOf(b), a.cls, b.cls);
                        double d = Seamanship.Closest(a.prevPos, a.pos, b.prevPos, b.pos);
                        if (d / room < worst)
                        {
                            worst = d / room;
                            worstWhat = $"hour {h}: {a.index} {a.state} ({a.prevPos.x:0},{a.prevPos.z:0})->({a.pos.x:0},{a.pos.z:0}) done {a.done:0}/{a.total:0} and " +
                                        $"{b.index} {b.state} ({b.prevPos.x:0},{b.prevPos.z:0})->({b.pos.x:0},{b.pos.z:0}) done {b.done:0}/{b.total:0}; {d:0} m of {room:0}";
                        }
                    }
            }
            return worst;
        }

        static bool Out(SimShip s) => s.state == ShipState.Sailing || s.state == ShipState.Anchored;
        string worstWhat = "";

        [Test]
        public void HeadOnTheyPassPortToPort()
        {
            var w = World(2.5, 2.5);
            var a = Ship(w, knarr, portA, portB, LaneGraph.Sea, (0, 0), (60000, 0));
            var b = Ship(w, knarr, portB, portA, LaneGraph.Sea, (60000, 0), (0, 0));
            double worst = Run(w, 16);
            Assert.GreaterOrEqual(worst, 1.0, "closest / passing room");
            Assert.AreEqual(ShipState.InPort, a.state, "she got there");
            Assert.AreEqual(ShipState.InPort, b.state, "and so did she");
            Assert.AreEqual(0, w.stats.closeCalls);
        }

        [Test]
        public void WithKeepClearOffTheyPassThroughEachOther()
        {
            // the game's setting for now (the user, 2026-10-03: "make it so the ships phase through each other ATM")
            var w = World(2.5, 2.5);
            w.keepClear = false;
            var a = Ship(w, knarr, portA, portB, LaneGraph.Sea, (0, -30000), (0, 30000));
            var b = Ship(w, knarr, portB, portA, LaneGraph.Sea, (-30000, 0), (30000, 0));
            Run(w, 24);
            Assert.AreEqual(0, w.stats.giveWays, "nobody gave way");
            Assert.AreEqual(0, w.stats.standOuts, "nobody stood out");
            Assert.AreEqual(ShipState.InPort, a.state);
            Assert.AreEqual(ShipState.InPort, b.state);
        }

        [Test]
        public void CrossingTheOneWithTheOtherOnHerStarboardGivesWay()
        {
            var w = World(2.5, 2.5);
            var a = Ship(w, knarr, portA, portB, LaneGraph.Sea, (0, -30000), (0, 30000));      // northward
            var b = Ship(w, knarr, portB, portA, LaneGraph.Sea, (-30000, 0), (30000, 0));      // eastward: a on her starboard
            double worst = Run(w, 24);
            Assert.GreaterOrEqual(worst, 1.0, "closest / passing room");
            Assert.Greater(w.stats.giveWays, 0, "someone gave way");
            Assert.AreEqual(ShipState.InPort, a.state);
            Assert.AreEqual(ShipState.InPort, b.state);
            Assert.AreEqual(0, w.stats.closeCalls);
        }

        [Test]
        public void TheOvertakingShipKeepsClearAndGetsBy()
        {
            var w = World(1.5, 3.0);
            var slow = Ship(w, knarr, portA, portB, LaneGraph.Sea, (0, 0), (100000, 0));
            var fast = Ship(w, knarr, portA, portB, LaneGraph.Sea, (-15000, 0), (0, 0), (100000, 0));
            bool fastFirst = false;
            double worst = Run(w, 40, sim =>
            {
                if (fast.state == ShipState.InPort && slow.state == ShipState.Sailing)
                    fastFirst = true;
            });
            Assert.GreaterOrEqual(worst, 1.0, "closest / passing room");
            Assert.IsTrue(fastFirst, "the faster ship got by and arrived first");
            Assert.Greater(w.stats.standOuts, 0, "she stood out to pass");
            Assert.AreEqual(0, w.stats.closeCalls);
        }

        [Test]
        public void InANarrowRiverTheyKeepTheirSides()
        {
            var w = World(1.5, 1.5);
            var a = Ship(w, barge, portA, portB, LaneGraph.River, (0, 0), (6000, 2000), (12000, 0), (20000, 3000));
            var b = Ship(w, barge, portB, portA, LaneGraph.River, (20000, 3000), (12000, 0), (6000, 2000), (0, 0));
            double worst = Run(w, 12);
            Assert.GreaterOrEqual(worst, 1.0, "closest / passing room");
            Assert.AreEqual(ShipState.InPort, a.state);
            Assert.AreEqual(ShipState.InPort, b.state);
        }

        [Test]
        public void TheyKeepClearOfTheMerchantsShip()
        {
            var w = World(2.5);
            var other = Ship(w, knarr, portA, portB, LaneGraph.Sea, (0, -30000), (0, 30000));     // northward, across her way
            // the merchant's ship: eastward along z = 0 at 2.5 m/s, meeting her at the crossing; she never alters
            double perHour = 2.5 * 60.0 * SimUnits.Across;
            double x = -30000.0 - perHour;
            w.playerNow = () => (new WorldXZ(x, 0.0), new WorldXZ(perHour, 0.0));
            double worst = double.MaxValue;
            for (int h = 0; h < 24; h++)
            {
                x += perHour;
                w.Hour();
                var me0 = new WorldXZ(x, 0.0);
                var me1 = new WorldXZ(x + perHour, 0.0);
                // over this hour: her move against the merchant's
                double room = rules.PassM(LaneGraph.Sea, LaneGraph.Sea, knarr, w.Player.cls);
                if (other.state == ShipState.Sailing)
                    worst = System.Math.Min(worst, Seamanship.Closest(other.prevPos, other.pos, me0, me1) / room);
            }
            Assert.GreaterOrEqual(worst, 0.95, "closest / passing room");
            Assert.Greater(w.stats.giveWays, 0, "she gave way to him");
            Assert.AreEqual(ShipState.InPort, other.state, "and still got there");
        }

        [Test]
        public void AFullLandingQueuesItsNewcomersAtAnchor()
        {
            int ships = 6;
            var speeds = new double[ships];
            for (int i = 0; i < ships; i++)
                speeds[i] = 2.0 + 0.1 * i;
            var w = World(speeds);
            for (int i = 0; i < ships; i++)
            {
                double ang = i * Mathf.PI / 7.0, r = 25000.0 + 1500.0 * i;          // from different airts, all to one landing
                Ship(w, knarr, portA, portB, LaneGraph.Sea, (r * System.Math.Cos(ang), r * System.Math.Sin(ang)), (0, 0));
            }
            int most = 0, everAnchored = 0;
            var berthed = new HashSet<int>();
            var inSince = new Dictionary<int, double>();
            double worst = Run(w, 96, sim =>
            {
                int here = 0;
                foreach (var s in sim.ships)
                {
                    if (s.state == ShipState.InPort && s.at == portB)
                    {
                        here++;
                        berthed.Add(s.index);
                        if (!inSince.ContainsKey(s.index))
                            inSince[s.index] = sim.Hours;
                        else if (sim.Hours - inSince[s.index] >= 8.0)
                            s.state = ShipState.LaidUp;                         // (she's done her business and gone)
                    }
                    if (s.state == ShipState.Anchored)
                        everAnchored++;
                }
                most = System.Math.Max(most, here);
                // no two at anchor in the same place
                foreach (var p in sim.ships)
                    foreach (var q in sim.ships)
                        if (p.index < q.index && p.state == ShipState.Anchored && q.state == ShipState.Anchored)
                            Assert.Greater(WorldXZ.Distance(p.pos, q.pos), 300.0, "two ships anchored in one place");
            });
            Assert.LessOrEqual(most, w.Berths(portB), "never more visitors than berths");
            Assert.Greater(everAnchored, 0, "some had to anchor off");
            Assert.AreEqual(ships, berthed.Count, "every ship got her berth in the end");
            Assert.GreaterOrEqual(worst, 1.0, "closest / passing room: " + worstWhat);
        }
    }
}
