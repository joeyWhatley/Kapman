using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// NOTES WHILE PLAYING, MADE AS A PLAYER MAKES THEM (the user, 7 Oct 2026: "create a mode where I can edit and note
    /// changes to be made on the go while playing"). A new game at Dorestat; F2; a click on words in a window; the slip
    /// is filled in and saved; then a note on the whole screen; then Esc out of the picker, which must not open the
    /// menu. The files are read back. Pictures: Captures/chart/notes_*.png.
    /// Run: -runTests -testPlatform PlayMode -testFilter PlayNotesTests   (with graphics: no -nographics)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class PlayNotesTests
    {
        [UnityTest]
        public IEnumerator ANoteIsWrittenDownFromInsideTheGame()
        {
            string folder = Path.Combine(Application.temporaryCachePath, "kapman_test_notes");
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            PlayNotes.FolderOverride = folder;
            yield return ShipViewChartTests.ChartWithNoGame();
            var play = ChartPlay.Begin(false);
            try
            {
                yield return ShipViewChartTests.NewGameAtDorestat(play);
                yield return ChartPlay.Settle(1f);
                var notes = PlayNotes.Current;
                Assert.IsNotNull(notes, "the notes mode did not start with the frame");
                Assert.IsFalse(PlayNotes.Busy);

                // ---- F2: the clock is held and the picker is up
                yield return play.Press(Key.F2);
                Assert.IsTrue(notes.Picking, "F2 did not start the picker");
                Assert.IsTrue(GameSpeed.HeldFor(PlayNotes.Why), "the clock is not held while a note is made");
                Assert.IsTrue(GameSpeed.Paused);
                yield return play.Shot("notes_1_picker");

                // ---- a click on words in a window: the slip, about those words, with the words to change
                Label thing = null;
                foreach (var l in play.Frame.Root.Query<Label>().ToList())
                {
                    if (!ChartPlay.Shown(l) || (l.text ?? "").Trim().Length < 4 || l.worldBound.width < 20f)
                        continue;
                    bool inWindow = false;
                    for (var e = l.parent; e != null; e = e.parent)
                        inWindow |= e.ClassListContains("frame-win");
                    if (inWindow && play.Panel.Pick(l.worldBound.center) != null)
                    {
                        thing = l;
                        break;
                    }
                }
                Assert.IsNotNull(thing, "no words in any window to click on");
                string says = thing.text;
                bool wasMenu = MainMenu.Current.Open;
                play.ClickAt(thing.worldBound.center);
                yield return ChartPlay.Until(() => notes.NoteBox != null, 5f);
                Assert.IsTrue(notes.Writing, "a click in the picker did not open the slip");
                Assert.IsNotNull(notes.NoteBox, "the slip has no box to write in");
                Assert.AreEqual("note-box", notes.NoteBox.name, "the slip is not a copy of Parts/NoteSlip.uxml");
                Assert.AreEqual("note-slip", notes.NoteBox.parent.name, "the box is not in the slip of the file");
                Assert.IsNotNull(notes.WordsBox, "the thing has words (" + says + ") but the slip offers none to change: " + notes.About);
                StringAssert.StartsWith("the words \"", notes.About);
                notes.NoteBox.value = "Make this plainer.\nA second line with \"quotes\" in it.";
                notes.WordsBox.value = "New words for the test";
                yield return play.Shot("notes_2_slip");
                yield return play.ClickButton("Save the note", play.Frame.Root);
                yield return ChartPlay.Settle(0.3f);
                Assert.IsFalse(notes.Writing);
                Assert.IsFalse(notes.Picking);
                Assert.IsFalse(GameSpeed.HeldFor(PlayNotes.Why), "the clock is still held after the note was saved");
                Assert.AreEqual(1, notes.Count);
                string md = File.ReadAllText(Path.Combine(folder, "play_notes.md"));
                StringAssert.Contains("## Note 1 (", md);
                StringAssert.Contains("at Dorestat", md);
                StringAssert.Contains("**Change:** Make this plainer.", md);
                StringAssert.Contains("**It should say:** New words for the test", md);
                string[] lines = File.ReadAllLines(Path.Combine(folder, "play_notes.jsonl"));
                Assert.AreEqual(1, lines.Length);
                var one = Json.Parse(lines[0]) as Dictionary<string, object>;
                Assert.IsNotNull(one, "the note's line is not JSON: " + lines[0]);
                Assert.AreEqual("Make this plainer.\nA second line with \"quotes\" in it.", Json.Str(one, "note"));
                Assert.AreEqual(says, Json.Str(one, "says"));
                Assert.AreEqual("New words for the test", Json.Str(one, "should_say"));
                Assert.AreEqual(1.0, Json.Num(one, "n"));
                Assert.IsTrue(Json.Str(one, "thing").Length > 0, "the note does not say which thing it is about");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.Whole.StartsWith("Note 1 is written down")), "the log does not say the note was kept");

                // ---- F2 twice: a note on the whole screen, with no words to change; saved by its own call
                yield return play.Press(Key.F2);
                Assert.IsTrue(notes.Picking);
                yield return play.Press(Key.F2);
                yield return ChartPlay.Until(() => notes.NoteBox != null, 5f);
                Assert.AreEqual("the whole screen", notes.About);
                Assert.IsNull(notes.WordsBox);
                notes.NoteBox.value = "The second note.";
                notes.Save();
                Assert.AreEqual(2, notes.Count);
                Assert.AreEqual(2, File.ReadAllLines(Path.Combine(folder, "play_notes.jsonl")).Length);

                // ---- an empty slip keeps nothing; Esc leaves the picker and the menu stays shut
                yield return play.Press(Key.F2);
                yield return play.Press(Key.F2);
                yield return ChartPlay.Until(() => notes.NoteBox != null, 5f);
                notes.Save();
                Assert.AreEqual(2, notes.Count, "an empty note was kept");
                yield return ChartPlay.Frames(3);
                yield return play.Press(Key.F2);
                Assert.IsTrue(notes.Picking);
                yield return play.Press(Key.Escape);
                Assert.IsFalse(notes.Picking, "Esc did not leave the picker");
                yield return ChartPlay.Frames(3);
                Assert.AreEqual(wasMenu, MainMenu.Current.Open, "Esc out of the notes also opened the menu");
                Assert.IsFalse(GameSpeed.HeldFor(PlayNotes.Why));
                yield return play.Shot("notes_3_after");
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                PlayNotes.Current?.Leave();
                play.End();
                PlayNotes.FolderOverride = null;
                FrameRoot.FreshLayout = false;
            }
        }
    }
}
