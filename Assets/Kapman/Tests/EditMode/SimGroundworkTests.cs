using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Weather;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The world simulation's groundwork: the calendar, the units, the dice, the weather for many places at
    /// once, and every merchant dealing with the same market the same way.</summary>
    public class SimGroundworkTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Markets.Clear();
        }

        [Test]
        public void TheYearTurnsOver()
        {
            var start = Calendar.At(793, 172, 0.0);
            Assert.AreEqual((793, 6, 21), (start.year, start.month, start.day), "day 172 of 793 is 21 June");
            var newYear = Calendar.At(793, 365, 24.0 + 1.0);
            Assert.AreEqual((794, 1, 1), (newYear.year, newYear.month, newYear.day), "the day after 31 December 793");
            var leap = Calendar.At(796, 60, 0.0);
            Assert.AreEqual((2, 29), (leap.month, leap.day), "796 is a leap year (Julian)");
            Assert.AreEqual(365 + 366, Calendar.At(795, 1, (365 + 366) * 24.0).dayOfYear - 1 + 365 + 366, "two years on");
            Assert.AreEqual(797, Calendar.At(795, 1, (365 + 366) * 24.0).year);
        }

        [Test]
        public void AGameHourAtFiveKnots()
        {
            Assert.AreEqual(4939.0, SimUnits.RealMetresPerGameHour(SimUnits.Knots(5.0)), 1.0);
        }

        [Test]
        public void EachShipHasItsOwnDice()
        {
            var a = SimRandom.For(793, "dorestad.1");
            var b = SimRandom.For(793, "dorestad.1");
            var c = SimRandom.For(793, "dorestad.2");
            var d = SimRandom.For(794, "dorestad.1");
            for (int i = 0; i < 100; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), "the same seed and ship, the same draws");
            Assert.AreNotEqual(SimRandom.For(793, "dorestad.1").NextUInt(), c.NextUInt());
            Assert.AreNotEqual(SimRandom.For(793, "dorestad.1").NextUInt(), d.NextUInt());
            double sum = 0;
            var r = SimRandom.For(1, "spread");
            for (int i = 0; i < 10000; i++)
                sum += r.Next();
            Assert.AreEqual(0.5, sum / 10000.0, 0.02);
        }

        [Test]
        public void TheWeatherForManyPlacesIsTheSame()
        {
            var w = new WeatherModel(793);
            var r = SimRandom.For(7, "weather test");
            for (int i = 0; i < 10; i++)
            {
                double lat = r.Range(48.0, 62.0), lon = r.Range(-6.0, 20.0), hours = r.Range(0.0, 24.0 * 400.0);
                bool sea = r.Chance(0.5);
                var one = w.Sample(lat, lon, hours, sea);
                var many = w.Sample(w.Snapshot(hours), lat, lon, hours, sea);
                Assert.AreEqual(one.windSpeed, many.windSpeed, 1e-6f);
                Assert.AreEqual(one.windFrom, many.windFrom, 1e-6f);
                Assert.AreEqual(one.pressure, many.pressure, 1e-6f);
            }
        }

        [Test]
        public void EveryMerchantDealsTheSameWay()
        {
            Reputation.Load(null);                                       // (at no standing: his prices are anyone's)
            // the player (Trade) and another ship (Deal) buy the same in two copies of a market: the same stock, silver and hold
            var dorestad = GameData.Port("dorestad");
            var a = new Market(dorestad, rules, 793, 0.0);
            var b = new Market(dorestad, rules, 793, 0.0);
            Trade.Begin(rules, 900f, 0.0);
            var other = new Purse();
            foreach (var kv in rules.startCoins)
                other.Add(kv.Key, kv.Value);
            var otherHold = new Hold { capacity = 900f };
            var bought = Trade.Buy(a, "bar_iron", 4, 0.0, 6, 793);
            var dealt = Deal.Buy(b, other, otherHold, "bar_iron", 4, 6, 0.0, "dorestad.1");
            Assert.IsTrue(bought.ok && dealt.ok);
            Assert.AreEqual(bought.count, dealt.count);
            Assert.AreEqual(bought.deniers, dealt.deniers, 1e-6);
            Assert.AreEqual(a.Get("bar_iron").stock, b.Get("bar_iron").stock, 1e-6f);
            Assert.AreEqual(Trade.Purse.Worth(a.zone, rules), other.Worth(b.zone, rules), 1e-6);
            Assert.AreEqual(Trade.Hold.Count("bar_iron"), otherHold.Count("bar_iron"));
            // what another ship buys makes it dearer for everyone
            float before = b.Asks(b.Get("bar_iron"), 6);
            Deal.Buy(b, other, otherHold, "bar_iron", 10, 6, 0.0, "dorestad.1");
            Assert.Greater(b.Asks(b.Get("bar_iron"), 6), before);
            // and what it sells gluts the market
            float pays = b.Pays(b.Get("bar_iron"), 6);
            var sold = Deal.Sell(b, other, otherHold, "bar_iron", 6, 6, 0.0, "dorestad.1");
            Assert.IsTrue(sold.ok);
            Assert.Less(b.Pays(b.Get("bar_iron"), 6), pays);
        }

        [Test]
        public void HowFarALookoutSees()
        {
            Assert.AreEqual(18.5, Sight.HorizonKm(1.8, 12.0), 0.1, "a 12 m sail clears the horizon at 18.5 km from a 1.8 m eye");
            Assert.AreEqual(2.3, Sight.RangeKm(1.8, 12.0, 2.3, 30.0, 0.5), 1e-6, "fog: no further than the visibility");
            Assert.AreEqual(0.5, Sight.RangeKm(1.8, 12.0, 40.0, -20.0, 0.0), 1e-6, "a moonless night: half a kilometre");
            Assert.AreEqual(2.5, Sight.RangeKm(1.8, 12.0, 40.0, -20.0, 1.0), 1e-6, "under a full moon: two and a half");
            Assert.Greater(Sight.SunElevation(52.0, 172, 12.0), 55.0, "midsummer noon at 52 N");
            Assert.Less(Sight.SunElevation(52.0, 355, 0.0), -50.0, "midwinter midnight");
        }

        [Test]
        public void AllTheMarketsOpenInTheTablesOrder()
        {
            Markets.OpenAll(793, 0.0);
            int last = -1, open = 0;
            foreach (var m in Markets.All)
            {
                Assert.Greater(m.port.index, last);
                last = m.port.index;
                open++;
            }
            Assert.Greater(open, 40);
            Markets.Clear();
            Assert.AreEqual(0, Markets.All.Count);
        }
    }
}
