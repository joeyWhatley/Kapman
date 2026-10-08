using System.Linq;
using Kapman.Encounters;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// WHO IS SPEAKING ON A CARD (8 Oct 2026; the user, by the picker: "Fix the cards first"). The four of the crew are
    /// four people, the same on every card and at every port (an actor's "cast" and "name": NpcRegistry.Cast), where
    /// before each quay made up a stranger of that trade; and each line of a card carries who says it, so the face and
    /// the name in the talk window change with the speaker (FloatSpec.voices).
    /// </summary>
    public class CastTests
    {
        static EncounterWorld World(EncounterBook book, NpcRegistry npcs, string port)
        {
            return new EncounterWorld
            {
                book = book, state = new EncounterState(), npcs = npcs, game = new FakeGame(), vars = new EncounterVars(),
                port = new PortFacts { id = port, realm = "franks", faith = "christian", peoples = { "frisian" } }, year = 793, day = 3.0,
            };
        }

        [Test]
        public void TheCrewOfTheCardsAreTheStandingCast()
        {
            var book = EncounterData.Book();
            var lent = book.Card("crew.faith.lent_meat");
            Assert.IsNotNull(lent, "the card of the pork in Lent is gone");
            var dane = lent.actors.First(a => a.key == "dane");
            Assert.AreEqual("cast.crew.ravn", dane.cast, "the Dane of the crew cards is Ravn");
            Assert.AreEqual("Ravn", dane.name);
            Assert.AreEqual("norse", dane.people);
            Assert.AreEqual("Aldger", lent.actors.First(a => a.key == "steersman").name);
            Assert.AreEqual("Brunger", lent.actors.First(a => a.key == "mate").name);
            var fog = book.Card("sea.fog.bank");
            Assert.AreEqual("cast.crew.aldger", fog.actors.First(a => a.key == "steersman").cast);
            Assert.AreEqual("Beornwine", fog.actors.First(a => a.key == "lookout").name, "the lookout of the sea cards is the English boy of the crew cards");
            Assert.AreEqual("english", fog.actors.First(a => a.key == "lookout").people);
            // the Dane who asks for an oar is a stranger
            var hire = book.Card("crew.hire.dane");
            Assert.AreEqual("", hire.actors.First(a => a.key == "dane").cast, "the Dane to be hired was made Ravn");
            // nobody named is without a name, and the four of the crew have a people of their own (anybody else named
            // may be of the port's people, as the places are)
            foreach (var c in book.cards)
                foreach (var a in c.actors)
                    if (a.cast.Length > 0)
                    {
                        Assert.IsNotEmpty(a.name, c.id + " " + a.key);
                        if (a.cast.StartsWith("cast.crew.", System.StringComparison.Ordinal))
                            Assert.AreNotEqual("port", a.people, c.id + " " + a.key);
                    }
        }

        [Test]
        public void OneOfTheCastIsTheSamePersonAtEveryPortAndAtSea()
        {
            var book = EncounterData.Book();
            var npcs = new NpcRegistry(793, book.roles, book.names);
            var atHome = EncounterSession.Open(book.Card("crew.faith.lent_meat"), World(book, npcs, "dorestad"));
            var atSea = EncounterSession.Open(book.Card("sea.fog.bank"), World(book, npcs, ""));
            var elsewhere = EncounterSession.Open(book.Card("crew.faith.lent_meat"), World(book, npcs, "traiectum"));
            var ravn = atHome.People.First(p => p.id == "cast.crew.ravn");
            Assert.AreEqual("Ravn", ravn.name);
            Assert.AreEqual("norse", ravn.people);
            Assert.AreSame(ravn, elsewhere.People.First(p => p.id == "cast.crew.ravn"), "Ravn at Traiectum is another man than Ravn at Dorestat");
            var aldger = atHome.People.First(p => p.id == "cast.crew.aldger");
            Assert.AreEqual("Aldger", aldger.name);
            Assert.AreSame(aldger, atSea.People.First(p => p.role == "steersman"), "the steersman at sea is another man than the steersman in port");
            Assert.AreSame(npcs.Get("cast.crew.ravn"), ravn);
            Assert.IsTrue(NpcRegistry.IsCast(ravn.id));
            StringAssert.StartsWith("Ravn, ", atHome.Speaker(ravn), "the line under the title names him");
        }

        [Test]
        public void EachLineOfACardCarriesWhoSaysIt()
        {
            var book = EncounterData.Book();
            var npcs = new NpcRegistry(793, book.roles, book.names);
            var s = EncounterSession.Open(book.Card("crew.faith.lent_meat"), World(book, npcs, "dorestad"));
            var spec = FrameRoot.CardSpec(null, s);
            Assert.AreEqual(s.lines.Count, spec.lines.Count);
            Assert.GreaterOrEqual(spec.voices.Count, 3, "the card has three speakers and the teller");
            int ravn = 0, aldger = 0, teller = 0;
            foreach (var l in s.lines)
            {
                Assert.IsTrue(spec.voices.TryGetValue(l.text, out var v), "a line has nobody to say it: " + l.text);
                if (l.who == null)
                {
                    Assert.IsTrue(v.Teller, "the teller has a face: " + l.text);
                    teller++;
                }
                else
                {
                    StringAssert.StartsWith(l.who.name + ", ", v.who, "the line is not under its own speaker's name");
                    Assert.AreEqual(l.who.id, v.face);
                    ravn += l.who.name == "Ravn" ? 1 : 0;
                    aldger += l.who.name == "Aldger" ? 1 : 0;
                }
            }
            Assert.AreEqual(1, ravn, "Ravn says one line on this card");
            Assert.AreEqual(1, aldger, "and Aldger one");
            Assert.LessOrEqual(teller, 1, "and the teller at most one (his line is of the first years)");
            // the window opens on its first speaker, as before
            StringAssert.StartsWith("Aldger, ", spec.who);
        }
    }
}
