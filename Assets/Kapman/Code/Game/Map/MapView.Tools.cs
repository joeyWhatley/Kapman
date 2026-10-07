using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE MAP'S TOOLS (Plans/map_play.md: hands on the map) - the merchant's instruments at the table:
    ///   - DIVIDERS [D]: press and drag from one place to another: how far it is, and about how many days' sail;
    ///   - PINS [P]: click to pin a note to the map (it opens in the chest to be written);
    ///   - STRING [S]: click one pinned note, then another, to tie a string between them (again: untie it) - a fleet's
    ///     track, a line of reasoning, the way an old text sails.
    /// While a tool is in hand, the left button works it and the right button moves the map. [Esc] puts it down.
    /// </summary>
    public partial class MapView
    {
        enum Tool { None, Dividers, Pin, String }

        Tool tool;
        ToolLayer toolLayer;
        Label measureLabel, toolHint;
        Button toolDividers, toolPin, toolString;
        bool measuring, haveMeasure;
        double mAx, mAz, mBx, mBz;
        int stringFrom = -1;
        bool rightDragging;
        // laying out an old text's sailing directions: the card of its legs, and what the next pin is for
        VisualElement layoutCard;
        string layoutText, pinLink, pinLabel;

        /// <summary>Lays out an old text's sailing directions on the map: a card of its legs; pin the start, then where each
        /// leg ends (the dividers measure the days); each pin is strung to the one before. Sailing near a leg's true end finds
        /// it (Chest.CheckVoyages), and says how near your pin was.</summary>
        public void LayOut(string textId)
        {
            var t = ChestData.Text(textId);
            if (t == null || !IsOpen)
                return;
            layoutText = textId;
            layoutCard?.RemoveFromHierarchy();
            layoutCard = new VisualElement();
            layoutCard.AddToClassList("map-layout-card");
            var head = new Label(t.title.Split('(')[0].Trim());
            head.AddToClassList("map-title");
            head.style.fontSize = 16;
            layoutCard.Add(head);
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("map-layout-legs");
            layoutCard.Add(scroll);
            var item = Chest.Items.FirstOrDefault(i => i.def == textId);
            bool eng = item != null && Chest.CanRead(item);
            var legs = ChestData.LegsOf(textId);
            if (legs.Count > 0)
                scroll.Add(LayoutRow("Start: " + legs[0].fromName, "legstart:" + textId, $"{t.title.Split('(')[0].Trim()}: the start"));
            foreach (var leg in legs)
            {
                string words = eng && !string.IsNullOrEmpty(leg.translation) ? leg.translation : leg.original;
                scroll.Add(LayoutRow($"{leg.order}. {words}\n{leg.bearing} · {leg.days}" + (Chest.Found(leg) ? $" · found: {leg.toName}" : ""),
                                     "leg:" + leg.Key, $"{t.title.Split('(')[0].Trim()}, leg {leg.order}: its end"));
            }
            var shut = new Button(() => { layoutCard.RemoveFromHierarchy(); layoutCard = null; layoutText = null; pinLink = null; SetTool(Tool.None); }) { text = "Done" };
            shut.AddToClassList("map-button");
            layoutCard.Add(shut);
            root.Add(layoutCard);
            SetTool(Tool.Dividers);
        }

        /// <summary>Puts the pin in hand for a hoard's dig mark (the next click sets it).</summary>
        public void MarkDig(string hoardId)
        {
            if (!IsOpen || Hoards.Get(hoardId) == null)
                return;
            layoutText = null;
            SetTool(Tool.Pin);
            pinLink = "dig:" + hoardId;
            pinLabel = "Dig here";
            toolHint.text = "Click where to dig. Close in on the clue's landmarks; lay the string and paces first. [Esc] puts the pin down.";
        }

        VisualElement LayoutRow(string text, string link, string label)
        {
            var row = new VisualElement();
            row.AddToClassList("map-layout-row");
            var words = new Label(text);
            words.AddToClassList("map-layout-words");
            row.Add(words);
            bool pinned = Chest.Notes.Any(n => n.links.Contains(link) && !double.IsNaN(n.x));
            var pin = new Button(() => { SetTool(Tool.Pin); pinLink = link; pinLabel = label; toolHint.text = "Click where it ends (or starts). [Esc] puts the pin down."; })
                      { text = pinned ? "Pinned (pin again)" : "Pin it" };
            pin.AddToClassList("map-button");
            row.Add(pin);
            return row;
        }

        void BuildTools(VisualElement foot)
        {
            toolDividers = ToolButton(foot, "Dividers (D)", Tool.Dividers, "Measure: press and drag from one place to another.");
            toolPin = ToolButton(foot, "Pin (P)", Tool.Pin, "Pin a note to the map: click where.");
            toolString = ToolButton(foot, "String (S)", Tool.String, "Tie a string between two pinned notes: click one, then the other.");
            toolLayer = new ToolLayer(this) { pickingMode = PickingMode.Ignore };
            toolLayer.style.position = Position.Absolute;
            toolLayer.style.left = toolLayer.style.top = toolLayer.style.right = toolLayer.style.bottom = 0;
            overlays.Add(toolLayer);
            measureLabel = new Label { pickingMode = PickingMode.Ignore };
            measureLabel.AddToClassList("map-measure");
            measureLabel.style.display = DisplayStyle.None;
            overlays.Add(measureLabel);
            toolHint = new Label { pickingMode = PickingMode.Ignore };
            toolHint.AddToClassList("map-tool-hint");
            toolHint.style.display = DisplayStyle.None;
            root.Add(toolHint);
        }

        Button ToolButton(VisualElement foot, string text, Tool which, string tip)
        {
            var b = new Button(() => SetTool(tool == which ? Tool.None : which)) { text = text, tooltip = tip };
            b.AddToClassList("map-button");
            b.AddToClassList("map-foot-button");
            foot.Add(b);
            return b;
        }

        void SetTool(Tool t)
        {
            tool = t;
            stringFrom = -1;
            measuring = false;
            if (t != Tool.Dividers)
                haveMeasure = false;
            toolDividers.EnableInClassList("on", t == Tool.Dividers);
            toolPin.EnableInClassList("on", t == Tool.Pin);
            toolString.EnableInClassList("on", t == Tool.String);
            toolHint.text = t == Tool.Dividers ? "Dividers: press and drag to measure. Right button moves the map. [Esc] puts them down."
                          : t == Tool.Pin ? "Pin: click where to pin a note. [Esc] puts it down."
                          : t == Tool.String ? "String: click a pinned note, then another. [Esc] puts it down." : "";
            toolHint.style.display = t == Tool.None ? DisplayStyle.None : DisplayStyle.Flex;
            toolLayer?.MarkDirtyRepaint();
        }

        /// <summary>The tools' keys; true if [Esc] was used to put a tool down (so the map stays open).</summary>
        bool ToolKeys(Keyboard kb)
        {
            // (with the frame up, [D] is the frame's measure and [Esc] comes through PutToolDown)
            if (!Framed && kb.dKey.wasPressedThisFrame) SetTool(tool == Tool.Dividers ? Tool.None : Tool.Dividers);
            else if (kb.pKey.wasPressedThisFrame) SetTool(tool == Tool.Pin ? Tool.None : Tool.Pin);
            else if (kb.sKey.wasPressedThisFrame) SetTool(tool == Tool.String ? Tool.None : Tool.String);
            else if (!Framed && kb.escapeKey.wasPressedThisFrame && (tool != Tool.None || haveMeasure))
            {
                PutToolDown();
                return true;
            }
            return false;
        }

        /// <summary>Puts the tool in hand down and clears the dividers' measure ([Esc]; the frame calls it for its own Esc).</summary>
        public void PutToolDown()
        {
            pinLink = null;
            SetTool(Tool.None);
            haveMeasure = false;
            measureLabel.style.display = DisplayStyle.None;
        }

        (double x, double z) PxToWorld(Vector2 local) => (drawWest + local.x * mpp, drawNorth - local.y * mpp);
        Vector2 WorldToPx(double x, double z) => new Vector2((float)((x - drawWest) / mpp), (float)((drawNorth - z) / mpp));

        /// <summary>A pointer pressed on the map with a tool in hand: true if the tool took it.</summary>
        bool ToolDown(PointerDownEvent e)
        {
            if (tool == Tool.None)
                return false;
            if (e.button == 1)
            {
                rightDragging = true;                                      // (the right button moves the map)
                return false;
            }
            if (e.button != 0)
                return false;
            var local = layer.WorldToLocal(e.position);
            var (x, z) = PxToWorld(local);
            switch (tool)
            {
                case Tool.Dividers:
                    measuring = haveMeasure = true;
                    mAx = mBx = x;
                    mAz = mBz = z;
                    layer.CapturePointer(e.pointerId);
                    break;
                case Tool.Pin:
                    if (pinLink != null)
                    {
                        // a pin for a leg of the text being laid out: joined to it, strung to the pin before
                        foreach (var old in Chest.Notes.Where(o => o.links.Contains(pinLink)).ToList())
                            Chest.DeleteNote(old);
                        var leg = layoutText != null ? Chest.AddNote(pinLabel ?? "", pinLink, "text:" + layoutText) : Chest.AddNote(pinLabel ?? "", pinLink);
                        Chest.Pin(leg, x, z);
                        var before = PreviousLegPin(pinLink);
                        if (before != null)
                            Chest.Thread(before.id, leg.id);
                        bool digMark = pinLink.StartsWith("dig:");
                        pinLink = null;
                        SetTool(digMark ? Tool.None : Tool.Dividers);
                        if (digMark)
                            table?.OpenChest("hoards");
                        if (layoutText != null)
                            LayOut(layoutText);                           // (the card shows it pinned)
                        break;
                    }
                    var n = Chest.AddNote("");
                    Chest.Pin(n, x, z);
                    table?.OpenChest("mine");
                    break;
                case Tool.String:
                    var pin = PinNear(local);
                    if (pin == null)
                        break;
                    if (stringFrom < 0)
                        stringFrom = pin.id;
                    else
                    {
                        Chest.Thread(stringFrom, pin.id);
                        stringFrom = -1;
                    }
                    break;
            }
            toolLayer.MarkDirtyRepaint();
            e.StopPropagation();
            return true;
        }

        bool ToolMove(PointerMoveEvent e)
        {
            if (!measuring)
                return false;
            var (x, z) = PxToWorld(layer.WorldToLocal(e.position));
            mBx = x;
            mBz = z;
            toolLayer.MarkDirtyRepaint();
            return true;
        }

        void ToolUp(PointerUpEvent e)
        {
            rightDragging = false;
            if (!measuring)
                return;
            measuring = false;
            if (layer.HasPointerCapture(e.pointerId))
                layer.ReleasePointer(e.pointerId);
        }

        /// <summary>The pin of the leg before (or the start), for stringing them in order.</summary>
        PlayerNote PreviousLegPin(string link)
        {
            if (link == null || !link.StartsWith("leg:"))
                return null;
            string key = link.Substring(4);
            int c = key.LastIndexOf(':');
            if (c < 0 || !int.TryParse(key.Substring(c + 1), out int order))
                return null;
            string text = key.Substring(0, c);
            string prev = order <= 1 ? "legstart:" + text : $"leg:{text}:{order - 1}";
            return Chest.Notes.FirstOrDefault(n => n.links.Contains(prev) && !double.IsNaN(n.x));
        }

        /// <summary>The pinned note nearest the point, within 12 px.</summary>
        PlayerNote PinNear(Vector2 local)
        {
            PlayerNote best = null;
            float bestD = 12f;
            foreach (var n in Chest.Notes)
            {
                if (double.IsNaN(n.x))
                    continue;
                float d = (WorldToPx(n.x, n.z) - local).magnitude;
                if (d < bestD)
                {
                    bestD = d;
                    best = n;
                }
            }
            return best;
        }

        /// <summary>Each frame: the measurement's words follow the dividers.</summary>
        void ToolsDraw()
        {
            if (toolLayer == null)
                return;
            toolLayer.MarkDirtyRepaint();
            if (!haveMeasure)
            {
                measureLabel.style.display = DisplayStyle.None;
                return;
            }
            double km = Math.Sqrt((mBx - mAx) * (mBx - mAx) + (mBz - mAz) * (mBz - mAz)) / 1000.0;
            var b = WorldToPx(mBx, mBz);
            measureLabel.text = km < 0.05 ? "" : $"{km:0.#} km · about {Course.Days(km):0.#} days' sail";
            measureLabel.style.display = km < 0.05 ? DisplayStyle.None : DisplayStyle.Flex;
            measureLabel.style.left = b.x + 10f;
            measureLabel.style.top = b.y + 6f;
        }

        /// <summary>Draws the dividers' line and the strings between pinned notes.</summary>
        sealed class ToolLayer : VisualElement
        {
            readonly MapView map;

            public ToolLayer(MapView map)
            {
                this.map = map;
                generateVisualContent += Paint;
            }

            void Paint(MeshGenerationContext ctx)
            {
                if (map.mpp <= 0.0)
                    return;
                var p = ctx.painter2D;
                // the strings
                p.strokeColor = new Color(0.55f, 0.16f, 0.12f, 0.9f);
                p.lineWidth = 1.6f;
                foreach (var (a, b) in Chest.Threads)
                {
                    var na = Chest.Note(a);
                    var nb = Chest.Note(b);
                    if (na == null || nb == null || double.IsNaN(na.x) || double.IsNaN(nb.x))
                        continue;
                    p.BeginPath();
                    p.MoveTo(map.WorldToPx(na.x, na.z));
                    p.LineTo(map.WorldToPx(nb.x, nb.z));
                    p.Stroke();
                }
                // a string being tied: a ring round the first pin
                if (map.stringFrom >= 0)
                {
                    var n = Chest.Note(map.stringFrom);
                    if (n != null && !double.IsNaN(n.x))
                    {
                        p.BeginPath();
                        p.Arc(map.WorldToPx(n.x, n.z), 9f, 0f, 360f);
                        p.Stroke();
                    }
                }
                // the dividers: two points and the line between
                if (map.haveMeasure)
                {
                    var a = map.WorldToPx(map.mAx, map.mAz);
                    var b = map.WorldToPx(map.mBx, map.mBz);
                    p.strokeColor = new Color(0.07f, 0.07f, 0.09f, 0.9f);
                    p.lineWidth = 1.5f;
                    p.BeginPath();
                    p.MoveTo(a);
                    p.LineTo(b);
                    p.Stroke();
                    foreach (var end in new[] { a, b })
                    {
                        p.BeginPath();
                        p.Arc(end, 4f, 0f, 360f);
                        p.Stroke();
                    }
                }
            }
        }
    }
}
