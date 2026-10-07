using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>A hard winter the chronicle records: the winter it belongs to (the year it ends in) and the chronicle's
    /// region ids it touched.</summary>
    public sealed class ChronicleWinter
    {
        public string id = "";
        /// <summary>The winter of (year - 1)/year: a winter firing in November or December belongs to the next year.</summary>
        public int year;
        public readonly List<string> regions = new List<string>();
    }

    /// <summary>
    /// THE YEAR STATE (Design/wildlife_and_seasons.md 2.7-2.8; GAME_PLAN 2.4.4: one die for the year) - a year's hard
    /// winter by region, the mast, the herring stock, and each species' crash and recovery after hard winters, all from
    /// the world's seed and the year, so a saved game and another player get the same years. The chronicle wins: a winter
    /// it records as hard is hard (HW = 1) in the regions it touched; the dice only fill the years it says nothing of.
    /// The economy's murrain and bad-herring shocks read HardWinter and Stock here instead of rolling their own.
    /// </summary>
    public sealed class YearState
    {
        public readonly int seed;
        public readonly Climate climate;
        public readonly List<ChronicleWinter> winters;

        // each species' crash state by region, year by year from climate.firstYear (built lazily, extended as asked)
        readonly Dictionary<string, List<double>> kCache = new Dictionary<string, List<double>>();

        public YearState(int seed, Climate climate, List<ChronicleWinter> winters)
        {
            this.seed = seed;
            this.climate = climate ?? new Climate();
            this.winters = winters ?? new List<ChronicleWinter>();
        }

        /// <summary>
        /// The chronicle's hard winters (kind "winter") from chronicle_793.json's text. An entry fired from day 274
        /// (October) of a year on is the winter ending in the next year; one fired up to day 120 (April) is the winter
        /// ending that year. Entries of other kinds are not winters here (famines and floods are the economy's).
        /// </summary>
        public static List<ChronicleWinter> WintersFrom(string chronicleJson)
        {
            var list = new List<ChronicleWinter>();
            var root = WildlifeJson.Root(chronicleJson, "chronicle_793.json");
            var entries = Json.Arr(root, "entries") ?? throw new FormatException("chronicle_793.json: 'entries' is missing");
            foreach (var e in entries)
            {
                if (Json.Str(e, "kind") != "winter")
                    continue;
                string fire = Json.Str(e, "fire");
                if (!TryDate(fire, out int y, out int doy))
                    throw new FormatException("chronicle_793.json: the winter " + Json.Str(e, "id") + " has no readable fire date: " + fire);
                var w = new ChronicleWinter { id = Json.Str(e, "id"), year = doy >= 274 ? y + 1 : y };
                w.regions.AddRange(WildlifeJson.Strings(e, "regions"));
                list.Add(w);
            }
            return list;
        }

        /// <summary>"YYYY-MM-DD" or "YYYY-MM" as a year and a Julian day of the year.</summary>
        static bool TryDate(string s, out int year, out int dayOfYear)
        {
            year = dayOfYear = 0;
            if (string.IsNullOrEmpty(s))
                return false;
            var parts = s.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out year)
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int month) || month < 1 || month > 12)
                return false;
            int day = 1;
            if (parts.Length >= 3 && !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
                return false;
            int[] lengths = { 31, year % 4 == 0 ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            dayOfYear = day;
            for (int m = 0; m < month - 1; m++)
                dayOfYear += lengths[m];
            return true;
        }

        /// <summary>The chronicle's hard winter that ended in this year and touched the region, or null.</summary>
        public ChronicleWinter ChronicleWinterOf(int year, Region region)
        {
            if (region == null)
                return null;
            foreach (var w in winters)
                if (w.year == year && region.MatchesChronicle(w.regions))
                    return w;
            return null;
        }

        /// <summary>
        /// The hard-winter index HW (0..1) of the winter ending in this year, in a region: 1 when the chronicle records
        /// it there; else the year's die over the threshold of the climate (cooler years harder: decision 17), as a ramp.
        /// The die is the same for every region (a hard winter is a broad thing); only the chronicle tells regions apart.
        /// </summary>
        public double HardWinter(int year, Region region)
        {
            if (ChronicleWinterOf(year, region) != null)
                return 1.0;
            double m = climate.WarmIndex(year);
            double threshold = climate.hardWinterCold + (climate.hardWinterWarm - climate.hardWinterCold) * m;
            double die = Dice.Roll(seed, year, "winter");
            return Presence.Clamp01((die - threshold) / Math.Max(1e-6, climate.hardWinterWidth));
        }

        /// <summary>The mast (2.7): 1.5 in a good year (three in ten), 0.3 in a failed one (one in four), else 1.</summary>
        public double Mast(int year)
        {
            double die = Dice.Roll(seed, year, "mast");
            return die < 0.3 ? 1.5 : die < 0.55 ? 0.3 : 1.0;
        }

        /// <summary>A run's stock in a year (2.8): 0.3 in a failed year (one in eight), else 1; "mast" gives the mast; an
        /// empty component is always 1.</summary>
        public double Stock(string component, int year)
        {
            if (string.IsNullOrEmpty(component))
                return 1.0;
            if (component == "mast")
                return Mast(year);
            return Dice.Roll(seed, year, component) < 0.125 ? 0.3 : 1.0;
        }

        /// <summary>
        /// One year of the crash-and-recovery rule (2.7), linear so it can be tested exactly: after a hard winter
        /// (HW > 0.5) the stock k loses crash * HW of itself and the whole shortfall is to be made good in recoverYears;
        /// in any other year it climbs back by that shortfall's share, up to 1.
        /// </summary>
        public static void Step(ref double k, ref double shortfall, double hw, double crash, double recoverYears)
        {
            if (hw > 0.5 && crash > 0.0)
            {
                k *= 1.0 - crash * hw;
                shortfall = 1.0 - k;
                return;
            }
            if (k < 1.0)
            {
                k = Math.Min(1.0, k + shortfall / Math.Max(1.0, recoverYears));
                if (k >= 1.0)
                    shortfall = 0.0;
            }
        }

        /// <summary>A species' crash state K (0..1) in a region in a year, the winters since climate.firstYear applied in
        /// turn; 1 before that year.</summary>
        public double K(Species s, Region region, int year)
        {
            if (s == null || s.crash <= 0.0 || year < climate.firstYear)
                return 1.0;
            string key = s.id + "|" + (region != null ? region.id : "");
            if (!kCache.TryGetValue(key, out var ks))
            {
                ks = new List<double>();
                kCache[key] = ks;
            }
            // carry on from the last year worked out
            double k = ks.Count > 0 ? ks[ks.Count - 1] : 1.0;
            double shortfall = shortfalls.TryGetValue(key, out var had) ? had : 0.0;
            for (int y = climate.firstYear + ks.Count; y <= year; y++)
            {
                Step(ref k, ref shortfall, HardWinter(y, region), s.crash, s.recoverYears);
                ks.Add(k);
            }
            shortfalls[key] = shortfall;
            return ks[year - climate.firstYear];
        }

        // the shortfall still to recover after the last cached year, per species and region
        readonly Dictionary<string, double> shortfalls = new Dictionary<string, double>();

        /// <summary>Forgets the cached crash states (after the seed or the chronicle changed).</summary>
        public void Clear()
        {
            kCache.Clear();
            shortfalls.Clear();
        }
    }
}
