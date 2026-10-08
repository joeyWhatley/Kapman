using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE BOAT IN THE GAME (the alpha's row 4; Design/fixes/risk_is_real.md 10.2: the tests that need Unity). The
    /// boat's own rules are held by BoatTests. Here: the cards are the boat (a leak from a card is her hull, and the
    /// deck reads it back from her); the gale cards read the weather that will truly blow; shares halve the wage; the
    /// hard pull tires the men; the table of rules is read; the clock has its reasons for weather; the last page.
    /// </summary>
    public class BoatLiveTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        static BoatHour At(long hour, double wind, double galeIn = double.PositiveInfinity, bool hardOars = false) => new BoatHour
        {
            hour = hour, wind = wind, offWind = 150.0, underWay = true, galeIn = galeIn, hardOars = hardOars,
            men = 4, hands = 4, length = 7.7, beam = 1.5, fragility = 0.6, lossPerHour = 0.0004, seed = 793,
        };

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            var rules = TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            Markets.Clear();
            Trade.Begin(rules, 660f, 0.0);
            Voyage.Begin(VoyageRules.Load(File("voyage_rules.json")));
            Boat.Begin(BoatRules.Load(File("boat_rules.json")));
            Oars.Reset();
        }

        [TearDown]
        public void Done()
        {
            ContentFlags.Use(null);
            Markets.Clear();
            Boat.Begin(new BoatRules());
            Oars.Reset();
        }

        [Test]
        public void TheTableOfTheBoatsRulesIsRead()
        {
            Assert.IsNotNull(File("boat_rules.json"), "Game/Data/boat_rules.json is not there");
            var r = BoatRules.Load(File("boat_rules.json"));
            var own = new BoatRules();
            // every number of the code is in the table, by its own name, and the two agree today
            var text = File("boat_rules.json").text;
            foreach (var f in typeof(BoatRules).GetFields().Where(f => f.FieldType == typeof(double)))
            {
                StringAssert.Contains("\"" + f.Name + "\"", text, "the table has no " + f.Name);
                Assert.AreEqual((double)f.GetValue(own), (double)f.GetValue(r), 1e-12, f.Name);
            }
            Assert.AreEqual(8.0, r.diceCarry);
            Assert.AreEqual(12.0, r.lookAhead);
        }

        [Test]
        public void ALeakFromACardIsTheBoat()
        {
            var game = new GameEffects { book = EncounterData.Book() };
            var d = new EncounterDirector(EncounterData.BookOf("sea.leak.seam", "sea.sail.splits"), 793, game);
            EncounterGameVars.Attach(d.vars, () => d.state.callPort);
            ContentFlags.Use(d.state, () => d.Day);
            d.roll = () => 0.0;
            d.SetTime(3.0, 793, 5, 72);
            Assert.AreEqual("sound", Boat.HullWord);
            Assert.AreEqual(0.0, d.vars.Num("flag.sea_leak"));
            // a weeping seam, and a man set to bail: her hull works, and is not yet a started strake
            d.state.Enqueue("sea.leak.seam", true);
            var s = d.OpenNext();
            Assert.IsTrue(d.Choose(s.card.Option("bail").index).ok);
            d.Continue();
            Assert.AreEqual(0.8, Boat.Hull, 1e-9);
            Assert.AreEqual("working", Boat.HullWord);
            Assert.AreEqual(0.0, d.vars.Num("flag.sea_leak"), "a hull that only works reads as a leak");
            // the second makes it one: and the deck reads the leak from the boat, with no flag of its own
            d.state.Enqueue("sea.leak.seam", true);
            s = d.OpenNext();
            d.Choose(s.card.Option("bail").index);
            d.Continue();
            Assert.AreEqual("a strake started", Boat.HullWord);
            Assert.AreEqual(1.0, d.vars.Num("flag.sea_leak"), "the deck does not read the leak from the boat");
            Assert.AreEqual(0.0, d.state.Flag("sea_leak", d.Day), "the deck keeps a second copy of the leak");
            Assert.AreEqual(0.6, d.vars.Num("boat.hull"), 1e-9);
            // a flag that is the boat's is set on the boat; one that is not is the deck's own
            Assert.IsTrue(game.Flagged("sea_reefed", 1.0, 2.0));
            Assert.AreEqual(SailSet.Reefed, Boat.EffectiveSet);
            Assert.IsFalse(game.Flagged("sea_gale_warned", 1.0, 2.0), "the boat took a flag that is not hers");
            // the split sail: the card comes when the boat's sail is split, and sewing takes its hours
            Assert.AreEqual("whole", d.vars.Text("boat.sail"));
            Boat.SetCloth(SailCloth.Split);
            Assert.AreEqual("split", d.vars.Text("boat.sail"));
            d.state.Enqueue("sea.sail.splits", true);
            s = d.OpenNext();
            Assert.AreEqual("sea.sail.splits", s.card.id);
            d.Choose(s.card.Option("sew").index);
            Assert.AreEqual(SailCloth.Whole, Boat.Cloth, "the sail was sewn and is not whole");
            Assert.IsTrue(Boat.LyingTo, "sewing a sail at sea takes no time");
        }

        [Test]
        public void TheGaleCardsReadTheWeatherThatWillBlow()
        {
            var words = EncounterData.Book().words;
            Assert.DoesNotThrow(() => Cond.Parse("under_way and gale_in <= 12 and gale = 0 and boat.water >= 400 and boat.hull < 1 and boat.weary >= 0 and boat.sail = split", words));
            Assert.AreEqual(99.0, EncounterGameVars.Num("gale_in", null), "no gale in sight should read as 99 hours");
            Assert.AreEqual(0.0, EncounterGameVars.Num("gale", null));
            Boat.Hour(At(100, 8.0, 5.0));
            Assert.AreEqual(5.0, EncounterGameVars.Num("gale_in", null));
            Assert.AreEqual(2, Boat.Sign, "five hours off is the second sign");
            Boat.Hour(At(105, 19.0));
            Assert.AreEqual(1.0, EncounterGameVars.Num("gale", null));
            Assert.AreEqual(0.0, EncounterGameVars.Num("gale_in", null));
            // the cards of the release load with the boat's verb and the boat's names, and four of them give her orders
            var book = EncounterData.Book();
            var verbs = new List<string>();
            foreach (string id in new[] { "sea.swell.long", "sea.gale.breaks", "sea.leak.gains", "sea.leak.seam", "sea.sail.splits", "sea.sail.seam", "sea.cargo.shifts", "crew.council.wind" })
            {
                var c = book.Card(id);
                Assert.IsNotNull(c, id);
                foreach (var o in c.options)
                    foreach (var e in o.effects)
                        foreach (var v in e.Verbs())
                            if (v.verb == "boat")
                                verbs.Add(id + " " + o.key + ": " + v.a + " " + v.b);
            }
            CollectionAssert.Contains(verbs, "sea.gale.breaks lie: order lie_to");
            CollectionAssert.Contains(verbs, "sea.gale.breaks run: order reefed");
            CollectionAssert.Contains(verbs, "sea.gale.breaks full: order carry");
            CollectionAssert.Contains(verbs, "sea.swell.long lee: shelter ");
            Assert.GreaterOrEqual(verbs.Count, 12, string.Join(" | ", verbs));
            // what such a card takes when nobody answers is the safe thing
            var rules = CardRules.Read(System.IO.File.ReadAllText(Data + "card_rules_793.json"));
            Assert.AreEqual("lie", rules.For("sea.gale.breaks").take[0]);
            Assert.AreEqual("reef", rules.For("sea.swell.long").take[0]);
            Assert.IsFalse(rules.For("sea.swell.long").holdsShip, "the warning holds her where she is: it is time to use");
            Assert.AreEqual("beach", rules.For("sea.leak.gains").take[0]);
            Assert.AreEqual("sew", rules.For("sea.sail.splits").take[0]);
            // a bad order is refused when the cards are read
            foreach (string bad in new[] { "boat sink", "boat sail torn", "boat order flee", "boat hull", "boat lie_to" })
            {
                var errors = new List<string>();
                Effects.ParseList(new List<object> { bad }, errors, "test");
                Assert.IsNotEmpty(errors, "the cards' reader let through: " + bad);
            }
        }

        [Test]
        public void SharesHalveTheWageAndTheHardPullTiresTheMen()
        {
            var r = Voyage.Rules;
            Voyage.Day(true, 0.5);
            Assert.AreEqual(r.men * r.pay, Voyage.PayOwed, 1e-9, "a day's wage");
            Boat.Shares(true, 0.0);
            double owed = Voyage.PayOwed;
            Voyage.Day(true, 0.5);
            Assert.AreEqual(owed + r.men * r.pay * Boat.R.wageOnShares, Voyage.PayOwed, 1e-9, "on shares the day's wage should be half");
            // the men tire at the hard pull by the player's order far faster than at their own stroke in a calm
            Boat.Begin(Boat.R);
            Boat.Hour(At(10, 6.0));
            double sailing = Boat.Weary;
            Boat.Begin(Boat.R);
            Boat.Hour(At(10, 6.0, hardOars: true));
            Assert.AreEqual(sailing + Boat.R.wearyHardOars, Boat.Weary, 1e-9, "the hard pull should tire them");
            Boat.Begin(Boat.R);
            Boat.Hour(At(10, 1.0));
            Assert.AreEqual(sailing + Boat.R.wearyOarsHour, Boat.Weary, 1e-9, "in a calm the men row at their own stroke, and tire a little");
            Assert.Less(Boat.R.wearyOarsHour * 2.0, Boat.R.wearyHardOars, "their own stroke should cost far less than the hard pull");
            // and her speed under that order goes through her water and her men
            for (int i = 0; i < 30; i++)
                Boat.WearyBy(0.03);
            Assert.Less(Boat.OarFactor, 1.0);
            Boat.WaterBy(Boat.Swamp * 0.5);
            Assert.AreEqual(1.0 - Boat.R.waterSlow, Boat.WaterFactor, 1e-9);
        }

        [Test]
        public void TheWeatherIsAReasonForTheClockToDrop()
        {
            Assert.AreEqual(1, ClockSettings.Level(ClockReason.WeatherFar), "the first sign should drop the clock to speed 2");
            Assert.AreEqual(0, ClockSettings.Level(ClockReason.WeatherNear), "a gale within the watch should drop it to speed 1");
            Assert.IsNotEmpty(ClockSettings.Says(ClockReason.WeatherFar));
            Assert.IsNotEmpty(ClockSettings.Says(ClockReason.WeatherNear));
            StringAssert.DoesNotContain("Weather", ClockSettings.Label(ClockReason.WeatherFar).Substring(0, 3));
            Assert.IsTrue(ClockSettings.Default(ClockReason.WeatherFar) && ClockSettings.Default(ClockReason.WeatherNear));
        }

        [Test]
        public void TheLastPageSaysHowAndOffersTheLastSavedGame()
        {
            bool loaded = false, titled = false;
            var log = new List<string> { "White water to windward. It is on her within the hour.", "All hands to the bailing.", "She filled, and the sea came over the gunwale." };
            var spec = FrameRoot.LostSpec("She filled, and the sea came over the gunwale.", log, true, () => loaded = true, () => titled = true);
            Assert.AreEqual("lost", spec.id);
            Assert.AreEqual("She is lost", spec.title);
            Assert.IsTrue(spec.mustAnswer, "the last page can be shut");
            Assert.IsTrue(spec.holdsClock);
            Assert.AreEqual("She filled, and the sea came over the gunwale.", spec.lines[0].text);
            Assert.AreEqual(1, spec.lines.Count(l => l.text.StartsWith("She filled")), "how she was lost is said twice");
            Assert.IsTrue(spec.lines.Any(l => l.text == "All hands to the bailing."), "the last of the log is not on the last page");
            Assert.AreEqual(2, spec.options.Count);
            Assert.IsTrue(spec.options[0].enabled);
            spec.options[0].pick();
            spec.options[1].pick();
            Assert.IsTrue(loaded && titled);
            foreach (var l in spec.lines)
                Assert.IsNull(PageWords.Fault(l.text, prose: true), l.text);
            // with no saved game the first way is shut, and says why
            var none = FrameRoot.LostSpec("", null, false, null, null);
            Assert.IsFalse(none.options[0].enabled);
            Assert.IsNotEmpty(none.options[0].rule);
            Assert.AreEqual("She is lost.", none.lines[0].text);
        }

        [Test]
        public void ALostBoatIsNotSavedAndAnOlderSaveIsASoundBoat()
        {
            Boat.HullBy(-1.0);                                          // (her strakes open: the men cannot bail that)
            Boat.Hour(At(50, 6.0));
            Assert.IsTrue(Boat.IsLost);
            StringAssert.StartsWith("Her strakes opened", Boat.LostHow);
            // as a save is: text, read back
            var saved = Json.Parse(Json.Write(Boat.Capture())) as Dictionary<string, object>;
            Boat.Begin(Boat.R);
            Assert.IsFalse(Boat.IsLost);
            Boat.Restore(saved);
            Assert.IsTrue(Boat.IsLost, "the boat's loss is not kept with her");
            // (a lost boat refuses a SAVE and not a LOAD: the last page's way back is a load. With no game running
            // both say the game is not ready; the refusal for a lost boat is the save's own)
            StringAssert.DoesNotContain("She is lost", SaveGame.Busy() ?? "", "a lost boat may not be loaded from: the last page could not bring her back");
            Boat.Restore(null);
            Assert.IsFalse(Boat.IsLost, "a save from before the boat was kept is a lost boat");
            Assert.AreEqual(1.0, Boat.Hull);
            Assert.IsFalse(Boat.OnShares);
        }
    }
}
