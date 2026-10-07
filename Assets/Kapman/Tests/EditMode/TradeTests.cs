using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The trade's rules: the tables read, money paid and changed by zone, prices answering stock and gluts,
    /// the hold's room, and buying and selling end to end.</summary>
    public class TradeTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
        }

        [Test]
        public void TheTablesRead()
        {
            Assert.GreaterOrEqual(GameData.Goods.Count, 140);
            var wheat = GameData.Good("wheat");
            Assert.NotNull(wheat);
            Assert.AreEqual(3f, wheat.MadeIn("neustria"));
            Assert.AreEqual(3f, wheat.WantedIn("frisia"));
            CollectionAssert.Contains(wheat.cheap, 9);
            Assert.AreEqual("arms", GameData.Good("swords").law);
            Assert.AreEqual("frankish", rules.ZoneOf("rhine_delta").id);
            Assert.AreEqual("weight", rules.ZoneOf("svealand").id);
            Assert.AreEqual(4, rules.coins.Count);
            Assert.IsNotEmpty(GameData.Port("dorestad").region);
            // every region a good names is a region of the table
            foreach (var g in GameData.Goods)
                foreach (var r in g.made.Keys.Concat(g.wanted.Keys))
                    Assert.IsTrue(GameData.Regions.ContainsKey(r), $"{g.id}: unknown region {r}");
        }

        [Test]
        public void ThePursePaysInTheLocalCoinFirst()
        {
            var frankish = rules.ZoneOf("rhine_delta");
            var p = new Purse();
            p.Add("denier", 10);
            p.Add("penny", 10);
            Assert.IsTrue(p.Pay(8, frankish, rules, out _));
            Assert.AreEqual(2, p.Count("denier"));
            Assert.AreEqual(10, p.Count("penny"), "the foreign coin is kept while the local will do");
            // now the pennies go, at their silver less the mint's tenth, with the change in deniers
            Assert.IsTrue(p.Pay(5, frankish, rules, out string how));
            Assert.Less(p.Count("penny"), 10);
            StringAssert.Contains("paid", how);
            Assert.IsFalse(p.Pay(1000, frankish, rules, out _), "it can't pay more than it has");
        }

        [Test]
        public void SilverIsWorthMoreWhereItGoesByWeight()
        {
            var p = new Purse();
            p.Add("denier", 10);
            double inFrancia = p.Worth(rules.ZoneOf("rhine_delta"), rules);
            double inEngland = p.Worth(rules.ZoneOf("england_south"), rules);
            Assert.AreEqual(10.0, inFrancia, 1e-6, "deniers are good money in Francia");
            Assert.Less(inEngland, inFrancia, "and lose at the English moneyer's");
            var cut = new Purse();
            cut.Add("hacksilver", 17);                              // 17 g = 10 deniers' silver
            Assert.AreEqual(10.0, cut.Worth(rules.ZoneOf("svealand"), rules), 0.01, "cut silver at full weight in the north");
        }

        [Test]
        public void PricesAnswerStock()
        {
            var market = new Market(GameData.Port("dorestad"), rules, 793, 0);
            var g = market.Goods.First(x => x.usual > 0f);
            g.stock = g.usual;
            float usual = market.Asks(g, 6);
            g.stock = g.usual * 0.25f;
            Assert.Greater(market.Asks(g, 6), usual, "scarce is dear");
            g.stock = g.usual * 3f;
            Assert.Less(market.Asks(g, 6), usual, "plenty is cheap");
            Assert.Less(market.Pays(g, 6), market.Asks(g, 6), "a market pays less than it asks");
            // a week on, the stock has settled back towards the usual
            market.CatchUp(7);
            Assert.Less(g.stock, g.usual * 3f);
        }

        [Test]
        public void SellingGlutsAMarket()
        {
            var market = new Market(GameData.Port("dorestad"), rules, 793, 0);
            var g = market.Goods.First(x => x.wanted > 0f && x.usual <= 0f);
            float before = market.Pays(g, 6);
            for (int i = 0; i < 50; i++)
                market.GotOne(g);
            Assert.Less(market.Pays(g, 6), before, "the more you sell, the less they pay");
        }

        [Test]
        public void TheHoldFills()
        {
            var hold = new Hold { capacity = 100f };
            var quern = GameData.Good("quernstones");
            int room = hold.RoomFor(quern);
            Assert.AreEqual((int)(100f / quern.Space), room);
            hold.Add(quern, room, room * 12.0, 0);
            Assert.AreEqual(0, hold.RoomFor(quern));
            Assert.AreEqual(12.0, hold.CostEach("quernstones"), 1e-6);
            Assert.AreEqual(12.0, hold.Remove(quern, 1), 1e-6);
        }

        [Test]
        public void BuyingAndSellingEndToEnd()
        {
            Trade.Begin(rules, 900f, 0);
            var market = new Market(GameData.Port("dorestad"), rules, 793, 0);
            var g = market.Goods.First(x => x.OnOffer && x.good.value < 10f);
            double worth = Trade.Purse.Worth(market.zone, rules);
            var bought = Trade.Buy(market, g.good.id, 3, 0, 6, 793);
            Assert.IsTrue(bought.ok, bought.text);
            Assert.AreEqual(3, Trade.Hold.Count(g.good.id));
            Assert.AreEqual(worth - bought.deniers, Trade.Purse.Worth(market.zone, rules), 1.0);
            var sold = Trade.Sell(market, g.good.id, 3, 0, 6, 793);
            Assert.IsTrue(sold.ok, sold.text);
            Assert.AreEqual(0, Trade.Hold.Count(g.good.id));
            Assert.Greater(sold.toll, 0.0, "the king's tenth on a sale at Dorestad");
            Assert.Less(sold.deniers - sold.toll, bought.deniers, "buying and selling straight back loses money");
        }
    }
}
