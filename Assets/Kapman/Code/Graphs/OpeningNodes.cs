using System.Collections;
using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// THE OPENING'S FLOW (KAPMAN, step 3: flows as graphs). The opening begins (a new game from the title or from the
    /// menu): the graph "Opening - the lender, the boat, the first morning" starts here and runs the parts of the show
    /// one after another (the "Opening: ..." nodes below, each a part of Code/Game/UI/Showtime.cs). Put a wait, a sound
    /// or a part of your own between two of them; leave one out to skip it.
    /// </summary>
    [UnitTitle("On the opening begins")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnOpening : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.Opening";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for the opening now (with none, the show runs its parts from code).</summary>
        public static int Listening { get; private set; }

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
            Showtime.OpeningFlow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>A part of the opening: it is run, and the flow goes on when it is over.</summary>
    public abstract class OpeningPart : WaitUnit
    {
        /// <summary>The part of the show to run (null: it was done at once).</summary>
        protected abstract IEnumerator Part(Showtime show);

        protected override IEnumerator Await(Flow flow)
        {
            var show = Showtime.Current;
            var part = show != null ? Part(show) : null;
            if (part != null)
                yield return part;
            yield return exit;
        }
    }

    /// <summary>The purple, and the box: up from the bottom to the middle, then it opens. The game is taken in hand here
    /// (the clock held, the keys the show's) until "Opening: the first morning" gives it back.</summary>
    [UnitTitle("Opening: the box comes up")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningBoxComesUp : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningBox();
    }

    /// <summary>Somebody asks: the face, the land, the name, the faith, the past.</summary>
    [UnitTitle("Opening: who are you?")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningWhoAreYou : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningQuestions();
    }

    /// <summary>The game begins behind the purple, with what was answered: who the merchant is, the first jobs.</summary>
    [UnitTitle("Opening: the game begins as answered")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningGameBegins : OpeningPart
    {
        protected override IEnumerator Part(Showtime show)
        {
            show.OpeningBegin();
            return null;
        }
    }

    /// <summary>The lender steps out of the dark and says his lines.</summary>
    [UnitTitle("Opening: the lender speaks")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningLenderSpeaks : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningLender();
    }

    /// <summary>Through black to the boat turning in her showroom, her plate, what is owed; a click goes on.</summary>
    [UnitTitle("Opening: the showroom")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningTheShowroom : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningShowroom();
    }

    /// <summary>The game is given back (the clock runs, the windows answer); the card of the place and the day comes
    /// over the black; the next click is armed.</summary>
    [UnitTitle("Opening: the first morning")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningTheFirstMorning : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningFirstMorning();
    }

    /// <summary>Waits for the first click or key of the first morning.</summary>
    [UnitTitle("Opening: wait for the first click")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningWaitForTheFirstClick : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningFirstClick();
    }

    /// <summary>"!!!" pop up where the click fell, shake, and fly down to where Ravn's face is about to stand.</summary>
    [UnitTitle("Opening: the marks")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningTheMarks : OpeningPart
    {
        protected override IEnumerator Part(Showtime show) => show.OpeningMarks();
    }

    /// <summary>Ravn's first talk comes up in the foot (the flow goes on at once: he is not waited for).</summary>
    [UnitTitle("Opening: Ravn speaks")]
    [UnitCategory("Kapman/Opening")]
    public sealed class OpeningRavnSpeaks : OpeningPart
    {
        protected override IEnumerator Part(Showtime show)
        {
            show.RavnSpeaks();
            return null;
        }
    }
}
