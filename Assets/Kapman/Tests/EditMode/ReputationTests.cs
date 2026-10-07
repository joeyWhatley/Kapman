using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Standing: selling captives costs dearly among the Franks and barely matters at a Norse market; it counts
    /// fully where done, partly in the region, a little among the same people; it moves the merchant's prices; and a
    /// place that shuns him won't deal.</summary>
    public class ReputationTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Wording.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/wording_793.json"));
            Reputation.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/reputation_793.json"));
            Markets.Clear();
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Reputation.Load(null);                                      // (standing is the game's: don't leave it to other tests)
        }

        [Test]
        public void EachPeopleWeighsADeedItsOwnWay()
        {
            var quentovic = GameData.Port("quentovic");                 // Franks
            var hedeby = GameData.Port("sliesthorp");                  // Northmen
            double qBefore = Reputation.At(quentovic), hBefore = Reputation.At(hedeby);
            Reputation.Deed("sold_captives", quentovic);
            Reputation.Deed("sold_captives", hedeby);
            Assert.Less(Reputation.At(quentovic), qBefore - 5.0, "the Franks: a heavy loss");
            Assert.GreaterOrEqual(Reputation.At(hedeby), hBefore, "the Norse market: no loss");
            // spread: fully at the place, partly in the region
            Assert.Less(Reputation.OfRegion(quentovic.region), 0.0, "the region heard");
            Assert.Greater(Reputation.OfRegion(quentovic.region), Reputation.OfPort(quentovic.id), "less than at the place");
        }

        [Test]
        public void StandingMovesHisPricesAndShunningStopsTrade()
        {
            var quentovic = GameData.Port("quentovic");
            var m = new Market(quentovic, rules, 793, 0.0);
            MarketGood g = null;
            foreach (var x in m.Goods)
                if (x.OnOffer) { g = x; break; }
            float plain = Deal.AsksFor(m, g, 6, "player");
            for (int i = 0; i < 4; i++)
                Reputation.Deed("gave_alms", quentovic);
            Assert.Less(Deal.AsksFor(m, g, 6, "player"), plain, "well regarded: cheaper");
            Assert.AreEqual(m.Asks(g, 6), Deal.AsksFor(m, g, 6, "a trader"), 1e-4, "others pay the market's price");
            for (int i = 0; i < 12; i++)
                Reputation.Deed("fled_kings_men", quentovic);
            Assert.IsTrue(Reputation.Refuses(quentovic), "shunned");
            Trade.Begin(rules, 900f, 0.0);
            var r = Trade.Buy(m, g.good.id, 1, 0.0, 6, 793);
            Assert.IsFalse(r.ok, "they won't deal");
            StringAssert.Contains("Your standing here", Reputation.Line(quentovic));
        }

        [Test]
        public void ADeedIsHeardFarAwayOnlyWhenItsNewsGetsThere()
        {
            var world = Kapman.EditorTools.SimHarness.MakeWorld(793);
            world.Populate();
            Reputation.World = world;
            try
            {
                var quentovic = GameData.Port("quentovic");
                var far = new System.Collections.Generic.Dictionary<string, double>();          // (region: its standing before)
                foreach (var p in GameData.Ports)
                    if (p.region != quentovic.region && Wording.CultureOf(p) == "frankish")
                        far[p.region] = Reputation.OfRegion(p.region);
                double own = Reputation.OfRegion(quentovic.region);
                Reputation.Deed("sold_captives", quentovic, "sold 3 captives");
                Assert.Less(Reputation.OfRegion(quentovic.region), own, "its own region hears at once");
                foreach (var kv in far)
                    Assert.AreEqual(kv.Value, Reputation.OfRegion(kv.Key), 1e-9, "the other Frankish regions: not yet");
                Assert.AreEqual(1, Reputation.TravellingCount, "the news is on its way");
                string news = NewsId(world, quentovic);
                Assert.IsNotNull(news, "posted at Quentovic");
                // the ships carry it: a region's standing changes once a port there knows
                for (int day = 0; day < 90; day++)
                {
                    for (int h = 0; h < 24; h++)
                        world.Hour();
                    Reputation.Daily();
                }
                // (a busy waterfront may have forgotten it again by now: what counts is that it came)
                int reached = 0;
                foreach (var kv in far)
                    if (Reputation.OfRegion(kv.Key) < kv.Value)
                        reached++;
                Assert.Greater(reached, 0, "the news reached some Frankish region in three months, and counted there");
            }
            finally
            {
                Reputation.World = null;
            }
        }

        // the deed's news item, as Quentovic's waterfront has it
        static string NewsId(Kapman.Game.Sim.WorldSim world, PortDef at)
        {
            foreach (var (item, _, _) in world.Board(at.index).News)
                if (item.kind == "deed")
                    return item.id;
            return null;
        }
    }
}
