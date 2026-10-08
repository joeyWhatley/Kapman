using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>An order: a place wants so many of a good brought to it by a day, at so much a unit.</summary>
    public sealed class Commission
    {
        public string id, port, good;
        public int units;
        /// <summary>Deniers for the whole order before the toll (units x each); the day it runs out; the day it was given.</summary>
        public double pay, deadline, posted;
        /// <summary>Deniers a unit before the toll, fixed on the day the order is given.</summary>
        public double each;
        /// <summary>Units brought so far: an order may be brought in parts.</summary>
        public int done;
        /// <summary>Taken by you: it stands in Tasks until it is brought or its day passes.</summary>
        public bool mine;
        /// <summary>Offered to you by name (a card): kept for you until you take or refuse it, or the offer runs out.</summary>
        public bool offered;
        /// <summary>The last warning said (0 none, 1 a week left, 2 a day left).</summary>
        public int warned;

        /// <summary>Units still to bring.</summary>
        public int Left => Math.Max(0, units - done);
    }

    /// <summary>
    /// ORDERS (Data/commissions_793.json; Design/fixes/orders.md). A place wants so many of a good by a day.
    ///   - WHERE THEY COME FROM. A port posts them by its kind, open to every merchant (others fill them too). A card
    ///     offers one to you by name: it is kept for you, and asked of you at once (take it or refuse it).
    ///   - TAKEN, it stands in Tasks with its day. It may be brought in parts: each part is paid as it lands.
    ///   - THE PAY is so much a unit, fixed when the order is given: what the market there pays that day (never more
    ///     than it usually pays), plus at most a half, and that part over the market is never more than 60 d for the
    ///     whole order. The toll comes off every part landed, as off any sale.
    ///   - ITS DAY PASSED: nothing brought, it is broken and they remember it; part brought, it is short, a smaller
    ///     mark; what was landed stays paid. All brought in time: kept, and your standing there rises.
    /// </summary>
    public static class Commissions
    {
        public static readonly List<Commission> All = new List<Commission>();
        static Dictionary<string, object> byKind = new Dictionary<string, object>();
        static double aiFill = 0.04, premiumMax = 0.5, bonusMax = 60.0, offerDays = 7.0, warnDays = 7.0;
        static int made;
        static System.Random dice = new System.Random(800);

        /// <summary>An order was offered to you by name (the frame asks at once: take it or refuse it).</summary>
        public static event Action<Commission> Offered;

        /// <summary>The most an order pays over the market, as a share of it; and in deniers for the whole order (0: no such cap).</summary>
        public static double PremiumMax => premiumMax;
        public static double BonusMax => bonusMax;
        /// <summary>Game days an offer stands unanswered.</summary>
        public static double OfferDays => offerDays;

        public static void Load(TextAsset json)
        {
            All.Clear();
            made = 0;
            aiFill = 0.04;
            premiumMax = 0.5;
            bonusMax = 60.0;
            offerDays = 7.0;
            warnDays = 7.0;
            byKind = new Dictionary<string, object>();
            if (json == null)
                return;
            var o = Json.Parse(json.text);
            byKind = Json.Obj(o, "by_kind") ?? new Dictionary<string, object>();
            aiFill = Json.Num(o, "ai_fill_per_day", aiFill);
            premiumMax = Json.Num(o, "premium_max", premiumMax);
            bonusMax = Json.Num(o, "bonus_max_d", bonusMax);
            offerDays = Json.Num(o, "offer_days", offerDays);
            warnDays = Json.Num(o, "warn_days", warnDays);
        }

        // ------------------------------------------------------------------ the price

        /// <summary>What the market there pays for one unit, as an order reckons it: its price to a seller today, but never
        /// more than it usually pays in this month. So a run on the town's stock, or a famine, cannot be locked into an order.</summary>
        public static double MarketEach(PortDef p, GoodDef g, int year, int month, double today)
        {
            if (p == null || g == null)
                return 0.0;
            var m = Markets.Of(p, year, today);
            var mg = m != null ? m.Get(g.id) : null;
            if (mg == null)
                return g.value;
            return Math.Min(m.Pays(mg, month), m.UsualPrice(mg, month) * (1f - m.Spread));
        }

        /// <summary>Deniers a unit an order pays, before the toll: the market's price there plus the premium asked, the
        /// premium cut to the most allowed (a half) and to the most the whole order may pay over the market (60 d).</summary>
        public static double PriceEach(PortDef p, GoodDef g, int units, double premium, int year, int month, double today)
        {
            double market = MarketEach(p, g, year, month, today);
            if (market <= 0.0 || units <= 0)
                return 0.0;
            double share = Math.Max(0.0, Math.Min(premium, premiumMax));
            if (bonusMax > 0.0 && market * units * share > bonusMax)
                share = bonusMax / (market * units);
            return Math.Round(market * (1.0 + share), 2);
        }

        static Commission Make(PortDef p, GoodDef g, int units, double days, double premium, int year, int month, double today)
        {
            double each = PriceEach(p, g, units, premium, year, month, today);
            return new Commission
            {
                id = "c" + (++made), port = p.id, good = g.id, units = units, each = each, pay = Math.Round(each * units),
                posted = today, deadline = today + days,
            };
        }

        // ------------------------------------------------------------------ the day's work

        /// <summary>Once a game day: new orders posted, others filled by other merchants, yours warned of and closed.</summary>
        public static void Daily(int day)
        {
            var game = KapmanGame.Current;
            if (game != null)
                Daily(game.Day, game.Year, game.Month, game.World != null ? game.World.seed : 793);
        }

        /// <summary>The day's work, given the day, year, month and the world's seed.</summary>
        public static void Daily(double today, int year, int month, int seed)
        {
            dice = new System.Random(seed * 911 + (int)Math.Floor(today));
            for (int i = All.Count - 1; i >= 0; i--)
            {
                var c = All[i];
                if (c.mine)
                {
                    if (today > c.deadline)
                    {
                        All.RemoveAt(i);
                        Close(c);
                    }
                    else
                        Warn(c, today);
                }
                else if (c.offered)
                {
                    // (kept for you: no other merchant fills it; unanswered, it is let go, which changes least)
                    if (today > c.posted + offerDays || today > c.deadline)
                    {
                        All.RemoveAt(i);
                        Voyage.Say($"The order offered at {GameData.Port(c.port)?.name} had no answer from you: it has gone to another.");
                    }
                }
                else if (today > c.deadline || dice.NextDouble() < aiFill)
                    All.RemoveAt(i);                                        // (filled by another, or no longer wanted)
            }
            foreach (var p in GameData.Ports)
            {
                if (!p.Exists(year) || !(Json.Obj(byKind, p.kind ?? "") is Dictionary<string, object> kind))
                    continue;
                if (dice.NextDouble() >= Json.Num(kind, "chance_per_month") / 30.0 || All.Exists(c => c.port == p.id && !c.mine && !c.offered))
                    continue;
                var goods = Json.Obj(kind, "goods");
                if (goods == null || goods.Count == 0)
                    continue;
                var keys = new List<string>(goods.Keys);
                string id = keys[dice.Next(keys.Count)];
                var good = GameData.Good(id);
                var range = goods[id] as List<object>;
                if (good == null || range == null || range.Count < 2)
                    continue;
                int units = dice.Next((int)(double)range[0], (int)(double)range[1] + 1);
                All.Add(Make(p, good, units, Json.Num(kind, "days", 40), Json.Num(kind, "premium", 0.3), year, month, today));
            }
        }

        /// <summary>A taken order whose day has passed with units still to bring: broken (none brought) or short (part brought).</summary>
        static void Close(Commission c)
        {
            var p = GameData.Port(c.port);
            var good = GameData.Good(c.good);
            string what = good != null ? Trade.UnitsNamed(good, c.units) : c.units + " " + c.good;
            if (c.done <= 0)
            {
                Sim.Deeds.Record("broke_commission", $"didn't bring {what} to {p?.name}", p);
                Voyage.Say($"The order for {what} at {p?.name} has run out with nothing brought: they'll remember it.");
            }
            else
            {
                Sim.Deeds.Record("short_commission", $"brought {c.done} of {what} to {p?.name}", p);
                Voyage.Say($"The order for {what} at {p?.name} has run out: you brought {c.done}, and were paid for them. They wanted the rest.");
            }
        }

        /// <summary>A week before its day, and a day before: said once each, and the clock drops to its slow speed.</summary>
        static void Warn(Commission c, double today)
        {
            double left = c.deadline - today;
            int stage = left <= 1.0 ? 2 : left <= warnDays ? 1 : 0;
            if (stage <= c.warned)
                return;
            c.warned = stage;
            var good = GameData.Good(c.good);
            string still = good != null ? Trade.UnitsNamed(good, c.Left) : c.Left + " " + c.good;
            Voyage.Say(stage == 2
                ? $"Tomorrow is the day for the order at {GameData.Port(c.port)?.name}: {still} still to bring."
                : $"{Math.Ceiling(left):0} days left for the order at {GameData.Port(c.port)?.name}: {still} still to bring.");
            // (the clock drops back by itself for it, as for the debt's day: ruling 26, with its own switch in the settings)
            ClockDrops.Raise(stage == 2 ? ClockReason.OrderDay : ClockReason.OrderNear);
        }

        // ------------------------------------------------------------------ reading them

        /// <summary>Orders open at a port: posted there for any merchant, not yet taken by you.</summary>
        public static List<Commission> At(PortDef p) => All.FindAll(c => p != null && c.port == p.id && !c.mine && !c.offered);

        /// <summary>Orders offered to you by name and not yet answered.</summary>
        public static List<Commission> OfferedToYou => All.FindAll(c => c.offered && !c.mine);

        /// <summary>Orders you've taken.</summary>
        public static List<Commission> Mine => All.FindAll(c => c.mine);

        /// <summary>How many units of the order's good her hold takes when it is empty (0: she cannot carry it at all).</summary>
        public static int FitsAtOnce(Commission c)
        {
            var g = c != null ? GameData.Good(c.good) : null;
            if (g == null || !Trade.Started)
                return 0;
            return (int)Math.Floor(Trade.Hold.capacity / Math.Max(0.001f, g.Space) + 1e-6);
        }

        /// <summary>How many full holds what is left of the order comes to (0: she cannot carry it at all).</summary>
        public static int Loads(Commission c)
        {
            int fits = FitsAtOnce(c);
            return fits <= 0 || c == null ? 0 : (c.Left + fits - 1) / fits;
        }

        /// <summary>"She carries 4 at a time: 3 loads." ("" when one load does it; the reason when she cannot carry it.)</summary>
        public static string LoadsLine(Commission c)
        {
            if (c == null || !Trade.Started)
                return "";
            var g = GameData.Good(c.good);
            int fits = FitsAtOnce(c);
            if (fits <= 0)
                return $"She cannot carry {(g != null ? g.name.ToLowerInvariant() : c.good)}: one alone is more than her hold.";
            int loads = Loads(c);
            return loads <= 1 ? "" : $"She carries {fits} at a time: {loads} loads. It may be brought in parts.";
        }

        static ZoneDef ZoneOf(PortDef p) => p != null && TradeRules.Current != null ? TradeRules.Current.ZoneOf(p.region) : null;

        static string Show(PortDef p, double deniers)
        {
            var z = ZoneOf(p);
            return z != null ? z.Show(deniers) : $"{deniers:0.#} d";
        }

        /// <summary>The date of a game day, "3 August 793".</summary>
        static string DateOf(double day)
        {
            // ported 4 Oct: the first build read the start year and day off KapmanGame.clock (the old GameClock); the
            // rebuild's level clock (Kapman.Levels.LevelClock) carries the same two fields
            var clock = Kapman.Levels.LevelClock.Current;
            return clock != null ? Kapman.World.Calendar.At(clock.year, clock.startDay, day * 24.0).ToString() : $"day {day:0}";
        }

        /// <summary>"5 casks of Rhenish wine for Lindisfarena by 3 August 793, at 76 d each, 380 d in all".</summary>
        public static string Describe(Commission c)
        {
            var good = GameData.Good(c.good);
            var p = GameData.Port(c.port);
            string what = good != null ? Trade.UnitsNamed(good, c.units) : c.units + " " + c.good;
            return $"{what} for {p?.name} by {DateOf(c.deadline)}, at {Show(p, c.each)} each, {Show(p, c.each * c.units)} in all";
        }

        // ------------------------------------------------------------------ offered, taken, refused

        /// <summary>A card offers you an order by name: kept for you, asked of you at once. Null if it cannot be made.</summary>
        public static Commission Offer(PortDef p, GoodDef g, int units, double days, double premium, int year, int month, double today)
        {
            if (p == null || g == null || units <= 0 || days <= 0.0)
                return null;
            var c = Make(p, g, units, days, premium, year, month, today);
            c.offered = true;
            All.Add(c);
            Offered?.Invoke(c);
            return c;
        }

        /// <summary>Why you cannot take it ("" if you can).</summary>
        public static string WhyNot(Commission c)
        {
            if (c == null || c.mine)
                return "";
            return Trade.Started && FitsAtOnce(c) <= 0 ? LoadsLine(c) : "";
        }

        public static string Take(Commission c)
        {
            if (c == null || c.mine)
                return "";
            string why = WhyNot(c);
            if (why.Length > 0)
                return why;
            c.mine = true;
            c.offered = false;
            return $"You took the order: {Describe(c)}.";
        }

        /// <summary>You refuse an order offered to you: it goes, and nobody thinks the worse of you. (An order you have
        /// taken cannot be refused: it is brought, or its day passes.)</summary>
        public static string Refuse(Commission c)
        {
            if (c == null || c.mine || !c.offered)
                return "";
            All.Remove(c);
            return $"You refused the order at {GameData.Port(c.port)?.name}.";
        }

        // ------------------------------------------------------------------ bringing it

        /// <summary>In port: what is aboard for your orders here, as one line to say on arrival ("" if nothing is).</summary>
        public static string Waiting(PortDef p)
        {
            if (p == null || !Trade.Started)
                return "";
            var told = new List<string>();
            foreach (var c in Mine)
            {
                var good = GameData.Good(c.good);
                int n = good != null && c.port == p.id ? Math.Min(Trade.Hold.Count(c.good), c.Left) : 0;
                if (n > 0)
                    told.Add(Trade.UnitsNamed(good, n));
            }
            return told.Count == 0 ? "" : $"Aboard for your order here: {string.Join(", ", told)}. Hand them over in Errands.";
        }

        /// <summary>In port: every order of yours for here, as much of each as is aboard, landed and paid. The facts ("" if none).</summary>
        public static string Deliver(PortDef p)
        {
            if (p == null || !Trade.Started)
                return "";
            var told = new List<string>();
            foreach (var c in Mine)
            {
                string fact = Deliver(c, p);
                if (fact.Length > 0)
                    told.Add(fact);
            }
            return string.Join(" ", told);
        }

        /// <summary>In port: as much of this order as is aboard (at most `most` units) landed and paid, less the toll of
        /// the place, as on any sale. The last part closes the order and is a deed kept. The fact ("" if nothing landed).</summary>
        public static string Deliver(Commission c, PortDef p, int most = int.MaxValue)
        {
            var good = c != null ? GameData.Good(c.good) : null;
            if (p == null || good == null || !c.mine || c.port != p.id || !Trade.Started)
                return "";
            int n = Math.Min(Math.Min(Trade.Hold.Count(c.good), c.Left), most);
            if (n <= 0)
                return "";
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            double today = game != null ? game.Day : c.posted;
            var zone = ZoneOf(p);
            double total = Math.Round(n * c.each, 2);
            bool free = Chest.TollFreeAt(p, year);
            // (the toll is the rule the graphs hold for every sale of the merchant's: GameRules.Toll, as in Deal.Sell)
            double toll = zone == null ? 0.0 : Math.Round(GameRules.Toll(total, zone.toll, EncounterRunner.TollFactor(p.id, today), free), 2);
            Trade.Hold.Remove(good, n);
            Trade.Purse.Receive(total - toll, zone, TradeRules.Current, out _);
            c.done += n;
            string paid = toll > 0.0 ? $"{Show(p, total)}, less the toll of {Show(p, toll)}: took {Trade.Purse.LastCoins}" : $"{Show(p, total)}: took {Trade.Purse.LastCoins}";
            // (while she works for the lender his share of what an order fetches goes to the stick, as of any sale)
            string stick = Trade.WorkShare(zone, total - toll);
            if (c.Left > 0)
                return $"Delivered {Trade.UnitsNamed(good, n)} of the {c.units} ordered, for {paid}. {c.Left} still to bring by {DateOf(c.deadline)}." + stick;
            All.Remove(c);
            Sim.Deeds.Record("kept_commission", $"brought {Trade.UnitsNamed(good, c.units)} to {p.name} as ordered", p);
            return (c.done == n
                ? $"Delivered {Trade.UnitsNamed(good, n)} as ordered, for {paid}."
                : $"Delivered the last {Trade.UnitsNamed(good, n)} of the {c.units} ordered, for {paid}. The order is kept.") + stick;
        }

        // ------------------------------------------------------------------ the saved game

        public static List<object> Capture() => All.ConvertAll(c => (object)new Dictionary<string, object>
        {
            ["id"] = c.id, ["port"] = c.port, ["good"] = c.good, ["units"] = c.units, ["pay"] = c.pay, ["deadline"] = c.deadline,
            ["posted"] = c.posted, ["mine"] = c.mine, ["each"] = c.each, ["done"] = c.done, ["offered"] = c.offered, ["warned"] = c.warned,
        });

        public static void Restore(List<object> data)
        {
            All.Clear();
            foreach (var x in data ?? new List<object>())
            {
                var c = new Commission
                {
                    id = Json.Str(x, "id"), port = Json.Str(x, "port"), good = Json.Str(x, "good"), units = (int)Json.Num(x, "units"),
                    pay = Json.Num(x, "pay"), deadline = Json.Num(x, "deadline"), posted = Json.Num(x, "posted"), mine = Json.Bool(x, "mine"),
                    done = (int)Json.Num(x, "done"), offered = Json.Bool(x, "offered"), warned = (int)Json.Num(x, "warned"),
                };
                // (a game saved before orders had a price a unit: the whole pay shared out)
                c.each = Json.Num(x, "each", c.units > 0 ? c.pay / c.units : 0.0);
                All.Add(c);
            }
            // (ids go on from the highest saved, so a new order never shares one with an old)
            made = 1000;
            foreach (var c in All)
                if (c.id != null && c.id.Length > 1 && int.TryParse(c.id.Substring(1), out int n) && n > made)
                    made = n;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            All.Clear();
            made = 0;
        }
    }
}
