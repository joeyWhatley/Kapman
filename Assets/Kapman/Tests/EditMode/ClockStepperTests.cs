using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
#if !STEPCHECK
using NUnit.Framework;
#endif

namespace Kapman.Tests
{
    /// <summary>
    /// A small game on the clock's step, with no scene: a boat that sails each minute, a world that does something each
    /// hour and reads where the boat is, cards that come on seeded hours and hold the clock until a player answers some
    /// frames later, and things that drop the speed. It is run frame by frame as KapmanGame.RunClock runs the real one
    /// (Take, Step, Stop), so the test can ask the one question of ruling 26: does the same seed end in the same world
    /// at speed 1 and at speed 5, whatever the frames were?
    /// </summary>
    public sealed class ClockRun : ITickSink
    {
        public static readonly double[] Rates = { 60.0, 240.0, 1440.0, 5760.0, 21600.0 };

        public readonly ClockStepper stepper = new ClockStepper();
        readonly TickLadder ladder = new TickLadder(793, 121);
        readonly int seed, wanted, answerFrames;

        /// <summary>The world, as one number: every tick and everything it did goes into it in order.</summary>
        public ulong hash = 14695981039346656037UL;
        /// <summary>The game minutes at which a card came (and held the clock), and at which the speed dropped.</summary>
        public readonly List<long> cards = new List<long>(), drops = new List<long>();
        public long until = long.MaxValue;
        public int frames, mostSteps;
        public double x, z;

        int level, hold;
        double lastHours;
        bool card, drop;

        public ClockRun(int seed, int level, int answerFrames)
        {
            this.seed = seed;
            this.level = wanted = level;
            this.answerFrames = answerFrames;
        }

        void Mix(long v)
        {
            unchecked
            {
                for (int b = 0; b < 8; b++)
                {
                    hash ^= (byte)(v >> (8 * b));
                    hash *= 1099511628211UL;
                }
            }
        }

        static uint Dice(int seed, long n)
        {
            unchecked
            {
                ulong v = (ulong)n * 6364136223846793005UL + (ulong)seed * 1442695040888963407UL;
                v ^= v >> 29;
                v *= 0xBF58476D1CE4E5B9UL;
                v ^= v >> 32;
                return (uint)v;
            }
        }

        public void Minute(long minute) => Mix(minute);

        public void Hour(long hour)
        {
            // the world's hour reads where the boat is (the world's ships keep clear of her): a boat moved by the frame
            // and not by the step would be somewhere else here at speed 5
            Mix(hour);
            Mix((long)Math.Round(x * 1000.0));
            Mix((long)Math.Round(z * 1000.0));
            if (Dice(seed, hour) % 37 == 0)
            {
                cards.Add(hour * 60);
                Mix(hour * 31 + 5);                               // (what the card did is in the world)
                card = true;
            }
            if (Dice(seed, hour + 7777) % 53 == 0)
            {
                drops.Add(hour * 60);
                drop = true;
            }
        }

        public void Day(int day) => Mix(day * 1000003L);
        public void Year(int year) => Mix(year * 7919L);
        public void Jumped(double fromHours, double toHours) => Mix(-1);
        public void Reversed(double fromHours, double toHours) => Mix(-2);

        /// <summary>One frame of `realSeconds`.</summary>
        public void Frame(double realSeconds)
        {
            frames++;
            if (hold > 0)
            {
                stepper.Take(realSeconds, 0.0);                   // (a card holds the clock: nothing runs, nothing is owed)
                if (--hold == 0)
                    level = wanted;                               // (answered: the player picks the speed again)
                return;
            }
            int want = stepper.Take(realSeconds, Rates[level]);
            int ran = 0;
            for (int i = 0; i < want && stepper.Minute < until; i++)
            {
                long minute = stepper.Step();
                ran++;
                double a = (minute % 360) * Math.PI / 180.0;      // she sails the minute (a heading that turns with it)
                x += Math.Sin(a) * 123.6;
                z += Math.Cos(a) * 123.6;
                double to = minute / 60.0;
                ladder.Advance(lastHours, to, false, this);
                lastHours = to;
                if (card)
                {
                    card = drop = false;
                    hold = answerFrames;
                    stepper.Stop();
                    break;
                }
                if (drop)
                {
                    drop = false;
                    level = 0;                                    // (the clock drops back to speed 1 by itself...)
                    hold = 2;                                     // (...and two frames on the player has it back)
                    stepper.Stop();
                    break;
                }
            }
            if (stepper.Minute >= until)
                stepper.Stop();
            mostSteps = Math.Max(mostSteps, ran);
        }

