using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// THE WATER UNDER A POINT - one place to ask how high the water stands somewhere, and which way its surface leans.
    /// With an HDRP water surface in the level the answer comes from HDRP's own CPU copy of the waves (the surface's
    /// "script interactions", switched on by HdrpWater.EnableWater): the ship then rides exactly the sea you see. Without
    /// one (no HDRP surface yet, or the simulation not ready in its first frames) it falls back to the level's resting
    /// water level plus WaterWaves' sine set, so nothing ever sinks or floats in the air.
    /// </summary>
    public static class WaterSampler
    {
        static WaterSurface surface;
        static int lookedFrame = -1;

        /// <summary>The HDRP water surface ships ride here (the sea's HdrpSea first, else any surface in the scene).</summary>
        public static WaterSurface Surface
        {
            get
            {
                if (surface == null && Time.frameCount != lookedFrame)
                {
                    lookedFrame = Time.frameCount;
                    surface = HdrpSea.Current != null ? HdrpSea.Current.Surface : Object.FindAnyObjectByType<WaterSurface>();
                }
                return surface;
            }
        }

        /// <summary>True while the HDRP surface answers (its CPU simulation is running): heights are the real waves.</summary>
        public static bool Live { get; private set; }

        /// <summary>
        /// The water's height and surface normal at a point. Returns true when HDRP answered; false when the sine
        /// fallback was used (the height is still filled in). feelDecals: include the ships' own wakes and the shore's
        /// surf (off for a ship riding her own wake, or she'd climb her bow wave).
        /// </summary>
        public static bool Sample(Vector3 p, out float height, out Vector3 normal, bool feelDecals = false, int iterations = 8)
        {
            var s = Surface;
            normal = Vector3.up;
            if (s != null && s.scriptInteractions && s.isActiveAndEnabled)
            {
                var search = new WaterSearchParameters
                {
                    targetPositionWS = p,
                    startPositionWS = p,
                    error = 0.003f,                                          // tight: a loose search jitters frame to frame
                    maxIterations = iterations,
                    includeDeformation = feelDecals,
                    excludeSimulation = false,
                    outputNormal = true,
                };
                if (s.ProjectPointOnWaterSurface(search, out var r))
                {
                    Live = true;
                    Vector3 hit = r.projectedPositionWS;
                    height = hit.y;
                    normal = r.normalWS;
                    if (normal.sqrMagnitude < 0.5f)
                        normal = Vector3.up;
                    return true;
                }
            }
            Live = false;
            var info = LevelInfo.Current;
            height = (info != null ? info.WaterLevel : 0f) + WaterWaves.Height(p);
            return false;
        }

        /// <summary>The water's height at a point (m).</summary>
        public static float Height(Vector3 p)
        {
            Sample(p, out float h, out _);
            return h;
        }

        /// <summary>Forgets the cached surface (after a scene change).</summary>
        public static void Forget()
        {
            surface = null;
            Live = false;
        }
    }
}
