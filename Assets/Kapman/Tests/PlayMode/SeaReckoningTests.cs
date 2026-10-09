using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE STEERSMAN'S RECKONING, A SHORE, AND ROUGH WATER, ON THE CHART AS A PLAYER HAS IT (the user, 9 Oct 2026: "We need
    /// an intelligent realistic current and weather system that can blow and push the ship off course and rough ocean
    /// areas"; by the picker: "Always adrift", "Strained and held off"). Each with a sea the test has made, in place of
    /// the tide and the wind of the day:
    ///   - HIS LANDFALL IN SIGHT: set a little sideways on her way in to Walichrum from the open sea, she comes within
    ///     sight of the end of her open water, he knows where he is, and her course is laid again from there by itself;
    ///   - HIS RECKONING LOST: held back by a sea that runs against her, she has run her distance by his reckoning
    ///     with her landfall nowhere in sight: the log says so, she lies to, the clock drops back, and the Voyage
    ///     window lays her course again from where she truly is;
    ///   - LYING TO in deep water she is carried, in shallow water she lies to her anchor;
    ///   - A SHORE: driven on a shore harder than she can keep clear of, she is held off it (never on the land), it
    ///     works her hull, the log says so and the clock drops back;
    ///   - ROUGH WATER: in the reach of a shoal that breaks, the log names it, the clock drops back, she makes less
    ///     way, and the Wind and tide window says so.
    /// Pictures and what happened in Captures/sea.
    /// </summary>
    public class SeaReckoningTests
    {
        /// <summary>Where she is bound in the reckoning test: a haven come to from the open sea.</summary>
        const string Bound = "walichrum";

        static WorldXZ At(double lat, double lon)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldXZ(x, z);
        }

        static void Put(KapmanGame game, WorldXZ at, float heading)
        {
            var u = FloatingOrigin.ToUnity(new WorldPos(at.x, 0.0, at.z));
            game.ship.Teleport(new Vector3(u.x, 0f, u.z), heading);
            Exploration.See(at.x, at.z, 45000.0);                         // (the water round her is known: the pictures show it)
            Exploration.Apply();
        }

        /// <summary>The place `back` metres before a point of a line, along it.</summary>
        static WorldXZ Before(IList<WorldXZ> line, int point, double back)
        {
            for (int i = point; i > 0; i--)
            {
                double len = WorldXZ.Distance(line[i - 1], line[i]);
                if (back <= len)
                {
                    double k = back / len;
                    return new WorldXZ(line[i].x + (line[i - 1].x - line[i].x) * k, line[i].z + (line[i - 1].z - line[i].z) * k);
                }
                back -= len;
            }
            return line[0];
        }

        /// <summary>The clock runs at speed 5 until `done` says so (or so many game hours have passed, or the wall
        /// clock runs out): a card that comes up is answered, and the clock is put back to speed 5 when something
        /// else has dropped it.</summary>
        static IEnumerator Sail(ChartPlay play, KapmanGame game, double hours, System.Func<bool> done, float wallSeconds = 200f)
        {
            double from = game.Hours;
            float wall = Time.realtimeSinceStartup;
            while (!done() && game.Hours - from < hours && Time.realtimeSinceStartup - wall < wallSeconds)
            {
                if (play.FrontFloat() != null)
                    yield return SeaMovesHerTests.AnswerAll(play);
                if (!done() && play.FrontFloat() == null && (GameSpeed.Level < 4 || GameSpeed.Paused))
                    yield return play.Click(SeaMovesHerTests.Small(play, "5"), "speed 5");
                yield return null;
            }
        }

        [UnityTest, Timeout(1200000)]
        public IEnumerator TheSteersmanFindsHisLandfallOrLosesHisReckoning()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_reckoning_saves", p => play = p);
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;
            var dropped = new List<ClockReason>();
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
            var dry = new List<string>();
            System.Action<long> onStep = minute =>
            {
                var w = ChartWorld.ShipWorld();
                if (Adrift.Helm != Helm.Fast && Adrift.Helm != Helm.River && Adrift.Helm != Helm.ByEye && !Course.IsWater(w.x, w.z))
                    dry.Add($"minute {minute}: {w.x:0} {w.z:0} ({Adrift.Helm})");
            };
            KapmanGame.Stepped += onStep;
            var told = new StringBuilder();
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;
                var body = ChartBody.Current;
                // (the test's own sight of a landfall, 12 km: she is put 20 km out, where the game's 30 km would have
                // it in sight from the first minute, and there would be no reckoning to lose)
                SeaPush.Use(new SeaPushRules { landfallM = 12000.0 });

                // ---- she casts off, and is put on the open sea to the north-west of Walichrum, bound there: where the
                // open water of that course ends is her landfall, and she is put some 20 km before it along the course
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                var port = GameData.Port(Bound);
                Assert.IsNotNull(port, "no port " + Bound);
                var haven = At(port.lat, port.lon);
                var out20 = new WorldXZ(haven.x - 26000.0, haven.z + 26000.0);
                var mark = haven;
                float heading = 135f;
                double alongM = 0.0;
                for (int tries = 0; tries < 6; tries++)
                {
                    Assert.IsTrue(Course.IsWater(out20.x, out20.z), "the test's place off Walichrum is not water: " + out20.x + " " + out20.z);
                    Put(game, out20, heading);
                    game.ship.anchored = false;
                    Exploration.See(haven.x, haven.z, 45000.0);
                    Exploration.Apply();
                    Assert.IsTrue(Course.SetTo(Bound, ChartWorld.ShipWorld(), null), "no course to Walichrum from the open water");
                    Assert.IsTrue(Course.LegIsOpen(1), "her first leg is not open water");
                    int end = Course.OpenRunEnd(1);
                    mark = Course.Points[end];
                    alongM = 0.0;
                    for (int i = 1; i <= end; i++)
                        alongM += WorldXZ.Distance(Course.Points[i - 1], Course.Points[i]);
                    heading = Mathf.Atan2((float)(Course.Points[1].x - out20.x), (float)(Course.Points[1].z - out20.z)) * Mathf.Rad2Deg;
                    told.AppendLine($"try {tries}: at {out20.x:0} {out20.z:0}; her landfall at {mark.x:0} {mark.z:0}, {alongM / 1000.0:0.0} km on along her course, {WorldXZ.Distance(mark, haven) / 1000.0:0.0} km from the haven; {Course.Points.Count} points");
                    if (alongM >= 15000.0 && alongM <= 25000.0)
                        break;
                    out20 = alongM > 25000.0 ? Before(Course.Points, end, 20000.0) : new WorldXZ(out20.x - 6000.0, out20.z + 6000.0);
                }
                File.WriteAllText(Path.Combine(play.dir, "sea_reckoning.txt"), told.ToString());
                Assert.That(alongM, Is.InRange(15000.0, 25000.0), "she could not be put some 20 km before the end of her open water:\n" + told);
                Adrift.Forget();

                // ======== HIS LANDFALL IN SIGHT: set a little sideways, he sees the mouth and knows where he is
                SeaPush.Fixed = at =>
                {
                    double h = game.ship.Heading * System.Math.PI / 180.0, v = System.Math.Max(0.3, body.Knots * 0.5144) * 0.12;
                    return (System.Math.Cos(h) * v, -System.Math.Sin(h) * v);       // (on her starboard beam, an eighth of her way)
                };
                int logFrom = FrameLog.Lines.Count;
                bool reckoned = false;
                double mostSet = 0.0;
                yield return Sail(play, game, 14.0, () =>
                {
                    reckoned |= Adrift.Helm == Helm.Reckoning || Adrift.Helm == Helm.Coasting;
                    mostSet = System.Math.Max(mostSet, Adrift.SetM);
                    return reckoned && Adrift.Helm == Helm.ByEye;
                });
                var seen = ChartWorld.ShipWorld();
                told.AppendLine($"landfall: reckoned {reckoned}; the most she was set {mostSet:0} m; now helm {Adrift.Helm}, set {Adrift.SetM:0} m, {WorldXZ.Distance(seen, mark) / 1000.0:0.0} km from her landfall, sight {Adrift.Sight() / 1000.0:0.0} km; course from {Course.Points[0].x:0} {Course.Points[0].z:0}");
                File.WriteAllText(Path.Combine(play.dir, "sea_reckoning.txt"), told.ToString());
                Assert.IsTrue(reckoned, "20 km out on open water he did not steer by reckoning:\n" + told);
                Assert.AreEqual(Helm.ByEye, Adrift.Helm, "she came in towards Walichrum and the steersman never had his landfall in sight:\n" + told);
                Assert.Greater(mostSet, Adrift.RelayM, "the test's sea did not set her:\n" + told);
                Assert.Less(Adrift.SetM, 1.0, "with his landfall in sight he is still out in his reckoning");
                Assert.LessOrEqual(WorldXZ.Distance(seen, mark), Adrift.Sight() + 600.0, "he knew his place before his landfall was in sight:\n" + told);
                Assert.IsTrue(Course.IsSet && Course.Destination == Bound, "the landfall lost her course");
                Assert.Less(WorldXZ.Distance(Course.Points[0], seen), 1500.0, "with the landfall in sight her course was not laid again from where she is:\n" + told);
                Assert.IsFalse(FrameLog.Lines.Skip(logFrom).Any(l => l.text.Contains("cannot make it out")), "the reckoning was called lost with the landfall in sight");
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                map.Follow();
                yield return ChartPlay.Settle(0.6f);
                yield return play.Shot("sea_4_the_landfall_in_sight");

                // ======== HIS RECKONING LOST: a sea that runs against her; by his reckoning she is there, and she is not
                Put(game, out20, heading);
                game.ship.anchored = false;
                Assert.IsTrue(Course.SetTo(Bound, ChartWorld.ShipWorld(), null));
                Adrift.Forget();
                SeaPush.Fixed = at =>
                {
                    double h = game.ship.Heading * System.Math.PI / 180.0, v = System.Math.Max(0.3, body.Knots * 0.5144) * 0.9;
                    return (-System.Math.Sin(h) * v, -System.Math.Cos(h) * v);      // (dead against her, nine tenths of her way)
                };
                dropped.Clear();
                logFrom = FrameLog.Lines.Count;
                bool lost = false;
                yield return Sail(play, game, 30.0, () =>
                {
                    for (int i = logFrom; i < FrameLog.Lines.Count && !lost; i++)
                        lost = FrameLog.Lines[i].text.Contains("and he cannot make it out");
                    return lost;
                }, 300f);
                var astray = ChartWorld.ShipWorld();
                told.AppendLine($"lost {lost}: at {astray.x:0} {astray.z:0}, {WorldXZ.Distance(astray, mark) / 1000.0:0.0} km from her landfall; helm {Adrift.Helm}; course set {Course.IsSet}; bound {Adrift.LostBound}; anchored {game.ship.anchored}; speed {GameSpeed.Level + 1}; drops {string.Join(" ", dropped)}");
                told.AppendLine("the log: " + string.Join(" | ", FrameLog.Lines.Skip(logFrom).Select(l => l.text)));
                File.WriteAllText(Path.Combine(play.dir, "sea_reckoning.txt"), told.ToString());
                Assert.IsTrue(lost, "held back by a sea dead against her she ran her distance and nobody said the landfall was missing:\n" + told);
                SeaPush.Fixed = at => (0.0, 0.0);
                yield return ChartPlay.Frames(6);
                Assert.IsFalse(Course.IsSet, "the reckoning is lost and she still has her course");
                Assert.AreEqual(Bound, Adrift.LostBound, "it is not kept where she was bound");
                Assert.IsTrue(game.ship.anchored, "the reckoning is lost and she does not lie to");
                Assert.Greater(WorldXZ.Distance(astray, mark), Adrift.Sight(), "the reckoning was called lost with her landfall in sight:\n" + told);
                Assert.Contains(ClockReason.OffLine, dropped, "the clock did not drop back when the reckoning was lost");
                Assert.AreEqual(0, GameSpeed.Level, "the clock did not drop to speed 1");
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Voyage"), "Voyage");
                yield return ChartPlay.Settle(0.8f);
                string reads = ChartPlay.Read(frame.Root);
                StringAssert.Contains("The reckoning is lost", reads, "the Voyage window does not say the reckoning is lost");
                map.Follow();
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_5_the_reckoning_lost");
                yield return play.ClickWhen(() => play.Button("Lay the course again", frame.Root), "Lay the course again");
                yield return ChartPlay.Frames(6);
                Assert.IsTrue(Course.IsSet && Course.Destination == Bound, "the Voyage window did not lay her course again");
                Assert.Less(WorldXZ.Distance(Course.Points[0], astray), 1500.0, "the course laid again does not start where she truly is");
                Assert.AreEqual("", Adrift.LostBound, "her course is laid again and the reckoning still counts as lost");
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_6_laid_again_after_the_reckoning_was_lost");
                Assert.IsEmpty(dry, "she was carried on to the land: " + string.Join("; ", dry.Take(5)));
            }
            finally
            {
                SeaPush.Fixed = null;
                SeaPush.Use(null);
                KapmanGame.Stepped -= onStep;
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (told.Length > 0 && play != null)
                    File.WriteAllText(Path.Combine(play.dir, "sea_reckoning.txt"), told.ToString());
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }

        [UnityTest, Timeout(1200000)]
        public IEnumerator LyingToSheIsCarriedAShoreHoldsHerOffAndRoughWaterIsRough()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_shore_saves", p => play = p);
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;
            var dropped = new List<ClockReason>();
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
            var dry = new List<string>();
            bool onShore = false, strained = false;
            System.Action<long> onStep = minute =>
            {
                var w = ChartWorld.ShipWorld();
                onShore |= Adrift.OnShore;
                strained |= Adrift.Strained;
                if (Adrift.Helm != Helm.Fast && Adrift.Helm != Helm.River && Adrift.Helm != Helm.ByEye && !Course.IsWater(w.x, w.z))
                    dry.Add($"minute {minute}: {w.x:0} {w.z:0} ({Adrift.Helm})");
            };
            KapmanGame.Stepped += onStep;
            var told = new StringBuilder();
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var frame = play.Frame;
                var body = ChartBody.Current;
                Assert.IsTrue(Course.KnowsWater, "no world bake to ask what is water");

                // ---- she casts off and is put out on deep water, with no course: she lies to
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Traiectum");
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                var deep = At(52.5, 3.0);
                Assert.Greater(SeaPush.Depth(deep), SeaPush.Rules.anchorDepthM + 5.0, "the test's deep water is not deep");
                Put(game, deep, 0f);
                Course.Clear();
                Adrift.Forget();
                SeaPush.Fixed = at => (0.5, 0.0);                             // (half a metre a second to the east)

                // ======== LYING TO in deep water she is carried
                yield return Sail(play, game, 3.0, () => game.ship.anchored && game.Hours > 0 && WorldXZ.Distance(ChartWorld.ShipWorld(), deep) > 2500.0);
                var carried = ChartWorld.ShipWorld();
                told.AppendLine($"lying to in {SeaPush.Depth(deep):0} m: carried {carried.x - deep.x:0} m east, {carried.z - deep.z:0} m north; helm {Adrift.Helm}; anchored {game.ship.anchored}");
                Assert.IsTrue(game.ship.anchored, "with no course she does not lie to");
                Assert.IsFalse(Course.IsSet);
                Assert.AreEqual(Helm.Drifting, Adrift.Helm, "lying to in deep water she is not adrift:\n" + told);
                Assert.Greater(carried.x - deep.x, 2000.0, "lying to in deep water in a sea setting east she was not carried east:\n" + told);
                Assert.Less(System.Math.Abs(carried.z - deep.z), 400.0, "she was carried north or south by a sea setting east:\n" + told);
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                map.Follow();
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_7_lying_to_and_carried");

                // ======== in shallow water she lies to her anchor
                var sands = RoughWaters.All.First(w => w.id == "long_sands").at;
                Assert.Less(SeaPush.Depth(sands), SeaPush.Rules.anchorDepthM, "the test's shallow water is not shallow");
                Assert.IsTrue(Course.IsWater(sands.x, sands.z));
                Put(game, sands, 0f);
                Adrift.Forget();
                yield return ChartPlay.Frames(4);
                double h0 = game.Hours;
                yield return Sail(play, game, 1.0, () => game.Hours - h0 >= 0.75);
                var held = ChartWorld.ShipWorld();
                told.AppendLine($"lying to in {SeaPush.Depth(sands):0.0} m with {body.Wind:0} m/s of wind: moved {WorldXZ.Distance(held, sands):0} m; helm {Adrift.Helm}");
                if (body.Wind <= GaleDice.GaleWind)
                {
                    Assert.AreEqual(Helm.Anchored, Adrift.Helm, "lying to in shallow water she does not lie to her anchor:\n" + told);
                    Assert.Less(WorldXZ.Distance(held, sands), 50.0, "lying to her anchor she was carried:\n" + told);
                }

                // ======== ROUGH WATER: on these same sands, with a sea the test has made rough
                RoughWaters.Fixed = w => 1.0;
                RoughWaters.Forget();
                dropped.Clear();
                int logFrom = FrameLog.Lines.Count;
                double hullBefore = Boat.Hull;
                var north = new WorldXZ(sands.x, sands.z + 30000.0);
                Assert.IsTrue(Course.SetToPlace(north, ChartWorld.ShipWorld(), null), "no course north from the long sands");
                SeaPush.Fixed = at => (0.0, 0.0);
                bool rough = false;
                yield return Sail(play, game, 2.0, () =>
                {
                    for (int i = logFrom; i < FrameLog.Lines.Count && !rough; i++)
                        rough = FrameLog.Lines[i].text.Contains("the sea breaks on the shoals");
                    return rough;
                });
                told.AppendLine($"rough water: said {rough}; now {(RoughWaters.Now != null ? RoughWaters.Now.id : "none")} at {RoughWaters.RoughNow:0.00}; slow {RoughWaters.Slow:0.00}; drops {string.Join(" ", dropped)}");
                told.AppendLine("the log: " + string.Join(" | ", FrameLog.Lines.Skip(logFrom).Select(l => l.text)));
                File.WriteAllText(Path.Combine(play.dir, "sea_shore.txt"), told.ToString());
                Assert.IsTrue(rough, "on the long sands, with the sea made rough, the log never said so:\n" + told);
                Assert.IsNotNull(RoughWaters.Now, "she is on the long sands and in no rough water");
                Assert.AreEqual(1.0, RoughWaters.RoughNow, 1e-6);
                Assert.AreEqual(SeaPush.Rules.roughSlow, RoughWaters.Slow, 1e-6, "rough water does not take her way off");
                Assert.Contains(ClockReason.RoughWater, dropped, "the clock did not drop back in rough water");
                Assert.IsTrue(FrameLog.Lines.Skip(logFrom).Any(l => l.text.StartsWith("The long sands: the sea breaks on the shoals")),
                              "the rough water is not said by the name of the thing found there:\n" + told);
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                frame.Show("sky");
                yield return ChartPlay.Settle(0.8f);
                StringAssert.Contains("Rough water", ChartPlay.Read(frame.Root), "the Wind and tide window does not say she is in rough water");
                map.Follow();
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_8_rough_water_on_the_sands");
                // an hour or two of it works her hull
                yield return Sail(play, game, 2.2, () => false);
                Assert.Less(Boat.Hull, hullBefore - SeaPush.Rules.roughWear * 0.9, "two hours of rough water did not work her hull");
                RoughWaters.Fixed = w => 0.0;

                // ======== A SHORE: off Katwijk on a coasting course, a sea that drives her on the land harder than she can claw off
                var off = At(52.24, 4.33);
                Assert.IsTrue(Course.IsWater(off.x, off.z), "the test's place off the Holland shore is not water");
                Put(game, off, 20f);
                game.ship.anchored = false;
                var along = At(52.62, 4.52);
                Exploration.See(along.x, along.z, 45000.0);
                Exploration.Apply();
                Assert.IsTrue(Course.SetToPlace(along, ChartWorld.ShipWorld(), null), "no course up the Holland shore");
                Adrift.Forget();
                SeaPush.Fixed = at => (2.2, 0.0);                             // (over four knots, straight on to the shore)
                dry.Clear();
                dropped.Clear();
                onShore = strained = false;
                logFrom = FrameLog.Lines.Count;
                double hull0 = Boat.Hull;
                bool said = false;
                yield return Sail(play, game, 4.0, () =>
                {
                    for (int i = logFrom; i < FrameLog.Lines.Count && !said; i++)
                        said = FrameLog.Lines[i].text == Adrift.LeeShoreWords || FrameLog.Lines[i].text == Adrift.LeeShoreGaleWords;
                    return said && Boat.Hull < hull0 - 0.002;
                });
                var ashore = ChartWorld.ShipWorld();
                told.AppendLine($"a shore: said {said}; on shore {onShore}; strained {strained}; hull {hull0:0.0000} -> {Boat.Hull:0.0000}; at {ashore.x:0} {ashore.z:0} (water {Course.IsWater(ashore.x, ashore.z)}); helm {Adrift.Helm}; drops {string.Join(" ", dropped)}; dry {dry.Count}");
                told.AppendLine("the log: " + string.Join(" | ", FrameLog.Lines.Skip(logFrom).Select(l => l.text)));
                File.WriteAllText(Path.Combine(play.dir, "sea_shore.txt"), told.ToString());
                Assert.IsEmpty(dry, "she was put on the land: " + string.Join("; ", dry.Take(5)) + "\n" + told);
                Assert.IsTrue(Course.IsWater(ashore.x, ashore.z), "she lies on the land:\n" + told);
                Assert.IsTrue(onShore, "a sea driving her on the shore never brought her to it:\n" + told);
                Assert.IsTrue(strained && said, "driven on a shore harder than she can keep clear of, nothing was said and nothing worked her:\n" + told);
                Assert.Less(Boat.Hull, hull0, "the shore did not work her hull");
                Assert.Contains(ClockReason.LeeShore, dropped, "the clock did not drop back on a lee shore");
                Assert.IsFalse(Boat.IsLost, "an hour on a shore in fair weather lost her");
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                map.Follow();
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_9_held_off_a_shore");
            }
            finally
            {
                SeaPush.Fixed = null;
                RoughWaters.Fixed = null;
                KapmanGame.Stepped -= onStep;
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (told.Length > 0 && play != null)
                    File.WriteAllText(Path.Combine(play.dir, "sea_shore.txt"), told.ToString());
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
