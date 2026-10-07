// 7 Oct 2026: THE FLAGS, ASKED FROM ANYWHERE. Everything the content remembers as a number is a flag of the encounter
// layer (Core/Encounters/EncounterState.cs): a chain's steps and endings (ch_<id>_<n>, ch_<id>_end_<name>), what the
// house has won (unlock_<kind>_<what>), history changed (changed_<event>), the house's own (house_wed, wintering), the
// town acts' (act_<what>), and what the living world writes for the cards to read (st_<port>_heads, here_hungry, age,
// war_<id>). They are saved with the game already (SaveGame's "encounters"). This is the one door to them for the code
// outside the cards: the unlock registry, the chain journal, the town, the ledgers, the HUD.
using System;
using System.Collections.Generic;
using Kapman.Encounters;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// Reads and sets flags on the game's encounter state (ContentRules.Director: the live game's, or the one a test
    /// handed in), on the day the encounter layer is at. With no game running every flag reads 0 and nothing can be set.
    /// A test that only needs flags hands in a state of its own with <see cref="Use"/>.
    /// </summary>
    public static class ContentFlags
    {
        static EncounterState testState;
        static Func<double> testDay;
        static EncounterState lastState;
        static int serial;

        /// <summary>A test's own flags and day (null, null: the game's again).</summary>
        public static void Use(EncounterState state, Func<double> day = null)
        {
            testState = state;
            testDay = state != null ? day : null;
        }

        /// <summary>The state the flags are in (null: no game is running).</summary>
        public static EncounterState State
        {
            get
            {
                if (testState != null)
                    return testState;
                var d = ContentRules.Director;
                return d != null ? d.state : null;
            }
        }

        /// <summary>Game days since the start, as the encounter layer counts them (a flag lapses by this).</summary>
        public static double Day
        {
            get
            {
                if (testState != null)
                    return testDay != null ? testDay() : 0.0;
                var d = ContentRules.Director;
                return d != null ? d.Day : 0.0;
            }
        }

        /// <summary>Is there a game to ask?</summary>
        public static bool Ready => State != null;

        /// <summary>
        /// Changes whenever any flag may have changed (one was set, a lapsed one was dropped, a game was loaded, or the
        /// game itself is another one). A reader that keeps a list made from the flags keeps the number it saw and makes
        /// its list again when this differs.
        /// </summary>
        public static int Version
        {
            get
            {
                var s = State;
                if (!ReferenceEquals(s, lastState))
                {
                    lastState = s;
                    serial++;
                }
                return unchecked(serial * 1000003 + (s != null ? s.FlagVersion : 0));
            }
        }

        /// <summary>A flag's value today (0: never set, lapsed, or no game).</summary>
        public static double Value(string name)
        {
            var s = State;
            return s != null ? s.Flag(name, Day) : 0.0;
        }

        /// <summary>Is the flag set (not 0) today? What "flag.&lt;name&gt;" means in a card's condition.</summary>
        public static bool Has(string name) => Value(name) != 0.0;

        /// <summary>Sets a flag (to 1, or a value); with days it lapses that many days from today. False (and nothing
        /// done) with no game running or a name that is not letters, digits and underscores.</summary>
        public static bool Set(string name, double value = 1.0, double days = 0.0)
        {
            var s = State;
            if (s == null)
                return false;
            if (!ValidName(name))
            {
                ContentFiles.Warn("flagname:" + name, $"'{name}' is not a flag's name (letters, digits and underscores only): not set");
                return false;
            }
            s.SetFlag(name, value, Day, days);
            return true;
        }

        /// <summary>Adds to a flag's value (a count: st_dorestad_helped), as the effect "flag &lt;name&gt; +n" does. A flag that
        /// lapses keeps its day; one never set starts from 0 and holds for good. False as for Set.</summary>
        public static bool Add(string name, double by)
        {
            var s = State;
            if (s == null)
                return false;
            if (!ValidName(name))
            {
                ContentFiles.Warn("flagname:" + name, $"'{name}' is not a flag's name (letters, digits and underscores only): not set");
                return false;
            }
            s.AddFlag(name, by, Day);
            return true;
        }

        /// <summary>Sets a flag to 0 (it then reads as not set).</summary>
        public static bool Clear(string name) => Set(name, 0.0);

        /// <summary>Every flag that holds today with its value, in the names' order (empty with no game).</summary>
        public static List<KeyValuePair<string, double>> All()
        {
            var s = State;
            return s != null ? s.Flags(Day) : new List<KeyValuePair<string, double>>();
        }

        /// <summary>The flags that hold today whose names begin so ("unlock_", "ch_black_stones_"), in the names' order.</summary>
        public static List<KeyValuePair<string, double>> WithPrefix(string prefix)
        {
            var all = All();
            if (string.IsNullOrEmpty(prefix))
                return all;
            all.RemoveAll(kv => !kv.Key.StartsWith(prefix, StringComparison.Ordinal));
            return all;
        }

        /// <summary>A flag's name is letters, digits and underscores (CHAINS.md): only such a name can be read back as
        /// flag.&lt;name&gt; by a condition.</summary>
        public static bool ValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            foreach (char c in name)
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
            return true;
        }

        // Enter Play Mode without a domain reload: no test's state is left behind
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            testState = null;
            testDay = null;
            lastState = null;
        }
    }
}
