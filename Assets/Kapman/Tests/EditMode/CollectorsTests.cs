using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE LENDER SENDS HIS MEN (the user, 9 Oct 2026: "Sends his goons to collect", "A handful of times", "It'll be
    /// dynamic you know", "Maybe you can pay them off in installments, maybe you can give them cargo maybe they kill a
    /// crew member ect", "It'll be comedic but also shockingly brutal at times", "Just clueless brutes", "Maybe it you
    /// don't pay up for the 3rd time they kill Ravn?"). Held here, without the screen:
    ///   - the table is whole and holds only names the code reads; four sendings, in their order;
    ///   - a sending is due on its day, and not while a boat of them is out;
    ///   - what they do when they are given nothing depends: they talk, take, beat, kill; never the last man; and the
    ///     third time she does not pay it is Ravn;
    ///   - a way round them is harder each time it has fooled them, and for each later sending;
    ///   - in the live game: a pound's worth of silver and cargo taken by force and counted low on the stick; cargo
    ///     given at their price; a pound paid cools the lender; Ravn killed, and the flag that says so;
    ///   - the dead in the cards: a card that leans on Ravn does not come when he is dead, another leaves his lines
    ///     out, and a text written for when he is gone is the one read.
    /// What it is on the chart (their boat following her from port to port, the catch at the quay and at sea) is held
    /// by GoonsTests (play).
    /// </summary>
    public class CollectorsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const string Ravn = "cast.crew.ravn";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        EncounterState flags;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            Markets.Clear();
            Collectors.Reset();
            flags = new EncounterState();
            ContentFlags.Use(flags, () => 0.0);
        }

        [TearDown]
        public void Done()
        {
            Collectors.Reset();
            ContentFlags.Use(null);
            Markets.Clear();
        }

        [Test]
        public void TheTableIsWholeAndHoldsOnlyNamesTheCodeReads()
        {
            var text = File(Collectors.File);
            Assert.IsNotNull(text, Collectors.File + " is not in Game/Data");
            var root = Json.Parse(text.text) as Dictionary<string, object>;
            Assert.IsNotNull(root);
            var fields = typeof(CollectorRules).GetFields().Where(f => !f.IsStatic && (f.FieldType == typeof(double) || f.FieldType == typeof(bool) || f.FieldType == typeof(string)))
                                               .Select(f => f.Name).ToList();
            foreach (string key in root.Keys)
                if (key != "about" && key != "sendings" && key != "tricks")
                    CollectionAssert.Contains(fields, key, "collectors_793.json names a number the code does not read: " + key);
            foreach (string field in fields)
                Assert.IsTrue(root.ContainsKey(field), "collectors_793.json leaves out " + field);
            var r = Collectors.Rules;
            Assert.IsTrue(r.on);
            Assert.AreEqual(4, r.sendings.Count, "a handful of times: four sendings");
            for (int i = 1; i < r.sendings.Count; i++)
                Assert.Greater(r.sendings[i].day, r.sendings[i - 1].day + r.sendings[i - 1].daysOut, "a sending falls due while the one before may still be out");
            Assert.Less(r.sendings[r.sendings.Count - 1].day + r.sendings[r.sendings.Count - 1].daysOut, 516.0, "the last sending is still out on the day itself");
            Assert.GreaterOrEqual(r.sendings[0].day, 14.0, "they come in the first fortnight");
            Assert.IsNotNull(GameData.Port(r.home), "their home is no port");
            Assert.AreEqual(3.0, r.ravnAtRefusal, "the third time she does not pay (the user's own words)");
            foreach (string trick in new[] { "hide", "lie", "drink", "plead", "run" })
            {
                Assert.IsTrue(r.tricks.ContainsKey(trick), "no stat carries the way round them '" + trick + "'");
                Assert.IsTrue(Self.IsStat(r.tricks[trick].stat), trick + " is carried by no stat: " + r.tricks[trick].stat);
            }
            // and their two cards are in the book, with a rule each for when nobody answers
            var book = EncounterData.Book();
            foreach (string id in new[] { Collectors.QuayCard, Collectors.SeaCard })
            {
                var card = book.Card(id);
                Assert.IsNotNull(card, id + " is not in the book");
                Assert.AreEqual(0.0, card.weight + card.rate, id + " can be drawn: it is only ever put in line when they have caught her");
                Assert.IsNotNull(card.Option("nothing"), id + " has no choice to give them nothing");
                foreach (var o in card.options)
                    if (o.Chance != null)
                        StringAssert.StartsWith("goons.ease_", o.Chance.by, id + " " + o.key + ": a way round them that her stat does not carry");
            }
        }

        [Test]
        public void ASendingIsDueOnItsDayAndNotWhileABoatIsOut()
        {
            var r = new CollectorRules();
            r.sendings.Add(new Sending { day = 21.0 });
            r.sendings.Add(new Sending { day = 130.0 });
            Assert.IsFalse(Collectors.Due(r, 20.9, 0, false));
            Assert.IsTrue(Collectors.Due(r, 21.0, 0, false));
            Assert.IsFalse(Collectors.Due(r, 60.0, 1, false), "the second is due before its day");
            Assert.IsFalse(Collectors.Due(r, 131.0, 1, true), "a second boat is sent while the first is out");
            Assert.IsTrue(Collectors.Due(r, 131.0, 1, false));
            Assert.IsFalse(Collectors.Due(r, 500.0, 2, false), "more sendings than the table has");
        }

        [Test]
        public void WhatTheyDoWhenGivenNothingDepends()
        {
            var r = new CollectorRules();
            // the lender easy: they only talk
            var bite = Collectors.Bite(r, 1.0, false, 500.0, 4, 0.5);
            Assert.IsTrue(bite.warned);
            Assert.IsFalse(bite.beat || bite.kill || bite.ravn);
            // his temper up: they take an instalment, and nobody is hurt while there is enough aboard
            bite = Collectors.Bite(r, 2.0, false, 500.0, 4, 0.5);
            Assert.IsFalse(bite.warned);
            Assert.AreEqual(240.0, bite.want, 0.01);
            Assert.IsFalse(bite.beat || bite.kill);
            // higher: they mean to take more
            Assert.Greater(Collectors.Bite(r, 4.0, false, 5000.0, 4, 0.5).want, 400.0);
            // little aboard: a man is beaten, and from four a man is killed
            bite = Collectors.Bite(r, 3.0, false, 10.0, 4, 0.5);
            Assert.IsTrue(bite.beat);
            Assert.IsFalse(bite.kill);
            bite = Collectors.Bite(r, 4.0, false, 10.0, 4, 0.5);
            Assert.IsTrue(bite.kill, "with the lender at four and nothing aboard nobody is killed");
            Assert.IsFalse(bite.beat);
            // enough aboard, and nobody is hurt even then: unless she has just made fools of them
            Assert.IsFalse(Collectors.Bite(r, 4.0, false, 5000.0, 4, 0.5).kill, "they kill with the hold full and no lie told");
            Assert.IsTrue(Collectors.Bite(r, 3.0, true, 5000.0, 4, 0.5).kill, "a way round them that failed, at three, costs no life");
            Assert.IsTrue(Collectors.Bite(r, 2.0, true, 5000.0, 4, 0.5).beat);
            // a step short of a killing, with nothing aboard and a lie just told: one time in three
            Assert.IsTrue(Collectors.Bite(r, 2.0, true, 0.0, 4, 0.1).kill);
            Assert.IsFalse(Collectors.Bite(r, 2.0, true, 0.0, 4, 0.9).kill);
            // never the last man aboard
            bite = Collectors.Bite(r, 6.0, true, 0.0, 1, 0.0);
            Assert.IsFalse(bite.kill || bite.ravn, "the last man aboard is killed");
            Assert.IsTrue(bite.beat);
            // THE THIRD TIME SHE DOES NOT PAY IT IS RAVN: whatever the lender's temper, while he lives and is not alone
            bite = Collectors.Bite(r, 1.0, false, 500.0, 4, 0.5, 3, true);
            Assert.IsTrue(bite.ravn, "the third time she does not pay, Ravn lives");
            Assert.IsFalse(bite.warned || bite.kill);
            Assert.Greater(bite.want, 0.0, "they kill Ravn and take nothing");
            Assert.IsFalse(Collectors.Bite(r, 3.0, false, 500.0, 4, 0.5, 2, true).ravn, "the second time is the third");
            Assert.IsFalse(Collectors.Bite(r, 3.0, false, 500.0, 4, 0.5, 3, false).ravn, "Ravn is killed twice");
            Assert.IsFalse(Collectors.Bite(r, 3.0, false, 500.0, 1, 0.5, 3, true).ravn, "Ravn is killed when he is the only man aboard");
            r.ravnAtRefusal = 0.0;
            Assert.IsFalse(Collectors.Bite(r, 3.0, false, 500.0, 4, 0.5, 9, true).ravn, "the rule is off in the table and Ravn is killed all the same");
        }

        [Test]
        public void AWayRoundThemIsHarderEachTimeAndForEachLaterSending()
        {
            var r = new CollectorRules();
            double first = Collectors.Ease(r, 3.0, 0, 1);
            Assert.AreEqual(3.0, first, 1e-9, "the first time, the first sending: her stat as it is");
            Assert.AreEqual(first - r.toldPenalty, Collectors.Ease(r, 3.0, 1, 1), 1e-9, "the same way a second time is no harder");
            Assert.AreEqual(first - r.laterPenalty * 2.0, Collectors.Ease(r, 3.0, 0, 3), 1e-9, "the third sending is no sharper than the first");
            Assert.Greater(Collectors.Ease(r, 5.0, 0, 1), first, "a better stat does not help");
            // as a card's odds: three quarters the first time, half the second
            var chance = new ChanceEffect { chance = 0.75, by = "x" };
            var vars = new EncounterVars();
            vars.nums["x"] = Collectors.Ease(r, 3.0, 0, 1);
            Assert.AreEqual(0.75, chance.Odds(vars), 1e-9);
            vars.nums["x"] = Collectors.Ease(r, 3.0, 1, 1);
            Assert.AreEqual(0.5, chance.Odds(vars), 1e-9);
        }

        [Test]
        public void GivenNothingTheyTakeAndTheThirdTimeTheyKillRavn()
        {
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            var rules = TradeRules.Current;
            Trade.Purse.Add("denier", 300.0);
            var wine = GameData.Good("rhenish_wine");
            Trade.Hold.Add(wine, 4, 0.0, 0.0);
            Voyage.SetMen(4);
            flags.SetFlag(Collectors.OpeningFlag, 1.0, 0.0, 0.0);
            var r = Collectors.Rules;
            var home = GameData.Port(r.home);
            Assert.IsNotNull(home);
            var (hx, hz) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            var quay = new WorldXZ(hx, hz);

            // ---- the first time: the lender easy, they only talk, and go home with nothing
            Collectors.PutOut(1, quay, r.temperStart, r.home);
            double owed = Trade.Debt, silver = Trade.Purse.Silver(rules);
            string said = Collectors.Law("goons_teeth");
            StringAssert.Contains("not sure what comes next", said, "the first time they do more than talk: " + said);
            Assert.AreEqual(owed, Trade.Debt, 0.001);
            Assert.AreEqual(silver, Trade.Purse.Silver(rules), 0.001);
            Assert.AreEqual(GoonState.Home, Collectors.State, "they did not go home");
            Assert.AreEqual(1, Collectors.Unpaid);
            Assert.AreEqual(r.temperStart + 1.0, Collectors.Temper, 1e-9, "home with nothing, and the lender is no angrier");
            Assert.AreEqual(0.0, Collectors.Var("goons.ravn_next"));

            // ---- the second: they turn out the purse, and count three quarters of it on the stick
            Collectors.PutOut(2, quay, Collectors.Temper, r.home);
            Assert.AreEqual(0.0, Collectors.Var("goons.ravn_next"), "the second time is said to be the third");
            said = Collectors.Law("goons_teeth");
            StringAssert.Contains("turn out the purse", said);
            Assert.AreEqual(silver - 240.0, Trade.Purse.Silver(rules), 6.0, "an instalment's worth of silver did not leave the purse: " + said);
            Assert.AreEqual(owed - 240.0 * (1.0 - r.troubleShare), Trade.Debt, 6.0, "what they took was not counted on the stick, less their trouble: " + said);
            Assert.AreEqual(4, Trade.Hold.Count(wine.id), "they took cargo with the purse enough");
            Assert.AreEqual(4, Voyage.Men);
            Assert.AreEqual(2, Collectors.Unpaid);
            Assert.IsFalse(Collectors.RavnGone);

            // ---- the third: Ravn
            Collectors.PutOut(3, quay, Collectors.Temper, r.home);
            Assert.AreEqual(1.0, Collectors.Var("goons.ravn_next"), "it is the third time, and the card does not know that nothing now means Ravn");
            double before = Trade.Debt;
            said = Collectors.Law("goons_teeth");
            StringAssert.Contains("Ravn", said, "the third time she gave them nothing, and Ravn lives: " + said);
            Assert.IsTrue(Collectors.RavnGone, "Ravn is dead and no flag says so");
            Assert.AreNotEqual(0.0, flags.Flag(EncounterState.GoneFlag(Ravn), 0.0));
            Assert.AreEqual(3, Voyage.Men, "Ravn is dead and as many men are aboard");
            // (and they took what there was besides: 60 d of silver, and wine for the rest)
            Assert.Less(Trade.Purse.Silver(rules), 1.0);
            Assert.Less(Trade.Hold.Count(wine.id), 4, "with the purse short they took no cargo: " + said);
            Assert.Less(Trade.Debt, before);
            Assert.AreEqual(0.0, Collectors.Var("goons.ravn_next"), "Ravn is dead and next all the same");

            // ---- a fourth: he is not killed twice
            Collectors.PutOut(4, quay, 2.0, r.home);
            said = Collectors.Law("goons_teeth");
            StringAssert.DoesNotContain("Ravn", said);
        }

        [Test]
        public void SheMayGiveCargoOrPayAndThatCoolsHim()
        {
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            var wine = GameData.Good("rhenish_wine");
            Trade.Hold.Add(wine, 8, 0.0, 0.0);
            Voyage.SetMen(4);
            flags.SetFlag(Collectors.OpeningFlag, 1.0, 0.0, 0.0);
            var r = Collectors.Rules;
            var home = GameData.Port(r.home);
            var (hx, hz) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            var quay = new WorldXZ(hx, hz);
            Assert.AreEqual(1.0, Collectors.Var("goons.cargo"), "eight casks of wine are not cargo enough for an instalment at their price");
            Assert.AreEqual(1.0, Collectors.Var("goons.drink"));

            // cargo for an instalment, at six tenths of its worth: 400 d of wine for 240 on the stick
            Collectors.PutOut(1, quay, 3.0, r.home);
            double owed = Trade.Debt;
            string said = Collectors.Law("goons_cargo");
            int left = Trade.Hold.Count(wine.id);
            Assert.AreEqual(8 - 7, left, "400 d of wine at 60 d a cask is seven casks: " + said);
            Assert.AreEqual(owed - System.Math.Round(7 * 60.0 * r.cargoPrice), Trade.Debt, 0.5, said);
            StringAssert.Contains("on the stick", said);
            Assert.AreEqual(GoonState.Home, Collectors.State);
            Assert.AreEqual(3.0, Collectors.Temper, 1e-9, "cargo given moves his temper");

            // a pound paid of her own will: they go home and he cools; two pounds, twice
            Collectors.PutOut(2, quay, 3.0, r.home);
            Collectors.Law("goons_paid");
            Assert.AreEqual(2.0, Collectors.Temper, 1e-9);
            Assert.AreEqual(GoonState.Home, Collectors.State);
            Collectors.PutOut(3, quay, 3.0, r.home);
            Collectors.Law("goons_paid_well");
            Assert.AreEqual(1.0, Collectors.Temper, 1e-9);
            // a way round them that worked is counted, and makes him angrier when they come home with nothing
            Collectors.PutOut(4, quay, 1.0, r.home);
            Collectors.Law("goons_fooled_hide");
            Assert.AreEqual(1, Collectors.Used("hide"));
            Assert.AreEqual(1, Collectors.Fooled);
            Assert.AreEqual(2.0, Collectors.Temper, 1e-9);
            Assert.AreEqual(1.0, Collectors.Var("goons.used_hide"));
            Assert.Less(Collectors.Var("goons.ease_hide"), Self.StatNow("nerve") - 4.0, "hiding the silver is no harder the second time");
            // the cask goes whether or not it makes them drunk
            int casks = Trade.Hold.Count(wine.id);
            Collectors.PutOut(4, quay, 1.0, r.home);
            Collectors.Law("goons_poured");
            Assert.AreEqual(casks - 1, Trade.Hold.Count(wine.id));
            Collectors.Law("goons_drunk");
            Assert.AreEqual(GoonState.Sleeping, Collectors.State);
            // the stick clean: nobody is out, and his temper is spent
            Collectors.Law("goons_done");
            Assert.AreEqual(GoonState.Home, Collectors.State);
            Assert.AreEqual(0.0, Collectors.Temper, 1e-9);
        }

        [Test]
        public void TheDeadDoNotSpeakAndACardThatLeansOnThemDoesNotCome()
        {
            Assert.AreEqual("gone_cast_crew_ravn", EncounterState.GoneFlag(Ravn));
            Assert.AreEqual("ravn", EncounterState.ShortName(Ravn));
            var book = EncounterData.Book();
            var his = book.cards.Where(c => c.actors.Any(a => a.cast == Ravn)).ToList();
            Assert.Greater(his.Count, 20, "few cards have Ravn in them");
            var leaning = his.Where(c => c.leans.Contains(Ravn)).ToList();
            Assert.Greater(leaning.Count, 5, "no card leans on Ravn");
            var state = new EncounterState();
            var card = leaning[0];
            Assert.IsTrue(state.Ready(card, 10.0));
            state.SetFlag(EncounterState.GoneFlag(Ravn), 1.0, 5.0, 0.0);
            Assert.IsTrue(state.Gone(Ravn, 10.0));
            Assert.IsFalse(state.Ready(card, 10.0), card.id + " leans on Ravn and comes when he is dead");
            // every card of his that still comes: none of its words names him, but for a text written for when he is gone
            foreach (var c in his.Where(c => !c.leans.Contains(Ravn)))
            {
                var keys = new List<string> { c.titleKey, c.intentionKey };
                keys.AddRange(c.lines.Where(l => c.Actor(l.who) == null || c.Actor(l.who).cast != Ravn).Select(l => l.key));
                foreach (var o in c.options)
                {
                    keys.Add(o.labelKey);
                    foreach (string branch in new[] { "", "success", "failure" })
                        keys.AddRange(Outcomes.Keys(o, branch));
                }
                foreach (string key in keys)
                {
                    string text = book.Written(key);
                    if (text != null && text.Contains("Ravn"))
                        Assert.IsNotNull(book.Written(key + ".without_ravn"), c.id + " comes when Ravn is dead, and '" + key + "' has him in it with nothing written in its place");
                }
            }
            // the clean stick: its card comes with Ravn dead, he says nothing, and its end is the one written for that
            var stick = book.Card("harbour.wokerers_stick.in_time");
            Assert.IsNotNull(stick);
            CollectionAssert.DoesNotContain(stick.leans, Ravn, "the stick cannot be given up when Ravn is dead: the debt would have no end");
            var game = new FakeGame();
            var d = new EncounterDirector(EncounterData.BookOf(new[] { "harbour.wokerers_stick.in_time" }), 793, game);
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.vars.nums["debt"] = 0.0;
            d.state.SetFlag(Trade.LenderFlag, 1.0, 0.0, 0.0);
            d.SetTime(300.0, 794, 2, 300 * 24);
            CollectionAssert.Contains(d.BeginCall("dorestad").Select(c => c.id).ToList(), stick.id);
            var s = d.OpenNext();
            Assert.IsTrue(s.lines.Any(l => l.who != null && l.who.id == Ravn), "with Ravn alive he has nothing to say of the stick");
            var r = d.Choose(stick.Option("take").index);
            StringAssert.Contains("Ravn", r.written);
            // and again with him dead
            d = new EncounterDirector(EncounterData.BookOf(new[] { "harbour.wokerers_stick.in_time" }), 793, game);
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.vars.nums["debt"] = 0.0;
            d.state.SetFlag(Trade.LenderFlag, 1.0, 0.0, 0.0);
            d.state.SetFlag(EncounterState.GoneFlag(Ravn), 1.0, 0.0, 0.0);
            d.SetTime(300.0, 794, 2, 300 * 24);
            CollectionAssert.Contains(d.BeginCall("dorestad").Select(c => c.id).ToList(), stick.id, "the stick's card does not come with Ravn dead");
            s = d.OpenNext();
            Assert.IsFalse(s.lines.Any(l => l.who != null && l.who.id == Ravn), "Ravn is dead and speaks");
            r = d.Choose(stick.Option("take").index);
            Assert.AreEqual(book.Written("wokerer.stick.in_time.take.done.without_ravn"), r.written, "the end of the stick is not the one written for when Ravn is gone");
            StringAssert.DoesNotContain("Ravn", r.written);
        }
    }
}
