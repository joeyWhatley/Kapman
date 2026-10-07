using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE CHART'S OWN TOOLS, at the bottom right by the boat's view (the user, 7 Oct 2026, late: "why is measure up there
    /// and not down on the right bottom region, maybe attached to the map thing / 3d view, and add alt map modes like
    /// borders, religion, culture maps"). A small row standing on the boat's box: Measure (the order it always was: the
    /// rack keeps its name, its tip and its light), and the chart's four readings: Land, Realms, Faith, Peoples
    /// (MapView.SetMode), the one that is on lit.
    /// </summary>
    public partial class FrameRoot
    {
        VisualElement mapTools;
        readonly List<(MapMode mode, Button button)> modeButtons = new List<(MapMode, Button)>();

        partial void MapToolsBuild()
        {
            modeButtons.Clear();
            mapTools = new VisualElement { pickingMode = PickingMode.Ignore };
            mapTools.AddToClassList("frame-maptools");
            var measure = new Button(() => Do(FrameIcon.Kind.Measure));
            measure.AddToClassList("frame-verb");
            measure.AddToClassList("frame-tool");
            measure.Add(new FrameIcon(FrameIcon.Kind.Measure));
            var label = new Label("Measure") { pickingMode = PickingMode.Ignore };
            label.AddToClassList("frame-verb-name");
            measure.Add(label);
            mapTools.Add(measure);
            rack.Add(new Verb { button = measure, label = label, kind = FrameIcon.Kind.Measure });
            foreach (var (mode, name, tip) in new[]
            {
                (MapMode.Land, "Land", "The chart as it is drawn."), (MapMode.Realms, "Realms", "Whose each port is."),
                (MapMode.Faith, "Faith", "The faith of each port."), (MapMode.Peoples, "Peoples", "The people of each port."),
            })
            {
                var m = mode;
                var b = FrameUi.Btn(name, () => MapView.Current?.SetMode(m), tip);
                b.AddToClassList("frame-tool");
                mapTools.Add(b);
                modeButtons.Add((mode, b));
            }
            root.Add(mapTools);
        }

        partial void MapToolsUpdate()
        {
            var map = MapView.Current;
            foreach (var (mode, button) in modeButtons)
                button.EnableInClassList("on", map != null && map.Mode == mode);
        }
    }
}
