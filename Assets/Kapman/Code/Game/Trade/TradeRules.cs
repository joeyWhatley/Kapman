using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>A coin of the time, or weighed silver; its silver in deniers (Data/trade_rules.json).</summary>
    public sealed class CoinDef
    {
        public string id, name, shortName, note;
        public float silverGrams;
        /// <summary>One coin's silver, in deniers of 1.7 g (hacksilver: one gram's).</summary>
        public float Deniers => silverGrams / 1.7f;
    }

    /// <summary>A money zone: which coin is good money there, how it reckons, what it costs to pay in other silver,
    /// and its toll (Data/trade_rules.json).</summary>
    public sealed class ZoneDef
    {
        public string id, name, coin, unit, note;
        /// <summary>One unit of its reckoning, in deniers (an English penny 0.706, a gram of silver 0.588).</summary>
        public float unitDeniers = 1f;
        /// <summary>The share lost paying in other coin or silver (to the mint, or nicked and tested on the scales).</summary>
        public float foreignLoss, toll;
        public readonly List<string> regions = new List<string>();

        /// <summary>Deniers as this zone reckons them ("12 d", "17 p", "20 g silver").</summary>
        public string Show(double deniers)
        {
            double v = deniers / Mathf.Max(0.0001f, unitDeniers);
            return (v >= 10 ? v.ToString("0") : v.ToString("0.#")) + " " + unit;
        }
    }

    /// <summary>A law or a rule of the Church on a kind of good, where and when it holds.</summary>
    public sealed class LawDef
    {
        public string law, rule, note;
        public int from, to;
        public readonly List<string> zones = new List<string>();
        public bool Holds(string zone, int year) => zones.Contains(zone) && (from == 0 || from <= year) && (to == 0 || to >= year);
    }

    /// <summary>
    /// THE RULES OF TRADE (Data/trade_rules.json, read once): the coins, the money zones with their tolls, the laws,
    /// how markets answer supply and demand, and what a merchant starts with. All prices are reckoned in deniers of
    /// silver; each zone shows them its own way.
    /// </summary>
    public sealed class TradeRules
    {
        public readonly Dictionary<string, CoinDef> coins = new Dictionary<string, CoinDef>();
        public readonly List<ZoneDef> zones = new List<ZoneDef>();
        public readonly List<LawDef> laws = new List<LawDef>();
        readonly Dictionary<string, ZoneDef> zoneOfRegion = new Dictionary<string, ZoneDef>();

        // the market
        public float spread = 0.2f, stockValue = 150f, stockMin = 1f, stockMax = 120f, regen = 0.12f, elasticity = 0.4f;
        public float priceMin = 0.6f, priceMax = 2.2f, appetiteValue = 120f, glutElasticity = 0.5f, glutFade = 0.12f;
        public float unwanted = 0.6f, cheap = 0.85f, dear = 1.2f;
        /// <summary>The share of a shock's dearness (a famine, a raid, a fair) that fades each day.</summary>
        public float shockFade = 0.03f;

        // the start
        public readonly Dictionary<string, double> startCoins = new Dictionary<string, double>();
        public float startDebt = 1920f;                                    // (eight pounds: ruling 30 of 7 Oct 2026; it was 480)
        public string creditor = "creditor.count";
        /// <summary>The days to the due day when no calendar is known (tests, a level with no clock).</summary>
        public int dueDays = 365;
        /// <summary>The day of the year the debt falls due: Michaelmas, 29 September (ruling 14 of 7 Oct 2026: "the debt
        /// due on a day"). A month of 0: after <see cref="dueDays"/> instead.</summary>
        public int dueMonth = 9, dueDay = 29;
        public string dueName = "Michaelmas";
        /// <summary>The month and day the world's traders and markets start (1 April: they have been at work for weeks
        /// before the player's first morning, so no market is untouched). A month of 0: they start with the player.</summary>
        public int worldFromMonth = 4, worldFromDay = 1;

        /// <summary>Whole days from the world's start day back from the clock's start (0 when the game begins on or
        /// before it, or when there is none): 30 for a game begun on 1 May 793.</summary>
        public int PreRunDays(int year, int startDay)
        {
            if (worldFromMonth < 1 || worldFromMonth > 12 || worldFromDay < 1)
                return 0;
            int doy = worldFromDay;
            for (int m = 1; m < worldFromMonth; m++)
                doy += Kapman.World.Calendar.DaysIn(year, m);
            return System.Math.Max(0, startDay - doy);
        }

        public static TradeRules Current { get; private set; } = new TradeRules();

        /// <summary>The zone a trade region's money and laws are of (a region with none: the first zone).</summary>
        public ZoneDef ZoneOf(string region) =>
            region != null && zoneOfRegion.TryGetValue(region, out var z) ? z : zones.Count > 0 ? zones[0] : null;

        public CoinDef Coin(string id) => id != null && coins.TryGetValue(id, out var c) ? c : null;

        /// <summary>The laws that hold on a good where and when (none for most goods).</summary>
        public List<LawDef> LawsOn(GoodDef good, ZoneDef zone, int year)
        {
            var found = new List<LawDef>();
            if (good == null || string.IsNullOrEmpty(good.law) || zone == null)
                return found;
            foreach (var l in laws)
                if (l.law == good.law && l.Holds(zone.id, year))
                    found.Add(l);
            return found;
        }

        public static TradeRules Load(TextAsset json)
        {
            var r = new TradeRules();
            if (json != null)
            {
                var root = Json.Parse(json.text);
                foreach (var o in Json.Arr(root, "coins") ?? new List<object>())
                {
                    var c = new CoinDef
                    {
                        id = Json.Str(o, "id"), name = Json.Str(o, "name"), shortName = Json.Str(o, "short"),
                        note = Json.Str(o, "note"), silverGrams = (float)Json.Num(o, "silver_g", 1.7),
                    };
                    if (!string.IsNullOrEmpty(c.id))
                        r.coins[c.id] = c;
                }
                foreach (var o in Json.Arr(root, "zones") ?? new List<object>())
                {
                    var z = new ZoneDef
                    {
                        id = Json.Str(o, "id"), name = Json.Str(o, "name"), coin = Json.Str(o, "coin"),
                        unit = Json.Str(o, "unit", "d"), note = Json.Str(o, "note"),
                        unitDeniers = (float)Json.Num(o, "unit_d", 1), foreignLoss = (float)Json.Num(o, "foreign_loss"),
                        toll = (float)Json.Num(o, "toll"),
                    };
                    foreach (var reg in Json.Arr(o, "regions") ?? new List<object>())
                        if (reg is string s)
                        {
                            z.regions.Add(s);
                            r.zoneOfRegion[s] = z;
                        }
                    r.zones.Add(z);
                }
                foreach (var o in Json.Arr(root, "laws") ?? new List<object>())
                {
                    var l = new LawDef
                    {
                        law = Json.Str(o, "law"), rule = Json.Str(o, "rule"), note = Json.Str(o, "note"),
                        from = (int)Json.Num(o, "from"), to = (int)Json.Num(o, "to"),
                    };
                    foreach (var z in Json.Arr(o, "zones") ?? new List<object>())
                        if (z is string s)
                            l.zones.Add(s);
                    r.laws.Add(l);
                }
                var m = Json.Obj(root, "market");
                if (m != null)
                {
                    r.spread = (float)Json.Num(m, "spread", r.spread);
                    r.stockValue = (float)Json.Num(m, "stock_value", r.stockValue);
                    r.stockMin = (float)Json.Num(m, "stock_min", r.stockMin);
                    r.stockMax = (float)Json.Num(m, "stock_max", r.stockMax);
                    r.regen = (float)Json.Num(m, "regen", r.regen);
                    r.elasticity = (float)Json.Num(m, "elasticity", r.elasticity);
                    r.priceMin = (float)Json.Num(m, "price_min", r.priceMin);
                    r.priceMax = (float)Json.Num(m, "price_max", r.priceMax);
                    r.appetiteValue = (float)Json.Num(m, "appetite_value", r.appetiteValue);
                    r.glutElasticity = (float)Json.Num(m, "glut_elasticity", r.glutElasticity);
                    r.glutFade = (float)Json.Num(m, "glut_fade", r.glutFade);
            r.shockFade = (float)Json.Num(m, "shock_fade", r.shockFade);
                    r.unwanted = (float)Json.Num(m, "unwanted", r.unwanted);
                    r.cheap = (float)Json.Num(m, "cheap", r.cheap);
                    r.dear = (float)Json.Num(m, "dear", r.dear);
                }
                var st = Json.Obj(root, "start");
                if (st != null)
                {
                    foreach (var kv in Json.Obj(st, "coins") ?? new Dictionary<string, object>())
                        if (kv.Value is double v)
                            r.startCoins[kv.Key] = v;
                    r.startDebt = (float)Json.Num(st, "debt", r.startDebt);
                    r.creditor = Json.Str(st, "creditor", r.creditor);
                    r.dueDays = (int)Json.Num(st, "due_days", r.dueDays);
                    r.dueMonth = (int)Json.Num(st, "due_month", r.dueMonth);
                    r.dueDay = (int)Json.Num(st, "due_day", r.dueDay);
                    r.dueName = Json.Str(st, "due_name", r.dueName);
                    // "04-01": the month and the day; empty or unreadable: no pre-run
                    var from = (Json.Str(st, "world_from", $"{r.worldFromMonth:00}-{r.worldFromDay:00}") ?? "").Split('-');
                    if (from.Length == 2 && int.TryParse(from[0], out int fromMonth) && int.TryParse(from[1], out int fromDay))
                    {
                        r.worldFromMonth = fromMonth;
                        r.worldFromDay = fromDay;
                    }
                    else
                        r.worldFromMonth = 0;
                }
            }
            Current = r;
            return r;
        }
    }
}
