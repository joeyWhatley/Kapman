using System.Collections.Generic;
using Kapman.Encounters;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The log reads as sentences (DECISIONS 7 Oct 2026, item 39): what a card's choice is told as, and the feelings of
    /// a card gathered into one sentence with no inner word in brackets.
    /// </summary>
    public class LogProseTests
    {
        [Test]
        public void WhatThePeopleRememberIsToldAsWhatYouDid()
        {
            Assert.AreEqual("You sat low and listened.", LogProse.YouDid("sat low and listened"));
            Assert.AreEqual("You were hurt at the west gate on the feast.", LogProse.YouDid("was hurt at the west gate on the feast"));
            Assert.AreEqual("You paid the debt for the boat on the day.", LogProse.YouDid(" paid the debt for the boat on the day. "));
            Assert.AreEqual("", LogProse.YouDid(""));
            Assert.AreEqual("", LogProse.YouDid(null));
        }

        [Test]
        public void AChoiceWithNoMemoryLineIsToldByItsButton()
        {
            Assert.AreEqual("Your answer was \"Walk on\".", LogProse.Answered("Walk on"));
            Assert.AreEqual("Your answer was \"Thank him and take no favours\".", LogProse.Answered("Thank him and take no favours."));
            Assert.AreEqual("", LogProse.Answered("  "));
        }

        [Test]
        public void FeelingsAreOneSentenceWithNoBrackets()
        {
            var felt = new List<(string name, double n)> { ("Tofi", 4.0), ("Bernric", -3.0) };
            Assert.AreEqual("Tofi thinks better of you for it; Bernric thinks worse.", LogProse.Felt(felt));
            Assert.AreEqual("Tofi thinks better of you for it.", LogProse.Felt(new List<(string name, double n)> { ("Tofi", 2.0) }));
            Assert.AreEqual("Tofi and Ada think better of you for it.", LogProse.Felt(new List<(string name, double n)> { ("Tofi", 2.0), ("Ada", 1.0) }));
            Assert.AreEqual("Tofi, Ada and Hartwin think worse of you for it.",
                LogProse.Felt(new List<(string name, double n)> { ("Tofi", -2.0), ("Ada", -1.0), ("Hartwin", -6.0) }));
            // one person moved both ways counts once, by the sum; nobody moved says nothing
            Assert.AreEqual("Tofi thinks worse of you for it.", LogProse.Felt(new List<(string name, double n)> { ("Tofi", 2.0), ("Tofi", -5.0) }));
            Assert.AreEqual("", LogProse.Felt(new List<(string name, double n)> { ("Tofi", 2.0), ("Tofi", -2.0) }));
            Assert.AreEqual("", LogProse.Felt(new List<(string name, double n)>()));
            foreach (string s in new[] { LogProse.Felt(felt), LogProse.YouDid("took the order"), LogProse.Answered("Go on") })
            {
                StringAssert.DoesNotContain("(", s);
                Assert.IsTrue(s.EndsWith("."), s);
            }
        }

        [Test]
        public void ACardsResultLeadsWithWhatYouDid()
        {
            var r = new ChoiceResult { ok = true, told = "You sat low and listened." };
            r.facts.Add("You have a text from Torhtred in the chest now.");
            r.facts.Add("The crew are in better heart.");
            Assert.AreEqual("You sat low and listened. You have a text from Torhtred in the chest now. The crew are in better heart.", r.Text);
            StringAssert.DoesNotContain(": ", r.Text);
        }
    }
}
