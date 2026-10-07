using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The economy design's night-two tests (4.3: 2 and 4, and the ruined market of 2.5): prices drift into
    /// their seasons over about 20 days and never step 2 % in a day without a shock; the granary carries the year, so
    /// wheat at Parisius is dearest in May and cheapest in September; the new wine is cheap at Dorestad in November;
    /// a burning rebuilds a market smaller; Dorestad's last sack in 863 ruins it, and the save and the hash keep it so.</summary>
    public class EconomyNightTwoTests
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
            Seasons.Load(Asset("seasons_793.json"));
            Seasons.SetStart(793, 172);                                      // game day 0 = 21 June 793, as the clock starts
            Shocks.Clear();
            Markets.Clear();
        }

        [TearDown]
        public void Forget()
        {
            Shocks.Clear();
            Markets.Clear();
            Seasons.SetStart(793, 172);
        }

        static ChronicleEntry Entry(string id) => Chronicle.Entries.First(e => e.id == id);

        /// <summary>The game day an entry happened on.</summary>
        static double DayOf(ChronicleEntry e) => Seasons.DayOf(e.year, e.month, e.day);

        [Test]
        public void TheCalendarAgreesWithTheClock()
        {
            foreach (int day in new[] { -400, -1, 0, 1, 10, 193, 194, 365, 1000, 25000, 99700 })
            {
                var date = Calendar.At(793, 172, day * 24.0);
                Seasons.DateOf(day, out int year, out int doy);
                Assert.AreEqual(date.year, year, $"day {day}");
                Assert.AreEqual(date.dayOfYear, doy, $"day {day}");
                Assert.AreEqual(day, Seasons.DayOf(year, doy), 1e-9, $"day {day} back");
            }
            Assert.AreEqual(194.0, Seasons.DayOf(794, 1, 1), 1e-9, "1 January 794 is 194 days after 21 June 793");
            Assert.AreEqual(58, Seasons.Index(796, 60), "a leap year's 29 February reads the 28th");
            Assert.AreEqual(59, Seasons.Index(796, 61), "and its 1 March the 1st");
            Assert.AreEqual(5, Seasons.MonthOfIndex(Seasons.MidMonth(5)));
        }

        /// <summary>Design test 2: the price answers stock within its bounds, the cheap and dear months blend over 20 days,
        /// and no price in any market steps 2 % in a day without a shock (two plain years, every market, every good).</summary>
        [Test]
        public void PricesDriftIntoTheirSeasons()
        {
            var rules = TradeRules.Current;
            var wheat = GameData.Good("wheat");
            Assert.AreEqual(rules.dear, Seasons.PriceFactor(wheat, Seasons.MidMonth(5)), 1e-6f, "a dear month's middle");
            Assert.AreEqual(rules.cheap, Seasons.PriceFactor(wheat, Seasons.MidMonth(9)), 1e-6f, "a cheap month's middle");
            Assert.AreEqual(1f, Seasons.PriceFactor(wheat, Seasons.MidMonth(7)), 1e-6f, "a plain month's middle");
            float firstOfJuly = Seasons.PriceFactor(wheat, Seasons.Index(793, 182));
            Assert.IsTrue(firstOfJuly > 1f && firstOfJuly < rules.dear, $"1 July is between June's dear and July's plain: {firstOfJuly}");
            int blended = 0;
            for (int i = 150; i < 210; i++)
            {
                float f = Seasons.PriceFactor(wheat, i);
                if (f > 1f + 1e-5f && f < rules.dear - 1e-5f)
                    blended++;
            }
            Assert.AreEqual(Mathf.RoundToInt(Seasons.EdgeDays), blended, "the edge blends over 20 days");
            foreach (var g in GameData.Goods)
                for (int i = 0; i < Seasons.YearDays; i++)
                {
                    float a = Seasons.PriceFactor(g, i), b = Seasons.PriceFactor(g, (i + 1) % Seasons.YearDays);
                    Assert.Less(Mathf.Abs(b / a - 1f), 0.02f, $"{g.id}'s season from day {i} to the next");
                }

            Markets.OpenAll(793, 0.0);
            var all = Markets.All;
            var last = new float[all.Count][];
            double worst = 0.0;
            string where = "";
            for (int d = 0; d <= 730; d++)
            {
                Markets.CatchUpAll(d);
                for (int i = 0; i < all.Count; i++)
                {
                    var m = all[i];
                    if (last[i] == null)
                        last[i] = new float[m.Goods.Count];
                    for (int j = 0; j < m.Goods.Count; j++)
                    {
                        float p = m.Price(m.Goods[j], m.Month);
                        if (d > 0)
                        {
                            double step = Math.Abs(p / last[i][j] - 1.0);
                            if (step > worst)
                            {
                                worst = step;
                                where = $"{m.port.id} {m.Goods[j].good.id} on day {d} ({Calendar.At(793, 172, d * 24.0)}): {last[i][j]:0.###} to {p:0.###}";
                            }
                        }
                        last[i][j] = p;
                    }
                }
            }
            Assert.Less(worst, 0.02, "the largest day's step: " + where);

            // stock below the usual is dearer, above it cheaper, within the bounds
            var dorestad = Markets.Of(GameData.Port("dorestad"), 793, 730.0);
            var wine = dorestad.Get("rhenish_wine");
            float keep = wine.stock;
            wine.stock = wine.usual;
            float atUsual = dorestad.Price(wine, dorestad.Month);
            wine.stock = wine.usual * 0.5f;
            Assert.Greater(dorestad.Price(wine, dorestad.Month), atUsual, "scarce is dear");
            wine.stock = wine.usual * 2f;
            Assert.Less(dorestad.Price(wine, dorestad.Month), atUsual, "plenty is cheap");
            wine.stock = 0f;
            Assert.AreEqual(atUsual * rules.priceMax, dorestad.Price(wine, dorestad.Month), atUsual * 1e-3f, "none left: price_max");
            wine.stock = wine.usual * 1000f;
            Assert.AreEqual(atUsual * rules.priceMin, dorestad.Price(wine, dorestad.Month), atUsual * 1e-3f, "a glut of stock: price_min");
            wine.stock = keep;
        }

        /// <summary>Design test 4: in a plain year (no chronicle) wheat at Parisius is dearest in May (the hungry spring,
        /// the granary low) and cheapest in September (the harvest in); and the wine Dorestad sells on (rhine_delta 2) is
        /// cheap in November, after the vintage, and dear in August.</summary>
        [Test]
        public void WheatAtParisiusIsDearestInMayAndCheapestInSeptember()
        {
            Assert.AreEqual(2f, GameData.Good("rhenish_wine").MadeIn("rhine_delta"), "Dorestad sells the Rhine's wine on");
            Markets.OpenAll(793, 0.0);
            var paris = GameData.Port("parisius");
            var dorestad = GameData.Port("dorestad");
            Assert.IsNotNull(paris, "Parisius is in ports_793");
            var wheat = new double[13];
            var wine = new double[13];
            var stock = new double[13];
            var days = new int[13];
            double from = Seasons.DayOf(794, 1), to = Seasons.DayOf(795, 1);
            for (double d = 1.0; d < to; d += 1.0)
            {
                Markets.CatchUpAll(d);
                if (d < from)
                    continue;
                var m = Markets.Of(paris, 794, d);
                var w = Markets.Of(dorestad, 794, d);
                wheat[m.Month] += m.Asks(m.Get("wheat"), m.Month);
                stock[m.Month] += m.Get("wheat").stock / m.Get("wheat").usual;
                wine[w.Month] += w.Asks(w.Get("rhenish_wine"), w.Month);
                days[m.Month]++;
            }
            var mean = new double[13];
            for (int month = 1; month <= 12; month++)
            {
                Assert.Greater(days[month], 27, $"month {month} sampled");
                mean[month] = wheat[month] / days[month];
                stock[month] /= days[month];
                wine[month] /= days[month];
            }
            string table = string.Join(", ", Enumerable.Range(1, 12).Select(i => $"{i}: {mean[i]:0.00} d (stock {stock[i]:0.00})"));
            int dearest = Enumerable.Range(1, 12).OrderByDescending(i => mean[i]).First();
            int cheapest = Enumerable.Range(1, 12).OrderBy(i => mean[i]).First();
            Assert.AreEqual(5, dearest, "dearest in May: " + table);
            Assert.AreEqual(9, cheapest, "cheapest in September: " + table);
            Assert.Less(stock[5], 0.8, "the granary is low in May");
            Assert.Greater(stock[9], 1.2, "and full in September");
            Assert.Less(wine[11], 0.75 * wine[8], $"Dorestad's wine: {wine[11]:0} d in November against {wine[8]:0} d in August");
        }

        /// <summary>The catch-up is exact however it is split: a market touched every few hours and one touched once a
        /// year on end up the same (the seasons of making are stepped a day at a time either way).</summary>
        [Test]
        public void TheCatchUpIsTheSameHoweverItIsSplit()
        {
            var paris = GameData.Port("parisius");
            var often = new Market(paris, TradeRules.Current, 793, 0.0);
            var once = new Market(paris, TradeRules.Current, 793, 0.0);
            for (double t = 0.37; t < 400.0; t += 0.61)
                often.CatchUp(t);
            often.CatchUp(400.0);
            once.CatchUp(400.0);
            foreach (var g in often.Goods)
                Assert.AreEqual(g.stock, once.Get(g.good.id).stock, 1e-3f * Mathf.Max(1f, g.usual), g.good.id);
        }

        /// <summary>Design 2.5: Dorestad's last sack (1 March 863) ruins it: no more deals, its size falling a step at each
        /// new year to nothing (4 by 864, 0 by 870); Cologne, raided the same day, stands. The hash and the save keep the
        /// ruin; and a market opened after a port's last year is ruined anyway.</summary>
        [Test]
        public void DorestadIsRuinedByItsLastSack()
        {
            Seasons.SetStart(863, 1);                                        // game day 0 = 1 January 863
            Markets.OpenAll(863, 0.0);
            var sack = Entry("863-dorestad-last");
            double happened = DayOf(sack);
            Assert.AreEqual(59.0, happened, 1e-9, "1 March 863");
            var at = Shocks.PortsOf(sack, 863);
            Shocks.Apply(sack, at.Select(p => Markets.Of(p, 863, happened)), happened, happened);
            var dorestad = Markets.Of(GameData.Port("dorestad"), 863, happened);
            var colonia = Markets.Of(GameData.Port("colonia"), 863, happened);
            Assert.IsTrue(dorestad.Ruined, "ruined by its last sack");
            Assert.AreEqual(863, dorestad.RuinYear);
            Assert.AreEqual(5, dorestad.Size, "still its size in 863");
            Assert.IsFalse(colonia.Ruined, "Cologne is raided, not ruined");

            // long after the raid's closing, Dorestad still takes no deals; Cologne does again
            double later = happened + 200.0;
            Markets.CatchUpAll(later);
            Assert.IsFalse(colonia.Disrupted(later), "Cologne open again");
            Assert.IsTrue(dorestad.Disrupted(later), "Dorestad shut for good");
            var purse = new Purse();
            purse.Add("denier", 1000);
            var hold = new Hold { capacity = 2000f };
            Assert.AreEqual("ruined", Deal.Buy(dorestad, purse, hold, "rhenish_wine", 1, dorestad.Month, later, "a trader").why);
            hold.Add(GameData.Good("rhenish_wine"), 2, 100.0, 0.0);
            Assert.AreEqual("ruined", Deal.Sell(dorestad, purse, hold, "rhenish_wine", 1, dorestad.Month, later, "a trader").why);
            Assert.AreEqual(2, hold.Count("rhenish_wine"), "nothing sold");

            // its size falls with the years
            Markets.CatchUpAll(Seasons.DayOf(864, 3, 1));
            Assert.AreEqual(4, dorestad.Size, "a step smaller by March 864");
            Markets.CatchUpAll(Seasons.DayOf(870, 1, 1));
            Assert.AreEqual(0, dorestad.Size, "nothing left by 870");
            Assert.IsTrue(dorestad.Goods.All(g => g.usual == 0f), "it makes nothing");
            Assert.AreEqual(GameData.Port("colonia").size, colonia.Size, "Cologne keeps its size");

            // the hash covers the ruin, and a save brings it back
            ulong hash = Markets.Hash();
            string saved = Json.Write(Markets.Capture());
            Markets.Clear();
            Markets.Restore(Json.Parse(saved) as List<object>, 863);
            Assert.AreEqual(hash, Markets.Hash(), "the same markets after a save");
            var back = Markets.Of(GameData.Port("dorestad"), 870, Seasons.DayOf(870, 1, 1));
            Assert.IsTrue(back.Ruined);
            Assert.AreEqual(863, back.RuinYear);
            Assert.AreEqual(happened, back.RuinedFrom, 1e-9);
            Assert.AreEqual(0, back.Size);

            // without the chronicle, a port whose last year (Dorestad's is 863) has passed is ruined from the new year
            Markets.Clear();
            Seasons.SetStart(865, 1);
            var fresh = new Market(GameData.Port("dorestad"), TradeRules.Current, 865, 10.0);
            Assert.IsTrue(fresh.Ruined, "ruined in 865");
            Assert.AreEqual(3, fresh.Size, "two years since its last: two steps smaller");
            Assert.AreEqual("ruined", Deal.Buy(fresh, purse, new Hold { capacity = 2000f }, "rhenish_wine", 1, fresh.Month, 10.0, "a trader").why);
        }

        /// <summary>Design 2.5: a burning shuts a market for a while and rebuilds it a step smaller (its usual stock with
        /// it): Dorestad in 846; Nijmegen, named in the same entry, feels the raid but isn't burned.</summary>
        [Test]
        public void ABurningRebuildsAMarketSmaller()
        {
            Seasons.SetStart(846, 1);
            Markets.OpenAll(846, 0.0);
            var burned = Entry("846-dorestad-burned");
            double happened = DayOf(burned);
            var dorestad = Markets.Of(GameData.Port("dorestad"), 846, happened);
            var nijmegen = Markets.Of(GameData.Port("noviomagus"), 846, happened);
            float wineBefore = dorestad.Get("rhenish_wine").usual;
            Shocks.Apply(burned, Shocks.PortsOf(burned, 846).Select(p => Markets.Of(p, 846, happened)), happened, happened);
            Assert.AreEqual(4, dorestad.Size, "rebuilt a step smaller");
            Assert.AreEqual(1, dorestad.SizeLoss);
            Assert.Less(dorestad.Get("rhenish_wine").usual, wineBefore, "it keeps less wine");
            Assert.AreEqual(3, nijmegen.Size, "Nijmegen isn't burned");
            Assert.IsTrue(dorestad.Disrupted(happened + 5.0), "shut after the burning");
            Assert.IsFalse(dorestad.Disrupted(happened + 30.0), "open again, smaller");
            Assert.IsFalse(dorestad.Ruined, "burned, not ruined");
        }
    }
}
