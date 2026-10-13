using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE MARKET IS LAID OUT IN A FILE (KAPMAN_PLAN step 2d, 14 Oct 2026). Game/UI/MarketPanel.uxml holds every part
    /// the market's code looks for by name and is the Source Asset of the Market panel's UI Document in the Kapman
    /// scene; a row of goods is Game/UI/Parts/MarketRow.uxml, named in Resources/FrameParts.
    /// </summary>
    public class MarketLayoutTests
    {
        const string Layout = "Assets/Kapman/Game/UI/MarketPanel.uxml";
        const string RowLayout = "Assets/Kapman/Game/UI/Parts/MarketRow.uxml";
        const string Scene = "Assets/Kapman/Scenes/Kapman.unity";
        const string PartsAsset = "Assets/Kapman/Game/Resources/FrameParts.asset";

        static readonly string[] Parts =
        {
            "market", "box", "head", "titles", "title", "subtitle", "writing", "leave", "body", "left", "tabs",
            "head-row", "head-good", "head-unit", "head-here", "head-asks", "head-pays", "head-aboard", "list",
            "right", "purse", "hold", "details", "outcome",
        };

        static readonly string[] RowParts = { "item", "row", "name", "good-name", "good-marks", "good-law", "unit", "here", "asks", "pays", "aboard", "buttons" };

        static string Guid(string asset) => Regex.Match(File.ReadAllText(asset + ".meta"), "guid: ([0-9a-f]{32})").Groups[1].Value;

        [Test]
        public void TheLayoutFilesHoldEveryPartTheMarketLooksFor()
        {
            Assert.IsTrue(File.Exists(Layout), Layout + " is missing");
            Assert.IsTrue(File.Exists(RowLayout), RowLayout + " is missing");
            string uxml = File.ReadAllText(Layout);
            var missing = Parts.Where(p => !uxml.Contains("name=\"" + p + "\"")).ToList();
            Assert.IsEmpty(missing, "parts not in " + Layout + ": " + string.Join(", ", missing));
            string row = File.ReadAllText(RowLayout);
            missing = RowParts.Where(p => !row.Contains("name=\"" + p + "\"")).ToList();
            Assert.IsEmpty(missing, "parts not in " + RowLayout + ": " + string.Join(", ", missing));
            Assert.AreEqual(MarketPanel.RootName, "market");
            StringAssert.Contains("guid: " + Guid(Layout), File.ReadAllText(Scene),
                                  "the scene's Market panel document has not " + Layout + " as its Source Asset (Kapman > Make physical > 2e)");
            StringAssert.Contains("guid: " + Guid(RowLayout), File.ReadAllText(PartsAsset),
                                  PartsAsset + " does not name " + RowLayout + " (Kapman > Make physical > 2e)");
        }

        [Test]
        public void TheLayoutInstantiatesWithItsPartsInTheirOrder()
        {
#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Layout);
            Assert.IsNotNull(asset, Layout + " did not import");
            var root = asset.Instantiate().Q<VisualElement>(MarketPanel.RootName);
            Assert.IsNotNull(root, "no element named \"" + MarketPanel.RootName + "\"");
            foreach (var p in Parts)
                Assert.IsNotNull(root.Q<VisualElement>(p), "no part named \"" + p + "\" once instantiated");
            Assert.AreEqual(PickingMode.Ignore, root.pickingMode, "the market's root takes the pointer while it is shut");
            CollectionAssert.AreEqual(new[] { "left", "right" }, root.Q<VisualElement>("body").Children().Select(c => c.name).ToList(), "the body's two sides are not in their order");
            CollectionAssert.AreEqual(new[] { "tabs", "head-row", "list" }, root.Q<VisualElement>("left").Children().Select(c => c.name).ToList(), "the table's parts are not in their order");
            CollectionAssert.AreEqual(new[] { "head-good", "head-unit", "head-here", "head-asks", "head-pays", "head-aboard", "head-buttons" },
                                      root.Q<VisualElement>("head-row").Children().Select(c => c.name).ToList(), "the head row's cells are not in their order");
            CollectionAssert.AreEqual(new[] { "purse", "hold", "details" }, root.Q<VisualElement>("right").Children().Select(c => c.name).ToList(), "the cards are not in their order");
            Assert.IsInstanceOf<Button>(root.Q<VisualElement>("leave"));
            Assert.IsInstanceOf<ScrollView>(root.Q<VisualElement>("list"));
            Assert.IsInstanceOf<Label>(root.Q<VisualElement>("outcome"));

            var rowAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RowLayout);
            Assert.IsNotNull(rowAsset, RowLayout + " did not import");
            var item = FrameParts.Make<VisualElement>(rowAsset, "item");
            Assert.IsNull(item.parent, "the row's copy is still in the file's preview");
            foreach (var p in RowParts.Skip(1))
                Assert.IsNotNull(item.Q<VisualElement>(p), "no part named \"" + p + "\" in a row once instantiated");
            CollectionAssert.AreEqual(new[] { "name", "unit", "here", "asks", "pays", "aboard", "buttons" },
                                      item.Q<VisualElement>("row").Children().Select(c => c.name).ToList(), "the row's cells are not in their order");
            var parts = UnityEditor.AssetDatabase.LoadAssetAtPath<FrameParts>(PartsAsset);
            Assert.IsNotNull(parts, PartsAsset + " is missing");
            Assert.AreEqual(rowAsset, parts.marketRow, PartsAsset + " does not name " + RowLayout + " as the market's row");
#endif
        }
    }
}
