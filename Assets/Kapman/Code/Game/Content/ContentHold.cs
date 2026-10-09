using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// WHAT IS IN THE GAME FOR NOW, AND WHAT IS HELD BACK (the user, 9 Oct 2026: "for now we don't include any fully
    /// fleshed out quests/ cards until they're polished, only keep in the good polished stuff for now and we will add
    /// in the other content when it's all done").
    ///
    /// Game/Data/content_hold.json names the card files that are IN. A card file it does not name is not read into
    /// the game at all: its quests and cards never come up, nothing of it is drawn, offered or scheduled. A file may
    /// be in whole ("all") or by a list of its cards' ids (the others of that file stay held). NOTHING IS DELETED:
    /// the held files lie in Game/Data/encounters as before, and the tests go on reading every one of them off the
    /// disk, so what is held does not rot while it waits. To let a file in when it is polished, name it in the table.
    /// A new card file is held until it is named there.
    ///
    /// Only the cards of the card engine are held (the quests, the ports' cards, the dated cards, the crew's; the
    /// sea's are in since the user's answer of the same day). The game's other tables are as they were.
    /// </summary>
    public static class ContentHold
    {
        public const string File = "content_hold.json";

        /// <summary>A test that needs the whole book for its samples (the cards' own machinery is shown on the ports'
        /// cards and the quests) sets this, as it sets its other switches, after each new game. Never set by the game.</summary>
        public static bool AllIn;

        public sealed class Table
        {
            /// <summary>False: nothing is held (the table is missing, or says "hold": false).</summary>
            public bool hold;
            /// <summary>The card files that are in, by their name ("sea_793.json"): null for the whole file, or the ids of
            /// its cards that are in.</summary>
            public readonly Dictionary<string, HashSet<string>> files = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        static readonly ContentCache<Table> cache = new ContentCache<Table>("what is held back", () => Read(ContentFiles.ParseObject(File)));

        static Table own;

        /// <summary>The table as the game has it (or one a test has put in its place: <see cref="Use"/>).</summary>
        public static Table Current => own ?? cache.Value;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(Table table) => own = table;

        /// <summary>Is anything held back now?</summary>
        public static bool On => !AllIn && Current.hold;

        /// <summary>The table from its text's root (null: nothing is held).</summary>
        public static Table Read(Dictionary<string, object> root)
        {
            var table = new Table();
            if (root == null)
                return table;
            table.hold = Json.Bool(root, "hold", true);
            foreach (var kv in Json.Obj(root, "in") ?? new Dictionary<string, object>())
            {
                if (kv.Value is List<object> ids)
                {
                    var set = new HashSet<string>(StringComparer.Ordinal);
                    foreach (object id in ids)
                        if (id is string s && s.Length > 0)
                            set.Add(s);
                    table.files[kv.Key] = set;
                }
                else
                    table.files[kv.Key] = null;                           // ("all")
            }
            return table;
        }

        static string NameOf(string file)
        {
            if (string.IsNullOrEmpty(file))
                return "";
            int cut = Math.Max(file.LastIndexOf('/'), file.LastIndexOf('\\'));
            return cut >= 0 ? file.Substring(cut + 1) : file;
        }

        /// <summary>Is this card file read into the game (whole or in part)? Its name with or without its folder.</summary>
        public static bool FileIn(string file) => !On || Current.files.ContainsKey(NameOf(file));

        /// <summary>Is this card of this file in the game?</summary>
        public static bool CardIn(string file, string cardId)
        {
            if (!On)
                return true;
            if (!Current.files.TryGetValue(NameOf(file), out var ids))
                return false;
            return ids == null || (cardId != null && ids.Contains(cardId));
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState): a test's
        // switch never outlives its game
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            AllIn = false;
            own = null;
        }
    }
}
