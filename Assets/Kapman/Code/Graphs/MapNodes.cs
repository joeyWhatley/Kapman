using System.Collections;
using Kapman.Game;
using Unity.VisualScripting;

namespace Kapman.Graphs
{
    /// <summary>Rises from the ship into the map over a town (its market docked beside it), and waits till the
    /// player is back down at the ship.</summary>
    [UnitTitle("Town: Explore")]
    [UnitCategory("Kapman/Map")]
    public sealed class TownExplore : WaitUnit
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
            var map = MapView.Current;
            if (map != null)
            {
                map.ExploreTown(flow.GetValue<Port>(port));
                yield return null;
                while (map.IsOpen)
                    yield return null;
            }
            yield return exit;
        }
    }

    /// <summary>Whether the map view is open (or on its way in or out).</summary>
    [UnitTitle("Map: Is open")]
    [UnitCategory("Kapman/Map")]
    public sealed class MapIsOpen : Unit
    {
        [DoNotSerialize] public ValueOutput open { get; private set; }

        protected override void Definition() =>
            open = ValueOutput<bool>(nameof(open), flow => MapView.Current != null && MapView.Current.IsOpen);
    }
}
