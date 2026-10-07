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

        const int MostNews = 12, MostLines = 80;
        bool days;

        protected override int Stamp()
        {
            int h = FrameUi.Mix(FrameUi.Mix(17, Known.Version), FrameLog.Version);
            h = FrameUi.Mix(h, Trade.Started ? Trade.Ledger.Count : 0);
            return FrameUi.Mix(FrameUi.Mix(h, Self.Version), days);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var learned = FrameUi.Btn("Learned", () => { days = false; Redraw(); });
            var byDay = FrameUi.Btn("The days", () => { days = true; Redraw(); });
            learned.EnableInClassList("on", !days);
            byDay.EnableInClassList("on", days);
            body.Add(FrameUi.Row(learned, byDay));
            if (days)
                Days(body);
            else
                Learned(body, game != null ? game.Day : 0.0);
        }

        static void Learned(VisualElement body, double today)
        {
            body.Add(FrameUi.H("What you have learned"));
            // the markets
            var notes = Trade.Started ? Trade.Ledger.Notes.OrderByDescending(n => n.day).ToList() : new List<PriceNote>();
            if (notes.Count > 0)
            {
                var names = notes.Select(n => GameData.Port(n.port)?.name).Where(n => n != null).ToList();
                body.Add(FrameUi.Ln(notes.Count == 1 ? "One market's prices" : $"{notes.Count} markets' prices", string.Join(", ", names.Take(10)) + (names.Count > 10 ? $" and {names.Count - 10} more" : "") + ". The Wares window has each ware."));
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
            if (notes.Count == 0 && met.Count == 0 && heard.Count == 0 && news.Count == 0)
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
                body.Add(FrameUi.Ln(l.text, string.IsNullOrEmpty(l.part) ? null : Cap(l.part)));
            }
            if (lines.Count > shown)
                body.Add(FrameUi.P($"And {lines.Count - shown} lines before that."));
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
