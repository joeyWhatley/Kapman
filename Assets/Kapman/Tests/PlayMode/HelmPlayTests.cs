using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.ShipView;
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
    /// YOUR HAND ON THE HELM, IN THE 3D VIEW AS A PLAYER HAS IT (the user, 13 Oct 2026: "need a way to control while in
    /// 3d mode"; by the picker: "Steer her yourself in 3D"). A new game; she casts off from Dorestat for Traiectum and
    /// is under way; the 3D view takes the screen (the Swap button). The key D held: the helm is taken, she turns to
    /// starboard, the helm's mark says whose hand and shows the tiller over, and the log says the helm is taken. The
    /// key let go: she holds her heading. H: the helm is the steersman's again, her course is laid again from where
    /// she is, and the log says so. Pictures in Captures/helm (helm_*.png).
    /// </summary>
    public class HelmPlayTests
    {
        [UnityTest, Timeout(900000)]
        public IEnumerator TheHelmIsTakenInTheThreeDViewAndGivenBack()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_helm_saves", p => play = p, "helm");
            var heard = new List<string>();
            System.Action<string> hear = words => heard.Add(words);
            Voyage.Notice += hear;
            try
            {
                var game = KapmanGame.Current;
                var frame = play.Frame;
                Assert.IsNotNull(game, "no game");

                // ---- she casts off for Traiectum and gets under way
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                yield return play.Click(SeaMovesHerTests.Small(play, "1"), "speed 1");    // (the new game begins paused)
                float from = Time.realtimeSinceStartup;
                while ((game.ship.anchored || ChartBody.Current == null || ChartBody.Current.Knots < 0.3f) && Time.realtimeSinceStartup - from < 40f)
                {
                    yield return SeaMovesHerTests.AnswerAll(play);
                    yield return null;
                }
                Assert.IsFalse(game.ship.anchored, "she did not get under way after casting off");
                Assert.Greater(ChartBody.Current.Knots, 0.3f, "she makes no way after casting off");
                Assert.IsTrue(Course.IsSet, "her course for Traiectum is gone");

                // ---- the 3D view takes the screen
                var hook = Object.FindFirstObjectByType<ShipStageHook>();
                Assert.IsNotNull(hook, "no 3D view in the scene");
                Assert.IsNotNull(hook.SwapButtonOnShip, "no Swap button");
                yield return play.Click(hook.SwapButtonOnShip, "Swap");
                yield return ChartPlay.Settle(1.0f);
                Assert.IsTrue(hook.Swapped && hook.Applied, "the Swap button did not give the 3D view the screen");
                Assert.IsNotNull(hook.HelmMark, "no mark of the helm");
                Assert.IsTrue(ChartPlay.Shown(hook.HelmMark), "the helm's mark is not shown with the 3D view on the screen");
                StringAssert.Contains("steersman", ChartPlay.Read(hook.HelmMark).ToLowerInvariant(), "the mark does not say the helm is the steersman's");
                Assert.IsFalse(HandHelm.Held);
                yield return play.Shot("helm_0_the_steersmans");

                // ---- D held: the helm is taken, and she turns to starboard
                float headingBefore = game.ship.Heading;
                heard.Clear();
                int graphs = HandHelm.ByGraph;
                yield return play.Hold(Key.D, 90);
                Assert.IsTrue(HandHelm.Held, "the key D did not take the helm. Said: " + string.Join(" | ", heard));
                Assert.AreEqual(graphs + 2, HandHelm.ByGraph, "the taking and the letting go were not both run by the graph");
                Assert.AreEqual("let_go", HandHelm.What, "the key let go did not put the helm amidships");
                Assert.IsTrue(heard.Any(w => w.Contains("helm")), "the log was not told the helm is taken: " + string.Join(" | ", heard));
                float turned = Mathf.DeltaAngle(headingBefore, game.ship.Heading);
                Assert.Greater(turned, 8f, $"she did not turn to starboard under the hand: from {headingBefore:0} to {game.ship.Heading:0}");
                Assert.IsTrue(Course.IsSet, "her course was given up when the helm was taken: it should wait");
                StringAssert.Contains("your hand", ChartPlay.Read(hook.HelmMark).ToLowerInvariant(), "the mark does not say the hand is yours");
                yield return play.Shot("helm_1_your_hand");
                Debug.Log($"The helm taken in the 3D view: she turned {turned:0} degrees to starboard in 90 frames, from {headingBefore:0} to {game.ship.Heading:0}. Said: {string.Join(" | ", heard)}");

                // ---- let go: she holds her heading
                Assert.AreEqual(0f, HandHelm.Rudder, "the helm is still over after the key was let go");
                float held = game.ship.Heading;
                yield return ChartPlay.Frames(30);
                Assert.Less(Mathf.Abs(Mathf.DeltaAngle(held, game.ship.Heading)), 4f, "she does not hold her heading with the helm amidships");

                // ---- H: the helm is the steersman's again, her course laid again from where she is
                heard.Clear();
                var here = ChartWorld.ShipWorld();
                yield return play.Press(Key.H);
                yield return ChartPlay.Settle(0.6f);
                Assert.IsFalse(HandHelm.Held, "the key H did not give the helm back");
                Assert.IsTrue(HandHelm.Laid, "her course was not laid again from where she is: " + string.Join(" | ", heard));
                Assert.IsTrue(Course.IsSet && Course.Destination == "traiectum", "her course for Traiectum is gone");
                Assert.Less(WorldXZ.Distance(Course.Points[0], here), 600.0, "the course laid again does not begin where she is");
                Assert.IsTrue(heard.Any(w => w.Contains("steersman")), "the log was not told the helm is the steersman's again: " + string.Join(" | ", heard));
                StringAssert.Contains("steersman", ChartPlay.Read(hook.HelmMark).ToLowerInvariant());
                yield return play.Shot("helm_2_given_back");

                // ---- the swap back to the chart gives the helm back too
                yield return play.Hold(Key.A, 20);
                Assert.IsTrue(HandHelm.Held, "the key A did not take the helm");
                yield return play.Click(hook.SwapButtonOnChart, "Swap");
                yield return ChartPlay.Settle(0.6f);
                Assert.IsFalse(hook.Swapped, "the Swap button did not give the chart the screen");
                Assert.IsFalse(HandHelm.Held, "the swap back to the chart left your hand on the helm");
                Assert.IsFalse(ChartPlay.Shown(hook.HelmMark), "the helm's mark shows with the chart on the screen");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                Voyage.Notice -= hear;
                play.End();
            }
        }
    }
}
