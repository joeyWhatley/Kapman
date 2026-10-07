using System.Collections;
using Kapman.Game;
using Unity.VisualScripting;

namespace Kapman.Graphs
{
    /// <summary>Opens the text panel on a place or a happening. The key finds your writing for it (Data/writing.json).</summary>
    [UnitTitle("Text: Show")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextShow : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput key { get; private set; }
        [DoNotSerialize] public ValueInput title { get; private set; }
        [DoNotSerialize] public ValueInput intention { get; private set; }
        /// <summary>Blur the scene behind the panel (off for a meeting at sea: the ships are what it's about).</summary>
        [DoNotSerialize] public ValueInput frost { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            key = ValueInput(nameof(key), "");
            title = ValueInput(nameof(title), "");
            intention = ValueInput(nameof(intention), "");
            frost = ValueInput(nameof(frost), true);
            Requirement(key, enter);
            Requirement(title, enter);
            Requirement(intention, enter);
            Requirement(frost, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            TextPanel.Current?.Show(flow.GetValue<string>(key), flow.GetValue<string>(title), flow.GetValue<string>(intention),
                                    flow.GetValue<bool>(frost));
            return exit;
        }
    }

    /// <summary>Adds an option to the open panel: its key (for your writing) and what choosing it means.</summary>
    [UnitTitle("Text: Add option")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextAddOption : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput key { get; private set; }
        [DoNotSerialize] public ValueInput intention { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            key = ValueInput(nameof(key), "");
            intention = ValueInput(nameof(intention), "");
            Requirement(key, enter);
            Requirement(intention, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            TextPanel.Current?.AddOption(flow.GetValue<string>(key), flow.GetValue<string>(intention));
            return exit;
        }
    }

    /// <summary>Waits for the player to pick an option (by mouse or number key); gives its number, from 0.</summary>
    [UnitTitle("Text: Wait for choice")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextWaitForChoice : WaitUnit
    {
        [DoNotSerialize] public ValueOutput choice { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            choice = ValueOutput<int>(nameof(choice));
            Assignment(enter, choice);
        }

        protected override IEnumerator Await(Flow flow)
        {
            // (a flow whose wait is given up ends here, without going on: TextPanel.AbandonWaits)
            int epoch = TextPanel.Abandoned;
            TextPanel.Waiters++;
            while ((TextPanel.Current == null || TextPanel.Current.Choice < 0) && TextPanel.Abandoned == epoch)
                yield return null;
            if (TextPanel.Abandoned != epoch)
                yield break;
            TextPanel.Waiters--;
            flow.SetValue(choice, TextPanel.Current.Choice);
            yield return exit;
        }
    }

    /// <summary>Waits for the player to pick an option, or for so many real seconds - then takes the default (e.g. a
    /// ship met at sea: hold your course). Gives the option's number, from 0.</summary>
    [UnitTitle("Text: Wait for choice (or time out)")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextWaitForChoiceTimed : WaitUnit
    {
        [DoNotSerialize] public ValueInput seconds { get; private set; }
        [DoNotSerialize] public ValueInput fallback { get; private set; }
        [DoNotSerialize] public ValueOutput choice { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            seconds = ValueInput(nameof(seconds), 60f);
            fallback = ValueInput(nameof(fallback), 0);
            choice = ValueOutput<int>(nameof(choice));
            Requirement(seconds, enter);
            Requirement(fallback, enter);
            Assignment(enter, choice);
        }

        protected override IEnumerator Await(Flow flow)
        {
            // 7 Oct 2026: on the chart the seconds are the game's own (TextPanel.WaitsByGameClock): none pass while the
            // clock is held, so a talk that holds the clock waits for its answer; with the clock let run, sixty seconds
            // is an hour of the game. Elsewhere they are real seconds, as before.
            float left = flow.GetValue<float>(seconds);
            int epoch = TextPanel.Abandoned;
            TextPanel.Waiters++;
            while ((TextPanel.Current == null || TextPanel.Current.Choice < 0) && left > 0f && TextPanel.Abandoned == epoch)
            {
                yield return null;
                // (the stepped clock: a "second" of this wait is a game minute, and the frame has run so many of them)
                left -= !TextPanel.WaitsByGameClock ? UnityEngine.Time.unscaledDeltaTime
                      : GameSpeed.Stepped ? KapmanGame.StepsLastFrame : UnityEngine.Time.deltaTime;
            }
            if (TextPanel.Abandoned != epoch)
                yield break;
            TextPanel.Waiters--;
            flow.SetValue(choice, TextPanel.Current != null && TextPanel.Current.Choice >= 0 ? TextPanel.Current.Choice : flow.GetValue<int>(fallback));
            yield return exit;
        }
    }

    /// <summary>Shows what came of the choice, as plain facts (your writing for the key above them, if any).</summary>
    [UnitTitle("Text: Show outcome")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextShowOutcome : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput key { get; private set; }
        [DoNotSerialize] public ValueInput outcome { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            key = ValueInput(nameof(key), "");
            outcome = ValueInput(nameof(outcome), "");
            Requirement(key, enter);
            Requirement(outcome, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            TextPanel.Current?.ShowOutcome(flow.GetValue<string>(key), flow.GetValue<string>(outcome));
            return exit;
        }
    }

    /// <summary>Closes the text panel.</summary>
    [UnitTitle("Text: Hide")]
    [UnitCategory("Kapman/Text")]
    public sealed class TextHide : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                TextPanel.Current?.Hide();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }
}
