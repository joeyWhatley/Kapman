using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE OARS (ship physics 2.6, the first night's part): the men's sustained power through the oars' efficiency,
    /// turned into a pull at her speed through the water (power / speed, never figured below 0.8 m/s), and capped by
    /// what an oar can pull at the catch. Effort 1 is "steady" (the class's sustained watts a man); a negative effort
    /// backs water. Fatigue and the sprint come with the crew.
    /// </summary>
    public static class Oars
    {
        public static float Thrust(ShipClass c, float effort, float aheadSpeed)
        {
            if (effort == 0f || c.oars <= 0)
                return 0f;
            float e = Mathf.Abs(effort);
            float power = c.oars * c.oarEfficiency * c.oarPowerW * e;
            float pull = Mathf.Min(power / Mathf.Max(Mathf.Abs(aheadSpeed), 0.8f), c.oars * c.oarPullMaxN * e);
            return effort > 0f ? pull : -pull;
        }
    }

    /// <summary>
    /// THE SHIP AS A BODY ON THE WATER - everything one 0.02 s step does, in one place, so the EditMode tests step exactly
    /// what the scene's ShipBody steps: the water's forces on the probe cells (HullForces, with the shallow-water factor
    /// from the bottom), her weight, the oars' pull at the effort ordered (less the men bailing), the helmsman on the
    /// steering oar and the oars on one side (Helmsman: night two), the square sail (Sail), the wind on her hull and bare
    /// mast, the water coming aboard and its free surface (Bilge), the bottom under her (Grounding), the anchor rope and
    /// the berth's lines (Anchor, Berth), anything pushed on her from outside (ExtraForce, ExtraTorque: the breaking seas
    /// to come, the tests), then the integrator. Her state is kept at her centre of gravity; <see cref="Origin"/> is the
    /// model's origin. The water aboard is in her mass: as it changes by more than 4 litres her mass, centre of gravity
    /// and inertia are made again with her hull left where it is.
    /// </summary>
    public sealed class ShipSim
    {
        /// <summary>The step (s): 50 a second of game time.</summary>
        public const float Dt = 0.02f;
        /// <summary>The wind on her hull against the 10 m wind (a log profile at about 1.5 m over open water).</summary>
        public const float HullWindFactor = 0.8f;

        public readonly ShipClass Class;
        public readonly HullProbes Hull;
        public readonly HullForces Forces;
        /// <summary>The man on the steering oar, the water in her, the bottom under her, her anchor and her berth's lines.</summary>
        public readonly Helmsman Helmsman = new Helmsman();
        public readonly Bilge Bilge;
        public readonly Grounding Grounding;
        public readonly Anchor Anchor;
        public readonly Berth Berth;
        public MassProps Mass { get; private set; }
        /// <summary>Her mass without the water aboard (hull, crew, gear, cargo).</summary>
        public MassProps DryMass { get; private set; }
        public ShipState State;

        /// <summary>The oars: 0 shipped, 1 steady, below 0 backing water.</summary>
        public float OarEffort;
        /// <summary>The heading she is ordered to (degrees from north), while HasHeadingOrder.</summary>
        public bool HasHeadingOrder;
        public float HeadingOrder;
        /// <summary>The fastest she is turned (degrees a second, with way on; a third of it dead in the water).</summary>
        public float TurnRateMax = 14f;
        /// <summary>The crew may pull or back the oars on one side to turn her when the steering oar has no water past it
        /// (the helm's tack keeps them shipped until it calls for them).</summary>
        public bool OarsMayTurn = true;
        /// <summary>The true wind at 10 m (world, m/s, the way the air moves; y ignored).</summary>
        public Vector3 TrueWind;
        /// <summary>How much of the sail is set: 0 furled, 1 full; between, the reefs taken in.</summary>
        public float SailSet;
        /// <summary>The bottom under the water (null: deep water everywhere).</summary>
        public IBedField Bed;
        /// <summary>The weather's rain (0-1), and the sea: its significant wave height (m) and the way its waves run
        /// (degrees from north), for the spray (the sea-state model sets them; 0 until it exists).</summary>
        public float Rain, SeaHs, SeaTowards;
        /// <summary>A force and torque from outside (world; through her centre of gravity), applied every step until cleared.</summary>
        public Vector3 ExtraForce, ExtraTorque;

        /// <summary>After each step: the oars' pull ahead (N) and the helm's turning torque (N m).</summary>
        public float Thrust { get; private set; }
        public float YawTorque { get; private set; }
        /// <summary>After each step: the sail's force (world, N), its part along her heading (N), the apparent wind's angle
        /// off the bow and the angle of attack the crew gave the sail (degrees).</summary>
        public Vector3 SailForce { get; private set; }
        public float SailAhead { get; private set; }
        public float ApparentWindOffBow { get; private set; }
        public float SailAlpha { get; private set; }
        /// <summary>The ahead pull the sail would give in this wind at this heading, set full (N; what the master weighs
        /// against the oars).</summary>
        public float SailAheadIfSet { get; private set; }
        /// <summary>After each step: the wind's force on her hull and bare mast (world, N).</summary>
        public Vector3 Windage { get; private set; }

        /// <summary>Her fate (the bilge's): afloat, swamped or capsized; lost if either.</summary>
        public ShipFate Fate => Bilge.Fate;
        public bool Lost => Bilge.Lost;
        /// <summary>The share of the men still at the oars (the rest are bailing).</summary>
        public float RowingShare => Bilge.Bailers <= 0 ? 1f : Mathf.Clamp01(1f - Bilge.Bailers / Mathf.Max(1f, Class.crew));

        Vector3 massDiagonal, inertiaDiagonal;
        float waterInMass;

        /// <summary>Her laden body, the model's origin <paramref name="waterlineOffset"/> above her design waterline.</summary>
        public ShipSim(ShipClass cls, float waterlineOffset = 0f)
        {
            Class = cls;
            Hull = HullProbes.ForClass(cls, waterlineOffset);
            State.rotation = Quaternion.identity;
            Bilge = new Bilge(cls, Hull);
            Anchor = new Anchor(cls);
            Berth = new Berth(cls);
            SetMass(Hydrostatics.Laden(cls, Hull.keelY));
            Forces = new HullForces(Hull, cls, Mass.mass);
            Grounding = new Grounding(cls, Hull, Mass.mass);
        }

        /// <summary>Changes what she carries (the mass, its centre, the inertia; the water aboard is added to it), her hull
        /// left where it is; the water's damping keeps the laden figure.</summary>
        public void SetMass(MassProps m)
        {
            DryMass = m;
            Combine();
        }

        /// <summary>Her mass now: the dry mass and the water aboard, its weight at the pool's centre on her centreline.</summary>
        void Combine()
        {
            var dry = DryMass;
            float water = Bilge.Water * Hydrostatics.Rho;
            MassProps now = dry;
            if (water > 0f)
            {
                float total = dry.mass + water;
                Vector3 pool = new Vector3(0f, Hull.keelY + Bilge.PoolHeight, dry.cg.z);
                now.mass = total;
                now.cg = (dry.cg * dry.mass + pool * water) / total;
                now.inertia = dry.inertia + water * new Vector3(Class.kPitch * Class.kPitch, Class.kYaw * Class.kYaw, 0f);
            }
            Vector3 origin = State.position - State.rotation * Mass.cg;
            Mass = now;
            massDiagonal = Hydrostatics.MassDiagonal(now, Class);
            inertiaDiagonal = Hydrostatics.InertiaDiagonal(now, Class);
            State.position = origin + State.rotation * now.cg;
            waterInMass = Bilge.Water;
        }

        public Vector3 MassDiagonal => massDiagonal;
        public Vector3 InertiaDiagonal => inertiaDiagonal;

        /// <summary>The model's origin in the world.</summary>
        public Vector3 Origin => State.position - State.rotation * Mass.cg;

        /// <summary>Puts her with the model's origin at a place and turn, moving at a velocity, not turning; the helmsman
        /// starts afresh and the bottom is looked at anew (the anchor and the lines are the caller's to stow).</summary>
        public void PlaceAt(Vector3 origin, Quaternion rotation, Vector3 velocity)
        {
            State.rotation = Rot.Normalize(rotation);
            State.position = origin + State.rotation * Mass.cg;
            State.velocity = velocity;
            State.angularVelocity = Vector3.zero;
            Forces.Reset();
            Helmsman.Reset();
            Grounding.Forget();
        }

        /// <summary>One step on this water.</summary>
        public void Step(IWaterField water, float dt = Dt)
        {
            Forces.Shallow = Grounding.Shallow;
            Forces.Compute(State, Mass, water, dt, out Vector3 force, out Vector3 torque);
            force.y -= Mass.mass * Hydrostatics.G;
            Thrust = Oars.Thrust(Class, OarEffort * RowingShare, Forces.AheadSpeed);
            force += State.rotation * new Vector3(0f, 0f, Thrust);
            Helmsman.Step(this, water, OarsMayTurn, Grounding.SternClearance, dt, ref force, ref torque);
            YawTorque = Helmsman.YawTorque;
            SailStep(ref force, ref torque);
            WindageStep(ref force, ref torque);
            Bilge.damageLeak = Grounding.LeakAdded;
            Bilge.Step(this, Rain, SeaHs, SeaTowards, DryMass.mass, dt, ref torque);
            Grounding.Compute(State, Mass, massDiagonal, Bed, dt, ref force, ref torque);
            Anchor.Compute(this, water, Bed, dt, ref force, ref torque);
            Berth.Compute(this, dt, ref force, ref torque);
            force += ExtraForce;
            torque += ExtraTorque;
            ShipIntegrator.Step(ref State, force, torque, massDiagonal, inertiaDiagonal, dt);
            if (Mathf.Abs(Bilge.Water - waterInMass) > 0.004f || (Bilge.Water == 0f && waterInMass != 0f))
                Combine();
        }

        /// <summary>
        /// The sail's push and heel: its force acts at the centre of effort, sailCe above the design waterline on the mast
        /// (amidships), so the side force heels her against the hull's lateral resistance lower down.
        /// </summary>
        void SailStep(ref Vector3 force, ref Vector3 torque)
        {
            SailForce = Vector3.zero;
            SailAhead = 0f;
            SailAheadIfSet = 0f;
            if (Class.sailArea <= 0f)
                return;
            Vector3 full = Sail.Force(TrueWind, State.velocity, State.rotation, Class.sailArea, Class.closestToWind, out float off, out float alpha);
            ApparentWindOffBow = off;
            SailAlpha = alpha;
            Vector3 fwd = State.rotation * Vector3.forward;
            fwd.y = 0f;
            fwd.Normalize();
            SailAheadIfSet = Vector3.Dot(full, fwd);
            float set = Mathf.Clamp01(SailSet);
            if (set <= 0f)
                return;
            SailForce = full * set;
            SailAhead = SailAheadIfSet * set;
            Vector3 ce = new Vector3(0f, Hull.keelY + Class.draught + Class.sailCe, 0f);
            Vector3 arm = State.rotation * (ce - Mass.cg);
            force += SailForce;
            torque += Vector3.Cross(arm, SailForce);
        }

        /// <summary>
        /// The wind on her hull and bare mast (ship physics 2.6, 2.9): 0.5 rho_air A |V_a| V_a along the apparent wind at
        /// her hull's height, A her side-on windage when the wind is on her beam and a third of it head or stern on, acting
        /// a little above her gunwale amidships. It is what blows her back on her anchor, sets her to leeward hove to and
        /// holds her back rowing into a breeze.
        /// </summary>
        void WindageStep(ref Vector3 force, ref Vector3 torque)
        {
            Windage = Vector3.zero;
            if (Class.windage <= 0f)
                return;
            Vector3 air = TrueWind * HullWindFactor - State.velocity;
            air.y = 0f;
            float speed = air.magnitude;
            if (speed < 0.05f)
                return;
            Vector3 fwd = State.rotation * Vector3.forward;
            fwd.y = 0f;
            fwd.Normalize();
            float across = Mathf.Abs(Vector3.Cross(fwd, air / speed).y);
            float area = Class.windage * (0.3f + 0.7f * across);
            Vector3 f = 0.5f * Sail.RhoAir * area * speed * air;
            Vector3 at = new Vector3(0f, Hull.keelY + Hull.depth + 0.3f, Mass.cg.z);
            force += f;
            torque += Vector3.Cross(State.rotation * (at - Mass.cg), f);
            Windage = f;
        }
    }

    /// <summary>
    /// THE BODY'S CLOCK (ship physics 2.1): Time.timeScale is never touched; instead each frame adds real seconds x
    /// LevelClock.Speed to an accumulator and the body takes as many 0.02 s steps as it holds - one a frame at 1x and
    /// 60 fps (some frames none), two at 2x, four at 4x - never more than eight a frame, so a hitch does not spiral
    /// (the excess is dropped). <see cref="Alpha"/> is how far the frame has gone into the next step, for drawing her
    /// between the last two states.
    /// </summary>
    public sealed class StepClock
    {
        public const int MaxSteps = 8;
        public readonly float step;
        float acc;

        /// <summary>Game seconds stepped in all.</summary>
        public double Stepped { get; private set; }

        public StepClock(float step = ShipSim.Dt)
        {
            this.step = step;
        }

        /// <summary>The steps to take this frame.</summary>
        public int Take(float realDelta, float speed)
        {
            acc += Mathf.Max(0f, realDelta) * Mathf.Max(0f, speed);
            int n = Mathf.FloorToInt(acc / step);
            if (n > MaxSteps)
            {
                n = MaxSteps;
                acc = step * MaxSteps;                                    // the rest of a long frame is let go
            }
            acc -= n * step;
            Stepped += n * step;
            return n;
        }

        /// <summary>The share of a step already gone by (0-1).</summary>
        public float Alpha => Mathf.Clamp01(acc / step);

        /// <summary>Starts afresh (after a pause or a move): nothing owed.</summary>
        public void Clear() => acc = 0f;
    }
}
