using System.Collections;
using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE MARKET AS PLAYED IS THE FILE'S (14 Oct 2026, KAPMAN_PLAN step 2d). A new game, the market of the port she
    /// lies at opened beside the chart: the panel on screen is the layout file's root, filled in; every good is a row
    /// of the row file, with its name and its prices; a click on Leave shuts it. A picture is taken (Captures/market).
    /// </summary>
    public class MarketLayoutPlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator TheMarketOnScreenIsTheLayoutFilesFilledIn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_market_saves", p => play = p, "market");
            try
            {
                var market = MarketPanel.Current;
                Assert.IsNotNull(market, "no market panel");
                var here = Port.All.FirstOrDefault(p => p.id == Port.LyingId());
                Assert.IsNotNull(here, "she lies at no port");
                market.Open(here, docked: true);
                yield return ChartPlay.Settle(0.5f);
                Assert.IsTrue(market.IsOpen, "the market did not open");
                var docRoot = market.GetComponent<UIDocument>().rootVisualElement;
                var root = docRoot.Q<VisualElement>(MarketPanel.RootName);
                Assert.IsNotNull(root, "the screen has no element named \"" + MarketPanel.RootName + "\": the market is not the layout file's");
                Assert.AreSame(market, root.userData, "the market's root on screen is not the one the panel filled in");
                Assert.AreEqual(1, docRoot.Query<VisualElement>(MarketPanel.RootName).ToList().Count, "more than one market root on screen");
                Assert.IsTrue(ChartPlay.Shown(root), "the market's root is not shown");
                string title = root.Q<Label>("title").text;
                Assert.IsFalse(string.IsNullOrWhiteSpace(title), "the market has no title");
                var items = root.Q<ScrollView>("list").Query<VisualElement>("item").ToList();
                Assert.Greater(items.Count, 3, "fewer than four goods in the market of " + here.id);
                foreach (var item in items)
                {
                    var row = item.Q<VisualElement>("row");
                    Assert.IsNotNull(row, "a good's row is not the row file's (no element named \"row\")");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(item.Q<Label>("good-name")?.text), "a good's row has no name");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(item.Q<Label>("pays")?.text), "a good's row has no price paid");
                    Assert.IsNull(item.Q<VisualElement>("preview"), "a good's row came with the file's preview");
                }
                Assert.Greater(items.Count(i => i.Q<VisualElement>("buttons").childCount > 0), 0, "no good has a button to buy or sell");
                Assert.IsTrue(root.Q<VisualElement>("purse").childCount > 1, "the purse card is empty");
                Assert.IsTrue(root.Q<VisualElement>("hold").childCount > 1, "the hold card is empty");
                yield return play.Shot("market_0_the_files_own");
                yield return play.Click(root.Q<Button>("leave"), "Leave");
                yield return ChartPlay.Settle(0.3f);
                Assert.IsFalse(market.IsOpen, "the market did not shut on Leave");
                Assert.IsFalse(ChartPlay.Shown(root), "the market's root is still shown after Leave");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
