using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TASKS - what you have taken on: the debt and its day (and paying toward it at a market), the orders you have taken
    /// with their days left (delivered here when you carry them), the orders posted where she lies, and the passengers
    /// (the game carries none yet, and says so).
    /// </summary>
    public sealed class TasksPanel : FramePanel
    {
        public override string Id => "tasks";
        public override string Title => "Errands";
        public override int Order => 40;

        string said = "";

        protected override void OnLoaded() => said = "";              // (what was last said here was of the game before)

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(FrameUi.Mix(17, here != null ? here.id : ""), said);
            h = FrameUi.Mix(FrameUi.Mix(h, FirstJobs.Version), Voyage.Men);
            if (Trade.Started)
            {
                h = FrameUi.Mix(FrameUi.Mix(h, (int)Trade.Debt), (int)FrameGame.Silver);
                h = FrameUi.Mix(h, game != null ? (int)Trade.DebtDaysLeft(game.Day) : 0);
                h = FrameUi.Mix(h, FrameGame.HoldStamp());
            }
            foreach (var c in Commissions.All)
                h = FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(h, c.id), c.mine), game != null ? (int)(c.deadline - game.Day) : 0);
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("Nothing taken on yet."));
                return;
            }
            var here = FrameGame.PortHere;
            if (said.Length > 0)
                body.Add(FrameUi.Text(said));

            // ---- the debt and its day
            body.Add(FrameUi.H("Owed"));
            if (Trade.Debt > 0.01)
            {
                double days = Trade.DebtDaysLeft(game.Day);
                string day = FrameGame.DebtDay(true);
                string when = days > 0.0 ? $"Due at {day}: {days:0} {(days == 1.0 ? "day" : "days")} from now."
                            : days == 0.0 ? $"Due today: {day}."
                            : $"It fell due at {day}: {-days:0} {(days == -1.0 ? "day" : "days")} ago.";
                double silver = FrameGame.Silver;
                string pace = silver >= Trade.Debt ? "You have the silver for it." : $"You are short of it by {Trade.Debt - silver:0} d.";
                body.Add(FrameUi.Ln("The debt", when + " " + pace, FrameUi.Num($"{Trade.Debt:0} d")));
                string sunday = FrameGame.DebtDayWarning();
                if (sunday.Length > 0)
                    body.Add(FrameUi.P(sunday));
                if (here != null)
                {
                    var market = Markets.Of(here, game.Year, game.Day);
                    var part = FrameUi.Btn("Pay 20 d", () => Pay(market, 20.0));
                    var all = FrameUi.Btn("Pay it all", () => Pay(market, Trade.Debt));
                    part.SetEnabled(silver >= System.Math.Min(20.0, Trade.Debt));
                    all.SetEnabled(silver >= Trade.Debt);
                    body.Add(FrameUi.Row(part, all));
                }
                else
                    body.Add(FrameUi.P("It is paid at a market, in port."));
            }
            else
                body.Add(FrameUi.P("You owe nothing."));

            // ---- orders taken
            body.Add(FrameUi.H("Orders taken"));
            var mine = Commissions.Mine;
            if (mine.Count == 0)
                body.Add(FrameUi.P("None. Orders are posted in the ports: see below when she lies in one."));
            bool canDeliver = false;
            foreach (var c in mine)
            {
                double left = c.deadline - game.Day;
                int aboard = Trade.Hold.Count(c.good);
                bool ready = here != null && c.port == here.id && aboard >= c.units;
                canDeliver |= ready;
                string small = (left >= 0.0 ? $"{left:0} days left. " : $"{-left:0} days late. ") + $"Aboard: {aboard} of {c.units}." + (ready ? " This is the place." : "");
                body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small));
            }
            if (canDeliver)
                body.Add(FrameUi.Go("Deliver what is ordered here", () =>
                {
                    Say(Commissions.Deliver(here));
                }));

            // ---- orders posted here
            if (here != null)
            {
                var posted = Commissions.At(here);
                body.Add(FrameUi.H("Orders posted at " + here.name));
                if (posted.Count == 0)
                    body.Add(FrameUi.P("None posted here now."));
                foreach (var c in posted)
                {
                    var order = c;
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), $"{c.deadline - game.Day:0} days to bring it.",
                                        FrameUi.Btn("Take", () => Say(Commissions.Take(order)))));
                }
            }

            // ---- passengers
            body.Add(FrameUi.H("Passengers"));
            body.Add(FrameUi.P("Nobody is carried as a passenger."));
        }

        void Pay(Market market, double deniers)
        {
            Say(Trade.PayDebt(market, deniers).text);
        }

        void Say(string text)
        {
            said = text ?? "";
            FrameLog.Add(said);
            Redraw();
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
