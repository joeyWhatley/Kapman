using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Ship physics night three, the bottom (2.9, test 11): she touches and stops on soft ground and cracks a
    /// plank on gravel at speed, lies over on her bilge as the water leaves her, floats off when it comes back, is pushed
    /// off by the crew, slows over shoal water, and is stopped by a quay's edge without being lifted onto it.</summary>
    public class GroundingTests
    {
        /// <summary>A quay's edge: deep water short of a line across her course, the quay's top above the water beyond it.</summary>
        sealed class QuayBed : IBedField
        {
            public float edgeZ, deep, top;

            public bool Bed(Vector3 p, out float height, out Vector3 normal, out GroundKind kind)
            {
                height = p.z < edgeZ ? deep : top;
                normal = Vector3.up;
                kind = GroundKind.Rock;
                return true;
            }
        }

        /// <summary>A Gislinge afloat on still water at level 0, heading north at a speed, and the height of her keel.</summary>
        static ShipSim Afloat(float ahead, out float keel)
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim, 0f, ahead);
            keel = sim.Origin.y + sim.Hull.keelY;
            return sim;
        }

        [Test]
        public void OnASoftSlopeSheTouchesAndStopsUnhurt()
        {
            var sim = Afloat(0.5f, out float keel);
            sim.Bed = new SlopingBed(keel - 0.3f, 0.02f, 0f, GroundKind.Sand);     // a 1:50 sand slope rising ahead
            ShipKit.Run(sim, new FlatWater(), 120f);
            Assert.AreEqual(1, sim.Grounding.Touches, "she touched");
            Assert.IsTrue(sim.Grounding.Aground, "and lies on it");
            Assert.Less(sim.State.velocity.magnitude, 0.02f, "stopped");
            Assert.AreEqual(0, sim.Grounding.PlanksCracked + sim.Grounding.PlanksStove, "soft ground at half a metre a second does no harm");
            Assert.Greater(sim.Grounding.LeastClearance, -0.06f, "she rests on the sand, not in it");
        }

        [Test]
        public void FastOntoGravelSheCracksAPlank()
        {
            var sim = Afloat(2f, out float keel);
            sim.Bed = new SlopingBed(keel - 0.3f, 0.02f, 0f, GroundKind.Gravel);
            ShipKit.Run(sim, new FlatWater(), 60f);
            Assert.AreEqual(1, sim.Grounding.PlanksCracked, "one blow, one plank");
            Assert.AreEqual(0, sim.Grounding.PlanksStove);
            Assert.AreEqual(Grounding.CrackLeak, sim.Bilge.damageLeak, 1e-5f, "and it leaks into the bilge");

            var sand = Afloat(2f, out keel);
            sand.Bed = new SlopingBed(keel - 0.3f, 0.02f, 0f, GroundKind.Sand);
            ShipKit.Run(sand, new FlatWater(), 60f);
            Assert.AreEqual(0, sand.Grounding.PlanksCracked, "on sand she only stops");
            Assert.IsTrue(sand.Grounding.Aground);
        }

        [Test]
        public void TheWaterLeavesHerAndComesBack()
        {
            var sim = Afloat(0f, out float keel);
            var water = new FlatWater();
            sim.Bed = new FlatBed(keel - 0.05f, GroundKind.Mud);
            for (int i = 0; i < 1500; i++)                                  // the water falls 0.3 m in 30 s
            {
                water.level = -0.3f * Mathf.Min(1f, i / 1500f);
                sim.Step(water);
            }
            ShipKit.Run(sim, water, 10f);
            Assert.IsTrue(sim.Grounding.Aground, "the water fell away: she sits on the mud");
            Assert.Greater(sim.Grounding.Load, 0.2f * sim.Mass.mass * Hydrostatics.G, "carrying much of her weight");
            for (int i = 0; i < 3000; i++)                                  // and rises 0.6 m in 60 s
            {
                water.level = -0.3f + 0.6f * Mathf.Min(1f, i / 3000f);
                sim.Step(water);
            }
            ShipKit.Run(sim, water, 10f);
            Assert.IsFalse(sim.Grounding.Touching, "the water came back: she floats");
            Assert.IsFalse(sim.Grounding.Aground);
            Assert.AreEqual(ShipFate.Afloat, sim.Fate);
        }

        [Test]
        public void DriedOutSheLiesOverOnHerBilge()
        {
            var sim = Afloat(0f, out float keel);
            var water = new FlatWater();
            sim.Bed = new SlopingBed(keel - 0.05f, 0.02f, 90f, GroundKind.Mud); // the mud falls away a little to the west
            for (int i = 0; i < 6000; i++)                                  // the tide goes out 0.8 m in two minutes
            {
                water.level = -0.8f * Mathf.Min(1f, i / 6000f);
                sim.Step(water);
            }
            ShipKit.Run(sim, water, 30f);
            // a keeled boat cannot stand on her keel alone: she falls over onto one bilge (which one, small things decide)
            float heel = Mathf.Abs(Rot.Heel(sim.State.rotation));
            Assert.That(heel, Is.InRange(8f, 35f), $"she has fallen over onto a bilge: heel {heel:0.0} degrees");
            Assert.IsTrue(sim.Grounding.Aground);
            Assert.AreEqual(sim.Mass.mass * Hydrostatics.G, sim.Grounding.Load, 0.1f * sim.Mass.mass * Hydrostatics.G, "the mud carries all of her");
            Assert.Greater(sim.Grounding.LeastClearance, -0.08f, "and she does not sink into it");
            Assert.AreEqual(ShipFate.Afloat, sim.Fate, "a boat lying over on the mud is not lost");
        }

        [Test]
        public void TheCrewPushHerOff()
        {
            var sim = Afloat(1f, out float keel);
            sim.Bed = new SlopingBed(keel - 0.2f, 0.04f, 0f, GroundKind.Sand);     // a 1:25 bank ahead
            ShipKit.Run(sim, new FlatWater(), 60f);
            Assert.IsTrue(sim.Grounding.Aground, "she has run up onto the bank");
            float z = sim.Origin.z;
            sim.Grounding.PushOff();
            Assert.IsTrue(sim.Grounding.PushingOff);
            ShipKit.Run(sim, new FlatWater(), 60f);
            Assert.IsFalse(sim.Grounding.Touching, "over the side and push: she floats");
            Assert.Less(sim.Origin.z, z - 0.5f, "pushed back towards deeper water");
            Assert.IsFalse(sim.Grounding.PushingOff, "and the men are back aboard");
        }

        [Test]
        public void ShoalWaterHoldsHerBackAndLiftsTheOar()
        {
            ShipSim Over(float underKeel)
            {
                var sim = new ShipSim(ShipKit.Gislinge());
                ShipKit.Float(sim, 0f, 2.6f);                               // a good 5 knots
                float keel = sim.Origin.y + sim.Hull.keelY;
                sim.Bed = new FlatBed(keel - underKeel, GroundKind.Sand);
                ShipKit.Run(sim, new FlatWater(), 1f);
                Assert.IsFalse(sim.Grounding.Touching, "she does not touch");
                return sim;
            }
            var deep = Over(10f);
            var shoal = Over(0.1f);
            Assert.AreEqual(1f, deep.Grounding.Shallow, 1e-6f, "deep water: no shallow-water factor");
            Assert.Greater(shoal.Grounding.Shallow, 1.1f, "under two draughts of water the factor rises (design 2.4)");
            Assert.Greater(shoal.Forces.Resistance, 1.03f * deep.Forces.Resistance, "the bank holds her back before she touches it");
            Assert.IsTrue(shoal.Helmsman.Lifted, "a hand's breadth under her keel the steering oar is up");
            Assert.IsFalse(deep.Helmsman.Lifted);
        }

        [Test]
        public void AQuaysEdgeStopsHerWithoutLiftingHer()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { OarEffort = 1f, HasHeadingOrder = true, HeadingOrder = 0f };
            ShipKit.Float(sim);
            sim.Bed = new QuayBed { edgeZ = 30f, deep = -3f, top = 0.8f };
            float highest = sim.Origin.y;
            bool against = false;
            for (int i = 0; i < 6000; i++)
            {
                sim.Step(new FlatWater());
                highest = Mathf.Max(highest, sim.Origin.y);
                against |= sim.Grounding.AgainstStep;
            }
            Assert.IsTrue(against, "she came up against the quay");
            Assert.Less(sim.Origin.z + 0.5f * sim.Hull.lwl, 31.5f, "and was stopped at it");
            Assert.Less(highest, 0.15f, "not lifted onto it");
        }
    }
}
