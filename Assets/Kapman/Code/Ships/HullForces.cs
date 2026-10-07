using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// WHAT THE WATER DOES TO HER HULL (ship physics 2.2 and 2.4), each step, from the water under each probe cell:
    ///  - buoyancy: rho g times the cell's immersed volume, along the water's surface normal there (not straight up: a
    ///    wave's slope pushes a small hull sideways, which is the real wave force on her), at the immersed volume's centre;
    ///  - heave damping per cell: -(b1 + b2 |w|) w, w the cell's vertical speed against the water's own rise and fall,
    ///    b1 from the class's damping ratio (shared by footprint), b2 a plate drag;
    ///  - resistance ahead: 0.5 rho S v^2 (C_f + C_r), S her wetted surface now (Denny-Mumford 1.7 L T + V / T), C_f the
    ///    ITTC 1957 line, C_r the class's table against the Froude number; v through the water (the stream taken off);
    ///  - lateral resistance and yaw damping together: the hull's side area in five (or seven) stations, each feeling
    ///    0.5 rho C_lat A |v| v of its own sideways speed (her drift plus her turning), so a turn is damped as the
    ///    design's -c_y w|w| with c_y from the side area and length; and, with way on, the hull's own lift against
    ///    drift, 0.5 rho A (pi/2 AR) |u| v (night two: see <see cref="LateralLiftSlope"/>).
    ///  - shallow water (night 3): C_r times <see cref="Shallow"/>, which the grounding sets from the water under her
    ///    keel (squat and the shallow-water hump: she slows over a bank before she touches).
    /// Ice comes with the weather's ice. One instance per ship: it keeps each cell's last water height to know how fast
    /// the water itself rises, and the height it found this step (<see cref="HeightAt"/>, for the bilge).
    /// </summary>
    public sealed class HullForces
    {
        /// <summary>Sea water's kinematic viscosity (m2/s), for the Reynolds number.</summary>
        public const float Nu = 1.19e-6f;

        readonly HullProbes hull;
        readonly ShipClass cls;
        readonly float[] b1, b2, waterAt, waterRate, heightNow;
        int stamp;
        float sinceStamp;
        bool primed;

        /// <summary>After each Compute: the volume under water (m3), the draught amidships (m), the wetted surface (m2),
        /// her speed through the water ahead and sideways (m/s; + to starboard), the resistance ahead (N).</summary>
        public float Immersed { get; private set; }
        public float MeanDraught { get; private set; }
        public float WettedSurface { get; private set; }
        public float AheadSpeed { get; private set; }
        public float SideSpeed { get; private set; }
        public float Resistance { get; private set; }

        /// <summary>The shallow-water factor on the residuary resistance (1 in deep water; ship physics 2.4): set by the
        /// grounding from the water under her keel.</summary>
        public float Shallow { get; set; } = 1f;

        /// <summary>The water's height over a cell's keel point found in the last Compute (m, world).</summary>
        public float HeightAt(int cell) => heightNow[cell];

        public HullForces(HullProbes hull, ShipClass cls, float mass)
        {
            this.hull = hull;
            this.cls = cls;
            int n = hull.cells.Length;
            b1 = new float[n];
            b2 = new float[n];
            waterAt = new float[n];
            waterRate = new float[n];
            heightNow = new float[n];
            // the heave damping: the class's share of critical damping at her laden draught, shared out by footprint
            float d = Hydrostatics.DraughtFor(hull, mass);
            float awp = Hydrostatics.WaterplaneArea(hull, d);
            float critical = 2f * Mathf.Sqrt(Hydrostatics.Rho * Hydrostatics.G * awp * mass * (1f + cls.addHeave));
            for (int i = 0; i < n; i++)
            {
                float share = hull.cells[i].area / Mathf.Max(1e-6f, SumArea(hull));
                b1[i] = cls.heaveDampingRatio * critical * share;
                b2[i] = 0.5f * Hydrostatics.Rho * cls.heaveDragCd * hull.cells[i].area;
            }
        }

        static float SumArea(HullProbes hull)
        {
            float a = 0f;
            foreach (var c in hull.cells)
                a += c.area;
            return a;
        }

        /// <summary>Forgets the water's past (after she is put somewhere new): its rise and fall is counted afresh.</summary>
        public void Reset() => primed = false;

        /// <summary>The water's force on her (world, through her centre of gravity) and its torque about that centre.</summary>
        public void Compute(in ShipState s, in MassProps m, IWaterField water, float dt, out Vector3 force, out Vector3 torque)
        {
            force = Vector3.zero;
            torque = Vector3.zero;
            Quaternion r = s.rotation;
            Vector3 up = r * Vector3.up;
            sinceStamp += dt;
            bool fresh = primed && water.Stamp != stamp && sinceStamp > 0f;

            float immersed = 0f, centreDepth = 0f;
            int centres = 0;
            var cells = hull.cells;
            for (int i = 0; i < cells.Length; i++)
            {
                var c = cells[i];
                Vector3 p = s.position + r * (c.keel - m.cg);
                water.Sample(p, out float h, out Vector3 n);
                heightNow[i] = h;
                if (!primed)
                {
                    waterAt[i] = h;
                    waterRate[i] = 0f;
                }
                else if (fresh)
                {
                    waterRate[i] = (h - waterAt[i]) / sinceStamp;
                    waterAt[i] = h;
                }
                float d = h - p.y;
                if (c.centre)
                {
                    centreDepth += Mathf.Clamp(d, 0f, c.depth);
                    centres++;
                }
                float v = c.Volume(d);
                if (v <= 0f)
                    continue;
                immersed += v;
                Vector3 at = p + up * c.Centroid(d);
                Vector3 f = Hydrostatics.Rho * Hydrostatics.G * v * n;
                float w = s.PointVelocity(at).y - waterRate[i];
                f.y -= (b1[i] + b2[i] * Mathf.Abs(w)) * w;
                force += f;
                torque += Vector3.Cross(at - s.position, f);
            }
            if (!primed || fresh)
            {
                stamp = water.Stamp;
                sinceStamp = 0f;
            }
            primed = true;
            Immersed = immersed;
            float t = centres > 0 ? centreDepth / centres : 0f;
            MeanDraught = t;
            WettedSurface = t > 0.005f ? 1.7f * hull.lwl * t + immersed / t : 0f;

            // through the water: her velocity with the stream taken off, on her own axes
            Vector3 through = Rot.Inverse(r) * (s.velocity - water.Current(s.position));
            float u = through.z, side = through.x;
            AheadSpeed = u;
            SideSpeed = side;
            float hullMid = hull.keelY + 0.5f * t;                        // the lateral plane's centre, in the model's frame

            // resistance ahead
            float speed = Mathf.Abs(u);
            float re = Mathf.Max(speed * hull.lwl / Nu, 1e5f);
            float lg = Mathf.Log10(re) - 2f;
            float cf = 0.075f / (lg * lg);
            float fn = speed / Mathf.Sqrt(Hydrostatics.G * hull.lwl);
            float drag = 0.5f * Hydrostatics.Rho * WettedSurface * u * speed * (cf + cls.Cr(fn) * Shallow);
            Resistance = drag;
            if (drag != 0f)
                Apply(s, m, new Vector3(0f, hullMid, m.cg.z), r * new Vector3(0f, 0f, -drag), ref force, ref torque);

            // lateral resistance and yaw damping, station by station
            float wet = Mathf.Clamp01(t / hull.draught);
            float stationArea = cls.lateralArea * wet / hull.stationZ.Length;
            float yawRate = s.angularVelocity.y;
            float lift = LateralLiftSlope(hull, cls) * Mathf.Abs(u);
            foreach (float z in hull.stationZ)
            {
                float vs = side + yawRate * (z - m.cg.z);
                float fs = -0.5f * Hydrostatics.Rho * stationArea * (cls.lateralCd * Mathf.Abs(vs) + lift) * vs;
                if (fs != 0f)
                    Apply(s, m, new Vector3(0f, hullMid, z), r * new Vector3(fs, 0f, 0f), ref force, ref torque);
            }
        }

        /// <summary>
        /// The hull's lift against drift, per radian (night two): moving ahead with a drift angle beta, her underwater body
        /// is a very low wing whose lift is (pi / 2) AR beta (slender-body theory) on her side area, AR = 2 T^2 / A_lat (her
        /// draught as the span, doubled by the water's surface, over the side area, doubled likewise). It is linear in the
        /// drift where the cross-flow drag is quadratic, so it is what holds her at small drift angles: close-hauled it
        /// takes her leeway from about 16 to about 12 degrees (the design expects 8-12).
        /// </summary>
        public static float LateralLiftSlope(HullProbes hull, ShipClass cls) =>
            cls.lateralArea > 0f ? 0.5f * Mathf.PI * 2f * hull.draught * hull.draught / cls.lateralArea : 0f;

        /// <summary>Adds a force (world) acting at a point of the model's frame.</summary>
        static void Apply(in ShipState s, in MassProps m, Vector3 local, Vector3 f, ref Vector3 force, ref Vector3 torque)
        {
            force += f;
            torque += Vector3.Cross(s.rotation * (local - m.cg), f);
        }
    }
}
