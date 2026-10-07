using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>An order: a place wants so many of a good brought to it by a day, for so much.</summary>
    public sealed class Commission
    {
        public string id, port, good;
        public int units;
        public double pay, deadline, posted;
        public bool mine;
    }

    /// <summary>
    /// COMMISSIONS - orders worth taking (Data/commissions_793.json; Plans/ai_fleets.md, phase 6). Each port posts them by
    /// its kind (a monastery wants wine, oil, wax, incense, parchment; a hall wants arms, horses, silk...), paying above its
    /// market. You hear of them talking to people there and may take one; it's delivered when you lie there with the goods
    /// aboard, by the day. Other merchants fill them too. Keeping one raises your standing there; breaking one lowers it.
    /// </summary>
    public static class Commissions
    {
        public static readonly List<Commission> All = new List<Commission>();
        static Dictionary<string, object> byKind = new Dictionary<string, object>();
        static double aiFill = 0.04;
        static int made;
        static System.Random dice = new System.Random(800);

        public static void Load(TextAsset json)
        {
            All.Clear();
            made = 0;
            if (json == null)
                return;
            var o = Json.Parse(json.text);
            byKind = Json.Obj(o, "by_kind") ?? new Dictionary<string, object>();
            aiFill = Json.Num(o, "ai_fill_per_day", aiFill);
        }

        /// <summary>Once a game day: new orders posted, others filled by other merchants, yours past their day broken.</summary>
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
                if (c.mine && today > c.deadline)
                {
                    All.RemoveAt(i);
                    var p = GameData.Port(c.port);
                    Sim.Deeds.Record("broke_commission", $"didn't bring {c.units} {GameData.Good(c.good)?.name} to {p?.name}", p);
                    Voyage.Say($"The order for {c.units} {GameData.Good(c.good)?.name.ToLowerInvariant()} at {p?.name} has run out: they'll remember it.");
                }
                else if (!c.mine && (today > c.deadline || dice.NextDouble() < aiFill))
                    All.RemoveAt(i);                                        // (filled by another, or no longer wanted)
            }
            foreach (var p in GameData.Ports)
            {
                if (!p.Exists(year) || !(Json.Obj(byKind, p.kind ?? "") is Dictionary<string, object> kind))
                    continue;
                if (dice.NextDouble() >= Json.Num(kind, "chance_per_month") / 30.0 || All.Exists(c => c.port == p.id && !c.mine))
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
                var m = Markets.Of(p, year, today);
                var mg = m.Get(id);
                double each = (mg != null ? m.Pays(mg, month) : good.value) * (1.0 + Json.Num(kind, "premium", 0.3));
                All.Add(new Commission
                {
                    id = "c" + (++made), port = p.id, good = id, units = units, pay = Math.Round(each * units),
                    posted = today, deadline = today + Json.Num(kind, "days", 40),
                });
            }
        }

        /// <summary>Orders open at a port (not yet taken by you).</summary>
        public static List<Commission> At(PortDef p) => All.FindAll(c => p != null && c.port == p.id && !c.mine);

        /// <summary>Orders you've taken.</summary>
        public static List<Commission> Mine => All.FindAll(c => c.mine);

        /// <summary>"5 casks of Rhenish wine for Lindisfarena by 3 August 793, for 380 deniers".</summary>
        public static string Describe(Commission c)
        {
            var good = GameData.Good(c.good);
            var p = GameData.Port(c.port);
            // ported 4 Oct: the first build read the start year and day off KapmanGame.clock (the old GameClock); the
            // rebuild's level clock (Kapman.Levels.LevelClock) carries the same two fields
            var clock = Kapman.Levels.LevelClock.Current;
            string by = clock != null ? Kapman.World.Calendar.At(clock.year, clock.startDay, c.deadline * 24.0).ToString() : $"day {c.deadline:0}";
            return $"{(good != null ? Trade.Units(good, c.units) : c.units + " " + c.good)} for {p?.name} by {by}, for {c.pay:0} deniers";
        }

        public static string Take(Commission c)
        {
            if (c == null || c.mine)
                return "";
            c.mine = true;
            return $"You took the order: {Describe(c)}.";
        }

        /// <summary>In port: the orders for here you can fill from your hold, delivered and paid. The facts ("" if none).</summary>
        public static string Deliver(PortDef p)
        {
            if (p == null || !Trade.Started)
                return "";
            var told = new List<string>();
            foreach (var c in Mine)
            {
                var good = GameData.Good(c.good);
                if (c.port != p.id || good == null || Trade.Hold.Count(c.good) < c.units)
                    continue;
                Trade.Hold.Remove(good, c.units);
                Trade.Purse.Receive(c.pay, TradeRules.Current.ZoneOf(p.region), TradeRules.Current, out _);
                All.Remove(c);
                Sim.Deeds.Record("kept_commission", $"brought {Trade.Units(good, c.units)} to {p.name} as ordered", p);
                told.Add($"Delivered {Trade.Units(good, c.units)} as ordered: paid {c.pay:0} deniers.");
            }
            return string.Join(" ", told);
        }

        public static List<object> Capture() => All.ConvertAll(c => (object)new Dictionary<string, object>
        {
            ["id"] = c.id, ["port"] = c.port, ["good"] = c.good, ["units"] = c.units, ["pay"] = c.pay, ["deadline"] = c.deadline,
            ["posted"] = c.posted, ["mine"] = c.mine,
        });

        public static void Restore(List<object> data)
        {
            All.Clear();
            foreach (var x in data ?? new List<object>())
                All.Add(new Commission
                {
                    id = Json.Str(x, "id"), port = Json.Str(x, "port"), good = Json.Str(x, "good"), units = (int)Json.Num(x, "units"),
                    pay = Json.Num(x, "pay"), deadline = Json.Num(x, "deadline"), posted = Json.Num(x, "posted"), mine = Json.Bool(x, "mine"),
                });
            made = All.Count + 1000;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            All.Clear();
            made = 0;
        }
    }
}
