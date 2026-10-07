using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One good in one market: what's on offer, what the market settles back to, and how much it has been
    /// sold lately beyond what it uses.</summary>
    public sealed class MarketGood
    {
        public GoodDef good;
        /// <summary>Units on offer now.</summary>
        public float stock;
        /// <summary>The stock the market settles back to day by day over the year (0: it doesn't make the good; it only has
        /// what's brought). A season of making moves what it settles toward on the day (Seasons); the price still reads
        /// scarcity against this.</summary>
        public float usual;
        /// <summary>Units sold to it lately beyond what it uses (it pays less while there's a glut).</summary>
        public float glut;
        /// <summary>Units it takes at a good price before a glut sets in.</summary>
        public float appetite;
        /// <summary>How strongly it makes the good, and how keenly it wants it (from the goods table, by its region).</summary>
        public float made, wanted;
        /// <summary>A passing shortage or rush (a famine, a raid, a fair): the price is this share dearer, fading day by day.</summary>
        public float dearer;
        /// <summary>The season's share of the price by day of the year, and the share of the usual stock the market
        /// settles toward (null: the same all year): Seasons' tables.</summary>
        internal float[] season, make;

        public bool OnOffer => stock >= 1f;
    }

    /// <summary>
    /// A PORT'S MARKET: its goods, their stock and prices. Prices follow supply and demand:
    ///   - a good's usual price there is its value, cheaper where it's made (made_factor) and dearer where it's wanted
    ///     (wanted_factor), cheaper in its cheap months and dearer in its dear ones, blended over about 20 days at the
    ///     months' edges (Seasons);
    ///   - stock below the usual makes it dearer, stock above cheaper (elasticity); a good sold to a market beyond
    ///     what it wants gluts it, and it pays less till the glut is used up;
    ///   - day by day the stock settles back toward the usual (made, sold on, used) and gluts fade (regen, glut_fade);
    ///     for a good with a season of making (the harvest, the vintage, the herring) it settles toward that day's share
    ///     of the usual, so a granary runs low in spring and fills after the harvest (seasons_793.json);
    ///   - it asks a buyer more than its price and pays a seller less (spread).
    /// A market can be shut for a while (a raid, a burning: disruptedUntil), rebuilt smaller after a burning, and ruined
    /// for good (its last sack, or its last year as a trading place passed): a ruined market takes no deals and its size
    /// falls a step at each new year after, to nothing (economy design 2.1 and 2.5).
    /// All in deniers of silver (shown in the zone's own reckoning). Data/trade_rules.json holds every number.
    /// </summary>
    public sealed class Market
    {
        public readonly PortDef port;
        public readonly ZoneDef zone;
        readonly TradeRules rules;
        readonly Dictionary<string, MarketGood> goods = new Dictionary<string, MarketGood>();
        readonly List<MarketGood> list = new List<MarketGood>();          // in the goods table's order (the same every run)
        readonly List<MarketGood> seasonal = new List<MarketGood>();      // those with a season of making (stepped by the day)
        double day;                                                  // the game day it was last brought up to date
        /// <summary>The market takes no deals before this game day (a raid or a burning: Shocks).</summary>
        public double disruptedUntil;
        // steps of size lost to burnings; the year it was ruined (0: it wasn't) and the game day from which; its size now
        int sizeLoss, ruinYear, size = -1;
        double ruinedFrom;
        // where it stands in the year (for the season's price): the year, the day of the year, the month, the tables' place
        int nowYear, nowDoy, nowMonth, nowIndex;

        /// <summary>Whether the market is shut to deals on this day (raided or burned lately, or ruined).</summary>
        public bool Disrupted(double today) => today < disruptedUntil || RuinedOn(today);

        /// <summary>Whether the market is ruined on this day: it takes no more deals.</summary>
        public bool RuinedOn(double today) => ruinYear > 0 && today >= ruinedFrom;

        /// <summary>Ruined as of the day it was last brought up to.</summary>
        public bool Ruined => RuinedOn(day);

        /// <summary>Its size now (its port's, less burnings, less a step a year since it was ruined; 0: nothing left).</summary>
        public int Size => size;
        /// <summary>The year it was ruined (0: it wasn't), the game day it was ruined from, and the steps lost to burnings.</summary>
        public int RuinYear => ruinYear;
        public double RuinedFrom => ruinedFrom;
        public int SizeLoss => sizeLoss;
        /// <summary>The game day it was last brought up to, and that day's year and month.</summary>
        public double Day => day;
        public int Year => nowYear;
        public int Month => nowMonth;

        public Market(PortDef port, TradeRules rules, int year, double today)
        {
            this.port = port;
            this.rules = rules;
            zone = rules.ZoneOf(port.region);
            day = today;
            foreach (var g in GameData.Goods)
            {
                if (!g.Traded(year))
                    continue;
                var mg = new MarketGood
                {
                    good = g, made = g.MadeIn(port.region), wanted = g.WantedIn(port.region),
                    season = Seasons.PriceTable(g, rules), make = Seasons.MakeTable(g),
                };
                goods[g.id] = mg;
                list.Add(mg);
                if (mg.make != null)
                    seasonal.Add(mg);
            }
            Now();                                                       // (a port whose last year has passed is ruined)
            Resize();                                                    // usual and appetite at its size
            // not every market is full the day you come: a little above or below its usual, the same for every game (a good
            // with a season of making then drifts toward the day's share of it, which the tests found gentler at the start
            // than seeding at that share)
            foreach (var mg in list)
                mg.stock = mg.usual > 0f ? Mathf.Round(mg.usual * (0.75f + 0.5f * Hash(port.id, mg.good.id))) : 0f;
        }

        public IReadOnlyList<MarketGood> Goods => list;

        /// <summary>The market as plain data (a saved game): its day, its closing, its burnings and ruin, and each good's
        /// stock, glut and dearness.</summary>
        public Dictionary<string, object> Capture()
        {
            var goodsData = new List<object>();
            foreach (var g in list)
                goodsData.Add(new List<object> { g.good.id, g.stock, g.glut, g.dearer });
            return new Dictionary<string, object>
            {
                ["port"] = port.id, ["day"] = day, ["disrupted"] = disruptedUntil,
                ["size_loss"] = (double)sizeLoss, ["ruin"] = (double)ruinYear, ["ruined_from"] = ruinedFrom, ["goods"] = goodsData,
            };
        }

        public void Restore(Dictionary<string, object> d)
        {
            if (d == null)
                return;
            day = Json.Num(d, "day", day);
            disruptedUntil = Json.Num(d, "disrupted", 0.0);
            sizeLoss = (int)Json.Num(d, "size_loss", 0.0);
            ruinYear = (int)Json.Num(d, "ruin", 0.0);
            ruinedFrom = Json.Num(d, "ruined_from", 0.0);
            Now();
            Resize();
            foreach (var x in Json.Arr(d, "goods") ?? new List<object>())
                if (x is List<object> l && l.Count >= 4 && goods.TryGetValue(l[0] as string ?? "", out var g))
                {
                    g.stock = (float)(double)l[1];
                    g.glut = (float)(double)l[2];
                    g.dearer = (float)(double)l[3];
                }
        }
        public MarketGood Get(string id) => id != null && goods.TryGetValue(id, out var g) ? g : null;

        /// <summary>Brings the market up to the day: stock settles back (toward the season's share for a good with a
        /// season of making), gluts and shocks fade; a ruined market's size steps down as each year turns.</summary>
        public void CatchUp(double today)
        {
            while (day < today)
            {
                // a span that stays in one year: the size is the year's
                double yearEnd = Math.Floor(day) + (Calendar.DaysIn(nowYear) - nowDoy + 1);
                double to = Math.Min(today, yearEnd);
                Settle(day, to);
                day = to;
                Now();
                Resize();
            }
        }

        // the closed form over a span (as the first build's CatchUp), and for the goods with a season of making, a step a
        // day (exact for any split of the span: a part of a day settles by the same share as its hours would)
        void Settle(double from, double to)
        {
            double span = to - from;
            if (span <= 0.0)
                return;
            float settle = 1f - Mathf.Pow(1f - rules.regen, (float)span);
            float fade = Mathf.Pow(1f - rules.glutFade, (float)span);
            float ease = Mathf.Pow(1f - rules.shockFade, (float)span);
            foreach (var g in list)
            {
                g.dearer *= ease;
                g.glut *= fade;
                if (g.make == null || g.usual <= 0f)
                    g.stock += (g.usual - g.stock) * settle;
            }
            if (seasonal.Count == 0)
                return;
            Seasons.DateOf(from, out int year, out int doy);
            double t = from;
            while (t < to)
            {
                double midnight = Math.Floor(t) + 1.0;
                double next = Math.Min(to, midnight);
                float share = next - t >= 1.0 ? rules.regen : 1f - Mathf.Pow(1f - rules.regen, (float)(next - t));
                int i = Seasons.Index(year, doy);
                foreach (var g in seasonal)
                    if (g.usual > 0f)
                        g.stock += (g.usual * g.make[i] - g.stock) * share;
                t = next;
                if (t >= midnight)
                    doy++;
            }
        }

        /// <summary>The economy's calendar was moved (the dev keys' year): the market reads its date again (and is ruined if
        /// its port's last year has passed).</summary>
        public void Recalendar()
        {
            Now();
            Resize();
        }

        // the date of the market's day; a port whose last year as a trading place has passed is ruined from the turn of
        // the year after, its size counted down from that last year (design 2.1)
        void Now()
        {
            Seasons.DateOf(day, out nowYear, out nowDoy);
            nowIndex = Seasons.Index(nowYear, nowDoy);
            nowMonth = Seasons.MonthOfIndex(nowIndex);
            if (ruinYear == 0 && port.to > 0 && nowYear > port.to)
            {
                ruinYear = port.to;
                ruinedFrom = Seasons.DayOf(port.to + 1, 1);
            }
        }

        /// <summary>Its size in a year: its port's, less the steps lost to burnings, less a step for each new year since it
        /// was ruined, to 0.</summary>
        public int SizeIn(int year)
        {
            int s = Math.Max(1, port.size) - sizeLoss;
            if (ruinYear > 0)
                s -= Math.Max(0, year - ruinYear);
            return Math.Max(0, s);
        }

        // usual and appetite follow the size (as the first build worked them out from the port's size)
        void Resize()
        {
            int s = SizeIn(nowYear);
            if (s == size)
                return;
            size = s;
            foreach (var mg in list)
            {
                float worth = Mathf.Max(0.5f, mg.good.value);
                mg.usual = s > 0 && mg.made > 0f ? Mathf.Clamp(s * mg.made * rules.stockValue / worth, rules.stockMin, rules.stockMax) : 0f;
                mg.appetite = Mathf.Max(1f, s * (mg.wanted > 0f ? mg.wanted : 0.5f) * rules.appetiteValue / worth);
            }
        }

        /// <summary>Ruined from this game day (its last sack, design 2.5): it takes no more deals, and its size falls a step at
        /// each new year after, to nothing. An earlier ruin stands.</summary>
        public void Ruin(double from)
        {
            if (ruinYear > 0 && ruinedFrom <= from)
                return;
            ruinYear = Seasons.YearOf(from);
            ruinedFrom = from;
            Resize();
        }

        /// <summary>Burned (design 2.5): rebuilt this many steps of size smaller.</summary>
        public void Burn(int steps)
        {
            if (steps <= 0)
                return;
            sizeLoss += steps;
            Resize();
        }

        /// <summary>The market's own price for one unit, in deniers, before what it asks or pays on top.</summary>
        public float Price(MarketGood g, int month)
        {
            float p = g.good.value * GameData.MadeFactor(g.made) * GameData.WantedFactor(g.wanted) * SeasonNow(g, month);
            if (g.usual > 0f)
                p *= Mathf.Clamp(Mathf.Pow(g.usual / Mathf.Max(g.stock, 0.5f), rules.elasticity), rules.priceMin, rules.priceMax);
            else if (g.made <= 0f && g.wanted <= 0f)
                p *= rules.unwanted;                                 // nobody here makes it or wants it
            p *= Mathf.Pow(1f + g.glut / g.appetite, -rules.glutElasticity);
            p *= 1f + g.dearer;
            return Mathf.Max(0.05f, p);
        }

        /// <summary>The price everyone knows a market gives for a good, without knowing its stock today: where it's made
        /// it's cheap, where it's wanted dear, by season (the AI traders' guess for markets they haven't seen lately).
        /// By the month as a whole, as Lore has it.</summary>
        public float UsualPrice(MarketGood g, int month)
        {
            float p = g.good.value * GameData.MadeFactor(g.made) * GameData.WantedFactor(g.wanted) * Season(g.good, month);
            if (g.usual <= 0f && g.made <= 0f && g.wanted <= 0f)
                p *= rules.unwanted;
            return Mathf.Max(0.05f, p);
        }

        /// <summary>The share a buyer pays over the price, and a seller gets under it (half the spread each way).</summary>
        public float Spread => rules.spread * 0.5f;

        /// <summary>What it asks a buyer for one unit (deniers).</summary>
        public float Asks(MarketGood g, int month) => Price(g, month) * (1f + rules.spread * 0.5f);

        /// <summary>What it pays a seller for one unit (deniers), before the toll.</summary>
        public float Pays(MarketGood g, int month) => Price(g, month) * (1f - rules.spread * 0.5f);

        /// <summary>One unit bought from the market (its stock falls).</summary>
        public void TookOne(MarketGood g) => g.stock = Mathf.Max(0f, g.stock - 1f);

        /// <summary>One unit sold to the market: into its stock if it makes the good, else a glut till it's used.</summary>
        public void GotOne(MarketGood g)
        {
            g.stock += 1f;
            if (g.wanted <= 0f || g.glut > 0f || g.usual <= 0f)
                g.glut += 1f;
        }

        /// <summary>
        /// A shock to the market (the chronicle's events): its stock of the goods that match falls to this share, and they
        /// are dearer by this much, fading over the weeks after (a raid: everything; a famine: food; a murrain: livestock).
        /// A share above 1 fills it (a fair: goods brought in).
        /// </summary>
        public void Shock(Func<GoodDef, bool> which, float stockLeft, float dearer)
        {
            foreach (var g in list)
                if (which == null || which(g.good))
                {
                    g.stock *= Mathf.Max(0f, stockLeft);
                    g.dearer = dearer >= 0f ? Mathf.Max(g.dearer, dearer) : Mathf.Min(g.dearer, dearer);   // (negative: a glut, cheaper)
                }
        }

        /// <summary>A good's season in a month as a whole: 0.85 in its cheap months, 1.2 in its dear ones (trade_rules).</summary>
        public float Season(GoodDef g, int month)
        {
            if (Array.IndexOf(g.cheap, month) >= 0) return rules.cheap;
            if (Array.IndexOf(g.dear, month) >= 0) return rules.dear;
            return 1f;
        }

        /// <summary>A good's season as the market prices it: today's, blended at the months' edges (design 2.2), when the
        /// month asked is the month the market stands in; else that month's middle (its own season, as Season gives).</summary>
        public float SeasonNow(MarketGood g, int month)
        {
            if (g.season == null || month < 1 || month > 12)
                return Season(g.good, month);
            return g.season[month == nowMonth ? nowIndex : Seasons.MidMonth(month)];
        }

        static float Hash(string a, string b)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in a + "|" + b)
                    h = (h ^ c) * 16777619;
                return (h & 0xFFFF) / 65535f;
            }
        }
    }

    /// <summary>Every port's market, made the first time it's needed and kept for the game.</summary>
    public static class Markets
    {
        static readonly Dictionary<string, Market> markets = new Dictionary<string, Market>();
        static readonly List<Market> order = new List<Market>();               // as they were opened (OpenAll: the ports table's order)

        /// <summary>Every market open so far.</summary>
        public static IReadOnlyList<Market> All => order;

        /// <summary>Opens every port's market that trades in the year (the world's ships deal in all of them).</summary>
        public static void OpenAll(int year, double today)
        {
            foreach (var p in GameData.Ports)
                if (p.Exists(year))
                    Of(p, year, today);
        }

        /// <summary>Forgets every market (a new game, or a headless run starting over).</summary>
        public static void Clear()
        {
            markets.Clear();
            order.Clear();
        }

        public static Market Of(PortDef port, int year, double today)
        {
            if (port == null)
                return null;
            if (!markets.TryGetValue(port.id, out var m))
            {
                m = new Market(port, TradeRules.Current, year, today);
                markets[port.id] = m;
                order.Add(m);
            }
            m.CatchUp(today);
            return m;
        }

        /// <summary>Every market brought up to the day (the graphs call it each game day).</summary>
        public static void CatchUpAll(double today)
        {
            foreach (var m in order)
                m.CatchUp(today);
        }

        /// <summary>The economy's calendar was moved (Seasons.SetStart: the dev keys' year): every market reads its date again.</summary>
        public static void Recalendar()
        {
            foreach (var m in order)
                m.Recalendar();
        }

        /// <summary>
        /// A fingerprint of every open market (FNV-1a over each good's stock, glut and dearness and each market's closing,
        /// size and ruin, in the order they opened): two runs that should agree give the same number (a desync check later).
        /// </summary>
        public static ulong Hash()
        {
            ulong h = 14695981039346656037UL;
            void Mix(int v)
            {
                for (int b = 0; b < 4; b++)
                {
                    h ^= (byte)(v >> (8 * b));
                    h *= 1099511628211UL;
                }
            }
            foreach (var m in order)
            {
                foreach (char c in m.port.id)
                    Mix(c);
                Mix((int)Math.Round(m.disruptedUntil * 1000.0));
                Mix(m.Size);
                Mix(m.SizeLoss);
                Mix(m.RuinYear);
                Mix((int)Math.Round(m.RuinedFrom * 1000.0));
                foreach (var g in m.Goods)
                {
                    Mix(BitConverter.SingleToInt32Bits(g.stock));
                    Mix(BitConverter.SingleToInt32Bits(g.glut));
                    Mix(BitConverter.SingleToInt32Bits(g.dearer));
                }
            }
            return h;
        }

        /// <summary>Every open market as plain data (a saved game).</summary>
        public static List<object> Capture()
        {
            var list = new List<object>();
            foreach (var m in order)
                list.Add(m.Capture());
            return list;
        }

        /// <summary>The markets as saved (opening any that weren't open yet).</summary>
        public static void Restore(List<object> data, int year)
        {
            foreach (var x in data ?? new List<object>())
            {
                var p = GameData.Port(Json.Str(x, "port"));
                if (p == null)
                    continue;
                if (!markets.TryGetValue(p.id, out var m))
                {
                    m = new Market(p, TradeRules.Current, year, Json.Num(x, "day"));
                    markets[p.id] = m;
                    order.Add(m);
                }
                m.Restore(x as Dictionary<string, object>);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Clear();
    }
}
