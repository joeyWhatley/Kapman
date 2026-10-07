using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// REST - the night passes (the user, 2026-10-02: nights play in real time, with an option to "go to bed or rest
    /// against the oars"). When it's dark (the Sun 6 degrees under), [Z] rests the crew till dawn: the clock runs at half
    /// an hour a second (the stars wheel over), the ship goes on as she was (sailing her course with a man at the helm,
    /// or lying where she lies); it ends at first light, at [Z] again, or the moment anything happens (a ship sighted and
    /// met, raiders, the king's men, an event, a panel, running aground). Nightfall is told once a night.
    /// ported 4 Oct: the clock is the level's (Kapman.Levels.LevelClock; its rate is game seconds per real second, as
    /// GameClock's was, so the resting rate goes in the same place). The Sun comes from Kapman.World.Sky, which the
    /// level's clock works out each frame, as the first build's clock did.
    /// </summary>
    public static class Rest
    {
        public static bool On { get; private set; }
        /// <summary>Game seconds per real second while resting (1800: half an hour a second).</summary>
        public static float Rate = 1800f;
        /// <summary>The most game seconds a real second while resting (0: no cap). The chart's frame sets it to the clock's
        /// own pace: no hours are skipped there (the user's ruling of 7 Oct 2026), so resting is waiting at the game's speed.</summary>
        public static float RateCap;
        /// <summary>The Sun's altitude (degrees) under which it's night for resting: from dusk's end to first light.</summary>
        public const double DarkBelow = -6.0;

        static float rateWas;
        static bool toldTonight;

        public static bool Dark => Sky.Known && Sky.Sun.altitude < DarkBelow;

        /// <summary>Anything going on that resting would skip past?</summary>
        static bool Busy() =>
            (TextPanel.Current != null && TextPanel.Current.Open) || Alerts.Open.Count > 0
            || Sim.Encounter.Stage != Sim.EncounterStage.None || Sim.RaiderEncounter.Stage != Sim.RaidStage.None
            || Sim.Patrols.Stage != Sim.PatrolStage.None || Aground.Stage != AgroundStage.None || GameEvents.Current != null
            || TideWait.On || (MainMenu.Current != null && MainMenu.Current.Open);

        /// <summary>Each frame (the game): [Z] at night, and the end of resting, on the level's clock.</summary>
        public static void Tick() => Tick(LevelClock.Current);

        /// <summary>Each frame (the game): [Z] at night, and the end of resting.</summary>
        public static void Tick(LevelClock clock)
        {
            if (clock == null)
                return;
            if (!Dark)
                toldTonight = false;
            else if (!toldTonight && !On)
            {
                toldTonight = true;
                // (where no hours are skipped, on the chart, resting changes nothing: the key is not offered there)
                Voyage.Say(RateCap > 0f ? "Night falls." : "Night falls. [Z] to rest till dawn.");
            }
            var kb = UnityEngine.InputSystem.Keyboard.current;
            bool pressed = kb != null && !UiFocus.Modal && kb.zKey.wasPressedThisFrame && !kb.ctrlKey.isPressed;   // ([Z]: Ctrl+Z is Build mode's undo)
            if (On)
            {
                if (pressed)
                    End(clock, "You stop resting.");
                else if (!Dark)
                {
                    End(clock, "Dawn.");
                    ClockDrops.Raise(ClockReason.WaitOver);               // (the wait is over: the clock drops back)
                }
                else if (Busy())
                    End(clock, null);
                return;
            }
            if (!pressed)
                return;
            if (GameSpeed.Multiplayer)
                Voyage.Say("No resting through the night with others aboard the world.");
            else if (!Dark)
                Voyage.Say("It's not night: there's no resting yet.");
            else if (Busy())
                Voyage.Say("Not now.");
            else
                Begin(clock);
        }

        /// <summary>Something with its own pace takes over (waiting for the tide): resting ends quietly.</summary>
        public static void Interrupt(LevelClock clock)
        {
            if (On)
                End(clock, null);
        }

        static void Begin(LevelClock clock)
        {
            On = true;
            rateWas = clock.rate;
            clock.rate = RateCap > 0f ? System.Math.Min(Rate, System.Math.Max(rateWas, RateCap)) : Rate;
            Voyage.Say("The crew rest till dawn; the watch keeps her going.");
        }

        static void End(LevelClock clock, string fact)
        {
            On = false;
            if (clock != null && rateWas > 0f)
                clock.rate = rateWas;
            rateWas = 0f;
            if (!string.IsNullOrEmpty(fact))
                Voyage.Say(fact);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            On = false;
            rateWas = 0f;
            toldTonight = false;
        }
    }
}
