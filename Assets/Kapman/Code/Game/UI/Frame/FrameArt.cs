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

        static string[] pool;

        /// <summary>
        /// THE PLAYER'S PORTRAIT WHILE NO FACE HAS BEEN CHOSEN (a game that did not begin through the opening; a save from
        /// before the choice). The user, 8 Oct 2026, of a picture from such a game: "Why can I see the old ai generated
        /// portrait"; and of the face I then took from the pool by the people's name: "Use one of the portraits I gave
        /// the one you were using earlier was fine". So it is the FIRST face of the pool the opening offers
        /// (Showtime.FacePool: the user's own in Art/final/portrait_pool, the pictures the user gave in
        /// Art/standin/portrait_pool), which is the one the pictures of the opening had shown. Only on a machine with no
        /// pool at all does the drawn portrait of the people stand.
        /// </summary>
        public static string SelfUnchosen(string people)
        {
            if (pool == null)
                pool = Showtime.FacePool().ToArray();
            return pool.Length > 0 ? pool[0] : Self(people, false);
        }

        /// <summary>The drawn portrait of a people, the art list's own slot for the player: "portrait/self.frisian.man.png".</summary>
        public static string Self(string people, bool woman) => $"portrait/self.{(string.IsNullOrEmpty(people) ? "frisian" : people)}.{(woman ? "woman" : "man")}.png";

        /// <summary>A person of a places file: "portrait/person.dorestad.tollman.png" for the id "dorestad.tollman".</summary>
        public static string Person(string personId) => $"portrait/person.{personId}.png";

        /// <summary>The picture of somebody who speaks on a card: one of the standing cast has a portrait of their own
        /// ("portrait/person.cast.crew.ravn.png"); anybody else has the portrait of their trade ("portrait/role.tollman.png").
        /// "" where there is no such picture (the plain shape stands).</summary>
        public const string LenderId = "dorestad.wursing";

        public static string OfSpeaker(string id, string role)
        {
            // (a named person: their own portrait; one of the places who is of the story's cast had theirs issued
            // under the cast's id)
            // (the lender of the opening is a person of Dorestat now, and his portrait was issued as the lender's)
            if (id == LenderId && Load(Named("lender")) != null)
                return Named("lender");
            if (Kapman.Encounters.NpcRegistry.IsCast(id))
            {
                string own = Person(id);
                if (Load(own) != null)
                    return own;
                string same = PlaceBook.Current.Person(id)?.same ?? "";
                if (same.Length > 0 && Load(Person(same)) != null)
                    return Person(same);
            }
            string trade = string.IsNullOrEmpty(role) ? "" : $"portrait/role.{role}.png";
            return trade.Length > 0 && Load(trade) != null ? trade : "";
        }

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
                    Drop(tex);
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

        /// <summary>A picture is let go (in the editor outside play, where a test may have read one, at once).</summary>
        static void Drop(Texture2D tex)
        {
            if (Application.isPlaying)
                Object.Destroy(tex);
            else
                Object.DestroyImmediate(tex);
        }

        // Enter Play Mode runs without a domain reload: the pictures are read again each play (the user may have drawn one)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            foreach (var tex in cache.Values)
                if (tex != null)
                    Drop(tex);
            cache.Clear();
            pool = null;
        }
    }
}
