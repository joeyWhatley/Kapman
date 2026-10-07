using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.ShipView;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE BEGINNING OF THE GAME, PLAYED AS A PLAYER PLAYS IT (the user, 7 Oct 2026: the slow start with the turning
    /// boat, the name on the purple, the box that comes up and opens, the lender's questions, the showroom, the first
    /// morning, the marks and Ravn, the first jobs). The show is switched on for this test (it is off in a batch run),
    /// its waits are shortened, and every step is a picture in Captures/chart/open_*.png.
    /// Run: -runTests -testPlatform PlayMode -testFilter OpeningTests   (with graphics: no -nographics)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class OpeningTests
    {
        static IEnumerator FreshTitle()
        {
            // as MainMenu.NewGame clears a game, but back to the title, with the show on
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (SceneManager.GetActiveScene().name == "L_Chart")
                    go.SetActive(false);
            MainMenu.ClearAllState();
            Showtime.ForceOn = true;
            FrameWindows.OneWindow = true;                                // (the side as a new player has it, whatever a test before left)
            Showtime.LoadingSeconds = 6.5f;
            Showtime.SlideSeconds = 1.4f;
            FrameRoot.FreshLayout = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_test_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            ShipStageHook.TurnOn();
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings");
            while (!load.isDone)
                yield return null;
            Showtime.ForceOn = true;
            FrameWindows.OneWindow = true;                                // (the side as a new player has it, whatever a test before left)
            SaveGame.FolderOverride = saves;
            FrameRoot.FreshLayout = true;
            yield return ChartPlay.Frames(5);
        }

        static bool Up(VisualElement v) => v != null && v.resolvedStyle.display != DisplayStyle.None && v.resolvedStyle.opacity > 0.5f;

        static IEnumerator EnterUntil(ChartPlay play, System.Func<bool> there, int most = 40)
        {
            for (int i = 0; i < most && !there(); i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
        }

        [UnityTest]
        public IEnumerator TheGameBeginsWithTheLenderTheShowroomAndRavn()
        {
            foreach (string old in Directory.GetFiles(Path.Combine(Application.dataPath, "..", "Captures", "chart"), "open_*.png"))
                File.Delete(old);
            yield return FreshTitle();
            var play = ChartPlay.Begin(false);
            var notes = new System.Text.StringBuilder();
            try
            {
                // ---- 1. the slow start: black, the purple, the boat turning at the bottom right
                yield return ChartPlay.Until(() => Showtime.Current != null && Showtime.Current.Stage == "loading", 15f);
                var show = Showtime.Current;
                Assert.IsNotNull(show, "the show did not start with the menu");
                Assert.AreEqual("loading", show.Stage, "the slow start did not begin on the title");
                Assert.IsTrue(MainMenu.Current.OnTitle);
                Assert.IsTrue(Showtime.BeforeTheGame);
                yield return ChartPlay.Until(() => Up(show.Loader), 6f);
                yield return ChartPlay.Settle(2.6f);
                Assert.IsTrue(Up(show.Loader), "the loader went before it could be looked at");
                var hook = ShipStageHook.Current;
                notes.AppendLine("the loader's boat: " + (hook != null && hook.StagePicture != null ? hook.StagePicture.width + " x " + hook.StagePicture.height : "no picture (no 3D view in this run)"));
                yield return play.Shot("open_01_loading", 0f);

                // ---- 2. the name, and the menu under it
                yield return ChartPlay.Until(() => show.Stage == "title", 25f);
                Assert.AreEqual("title", show.Stage);
                Assert.IsTrue(Up(show.BigName), "the game's name is not up on the title");
                Assert.IsFalse(show.MenuShown, "the title should be the name alone");
                Assert.IsFalse(ChartPlay.Shown(MainMenu.Current.Root), "the menu is on the title before anybody asked for it");
                if (hook != null && hook.Stage != null)
                {
                    Assert.IsTrue(hook.Stage.TitleSea, "the title's sea is not on the stage");
                    Assert.IsTrue(Up(show.SeaPicture), "the title's sea is not on the screen");
                }
                yield return ChartPlay.Settle(1.5f);
                yield return play.Shot("open_02_title", 0f);
                yield return ChartPlay.Settle(1.1f);
                yield return play.Shot("open_02a_title_later", 0f);

                // Esc brings the menu up (the sound, Quit), and puts it away again
                yield return play.Press(Key.Escape);
                yield return ChartPlay.Frames(4);
                Assert.IsTrue(show.MenuShown, "Esc did not bring the title's menu up");
                Assert.IsNotNull(play.Button("1. New game"), "the title's menu has no New game");
                yield return play.Shot("open_02b_title_menu");
                yield return play.Press(Key.Escape);
                yield return ChartPlay.Frames(4);
                Assert.IsFalse(show.MenuShown);

                // ---- 3. a click anywhere plays: the letters go under, the sea fades into the purple, the box comes up
                Rect whole = show.BigName.parent.worldBound;
                play.ClickAt(new Vector2(whole.width * 0.3f, whole.height * 0.75f));
                yield return ChartPlay.Until(() => show.Stage == "letters", 3f);
                Assert.AreEqual("letters", show.Stage, "a click on the title did not send the letters under");
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("open_02c_letters_going", 0f);
                yield return ChartPlay.Until(() => show.Stage == "slide" && Up(show.Box), 14f);
                Assert.IsTrue(Up(show.Box), "the box did not come up");
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("open_03_box_rising", 0f);
                yield return ChartPlay.Until(() => show.Stage == "asking", 15f);
                Assert.AreEqual("asking", show.Stage);
                Assert.IsFalse(FrameRoot.Current.AskingSelf, "the old creation window is up under the opening");

                // ---- 4. "..." then, where this machine has faces to choose from, the player's own face first
                yield return EnterUntil(play, () => show.Choices.childCount > 0);
                if (show.Stage == "face")
                {
                    Assert.AreEqual(Showtime.AskFace, show.Words.text, "the faces are up under some other words");
                    yield return ChartPlay.Settle(0.3f);
                    yield return play.Shot("open_03b_face");
                    yield return play.Press(Key.Digit1);
                    yield return ChartPlay.Until(() => show.Stage == "asking", 4f);
                    Assert.AreEqual(Showtime.FacePool()[0], show.PickedFace, "the first face was not taken by its number");
                    yield return EnterUntil(play, () => show.Stage == "asking" && show.Choices.childCount > 0 && show.Words.text == Showtime.AskPeople);
                }
                // ---- then "And from where do you hail?"
                Assert.AreEqual(Showtime.AskPeople, show.Words.text, "the first question is not where you hail from");
                Assert.GreaterOrEqual(show.Choices.childCount, 2, "no peoples to choose from");
                yield return play.Shot("open_04_hail");
                yield return play.Press(Key.Digit1);
                yield return ChartPlay.Until(() => show.NameField != null, 6f);
                Assert.AreEqual("frisian", show.PickedPeople);

                // ---- 5. "And your name?": one is given, then one is typed
                Assert.IsNotNull(show.NameField, "no box to type a name in");
                yield return ChartPlay.Until(() => show.Choices.Query<Button>().ToList().Count >= 3, 4f);
                var given = show.Choices.Query<Button>().ToList().First(b => b.text == "A man's name");
                yield return play.Click(given, "A man's name");
                yield return ChartPlay.Frames(3);
                string name = show.NameField.value;
                CollectionAssert.Contains(PeriodNames.Of("frisian", false).ToList(), name, "the name given is not one of the Frisian men's names");
                yield return play.Shot("open_05_name");
                show.NameField.value = "Radbod";
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Until(() => show.NameField == null, 4f);
                Assert.AreEqual("Radbod", show.PickedName);

                // ---- 6. the faith, 7. the past
                yield return ChartPlay.Until(() => show.Words.text == Showtime.AskFaith && show.Choices.childCount > 0, 6f);
                Assert.AreEqual(Showtime.AskFaith, show.Words.text);
                yield return play.Shot("open_06_swear");
                yield return play.Press(Key.Digit1);
                yield return ChartPlay.Until(() => show.Words.text == Showtime.AskPast && show.Choices.childCount > 0, 6f);
                Assert.AreEqual(Showtime.AskPast, show.Words.text);
                yield return play.Shot("open_07_before");
                yield return play.Press(Key.Digit1);

                // ---- 8. who asked: the lender, speaking a letter at a time
                yield return ChartPlay.Until(() => show.Stage == "lender", 6f);
                Assert.AreEqual("lender", show.Stage);
                yield return ChartPlay.Until(() => show.Words.text.Contains("<color=#00000000>"), 3f);
                StringAssert.Contains("<color=#00000000>", show.Words.text, "the lender's line does not come a letter at a time");
                Assert.IsTrue(Self.Chosen, "the game did not begin with the answers");
                Assert.AreEqual("Radbod", Self.Called);
                Assert.AreEqual("frisian", Self.People);
                Assert.IsTrue(FirstJobs.Active, "the first jobs were not set");
                Assert.AreEqual(1, Voyage.Men, "she should begin with one man aboard, Ravn");
                yield return play.Press(Key.Enter);                       // (the line finished at once)
                yield return ChartPlay.Frames(3);
                StringAssert.DoesNotContain("<color", show.Words.text, "Enter did not finish the line");
                StringAssert.Contains("Radbod", show.Words.text);
                yield return play.Shot("open_08_lender");
                yield return EnterUntil(play, () => show.Stage == "showroom" || show.Stage == "first morning", 60);

                // ---- 9. the showroom: the boat alone, turning on the pink
                if (hook != null && hook.Stage != null)
                {
                    Assert.AreEqual("showroom", show.Stage);
                    yield return ChartPlay.Settle(4f);
                    Assert.IsTrue(hook.Stage.Showroom, "the stage is not in its showroom");
                    yield return play.Shot("open_09_showroom");
                    yield return ChartPlay.Settle(1.2f);
                    yield return play.Shot("open_09b_showroom_turned");
                    yield return play.Press(Key.Enter);
                }
                else
                    notes.AppendLine("no 3D view in this run: the showroom was passed over");

                // ---- 10. the first morning: the place's card, then the game
                yield return ChartPlay.Until(() => show.Stage == "first morning", 8f);
                yield return ChartPlay.Settle(1.2f);
                yield return play.Shot("open_10_first_morning", 0f);
                yield return ChartPlay.Until(() => show.Stage == "free", 15f);
                Assert.AreEqual("free", show.Stage);
                Assert.IsTrue(show.Armed);
                Assert.IsFalse(Showtime.BeforeTheGame, "the game proper has begun: the title's music should let go");
                Assert.IsTrue(Showtime.HoldsCards, "a card could open before Ravn has spoken");
                Assert.IsTrue(ContentFlags.Has(Trade.LenderFlag), "the game does not carry the lender's flag");
                Assert.AreEqual(Trade.LenderByname, Trade.CreditorSaid);
                Assert.IsFalse(hook != null && hook.Stage != null && hook.Stage.Showroom, "the stage is still in its showroom");
                yield return ChartPlay.Settle(1f);
                // the port's own picture stands where the chart was, lit by the hour
                var portView = PortView.Current;
                Assert.IsNotNull(portView, "no port view was made with the frame");
                Assert.IsTrue(portView.Shown, "she lies at Dorestat and the chart is still in the middle of the screen");
                Assert.AreEqual("dorestad", portView.Port);
                Assert.IsNotNull(portView.Picture.image, "the port's picture has no picture");
                yield return play.Shot("open_11_free");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text.Contains(Trade.LenderByname)), "the log does not say whom the debt is owed to: " +
                              string.Join(" / ", FrameLog.Lines.Select(l => l.text)));

                // ---- 11. the first click: the marks, then Ravn
                Rect all = play.Frame.Root.worldBound;
                play.ClickAt(new Vector2(all.width * 0.5f, all.height * 0.5f));
                yield return ChartPlay.Until(() => show.Stage == "marks", 3f);
                Assert.AreEqual("marks", show.Stage, "the first click was not answered by the marks");
                yield return ChartPlay.Settle(0.75f);
                yield return play.Shot("open_12_marks", 0f);
                // (and they fly down to where Ravn is about to stand: the user, 8 Oct 2026)
                yield return ChartPlay.Until(() => show.MarksFlying, 5f);
                Assert.IsTrue(show.MarksFlying, "the marks did not set off for Ravn's portrait");
                yield return ChartPlay.Frames(10);
                yield return play.Shot("open_12b_marks_fly_to_ravn", 0f);
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("ravn.first"), 6f);
                Assert.IsTrue(play.Frame.FloatOpen("ravn.first"), "Ravn did not speak after the marks");
                Assert.IsTrue(Showtime.OpeningByGraph, "the opening ran from code: no graph of the scene listened for it (the Showtime object's Script Machine, \"Opening - the lender, the boat, the first morning\")");
                yield return ChartPlay.Settle(0.8f);
                Assert.IsNotNull(play.Frame.FrontReveal, "Ravn's lines stand whole: they should be said a letter at a time");
                Assert.IsFalse(play.Frame.FrontReveal.Done, "Ravn has said everything within a second");
                yield return play.Shot("open_13_ravn_speaking", 0f);
                // one paragraph at a time: Enter finishes the one being said, which then stands alone and waits; Enter
                // again and the next takes its place. His choices are not up until he has said the last.
                Assert.IsTrue(play.Frame.FrontReveal.OneAtATime, "Ravn's lines are not said one at a time");
                int linesBefore = play.Frame.FrontReveal.Index;
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(3);
                Assert.AreEqual(linesBefore, play.Frame.FrontReveal.Index, "Enter went past the paragraph Ravn was saying: it should finish it and wait");
                Assert.IsTrue(play.Frame.FrontReveal.Waiting, "the paragraph Enter finished does not wait for the next press");
                {
                    int seen = 0;
                    play.FrontFloat().Query<VisualElement>(className: "frame-float-line").ForEach(l => seen += l.resolvedStyle.display == DisplayStyle.Flex ? 1 : 0);
                    Assert.AreEqual(1, seen, "more than one paragraph of Ravn's stands in the window at one time");
                    var choices = play.FrontFloat().Q<VisualElement>(className: "frame-float-options");
                    Assert.IsTrue(choices == null || choices.resolvedStyle.display == DisplayStyle.None, "Ravn's choices are up before he has finished");
                }
                yield return play.Shot("open_13b_ravn_one_paragraph", 0f);
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(3);
                Assert.Greater(play.Frame.FrontReveal.Index, linesBefore, "Enter did not go on to Ravn's next paragraph");
                // WHAT HE SPEAKS OF IS POINTED AT (the user, 8 Oct 2026: "can you highlight what he's talking about"): as each
                // paragraph is said, the frame's pointer stands round the part of the screen it is about
                var pointedAt = new System.Collections.Generic.List<string>();
                bool rowShot = false;
                for (int press = 0; press < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; press++)
                {
                    float until = Time.realtimeSinceStartup + 3f;
                    while (Time.realtimeSinceStartup < until && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done)
                    {
                        string now = play.Frame.PointedAt;
                        if (now.Length > 0 && !pointedAt.Contains(now))
                            pointedAt.Add(now);
                        if (!rowShot && now.StartsWith("verb-cast-off verb-voyage"))
                        {
                            rowShot = true;
                            Assert.AreEqual(DisplayStyle.Flex, play.Frame.Pointer.resolvedStyle.display, "he speaks of the top row and the pointer is not shown");
                            var row = play.Frame.Root.Q<VisualElement>("verb-cast-off").worldBound;
                            var hold = play.Frame.Root.Q<VisualElement>("verb-hold").worldBound;
                            var frameRound = play.Frame.Pointer.worldBound;
                            Assert.IsTrue(frameRound.xMin <= row.xMin && frameRound.xMax >= hold.xMax && frameRound.yMin <= row.yMin && frameRound.yMax >= row.yMax,
                                "the pointer is not round the top row of buttons: " + frameRound + " against " + row + " to " + hold);
                            yield return play.Shot("open_13c_ravn_points_at_the_top_row", 0f);
                        }
                        yield return null;
                    }
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(3);
                    string then = play.Frame.PointedAt;
                    if (then.Length > 0 && !pointedAt.Contains(then))
                        pointedAt.Add(then);
                }
                notes.AppendLine("Ravn pointed at: " + string.Join(" | ", pointedAt));
                CollectionAssert.Contains(pointedAt, "ship", "he spoke of the boat and her window was not pointed at");
                CollectionAssert.Contains(pointedAt, "verb-alehouse", "he spoke of the alehouse and its button was not pointed at");
                Assert.IsTrue(pointedAt.Exists(at => at.StartsWith("speed-pause")), "he spoke of the clock and it was not pointed at: " + string.Join(" | ", pointedAt));
                Assert.IsTrue(rowShot, "the top row of buttons was never pointed at while he spoke of it: " + string.Join(" | ", pointedAt));
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("open_14_ravn_said");
                string ravn = ChartPlay.Read(play.FrontFloat());
                notes.AppendLine("Ravn says: " + ravn);
                StringAssert.Contains("sail", ravn);
                StringAssert.Contains("men", ravn);
                yield return play.Press(Key.Enter);                       // (Go on)
                yield return ChartPlay.Until(() => !play.Frame.FloatOpen("ravn.first"), 4f);
                Assert.IsFalse(play.Frame.FloatOpen("ravn.first"));
                yield return ChartPlay.Frames(2);
                Assert.AreEqual("", play.Frame.PointedAt, "Ravn has done and the pointer still stands");
                Assert.IsFalse(Showtime.Busy);

                // ---- 12. the first jobs: no casting off until the hands are hired and the sail is mended
                play.Frame.Look(FrameIcon.Kind.CastOff, out _, out string why, out bool can, out _);
                Assert.IsFalse(can, "she may cast off with one man and a rotten sail");
                StringAssert.StartsWith("Not yet.", why);
                Assert.IsFalse(FrameGame.CastOff(), "the port's own way out is open before the jobs are done");
                yield return ChartPlay.Settle(0.6f);
                // (one window on the side: in port it is the Port window; the People here window is had by its name)
                Assert.IsTrue(FrameWindows.OneWindow, "the side has more than one window for a new player");
                Assert.AreEqual(1, play.Frame.Windows.Space.slots.Count, "the side has more than one window");
                string screen = ChartPlay.Read(play.Frame.Root);
                StringAssert.Contains("Sails tattered", screen, "the Port window does not offer to mend the sail");
                play.Frame.Windows.Show("people");
                yield return ChartPlay.Settle(0.6f);
                screen = ChartPlay.Read(play.Frame.Root);
                StringAssert.Contains("Shipmen looking for a boat", screen, "the People here window does not offer the shipmen");
                play.Frame.Windows.Show("port");
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("open_15_jobs");
                double silver = FrameGame.Silver;
                for (int i = 0; i < FirstJobs.Hands.Count && FirstJobs.HandsShort > 0; i++)
                    Assert.IsTrue(FirstJobs.Hire(i, out string fact), "a hand could not be hired: " + fact);
                Assert.AreEqual(FirstJobs.CrewWanted, Voyage.Men);
                Assert.AreEqual(silver - 3 * FirstJobs.HandMoney, FrameGame.Silver, 0.6, "hand money was not paid");
                Assert.IsTrue(FirstJobs.Active, "the jobs are over with the sail still worn");
                Assert.IsTrue(FirstJobs.Mend(KapmanGame.Current.Day, out string mend), mend);
                Assert.IsTrue(FirstJobs.Mending);
                play.Frame.Look(FrameIcon.Kind.CastOff, out _, out why, out can, out _);
                Assert.IsFalse(can);
                StringAssert.Contains("being mended", why);
                GameSpeed.SetLevel(4);
                yield return ChartPlay.Until(() => !FirstJobs.SailWorn, 30f);
                GameSpeed.SetLevel(0);
                Assert.IsFalse(FirstJobs.SailWorn, "six hours of the clock did not mend the sail");
                Assert.IsFalse(FirstJobs.Active, "both jobs are done and they are still set");
                play.Frame.Look(FrameIcon.Kind.CastOff, out _, out why, out can, out _);
                Assert.IsFalse(why.StartsWith("Not yet."), "the jobs are done and she is still held: " + why);
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("open_16_jobs_done");

                // ---- 13. the port's picture: the chart can be had back, and the hour changes its light
                var view = PortView.Current;
                Assert.IsTrue(view.Shown);
                Assert.AreEqual(1f, PortView.LightAt(40f).r, 1e-4f, "by day the picture is as it was drawn");
                Assert.Less(PortView.LightAt(-20f).r, 0.3f, "by night the picture is not dark");
                Assert.Greater(PortView.LightAt(-20f).b, PortView.LightAt(-20f).r, "night is not blue");
                yield return play.Click(view.Flip, "Chart");
                yield return ChartPlay.Frames(4);
                Assert.IsFalse(view.Shown, "the button did not put the chart back");
                yield return play.Shot("open_17_chart_in_port");
                yield return play.Click(view.Flip, "The port");
                yield return ChartPlay.Frames(4);
                Assert.IsTrue(view.Shown);
                PortView.SunOverride = -1f;                               // (the Sun on the horizon, then well under it)
                yield return ChartPlay.Settle(2.5f);
                yield return play.Shot("open_18_port_at_sunset");
                PortView.SunOverride = -20f;
                yield return ChartPlay.Settle(3f);
                Assert.Less(view.Light.r, 0.35f, "the night's light did not come over the port");
                yield return play.Shot("open_19_port_at_night");
                PortView.SunOverride = null;
                Debug.Log("The opening:\n" + notes);
                File.WriteAllText(Path.Combine(Application.dataPath, "..", "Captures", "chart", "open.txt"), notes.ToString());
            }
            finally
            {
                File.WriteAllText(Path.Combine(Application.dataPath, "..", "Captures", "chart", "open.txt"), notes.ToString());
                play.End();
                Showtime.ForceOn = false;
                Showtime.LoadingSeconds = 6.5f;
                Showtime.SlideSeconds = 2.8f;
                FrameRoot.AskedElsewhere = false;
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
