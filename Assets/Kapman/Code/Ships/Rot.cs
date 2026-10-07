using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// The few rotation sums the ship needs, written out so the pure code never calls into the engine (Unity's own
    /// Quaternion.Inverse, AngleAxis and Euler are engine calls; these are the same formulas): the tests then run the
    /// very sums the scene runs, with or without an engine under them. The body frame is Unity's: x starboard, y up,
    /// z ahead.
    /// </summary>
    public static class Rot
    {
        /// <summary>The inverse of a unit rotation (its conjugate).</summary>
        public static Quaternion Inverse(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, q.w);

        /// <summary>A turn of an angle (radians) about a unit axis, the same way Unity's AngleAxis turns.</summary>
        public static Quaternion AxisAngle(Vector3 unitAxis, float radians)
        {
            float s = Mathf.Sin(radians * 0.5f);
            return new Quaternion(unitAxis.x * s, unitAxis.y * s, unitAxis.z * s, Mathf.Cos(radians * 0.5f));
        }

        /// <summary>A heading (degrees from north, clockwise seen from above) as a rotation.</summary>
        public static Quaternion Yaw(float degrees) => AxisAngle(Vector3.up, degrees * Mathf.Deg2Rad);

        /// <summary>A rotation scaled back to unit length (the integrator's steps drift it a little).</summary>
        public static Quaternion Normalize(Quaternion q)
        {
            float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return m > 1e-9f ? new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m) : Quaternion.identity;
        }

        /// <summary>Her heading (degrees from north, 0-360) from a rotation.</summary>
        public static float Heading(Quaternion q)
        {
            Vector3 f = q * Vector3.forward;
            float h = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
            return h < 0f ? h + 360f : h;
        }

        /// <summary>Her heel (degrees): + with the starboard side lifted (Unity's roll sign).</summary>
        public static float Heel(Quaternion q) => Mathf.Asin(Mathf.Clamp((q * Vector3.right).y, -1f, 1f)) * Mathf.Rad2Deg;

        /// <summary>Her trim (degrees): + bow down (Unity's pitch sign).</summary>
        public static float Trim(Quaternion q) => -Mathf.Asin(Mathf.Clamp((q * Vector3.forward).y, -1f, 1f)) * Mathf.Rad2Deg;
    }
}
