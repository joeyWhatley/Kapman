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
    /// THE LENDER SENDS HIS MEN, ON THE CHART AS A PLAYER HAS IT (the user, 9 Oct 2026: "Sends his goons to collect", "A
    /// handful of times", "Ports and at sea", "It'll be dynamic you know", "It'll be comedic but also shockingly brutal at
    /// times", "Just clueless brutes", "Maybe it you don't pay up for the 3rd time they kill Ravn?").
    /// A new game, with a table of the test's own whose first sending is today. She lies at home: the rope men come down
    /// to her berth, their card opens, and she gives them nothing: the first time they only talk. The second time they
    /// turn out the purse. The third time the card says who is next, and given nothing they kill Ravn: the log and the
    /// card say so, a man fewer is aboard, and no card that leans on him can come again. Then at sea: their boat comes
    /// in sight astern (the log, the clock, their mark on the chart), comes up with her, and their card holds her.
    /// Pictures and what happened in Captures/goons.
    /// </summary>
    public class GoonsTests
    {
        internal static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }

        /// <summary>The clock runs (speed 2) until `done`, a card that is not theirs answered on the way (nor one whose
        /// id has `alsoLeave` in it: the feud's own test leaves the feud's cards to itself).</summary>
        internal static IEnumerator Run(ChartPlay play, KapmanGame game, double hours, System.Func<bool> done, float wallSeconds = 120f, string alsoLeave = null)
        {
            double from = game.Hours;
            float wall = Time.realtimeSinceStartup;
            while (!done() && game.Hours - from < hours && Time.realtimeSinceStartup - wall < wallSeconds)
            {
                var cards = EncounterRunner.Director;
                bool theirs = cards != null && cards.Current != null &&
                              (cards.Current.card.id.Contains(".goons.") || (alsoLeave != null && cards.Current.card.id.Contains(alsoLeave)));
                if (play.FrontFloat() != null && !theirs)
                    yield return SeaMovesHerTests.AnswerAll(play);
                if (!done() && play.FrontFloat() == null && (GameSpeed.Level < 1 || GameSpeed.Paused))
                    yield return play.Click(SeaMovesHerTests.Small(play, "2"), "speed 2");
                yield return null;
            }
        }

        internal static bool TheirCardUp(string id)
        {
            var cards = EncounterRunner.Director;
            return cards != null && cards.Current != null && cards.Current.card.id == id;
        }

        /// <summary>Their card, open: every line read out, then the choice whose words begin so is taken, and what came of
        /// it is read (into `end`). The card is then shut.</summary>
        internal static IEnumerator Answer(ChartPlay play, string begins, System.Action<string, string> read)
        {
            yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 15f);
            Assert.IsTrue(play.Frame.FloatOpen("card"), "their card is first in line and did not open");
            for (int i = 0; i < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
            yield return ChartPlay.Settle(0.4f);
            string said = ChartPlay.Read(play.FrontFloat());
            var choices = play.Choices(play.FrontFloat());
            var pick = choices.FirstOrDefault(c => c.words.StartsWith(begins, System.StringComparison.Ordinal));
            Assert.IsNotNull(pick.button, "their card has no choice that begins '" + begins + "': " + string.Join(" / ", choices.Select(c => c.words)));
            Assert.IsTrue(pick.can, "the choice '" + begins + "' cannot be taken");
            yield return play.Click(pick.button, pick.words);
            yield return ChartPlay.Settle(0.5f);
            for (int i = 0; i < 6 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Waiting && !play.Frame.FrontReveal.Done; i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(6);
            }
            string end = play.FrontFloat() != null ? ChartPlay.Read(play.FrontFloat()) : "";
            read(said, end);
            for (int i = 0; i < 40 && play.Frame.FloatOpen("card"); i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(6);
            }
        }

        [UnityTest, Timeout(1500000)]
        public IEnumerator TheLendersMenComeAndWhatTheyDoDepends()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_goons_saves", p => play = p, "goons");
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;
            var dropped = new List<ClockReason>();
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
            var told = new StringBuilder();
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "goons");
            Directory.CreateDirectory(dir);
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;
                var cards = EncounterRunner.Director;
                var rules = TradeRules.Current;
                Assert.IsNotNull(cards, "no cards in the game");
                Assert.IsTrue(Trade.Started && Trade.Debt > 1000.0, "a new game owes nothing for the boat");
                // (a game begun by the menu has no lender: the opening sets that flag, and the test sets it as the opening does)
                ContentFlags.Set(Trade.LenderFlag);
                Assert.IsTrue(ContentFlags.Has(Collectors.OpeningFlag), "the lender's flag could not be set");
                Assert.AreEqual(GoonState.Home, Collectors.State, "the lender's men are out on the first morning");
                Assert.IsNotNull(cards.state, "the cards have no state");
                Assert.IsNotNull(EncounterRunner.Book.Card(Collectors.QuayCard), "their card at the quay is not in the game");
                Assert.IsNotNull(EncounterRunner.Book.Card(Collectors.SeaCard), "their card at sea is not in the game");

                // ---- a table of the test's own: the first sending is today, and they see as far by night as by day
                var table = CollectorRules.Read(ContentFiles.ParseObject(Collectors.File));
                Assert.AreEqual(4, table.sendings.Count);
                table.sendings[0].day = game.Day;
                table.sightNightKm = table.sightKm = 8.0;
                table.chaseKn = 9.0;                                        // (and they row faster than she can sail in any wind of the day)
                Collectors.Use(table);
                Voyage.SetMen(4);                                           // (her men are aboard: the first morning's jobs are not this test's)
                var home = GameData.Port(table.home);
                var quay = At(home.lat, home.lon);
                double owed0 = Trade.Debt;

                // ======== 1. THE FIRST TIME: they come down to her berth, and given nothing they only talk
                yield return Run(play, game, 3.0, () => TheirCardUp(Collectors.QuayCard));
                told.AppendLine($"first: state {Collectors.State}, visit {Collectors.Visit}, temper {Collectors.Temper}, card {(cards.Current != null ? cards.Current.card.id : "none")}, drops {string.Join(" ", dropped)}");
                Assert.IsTrue(TheirCardUp(Collectors.QuayCard), "the day of the first sending came, she lies at home, and their card did not come:\n" + told);
                Assert.AreEqual(GoonState.Quay, Collectors.State);
                Assert.AreEqual(1, Collectors.Visit);
                Assert.IsTrue(Collectors.Met, "they stand at her berth and she has not met them");
                Assert.Contains(ClockReason.Goons, dropped, "the clock did not drop back when they came");
                string said1 = "", end1 = "";
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 15f);
                yield return ChartPlay.Settle(0.8f);
                yield return play.Shot("goons_1_at_the_quay");
                yield return Answer(play, "Tell them you have not got it", (s, e) => { said1 = s; end1 = e; });
                told.AppendLine("first, said: " + ChartPlay.Short(said1));
                told.AppendLine("first, end: " + ChartPlay.Short(end1));
                StringAssert.Contains("says you have to pay him", said1, "the rope man does not say what he was told to say");
                StringAssert.DoesNotContain("the Dane", said1);
                StringAssert.Contains("not sure what comes next", end1, "the first time, given nothing, they did more than talk");
                Assert.AreEqual(owed0, Trade.Debt, 0.01);
                Assert.AreEqual(1, Collectors.Unpaid);
                Assert.AreEqual(table.temperStart + 1.0, Collectors.Temper, 1e-9, "they went home with nothing and the lender is no angrier");
                Assert.AreEqual(GoonState.Home, Collectors.State, "they did not go home");

                // ======== 2. THE SECOND TIME: they turn out the purse
                Trade.Purse.Add("denier", 300.0);
                double silver = Trade.Purse.Silver(rules);
                Collectors.PutOut(2, quay, Collectors.Temper, table.home);
                yield return Run(play, game, 3.0, () => TheirCardUp(Collectors.QuayCard));
                Assert.IsTrue(TheirCardUp(Collectors.QuayCard), "they are at the quay again and their card did not come");
                string said2 = "", end2 = "";
                yield return Answer(play, "Tell them you have not got it", (s, e) => { said2 = s; end2 = e; });
                told.AppendLine("second, said: " + ChartPlay.Short(said2));
                told.AppendLine("second, end: " + ChartPlay.Short(end2));
                StringAssert.Contains("You again", said2, "the rope man does not know her the second time");
                StringAssert.Contains("turn out the purse", end2, "the second time, given nothing, they took nothing");
                Assert.Less(Trade.Purse.Silver(rules), silver - 200.0, "an instalment's worth of silver did not leave the purse");
                Assert.Less(Trade.Debt, owed0 - 150.0, "what they took was not counted on the stick");
                Assert.AreEqual(4, Voyage.Men, "a man was lost the second time");
                Assert.AreEqual(2, Collectors.Unpaid);

                // ======== 3. THE THIRD TIME: the card says who is next; given nothing, they kill Ravn
                Assert.IsFalse(Collectors.RavnGone);
                Collectors.PutOut(3, quay, Collectors.Temper, table.home);
                yield return Run(play, game, 3.0, () => TheirCardUp(Collectors.QuayCard));
                Assert.IsTrue(TheirCardUp(Collectors.QuayCard), "they are at the quay a third time and their card did not come");
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 15f);
                for (int i = 0; i < 40 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("goons_2_the_dane_is_next");
                string said3 = "", end3 = "";
                int logFrom = FrameLog.Lines.Count;
                yield return Answer(play, "Tell them you have not got it", (s, e) => { said3 = s; end3 = e; });
                told.AppendLine("third, said: " + ChartPlay.Short(said3));
                told.AppendLine("third, end: " + ChartPlay.Short(end3));
                File.WriteAllText(Path.Combine(dir, "goons.txt"), told.ToString());
                StringAssert.Contains("it is the Dane", said3, "the third time, their card does not say that nothing now means Ravn");
                StringAssert.Contains("Ravn", end3, "the third time she gave them nothing, and Ravn lives");
                Assert.IsTrue(Collectors.RavnGone, "Ravn is dead and no flag says so");
                Assert.AreEqual(3, Voyage.Men, "Ravn is dead and as many men are aboard");
                Assert.IsTrue(FrameLog.Lines.Skip(logFrom).Any(l => l.text.Contains("Ravn")), "the log does not say what was done to Ravn");
                // (9 Oct 2026, later: his death is a scene of its own and a matter that lies on her, Voyage/Feud.cs: held by
                // FeudTests. Here only its beginning: it is owed, and the crew's heart has its ceiling from this minute)
                Assert.AreNotEqual(FeudStage.None, Feud.Stage, "Ravn is dead and nothing follows");
                Assert.AreEqual(Feud.Rules.heartCeiling, Feud.HeartCeiling, 1e-9);
                Assert.LessOrEqual(Voyage.Morale, Feud.Rules.heartCeiling + 1e-9, "their heart is over its ceiling with Ravn dead on the quay");
                Assert.IsTrue(Self.HasMark(Feud.Mark), "no mark lies on her for it");
                Feud.Forget();                                                // (what follows of it is FeudTests' to hold: here, the rope men at sea)
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("goons_3_after_ravn");
                // no card that leans on him can come again
                var leaning = EncounterRunner.Book.cards.Where(c => c.leans.Contains(table.ravnCast)).ToList();
                Assert.Greater(leaning.Count, 5, "no card of the game leans on Ravn");
                foreach (var c in leaning)
                    Assert.IsFalse(cards.state.Ready(c, game.Day), c.id + " leans on Ravn and can still come");

                // ======== 4. AT SEA: their boat in sight astern, it comes up with her, and their card holds her
                SeaPush.Fixed = at => (0.0, 0.0);                             // (a still sea: this is a test of them, not of the tide)
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                var sea = At(52.5, 3.0);
                var u = FloatingOrigin.ToUnity(new WorldPos(sea.x, 0.0, sea.z));
                game.ship.Teleport(new Vector3(u.x, 0f, u.z), 270f);
                game.ship.anchored = false;
                Exploration.See(sea.x, sea.z, 45000.0);
                Exploration.Apply();
                Assert.IsTrue(Course.SetToPlace(new WorldXZ(sea.x - 60000.0, sea.z), ChartWorld.ShipWorld(), null), "no course west on the open sea");
                Adrift.Forget();
                map.Follow();
                dropped.Clear();
                logFrom = FrameLog.Lines.Count;
                Collectors.PutOut(4, new WorldXZ(sea.x + 5000.0, sea.z), 2.0);
                Assert.IsTrue(Collectors.Out, "their boat could not be put on the sea");
                bool seen = false, marked = false;
                yield return Run(play, game, 6.0, () =>
                {
                    seen |= Collectors.InSight;
                    marked |= map.GoonsShown;
                    return TheirCardUp(Collectors.SeaCard);
                }, 240f);
                var her = ChartWorld.ShipWorld();
                told.AppendLine($"at sea: state {Collectors.State}; {WorldXZ.Distance(her, Collectors.At):0} m apart; seen {seen}; marked {marked}; card {(cards.Current != null ? cards.Current.card.id : "none")}; drops {string.Join(" ", dropped)}");
                told.AppendLine("the log: " + string.Join(" | ", FrameLog.Lines.Skip(logFrom).Select(l => l.text)));
                File.WriteAllText(Path.Combine(dir, "goons.txt"), told.ToString());
                Assert.IsTrue(seen, "their boat was five km astern and she never had it in sight:\n" + told);
                Assert.IsTrue(marked, "their boat was in sight and had no mark on the chart");
                Assert.IsTrue(FrameLog.Lines.Skip(logFrom).Any(l => l.text.Contains("rowing hard: the lender's men")), "the log does not say their boat is in sight:\n" + told);
                Assert.Contains(ClockReason.Goons, dropped, "the clock did not drop back when their boat came in sight");
                Assert.IsTrue(TheirCardUp(Collectors.SeaCard), "their boat rows faster than she sails, and never came up with her:\n" + told);
                Assert.AreEqual(GoonState.Alongside, Collectors.State);
                Assert.IsTrue(EncounterRunner.ShipHeld, "they are alongside and she is not held");
                yield return ChartPlay.Until(() => play.Frame.FloatOpen("card"), 15f);
                yield return ChartPlay.Settle(0.8f);
                map.Follow();
                yield return ChartPlay.Settle(0.4f);
                yield return play.Shot("goons_4_alongside_at_sea");
                string said4 = "", end4 = "";
                double owed4 = Trade.Debt;
                yield return Answer(play, "Tell them you have not got it", (s, e) => { said4 = s; end4 = e; });
                told.AppendLine("at sea, said: " + ChartPlay.Short(said4));
                told.AppendLine("at sea, end: " + ChartPlay.Short(end4));
                StringAssert.DoesNotContain("Ravn", end4, "Ravn is killed twice");
                Assert.AreEqual(3, Voyage.Men);
                Assert.That(Collectors.State, Is.EqualTo(GoonState.Homing).Or.EqualTo(GoonState.Home), "given their answer at sea they did not go home");
                Assert.IsFalse(EncounterRunner.ShipHeld, "they are gone and she is still held");
                Assert.LessOrEqual(Trade.Debt, owed4);
            }
            finally
            {
                SeaPush.Fixed = null;
                Collectors.Use(null);
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (told.Length > 0)
                    File.WriteAllText(Path.Combine(dir, "goons.txt"), told.ToString());
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
