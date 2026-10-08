using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>
    /// One card's rule of 7 Oct 2026 (Game/Data/card_rules_793.json, written by Bake/content/card_rules.py): how long
    /// it waits unanswered while the clock runs, whether it holds the ship meanwhile, and the options it then takes by
    /// itself, in the order to try them (by their keys: a key outlives a card being edited, a number does not).
    /// </summary>
    public sealed class CardRule
    {
        public string id = "";
        /// <summary>A raider, a squall, a man overboard: its time is hours, not a week.</summary>
        public bool pressing;
        /// <summary>She makes no way until it is answered or has answered itself.</summary>
        public bool holdsShip;
        public double patienceHours = CardRules.OrdinaryHours;
        public readonly List<string> take = new List<string>();
    }

    /// <summary>
    /// THE CARDS' RULES (the user, 7 Oct 2026, rulings 10 and 12). "Ordinary cards wait a week for it to automatically
    /// press if the player pressed play without interacting with it." A pressing card "will just keep them there until
    /// it automatically presses an option". A card with no row is ordinary.
    /// </summary>
    public sealed class CardRules
    {
        /// <summary>An ordinary card waits seven game days of running clock.</summary>
        public const double OrdinaryHours = 168.0;

        readonly Dictionary<string, CardRule> byId = new Dictionary<string, CardRule>();
        /// <summary>What an ordinary card waits, as the file has it.</summary>
        public double ordinaryHours = OrdinaryHours;

        public int Count => byId.Count;

        public bool Has(string id) => id != null && byId.ContainsKey(id);

        /// <summary>A card's rule; a card with no row is ordinary: a week, the ship not held, its own usual answer.</summary>
        public CardRule For(string id) => id != null && byId.TryGetValue(id, out var r) ? r : new CardRule { id = id ?? "", patienceHours = ordinaryHours };

        public IEnumerable<CardRule> All => byId.Values;

        public static CardRules Read(string json)
        {
            var rules = new CardRules();
            if (string.IsNullOrEmpty(json))
                return rules;
            object root;
            try
            {
                root = Json.Parse(json);
            }
            catch (Exception)
            {
                return rules;
            }
            if (!(root is Dictionary<string, object> d))
                return rules;
            rules.ordinaryHours = Math.Max(1.0, Json.Num(d, "ordinary_hours", OrdinaryHours));
            foreach (var x in Json.Arr(d, "cards") ?? new List<object>())
            {
                if (!(x is Dictionary<string, object> row))
                    continue;
                var r = new CardRule { id = Json.Str(row, "id"), pressing = Json.Bool(row, "pressing") };
                // (only a pressing card has a time of its own, and only a pressing card can hold her)
                r.holdsShip = r.pressing && Json.Bool(row, "holds_ship");
                r.patienceHours = r.pressing ? Math.Max(1.0, Json.Num(row, "hours", 6.0)) : rules.ordinaryHours;
                foreach (var k in Json.Arr(row, "take") ?? new List<object>())
                    if (k is string key && key.Length > 0)
                        r.take.Add(key);
                if (r.id.Length > 0)
                    rules.byId[r.id] = r;
            }
            return rules;
        }
    }
}
