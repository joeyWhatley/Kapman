using System;

namespace Kapman.World
{
    /// <summary>A day of the Julian calendar (the calendar of 793), with the hour.</summary>
    public readonly struct GameDate
    {
        public readonly int year, dayOfYear, month, day;
        public readonly double hour;

        public GameDate(int year, int dayOfYear, int month, int day, double hour)
        {
            this.year = year;
            this.dayOfYear = dayOfYear;
            this.month = month;
            this.day = day;
            this.hour = hour;
        }

        public string MonthName => Calendar.MonthNames[month - 1];
        public override string ToString() => $"{day} {MonthName} {year}";
    }

    /// <summary>
    /// THE CALENDAR - the Julian calendar the year 793 was reckoned in: months of their real lengths, a leap year every
    /// fourth year (796, 800...), and the year turning over on 1 January. The game counts hours from its start; this
    /// turns them into dates, so laws by year (779, 805, 864), ports' first and last years and the seasons follow time.
    /// </summary>
    public static class Calendar
    {
        public static readonly string[] MonthNames =
            { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static readonly int[] MonthDays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static bool Leap(int year) => year % 4 == 0;
        public static int DaysIn(int year) => Leap(year) ? 366 : 365;
        public static int DaysIn(int year, int month) => month == 2 && Leap(year) ? 29 : MonthDays[month - 1];

        /// <summary>The date `hours` game hours after the start of day `startDay` of `startYear` (hours from midnight).</summary>
        public static GameDate At(int startYear, int startDay, double hours)
        {
            int year = startYear;
            long day = (long)Math.Floor(hours / 24.0) + (startDay - 1);         // days since 1 January of startYear
            double hour = hours - Math.Floor(hours / 24.0) * 24.0;
            while (day < 0)
                day += DaysIn(--year);
            while (day >= DaysIn(year))
                day -= DaysIn(year++);
            int doy = (int)day + 1, d = doy, m = 1;
            while (m < 12 && d > DaysIn(year, m))
                d -= DaysIn(year, m++);
            return new GameDate(year, doy, m, d, hour);
        }

        /// <summary>The month (1-12) of a day of the year.</summary>
        public static int MonthOf(int year, int dayOfYear) => At(year, dayOfYear, 0.0).month;

        /// <summary>The day of the week of a Julian day (LevelClock.JulianDay), 0 = Sunday .. 6 = Saturday: the standard
        /// rule floor(JD + 1.5) mod 7 (the days of the week ran on unbroken through the change of calendars).</summary>
        public static int Weekday(double julianDay)
        {
            long n = (long)Math.Floor(julianDay + 1.5) % 7;
            return (int)(n < 0 ? n + 7 : n);
        }
    }
}
