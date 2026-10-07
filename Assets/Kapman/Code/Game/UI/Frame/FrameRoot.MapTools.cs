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
            // (they stand in the layout file, Frame.uxml, under "maptools")
            mapTools = Part<VisualElement>("maptools");
            var measure = Part<Button>("tool-measure");
            measure.clicked += () => Do(FrameIcon.Kind.Measure);
            var label = measure.Q<Label>(className: "frame-verb-name");
            if (label == null)
            {
                label = new Label { pickingMode = PickingMode.Ignore };
                label.AddToClassList("frame-verb-name");
                measure.Add(label);
            }
            rack.Add(new Verb { button = measure, label = label, kind = FrameIcon.Kind.Measure });
            foreach (var (mode, partName) in new[]
            {
                (MapMode.Land, "mode-land"), (MapMode.Realms, "mode-realms"), (MapMode.Faith, "mode-faith"), (MapMode.Peoples, "mode-peoples"),
            })
            {
                var m = mode;
                var b = Part<Button>(partName);
                b.clicked += () => MapView.Current?.SetMode(m);
                modeButtons.Add((mode, b));
            }
        }

        partial void MapToolsUpdate()
        {
            var map = MapView.Current;
            foreach (var (mode, button) in modeButtons)
                button.EnableInClassList("on", map != null && map.Mode == mode);
        }
    }
}
