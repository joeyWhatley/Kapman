using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings");
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
                Assert.AreEqual(71, book.cards.Count, "the cards in a new game (the toll, the lender's seven, the sea's 63): " + inNow);
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
    }
}
