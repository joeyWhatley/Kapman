using System;
using System.Collections.Generic;
using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// THE MEETING WITH THE LENDER'S MEN IS A FLOW (the user's ruling of 7 Oct 2026: flows are graphs; 9 Oct, of that
    /// day's work: "Btw is this all being built like I asked"). Four moments of theirs are events of the graph
    /// "Encounter - the lender's men" (on the '--- GAME ---' object): the lender's day to send comes; they catch her;
    /// she answers them; they give up. What happens at each is a row of small parts (the "The lender: ..." and "The
    /// lender's men: ..." nodes below, each a part of Code/Game/Voyage/Collectors.cs). Change what an answer does,
    /// put a sound or a line of the log between two parts, make the lender slower to anger. The flows run at once
    /// (nothing in them waits), so a saved game holds nothing of them. Their boat itself (sailing the lanes, asking
    /// round a quay, seeing her, rowing for her) stays in the code.
    /// </summary>
    public abstract class LendersMenEvent : GlobalEventUnit<EmptyEventArgs>
    {
        static readonly Dictionary<Type, int> listening = new Dictionary<Type, int>();

        /// <summary>How many graphs of the scene listen for an event of theirs now (with none, the code runs the same
        /// parts itself).</summary>
        public static int Listening(Type of) => listening.TryGetValue(of, out int n) ? n : 0;

        /// <summary>The event of a moment of theirs ("due", "caught", "answered", "gave_up"); null for no such moment.</summary>
        public static Type Of(string moment)
        {
            switch (moment)
            {
                case "due": return typeof(OnTheLendersDayToSend);
                case "caught": return typeof(OnTheLendersMenCatchHer);
                case "answered": return typeof(OnTheLendersMenAreAnswered);
                case "gave_up": return typeof(OnTheLendersMenGiveUp);
            }
            return null;
        }

        public override void StartListening(GraphStack stack)
        {
            bool was = stack.GetElementData<Data>(this).isListening;
            base.StartListening(stack);
            if (!was && stack.GetElementData<Data>(this).isListening)
                listening[GetType()] = Listening(GetType()) + 1;
        }

        public override void StopListening(GraphStack stack)
        {
            bool was = stack.GetElementData<Data>(this).isListening;
            base.StopListening(stack);
            if (was && !stack.GetElementData<Data>(this).isListening)
                listening[GetType()] = Mathf.Max(0, Listening(GetType()) - 1);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bridge()
        {
            listening.Clear();
            Collectors.Flow = moment =>
            {
                var of = Of(moment);
                if (of == null || Listening(of) <= 0)
                    return false;
                EventBus.Trigger("Kapman.LendersMen." + moment);
                return true;
            };
        }
    }

    /// <summary>The lender's day to send his men has come (four times in the life of the debt: collectors_793.json,
    /// "sendings"). WHICH SENDING it is, and whether SHE HAS PAID HIM LATELY (a pound or more on the stick since the
    /// last time).</summary>
    [UnitTitle("On the lender's day to send comes")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheLendersDayToSend : LendersMenEvent
    {
        protected override string hookName => "Kapman.LendersMen.due";

        [DoNotSerialize, PortLabel("Which sending (1 the first)")] public ValueOutput sending { get; private set; }
        [DoNotSerialize, PortLabel("She has paid him lately")] public ValueOutput paidLately { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            sending = ValueOutput<int>(nameof(sending), flow => Collectors.Sent);
            paidLately = ValueOutput<bool>(nameof(paidLately), flow => Collectors.PaidLately);
        }
    }

    /// <summary>The lender's men have caught her: made fast at a quay, or alongside AT SEA.</summary>
    [UnitTitle("On the lender's men catch her")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheLendersMenCatchHer : LendersMenEvent
    {
        protected override string hookName => "Kapman.LendersMen.caught";

        [DoNotSerialize, PortLabel("At sea")] public ValueOutput atSea { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            atSea = ValueOutput<bool>(nameof(atSea), flow => Collectors.State == GoonState.Alongside);
        }
    }

    /// <summary>She has answered the lender's men on their card. THE ANSWER: "paid" (a pound on the stick), "paid_well"
    /// (two or more), "done" (all of it), "cargo" (cargo for an instalment), "astray" (the lie worked: sent the wrong
    /// way), "drunk" (the drink worked), "poured" (the drink was wasted), "shaken" (she ran, at sea), "routed" (she
    /// killed the one who killed Ravn), "fooled" (another way round them worked: which is THE WAY), or "nothing".
    /// AFTER A WAY FAILED: nothing is given after a way round them has just failed.</summary>
    [UnitTitle("On the lender's men are answered")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheLendersMenAreAnswered : LendersMenEvent
    {
        protected override string hookName => "Kapman.LendersMen.answered";

        [DoNotSerialize, PortLabel("The answer")] public ValueOutput answer { get; private set; }
        [DoNotSerialize, PortLabel("The way round them")] public ValueOutput trick { get; private set; }
        [DoNotSerialize, PortLabel("After a way failed")] public ValueOutput angry { get; private set; }
        [DoNotSerialize, PortLabel("At sea")] public ValueOutput atSea { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            answer = ValueOutput<string>(nameof(answer), flow => Collectors.Answer);
            trick = ValueOutput<string>(nameof(trick), flow => Collectors.Trick);
            angry = ValueOutput<bool>(nameof(angry), flow => Collectors.Angry);
            atSea = ValueOutput<bool>(nameof(atSea), flow => Collectors.State == GoonState.Alongside);
        }
    }

    /// <summary>The lender's men give up and go home with nothing. WHY: "days" (their days are out), "nobody" (no quay
    /// knows where she is), "astray" (they were sent the wrong way, and have found it out). That they go home is not
    /// the flow's to say; what the lender makes of it is.</summary>
    [UnitTitle("On the lender's men give up")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTheLendersMenGiveUp : LendersMenEvent
    {
        protected override string hookName => "Kapman.LendersMen.gave_up";

        [DoNotSerialize, PortLabel("Why")] public ValueOutput why { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            why = ValueOutput<string>(nameof(why), flow => Collectors.Why);
        }
    }

    /// <summary>Runs the flow of a moment of theirs straight from a graph asset, with no scene and no machine: for a test
    /// that holds the graph to the code, and for a game without the scene's machine that still wants the graph.</summary>
    public static class LendersMenFlow
    {
        static ScriptGraphAsset held;
        static GraphReference reference;

        public static bool Run(ScriptGraphAsset asset, string moment)
        {
            var of = LendersMenEvent.Of(moment);
            if (asset == null || asset.graph == null || of == null)
                return false;
            LendersMenEvent start = null;
            foreach (var unit in asset.graph.units)
                if (unit.GetType() == of)
                    start = (LendersMenEvent)unit;
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

    /// <summary>A part of the lender's men's flow: it is done at once, and the flow goes on.</summary>
    public abstract class LendersMenPart : Unit
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

    // ------------------------------------------------------------------------------------------------ the lender

    /// <summary>The lender's temper falls by so much (she has paid him something; he has had something of his own).</summary>
    [UnitTitle("The lender: he cools")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LenderCools : LendersMenPart
    {
        [DoNotSerialize, PortLabel("By")] public ValueInput by { get; private set; }

        protected override void Ports()
        {
            by = ValueInput(nameof(by), 1.0);
            Requirement(by, enter);
        }

        protected override void Do(Flow flow) => Collectors.Cools(flow.GetValue<double>(by));
    }

    /// <summary>The lender is the angrier by so much (his men are home with nothing, or she has made fools of them).
    /// His temper runs from 0 to the table's temperMost; what his men do when they are given nothing goes by it.</summary>
    [UnitTitle("The lender: he is the angrier")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LenderIsTheAngrier : LendersMenPart
    {
        [DoNotSerialize, PortLabel("By")] public ValueInput by { get; private set; }

        protected override void Ports()
        {
            by = ValueInput(nameof(by), 1.0);
            Requirement(by, enter);
        }

        protected override void Do(Flow flow) => Collectors.Heats(flow.GetValue<double>(by));
    }

    /// <summary>The lender has all of it: his temper is spent, and nothing is counted against her any more.</summary>
    [UnitTitle("The lender: he has all of it")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LenderHasAllOfIt : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.Content();
    }

    // ------------------------------------------------------------------------------------------------ their boat

    /// <summary>A boat of his men sets out from home, as the sending that has just fallen due: they ask at home first,
    /// then follow the word of her from port to port.</summary>
    [UnitTitle("The lender's men: set out")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenSetOut : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.SetsOut();
    }

    /// <summary>Their card comes (the quay's, or the one alongside at sea): put first in line, and asked for again each
    /// minute while they stand by her, until it can come. The clock drops to a walk.</summary>
    [UnitTitle("The lender's men: their card comes")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenTheirCardComes : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.TheirCard();
    }

    /// <summary>They go home to the lender, by the lanes, with what they got or with nothing.</summary>
    [UnitTitle("The lender's men: go home")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenGoHome : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.GoesHome();
    }

    /// <summary>The sending is called off for good (the stick is clean): nobody is out.</summary>
    [UnitTitle("The lender's men: called off")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenCalledOff : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.CalledOff();
    }

    /// <summary>Sent the wrong way, they row off for a port far from her. NOWHERE: the lanes lead to no such port from
    /// where they are, and they go off to look for it.</summary>
    [UnitTitle("The lender's men: sent the wrong way")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenSentTheWrongWay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabel("They row off")] public ControlOutput rowedOff { get; private set; }
        [DoNotSerialize, PortLabel("Nowhere to send them")] public ControlOutput nowhere { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow => Collectors.RowsOffAstray() ? rowedOff : nowhere);
            rowedOff = ControlOutput(nameof(rowedOff));
            nowhere = ControlOutput(nameof(nowhere));
            Succession(enter, rowedOff);
            Succession(enter, nowhere);
        }
    }

    /// <summary>They sleep, under her thwarts or on the quay, for the table's sleepHours; when they wake they catch her
    /// again if she is still there.</summary>
    [UnitTitle("The lender's men: they sleep")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenSleep : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.Sleeps();
    }

    /// <summary>She has run from them at sea: they lose her for the table's blindHours, and go where the word of her
    /// leads.</summary>
    [UnitTitle("The lender's men: they lose her")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenLoseHer : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.LosesHer();
    }

    // ------------------------------------------------------------------------------------------------ the stick, and the count

    /// <summary>She has paid on the stick of her own will: so many of the visits that ended with nothing are forgiven her
    /// (the third such visit is the one that costs Ravn), and what she has paid "lately" is counted from now.</summary>
    [UnitTitle("The lender's men: she has paid")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenSheHasPaid : LendersMenPart
    {
        [DoNotSerialize, PortLabel("Visits forgiven")] public ValueInput visits { get; private set; }

        protected override void Ports()
        {
            visits = ValueInput(nameof(visits), 1);
            Requirement(visits, enter);
        }

        protected override void Do(Flow flow) => Collectors.Paid(flow.GetValue<int>(visits));
    }

    /// <summary>She gives cargo for an instalment, at their price (the table's cargoPrice): the dearest lots first,
    /// counted on the stick. Said in the card's end.</summary>
    [UnitTitle("The lender's men: cargo for an instalment")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenCargoForAnInstalment : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.CargoForAnInstalment();
    }

    /// <summary>A way round them has worked: it is counted (that way is harder the next time), she has made fools of
    /// them, and they went away with nothing. THE WAY: "hide", "lie", "drink", "plead", "run".</summary>
    [UnitTitle("The lender's men: a way round them worked")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenAWayRoundThemWorked : LendersMenPart
    {
        [DoNotSerialize, PortLabel("The way")] public ValueInput trick { get; private set; }

        protected override void Ports()
        {
            trick = ValueInput(nameof(trick), "");
            Requirement(trick, enter);
        }

        protected override void Do(Flow flow) => Collectors.Tricked(flow.GetValue<string>(trick));
    }

    /// <summary>The cheapest cask of drink aboard is gone: drunk by them, or poured out for nothing.</summary>
    [UnitTitle("The lender's men: a cask is gone")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenACaskIsGone : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.CaskGone();
    }

    // ------------------------------------------------------------------------------------------------ given nothing

    /// <summary>She gives them nothing. The visit is counted as one that ended with nothing paid, and WHAT THEY DO is
    /// reckoned by its two rules ("Rule - what the lender's men come for", "Rule - what the lender's men do"):
    /// 0 they only talk, 1 they take what they came for, 2 and beat a man, 3 and kill a man, 4 and kill Ravn.
    /// Nothing is done yet: the parts that follow do it.</summary>
    [UnitTitle("The lender's men: given nothing")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenGivenNothing : LendersMenPart
    {
        [DoNotSerialize, PortLabel("After a way failed")] public ValueInput angry { get; private set; }
        [DoNotSerialize, PortLabel("What they do (0 to 4)")] public ValueOutput does { get; private set; }

        protected override void Ports()
        {
            angry = ValueInput(nameof(angry), false);
            does = ValueOutput<int>(nameof(does), flow => Collectors.WhatTheyDo);
            Requirement(angry, enter);
        }

        protected override void Do(Flow flow) => Collectors.GivenNothing(flow.GetValue<bool>(angry));
    }

    /// <summary>They only talk: they say what he told them to say, and stand about.</summary>
    [UnitTitle("The lender's men: they only talk")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenOnlyTalk : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.OnlyTalk();
    }

    /// <summary>They take what they came for: silver first, then cargo for the rest; a share of it is for their trouble
    /// (the table's troubleShare) and the rest is counted on the stick. GOT NOTHING: there was nothing in the purse
    /// or the hold worth carrying.</summary>
    [UnitTitle("The lender's men: they take what they came for")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenTake : LendersMenPart
    {
        [DoNotSerialize, PortLabel("Got nothing")] public ValueOutput gotNothing { get; private set; }

        protected override void Ports() => gotNothing = ValueOutput<bool>(nameof(gotNothing), flow => Collectors.GotNothing);

        protected override void Do(Flow flow) => Collectors.Takes();
    }

    /// <summary>One of them beats the nearest of her men with the rope's end: the crew's health and heart fall.</summary>
    [UnitTitle("The lender's men: they beat a man")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenBeatAMan : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.BeatsAMan();
    }

    /// <summary>One of them kills the nearest of her men (never the last man aboard).</summary>
    [UnitTitle("The lender's men: they kill a man")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenKillAMan : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.KillsAMan();
    }

    /// <summary>They kill Ravn (never when he is her last man, and not twice). This card's end only turns to him: his
    /// death has a scene of its own, played when the card is shut (the graph "Scene - Ravn is killed").</summary>
    [UnitTitle("The lender's men: they kill Ravn")]
    [UnitCategory("Kapman/The lender's men")]
    public sealed class LendersMenKillRavn : LendersMenPart
    {
        protected override void Do(Flow flow) => Collectors.KillsRavn();
    }
}
