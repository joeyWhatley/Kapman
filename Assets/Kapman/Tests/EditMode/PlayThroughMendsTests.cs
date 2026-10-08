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
    /// WHAT THE FIRST PLAY-THROUGH SHOWED, HELD (the road's step 9; Design/checks/playthrough_1.md). The walk from a new
    /// game to Michaelmas (Tests/PlayMode/PlayThroughTests.cs) is too long for the suites; the faults it found that a
    /// test can hold without the running game are held here.
    /// </summary>
    public class PlayThroughMendsTests
    {
        const string Data = "Assets/Kapman/Game/Data/";

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        sealed class Vars : IVars
        {
            public readonly Dictionary<string, double> nums = new Dictionary<string, double>();
            public double Num(string name) => nums.TryGetValue(name, out double v) ? v : 0.0;
            public string Text(string name) => "";
        }

        static BoatHour At(long hour, double wind) => new BoatHour
        {
            hour = hour, wind = wind, offWind = 150.0, underWay = true, galeIn = double.PositiveInfinity,
            men = 4, hands = 4, length = 7.7, beam = 1.5, fragility = 0.6, lossPerHour = 0.0004, seed = 793,
        };

        [TearDown]
        public void Done()
        {
            Cond.NameOf = null;
            Boat.Begin(new BoatRules());
            ChestData.Load(null, null, null);
        }

        [Test]
        public void TheOldHappeningsThatDoubledTheBoatAreGone()
        {
            GameEvents.Load(File("events_793.json"));
            // (she took water by a happening while the Ship window said her hull was sound; a cask went foul by a
            // happening the day before the card of the same thing)
            foreach (string gone in new[] { "leak", "gale", "casks_spoil" })
                Assert.IsFalse(GameEvents.All.Any(e => e.id == gone), "the old happening '" + gone + "' still comes beside the boat and its cards");
            Assert.IsTrue(GameEvents.All.Any(e => e.id == "catch"), "the table lost a happening it should keep");
            // the cards of her wounds, which the watch offers when they befall her, are in the book
            var book = EncounterData.Book();
            Assert.IsNotNull(book.Card(BoatWatch.SailCard), BoatWatch.SailCard);
            Assert.IsNotNull(book.Card(BoatWatch.LeakCard), BoatWatch.LeakCard);
        }

        [Test]
        public void ATightBoatIsDryAndWantsNoYard()
        {
            Boat.Begin(BoatRules.Load(File("boat_rules.json")));
            // (left to stand, a sound hull's weep took a hundred kg of her room every two days and she was "wet underfoot")
            for (int h = 0; h < 72; h++)
                Boat.Hour(At(h, 6.0));
            Assert.Less(Boat.Water, 1.0, "a sound boat on a quiet passage should be dry");
            Assert.AreEqual("dry", Boat.WaterWord);
            Assert.AreEqual(0, Boat.Bailers, "no man is set to bail a tight boat");
            Assert.Less(Boat.TakenKg, 1.0, "the bilge of a tight boat takes no room in the hold");
            // three days of wear is not work for a yard; a hull that works is
            Assert.IsFalse(Boat.WantsSeams, "a sound hull was sent to the yard");
            Boat.HullBy(-0.2);
            Assert.IsTrue(Boat.WantsSeams);
            Assert.Greater(Boat.HullMendCost, 0.0);
            // and a started strake still sets a man to it
            Boat.Strake();
            Boat.Hour(At(100, 6.0));
            Assert.AreEqual(1, Boat.Bailers);
        }

        [Test]
        public void AStoneIsNotCopiedBeforeItStands()
        {
            Assert.AreEqual(940, InscriptionDef.CarvedFrom("c. 940-958"));
            Assert.AreEqual(720, InscriptionDef.CarvedFrom("c. 720-730"));
            Assert.AreEqual(900, InscriptionDef.CarvedFrom("brooch c. 700; runes added c. 900-950 (?)"));
            Assert.AreEqual(800, InscriptionDef.CarvedFrom("c. 800-850 (Stephens: 750-800)"));
            Assert.AreEqual(900, InscriptionDef.CarvedFrom("10th century (?)"));
            Assert.AreEqual(0, InscriptionDef.CarvedFrom(""));
            Assert.AreEqual("Gorm's stone", InscriptionDef.Plain("Gorm's stone (Jelling 1)"));
            Assert.AreEqual("Glavendrup stone", InscriptionDef.Plain("Glavendrup stone"));
            // of the table's stones at Ripa only the skull fragment stands in 793 (the walk was given Gorm's and Harald's)
            ChestData.Load(null, null, File("runes_793.json"));
            var ripa = ChestData.Inscriptions.Where(i => i.port == "ripa").ToList();
            Assert.Greater(ripa.Count, 3);
            CollectionAssert.AreEqual(new[] { "ribe_skull" }, ripa.Where(i => i.carved == 0 || i.carved <= 793).Select(i => i.id).ToArray());
            Assert.AreEqual("Harald's stone", ChestData.Text("stone:jelling_dr42").title);
            foreach (var i in ChestData.Inscriptions)
                Assert.Greater(i.carved, 0, "no year could be read for " + i.id + ": " + i.date);
        }

        [Test]
        public void AShutChoiceSaysOnlyWhatShutsIt()
        {
            var words = EncounterData.Book().words;
            Cond.NameOf = key => key == "why:flag.sea_reefed" ? "Only with the sail shortened."
                                : key == "units:frisian_cloth:4" ? "4 bolts of Frisian cloth" : null;
            var rich = new Vars();
            rich.nums["silver"] = 60.0;
            var poor = new Vars();
            // one part the page has no words for, one it has: only the part that fails is said
            var both = Cond.Parse("silver >= 48 and flag.ch_black_stones_3", words);
            Assert.AreEqual("Not now.", both.Why(rich, words), "she has the silver: the reason is not the silver");
            Assert.AreEqual("You need 48 d of silver.", both.Why(poor, words), "a reason that is known is said without 'Not now.' before it");
            // two parts with no words are one "Not now."
            Assert.AreEqual("Not now.", Cond.Parse("flag.a_thing and flag.another_thing", words).Why(poor, words));
            // a thing of the boat asked by its flag has its sentence; so has a count of a good
            Assert.AreEqual("Only with the sail shortened.", Cond.Parse("flag.sea_reefed", words).Why(poor, words));
            Assert.AreEqual("Only if you carry 4 bolts of Frisian cloth.", Cond.Parse("hold.frisian_cloth >= 4", words).Why(poor, words));
            Assert.AreEqual("Not now.", Cond.Parse("hold.value >= 200", words).Why(poor, words), "a sum of the hold is not a good");
            foreach (string cond in new[] { "silver >= 48 and flag.x", "flag.sea_reefed", "hold.frisian_cloth >= 4" })
                Assert.IsNull(PageWords.Fault(Cond.Parse(cond, words).Why(poor, words)), cond);
            // with no names from the game the plain description stands, as the tools read it
            Cond.NameOf = null;
            StringAssert.Contains("silver", both.Why(poor, words));
        }

        [Test]
        public void TheLogSaysYourWordAndHowMuchHeart()
        {
            Assert.AreEqual("You broke your word on the men's pay.", LogProse.YouDid("broke his word on the men's pay"));
            Assert.AreEqual("You kept your word and sent him home.", LogProse.YouDid("kept his word and sent him home"));
            Assert.AreEqual("You sent silver to his father.", LogProse.YouDid("sent silver to his father"), "another man's 'his' is his own");
            Voyage.Begin(VoyageRules.Load(File("voyage_rules.json")));
            var game = new GameEffects { book = EncounterData.Book() };
            Assert.AreEqual("The crew grumble.", game.Morale(-0.01), "a hundredth part is not 'lost heart'");
            Assert.AreEqual("The crew have lost some heart.", game.Morale(-0.03));
            Assert.AreEqual("The crew have lost heart.", game.Morale(-0.1));
            Assert.AreEqual("The crew are a little easier.", game.Morale(0.01));
            Assert.AreEqual("The crew are in better heart.", game.Morale(0.04));
            Assert.IsNull(game.Morale(0.0));
            foreach (double share in new[] { -0.1, -0.03, -0.01, 0.01, 0.04 })
                Assert.IsNull(PageWords.Fault(game.Morale(share), prose: true));
        }
    }
}
