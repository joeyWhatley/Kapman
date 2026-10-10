using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>Something that gives its own hover card (worked out when shown: a price's explanation, a fact's age).</summary>
    public interface IHoverTip
    {
        string Tip();
    }

    /// <summary>
    /// HOVER CARDS - the small black-glass cards that come up when the pointer rests on something: what it is, where a
    /// figure comes from. Unity's own tooltips only show in the editor, so the game draws its own: any element with a
    /// tooltip set, or whose userData gives one (<see cref="IHoverTip"/>), gets a card after a moment's rest, beside the
    /// pointer and kept on the screen. One card for every panel (all the game's documents share one panel). It never
    /// takes the clicks.
    /// </summary>
    public static class HoverCards
    {
        /// <summary>Seconds the pointer rests before the card comes.</summary>
        public static float Delay = 0.35f;

        static IPanel attached;
        static PanelSettings settings;
        static string floorClass;                                     // the floor under the text now on the card (TextFloor)
        static VisualElement tree, card, source, box;
        static Label text;
        static LogBox boxShown;
        static Vector2 pointer;
        static float showAt = -1f;
        static bool showing;

        /// <summary>Hooks the panel an element is on (once), with the stylesheet that holds the card's look.</summary>
        public static void Attach(VisualElement onPanel, StyleSheet look, PanelSettings panelSettings = null)
        {
            var panel = onPanel?.panel;
            if (panel == null || panel == attached)
                return;
            attached = panel;
            settings = panelSettings;
            floorClass = null;
            tree = panel.visualTree;
            card = new VisualElement { pickingMode = PickingMode.Ignore };
            card.AddToClassList("hover-card");
            if (look != null)
                card.styleSheets.Add(look);
            text = new Label { enableRichText = true, pickingMode = PickingMode.Ignore };
            text.AddToClassList("hover-card-text");
            card.Add(text);
            // (10 Oct 2026: a line of the log gives a box of rows in place of words)
            box = new VisualElement { pickingMode = PickingMode.Ignore };
            box.AddToClassList("hover-box");
            box.style.display = DisplayStyle.None;
            card.Add(box);
            card.style.position = Position.Absolute;
            card.style.display = DisplayStyle.None;
            tree.Add(card);
            tree.RegisterCallback<PointerMoveEvent>(OnMove, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerLeaveEvent>(_ => Hide(), TrickleDown.TrickleDown);
            card.RegisterCallback<GeometryChangedEvent>(_ => Place());
            card.schedule.Execute(Tick).Every(50);
        }

        static void OnMove(PointerMoveEvent e)
        {
            pointer = e.position;
            var found = FindTip(e.target as VisualElement);
            if (found != source)
            {
                source = found;
                Hide();
                showAt = found != null ? Time.realtimeSinceStartup + Delay : -1f;
            }
            else if (showing)
                Place();
        }

        /// <summary>The element under the pointer, or the nearest above it, that has a card to give.</summary>
        static VisualElement FindTip(VisualElement v)
        {
            for (; v != null && v != tree; v = v.parent)
                if (BoxOf(v) != null || !string.IsNullOrEmpty(TipOf(v)))
                    return v;
            return null;
        }

        /// <summary>The box an element gives for its card (a line of the log), or null.</summary>
        static LogBox BoxOf(VisualElement v)
        {
            var b = v.userData is IHoverBox h ? h.Box() : null;
            return b != null && !b.Empty ? b : null;
        }

        /// <summary>The card's rows for a box: its heading, then a copy of the row's part for each row (laid out in
        /// Game/UI/Parts/HoverBox.uxml).</summary>
        static void Fill(LogBox b)
        {
            box.Clear();
            var parts = Kapman.Game.Frame.FrameParts.Current;
            var layout = parts != null ? parts.hoverBox : null;
            if (b.head.Length > 0)
            {
                var head = Kapman.Game.Frame.FrameParts.Make<Label>(layout, "head");
                head.pickingMode = PickingMode.Ignore;
                head.AddToClassList("hover-box-head");
                head.text = b.head;
                box.Add(head);
            }
            foreach (var r in b.rows)
            {
                var row = Kapman.Game.Frame.FrameParts.Make<VisualElement>(layout, "box-row");
                row.pickingMode = PickingMode.Ignore;
                row.AddToClassList("hover-box-row");
                row.EnableInClassList("sub", r.sub);
                row.EnableInClassList("words", r.words);
                var label = Kapman.Game.Frame.FrameParts.In<Label>(row, "label");
                var value = Kapman.Game.Frame.FrameParts.In<Label>(row, "value");
                var change = Kapman.Game.Frame.FrameParts.In<Label>(row, "change");
                label.AddToClassList("hover-box-label");
                value.AddToClassList("hover-box-value");
                change.AddToClassList("hover-box-change");
                label.text = r.label;
                if (r.words || r.value.Length == 0)
                    value.RemoveFromHierarchy();
                else
                    value.text = r.value;
                if (r.words || r.change.Length == 0)
                    change.RemoveFromHierarchy();
                else
                {
                    change.text = r.change;
                    change.EnableInClassList("plus", r.change.StartsWith("+"));
                    change.EnableInClassList("minus", r.change.StartsWith("-"));
                }
                box.Add(row);
            }
            boxShown = b;
        }

        /// <summary>What the card shows now, as plain text (a test reads it): the box's rows, or the words.</summary>
        public static string Shown => !showing ? "" : boxShown != null ? boxShown.Text() : text != null ? text.text : "";

        static string TipOf(VisualElement v) => v.userData is IHoverTip t ? t.Tip() : v.tooltip;

        static void Tick()
        {
            TextFloor.Keep(card, settings, ref floorClass);               // (no text under 18 true pixels, here too)
            if (source == null)
            {
                Hide();
                return;
            }
            // gone or hidden under it: no card
            if (source.panel != attached || source.resolvedStyle.display == DisplayStyle.None || source.resolvedStyle.visibility == Visibility.Hidden)
            {
                source = null;
                Hide();
                return;
            }
            if (!showing && showAt > 0f && Time.realtimeSinceStartup >= showAt)
            {
                var b = BoxOf(source);
                if (b != null)
                {
                    Fill(b);
                    text.style.display = DisplayStyle.None;
                    box.style.display = DisplayStyle.Flex;
                }
                else
                {
                    string tip = TipOf(source);
                    if (string.IsNullOrEmpty(tip))
                        return;
                    boxShown = null;
                    text.text = tip;
                    text.style.display = DisplayStyle.Flex;
                    box.style.display = DisplayStyle.None;
                }
                card.style.display = DisplayStyle.Flex;
                card.BringToFront();
                showing = true;
                Place();
            }
            else if (showing && boxShown == null)
            {
                string tip = TipOf(source);                                  // (a figure that changes while shown)
                if (tip != text.text && !string.IsNullOrEmpty(tip))
                    text.text = tip;
            }
        }

        /// <summary>Beside the pointer, flipped to the other side where it would leave the screen.</summary>
        static void Place()
        {
            if (!showing || tree == null)
                return;
            float w = card.resolvedStyle.width, h = card.resolvedStyle.height, W = tree.resolvedStyle.width, H = tree.resolvedStyle.height;
            if (float.IsNaN(w)) w = 0f;
            if (float.IsNaN(h)) h = 0f;
            float x = pointer.x + 16f, y = pointer.y + 20f;
            if (x + w > W - 8f) x = Mathf.Max(8f, pointer.x - w - 12f);
            if (y + h > H - 8f) y = Mathf.Max(8f, pointer.y - h - 12f);
            card.style.left = x;
            card.style.top = y;
        }

        static void Hide()
        {
            showing = false;
            if (card != null)
                card.style.display = DisplayStyle.None;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            attached = null;
            settings = null;
            floorClass = null;
            tree = card = source = box = null;
            text = null;
            boxShown = null;
            showing = false;
            showAt = -1f;
        }
    }
}
