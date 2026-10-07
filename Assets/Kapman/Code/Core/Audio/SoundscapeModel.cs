using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Audio
{
    /// <summary>
    /// THE SOUNDSCAPE MODEL (audio design 2, nights one and two): pure numbers, no Unity audio. The Beaufort force, the
    /// speed of sound in the air's temperature and the thunder's delay, the air's absorption by ISO 9613-1 (which sets the
    /// low-pass on far sounds), the equal-power crossfade, and each layer's level in dB from the data's conditions. Night
    /// two adds the humidity from the weather, the wildlife's mask in a gale and in rain, the bending of far sounds by the
    /// wind and by a ground inversion, a voice's level by its source level and distance, the ship's laws (the bow wave by
    /// her speed, the hull working by her roll and pitch), and a guess at the surroundings until the cover map comes.
    /// (SoundContext, what the listener is in, is in SoundContext.cs.)
    /// </summary>
    public static class SoundscapeModel
    {
        static readonly float[] BeaufortFloor = { 0.3f, 1.6f, 3.4f, 5.5f, 8.0f, 10.8f, 13.9f, 17.2f, 20.8f, 24.5f, 28.5f, 32.7f };

        /// <summary>The Beaufort number of a 10 m wind (m/s), by the WMO table's lower limits.</summary>
        public static int Beaufort(float u)
        {
            int b = 0;
            while (b < BeaufortFloor.Length && u >= BeaufortFloor[b])
                b++;
            return b;
        }

        /// <summary>The force as a smooth number (v = 0.836 B^1.5), so beds do not step at the table's limits.</summary>
        public static float Force(float u) => Mathf.Min(12f, Mathf.Pow(Mathf.Max(0f, u) / 0.836f, 2f / 3f));

        /// <summary>The speed of sound in dry air at a temperature (m/s): 331.3 sqrt(1 + T/273.15).</summary>
        public static float SpeedOfSound(float celsius) => 331.3f * Mathf.Sqrt(1f + celsius / 273.15f);

        /// <summary>Seconds from the flash to the thunder at this distance (m).</summary>
        public static float ThunderDelay(float metres, float celsius) => metres / SpeedOfSound(celsius);

        public enum Thunder { Near, Far, Silent }

        /// <summary>A crack within 1.5 km, a rumble beyond, nothing heard past 16 km (design 2.5).</summary>
        public static Thunder ThunderKind(float metres) => metres < 1500f ? Thunder.Near : metres < 16000f ? Thunder.Far : Thunder.Silent;

        /// <summary>The thunder's level (dB on the clip): a near crack -3 dB at 300 m, a far rumble -6 dB at 1.5 km (the far
        /// clips are distant recordings already), each falling 6 dB a doubling of distance.</summary>
        public static float ThunderDb(float metres)
        {
            if (ThunderKind(metres) == Thunder.Near)
                return -3f - 20f * Mathf.Log10(Mathf.Max(metres, 300f) / 300f);
            return -6f - 20f * Mathf.Log10(Mathf.Max(metres, 1500f) / 1500f);
        }

        /// <summary>
        /// The air's absorption at a frequency (dB per metre), ISO 9613-1 at sea-level pressure: the classical term and the
        /// oxygen and nitrogen relaxations, which hang on the humidity.
        /// </summary>
        public static double AirAbsorption(double hz, double celsius, double humidity)
        {
            double T = celsius + 273.15, T0 = 293.15, T01 = 273.16;
            double c = -6.8346 * Math.Pow(T01 / T, 1.261) + 4.6151;
            double h = Math.Max(0.0, humidity) * 100.0 * Math.Pow(10.0, c);              // molar concentration of water vapour, %
            double frO = 24.0 + 4.04e4 * h * (0.02 + h) / (0.391 + h);
            double frN = Math.Pow(T / T0, -0.5) * (9.0 + 280.0 * h * Math.Exp(-4.170 * (Math.Pow(T / T0, -1.0 / 3.0) - 1.0)));
            double f2 = hz * hz;
            return 8.686 * f2 * (1.84e-11 * Math.Pow(T / T0, 0.5)
                + Math.Pow(T / T0, -2.5) * (0.01275 * Math.Exp(-2239.1 / T) / (frO + f2 / frO)
                                           + 0.1068 * Math.Exp(-3352.0 / T) / (frN + f2 / frN)));
        }

        /// <summary>The air's loss at the low-pass's corner (dB): where the absorption over the path reaches this.</summary>
        public const float CutoffLossDb = 10f;

        /// <summary>The low-pass corner (Hz) for a sound this far away: the frequency the air takes 10 dB from over the
        /// path; 22 kHz (open) when it takes less from everything heard.</summary>
        public static float AirAbsorptionCutoff(float metres, float celsius, float humidity)
        {
            double lo = 100.0, hi = 22000.0;
            if (AirAbsorption(hi, celsius, humidity) * metres < CutoffLossDb)
                return (float)hi;
            if (AirAbsorption(lo, celsius, humidity) * metres > CutoffLossDb)
                return (float)lo;
            for (int i = 0; i < 40; i++)                                          // the loss rises with frequency: halve in log
            {
                double mid = Math.Sqrt(lo * hi);
                if (AirAbsorption(mid, celsius, humidity) * metres < CutoffLossDb) lo = mid; else hi = mid;
            }
            return (float)Math.Sqrt(lo * hi);
        }

        /// <summary>
        /// Equal-power weights for clips set at points along a driver (ascending): between two points the gains are cos and
        /// sin of the way across, so the summed power stays one; below the first or past the last, that clip alone.
        /// </summary>
        public static void Crossfade(float x, IList<float> points, float[] weights)
        {
            int n = points.Count;
            for (int i = 0; i < n; i++)
                weights[i] = 0f;
            if (n == 0)
                return;
            if (x <= points[0]) { weights[0] = 1f; return; }
            if (x >= points[n - 1]) { weights[n - 1] = 1f; return; }
            for (int i = 0; i < n - 1; i++)
                if (x < points[i + 1])
                {
                    float t = Mathf.InverseLerp(points[i], points[i + 1], x) * Mathf.PI * 0.5f;
                    weights[i] = Mathf.Cos(t);
                    weights[i + 1] = Mathf.Sin(t);
                    return;
                }
        }

        public static float DbToGain(float db) => db <= -80f ? 0f : Mathf.Pow(10f, db / 20f);

        /// <summary>A layer's level (dB, -80 for silent) in this context: its base plus its slope times the driver, capped,
        /// less the conditions (each a 0-1 ramp; together they multiply the gain).</summary>
        public static float LayerDb(SoundLayerDef layer, SoundContext c)
        {
            float when = layer.When(c);
            if (when <= 0f)
                return -80f;
            float drive = string.IsNullOrEmpty(layer.driver) ? 0f : c.Get(layer.driver);
            float db = Mathf.Min(layer.capDb, layer.baseDb + layer.dbPerUnit * drive);
            return Mathf.Max(-80f, db + 20f * Mathf.Log10(when));
        }

        // ------------------------------------------------------------------ night two

        /// <summary>
        /// How much the ground has cooled the air on it (0-1): all night, and in the first two hours or so after dawn (the
        /// Sun under 8 degrees in the morning, gone by 16); in the evening from the Sun 4 degrees up to its setting. A
        /// calm clear spell then has an inversion, and a fair night's air is damp (design 2.1, 2.2).
        /// </summary>
        public static float GroundCooled(float hour, float sunAltitude)
        {
            if (sunAltitude <= 0f)
                return 1f;
            return hour < 12f ? Mathf.InverseLerp(16f, 8f, sunAltitude) : Mathf.InverseLerp(4f, 0f, sunAltitude);
        }

        /// <summary>
        /// The relative humidity (0-1) from the kind of weather (LevelWeather's kind by name), since the weather has none of
        /// its own (design 2.1): fog and mist 1.0; rain, snow and a storm 0.95; overcast 0.85; fair and hazy 0.7 by day and
        /// 0.9 at night and in the hours after dawn (the river valley's dew). It feeds the air's absorption only.
        /// </summary>
        public static float HumidityFromWeather(string kind, float hour, float sunAltitude)
        {
            switch (kind)
            {
                case "Fog":
                case "Mist":
                    return 1f;
                case "Rain":
                case "Snow":
                case "Storm":
                    return 0.95f;
                case "Overcast":
                    return 0.85f;
                default:
                    return Mathf.Lerp(0.7f, 0.9f, GroundCooled(hour, sunAltitude));
            }
        }

        /// <summary>Wildlife falls silent from this Beaufort force (design 2.2 and 2.3: birds do not sing in a near gale).</summary>
        public const int WildlifeSilentFrom = 7;

        /// <summary>How open the air is to the wildlife (0-1): nothing at Beaufort 7 and over, else (1 - rain) squared
        /// (design 2.3: the windows times (1 - R)^2 times (B under 7)). Layers marked "mask": "wildlife" take it.</summary>
        public static float WildlifeOpen(SoundContext c)
        {
            if (Beaufort(c.wind) >= WildlifeSilentFrom)
                return 0f;
            float dry = 1f - Mathf.Clamp01(c.rain);
            return dry * dry;
        }

        /// <summary>
        /// How likely a ground inversion is (0-1): a calm (under about 2 m/s), a clear sky, and the ground cooled (night and
        /// the hours after dawn). Then sound bends down and carries: a frosty calm morning is the loudest-at-distance
        /// weather in the game (design 1.2, 2.2).
        /// </summary>
        public static float Inversion(SoundContext c)
        {
            float calm = Mathf.InverseLerp(2.5f, 1.5f, c.wind);
            float clear = Mathf.InverseLerp(0.7f, 0.3f, c.cloud);
            return calm * clear * GroundCooled(c.hour, c.sunAltitude);
        }

        /// <summary>
        /// The bending of a far sound (design 2.2), for a source this far away (m) with the wind blowing this much from it
        /// toward the listener (m/s; below 0 when the listener is upwind of it): the dB to add, and the factor on its
        /// low-pass corner. Downwind +3 dB and the corner times 1.5, upwind -6 dB and times 0.5 (cosine-weighted, and only
        /// once the wind is more than a breath); an inversion +4 dB and times 1.5. Sources nearer than 60-100 m are not bent.
        /// </summary>
        public static float RefractionDb(float metres, float windToward, SoundContext c, out float cutoffFactor)
        {
            float far = Mathf.InverseLerp(60f, 100f, metres);
            float inv = Inversion(c);
            float cos = c.wind > 0.01f ? Mathf.Clamp(windToward / c.wind, -1f, 1f) * Mathf.InverseLerp(1f, 4f, c.wind) : 0f;
            float db = 4f * inv + (cos >= 0f ? 3f * cos : 6f * cos);
            cutoffFactor = Mathf.Lerp(1f, (1f + 0.5f * inv) * (1f + 0.5f * cos), far);
            return db * far;
        }

        /// <summary>A voice's level (dB on the clip) this far away (m): its source level at 1 m in dB SPL, the data's offset
        /// from SPL to the game's level, its trim, less 6 dB a doubling of distance (the inverse distance law, from 1 m).</summary>
        public static float VoiceDb(SoundLayerDef voice, float metres, float splToDb)
        {
            float db = voice.splDb + splToDb + voice.trimDb - 20f * Mathf.Log10(Mathf.Max(metres, 1f));
            return Mathf.Max(-80f, db);
        }

        /// <summary>The bow wave over its floor (dB, 0-20): L_src = 60 + 20 log10(1 + v^2 / 0.8), capped at 80 (design 2.7;
        /// v her speed through the water, m/s).</summary>
        public static float BowWaveDb(float speed) => Mathf.Min(20f, 20f * Mathf.Log10(1f + speed * speed / 0.8f));

        /// <summary>The hull working over its floor (dB): 12 dB a doubling of the roll and pitch rates over 4 degrees a second
        /// (design 2.7: L_src = 55 + 12 log2(1 + (|roll rate| + |pitch rate|) / 4)).</summary>
        public static float HullWorkDb(float rollRate, float pitchRate) =>
            12f * Mathf.Log(1f + (Mathf.Abs(rollRate) + Mathf.Abs(pitchRate)) / 4f, 2f);

        /// <summary>
        /// Night two's stand-in for the acoustic cover map (night three): the surroundings guessed from the signed distance
        /// to the shore, as Dorestad lies. Open water on the water side of a 60 m disc; a reed fringe along the bank; hedges,
        /// scrub and trees on the land side; the town on the levee within a few hundred metres of the river; the strip
        /// fields behind it; the wet basins far back. The land's cover fades as the listener rises (the disc is under the
        /// ear only near the ground); the water does not.
        /// </summary>
        public static void GuessSurroundings(ref SoundContext c)
        {
            float s = c.shore;
            c.water = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-60f, 60f, s));
            float land = 1f - c.water;
            float near = Mathf.InverseLerp(150f, 30f, c.aboveWater);
            c.reed = near * 0.45f * Mathf.Clamp01(1f - Mathf.Abs(s + 5f) / 55f);
            c.hedge = near * 0.15f * land;
            c.wood = near * 0.25f * land;
            c.town = near * 0.5f * land * Mathf.InverseLerp(-500f, -250f, s);
            c.fields = near * 0.35f * Mathf.InverseLerp(-40f, -200f, s);
            c.basin = near * 0.5f * Mathf.InverseLerp(-250f, -700f, s);
        }
    }
}
