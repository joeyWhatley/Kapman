using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// WHAT THE SEA TELLS HER IS A FLOW (the user's ruling of 7 Oct 2026: flows are graphs). Wind and tide have done
    /// something to her that she should know of: the graph "Voyage - the sea moves her" (on the '--- GAME ---' object)
    /// starts here. WHAT: "off_line" (set well off her line), "lee_shore" (held off a shore she cannot keep clear of),
    /// "adrift" (lying to in water too deep to anchor in), "lost" (the steersman has lost his reckoning, and she lies
    /// to), "rough" (she comes into rough water), "relaid" (along a coast the steersman has laid her course again
    /// himself), "cleared" (a fog has cleared and she sails on). THE WORDS the game has for it (placeholders, the
    /// user's to write).
    /// Whether THE CLOCK DROPS BACK for it, and WHY (a reason the player can switch off in the settings).
    /// </summary>
    [UnitTitle("On the sea moves her")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheSeaMovesHer : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.Sea.Moment";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for the sea's moments now (with none, the code tells the log and
        /// drops the clock itself).</summary>
        public static int Listening { get; private set; }

        [DoNotSerialize, PortLabel("What")] public ValueOutput what { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueOutput words { get; private set; }
        [DoNotSerialize, PortLabel("The clock drops back")] public ValueOutput drops { get; private set; }
        [DoNotSerialize, PortLabel("Why")] public ValueOutput why { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            what = ValueOutput<string>(nameof(what), flow => SeaMoments.What);
            words = ValueOutput<string>(nameof(words), flow => SeaMoments.Words);
            drops = ValueOutput<bool>(nameof(drops), flow => SeaMoments.Drops);
            why = ValueOutput<ClockReason>(nameof(why), flow => SeaMoments.Why);
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
            SeaMoments.Flow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>Runs the flow of the sea's moment straight from a graph asset, with no scene and no machine: for a test.</summary>
    public static class SeaFlow
    {
        static ScriptGraphAsset held;
        static GraphReference reference;

        public static bool Run(ScriptGraphAsset asset)
        {
            if (asset == null || asset.graph == null)
                return false;
            OnTheSeaMovesHer start = null;
            foreach (var unit in asset.graph.units)
                if (unit is OnTheSeaMovesHer on)
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

    /// <summary>The log says a plain fact: a line of the notices, kept in the log's window. With no words, nothing.</summary>
    [UnitTitle("Log: say")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class LogSay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueInput words { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                SeaMoments.Told(flow.GetValue<string>(words));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            words = ValueInput(nameof(words), "");
            Requirement(words, enter);
            Succession(enter, exit);
        }
    }

    /// <summary>The clock drops back by itself: to a walk for what is happening now, to the second speed for what is
    /// coming. WHY is a reason the player can switch off in the settings (then nothing drops), and what the top bar
    /// says after the drop.</summary>
    [UnitTitle("Clock: drop back")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class ClockDropBack : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("Why")] public ValueInput why { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                SeaMoments.ClockDropsBack(flow.GetValue<ClockReason>(why));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            why = ValueInput(nameof(why), ClockReason.Pressing);
            Requirement(why, enter);
            Succession(enter, exit);
        }
    }
}
