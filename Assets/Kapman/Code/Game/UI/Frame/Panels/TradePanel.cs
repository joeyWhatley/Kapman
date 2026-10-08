using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TRADE - the market where she lies: each good that is on offer here, wanted here, or aboard, with the price as seen
    /// (what they ask, what they pay), how many lie here and how many are aboard, the word on it from elsewhere
    /// (Known.WordOn: the freshest thing seen or heard, else the season or the law), and buying and selling by the unit.
    /// The deals are the game's own (Trade.Buy, Trade.Sell: the toll, the laws, the coins). At sea there is no market:
    /// the window says what is known of what she carries.
    /// </summary>
    public sealed class TradePanel : FramePanel
    {
        public override string Id => "trade";
        public override string Title => "Trade";
        public override int Order => 20;

        string said = "", notedAt = "";
        int notedDay = int.MinValue;

        protected override void OnLoaded()
        {
            said = "";                                                   // (the last deal's words were of the game before)
            notedAt = "";
            notedDay = int.MinValue;
        }

        static Market MarketHere(PortDef here)
        {
            var game = KapmanGame.Current;
            return here != null && game != null && Trade.Started ? Markets.Of(here, game.Year, game.Day) : null;
        }

        static IEnumerable<MarketGood> Shown(Market m) =>
            m.Goods.Where(g => g.OnOffer || g.wanted > 0f || Trade.Hold.Count(g.good.id) > 0)
             .OrderByDescending(g => Trade.Hold.Count(g.good.id) > 0)
             .ThenByDescending(g => g.OnOffer)
             .ThenBy(g => g.good.name);

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(FrameUi.Mix(17, here != null ? here.id : ""), FrameGame.HoldStamp());
            h = FrameUi.Mix(FrameUi.Mix(h, (int)FrameGame.Silver), said);
            h = FrameUi.Mix(h, Known.Version);
            h = FrameUi.Mix(h, Self.Chosen);
            h = FrameUi.Mix(h, FirstJobs.Version);
            var m = MarketHere(here);
            if (m != null && game != null)
            {
                h = FrameUi.Mix(h, m.Disrupted(game.Day));
                foreach (var g in m.Goods)
                    if (g.OnOffer)
                        h = FrameUi.Mix(FrameUi.Mix(h, Mathf.FloorToInt(g.stock)), (int)(m.Price(g, game.Month) * 4f));
            }
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("No trade yet."));
                return;
            }
            var here = FrameGame.PortHere;
            var ask = Known.AskNow();
            if (here == null)
            {
                body.Add(FrameUi.H("No market here"));
                body.Add(FrameUi.P("She lies at no market. What you know of what she carries:"));
                int n = 0;
                foreach (var (good, count) in Trade.Hold.Contents)
                {
                    n++;
                    body.Add(FrameUi.Ln(FrameGame.Units(good, count), null));
                    body.Add(new WordLine(Known.WordOn(good, ask)));
                }
                if (n == 0)
                    body.Add(FrameUi.P("She is empty."));
                return;
            }
            var market = MarketHere(here);
            int month = game.Month;
            // looking at the market is seeing its prices: they go in the price notes (once a call and a day)
            int today = (int)System.Math.Floor(game.Day);
            // (not before a new game has chosen who it is: she lies at Dorestat while it asks, and a Dane who then began
            // at Ripa "saw ship's bread pay 4.3 d at Dorestat, this morning")
            bool begun = Self.Chosen || ChartWorld.Current == null;
            if (begun && (notedAt != here.id || notedDay != today))
            {
                if (notedAt != here.id)
                    said = "";                                            // (the last deal's words were another market's)
                notedAt = here.id;
                notedDay = today;
                Trade.Ledger.Note(market, game.Day, month);
            }
            body.Add(FrameUi.H(Wording.MarketTitle(here)));
            double toll = market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day);
            body.Add(FrameUi.P($"Prices in {market.zone.unit}. " +
                               (toll > 0.0005 ? $"The toll takes {toll * 100.0:0.#} in the hundred of what you sell here." : "No toll is taken on what you sell here.")));
            if (said.Length > 0)
                body.Add(FrameUi.Text(said));
            if (market.Disrupted(game.Day))
            {
                body.Add(FrameUi.P(market.Ruined ? "Nobody trades here any more." : "The market is shut."));
                return;
            }
            int rows = 0;
            foreach (var g in Shown(market))
            {
                rows++;
                var good = g.good;
                int held = Trade.Hold.Count(good.id);
                string asks = g.OnOffer ? $"They ask {market.zone.Show(Deal.AsksFor(market, g, month, "player"))}, {Mathf.FloorToInt(g.stock)} here. " : "None on offer. ";
                string pays = $"They pay {market.zone.Show(Deal.PaysFor(market, g, month, "player"))}.";
                string unit = string.IsNullOrEmpty(good.unit) ? "" : $" By the {good.unit.Split(' ')[0]}.";
                var buy = g.OnOffer ? FrameUi.Btn("Buy", () => DealOne(good.id, true), "Buy one") : null;
                var sell = held > 0 ? FrameUi.Btn("Sell", () => DealOne(good.id, false), "Sell one") : null;
                // (Ravn's word of the first morning: the good he named is marked until she first casts off)
                var word = FirstJobs.Listed && FirstJobs.TipGood == good.id ? FrameUi.Tag("Ravn's word") : null;
                var line = FrameUi.Ln(good.name, asks + pays + unit, word, held > 0 ? FrameUi.Num(held.ToString()) : null, buy, sell);
                if (held > 0)
                    line.tooltip = $"{held} aboard. " + FrameGame.CostWords(Trade.Hold, good, held);
                body.Add(line);
                body.Add(new WordLine(Known.WordOn(good, ask)));
            }
            if (rows == 0)
                body.Add(FrameUi.P("Nothing is bought or sold here now."));
            body.Add(FrameUi.P($"{FrameGame.SilverWords(here)} Room left in the hold: {Trade.Hold.Free:0} kg."));
        }

        /// <summary>Buys or sells one unit of a good at the market where she lies (the Buy and Sell buttons): the game's own
        /// deal, what came of it said in the window and in the log. Null when she lies at no market.</summary>
        public TradeResult DealOne(string good, bool buy)
        {
            var game = KapmanGame.Current;
            var market = MarketHere(FrameGame.PortHere);
            if (game == null || market == null)
                return null;
            var r = buy ? Trade.Buy(market, good, 1, game.Day, game.Month, game.Year) : Trade.Sell(market, good, 1, game.Day, game.Month, game.Year);
            said = r.text ?? "";
            // (the good by its own name: the deal's words have it all in small letters, "rhenish wine")
            var named = GameData.Good(good);
            if (named != null)
                said = said.Replace(named.name.ToLowerInvariant(), Trade.Named(named));
            if (r.ok)
                Trade.Ledger.Note(market, game.Day, game.Month);        // (the market as it is after the deal)
            FrameLog.Add(said);
            Redraw();
            return r;
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
