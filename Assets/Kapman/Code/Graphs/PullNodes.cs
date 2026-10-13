using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// THE COURSE PULLED ON THE CHART IS A FLOW (the user, 13 Oct 2026: drag and correct the course with the mouse; the
    /// rule of 7 Oct 2026: flows are graphs). A knot of her course has been let go, pulled again or taken off: the graph
    /// "Chart - the course is pulled" (on the '--- GAME ---' object) starts here. WHAT: "pulled" (a new knot), "moved"
    /// (a knot pulled again), "off" (a knot taken off). ON LAND: the point let go lies on dry land or the sands.
    /// The parts that follow are the nodes below: the knot to the water, her course laid again through her knots, the
    /// log told.
    /// </summary>
    [UnitTitle("On the course is pulled")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheCourseIsPulled : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.Chart.Pulled";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for the course pulled now (with none, the code runs the parts).</summary>
        public static int Listening { get; private set; }

        [DoNotSerialize, PortLabel("What")] public ValueOutput what { get; private set; }
        [DoNotSerialize, PortLabel("On land")] public ValueOutput onLand { get; private set; }
        [DoNotSerialize, PortLabel("Which knot")] public ValueOutput knot { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            what = ValueOutput<string>(nameof(what), flow => CoursePulls.What);
            onLand = ValueOutput<bool>(nameof(onLand), flow => CoursePulls.OnLand);
            knot = ValueOutput<int>(nameof(knot), flow => CoursePulls.Knot);
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
            CoursePulls.Flow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>Runs the flow of the course pulled straight from a graph asset, with no scene and no machine: for a test.</summary>
    public static class PullFlow
    {
        static ScriptGraphAsset held;
        static GraphReference reference;

        public static bool Run(ScriptGraphAsset asset)
        {
            if (asset == null || asset.graph == null)
                return false;
            OnTheCourseIsPulled start = null;
            foreach (var unit in asset.graph.units)
                if (unit is OnTheCourseIsPulled on)
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

    /// <summary>The point let go lay on land: the knot goes to the water nearest it (within Course.ToWaterM).</summary>
    [UnitTitle("Course: take the knot to the water")]
    [UnitCategory("Kapman/Chart")]
    public sealed class KnotToWater : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                CoursePulls.TakeToWater();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>Her course is laid again from where she is, through her knots with this one let go, moved or taken off
    /// among them, and on to the same end. LAID: it was (false: no water she can sail leads through that point, and her
    /// course stands as it was). THE WORDS for the log, either way. KM of the course laid.</summary>
    [UnitTitle("Course: lay her course again through her knots")]
    [UnitCategory("Kapman/Chart")]
    public sealed class LayThroughKnots : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("Laid")] public ValueOutput laid { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueOutput words { get; private set; }
        [DoNotSerialize, PortLabel("Km")] public ValueOutput km { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                CoursePulls.LayThroughKnots();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            laid = ValueOutput<bool>(nameof(laid), flow => CoursePulls.Laid);
            words = ValueOutput<string>(nameof(words), flow => CoursePulls.Words);
            km = ValueOutput<double>(nameof(km), flow => CoursePulls.Km);
            Succession(enter, exit);
        }
    }

    /// <summary>The log is told of the course: the moment's own words go in their short form, with the whole of them
    /// in the line's box; any other words as they are. With no words, nothing.</summary>
    [UnitTitle("Log: say of the course")]
    [UnitCategory("Kapman/Chart")]
    public sealed class CourseSay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize, PortLabel("The words")] public ValueInput words { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                CoursePulls.Told(flow.GetValue<string>(words));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            words = ValueInput(nameof(words), "");
            Requirement(words, enter);
            Succession(enter, exit);
        }
    }
}
