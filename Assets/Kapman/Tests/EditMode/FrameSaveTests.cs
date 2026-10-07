using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// WHAT COMES BACK FROM A SAVED GAME (7 Oct 2026, Design/save_audit.md): the frame's log, its windows with their
    /// sizes, what each person has been asked, what she waits for; the kinds of a course's legs; and an open card with
    /// the people it had. Each is written out as a saved game writes it (Json.Write) and read back as a load reads it.
    /// </summary>
    public class FrameSaveTests
    {
        /// <summary>Through the saved game's own form and back: text, then parsed (numbers come back as double).</summary>
        static object Saved(object captured) => Json.Parse(Json.Write(captured));

        List<FrameLog.Line> logWas;

        [SetUp]
        public void Keep() => logWas = new List<FrameLog.Line>(FrameLog.Lines);

        [TearDown]
        public void PutBack()
        {
            FrameLog.Lines.Clear();
            FrameLog.Lines.AddRange(logWas);
            PersonTalk.Restore(null);
            FrameWait.Restore(null);
            Course.Restore(null);
        }

        // ------------------------------------------------------------------ the windows and their sizes

        [Test]
        public void TheSizesOfTheWindowsAreKeptWithTheArrangement()
        {
            var ids = new[] { "voyage", "hold", "trade", "port", "people", "self", "bonds", "faith", "tongues", "house", "tasks", "word", "chronicle" };
            var w = new FrameWindows(() => ids);
            Assert.AreEqual(FrameWindows.Medium, w.SizeOf(0), "a window is of the middle size until it is set");
            int version = w.Version;
            w.SetSize(1, FrameWindows.Large);
            Assert.Greater(w.Version, version, "a change of size is a change of the windows");
            w.Go(FrameWindows.PortSpace);
            w.SetSize(0, FrameWindows.Small);
            w.SetSize(2, 99);
            Assert.AreEqual(FrameWindows.Large, w.SizeOf(2), "a size past the largest is the largest");
            string kept = w.Capture();
            StringAssert.Contains("hold:2", kept);
            StringAssert.Contains("trade:0", kept);

            var again = new FrameWindows(() => ids);
            Assert.IsTrue(again.Restore(kept));
            Assert.AreEqual(kept, again.Capture(), "the arrangement comes back the same");
            Assert.AreEqual(FrameWindows.PortSpace, again.Current);
            Assert.AreEqual(FrameWindows.Small, again.SizeOf(0));
            Assert.AreEqual(FrameWindows.Medium, again.SizeOf(1));
            Assert.AreEqual(FrameWindows.Large, again.SizeOf(2));
            again.Go(FrameWindows.SeaSpace);
            Assert.AreEqual(FrameWindows.Large, again.SizeOf(1), "each workspace keeps its own sizes");

            // a window keeps its size when the windows change places, and loses nothing when it goes back to the middle
            again.Set(0, "hold");
            Assert.AreEqual(FrameWindows.Large, again.SizeOf(0), "the size goes with the panel, not with the place");
            again.SetSize(0, FrameWindows.Medium);
            StringAssert.DoesNotContain("hold:", again.Capture().Split(';')[1]);
        }

        [Test]
        public void AnArrangementKeptBeforeTheSizesStillReads()
        {
            var ids = new[] { "voyage", "hold", "trade", "port", "people", "self", "bonds", "faith", "tongues", "house", "tasks", "word", "chronicle" };
            var fresh = new FrameWindows(() => ids);
            string asKeptBefore = fresh.Capture();                        // (no size set: four fields a workspace, as before)
            Assert.AreEqual(4, asKeptBefore.Split(';')[1].Split('|').Length, "with no size set the line is as it was kept before");
            var w = new FrameWindows(() => ids);
            w.SetLayout(FrameLayout.Tabs);
            Assert.IsTrue(w.Restore(asKeptBefore));
            Assert.AreEqual(FrameLayout.Rows, w.Space.layout);
            Assert.AreEqual(FrameWindows.Medium, w.SizeOf(0));
        }

        // ------------------------------------------------------------------ the log

        [Test]
        public void TheLogComesBackAsItWasSaid()
        {
            FrameLog.Lines.Clear();
            FrameLog.Lines.Add(new FrameLog.Line { text = "You cast off from Dorestat for Traiectum.", day = 0.125, date = "1 May", part = "early morning" });
            FrameLog.Lines.Add(new FrameLog.Line { text = "The lookout: a sail sighted, 6 km to the west.", day = 0.25, date = "1 May", part = "morning" });
            FrameLog.Lines.Add(new FrameLog.Line { text = "Arrived at Traiectum.", day = 0.41, date = "1 May", part = "morning" });
            var saved = Saved(FrameLog.Capture()) as List<object>;
            Assert.IsNotNull(saved);
            int version = FrameLog.Version;
            FrameLog.Lines.Clear();
            FrameLog.Lines.Add(new FrameLog.Line { text = "A line of another game.", day = 30.0 });
            FrameLog.Restore(saved);
            Assert.Greater(FrameLog.Version, version, "the frame is told to draw the log again");
            Assert.AreEqual(3, FrameLog.Lines.Count, "the lines of the game that was running are gone, the saved ones are back");
            Assert.AreEqual("You cast off from Dorestat for Traiectum.", FrameLog.Lines[0].text);
            Assert.AreEqual(0.25, FrameLog.Lines[1].day, 1e-9);
            Assert.AreEqual("1 May", FrameLog.Lines[2].date);
            Assert.AreEqual("morning", FrameLog.Lines[2].part);
        }

        [Test]
        public void ASaveFromBeforeTheLogWasKeptLoadsWithAnEmptyLog()
        {
            FrameLog.Lines.Clear();
            FrameLog.Lines.Add(new FrameLog.Line { text = "A line of the game that was running.", day = 3.0 });
            string layoutWas = FrameSave.Layout;
            int loads = FrameSave.Loads;
            FrameSave.Restore(null);                                     // (an older save has no "frame" part)
            Assert.AreEqual(0, FrameLog.Lines.Count, "the log of the game before must not stand over an older save");
            Assert.AreEqual("", FrameSave.Layout, "an older save brings no arrangement: the one on screen stays");
            Assert.AreEqual(loads + 1, FrameSave.Loads, "the frame is told a game was loaded");
            FrameSave.Layout = layoutWas;
        }

        // ------------------------------------------------------------------ the part as a whole

        [Test]
        public void ThePartKeepsWhatWasAskedAndWhatSheWaitsFor()
        {
            var part = new Dictionary<string, object>
            {
                ["log"] = new List<object> { new Dictionary<string, object> { ["text"] = "You wait for a fair wind.", ["day"] = 2.5, ["date"] = "3 May", ["part"] = "midday" } },
                ["windows"] = "1;Sea|0|0|voyage,hold;Port|0|1|trade,port,people|trade:2;Self|2|0|self,bonds,faith,tongues;House|2|0|house,tasks,word,chronicle",
                ["full"] = new List<object> { new Dictionary<string, object> { ["panel"] = "crew", ["left"] = 40.0, ["top"] = 30.0 } },
                ["asked"] = new Dictionary<string, object> { ["dorestad.ravn/rumour"] = 2.0, ["dorestad.ravn/saying"] = 1.0 },
                ["wait"] = new Dictionary<string, object> { ["until"] = "FairWind", ["resume"] = "lundenwic" },
                ["lie_to"] = "witla",
            };
            string layoutWas = FrameSave.Layout, lieWas = FrameSave.LieTo;
            try
            {
                FrameSave.Restore(Saved(part));
                Assert.AreEqual(1, FrameLog.Lines.Count);
                Assert.AreEqual("You wait for a fair wind.", FrameLog.Lines[0].text);
                StringAssert.Contains("trade:2", FrameSave.Layout);
                Assert.AreEqual("witla", FrameSave.LieTo);
                Assert.AreEqual(1, FrameSave.Full.Count, "a full window that stood open is carried, though nothing opens it yet");
                Assert.AreEqual("crew", FrameSave.Full[0].panel);
                Assert.AreEqual(40f, FrameSave.Full[0].left, 1e-4f);
                Assert.AreEqual(2, PersonTalk.Asked("dorestad.ravn", "rumour"), "Ravn has told two rumours: the next asking gives the third");
                Assert.AreEqual(0, PersonTalk.Asked("dorestad.ravn", "riddle"));
                Assert.AreEqual(FrameWait.For.FairWind, FrameWait.Until, "she was waiting for a fair wind");
                Assert.AreEqual("lundenwic", FrameWait.Bound, "and for Lundenwic, the course laid by meanwhile");

                // written out again it is the same (no frame is up in an EditMode test: what was loaded is what is saved)
                var again = Saved(FrameSave.Capture()) as Dictionary<string, object>;
                Assert.IsNotNull(again);
                Assert.AreEqual(Json.Str(part, "windows"), Json.Str(again, "windows"));
                Assert.AreEqual("witla", Json.Str(again, "lie_to"));
                Assert.AreEqual("FairWind", Json.Str(Json.Obj(again, "wait"), "until"));
                Assert.AreEqual(2.0, Json.Num(Json.Obj(again, "asked"), "dorestad.ravn/rumour"), 1e-9);
                Assert.AreEqual(1, Json.Arr(again, "full").Count);
                Assert.AreEqual(1, Json.Arr(again, "log").Count);

                // and a save with none of it leaves nothing of this game behind
                FrameSave.Restore(null);
                Assert.AreEqual(0, PersonTalk.Asked("dorestad.ravn", "rumour"), "what was asked in the game before is not carried into an older save");
                Assert.IsFalse(FrameWait.On, "a wait of the game before must not go on under the loaded one");
                Assert.AreEqual("", FrameSave.LieTo);
                Assert.AreEqual(0, FrameSave.Full.Count);
            }
            finally
            {
                FrameSave.Restore(null);
                FrameSave.Layout = layoutWas;
                FrameSave.LieTo = lieWas;
            }
        }

        // ------------------------------------------------------------------ the course's kinds

        [Test]
        public void TheKindsOfACoursesLegsComeBackOnLoad()
        {
            var saved = new Dictionary<string, object>
            {
                ["dest"] = "dorestad", ["next"] = 2.0, ["km"] = 3.0,
                ["points"] = new List<object>
                {
                    new List<object> { 0.0, 0.0 }, new List<object> { 1000.0, 0.0 },
                    new List<object> { 2000.0, 0.0 }, new List<object> { 3000.0, 0.0 },
                },
                ["water"] = new List<object> { 9999.0, 9999.0, 4.0, 9999.0 },
                ["kinds"] = new List<object> { (double)LaneGraph.Sea, (double)LaneGraph.River, (double)LaneGraph.Sea, (double)LaneGraph.Sea },
            };
            Course.Restore(Saved(saved) as Dictionary<string, object>);
            Assert.AreEqual("dorestad", Course.Destination);
            Assert.AreEqual(4, Course.Kinds.Count, "each point of the line has its leg's kind again");
            Assert.IsTrue(Course.LegIsRiver(1), "the river leg is a river after a load: she rows it");
            Assert.IsFalse(Course.LegIsRiver(2));
            // what Capture writes, Restore reads to the same
            var again = Saved(Course.Capture()) as Dictionary<string, object>;
            Course.Restore(null);
            Assert.AreEqual(0, Course.Kinds.Count);
            Assert.IsFalse(Course.IsSet);
            Course.Restore(again);
            Assert.IsTrue(Course.LegIsRiver(1));
            Assert.AreEqual(2, Course.Next);
            Assert.AreEqual(4.0f, Course.LegWater(2), 1e-6f);
            Assert.AreEqual("dorestad", Course.Destination);
        }

        // ------------------------------------------------------------------ an open card

        const string Toll = "harbour.tollman.frankish";

        /// <summary>A director with the toll man's card alone, and the toll man made a stranger: forged anew each time the
        /// card opens, as the people of most cards are (the toll man himself holds his office, and so was never the fault).</summary>
        static EncounterDirector Director()
        {
            var book = EncounterData.BookOf(Toll);
            foreach (var actor in book.Card(Toll).actors)
                actor.fixedRole = false;
            var d = new EncounterDirector(book, 793, new FakeGame());
            d.portInfo = port => port == "dorestad" ? ("franks", "frisian") : ("", "");
            d.roll = () => 0.0;
            d.vars.nums["port.toll"] = 0.1;
            d.vars.nums["hold.value"] = 0.0;
            d.vars.nums["crew.trade_best"] = 3.0;
            d.vars.numSource = name => name == "silver" ? 100.0 : 0.0;
            d.SetTime(10.0, 793, 6, 240);
            d.state.BeginCall("dorestad", 10.0);
            return d;
        }

        [Test]
        public void AnOpenCardComesBackWithThePeopleItHad()
        {
            var d = Director();
            d.state.Enqueue(Toll, true);
            var s = d.OpenNext();
            Assert.IsNotNull(s, "the card did not open");
            Assert.Greater(s.actors.Count, 0, "the card has nobody in it");
            var faces = s.actors.ToDictionary(a => a.Key, a => a.Value.id);
            var names = s.actors.ToDictionary(a => a.Key, a => a.Value.name);
            var said = s.lines.Select(l => l.text).ToList();
            var choices = s.options.Select(o => o.label).ToList();

            var saved = Saved(d.Capture()) as Dictionary<string, object>;
            Assert.IsNotNull(Json.Obj(saved, "open"), "the open card is not in the saved game");

            var e = Director();
            e.SetTime(10.5, 793, 6, 252);                                 // (loaded and looked at twelve game hours on)
            e.Restore(saved);
            Assert.IsNull(e.Current, "a load shuts the card on screen");
            Assert.AreEqual(Toll, e.KeptCard, "the card is not kept to open again");
            var t = e.OpenNext();
            Assert.IsNotNull(t, "the card did not open again after the load");
            Assert.AreEqual(Toll, t.card.id);
            foreach (var f in faces)
            {
                Assert.AreEqual(f.Value, t.actors[f.Key].id, "another person stands in the part '" + f.Key + "' after the load");
                Assert.AreEqual(names[f.Key], t.actors[f.Key].name, "the person in the part '" + f.Key + "' has another name after the load");
            }
            CollectionAssert.AreEqual(said, t.lines.Select(l => l.text).ToList(), "the card says something else after the load");
            CollectionAssert.AreEqual(choices, t.options.Select(o => o.label).ToList());
            Assert.AreEqual(0, e.state.queue.Count(id => id == Toll), "the card is in line a second time");
            Assert.IsNull(e.KeptCard);

            // saved again before it has opened again: still kept, still first in line
            var g = Director();
            g.Restore(saved);
            var again = Saved(g.Capture()) as Dictionary<string, object>;
            Assert.AreEqual(Toll, Json.Str(Json.Obj(again, "open"), "card"));
            Assert.AreEqual(Toll, Json.Arr(Json.Obj(again, "state"), "queue")[0]);
            var h = Director();
            h.Restore(again);
            var u = h.OpenNext();
            foreach (var f in faces)
                Assert.AreEqual(f.Value, u.actors[f.Key].id, "saved twice, the people are still the same");
        }

        [Test]
        public void AnOlderSaveWithoutItsCastStillOpensTheCard()
        {
            var d = Director();
            d.state.Enqueue(Toll, true);
            Assert.IsNotNull(d.OpenNext());
            var saved = Saved(d.Capture()) as Dictionary<string, object>;
            saved.Remove("open");                                        // (as a save of before 7 October has it: only first in line)
            var e = Director();
            e.Restore(saved);
            Assert.IsNull(e.KeptCard);
            var t = e.OpenNext();
            Assert.IsNotNull(t, "an older save's open card did not open again");
            Assert.AreEqual(Toll, t.card.id);
        }

        [Test]
        public void ACardAnsweredBeforeTheSaveIsNotBroughtBack()
        {
            var d = Director();
            d.state.Enqueue(Toll, true);
            var s = d.OpenNext();
            int first = s.options.FindIndex(o => o.enabled);
            Assert.GreaterOrEqual(first, 0);
            Assert.IsTrue(d.Choose(first).ok);
            var saved = Saved(d.Capture()) as Dictionary<string, object>;
            Assert.IsNull(Json.Obj(saved, "open"), "a card already answered has done what it does: it is not opened again");
            var e = Director();
            e.Restore(saved);
            Assert.IsNull(e.OpenNext());
        }
    }
}
