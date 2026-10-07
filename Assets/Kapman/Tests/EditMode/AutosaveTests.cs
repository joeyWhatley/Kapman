using System.IO;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE GAME SAVING ITSELF (7 Oct 2026): the last three are kept, the oldest goes only when a newer one is there,
    /// the saves the player asked for are never touched, and nothing is written while the game cannot be saved.
    /// (That she is saved when she moors and when the game is quit is played on the chart: ChartGameTests.)
    /// </summary>
    public class AutosaveTests
    {
        const string Game = ".kapman.json.gz", Info = ".kapman.info";
        string folder;

        [SetUp]
        public void Make()
        {
            folder = Path.Combine(Path.GetTempPath(), "kapman_autosave_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void Done()
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        void Put(string slot, string what, bool info = true)
        {
            File.WriteAllText(Path.Combine(folder, slot + Game), what);
            if (info)
                File.WriteAllText(Path.Combine(folder, slot + Info), what + " info");
        }

        string Read(string slot) => File.Exists(Path.Combine(folder, slot + Game)) ? File.ReadAllText(Path.Combine(folder, slot + Game)) : null;
        string ReadInfo(string slot) => File.Exists(Path.Combine(folder, slot + Info)) ? File.ReadAllText(Path.Combine(folder, slot + Info)) : null;

        void Autosaved(string what, bool info = true)
        {
            Put(Autosave.Fresh, what, info);
            Autosave.Shift(folder);
        }

        [Test]
        public void TheLastThreeAreKeptNewestFirst()
        {
            Autosaved("a");
            Assert.AreEqual("a", Read("autosave 1"));
            Assert.IsNull(Read("autosave 2"));
            Autosaved("b");
            Autosaved("c");
            Assert.AreEqual("c", Read("autosave 1"));
            Assert.AreEqual("b", Read("autosave 2"));
            Assert.AreEqual("a", Read("autosave 3"));
            Autosaved("d");
            Autosaved("e");
            Assert.AreEqual("e", Read("autosave 1"));
            Assert.AreEqual("d", Read("autosave 2"));
            Assert.AreEqual("c", Read("autosave 3"));
            Assert.AreEqual("e info", ReadInfo("autosave 1"), "the list's line moves with its game");
            Assert.AreEqual("c info", ReadInfo("autosave 3"));
            Assert.IsNull(Read(Autosave.Fresh), "the fresh one is moved, not left");
            Assert.AreEqual(3, Directory.GetFiles(folder, "*" + Game).Length, "three and no more");
        }

        [Test]
        public void WithNoFreshSaveNothingIsDropped()
        {
            Autosaved("a");
            Autosaved("b");
            Autosaved("c");
            Autosave.Shift(folder);                                      // (a save that failed: there is no fresh file)
            Autosave.Shift(folder);
            Assert.AreEqual("c", Read("autosave 1"));
            Assert.AreEqual("b", Read("autosave 2"));
            Assert.AreEqual("a", Read("autosave 3"));
        }

        [Test]
        public void TheSavesShesAskedForAreNotTouched()
        {
            Put("quick", "hers");
            Put("save 1", "hers too");
            for (int i = 0; i < 6; i++)
                Autosaved("auto " + i);
            Assert.AreEqual("hers", Read("quick"));
            Assert.AreEqual("hers too", Read("save 1"));
            Assert.AreEqual("hers info", ReadInfo("quick"));
            Assert.IsTrue(Autosave.IsAutosave("autosave 2"));
            Assert.IsFalse(Autosave.IsAutosave("quick"));
            Assert.IsFalse(Autosave.IsAutosave("save 1"));
        }

        [Test]
        public void AGameWithNoLineForTheListDoesNotKeepAnOlderGamesLine()
        {
            Autosaved("a");
            Autosaved("b", info: false);                                 // (its line could not be written)
            Assert.AreEqual("b", Read("autosave 1"));
            Assert.IsNull(ReadInfo("autosave 1"), "the newest has no line: it must not show the older game's");
            Assert.AreEqual("a info", ReadInfo("autosave 2"));
        }

        [Test]
        public void ACrashBetweenTheSaveAndTheShuffleLosesNothing()
        {
            Autosaved("a");
            Autosaved("b");
            Put(Autosave.Fresh, "c");                                    // (written whole, and then the game died before the shuffle)
            // the next autosave finds it, and takes it in before it writes its own (Autosave.Now does this first)
            Autosave.Shift(folder);
            Assert.AreEqual("c", Read("autosave 1"), "the save left by the crash is whole: it is kept");
            Assert.AreEqual("b", Read("autosave 2"));
            Assert.AreEqual("a", Read("autosave 3"));
            Autosaved("d");
            Assert.AreEqual("d", Read("autosave 1"));
            Assert.AreEqual("c", Read("autosave 2"));
            Assert.AreEqual("b", Read("autosave 3"));
        }

        [Test]
        public void NothingIsSavedWhileTheGameCannotBe()
        {
            // (EditMode: no game is running, so the game says why it cannot be saved: the same gate as a meeting or
            // being aground. And a headless run never writes among the player's saves.)
            Assert.IsNotNull(SaveGame.CannotSave());
            Assert.IsNotNull(Autosave.Refused());
            string fresh = Path.Combine(SaveGame.Folder, Autosave.Fresh + Game);
            bool was = File.Exists(fresh);
            int made = Autosave.Made;
            Assert.IsFalse(Autosave.Now());
            Autosave.Quitting();
            Autosave.Moored();
            Assert.IsTrue(Autosave.Held, "a mooring's save that could not be made is held");
            Autosave.Tick();
            Assert.IsTrue(Autosave.Held);
            Autosave.CastOff();
            Assert.IsFalse(Autosave.Held, "cast off: the held save is let go");
            Assert.AreEqual(made, Autosave.Made);
            Assert.AreEqual(was, File.Exists(fresh), "no file written while the game cannot be saved");
        }
    }
}