        /// <summary>Runs to a game minute with the frames a pattern gives, and tells how it ended.</summary>
        public static ClockRun To(long minute, int seed, int level, double[] frameSeconds, int answerFrames)
        {
            var run = new ClockRun(seed, level, answerFrames) { until = minute };
            for (int i = 0; run.stepper.Minute < minute; i++)
            {
                if (i > 400000000)
                    throw new InvalidOperationException("the clock does not get there");
                run.Frame(frameSeconds[i % frameSeconds.Length]);
            }
            return run;
        }

        /// <summary>Frames as a real machine gives them: sixty a second, thirty, a hundred and forty-four, a half-second
        /// hitch, a few short ones.</summary>
        public static readonly double[] Ragged = { 1.0 / 60.0, 1.0 / 30.0, 1.0 / 144.0, 0.5, 1.0 / 60.0, 0.004, 0.05, 1.0 / 60.0, 0.011 };
        public static readonly double[] Even = { 1.0 / 60.0 };
    }

#if !STEPCHECK
    /// <summary>Ruling 26 of 7 October 2026: the clock's step. Nothing may be skipped or lost at speed 5 that would have
    /// happened at speed 1.</summary>
    public class ClockStepperTests
    {
        const long TwentyDays = 20L * 24 * 60;

        [Test]
        public void TheSameSeedAtSpeedOneAndAtSpeedFiveEndsTheSame()
        {
            var slow = ClockRun.To(TwentyDays, 793, 0, ClockRun.Even, 90);
            var fast = ClockRun.To(TwentyDays, 793, 4, ClockRun.Ragged, 7);
            Assert.AreEqual(slow.stepper.Minute, fast.stepper.Minute, "the minute they ended on");
            Assert.AreEqual(slow.hash, fast.hash, "the world");
            Assert.AreEqual(slow.x, fast.x, "her boat, east");
            Assert.AreEqual(slow.z, fast.z, "her boat, north");
            CollectionAssert.AreEqual(slow.cards, fast.cards, "the minutes the cards came on");
            CollectionAssert.AreEqual(slow.drops, fast.drops, "the minutes the clock dropped on");
            Assert.Greater(slow.cards.Count, 5, "the test met some cards");
            Assert.LessOrEqual(slow.mostSteps, 1, "speed 1 at sixty frames a second never runs two steps in a frame");
            Assert.Greater(fast.mostSteps, 5, "speed 5 runs several steps a frame");
            Assert.Less(fast.frames, slow.frames / 50, "and gets there in far fewer frames");
        }

        [Test]
        public void EverySpeedEndsTheSame()
        {
            var first = ClockRun.To(TwentyDays / 4, 805, 0, ClockRun.Even, 30);
            for (int level = 1; level < ClockRun.Rates.Length; level++)
            {
                var run = ClockRun.To(TwentyDays / 4, 805, level, ClockRun.Ragged, 3 + level);
                Assert.AreEqual(first.hash, run.hash, "speed " + (level + 1));
                CollectionAssert.AreEqual(first.cards, run.cards, "speed " + (level + 1));
            }
            Assert.AreNotEqual(first.hash, ClockRun.To(TwentyDays / 4, 806, 0, ClockRun.Even, 30).hash, "another seed is another world");
        }

        [Test]
        public void AHitchIsNotAJump()
        {
            var s = new ClockStepper();
            // ten real seconds in one frame at speed 5 count as a tenth of a second: 36 game minutes, not 3600
            Assert.AreEqual(36, s.Take(10.0, 21600.0), "a hitch counts as the longest frame");
            // frames whose steps are not run (the machine cannot keep up) do not pile up without end
            Assert.AreEqual(ClockStepper.MostSteps, s.Take(10.0, 21600.0), "what is owed stops at the most steps of a frame");
            Assert.AreEqual(ClockStepper.MostSteps, s.Take(10.0, 21600.0));
            for (int i = 0; i < ClockStepper.MostSteps; i++)
                s.Step();
            Assert.AreEqual(0, s.Take(0.0, 21600.0), "and when they are run nothing more is owed");
        }

        [Test]
        public void AStoppedClockOwesNothing()
        {
            var s = new ClockStepper();
            Assert.AreEqual(3, s.Take(1.0 / 120.0, 21600.0), "half a frame at speed 5 is three game minutes");
            Assert.AreEqual(0, s.Take(1.0, 0.0), "held");
            Assert.AreEqual(0, s.Take(1.0 / 60.0, 60.0), "and going on at speed 1 there is no burst");
            Assert.AreEqual(0L, s.Minute);
        }

        [Test]
        public void SpeedOneIsAGameMinuteARealSecond()
        {
            var s = new ClockStepper();
            for (int frame = 0; frame < 60 * 60 * 3; frame++)          // three real minutes at sixty frames a second
                for (int n = s.Take(1.0 / 60.0, 60.0); n > 0; n--)
                    s.Step();
            Assert.AreEqual(180L, s.Minute);
        }

