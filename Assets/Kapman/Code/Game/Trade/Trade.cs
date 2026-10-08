using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>What a buy or a sale came to, as plain facts (the market panel and the graphs show them).</summary>
    public sealed class TradeResult
    {
        public bool ok;
        public bool bought;
        public string port, good;
        public int count;
        /// <summary>Deniers that changed hands (a sale: before the toll), and the toll taken.</summary>
        public double deniers, toll;
        /// <summary>The facts: "Bought 5 sacks of wheat for 34 d: paid 34 deniers." Or why not.</summary>
        public string text;
        /// <summary>The laws that bite on this good here and now (their notes), if any.</summary>
        public List<string> laws = new List<string>();
    }

    /// <summary>
    /// THE MERCHANT'S TRADE: the purse, the hold, the ledger and the debt, and buying and selling in a market.
    /// Prices move as you deal: each unit bought makes the next dearer, each sold makes the next cheaper. Sales pay
    /// the zone's toll. Every deal is told to the graphs (Trade: On trade), so reputation, events and the like can
    /// answer it there.
    /// </summary>
    public static class Trade
    {
        public static Purse Purse { get; private set; }
        public static Hold Hold { get; private set; }
        public static Ledger Ledger { get; private set; }
        /// <summary>Deniers owed, to whom (a writing key: the creditor's name is yours to write), and the game day it's due.</summary>
        public static double Debt { get; set; }

        /// <summary>The flag a game carries when its debt was laid on it by the lender of the opening (Showtime), not by
        /// the count's steward on the first morning (the chain house_debt, which then sleeps).</summary>
        public const string LenderFlag = "lender_opening";
        /// <summary>The lender's name, and the byname the waterfront gives him. Placeholders: the byname is made from
        /// Old Frisian woker, usury, and that word needs its source checked.</summary>
        public const string LenderName = "Wursing", LenderByname = "the W\u014dkerer";

        /// <summary>
        /// THE BOAT'S WORK (8 Oct 2026, night; decided by me while the user had said "Do what you think needs to be
        /// done", and theirs to overrule: DECISIONS 104). What follows when the day passes unpaid had never been
        /// ruled or built: the bar turned to "overdue" and nothing happened. The lender may ask back no more than
        /// the sum (the king's law of 789), so a late debtor pays in other coin: the boat's work. From the day he says
        /// so (the card harbour.wokerers_day sets this flag) a share of everything she sells goes to the stick before
        /// it reaches the purse (TradeRules.workShare, a half), until the stick is clean. Nothing is added to the
        /// debt and the game does not end.
        /// </summary>
        public const string WorkFlag = "wk_work";

        /// <summary>She works for the lender now: a share of what she sells goes to the stick.</summary>
        public static bool Working => Started && Debt > 0.01 && ContentFlags.Ready && ContentFlags.Has(WorkFlag);

        /// <summary>Who the debt is owed to, as it is said: the user's own writing for the creditor's key, else the
        /// lender of the opening, else the count.</summary>
        public static string CreditorSaid
        {
            get
            {
                string who = TextPanel.Current != null ? TextPanel.Current.Writing(Creditor) : "";
                if (!string.IsNullOrEmpty(who) && !who.StartsWith("[write:", StringComparison.Ordinal))
                    return who;
                return ContentFlags.Ready && ContentFlags.Has(LenderFlag) ? LenderByname : "the count";
            }
        }

        /// <summary>"the count's own", "the W\u014dkerer's own": whose the goods put aboard with the boat are.</summary>
        public static string Creditor { get; private set; }
        public static double DebtDue { get; private set; }
        /// <summary>What the due day is called ("Michaelmas"), or "" when it is only a count of days.</summary>
        public static string DebtDueName { get; private set; } = "";

        /// <summary>A deal was done (the graphs' "Trade: On trade").</summary>
        public static event Action<TradeResult> Traded;

        public static bool Started => Purse != null;

        /// <summary>A new merchant: the starting purse and debt from the rules, an empty hold of the ship's size.
        /// The debt falls due after the rules' count of days (no calendar is known: tests, a level with no clock).</summary>
        public static void Begin(TradeRules rules, float cargoKg, double today)
        {
            Purse = new Purse();
            foreach (var kv in rules.startCoins)
                Purse.Add(kv.Key, kv.Value);
            Hold = new Hold { capacity = cargoKg };
            Ledger = new Ledger();
            Debt = rules.startDebt;
            Creditor = rules.creditor;
            DebtDue = today + rules.dueDays;
            DebtDueName = "";
        }

        /// <summary>The same, with the calendar known: the debt falls due on the rules' day of the year (Michaelmas, 29
        /// September), the next one after today. `year`, `startDay`: the year and the day of the year that game day 0 is.</summary>
        public static void Begin(TradeRules rules, float cargoKg, double today, int year, int startDay)
        {
            Begin(rules, cargoKg, today);
            double due = DueDayOf(year, startDay, today, rules.dueMonth, rules.dueDay);
            if (due < 0.0)
                return;
            DebtDue = due;
            DebtDueName = rules.dueName ?? "";
        }

        /// <summary>The game day (whole: its midnight) of the next `day` of `month` after `today`; -1 if there is none
        /// within two years (a month of 0, or a day that month never has). Begun on the day itself: a year on.</summary>
        public static double DueDayOf(int year, int startDay, double today, int month, int day)
        {
            if (month < 1 || month > 12 || day < 1)
                return -1.0;
            int from = (int)Math.Floor(today) + 1;
            for (int i = from; i < from + 732; i++)
            {
                var d = Kapman.World.Calendar.At(year, startDay, i * 24.0);
                if (d.month == month && d.day == day)
                    return i;
            }
            return -1.0;
        }

        /// <summary>Whole days left to the due day: 0 on the day itself, below 0 after it. One count for every place that
        /// says it (the bar, the panels, the port's talk, the cards).</summary>
        public static double DebtDaysLeft(double today) => Math.Floor(DebtDue) - Math.Floor(today);

        /// <summary>The merchant's affairs as plain data (a saved game).</summary>
        public static Dictionary<string, object> Capture() => !Started ? null : new Dictionary<string, object>
        {
            ["purse"] = Purse.Capture(), ["hold"] = Hold.Capture(), ["capacity"] = (double)Hold.capacity, ["ledger"] = Ledger.Capture(),
            ["debt"] = Debt, ["creditor"] = Creditor, ["due"] = DebtDue, ["due_name"] = DebtDueName,
        };

        public static void Restore(Dictionary<string, object> d)
        {
            if (d == null || !Started)
                return;
            Purse.Restore(Json.Obj(d, "purse"));
            Hold.capacity = (float)Json.Num(d, "capacity", Hold.capacity);
            Hold.Restore(Json.Arr(d, "hold"));
            Ledger.Restore(Json.Arr(d, "ledger"));
            Debt = Json.Num(d, "debt");
            Creditor = Json.Str(d, "creditor", Creditor);
            DebtDue = Json.Num(d, "due");
            DebtDueName = Json.Str(d, "due_name", "");                   // (a save from before the day had a name: only the day)
        }

        // Enter Play Mode runs without a domain reload: start each play with no merchant (the graphs' bridge keeps
        // its hold on Traded, so that isn't cleared)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Purse = null;
            Hold = null;
            Ledger = null;
            DebtDueName = "";
        }

        /// <summary>Buys up to `count` units, as many as there are on offer, room for and silver to pay (through the
        /// same <see cref="Deal"/> as every ship), in the words of the place.</summary>
        public static TradeResult Buy(Market m, string goodId, int count, double day, int month, int year)
        {
            var r = new TradeResult { bought = true, port = m.port.id, good = goodId };
            var good = GameData.Good(goodId);
            if (good != null)
                r.laws = LawNotes(good, m.zone, year);
            if (Reputation.Refuses(m.port))
                return Fail(r, "They won't deal with you here.");
            var d = Deal.Buy(m, Purse, Hold, goodId, count, month, day, "player");
            if (!d.ok)
                return Fail(r, Why(m, d.why, good));
            r.ok = true;
            r.count = d.count;
            r.deniers = d.deniers;
            Reputation.Dealt(m.port, day);
            if (good != null && good.cat == "Captives")
                Sim.Deeds.Record("bought_captives", $"bought {d.count} captives", m.port);
            r.text = Wording.Outcome(m.port, Purse.LastBack.Length > 0 ? "bought_change" : "bought",
                                     Purse.LastBack.Length > 0 ? "Bought {goods} for {price}: paid {coins}, {back} back." : "Bought {goods} for {price}: paid {coins}.",
                                     // (the price to the tenth: whole coins are paid, so "for 34 d: paid 35 deniers" read as a
                                     // fault of the sum. "For 34.4 d: paid 35 deniers" is the coin rounding it is.)
                                     ("goods", Units(good, d.count)), ("good", good.name.ToLowerInvariant()), ("price", Exact(m.zone, d.deniers)),
                                     ("coins", Purse.LastCoins), ("back", Purse.LastBack)) +
                     (d.why.Length > 0 ? " " + Why(m, d.why, good, true) : "");
            Traded?.Invoke(r);
            return r;
        }

        /// <summary>Sells up to `count` units from the hold; the zone's toll comes off what the market pays.</summary>
        public static TradeResult Sell(Market m, string goodId, int count, double day, int month, int year)
        {
            var r = new TradeResult { bought = false, port = m.port.id, good = goodId };
            var good = GameData.Good(goodId);
            if (good != null)
                r.laws = LawNotes(good, m.zone, year);
            if (Reputation.Refuses(m.port))
                return Fail(r, "They won't deal with you here.");
            var d = Deal.Sell(m, Purse, Hold, goodId, count, month, day, "player");
            if (!d.ok)
                return Fail(r, Why(m, d.why, good));
            r.ok = true;
            r.count = d.count;
            r.deniers = d.deniers;
            r.toll = d.toll;
            Reputation.Dealt(m.port, day);
            if (good != null && good.cat == "Captives")
                Sim.Deeds.Record("sold_captives", $"sold {d.count} captives", m.port);
            r.text = Wording.Outcome(m.port, d.toll > 0 ? "sold_toll" : "sold",
                                     d.toll > 0 ? "Sold {goods} for {price}, less the toll of {toll_paid}: took {coins}." : "Sold {goods} for {price}: took {coins}.",
                                     ("goods", Units(good, d.count)), ("good", good.name.ToLowerInvariant()), ("price", m.zone.Show(d.deniers)),
                                     ("coins", Purse.LastCoins), ("toll_paid", m.zone.Show(d.toll))) +
                     SaleReckoned(m.zone, d, Hold.LastFree, Hold.LastOwed);
            r.text += WorkShare(m.zone, d.deniers - d.toll);
            Traded?.Invoke(r);
            return r;
        }

        /// <summary>The stick's share of a sale while she works for the lender (see <see cref="WorkFlag"/>): taken from
        /// the purse and off the debt, and said in a sentence after the sale's own (with a space before it, or "").</summary>
        static string WorkShare(ZoneDef zone, double took)
        {
            if (!Working || took <= 0.0)
                return "";
            var rules = TradeRules.Current;
            double share = Math.Min(Debt, Math.Floor(took * rules.workShare));
            if (share < 1.0)
                return "";
            double given = share;
            if (!Purse.Pay(share, zone, rules, out _))
                given = Purse.PayUpTo(share, rules);
            if (given <= 0.0)
                return "";
            Debt = Math.Max(0.0, Debt - given);
            string who = CreditorSaid;
            return Debt <= 0.01
                ? $" Of that, {zone.Show(given)} went to the stick, and the stick is clean."
                : $" Of that, {zone.Show(given)} went to the stick, for she works for {who}. Still owed: {zone.Show(Debt)}.";
        }

        /// <summary>
        /// What a sale came to against what the goods cost, as the words after "took N deniers." (with a space before
        /// them, or ""). Gain is reckoned only on the units that were bought: `free` of those sold had cost nothing, and
        /// `owed` of them came with the boat, put aboard by the lender at the start. Of those nothing is said: they are
        /// the merchant's to sell and are owed for with the boat (the lender: "They are on the stick too"), so what they
        /// fetch is neither told as gain (money.md 10: the first reckoning must not read "made 215 d") nor as another
        /// man's money. (Until 8 Oct 2026 a sentence here called it "his stake and not your gain", which read as if
        /// the silver were not the player's to spend: the critic's B3, and it came back for every cask.)
        /// </summary>
        public static string SaleReckoned(ZoneDef zone, DealResult d, int free, int owed)
        {
            free = Math.Max(0, Math.Min(free, d.count));
            owed = Math.Max(0, Math.Min(owed, free));
            int bought = d.count - free;
            string said = "";
            // (each a sentence of its own, with no colon in it: ruling 39 of 7 Oct, the log must read as sentences)
            if (d.cost > 0 && bought > 0)
            {
                double gain = (d.deniers - d.toll) * bought / d.count - d.cost;
                string it = bought == 1 ? "it" : "they";
                said = gain >= 0 ? $"{zone.Show(gain)} more than {it} cost." : $"{zone.Show(-gain)} less than {it} cost.";
                if (free > 0)
                    said = (bought == 1 ? "The one you bought fetched " : $"The {bought} you bought fetched ") + said;
                else
                    said = "That is " + said;
            }
            return said.Length > 0 ? " " + said : "";
        }

        /// <summary>Why a deal failed, or stopped short (`short`), in the words of the place.</summary>
        static string Why(Market m, string why, GoodDef good, bool shortOf = false)
        {
            string name = good != null ? good.name.ToLowerInvariant() : "";
            switch (why)
            {
                case "not_dealt": return Wording.Outcome(m.port, "not_dealt", "Nobody here deals in {good}.", ("good", name));
                case "none_left": return shortOf ? Wording.Outcome(m.port, "short_stock", "That was all they had.") : Wording.Outcome(m.port, "none_left", "None left to buy.");
                case "no_room": return shortOf ? Wording.Outcome(m.port, "short_room", "The hold is full.") : Wording.Outcome(m.port, "no_room", "No room in the hold.");
                case "no_silver": return shortOf ? Wording.Outcome(m.port, "short_silver", "Your silver ran out.") : Wording.Outcome(m.port, "no_silver", "Not enough silver.");
                case "none_aboard": return "None in the hold.";
                case "disrupted": return Wording.Outcome(m.port, "disrupted", "The market is shut.");
                case "ruined": return Wording.Outcome(m.port, "ruined", "Nobody trades here any more.");
                default: return "Nothing to sell.";
            }
        }

        /// <summary>Pays toward the debt (deniers' worth, here).</summary>
        public static TradeResult PayDebt(Market m, double deniers)
        {
            var rules = TradeRules.Current;
            var r = new TradeResult { port = m?.port.id };
            deniers = Math.Min(deniers, Debt);
            if (deniers <= 0.0)
                return Fail(r, "Nothing owed.");
            if (!Purse.Pay(deniers, m?.zone, rules, out string how))
                return Fail(r, how);
            Debt -= deniers;
            r.ok = true;
            r.deniers = deniers;
            r.text = $"Paid {m.zone.Show(deniers)} toward the debt: {how}. Still owed: {m.zone.Show(Debt)}.";
            return r;
        }

        public static List<string> LawNotes(GoodDef good, ZoneDef zone, int year)
        {
            var notes = new List<string>();
            foreach (var l in TradeRules.Current.LawsOn(good, zone, year))
                notes.Add(l.note);
            return notes;
        }

        /// <summary>"5 sacks of wheat" (the unit's first word, made plural).</summary>
        public static string Units(GoodDef g, int n)
        {
            string unit = string.IsNullOrEmpty(g.unit) ? "" : g.unit.Split(' ', '(', ',')[0];
            if (string.IsNullOrEmpty(unit))
                return $"{n} {g.name.ToLowerInvariant()}";
            if (n != 1)
                unit = unit.EndsWith("s") || unit.EndsWith("sh") || unit.EndsWith("ch") ? unit + "es" : unit.EndsWith("y") ? unit.Substring(0, unit.Length - 1) + "ies" : unit + "s";
            return $"{n} {unit} of {g.name.ToLowerInvariant()}";
        }

        /// <summary>A sum in a zone's reckoning, to the tenth whatever its size ("34.4 d").</summary>
        static string Exact(ZoneDef zone, double deniers) =>
            (deniers / Mathf.Max(0.0001f, zone.unitDeniers)).ToString("0.#") + " " + zone.unit;

        /// <summary>"4 casks of Rhenish wine", "1 pair of Mayen quernstones": so many of a good by its unit, with the
        /// good's own name as it is written (a place in a name keeps its capital). For what is said to the player of
        /// goods taken, seized or thrown over: "They took 4 Rhenish wine" was no sentence.</summary>
        public static string UnitsNamed(GoodDef g, int n)
        {
            if (g == null)
                return n.ToString();
            string counted = Units(g, n), lower = g.name.ToLowerInvariant();
            return counted.EndsWith(lower, StringComparison.Ordinal) ? counted.Substring(0, counted.Length - lower.Length) + Named(g) : counted;
        }

        // the first words of goods' names that are a place's or a people's: such a name keeps its capital inside a sentence
        static readonly string[] ProperFirst = { "Rhenish", "Frankish", "Droitwich", "Frisian", "Mayen", "Tating", "Pingsdorf", "Ipswich", "Ulfberht", "Badorf" };

        /// <summary>A good's name as it stands inside a sentence: "Rhenish wine" keeps its capital, "Ship's bread" is
        /// "ship's bread", "Wine of Aquitaine" is "wine of Aquitaine".</summary>
        public static string Named(GoodDef g)
        {
            string name = g != null ? g.name ?? "" : "";
            if (name.Length == 0)
                return name;
            foreach (string proper in ProperFirst)
                if (name.StartsWith(proper, StringComparison.Ordinal))
                    return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        static TradeResult Fail(TradeResult r, string why)
        {
            r.ok = false;
            r.text = why;
            return r;
        }
    }
}