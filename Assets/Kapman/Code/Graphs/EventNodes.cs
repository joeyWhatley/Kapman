using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>An event is to be shown (Data/events_793.json): its key (for your writing), title and intention.</summary>
    [UnitTitle("On event")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnGameEvent : GlobalEventUnit<EventDef>
    {
        public const string Hook = "Kapman.GameEvent";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput id { get; private set; }
        [DoNotSerialize] public ValueOutput key { get; private set; }
        [DoNotSerialize] public ValueOutput title { get; private set; }
        [DoNotSerialize] public ValueOutput intention { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            id = ValueOutput<string>(nameof(id));
            key = ValueOutput<string>(nameof(key));
            title = ValueOutput<string>(nameof(title));
            intention = ValueOutput<string>(nameof(intention));
        }

        protected override void AssignArguments(Flow flow, EventDef e)
        {
            flow.SetValue(id, e.id);
            flow.SetValue(key, e.Key);
            flow.SetValue(title, e.title);
            flow.SetValue(intention, e.intention);
        }
    }

    /// <summary>Adds the event's options to the open panel (each with its key, "event.id.option", for your writing).</summary>
    [UnitTitle("Event: Add its options")]
    [UnitCategory("Kapman/Events")]
    public sealed class EventAddOptions : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var e = GameEvents.Current;
                if (e != null && TextPanel.Current != null)
                    foreach (var o in e.options)
                        TextPanel.Current.AddOption(e.Key + "." + o.key, o.label);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The option chosen: its effects done (health, heart, men, silver, goods, water, food, deeds); gives the
    /// outcome as facts.</summary>
    [UnitTitle("Event: Choose")]
    [UnitCategory("Kapman/Events")]
    public sealed class EventChoose : Unit
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
                text = GameEvents.Choose(flow.GetValue<int>(choice));
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

    static class GameEventsBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            GameEvents.Due -= Due;
            GameEvents.Due += Due;
        }

        static void Due(EventDef e) => EventBus.Trigger(OnGameEvent.Hook, e);
    }
}
