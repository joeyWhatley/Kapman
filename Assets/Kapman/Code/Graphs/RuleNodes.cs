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
