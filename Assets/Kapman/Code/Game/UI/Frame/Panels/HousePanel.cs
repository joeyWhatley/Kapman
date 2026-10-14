using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// HOUSE - where the house stands: what is owed and what is in hand, when the debt falls due, the boat, the home
    /// port and the standing there, and the house's Name as pips. Plots, stores, other boats and an heir are not in the
    /// game yet: the panel says so.
    /// </summary>
    public sealed class HousePanel : FramePanel
    {
        public override string Id => "house";
        public override string Title => "House";
        public override int Order => 92;

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, Self.Version);
            h = FrameUi.Mix(h, (int)FrameGame.Silver);
            h = FrameUi.Mix(h, Trade.Started ? (int)Trade.Debt : 0);
            h = FrameUi.Mix(h, game != null && Trade.Started ? (int)Trade.DebtDaysLeft(game.Day) : 0);
            var home = GameData.Port(ChartWorld.Current != null ? ChartWorld.Current.homePort : null);
            return FrameUi.Mix(h, home != null ? (int)Reputation.At(home) : 0);
        }

        protected override void Fill(VisualElement body)
        {
            // (the sections of Game/UI/Panels/house.uxml)
            var silver = Section("silver");
            if (!Trade.Started)
            {
                silver.Add(FrameUi.P("There is no house to reckon yet."));
                return;
            }
            var game = KapmanGame.Current;
            silver.Add(FrameUi.H(Trade.Debt > 0.01 ? $"Owed {Trade.Debt:0} d. In hand {FrameGame.Silver:0} d." : $"Nothing owed. In hand {FrameGame.Silver:0} d."));
            if (Trade.Debt > 0.01 && game != null)
            {
                double days = Trade.DebtDaysLeft(game.Day);
                string day = FrameGame.DebtDay(true);
                silver.Add(FrameUi.Text(days > 0.0 ? $"The debt falls due at {day}: {days:0} {(days == 1.0 ? "day" : "days")} from now."
                                        : days == 0.0 ? $"The debt falls due today: {day}."
                                        : $"The debt fell due at {day}: {-days:0} {(days == -1.0 ? "day" : "days")} ago."));
                string sunday = FrameGame.DebtDayWarning();
                if (sunday.Length > 0)
                    silver.Add(FrameUi.Text(sunday));
                double short_ = Trade.Debt - FrameGame.Silver;
                silver.Add(FrameUi.P(short_ > 0.5 ? $"Short by {short_:0} d today." : "The silver in hand would cover it today."));
            }
            var ship = game != null ? game.ship : null;
            Section("boats").Add(FrameUi.Ln("Boats", ship != null && ship.shipClass != null ? "The " + ship.shipClass.shipName : null, FrameUi.Num("1")));
            var home = GameData.Port(ChartWorld.Current != null ? ChartWorld.Current.homePort : null);
            if (home != null)
                // ("here" only where she lies there)
                Section("home").Add(FrameUi.Ln("Home: " + home.name, FrameGame.PortHere != null && FrameGame.PortHere.id == home.id
                                                                         ? Reputation.Line(home) : Reputation.Line(home).Replace("Your standing here", "Your standing there")));
            var name = SelfBook.Current.Stat("name");
            Section("your-name").Add(FrameUi.Ln(name != null ? name.name : "Name", name != null ? name.what : null,
                                           name != null && name.Level(Self.StatNow("name")).Length > 0 ? FrameUi.Tag(name.Level(Self.StatNow("name"))) : null,
                                           FrameUi.Pips(Self.StatNow("name"))));
        }
    }
}
