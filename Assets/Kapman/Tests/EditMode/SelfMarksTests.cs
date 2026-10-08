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
    /// EVENTS MOVE THE STATS (the alpha's row 10; the user, 7 Oct 2026: "Stats can be effected by in game events with
    /// caps and dynamic decisions and what not like all the other games that have implemented such mechanics"; the
    /// rule: Design/fixes/stats_events.md). A stat is a base and the marks that lie on it. A mark has a size, a cause
    /// and an end; all marks together move a stat by no more than two; a card puts one on with a verb, a known deed
    /// with the self's table, hunger and sickness by themselves.
    /// </summary>
    public class SelfMarksTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        [SetUp]
        public void Fresh()
        {
            ContentFlags.Use(null);
            Self.Reset();
            Self.Begin("frisian", "christian", "");
        }

        [TearDown]
        public void Done()
        {
            ContentFlags.Use(null);
            Self.Reset();
        }

        [Test]
        public void AMarkLiesOnAStatUntilItsDay()
        {
            Self.SetStat("nerve", 2.0);
            Assert.AreEqual(2, Self.StatNow("nerve", 10.0));
            var m = Self.Mark("nerve", 1, "stood_firm", 30.0, "", "you stood firm", 10.0);
            Assert.IsNotNull(m);
            Assert.AreEqual(3, Self.StatNow("nerve", 10.0), "the mark does not lift the stat");
            Assert.AreEqual(2, Self.Stat("nerve"), "the base moved with the mark");
            Assert.AreEqual(30, m.DaysLeft(10.0));
            Assert.AreEqual(3, Self.StatNow("nerve", 39.9));
            Assert.AreEqual(2, Self.StatNow("nerve", 40.0), "the mark outlived its day");
            Assert.IsEmpty(Self.MarksOn("nerve", 40.0));
            // other stats are not touched
            Self.Mark("lore", -1, "muddled", 5.0, "", "", 0.0);
            Assert.AreEqual(Self.Stat("tongue"), Self.StatNow("tongue", 1.0));
            // a mark that is nothing is not put on
            Assert.IsNull(Self.Mark("luck", 1, "x", 5.0, "", "", 0.0), "a stat that is none of the five took a mark");
            Assert.IsNull(Self.Mark("nerve", 0, "x", 5.0, "", "", 0.0));
            Assert.IsNull(Self.Mark("nerve", 1, "", 5.0, "", "", 0.0));
        }

        [Test]
        public void AllMarksTogetherMoveAStatByNoMoreThanTwo()
        {
            Self.SetStat("name", 2.0);
            Self.Mark("name", 1, "a", 10.0, "", "", 0.0);
            Self.Mark("name", 1, "b", 20.0, "", "", 0.0);
            Self.Mark("name", 1, "c", 30.0, "", "", 0.0);
            Assert.AreEqual(4, Self.StatNow("name", 1.0), "three good things lifted it by three");
            Assert.IsTrue(Self.Capped("name", 1.0));
            // when one ends the third counts again
            Assert.AreEqual(4, Self.StatNow("name", 11.0), "the third mark did not count when the first ended");
            Assert.IsFalse(Self.Capped("name", 11.0));
            Assert.AreEqual(3, Self.StatNow("name", 21.0));
            // down as well as up, and up and down against each other
            Self.SetStat("troth", 4.0);
            Self.Mark("troth", -1, "x", 50.0, "", "", 0.0);
            Self.Mark("troth", -2, "y", 50.0, "", "", 0.0);
            Assert.AreEqual(2, Self.StatNow("troth", 1.0));
            Self.Mark("troth", 1, "z", 50.0, "", "", 0.0);
            Assert.AreEqual(2, Self.StatNow("troth", 1.0), "three down and one up is two down");
            // never under one, never over five
            Self.SetStat("lore", 1.0);
            Self.Mark("lore", -2, "l", 50.0, "", "", 0.0);
            Assert.AreEqual(1, Self.StatNow("lore", 1.0));
            Self.SetStat("tongue", 5.0);
            Self.Mark("tongue", 2, "t", 50.0, "", "", 0.0);
            Assert.AreEqual(5, Self.StatNow("tongue", 1.0));
            // a size beyond two is cut to two
            Assert.AreEqual(2, Self.Mark("nerve", 7, "big", 5.0, "", "", 0.0).size);
        }

        [Test]
        public void TheSameMarkTwiceStartsItsTimeAgain()
        {
            Self.SetStat("nerve", 2.0);
            Self.Mark("nerve", 1, "stood_firm", 10.0, "", "", 0.0);
            Self.Mark("nerve", 1, "stood_firm", 10.0, "", "", 8.0);
            Assert.AreEqual(1, Self.MarksOn("nerve", 8.0).Count, "the same mark lies on the stat twice");
            Assert.AreEqual(3, Self.StatNow("nerve", 12.0), "the second putting on did not start its time again");
            Assert.AreEqual(2, Self.StatNow("nerve", 18.0));
            Assert.IsTrue(Self.Mark("nerve", 1, "again", 3.0, "", "", 0.0) != null && Self.Unmark("again"));
            Assert.IsFalse(Self.Unmark("again"), "a mark was taken off twice");
        }

        [Test]
        public void AMarkEndsWhenItsFlagIsSet()
        {
            var state = new EncounterState();
            double day = 5.0;
            ContentFlags.Use(state, () => day);
            Self.SetStat("troth", 3.0);
            var m = Self.Mark("troth", -1, "broke_the_fast", 0.0, "shriven", "you ate meat on a Friday before the crew", day, "until you are shriven");
            Assert.AreEqual(-1, m.DaysLeft(day), "a mark that ends by a flag counts days");
            Assert.AreEqual(2, Self.StatNow("troth", 400.0), "a mark with no day ended by itself");
            Assert.AreEqual("until you are shriven", Self.Lasts(m, day));
            StringAssert.Contains("Troth is down by one until you are shriven: you ate meat", Self.Said(m, day));
            state.SetFlag("shriven", 1.0, day, 0.0);
            Assert.AreEqual(3, Self.StatNow("troth", day), "the flag is set and the mark still lies on the stat");
            // a mark given neither a day nor a flag has a month: a mark must end
            var loose = Self.Mark("lore", 1, "loose", 0.0, "", "", 2.0);
            Assert.AreEqual(30, loose.DaysLeft(2.0));
        }

        [Test]
        public void MarksAreSavedAndComeBack()
        {
            Self.SetStat("nerve", 2.0);
            Self.Mark("nerve", 1, "stood_firm", 30.0, "", "you stood firm", 10.0);
            Self.Mark("name", -1, "deed.missed_the_day", 0.0, "wk_clean", "you let the day pass", 10.0, "until the stick is clean");
            var saved = Self.Capture();
            // (as a save is: text, read back)
            object back = Json.Parse(Json.Write(saved));
            Self.Reset();
            Assert.IsEmpty(Self.MarksOn(null, 10.0), "a new game carries the last game's marks");
            Self.Restore(back);
            var on = Self.MarksOn(null, 10.0);
            Assert.AreEqual(2, on.Count);
            Assert.AreEqual(3, Self.StatNow("nerve", 10.0));
            var kept = on.First(m => m.id == "deed.missed_the_day");
            Assert.AreEqual("wk_clean", kept.untilFlag);
            Assert.AreEqual("until the stick is clean", kept.untilSaid);
            Assert.AreEqual("you let the day pass", kept.why);
            Assert.AreEqual(-1, kept.size);
            Assert.AreEqual(2, Self.StatNow("nerve", 40.0), "a mark that came back from a save has lost its day");
        }

        [Test]
        public void ACardPutsAMarkOnWithAVerb()
        {
            var errors = new List<string>();
            var good = Effects.ParseList(new List<object>
            {
                "mark nerve +1 stood_at_the_bar for 30", "mark troth -1 broke_the_fast until shriven", "unmark stood_at_the_bar", "stat lore +1", "mark name -2 shamed for 7",
            }, errors, "test");
            Assert.IsEmpty(errors, string.Join("\n", errors));
            Assert.AreEqual(5, good.Count);
            foreach (string bad in new[]
            {
                "mark luck +1 x for 3", "mark nerve +3 x for 3", "mark nerve 1 x for 3", "mark nerve +1 x", "mark nerve +1 x for 0", "mark nerve +1 x since 3",
                "stat nerve +2", "stat luck +1", "unmark", "mark nerve +0 x for 3",
            })
            {
                var e = new List<string>();
                Effects.ParseList(new List<object> { bad }, e, "test");
                Assert.IsNotEmpty(e, "the cards' reader let through: " + bad);
            }
            // through the fake game: the verb reaches the game with its parts
            var fake = new FakeGame();
            var ctx = new EffectContext { game = fake, state = new EncounterState(), day = 3.0 };
            Effects.Apply(good, ctx);
            CollectionAssert.AreEqual(new[] { "mark nerve +1 stood_at_the_bar for 30", "mark troth -1 broke_the_fast until shriven", "unmark stood_at_the_bar", "stat lore +1", "mark name -2 shamed for 7" }, fake.calls);
            // through the real game: the mark lies on the stat, and the card says so
            Self.SetStat("nerve", 2.0);
            Self.SetStat("lore", 1.0);
            var real = new GameEffects { book = EncounterData.Book() };
            var live = new EffectContext { game = real, state = new EncounterState(), day = 3.0 };
            Effects.Apply(Effects.ParseList(new List<object> { "mark nerve +1 stood_at_the_bar for 30", "stat lore +1" }, errors, "test"), live);
            Assert.IsTrue(Self.HasMark("stood_at_the_bar"));
            Assert.AreEqual(3, Self.StatNow("nerve"));
            Assert.AreEqual(2, Self.Stat("lore"), "the base did not move for good");
            StringAssert.Contains("Your Nerve is up by one for 30 days: stood at the bar.", string.Join(" ", live.facts));
            StringAssert.Contains("Your Lore is up by one, for good.", string.Join(" ", live.facts));
            // a written cause is used where there is one (the lender's man's cards have two)
            Effects.Apply(Effects.ParseList(new List<object> { "mark troth -1 lied_to_the_lenders_man for 30" }, errors, "test"), live);
            StringAssert.Contains("you told the lender's man the purse was empty", string.Join(" ", live.facts));
            // a choice may ask for the stat as it is now, for its base, and whether a mark lies on the player
            Assert.AreEqual(3.0, EncounterGameVars.Num("me.nerve", null));
            Assert.AreEqual(2.0, EncounterGameVars.Num("me.nerve.base", null));
            Assert.AreEqual(1.0, EncounterGameVars.Num("mark.stood_at_the_bar", null));
            Assert.AreEqual(0.0, EncounterGameVars.Num("mark.never_put_on", null));
            var words = new WordTable();
            Assert.DoesNotThrow(() => Cond.Parse("me.nerve >= 3 and me.nerve.base < 3 and mark.stood_at_the_bar", words));
            // and the lender's man's cards use the verb
            var card = EncounterData.Book().Card("harbour.wokerers_man.home");
            Assert.IsTrue(card.Option("lie").Chance.failure.SelectMany(x => x.Verbs()).Any(v => v.verb == "mark" && v.a == "troth"), "lying to the lender's man and being seen leaves no mark");
        }

        [Test]
        public void AKnownDeedAndTheBodyPutTheirMarks()
        {
            GameData.LoadPorts(File("ports_793.json"));
            Reputation.Load(File("reputation_793.json"));
            var book = SelfBook.Current;
            Assert.IsTrue(book.FromFile, "the self's book did not come from its file");
            Assert.GreaterOrEqual(book.deedMarks.Count, 15, "the deeds' marks are not in the self's book");
            foreach (var d in book.deedMarks)
            {
                Assert.IsTrue(Self.IsStat(d.stat), d.id);
                Assert.IsTrue(d.size == 1 || d.size == -1 || d.size == 2 || d.size == -2, d.id);
                Assert.IsNotEmpty(d.why, d.id + " has no cause the player can read");
                Assert.IsTrue(d.days > 0.0 || d.untilFlag.Length > 0, d.id + " has no end");
                if (d.untilFlag.Length > 0)
                    Assert.IsNotEmpty(d.untilSaid, d.id + " ends by a flag and does not say when in words");
            }
            Self.SetStat("nerve", 2.0);
            Kapman.Game.Sim.Deeds.Record("fought_raiders", "fought raiders", GameData.Port("dorestad"));
            Assert.IsTrue(Self.HasMark("deed.fought_raiders"), "fighting raiders off left no mark");
            Assert.AreEqual(3, Self.StatNow("nerve"));
            // the missed day lies on the name until the stick is clean
            var state = new EncounterState();
            ContentFlags.Use(state, () => 0.0);
            Kapman.Game.Sim.Deeds.Record("missed_the_day", "missed the day", GameData.Port("dorestad"));
            var missed = Self.MarksOn("name").FirstOrDefault(m => m.id == "deed.missed_the_day");
            Assert.IsNotNull(missed);
            Assert.AreEqual("until the stick is clean", Self.Lasts(missed, 0.0));
            StringAssert.StartsWith("−1 you let the day pass with the boat unpaid (until the stick is clean)", SelfPanel.MarkLine(missed, 0.0));
            state.SetFlag("wk_clean", 1.0, 0.0, 0.0);
            Assert.IsFalse(Self.HasMark("deed.missed_the_day"), "the stick is clean and the missed day still lies on the name");
            // a heathen does not lose troth by a Christian's rule
            Self.Reset();
            Self.Begin("danish", "old_gods", "");
            Kapman.Game.Sim.Deeds.Record("sold_captives", "sold captives", GameData.Port("dorestad"));
            Assert.IsFalse(Self.HasMark("deed.sold_captives"));
            Self.Reset();
            Self.Begin("frisian", "christian", "");
            Kapman.Game.Sim.Deeds.Record("sold_captives", "sold captives", GameData.Port("dorestad"));
            Assert.IsTrue(Self.HasMark("deed.sold_captives"));
            // the body: hunger and sickness lie on her while they last
            Self.SetStat("nerve", 3.0);
            Self.Unmark("deed.fought_raiders");
            Self.Body(true, false);
            Assert.IsTrue(Self.HasMark("body.hungry"));
            Assert.AreEqual(2, Self.StatNow("nerve"));
            Assert.AreEqual("until the men are fed", Self.Lasts(Self.MarksOn("nerve").First(m => m.id == "body.hungry"), 0.0));
            Self.Body(false, true);
            Assert.IsFalse(Self.HasMark("body.hungry"), "the men are fed and hunger still lies on her");
            Assert.IsTrue(Self.HasMark("body.sick"));
            Self.Body(false, false);
            Assert.IsFalse(Self.HasMark("body.sick"), "the crew is well and the sickness still lies on her");
            Assert.IsEmpty(Self.MarksOn(null).Where(m => m.id.StartsWith("body.", System.StringComparison.Ordinal)).ToList());
        }
    }
}
