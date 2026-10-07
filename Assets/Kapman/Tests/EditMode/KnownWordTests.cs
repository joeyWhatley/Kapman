using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The word on the item (Design/research_trade_seasons_and_hints.md 6): the one line of what the merchant knows of a
    /// good at a port. Each of the eight rules fires in its turn and the earlier one wins; the age is said in words by
    /// its bands; with nothing known it falls through to "no word"; a told price never shows a number; the words are all
    /// in wording_793.json; and trade_year_793.json loads and covers every region and month.
    /// The lines are checked against the built-in words (the table is not loaded for those tests), so rewriting
    /// wording_793.json cannot fail them. Game day 0 is 21 June 793; day 40 is 31 July.
    /// </summary>
    public class KnownWordTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const double Today = 40.0;

        static TextAsset Asset(string file) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + file);

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            TradeRules.Load(Asset("trade_rules.json"));
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
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Wording.Load(Asset("wording_793.json"));
        }

        static PortDef Dorestad => GameData.Port("dorestad");
        static PortDef London => GameData.Port("lundenwic");
        static PortDef Quentovic => GameData.Port("quentovic");
        static GoodDef Wine => GameData.Good("rhenish_wine");
        static GoodDef Whetstones => GameData.Good("whetstones");
        static GoodDef Swords => GameData.Good("swords");

        static WordAsk Ask(double today = Today, PortDef here = null, IReadOnlyList<SimShip> ships = null) =>
            new WordAsk { today = today, here = here, ships = ships };

        static Market MarketOf(PortDef port, double day) => new Market(port, TradeRules.Current, 793, day);

        /// <summary>The merchant sees a market's prices on a day.</summary>
        static void See(PortDef port, double day) => Trade.Ledger.Note(MarketOf(port, day), day, 7);

        /// <summary>The merchant is told a market's prices as they were on a day.</summary>
        static void Tell(PortDef port, double day, How how, int hops, string teller = null, string tellerOf = null) =>
            Trade.Ledger.Hear(PriceSnapshot.Of(MarketOf(port, day), 7, day), "a label that is never shown", how, hops, day, teller, tellerOf);

        static SimShip Ship(PortDef from, PortDef to, double left, GoodDef cargo, int count = 4)
        {
            var hold = new Hold();
            if (cargo != null)
                hold.Add(cargo, count, 0.0, left);
            return new SimShip { id = from.id + ".1", at = from.index, dest = to.index, state = ShipState.Sailing, departedDay = left, hold = hold };
        }

        static bool HasDigit(string text) => Regex.IsMatch(text ?? "", @"\d");

        // ------------------------------------------------------------------ the eight rules, each in its turn

        [Test]
        public void Rule1HardNewsComesBeforeAPriceSeen()
        {
            See(London, Today - 2.0);
            Assert.AreEqual(2, Known.Word(Wine, London, Ask()).rule, "with no news: the price seen");
            Known.HearNews(new NewsItem { id = "raid.test.1", kind = "raid", text = "Raiders struck Lundenwic", port = London.index, day = Today - 3.0 },
                           Today - 1.0, "a skipper from the Thames");
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual(1, w.rule);
            Assert.AreEqual("news.dear", w.key, "the world's raiders carry the stock off: what is left is dear");
            Assert.AreEqual(WordMark.Heard, w.mark);
            Assert.AreEqual(1, w.tilt);
            Assert.AreEqual("lundenwic", w.port);
            Assert.AreEqual("Raiders struck Lundenwic. Rhenish wine is dear at Lundenwic now.", w.Text);
            Assert.AreNotEqual(1, Known.Word(Wine, Quentovic, Ask()).rule, "the news is of London, not of Quentovic");
            Assert.AreNotEqual(1, Known.Word(Wine, London, Ask(Today + 57.5)).rule, "sixty days on it is no longer news");
            Assert.AreEqual(1, Known.Word(Wine, London, Ask(Today + 56.0)).rule, "at fifty-nine days it still is");
        }

        [Test]
        public void Rule1ASackedMarketIsShutThenDear()
        {
            var e = Chronicle.Entry("793-06-08-lindisfarena-raid");
            Assert.IsNotNull(e, "the chronicle has the sack of Lindisfarne");
            var port = GameData.Port("lindisfarena");
            var wheat = GameData.Good("wheat");
            Known.HearNews(new NewsItem { id = "chronicle.test", kind = e.kind, refId = e.id, text = e.title, port = port.index, day = Today - 4.0 },
                           Today, "talk on a waterfront", How.Talk, 2);
            var shut = Known.Word(wheat, port, Ask());
            Assert.AreEqual(1, shut.rule);
            Assert.AreEqual("news.shut", shut.key, "the shock shuts the market for ten days");
            Assert.AreEqual(-1, shut.tilt);
            StringAssert.EndsWith("Nobody there is buying.", shut.Text);
            StringAssert.StartsWith(e.title, shut.Text);
            var dear = Known.Word(wheat, port, Ask(Today + 10.0));
            Assert.AreEqual("news.dear", dear.key, "after it opens again, what is left is dear");
            Assert.AreEqual(1, dear.tilt);
        }

        [Test]
        public void Rule1TheNewsMustTouchTheGood()
        {
            var e = Chronicle.Entry("793-great-famine");
            Assert.IsNotNull(e, "the chronicle has the great famine");
            Known.HearNews(new NewsItem { id = "famine.test", kind = e.kind, refId = e.id, text = e.title, port = Dorestad.index, day = Today - 10.0 },
                           Today, "talk on a waterfront");
            var wheat = Known.Word(GameData.Good("wheat"), Dorestad, Ask());
            Assert.AreEqual(1, wheat.rule, "a famine touches food");
            Assert.AreEqual("news.dear", wheat.key);
            Assert.AreEqual(1, wheat.tilt);
            Assert.AreEqual(1, Known.Word(GameData.Good("wheat"), Quentovic, Ask()).rule, "in every port of its regions");
            Assert.AreNotEqual(1, Known.Word(Swords, Dorestad, Ask()).rule, "but not swords");
            Assert.AreNotEqual(1, Known.Word(GameData.Good("wheat"), London, Ask()).rule, "nor a port outside its regions");
        }

        [Test]
        public void Rule2APriceSeenLatelyIsSaidWithItsNumber()
        {
            See(London, Today - 2.0);
            Tell(London, Today - 1.0, How.Told, 0, "skipper", "dorestad");    // (told since, and newer: the price seen still wins)
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual(2, w.rule);
            Assert.AreEqual("seen", w.key);
            Assert.AreEqual(WordMark.Seen, w.mark);
            Assert.AreEqual("age.two_days", w.ageKey);
            float pays = Trade.Ledger.History("lundenwic")[0].prices["rhenish_wine"].pays;
            string price = TradeRules.Current.ZoneOf(London.region).Show(pays);
            Assert.AreEqual(price, w.Slot("price"), "in the port's own reckoning");
            Assert.IsTrue(HasDigit(price));
            Assert.AreEqual($"You saw Rhenish wine pay {price} at Lundenwic, two days back.", w.Text);
            Known.PaysKey(pays, Wine, out int tilt);
            Assert.AreEqual(tilt, w.tilt);
            Assert.AreEqual(1, w.tilt, "London wants wine: it pays well there");
        }

        [Test]
        public void Rule2LastsSevenDays()
        {
            See(London, Today - 6.5);
            Assert.AreEqual(2, Known.Word(Wine, London, Ask()).rule);
            var later = Known.Word(Wine, London, Ask(Today + 1.0));
            Assert.AreEqual(4, later.rule, "seen a week ago and more: no longer a number, but still the merchant's own sight");
            Assert.AreEqual("saw", later.key);
            Assert.AreEqual(WordMark.Seen, later.mark);
            Assert.IsFalse(HasDigit(later.Text), later.Text);
            StringAssert.StartsWith("You saw Lundenwic paying ", later.Text);
        }

        [Test]
        public void Rule3APriceToldLatelyIsSaidByAPerson()
        {
            Tell(London, Today - 5.0, How.Told, 0, "skipper", "dorestad");
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual(3, w.rule);
            Assert.AreEqual("told", w.key);
            Assert.AreEqual(WordMark.Heard, w.mark);
            Assert.AreEqual("age.few_days", w.ageKey);
            // (with no wording table loaded every port's people read as Frankish; the table makes Dorestad's Frisian)
            Assert.AreEqual("a Frankish skipper", w.Slot("person"), "a person of his people, not the note's label");
            Assert.AreEqual("well", w.Slot("pays"));
            Assert.AreEqual(1, w.tilt);
            Assert.AreEqual("A Frankish skipper said Lundenwic was paying well for Rhenish wine. A few days back.", w.Text);
            StringAssert.DoesNotContain("label", w.Text);
            Wording.Load(Asset("wording_793.json"));
            Assert.AreEqual("frisian", Wording.CultureOf(Dorestad));
            string skipper = Wording.Fill(Wording.WordText(Dorestad, "person.skipper"), ("people", Wording.WordText(Dorestad, "people.frisian")), ("place", Dorestad.name));
            Assert.AreEqual(skipper, Known.Word(Wine, London, Ask()).Slot("person"), "in the table's words: a skipper of Dorestad's people");
        }

        [Test]
        public void Rule3TakesTalkAtSecondHandAndLastsAFortnight()
        {
            Tell(London, Today - 13.0, How.Talk, 1, "quay", "dorestad");
            var w = Known.Word(Wine, London, Ask());
            Assert.AreEqual(3, w.rule, "one mouth between: still someone said it");
            Assert.AreEqual("age.fortnight", w.ageKey);
            var quay = new[] { "the toll man at Dorestat", "a boatman at Dorestat", "a porter at Dorestat", "an ale-wife at Dorestat" };
            CollectionAssert.Contains(quay, w.Slot("person"), "someone on the waterfront it was heard at");
            Assert.AreEqual(w.Slot("person"), Known.Word(GameData.Good("wheat"), London, Ask()).Slot("person"), "the same one for the same note");
            Assert.AreEqual(w.Text, Known.Word(Wine, London, Ask()).Text, "and the same line every time");
            var later = Known.Word(Wine, London, Ask(Today + 1.0));
            Assert.AreEqual(4, later.rule, "at fourteen days it is talk");
            Assert.AreEqual("talk.at", later.key);
        }

        [Test]
        public void Rule4TalkAndRumourAreSaidAsSuch()
        {
            // through more mouths: they say
            Tell(London, Today - 3.0, How.Talk, 3, "quay", "quentovic");
            var talk = Known.Word(Wine, London, Ask());
            Assert.AreEqual(4, talk.rule);
            Assert.AreEqual("talk.at", talk.key);
            Assert.AreEqual(WordMark.Heard, talk.mark);
            Assert.AreEqual("They say at Quentovic that Lundenwic pays well for Rhenish wine. A few days back.", talk.Text);
            // a rumour
            Tell(Quentovic, Today - 3.0, How.Rumour, 3);
            var rumour = Known.Word(Wine, Quentovic, Ask());
            Assert.AreEqual(4, rumour.rule);
            Assert.AreEqual("rumour", rumour.key);
            StringAssert.StartsWith("There is a rumour that Quentovic pays ", rumour.Text);
            // told, but long ago: old talk, with no place to name
            Tell(Dorestad, Today - 70.0, How.Told, 0);
            var old = Known.Word(Wine, Dorestad, Ask());
            Assert.AreEqual(4, old.rule);
            Assert.AreEqual("talk", old.key);
            Assert.AreEqual("age.old_talk", old.ageKey, "two or three months old is old talk");
            StringAssert.EndsWith("Old talk.", old.Text);
            Assert.AreEqual(-1, old.tilt, "Dorestad has wine of its own: it pays poorly for more");
        }

        [Test]
        public void Rule4WordOfAFair()
        {
            var paris = GameData.Port("parisius");
            var wheat = GameData.Good("wheat");
            Known.HearNews(new NewsItem { id = "fair.test", kind = "fair", text = "The fair of Saint-Denis has begun (28 days).", port = paris.index, day = Today - 2.0 },
                           Today, "ships from Parisius");
            var w = Known.Word(wheat, paris, Ask());
            Assert.AreEqual(4, w.rule);
            Assert.AreEqual("talk.fair", w.key);
            Assert.AreEqual(-1, w.tilt);
            Assert.IsFalse(HasDigit(w.Text), w.Text);
        }

        [Test]
        public void Rule5TheSeasonWhenNothingIsNoted()
        {
            var frisia = GameData.Port("medemelacha");
            // July in Frisia (trade_year_793.json): the old wine is dear, and wanted; the new wool comes to market
            var wanted = Known.Word(Wine, frisia, Ask());
            Assert.AreEqual(5, wanted.rule);
            Assert.AreEqual("season.wanted", wanted.key);
            Assert.AreEqual(WordMark.Season, wanted.mark);
            Assert.AreEqual(1, wanted.tilt);
            Assert.AreEqual("Medemelacha wants Rhenish wine in July.", wanted.Text);
            Assert.AreEqual(TradeYear.Of("frisia", 7).line, wanted.Slot("line"), "the month's plain line is there for the words to use");
            var comes = Known.Word(GameData.Good("raw_wool"), frisia, Ask());
            Assert.AreEqual("season.comes", comes.key);
            Assert.AreEqual(-1, comes.tilt);
            Assert.AreEqual("Wool comes to market at Medemelacha in July: plenty, and cheap.", comes.Text);
            // where the trading year says nothing of the good, its own dear and cheap months (the goods table)
            var dear = Known.Word(GameData.Good("salt_pork"), Dorestad, Ask());
            Assert.AreEqual(5, dear.rule);
            Assert.AreEqual("season.dear", dear.key);
            Assert.AreEqual(1, dear.tilt);
            var cheap = Known.Word(GameData.Good("cheese"), Dorestad, Ask());
            Assert.AreEqual("season.cheap", cheap.key);
            Assert.AreEqual(-1, cheap.tilt);
            // and anything noted comes before the season
            Tell(frisia, Today - 30.0, How.Talk, 2);
            Assert.AreEqual(4, Known.Word(Wine, frisia, Ask()).rule);
        }

        [Test]
        public void Rule6WhoSailedAhead()
        {
            var ships = new List<SimShip>
            {
                Ship(Dorestad, London, Today - 1.0, Whetstones),
                Ship(Dorestad, London, Today - 2.5, Whetstones),
                Ship(Dorestad, London, Today - 4.0, Whetstones),             // left too long ago
                Ship(Dorestad, Quentovic, Today - 1.0, Whetstones),          // bound elsewhere
                Ship(Dorestad, London, Today - 1.0, Wine),                   // another cargo
                Ship(Quentovic, London, Today - 1.0, Whetstones),            // left from another port
            };
            var raider = Ship(Dorestad, London, Today - 1.0, Whetstones);
            raider.raider = true;
            ships.Add(raider);
            var w = Known.Word(Whetstones, London, Ask(Today, Dorestad, ships));
            Assert.AreEqual(6, w.rule);
            Assert.AreEqual("ahead", w.key);
            Assert.AreEqual(WordMark.Seen, w.mark, "she watched them go");
            Assert.AreEqual(-1, w.tilt, "they will have filled the market before her");
            Assert.AreEqual("two ships", w.Slot("ships"));
            Assert.AreEqual("Two ships left for Lundenwic before you with whetstones aboard.", w.Text);
            Assert.AreEqual(8, Known.Word(Whetstones, London, Ask(Today, null, ships)).rule, "at sea she saw nobody leave");
            Assert.AreEqual(8, Known.Word(Whetstones, London, Ask(Today, Dorestad)).rule, "and with no ships there is nothing to see");
            // the season comes before the ships
            Assert.AreEqual(5, Known.Word(Wine, London, Ask(Today, Dorestad, ships)).rule);
        }

        [Test]
        public void Rule7TheLaw()
        {
            var w = Known.Word(Swords, Quentovic, Ask());
            Assert.AreEqual(7, w.rule);
            Assert.AreEqual("law.no_export", w.key);
            Assert.AreEqual(WordMark.Law, w.mark);
            Assert.AreEqual(0, w.tilt);
            Assert.AreEqual("Swords may not be carried out of the Frankish realm. They search ships at Quentovic.", w.Text);
            StringAssert.Contains("Herstal", w.Slot("law"), "the law in full is there for the words to use");
            var later = Known.Word(Swords, Quentovic, new WordAsk { today = Today, month = 7, year = 870 });
            Assert.AreEqual("law.death", later.key, "from 864 it is death");
            Assert.AreEqual(8, Known.Word(Swords, GameData.Port("birka"), Ask()).rule, "no such law among the Svear");
            // ships that sailed ahead come before the law
            var ships = new List<SimShip> { Ship(Dorestad, Quentovic, Today - 1.0, Swords) };
            var ahead = Known.Word(Swords, Quentovic, Ask(Today, Dorestad, ships));
            Assert.AreEqual(6, ahead.rule);
            Assert.AreEqual("A ship left for Quentovic before you with swords aboard.", ahead.Text);
        }

        [Test]
        public void Rule8NothingKnownFallsThrough()
        {
            var w = Known.Word(Whetstones, London, Ask());
            Assert.AreEqual(8, w.rule);
            Assert.AreEqual("none", w.key);
            Assert.AreEqual(WordMark.None, w.mark);
            Assert.AreEqual(0, w.tilt);
            Assert.AreEqual("", w.ageKey);
            Assert.AreEqual("No word on whetstones at Lundenwic.", w.Text);
            // a note older than ninety days is no longer knowledge: only when it was is said
            Tell(London, -20.0, How.Told, 0);                                // (1 June: spring)
            var since = Known.Word(Whetstones, London, Ask(80.0));           // (9 September: summer, a hundred days on)
            Assert.AreEqual(8, since.rule);
            Assert.AreEqual("none.since", since.key);
            Assert.AreEqual("age.since_spring", since.ageKey);
            Assert.AreEqual("No word on whetstones at Lundenwic since spring.", since.Text);
            var old = Known.Word(Whetstones, London, Ask(300.0));
            Assert.AreEqual("none.old", old.key);
            Assert.AreEqual("Only old talk of whetstones at Lundenwic.", old.Text);
            // nothing to ask about
            Assert.AreEqual(8, Known.Word(null, London, Ask()).rule);
            Assert.AreEqual(8, Known.Word(Whetstones, null, Ask()).rule);
            Assert.AreEqual(8, Known.Word("no_such_good", "no_such_port").rule, "the live game's form, with no game running");
        }

        // ------------------------------------------------------------------ the age, in words

        [Test]
        public void TheAgeBands()
        {
            const double now = 100.5;                                        // (29 September 793: autumn)
            Assert.AreEqual("age.this_morning", Known.AgeKey(100.1, now));
            Assert.AreEqual("age.yesterday", Known.AgeKey(99.9, now));
            Assert.AreEqual("age.yesterday", Known.AgeKey(99.0, now));
            Assert.AreEqual("age.two_days", Known.AgeKey(98.7, now));
            Assert.AreEqual("age.few_days", Known.AgeKey(97.2, now));
            Assert.AreEqual("age.few_days", Known.AgeKey(94.0, now));
            Assert.AreEqual("age.last_week", Known.AgeKey(93.9, now));
            Assert.AreEqual("age.last_week", Known.AgeKey(90.0, now));
            Assert.AreEqual("age.fortnight", Known.AgeKey(89.5, now));
            Assert.AreEqual("age.fortnight", Known.AgeKey(80.0, now));
            Assert.AreEqual("age.last_month", Known.AgeKey(79.0, now));
            Assert.AreEqual("age.last_month", Known.AgeKey(41.0, now));
            Assert.AreEqual("age.since_summer", Known.AgeKey(40.0, now), "sixty days back, in another season");
            Assert.AreEqual("age.since_spring", Known.AgeKey(-10.0, now));
            Assert.AreEqual("age.old_talk", Known.AgeKey(now - 270.0, now));
            Assert.AreEqual("age.old_talk", Known.AgeKey(double.NaN, now));
            Assert.AreEqual("age.this_morning", Known.AgeKey(now + 3.0, now), "a day to come reads as today");
            // within one season: sixty days is still last month, ninety is old talk
            Assert.AreEqual("age.last_month", Known.AgeKey(25.0, 90.0));
            Assert.AreEqual("age.old_talk", Known.AgeKey(1.0, 93.0));
            // the other seasons
            Assert.AreEqual("age.since_autumn", Known.AgeKey(150.0, 250.0));
            Assert.AreEqual("age.since_winter", Known.AgeKey(230.0, 320.0));
        }

        [Test]
        public void TheAgeIsSaidInWordsNeverInDays()
        {
            var said = new Dictionary<string, string>
            {
                ["age.this_morning"] = "this morning", ["age.yesterday"] = "yesterday", ["age.two_days"] = "two days back",
                ["age.few_days"] = "a few days back", ["age.last_week"] = "last week", ["age.fortnight"] = "a fortnight ago",
                ["age.last_month"] = "last month", ["age.since_spring"] = "since spring", ["age.old_talk"] = "old talk",
            };
            foreach (var kv in said)
                Assert.AreEqual(kv.Value, Wording.WordText(null, kv.Key), kv.Key);
            foreach (var key in Wording.WordKeys.Where(k => k.StartsWith("age.")))
                Assert.IsFalse(HasDigit(Wording.WordText(null, key)), key);
            // through the bands of one note as it ages: the line never shows a day count
            Tell(London, 0.0, How.Told, 0, "skipper", "dorestad");
            var seen = new HashSet<string>();
            for (double day = 0.5; day < 420.0; day += 1.0)
            {
                var w = Known.Word(Whetstones, London, Ask(day));
                Assert.IsFalse(HasDigit(w.Text), $"day {day}: {w.Text}");
                seen.Add(w.ageKey);
            }
            CollectionAssert.IsSupersetOf(seen.ToList(), new[]
            {
                "age.this_morning", "age.yesterday", "age.two_days", "age.few_days", "age.last_week", "age.fortnight", "age.last_month", "age.old_talk",
                "age.since_summer",
            });
        }

        // ------------------------------------------------------------------ a told price never shows a number

        [Test]
        public void AToldPriceNeverRendersANumber()
        {
            Tell(London, Today - 1.0, How.Told, 0, "skipper", "dorestad");          // rule 3: he said
            Tell(Quentovic, Today - 9.0, How.Talk, 1, "quay", "dorestad");          // rule 3: someone on the quay said
            Tell(Dorestad, Today - 5.0, How.Talk, 3, "quay", "quentovic");          // rule 4: they say
            Tell(GameData.Port("birka"), Today - 20.0, How.Rumour, 4);              // rule 4: a rumour
            Tell(GameData.Port("ripa"), Today - 40.0, How.Written, 0);              // rule 4: a letter, grown old
            int lines = 0;
            foreach (var id in new[] { "lundenwic", "quentovic", "dorestad", "birka", "ripa" })
            {
                var port = GameData.Port(id);
                var note = Trade.Ledger.Latest(id);
                Assert.IsNotNull(note, id);
                foreach (var good in GameData.Goods)
                {
                    if (!note.prices.ContainsKey(good.id))
                        continue;
                    var w = Known.Word(good, port, Ask());
                    Assert.IsTrue(w.rule == 3 || w.rule == 4, $"{good.id} at {id}: rule {w.rule}");
                    Assert.AreEqual(WordMark.Heard, w.mark);
                    Assert.AreEqual("", w.Slot("price"), "no price is handed to the words");
                    CollectionAssert.Contains(new[] { "well", "the usual", "poorly" }, w.Slot("pays"));
                    Assert.IsFalse(HasDigit(w.Text), $"{good.id} at {id}: {w.Text}");
                    foreach (var (name, value) in w.slots)
                        Assert.IsFalse(HasDigit(value), $"{good.id} at {id}: the slot {name} is '{value}'");
                    lines++;
                }
            }
            Assert.Greater(lines, 400, "every good of five markets");
            // the three words, by what the market pays against the good's table value
            Assert.AreEqual("pays.well", Known.PaysKey(Wine.value * 1.2f, Wine, out int up));
            Assert.AreEqual("pays.fair", Known.PaysKey(Wine.value, Wine, out int level));
            Assert.AreEqual("pays.poorly", Known.PaysKey(Wine.value * 0.8f, Wine, out int down));
            Assert.AreEqual((1, 0, -1), (up, level, down));
        }

        [Test]
        public void TheWordingTableHasEveryLineAndKeepsToldPricesInWords()
        {
            Wording.Load(Asset("wording_793.json"));
            foreach (var key in Wording.WordKeys)
            {
                Assert.IsTrue(Wording.HasWord(key), $"wording_793.json has no word.{key}");
                Assert.IsNotEmpty(Wording.WordText(null, key), key);
            }
            // whatever the user writes there: only the line for a price seen may ask for the number
            foreach (var key in Wording.WordKeys.Where(k => k != "seen"))
            {
                string line = Wording.WordText(null, key);
                StringAssert.DoesNotContain("{price}", line, $"word.{key}: only word.seen may show a number");
                Assert.IsFalse(HasDigit(line), $"word.{key}: '{line}' has a number in it");
            }
            StringAssert.Contains("{price}", Wording.WordText(null, "seen"));
            // a people's own words win over the table's, and a key nothing has falls back to its parent
            var ripa = GameData.Port("ripa");
            StringAssert.DoesNotContain("toll man", Wording.WordText(ripa, "person.quay", 0f), "no toll is taken where silver goes by weight");
            StringAssert.Contains("toll man", Wording.WordText(Dorestad, "person.quay", 0f));
            Assert.AreEqual(Wording.WordText(null, "law"), Wording.WordText(null, "law.some_new_rule"));
            Assert.AreEqual("", Wording.WordText(null, "no_such_key"));
            // a slot named with a large first letter is filled with a large first letter
            Assert.AreEqual("A Frisian skipper said so; so said a Frisian skipper.",
                            Wording.Fill("{Person} said so; so said {person}.", ("person", "a Frisian skipper")));
        }

        // ------------------------------------------------------------------ the note on the item, in the market and the hold

        [Test]
        public void TheNoteOnAnItemIsTheFreshestWordFromElsewhere()
        {
            Assert.AreEqual("none.anywhere", Known.WordOn(Whetstones, Ask()).key, "nothing known anywhere, no season, no law");
            Assert.AreEqual("season.wanted", Known.WordOn(Wine, Ask(Today, London)).key, "nothing noted: the season here");
            Assert.AreEqual("lundenwic", Known.WordOn(Wine, Ask(Today, London)).port);
            Tell(London, Today - 5.0, How.Told, 0, "skipper", "dorestad");
            Tell(Quentovic, Today - 1.0, How.Told, 0, "skipper", "dorestad");
            Assert.AreEqual("quentovic", Known.WordOn(Wine, Ask(Today, Dorestad)).port, "of two things told, the younger word");
            Assert.AreEqual("lundenwic", Known.WordOn(Wine, Ask(Today, Quentovic)).port, "lying at Quentovic, the word of London");
            See(Dorestad, Today);
            var w = Known.WordOn(Wine, Ask(Today, Dorestad));
            Assert.AreEqual(3, w.rule, "what she sees here is in front of her: the note is of elsewhere");
            Assert.AreEqual("quentovic", w.port);
            Assert.AreEqual(2, Known.WordOn(Wine, Ask(Today, London)).rule, "from London, what she saw at Dorestad this morning");
            Assert.AreEqual("dorestad", Known.WordOn(Wine, Ask(Today, London)).port);
            // hard news anywhere comes first
            Known.HearNews(new NewsItem { id = "raid.test.2", kind = "raid", text = "Raiders struck Lundenwic", port = London.index, day = Today - 2.0 }, Today, "a skipper");
            Assert.AreEqual(1, Known.WordOn(Wine, Ask(Today, Dorestad)).rule);
            Assert.AreEqual("lundenwic", Known.WordOn(Wine, Ask(Today, Dorestad)).port);
            // ships that left here with it, when nothing else is known of it
            var ships = new List<SimShip> { Ship(Dorestad, London, Today - 1.0, Swords), Ship(Dorestad, Quentovic, Today - 1.0, Swords), Ship(Dorestad, Quentovic, Today - 2.0, Swords) };
            Known.Clear();
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            var ahead = Known.WordOn(Swords, Ask(Today, Dorestad, ships));
            Assert.AreEqual(6, ahead.rule);
            Assert.AreEqual("quentovic", ahead.port, "where most of them went");
            Assert.AreEqual(7, Known.WordOn(Swords, Ask(Today, Dorestad)).rule, "else the law here");
        }

        [Test]
        public void WhoToldItIsKeptWithTheNoteAndSaved()
        {
            Wording.Load(Asset("wording_793.json"));                         // (a port's people come from the table)
            var ripa = GameData.Port("ripa");
            Tell(London, Today - 1.0, How.Told, 0, "skipper", "ripa");
            Tell(Quentovic, Today - 1.0, How.Told, 0);                       // (an older save, or a teller nobody noted)
            Assert.AreEqual("norse", Wording.CultureOf(ripa));
            Assert.AreEqual(Wording.Fill(Wording.WordText(ripa, "person.skipper"), ("people", Wording.WordText(ripa, "people.norse")), ("place", ripa.name)),
                            Known.Person(Trade.Ledger.Latest("lundenwic")), "a skipper of Ribe's people");
            Assert.AreEqual(Wording.WordText(null, "person.witness"), Known.Person(Trade.Ledger.Latest("quentovic")));
            Assert.IsNotEmpty(Known.Person(Trade.Ledger.Latest("quentovic")));
            var saved = Json.Parse(Json.Write(Trade.Ledger.Capture())) as List<object>;
            var back = new Ledger();
            back.Restore(saved);
            Assert.AreEqual("skipper", back.Latest("lundenwic").teller);
            Assert.AreEqual("ripa", back.Latest("lundenwic").tellerOf);
            Assert.IsNull(back.Latest("quentovic").teller);
            Assert.AreEqual(Json.Write(Trade.Ledger.Capture()), Json.Write(back.Capture()), "the same after a save and a load");
        }

        // ------------------------------------------------------------------ the trading year

        [Test]
        public void TheTradingYearLoadsAndCoversEveryRegionAndMonth()
        {
            Assert.IsEmpty(TradeYear.Faults, string.Join("\n", TradeYear.Faults));
            Assert.Greater(GameData.Regions.Count, 30);
            CollectionAssert.AreEquivalent(GameData.Regions.Keys, TradeYear.Regions, "the trade regions of goods_793.json, no more and no fewer");
            int seasonal = 0;
            foreach (var region in GameData.Regions.Keys)
            {
                Assert.IsNotEmpty(TradeYear.Name(region), region);
                for (int m = 1; m <= 12; m++)
                {
                    var month = TradeYear.Of(region, m);
                    Assert.IsNotNull(month, $"{region}, month {m}");
                    Assert.IsNotEmpty(month.line, $"{region}, month {m}: one plain line");
                    foreach (var id in month.comes)
                    {
                        var g = GameData.Good(id);
                        Assert.IsNotNull(g, $"{region}, month {m}: comes: no good '{id}'");
                        Assert.Greater(g.MadeIn(region), 0f, $"{region}, month {m}: {id} comes to market, but the goods table does not make it there");
                        Assert.Less(System.Array.IndexOf(g.dear, m), 0, $"{region}: {id} comes in month {m}, one of its dear months");
                    }
                    foreach (var id in month.wanted)
                    {
                        var g = GameData.Good(id);
                        Assert.IsNotNull(g, $"{region}, month {m}: wanted: no good '{id}'");
                        Assert.Greater(g.WantedIn(region), 0f, $"{region}, month {m}: {id} is wanted, but the goods table does not want it there");
                        Assert.Less(System.Array.IndexOf(g.cheap, m), 0, $"{region}: {id} is wanted in month {m}, one of its cheap months");
                        CollectionAssert.DoesNotContain(month.comes, id, $"{region}, month {m}: {id} both comes and is wanted");
                    }
                    seasonal += month.comes.Count + month.wanted.Count;
                }
            }
            Assert.Greater(seasonal, 500, "the year has its goods");
            // a few things the research is sure of
            CollectionAssert.Contains(TradeYear.Of("rhine_delta", 11).comes, "rhenish_wine", "the new wine comes down the Rhine at Martinmas");
            CollectionAssert.Contains(TradeYear.Of("england_south", 6).comes, "raw_wool", "the shearing");
            CollectionAssert.Contains(TradeYear.Of("frisia", 5).wanted, "wheat", "the hungry gap on the mounds");
            Assert.IsTrue(TradeYear.Of("neustria", 10).fairs.Any(f => f.Contains("Saint-Denis")), "the fair of Saint-Denis from 9 October");
            Assert.IsTrue(TradeYear.Of("ladoga", 1).Closed, "Ladoga is frozen in January");
            Assert.IsFalse(TradeYear.Of("rhine_delta", 1).Closed);
            Assert.IsTrue(TradeYear.Of("neustria", 4).feasts.Any(f => f.StartsWith("Easter")));
            Assert.IsNull(TradeYear.Of("no_such_region", 6));
            // every port of the game lies in a region the year covers
            foreach (var port in GameData.Ports)
                Assert.IsNotNull(TradeYear.Of(port.region, 6), $"{port.id}: its region {port.region} is not in the trading year");
        }
    }
}
