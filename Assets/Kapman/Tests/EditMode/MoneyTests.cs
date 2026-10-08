using System.Collections.Generic;
using System.Linq;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE MONEY FIX (rulings 14 and 30 of 7 Oct 2026; Design/research/next/money.md, Design/fixes/money_that_pays.md):
    /// the world's traders at work from 1 April before the player's first morning; the famine of 793 on grain only; the
    /// English toll a tenth; the debt eight pounds, due on a day (Michaelmas, a Sunday in 793); the traders' least
    /// margin a quarter; the count's four casks his own, and what they fetch never told as gain; the first cask of wine
    /// at Dorestat within the purse.
    /// </summary>
    public class MoneyTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const int FirstOfMay = 121;
        const double SevenInTheMorning = 7.0;
        TradeRules rules;

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            rules = TradeRules.Load(File("trade_rules.json"));
            Markets.Clear();
            Shocks.Clear();
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Shocks.Clear();
            Seasons.SetStart(793, 172);                                  // (the economy's calendar as the other tests expect it)
        }

        /// <summary>The world as the game makes it for a start on 1 May 793 at seven: made `back` days before, stepped
        /// to the clock's start with nobody watching.</summary>
        static WorldSim MorningOf(int seed, int back)
        {
            var w = SimHarness.MakeWorldAt(seed, FirstOfMay, SevenInTheMorning - back * 24.0);
            w.stats.csvEvery = w.stats.sampleEvery = 0;
            w.Populate();
            w.RunTo(SevenInTheMorning, Shocks.Daily);
            return w;
        }

        static float WineAsked()
        {
            var m = Markets.Of(GameData.Port("dorestad"), 793, SevenInTheMorning / 24.0);
            return m.Asks(m.Get("rhenish_wine"), 5);
        }

        // ------------------------------------------------------------------ the data

        [Test]
        public void TheFamineOf793FallsOnGrainOnly()
        {
            Shocks.Load(File("shocks_793.json"));
            var famine = Shocks.For(new ChronicleEntry { id = "793-great-famine", kind = "famine" });
            Assert.IsNotNull(famine, "shocks_793.json has no 793-great-famine");
            foreach (string grain in new[] { "wheat", "rye", "barley", "oats", "malt", "flour" })
                Assert.IsTrue(Shocks.Touches(famine.goods, GameData.Good(grain)), grain + " should be dear in the famine");
            foreach (string other in new[] { "honey", "cheese", "ships_bread", "rhenish_wine", "herring" })
                if (GameData.Good(other) != null)
                    Assert.IsFalse(Shocks.Touches(famine.goods, GameData.Good(other)), other + " is not grain: the famine of 793 does not touch it");
        }

        [Test]
        public void TheEnglishTollIsATenth()
        {
            Assert.AreEqual(0.10f, rules.ZoneOf("england_south").toll, 1e-6f, "the English toll (ruling 14: a tenth, it was a twentieth)");
            Assert.AreEqual(0.10f, rules.ZoneOf("rhine_delta").toll, 1e-6f, "the Frankish toll");
            Assert.AreEqual(0f, rules.ZoneOf("jutland").toll, 1e-6f, "no toll where silver is weighed");
        }

        /// <summary>Ruling 30: "Two numbers that go together or not at all". If one is moved, move the other and read
        /// Design/fixes/money_that_pays.md section 8 first.</summary>
        [Test]
        public void EightPoundsAndAQuarterGoTogether()
        {
            Assert.AreEqual(1920f, rules.startDebt, 1e-3f, "the debt (trade_rules.json start.debt): eight pounds");
            Assert.AreEqual(0.25f, new SimRules().minMargin, 1e-6f, "the least margin the world's traders carry for (SimRules.minMargin)");
        }

        // ------------------------------------------------------------------ the pre-run

        [Test]
        public void TheWorldStartsOnTheFirstOfApril()
        {
            Assert.AreEqual(4, rules.worldFromMonth);
            Assert.AreEqual(1, rules.worldFromDay);
            Assert.AreEqual(30, rules.PreRunDays(793, 121), "a game begun on 1 May");
            Assert.AreEqual(81, rules.PreRunDays(793, 172), "a game begun on 21 June");
            Assert.AreEqual(0, rules.PreRunDays(793, 60), "a start before 1 April: the world starts with the player");
            Assert.AreEqual(0, rules.PreRunDays(793, 91), "a start on 1 April itself");
        }

        [Test]
        public void ThePreRunEndsOnTheClocksStartAndOneSeedGivesOneWorld()
        {
            var w = MorningOf(7, 30);
            Assert.AreEqual(SevenInTheMorning, w.Hours, 1e-6, "the world's hours at the end of the pre-run are the clock's");
            Assert.AreEqual(5, w.Date.month);
            Assert.AreEqual(1, w.Date.day);
            Assert.AreEqual(793, w.Date.year);
            ulong first = Markets.Hash();
            long ships = w.StateHash();
            w = MorningOf(7, 30);
            Assert.AreEqual(first, Markets.Hash(), "two pre-runs of one seed leave the same markets");
            Assert.AreEqual(ships, w.StateHash(), "and the same ships");
            MorningOf(8, 30);
            Assert.AreNotEqual(first, Markets.Hash(), "another seed, another world");
        }

        /// <summary>The first morning is reckoned once and kept on disk (FirstMorning): read back into a world just made it
        /// is the same world, and it plays on the same from there.</summary>
        [Test]
        public void AKeptFirstMorningIsTheSameWorld()
        {
            string folder = System.IO.Path.Combine(Application.temporaryCachePath, "kapman_first_morning_test");
            if (System.IO.Directory.Exists(folder))
                System.IO.Directory.Delete(folder, true);
            FirstMorning.FolderOverride = folder;
            try
            {
                string key = FirstMorning.Key(new[] { "a test", "seed 7" }, new SimRules());
                Assert.AreEqual(key, FirstMorning.Key(new[] { "a test", "seed 7" }, new SimRules()), "one set of parts, one key");
                Assert.AreNotEqual(key, FirstMorning.Key(new[] { "a test", "seed 8" }, new SimRules()), "another seed, another key");
                Assert.AreNotEqual(key, FirstMorning.Key(new[] { "a test", "seed 7" }, new SimRules { minMargin = 0.12f }), "another rule, another key");

                var a = MorningOf(7, 30);
                Assert.IsFalse(FirstMorning.TryRestore(key, a, 793, SevenInTheMorning), "nothing is kept yet");
                FirstMorning.Keep(key, a);
                ulong markets = Markets.Hash();
                long ships = a.StateHash();
                int holding = Shocks.Holding;
                for (int h = 0; h < 72; h++)
                    a.Hour();
                ulong marketsOn = Markets.Hash();
                long shipsOn = a.StateHash();
                Assert.AreNotEqual(ships, shipsOn, "three days on the ships have moved");

                // a world just made, as of 1 April; the kept morning put into it
                var b = SimHarness.MakeWorldAt(7, FirstOfMay, SevenInTheMorning - 30 * 24.0);
                b.stats.csvEvery = b.stats.sampleEvery = 0;
                b.Populate();
                Shocks.Clear();
                Assert.IsTrue(FirstMorning.TryRestore(key, b, 793, SevenInTheMorning), "the kept morning was not read back");
                Assert.AreEqual(SevenInTheMorning, b.Hours, 1e-9);
                Assert.AreEqual(a.ships.Count, b.ships.Count);
                Assert.AreEqual(markets, Markets.Hash(), "the markets of the kept morning are not those of the morning reckoned");
                Assert.AreEqual(ships, b.StateHash(), "the ships of the kept morning are not those of the morning reckoned");
                Assert.AreEqual(holding, Shocks.Holding);
                for (int h = 0; h < 72; h++)
                    b.Hour();
                Assert.AreEqual(shipsOn, b.StateHash(), "three days on, the world from the kept morning has gone another way (ships)");
                Assert.AreEqual(marketsOn, Markets.Hash(), "three days on, the world from the kept morning has gone another way (markets)");

                // another hour asked for, or another key: not this morning
                Assert.IsFalse(FirstMorning.TryRestore(key, b, 793, SevenInTheMorning + 1.0), "a morning kept for seven is not one for eight");
                Assert.IsFalse(FirstMorning.TryRestore("0000000000000000", b, 793, SevenInTheMorning));
                Assert.IsFalse(FirstMorning.Spoiled);
            }
            finally
            {
                FirstMorning.FolderOverride = null;
                if (System.IO.Directory.Exists(folder))
                    System.IO.Directory.Delete(folder, true);
            }
        }

        [Test]
        public void NoMarketIsUntouchedOnTheFirstMorning()
        {
            // the cold table: Dorestat on 1 May with no ship having traded
            MorningOf(1, 0);
            float cold = WineAsked();
            ulong coldHash = Markets.Hash();
            var asked = new List<float>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int seed = 1; seed <= 4; seed++)
            {
                var w = MorningOf(seed, 30);
                asked.Add(WineAsked());
                Assert.AreNotEqual(coldHash, Markets.Hash(), "seed " + seed + ": the markets are as if nobody had traded");
                Assert.Greater(w.ships.Count(s => !s.raider && s.state != ShipState.Lost), 100, "the world's traders are there");
            }
            float mean = asked.Average();
            Debug.Log($"Money: a cask of Rhenish wine at Dorestat on 1 May 793 asks {cold:0.0} d with nobody having traded, " +
                      $"and {mean:0.0} d (seeds 1 to 4: {string.Join(", ", asked.Select(a => a.ToString("0.0")))}) with the world's traders at work since 1 April. " +
                      $"Four pre-runs of 30 days took {sw.ElapsedMilliseconds} ms.");
            // (money_that_pays.md, soak2: 49.9 d at the margin 0.25, against 44 d cold. The model is a copy in Python with
            // twelve ports; the game has every port. The band is wide on purpose: what must hold is "dearer than cold,
            // and still within the purse")
            Assert.Greater(mean, cold + 0.5f, "the traders have bought wine at Dorestat: it asks more than the cold table");
            Assert.Less(mean, 75f, "but not out of all reason");
        }

        [Test]
        public void ThePreRunIsQuick()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var w = FirstMorningOf(172, 14.0, 81, 1);
            Debug.Log($"Money: the longest pre-run (a start on 21 June, 81 days, {81 * 24} hours) took {sw.ElapsedMilliseconds} ms.");
            Assert.AreEqual(14.0, w.Hours, 1e-6);
            Assert.Less(sw.ElapsedMilliseconds, 20000, "the pre-run of the longest start must not hang the first frame for long");
        }

        static WorldSim FirstMorningOf(int startDay, double startHour, int back, int seed)
        {
            var w = SimHarness.MakeWorldAt(seed, startDay, startHour - back * 24.0);
            w.stats.csvEvery = w.stats.sampleEvery = 0;
            w.Populate();
            w.RunTo(startHour, Shocks.Daily);
            return w;
        }

        // ------------------------------------------------------------------ the purse, and the first cask

        [Test]
        public void ThePurseIsEightySixInSilverAndBuysTheFirstCaskAtDorestat()
        {
            // "A new player starts with 40 d while the bar shows 86 d": both are true. The purse is four kinds of silver:
            // 40 deniers of the king's coin, 25 English pennies, 6 dirhams and 30 g of cut silver. All of it is 85.5 d
            // of silver (the bar); at Dorestat the other three go at a tenth's loss, so it buys 81 d's worth
            Trade.Begin(rules, 660f, 0.29, 793, FirstOfMay);
            var dorestad = GameData.Port("dorestad");
            var zone = rules.ZoneOf(dorestad.region);
            Assert.AreEqual(40.0, Trade.Purse.Count("denier"), 1e-9, "deniers of the king's coin");
            Assert.AreEqual(85.5, Trade.Purse.Silver(rules), 0.5, "all the silver, in deniers' worth (what the top bar shows)");
            Assert.AreEqual(81.0, Trade.Purse.Worth(zone, rules), 0.5, "what it buys at Dorestat");
            Assert.AreEqual(81.0, FrameGame.SilverHere(dorestad), 0.5);
            StringAssert.Contains("it buys 81 d's worth here", FrameGame.SilverWords(dorestad));

            for (int seed = 1; seed <= 4; seed++)
            {
                MorningOf(seed, 30);
                Trade.Begin(rules, 660f, 0.29, 793, FirstOfMay);
                var market = Markets.Of(dorestad, 793, SevenInTheMorning / 24.0);
                float asks = market.Asks(market.Get("rhenish_wine"), 5);
                Assert.Less(asks, Trade.Purse.Worth(zone, rules), $"seed {seed}: a cask asks {asks:0.0} d and the purse buys {Trade.Purse.Worth(zone, rules):0.0} d's worth");
                var deal = Deal.Buy(market, Trade.Purse, Trade.Hold, "rhenish_wine", 1, 5, SevenInTheMorning / 24.0, "test");
                Assert.IsTrue(deal.ok, $"seed {seed}: the first cask could not be bought: {deal.why}");
                Assert.AreEqual(1, Trade.Hold.Count("rhenish_wine"));
            }
        }

        // ------------------------------------------------------------------ the debt's day

        [Test]
        public void TheDebtFallsDueAtMichaelmas()
        {
            // a game begun 1 May 793 (day 121 of the year): due 29 September 793, 151 days on
            Trade.Begin(rules, 900f, 0.29, 793, FirstOfMay);
            Assert.AreEqual(1920.0, Trade.Debt, 0.001);
            Assert.AreEqual(151.0, Trade.DebtDue, 0.001);
            Assert.AreEqual("Michaelmas", Trade.DebtDueName);
            var due = Calendar.At(793, FirstOfMay, Trade.DebtDue * 24.0);
            Assert.AreEqual(793, due.year);
            Assert.AreEqual(9, due.month);
            Assert.AreEqual(29, due.day);
            // whole days, whatever the hour
            Assert.AreEqual(151.0, Trade.DebtDaysLeft(0.9), 0.001);
            Assert.AreEqual(1.0, Trade.DebtDaysLeft(150.99), 0.001);
            Assert.AreEqual(0.0, Trade.DebtDaysLeft(151.5), 0.001);
            Assert.AreEqual(-3.0, Trade.DebtDaysLeft(154.2), 0.001);

            // begun on Michaelmas itself (day 272): the next one, a year on; begun the day before: tomorrow
            Trade.Begin(rules, 900f, 0.0, 793, 272);
            Assert.AreEqual(365.0, Trade.DebtDue, 0.001);
            Trade.Begin(rules, 900f, 0.0, 793, 271);
            Assert.AreEqual(1.0, Trade.DebtDue, 0.001);
            // a start on 21 June (the 3D levels): 100 days
            Trade.Begin(rules, 900f, 0.8, 793, 172);
            Assert.AreEqual(100.0, Trade.DebtDue, 0.001);

            // no calendar: the count of days, and no name
            Trade.Begin(rules, 900f, 10.0);
            Assert.AreEqual(10.0 + rules.dueDays, Trade.DebtDue, 0.001);
            Assert.AreEqual("", Trade.DebtDueName);

            // the day and its name survive a save; a save from before the name has only the day
            Trade.Begin(rules, 900f, 0.29, 793, FirstOfMay);
            var saved = Trade.Capture();
            Trade.Begin(rules, 900f, 0.0);
            Trade.Restore(saved);
            Assert.AreEqual(151.0, Trade.DebtDue, 0.001);
            Assert.AreEqual("Michaelmas", Trade.DebtDueName);
            saved.Remove("due_name");
            Trade.Restore(saved);
            Assert.AreEqual(151.0, Trade.DebtDue, 0.001);
            Assert.AreEqual("", Trade.DebtDueName);
        }

        [Test]
        public void TheBarSaysTheDayAndTheDaysLeft()
        {
            Assert.AreEqual("Owed 1920 d by Michaelmas, 29 September: 151 days left", FrameGame.DebtLine(1920.0, "Michaelmas, 29 September", 151.0));
            Assert.AreEqual("Owed 1920 d by Michaelmas, 29 September: 1 day left", FrameGame.DebtLine(1920.0, "Michaelmas, 29 September", 1.0));
            Assert.AreEqual("Owed 300 d by Michaelmas, 29 September: due today", FrameGame.DebtLine(300.0, "Michaelmas, 29 September", 0.0));
            Assert.AreEqual("Owed 300 d since Michaelmas, 29 September: 1 day overdue", FrameGame.DebtLine(300.0, "Michaelmas, 29 September", -1.0));
            Assert.AreEqual("Owed 300 d since Michaelmas, 29 September: 12 days overdue", FrameGame.DebtLine(300.0, "Michaelmas, 29 September", -12.0));
        }

        [Test]
        public void MichaelmasOf793IsASundayAndThePanelsWarnOfIt()
        {
            // (the data's own anchors: 1 May 793 a Wednesday, 8 June 793 a Saturday; Design/checks/calendar_first_season.md)
            Assert.AreEqual(3, FaithCalendar.Weekday(793, 5, 1), "1 May 793 was a Wednesday");
            Assert.AreEqual(0, FaithCalendar.Weekday(793, 9, 29), "29 September 793 was a Sunday");
            string warn = FrameGame.DebtDayWarning(793, 9, 29, "Michaelmas");
            StringAssert.StartsWith("Michaelmas is a Sunday in 793.", warn);
            StringAssert.Contains("by Saturday, 28 September", warn);
            Assert.AreEqual("", FrameGame.DebtDayWarning(794, 9, 29, "Michaelmas"), "29 September 794 is a Monday: no warning");
            StringAssert.Contains("by Saturday, 31 August", FrameGame.DebtDayWarning(793, 9, 1, ""), "the day before, across a month's end");
        }

        // ------------------------------------------------------------------ the count's casks

        [Test]
        public void TheCountsCasksAreHisAndNeverJoinWhatIsBought()
        {
            var wine = GameData.Good("rhenish_wine");
            Assert.IsNotNull(wine);
            var hold = new Hold { capacity = 5000f };
            hold.Add(wine, 4, 0.0, 0.0, "count");                        // the count's own: nothing paid
            hold.Add(wine, 1, 30.0, 0.0);                                // bought the same day: its own lot
            Assert.AreEqual(2, hold.Lots.Count, "a cask bought beside the count's four is not five casks at the cost of one");
            Assert.AreEqual(5, hold.Count("rhenish_wine"));
            Assert.AreEqual(4, hold.Owed("rhenish_wine"));
            StringAssert.Contains("4 of them came with the boat", FrameGame.CostWords(hold, wine, 5));

            // a saved game keeps whose they are; a save from before lots had an owner reads as the merchant's own
            var saved = Json.Parse(Json.Write(hold.Capture())) as List<object>;   // (as a saved game holds it: written and read)
            var again = new Hold { capacity = 5000f };
            again.Restore(saved);
            Assert.AreEqual(4, again.Owed("rhenish_wine"));
            Assert.AreEqual(30.0, again.Lots.Sum(l => l.cost), 1e-9);
            var old = new List<object> { new List<object> { "rhenish_wine", 4.0, 0.0, 0.0 } };
            again.Restore(old);
            Assert.AreEqual(4, again.Count("rhenish_wine"));
            Assert.AreEqual(0, again.Owed("rhenish_wine"));

            // the oldest go first: the count's four, then the one that was bought
            Assert.AreEqual(0.0, hold.Remove(wine, 4), 0.001);
            Assert.AreEqual(4, hold.LastFree);
            Assert.AreEqual(4, hold.LastOwed);
            Assert.AreEqual(30.0, hold.Remove(wine, 1), 0.001);
            Assert.AreEqual(0, hold.LastFree);
            Assert.AreEqual(0, hold.LastOwed);

            // two bought the same day still make one lot; a find (cost nothing, nobody's) is a lot of its own
            hold.Add(wine, 1, 30.0, 3.0);
            hold.Add(wine, 2, 64.0, 3.2);
            Assert.AreEqual(1, hold.Lots.Count);
            Assert.AreEqual(94.0, hold.Lots[0].cost, 0.001);
            hold.Add(wine, 1, 0.0, 3.3);
            Assert.AreEqual(2, hold.Lots.Count);
            Assert.AreEqual(0, hold.Owed("rhenish_wine"));
            // (the critic's B3, 8 Oct 2026: the casks are the player's to sell and are owed for with the boat; nothing says "his")
            Assert.AreEqual("Came with the boat. Yours to sell, and owed for already.", FrameGame.CostWords(Counts(wine, 4), wine, 4));
            Assert.AreEqual("Your own from the start.", FrameGame.CostWords(Own(wine, 2, 0.0), wine, 2));
            Assert.AreEqual("Cost you 60 d.", FrameGame.CostWords(Own(wine, 2, 60.0), wine, 2));
        }

        static Hold Counts(GoodDef g, int n)
        {
            var h = new Hold { capacity = 5000f };
            h.Add(g, n, 0.0, 0.0, "count");
            return h;
        }

        static Hold Own(GoodDef g, int n, double cost)
        {
            var h = new Hold { capacity = 5000f };
            h.Add(g, n, cost, 0.0);
            return h;
        }

        [Test]
        public void WhatTheCountsCasksFetchIsNotToldAsGain()
        {
            var zone = rules.ZoneOf("rhine_delta");
            // four of the casks that came with the boat sold for 240 d less the tenth: no "more than they cost", and (since
            // 8 Oct 2026, the critic's B3) no word that the silver is another man's: it is the player's to spend
            string four = Trade.SaleReckoned(zone, new DealResult { ok = true, count = 4, deniers = 240.0, toll = 24.0, cost = 0.0 }, 4, 4);
            Assert.AreEqual("", four);
            string one = Trade.SaleReckoned(zone, new DealResult { ok = true, count = 1, deniers = 60.0, toll = 6.0, cost = 0.0 }, 1, 1);
            Assert.AreEqual("", one);
            // two of those and one bought at 30 d, sold for 180 d less 18: the gain is on the one that was bought
            string mixed = Trade.SaleReckoned(zone, new DealResult { ok = true, count = 3, deniers = 180.0, toll = 18.0, cost = 30.0 }, 2, 2);
            Assert.AreEqual(" The one you bought fetched 24 d more than it cost.", mixed);
            foreach (string said in new[] { four, one, mixed })
                StringAssert.DoesNotContain("stake", said);
            // bought goods only: as before
            Assert.AreEqual(" That is 24 d more than they cost.", Trade.SaleReckoned(zone, new DealResult { ok = true, count = 2, deniers = 160.0, toll = 16.0, cost = 120.0 }, 0, 0));
            Assert.AreEqual(" That is 12 d less than they cost.", Trade.SaleReckoned(zone, new DealResult { ok = true, count = 2, deniers = 120.0, toll = 12.0, cost = 120.0 }, 0, 0));
            // (the log must read as sentences: one colon at most in what a sale says, and no bracket)
            foreach (string said in new[] { four, one, mixed })
            {
                StringAssert.DoesNotContain(":", said);
                StringAssert.DoesNotContain("(", said);
            }
            // a find sold (cost nothing, nobody's): nothing to say of gain
            Assert.AreEqual("", Trade.SaleReckoned(zone, new DealResult { ok = true, count = 1, deniers = 20.0, toll = 2.0, cost = 0.0 }, 1, 0));
        }

        [Test]
        public void ASaleOfTheCountsCasksAtAMarketSaysSo()
        {
            Trade.Begin(rules, 660f, 0.29, 793, FirstOfMay);
            var wine = GameData.Good("rhenish_wine");
            Trade.Hold.Add(wine, 4, 0.0, 0.29, ChartWorld.CountsOwn);
            var market = Markets.Of(GameData.Port("dorestad"), 793, 0.29);
            double before = Trade.Purse.Silver(rules);
            var sold = Trade.Sell(market, "rhenish_wine", 1, 0.29, 5, 793);
            Assert.IsTrue(sold.ok, sold.text);
            Assert.Greater(Trade.Purse.Silver(rules), before);
            StringAssert.DoesNotContain("stake", sold.text);
            StringAssert.DoesNotContain("own", sold.text);
            StringAssert.DoesNotContain("more than", sold.text);
            Assert.AreEqual("She has aboard 3 casks of Rhenish wine that came with the boat.", FrameGame.AboardWords());
        }
    }
}
