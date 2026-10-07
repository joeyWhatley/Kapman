using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// NOTICES - plain facts as they happen ("The water casks are empty.", "The men are paid 3 d."), a line or two at the
    /// top of the screen in the panels' look (black glass, a hard black line, square corners), each fading after a few
    /// seconds. The voyage tells it (Voyage.Notice); graphs can too (Notice: Show). No story: your writing goes elsewhere.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class Notices : MonoBehaviour
    {
        [Tooltip("The notices' font (Gentium Book Plus).")]
        public Font font;
        [Tooltip("Seconds a notice stays.")]
        public float stay = 7f;
        [Tooltip("The most notices on screen at once.")]
        public int most = 3;

        public static Notices Current { get; private set; }

        VisualElement root;
        readonly List<(Label label, float until)> shown = new List<(Label, float)>();

        void Awake() => Current = this;

        void OnEnable()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            root?.RemoveFromHierarchy();
            docRoot.style.position = Position.Absolute;
            docRoot.style.left = 0;
            docRoot.style.right = 0;
            docRoot.style.top = 0;
            docRoot.style.bottom = 0;
            docRoot.pickingMode = PickingMode.Ignore;
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.style.position = Position.Absolute;
            root.style.top = 14;
            root.style.left = 0;
            root.style.right = 0;
            root.style.alignItems = Align.Center;
            docRoot.Add(root);
            Voyage.Notice += Show;
        }

        void OnDisable() => Voyage.Notice -= Show;

        /// <summary>Shows a fact.</summary>
        public void Show(string text)
        {
            if (string.IsNullOrEmpty(text) || root == null)
                return;
            foreach (var s in shown)
                if (s.label.text == text)
                    return;                                          // (once is enough)
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.style.backgroundColor = new Color(8f / 255f, 9f / 255f, 11f / 255f, 0.6f);          // (the glass: see-through, as every panel)
            l.style.textShadow = new TextShadow { offset = new Vector2(0f, 1f), blurRadius = 3f, color = new Color(0f, 0f, 0f, 0.9f) };
            l.style.borderTopWidth = l.style.borderBottomWidth = l.style.borderLeftWidth = l.style.borderRightWidth = 1;
            l.style.borderTopColor = l.style.borderBottomColor = l.style.borderLeftColor = l.style.borderRightColor = Color.black;
            l.style.color = new Color(232f / 255f, 231f / 255f, 226f / 255f);
            l.style.fontSize = 14;
            l.style.paddingLeft = l.style.paddingRight = 12;
            l.style.paddingTop = l.style.paddingBottom = 5;
            l.style.marginBottom = 4;
            if (font != null)
                l.style.unityFontDefinition = FontDefinition.FromFont(font);
            root.Add(l);
            shown.Add((l, Time.unscaledTime + stay));
            while (shown.Count > most)
            {
                shown[0].label.RemoveFromHierarchy();
                shown.RemoveAt(0);
            }
        }

        void Update()
        {
            // at the map, under its top bar
            if (root != null)
                root.style.top = MapView.Current != null && MapView.Current.IsOpen ? 62 : 14;
            for (int i = shown.Count - 1; i >= 0; i--)
            {
                float left = shown[i].until - Time.unscaledTime;
                shown[i].label.style.opacity = Mathf.Clamp01(left / 1.5f);
                if (left <= 0f)
                {
                    shown[i].label.RemoveFromHierarchy();
                    shown.RemoveAt(i);
                }
            }
        }
    }
}
