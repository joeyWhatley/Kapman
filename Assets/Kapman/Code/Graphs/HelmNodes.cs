using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// YOUR HAND ON THE HELM IS A FLOW (the user, 13 Oct 2026, by the picker: "Steer her yourself in 3D"; the rule of
    /// 7 Oct 2026: flows are graphs). The graph "Voyage - the helm is yours" (on the '--- GAME ---' object) starts
    /// here. WHAT: "taken" (your hand on the helm), "let_go" (the helm amidships: she holds her heading), "given_back"
    /// (the helm the steersman's again), "aground" (run aground by your hand). GIVEN BACK: the moment is that one.
    /// THE WORDS the game has for it (placeholders, the user's to write).
    /// The parts that follow are the nodes below: her course laid again from where she is, the log told.
    /// </summary>
    [UnitTitle("On the helm")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheHelm : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.Helm.Moment";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for the helm's moments now (with none, the code runs the parts).</summary>
        public static int Listening { get; private set; }

        [DoNotSerialize, PortLabel("What")] public ValueOutput what { get; private set; }
        [DoNotSerialize, PortLabel("Given back")] public ValueOutput givenBack { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueOutput words { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            what = ValueOutput<string>(nameof(what), flow => HandHelm.What);
            givenBack = ValueOutput<bool>(nameof(givenBack), flow => HandHelm.GivenBack);
            words = ValueOutput<string>(nameof(words), flow => HandHelm.Words);
        }

        public override void StartListening(GraphStack stack)
        {
            bool was = stack.GetElementData<Data>(this).isListening;
            base.StartListening(stack);
            if (!was && stack.GetElementData<Data>(this).isListening)
                Listening++;
        }

        public override void StopListening(GraphStack stack)
        {
            bool was = stack.GetElementData<Data>(this).isListening;
            base.StopListening(stack);
            if (was && !stack.GetElementData<Data>(this).isListening)
                Listening = Mathf.Max(0, Listening - 1);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bridge()
        {
            Listening = 0;
            HandHelm.Flow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>Runs the flow of the helm's moment straight from a graph asset, with no scene and no machine: for a test.</summary>
    public static class HelmFlow
    {
        static ScriptGraphAsset held;
        static GraphReference reference;

        public static bool Run(ScriptGraphAsset asset)
        {
            if (asset == null || asset.graph == null)
                return false;
            OnTheHelm start = null;
            foreach (var unit in asset.graph.units)
                if (unit is OnTheHelm on)
                    start = on;
            if (start == null)
                return false;
            if (held != asset || reference == null)
            {
                held = asset;
                reference = GraphReference.New(asset, true);
            }
            Flow.New(reference).Run(start.trigger);
            return true;
        }
    }

    /// <summary>The helm given back: the steersman lays her course again from where she is, through her knots and on to
    /// the same end, where the rule says he does (the rule graph "does the steersman lay her course again when the helm
    /// is given back") and the water lets him; else she lies to. LAID: it was. THE WORDS for the log, either way.</summary>
    [UnitTitle("Helm: lay her course again from here")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class HelmLayAgain : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("Laid")] public ValueOutput laid { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueOutput words { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                HandHelm.LayAgainFromHere();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            laid = ValueOutput<bool>(nameof(laid), flow => HandHelm.Laid);
            words = ValueOutput<string>(nameof(words), flow => HandHelm.Words);
            Succession(enter, exit);
        }
    }

    /// <summary>The log is told of the helm: the moment's own words go in their short form, with the whole of them in
    /// the line's box; any other words as they are. With no words, nothing.</summary>
    [UnitTitle("Log: say of the helm")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class HelmSay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueInput words { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                HandHelm.Told(flow.GetValue<string>(words));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            words = ValueInput(nameof(words), "");
            Requirement(words, enter);
            Succession(enter, exit);
        }
    }
}
