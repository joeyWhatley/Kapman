using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Aground (Code/Game/Voyage/Aground.cs): its numbers come from the voyage rules' table. (The getting off
    /// itself needs the land, the tide and the ship: it's tried in Play.)</summary>
    public class AgroundTests
    {
        [Test]
        public void TheRulesComeFromTheTable()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/voyage_rules.json");
            Assert.IsNotNull(json);
            var r = VoyageRules.Load(json);
            var table = Json.Obj(Json.Parse(json.text), "aground");
            Assert.IsNotNull(table, "voyage_rules.json has an 'aground' section");
            Assert.AreEqual(Json.Num(table, "offer_after_minutes"), r.agroundOfferMinutes);
            Assert.AreEqual(Json.Num(table, "waiting_rate"), r.tideWaitRate);
            Assert.AreEqual(Json.Num(table, "push_reach_lengths"), r.pushReachLengths);
            Assert.Greater(r.tideWaitRate, 60.0, "waiting runs the clock faster than the game's own rate");
            Assert.Greater(r.pushMargin, 1.0, "they push her to more water than she needs");
        }
    }
}
