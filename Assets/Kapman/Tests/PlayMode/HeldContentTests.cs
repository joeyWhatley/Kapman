using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE GAME AS IT IS FOR NOW: ONLY WHAT IS POLISHED (the user, 9 Oct 2026: "for now we don't include any fully
    /// fleshed out quests/ cards until they're polished, only keep in the good polished stuff for now and we will add
    /// in the other content when it's all done"). The other play tests have the whole book, for the samples they show
    /// the cards' machinery on (ContentHold.AllIn). This one plays the game as a player has it now: a new game, with
    /// the quests, the ports' cards, the dated cards and the crew's held back. The book is the cards that are in; a held card cannot be
    /// asked for; she casts off and sails two days down the Lek, and every card that comes up on the way is one that
    /// is in; nothing a reader would fault is on the screen.
    /// </summary>
    public class HeldContentTests
    {
        static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        static IEnumerator AnswerAll(ChartPlay play)
        {
            for (int i = 0; i < 12 && play.FrontFloat() != null; i++)
            {
                var front = play.FrontFloat();
                for (int k = 0; k < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; k++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                var pick = play.Choices(front).FirstOrDefault(c => c.can);
                if (pick.button != null)
                    yield return play.Click(pick.button, pick.words);
                else
                {
                    var cross = front.Q<Button>(className: "frame-float-shut");
                    if (cross == null)
                        yield break;
                    yield return play.Click(cross, "the cross");
                }
                yield return ChartPlay.Frames(4);
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator ANewGameHasOnlyTheCardsThatAreIn()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_held_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            var load = SceneManager.LoadSceneAsync("Kapman", LoadSceneMode.Single);
            Assert.IsNotNull(load, "the Kapman scene is not in the build settings");
            while (!load.isDone)
                yield return null;
            yield return ChartPlay.Frames(5);
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = saves;
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var play = ChartPlay.Begin(false);
            try
            {
                yield return ChartPlay.Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                yield return ChartPlay.Frames(30);
                var view = PortView.Current;
                if (view != null && view.Shown)
                {
                    yield return play.Click(view.Flip, "Chart");
                    yield return ChartPlay.Frames(6);
                }
                var game = KapmanGame.Current;

                // ---- the book of a new game: the cards that are in, and no others
                Assert.IsFalse(ContentHold.AllIn, "a test's switch is on in a new game");
                Assert.IsTrue(ContentHold.On, "nothing is held back in a new game");
                var book = EncounterRunner.Book;
                var director = EncounterRunner.Director;
                Assert.IsNotNull(book, "the game has no book of cards");
                Assert.IsNotNull(director, "with the rest held back the cards have no director: the book has faults");
                Assert.IsEmpty(EncounterRunner.ShutFiles);
                string inNow = string.Join(", ", book.cards.Select(c => c.id));
                Assert.AreEqual(78, book.cards.Count, "the cards in a new game (the toll, the lender's fourteen, the sea's 63): " + inNow);
                Assert.GreaterOrEqual(EncounterRunner.HeldFiles.Count, 50, "the card files held back");
                foreach (var c in book.cards)
                    Assert.IsTrue(ContentHold.CardIn(c.file, c.id), c.id + " is in the game and the table does not name it");
                // a held card cannot be asked for, however right the place and the day
                foreach (string held in new[] { "harbour.first_question.dorestad", "harbour.kings_mint.dorestad", "harbour.chain_black_stones.s01_unsold_stones" })
                {
                    Assert.IsNull(book.Card(held), held + " is in a new game's book");
                    Assert.IsFalse(director.Offer(held), held + " is held back and can still be asked for");
                }
                yield return ChartPlay.Frames(20);
                Assert.IsNull(director.Current, "a card came up at home on the first morning: " + director.Current?.card.id);

                // ---- she sails: every card that comes up is one that is in
                var came = new List<string>();
                Assert.IsTrue(Course.SetTo("witla", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Witla");
                yield return ChartPlay.Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return play.Click(Small(play, "5"), "speed 5");
                double day0 = game.Day;
                float wall = Time.realtimeSinceStartup;
                while (game.Day - day0 < 2.0 && Time.realtimeSinceStartup - wall < 240f && Port.LyingId() != "witla")
                {
                    var open = director.Current;
                    if (open != null && (came.Count == 0 || came[came.Count - 1] != open.card.id))
                        came.Add(open.card.id);
                    if (play.FrontFloat() != null)
                    {
                        yield return AnswerAll(play);
                        if (GameSpeed.Level < 4 && play.FrontFloat() == null)
                            yield return play.Click(Small(play, "5"), "speed 5");
                    }
                    yield return null;
                }
                Debug.Log($"Held content: {book.cards.Count} cards in the game ({inNow}); {EncounterRunner.HeldFiles.Count} card files held back. " +
                          $"In {game.Day - day0:0.0} days down the Lek these came up: {(came.Count > 0 ? string.Join(", ", came) : "none")}; she lies at {Port.LyingId() ?? "sea"}.");
                foreach (string id in came)
                    Assert.IsTrue(ContentHold.CardIn(book.Card(id).file, id), "a card that is held back came up: " + id);
                Assert.Greater(game.Day - day0, 0.3, "she did not sail");
                yield return play.Click(Small(play, "Pause"), "Pause");
                yield return AnswerAll(play);
                yield return play.Shot("held_1_two_days_out");
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }

        /// <summary>
        /// THE KING'S TOLL AS SHE ANSWERED THE LAST TIME, IN THE FRAME (the user by the picker, 10 Oct 2026: "The first
        /// time at a port you answer the card. After that, if you declared your cargo or showed a charter last time, the
        /// toll is taken the same way and the log says what was paid."). She lies at Dorestat in a new game, and the
        /// test brings her in twice more (it ends the call and begins it again, as an arrival does):
        ///   - the first time the toll man's card comes up, and she declares her cargo;
        ///   - the second time no card comes: the log says with whom it went as it went the last time, and the Port
        ///     window has a line for it;
        ///   - pressed, that line brings the card up, and she answers it by her own hand; the line is gone, and
        ///     nothing a reader would fault is on the screen.
        /// (The director's side, and when the card comes all the same, are CardClockTests'. Pictures in Captures/toll.)
        /// </summary>
        [UnityTest, Timeout(900000)]
        public IEnumerator TheTollGoesAsItWentAndItsLineBringsTheCardBack()
        {
            const string Toll = "harbour.tollman.frankish";
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_toll_saves", p => play = p, "toll");
            EncounterDirector cards = null;
            int asBefore = 0;
            System.Action<EncounterSession, ChoiceResult> onAsBefore = (s, r) => asBefore++;
            try
            {
                var frame = play.Frame;
                cards = EncounterRunner.Director;
                Assert.IsNotNull(cards, "no cards in the game");
                Assert.IsNotNull(EncounterRunner.Book.Card(Toll), "the toll man's card is not in the game");
                var here = FrameGame.PortHere;
                Assert.IsNotNull(here, "a new game does not lie at a port");
                yield return AnswerAll(play);
                cards.AnsweredAsBefore += onAsBefore;

                // ---- she comes in: the first time the toll man's card comes up, and she declares her cargo
                if (cards.InCall)
                    cards.EndCall();
                cards.BeginCall(here.id);
                yield return ChartPlay.Until(() => GoonsTests.TheirCardUp(Toll) && frame.FloatOpen("card"), 15f);
                Assert.IsTrue(GoonsTests.TheirCardUp(Toll), "the first time at " + here.name + " the toll man's card did not come up: " +
                                                            (cards.Current != null ? cards.Current.card.id : "no card is open"));
                string who = cards.Current.People.First().name;
                string said = "", end = "";
                yield return GoonsTests.Answer(play, "Declare the cargo", (a, b) => { said = a; end = b; });
                yield return AnswerAll(play);
                Assert.AreEqual(0, asBefore, "the first time at a port the toll was taken as before");
                Assert.IsEmpty(cards.AsBeforeThisCall);
                Assert.IsNull(play.ButtonStarting("It went as it went the last time", frame.Root), "the Port window has the line before anything went by itself");

                // ---- she comes in again: no card, the log says how it went, and the Port window has its line
                cards.EndCall();
                cards.BeginCall(here.id);
                yield return ChartPlay.Frames(40);
                yield return ChartPlay.Settle(0.5f);
                Assert.IsNull(cards.Current, "she declared the last time, and a card came up again: " + (cards.Current != null ? cards.Current.card.id : ""));
                Assert.IsFalse(frame.FloatOpen("card"), "a card's window is open");
                Assert.AreEqual(1, asBefore, "the toll was not taken as she answered the last time");
                string line = FrameLog.Lines.Select(l => l.Whole).LastOrDefault(t => t.Contains("it goes as it went the last time")) ?? "";
                StringAssert.StartsWith("With " + who + " it goes as it went the last time. ", line, "the log does not say with whom the toll went as it went the last time");
                // (10 Oct 2026, the log's short form: the line is a few words and the number that changed; its whole
                // telling comes up in a box when the pointer rests on it)
                var logged = FrameLog.Lines.Last(l => l.Whole.Contains("it goes as it went the last time"));
                StringAssert.EndsWith(" as before", logged.text, "the log's line for it is not the short one: " + logged.text);
                Assert.IsNotNull(logged.box, "the short line has no box");
                StringAssert.Contains("With " + who + " it goes as it went the last time.", logged.box.Text());
                var logRow = frame.Root.Query<VisualElement>(className: "frame-log-line").ToList()
                                  .LastOrDefault(r => r.Q<Label>("text") != null && r.Q<Label>("text").text == FrameLog.Shown(logged.text));
                Assert.IsNotNull(logRow, "the log does not show the line '" + logged.text + "'");
                if (logged.figure.Length > 0)
                    Assert.AreEqual(logged.figure, logRow.Q<Label>("figure")?.text, "the number that changed is not at the line's end");
                play.MoveTo(logRow.worldBound.center);
                yield return ChartPlay.Settle(1.0f);
                StringAssert.Contains("With " + who + " it goes as it went the last time.", HoverCards.Shown, "resting the pointer on the line did not bring up its box");
                StringAssert.Contains(logged.box.head, HoverCards.Shown);
                yield return play.Shot("toll_1b_the_box_over_the_line_of_the_log");
                play.MoveTo(new Vector2(700f, 420f));
                yield return ChartPlay.Settle(0.3f);
                Assert.AreEqual(1, cards.AsBeforeThisCall.Count);
                frame.Show("port");
                yield return ChartPlay.Settle(0.6f);
                var row = play.ButtonStarting("It went as it went the last time", frame.Root);
                Assert.IsNotNull(row, "the Port window has no line for the toll that went by itself:\n" + ChartPlay.Read(frame.Root));
                StringAssert.Contains(cards.AsBeforeThisCall[0].title, ChartPlay.Words(row), "the line does not name the card");
                yield return play.Shot("toll_1_it_went_as_it_went_the_last_time");

                // ---- pressed, the line brings the card up, and she answers it by her own hand
                yield return play.Click(row, "the toll's line");
                yield return ChartPlay.Until(() => GoonsTests.TheirCardUp(Toll) && frame.FloatOpen("card"), 15f);
                Assert.IsTrue(GoonsTests.TheirCardUp(Toll), "pressed, the toll's line did not bring the card up");
                Assert.IsFalse(cards.Current.Chosen, "asked for, the card answered itself again");
                Assert.IsEmpty(cards.AsBeforeThisCall, "the line's card is still listed after it was asked for");
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("toll_2_the_card_had_again");
                yield return GoonsTests.Answer(play, "Declare the cargo", (a, b) => { said = a; end = b; });
                yield return AnswerAll(play);
                Assert.AreEqual(1, asBefore, "answered by her own hand, it was counted as answered by itself");
                Assert.IsNotEmpty(said, "the card that was had again said nothing");
                frame.Show("port");
                yield return ChartPlay.Settle(0.5f);
                Assert.IsNull(play.ButtonStarting("It went as it went the last time", frame.Root), "the toll's line stands in the Port window after it was pressed");
                Assert.IsNull(cards.Current, "a card is left open");
                yield return play.Shot("toll_3_answered_by_her_own_hand");
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                if (cards != null)
                    cards.AnsweredAsBefore -= onAsBefore;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
