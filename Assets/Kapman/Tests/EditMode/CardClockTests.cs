using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// CARDS THAT WAIT (the alpha's row 7; the user's rulings 10 and 12 of 7 Oct 2026; Design/fixes/card_clock.md).
    /// "Ordinary cards wait a week for it to automatically press if the player pressed play without interacting with
    /// it." A pressing card (a squall, a man overboard) "will just keep them there until it automatically presses an
    /// option". The time counts only while the clock runs; the card names what it takes; the log says so; a harbour
    /// card left unanswered goes its own way when she casts off; the wait is saved with the game.
    /// </summary>
    public class CardClockTests
    {
        const string Toll = "harbour.tollman.frankish";
        const string Squall = "sea.squall.line";
        const string RulesFile = "Assets/Kapman/Game/Data/card_rules_793.json";

        static EncounterBook three;
        static CardRules real;

        /// <summary>A small book of its own, read once (the director does not change the book).</summary>
        static EncounterBook Three() => three ?? (three = EncounterData.BookOf(Toll, Squall, "sea.overboard.cry"));

        static CardRules Real() => real ?? (real = CardRules.Read(File.ReadAllText(RulesFile)));

        static EncounterDirector AtTheToll(FakeGame game = null, CardRules rules = null, EncounterBook book = null, double silver = 100.0)
        {
            var d = new EncounterDirector(book ?? Three(), 793, game ?? new FakeGame()) { rules = rules ?? Real() };
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.vars.nums["port.toll"] = 0.1;
            d.vars.nums["hold.value"] = 300.0;
            d.vars.nums["hold.weapons"] = 2.0;
            d.vars.nums["crew.trade_best"] = 3.0;
            d.vars.numSource = name => name == "silver" ? silver : 0.0;
            d.SetTime(10.0, 793, 6, 240);
            d.BeginCall("dorestad");
            return d;
        }

        [Test]
        public void TheRulesAreReadAndEveryRowPointsAtItsCard()
        {
            var rules = Real();
            var book = EncounterData.Book();
            Assert.AreEqual(168.0, rules.ordinaryHours);
            int pressing = 0, holds = 0;
            foreach (var c in book.cards)
            {
                Assert.IsTrue(rules.Has(c.id), "the card " + c.id + " has no rule: run Bake/content/card_rules.py");
                var r = rules.For(c.id);
                Assert.IsNotEmpty(r.take, c.id + " names no option to take by itself");
                foreach (string key in r.take)
                    Assert.IsNotNull(c.Option(key), c.id + " is to take the option '" + key + "', and has none of that name: run Bake/content/card_rules.py");
                Assert.AreEqual(c.options.Count, r.take.Distinct().Count(), c.id + ": the options to try are not all of its options, each once");
                if (r.pressing)
                {
                    pressing++;
                    Assert.IsTrue(r.patienceHours >= 1.0 && r.patienceHours <= 72.0, c.id + " is pressing and waits " + r.patienceHours + " hours");
                    Assert.AreNotEqual(Stage.Harbour, c.stage, c.id + " is a harbour card and pressing: what holds her at a quay?");
                }
                else
                {
                    Assert.AreEqual(168.0, r.patienceHours, c.id);
                    Assert.IsFalse(r.holdsShip, c.id + " is ordinary and holds the ship");
                }
                if (r.holdsShip)
                    holds++;
            }
            foreach (var r in rules.All)
                Assert.IsNotNull(book.Card(r.id), "a rule for a card that is not in the book: " + r.id);
            Assert.GreaterOrEqual(pressing, 30, "the pressing cards have gone from the rules");
            Assert.GreaterOrEqual(holds, 25);
            UnityEngine.Debug.Log($"Card rules: {rules.Count} cards; {pressing} pressing, {holds} of them hold the ship.");
            // two that the tests below stand on
            var toll = rules.For(Toll);
            Assert.IsFalse(toll.pressing);
            Assert.AreEqual("declare", toll.take[0]);
            var squall = rules.For(Squall);
            Assert.IsTrue(squall.pressing && squall.holdsShip);
            Assert.AreEqual(1.0, squall.patienceHours);
            Assert.AreEqual("ride", squall.take[0]);
            // a card with no row is ordinary, and a file that cannot be read is no rules at all
            var none = rules.For("no.such.card");
            Assert.IsFalse(none.pressing || none.holdsShip);
            Assert.AreEqual(168.0, none.patienceHours);
            Assert.IsEmpty(none.take);
            Assert.AreEqual(0, CardRules.Read(null).Count);
            Assert.AreEqual(0, CardRules.Read("not json {").Count);
            // by hand: only a pressing card has its own hours and can hold her
            var hand = CardRules.Read("{\"ordinary_hours\":100,\"cards\":[{\"id\":\"a\",\"holds_ship\":true,\"hours\":3,\"take\":[\"x\"]}," +
                                      "{\"id\":\"b\",\"pressing\":true,\"holds_ship\":true,\"hours\":2,\"take\":[\"y\",\"z\"]},{\"id\":\"c\",\"pressing\":true}]}");
            Assert.AreEqual(3, hand.Count);
            Assert.AreEqual(100.0, hand.For("a").patienceHours);
            Assert.IsFalse(hand.For("a").holdsShip);
            Assert.AreEqual(2.0, hand.For("b").patienceHours);
            Assert.IsTrue(hand.For("b").holdsShip);
            CollectionAssert.AreEqual(new[] { "y", "z" }, hand.For("b").take);
            Assert.IsFalse(hand.For("c").holdsShip, "a pressing card that does not say it holds the ship holds her");
            Assert.AreEqual(100.0, hand.For("never").patienceHours);
        }

        // ------------------------------------------------------------------ as she answered the last time (10 Oct 2026)

        static int At(EncounterSession s, string key) => s.options.FindIndex(o => o.def.key == key);

        /// <summary>She lies at the toll again, some days on, with nothing to hide unless the test says so.</summary>
        static void CallsAgain(EncounterDirector d, double day, double weapons = 0.0)
        {
            if (d.InCall)
                d.EndCall();
            d.vars.nums["hold.weapons"] = weapons;
            d.SetTime(day, 793, 6, (long)(day * 24.0));
            d.BeginCall("dorestad");
        }

        /// <summary>THE USER'S RULING OF 10 OCT 2026 (the picker: "As you answered last time"). The first time at a port
        /// she answers the card. After that, if she declared, the toll is taken the same way with nothing asked, by the
        /// card's own choice and effects, and it is said. The long walk had answered this card 117 times.</summary>
        [Test]
        public void TheTollIsTakenAsSheAnsweredTheLastTime()
        {
            var rules = Real();
            CollectionAssert.AreEqual(new[] { "charter", "declare" }, rules.For(Toll).asBefore, "the toll's rule does not name the answers that may stand again: run Bake/content/card_rules.py");
            CollectionAssert.AreEqual(new[] { "hide" }, rules.For(Toll).meets);
            var d = AtTheToll();
            d.vars.nums["hold.weapons"] = 0.0;
            var asBefore = new List<string>();
            var chose = new List<string>();
            d.AnsweredAsBefore += (s, r) => asBefore.Add(r.option + ": " + r.Text);
            d.Chose += (s, r) => chose.Add(r.option);
            // the first time at a port: the card comes, and she answers it
            var first = d.OpenNext();
            Assert.IsNotNull(first, "the first time at a port the toll man's card did not come");
            Assert.AreEqual(Toll, first.card.id);
            var man = first.People.First();
            Assert.IsTrue(d.Choose(At(first, "declare")).ok);
            Assert.IsTrue(d.Continue());
            double feeling = d.npcs.Feeling(man.id, d.merchant);
            Assert.IsEmpty(asBefore);
            Assert.IsEmpty(d.AsBeforeThisCall);
            // the next call: he remembers, and takes it the same way with nothing asked
            CallsAgain(d, 14.0);
            Assert.IsNull(d.OpenNext(), "she declared the last time, and the card came up again");
            Assert.IsNull(d.Current, "a card is left open");
            Assert.AreEqual(1, asBefore.Count, "it was not said that the toll went as before");
            StringAssert.StartsWith("declare: ", asBefore[0]);
            Assert.AreEqual(2, chose.Count, "the card's own choice was not made (its effects are the toll)");
            Assert.AreEqual(feeling + 2.0, d.npcs.Feeling(man.id, d.merchant), 1e-9, "the choice's effects did not run: the toll man thinks no better of her for declaring");
            Assert.AreEqual(1, d.AsBeforeThisCall.Count);
            Assert.AreEqual(Toll, d.AsBeforeThisCall[0].id);
            Assert.IsNotEmpty(d.AsBeforeThisCall[0].title);
            Assert.IsNull(d.OpenNext(), "the card came a second time at one call");
            // and the call after that, the same
            CallsAgain(d, 20.0);
            Assert.IsNull(d.OpenNext());
            Assert.AreEqual(2, asBefore.Count);
            // the frame's line for the log: whole sentences, and the page's rule kept
            var again = AtTheToll();
            again.vars.nums["hold.weapons"] = 0.0;
            var shown = again.OpenNext();
            var result = again.Choose(At(shown, "declare"));
            string line = FrameRoot.AsBeforeLine(shown, result);
            StringAssert.StartsWith("With " + shown.People.First().name + " it goes as it went the last time. ", line);
            StringAssert.Contains(result.Text, line);
            Assert.IsNull(PageWords.Fault(line), "the page's rule: " + line);
        }

        /// <summary>The card comes back if she gave a gift the last time (the ruling's own words), and it comes all the
        /// same when something is different today: there is something to hide, or she holds a charter that she did not
        /// show the last time (those two are mine, the user's to overrule). A charter shown stands again.</summary>
        [Test]
        public void TheTollManComesAgainWhenSomethingIsDifferent()
        {
            var d = AtTheToll();
            d.vars.nums["hold.weapons"] = 0.0;
            int asBefore = 0;
            d.AnsweredAsBefore += (s, r) => asBefore++;
            var s1 = d.OpenNext();
            Assert.IsTrue(d.Choose(At(s1, "declare")).ok);
            d.Continue();
            // swords aboard: with something to hide he is met in person
            CallsAgain(d, 14.0, 2.0);
            var s2 = d.OpenNext();
            Assert.IsNotNull(s2, "with swords aboard the toll was taken as before: she could not choose to hide them");
            Assert.IsTrue(s2.options[At(s2, "hide")].enabled);
            Assert.IsTrue(d.Choose(At(s2, "gift")).ok, "the gift could not be given");
            d.Continue();
            // she gave a gift the last time: the card comes back
            CallsAgain(d, 20.0);
            var s3 = d.OpenNext();
            Assert.IsNotNull(s3, "she gave a gift the last time, and the toll was taken by itself");
            Assert.IsTrue(d.Choose(At(s3, "declare")).ok);
            d.Continue();
            Assert.AreEqual(0, asBefore);
            // declared the last time, and now she holds the charter: the better answer is hers to give
            CallsAgain(d, 26.0);
            d.vars.nums["text.charter_toll_dorestad"] = 1.0;
            var s4 = d.OpenNext();
            Assert.IsNotNull(s4, "she holds a charter now, and the toll was taken as when she had none");
            Assert.IsTrue(s4.options[At(s4, "charter")].enabled, "the charter cannot be shown");
            Assert.IsTrue(d.Choose(At(s4, "charter")).ok);
            d.Continue();
            // the charter shown: the next time it stands by itself
            CallsAgain(d, 32.0);
            string gave = "";
            d.AnsweredAsBefore += (s, r) => gave = r.option;
            Assert.IsNull(d.OpenNext(), "she showed her charter the last time, and the card came up again");
            Assert.AreEqual("charter", gave);
            // the charter gone (the answer cannot be given today): the card comes
            CallsAgain(d, 38.0);
            d.vars.nums["text.charter_toll_dorestad"] = 0.0;
            Assert.IsNotNull(d.OpenNext(), "her last answer cannot be given today, and no card came");
        }

        /// <summary>A card that went by itself at this call can be had again, once, and answered by her own hand; and a
        /// card whose rule names no answers that may stand again always comes.</summary>
        [Test]
        public void ACardThatWentAsBeforeCanBeHadAgainAndAnsweredByHerOwnHand()
        {
            var d = AtTheToll();
            d.vars.nums["hold.weapons"] = 0.0;
            var s1 = d.OpenNext();
            d.Choose(At(s1, "declare"));
            d.Continue();
            CallsAgain(d, 14.0);
            Assert.IsNull(d.OpenNext());
            int version = d.AsBeforeVersion;
            Assert.IsFalse(d.AskAgain("no.such.card"));
            Assert.IsTrue(d.AskAgain(Toll), "the toll that went by itself could not be had again");
            Assert.Greater(d.AsBeforeVersion, version, "the window that shows it is not told");
            Assert.IsEmpty(d.AsBeforeThisCall);
            var again = d.OpenNext();
            Assert.IsNotNull(again, "asked for, the card did not come up");
            Assert.AreEqual(Toll, again.card.id);
            Assert.IsFalse(again.Chosen, "asked for, the card answered itself again");
            Assert.IsTrue(d.Choose(At(again, "gift")).ok);
            d.Continue();
            Assert.IsFalse(d.AskAgain(Toll), "it can be had again more than once at one call");
            // casting off empties the list
            CallsAgain(d, 20.0);
            Assert.IsNotNull(d.OpenNext(), "after a gift the card should come");
            d.Choose(0);
            d.Continue();
            d.EndCall();
            Assert.IsEmpty(d.AsBeforeThisCall);
            // with no such rule (the rules of 7 Oct alone) the card always comes
            var plain = AtTheToll(rules: CardRules.Read("{\"cards\":[{\"id\":\"" + Toll + "\",\"take\":[\"declare\"]}]}"));
            plain.vars.nums["hold.weapons"] = 0.0;
            var p1 = plain.OpenNext();
            plain.Choose(At(p1, "declare"));
            plain.Continue();
            CallsAgain(plain, 14.0);
            Assert.IsNotNull(plain.OpenNext(), "a card with no answers that may stand again was answered by itself");
        }

        [Test]
        public void AnOrdinaryCardWaitsAWeekOfRunningClockThenTakesItsOwnOption()
        {
            var game = new FakeGame();
            var d = AtTheToll(game);
            var s = d.OpenNext();
            Assert.AreEqual(Toll, s.card.id);
            Assert.AreEqual(168.0, d.HoursLeft);
            Assert.IsFalse(d.Pressing);
            Assert.IsFalse(d.HoldsShip, "the toll man holds the ship");
            Assert.AreEqual(s.card.Option("declare").index, d.OwnOption);
            // the clock held: no time counts
            for (int i = 0; i < 1000; i++)
                Assert.IsNull(d.Wait(0.0));
            Assert.AreEqual(0.0, d.WaitedHours);
            int took = 0, chose = 0;
            bool insideChose = false, leftBehind = true;
            ChoiceResult told = null;
            d.TookItself += (card, r, castOff) =>
            {
                took++;
                told = r;
                leftBehind = castOff;
                Assert.AreSame(s, card);
            };
            d.Chose += (card, r) =>
            {
                chose++;
                insideChose = d.TakingItself;
            };
            Assert.IsNull(d.Wait(167.9));
            Assert.AreSame(s, d.Current, "the card closed before its week was out");
            Assert.AreEqual(0.1, d.HoursLeft, 1e-9);
            Assert.AreEqual(0, game.calls.Count, "something came of the card before it was answered");
            var result = d.Wait(0.2);
            Assert.IsNotNull(result, "its week is out and the card still waits");
            Assert.IsTrue(result.ok);
            Assert.AreEqual("declare", result.option);
            Assert.IsNull(d.Current, "the card that answered itself is still open");
            Assert.AreEqual(0.0, d.WaitedHours);
            Assert.AreEqual(0.0, d.HoursLeft);
            Assert.AreEqual(1, took, "the log was told once");
            Assert.AreSame(result, told);
            Assert.IsFalse(leftBehind);
            Assert.AreEqual(1, chose);
            Assert.IsTrue(insideChose, "whoever hears the choice cannot tell that the card made it");
            Assert.IsFalse(d.TakingItself);
            Assert.IsNotEmpty(result.Text, "the option was taken and nothing is told of it");
            // nothing waits now
            Assert.IsNull(d.Wait(500.0));
            Assert.AreEqual(-1, d.OwnOption);
            // the line for the log
            string line = FrameRoot.TookItselfLine(s, result, false);
            StringAssert.StartsWith("You gave “" + s.Title + "” no answer, so it went this way: ", line);
            StringAssert.Contains(s.options[s.card.Option("declare").index].label.TrimEnd('.', ' '), line);
            StringAssert.EndsWith(result.Text, line);
            // (a choice the card has no line of its own for: its words are not said twice)
            var plain = new ChoiceResult { ok = true, option = "declare", told = LogProse.Answered(s.options[s.card.Option("declare").index].label) };
            plain.facts.Add("The crew are in better heart.");
            string once = FrameRoot.TookItselfLine(s, plain, false);
            StringAssert.EndsWith("so it went this way: " + s.options[s.card.Option("declare").index].label.TrimEnd('.', ' ') + ". The crew are in better heart.", once);
            StringAssert.DoesNotContain("Your answer was", once);
            StringAssert.StartsWith("You cast off and left “", FrameRoot.TookItselfLine(s, result, true));
        }

        [Test]
        public void AnsweredInTimeItTakesNothingByItself()
        {
            var game = new FakeGame();
            var d = AtTheToll(game);
            var s = d.OpenNext();
            d.Wait(100.0);
            Assert.IsTrue(d.Choose(s.card.Option("declare").index).ok);
            int calls = game.calls.Count;
            Assert.AreEqual(0.0, d.HoursLeft);
            Assert.IsNull(d.Wait(500.0), "a card already answered answered itself as well");
            Assert.AreEqual(calls, game.calls.Count);
            Assert.AreSame(s, d.Current, "the answered card stays up until the player goes on");
            Assert.IsTrue(d.Continue());
            Assert.AreEqual(0.0, d.WaitedHours);
        }

        [Test]
        public void AnOptionThatCannotBeTakenIsPassedOverInOrder()
        {
            // "charter" needs a charter in the chest (there is none); "gift" costs twelve pennies
            var order = CardRules.Read("{\"cards\":[{\"id\":\"" + Toll + "\",\"take\":[\"charter\",\"gift\",\"declare\",\"hide\"]}]}");
            var rich = AtTheToll(rules: order, silver: 100.0);
            var s = rich.OpenNext();
            Assert.IsFalse(s.options[s.card.Option("charter").index].enabled);
            Assert.AreEqual(s.card.Option("gift").index, rich.OwnOption);
            Assert.AreEqual("gift", rich.Wait(168.0).option);
            var poor = AtTheToll(rules: order, silver: 0.0);
            s = poor.OpenNext();
            Assert.IsFalse(s.options[s.card.Option("gift").index].enabled, "a gift of twelve pennies can be made from an empty purse");
            Assert.AreEqual("declare", poor.Wait(168.0).option);
            // a rule that names an option the card no longer has: passed over
            var stale = AtTheToll(rules: CardRules.Read("{\"cards\":[{\"id\":\"" + Toll + "\",\"take\":[\"gone\",\"hide\"]}]}"));
            stale.OpenNext();
            Assert.AreEqual("hide", stale.Wait(168.0).option);
            // no rules at all: the card's own usual answer
            var bare = AtTheToll(rules: new CardRules());
            s = bare.OpenNext();
            var usual = s.card.options.First(o => o.isDefault);
            Assert.AreEqual(usual.key, bare.Wait(168.0).option);
            var noRules = new EncounterDirector(Three(), 793, new FakeGame());
            Assert.IsNull(noRules.rules);
            noRules.state.Enqueue(Squall, true);
            noRules.OpenNext();
            Assert.IsFalse(noRules.Pressing || noRules.HoldsShip, "with no rules a card is ordinary");
            Assert.AreEqual(168.0, noRules.HoursLeft);
        }

        [Test]
        public void WhereNoneCanBeTakenItGoesOnWaiting()
        {
            var book = EncounterData.BookOf(Toll);                       // (a book of its own: the test changes the card)
            foreach (var o in book.Card(Toll).options)
                o.check = Cond.Parse("silver >= 1000000", book.words);
            var d = AtTheToll(book: book);
            var s = d.OpenNext();
            Assert.IsTrue(s.options.All(o => !o.enabled));
            Assert.AreEqual(-1, d.OwnOption);
            Assert.IsNull(d.Wait(200.0));
            Assert.AreSame(s, d.Current, "a card none of whose options can be taken closed by itself");
            Assert.AreEqual(200.0, d.WaitedHours);
            Assert.IsNull(d.Wait(0.5));
            Assert.IsNull(d.Wait(0.6));
            Assert.AreEqual("It waits for your answer.", FrameRoot.WaitLine(d, s));
            // she casts off: nobody follows her out, and nothing can be taken, so it is put away
            int closed = 0;
            d.Closed += c => closed++;
            d.EndCall();
            Assert.IsNull(d.Current);
            Assert.AreEqual(1, closed);
            Assert.AreEqual(0.0, d.WaitedHours);
        }

        [Test]
        public void APressingCardHoldsTheShipAndAnswersItselfInItsOwnHours()
        {
            var game = new FakeGame();
            var d = new EncounterDirector(Three(), 793, game) { rules = Real() };
            d.roll = () => 0.0;
            d.SetTime(10.0, 793, 6, 240);
            Assert.IsFalse(d.HoldsShip, "she is held with no card open");
            d.state.Enqueue(Squall, true);
            var s = d.OpenNext();
            Assert.AreEqual(Squall, s.card.id);
            Assert.IsTrue(d.Pressing);
            Assert.IsTrue(d.HoldsShip, "a squall's card does not hold her");
            Assert.AreEqual(1.0, d.HoursLeft);
            Assert.IsNull(d.Wait(0.5));
            Assert.IsTrue(d.HoldsShip);
            var r = d.Wait(0.5 + 1.0 / 60.0);
            Assert.IsNotNull(r, "its hour is up and the squall's card still waits");
            Assert.AreEqual("ride", r.option);
            Assert.IsFalse(d.HoldsShip, "she is still held after the card answered itself");
            Assert.IsFalse(d.Pressing);
            Assert.IsNull(d.Current);
            // answered by the player: she is let go at once
            d.state.Enqueue("sea.overboard.cry", true);
            s = d.OpenNext();
            Assert.IsTrue(d.HoldsShip);
            Assert.IsTrue(d.Choose(0).ok);
            Assert.IsFalse(d.HoldsShip, "she is held by a card that has its answer");
            Assert.IsFalse(d.Pressing);
            d.Continue();
            // a pressing card that does not hold her (eight of them do not)
            var loose = new EncounterDirector(Three(), 793, new FakeGame())
            {
                rules = CardRules.Read("{\"cards\":[{\"id\":\"" + Squall + "\",\"pressing\":true,\"holds_ship\":false,\"hours\":2,\"take\":[\"bare\"]}]}"),
            };
            loose.SetTime(10.0, 793, 6, 240);
            loose.state.Enqueue(Squall, true);
            loose.OpenNext();
            Assert.IsTrue(loose.Pressing);
            Assert.IsFalse(loose.HoldsShip);
            Assert.AreEqual(2.0, loose.HoursLeft);
            Assert.IsNull(loose.Wait(1.99));
            Assert.AreEqual("bare", loose.Wait(0.02).option);
            // a sea card is not a harbour card: casting off does not answer it
            var mixed = AtTheToll();
            mixed.state.Enqueue(Squall, true);
            s = mixed.OpenNext();
            Assert.AreEqual(Squall, s.card.id);
            mixed.EndCall();
            Assert.AreSame(s, mixed.Current, "casting off answered a card of the sea");
            Assert.IsFalse(s.Chosen);
        }

        [Test]
        public void SheCastsOffAndTheHarbourCardGoesItsOwnWay()
        {
            var game = new FakeGame();
            var d = AtTheToll(game);
            var s = d.OpenNext();
            d.Wait(5.0);
            ChoiceResult told = null;
            bool leftBehind = false;
            d.TookItself += (card, r, castOff) =>
            {
                told = r;
                leftBehind = castOff;
            };
            string ended = null;
            d.CallEnded += port => ended = port;
            d.EndCall();
            Assert.IsNotNull(told, "she cast off with the toll man unanswered, and nothing came of it");
            Assert.IsTrue(leftBehind);
            Assert.AreEqual("declare", told.option);
            Assert.IsNull(d.Current);
            Assert.AreEqual("dorestad", ended);
            Assert.IsFalse(d.InCall);
            Assert.IsNotEmpty(told.Text);
            // an answered card still up when she casts off is left as it is
            var e = AtTheToll();
            s = e.OpenNext();
            e.Choose(s.card.Option("declare").index);
            int took = 0;
            e.TookItself += (card, r, castOff) => took++;
            e.EndCall();
            Assert.AreEqual(0, took);
            Assert.AreSame(s, e.Current);
        }

        [Test]
        public void TheWaitIsSavedAndComesBack()
        {
            var d = AtTheToll();
            d.OpenNext();
            d.Wait(100.0);
            // (as a save is: text, read back)
            var saved = Json.Parse(Json.Write(d.Capture())) as Dictionary<string, object>;
            Assert.IsNotNull(saved);
            var e = AtTheToll();
            e.Restore(saved);
            Assert.AreEqual(0.0, e.WaitedHours);
            // (saved again before the card has come back up: the hours are not lost)
            var again = Json.Parse(Json.Write(e.Capture())) as Dictionary<string, object>;
            var s = e.OpenNext();
            Assert.AreEqual(Toll, s.card.id);
            Assert.AreEqual(100.0, e.WaitedHours, 1e-9);
            Assert.AreEqual(68.0, e.HoursLeft, 1e-9);
            Assert.IsNull(e.Wait(67.9));
            Assert.AreEqual("declare", e.Wait(0.2).option);
            var f = AtTheToll();
            f.Restore(again);
            f.OpenNext();
            Assert.AreEqual(100.0, f.WaitedHours, 1e-9, "a game saved before its open card came back up lost the card's hours");
            // an older save has neither key: the card starts its wait again
            saved.Remove("wait_card");
            saved.Remove("wait_hours");
            var old = AtTheToll();
            old.Restore(saved);
            old.OpenNext();
            Assert.AreEqual(Toll, old.Current.card.id);
            Assert.AreEqual(0.0, old.WaitedHours);
            Assert.AreEqual(168.0, old.HoursLeft);
            // the hours of one card are not another's
            var other = AtTheToll();
            other.Restore(again);
            other.state.Enqueue(Squall, true);
            other.OpenNext();
            Assert.AreEqual(Squall, other.Current.card.id);
            Assert.AreEqual(0.0, other.WaitedHours, "the squall's card took the toll man's hours");
        }

        [Test]
        public void ACardLeftToWaitSaysHowLongAndWhatItThenDoes()
        {
            Assert.AreEqual("7 days", FrameRoot.Span(168.0));
            Assert.AreEqual("6 days and 5 hours", FrameRoot.Span(148.2));
            Assert.AreEqual("1 hour", FrameRoot.Span(0.4));
            Assert.AreEqual("1 hour", FrameRoot.Span(1.0));
            Assert.AreEqual("2 hours", FrameRoot.Span(1.01));
            Assert.AreEqual("1 day", FrameRoot.Span(24.0));
            Assert.AreEqual("1 day and 1 hour", FrameRoot.Span(24.5));
            Assert.AreEqual("0 hours", FrameRoot.Span(0.0));
            var d = AtTheToll();
            var s = d.OpenNext();
            var spec = FrameRoot.CardSpec(d, s);
            Assert.IsNotNull(spec.waits, "an unanswered card does not say how long it waits");
            string declare = s.options[s.card.Option("declare").index].label.TrimEnd('.', ' ');
            Assert.AreEqual("It waits 7 days more. Then: " + declare + ".", spec.waits());
            d.Wait(20.0);
            Assert.AreEqual("It waits 6 days and 4 hours more. Then: " + declare + ".", spec.waits());
            d.Wait(147.5);
            Assert.AreEqual("It waits 1 hour more. Then: " + declare + ".", spec.waits());
            d.Choose(s.card.Option("declare").index);
            Assert.AreEqual("", spec.waits(), "an answered card still says it waits");
            Assert.IsNull(FrameRoot.CardSpec(d, s).waits);
            d.Continue();
            // a card that holds her says so first
            d.state.Enqueue(Squall, true);
            s = d.OpenNext();
            string ride = s.options[s.card.Option("ride").index].label.TrimEnd('.', ' ');
            Assert.AreEqual("She is held here. It waits 1 hour more. Then: " + ride + ".", FrameRoot.WaitLine(d, s));
            Assert.AreEqual("", FrameRoot.WaitLine(d, null));
            Assert.AreEqual("", FrameRoot.WaitLine(null, s));
        }
    }
}
