using System;

namespace Kapman.Game
{
    /// <summary>What a deal came to, as numbers (no words): units, deniers, the toll, what the goods had cost, and why
    /// it stopped short or failed.</summary>
    public struct DealResult
    {
        public bool ok;
        public int count;
        /// <summary>Deniers that changed hands (a sale: before the toll), the toll taken, and what the goods sold had cost.</summary>
        public double deniers, toll, cost;
        /// <summary>Why it failed, or stopped short of the count asked: "not_dealt", "none_left", "no_room", "no_silver",
        /// "none_aboard", "nothing", "disrupted" (the market shut for a while), "ruined" (shut for good) ("" if it went through in full).</summary>
        public string why;
    }

    /// <summary>A deal done in a market, by anyone (the player or another ship): for the statistics, and so an open
    /// market panel shows what others' dealing did to its prices.</summary>
    public struct DealInfo
    {
        public string port, good, who;
        public bool bought;
        public int count;
        public double deniers, toll;
        public double day;
    }

    /// <summary>
    /// THE DEAL - buying and selling in a market, for any merchant: the player (through <see cref="Trade"/>, which adds
    /// the words and tells the graphs) and the other ships of the world. Everyone deals with the same market the same
    /// way, so what others buy makes it scarce and dear and what they sell gluts it (the user: "what they sell and buy
    /// affects the market of course"). Prices move unit by unit: each bought makes the next dearer, each sold the next
    /// cheaper; sales pay the zone's toll.
    /// </summary>
    public static class Deal
    {
        /// <summary>Every deal done, by anyone.</summary>
        public static event Action<DealInfo> Dealt;

        /// <summary>What the market asks this buyer for one unit: the merchant's standing there moves it (Reputation).</summary>
        public static float AsksFor(Market m, MarketGood g, int month, string who) =>
            m.Asks(g, month) * (float)(who == "player" ? 1.0 - Reputation.PriceFavour(m.port) : 1.0);

        /// <summary>What the market pays this seller for one unit (the merchant's standing moves it).</summary>
        public static float PaysFor(Market m, MarketGood g, int month, string who) =>
            m.Pays(g, month) * (float)(who == "player" ? 1.0 + Reputation.PriceFavour(m.port) : 1.0);

        /// <summary>Buys up to `count` units: as many as there are on offer, room for, and silver to pay.</summary>
        public static DealResult Buy(Market m, Purse purse, Hold hold, string goodId, int count, int month, double day, string who)
        {
            var rules = TradeRules.Current;
            var r = new DealResult { why = "" };
            var g = m?.Get(goodId);
            if (g == null || count <= 0)
                return Fail(ref r, "not_dealt");
            if (m.Disrupted(day))
                return Fail(ref r, m.RuinedOn(day) ? "ruined" : "disrupted");
            int room = hold.RoomFor(g.good);
            double worth = purse.Worth(m.zone, rules);
            double total = 0.0;
            int n = 0;
            float stock = g.stock;
            while (n < count && n < room && stock - n >= 1f)
            {
                g.stock = stock - n;
                double each = AsksFor(m, g, month, who);
                if (total + each > worth + 1e-6)
                    break;
                total += each;
                n++;
            }
            g.stock = stock;                                         // nothing's changed hands yet
            if (n == 0)
                return Fail(ref r, g.stock < 1f ? "none_left" : room <= 0 ? "no_room" : "no_silver");
            total = Math.Round(total, 2);
            if (!purse.Pay(total, m.zone, rules, out _))
                return Fail(ref r, "no_silver");
            g.stock = stock - n;
            hold.Add(g.good, n, total, day);
            r.ok = true;
            r.count = n;
            r.deniers = total;
            if (n < count)
                r.why = g.stock < 1f ? "none_left" : n >= room ? "no_room" : "no_silver";
            Dealt?.Invoke(new DealInfo { port = m.port.id, good = goodId, who = who, bought = true, count = n, deniers = total, day = day });
            return r;
        }

        /// <summary>Sells up to `count` units from the hold; the zone's toll comes off what the market pays.</summary>
        public static DealResult Sell(Market m, Purse purse, Hold hold, string goodId, int count, int month, double day, string who)
        {
            var rules = TradeRules.Current;
            var r = new DealResult { why = "" };
            var good = GameData.Good(goodId);
            if (good == null || count <= 0)
                return Fail(ref r, "nothing");
            var g = m?.Get(goodId);
            if (g == null)
                return Fail(ref r, "not_dealt");
            if (m.Disrupted(day))
                return Fail(ref r, m.RuinedOn(day) ? "ruined" : "disrupted");
            int n = Math.Min(count, hold.Count(goodId));
            if (n <= 0)
                return Fail(ref r, "none_aboard");
            double total = 0.0;
            for (int i = 0; i < n; i++)
            {
                total += PaysFor(m, g, month, who);
                m.GotOne(g);                                         // each unit sold makes the next cheaper
            }
            total = Math.Round(total, 2);
            // (the merchant's own sales are toll-free where a riddle won that: Chest.TollFreeAt)
            bool free = who == "player" && Chest.TollFreeAt(m.port, KapmanGame.Current != null ? KapmanGame.Current.Year : 793);
            double toll = free ? 0.0 : Math.Round(total * m.zone.toll * (who == "player" ? EncounterRunner.TollFactor(m.port.id, day) : 1.0), 2);
            r.cost = hold.Remove(good, n);
            purse.Receive(total - toll, m.zone, rules, out _);
            r.ok = true;
            r.count = n;
            r.deniers = total;
            r.toll = toll;
            Dealt?.Invoke(new DealInfo { port = m.port.id, good = goodId, who = who, bought = false, count = n, deniers = total, toll = toll, day = day });
            return r;
        }

        static DealResult Fail(ref DealResult r, string why)
        {
            r.ok = false;
            r.why = why;
            return r;
        }
    }
}
