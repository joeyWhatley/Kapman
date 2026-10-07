using System;

namespace Kapman.Game.Sim
{
    /// <summary>What the ladder tells, in order, as the game's time moves on.</summary>
    public interface ITickSink
    {
        /// <summary>A game minute has passed (the count of minutes since the clock's start).</summary>
        void Minute(long minute);
        /// <summary>A game hour has passed (the count of hours since the clock's start).</summary>
        void Hour(long hour);
        /// <summary>A game day has passed (the count of days since the clock's start; the day that has just begun).</summary>
        void Day(int day);
        /// <summary>The year has turned (the new year).</summary>
        void Year(int year);
        /// <summary>Time was jumped (the T, Y and U keys, a saved game, a long step): every tick in between is still
        /// raised, in order, in the same frame, after this.</summary>
        void Jumped(double fromHours, double toHours);
        /// <summary>Time went backwards (Shift with the jump keys): nothing is ticked; the systems re-read the clock.</summary>
        void Reversed(double fromHours, double toHours);
    }

    /// <summary>
    /// THE TICK LADDER - the one place the game's time is turned into minute, hour, day and year events for every system
    /// (the weather engine, the markets, the chronicle, the wildlife, the encounters), in order and exactly once, however
    /// the clock moved: a frame at 4x, a jump from the keys, a saved game. Pure C#, no scene: the game's KapmanGame feeds
    /// it the level clock's hours each frame. The calendar is Julian (every fourth year a leap year, as the chronicle's
    /// dates are): the year turns when the day of the year passes the year's length.
    /// </summary>
    public sealed class TickLadder
    {
        /// <summary>A step longer than this, in game hours, counts as a jump even without a key: the frame could not
        /// have run that long at 4x (a 4x frame of a sixtieth of a second is four game minutes).</summary>
        public const double JumpHours = 2.0;

        readonly int startYear;
        readonly int startDay;          // the day of the year (1..366) at hours 0

        public TickLadder(int startYear, int startDayOfYear)
        {
            this.startYear = startYear;
            this.startDay = Math.Max(1, startDayOfYear);
        }

        /// <summary>Days in a Julian year.</summary>
        public static int DaysInYear(int year) => year % 4 == 0 ? 366 : 365;

        /// <summary>The year on the given day (days since the start, 0 = the start day).</summary>
        public int YearOf(int day)
        {
            int year = startYear, doy = startDay + day;
            while (doy > DaysInYear(year))
            {
                doy -= DaysInYear(year);
                year++;
            }
            return year;
        }

        /// <summary>
        /// Move the ladder from one clock reading to another, raising what passed in between. Backwards raises only
        /// Reversed. A jump (asked, or a step over JumpHours) raises Jumped first, then every tick. Ticks for a moment
        /// come in the order minute, hour, day, year.
        /// </summary>
        public void Advance(double fromHours, double toHours, bool jump, ITickSink sink)
        {
            if (sink == null)
                return;
            if (toHours < fromHours)
            {
                sink.Reversed(fromHours, toHours);
                return;
            }
            if (jump || toHours - fromHours > JumpHours)
                sink.Jumped(fromHours, toHours);
            // the minutes that end in (from, to]: a minute is raised when its end is reached
            long firstMinute = (long)Math.Floor(fromHours * 60.0 + 1e-9) + 1;
            long lastMinute = (long)Math.Floor(toHours * 60.0 + 1e-9);
            for (long m = firstMinute; m <= lastMinute; m++)
            {
                sink.Minute(m);
                if (m % 60 != 0)
                    continue;
                long h = m / 60;
                sink.Hour(h);
                if (h % 24 != 0)
                    continue;
                int d = (int)(h / 24);
                sink.Day(d);
                if (YearOf(d) != YearOf(d - 1))
                    sink.Year(YearOf(d));
            }
        }
    }
}
