using System.Linq;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The law: swords bound out of the Frankish realm are under it, bound within it they aren't; the 864 rule;
    /// a full hold hides more; the world's traders are searched leaving port.</summary>
    public class PatrolTests
    {
        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Patrols.Rules = PatrolRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/patrols_793.json"));
        }

        [Test]
        public void SwordsBoundOutOfTheRealmAreUnderTheLaw()
        {
            var hold = new Hold { capacity = 900f };
            hold.Add(GameData.Good("swords"), 3, 30.0, 0.0);
            hold.Add(GameData.Good("wheat"), 5, 10.0, 0.0);
            var dorestad = GameData.Port("dorestad");
            var zone = TradeRules.Current.ZoneOf(dorestad.region);
            Assert.IsTrue(Patrols.BoundOut(dorestad, GameData.Port("ripa")), "Dorestad to Ribe leaves the realm");
            Assert.IsFalse(Patrols.BoundOut(dorestad, GameData.Port("quentovic")), "Dorestad to Quentovic doesn't");
            var outward = Patrols.UnderLaw(hold, zone, 793, boundOut: true);
            Assert.AreEqual(1, outward.Count, "the swords");
            Assert.AreEqual("swords", outward[0].good.id);
            Assert.AreEqual(0, Patrols.UnderLaw(hold, zone, 793, boundOut: false).Count, "within the realm: no offence");
            Assert.IsTrue(Patrols.UnderLaw(hold, zone, 870, boundOut: true).Any(l => l.law.rule == "death"), "870: the 864 rule");
            Assert.Greater(Patrols.Rules.Strictness(dorestad), Patrols.Rules.Strictness(GameData.Port("gipeswic")), "Dorestad searches hardest");
            Assert.AreEqual(0.0, Patrols.Rules.Strictness(GameData.Port("birka")), "no searches in the north");
        }

        [Test]
        public void AFullHoldHidesMore()
        {
            var empty = new Hold { capacity = 900f };
            var full = new Hold { capacity = 900f };
            full.Add(GameData.Good("wheat"), full.RoomFor(GameData.Good("wheat")), 10.0, 0.0);
            Assert.AreEqual(0.0, Patrols.Cover(empty), 1e-6);
            Assert.Greater(Patrols.Cover(full), Patrols.Rules.coverMax * 0.9, "a hold full to the last whole sack hides nearly the most");
            Assert.LessOrEqual(Patrols.Cover(full), Patrols.Rules.coverMax + 1e-9);
        }

        [Test]
        public void TheWorldsTradersAreSearched()
        {
            SimHarness.RunYear(793, 60, out var sim, false);
            Assert.Greater(sim.stats.searches, 10, "traders searched leaving port");
        }
    }
}
