# Autosave: each time she moors, and when the game is quit (7 Oct 2026)

Read, not run, not compiled. Paths under G:/kapman_unity/Kapman2/Assets/Kapman.

## What was found

- No autosave today. Callers of SaveGame.Save: KapmanGame.cs:482 (F5), MainMenu.cs:144, MapView.cs:246, PortNodes.cs:324.
- MOORING on the chart is raised in one place: `MooringWatch.Look` returns `Change.Moored`
  (Code/Game/Encounters/EncounterDirector.cs:82), passed on by `EncounterDirector.Look` (:236-244, which calls BeginCall).
  The scene calls it from `EncounterRunner.Update`, Code/Game/Encounters/EncounterRunner.cs:161 (chart) and :166
  (other levels). Both calls THROW THE RESULT AWAY today. There is no event. That return value is the hook.
  `BeginCallQuietly` (EncounterRunner.cs:189: a load, or a new game laid at home) does not go through Look, so a load
  does not autosave straight back over itself. Good, leave it.
- QUIT is seen nowhere: no OnApplicationQuit, no Application.quitting, no wantsToQuit in Code/. The only quit is
  `Application.Quit()` at Code/Game/UI/MainMenu.cs:252 (after the ask). The window's X and Alt+F4 pass unseen.
  KapmanGame.OnDestroy (KapmanGame.cs:439) is too late and also runs on a level change.
- SaveGame.Save (SaveGame.cs:75) already writes to `<slot>.part` and moves it, so a half-written file never replaces a
  good one. It returns a sentence, not a bool: success begins "Saved (".
- SaveGame.Slots (SaveGame.cs:219) lists every `*.kapman.json.gz`, newest first: the autosaves appear in the menu's
  load list (MainMenu.cs:163) with no menu change. NewSlot (:45) only makes "save N": no name clash.
- SaveGame.Busy (:53) is the gate. At the moment of mooring it may refuse (a meeting not over, TextPanel open and
  `ship.anchored` not yet true): so the autosave is HELD and tried again each frame until it goes through or she casts off.

## The design

- Slots: `autosave 1` (newest), `autosave 2`, `autosave 3`. Beside "quick" and "save N", never touching them.
- Order that cannot lose a save: write the new game to `autosave new` first; ONLY when that file is there:
  delete 3, 2 -> 3, 1 -> 2, new -> 1. A failed or refused save changes nothing on disk. A crash in the middle of the
  shuffle loses at most the oldest; a left-over `autosave new` is shuffled in at the next autosave.
