using System;

namespace Kapman.Game
{
    /// <summary>
    /// NUMBERS AS THEY ARE SAID ALOUD (the script critic's report of 8 Oct 2026, taken up by the user: Ravn "says prices
    /// as '12 d', '4 d', '9 d', which nobody can say aloud"; its third change: "speak prices as 'twelve pennies', never
    /// '12 d'"). For the words people say. The figures of the bar, the windows and the log stay figures.
    /// </summary>
    public static class Spoken
    {
        static readonly string[] Small =
        {
            "no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen",
            "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
        };
        static readonly string[] Tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        /// <summary>A whole number in words: "twelve", "forty-two", "a hundred and twenty". From a thousand up, and
        /// under nothing, in figures.</summary>
        public static string Number(double value)
        {
            int n = (int)Math.Round(value);
            if (n < 0 || n >= 1000)
                return n.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
            if (n < 20)
                return Small[n];
            if (n < 100)
                return Tens[n / 10] + (n % 10 > 0 ? "-" + Small[n % 10] : "");
            return (n / 100 == 1 ? "a hundred" : Small[n / 100] + " hundred") + (n % 100 > 0 ? " and " + Number(n % 100) : "");
        }

        /// <summary>A sum of pennies in words: "twelve pennies", "one penny".</summary>
        public static string Pennies(double value) => (int)Math.Round(value) == 1 ? "one penny" : Number(value) + " pennies";

        /// <summary>The same words with a capital, to begin a sentence.</summary>
        public static string Capital(string words) => string.IsNullOrEmpty(words) ? "" : char.ToUpperInvariant(words[0]) + words.Substring(1);
    }
}
