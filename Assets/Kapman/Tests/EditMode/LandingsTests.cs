using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE LANDINGS AND THE FIRST JOBS' LIST (the critic's play of 8 Oct 2026, B1 and B4, taken up by the user): each
    /// time she makes fast after a passage a slip is kept and closed when she casts off again; lying at a port when the
    /// first look comes is no landing; out and straight back in is none; the slip's words; what she still wants before
    /// she can put out, as the list's last row says it.
    /// </summary>
    public class LandingsTests
    {
        [SetUp]
        public void Clear() => Landings.Clear();

        [TearDown]
        public void Done() => Landings.Clear();

        static Landings.Figures Of(double silver, double cost, double fetch, double debt = 1920.0, double daysLeft = 150.0) =>
            new Landings.Figures { date = "1 May", silver = silver, cost = cost, fetch = fetch, debt = debt, daysLeft = daysLeft };

        [Test]
        public void LyingAtHomeWhenTheGameBeginsIsNoLanding()
        {
            Assert.IsNull(Landings.Look("aaa", 0.3, 86.0, () => Of(86, 0, 0)));
            Assert.IsNull(Landings.Look("aaa", 0.4, 86.0, () => Of(86, 0, 0)));
            Assert.AreEqual(0, Landings.All.Count);
            Assert.AreEqual("aaa", Landings.Lying);
            Assert.IsNull(Landings.Here, "no landing was made, so there is none she lies at");
        }

        [Test]
        public void APassageEndsInASlipAndCastingOffClosesIt()
        {
            Landings.Look("aaa", 0.3, 86.0, null);
            Assert.IsNull(Landings.Look("", 0.5, 12.0, null), "casting off is no landing");
            Assert.AreEqual("", Landings.Lying);
            Assert.IsNull(Landings.Look(null, 0.8, 12.0, null));
            int asked = 0;
            var made = Landings.Look("bbb", 1.0, 12.0, () =>
            {
                asked++;
                return Of(12, 50, 122);
            });
            Assert.IsNotNull(made, "she made fast at another port and no slip was kept");
            Assert.AreEqual(1, asked, "the figures are asked once, at the landing");
            Assert.AreEqual("aaa", made.from);
            Assert.AreEqual("bbb", made.to);
            Assert.AreEqual(0.5, made.Took, 1e-9);
            Assert.AreEqual(12.0, made.silver);
            Assert.AreEqual(50.0, made.cost);
            Assert.AreEqual(122.0, made.fetch);
            Assert.Less(made.away, 0.0, "she lies there still");
            Assert.AreSame(made, Landings.Here);
            Assert.IsNull(Landings.Look("bbb", 1.2, 100.0, () => Of(0, 0, 0)), "lying on is no second landing");
            // she casts off again with what the stay brought
            Landings.Look("", 2.0, 152.0, null);
            Assert.AreEqual(152.0, made.away, "the slip was not closed with the purse she left with");
            Assert.IsNull(Landings.Here);
            var next = Landings.Look("ccc", 4.5, 150.0, () => Of(150, 0, 0));
            Assert.AreEqual("bbb", next.from);
            Assert.AreEqual(2.5, next.Took, 1e-9);
            Assert.AreEqual(2, Landings.All.Count);
        }

        [Test]
        public void OutAndStraightBackInIsNoPassage()
        {
            Landings.Look("aaa", 0.3, 86.0, null);
            Landings.Look("", 0.5, 80.0, null);
            var first = Landings.Look("bbb", 1.0, 80.0, () => Of(80, 0, 0));
            Landings.Look("", 1.5, 90.0, null);
            Assert.AreEqual(90.0, first.away);
            // lying to off the quay and made fast again within the hour: the same stay
            Assert.IsNull(Landings.Look("bbb", 1.5 + Landings.NoPassage * 0.5, 90.0, () => Of(90, 0, 0)));
            Assert.AreEqual(1, Landings.All.Count);
            Assert.Less(first.away, 0.0, "the stay goes on: it is not closed");
            // out for two days fishing and back to the same port: a passage of its own
            Landings.Look("", 2.0, 90.0, null);
            var back = Landings.Look("bbb", 4.0, 85.0, () => Of(85, 0, 0));
            Assert.IsNotNull(back);
            Assert.AreEqual("bbb", back.from);
            Assert.AreEqual("bbb", back.to);
        }

        [Test]
        public void ANewGameForgetsWhereTheLastOneLay()
        {
            Landings.Look("aaa", 0.3, 86.0, null);
            Landings.Look("", 0.5, 80.0, null);
            Landings.Look("bbb", 1.0, 80.0, () => Of(80, 0, 0));
            Landings.Clear();
            Assert.AreEqual(0, Landings.All.Count);
            Assert.IsNull(Landings.Look("ccc", 0.3, 86.0, () => Of(86, 0, 0)), "the first look of a new game made a landing out of the game before");
        }

        [Test]
        public void TheSlipSaysThePassageThePurseTheCargoAndTheDebt()
        {
            var l = new Landings.Landing { from = "Aaa", to = "Bbb", leftDay = 0.5, day = 1.0, silver = 12, cost = 50, fetch = 122, debt = 1920, daysLeft = 150 };
            Assert.AreEqual("Aaa to Bbb, half a day. The purse holds 12 d. What she carries cost 50 d and would fetch about 122 d here. Owed 1920 d, with 150 days left.",
                            FrameGame.LandingWords(l));
            // what came with the boat cost her nothing: only what it would fetch is said
            l.cost = 0;
            l.leftDay = -1.5;
            Assert.AreEqual("Aaa to Bbb, 2 and a half days. The purse holds 12 d. What she carries would fetch about 122 d here. Owed 1920 d, with 150 days left.",
                            FrameGame.LandingWords(l));
            // an empty hold, out and back, the debt paid
            var empty = new Landings.Landing { from = "Bbb", to = "Bbb", leftDay = 3.0, day = 5.0, silver = 300 };
            Assert.AreEqual("Out from Bbb and back, 2 days. The purse holds 300 d and there is nothing aboard to sell.", FrameGame.LandingWords(empty));
            // where she came from is not known (a game that began at sea)
            var first = new Landings.Landing { to = "Bbb", day = 5.0, silver = 30, debt = 100, daysLeft = 1 };
            Assert.AreEqual("Made fast at Bbb. The purse holds 30 d and there is nothing aboard to sell. Owed 100 d, with 1 day left.", FrameGame.LandingWords(first));
            first.daysLeft = 0;
            StringAssert.EndsWith("Owed 100 d, and the day has come.", FrameGame.LandingWords(first));
            // (the log reads as sentences: no colon and no bracket in what a slip says)
            foreach (string said in new[] { FrameGame.LandingWords(l), FrameGame.LandingWords(empty), FrameGame.LandingWords(first) })
            {
                StringAssert.DoesNotContain(":", said);
                StringAssert.DoesNotContain("(", said);
                Assert.IsTrue(said.EndsWith("."), said);
            }
            Assert.AreEqual("", FrameGame.LandingWords(null));
        }

        [Test]
        public void RavnSpeaksOnlyAtTheFirstLandingOfAGameBegunThroughTheOpening()
        {
            var l = new Landings.Landing { from = "Aaa", to = "Bbb", cost = 50, fetch = 122 };
            // (no first jobs were set: the game did not begin through the opening, and Ravn is not aboard to say it)
            if (FirstJobs.Port.Length == 0)
                Assert.AreEqual("", FrameGame.RavnAtLanding(l, 1));
            Assert.AreEqual("", FrameGame.RavnAtLanding(l, 2), "he says it once, at the first landing");
            Assert.AreEqual("", FrameGame.RavnAtLanding(null, 1));
        }

        [Test]
        public void WhatSheStillWantsIsTheCourseWhenTheJobsAreDone()
        {
            // (with no first jobs set she wants only a course)
            if (FirstJobs.Active)
                Assert.Ignore("a game's first jobs are set: this test is of the state with none");
            CollectionAssert.AreEqual(new[] { "a course" }, FirstJobs.Wanting(false));
            Assert.AreEqual(0, FirstJobs.Wanting(true).Count);
            Assert.IsFalse(FirstJobs.Listed, "the list stands with no first jobs set");
        }
    }
}
