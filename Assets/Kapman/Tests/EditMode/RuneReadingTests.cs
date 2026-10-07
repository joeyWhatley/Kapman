using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Reading runes: one rune for several sounds, doubles carved once, n and m dropped before their stops (Guðmundr
    /// carved kuþmutr); a word reads when the chosen sounds say it.</summary>
    public class RuneReadingTests
    {
        const string Futhark = @"{ ""futhark"": [
            { ""rune"": ""ᚠ"", ""sounds"": [""f"", ""v""] }, { ""rune"": ""ᚢ"", ""sounds"": [""u"", ""o"", ""v"", ""w"", ""y""] },
            { ""rune"": ""ᚦ"", ""sounds"": [""þ"", ""ð""] }, { ""rune"": ""ᚬ"", ""sounds"": [""o"", ""a""] }, { ""rune"": ""ᚱ"", ""sounds"": [""r""] },
            { ""rune"": ""ᚴ"", ""sounds"": [""k"", ""g""] }, { ""rune"": ""ᚼ"", ""sounds"": [""h""] }, { ""rune"": ""ᚾ"", ""sounds"": [""n""] },
            { ""rune"": ""ᛁ"", ""sounds"": [""i"", ""e"", ""j""] }, { ""rune"": ""ᛅ"", ""sounds"": [""a"", ""æ"", ""e""] }, { ""rune"": ""ᛋ"", ""sounds"": [""s""] },
            { ""rune"": ""ᛏ"", ""sounds"": [""t"", ""d""] }, { ""rune"": ""ᛒ"", ""sounds"": [""b"", ""p""] }, { ""rune"": ""ᛘ"", ""sounds"": [""m""] },
            { ""rune"": ""ᛚ"", ""sounds"": [""l""] }, { ""rune"": ""ᛦ"", ""sounds"": [""r"", ""y""] } ] }";

        [SetUp]
        public void Load() => ChestData.Load(null, null, new TextAsset(Futhark));

        [TearDown]
        public void Done() => ChestData.Load(null, null, null);

        [Test]
        public void GudmundrIsReadFromKuthmutr()
        {
            var words = RuneReading.Words("ᚴᚢᚦᛘᚢᛏᛦ");
            Assert.AreEqual(1, words.Count);
            Assert.AreEqual(7, words[0].Count);
            Assert.IsTrue(RuneReading.Readable(words[0], "Guðmundr"));
            Assert.IsTrue(RuneReading.IsRead(new[] { "g", "u", "þ", "m", "u", "d", "r" }, "Guðmundr"));
            Assert.IsFalse(RuneReading.IsRead(new[] { "k", "u", "þ", "m", "u", "t", "r" }, "Guðmundr"), "k and t: not how it was said");
        }

        [Test]
        public void DoublesAreCarvedOnceAndWordsSplitAtDividers()
        {
            var words = RuneReading.Words("ᚼᛅᚱᛅᛚᛏᛦ᛫ᚴᚢᚾᚢᚴᛦ");
            Assert.AreEqual(2, words.Count);
            Assert.IsTrue(RuneReading.Readable(words[0], "Haraldr"));
            Assert.IsTrue(RuneReading.Readable(words[1], "konungr"), "kunukR: the n before g not carved");
            Assert.AreEqual(RuneReading.Canon("Gunnarr"), RuneReading.Canon("gunar"));
        }
    }
}