        [Test]
        public void TheFiveSpeedsAreAsRuled()
        {
            var r = Kapman.Game.GameSpeed.Rates;
            Assert.AreEqual(5, r.Length);
            Assert.AreEqual(60.0, r[0], "speed 1: a game minute a real second");
            Assert.AreEqual(4.0, r[1] / r[0], "speed 2: four times that");
            Assert.AreEqual(60.0, 86400.0 / r[2], "speed 3: a day in a minute");
            Assert.AreEqual(15.0, 86400.0 / r[3], "speed 4: a day in fifteen seconds");
            Assert.AreEqual(4.0, 86400.0 / r[4], "speed 5: a day in four seconds");
        }

        [Test]
        public void TheClockDropsBackAndOneKeyGoesBack()
        {
            Kapman.Game.GameSpeed.Multiplayer = false;
            Kapman.Game.GameSpeed.SetLevel(4);
            try
            {
                Assert.IsTrue(Kapman.Game.GameSpeed.DropTo(1, "nearing port"));
                Assert.AreEqual(1, Kapman.Game.GameSpeed.Level);
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Before, "the speed she ran at is kept");
                Assert.IsTrue(Kapman.Game.GameSpeed.DropTo(0, "moored"), "a second drop");
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Before, "keeps the first speed to go back to");
                Assert.IsFalse(Kapman.Game.GameSpeed.DropTo(2, "a sail"), "a drop never raises the speed");
                Kapman.Game.GameSpeed.Faster();
                Assert.AreEqual(4, Kapman.Game.GameSpeed.Level, "one key: back to the speed she had");
                Assert.AreEqual(-1, Kapman.Game.GameSpeed.Before);
                Kapman.Game.GameSpeed.Slower();
                Assert.AreEqual(3, Kapman.Game.GameSpeed.Level);
            }
            finally
            {
                Kapman.Game.GameSpeed.SetLevel(0);
            }
        }
    }
#else
    public static class StepCheck
    {
        public static int Main()
        {
            const long twentyDays = 20L * 24 * 60;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var slow = ClockRun.To(twentyDays, 793, 0, ClockRun.Even, 90);
            var fast = ClockRun.To(twentyDays, 793, 4, ClockRun.Ragged, 7);
            bool same = slow.hash == fast.hash && slow.x == fast.x && slow.z == fast.z && slow.stepper.Minute == fast.stepper.Minute
                        && slow.cards.Count == fast.cards.Count && slow.drops.Count == fast.drops.Count;
            for (int i = 0; same && i < slow.cards.Count; i++)
                same = slow.cards[i] == fast.cards[i];
            Console.WriteLine($"speed 1: {slow.frames} frames, most steps a frame {slow.mostSteps}, hash {slow.hash:X16}, cards {slow.cards.Count}, drops {slow.drops.Count}");
            Console.WriteLine($"speed 5: {fast.frames} frames, most steps a frame {fast.mostSteps}, hash {fast.hash:X16}, cards {fast.cards.Count}, drops {fast.drops.Count}");
            Console.WriteLine(same ? "THE SAME WORLD" : "NOT THE SAME");
            var first = ClockRun.To(twentyDays / 4, 805, 0, ClockRun.Even, 30);
            for (int level = 1; level < ClockRun.Rates.Length; level++)
            {
                var run = ClockRun.To(twentyDays / 4, 805, level, ClockRun.Ragged, 3 + level);
                bool ok = run.hash == first.hash && run.cards.Count == first.cards.Count;
                Console.WriteLine($"speed {level + 1}: {run.frames} frames, most {run.mostSteps}: {(ok ? "the same" : "NOT THE SAME")}");
                same &= ok;
            }
            var other = ClockRun.To(twentyDays / 4, 806, 0, ClockRun.Even, 30);
            Console.WriteLine("another seed: " + (other.hash != first.hash ? "another world" : "THE SAME (wrong)"));
            var s = new ClockStepper();
            int hitch = s.Take(10.0, 21600.0);
            int piled = s.Take(10.0, 21600.0);
            Console.WriteLine($"a ten second hitch at speed 5 wants {hitch} steps; a second one unrun, {piled} (most {ClockStepper.MostSteps})");
            var one = new ClockStepper();
            for (int frame = 0; frame < 60 * 60 * 3; frame++)
                for (int n = one.Take(1.0 / 60.0, 60.0); n > 0; n--)
                    one.Step();
            Console.WriteLine($"three real minutes at speed 1, sixty frames a second: {one.Minute} game minutes");
            Console.WriteLine($"{watch.ElapsedMilliseconds} ms");
            return same && hitch == 36 && piled == ClockStepper.MostSteps && one.Minute == 180 && other.hash != first.hash ? 0 : 1;
        }
    }
#endif
}
