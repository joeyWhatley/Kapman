using Kapman.Levels;
using Kapman.World;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// FAITH AND DAYS - your faith, your Troth as pips, and the days ahead that your faith keeps (FaithCalendar, from
    /// story/calendar_793.json): each with when it falls and what may not be done on it, as the data words it. Nothing
    /// enforces a day yet and Troth does not move by them: the panel says so.
    /// </summary>
    public sealed class FaithPanel : FramePanel
    {
        public override string Id => "faith";
        public override string Title => "Faith and days";
        public override int Order => 84;

        protected override int Stamp()
        {
            var clock = LevelClock.Current;
            int h = FrameUi.Mix(17, Self.Version);
            h = FrameUi.Mix(h, ContentFiles.Version);
            return clock != null ? FrameUi.Mix(FrameUi.Mix(h, clock.Date.year), clock.Date.dayOfYear) : h;
        }

        protected override void Fill(VisualElement body)
        {
            var book = SelfBook.Current;
            var faith = book.Faith(Self.Faith);
            var troth = book.Stat("troth");
            body.Add(FrameUi.H(faith != null ? faith.name : Self.Faith));
            body.Add(FrameUi.Ln(troth != null ? troth.name : "Troth", troth != null ? troth.what : null,
                                troth != null && troth.Level(Self.StatNow("troth")).Length > 0 ? FrameUi.Tag(troth.Level(Self.StatNow("troth"))) : null,
                                FrameUi.Pips(Self.StatNow("troth"))));
            if (faith != null && faith.swears.Length > 0)
                body.Add(FrameUi.P("You swear: " + faith.swears));
            var cal = FaithCalendar.Current;
            var clock = LevelClock.Current;
            if (!cal.FromFile || clock == null)
            {
                body.Add(FrameUi.P("The year's days are not written yet."));
                return;
            }
            GameDate today = clock.Date;
            body.Add(FrameUi.H("The days ahead"));
            var ahead = cal.Ahead(Self.Faith, faith != null ? faith.countsAs : "", today.year, today.month, today.day);
            foreach (var d in ahead)
            {
                string when = d.daysAhead == 0 ? "today" : d.daysAhead == 1 ? "tomorrow" : $"in {d.daysAhead} days";
                string date = d.weekly ? FaithCalendar.WeekdayWords[FaithCalendar.Weekday(d.year, d.month, d.day)] : $"{d.day} {Calendar.MonthNames[d.month - 1]}";
                string small = date + (d.kind.Length > 0 ? ", a " + d.kind : "") + (d.mayNot.Length > 0 ? ". Not to be done: " + d.MayNot + "." : ".");
                body.Add(FrameUi.Ln(d.name, small, FrameUi.Num(when)));
            }
            if (ahead.Count == 0)
                body.Add(FrameUi.P("No day of your faith falls in the next two months."));
            foreach (var (name, when) in cal.Undated(Self.Faith, faith != null ? faith.countsAs : ""))
                body.Add(FrameUi.P(name + ": " + when + "."));
            body.Add(FrameUi.P("Keeping these days is not counted yet: nothing stops a sale on a feast, and Troth does not move by them."));
        }
    }
}
