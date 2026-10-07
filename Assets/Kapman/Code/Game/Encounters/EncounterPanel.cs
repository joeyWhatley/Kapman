using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Game
{
    /// <summary>
    /// THE CARD ON SCREEN (npc_text_encounters.md 3.1 and 4.3, night one): the encounter EncounterRunner's director has
    /// open, drawn plainly with IMGUI in the user's look: black see-through glass, a thin hard black border, square
    /// corners, plain text (Gentium when the font is set). Top to bottom: the title, who is speaking with how they feel
    /// toward you as a word, the lines (the user's words, or [write: key] until written), what this step is, the options
    /// 1 to 9 (a greyed one shows its rule under it, a chance shows its odds as a word, the one-key choice on a return is
    /// marked [Enter]), and after the choice what came of it. Keys: 1-9 choose, Enter takes the marked choice and, after a
    /// choice, goes on; Esc is refused on an arrival card (the toll man needs an answer). The styled UI Toolkit panel of
    /// 4.3 is night two. While a card is open it holds the player's attention (UiFocus), and a click on it does not send
    /// the ship off (ShipMotion reads the mouse itself: the order a click on the card gave is taken back).
    /// </summary>
    [DefaultExecutionOrder(-60)]
    public class EncounterPanel : MonoBehaviour
    {
        [Tooltip("The card's font (Assets/Kapman/Game/UI/Fonts/GentiumBookPlus-Regular.ttf; empty: Unity's own).")]
        public Font font;
        [Tooltip("The card's width at 1440 lines (px); it scales with the screen's height.")]
        public float width = 560f;

        /// <summary>Where the card is on screen (IMGUI pixels, y down), and whether it is up.</summary>
        public static Rect Area { get; private set; }
        public static bool Showing { get; private set; }

        /// <summary>Is a point (IMGUI pixels, y down) on the card?</summary>
        public static bool Covers(Vector2 guiPoint) => Showing && Area.Contains(guiPoint);

        bool holding;
        string hint = "";
        float hintUntil;

        // a click on the card: the ship's order before it, taken back after ShipMotion has read the click
        bool clickOnCard;
        Vector3? goalBefore;
        string statusBefore = "";

        GUIStyle title, body, dim, faint;
        Texture2D glass, black;
        float styledFor = -1f;
        Font styledFont;
        readonly List<(string text, GUIStyle style, float gap)> rows = new List<(string, GUIStyle, float)>();

        static readonly Color Ink = new Color(232f / 255f, 231f / 255f, 226f / 255f);

        void Update()
        {
            var d = EncounterRunner.Director;
            var s = d != null ? d.Current : null;
            bool open = s != null;
            if (open != holding)
            {
                if (open)
                    UiFocus.Take(this, false);
                else
                    UiFocus.Release(this);
                holding = open;
                hint = "";
            }
            Showing = open;
            if (!open)
            {
                Area = Rect.zero;
                return;
            }
            NoteClick();
            var kb = Keyboard.current;
            if (kb == null)
                return;
            bool enter = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
            if (s.Chosen)
            {
                if (enter || kb.escapeKey.wasPressedThisFrame)
                    d.Continue();
                return;
            }
            for (int i = 0; i < 9; i++)
                if (Pressed(kb, i))
                {
                    Said(d.Choose(i));
                    return;
                }
            if (enter)
            {
                if (s.DefaultIndex >= 0)
                    Said(d.ChooseDefault());
                return;
            }
            if (kb.escapeKey.wasPressedThisFrame && !d.Dismiss())
                Hint("Choose one first.");                             // (placeholder wording: the user's to change)
        }

        void LateUpdate()
        {
            if (!clickOnCard)
                return;
            clickOnCard = false;
            var ship = ShipMotion.Player;
            if (ship == null)
                return;
            if (goalBefore.HasValue)
            {
                if (!ship.Goal.HasValue || (ship.Goal.Value - goalBefore.Value).sqrMagnitude > 0.01f)
                    ship.MakeFor(goalBefore.Value);
            }
            else if (ship.Goal.HasValue)
                ship.EndOrder(statusBefore);
        }

        void OnDisable()
        {
            if (holding)
                UiFocus.Release(this);
            holding = false;
            Showing = false;
            Area = Rect.zero;
        }

        void OnDestroy()
        {
            if (glass != null)
                Destroy(glass);
            if (black != null)
                Destroy(black);
        }

        /// <summary>A left click on the card this frame: the ship's order as it was, to put back once ShipMotion has read it.</summary>
        void NoteClick()
        {
            var mouse = Mouse.current;
            var ship = ShipMotion.Player;
            if (mouse == null || ship == null || !mouse.leftButton.wasPressedThisFrame)
                return;
            Vector2 p = mouse.position.ReadValue();
            p.y = Screen.height - p.y;
            if (!Area.Contains(p))
                return;
            clickOnCard = true;
            goalBefore = ship.Goal;
            statusBefore = ship.Status;
        }

        void Said(ChoiceResult r)
        {
            if (r != null && !r.ok)
                Hint(r.Text);
        }

        void Hint(string text)
        {
            hint = text ?? "";
            hintUntil = Time.unscaledTime + 4f;
        }

        static bool Pressed(Keyboard kb, int i)
        {
            switch (i)
            {
                case 0: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                case 1: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                case 2: return kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame;
                case 3: return kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame;
                case 4: return kb.digit5Key.wasPressedThisFrame || kb.numpad5Key.wasPressedThisFrame;
                case 5: return kb.digit6Key.wasPressedThisFrame || kb.numpad6Key.wasPressedThisFrame;
                case 6: return kb.digit7Key.wasPressedThisFrame || kb.numpad7Key.wasPressedThisFrame;
                case 7: return kb.digit8Key.wasPressedThisFrame || kb.numpad8Key.wasPressedThisFrame;
                case 8: return kb.digit9Key.wasPressedThisFrame || kb.numpad9Key.wasPressedThisFrame;
                default: return false;
            }
        }

        // ------------------------------------------------------------------ drawing

        void OnGUI()
        {
            var d = EncounterRunner.Director;
            var s = d != null ? d.Current : null;
            if (s == null)
                return;
            GUI.depth = -10;                                              // (over the test panel)
            float k = Mathf.Clamp(Screen.height / 1440f, 0.6f, 2.5f);
            Styles(k);

            rows.Clear();
            float gap = 6f * k, section = 14f * k;
            rows.Add((s.Title, title, 0f));
            foreach (var who in s.People)
                rows.Add((s.Speaker(who), dim, gap));
            bool first = true;
            foreach (var l in s.lines)
            {
                rows.Add((l.text, body, first ? section : gap));
                first = false;
            }
            if (!string.IsNullOrEmpty(s.Intention))
                rows.Add((s.Intention, dim, section));
            first = true;
            for (int i = 0; i < s.options.Count; i++)
            {
                var o = s.options[i];
                if (s.Chosen && s.result.option != o.def.key)
                    continue;                                             // (after the choice only the chosen one stays)
                string line = (i + 1) + ". " + o.Tagged;
                if (o.odds.Length > 0)
                    line += "  (" + o.odds + ")";
                if (o.isDefault && !s.Chosen)
                    line += "   [Enter]";
                rows.Add((line, o.enabled ? body : faint, first ? section : gap));
                first = false;
                if (!o.enabled && o.rule.Length > 0)
                    rows.Add(("    " + o.rule, faint, 0f));
            }
            if (s.Chosen)
                rows.Add((s.result.Text, body, section));
            // the keys, in the panel's plain words (placeholders: the user's to change)
            string keys = s.Chosen ? "[Enter] go on" : "[1-" + s.options.Count + "] choose";
            if (hint.Length > 0 && Time.unscaledTime < hintUntil)
                keys = hint + "     " + keys;
            rows.Add((keys, faint, section));

            float w = Mathf.Min(width * k, Screen.width - 32f), pad = 18f * k, inner = w - 2f * pad;
            float h = 2f * pad;
            foreach (var r in rows)
                h += r.gap + r.style.CalcHeight(new GUIContent(r.text), inner);
            float x = Mathf.Max(16f, Screen.width * 0.5f - w - 24f * k);   // left of centre: the ship stays in view on the right
            float y = Mathf.Max(16f, (Screen.height - h) * 0.5f);
            var area = new Rect(x, y, w, h);
            Area = area;
            GUI.DrawTexture(area, glass);
            GUI.DrawTexture(new Rect(area.x, area.y, area.width, 1f), black);
            GUI.DrawTexture(new Rect(area.x, area.yMax - 1f, area.width, 1f), black);
            GUI.DrawTexture(new Rect(area.x, area.y, 1f, area.height), black);
            GUI.DrawTexture(new Rect(area.xMax - 1f, area.y, 1f, area.height), black);
            float cy = y + pad;
            foreach (var r in rows)
            {
                cy += r.gap;
                var c = new GUIContent(r.text);
                float rh = r.style.CalcHeight(c, inner);
                GUI.Label(new Rect(x + pad, cy, inner, rh), c, r.style);
                cy += rh;
            }
        }

        void Styles(float k)
        {
            if (glass == null)
            {
                glass = Plain(new Color(8f / 255f, 9f / 255f, 11f / 255f, 0.78f));
                black = Plain(Color.black);
            }
            if (body != null && Mathf.Approximately(k, styledFor) && styledFont == font)
                return;
            styledFor = k;
            styledFont = font;
            body = new GUIStyle { font = this.font, fontSize = Mathf.RoundToInt(18f * k), wordWrap = true, richText = false, normal = { textColor = Ink } };
            title = new GUIStyle(body) { fontSize = Mathf.RoundToInt(22f * k) };
            dim = new GUIStyle(body) { normal = { textColor = new Color(Ink.r, Ink.g, Ink.b, 0.65f) } };
            faint = new GUIStyle(body) { fontSize = Mathf.RoundToInt(15f * k), normal = { textColor = new Color(Ink.r, Ink.g, Ink.b, 0.45f) } };
        }

        static Texture2D Plain(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}
