using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>Opens the market of a port (the market panel) and waits till the player leaves it.</summary>
    [UnitTitle("Market: Open")]
    [UnitCategory("Kapman/Trade")]
    public sealed class MarketOpen : WaitUnit
    {
        [DoNotSerialize] public ValueInput port { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            port = ValueInput<Port>(nameof(port), null);
            Requirement(port, enter);
        }

        protected override IEnumerator Await(Flow flow)
        {
            var panel = MarketPanel.Current;
            if (panel != null)
            {
                panel.Open(flow.GetValue<Port>(port));
                while (panel.IsOpen)
                    yield return null;
            }
            yield return exit;
        }
    }

    /// <summary>Brings every market up to the day: stock made and used, gluts fading, prices with them.</summary>
    [UnitTitle("Markets: Next day")]
    [UnitCategory("Kapman/Trade")]
    public sealed class MarketsNextDay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                if (KapmanGame.Current != null)
                    Markets.CatchUpAll(KapmanGame.Current.Day);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>A deal was done in a market: what, how many, for how much (deniers), and where. Hook reputation,
    /// events and the like on it.</summary>
    [UnitTitle("On trade")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnTrade : GlobalEventUnit<TradeResult>
    {
        public const string Hook = "Kapman.Trade";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput bought { get; private set; }
        [DoNotSerialize] public ValueOutput good { get; private set; }
        [DoNotSerialize] public ValueOutput count { get; private set; }
        [DoNotSerialize] public ValueOutput deniers { get; private set; }
        [DoNotSerialize] public ValueOutput port { get; private set; }
        [DoNotSerialize] public ValueOutput law { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            bought = ValueOutput<bool>(nameof(bought));
            good = ValueOutput<string>(nameof(good));
            count = ValueOutput<int>(nameof(count));
            deniers = ValueOutput<float>(nameof(deniers));
            port = ValueOutput<string>(nameof(port));
            law = ValueOutput<string>(nameof(law));
        }

        protected override void AssignArguments(Flow flow, TradeResult r)
        {
            flow.SetValue(bought, r.bought);
            flow.SetValue(good, r.good);
            flow.SetValue(count, r.count);
            flow.SetValue(deniers, (float)r.deniers);
            flow.SetValue(port, r.port);
            flow.SetValue(law, GameData.Good(r.good)?.law ?? "");
        }
    }

    /// <summary>The merchant's silver: worth paying at a port (in deniers), all of it by weight, and the debt.</summary>
    [UnitTitle("Purse: Silver")]
    [UnitCategory("Kapman/Trade")]
    public sealed class PurseSilver : Unit
    {
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput worthHere { get; private set; }
        [DoNotSerialize] public ValueOutput silver { get; private set; }
        [DoNotSerialize] public ValueOutput debt { get; private set; }

        protected override void Definition()
        {
            port = ValueInput<Port>(nameof(port), null);
            worthHere = ValueOutput<float>(nameof(worthHere), flow =>
            {
                if (!Trade.Started) return 0f;
                var p = flow.GetValue<Port>(port);
                return (float)Trade.Purse.Worth(TradeRules.Current.ZoneOf(p?.Def?.region), TradeRules.Current);
            });
            silver = ValueOutput<float>(nameof(silver), flow => Trade.Started ? (float)Trade.Purse.Silver(TradeRules.Current) : 0f);
            debt = ValueOutput<float>(nameof(debt), flow => (float)Trade.Debt);
        }
    }

    /// <summary>What the hold carries that a law can bite on (arms, captives, sacred things, horses): their ids, and
    /// whether there's any. For searches by officers and raiders.</summary>
    [UnitTitle("Hold: Goods under the law")]
    [UnitCategory("Kapman/Trade")]
    public sealed class HoldUnderLaw : Unit
    {
        [DoNotSerialize] public ValueOutput goods { get; private set; }
        [DoNotSerialize] public ValueOutput any { get; private set; }

        protected override void Definition()
        {
            goods = ValueOutput<List<string>>(nameof(goods), flow =>
                Trade.Started ? Trade.Hold.UnderLaw.Select(g => g.id).ToList() : new List<string>());
            any = ValueOutput<bool>(nameof(any), flow => Trade.Started && Trade.Hold.UnderLaw.Any());
        }
    }

    /// <summary>Passes the merchant's deals on to the graphs' "On trade".</summary>
    static class TradeEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Trade.Traded -= Traded;
            Trade.Traded += Traded;
        }

        static void Traded(TradeResult r) => EventBus.Trigger(OnTrade.Hook, r);
    }
}
