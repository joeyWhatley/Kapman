using System.Linq;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The world simulation: the same seed plays the same, ships sail and deal, prices stay in their bounds; and
    /// (Long) the year's test run that writes the spreadsheets.</summary>
    public class SimTests
    {
        [Test]
        public void TheSameSeedPlaysTheSame()
        {
            SimHarness.RunYear(793, 20, out var a, write: false);
            long ha = a.StateHash();
            SimHarness.RunYear(793, 20, out var b, write: false);
            Assert.AreEqual(ha, b.StateHash(), "seed 793 twice");
            SimHarness.RunYear(794, 20, out var c, write: false);
            Assert.AreNotEqual(ha, c.StateHash(), "seed 794 differs");
        }

        [Test]
        public void TradersSailAndDeal()
        {
            SimHarness.RunYear(793, 40, out var sim, write: false);
            Assert.Greater(sim.ships.Count, 100, "the world's traders");
            Assert.Greater(sim.stats.voyages.Count, sim.ships.Count / 2, "in 40 summer days most have sailed");
            Assert.Greater(sim.stats.voyages.Count(v => !double.IsNaN(v.arrived)), 20, "and arrived");
            Assert.Greater(sim.stats.deals, 100, "and dealt");
        }

        [Test, Category("Long")]
        public void TestRunAYear()
        {
            string summary = SimHarness.RunYear(793, 365, out var sim);
            Debug.Log(summary);
            Assert.Greater(sim.stats.voyages.Count, 0);
            StringAssert.Contains("report.html", summary);
        }

        /// <summary>Two years, then every trader that hasn't begun a voyage in the last 120 days, with what holds her
        /// (her state, purse, debts to her crew, cargo, where she's bound): for finding ships that stop sailing.</summary>
        [Test, Explicit("Long: two years, then the idle traders"), Category("Long")]
        public void TestWhyTradersStop()
        {
            var w = SimHarness.MakeWorld(793);
            w.Populate();
            for (int h = 0; h < 730 * 24; h++)
                w.Hour();
            var last = LastSailed(w);
            var rules = TradeRules.Current;
            var lines = new System.Text.StringBuilder($"{w.Date}: voyages {w.stats.voyages.Count}\n");
            foreach (var s in w.ships.Where(s => !s.raider && s.state != ShipState.Lost && (!last.TryGetValue(s.id, out double d) || w.Day - d > 120.0)))
                lines.Append($"{s.id} {s.cls.id}: {s.state} at {GameData.Ports[s.at].id}{(s.dest >= 0 ? " for " + GameData.Ports[s.dest].id : "")}, " +
                             $"silver {s.purse.Silver(rules):0}, owed {s.owedWages:0.0}, cargo [{string.Join(" ", s.hold.Contents.Select(c => c.count + " " + c.good.id))}], " +
                             $"ready {s.readyDay - w.Day:0.0} d, waited {w.Day - s.waitSince:0.0} d, anchored {(w.Hours - s.anchoredSince) / 24.0:0.0} d, leg {s.leg}/{s.path.Count}, done {s.done / 1000.0:0}/{s.total / 1000.0:0} km\n");
            Markets.Clear();
            Debug.Log(lines.ToString());
            Assert.Pass(lines.ToString());
        }

        static System.Collections.Generic.Dictionary<string, double> LastSailed(WorldSim w) =>
            w.stats.voyages.GroupBy(v => v.ship).ToDictionary(g => g.Key, g => g.Max(v => v.departed));

        /// <summary>Ten years with nobody watching (a few minutes: run it by name). No trader stops sailing, and the
        /// watched goods' prices stay within twofold of where they began (Plans/ai_fleets.md, phase 6).</summary>
        [Test, Explicit("Long: ten years of the world"), Category("Long")]
        public void TestRunTenYears()
        {
            string summary = SimHarness.RunYear(793, 3653, out var sim, true, 1.0, 7);
            Debug.Log(summary);
            var health = SimReport.Health(sim, out int stuck, out int drifted);
            Debug.Log(string.Join("\n", health));
            Assert.AreEqual(0, stuck, "traders that stopped sailing");
            Assert.AreEqual(0, drifted, "goods whose price drifted more than twofold");
        }
    }
}
