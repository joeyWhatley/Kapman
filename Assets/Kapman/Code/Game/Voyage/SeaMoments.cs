using System;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// WHAT THE SEA TELLS HER IS A FLOW (the user's ruling of 7 Oct 2026: flows are graphs). Seven moments of wind and
    /// tide come to her as one event, "On the sea moves her", of the graph "Voyage - the sea moves her" (on the
    /// '--- GAME ---' object; its nodes are in Code/Graphs/SeaNodes.cs):
    ///   "off_line"   wind and tide have set her well off her line                 (Adrift)
    ///   "lee_shore"  she is set down on a shore she has not the way on to keep clear of   (Adrift)
    ///   "adrift"     lying to in water too deep to anchor in, she is carried       (Adrift)
    ///   "lost"       the steersman has run his distance and cannot make out his landfall: she lies to (VoyagePlanner)
    ///   "rough"      she comes into rough water: a race, a bar, a shoal, an open cape   (RoughWaters)
    ///   "relaid"     set well off her line along a coast, the steersman has laid her course again himself: the log
    ///                says so and the clock runs on (10 Oct 2026)                              (Adrift)
    ///   "cleared"    the air has cleared over a reckoning lost in it, and she sails on (10 Oct 2026)   (Adrift)
    /// The graph says what the log says and whether the clock drops back. Where no graph listens the code does as it
    /// always did: the log is told the words, and the clock drops back where the moment calls for it. The flow runs at
    /// once (nothing in it waits). What moves her, and how much, is Adrift's and the rule graphs' (GameRules).
    /// </summary>
    public static class SeaMoments
    {
        /// <summary>Set by the graphs' side (Kapman.Graphs): runs the graph's flow for the moment that is on and returns
        /// true; false where no graph listens for it.</summary>
        public static Func<bool> Flow;
        /// <summary>How many moments a graph has run since the game began, and how many the code ran itself.</summary>
        public static int ByGraph { get; private set; }
        public static int ByCode { get; private set; }
        /// <summary>THE MOMENT THAT IS ON (the graph's event reads these): which it is; the words the game has for it
        /// (PLACEHOLDER WORDS, the user's to write: Adrift.cs, RoughWaters.cs); whether the clock drops back for it,
        /// and why.</summary>
        public static string What { get; private set; } = "";
        public static string Words { get; private set; } = "";
        public static bool Drops { get; private set; }
        public static ClockReason Why { get; private set; }

        /// <summary>The moment's short form for the log (10 Oct 2026): a few words, the number at the line's end, and the
        /// box that comes up over the line ("" and null: none, and the words are said as they are).</summary>
        public static string Short { get; private set; } = "";
        public static string Figure { get; private set; } = "";
        public static LogBox Box { get; private set; }

        /// <summary>A moment comes: the graph's flow where one listens, else the log is told and the clock drops back.</summary>
        public static void Raise(string what, string words, ClockReason? drop = null) => Raise(what, words, drop, "", "", null);

        /// <summary>The same, with the moment's short form for the log: its line, the number at the line's end, and its
        /// box (null: the words themselves, a sentence a row). The graph's node "Log: say" says the moment's words as
        /// before: the short form goes with them (<see cref="Told"/>).</summary>
        public static void Raise(string what, string words, ClockReason? drop, string shortLine, string figure, LogBox box)
        {
            What = what ?? "";
            Words = words ?? "";
            Short = shortLine ?? "";
            Figure = figure ?? "";
            Box = box;
            Drops = drop.HasValue;
            Why = drop ?? ClockReason.Pressing;
            bool byGraph = false;
            try
            {
                byGraph = Flow != null && Flow();
            }
            catch (Exception e)
            {
                Debug.LogError("Kapman, the sea: the graph's flow for the moment \"" + What + "\" failed (" + e.Message + ").");
                byGraph = true;
            }
            if (byGraph)
            {
                ByGraph++;
                return;
            }
            ByCode++;
            Told(Words);
            if (Drops)
                ClockDropsBack(Why);
        }

        /// <summary>The log is told a plain fact (the notices line).</summary>
        public static void Told(string words)
        {
            if (string.IsNullOrEmpty(words))
                return;
            // (the moment's own words go to the log in its short form, and are kept whole in its box)
            if (Short.Length > 0 && words == Words)
                Voyage.Say(words, Short, Figure, Box ?? new LogBox().SayEach(words));
            else
                Voyage.Say(words);
        }

        /// <summary>The clock drops back by itself, for a reason the player can switch off in the settings.</summary>
        public static void ClockDropsBack(ClockReason why) => ClockDrops.Raise(why);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ByGraph = ByCode = 0;
            What = Words = Short = Figure = "";
            Box = null;
            Drops = false;
        }
    }
}
