using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// WHAT SHE DOES ABOUT RAVN'S DEATH IS A FLOW (the user's ruling of 7 Oct 2026: flows are graphs; 9 Oct, of that
    /// day's work: "Btw is this all being built like I asked"). She has answered one of the feud's cards (or sworn his
    /// blood from the Errands window, or said nothing until the days of that ran out): the graph "Feud - what she does
    /// about Ravn's death" (on the '--- GAME ---' object) starts here, and has a row of small parts for each answer
    /// (the "Feud: ..." nodes below, each a part of Code/Game/Voyage/Feud.cs). THE ANSWER: "swear", "law", "quiet"
    /// (what she says over him); "bury_priest", "pyre", "bury_water", "bury_aboard" (where he lies); "blood" (she has
    /// killed the one who did it), "blood_lost"; "lender" (she has killed the lender), "lender_lost"; "price" (the
    /// count sets his price on the lender); "outlaw_paid", "outlaw_seized"; "let_lie".
    /// </summary>
    [UnitTitle("On Ravn's death is answered")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnRavnsDeathIsAnswered : GlobalEventUnit<EmptyEventArgs>
    {
        public const string Hook = "Kapman.Feud.Answered";
        protected override string hookName => Hook;

        /// <summary>How many graphs of the scene listen for an answer now (with none, the code runs the same parts itself).</summary>
        public static int Listening { get; private set; }

        [DoNotSerialize, PortLabel("The answer")] public ValueOutput answer { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            answer = ValueOutput<string>(nameof(answer), flow => Feud.Answer);
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
            Feud.AnswerFlow = () =>
            {
                if (Listening <= 0)
                    return false;
                EventBus.Trigger(Hook);
                return true;
            };
        }
    }

    /// <summary>Runs the flow of an answer straight from a graph asset, with no scene and no machine: for a test that
    /// holds the graph to the code.</summary>
    public static class FeudFlow
    {
        static ScriptGraphAsset held;
        static GraphReference reference;

        public static bool Run(ScriptGraphAsset asset)
        {
            if (asset == null || asset.graph == null)
                return false;
            OnRavnsDeathIsAnswered start = null;
            foreach (var unit in asset.graph.units)
                if (unit is OnRavnsDeathIsAnswered on)
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

    /// <summary>A part of the feud's flow: it is done at once, and the flow goes on.</summary>
    public abstract class FeudPart : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                Do(flow);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
            Ports();
        }

        /// <summary>The part's own ports, if it has any.</summary>
        protected virtual void Ports()
        {
        }

        protected abstract void Do(Flow flow);
    }

    /// <summary>She swears his blood on the one who killed him: the crew's heart rises a little the first time (the
    /// table's swornHeart), and the matter is open. At Dorestat she may then call him out, or go in to the lender.</summary>
    [UnitTitle("Feud: she swears his blood")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudSheSwearsHisBlood : FeudPart
    {
        protected override void Do(Flow flow) => Feud.Swears();
    }

    /// <summary>She takes it to the count's court, for his price: the matter is open.</summary>
    [UnitTitle("Feud: she takes it to the count")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudSheTakesItToTheCount : FeudPart
    {
        protected override void Do(Flow flow) => Feud.TakesItToTheCount();
    }

    /// <summary>She says nothing: the matter is open for the table's quietDays, and then it is let lie.</summary>
    [UnitTitle("Feud: she says nothing")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudSheSaysNothing : FeudPart
    {
        protected override void Do(Flow flow) => Feud.SaysNothing();
    }

    /// <summary>He is laid to rest. WHERE: 1 the priest's ground, 2 the fire, 3 the water.</summary>
    [UnitTitle("Feud: he is laid to rest")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudHeIsLaidToRest : FeudPart
    {
        [DoNotSerialize, PortLabel("Where (1 priest, 2 fire, 3 water)")] public ValueInput place { get; private set; }

        protected override void Ports()
        {
            place = ValueInput(nameof(place), 1);
            Requirement(place, enter);
        }

        protected override void Do(Flow flow) => Feud.LaidToRest(flow.GetValue<int>(place));
    }

    /// <summary>He is carried aboard, for a fire on a shore of his own: asked for some hours out, off a shore, in no gale
    /// (the table's pyreAfterHours and shoreKm).</summary>
    [UnitTitle("Feud: he is carried aboard")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudHeIsCarriedAboard : FeudPart
    {
        protected override void Do(Flow flow) => Feud.CarriedAboard();
    }

    /// <summary>The one who put his knife in Ravn is dead. Said in the card's end.</summary>
    [UnitTitle("Feud: his killer is dead")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudHisKillerIsDead : FeudPart
    {
        protected override void Do(Flow flow) => Feud.KillerIsDead();
    }

    /// <summary>Blood shed in a port is its lord's to fine (the table's townFineD, or what she has); at sea there is
    /// nobody to. Said in the card's end.</summary>
    [UnitTitle("Feud: the port's lord fines the blood")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudThePortsLordFinesTheBlood : FeudPart
    {
        protected override void Do(Flow flow) => Feud.LordFinesTheBlood();
    }

    /// <summary>The word of a deed goes round, from the port she lies at (at sea, from the lender's own). THE DEED: one of
    /// Game/Data/reputation_793.json by its name ("took_blood_for_her_man", "killed_the_lender").</summary>
    [UnitTitle("Feud: the word goes round")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudTheWordGoesRound : FeudPart
    {
        [DoNotSerialize, PortLabel("The deed")] public ValueInput deed { get; private set; }

        protected override void Ports()
        {
            deed = ValueInput(nameof(deed), "");
            Requirement(deed, enter);
        }

        protected override void Do(Flow flow) => Feud.WordGoesRound(flow.GetValue<string>(deed));
    }

    /// <summary>It is over. HOW: 1 his blood, 2 his price, 3 the lender's blood, 4 let lie. The ceiling on the crew's heart
    /// and the mark on her lift, and their heart rises by the table's share for how it ended (bloodHeart, priceHeart).</summary>
    [UnitTitle("Feud: it is over")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudItIsOver : FeudPart
    {
        [DoNotSerialize, PortLabel("How (1 blood, 2 price, 3 the lender, 4 let lie)")] public ValueInput how { get; private set; }

        protected override void Ports()
        {
            how = ValueInput(nameof(how), 1);
            Requirement(how, enter);
        }

        protected override void Do(Flow flow) => Feud.Over(flow.GetValue<int>(how));
    }

    /// <summary>The fight goes against her: a man of hers is dead (never her last). Said in the card's end.</summary>
    [UnitTitle("Feud: the fight is lost")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudTheFightIsLost : FeudPart
    {
        protected override void Do(Flow flow) => Feud.FightLost();
    }

    /// <summary>The lender is dead by her hand: the stick burns with him (nothing is owed, and she works for nobody), his
    /// cards come no more, and she is an outlaw in every port of the Franks. Said in the card's end.</summary>
    [UnitTitle("Feud: the lender is dead")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudTheLenderIsDead : FeudPart
    {
        protected override void Do(Flow flow) => Feud.LenderIsDead();
    }

    /// <summary>She is thrown out of the lender's house. Said in the card's end.</summary>
    [UnitTitle("Feud: she is thrown out of his house")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudSheIsThrownOut : FeudPart
    {
        protected override void Do(Flow flow) => Feud.ThrownOut();
    }

    /// <summary>The count sets Ravn's price on the lender (the table's priceD): it comes off her stick. Said in the card's end.</summary>
    [UnitTitle("Feud: his price comes off the stick")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudHisPriceComesOffTheStick : FeudPart
    {
        protected override void Do(Flow flow) => Feud.PriceOffTheStick();
    }

    /// <summary>She pays the lender's price to the count (the table's outlawFineD), where she has it: the outlawry is
    /// lifted. Short of it nothing is taken, and she is an outlaw still. Said in the card's end.</summary>
    [UnitTitle("Feud: she pays the lender's price")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudShePaysTheLendersPrice : FeudPart
    {
        protected override void Do(Flow flow) => Feud.PaysTheLendersPrice();
    }

    /// <summary>The count's men take the cargo of an outlaw (the crew's own bread and ale are left). Said in the card's end.</summary>
    [UnitTitle("Feud: the count's men take the cargo")]
    [UnitCategory("Kapman/Feud")]
    public sealed class FeudTheCountsMenTakeTheCargo : FeudPart
    {
        protected override void Do(Flow flow) => Feud.CargoSeized();
    }
}
