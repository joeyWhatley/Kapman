using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Weather;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SEA MOVES HER (the user, 9 Oct 2026: "We need an intelligent realistic current and weather system that can
    /// blow and push the ship off course and rough ocean areas"; by the picker: "Always adrift", "Strained and held
    /// off"). Held here, without the screen:
    ///   - the tide's stream, reckoned from the tide model: it turns with the tide, runs hard in the narrow seas and
    ///     hardly in the Baltic, harder in shallow water, and not at all inland;
    ///   - the wind's push: leeway to leeward under sail, most when beating; bodily downwind lying to; none in a calm;
    ///   - the table of its numbers holds only names the code reads;
    ///   - a course laid from open water goes on from where she is: it does not sail back to the lanes first, its
    ///     first stretch is open water, and the legs of a course know open water from a river;
    ///   - the rough waters: each a real place she can come to, tied to a thing to find; a race is quiet at slack
    ///     water and worst with the wind against the tide; a bar is worst with the wind on it; rough water comes
    ///     aboard and works the hull.
    /// What she does on the chart (carried off her line, the log and the clock, the course laid again, held off a
    /// shore) is held by SeaMovesHerTests (play).
    /// </summary>
    public class SeaMovesHerEditTests
    {
        const double June10Of793 = 1721423.5 + 365.0 * 792 + 198 + 160;

        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + name);

        [SetUp]
        public void Begin()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            SeaPush.Use(null);
            SeaPush.Fixed = null;
            RoughWaters.Use(null);
            RoughWaters.Fixed = null;
            Discoveries.Reset();
            Course.Clear();
            Boat.Begin(new BoatRules());
        }

        [TearDown]
        public void Done()
        {
            SeaPush.Use(null);
            SeaPush.Fixed = null;
            RoughWaters.Use(null);
            RoughWaters.Fixed = null;
            Discoveries.Reset();
            Course.Clear();
        }

        static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }

        static TideModel Tides()
        {
            var model = SeaPush.Tides();
            if (model == null)
                Assert.Ignore("no baked tides at " + SeaPush.DefaultTides);
            return model;
        }

        static void NeedLanes()
        {
            if (!File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
        }

        /// <summary>The stream at a place over a day and an hour, every ten minutes: east, north (m/s).</summary>
        static List<(double e, double n)> Day(TideModel model, double lat, double lon, double depth, double jd = June10Of793)
        {
            var list = new List<(double, double)>();
            for (int k = 0; k < 150; k++)
            {
                model.StreamAt(lat, lon, model.At(jd + k / 144.0), depth, out double e, out double n);
                list.Add((e, n));
            }
            return list;
        }

        static double Most(List<(double e, double n)> day) => day.Max(v => Math.Sqrt(v.e * v.e + v.n * v.n));

        // ------------------------------------------------------------------ the tide's stream

        [Test]
        public void TheStreamTurnsWithTheTide()
        {
            var model = Tides();
            var day = Day(model, 51.0, 1.5, 30.0);                            // the narrows off Dover
            double most = Most(day);
            Assert.That(most, Is.InRange(0.7, 2.6), "the stream in the narrows off Dover should run at a knot and a half to five knots at its hardest (it runs to four)");
            int at = day.FindIndex(v => Math.Sqrt(v.e * v.e + v.n * v.n) >= most * 0.999);
            // it runs along the strait: to the north-east or to the south-west
            double bearing = (Math.Atan2(day[at].e, day[at].n) * 180.0 / Math.PI + 360.0) % 180.0;
            Assert.That(bearing, Is.InRange(20.0, 100.0), "the stream in the narrows should run along them, north-east and south-west");
            // some six hours on it runs the other way, and between the two it stands
            bool turned = false, slack = false;
            foreach (var v in day)
            {
                if (v.e * day[at].e + v.n * day[at].n < -0.4 * most * most)
                    turned = true;
                if (Math.Sqrt(v.e * v.e + v.n * v.n) < 0.25 * most)
                    slack = true;
            }
            Assert.IsTrue(turned, "in a day the stream off Dover never ran the other way");
            Assert.IsTrue(slack, "in a day the stream off Dover never stood");
        }

        [Test]
        public void ItRunsHardInTheNarrowSeasAndHardlyInTheBalticAndNotInland()
        {
            var model = Tides();
            double dover = Most(Day(model, 51.0, 1.5, 30.0));
            double north = Most(Day(model, 58.0, 2.0, 100.0));               // the middle of the northern North Sea
            double baltic = Most(Day(model, 56.0, 18.0, 80.0));
            double inland = Most(Day(model, 51.97, 5.34, 4.0));             // Dorestat
            Assert.Greater(dover, north * 3.0, $"the narrows ({dover:0.00} m/s) should run far harder than the open North Sea ({north:0.00})");
            Assert.Less(north, 0.5, "the open northern North Sea runs under a knot");
            Assert.Less(baltic, 0.06, "the Baltic has next to no tide");
            Assert.AreEqual(0.0, inland, 1e-9, "the tide runs no stream far inland");
        }

        [Test]
        public void ShallowWaterRunsHarderAndTheGameCapsIt()
        {
            var model = Tides();
            double deep = Most(Day(model, 53.0, 3.0, 60.0)), shallow = Most(Day(model, 53.0, 3.0, 12.0));
            Assert.Greater(shallow, deep * 1.3, "the same tide over shallower water should run harder");
            // the game's own asking: never over its cap, and scaled by its number
            var at = At(51.35, -3.6);                                         // the Severn sea, where the reckoning runs wild
            double most = 0.0;
            for (int k = 0; k < 150; k++)
            {
                var (e, n) = SeaPush.Stream(at, June10Of793 + k / 144.0, 10.0);
                most = Math.Max(most, Math.Sqrt(e * e + n * n));
            }
            Assert.LessOrEqual(most, SeaPush.Rules.tideMost + 1e-6, "the stream runs over the game's cap");
            SeaPush.Use(new SeaPushRules { tide = 0.0 });
            var none = SeaPush.Stream(at, June10Of793, 10.0);
            Assert.AreEqual(0.0, Math.Abs(none.east) + Math.Abs(none.north), 1e-12, "with the tide's number at 0 a stream still runs");
        }

        // ------------------------------------------------------------------ the wind's push

        [Test]
        public void UnderSailSheIsSetToLeewardAndMostWhenBeating()
        {
            SeaPush.Use(new SeaPushRules { seaShare = 0.0 });                 // (the wind's own part alone)
            // heading north at 2 m/s, the wind on her port beam (from the west): set to the east
            var reach = SeaPush.Wind(0.0, 2.0, 270.0, 8.0, true, false);
            Assert.Greater(reach.east, 0.15, "a west wind on a boat heading north should set her east");
            Assert.AreEqual(0.0, reach.north, 1e-9, "leeway is sideways: it should not move her along her heading");
            Assert.AreEqual(2.0 * Math.Tan(6.0 * Math.PI / 180.0), reach.east, 1e-6, "six degrees of leeway on a reach in 8 m/s");
            // the wind from the east: set to the west
            Assert.Less(SeaPush.Wind(0.0, 2.0, 90.0, 8.0, true, false).east, -0.15);
            // close to the wind she makes more leeway than with it abaft the beam
            double beat = Math.Abs(SeaPush.Wind(0.0, 2.0, 315.0, 8.0, true, false).east) + Math.Abs(SeaPush.Wind(0.0, 2.0, 315.0, 8.0, true, false).north);
            double run = Math.Abs(SeaPush.Wind(0.0, 2.0, 200.0, 8.0, true, false).east) + Math.Abs(SeaPush.Wind(0.0, 2.0, 200.0, 8.0, true, false).north);
            Assert.Greater(beat, run * 2.0, "beating she should be set far more than running");
            // more wind, more leeway; never over the most
            Assert.Greater(SeaPush.Wind(0.0, 2.0, 270.0, 14.0, true, false).east, reach.east * 1.4);
            Assert.LessOrEqual(SeaPush.Wind(0.0, 2.0, 315.0, 40.0, true, false).east, 2.0 * Math.Tan(25.0 * Math.PI / 180.0) + 1e-6);
        }

        [Test]
        public void LyingToSheIsBlownDownwindAndInACalmNothingMovesHer()
        {
            var r = new SeaPushRules { seaShare = 0.0 };
            SeaPush.Use(r);
            // a gale from the north-west, lying to: carried south-east
            var lying = SeaPush.Wind(40.0, 0.0, 315.0, 20.0, false, true);
            Assert.AreEqual(20.0 * r.windageLyingTo, Math.Sqrt(lying.east * lying.east + lying.north * lying.north), 1e-6);
            Assert.Greater(lying.east, 0.0);
            Assert.Less(lying.north, 0.0);
            // under oars she is blown less than lying to
            var oars = SeaPush.Wind(40.0, 1.5, 315.0, 20.0, false, false);
            Assert.Less(Math.Sqrt(oars.east * oars.east + oars.north * oars.north), 20.0 * r.windageLyingTo);
            var calm = SeaPush.Wind(40.0, 2.0, 315.0, 0.0, true, false);
            Assert.AreEqual(0.0, Math.Abs(calm.east) + Math.Abs(calm.north), 1e-12, "no wind should move her nowhere");
            // the top of the sea runs a little to the right of the wind
            SeaPush.Use(new SeaPushRules { windageLyingTo = 0.0, windageBare = 0.0 });
            var sea = SeaPush.Wind(0.0, 0.0, 0.0, 10.0, false, true);        // (a north wind blows south; the sea sets a little west of south)
            Assert.Less(sea.north, 0.0);
            Assert.Less(sea.east, 0.0, "the top of the sea should run to the right of where the wind blows");
            Assert.AreEqual(10.0 * SeaPush.Rules.seaShare, Math.Sqrt(sea.east * sea.east + sea.north * sea.north), 1e-6);
            Assert.AreEqual("south", SeaPush.Sets(0.0, -1.0));
            Assert.AreEqual("", SeaPush.Runs(0.05, 0.0), "a tenth of a knot is not worth a word");
            Assert.AreEqual("a knot", SeaPush.Runs(0.5144, 0.0));
        }

        /// <summary>A reckoning lost in thick air says that it is the air, and that he would wait for it to clear (10 Oct
        /// 2026: the long walk saw him lose it four times in four hours of fog, and nothing said why).</summary>
        [Test]
        public void AReckoningLostInThickAirSaysThatItIsTheAir()
        {
            string clear = Adrift.LostWords("Witla"), thick = Adrift.LostWords("Witla", true);
            StringAssert.Contains("off Witla", clear);
            StringAssert.Contains("and he cannot make it out", clear);
            StringAssert.DoesNotContain("thick", clear);
            StringAssert.Contains("off Witla", thick);
            StringAssert.Contains("and he cannot make it out", thick, "the tests that look for a lost reckoning in the log know it by these words");
            StringAssert.Contains("thick", thick);
            StringAssert.Contains("wait for it to clear", thick);
            StringAssert.Contains("in with the land", Adrift.LostWords(null, true), "a place pointed at has no name to be off");
        }

        [Test]
        public void TheTableOfItsNumbersHoldsOnlyNamesTheCodeReads()
        {
            var text = Asset(SeaPush.File);
            Assert.IsNotNull(text, SeaPush.File + " is not in Game/Data");
            var root = Json.Parse(text.text) as Dictionary<string, object>;
            Assert.IsNotNull(root);
            var fields = typeof(SeaPushRules).GetFields().Where(f => !f.IsStatic).Select(f => f.Name).ToList();
            foreach (string key in root.Keys)
                if (key != "about")
                    CollectionAssert.Contains(fields, key, "sea_push.json names a number the code does not read: " + key);
            foreach (string field in fields)
                Assert.IsTrue(root.ContainsKey(field), "sea_push.json leaves out " + field + " (it would not be seen to be tunable)");
            var rules = SeaPushRules.Read(root);
            Assert.IsTrue(rules.on, "the sea's push is switched off in the table");
            Assert.Greater(rules.offLineWarnM, 1000.0);
            Assert.AreEqual(3.5, SeaPushRules.Read(new Dictionary<string, object> { ["tideMost"] = 3.5 }).tideMost, 1e-9);
        }

        // ------------------------------------------------------------------ a course laid from open water

        [Test]
        public void ACourseLaidFromOpenWaterGoesOnFromWhereSheIs()
        {
            NeedLanes();
            if (!Course.KnowsWater)
                Assert.Ignore("no world bake to ask what is water");
            var line = new List<WorldXZ>();
            var water = new List<float>();
            var kinds = new List<byte>();
            var dorestat = GameData.Port("dorestad");
            Assert.IsTrue(Course.Plan("lundenwic", At(dorestat.lat, dorestat.lon), "dorestad", line, water, kinds, out double whole), "no way from Dorestat to Lundenwic");
            Assert.AreEqual(line.Count, kinds.Count);
            Assert.IsTrue(kinds[1] != LaneGraph.Sea && kinds[1] != LaneGraph.Coastal, "she leaves Dorestat by open water");
            CollectionAssert.Contains(kinds, LaneGraph.River, "the way from Dorestat has no river in it");
            // the longest open leg of the way, and a place eight km off its middle
            int leg = -1;
            double longest = 0.0;
            for (int i = 2; i < line.Count - 1; i++)
                if ((kinds[i] == LaneGraph.Sea || kinds[i] == LaneGraph.Coastal) && WorldXZ.Distance(line[i - 1], line[i]) > longest)
                {
                    longest = WorldXZ.Distance(line[i - 1], line[i]);
                    leg = i;
                }
            Assert.Greater(longest, 20000.0, "the way to Lundenwic has no long leg of open water");
            var a = line[leg - 1];
            var b = line[leg];
            double dx = (b.x - a.x) / longest, dz = (b.z - a.z) / longest;
            WorldXZ off = default;
            bool found = false;
            foreach (double side in new[] { 8000.0, -8000.0 })
            {
                off = new WorldXZ((a.x + b.x) * 0.5 - dz * side, (a.z + b.z) * 0.5 + dx * side);
                if (Course.OpenWater(off, b) && Course.IsWater(off.x, off.z))
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "no open water eight km off the middle of the longest open leg");
            var again = new List<WorldXZ>();
            var againKinds = new List<byte>();
            Assert.IsTrue(Course.Plan("lundenwic", off, null, again, null, againKinds, out double km), "no way to Lundenwic from open water");
            var end = line[line.Count - 1];
            Assert.Less(WorldXZ.Distance(again[0], off), 1.0, "the course does not begin where she is");
            Assert.AreEqual(LaneGraph.Sea, againKinds[1], "the stretch from her to the lanes is open water, whatever lane it comes to");
            Assert.Greater(WorldXZ.Distance(again[0], again[1]), 3000.0, "the course begins with a hop to the lanes' nearest point");
            // it goes on: its first stretch makes for the far end of the leg (or beyond), not back to its near end
            double ahead = (again[1].x - off.x) * dx + (again[1].z - off.z) * dz;
            Assert.Greater(ahead, 0.0, "a course laid from open water begins by sailing back along her way");
            Assert.Greater(WorldXZ.Distance(again[1], a), 1000.0, "it begins by going back to the point of the lanes she had passed");
            // and it is no longer than going straight to the leg's far end and on
            double rest = 0.0;
            for (int i = leg; i + 1 < line.Count; i++)
                rest += WorldXZ.Distance(line[i], line[i + 1]);
            // (it takes up the lanes at their own nearest point, which is not always the shortest way: a twelfth more at most)
            Assert.LessOrEqual(km * 1000.0, (WorldXZ.Distance(off, b) + rest) * 1.08, "the course laid from open water makes a dog-leg");
            // set, the legs of the course know open water from the rest
            Assert.IsTrue(Course.SetTo("lundenwic", off, null));
            Assert.IsTrue(Course.LegIsOpen(1), "her first leg is open water");
            int runEnd = Course.OpenRunEnd(1);
            Assert.Greater(runEnd, 0);
            Assert.IsTrue(runEnd == Course.Points.Count - 1 || !Course.LegIsOpen(runEnd + 1), "the run of open water does not end where the next leg is not open");
            Assert.IsFalse(Course.LegIsOpen(Course.Points.Count - 1), "the last leg in to Lundenwic's berth, up the river, is taken for open water");
            Assert.AreEqual(0.0, Course.OffLine(off), 1.0, "on the first point of her line she is off it");
            Assert.AreEqual(5000.0, Course.OffLine(new WorldXZ(off.x - (again[1].z - off.z) / WorldXZ.Distance(off, again[1]) * 5000.0,
                                                               off.z + (again[1].x - off.x) / WorldXZ.Distance(off, again[1]) * 5000.0)), 50.0,
                            "five km abeam of her first leg she is five km off her line");
        }

        [Test]
        public void TheSteersmanKnowsHisPlaceOnARiverAndReckonsItOnOpenWater()
        {
            NeedLanes();
            if (!Course.KnowsWater)
                Assert.Ignore("no world bake to ask what is water");
            Adrift.Reset();
            var dorestat = GameData.Port("dorestad");
            var home = At(dorestat.lat, dorestat.lon);
            Assert.IsTrue(Course.SetTo("lundenwic", home, "dorestad"));
            // on the way out from the berth he steers by eye; on the river the banks hold her
            var by = Adrift.SteersBy(Course.Points[0], 1.0 / 60.0, out bool lost);
            Assert.IsFalse(lost);
            Assert.That(Adrift.Helm, Is.EqualTo(Helm.ByEye).Or.EqualTo(Helm.River), "leaving the berth he steers by reckoning");
            int river = -1;
            for (int i = 2; i < Course.Points.Count - 1 && river < 0; i++)
                if (Course.LegKind(i) == LaneGraph.River && Course.LegKind(i + 1) == LaneGraph.River)
                    river = i;
            Assert.Greater(river, 0, "the way from Dorestat has no river in it");
            Course.Aim(Course.Points[river], 50.0, out _);               // (she has come that far along her line)
            by = Adrift.SteersBy(Course.Points[river], 1.0 / 60.0, out lost);
            Assert.IsFalse(lost);
            Assert.AreEqual(Helm.River, Adrift.Helm, "on the Rhine the banks hold her");
            Assert.Less(WorldXZ.Distance(by, Course.Points[river]), 1e-6);
            // out on the open water of her way, far from its end: by reckoning
            int open = -1;
            for (int i = 2; i < Course.Points.Count - 1 && open < 0; i++)
                if (Course.LegIsOpen(i) && WorldXZ.Distance(Course.Points[i], Course.Points[Course.OpenRunEnd(i)]) > 60000.0)
                    open = i;
            Assert.Greater(open, 0, "the way to Lundenwic has no open water far from its landfall");
            Assert.IsTrue(Course.SetTo("lundenwic", Course.Points[open], null));
            Assert.IsTrue(Course.LegIsOpen(1));
            by = Adrift.SteersBy(Course.Points[0], 1.0 / 60.0, out lost);
            Assert.IsFalse(lost);
            Assert.That(Adrift.Helm, Is.EqualTo(Helm.Reckoning).Or.EqualTo(Helm.Coasting), "on open water far from her landfall he steers by reckoning");
            Assert.Less(Adrift.SetM, 1e-6, "a course just laid has no set");
            Adrift.Reset();
            Course.Clear();
        }

        // ------------------------------------------------------------------ rough waters

        [Test]
        public void EachRoughWaterIsARealPlaceSheCanComeToAndAThingToFind()
        {
            Assert.IsNotNull(Asset(RoughWaters.File), RoughWaters.File + " is not in Game/Data");
            var all = RoughWaters.All;
            Assert.GreaterOrEqual(all.Count, 25, "there are few rough waters");
            Assert.AreEqual(all.Count, all.Select(w => w.id).Distinct().Count(), "a rough water is there twice");
            var kinds = new[] { "race", "bar", "shoal", "cape" };
            foreach (var w in all)
            {
                CollectionAssert.Contains(kinds, w.kind, w.id + " is of no kind the game knows");
                Assert.That(w.rough, Is.InRange(0.3, 2.0), w.id);
                Assert.That(w.reachM, Is.InRange(2000.0, 20000.0), w.id);
                Assert.IsNotEmpty(w.today, w.id + " does not say what the place is called now");
                Assert.IsNotEmpty(w.find, w.id + " is tied to no thing to find: she could never have a name for it");
                var find = Discoveries.Book.Get(w.find);
                Assert.IsNotNull(find, w.id + " is tied to a thing that is not in the table of things to find: " + w.find);
                if (w.kind == "bar")
                    Assert.IsFalse(double.IsNaN(w.openTo), w.id + " is a bar and does not say which way it lies open");
                Assert.AreSame(w, RoughWaters.At(w.at), w.id + " is not the rough water at its own place");
            }
            // a place in the open sea is in none
            Assert.IsNull(RoughWaters.At(At(56.0, 3.0)));
            // names: by what it plainly is until the thing is found, then by its name
            var firth = all.First(w => w.id == "picts_firth");
            Assert.AreEqual("A tide race", RoughWaters.NameOf(firth));
            StringAssert.StartsWith("A tide race: ", RoughWaters.Words(firth, true));
        }

        [Test]
        public void ARaceIsQuietAtSlackWaterAndWorstWithTheWindAgainstTheTide()
        {
            Tides();
            var race = new RoughWater { id = "test_narrows", kind = "race", lat = 51.0, lon = 1.5, reachM = 10000.0, rough = 1.0, find = "narrows" };
            RoughWaters.Use(new[] { race });
            // the hour of the hardest stream and of the slackest, in a day of June 793
            double hard = 0.0, hardAt = 0.0, slack = double.MaxValue, slackAt = 0.0, hardE = 0.0, hardN = 0.0;
            for (int k = 0; k < 150; k++)
            {
                double jd = June10Of793 + k / 144.0;
                var (e, n) = SeaPush.Stream(race.at, jd);
                double v = Math.Sqrt(e * e + n * n);
                if (v > hard)
                {
                    hard = v;
                    hardAt = jd;
                    hardE = e;
                    hardN = n;
                }
                if (v < slack)
                {
                    slack = v;
                    slackAt = jd;
                }
            }
            Assert.Greater(hard, 0.3, "the test's race has no stream to speak of");
            // where the stream sets, as a compass bearing: a wind FROM there blows against it
            double sets = (Math.Atan2(hardE, hardN) * 180.0 / Math.PI + 360.0) % 360.0;
            double against = RoughWaters.Roughness(race, 12.0, sets, hardAt), with = RoughWaters.Roughness(race, 12.0, (sets + 180.0) % 360.0, hardAt);
            double calm = RoughWaters.Roughness(race, 0.0, 0.0, hardAt), atSlack = RoughWaters.Roughness(race, 12.0, sets, slackAt);
            Assert.Greater(against, with * 1.8, $"wind against tide ({against:0.00}) should be far worse than wind with it ({with:0.00})");
            Assert.Greater(against, 0.5);
            Assert.Greater(calm, 0.1, "a race boils at the height of the stream even in a calm");
            Assert.Less(calm, against);
            Assert.Less(atSlack, against * 0.4, $"at slack water ({atSlack:0.00}) a race should be far quieter than at the height of the stream ({against:0.00})");
            Assert.LessOrEqual(against, race.rough * 1.2 + 1e-9);
            // a test's own sea stands in for the reckoning
            RoughWaters.Fixed = w => 0.75;
            Assert.AreEqual(0.75, RoughWaters.Roughness(race, 0.0, 0.0, hardAt), 1e-9);
        }

        [Test]
        public void ABarIsWorstWithTheWindOnItAndAShoalBreaksInAFreshWind()
        {
            Tides();
            var bar = new RoughWater { id = "test_bar", kind = "bar", lat = 51.98, lon = 4.02, reachM = 6000.0, rough = 1.0, openTo = 280.0 };
            var shoal = new RoughWater { id = "test_shoal", kind = "shoal", lat = 51.25, lon = 1.55, reachM = 8000.0, rough = 1.0 };
            RoughWaters.Use(new[] { bar, shoal });
            double on = RoughWaters.Roughness(bar, 12.0, 280.0, June10Of793), offIt = RoughWaters.Roughness(bar, 12.0, 100.0, June10Of793);
            Assert.Greater(on, offIt * 2.5, $"a wind on the bar ({on:0.00}) should be far worse than a wind off the land ({offIt:0.00})");
            Assert.AreEqual(0.0, RoughWaters.Roughness(bar, 2.0, 280.0, June10Of793), 1e-9, "a light air raises nothing on a bar");
            Assert.AreEqual(0.0, RoughWaters.Roughness(shoal, 3.0, 0.0, June10Of793), 1e-9);
            Assert.Greater(RoughWaters.Roughness(shoal, 14.0, 0.0, June10Of793), RoughWaters.Roughness(shoal, 8.0, 0.0, June10Of793));
            Assert.Greater(RoughWaters.Roughness(shoal, 14.0, 0.0, June10Of793), 0.6);
        }

        [Test]
        public void RoughWaterComesAboardAndWorksTheHull()
        {
            BoatHour Hour(long hour, double rough) => new BoatHour
            {
                hour = hour, wind = 6.0, offWind = 150.0, underWay = true, moored = false, river = false, night = false, galeIn = double.PositiveInfinity,
                men = 4, hands = 4, length = 7.7, beam = 2.2, fragility = 0.6, lossPerHour = 0.0004, seed = 793, rough = rough,
            };
            Boat.Begin(new BoatRules());
            for (int h = 0; h < 6; h++)
                Boat.Hour(Hour(h, 0.0));
            double hullQuiet = Boat.Hull;
            Assert.AreEqual(0, Boat.Bailers, "in a moderate breeze on quiet water nobody bails");
            Boat.Begin(new BoatRules());
            for (int h = 0; h < 6; h++)
                Boat.Hour(Hour(h, 1.0));
            Assert.Less(Boat.Hull, hullQuiet - 5.0 * SeaPush.Rules.roughWear * 0.9, "six hours of rough water did not work her hull");
            Assert.GreaterOrEqual(Boat.Bailers, 1, "rough water came aboard and nobody bails");
            Assert.IsFalse(Boat.IsLost, "six hours in a race lost a sound boat");
            // made fast, it does nothing
            Boat.Begin(new BoatRules());
            var moored = Hour(0, 1.0);
            moored.moored = true;
            moored.underWay = false;
            double before = Boat.Hull;
            Boat.Hour(moored);
            Assert.AreEqual(before, Boat.Hull, 1e-12);
        }
    }
}
