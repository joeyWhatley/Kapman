namespace Kapman.Nature
{
    /// <summary>
    /// THE YEAR'S DICE - every die of the wildlife model (a year's early or late spring, a hard winter, a mast year, a
    /// failed herring, an event's chance) goes through these two functions, so a saved game, another machine and the
    /// later shared world all throw the same numbers. Hash is the integer mix LevelWeather.Hash uses (copied here; the
    /// weather keeps its own copy until the user says to merge them); Id turns a name into a number with FNV-1a, 32 bits
    /// (the same construction SimRandom.For uses with 64). Never string.GetHashCode: it differs between Mono, IL2CPP and
    /// platforms.
    /// </summary>
    public static class Dice
    {
        /// <summary>A number in [0, 1) from two integers: the same two always give the same number.</summary>
        public static float Hash(int a, int b)
        {
            unchecked
            {
                uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        /// <summary>A name as a number (FNV-1a, 32 bits, over the UTF-16 code units).</summary>
        public static int Id(string name)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char c in name ?? "")
                {
                    h ^= c;
                    h *= 16777619u;
                }
                return (int)h;
            }
        }

        /// <summary>The die for a thing in a year: Hash(seed ^ year, Id(name)) (the design's hash(seed, y, s)).</summary>
        public static float Roll(int seed, int year, string name) => Hash(seed ^ year, Id(name));

        /// <summary>The die for a thing on a day: Hash(seed ^ (year * 366 + day), Id(name)) (the events' die).</summary>
        public static float RollDay(int seed, int year, int dayOfYear, string name) => Hash(seed ^ (year * 366 + dayOfYear), Id(name));
    }
}
