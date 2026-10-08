using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// ERRANDS - what you have taken on: the debt and its day (and paying toward it at a market); the orders offered to
    /// you and not yet answered (take or refuse); the orders you have taken, each with its day, how much of it is
    /// brought and how much is aboard (handed over here, in parts if need be, when she lies where it is wanted); the
    /// orders posted where she lies; and the passengers (the game carries none yet, and says so).
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
                h = FrameUi.Mix(h, Trade.Working ? 1 : 0);
                h = FrameUi.Mix(h, FrameGame.HoldStamp());
                h = FrameUi.Mix(h, (int)Trade.Hold.capacity);
            }
            foreach (var c in Commissions.All)
            {
                h = FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(h, c.id), c.mine), game != null ? (int)(c.deadline - game.Day) : 0);
                h = FrameUi.Mix(FrameUi.Mix(h, c.done), c.offered);
            }
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
                // WHAT FOLLOWS (the critic's play of 8 Oct, B5: "after the opening, nothing on screen says what happens
                // on the day"): the lender's own word before the day, and after it what he has made of her
                if (Trade.Working)
                    body.Add(FrameUi.P($"She works for {FrameGame.CreditorName()} until the stick is clean: {TradeRules.Current.workShare * 100f:0} pennies in every hundred she sells go to it before they reach you."));
                else if (ContentFlags.Ready && ContentFlags.Has(Trade.LenderFlag))
                    body.Add(FrameUi.P(days >= 0.0 ? "The day after, he comes with two men and a rope."
                                                   : $"The day is past. {Trade.LenderName} is waiting at Dorestat with two men and a rope."));
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

            // ---- orders offered to you by name, not yet answered (Design/fixes/orders.md)
            var offers = Commissions.OfferedToYou;
            if (offers.Count > 0)
            {
                body.Add(FrameUi.H("Orders offered to you"));
                foreach (var c in offers)
                {
                    var order = c;
                    double stands = c.posted + Commissions.OfferDays - game.Day;
                    string why = Commissions.WhyNot(c);
                    string small = (why.Length > 0 ? why : Commissions.LoadsLine(c)) + $" The offer stands {System.Math.Max(0.0, System.Math.Ceiling(stands)):0} days more.";
                    var take = FrameUi.Btn("Take", () => Say(Commissions.Take(order)), why.Length > 0 ? why : null);
                    take.SetEnabled(why.Length == 0);
                    // (the buttons stand under the line, not beside it: the window is narrow)
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small.Trim()));
                    body.Add(FrameUi.Row(take, FrameUi.Btn("Refuse", () => Say(Commissions.Refuse(order)))));
                }
            }

            // ---- orders taken: each with its day, what is brought, what is aboard; handed over here, part by part
            body.Add(FrameUi.H("Orders taken"));
            var mine = Commissions.Mine;
            if (mine.Count == 0)
                body.Add(FrameUi.P("None. A port posts its orders where she lies; some people offer you one themselves."));
            foreach (var c in mine)
            {
                var order = c;
                double left = c.deadline - game.Day;
                int aboard = Trade.Hold.Count(c.good);
                int now = System.Math.Min(aboard, c.Left);
                bool isHere = here != null && c.port == here.id;
                string small = (left >= 0.0 ? $"{System.Math.Ceiling(left):0} days left. " : "Its day has passed. ") + $"Brought: {c.done} of {c.units}. Aboard: {aboard}.";
                string loads = Commissions.LoadsLine(c);
                if (loads.Length > 0)
                    small += " " + loads;
                if (isHere && now > 0)
                {
                    var market = Markets.Of(here, game.Year, game.Day);
                    double toll = Chest.TollFreeAt(here, game.Year) ? 0.0 : market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day);
                    small += toll > 0.0 ? $" This is the place: the toll here is {toll * 100.0:0} in the hundred." : " This is the place: no toll here.";
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small));
                    body.Add(FrameUi.Row(FrameUi.Go(now >= c.Left ? $"Hand over {now}" : $"Hand over {now} now", () => Say(Commissions.Deliver(order, here)))));
                }
                else
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small + (isHere ? " This is the place, but none of it is aboard." : "")));
            }

            // ---- orders posted here, open to every merchant
            if (here != null)
            {
                var posted = Commissions.At(here);
                body.Add(FrameUi.H("Orders posted at " + here.name));
                if (posted.Count == 0)
                    body.Add(FrameUi.P("None posted here now."));
                foreach (var c in posted)
                {
                    var order = c;
                    string why = Commissions.WhyNot(c);
                    string small = $"{System.Math.Ceiling(c.deadline - game.Day):0} days to bring it. " + (why.Length > 0 ? why : Commissions.LoadsLine(c));
                    var take = FrameUi.Btn("Take", () => Say(Commissions.Take(order)), why.Length > 0 ? why : null);
                    take.SetEnabled(why.Length == 0);
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small.Trim()));
                    body.Add(FrameUi.Row(take));
                }
                if (posted.Count > 0)
                    body.Add(FrameUi.P("Left untaken, another merchant may fill it."));
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
