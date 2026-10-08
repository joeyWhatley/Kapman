using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TRADE - the market where she lies: one row for each good that is on offer here, wanted here, or aboard, with the
    /// price as seen (what they ask, what they pay), how many lie here and how many are aboard, and under it the word on
    /// it from elsewhere, said against the price here (Known.WordOn). Buying and selling are BY THE LOT (the alpha's row
    /// 5; Design/fixes/word_and_lots.md): one unit ("Buy", "Sell", at the end of the good's own line), and under the
    /// line half of what can go and all that can go, each button with its count and its sum on it, each one deal and
    /// one settlement (FrameDeal: the game's own Trade.Buy and Trade.Sell, with the toll, the laws and the coins).
    /// The window is BUILT only when its rows change (another port, a good sold out or bought in). The figures on it
    /// change in place a few times a second (Live): at the clock's fast speeds prices move at every look, and a window
    /// built again under the pointer loses the click.
    /// At sea there is no market: the window says what is known of what she carries.
    /// </summary>
    public sealed class TradePanel : FramePanel
    {
        public override string Id => "trade";
        public override string Title => "Trade";
        public override int Order => 20;

        /// <summary>One good's row: the labels and buttons whose words change in place. Of each side's three buttons the
        /// first deals one unit and stands in the good's line; half and all stand in the row under it.</summary>
        sealed class Row
        {
            public GoodDef good;
            public Label quote, aboard;
            public VisualElement word, sellLots, buyLots;
            public string wordSaid;
            public readonly Button[] buy = new Button[3], sell = new Button[3];
        }

        static readonly LotSize[] Lots = { LotSize.One, LotSize.Half, LotSize.All };

        readonly List<Row> rows = new List<Row>();
        Label purse, saidLabel;
        string said = "", saidAt = "";
        int wordKey;

        protected override void OnLoaded()
        {
            said = "";                                                   // (the last deal's words were of the game before)
            saidAt = "";
        }

        static IEnumerable<MarketGood> Goods(Market m) =>
            m.Goods.Where(g => g.OnOffer || g.wanted > 0f || Trade.Hold.Count(g.good.id) > 0)
             .OrderByDescending(g => Trade.Hold.Count(g.good.id) > 0)
             .ThenByDescending(g => g.OnOffer)
             .ThenBy(g => g.good.name);

        /// <summary>What makes the window be built again: where she lies, whether its market is open, and which goods
        /// have a row with which buttons. Not the prices: they change in place.</summary>
        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(FrameUi.Mix(17, here != null ? here.id : ""), Trade.Started);
            h = FrameUi.Mix(h, Self.Chosen);
            h = FrameUi.Mix(h, FirstJobs.Version);
            var m = FrameDeal.MarketHere();
            if (m == null || game == null)
            {
                // at sea: what she carries and what is known of it (no buttons here, so it may be built again each day)
                h = FrameUi.Mix(FrameUi.Mix(h, FrameGame.HoldStamp()), Known.Version);
                return FrameUi.Mix(h, game != null ? (int)System.Math.Floor(game.Day) : 0);
            }
            FrameDeal.See();
            h = FrameUi.Mix(h, m.Disrupted(game.Day));
            foreach (var g in Goods(m))
                h = FrameUi.Mix(FrameUi.Mix(h, g.good.id), (g.OnOffer ? 1 : 0) + (Trade.Hold.Count(g.good.id) > 0 ? 2 : 0));
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            rows.Clear();
            purse = saidLabel = null;
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
            var market = FrameDeal.MarketHere();
            FrameDeal.See();                                              // (looking at the market is seeing its prices)
            if (saidAt != here.id)
            {
                said = "";                                                // (the last deal's words were another market's)
                saidAt = here.id;
            }
            body.Add(FrameUi.H(Wording.MarketTitle(here)));
            double toll = market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day);
            body.Add(FrameUi.P($"Prices in {market.zone.unit}. " +
                               (toll > 0.0005 ? $"The toll takes {toll * 100.0:0.#} in the hundred of what you sell here." : "No toll is taken on what you sell here.")));
            saidLabel = FrameUi.Text(said);
            body.Add(saidLabel);
            if (market.Disrupted(game.Day))
            {
                body.Add(FrameUi.P(market.Ruined ? "Nobody trades here any more." : "The market is shut."));
                Live();
                return;
            }
            foreach (var g in Goods(market))
            {
                var row = new Row { good = g.good };
                bool held = Trade.Hold.Count(g.good.id) > 0;
                // one of each stands in the good's own line, as it always has; half and all in a row under it (the
                // window is narrow: a line with six buttons at its end had no room left for its words)
                Lot(row, true, g.OnOffer);
                Lot(row, false, held);
                row.aboard = held ? FrameUi.Num("") : null;
                // (Ravn's word of the first morning: the good he named is marked until she first casts off)
                var tip = FirstJobs.Listed && FirstJobs.TipGood == g.good.id ? FrameUi.Tag("Ravn's word") : null;
                // (on a row of its own over the good: beside the buttons it pushed Sell out of the line)
                if (tip != null)
                    body.Add(FrameUi.Row(tip));
                var line = FrameUi.Ln(g.good.name, " ", row.aboard, row.buy[0], row.sell[0]);
                row.quote = line.Q<Label>(className: "frame-ln-small");
                body.Add(line);
                // (a row for selling and a row for buying, two buttons each at most)
                row.sellLots = FrameUi.Row(row.sell[1], row.sell[2]);
                row.sellLots.AddToClassList("frame-lots");
                body.Add(row.sellLots);
                row.buyLots = FrameUi.Row(row.buy[1], row.buy[2]);
                row.buyLots.AddToClassList("frame-lots");
                body.Add(row.buyLots);
                row.word = new VisualElement();
                body.Add(row.word);
                rows.Add(row);
            }
            if (rows.Count == 0)
                body.Add(FrameUi.P("Nothing is bought or sold here now."));
            purse = FrameUi.P("");
            body.Add(purse);
            wordKey = 0;
            Live();
        }

        /// <summary>The three buttons of one side of a row (none where that side has nothing to deal): one unit ("Buy",
        /// "Sell"), half of what can go, all of it.</summary>
        void Lot(Row row, bool buy, bool any)
        {
            if (!any)
                return;
            var buttons = buy ? row.buy : row.sell;
            string id = row.good.id;
            for (int i = 0; i < Lots.Length; i++)
            {
                LotSize lot = Lots[i];
                buttons[i] = FrameUi.Btn(buy ? "Buy" : "Sell", () => DealLot(id, buy, lot), i == 0 ? (buy ? "Buy one" : "Sell one") : null);
                // (named for what it deals: "sell-half", "buy-all": its words change with the market)
                buttons[i].name = (buy ? "buy-" : "sell-") + (i == 0 ? "one" : i == 1 ? "half" : "all");
            }
        }

        /// <summary>The figures, in place: each row's quote and its lots' counts and sums, the word on each good when what
        /// is known has moved, the last deal's words, and what her silver is worth here with the room left.</summary>
        protected override void Live()
        {
            var game = KapmanGame.Current;
            var market = FrameDeal.MarketHere();
            if (game == null || market == null || saidLabel == null)
                return;
            var zone = market.zone;
            int month = game.Month;
            saidLabel.text = said;
            saidLabel.style.display = said.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (purse != null)
                purse.text = $"{FrameGame.SilverWords(market.port)} Room left in the hold: {Trade.Hold.Free:0} kg.";
            // the word on each good is asked again only when something was learned, the market was looked at, or the day turned
            int key = FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameDeal.Seen), (int)System.Math.Floor(game.Day));
            bool words = key != wordKey;
            wordKey = key;
            var ask = words ? Known.AskNow() : default;
            foreach (var row in rows)
            {
                var g = market.Get(row.good.id);
                if (g == null)
                    continue;
                string id = row.good.id;
                int held = Trade.Hold.Count(id);
                string asks = g.OnOffer ? $"They ask {zone.Show(Deal.AsksFor(market, g, month, "player"))}, {Mathf.FloorToInt(g.stock)} here. " : "None on offer. ";
                string pays = $"They pay {zone.Show(Deal.PaysFor(market, g, month, "player"))}.";
                string unit = string.IsNullOrEmpty(row.good.unit) ? "" : $" By the {row.good.unit.Split(' ', ',', '(')[0]}.";
                if (row.quote != null)
                    row.quote.text = asks + pays + unit;
                if (row.aboard != null)
                {
                    row.aboard.text = held.ToString();
                    row.aboard.tooltip = held > 0 ? $"{held} aboard. " + FrameGame.CostWords(Trade.Hold, row.good, held) : null;
                }
                row.sellLots.style.display = Lots3(row.sell, market, id, false, zone) ? DisplayStyle.Flex : DisplayStyle.None;
                row.buyLots.style.display = Lots3(row.buy, market, id, true, zone) ? DisplayStyle.Flex : DisplayStyle.None;
                if (!words)
                    continue;
                // (the word on a good says what they pay for it elsewhere: it is for what she carries or could buy here.
                // Under a good that is only wanted here it was the same sentence a dozen times over)
                if (held == 0 && !g.OnOffer)
                {
                    if (row.wordSaid != "")
                    {
                        row.wordSaid = "";
                        row.word.Clear();
                    }
                    continue;
                }
                var w = Known.WordOn(row.good, ask);
                string text = w.Text;
                if (text == row.wordSaid)
                    continue;
                row.wordSaid = text;
                row.word.Clear();
                row.word.Add(new WordLine(w));
            }
        }

        /// <summary>One side's lots, in a row under the good's line: "Sell 2 for 65 d" (half) and beside it "all 4 for
        /// 121 d" (alone, it says "Sell all 4 for 121 d"). Half is hidden where it is the same as one or as all; all is
        /// hidden where only one can go. True if either shows.</summary>
        static bool Lots3(Button[] b, Market market, string good, bool buy, ZoneDef zone)
        {
            if (b[0] == null)
                return false;
            var all = FrameDeal.Quote(market, good, buy, LotSize.All);
            int most = all.ok ? all.count : 0;
            int half = FrameDeal.CountOf(LotSize.Half, most);
            string verb = buy ? "Buy" : "Sell";
            // (a hidden button says nothing: only the first of a side ever reads "Buy" or "Sell")
            bool showHalf = half > 1 && half < most;
            b[1].style.display = showHalf ? DisplayStyle.Flex : DisplayStyle.None;
            b[1].text = showHalf ? $"{verb} {half} for {zone.Show(FrameDeal.Quote(market, good, buy, LotSize.Half).deniers)}" : "";
            b[2].style.display = most > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            b[2].text = most <= 1 ? "" : showHalf ? $"all {most} for {zone.Show(all.deniers)}" : $"{verb} all {most} for {zone.Show(all.deniers)}";
            return showHalf || most > 1;
        }

        /// <summary>Buys or sells a lot of a good at the market where she lies (the buttons of a row): one deal, one
        /// settlement, what came of it said in the window and in the log. Null when she lies at no market.</summary>
        public TradeResult DealLot(string good, bool buy, LotSize lot)
        {
            var r = FrameDeal.DealLot(good, buy, lot);
            if (r == null)
                return null;
            said = r.text ?? "";
            // (the good by its own name: the deal's words have it all in small letters, "rhenish wine")
            var named = GameData.Good(good);
            if (named != null)
                said = said.Replace(named.name.ToLowerInvariant(), Trade.Named(named));
            FrameLog.Add(said);
            Redraw();
            return r;
        }

        /// <summary>One unit (the name the first build of the frame gave it).</summary>
        public TradeResult DealOne(string good, bool buy) => DealLot(good, buy, LotSize.One);
    }
}
