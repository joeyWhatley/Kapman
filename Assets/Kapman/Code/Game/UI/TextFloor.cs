using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE FLOOR UNDER THE TEXT (7 Oct 2026; Design/text_size_audit.md): no text under 18 true pixels. The game's panel
    /// scales with the screen (Game/UI/GamePanel: 1920 x 1080 is a scale of 1), so on 1280 x 720 a size of 18 in a
    /// stylesheet is 12 on the glass. A stylesheet cannot read the screen. So each stylesheet names its sizes as four
    /// variables on its root (--t-quiet, --t-body, --t-name, --t-title) and has the classes t-floor-20 to t-floor-36
    /// that lift them; this puts the right class on a root for the panel's scale now. The class's number is the least
    /// size in panel pixels: that number times the scale is 18 or a little over.
    /// </summary>
    public static class TextFloor
    {
        /// <summary>The least true size of any text, in screen pixels.</summary>
        public const float LeastPx = 18f;

        static readonly int[] Steps = { 20, 22, 24, 26, 28, 30, 32, 34, 36 };

        /// <summary>The class for a panel's scale (true pixels to a panel pixel), or null where none is needed (a scale of
        /// 1 or more). Under a scale of a half the last class stands and text does fall under 18: such a screen cannot
        /// hold the frame.</summary>
        public static string ClassFor(float scale)
        {
            if (scale <= 0f || float.IsNaN(scale) || scale >= 1f)
                return null;
            float need = LeastPx / scale;                                  // panel pixels that make 18 true ones
            foreach (int step in Steps)
                if (need <= step + 0.01f)
                    return "t-floor-" + step;
            return "t-floor-" + Steps[Steps.Length - 1];
        }

        /// <summary>The least size in panel pixels a class stands for (18 for none).</summary>
        public static int PanelPx(string floorClass) =>
            string.IsNullOrEmpty(floorClass) ? 18 : int.Parse(floorClass.Substring("t-floor-".Length));

        /// <summary>The scale of a panel as it is drawn now: true pixels to a panel pixel. From the laid-out panel and
        /// what it is drawn into (the screen, or the picture a test draws it into), so it holds whatever the panel's
        /// settings are. 1 while the panel is not laid out yet.</summary>
        public static float ScaleOf(VisualElement root, PanelSettings settings)
        {
            if (root == null || root.panel == null)
                return 1f;
            float panelHeight = root.panel.visualTree.layout.height;
            if (float.IsNaN(panelHeight) || panelHeight < 1f)
                return 1f;
            float pixels = settings != null && settings.targetTexture != null ? settings.targetTexture.height : Screen.height;
            return pixels / panelHeight;
        }

        /// <summary>Keeps the right floor class on a root. Called every frame: it does nothing unless the scale has moved.
        /// `kept` is the caller's memory of the class now on the root. True when the class changed.</summary>
        public static bool Keep(VisualElement root, PanelSettings settings, ref string kept)
        {
            if (root == null || root.panel == null)
                return false;
            string want = ClassFor(ScaleOf(root, settings));
            if (want == kept)
                return false;
            if (kept != null)
                root.RemoveFromClassList(kept);
            if (want != null)
                root.AddToClassList(want);
            kept = want;
            return true;
        }
    }
}
