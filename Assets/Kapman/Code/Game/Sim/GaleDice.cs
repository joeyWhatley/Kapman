using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE GALE'S DICE, the same for every hull (the strategy critic, 7 Oct 2026: "the same gale dice for her as for the
    /// other ships"). A gale is wind over <see cref="GaleWind"/> where the ship is (the line Travel.Speed draws). Each
    /// game hour a ship is under way in one, the sea strikes her with the chance <see cref="Chance"/>: the table's chance
    /// an hour (SimRules.galeLossPerHour), times her class's fragility, times the state she is in (1 for a sound boat
    /// lying to, which is what the world's ships are; hers is Boat.DiceFactor).
    /// </summary>
    public static class GaleDice
    {
        /// <summary>Metres a second: over this a ship lies to (Travel.Speed) and the dice are thrown.</summary>
        public const double GaleWind = 17.0;

        public static double Chance(double perHour, double fragility, double state = 1.0) =>
            Math.Max(0.0, Math.Min(1.0, perHour * fragility * state));

        /// <summary>The throw for one hull in one game hour, 0 to 1. It has no memory: the seed, the hull's name and the
        /// hour's count give it, so it is the same at every speed of the clock, and the same after a saved game is loaded
        /// (loading again does not throw again: only a different order changes what the hour does).</summary>
        public static double Roll(int seed, string who, long hour) => SimRandom.For(seed, who + "@" + hour).Next();
    }
}
