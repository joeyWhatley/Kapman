using System.Collections.Generic;
using Kapman.EditorTools;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>Saved games: the world written out and read back (through the JSON writer and reader) goes on exactly as
    /// it would have; the merchant's affairs come back as they were.</summary>
    public class SaveTests
    {
        [TearDown]
        public void Done() => Markets.Clear();

        [Test]
        public void ARestoredWorldGoesOnAsItWouldHave()
        {
            var a = SimHarness.MakeWorld(793, 5.0);
            a.Populate();
            for (int h = 0; h < 20 * 24; h++)
                a.Hour();
            string world = Json.Write(a.Capture());
            string markets = Json.Write(Markets.Capture());
            for (int h = 0; h < 20 * 24; h++)
                a.Hour();
            long straight = a.StateHash();

            var b = SimHarness.MakeWorld(793, 5.0);                      // a fresh world (and fresh markets), then the save
            b.Restore(Json.Parse(world) as Dictionary<string, object>);
            Markets.Restore(Json.Parse(markets) as List<object>, 793);
            Assert.AreEqual(a.ships.Count, b.ships.Count, "every ship back");
            for (int h = 0; h < 20 * 24; h++)
                b.Hour();
            Assert.AreEqual(straight, b.StateHash(), "the same world 20 days on");
        }

        [Test]
        public void TheMerchantsAffairsComeBack()
        {
            SimHarness.MakeWorld(1);                                       // (the tables)
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Trade.Hold.Add(GameData.Good("wheat"), 7, 21.0, 2.0);
            double dirhams = Trade.Purse.Count("dirham") + 3.0;             // (the starting purse has some)
            Trade.Purse.Add("dirham", 3.0);
            Trade.Ledger.Note(Markets.Of(GameData.Port("dorestad"), 793, 0.0), 1.0, 6);
            Trade.Debt = 123.0;
            string saved = Json.Write(Trade.Capture());
            Trade.Begin(TradeRules.Current, 900f, 0.0);                   // a new merchant...
            Trade.Restore(Json.Parse(saved) as Dictionary<string, object>);  // ...then the save
            Assert.AreEqual(7, Trade.Hold.Count("wheat"));
            Assert.AreEqual(dirhams, Trade.Purse.Count("dirham"), 1e-9);
            Assert.AreEqual(123.0, Trade.Debt, 1e-9);
            int notes = 0;
            foreach (var _ in Trade.Ledger.Notes)
                notes++;
            Assert.AreEqual(1, notes, "the ledger");
        }

        [Test]
        public void TheListsShowEachSavedGamesDateAndPlace()
        {
            // a slot of our own (a name no player would use), removed again
            string slot = "zz test " + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            string folder = SaveGame.Folder, game = System.IO.Path.Combine(folder, slot + ".kapman.json.gz"),
                   info = System.IO.Path.Combine(folder, slot + ".kapman.info");
            System.IO.Directory.CreateDirectory(folder);
            try
            {
                System.IO.File.WriteAllBytes(game, new byte[] { 0 });
                System.IO.File.WriteAllText(info, Json.Write(new Dictionary<string, object> { ["date"] = "3 May 794, 09:00", ["where"] = "lying at Dorestad" }));
                var s = SaveGame.Slots().Find(x => x.slot == slot);
                Assert.AreEqual("3 May 794, 09:00", s.date);
                Assert.AreEqual("lying at Dorestad", s.where);
                Assert.AreNotEqual(slot, SaveGame.NewSlot(), "a new slot never overwrites one");
            }
            finally
            {
                System.IO.File.Delete(game);
                System.IO.File.Delete(info);
            }
        }

        [Test]
        public void ANewGameStartsClean()
        {
            Commissions.All.Add(new Commission { id = "x", port = "dorestad", good = "wheat", units = 1, mine = true });
            MainMenu.ClearAllState();                                       // what a new game does before the scene reloads
            Assert.AreEqual(0, Commissions.All.Count, "the orders gone, as on entering Play");
        }
    }
}
