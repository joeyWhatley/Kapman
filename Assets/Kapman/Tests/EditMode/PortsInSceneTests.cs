using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PORTS STAND IN THE SCENE (14 Oct 2026, KAPMAN_PLAN step 1b). The Kapman scene's file holds a "Port id" object
    /// with a Port for every trading place of Data/ports_793.json under the ports' holder; and ChartWorld.PlacePorts keeps
    /// a saved port where the hand left it and makes only what is missing.
    /// </summary>
    public class PortsInSceneTests
    {
        const string Scene = "Assets/Kapman/Scenes/Kapman.unity";
        const string Table = "Assets/Kapman/Game/Data/ports_793.json";

        [Test]
        public void TheSceneHoldsEveryPortOfTheTables()
        {
            var ids = Regex.Matches(File.ReadAllText(Table), "\"id\": *\"([a-z0-9_]+)\"").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
            Assert.Greater(ids.Count, 40, "the ports table reads oddly: " + ids.Count + " ids");
            string scene = File.ReadAllText(Scene).Replace("\r\n", "\n");
            StringAssert.Contains("m_Name: " + ChartWorld.PortsRootName + "\n", scene, "the scene has no holder of the ports (Kapman > Make physical > 4)");
            var meta = File.ReadAllText("Assets/Kapman/Code/Game/Ports/Port.cs.meta");
            StringAssert.Contains("guid: " + Regex.Match(meta, "guid: ([0-9a-f]{32})").Groups[1].Value, scene, "no Port component in the scene");
            var missing = ids.Where(id => !scene.Contains("m_Name: Port " + id + "\n")).ToList();
            Assert.IsEmpty(missing, "ports of the table not in the scene: " + string.Join(", ", missing));
            Assert.AreEqual(ids.Count, Regex.Matches(scene, "m_Name: Port [a-z0-9_]+\n").Count, "the scene holds more port objects than the table has ports");
        }

        [Test]
        public void SavedPortsAreKeptWhereTheyAreAndOnlyMissingOnesAreMade()
        {
            GameData.LoadPorts(GameFiles.Find("ports_793"));
            Assert.Greater(GameData.Ports.Count, 40, "no ports read");
            var root = new GameObject("ports under test").transform;
            try
            {
                Vector3 At(PortDef def) => new Vector3((float)def.lon * 1000f, 0f, (float)def.lat * 1000f);
                int made = ChartWorld.PlacePorts(root, At, 30f, out int found);
                Assert.AreEqual(GameData.Ports.Count, made, "not every port was made under an empty holder");
                Assert.AreEqual(0, found);
                var ports = root.GetComponentsInChildren<Port>(true);
                Assert.AreEqual(GameData.Ports.Count, ports.Length);
                Assert.IsTrue(ports.All(p => p.berth != null && p.berth.parent == p.transform && !p.autoPlace && !p.FromScene));
                var dorestad = ports.First(p => p.id == "dorestad");
                dorestad.transform.position += new Vector3(123f, 0f, -45f);
                dorestad.arrivalRadius = 77f;
                var moved = dorestad.transform.position;

                // the hand's work stands: made again over the same holder, nothing is made and nothing moved
                made = ChartWorld.PlacePorts(root, At, 30f, out found);
                Assert.AreEqual(0, made, "ports were made again that were there");
                Assert.AreEqual(GameData.Ports.Count, found);
                Assert.AreEqual(GameData.Ports.Count, root.GetComponentsInChildren<Port>(true).Length);
                Assert.AreEqual(moved, dorestad.transform.position, "the port moved by hand was put back");
                Assert.AreEqual(77f, dorestad.arrivalRadius, "the reach set by hand was put back");
                Assert.IsTrue(dorestad.FromScene, "a port found is not marked as from the scene");

                // one taken away is made again, alone
                Object.DestroyImmediate(root.GetComponentsInChildren<Port>(true).First(p => p.id == "traiectum").gameObject);
                made = ChartWorld.PlacePorts(root, At, 30f, out found);
                Assert.AreEqual(1, made);
                Assert.AreEqual(GameData.Ports.Count - 1, found);
            }
            finally
            {
                Object.DestroyImmediate(root.gameObject);
            }
        }
    }
}
