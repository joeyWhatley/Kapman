using System.Collections;
using Kapman.Game;
using Kapman.Game.Sim;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>The lookout has made out a ship (her kind, her home, where she's bound): which, her facts, and whether a
    /// meeting can be offered now (nothing else on, the merchant at sea, no panel open).</summary>
    [UnitTitle("On ship made out")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnShipMadeOut : GlobalEventUnit<SimShip>
    {
        public const string Hook = "Kapman.ShipMadeOut";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput ship { get; private set; }
        [DoNotSerialize] public ValueOutput label { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }
        [DoNotSerialize] public ValueOutput canMeet { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            ship = ValueOutput<int>(nameof(ship));
            label = ValueOutput<string>(nameof(label));
            facts = ValueOutput<string>(nameof(facts));
            canMeet = ValueOutput<bool>(nameof(canMeet));
        }

        protected override void AssignArguments(Flow flow, SimShip s)
        {
            flow.SetValue(ship, s.index);
            string l = SeaWatch.Label(s);
            flow.SetValue(label, char.ToUpperInvariant(l[0]) + l.Substring(1));
            flow.SetValue(facts, Encounter.Facts(s));
            flow.SetValue(canMeet, Encounter.CanOffer(s));
        }
    }

    /// <summary>The lookout has sighted a sail (too far yet to make out).</summary>
    [UnitTitle("On ship sighted")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnShipSighted : GlobalEventUnit<SimShip>
    {
        public const string Hook = "Kapman.ShipSighted";
        protected override string hookName => Hook;
        [DoNotSerialize] public ValueOutput ship { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            ship = ValueOutput<int>(nameof(ship));
        }

        protected override void AssignArguments(Flow flow, SimShip s) => flow.SetValue(ship, s.index);
    }

    /// <summary>A ship the lookout had is out of sight again.</summary>
    [UnitTitle("On ship lost")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnShipLost : GlobalEventUnit<SimShip>
    {
        public const string Hook = "Kapman.ShipLost";
        protected override string hookName => Hook;
        [DoNotSerialize] public ValueOutput ship { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            ship = ValueOutput<int>(nameof(ship));
        }

        protected override void AssignArguments(Flow flow, SimShip s) => flow.SetValue(ship, s.index);
    }

    /// <summary>Offers a meeting with a ship made out: she's the one met now. False if one can't be offered.</summary>
    [UnitTitle("Encounter: Offer")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterOffer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] public ControlOutput offered { get; private set; }
        [DoNotSerialize] public ControlOutput not { get; private set; }
        [DoNotSerialize] public ValueInput ship { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var w = KapmanGame.Current != null ? KapmanGame.Current.World : null;
                int i = flow.GetValue<int>(ship);
                return w != null && i >= 0 && i < w.ships.Count && Encounter.Offer(w.ships[i]) ? offered : not;
            });
            offered = ControlOutput(nameof(offered));
            not = ControlOutput(nameof(not));
            ship = ValueInput(nameof(ship), -1);
            Requirement(ship, enter);
            Succession(enter, offered);
            Succession(enter, not);
        }
    }

    /// <summary>"Close and hail": she heaves to and your ship makes for her.</summary>
    [UnitTitle("Encounter: Close and hail")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterCloseAndHail : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                Encounter.CloseAndHail();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>Waits till your ship is alongside her (the clock then holds), or the meeting is over.</summary>
    [UnitTitle("Encounter: Wait till alongside")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterWaitAlongside : WaitUnit
    {
        [DoNotSerialize] public ValueOutput alongside { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            alongside = ValueOutput<bool>(nameof(alongside));
            Assignment(enter, alongside);
        }

        protected override IEnumerator Await(Flow flow)
        {
            while (Encounter.Stage == EncounterStage.Closing)
                yield return null;
            flow.SetValue(alongside, Encounter.Stage == EncounterStage.Alongside);
            yield return exit;
        }
    }

    /// <summary>The weather she met on her way (every 6 game hours back to 48), as facts.</summary>
    [UnitTitle("Encounter: Weather she met")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterWeatherNews : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueOutput news { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = Encounter.WeatherNews(KapmanGame.Current != null ? KapmanGame.Current.World : null, Encounter.Ship);
                Encounter.LastNews = text;
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            news = ValueOutput<string>(nameof(news), f => text);
            Succession(enter, exit);
            Assignment(enter, news);
        }
    }

    /// <summary>The prices she knows, newer than yours, into your ledger as heard from her; the facts of it.</summary>
    [UnitTitle("Encounter: Prices she knows")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterPricesHeard : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueOutput news { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = Encounter.PricesHeard(KapmanGame.Current != null ? KapmanGame.Current.World : null, Encounter.Ship);
                Encounter.LastNews = text;
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            news = ValueOutput<string>(nameof(news), f => text);
            Succession(enter, exit);
            Assignment(enter, news);
        }
    }

    /// <summary>What she'd sell from her hold (a little of her biggest lot, at her destination's price less 15%); gives
    /// the facts, and whether there's an offer to take.</summary>
    [UnitTitle("Encounter: Her offer")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterMakeOffer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] public ControlOutput offered { get; private set; }
        [DoNotSerialize] public ControlOutput none { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = Encounter.MakeOffer(KapmanGame.Current != null ? KapmanGame.Current.World : null, Encounter.Ship);
                Encounter.LastNews = text;
                return Encounter.ForSale != null ? offered : none;
            });
            offered = ControlOutput(nameof(offered));
            none = ControlOutput(nameof(none));
            facts = ValueOutput<string>(nameof(facts), f => text);
            Succession(enter, offered);
            Succession(enter, none);
            Assignment(enter, facts);
        }
    }

    /// <summary>Takes her offer: her goods into your hold, your silver to her. The fact of it.</summary>
    [UnitTitle("Encounter: Take her offer")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterTakeOffer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                Encounter.LastNews = Encounter.TakeOffer();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The ship met now: her label ("A north sea merchant ship of Dorestad"), her facts, and what she last told
    /// (or that you're alongside, if nothing yet).</summary>
    [UnitTitle("Encounter: The ship")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterShip : Unit
    {
        [DoNotSerialize] public ValueOutput label { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }
        [DoNotSerialize] public ValueOutput news { get; private set; }

        protected override void Definition()
        {
            label = ValueOutput<string>(nameof(label), f => Encounter.Ship != null ? Cap(SeaWatch.Label(Encounter.Ship)) : "");
            facts = ValueOutput<string>(nameof(facts), f => Encounter.Facts(Encounter.Ship));
            news = ValueOutput<string>(nameof(news), f =>
                string.IsNullOrEmpty(Encounter.LastNews) ? "Alongside. Time stands still while you talk." : Encounter.LastNews);
        }

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);
    }

    /// <summary>They part (or the offer is let go): she sails on, the clock runs, your course resumes.</summary>
    [UnitTitle("Encounter: Part")]
    [UnitCategory("Kapman/Encounter")]
    public sealed class EncounterPart : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                Encounter.Part();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    // ------------------------------------------------------------------------------------------------ raiders

    /// <summary>The lookout has made out raiders: which band, their facts, and whether they're coming for you (they have
    /// room for more, it's daylight, you're at sea and nothing else is on).</summary>
    [UnitTitle("On raiders made out")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnRaidersMadeOut : GlobalEventUnit<SimShip>
    {
        public const string Hook = "Kapman.RaidersMadeOut";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput ship { get; private set; }
        [DoNotSerialize] public ValueOutput comes { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            ship = ValueOutput<int>(nameof(ship));
            comes = ValueOutput<bool>(nameof(comes));
        }

        protected override void AssignArguments(Flow flow, SimShip s)
        {
            flow.SetValue(ship, s.index);
            flow.SetValue(comes, RaiderEncounter.CanThreaten(s));
        }
    }

    /// <summary>They come for you: the chase is theirs to make from here (the sim leaves them to it).</summary>
    [UnitTitle("Raiders: They come on")]
    [UnitCategory("Kapman/Raiders")]
    public sealed class RaidersThreaten : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] public ControlOutput coming { get; private set; }
        [DoNotSerialize] public ControlOutput not { get; private set; }
        [DoNotSerialize] public ValueInput ship { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var w = KapmanGame.Current != null ? KapmanGame.Current.World : null;
                int i = flow.GetValue<int>(ship);
                return w != null && i >= 0 && i < w.ships.Count && RaiderEncounter.Threaten(w.ships[i]) ? coming : not;
            });
            coming = ControlOutput(nameof(coming));
            not = ControlOutput(nameof(not));
            ship = ValueInput(nameof(ship), -1);
            Requirement(ship, enter);
            Succession(enter, coming);
            Succession(enter, not);
        }
    }

    /// <summary>The chase in numbers (who, how many, how far, closing how fast, sunset).</summary>
    [UnitTitle("Raiders: Status")]
    [UnitCategory("Kapman/Raiders")]
    public sealed class RaidersStatus : Unit
    {
        [DoNotSerialize] public ValueOutput status { get; private set; }

        protected override void Definition() => status = ValueOutput<string>(nameof(status), f => RaiderEncounter.Status());
    }

    /// <summary>Run: your ship sails away from them at her best.</summary>
    [UnitTitle("Raiders: Run")]
    [UnitCategory("Kapman/Raiders")]
    public sealed class RaidersRun : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                RaiderEncounter.Run();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The chase, live: the panel's status line keeps the numbers; it ends "caught", "escaped", or with the
    /// option picked on the panel meanwhile ("fight", "tribute", "submit" for options 1, 2, 3).</summary>
    [UnitTitle("Raiders: Chase")]
    [UnitCategory("Kapman/Raiders")]
    public sealed class RaidersChase : WaitUnit
    {
        [DoNotSerialize] public ValueOutput result { get; private set; }
        static readonly string[] Picks = { "fight", "tribute", "submit" };

        protected override void Definition()
        {
            base.Definition();
            result = ValueOutput<string>(nameof(result));
            Assignment(enter, result);
        }

        protected override IEnumerator Await(Flow flow)
        {
            string how = "escaped";
            float next = 0f;
            while (true)
            {
                if (RaiderEncounter.Stage == RaidStage.Caught) { how = "caught"; break; }
                if (RaiderEncounter.Stage == RaidStage.None || !string.IsNullOrEmpty(RaiderEncounter.Ended)) { how = "escaped"; break; }
                var panel = TextPanel.Current;
                if (panel != null && panel.Choice >= 0 && panel.Choice < Picks.Length) { how = Picks[panel.Choice]; break; }
                if (panel != null && Time.unscaledTime >= next)
                {
                    panel.SetIntention(RaiderEncounter.Status());
                    next = Time.unscaledTime + 0.5f;
                }
                yield return null;
            }
            Debug.Log($"Kapman raiders: chase ended '{how}' (stage {RaiderEncounter.Stage}, choice {TextPanel.Current?.Choice})");
            flow.SetValue(result, how);
            yield return exit;
        }
    }

    /// <summary>Your answer to raiders: "fight", "tribute", "parley", "submit", or "escaped" (the chase ended without
    /// them). Gives the outcome as facts, and whether it's settled (a parley they won't hear isn't).</summary>
    [UnitTitle("Raiders: Answer")]
    [UnitCategory("Kapman/Raiders")]
    public sealed class RaidersAnswer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] public ControlOutput settled { get; private set; }
        [DoNotSerialize] public ControlOutput unsettled { get; private set; }
        [DoNotSerialize] public ValueInput answer { get; private set; }
        [DoNotSerialize] public ValueOutput outcome { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            settled = ControlOutput(nameof(settled));
            unsettled = ControlOutput(nameof(unsettled));
            answer = ValueInput(nameof(answer), "submit");
            outcome = ValueOutput<string>(nameof(outcome), f => text);
            Requirement(answer, enter);
            Succession(enter, settled);
            Succession(enter, unsettled);
            Assignment(enter, outcome);
        }

        ControlOutput Run(Flow flow)
        {
            bool done = true;
            Debug.Log($"Kapman raiders: answer '{flow.GetValue<string>(answer)}' at stage {RaiderEncounter.Stage}");
            switch (flow.GetValue<string>(answer))
            {
                case "fight": text = RaiderEncounter.Fight(); break;
                case "tribute": text = RaiderEncounter.PayTribute(); break;
                case "parley":
                    if (RaiderEncounter.ParleyTried)
                    {
                        text = "They won't talk.";
                        done = false;
                    }
                    else
                        text = RaiderEncounter.Parley(out done);
                    break;
                case "escaped": text = RaiderEncounter.Escaped(); break;
                default: text = RaiderEncounter.Submit(); break;
            }
            return done ? settled : unsettled;
        }
    }

    // ------------------------------------------------------------------------------------------------ the king's men

    /// <summary>The king's men: a search is due as you cast off ("port"), or a guard boat signals at sea ("sea"); the
    /// facts (whose men, what's aboard under their law).</summary>
    [UnitTitle("On the king's men")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnKingsMen : GlobalEventUnit<string>
    {
        public const string Hook = "Kapman.KingsMen";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput kind { get; private set; }
        [DoNotSerialize] public ValueOutput atSea { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            kind = ValueOutput<string>(nameof(kind));
            atSea = ValueOutput<bool>(nameof(atSea));
            facts = ValueOutput<string>(nameof(facts));
        }

        protected override void AssignArguments(Flow flow, string k)
        {
            flow.SetValue(kind, k);
            flow.SetValue(atSea, k == "sea");
            flow.SetValue(facts, Patrols.Facts());
        }
    }

    /// <summary>Your answer to the king's men: "search" (let them), "declare", "gift", "heave_to", "dump", "run". Gives
    /// the outcome, as facts (the hold and purse changed, deeds kept).</summary>
    [UnitTitle("Officers: Answer")]
    [UnitCategory("Kapman/Law")]
    public sealed class OfficersAnswer : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput answer { get; private set; }
        [DoNotSerialize] public ValueOutput outcome { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = Patrols.Answer(flow.GetValue<string>(answer));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            answer = ValueInput(nameof(answer), "search");
            outcome = ValueOutput<string>(nameof(outcome), f => text);
            Requirement(answer, enter);
            Succession(enter, exit);
            Assignment(enter, outcome);
        }
    }

    static class PatrolsEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Patrols.Due -= Due;
            Patrols.Due += Due;
        }

        static void Due(string kind) => EventBus.Trigger(OnKingsMen.Hook, kind);
    }

    /// <summary>Passes the lookout's sightings on to the graphs' "On ship sighted / made out / lost" (and raiders made out
    /// to "On raiders made out").</summary>
    static class SeaWatchEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            SeaWatch.Sighted -= Sighted;
            SeaWatch.Sighted += Sighted;
            SeaWatch.MadeOut -= MadeOut;
            SeaWatch.MadeOut += MadeOut;
            SeaWatch.Lost -= Lost;
            SeaWatch.Lost += Lost;
            RaiderEncounter.Coming -= Coming;
            RaiderEncounter.Coming += Coming;
        }

        static void Sighted(SimShip s) => EventBus.Trigger(OnShipSighted.Hook, s);
        static void MadeOut(SimShip s)
        {
            if (!s.raider)
                EventBus.Trigger(OnShipMadeOut.Hook, s);                     // (raiders come on through Coming, when they can)
        }
        static void Coming(SimShip s) => EventBus.Trigger(OnRaidersMadeOut.Hook, s);
        static void Lost(SimShip s) => EventBus.Trigger(OnShipLost.Hook, s);
    }
}
