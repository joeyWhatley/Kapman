using System;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WHAT THE FRAME READS OF THE GAME, in one place and in plain words: whether she lies in a port with its menu up,
    /// casting off, the date as it is said, the wind as a sailor puts it, the crew's state as a word. The frame's parts
    /// and its panels ask here, so one meaning of "in port" holds everywhere.
    /// </summary>
    public static class FrameGame
    {
        /// <summary>The port she lies moored at with the port's own business open (its menu, or what an errand or a talk
        /// there led to), or null: at sea, lying to off a port she has cast off from, or no game yet.</summary>
        public static PortDef PortHere
        {
            get
            {
                // (the port's business is open only while the ports' graph has her made fast: that, and the port she is
                // at. Not "anchored": lying to off a port she has cast off from she is anchored too)
                var text = TextPanel.Current;
                var game = KapmanGame.Current;
                if (text == null || !text.Open || !text.Key.StartsWith("port.", StringComparison.Ordinal) || game == null || game.ship == null)
                    return null;
                var port = Port.Nearest(game.ShipPosition);
                return port != null && port.DistanceTo(game.ShipPosition) < port.arrivalRadius + 5f ? port.Def : null;
            }
        }

        /// <summary>The port's menu waits for a choice (not in the middle of an errand's or a talk's own answer).</summary>
        public static bool PortMenuOpen
        {
            get
            {
                var text = TextPanel.Current;
                return text != null && text.Open && text.Key == TextPanel.PortMenuKey && text.Choice < 0;
            }
        }

        /// <summary>Casts off as the port's menu does (the ports' graph lets her go). False when the menu is not waiting.</summary>
        public static bool CastOff() => !FirstJobs.Blocks(out _) && PortMenuOpen && TextPanel.Current.PickKey("port.leave");

        /// <summary>Who the debt is owed to, as it is said (Trade.CreditorSaid).</summary>
        public static string CreditorName() => Trade.CreditorSaid;

        /// <summary>"21 June 793".</summary>
        public static string DateWords(GameDate d) => $"{d.day} {d.MonthName} {d.year}";

        /// <summary>The date of a game day (days since the start), "29 September 793".</summary>
        public static string DateOfDay(double day)
        {
            var clock = LevelClock.Current;
            return clock == null ? $"day {day:0}" : DateWords(Calendar.At(clock.year, clock.startDay, day * 24.0));
        }

        /// <summary>The wind as it is said aboard: "calm", "a gentle breeze from the west", "a gale from the north-west".</summary>
        public static string WindWords(float speed, float from)
        {
            int force = Kapman.Weather.Beaufort.Force(speed);
            if (force == 0)
                return "calm";
            string name = Kapman.Weather.Beaufort.Name(force);
            return $"{name} from the {FrameUi.Airt(from)}";
        }

        /// <summary>The crew's health as a word.</summary>
        public static string HealthWord(double health) =>
            health >= 0.85 ? "sound" : health >= 0.6 ? "worn" : health >= 0.35 ? "ailing" : "sick";

        /// <summary>The crew's heart as a word.</summary>
        public static string HeartWord(double heart) =>
            heart >= 0.75 ? "in good heart" : heart >= 0.5 ? "steady" : heart >= 0.3 ? "low" : "close to leaving";

        /// <summary>The top bar's words for the debt: the day it falls due and the days left (ruling 14 of 7 Oct 2026:
        /// "the debt due on a day"). "Owed 1920 d by Michaelmas: 151 days left", "... : due today",
        /// "Owed 1920 d since Michaelmas: 3 days overdue"; "" with no debt. (The day is named as the time named it, 8 Oct
        /// 2026: its feast, or reckoned: DebtDayOfTheTime.)</summary>
        public static string DebtLine()
        {
            if (!Trade.Started || Trade.Debt <= 0.01)
                return "";
            var game = KapmanGame.Current;
            if (game == null)
                return $"Owed {Trade.Debt:0} d";
            return DebtLine(Trade.Debt, DebtDayOfTheTime(), Trade.DebtDaysLeft(game.Day));
        }

        /// <summary>The day the debt falls due as the time named it: its own name where it has one ("Michaelmas"), else
        /// reckoned by the Kalends, Nones and Ides (PeriodDate). The bar says this; the plain date is in its tip.</summary>
        public static string DebtDayOfTheTime()
        {
            if (Trade.DebtDueName.Length > 0)
                return Trade.DebtDueName;
            var clock = LevelClock.Current;
            return clock == null ? $"day {Trade.DebtDue:0}" : PeriodDate.Day(Calendar.At(clock.year, clock.startDay, Math.Floor(Trade.DebtDue) * 24.0));
        }

        /// <summary>The same from its parts (a sum, the day in words, whole days left: 0 on the day, under 0 after it).</summary>
        public static string DebtLine(double debt, string day, double left)
        {
            if (left > 0.0)
                return $"Owed {debt:0} d by {day}: {left:0} {(left == 1.0 ? "day" : "days")} left";
            if (left == 0.0)
                return $"Owed {debt:0} d by {day}: due today";
            return $"Owed {debt:0} d since {day}: {-left:0} {(left == -1.0 ? "day" : "days")} overdue";
        }

        /// <summary>The day the debt falls due, in words: "Michaelmas, 29 September 793" (with the year), "Michaelmas,
        /// 29 September" (without), or only the date where the day has no name.</summary>
        public static string DebtDay(bool withYear)
        {
            var clock = LevelClock.Current;
            string date;
            if (clock == null)
                date = $"day {Trade.DebtDue:0}";
            else
            {
                var d = Calendar.At(clock.year, clock.startDay, Math.Floor(Trade.DebtDue) * 24.0);
                date = withYear ? DateWords(d) : $"{d.day} {d.MonthName}";
            }
            return Trade.DebtDueName.Length > 0 ? Trade.DebtDueName + ", " + date : date;
        }

        /// <summary>
        /// A warning when the debt's day is a Sunday (Michaelmas 793 is one: Design/checks/calendar_first_season.md): on
        /// the Lord's day no market is held and no court sits, so the silver must be in hand the day before. "" on any
        /// other day, with no debt, or once the day is past.
        /// </summary>
        public static string DebtDayWarning()
        {
            var clock = LevelClock.Current;
            var game = KapmanGame.Current;
            if (clock == null || game == null || !Trade.Started || Trade.Debt <= 0.01 || Trade.DebtDaysLeft(game.Day) < 0.0)
                return "";
            var d = Calendar.At(clock.year, clock.startDay, Math.Floor(Trade.DebtDue) * 24.0);
            return DebtDayWarning(d.year, d.month, d.day, Trade.DebtDueName);
        }

        /// <summary>The same for any date (the test asks of 29 September 793).</summary>
        public static string DebtDayWarning(int year, int month, int day, string name)
        {
            if (FaithCalendar.Weekday(year, month, day) != 0)
                return "";
            var eve = Calendar.At(year, 1, (FaithCalendar.DayNumber(year, month, day) - FaithCalendar.DayNumber(year, 1, 1) - 1) * 24.0);
            string what = string.IsNullOrEmpty(name) ? "The day" : name;
            return $"{what} is a Sunday in {year}. On the Lord's day no market is held and no court sits: have the silver in hand by Saturday, {eve.day} {eve.MonthName}.";
        }

        /// <summary>What the purse buys where she lies, in deniers' worth: the coin of the place at its full silver, other
        /// coin and cut silver at a loss. At sea, or where there is no such loss: all the silver.</summary>
        public static double SilverHere(PortDef port)
        {
            if (!Trade.Started)
                return 0.0;
            var rules = TradeRules.Current;
            return port != null ? Trade.Purse.Worth(rules.ZoneOf(port.region), rules) : Trade.Purse.Silver(rules);
        }

        /// <summary>"Silver 86 d." where all of it is good money; "Silver 86 d: it buys 81 d's worth here, where coin of
        /// other lands and cut silver go at a loss." where it is not (a new game's purse is four kinds of silver: 40
        /// deniers of the king's coin and three others).</summary>
        public static string SilverWords(PortDef port)
        {
            double all = Silver, here = SilverHere(port);
            return all - here > 0.5 ? $"Silver {all:0} d: it buys {here:0} d's worth here, where coin of other lands and cut silver go at a loss."
                                    : $"Silver {all:0} d.";
        }

        /// <summary>Where a good aboard came from and what it cost, for the small line under it: "Came with the boat.
        /// Yours to sell, and owed for already.", "Cost you 43 d.", "Your own from the start." (The lender's casks are
        /// the player's to sell and are part of the debt: the critic's B3 of 8 Oct 2026.)</summary>
        public static string CostWords(Hold hold, GoodDef good, int count)
        {
            if (hold == null || good == null)
                return "";
            int owed = hold.Owed(good.id);
            double cost = hold.CostEach(good.id) * count;
            if (owed >= count)
                return "Came with the boat. Yours to sell, and owed for already.";
            string said = cost > 0.5 ? $"Cost you {cost:0} d." : "Your own from the start.";
            return owed > 0 ? said + $" {owed} of them came with the boat." : said;
        }

        /// <summary>What a new game puts aboard, as one sentence for the log (ruling 39: no colon, no brackets): "She has
        /// aboard 4 casks of Rhenish wine that came with the boat, 1 pair of Mayen quernstones and 1 sack of ship's
        /// bread." "" with an empty hold.</summary>
        public static string AboardWords()
        {
            if (!Trade.Started)
                return "";
            var parts = new List<string>();
            foreach (var (good, count) in Trade.Hold.Contents)
            {
                int owed = Trade.Hold.Owed(good.id);
                parts.Add(Units(good, count) + (owed >= count ? " that came with the boat" : owed > 0 ? $", {owed} of them with the boat from the start" : ""));
            }
            if (parts.Count == 0)
                return "";
            string list = parts.Count == 1 ? parts[0] : string.Join(", ", parts.GetRange(0, parts.Count - 1)) + " and " + parts[parts.Count - 1];
            return "She has aboard " + list + ".";
        }

        // ---- a landing's slip (Landings; the critic's B4 of 8 Oct 2026: "the first voyage ends and nothing marks it")

        /// <summary>The figures of the moment she lands: the purse, what the cargo cost her and what it would fetch at
        /// this port's market with the toll taken (her own bread and ale are not cargo), and the debt.</summary>
        public static Landings.Figures LandingFigures(PortDef here, KapmanGame game)
        {
            var f = new Landings.Figures { silver = Silver, debt = Trade.Started ? Trade.Debt : 0.0 };
            if (game == null || !Trade.Started)
                return f;
            f.daysLeft = Trade.DebtDaysLeft(game.Day);
            var clock = LevelClock.Current;
            f.date = clock != null ? $"{clock.Date.day} {clock.Date.MonthName}" : "";
            var market = here != null ? Markets.Of(here, game.Year, game.Day) : null;
            bool open = market != null && !market.Disrupted(game.Day);
            double toll = open ? market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day) : 0.0;
            foreach (var lot in Trade.Hold.Lots)
            {
                if (lot.good == null || Stores(lot.good.id))
                    continue;
                if (lot.whose.Length == 0)
                    f.cost += lot.cost;
                var wanted = open ? market.Get(lot.good.id) : null;
                if (wanted != null)
                    f.fetch += Deal.PaysFor(market, wanted, game.Month, "player") * lot.count * (1.0 - toll);
            }
            return f;
        }

        /// <summary>A good the crew eats or drinks: stores, not cargo.</summary>
        static bool Stores(string good) =>
            Array.IndexOf(Voyage.Rules.foodGoods, good) >= 0 || Array.IndexOf(Voyage.Rules.drinkGoods, good) >= 0;

        /// <summary>A landing as the log says it: the passage, the purse and the cargo, and what is owed. "Dorestat to
        /// Traiectum, half a day. The purse holds 12 d. What she carries cost 50 d and would fetch about 122 d here.
        /// Owed 1920 d, with 150 days left."</summary>
        public static string LandingWords(Landings.Landing l)
        {
            if (l == null)
                return "";
            string to = GameData.Port(l.to)?.name ?? l.to, from = GameData.Port(l.from)?.name ?? l.from;
            string took = FrameUi.Days(l.Took);
            string said = l.from.Length == 0 ? $"Made fast at {to}." : l.from == l.to ? $"Out from {to} and back, {took}." : $"{from} to {to}, {took}.";
            said += " " + LandingPurse(l);
            if (l.debt > 0.01)
                said += l.daysLeft > 0.0 ? $" Owed {l.debt:0} d, with {l.daysLeft:0} {(l.daysLeft == 1.0 ? "day" : "days")} left." : $" Owed {l.debt:0} d, and the day has come.";
            return said;
        }

        /// <summary>The purse and the cargo of a landing, as a sentence or two.</summary>
        public static string LandingPurse(Landings.Landing l)
        {
            if (l.cost < 0.5 && l.fetch < 0.5)
                return $"The purse holds {l.silver:0} d and there is nothing aboard to sell.";
            if (l.cost >= 0.5)
                return $"The purse holds {l.silver:0} d. What she carries cost {l.cost:0} d and would fetch about {l.fetch:0} d here.";
            return $"The purse holds {l.silver:0} d. What she carries would fetch about {l.fetch:0} d here.";
        }

        /// <summary>What Ravn says at the first landing of a game begun through the opening, by how the cargo stands
        /// ("" otherwise). Placeholders for the user's own words, in the voice of the opening's Ravn: glad, so he shortens his
        /// words. (Cut down on the script critic's report of 8 Oct 2026: he no longer explains the trade, and "Mostly dry" was the
        /// opening's joke over again.)</summary>
        public static string RavnAtLanding(Landings.Landing l, int landings)
        {
            if (l == null || landings != 1 || FirstJobs.Port.Length == 0 || l.from == l.to)
                return "";
            string line = l.cost >= 0.5 && l.fetch > l.cost * 1.1
                              ? "Look at that! It's worth more here than it was there. Sell before you eat."
                        : l.fetch >= 0.5 ? "We're here! Sell what will sell, and don't look at me if it won't."
                                         : "We're here! With nothing to sell, mind. Still. We're here.";
            return "\"" + line + "\" says Ravn.";
        }

        /// <summary>"4 casks of Rhenish wine": so many of a good by its unit, as Trade.Units has it, with the good's own
        /// name as it is written (a place in a name keeps its capital).</summary>
        public static string Units(GoodDef g, int n)
        {
            return Trade.UnitsNamed(g, n);                              // ("1 sack of ship's bread", "4 casks of Rhenish wine")
        }

        /// <summary>The player's silver, in deniers' worth.</summary>
        public static double Silver => Trade.Started ? Trade.Purse.Silver(TradeRules.Current) : 0.0;

        /// <summary>The goods aboard as one number that changes when the hold does (a panel's stamp).</summary>
        public static int HoldStamp()
        {
            int h = 17;
            if (!Trade.Started)
                return h;
            foreach (var lot in Trade.Hold.Lots)
                h = FrameUi.Mix(FrameUi.Mix(h, lot.good.id), lot.count);
            return h;
        }
    }

    /// <summary>
    /// THE MESSAGE LOG under the chart: the game's notices as they come (Voyage.Notice: what the voyage, the crew, the
    /// lookout and the graphs say), each with when it was said; and what the frame's own parts add (a deal done, word
    /// heard, what came of a card). Kept for the play; the frame shows the last of it.
    /// </summary>
    public static class FrameLog
    {
        public struct Line
        {
            public string text;
            /// <summary>Game days since the start (with the hours) when it was said.</summary>
            public double day;
            public string date, part;
            /// <summary>"" a plain line; "slip" a landing's slip, which the log frames.</summary>
            public string kind;
        }

        public const int Keep = 200;
        public static readonly List<Line> Lines = new List<Line>();
        /// <summary>Counts up with every line.</summary>
        public static int Version { get; private set; }

        /// <summary>A line into the log, stamped with the game's day and the part of it. The same words twice running in
        /// the same hour are said once.</summary>
        public static void Add(string text) => Add(text, "");

        /// <summary>The same, of a kind the log shows in its own way ("slip": a landing, framed).</summary>
        public static void Add(string text, string kind)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;
            double day = game != null ? game.Day : 0.0;
            if (Lines.Count > 0 && Lines[Lines.Count - 1].text == text && day - Lines[Lines.Count - 1].day < 1.0 / 24.0)
                return;
            Lines.Add(new Line
            {
                text = text.Trim(), day = day, kind = kind ?? "",
                date = clock != null ? $"{clock.Date.day} {clock.Date.MonthName}" : "",
                part = clock != null ? FrameUi.PartOfDay(clock.HourOfDay) : "",
            });
            if (Lines.Count > Keep)
                Lines.RemoveRange(0, Lines.Count - Keep);
            Version++;
        }

        /// <summary>The log for the saved game: every line kept (200 at most), each with when it was said.</summary>
        public static List<object> Capture()
        {
            var saved = new List<object>(Lines.Count);
            foreach (var l in Lines)
                saved.Add(new Dictionary<string, object> { ["text"] = l.text, ["day"] = l.day, ["date"] = l.date ?? "", ["part"] = l.part ?? "", ["kind"] = l.kind ?? "" });
            return saved;
        }

        /// <summary>The log of a loaded game. Null (a save from before the log was kept): empty, never the lines of the
        /// game that was running.</summary>
        public static void Restore(List<object> saved)
        {
            Lines.Clear();
            if (saved != null)
                foreach (object o in saved)
                {
                    string text = Json.Str(o, "text");
                    if (text.Length > 0)
                        Lines.Add(new Line { text = text, day = Json.Num(o, "day"), date = Json.Str(o, "date"), part = Json.Str(o, "part"), kind = Json.Str(o, "kind") });
                }
            if (Lines.Count > Keep)
                Lines.RemoveRange(0, Lines.Count - Keep);
            Version++;                                                    // (the frame draws the log again)
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Lines.Clear();
            Version = 0;
        }
    }
}
