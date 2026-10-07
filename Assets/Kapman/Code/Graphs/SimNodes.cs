using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;
using Unity.VisualScripting;

namespace Kapman.Graphs
{
    /// <summary>Moves the world's other ships on by one game hour: they sail, arrive, deal and choose where next
    /// (the world simulation: Code/Game/Sim). Wire it to "On game hour".</summary>
    [UnitTitle("Sim: Hour")]
    [UnitCategory("Kapman/Sim")]
    public sealed class SimHour : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                KapmanGame.Current?.World?.Hour();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }

    /// <summary>The world's ships: how many there are, how many are at sea, and how many lie in a port.</summary>
    [UnitTitle("Sim: Ships")]
    [UnitCategory("Kapman/Sim")]
    public sealed class SimShips : Unit
    {
        [DoNotSerialize] public ValueInput port { get; private set; }
        [DoNotSerialize] public ValueOutput all { get; private set; }
        [DoNotSerialize] public ValueOutput atSea { get; private set; }
        [DoNotSerialize] public ValueOutput inPort { get; private set; }

        protected override void Definition()
        {
            port = ValueInput<string>(nameof(port), "");
            all = ValueOutput<int>(nameof(all), f => World?.ships.Count ?? 0);
            atSea = ValueOutput<int>(nameof(atSea), f => World?.ships.Count(s => s.state == ShipState.Sailing) ?? 0);
            inPort = ValueOutput<int>(nameof(inPort), f =>
            {
                var p = GameData.Port(f.GetValue<string>(port));
                return p == null ? 0 : World?.ships.Count(s => s.state != ShipState.Sailing && s.state != ShipState.Lost && s.at == p.index) ?? 0;
            });
        }

        static WorldSim World => KapmanGame.Current?.World;
    }
}
