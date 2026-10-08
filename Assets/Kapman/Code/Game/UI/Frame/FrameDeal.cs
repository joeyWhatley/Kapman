using System;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>How much of a good one button deals in: one unit, half of what can go, or all that can go.</summary>
    public enum LotSize { One, Half, All }

    /// <summary>
    /// DEALING BY THE LOT, and SEEING THE MARKET SHE LIES AT (the strategy critic, 7 October: "deals by the lot with one
    /// settling"). A lot is one deal of the game's own (Trade.Buy, Trade.Sell with a count): the price moves unit by unit
    /// inside it as it always did, and the purse is asked ONCE for the sum, so there is one rounding where a click a unit
    /// made one for each. "All" is all that can go: for buying, what is on offer, what the hold has room for and what the
    /// silver pays; for selling, all of the good aboard. "Half" is half of that, rounded up.
    /// The market she lies at is SEEN (its prices go into the price notes) when she moors, once each day she lies
    /// there, and again after every deal, whatever windows are up: the word on a good is said against that note
    /// (Known.Here), so it must be there before the first window is opened.
    /// </summary>
    public static class FrameDeal
    {
        static string seenAt = "";
        static int seenDay = int.MinValue;

        /// <summary>Counts up each time the market she lies at is noted (a panel's stamp).</summary>
        public static int Seen { get; private set; }

        /// <summary>The market where she lies moored, brought up to now; null at sea or with no trade begun.</summary>
        public static Market MarketHere()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            return here != null && game != null && Trade.Started ? Markets.Of(here, game.Year, game.Day) : null;
        }

        /// <summary>She looks at the market she lies at: its prices go into the price notes, once a call and a day
        /// (`again`: now, whatever was noted: after a deal has moved them). True when a note was made.</summary>
        public static bool See(bool again = false)
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            if (game == null || here == null || !Trade.Started)
            {
                if (here == null)
                    seenAt = "";                                          // (cast off: the next mooring is a new call)
                return false;
            }
            // (not before a new game has chosen who it is: she lies at Dorestat while it asks, and a Dane who then began at
            // Ripa "saw ship's bread pay 4.3 d at Dorestat, this morning")
            if (!Self.Chosen && ChartWorld.Current != null)
                return false;
            int today = (int)Math.Floor(game.Day);
            if (!again && seenAt == here.id && seenDay == today)
                return false;
            var market = Markets.Of(here, game.Year, game.Day);
            if (market == null)
                return false;
            seenAt = here.id;
            seenDay = today;
            Trade.Ledger.Note(market, game.Day, game.Month);
            Seen++;
            return true;
        }

        /// <summary>How many units make a lot, of the most that can go: one, half of it rounded up, or all of it.</summary>
        public static int CountOf(LotSize lot, int most) =>
            most <= 0 ? 0 : lot == LotSize.One ? 1 : lot == LotSize.All ? most : (most + 1) / 2;

        /// <summary>What a lot would come to if it were dealt now (nothing changes hands): how many units, the sum, and
        /// for a sale the toll and what the goods had cost. Not ok, with why, when nothing can go.</summary>
        public static DealResult Quote(Market m, string good, bool buy, LotSize lot)
        {
            var game = KapmanGame.Current;
            if (m == null || game == null || !Trade.Started)
                return new DealResult { why = "not_dealt" };
            var all = buy ? Deal.QuoteBuy(m, Trade.Purse, Trade.Hold, good, int.MaxValue, game.Month, game.Day, "player")
                          : Deal.QuoteSell(m, Trade.Hold, good, int.MaxValue, game.Month, game.Day, "player");
            if (!all.ok || lot == LotSize.All)
                return all;
            int n = CountOf(lot, all.count);
            return buy ? Deal.QuoteBuy(m, Trade.Purse, Trade.Hold, good, n, game.Month, game.Day, "player")
                       : Deal.QuoteSell(m, Trade.Hold, good, n, game.Month, game.Day, "player");
        }

        /// <summary>Buys or sells a lot of a good at the market where she lies: ONE deal and one settlement, in the
        /// game's own words. The count is worked out at the click, from the market as it then is. Where nothing can go,
        /// one unit is asked for, so the deal itself says why not. Null when she lies at no market.</summary>
        public static TradeResult DealLot(string good, bool buy, LotSize lot)
        {
            var game = KapmanGame.Current;
            var market = MarketHere();
            if (game == null || market == null)
                return null;
            int n = lot == LotSize.One ? 1 : Math.Max(1, Quote(market, good, buy, lot).count);
            var r = buy ? Trade.Buy(market, good, n, game.Day, game.Month, game.Year)
                        : Trade.Sell(market, good, n, game.Day, game.Month, game.Year);
            if (r.ok)
                See(true);                                                // (the market as it is after the deal)
            return r;
        }

        // Enter Play Mode runs without a domain reload: each play begins having seen no market
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            seenAt = "";
            seenDay = int.MinValue;
            Seen = 0;
        }
    }
}
