using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Orders (Design/fixes/orders.md): places post them by their kind; a card offers one by name and it is taken
    /// or refused; taken, it may be brought in parts, each paid by the unit and tolled; the pay is the market there plus
    /// at most a half and at most 60 d over the market; past its day it is broken or short.</summary>
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

        /// <summary>A card's order, as GameEffects.Commission makes it.</summary>
        static Commission Offer(string port, string good, int units, double days = 60.0, double premium = 0.3, double today = 0.0) =>
            Commissions.Offer(GameData.Port(port), GameData.Good(good), units, days, premium, 793, 7, today);

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

        [Test]
        public void ACardsOrderIsKeptForYouUntilYouAnswer()
        {
            var sarre = GameData.Port("sarre");
            Commission asked = null;
            System.Action<Commission> hear = x => asked = x;
            Commissions.Offered += hear;
            Commission c;
            try
            {
                c = Offer("sarre", "rhenish_wine", 4);
            }
            finally
            {
                Commissions.Offered -= hear;
            }
            Assert.AreSame(c, asked, "the frame is told at once, so it can ask");
            Assert.IsTrue(c.offered);
            Assert.IsFalse(c.mine);
            Assert.AreEqual(0, Commissions.At(sarre).Count, "it is not on the board for other merchants");
            for (int d = 1; d <= 7; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.IsTrue(Commissions.All.Contains(c), "nobody else fills it while the offer stands");
            StringAssert.StartsWith("You took the order", Commissions.Take(c));
            Assert.IsTrue(c.mine);
            Assert.IsFalse(c.offered);
            Assert.IsTrue(Commissions.Mine.Contains(c), "taken, it stands among the orders taken");
        }

        [Test]
        public void AnOfferRefusedOrLeftAWeekGoesAndCostsNothing()
        {
            var sarre = GameData.Port("sarre");
            double before = Reputation.At(sarre);
            var refused = Offer("sarre", "wax", 2);
            StringAssert.StartsWith("You refused", Commissions.Refuse(refused));
            Assert.IsFalse(Commissions.All.Contains(refused));
            var left = Offer("sarre", "wax", 2);
            for (int d = 1; d <= 8; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.IsFalse(Commissions.All.Contains(left), "seven days, then it goes");
            Assert.AreEqual(before, Reputation.At(sarre), 1e-9, "no order was taken, so none was broken");
            // an order once taken cannot be refused
            var taken = Offer("sarre", "wax", 2, 60.0, 0.3, 8.0);
            Commissions.Take(taken);
            Assert.AreEqual("", Commissions.Refuse(taken));
            Assert.IsTrue(Commissions.All.Contains(taken));
        }

        [Test]
        public void ItPaysTheMarketPlusAtMostAHalf()
        {
            var sarre = GameData.Port("sarre");
            var wax = GameData.Good("wax");
            double market = Commissions.MarketEach(sarre, wax, 793, 7, 0.0);
            Assert.Greater(market, 0.0);
            Assert.AreEqual(market * 1.5, Commissions.PriceEach(sarre, wax, 1, 1.5, 793, 7, 0.0), 0.011, "a premium of 1.5 is cut to a half");
            Assert.AreEqual(market * 1.1, Commissions.PriceEach(sarre, wax, 1, 0.1, 793, 7, 0.0), 0.011, "a small premium stands as written");
        }

        [Test]
        public void TheWholeOrderPaysAtMostSixtyOverTheMarket()
        {
            // the minster's four casks at Sarre (premium 1.5 in the card: about 756 d before the mend)
            var sarre = GameData.Port("sarre");
            var wine = GameData.Good("rhenish_wine");
            double market = Commissions.MarketEach(sarre, wine, 793, 7, 0.0);
            double each = Commissions.PriceEach(sarre, wine, 4, 1.5, 793, 7, 0.0);
            Assert.Greater(market * 4 * 0.5, Commissions.BonusMax, "a half of four casks is more than the cap, or this test proves nothing");
            Assert.AreEqual(Commissions.BonusMax, (each - market) * 4, 0.05);
        }

        [Test]
        public void ARunOnTheStockDoesNotRaiseAnOrder()
        {
            // where the good is made, buying the town's stock out makes today's price dear; the order keeps to the usual one
            var dorestad = GameData.Port("dorestad");
            var wine = GameData.Good("rhenish_wine");
            var m = Markets.Of(dorestad, 793, 0.0);
            var mg = m.Get("rhenish_wine");
            double usual = m.UsualPrice(mg, 7) * (1f - m.Spread);
            mg.stock = 1f;
            Assert.Greater(m.Pays(mg, 7), usual, "today's price is dear");
            Assert.AreEqual(usual, Commissions.MarketEach(dorestad, wine, 793, 7, 0.0), 0.01);
        }

        [Test]
        public void BroughtInPartsEachPartPaidByTheUnitAndTolled()
        {
            Trade.Begin(rules, 660f, 0.0);                                // the first boat: four casks at a time
            var port = GameData.Port("traiectum");
            var wine = GameData.Good("rhenish_wine");
            var c = Offer("traiectum", "rhenish_wine", 6);
            Commissions.Take(c);
            Assert.AreEqual(4, Commissions.FitsAtOnce(c));
            Assert.AreEqual(2, Commissions.Loads(c));
            double standing = Reputation.At(port), silver = Trade.Purse.Silver(rules);
            double toll = rules.ZoneOf(port.region).toll;
            Assert.Greater(toll, 0.0, "a Frankish town takes a toll, or this test proves nothing");

            Trade.Hold.Add(wine, 4, 0.0, 0.0);
            StringAssert.StartsWith("Delivered", Commissions.Deliver(port));
            Assert.AreEqual(4, c.done);
            Assert.AreEqual(2, c.Left);
            Assert.IsTrue(Commissions.Mine.Contains(c), "it stands until the rest is brought");
            Assert.AreEqual(0, Trade.Hold.Count("rhenish_wine"));
            Assert.AreEqual(4 * c.each * (1.0 - toll), Trade.Purse.Silver(rules) - silver, 1.0, "four casks at the price a cask, less the toll");
            Assert.AreEqual(standing, Reputation.At(port), 1e-9, "a part earns no thanks yet");

            silver = Trade.Purse.Silver(rules);
            Trade.Hold.Add(wine, 3, 0.0, 0.0);
            StringAssert.StartsWith("Delivered", Commissions.Deliver(port));
            Assert.AreEqual(1, Trade.Hold.Count("rhenish_wine"), "only what was ordered is taken");
            Assert.AreEqual(2 * c.each * (1.0 - toll), Trade.Purse.Silver(rules) - silver, 1.0);
            Assert.IsFalse(Commissions.All.Contains(c), "all brought: the order is closed");
            Assert.Greater(Reputation.At(port), standing, "kept: standing up");
        }

        [Test]
        public void SheCannotTakeWhatSheCannotCarry()
        {
            Trade.Begin(rules, 660f, 0.0);
            var c = Offer("ripa", "horses", 1);
            Assert.AreEqual(0, Commissions.FitsAtOnce(c));
            Assert.IsNotEmpty(Commissions.WhyNot(c));
            Commissions.Take(c);
            Assert.IsFalse(c.mine, "not taken: one horse is more than her hold");
        }

        [Test]
        public void PastItsDayNothingBroughtIsBrokenAndPartBroughtIsShort()
        {
            var port = GameData.Port("traiectum");
            var wine = GameData.Good("rhenish_wine");
            var a = Offer("traiectum", "rhenish_wine", 4, 30.0);
            Commissions.Take(a);
            double before = Reputation.At(port);
            Commissions.Daily(31.0, 793, 8, 793);
            Assert.IsFalse(Commissions.All.Contains(a));
            double broke = before - Reputation.At(port);
            Assert.Greater(broke, 0.0, "nothing brought: they remember it");

            var b = Offer("traiectum", "rhenish_wine", 4, 30.0, 0.3, 40.0);
            Commissions.Take(b);
            Trade.Hold.Add(wine, 2, 0.0, 40.0);
            Commissions.Deliver(port);
            double silver = Trade.Purse.Silver(rules);
            before = Reputation.At(port);
            Commissions.Daily(71.0, 793, 9, 793);
            Assert.IsFalse(Commissions.All.Contains(b));
            double shortBy = before - Reputation.At(port);
            Assert.Less(shortBy, broke, "part brought costs less than none");
            Assert.AreEqual(silver, Trade.Purse.Silver(rules), 1e-9, "what was landed stays paid");
        }

        [Test]
        public void AWeekBeforeItsDayYouAreToldOnce()
        {
            var said = new List<string>();
            System.Action<string> hear = said.Add;
            Voyage.Notice += hear;
            try
            {
                var c = Offer("traiectum", "rhenish_wine", 4, 30.0);
                Commissions.Take(c);
                ClockDrops.Quiet = true;                                 // (the clock's drop for it is the play test's to see)
                Commissions.Daily(22.0, 793, 7, 793);
                Assert.AreEqual(0, said.Count(s => s.Contains("Traiectum")), "eight days left: nothing said yet");
                Commissions.Daily(23.0, 793, 7, 793);
                Commissions.Daily(24.0, 793, 7, 793);
                Assert.AreEqual(1, said.Count(s => s.Contains("days left for the order at Traiectum")), "said once");
                Commissions.Daily(29.0, 793, 7, 793);
                Assert.AreEqual(1, said.Count(s => s.StartsWith("Tomorrow is the day")));
            }
            finally
            {
                ClockDrops.Quiet = false;
                Voyage.Notice -= hear;
            }
        }

        /// <summary>The clock drops back for an order's day as it does for the debt's: a reason each, with its switch.</summary>
        [Test]
        public void AnOrdersDayIsAReasonForTheClockToDrop()
        {
            Assert.AreEqual(1, ClockSettings.Level(ClockReason.OrderNear), "a week before an order's day the clock should drop to speed 2");
            Assert.AreEqual(0, ClockSettings.Level(ClockReason.OrderDay), "on the last day it should drop to speed 1");
            Assert.IsNotEmpty(ClockSettings.Says(ClockReason.OrderNear));
            Assert.IsNotEmpty(ClockSettings.Says(ClockReason.OrderDay));
            StringAssert.DoesNotContain("OrderNear", ClockSettings.Label(ClockReason.OrderNear));
            StringAssert.DoesNotContain("OrderDay", ClockSettings.Label(ClockReason.OrderDay));
            Assert.IsTrue(ClockSettings.Default(ClockReason.OrderNear) && ClockSettings.Default(ClockReason.OrderDay));
        }

        /// <summary>While she works for the lender (the day missed: DECISIONS 104) his share of what an order fetches goes
        /// to the stick, as of any sale.</summary>
        [Test]
        public void WorkingForTheLenderHisShareOfAnOrderGoesToTheStick()
        {
            var port = GameData.Port("traiectum");
            var c = Offer("traiectum", "rhenish_wine", 2);
            Commissions.Take(c);
            Trade.Hold.Add(GameData.Good("rhenish_wine"), 2, 0.0, 0.0);
            var state = new Kapman.Encounters.EncounterState();
            ContentFlags.Use(state, () => 0.0);
            try
            {
                state.SetFlag(Trade.WorkFlag, 1.0, 0.0, 0.0);
                Assert.Greater(Trade.Debt, 1.0, "she owes nothing, so this test proves nothing");
                Assert.IsTrue(Trade.Working);
                double debt = Trade.Debt, silver = Trade.Purse.Silver(rules);
                double toll = rules.ZoneOf(port.region).toll;
                string fact = Commissions.Deliver(port);
                StringAssert.StartsWith("Delivered", fact);
                double took = 2 * c.each * (1.0 - toll);
                double share = System.Math.Floor(took * rules.workShare);
                Assert.AreEqual(debt - share, Trade.Debt, 0.5, "the lender's share of the order did not come off the debt");
                Assert.AreEqual(took - share, Trade.Purse.Silver(rules) - silver, 1.5, "the lender's share of the order did not leave the purse");
                Assert.Greater(fact.Length, "Delivered 2 casks of Rhenish wine as ordered".Length + 20);
            }
            finally
            {
                ContentFlags.Use(null);
            }
        }

        [Test]
        public void SavedAndLoadedAndAnOlderSaveRead()
        {
            var port = GameData.Port("traiectum");
            var c = Offer("traiectum", "rhenish_wine", 6);
            Commissions.Take(c);
            Trade.Hold.Add(GameData.Good("rhenish_wine"), 2, 0.0, 0.0);
            Commissions.Deliver(port);
            var waiting = Offer("sarre", "wax", 2);
            Commissions.Restore(Json.Parse(Json.Write(Commissions.Capture())) as List<object>);
            var mine = Commissions.Mine.Single();
            Assert.AreEqual(2, mine.done);
            Assert.AreEqual(c.each, mine.each, 1e-9);
            Assert.AreEqual(1, Commissions.OfferedToYou.Count, "an offer not yet answered is still an offer");
            Assert.AreEqual(waiting.id, Commissions.OfferedToYou[0].id);
            var next = Offer("sarre", "wax", 1);
            Assert.AreEqual(1, Commissions.All.Count(x => x.id == next.id), "a new order never shares an id with a saved one");
            // a save from before the mend: the whole pay and no price a unit
            Commissions.Restore(Json.Parse("[{\"id\":\"c7\",\"port\":\"traiectum\",\"good\":\"rhenish_wine\",\"units\":4,\"pay\":200,\"deadline\":50,\"posted\":10,\"mine\":true}]") as List<object>);
            Assert.AreEqual(50.0, Commissions.Mine.Single().each, 1e-9);
            Assert.AreEqual(0, Commissions.Mine.Single().done);
        }

        /// <summary>The gate for the data edits of Design/fixes/orders.md: no card may write an order over a half.</summary>
        [Test]
        public void NoCardWritesAnOrderOverAHalf()
        {
            var rx = new Regex(@"commission\s+[a-z_0-9]+\s+\d+\s+by\s+[\d.]+\s+premium\s+([\d.]+)");
            var files = Directory.GetFiles("Assets/Kapman/Game/Data/encounters", "*.json").ToList();
            files.Add("Assets/Kapman/Game/Data/events_793.json");
            var over = new List<string>();
            int seen = 0;
            foreach (string path in files)
                foreach (Match m in rx.Matches(File.ReadAllText(path)))
                {
                    seen++;
                    if (double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) > Commissions.PremiumMax + 1e-9)
                        over.Add(Path.GetFileName(path) + ": " + m.Value);
                }
            Assert.Greater(seen, 100, "the cards' orders were found");
            Assert.IsEmpty(over, over.Count + " orders in the cards are written over a half:\n" + string.Join("\n", over.Take(20)));
        }
    }
}
