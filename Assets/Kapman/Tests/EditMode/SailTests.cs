using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The square sail (ship physics 2.5) on the Gislinge: the curve, her speed on a reach and a run against the
    /// replica's trials (4.8 kn on a beam reach in 8 m/s), nothing head to wind, and a sensible heel.</summary>
    public class SailTests
    {
        const float Kn = 0.514444f;

        /// <summary>She sails north from a standing start with the sail full, held to her heading, for a while; the wind
        /// is the way the air moves.</summary>
        static ShipSim Sailing(Vector3 trueWind, float seconds = 150f, float set = 1f)
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = trueWind, SailSet = set, HasHeadingOrder = true, HeadingOrder = 0f };
            ShipKit.Float(sim);
            ShipKit.Run(sim, new FlatWater(), seconds);
            return sim;
        }

        static float Heel(ShipSim sim) => Vector3.Angle(sim.State.rotation * Vector3.up, Vector3.up);

        [Test]
        public void TheCurveIsTheDesigns()
        {
            Sail.Coefficients(30f, out float cl, out float cd);
            Assert.AreEqual(1.2f, cl, 1e-4f);
            Assert.AreEqual(0.5f, cd, 1e-4f);
            Sail.Coefficients(25f, out cl, out cd);
            Assert.AreEqual(1.075f, cl, 1e-4f);
            Assert.AreEqual(0.4f, cd, 1e-4f);
            Sail.Coefficients(90f, out cl, out cd);
            Assert.AreEqual(0f, cl, 1e-4f);
            Assert.AreEqual(1.3f, cd, 1e-4f);
            Assert.AreEqual(0f, Sail.Trim(40f, 58f), "inside the closest point of sailing the sail shakes");
            Assert.AreEqual(90f, Sail.Trim(180f, 58f), "running: square to the wind");
        }

        [Test]
        public void ABeamReachInEightMetresASecondMakesHerTrialSpeed()
        {
            var sim = Sailing(new Vector3(-8f, 0f, 0f));                    // from the east: on her starboard beam
            float kn = sim.Forces.AheadSpeed / Kn;
            Assert.That(kn, Is.InRange(4.0f, 6.0f), $"{kn:0.00} kn (the replica: 4.8)");
            Assert.That(Heel(sim), Is.InRange(2f, 20f), $"heel {Heel(sim):0.0} deg");
            Assert.Greater(sim.SailAhead, 0f);
        }

        [Test]
        public void HeadToWindTheSailDoesNotDriveHer()
        {
            var sim = Sailing(new Vector3(0f, 0f, -8f), 40f);              // from the north, dead ahead
            Assert.LessOrEqual(sim.SailAheadIfSet, 0f);
            Assert.LessOrEqual(sim.Forces.AheadSpeed, 0.05f);
        }

        [Test]
        public void RunningBeforeTheWindSheMakesWay()
        {
            var sim = Sailing(new Vector3(0f, 0f, 8f));                    // from the south, dead astern
            float kn = sim.Forces.AheadSpeed / Kn;
            Assert.That(kn, Is.InRange(3.0f, 6.0f), $"{kn:0.00} kn");
            Assert.Less(Heel(sim), 3f, "no side force running");
        }

        [Test]
        public void ReefedInAStrongBreezeSheStandsUp()
        {
            float wind = 14f;
            var sim = Sailing(new Vector3(-wind, 0f, 0f), 120f, ShipBody.SailReef(wind));
            Assert.Less(Heel(sim), 25f, $"heel {Heel(sim):0.0} deg");
            Assert.That(sim.Forces.AheadSpeed / Kn, Is.InRange(4.0f, 8.0f), "a reefed reach in a strong breeze");
        }
    }
}
