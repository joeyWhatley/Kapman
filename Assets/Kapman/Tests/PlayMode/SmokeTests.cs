using System.Collections;
using Kapman.Game;
using Kapman.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SMOKE TESTS - each level scene opened in Play with the game in it, run for a few seconds: nothing may throw or
    /// log an error, the clock must run, the ship must float, the game's tables must be loaded and the world sim made.
    /// Run headless: Unity.exe -batchmode -projectPath ... -runTests -testPlatform PlayMode (no -nographics: HDRP renders).
    /// </summary>
    public class SmokeTests
    {
        static IEnumerator Open(string scene, int frames)
        {
            var load = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            Assert.IsNotNull(load, $"{scene} is not in the build settings (the level builders add it)");
            while (!load.isDone)
                yield return null;
            for (int i = 0; i < frames; i++)
                yield return null;
        }

        static void CheckLevel()
        {
            var clock = LevelClock.Current;
            Assert.IsNotNull(clock, "no LevelClock");
            Assert.Greater(clock.GameHours, 0.0, "the clock did not run");
            var ship = ShipMotion.Player;
            Assert.IsNotNull(ship, "no player's ship");
            Assert.IsFalse(float.IsNaN(ship.transform.position.y), "the ship's height is NaN");
            var game = KapmanGame.Current;
            Assert.IsNotNull(game, "no KapmanGame in the scene (GameSetup.AddToScene)");
            Assert.IsNotNull(game.World, "the world sim was not made");
            Assert.IsNotNull(game.ship, "the game has no ship");
            // she floats: at the waterline, upright (the ship body, 4 Oct: gravity, buoyancy, the waves)
            var body = ship.GetComponent<Kapman.Ships.ShipBody>();
            Assert.IsNotNull(body, "the player's ship has no ShipBody (GameSetup.AddShipBody)");
            Assert.IsTrue(body.enabled, "the ShipBody switched itself off (see the log: ships.json or its making)");
            float wl = LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : 0f;
            float y = ship.transform.position.y;
            Assert.That(y, Is.InRange(wl - 2.5f, wl + 2.5f), $"she should float at the waterline ({wl:0.00} m), not at {y:0.00} m");
            float tilt = Vector3.Angle(ship.transform.up, Vector3.up);
            Assert.Less(tilt, 35f, $"she should be upright, not heeled {tilt:0} degrees");
        }

        [UnityTest]
        public IEnumerator DorestadRunsInPlay()
        {
            yield return Open("L_Dorestad", 300);
            CheckLevel();
            // the modelled soundscape runs, and its shore map knows the river from the land
            var sound = Kapman.Levels.LevelSoundscape2.Current;
            Assert.IsNotNull(sound, "no LevelSoundscape2 (SoundscapeSetup.AddTo)");
            Assert.IsTrue(sound.enabled, "the soundscape switched itself off (see the log: soundscape.json)");
            Assert.IsNotNull(sound.shoreMap, "no shore map");
            float river = sound.shoreMap.GetPixelBilinear(3719.64f / sound.shoreSize, 1468.41f / sound.shoreSize).r;
            float field = sound.shoreMap.GetPixelBilinear(0.1f, 0.9f).r;
            Assert.Greater(river, 0f, "the channel by the exit is water on the shore map");
            Assert.Less(field, river, "the shore map rises toward the water");
            StringAssert.Contains("dB", Kapman.Levels.LevelSoundscape2.Loudest, "the soundscape reports its layers");
            // the markets live, with the famine of 793 on: the H panel's line has Dorestad's wheat
            string market = Kapman.Levels.DevHud.GameLine?.Invoke();
            StringAssert.Contains("wheat", market ?? "", "the market line on the H panel");
            Assert.Greater(Kapman.Game.Shocks.Holding, 0, "the chronicle's famine is held at the start");
            // the voyage's beats run: the first (casting off) is offered
            var director = Kapman.Game.VoyageDirector.Current;
            Assert.IsNotNull(director, "no VoyageDirector (GameSetup.AddToScene)");
            Assert.IsNotNull(director.Scheduler, "the director did not start");
            Assert.GreaterOrEqual(director.Scheduler.log.Offered, 1, "casting off is offered");
        }

        [UnityTest]
        public IEnumerator NorthSeaRunsInPlay()
        {
            yield return Open("L_NorthSea", 300);
            CheckLevel();
        }
    }
}
