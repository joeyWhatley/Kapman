using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE GAME'S TYPEFACE, HEARTH (the user, 7 Oct 2026: "ive also added a text font called hearth ... if you can use
    /// that with everything other than the title screen"; and that night: "i added the new text"). Hearth is a pixel
    /// face of 8 pixels (Assets/HearthPixelFont, the user's own pack): crisp at 16, 24 and 32, which is why the frame's
    /// sizes are 16 and 24 now. It has 161 letters: the plain alphabet and the common accents. The game writes more than
    /// that: thorn, eth and the long vowels of its Old English, Norse and Frisian (several thousand times), Cyrillic,
    /// runes. So the face cannot be named in a style sheet alone, where a missing letter would be a blank box: here it
    /// is made a font of the text engine's own with the old face (DejaVu Sans Condensed) and the rune face behind it,
    /// to be drawn from for any letter Hearth has not, and that is hung on the root of the panel, which every text of
    /// every document under it inherits (the frame, the chart, the menu, the opening's balloon). The title's name is a
    /// picture, not text, and keeps its own letters.
    ///
    /// The three faces are found through the style sheets (Frame.uss: the classes font-probe-main, -more and -runes
    /// name the files), so nothing is copied and no folder of the pack is touched: three unseen labels are put on the
    /// frame's root, and when the style sheet has dressed them their faces are read back. If the pack is not there the
    /// style sheets' own choice stands.
    /// </summary>
    public static class FrameFonts
    {
        static readonly string[] Probes = { "font-probe-main", "font-probe-more", "font-probe-runes" };
        public const string MainName = "Hearth";

        static FontAsset asset;
        static VisualElement appliedTo, probedOn;
        static Label[] probes;
        static int tries;
        static bool failed;

        /// <summary>The face in use, once it is made (for a test).</summary>
        public static FontAsset Asset => asset;

        /// <summary>Each frame, from the frame: the face is made when its files have been found, and kept on the panel.</summary>
        public static void Keep(VisualElement root)
        {
            if (failed || root == null || root.panel == null)
                return;
            var top = root.panel.visualTree;
            if (asset != null)
            {
                if (!ReferenceEquals(appliedTo, top))
                {
                    top.style.unityFontDefinition = new StyleFontDefinition(asset);
                    appliedTo = top;
                }
                return;
            }
            if (probes == null || !ReferenceEquals(probedOn, root) || probes[0].panel == null)
            {
                if (probes != null)
                    foreach (var old in probes)
                        old?.RemoveFromHierarchy();
                probes = new Label[Probes.Length];
                for (int i = 0; i < Probes.Length; i++)
                {
                    probes[i] = new Label { pickingMode = PickingMode.Ignore };
                    probes[i].AddToClassList("font-probe");
                    probes[i].AddToClassList(Probes[i]);
                    root.Add(probes[i]);
                }
                probedOn = root;
                tries = 0;
                return;                                                   // (dressed by the style sheet before the next frame)
            }
            Font main = probes[0].resolvedStyle.unityFont;
            if (main == null || main.name != MainName)
            {
                if (++tries > 240)
                {
                    failed = true;                                        // (the pack is not in this project: the style sheets' own face stands)
                    Debug.Log("Kapman fonts: the Hearth face was not found through Frame.uss (Assets/HearthPixelFont): the style sheets' own face is used.");
                }
                return;
            }
            var made = FontAsset.CreateFontAsset(main);
            if (made == null)
            {
                failed = true;
                Debug.LogWarning("Kapman fonts: the text engine could not take the Hearth face: the style sheets' own face is used.");
                return;
            }
            made.name = "Hearth (the game's)";
            made.hideFlags = HideFlags.DontSave;
            var behind = new List<FontAsset>();
            for (int i = 1; i < probes.Length; i++)
            {
                Font more = probes[i].resolvedStyle.unityFont;
                if (more == null || more.name == MainName)
                    continue;
                var a = FontAsset.CreateFontAsset(more);
                if (a == null)
                    continue;
                a.name = more.name + " (behind Hearth)";
                a.hideFlags = HideFlags.DontSave;
                behind.Add(a);
            }
            made.fallbackFontAssetTable = behind;
            asset = made;
            foreach (var p in probes)
                p.RemoveFromHierarchy();
            probes = null;
            top.style.unityFontDefinition = new StyleFontDefinition(asset);
            appliedTo = top;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            asset = null;
            appliedTo = probedOn = null;
            probes = null;
            tries = 0;
            failed = false;
        }
    }
}
