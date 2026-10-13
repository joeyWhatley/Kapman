using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CHART IS LAID OUT IN A FILE (KAPMAN_PLAN step 2d, 14 Oct 2026). Game/UI/MapView.uxml holds every part the
    /// chart's code looks for by name, the Kapman scene's Map view document has it as its Source Asset, and the file
    /// instantiates with the parts in the order the drawn layers are put in among.
    /// </summary>
    public class MapLayoutTests
    {
        const string Layout = "Assets/Kapman/Game/UI/MapView.uxml";
        const string Scene = "Assets/Kapman/Scenes/Kapman.unity";

        static readonly string[] Parts =
        {
            "map", "layer", "under", "over", "paper", "fog", "overlays", "finds", "sails", "ship", "knots", "names", "map-hearsay", "hearsay-note",
            "card", "title", "subtitle", "back", "foot", "scalebar", "scaletext", "hint", "chronicle", "save", "load",
            "chronicle-panel", "chronicle-list", "ship-card", "ship-card-text",
            "port-card", "port-card-text", "port-card-words", "port-card-set", "port-card-clear", "port-card-close",
        };

        [Test]
        public void TheLayoutFileHoldsEveryPartTheChartLooksFor()
        {
            Assert.IsTrue(File.Exists(Layout), Layout + " is missing");
            string uxml = File.ReadAllText(Layout);
            var missing = Parts.Where(p => !uxml.Contains("name=\"" + p + "\"")).ToList();
            Assert.IsEmpty(missing, "parts not in " + Layout + ": " + string.Join(", ", missing));
            Assert.AreEqual(MapView.RootName, "map");
            string scene = File.ReadAllText(Scene);
            var guid = Regex.Match(File.ReadAllText(Layout + ".meta"), "guid: ([0-9a-f]{32})").Groups[1].Value;
            StringAssert.Contains("guid: " + guid, scene, "the scene's Map view document has not " + Layout + " as its Source Asset (Kapman > Make physical > 2d)");
        }

        [Test]
        public void TheLayoutInstantiatesWithTheLayersInTheirOrder()
        {
#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Layout);
            Assert.IsNotNull(asset, Layout + " did not import");
            var tree = asset.Instantiate();
            var root = tree.Q<VisualElement>(MapView.RootName);
            Assert.IsNotNull(root, "no element named \"" + MapView.RootName + "\"");
            foreach (var p in Parts)
                Assert.IsNotNull(root.Q<VisualElement>(p), "no part named \"" + p + "\" once instantiated");
            var layer = root.Q<VisualElement>("layer");
            var order = layer.Children().Select(c => c.name).ToList();
            CollectionAssert.AreEqual(new[] { "under", "over", "paper", "fog", "overlays", "names" }, order, "the chart's layers are not in their order under \"layer\"");
            var overlays = root.Q<VisualElement>("overlays");
            CollectionAssert.AreEqual(new[] { "finds", "sails", "ship", "knots" }, overlays.Children().Select(c => c.name).ToList(), "the overlays are not in their order");
            Assert.AreEqual(PickingMode.Ignore, overlays.pickingMode, "the overlays take the pointer");
            Assert.AreEqual(PickingMode.Ignore, root.Q<VisualElement>("names").pickingMode, "the names take the pointer");
            Assert.AreEqual(PickingMode.Position, layer.pickingMode, "the layer does not take the pointer (it must: the chart is dragged on it)");
            Assert.IsInstanceOf<Image>(root.Q<VisualElement>("fog"));
            Assert.IsInstanceOf<Button>(root.Q<VisualElement>("back"));
            Assert.IsInstanceOf<ScrollView>(root.Q<VisualElement>("chronicle-list"));
#endif
        }
    }
}
