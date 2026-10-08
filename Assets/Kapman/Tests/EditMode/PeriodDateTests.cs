using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The date as it was kept in 793 (PeriodDate): the Kalends, the Nones and the Ides, the days counted down
    /// to them with both ends counted, and a feast named in the reckoning's place.</summary>
    public class PeriodDateTests
    {
        [Test]
        public void TheDaysAreReckonedTheRomanWay()
        {
            Assert.AreEqual("Kalends of May", PeriodDate.Reckoned(793, 5, 1));
            Assert.AreEqual("vi Nones of May", PeriodDate.Reckoned(793, 5, 2), "May's Nones are the 7th: the 2nd is the sixth day before them, both counted");
            Assert.AreEqual("Eve of the Nones of May", PeriodDate.Reckoned(793, 5, 6));
            Assert.AreEqual("Nones of May", PeriodDate.Reckoned(793, 5, 7));
            Assert.AreEqual("viii Ides of May", PeriodDate.Reckoned(793, 5, 8));
            Assert.AreEqual("Ides of May", PeriodDate.Reckoned(793, 5, 15));
            Assert.AreEqual("xvii Kalends of June", PeriodDate.Reckoned(793, 5, 16));
            Assert.AreEqual("Eve of the Kalends of June", PeriodDate.Reckoned(793, 5, 31));
            // a short month: the Nones the 5th, the Ides the 13th
            Assert.AreEqual("Nones of June", PeriodDate.Reckoned(793, 6, 5));
            Assert.AreEqual("vi Ides of June", PeriodDate.Reckoned(793, 6, 8), "the day Lindisfarne was sacked: 8 June 793");
            Assert.AreEqual("Ides of June", PeriodDate.Reckoned(793, 6, 13));
            Assert.AreEqual("iii Kalends of October", PeriodDate.Reckoned(793, 9, 29));
            // the year's end counts to January, and February to March in a leap year and a plain one
            Assert.AreEqual("Eve of the Kalends of January", PeriodDate.Reckoned(793, 12, 31));
            Assert.AreEqual("Eve of the Kalends of March", PeriodDate.Reckoned(793, 2, 28));
            Assert.AreEqual("iii Kalends of March", PeriodDate.Reckoned(796, 2, 28), "796 is a leap year");
        }

        [Test]
        public void AFeastIsNamedInItsPlace()
        {
            Assert.AreEqual("Michaelmas", PeriodDate.Feast(793, 9, 29));
            Assert.AreEqual("", PeriodDate.Feast(793, 5, 2));
            Assert.AreEqual("St John's Eve", PeriodDate.Feast(793, 6, 23), "the long name is cut to the feast");
        }
    }
}
