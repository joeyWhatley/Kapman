using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PEOPLE OF THE CARDS HAVE THEIR OWN NAMES (8 Oct 2026, night). The words of a card call its people by name
    /// ("Folkmar rolls four pairs down to your boat") while the window made up another for each ("Meinbod, the
    /// craftsman"). Now an actor whom the designers' note ties to a person is that person: one of a port's own people
    /// (the places' files) for the years they are there, or the one the story names; and a card that wants the holder
    /// of an office at a port (the toll man) meets the man the port's page names.
    /// </summary>
    public class NamedPeopleTests
    {
        static EncounterWorld World(EncounterBook book, NpcRegistry npcs, string port, int year)
        {
            return new EncounterWorld
            {
                book = book, state = new EncounterState(), npcs = npcs, game = new FakeGame(), vars = new EncounterVars(),
                port = new PortFacts { id = port, realm = "franks", faith = "christian", peoples = { "frisian" } }, year = year, day = 3.0,
            };
        }

        [Test]
        public void TheCardsNameTheirPeopleAsTheirWordsDo()
        {
            var book = EncounterData.Book();
            var stones = book.Card("harbour.chain_black_stones.s01_unsold_stones");
            Assert.IsNotNull(stones);
            var cutter = stones.actors.First(a => a.key == "cutter");
            Assert.AreEqual("Folkmar", cutter.name, "the quern cutter of the unsold stones is Folkmar in the card's own words");
            Assert.AreEqual("named.chain_black_stones.folkmar", cutter.cast);
            var rival = stones.actors.First(a => a.key == "rival");
            Assert.AreEqual("dorestad.thiadbod", rival.cast, "the rival skipper is Thiadbod of Dorestat's own page");
            Assert.AreEqual("Thiadbod", rival.name);
            var mint = book.Card("harbour.kings_mint.dorestad").actors.First(a => a.key == "moneyer");
            Assert.AreEqual("dorestad.sigimar", mint.cast);
            Assert.AreEqual("Sigimar", mint.name);
            Assert.Greater(mint.castTo, 793, "the moneyer has no last year");
            // how many of the cards' people have a name of their own now
            int named = 0, all = 0;
            foreach (var c in book.cards)
                foreach (var a in c.actors)
                {
                    all++;
                    if (a.cast.Length > 0)
                    {
                        named++;
                        Assert.IsNotEmpty(a.name, c.id + " " + a.key);
                    }
                }
            UnityEngine.Debug.Log($"Named people: {named} of the cards' {all} actors.");
            Assert.GreaterOrEqual(named, 1500, "the cards' people have lost their names");
            // no card has one person in two of its seats
            foreach (var c in book.cards)
            {
                var ids = c.actors.Where(a => a.cast.Length > 0).Select(a => a.cast).ToList();
                Assert.AreEqual(ids.Count, ids.Distinct().Count(), c.id + ": one person in two seats");
            }
        }

        [Test]
        public void ANamedPersonIsThatPersonOnlyInTheirYears()
        {
            var book = EncounterData.Book();
            var npcs = new NpcRegistry(793, book.roles, book.names);
            var mint = book.Card("harbour.kings_mint.dorestad");
            var early = EncounterSession.Open(mint, World(book, npcs, "dorestad", 793));
            var moneyer = early.People.First(p => p.role == "moneyer");
            Assert.AreEqual("Sigimar", moneyer.name);
            Assert.AreEqual("dorestad.sigimar", moneyer.id);
            StringAssert.StartsWith("Sigimar, ", early.Speaker(moneyer));
            // the same man on another card
            var stones = EncounterSession.Open(book.Card("harbour.chain_black_stones.s01_unsold_stones"), World(book, npcs, "dorestad", 793));
            Assert.AreEqual("Folkmar", stones.People.First(p => p.role == "craftsman").name);
            Assert.AreEqual("Thiadbod", stones.People.First(p => p.role == "skipper").name);
            // long after his years the mint has another man, whoever he is
            int after = mint.actors.First(a => a.key == "moneyer").castTo + 20;
            var late = EncounterSession.Open(mint, World(book, new NpcRegistry(793, book.roles, book.names), "dorestad", after));
            Assert.AreNotEqual("dorestad.sigimar", late.People.First(p => p.role == "moneyer").id, "Sigimar is at his pan twenty years after his last");
        }

        [Test]
        public void TheHolderOfAnOfficeIsThePortsOwn()
        {
            var places = PlaceBook.Current;
            var tollman = places.Holder("dorestad", "tollman", 793);
            Assert.IsNotNull(tollman, "Dorestat's page names no toll man for 793");
            Assert.AreEqual("Aldgrim", tollman.name);
            Assert.IsNull(places.Holder("dorestad", "skipper", 793), "a skipper is not an office: a card's skipper is its own");
            Assert.IsNull(places.Holder("", "tollman", 793));
            var book = EncounterData.Book();
            var npcs = new NpcRegistry(793, book.roles, book.names)
            {
                known = (port, role, year) =>
                {
                    var p = places.Holder(port, role, year);
                    return p == null ? null : new NpcRegistry.Known { id = p.id, name = p.name, people = p.people, faith = p.faith, born = p.born, to = p.died > 0 ? p.died : p.to };
                },
            };
            var toll = EncounterSession.Open(book.Card("harbour.tollman.frankish"), World(book, npcs, "dorestad", 793));
            var man = toll.People.First();
            Assert.AreEqual("Aldgrim", man.name, "the toll man of the card is not the toll man of Dorestat's page");
            Assert.AreEqual(tollman.id, man.id);
            Assert.AreSame(man, EncounterSession.Open(book.Card("harbour.tollman.frankish"), World(book, npcs, "dorestad", 794)).People.First(), "he is another man a year on");
            // a port whose page names nobody for the office: a man is made up, as before
            var elsewhere = EncounterSession.Open(book.Card("harbour.tollman.frankish"), World(book, npcs, "no_such_port", 793)).People.First();
            Assert.IsFalse(NpcRegistry.IsCast(elsewhere.id), "a made-up toll man counts as a named person: " + elsewhere.id);
        }

        [Test]
        public void ANamedPersonIsKnownByTheirId()
        {
            Assert.IsTrue(NpcRegistry.IsCast("cast.crew.ravn"));
            Assert.IsTrue(NpcRegistry.IsCast("dorestad.sigimar"));
            Assert.IsTrue(NpcRegistry.IsCast("dorestad.tollman"));
            Assert.IsTrue(NpcRegistry.IsCast("named.chain_black_stones.folkmar"));
            Assert.IsFalse(NpcRegistry.IsCast("dorestad.tollman.0"));
            Assert.IsFalse(NpcRegistry.IsCast(".sailor.12"));
            Assert.IsFalse(NpcRegistry.IsCast(""));
            Assert.IsFalse(NpcRegistry.IsCast(null));
            // one of the places who is of the story's cast: the tie that says so is read
            Assert.AreEqual("cast.thiadbod", PlaceBook.Current.Person("dorestad.thiadbod")?.same);
            Assert.AreEqual("", PlaceBook.Current.Person("dorestad.tollman")?.same);
            // the portrait: their own where there is one (the pictures are not in the repository: only looked at here)
            string slot = FrameArt.OfSpeaker("dorestad.thiadbod", "skipper");
            if (slot.Length > 0)
                StringAssert.Contains("thiadbod", slot);
        }
    }
}
