using System.Collections.Generic;
using System.IO;
using Kapman.Chain;
using Kapman.Water;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Each level's water from its record (Data/Water/water_793.json): the chain names real records, and the
    /// Kromme Rijn at Dorestad and the southern North Sea give the research's numbers through the year.</summary>
    public class WaterProfileTests
    {
        static Dictionary<string, WaterProfile> all;

        static Dictionary<string, WaterProfile> All =>
            all ??= WaterProfile.ParseAll(File.ReadAllText(Path.Combine(Application.dataPath, "Kapman", "Data", "Water", "water_793.json")));

        [Test]
        public void EveryLevelNamesARealRecord()
        {
            Assert.Greater(All.Count, 200, "the merged research");
            var chain = ChainData.Load(ChainData.ProjectPath);
            int named = 0;
            foreach (var level in chain.levels)
            {
                if (level.water == null || string.IsNullOrEmpty(level.water.profile))
                    continue;
                named++;
                Assert.IsTrue(All.ContainsKey(level.water.profile), $"{level.id}: no record '{level.water.profile}'");
                var p = All[level.water.profile];
                if (level.kind == "ocean")
                    Assert.IsTrue(p.HasSea || p.kind == "ocean", $"{level.id}: an ocean level takes a sea record");
            }
            Assert.GreaterOrEqual(named, 2, "Dorestad and the southern North Sea at least");
            var dorestad = chain.Level("dorestad");
            Assert.AreEqual("kromme_rijn_dorestad", dorestad.water.profile);
        }

        [Test]
        public void TheKrommeRijnThroughTheYear()
        {
            var p = All["kromme_rijn_dorestad"];
            Assert.IsTrue(p.HasRiver);
            Assert.IsFalse(p.HasTide, "no tide at Dorestad");
            Assert.AreEqual(0.3f, p.Current(Day(9, 1)), 0.03f, "low water in early September");
            Assert.AreEqual(1.0f, p.Current(Day(1, 20)), 0.05f, "the winter flood stream in January");
            Assert.AreEqual(0.5f, p.Current(Day(5, 20)), 0.08f, "the mean stream in late May");
            Assert.AreEqual(1.0f, p.Current(Day(8, 15), 1f), 0.01f, "a summer spate runs at the flood stream");
            float last = p.Current(1f);
            for (int d = 2; d <= 365; d++)
            {
                float c = p.Current(d);
                Assert.Less(Mathf.Abs(c - last), 0.05f, $"no jump in the stream on day {d}");
                last = c;
            }
            Assert.AreEqual(3.0f, p.Temperature(45f), 0.2f, "February water");
            Assert.AreEqual(18.0f, p.Temperature(227f), 0.2f, "August water");
            Assert.AreEqual(0.6f, p.Secchi(Day(7, 1)), 0.02f, "summer clarity");
            Assert.AreEqual(0.4f, p.Secchi(Day(1, 15)), 0.02f, "winter clarity");
            Assert.AreEqual(0.6f, WaterProfile.AbsorptionDistance(p.Secchi(Day(7, 1))), 0.03f, "the tuned river's 0.6 m");
            var s = p.Scattering;
            Assert.Greater(p.Turbidity, 0.95f, "a silty river");
            Assert.AreEqual(0.41f, s.r, 0.02f);
            Assert.AreEqual(0.37f, s.g, 0.02f);
            Assert.AreEqual(0.22f, s.b, 0.02f, "silty khaki-brown from above (the 4 Oct water look test)");
        }

        [Test]
        public void TheSouthernNorthSea()
        {
            var p = All["o1_southern_north_sea"];
            Assert.IsTrue(p.HasSea && p.HasTide);
            Assert.IsFalse(p.HasRiver);
            Assert.AreEqual(0f, p.Current(200f));
            Assert.AreEqual(2.5f, p.TideRange(1.5f), 0.05f, "springs");
            Assert.AreEqual(1.2f, p.TideRange(1.5f + 29.53f / 4f), 0.05f, "neaps a week later");
            Assert.AreEqual(6f, p.Secchi(Day(7, 1)), 0.1f, "clearer in summer");
            Assert.AreEqual(3f, p.Secchi(Day(1, 15)), 0.1f, "stirred up in winter");
            var s = p.Scattering;
            Assert.Greater(s.g, s.r, "grey-green, never the tropical teal");
            Assert.Less(s.g, 0.3f);
            Assert.AreEqual(5.2f, p.Temperature(45f), 0.2f);
            Assert.AreEqual(16.2f, p.Temperature(227f), 0.2f);
        }

        static float Day(int month, int day)
        {
            int[] before = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
            return before[month - 1] + day;
        }
    }
}
