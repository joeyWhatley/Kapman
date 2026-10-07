using System.Collections.Generic;
using Kapman.Chain;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The carried state survives the JSON writer and reader field for field, and moves with the cut.</summary>
    public class LevelCarryTests
    {
        static LevelCarry Sample() => new LevelCarry
        {
            levelId = "dorestad",
            x = 2731.123456789, z = 3398.987654321, heading = 278.8,
            speed = 1.6, pitch = -0.7, roll = 2.25,
            velocityX = -1.58, velocityZ = 0.245,
            hasGoal = true, goalX = 2690.5, goalZ = 3401.25,
            gameHours = 1234.5678, timeSpeed = 4.0, paused = true,
            weatherKind = "Rain", weatherManual = true,
            windX = -3.5, windZ = 6.125, temperature = 7.4, snowLying = 0.15, aurora = true,
            routeId = "dorestad_lundenwic", realKm = 1.71,
        };

        [Test]
        public void RoundTrip()
        {
            var a = Sample();
            string text = Json.Write(a.Capture());
            var b = LevelCarry.Restore(Json.Parse(text));
            Assert.AreEqual(a.levelId, b.levelId);
            Assert.AreEqual(a.x, b.x);
            Assert.AreEqual(a.z, b.z);
            Assert.AreEqual(a.heading, b.heading);
            Assert.AreEqual(a.speed, b.speed);
            Assert.AreEqual(a.pitch, b.pitch);
            Assert.AreEqual(a.roll, b.roll);
            Assert.AreEqual(a.velocityX, b.velocityX);
            Assert.AreEqual(a.velocityZ, b.velocityZ);
            Assert.AreEqual(a.hasGoal, b.hasGoal);
            Assert.AreEqual(a.goalX, b.goalX);
            Assert.AreEqual(a.goalZ, b.goalZ);
            Assert.AreEqual(a.gameHours, b.gameHours);
            Assert.AreEqual(a.timeSpeed, b.timeSpeed);
            Assert.AreEqual(a.paused, b.paused);
            Assert.AreEqual(a.weatherKind, b.weatherKind);
            Assert.AreEqual(a.weatherManual, b.weatherManual);
            Assert.AreEqual(a.windX, b.windX);
            Assert.AreEqual(a.windZ, b.windZ);
            Assert.AreEqual(a.temperature, b.temperature);
            Assert.AreEqual(a.snowLying, b.snowLying);
            Assert.AreEqual(a.aurora, b.aurora);
            Assert.AreEqual(a.routeId, b.routeId);
            Assert.AreEqual(a.realKm, b.realKm);
            Assert.AreEqual(text, Json.Write(b.Capture()), "written again, the same text");

            // an older save without the block's newer fields still loads, with the defaults
            var old = LevelCarry.Restore(new Dictionary<string, object> { ["level"] = "northsea_south", ["x"] = 10.0 });
            Assert.AreEqual("northsea_south", old.levelId);
            Assert.AreEqual(10.0, old.x);
            Assert.AreEqual(1.0, old.timeSpeed);
            Assert.IsFalse(old.hasGoal);

            // across the real mouth: the pose, the goal and the way move; the clock and the weather do not
            var d = ChainSamples.Real();
            var mouth = d.Join("rhine_mouth");
            var t = ChainData.Transform(mouth, mouth.a);
            var c = a.MovedBy(t, "northsea_south");
            Assert.AreEqual("northsea_south", c.levelId);
            var (gx, gz) = t.Apply(a.goalX, a.goalZ);
            Assert.AreEqual(gx, c.goalX, 1e-9);
            Assert.AreEqual(gz, c.goalZ, 1e-9);
            Assert.AreEqual(System.Math.Sqrt(a.velocityX * a.velocityX + a.velocityZ * a.velocityZ),
                            System.Math.Sqrt(c.velocityX * c.velocityX + c.velocityZ * c.velocityZ), 1e-9, "her way keeps its speed");
            Assert.AreEqual(a.gameHours, c.gameHours);
            Assert.AreEqual(a.weatherKind, c.weatherKind);
        }
    }
}
