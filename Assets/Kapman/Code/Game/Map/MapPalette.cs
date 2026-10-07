using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE WORLD'S COLOURS (the user, 7 Oct 2026: "depending on your culture you start somewhere else and the world is a
    /// different colour from the preset, take away the option to change it, also create several more 3bit colour palettes
    /// created around Byzantium colour"). A palette is three bits: eight colours, one for each kind of ground the chart
    /// has. Every palette holds Byzantium (#702963) in one of its eight places. The player's people choose the palette;
    /// there is no key to change it. The picture on disk stays as it was painted: a tile is recoloured as it is read.
    /// (Bake/world/recolour_map.py does the same sum for pictures on disk, and Design/palettes holds each as a .gpl.)
    ///   0 deep water   1 sea   2 shallows   3 shore (sand, beach)   4 marsh, heath and peat   5 cleared land
    ///   6 woodland     7 high and bare ground (rock, snow, and the pale of the tidal flats)
    /// </summary>
    public static class MapPalette
    {
        public sealed class Palette
        {
            public string people, name, home;
            public Color32[] colours;
        }

        static Palette P(string people, string name, string home, params string[] hex)
        {
            var c = new Color32[8];
            for (int i = 0; i < 8; i++)
            {
                ColorUtility.TryParseHtmlString(hex[i], out var col);
                c[i] = col;
            }
            return new Palette { people = people, name = name, home = home, colours = c };
        }

        /// <summary>The palettes, by people: the name of each, the port that people starts from, and its eight colours
        /// (deep, sea, shallows, shore, marsh, cleared, wood, high).</summary>
        public static readonly Palette[] All =
        {
            P("frisian", "Tide", "dorestad", "#37113c", "#702963", "#a8619b", "#e6c9a8", "#7d8442", "#b3b56a", "#4a5a2c", "#faf1df"),
            P("frankish", "Vineyard", "quentovic", "#2b1233", "#702963", "#9466ab", "#f0d27a", "#9a4a5a", "#cf9a6a", "#481224", "#f6efe0"),
            P("english", "Minster", "lundenwic", "#2e1233", "#702963", "#a070a6", "#efe5c2", "#5f8a5c", "#9fbf7a", "#297036", "#dfe8e0"),
            P("danish", "Amber", "ripa", "#14323a", "#38717e", "#73a7ad", "#f7dda8", "#a07c2c", "#d9a441", "#702963", "#f6efe0"),
            P("norse", "Fjord", "ogvaldsnes", "#171a38", "#585896", "#9490c2", "#cbbccb", "#702963", "#5fa08a", "#204a44", "#f4f1fa"),
            P("svea", "Silver", "birka", "#241a2e", "#463c43", "#8f82a3", "#cbcde2", "#702963", "#a9b08a", "#4f5a4a", "#faf8fc"),
            P("wendish", "Grove", "reric", "#2e1233", "#702963", "#b06aa0", "#dccba0", "#637029", "#94a23c", "#414c1a", "#f8f2d9"),
            P("irish", "Vellum", "hi", "#202638", "#2c678f", "#82b3ba", "#f3e7c4", "#ce4f48", "#d8c27a", "#4c6e1f", "#702963"),
        };

        const string Pref = "kapman.people";
        static int scheme = -1;

        /// <summary>Counts up each time the people (and so the colours) change: whoever shows the chart looks again.</summary>
        public static int Version { get; private set; }

        /// <summary>The palette in use: 0 is the picture as painted (tools and tests); 1.. are All[n - 1].</summary>
        public static int Scheme
        {
            get
            {
                if (scheme < 0)
                    scheme = IndexOf(PlayerPrefs.GetString(Pref, "frisian")) + 1;
                return scheme;
            }
        }

        /// <summary>The palette in use, or null for the picture as painted.</summary>
        public static Palette Current => Scheme > 0 ? All[Scheme - 1] : null;

        static int IndexOf(string people)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].people == people)
                    return i;
            return 0;
        }

        /// <summary>The world takes the colours of the player's people (set when a game begins; kept for the next run).
        /// A people with no palette of its own gets the Frisian one.</summary>
        public static void SetPeople(string people)
        {
            int next = IndexOf(people) + 1;
            PlayerPrefs.SetString(Pref, All[next - 1].people);
            if (next == scheme)
                return;
            scheme = next;
            Version++;
        }

        /// <summary>The picture as painted, for tools and tests (not kept).</summary>
        public static void AsPainted()
        {
            if (scheme == 0)
                return;
            scheme = 0;
            Version++;
        }

        /// <summary>Which of the eight kinds of ground a painted colour is.</summary>
        public static int Role(float r, float g, float b)
        {
            Hls(r, g, b, out float h, out float l, out float s);
            if (b > r && b >= g * 0.95f && s > 0.08f)                    // water, by its depth
                return l < 0.22f ? 0 : l < 0.32f ? 1 : 2;
            if (s < 0.12f || (l > 0.80f && s < 0.35f))                   // greys and near-whites: rock, snow, the flats
                return 7;
            if (g >= r && g > b)                                         // greens
                return l < 0.30f ? 6 : l < 0.40f ? 4 : 5;
            return l > 0.55f ? 3 : 4;                                    // warm colours: sand above, peat and earth below
        }

        /// <summary>One painted colour in a palette.</summary>
        public static Color32 Map(Color32 c, int which)
        {
            if (which <= 0 || which > All.Length)
                return c;
            var to = All[which - 1].colours[Role(c.r / 255f, c.g / 255f, c.b / 255f)];
            return new Color32(to.r, to.g, to.b, c.a);
        }

        /// <summary>Recolours a picture just read, in place (the picture has a few dozen colours: each is worked out once).</summary>
        public static void Apply(Texture2D tex, int which)
        {
            if (tex == null || which <= 0 || which > All.Length)
                return;
            var px = tex.GetPixels32();
            var seen = new Dictionary<int, Color32>(64);
            int lastKey = -1;
            Color32 last = default;
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                int key = (c.r << 16) | (c.g << 8) | c.b;
                if (key != lastKey)
                {
                    if (!seen.TryGetValue(key, out last))
                        seen[key] = last = Map(c, which);
                    lastKey = key;
                }
                px[i] = new Color32(last.r, last.g, last.b, c.a);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);                                     // (and the copy in memory is let go)
        }

        // hue, lightness, saturation as Python's colorsys has them (so the game and the bake script agree)
        static void Hls(float r, float g, float b, out float h, out float l, out float s)
        {
            float max = Mathf.Max(r, Mathf.Max(g, b)), min = Mathf.Min(r, Mathf.Min(g, b));
            l = (max + min) / 2f;
            if (max == min)
            {
                h = 0f; s = 0f;
                return;
            }
            float d = max - min;
            s = l <= 0.5f ? d / (max + min) : d / (2f - max - min);
            float rc = (max - r) / d, gc = (max - g) / d, bc = (max - b) / d;
            h = r == max ? bc - gc : g == max ? 2f + rc - bc : 4f + gc - rc;
            h = (h / 6f) % 1f;
            if (h < 0f)
                h += 1f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => scheme = -1;
    }
}
