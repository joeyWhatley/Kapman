using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Encounters;
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

        /// <summary>A card on the screen is said through, answered by its first choice that can be chosen, and gone on from
        /// until it is shut.</summary>
        static IEnumerator AnswerCard(ChartPlay play, System.Text.StringBuilder notes)
        {
            for (int i = 0; i < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
            yield return ChartPlay.Settle(0.3f);
            var front = play.FrontFloat();
            Button pick = null;
            string words = "";
            if (front != null)
                foreach (var c in play.Choices(front))
                    if (pick == null && c.can)
                    {
                        pick = c.button;
                        words = c.words;
                    }
            Assert.IsNotNull(pick, "the card has no choice that can be made: " + ChartPlay.Short(ChartPlay.Read(front)));
            notes.AppendLine("card answered: " + words);
            yield return play.Click(pick, "the choice '" + words + "'");
            yield return ChartPlay.Settle(0.4f);
            for (int i = 0; i < 30 && play.Frame.FloatOpen("card"); i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(6);
            }
            Assert.IsFalse(play.Frame.FloatOpen("card"), "the card could not be answered and shut");
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
                bool listShot = false;
                for (int press = 0; press < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; press++)
                {
                    float until = Time.realtimeSinceStartup + 3f;
                    while (Time.realtimeSinceStartup < until && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done)
                    {
                        string now = play.Frame.PointedAt;
                        if (now.Length > 0 && !pointedAt.Contains(now))
                            pointedAt.Add(now);
                        // (he no longer reads out the buttons: the script critic's report of 8 Oct. What he points at last is the list)
                        if (!listShot && now == "jobs")
                        {
                            listShot = true;
                            Assert.AreEqual(DisplayStyle.Flex, play.Frame.Pointer.resolvedStyle.display, "he speaks of the list and the pointer is not shown");
                            var listBox = play.Frame.Root.Q<VisualElement>("jobs").worldBound;
                            var frameRound = play.Frame.Pointer.worldBound;
                            Assert.IsTrue(frameRound.xMin <= listBox.xMin && frameRound.xMax >= listBox.xMax && frameRound.yMin <= listBox.yMin && frameRound.yMax >= listBox.yMax,
                                "the pointer is not round the list: " + frameRound + " against " + listBox);
                            yield return play.Shot("open_13c_ravn_points_at_the_list", 0f);
                        }
                        yield return null;
                    }
                    // (his last paragraph is short now and may be said out while this waits: Enter then would take "Go on")
                    if (play.Frame.FrontReveal == null || play.Frame.FrontReveal.Done)
                        break;
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(3);
                    string then = play.Frame.PointedAt;
                    if (then.Length > 0 && !pointedAt.Contains(then))
                        pointedAt.Add(then);
                }
                notes.AppendLine("Ravn pointed at: " + string.Join(" | ", pointedAt));
                CollectionAssert.Contains(pointedAt, "ship", "he spoke of the boat and her window was not pointed at");
                CollectionAssert.Contains(pointedAt, "verb-alehouse", "he spoke of the alehouse and its button was not pointed at");
                Assert.IsFalse(pointedAt.Exists(at => at.StartsWith("speed-pause") || at.StartsWith("rack")), "he still reads out the buttons or the clock: " + string.Join(" | ", pointedAt));
                Assert.IsTrue(listShot, "the list was never pointed at while he spoke of it: " + string.Join(" | ", pointedAt));
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("open_14_ravn_said");
                string ravn = ChartPlay.Read(play.FrontFloat());
                notes.AppendLine("Ravn says: " + ravn);
                // (he ends on himself, and does not say the five jobs over: the list holds them)
                StringAssert.Contains("That's how I know where it's kept.", ravn);
                StringAssert.DoesNotContain("Mend the sail, find the men", ravn);
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
                // THE LIST (the critic's B1, 8 Oct 2026: "after Ravn stops, nothing on screen holds what he said"): five rows,
                // none done, the sail's with its cost as the button, and Cast off saying what is wanting. IT IS A TAB OF
                // THE ONE SIDE WINDOW (the user, 8 Oct: "maybe make it one panel with a tab system"), and the tab shown.
                yield return ChartPlay.Until(() => play.Frame.JobsShown, 3f);
                Assert.IsTrue(play.Frame.JobsShown, "the list Before she sails is not on the screen after Ravn");
                var list = play.Frame.Root.Q<VisualElement>("jobs");
                string jobs = ChartPlay.Read(list);
                notes.AppendLine("The list: " + jobs);
                StringAssert.Contains("Before she sails", jobs);
                StringAssert.Contains("Sails tattered....", jobs);
                StringAssert.Contains("Needs to be repaired.", jobs);
                StringAssert.Contains("Crew: 1 of 4", jobs);
                StringAssert.Contains("No course set", jobs);
                StringAssert.Contains("First the sail mended, three more men and a course.", jobs);
                Assert.AreEqual(0, list.Query<VisualElement>(className: "frame-job").Where(r => r.ClassListContains("done")).ToList().Count, "a job is ticked before anything is done");
                var mendButton = play.Frame.Root.Q<Button>("mend-sail");
                Assert.IsTrue(ChartPlay.Shown(mendButton), "the sail's row has no button");
                Assert.AreEqual($"{FirstJobs.MendCost:0} d", mendButton.text, "the cost is not the button of the sail's row");
                if (FirstJobs.TipGood.Length > 0)
                    StringAssert.Contains("Ravn's word: ", jobs, "Ravn named a good and the list does not hold it");
                string screen = ChartPlay.Read(play.Frame.Root);
                StringAssert.Contains("Sails tattered", screen, "the screen does not offer to mend the sail");
                StringAssert.DoesNotContain("Look under", why, "the reason still sends the player to a window by its name");
                // one panel, two tabs: "To do" is the one shown, and the list is inside the window
                Assert.IsTrue(play.Frame.TodoOn, "the tab To do is not the one shown after Ravn");
                var sideWindow = play.Frame.Root.Q<VisualElement>(className: "frame-win");
                Assert.IsTrue(sideWindow.ClassListContains("has-todo") && sideWindow.ClassListContains("todo-on"), "the side window has no tabs");
                Assert.IsTrue(sideWindow.Contains(list), "the list is not inside the one side window");
                var todoTab = sideWindow.Q<Button>("todo");
                Assert.IsTrue(ChartPlay.Shown(todoTab), "the tab To do is not on the screen");
                StringAssert.DoesNotContain("Choose what to do", screen, "the Port window's own words show under the list: two panels at once");
                yield return play.Shot("open_15_jobs");
                // a window asked for by a button turns to that window's tab; the tab To do comes back to the list
                play.Frame.Windows.Show("people");
                yield return ChartPlay.Settle(0.6f);
                Assert.IsFalse(play.Frame.TodoOn, "a window was asked for and the list still covers it");
                Assert.IsFalse(play.Frame.JobsShown);
                screen = ChartPlay.Read(play.Frame.Root);
                StringAssert.Contains("Shipmen looking for a boat", screen, "the People here window does not offer the shipmen");
                StringAssert.DoesNotContain("Sails tattered", screen, "the list shows with the window: two panels at once");
                play.Frame.Windows.Show("port");
                yield return ChartPlay.Settle(0.4f);
                sideWindow = play.Frame.Root.Q<VisualElement>(className: "frame-win");
                Assert.IsTrue(sideWindow.ClassListContains("has-todo") && sideWindow.ClassListContains("window-on"), "the window's own tab is not marked as the one shown");
                yield return play.Shot("open_15a_port_tab");
                todoTab = sideWindow.Q<Button>("todo");
                yield return play.Click(todoTab, "the tab To do");
                yield return ChartPlay.Settle(0.3f);
                Assert.IsTrue(play.Frame.TodoOn && play.Frame.JobsShown, "the tab To do did not bring the list back");
                mendButton = play.Frame.Root.Q<Button>("mend-sail");
                double silver = FrameGame.Silver;
                // (the men on the quay are the crew the cards know: 8 Oct 2026, night)
                CollectionAssert.AreEqual(new[] { "Aldger", "Brunger", "Beornwine" }, FirstJobs.Hands.Select(h => h.name).ToArray(), "the men on the quay are not the crew of the cards");
                for (int i = 0; i < FirstJobs.Hands.Count && FirstJobs.HandsShort > 0; i++)
                    Assert.IsTrue(FirstJobs.Hire(i, out string fact), "a hand could not be hired: " + fact);
                Assert.AreEqual(FirstJobs.CrewWanted, Voyage.Men);
                Assert.AreEqual(silver - 3 * FirstJobs.HandMoney, FrameGame.Silver, 0.6, "hand money was not paid");
                Assert.IsTrue(FirstJobs.Active, "the jobs are over with the sail still worn");
                // (the sail is given to be mended by its row's own button)
                yield return play.Click(mendButton, "the cost, in the sail's row of the list");
                Assert.IsTrue(FirstJobs.Mending, "the button of the sail's row did not send the sail to be mended");
                yield return ChartPlay.Settle(0.5f);
                jobs = ChartPlay.Read(list);
                StringAssert.Contains("Crew: 4 of 4", jobs);
                StringAssert.Contains("Sails ashore", jobs);
                Assert.IsTrue(list.Q<VisualElement>("job-crew").ClassListContains("done"), "the crew is aboard and its row is not ticked");
                Assert.IsFalse(list.Q<VisualElement>("job-sail").ClassListContains("done"), "the sail is ashore and its row is ticked");
                yield return play.Shot("open_15b_jobs_half_done");
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
                // the list holds on until she sails: the sail and the crew ticked, the course still to set
                Assert.IsTrue(FirstJobs.Listed, "the list is done with before she has cast off");
                Assert.IsTrue(play.Frame.JobsShown);
                jobs = ChartPlay.Read(list);
                StringAssert.Contains("Sails mended", jobs);
                StringAssert.Contains("First a course.", jobs);
                Assert.IsTrue(list.Q<VisualElement>("job-sail").ClassListContains("done"));
                yield return play.Shot("open_16_jobs_done");
                // a course set: its row is ticked and the last row is the button that casts off
                string bound = FirstJobs.TipPort.Length > 0 ? FirstJobs.TipPort : "traiectum";
                Assert.IsTrue(Course.SetTo(bound, ChartWorld.ShipWorld(), Port.LyingId()), "no course from home to " + bound);
                yield return ChartPlay.Settle(0.6f);
                jobs = ChartPlay.Read(list);
                StringAssert.Contains("Course: ", jobs);
                StringAssert.Contains("She is ready.", jobs);
                Assert.IsTrue(list.Q<VisualElement>("job-course").ClassListContains("done"));
                Assert.IsTrue(list.Q<VisualElement>("job-cast-off").ClassListContains("ready"));
                yield return play.Shot("open_16b_ready_to_cast_off");
                Course.Clear();
                yield return ChartPlay.Settle(0.4f);

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
                // ---- 14. WHO IS SPEAKING ON A CARD (8 Oct 2026; the user, by the picker: "Fix the cards first"). Until then a
                // card showed every line under the face and name of its first speaker, and Ravn spoke under a stranger's name.
                var director = EncounterRunner.Director;
                Assert.IsNotNull(director, "the cards have no director");
                Assert.IsTrue(director.Offer("harbour.first_question.dorestad"), "Ravn's first question cannot come up at Dorestat on the first morning");
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the card of the first question did not open");
                yield return ChartPlay.Settle(0.8f);
                StringAssert.StartsWith("Ravn, ", play.Frame.SpeakerShown, "Ravn's own card is not said under his name");
                notes.AppendLine("On his own card: " + play.Frame.SpeakerShown);
                yield return play.Shot("open_20_ravn_on_his_card");
                yield return AnswerCard(play, notes);
                // a card of two speakers: the name under the title follows the one who says the line
                Assert.IsTrue(director.Offer("harbour.kings_mint.dorestad"), "the mint's card cannot come up at Dorestat on the first morning");
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the mint's card did not open");
                yield return ChartPlay.Settle(0.8f);
                var spoke = new List<string>();
                for (int press = 0; press < 20; press++)
                {
                    string now = play.Frame.SpeakerShown;
                    if (now.Length > 0 && !spoke.Contains(now))
                    {
                        spoke.Add(now);
                        yield return play.Shot("open_21_card_speaker_" + spoke.Count, 0.2f);
                    }
                    if (play.Frame.FrontReveal == null || play.Frame.FrontReveal.Done)
                        break;
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                notes.AppendLine("On the mint's card spoke: " + string.Join(" | ", spoke));
                Assert.GreaterOrEqual(spoke.Count, 2, "the name under the title did not change with the speaker: " + string.Join(" | ", spoke));
                yield return AnswerCard(play, notes);

                // ---- 15. WHAT CAME OF A CHOICE, AS IT IS WRITTEN (8 Oct 2026, night). The script's paragraphs of what
                // came of a choice were never shown: a card answered said "You did X." and the effects' facts.
                Assert.IsTrue(director.Offer("harbour.chain_black_stones.s01_unsold_stones"), "the card of the unsold stones cannot come up at Dorestat on the first morning");
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the card of the unsold stones did not open");
                yield return ChartPlay.Settle(0.6f);
                for (int i = 0; i < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return ChartPlay.Settle(0.3f);
                var session = director.Current;
                Assert.IsNotNull(session, "the cards' director has no card up");
                int at = session.options.FindIndex(o => o.enabled && o.def.Chance == null && Outcomes.Text(session.world.book, o.def, "").Length > 0);
                Assert.GreaterOrEqual(at, 0, "the card of the unsold stones has no choice with a written end that can be made");
                string end = Outcomes.Text(session.world.book, session.options[at].def, "");
                var stones = play.Choices(play.FrontFloat());
                Assert.AreEqual(session.options.Count, stones.Count, "the window does not show the card's choices one for one");
                yield return play.Shot("open_22_card_with_an_end");
                yield return play.Click(stones[at].button, "the choice '" + stones[at].words + "'");
                yield return ChartPlay.Settle(0.5f);
                // (the end is said a letter at a time: a press finishes the paragraph, and it waits)
                for (int i = 0; i < 3 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                string shown = ChartPlay.Read(play.FrontFloat());
                StringAssert.Contains(end.Substring(0, System.Math.Min(40, end.Length)), shown, "the choice's written end is not on the screen");
                notes.AppendLine("The written end: " + ChartPlay.Short(end));
                yield return play.Shot("open_23_the_written_end");
                for (int i = 0; i < 30 && play.Frame.FloatOpen("card"); i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                    if (i == 0 && play.Frame.FloatOpen("card"))
                        yield return play.Shot("open_24_and_its_facts", 0.2f);
                }
                Assert.IsFalse(play.Frame.FloatOpen("card"), "the card could not be gone on from after its end was told");

                // ---- 15b. A CARD OF THE BALTIC WITH ITS OWN WORDS (the road's step 1, 8 Oct 2026, night). The words of five
                // regions were never brought into the game: such a card showed "[write: key]" where each line should be.
                // (Put in line by hand: its port is Birka.)
                director.state.Enqueue("harbour.jetty_berth.birka", true);
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the Baltic's card did not open");
                yield return ChartPlay.Settle(0.8f);
                // (his first line, said to its last letter: a press finishes the paragraph, and it waits)
                if (play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                string baltic = ChartPlay.Read(play.FrontFloat());
                StringAssert.DoesNotContain("[write:", baltic, "the Baltic's card still shows a placeholder for its words");
                StringAssert.Contains("You lie where I say you lie", baltic, "the keeper of Birka does not say his own first line");
                yield return play.Shot("open_24b_a_card_of_the_baltic");
                for (int i = 0; i < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return ChartPlay.Settle(0.3f);
                StringAssert.DoesNotContain("[write:", ChartPlay.Read(play.FrontFloat()), "the Baltic's card shows a placeholder for a later line");
                var berth = director.Current;
                int gift = berth.card.Option("gift").index;
                var berthChoices = play.Choices(play.FrontFloat());
                int taken = berth.options[gift].enabled ? gift : berth.options.FindIndex(o => o.enabled);
                string berthEnd = Outcomes.Text(berth.world.book, berth.options[taken].def, "");
                Assert.IsNotEmpty(berthEnd, "the choice taken on the Baltic's card has no written end");
                yield return play.Click(berthChoices[taken].button, "the choice '" + berthChoices[taken].words + "'");
                yield return ChartPlay.Settle(0.5f);
                for (int i = 0; i < 3 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                StringAssert.Contains(berthEnd.Substring(0, System.Math.Min(40, berthEnd.Length)), ChartPlay.Read(play.FrontFloat()), "the end of the Baltic's choice is not on the screen");
                yield return play.Shot("open_24c_and_its_end");
                for (int i = 0; i < 30 && play.Frame.FloatOpen("card"); i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                Assert.IsFalse(play.Frame.FloatOpen("card"), "the Baltic's card could not be gone on from");

                // ---- 16. THE DEBT HAS AN END (8 Oct 2026, night; DECISIONS 33 and 104). The lender's man asks for
                // something on the stick, and a part paid comes off it; on the day the lender asks for all of it, and
                // unpaid she works for him. (Both cards are put in line by hand here: his man comes from the tenth day
                // of a game, and the day is Michaelmas.)
                Trade.Purse.Add("denier", 300.0);
                director.state.Enqueue("harbour.wokerers_man.home", true);
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the card of the lender's man did not open");
                yield return ChartPlay.Settle(0.8f);
                var hisCard = director.Current;
                Assert.AreEqual("harbour.wokerers_man.home", hisCard.card.id);
                string hisName = hisCard.People.First().name;
                StringAssert.StartsWith(hisName + ", ", play.Frame.SpeakerShown, "the lender's man is not under his own name");
                yield return play.Shot("open_25_the_lenders_man");
                for (int i = 0; i < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return ChartPlay.Settle(0.3f);
                int pound = hisCard.card.Option("pound").index;
                Assert.IsTrue(hisCard.options[pound].enabled, "a pound cannot be paid with three hundred pennies more in the purse: " + hisCard.options[pound].rule);
                double owedBefore = Trade.Debt;
                var hisChoices = play.Choices(play.FrontFloat());
                yield return play.Shot("open_26_what_he_can_be_told");
                yield return play.Click(hisChoices[pound].button, "the choice '" + hisChoices[pound].words + "'");
                yield return ChartPlay.Settle(0.5f);
                for (int i = 0; i < 3 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                Assert.AreEqual(owedBefore - 240.0, Trade.Debt, 0.01, "a pound paid to his man did not come off the stick");
                yield return play.Shot("open_27_a_notch_cut");
                for (int i = 0; i < 30 && play.Frame.FloatOpen("card"); i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                notes.AppendLine("Paid " + hisName + " a pound: owed " + Trade.Debt.ToString("0") + " d. The bar: " + FrameGame.DebtLine());
                // the day (made today for this, by the back door: the game itself never moves it)
                Assert.IsFalse(Trade.Working);
                typeof(Trade).GetProperty(nameof(Trade.DebtDue)).GetSetMethod(true).Invoke(null, new object[] { System.Math.Floor(KapmanGame.Current.Day) });
                Assert.AreEqual(0.0, Trade.DebtDaysLeft(KapmanGame.Current.Day), "the day was not made today");
                director.state.Enqueue("harbour.wokerers_day.home", true);
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 12f);
                Assert.IsTrue(play.Frame.FloatOpen("card"), "the card of the day did not open");
                yield return ChartPlay.Settle(0.8f);
                var theDay = director.Current;
                StringAssert.StartsWith(Trade.LenderName + ", ", play.Frame.SpeakerShown, "the lender is not under his own name on the day");
                yield return play.Shot("open_28_two_men_and_a_rope");
                for (int i = 0; i < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return ChartPlay.Settle(0.3f);
                int shortOfIt = theDay.card.Option("short").index;
                var dayChoices = play.Choices(play.FrontFloat());
                yield return play.Click(dayChoices[shortOfIt].button, "the choice '" + dayChoices[shortOfIt].words + "'");
                yield return ChartPlay.Settle(0.5f);
                for (int i = 0; i < 3 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                Assert.IsTrue(Trade.Working, "the day went unpaid and she does not work for the lender");
                yield return play.Shot("open_29_she_works_for_him");
                for (int i = 0; i < 30 && play.Frame.FloatOpen("card"); i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(6);
                }
                yield return ChartPlay.Settle(0.4f);
                StringAssert.Contains("she works for", FrameGame.DebtLine(), "the bar does not say whom she works for");
                notes.AppendLine("After the day: " + FrameGame.DebtLine());
                yield return play.Shot("open_30_the_bar_after_the_day");

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
