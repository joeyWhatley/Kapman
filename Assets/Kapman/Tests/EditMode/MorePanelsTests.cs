using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE REST OF THE PANELS (7 Oct 2026): Yourself, Crew, People here, Bonds, Faith and days, The field, Wind and
    /// tide, Ship, House, Chest, Tongues are found by the registry; each says something even with no game running (a
    /// panel with nothing true to show says so and is not left out); the places' book reads the writers' files with the
    /// people alive in a year; a talk with one of them is a floating window with their lines; the calendar gives the
    /// days ahead for a faith; and the field tells who is ahead on the way.
    /// </summary>
    public class MorePanelsTests
    {
        static readonly string[] Mine = { "self", "crew", "people", "bonds", "faith", "field", "sky", "ship", "house", "chest", "tongues" };

        string peopleWas;
        string folder;

        [SetUp]
        public void Up()
        {
            peopleWas = PlayerPrefs.GetString("kapman.people", "frisian");
            ContentFiles.SetRoot(null);
            Self.Reset();
        }

        [TearDown]
        public void Down()
        {
            ContentTestKit.Done(folder);
            folder = null;
            Self.Reset();
            MapPalette.SetPeople(peopleWas);
        }

        static VisualElement Filled(FramePanel panel)
        {
            var body = new VisualElement();
            var fill = panel.GetType().GetMethod("Fill", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(fill, panel.Id + " has no Fill");
            fill.Invoke(panel, new object[] { body });
            return body;
        }

        static List<string> Words(VisualElement body) => body.Query<Label>().ToList().Select(l => l.text).Where(t => !string.IsNullOrEmpty(t)).ToList();

        // ------------------------------------------------------------------ the registry

        [Test]
        public void TheNewPanelsAreFoundAndTheWorkspacesNameThem()
        {
            var ids = FramePanels.Ids;
            foreach (string id in Mine)
                CollectionAssert.Contains(ids, id, "no panel " + id);
            Assert.GreaterOrEqual(ids.Count, 19, "eight first panels and eleven more");
            Assert.AreEqual("Yourself", FramePanels.Title("self"));
            Assert.AreEqual("People here", FramePanels.Title("people"));
            Assert.AreEqual("Faith and days", FramePanels.Title("faith"));
            Assert.AreEqual("The field", FramePanels.Title("field"));
            Assert.AreEqual("Wind and tide", FramePanels.Title("sky"));
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "two panels with one id");
            var windows = new FrameWindows(() => FramePanels.Ids);
            CollectionAssert.AreEqual(new[] { "self", "bonds", "faith", "tongues" }, windows.spaces[2].slots, "the Self workspace");
            CollectionAssert.Contains(windows.spaces[3].slots, "house");
            CollectionAssert.Contains(windows.spaces[FrameWindows.PortSpace].slots, "people");
        }

        [Test]
        public void EveryNewPanelSaysSomethingWithNoGameRunning()
        {
            foreach (string id in Mine)
            {
                var panel = FramePanels.Get(id);
                Assert.IsNotNull(panel, id);
                var body = Filled(panel);
                Assert.Greater(body.childCount, 0, "the panel " + id + " is empty: a panel with nothing to show says so in a line");
                Assert.Greater(Words(body).Count, 0, "the panel " + id + " says nothing");
            }
        }

        [Test]
        public void YourselfShowsTheFiveStatsAsPips()
        {
            Self.Begin("frisian", "christian", "toll_clerk");
            var body = Filled(FramePanels.Get("self"));
            var words = Words(body);
            foreach (string stat in new[] { "Tongue", "Name", "Nerve", "Lore", "Troth" })
                CollectionAssert.Contains(words, stat);
            Assert.AreEqual(5, body.Query(className: "frame-pips").ToList().Count, "five rows of pips");
            // Tongue 3 for the clerk, Troth 3, the rest 1: nine pips lit of twenty-five
            Assert.AreEqual(25, body.Query(className: "frame-pip").ToList().Count);
            Assert.AreEqual(9, body.Query(className: "frame-pip").ToList().Count(p => !p.ClassListContains("off")));
            Assert.IsTrue(words.Any(w => w.Contains("Christian")), "the faith is named");
            Assert.IsTrue(words.Any(w => w.Contains("toll-keeper")), "the past is named");
        }

        // ------------------------------------------------------------------ the places and their people

        [Test]
        public void ThePlacesBookReadsTheWritersFiles()
        {
            var book = PlaceBook.Current;
            Assert.GreaterOrEqual(book.Places.Count, 40, "the places files: " + string.Join("; ", ContentFiles.Warnings));
            var dorestad = book.Place("dorestad");
            Assert.IsNotNull(dorestad);
            Assert.GreaterOrEqual(dorestad.sites.Count, 8);
            var ravn = book.Person("dorestad.ravn");
            Assert.IsNotNull(ravn, "Ravn is in the Dorestat file");
            Assert.AreEqual("Ravn", ravn.name);
            Assert.IsNotEmpty(ravn.Line("greet"));
            Assert.Greater(ravn.lines["rumour"].Length, 1, "a key may hold several lines");
            Assert.AreNotEqual(ravn.Line("rumour", 0), ravn.Line("rumour", 1));
            Assert.AreEqual(ravn.Line("rumour", 0), ravn.Line("rumour", ravn.lines["rumour"].Length), "and they go round");
            Assert.AreEqual("", ravn.Line("no_such_key"));
            // alive this year: the Dane who holds the town in 850 is not there in 793
            var now = book.People("dorestad", 793).Select(p => p.id).ToList();
            CollectionAssert.Contains(now, "dorestad.ravn");
            CollectionAssert.Contains(now, "dorestad.tollman");
            CollectionAssert.DoesNotContain(now, "dorestad.rorik");
            CollectionAssert.Contains(book.People("dorestad", 851).Select(p => p.id).ToList(), "dorestad.rorik");
            Assert.Greater(book.Sites("dorestad", 793).Count, 0);
            Assert.IsNotEmpty(book.Era("dorestad", 793), "the era's line for 793");
            Assert.IsTrue(book.People("dorestad", 793).Any(p => book.SiteOf(p, 793) != null), "nobody at Dorestat is found at a site");
            Assert.IsNull(book.Place("nowhere"));
            Assert.AreEqual(0, book.People("nowhere", 793).Count);
        }

        [Test]
        public void AFileHoldsOnePortOrSeveralAndAHalfWrittenOneIsSkipped()
        {
            folder = ContentTestKit.UseFolder();
            ContentTestKit.Put(folder, "places/two.json",
                "{ \"ports\": [" +
                "{ \"port\": \"alpha\", \"sites\": [ { \"id\": \"alpha.quay\", \"kind\": \"quay\", \"name\": \"The quay\", \"from\": 0, \"to\": 800, \"see\": \"Mud. More mud.\", \"do\": [\"market\", \"person:alpha.ada\"] } ]," +
                "  \"people\": [ { \"id\": \"alpha.ada\", \"name\": \"Ada\", \"role\": \"farm_wife\", \"from\": 793, \"to\": 810, \"died\": 805, \"lines\": { \"greet\": \"Well.\", \"rumour\": [\"One.\", \"Two.\"] } } ]," +
                "  \"eras\": [ { \"from\": 793, \"to\": 799, \"text\": \"Lean years.\" } ] }," +
                "{ \"port\": \"beta\", \"sites\": [], \"people\": [] } ] }");
            ContentTestKit.Put(folder, "places/one.json", "{ \"port\": \"gamma\", \"people\": [ { \"id\": \"gamma.g\", \"name\": \"G\", \"role\": \"smith\", \"from\": 0, \"to\": 0 } ] }");
            ContentTestKit.Put(folder, "places/half.json", "{ \"ports\": [ { \"port\": \"delta\", ");
            ContentFiles.SetRoot(folder);
            var book = PlaceBook.Current;
            CollectionAssert.AreEquivalent(new[] { "alpha", "beta", "gamma" }, book.Places.Select(p => p.port).ToList());
            var ada = book.Person("alpha.ada");
            Assert.AreEqual("farm wife", ada.Role);
            Assert.IsTrue(ada.Here(800));
            Assert.IsFalse(ada.Here(806), "dead by then");
            Assert.IsFalse(ada.Here(792), "not there yet");
            Assert.AreEqual("alpha.quay", book.SiteOf(ada, 795).id);
            Assert.IsNull(book.SiteOf(ada, 801), "the quay is gone by 801");
            Assert.AreEqual("Lean years.", book.Era("alpha", 795));
            Assert.AreEqual("", book.Era("alpha", 801));
            Assert.IsTrue(book.Person("gamma.g").Here(1000), "no years given: always there");
        }

        [Test]
        public void ATalkIsAFloatingWindowWithThePersonsLines()
        {
            ContentTestKit.LoadPorts();
            var book = PlaceBook.Current;
            var ravn = book.Person("dorestad.ravn");
            var port = GameData.Port("dorestad");
            var asked = new List<string>();
            bool left = false;
            Assert.AreEqual("greet", PersonTalk.GreetKey(ravn, false));
            Assert.AreEqual("greet_again", PersonTalk.GreetKey(ravn, true), "someone met before is greeted otherwise");
            var spec = PersonTalk.Spec(ravn, book.SiteOf(ravn, 793), port, "greet", 0, k => asked.Add(k), () => left = true);
            Assert.AreEqual("person:dorestad.ravn", spec.id);
            Assert.AreEqual("Ravn", spec.title);
            Assert.AreEqual("Ravn", spec.speaker, "he stands in the portrait's place while he speaks");
            StringAssert.StartsWith("Crewman", spec.who);
            StringAssert.EndsWith(ravn.Line("greet"), spec.lines[0].text, "his greeting, after the word he hails with");
            Assert.AreEqual(ravn.looks, spec.small, "what is seen of him, at the greeting");
            Assert.IsFalse(spec.mustAnswer, "a talk may be shut");
            Assert.IsFalse(spec.holdsClock, "and does not hold the clock");
            var labels = spec.options.Select(o => o.label).ToList();
            CollectionAssert.Contains(labels, "Ask what is said here");
            CollectionAssert.Contains(labels, "Ask for a saying");
            CollectionAssert.Contains(labels, "Ask for the riddle");
            Assert.AreEqual("Take your leave", labels[labels.Count - 1]);
            spec.options[labels.IndexOf("Ask what is said here")].pick();
            CollectionAssert.AreEqual(new[] { "rumour" }, asked);
            // the second thing said: more of the same may be asked for; a saying said is not asked for again
            var rumour = PersonTalk.Spec(ravn, null, port, "rumour", 1, k => asked.Add(k), () => left = true);
            Assert.AreEqual(ravn.Line("rumour", 1), rumour.lines[0].text);
            CollectionAssert.Contains(rumour.options.Select(o => o.label).ToList(), "Ask what else is said");
            Assert.AreEqual("", rumour.small);
            var saying = PersonTalk.Spec(ravn, null, port, "saying", 0, k => asked.Add(k), () => left = true);
            CollectionAssert.DoesNotContain(saying.options.Select(o => o.label).ToList(), "Ask for a saying");
            // leaving: his farewell, then the way out
            saying.options[saying.options.Count - 1].pick();
            Assert.AreEqual("farewell", asked[asked.Count - 1]);
            var farewell = PersonTalk.Spec(ravn, null, port, "farewell", 0, k => asked.Add(k), () => left = true);
            Assert.AreEqual(1, farewell.options.Count);
            farewell.options[0].pick();
            Assert.IsTrue(left);
            // every person of Dorestat in 793 can be talked to
            foreach (var p in book.People("dorestad", 793))
            {
                var s = PersonTalk.Spec(p, book.SiteOf(p, 793), port, PersonTalk.GreetKey(p, false), 0, null, null);
                Assert.IsNotEmpty(s.lines[0].text, p.id + " has nothing to greet with");
                Assert.Greater(s.options.Count, 0, p.id);
            }
        }

        // ------------------------------------------------------------------ the days of a faith

        [Test]
        public void TheCalendarGivesTheDaysAheadForAFaith()
        {
            var cal = FaithCalendar.Current;
            Assert.IsTrue(cal.FromFile, "story/calendar_793.json was not read: " + string.Join("; ", ContentFiles.Warnings));
            Assert.AreEqual(3, FaithCalendar.Weekday(793, 5, 1), "1 May 793 was a Wednesday");
            Assert.AreEqual(6, FaithCalendar.Weekday(793, 6, 8), "8 June 793, Lindisfarne, was a Saturday");
            Assert.AreEqual(0, FaithCalendar.Weekday(793, 4, 7), "Easter 793 was a Sunday");

            var christian = cal.Ahead("christian", "christian", 793, 5, 1);
            Assert.AreEqual(8, christian.Count, "at most eight");
            Assert.AreEqual("philip_james", christian[0].id, "the game begins on a feast");
            Assert.AreEqual(0, christian[0].daysAhead);
            for (int i = 1; i < christian.Count; i++)
                Assert.GreaterOrEqual(christian[i].daysAhead, christian[i - 1].daysAhead, "nearest first");
            var sunday = christian.First(d => d.id == "sunday");
            Assert.IsTrue(sunday.weekly);
            Assert.AreEqual(4, sunday.daysAhead, "Wednesday to Sunday");
            Assert.AreEqual(5, sunday.day);
            Assert.IsNotEmpty(sunday.MayNot, "what may not be done on it");
            var pentecost = christian.First(d => d.id == "pentecost");
            Assert.AreEqual(5, pentecost.month);
            Assert.AreEqual(26, pentecost.day, "Pentecost 793: forty-nine days after Easter on 7 April");
            Assert.AreEqual(25, pentecost.daysAhead);
            Assert.IsTrue(christian.Any(d => d.id == "ascension" && d.day == 16 && d.month == 5));

            var jew = cal.Ahead("jewish", "jewish", 793, 5, 1);
            Assert.IsTrue(jew.Any(d => d.id == "sabbath" && d.daysAhead == 3), "the Sabbath, on Saturday");
            Assert.IsTrue(jew.Any(d => d.id == "shavuot" && d.month == 5 && d.day == 22));
            Assert.IsFalse(jew.Any(d => d.id == "sunday"), "the Lord's day is not a Jew's to keep");

            var heathen = cal.Ahead("old_gods", "old_gods", 793, 10, 1);
            Assert.IsTrue(heathen.Any(d => d.id == "winter_feast" && d.month == 10 && d.day == 14 && d.daysAhead == 13));
            Assert.IsTrue(heathen.Any(d => d.id == "washing_day" && d.weekly));
            Assert.IsFalse(heathen.Any(d => d.id == "michaelmas"));
            Assert.IsTrue(cal.Undated("old_gods", "old_gods").Any(u => u.name == "Yule"), "a feast with no day known is still told of");

            // across the turn of the year
            var winter = cal.Ahead("christian", "christian", 793, 12, 20, 30, 20);
            Assert.IsTrue(winter.Any(d => d.id == "christmas" && d.daysAhead == 5));
            Assert.IsTrue(winter.Any(d => d.id == "epiphany" && d.year == 794 && d.daysAhead == 17));
            // a faith the calendar does not name keeps its family's days
            Assert.IsTrue(cal.Ahead("christian_greek", "christian", 793, 5, 1).Any(d => d.id == "sunday"));
        }

        // ------------------------------------------------------------------ the field, the wind, the tongues

        [Test]
        public void TheFieldTellsWhoIsAheadOnTheWay()
        {
            var cog = new ShipClassDef { id = "cog", name = "hulk" };
            var ships = new List<SimShip>
            {
                new SimShip { id = "a", cls = cog, home = 1, dest = 5, state = ShipState.Sailing, total = 100000.0, done = 50000.0 },
                new SimShip { id = "b", cls = cog, home = 2, dest = 5, state = ShipState.Sailing, total = 100000.0, done = 80000.0 },
                new SimShip { id = "c", cls = cog, home = 2, dest = 5, at = 3, state = ShipState.InPort },
                new SimShip { id = "d", cls = cog, home = 2, dest = 5, at = 3, state = ShipState.Waiting },
                new SimShip { id = "e", cls = cog, home = 2, dest = 4, state = ShipState.Sailing, total = 9000.0 },
                new SimShip { id = "f", cls = cog, home = 2, dest = 5, state = ShipState.Sailing, total = 9000.0, raider = true },
                new SimShip { id = "g", cls = cog, home = 2, dest = 5, state = ShipState.Sailing, total = 9000.0, isPlayer = true },
                new SimShip { id = "h", cls = cog, home = 2, dest = 5, state = ShipState.Lost },
            };
            var field = FieldPanel.Field(ships, 5, 30.0, i => "Port" + i);
            Assert.AreEqual(4, field.Count, "the traders bound there: not the raiders, the lost, the merchant herself, or ships bound elsewhere");
            Assert.AreEqual("A hulk of Port2", field[0].name);
            Assert.AreEqual(20.0, field[0].kmToRun, 1e-6, "the nearest to the port first");
            Assert.AreEqual("ahead", field[0].stands);
            Assert.AreEqual("behind", field[1].stands, "fifty to run against your thirty");
            Assert.AreEqual("not sailed", field[2].stands);
            Assert.AreEqual("not sailed", field[3].stands);
            Assert.IsTrue(field.Any(r => r.doing.Contains("waiting for a wind at Port3")));
            Assert.AreEqual(0, FieldPanel.Field(ships, -1, 30.0, null).Count, "no course: no field");
            Assert.AreEqual(0.0, FieldPanel.ToRunKm(), 1e-9, "no course set: nothing to run");
        }

        [Test]
        public void TheWindLiesFairOrFoulAndATongueShowsAsPips()
        {
            StringAssert.StartsWith("foul", SkyPanel.Lies(0f));
            StringAssert.StartsWith("foul", SkyPanel.Lies(-30f));
            Assert.AreEqual("on the beam", SkyPanel.Lies(90f));
            Assert.AreEqual("on the beam", SkyPanel.Lies(-100f));
            StringAssert.StartsWith("fair", SkyPanel.Lies(180f));
            StringAssert.StartsWith("fair", SkyPanel.Lies(140f));

            Assert.AreEqual(5, TonguesPanel.PipsOf(1.0));
            Assert.AreEqual(3, TonguesPanel.PipsOf(0.5));
            Assert.AreEqual(1, TonguesPanel.PipsOf(0.05), "a tongue held at all shows a pip");
            Assert.AreEqual(0, TonguesPanel.PipsOf(0.0));
            Self.Begin("danish", "old_gods", "hall_child");
            for (int i = 0; i < Self.StaysAfter; i++)
                Self.See("kāpmon");
            Self.See("heil");
            var words = Words(Filled(FramePanels.Get("tongues")));
            Assert.IsTrue(words.Any(w => w.StartsWith("Norse")), "a Dane has Norse: " + string.Join(" | ", words));
            Assert.IsTrue(words.Any(w => w.StartsWith("Learned: ") && w.Contains("kāpmon")), "a word seen often is learned: " + string.Join(" | ", words));
            Assert.IsTrue(words.Any(w => w.Contains("One more heard")));
        }
    }
}
