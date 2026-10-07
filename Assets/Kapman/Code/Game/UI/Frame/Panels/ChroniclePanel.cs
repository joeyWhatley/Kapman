using System;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.World;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// CHRONICLE - the news she has heard, and how old it is. The chronicle's own entries as far as word of them has
    /// reached the merchant (Known.EntryFact: when it was heard and from whom), with what had happened before the game
    /// began, which every merchant knows; then the other talk heard (Known.News: a raid told by a passing ship, a fair).
    /// Newest first. News is as old as the ship that brought it: each line says both when it happened and when it came.
    /// </summary>
    public sealed class ChroniclePanel : FramePanel
    {
        public override string Id => "chronicle";
        public override string Title => "Chronicle";
        public override int Order => 60;

        const int Most = 40;

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            return FrameUi.Mix(FrameUi.Mix(Chronicle.Happened, Known.Version), game != null ? (int)Math.Floor(game.Day) : 0);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;
            if (game == null || clock == null)
            {
                body.Add(FrameUi.P("Nothing is known yet."));
                return;
            }
            var today = clock.Date;
            var start = game.StartDate;
            int startKey = start.year * 400 + start.month * 32 + start.day;
            body.Add(FrameUi.H("What has happened"));
            int shown = 0;
            var told = new HashSet<string>();
            foreach (var e in Chronicle.SoFar())
            {
                var heard = Known.EntryFact(e.id);
                bool before = e.Key <= startKey;
                if (heard == null && !before)
                    continue;
                told.Add(e.id);
                if (shown >= Most)
                    continue;
                shown++;
                string small = e.When + (string.IsNullOrEmpty(e.place) ? "" : ", " + e.place);
                string old = Old(e, today);
                if (old.Length > 0)
                    small += ". " + char.ToUpperInvariant(old[0]) + old.Substring(1);
                small += heard != null ? ". Heard " + FrameUi.Ago(game.Day - heard.learnedDay) + (string.IsNullOrEmpty(heard.from) ? "" : ", " + heard.from)
                                       : ". Known to every merchant";
                var line = FrameUi.Ln(e.title, small + ".");
                line.tooltip = (e.sources.Length > 0 ? "Sources: " + string.Join("; ", e.sources) : "") +
                               (string.IsNullOrEmpty(e.note) ? "" : "\n" + e.note) + (e.certainty == "uncertain" ? "\n(uncertain)" : "");
                body.Add(line);
            }
            // the other talk heard: news no entry of the chronicle stands behind
            int talk = 0;
            foreach (var f in Known.News())
            {
                if (!string.IsNullOrEmpty(f.chronicleId) && told.Contains(f.chronicleId))
                    continue;
                if (string.IsNullOrEmpty(f.text) || talk >= 12)
                    continue;
                if (talk++ == 0)
                    body.Add(FrameUi.H("Talk heard"));
                double age = game.Day - f.trueDay;
                body.Add(FrameUi.Ln(f.text, (age < 1.0 ? "From today" : $"{age:0} days old") + ". Heard " + FrameUi.Ago(game.Day - f.learnedDay) +
                                            (string.IsNullOrEmpty(f.from) ? "" : ", " + f.from) + "."));
            }
            if (shown == 0 && talk == 0)
                body.Add(FrameUi.P("No word of anything has reached you yet."));
            body.Add(FrameUi.P("News is as old as the ship that brought it."));
        }

        /// <summary>How long ago an entry happened, where its day or month is known: "13 days old", "about 2 months old".</summary>
        static string Old(ChronicleEntry e, GameDate today)
        {
            if (e.year <= 0 || e.month <= 0)
                return "";
            int doy = Math.Max(1, e.day);
            for (int m = 1; m < e.month; m++)
                doy += Calendar.DaysIn(e.year, m);
            double days = (today.year - e.year) * 365.25 + (today.dayOfYear - doy);
            if (days < 0.0)
                return "";
            if (e.precision != "day" || days > 120.0)
            {
                int months = (int)Math.Round(days / 30.4);
                return months <= 0 ? "this month" : months == 1 ? "about a month old" : months < 24 ? $"about {months} months old" : $"about {months / 12} years old";
            }
            return days < 1.0 ? "today" : days < 2.0 ? "a day old" : $"{days:0} days old";
        }
    }
}
