using System.Linq;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Raiders: the odds of a fight, a raid on a monastery (stripped, dear, captives taken, left alone a while),
    /// a ship taken at sea (most of her cargo, half her silver), and a season of them in the year's run.</summary>
    public class RaiderTests
    {
        RaiderRules rules;
        SimData data;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            rules = RaiderRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/raiders_793.json"));
            data = SimData.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ship_classes_793.json"),
                                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/traffic_793.json"));
            Markets.Clear();
            Markets.OpenAll(793, 0.0);
        }

        [TearDown]
        public void Done() => Markets.Clear();

        [Test]
        public void FourMenAgainstThirtyRarelyWin()
        {
            Assert.Less(RaiderEncounter.FightOdds(4, 30), 0.05);
            Assert.AreEqual(0.5, RaiderEncounter.FightOdds(30, 30), 1e-9);
            Assert.Greater(rules.origins.Count, 0, "the table's origins");
            Assert.AreEqual(2.0, rules.BandsIn(793), 1e-9, "793: two bands");
        }

        SimShip Band(WorldSim w, int men)
        {
            var o = rules.origins[0];
            var home = GameData.Port(o.home);
            var b = new SimShip
            {
                index = w.ships.Count, id = "test.band", cls = data.classes[o.cls], home = home.index, at = home.index,
                rng = SimRandom.For(1, "test.band"), purse = new Purse(), hold = new Hold { capacity = data.classes[o.cls].CargoKg * 2 },
                raider = true, origin = o.id, men = men, bandShips = 2,
            };
            w.ships.Add(b);
            return b;
        }

        [Test]
        public void ARaidStripsAMonasteryAndTakesCaptives()
        {
            var w = new WorldSim(data, new SimRules(), new StraightRoutes(), new Kapman.Weather.WeatherModel(793), 1, 793, 172, 10.0, null, rules);
            var band = Band(w, 60);
            var lind = GameData.Port("lindisfarena");
            var m = w.MarketAt(lind.index);
            float before = m.Goods.Sum(g => g.stock);
            RaiderBrain.Raid(w, band, lind.index);
            Assert.Less(m.Goods.Sum(g => g.stock), before * 0.6f, "the market stripped");
            Assert.Greater(band.hold.Count("captives"), 0, "captives taken");
            Assert.Greater(w.RaidedUntil(lind.index), w.Day + 30.0, "left alone a while after");
            Assert.IsTrue(w.stats.raids[0].done);
            // a strong place: they sheer off
            var dorestad = GameData.Port("dorestad");
            int raids = w.stats.raids.Count;
            RaiderBrain.Raid(w, Band(w, 30), dorestad.index);
            Assert.IsFalse(w.stats.raids[raids].done, "too strong for 30 men");
        }

        [Test]
        public void ATraderTakenLosesMostOfHerCargoAndHalfHerSilver()
        {
            var w = new WorldSim(data, new SimRules(), new StraightRoutes(), new Kapman.Weather.WeatherModel(793), 1, 793, 172, 10.0, null, rules);
            var band = Band(w, 60);
            var prey = new SimShip
            {
                index = w.ships.Count, id = "test.prey", cls = data.classes["north_sea_merchant_ship"], home = 0, at = 0,
                rng = SimRandom.For(1, "test.prey"), purse = new Purse(), hold = new Hold { capacity = 20000f },
            };
            w.ships.Add(prey);
            prey.hold.Add(GameData.Good("wheat"), 50, 100.0, 0.0);
            prey.purse.Add("hacksilver", 170.0);
            double silver = prey.purse.Silver(TradeRules.Current);
            RaiderBrain.Take(w, band, prey);
            Assert.AreEqual(50 - (int)System.Math.Ceiling(50 * rules.takeShare), prey.hold.Count("wheat"), "most of her cargo gone");
            Assert.AreEqual(silver * (1.0 - rules.silverShare), prey.purse.Silver(TradeRules.Current), silver * 0.02, "half her silver");
            Assert.Greater(band.hold.Count("wheat"), 0);
            Assert.AreEqual(1, w.stats.shipsTaken);
        }

        [Test]
        public void ASeasonOfRaidersInTheYearsRun()
        {
            SimHarness.RunYear(793, 100, out var sim, false, 5.0);
            Assert.Greater(sim.ships.Count(s => s.raider), 3, "bands out (x5)");
            Assert.Greater(sim.stats.raids.Count(r => r.done), 0, "places raided or ships taken");
            Assert.Greater(sim.stats.voyages.Count, 100, "the traders still trade");
        }
    }
}
