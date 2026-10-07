// 7 Oct 2026: WHAT THE CONTENT KEEPS IN A SAVED GAME. Most of what the new content remembers is flags, and flags are
// saved already (the encounter layer). What is not a flag (a town's ledger of people and bread, a work half paid for, the
// house's other hulls, the talks left this call) is kept by the class that owns it, which names itself here once; SaveGame
// then writes every part under "content" and hands each its own piece back on a load. So a new part of the game never
// has to edit SaveGame.cs, and two engineers never meet in that file.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// The parts of the content that keep state in a saved game. A part registers a key, a capture and a restore:
    /// <code>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    /// static void OnPlay() { ContentSave.Register("settlements", Capture, Restore); /* and clear its own state */ }</code>
    /// Capture returns plain data that Json.Write takes (Dictionary&lt;string, object&gt;, List&lt;object&gt;, text, double,
    /// bool, null). Restore gets back what Json.Parse made of it (numbers as double), or NULL for a save that has no
    /// such part (an older save, a new game): it must then put itself as at the start. A part that throws is reported
    /// once and skipped; the other parts and the save go on.
    /// </summary>
    public static class ContentSave
    {
        sealed class Part
        {
            public Func<object> capture;
            public Action<object> restore;
        }

        // by key, in the keys' order: the same order every run, so a save's text and a load's steps do not depend on
        // which part happened to register first (registering again under a key replaces: Play without a domain reload)
        static readonly SortedDictionary<string, Part> parts = new SortedDictionary<string, Part>(StringComparer.Ordinal);

        /// <summary>Names a part of the content that is saved (again under the same key: the new one replaces the old).</summary>
        public static void Register(string key, Func<object> capture, Action<object> restore)
        {
            if (string.IsNullOrEmpty(key) || capture == null || restore == null)
                return;
            parts[key] = new Part { capture = capture, restore = restore };
        }

        /// <summary>Takes a part out (a test tidying up).</summary>
        public static void Unregister(string key)
        {
            if (key != null)
                parts.Remove(key);
        }

        public static bool Registered(string key) => key != null && parts.ContainsKey(key);

        /// <summary>The keys of the parts, in order.</summary>
        public static IEnumerable<string> Keys => parts.Keys;

        /// <summary>Every part's state by its key, for SaveGame.Save ("content").</summary>
        public static Dictionary<string, object> Capture()
        {
            var saved = new Dictionary<string, object>();
            foreach (var kv in parts)
            {
                try
                {
                    saved[kv.Key] = kv.Value.capture();
                }
                catch (Exception e)
                {
                    ContentFiles.Warn("save:" + kv.Key, $"the saved part '{kv.Key}' could not be written ({e.GetType().Name}: {e.Message}): left out of this save");
                }
            }
            return saved;
        }

        /// <summary>Hands every part its piece of a loaded game (null: the save has none of it, so the part starts afresh).
        /// For SaveGame.Load, after the encounter layer, so the flags are already the saved ones.</summary>
        public static void Restore(Dictionary<string, object> saved)
        {
            foreach (var kv in parts)
            {
                object piece = null;
                if (saved != null)
                    saved.TryGetValue(kv.Key, out piece);
                try
                {
                    kv.Value.restore(piece);
                }
                catch (Exception e)
                {
                    ContentFiles.Warn("load:" + kv.Key, $"the saved part '{kv.Key}' could not be read back ({e.GetType().Name}: {e.Message}): it starts afresh");
                }
            }
        }
    }
}
