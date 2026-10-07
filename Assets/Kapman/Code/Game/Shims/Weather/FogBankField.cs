using System;
using Kapman.World;
using UnityEngine;

namespace Kapman.Weather
{
    /// <summary>
    /// FOG BANKS, the facts (Plans/environment.md step B3): where the sea fog lies. Banks and tongues a few hundred game
    /// metres across, fixed to the world and drifting with the wind, covering more of the sea as the weather's fog
    /// thickens: none below <see cref="fogFrom"/>, all of it at 1. One field for everything, so the fog you see is the fog
    /// the lookout sees through: Kapman.HDRP.FogBanks draws it (Shaders/SeaFog.shader samples the same noise the same
    /// way), and <see cref="LocalWeather"/> reads how far one can see through it.
    /// </summary>
    [Serializable]
    public class FogBankField
    {
        [Tooltip("How deep a bank lies on the water, game metres: from low wisps a man sees over (this) ...")]
        public float topLow = 1.2f;
        [Tooltip("... to deep banks that swallow a ship, mast and all (this). Each bank's depth differs, and heaves as it drifts " +
                 "(the user, 2026-10-02: the mast should dip in and out of it). The Gislinge's masthead stands 4.7 m up.")]
        public float topHigh = 14f;
        [Tooltip("How much deeper the banks lie as the fog thickens (a shift of the depth noise per unit of strength above 0.6).")]
        public float topDeepens = 0.4f;
        [Tooltip("How fast the banks' tops roll through them: their pattern drifts this many times the banks' own drift.")]
        public float heave = 1.5f;
        [Tooltip("...and at least this fast, game m/s, so even in a calm the tops rise and fall.")]
        public float stillHeave = 1f;
        [Tooltip("The fog inside a bank in thick fog: game metres of it that hide 63% of what's behind (visibility about four times " +
                 "this). Thick, because the layer is thin: seen from above, a bank must still show white over the dark sea.")]
        public float thickMeanFreePath = 20f;
        [Tooltip("...and inside a bank when the fog is light.")]
        public float lightMeanFreePath = 45f;
        [Tooltip("The weather's fog (0-1) where the first banks form; at 1 they cover the sea.")]
        [Range(0f, 0.9f)] public float fogFrom = 0.25f;
        [Tooltip("The banks' size: game metres across a typical bank or gap.")]
        public float bankSize = 450f;
        [Tooltip("Thin mist between the banks, as a share of a bank's fog (times the share of the sea they cover; 0: clear gaps).")]
        [Range(0f, 1f)] public float mist = 0.1f;
        [Tooltip("How fast the banks drift, game m/s for each real m/s of wind (the clock's 60 over the map's 32).")]
        public float drift = 1.9f;
        [Tooltip("To try it: the fog's strength (0-1) whatever the weather says; below 0, the weather's.")]
        [Range(-1f, 1f)] public float forceFog = -1f;

        /// <summary>The noise: 8 banks across its 256 texels, tiling. Red and green are two independent fields of banks, blue how high they stand.</summary>
        public const int NoiseSize = 256, NoiseCells = 8;
        /// <summary>The smaller banks, 2.7 times smaller; the ragged edges, 2.3 times smaller again.</summary>
        public const float SecondSize = 2.7f, ThirdSize = 2.3f;
        /// <summary>Half the width of a bank's edge, in noise levels.</summary>
        public const float EdgeWidth = 0.045f;
        /// <summary>The depth noise is this many times smaller than the banks, so one bank's top rises and falls along it.</summary>
        public const float TopSize = 1.6f;

        Color32[] noise;
        float[] levels;                                                      // the mix's levels, sorted: the edge level that leaves a given share of the sea under banks

        /// <summary>Game metres the banks have drifted (east, north).</summary>
        public Vector2 Drifted { get; private set; }
        /// <summary>Game metres the pattern of their depths has drifted (faster, so the tops roll through the banks).</summary>
        public Vector2 TopDrifted { get; private set; }

        /// <summary>The noise's texels (row by row), made on first use.</summary>
        public Color32[] Noise
        {
            get
            {
                if (noise == null)
                    Make();
                return noise;
            }
        }

        /// <summary>Game metres before the noise repeats.</summary>
        public float Period => NoiseCells * Mathf.Max(1f, bankSize);

        /// <summary>The fog's strength (0-1) in this weather.</summary>
        public float Strength(WeatherSample w) => forceFog >= 0f ? forceFog : Mathf.Clamp01(w.fog);

