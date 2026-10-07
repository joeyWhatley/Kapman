using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE WATER A HULL FEELS (ship physics 4, HullForces' interface): its height and the lean of its surface under a
    /// point, the stream at a point, and a stamp that changes whenever the water itself has moved on (each frame for
    /// HDRP's simulated sea, each Advance for the test seas), so the hull can tell the water's own rise and fall from her
    /// passing over it. In the scene it is HDRP's water (HdrpWaterField); in the tests a flat or a sine sea.
    /// </summary>
    public interface IWaterField
    {
        /// <summary>The water's height (m) and its surface normal under a point.</summary>
        void Sample(Vector3 p, out float height, out Vector3 normal);

        /// <summary>The stream at a point (m/s, over the ground): zero at sea and wherever the stream does not push ships.</summary>
        Vector3 Current(Vector3 p);

        /// <summary>Changes whenever the water has moved on in time.</summary>
        int Stamp { get; }
    }

    /// <summary>Still water at one level, with an optional even stream (the tests' harbour).</summary>
    public sealed class FlatWater : IWaterField
    {
        public float level;
        public Vector3 current;

        public FlatWater(float level = 0f)
        {
            this.level = level;
        }

        public void Sample(Vector3 p, out float height, out Vector3 normal)
        {
            height = level;
            normal = Vector3.up;
        }

        public Vector3 Current(Vector3 p) => current;

        public int Stamp => 0;
    }

    /// <summary>
    /// One long-crested deep-water wave train (the tests' swell): height a cos(k x - w t) along the way it runs, with
    /// w = sqrt(g k) (deep-water dispersion) and the surface's true normal. Advance moves it on in time.
    /// </summary>
    public sealed class SineWater : IWaterField
    {
        public float level, amplitude, wavelength;
        /// <summary>The way the waves run (degrees from north).</summary>
        public float towards;
        public float time;
        int stamp;

        public SineWater(float amplitude, float wavelength, float towards, float level = 0f)
        {
            this.amplitude = amplitude;
            this.wavelength = wavelength;
            this.towards = towards;
            this.level = level;
        }

        /// <summary>The wave's speed over the water (m/s): c = sqrt(g L / 2 pi).</summary>
        public float Celerity => Mathf.Sqrt(Hydrostatics.G * wavelength / (2f * Mathf.PI));

        public void Advance(float dt)
        {
            time += dt;
            stamp++;
        }

        public void Sample(Vector3 p, out float height, out Vector3 normal)
        {
            float k = 2f * Mathf.PI / wavelength, w = Mathf.Sqrt(Hydrostatics.G * k);
            float r = towards * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            float phase = k * (dir.x * p.x + dir.y * p.z) - w * time;
            height = level + amplitude * Mathf.Cos(phase);
            float slope = -amplitude * k * Mathf.Sin(phase);              // dh along the way the waves run
            normal = new Vector3(-slope * dir.x, 1f, -slope * dir.y).normalized;
        }

        public Vector3 Current(Vector3 p) => Vector3.zero;

        public int Stamp => stamp;
    }
}
