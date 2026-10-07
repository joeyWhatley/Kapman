using Kapman.Game;
using Kapman.Game.Sim;
using Unity.VisualScripting;

namespace Kapman.Graphs
{
    /// <summary>Cuts to an event shot of a ship of the world (by her number, as "On ship made out" gives it): a long lens
    /// at her (sighting), the two ships side by side (two-shot), or looking back past your boat at her (chase), for so many
    /// seconds; then the camera blends back to your ship. The meetings at sea call their own shots; this is for yours.</summary>
    [UnitTitle("Camera: Event shot")]
    [UnitCategory("Kapman/Camera")]
    public sealed class CameraEventShot : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput shot { get; private set; }
        [DoNotSerialize] public ValueInput ship { get; private set; }
        [DoNotSerialize] public ValueInput seconds { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var world = KapmanGame.Current != null ? KapmanGame.Current.World : null;
                int i = flow.GetValue<int>(ship);
                if (ShotDirector.Current != null && world != null && i >= 0 && i < world.ships.Count)
                    ShotDirector.Current.Play(flow.GetValue<ShotDirector.Shot>(shot), world.ships[i], flow.GetValue<float>(seconds));
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            shot = ValueInput(nameof(shot), ShotDirector.Shot.Sighting);
            ship = ValueInput(nameof(ship), -1);
            seconds = ValueInput(nameof(seconds), 6f);
            Requirement(shot, enter);
            Requirement(ship, enter);
            Requirement(seconds, enter);
            Succession(enter, exit);
        }
    }

    /// <summary>Ends the event shot on now: the camera blends back to your ship.</summary>
    [UnitTitle("Camera: Back to the ship")]
    [UnitCategory("Kapman/Camera")]
    public sealed class CameraBackToShip : Unit
    {
        [DoNotSerialize, PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize, PortLabelHidden] public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                ShotDirector.Current?.Back();
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
        }
    }
}