        /// <summary>The share of the sea under banks at a fog strength.</summary>
        public float Cover(float strength) => Mathf.Pow(Mathf.Clamp01((strength - fogFrom) / (1f - fogFrom)), 1.3f);

        /// <summary>Game metres of a bank's fog that hide 63% of what's behind, at a fog strength.</summary>
        public float MeanFreePath(float strength) => Mathf.Max(0.1f, Mathf.Lerp(lightMeanFreePath, thickMeanFreePath, Mathf.SmoothStep(0f, 1f, strength)));

        /// <summary>The noise level a bank's edge stands at to leave this share of the sea under banks.</summary>
        public float EdgeLevel(float cover)
        {
            if (cover <= 0f)
                return 2f;                                                   // (above any level: no banks)
            if (cover >= 1f)
                return -1f;
            if (levels == null)
                Make();
            float at = (1f - cover) * (levels.Length - 1);
            int i = Mathf.FloorToInt(at);
            return Mathf.Lerp(levels[i], levels[Mathf.Min(i + 1, levels.Length - 1)], at - i);
        }

        /// <summary>The banks drift with the wind (the way it blows towards) for this many game seconds.</summary>
        public void Advance(WeatherSample w, float gameSeconds)
        {
            float rad = (w.windFrom + 180f) * Mathf.Deg2Rad;
            float metres = w.windSpeed * drift * gameSeconds / 60f;           // (drift is per real second at the clock's 60)
            var way = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            Drifted += way * metres;
            TopDrifted += way * Mathf.Max(metres * heave, stillHeave * gameSeconds / 60f);
        }

        /// <summary>Puts the banks back where they started (a new game, a load).</summary>
        public void ResetDrift()
        {
            Drifted = Vector2.zero;
            TopDrifted = Vector2.zero;
        }

        /// <summary>The shift of the depth noise at a fog strength (deeper banks in thicker fog).</summary>
        public float TopBias(float strength) => topDeepens * (strength - 0.6f);

