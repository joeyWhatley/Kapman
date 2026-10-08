using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE OARS (the alpha's row 3: "sail or oars"; the user's ruling 8 of 7 Oct 2026 names the order: "Oars (out oars,
    /// or sail)"). Left to herself she sails, and where the sail gives less the men take an easy stroke by themselves
    /// (Travel.Speed: as she has always gone, and as the world's other ships go). By the order OUT OARS the sail comes
    /// down and they row HARD: her class's full speed under oars whatever the wind (less into a head wind, nothing in
    /// a gale), for as long as their strength lasts: her class's rowing hours. It costs bread and strength: a full
    /// pull eats half a day's food on top of the day's, and the hours come back only with rest. When the men are
    /// spent the oars come in by themselves. Worth it against a foul wind, in a calm, to make a day or a tide, or to
    /// pull clear of a sail that is closing.
    /// THE NUMBERS ARE FIRST GUESSES (voyage_rules.json "oars"), to be tuned in play.
    /// </summary>
    public static class Oars
    {
        /// <summary>The order stands: the sail is down and the men row hard.</summary>
        public static bool Out { get; private set; }
        /// <summary>Hours of hard rowing taken out of the men and not yet rested back (0: they are fresh).</summary>
        public static double Spent { get; private set; }
        /// <summary>Changes with the order (a window that shows it is drawn again).</summary>
        public static int Version { get; private set; }

        static double breadOwed;                                         // days of food owed for the rowing, till the day is reckoned

        /// <summary>The hours of hard rowing in a fresh crew: her class's (0 for a boat with no oars).</summary>
        public static double Whole(ShipClassDef c) => c != null && c.oarsKn > 0f ? Math.Max(0.0, c.rowingHours) : 0.0;

        /// <summary>The hours of hard rowing the men have in them now.</summary>
        public static double Left(ShipClassDef c) => Math.Max(0.0, Whole(c) - Spent);

        /// <summary>Hours of rest until the order can be given again (0: it can).</summary>
        public static double RestWanted(ShipClassDef c)
        {
            double whole = Whole(c), least = Math.Min(whole, Voyage.Rules.oarsLeastHours);
            if (whole <= 0.0 || Left(c) >= least)
                return 0.0;
            return (least - Left(c)) * Math.Max(1.0, Voyage.Rules.oarsRestHours) / whole;
        }

        /// <summary>May the oars be ordered out now? If not, why, in a sentence.</summary>
        public static bool Can(ShipClassDef c, bool moored, bool lyingTo, out string why)
        {
            why = "";
            if (c == null || c.oarsKn <= 0f)
                why = "She has no oars.";
            else if (moored)
                why = "She is made fast. The oars are for the way.";
            else if (lyingTo)
                why = "A gale. She lies to, and no oar will keep her head up.";
            else if (RestWanted(c) > 0.0)
                why = "The men have not their strength back yet: " + Span(RestWanted(c)) + " more.";
            return why.Length == 0;
        }

        /// <summary>The order given (true: out oars) or taken back. Returns what is said of it: the order's own line,
        /// or why it cannot be taken; "" when nothing changed.</summary>
        public static string Order(bool on, ShipClassDef c, bool moored, bool lyingTo)
        {
            if (on == Out)
                return "";
            if (on && !Can(c, moored, lyingTo, out string why))
                return why;
            Out = on;
            Version++;
            bool sail = c == null || c.hasSail;
            return on ? (sail ? "The sail comes down and the men take to the oars." : "The men bend to the oars.")
                      : (sail ? "The oars come in and the sail goes up." : "The men ease to their own stroke.");
        }

        /// <summary>Her speed made good under the order (m/s): her class's speed under oars, less into a head wind (half
        /// of it dead into a gale's wind; nothing lost with the wind abeam or aft). In a gale she lies to.</summary>
        public static double Speed(ShipClassDef c, double courseDeg, double windFrom, double windSpeed, out bool sheltering)
        {
            sheltering = windSpeed > 17.0;
            if (sheltering || c == null || c.oarsKn <= 0f)
                return 0.0;
            double off = Math.Abs(((courseDeg - windFrom) % 360.0 + 540.0) % 360.0 - 180.0);   // 0 = into the wind
            double head = Math.Max(0.0, Math.Cos(off * Math.PI / 180.0));
            double lost = Voyage.Rules.oarsHeadWind * head * Math.Min(1.0, Math.Max(0.0, windSpeed) / 17.0);
            return c.oarsKn * (1.0 - lost) * SimUnits.KnotMs;
        }

        /// <summary>
        /// The clock has run `hours`. `rowing`: the order stands and she is under way by it. An hour rowed is an hour
        /// out of the men, and its share of the full pull's bread; any other hour they rest, and the whole comes back
        /// in the rule's hours of rest. Made fast, the oars are in. Spent, the oars come in by themselves, and it is said.
        /// </summary>
        public static void Step(double hours, bool rowing, bool moored, ShipClassDef c)
        {
            if (!(hours > 0.0))
                return;
            var r = Voyage.Rules;
            double whole = Whole(c);
            if (Out && (moored || whole <= 0.0))
            {
                Out = false;
                Version++;
            }
            if (Out && rowing)
            {
                double rowed = Math.Min(hours, whole - Spent);
                Spent = Math.Min(whole, Spent + hours);
                breadOwed += Math.Max(0.0, rowed) / whole * r.oarsBread;
                if (Spent >= whole - 1e-9)
                {
                    Out = false;
                    Version++;
                    Voyage.Affect(0.0, r.oarsSpentMorale);
                    Voyage.Say(c.hasSail ? "The men are spent. The oars come in and the sail goes up." : "The men are spent, and ease to their own stroke.");
                }
            }
            else if (Spent > 0.0 && whole > 0.0)
                Spent = Math.Max(0.0, Spent - hours * whole / Math.Max(1.0, r.oarsRestHours));
        }

        /// <summary>The food the rowing has cost since the last day's reckoning, in days of the crew's food: taken once,
        /// by the day (Voyage.Day).</summary>
        public static double TakeBread()
        {
            double owed = breadOwed;
            breadOwed = 0.0;
            return owed;
        }

        /// <summary>A span of hours in plain words, to the hour: "about an hour", "about 5 hours".</summary>
        public static string Span(double hours)
        {
            int h = Math.Max(1, (int)Math.Ceiling(hours - 1e-9));
            return h == 1 ? "about an hour" : "about " + h + " hours";
        }

        // ------------------------------------------------------------------ the saved game

        public static object Capture() => new Dictionary<string, object> { ["out"] = Out, ["spent"] = Spent, ["bread"] = breadOwed };

        /// <summary>The oars of a loaded game (null, an older save: in, the men fresh).</summary>
        public static void Restore(object saved)
        {
            var d = saved as Dictionary<string, object>;
            Out = d != null && Json.Bool(d, "out");
            Spent = d != null ? Math.Max(0.0, Json.Num(d, "spent")) : 0.0;
            breadOwed = d != null ? Math.Max(0.0, Json.Num(d, "bread")) : 0.0;
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too: the oars in, the men fresh
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Out = false;
            Spent = 0.0;
            breadOwed = 0.0;
            Version = 0;
            ContentSave.Register("oars", Capture, Restore);
        }
    }
}
