using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>What the ship tests share: the real ships.json, a ship put on the water at her floating height, and a
    /// run of the very steps the scene's ShipBody takes.</summary>
    static class ShipKit
    {
        public static ShipClassData Data() => ShipClassData.Load(ShipClassData.ProjectPath);
        public static ShipClass Gislinge() => Data().Get("gislinge");

        /// <summary>Puts her upright on still water at level 0 with her keel at the draught her mass floats at, plus a
        /// lift (+ up), heading north, moving ahead at a speed.</summary>
        public static void Float(ShipSim sim, float lift = 0f, float ahead = 0f, float heel = 0f)
        {
            float d = Hydrostatics.DraughtFor(sim.Hull, sim.Mass.mass);
            Quaternion turn = Rot.AxisAngle(Vector3.forward, heel * Mathf.Deg2Rad);
            sim.PlaceAt(new Vector3(0f, -d - sim.Hull.keelY + lift, 0f), turn, new Vector3(0f, 0f, ahead));
        }

        /// <summary>The water above her keel amidships (m), measured from her origin's height while she lies level.</summary>
        public static float Draught(ShipSim sim, float level = 0f) => level - (sim.Origin.y + sim.Hull.keelY);

        public static void Run(ShipSim sim, IWaterField water, float seconds)
        {
            int n = Mathf.RoundToInt(seconds / ShipSim.Dt);
            for (int i = 0; i < n; i++)
                sim.Step(water);
        }

        /// <summary>A plain box hull of these sizes and mass (the hydrostatics' check).</summary>
        public static ShipClass Box(float length, float beam, float depth, float mass)
        {
            var g = Gislinge();
            return new ShipClass
            {
                id = "box", name = "a box", length = length, beam = beam, lwl = length, depth = depth,
                draught = mass / (Hydrostatics.Rho * length * beam), cb = 1f, sectionQ = 1f,
                hullMassKg = mass, hullCgM = 0.5f * depth, manKg = 80f, crewCgM = depth, gearCgM = 0f, cargoCgM = 0f,
                kRoll = 0.4f * beam, kPitch = 0.25f * length, kYaw = 0.25f * length,
                addSurge = g.addSurge, addSway = g.addSway, addHeave = g.addHeave, addRoll = g.addRoll, addPitch = g.addPitch, addYaw = g.addYaw,
                heaveDampingRatio = g.heaveDampingRatio, heaveDragCd = g.heaveDragCd, lateralArea = length * 0.1f, lateralCd = 1f,
                crFn = g.crFn, cr = g.cr, gm = 1f, hsLimit = 1f,
            };
        }
    }

    /// <summary>Ship physics tests 1-3 (the first night): she floats at her draught, heaves and rolls at her natural
    /// periods and settles, on the probe cells and the integrator the scene runs.</summary>
    public class ShipFloatTests
    {
        [Test]
        public void ParametricCellsHoldTheHull()
        {
            var g = ShipKit.Gislinge();
            var hull = HullProbes.ForClass(g, 0f);
            Assert.AreEqual(15, hull.cells.Length, "5 stations by 3 under 12 m");
            Assert.AreEqual(21, HullProbes.ForClass(ShipKit.Data().Get("dorestat1"), 0f).cells.Length, "7 by 3 over 12 m");
            float v = Hydrostatics.Volume(hull, g.draught);
            Assert.AreEqual(g.cb * g.lwl * g.beam * g.draught, v, 0.002f * v, "the cells hold Cb L B T");
            float awp = Hydrostatics.WaterplaneArea(hull, g.draught);
            Assert.AreEqual(g.cb * g.sectionQ * g.lwl * g.beam, awp, 0.002f * awp, "the waterplane is Cb q L B");
            Assert.AreEqual(-g.draught, hull.keelY, 1e-5f, "the keel sits a draught below the origin at the waterline");
            foreach (var c in hull.cells)
                Assert.AreEqual(c.Area(c.draught - 1e-4f), c.area, 0.01f * c.area, "each cell's footprint is its volume's growth at the waterline");
        }

        [Test]
        public void ABoxFloatsAtItsDraught()
        {
            // a box 7.7 x 1.5 x 0.6 m at 1,800 kg in sea water: 1800 / (1025 x 7.7 x 1.5) = 0.152 m
            var box = ShipKit.Box(7.7f, 1.5f, 0.6f, 1800f);
            var sim = new ShipSim(box);
            Assert.AreEqual(1800f, sim.Mass.mass, 1e-3f);
            Assert.AreEqual(0.15204f, Hydrostatics.DraughtFor(sim.Hull, 1800f), 0.001f, "the hydrostatics");
            ShipKit.Float(sim, 0.1f);                                      // dropped from 10 cm too high
            ShipKit.Run(sim, new FlatWater(), 40f);
            Assert.AreEqual(0.15204f, ShipKit.Draught(sim), 0.001f, "the body settles at it");
        }

        [Test]
        public void TheGislingeFloatsAtHerDraught()
        {
            var g = ShipKit.Gislinge();
            var sim = new ShipSim(g);
            sim.SetMass(Hydrostatics.Of(1800f, sim.Mass.cg.y - sim.Hull.keelY, sim.Hull.keelY, g.kRoll, g.kPitch, g.kYaw));
            sim.PlaceAt(new Vector3(0f, 0.15f, 0f), Quaternion.identity, Vector3.zero);   // let go 15 cm high
            ShipKit.Run(sim, new FlatWater(), 40f);
            Assert.AreEqual(g.draught, ShipKit.Draught(sim), 0.02f, "1,800 kg floats within 2 cm of her class draught");
            Assert.Less(Mathf.Abs(Rot.Heel(sim.State.rotation)), 0.05f, "upright");
            Assert.Less(Mathf.Abs(Rot.Trim(sim.State.rotation)), 0.05f, "level");
        }

        [Test]
        public void SheHeavesAtHerPeriodAndSettles()
        {
            var g = ShipKit.Gislinge();
            var sim = new ShipSim(g);
            float rest = Hydrostatics.DraughtFor(sim.Hull, sim.Mass.mass);
            float period = Hydrostatics.HeavePeriod(sim.Hull, sim.Mass.mass, g.addHeave);
            ShipKit.Float(sim, 0.2f);                                      // dropped 0.2 m
            var water = new FlatWater();
            float y0 = sim.Origin.y - 0.2f;                                // her resting height
            // the times she passes down through her resting height: a full cycle between each two
            float last = sim.Origin.y - y0, t = 0f;
            var downs = new System.Collections.Generic.List<float>();
            float settledAt = 3f * period, after = 0f;
            while (t < 3f * period + 1f)
            {
                sim.Step(water);
                t += ShipSim.Dt;
                float now = sim.Origin.y - y0;
                if (last > 0f && now <= 0f)
                    downs.Add(t - ShipSim.Dt * now / (now - last));
                last = now;
                if (t >= settledAt && after == 0f)
                    after = Mathf.Abs(now);
            }
            Assert.GreaterOrEqual(downs.Count, 2, "she bobs");
            float measured = downs[1] - downs[0];
            Assert.AreEqual(period, measured, 0.15f * period, $"the heave period (rest draught {rest:0.000} m)");
            float peak = 0f;
            for (int i = 0; i < 25; i++)
            {
                sim.Step(water);
                peak = Mathf.Max(peak, Mathf.Abs(sim.Origin.y - y0));
            }
            Assert.Less(Mathf.Max(after, peak), 0.02f, "after three cycles under a tenth of the 0.2 m drop");
        }

        [Test]
        public void SheRollsAtHerPeriodAndRightsHerself()
        {
            var g = ShipKit.Gislinge();
            var sim = new ShipSim(g);
            float kg = sim.Mass.cg.y - sim.Hull.keelY;
            float gm = Hydrostatics.GM(sim.Hull, sim.Mass.mass, kg);
            Assert.AreEqual(g.gm, gm, 0.1f * g.gm, "GM from the cells within 10% of the class's");
            float period = Hydrostatics.RollPeriod(g.kRoll, gm, g.addRoll);

            ShipKit.Float(sim, 0f, 0f, 10f);                               // heeled 10 degrees and let go
            var water = new FlatWater();
            var ups = new System.Collections.Generic.List<float>();
            float last = Rot.Heel(sim.State.rotation), t = 0f, firstSwing = 0f, laterSwing = 0f;
            while (t < 4f * period)
            {
                sim.Step(water);
                t += ShipSim.Dt;
                float heel = Rot.Heel(sim.State.rotation);
                if (last < 0f && heel >= 0f)
                    ups.Add(t - ShipSim.Dt * heel / (heel - last));
                last = heel;
                if (t < period)
                    firstSwing = Mathf.Max(firstSwing, Mathf.Abs(heel));
                else if (t > 2f * period)
                    laterSwing = Mathf.Max(laterSwing, Mathf.Abs(heel));
            }
            Assert.GreaterOrEqual(ups.Count, 2, "she rolls back and forth");
            float measured = ups[1] - ups[0];
            Assert.AreEqual(period, measured, 0.15f * period, "the roll period: 2 pi k sqrt(1 + added) / sqrt(g GM), so the inertia is in use");
            Assert.Less(laterSwing, 0.6f * firstSwing, "the roll dies away");
        }

        [Test]
        public void SheRidesASwellAndComesBackToRest()
        {
            var g = ShipKit.Gislinge();
            var sim = new ShipSim(g);
            ShipKit.Float(sim);
            var swell = new SineWater(0.3f, 40f, 90f);                     // a 0.6 m swell 40 m long, running east, on her beam
            float rose = float.MinValue, fell = float.MaxValue, heelMost = 0f;
            for (int i = 0; i < 1500; i++)
            {
                swell.Advance(ShipSim.Dt);
                sim.Step(swell);
                if (i > 500)
                {
                    rose = Mathf.Max(rose, sim.Origin.y);
                    fell = Mathf.Min(fell, sim.Origin.y);
                    heelMost = Mathf.Max(heelMost, Mathf.Abs(Rot.Heel(sim.State.rotation)));
                }
            }
            Assert.AreEqual(0.6f, rose - fell, 0.2f, "she rises and falls with a long swell");
            Assert.Greater(heelMost, 1f, "the swell's slope rolls her");
            Assert.Less(heelMost, 15f, "but a long low swell does not lay her over");
            Assert.Less(new Vector2(sim.State.position.x, sim.State.position.z).magnitude, 5f, "and does not carry her off");
        }
    }
}
