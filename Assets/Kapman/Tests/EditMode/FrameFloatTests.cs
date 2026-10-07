using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// A card as a floating window, without the scene (7 Oct 2026): the toll man's card made into what the frame shows
    /// (FrameRoot.CardSpec): who speaks, the words, every choice with its odds as a word or the rule that forbids it; it
    /// must be answered and holds the clock; a choice made through it runs the card; after it, what came of it and one
    /// way on.
    /// </summary>
    public class FrameFloatTests
    {
        const string Toll = "harbour.tollman.frankish";

        static EncounterDirector AtTheToll()
        {
            var d = new EncounterDirector(EncounterData.BookOf(Toll), 793, new FakeGame());
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.vars.nums["port.toll"] = 0.1;
            d.vars.nums["hold.value"] = 300.0;
            d.vars.nums["hold.weapons"] = 2.0;                              // (swords aboard: hiding them is a choice, with odds)
            d.vars.nums["crew.trade_best"] = 3.0;
            d.vars.numSource = name => name == "silver" ? 100.0 : 0.0;
            d.SetTime(10.0, 793, 6, 240);
            d.BeginCall("dorestad");
            return d;
        }

        [Test]
        public void ACardIsAFloatingWindowThatMustBeAnswered()
        {
            var d = AtTheToll();
            var s = d.OpenNext();
            Assert.IsNotNull(s);
            var spec = FrameRoot.CardSpec(d, s);
            Assert.AreEqual("card", spec.id);
            Assert.AreEqual(s.Title, spec.title);
            Assert.IsTrue(spec.mustAnswer, "a card must be answered");
            Assert.IsTrue(spec.holdsClock, "and holds the clock while it waits");
            Assert.IsNotEmpty(spec.who, "the speaker's line");
            Assert.IsNotEmpty(spec.speaker, "the speaker, for the portrait's place");
            Assert.IsNotNull(spec.face, "a face place");
            Assert.Greater(spec.lines.Count, 0, "the words");
            Assert.AreEqual(s.options.Count, spec.options.Count, "every choice");
            var hide = spec.options[s.card.Option("hide").index];
            Assert.IsTrue(hide.enabled);
            Assert.IsNotEmpty(hide.odds, "a chance shows its odds as a word");
            Assert.IsFalse(double.TryParse(hide.odds, out _), "a word, not a number: " + hide.odds);
            var charter = spec.options[s.card.Option("charter").index];
            Assert.IsFalse(charter.enabled, "no charter in the chest");
            Assert.IsNotEmpty(charter.rule, "a choice that cannot be made says why");
            Assert.IsEmpty(spec.options[s.card.Option("declare").index].odds, "a sure thing shows no odds");
        }

        [Test]
        public void AChoiceMadeThroughItRunsTheCardAndLeavesOneWayOn()
        {
            var d = AtTheToll();
            var s = d.OpenNext();
            var spec = FrameRoot.CardSpec(d, s);
            spec.options[s.card.Option("declare").index].pick();
            Assert.IsTrue(s.Chosen, "the choice ran the card");
            var after = FrameRoot.CardSpec(d, s);
            Assert.IsFalse(after.mustAnswer, "answered: it may be shut");
            Assert.IsFalse(after.holdsClock, "and it holds the clock no longer");
            Assert.IsNotEmpty(after.result, "what came of it");
            Assert.AreEqual(1, after.options.Count);
            Assert.IsTrue(after.options[0].isDefault, "Enter goes on");
            Assert.IsNotNull(after.shut, "and so does shutting it");
            after.options[0].pick();
            Assert.IsNull(d.Current, "the card has closed");
        }

        /// <summary>The ruling of 7 Oct: only a card stops the clock, "but you can override it immediately if you want".</summary>
        [Test]
        public void OnlyACardStopsTheClockAndThePlayerMaySetItRunning()
        {
            bool card = false, letGo = false;
            bool wasOnly = PanelClock.OnlyCards;
            var wasHolds = PanelClock.Holds;
            var wasLetGo = PanelClock.LetGo;
            int wasLevel = GameSpeed.Level;
            try
            {
                PanelClock.OnlyCards = true;
                PanelClock.Holds = () => card && !letGo;
                PanelClock.LetGo = () => letGo = true;
                if (GameSpeed.PlayerPaused)
                    GameSpeed.TogglePause();
                PanelClock.Tick();
                Assert.IsFalse(PanelClock.Holding, "no card: nothing holds the clock, whatever windows are up");
                card = true;
                PanelClock.Tick();
                Assert.IsTrue(PanelClock.Holding, "a card that needs an answer holds the clock");
                Assert.IsTrue(GameSpeed.Paused);
                GameSpeed.TogglePause();                                   // Space
                Assert.IsTrue(letGo, "going on lets the card's hold go");
                Assert.IsFalse(PanelClock.Holding);
                Assert.IsFalse(GameSpeed.PlayerPaused, "going on under a card is not a pause");
                Assert.IsFalse(GameSpeed.Paused, "the clock runs, the card still open");
                PanelClock.Tick();
                Assert.IsFalse(PanelClock.Holding, "and it stays let go");
                // a new card holds again; a speed button sets it running too, even the speed already chosen
                letGo = false;
                PanelClock.Tick();
                Assert.IsTrue(PanelClock.Holding);
                GameSpeed.SetLevel(GameSpeed.Level);
                Assert.IsTrue(letGo);
                Assert.IsFalse(GameSpeed.Paused);
                // with nothing held, Space is the player's own pause as ever
                card = false;
                PanelClock.Tick();
                GameSpeed.TogglePause();
                Assert.IsTrue(GameSpeed.PlayerPaused);
                GameSpeed.TogglePause();
                Assert.IsFalse(GameSpeed.PlayerPaused);
            }
            finally
            {
                PanelClock.OnlyCards = wasOnly;
                PanelClock.Holds = wasHolds;
                PanelClock.LetGo = wasLetGo;
                GameSpeed.Release(PanelClock.Why);
                if (GameSpeed.PlayerPaused)
                    GameSpeed.TogglePause();
                GameSpeed.SetLevel(wasLevel);
            }
        }
    }
}
