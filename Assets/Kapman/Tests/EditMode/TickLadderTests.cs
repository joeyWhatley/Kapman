using System.Collections.Generic;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The tick ladder: every minute, hour, day and year raised once and in order, however the clock moved.</summary>
    public class TickLadderTests
    {
        sealed class Log : ITickSink
        {
            public readonly List<string> events = new List<string>();
            public int minutes, hours, days, years, jumps, reversals;
            public void Minute(long minute) { minutes++; events.Add("m" + minute); }
            public void Hour(long hour) { hours++; events.Add("h" + hour); }
            public void Day(int day) { days++; events.Add("d" + day); }
            public void Year(int year) { years++; events.Add("y" + year); }
            public void Jumped(double from, double to) { jumps++; events.Add("jump"); }
            public void Reversed(double from, double to) { reversals++; events.Add("back"); }
        }

        [Test]
        public void AThirtyDayJumpRaisesEveryHourAndDayInOrder()
        {
            var ladder = new TickLadder(793, 172);
            var log = new Log();
            ladder.Advance(0.0, 24.0 * 30.0, true, log);
            Assert.AreEqual(1, log.jumps);
            Assert.AreEqual("jump", log.events[0], "Jumped comes first");
            Assert.AreEqual(720, log.hours);
            Assert.AreEqual(30, log.days);
            Assert.AreEqual(43200, log.minutes);
            // order at the first day's end: the 1440th minute, then the 24th hour, then day 1
            int i = log.events.IndexOf("m1440");
            Assert.AreEqual("h24", log.events[i + 1]);
            Assert.AreEqual("d1", log.events[i + 2]);
        }

        [Test]
        public void AnHourAndAHalfAtFourTimesOverSmallFramesTicksNinetyMinutesAndOneHour()
        {
            var ladder = new TickLadder(793, 172);
            var log = new Log();
            double hours = 0.0;
            const double frame = 1.0 / 60.0;                         // real seconds
            const double gameHoursPerRealSecond = 4.0 * 60.0 / 3600.0; // 1 s = 1 game minute at 1x; 4x
            while (hours < 1.5 - 1e-9)
            {
                double next = System.Math.Min(1.5, hours + frame * gameHoursPerRealSecond);
                ladder.Advance(hours, next, false, log);
                hours = next;
            }
            Assert.AreEqual(90, log.minutes);
            Assert.AreEqual(1, log.hours);
            Assert.AreEqual(0, log.days);
            Assert.AreEqual(0, log.jumps, "small frames are no jump");
        }

        [Test]
        public void GoingBackwardsRaisesOnlyReversed()
        {
            var ladder = new TickLadder(793, 172);
            var log = new Log();
            ladder.Advance(50.0, 26.0, true, log);
            Assert.AreEqual(1, log.reversals);
            Assert.AreEqual(0, log.minutes + log.hours + log.days + log.years + log.jumps);
        }

        [Test]
        public void TheYearTurnsOnceAtTheJulianTurn()
        {
            // 793 is not a leap year: from day 172 (21 June) the year turns after 365 - 172 + 1 = 194 days
            var ladder = new TickLadder(793, 172);
            var log = new Log();
            ladder.Advance(0.0, 24.0 * 200.0, true, log);
            Assert.AreEqual(1, log.years);
            Assert.AreEqual("y794", log.events.Find(e => e.StartsWith("y")));
            int i = log.events.IndexOf("y794");
            Assert.AreEqual("d194", log.events[i - 1], "the year turns at the start of the 194th day after the start");
            // 796 is a Julian leap year: 366 days
            Assert.AreEqual(366, TickLadder.DaysInYear(796));
            Assert.AreEqual(365, TickLadder.DaysInYear(793));
            Assert.AreEqual(793, ladder.YearOf(0));
            Assert.AreEqual(794, ladder.YearOf(194));
            Assert.AreEqual(793, ladder.YearOf(193));
        }

        [Test]
        public void ALongStepWithoutAKeyIsAJumpToo()
        {
            var ladder = new TickLadder(793, 172);
            var log = new Log();
            ladder.Advance(10.0, 13.0, false, log);
            Assert.AreEqual(1, log.jumps);
            Assert.AreEqual(3, log.hours);
            ladder.Advance(13.0, 14.5, false, log);
            Assert.AreEqual(1, log.jumps, "a step under two hours is not a jump");
        }
    }
}
