using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// A ship's own dice: PCG32 (O'Neill 2014), seeded from the world's seed and the ship's id, so every ship draws the
    /// same numbers in the same order whatever the others do - a run can be played again from its seed, and one ship's
    /// change doesn't reshuffle the rest. Plain state (two numbers), so it saves and restores.
    /// </summary>
    public struct SimRandom
    {
        public ulong state, inc;

        public SimRandom(ulong seed, ulong stream)
        {
            state = 0UL;
            inc = (stream << 1) | 1UL;
            NextUInt();
            state += seed;
            NextUInt();
        }

        /// <summary>A stream for a thing in the world: the seed and a name (a ship's id, "weather", ...).</summary>
        public static SimRandom For(int seed, string name)
        {
            ulong h = 14695981039346656037UL;
            foreach (char c in name ?? "")
                h = (h ^ c) * 1099511628211UL;
            return new SimRandom((ulong)(uint)seed * 0x9E3779B97F4A7C15UL ^ h, h);
        }

        public uint NextUInt()
        {
            ulong old = state;
            state = unchecked(old * 6364136223846793005UL + inc);
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// <summary>0 (inclusive) to 1 (exclusive).</summary>
        public double Next() => NextUInt() * (1.0 / 4294967296.0);

        /// <summary>lo (inclusive) to hi (exclusive).</summary>
        public int Range(int lo, int hi) => hi <= lo ? lo : lo + (int)(Next() * (hi - lo));

        public double Range(double lo, double hi) => lo + (hi - lo) * Next();

        /// <summary>True with this chance (0-1).</summary>
        public bool Chance(double p) => Next() < p;

        /// <summary>A normal draw (mean 0, spread 1).</summary>
        public double Normal()
        {
            double u = 1.0 - Next(), v = Next();
            return Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
        }
    }

    /// <summary>
    /// The world's units. The simulation keeps real metres (as the bakes and the map do) and game hours (the clock's).
    /// A ship at v m/s sails v game metres a real second; a game hour is 60 real seconds; a game metre is 32 real
    /// metres across. So a game hour carries her 60 x v game metres = 1,920 x v real metres (5 kn: 4,939 m).
    /// </summary>
    public static class SimUnits
    {
        public const double KnotMs = 0.514444;

        /// <summary>The game's scale across (the user's decision: 32). The scene's, in play; the decided one, headless.</summary>
        /// <summary>The sim's pace: real metres a ship covers per game hour per metre-per-second of speed, over 60 s. The first
        /// build's world was 1:32; the rebuild's river places are 1:1 and its seas ~1:150, and the abstract sim (ships on real
        /// lanes) keeps ONE pace for the whole world: AcrossDefault (32 keeps the tuned balance and the tests; the level chain
        /// design may set it to the seas' shortening so the world's ships cross a sea in the time the player does).</summary>
        public static double AcrossDefault = 32.0;
        public static double Across => Kapman.World.FloatingOrigin.HorizontalScale > 1.0 ? Kapman.World.FloatingOrigin.HorizontalScale : AcrossDefault;

        /// <summary>Real metres sailed in one game hour at a speed (m/s, the ship's real speed).</summary>
        public static double RealMetresPerGameHour(double metresPerSecond) => metresPerSecond * 60.0 * Across;

        public static double Knots(double knots) => knots * KnotMs;
    }
}
