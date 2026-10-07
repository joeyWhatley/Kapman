using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE FRAME'S COLOURS. The panels are dark plum with a raised edge and gold for what is the player's to spend, the
    /// same in every world. Two colours follow the world's palette (MapPalette: the player's people choose it, and there
    /// is no key to change it), as the mockup's frame took its edge and its accent from the map's key colours: the EDGE
    /// is the palette's sea (what is switched on, the button a window is for, the sail's stripes), and the ACCENT is the
    /// hue of its shallows made light (labels, tags, rules). The frame wears the water she sails on.
    /// The stylesheet holds the same figures as one class a palette (Frame.uss: .pal-frisian ...); a test keeps the two
    /// in step.
    /// </summary>
    public static class FrameColours
    {
        public static readonly Color32 Byzantium = new Color32(112, 41, 99, 255);
        public static readonly Color32 Gold = new Color32(240, 210, 122, 255);
        public static readonly Color32 Paper = new Color32(236, 235, 230, 255);
        public static readonly Color32 Plum = new Color32(34, 22, 40, 255);
        public static readonly Color32 EdgeLight = new Color32(140, 106, 142, 255);
        public static readonly Color32 EdgeDark = new Color32(26, 15, 29, 255);

        /// <summary>The class the frame's root wears for a palette ("pal-frisian"; "pal-painted" for the picture as painted).</summary>
        public static string Class(MapPalette.Palette p) => "pal-" + (p != null ? p.people : "painted");

        /// <summary>The edge: the palette's sea.</summary>
        public static Color32 Edge(MapPalette.Palette p) => p != null ? p.colours[1] : Byzantium;

        /// <summary>The accent: the hue of the palette's shallows, light (lightness 0.76, a little more colour).</summary>
        public static Color32 Accent(MapPalette.Palette p)
        {
            Color32 from = p != null ? p.colours[2] : Byzantium;
            Hls(from.r / 255f, from.g / 255f, from.b / 255f, out float h, out _, out float s);
            Rgb(h, 0.76f, Mathf.Min(1f, s * 1.1f), out float r, out float g, out float b);
            return new Color32((byte)(int)(r * 255f + 0.5f), (byte)(int)(g * 255f + 0.5f), (byte)(int)(b * 255f + 0.5f), 255);
        }

        public static Color32 EdgeNow => Edge(MapPalette.Current);
        public static Color32 AccentNow => Accent(MapPalette.Current);

        // hue, lightness, saturation as the mockup's script and Python's colorsys have them
        static void Hls(float r, float g, float b, out float h, out float l, out float s)
        {
            float max = Mathf.Max(r, Mathf.Max(g, b)), min = Mathf.Min(r, Mathf.Min(g, b));
            l = (max + min) / 2f;
            h = 0f;
            s = 0f;
            if (max == min)
                return;
            float d = max - min;
            s = l <= 0.5f ? d / (max + min) : d / (2f - max - min);
            float rc = (max - r) / d, gc = (max - g) / d, bc = (max - b) / d;
            h = r == max ? bc - gc : g == max ? 2f + rc - bc : 4f + gc - rc;
            h = (h / 6f) % 1f;
            if (h < 0f)
                h += 1f;
        }

        static void Rgb(float h, float l, float s, out float r, out float g, out float b)
        {
            if (s <= 0f)
            {
                r = g = b = l;
                return;
            }
            float m2 = l <= 0.5f ? l * (1f + s) : l + s - l * s, m1 = 2f * l - m2;
            r = V(m1, m2, h + 1f / 3f);
            g = V(m1, m2, h);
            b = V(m1, m2, h - 1f / 3f);
        }

        static float V(float m1, float m2, float x)
        {
            x = ((x % 1f) + 1f) % 1f;
            return x < 1f / 6f ? m1 + (m2 - m1) * x * 6f : x < 0.5f ? m2 : x < 2f / 3f ? m1 + (m2 - m1) * (2f / 3f - x) * 6f : m1;
        }
    }
}
