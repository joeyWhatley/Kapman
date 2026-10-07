using System.Diagnostics;
using System.Linq;
using Kapman.Game;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The economy design's night-one tests (4.3: 1, 5, 8, 9, 11; 3 is TradeTests): the tables agree, the
    /// chronicle's 793 famine and the Lindisfarne raid move the markets, the markets are deterministic, fast over a
    /// century, and deaf to the clock going back.</summary>
    public class EconomyTests
    {
        const string Dir = "Assets/Kapman/Game/Data/";
        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + name);

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            TradeRules.Load(Asset("trade_rules.json"));
            Chronicle.Load(Asset("chronicle_793.json"));
            Shocks.Load(Asset("shocks_793.json"));
            Shocks.Clear();
            Markets.Clear();
        }

        [TearDown]
        public void Forget()
        {
            Shocks.Clear();
            Markets.Clear();
        }

        static int DayNumber(GameDate d) => d.year * 365 + d.year / 4 + d.dayOfYear;
        static int DayNumber(ChronicleEntry e) => DayNumber(Calendar.At(e.year, DayOfYear(e), 0.0));

        static int DayOfYear(ChronicleEntry e)
        {
            int doy = e.day;
            for (int m = 1; m < e.month; m++)
                doy += Calendar.DaysIn(e.year, m);
            return doy;
        }

        /// <summary>The start: 21 June 793 is game day 0; the entries still felt (as KapmanGame replays them) hit the markets.</summary>
        static void StartAt793(bool withChronicle)
        {
            var start = Calendar.At(793, 172, 0.0);
            Markets.OpenAll(793, 0.0);
            if (!withChronicle)
                return;
            foreach (var e in Chronicle.Ongoing(start, 60).Concat(Chronicle.Recent(start, 60)))
            {
                double happened = -(DayNumber(start) - DayNumber(e));
                Shocks.Apply(e, Shocks.PortsOf(e, 793).Select(p => Markets.Of(p, 793, 0.0)), happened, 0.0);
            }
        }

        [Test]
        public void TheTablesAgree()
        {
            foreach (var p in GameData.Ports)
                Assert.IsTrue(GameData.Regions.ContainsKey(p.region), $"port {p.id}: unknown region {p.region}");
            var ids = Chronicle.Entries.Select(e => e.id).ToHashSet();
            var missing = Chronicle.Entries.SelectMany(e => e.ports.Select(id => (entry: e.id, port: id))).Where(x => GameData.Port(x.port) == null).ToList();
            Assert.IsEmpty(missing, "chronicle ports not in ports_793: " + string.Join(", ", missing.Take(12).Select(x => $"{x.entry}->{x.port}")));
            foreach (var id in Shocks.Ids)
                Assert.IsTrue(ids.Contains(id), $"shocks_793 names '{id}', which the chronicle lacks");
            var famine = Chronicle.Entries.First(e => e.id == "793-great-famine");
            Assert.AreEqual(794 * 400 + 8 * 32 + 31, famine.EndKey, "end 794-08 is the last day of August 794");
            Assert.AreEqual(593.0, Shocks.DaysToEnd(famine), 0.5, "15 January 793 to 31 August 794");
        }

        [Test]
        public void TheFamineOf793IsOnAtTheStart()
        {
            StartAt793(false);
            var dorestad = GameData.Port("dorestad");
            var plain = Markets.Of(dorestad, 793, 0.0);
            float plainAsk = plain.Asks(plain.Get("wheat"), 6);
            Markets.Clear();
            StartAt793(true);
            var m = Markets.Of(dorestad, 793, 0.0);
            float ask = m.Asks(m.Get("wheat"), 6);
            Assert.GreaterOrEqual(ask, 1.4f * plainAsk, $"wheat at Dorestad on 21 June 793: {ask:0.0} d against {plainAsk:0.0} in a plain June");
            Assert.Greater(Shocks.Holding, 0, "the famine is held");
            // held through the summer: a month on (each day ticked), still dear
            for (int d = 1; d <= 30; d++)
            {
                Shocks.Daily(d);
                Markets.CatchUpAll(d);
            }
            Assert.GreaterOrEqual(m.Asks(m.Get("wheat"), 7), 1.4f * plain.Asks(plain.Get("wheat"), 7), "still dear a month on");
            // a good that is not food is not touched by it
            var wine = m.Get("rhenish_wine");
            if (wine != null)
                Assert.Less(wine.dearer, 0.01f, "the famine leaves the wine alone");
        }

        [Test]
        public void LindisfarneIsShutTenDaysAfterTheRaid()
        {
            var raid = Chronicle.Entries.First(e => e.id == "793-06-08-lindisfarena-raid");
            Markets.OpenAll(793, 0.0);                                       // day 0 = 8 June 793
            var at = Shocks.PortsOf(raid, 793);
            Assert.IsNotEmpty(at, "Lindisfarne is a port in 793");
            Shocks.Apply(raid, at.Select(p => Markets.Of(p, 793, 0.0)), 0.0, 0.0);
            var m = Markets.Of(at[0], 793, 0.0);
            Assert.IsTrue(m.Disrupted(0.0), "shut on the day");
            Assert.IsTrue(m.Disrupted(9.9), "shut for ten days");
            Assert.IsFalse(m.Disrupted(10.0), "open again by 18 June");
            var purse = new Purse();
            purse.Add("denier", 1000);
            var hold = new Hold { capacity = 100f };
            var good = m.Goods.First();
            Assert.AreEqual("disrupted", Deal.Buy(m, purse, hold, good.good.id, 1, 6, 5.0, "player").why, "no deals while shut");
        }

        static ulong Year793(bool chronicle)
        {
            Markets.Clear();
            Shocks.Clear();
            StartAt793(chronicle);
            for (int d = 1; d <= 365; d++)
            {
                Shocks.Daily(d);
                Markets.CatchUpAll(d);
            }
            return Markets.Hash();
        }

        [Test]
        public void TheSameYearGivesTheSameMarkets()
        {
            ulong a = Year793(true), b = Year793(true), c = Year793(false);
            Assert.AreEqual(a, b, "the same start and chronicle give the same markets");
            Assert.AreNotEqual(a, c, "the chronicle changes them");
        }

        [Test]
        public void ACenturyOfMarketsIsQuick()
        {
            Markets.OpenAll(793, 0.0);
            int ports = Markets.All.Count;
            Assert.GreaterOrEqual(ports, 40, "the slice's ports");
            var watch = Stopwatch.StartNew();
            for (int d = 10; d <= 36500; d += 10)
                Markets.CatchUpAll(d);
            watch.Stop();
            Assert.Less(watch.Elapsed.TotalSeconds, 10.0, $"{ports} ports over 100 years took {watch.Elapsed.TotalSeconds:0.00} s");
            watch.Restart();
            for (int i = 1; i <= 100; i++)
                Markets.CatchUpAll(36500 + i);
            Assert.Less(watch.Elapsed.TotalMilliseconds / 100.0, 2.0, "a day's catch-up of every port");
        }

        [Test]
        public void TheClockGoingBackChangesNothing()
        {
            StartAt793(true);
            for (int d = 1; d <= 100; d++)
            {
                Shocks.Daily(d);
                Markets.CatchUpAll(d);
            }
            ulong h = Markets.Hash();
            Markets.CatchUpAll(50.0);
            Shocks.Daily(50.0);
            Assert.AreEqual(h, Markets.Hash());
        }
    }
}
