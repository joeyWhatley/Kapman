using System;
using System.Collections.Generic;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// A TRADER'S MIND, in port. She sells what fetches here at least nearly what she believes it fetches anywhere she
    /// could take it; then weighs every port in her reach: what she could buy here and what she believes it sells for
    /// there (the prices she's seen or heard, fading over a month toward what everyone knows - cheap where it's made, dear
    /// where it's wanted), less tolls, spoilage and the crew's keep on the way; and picks one, the better the likelier
    /// (never quite the same choice for every trader, so they don't all rush one gap). She buys as she goes, unit by unit,
    /// and stops when the price no longer leaves a margin. She keeps a fifth of her silver back. Her dealing moves the
    /// market's stock and prices (Deal), as the player's does.
    /// </summary>
    public static class TraderBrain
    {
        /// <summary>What she believes a market pays for a good (deniers, before its toll), from what she knows.</summary>
        public static float BelievedPays(WorldSim w, SimShip s, int port, GoodDef g, int month)
        {
            var m = w.MarketAt(port);
            var mg = m?.Get(g.id);
            if (mg == null)
                return 0f;
            float usual = m.UsualPrice(mg, month) * (1f - m.Spread);
            var snap = s.knows.Of(port);
            if (snap == null || float.IsNaN(snap.pays[g.index]))
                return usual;
            float weight = (float)Math.Pow(0.5, Math.Max(0.0, w.Day - snap.day) / w.rules.beliefHalfLifeDays);
            return weight * snap.pays[g.index] + (1f - weight) * usual;
        }

        /// <summary>What she believes a market asks for a good (deniers), from what she knows; NaN if she doesn't expect to
        /// find it there (it isn't made there, and she hasn't lately seen it on offer).</summary>
        public static float BelievedAsks(WorldSim w, SimShip s, int port, MarketGood g, int month)
        {
            var m = w.MarketAt(port);
            float usual = g.usual >= 1f ? m.UsualPrice(g, month) * (1f + m.Spread) : float.NaN;
            var snap = s.knows.Of(port);
            float seen = snap != null ? snap.asks[g.good.index] : float.NaN;
            if (float.IsNaN(seen))
                return usual;
            float weight = (float)Math.Pow(0.5, Math.Max(0.0, w.Day - snap.day) / w.rules.beliefHalfLifeDays);
            if (float.IsNaN(usual))
                return weight >= 0.5f ? seen : float.NaN;                    // (brought there once: only if seen lately)
            return weight * seen + (1f - weight) * usual;
        }

        /// <summary>Sells, good by good, while what she gets here is near the best she believes she'd get elsewhere.</summary>
        public static void Sell(WorldSim w, SimShip s, Market m, int month, int year)
        {
            var aboard = new List<(GoodDef good, int count)>(s.hold.Contents);
            aboard.Sort((a, b) => a.good.index.CompareTo(b.good.index));
            foreach (var (good, count) in aboard)
            {
                var here = m.Get(good.id);
                if (here == null || Forbidden(good, m.zone, year))
                    continue;
                float best = 0f;
                foreach (int p in w.Reach(s))
                    if (p != m.port.index)
                        best = Math.Max(best, BelievedPays(w, s, p, good, month) * (1f - w.MarketAt(p).zone.toll));
                double costEach = s.hold.CostEach(good.id);
                int left = count, lot = Math.Max(1, w.rules.lotStep);
                while (left > 0)
                {
                    float net = m.Pays(here, month) * (1f - m.zone.toll);
                    bool worth = net >= w.rules.sellNearBest * best || (net >= costEach && s.at == s.home) || net >= costEach * 1.15;
                    if (!worth)
                        break;
                    var d = Deal.Sell(m, s.purse, s.hold, good.id, Math.Min(lot, left), month, w.Day, s.id);
                    if (!d.ok)
                        break;
                    s.voyageRevenue += d.deniers - d.toll;
                    left -= d.count;
                }
            }
        }

        /// <summary>Her crew are owed more than she has: she sells what she carries here, whatever it fetches, till she
        /// can pay them (good by good, in the table's order).</summary>
        public static void SellToPay(WorldSim w, SimShip s, Market m, int month, int year)
        {
            var aboard = new List<(GoodDef good, int count)>(s.hold.Contents);
            aboard.Sort((a, b) => a.good.index.CompareTo(b.good.index));
            foreach (var (good, count) in aboard)
            {
                if (m.Get(good.id) == null || Forbidden(good, m.zone, year))
                    continue;
                int left = count;
                while (left > 0 && s.purse.Worth(m.zone, TradeRules.Current) < s.owedWages)
                {
                    var d = Deal.Sell(m, s.purse, s.hold, good.id, Math.Min(Math.Max(1, w.rules.lotStep), left), month, w.Day, s.id);
                    if (!d.ok)
                        break;
                    s.voyageRevenue += d.deniers - d.toll;
                    left -= d.count;
                }
                if (s.purse.Worth(m.zone, TradeRules.Current) >= s.owedWages)
                    return;
            }
        }

        /// <summary>Chooses where to go next and buys for it. -1: nowhere worth it yet (she waits).</summary>
        public static int Plan(WorldSim w, SimShip s, Market m, int month, int year, bool goHome)
        {
            var rules = w.rules;
            double silver = s.purse.Worth(m.zone, TradeRules.Current);
            // a fifth kept back, and the crew's pay for a passage of a fortnight
            double spend = Math.Max(0.0, silver * (1.0 - rules.keepBack) - s.owedWages - 14.0 * s.cls.crew * rules.wagePerManDay);
            var options = new List<(int port, double score)>();
            var plans = new Dictionary<int, List<(MarketGood g, int units)>>();
            foreach (int p in w.Reach(s))
            {
                if (p == s.at || (goHome && p != s.home))
                    continue;
                var dm = w.MarketAt(p);
                double days = w.DaysTo(s, s.at, p);
                if (days <= 0.0)
                    continue;
                var plan = new List<(MarketGood g, int units)>();
                double profit = 0.0, room = s.hold.Free, cash = spend;
                // what she carries already (kept back from the market here): worth more there than here?
                foreach (var (good, count) in s.hold.Contents)
                {
                    if (Forbidden(good, dm.zone, year) || (good.keeps > 0 && days > good.keeps * 0.8))
                        continue;
                    var hg = m.Get(good.id);
                    float hereNet = hg != null && !Forbidden(good, m.zone, year) ? m.Pays(hg, month) * (1f - m.zone.toll) : 0f;
                    float there = BelievedPays(w, s, p, good, month) * (1f - dm.zone.toll);
                    if (there > hereNet)
                        profit += count * (there - hereNet) * 0.8;
                }
                var goods = new List<(MarketGood g, float margin)>();
                foreach (var g in m.Goods)
                {
                    if (!g.OnOffer || Forbidden(g.good, m.zone, year) || Forbidden(g.good, dm.zone, year))
                        continue;
                    if (g.good.keeps > 0 && days > g.good.keeps * 0.8)
                        continue;                                    // it would spoil on the way
                    float sells = BelievedPays(w, s, p, g.good, month) * (1f - dm.zone.toll);
                    float margin = sells - m.Asks(g, month);
                    if (margin > rules.minMargin * m.Asks(g, month))
                        goods.Add((g, margin));
                }
                goods.Sort((a, b) => (b.margin / b.g.good.Space).CompareTo(a.margin / a.g.good.Space));
                foreach (var (g, margin) in goods)
                {
                    float ask = m.Asks(g, month);
                    int byRoom = (int)Math.Floor(room / Math.Max(0.001, g.good.Space));
                    int byCash = (int)Math.Floor(cash / Math.Max(0.01, ask));
                    int byStock = (int)Math.Floor(g.stock * rules.stockShare);
                    int units = Math.Min(byRoom, Math.Min(byCash, Math.Min(byStock, rules.lotCap)));
                    if (units <= 0)
                        continue;
                    plan.Add((g, units));
                    profit += units * margin * 0.8;                // (the price rises as she buys)
                    room -= units * g.good.Space;
                    cash -= units * ask;
                }
                double keep = days * s.cls.crew * rules.keepPerManDay;
                double score = profit - keep + (p == s.home && goHome ? 1000.0 : 0.0);
                options.Add((p, score));
                plans[p] = plan;
            }
            if (options.Count == 0)
                return -1;
            options.Sort((a, b) => b.score != a.score ? b.score.CompareTo(a.score) : a.port.CompareTo(b.port));
            int choice;
            if (goHome)
                choice = options.Find(o => o.port == s.home).port;
            else
            {
                if (options[0].score <= 0.0)
                {
                    // nothing worth carrying from here: in ballast to where she can buy what sells back here or at home
                    // (weighed every time at home; away, now and then: she's mostly waiting for a cargo or heading home)
                    bool look = s.hold.Lots.Count == 0 && (s.at == s.home || s.rng.Chance(0.25));
                    int empty = look ? Ballast(w, s, month, year, spend) : -1;
                    if (empty >= 0)
                        s.cargoNote = "in ballast";
                    return empty;
                }
                // the better the likelier: a softmax over the best few
                int n = Math.Min(rules.considerTop, options.Count);
                double t = Math.Max(1.0, rules.choiceTemperature * options[0].score), sum = 0.0;
                var weights = new double[n];
                for (int i = 0; i < n; i++)
                    sum += weights[i] = options[i].score > 0.0 ? Math.Exp((options[i].score - options[0].score) / t) : 0.0;
                double r = s.rng.Next() * sum;
                choice = options[0].port;
                for (int i = 0; i < n; i++)
                {
                    r -= weights[i];
                    if (r <= 0.0)
                    {
                        choice = options[i].port;
                        break;
                    }
                }
            }
            // buy for it, a few at a time, while the rising price still leaves a margin
            var target = w.MarketAt(choice);
            var notes = new List<string>();
            foreach (var (g, units) in plans[choice])
            {
                int bought = 0, lot = Math.Max(1, rules.lotStep);
                float sells = BelievedPays(w, s, choice, g.good, month) * (1f - target.zone.toll);
                while (bought < units && m.Asks(g, month) < sells * (1f - rules.minMargin))
                {
                    var d = Deal.Buy(m, s.purse, s.hold, g.good.id, Math.Min(lot, units - bought), month, w.Day, s.id);
                    if (!d.ok)
                        break;
                    s.voyageCost += d.deniers;
                    bought += d.count;
                    if (d.why.Length > 0)
                        break;
                }
                if (bought > 0)
                    notes.Add($"{bought} {g.good.id}");
            }
            s.cargoNote = string.Join(" ", notes);
            return choice;
        }

        /// <summary>A passage in ballast: to the port where she believes she can buy the most worth bringing back (home, or
        /// here when she's at home), less the crew's keep both ways. -1: none worth it.</summary>
        static int Ballast(WorldSim w, SimShip s, int month, int year, double spend)
        {
            var rules = w.rules;
            int best = -1;
            double bestScore = 0.0;
            foreach (int p in w.Reach(s))
            {
                if (p == s.at)
                    continue;
                int back = s.home != p ? s.home : s.at;
                double go = w.DaysTo(s, s.at, p), ret = w.DaysTo(s, p, back);
                if (go <= 0.0 || ret <= 0.0)
                    continue;
                var pm = w.MarketAt(p);
                var bm = w.MarketAt(back);
                var goods = new List<(GoodDef g, float ask, float margin)>();
                foreach (var g in pm.Goods)
                {
                    if (Forbidden(g.good, pm.zone, year) || Forbidden(g.good, bm.zone, year))
                        continue;
                    if (g.good.keeps > 0 && ret > g.good.keeps * 0.8)
                        continue;
                    float ask = BelievedAsks(w, s, p, g, month);
                    if (float.IsNaN(ask) || ask <= 0f)
                        continue;
                    float margin = BelievedPays(w, s, back, g.good, month) * (1f - bm.zone.toll) - ask;
                    if (margin > rules.minMargin * ask)
                        goods.Add((g.good, ask, margin));
                }
                goods.Sort((a, b) => (b.margin / b.g.Space).CompareTo(a.margin / a.g.Space));
                double profit = 0.0, room = s.hold.Free, cash = spend;
                foreach (var (g, ask, margin) in goods)
                {
                    int units = Math.Min((int)Math.Floor(room / Math.Max(0.001, g.Space)), Math.Min((int)Math.Floor(cash / Math.Max(0.01, ask)), rules.lotCap));
                    if (units <= 0)
                        continue;
                    profit += units * margin * 0.6;                        // (believed from afar; the price rises as she buys)
                    room -= units * g.Space;
                    cash -= units * ask;
                }
                double score = profit - (go + ret) * s.cls.crew * rules.keepPerManDay;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }
            return best;
        }

        static bool Forbidden(GoodDef g, ZoneDef zone, int year) =>
            !string.IsNullOrEmpty(g.law) && TradeRules.Current.LawsOn(g, zone, year).Count > 0;
    }
}
