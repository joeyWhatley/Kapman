using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// WHAT IS HELD BACK FOR NOW (the user, 9 Oct 2026: "for now we don't include any fully fleshed out quests/ cards
    /// until they're polished, only keep in the good polished stuff for now and we will add in the other content when
    /// it's all done"). Held here:
    ///   - Game/Data/content_hold.json names real files and real cards (a slip of the pen there would hold a card nobody
    ///     meant to hold, or let nothing in);
    ///   - the game reads only what is in (the toll, the lender, the sea's cards: the user's cut by the picker the same
    ///     day): no quest, no port's card, no dated card, none of the crew's; the book it is left with is sound, and no
    ///     file had to be shut;
    ///   - the cards the game's own systems raise by name are in (the boat's four, the lender's day);
    ///   - a test may have the whole book for its samples, and every held card is still read off the disk and checked
    ///     (EncounterData.Book, as all the card tests do), so what waits does not rot.
    /// </summary>
    public class ContentHoldTests
    {
        [TearDown]
        public void Done()
        {
            ContentHold.AllIn = false;
            ContentHold.Use(null);
        }

        [Test]
        public void TheTableNamesRealFilesAndRealCards()
        {
            Assert.IsNotNull(ContentFiles.ParseObject(ContentHold.File), ContentHold.File + " is not there or does not parse");
            var table = ContentHold.Current;
            Assert.IsTrue(table.hold, "nothing is held back: the table says hold false");
            Assert.IsNotEmpty(table.files, "nothing is in the game at all");
            var all = EncounterData.Book();
            foreach (var kv in table.files)
            {
                Assert.IsTrue(ContentFiles.Exists("encounters/" + kv.Key), $"{ContentHold.File} lets in a card file that is not there: {kv.Key}");
                Assert.IsTrue(all.cards.Any(c => c.file == "encounters/" + kv.Key), $"{kv.Key} has no cards");
                if (kv.Value == null)
                    continue;
                Assert.IsNotEmpty(kv.Value, $"{kv.Key} is let in with an empty list of cards: name it \"all\", or leave it out");
                foreach (string id in kv.Value)
                {
                    var card = all.Card(id);
                    Assert.IsNotNull(card, $"{ContentHold.File} lets in a card of {kv.Key} that is not there: {id}");
                    Assert.AreEqual("encounters/" + kv.Key, card.file, $"the card {id} is let in under the wrong file");
                }
            }
        }

        [Test]
        public void TheGameReadsOnlyWhatIsIn()
        {
            var all = EncounterData.Book();
            var book = EncounterRunner.LoadBookAsTheGameDoes();
            Assert.IsNotNull(book, "the game read no book");
            Assert.IsTrue(book.Ok, "with the rest held back the book has faults:\n" + string.Join("\n", book.errors));
            Assert.IsEmpty(EncounterRunner.ShutFiles, "a card file had to be shut: a card that is in points at one that is held");
            string ids = string.Join(", ", book.cards.Select(c => c.id));
            // what the game's own systems raise by name must be there to raise
            foreach (string id in new[] { BoatWatch.GaleCard, BoatWatch.OrderCard, BoatWatch.SailCard, BoatWatch.LeakCard, EncounterRunner.LenderDayHome, EncounterRunner.LenderDayAway })
                Assert.IsNotNull(book.Card(id), $"the game raises the card {id} by its name, and it is held back. In the game: {ids}");
            Assert.IsNotNull(book.Card("harbour.tollman.frankish"), "the toll's card is held back");
            // every card read is one the table lets in, and every card it lets in is read
            foreach (var c in book.cards)
                Assert.IsTrue(ContentHold.CardIn(c.file, c.id), $"{c.id} of {c.file} is in the game and the table does not name it");
            Assert.AreEqual(all.cards.Count(c => ContentHold.CardIn(c.file, c.id)), book.cards.Count, "not every card the table lets in was read");
            // no quest, no port's card, no dated card, none of the crew's
            Assert.IsFalse(book.cards.Any(c => c.id.Contains(".chain_")), "a quest's card is in the game: " + ids);
            Assert.IsFalse(book.cards.Any(c => c.file.Contains("chain_") || c.file.Contains("long_") || c.file.Contains("crew_")), "a held file's card is in the game: " + ids);
            foreach (string held in new[] { "harbour.first_question.dorestad", "harbour.chain_lindisfarne_book.s01_strip_of_gilt", "crew.pay.reckoning", "long.godfred.dead" })
            {
                Assert.IsNotNull(all.Card(held), "the test names a card that is not in the files: " + held);
                Assert.IsNull(book.Card(held), held + " should be held back for now");
            }
            // (the user's cut, by the picker, 9 Oct 2026: "Sea cards back in")
            foreach (string sea in new[] { "sea.calm.falls", "sea.squall.line", "sea.whale.blow", "sea.wreck.mast" })
                Assert.IsNotNull(book.Card(sea), sea + " is held back: the sea's cards are in");
            // (9 Oct 2026: seven more of the lender's, for Ravn's death and the feud)
            Assert.AreEqual(78, book.cards.Count, "the cards in the game for now are the toll, the lender's fourteen and the sea's 63: " + ids);
            Assert.AreEqual(all.cards.Count - 78, HeldCards(all), "what is held and what is in do not make the whole");
            Assert.GreaterOrEqual(EncounterRunner.HeldFiles.Count, 50, "the files held back: " + string.Join(", ", EncounterRunner.HeldFiles));
            Assert.AreEqual(43, EncounterRunner.HeldFiles.Count(f => f.StartsWith("chain_")), "the quests held back");
            Assert.AreEqual(0, book.heldBack, "no file is in by a list of its cards now");
            Debug.Log($"Content hold: {book.cards.Count} cards in the game ({ids}); {HeldCards(all)} held in {EncounterRunner.HeldFiles.Count} files and {book.heldBack} more of the files that are in.");
        }

        static int HeldCards(EncounterBook all) => all.cards.Count(c => !ContentHold.CardIn(c.file, c.id));

        [Test]
        public void ATestMayHaveTheWholeBook()
        {
            ContentHold.AllIn = true;
            Assert.IsFalse(ContentHold.On);
            var book = EncounterRunner.LoadBookAsTheGameDoes();
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            Assert.AreEqual(EncounterData.Book().cards.Count, book.cards.Count, "with everything in, the game's book is not the whole of the card files");
            Assert.IsEmpty(EncounterRunner.HeldFiles);
            Assert.AreEqual(0, book.heldBack);
            Assert.IsNotNull(book.Card("harbour.first_question.dorestad"));
            ContentHold.AllIn = false;
            Assert.IsTrue(ContentHold.On, "the hold did not come back when the test let go of it");
        }

        [Test]
        public void TheTableReadsAsItIsWritten()
        {
            var table = ContentHold.Read(Json.Parse(@"{ ""hold"": true, ""in"": { ""a.json"": ""all"", ""b.json"": [ ""b.one"", ""b.two"" ] }, ""why_these"": { ""c.json"": ""a word"" } }") as Dictionary<string, object>);
            Assert.IsTrue(table.hold);
            Assert.AreEqual(2, table.files.Count, "only what stands under \"in\" is in");
            Assert.IsNull(table.files["a.json"], "\"all\" is the whole file");
            CollectionAssert.AreEquivalent(new[] { "b.one", "b.two" }, table.files["b.json"]);
            Assert.IsFalse(ContentHold.Read(Json.Parse(@"{ ""hold"": false, ""in"": { } }") as Dictionary<string, object>).hold);
            Assert.IsFalse(ContentHold.Read(null).hold, "with no table nothing is held: the game is whole, not empty");
            // a file is asked for with its folder or without; a file in by a list lets in those cards alone
            ContentHold.Use(table);
            Assert.IsTrue(ContentHold.On);
            Assert.IsTrue(ContentHold.FileIn("encounters/a.json") && ContentHold.FileIn("a.json") && ContentHold.FileIn("b.json"));
            Assert.IsFalse(ContentHold.FileIn("encounters/c.json"), "a file named only in the notes is not in");
            Assert.IsTrue(ContentHold.CardIn("encounters/a.json", "anything of a file that is in whole"));
            Assert.IsTrue(ContentHold.CardIn("encounters/b.json", "b.two"));
            Assert.IsFalse(ContentHold.CardIn("encounters/b.json", "b.three"));
            Assert.IsFalse(ContentHold.CardIn("encounters/c.json", "c.one"));
            ContentHold.Use(null);
            // the game's own table
            Assert.IsTrue(ContentHold.FileIn("encounters/sea_793.json") && ContentHold.FileIn("wokerer_793.json") && ContentHold.FileIn("harbour_793.json"));
            Assert.IsFalse(ContentHold.FileIn("encounters/chain_the_bell_793.json"));
            Assert.IsTrue(ContentHold.CardIn("encounters/sea_793.json", "sea.calm.falls"));
            Assert.IsFalse(ContentHold.CardIn("encounters/crew_793.json", "crew.pay.reckoning"));
        }
    }
}
