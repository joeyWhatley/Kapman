using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Chain;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Ships;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The playability design's first-slice tests (4.4): the beats on the scripted Dorestad route, the sail's
    /// speed against the world's ships' speed model, and the lessons offered once per house.</summary>
    public class BeatTests
    {
        static string Text(string assetPath) => File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), assetPath));

        static int Nearest(List<Vector2> line, Vector2 p)
        {
            int best = 0;
            for (int i = 1; i < line.Count; i++)
                if (Vector2.Distance(line[i], p) < Vector2.Distance(line[best], p))
                    best = i;
            return best;
        }

        /// <summary>The route from the berth to the way out: the channel from the point after the one nearest the berth
        /// to the point before the one nearest the "down" anchor.</summary>
        static List<Vector2> Route(out Vector2 exitAt)
        {
            var d = ChainData.Load(ChainData.ProjectPath).Level("dorestad");
            var line = d.channel.Select(p => new Vector2((float)p.x, (float)p.z)).ToList();
            var berth = d.Anchor("berth");
            var down = d.Anchor("down");
            Vector2 b = new Vector2((float)berth.pose.x, (float)berth.pose.z), e = new Vector2((float)down.pose.x, (float)down.pose.z);
            int i0 = Nearest(line, b), i1 = Nearest(line, e);
            var route = new List<Vector2> { b };
            for (int i = i0 + 1; i < i1; i++)
                route.Add(line[i]);
            route.Add(e);
            exitAt = e;
            return route;
        }

        static Vector2 FromDeg(float fromDeg, float speed)
        {
            float to = (fromDeg + 180f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(to), Mathf.Cos(to)) * speed;
        }

        [Test]
        public void BeatDensity_DorestadToExit()
        {
            var words = BeatWords.Parse(Text("Assets/Kapman/Game/Data/beats_793.json"));
            Lessons.Load(Text("Assets/Kapman/Game/Data/lessons_793.json"));
            Lessons.Forget();
            var marks = Landmarks.Parse(Text("Assets/Kapman/Data/Levels/landmarks_dorestad.json"));
            Assert.Greater(marks.Count, 3, "the builder wrote Dorestad's landmarks");
            var s = new BeatScheduler(words);
            s.raisers.Add(new CastOff(words));
            s.raisers.Add(new LandmarkSight(marks, words));
            s.raisers.Add(new WindBeats(words));
            s.raisers.Add(new ExitBeat(words));
            var route = Route(out Vector2 exitAt);
            const float speed = 2.0f;                                        // rowing 1.6 m/s with the stream's 0.4 under her
            const float prompt = 90f;                                        // the way out's prompt radius (chain.json)
            // a summer day's wind: south-west and light in the morning, veering west and freshening, north-west by evening
            Vector2 Wind(double m) => m < 300 ? FromDeg(225f, 4f) : m < 600 ? FromDeg(270f, 7f) : FromDeg(315f, 9f);
            int seg = 0;
            Vector2 pos = route[0];
            double minute = 0;
            for (; minute < 3000; minute++)
            {
                Vector2 dir = (route[seg + 1] - route[seg]).normalized;
                var c = new BeatContext
                {
                    minute = minute, realSeconds = minute, year = 793, position = pos,
                    heading = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg, speed = speed, wind = Wind(minute), visibility = 20000f,
                    exitNear = Vector2.Distance(pos, exitAt) < prompt, exitPrompt = "Sail down the Rhine to the sea",
                };
                s.Tick(c);
                if (s.Open != null && (s.Open.kind == "exit" || c.realSeconds - s.Open.raisedMinute >= 8))
                    s.Choose(0, c);                                          // the player answers after 8 seconds
                if (c.exitNear)
                    break;
                float step = speed;                                          // a real second at 1x: speed metres along the route
                while (step > 0f && seg < route.Count - 1)
                {
                    float left = Vector2.Distance(pos, route[seg + 1]);
                    if (step < left)
                    {
                        pos += dir * step;
                        step = 0f;
                    }
                    else
                    {
                        pos = route[seg + 1];
                        step -= left;
                        seg++;
                        if (seg < route.Count - 1)
                            dir = (route[seg + 1] - route[seg]).normalized;
                    }
                }
                if (seg >= route.Count - 1)
                    seg = route.Count - 2;
            }
            int beats = s.log.Offered;
            var gaps = s.log.Gaps(240.0, 0.0, minute);
            string report = string.Join("\n", s.log.entries.Where(e => e.what == "offered").Select(e => $"{e.realSeconds,5:0} s  {e.kind}: {e.text}"));
            Assert.That(beats, Is.InRange(4, 8), $"{beats} beats in {minute:0} s:\n{report}");
            Assert.IsEmpty(gaps, $"gaps over 4 real minutes: {string.Join(", ", gaps.Select(g => $"{g.from:0}-{g.to:0} s"))}\n{report}");
            Assert.IsTrue(s.log.entries.Any(e => e.kind == "exit"), "the way out is a beat in the log");
            Assert.IsTrue(Lessons.Given("make_for_there") && Lessons.Given("keep_to_your_side"), "the two lessons came on the way:\n" + report);
            StringAssert.StartsWith("game_minute,real_seconds", s.log.Csv());
            Debug.Log($"Kapman beats on the Dorestad route ({minute:0} s):\n{report}");
        }

        [Test]
        public void Speed_SailState()
        {
            var data = SimData.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ship_classes_793.json"),
                                    AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/traffic_793.json"));
            double polar = Travel.Speed(data.classes["gislinge"], 0.0, 90.0, 8.0, out _);    // north, the wind from the east
            float Steady(float set)
            {
                var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(-8f, 0f, 0f), SailSet = set, HasHeadingOrder = true, HeadingOrder = 0f };
                ShipKit.Float(sim);
                ShipKit.Run(sim, new FlatWater(), 150f);
                return sim.Forces.AheadSpeed;
            }
            float full = Steady(1f), reefed = Steady(0.55f);
            Assert.AreEqual(polar, full, polar * 0.2, $"the body ({full:0.00} m/s) and the world's ships' polar ({polar:0.00} m/s) agree on a beam reach");
            Assert.That(reefed, Is.InRange(0.5f * full, 0.95f * full), "reefed she goes slower, more than half as fast");
            var rowing = new ShipSim(ShipKit.Gislinge()) { OarEffort = 1f, HasHeadingOrder = true, HeadingOrder = 0f };
            ShipKit.Float(rowing);
            ShipKit.Run(rowing, new FlatWater(), 150f);
            float knots = rowing.Forces.AheadSpeed / 0.514444f;
            Assert.That(knots, Is.InRange(3.4f, 4.3f), $"all oars in a calm: {knots:0.00} kn (the museum's 3.5-4 kn; the world's ships 3.5)");
        }

        [Test]
        public void Lessons_FireOnce()
        {
            Lessons.Load(Text("Assets/Kapman/Game/Data/lessons_793.json"));
            Lessons.Forget();
            Assert.IsNotNull(Lessons.Offer("make_for_there"), "the first time");
            Assert.IsNull(Lessons.Offer("make_for_there"), "never twice");
            var saved = Lessons.Capture();
            Lessons.Forget();
            Assert.IsNotNull(Lessons.Offer("keep_to_your_side"));
            Lessons.Restore(saved);
            Assert.IsNull(Lessons.Offer("make_for_there"), "not again after a load");
            Assert.IsNull(Lessons.Offer("no_such_lesson"));
            Lessons.Forget();
        }
    }
}
