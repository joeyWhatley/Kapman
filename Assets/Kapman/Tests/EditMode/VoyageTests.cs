using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The voyage: the crew eat and drink through the stores, go hungry without, are paid in port.</summary>
    public class VoyageTests
    {
        TradeRules rules;
        VoyageRules voyage;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            voyage = VoyageRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/voyage_rules.json"));
            Trade.Begin(rules, 900f, 0.0);
            Voyage.Begin(voyage);
        }

        [Test]
        public void TheCrewEatThroughTheStores()
        {
            var bread = GameData.Good("ships_bread");
            Trade.Hold.Add(bread, 2, 4.0, 0.0);                       // two 20 kg sacks
            double food = Voyage.FoodKg, water = Voyage.DrinkL;
            Assert.AreEqual(40.0, food, 1e-6);
            Assert.AreEqual(voyage.WaterCapacityL, water, 1e-6, "the casks start full");
            Voyage.Day(true, 0.5);
            Assert.AreEqual(food - voyage.men * voyage.foodKg, Voyage.FoodKg, 1e-6, "a day's food eaten");
            Assert.AreEqual(water - voyage.men * voyage.drinkL, Voyage.DrinkL, 1e-6, "a day's water drunk");
            Assert.AreEqual(1.0, Voyage.Health, 1e-6, "fed and watered");
            Assert.AreEqual(voyage.men * voyage.pay, Voyage.PayOwed, 1e-6, "a day's pay owed");
        }

        [Test]
        public void WithoutFoodTheyGoHungry()
        {
            for (int d = 0; d < 3; d++)
                Voyage.Day(true, 0.5);
            Assert.Less(Voyage.Health, 1.0, "no food aboard");
            Assert.Less(Voyage.Morale, 0.8);
        }

        [Test]
        public void InPortTheCasksAreFilledAndTheMenPaid()
        {
            for (int d = 0; d < 5; d++)
                Voyage.Day(true, 0.5);
            Assert.Less(Voyage.DrinkL, voyage.WaterCapacityL);
            double silver = Trade.Purse.Silver(rules);
            var m = new Market(GameData.Port("dorestad"), rules, 793, 0.0);
            Voyage.InPort(m);
            Assert.AreEqual(voyage.WaterCapacityL, Voyage.DrinkL, 1e-6, "casks filled");
            Assert.AreEqual(0.0, Voyage.PayOwed, 0.02, "the men paid");
            Assert.Less(Trade.Purse.Silver(rules), silver, "from the purse");
        }
    }
}