        /// <summary>How deep the bank lies at a place, game metres above the sea (the shader's top, before its soft edge).</summary>
        public float TopAt(WorldPos p, float strength)
        {
            double hs = FloatingOrigin.HorizontalScale;
            float period = Period / TopSize;
            float u = (float)Repeat(p.x / hs - TopDrifted.x, period) / period, v = (float)Repeat(p.z / hs - TopDrifted.y, period) / period;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.8f, Bilinear(Noise, u, v, 2) + TopBias(strength)));
            return Mathf.Lerp(topLow, topHigh, s * s);                        // (squared: most banks low, a few deep)
        }

        /// <summary>How much of a bank's fog lies at a place (0-1), at the water: the shader's density, less its height.</summary>
        public float BankAt(WorldPos p, float strength)
        {
            float cover = Cover(strength);
            if (cover <= 0f)
                return 0f;
            double hs = FloatingOrigin.HorizontalScale;
            float period = Period;
            float u = (float)Repeat(p.x / hs - Drifted.x, period) / period, v = (float)Repeat(p.z / hs - Drifted.y, period) / period;
            float n = Mix(Noise, u, v);
            float edge = EdgeLevel(cover);
            float bank = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge - EdgeWidth, edge + EdgeWidth, n));
            return Mathf.Max(bank, mist * cover);
        }

        /// <summary>Fog per real metre at a place (the bank's; the haze is apart).</summary>
        public double ExtinctionAt(WorldPos p, float strength) => BankAt(p, strength) / (MeanFreePath(strength) * FloatingOrigin.HorizontalScale);

        /// <summary>The haze's visibility in real km (rain and the air, without the fog banks).</summary>
        public static double HazeKm(WeatherSample w) => 40.0 / (1.0 + 0.35 * w.rain);

        /// <summary>The visibility in real km at a place: the haze and the bank there.</summary>
        public double VisibilityKmAt(WeatherSample w, WorldPos p)
        {
            double sigma = 3.912 / (HazeKm(w) * 1000.0) + ExtinctionAt(p, Strength(w));
            return 3.912 / sigma / 1000.0;
        }

        /// <summary>The visibility in real km looking from one place to another: the fog along the line between them (a
        /// lookout in a clear gap still loses a sail behind a bank).</summary>
        public double VisibilityKmBetween(WeatherSample w, WorldPos from, WorldPos to)
        {
            double haze = 3.912 / (HazeKm(w) * 1000.0);
            double dx = to.x - from.x, dz = to.z - from.z, d = Math.Sqrt(dx * dx + dz * dz);
            float strength = Strength(w);
            if (d < 1.0 || Cover(strength) <= 0f)
                return HazeKm(w);
            const int Steps = 24;
            double od = 0.0;
            for (int i = 0; i < Steps; i++)
            {
                double t = (i + 0.5) / Steps;
                od += ExtinctionAt(new WorldPos(from.x + dx * t, 0.0, from.z + dz * t), strength) * d / Steps;
            }
            double sigma = haze + od / d;                                     // the line's mean fog per metre
            return 3.912 / sigma / 1000.0;
        }

        /// <summary>The three sizes of noise mixed, at noise coordinates (0-1 over a period), sampled as the GPU does.</summary>
        static float Mix(Color32[] px, float u, float v)
        {
            float r = Bilinear(px, u, v, 0);
            float g = Bilinear(px, u * SecondSize + 0.37f, v * SecondSize + 0.61f, 1);
            float c = Bilinear(px, u * SecondSize * ThirdSize + 0.13f, v * SecondSize * ThirdSize + 0.29f, 0);
            return 0.58f * r + 0.3f * g + 0.12f * c;                          // (SeaFog.shader: the same sum)
        }

        static float Bilinear(Color32[] px, float u, float v, int channel)
        {
            float x = (u - Mathf.Floor(u)) * NoiseSize - 0.5f, y = (v - Mathf.Floor(v)) * NoiseSize - 0.5f;
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            float a = Texel(px, x0, y0, channel), b = Texel(px, x0 + 1, y0, channel);
            float c = Texel(px, x0, y0 + 1, channel), d = Texel(px, x0 + 1, y0 + 1, channel);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        static float Texel(Color32[] px, int x, int y, int channel)
        {
            x = ((x % NoiseSize) + NoiseSize) % NoiseSize;
            y = ((y % NoiseSize) + NoiseSize) % NoiseSize;
            var c = px[y * NoiseSize + x];
            return (channel == 0 ? c.r : channel == 1 ? c.g : c.b) / 255f;
        }

        static double Repeat(double v, double period) => v - Math.Floor(v / period) * period;

        readonly object making = new object();

        void Make()
        {
            lock (making)                                                    // (made once, whoever asks first; never half-made)
            {
                if (noise != null && levels != null)
                    return;
                MakeNow();
            }
        }

        void MakeNow()
        {
            // (built whole, then published: nothing ever sees a half-made table)
            var made = new Color32[NoiseSize * NoiseSize];
            float cell = (float)NoiseSize / NoiseCells;
            for (int y = 0; y < NoiseSize; y++)
                for (int x = 0; x < NoiseSize; x++)
                {
                    float fx = x / cell, fy = y / cell;
                    made[y * NoiseSize + x] = new Color32(ToByte(TilingPerlin(fx, fy, 11)), ToByte(TilingPerlin(fx, fy, 23)), ToByte(TilingPerlin(fx, fy, 37)), 255);
                }
            const int Look = 128;
            var sorted = new float[Look * Look];
            for (int y = 0; y < Look; y++)
                for (int x = 0; x < Look; x++)
                    sorted[y * Look + x] = Mix(made, (x + 0.5f) / Look, (y + 0.5f) / Look);
            Array.Sort(sorted);
            noise = made;
            levels = sorted;
        }

        static byte ToByte(float v) => (byte)Mathf.Clamp(Mathf.RoundToInt((0.5f + v) * 255f), 0, 255);

        /// <summary>Gradient noise (about -0.7..0.7) that repeats every <see cref="NoiseCells"/> cells.</summary>
        static float TilingPerlin(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            float u = fx * fx * fx * (fx * (fx * 6f - 15f) + 10f), v = fy * fy * fy * (fy * (fy * 6f - 15f) + 10f);
            float g00 = Grad(x0, y0, seed, fx, fy), g10 = Grad(x0 + 1, y0, seed, fx - 1f, fy);
            float g01 = Grad(x0, y0 + 1, seed, fx, fy - 1f), g11 = Grad(x0 + 1, y0 + 1, seed, fx - 1f, fy - 1f);
            return Mathf.Lerp(Mathf.Lerp(g00, g10, u), Mathf.Lerp(g01, g11, u), v);
        }

        static float Grad(int ix, int iy, int seed, float dx, float dy)
        {
            ix = ((ix % NoiseCells) + NoiseCells) % NoiseCells;
            iy = ((iy % NoiseCells) + NoiseCells) % NoiseCells;
            uint h = (uint)(ix * 73856093 ^ iy * 19349663 ^ seed * 83492791);
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;
            float a = (h & 0xffff) / 65536f * 2f * Mathf.PI;
            return Mathf.Cos(a) * dx + Mathf.Sin(a) * dy;
        }
    }
}
