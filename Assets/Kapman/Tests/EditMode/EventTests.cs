using System.Collections.Generic;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Events: the table reads, every option has effects that name real goods, and effects do what they say.</summary>
    public class EventTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            GameEvents.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/events_793.json"));
            Voyage.Begin(VoyageRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/voyage_rules.json")));
            Trade.Begin(rules, 900f, 0.0);
        }

        [Test]
        public void TheTableReadsAndNamesRealGoods()
        {
            Assert.Greater(GameEvents.All.Count, 8);
            foreach (var e in GameEvents.All)
            {
                Assert.IsFalse(string.IsNullOrEmpty(e.title), e.id + ": a title");
                foreach (var o in e.options)
                {
                    Assert.IsNotNull(o.effects, $"{e.id}.{o.key}: effects");
                    foreach (var fx in new[] { o.effects, Json.Obj(o.effects, "success"), Json.Obj(o.effects, "failure") })
                        foreach (var kv in Json.Obj(fx, "goods") ?? new Dictionary<string, object>())
                            Assert.IsNotNull(GameData.Good(kv.Key), $"{e.id}.{o.key}: the good '{kv.Key}'");
                }
            }
        }

        [Test]
        public void EffectsDoWhatTheySay()
        {
            int men = Voyage.Men;
            double health = Voyage.Health;
            string fact = GameEvents.Apply(new Dictionary<string, object>
            {
                ["men"] = -1.0, ["health"] = -0.1, ["goods"] = new Dictionary<string, object> { ["dried_fish"] = 2.0 }, ["text"] = "Done.",
            });
            Assert.AreEqual("Done.", fact);
            Assert.AreEqual(men - 1, Voyage.Men, "a man lost");
            Assert.AreEqual(health - 0.1, Voyage.Health, 1e-9);
            Assert.AreEqual(2, Trade.Hold.Count("dried_fish"), "the catch aboard");
            double water = Voyage.DrinkL;
            GameEvents.Apply(new Dictionary<string, object> { ["water"] = -60.0 });
            Assert.AreEqual(water - 60.0, Voyage.DrinkL, 1e-6, "60 litres gone");
            double silver = Trade.Purse.Silver(rules);
            GameEvents.Apply(new Dictionary<string, object> { ["silver"] = -10.0 });
            Assert.AreEqual(silver - 10.0, Trade.Purse.Silver(rules), 1.0, "10 deniers paid, change given");
        }
    }
}
