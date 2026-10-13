using System.Collections;
using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PORTS THE GAME SAILS TO ARE THE SCENE'S OWN (14 Oct 2026, KAPMAN_PLAN step 1b). A new game: every port of
    /// the tables is a Port under the ports' holder, every one taken from the scene as saved and none made at play.
    /// </summary>
    public class PortsInScenePlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator EveryPortIsTheScenesOwn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_ports_saves", p => play = p, "ports");
            try
            {
                Assert.Greater(GameData.Ports.Count, 40, "no ports read");
                Assert.AreEqual(GameData.Ports.Count, Port.All.Count, "the chart has not a port for every trading place of the tables");
                var madeAtPlay = Port.All.Where(p => !p.FromScene).Select(p => p.id).ToList();
                Assert.IsEmpty(madeAtPlay, "ports made at play, not taken from the scene: " + string.Join(", ", madeAtPlay));
                var root = GameObject.Find(ChartWorld.PortsRootName);
                Assert.IsNotNull(root, "no holder of the ports");
                Assert.IsTrue(Port.All.All(p => p.transform.parent == root.transform), "a port stands outside the ports' holder");
                Assert.AreEqual("dorestad", Port.LyingId(), "she does not lie at Dorestat, the scene's own port");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
