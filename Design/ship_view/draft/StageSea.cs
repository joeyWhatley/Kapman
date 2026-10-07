// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/StageSea.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run.
using System;
using Kapman.Weather;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE STAGE'S SEA. Its state is the game's own model (Kapman.Weather.SeaStateModel, the one HdrpSea steps in the 3D
    /// levels): the wind raises a sea over the fetch and the depth of the place, by the growth laws in WaveGrowth, on the
    /// game's clock. It is made with no weather engine, so only the wind it is given raises it (no swell from far storms:
    /// that needs the engine and a place class, and is a later step).
    /// The surface is eight sine waves made from that state: six for the wind sea, spread in period and fanned about the
    /// wind, sharing the energy of the significant height; two for a swell when there is one. She is held still and the
    /// water goes by: each wave keeps its own running phase, moved on by its own speed and by her way through it, so she
    /// meets the seas faster head on and slower running, and nothing loses precision however long she sails.
    /// One function gives the height anywhere (the boat floats on it, the water mesh is moved by it), and the same waves
    /// are set as shader globals for a look that moves its water in the vertex shader:
    ///   _StageWaveA[8] = (direction x, direction z, amplitude m, wavenumber rad/m)
    ///   _StageWaveB[8] = (running phase rad, 0, 0, 0)
    ///   height(x, z) = sum of A.z * sin(A.w * (A.x * x + A.y * z) + B.x), x and z metres from her, east and north.
    /// </summary>
    public sealed class StageSea
    {
        public const int Count = 8, WindWaves = 6;
        const float G = 9.81f, TwoPi = Mathf.PI * 2f;

        // the wind sea's six waves: period over the peak period, share of the energy, how far off the wind (in spreads)
        static readonly float[] Period = { 1.25f, 1f, 0.85f, 0.7f, 0.55f, 0.42f };
        static readonly float[] Share = { 0.12f, 0.34f, 0.22f, 0.16f, 0.10f, 0.06f };
        static readonly float[] Fan = { 0.27f, -0.17f, 0.73f, -0.93f, 1.33f, -1.5f };
        static readonly float[] Start = { 0.3f, 2.1f, 4.4f, 1.2f, 5.6f, 3.3f, 0.9f, 2.8f };

        /// <summary>The least sea there ever is (m): a calm is never a dead plane.</summary>
        public float calmHeight = 0.025f;

        /// <summary>The sea now (the model's).</summary>
        public SeaState State { get; private set; }
        /// <summary>How much of the surface breaks white, 0..1 (Monahan, from the model).</summary>
        public float Whitecaps => State.whitecaps;
        /// <summary>The waves: direction x, direction z (they travel that way), amplitude, wavenumber.</summary>
        public readonly Vector4[] A = new Vector4[Count];
        /// <summary>Each wave's running phase.</summary>
        public readonly Vector4[] B = new Vector4[Count];
        /// <summary>The water that has gone by under her, metres east and north (what is fixed in the world, a jetty or a
        /// bank, is moved against it).</summary>
        public double DriftEast { get; private set; }
        public double DriftNorth { get; private set; }

        readonly float[] omega = new float[Count], amp = new float[Count], bearing = new float[Count], period = new float[Count];
        SeaStateModel model;
        float fetchKm = -1f, depth = -1f;
        double lastHours = double.NaN;
        bool settle = true;

        static readonly int AId = Shader.PropertyToID("_StageWaveA"), BId = Shader.PropertyToID("_StageWaveB");

        public StageSea()
        {
            for (int i = 0; i < Count; i++)
                B[i].x = Start[i];
        }

        /// <summary>The sea as it would be after two days of this wind, at once (a test's picture, a loaded game, a jump of the clock).</summary>
        public void Settle() => settle = true;

        /// <summary>Steps the sea to the state's moment, and the waves on by dt real seconds.</summary>
        public void Step(in ShipStageState s, float dt)
        {
            if (model == null || Mathf.Abs(s.fetchKm - fetchKm) > 0.01f || Mathf.Abs(s.depth - depth) > 0.01f)
            {
                fetchKm = s.fetchKm;
                depth = s.depth;
                var fetch = FetchTable.Open(Math.Max(0.05, fetchKm), Math.Max(0.5, depth));
                if (model == null)
                {
                    model = new SeaStateModel(null, s.latitude, s.longitude, "north_sea", fetch) { Swell = false };
                    settle = true;
                }
                else
                    model.Fetch = fetch;                                  // (out of the river mouth: the sea rises over the next hours)
            }
            bool jumped = double.IsNaN(lastHours) || s.hours < lastHours - 1e-6 || s.hours - lastHours > 0.75;
            if (settle || jumped)
                model.WarmStart(s.hours, s.windSpeed, s.windFrom, 1013.0);
            else if (s.hours > lastHours)
                model.Step(s.hours, s.hours - lastHours, s.windSpeed, s.windFrom, 1013.0);
            lastHours = s.hours;
            State = model.State;

            Waves(State, settle || jumped ? 1f : 1f - Mathf.Exp(-dt / 3f), settle || jumped ? 1f : 1f - Mathf.Exp(-dt / 20f));
            settle = false;

            // her way through the water (the 3D levels' ShipMotion moves her by the clock's speed too), and each wave's own
            float a = s.heading * Mathf.Deg2Rad;
            float ve = Mathf.Sin(a) * s.speed * s.timeFactor, vn = Mathf.Cos(a) * s.speed * s.timeFactor;
            DriftEast += ve * dt;
            DriftNorth += vn * dt;
            for (int i = 0; i < Count; i++)
            {
                B[i].x = Mathf.Repeat(B[i].x + (A[i].w * (A[i].x * ve + A[i].y * vn) - omega[i]) * dt, TwoPi);
                B[i].y = omega[i];
            }
            Shader.SetGlobalVectorArray(AId, A);
            Shader.SetGlobalVectorArray(BId, B);
        }

        /// <summary>The eight waves of a sea state. Heights follow within seconds, periods and bearings slowly (a wave that
        /// changes its length at once would race across the far water).</summary>
        void Waves(in SeaState sea, float fast, float slow)
        {
            float hs = Mathf.Max(sea.windSeaHs, calmHeight), tp = Mathf.Max(sea.windSeaTp, 0.9f);
            float spread = sea.spread > 1f ? sea.spread : 30f;
            float m0 = hs * hs / 16f;                                       // Hs = 4 sqrt(m0); a sine of amplitude a carries a^2 / 2
            for (int i = 0; i < Count; i++)
            {
                float t, a, to;
                if (i < WindWaves)
                {
                    t = tp * Period[i];
                    a = Mathf.Sqrt(2f * m0 * Share[i]);
                    to = sea.windSeaFrom + 180f + Fan[i] * spread;
                }
                else
                {
                    int k = i - WindWaves;
                    t = Mathf.Max(sea.swellTp, 4f) * (k == 0 ? 1f : 0.9f);
                    float sm0 = sea.swellHs * sea.swellHs / 16f;
                    a = Mathf.Sqrt(2f * sm0 * (k == 0 ? 0.7f : 0.3f));
                    to = sea.swellFrom + 180f + (k == 0 ? 4f : -5f);
                }
                bool first = period[i] <= 0f;
                period[i] = first ? t : Mathf.Lerp(period[i], t, slow);
                bearing[i] = first ? to : Mathf.LerpAngle(bearing[i], to, slow);
                float om = TwoPi / Mathf.Max(period[i], 0.3f), wn = om * om / G;    // deep water: omega^2 = g k
                a = Mathf.Min(a, 0.3f / wn);                                      // no wave steeper than it can stand
                amp[i] = Mathf.Lerp(amp[i], a, fast);
                omega[i] = om;
                float b = bearing[i] * Mathf.Deg2Rad;
                A[i] = new Vector4(Mathf.Sin(b), Mathf.Cos(b), amp[i], wn);
            }
        }

        /// <summary>The water's height over its level (m) at a point east and north of her (m).</summary>
        public float Height(float east, float north)
        {
            float h = 0f;
            for (int i = 0; i < Count; i++)
                h += A[i].z * Mathf.Sin(A[i].w * (A[i].x * east + A[i].y * north) + B[i].x);
            return h;
        }

        /// <summary>The height and its slopes east and north, leaving out waves shorter than a mesh of this cell can carry
        /// (two cells: gone; four: whole). cell 0 takes every wave.</summary>
        public float Height(float east, float north, float cell, out float slopeEast, out float slopeNorth)
        {
            float h = 0f;
            slopeEast = slopeNorth = 0f;
            for (int i = 0; i < Count; i++)
            {
                float a = A[i].z;
                if (a < 1e-4f)
                    continue;
                if (cell > 0f)
                {
                    float lambda = TwoPi / A[i].w;
                    float keep = Mathf.Clamp01((lambda - 2f * cell) / (2f * cell));
                    if (keep <= 0f)
                        continue;
                    a *= keep;
                }
                float th = A[i].w * (A[i].x * east + A[i].y * north) + B[i].x;
                float sin = Mathf.Sin(th), cos = Mathf.Cos(th);
                h += a * sin;
                slopeEast += a * A[i].w * A[i].x * cos;
                slopeNorth += a * A[i].w * A[i].y * cos;
            }
            return h;
        }
    }
}
