using System;
using System.IO;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME SAVES ITSELF (7 Oct 2026: "a crash loses the game"): each time she moors, and when the game is quit.
    /// The last three are kept ("autosave 1" the newest, then 2, then 3) beside the saves the player asked for, which it
    /// never touches. They show in the menu's list of saved games like any other, and the title's Continue takes the
    /// newest save there is.
    ///   - Never while the game cannot be saved (SaveGame.Busy: a meeting, a chase, a search, aground; a game not yet
    ///     begun). A mooring's save is held and tried again each frame until it can be made, or she casts off.
    ///   - The new game is written whole to "autosave new" first; only when that file is there do the older ones move
    ///     down. A save that fails changes nothing on disk; a crash in the middle loses at most the oldest.
    ///   - At a quit that cannot be saved the last one stands, and the quit is not stopped.
    ///   - A headless run (the tests, a bake) never writes among the player's own saves: only into a folder it has
    ///     named itself (SaveGame.FolderOverride).
    /// </summary>
    public static class Autosave
    {
        public const int Keep = 3;
        public const string Prefix = "autosave ";
        /// <summary>Written first; the older ones move only when it is there.</summary>
        public const string Fresh = "autosave new";
        const string GameFile = ".kapman.json.gz", InfoFile = ".kapman.info";

        /// <summary>Off: the game does not save itself (a tool that wants none).</summary>
        public static bool Enabled = true;
        /// <summary>How many it has made this play (a test, a tool).</summary>
        public static int Made { get; private set; }
        /// <summary>A mooring's save is still to be made.</summary>
        public static bool Held { get; private set; }

        static float lastQuitSave = -100f;

        public static string Slot(int n) => Prefix + n;

        /// <summary>Is this one of the game's own saves (not one the player asked for)?</summary>
        public static bool IsAutosave(string slot) => slot != null && slot.StartsWith(Prefix, StringComparison.Ordinal);

        /// <summary>She has moored (the watch on the moorings): save now, or as soon as it can be.</summary>
        public static void Moored()
        {
            Held = true;
            Tick();
        }

        /// <summary>She has cast off with the save still held: let it go (the next mooring makes one).</summary>
        public static void CastOff() => Held = false;

        /// <summary>Each frame: the held save, once nothing is in the way.</summary>
        public static void Tick()
        {
            if (Held && Now())
                Held = false;
        }

        /// <summary>The game is being quit: one save, if it can be made. Asked twice in one quit, it saves once.</summary>
        public static void Quitting()
        {
            if (Time.realtimeSinceStartup - lastQuitSave < 5f)
                return;
            if (Now())
                lastQuitSave = Time.realtimeSinceStartup;
            Held = false;
        }

        /// <summary>Why the game does not save itself just now (null: it would).</summary>
        public static string Refused()
        {
            if (!Enabled)
                return "off";
            if (Application.isBatchMode && SaveGame.FolderOverride == null)
                return "a headless run does not write among the player's saves";
            return SaveGame.Busy();
        }

        /// <summary>Makes an autosave now; false if it could not be (busy, not ready, the disk): nothing is changed then.</summary>
        public static bool Now()
        {
            if (Refused() != null)
                return false;
            try
            {
                string folder = SaveGame.Folder;
                if (File.Exists(Path.Combine(folder, Fresh + GameFile)))
                    Shift(folder);                                       // (left by a crash between the save and the shuffle: it is whole, keep it)
                SaveGame.Save(Fresh);
                if (!File.Exists(Path.Combine(folder, Fresh + GameFile)))
                    return false;
                Shift(folder);
                Made++;
                Debug.Log("Kapman: the game saved itself (" + Slot(1) + ", " + KapmanGame.Current.Date + ").");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the autosave could not be made: " + ex.Message);
                return false;
            }
        }

        /// <summary>The shuffle, in a folder: the oldest goes, each moves down one, the fresh one becomes "autosave 1".
        /// Does nothing if there is no fresh one (so nothing is ever dropped without a newer save in its place).</summary>
        public static void Shift(string folder, int keep = Keep)
        {
            if (!File.Exists(Path.Combine(folder, Fresh + GameFile)))
                return;
            for (int n = keep; n >= 1; n--)
            {
                string from = n == 1 ? Fresh : Slot(n - 1);
                if (!File.Exists(Path.Combine(folder, from + GameFile)))
                    continue;                                            // (fewer than three yet: the one below stays as it is)
                Move(folder, from, Slot(n), GameFile);
                // (the list's line goes with its game; a game that has none must not keep the line of the one it replaced)
                string staleInfo = Path.Combine(folder, Slot(n) + InfoFile);
                if (File.Exists(staleInfo))
                    File.Delete(staleInfo);
                Move(folder, from, Slot(n), InfoFile);
            }
        }

        static void Move(string folder, string from, string to, string ending)
        {
            string a = Path.Combine(folder, from + ending), b = Path.Combine(folder, to + ending);
            if (!File.Exists(a))
                return;
            if (File.Exists(b))
                File.Delete(b);
            File.Move(a, b);
        }

        // the built game: the window's X, Alt+F4, Application.Quit, before the scene is taken down
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook()
        {
            Held = false;
            Made = 0;
            lastQuitSave = -100f;
            Application.wantsToQuit -= WantsToQuit;
            Application.wantsToQuit += WantsToQuit;
        }

        static bool WantsToQuit()
        {
            Quitting();
            return true;                                                 // (never stands in the way of the quit)
        }
    }
}
