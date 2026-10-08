using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE DEBT HAS AN END (8 Oct 2026, night; DECISIONS 33 and 104). The lender's man keeps turning up and any
    /// sum may be paid to him on the stick; on the day the lender asks for all of it; unpaid, she works for him (half
    /// of what she sells goes to the stick) until it is clean; a clean stick is given up. Until now the day passed
    /// and nothing happened.
    /// </summary>
    public class WokererTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const string Home = "harbour.wokerers_man.home", Away = "harbour.wokerers_man.away", Fog = "sea.wokerers_man.fog";
        const string DayHome = "harbour.wokerers_day.home", DayAway = "harbour.wokerers_day.away";
        const string CleanInTime = "harbour.wokerers_stick.in_time", CleanLate = "harbour.wokerers_stick.late", Aboard = "crew.wokerers_man.aboard";
        static readonly string[] All = { Home, Away, Fog, DayHome, DayAway, CleanInTime, CleanLate, Aboard };

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            Markets.Clear();
        }

        [TearDown]
        public void Done()
        {
            ContentFlags.Use(null);
            Markets.Clear();
        }

        [Test]
        public void TheLendersCardsAreWholeAndEveryChoiceHasItsEnd()
        {
            var book = EncounterData.Book();
            foreach (string id in All)
            {
                var c = book.Card(id);
                Assert.IsNotNull(c, "the card " + id + " is not in the book");
                Assert.IsNotNull(book.Written(c.titleKey), id + " has no title written");
                Assert.IsNotNull(book.Written(c.intentionKey), id + " has no scene written");
                foreach (var o in c.options)
                {
                    Assert.IsNotNull(book.Written(o.labelKey), id + " " + o.key + " has no label written");
                    if (o.Chance == null)
                        Assert.IsNotEmpty(Outcomes.Text(book, o, ""), id + " " + o.key + " has no written end");
                    else
                    {
                        Assert.IsNotEmpty(Outcomes.Text(book, o, "success"), id + " " + o.key + " has no end for the chance won");
                        Assert.IsNotEmpty(Outcomes.Text(book, o, "failure"), id + " " + o.key + " has no end for the chance lost");
                        Assert.AreNotEqual(Outcomes.Text(book, o, "success"), Outcomes.Text(book, o, "failure"), id + " " + o.key);
                    }
                }
            }
            // the man is one man wherever he turns up, and the lender is the Wursing of Dorestat's own page
            var man = book.Card(Home).actors.First(a => a.key == "man");
            Assert.IsNotEmpty(man.name);
            foreach (string id in new[] { Away, Fog, DayHome, DayAway, CleanLate, Aboard })
                Assert.AreEqual(man.cast, book.Card(id).actors.First(a => a.key == "man").cast, id + " has another man");
            Assert.AreEqual(FrameArt.LenderId, book.Card(DayHome).actors.First(a => a.key == "lender").cast);
            var wursing = PlaceBook.Current.Person(FrameArt.LenderId);
            Assert.IsNotNull(wursing, "Wursing is not a person of Dorestat");
            Assert.AreEqual(Trade.LenderName, wursing.name);
            Assert.IsTrue(wursing.Has("debt"), "nobody at Dorestat can be spoken to of the debt");
            Assert.IsTrue(wursing.Here(793));
            // the same man at home and far away
            var npcs = new NpcRegistry(793, book.roles, book.names);
            EncounterWorld At(string port) => new EncounterWorld
            {
                book = book, state = new EncounterState(), npcs = npcs, game = new FakeGame(), vars = new EncounterVars(),
                port = new PortFacts { id = port, realm = port == "ripa" ? "danes" : "franks", faith = "christian", peoples = { "frisian" } }, year = 793, day = 20.0,
            };
            var atHome = EncounterSession.Open(book.Card(Home), At("dorestad")).People.First();
            var farOff = EncounterSession.Open(book.Card(Away), At("ripa")).People.First();
            Assert.AreSame(atHome, farOff, "the lender's man at Ripa is another man than at Dorestat");
            Assert.AreEqual(man.name, atHome.name);
        }

        /// <summary>A director over the lender's cards and nothing else, with the fake game and the numbers set by hand.</summary>
        static (EncounterDirector d, FakeGame game) Director()
        {
            var game = new FakeGame();
            var d = new EncounterDirector(EncounterData.BookOf(All), 793, game);
            d.portInfo = port => port == "dorestad" ? ("franks", "frisian") : ("danes", "danish");
            d.roll = () => 0.0;
            d.vars.nums["silver"] = 300.0;
            d.vars.nums["debt"] = 1920.0;
            d.vars.nums["debt_short"] = 1620.0;
            d.vars.nums["fog"] = 0.0;
            d.state.SetFlag(Trade.LenderFlag, 1.0, 0.0, 0.0);
            return (d, game);
        }

        static List<string> Call(EncounterDirector d, string port)
        {
            var came = d.BeginCall(port).Select(c => c.id).ToList();
            return came;
        }

        static void Leave(EncounterDirector d)
        {
            for (int i = 0; i < 12; i++)
            {
                var s = d.OpenNext();
                if (s == null)
                    break;
                var r = d.ChooseDefault();
                if (r == null || !r.ok)
                    d.Choose(s.options.FindIndex(o => o.enabled));
                d.Continue();
            }
            d.EndCall();
        }

        [Test]
        public void HisManComesInHisOwnTimeAndSoonerWhenHeWasPutOff()
        {
            var (d, game) = Director();
            // the first ten days of the game: nobody
            d.vars.nums["debt_due_days"] = 150.0;
            d.SetTime(1.0, 793, 5, 24);
            CollectionAssert.DoesNotContain(Call(d, "dorestad"), Home, "he is on the dam head on the first morning");
            Leave(d);
            // then he is there when she moors at home
            d.vars.nums["debt_due_days"] = 130.0;
            d.SetTime(21.0, 793, 5, 21 * 24);
            CollectionAssert.Contains(Call(d, "dorestad"), Home);
            var s = d.OpenNext();
            Assert.AreEqual(Home, s.card.id);
            StringAssert.Contains("I carry the other half", string.Join(" ", s.lines.Select(l => l.text)), "he does not say who he is the first time");
            // a pound cannot be paid with 300 d aboard? it can: 240
            Assert.IsTrue(s.options[s.card.Option("pound").index].enabled);
            Assert.IsFalse(s.options[s.card.Option("all").index].enabled, "all of it can be paid with 300 d aboard");
            var r = d.Choose(s.card.Option("pound").index);
            Assert.IsTrue(r.ok, r.Text);
            CollectionAssert.Contains(game.laws.Select(l => l.kind).ToList(), "debt_pay_240");
            Assert.IsNotEmpty(r.written, "a pound paid has no written end");
            d.Continue();
            Leave(d);
            // he has had his word: not the next day, nor ten days on; after twenty-four days, again
            d.SetTime(22.0, 793, 5, 22 * 24);
            CollectionAssert.DoesNotContain(Call(d, "dorestad"), Home, "he asks again the day after he was paid");
            Leave(d);
            d.vars.nums["debt_due_days"] = 110.0;
            d.SetTime(41.0, 793, 6, 41 * 24);
            CollectionAssert.DoesNotContain(Call(d, "ripa"), Away, "he is at Ripa twenty days after he was paid at home");
            Leave(d);
            d.vars.nums["debt_due_days"] = 100.0;
            d.SetTime(51.0, 793, 6, 51 * 24);
            CollectionAssert.Contains(Call(d, "ripa"), Away, "thirty days on he has not found her at Ripa");
            s = d.OpenNext();
            Assert.AreEqual(Away, s.card.id);
            StringAssert.Contains("again", string.Join(" ", s.lines.Select(l => l.text)), "he does not know her the second time");
            // put off: he comes sooner
            r = d.Choose(s.card.Option("put_off").index);
            Assert.IsTrue(r.ok);
            d.Continue();
            Leave(d);
            d.vars.nums["debt_due_days"] = 95.0;
            d.SetTime(56.0, 793, 6, 56 * 24);
            CollectionAssert.DoesNotContain(Call(d, "dorestad"), Home, "five days after he was put off he is back");
            Leave(d);
            d.vars.nums["debt_due_days"] = 88.0;
            d.SetTime(63.0, 793, 7, 63 * 24);
            CollectionAssert.Contains(Call(d, "dorestad"), Home, "twelve days after he was put off he has not come");
            s = d.OpenNext();
            StringAssert.Contains("next time", string.Join(" ", s.lines.Select(l => l.text)), "he does not remember being put off");
            Assert.IsTrue(d.Choose(s.card.Option("put_off").index).ok);
            d.Continue();
            Leave(d);
            // close to the day he is there every few days
            d.vars.nums["debt_due_days"] = 12.0;
            d.SetTime(139.0, 793, 9, 139 * 24);
            CollectionAssert.Contains(Call(d, "dorestad"), Home);
            Leave(d);
            d.vars.nums["debt_due_days"] = 8.0;
            d.SetTime(143.0, 793, 9, 143 * 24);
            CollectionAssert.Contains(Call(d, "dorestad"), Home, "four days on, with Michaelmas near, he has not come back");
            Leave(d);
        }

        [Test]
        public void OnTheDayHeAsksForAllOfItAndUnpaidSheWorksForHim()
        {
            var (d, game) = Director();
            d.vars.nums["debt_due_days"] = 0.0;
            d.SetTime(151.0, 793, 9, 151 * 24);
            var came = Call(d, "dorestad");
            CollectionAssert.Contains(came, DayHome, "Michaelmas, at home, with the stick not clean: nobody is on the dam head");
            CollectionAssert.DoesNotContain(came, Home, "his man asks for a part on the day itself");
            var s = d.OpenNext();
            Assert.AreEqual(DayHome, s.card.id);
            Assert.AreEqual(2, s.People.Count(), "the lender and his man");
            Assert.AreEqual(Trade.LenderName, s.People.First().name);
            Assert.IsFalse(s.options[s.card.Option("all").index].enabled, "all of it can be counted out with 300 d aboard");
            var r = d.Choose(s.card.Option("short").index);
            Assert.IsTrue(r.ok, r.Text);
            StringAssert.Contains("works for him", r.written);
            Assert.AreNotEqual(0.0, d.state.Flag(Trade.WorkFlag, d.Day), "she does not work for him after the day went unpaid");
            CollectionAssert.Contains(game.deeds.Select(x => x.kind).ToList(), "missed_the_day");
            d.Continue();
            Leave(d);
            // he does not ask twice: neither he nor his man is on the dam head the next time
            d.vars.nums["debt_due_days"] = -9.0;
            d.SetTime(160.0, 793, 10, 160 * 24);
            came = Call(d, "dorestad");
            CollectionAssert.DoesNotContain(came, DayHome);
            CollectionAssert.DoesNotContain(came, Home);
            Leave(d);
            CollectionAssert.DoesNotContain(Call(d, "ripa"), DayAway);
            Leave(d);
            // the stick clean at last: he gives it up, and his man comes ashore
            d.vars.nums["debt"] = 0.0;
            d.vars.nums["debt_short"] = 0.0;
            d.SetTime(200.0, 793, 11, 200 * 24);
            came = Call(d, "dorestad");
            CollectionAssert.Contains(came, CleanLate);
            CollectionAssert.DoesNotContain(came, CleanInTime);
            s = d.OpenNext();
            Assert.AreEqual(CleanLate, s.card.id);
            Assert.IsTrue(d.Choose(s.card.Option("take").index).ok);
            d.Continue();
            Assert.AreEqual(0.0, d.state.Flag(Trade.WorkFlag, d.Day), "she still works for him with the stick clean");
            Leave(d);
            CollectionAssert.IsEmpty(Call(d, "dorestad").Where(All.Contains).ToList(), "the stick is given up twice");
        }

        [Test]
        public void PaidOnTheDayTheStickIsGivenUpAtOnce()
        {
            var (d, game) = Director();
            d.vars.nums["debt_due_days"] = 0.0;
            d.vars.nums["silver"] = 2000.0;
            d.vars.nums["debt_short"] = 0.0;
            d.SetTime(151.0, 793, 9, 151 * 24);
            CollectionAssert.Contains(Call(d, "dorestad"), DayHome);
            var s = d.OpenNext();
            var r = d.Choose(s.card.Option("all").index);
            Assert.IsTrue(r.ok, r.Text);
            CollectionAssert.Contains(game.laws.Select(l => l.kind).ToList(), "debt_paid");
            Assert.AreEqual(0.0, d.state.Flag(Trade.WorkFlag, d.Day));
            d.vars.nums["debt"] = 0.0;                                   // (the fake game does not pay: the real one is the next test)
            d.Continue();
            s = d.OpenNext();
            Assert.IsNotNull(s, "nothing follows the day paid");
            Assert.AreEqual(CleanInTime, s.card.id, "the clean stick does not follow the day paid");
            Assert.AreEqual("Ravn", s.People.Last().name);
            Assert.IsTrue(d.Choose(s.card.Option("take").index).ok);
            CollectionAssert.Contains(game.deeds.Select(x => x.kind).ToList(), "paid_the_stick");
            d.Continue();
            Leave(d);
            // a week past the day, far from home, with the stick not clean: his man comes with the rope
            var (far, _) = Director();
            far.vars.nums["debt_due_days"] = -3.0;
            far.SetTime(154.0, 793, 10, 154 * 24);
            CollectionAssert.DoesNotContain(Call(far, "ripa"), DayAway, "three days past the day the rope is at Ripa already");
            Leave(far);
            far.vars.nums["debt_due_days"] = -8.0;
            far.SetTime(159.0, 793, 10, 159 * 24);
            CollectionAssert.Contains(Call(far, "ripa"), DayAway);
            Leave(far);
            Assert.AreNotEqual(0.0, far.state.Flag(Trade.WorkFlag, far.Day), "with 300 d aboard the day's default is that she works for him");
        }

        [Test]
        public void APartPaidThroughACardComesOffTheStick()
        {
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            var book = EncounterData.Book();
            var effects = new GameEffects { book = book, portNow = () => "dorestad" };
            var rules = TradeRules.Current;
            double owed = Trade.Debt, silver = Trade.Purse.Silver(rules);
            Assert.Greater(owed, 1000.0);
            Assert.Greater(silver, 60.0, "the starting purse has not sixty pennies' worth");
            string said = effects.Law("debt_pay_60", 0.0, "dorestad");
            Assert.AreEqual(owed - 60.0, Trade.Debt, 0.01, said);
            Assert.AreEqual(silver - 60.0, Trade.Purse.Silver(rules), 6.0, "sixty pennies of silver did not leave the purse");
            StringAssert.Contains("on the stick", said);
            StringAssert.Contains("Still owed", said);
            // more asked than is aboard: what is aboard goes, and no more comes off the stick than was given
            double left = Trade.Purse.Silver(rules), before = Trade.Debt;
            effects.Law("debt_pay_240", 0.0, "dorestad");
            double given = left - Trade.Purse.Silver(rules);
            Assert.Greater(given, 1.0, "nothing was given though there was silver aboard");
            Assert.Less(given, 240.0);
            Assert.AreEqual(before - given, Trade.Debt, 6.0);
            // all that is aboard; and with nothing aboard, nothing paid, and said
            effects.Law("debt_all_aboard", 0.0, "dorestad");
            double now = Trade.Debt;
            StringAssert.Contains("no silver", effects.Law("debt_all_aboard", 0.0, "dorestad") ?? "");
            Assert.AreEqual(now, Trade.Debt, 0.001);
            // a sum that is no number does nothing
            Assert.IsNull(effects.Law("debt_pay_x", 0.0, "dorestad"));
            Assert.AreEqual(now, Trade.Debt, 0.001);
        }

        [Test]
        public void WhileSheWorksForHimHalfOfASaleGoesToTheStick()
        {
            var rules = TradeRules.Current;
            Assert.AreEqual(0.5f, rules.workShare, 0.001f, "the rules' share of the boat's work");
            var state = new EncounterState();
            ContentFlags.Use(state, () => 0.29);
            Trade.Begin(rules, 660f, 0.29, 793, 121);
            var wine = GameData.Good("rhenish_wine");
            Trade.Hold.Add(wine, 4, 0.0, 0.29, "");
            var market = Markets.Of(GameData.Port("dorestad"), 793, 0.29);
            // she does not work for him: all of it is hers
            Assert.IsFalse(Trade.Working);
            double purse = Trade.Purse.Silver(rules), owed = Trade.Debt;
            var sold = Trade.Sell(market, "rhenish_wine", 1, 0.29, 5, 793);
            Assert.IsTrue(sold.ok, sold.text);
            double took = Trade.Purse.Silver(rules) - purse;
            Assert.Greater(took, 10.0);
            Assert.AreEqual(owed, Trade.Debt, 0.001, "a sale paid the stick with no word from the lender");
            StringAssert.DoesNotContain("stick", sold.text);
            // from the day he says so: half before it reaches the purse
            state.SetFlag(Trade.WorkFlag, 1.0, 0.29, 0.0);
            Assert.IsTrue(Trade.Working);
            purse = Trade.Purse.Silver(rules);
            sold = Trade.Sell(market, "rhenish_wine", 1, 0.29, 5, 793);
            Assert.IsTrue(sold.ok, sold.text);
            double net = sold.deniers - sold.toll, share = System.Math.Floor(net * 0.5);
            Assert.AreEqual(owed - share, Trade.Debt, 0.01, sold.text);
            Assert.AreEqual(purse + net - share, Trade.Purse.Silver(rules), 1.0, "the purse did not keep the other half");
            StringAssert.Contains("went to the stick", sold.text);
            StringAssert.Contains("Still owed", sold.text);
            // the last of it: no more than is owed, and then she is her own again
            Trade.Debt = 5.0;
            purse = Trade.Purse.Silver(rules);
            sold = Trade.Sell(market, "rhenish_wine", 1, 0.29, 5, 793);
            Assert.IsTrue(sold.ok, sold.text);
            Assert.AreEqual(0.0, Trade.Debt, 0.001);
            Assert.AreEqual(purse + (sold.deniers - sold.toll) - 5.0, Trade.Purse.Silver(rules), 1.0);
            StringAssert.Contains("the stick is clean", sold.text);
            Assert.IsFalse(Trade.Working, "she works for him with nothing owed");
        }
    }
}
