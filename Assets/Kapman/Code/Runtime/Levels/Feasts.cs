namespace Kapman.Levels
{
    /// <summary>
    /// THE FEAST DAYS - the Church's year as a Frankish emporium kept it in the 790s (Julian calendar), with Easter worked
    /// out by the Julian computus and the feasts that hang on it. Each says whether fires burn on its eve (the midsummer
    /// fires of St John's Eve) and whether bells are rung. Which customs Dorestad itself kept is still to check.
    /// </summary>
    public static class Feasts
    {
        public struct Feast
        {
            public string name;
            public bool fires;
            public bool bells;
        }

        /// <summary>The feast on a day (month 1-12, day 1-31), or null.</summary>
        public static Feast? On(int year, int month, int day)
        {
            int em, ed;
            Easter(year, out em, out ed);
            int easter = DayOfYear(year, em, ed), today = DayOfYear(year, month, day);
            int d = today - easter;
            if (d == -2) return New("Good Friday", false, false);
            if (d == 0) return New("Easter", false, true);
            if (d == 39) return New("Ascension Day", false, true);
            if (d == 49) return New("Pentecost", false, true);
            switch (month * 100 + day)
            {
                case 101: return New("New Year's Day (the Circumcision)", false, true);
                case 106: return New("Epiphany", false, true);
                case 202: return New("Candlemas", false, true);
                case 623: return New("St John's Eve: the midsummer fires", true, false);
                case 624: return New("St John's Day", false, true);
                case 629: return New("Sts Peter and Paul", false, true);
                case 815: return New("The Assumption", false, true);
                case 929: return New("Michaelmas", false, true);
                case 1101: return New("All Saints", false, true);
                case 1111: return New("Martinmas", true, true);
                case 1224: return New("Christmas Eve", true, false);
                case 1225: return New("Christmas", false, true);
            }
            return null;
        }

        static Feast New(string name, bool fires, bool bells) => new Feast { name = name, fires = fires, bells = bells };

        /// <summary>Easter Sunday on the Julian calendar (Meeus' Julian computus).</summary>
        public static void Easter(int year, out int month, out int day)
        {
            int a = year % 4, b = year % 7, c = year % 19;
            int d = (19 * c + 15) % 30;
            int e = (2 * a + 4 * b - d + 34) % 7;
            month = (d + e + 114) / 31;
            day = (d + e + 114) % 31 + 1;
        }

        static readonly int[] Starts = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };

        static int DayOfYear(int year, int month, int day) => Starts[month - 1] + day + (month > 2 && year % 4 == 0 ? 1 : 0);
    }
}
