using Kapman.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Ship physics night two: the steering oar (its lift, its stall, weak and reversed astern, lifted in shoal
    /// water), the helmsman who steers with it and the oars on one side when she has no way, and the helm's judgement under
    /// sail (the refusal inside her closest point, beating, tacking, wearing, in irons).</summary>
    public class HelmTests
    {
        const float Kn = 0.514444f;

        /// <summary>A Gislinge under sail in a wind from the north, heading a way, settled for a while with the helmsman on it.</summary>
        static ShipSim Sailing(float wind, float heading, float seconds = 150f)
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -wind), SailSet = 1f, HasHeadingOrder = true, HeadingOrder = heading };
            ShipKit.Float(sim);
            sim.State.rotation = Rot.Yaw(heading);
            ShipKit.Run(sim, new FlatWater(), seconds);
            return sim;
        }

        /// <summary>Runs the helm and the body together for a while, the helm steering for a fixed point.</summary>
        static void Make(ShipSim sim, Helm helm, Vector3 goal, bool underSail, float seconds, IWaterField water = null)
        {
            water ??= new FlatWater();
            int n = Mathf.RoundToInt(seconds / ShipSim.Dt);
            for (int i = 0; i < n; i++)
            {
                Vector3 to = goal - sim.Origin;
                helm.Steer(sim, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg, underSail, ShipSim.Dt);
                sim.Step(water);
            }
        }

        [Test]
        public void TheOarsCurveStallsPastThirtyDegrees()
        {
            SteeringOar.Coefficients(20f, out float cl20, out _);
            SteeringOar.Coefficients(30f, out float cl30, out float cd30);
            SteeringOar.Coefficients(31f, out float cl31, out float cd31);
            SteeringOar.Coefficients(40f, out float cl40, out _);
            SteeringOar.Coefficients(-20f, out float clm20, out _);
            Assert.AreEqual(1.5f * Mathf.Sin(20f * Mathf.Deg2Rad), cl20, 1e-4f, "C_L = 1.5 sin(alpha) below the stall");
            Assert.AreEqual(0.75f, cl30, 1e-4f);
            Assert.Less(cl31, 0.7f * cl30, "past 30 degrees it stalls: the lift falls away");
            Assert.Less(cl40, cl30, "and stays below what it was");
            Assert.Greater(cd31, 2f * cd30, "while the drag jumps");
            Assert.AreEqual(-cl20, clm20, 1e-6f, "the other way, the other side");
        }

        [Test]
        public void TheOarTurnsHerTheWayItIsPutAndWeaklyAstern()
        {
            // the blade moving ahead at 2 m/s, its after edge put 20 degrees to starboard: the water pushes it to port,
            // so her stern goes to port and her head to starboard
            Vector3 ahead = SteeringOar.Force(0.12f, 20f, new Vector3(0f, 0f, 2f), out float alpha);
            Assert.AreEqual(20f, alpha, 0.01f);
            Assert.Less(ahead.x, 0f, "the stern is pushed to port");
            Assert.Less(ahead.z, 0f, "and the blade drags");
            float q = 0.5f * Hydrostatics.Rho * 0.12f * 4f;
            SteeringOar.Coefficients(20f, out float cl, out float cd);
            Assert.AreEqual(q * Mathf.Sqrt(cl * cl + cd * cd), ahead.magnitude, 0.01f * ahead.magnitude, "0.5 rho A v^2 sqrt(C_L^2 + C_D^2)");
            Vector3 astern = SteeringOar.Force(0.12f, 20f, new Vector3(0f, 0f, -2f), out _);
            Assert.Greater(astern.x, 0f, "going astern the oar works the other way");
            Assert.AreEqual(0.5f * Mathf.Abs(ahead.x), Mathf.Abs(astern.x), 0.05f * Mathf.Abs(ahead.x), "and half as hard");
        }

        [Test]
        public void TheHelmsmanBringsHerRoundUnderOarsWithoutOvershooting()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { OarEffort = 1f, HasHeadingOrder = true, HeadingOrder = 0f };
            ShipKit.Float(sim);
            ShipKit.Run(sim, new FlatWater(), 120f);
            sim.HeadingOrder = 90f;
            float reached = -1f, over = 0f, most = 0f;
            for (int i = 0; i < 3000; i++)
            {
                sim.Step(new FlatWater());
                float e = Mathf.DeltaAngle(90f, Rot.Heading(sim.State.rotation));
                if (reached < 0f && Mathf.Abs(e) < 3f)
                    reached = i * ShipSim.Dt;
                if (reached >= 0f)
                    over = Mathf.Max(over, Mathf.Abs(e));
                most = Mathf.Max(most, Mathf.Abs(sim.Helmsman.Angle));
            }
            Assert.That(reached, Is.InRange(5f, 40f), "she comes round 90 degrees in her own time");
            Assert.Less(over, 5f, "and holds it without swinging past");
            Assert.LessOrEqual(most, SteeringOar.MaxAngle + 1e-3f, "the oar never goes past 35 degrees");
        }

        [Test]
        public void WithNoWayTheOarDoesNothingAndTheOarsTurnHer()
        {
            var still = new ShipSim(ShipKit.Gislinge()) { HasHeadingOrder = true, HeadingOrder = 90f, OarsMayTurn = false };
            ShipKit.Float(still);
            ShipKit.Run(still, new FlatWater(), 20f);
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(0f, Rot.Heading(still.State.rotation))), 3f, "dead in the water the steering oar has nothing to bite on");

            var rowed = new ShipSim(ShipKit.Gislinge()) { HasHeadingOrder = true, HeadingOrder = 90f };
            ShipKit.Float(rowed);
            ShipKit.Run(rowed, new FlatWater(), 90f);
            Assert.AreEqual(90f, Rot.Heading(rowed.State.rotation), 5f, "the oars on one side bring her round where she lies");
            Assert.Less(new Vector2(rowed.Origin.x, rowed.Origin.z).magnitude, 3f, "turning, not going");
        }

        [Test]
        public void InShoalWaterTheOarIsLifted()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { OarEffort = 1f, HasHeadingOrder = true, HeadingOrder = 0f };
            ShipKit.Float(sim);
            float keel = sim.Origin.y + sim.Hull.keelY;
            sim.Bed = new FlatBed(keel - 0.2f, GroundKind.Sand);
            ShipKit.Run(sim, new FlatWater(), 5f);
            Assert.IsTrue(sim.Helmsman.Lifted, "0.2 m under her keel: the oar comes up");
            Assert.AreEqual(Vector3.zero, sim.Helmsman.OarForce);
            sim.Bed = new FlatBed(keel - 2f, GroundKind.Sand);
            ShipKit.Run(sim, new FlatWater(), 2f);
            Assert.IsFalse(sim.Helmsman.Lifted, "and goes down again in deep water");
        }

        [Test]
        public void UnderSailTheHelmWillNotLieInsideHerClosestPoint()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -8f), SailSet = 1f };
            ShipKit.Float(sim);
            sim.State.rotation = Rot.Yaw(70f);
            var helm = new Helm { minBoard = 1000f };                      // one long board: no tack in this test
            Make(sim, helm, new Vector3(0f, 0f, 5000f), true, 150f);     // the goal dead to windward
            Assert.IsTrue(helm.Beating, "the course lies inside her closest point");
            StringAssert.Contains("won't lie closer", helm.Line, "and the helm says so");
            float off = Mathf.Abs(Mathf.DeltaAngle(0f, Rot.Heading(sim.State.rotation)));
            Assert.That(off, Is.InRange(sim.Class.closestToWind - 1f, sim.Class.closestToWind + 6f), "full and by: just off her closest point");
            Assert.Greater(sim.Forces.AheadSpeed, 1f, "and she sails");
            Assert.Greater(sim.State.velocity.z, 0.2f, "making ground to windward");

            var rowing = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -8f), OarEffort = 1f };
            ShipKit.Float(rowing);
            var helm2 = new Helm();
            Make(rowing, helm2, new Vector3(0f, 0f, 5000f), false, 60f);
            Assert.IsFalse(helm2.Beating, "under oars she goes straight at it");
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, Rot.Heading(rowing.State.rotation)), 3f);
        }

        [Test]
        public void SheBeatsUpToAGoalToWindwardTackingAsSheGoes()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -8f), SailSet = 1f };
            ShipKit.Float(sim);
            sim.State.rotation = Rot.Yaw(60f);
            var helm = new Helm();
            Make(sim, helm, new Vector3(0f, 0f, 400f), true, 900f);
            Assert.GreaterOrEqual(helm.Tacks, 2, "she goes about more than once");
            Assert.AreEqual(0, helm.Wears, "in 8 m/s the crew tack");
            Assert.Greater(sim.Origin.z, 250f, $"she has worked up to windward ({sim.Origin.z:0} m of 400 in 15 minutes)");
            Assert.Less(Mathf.Abs(sim.Origin.x), 250f, "keeping near the line to the goal");
        }

        [Test]
        public void InAStrongWindTheCrewWearRatherThanTack()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -14f), SailSet = ShipBody.SailReef(14f) };
            ShipKit.Float(sim);
            sim.State.rotation = Rot.Yaw(60f);
            var helm = new Helm();
            bool ran = false;
            int n = Mathf.RoundToInt(400f / ShipSim.Dt);
            for (int i = 0; i < n; i++)
            {
                Vector3 to = new Vector3(0f, 0f, 400f) - sim.Origin;
                helm.Steer(sim, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg, true, ShipSim.Dt);
                sim.Step(new FlatWater());
                if (helm.Doing == Manoeuvre.Wearing && Mathf.Abs(Mathf.DeltaAngle(180f, Rot.Heading(sim.State.rotation))) < 30f)
                    ran = true;
            }
            Assert.GreaterOrEqual(helm.Wears, 1, "she goes about by wearing");
            Assert.AreEqual(0, helm.Tacks, "not by tacking");
            Assert.IsTrue(ran, "round through the run, her stern to the wind");
            Assert.AreEqual(ShipFate.Afloat, sim.Fate);
        }

        [Test]
        public void InIronsTheCrewPullHerHeadRoundWithTheOars()
        {
            var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -8f), SailSet = 1f };
            ShipKit.Float(sim);
            sim.State.rotation = Rot.Yaw(60f);
            sim.State.velocity = sim.State.rotation * new Vector3(0f, 0f, 0.9f);   // close-hauled with little way
            var helm = new Helm { tackMinSpeed = 0f };                             // the crew try to tack anyway
            bool shipped = true;
            int n = Mathf.RoundToInt(100f / ShipSim.Dt);
            for (int i = 0; i < n; i++)
            {
                helm.Steer(sim, 300f, true, ShipSim.Dt);                           // a course on the other tack
                if (helm.Doing == Manoeuvre.Tacking && helm.Irons == 0)
                    shipped &= !sim.OarsMayTurn;
                sim.Step(new FlatWater());
            }
            Assert.AreEqual(1, helm.Tacks);
            Assert.AreEqual(1, helm.Irons, "she lost her way in the wind's eye");
            Assert.IsTrue(shipped, "the oars stayed in until she was in irons");
            Assert.AreEqual(Manoeuvre.None, helm.Doing, "and came round in the end");
            Assert.AreEqual(300f, Rot.Heading(sim.State.rotation), 15f, "on the new tack");
        }

        [Test]
        public void TheSameOrdersAndWaterGiveTheSameShip()
        {
            ShipSim Run()
            {
                var sim = new ShipSim(ShipKit.Gislinge()) { TrueWind = new Vector3(0f, 0f, -8f), SailSet = 1f };
                ShipKit.Float(sim);
                sim.State.rotation = Rot.Yaw(60f);
                var swell = new SineWater(0.3f, 30f, 200f);
                var helm = new Helm();
                for (int i = 0; i < 6000; i++)
                {
                    swell.Advance(ShipSim.Dt);
                    Vector3 to = new Vector3(0f, 0f, 400f) - sim.Origin;
                    helm.Steer(sim, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg, true, ShipSim.Dt);
                    sim.Step(swell);
                }
                return sim;
            }
            var a = Run();
            var b = Run();
            Assert.AreEqual(a.State.position, b.State.position, "test 13: deterministic");
            Assert.AreEqual(a.State.rotation.x, b.State.rotation.x);
            Assert.AreEqual(a.State.rotation.w, b.State.rotation.w);
            Assert.AreEqual(a.Bilge.Water, b.Bilge.Water);
        }
    }
}
