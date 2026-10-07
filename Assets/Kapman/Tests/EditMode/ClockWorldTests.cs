using System;
using System.Collections.Generic;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// Ruling 26 of 7 October 2026: "prove it by a test that runs the same seed at 1 and at 5". The real world of the game
    /// (its traders, raiders, markets and weather: WorldSim, as KapmanGame makes it) on the clock's step, with a boat of
    /// the player's sailing from Dorestad toward Lundenwic that the world's ships keep clear of. Run once at speed 1 with
    /// even frames and once at speed 5 with ragged ones: the ships, the markets and her boat must end the same.
    /// </summary>
    public class ClockWorldTests
    {
        sealed class Run : ITickSink
        {
            public WorldSim world;
            public readonly ClockStepper stepper = new ClockStepper();
            public readonly List<ulong> marketsByDay = new List<ulong>();
            public WorldXZ boat, perStep;
            public int frames;
            readonly TickLadder ladder = new TickLadder(793, 172);
            double lastHours;

            public void Minute(long minute) { }
            public void Hour(long hour) => world.Hour();
            public void Day(int day) => marketsByDay.Add(Markets.Hash());
            public void Year(int year) { }
            public void Jumped(double fromHours, double toHours) { }
            public void Reversed(double fromHours, double toHours) { }

            public void Frame(double realSeconds, double rate, long until)
            {
                frames++;
                int want = stepper.Take(realSeconds, rate);
                for (int i = 0; i < want && stepper.Minute < until; i++)
                {
                    long minute = stepper.Step();
                    boat = new WorldXZ(boat.x + perStep.x, boat.z + perStep.z);      // she sails the minute
                    double to = minute / 60.0;
                    ladder.Advance(lastHours, to, false, this);                      // the hour: the world's ships, by where she is
                    lastHours = to;
                }
                if (stepper.Minute >= until)
                    stepper.Stop();
            }
        }

        static Run Play(int seed, double rate, double[] frameSeconds, int days)
        {
            var run = new Run { world = SimHarness.MakeWorld(seed) };
            var from = GameData.Port("dorestad");
            var to = GameData.Port("lundenwic");
            Assert.IsNotNull(from, "dorestad");
            Assert.IsNotNull(to, "lundenwic");
            var (ax, az) = Kapman.World.WorldFrame.ToWorld(from.lat, from.lon);
            var (bx, bz) = Kapman.World.WorldFrame.ToWorld(to.lat, to.lon);
            double far = Math.Sqrt((bx - ax) * (bx - ax) + (bz - az) * (bz - az));
            const double metresAStep = 2.0 * 60.0;                                   // about four knots
            run.boat = new WorldXZ(ax, az);
            run.perStep = new WorldXZ((bx - ax) / far * metresAStep, (bz - az) / far * metresAStep);
            run.world.playerNow = () => (run.boat, new WorldXZ(run.perStep.x * 60.0, run.perStep.z * 60.0));
            run.world.keepClear = true;                                              // (so her place matters to the world)
            run.world.stats.csvEvery = run.world.stats.sampleEvery = 0;
            run.world.Populate();
            long until = days * 24L * 60L;
            for (int i = 0; run.stepper.Minute < until; i++)
                run.Frame(frameSeconds[i % frameSeconds.Length], rate, until);
            return run;
        }

        [Test]
        public void TheSameSeedAtSpeedOneAndAtSpeedFiveEndsInTheSameWorld()
        {
            const int days = 4;
            var slow = Play(793, GameSpeed.Rates[0], ClockRun.Even, days);
            long slowShips = slow.world.StateHash();
            ulong slowMarkets = Markets.Hash();
            var fast = Play(793, GameSpeed.Rates[4], ClockRun.Ragged, days);
            long fastShips = fast.world.StateHash();
            ulong fastMarkets = Markets.Hash();
            Markets.Clear();

            Assert.AreEqual(slow.stepper.Minute, fast.stepper.Minute, "the minute they ended on");
            Assert.AreEqual(slow.world.Hours, fast.world.Hours, "the world's hours");
            Assert.AreEqual(slowShips, fastShips, "the world's ships");
            Assert.AreEqual(slowMarkets, fastMarkets, "the markets");
            CollectionAssert.AreEqual(slow.marketsByDay, fast.marketsByDay, "the markets, day by day");
            Assert.AreEqual(slow.boat.x, fast.boat.x, "her boat, east");
            Assert.AreEqual(slow.boat.z, fast.boat.z, "her boat, north");
            Assert.AreEqual(days, slow.marketsByDay.Count, "a tick for every day");
            Assert.Less(fast.frames, slow.frames / 50, "speed 5 got there in far fewer frames");
        }
    }
}
