using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The word against where she lies, the ledger's cells, and dealing by the lot (Design/fixes/word_and_lots.md, after
    /// the strategy critic of 7 October). The word: said against the price she has in front of her, never against the
    /// good's table value while she has one; a figure for what she saw, "about" for what a man who saw it told her, a
    /// word for talk. The ledger: a cell and the line under a good never disagree. The lot: a quote is what the deal
    /// comes to and moves nothing; the purse settles once and no longer rounds to the whole coin.
    /// The lines are checked against the built-in words (the table is not loaded), as in KnownWordTests.
    /// Game day 0 is 21 June 793; day 40 is 31 July.
    /// </summary>
    public class WordAndLotsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const double Today = 40.0;

        static TextAsset Asset(string file) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + file);

        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            rules = TradeRules.Load(Asset("trade_rules.json"));
            Known.Load(Asset("knowing_793.json"));
            Chronicle.Load(Asset("chronicle_793.json"));
            Shocks.Load(Asset("shocks_793.json"));
            Seasons.Load(Asset("seasons_793.json"));
            Seasons.SetStart(793, 172);
            TradeYear.Load(File.ReadAllText(Data + "trade_year_793.json"));
            Wording.Load(null);                                              // the built-in words
            Markets.Clear();
            Known.Clear();
            Trade.Begin(TradeRules.Current, 900f, 0.0);                      // a new merchant: an empty ledger
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Known.Clear();
            Known.Load(Asset("knowing_793.json"));
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Wording.Load(Asset("wording_793.json"));
        }

        static PortDef Dorestad => GameData.Port("dorestad");
        static PortDef Traiectum => GameData.Port("traiectum");
        static PortDef London => GameData.Port("lundenwic");
        static PortDef Quentovic => GameData.Port("quentovic");
        static PortDef Ripa => GameData.Port("ripa");
        static GoodDef Wine => GameData.Good("rhenish_wine");

        static WordAsk Ask(double today = Today, PortDef here = null) => new WordAsk { today = today, here = here };

        static Market MarketOf(PortDef port, double day) => new Market(port, TradeRules.Current, 793, day);

        /// <summary>The merchant sees a market's prices on a day.</summary>
        static void See(PortDef port, double day) => Trade.Ledger.Note(MarketOf(port, day), day, 7);

        /// <summary>What she noted that a market paid for a good (its newest note).</summary>
        static float Noted(PortDef port, GoodDef good) => Trade.Ledger.Latest(port.id).prices[good.id].pays;

        /// <summary>The merchant is told what one market paid for one good on a day: a price of the test's own choosing.</summary>
        static void TellPays(PortDef port, GoodDef good, float pays, double day, How how, int hops, string teller = null, string tellerOf = null)
        {
            int n = GameData.Goods.Count;
            var snap = new PriceSnapshot { port = port.index, day = day, asks = new float[n], pays = new float[n] };
            for (int i = 0; i < n; i++)
                snap.asks[i] = snap.pays[i] = float.NaN;
            snap.pays[good.index] = pays;
            Trade.Ledger.Hear(snap, "a label that is never shown", how, hops, day, teller, tellerOf);
        }

        static bool HasDigit(string text) => Regex.IsMatch(text ?? "", @"\d");

        // ------------------------------------------------------------------ the word, against where she lies

        [Test]
        public void TraiectumPaysMoreThanHomeAndTheWordSaysSo()
        {
            // the critic's case: home pays 34 d, Traiectum 36 d, and the word said "Traiectum was paying poorly"
            See(Dorestad, Today);
            float home = Noted(Dorestad, Wine), there = home * 36f / 34f;
            TellPays(Traiectum, Wine, there, Today - 20.0, How.Told, 0);     // (told long enough ago to be talk: a word, and no error in it)
            var w = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual(4, w.rule);
            Assert.AreEqual("more than here", w.Slot("pays"));
            Assert.AreEqual(1, w.tilt, "the wedge points up: they pay more there than where she lies");
            Assert.AreEqual("They say Traiectum pays more than here for Rhenish wine. A fortnight ago.", w.Text);
            StringAssert.DoesNotContain("poorly", w.Text);
            Assert.IsFalse(HasDigit(w.Text), "talk is a word, never a figure: " + w.Text);
            // and the other way round: a port that pays less than here is said to pay less, whatever the good's table value
            TellPays(Quentovic, Wine, home * 0.9f, Today - 20.0, How.Told, 0);
            var less = Known.Word(Wine, Quentovic, Ask(Today, Dorestad));
            Assert.AreEqual("less than here", less.Slot("pays"));
            Assert.AreEqual(-1, less.tilt);
            // a price within a twentieth of the one here is much the same
            TellPays(London, Wine, home * 1.03f, Today - 20.0, How.Told, 0);
            var same = Known.Word(Wine, London, Ask(Today, Dorestad));
            Assert.AreEqual("much the same as here", same.Slot("pays"));
            Assert.AreEqual(0, same.tilt);
        }

        [Test]
        public void AManWhoSawItSaysAboutAndAFigure()
        {
            See(Dorestad, Today);
            float home = Noted(Dorestad, Wine), there = home * 1.5f;
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Told, 0, "skipper", "dorestad");
            var zone = TradeRules.Current.ZoneOf(Dorestad.region);
            string about = Known.Rough(there, zone);
            var w = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual(3, w.rule);
            Assert.AreEqual("told.about", w.key);
            Assert.AreEqual(WordMark.Heard, w.mark);
            Assert.AreEqual(about, w.Slot("price"));
            Assert.AreEqual($"A Frankish skipper said Traiectum was paying about {about} for Rhenish wine, yesterday. That is far more than here.", w.Text);
            Assert.AreEqual(1, w.tilt);
            // the same telling through one more mouth is a word, and the same word
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            See(Dorestad, Today);
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Talk, 1, "quay", "dorestad");
            var talk = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual("told", talk.key);
            Assert.AreEqual("far more than here", talk.Slot("pays"));
            Assert.IsFalse(HasDigit(talk.Text), talk.Text);
            // with the switch off (knowing_793.json word.told_figure) a telling at first hand is a word too: the rule of 4 October
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            See(Dorestad, Today);
            TellPays(Traiectum, Wine, there, Today - 1.0, How.Told, 0, "skipper", "dorestad");
            Known.Rules.toldFigure = false;
            var word = Known.Word(Wine, Traiectum, Ask(Today, Dorestad));
            Assert.AreEqual("told", word.key);
            Assert.IsFalse(HasDigit(word.Text), word.Text);
        }

        [Test]
        public void ARoughFigureCannotBeTakenForAPriceSeen()
        {
            var d = TradeRules.Current.ZoneOf(Dorestad.region);
            Assert.AreEqual("4 d", Known.Rough(4.3, d));
            Assert.AreEqual("19 d", Known.Rough(19.4, d));
            Assert.AreEqual("36 d", Known.Rough(36.4, d), "under fifty: to the nearest two");
            Assert.AreEqual("38 d", Known.Rough(37.0, d));
            Assert.AreEqual("60 d", Known.Rough(59.0, d), "under two hundred: to the nearest five");
            Assert.AreEqual("480 d", Known.Rough(476.0, d), "then to the nearest ten");
            Assert.AreEqual("1 d", Known.Rough(0.2, d), "never nothing");
            StringAssert.EndsWith(" p", Known.Rough(59.0, TradeRules.Current.ZoneOf(London.region)));
        }

        [Test]
        public void APriceSeenIsSetAgainstHereAsAFigure()
        {
            See(Dorestad, Today);
            See(Traiectum, Today - 1.0);
            float home = Noted(Dorestad, Wine), there = Noted(Traiectum, Wine);
            var ask = Ask(Today, Dorestad);
            var cell = Known.Cell(Wine, Traiectum, ask);
            Assert.AreEqual(PriceGrade.Exact, cell.grade);
            Assert.IsTrue(cell.compared);
            var w = Known.Word(Wine, Traiectum, ask);
            Assert.AreEqual(2, w.rule);
            Assert.AreEqual("seen", w.key);
            Assert.AreEqual(System.Math.Sign(cell.paysLean), w.tilt);
            var zone = TradeRules.Current.ZoneOf(Dorestad.region);
            string start = $"You saw Rhenish wine pay {zone.Show(there)} at Traiectum, yesterday. That is ";
            StringAssert.StartsWith(start, w.Text);
            double by = there - home;
            if (System.Math.Abs(by) >= 0.05)
                Assert.AreEqual(start + zone.Show(System.Math.Abs(by)) + (by > 0.0 ? " more than here." : " less than here."), w.Text);
            else
                Assert.AreEqual(start + "much the same as here.", w.Text);
            // the port she lies at is not set against itself
            var here = Known.Word(Wine, Dorestad, ask);
            Assert.AreEqual($"You saw Rhenish wine pay {zone.Show(home)} at Dorestat, this morning.", here.Text);
            Assert.AreEqual(0, here.tilt);
        }

        [Test]
        public void AtSeaTheWordIsSaidAgainstWhereSheLastSawOne()
        {
            See(Traiectum, Today - 9.0);
            See(Dorestad, Today - 2.0);
            var here = Known.Here(Wine, Ask());
            Assert.IsTrue(here.Known);
            Assert.IsFalse(here.lying);
            Assert.AreEqual("dorestad", here.port.id, "the newest of her own sights");
            Assert.AreEqual(Noted(Dorestad, Wine), here.pays);
            TellPays(London, Wine, here.pays * 1.6f, Today - 20.0, How.Told, 0);
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual("They say Lundenwic pays far more than at Dorestat for Rhenish wine. A fortnight ago.", w.Text);
            // lying at a port whose market she has seen today, that port is "here"
            See(Traiectum, Today);
            var lying = Known.Here(Wine, Ask(Today, Traiectum));
            Assert.IsTrue(lying.lying);
            Assert.AreEqual("traiectum", lying.port.id);
            // lying where she has never looked at the market, it is still the last one she saw
            Assert.AreEqual("traiectum", Known.Here(Wine, Ask(Today, Quentovic)).port.id);
            Assert.IsFalse(Known.Here(Wine, Ask(Today, Quentovic)).lying);
            // a sight ninety days old is no longer a price to set anything against
            Assert.IsFalse(Known.Here(Wine, Ask(Today + 95.0)).Known);
        }

        [Test]
        public void FarMoreMeansCarryingItTherePays()
        {
            float half = rules.spread * 0.5f, gap = (1f + half) / (1f - half);
            float tollD = rules.ZoneOf(Dorestad.region).toll, tollR = rules.ZoneOf(Ripa.region).toll;
            Assert.AreEqual(gap / (1f - tollD) - 1f, Known.FarOver(Dorestad), 1e-5f);
            Assert.AreEqual(gap / (1f - tollR) - 1f, Known.FarOver(Ripa), 1e-5f);
            Assert.Greater(Known.FarOver(Dorestad), Known.FarOver(Ripa), "a toll where it is sold asks a wider gap");
            float far = Known.FarOver(Dorestad);
            Assert.AreEqual(2, Known.Lean(100f * (1f + far) + 0.5f, 100f, far, far));
            Assert.AreEqual(1, Known.Lean(100f * (1f + far) - 0.5f, 100f, far, far));
            Assert.AreEqual(1, Known.Lean(106f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(104f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(96f, 100f, far, far));
            Assert.AreEqual(-1, Known.Lean(95f, 100f, far, far));
            Assert.AreEqual(-2, Known.Lean(100f / (1f + far) - 0.5f, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(float.NaN, 100f, far, far));
            Assert.AreEqual(0, Known.Lean(50f, 0f, far, far));
            // what the word means: bought here at what they ask, sold there at what they pay less the toll
            foreach (float paysThere in new[] { 60f, 100f, 120f, 130f, 135f, 136f, 140f, 200f })
            {
                const float paysHere = 100f;
                float asksHere = paysHere * gap, inHand = paysThere * (1f - tollD);
                Assert.AreEqual(inHand > asksHere, Known.Lean(paysThere, paysHere, far, far) == 2, $"they pay {paysThere} there");
            }
        }

        // ------------------------------------------------------------------ the ledger

        [Test]
        public void ACellAndItsLineAgree()
        {
            See(London, Today);                                              // where she lies
            See(Quentovic, Today - 2.0);                                     // her own eyes this week: the figure
            See(Dorestad, Today - 10.0);                                     // her own eyes, longer ago: about
            TellPays(Traiectum, Wine, 40f, Today - 3.0, How.Told, 0, "skipper", "dorestad");   // a man who saw it: about
            TellPays(Ripa, Wine, 90f, Today - 3.0, How.Talk, 3, "quay", "dorestad");            // talk: a word
            var ask = Known.AskFor(Wine, Ask(Today, London));
            var grades = new Dictionary<string, PriceGrade>
            {
                ["quentovic"] = PriceGrade.Exact, ["dorestad"] = PriceGrade.About, ["traiectum"] = PriceGrade.About,
                ["ripa"] = PriceGrade.Word, ["birka"] = PriceGrade.None, ["lundenwic"] = PriceGrade.Exact,
            };
            foreach (var kv in grades)
            {
                var port = GameData.Port(kv.Key);
                var cell = Known.Cell(Wine, port, ask);
                string text = Known.CellText(cell, port, ask, false);
                var line = Known.Word(Wine, port, ask);
                Assert.AreEqual(kv.Value, cell.grade, kv.Key);
                Assert.AreEqual(kv.Key != "lundenwic" && kv.Value != PriceGrade.None, cell.compared, kv.Key + ": set against where she lies, but for that port itself");
                switch (kv.Value)
                {
                    case PriceGrade.Exact:
                        Assert.AreEqual(line.Slot("price"), text, kv.Key + ": the cell is the line's figure");
                        Assert.IsTrue(HasDigit(text));
                        StringAssert.EndsWith(" p", text, "in the reckoning of where she lies");
                        break;
                    case PriceGrade.About:
                        Assert.AreEqual("about " + line.Slot("price"), text, kv.Key + ": the cell is the line's rough figure");
                        StringAssert.Contains("about " + line.Slot("price"), line.Text);
                        break;
                    case PriceGrade.Word:
                        Assert.IsFalse(HasDigit(text), text);
                        Assert.IsFalse(HasDigit(line.Text), line.Text);
                        StringAssert.Contains(text, line.Slot("pays"), "'far more' in the cell, 'far more than here' in the line");
                        break;
                    default:
                        Assert.AreEqual("", text);
                        Assert.GreaterOrEqual(line.rule, 5, "nothing noted: the line is the season, the ships ahead, the law, or no word");
                        break;
                }
                if (cell.compared)
                    Assert.AreEqual(System.Math.Sign(cell.paysLean), line.tilt, kv.Key + ": the wedge is the cell's lean");
            }
            // what they ask: where none was on offer the cell says so
            var none = Known.Cell(Wine, GameData.Port("traiectum"), ask);
            Assert.IsFalse(none.onOffer, "he told what they paid, and nothing was on offer");
            Assert.AreEqual("none", Known.CellText(none, GameData.Port("traiectum"), ask, true));
        }

        [Test]
        public void AQuoteIsWhatTheDealComesToAndMovesNothing()
        {
            var market = MarketOf(Dorestad, 0);
            var g = market.Goods.First(x => x.OnOffer && x.good.value < 10f && x.stock >= 6f);
            string id = g.good.id;
            float stock = g.stock, glut = g.glut;
            double worth = Trade.Purse.Worth(market.zone, rules);
            var quote = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.IsTrue(quote.ok);
            Assert.AreEqual(3, quote.count);
            Assert.AreEqual(stock, g.stock, "a quote takes nothing from the market");
            Assert.AreEqual(worth, Trade.Purse.Worth(market.zone, rules), 1e-9, "nor from the purse");
            Assert.AreEqual(0, Trade.Hold.Count(id));
            var bought = Deal.Buy(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.AreEqual(quote.count, bought.count);
            Assert.AreEqual(quote.deniers, bought.deniers, 1e-9, "the lot costs what it was quoted at");
            Assert.AreEqual(worth - bought.deniers, Trade.Purse.Worth(market.zone, rules), 0.25, "one settlement: the purse is down by the sum, to within the scales and the mint's tenth on the change");
            // and the sale
            stock = g.stock;
            glut = g.glut;
            var sellQuote = Deal.QuoteSell(market, Trade.Hold, id, 3, 6, 0, "player");
            Assert.IsTrue(sellQuote.ok);
            Assert.AreEqual(stock, g.stock);
            Assert.AreEqual(glut, g.glut);
            Assert.AreEqual(3, Trade.Hold.Count(id));
            Assert.AreEqual(bought.deniers, sellQuote.cost, 1e-6, "what they cost her, with nothing taken out of the hold");
            var sold = Deal.Sell(market, Trade.Purse, Trade.Hold, id, 3, 6, 0, "player");
            Assert.AreEqual(sellQuote.count, sold.count);
            Assert.AreEqual(sellQuote.deniers, sold.deniers, 1e-9);
            Assert.AreEqual(sellQuote.toll, sold.toll, 1e-9);
            Assert.AreEqual(sellQuote.cost, sold.cost, 1e-6);
        }

        [Test]
        public void AllThatFitsStopsAtTheStockTheRoomAndTheSilver()
        {
            var market = MarketOf(Dorestad, 0);
            var g = market.Goods.First(x => x.OnOffer && x.good.value >= 2f && x.good.value < 10f && x.stock >= 6f);
            string id = g.good.id;
            // plenty of silver and room: all that is on offer
            Trade.Purse.Add("denier", 100000);
            Trade.Hold.capacity = 1e7f;
            var all = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(Mathf.FloorToInt(g.stock), all.count);
            Assert.AreEqual("none_left", all.why);
            // room for two only
            Trade.Hold.capacity = g.good.Space * 2.5f;
            var room = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(2, room.count);
            Assert.AreEqual("no_room", room.why);
            // silver for the first two only
            Trade.Hold.capacity = 1e7f;
            double two = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, 2, 6, 0, "player").deniers;
            Trade.Purse.Clear();
            Trade.Purse.Add("denier", System.Math.Ceiling(two));
            var silver = Deal.QuoteBuy(market, Trade.Purse, Trade.Hold, id, int.MaxValue, 6, 0, "player");
            Assert.AreEqual(2, silver.count);
            Assert.AreEqual("no_silver", silver.why);
            // the lots of the buttons
            Assert.AreEqual(0, FrameDeal.CountOf(LotSize.One, 0));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.One, 5));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.Half, 1));
            Assert.AreEqual(1, FrameDeal.CountOf(LotSize.Half, 2));
            Assert.AreEqual(2, FrameDeal.CountOf(LotSize.Half, 3));
            Assert.AreEqual(2, FrameDeal.CountOf(LotSize.Half, 4));
            Assert.AreEqual(3, FrameDeal.CountOf(LotSize.Half, 5));
            Assert.AreEqual(5, FrameDeal.CountOf(LotSize.All, 5));
        }

        [Test]
        public void ThePurseGivesChangeUnderACoinInCutSilver()
        {
            var frankish = rules.ZoneOf("rhine_delta");
            double perGram = rules.Coin("hacksilver").Deniers;
            var p = new Purse();
            p.Add("denier", 10);
            Assert.IsTrue(p.Pay(5.2, frankish, rules, out string how));
            Assert.AreEqual(4.0, p.Count("denier"), 1e-9, "six whole coins are handed over");
            Assert.AreEqual(1.3, p.Count("hacksilver"), 1e-9, "and the 0.8 d over comes back weighed out, never more than is due");
            Assert.AreEqual("1.3 g of hacksilver", p.LastBack);
            StringAssert.Contains("back", how);
            Assert.AreEqual(10.0 - 5.2, p.Silver(rules), 0.06, "by weight she is down by the sum, to a tenth of a gram");
            Assert.LessOrEqual(p.Silver(rules), 10.0 - 5.2 + 1e-9, "the market never gives over");
            // a sum under one coin is made up from the cut silver while there is enough of it: no coin is broken
            Assert.IsTrue(p.Pay(0.5, frankish, rules, out _));
            Assert.AreEqual(4.0, p.Count("denier"), 1e-9);
            Assert.Less(p.Count("hacksilver"), 1.3);
            // a whole sum takes whole coins and gives nothing back
            var q = new Purse();
            q.Add("denier", 10);
            Assert.IsTrue(q.Pay(8, frankish, rules, out _));
            Assert.AreEqual(2.0, q.Count("denier"), 1e-9);
            Assert.AreEqual(0.0, q.Count("hacksilver"), 1e-9);
            Assert.AreEqual("", q.LastBack);
            // what is taken in a sale: whole coins and the rest weighed out, not the nearest coin
            var r = new Purse();
            r.Receive(32.4, frankish, rules, out string took);
            Assert.AreEqual(32.0, r.Count("denier"), 1e-9);
            Assert.AreEqual(0.6, r.Count("hacksilver"), 1e-9);
            Assert.AreEqual("took 32 deniers and 0.6 g of hacksilver", took);
            Assert.AreEqual(32.4, r.Silver(rules), perGram * 0.1 + 1e-9);
            var s = new Purse();
            s.Receive(32.76, frankish, rules, out _);
            Assert.AreEqual(32.0, s.Count("denier"), 1e-9, "32.76 is no longer 33");
            Assert.AreEqual(1.2, s.Count("hacksilver"), 1e-9);
            // where silver goes by weight nothing has changed: all of it on the scales
            var north = rules.ZoneOf("jutland");
            var w = new Purse();
            w.Receive(10.0, north, rules, out _);
            Assert.AreEqual(0.0, w.Count("denier"));
            Assert.AreEqual(10.0, w.Worth(north, rules), 0.05);
        }

        [Test]
        public void TheCrewsPayNoLongerCostsAWholePennyACall()
        {
            // sixty-five calls in a season, half a penny owed at each (the critic's sum): 32.5 d in all
            var frankish = rules.ZoneOf("rhine_delta");
            var p = new Purse();
            foreach (var kv in rules.startCoins)
                p.Add(kv.Key, kv.Value);
            double before = p.Worth(frankish, rules);
            for (int i = 0; i < 65; i++)
                Assert.IsTrue(p.Pay(0.5, frankish, rules, out _));
            double cost = before - p.Worth(frankish, rules);
            Assert.GreaterOrEqual(cost, 32.5 - 1e-6, "never less than was owed");
            Assert.Less(cost, 37.0, "and not the 56 d it cost when every call took a whole coin (purse_sim.py: 35.35 d)");
        }

        [Test]
        public void OneLotIsNeverDearerThanTheSameUnitsOneByOne()
        {
            // two merchants alike, two markets alike: one buys four at once, the other one at a time
            double Spend(bool byTheLot)
            {
                var market = MarketOf(Dorestad, 0);
                var g = market.Goods.First(x => x.OnOffer && x.good.value < 10f && x.stock >= 6f);
                var purse = new Purse();
                purse.Add("denier", 200);
                var hold = new Hold { capacity = 1e6f };
                double before = purse.Worth(market.zone, rules);
                if (byTheLot)
                    Assert.AreEqual(4, Deal.Buy(market, purse, hold, g.good.id, 4, 6, 0, "ship").count);
                else
                    for (int i = 0; i < 4; i++)
                        Assert.AreEqual(1, Deal.Buy(market, purse, hold, g.good.id, 1, 6, 0, "ship").count);
                return before - purse.Worth(market.zone, rules);
            }
            double lot = Spend(true), singles = Spend(false);
            Assert.LessOrEqual(lot, singles + 0.06, "one settlement is not the dearer (to a tenth of a gram on the scales)");
            Assert.Less(singles - lot, 0.5, "and a unit at a time no longer costs a penny a click more (it was up to 4 d on four units)");
        }
    }
}
