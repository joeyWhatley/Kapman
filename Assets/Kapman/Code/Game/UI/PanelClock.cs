namespace Kapman.Game
{
    /// <summary>
    /// THE CLOCK WAITS WHILE YOU THINK (the user, 4 Oct 2026, picker: "hold the clock while a panel is open") - days are
    /// the game's one fuel, spent on purpose: by sailing and by waiting. So while the port's menu, the market, a card or
    /// the chest is open, the game is held (GameSpeed.Hold), and it runs again when the panel closes. Resting through the
    /// night and waiting for a tide are waiting: they run the clock though a panel is up.
    /// KapmanGame calls Tick each frame, before GameSpeed.Tick sets the time scale.
    ///
    /// ONLY A CARD STOPS THE CLOCK (the user's ruling of 7 Oct 2026, Design/DECISIONS.md, which changes the rule above
    /// where the command frame is up: <see cref="OnlyCards"/>). Tiled windows never hold the clock, at sea or in port:
    /// the player reads, plans, trades and looks things up while time runs. Only a card or a talk that needs an answer
    /// holds it (<see cref="Holds"/>), and the player's own pause. "It stops the clock but you can override it
    /// immediately if you want": going on (Space, a clock button) with a card still open lets the clock run and the card
    /// wait (<see cref="LetGo"/>).
    /// </summary>
    public static class PanelClock
    {
        public const string Why = "panel";

        /// <summary>The rule (off: the clock runs in panels, as before 4 Oct).</summary>
        public static bool On = true;

        /// <summary>Only what <see cref="Holds"/> says holds the clock: an open panel alone does not (the frame sets it).</summary>
        public static bool OnlyCards;

        /// <summary>A panel that takes the player's thought is open.</summary>
        public static bool PanelOpen =>
            (TextPanel.Current != null && TextPanel.Current.Open) ||
            (MarketPanel.Current != null && MarketPanel.Current.IsOpen) ||
            EncounterPanel.Showing ||
            (MapView.Current != null && MapView.Current.ChestOpen);

        /// <summary>True while the rule holds the clock.</summary>
        public static bool Holding => GameSpeed.HeldFor(Why);

        /// <summary>Something else that holds the clock while it wants an answer (the frame: a card floating over the
        /// chart). It holds though the player is waiting.</summary>
        public static System.Func<bool> Holds;
        /// <summary>Another kind of waiting that runs the clock though a panel is up (the frame's Wait button).</summary>
        public static System.Func<bool> Waiting;
        /// <summary>The player goes on though something holds the clock (Space, a clock button): what holds it lets go,
        /// and stays open. Null: nothing can be overridden (as before the frame).</summary>
        public static System.Action LetGo;

        public static void Tick()
        {
            bool waiting = Rest.On || TideWait.On || (Waiting != null && Waiting());
            bool panel = !OnlyCards && PanelOpen && !waiting;
            if (On && (panel || (Holds != null && Holds())))
                GameSpeed.Hold(Why);
            else
                GameSpeed.Release(Why);
        }

        /// <summary>The player goes on: if the rule holds the clock and it may be overridden, it is. True if it was.</summary>
        public static bool GoOn()
        {
            if (!Holding || LetGo == null)
                return false;
            LetGo();
            GameSpeed.Release(Why);
            return true;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Holds = null;
            Waiting = null;
            LetGo = null;
            OnlyCards = false;
        }
    }
}
