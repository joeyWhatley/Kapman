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

    // ------------------------------------------------------------------ a fight, and the lender's men (9 Oct 2026)

    /// <summary>How a fight stands, on her nerve's own scale. Given: her nerve, her men and theirs, whether there are
    /// weapons aboard, what the place costs her, and what a man and weapons are worth. The answer: a number a card's
    /// chance is carried by (each point over 3 is a twentieth on its odds).</summary>
    [UnitTitle("Rule: how a fight stands")]
    [UnitShortTitle("Rule: how a fight stands")]
    [UnitCategory("Kapman/Rules")]
    public sealed class FightRule : RuleUnit
    {
        public override string Rule => "fight";

        [DoNotSerialize, PortLabel("Her nerve (1 to 5)")] public ValueOutput nerve { get; private set; }
        [DoNotSerialize, PortLabel("Her men")] public ValueOutput herMen { get; private set; }
        [DoNotSerialize, PortLabel("Their men")] public ValueOutput theirMen { get; private set; }
        [DoNotSerialize, PortLabel("Weapons aboard")] public ValueOutput weapons { get; private set; }
        [DoNotSerialize, PortLabel("What the place costs her")] public ValueOutput place { get; private set; }
        [DoNotSerialize, PortLabel("A man is worth")] public ValueOutput perMan { get; private set; }
        [DoNotSerialize, PortLabel("Weapons are worth")] public ValueOutput forWeapons { get; private set; }

        protected override void Given()
        {
            nerve = ValueOutput<double>(nameof(nerve));
            herMen = ValueOutput<double>(nameof(herMen));
            theirMen = ValueOutput<double>(nameof(theirMen));
            weapons = ValueOutput<bool>(nameof(weapons));
            place = ValueOutput<double>(nameof(place));
            perMan = ValueOutput<double>(nameof(perMan));
            forWeapons = ValueOutput<double>(nameof(forWeapons));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(nerve, given[0]);
            flow.SetValue(herMen, given[1]);
            flow.SetValue(theirMen, given[2]);
            flow.SetValue(weapons, given[3] != 0.0);
            flow.SetValue(place, given[4]);
            flow.SetValue(perMan, given[5]);
            flow.SetValue(forWeapons, given[6]);
        }
    }

    /// <summary>How easy a way round the lender's men is, on her stat's own scale. Given: the stat that carries the
    /// trick, how often that way has fooled them before, which sending this is, and what each of those costs.</summary>
    [UnitTitle("Rule: a way round the lender's men")]
    [UnitShortTitle("Rule: a way round the lender's men")]
    [UnitCategory("Kapman/Rules")]
    public sealed class TrickRule : RuleUnit
    {
        public override string Rule => "trick";

        [DoNotSerialize, PortLabel("Her stat (1 to 5)")] public ValueOutput stat { get; private set; }
        [DoNotSerialize, PortLabel("Times it fooled them before")] public ValueOutput usedBefore { get; private set; }
        [DoNotSerialize, PortLabel("Which sending (1 the first)")] public ValueOutput visit { get; private set; }
        [DoNotSerialize, PortLabel("Each time before costs")] public ValueOutput toldPenalty { get; private set; }
        [DoNotSerialize, PortLabel("Each later sending costs")] public ValueOutput laterPenalty { get; private set; }

        protected override void Given()
        {
            stat = ValueOutput<double>(nameof(stat));
            usedBefore = ValueOutput<double>(nameof(usedBefore));
            visit = ValueOutput<double>(nameof(visit));
            toldPenalty = ValueOutput<double>(nameof(toldPenalty));
            laterPenalty = ValueOutput<double>(nameof(laterPenalty));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(stat, given[0]);
            flow.SetValue(usedBefore, given[1]);
            flow.SetValue(visit, given[2]);
            flow.SetValue(toldPenalty, given[3]);
            flow.SetValue(laterPenalty, given[4]);
        }
    }

    /// <summary>What the lender's men come for, in deniers. Given: the lender's temper (a step more when a way round
    /// them has just failed), the instalment, and how much more of it for each step of his temper over 2.</summary>
    [UnitTitle("Rule: what the lender's men come for")]
    [UnitShortTitle("Rule: what the lender's men come for")]
    [UnitCategory("Kapman/Rules")]
    public sealed class InstalmentRule : RuleUnit
    {
        public override string Rule => "instalment";

        [DoNotSerialize, PortLabel("The lender's temper")] public ValueOutput temper { get; private set; }
        [DoNotSerialize, PortLabel("An instalment (d)")] public ValueOutput instalment { get; private set; }
        [DoNotSerialize, PortLabel("More of it for each step over 2")] public ValueOutput moreByTemper { get; private set; }

        protected override void Given()
        {
            temper = ValueOutput<double>(nameof(temper));
            instalment = ValueOutput<double>(nameof(instalment));
            moreByTemper = ValueOutput<double>(nameof(moreByTemper));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(temper, given[0]);
            flow.SetValue(instalment, given[1]);
            flow.SetValue(moreByTemper, given[2]);
        }
    }

    /// <summary>What the lender's men do when they are given nothing. The answer is one of five: 0 they only talk, 1
    /// they take what they came for, 2 they take and beat a man, 3 they take and kill a man, 4 they take and kill
    /// Ravn.</summary>
    [UnitTitle("Rule: what the lender's men do")]
    [UnitShortTitle("Rule: what the lender's men do")]
    [UnitCategory("Kapman/Rules")]
    public sealed class GoonsDoRule : RuleUnit
    {
        public override string Rule => "goons";

        [DoNotSerialize, PortLabel("The lender's temper")] public ValueOutput temper { get; private set; }
        [DoNotSerialize, PortLabel("A way round them has just failed")] public ValueOutput fooled { get; private set; }
        [DoNotSerialize, PortLabel("Aboard to take (d)")] public ValueOutput aboard { get; private set; }
        [DoNotSerialize, PortLabel("Her men")] public ValueOutput men { get; private set; }
        [DoNotSerialize, PortLabel("A throw (0 to 1)")] public ValueOutput throwOf { get; private set; }
        [DoNotSerialize, PortLabel("Visits with nothing paid")] public ValueOutput unpaid { get; private set; }
        [DoNotSerialize, PortLabel("Ravn lives")] public ValueOutput ravnAlive { get; private set; }
        [DoNotSerialize, PortLabel("They only talk up to")] public ValueOutput warnUpTo { get; private set; }
        [DoNotSerialize, PortLabel("A man is beaten from")] public ValueOutput beatFrom { get; private set; }
        [DoNotSerialize, PortLabel("A man is killed from")] public ValueOutput killFrom { get; private set; }
        [DoNotSerialize, PortLabel("Chance of a killing a step early")] public ValueOutput killEarly { get; private set; }
        [DoNotSerialize, PortLabel("It is Ravn at this count")] public ValueOutput ravnAt { get; private set; }
        [DoNotSerialize, PortLabel("What they came for (d)")] public ValueOutput want { get; private set; }

        protected override void Given()
        {
            temper = ValueOutput<double>(nameof(temper));
            fooled = ValueOutput<bool>(nameof(fooled));
            aboard = ValueOutput<double>(nameof(aboard));
            men = ValueOutput<double>(nameof(men));
            throwOf = ValueOutput<double>(nameof(throwOf));
            unpaid = ValueOutput<double>(nameof(unpaid));
            ravnAlive = ValueOutput<bool>(nameof(ravnAlive));
            warnUpTo = ValueOutput<double>(nameof(warnUpTo));
            beatFrom = ValueOutput<double>(nameof(beatFrom));
            killFrom = ValueOutput<double>(nameof(killFrom));
            killEarly = ValueOutput<double>(nameof(killEarly));
            ravnAt = ValueOutput<double>(nameof(ravnAt));
            want = ValueOutput<double>(nameof(want));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(temper, given[0]);
            flow.SetValue(fooled, given[1] != 0.0);
            flow.SetValue(aboard, given[2]);
            flow.SetValue(men, given[3]);
            flow.SetValue(throwOf, given[4]);
            flow.SetValue(unpaid, given[5]);
            flow.SetValue(ravnAlive, given[6] != 0.0);
            flow.SetValue(warnUpTo, given[7]);
            flow.SetValue(beatFrom, given[8]);
            flow.SetValue(killFrom, given[9]);
            flow.SetValue(killEarly, given[10]);
            flow.SetValue(ravnAt, given[11]);
            flow.SetValue(want, given[12]);
        }
    }

    /// <summary>A number kept between a least and a most.</summary>
    [UnitTitle("Keep between")]
    [UnitShortTitle("Keep between")]
    [UnitCategory("Kapman/Rules")]
    public sealed class KeepBetween : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput number { get; private set; }
        [DoNotSerialize, PortLabel("At least")] public ValueInput least { get; private set; }
        [DoNotSerialize, PortLabel("At most")] public ValueInput most { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput kept { get; private set; }

        protected override void Definition()
        {
            number = ValueInput<double>(nameof(number), 0.0);
            least = ValueInput<double>(nameof(least), 0.0);
            most = ValueInput<double>(nameof(most), 1.0);
            kept = ValueOutput(nameof(kept), flow => Math.Max(flow.GetValue<double>(least), Math.Min(flow.GetValue<double>(most), flow.GetValue<double>(number))));
            Requirement(number, kept);
            Requirement(least, kept);
            Requirement(most, kept);
        }
    }

    /// <summary>A sum to the whole number, as the game's own reckoning rounds it.</summary>
    [UnitTitle("To the whole number")]
    [UnitShortTitle("To the whole")]
    [UnitCategory("Kapman/Rules")]
    public sealed class ToTheWhole : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput sum { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput rounded { get; private set; }

        protected override void Definition()
        {
            sum = ValueInput<double>(nameof(sum), 0.0);
            rounded = ValueOutput(nameof(rounded), flow => Math.Round(flow.GetValue<double>(sum)));
            Requirement(sum, rounded);
        }
    }

    /// <summary>Is one number at least another, over it, or under it? (Pick which with "How".)</summary>
    [UnitTitle("Is it so?")]
    [UnitShortTitle("Is it so?")]
    [UnitCategory("Kapman/Rules")]
    public sealed class IsIt : Unit
    {
        public enum How { AtLeast, Over, Under, AtMost }

        /// <summary>Which question is asked of the two numbers.</summary>
        [Serialize, Inspectable, UnitHeaderInspectable("How")] public How how = How.AtLeast;

        [DoNotSerialize, PortLabel("This")] public ValueInput a { get; private set; }
        [DoNotSerialize, PortLabel("That")] public ValueInput b { get; private set; }
        [DoNotSerialize, PortLabel("So")] public ValueOutput so { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<double>(nameof(a), 0.0);
            b = ValueInput<double>(nameof(b), 0.0);
            so = ValueOutput(nameof(so), flow =>
            {
                double x = flow.GetValue<double>(a), y = flow.GetValue<double>(b);
                return how == How.AtLeast ? x >= y : how == How.Over ? x > y : how == How.Under ? x < y : x <= y;
            });
            Requirement(a, so);
            Requirement(b, so);
        }
    }

    /// <summary>Are two things both so, or is either of them so? (Pick which with "How"; "Not" turns the answer round.)</summary>
    [UnitTitle("Both, or either")]
    [UnitShortTitle("Both, or either")]
    [UnitCategory("Kapman/Rules")]
    public sealed class BothOrEither : Unit
    {
        public enum How { Both, Either, FirstAndNotSecond }

        [Serialize, Inspectable, UnitHeaderInspectable("How")] public How how = How.Both;

        [DoNotSerialize, PortLabel("One")] public ValueInput one { get; private set; }
        [DoNotSerialize, PortLabel("Other")] public ValueInput other { get; private set; }
        [DoNotSerialize, PortLabel("So")] public ValueOutput so { get; private set; }

        protected override void Definition()
        {
            one = ValueInput<bool>(nameof(one), false);
            other = ValueInput<bool>(nameof(other), false);
            so = ValueOutput(nameof(so), flow =>
            {
                bool x = flow.GetValue<bool>(one), y = flow.GetValue<bool>(other);
                return how == How.Both ? x && y : how == How.Either ? x || y : x && !y;
            });
            Requirement(one, so);
            Requirement(other, so);
        }
    }

    /// <summary>One number or another, by whether a thing is so.</summary>
    [UnitTitle("If so, this; else that")]
    [UnitShortTitle("If so, this; else that")]
    [UnitCategory("Kapman/Rules")]
    public sealed class ThisOrThat : Unit
    {
        [DoNotSerialize, PortLabel("If")] public ValueInput when { get; private set; }
        [DoNotSerialize, PortLabel("This")] public ValueInput yes { get; private set; }
        [DoNotSerialize, PortLabel("Else that")] public ValueInput no { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput picked { get; private set; }

        protected override void Definition()
        {
            when = ValueInput<bool>(nameof(when), false);
            yes = ValueInput<double>(nameof(yes), 0.0);
            no = ValueInput<double>(nameof(no), 0.0);
            picked = ValueOutput(nameof(picked), flow => flow.GetValue<bool>(when) ? flow.GetValue<double>(yes) : flow.GetValue<double>(no));
            Requirement(when, picked);
            Requirement(yes, picked);
            Requirement(no, picked);
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
