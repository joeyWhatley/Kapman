using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE CLOCK'S STEP (ruling 26 of 7 October 2026: a pause and five speeds). The game's time moves in whole game
    /// minutes, at every speed: a frame at speed 1 runs no step or one, a frame at speed 5 runs six or more, and each
    /// step is the same step. So what happens in the world does not depend on the speed it was played at, nor on how
    /// long the frames were: only on how many steps have been run. Pure C#, no scene: the game (KapmanGame.RunClock)
    /// asks <see cref="Take"/> how many steps this frame wants, and calls <see cref="Step"/> for each one it runs.
    /// </summary>
    public sealed class ClockStepper
    {
        /// <summary>One step is one game minute, at every speed.</summary>
        public const double StepSeconds = 60.0;
        /// <summary>A frame longer than this (real seconds) counts as this long: a hitch is not a jump of the clock.</summary>
        public const double LongestFrame = 0.1;
        /// <summary>The most steps one frame may run (speed 5 at ten frames a second is 36).</summary>
        public const int MostSteps = 40;

        double owed;                                              // game seconds wanted and not yet run

        /// <summary>Whole game minutes since the clock's start.</summary>
        public long Minute { get; private set; }
        /// <summary>The same in game hours.</summary>
        public double Hours => Minute / 60.0;
        /// <summary>How far the clock is into the step to come, 0..1 (for drawing her mark between two steps).</summary>
        public double Fraction => Math.Max(0.0, Math.Min(1.0, owed / StepSeconds));

        /// <summary>The clock was set from outside (a new game, a loaded one): the stepper stands at its minute, owing nothing.</summary>
        public void SetTo(double gameHours)
        {
            Minute = (long)Math.Floor(gameHours * 60.0 + 1e-6);
            owed = 0.0;
        }

        /// <summary>
        /// A frame of `realSeconds` at `rate` game seconds a real second: how many whole steps it wants now. What is
        /// left under one step is kept for the next frame. A rate of 0 (paused, held) wants none and owes none.
        /// </summary>
        public int Take(double realSeconds, double rate)
        {
            if (!(rate > 0.0))
            {
                owed = 0.0;
                return 0;
            }
            if (realSeconds > 0.0)
                owed += Math.Min(realSeconds, LongestFrame) * rate;
            double most = MostSteps * StepSeconds;
            if (owed > most)
                owed = most;                                      // (what the machine could not run is not owed: the clock runs slower, nothing is skipped)
            return (int)Math.Floor(owed / StepSeconds + 1e-9);
        }

        /// <summary>One step is run: the minute it ends on.</summary>
        public long Step()
        {
            owed = Math.Max(0.0, owed - StepSeconds);
            return ++Minute;
        }

        /// <summary>Something needs the player, or the speed changed: what was still wanted of this frame is let go.</summary>
        public void Stop() => owed = 0.0;
    }
}
