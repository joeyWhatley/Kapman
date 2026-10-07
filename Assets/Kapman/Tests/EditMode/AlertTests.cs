using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The game's speed and the map's alerts: an urgent alert stops the game till it's answered (going on answers
    /// it); any other brings it back to 1x; multiplayer never pauses or speeds up.</summary>
    public class AlertTests
    {
        [SetUp]
        [TearDown]
        public void Clean()
        {
            while (Alerts.Open.Count > 0)
                Alerts.Drop(Alerts.Open[0].id);
            GameSpeed.Multiplayer = false;
            GameSpeed.SetLevel(0);
            if (GameSpeed.Paused)
                GameSpeed.TogglePause();
        }

        [Test]
        public void AnUrgentAlertStopsTheGameTillAnswered()
        {
            GameSpeed.SetLevel(2);
            bool answered = false;
            Alerts.Raise("t.raiders", "raiders", "Raiders.", true, () => answered = true);
            Assert.IsTrue(GameSpeed.Paused, "stopped");
            Assert.AreEqual(0f, GameSpeed.Factor);
            GameSpeed.TogglePause();                                     // going on...
            Assert.IsTrue(answered, "...answers it");
            Assert.IsFalse(GameSpeed.Paused);
            Assert.AreEqual(0, Alerts.Open.Count);
        }

        [Test]
        public void AnyOtherAlertSlowsTheGameTo1x()
        {
            GameSpeed.SetLevel(2);
            Assert.AreEqual(4f, GameSpeed.Factor);
            Alerts.Raise("t.ship", "ship", "A ship.", false, null);
            Assert.IsFalse(GameSpeed.Paused);
            Assert.AreEqual(1f, GameSpeed.Factor);
            Alerts.Raise("t.ship", "ship", "A ship.", false, null);
            Assert.AreEqual(1, Alerts.Open.Count, "the same alert once");
        }

        [Test]
        public void MultiplayerNeverPausesOrSpeedsUp()
        {
            GameSpeed.Multiplayer = true;
            GameSpeed.SetLevel(2);
            GameSpeed.TogglePause();
            GameSpeed.Hold("menu");
            Assert.IsFalse(GameSpeed.Paused);
            Assert.AreEqual(1f, GameSpeed.Factor);
            GameSpeed.Release("menu");
        }
    }
}
