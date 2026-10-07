// 7 Oct 2026: THE RULES OF A DATA ROW. The story files write their conditions ("when", "check", "turned") in the grammar
// of Core/Encounters/Cond.cs and their costs and effects in the verbs of Core/Encounters/Effects.cs (SCHEMAS.md rules 6
// and 7; town_actions_793.json; unlocks_793.json). So a town act, an unlock's price, a job, a hinge and a card all run
// on one engine. This is the one door to that engine for everything that is not a card: parse a row's condition, cost
// and effects when the file loads (a fault is one warning and the row is shut, never an exception), ask whether a
// condition holds now, say why not in plain words, and run a cost and effects on the live game.
using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Encounters;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>What came of running a data row (ContentRules.Run): done, with the plain facts of what happened; or not,
    /// with why in plain words (the condition's rule, or what the merchant lacks).</summary>
    public sealed class ContentResult
    {
        public bool ok;
        /// <summary>Why it was not done ("" when it was).</summary>
        public string why = "";
        /// <summary>What happened, in order ("Paid 8 d.", "The crew's heart rises.").</summary>
        public readonly List<string> facts = new List<string>();
        public string Text => string.Join(" ", facts);
    }

    /// <summary>
    /// The encounter engine for the content outside the cards. The live game's director is EncounterRunner.Director; a
    /// test hands in one of its own with <see cref="Use"/> (ContentFlags then reads that one's flags too). With no game
    /// running, conditions are asked of a world where every number is 0 and every text is empty, and nothing can be run.
    /// </summary>
    public static class ContentRules
    {
        static EncounterDirector testDirector;
        static WordTable ownWords;
        static int ownWordsVersion;
        static readonly EncounterVars nothing = new EncounterVars();
        static Cond never;

        /// <summary>A test's own director (null: the live game's again).</summary>
        public static void Use(EncounterDirector director) => testDirector = director;

        /// <summary>The encounter layer the content runs on (null: no game is running, or its cards have faults).</summary>
        public static EncounterDirector Director => testDirector ?? EncounterRunner.Director;

        /// <summary>Is there a game to run on?</summary>
        public static bool Ready => Director != null;

        /// <summary>
        /// The words a condition may compare with (standing >= known, npc.reeve.feeling >= friendly): the game's own
        /// tables. With no game running they are read from the same three files the cards' loader reads, so a loader can
        /// parse its conditions before the game starts and in a test.
        /// </summary>
        public static WordTable Words
        {
            get
            {
                var d = Director;
                if (d != null)
                    return d.book.words;
                if (EncounterRunner.Book != null)
                    return EncounterRunner.Book.words;
                if (ownWords == null || ownWordsVersion != ContentFiles.Version)
                {
                    ownWordsVersion = ContentFiles.Version;
                    var errors = new List<string>();
                    ownWords = WordTable.FromData(Table("encounters/npc_roles_793.json"), Table("reputation_793.json"),
                                                  Table("knowing_793.json"), errors);
                    if (errors.Count > 0)
                        ContentFiles.Warn("words", "the words of feeling, standing and odds are not all there (" + string.Join(" ", errors) +
                                                   "): a condition that uses one will not parse");
                }
                return ownWords;
            }
        }

        /// <summary>A shared table: the root's own if it has one (a test may bring its own words), else the project's.</summary>
        static string Table(string file) => ContentFiles.Exists(file) ? ContentFiles.Read(file) : ContentFiles.ReadDefault(file);

        // ------------------------------------------------------------------ parsing, when a file loads

        /// <summary>A condition that never holds: what a condition that does not parse becomes.</summary>
        public static Cond Never => never ?? (never = Cond.Parse("false", null));

        /// <summary>
        /// A row's condition, parsed once. Nothing, "" or "true" always holds. A condition that does not parse (a name
        /// Cond.cs does not know, a line of plain prose) gives one warning that names the row, and NEVER holds: a faulty
        /// row is shut, not open to all. `where` names the row for the warning ("story/town_actions_793.json act repair").
        /// </summary>
        public static Cond ParseWhen(string text, string where)
        {
            if (TryParseWhen(text, out Cond cond, out string fault))
                return cond;
            ContentFiles.Warn("when:" + where + ":" + text, $"{where}: the condition '{Short(text)}' does not parse ({fault}): it never holds");
            return Never;
        }

        /// <summary>As ParseWhen without the warning: false for text that is not a condition (some files keep a line of
        /// plain words under "when", for a person to read: the winter reckoning's years).</summary>
        public static bool TryParseWhen(string text, out Cond cond) => TryParseWhen(text, out cond, out _);

        static bool TryParseWhen(string text, out Cond cond, out string fault)
        {
            fault = "";
            cond = Cond.True;
            if (string.IsNullOrWhiteSpace(text))
                return true;
            try
            {
                cond = Cond.Parse(text, Words);
                return true;
            }
            catch (Exception e)
            {
                cond = Never;
                fault = e.Message;
                return false;
            }
        }

        /// <summary>
        /// A row's effects (a list of verb strings and chance objects, or one), parsed once by Effects.cs. True when every
        /// one parsed; each that did not gives one warning and is left out. `effects` is never null.
        /// </summary>
        public static bool ParseEffects(object node, string where, out List<Effect> effects)
        {
            var errors = new List<string>();
            try
            {
                effects = Kapman.Encounters.Effects.ParseList(node, errors, where);
            }
            catch (Exception e)
            {
                effects = new List<Effect>();
                errors.Add($"{where}: {e.Message}");
            }
            foreach (string error in errors)
                ContentFiles.Warn("effect:" + error, error);
            return errors.Count == 0;
        }

        /// <summary>
        /// A row's cost ("silver 12", "goods lead 2", "men 1"; a list of such, or one), parsed once. Nothing, "" and
        /// "none" cost nothing. True when it was understood. FALSE (with one warning) for a cost written in plain words
        /// ("silver 400 to 1200, by the port"): the caller must then keep the thing shut, never give it for nothing.
        /// `cost` is never null.
        /// </summary>
        public static bool ParseCost(object node, string where, out List<Effect> cost)
        {
            cost = new List<Effect>();
            if (node == null)
                return true;
            var parts = new List<object>();
            foreach (object part in node as List<object> ?? new List<object> { node })
            {
                if (part is string s && (s.Trim().Length == 0 || string.Equals(s.Trim(), "none", StringComparison.OrdinalIgnoreCase)))
                    continue;
                parts.Add(part);
            }
            if (parts.Count == 0)
                return true;
            var errors = new List<string>();
            try
            {
                cost = Kapman.Encounters.Effects.ParseCost(parts, errors, where);
            }
            catch (Exception e)
            {
                errors.Add($"{where}: {e.Message}");
            }
            foreach (string error in errors)
                ContentFiles.Warn("cost:" + error, error);
            return errors.Count == 0;
        }

        // ------------------------------------------------------------------ asking, in play

        /// <summary>The variables a condition reads, as they stand now: the live game's, at the port she lies at.</summary>
        public static IVars Vars
        {
            get
            {
                var d = Director;
                return d != null ? d.VarsNow() : nothing;
            }
        }

        /// <summary>Does the condition hold now? (None at all holds.)</summary>
        public static bool Holds(Cond when)
        {
            if (when == null)
                return true;
            try
            {
                return when.Eval(Vars);
            }
            catch (Exception e)
            {
                ContentFiles.Warn("eval:" + when.Source, $"the condition '{Short(when.Source)}' could not be asked ({e.GetType().Name}: {e.Message}): taken as not so");
                return false;
            }
        }

        /// <summary>The condition as a rule in plain words, for a thing that is shut ("Your standing here must be known or
        /// better."); "" for none.</summary>
        public static string Why(Cond when) => when == null ? "" : when.Describe(Words);

        /// <summary>What of a cost the merchant has not got, in plain words ("You need 24 d of silver."), or null when she
        /// can pay it all.</summary>
        public static string Lacks(List<Effect> cost)
        {
            if (cost == null || cost.Count == 0)
                return null;
            var vars = Vars;
            foreach (var e in cost)
                foreach (var v in e.Verbs())
                {
                    if (v.n >= 0.0)
                        continue;
                    double need = -v.n;
                    switch (v.verb)
                    {
                        case "silver":
                            if (vars.Num("silver") < need)
                                return $"You need {Amount(need)} d of silver.";
                            break;
                        case "goods":
                            if (vars.Num("hold." + v.a) < need)
                            {
                                var good = GameData.Good(v.a);
                                string name = good != null && !string.IsNullOrEmpty(good.name) ? good.name : v.a.Replace('_', ' ');
                                return $"You need {Amount(need)} of {name} aboard.";
                            }
                            break;
                        case "men":
                            if (vars.Num("crew") < need + 1.0)
                                return need <= 1.0 ? "You have no man to spare." : $"You have not {Amount(need)} men to spare.";
                            break;
                    }
                }
            return null;
        }

        /// <summary>
        /// Does a data row: if its condition holds and its cost can be paid, the cost is paid and the effects run on the
        /// live game (EncounterDirector.RunEffects: the same engine and the same people as a card), and the result holds
        /// the facts. Otherwise nothing is done and the result says why in plain words. `id` is the row's own id (the
        /// act's, the job's): what the people remember it by, and the seed of its dice.
        /// </summary>
        public static ContentResult Run(string id, Cond when, List<Effect> cost, List<Effect> effects)
        {
            var result = new ContentResult();
            var d = Director;
            if (d == null)
            {
                result.why = "The game is not ready.";
                return result;
            }
            if (!Holds(when))
            {
                result.why = Why(when);
                if (result.why.Length == 0)
                    result.why = "Not now.";
                return result;
            }
            string lacks = Lacks(cost);
            if (lacks != null)
            {
                result.why = lacks;
                return result;
            }
            try
            {
                result.facts.AddRange(d.RunEffects(id, cost, effects));
                result.ok = true;
            }
            catch (Exception e)
            {
                ContentFiles.Warn("run:" + id, $"{id}: its effects could not be run ({e.GetType().Name}: {e.Message})");
                result.why = "Nothing came of it.";
            }
            return result;
        }

        static string Amount(double n) => n.ToString("0.##", CultureInfo.InvariantCulture);

        static string Short(string text)
        {
            text = (text ?? "").Trim();
            return text.Length <= 80 ? text : text.Substring(0, 77) + "...";
        }

        // Enter Play Mode without a domain reload: no test's director is left behind
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            testDirector = null;
            ownWords = null;
        }
    }
}
