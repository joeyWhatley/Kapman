using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE WORD - what is known of prices, by port and by age: every market the merchant has a price note for
    /// (Trade.Ledger: seen there, or told, or talk), the freshest first, each with how the word came and how old it is,
    /// and under it the word on what she carries now (Known.Word: what it fetched there, how sure, how stale).
    /// </summary>
    public sealed class WordPanel : FramePanel
    {
        public override string Id => "word";
        public override string Title => "The word";
        public override int Order => 50;

        const int MostPorts = 16, MostGoods = 3;

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameGame.HoldStamp());
            h = FrameUi.Mix(h, Trade.Started ? Trade.Ledger.Count : 0);
            return FrameUi.Mix(h, game != null ? (int)System.Math.Floor(game.Day) : 0);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("No word of any market yet."));
                return;
            }
            var notes = Trade.Ledger.Notes.OrderByDescending(n => n.day).ToList();
            if (notes.Count == 0)
            {
                body.Add(FrameUi.P("You have no word of any market's prices yet. Look at a market, or talk to people in port."));
                return;
            }
            double freshest = game.Day - notes[0].day;
            body.Add(FrameUi.H(notes.Count == 1 ? "You know of one market" : $"You know of {notes.Count} markets"));
            body.Add(FrameUi.P($"The freshest word is {(freshest < 1.0 ? "from today" : freshest < 2.0 ? "a day old" : $"{freshest:0} days old")}. " +
                               "A price goes half stale in a month."));
            var goods = new List<GoodDef>();
            foreach (var (good, _) in Trade.Hold.Contents)
                if (goods.Count < MostGoods)
                    goods.Add(good);
            var ask = Known.AskNow();
            string here = Port.LyingId();
            int shown = 0;
            foreach (var n in notes)
            {
                var port = GameData.Port(n.port);
                if (port == null || shown >= MostPorts)
                    continue;
                shown++;
                double age = game.Day - n.day;
                string how = HowWords(n) + (string.IsNullOrEmpty(n.source) || n.how == How.Seen ? "" : ": " + n.source);
                body.Add(FrameUi.Ln(port.name + (n.port == here ? ", here" : ""), how + ".", FrameUi.Tag(Age(age))));
                if (n.port == here)
                    continue;                                             // (the Trade window has this market in full)
                foreach (var good in goods)
                    body.Add(new WordLine(Known.Word(good, port, ask)));
            }
            if (notes.Count > shown)
                body.Add(FrameUi.P($"And {notes.Count - shown} more, older."));
            if (goods.Count == 0)
                body.Add(FrameUi.P("With goods aboard, each market here says what they fetch there."));
        }

        static string HowWords(PriceNote n)
        {
            switch (n.how)
            {
                case How.Seen: return "Seen with your own eyes";
                case How.Told: return "Told by one who saw it";
                case How.Written: return "Written word";
                case How.Rumour: return "A rumour";
                case How.Lore: return "What every merchant knows";
                default: return n.hops > 1 ? "Talk, at second hand or worse" : "Talk";
            }
        }

        static string Age(double days) => days < 1.0 ? "today" : days < 2.0 ? "a day old" : days < 60.0 ? $"{days:0} days old" : $"{days / 30.0:0} months old";
    }
}
