using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>Numbers as they are said aloud (Spoken): what a person says is in words, never "12 d".</summary>
    public class SpokenTests
    {
        [Test]
        public void NumbersAreSaidInWords()
        {
            Assert.AreEqual("no", Spoken.Number(0));
            Assert.AreEqual("four", Spoken.Number(4));
            Assert.AreEqual("twelve", Spoken.Number(12.2));
            Assert.AreEqual("nineteen", Spoken.Number(19));
            Assert.AreEqual("twenty", Spoken.Number(20));
            Assert.AreEqual("forty-two", Spoken.Number(42));
            Assert.AreEqual("a hundred", Spoken.Number(100));
            Assert.AreEqual("a hundred and twenty", Spoken.Number(120));
            Assert.AreEqual("three hundred and seven", Spoken.Number(307));
            Assert.AreEqual("1,920", Spoken.Number(1920), "from a thousand up it is figures");
        }

        [Test]
        public void PenniesAreSaidInWords()
        {
            Assert.AreEqual("twelve pennies", Spoken.Pennies(12));
            Assert.AreEqual("one penny", Spoken.Pennies(1));
            Assert.AreEqual("Twelve pennies", Spoken.Capital(Spoken.Pennies(12)));
            Assert.AreEqual("", Spoken.Capital(""));
        }

        [Test]
        public void TheLenderSaysHisBynameOnceAndWhatItMeans()
        {
            // (the user, twice: "I don't know what that other word means"; by the picker: "Keep it, said once")
            int said = 0;
            foreach (string line in Showtime.LenderLines)
                if (line.Contains("{byname}"))
                {
                    said++;
                    StringAssert.Contains("{byname}, which means the usurer", line, "his byname is not followed by what it means");
                }
            Assert.AreEqual(1, said, "his byname is said once in his lines");
            foreach (string line in Showtime.LenderLines)
            {
                StringAssert.DoesNotContain(", mind", line, "\"mind\" is Ravn's word");
                // (9 Oct 2026, the user of all the dialogue: "It does this weird. Full stop. Thing." No line of his is made of
                // fragments: a sentence of his has five words or more)
                foreach (string sentence in line.Split(new[] { ". ", "? ", "! " }, System.StringSplitOptions.RemoveEmptyEntries))
                    Assert.GreaterOrEqual(sentence.Split(' ').Length, 5, "the lender speaks in fragments again: \"" + sentence + "\" in \"" + line + "\"");
            }
        }
    }
}
