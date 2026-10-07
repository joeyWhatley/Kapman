using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game.Sim;

namespace Kapman.Game
{
    /// <summary>
    /// A PORT'S LIFE - what the port menu's other choices do (the user's menu: Talk to people, Visit the toll house,
    /// Exchange; and the place's own errands from wording_793.json: lodging, alms, the monastery's guest house and its
    /// trade). Each returns plain facts and changes what it changes (the ledger, the purse, the crew). No story: your
    /// writing for each key goes in writing.json.
    /// </summary>
    public static class PortLife
    {
        /// <summary>The menu's fixed options (town, talk, cast off) come first; the place's errands after them.</summary>
        public const int FixedOptions = 3;

        /// <summary>The errand behind a choice on the port menu (null: not an errand).</summary>
        public static string ErrandKey(PortDef port, int choice)
        {
            var list = Wording.Errands(port);
            int i = choice - FixedOptions;
            return i >= 0 && i < list.Count ? list[i].key : null;
        }

        /// <summary>The errand's label (for the panel's title).</summary>
        public static string ErrandLabel(PortDef port, int choice)
        {
            var list = Wording.Errands(port);
            int i = choice - FixedOptions;
            return i >= 0 && i < list.Count ? list[i].label : "";
        }

        /// <summary>Talk to people: the waterfront's news you haven't had, with its age and who brought it (kept in what you
        /// know: Known); the prices known here go into your price notes, as told (by a witness if the ship that brought them
        /// saw them herself, else as talk, a little less sure and maybe a little off); a fair, if one is on.</summary>
        public static string Talk(PortDef port)
        {
            var game = KapmanGame.Current;
            var w = game != null ? game.World : null;
            if (port == null || w == null)
                return "Nobody has much to say.";
            var board = w.Board(port.index);
            var lines = new List<string>();
            var fair = Fairs.On(w, port.index);
            if (fair != null)
                lines.Add($"{Cap(fair.name)} is on here.");
            int told = 0;
            double today = game.Day;
            Known.HearOf(port);
            foreach (var (item, learned, from) in board.News)
            {
                if (Known.HeardNews(item.id))
                    continue;
                Known.HearNews(item, today, from.StartsWith("the waterfront") ? "talk on the waterfront at " + port.name : from + ", told at " + port.name,
                               How.Talk, from.StartsWith("the waterfront") ? 1 : 2, port.index);
                lines.Add($"{item.text} ({Age(w.Day - item.day)}; " + (from.StartsWith("the waterfront") ? "known here" : "brought by " + from) + ").");
                if (++told >= 8)
                    break;
            }
            foreach (var c in Commissions.At(port))
                lines.Add($"Wanted here: {Commissions.Describe(c)}.");
            foreach (var c in Commissions.Mine)
                lines.Add($"Your order: {Commissions.Describe(c)}.");
            var prices = new List<string>();
            if (Trade.Started)
                foreach (var snap in board.All)
                {
                    if (snap.port == port.index)
                        continue;
                    // the ship that saw them is in port, her master there to say so; or it's the waterfront's word
                    var master = snap.seenBy != null ? w.ships.Find(s => s.id == snap.seenBy && s.at == port.index && s.state != ShipState.Sailing) : null;
                    bool witness = master != null;
                    // (who told it, for the word on the item: the master, of his home port; or someone on this waterfront)
                    string tellerOf = witness && master.home >= 0 && master.home < GameData.Ports.Count ? GameData.Ports[master.home].id : port.id;
                    if (Trade.Ledger.Hear(snap, (witness ? "told by the master who saw them, at " : "talk on the waterfront at ") + port.name,
                                          witness ? How.Told : How.Talk, witness ? 0 : 1, today, witness ? "skipper" : "quay", tellerOf))
                    {
                        prices.Add($"{GameData.Ports[snap.port].name} ({Age(w.Day - snap.day)})");
                        Known.HearOf(GameData.Ports[snap.port]);
                    }
                }
            if (prices.Count > 0)
                lines.Add($"Prices heard for {prices.Count} {(prices.Count == 1 ? "place" : "places")}, now in your ledger: " +
                          string.Join(", ", prices.Take(8)) + (prices.Count > 8 ? $" and {prices.Count - 8} more" : "") + ".");
            return lines.Count == 0 ? "Nobody here has news you haven't heard." : string.Join("\n", lines);
        }

        /// <summary>An older saved game's news heard (ids only): what the waterfronts still hold comes back into Known.</summary>
        public static void RestoreOld(List<object> data)
        {
            var game = KapmanGame.Current;
            if (data != null && game != null)
                Known.FromOldSave(data, game.World, game.Day);
        }

