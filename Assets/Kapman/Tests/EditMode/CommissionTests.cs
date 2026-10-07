using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Orders: places post them by their kind, above the market; taken and delivered they pay and raise your
    /// standing; broken they lower it.</summary>
    public class CommissionTests
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
            Commissions.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/commissions_793.json"));
            Markets.Clear();
            Trade.Begin(rules, 9000f, 0.0);
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Reputation.Load(null);
            Commissions.Load(null);
        }

        [Test]
        public void OrdersArePostedKeptAndBroken()
        {
            for (int d = 0; d < 60; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.Greater(Commissions.All.Count, 0, "orders posted in two months");
            var c = Commissions.All[0];
            var port = GameData.Port(c.port);
            Assert.IsTrue(port.kind == "monastery" || port.kind == "royal seat" || port.kind == "town" || port.kind == "emporium");
            Commissions.Take(c);
            Assert.AreEqual(1, Commissions.Mine.Count);
            // delivered: paid, gone, and his standing there rises
            double standing = Reputation.At(port), silver = Trade.Purse.Silver(rules);
            Trade.Hold.Add(GameData.Good(c.good), c.units, 0.0, 0.0);
            string fact = Commissions.Deliver(port);
            StringAssert.StartsWith("Delivered", fact);
            Assert.AreEqual(0, Commissions.Mine.Count);
            Assert.Greater(Trade.Purse.Silver(rules), silver, "paid");
            Assert.Greater(Reputation.At(port), standing, "kept: standing up");
            // another taken and let run out: standing down
            var other = Commissions.All.FirstOrDefault(x => !x.mine);
            if (other != null)
            {
                Commissions.Take(other);
                var there = GameData.Port(other.port);
                double before = Reputation.At(there);
                Commissions.Daily(other.deadline + 1.0, 793, 9, 793);
                Assert.IsFalse(Commissions.All.Contains(other), "broken");
                Assert.Less(Reputation.At(there), before, "broken: standing down");
            }
        }
    }
}
