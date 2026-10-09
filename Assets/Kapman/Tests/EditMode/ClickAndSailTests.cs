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
    /// CLICK AND SAIL, AND THE RINGS OF HEARSAY (the user, 9 Oct 2026: "We need a more intuitive sailing system for
    /// navigating unknown waters", "Maybe don't even show dots for unfound locations, needs to be more vague"; by the
    /// picker: "Click and sail", "A wide hazy ring"). Held here, without the screen:
    ///   - a course can be charted to any place: it goes by water, and ends at the place where open water reaches it,
    ///     else as near it as the lanes come;
    ///   - a short hop at sea is a short hop: it does not begin by sailing back to the lanes' last crossing;
    ///   - such a course is set, said, saved, and taken up again after a wait;
    ///   - a port only heard of has a ring that holds it, never in its middle, the same in every game;
    ///   - a harbour that comes in sight is told once, and which have been told is saved.
    /// What the chart shows of all this is held by MapZoomTests and SailByPointingTests (play).
    /// </summary>
    public class ClickAndSailTests
    {
        static TextAsset Asset(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + name);

        [SetUp]
        public void Begin()
        {
            GameData.LoadGoods(Asset("goods_793.json"));
            GameData.LoadPorts(Asset("ports_793.json"));
            Discoveries.Reset();
            Sightings.Reset();
            Exploration.Clear();
            Course.Clear();
        }

        [TearDown]
        public void Done()
        {
            Discoveries.Reset();
            Sightings.Reset();
            Exploration.Clear();
            Course.Clear();
        }

        static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }

        static WorldXZ Of(string port)
        {
            var p = GameData.Port(port);
            return At(p.lat, p.lon);
        }

        static void NeedLanes()
        {
            if (!System.IO.File.Exists(SimRoutes.DefaultLanes))
                Assert.Ignore("no lanes baked at " + SimRoutes.DefaultLanes);
        }

        [Test]
        public void ACourseToAPlaceGoesByWaterAndEndsWhereSheCanCome()
        {
            NeedLanes();
            var line = new List<WorldXZ>();
            var water = new List<float>();
            var kinds = new List<byte>();
            // a place in the open sea, off the Maas mouth
            var sea = At(52.05, 3.85);
            Assert.IsTrue(Course.PlanToPlace(sea, Of("dorestad"), "dorestad", line, water, kinds, out double km), "no way from Dorestat to a place in the sea off the Maas mouth");
            Assert.AreEqual(line.Count, water.Count, "a point of the course has no water");
            Assert.AreEqual(line.Count, kinds.Count, "a point of the course has no kind of lane");
            Assert.That(km, Is.InRange(95.0, 190.0), "km by water from Dorestat to the sea off the Maas mouth");
            double offEnd = WorldXZ.Distance(line[line.Count - 1], sea);
            Assert.LessOrEqual(offEnd, Course.BeyondLanesM, "the course ends far from the place pointed at");
            // (where open water lies between the lanes and the place she stands on to it; where she has not, it does not)
            if (offEnd >= 1.0)
                Assert.IsFalse(offEnd > 50.0 && Course.OpenWater(line[line.Count - 1], sea), "open water lies between the lanes and the place, and she does not stand on to it");
            Debug.Log($"Click and sail: Dorestat to a place in the sea off the Maas mouth: {km:0} km, {line.Count} points; she ends {offEnd / 1000.0:0.0} km from it; the world bake answers: {MapLand.Shared() != null}");
            // the way begins down her own river, not over the land
            Assert.Less(WorldXZ.Distance(line[0], Of("dorestad")), 2000.0);
            Assert.Greater(kinds.Count(k => k == LaneGraph.River), 20, "from Dorestat the way to the sea should run down a river");

            // a place on dry land, the heath east of the Almere: she goes as near as the water lets her, and no nearer
            var heath = At(52.2, 5.9);
            Assert.IsTrue(Course.PlanToPlace(heath, Of("dorestad"), "dorestad", line, water, kinds, out km), "a place on land: no water led towards it");
            double shortOf = WorldXZ.Distance(line[line.Count - 1], heath);
            Assert.Greater(shortOf, 1500.0, "she sails onto dry land");
            Assert.Less(shortOf, 60000.0, "she lies to nowhere near the place");
            Debug.Log($"Click and sail: a place on the heath: she lies to {shortOf / 1000.0:0.0} km short of it, after {km:0} km");
        }

        /// <summary>
        /// POINTING AT THE MIDDLE OF A PORT'S RING BRINGS HER WITHIN SIGHT OF THE PORT (10 Oct 2026). The Voyage window
        /// tells the player to "point at the water there", and a port is found only by being seen. The long walk pointed
        /// at Lundenwic's ring from Sarre and she lay to in the Thames mouth, 50 km short: the deep lanes ended there,
        /// and a course to a place never tried the flats. Held for every port of the game that has a way from Dorestat:
        /// her course to the middle of its ring ends where the port lies within what she sees round her.
        /// </summary>
        [Test]
        public void PointingAtTheMiddleOfAPortsRingBringsHerWithinSightOfIt()
        {
            NeedLanes();
            var line = new List<WorldXZ>();
            var way = new List<WorldXZ>();
            var far = new List<string>();
            int held = 0;
            foreach (var def in GameData.Ports)
            {
                if (def.id == "dorestad" || !Course.Plan(def.id, Of("dorestad"), "dorestad", way, out _))
                    continue;
                var middle = Hearsay.RingMiddle(def);
                if (!Course.PlanToPlace(middle, Of("dorestad"), "dorestad", line, null, null, out double km))
                {
                    far.Add(def.name + ": no water leads towards its ring");
                    continue;
                }
                held++;
                double off = WorldXZ.Distance(line[line.Count - 1], Of(def.id));
                if (off > Exploration.NearSight)
                    far.Add($"{def.name}: she lies to {off / 1000.0:0} km from it, after {km:0} km");
            }
            Debug.Log($"Click and sail: pointing at the middle of its ring brings her within sight of {held - far.Count} of {held} ports that have a way from Dorestat");
            Assert.Greater(held, 30, "too few ports have a way from Dorestat for this to be a test");
            Assert.IsEmpty(far, "pointing at the middle of these ports' rings leaves her out of sight of them:\n" + string.Join("\n", far));
            // and as the walk had it: from Sarre, to the ring of Lundenwic
            var london = GameData.Port("lundenwic");
            Assert.IsTrue(Course.PlanToPlace(Hearsay.RingMiddle(london), Of("sarre"), "sarre", line, null, null, out double up));
            Assert.Less(WorldXZ.Distance(line[line.Count - 1], Of("lundenwic")), Exploration.NearSight, $"from Sarre she lies to out of sight of Lundenwic, after {up:0} km");
        }

        /// <summary>
        /// SHE NEVER STEERS FOR HER OWN PLACE (10 Oct 2026). The long walk cast off from Doccinga and she never moved
        /// again: her course went 150 m up the lane to its end and came back past the berth, so the point 300 m ahead
        /// along it was the spot she lay on. Held on a line made by hand that doubles back so, and for the courses out
        /// of every port to each port of the first release, laid from where she lies made fast on her way in (150 m
        /// short of the berth's own point): the point to steer for stands clear of her.
        /// </summary>
        [Test]
        public void ThePointSheSteersForIsNeverHerOwnPlace()
        {
            try
            {
                // out 150 m, and back along the same water past where she lies
                Course.Points.Clear();
                foreach (double z in new[] { 0.0, 150.0, 54.0, -42.0, -138.0, -500.0, -2000.0 })
                    Course.Points.Add(new WorldXZ(1000.0, z));
                var ship = new WorldXZ(1000.0, 0.0);
                var p = Course.Peek(ship, 300.0);
                Assert.GreaterOrEqual(WorldXZ.Distance(ship, p), Course.ClearOfHerM - 0.01, "on a line that doubles back she is given her own place to steer for");
                Assert.Less(p.z, 0.0, "the point to steer for is not on along the line, past where she lies");
                Assert.Greater(p.z, -150.0, "the point to steer for is far on along the line: " + p.z);
                // a line that does not double back: 300 m on, as it always was
                Course.Points.Clear();
                foreach (double z in new[] { 0.0, 100.0, 400.0, 2000.0 })
                    Course.Points.Add(new WorldXZ(1000.0, z));
                Assert.AreEqual(300.0, Course.Peek(ship, 300.0).z, 1e-6, "a straight line: the point 300 m on");
            }
            finally
            {
                Course.Points.Clear();
            }

            NeedLanes();
            var line = new List<WorldXZ>();
            var own = new List<string>();
            int courses = 0;
            foreach (var from in GameData.Ports)
                foreach (string to in new[] { "dorestad", "traiectum", "walichrum", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "lundenwic", "sarre", "fordwic", "ripa" })
                {
                    if (to == from.id || !Course.SetTo(to, Of(from.id), from.id) || Course.Points.Count < 4)
                    {
                        Course.Clear();
                        continue;
                    }
                    // WHERE SHE LIES MADE FAST: on her way in, 150 m short of the berth's own point (the line's second),
                    // on the water her course out runs over. Her course from there goes to the berth's point first
                    // and comes back past her.
                    var lies = Course.Points[Course.Points.Count - 1];
                    double along = 150.0;
                    for (int i = 2; i < Course.Points.Count; i++)
                    {
                        double len = WorldXZ.Distance(Course.Points[i - 1], Course.Points[i]);
                        if (along < len)
                        {
                            double k = along / len;
                            lies = new WorldXZ(Course.Points[i - 1].x + (Course.Points[i].x - Course.Points[i - 1].x) * k, Course.Points[i - 1].z + (Course.Points[i].z - Course.Points[i - 1].z) * k);
                            break;
                        }
                        along -= len;
                    }
                    Course.Clear();
                    if (!Course.SetTo(to, lies, from.id))
                        continue;
                    courses++;
                    var berth = Course.Points[0];
                    // (as it was before the mend: the point exactly 300 m on along the line from its beginning)
                    double left = 300.0;
                    var old = Course.Points[Course.Points.Count - 1];
                    for (int i = 1; i < Course.Points.Count; i++)
                    {
                        double len = WorldXZ.Distance(Course.Points[i - 1], Course.Points[i]);
                        if (left < len)
                        {
                            double k = left / len;
                            old = new WorldXZ(Course.Points[i - 1].x + (Course.Points[i].x - Course.Points[i - 1].x) * k, Course.Points[i - 1].z + (Course.Points[i].z - Course.Points[i - 1].z) * k);
                            break;
                        }
                        left -= len;
                    }
                    if (WorldXZ.Distance(berth, old) < 1.2)
                        own.Add(from.name + " for " + GameData.Port(to).name);
                    var steer = Course.Peek(berth, 300.0);
                    Assert.GreaterOrEqual(WorldXZ.Distance(berth, steer), System.Math.Min(Course.ClearOfHerM, WorldXZ.Distance(berth, Course.Points[Course.Points.Count - 1])) - 0.01,
                                          $"made fast at {from.name}, 150 m short of the berth's point, with her course for {GameData.Port(to).name}: the point she steers for is her own place");
                    Course.Clear();
                }
            Assert.Greater(courses, 300, "too few courses were laid for this to be a test");
            Debug.Log($"Click and sail: of {courses} courses out of a port, laid from 150 m short of its berth, {own.Count} had the point 300 m on along the line within 1.2 m of where she lay " +
                      "(she would have stood still before the mend): " + string.Join("; ", own.Take(40)) + (own.Count > 40 ? "; and more" : ""));
            Assert.Greater(own.Count, 0, "no course out of any port doubles back past where she lies: this test no longer lays her where the fault was");
        }

        [Test]
        public void AShortHopAtSeaIsAShortHop()
        {
            NeedLanes();
            // her course to Ripa, for a stretch of open sea to stand on
            var line = new List<WorldXZ>();
            var kinds = new List<byte>();
            Assert.IsTrue(Course.Plan("ripa", Of("dorestad"), "dorestad", line, null, kinds, out _));
            int i = Enumerable.Range(0, line.Count).Where(k => k > line.Count / 3 && k < line.Count * 2 / 3 && kinds[k] == LaneGraph.Sea).DefaultIfEmpty(-1).First();
            if (i < 0)
                i = line.Count / 2;
            var here = line[i];
            // eight km on along the same lane, and eight km back
            WorldXZ Along(int from, int step, double metres)
            {
                double run = 0.0;
                for (int k = from; k + step >= 0 && k + step < line.Count; k += step)
                {
                    double leg = WorldXZ.Distance(line[k], line[k + step]);
                    if (run + leg >= metres)
                    {
                        double t = (metres - run) / leg;
                        return new WorldXZ(line[k].x + (line[k + step].x - line[k].x) * t, line[k].z + (line[k + step].z - line[k].z) * t);
                    }
                    run += leg;
                }
                return line[step > 0 ? line.Count - 1 : 0];
            }
            var hop = new List<WorldXZ>();
            foreach (var (to, what) in new[] { (Along(i, 1, 8000.0), "eight km ahead"), (Along(i, -1, 8000.0), "eight km astern") })
            {
                Assert.IsTrue(Course.PlanToPlace(to, here, null, hop, null, null, out double km), "no way to a place " + what);
                Assert.Less(km, 13.0, $"a place {what} on her own lane is {km:0} km by the course: she is sent round by the lanes' crossing");
                Assert.Less(WorldXZ.Distance(hop[hop.Count - 1], to), 1500.0, $"the course to a place {what} does not end there");
            }
            // from a place off the lanes (where she lay to in open water) she goes back to them without a detour
            var off = new WorldXZ(here.x + 6000.0, here.z + 6000.0);
            Assert.IsTrue(Course.PlanToPlace(Along(i, 1, 20000.0), off, null, hop, null, null, out double back));
            Assert.Less(back, 45.0, "from a place six km off the lanes, twenty km on is " + back.ToString("0") + " km");
        }

        [Test]
        public void ACourseToAPlaceIsSetSavedAndTakenUpAgain()
        {
            NeedLanes();
            var place = At(52.05, 3.85);
            int changed = 0;
            System.Action onChange = () => changed++;
            Course.Changed += onChange;
            try
            {
                Assert.IsTrue(Course.SetToPlace(place, Of("dorestad"), "dorestad"));
                Assert.AreEqual(1, changed, "a course set to a place is not told");
                Assert.IsTrue(Course.IsSet && Course.ToPlace);
                Assert.AreEqual(Course.PlaceId, Course.Destination);
                Assert.IsNull(GameData.Port(Course.Destination), "the id of a place pointed at is a port's id");
                Assert.AreEqual(Course.PlaceWords, Course.BoundName);
                Assert.Less(WorldXZ.Distance(Course.Place, place), 0.001);
                Assert.Greater(Course.Km, 50.0);
                int points = Course.Points.Count;
                Assert.AreEqual(points, Course.Kinds.Count);
                // saved and back, through the save's own text
                var saved = Json.Parse(Json.Write(Course.Capture())) as Dictionary<string, object>;
                Course.Clear();
                Assert.IsFalse(Course.IsSet);
                Assert.AreEqual("", Course.BoundName);
                Course.Restore(saved);
                Assert.IsTrue(Course.ToPlace, "a saved course to a place came back as something else");
                Assert.AreEqual(points, Course.Points.Count);
                Assert.Less(WorldXZ.Distance(Course.Place, place), 0.01, "the place pointed at was not saved");
                // laid by for a wait (the frame keeps only where she was bound), and taken up again from where she is
                string bound = Course.Destination;
                Course.Clear();
                Assert.IsTrue(Course.SetTo(bound, Of("witla"), null), "a course to a place, laid by, cannot be taken up again");
                Assert.IsTrue(Course.ToPlace);
                Assert.Less(WorldXZ.Distance(Course.Place, place), 0.01);
                Assert.Less(Course.Km, 60.0, "taken up again at Witla, the place off the Maas mouth is near");
                // a port's course is a port's course still
                Assert.IsTrue(Course.SetTo("traiectum", Of("dorestad"), "dorestad"));
                Assert.IsFalse(Course.ToPlace);
                Assert.AreEqual(GameData.Port("traiectum").name, Course.BoundName);
            }
            finally
            {
                Course.Changed -= onChange;
                Course.Clear();
            }
        }

        [Test]
        public void ARingHoldsItsPortAndNeverInItsMiddle()
        {
            var eighths = new HashSet<int>();
            foreach (var def in GameData.Ports)
            {
                var middle = Hearsay.RingMiddle(def);
                var port = At(def.lat, def.lon);
                double off = WorldXZ.Distance(middle, port);
                Assert.That(off, Is.InRange(Hearsay.RingRadius * 0.25 - 1.0, Hearsay.RingRadius * 0.65 + 1.0), $"{def.name} lies {off / 1000.0:0.0} km from the middle of its ring of {Hearsay.RingRadius / 1000.0:0} km");
                var again = Hearsay.RingMiddle(def);
                Assert.AreEqual(middle.x, again.x, 1e-9, "a ring is not in the same place twice");
                Assert.AreEqual(middle.z, again.z, 1e-9);
                eighths.Add((int)System.Math.Floor((System.Math.Atan2(port.z - middle.z, port.x - middle.x) + System.Math.PI) / (System.Math.PI / 4.0)) % 8);
            }
            Assert.GreaterOrEqual(eighths.Count, 7, "the ports all lie the same way from the middles of their rings: that is a rule to read, not a ring");
        }

        [Test]
        public void AHarbourComesInSightOnceAndIsSaved()
        {
            var home = Of("dorestad");
            Exploration.Begin(home.x - 600000.0, home.z + 600000.0, home.x + 600000.0, home.z - 600000.0);
            KapmanGame.KnowStart(GameData.Port("dorestad"));
            var witla = GameData.Port("witla");
            Known.HearOf(witla);
            Assert.IsTrue(Sightings.Told("dorestad") && Sightings.Told("traiectum"), "the ports of the first morning are to be sighted still");
            Assert.IsFalse(Sightings.Told("witla"));
            Assert.IsEmpty(Sightings.Step(793), "a harbour came in sight on the first morning with nothing lifted");
            Assert.IsTrue(Hearsay.Found(GameData.Port("traiectum")) && !Hearsay.Found(witla));
            Assert.IsTrue(Hearsay.Ringed(witla), "heard of and not found, Witla has no ring");
            Assert.IsFalse(Hearsay.Ringed(GameData.Port("traiectum")), "a port she has found has a ring");
            CollectionAssert.Contains(Hearsay.RingsAt(Hearsay.RingMiddle(witla), 793), witla);
            StringAssert.Contains(witla.name, Hearsay.Words(Hearsay.RingsAt(Hearsay.RingMiddle(witla), 793)));
            Assert.AreEqual("", Hearsay.Words(new List<PortDef>()));

            // she comes down the Lek: the fog lifts over Witla
            var at = Of("witla");
            Exploration.Reveal(at.x, at.z, Exploration.Sight);
            var now = Sightings.Step(793);
            Assert.AreEqual("witla", now.Single().id, "the harbour that came in sight was not told");
            Assert.IsEmpty(Sightings.Step(793), "a harbour is told twice");
            Assert.IsTrue(Hearsay.Found(witla) && !Hearsay.Ringed(witla), "found, Witla still has its ring");
            string line = Sightings.Line(witla, new WorldXZ(at.x + 20000.0, at.z));
            StringAssert.StartsWith("A harbour in sight to the west: " + witla.name, line);
            StringAssert.Contains("heard of", line);

            // saved and back
            Assert.IsTrue(ContentSave.Registered("sighted"), "the harbours sighted are not a part of the saved game");
            var saved = Json.Parse(Json.Write(ContentSave.Capture())) as Dictionary<string, object>;
            Sightings.Forget();
            Assert.IsFalse(Sightings.Told("witla"));
            ContentSave.Restore(saved);
            Assert.IsTrue(Sightings.Told("witla") && Sightings.Told("dorestad"));
            Assert.IsEmpty(Sightings.Step(793), "a loaded game was told again of a harbour it had seen");
            // a game saved before sightings were kept: what she has seen is not told as new
            var older = new Dictionary<string, object>(saved);
            older.Remove("sighted");
            ContentSave.Restore(older);
            Assert.IsFalse(Sightings.Told("witla"));
            Assert.IsEmpty(Sightings.Step(793), "an older save was told of old harbours as new");
            Assert.IsTrue(Sightings.Told("witla"));
        }
    }
}
