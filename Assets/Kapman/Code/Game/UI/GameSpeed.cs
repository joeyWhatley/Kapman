using System;
using System.Collections.Generic;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME'S SPEED - one owner for how fast the game runs. Ruling 26 of 7 October 2026: a pause and five speeds,
    /// like CK3. Speed 1 is a game minute a real second (a day in 24 minutes); 2 is four times that (a day in 6
    /// minutes); 3 a day in a minute; 4 a day in fifteen seconds; 5 a day in four seconds.
    ///   - [Space] pauses and goes on; [+] and [-] (or the top bar's buttons) choose the speed. The number keys are the
    ///     workspaces' and the cards'.
    ///   - Things that must stop the game hold it while they're on (the menu, a card that wants an answer); it runs
    ///     again when the last hold is let go.
    ///   - The clock drops back by itself when something needs the player (<see cref="DropTo"/>; the reasons and their
    ///     switches are ClockDrops and ClockSettings). The speed she ran at is kept (<see cref="Before"/>): [+] goes
    ///     straight back to it.
    ///   - On the chart the speed is NOT Unity's time scale. The game steps its own clock, a game minute at a time
    ///     (<see cref="Stepped"/>, KapmanGame.RunClock, <see cref="Rate"/>), and the time scale stays 1 (0 when stopped):
    ///     so the view of the ship, the water and every animation keep their own calm pace at every speed.
    ///   - In a 3D level (no chart) the old way holds: the speed is the time scale, and nothing there runs faster
    ///     than four times (speeds 2 to 5 are all 4x there).
    /// The clock's own hold (two ships alongside, dealing) stops only the game's clock, not the sea: that stays with the
    /// clock (GameClock.held).
    /// </summary>
    public static class GameSpeed
    {
        /// <summary>Game seconds a real second at each of the five speeds.</summary>
        public static readonly double[] Rates = { 60.0, 240.0, 1440.0, 5760.0, 21600.0 };
        /// <summary>What each speed is, for the buttons (placeholder wording).</summary>
        public static readonly string[] Tips =
        {
            "Speed 1. A game minute a second: a day in 24 minutes. For being aboard.",
            "Speed 2. Four game minutes a second: a day in 6 minutes.",
            "Speed 3. An hour in two and a half seconds: a day in a minute.",
            "Speed 4. A day in fifteen seconds.",
            "Speed 5. A day in four seconds.",
        };
        public const int Count = 5;

        /// <summary>Which of the five is chosen: 0 is speed 1.</summary>
        public static int Level { get; private set; }
        /// <summary>Paused by the player.</summary>
        public static bool PlayerPaused { get; private set; }
        /// <summary>Multiplayer: no pausing, no faster speeds (the world is everyone's).</summary>
        public static bool Multiplayer;
        /// <summary>The game steps its own clock (the chart: KapmanGame sets it each frame). Off: the speed is the time scale.</summary>
        public static bool Stepped;
        /// <summary>The fastest level the clock may run at now (a pressing matter unanswered: ClockDrops sets it). The
        /// chosen level is kept: when the cap goes, the clock runs at it again.</summary>
        public static int Cap = Count - 1;

        /// <summary>The level she ran at before the clock dropped back by itself (-1: it has not, or the player has
        /// chosen a speed since).</summary>
        public static int Before { get; private set; } = -1;
        /// <summary>Why it dropped, in a few words for the top bar ("" none).</summary>
        public static string DroppedFor { get; private set; } = "";

        static readonly HashSet<string> holds = new HashSet<string>();

        /// <summary>The speed or the pause changed.</summary>
        public static event Action Changed;

        public static bool Paused => !Multiplayer && (PlayerPaused || holds.Count > 0);
        public static bool HeldFor(string why) => holds.Contains(why);

        /// <summary>The level the clock runs at now: the chosen one, under the cap.</summary>
        public static int Running => Multiplayer ? 0 : Mathf.Clamp(Mathf.Min(Level, Cap), 0, Count - 1);

        /// <summary>Game seconds a real second now: 0 stopped, else the running speed's.</summary>
        public static double Rate => Paused ? 0.0 : Rates[Running];

        /// <summary>The time scale wanted now. Stepped (the chart): 0 stopped, else 1 at every speed. Not stepped (a 3D
        /// level): the speed itself, no more than four times (1x while resting or in multiplayer).</summary>
        public static float Factor => Paused ? 0f : Stepped || Multiplayer || OwnPace ? 1f : Mathf.Min(4f, (float)(Rates[Running] / 60.0));

        /// <summary>A 3D level only: resting or waiting for a tide at a pace of its own (the clock's rate raised).</summary>
        static bool OwnPace => (Rest.On && Rest.RateCap <= 0f) || (TideWait.On && TideWait.RateCap <= 0f);

        public static string Describe => Paused ? (!PlayerPaused && holds.Count == 1 && holds.Contains(PanelClock.Why) ? "the clock waits" : "paused") : $"speed {Running + 1}";

        /// <summary>The player chooses a speed (a button, or through <see cref="Faster"/> and <see cref="Slower"/>).</summary>
        public static void SetLevel(int level)
        {
            level = Mathf.Clamp(level, 0, Count - 1);
            if (Multiplayer)
                level = 0;
            bool unpause = PlayerPaused;
            PlayerPaused = false;
            bool forgot = Before >= 0;                                    // (the player has chosen: nothing to go back to)
            Before = -1;
            DroppedFor = "";
            if (PanelClock.GoOn() || forgot)                              // (a card held the clock: a speed chosen sets it running, the card waits)
                Changed?.Invoke();
            if (level == Level && !unpause)
                return;
            Level = level;
            if (unpause)
                Alerts.AnswerWaitingUrgent();
            Changed?.Invoke();
        }

        /// <summary>[+]: after the clock dropped back by itself, straight back to the speed she ran at; else one faster.</summary>
        public static void Faster() => SetLevel(Before > Level ? Before : Level + 1);

        /// <summary>[-]: one slower.</summary>
        public static void Slower() => SetLevel(Level - 1);

        public static void TogglePause()
        {
            if (Multiplayer)
                return;
            if (PlayerPaused || holds.Contains(Alerts.Hold))
            {
                PlayerPaused = false;
                Alerts.AnswerWaitingUrgent();                              // (going on answers what was waiting)
            }
            else if (PanelClock.GoOn())
            {
                // (a card held the clock and the player goes on: the clock runs and the card waits, still open. Its
                // week or its hours are counted from now, so the clock goes on no faster than speed 2 until the
                // player asks for more: at speed 5 a card's week is 28 seconds)
                if (!ClockDrops.Quiet)
                    DropTo(LetRunLevel, "a card is open");
            }
            else
                PlayerPaused = true;
            Changed?.Invoke();
        }

        /// <summary>The fastest level the clock goes on at when the player lets it run with a card open (1: speed 2).</summary>
        public const int LetRunLevel = 1;

        /// <summary>Stops the game for something (the menu, a card) until <see cref="Release"/>.</summary>
        public static void Hold(string why)
        {
            if (!string.IsNullOrEmpty(why) && holds.Add(why))
                Changed?.Invoke();
        }

        public static void Release(string why)
        {
            if (why != null && holds.Remove(why))
                Changed?.Invoke();
        }

        /// <summary>The clock drops back by itself to a level (0 is speed 1), and says why. It never raises the speed.
        /// The speed she ran at is kept for <see cref="Faster"/>. True if it dropped.</summary>
        public static bool DropTo(int level, string why)
        {
            level = Mathf.Clamp(level, 0, Count - 1);
            if (Multiplayer || Level <= level)
                return false;
            if (Before < 0)
                Before = Level;                                           // (a second drop keeps the first speed to go back to)
            Level = level;
            DroppedFor = why ?? "";
            Changed?.Invoke();
            return true;
        }

        /// <summary>Back to speed 1 (a non-urgent alert: you'll want to see it come).</summary>
        public static void Slow() => DropTo(0, "");

        /// <summary>Each frame (the game): the keys, and the time scale set to what's wanted.</summary>
        public static void Tick()
        {
            var kb = Keyboard.current;
            bool menu = MainMenu.Current != null && MainMenu.Current.Open;
            if (kb != null && !menu && !Multiplayer && !UiTyping.Active)
            {
                if (kb.spaceKey.wasPressedThisFrame)
                    TogglePause();
                else if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
                    Faster();
                else if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
                    Slower();
            }
            float want = Factor;
            if (!Mathf.Approximately(Time.timeScale, want))
                Time.timeScale = want;
            // the level clock has a speed and a pause of its own (its Tab and Space keys): kept at 1x and unpaused, so
            // what reads LevelClock.TimeFactor (the view of the ship, the water) runs at its own calm pace. Stepped, the
            // level clock does not add the frame's time to its hours either: KapmanGame.RunClock moves them
            LevelClock.Speed = 1f;
            LevelClock.Paused = false;
            LevelClock.External = Stepped;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Level = 0;
            PlayerPaused = false;
            Stepped = false;
            Cap = Count - 1;
            Before = -1;
            DroppedFor = "";
            holds.Clear();
            Changed = null;
            Time.timeScale = 1f;
            LevelClock.External = false;
        }
    }
}
