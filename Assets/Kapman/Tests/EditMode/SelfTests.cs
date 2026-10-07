using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SELF (7 Oct 2026): the writers' file of peoples, faiths, pasts and stats is read, and a small table stands in
    /// when it cannot be; the three picks set the stats, the tongues and the world's colours; the record goes into a
    /// saved game and comes back; a stat rises by use, slowly, and never by points; and each pick has its line of what
    /// it changes for the creation window.
    /// </summary>
    public class SelfTests
    {
        string peopleWas;
        string folder;

        [SetUp]
        public void Up()
        {
            peopleWas = PlayerPrefs.GetString("kapman.people", "frisian");
            ContentFiles.SetRoot(null);
            Self.Reset();
        }

        [TearDown]
        public void Down()
        {
            ContentTestKit.Done(folder);
            folder = null;
            Self.Reset();
            MapPalette.SetPeople(peopleWas);
        }

        [Test]
        public void TheWritersFileIsRead()
        {
            var book = SelfBook.Current;
            Assert.IsTrue(book.FromFile, "story/self_793.json was not read: " + string.Join("; ", ContentFiles.Warnings));
            foreach (string id in new[] { "frisian", "frankish", "english", "danish" })
                Assert.IsNotNull(book.People(id), "no people " + id);
            Assert.GreaterOrEqual(book.faiths.Count, 6);
            Assert.GreaterOrEqual(book.pasts.Count, 8);
            foreach (string id in Self.StatIds)
            {
                var stat = book.Stat(id);
                Assert.IsNotNull(stat, "no stat " + id);
                Assert.AreEqual(5, stat.levels.Length, id + " has a word for each of five pips");
                Assert.IsNotEmpty(stat.Level(3));
            }
            Assert.AreEqual("dorestad", book.People("frisian").home);
            Assert.AreEqual("old_norse", book.People("danish").firstTongue);
            Assert.AreEqual(3, book.Past("toll_clerk").tilt["tongue"]);

            // the peoples a new game offers: of the first release, with their home port in the game
            ContentTestKit.LoadPorts();
            var offered = book.Offered(id => GameData.Port(id) != null).Select(p => p.id).ToList();
            CollectionAssert.Contains(offered, "frisian");
            CollectionAssert.Contains(offered, "danish");
            CollectionAssert.DoesNotContain(offered, "irish", "the Irish start waits for its waters");
            foreach (string id in offered)
                Assert.Greater(book.FaithsFor(book.People(id)).Count, 0, id + " has no faith to begin in");
            CollectionAssert.DoesNotContain(book.FaithsFor(book.People("english")).Select(f => f.id).ToList(), "old_gods");
        }

        [Test]
        public void AMissingFileGivesTheSmallTable()
        {
            folder = ContentTestKit.UseFolder();
            var book = SelfBook.Current;
            Assert.IsFalse(book.FromFile);
            Assert.IsNotNull(book.People("frisian"));
            Assert.Greater(book.pasts.Count, 0);
            foreach (string id in Self.StatIds)
                Assert.IsNotNull(book.Stat(id), "the small table has no stat " + id);
            ContentTestKit.Put(folder, SelfBook.File, "{ \"peoples\": [ { \"id\": \"frisian\" } ");      // (half written)
            ContentFiles.SetRoot(folder);
            Assert.IsFalse(SelfBook.Current.FromFile, "a half-written file must not be taken");
            Assert.IsNotNull(SelfBook.Current.Stat("troth"));
        }

        [Test]
        public void ThePicksSetTheStatsTheTonguesAndTheColours()
        {
            Assert.IsFalse(Self.Chosen, "nothing is chosen before a game asks");
            Assert.AreEqual("frisian", Self.People, "the plain start");
            Self.Begin("danish", "old_gods", "toll_clerk");
            Assert.IsTrue(Self.Chosen);
            Assert.AreEqual("danish", Self.People);
            Assert.AreEqual("old_gods", Self.Faith);
            Assert.AreEqual("toll_clerk", Self.Past);
            Assert.AreEqual(3, Self.Stat("tongue"), "the toll-keeper's clerk has Tongue 3");
            Assert.AreEqual(3, Self.Stat("troth"), "Troth begins at 3");
            Assert.AreEqual(1, Self.Stat("name"));
            Assert.AreEqual(1, Self.Stat("nerve"));
            Assert.AreEqual(1, Self.Stat("lore"));
            Assert.AreEqual(1.0, Self.TongueHeld("old_norse"), 1e-9);
            Assert.AreEqual(0.5, Self.TongueHeld("old_frisian"), 1e-9);
            Assert.AreEqual(0.0, Self.TongueHeld("irish"), 1e-9);
            Assert.AreEqual(1.0, Self.TongueHeld("norse"), 1e-9, "the cards' older name for the tongue");
            Assert.AreEqual("danish", MapPalette.Current.people, "the world takes the people's colours");

            Self.Begin("frisian", "christian", "minster_schooled");
            Assert.AreEqual(3, Self.Stat("lore"));
            Assert.AreEqual(1.0, Self.TongueHeld("latin"), 1e-9, "the minster's schooling is Latin letters");
            Assert.AreEqual("frisian", MapPalette.Current.people);

            Self.Begin("nobody", "nothing", "nowhere");
            Assert.IsTrue(Self.Chosen);
            Assert.IsNotNull(SelfBook.Current.People(Self.People), "a people the book does not have falls back to one it has");
            Assert.IsNotNull(SelfBook.Current.Faith(Self.Faith));
            Assert.AreEqual("", Self.Past);
        }

        [Test]
        public void AStatRisesByUseSlowlyAndNeverByPoints()
        {
            Self.Begin("frisian", "christian", "steersmans_child");
            Assert.AreEqual(1, Self.Stat("tongue"));
            // against someone who resisted (a written chance of 0.55 or less): three wins fill the second pip
            Assert.IsFalse(Self.Used("tongue", 0.55));
            Assert.IsFalse(Self.Used("tongue", 0.35));
            Assert.AreEqual(1, Self.Stat("tongue"));
            Assert.IsTrue(Self.Used("tongue", 0.55), "the third hard win fills the pip");
            Assert.AreEqual(2, Self.Stat("tongue"));
            // the higher it stands the slower it rises: at two pips, six hard wins
            for (int i = 0; i < 5; i++)
                Assert.IsFalse(Self.Used("tongue", 0.5), "win " + (i + 1) + " at two pips");
            Assert.IsTrue(Self.Used("tongue", 0.5));
            Assert.AreEqual(3, Self.Stat("tongue"));
            // an easy win adds less: ten at one pip
            Assert.AreEqual(2, Self.Stat("nerve"), "the steersman's child has Nerve 2");
            Self.SetStat("nerve", 1.0);
            for (int i = 0; i < 9; i++)
                Assert.IsFalse(Self.Used("nerve", 0.75));
            Assert.IsTrue(Self.Used("nerve", 0.75));
            Assert.AreEqual(2, Self.Stat("nerve"));
            // Troth follows the faith's days, not talk; a full stat rises no further; a name that is no stat does nothing
            Assert.IsFalse(Self.Used("troth", 0.35));
            Assert.AreEqual(3.0, Self.StatRaw("troth"), 1e-9);
            Self.SetStat("lore", 5.0);
            Assert.IsFalse(Self.Used("lore", 0.35));
            Assert.AreEqual(5, Self.Stat("lore"));
            Assert.IsFalse(Self.Used("luck", 0.35));
            Assert.Greater(Self.Gain(1, 0.4), Self.Gain(1, 0.8));
            Assert.Greater(Self.Gain(1, 0.4), Self.Gain(4, 0.4));
        }

        [Test]
        public void TheSelfIsKeptInTheSavedGame()
        {
            Self.Begin("english", "christian", "hall_child");
            Self.Used("tongue", 0.4);
            Self.See("Heill,");
            Self.See("heill");
            Self.MarkMet("dorestad.ravn", 3.0);
            Self.MarkMet("dorestad.ravn", 9.5);
            double tongue = Self.StatRaw("tongue");
            var saved = Json.Parse(Json.Write(Self.Capture()));
            Self.Reset();
            Assert.IsFalse(Self.Chosen);
            Assert.AreEqual(0, Self.Seen("heill"));
            Self.Restore(saved);
            Assert.IsTrue(Self.Chosen);
            Assert.AreEqual("english", Self.People);
            Assert.AreEqual("hall_child", Self.Past);
            Assert.AreEqual(3, Self.Stat("name"));
            Assert.AreEqual(tongue, Self.StatRaw("tongue"), 1e-9, "the part of a pip gathered is kept");
            Assert.AreEqual(1.0, Self.TongueHeld("old_english"), 1e-9);
            Assert.AreEqual(2, Self.Seen("heill"), "a word seen is counted without its marks or its capital");
            Assert.AreEqual(2, Self.Met("dorestad.ravn").times);
            Assert.AreEqual(3.0, Self.Met("dorestad.ravn").first, 1e-9);
            Assert.AreEqual(9.5, Self.Met("dorestad.ravn").last, 1e-9);
            Assert.AreEqual("english", MapPalette.Current.people, "a loaded game has its people's colours");

            // a save from before the self was kept: the plain start, and it is not asked who it is
            Self.Restore(null);
            Assert.IsTrue(Self.Chosen);
            Assert.AreEqual("frisian", Self.People);
            Assert.AreEqual(1, Self.Stat("name"));
        }

        [Test]
        public void EachPickHasItsLineOfWhatItChanges()
        {
            ContentTestKit.LoadPorts();
            var book = SelfBook.Current;
            string people = FrameRoot.PeopleLine(book.People("danish"));
            StringAssert.Contains(GameData.Port("ripa").name, people, "where she starts");
            StringAssert.Contains("Amber", people, "the world's colours");
            StringAssert.Contains("Norse", people, "her tongue");
            string faith = FrameRoot.FaithLine(book.Faith("christian"));
            StringAssert.Contains("It asks", faith);
            StringAssert.Contains("It opens", faith);
            string past = FrameRoot.PastLine(book, book.Past("steersmans_child"));
            StringAssert.Contains("Nerve 2", past);
            StringAssert.Contains("Lore 2", past);
            StringAssert.Contains("You keep", past);
            foreach (var p in book.pasts)
                Assert.IsNotEmpty(FrameRoot.PastLine(book, p), p.id + " has no line");
            foreach (var f in book.faiths)
                Assert.IsNotEmpty(FrameRoot.FaithLine(f), f.id + " has no line");
            var lines = new List<string> { people, faith, past };
            foreach (string line in lines)
                Assert.Less(line.Length, 420, "a line, not a page: " + line);
        }
    }
}
