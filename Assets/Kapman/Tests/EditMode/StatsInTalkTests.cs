using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE STATS IN TALK (7 Oct 2026; Design/fate_faith_and_self.md section 2): the cards' conditions read the self
    /// (me.tongue, me.name, me.nerve, me.lore, me.troth as numbers; me.people, me.faith, me.past as text); a chance
    /// may be "by me.tongue"; an option whose label begins with a tag in square brackets shows the tag and the odds as
    /// a word; a stat used with success rises slowly, and the card says so when a pip fills; a tongue a card teaches is
    /// held by the self. The cards already written (crew.trade_best and the like) are untouched.
    /// </summary>
    public class StatsInTalkTests
    {
        const string Plea =
            "{\"id\":\"test.self.plea\",\"stage\":\"harbour\",\"arrival\":true,\"when\":\"true\",\"cooldown_days\":0," +
            "\"title_key\":\"test.self.plea.title\",\"title\":\"A plea at the toll\"," +
            "\"options\":[" +
            "{\"key\":\"plead\",\"label_key\":\"test.self.plea.plead\",\"label\":\"[Tongue] Ask him to wait till Lammas\",\"check\":\"true\"," +
            "\"effects\":[{\"chance\":0.55,\"by\":\"me.tongue\",\"success\":[\"silver +5\"],\"failure\":[\"silver -5\"]}]}," +
            "{\"key\":\"quote\",\"label_key\":\"test.self.plea.quote\",\"label\":\"[Lore] Quote the law to him\",\"check\":\"me.lore >= 3\"}," +
            "{\"key\":\"kin\",\"label_key\":\"test.self.plea.kin\",\"label\":\"[Frisian to Frisian] Speak as one of his own\",\"check\":\"me.people = frisian\"}," +
            "{\"key\":\"pay\",\"label_key\":\"test.self.plea.pay\",\"label\":\"Pay and go\",\"check\":\"true\"}" +
            "]}";

        string peopleWas;

        [SetUp]
        public void Up()
        {
            peopleWas = PlayerPrefs.GetString("kapman.people", "frisian");
            ContentFiles.SetRoot(null);
            Self.Reset();
        }

        [TearDown]
        public void Down()
        {
            Self.Reset();
            MapPalette.SetPeople(peopleWas);
        }

        /// <summary>The test's card first in line at Dorestat, with the live game's variables (the self among them).</summary>
        static EncounterDirector AtTheToll(double roll = 0.0)
        {
            var src = ContentTestKit.Tables();
            src.cards.Add(("encounters/test_self.json", "{\"cards\":[" + Plea + "]}"));
            var book = EncounterLoader.Load(src);
            Assert.IsTrue(book.Ok, "the test's card has faults:\n" + string.Join("\n", book.errors));
            var d = new EncounterDirector(book, 793, new FakeGame());
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => roll;
            EncounterGameVars.Attach(d.vars, () => d.state.callPort);
            d.SetTime(10.0, 793, 6, 240);
            d.BeginCall("dorestad");
            return d;
        }

        [Test]
        public void TheConditionLanguageKnowsTheSelf()
        {
            foreach (string name in new[] { "me.tongue", "me.name", "me.nerve", "me.lore", "me.troth" })
            {
                Assert.IsTrue(VarNames.TryType(name, out var type, out _), name + " is not a variable");
                Assert.AreEqual(VarType.Number, type, name);
            }
            foreach (string name in new[] { "me.people", "me.faith", "me.past" })
            {
                Assert.IsTrue(VarNames.TryType(name, out var type, out _), name + " is not a variable");
                Assert.AreEqual(VarType.Text, type, name);
            }
            Assert.IsFalse(VarNames.TryType("me.luck", out _, out _), "only the five stats and the three picks");

            var words = new WordTable();
            var v = new EncounterVars();
            v.nums["me.lore"] = 3.0;
            v.texts["me.people"] = "frisian";
            v.texts["me.faith"] = "old_gods";
            Assert.IsTrue(Cond.Parse("me.lore >= 3", words).Eval(v));
            Assert.IsFalse(Cond.Parse("me.lore > 3", words).Eval(v));
            Assert.IsTrue(Cond.Parse("me.people = frisian and me.faith != christian", words).Eval(v));
            Assert.IsFalse(Cond.Parse("me.past = toll_clerk", words).Eval(v));
            // a check that fails says its rule in plain words
            Assert.AreEqual("Your Lore must be 4 or more.", Cond.Parse("me.lore >= 4", words).Describe(words));
            Assert.AreEqual("Only if your people is danish.", Cond.Parse("me.people = danish", words).Describe(words));
        }

        [Test]
        public void TheLiveGameAnswersWithTheSelf()
        {
            Self.Begin("danish", "prime_signed", "captive_home");
            Assert.AreEqual(3.0, EncounterGameVars.Num("me.nerve", null), 1e-9, "a captive come home has Nerve 3");
            Assert.AreEqual(1.0, EncounterGameVars.Num("me.tongue", null), 1e-9);
            Assert.AreEqual(3.0, EncounterGameVars.Num("me.troth", null), 1e-9);
            Assert.AreEqual(0.0, EncounterGameVars.Num("me.luck", null), 1e-9);
            Assert.AreEqual("danish", EncounterGameVars.Text("me.people", null));
            Assert.AreEqual("prime_signed", EncounterGameVars.Text("me.faith", null));
            Assert.AreEqual("captive_home", EncounterGameVars.Text("me.past", null));
            Assert.AreEqual(1.0, EncounterGameVars.Num("tongue.old_norse", null), 1e-9, "a Dane has Norse");
            Assert.GreaterOrEqual(EncounterGameVars.Num("tongue.old_frisian", null), 0.5, "and half of Frisian");
            Assert.Less(EncounterGameVars.Num("tongue.irish", null), 0.5);
        }

        [Test]
        public void ATaggedOptionShowsItsTagAndItsOddsAsAWord()
        {
            Self.Begin("frisian", "christian", "steersmans_child");      // Tongue 1, Lore 2
            var d = AtTheToll();
            var s = d.OpenNext();
            Assert.IsNotNull(s, "the test's card did not open");
            var plead = s.options[s.card.Option("plead").index];
            Assert.AreEqual("Tongue", plead.tag);
            Assert.AreEqual("Ask him to wait till Lammas", plead.label, "the label is the words without the tag");
            Assert.AreEqual("[Tongue] Ask him to wait till Lammas", plead.Tagged);
            Assert.AreEqual(0.45, plead.chance, 1e-9, "0.55 as written, less a tenth for Tongue 1 (0.05 a pip from 3)");
            Assert.AreEqual("doubtful", plead.odds);
            var quote = s.options[s.card.Option("quote").index];
            Assert.AreEqual("Lore", quote.tag);
            Assert.IsFalse(quote.enabled, "Lore 2 is not Lore 3");
            Assert.AreEqual("Your Lore must be 3 or more.", quote.rule);
            var kin = s.options[s.card.Option("kin").index];
            Assert.AreEqual("Frisian to Frisian", kin.tag);
            Assert.IsTrue(kin.enabled, "a Frisian may speak as kin");
            var pay = s.options[s.card.Option("pay").index];
            Assert.AreEqual("", pay.tag);
            Assert.AreEqual("Pay and go", pay.label);

            // as the frame shows it: the tag, the words, the odds word; the rule under a choice that cannot be made
            var spec = FrameRoot.CardSpec(d, s);
            Assert.AreEqual("Tongue", spec.options[plead.def.index].tag);
            Assert.AreEqual("Ask him to wait till Lammas", spec.options[plead.def.index].label);
            Assert.AreEqual("doubtful", spec.options[plead.def.index].odds);
            Assert.IsFalse(spec.options[quote.def.index].enabled);
            Assert.AreEqual("Your Lore must be 3 or more.", spec.options[quote.def.index].rule);
        }

        [Test]
        public void TheStatMovesTheOddsAndThePicksOpenTheirOwnChoices()
        {
            Self.Begin("danish", "old_gods", "toll_clerk");               // Tongue 3
            var s = AtTheToll().OpenNext();
            var plead = s.options[s.card.Option("plead").index];
            Assert.AreEqual(0.55, plead.chance, 1e-9, "at Tongue 3 the chance is as written");
            Assert.AreEqual("likely", plead.odds);
            Assert.IsFalse(s.options[s.card.Option("kin").index].enabled, "a Dane cannot speak as a Frisian");
            Assert.AreEqual("Only if your people is frisian.", s.options[s.card.Option("kin").index].rule);

            Self.SetStat("tongue", 5.0);
            Self.SetStat("lore", 3.0);
            s = AtTheToll().OpenNext();
            Assert.AreEqual(0.65, s.options[s.card.Option("plead").index].chance, 1e-9, "two pips over three: a tenth better");
            Assert.IsTrue(s.options[s.card.Option("quote").index].enabled, "Lore 3 may quote");
        }

        [Test]
        public void AStatUsedWithSuccessRisesAndTheCardSaysSoWhenAPipFills()
        {
            Self.Begin("frisian", "christian", "steersmans_child");      // Tongue 1
            ChoiceResult last = null;
            for (int win = 1; win <= 3; win++)
            {
                var d = AtTheToll(0.0);                                   // (the dice always win)
                var s = d.OpenNext();
                last = d.Choose(s.card.Option("plead").index);
                Assert.IsTrue(last.ok);
                Assert.AreEqual("success", last.branch);
                if (win < 3)
                {
                    Assert.AreEqual(1, Self.Stat("tongue"), "one win is not a pip");
                    Assert.IsFalse(last.Text.Contains("Tongue"), "nothing is said until a pip fills: " + last.Text);
                }
            }
            Assert.AreEqual(2, Self.Stat("tongue"), "three wins against one who resisted fill the second pip");
            StringAssert.Contains("Your Tongue has grown to 2 of 5.", last.Text);

            // a lost check teaches nothing
            double before = Self.StatRaw("tongue");
            var lost = AtTheToll(0.99);
            var open = lost.OpenNext();
            var r = lost.Choose(open.card.Option("plead").index);
            Assert.AreEqual("failure", r.branch);
            Assert.AreEqual(before, Self.StatRaw("tongue"), 1e-9);

            // a choice with no chance by a stat moves nothing
            var plain = AtTheToll();
            open = plain.OpenNext();
            plain.Choose(open.card.Option("pay").index);
            Assert.AreEqual(before, Self.StatRaw("tongue"), 1e-9);

            // the director's rule can be put aside (a test, a tool)
            var quiet = AtTheToll(0.0);
            quiet.statUsed = null;
            open = quiet.OpenNext();
            quiet.Choose(open.card.Option("plead").index);
            Assert.AreEqual(before, Self.StatRaw("tongue"), 1e-9);
        }

        [Test]
        public void TheCardsAlreadyWrittenKeepWorking()
        {
            // every card file of the game still loads with the new names in the language, and none uses a tag yet
            var book = EncounterData.Book();
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            var tagged = new List<string>();
            foreach (var c in book.cards)
                foreach (var o in c.options)
                {
                    ShownOption.SplitTag(book.Written(o.labelKey) ?? o.label, out string tag, out _);
                    if (tag.Length > 0)
                        tagged.Add(c.id + "." + o.key + " [" + tag + "]");
                }
            Debug.Log($"Stats in talk: {book.cards.Count} cards load; options with a tag today: {tagged.Count}" +
                      (tagged.Count > 0 ? " (" + string.Join(", ", tagged.GetRange(0, System.Math.Min(5, tagged.Count))) + ")" : ""));
            // a label with brackets that are not a tag is left alone
            ShownOption.SplitTag("[sic]", out string t, out string l);
            Assert.AreEqual("", t);
            Assert.AreEqual("[sic]", l);
            ShownOption.SplitTag("Pay [in kind]", out t, out l);
            Assert.AreEqual("", t);
            Assert.AreEqual("Pay [in kind]", l);
            ShownOption.SplitTag("  [Nerve]   Say nothing ", out t, out l);
            Assert.AreEqual("Nerve", t);
            Assert.AreEqual("Say nothing", l);
        }

        [Test]
        public void ATongueACardTeachesIsHeldByTheSelf()
        {
            Self.Begin("frisian", "christian", "steersmans_child");
            var game = new GameEffects();
            Assert.AreEqual(0.0, Self.TongueHeld("old_norse"), 1e-9);
            Assert.AreEqual("You follow some Norse now.", NoBrackets(game.Tongue("old_norse", 0.5)));
            Assert.AreEqual(0.5, Self.TongueHeld("old_norse"), 1e-9);
            Assert.IsNull(game.Tongue("old_norse", 0.4), "a tongue is never taken away");
            Assert.AreEqual(0.5, Self.TongueHeld("old_norse"), 1e-9);
            Assert.IsNotNull(game.Tongue("norse", 1.0), "the cards' older name for it");
            Assert.AreEqual(1.0, Self.TongueHeld("old_norse"), 1e-9);
        }

        /// <summary>"Norse (the Danish tongue)" as "Norse": the tongues' book may give the longer name.</summary>
        static string NoBrackets(string s)
        {
            if (s == null)
                return null;
            int open = s.IndexOf(" (", System.StringComparison.Ordinal), close = s.IndexOf(')');
            return open > 0 && close > open ? s.Remove(open, close - open + 1) : s;
        }
    }
}
