using System.Collections.Generic;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE OARS (the alpha's row 3: "sail or oars"; the user's ruling 8 of 7 Oct 2026: "Oars (out oars, or sail)").
    /// Left to herself she goes as she always has. By the order Out oars the sail comes down and the men row hard:
    /// her class's full speed under oars whatever the wind, less into a head wind, nothing in a gale, for as long as
    /// their strength lasts. It eats bread and strength, and the strength comes back only with rest.
    /// </summary>
    public class OarsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        VoyageRules rules;

        /// <summary>The Gislinge boat, as the class table has her (six oars, six hours of rowing in the men).</summary>
        static ShipClassDef Boat() => new ShipClassDef
        {
            id = "gislinge", name = "Gislinge boat", sailKn = 4.8f, maxKn = 6.5f, oarsKn = 3.5f, rowingHours = 6f, closestToWind = 58f, polar = 1f,
            hasSail = true, crew = 4f,
        };

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ports_793.json"));
            var trade = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "trade_rules.json"));
            rules = VoyageRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "voyage_rules.json"));
            Trade.Begin(trade, 900f, 0.0);
            Voyage.Begin(rules);
            Oars.Reset();
        }

        [TearDown]
        public void Done() => Oars.Reset();

        [Test]
        public void TheRulesAreInTheTable()
        {
            Assert.AreEqual(18.0, rules.oarsRestHours);
            Assert.AreEqual(0.5, rules.oarsBread);
            Assert.AreEqual(1.0, rules.oarsLeastHours);
            Assert.AreEqual(0.5, rules.oarsHeadWind);
            Assert.Less(rules.oarsSpentMorale, 0.0);
            // and the boat she sails has oars to order out
            var world = AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ship_classes_793.json");
            Assert.IsNotNull(world);
            StringAssert.Contains("\"speed_oars_kn\"", world.text);
            StringAssert.Contains("\"rowing_hours\"", world.text);
        }

        [Test]
        public void OutOarsSheRowsHardWhateverTheWind()
        {
            var c = Boat();
            double full = 3.5 * SimUnits.KnotMs;
            // a calm: under the order her full speed under oars; left to herself only the easy stroke
            Assert.AreEqual(full, Oars.Speed(c, 0.0, 0.0, 0.0, out bool lying), 1e-9);
            Assert.IsFalse(lying);
            double easy = Travel.Speed(c, 0.0, 0.0, 0.0, out _);
            Assert.AreEqual(3.5 * (6.0 / 12.0) * SimUnits.KnotMs, easy, 1e-9, "left to herself in a calm she should go at the easy stroke");
            Assert.Greater(Oars.Speed(c, 0.0, 0.0, 0.0, out _), easy * 1.9, "the hard pull is no faster than the easy stroke");
            // dead into a light wind: faster than beating; less than in a calm
            double into = Oars.Speed(c, 0.0, 0.0, 4.0, out _);
            Assert.Less(into, full);
            Assert.Greater(into, Travel.Speed(c, 0.0, 0.0, 4.0, out _), "against a light head wind the oars should beat tacking");
            // the wind abeam or aft takes nothing from the oars
            Assert.AreEqual(full, Oars.Speed(c, 0.0, 90.0, 12.0, out _), 1e-9);
            Assert.AreEqual(full, Oars.Speed(c, 0.0, 180.0, 12.0, out _), 1e-9);
            // dead into a strong wind: half is lost at a gale's strength, less before
            Assert.AreEqual(full * (1.0 - 0.5 * 12.0 / 17.0), Oars.Speed(c, 0.0, 0.0, 12.0, out _), 1e-9);
            // with a fair fresh breeze the sail is the better way: the order is a choice, not always the faster
            Assert.Greater(Travel.Speed(c, 0.0, 225.0, 8.0, out _), Oars.Speed(c, 0.0, 225.0, 8.0, out _));
            // a gale: she lies to, oars or no
            Assert.AreEqual(0.0, Oars.Speed(c, 0.0, 0.0, 18.0, out lying));
            Assert.IsTrue(lying);
            // a boat with no oars
            var bare = Boat();
            bare.oarsKn = 0f;
            Assert.AreEqual(0.0, Oars.Speed(bare, 0.0, 0.0, 0.0, out _));
            Assert.AreEqual(0.0, Oars.Whole(bare));
        }

        [Test]
        public void TheOrderIsTakenOrRefusedWithAReason()
        {
            var c = Boat();
            Assert.IsFalse(Oars.Out);
            Assert.AreEqual(6.0, Oars.Left(c));
            Assert.IsTrue(Oars.Can(c, false, false, out string why));
            Assert.AreEqual("", why);
            Assert.IsFalse(Oars.Can(c, true, false, out why), "the oars can be ordered out at the quay");
            StringAssert.Contains("made fast", why);
            Assert.IsFalse(Oars.Can(c, false, true, out why), "the oars can be ordered out in a gale");
            StringAssert.Contains("gale", why);
            var bare = Boat();
            bare.oarsKn = 0f;
            Assert.IsFalse(Oars.Can(bare, false, false, out why));
            Assert.AreEqual("She has no oars.", why);
            Assert.IsFalse(Oars.Can(null, false, false, out why));
            // refused: said, and nothing changes
            Assert.AreEqual("She is made fast. The oars are for the way.", Oars.Order(true, c, true, false));
            Assert.IsFalse(Oars.Out);
            int version = Oars.Version;
            Assert.AreEqual("The sail comes down and the men take to the oars.", Oars.Order(true, c, false, false));
            Assert.IsTrue(Oars.Out);
            Assert.Greater(Oars.Version, version);
            Assert.AreEqual("", Oars.Order(true, c, false, false), "the same order twice is said twice");
            Assert.AreEqual("The oars come in and the sail goes up.", Oars.Order(false, c, false, false));
            Assert.IsFalse(Oars.Out);
            // a boat with no sail has other words
            var rower = Boat();
            rower.hasSail = false;
            Assert.AreEqual("The men bend to the oars.", Oars.Order(true, rower, false, false));
            Assert.AreEqual("The men ease to their own stroke.", Oars.Order(false, rower, false, false));
        }

        [Test]
        public void TheMenTireAndTheStrengthComesBackWithRest()
        {
            var c = Boat();
            var said = new List<string>();
            System.Action<string> hear = said.Add;
            Voyage.Notice += hear;
            try
            {
                Oars.Order(true, c, false, false);
                double heart = Voyage.Morale;
                // an hour rowed is an hour out of them; an hour with the order standing and no way on her is not
                Oars.Step(1.0, true, false, c);
                Assert.AreEqual(5.0, Oars.Left(c), 1e-9);
                Oars.Step(1.0, false, false, c);
                Assert.AreEqual(5.0 + 6.0 / 18.0, Oars.Left(c), 1e-9, "an hour not rowed should rest them a little");
                Assert.IsTrue(Oars.Out);
                Assert.AreEqual(heart, Voyage.Morale, 1e-9);
                Assert.IsEmpty(said);
                // rowed until they cannot: the oars come in by themselves, it is said, and their heart is the less for it
                for (int i = 0; i < 20 && Oars.Out; i++)
                    Oars.Step(0.5, true, false, c);
                Assert.IsFalse(Oars.Out, "the men rowed on with nothing left in them");
                Assert.AreEqual(0.0, Oars.Left(c), 1e-9);
                Assert.AreEqual(1, said.Count);
                Assert.AreEqual("The men are spent. The oars come in and the sail goes up.", said[0]);
                Assert.AreEqual(heart + rules.oarsSpentMorale, Voyage.Morale, 1e-9);
                // spent, the order is refused, and says how long
                Assert.IsFalse(Oars.Can(c, false, false, out string why));
                Assert.AreEqual("The men have not their strength back yet: about 3 hours more.", why);
                Assert.AreEqual(3.0, Oars.RestWanted(c), 1e-9);
                Assert.AreEqual(why, Oars.Order(true, c, false, false));
                Assert.IsFalse(Oars.Out);
                // three hours of rest: an hour of rowing is in them again, and the order is taken
                Oars.Step(3.0, false, false, c);
                Assert.AreEqual(1.0, Oars.Left(c), 1e-9);
                Assert.IsTrue(Oars.Can(c, false, false, out _));
                // eighteen hours in all: whole again, and never more than whole
                Oars.Step(15.0, false, false, c);
                Assert.AreEqual(6.0, Oars.Left(c), 1e-9);
                Oars.Step(100.0, false, false, c);
                Assert.AreEqual(6.0, Oars.Left(c), 1e-9);
                Assert.AreEqual(0.0, Oars.Spent);
                // made fast, the oars are in, and nothing is said of it
                Oars.Order(true, c, false, false);
                Oars.Step(0.1, false, true, c);
                Assert.IsFalse(Oars.Out, "she lies at a quay with the oars out");
                Assert.AreEqual(1, said.Count);
                // no time, no change
                Oars.Order(true, c, false, false);
                double left = Oars.Left(c);
                Oars.Step(0.0, true, false, c);
                Assert.AreEqual(left, Oars.Left(c));
            }
            finally
            {
                Voyage.Notice -= hear;
            }
        }

        [Test]
        public void AFullPullEatsHalfADaysFoodMore()
        {
            var c = Boat();
            Trade.Hold.Add(GameData.Good("ships_bread"), 2, 4.0, 0.0);   // two 20 kg sacks
            double day = rules.men * rules.foodKg;
            // a day with no rowing
            double before = Voyage.FoodKg;
            Voyage.Day(true, 0.5);
            Assert.AreEqual(before - day, Voyage.FoodKg, 1e-6);
            // a day with half a pull: a quarter of a day's food more
            Oars.Order(true, c, false, false);
            Oars.Step(3.0, true, false, c);
            before = Voyage.FoodKg;
            Voyage.Day(true, 0.5);
            Assert.AreEqual(before - day * 1.25, Voyage.FoodKg, 1e-6, "three hours at the oars should eat a quarter of a day's food more");
            // it is taken once
            before = Voyage.FoodKg;
            Voyage.Day(true, 0.5);
            Assert.AreEqual(before - day, Voyage.FoodKg, 1e-6);
            // a full pull, rowed past its end: half a day's food more and no more
            Oars.Reset();
            Oars.Order(true, c, false, false);
            Oars.Step(10.0, true, false, c);
            Assert.IsFalse(Oars.Out);
            Assert.AreEqual(0.5, Oars.TakeBread(), 1e-9);
            Assert.AreEqual(0.0, Oars.TakeBread());
        }

        [Test]
        public void TheOarsAreSavedAndComeBack()
        {
            var c = Boat();
            Oars.Order(true, c, false, false);
            Oars.Step(2.5, true, false, c);
            Assert.IsTrue(ContentSave.Registered("oars"), "the oars are not saved with the game");
            // (as a save is: text, read back)
            object saved = Json.Parse(Json.Write(Oars.Capture()));
            Oars.Reset();
            Assert.IsFalse(Oars.Out);
            Assert.AreEqual(6.0, Oars.Left(c));
            Oars.Restore(saved);
            Assert.IsTrue(Oars.Out);
            Assert.AreEqual(3.5, Oars.Left(c), 1e-9);
            Assert.AreEqual(2.5 / 6.0 * 0.5, Oars.TakeBread(), 1e-9);
            // an older save has no oars: in, and the men fresh
            Oars.Restore(null);
            Assert.IsFalse(Oars.Out);
            Assert.AreEqual(6.0, Oars.Left(c));
        }
    }
}
