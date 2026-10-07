using Kapman.Levels.Water;
using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE WAVES (THE FALLBACK) - four sine waves, lifted by the wind and stilled by ice, for ships that are NOT on an
    /// HDRP water surface: a level without one yet, or the first frames before HDRP's CPU simulation answers. With an
    /// HDRP surface in the level the ships ride its real waves instead (WaterSampler), and these only keep ticking.
    /// The waves keep moving while the game is paused; at 2x and 4x they run faster with everything else. The shader
    /// globals are still set for any material that wants them.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-130)]
    public class WaterWaves : MonoBehaviour
    {
        [Tooltip("Up to four waves: direction (x, z), height (m, half the crest-to-trough), length (m).")]
        public Vector4[] waves =
        {
            new Vector4(1f, 0.35f, 0.035f, 7f),
            new Vector4(0.6f, -0.8f, 0.025f, 4.3f),
            new Vector4(-0.3f, 1f, 0.015f, 2.6f),
            new Vector4(0.9f, 0.5f, 0.01f, 1.7f),
        };
        [Tooltip("How much each m/s of wind over 3 m/s lifts the waves.")]
        public float windLift = 0.08f;

        static readonly Vector4[] Active = new Vector4[4];
        static readonly int WavesId = Shader.PropertyToID("_KapmanWaves");
        static readonly int TimeId = Shader.PropertyToID("_KapmanWaveTime");

        /// <summary>The waves' clock (s).</summary>
        public static float WaveTime { get; private set; }

        /// <summary>True while ships ride an HDRP surface instead of these waves.</summary>
        public static bool Hdrp => WaterSampler.Live;

        void Update()
        {
            var w = LevelWeather.Current;
            float wind = w != null ? w.Wind : 3f;
            float ice = w != null ? w.Ice : 0f;
            float gain = Mathf.Max(0.3f, 1f + (wind - 3f) * windLift) * (1f - ice);
            for (int i = 0; i < 4; i++)
            {
                var v = i < waves.Length ? waves[i] : Vector4.zero;
                Active[i] = new Vector4(v.x, v.y, v.z * gain, v.w);
            }
            if (Application.isPlaying)
                WaveTime += Time.deltaTime * (LevelClock.Paused ? 1f : LevelClock.Speed);
            Shader.SetGlobalVectorArray(WavesId, Active);
            Shader.SetGlobalFloat(TimeId, WaveTime);
        }

        /// <summary>The water's rise over its level at a point (m), by the sine set.</summary>
        public static float Height(float x, float z)
        {
            float h = 0f;
            for (int k = 0; k < 4; k++)
            {
                var w = Active[k];
                if (w.z <= 0f)
                    continue;
                var dir = new Vector2(w.x, w.y).normalized;
                float kk = 2f * Mathf.PI / Mathf.Max(w.w, 0.1f);
                float om = Mathf.Sqrt(9.81f * kk);
                h += w.z * Mathf.Sin(kk * (dir.x * x + dir.y * z) - om * WaveTime);
            }
            return h;
        }

        public static float Height(Vector3 p) => Height(p.x, p.z);
    }
}
