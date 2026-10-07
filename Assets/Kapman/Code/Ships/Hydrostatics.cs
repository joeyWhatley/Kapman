using System;
using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// What she weighs, where it sits and how hard she is to swing (ship physics 2.3): the mass, the centre of gravity in
    /// the model's frame and the inertia on the body's own axes - x (pitch), y (yaw), z (roll).
    /// </summary>
    public struct MassProps
    {
        public float mass;
        public Vector3 cg;
        public Vector3 inertia;
    }

    /// <summary>
    /// THE SHIP AT REST ON STILL WATER (ship physics 2.2-2.3), from the probe cells: the volume she displaces at a draught,
    /// her waterplane, the height of the centre of buoyancy, the metacentric height GM that says how stiffly she stands
    /// up, the draught a mass floats at, and the mass, centre of gravity and inertia of a laden ship. Sea water.
    /// </summary>
    public static class Hydrostatics
    {
        /// <summary>Sea water (kg/m3) and gravity (m/s2).</summary>
        public const float Rho = 1025f, G = 9.81f;

        /// <summary>The volume below the water when she floats upright with the water d metres above her keel (m3).</summary>
        public static float Volume(HullProbes hull, float d)
        {
            float v = 0f;
            foreach (var c in hull.cells)
                v += c.Volume(d);
            return v;
        }

        /// <summary>Her waterplane at draught d (m2).</summary>
        public static float WaterplaneArea(HullProbes hull, float d)
        {
            float a = 0f;
            foreach (var c in hull.cells)
                a += c.Area(d);
            return a;
        }

        /// <summary>The centre of buoyancy's height above the keel at draught d (m).</summary>
        public static float KB(HullProbes hull, float d)
        {
            float v = 0f, m = 0f;
            foreach (var c in hull.cells)
            {
                float cv = c.Volume(d);
                v += cv;
                m += cv * c.Centroid(d);
            }
            return v > 0f ? m / v : 0f;
        }

        /// <summary>The waterplane's second moment about her centreline at draught d (m4): what makes her stand up.</summary>
        public static float TransverseInertia(HullProbes hull, float d)
        {
            float i = 0f;
            foreach (var c in hull.cells)
                i += c.Area(d) * c.keel.x * c.keel.x;
            return i;
        }

        /// <summary>The draught she floats at, upright on still water, with this mass aboard (m; bisection to 0.1 mm).</summary>
        public static float DraughtFor(HullProbes hull, float mass)
        {
            float want = mass / Rho;
            if (want > Volume(hull, hull.depth))
                throw new ArgumentException($"Hydrostatics: {mass:0} kg sinks this hull (she holds {Volume(hull, hull.depth) * Rho:0} kg to the gunwale)");
            float lo = 0f, hi = hull.depth;
            while (hi - lo > 1e-4f)
            {
                float mid = 0.5f * (lo + hi);
                if (Volume(hull, mid) < want)
                    lo = mid;
                else
                    hi = mid;
            }
            return 0.5f * (lo + hi);
        }

        /// <summary>The metacentric height (m) with this mass aboard and its centre of gravity kg metres above the keel:
        /// GM = KB + BM - KG, with BM = I / V.</summary>
        public static float GM(HullProbes hull, float mass, float kg)
        {
            float d = DraughtFor(hull, mass);
            return KB(hull, d) + TransverseInertia(hull, d) / Volume(hull, d) - kg;
        }

        /// <summary>The natural heave period (s): 2 pi sqrt((m + m_a) / (rho g A_wp)).</summary>
        public static float HeavePeriod(HullProbes hull, float mass, float addedHeave) =>
            2f * Mathf.PI * Mathf.Sqrt(mass * (1f + addedHeave) / (Rho * G * WaterplaneArea(hull, DraughtFor(hull, mass))));

        /// <summary>The natural roll period (s): 2 pi k sqrt(1 + added) / sqrt(g GM) - the design's 2 pi k / sqrt(g GM)
        /// with the roll's added inertia in it, as the integrator carries it.</summary>
        public static float RollPeriod(float kRoll, float gm, float addedRoll) =>
            2f * Mathf.PI * kRoll * Mathf.Sqrt(1f + addedRoll) / Mathf.Sqrt(G * gm);

        /// <summary>
        /// The laden ship: hull, crew, gear and cargo at their heights above the keel, all on the centreline amidships
        /// (cargo stowage and trim come with the cargo, later); the inertia as m k^2 on each axis from the class's radii.
        /// </summary>
        public static MassProps Laden(ShipClass c, float keelY)
        {
            float men = c.crew * c.manKg;
            float mass = c.hullMassKg + men + c.gearKg + c.cargoKg;
            float kg = (c.hullMassKg * c.hullCgM + men * c.crewCgM + c.gearKg * c.gearCgM + c.cargoKg * c.cargoCgM) / mass;
            return Of(mass, kg, keelY, c.kRoll, c.kPitch, c.kYaw);
        }

        /// <summary>A mass with its centre kg metres above the keel, amidships, and its radii of gyration.</summary>
        public static MassProps Of(float mass, float kg, float keelY, float kRoll, float kPitch, float kYaw) => new MassProps
        {
            mass = mass,
            cg = new Vector3(0f, keelY + kg, 0f),
            inertia = new Vector3(mass * kPitch * kPitch, mass * kYaw * kYaw, mass * kRoll * kRoll),
        };

        /// <summary>The mass she answers a force with on each body axis, the added mass in (ship physics 2.2):
        /// x sway, y heave, z surge.</summary>
        public static Vector3 MassDiagonal(MassProps m, ShipClass c) =>
            new Vector3(m.mass * (1f + c.addSway), m.mass * (1f + c.addHeave), m.mass * (1f + c.addSurge));

        /// <summary>The inertia she answers a torque with on each body axis, the added inertia in: x pitch, y yaw, z roll.</summary>
        public static Vector3 InertiaDiagonal(MassProps m, ShipClass c) =>
            new Vector3(m.inertia.x * (1f + c.addPitch), m.inertia.y * (1f + c.addYaw), m.inertia.z * (1f + c.addRoll));
    }
}
