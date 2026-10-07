using System;
using System.Collections.Generic;

namespace Kapman.Encounters
{
    /// <summary>
    /// THE VARIABLES a card reads (VarNames): the encounter layer's own (flags, the port's id and calls, the people's
    /// feelings and memories, the best feeling at a port) worked out here from its state and registry, and the rest from the live game,
    /// which the runner puts in <see cref="nums"/> and <see cref="texts"/> before a card is drawn or opened, or answers
    /// through <see cref="numSource"/> and <see cref="textSource"/> for the families (hold.&lt;good&gt;, text.&lt;id&gt; ...).
    /// A test fills the two tables by hand.
    /// </summary>
    public sealed class EncounterVars : IVars
    {
        public readonly Dictionary<string, double> nums = new Dictionary<string, double>(StringComparer.Ordinal);
        public readonly Dictionary<string, string> texts = new Dictionary<string, string>(StringComparer.Ordinal);
        /// <summary>The live game's answer for a number not in the table (null: 0).</summary>
        public Func<string, double> numSource;
        /// <summary>The live game's answer for a text not in the table (null: "").</summary>
        public Func<string, string> textSource;

        public EncounterState state;
        public NpcRegistry npcs;
        public string merchant = "player";
        /// <summary>The port the card is at ("" at sea).</summary>
        public string port = "";
        /// <summary>Game days since the start (flags lapse by it).</summary>
        public double day;
        /// <summary>The card's people by actor key (for npc.&lt;role&gt;.x); empty while drawing.</summary>
        public Dictionary<string, Npc> actors = new Dictionary<string, Npc>();

        public double Num(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0.0;
            if (name.StartsWith("flag.", StringComparison.Ordinal))
                return state != null ? state.Flag(name.Substring(5), day) : 0.0;
            if (name == "port.visits")
                return state != null ? state.VisitsBefore(port) : 0.0;
            if (name == "port.best_feeling")
                return npcs != null ? npcs.BestFeelingAt(port, merchant) : -100.0;
            if (name == "generation")
                return npcs != null ? npcs.Generation(merchant) : 0.0;
            string role = VarNames.RoleOf(name);
            if (role != null)
            {
                var who = actors.TryGetValue(role, out var n) ? n : null;
                if (who == null || npcs == null)
                    return 0.0;
                if (name.EndsWith(".feeling", StringComparison.Ordinal))
                    return npcs.Feeling(who.id, merchant);
                if (name.EndsWith(".met", StringComparison.Ordinal))
                    return npcs.Met(who.id, merchant) ? 1.0 : 0.0;
                if (name.EndsWith(".owed", StringComparison.Ordinal))
                    return npcs.Rel(who.id, merchant).owed;
                if (name.EndsWith(".generation", StringComparison.Ordinal))
                    return who.generation;
                return 0.0;
            }
            if (nums.TryGetValue(name, out double v))
                return v;
            return numSource != null ? numSource(name) : 0.0;
        }

        public string Text(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            if (name == "port.id")
                return port ?? "";                                       // the port she lies at ("" at sea), as in ports_793.json
            string role = VarNames.RoleOf(name);
            if (role != null)
            {
                var who = actors.TryGetValue(role, out var n) ? n : null;
                return who != null && npcs != null && name.EndsWith(".last", StringComparison.Ordinal) ? npcs.LastOption(who.id, merchant) : "";
            }
            if (texts.TryGetValue(name, out string s))
                return s ?? "";
            return textSource != null ? textSource(name) ?? "" : "";
        }
    }
}
