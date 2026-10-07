using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Ship physics tests 14 and 15: the body's clock keeps the game's (more steps at 2x and 4x, never the
    /// engine's timeScale, never more than eight a frame), and the water's resistance slows her as it should; and the
    /// plain helm and the oars of the first night move her.</summary>
    public class ShipMotionTests
    {
        /// <summary>Real seconds of 60 fps frames through the body's clock, stepping the sim; the game seconds it stepped.</summary>
        static double Frames(ShipSim sim, StepClock clock, IWaterField water, float seconds, float speed)
        {
            int frames = Mathf.RoundToInt(seconds * 60f);
            for (int f = 0; f < frames; f++)
            {
                int n = clock.Take(1f / 60f, speed);
                Assert.LessOrEqual(n, StepClock.MaxSteps);
                for (int i = 0; i < n; i++)
                    sim.Step(water);
            }
            return clock.Stepped;
        }

        static ShipSim Rowing()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { OarEffort = 1f };
            ShipKit.Float(sim);
            ShipKit.Run(sim, new FlatWater(), 120f);                       // up to her steady speed under oars
            return sim;
        }

        [Test]
        public void TheClockKeepsTheGamesPace()
        {
            var water = new FlatWater();
            var a = Rowing();
            var b = Rowing();
            Vector3 fromA = a.Origin, fromB = b.Origin;
            double at1 = Frames(a, new StepClock(), water, 10f, 1f);
            double at4 = Frames(b, new StepClock(), water, 10f, 4f);
            Assert.AreEqual(10.0, at1, ShipSim.Dt, "1x: ten game seconds in ten real ones (the clock: 1 s = 1 min)");
            Assert.AreEqual(40.0, at4, ShipSim.Dt, "4x: forty game seconds, as LevelClock adds deltaTime x Speed");
            float d1 = (a.Origin - fromA).magnitude, d4 = (b.Origin - fromB).magnitude;
            Assert.AreEqual(4f, d4 / d1, 0.04f, "at 4x she covers four times the water in a real second");
            Assert.AreEqual(a.Forces.AheadSpeed * 10f, d1, 0.01f * d1, "at 1x she covers her real speed in game metres a real second");
        }

        [Test]
        public void AHitchDoesNotSpiral()
        {
            var clock = new StepClock();
            Assert.AreEqual(StepClock.MaxSteps, clock.Take(1f, 4f), "a one-second hitch at 4x: eight steps, not two hundred");
            Assert.LessOrEqual(clock.Take(1f / 60f, 4f), 4, "and the next frame is an ordinary one");
            Assert.AreEqual(0, new StepClock().Take(0.5f, 0f), "nothing while the clock stands");
        }

        [Test]
        public void SheCoastsToAStopAsTheWaterHoldsHer()
        {
            // test 15: coasting from 2 m/s with no oars, under 1 m/s in 40-80 s (the design's sum: about 55 s)
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim, 0f, 2f);
            var water = new FlatWater();
            float t = 0f;
            while (sim.State.velocity.z >= 1f && t < 200f)
            {
                sim.Step(water);
                t += ShipSim.Dt;
            }
            Assert.That(t, Is.InRange(40f, 80f), $"2 to 1 m/s took {t:0.0} s");
            Assert.AreEqual(1f, sim.Forces.AheadSpeed, 0.02f, "her speed through the still water is her speed");
            Assert.Less(Mathf.Abs(sim.State.velocity.x), 0.01f, "she keeps her line");
        }

        [Test]
        public void SheRowsAndTurnsToTheHeadingOrdered()
        {
            var sim = Rowing();
            float knots = sim.Forces.AheadSpeed * 3600f / 1852f;
            Assert.That(knots, Is.InRange(3.5f, 5f), "six men at a steady stroke: the museum's 3.5-4 kn and the design's 4.2, near enough tonight");
            sim.HasHeadingOrder = true;
            sim.HeadingOrder = 90f;
            ShipKit.Run(sim, new FlatWater(), 40f);
            Assert.AreEqual(90f, Rot.Heading(sim.State.rotation), 3f, "she comes round to the heading ordered");
            Vector3 v = sim.State.velocity;
            Assert.AreEqual(90f, Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 5f, "and goes that way (the side resistance takes her drift off)");
        }

        [Test]
        public void TheStreamCarriesHerThroughTheWater()
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            var water = new FlatWater { current = new Vector3(0f, 0f, -0.4f) };   // a river running south at 0.4 m/s
            ShipKit.Float(sim);
            sim.State.velocity = water.current;                            // she lies in the stream, moving with it
            ShipKit.Run(sim, water, 30f);
            Assert.AreEqual(-0.4f, sim.State.velocity.z, 0.01f, "drifting, she goes with the stream");
            sim.OarEffort = 1f;
            ShipKit.Run(sim, water, 150f);
            var still = Rowing();
            Assert.AreEqual(still.Forces.AheadSpeed - 0.4f, sim.State.velocity.z, 0.05f, "rowing up it, she makes her speed less the stream");
        }
    }
}
