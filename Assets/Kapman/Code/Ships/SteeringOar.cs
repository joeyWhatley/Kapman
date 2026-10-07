using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE STEERING OAR (ship physics 2.7): a lifting blade on the starboard quarter. Its force is
    /// 0.5 rho A v^2 (C_L l + C_D d) in the water's flow past the blade (her own motion through the water there, her
    /// turning included): C_L = 1.5 sin(alpha) up to a stall at 30 degrees, then a flat plate's normal force
    /// (1.1 sin alpha, with its lift and drag parts), so an oar put over too far loses its grip; the drag is a small
    /// profile drag and the lift's induced drag for a blade of span twice its chord (a guess). Going astern the water
    /// meets the blade's after edge first: the force is reversed by the geometry and halved (the design: "weak and
    /// reversed"). The helmsman's angle (+ the blade's after edge to starboard, which turns her to starboard) is limited
    /// to 35 degrees and moves at 30 degrees a second.
    /// </summary>
    public static class SteeringOar
    {
        public const float MaxAngle = 35f, SlewRate = 30f, StallDeg = 30f, LiftSlope = 1.5f, AspectRatio = 2f;
        /// <summary>With less water than this under her keel at the stern (m) the oar is lifted and only the oars steer her.</summary>
        public const float LiftedBelow = 0.3f;

        /// <summary>The blade's lift and drag coefficients at an angle of attack (degrees, signed: lift takes its sign).</summary>
        public static void Coefficients(float alphaDeg, out float cl, out float cd)
        {
            float a = Mathf.Min(Mathf.Abs(alphaDeg), 90f) * Mathf.Deg2Rad;
            if (Mathf.Abs(alphaDeg) <= StallDeg)
            {
                cl = LiftSlope * Mathf.Sin(a);
                cd = 0.02f + cl * cl / (Mathf.PI * AspectRatio * 0.9f);
            }
            else
            {
                float cn = 1.1f * Mathf.Sin(a);                          // stalled: a flat plate's normal force
                cl = cn * Mathf.Cos(a);
                cd = 0.02f + cn * Mathf.Sin(a);
            }
            if (alphaDeg < 0f)
                cl = -cl;
        }

        /// <summary>
        /// The water's force on the blade (on her own axes, N; y nought) for the blade's velocity through the water on her
        /// own axes and its angle (degrees, + after edge to starboard). Out: the angle of attack it meets (degrees; from
        /// astern, measured to the after edge).
        /// </summary>
        public static Vector3 Force(float area, float angleDeg, Vector3 bladeVelocity, out float alphaDeg)
        {
            alphaDeg = 0f;
            float wx = -bladeVelocity.x, wz = -bladeVelocity.z;          // the water past the blade
            float speed = Mathf.Sqrt(wx * wx + wz * wz);
            if (speed < 1e-3f || area <= 0f)
                return Vector3.zero;
            float fx = wx / speed, fz = wz / speed;
            float d = angleDeg * Mathf.Deg2Rad;
            float cx = Mathf.Sin(d), cz = -Mathf.Cos(d);                  // the blade's chord, fore edge to after edge
            float alpha = Mathf.Atan2(fx * cz - fz * cx, fx * cx + fz * cz) * Mathf.Rad2Deg;
            float weak = 1f;
            if (Mathf.Abs(alpha) > 90f)
            {
                alpha -= 180f * Mathf.Sign(alpha);                       // the flow from astern: the after edge leads
                weak = 0.5f;
            }
            alphaDeg = alpha;
            Coefficients(alpha, out float cl, out float cdrag);
            float q = 0.5f * Hydrostatics.Rho * area * speed * speed * weak;
            // lift across the flow, drag along it
            return new Vector3(q * (cl * fz + cdrag * fx), 0f, q * (-cl * fx + cdrag * fz));
        }
    }

    /// <summary>
    /// THE HELMSMAN (ship physics 2.7): a man on the steering oar, not the player. Given the heading ordered he wants a
    /// rate of turn (0.35 a second of the error, up to the turn rate she has, a third of it with no way on), and puts the
    /// oar over to reach it: a rate loop whose gain he sets by feel from the oar's bite at the flow he feels (so he uses
    /// little helm at speed and a lot when she is slow), with an integral that learns the steady helm a wind or a stream
    /// needs. A good helmsman (skill 1) sees her swing almost at once and keeps the blade out of its stall; a poor one
    /// (skill 0) sees it late and puts on too much helm, so she wanders and he stalls the oar and loses a little speed.
    /// When the oar has little water past it (under 0.8 m/s, or lifted in shoal water) or the men are rowing anyway, the
    /// oars on one side pull or back to turn her (the class's turning torque), if the crew may (the tack's own logic
    /// keeps them shipped until it calls for them). With no heading ordered he holds the oar amidships.
    /// </summary>
    public sealed class Helmsman
    {
        /// <summary>The man's skill (0-1; the crew design will give it).</summary>
        public float skill = 0.7f;

        /// <summary>The share of the heading error he asks of her rate of turn, each second.</summary>
        public const float HeadingGain = 0.35f;
        /// <summary>The oars' turning stops once she has this much way through the water (m/s).</summary>
        public const float OarsTurnBelow = 0.8f;

        float integral, seenError, seenRate;

        /// <summary>After each step: the oar's angle (degrees, + turns her to starboard), the angle of attack it meets, the
        /// water's speed past it, whether it is lifted, its force (world, N), the oars' turning torque (N m), and the
        /// whole turning torque the helm gave her (N m about her yaw axis, + to starboard).</summary>
        public float Angle { get; private set; }
        public float Alpha { get; private set; }
        public float Flow { get; private set; }
        public bool Lifted { get; private set; }
        public Vector3 OarForce { get; private set; }
        public float OarsTorque { get; private set; }
        public float YawTorque { get; private set; }
        /// <summary>The oar's grip: the water's speed past it squared over her speed through the water squared (1 in still
        /// water; low when a following sea runs with her and robs it, or when she has no way).</summary>
        public float Authority { get; private set; } = 1f;
        public bool Stalled => Mathf.Abs(Alpha) > SteeringOar.StallDeg;

        /// <summary>Amidships, nothing learned (after she is put somewhere new).</summary>
        public void Reset()
        {
            Angle = Alpha = 0f;
            integral = seenError = seenRate = 0f;
            OarsTorque = YawTorque = 0f;
            OarForce = Vector3.zero;
        }

        /// <summary>
        /// One step: steers for sim's heading order (if any), adds the oar's force and the oars' torque to hers.
        /// <paramref name="sternClearance"/> is the water under her keel at the stern (m; infinity in deep water).
        /// </summary>
        public void Step(ShipSim sim, IWaterField water, bool oarsMayTurn, float sternClearance, float dt, ref Vector3 force, ref Vector3 torque)
        {
            var s = sim.State;
            var c = sim.Class;
            var m = sim.Mass;
            Quaternion r = s.rotation;
            // the blade's force is taken on her centreline at its depth and arm: it hangs on the starboard quarter, but the
            // small yaw its drag would give from there is the helmsman's to hold without thinking
            Vector3 local = new Vector3(0f, sim.Hull.keelY + 0.5f * c.draught, m.cg.z - c.rudderArm);
            Vector3 at = s.position + r * (local - m.cg);
            Vector3 vb = Rot.Inverse(r) * (s.PointVelocity(at) - water.Current(at));
            float u = sim.Forces.AheadSpeed;
            Lifted = sternClearance < SteeringOar.LiftedBelow;
            float area = Lifted ? 0f : c.rudderArea;
            float rate = s.angularVelocity.y;
            float iyaw = sim.InertiaDiagonal.y;
            float lag = 0.15f + 0.6f * (1f - skill);
            float k = Mathf.Min(1f, dt / lag);

            float wantRate = 0f, command = 0f;
            bool steering = sim.HasHeadingOrder;
            if (steering)
            {
                float error = Mathf.DeltaAngle(Rot.Heading(r), sim.HeadingOrder) * Mathf.Deg2Rad;
                seenError += (error - seenError) * k;
                seenRate += (rate - seenRate) * k;
                float most = sim.TurnRateMax * Mathf.Deg2Rad * (0.35f + 0.65f * Mathf.Clamp01(Mathf.Max(0f, u)));
                wantRate = Mathf.Clamp(HeadingGain * seenError, -most, most);

                // the rate loop: his gain from the oar's bite (N m per radian of oar) at the flow he feels
                float flow = Mathf.Max(Mathf.Abs(vb.z), 0.3f);
                float bite = 0.5f * Hydrostatics.Rho * area * flow * flow * SteeringOar.LiftSlope * c.rudderArm;
                float tau = 1.5f;
                float gain = bite > 1e-3f ? Mathf.Clamp(iyaw / (tau * bite) - c.rudderArm / flow, 0.3f, 6f) : 6f;
                gain *= 1f + 0.6f * (1f - skill);                            // the poor helmsman's heavy hand
                float rateError = wantRate - seenRate;
                command = gain * rateError + integral;
                float most2 = SteeringOar.MaxAngle * Mathf.Deg2Rad;
                bool pinned = Mathf.Abs(command) >= most2 && Mathf.Sign(command) == Mathf.Sign(rateError);
                if (!pinned)
                    integral = Mathf.Clamp(integral + gain / 4f * rateError * dt, -0.26f, 0.26f);
                command *= Mathf.Rad2Deg;
                // a good man keeps the blade out of its stall: the angle of attack is his angle plus the flow's
                if (skill >= 0.5f && vb.z > 0.3f)
                {
                    float phi = Mathf.Atan2(vb.x, vb.z) * Mathf.Rad2Deg;
                    float edge = SteeringOar.StallDeg - 2f;
                    command = Mathf.Clamp(command, -edge - phi, edge - phi);
                }
                command = Mathf.Clamp(command, -SteeringOar.MaxAngle, SteeringOar.MaxAngle);
            }
            else
            {
                integral = 0f;
                seenError = 0f;
                seenRate = rate;
            }
            Angle = Mathf.MoveTowards(Angle, command, SteeringOar.SlewRate * dt);

            // the oar's force where it is
            Vector3 fb = SteeringOar.Force(area, Angle, vb, out float alpha);
            Alpha = alpha;
            Flow = Mathf.Sqrt(vb.x * vb.x + vb.z * vb.z);
            Authority = u * u > 0.01f ? Mathf.Clamp(vb.z * vb.z / (u * u), 0f, 2f) : 0f;
            Vector3 fw = r * fb;
            Vector3 tw = Vector3.Cross(at - s.position, fw);
            force += fw;
            torque += tw;
            OarForce = fw;

            // the oars on one side, when the oar has no water past it or the men are rowing anyway
            OarsTorque = 0f;
            if (steering && oarsMayTurn && c.oars > 0 && c.oarTurnTorqueNm > 0f)
            {
                float share = sim.OarEffort != 0f || Lifted ? 1f : 1f - Mathf.Clamp01(Mathf.Max(0f, u) / OarsTurnBelow);
                if (share > 0f)
                {
                    float cap = c.oarTurnTorqueNm * share;
                    OarsTorque = Mathf.Clamp(iyaw * (wantRate - rate) / 0.5f, -cap, cap);
                    torque += r * new Vector3(0f, OarsTorque, 0f);
                }
            }
            YawTorque = (Rot.Inverse(r) * tw).y + OarsTorque;
        }
    }
}
