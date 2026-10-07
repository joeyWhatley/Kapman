using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE CHAIN AT RUNTIME, as far as tonight goes (gateways 2.5; the streamer itself comes with the cut, night 2):
    /// which level the ship is in, and the one rule every level singleton keeps when it wakes - take the static Current
    /// (LevelInfo, LevelClock, LevelWeather, LevelWater, HdrpSea, RiverCurrent, ShipMotion.Player, SkyDome) only when no
    /// chain is running or the component belongs to the current level, so a level preloaded beside the current one never
    /// brings a second sun, clock or listener. Until the streamer exists nothing sets <see cref="Current"/>: it stays null,
    /// every guard lets the component through, and the game behaves exactly as before.
    /// </summary>
    public static class ChainRuntime
    {
        /// <summary>The level the ship is in, while the chain is streaming levels; null when a level was opened alone (the
        /// editor, the shots, Play from a level scene).</summary>
        public static LevelRoot Current { get; set; }

        /// <summary>True while a streamer loads and cuts levels: a level root then waits to be activated by it instead of
        /// waking itself.</summary>
        public static bool Streaming { get; set; }

        /// <summary>May this component take its level's static Current? Yes with no chain running, yes outside every
        /// level root (the persistent scene's own), and yes inside the current level; no inside any other level.</summary>
        public static bool Allows(Component component)
        {
            if (Current == null || component == null)
                return true;
            var root = LevelRoot.Of(component);
            return root == null || root == Current;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Current = null;
            Streaming = false;
        }
    }
}
