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
        static VisualElement tree, card, source;
        static Label text;
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
                if (!string.IsNullOrEmpty(TipOf(v)))
                    return v;
            return null;
        }

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
                string tip = TipOf(source);
                if (string.IsNullOrEmpty(tip))
                    return;
                text.text = tip;
                card.style.display = DisplayStyle.Flex;
                card.BringToFront();
                showing = true;
                Place();
            }
            else if (showing)
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
            tree = card = source = null;
            text = null;
            showing = false;
            showAt = -1f;
        }
    }
}
