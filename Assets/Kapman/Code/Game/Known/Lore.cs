using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// LORE - what every merchant of the time knows without having been anywhere lately: a good is cheap where it's made,
    /// dear where it's wanted, cheaper in its cheap months and dearer in its dear ones (the goods table). It's the same
    /// "usual price" the markets and the AI traders work from (Market.UsualPrice), worked out from the tables alone, so
    /// reading it never touches a market (asking a market brings it up to date, which the world's dice depend on).
    /// </summary>
    public static class Lore
    {
        /// <summary>The usual price of one unit at a port in a month (deniers), before what a market asks or pays on top.</summary>
        public static float UsualPrice(PortDef port, GoodDef g, int month)
        {
            var rules = TradeRules.Current;
            if (port == null || g == null || rules == null)
                return 0f;
            float made = g.MadeIn(port.region), wanted = g.WantedIn(port.region);
            float p = g.value * GameData.MadeFactor(made) * GameData.WantedFactor(wanted) * Season(g, month, rules);
            if (made <= 0f && wanted <= 0f)
                p *= rules.unwanted;                                     // nobody there makes it or wants it
            return Mathf.Max(0.05f, p);
        }

        /// <summary>What a market usually pays a seller (deniers, before its toll).</summary>
        public static float UsualPays(PortDef port, GoodDef g, int month)
        {
            var rules = TradeRules.Current;
            return rules == null ? 0f : UsualPrice(port, g, month) * (1f - rules.spread * 0.5f);
        }

        /// <summary>What a market usually asks a buyer (deniers); NaN where it isn't made, so nobody expects it on offer.</summary>
        public static float UsualAsks(PortDef port, GoodDef g, int month)
        {
            var rules = TradeRules.Current;
            if (rules == null || port == null || g == null || g.MadeIn(port.region) <= 0f)
                return float.NaN;
            return UsualPrice(port, g, month) * (1f + rules.spread * 0.5f);
        }

        static float Season(GoodDef g, int month, TradeRules rules)
        {
            if (System.Array.IndexOf(g.cheap, month) >= 0) return rules.cheap;
            if (System.Array.IndexOf(g.dear, month) >= 0) return rules.dear;
            return 1f;
        }
    }
}
