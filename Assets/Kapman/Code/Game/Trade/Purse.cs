using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE PURSE: the merchant's money, coin by coin, and silver by weight. What it's worth depends on where you are
    /// (Data/trade_rules.json): each zone takes its own coin at full value and other coin and silver at their silver
    /// less a loss (to the mint, or nicked and tested on the scales). Paying uses the local coin first, then the
    /// rest, least lost first; change comes back in the local coin (or cut silver where silver goes by weight).
    /// </summary>
    public sealed class Purse
    {
        readonly Dictionary<string, double> held = new Dictionary<string, double>();

        /// <summary>Coins of a kind held (hacksilver: grams).</summary>
        public double Count(string coin) => coin != null && held.TryGetValue(coin, out var n) ? n : 0.0;

        public IEnumerable<KeyValuePair<string, double>> All => held.Where(kv => kv.Value > 0.0001);

        /// <summary>Empties it (a saved game loaded).</summary>
        public void Clear() => held.Clear();

        /// <summary>The purse as plain data: coin -> count.</summary>
        public Dictionary<string, object> Capture()
        {
            var d = new Dictionary<string, object>();
            foreach (var kv in held)
                d[kv.Key] = kv.Value;
            return d;
        }

        public void Restore(Dictionary<string, object> data)
        {
            held.Clear();
            foreach (var kv in data ?? new Dictionary<string, object>())
                if (kv.Value is double n)
                    held[kv.Key] = n;
        }

        public void Add(string coin, double n)
        {
            if (string.IsNullOrEmpty(coin) || n == 0.0)
                return;
            held[coin] = Math.Max(0.0, Count(coin) + n);
        }

        /// <summary>Deniers one coin of a kind is worth paying here.</summary>
        public static double Rate(string coin, ZoneDef zone, TradeRules rules)
        {
            var c = rules.Coin(coin);
            if (c == null)
                return 0.0;
            double silver = c.Deniers;
            return zone != null && coin == zone.coin ? silver : silver * (1.0 - (zone?.foreignLoss ?? 0f));
        }

        /// <summary>What the purse is worth paying here, in deniers.</summary>
        public double Worth(ZoneDef zone, TradeRules rules) => All.Sum(kv => kv.Value * Rate(kv.Key, zone, rules));

        /// <summary>All the silver in it, in deniers, wherever you are.</summary>
        public double Silver(TradeRules rules) => All.Sum(kv => kv.Value * (rules.Coin(kv.Key)?.Deniers ?? 0.0));

        /// <summary>Takes a share (0-1) of every coin in it (raiders, tribute); returns the deniers' worth taken.</summary>
        public double TakeShare(double share, TradeRules rules)
        {
            share = Math.Max(0.0, Math.Min(1.0, share));
            double worth = 0.0;
            foreach (var kv in held.ToList())
            {
                double n = Divisible(kv.Key) ? kv.Value * share : Math.Floor(kv.Value * share);
                if (n <= 0.0)
                    continue;
                held[kv.Key] = kv.Value - n;
                worth += n * (rules?.Coin(kv.Key)?.Deniers ?? 0.0);
            }
            LastCoins = worth > 0.0 ? $"{worth:0} d's worth" : "";
            LastBack = "";
            return worth;
        }

        static bool Divisible(string coin) => coin == "hacksilver";

        /// <summary>The coins of the last payment or takings ("12 deniers and 3 pennies"), and the change given ("" if none).</summary>
        public string LastCoins { get; private set; } = "";
        public string LastBack { get; private set; } = "";

        /// <summary>Pays deniers' worth here. False (and nothing paid) if the purse can't. `how` says what changed hands.</summary>
        public bool Pay(double deniers, ZoneDef zone, TradeRules rules, out string how)
        {
            how = "";
            if (deniers <= 0.0)
                return true;
            if (Worth(zone, rules) + 1e-6 < deniers)
            {
                how = "not enough silver";
                return false;
            }
            // the local coin first, then the others, the least lost first; cut silver last (it can make up any sum)
            var order = All.Select(kv => kv.Key).OrderBy(c => c == zone?.coin ? 0 : Divisible(c) ? 2 : 1)
                           .ThenByDescending(c => Rate(c, zone, rules) / Math.Max(1e-6, rules.Coin(c)?.Deniers ?? 1.0)).ToList();
            double left = deniers;
            var paid = new List<string>();
            foreach (var coin in order)
            {
                if (left <= 1e-6)
                    break;
                double rate = Rate(coin, zone, rules);
                if (rate <= 0.0)
                    continue;
                double have = Count(coin);
                double take = Divisible(coin) ? Math.Min(have, Math.Round(left / rate + 0.049, 1)) : Math.Min(have, Math.Ceiling(left / rate - 1e-9));
                if (take <= 0.0)
                    continue;
                held[coin] = have - take;
                left -= take * rate;
                paid.Add(Amount(coin, take, rules));
            }
            how = "paid " + Join(paid);
            LastCoins = Join(paid);
            LastBack = "";
            if (left < -0.01)
            {
                // change, in the local coin (cut silver where silver goes by weight)
                string local = zone?.coin ?? "denier";
                double back = -left / Math.Max(1e-6, Rate(local, zone, rules));
                back = Divisible(local) ? Math.Round(back, 1) : Math.Floor(back + 1e-6);
                if (back > 0.0)
                {
                    Add(local, back);
                    how += ", " + Amount(local, back, rules) + " back";
                    LastBack = Amount(local, back, rules);
                }
            }
            return true;
        }

        /// <summary>Pays this much (deniers' worth, coins at their silver: no zone's loss, as at sea), with change; if the
        /// purse can't, everything in it. Returns the deniers' worth paid.</summary>
        public double PayUpTo(double deniers, TradeRules rules)
        {
            if (deniers <= 0.0)
                return 0.0;
            double have = Silver(rules);
            if (have <= deniers + 1e-6)
                return TakeShare(1.0, rules);
            Pay(deniers, null, rules, out _);
            return deniers;
        }

        /// <summary>Takes deniers' worth paid here, in the local coin (cut silver where silver goes by weight).</summary>
        public void Receive(double deniers, ZoneDef zone, TradeRules rules, out string how)
        {
            string local = zone?.coin ?? "denier";
            double n = deniers / Math.Max(1e-6, Rate(local, zone, rules));
            n = Divisible(local) ? Math.Round(n, 1) : Math.Round(n);
            Add(local, n);
            how = "took " + Amount(local, n, rules);
            LastCoins = Amount(local, n, rules);
            LastBack = "";
        }

        /// <summary>"12 deniers", "3.5 g of hacksilver".</summary>
        public static string Amount(string coin, double n, TradeRules rules)
        {
            var c = rules.Coin(coin);
            string name = c?.name ?? coin;
            if (Divisible(coin))
                return n.ToString("0.#") + " g of " + name;
            if (Math.Abs(n - 1.0) < 1e-9)
                name = name.EndsWith("ies") ? name.Substring(0, name.Length - 3) + "y" : name.EndsWith("s") ? name.Substring(0, name.Length - 1) : name;
            return n.ToString("0") + " " + name;
        }

        static string Join(List<string> parts) =>
            parts.Count == 0 ? "nothing" : parts.Count == 1 ? parts[0] : string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts[parts.Count - 1];
    }
}
