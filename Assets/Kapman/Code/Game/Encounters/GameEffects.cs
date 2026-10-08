using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Encounters;

namespace Kapman.Game
{
    /// <summary>
    /// THE ENCOUNTERS' EFFECTS ON THE REAL GAME (npc_text_encounters.md 4.1; the first build's GameEvents.Apply is the
    /// model): silver into and out of the purse in the port's money, goods into and out of the hold, the crew's health,
    /// heart and men, deeds that move the standing (Sim.Deeds, so the deed is logged and its news travels), news on a
    /// port's board, a market's price pushed, an order posted, texts and riddles into the chest, and the law's seizure
    /// of forbidden goods. Each says what happened as a plain fact for the card's outcome (null: nothing to say); the
    /// facts' wording is a short placeholder, the user's to change.
    /// Not built yet, so they do nothing and say nothing: the silver fetch, passengers, the sea sim's orders from a card,
    /// a fine or a hearing (no law state yet), learning to read, and tongues.
    /// </summary>
    public sealed class GameEffects : IGameEffects
    {
        /// <summary>The port of the call in progress ("" at sea): its money is what silver is paid and taken in.</summary>
        public Func<string> portNow;
        /// <summary>The cards' writing, for a news item's words.</summary>
        public EncounterBook book;

        int ordersMade;

        PortDef Here => GameData.Port(portNow != null ? portNow() : null);

        static double Today => KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0;
        static int Year => KapmanGame.Current != null ? KapmanGame.Current.Year : 793;
        static int Month => KapmanGame.Current != null ? KapmanGame.Current.Month : 6;

        ZoneDef Zone
        {
            get
            {
                var p = Here;
                return p != null ? TradeRules.Current.ZoneOf(p.region) : null;
            }
        }

