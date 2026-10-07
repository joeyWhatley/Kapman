using System;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>Why the clock drops back by itself (ruling 26 of 7 October 2026). Each has a switch in the settings.</summary>
    public enum ClockReason
    {
        /// <summary>A pressing matter waits for an answer: raiders, the king's men, aground, a pressing card.</summary>
        Pressing,
        /// <summary>A band of raiders is made out by the lookout.</summary>
        Raiders,
        /// <summary>Any other sail is made out (off by default: on the Rhine that is every few minutes).</summary>
        Sail,
        /// <summary>She is within <see cref="ClockDrops.NearPortM"/> of the berth she is bound for.</summary>
        NearPort,
        /// <summary>She is made fast at a port.</summary>
        Moored,
        /// <summary>A wait is over: first light, a fair wind, the tide has made.</summary>
        WaitOver,
        /// <summary>Bread for under two days.</summary>
        BreadLow,
        /// <summary>No bread aboard.</summary>
        BreadGone,
        /// <summary>The debt falls due within seven days.</summary>
        DebtNear,
        /// <summary>The debt falls due within a day.</summary>
        DebtDay,
        /// <summary>The player opens a talk with someone, or the chest: reading takes the time it takes.</summary>
        Talk,
    }

    /// <summary>The switches for the clock's own drops (kept on this machine, not in the saved game).</summary>
    public static class ClockSettings
    {
        public const string Pref = "kapman.clock.drop.";
        static bool[] on;

        /// <summary>On from the start, all but <see cref="ClockReason.Sail"/>.</summary>
        public static bool Default(ClockReason why) => why != ClockReason.Sail;

        public static bool On(ClockReason why)
        {
            Load();
            return on[(int)why];
        }

        public static void Set(ClockReason why, bool value)
        {
            Load();
            on[(int)why] = value;
            if (!asNew)
                PlayerPrefs.SetInt(Pref + why, value ? 1 : 0);
        }

        static bool asNew;

        /// <summary>A test's: the switches as a new player has them, neither read from this machine nor written to it
        /// (the player's own are left alone). Set back to false when the test is over.</summary>
        public static bool AsNew
        {
            get => asNew;
            set
            {
                asNew = value;
                on = null;
            }
        }

        static void Load()
        {
            if (on != null)
                return;
            var all = (ClockReason[])Enum.GetValues(typeof(ClockReason));
            on = new bool[all.Length];
            foreach (var why in all)
                on[(int)why] = asNew ? Default(why) : PlayerPrefs.GetInt(Pref + why, Default(why) ? 1 : 0) != 0;
        }

        /// <summary>The level each reason drops the clock to (0 is speed 1). What is happening now drops it to speed 1;
        /// what is coming drops it to speed 2.</summary>
        public static int Level(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Sail:
                case ClockReason.NearPort:
                case ClockReason.BreadLow:
                case ClockReason.DebtNear:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>The few words the top bar says after the drop (placeholder wording).</summary>
        public static string Says(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Pressing: return "a pressing matter";
                case ClockReason.Raiders: return "raiders made out";
                case ClockReason.Sail: return "a sail close";
                case ClockReason.NearPort: return "nearing port";
                case ClockReason.Moored: return "moored";
                case ClockReason.WaitOver: return "the wait is over";
                case ClockReason.BreadLow: return "bread for two days";
                case ClockReason.BreadGone: return "no bread";
                case ClockReason.DebtNear: return "the debt, in a week";
                case ClockReason.DebtDay: return "the debt's day";
                case ClockReason.Talk: return "talking";
                default: return "";
            }
        }

        /// <summary>The line beside the switch in the settings (placeholder wording).</summary>
        public static string Label(ClockReason why)
        {
            switch (why)
            {
                case ClockReason.Pressing: return "A pressing matter: slow to speed 1, and no faster than speed 2 until it is answered";
                case ClockReason.Raiders: return "Raiders made out: slow to speed 1";
                case ClockReason.Sail: return "Any sail made out: slow to speed 2";
                case ClockReason.NearPort: return "Nearing the port she is bound for: slow to speed 2";
                case ClockReason.Moored: return "Made fast in port: slow to speed 1";
                case ClockReason.WaitOver: return "First light, a fair wind, the tide: slow to speed 1 when the wait is over";
                case ClockReason.BreadLow: return "Bread for under two days: slow to speed 2";
                case ClockReason.BreadGone: return "No bread aboard: slow to speed 1";
                case ClockReason.DebtNear: return "A week before the debt's day: slow to speed 2";
                case ClockReason.DebtDay: return "The debt's day: slow to speed 1";
                case ClockReason.Talk: return "Talking with someone, or reading in the chest: slow to speed 1";
                default: return why.ToString();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Forget() => on = null;
    }

    /// <summary>
    /// THE CLOCK DROPS BACK BY ITSELF (ruling 26): each step of the clock (KapmanGame.StepMinute, last of all) this looks
    /// at what has BECOME true in that minute and drops the speed for it, once: it never holds the clock (a card does
    /// that: PanelClock) and never raises it. Each reason is told once and armed again when it is over, so the player who
    /// goes back to speed 5 with bread for a day is not slowed again every minute. Because it is asked in the step, in
    /// game time, the drop comes at the same game minute whatever speed the clock was running at.
    /// </summary>
    public static class ClockDrops
    {
        /// <summary>Real metres from the berth she is bound for at which she is "nearing port" (40 game minutes at 4 knots).</summary>
        public const double NearPortM = 5000.0;
        public const double BreadLowDays = 2.0, BreadGoneDays = 0.25, BreadArmDays = 3.0;
        public const double DebtNearDays = 7.0, DebtDayDays = 1.0;
        /// <summary>The fastest level while a pressing matter is unanswered (1: speed 2, where an hour is 15 seconds).</summary>
        public const int PressingCap = 1;

        /// <summary>A pressing card is open and unanswered (the card clock sets it: fixes/card_clock.md; null: none).</summary>
        public static Func<bool> PressingCard;

        static bool pressing, moored, nearPort, breadLow, breadGone, debtNear, debtDay;

        /// <summary>The last reason the clock dropped for (null: none since the game began).</summary>
        public static ClockReason? Last { get; private set; }
        /// <summary>A drop happened (the reason): for the log and the sound.</summary>
        public static event Action<ClockReason> Dropped;

        /// <summary>No drop at all, whatever the switches say (a test in a hurry; never kept, never the player's).</summary>
        public static bool Quiet;

        /// <summary>Drops the clock for a reason, if its switch is on and the clock runs faster than the reason's speed.</summary>
        public static bool Raise(ClockReason why)
        {
            if (Quiet || !ClockSettings.On(why) || !GameSpeed.DropTo(ClockSettings.Level(why), ClockSettings.Says(why)))
                return false;
            Last = why;
            Dropped?.Invoke(why);
            return true;
        }

        /// <summary>Does a pressing matter wait for an answer now?</summary>
        public static bool Pressing()
        {
            switch (SeaHolds.Now)
            {
                case SeaHold.GuardBoat:
                case SeaHold.PortSearch:
                case SeaHold.RaidersComing:
                case SeaHold.RaidersChasing:
                case SeaHold.RaidersAlongside:
                    return true;
            }
            if (Aground.Stage != AgroundStage.None)
                return true;
            return PressingCard != null && PressingCard();
        }

        /// <summary>After each step of the clock: what became true in this minute.</summary>
        public static void Step(long minute)
        {
            var game = KapmanGame.Current;
            if (game == null)
                return;
            Look(game, out bool press, out bool lying, out bool near, out double bread, out double debtLeft);

            // a pressing matter: the clock drops, and may not run fast while it waits (its hours are short)
            GameSpeed.Cap = press && !Quiet && ClockSettings.On(ClockReason.Pressing) ? PressingCap : GameSpeed.Count - 1;
            if (press && !pressing)
                Raise(ClockReason.Pressing);
            pressing = press;

            if (lying && !moored)
                Raise(ClockReason.Moored);
            moored = lying;
            if (near && !nearPort)
                Raise(ClockReason.NearPort);
            nearPort = near;

            if (bread < BreadGoneDays)
            {
                if (!breadGone)
                    Raise(ClockReason.BreadGone);
                breadGone = breadLow = true;
            }
            else if (bread < BreadLowDays)
            {
                if (!breadLow)
                    Raise(ClockReason.BreadLow);
                breadLow = true;
                breadGone = false;
            }
            else if (bread >= BreadArmDays)
                breadLow = breadGone = false;

            if (debtLeft <= DebtDayDays)
            {
                if (!debtDay)
                    Raise(ClockReason.DebtDay);
                debtDay = debtNear = true;
            }
            else if (debtLeft <= DebtNearDays)
            {
                if (!debtNear)
                    Raise(ClockReason.DebtNear);
                debtNear = true;
            }
            else
                debtNear = debtDay = false;
        }

        /// <summary>A game was loaded or begun: every reason stands as it is now, and none is told for it.</summary>
        public static void Rearm()
        {
            var game = KapmanGame.Current;
            pressing = moored = nearPort = breadLow = breadGone = debtNear = debtDay = false;
            GameSpeed.Cap = GameSpeed.Count - 1;
            if (game == null)
                return;
            Look(game, out pressing, out moored, out nearPort, out double bread, out double debtLeft);
            breadGone = bread < BreadGoneDays;
            breadLow = bread < BreadLowDays;
            debtDay = debtLeft <= DebtDayDays;
            debtNear = debtLeft <= DebtNearDays;
        }

        static void Look(KapmanGame game, out bool press, out bool lying, out bool near, out double bread, out double debtLeft)
        {
            press = Pressing();
            lying = game.ship != null && game.ship.anchored && Port.LyingId() != null;
            near = false;
            if (!lying && game.ship != null && Course.IsSet && Course.Points.Count > 0)
                near = WorldXZ.Distance(ChartWorld.ShipWorld(), Course.Points[Course.Points.Count - 1]) < NearPortM;
            bread = Voyage.Started ? Voyage.FoodDays : double.PositiveInfinity;
            // (whole days to the day itself, as the top bar counts them)
            debtLeft = Trade.Started && Trade.Debt > 0.01 ? Trade.DebtDaysLeft(game.Day) : double.PositiveInfinity;
        }

        static void MadeOut(SimShip s)
        {
            if (s != null && GameSpeed.Stepped)
                Raise(s.raider ? ClockReason.Raiders : ClockReason.Sail);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            pressing = moored = nearPort = breadLow = breadGone = debtNear = debtDay = false;
            PressingCard = null;
            Last = null;
            Dropped = null;
            SeaWatch.MadeOut -= MadeOut;
            SeaWatch.MadeOut += MadeOut;
        }
    }
}
