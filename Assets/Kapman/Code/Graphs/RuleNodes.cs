using System;
using System.Collections.Generic;
using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// A RULE'S START (KAPMAN, step 3: "Rules as graphs too"). A graph that holds a rule begins with one of these: its
    /// ports on the right are what the game gives the rule (what was sold for, the toll's share...), and the flow from
    /// it must reach a "Rule: the answer" node. The game asks for the rule by its name (GameRules), and
    /// <see cref="RuleGraphs"/> runs the graph from here.
    /// TO ADD A RULE: a class like <see cref="TollRule"/> (its name, its ports, how what is given is put on them), a
    /// graph that starts with it (Editor/Game/RuleGraphsMade.cs makes the first cut), and the asking in GameRules.
    /// </summary>
    public abstract class RuleUnit : Unit
    {
        /// <summary>The rule's name in the code (GameRules asks for it by this).</summary>
        public abstract string Rule { get; }

        [DoNotSerialize, PortLabelHidden] public ControlOutput trigger { get; private set; }

        protected override void Definition()
        {
            isControlRoot = true;
            trigger = ControlOutput(nameof(trigger));
            Given();
        }

        /// <summary>Makes the ports of what the rule is given.</summary>
        protected abstract void Given();

        /// <summary>Puts what the code gives on the ports, for this run of the graph.</summary>
        public abstract void Give(Flow flow, double[] given);
    }

    /// <summary>The toll on a sale of the merchant's. Given: what the market paid, the zone's share, what the toll is
    /// multiplied by here and now, and whether she is free of toll here. The answer: the toll in deniers.</summary>
    [UnitTitle("Rule: the toll on a sale")]
    [UnitShortTitle("Rule: the toll on a sale")]
    [UnitCategory("Kapman/Rules")]
    public sealed class TollRule : RuleUnit
    {
        public override string Rule => "toll";

        [DoNotSerialize, PortLabel("Sold for (d)")] public ValueOutput soldFor { get; private set; }
        [DoNotSerialize, PortLabel("The toll's share")] public ValueOutput share { get; private set; }
        [DoNotSerialize, PortLabel("Times, here and now")] public ValueOutput factor { get; private set; }
        [DoNotSerialize, PortLabel("Free of toll here")] public ValueOutput free { get; private set; }

        protected override void Given()
        {
            soldFor = ValueOutput<double>(nameof(soldFor));
            share = ValueOutput<double>(nameof(share));
            factor = ValueOutput<double>(nameof(factor));
            free = ValueOutput<bool>(nameof(free));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(soldFor, given[0]);
            flow.SetValue(share, given[1]);
            flow.SetValue(factor, given[2]);
            flow.SetValue(free, given[3] != 0.0);
        }
    }

    /// <summary>What the crew is owed for a day. Given: the men aboard and a man's pay for a day. The answer: deniers.</summary>
    [UnitTitle("Rule: the crew's pay for a day")]
    [UnitShortTitle("Rule: the crew's pay for a day")]
    [UnitCategory("Kapman/Rules")]
    public sealed class PayRule : RuleUnit
    {
        public override string Rule => "pay";

        [DoNotSerialize, PortLabel("Men aboard")] public ValueOutput men { get; private set; }
        [DoNotSerialize, PortLabel("A man's pay a day (d)")] public ValueOutput payAMan { get; private set; }

        protected override void Given()
        {
            men = ValueOutput<double>(nameof(men));
            payAMan = ValueOutput<double>(nameof(payAMan));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(men, given[0]);
            flow.SetValue(payAMan, given[1]);
        }
    }

    /// <summary>The crew's health after a day (0 to 1). Given: their health, whether they went hungry, went thirsty or
    /// were fed and watered, and what each takes or gives. The answer: their health now.</summary>
    [UnitTitle("Rule: the crew's health after a day")]
    [UnitShortTitle("Rule: the crew's health after a day")]
    [UnitCategory("Kapman/Rules")]
    public sealed class HealthRule : RuleUnit
    {
        public override string Rule => "health";

        [DoNotSerialize, PortLabel("Health (0 to 1)")] public ValueOutput health { get; private set; }
        [DoNotSerialize, PortLabel("Went hungry")] public ValueOutput hungry { get; private set; }
        [DoNotSerialize, PortLabel("Went thirsty")] public ValueOutput thirsty { get; private set; }
        [DoNotSerialize, PortLabel("Fed and watered")] public ValueOutput well { get; private set; }
        [DoNotSerialize, PortLabel("A hungry day takes")] public ValueOutput hungerLoss { get; private set; }
        [DoNotSerialize, PortLabel("A thirsty day takes")] public ValueOutput thirstLoss { get; private set; }
        [DoNotSerialize, PortLabel("A good day gives")] public ValueOutput gain { get; private set; }

        protected override void Given()
        {
            health = ValueOutput<double>(nameof(health));
            hungry = ValueOutput<bool>(nameof(hungry));
            thirsty = ValueOutput<bool>(nameof(thirsty));
            well = ValueOutput<bool>(nameof(well));
            hungerLoss = ValueOutput<double>(nameof(hungerLoss));
            thirstLoss = ValueOutput<double>(nameof(thirstLoss));
            gain = ValueOutput<double>(nameof(gain));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(health, given[0]);
            flow.SetValue(hungry, given[1] != 0.0);
            flow.SetValue(thirsty, given[2] != 0.0);
            flow.SetValue(well, given[3] != 0.0);
            flow.SetValue(hungerLoss, given[4]);
            flow.SetValue(thirstLoss, given[5]);
            flow.SetValue(gain, given[6]);
        }
    }

    /// <summary>The crew's heart after a day (0 to 1). Given: their heart, what the sea took today, whether they went
    /// hungry or thirsty, whether they are owed more than a week's pay, whether the day was spent in port, and what
    /// each takes or gives. The answer: their heart now.</summary>
    [UnitTitle("Rule: the crew's heart after a day")]
    [UnitShortTitle("Rule: the crew's heart after a day")]
    [UnitCategory("Kapman/Rules")]
    public sealed class HeartRule : RuleUnit
    {
        public override string Rule => "heart";

        [DoNotSerialize, PortLabel("Heart (0 to 1)")] public ValueOutput heart { get; private set; }
        [DoNotSerialize, PortLabel("Lost to the sea today")] public ValueOutput lostToTheSea { get; private set; }
        [DoNotSerialize, PortLabel("Went hungry")] public ValueOutput hungry { get; private set; }
        [DoNotSerialize, PortLabel("Went thirsty")] public ValueOutput thirsty { get; private set; }
        [DoNotSerialize, PortLabel("Owed over a week's pay")] public ValueOutput unpaid { get; private set; }
        [DoNotSerialize, PortLabel("The day was in port")] public ValueOutput inPort { get; private set; }
        [DoNotSerialize, PortLabel("A hungry day takes")] public ValueOutput hungryLoss { get; private set; }
        [DoNotSerialize, PortLabel("A thirsty day takes")] public ValueOutput thirstyLoss { get; private set; }
        [DoNotSerialize, PortLabel("An unpaid day takes")] public ValueOutput unpaidLoss { get; private set; }
        [DoNotSerialize, PortLabel("A day in port gives")] public ValueOutput portGain { get; private set; }

        protected override void Given()
        {
            heart = ValueOutput<double>(nameof(heart));
            lostToTheSea = ValueOutput<double>(nameof(lostToTheSea));
            hungry = ValueOutput<bool>(nameof(hungry));
            thirsty = ValueOutput<bool>(nameof(thirsty));
            unpaid = ValueOutput<bool>(nameof(unpaid));
            inPort = ValueOutput<bool>(nameof(inPort));
            hungryLoss = ValueOutput<double>(nameof(hungryLoss));
            thirstyLoss = ValueOutput<double>(nameof(thirstyLoss));
            unpaidLoss = ValueOutput<double>(nameof(unpaidLoss));
            portGain = ValueOutput<double>(nameof(portGain));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(heart, given[0]);
            flow.SetValue(lostToTheSea, given[1]);
            flow.SetValue(hungry, given[2] != 0.0);
            flow.SetValue(thirsty, given[3] != 0.0);
            flow.SetValue(unpaid, given[4] != 0.0);
            flow.SetValue(inPort, given[5] != 0.0);
            flow.SetValue(hungryLoss, given[6]);
            flow.SetValue(thirstyLoss, given[7]);
            flow.SetValue(unpaidLoss, given[8]);
            flow.SetValue(portGain, given[9]);
        }
    }

    /// <summary>Takes an amount off a number, if a thing is so (with nothing joined to "If", always).</summary>
    [UnitTitle("If so, take off")]
    [UnitShortTitle("If so, take off")]
    [UnitCategory("Kapman/Rules")]
    public sealed class TakeOffIf : Unit
    {
        [DoNotSerialize, PortLabel("From")] public ValueInput from { get; private set; }
        [DoNotSerialize, PortLabel("Take off")] public ValueInput amount { get; private set; }
        [DoNotSerialize, PortLabel("If")] public ValueInput when { get; private set; }
        [DoNotSerialize, PortLabel("Left")] public ValueOutput left { get; private set; }

        protected override void Definition()
        {
            from = ValueInput<double>(nameof(from), 0.0);
            amount = ValueInput<double>(nameof(amount), 0.0);
            when = ValueInput<bool>(nameof(when), true);
            left = ValueOutput(nameof(left), flow => flow.GetValue<bool>(when) ? flow.GetValue<double>(from) - flow.GetValue<double>(amount) : flow.GetValue<double>(from));
            Requirement(from, left);
            Requirement(amount, left);
            Requirement(when, left);
        }
    }

    /// <summary>Adds an amount to a number, if a thing is so (with nothing joined to "If", always).</summary>
    [UnitTitle("If so, add")]
    [UnitShortTitle("If so, add")]
    [UnitCategory("Kapman/Rules")]
    public sealed class AddIf : Unit
    {
        [DoNotSerialize, PortLabel("To")] public ValueInput to { get; private set; }
        [DoNotSerialize, PortLabel("Add")] public ValueInput amount { get; private set; }
        [DoNotSerialize, PortLabel("If")] public ValueInput when { get; private set; }
        [DoNotSerialize, PortLabel("Sum")] public ValueOutput sum { get; private set; }

        protected override void Definition()
        {
            to = ValueInput<double>(nameof(to), 0.0);
            amount = ValueInput<double>(nameof(amount), 0.0);
            when = ValueInput<bool>(nameof(when), true);
            sum = ValueOutput(nameof(sum), flow => flow.GetValue<bool>(when) ? flow.GetValue<double>(to) + flow.GetValue<double>(amount) : flow.GetValue<double>(to));
            Requirement(to, sum);
            Requirement(amount, sum);
            Requirement(when, sum);
        }
    }

    /// <summary>A share kept between none and all: under 0 it is 0, over 1 it is 1.</summary>
    [UnitTitle("Keep between 0 and 1")]
    [UnitShortTitle("Keep between 0 and 1")]
    [UnitCategory("Kapman/Rules")]
    public sealed class Between01 : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput number { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput kept { get; private set; }

        protected override void Definition()
        {
            number = ValueInput<double>(nameof(number), 0.0);
            kept = ValueOutput(nameof(kept), flow => Math.Max(0.0, Math.Min(1.0, flow.GetValue<double>(number))));
            Requirement(number, kept);
        }
    }

    /// <summary>The end of a rule's graph: the number it comes to goes back to the game.</summary>
    [UnitTitle("Rule: the answer")]
    [UnitShortTitle("Rule: the answer")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RuleAnswer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabel("Answer")] public ValueInput number { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                RuleGraphs.Answered(flow.GetValue<double>(number));
                return null;
            });
            number = ValueInput<double>(nameof(number), 0.0);
            Requirement(number, enter);
        }
    }

    /// <summary>A sum of money to the hundredth of a denier, as the game's own reckoning rounds it.</summary>
    [UnitTitle("Money: to the hundredth")]
    [UnitShortTitle("To the hundredth")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoundToHundredth : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput sum { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput rounded { get; private set; }

        protected override void Definition()
        {
            sum = ValueInput<double>(nameof(sum), 0.0);
            rounded = ValueOutput(nameof(rounded), flow => Math.Round(flow.GetValue<double>(sum), 2));
            Requirement(sum, rounded);
        }
    }

    /// <summary>
    /// RUNS A RULE'S GRAPH for the game (GameRules.Graph): finds the graph whose start is the rule asked for, puts what
    /// is given on the start's ports, runs the flow from it, and returns what reached "Rule: the answer". It needs no
    /// scene and no play: the graph is run where it lies, as an asset.
    /// </summary>
    public static class RuleGraphs
    {
        sealed class Entry
        {
            public ScriptGraphAsset asset;
            public RuleUnit start;
            public GraphReference reference;
        }

        static Dictionary<string, Entry> found;
        static double answer;
        static bool answered;
        static int depth;

        /// <summary>The book in use (Resources/GameRules); a test may put its own.</summary>
        public static RuleBook Book
        {
            get => book != null ? book : book = Resources.Load<RuleBook>(RuleBook.ResourceName);
            set
            {
                book = value;
                found = null;
            }
        }
        static RuleBook book;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Install()
        {
            found = null;
            book = null;
            depth = 0;
            GameRules.Graph = Run;
        }

        /// <summary>Has a graph of the book this rule?</summary>
        public static bool Has(string rule) => Find(rule) != null;

        static Entry Find(string rule)
        {
            if (found == null)
            {
                found = new Dictionary<string, Entry>();
                var b = Book;
                if (b != null)
                    foreach (var asset in b.graphs)
                    {
                        if (asset == null || asset.graph == null)
                            continue;
                        foreach (var unit in asset.graph.units)
                            if (unit is RuleUnit start && !found.ContainsKey(start.Rule))
                                found[start.Rule] = new Entry { asset = asset, start = start };
                    }
            }
            return found.TryGetValue(rule, out var e) ? e : null;
        }

        /// <summary>Runs the graph of a rule. `ok` false where no graph holds it, or its flow reached no answer.</summary>
        public static (bool ok, double answer) Run(string rule, double[] given)
        {
            var e = Find(rule);
            if (e == null)
                return (false, 0.0);
            if (e.reference == null)
                e.reference = GraphReference.New(e.asset, true);
            // (a rule asked for from inside another rule's graph: each keeps its own answer)
            double outerAnswer = answer;
            bool outerAnswered = answered;
            depth++;
            answered = false;
            try
            {
                var flow = Flow.New(e.reference);
                e.start.Give(flow, given);
                flow.Run(e.start.trigger);
                return (answered, answer);
            }
            finally
            {
                depth--;
                answer = outerAnswer;
                answered = outerAnswered;
            }
        }

        /// <summary>From "Rule: the answer": what the graph came to.</summary>
        public static void Answered(double value)
        {
            if (depth <= 0)
                return;
            answer = value;
            answered = true;
        }
    }
}
