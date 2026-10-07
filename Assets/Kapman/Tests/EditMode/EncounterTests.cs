using System.Collections.Generic;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Meeting a ship at sea: prices heard go into the ledger only where newer, a hove-to ship stays where she
    /// is, and her kind is named without its description.</summary>
    public class EncounterTests
    {
        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Markets.Clear();
        }

        [Test]
        public void HeardPricesGoInOnlyWhereNewer()
        {
            var ledger = new Ledger();
            var m = new Market(GameData.Port("dorestad"), TradeRules.Current, 793, 3.0);
            ledger.Note(m, 3.0, 6);
            var newer = PriceSnapshot.Of(m, 6, 5.0);
            Assert.IsTrue(ledger.Hear(newer, "heard from a ship"), "newer: noted");
            var older = PriceSnapshot.Of(m, 6, 2.0);
            Assert.IsFalse(ledger.Hear(older, "heard from another"), "older: not");
            foreach (var n in ledger.Notes)
                if (n.port == "dorestad")
                {
                    Assert.AreEqual(5.0, n.day, 1e-9);
                    Assert.AreEqual("heard from a ship", n.source);
                    Assert.Greater(n.prices.Count, 0);
                }
        }

        [Test]
        public void AShipHoveToStaysWhereSheIs()
        {
            var data = new SimData();
            var cls = new ShipClassDef { id = "t", name = "Test ship (a description)", length = 12f, beam = 3f, crew = 4, cargoT = 5,
                                         sailKn = 4, maxKn = 6, hasSail = true, sea = true, uses = new[] { "trade_long" } };
            data.classes[cls.id] = cls;
            Assert.AreEqual("Test ship", SeaWatch.ShortName(cls));
            var w = new WorldSim(data, new SimRules(), new StraightRoutes(), new Kapman.Weather.WeatherModel(793), 1, 793, 172, 10.0);
            w.speedOverride = _ => 2.5;
            var s = new SimShip { index = 0, id = "t.1", cls = cls, home = 0, at = 0, rng = SimRandom.For(1, "t.1"), purse = new Purse(),
                                  hold = new Hold { capacity = cls.CargoKg } };
            w.ships.Add(s);
            w.SetSail(s, new List<WorldXZ> { new WorldXZ(0, 0), new WorldXZ(50000, 0) }, new List<byte> { LaneGraph.Sea }, 1);
            w.Hour();
            var moved = s.pos;
            Assert.Greater(moved.x, 1000.0, "under way");
            s.heaveTo = true;
            w.Hour();
            w.Hour();
            Assert.AreEqual(moved.x, s.pos.x, 1.0, "hove to: she waits");
            s.heaveTo = false;
            w.Hour();
            Assert.Greater(s.pos.x, moved.x + 1000.0, "and sails on");
        }
    }
}
