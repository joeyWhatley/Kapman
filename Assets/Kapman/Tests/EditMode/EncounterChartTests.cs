using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The encounters on the chart (7 Oct 2026), without the scene: a card file with faults is shut alone and the rest of
    /// the book plays; a new game's call at home is in progress but brings nobody down to the boat; and the pace holds
    /// for a book of a thousand cards (at sea one drawn card in some hours, in port one drawn card a call).
    /// </summary>
    public class EncounterChartTests
    {
        const string Toll = "harbour.tollman.frankish";

        static string Card(string id, string stage, string how) =>
            "{\"id\":\"" + id + "\",\"stage\":\"" + stage + "\"," + how + ",\"when\":\"true\",\"cooldown_days\":0," +
            "\"title_key\":\"" + id + ".title\",\"title\":\"A test\"," +
            "\"options\":[{\"key\":\"ok\",\"label_key\":\"" + id + ".ok\",\"label\":\"Go on\",\"check\":\"true\"}]}";

        /// <summary>A book of the test's own cards, with the real roles, names and words.</summary>
        static EncounterBook BookOf(params string[] cards)
        {
            var src = EncounterData.Sources();
            src.cards.Clear();
            src.cards.Add(("encounters/test_cards.json", "{\"cards\":[" + string.Join(",", cards) + "]}"));
            var book = EncounterLoader.Load(src);
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            return book;
        }

        static EncounterDirector Director(EncounterBook book)
        {
            var d = new EncounterDirector(book, 793, new FakeGame());
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.SetTime(10.0, 793, 6, 240);
            return d;
        }

        static void Answer(EncounterDirector d)
        {
            Assert.IsNotNull(d.OpenNext(), "the card in line opens");
            Assert.IsTrue(d.Choose(0).ok);
            Assert.IsTrue(d.Continue());
        }

        [Test]
        public void AFaultyCardFileShutsOnlyItself()
        {
            var src = EncounterData.Sources();
            int files = src.cards.Count;
            src.cards.Add(("encounters/zz_bad_stage.json", "{\"cards\":[{\"id\":\"bad.one\",\"stage\":\"nonsense\",\"title_key\":\"bad.one.title\"}]}"));
            src.cards.Add(("encounters/zz_not_json.json", "{ \"cards\": [ this is not json"));
            var whole = EncounterLoader.Load(EncounterData.Sources());
            var shut = new List<string>();
            var book = EncounterRunner.LoadWithout(src, shut);
            Assert.IsTrue(book.Ok, "the book is sound once the two bad files are out:\n" + string.Join("\n", book.errors));
            Assert.AreEqual(2, shut.Count, string.Join("\n", shut));
            Assert.IsTrue(shut.Exists(s => s.StartsWith("encounters/zz_bad_stage.json")), "the bad stage's file is named");
            Assert.IsTrue(shut.Exists(s => s.StartsWith("encounters/zz_not_json.json")), "the broken file is named");
            Assert.AreEqual(files, src.cards.Count, "every other file stays");
            Assert.AreEqual(whole.cards.Count, book.cards.Count, "and every card of theirs");
            Assert.IsNotNull(book.Card(Toll));
            Assert.IsNull(book.Card("bad.one"));
        }

        [Test]
        public void AFileThatPointedAtAShutFileGoesToo()
        {
            var src = EncounterData.Sources();
            src.cards.Clear();
            src.cards.Add(("encounters/a_good.json", "{\"cards\":[" + Card("test.good", "sea", "\"rate\":1") + "]}"));
            src.cards.Add(("encounters/b_bad.json", "{\"cards\":[{\"id\":\"test.gone\",\"stage\":\"nonsense\",\"title_key\":\"t\"}]}"));
            src.cards.Add(("encounters/c_points.json", "{\"cards\":[" +
                Card("test.points", "sea", "\"rate\":1").Replace("\"check\":\"true\"", "\"check\":\"true\",\"then\":\"test.gone\"") + "]}"));
            var shut = new List<string>();
            var book = EncounterRunner.LoadWithout(src, shut);
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            Assert.AreEqual(2, shut.Count, string.Join("\n", shut));
            Assert.IsNotNull(book.Card("test.good"), "the sound file plays");
            Assert.IsNull(book.Card("test.points"), "the file whose card led to a shut one is shut too");
        }

        [Test]
        public void ANewGamesCallAtHomeBringsNobody()
        {
            var d = Director(EncounterData.BookOf(Toll));
            d.vars.nums["port.toll"] = 0.1;
            d.watch.points.Add(new MooringPoint("dorestad", 0.0, 0.0, 35.0, true));
            d.BeginCallQuietly("dorestad");
            Assert.IsTrue(d.InCall);
            Assert.AreEqual("dorestad", d.state.callPort);
            Assert.AreEqual(1, d.state.Visits("dorestad"), "lying at home counts as having been there");
            Assert.IsFalse(d.Waiting, "nobody comes down to a boat that has not arrived");
            Assert.IsNull(d.OpenNext());
            // she sails, and comes back: now it is an arrival, and the toll man knows her
            Assert.AreEqual(MooringWatch.Change.CastOff, d.Look(500.0, 0.0, true, 2.0, 0.5));
            Assert.IsFalse(d.InCall);
            var c = MooringWatch.Change.None;
            for (int i = 0; i < 20 && c == MooringWatch.Change.None; i++)
                c = d.Look(5.0, 0.0, false, 0.0, 0.5);
            Assert.AreEqual(MooringWatch.Change.Moored, c);
            var s = d.OpenNext();
            Assert.IsNotNull(s, "the toll man comes on the arrival");
            Assert.AreEqual(Toll, s.card.id);
            Assert.IsTrue(s.returning, "and she has called here before");
        }

        [Test]
        public void AtSeaADrawnCardComesNoSoonerThanTheGap()
        {
            var d = Director(BookOf(Card("test.sea", "sea", "\"rate\":50")));
            d.Look(0.0, 0.0, true, 2.0, 1.0);                             // under way, no mooring near
            d.seaGapHours = 4.0;
            Assert.IsNotNull(d.Tick(241), "the first hour's draw comes");
            Answer(d);
            Assert.IsNull(d.Tick(242), "an hour on: too soon");
            Assert.IsNull(d.Tick(243));
            Assert.IsNull(d.Tick(244));
            Assert.IsFalse(d.Waiting, "a card over the pace is let go, not kept in line");
            Assert.IsNotNull(d.Tick(245), "four hours on, the next may come");
            Answer(d);
            d.seaGapHours = 0.0;
            Assert.IsNotNull(d.Tick(246), "with no gap every hour may draw, as before");
        }

        [Test]
        public void InPortOneDrawnCardACall()
        {
            var book = BookOf(Card("test.visit", "harbour", "\"weight\":50"), Card("test.crew", "crew", "\"rate\":50"));
            var d = Director(book);
            d.callDrawMax = 1;
            var queued = d.BeginCall("dorestad");
            Assert.AreEqual(1, queued.Count, "the visit's card");
            Answer(d);
            Assert.IsNull(d.Tick(241), "the call has had its drawn card");
            Assert.IsFalse(d.Waiting);
            // without the pace the hours bring the crew's card as the scheduler has it
            var free = Director(book);
            free.BeginCall("dorestad");
            Answer(free);
            Assert.IsNotNull(free.Tick(241));
        }
    }
}
