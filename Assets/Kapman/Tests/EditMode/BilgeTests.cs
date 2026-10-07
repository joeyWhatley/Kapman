using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Ship physics night three, the bilge (2.8, test 9 and the bilge part of test 3): green water over the
    /// gunwale by the weir, the crew bailing by the standing order, the water's weight and free surface, rain and the laps,
    /// and her fate: swamped (awash, not sunk) or capsized, which the body only reports.</summary>
    public class BilgeTests
    {
        static ShipSim Afloat()
        {
            var sim = new ShipSim(ShipKit.Gislinge());
            ShipKit.Float(sim);
            return sim;
        }

        static float Heel(ShipSim sim) => Rot.Heel(sim.State.rotation);

        [Test]
        public void TheWeirAndTheBailersAreTheDesigns()
        {
            float litres = Bilge.Overflow(2f, 0.1f) * 1f * 1000f;
            Assert.That(litres, Is.InRange(25f, 32f), "2 m of gunwale 0.1 m under for a second lets in 25-32 l");
            float bailed = Bilge.BailRate(4, 0.2f, false) * 60000f;
            Assert.That(bailed, Is.InRange(90f, 110f), "four men bailing hold about 100 l a minute");
            Assert.Less(Bilge.BailRate(4, 0.2f, true), 0.1f * Bilge.Overflow(2f, 0.1f), "and cannot bail a sea that pours in: it must be stopped");
            Assert.Less(Bilge.BailRate(1, 0.02f, false), Bilge.BailRate(1, 0.2f, false), "reaching for the last of it is slower");
            Assert.AreEqual(0f, Bilge.Overflow(2f, -0.1f), "a gunwale above the water lets nothing in");
        }

        [Test]
        public void PressedPastHerGunwaleSheShipsGreenWater()
        {
            // a press (a squall on the sail) that grows until the lee gunwale goes under, eased as soon as she has
            // shipped 150 litres (the crew luff her)
            var sim = Afloat();
            var water = new FlatWater();
            float press = 0f, heelThen = 0f, mostIn = 0f;
            bool under = false;
            for (int i = 0; i < 3000 && sim.Bilge.Water < 0.15f; i++)
            {
                press += 200f * ShipSim.Dt;
                sim.ExtraTorque = new Vector3(0f, 0f, press);              // starboard side up, the port gunwale down
                sim.Step(water);
                under |= sim.Bilge.GunwaleUnder;
                heelThen = Heel(sim);
                mostIn = Mathf.Max(mostIn, sim.Bilge.OverGunwale);
            }
            Assert.IsTrue(under, "the lee gunwale went under");
            Assert.Greater(heelThen, sim.Class.DownfloodingDeg - 3f, $"near or past the downflooding angle ({heelThen:0.0} degrees)");
            Assert.Greater(mostIn, 0.01f, "the sea poured over it, litres a second");
            Assert.GreaterOrEqual(sim.Bilge.Water, 0.15f, "and she shipped it");
            sim.ExtraTorque = Vector3.zero;
            ShipKit.Run(sim, water, 20f);
            Assert.Less(Mathf.Abs(Heel(sim)), 10f, "eased, she comes up");
            Assert.AreEqual(ShipFate.Afloat, sim.Fate);
            Assert.Greater(sim.Bilge.Bailers, 0, "with water in her, the crew bail");
            Assert.Greater(sim.Mass.mass, sim.DryMass.mass + 100f, "the water's weight is in her");
            Assert.Greater(sim.Bilge.GMLoss, 0f, "and its free surface takes some of her stiffness");
        }

        [Test]
        public void LooseWaterMakesHerTender()
        {
            float HeelUnder(float waterM3)
            {
                var sim = Afloat();
                sim.Bilge.Add(waterM3);
                sim.Bilge.order = BailOrder.Stop;                          // the water stays as it is
                sim.ExtraTorque = new Vector3(0f, 0f, 800f);
                ShipKit.Run(sim, new FlatWater(), 30f);
                Assert.AreEqual(waterM3, sim.Bilge.Water, 0.002f, "none came over the gunwale");
                return Heel(sim);
            }
            float dry = HeelUnder(0f), wet = HeelUnder(0.5f);
            Assert.That(dry, Is.InRange(3f, 8f), "dry, a steady press heels her a little");
            Assert.Greater(wet, 1.15f * dry, $"with 0.5 m3 loose in her she heels further ({wet:0.0} against {dry:0.0} degrees)");

            // the design's sum (2.8): in a box of the Gislinge's size, 0.25 m3 of loose water takes all of its GM; the
            // round-bilged hull's pool is narrower (its breadth at the depth of the water), so she keeps some of hers
            var box = new ShipSim(ShipKit.Box(7.7f, 1.5f, 0.6f, 1800f));
            ShipKit.Float(box);
            box.Bilge.Add(0.25f);
            ShipKit.Run(box, new FlatWater(), 0.1f);
            float boxGm = Hydrostatics.GM(box.Hull, box.Mass.mass, box.Mass.cg.y - box.Hull.keelY);
            Assert.Less(boxGm - box.Bilge.GMLoss, 0f, "the box: GM_eff below zero");
            var g = Afloat();
            g.Bilge.Add(0.25f);
            ShipKit.Run(g, new FlatWater(), 0.1f);
            float gm = Hydrostatics.GM(g.Hull, g.Mass.mass, g.Mass.cg.y - g.Hull.keelY);
            Assert.Greater(gm - g.Bilge.GMLoss, 0.2f, "the Gislinge: still stiff");
        }

        [Test]
        public void TheCrewBailByTheStandingOrder()
        {
            var sim = Afloat();
            sim.Bilge.Add(0.12f);                                          // about 6 cm over the keel
            ShipKit.Run(sim, new FlatWater(), 1f);
            Assert.Greater(sim.Bilge.Depth, Bilge.OneManFrom);
            Assert.AreEqual(1, sim.Bilge.Bailers, "over 5 cm one man bails");
            float before = sim.Bilge.Water;
            ShipKit.Run(sim, new FlatWater(), 60f);
            Assert.AreEqual(0.025f, before - sim.Bilge.Water, 0.004f, "25 litres a minute");
            sim.OarEffort = 1f;
            ShipKit.Run(sim, new FlatWater(), 0.1f);
            Assert.AreEqual(0.75f, sim.RowingShare, 1e-4f, "and he is off his oar");

            var all = Afloat();
            all.Bilge.Add(0.3f);
            all.Bilge.order = BailOrder.AllHands;
            ShipKit.Run(all, new FlatWater(), 1f);
            float start = all.Bilge.Water;
            ShipKit.Run(all, new FlatWater(), 30f);
            Assert.AreEqual(4, all.Bilge.Bailers, "all hands");
            Assert.AreEqual(0.08f, start - all.Bilge.Water, 0.01f, "four men in a burst: 160 l a minute");
            Assert.AreEqual(0f, all.RowingShare, "nobody rowing");

            var none = Afloat();
            none.Bilge.Add(0.3f);
            none.Bilge.order = BailOrder.Stop;
            ShipKit.Run(none, new FlatWater(), 30f);
            Assert.AreEqual(0.3f, none.Bilge.Water, 0.002f, "nobody bails when told not to");
        }

        [Test]
        public void RainAndTheLapsFillHerSlowly()
        {
            var sim = Afloat();
            sim.Rain = 1f;
            ShipKit.Run(sim, new FlatWater(), 1800f);
            float open = 0f;
            foreach (var c in sim.Hull.cells)
                open += c.area;
            float expect = 0.5f * (open * 0.01f + sim.Class.leakM3h);    // half an hour of 10 mm/h and a sound hull's leak
            Assert.AreEqual(expect, sim.Bilge.Water, 0.05f * expect, "rain on her open hull and the laps");
            Assert.AreEqual(0, sim.Bilge.Bailers, "under 5 cm nobody bails yet");
        }

        [Test]
        public void SwampedSheFloatsAwashAndIsLost()
        {
            var sim = Afloat();
            sim.Bilge.Add(10f);                                            // as much as she can take
            sim.Step(new FlatWater());
            Assert.AreEqual(sim.Bilge.Full, sim.Bilge.Water, 1e-3f);
            Assert.Less(sim.Bilge.Full, sim.Bilge.Capacity, "laden, she is awash before she is full");
            ShipKit.Run(sim, new FlatWater(), 10f);
            Assert.AreEqual(ShipFate.Swamped, sim.Fate, "awash for 3 s with her hull full: swamped");
            Assert.IsTrue(sim.Lost);
            float y = sim.Origin.y;
            ShipKit.Run(sim, new FlatWater(), 30f);
            Assert.AreEqual(y, sim.Origin.y, 0.05f, "she lies awash on her own wood: she does not go down");
            Assert.That(sim.Bilge.LowestFreeboard, Is.InRange(-0.2f, 0.05f), "with her gunwale at the water");
            sim.Bilge.Empty();
            Assert.AreEqual(ShipFate.Afloat, sim.Fate, "pumped out (a salvage), she is afloat again");
        }

        [Test]
        public void LaidPastHerVanishingAngleSheCapsizes()
        {
            var sim = Afloat();
            sim.ExtraTorque = new Vector3(0f, 0f, 30000f);                // a blow no crew could hold
            ShipKit.Run(sim, new FlatWater(), 10f);
            Assert.AreEqual(ShipFate.Capsized, sim.Fate);
            Assert.IsTrue(sim.Lost);
            var held = Afloat();
            held.ExtraTorque = new Vector3(0f, 0f, 2000f);
            ShipKit.Run(held, new FlatWater(), 10f);
            Assert.AreEqual(ShipFate.Afloat, held.Fate, "a press she can stand up to is not a capsize");
        }
    }
}
