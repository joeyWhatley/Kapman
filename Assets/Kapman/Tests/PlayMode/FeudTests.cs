using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// RAVN'S DEATH AND THE FEUD, AS A PLAYER HAS IT (the user, 9 Oct 2026: "Ravn needs an impactful death scene as genuine
    /// punishment for not repaying the debt but opens up a bloodfued opportunity"; by the picker: the feud is "You
    /// against the lender", it ends in "Blood or silver", and "All of it, live").
    /// A new game, WITH THE SHOW ON (the talking text, and the scene: the pictures are what a player sees). The rope men
    /// stand at her berth for the third time she gives them nothing. Their card says its lines one at a time; its end
    /// only turns to Ravn. When it is shut the screen goes black and his death is told a line at a time, the clock
    /// held; then his name and the day. Over him she swears his blood; he is carried aboard. The Errands and Crew
    /// windows say how it stands, and the crew's heart will not rise. The rope men come again: the one who did it is
    /// put before her first, on a card of his own, and she kills him: it is answered, the port's lord fines it, the
    /// ceiling and the mark lift, and the rest are gone. At sea the men find a shore, and he has his fire. Pictures and
    /// what was said in Captures/feud.
    /// </summary>
    public class FeudTests
    {
        static IEnumerator ReadOut(ChartPlay play)
        {
            for (int i = 0; i < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
            yield return ChartPlay.Settle(0.5f);
        }

        /// <summary>The card on screen, WITH THE SHOW ON: its lines read out one at a time, the choice whose words begin
        /// so taken, and what came of it read a paragraph at a time (every one, into `end`). The card is then shut.</summary>
        static IEnumerator Answer(ChartPlay play, string begins, System.Action<string, string> read)
        {
            yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 15f);
            Assert.IsTrue(play.Frame.FloatOpen("card"), "the card is first in line and did not open");
            var said = new List<string>();
            for (int i = 0; i < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
            {
                said.Add(ChartPlay.Read(play.FrontFloat()));
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
            yield return ChartPlay.Settle(0.4f);
            said.Add(ChartPlay.Read(play.FrontFloat()));
            var choices = play.Choices(play.FrontFloat());
            var pick = choices.FirstOrDefault(c => c.words.StartsWith(begins, System.StringComparison.Ordinal));
            Assert.IsNotNull(pick.button, "the card has no choice that begins with these words: " + begins + ": " + string.Join(" / ", choices.Select(c => c.words)));
            Assert.IsTrue(pick.can, "this choice cannot be taken: " + begins);
            yield return play.Click(pick.button, pick.words);
            yield return ChartPlay.Settle(0.5f);
            var end = new List<string>();
            for (int i = 0; i < 24 && play.FrontFloat() != null; i++)
            {
                end.Add(ChartPlay.Read(play.FrontFloat()));
                var reveal = play.Frame.FrontReveal;
                if (reveal == null || reveal.Done)
                    break;
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(6);
            }
            read(string.Join(" | ", said.Distinct()), string.Join(" | ", end.Distinct()));
            for (int i = 0; i < 40 && play.Frame.FloatOpen("card"); i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(6);
            }
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator RavnDiesInASceneAndTheFeudIsHersToTakeUp()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_feud_saves", p => play = p, "feud");
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;
            var told = new StringBuilder();
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "feud");
            Directory.CreateDirectory(dir);
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;
                var cards = EncounterRunner.Director;
                var rules = TradeRules.Current;
                Assert.IsNotNull(cards, "no cards in the game");
                foreach (string id in new[] { Feud.OathCard, Feud.BurialCard, Feud.PyreCard, Feud.KillerCard, Feud.DoorCard, Feud.CourtCard, Feud.OutlawCard })
                    Assert.IsNotNull(EncounterRunner.Book.Card(id), id + " is not in the game");
                // (a game begun by the menu has no lender: the opening sets that flag, and the test sets it as the opening does)
                ContentFlags.Set(Trade.LenderFlag);
                Assert.IsTrue(Trade.Started && Trade.Debt > 1000.0, "a new game owes nothing for the boat");
                Assert.AreEqual(FeudStage.None, Feud.Stage);
                Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9);

                // a table of the test's own: the men look for a shore an hour out, not six
                var table = FeudRules.Read(ContentFiles.ParseObject(Feud.File));
                Assert.IsTrue(table.on);
                table.pyreAfterHours = 1.0;
                Feud.Use(table);
                Voyage.SetMen(4);
                Voyage.Rest(0.0, 1.0);
                Trade.Purse.Add("denier", 400.0);
                var home = GameData.Port(Collectors.Rules.home);
                var quay = GoonsTests.At(home.lat, home.lon);
                Assert.Greater(Voyage.Morale, 0.9);

                // THE SHOW IS ON from here: cards say their lines one at a time, and a scene can be told
                Showtime.ForceOn = true;
                yield return ChartPlay.Frames(20);
                var show = Showtime.Current;
                Assert.IsNotNull(show, "there is no show in the game");
                Assert.IsFalse(Showtime.Busy, "turned on in a running game, the show began something of its own: " + show.Stage);

                // ======== 1. THE THIRD TIME: their card, as it is played; given nothing, its end turns to Ravn
                Collectors.Refused(2);
                Collectors.PutOut(3, quay, 2.0, home.id);
                yield return GoonsTests.Run(play, game, 3.0, () => GoonsTests.TheirCardUp(Collectors.QuayCard));
                Assert.IsTrue(GoonsTests.TheirCardUp(Collectors.QuayCard), "they are at the quay and their card did not come");
                yield return ChartPlay.Until(() => frame.FloatOpen("card"), 15f);
                yield return ChartPlay.Settle(0.3f);
                Assert.IsNotNull(frame.FrontReveal, "with the show on, the card does not say its lines");
                Assert.IsTrue(frame.FrontReveal.OneAtATime, "the card shows all its lines at once");
                yield return play.Press(Key.Enter);                         // (the first line, said in full)
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("feud_0_their_card_as_it_is_played");
                string said3 = "", end3 = "";
                yield return Answer(play, "Tell them you have not got it", (s, e) => { said3 = s; end3 = e; });
                told.AppendLine("the third time, end: " + ChartPlay.Short(end3));
                StringAssert.Contains("looks at Ravn", end3, "the third time she gave them nothing, and the card does not turn to Ravn");
                StringAssert.DoesNotContain("knife", end3, "the card tells his death, and the scene tells it again");
                Assert.IsTrue(Collectors.RavnGone, "Ravn is dead and no flag says so");
                Assert.AreEqual(3, Voyage.Men);
                Assert.AreEqual(FeudStage.Scene, Feud.Stage, "Ravn is dead and no scene is owed him");

                // ======== 2. THE SCENE: black, a line at a time, the clock held; then his name and the day
                double hours = game.Hours;
                yield return ChartPlay.Until(() => Showtime.Telling, 10f);
                Assert.IsTrue(Showtime.Telling, "their card is shut, Ravn is dead, and no scene began");
                Assert.IsTrue(Showtime.Busy);
                var lines = Feud.SceneLines();
                Assert.GreaterOrEqual(lines.Count, 5, "the scene has no lines of the writing's");
                told.AppendLine("the scene: " + lines.Count + " lines");
                for (int i = 1; i <= lines.Count; i++)
                {
                    int n = i;
                    yield return ChartPlay.Until(() => show.SceneLine == n, 15f);
                    Assert.AreEqual(n, show.SceneLine, "line " + n + " of the scene did not come:\n" + told);
                    Assert.AreEqual(lines[n - 1], show.SceneText);
                    Assert.IsFalse(frame.FloatOpen("card"), "a card opened under the scene");
                    // (it is said a letter at a time: a key says it in full, and then it waits for the next key)
                    yield return ChartPlay.Settle(0.25f);
                    if (!show.SceneWaits)
                    {
                        yield return play.Press(Key.Enter);
                        yield return ChartPlay.Until(() => show.SceneWaits, 5f);
                    }
                    Assert.IsTrue(show.SceneWaits, "line " + n + " never stood whole");
                    yield return ChartPlay.Settle(0.4f);
                    Assert.AreEqual(n, show.SceneLine, "the scene went on by itself: a line is not waited for");
                    told.AppendLine("  " + n + ": " + show.SceneText);
                    if (n == 1 || n == 4 || n == 6 || n == lines.Count)
                        yield return play.Shot("feud_1_scene_line_" + n);
                    yield return play.Press(Key.Enter);
                }
                yield return ChartPlay.Until(() => show.CardText.Length > 0, 8f);
                yield return ChartPlay.Settle(0.9f);
                told.AppendLine("his card: " + show.CardText);
                StringAssert.StartsWith("Ravn / Killed at " + home.name, show.CardText, "his name and where he died do not stand at the scene's end");
                yield return play.Shot("feud_2_his_name_and_the_day");
                yield return ChartPlay.Until(() => !Showtime.Busy, 15f);
                Assert.IsFalse(Showtime.Busy, "the scene never ended");
                Assert.AreEqual(hours, game.Hours, 0.05, "the clock ran under the scene");
                Assert.AreEqual(FeudStage.Oath, Feud.Stage);
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text.Contains("Ravn is dead")), "the log does not keep that Ravn is dead");
                // the punishment, from that minute
                Assert.LessOrEqual(Voyage.Morale, table.heartCeiling + 1e-9, "their heart is over its ceiling");
                Assert.IsTrue(Self.HasMark(Feud.Mark), "no mark lies on her");

                // ======== 3. OVER HIM: she swears his blood
                yield return ChartPlay.Until(() => GoonsTests.TheirCardUp(Feud.OathCard), 15f);
                Assert.IsTrue(GoonsTests.TheirCardUp(Feud.OathCard), "the scene is over, and she does not stand over him");
                yield return ChartPlay.Until(() => frame.FloatOpen("card"), 15f);
                yield return ReadOut(play);
                yield return play.Shot("feud_3_over_ravn");
                string saidOath = "", endOath = "";
                yield return Answer(play, "Swear his blood", (s, e) => { saidOath = s; endOath = e; });
                told.AppendLine("over him, said: " + ChartPlay.Short(saidOath));
                told.AppendLine("over him, end: " + ChartPlay.Short(endOath));
                StringAssert.Contains("Ten years", saidOath, "his friend of ten years says nothing over him");
                Assert.IsTrue(Feud.IsSworn, "she swore it, and nothing is sworn");
                Assert.AreEqual(FeudStage.Open, Feud.Stage);

                // ======== 4. WHERE HE LIES: carried aboard, for a fire on a shore of his own
                yield return ChartPlay.Until(() => GoonsTests.TheirCardUp(Feud.BurialCard), 15f);
                Assert.IsTrue(GoonsTests.TheirCardUp(Feud.BurialCard), "where he lies was not asked");
                yield return ChartPlay.Until(() => frame.FloatOpen("card"), 15f);
                yield return ReadOut(play);
                yield return play.Shot("feud_4_where_he_lies");
                string saidLie = "", endLie = "";
                yield return Answer(play, "Carry him aboard", (s, e) => { saidLie = s; endLie = e; });
                told.AppendLine("where he lies, said: " + ChartPlay.Short(saidLie));
                told.AppendLine("where he lies, end: " + ChartPlay.Short(endLie));
                StringAssert.Contains("priest", saidLie, "at a quay, nobody speaks of the priest");
                Assert.IsTrue(Feud.LiesAboard);
                Assert.AreEqual(0.0, Feud.Var("feud.buried"));

                // ======== 5. HOW IT STANDS: the Errands window, the Crew window, and their heart will not rise
                yield return ChartPlay.Until(() => play.FrontFloat() == null, 8f);
                frame.Show("tasks");
                yield return ChartPlay.Settle(0.8f);
                string errands = ChartPlay.Read(play.Screen);
                StringAssert.Contains("You swore his blood", errands, "the Errands window does not say how it stands with Ravn");
                StringAssert.Contains("He lies aboard", errands);
                StringAssert.DoesNotContain("in good heart", errands, "the Errands window says what the Crew window is there to say");
                yield return play.Shot("feud_5_errands");
                frame.Show("crew");
                yield return ChartPlay.Settle(0.8f);
                StringAssert.Contains("while Ravn lies unanswered", ChartPlay.Read(play.Screen), "the Crew window does not say why their heart is held down");
                yield return play.Shot("feud_5_crew");
                Voyage.Rest(0.0, 1.0);
                Assert.LessOrEqual(Voyage.Morale, table.heartCeiling + 1e-9, "a rest lifts their heart over its ceiling");

                // ======== 6. BLOOD: the rope men come again, and she kills the one who did it
                double silver = Trade.Purse.Silver(rules);
                cards.roll = () => 0.0;                                      // (the fight goes her way: a card's dice are taken when it opens)
                Collectors.PutOut(4, quay, 3.0, home.id);
                yield return GoonsTests.Run(play, game, 3.0, () => GoonsTests.TheirCardUp(Feud.KillerCard), 120f, ".feud.");
                Assert.IsTrue(GoonsTests.TheirCardUp(Feud.KillerCard), "they are at the quay again, and the one who did it is not put before her first: " +
                                                                        (cards.Current != null ? cards.Current.card.id : "no card"));
                Assert.AreEqual(GoonState.Quay, Collectors.State);
                yield return ChartPlay.Until(() => frame.FloatOpen("card"), 15f);
                yield return ReadOut(play);
                yield return play.Shot("feud_6_the_one_who_did_it");
                string said4 = "", end4 = "";
                yield return Answer(play, "Kill him", (s, e) => { said4 = s; end4 = e; });
                cards.roll = null;
                told.AppendLine("the one who did it, said: " + ChartPlay.Short(said4));
                told.AppendLine("the one who did it, end: " + ChartPlay.Short(end4));
                File.WriteAllText(Path.Combine(dir, "feud.txt"), told.ToString());
                StringAssert.Contains("I remember", said4);
                // they are gone, and their own card is gone out of the line with them
                yield return ChartPlay.Settle(1.0f);
                Assert.IsFalse(cards.Waiting, "one of them is dead, the rest are gone, and their card still waits in line");
                Assert.IsFalse(GoonsTests.TheirCardUp(Collectors.QuayCard), "one of them is dead, and the rest ask for a pound");
                StringAssert.Contains("It is answered", end4, "she killed him and it is not answered");
                StringAssert.Contains("blood shed on his quay", end4, "blood was shed in the count's port and nobody fined it");
                Assert.AreEqual(FeudStage.Settled, Feud.Stage);
                Assert.AreEqual(1, Feud.Ended);
                Assert.IsFalse(Feud.KillerAlive);
                Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9, "it is answered, and their heart is held down still");
                Assert.IsFalse(Self.HasMark(Feud.Mark), "it is answered, and the mark lies on her still");
                Assert.AreEqual(silver - table.townFineD, Trade.Purse.Silver(rules), 6.0, "the port's lord took no fine, or another sum");
                Assert.AreEqual(3, Voyage.Men, "the fight was won and a man of hers is dead");
                Assert.That(Collectors.State, Is.EqualTo(GoonState.Home).Or.EqualTo(GoonState.Homing), "one of them is dead and the rest stay");
                Voyage.Rest(0.0, 1.0);
                Assert.Greater(Voyage.Morale, 0.9, "it is answered, and a rest does not lift their heart");
                Assert.IsTrue(Feud.LiesAboard, "he was carried aboard and is gone from her");

                // ======== 7. HIS FIRE: at sea the men find a shore
                SeaPush.Fixed = at => (0.0, 0.0);                             // (a still sea: this is no test of the tide)
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                // far out, sixty km from any land: the hours pass and no shore is looked for (there is none)
                var sea = GoonsTests.At(52.5, 3.0);
                var u = FloatingOrigin.ToUnity(new WorldPos(sea.x, 0.0, sea.z));
                game.ship.Teleport(new Vector3(u.x, 0f, u.z), 270f);
                game.ship.anchored = false;
                Exploration.See(sea.x, sea.z, 45000.0);
                Exploration.Apply();
                Course.Clear();
                Adrift.Forget();
                map.Follow();
                Assert.IsTrue(Course.KnowsWater, "the chart does not know land from water");
                Assert.IsFalse(Feud.ShoreNear(ChartWorld.ShipWorld(), table.shoreKm), "sixty km out there is a shore within " + table.shoreKm + " km");
                Feud.Law("feud_bury_aboard");                                 // (the hours he has lain aboard are counted from here, at sea)
                yield return GoonsTests.Run(play, game, 2.5, () => GoonsTests.TheirCardUp(Feud.PyreCard), 120f, ".feud.");
                Assert.IsFalse(GoonsTests.TheirCardUp(Feud.PyreCard), "in the open sea, with no land in sixty km, the men found a shore to burn him on");
                // off a shore: the first land due east on this latitude, and she lies two km off it
                double x = sea.x;
                while (Course.IsWater(x, sea.z) && x < sea.x + 200000.0)
                    x += 250.0;
                Assert.IsFalse(Course.IsWater(x, sea.z), "no land due east of the open sea at 52.5 N");
                var off = new WorldXZ(x - 2000.0, sea.z);
                u = FloatingOrigin.ToUnity(new WorldPos(off.x, 0.0, off.z));
                game.ship.Teleport(new Vector3(u.x, 0f, u.z), 270f);
                game.ship.anchored = false;
                Exploration.See(off.x, off.z, 30000.0);
                Exploration.Apply();
                Course.Clear();
                Adrift.Forget();
                map.Follow();
                yield return ChartPlay.Frames(10);
                Assert.IsTrue(Feud.ShoreNear(ChartWorld.ShipWorld(), table.shoreKm), "two km off the land there is no shore within " + table.shoreKm + " km");
                told.AppendLine($"off the shore: {WorldXZ.Distance(ChartWorld.ShipWorld(), new WorldXZ(x, sea.z)):0} m from the land due east");
                yield return GoonsTests.Run(play, game, 4.0, () => GoonsTests.TheirCardUp(Feud.PyreCard), 240f, ".feud.");
                Assert.IsTrue(GoonsTests.TheirCardUp(Feud.PyreCard), "he lies aboard, she lies off a shore, and no shore was looked for:\n" + told);
                yield return ChartPlay.Until(() => frame.FloatOpen("card"), 15f);
                yield return ReadOut(play);
                yield return play.Shot("feud_7_a_shore_of_his_own");
                string saidFire = "", endFire = "";
                yield return Answer(play, "Put in, and burn him here", (s, e) => { saidFire = s; endFire = e; });
                told.AppendLine("his fire, said: " + ChartPlay.Short(saidFire));
                told.AppendLine("his fire, end: " + ChartPlay.Short(endFire));
                Assert.IsFalse(Feud.LiesAboard, "he was burned and lies aboard still");
                Assert.AreEqual(2.0, Feud.Var("feud.buried"));
                Assert.IsTrue(Self.HasMark("gave_ravn_his_fire"), "she gave him his fire and it left no mark on her");
            }
            finally
            {
                Showtime.ForceOn = false;
                SeaPush.Fixed = null;
                Feud.Use(null);
                Collectors.Use(null);
                var cards = EncounterRunner.Director;
                if (cards != null)
                    cards.roll = null;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (told.Length > 0)
                    File.WriteAllText(Path.Combine(dir, "feud.txt"), told.ToString());
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
