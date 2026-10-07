using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The encounters in play without the scene (npc_text_encounters.md 5, night one, done-when 2): the runner's logic
    /// (EncounterDirector) moored at Dorestad's berth from chain.json. Lying there when the game starts is no arrival;
    /// sailing in and lying still opens the toll man; a gift pays 12 d, costs standing, eases the toll and is remembered;
    /// moving along the dam heads is the same call; after casting off and coming back the same man greets you again with
    /// "declare" as the one-key choice; another merchant meets the same man with no feeling either way; a card left waiting
    /// goes when she casts off; the state saves and loads byte for byte; and the real GameEffects pays from the purse.
    /// </summary>
    public class EncounterRunnerTests
    {
        const string Toll = "harbour.tollman.frankish";

        static Kapman.Chain.ChainLevel Dorestad() =>
            Kapman.Chain.ChainData.Load(Kapman.Chain.ChainData.ProjectPath).Level("dorestad");

        /// <summary>A director at Dorestad with the fake game: the Frankish realm, the tenth, an empty hold, 100 d.</summary>
        static (EncounterDirector d, FakeGame game) Director(NpcRegistry npcs = null, string merchant = "player")
        {
            var game = new FakeGame();
            // the toll man alone, as on night one: the game has hundreds of cards now, and these tests are of this one
            var d = new EncounterDirector(EncounterData.BookOf(Toll), 793, game, npcs) { merchant = merchant };
            d.portInfo = port => port == "dorestad" ? ("franks", "frisian") : ("", "");
            d.roll = () => 0.0;
            d.vars.nums["port.toll"] = 0.1;
            d.vars.nums["hold.value"] = 0.0;
            d.vars.nums["crew.trade_best"] = 3.0;
            d.vars.numSource = name => name == "silver" ? game.silver : 0.0;
            Assert.AreEqual(1, d.watch.AddBerths(Dorestad(), 40.0), "chain.json gives Dorestad one berth");
            d.SetTime(10.0, 793, 6, 240);
            return (d, game);
        }

        static MooringPoint Berth(EncounterDirector d) => d.watch.points[0];

        /// <summary>Under way toward a place, then lying still there (half-second looks) until the watch says something.</summary>
        static MooringWatch.Change SailInAndLie(EncounterDirector d, double x, double z)
        {
            var c = d.Look(x + 60.0, z, true, 1.6, 0.5);
            if (c != MooringWatch.Change.None)
                return c;
            for (int i = 0; i < 20; i++)
            {
                c = d.Look(x, z, false, 0.0, 0.5);
                if (c != MooringWatch.Change.None)
                    return c;
            }
            return MooringWatch.Change.None;
        }

        static List<string> LineKeys(EncounterSession s) => s.lines.ConvertAll(l => l.def.key);

        [Test]
        public void LyingAtTheBerthWhenTheGameStartsIsNoArrival()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            for (int i = 0; i < 20; i++)
                Assert.AreEqual(MooringWatch.Change.None, d.Look(berth.x, berth.z, false, 0.0, 0.5));
            Assert.IsFalse(d.InCall);
            Assert.IsNull(d.OpenNext(), "no card: she has not come from anywhere");
        }

        [Test]
        public void TheFirstMooringAtDorestadOpensTheTollMan()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            Assert.AreEqual(MooringWatch.Change.Moored, SailInAndLie(d, berth.x + 10.0, berth.z));
            Assert.AreEqual("dorestad", d.state.callPort);
            Assert.AreEqual(1, d.state.Visits("dorestad"));
            var s = d.OpenNext();
            Assert.IsNotNull(s, "the toll man opens on the first arrival");
            Assert.AreEqual(Toll, s.card.id);
            Assert.IsFalse(s.returning);
            Assert.AreEqual(-1, s.DefaultIndex, "no one-key choice on a first call");
            var keys = LineKeys(s);
            CollectionAssert.Contains(keys, "tollman.greet");
            CollectionAssert.DoesNotContain(keys, "tollman.greet_again");
            CollectionAssert.Contains(keys, "tollman.law_mail");
            CollectionAssert.Contains(keys, "tollman.ask_empty");
            Assert.AreEqual("dorestad.tollman.0", s.actors["tollman"].id);
            Assert.IsTrue(s.options[s.card.Option("declare").index].enabled);
            Assert.IsTrue(s.options[s.card.Option("gift").index].enabled, "100 d aboard and he is easy with you");
            Assert.IsFalse(s.options[s.card.Option("hide").index].enabled, "nothing to hide");
            Assert.IsFalse(s.options[s.card.Option("charter").index].enabled, "no charter in the chest");
            Assert.IsFalse(d.Dismiss(), "Esc is refused on the toll man");
            Assert.AreSame(s, d.Current);
            Assert.IsNull(d.OpenNext(), "one card at a time");
        }

        [Test]
        public void AGiftPaysTwelveEasesTheTollAndIsRemembered()
        {
            var (d, game) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            var s = d.OpenNext();
            var r = d.Choose(s.card.Option("gift").index);
            Assert.IsTrue(r.ok, r.Text);
            CollectionAssert.Contains(game.calls, "silver -12");
            Assert.AreEqual(88.0, game.silver, 1e-9);
            CollectionAssert.Contains(game.deeds, ("bribed_officer", "dorestad"));
            string toll = s.actors["tollman"].id;
            Assert.AreEqual(8.0, d.npcs.Feeling(toll, "player"), 1e-9);
            Assert.AreEqual(1, d.npcs.Rel(toll, "player").memory.Count, "one memory of it");
            Assert.AreEqual("gift", d.npcs.LastOption(toll, "player"));
            Assert.AreEqual(0.5, d.TollFactor("dorestad", 10.0), 1e-9, "half the toll for 30 days");
            Assert.AreEqual(1.0, d.TollFactor("dorestad", 40.0), 1e-9);
            Assert.IsNull(d.Choose(0), "answered once");
            Assert.IsTrue(d.Continue());
            Assert.IsNull(d.Current);
        }

        [Test]
        public void DeclaringTakesTheTollOnSales()
        {
            var (d, game) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            var s = d.OpenNext();
            var r = d.Choose(s.card.Option("declare").index);
            Assert.IsTrue(r.ok, r.Text);
            CollectionAssert.Contains(r.facts, "The toll is taken on what you sell here.");
            Assert.IsEmpty(game.calls, "declaring touches nothing in the live game");
            Assert.AreEqual(1.0, d.TollFactor("dorestad", 10.0), 1e-9);
            Assert.AreEqual(2.0, d.npcs.Feeling(s.actors["tollman"].id, "player"), 1e-9);
        }

        [Test]
        public void TheSecondArrivalMeetsTheSameManWithDeclareAsTheDefault()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            var first = d.OpenNext();
            var toll = first.actors["tollman"];
            d.Choose(first.card.Option("gift").index);
            d.Continue();

            // along the dam heads: the same call, nothing more opens
            Assert.AreEqual(MooringWatch.Change.None, SailInAndLie(d, berth.x + 20.0, berth.z));
            Assert.IsNull(d.OpenNext());
            // down the river, out of the harbour: the call ends
            Assert.AreEqual(MooringWatch.Change.CastOff, d.Look(berth.x + 1000.0, berth.z, true, 1.6, 0.5));
            Assert.IsFalse(d.InCall);

            d.SetTime(12.0, 793, 6, 288);
            Assert.AreEqual(MooringWatch.Change.Moored, SailInAndLie(d, berth.x, berth.z));
            var again = d.OpenNext();
            Assert.IsNotNull(again);
            Assert.AreEqual(Toll, again.card.id);
            Assert.AreEqual(toll.id, again.actors["tollman"].id, "the same man");
            Assert.AreEqual(toll.name, again.actors["tollman"].name);
            Assert.IsTrue(again.returning);
            var keys = LineKeys(again);
            CollectionAssert.Contains(keys, "tollman.greet_again");
            CollectionAssert.Contains(keys, "tollman.memory_gift");
            CollectionAssert.DoesNotContain(keys, "tollman.greet");
            Assert.AreEqual(again.card.Option("declare").index, again.DefaultIndex, "Enter: declare, as last time");
            var r = d.ChooseDefault();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.ok, r.Text);
            Assert.AreEqual(10.0, d.npcs.Feeling(toll.id, "player"), 1e-9, "8 for the gift, 2 for declaring");
            Assert.AreEqual(0.5, d.TollFactor("dorestad", 12.0), 1e-9, "declaring keeps the ease he gave");
        }

        [Test]
        public void AnotherMerchantMeetsTheSameManWithNoFeeling()
        {
            var (a, _) = Director();
            var berth = Berth(a);
            SailInAndLie(a, berth.x, berth.z);
            var s = a.OpenNext();
            string toll = s.actors["tollman"].id;
            a.Choose(s.card.Option("gift").index);
            a.Continue();

            var (b, _) = Director(a.npcs, "other");
            Assert.AreEqual(MooringWatch.Change.Moored, SailInAndLie(b, berth.x, berth.z));
            var t = b.OpenNext();
            Assert.AreEqual(toll, t.actors["tollman"].id);
            Assert.AreEqual(0.0, b.npcs.Feeling(toll, "other"), 1e-9);
            Assert.AreEqual(8.0, b.npcs.Feeling(toll, "player"), 1e-9);
        }

        [Test]
        public void ACardLeftWaitingGoesWhenSheCastsOff()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            Assert.IsTrue(d.Waiting, "the toll man waits (another panel was open)");
            Assert.AreEqual(MooringWatch.Change.CastOff, d.Look(berth.x + 1000.0, berth.z, true, 1.6, 0.5));
            Assert.IsFalse(d.Waiting);
            Assert.IsNull(d.OpenNext(), "the toll man does not follow her out");
        }

        [Test]
        public void TheEncounterStateSavesAndLoadsWhole()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            var s = d.OpenNext();
            string toll = s.actors["tollman"].id;
            d.Choose(s.card.Option("gift").index);
            d.Continue();
            string saved = Json.Write(d.Capture());

            var (e, _) = Director();
            e.Restore(Json.Parse(saved) as Dictionary<string, object>);
            Assert.AreEqual(8.0, e.npcs.Feeling(toll, "player"), 1e-9);
            Assert.AreEqual(1, e.state.Visits("dorestad"));
            Assert.AreEqual("dorestad", e.state.callPort);
            Assert.AreEqual(0.5, e.TollFactor("dorestad", 10.0), 1e-9);
            Assert.AreEqual(saved, Json.Write(e.Capture()), "byte for byte");
        }

        [Test]
        public void AnOpenCardComesBackAfterALoad()
        {
            var (d, _) = Director();
            var berth = Berth(d);
            SailInAndLie(d, berth.x, berth.z);
            Assert.IsNotNull(d.OpenNext());
            string saved = Json.Write(d.Capture());
            var (e, _) = Director();
            e.Restore(Json.Parse(saved) as Dictionary<string, object>);
            var s = e.OpenNext();
            Assert.IsNotNull(s, "the unanswered toll man opens again");
            Assert.AreEqual(Toll, s.card.id);
        }

        [Test]
        public void TheGiftThroughTheRealGameEffectsPaysFromThePurse()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Wording.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/wording_793.json"));
            Reputation.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/reputation_793.json"));
            Markets.Clear();
            try
            {
                Trade.Begin(TradeRules.Current, 900f, 0.0);
                var book = EncounterData.Book();
                var effects = new GameEffects { book = book };
                var d = new EncounterDirector(book, 793, effects);
                effects.portNow = () => d.state.callPort;
                d.portInfo = EncounterRunner.PortInfo;
                d.standingAt = id => Reputation.At(GameData.Port(id));
                EncounterGameVars.Attach(d.vars, () => d.state.callPort);
                d.SetTime(10.0, 793, 6, 240);

                var dorestad = GameData.Port("dorestad");
                var zone = TradeRules.Current.ZoneOf(dorestad.region);
                double purse = Trade.Purse.Worth(zone, TradeRules.Current), standing = Reputation.At(dorestad);
                d.BeginCall("dorestad");
                var s = d.OpenNext();
                Assert.IsNotNull(s, "the toll man opens on the live game's realm, toll and silver");
                Assert.AreEqual(Toll, s.card.id);
                var gift = s.options[s.card.Option("gift").index];
                Assert.IsTrue(gift.enabled, gift.rule);
                var r = d.Choose(gift.def.index);
                Assert.IsTrue(r.ok, r.Text);
                Assert.AreEqual(purse - 12.0, Trade.Purse.Worth(zone, TradeRules.Current), 0.01, "12 d paid in deniers");
                Assert.Less(Reputation.At(dorestad), standing, "a bribe costs standing at Dorestad");
                Assert.AreEqual(0.5, d.TollFactor("dorestad", 10.0), 1e-9);
            }
            finally
            {
                Markets.Clear();
                Reputation.Load(null);                                    // (standing is the game's: don't leave it to other tests)
            }
        }
    }
}
