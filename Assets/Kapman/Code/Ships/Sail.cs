using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE SQUARE SAIL (ship physics 2.5): one sail on a yard the crew braces. The force is
    /// 0.5 rho_air A |V_a|^2 (C_L l + C_D d): d along the apparent wind, l across it in the horizontal plane, on the side
    /// that drives her ahead. The crew trims the yard to the best angle of attack the point of sail allows: none with the
    /// apparent wind within about 42 degrees of the bow (the sail shakes; with her way on that is about her closest point
    /// to the true wind, 58), a thin drive braced hard round just outside it, rising across a reach, square to the wind
    /// when running (drag only). The force acts at the sail's centre of effort above the waterline,
    /// which heels her. The C_L and C_D curve is the design's (a loose-footed woollen square sail, flattened; a guess the
    /// tests fit to the replica trials: the Gislinge at 4.8 kn on a beam reach in 8 m/s).
    /// </summary>
    public static class Sail
    {
        public const float RhoAir = 1.225f;
        static readonly float[] AlphaDeg = { 0f, 10f, 20f, 30f, 40f, 50f, 60f, 90f };
        static readonly float[] Lift = { 0f, 0.5f, 0.95f, 1.2f, 1.15f, 1.0f, 0.8f, 0f };
        static readonly float[] Drag = { 0.1f, 0.15f, 0.3f, 0.5f, 0.65f, 0.85f, 1.0f, 1.3f };
        /// <summary>The wind at the sail's height against the 10 m wind (a log profile over open water, z0 = 0.2 mm).</summary>
        public const float HeightFactor = 0.9f;

        public static void Coefficients(float alphaDeg, out float cl, out float cd)
        {
            float a = Mathf.Clamp(alphaDeg, 0f, 90f);
            int i = 0;
            while (i < AlphaDeg.Length - 2 && a > AlphaDeg[i + 1])
                i++;
            float t = Mathf.InverseLerp(AlphaDeg[i], AlphaDeg[i + 1], a);
            cl = Mathf.Lerp(Lift[i], Lift[i + 1], t);
            cd = Mathf.Lerp(Drag[i], Drag[i + 1], t);
        }

        /// <summary>How far inside the closest point the apparent wind may come before the sail shakes (degrees), and where
        /// the hard-braced yard begins to draw fully (degrees inside the closest point).</summary>
        public const float PinchDeg = 16f, BracedDeg = 10f;

        /// <summary>
        /// The angle of attack the crew can give the sail with the apparent wind this many degrees off the bow (0 = dead
        /// ahead, 180 = dead astern). closestDeg is her closest point to the TRUE wind (the class's); at her close-hauled
        /// speed the apparent wind stands some 10-15 degrees further forward than the true wind, so the yard braced hard
        /// round still draws a little inside closestDeg on the apparent wind: nothing more than 16 degrees inside it (the
        /// sail shakes), 0 rising to 10 degrees over the next 6, 10 degrees on to the closest point, then as before: the
        /// best (about 30) on a beam reach, square to the wind (90) on a run.
        /// </summary>
        public static float Trim(float windOffBowDeg, float closestDeg)
        {
            float b = Mathf.Abs(windOffBowDeg);
            if (b < closestDeg - PinchDeg)
                return 0f;                                              // the sail shakes: no drive
            if (b < closestDeg - BracedDeg)
                return Mathf.Lerp(0f, 10f, Mathf.InverseLerp(closestDeg - PinchDeg, closestDeg - BracedDeg, b));
            if (b < closestDeg)
                return 10f;                                             // braced hard round: a thin, flat drive
            if (b < 90f)
                return Mathf.Lerp(10f, 30f, Mathf.InverseLerp(closestDeg, 90f, b));
            return Mathf.Lerp(30f, 90f, Mathf.InverseLerp(90f, 165f, b));
        }

        /// <summary>
        /// The sail's force on her (world, newtons) for the true wind at 10 m (world, m/s, the way the air moves), her
        /// velocity over the water and her rotation; area the set sail (m2: reefs taken off). Out: the apparent wind's
        /// angle off the bow and the angle of attack used.
        /// </summary>
        public static Vector3 Force(Vector3 trueWind10, Vector3 velocity, Quaternion rotation, float area, float closestDeg,
                                    out float windOffBowDeg, out float alphaDeg)
        {
            Vector3 air = trueWind10 * HeightFactor - velocity;       // the apparent wind: where the air moves, relative to her
            air.y = 0f;
            float speed = air.magnitude;
            windOffBowDeg = 0f;
            alphaDeg = 0f;
            if (speed < 0.05f || area <= 0f)
                return Vector3.zero;
            Vector3 fwd = rotation * Vector3.forward;
            fwd.y = 0f;
            fwd.Normalize();
            Vector3 d = air / speed;                                  // drag: with the air
            // the wind comes FROM -d: its angle off the bow
            windOffBowDeg = Vector3.Angle(fwd, -d);
            alphaDeg = Trim(windOffBowDeg, closestDeg);
            if (alphaDeg <= 0f)
                return Vector3.zero;
            Coefficients(alphaDeg, out float cl, out float cd);
            Vector3 l = Vector3.Cross(Vector3.up, d);                 // across the air, horizontal
            if (Vector3.Dot(l, fwd) < 0f)
                l = -l;                                               // the side that drives her ahead
            float q = 0.5f * RhoAir * area * speed * speed;
            return q * (cl * l + cd * d);
        }
    }
}
