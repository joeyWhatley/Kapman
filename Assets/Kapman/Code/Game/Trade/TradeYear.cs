using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One month of a trade region's year (trade_year_793.json): what comes to its markets, what they want, the
    /// fairs and feasts, whether its water is closed, and one plain line.</summary>
    public sealed class TradeMonth
    {
        /// <summary>Good ids: what comes to market this month (plenty, and cheap), and what is wanted (dear).</summary>
        public readonly List<string> comes = new List<string>(), wanted = new List<string>();
        /// <summary>Fairs, markets and things (the gatherings that draw buyers), and the feasts kept.</summary>
        public readonly List<string> fairs = new List<string>(), feasts = new List<string>();
        /// <summary>Why no ship gets there this month ("" when the water is open).</summary>
        public string closed = "";
        /// <summary>The month in one plain line ("Slaughter month: salt is dear.").</summary>
        public string line = "";

        public bool Closed => closed.Length > 0;
        public bool Comes(string good) => good != null && comes.Contains(good);
        public bool Wants(string good) => good != null && wanted.Contains(good);
    }

    /// <summary>
    /// THE TRADING YEAR BY REGION (Data/trade_year_793.json: Design/research_trade_seasons_and_hints.md section 2 as data):
    /// for each trade region of the goods table and each month, what comes to market, what is wanted, the fairs and
    /// feasts, the closed water, and one plain line. What every merchant of the time carried in his head: the word on an
    /// item (<see cref="Known.Word(GoodDef, PortDef, WordAsk)"/>) reads it for the season, and the map's port card shows
    /// the month's line. It only tells: nothing here moves a price (the goods table's cheap and dear months and
    /// seasons_793.json do that).
    /// </summary>
    public static class TradeYear
    {
        static readonly Dictionary<string, TradeMonth[]> regions = new Dictionary<string, TradeMonth[]>(StringComparer.Ordinal);
        static readonly Dictionary<string, string> names = new Dictionary<string, string>(StringComparer.Ordinal);
        static readonly List<string> faults = new List<string>();
        static bool loaded;

        /// <summary>The regions the table has, by id.</summary>
        public static IReadOnlyCollection<string> Regions
        {
            get
            {
                EnsureLoaded();
                return regions.Keys;
            }
        }

        /// <summary>What was wrong with the table when it was read (a region without twelve months, a month that is not
        /// an object); empty when it is whole.</summary>
        public static IReadOnlyList<string> Faults
        {
            get
            {
                EnsureLoaded();
                return faults;
            }
        }

        /// <summary>A region's month (1-12), or null when the table has no such region.</summary>
        public static TradeMonth Of(string region, int month)
        {
            EnsureLoaded();
            if (region == null || !regions.TryGetValue(region, out var months))
                return null;
            return months[Mathf.Clamp(month, 1, 12) - 1];
        }

        /// <summary>A region's name as the table has it ("" when it has none).</summary>
        public static string Name(string region)
        {
            EnsureLoaded();
            return region != null && names.TryGetValue(region, out var n) ? n : "";
        }

        public static void Load(TextAsset json) => Load(json != null ? json.text : null);

        /// <summary>Reads trade_year_793.json. A month the table leaves out reads as an empty one (and is a fault).</summary>
        public static void Load(string text)
        {
            loaded = true;
            regions.Clear();
            names.Clear();
            faults.Clear();
            if (string.IsNullOrEmpty(text))
                return;
            object root;
            try
            {
                root = Json.Parse(text);
            }
            catch (FormatException e)
            {
                faults.Add("trade_year_793.json: " + e.Message);
                return;
            }
            foreach (var kv in Json.Obj(root, "regions") ?? new Dictionary<string, object>())
            {
                var months = new TradeMonth[12];
                var table = Json.Obj(kv.Value, "months");
                for (int m = 1; m <= 12; m++)
                {
                    var t = months[m - 1] = new TradeMonth();
                    var o = Json.Obj(table, m.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    if (o == null)
                    {
                        faults.Add($"trade_year_793.json: {kv.Key} has no month {m}.");
                        continue;
                    }
                    Strings(Json.Arr(o, "comes"), t.comes);
                    Strings(Json.Arr(o, "wanted"), t.wanted);
                    Strings(Json.Arr(o, "fairs"), t.fairs);
                    Strings(Json.Arr(o, "feasts"), t.feasts);
                    t.closed = Json.Str(o, "closed");
                    t.line = Json.Str(o, "line");
                }
                regions[kv.Key] = months;
                names[kv.Key] = Json.Str(kv.Value, "name");
            }
        }

        static void Strings(List<object> from, List<string> into)
        {
            foreach (var x in from ?? new List<object>())
                if (x is string s && s.Length > 0)
                    into.Add(s);
        }

        /// <summary>Reads the table from the Data folder the first time it is needed, if nobody loaded it.</summary>
        static void EnsureLoaded()
        {
            if (loaded)
                return;
            loaded = true;
            Load(GameFiles.Find("trade_year_793"));
        }

        // Enter Play Mode runs without a domain reload: each play reads the table again
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            loaded = false;
            regions.Clear();
            names.Clear();
            faults.Clear();
        }
    }
}
