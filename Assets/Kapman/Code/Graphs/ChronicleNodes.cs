using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>A real event's day has come (Data/chronicle_793.json): what it is, where, and the game's hint for its
    /// effect. Hook what it does to the world on it: a raid on a port, a law from its year, a famine's dear grain.</summary>
    [UnitTitle("On chronicle event")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnChronicleEvent : GlobalEventUnit<ChronicleEntry>
    {
        public const string Hook = "Kapman.Chronicle";
        protected override string hookName => Hook;

        [DoNotSerialize] public ValueOutput id { get; private set; }
        [DoNotSerialize] public ValueOutput kind { get; private set; }
        [DoNotSerialize] public ValueOutput title { get; private set; }
        [DoNotSerialize] public ValueOutput when { get; private set; }
        [DoNotSerialize] public ValueOutput port { get; private set; }
        [DoNotSerialize] public ValueOutput region { get; private set; }
        [DoNotSerialize] public ValueOutput effect { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            id = ValueOutput<string>(nameof(id));
            kind = ValueOutput<string>(nameof(kind));
            title = ValueOutput<string>(nameof(title));
            when = ValueOutput<string>(nameof(when));
            port = ValueOutput<string>(nameof(port));
            region = ValueOutput<string>(nameof(region));
            effect = ValueOutput<string>(nameof(effect));
        }

        protected override void AssignArguments(Flow flow, ChronicleEntry e)
        {
            flow.SetValue(id, e.id);
            flow.SetValue(kind, e.kind);
            flow.SetValue(title, e.title);
            flow.SetValue(when, e.When);
            flow.SetValue(port, e.ports.Length > 0 ? e.ports[0] : "");
            flow.SetValue(region, e.regions.Length > 0 ? e.regions[0] : "");
            flow.SetValue(effect, e.effect);
        }
    }

    /// <summary>
    /// What a chronicle event does to the markets where it happened: their stock of the goods named (a list of categories,
    /// e.g. "Food, Timber"; empty: everything) falls to a share, and the goods are dearer by a share, fading over the
    /// weeks after (trade_rules: shock_fade). Where: the event's ports ("ports"), or every market in its regions
    /// ("regions"). A negative "dearer" makes them cheaper (a good harvest).
    /// </summary>
    [UnitTitle("Chronicle: Shock the markets")]
    [UnitCategory("Kapman/Chronicle")]
    public sealed class ChronicleShock : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput id { get; private set; }
        [DoNotSerialize] public ValueInput where { get; private set; }
        [DoNotSerialize] public ValueInput goods { get; private set; }
        [DoNotSerialize] public ValueInput stockLeft { get; private set; }
        [DoNotSerialize] public ValueInput dearer { get; private set; }
        [DoNotSerialize] public ValueOutput markets { get; private set; }
        int hit;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            id = ValueInput(nameof(id), "");
            where = ValueInput(nameof(where), "ports");
            goods = ValueInput(nameof(goods), "");
            stockLeft = ValueInput(nameof(stockLeft), 1f);
            dearer = ValueInput(nameof(dearer), 0f);
            markets = ValueOutput<int>(nameof(markets), f => hit);
            Requirement(id, enter);
            Requirement(where, enter);
            Requirement(goods, enter);
            Requirement(stockLeft, enter);
            Requirement(dearer, enter);
            Succession(enter, exit);
            Assignment(enter, markets);
        }

        ControlOutput Run(Flow flow)
        {
            hit = 0;
            string key = flow.GetValue<string>(id);
            ChronicleEntry e = null;
            foreach (var c in Chronicle.Entries)
                if (c.id == key)
                {
                    e = c;
                    break;
                }
            var game = KapmanGame.Current;
            if (e == null || game == null)
                return exit;
            var cats = flow.GetValue<string>(goods).Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < cats.Length; i++)
                cats[i] = cats[i].Trim();
            System.Func<GoodDef, bool> which = cats.Length == 0 ? (System.Func<GoodDef, bool>)null : g => System.Array.IndexOf(cats, g.cat) >= 0;
            float left = flow.GetValue<float>(stockLeft), more = flow.GetValue<float>(dearer);
            bool regions = flow.GetValue<string>(where) == "regions";
            foreach (var p in GameData.Ports)
            {
                bool here = regions ? System.Array.IndexOf(e.regions, p.region) >= 0 : System.Array.IndexOf(e.ports, p.id) >= 0;
                if (!here || !p.Exists(game.Year))
                    continue;
                Markets.Of(p, game.Year, game.Day).Shock(which, left, more);
                hit++;
            }
            return exit;
        }
    }

    /// <summary>Passes the chronicle's events on to the graphs' "On chronicle event".</summary>
    static class ChronicleEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Chronicle.Came -= Came;
            Chronicle.Came += Came;
        }

        static void Came(ChronicleEntry e) => EventBus.Trigger(OnChronicleEvent.Hook, e);
    }
}
