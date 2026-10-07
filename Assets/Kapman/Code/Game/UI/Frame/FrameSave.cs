using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WHAT THE FRAME KEEPS IN A SAVED GAME (7 Oct 2026; ContentSave part "frame"):
    ///   - the LOG of what was said (in a game of words it is the player's memory of the voyage; before, a load left
    ///     the lines of the game that had been running on screen, and a restart left none);
    ///   - the ARRANGEMENT of the windows, with their sizes (it is also kept in the settings for a new game, at every
    ///     change and not only on a clean close);
    ///   - which FULL WINDOWS stand open and where (the brief faces' builder opens them: <see cref="OpenFull"/>; until
    ///     then a saved list is carried through as it is);
    ///   - the small things a player would miss: which lines each person has been asked for already, what she waits
    ///     for and the course laid by meanwhile, and the course she lies to on.
    /// Static, so a save made while the frame is not up still writes what the frame last had, and a load before the
    /// frame is up is held for it. A save without the part (an older one) gives an empty log and leaves the windows
    /// as the player has them.
    /// </summary>
    public static class FrameSave
    {
        public const string Key = "frame";

        /// <summary>A full window that stands open: its panel, and its place in percent of the frame (below 0: where a
        /// new one goes).</summary>
        public struct FullWindow
        {
            public string panel;
            public float left, top;
        }

        /// <summary>The arrangement of the game last loaded or saved (FrameWindows.Capture's line), "" for none.</summary>
        public static string Layout = "";
        /// <summary>The course she lies to on by the Lie to button ("" none), of the game last loaded or saved.</summary>
        public static string LieTo = "";
        /// <summary>The full windows last known, back to front.</summary>
        public static readonly List<FullWindow> Full = new List<FullWindow>();
        /// <summary>Counts up when a load hands the frame a new state (the frame takes it in its next Update, when the
        /// game is whole again).</summary>
        public static int Loads { get; private set; }

        /// <summary>Opens a panel's full window and gives its element back (null: could not). Set by whoever builds the
        /// full windows (the ruling of 7 October: every panel a brief face and a full window). While it is null the
        /// saved list is kept as it is and written out again, so nothing is lost.</summary>
        public static Func<string, VisualElement> OpenFull;

        public static object Capture()
        {
            FrameRoot.Current?.SaveState();                              // (the frame up: its state as it is now)
            var full = new List<object>();
            foreach (var w in Full)
                full.Add(new Dictionary<string, object> { ["panel"] = w.panel, ["left"] = (double)w.left, ["top"] = (double)w.top });
            return new Dictionary<string, object>
            {
                ["log"] = FrameLog.Capture(), ["windows"] = Layout, ["full"] = full,
                ["asked"] = PersonTalk.Capture(), ["wait"] = FrameWait.Capture(), ["lie_to"] = LieTo,
            };
        }

        /// <summary>Null (an older save): the log is emptied, nothing is waited for, and the arrangement on screen stays
        /// as the player has it.</summary>
        public static void Restore(object saved)
        {
            var d = saved as Dictionary<string, object>;
            FrameLog.Restore(d != null ? Json.Arr(d, "log") : null);
            PersonTalk.Restore(d != null ? Json.Obj(d, "asked") : null);
            FrameWait.Restore(d != null ? Json.Obj(d, "wait") : null);
            LieTo = d != null ? Json.Str(d, "lie_to") : "";
            Layout = d != null ? Json.Str(d, "windows") : "";
            Full.Clear();
            if (d != null && Json.Arr(d, "full") is List<object> full)
                foreach (object o in full)
                    if (Json.Str(o, "panel").Length > 0)
                        Full.Add(new FullWindow { panel = Json.Str(o, "panel"), left = (float)Json.Num(o, "left", -1.0), top = (float)Json.Num(o, "top", -1.0) });
            Loads++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            Layout = "";
            LieTo = "";
            Full.Clear();
            Loads = 0;
            OpenFull = null;
            ContentSave.Register(Key, Capture, Restore);
        }
    }
}
