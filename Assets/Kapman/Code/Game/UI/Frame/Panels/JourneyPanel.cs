using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// JOURNEY - the record of what was done and what was learned (the user, 7 Oct 2026, late: "a journey button, this
    /// will be a record of all interactions and knowledge gained, conversations are compact into key information
    /// learned and all linked together to the wiki").
    ///
    /// Two leaves. WHAT YOU HAVE LEARNED: the markets a price is known of (the ledger), the news heard with who brought
    /// it (Known), the places heard of, and the people spoken with. THE DAYS: the log of the play, the newest day first,
    /// a day a heading (the 200 lines that are kept). Every name in it that has a page of the Book becomes a link when
    /// the frame's linking is in.
    /// </summary>
    public sealed class JourneyPanel : FramePanel
    {
        public override string Id => "journey";
        public override string Title => "Journey";
        public override int Order => 47;

        const int MostNews = 12, MostLines = 80, MostLandings = 30, MostFinds = 40;
        /// <summary>Which of its three leaves is open: what is learned, the days as they went, the landings.</summary>
        int leaf;

        protected override int Stamp()
        {
            int h = FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameLog.Version);
            h = FrameUi.Mix(h, Trade.Started ? Trade.Ledger.Count : 0);
            h = FrameUi.Mix(h, Landings.Version);
            h = FrameUi.Mix(h, Discoveries.Version);
            return FrameUi.Mix(FrameUi.Mix(h, Self.Version), leaf);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var learned = FrameUi.Btn("Learned", () => { leaf = 0; Redraw(); });
            var byDay = FrameUi.Btn("The days", () => { leaf = 1; Redraw(); });
            var landings = FrameUi.Btn("Landings", () => { leaf = 2; Redraw(); });
            learned.EnableInClassList("on", leaf == 0);
            byDay.EnableInClassList("on", leaf == 1);
            landings.EnableInClassList("on", leaf == 2);
            body.Add(FrameUi.Row(learned, byDay, landings));
            if (leaf == 1)
                Days(body);
            else if (leaf == 2)
                Landed(body);
            else
                Learned(body, game != null ? game.Day : 0.0);
        }

        /// <summary>The landings, the last first: each passage with what the purse held when she came and when she left
        /// (the slips the log said as they happened: Landings, FrameGame.LandingWords).</summary>
        static void Landed(VisualElement body)
        {
            var all = Landings.All;
            body.Add(FrameUi.H("Where she has landed"));
            if (all.Count == 0)
            {
                body.Add(FrameUi.P("Nowhere yet. Each landing is written here: the passage, the purse, and what the cargo would fetch."));
                return;
            }
            int shown = 0;
            for (int i = all.Count - 1; i >= 0 && shown < MostLandings; i--, shown++)
            {
                var l = all[i];
                string to = GameData.Port(l.to)?.name ?? l.to, from = GameData.Port(l.from)?.name ?? l.from;
                string name = l.from.Length == 0 ? to : l.from == l.to ? $"Out from {to} and back" : $"{from} to {to}";
                string small = (l.date.Length > 0 ? l.date + ", " : "") + (l.from.Length > 0 ? FrameUi.Days(l.Took) + ". " : "") +
                               (l.away >= 0.0 ? $"Came with {l.silver:0} d, left with {l.away:0} d." : FrameGame.LandingPurse(l));
                // (a stay that is over shows what it came to in the purse)
                double came = l.away - l.silver;
                var sum = l.away >= 0.0 ? FrameUi.Num((came >= 0.0 ? "+" : "-") + $"{System.Math.Abs(came):0} d") : FrameUi.Tag("here now");
                body.Add(FrameUi.Ln(name, small, sum));
            }
            if (all.Count > shown)
                body.Add(FrameUi.P($"And {all.Count - shown} before that."));
        }

        static void Learned(VisualElement body, double today)
        {
            body.Add(FrameUi.H("What you have learned"));
            // the markets
            var notes = Trade.Started ? Trade.Ledger.Notes.OrderByDescending(n => n.day).ToList() : new List<PriceNote>();
            if (notes.Count > 0)
            {
                var names = notes.Select(n => GameData.Port(n.port)?.name).Where(n => n != null).ToList();
                body.Add(FrameUi.Ln(notes.Count == 1 ? "One market's prices" : $"{notes.Count} markets' prices", string.Join(", ", names.Take(10)) + (names.Count > 10 ? $" and {names.Count - 10} more" : "") + "."));
            }
            // the people
            var met = new List<string>();
            foreach (var kv in Self.MetAll)
            {
                var p = PlaceBook.Current.Person(kv.Key);
                if (p != null)
                    met.Add((p.name.Length > 0 ? p.name + ", the " + p.Role : "The " + p.Role) + (GameData.Port(p.port) != null ? " at " + GameData.Port(p.port).name : ""));
            }
            if (met.Count > 0)
                body.Add(FrameUi.Ln(met.Count == 1 ? "One person spoken with" : $"{met.Count} people spoken with", string.Join("; ", met.Take(8)) + (met.Count > 8 ? $"; and {met.Count - 8} more" : "") + "."));
            // the places heard of
            var heard = GameData.Ports.Where(p => Known.IsHeardOf(p)).Select(p => p.name).ToList();
            if (heard.Count > 0)
                body.Add(FrameUi.Ln(heard.Count == 1 ? "One place heard of" : $"{heard.Count} places heard of", string.Join(", ", heard.Take(12)) + (heard.Count > 12 ? $" and {heard.Count - 12} more" : "") + "."));
            // the waters and landmarks found (Discoveries: the newest first; what she knew on the first morning last)
            var game = KapmanGame.Current;
            int toFind = Discoveries.Total(game != null ? game.Year : 793);
            var found = Discoveries.FoundIds;
            if (toFind > 0)
            {
                body.Add(FrameUi.H(found.Count == 0 ? "Waters and landmarks" : $"Waters and landmarks: {found.Count} of {toFind}"));
                if (found.Count == 0)
                    body.Add(FrameUi.P("None yet. They are found by sailing: a water when she is on it, a landmark when it comes in sight."));
                int listed = 0;
                for (int i = found.Count - 1; i >= 0 && listed < MostFinds; i--)
                {
                    var d = Discoveries.Book.Get(found[i]);
                    if (d == null)
                        continue;
                    listed++;
                    body.Add(FrameUi.Ln(d.name, d.note.Length > 0 ? d.note : null, FrameUi.Tag(d.kindName)));
                }
                if (found.Count > listed)
                    body.Add(FrameUi.P($"And {found.Count - listed} more."));
            }
            // the news
            var news = Known.News();
            if (news.Count > 0)
            {
                body.Add(FrameUi.H(news.Count == 1 ? "One piece of news" : $"{news.Count} pieces of news"));
                foreach (var f in news.OrderByDescending(f => f.learnedDay).Take(MostNews))
                    body.Add(FrameUi.Ln(f.text, string.IsNullOrEmpty(f.from) ? null : Cap(f.from) + ".", FrameUi.Tag(FrameUi.Ago(today - f.learnedDay))));
                if (news.Count > MostNews)
                    body.Add(FrameUi.P($"And {news.Count - MostNews} older."));
            }
            if (notes.Count == 0 && met.Count == 0 && heard.Count == 0 && news.Count == 0 && toFind == 0)
                body.Add(FrameUi.P("Nothing yet. What you see, hear and are told comes in here."));
        }

        static void Days(VisualElement body)
        {
            var lines = FrameLog.Lines;
            if (lines.Count == 0)
            {
                body.Add(FrameUi.P("Nothing has happened yet."));
                return;
            }
            string date = null;
            int shown = 0;
            for (int i = lines.Count - 1; i >= 0 && shown < MostLines; i--, shown++)
            {
                var l = lines[i];
                if (l.date != date)
                {
                    date = l.date;
                    body.Add(FrameUi.H(string.IsNullOrEmpty(date) ? "Before the first day" : date));
                }
                body.Add(FrameUi.Ln(l.Whole, string.IsNullOrEmpty(l.part) ? null : Cap(l.part)));   // (the whole words: the log shows the short)
            }
            if (lines.Count > shown)
                body.Add(FrameUi.P($"And {lines.Count - shown} lines before that."));
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