        /// <summary>An errand of the place: the facts of it. `toTown`: it's done in the town's market (the minster's
        /// trade), so the graph takes you there.</summary>
        public static string Errand(PortDef port, string key, out bool toTown)
        {
            toTown = false;
            var game = KapmanGame.Current;
            if (port == null || !Trade.Started)
                return "";
            int year = game != null ? game.Year : 793;
            double today = game != null ? game.Day : 0.0;
            var rules = TradeRules.Current;
            var zone = rules.ZoneOf(port.region);
            switch (key)
            {
                case "port.tollhouse":
                {
                    var lines = new List<string> { zone != null && zone.toll > 0f ? $"The toll here: {zone.toll * 100f:0}% of what you sell." : "No toll is taken here." };
                    foreach (var (good, _) in Trade.Hold.Contents)
                        foreach (string note in Trade.LawNotes(good, zone, year))
                            lines.Add($"On your {good.name.ToLowerInvariant()}: {note}");
                    lines.Add(Reputation.Line(port));
                    if (Trade.Debt > 0.0)
                        lines.Add($"You owe {Creditor()} {zone?.Show(Trade.Debt) ?? Trade.Debt.ToString("0") + " d"}" +
                                  (Trade.DebtDue > 0.0 ? $", due in {Math.Max(0.0, Trade.DebtDaysLeft(today)):0} days." : "."));
                    return string.Join("\n", lines);
                }
                case "port.moneyer":
                    return Exchange(zone, rules);
                case "port.host":
                    return Pay(zone, rules, Voyage.Men * 1.0, "You find lodging ashore for the crew. They rest.", 0.15, 0.15);
                case "port.guesthouse":
                    return Pay(zone, rules, Voyage.Men * 0.5, "You stay at the guest house. The crew rest.", 0.15, 0.1);
                case "port.church":
                {
                    double alms = Math.Max(2.0, Math.Round(Trade.Purse.Silver(rules) * 0.02));
                    string fact = Pay(zone, rules, alms, $"You give {alms:0} deniers' worth in alms at the church.", 0.0, 0.05);
                    if (!fact.StartsWith("You haven't"))
                        Deeds.Record("gave_alms", "gave alms at the church", port);
                    return fact;
                }
                case "port.minster":
                    toTown = true;
                    return "The monastery trades through its market.";
                default:
                    return "Not built yet: it comes with its system.";
            }
        }

        /// <summary>Every coin that isn't this zone's own changed into it, at the zone's loss.</summary>
        static string Exchange(ZoneDef zone, TradeRules rules)
        {
            var local = zone != null ? rules.Coin(zone.coin) : null;
            if (local == null || local.Deniers <= 0.0)
                return "Nobody here changes money.";
            double deniers = 0.0;
            var given = new List<string>();
            foreach (var kv in Trade.Purse.All.ToList())
            {
                if (kv.Key == zone.coin)
                    continue;
                var c = rules.Coin(kv.Key);
                if (c == null)
                    continue;
                deniers += kv.Value * c.Deniers;
                given.Add(Purse.Amount(kv.Key, kv.Value, rules));
                Trade.Purse.Add(kv.Key, -kv.Value);
            }
            if (given.Count == 0)
                return "You have no foreign coin to change.";
            double back = deniers * (1.0 - zone.foreignLoss);
            Trade.Purse.Add(zone.coin, back / local.Deniers);
            return $"You change {string.Join(" and ", given)} into {Purse.Amount(zone.coin, back / local.Deniers, rules)} " +
                   $"(the moneyer keeps {deniers - back:0.#} deniers' worth).";
        }

        static string Pay(ZoneDef zone, TradeRules rules, double deniers, string done, double health, double heart)
        {
            if (!Trade.Purse.Pay(deniers, zone, rules, out string how))
                return $"You haven't the {zone?.Show(deniers) ?? deniers.ToString("0") + " d"} it costs.";
            Voyage.Rest(health, heart);
            return done + (string.IsNullOrEmpty(how) ? "" : $" Paid {how}.");
        }

        /// <summary>Who the debt is owed to: your writing for the creditor's key, or "the count" until you write it.</summary>
        static string Creditor()
        {
            return Frame.FrameGame.CreditorName();
        }

        static string Age(double days) => days < 1.0 ? "today" : days < 2.0 ? "yesterday" : $"{days:0} days ago";

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);

    }
}
