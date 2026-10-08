using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE NEWS OF A LAND WITH NO PORT COMES ASHORE SOMEWHERE (9 Oct 2026; the user: "have we got the Iceland settlement
    /// event? And Greenland / Vinland?"). They were in the chronicle, and three of the four could never be heard: an
    /// entry is told at the ports of its regions, and Iceland and Greenland have none. Held here: the four are in the
    /// table with their years, and each has a port to be told at now.
    /// </summary>
    public class ChronicleNewsTests
    {
        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + name);

        [SetUp]
        public void Begin()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            Chronicle.Load(Asset("chronicle_793.json"));
        }

        /// <summary>The ports an entry's news is told at, as the game reckons it (KapmanGame.ChronicleCame).</summary>
        static List<PortDef> ToldAt(ChronicleEntry e)
        {
            var at = Shocks.PortsOf(e, e.year);
            return at.Count > 0 ? at : Chronicle.NewsLandsAt(e, e.year);
        }

        [Test]
        public void IcelandGreenlandAndVinlandAreInTheChronicleAndCanBeHeard()
        {
            foreach (var (id, year, word) in new[]
                     {
                         ("874-iceland-settled", 874, "Iceland"),
                         ("930-althing", 930, "Althing"),
                         ("985-greenland", 985, "Greenland"),
                         ("1000-iceland-christian", 1000, "Christian"),
                         ("1000-vinland", 1000, "Vinland"),
                     })
            {
                var e = Chronicle.Entry(id);
                Assert.IsNotNull(e, "the chronicle has no entry " + id);
                Assert.AreEqual(year, e.year, id + ": its year");
                StringAssert.Contains(word, e.title, id);
                var told = ToldAt(e);
                Assert.IsNotEmpty(told, $"'{e.title}' is told at no port: nobody could ever hear of it");
                Assert.IsTrue(told.Any(p => p.id == "ogvaldsnes"), $"'{e.title}' should come ashore in western Norway; it is told at {string.Join(", ", told.Select(p => p.name))}");
            }
            // (the settlement of Iceland was heard already, by way of western Norway; the others had no port at all)
            Assert.IsNotEmpty(Shocks.PortsOf(Chronicle.Entry("874-iceland-settled"), 874));
            foreach (string id in new[] { "930-althing", "985-greenland", "1000-iceland-christian", "1000-vinland" })
                Assert.IsEmpty(Shocks.PortsOf(Chronicle.Entry(id), Chronicle.Entry(id).year), id + " has a port of its own after all: its market would feel the news");
        }

        [Test]
        public void ADateFromTheGroundIsNotToldAsNews()
        {
            foreach (string id in new[] { "939-eldgja", "1021-lanse-aux-meadows" })
            {
                var e = Chronicle.Entry(id);
                Assert.IsNotNull(e, "the chronicle has no entry " + id);
                Assert.AreEqual("evidence", e.kind);
                Assert.IsEmpty(Chronicle.NewsLandsAt(e, e.year), $"'{e.title}' is a date from the ground, not something said on a waterfront");
            }
        }

        [Test]
        public void HowMuchOfTheChronicleIsStillToldNowhere()
        {
            // (not a rule, a count kept in sight: the lands with no port in the game, whose news nobody hears yet)
            var nowhere = Chronicle.Entries.Where(e => e.kind != "evidence" && ToldAt(e).Count == 0).ToList();
            var regions = nowhere.SelectMany(e => e.regions.Length > 0 ? e.regions : new[] { "(no region)" }).GroupBy(r => r)
                                 .OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}");
            Debug.Log($"Chronicle: {nowhere.Count} of {Chronicle.Entries.Count} entries are told at no port: {string.Join(", ", regions)}");
            Assert.IsFalse(nowhere.Any(e => e.regions.Contains("iceland") || e.regions.Contains("greenland")),
                           "an entry of Iceland or Greenland is still told nowhere: " + string.Join("; ", nowhere.Where(e => e.regions.Contains("iceland") || e.regions.Contains("greenland")).Select(e => e.id)));
            Assert.Less(nowhere.Count, Chronicle.Entries.Count / 2, "more than half of the chronicle can never be heard");
        }
    }
}
