using Kapman.Levels;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE ECONOMY'S DEV KEYS (economy design 5, night two), in Play in the editor or a development build:
    ///   - B buys and N sells one cask of Rhenish wine at Dorestad (for now: the port the ship lies at comes later) for
    ///     the merchant's purse and hold, through Deal, as every ship deals; what came of it, the price after and the
    ///     market's size and state go to the console and the notices;
    ///   - Set Year (in the inspector, in Play, or before Play) moves the game on to that year's same day, so 834 and 863
    ///     are reachable without hundreds of presses of U: the clock's year moves (the game days don't, so nothing is
    ///     ticked through), the markets read their dates again, and the chronicle catches up at the next game hour (every
    ///     entry in between comes at once: its raids all strike then);
    ///   - once each game month (U moves a month), the console gets a line of the watched goods at the port: asks,
    ///     pays, stock, and the market's size and state (Parisius shows the grain year best: Dorestad makes no wheat,
    ///     and the famine of 793 holds its wheat dear until the harvest of 794).
    /// It also keeps the economy's calendar on the level clock's start (Seasons.SetStart). Put it on the Game object
    /// beside KapmanGame.
    /// </summary>
    public class EconomyDevKeys : MonoBehaviour
    {
        [Tooltip("The market B and N deal in (a port id of ports_793.json). Dorestad for now.")]
        [SerializeField] string portId = "dorestad";
        [Tooltip("The good B buys and N sells, a unit at a time.")]
        [SerializeField] string goodId = "rhenish_wine";
        [Tooltip("Set a later year (in Play, or before Play) and the game jumps to that year's same day: 834, 863... " +
                 "Forward only. 0: nothing.")]
        [SerializeField] int setYear;
        [Tooltip("Each game month, log these goods (ids joined by commas) at these ports (ids joined by commas). Empty: no log.")]
        [SerializeField] string watchGoods = "wheat,rhenish_wine", watchPorts = "dorestad,parisius";

        int appliedYear, loggedMonth;

        /// <summary>The B and N keys deal (off while the frame is up: N is its "new window").</summary>
        public static bool Keys = true;

        void Update()
        {
            var clock = Clock();
            if (clock != null)
                KeepCalendar(clock);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (setYear > 0 && setYear != appliedYear && clock != null && KapmanGame.Current != null)
            {
                appliedYear = setYear;
                JumpTo(clock, setYear);
            }
            if (clock != null && KapmanGame.Current != null)
            {
                int month = clock.Year * 12 + KapmanGame.Current.Month;
                if (month != loggedMonth)
                {
                    loggedMonth = month;
                    LogMonth();
                }
            }
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null || !Keys)
                return;
            if (kb.bKey.wasPressedThisFrame)
                DealOne(true);
            else if (kb.nKey.wasPressedThisFrame)
                DealOne(false);
#endif
        }

        static LevelClock Clock()
        {
            var game = KapmanGame.Current;
            return game != null && game.clock != null ? game.clock : LevelClock.Current;
        }

        /// <summary>The economy's calendar on the clock's start: game day 0 is the clock's start day of its year.</summary>
        static void KeepCalendar(LevelClock clock)
        {
            if (Seasons.StartYear == clock.year && Seasons.StartDay == clock.startDay)
                return;
            Seasons.SetStart(clock.year, clock.startDay);
            Markets.Recalendar();
        }

        static void JumpTo(LevelClock clock, int year)
        {
            int now = clock.Year;
            if (year <= now)
            {
                Debug.LogWarning($"Kapman dev: the year only goes forward (it is {now}).");
                return;
            }
            clock.year += year - now;
            KeepCalendar(clock);
            var game = KapmanGame.Current;
            if (game != null)
                Markets.CatchUpAll(game.Day);
            Say($"Dev: the year is now {clock.Year} ({clock.Text}). The chronicle catches up at the next game hour.");
        }

        void DealOne(bool buy)
        {
            var game = KapmanGame.Current;
            var port = GameData.Port(portId);
            if (game == null || !Trade.Started || port == null)
            {
                Debug.LogWarning("Kapman dev: no game, merchant or port " + portId + " to deal with.");
                return;
            }
            var m = Markets.Of(port, game.Year, game.Day);
            int month = game.Month;
            var r = buy ? Deal.Buy(m, Trade.Purse, Trade.Hold, goodId, 1, month, game.Day, "player")
                        : Deal.Sell(m, Trade.Purse, Trade.Hold, goodId, 1, month, game.Day, "player");
            string state = port.name + " " + m.Year + ": size " + m.Size +
                           (m.RuinedOn(game.Day) ? ", ruined" : m.Disrupted(game.Day) ? ", shut" : "");
            var g = m.Get(goodId);
            string after = g == null ? "" :
                $" Now asks {Deal.AsksFor(m, g, month, "player"):0.#} d, pays {Deal.PaysFor(m, g, month, "player"):0.#} d, stock {g.stock:0}; {Trade.Hold.Count(goodId)} aboard.";
            string text;
            if (!r.ok)
                text = $"Dev: no {(buy ? "buying" : "selling")} {goodId} at {port.name} ({r.why}).";
            else if (buy)
                text = $"Dev: bought 1 {goodId} at {port.name} for {r.deniers:0.#} d.";
            else
                text = $"Dev: sold 1 {goodId} at {port.name} for {r.deniers:0.#} d, less {r.toll:0.#} d toll.";
            Say(text + after + " (" + state + ")");
        }

        /// <summary>A line per watched port: its watched goods' asks, pays and stock, and the market's size and state.</summary>
        void LogMonth()
        {
            var game = KapmanGame.Current;
            if (game == null || string.IsNullOrEmpty(watchGoods) || string.IsNullOrEmpty(watchPorts))
                return;
            foreach (var pid in watchPorts.Split(','))
            {
                var port = GameData.Port(pid.Trim());
                if (port == null)
                    continue;
                var m = Markets.Of(port, game.Year, game.Day);
                var sb = new System.Text.StringBuilder("Kapman dev market: ");
                sb.Append(port.name).Append(", ").Append(Kapman.World.Calendar.MonthNames[m.Month - 1]).Append(' ').Append(m.Year)
                  .Append(" (size ").Append(m.Size).Append(m.RuinedOn(game.Day) ? ", ruined" : m.Disrupted(game.Day) ? ", shut" : "").Append("):");
                foreach (var gid in watchGoods.Split(','))
                {
                    var g = m.Get(gid.Trim());
                    if (g == null)
                        continue;
                    sb.Append("  ").Append(g.good.id).Append(' ').Append(m.Asks(g, m.Month).ToString("0.#")).Append('/')
                      .Append(m.Pays(g, m.Month).ToString("0.#")).Append(" d (stock ").Append(g.stock.ToString("0")).Append(')');
                }
                Debug.Log(sb.ToString());
            }
        }

        static void Say(string text)
        {
            Debug.Log("Kapman " + text);
            Voyage.Say(text);
        }
    }
}
