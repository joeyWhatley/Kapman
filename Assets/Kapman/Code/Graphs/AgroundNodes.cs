using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>She's stuck fast and the choice is to be shown (Code/Game/Voyage/Aground.cs): how she lies, as facts.</summary>
    [UnitTitle("On aground")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnAground : GlobalEventUnit<string>
    {
        public const string Hook = "Kapman.Aground";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput facts { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            facts = ValueOutput<string>(nameof(facts));
        }

        protected override void AssignArguments(Flow flow, string f) => flow.SetValue(facts, f);
    }

    /// <summary>Adds the ways of getting her off that there are here to the open panel (each with its key for your
    /// writing: "aground.tide", "aground.row", "aground.push", "aground.leave"). Waiting for the tide is there only
    /// where a tide will float her soon enough.</summary>
    [UnitTitle("Aground: Add the options")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class AgroundAddOptions : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                if (TextPanel.Current != null)
                    foreach (var (key, intention) in Aground.Options())
                        TextPanel.Current.AddOption(key, intention);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The option chosen (its number from 0): it's begun - the wait for the tide, the rowing, the pushing - or
    /// she's left as she lies for now. Gives what's happening, as facts.</summary>
    [UnitTitle("Aground: Choose")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class AgroundChoose : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput choice { get; private set; }
        [DoNotSerialize] public ValueOutput outcome { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = Aground.Choose(flow.GetValue<int>(choice));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            choice = ValueInput(nameof(choice), 0);
            outcome = ValueOutput<string>(nameof(outcome), f => text);
            Requirement(choice, enter);
            Succession(enter, exit);
            Assignment(enter, outcome);
        }
    }

    static class AgroundEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Aground.Due -= Due;
            Aground.Due += Due;
        }

        static void Due(string facts) => EventBus.Trigger(OnAground.Hook, facts);
    }
}
