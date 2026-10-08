using System.Linq;
using Kapman.Encounters;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE WRITTEN END OF A CHOICE (8 Oct 2026). The script's paragraphs of what came of a choice are found by the
    /// key of the choice's label (Outcomes), carried on the choice's result, and shown by the card's window: the
    /// choice told back small, the end in the teller's voice, the effects' facts in the box.
    /// </summary>
    public class OutcomeTests
    {
        static EncounterWorld World(EncounterBook book, NpcRegistry npcs, string port, double roll)
        {
            return new EncounterWorld
            {
                book = book, state = new EncounterState(), npcs = npcs, game = new FakeGame(), vars = new EncounterVars(),
                port = new PortFacts { id = port, realm = "franks", faith = "christian", peoples = { "frisian" } }, year = 793, day = 3.0,
                roll = () => roll,
            };
        }

        [Test]
        public void TheKeysFollowTheWritersNaming()
        {
            Assert.AreEqual("chain_riddle_trail.s08.out_heir", Outcomes.Stem("chain_riddle_trail.s08.opt_heir"));
            Assert.AreEqual("rhineland_flanders.colonia.hulls.out.carry", Outcomes.Stem("rhineland_flanders.colonia.hulls.o.carry"));
            Assert.IsNull(Outcomes.Stem("tollman.declare"));
            Assert.IsNull(Outcomes.Stem(""));
            var o = new OptionDef { key = "ask", labelKey = "x.s03.opt_ask" };
            CollectionAssert.AreEqual(new[] { "x.s03.opt_ask.done", "x.s03.out_ask" }, Outcomes.Keys(o, "").ToArray());
            CollectionAssert.AreEqual(new[] { "x.s03.opt_ask.success", "x.s03.out_ask_success", "x.s03.out_ask_ok", "x.s03.opt_ask.done", "x.s03.out_ask" },
                                      Outcomes.Keys(o, "success").ToArray());
            CollectionAssert.Contains(Outcomes.Keys(o, "failure").ToArray(), "x.s03.out_ask_fail");
            var named = new OptionDef { key = "watch", labelKey = "x.opt_watch", outSuccess = "x.out_for", outFailure = "x.out_against", outKey = "x.out_own" };
            Assert.AreEqual("x.out_for", Outcomes.Keys(named, "success").First());
            Assert.AreEqual("x.out_against", Outcomes.Keys(named, "failure").First());
            Assert.AreEqual("x.out_own", Outcomes.Keys(named, "").First());
        }

        [Test]
        public void MostOfTheScriptsEndsAreReached()
        {
            var book = EncounterData.Book();
            int plain = 0, won = 0, lost = 0, choices = 0;
            foreach (var c in book.cards)
                foreach (var o in c.options)
                {
                    choices++;
                    if (Outcomes.Text(book, o, "").Length > 0)
                        plain++;
                    if (o.Chance == null)
                        continue;
                    string good = Outcomes.Text(book, o, "success"), bad = Outcomes.Text(book, o, "failure");
                    if (good.Length > 0)
                        won++;
                    if (bad.Length > 0)
                        lost++;
                }
            UnityEngine.Debug.Log($"The written ends: {plain} of {choices} choices have one; of the chances {won} have one when won, {lost} when lost.");
            Assert.GreaterOrEqual(plain, 1900, "the plain ends of the script are no longer found by their keys");
            Assert.GreaterOrEqual(won, 220, "the ends of chances won are no longer found");
            Assert.GreaterOrEqual(lost, 220, "the ends of chances lost are no longer found");
            // the six the writers named otherwise
            var lots = book.Card("harbour.chain_mission_north.s12_lots").Option("watch");
            Assert.AreEqual(book.Written("chain_mission_north.s12.out_for"), Outcomes.Text(book, lots, "success"));
            Assert.AreEqual(book.Written("chain_mission_north.s12.out_against"), Outcomes.Text(book, lots, "failure"));
            var floor = book.Card("harbour.chain_silver_road.s08_under_floor").Option("fee");
            Assert.AreEqual(book.Written("chain_silver_road.s08.out_own"), Outcomes.Text(book, floor, ""));
        }

        [Test]
        public void AChoiceAnsweredTellsItsEnd()
        {
            var book = EncounterData.Book();
            var npcs = new NpcRegistry(793, book.roles, book.names);
            // a plain choice of a sea card: the end is the paragraph under "<label key>.done"
            var calm = book.Card("sea.calm.falls");
            Assert.IsNotNull(calm, "the card of the falling calm is gone");
            var s = EncounterSession.Open(calm, World(book, npcs, "", 0.5));
            int pick = s.options.FindIndex(x => x.def.key == "row");
            Assert.GreaterOrEqual(pick, 0, "the falling calm has lost its choice to row");
            Assert.IsTrue(s.options[pick].enabled, "rowing out of the calm cannot be chosen");
            string end = Outcomes.Text(book, s.options[pick].def, "");
            var r = s.Choose(pick);
            Assert.IsTrue(r.ok);
            Assert.AreEqual(end, r.written, "the choice's result does not carry its written end");
            CollectionAssert.DoesNotContain(r.facts, "Nothing more came of it.", "a choice with a written end says nothing came of it");
            // the window: the choice told back small, the end as a line that is told, the facts alone in the box
            var spec = FrameRoot.CardSpec(null, s);
            StringAssert.StartsWith("\u203a ", spec.chose);
            Assert.AreEqual(spec.chose, spec.lines[0].text);
            Assert.AreEqual(end, spec.lines[1].text);
            Assert.IsTrue(spec.ending.Contains(end));
            Assert.IsTrue(spec.plain.Contains(end), "the end is told, not said");
            Assert.AreEqual(r.Facts, spec.result);
            // a choice with no written end: what you did and the facts, as before
            var toll = book.Card("harbour.tollman.frankish");
            var t = EncounterSession.Open(toll, World(book, npcs, "dorestad", 0.5));
            var declared = t.Choose(0);
            Assert.AreEqual("", declared.written);
            Assert.AreEqual(declared.Text, FrameRoot.CardSpec(null, t).result);
        }

        [Test]
        public void AChanceTellsTheEndOfTheBranchThatFell()
        {
            var book = EncounterData.Book();
            EncounterDef card = null;
            OptionDef option = null;
            foreach (var c in book.cards)
                foreach (var o in c.options)
                    if (card == null && o.Chance != null && o.cost.Count == 0
                        && Outcomes.Text(book, o, "success").Length > 0 && Outcomes.Text(book, o, "failure").Length > 0
                        && Outcomes.Text(book, o, "success") != Outcomes.Text(book, o, "failure"))
                    {
                        card = c;
                        option = o;
                    }
            Assert.IsNotNull(card, "no chance in the book has both its ends written");
            foreach (bool win in new[] { true, false })
            {
                var npcs = new NpcRegistry(793, book.roles, book.names);
                var s = EncounterSession.Open(card, World(book, npcs, "dorestad", win ? 0.0 : 0.999));
                int at = s.options.FindIndex(x => x.def == option);
                Assert.GreaterOrEqual(at, 0);
                if (!s.options[at].enabled)
                    Assert.Ignore("the chance found (" + card.id + " " + option.key + ") cannot be chosen in the test's world");
                var r = s.Choose(at);
                Assert.AreEqual(win ? "success" : "failure", r.branch);
                Assert.AreEqual(Outcomes.Text(book, option, r.branch), r.written, card.id + " " + option.key);
            }
        }
    }
}
