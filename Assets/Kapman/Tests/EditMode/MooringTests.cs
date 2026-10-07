using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Ship physics night three, the anchor and the berth (2.9, test 10): the holding table; the Gislinge's 15 kg
    /// iron anchor at 3:1 in sand holds in 10 m/s with a 0.5 m sea and drags in 20 m/s with a 1.5 m sea; the new rope
    /// holds in both and an old one parts; weighing brings her up to it; no bottom, no anchor; and at a berth she lies
    /// still in wind and stream and rises with the water.</summary>
    public class MooringTests
    {
        /// <summary>A Gislinge anchored in 3 m over a bottom, a wind from the north and a sea running south (height crest to
        /// trough, m; 0 for still water) for a while; the water she rode in.</summary>
        static ShipSim Riding(float wind, float sea, GroundKind bottom, float seconds, out IWaterField water, float integrity = 1f)
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim);
            sim.Bed = new FlatBed(-3f, bottom);
            SineWater waves = sea > 0f ? new SineWater(0.5f * sea, 39f, 180f) : null;   // a 5 s wind sea, 39 m long
            water = waves != null ? (IWaterField)waves : new FlatWater();
            sim.Anchor.integrity = integrity;
            Assert.IsTrue(sim.Anchor.LetGo(sim, water, sim.Bed), "she has a bottom for her rope");
            sim.TrueWind = new Vector3(0f, 0f, -wind);
            int n = Mathf.RoundToInt(seconds / ShipSim.Dt);
            for (int i = 0; i < n; i++)
            {
                waves?.Advance(ShipSim.Dt);
                sim.Step(water);
            }
            return sim;
        }

        [Test]
        public void TheHoldingIsTheDesigns()
        {
            Assert.AreEqual(1471.5f, Grounds.Holding(GroundKind.Sand, false, 15f), 0.5f, "15 kg of iron in sand: 10 times its weight");
            Assert.AreEqual(735.75f, Grounds.Holding(GroundKind.Sand, true, 25f), 0.5f, "a 25 kg stone in sand: 3 times");
            Assert.Less(Grounds.Holding(GroundKind.Rock, false, 15f), Grounds.Holding(GroundKind.Mud, false, 15f), "rock holds worst");
            var sim = Riding(5f, 0f, GroundKind.Sand, 120f, out _);
            Assert.AreEqual(3f * 3f + 1f, sim.Anchor.Rode, 0.01f, "3:1 and a metre over, paid out as she dropped back");
            Assert.AreEqual(1471.5f, sim.Anchor.Holding, 1f, "at 3:1 the full hold");
        }

        [Test]
        public void ItHoldsInABreezeAndDragsInAGale()
        {
            var breeze = Riding(10f, 0.5f, GroundKind.Sand, 300f, out _);
            Assert.Less(breeze.Anchor.Dragged, 1f, $"10 m/s, a 0.5 m sea: it holds ({breeze.Anchor.Dragged:0.00} m)");
            Assert.IsTrue(breeze.Anchor.Down);
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, Rot.Heading(breeze.State.rotation)), 30f, "she lies head to wind");

            var gale = Riding(20f, 1.5f, GroundKind.Sand, 300f, out _);
            Assert.Greater(gale.Anchor.Dragged, 3f, $"20 m/s, a 1.5 m sea: the bow's snatches drag it ({gale.Anchor.Dragged:0.0} m)");
            Assert.IsFalse(gale.Anchor.Parted, "a new rope holds");
            Assert.Less(gale.Origin.z, -15f, "and she goes to leeward with it");
        }

        [Test]
        public void AnOldRopePartsInAGale()
        {
            var sim = Riding(20f, 1.5f, GroundKind.Sand, 300f, out _, 0.1f);
            Assert.IsTrue(sim.Anchor.Parted, "worn to a tenth of its strength, the rope parts under the snatches");
            Assert.IsFalse(sim.Anchor.Down);
            Assert.IsFalse(sim.Anchor.LetGo(sim, new FlatWater(), sim.Bed), "and there is no anchor to let go");
            sim.Anchor.Restore();
            Assert.IsTrue(sim.Anchor.LetGo(sim, new FlatWater(), sim.Bed), "until a new one is bought");
        }

        [Test]
        public void WeighingBringsHerUpToItAndBreaksItOut()
        {
            var sim = Riding(6f, 0f, GroundKind.Mud, 150f, out var water);
            Vector3 at = sim.Anchor.Point;
            sim.Anchor.Weigh();
            float t = 0f;
            while (sim.Anchor.Down && t < 180f)
            {
                sim.Step(water);
                t += ShipSim.Dt;
            }
            Assert.IsFalse(sim.Anchor.Down, $"aweigh ({t:0} s)");
            Vector3 bow = sim.State.position + sim.State.rotation * (Anchor.BowLocal(sim) - sim.Mass.cg);
            Assert.Less(new Vector2(bow.x - at.x, bow.z - at.z).magnitude, 3f, "hauled up over it");
        }

        [Test]
        public void WithNoBottomInReachSheCannotAnchor()
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim);
            Assert.IsFalse(sim.Anchor.LetGo(sim, new FlatWater(), new FlatBed(-40f)), "40 m of water, 30 m of rope");
            Assert.IsFalse(sim.Anchor.LetGo(sim, new FlatWater(), null), "and none at all with no bottom");
        }

        [Test]
        public void AtABerthSheLiesStillAndRisesWithTheWater()
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim);
            var water = new FlatWater { current = new Vector3(0f, 0f, -0.4f) };     // a river running south
            sim.TrueWind = new Vector3(12f, 0f, 0f);                                 // and a strong breeze on her beam
            sim.Berth.MakeFastHere(sim);
            Vector3 at = sim.Origin;
            float most = 0f;
            for (int i = 0; i < 15000; i++)
            {
                water.level = Mathf.Min(1f, i / 7500f);                              // a metre of flood in 150 s
                sim.Step(water);
                most = Mathf.Max(most, new Vector2(sim.Origin.x - at.x, sim.Origin.z - at.z).magnitude);
            }
            Assert.Less(most, 1.5f, "the lines hold her at her berth");
            Assert.Greater(sim.Berth.Tension, 10f, "they are working");
            Assert.AreEqual(at.y + 1f, sim.Origin.y, 0.1f, "and she rises with the water: the lines' eyes ride up the posts");
            Assert.IsFalse(sim.Berth.Parted);
        }
    }
}
