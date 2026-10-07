using Kapman.Levels.Water;
using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE LEVEL'S WATER FOR A HULL: HDRP's simulated surface through WaterSampler (the sea or the river the player sees;
    /// the sine fallback when there is no surface yet), without the decals (includeDeformation off: she never feels her
    /// own wake, or she would climb her bow wave), and the river's stream from RiverCurrent where it pushes ships
    /// (RiverCurrent.pushesShips, from chain.json). HDRP moves its water once a frame, so the stamp is the frame.
    /// </summary>
    public sealed class HdrpWaterField : IWaterField
    {
        /// <summary>The surface search's iterations (8 for the player's ship, as ShipMotion does).</summary>
        public int iterations = 8;

        int liveFrame = -1, deadFrame = -1;

        /// <summary>True if HDRP's water answered in the latest frame that was sampled (false: the sine fallback).</summary>
        public bool Live => liveFrame >= deadFrame && liveFrame >= 0;

        public void Sample(Vector3 p, out float height, out Vector3 normal)
        {
            if (WaterSampler.Sample(p, out height, out normal, false, iterations))
                liveFrame = Time.frameCount;
            else
                deadFrame = Time.frameCount;
        }

        public Vector3 Current(Vector3 p)
        {
            var river = RiverCurrent.Current;
            return river != null && river.pushesShips ? river.Drift(p) : Vector3.zero;
        }

        public int Stamp => Time.frameCount;
    }
}
