using System.Collections;
using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CHEST AS PLAYED IS THE CHART FILE'S (14 Oct 2026, KAPMAN_PLAN step 2d). A new game, I pressed: the chest
    /// that opens is the element "chest" of Game/UI/MapView.uxml, its tabs the file's buttons with the open one marked,
    /// its area filled with a page; a tab's click turns the page; I again shuts it. A picture is taken (Captures/chest).
    /// </summary>
    public class ChestLayoutPlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator TheChestOnScreenIsTheChartFilesOwn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_chest_saves", p => play = p, "chest");
            try
            {
                var map = MapView.Current;
                Assert.IsNotNull(map, "no chart");
                var root = map.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(MapView.RootName);
                Assert.IsNotNull(root, "the chart's root is not the file's");
                var chest = root.Q<VisualElement>("chest");
                Assert.IsNotNull(chest, "no element named \"chest\" in the chart");
                Assert.AreEqual(1, root.Query<VisualElement>("chest").ToList().Count, "more than one chest in the chart");
                Assert.IsFalse(map.ChestOpen, "the chest is open before I is pressed");
                Assert.IsFalse(ChartPlay.Shown(chest), "the chest is shown before I is pressed");
                yield return play.Press(Key.I);
                yield return ChartPlay.Settle(0.5f);
                Assert.IsTrue(map.ChestOpen, "I did not open the chest");
                Assert.IsTrue(ChartPlay.Shown(chest), "the chest is not shown though open");
                // (over every other part of the file; the code adds a nameless helper or two after it)
                Assert.Greater(root.IndexOf(chest), root.IndexOf(root.Q<VisualElement>("port-card")), "the chest does not lie over the rest of the chart");
                var tabs = chest.Q<VisualElement>("chest-tabs").Children().OfType<Button>().ToList();
                Assert.AreEqual(4, tabs.Count, "the chest has not its four tabs");
                Assert.AreEqual(1, tabs.Count(t => t.ClassListContains("on")), "not one tab marked open");
                Assert.IsTrue(tabs[0].ClassListContains("on"), "the riddles are not the open tab at first");
                var area = chest.Q<VisualElement>("chest-area");
                Assert.Greater(area.childCount, 0, "the chest's area holds no page");
                yield return play.Shot("chest_0_the_files_own");
                yield return play.Click(chest.Q<Button>("chest-tab-notes"), "Price notes");
                yield return ChartPlay.Settle(0.3f);
                Assert.IsTrue(chest.Q<Button>("chest-tab-notes").ClassListContains("on"), "the price notes' tab is not marked open after its click");
                Assert.IsFalse(tabs[0].ClassListContains("on"), "the riddles' tab is still marked open");
                Assert.Greater(area.childCount, 0, "the price notes' page is empty");
                yield return play.Click(chest.Q<Button>("chest-close"), "Close");
                yield return ChartPlay.Settle(0.3f);
                Assert.IsFalse(map.ChestOpen, "Close did not shut the chest");
                Assert.IsFalse(ChartPlay.Shown(chest), "the chest is still shown after Close");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
