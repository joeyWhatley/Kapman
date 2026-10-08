using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>
    /// THE LANDINGS (the critic's play of 8 Oct 2026, B4, taken up by the user the same day: "the first voyage ends and
    /// nothing marks it"). Each time she makes fast at a port after a passage, a slip is kept: where she came from and
    /// how long it took, what is in the purse, what the hold cost and what it would fetch here, and what is owed. When
    /// she casts off again the slip is closed with what the purse held then, so a stay can be read as a sum: came with
    /// so much, left with so much. The frame says the slip in the log and keeps them all in the Journey window.
    /// Pure rules: the frame feeds it where she lies and the figures of the moment; the tests feed it numbers.
    /// </summary>
    public static class Landings
    {
        public sealed class Landing
        {
            /// <summary>The port she cast off from ("" not known) and the port she made.</summary>
            public string from = "", to = "";
            /// <summary>The game days she cast off and landed at, and the landing's date in words.</summary>
            public double leftDay, day;
            public string date = "";
            /// <summary>In the purse at the landing; in the purse when she cast off again (under 0: she lies there still).</summary>
            public double silver, away = -1.0;
            /// <summary>What the hold cost her, and what it would fetch at this port, toll taken.</summary>
            public double cost, fetch;
            /// <summary>What was owed at the landing, and the days left to its day.</summary>
            public double debt, daysLeft;

            public double Took => Math.Max(0.0, day - leftDay);
        }

        /// <summary>The figures of the moment she lands, which the rules here do not reckon themselves.</summary>
        public struct Figures
        {
            public string date;
            public double silver, cost, fetch, debt, daysLeft;
        }

        public const int Keep = 60;
        /// <summary>Out and straight back in under this many days is no passage (she lay to off the quay).</summary>
        public const double NoPassage = 0.03;

        public static IReadOnlyList<Landing> All => all;
        /// <summary>Counts up with every change (a panel's stamp).</summary>
        public static int Version { get; private set; }
        /// <summary>The last landing, while she still lies there (null at sea, and before the first).</summary>
        public static Landing Here => all.Count > 0 && all[all.Count - 1].away < 0.0 && all[all.Count - 1].to == lying ? all[all.Count - 1] : null;

        static readonly List<Landing> all = new List<Landing>();
        static bool looked;
        static string lying = "", leftFrom = "";
        static double leftDay;

        /// <summary>Where she lay at the last look ("" at sea, and before the first look).</summary>
        public static string Lying => lying;

        /// <summary>
        /// One look: the port she lies at now (null or "": none), the day, and what the purse holds. Says the landing
        /// made just now, or null. `figures` is asked only then. Lying at a port when the first look comes (a new game
        /// at home, a game loaded) is no landing.
        /// </summary>
        public static Landing Look(string port, double day, double silver, Func<Figures> figures)
        {
            port = port ?? "";
            if (!looked)
            {
                looked = true;
                lying = port;
                return null;
            }
            if (port == lying)
                return null;
            Landing made = null;
            if (lying.Length > 0)
            {
                // she has cast off: the stay is closed, and the first jobs' list is done with
                var last = all.Count > 0 ? all[all.Count - 1] : null;
                if (last != null && last.to == lying && last.away < 0.0)
                    last.away = silver;
                leftFrom = lying;
                leftDay = day;
                FirstJobs.Sailed();
                Version++;
            }
            if (port.Length > 0)
            {
                var last = all.Count > 0 ? all[all.Count - 1] : null;
                if (port == leftFrom && day - leftDay < NoPassage)
                {
                    if (last != null && last.to == port)
                        last.away = -1.0;                                 // (never away: the stay goes on)
                }
                else
                {
                    var f = figures != null ? figures() : new Figures { silver = silver };
                    made = new Landing
                    {
                        from = leftFrom, to = port, leftDay = leftFrom.Length > 0 ? leftDay : day, day = day, date = f.date ?? "",
                        silver = f.silver, cost = f.cost, fetch = f.fetch, debt = f.debt, daysLeft = f.daysLeft,
                    };
                    all.Add(made);
                    if (all.Count > Keep)
                        all.RemoveRange(0, all.Count - Keep);
                }
                Version++;
            }
            lying = port;
            return made;
        }

        // ------------------------------------------------------------------ the saved game

        static Dictionary<string, object> Capture()
        {
            var list = new List<object>();
            foreach (var l in all)
                list.Add(new Dictionary<string, object>
                {
                    ["from"] = l.from, ["to"] = l.to, ["left_day"] = l.leftDay, ["day"] = l.day, ["date"] = l.date, ["silver"] = l.silver,
                    ["away"] = l.away, ["cost"] = l.cost, ["fetch"] = l.fetch, ["debt"] = l.debt, ["days_left"] = l.daysLeft,
                });
            return new Dictionary<string, object> { ["lying"] = lying, ["left_from"] = leftFrom, ["left_day"] = leftDay, ["all"] = list };
        }

        static void Restore(object saved)
        {
            Clear();
            if (!(saved is Dictionary<string, object> d))
                return;                                                   // (a save from before the slips: the first look sets where she lies)
            looked = true;
            lying = Json.Str(d, "lying");
            leftFrom = Json.Str(d, "left_from");
            leftDay = Json.Num(d, "left_day", 0.0);
            foreach (var o in Json.Arr(d, "all") ?? new List<object>())
                all.Add(new Landing
                {
                    from = Json.Str(o, "from"), to = Json.Str(o, "to"), leftDay = Json.Num(o, "left_day", 0.0), day = Json.Num(o, "day", 0.0),
                    date = Json.Str(o, "date"), silver = Json.Num(o, "silver", 0.0), away = Json.Num(o, "away", -1.0), cost = Json.Num(o, "cost", 0.0),
                    fetch = Json.Num(o, "fetch", 0.0), debt = Json.Num(o, "debt", 0.0), daysLeft = Json.Num(o, "days_left", 0.0),
                });
            Version++;
        }

        /// <summary>Nothing kept and nothing looked at yet (a new game; the tests).</summary>
        public static void Clear()
        {
            all.Clear();
            looked = false;
            lying = leftFrom = "";
            leftDay = 0.0;
            Version++;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            Clear();
            Version = 0;
            ContentSave.Register("landings", Capture, Restore);
        }
    }
}
