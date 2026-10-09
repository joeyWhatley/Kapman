using System.Collections;
using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// A SCENE TOLD ON THE PURPLE IS A FLOW (the user's ruling of 7 Oct 2026: flows are graphs; 9 Oct, of that day's
    /// work: "Btw is this all being built like I asked"). The lender's men have killed Ravn and their card is shut: the
    /// graph "Scene - Ravn is killed" starts here and runs the scene part by part, line by line (the "Scene: ..." nodes
    /// below, each a part of Code/Game/UI/Showtime.cs), and ends with what follows his death. Put a wait, a sound or a
    /// line of your own between two of them; move the hit; leave a line out.
    /// </summary>
    [UnitTitle("On Ravn is killed")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnRavnIsKilled : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.RavnIsKilled";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for his death now (with none, the scene is run from code as the
        /// table stages it).</summary>
        public static int Listening { get; private set; }

        /// <summary>He died at sea (false: at a quay).</summary>
        [DoNotSerialize, PortLabel("At sea")] public ValueOutput atSea { get; private set; }
        /// <summary>Where and when, as it stands under his name ("Killed at Dorestat, 1 May 793").</summary>
        [DoNotSerialize, PortLabel("Where and when")] public ValueOutput plate { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            atSea = ValueOutput<bool>(nameof(atSea), flow => Feud.AtSea);
            plate = ValueOutput<string>(nameof(plate), flow => Feud.Plate());
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
            Feud.SceneFlow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>A part of a scene told on the purple: it is run, and the flow goes on when it is over.</summary>
    public abstract class ScenePart : WaitUnit
    {
        /// <summary>The part of the show to run (null: nothing to wait for).</summary>
        protected abstract IEnumerator Part(Showtime show, Flow flow);

        protected override IEnumerator Await(Flow flow)
        {
            var show = Showtime.Current;
            var part = show != null ? Part(show, flow) : null;
            if (part != null)
                yield return part;
            yield return exit;
        }
    }

    /// <summary>The game is taken in hand (the clock held, the keys the show's, the music dying away) and the purple of
    /// the opening comes up over the whole screen. "Scene: the purple lifts" gives the game back.</summary>
    [UnitTitle("Scene: the purple comes up")]
    [UnitCategory("Kapman/Scene")]
    public sealed class SceneThePurpleComesUp : ScenePart
    {
        protected override IEnumerator Part(Showtime show, Flow flow) => show.SceneOpens();
    }

    /// <summary>Somebody says a line, in the one box of talk at the foot of the screen, a sentence or two to a box, and
    /// the key brings the next. WHO: one of the scene's cast (feud_rules_793.json, "sceneCast": brute, man, ravn,
    /// aldger), or nothing for the teller, who has no face. KEY: the line's words in the writing (Game/Data/writing;
    /// at sea the words of the key with ".sea" after it stand in, where they are written). BREAKS OFF: the line is
    /// said to its last letter and nobody is waited for. A key with no words written says nothing.</summary>
    [UnitTitle("Scene: say a line")]
    [UnitCategory("Kapman/Scene")]
    public sealed class SceneSayALine : ScenePart
    {
        [DoNotSerialize, PortLabel("Who (nothing: the teller)")] public ValueInput who { get; private set; }
        [DoNotSerialize, PortLabel("The words' key")] public ValueInput key { get; private set; }
        [DoNotSerialize, PortLabel("Breaks off")] public ValueInput breaksOff { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            who = ValueInput(nameof(who), "");
            key = ValueInput(nameof(key), "");
            breaksOff = ValueInput(nameof(breaksOff), false);
            Requirement(who, enter);
            Requirement(key, enter);
            Requirement(breaksOff, enter);
        }

        protected override IEnumerator Part(Showtime show, Flow flow)
        {
            var line = Feud.SceneLine(flow.GetValue<string>(who), flow.GetValue<string>(key), flow.GetValue<bool>(breaksOff), false);
            return line != null ? show.SceneSays(line) : null;
        }
    }

    /// <summary>The screen is struck: a blood-red filter flashes over the whole screen and the picture is shaken, in hard
    /// steps as an old console did it (two flashes and no more). Nothing is waited for: the line that follows comes
    /// up under it, whole at once.</summary>
    [UnitTitle("Scene: the screen is struck")]
    [UnitCategory("Kapman/Scene")]
    public sealed class SceneTheScreenIsStruck : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                if (Showtime.Current != null)
                    Showtime.Current.SceneStrikes();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The box of talk goes, and a name and what stands under it come up on the purple and stand.</summary>
    [UnitTitle("Scene: the name's card")]
    [UnitCategory("Kapman/Scene")]
    public sealed class SceneTheNamesCard : ScenePart
    {
        [DoNotSerialize, PortLabel("The name")] public ValueInput big { get; private set; }
        [DoNotSerialize, PortLabel("Under it")] public ValueInput small { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            big = ValueInput(nameof(big), "");
            small = ValueInput(nameof(small), "");
            Requirement(big, enter);
            Requirement(small, enter);
        }

        protected override IEnumerator Part(Showtime show, Flow flow) => show.SceneCard(flow.GetValue<string>(big), flow.GetValue<string>(small));
    }

    /// <summary>The purple lifts on the game (a name's card that is up stands a moment longer, and goes), and the game is
    /// given back: the clock runs, the keys are the game's.</summary>
    [UnitTitle("Scene: the purple lifts")]
    [UnitCategory("Kapman/Scene")]
    public sealed class SceneThePurpleLifts : ScenePart
    {
        protected override IEnumerator Part(Showtime show, Flow flow) => show.SceneCloses();
    }

    /// <summary>What follows Ravn's scene: the log keeps that he is dead, the mark that lies on her from now is said, and
    /// she stands over him to say what is to be done (the card crew.feud.oath).</summary>
    [UnitTitle("Ravn: what follows his death")]
    [UnitCategory("Kapman/Scene")]
    public sealed class RavnWhatFollowsHisDeath : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                Feud.AfterScene();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }
}
