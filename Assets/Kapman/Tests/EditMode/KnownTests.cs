using System.Collections.Generic;
using System.IO;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>What the merchant knows (Plans/map_play.md: "only what you know"): the price notes keep their history; a
    /// told price is off by how it came, the same every time; belief fades toward what every merchant knows; it all saves
    /// and loads; and none of it touches the world's markets or changes what the world does.</summary>
    public class KnownTests
    {
        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Known.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/knowing_793.json"));
            Markets.Clear();
            Known.Clear();
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Known.Clear();
        }

        [Test]
        public void ThePriceNotesKeepTheirHistory()
        {
            var ledger = new Ledger();
            var m = new Market(GameData.Port("dorestad"), TradeRules.Current, 793, 1.0);
            ledger.Note(m, 1.0, 6);
            ledger.Note(m, 1.2, 6);                                          // (the same day again: renewed, not added)
            Assert.IsTrue(ledger.Hear(PriceSnapshot.Of(m, 6, 5.0), "talk", How.Talk, 1, 6.0), "newer: the newest now");
            Assert.IsFalse(ledger.Hear(PriceSnapshot.Of(m, 6, 3.0), "talk", How.Talk, 1, 6.0), "older: not the newest...");
            var h = ledger.History("dorestad");
            Assert.AreEqual(3, h.Count, "...but kept as history");
            Assert.AreEqual(1.2, h[0].day, 1e-9);
            Assert.AreEqual(3.0, h[1].day, 1e-9);
            Assert.AreEqual(5.0, h[2].day, 1e-9);
            Assert.AreEqual(How.Seen, h[0].how);
        }

        [Test]
        public void AToldPriceIsOffByHowItCameAndTheSameEveryTime()
        {
            var m = new Market(GameData.Port("dorestad"), TradeRules.Current, 793, 1.0);
            var snap = PriceSnapshot.Of(m, 6, 4.0);
            var told = new Ledger();
            told.Hear(snap, "a witness", How.Told, 0, 5.0);
            var talk = new Ledger();
            talk.Hear(snap, "talk", How.Talk, 1, 5.0);
            var talkAgain = new Ledger();
            talkAgain.Hear(snap, "talk", How.Talk, 1, 6.0);
            var rumour = new Ledger();
            rumour.Hear(snap, "a rumour", How.Rumour, 3, 5.0);
            int differs = 0;
            foreach (var g in GameData.Goods)
            {
                if (float.IsNaN(snap.pays[g.index]))
                    continue;
                float truth = snap.pays[g.index];
                Assert.AreEqual(truth, told.Latest("dorestad").prices[g.id].pays, 1e-4f, "a witness: exact");
                float t = talk.Latest("dorestad").prices[g.id].pays;
                Assert.LessOrEqual(Mathf.Abs(t / truth - 1f), Known.ErrorShare(How.Talk, 1) + 1e-4f, "talk: a little off at most");
                Assert.AreEqual(t, talkAgain.Latest("dorestad").prices[g.id].pays, 1e-6f, "the same error every time");
                Assert.LessOrEqual(Mathf.Abs(rumour.Latest("dorestad").prices[g.id].pays / truth - 1f), Known.ErrorShare(How.Rumour, 3) + 1e-4f);
                if (Mathf.Abs(t - truth) > 1e-5f)
                    differs++;
            }
            Assert.Greater(differs, 0, "talk is off for some goods");
        }

        [Test]
        public void BeliefFadesTowardWhatEveryMerchantKnows()
        {
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            var port = GameData.Port("dorestad");
            var m = new Market(port, TradeRules.Current, 793, 10.0);
            Trade.Ledger.Note(m, 10.0, 6);
            GoodDef good = null;
            foreach (var g in m.Goods)
                if (Mathf.Abs(m.Pays(g, 6) - Lore.UsualPays(port, g.good, 6)) > 0.5f)
                {
                    good = g.good;
                    break;
                }
            Assert.IsNotNull(good, "a good whose price today isn't its usual one");
            float seen = Trade.Ledger.Latest(port.id).prices[good.id].pays, usual = Lore.UsualPays(port, good, 6);
            var fresh = Known.Price(port, good, 6, 10.0);
            Assert.AreEqual(seen, fresh.pays, 1e-3f, "fresh: what was seen");
            Assert.AreEqual(1f, fresh.sure, 1e-3f);
            Assert.AreEqual("sure", Known.Words(fresh.sure));
            var month = Known.Price(port, good, 6, 10.0 + Known.Rules.priceHalfLife);
            Assert.AreEqual((seen + usual) * 0.5f, month.pays, 1e-3f, "a half-life on: half-way to the usual");
            Assert.AreEqual(0.5f, month.sure, 1e-3f);
            var nowhere = Known.Price(GameData.Port("birka"), good, 6, 10.0);
            Assert.IsFalse(nowhere.noted);
            Assert.AreEqual(Lore.UsualPays(GameData.Port("birka"), good, 6), nowhere.pays, 1e-4f, "never noted: what everyone knows");
            Trade.Begin(TradeRules.Current, 900f, 0.0);
        }

        [Test]
        public void LoreIsTheMarketsUsualPrice()
        {
            foreach (var id in new[] { "dorestad", "birka", "lundenwic", "kaupang" })
            {
                var port = GameData.Port(id);
                if (port == null)
                    continue;
                var m = new Market(port, TradeRules.Current, 793, 0.0);
                foreach (var g in m.Goods)
                    for (int month = 1; month <= 12; month += 5)
                        Assert.AreEqual(m.UsualPrice(g, month), Lore.UsualPrice(port, g.good, month), 1e-4f, $"{id} {g.good.id} {month}");
            }
        }

        [Test]
        public void WhatIsKnownSavesAndLoads()
        {
            var news = new NewsItem { id = "raid.3.0", kind = "raid", text = "Raiders struck somewhere.", port = 3, day = 4.0, refId = "e793_lindisfarne" };
            Known.HearNews(news, 6.0, "talk on the waterfront", How.Talk, 1, 0);
            var taken = new NewsItem { id = "taken.-1.1", kind = "taken", text = "A ship was taken.", port = -1, day = 5.0, x = 1234.5, z = -987.0 };
            Known.HearNews(taken, 7.0, "heard from a ship", How.Talk, 2);
            var ship = new SimShip { id = "dorestad.1", home = 0, dest = 1, raider = false };
            Known.Spoke(ship, 8.0);
            Known.HearOf(GameData.Port("birka"));
            string saved = Json.Write(Known.Capture());
            Known.Clear();
            Assert.IsFalse(Known.HeardNews("raid.3.0"));
            Known.Restore(Json.Parse(saved) as Dictionary<string, object>);
            Assert.IsTrue(Known.HeardNews("raid.3.0"));
            Assert.IsTrue(Known.HeardOfEntry("e793_lindisfarne"));
            Assert.IsTrue(Known.HasSpoken("dorestad.1"));
            Assert.IsTrue(Known.IsHeardOf(GameData.Port("birka")));
            var back = Known.News().Find(f => f.refId == "taken.-1.1");
            Assert.AreEqual(1234.5, back.x, 1e-9);
            Assert.AreEqual(-987.0, back.z, 1e-9);
            Assert.AreEqual(2, back.hops);
            Assert.AreEqual(How.Talk, back.how);
            var raid = Known.News().Find(f => f.refId == "raid.3.0");
            Assert.IsTrue(double.IsNaN(raid.x), "no place but its port");
        }

        [Test]
        public void ThePriceNotesSaveAndLoadAndOldNotesStillRead()
        {
            var m = new Market(GameData.Port("dorestad"), TradeRules.Current, 793, 1.0);
            var a = new Ledger();
            a.Note(m, 1.0, 6);
            a.Hear(PriceSnapshot.Of(m, 6, 4.0), "a rumour", How.Rumour, 3, 5.0);
            var b = new Ledger();
            b.Restore(Json.Parse(Json.Write(a.Capture())) as List<object>);
            Assert.AreEqual(2, b.History("dorestad").Count);
            Assert.AreEqual(How.Rumour, b.Latest("dorestad").how);
            Assert.AreEqual(3, b.Latest("dorestad").hops);
            Assert.AreEqual(5.0, b.Latest("dorestad").learned, 1e-9);
            // a version-1 note: no how, hops or learned
            var old = new List<object> { new Dictionary<string, object> { ["port"] = "birka", ["day"] = 2.0, ["source"] = "talk", ["prices"] = new Dictionary<string, object>() } };
            var c = new Ledger();
            c.Restore(Json.Parse(Json.Write(old)) as List<object>);
            Assert.AreEqual(How.Talk, c.Latest("birka").how);
            Assert.AreEqual(2.0, c.Latest("birka").learned, 1e-9);
        }

        [Test]
        public void ThinningKeepsTheLastYearAndOneAMonthBefore()
        {
            var m = new Market(GameData.Port("dorestad"), TradeRules.Current, 793, 0.0);
            var l = new Ledger();
            for (int d = 0; d < 800; d += 2)
                l.Hear(PriceSnapshot.Of(m, 6, d), "talk", How.Talk, 1, d);
            l.Thin(800.0, 365.0, 10.0);
            int recent = 0, older = 0;
            foreach (var n in l.History("dorestad"))
                if (800.0 - n.day <= 365.0) recent++; else older++;
            Assert.AreEqual(182, recent, "every note of the last year");
            Assert.LessOrEqual(older, 16, "about one a month before that");
            Assert.Greater(older, 12);
        }

        [Test]
        public void KnowingNeverChangesWhatTheWorldDoes()
        {
            var a = SimHarness.MakeWorld(793, 5.0);
            a.Populate();
            for (int h = 0; h < 10 * 24; h++)
                a.Hour();
            long alone = a.StateHash();

            var b = SimHarness.MakeWorld(793, 5.0);
            b.Populate();
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            for (int h = 0; h < 10 * 24; h++)
            {
                b.Hour();
                if (h % 24 != 12)
                    continue;
                // every day, the merchant hears every waterfront's prices and news, and works out every belief
                for (int p = 0; p < GameData.Ports.Count; p++)
                {
                    foreach (var snap in b.Board(p).All)
                        Trade.Ledger.Hear(snap, "talk", How.Talk, 1, b.Day);
                    foreach (var (item, _, from) in b.Board(p).News)
                        Known.HearNews(item, b.Day, from);
                    foreach (var g in GameData.Goods)
                        Known.Price(GameData.Ports[p], g, b.Date.month, b.Day);
                }
            }
            Assert.AreEqual(alone, b.StateHash(), "the same world, whatever the merchant learns");
            Trade.Begin(TradeRules.Current, 900f, 0.0);
        }

        [Test]
        public void TheKnowingCodeNeverReadsTheHiddenTruth()
        {
            string[] banned = { "Markets.", "MarketAt(", ".stats", "World.ships", "Reputation.At(" };
            foreach (var file in Directory.GetFiles("Assets/Kapman/Code/Game/Known", "*.cs"))
            {
                string text = File.ReadAllText(file);
                foreach (var b in banned)
                    Assert.IsFalse(text.Contains(b), $"{Path.GetFileName(file)} reads {b}");
            }
        }
    }
}