- Never while Busy: Autosave asks SaveGame.Busy() itself first, and Save asks again.
- Quit: two hooks, one guard. `Application.wantsToQuit` (the built game: X, Alt+F4, Application.Quit; the scene is
  still whole) and `KapmanGame.OnApplicationQuit` (the editor's stop button, where wantsToQuit does not fire).
  If Busy at quit (mid-meeting, aground) nothing is written and the quit is NOT blocked: the last mooring autosave stands.
  Blocking the quit to ask is a design choice for her, not made here.

## Change 1: new file Code/Game/Autosave.cs

```csharp
using System;
using System.IO;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME SAVES ITSELF: each time she moors, and when the game is quit. The last three are kept ("autosave 1" the
    /// newest, 2, 3) beside the saves she asked for. Never while SaveGame.Busy: a mooring's save is held and tried again
    /// until it can be made (or she casts off); at a quit that can't be saved the last one stands.
    /// </summary>
    public static class Autosave
    {
        public const int Keep = 3;
        public const string Prefix = "autosave ";
        public const string Fresh = "autosave new";                      // written first; the old ones move only when it is there
        const string Game = ".kapman.json.gz", Info = ".kapman.info";

        /// <summary>Off for the tests and the headless runs.</summary>
        public static bool Enabled = true;

        static bool held;                                                // a mooring's save still to be made
        static float lastQuitSave = -100f;

        public static string Slot(int n) => Prefix + n;

        /// <summary>She has moored (EncounterRunner, from the watch on the moorings): save now, or as soon as it can be.</summary>
        public static void Moored()
        {
            held = true;
            Tick();
        }

        /// <summary>She has cast off with the save still held: let it go (the next mooring makes one).</summary>
        public static void CastOff() => held = false;

        /// <summary>Each frame: the held save, once nothing is in the way.</summary>
        public static void Tick()
        {
            if (held && Now())
                held = false;
        }

        /// <summary>The game is being quit: one save, if it can be made. Asked twice in one quit, it saves once.</summary>
        public static void Quitting()
        {
            if (Time.realtimeSinceStartup - lastQuitSave < 5f)
                return;
            if (Now())
                lastQuitSave = Time.realtimeSinceStartup;
            held = false;
        }

        /// <summary>Makes an autosave now; false if it couldn't be (busy, not ready, the disk): nothing is changed then.</summary>
        public static bool Now()
        {
            if (!Enabled || SaveGame.Busy() != null)
                return false;
            try
            {
                string folder = SaveGame.Folder;
                if (File.Exists(Path.Combine(folder, Fresh + Game)))
                    Shift(folder);                                       // (left by a crash between the save and the shuffle: it is whole, keep it)
                SaveGame.Save(Fresh);
                if (!File.Exists(Path.Combine(folder, Fresh + Game)))
                    return false;
                Shift(folder);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the autosave couldn't be made: " + ex.Message);
                return false;
            }
        }

        /// <summary>The shuffle, in a folder: the oldest goes, each moves down one, the fresh one becomes "autosave 1".
        /// Does nothing if there is no fresh one (so nothing is ever dropped without a newer save in its place).</summary>
        public static void Shift(string folder, int keep = Keep)
        {
            if (!File.Exists(Path.Combine(folder, Fresh + Game)))
                return;
            for (int n = keep; n >= 1; n--)
            {
                string from = n == 1 ? Fresh : Slot(n - 1);
                Move(folder, from, Slot(n), Game);
                Move(folder, from, Slot(n), Info);
            }
        }

        static void Move(string folder, string from, string to, string ending)
        {
            string a = Path.Combine(folder, from + ending), b = Path.Combine(folder, to + ending);
            if (!File.Exists(a))
                return;                                                  // (fewer than three yet: the one below stays as it is)
            if (File.Exists(b))
                File.Delete(b);
            File.Move(a, b);
        }

        // the built game: the window's X, Alt+F4, Application.Quit - before the scene is taken down
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook()
        {
            held = false;
            lastQuitSave = -100f;
            Application.wantsToQuit -= WantsToQuit;
            Application.wantsToQuit += WantsToQuit;
        }

        static bool WantsToQuit()
        {
            Quitting();
            return true;                                                 // (never stands in the way of the quit)
        }
    }
}
```

Note on Move when fewer than three exist: with only `autosave 1` on disk, the n=3 step finds no `autosave 2` and
does nothing; n=2 moves 1 -> 2; n=1 moves new -> 1. Right.
Note: File.Move keeps the file's write time, so Slots() still sorts them newest first.
Known small flaw: if a save's .info is missing but an older .info sits in the target slot, the old .info stays
beside the newer game (the list line would be stale, the game itself is right). To be strict, delete the target
.info whenever the game file moved.

## Change 2: Code/Game/Encounters/EncounterRunner.cs, lines 161 and 166

Line 161, now:
```csharp
                d.Look(p.x, p.z, sailing, sailing ? ship.Speed : 0.0, dt);
```
to:
```csharp
                Autosaved(d.Look(p.x, p.z, sailing, sailing ? ship.Speed : 0.0, dt));
```
Line 166, now:
```csharp
                d.Look(p.x, p.z, ship.Goal.HasValue, ship.Speed, Time.deltaTime);
```
to:
```csharp
                Autosaved(d.Look(p.x, p.z, ship.Goal.HasValue, ship.Speed, Time.deltaTime));
```
Straight after the if/else (before `bool blocked = ...`, now line 168) add:
```csharp
            Autosave.Tick();                                              // (a mooring's save that had to wait)
```
And a new method in the class (for instance above `void Moorings(MooringWatch watch)`, line 345):
```csharp
        /// <summary>She has moored: the game saves itself (held if something is in the way). Cast off: a held one is let go.</summary>
        static void Autosaved(MooringWatch.Change change)
        {
            if (change == MooringWatch.Change.Moored)
                Autosave.Moored();
            else if (change == MooringWatch.Change.CastOff)
                Autosave.CastOff();
        }
```
The save is made AFTER BeginCall has put the arrival cards in line (Look does that first), and before OpenNext
(:169-170) in the same frame: the save holds the call begun and its cards waiting (EncounterState "queue",
"call_port"), so a load comes back to the moment of arrival with the cards still to come. That is the right moment.

## Change 3: Code/Game/KapmanGame.cs, beside OnDestroy (line 439)

Add above `void OnDestroy()`:
```csharp
        /// <summary>The game is quit (the editor's stop too): one last autosave, if nothing is in the way.</summary>
        void OnApplicationQuit()
        {
            if (Current == this)
                Autosave.Quitting();
        }
```
OnApplicationQuit runs before any OnDisable/OnDestroy, so the world, the ship and the clock are still there.

## Change 4 (only if needed): Code/Game/UI/MainMenu.cs:252

Nothing needed as read: Application.Quit() raises wantsToQuit, which saves. Lines around 252 were not read; if the
menu tears the game down BEFORE calling Application.Quit, put `Autosave.Quitting();` as the first line of that handler.

## The EditMode test: Tests/EditMode/AutosaveTests.cs

```csharp
using System.IO;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The game saving itself: the last three are kept, the oldest goes only when a newer one is there, the
    /// saves she asked for are never touched, and nothing is written while the game can't be saved.</summary>
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

        void Put(string slot, string what)
        {
            File.WriteAllText(Path.Combine(folder, slot + Game), what);
            File.WriteAllText(Path.Combine(folder, slot + Info), what + " info");
        }

        string Read(string slot) => File.Exists(Path.Combine(folder, slot + Game)) ? File.ReadAllText(Path.Combine(folder, slot + Game)) : null;
        string ReadInfo(string slot) => File.Exists(Path.Combine(folder, slot + Info)) ? File.ReadAllText(Path.Combine(folder, slot + Info)) : null;

        void Autosaved(string what)
        {
            Put(Autosave.Fresh, what);
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
        public void TheSavesSheAskedForAreNotTouched()
        {
            Put("quick", "hers");
            Put("save 1", "hers too");
            for (int i = 0; i < 6; i++)
                Autosaved("auto " + i);
            Assert.AreEqual("hers", Read("quick"));
            Assert.AreEqual("hers too", Read("save 1"));
            Assert.AreEqual("hers info", ReadInfo("quick"));
        }

        [Test]
        public void NothingIsSavedWhileTheGameCannotBe()
        {
            // (EditMode: no game is running, so SaveGame.Busy says why not - the same gate as a meeting or being aground)
            Assert.IsNotNull(SaveGame.Busy());
            string fresh = Path.Combine(SaveGame.Folder, Autosave.Fresh + Game);
            bool was = File.Exists(fresh);
            Assert.IsFalse(Autosave.Now());
            Autosave.Quitting();
            Autosave.Moored();
            Autosave.CastOff();
            Assert.AreEqual(was, File.Exists(fresh), "no file written while busy");
        }
    }
}
```
The last test reads SaveGame.Folder (the real saves folder) but writes nothing there: Now() returns at the Busy gate.
The test assembly is Tests/EditMode/Kapman.Tests.EditMode.asmdef, which already sees Kapman.Game (SaveTests.cs).

## Not done / to check when compiling

- Not compiled, not run (rule: no Unity). MainMenu.cs:240-255 not read (see change 4).
- A Busy state that outlasts the stay (she moors, a meeting never ends, she casts off): no autosave for that call.
- Quit while Busy writes nothing and does not ask. If she wants "you can't save now, quit anyway?", that is a
  MainMenu change and her ruling.
- Each autosave runs `Trade.Ledger.Thin` (SaveGame.cs:83): saving changes the live price notes (save_audit.md, the
  line under the five losses). Autosaving makes that happen at every mooring. It only thins OLD notes by the rules,
  but it is worth moving Thin out of Save (to the day's turn) so that saving never changes the game.
- The autosaves show in the load list under their slot names "autosave 1..3" (MainMenu.Describe, :178, not read):
  wording for her to choose.
- A save is one gzip of the whole world on the main thread: a hitch at the moment of mooring is possible; not measured.
- Whether `ship.anchored` is already true at the frame the watch says Moored on the chart was not traced; if TextPanel
  is open and she is not yet "lying in port", Busy refuses and the held save goes through a frame or so later (Tick).
- A save loaded in another level than it was made in does not restore her place (save_audit.md loss 3): that holds
  for autosaves too.
