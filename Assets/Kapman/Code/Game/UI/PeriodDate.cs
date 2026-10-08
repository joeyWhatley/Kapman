using Kapman.Levels;
using Kapman.World;

namespace Kapman.Game
{
    /// <summary>
    /// THE DATE AS IT WAS KEPT IN 793 (the user, 8 Oct 2026: "use period accurate date keeping"). Nobody counted the days
    /// of a month from one to thirty-one. A day was named by the feast it was, where it was one (Michaelmas, Martinmas,
    /// Easter); else it was reckoned as the Church and every clerk reckoned it, the Roman way: each month has its
    /// Kalends (the 1st), its Nones (the 5th; the 7th in March, May, July and October) and its Ides (the 13th; the 15th
    /// in those four), and a day between is counted down to the next of them, the day itself and the mark both counted.
    /// So 2 May is "vi Nones of May", 14 May "Eve of the Ides of May", and 16 May "xvii Kalends of June". The year is
    /// the year of the Lord, as Bede and the Frankish annals wrote it.
    /// This is what the frame's bar shows; the log and the windows still say "2 May 793" for now, and the bar's tip
    /// gives that too.
    /// </summary>
    public static class PeriodDate
    {
        static readonly string[] Small = { "", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x", "xi", "xii", "xiii", "xiv", "xv", "xvi", "xvii", "xviii", "xix" };

        static bool LongMonth(int month) => month == 3 || month == 5 || month == 7 || month == 10;

        /// <summary>The day in the Roman reckoning: "Kalends of May", "vi Nones of May", "Eve of the Ides of May",
        /// "xvii Kalends of June".</summary>
        public static string Reckoned(int year, int month, int day)
        {
            int nones = LongMonth(month) ? 7 : 5, ides = nones + 8;
            string name = Calendar.MonthNames[month - 1];
            if (day == 1)
                return "Kalends of " + name;
            if (day < nones)
                return Count(nones - day + 1, "Nones", name);
            if (day == nones)
                return "Nones of " + name;
            if (day < ides)
                return Count(ides - day + 1, "Ides", name);
            if (day == ides)
                return "Ides of " + name;
            return Count(Calendar.DaysIn(year, month) - day + 2, "Kalends", Calendar.MonthNames[month % 12]);
        }

        static string Count(int n, string mark, string month) =>
            n == 2 ? "Eve of the " + mark + " of " + month : (n > 0 && n < Small.Length ? Small[n] : n.ToString()) + " " + mark + " of " + month;

        /// <summary>The feast a day is, in a word or two ("Michaelmas", "St John's Eve"), or "".</summary>
        public static string Feast(int year, int month, int day)
        {
            var feast = Feasts.On(year, month, day);
            if (feast == null)
                return "";
            string name = feast.Value.name;
            int cut = name.IndexOfAny(new[] { '(', ':' });
            return (cut > 0 ? name.Substring(0, cut) : name).Trim();
        }

        /// <summary>The day as the bar says it: its feast if it is one, else as reckoned; and the year of the Lord.</summary>
        public static string Words(GameDate d)
        {
            string feast = Feast(d.year, d.month, d.day);
            return (feast.Length > 0 ? feast : Reckoned(d.year, d.month, d.day)) + ", " + d.year;
        }

        /// <summary>The day without its year ("Michaelmas", "Ides of May").</summary>
        public static string Day(GameDate d)
        {
            string feast = Feast(d.year, d.month, d.day);
            return feast.Length > 0 ? feast : Reckoned(d.year, d.month, d.day);
        }

        /// <summary>What the bar's tip says of the day: the plain count, the reckoning, and how the reckoning goes.</summary>
        public static string Tip(GameDate d)
        {
            string feast = Feast(d.year, d.month, d.day);
            return $"{d.day} {d.MonthName} {d.year}, by the plain count of days." +
                   (feast.Length > 0 ? " By the Church's reckoning: " + Reckoned(d.year, d.month, d.day) + "." : "") +
                   " A day is named by its feast, or counted down to the month's next mark: the Kalends (the 1st), the Nones (the 5th or 7th) or the Ides (the 13th or 15th), the day itself and the mark both counted.";
        }
    }
}
