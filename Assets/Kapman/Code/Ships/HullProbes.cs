using System;
using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// ONE PROBE CELL of the hull: a slice of her, a station long and a third of her breadth wide, that knows how much
    /// hull lies below the water when the water stands d metres above its keel point (ship physics 2.2). Below the
    /// design waterline the slice fills as d^q (q = 1 a flat-bottomed box, 1.5 a round bilge); above it the sides run
    /// straight up to the gunwale; past the gunwale nothing more is added (the water then pours in: the bilge, night 3).
    /// </summary>
    public struct HullCell
    {
        /// <summary>The cell's keel point in the model's frame (x starboard, y up, z ahead; the model's origin).</summary>
        public Vector3 keel;
        /// <summary>The hull's volume in this cell below the design waterline (m3), and its footprint there (m2).</summary>
        public float volumeAtDraught, area;
        /// <summary>The design draught and the hull's depth to the gunwale (m), and the section's shape exponent.</summary>
        public float draught, depth, q;
        /// <summary>The station (0 at the stern) and whether this is the centre cell of its station (or a side one).</summary>
        public int station;
        public bool centre;

        /// <summary>The hull's volume below water standing d metres above the keel point (m3).</summary>
        public float Volume(float d)
        {
            if (d <= 0f)
                return 0f;
            if (d > depth)
                d = depth;
            if (d <= draught)
                return volumeAtDraught * Mathf.Pow(d / draught, q);
            return volumeAtDraught + area * (d - draught);
        }

        /// <summary>The waterplane this cell cuts at d (m2): how fast its volume grows as she sinks.</summary>
        public float Area(float d)
        {
            if (d <= 0f || d >= depth)
                return 0f;
            if (d <= draught)
                return q * volumeAtDraught / draught * Mathf.Pow(d / draught, q - 1f);
            return area;
        }

        /// <summary>The height of the immersed volume's centre above the keel point (m).</summary>
        public float Centroid(float d)
        {
            if (d <= 0f)
                return 0f;
            if (d > depth)
                d = depth;
            float k = q / (q + 1f);
            if (d <= draught)
                return k * d;
            float side = area * (d - draught);
            return (volumeAtDraught * k * draught + side * 0.5f * (draught + d)) / (volumeAtDraught + side);
        }
    }

    /// <summary>
    /// THE HULL AS PROBE CELLS (ship physics 2.2, HullProbes.Parametric): 5 stations along the waterline by 3 across
    /// (port, centre, starboard) for small boats, 7 by 3 over 12 m, made from her length on the waterline, beam, depth,
    /// draught, block coefficient Cb and section shape q - no mesh. The section area along her runs as
    /// A(x) = A_mid (1 - |2x/L|^p), with p chosen so the hull holds exactly Cb L B T, and each section fills from the keel
    /// as (d/T)^q, so its width at the waterline is B there and its midship coefficient is 1/q. The side cells sit
    /// 0.354 of the station's breadth out (not a third): there the three cells' waterplane has the true second moment
    /// b^3/12 of the strip, so the heel the cells feel is the heel the metacentre gives.
    /// </summary>
    public sealed class HullProbes
    {
        public readonly HullCell[] cells;
        public readonly float lwl, beam, depth, draught, cb, q;
        /// <summary>The keel's height in the model's frame (m): below the origin by the waterline offset and the draught.</summary>
        public readonly float keelY;
        /// <summary>Each station's middle along her (z, model frame) and the stations' length (m).</summary>
        public readonly float[] stationZ;
        public readonly float stationLength;

        /// <summary>The side cells' distance out, as a share of their station's waterline breadth (1 / (2 sqrt 2)).</summary>
        public const float SideOut = 0.35355339f;

        HullProbes(HullCell[] cells, float lwl, float beam, float depth, float draught, float cb, float q, float keelY, float[] stationZ, float stationLength)
        {
            this.cells = cells;
            this.lwl = lwl;
            this.beam = beam;
            this.depth = depth;
            this.draught = draught;
            this.cb = cb;
            this.q = q;
            this.keelY = keelY;
            this.stationZ = stationZ;
            this.stationLength = stationLength;
        }

        /// <summary>The cells for a ship class: 5 stations, 7 for a hull over 12 m. The model's origin sits
        /// <paramref name="waterlineOffset"/> above her design waterline (ShipMotion.waterline).</summary>
        public static HullProbes ForClass(ShipClass c, float waterlineOffset) =>
            Parametric(c.lwl, c.beam, c.depth, c.draught, c.cb, c.sectionQ, -waterlineOffset - c.draught, c.lwl > 12f ? 7 : 5);

        /// <summary>
        /// The probe cells from the hull's numbers. Cb x q must not pass 1 (the fullest hull for a section shape is the
        /// one with no taper at all); at 1 the hull is a straight-sided box of that section, end to end.
        /// </summary>
        public static HullProbes Parametric(float lwl, float beam, float depth, float draught, float cb, float q, float keelY, int sections = 5)
        {
            if (lwl <= 0f || beam <= 0f || draught <= 0f || depth <= draught || cb <= 0f || q < 1f || sections < 1)
                throw new ArgumentException($"HullProbes: impossible hull (lwl {lwl}, beam {beam}, depth {depth}, draught {draught}, cb {cb}, q {q})");
            float ratio = cb * q;                                         // Cb over the midship coefficient 1/q: the taper's fullness
            if (ratio > 1.0001f)
                throw new ArgumentException($"HullProbes: cb {cb} is too full for section shape q {q}");
            bool box = ratio >= 0.9999f;
            float p = box ? 0f : ratio / (1f - ratio);                    // L p/(p+1) = Cb q L
            float aMid = beam * draught / q;                              // the midship section's area below the waterline
            float dl = lwl / sections;
            var cells = new HullCell[sections * 3];
            var stationZ = new float[sections];
            for (int s = 0; s < sections; s++)
            {
                float z0 = -0.5f * lwl + s * dl, z1 = z0 + dl;
                float mean = box ? 1f : (Taper(2f * z1 / lwl, p) - Taper(2f * z0 / lwl, p)) / (2f * dl / lwl);
                float zc = box ? 0.5f * (z0 + z1) : Centre(z0, z1, lwl, p);
                stationZ[s] = zc;
                float volume = aMid * mean * dl;                          // the station's hull below the waterline
                float breadth = beam * mean;                              // its waterline breadth (mean over the station)
                float area = breadth * dl;
                for (int k = 0; k < 3; k++)
                {
                    float x = (k - 1) * SideOut * breadth;
                    cells[s * 3 + k] = new HullCell
                    {
                        keel = new Vector3(x, keelY, zc),
                        volumeAtDraught = volume / 3f,
                        area = area / 3f,
                        draught = draught,
                        depth = depth,
                        q = q,
                        station = s,
                        centre = k == 1,
                    };
                }
            }
            return new HullProbes(cells, lwl, beam, depth, draught, cb, q, keelY, stationZ, dl);
        }

        /// <summary>The integral of the taper 1 - |u|^p from 0 to u (u = 2x/L, signed).</summary>
        static float Taper(float u, float p)
        {
            float a = Mathf.Abs(u);
            return Mathf.Sign(u) * (a - Mathf.Pow(a, p + 1f) / (p + 1f));
        }

        /// <summary>The centre of the taper's area between z0 and z1 (by 64 strips: only the pitch moment uses it).</summary>
        static float Centre(float z0, float z1, float lwl, float p)
        {
            const int n = 64;
            double sum = 0.0, moment = 0.0;
            for (int i = 0; i < n; i++)
            {
                float z = z0 + (i + 0.5f) * (z1 - z0) / n;
                float f = 1f - Mathf.Pow(Mathf.Abs(2f * z / lwl), p);
                sum += f;
                moment += f * z;
            }
            return sum > 1e-9 ? (float)(moment / sum) : 0.5f * (z0 + z1);
        }
    }
}