        /// <summary>Deniers as the port reckons them ("12 d"); at sea, in deniers.</summary>
        string Show(double deniers)
        {
            var z = Zone;
            return z != null ? z.Show(deniers) : deniers.ToString("0.#", CultureInfo.InvariantCulture) + " d";
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
        static string Name(Npc n) => n == null ? "" : string.IsNullOrEmpty(n.name) ? "The " + n.role.Replace('_', ' ') : n.name;

        // ------------------------------------------------------------------ the merchant

        public string Silver(double deniers)
        {
            if (!Trade.Started || deniers == 0.0)
                return null;
            var rules = TradeRules.Current;
            var zone = Zone;
            if (deniers > 0.0)
            {
                Trade.Purse.Receive(deniers, zone, rules, out _);
                return $"You took {Show(deniers)}.";
            }
            double owed = -deniers;
            if (Trade.Purse.Pay(owed, zone, rules, out _))
                return $"You paid {Show(owed)}.";
            double paid = Trade.Purse.PayUpTo(owed, rules);
            return $"You paid {Show(paid)}, all the silver aboard.";
        }

        public string Goods(string good, int count)
        {
            var g = GameData.Good(good);
            if (g == null || count == 0 || !Trade.Started)
                return null;
            if (count > 0)
            {
                int n = Math.Min(count, Trade.Hold.RoomFor(g));
                if (n <= 0)
                    return "There was no room in the hold.";
                Trade.Hold.Add(g, n, 0.0, Today);
                return $"You took {Trade.Units(g, n)} aboard.";
            }
            int k = Math.Min(-count, Trade.Hold.Count(g.id));
            if (k <= 0)
                return null;
            Trade.Hold.Remove(g, k);
            return $"{Cap(Trade.Units(g, k))} went out of the hold.";
        }

        public string Health(double share)
        {
            if (share == 0.0)
                return null;
            Voyage.Affect(share, 0.0);
            return share > 0.0 ? "The crew are in better health." : "The crew are in worse health.";
        }

        public string Morale(double share)
        {
            if (share == 0.0)
                return null;
            Voyage.Affect(0.0, share);
            return share > 0.0 ? "The crew are in better heart." : "The crew have lost heart.";
        }

        public string Men(int count)
        {
            if (count > 0)
            {
                Voyage.Hire(count);
                return count == 1 ? "You took on a man." : $"You took on {count} men.";
            }
            if (count < 0)
            {
                int lost = Voyage.LoseMen(-count);
                return lost <= 0 ? null : lost == 1 ? "You lost a man." : $"You lost {lost} men.";
            }
            return null;
        }

        // ------------------------------------------------------------------ standing, news, markets, orders

        public string Deed(string kind, string port)
        {
            var p = GameData.Port(port) ?? Here;
            if (p == null || string.IsNullOrEmpty(kind))
                return null;
            double before = Reputation.At(p);
            Sim.Deeds.Record(kind, kind.Replace('_', ' '), p);
            double after = Reputation.At(p);
            if (Math.Abs(after - before) < 0.5)
                return null;
            return $"Your standing at {p.name} {(after > before ? "rises" : "falls")}. You are {Reputation.Word(after)} there now.";
        }

        public string News(string key, string port)
        {
            var p = GameData.Port(port) ?? Here;
            var world = KapmanGame.Current != null ? KapmanGame.Current.World : null;
            if (p == null || world == null || string.IsNullOrEmpty(key))
                return null;
            world.Post("encounter", book != null ? book.Shown(key) : "[write: " + key + "]", p.index);
            return null;                                                  // (news goes round quietly; the board shows it)
        }

        /// <summary>The good's price at the port is pushed by the factor. Approximation: the market's dearness fades by the
        /// trade rules' shock fade, day by day, rather than ending on the day given.</summary>
        public string PriceShock(string port, string good, double factor, double days)
        {
            var p = GameData.Port(port) ?? Here;
            if (p == null || factor <= 0.0 || GameData.Good(good) == null)
                return null;
            var m = Markets.Of(p, Year, Today);
            if (m == null)
                return null;
            m.Shock(g => g.id == good, 1f, (float)(factor - 1.0));
            return null;
        }

        public string Commission(string port, string good, int count, double days, double premium)
        {
            var p = GameData.Port(port) ?? Here;
            var g = GameData.Good(good);
            if (p == null || g == null || count <= 0 || days <= 0.0)
                return null;
            var m = Markets.Of(p, Year, Today);
            var mg = m != null ? m.Get(good) : null;
            double each = (mg != null ? m.Pays(mg, Month) : g.value) * (1.0 + premium);
            Commissions.All.Add(new Kapman.Game.Commission
            {
                id = "enc" + (++ordersMade) + "." + p.id + "." + Today.ToString("0.###", CultureInfo.InvariantCulture),
                port = p.id, good = g.id, units = count, pay = Math.Round(each * count), posted = Today, deadline = Today + days,
            });
            return $"You have an order at {p.name} for {Trade.Units(g, count)}, wanted within {days.ToString("0", CultureInfo.InvariantCulture)} days.";
        }

        // ------------------------------------------------------------------ the chest

        public string GiveText(string id, string from, string port)
        {
            var p = GameData.Port(port) ?? Here;
            var item = Chest.Give("text", id, from, p != null ? p.id : null, true);
            return item == null ? null : $"You have a text{(string.IsNullOrEmpty(from) ? "" : " from " + from)} in the chest now.";
        }

        public string GiveRiddle(string id, string from, string port)
        {
            var p = GameData.Port(port) ?? Here;
            var item = Chest.Give("riddle", id, from, p != null ? p.id : null, true);
            return item == null ? null : $"You have a riddle{(string.IsNullOrEmpty(from) ? "" : " from " + from)} in the chest now.";
        }

        public string OpenFetch(string id) => null;                       // (the silver fetch: to come)

        // ------------------------------------------------------------------ people

        public string Hire(Npc npc)
        {
            if (npc == null)
                return null;
            Voyage.Hire(1);
            return $"{Name(npc)} joins the crew.";
        }

        /// <summary>A man paid off: one fewer aboard (Voyage has only LoseMen, which also lowers the heart a little).</summary>
        public string PayOff(Npc npc)
        {
            if (npc == null || Voyage.LoseMen(1) <= 0)
                return null;
            return $"{Name(npc)} is paid off.";
        }

        public string Passenger(Npc npc, string port) => null;            // (passengers: to come)

        public string Encounter(string kind) => null;                     // (a card's order to the sea sim: to come)

        // ------------------------------------------------------------------ the law, reading, tongues

        /// <summary>"seizure": the forbidden goods aboard (arms and captives, as the cards' hold.weapons and hold.captives
        /// count them) are taken. Fines and hearings wait for the law's state.</summary>
        /// <summary>The share of a debt reckoned by tale that its weight in the king's new, heavier penny comes to (the
        /// debt chain's fifth card: 367 new pennies for 480 old), and the share a surety asks down.</summary>
        public const double DebtByWeight = 367.0 / 480.0, DebtSurety = 0.25;

        /// <summary>
        /// THE DEBT FOR THE BOAT MET THROUGH A CARD ("law debt_paid", "law debt_weight", "law debt_quarter",
        /// "law debt_cleared"). It is the same debt the bar shows (Trade.Debt), whatever has been paid toward it at a
        /// market already: paid in full by tale, by weight in the new penny, a quarter down on a surety, or struck off
        /// with no silver. Until 7 Oct 2026 the cards took a sum written in them (480 d) and the bar's debt never moved.
        /// From 8 Oct 2026, for the lender's man who cuts a notch wherever he finds her: "law debt_pay_240" pays that
        /// many pennies on the stick (or what is still owed, if less), and "law debt_all_aboard" pays all the silver
        /// aboard toward it (the day itself, when she has not got it).
        /// </summary>
        string DebtLaw(string kind)
        {
            if (!Trade.Started)
                return null;
            double owed = Trade.Debt;
            if (owed <= 0.01)
            {
                Trade.Debt = 0.0;
                return null;
            }
            if (kind == "debt_cleared")
            {
                Trade.Debt = 0.0;
                return "You owe nothing for the boat now.";
            }
            if (kind == "debt_all_aboard" || kind.StartsWith("debt_pay_", StringComparison.Ordinal))
            {
                double want = owed;
                if (kind != "debt_all_aboard"
                    && (!double.TryParse(kind.Substring("debt_pay_".Length), System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out want) || want <= 0.0))
                    return null;
                want = Math.Min(Math.Round(want), owed);
                var purse = Trade.Purse;
                double given = want;
                if (kind == "debt_all_aboard" || !purse.Pay(want, Zone, TradeRules.Current, out _))
                    given = purse.PayUpTo(want, TradeRules.Current);
                if (given <= 0.0)
                    return "There was no silver aboard to give him.";
                Trade.Debt = Math.Max(0.0, owed - given);
                return Trade.Debt <= 0.01
                    ? $"You paid {Show(given)} on the stick. You owe nothing for the boat now."
                    : $"You paid {Show(given)} on the stick. Still owed: {Show(Trade.Debt)}.";
            }
            double due = Math.Round(owed * (kind == "debt_weight" ? DebtByWeight : kind == "debt_quarter" ? DebtSurety : 1.0));
            var rules = TradeRules.Current;
            double paid = due;
            if (!Trade.Purse.Pay(due, Zone, rules, out _))
                paid = Trade.Purse.PayUpTo(due, rules);
            if (paid + 0.5 < due)
            {
                Trade.Debt = Math.Max(0.0, owed - paid);                  // (short of it: what was paid comes off, the rest stands)
                return $"You paid {Show(paid)}, all the silver aboard. You still owe {Show(Trade.Debt)}.";
            }
            Trade.Debt = 0.0;
            return $"You paid {Show(paid)}. You owe nothing for the boat now.";
        }

        public string Law(string kind, double days, string port)
        {
            if (kind != null && kind.StartsWith("debt_", StringComparison.Ordinal))
                return DebtLaw(kind);
            if (kind != "seizure" || !Trade.Started)
                return null;
            var taken = new List<string>();
            foreach (var (good, count) in new List<(GoodDef good, int count)>(Trade.Hold.Contents))
            {
                if (count <= 0 || !(EncounterGameVars.IsWeapon(good) || EncounterGameVars.IsCaptive(good)))
                    continue;
                Trade.Hold.Remove(good, count);
                taken.Add(Trade.Units(good, count));
            }
            return taken.Count == 0 ? null : "They seized " + string.Join(", ", taken) + ".";
        }

        public string Learn(string what, double amount) => null;          // (reading lessons from a card: to come)

        // ------------------------------------------------------------------ marks on the stats

        /// <summary>
        /// EVENTS MOVE THE STATS (the user, 7 Oct 2026; Design/fixes/stats_events.md). A card's choice puts a mark on a
        /// stat ("mark nerve +1 stood_at_the_bar for 30", "mark troth -1 broke_the_fast until shriven"). Its cause, as
        /// the player reads it, is the writing under "mark.<id>" (and "mark.<id>.until" for an end that is no count of
        /// days); where nobody has written one, the mark's own name made plain.
        /// </summary>
        public string Mark(string stat, double size, string id, double days, string untilFlag)
        {
            string why = book != null ? book.Written("mark." + id) : null;
            string until = book != null ? book.Written("mark." + id + ".until") : null;
            var m = Self.Mark(stat, (int)Math.Round(size), id, days, untilFlag, why ?? Self.PlainCause(id), Today, until ?? "");
            return Self.Said(m, Today);
        }

        public string Unmark(string id)
        {
            Self.Unmark(id);
            return null;
        }

        /// <summary>The base of a stat moved by one, for good (rare: a winter with the monks, an oath broken before
        /// witnesses).</summary>
        public string StatBase(string stat, double by)
        {
            if (!Self.IsStat(stat) || by == 0.0)
                return null;
            int before = Self.Stat(stat);
            Self.SetStat(stat, Self.StatRaw(stat) + (by > 0.0 ? 1.0 : -1.0));
            return Self.Stat(stat) == before ? null : $"Your {Self.StatName(stat)} is {(by > 0.0 ? "up" : "down")} by one, for good.";
        }

        /// <summary>A tongue learned to a level (7 Oct 2026: the self holds the player's tongues). It never takes a
        /// tongue away: a level at or under what is held changes nothing and says nothing.</summary>
        public string Tongue(string name, double level)
        {
            string id = Self.TongueId(name);
            if (id.Length == 0 || level <= Self.TongueHeld(id) + 1e-9)
                return null;
            Self.SetTongue(id, level);
            string tongue = Self.TongueName(id);
            return level >= 0.9 ? $"You have {tongue} now." : $"You follow some {tongue} now.";
        }
    }
}
