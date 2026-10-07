using System;

namespace Kapman.Chain
{
    /// <summary>
    /// THE CUT'S RIGID TRANSFORM - a turn about the vertical then a shift, from one level's metres to another's (gateways
    /// 2.4): pos' = R(yaw) * pos + shift, heading' = heading + yaw. Heights are never touched (the water level matches by
    /// data). It moves the ship, her goal, her velocity, the camera's pivot and anything else in a corridor at the cut,
    /// in one frame. Doubles throughout: a level is under 6 km across and the result is exact to well under a millimetre.
    /// Yaw is a compass turn (clockwise seen from above, as headings are), x east and z north.
    /// </summary>
    public struct JoinTransform
    {
        /// <summary>The turn (degrees, clockwise from above).</summary>
        public double yaw;
        /// <summary>The shift after the turn (m).</summary>
        public double shiftX, shiftZ;

        public static JoinTransform Identity => new JoinTransform();

        /// <summary>The transform that puts a ship at one pose onto another (heading included).</summary>
        public static JoinTransform Between(ChainPose from, ChainPose to)
        {
            var t = new JoinTransform { yaw = ChainPose.Turn(from.heading, to.heading) };
            var (rx, rz) = t.ApplyDirection(from.x, from.z);
            t.shiftX = to.x - rx;
            t.shiftZ = to.z - rz;
            return t;
        }

        /// <summary>A point in the first level's metres, in the second's.</summary>
        public (double x, double z) Apply(double x, double z)
        {
            var (rx, rz) = ApplyDirection(x, z);
            return (rx + shiftX, rz + shiftZ);
        }

        /// <summary>A direction or a velocity (turned, not shifted).</summary>
        public (double x, double z) ApplyDirection(double x, double z)
        {
            double r = yaw * Math.PI / 180.0, c = Math.Cos(r), s = Math.Sin(r);
            // a heading h is the direction (sin h, cos h); turning it by yaw clockwise gives (sin(h+yaw), cos(h+yaw))
            return (x * c + z * s, -x * s + z * c);
        }

        public double ApplyHeading(double heading) => ChainPose.Wrap(heading + yaw);

        public ChainPose Apply(ChainPose p)
        {
            var (x, z) = Apply(p.x, p.z);
            return new ChainPose(x, z, ApplyHeading(p.heading));
        }

        /// <summary>The way back: Inverse().Apply(Apply(p)) == p.</summary>
        public JoinTransform Inverse()
        {
            var inv = new JoinTransform { yaw = -yaw };
            var (x, z) = inv.ApplyDirection(shiftX, shiftZ);
            inv.shiftX = -x;
            inv.shiftZ = -z;
            return inv;
        }

        /// <summary>This transform, then another: Then(next).Apply(p) == next.Apply(Apply(p)).</summary>
        public JoinTransform Then(JoinTransform next)
        {
            var t = new JoinTransform { yaw = ChainPose.Turn(0.0, yaw + next.yaw) };
            var (x, z) = next.ApplyDirection(shiftX, shiftZ);
            t.shiftX = x + next.shiftX;
            t.shiftZ = z + next.shiftZ;
            return t;
        }

        /// <summary>Whether this moves nothing more than a tolerance: metres for the shift, ten times that in degrees for the
        /// turn (1 mm and a hundredth of a degree by default).</summary>
        public bool IsIdentity(double metres = 0.001) =>
            Math.Abs(ChainPose.Turn(0.0, yaw)) < metres * 10.0 && Math.Abs(shiftX) < metres && Math.Abs(shiftZ) < metres;
    }
}
