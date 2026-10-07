using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>What one chronicle entry does to a market (shocks_793.json).</summary>
    public sealed class ShockDef
    {
        public string goods = "all";
        public float stockLeft = 1f, dearer;
        public int days, disruptDays;
        /// <summary>Steps of size a burned market is rebuilt smaller by, and the ports burned (port ids joined by commas;
        /// empty: every port the entry hits).</summary>
        public int sizeLoss;
        public string burns = "";
        /// <summary>The ports the entry ruins for good (port ids joined by commas, or "all"; empty: none): a ruined market
        /// takes no more deals and its size falls a step at each new year after (design 2.5: Dorestad in 863).</summary>
        public string ruin = "";
    }

    /// <summary>
    /// THE CHRONICLE'S SHOCKS (economy design 2.5, night one): when an entry comes (a famine, a raid, a burning), the
    /// markets of its ports (or of every port in its regions) lose stock and turn dear; the dearness is held for the
    /// shock's days, or to the entry's end (the great famine of 793 to the harvest of 794), and fades after at
    /// trade_rules' shock_fade; a raid shuts the market to deals for some days. Held shocks are re-applied each day after
    /// the market is caught up, so the fade starts only when the hold ends. By entry id first, then by kind.
    /// Night two: a burning rebuilds the market smaller (size_loss), and a last sack ruins it for good (ruin: Dorestad
    /// in 863), after which it takes no deals and its size falls a step a year.
    /// </summary>
    public static class Shocks
    {
        sealed class Hold
        {
            public Market market;
            public string goods;
            public float dearer;
            public double until;
        }

        static readonly Dictionary<string, ShockDef> byId = new Dictionary<string, ShockDef>();
        static readonly Dictionary<string, ShockDef> byKind = new Dictionary<string, ShockDef>();
        static readonly List<Hold> holds = new List<Hold>();
        static double lastDay = double.MinValue;                    // the last day held: an earlier one (the clock put back) does nothing

        public static IReadOnlyCollection<string> Ids => byId.Keys;
        public static int Holding => holds.Count;

        public static void Load(TextAsset json) => Load(json != null ? json.text : null);

        /// <summary>Reads the tables (each level's game does it; the held shocks carry on: Clear starts a new world).</summary>
        public static void Load(string text)
        {
            byId.Clear();
            byKind.Clear();
            if (string.IsNullOrEmpty(text))
                return;
            var root = Json.Parse(text);
            Read(Json.Obj(root, "by_id"), byId);
            Read(Json.Obj(root, "by_kind"), byKind);
        }

        static void Read(Dictionary<string, object> from, Dictionary<string, ShockDef> into)
        {
            if (from == null)
                return;
            foreach (var kv in from)
                into[kv.Key] = new ShockDef
                {
                    goods = Json.Str(kv.Value, "goods", "all"),
                    stockLeft = (float)Json.Num(kv.Value, "stock_left", 1.0),
                    dearer = (float)Json.Num(kv.Value, "dearer", 0.0),
                    days = (int)Json.Num(kv.Value, "days", 0.0),
                    disruptDays = (int)Json.Num(kv.Value, "disrupt_days", 0.0),
                    sizeLoss = (int)Json.Num(kv.Value, "size_loss", 0.0),
                    burns = Json.Str(kv.Value, "burns", ""),
                    ruin = Json.Str(kv.Value, "ruin", ""),
                };
        }

        /// <summary>The shock an entry makes, or null.</summary>
        public static ShockDef For(ChronicleEntry e)
        {
            if (e == null)
                return null;
            if (e.id != null && byId.TryGetValue(e.id, out var s))
                return s;
            return e.kind != null && byKind.TryGetValue(e.kind, out s) ? s : null;
        }

        /// <summary>The shock a kind of event makes when the table has nothing for the entry itself (a raid of the world's
        /// own raiders has no entry), or null.</summary>
        public static ShockDef ForKind(string kind) => kind != null && byKind.TryGetValue(kind, out var s) ? s : null;

        /// <summary>Whether a good is among a shock's goods: all, food, a category, or a list of ids.</summary>
        public static bool Touches(string goods, GoodDef g)
        {
            if (string.IsNullOrEmpty(goods) || goods == "all")
                return true;
            if (goods == "food")
                return string.Equals(g.cat, "Food", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(g.cat, goods, StringComparison.OrdinalIgnoreCase))
                return true;
            foreach (var id in goods.Split(','))
                if (id.Trim() == g.id)
                    return true;
            return false;
        }

        /// <summary>
        /// An entry has come: shocks the markets of these ports. happened is the game day the entry happened (earlier
        /// than today for the backstory replayed at the start); the hold and the closing run from it.
        /// </summary>
        public static void Apply(ChronicleEntry e, IEnumerable<Market> markets, double happened, double today)
        {
            var s = For(e);
            if (s == null)
                return;
            double holdDays = s.days > 0 ? s.days : DaysToEnd(e);
            foreach (var m in markets)
            {
                if (m == null)
                    continue;
                m.CatchUp(today);
                m.Shock(g => Touches(s.goods, g), s.stockLeft, s.dearer);
                if (s.disruptDays > 0)
                    m.disruptedUntil = Math.Max(m.disruptedUntil, happened + s.disruptDays);
                if (holdDays > 0.0 && happened + holdDays > today)
                    holds.Add(new Hold { market = m, goods = s.goods, dearer = s.dearer, until = happened + holdDays });
                // a burning: the market is rebuilt smaller; a last sack: ruined from the day it happened
                if (s.sizeLoss > 0 && Names(s.burns, m.port.id, true))
                    m.Burn(s.sizeLoss);
                if (Names(s.ruin, m.port.id, false))
                    m.Ruin(happened);
            }
        }

        /// <summary>Whether a list of port ids (joined by commas, or "all") names a port; an empty list names every port or
        /// none, as asked.</summary>
        static bool Names(string ids, string port, bool emptyIsAll)
        {
            if (string.IsNullOrEmpty(ids))
                return emptyIsAll;
            if (ids == "all")
                return true;
            foreach (var id in ids.Split(','))
                if (id.Trim() == port)
                    return true;
            return false;
        }

        /// <summary>The ports an entry happens at: those it names, or else every port of its regions, that exist in the year.</summary>
        public static List<PortDef> PortsOf(ChronicleEntry e, int year)
        {
            var at = new List<PortDef>();
            foreach (string id in e.ports)
            {
                var p = GameData.Port(id);
                if (p != null && p.Exists(year))
                    at.Add(p);
            }
            if (at.Count == 0)
                foreach (var p in GameData.Ports)
                    if (p.Exists(year) && Array.IndexOf(e.regions, p.region) >= 0)
                        at.Add(p);
            return at;
        }

        /// <summary>The days from an entry's firing to its end (0 when it gives none).</summary>
        public static double DaysToEnd(ChronicleEntry e)
        {
            if (e == null || e.EndKey <= 0)
                return 0.0;
            int ey = e.EndKey / 400, em = e.EndKey % 400 / 32, ed = e.EndKey % 32;
            return Math.Max(0, DayNumber(ey, em, ed) - DayNumber(e.year, e.month, e.day));
        }

        static int DayNumber(int year, int month, int day)
        {
            int n = year * 365 + year / 4 + Math.Max(1, day);
            for (int m = 1; m < Math.Max(1, month); m++)
                n += Kapman.World.Calendar.DaysIn(year, m);
            return n;
        }

        /// <summary>Each game day: the held shocks stay dear (each market caught up first); finished holds let go.</summary>
        public static void Daily(double today)
        {
            if (today <= lastDay)
                return;
            lastDay = today;
            for (int i = holds.Count - 1; i >= 0; i--)
            {
                var h = holds[i];
                if (today > h.until)
                {
                    holds.RemoveAt(i);
                    continue;
                }
                h.market.CatchUp(today);
                h.market.Shock(g => Touches(h.goods, g), 1f, h.dearer);
            }
        }

        public static void Clear()
        {
            holds.Clear();
            lastDay = double.MinValue;
        }

        /// <summary>The held shocks as plain data (a saved game).</summary>
        public static List<object> Capture()
        {
            var list = new List<object>();
            foreach (var h in holds)
                list.Add(new List<object> { h.market.port.id, h.goods, (double)h.dearer, h.until });
            return list;
        }

        public static void Restore(List<object> data, int year, double today)
        {
            holds.Clear();
            lastDay = today;
            foreach (var x in data ?? new List<object>())
                if (x is List<object> l && l.Count >= 4)
                {
                    var p = GameData.Port(l[0] as string);
                    if (p == null)
                        continue;
                    holds.Add(new Hold { market = Markets.Of(p, year, today), goods = l[1] as string, dearer = (float)(double)l[2], until = (double)l[3] });
                }
        }
    }
}
