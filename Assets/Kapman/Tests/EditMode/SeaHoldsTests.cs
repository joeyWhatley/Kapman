using System;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// NOTHING HOLDS HER FOR EVER (7 Oct 2026, Design/fix_sea_meetings.md): a ship met, a band of raiders and the
    /// king's men each hold the voyage until they are answered. Each has a time of its own in game hours of running
    /// clock, after which it is ended for her; and one whose talk is not on screen is let go in a few real seconds.
    /// This is the arithmetic of it (HoldWatch); the chart's play test brings a guard boat and an event on for real.
    /// </summary>
    public class SeaHoldsTests
    {
        static readonly SeaHold[] All = (SeaHold[])Enum.GetValues(typeof(SeaHold));

        [Test]
        public void On_the_chart_she_is_never_away_from_the_sea()
        {
            // (no map in an EditMode test: nothing is "at the map table". The chart's own case is in the play test.)
            Assert.IsFalse(MapView.AwayFromSea);
        }

        [Test]
        public void Every_hold_has_a_time_of_its_own()
        {
            foreach (var kind in All)
            {
                if (kind == SeaHold.None)
                {
                    Assert.IsTrue(double.IsInfinity(SeaHolds.LimitHours(kind)), "nothing holds her: no time runs");
                    continue;
                }
                double limit = SeaHolds.LimitHours(kind);
                Assert.Greater(limit, 0.0, kind + " has no time");
                Assert.LessOrEqual(limit, 48.0, kind + " may hold her longer than two days of running clock");
            }
            Assert.AreEqual(Patrols.PressHours, SeaHolds.LimitHours(SeaHold.GuardBoat), 1e-9, "the guard boat's time is the king's men's");
            Assert.AreEqual(Patrols.PressHours, SeaHolds.LimitHours(SeaHold.PortSearch), 1e-9);
        }

        [Test]
        public void With_the_clock_running_each_hold_ends_when_its_time_is_up()
        {
            foreach (var kind in All)
            {
                if (kind == SeaHold.None)
                    continue;
                var watch = new HoldWatch();
                double limit = SeaHolds.LimitHours(kind);
                const double step = 1.0 / 60.0;                             // a game minute a look
                int looks = 0;
                var verdict = HoldWatch.Verdict.Wait;
                while (verdict == HoldWatch.Verdict.Wait && looks < 100000)
                {
                    verdict = watch.Look(kind, true, step, 1f / 30f, limit);    // (its talk is up the whole time)
                    looks++;
                }
                Assert.AreEqual(HoldWatch.Verdict.TimeUp, verdict, kind + " never ended with the clock running");
                Assert.AreEqual(limit, looks * step, 2 * step, kind + " ended at the wrong hour");
            }
        }

        [Test]
        public void With_the_clock_held_no_time_passes()
        {
            // (a talk that holds the clock waits for the answer: it must not answer itself meanwhile)
            var watch = new HoldWatch();
            for (int i = 0; i < 100000; i++)
                Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.GuardBoat, true, 0.0, 1f / 30f, SeaHolds.LimitHours(SeaHold.GuardBoat)));
            Assert.AreEqual(0.0, watch.Hours, 1e-12);
        }

        [Test]
        public void A_hold_with_no_talk_on_screen_is_let_go_in_seconds()
        {
            foreach (var kind in All)
            {
                if (!HoldWatch.NeedsTalk(kind))
                    continue;
                var watch = new HoldWatch();
                float seconds = 0f;
                var verdict = HoldWatch.Verdict.Wait;
                // (the clock stands still: this is the case that held her for ever, a stage set and nothing to answer)
                while (verdict == HoldWatch.Verdict.Wait && seconds < 60f)
                {
                    verdict = watch.Look(kind, false, 0.0, 0.25f, SeaHolds.LimitHours(kind));
                    seconds += 0.25f;
                }
                Assert.AreEqual(HoldWatch.Verdict.Lost, verdict, kind + " held her with nothing to answer");
                Assert.LessOrEqual(seconds, HoldWatch.LostSeconds + 0.5f, kind + " took too long to be let go");
            }
        }

        [Test]
        public void A_talk_that_comes_back_starts_the_seconds_again()
        {
            var watch = new HoldWatch();
            double limit = SeaHolds.LimitHours(SeaHold.RaidersAlongside);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.RaidersAlongside, false, 0.0, 0.25f, limit));   // 2.5 s with no talk
            Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.RaidersAlongside, true, 0.0, 0.25f, limit));        // it is up again
            Assert.AreEqual(0f, watch.Unseen, 1e-6f);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.RaidersAlongside, false, 0.0, 0.25f, limit));
        }

        [Test]
        public void Closing_with_a_ship_needs_no_talk_and_ends_by_its_hours()
        {
            var watch = new HoldWatch();
            double limit = SeaHolds.LimitHours(SeaHold.TraderClosing);
            Assert.IsFalse(HoldWatch.NeedsTalk(SeaHold.TraderClosing));
            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.TraderClosing, false, 0.0, 0.25f, limit), "she sails to her with no window up: that is not a fault");
            Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.TraderClosing, false, limit - 0.5, 0.25f, limit));
            Assert.AreEqual(HoldWatch.Verdict.TimeUp, watch.Look(SeaHold.TraderClosing, false, 1.0, 0.25f, limit));
        }

        [Test]
        public void A_new_hold_starts_its_own_time()
        {
            var watch = new HoldWatch();
            watch.Look(SeaHold.RaidersComing, true, 1.5, 0.1f, 2.0);
            Assert.AreEqual(1.5, watch.Hours, 1e-9);
            Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.RaidersChasing, true, 1.0, 0.1f, 48.0), "the chase is a new hold: the threat's hours are not its own");
            Assert.AreEqual(1.0, watch.Hours, 1e-9);
            Assert.AreEqual(HoldWatch.Verdict.Wait, watch.Look(SeaHold.None, false, 5.0, 5f, double.PositiveInfinity));
            Assert.AreEqual(0.0, watch.Hours, 1e-9, "nothing holds her: nothing is counted");
        }

        [Test]
        public void What_holds_her_is_read_from_the_three_meetings()
        {
            // (EditMode has no meeting, chase or search of its own; whatever another test left, reading it must not throw)
            Assert.DoesNotThrow(() => { var now = SeaHolds.Now; });
            Assert.IsTrue(Enum.IsDefined(typeof(SeaHold), SeaHolds.Now));
        }
    }
}
