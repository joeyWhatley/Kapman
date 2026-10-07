using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE TONGUES AND THE TURNING LINE (7 Oct 2026; Design/fate_faith_and_self.md section 6c): the tongues file is
    /// read (tongues with their kin, attested words, real lines in pieces); a tongue is followed through its kin; a line
    /// comes up in the speaker's tongue and turns into English piece by piece: quickly and wholly for a tongue you
    /// know (well under two seconds), slower with the hard pieces left for one you half know, not at all for one you
    /// do not have; a piece seen often stays English; a word set into a line of English is marked, with its meaning for
    /// the hover; and the people of the first ports have a word to hail with and, where the file has one, a true line.
    /// </summary>
    public class TongueTests
    {
        static readonly string[] FirstPorts =
            { "dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic", "ripa" };

        string peopleWas;

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
            ContentTestKit.Done();
            Self.Reset();
            MapPalette.SetPeople(peopleWas);
        }

        static string Theirs(TongueLine l) => string.Join(" ", l.pairs.Select(p => p.original));
        static string Ours(TongueLine l) => string.Join(" ", l.pairs.Select(p => p.english));

        // ------------------------------------------------------------------ the book

        [Test]
        public void TheTonguesFileIsRead()
        {
            var book = TongueBook.Current;
            Assert.IsTrue(book.FromFile, "story/tongues_793.json was not read: " + string.Join("; ", ContentFiles.Warnings));
            Assert.AreEqual("Norse", book.Name("old_norse"));
            Assert.AreEqual("Latin", book.Name("latin"));
            Assert.AreEqual("", book.Name("no_such_tongue"));
            Assert.AreEqual("old_norse", book.Of("danish"));
            Assert.AreEqual("old_frisian", book.Of("frisian"));
            Assert.AreEqual("old_english", book.Of("english"));
            Assert.AreEqual("", book.Of("nobody"));
            var heill = book.Word("old_norse", "Heill");
            Assert.IsNotNull(heill, "the Norse greeting");
            Assert.AreEqual("greeting", heill.kind);
            StringAssert.Contains("said " + heill.say, book.Tip(heill));
            StringAssert.Contains("Norse: ", book.Tip(heill));
            Assert.AreEqual("wes hāl", book.Greeting("old_english").word);
            Assert.IsNull(book.Greeting("old_frisian"), "nothing in Frisian is written down so early: no greeting is made up");

            var line = book.Line("on_hav_076", 793);
            Assert.IsNotNull(line);
            Assert.AreEqual("old_norse", line.tongue);
            Assert.AreEqual(6, line.pairs.Count);
            Assert.AreEqual(line.original, Theirs(line), "the pieces' left sides, joined, are the line");
            Assert.AreEqual("Hávamál 76", line.Where);
            Assert.AreEqual("Psalm 106:23-24", book.Line("la_ps106_23", 793).Where);
            // a line is not given out before it was spoken, nor while it is only quoted from memory
            Assert.IsNull(book.Line("os_hel_storm", 793), "the Heliand is of about 830");
            Assert.IsNotNull(book.Line("os_hel_storm", 830));
            Assert.IsNotNull(book.LineRaw("la_mass_ite"));
            Assert.IsNull(book.Line("la_mass_ite", 900), "quoted from memory: to be checked before it is shown");
            Assert.IsNull(book.Line("no_such_line", 793));
            // what the file promises of every line: pieces that join into the original
            int lines = 0, whole = 0;
            foreach (var l in book.Lines)
            {
                lines++;
                if (l.pairs.Count > 0 && Theirs(l) == l.original)
                    whole++;
            }
            Debug.Log($"Tongues: {book.Tongues.Count} tongues, {book.Words.Count} words, {lines} lines; {whole} lines whose pieces join into their original exactly.");
            Assert.Greater(lines, 80);
            Assert.Greater(whole, lines * 3 / 4, "most lines' pieces should join into the original");
        }

        [Test]
        public void ATongueIsFollowedThroughItsKin()
        {
            Self.Begin("frisian", "christian", "steersmans_child");
            Assert.AreEqual(1.0, Self.TongueLevel("old_frisian"), 1e-9);
            Assert.AreEqual(0.3, Self.TongueLevel("old_norse"), 1e-9, "a Frisian follows three parts in ten of Norse");
            Assert.AreEqual(0.0, Self.TongueHeld("old_norse"), 1e-9, "and holds none of it");
            Assert.AreEqual(0.6, Self.TongueLevel("old_english"), 1e-9, "more through his own tongue than the half he has learned");
            Assert.AreEqual(0.0, Self.TongueLevel("irish"), 1e-9, "Irish is kin to nothing he has");
            Assert.AreEqual(0.0, Self.TongueLevel("latin"), 1e-9);
            Self.Begin("danish", "old_gods", "hall_child");
            Assert.AreEqual(1.0, Self.TongueLevel("old_norse"), 1e-9);
            Assert.AreEqual(0.35, Self.TongueLevel("old_english"), 1e-9);
            Assert.AreEqual("Norse", Self.TongueName("old_norse"));
            Assert.AreEqual("Norse", Self.TongueName("norse"));
        }

        // ------------------------------------------------------------------ how a line turns

        [Test]
        public void ATongueYouKnowTurnsWhollyAndQuickly()
        {
            var line = TongueBook.Current.Line("on_hav_076", 793);
            var plan = TurnPlan.Make(line.pairs, 1.0, null);
            Assert.AreEqual(6, plan.Turning);
            Assert.AreEqual(0, plan.Staying);
            Assert.Less(plan.Duration, 1.5f, "well under two seconds for a tongue you know");
            Assert.Greater(plan.Duration, 0.3f, "but it is seen to turn");
            Assert.AreEqual(line.original, plan.ReadAt(0f), "it comes up in the speaker's tongue");
            Assert.AreEqual(Ours(line), plan.ReadAt(plan.Duration + 0.2f), "and ends in English");
            for (int i = 1; i < plan.pieces.Length; i++)
                Assert.Greater(plan.pieces[i].at, plan.pieces[i - 1].at, "piece by piece, in order");
            // in the middle: the first pieces English, the last still as they were
            float mid = (plan.pieces[2].at + plan.pieces[3].at) * 0.5f;
            Assert.AreEqual(line.pairs[0].english, plan.TextAt(0, mid));
            Assert.AreEqual(line.pairs[5].original, plan.TextAt(5, mid));
            // a piece goes out and comes back: at the very moment of its turn it is all but gone
            Assert.Less(plan.Opacity(2, plan.pieces[2].at), 0.1f);
            Assert.AreEqual(1f, plan.Opacity(2, 0f), 1e-6f);
            Assert.AreEqual(1f, plan.Opacity(2, plan.Duration + 1f), 1e-6f);
            // the longest line in the file still turns in well under two seconds
            foreach (var l in TongueBook.Current.Lines)
                if (l.pairs.Count > 0)
                    Assert.Less(TurnPlan.Make(l.pairs, 1.0, null).Duration, 1.5f, l.id);
        }

        [Test]
        public void ATongueYouHalfKnowTurnsSlowerAndLeavesTheHardPieces()
        {
            var line = TongueBook.Current.Line("on_hav_076", 793);
            var known = TurnPlan.Make(line.pairs, 1.0, null);
            var half = TurnPlan.Make(line.pairs, 0.5, null);
            Assert.Greater(half.Staying, 0, "the hard pieces stay as they were");
            Assert.Greater(half.Turning, 0, "but you get the gist");
            Assert.Greater(half.Duration, known.Duration, "and it turns slower");
            int longest = Enumerable.Range(0, line.pairs.Count).OrderByDescending(i => line.pairs[i].original.Length).First();
            Assert.Less(half.pieces[longest].at, 0f, "the longest piece is the hardest");
            string end = half.ReadAt(60f);
            StringAssert.Contains(line.pairs[longest].original, end);
            Assert.AreNotEqual(line.original, end);
            Assert.AreNotEqual(Ours(line), end);
            // the less you know, the more is lost
            Assert.Greater(TurnPlan.Make(line.pairs, 0.3, null).Staying, TurnPlan.Make(line.pairs, 0.8, null).Staying);
            // a hard piece seen a few times is hard no longer: another stays in its place
            var seen = TurnPlan.Make(line.pairs, 0.75, w => w == line.pairs[longest].original ? TurnPlan.EasyAfter : 0);
            Assert.Greater(seen.pieces[longest].at, 0f);
            Assert.AreEqual(1, seen.Staying);
        }

        [Test]
        public void ATongueYouDoNotKnowDoesNotTurnAndAPieceSeenOftenStaysEnglish()
        {
            var line = TongueBook.Current.Line("la_ps106_23", 793);
            var none = TurnPlan.Make(line.pairs, 0.1, null);
            Assert.AreEqual(0, none.Turning);
            Assert.AreEqual(0f, none.Duration, 1e-6f);
            Assert.AreEqual(line.original, none.ReadAt(30f), "it stands as it was said");
            // the tenth time a piece is heard it is English from the start, whatever the skill
            string first = line.pairs[0].original;
            var learned = TurnPlan.Make(line.pairs, 0.0, w => w == first ? Self.StaysAfter : 0);
            Assert.AreEqual(1, learned.FromStart);
            Assert.AreEqual(line.pairs[0].english, learned.TextAt(0, 0f));
            Assert.AreEqual(line.pairs[1].original, learned.TextAt(1, 30f));
            // and the self keeps the count: heard through a turning line's pieces
            Self.Begin("frisian", "christian", "minster_schooled");
            for (int i = 0; i < Self.StaysAfter; i++)
                Self.See(first);
            var turn = FloatTurn.Of(line);
            Assert.AreEqual(1.0, turn.skill, 1e-9, "the minster's schooling is Latin");
            Assert.AreEqual(1, turn.Plan.FromStart);
            Assert.AreEqual(line.pairs.Count - 1, turn.Plan.Turning);
        }

        // ------------------------------------------------------------------ on the screen

        [Test]
        public void TheTurningLineShowsItsPiecesAndSaysWhatWasMadeOfIt()
        {
            Self.Begin("danish", "old_gods", "hall_child");
            var line = TongueBook.Current.Line("on_hav_076", 793);
            var el = new TurningLine(FloatTurn.Of(line));
            var pieces = el.Query<Label>(className: "frame-turn-piece").ToList();
            Assert.AreEqual(6, pieces.Count);
            el.HoldAt(0f);
            Assert.AreEqual(line.original, el.Reads);
            Assert.IsFalse(pieces.Any(p => p.ClassListContains("eng")));
            Assert.AreEqual(line.pairs[0].original, pieces[0].text);
            var plan = el.Turn.Plan;
            el.HoldAt((plan.pieces[2].at + plan.pieces[3].at) * 0.5f);
            Assert.AreEqual(3, pieces.Count(p => p.ClassListContains("eng")), "caught in mid-turn: half of it English");
            Assert.AreEqual(line.pairs[0].english, pieces[0].text);
            Assert.AreEqual(line.pairs[5].original, pieces[5].text);
            Assert.IsFalse(el.Done);
            el.HoldAt(plan.Duration + 0.5f);
            Assert.IsTrue(el.Done);
            Assert.AreEqual(Ours(line), el.Reads);
            Assert.AreEqual("Norse. Hávamál 76.", el.Turn.Note());

            // a Frisian with no Latin: nothing turns, and the sense is given him
            Self.Begin("frisian", "christian", "steersmans_child");
            var psalm = TongueBook.Current.Line("la_ps106_23", 793);
            var latin = FloatTurn.Of(psalm);
            Assert.AreEqual(0, latin.Plan.Turning);
            StringAssert.StartsWith("Latin. Psalm 106:23-24. You do not have the tongue.", latin.Note());
            StringAssert.Contains(psalm.english, latin.Note());
            // and half of Norse through his own tongue: some of it is lost on him
            var norse = FloatTurn.Of(line);
            Assert.AreEqual(0.3, norse.skill, 1e-9);
            Assert.Greater(norse.Plan.Staying, 0);
            StringAssert.Contains("half follow", norse.Note());
        }

        [Test]
        public void AWordOfAnotherTongueInALineIsSetApartWithItsMeaning()
        {
            var book = TongueBook.Current;
            var heill = book.Word("old_norse", "heill");
            var runs = book.Mark("Heill. You are late.", "old_norse", new[] { heill });
            Assert.AreEqual(2, runs.Count);
            Assert.AreSame(heill, runs[0].word);
            Assert.AreEqual("Heill", runs[0].text, "as it was written in the line");
            Assert.IsNull(runs[1].word);
            Assert.AreEqual(". You are late.", runs[1].text);
            // an English word is never taken for a word of the old tongue that is spelled the same
            Assert.IsNotNull(book.Word("old_english", "wine"), "Old English wine, a friend, is in the file");
            Assert.IsFalse(book.Mark("The wine is in the port, and the toll is paid.", "old_english").Any(r => r.word != null));
            // but a word English could not have is found by itself, whole words only
            var felagi = book.Mark("My félagi waits at the boat.", "old_norse");
            Assert.AreEqual(3, felagi.Count);
            Assert.AreEqual("félagi", felagi[1].word.word);
            Assert.IsFalse(book.Mark("The félagiship of the bench.", "old_norse").Any(r => r.word != null), "not inside a longer word");
            Assert.IsFalse(book.Mark("My félagi waits.", "latin").Any(r => r.word != null), "only the speaker's tongue is looked for");
            Assert.AreEqual(0, book.Mark("", "old_norse").Count);

            var el = new SprinkledText(runs);
            var labels = el.Query<Label>().ToList();
            Assert.IsTrue(labels[0].ClassListContains("word"));
            Assert.AreEqual(book.Tip(heill), labels[0].tooltip, "its meaning on hover");
            Assert.AreEqual(".", labels[1].text);
            Assert.IsTrue(labels[1].ClassListContains("glue"), "the full stop stands against the word");
            CollectionAssert.AreEqual(new[] { "Heill", ".", "You", "are", "late." }, labels.Select(l => l.text).ToList());
            Assert.AreEqual(1, el.Marked.Count);
        }

        // ------------------------------------------------------------------ who says what

        [Test]
        public void ThePeopleOfTheFirstPortsHaveAHailAndWhereTheFileHasOneATrueLine()
        {
            var talk = TalkTongues.Current;
            var book = TongueBook.Current;
            Assert.IsTrue(talk.FromFile, "story/talk_tongues_793.json was not read: " + string.Join("; ", ContentFiles.Warnings));
            // every word and line the rules name is one of the tongues file, and may be given out in 793
            foreach (var r in talk.rules)
            {
                if (r.hailWord.Length > 0)
                    Assert.IsNotNull(book.Word(r.hailTongue, r.hailWord), $"the tongues file has no {r.hailTongue} word '{r.hailWord}'");
                if (r.line.Length > 0)
                {
                    Assert.IsNotNull(book.LineRaw(r.line), "the tongues file has no line " + r.line);
                    Assert.IsNotNull(book.Line(r.line, 793), r.line + " may not be given out in 793 (quoted from memory, or later)");
                }
            }
            var places = PlaceBook.Current;
            var ravn = places.Person("dorestad.ravn");
            Assert.AreEqual("heill", talk.Hail(ravn).word);
            Assert.AreEqual("on_hav_076", talk.LineOf(ravn, 793, out string on).id);
            Assert.AreEqual("saying", on, "his saying is that verse");
            var priest = places.Person("dorestad.priest");
            Assert.AreEqual("Dominus vobiscum", talk.Hail(priest).word, "a churchman greets in Latin whatever his people");
            Assert.AreEqual("la_ps106_23", talk.LineOf(priest, 793, out on).id);
            Assert.AreEqual("greet", on);
            var tollman = places.Person("dorestad.tollman");
            Assert.AreEqual("kāpmon", talk.Hail(tollman).word, "a Frisian calls you what you are");
            Assert.IsNull(talk.LineOf(tollman, 793, out _), "no whole line in Frisian exists so early: none is made up");
            var count = places.Person("dorestad.folcbert");
            Assert.AreEqual("heil", talk.Hail(count).word);
            Assert.IsNull(talk.LineOf(count, 793, out _), "the Frankish lines a layman would say are all later than 793");

            int people = 0, hailed = 0, lined = 0;
            foreach (string port in FirstPorts)
                foreach (var p in places.People(port, 793))
                {
                    people++;
                    if (talk.Hail(p) != null)
                        hailed++;
                    if (talk.LineOf(p, 793, out _) != null)
                        lined++;
                }
            Debug.Log($"Tongues: of {people} people of the twelve first ports in 793, {hailed} have a word to hail with and {lined} a true line.");
            Assert.Greater(people, 40);
            Assert.AreEqual(people, hailed, "everyone of the first ports has a word to hail with");
            Assert.Greater(lined, 20);
        }

        [Test]
        public void ATalkCarriesTheHailAndTheTrueLine()
        {
            ContentTestKit.LoadPorts();
            Self.Begin("frisian", "christian", "steersmans_child");
            var places = PlaceBook.Current;
            var port = GameData.Port("dorestad");
            var ravn = places.Person("dorestad.ravn");
            var greet = PersonTalk.Spec(ravn, null, port, "greet", 0, null, null, 793, false);
            StringAssert.StartsWith("Heill. ", greet.lines[0].text);
            Assert.AreEqual("heill", greet.marks[0].word);
            Assert.AreEqual("old_norse", greet.tongue);
            Assert.AreEqual(0, greet.turns.Count, "his true line is his saying, not his greeting");
            var saying = PersonTalk.Spec(ravn, null, port, "saying", 0, null, null, 793, true);
            Assert.AreEqual(1, saying.turns.Count, "the verse comes up in Norse and turns");
            Assert.AreEqual(6, saying.turns[0].pairs.Count);
            Assert.AreEqual(ravn.Line("saying"), saying.lines[0].text, "and his own words follow, with no hail");
            Assert.AreEqual(0, saying.marks.Count);
            // the priest: Latin at the first greeting, and not again
            var priest = places.Person("dorestad.priest");
            var first = PersonTalk.Spec(priest, null, port, "greet", 0, null, null, 793, false);
            Assert.AreEqual(1, first.turns.Count);
            Assert.AreEqual("latin", first.turns[0].tongue);
            StringAssert.StartsWith("Dominus vobiscum. ", first.lines[0].text);
            Assert.AreEqual(0, PersonTalk.Spec(priest, null, port, "greet", 0, null, null, 793, true).turns.Count, "met before: not said again");
            Assert.AreEqual(0, PersonTalk.Spec(priest, null, port, "rumour", 0, null, null, 793, true).turns.Count);
        }

        [Test]
        public void ACardLineMayNameARealLineToTurnFrom()
        {
            const string card =
                "{\"id\":\"test.turn\",\"stage\":\"harbour\",\"arrival\":true,\"when\":\"true\",\"cooldown_days\":0," +
                "\"title_key\":\"test.turn.title\",\"title\":\"A verse on the quay\"," +
                "\"lines\":[{\"key\":\"test.turn.verse\",\"tongue\":\"old_norse\",\"turns\":\"on_hav_076\"},{\"key\":\"test.turn.other\",\"turns\":\"no_such_line\"}]," +
                "\"options\":[{\"key\":\"ok\",\"label_key\":\"test.turn.ok\",\"label\":\"Go on\",\"check\":\"true\"}]}";
            var src = ContentTestKit.Tables();
            src.cards.Add(("encounters/test_turn.json", "{\"cards\":[" + card + "]}"));
            var book = EncounterLoader.Load(src);
            Assert.IsTrue(book.Ok, string.Join("\n", book.errors));
            Assert.AreEqual("on_hav_076", book.Card("test.turn").lines[0].turns);
            Assert.IsFalse(book.warnings.Any(w => w.Contains("turns")), "the loader knows the field: " + string.Join("; ", book.warnings));
            var d = new EncounterDirector(book, 793, new FakeGame());
            d.portInfo = port => ("danes", "danish");
            d.SetTime(10.0, 793, 6, 240);
            d.BeginCall("ripa");
            var s = d.OpenNext();
            Assert.IsNotNull(s);
            Assert.AreEqual("on_hav_076", s.lines[0].turns);
            Self.Begin("danish", "old_gods", "hall_child");
            var spec = FrameRoot.CardSpec(d, s);
            Assert.AreEqual(1, spec.turns.Count, "the real line turns; a name the file does not have is passed over");
            Assert.AreEqual(1.0, spec.turns[0].skill, 1e-9);
            Assert.AreEqual(2, spec.lines.Count, "the card's own lines stand under it");
            Assert.AreEqual("old_norse", spec.tongue);
            Assert.IsTrue(ContentFiles.Warnings.Any(w => w.Contains("no_such_line")), "and it is said once");
        }
    }
}
