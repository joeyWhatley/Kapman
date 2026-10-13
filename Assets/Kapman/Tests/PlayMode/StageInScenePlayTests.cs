using System.Collections;
using System.Linq;
using Kapman.Game.ShipView;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE STAGE ON THE SCREEN IS THE SCENE'S OWN (the user, 14 Oct 2026: "l chart is a bunch of scripts still"; by the
    /// picker, "Yes, both: frame and stage"). A new game; the 3D view takes the screen. The stage it draws is the object
    /// saved in the Kapman scene under SYSTEMS, taken up with its Sun, Moon, sky, camera and sea as saved, none of them
    /// made a second time; the sky and air's profile is the saved asset. Picture in Captures/stage.
    /// </summary>
    public class StageInScenePlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator TheStageOnTheScreenIsTheScenesOwn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_stage_saves", p => play = p, "stage");
            try
            {
                var hook = Object.FindFirstObjectByType<ShipStageHook>();
                Assert.IsNotNull(hook, "no 3D view in the scene");
                Assert.IsNotNull(hook.SwapButtonOnShip, "no Swap button");
                yield return play.Click(hook.SwapButtonOnShip, "Swap");
                yield return ChartPlay.Settle(1.0f);
                Assert.IsTrue(hook.Swapped && hook.Applied, "the Swap button did not give the 3D view the screen");
                var stage = hook.Stage;
                Assert.IsNotNull(stage, "no stage");
                Assert.IsTrue(stage.FromScene, "the stage was made from nothing: the scene's own was not taken up");
                Assert.AreEqual(ShipStage.ObjectName, stage.name);
                Assert.IsNotNull(stage.transform.parent, "the stage is not under SYSTEMS");
                Assert.AreEqual("--- SYSTEMS ---", stage.transform.parent.name, "the stage is not under SYSTEMS");
                var all = stage.GetComponentsInChildren<Transform>(true);
                foreach (var part in new[] { "Sun", "Moon", "Sky and air", "Ship stage camera", "Rain", "Sea (HDRP water)", "Ship", "Model", "Fit-out" })
                    Assert.AreEqual(1, all.Count(t => t.name == part), "the stage has not exactly one \"" + part + "\"");
                Assert.AreEqual(1, stage.GetComponentsInChildren<Camera>(true).Length, "the stage has not one camera");
                Assert.IsNotNull(stage.Volume.sharedProfile, "the sky and air's profile is not the saved asset");
                Assert.IsTrue(stage.Volume.HasInstantiatedProfile(), "the volume works on the saved profile itself, not its own copy");
                Assert.IsTrue(stage.Sun.enabled, "the saved Sun was not woken");
                Assert.IsTrue(stage.View.Camera.targetTexture != null || stage.View.Picture != null, "the stage draws no picture");
                yield return play.Shot("stage_0_the_scenes_own");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
