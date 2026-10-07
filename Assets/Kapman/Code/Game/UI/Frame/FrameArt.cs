using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// A PICTURE BY ITS SLOT (the user's ruling of 7 Oct 2026, Design/DECISIONS.md: "just make a description instead and a
    /// placeholder, for me to go over later and make the art for it"). Every picture of the game is a slot with a fixed
    /// file name (Art/art_list.csv says what each is and where it goes). The game shows Art/final/&lt;file&gt; when the user
    /// has made it, else Art/placeholder/&lt;file&gt;, read from the project folder as the chart's tiles are; with neither
    /// (a slot not listed, or a built game without the folder) there is no picture and the caller draws a plain stand-in.
    /// Pixel art: read as it is, not smoothed.
    /// </summary>
    public static class FrameArt
    {
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        /// <summary>The Art folder beside Assets.</summary>
        public static string Root => Path.Combine(Application.dataPath, "..", "Art");

        /// <summary>The player's own portrait: "portrait/self.frisian.man.png".</summary>
        public static string Self(string people, bool woman) => $"portrait/self.{(string.IsNullOrEmpty(people) ? "frisian" : people)}.{(woman ? "woman" : "man")}.png";

        /// <summary>A person of a places file: "portrait/person.dorestad.tollman.png" for the id "dorestad.tollman".</summary>
        public static string Person(string personId) => $"portrait/person.{personId}.png";

        /// <summary>The slot of somebody the opening knows by a bare name (the lender; Ravn, your first hand); "" for
        /// anybody else.</summary>
        public static string Named(string who) =>
            who == "lender" ? "portrait/person.lender.png" : who == "Ravn" ? "portrait/person.cast.crew.ravn.png" : "";

        /// <summary>The picture of a slot ("portrait/self.frisian.man.png"), or null when there is none. Read once.</summary>
        public static Texture2D Load(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            if (cache.TryGetValue(file, out var known))
                return known;
            Texture2D tex = null;
            // (the user's own drawing; or a real picture standing in for it, Art/standin, listed in Design/art_standins.md;
            // or the drawn placeholder of the art list)
            foreach (string kind in new[] { "final", "standin", "placeholder" })
            {
                string path = Path.Combine(Root, kind, file);
                try
                {
                    if (!File.Exists(path))
                        continue;
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                    if (tex.LoadImage(File.ReadAllBytes(path), true))
                        break;
                    Object.Destroy(tex);
                    tex = null;
                }
                catch (IOException e)
                {
                    Debug.LogWarning("Kapman art: " + path + " could not be read (" + e.Message + ").");
                    tex = null;
                }
            }
            cache[file] = tex;
            return tex;
        }

        // Enter Play Mode runs without a domain reload: the pictures are read again each play (the user may have drawn one)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            foreach (var tex in cache.Values)
                if (tex != null)
                    Object.Destroy(tex);
            cache.Clear();
        }
    }
}
