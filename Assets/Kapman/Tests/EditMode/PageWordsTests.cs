using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PAGE'S RULE (the alpha's row 17, "a clean screen"; Design/fixes/page_and_strings.md, 7 Oct 2026): nothing
    /// reaches the screen that was written for the makers. PageWords.Fault names what is wrong with a string; the play
    /// tests hold every word on every screen they picture to it. Here: the function itself, the data the frame
    /// prints, a choice's price said as an aside, a lock said in the world's words, and the first morning's order.
    /// </summary>
    public class PageWordsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        [TearDown]
        public void Done()
        {
            Cond.NameOf = null;
            typeof(FirstJobs).GetMethod("OnPlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
            Commissions.Load(null);
        }

        [Test]
        public void WhatWasWrittenForTheMakersIsNamed()
        {
            var faulty = new Dictionary<string, string>
            {
                ["News, prices elsewhere, the weather (to come)."] = "says what is to come",
                ["Her wear, her fittings and a yard to mend her at are not in the game yet."] = "says what the game lacks",
                ["Keeping these days is not counted yet: nothing stops a sale on a feast."] = "says what the game lacks",
                ["not yet"] = "says what the game lacks",
                ["[write: harbour.tollman.greet]"] = "an unwritten key",
                ["Your standing here: known (+25). In the region: known (+10)."] = "a number in brackets",
                ["Give him something (12 d)"] = "a number in brackets",
                ["Dorestat (here)"] = "a word in brackets",
                ["[Tongue] Ask him to wait till Lammas"] = "a tag or key in brackets",
                ["Only with the writing \"charter toll dorestad\" in your chest."] = "a name in quote marks",
                ["Either: Only with weapons aboard; or: Only with captives aboard."] = "the sums joined",
                ["Only when crew.seamanship_best."] = "an id",
                ["For a Dane it is old_frisian."] = "an id",
                ["calendar_793.json gives them for each year."] = "an id",
                ["The Wares window has each ware."] = "a workshop word",
                ["A stat rises by use, never by spending."] = "a workshop word",
                ["Only when hold value >= 200."] = "a sign of the sums",
            };
            foreach (var kv in faulty)
                Assert.AreEqual(kv.Value, PageWords.Fault(kv.Key), kv.Key);
            foreach (string clean in new[]
            {
                "News, and what other places are paying.", "Your standing here: known. In the region: known.", "Give him something, 12 d",
                "Dorestat, here", "Only with the toll charter of Dorestat in your chest.", "Only if you carry weapons, or captives.",
                "Traiectum (Utrecht)", "Not yet: one more season, and then I will.", "These grow by use, never by silver.",
                "2 casks of Rhenish wine for Traiectum by 11 May 793, at 46 d each, 91 d in all", "She is held here. It waits 1 hour more. Then: Keep the sail.",
                "", null,
            })
                Assert.IsNull(PageWords.Fault(clean), clean);
            // a tooltip is the interface speaking, and the player asked: a key and the word window may stand there
            Assert.IsNull(PageWords.Fault("Save, load, a new game, quit  [Esc]", tooltip: true));
            Assert.IsNull(PageWords.Fault("Brings up the Voyage window.", tooltip: true));
            Assert.IsNotNull(PageWords.Fault("Save, load, a new game, quit  [Esc]"));
            // in a card, a talk and the log the world speaks: a window is a hole in a wall, and somebody may quote somebody
            Assert.IsNull(PageWords.Fault("She leaned out of the window and called down.", prose: true));
            Assert.IsNull(PageWords.Fault("He said \"go\" and they went.", prose: true));
            Assert.IsNotNull(PageWords.Fault("Count him out a pound (240 d)", prose: true), "a number in brackets is a fault in a card too");
        }

        [Test]
        public void AChoicesPriceIsSaidAsAnAside()
        {
            Assert.AreEqual("Give him something, 12 d", ShownOption.Aside("Give him something (12 d)"));
            Assert.AreEqual("Buy a brace of duck, 4 d, and ask him the way in over the Deben bar", ShownOption.Aside("Buy a brace of duck (4 d) and ask him the way in over the Deben bar"));
            Assert.AreEqual("Haul her and broach the tar in the hold, 3 d for moss and rollers", ShownOption.Aside("Haul her and broach the tar in the hold (3 d for moss and rollers)"));
            Assert.AreEqual("Pay him, 6 d.", ShownOption.Aside("Pay him (6 d)."));
            Assert.AreEqual("Walk on", ShownOption.Aside("Walk on"));
            Assert.AreEqual("Change a purse of deniers at his rate, the fire takes 5 d", ShownOption.Aside("Change a purse of deniers at his rate (the fire takes 5 d)"));
            Assert.AreEqual("Wait (as he asks)", ShownOption.Aside("Wait (as he asks)"), "a bracket with no number in it is not a price");
            Assert.AreEqual("", ShownOption.Aside(null));
            ShownOption.SplitTag("[Tongue] Ask him to take less (3 d)", out string tag, out string label);
            Assert.AreEqual("Tongue", tag);
            Assert.AreEqual("Ask him to take less, 3 d", label);
            // every choice of the script, as it is shown: none has a number in brackets left
            var book = EncounterData.Book();
            int priced = 0, choices = 0;
            foreach (var c in book.cards)
                foreach (var o in c.options)
                {
                    string written = book.Written(o.labelKey) ?? o.label;
                    if (string.IsNullOrEmpty(written))
                        continue;
                    choices++;
                    ShownOption.SplitTag(written, out _, out string shown);
                    if (shown != written.Trim() && written.IndexOf('(') >= 0)
                        priced++;
                    Assert.AreNotEqual("a number in brackets", PageWords.Fault(shown, prose: true), c.id + " " + o.key + ": " + shown);
                }
            Assert.Greater(priced, 300, "the script's choices with a price in brackets were not found");
            Debug.Log($"Page words: {choices} choices, {priced} with a price that is said as an aside.");
        }

        [Test]
        public void ALockIsSaidInTheWorldsWords()
        {
            var words = EncounterData.Book().words;
            // (as the game sets it: a writing's title, a good's name, a port's, a role's title, a stat's level)
            Cond.NameOf = key => key == "text.charter_toll_dorestad" ? "the toll charter of Dorestat"
                                : key == "hold.captives" ? "captives" : key == "hold.weapons" ? "weapons"
                                : key == "port:dorestad" ? "Dorestat" : key == "role:tollman" ? "the toll man"
                                : key == "level:tongue:3" ? "ready" : null;
            var said = new Dictionary<string, string>
            {
                ["text.charter_toll_dorestad"] = "Only with the toll charter of Dorestat in your chest.",
                ["text.no_such_writing"] = "Only with a writing you have not got.",
                ["flag.ch_black_stones_3"] = "Not now.",
                ["not flag.ch_black_stones_3"] = "Not now.",
                ["hold.captives"] = "Only if you carry captives.",
                ["not hold.weapons"] = "Not while you carry weapons.",
                ["hold.weapons > 0 or hold.captives > 0"] = "Only if you carry weapons, or captives.",
                ["hold.captives = 0"] = "Only if you carry no captives.",
                ["port.id = dorestad"] = "Only at Dorestat.",
                ["me.people = danish"] = "Not for your people.",
                ["me.tongue >= 3"] = "Your Tongue must be ready or better.",
                ["me.tongue >= 5"] = "Your Tongue must be 5 or more.",
                ["hold.value >= 200"] = "Not now.",
                ["crew.seamanship_best"] = "Not now.",
            };
            foreach (var kv in said)
            {
                string line = Cond.Parse(kv.Key, words).Describe(words);
                Assert.AreEqual(kv.Value, line, kv.Key);
                Assert.IsNull(PageWords.Fault(line), kv.Key + ": " + line);
            }
            // every lock of every card, as the game says it with its names at hand: no id, no sum, no key
            Cond.NameOf = EncounterRunner.NameOf;
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ports_793.json"));
            var book = EncounterData.Book();
            var faults = new List<string>();
            int locks = 0;
            foreach (var c in book.cards)
                foreach (var o in c.options)
                {
                    string line = o.check.Describe(book.words);
                    if (string.IsNullOrEmpty(line))
                        continue;
                    locks++;
                    string wrong = PageWords.Fault(line);
                    if (wrong != null && faults.Count < 12)
                        faults.Add($"{c.id} {o.key}: {wrong}: {line}");
                }
            Assert.Greater(locks, 200, "the cards' locks were not found");
            Assert.IsEmpty(faults, "locks that show the makers' words:\n" + string.Join("\n", faults));
            // with no names at hand (a test of the rules alone) a lock is described by its parts, as it always was
            Cond.NameOf = null;
            Assert.AreEqual("Only after: ch black stones 3.", Cond.Parse("flag.ch_black_stones_3", words).Describe(words));
        }

        [Test]
        public void TheFirstMorningsOrderIsOfferedOnce()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ports_793.json"));
            var rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "trade_rules.json"));
            Commissions.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "commissions_793.json"));
            Markets.Clear();
            Assert.AreEqual("traiectum", rules.firstOrderPort);
            Assert.AreEqual("rhenish_wine", rules.firstOrderGood);
            Assert.AreEqual(2, rules.firstOrderUnits);
            Assert.AreEqual(10.0, rules.firstOrderDays);
            Trade.Begin(rules, 660f, 0.0);
            Assert.IsFalse(FirstJobs.FirstOrder(out _, out _, out _, out _, out _), "an order before the first morning");
            FirstJobs.Begin("dorestad", "frisian", 793);
            Assert.IsFalse(FirstJobs.FirstOrder(out _, out _, out _, out _, out _), "an order for wine she does not carry");
            Trade.Hold.Add(GameData.Good("rhenish_wine"), 4, 0.0, 0.0);
            Assert.IsTrue(FirstJobs.FirstOrder(out var port, out var good, out int units, out double days, out _));
            Assert.AreEqual("traiectum", port.id);
            Assert.AreEqual("rhenish_wine", good.id);
            Assert.AreEqual(2, units);
            Assert.AreEqual(0, Commissions.All.Count, "asking whether there is one made one");
            var order = FirstJobs.OfferFirstOrder();
            Assert.IsNotNull(order);
            Assert.IsTrue(order.offered && !order.mine, "the first order is taken for her");
            Assert.AreEqual(10.0, order.deadline - order.posted, 1e-9);
            Assert.Greater(order.each, 0.0);
            Assert.AreEqual(1, Commissions.OfferedToYou.Count);
            Assert.IsNull(PageWords.Fault(Commissions.Describe(order)), Commissions.Describe(order));
            // once a game
            Assert.IsTrue(FirstJobs.OrderOffered);
            Assert.IsNull(FirstJobs.OfferFirstOrder(), "the first order was offered twice");
            Assert.IsFalse(FirstJobs.FirstOrder(out _, out _, out _, out _, out _));
            // a home that is the port itself has no such order
            FirstJobs.Begin("traiectum", "frisian", 793);
            Assert.IsFalse(FirstJobs.FirstOrder(out _, out _, out _, out _, out _), "wine for Traiectum is ordered of a boat that lies at Traiectum");
        }
    }
}
