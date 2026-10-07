using Kapman.Game;
using Kapman.Sailing;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>The port nearest the ship, how far off its berth is (game metres), and whether she's in it now.</summary>
    [UnitTitle("Ports: Nearest to the ship")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortNearestToShip : Unit
    {
        [DoNotSerialize] public ValueOutput port { get; private set; }
        [DoNotSerialize] public ValueOutput distance { get; private set; }
        [DoNotSerialize] public ValueOutput inPort { get; private set; }

        protected override void Definition()
        {
            port = ValueOutput<Port>(nameof(port), Nearest);
            distance = ValueOutput<float>(nameof(distance), flow =>
            {
                var p = Nearest(flow);
                return p != null ? p.DistanceTo(ShipPosition()) : float.MaxValue;
            });
            inPort = ValueOutput<bool>(nameof(inPort), flow =>
            {
                var p = Nearest(flow);
                return p != null && p.Placed && p.DistanceTo(ShipPosition()) < p.arrivalRadius;
            });
        }

        static Vector3 ShipPosition() => KapmanGame.Current != null ? KapmanGame.Current.ShipPosition : Vector3.zero;

        static Port Nearest(Flow flow) => Port.Nearest(ShipPosition());
    }

    /// <summary>A port's name for showing ("Dorestat (Wijk bij Duurstede)"), and its id.</summary>
    [UnitTitle("Port: Name")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortName : Unit
    {
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput title { get; private set; }
        [DoNotSerialize] public ValueOutput id { get; private set; }

        protected override void Definition()
        {
            port = ValueInput<Port>(nameof(port), null);
            title = ValueOutput<string>(nameof(title), flow => flow.GetValue<Port>(port)?.Title ?? "");
            id = ValueOutput<string>(nameof(id), flow => flow.GetValue<Port>(port)?.id ?? "");
        }
    }

    /// <summary>A port's words for one of its menu's keys (Data/wording_793.json, by its people): the option's label and
    /// its intention. The key "port.arrive" gives what lying in port here means.</summary>
    [UnitTitle("Port: Words")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortWords : Unit
    {
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueInput key { get; private set; }
        [DoNotSerialize] public ValueOutput label { get; private set; }
        [DoNotSerialize] public ValueOutput intention { get; private set; }

        protected override void Definition()
        {
            port = ValueInput<Port>(nameof(port), null);
            key = ValueInput(nameof(key), "port.town");
            label = ValueOutput<string>(nameof(label), flow => Words(flow)?.label ?? "");
            intention = ValueOutput<string>(nameof(intention), flow =>
                flow.GetValue<string>(key) == "port.arrive" ? Wording.Arrive(flow.GetValue<Port>(port)?.Def) : Words(flow)?.intention ?? "");
        }

        PortOption Words(Flow flow) => Wording.Option(flow.GetValue<Port>(port)?.Def, flow.GetValue<string>(key));
    }

    /// <summary>Adds one of the port's menu options to the text panel, in the words of its people (its label, and its
    /// intention in small print). The fallback is used where the table hasn't the key.</summary>
    [UnitTitle("Port: Add option")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortAddOption : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueInput key { get; private set; }
        [DoNotSerialize] public ValueInput fallback { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            key = ValueInput(nameof(key), "port.town");
            fallback = ValueInput(nameof(fallback), "");
            Requirement(port, enter);
            Requirement(key, enter);
            Requirement(fallback, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            string k = flow.GetValue<string>(key);
            var o = Wording.Option(flow.GetValue<Port>(port)?.Def, k);
            if (o != null)
                TextPanel.Current?.AddOption(k, o.intention, o.label);
            else
                TextPanel.Current?.AddOption(k, flow.GetValue<string>(fallback));
            return exit;
        }
    }

    /// <summary>Adds the errands this kind of place offers (the procurator or the moneyer at an emporium, the guest-house
    /// at a monastery, the hall at a royal seat...) after the menu's own options; they come with their systems.
    /// Gives how many it added.</summary>
    [UnitTitle("Port: Add errands")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortAddErrands : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput count { get; private set; }
        int added;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            count = ValueOutput<int>(nameof(count), flow => added);
            Requirement(port, enter);
            Succession(enter, exit);
            Assignment(enter, count);
        }

        ControlOutput Run(Flow flow)
        {
            added = 0;
            foreach (var e in Wording.Errands(flow.GetValue<Port>(port)?.Def))
            {
                TextPanel.Current?.AddOption(e.key, e.intention, e.label);
                added++;
            }
            return exit;
        }
    }

    /// <summary>Talk to people: the waterfront's news you haven't had (its age, who brought it); prices known here into
    /// your ledger; a fair, if one is on. Gives the facts.</summary>
    [UnitTitle("Port: Talk to people")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortTalk : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = PortLife.Talk(flow.GetValue<Port>(port)?.Def);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            facts = ValueOutput<string>(nameof(facts), f => text);
            Requirement(port, enter);
            Succession(enter, exit);
            Assignment(enter, facts);
        }
    }

    /// <summary>Adds an option for each order posted here ("Take the order: ..."); gives how many.</summary>
    [UnitTitle("Port: Add order options")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortAddCommissions : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput count { get; private set; }
        int added;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                added = 0;
                foreach (var c in Commissions.At(flow.GetValue<Port>(port)?.Def))
                {
                    TextPanel.Current?.AddOption("port.commission.take", "Take the order: " + Commissions.Describe(c));
                    added++;
                }
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            count = ValueOutput<int>(nameof(count), f => added);
            Requirement(port, enter);
            Succession(enter, exit);
            Assignment(enter, count);
        }
    }

    /// <summary>Takes the order behind a choice (options from "Port: Add order options", counted from `first`); gives the
    /// fact, or nothing if the choice wasn't an order.</summary>
    [UnitTitle("Port: Take order")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortTakeCommission : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueInput choice { get; private set; }
        [DoNotSerialize] public ValueInput first { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var open = Commissions.At(flow.GetValue<Port>(port)?.Def);
                int i = flow.GetValue<int>(choice) - flow.GetValue<int>(first);
                if (i >= 0 && i < open.Count)
                    Voyage.Say(Commissions.Take(open[i]));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            choice = ValueInput(nameof(choice), 0);
            first = ValueInput(nameof(first), 0);
            Requirement(port, enter);
            Requirement(choice, enter);
            Requirement(first, enter);
            Succession(enter, exit);
        }
    }

    /// <summary>The place's errand behind a choice on the port menu (the toll house, the moneyer, lodging, alms, the
    /// minster...): does it and gives the facts, its title, and whether it's done in the town's market instead.</summary>
    [UnitTitle("Port: Do errand")]
    [UnitCategory("Kapman/Ports")]
    public sealed class PortDoErrand : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueInput choice { get; private set; }
        [DoNotSerialize] public ValueOutput facts { get; private set; }
        [DoNotSerialize] public ValueOutput title { get; private set; }
        [DoNotSerialize] public ValueOutput toTown { get; private set; }
        string text = "", label = "";
        bool town;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var def = flow.GetValue<Port>(port)?.Def;
                int i = flow.GetValue<int>(choice);
                label = PortLife.ErrandLabel(def, i);
                text = PortLife.Errand(def, PortLife.ErrandKey(def, i), out town);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            choice = ValueInput(nameof(choice), 3);
            facts = ValueOutput<string>(nameof(facts), f => text);
            title = ValueOutput<string>(nameof(title), f => label);
            toTown = ValueOutput<bool>(nameof(toTown), f => town);
            Requirement(port, enter);
            Requirement(choice, enter);
            Succession(enter, exit);
            Assignment(enter, facts);
            Assignment(enter, title);
            Assignment(enter, toTown);
        }
    }

    /// <summary>A saved game loaded: the graphs' shared variables set to match (the port she lies at, not leaving), and
    /// the port's menu opened if she lies in one.</summary>
    static class SaveGameBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            SaveGame.Loaded -= Loaded;
            SaveGame.Loaded += Loaded;
        }

        static void Loaded(string portId)
        {
            var game = KapmanGame.Current;
            if (game == null)
                return;
            var port = Port.All.Find(p => p.id == portId);
            var vars = Variables.Object(game.gameObject);
            vars.Set("Docked port", port);
            vars.Set("Leaving", false);
            if (port != null)
                CustomEvent.Trigger(game.gameObject, "Port menu");
        }
    }

    /// <summary>Saves the game to a slot ("quick" by default); gives what came of it.</summary>
    [UnitTitle("Game: Save")]
    [UnitCategory("Kapman/Game")]
    public sealed class GameSave : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput slot { get; private set; }
        [DoNotSerialize] public ValueOutput fact { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = SaveGame.Save(flow.GetValue<string>(slot));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            slot = ValueInput(nameof(slot), "quick");
            fact = ValueOutput<string>(nameof(fact), f => text);
            Requirement(slot, enter);
            Succession(enter, exit);
            Assignment(enter, fact);
        }
    }

    /// <summary>Loads the game from a slot ("quick" by default); gives what came of it.</summary>
    [UnitTitle("Game: Load")]
    [UnitCategory("Kapman/Game")]
    public sealed class GameLoad : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput slot { get; private set; }
        [DoNotSerialize] public ValueOutput fact { get; private set; }
        string text = "";

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                text = SaveGame.Load(flow.GetValue<string>(slot));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            slot = ValueInput(nameof(slot), "quick");
            fact = ValueOutput<string>(nameof(fact), f => text);
            Requirement(slot, enter);
            Succession(enter, exit);
            Assignment(enter, fact);
        }
    }

    /// <summary>Lays the ship to at her berth (she stops and stays), or lets her go again.</summary>
    [UnitTitle("Ship: Lie to / cast off")]
    [UnitCategory("Kapman/Ship")]
    public sealed class ShipMoor : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput moored { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            moored = ValueInput(nameof(moored), true);
            Requirement(moored, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            var ship = KapmanGame.Current != null ? KapmanGame.Current.ship : null;
            if (ship != null)
                ship.anchored = flow.GetValue<bool>(moored);
            return exit;
        }
    }

    /// <summary>The ship's speed through the water, metres a second (a knot is 0.51).</summary>
    [UnitTitle("Ship: Speed")]
    [UnitCategory("Kapman/Ship")]
    public sealed class ShipSpeed : Unit
    {
        [DoNotSerialize] public ValueOutput speed { get; private set; }

        protected override void Definition()
        {
            speed = ValueOutput<float>(nameof(speed), flow =>
            {
                // (7 Oct 2026: its one use is the test "has she slowed inside a port's reach: arrived". While a talk at
                // sea is up (a ship met, raiders, the king's men) she has not: else the port's menu came up over the
                // talk and both flows took the same choice. She comes in when the talk is over.)
                if (TextPanel.TalkAtSea)
                    return 1000f;
                SailingShip ship = KapmanGame.Current != null ? KapmanGame.Current.ship : null;
                return ship != null ? ship.Speed : 0f;
            });
        }
    }
}
