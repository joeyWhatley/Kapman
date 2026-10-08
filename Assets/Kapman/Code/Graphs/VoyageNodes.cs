using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>A day of the voyage: the crew eat and drink from the stores, are owed their pay, and the sea works on them
    /// (seasick in rough water). Under way or lying in port is worked out from the ship; the sea's height from the wind.</summary>
    [UnitTitle("Voyage: Day")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class VoyageDay : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueOutput atSea { get; private set; }
        bool wasAtSea;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            atSea = ValueOutput<bool>(nameof(atSea), f => wasAtSea);
            Succession(enter, exit);
            Assignment(enter, atSea);
        }

        ControlOutput Run(Flow flow)
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
                return exit;
            var port = Port.Nearest(game.ShipPosition);
            bool inPort = port != null && port.Placed && port.DistanceTo(game.ShipPosition) < port.arrivalRadius && game.ship.anchored;
            wasAtSea = !inPort;
            // the sea's height from the wind (a fully risen sea: 0.025 x wind^2, real m), less within a few km of a port
            var weather = Kapman.Weather.LocalWeather.Active;
            float wind = weather != null ? weather.Now.windSpeed : 5f;
            double wave = Mathf.Min(6f, 0.0246f * wind * wind) * (port != null && port.DistanceTo(game.ShipPosition) < 300f ? 0.3 : 1.0);
            Voyage.Day(!inPort, wave);
            if (inPort)
                Voyage.InPort(Markets.Of(port.Def, game.Year, game.Day));
            return exit;
        }
    }

    /// <summary>In port: the water casks filled, the men paid from the purse, any owed too long go ashore.</summary>
    [UnitTitle("Voyage: In port")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class VoyageInPort : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput port { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Run);
            exit = ControlOutput(nameof(exit));
            port = ValueInput<Port>(nameof(port), null);
            Requirement(port, enter);
            Succession(enter, exit);
        }

        ControlOutput Run(Flow flow)
        {
            var p = flow.GetValue<Port>(port);
            var game = KapmanGame.Current;
            if (p != null && game != null)
            {
                Voyage.InPort(Markets.Of(p.Def, game.Year, game.Day));
                // (Design/fixes/orders.md: an order is handed over by the player, in Errands, part by part: a card may
                // have eased the toll for some days, or the casks may be meant for another order. On arrival she is
                // only told what is aboard for it.)
                string waiting = Commissions.Waiting(p.Def);
                if (waiting.Length > 0)
                    Voyage.Say(waiting);
            }
            return exit;
        }
    }

    /// <summary>The crew and their stores: men, health and heart (0-1), days of food and drink aboard, pay owed (deniers).</summary>
    [UnitTitle("Voyage: Crew")]
    [UnitCategory("Kapman/Voyage")]
    public sealed class VoyageCrew : Unit
    {
        [DoNotSerialize] public ValueOutput men { get; private set; }
        [DoNotSerialize] public ValueOutput health { get; private set; }
        [DoNotSerialize] public ValueOutput morale { get; private set; }
        [DoNotSerialize] public ValueOutput foodDays { get; private set; }
        [DoNotSerialize] public ValueOutput drinkDays { get; private set; }
        [DoNotSerialize] public ValueOutput payOwed { get; private set; }

        protected override void Definition()
        {
            men = ValueOutput<int>(nameof(men), f => Voyage.Men);
            health = ValueOutput<float>(nameof(health), f => (float)Voyage.Health);
            morale = ValueOutput<float>(nameof(morale), f => (float)Voyage.Morale);
            foodDays = ValueOutput<float>(nameof(foodDays), f => (float)Voyage.FoodDays);
            drinkDays = ValueOutput<float>(nameof(drinkDays), f => (float)Voyage.DrinkDays);
            payOwed = ValueOutput<float>(nameof(payOwed), f => (float)Voyage.PayOwed);
        }
    }
}
