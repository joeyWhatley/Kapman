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

    // ------------------------------------------------------------------ wind and tide (9 Oct 2026)

    /// <summary>Her leeway under sail: how many degrees off her heading she is set, to the side the wind blows to. Given: how far the wind is off her bow, the wind, and the table's angles.</summary>
    [UnitTitle("Rule: her leeway under sail")]
    [UnitShortTitle("Rule: her leeway under sail")]
    [UnitCategory("Kapman/Rules")]
    public sealed class LeewayRule : RuleUnit
    {
        public override string Rule => "leeway";

        [DoNotSerialize, PortLabel("The wind off her bow (0 ahead, 180 astern)")] public ValueOutput off { get; private set; }
        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("Leeway beating")] public ValueOutput beating { get; private set; }
        [DoNotSerialize, PortLabel("Leeway reaching")] public ValueOutput reaching { get; private set; }
        [DoNotSerialize, PortLabel("Leeway running")] public ValueOutput running { get; private set; }
        [DoNotSerialize, PortLabel("Those are at a wind of (m/s)")] public ValueOutput atWind { get; private set; }
        [DoNotSerialize, PortLabel("Never more than")] public ValueOutput most { get; private set; }

        protected override void Given()
        {
            off = ValueOutput<double>(nameof(off));
            wind = ValueOutput<double>(nameof(wind));
            beating = ValueOutput<double>(nameof(beating));
            reaching = ValueOutput<double>(nameof(reaching));
            running = ValueOutput<double>(nameof(running));
            atWind = ValueOutput<double>(nameof(atWind));
            most = ValueOutput<double>(nameof(most));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(off, given[0]);
            flow.SetValue(wind, given[1]);
            flow.SetValue(beating, given[2]);
            flow.SetValue(reaching, given[3]);
            flow.SetValue(running, given[4]);
            flow.SetValue(atWind, given[5]);
            flow.SetValue(most, given[6]);
        }
    }

    /// <summary>How fast she is blown bodily downwind with no sail drawing (under oars, the sail down, lying to), in m/s. Given: the wind, whether she lies to, and the table's two shares of the wind's speed.</summary>
    [UnitTitle("Rule: blown downwind with no sail")]
    [UnitShortTitle("Rule: blown downwind with no sail")]
    [UnitCategory("Kapman/Rules")]
    public sealed class BlownRule : RuleUnit
    {
        public override string Rule => "blown";

        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("She lies to")] public ValueOutput lyingTo { get; private set; }
        [DoNotSerialize, PortLabel("Share of the wind, under oars")] public ValueOutput bare { get; private set; }
        [DoNotSerialize, PortLabel("Share of the wind, lying to")] public ValueOutput lyingToShare { get; private set; }

        protected override void Given()
        {
            wind = ValueOutput<double>(nameof(wind));
            lyingTo = ValueOutput<bool>(nameof(lyingTo));
            bare = ValueOutput<double>(nameof(bare));
            lyingToShare = ValueOutput<double>(nameof(lyingToShare));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(wind, given[0]);
            flow.SetValue(lyingTo, given[1] != 0.0);
            flow.SetValue(bare, given[2]);
            flow.SetValue(lyingToShare, given[3]);
        }
    }

    /// <summary>How fast the top of the sea itself runs before a steady wind, in m/s (it runs a little to the right of the wind: the table's seaTurn). Given: the wind and the table's share of it.</summary>
    [UnitTitle("Rule: the top of the sea runs")]
    [UnitShortTitle("Rule: the top of the sea runs")]
    [UnitCategory("Kapman/Rules")]
    public sealed class SeaRunsRule : RuleUnit
    {
        public override string Rule => "sea_runs";

        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("Share of the wind")] public ValueOutput share { get; private set; }

        protected override void Given()
        {
            wind = ValueOutput<double>(nameof(wind));
            share = ValueOutput<double>(nameof(share));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(wind, given[0]);
            flow.SetValue(share, given[1]);
        }
    }

    /// <summary>How far the steersman makes out his landfall, in real metres: less by night, and in thick weather no further than the air lets him see; never under 300 m.</summary>
    [UnitTitle("Rule: how far he makes out a landfall")]
    [UnitShortTitle("Rule: how far he makes out a landfall")]
    [UnitCategory("Kapman/Rules")]
    public sealed class LandfallRule : RuleUnit
    {
        public override string Rule => "landfall";

        [DoNotSerialize, PortLabel("By day, in clear air (m)")] public ValueOutput far { get; private set; }
        [DoNotSerialize, PortLabel("It is night")] public ValueOutput night { get; private set; }
        [DoNotSerialize, PortLabel("The share of that by night")] public ValueOutput nightShare { get; private set; }
        [DoNotSerialize, PortLabel("Thick weather")] public ValueOutput thick { get; private set; }
        [DoNotSerialize, PortLabel("As far as the air lets him see (m)")] public ValueOutput air { get; private set; }

        protected override void Given()
        {
            far = ValueOutput<double>(nameof(far));
            night = ValueOutput<bool>(nameof(night));
            nightShare = ValueOutput<double>(nameof(nightShare));
            thick = ValueOutput<bool>(nameof(thick));
            air = ValueOutput<double>(nameof(air));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(far, given[0]);
            flow.SetValue(night, given[1] != 0.0);
            flow.SetValue(nightShare, given[2]);
            flow.SetValue(thick, given[3] != 0.0);
            flow.SetValue(air, given[4]);
        }
    }

    /// <summary>Along a coast the land tells the steersman a share of his error each hour. The answer: the share of his error he still has after so many hours (1: all of it). In thick fog the land tells him nothing.</summary>
    [UnitTitle("Rule: what he keeps of his error along a coast")]
    [UnitShortTitle("Rule: what he keeps of his error along a coast")]
    [UnitCategory("Kapman/Rules")]
    public sealed class ErrorKeptRule : RuleUnit
    {
        public override string Rule => "error_kept";

        [DoNotSerialize, PortLabel("Hours")] public ValueOutput hours { get; private set; }
        [DoNotSerialize, PortLabel("It is night")] public ValueOutput night { get; private set; }
        [DoNotSerialize, PortLabel("Thick fog")] public ValueOutput thick { get; private set; }
        [DoNotSerialize, PortLabel("Found out in an hour, by day")] public ValueOutput eyeDay { get; private set; }
        [DoNotSerialize, PortLabel("Found out in an hour, by night")] public ValueOutput eyeNight { get; private set; }

        protected override void Given()
        {
            hours = ValueOutput<double>(nameof(hours));
            night = ValueOutput<bool>(nameof(night));
            thick = ValueOutput<bool>(nameof(thick));
            eyeDay = ValueOutput<double>(nameof(eyeDay));
            eyeNight = ValueOutput<double>(nameof(eyeNight));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(hours, given[0]);
            flow.SetValue(night, given[1] != 0.0);
            flow.SetValue(thick, given[2] != 0.0);
            flow.SetValue(eyeDay, given[3]);
            flow.SetValue(eyeNight, given[4]);
        }
    }

    /// <summary>Set well off her line, does the steersman lay her course again himself (the user's ruling of 10 Oct 2026)? The answer: 1 he does, 0 he tells her. Given: is land in sight, and is the air thick (each 1 or 0).</summary>
    [UnitTitle("Rule: does the steersman lay her course again himself")]
    [UnitShortTitle("Rule: does he lay her course again himself")]
    [UnitCategory("Kapman/Rules")]
    public sealed class LaysAgainRule : RuleUnit
    {
        public override string Rule => "lays_again";

        [DoNotSerialize, PortLabel("Land is in sight (1 or 0)")] public ValueOutput land { get; private set; }
        [DoNotSerialize, PortLabel("The air is thick (1 or 0)")] public ValueOutput thick { get; private set; }

        protected override void Given()
        {
            land = ValueOutput<double>(nameof(land));
            thick = ValueOutput<double>(nameof(thick));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(land, given[0]);
            flow.SetValue(thick, given[1]);
        }
    }

    /// <summary>Does a press on the chart take her line, so that the course can be pulled (13 Oct 2026)? Given: how far
    /// the pointer is from the line, in pixels of the chart as shown, and how many pixels the line is held from. The
    /// answer: 1 it takes the line, 0 the press is the chart's (a drag of the map, or a click to sail).</summary>
    [UnitTitle("Rule: does the press take the line")]
    [UnitShortTitle("Rule: does the press take the line")]
    [UnitCategory("Kapman/Rules")]
    public sealed class PressTakesLineRule : RuleUnit
    {
        public override string Rule => "press_takes_line";

        [DoNotSerialize, PortLabel("Pixels from the line")] public ValueOutput distance { get; private set; }
        [DoNotSerialize, PortLabel("Pixels the line is held from")] public ValueOutput hold { get; private set; }

        protected override void Given()
        {
            distance = ValueOutput<double>(nameof(distance));
            hold = ValueOutput<double>(nameof(hold));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(distance, given[0]);
            flow.SetValue(hold, given[1]);
        }
    }

    /// <summary>Lying to with a reckoning lost, does she sail on by herself now (the user's ruling of 10 Oct 2026)? The answer: 1 she does, 0 she waits for her course to be laid. Given: was it lost in thick air (1 or 0), how far the steersman sees now, and from how far the air counts as clear.</summary>
    [UnitTitle("Rule: does she sail on when the air clears")]
    [UnitShortTitle("Rule: does she sail on when the air clears")]
    [UnitCategory("Kapman/Rules")]
    public sealed class SailsOnRule : RuleUnit
    {
        public override string Rule => "sails_on";

        [DoNotSerialize, PortLabel("Lost in thick air (1 or 0)")] public ValueOutput lost { get; private set; }
        [DoNotSerialize, PortLabel("He sees now (m)")] public ValueOutput sees { get; private set; }
        [DoNotSerialize, PortLabel("Clear from (m)")] public ValueOutput clearFrom { get; private set; }

        protected override void Given()
        {
            lost = ValueOutput<double>(nameof(lost));
            sees = ValueOutput<double>(nameof(sees));
            clearFrom = ValueOutput<double>(nameof(clearFrom));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(lost, given[0]);
            flow.SetValue(sees, given[1]);
            flow.SetValue(clearFrom, given[2]);
        }
    }

    /// <summary>Is a card answered as she answered it the last time, with nothing asked of her (the user's ruling of 10 Oct 2026, of the king's toll)? The answer: 1 it is, 0 the card comes. Given, each 1 or 0: her last answer may stand and can be given today; a better answer is open to her today; a choice is open that brings its people to her in person.</summary>
    [UnitTitle("Rule: is a card answered as before")]
    [UnitShortTitle("Rule: is a card answered as before")]
    [UnitCategory("Kapman/Rules")]
    public sealed class AsBeforeRule : RuleUnit
    {
        public override string Rule => "as_before";

        [DoNotSerialize, PortLabel("Her last answer stands (1 or 0)")] public ValueOutput stands { get; private set; }
        [DoNotSerialize, PortLabel("A better answer is open (1 or 0)")] public ValueOutput better { get; private set; }
        [DoNotSerialize, PortLabel("Something brings them (1 or 0)")] public ValueOutput meets { get; private set; }

        protected override void Given()
        {
            stands = ValueOutput<double>(nameof(stands));
            better = ValueOutput<double>(nameof(better));
            meets = ValueOutput<double>(nameof(meets));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(stands, given[0]);
            flow.SetValue(better, given[1]);
            flow.SetValue(meets, given[2]);
        }
    }

    /// <summary>Lying to, can she lie to her anchor and not be carried? The answer: 1 she can, 0 she cannot. Given: the wind, the wind of a gale, the depth of the water, and the table's deepest water to anchor in.</summary>
    [UnitTitle("Rule: can she lie to her anchor")]
    [UnitShortTitle("Rule: can she lie to her anchor")]
    [UnitCategory("Kapman/Rules")]
    public sealed class AnchorRule : RuleUnit
    {
        public override string Rule => "anchor";

        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("A gale blows from (m/s)")] public ValueOutput gale { get; private set; }
        [DoNotSerialize, PortLabel("The water's depth (m)")] public ValueOutput depth { get; private set; }
        [DoNotSerialize, PortLabel("Deepest to anchor in (m)")] public ValueOutput anchorDepth { get; private set; }

        protected override void Given()
        {
            wind = ValueOutput<double>(nameof(wind));
            gale = ValueOutput<double>(nameof(gale));
            depth = ValueOutput<double>(nameof(depth));
            anchorDepth = ValueOutput<double>(nameof(anchorDepth));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(wind, given[0]);
            flow.SetValue(gale, given[1]);
            flow.SetValue(depth, given[2]);
            flow.SetValue(anchorDepth, given[3]);
        }
    }

    /// <summary>Held off a shore the push would set her on: the share of her hull it costs in this step, or nothing when she has the way on to keep clear of it.</summary>
    [UnitTitle("Rule: what a lee shore costs her hull")]
    [UnitShortTitle("Rule: what a lee shore costs her hull")]
    [UnitCategory("Kapman/Rules")]
    public sealed class LeeShoreRule : RuleUnit
    {
        public override string Rule => "lee_shore";

        [DoNotSerialize, PortLabel("She lies to")] public ValueOutput lyingTo { get; private set; }
        [DoNotSerialize, PortLabel("Her way through the water (m/s)")] public ValueOutput speed { get; private set; }
        [DoNotSerialize, PortLabel("The push onto the shore (m/s)")] public ValueOutput push { get; private set; }
        [DoNotSerialize, PortLabel("Her way must be so many times the push")] public ValueOutput clawOff { get; private set; }
        [DoNotSerialize, PortLabel("It blows a gale")] public ValueOutput gale { get; private set; }
        [DoNotSerialize, PortLabel("The hull's share an hour")] public ValueOutput strainHour { get; private set; }
        [DoNotSerialize, PortLabel("The hull's share an hour, in a gale")] public ValueOutput strainGale { get; private set; }
        [DoNotSerialize, PortLabel("Hours")] public ValueOutput hours { get; private set; }

        protected override void Given()
        {
            lyingTo = ValueOutput<bool>(nameof(lyingTo));
            speed = ValueOutput<double>(nameof(speed));
            push = ValueOutput<double>(nameof(push));
            clawOff = ValueOutput<double>(nameof(clawOff));
            gale = ValueOutput<bool>(nameof(gale));
            strainHour = ValueOutput<double>(nameof(strainHour));
            strainGale = ValueOutput<double>(nameof(strainGale));
            hours = ValueOutput<double>(nameof(hours));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(lyingTo, given[0] != 0.0);
            flow.SetValue(speed, given[1]);
            flow.SetValue(push, given[2]);
            flow.SetValue(clawOff, given[3]);
            flow.SetValue(gale, given[4] != 0.0);
            flow.SetValue(strainHour, given[5]);
            flow.SetValue(strainGale, given[6]);
            flow.SetValue(hours, given[7]);
        }
    }

    /// <summary>How rough a tide race is now: 0 quiet, up to its own roughness and a fifth over. Rough while the tide's stream runs, worst with the wind against the stream, quiet at slack water.</summary>
    [UnitTitle("Rule: how rough a tide race is")]
    [UnitShortTitle("Rule: how rough a tide race is")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoughRaceRule : RuleUnit
    {
        public override string Rule => "rough_race";

        [DoNotSerialize, PortLabel("The water's own roughness")] public ValueOutput rough { get; private set; }
        [DoNotSerialize, PortLabel("How hard the stream runs (0 slack, 1 its hardest)")] public ValueOutput run { get; private set; }
        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("The wind against the stream (0 to 1)")] public ValueOutput against { get; private set; }

        protected override void Given()
        {
            rough = ValueOutput<double>(nameof(rough));
            run = ValueOutput<double>(nameof(run));
            wind = ValueOutput<double>(nameof(wind));
            against = ValueOutput<double>(nameof(against));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(rough, given[0]);
            flow.SetValue(run, given[1]);
            flow.SetValue(wind, given[2]);
            flow.SetValue(against, given[3]);
        }
    }

    /// <summary>How rough the bar of a river mouth is now: 0 quiet, up to its own roughness. Rough with the wind blowing on to it, worse while the tide runs.</summary>
    [UnitTitle("Rule: how rough a river's bar is")]
    [UnitShortTitle("Rule: how rough a river's bar is")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoughBarRule : RuleUnit
    {
        public override string Rule => "rough_bar";

        [DoNotSerialize, PortLabel("The water's own roughness")] public ValueOutput rough { get; private set; }
        [DoNotSerialize, PortLabel("How hard the stream runs (0 slack, 1 its hardest)")] public ValueOutput run { get; private set; }
        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }
        [DoNotSerialize, PortLabel("The wind blowing on to it (0 to 1)")] public ValueOutput onshore { get; private set; }

        protected override void Given()
        {
            rough = ValueOutput<double>(nameof(rough));
            run = ValueOutput<double>(nameof(run));
            wind = ValueOutput<double>(nameof(wind));
            onshore = ValueOutput<double>(nameof(onshore));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(rough, given[0]);
            flow.SetValue(run, given[1]);
            flow.SetValue(wind, given[2]);
            flow.SetValue(onshore, given[3]);
        }
    }

    /// <summary>How rough shoal water is now: 0 quiet, up to its own roughness. It breaks in any fresh wind, a little worse while the tide runs.</summary>
    [UnitTitle("Rule: how rough a shoal is")]
    [UnitShortTitle("Rule: how rough a shoal is")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoughShoalRule : RuleUnit
    {
        public override string Rule => "rough_shoal";

        [DoNotSerialize, PortLabel("The water's own roughness")] public ValueOutput rough { get; private set; }
        [DoNotSerialize, PortLabel("How hard the stream runs (0 slack, 1 its hardest)")] public ValueOutput run { get; private set; }
        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }

        protected override void Given()
        {
            rough = ValueOutput<double>(nameof(rough));
            run = ValueOutput<double>(nameof(run));
            wind = ValueOutput<double>(nameof(wind));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(rough, given[0]);
            flow.SetValue(run, given[1]);
            flow.SetValue(wind, given[2]);
        }
    }

    /// <summary>How rough the water off a cape with no shelter is now: 0 quiet, up to its own roughness. Rough in a fresh wind, worse while the tide runs.</summary>
    [UnitTitle("Rule: how rough an open cape is")]
    [UnitShortTitle("Rule: how rough an open cape is")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoughCapeRule : RuleUnit
    {
        public override string Rule => "rough_cape";

        [DoNotSerialize, PortLabel("The water's own roughness")] public ValueOutput rough { get; private set; }
        [DoNotSerialize, PortLabel("How hard the stream runs (0 slack, 1 its hardest)")] public ValueOutput run { get; private set; }
        [DoNotSerialize, PortLabel("The wind (m/s)")] public ValueOutput wind { get; private set; }

        protected override void Given()
        {
            rough = ValueOutput<double>(nameof(rough));
            run = ValueOutput<double>(nameof(run));
            wind = ValueOutput<double>(nameof(wind));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(rough, given[0]);
            flow.SetValue(run, given[1]);
            flow.SetValue(wind, given[2]);
        }
    }

    /// <summary>The share of her speed she keeps in rough water (1: all of it). Given: how rough it is now, and the table's share kept where the roughness is 1.</summary>
    [UnitTitle("Rule: her speed in rough water")]
    [UnitShortTitle("Rule: her speed in rough water")]
    [UnitCategory("Kapman/Rules")]
    public sealed class RoughSlowRule : RuleUnit
    {
        public override string Rule => "rough_slow";

        [DoNotSerialize, PortLabel("How rough it is now")] public ValueOutput rough { get; private set; }
        [DoNotSerialize, PortLabel("Share of her speed kept at 1")] public ValueOutput kept { get; private set; }

        protected override void Given()
        {
            rough = ValueOutput<double>(nameof(rough));
            kept = ValueOutput<double>(nameof(kept));
        }

        public override void Give(Flow flow, double[] given)
        {
            flow.SetValue(rough, given[0]);
            flow.SetValue(kept, given[1]);
        }
    }

    /// <summary>One number times another.</summary>
    [UnitTitle("Times")]
    [UnitShortTitle("Times")]
    [UnitCategory("Kapman/Rules")]
    public sealed class Times : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput a { get; private set; }
        [DoNotSerialize, PortLabel("Times")] public ValueInput b { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput product { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<double>(nameof(a), 0.0);
            b = ValueInput<double>(nameof(b), 1.0);
            product = ValueOutput(nameof(product), flow => flow.GetValue<double>(a) * flow.GetValue<double>(b));
            Requirement(a, product);
            Requirement(b, product);
        }
    }

    /// <summary>One number divided by another.</summary>
    [UnitTitle("Divided by")]
    [UnitShortTitle("Divided by")]
    [UnitCategory("Kapman/Rules")]
    public sealed class DividedBy : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput a { get; private set; }
        [DoNotSerialize, PortLabel("Divided by")] public ValueInput b { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput quotient { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<double>(nameof(a), 0.0);
            b = ValueInput<double>(nameof(b), 1.0);
            quotient = ValueOutput(nameof(quotient), flow => flow.GetValue<double>(a) / flow.GetValue<double>(b));
            Requirement(a, quotient);
            Requirement(b, quotient);
        }
    }

    /// <summary>Where a number stands between two others, as a share: 0 at "from" and under it, 1 at "up to" and over it
    /// (a wind of 6 between 2 and 10 is a half).</summary>
    [UnitTitle("Share between")]
    [UnitShortTitle("Share between")]
    [UnitCategory("Kapman/Rules")]
    public sealed class ShareBetween : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput number { get; private set; }
        [DoNotSerialize, PortLabel("From")] public ValueInput from { get; private set; }
        [DoNotSerialize, PortLabel("Up to")] public ValueInput upTo { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput share { get; private set; }

        protected override void Definition()
        {
            number = ValueInput<double>(nameof(number), 0.0);
            from = ValueInput<double>(nameof(from), 0.0);
            upTo = ValueInput<double>(nameof(upTo), 1.0);
            share = ValueOutput(nameof(share), flow =>
            {
                double lo = flow.GetValue<double>(from), hi = flow.GetValue<double>(upTo);
                return Math.Max(0.0, Math.Min(1.0, (flow.GetValue<double>(number) - lo) / Math.Max(1e-6, hi - lo)));
            });
            Requirement(number, share);
            Requirement(from, share);
            Requirement(upTo, share);
        }
    }

    /// <summary>What is left of a thing that goes at a steady share an hour, after so many hours (1: all of it): at a half
    /// an hour, six tenths is left after one hour and a little over a third after two.</summary>
    [UnitTitle("What is left after")]
    [UnitShortTitle("What is left after")]
    [UnitCategory("Kapman/Rules")]
    public sealed class WhatIsLeftAfter : Unit
    {
        [DoNotSerialize, PortLabel("Going, a share an hour")] public ValueInput rate { get; private set; }
        [DoNotSerialize, PortLabel("Hours")] public ValueInput hours { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput left { get; private set; }

        protected override void Definition()
        {
            rate = ValueInput<double>(nameof(rate), 0.0);
            hours = ValueInput<double>(nameof(hours), 1.0);
            left = ValueOutput(nameof(left), flow => Math.Exp(-Math.Max(0.0, flow.GetValue<double>(rate)) * flow.GetValue<double>(hours)));
            Requirement(rate, left);
            Requirement(hours, left);
        }
    }

    /// <summary>A number, or the most it may be if it is over that.</summary>
    [UnitTitle("No more than")]
    [UnitShortTitle("No more than")]
    [UnitCategory("Kapman/Rules")]
    public sealed class NoMoreThan : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput number { get; private set; }
        [DoNotSerialize, PortLabel("No more than")] public ValueInput most { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput kept { get; private set; }

        protected override void Definition()
        {
            number = ValueInput<double>(nameof(number), 0.0);
            most = ValueInput<double>(nameof(most), 1.0);
            kept = ValueOutput(nameof(kept), flow => Math.Min(flow.GetValue<double>(most), flow.GetValue<double>(number)));
            Requirement(number, kept);
            Requirement(most, kept);
        }
    }

    /// <summary>A number, or the least it may be if it is under that.</summary>
    [UnitTitle("No less than")]
    [UnitShortTitle("No less than")]
    [UnitCategory("Kapman/Rules")]
    public sealed class NoLessThan : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ValueInput number { get; private set; }
        [DoNotSerialize, PortLabel("No less than")] public ValueInput least { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ValueOutput kept { get; private set; }

        protected override void Definition()
        {
            number = ValueInput<double>(nameof(number), 0.0);
            least = ValueInput<double>(nameof(least), 0.0);
            kept = ValueOutput(nameof(kept), flow => Math.Max(flow.GetValue<double>(least), flow.GetValue<double>(number)));
            Requirement(number, kept);
            Requirement(least, kept);
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
