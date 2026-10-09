using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// WATERS AND LANDMARKS TO FIND, AND THE NARROW START (the user, 9 Oct 2026: "narrow the starting fog area, just
    /// Dorestead and the closest port, then add waters to discover and land marks ect points of interest").
    ///   - the table is whole: every row read, every one on the painted chart, and every one where a ship can come
    ///     (the sea lanes pass within its reach: Bake/world/check_discoveries.py says the same outside the game);
    ///   - a thing is found when she comes within its reach, once, and the game is told; not before its years;
    ///   - what lies in the waters she knows on the first morning is known without a word;
    ///   - what is found is saved and comes back, and a game saved before there was anything to find is not told a
    ///     heap of old news;
    ///   - the first morning's chart is her home and the port nearest it by water, and no more.
    /// </summary>
    public class DiscoveriesTests
    {
        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + name);

        [SetUp]
        public void Begin()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            Discoveries.Reset();
            Exploration.Clear();
        }

        [TearDown]
        public void Done()
        {
            Discoveries.Reset();
            Exploration.Clear();
        }

        /// <summary>A small table of a test's own: two waters a hundred km apart, and a light that is built in 900.</summary>
        static DiscoveryBook Small() => DiscoveryBook.Read(Json.Parse(@"{
            ""kinds"": { ""water"": { ""name"": ""water"", ""reach_km"": 20 }, ""landmark"": { ""name"": ""landmark"", ""reach_km"": 12 } },
            ""things"": [
                { ""id"": ""near"", ""kind"": ""water"", ""name"": ""The near water"", ""lat"": 52.0, ""lon"": 4.0, ""reach_km"": 10, ""note"": ""Brown and shallow."" },
                { ""id"": ""far"", ""kind"": ""water"", ""name"": ""The far water"", ""lat"": 53.0, ""lon"": 4.0, ""note"": ""Grey and deep."" },
                { ""id"": ""light"", ""kind"": ""landmark"", ""name"": ""The new light"", ""lat"": 52.0, ""lon"": 4.0, ""from"": 900, ""note"": ""Built in 900."" },
                { ""id"": ""near"", ""kind"": ""water"", ""name"": ""The same id again"", ""lat"": 50.0, ""lon"": 4.0 },
                { ""id"": """", ""kind"": ""water"", ""name"": ""No id"", ""lat"": 50.0, ""lon"": 4.0 },
                { ""id"": ""nowhere"", ""kind"": ""water"", ""name"": ""No place"" }
            ] }") as Dictionary<string, object>);

        static WorldXZ At(double lat, double lon, double eastKm = 0.0, double northKm = 0.0)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x + eastKm * 1000.0, z + northKm * 1000.0);
        }

        [Test]
        public void TheTableIsWholeAndEveryThingLiesOnThePaintedChart()
        {
            var root = ContentFiles.ParseObject(Discoveries.File);
            Assert.IsNotNull(root, Discoveries.File + " is not there or does not parse");
            int rows = ContentJson.Rows(root, "things").Count();
            var book = Discoveries.Book;
            Assert.AreEqual(rows, book.things.Count, "a row of the table was left out: it has no id, no name, no place, or an id used twice");
            Assert.GreaterOrEqual(book.things.Count, 60, "there is little to find");
            var kinds = new HashSet<string> { "water", "river", "island", "headland", "shore", "bank", "landmark", "holy", "race" };   // (the marks the style sheet has: MapView.uss, .find-*)
            var art = MapArt.Shared();
            foreach (var d in book.things)
            {
                Assert.IsTrue(kinds.Contains(d.kind), $"{d.id} is of a kind the chart has no mark for: '{d.kind}'");
                Assert.IsFalse(string.IsNullOrWhiteSpace(d.note), $"{d.id} has nothing said of it");
                Assert.IsFalse(string.IsNullOrWhiteSpace(d.kindName), $"{d.id}: its kind has no word");
                Assert.That(d.reach, Is.InRange(5000.0, 29000.0), $"{d.id}: found from {d.reach / 1000.0:0} km off; she sees {Exploration.Sight / 1000.0:0} km, and a thing found must lie where the fog has lifted");
                Assert.Less(d.name.Length, 40, $"{d.id}: its name is long for a slip on the chart: '{d.name}'");
                if (art != null)
                    Assert.IsTrue(d.x > art.West && d.x < art.East && d.z < art.North && d.z > art.South, $"{d.id} ({d.name}) lies off the painted chart");
            }
            Assert.AreEqual(book.things.Count, Discoveries.Total(793), "in 793 they are all there (none has years yet)");
        }

        [Test]
        public void EveryThingLiesWhereAShipCanCome()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
            var g = SimRoutes.Graph();
            Assert.IsNotNull(g, "the sea lanes did not load");
            // the lanes a ship of hers may sail (not the rivers that are not built, nor a haul over land): Course.Profile
            var book = Discoveries.Book;
            var best = new double[book.things.Count];
            for (int i = 0; i < best.Length; i++)
                best[i] = double.MaxValue;
            for (int e = 0; e < g.EdgeCount; e++)
            {
                byte kind = g.edgeKind[e];
                if (kind == LaneGraph.Pending || kind == LaneGraph.Portage)
                    continue;
                for (int k = g.edgePoint[e]; k < g.edgePoint[e] + g.edgePoints[e]; k++)
                    for (int i = 0; i < best.Length; i++)
                    {
                        double dx = g.px[k] - book.things[i].x, dz = g.pz[k] - book.things[i].z;
                        double d2 = dx * dx + dz * dz;
                        if (d2 < best[i])
                            best[i] = d2;
                    }
            }
            var never = new List<string>();
            for (int i = 0; i < best.Length; i++)
                if (System.Math.Sqrt(best[i]) > book.things[i].reach * 0.85)
                    never.Add($"{book.things[i].id} (the nearest lane passes {System.Math.Sqrt(best[i]) / 1000.0:0} km off, its reach is {book.things[i].reach / 1000.0:0} km)");
            Assert.IsEmpty(never, "she only ever sails the lanes, so these can never be found: " + string.Join("; ", never));
        }

        [Test]
        public void SheFindsAThingWhenSheComesWithinItsReachAndOnlyOnce()
        {
            var book = Small();
            Assert.AreEqual(3, book.things.Count, "a row with no id, no place or a second use of an id should be left out");
            Assert.AreEqual(10000.0, book.Get("near").reach, 1e-6, "a row's own reach");
            Assert.AreEqual(20000.0, book.Get("far").reach, 1e-6, "its kind's reach");
            Assert.AreEqual("water", book.Get("far").kindName);
            Discoveries.Use(book);
            var told = new List<string>();
            Discoveries.Found += d => told.Add(d.id);
            int version = Discoveries.Version;

            Assert.IsEmpty(Discoveries.Step(At(52.0, 4.0, eastKm: 60.0), 3.0, 793), "sixty km off, nothing is found");
            Assert.IsEmpty(Discoveries.Step(At(52.0, 4.0, eastKm: 10.6), 3.1, 793), "just outside its reach it is not found");
            Assert.AreEqual(version, Discoveries.Version, "nothing was found, and the count of what is found moved");
            var now = Discoveries.Step(At(52.0, 4.0, eastKm: 9.4), 3.25, 793);
            Assert.AreEqual(1, now.Count, "inside its reach the near water is found (and the light is not there before 900)");
            Assert.AreEqual("near", now[0].id);
            Assert.IsTrue(Discoveries.IsFound("near"));
            Assert.AreEqual(3.25, Discoveries.DayFound("near"), 1e-9);
            Assert.IsTrue(double.IsNaN(Discoveries.DayFound("far")));
            Assert.Greater(Discoveries.Version, version);
            CollectionAssert.AreEqual(new[] { "near" }, told, "the game is told of a find, once");
            StringAssert.Contains("The near water", Discoveries.Line(now[0]));
            StringAssert.Contains("Brown and shallow.", Discoveries.Line(now[0]));

            Assert.IsEmpty(Discoveries.Step(At(52.0, 4.0), 4.0, 793), "a thing found is not found again");
            CollectionAssert.AreEqual(new[] { "near" }, told);
            Assert.AreEqual(2, Discoveries.Total(793), "the light is not there to find before 900");
            Assert.AreEqual(3, Discoveries.Total(900));
            now = Discoveries.Step(At(52.0, 4.0), 40000.0, 900);
            Assert.AreEqual("light", now.Single().id, "in 900 the light is there and she lies under it");
            now = Discoveries.Step(At(53.0, 4.0, northKm: 19.0), 40001.0, 900);
            Assert.AreEqual("far", now.Single().id);
            CollectionAssert.AreEqual(new[] { "near", "light", "far" }, Discoveries.FoundIds, "in the order found");
        }

        [Test]
        public void WhatSheKnowsOnTheFirstMorningIsKnownWithoutAWord()
        {
            Discoveries.Use(Small());
            var told = new List<string>();
            Discoveries.Found += d => told.Add(d.id);
            var near = At(52.0, 4.0);
            Exploration.Begin(near.x - 400000.0, near.z + 400000.0, near.x + 400000.0, near.z - 400000.0);
            // the fog is lifted round the far water; she lies 8 km from the near one, in the fog
            var far = At(53.0, 4.0);
            Exploration.Reveal(far.x, far.z, 25000.0);
            Assert.AreEqual(2, Discoveries.KnowAtStart(793, At(52.0, 4.0, eastKm: 8.0)), "the water where the fog is lifted, and the water she lies within reach of");
            Assert.IsTrue(Discoveries.IsFound("far") && Discoveries.IsFound("near"));
            Assert.Less(Discoveries.DayFound("far"), 0.0, "what she knew from the start has no day it was found on");
            Assert.IsEmpty(told, "nothing is said of what she knew on the first morning");
            Assert.IsFalse(Discoveries.IsFound("light"));
            Assert.AreEqual(0, Discoveries.KnowAtStart(793, near), "known once");
            // a new game somewhere else: all is to find again
            Discoveries.Forget();
            Assert.AreEqual(0, Discoveries.Count);
        }

        [Test]
        public void WhatIsFoundIsSavedAndComesBack()
        {
            Discoveries.Use(Small());
            Assert.IsTrue(ContentSave.Registered("found"), "what is found is not a part of the saved game");
            Discoveries.Step(At(52.0, 4.0), 12.5, 793);
            Discoveries.Step(At(53.0, 4.0), 14.0, 793);
            CollectionAssert.AreEqual(new[] { "near", "far" }, Discoveries.FoundIds);
            // (through the save's own text, as a saved game goes)
            var saved = Json.Parse(Json.Write(ContentSave.Capture())) as Dictionary<string, object>;
            Discoveries.Forget();
            Assert.AreEqual(0, Discoveries.Count);
            var told = new List<string>();
            Discoveries.Found += d => told.Add(d.id);
            int version = Discoveries.Version;
            ContentSave.Restore(saved);
            CollectionAssert.AreEqual(new[] { "near", "far" }, Discoveries.FoundIds, "what was found did not come back in its order");
            Assert.AreEqual(12.5, Discoveries.DayFound("near"), 1e-9);
            Assert.AreEqual(14.0, Discoveries.DayFound("far"), 1e-9);
            Assert.Greater(Discoveries.Version, version, "the chart is not told that a game was loaded");
            Assert.IsEmpty(Discoveries.Step(At(52.0, 4.0), 20.0, 793), "a loaded game found again what it had");
            Assert.IsEmpty(told);

            // A GAME SAVED BEFORE THERE WAS ANYTHING TO FIND has no such part. What lies where she has been is known to
            // her, and nothing is said of it: she is not told a heap of old news on her first minute.
            var near = At(52.0, 4.0);
            Exploration.Begin(near.x - 400000.0, near.z + 400000.0, near.x + 400000.0, near.z - 400000.0);
            var far = At(53.0, 4.0);
            Exploration.Reveal(far.x, far.z, 25000.0);                    // (she has been on the far water)
            var older = new Dictionary<string, object>(saved);
            older.Remove("found");
            ContentSave.Restore(older);
            Assert.AreEqual(0, Discoveries.Count);
            Assert.IsEmpty(Discoveries.Step(At(52.0, 4.0, eastKm: 300.0), 30.0, 793), "an older save was told of old finds as news");
            Assert.IsTrue(Discoveries.IsFound("far"), "where she has been is not known to her after an older save");
            Assert.IsFalse(Discoveries.IsFound("near"));
            Assert.IsEmpty(told);
            Assert.AreEqual("near", Discoveries.Step(near, 31.0, 793).Single().id, "after an older save a new water is found as any other");
            CollectionAssert.AreEqual(new[] { "near" }, told);
        }

        [Test]
        public void TheFirstMorningsChartIsHomeAndThePortNearestIt()
        {
            var dorestad = GameData.Port("dorestad");
            var nearest = KapmanGame.NearestPort(dorestad, 793);
            Assert.IsNotNull(nearest);
            Assert.AreEqual("traiectum", nearest.id, "the port nearest Dorestat by water is Traiectum, up the old Rhine");
            var home = At(dorestad.lat, dorestad.lon);
            Exploration.Begin(home.x - 600000.0, home.z + 600000.0, home.x + 600000.0, home.z - 600000.0);
            float Seen(string port)
            {
                var p = GameData.Port(port);
                var w = At(p.lat, p.lon);
                return Exploration.Seen(w.x, w.z);
            }
            KapmanGame.KnowStart(dorestad);
            Assert.AreEqual(("dorestad", "traiectum"), KapmanGame.StartKnows);
            Assert.AreEqual(1f, Seen("dorestad"), 1e-6f, "home is not known on the first morning");
            Assert.Greater(Seen("traiectum"), 0.9f, "the port nearest home is not known on the first morning");
            foreach (string far in new[] { "witla", "walichrum", "medemelacha", "daventre", "colonia", "andoverpis", "ripa", "lundenwic" })
                Assert.AreEqual(0f, Seen(far), 1e-6f, $"{GameData.Port(far).name} is known on the first morning: the start should be home and the nearest port only");
            // (how much of the world that is: under a hundredth of what it was, 160 km round home)
            double known = 0.0;
            for (double x = home.x - 200000.0; x <= home.x + 200000.0; x += Exploration.CellSize)
                for (double z = home.z - 200000.0; z <= home.z + 200000.0; z += Exploration.CellSize)
                    known += Exploration.Seen(x, z) * Exploration.CellSize * Exploration.CellSize / 1e6;
            Assert.That(known, Is.InRange(2500.0, 6500.0), "square km known on the first morning (it was some 80,000)");
            // her own river and its arms are no discovery; the inland sea and the river's mouth are
            Assert.IsTrue(Discoveries.IsFound("rhenus"), "the Rhine at her own quay is to be found");
            Assert.Less(Discoveries.DayFound("rhenus"), 0.0);
            foreach (string far in new[] { "almere", "maas_mouth", "fli", "narrows", "temes_mouth" })
                Assert.IsFalse(Discoveries.IsFound(far), far + " is known on the first morning");
            Assert.That(Discoveries.Count, Is.InRange(1, 6), "what she knows on the first morning: " + string.Join(", ", Discoveries.FoundIds));

            // ANOTHER HOME: what was lifted round the first is put back, and hers is known
            var ripa = GameData.Port("ripa");
            KapmanGame.KnowStart(ripa);
            Assert.AreEqual("ripa", KapmanGame.StartKnows.home);
            Assert.AreEqual(1f, Seen("ripa"), 1e-6f);
            Assert.AreEqual(0f, Seen("dorestad"), 1e-6f, "a Dane's chart is open round Dorestat");
            Assert.IsFalse(Discoveries.IsFound("rhenus"), "a Dane knows the Rhine from the start");
            Assert.IsTrue(Discoveries.IsFound("ripa_deeps"), "the deeps off Ripa are to be found by one who lives there");
        }

        /// <summary>What she finds along a charted course, in order: the course is walked a kilometre at a time, as she
        /// sails it. With the ports of the course as it passes them (within 3 km), for the record.</summary>
        static List<string> FoundAlong(string from, string to, out double km, System.Text.StringBuilder way = null)
        {
            var a = GameData.Port(from);
            var start = At(a.lat, a.lon);
            var line = new List<WorldXZ>();
            Assert.IsTrue(Course.Plan(to, start, from, line, out km), $"no way by water from {from} to {to}");
            var found = new List<string>();
            double day = 0.0, since = 1e9;
            for (int i = 1; i < line.Count; i++)
            {
                double leg = WorldXZ.Distance(line[i - 1], line[i]);
                int steps = System.Math.Max(1, (int)System.Math.Ceiling(leg / 1000.0));
                for (int k = 1; k <= steps; k++)
                {
                    var at = new WorldXZ(line[i - 1].x + (line[i].x - line[i - 1].x) * k / steps, line[i - 1].z + (line[i].z - line[i - 1].z) * k / steps);
                    day += leg / steps / 150000.0;
                    since += leg / steps;
                    foreach (var d in Discoveries.Step(at, day, 793))
                        found.Add(d.id);
                    if (way != null && since >= 25000.0)
                    {
                        since = 0.0;
                        var (lat, lon) = WorldFrame.ToLatLon(at.x, at.z);
                        way.Append($" ({lat:0.00}, {lon:0.00})");
                    }
                }
            }
            return found;
        }

        [Test]
        public void AlongHerCoursesThereIsSomethingToFind()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
            var dorestad = GameData.Port("dorestad");
            var home = At(dorestad.lat, dorestad.lon);
            Exploration.Begin(home.x - 1200000.0, home.z + 1200000.0, home.x + 1200000.0, home.z - 1200000.0);
            // (the ways a boat of hers really takes from home: down the Lek to the Maas mouth for the south and the
            // west, by the old Rhine to the sea at its old mouth for the north, by the Vecht to the Almere)
            foreach (var (to, least) in new[] { ("witla", 1), ("lundenwic", 4), ("ripa", 6), ("medemelacha", 1), ("quentovic", 4), ("hamwic", 6), ("walichrum", 2) })
            {
                KapmanGame.KnowStart(dorestad);
                var way = new System.Text.StringBuilder();
                var found = FoundAlong("dorestad", to, out double km, way);
                Debug.Log($"Discoveries: Dorestat to {GameData.Port(to).name}, {km:0} km: she finds {found.Count}: {string.Join(", ", found)}. Her way:{way}");
                Assert.GreaterOrEqual(found.Count, least, $"from Dorestat to {GameData.Port(to).name} ({km:0} km) she finds {found.Count}: {string.Join(", ", found)}");
            }
        }
    }
}
