using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE SHIP'S BODY AT ONE INSTANT (ship physics 2.1's ShipState, the motion part; the orders, damage and cargo join it
    /// as those systems come): where her centre of gravity is, how she is turned, how fast she goes and turns. World
    /// metres; the angular velocity on her own axes (x pitch, y yaw, z roll; rad/s). Plain fields, so a save can write it.
    /// </summary>
    public struct ShipState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        /// <summary>The velocity of a point of her (world), from her own and her turning.</summary>
        public Vector3 PointVelocity(Vector3 worldPoint) =>
            velocity + Vector3.Cross(rotation * angularVelocity, worldPoint - position);
    }

    /// <summary>
    /// THE ONLY THING THAT MOVES A SHIP (ship physics 2.1): one semi-implicit Euler step of the six degrees of freedom.
    /// The force (world, through the centre of gravity) is taken onto her own axes and divided by the mass she answers
    /// with on each - her own plus the water she drags along (added mass: surge, sway, heave differ) - so the added mass
    /// goes in properly; the torque likewise with her inertia per axis, with the gyroscopic term of Euler's equations.
    /// Velocities are stepped first, then the position and the turn with the new velocities.
    /// </summary>
    public static class ShipIntegrator
    {
        public static void Step(ref ShipState s, Vector3 force, Vector3 torque, Vector3 massDiagonal, Vector3 inertiaDiagonal, float dt)
        {
            Quaternion r = s.rotation, back = Rot.Inverse(r);

            Vector3 vb = back * s.velocity, fb = back * force;
            vb.x += dt * fb.x / massDiagonal.x;
            vb.y += dt * fb.y / massDiagonal.y;
            vb.z += dt * fb.z / massDiagonal.z;
            s.velocity = r * vb;

            Vector3 w = s.angularVelocity, tb = back * torque;
            Vector3 iw = new Vector3(inertiaDiagonal.x * w.x, inertiaDiagonal.y * w.y, inertiaDiagonal.z * w.z);
            Vector3 net = tb - Vector3.Cross(w, iw);
            w.x += dt * net.x / inertiaDiagonal.x;
            w.y += dt * net.y / inertiaDiagonal.y;
            w.z += dt * net.z / inertiaDiagonal.z;
            s.angularVelocity = w;

            s.position += s.velocity * dt;
            float rate = w.magnitude;
            if (rate > 1e-9f)
                s.rotation = Rot.Normalize(r * Rot.AxisAngle(w / rate, rate * dt));
        }
    }
}
