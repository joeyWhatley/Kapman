using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Phase 6's depth: news passes between ships and waterfronts once each, a fair fills its market and is
    /// talked of, the moneyer changes foreign coin at the zone's loss, and the waterfronts start knowing nearby prices.</summary>
    public class PortLifeTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Markets.Clear();
            Markets.OpenAll(793, 0.0);
        }

        [TearDown]
        public void Done() => Markets.Clear();

        [Test]
        public void NewsPassesOnceEachWay()
        {
            var ship = new Knowledge();
            var board = new Knowledge();
            var item = new NewsItem { id = "raid.1", kind = "raid", text = "Raiders struck a place.", port = 1, day = 3.0 };
            board.Hear(item, 3.0, "the waterfront");
            Knowledge.SwapNews(ship, board, 5.0, "a ship", "the waterfront at X");
            Assert.AreEqual(1, ship.News.Count(), "the ship heard it");
            Assert.AreEqual("the waterfront at X", ship.News.First().from);
            Knowledge.SwapNews(ship, board, 6.0, "a ship", "the waterfront at X");
            Assert.AreEqual(1, board.News.Count(), "not twice");
        }

        WorldSim World()
        {
            var data = SimData.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ship_classes_793.json"),
                                    AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/traffic_793.json"));
            return new WorldSim(data, new SimRules(), new StraightRoutes(), new Kapman.Weather.WeatherModel(793), 1, 793, 172, 10.0);
        }

        [Test]
        public void AFairFillsItsMarketAndIsTalkedOf()
        {
            var w = World();
            var parisius = GameData.Port("parisius");
            w.fairs = new List<FairDef> { new FairDef { port = "parisius", name = "the fair", startDay = w.Date.dayOfYear, days = 20, stockX = 1.5f, dearer = -0.1f } };
            var m = w.MarketAt(parisius.index);
            float before = m.Goods.Sum(g => g.stock);
            Fairs.Daily(w);
            Assert.Greater(m.Goods.Sum(g => g.stock), before * 1.4f, "goods brought in");
            Assert.IsTrue(w.Board(parisius.index).News.Any(n => n.item.kind == "fair"), "and it's talked of there");
            Assert.IsNotNull(Fairs.On(w, parisius.index));
        }

        [Test]
        public void TheWaterfrontsStartKnowingNearbyPrices()
        {
            var w = World();
            w.Post("raid", "Raiders struck Lindisfarena.", GameData.Port("lindisfarena").index, w.Day - 20.0);
            w.SeedKnowledge();
            var dorestad = GameData.Port("dorestad").index;
            Assert.Greater(w.Board(dorestad).Count, 10, "prices of the ports round Dorestad");
            Assert.IsTrue(w.Board(GameData.Port("eoforwic").index).News.Any(n => n.item.kind == "raid"), "York has heard of Lindisfarne");
        }

        [Test]
        public void TheMoneyerChangesForeignCoinAtALoss()
        {
            Trade.Begin(rules, 900f, 0.0);
            Trade.Purse.Add("dirham", 10.0);
            var dorestad = GameData.Port("dorestad");
            var zone = rules.ZoneOf(dorestad.region);
            double before = Trade.Purse.Silver(rules);
            string fact = PortLife.Errand(dorestad, "port.moneyer", out bool town);
            Assert.IsFalse(town);
            StringAssert.StartsWith("You change", fact);
            Assert.AreEqual(0.0, Trade.Purse.Count("dirham"), 1e-9, "the dirhams changed");
            Assert.Less(Trade.Purse.Silver(rules), before, "at a loss");
            Assert.Greater(Trade.Purse.Silver(rules), before * (1.0 - zone.foreignLoss) - 1.0, "no more than the zone's loss");
        }
    }
}
