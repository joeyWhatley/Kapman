using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE LOG IS SHORT (the user, 10 Oct 2026: "I feel like there's too much info at the bottom maybe have just a much
    /// more simplified snappier version of information and if you hover over it has an info box pop up with the more key
    /// details but laid out like / Plus however many days", "Like how ck does it"; by the picker: "The log's lines",
    /// "Short, with signed numbers"). Held here, without the screen:
    ///   - a telling of several sentences shows its first, and keeps the whole, a sentence a row, in its box;
    ///   - a line in the short form has its figure, its box and its whole words;
    ///   - a notice of the voyage comes with its short form, and every other listener still has the whole words;
    ///   - a saved game keeps all of it;
    ///   - an arrival is the line and the box of the mock-up the user picked;
    ///   - the short words keep the page's rule.
    /// What it looks like, and the box that comes up under the pointer, are the play tests' (Captures/toll, Captures/sea).
    /// </summary>
    public class SnappyLogTests
    {
        List<FrameLog.Line> logWas;

        [SetUp]
        public void Keep()
        {
            logWas = new List<FrameLog.Line>(FrameLog.Lines);
            FrameLog.Lines.Clear();
        }

        [TearDown]
        public void Back()
        {
            FrameLog.Lines.Clear();
            FrameLog.Lines.AddRange(logWas);
        }

        [Test]
        public void ATellingOfSeveralSentencesShowsItsFirstAndKeepsTheWhole()
        {
            FrameLog.Add("You hail a ship of Ripa. She does not answer. Her men are at the oars, and they pull away.");
            Assert.AreEqual(1, FrameLog.Lines.Count);
            var l = FrameLog.Lines[0];
            Assert.AreEqual("You hail a ship of Ripa.", l.text, "the line is not the telling's first sentence");
            Assert.AreEqual("You hail a ship of Ripa", FrameLog.Shown(l.text), "the line is shown with its full stop");
            Assert.AreEqual("You hail a ship of Ripa. She does not answer. Her men are at the oars, and they pull away.", l.Whole, "the whole words are not kept");
            Assert.IsNotNull(l.box, "the rest of the telling has no box");
            CollectionAssert.AreEqual(new[] { "You hail a ship of Ripa.", "She does not answer.", "Her men are at the oars, and they pull away." }, l.box.rows.Select(r => r.label).ToList());
            Assert.IsTrue(l.box.rows.All(r => r.words), "a sentence of the telling is laid out as a figure");
            // one sentence is all there is: no box, and the text is the whole
            FrameLog.Add("Night falls.");
            Assert.AreEqual("Night falls.", FrameLog.Lines[1].text);
            Assert.AreEqual("Night falls.", FrameLog.Lines[1].Whole);
            Assert.IsNull(FrameLog.Lines[1].box);
            Assert.AreEqual("", FrameLog.Lines[1].figure);
            // a sum with a point in it, and a colon, are not the ends of sentences
            CollectionAssert.AreEqual(new[] { "Sold 4 casks for 208.1 d: took 208 deniers.", "That is 96 d more than they cost you." },
                                      FrameLog.Sentences("Sold 4 casks for 208.1 d: took 208 deniers. That is 96 d more than they cost you."));
            CollectionAssert.AreEqual(new[] { "She lies to." }, FrameLog.Sentences("She lies to."));
            Assert.IsEmpty(FrameLog.Sentences(""));
            // what somebody says is not cut: nothing is cut between quote marks
            string ravn = "\"Look at that! It is worth more here than it was there. Sell before you eat.\" says Ravn.";
            CollectionAssert.AreEqual(new[] { ravn }, FrameLog.Sentences(ravn), "a saying was cut in the middle");
            CollectionAssert.AreEqual(new[] { "You gave \u201cA black line\u201d no answer, so it went this way: Go on.", "The squall passed." },
                                      FrameLog.Sentences("You gave \u201cA black line\u201d no answer, so it went this way: Go on. The squall passed."));
            // the same words twice running in the same hour are said once, in the short form too
            FrameLog.Add("Night falls.");
            Assert.AreEqual(2, FrameLog.Lines.Count);
        }

        [Test]
        public void AShortLineHasItsFigureItsBoxAndItsWholeWords()
        {
            var box = new LogBox("The men's pay").Row("Paid", "1 d", LogBox.Signed(-1.0)).Row("Still owed", "3 d");
            FrameLog.Short("The men paid", LogBox.Signed(-1.0), box, "The men are paid 1 d. Still owed 3 d.");
            var l = FrameLog.Lines[0];
            Assert.AreEqual("The men paid", l.text);
            Assert.AreEqual("-1 d", l.figure, "the number that changed is not at the line's end, signed");
            Assert.AreEqual("The men are paid 1 d. Still owed 3 d.", l.Whole);
            Assert.AreEqual("The men's pay\nPaid  1 d  -1 d\nStill owed  3 d", l.box.Text());
            Assert.AreEqual("+6 d", LogBox.Signed(6.4));
            Assert.AreEqual("-37 d", LogBox.Signed(-37.0));
            Assert.AreEqual("", LogBox.Signed(0.2), "nothing changed, and a figure is shown");
            Assert.AreEqual("+3 km", LogBox.Signed(3.0, "km"));
            // a short line with no whole words is its own whole; with no short line, the whole is told as any telling
            FrameLog.Short("Casks filled", "", null);
            Assert.AreEqual("Casks filled", FrameLog.Lines[1].Whole);
            FrameLog.Short("", "", null, "The oars come in. The sail goes up.");
            Assert.AreEqual("The oars come in.", FrameLog.Lines[2].text);
            Assert.AreEqual("The oars come in. The sail goes up.", FrameLog.Lines[2].Whole);
            // (and a notice told with an empty short form is told as any telling: it once called itself for ever)
            System.Action<string> log = FrameLog.Add;
            Voyage.Notice += log;
            try
            {
                Voyage.Say("She waits for the tide. It turns at dusk.", "", "", null);
            }
            finally
            {
                Voyage.Notice -= log;
            }
            Assert.AreEqual("She waits for the tide.", FrameLog.Lines[3].text);
            Assert.AreEqual("She waits for the tide. It turns at dusk.", FrameLog.Lines[3].Whole);
            FrameLog.Lines.RemoveAt(3);
            // an empty box is no box
            FrameLog.Short("She lies to", "", new LogBox());
            Assert.IsNull(FrameLog.Lines[3].box);
        }

        [Test]
        public void ANoticeOfTheVoyageComesWithItsShortFormAndTheWholeWordsStillReachEveryListener()
        {
            var heard = new List<string>();
            System.Action<string> listener = heard.Add, log = FrameLog.Add;
            Voyage.Notice += listener;
            Voyage.Notice += log;
            try
            {
                Voyage.Say("The water casks are filled.", "Casks filled", "", null);
                Voyage.Say("Wind and tide have set her 8 km to the east of her line.", "Off her line", "8 km", new LogBox("Off her line").Row("Set by wind and tide", "8 km"));
                Voyage.Say("Night falls.");
            }
            finally
            {
                Voyage.Notice -= listener;
                Voyage.Notice -= log;
            }
            CollectionAssert.AreEqual(new[] { "The water casks are filled.", "Wind and tide have set her 8 km to the east of her line.", "Night falls." }, heard,
                                      "a listener of the voyage's notices did not get the whole words");
            Assert.AreEqual(3, FrameLog.Lines.Count);
            Assert.AreEqual("Casks filled", FrameLog.Lines[0].text);
            Assert.AreEqual("The water casks are filled.", FrameLog.Lines[0].Whole);
            Assert.AreEqual("Off her line", FrameLog.Lines[1].text);
            Assert.AreEqual("8 km", FrameLog.Lines[1].figure);
            Assert.AreEqual("Off her line\nSet by wind and tide  8 km", FrameLog.Lines[1].box.Text());
            Assert.AreEqual("Night falls.", FrameLog.Lines[2].text);
            Assert.IsNull(Voyage.SaidShort, "the short form of a notice is still held after it was told");
            Assert.IsNull(Voyage.SaidBox);
        }

        [Test]
        public void ASavedGameKeepsTheFiguresTheBoxesAndTheWholeWords()
        {
            FrameLog.Short("Quentovic, 1 day from Serrae", "", new LogBox("Serrae to Quentovic").Row("Passage", "1 day").Row("Cargo cost", "377 d").Sub("would fetch here", "340 d", "-37 d"),
                           "Serrae to Quentovic, 1 day. The purse holds 30 d.", "slip");
            FrameLog.Short("The men paid", "-1 d", null, "The men are paid 1 d.");
            FrameLog.Add("You hail her. She does not answer.");
            FrameLog.Add("Night falls.");
            // (as a saved game has it: written out and read back)
            var saved = Json.Parse(Json.Write(FrameLog.Capture())) as List<object>;
            Assert.IsNotNull(saved);
            FrameLog.Lines.Clear();
            FrameLog.Restore(saved);
            Assert.AreEqual(4, FrameLog.Lines.Count);
            var slip = FrameLog.Lines[0];
            Assert.AreEqual("Quentovic, 1 day from Serrae", slip.text);
            Assert.AreEqual("slip", slip.kind);
            Assert.AreEqual("Serrae to Quentovic, 1 day. The purse holds 30 d.", slip.Whole);
            Assert.AreEqual("Serrae to Quentovic\nPassage  1 day\nCargo cost  377 d\n  would fetch here  340 d  -37 d", slip.box.Text(), "the box did not come back as it was saved");
            Assert.IsTrue(slip.box.rows[2].sub, "a row that hung under another came back as a row of its own");
            Assert.AreEqual("-1 d", FrameLog.Lines[1].figure);
            Assert.IsNull(FrameLog.Lines[1].box);
            Assert.AreEqual("You hail her.", FrameLog.Lines[2].text);
            Assert.AreEqual("You hail her. She does not answer.", FrameLog.Lines[2].Whole);
            Assert.AreEqual(2, FrameLog.Lines[2].box.rows.Count);
            Assert.IsTrue(FrameLog.Lines[2].box.rows[0].words);
            Assert.AreEqual("Night falls.", FrameLog.Lines[3].Whole);
            // a save from before the short form: the text alone, and it is the whole
            FrameLog.Lines.Clear();
            FrameLog.Restore(new List<object> { new Dictionary<string, object> { ["text"] = "Arrived at Traiectum.", ["day"] = 0.4 } });
            Assert.AreEqual("Arrived at Traiectum.", FrameLog.Lines[0].Whole);
            Assert.AreEqual("", FrameLog.Lines[0].figure);
            Assert.IsNull(FrameLog.Lines[0].box);
        }

        /// <summary>The mock-up the user picked: the line "Quentovic, 1 day from Serrae", and under the pointer a box
        /// headed "Serrae to Quentovic" with the passage, the purse, what the cargo cost and would fetch (and the
        /// difference, signed), what is owed and the days left. The whole words are the slip as it was.</summary>
        [Test]
        public void AnArrivalIsTheLineAndTheBoxOfTheUsersMockUp()
        {
            var l = new Landings.Landing { from = "Serrae", to = "Quentovic", leftDay = 3.0, day = 4.0, silver = 30.0, cost = 377.0, fetch = 340.0, debt = 1680.0, daysLeft = 464.0 };
            Assert.AreEqual("Quentovic, 1 day from Serrae", FrameGame.LandingShort(l));
            Assert.AreEqual("Serrae to Quentovic\nPassage  1 day\nPurse  30 d\nCargo cost  377 d\n  would fetch here  340 d  -37 d\nOwed  1680 d\n  days left  464",
                            FrameGame.LandingBox(l).Text());
            Assert.AreEqual("Serrae to Quentovic, 1 day. The purse holds 30 d. What she carries cost 377 d and would fetch about 340 d here. Owed 1680 d, with 464 days left.",
                            FrameGame.LandingWords(l), "the whole words of the slip are not what they were");
            // nothing aboard, nothing owed, and out and back
            var empty = new Landings.Landing { from = "Quentovic", to = "Quentovic", leftDay = 1.0, day = 3.0, silver = 300.0 };
            Assert.AreEqual("Quentovic again, 2 days out", FrameGame.LandingShort(empty));
            Assert.AreEqual("Out from Quentovic and back\nPassage  2 days\nPurse  300 d\nCargo  nothing to sell", FrameGame.LandingBox(empty).Text());
            var first = new Landings.Landing { to = "Quentovic", silver = 30.0, fetch = 40.0, debt = 100.0, daysLeft = 0.0 };
            Assert.AreEqual("Made fast at Quentovic", FrameGame.LandingShort(first));
            Assert.AreEqual("Quentovic\nPurse  30 d\nCargo would fetch here  40 d\nOwed  100 d\n  the day has come", FrameGame.LandingBox(first).Text());
            Assert.AreEqual("", FrameGame.LandingShort(null));
            Assert.IsNull(FrameGame.LandingBox(null));
            // the page's rule, over the short words and every row
            foreach (var landing in new[] { l, empty, first })
            {
                Assert.IsNull(PageWords.Fault(FrameGame.LandingShort(landing), prose: true), FrameGame.LandingShort(landing));
                var box = FrameGame.LandingBox(landing);
                Assert.IsNull(PageWords.Fault(box.head, tooltip: true), box.head);
                foreach (var r in box.rows)
                    Assert.IsNull(PageWords.Fault(r.label + " " + r.value + " " + r.change, tooltip: true), r.label);
            }
        }
    }
}
