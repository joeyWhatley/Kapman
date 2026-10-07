using System.Collections.Generic;
using System.IO;
using Kapman.Chain;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>A small chain for the tests: two river places at one scale joined by a bend, with corridors, a berth in
    /// each, and a route through them (the real chain.json has no corridor join yet).</summary>
    static class ChainSamples
    {
        public const string Bend = @"{
 ""version"": 1, ""home"": ""upper"",
 ""pace"": { ""riverMps"": 1.6, ""seaMps"": 2.8, ""reachMps"": 2.572 },
 ""levels"": [
  { ""id"": ""upper"", ""kind"": ""river_place"", ""realMetresPerUnit"": 1, ""layer"": ""Level0"",
    ""water"": { ""base"": 2.0, ""tide"": false, ""currentMps"": 0.4, ""pushesShips"": false, ""preset"": ""river_brown"" },
    ""ports"": [""up_town""],
    ""channel"": [[0, 0], [0, 1000], [700, 1700]],
    ""chartRoute"": [[52.0, 5.0], [52.009, 5.0], [52.0153, 5.0103]],
    ""anchors"": [
      { ""id"": ""berth"", ""kind"": ""berth"", ""port"": ""up_town"", ""pos"": [10, 200], ""heading"": 0 },
      { ""id"": ""down"", ""pos"": [600, 1600], ""heading"": 45, ""tideShare"": 0,
        ""corridor"": [[494.0, 1494.0], [600, 1600], [706.0, 1706.0]] } ] },
  { ""id"": ""lower"", ""kind"": ""river_place"", ""realMetresPerUnit"": 1, ""layer"": ""Level1"",
    ""water"": { ""preset"": ""river_brown"", ""pushesShips"": false, ""tide"": false, ""currentMps"": 0.4, ""base"": 2.0 },
    ""ports"": [""low_town""],
    ""channel"": [[400, 0], [300, 100], [300, 1200]],
    ""chartRoute"": [[52.03, 5.03], [52.031, 5.0285], [52.041, 5.0285]],
    ""anchors"": [
      { ""id"": ""up"", ""pos"": [300, 100], ""heading"": 135, ""tideShare"": 0,
        ""corridor"": [[406.0, -6.0], [300, 100], [194.0, 206.0]] },
      { ""id"": ""berth"", ""kind"": ""berth"", ""port"": ""low_town"", ""pos"": [300, 900], ""heading"": 0 } ] }
 ],
 ""joins"": [
  { ""id"": ""the_bend"", ""a"": ""upper/down"", ""b"": ""lower/up"", ""kind"": ""bend"", ""hide"": ""bend"", ""realKm"": 21.0 }
 ],
 ""routes"": [
  { ""id"": ""down_the_river"", ""stops"": [""upper/berth"", ""upper/down"", ""lower/up"", ""lower/berth""], ""budgetMinutes"": 25, ""realKm"": 23.5 }
 ]
}";

        public static ChainData Real() => ChainData.Load(ChainData.ProjectPath);
        public static ChainData BendChain() => ChainData.Parse(Bend);
    }

    /// <summary>chain.json: it reads, the graph is whole, it keeps the rules of the gateways design, and the named routes
    /// take the minutes they are budgeted (decision 1: Dorestad kept long).</summary>
    public class ChainDataTests
    {
        [Test]
        public void Parses()
        {
            var d = ChainSamples.Real();
            Assert.AreEqual("dorestad", d.home);
            CollectionAssert.IsSubsetOf(new[] { "dorestad", "northsea_south" }, d.levels.ConvertAll(l => l.id));
            Assert.AreEqual(d.levels.Count, new HashSet<string>(d.levels.ConvertAll(l => l.id)).Count, "level ids unique");
            Assert.AreEqual(d.joins.Count, new HashSet<string>(d.joins.ConvertAll(j => j.id)).Count, "join ids unique");
            foreach (var j in d.joins)
            {
                Assert.IsNotNull(j.a, j.id);
                Assert.IsNotNull(j.b, j.id);
                Assert.AreSame(j.a, d.Anchor(j.a.Ref), j.id);
                Assert.AreSame(j.b, d.Anchor(j.b.Ref), j.id);
            }

            var dorestad = d.Level("dorestad");
            Assert.IsTrue(dorestad.Built);
            Assert.AreEqual("Level0", dorestad.layer);
            Assert.AreEqual("Level1", d.Level("northsea_south").layer);
            Assert.IsTrue(dorestad.water.pushesShips, "decided 4 Oct: the stream pushes her");
            Assert.IsFalse(dorestad.water.tide, "no tide at Dorestad");

            // the one join to the sea: the builder's exit at the bake's edge, hidden by the deck beat (decisions 1 and 2)
            var mouth = d.Join("rhine_mouth");
            Assert.AreEqual("dorestad/down", mouth.a.Ref);
            Assert.AreEqual("northsea_south/rhine_mouth", mouth.b.Ref);
            Assert.AreEqual("mouth", mouth.kind);
            Assert.AreEqual("deck_beat", mouth.hide);
            Assert.IsTrue(mouth.beat.skippableAfterFirst);
            Assert.That(mouth.beat.seconds, Is.InRange(3.0, 5.0));
            Assert.AreEqual(2737.98, mouth.a.pose.x, 1e-6, "plan.exitDownstream");
            Assert.That(mouth.realKm, Is.GreaterThan(80.0), "the Rhine below Dorestad to the sea is cut out");

            // the Thames and Walichrum: mouths whose far side is not built yet
            foreach (var id in new[] { "thames_mouth", "walichrum" })
            {
                var j = d.Join(id);
                Assert.AreEqual("mouth", j.kind, id);
                Assert.IsFalse(j.Built, id);
                StringAssert.EndsWith("(not built yet)", j.PromptAt(j.a), id);
            }
            Assert.AreEqual("Sail down the Rhine to the sea", mouth.PromptAt(mouth.a));
        }

        [Test]
        public void Connected()
        {
            var d = ChainSamples.Real();
            var reach = d.Reachable(d.Level("dorestad"));
            foreach (var l in d.levels)
                Assert.IsTrue(reach.Contains(l), $"{l.id} is reachable from Dorestad");

            // every port the chain names is a port of 793, and in one level only
            var known = new HashSet<string>();
            foreach (var p in Json.Arr(Json.Parse(File.ReadAllText("Assets/Kapman/Game/Data/ports_793.json")), "ports"))
                known.Add(Json.Str(p, "id"));
            var seen = new Dictionary<string, string>();
            foreach (var l in d.levels)
                foreach (var port in l.ports)
                {
                    Assert.IsTrue(known.Contains(port), $"{l.id} names the port {port}, which ports_793.json does not have");
                    Assert.IsFalse(seen.ContainsKey(port), $"the port {port} is in {(seen.TryGetValue(port, out var o) ? o : "")} and {l.id}");
                    seen[port] = l.id;
                }
            Assert.AreEqual("dorestad", seen["dorestad"]);
            Assert.AreEqual("lundenwic", seen["lundenwic"]);
        }

        [Test]
        public void Rules()
        {
            var errors = ChainCheck.Errors(ChainSamples.Real());
            Assert.IsEmpty(errors, string.Join("\n", errors.ConvertAll(e => e.ToString())));
            Assert.IsEmpty(ChainCheck.Errors(ChainSamples.BendChain()), "the sample bend chain is sound");

            // and the checker does catch a broken chain: one fault at a time in the sample
            void Breaks(string rule, string from, string to)
            {
                string json = ChainSamples.Bend.Replace(from, to);
                Assert.AreNotEqual(ChainSamples.Bend, json, "the fault was put in");
                var found = ChainCheck.Errors(ChainData.Parse(json));
                Assert.IsTrue(found.Exists(f => f.rule == rule), $"'{rule}' is caught: {string.Join("; ", found.ConvertAll(f => f.ToString()))}");
            }
            Breaks("layer", "\"layer\": \"Level1\"", "\"layer\": \"Level0\"");
            Breaks("current", "\"pushesShips\": false, \"tide\": false", "\"pushesShips\": true, \"tide\": false");
            Breaks("scale", "\"realMetresPerUnit\": 1, \"layer\": \"Level1\"", "\"realMetresPerUnit\": 150, \"layer\": \"Level1\"");
            Breaks("tide", "\"heading\": 135, \"tideShare\": 0", "\"heading\": 135, \"tideShare\": 0.5");
            Breaks("tide", "\"heading\": 135, \"tideShare\": 0", "\"heading\": 135, \"tideShare\": 1");
            Breaks("corridor", "\"corridor\": [[406.0, -6.0], [300, 100], [194.0, 206.0]]", "\"corridor\": []");
            Breaks("prompt", "\"kind\": \"bend\", \"hide\": \"bend\"", "\"kind\": \"mouth\", \"hide\": \"deck_beat\"");
            Breaks("ports", "\"ports\": [\"low_town\"]", "\"ports\": [\"up_town\"]");
            Breaks("route", "\"realKm\": 23.5", "\"realKm\": 40");
            Breaks("route", "\"budgetMinutes\": 25", "\"budgetMinutes\": 60");
        }

        [Test]
        public void RouteMinutes()
        {
            var d = ChainSamples.Real();
            var player = ChainShip.Player(d);
            foreach (var r in d.routes)
            {
                var map = new RouteMap(d, r);
                double minutes = ChainSim.PlayMinutes(map, player);
                Assert.That(minutes, Is.InRange(r.budgetMinutes * 0.8, r.budgetMinutes * 1.2),
                            $"{r.id}: {minutes:0.0} minutes against a budget of {r.budgetMinutes}");
            }

            // decision 1: Dorestad kept long, 15-19 minutes from the berth to the level's edge
            var home = new RouteMap(d, d.Route("dorestad_lundenwic"));
            var dorestad = home.legs[0];
            Assert.AreEqual("dorestad", dorestad.level.id);
            Assert.That(dorestad.metres, Is.InRange(1650.0, 1850.0), "about 1,800 m of channel from the mooring to the exit");
            // (14, not 15: since 4 Oct the stream carries her down, which takes about half a minute off)
            Assert.That(ChainSim.LegHours(dorestad, player), Is.InRange(14.0, 19.0), "game hours, which are minutes of play at 1x, rowing down with the stream");
            Assert.IsTrue(dorestad.downstream, "the berth is upstream of the exit");
        }
    }
}
