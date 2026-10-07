using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WARES - all that is known of every good (the user, 7 Oct 2026, late: "we need a market button with all known
    /// things about all known products, so at the start you know basically no products but you learn it and it gets
    /// added here, then you can further investigate and analyse the product: where it's needed or where it's not").
    ///
    /// A ware comes into the window when it has been seen sold, heard priced, or carried (the hold, and every price
    /// note of the ledger: Trade.Ledger). The list gives each with where it is best sold and where it is cheapest, as
    /// far as is known; a ware picked opens its own leaf: what it is, how it is counted and what it weighs, how long it
    /// keeps, any law on it, and every market it is known at, the best payers first, each with how the word came and
    /// how old it is. Nothing here is the world's own truth: it is the merchant's own notes.
    /// </summary>
    public sealed class WaresPanel : FramePanel
    {
        public override string Id => "wares";
        public override string Title => "Wares";
        public override int Order => 46;

        string picked = "";

        struct At
        {
            public PortDef port;
            public PriceNote note;
            public SeenPrice price;
        }

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameGame.HoldStamp());
            h = FrameUi.Mix(h, Trade.Started ? Trade.Ledger.Count : 0);
            h = FrameUi.Mix(h, picked);
            return FrameUi.Mix(h, game != null ? (int)Math.Floor(game.Day) : 0);
        }

        protected override void OnLoaded() => picked = "";

        /// <summary>Every ware known, each with the markets it is known at (the freshest note of each market).</summary>
        static Dictionary<string, List<At>> Known_()
        {
            var known = new Dictionary<string, List<At>>(StringComparer.Ordinal);
            if (!Trade.Started)
                return known;
            foreach (var (good, _) in Trade.Hold.Contents)
                if (!known.ContainsKey(good.id))
                    known[good.id] = new List<At>();
            foreach (var note in Trade.Ledger.Notes)
            {
                var port = GameData.Port(note.port);
                if (port == null)
                    continue;
                foreach (var kv in note.prices)
                {
                    if (GameData.Good(kv.Key) == null)
                        continue;
                    if (!known.TryGetValue(kv.Key, out var list))
                        known[kv.Key] = list = new List<At>();
                    list.Add(new At { port = port, note = note, price = kv.Value });
                }
            }
            return known;
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("No ware is known to you yet."));
                return;
            }
            var known = Known_();
            var good = GameData.Good(picked);
            if (good != null && known.ContainsKey(good.id))
            {
                Leaf(body, good, known[good.id], game.Day);
                return;
            }
            picked = "";
            body.Add(FrameUi.H(known.Count == 0 ? "No wares known" : known.Count == 1 ? "One ware known" : $"{known.Count} wares known"));
            body.Add(FrameUi.P("A ware comes in here when you see it sold, hear its price or carry it. Pick one for all you know of it."));
            foreach (var kv in known.OrderBy(k => GameData.Good(k.Key).name, StringComparer.OrdinalIgnoreCase))
            {
                var g = GameData.Good(kv.Key);
                string id = g.id;
                body.Add(FrameUi.LnButton(g.name, Short(kv.Value), () => { picked = id; Redraw(); }));
            }
        }

        /// <summary>"Best paid at Medemelacha, 13 d. Cheapest at Dorestat, 7 d." as far as is known.</summary>
        static string Short(List<At> at)
        {
            if (at.Count == 0)
                return "Aboard. No market's price for it is known yet.";
            var pays = at.OrderByDescending(a => a.price.pays).First();
            var sold = at.Where(a => a.price.onOffer).OrderBy(a => a.price.asks).ToList();
            string s = $"Best paid at {pays.port.name}, {pays.price.pays:0} d.";
            if (sold.Count > 0)
                s += $" Cheapest at {sold[0].port.name}, {sold[0].price.asks:0} d.";
            return s;
        }

        void Leaf(VisualElement body, GoodDef good, List<At> at, double today)
        {
            body.Add(FrameUi.Row(FrameUi.Btn("All wares", () => { picked = ""; Redraw(); })));
            body.Add(FrameUi.H(good.name));
            if (!string.IsNullOrEmpty(good.note))
                body.Add(FrameUi.P(good.note));
            string counted = $"Counted by the {good.unit}: {good.kg:0.#} kg each" + (good.keeps > 0 ? $". Keeps {good.keeps} days" : ". It keeps") + ".";
            body.Add(FrameUi.P(counted));
            if (!string.IsNullOrEmpty(good.law))
                body.Add(FrameUi.P("The law: " + good.law));
            int aboard = 0;
            foreach (var (g, n) in Trade.Hold.Contents)
                if (g.id == good.id)
                    aboard += n;
            if (aboard > 0)
                body.Add(FrameUi.P("Aboard: " + FrameGame.Units(good, aboard) + "."));
            if (at.Count == 0)
            {
                body.Add(FrameUi.P("No market's price for it is known yet. Look at a market, or sit with the masters in an alehouse."));
                return;
            }
            body.Add(FrameUi.H("Where it is wanted"));
            foreach (var a in at.OrderByDescending(x => x.price.pays))
                body.Add(FrameUi.Ln(a.port.name, $"They pay {a.price.pays:0} d. {How_(a.note)}.", FrameUi.Tag(FrameUi.Ago(today - a.note.day))));
            var sold = at.Where(a => a.price.onOffer).OrderBy(a => a.price.asks).ToList();
            if (sold.Count > 0)
            {
                body.Add(FrameUi.H("Where it is sold"));
                foreach (var a in sold)
                    body.Add(FrameUi.Ln(a.port.name, $"They ask {a.price.asks:0} d. {How_(a.note)}.", FrameUi.Tag(FrameUi.Ago(today - a.note.day))));
            }
            if (sold.Count > 0)
            {
                var best = at.OrderByDescending(x => x.price.pays).First();
                var cheap = sold[0];
                if (best.port.id != cheap.port.id && best.price.pays > cheap.price.asks)
                    body.Add(FrameUi.P($"By your notes: bought at {cheap.port.name} and sold at {best.port.name} it would gain {best.price.pays - cheap.price.asks:0} d a {good.unit}, before toll and bread. A price goes half stale in a month."));
            }
        }

        static string How_(PriceNote n)
        {
            switch (n.how)
            {
                case How.Seen: return "Seen with your own eyes";
                case How.Told: return "Told by one who saw it" + (string.IsNullOrEmpty(n.source) ? "" : ": " + n.source);
                case How.Written: return "Written word" + (string.IsNullOrEmpty(n.source) ? "" : ": " + n.source);
                case How.Rumour: return "A rumour" + (string.IsNullOrEmpty(n.source) ? "" : ": " + n.source);
                default: return "Talk" + (string.IsNullOrEmpty(n.source) ? "" : ": " + n.source);
            }
        }
    }
}
