using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CONTENT AS IT STANDS (Design/wiring_plan.md): one test that reads every data file the way the game's loaders
    /// will and writes down what it found, in the log and in Logs/content_report.txt: which files parse; how many of the
    /// story files' conditions, costs and effects the cards' engine understands (and the first that it does not); whether
    /// the winter's cards load beside the others; how long the gazetteer takes. It fails only if it cannot make the report
    /// at all: the writers are still at work, so a half-written file is a line in the report, not a failed test.
    /// </summary>
    public class ContentReportTests
    {
        sealed class Tally
        {
            public int conditions, conditionFaults, costs, costFaults, effects, effectFaults;
            public readonly List<string> samples = new List<string>();

            public void Sample(string line)
            {
                if (samples.Count < 6)
                    samples.Add(line.Length <= 150 ? line : line.Substring(0, 147) + "...");
            }
        }

        [Test]
        public void TheContentAsItStands()
        {
            ContentFiles.SetRoot(null);
            ContentRules.Use(null);
            var report = new StringBuilder();
            report.AppendLine("KAPMAN CONTENT REPORT, " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            report.AppendLine("data folder: " + ContentFiles.Root);

            // ---- A. every file, parsed
            report.AppendLine();
            report.AppendLine("A. FILES THAT PARSE (as a JSON object)");
            var parsed = new List<KeyValuePair<string, Dictionary<string, object>>>();
            foreach (string folder in new[] { "", "encounters", "places", "story", "story/settlements", "story/wars", "story/hinges", "writing" })
            {
                if (!ContentFiles.FolderExists(folder))
                {
                    report.AppendLine($"  {(folder.Length > 0 ? folder : "(root)"),-20} not there yet");
                    continue;
                }
                var files = ContentFiles.List(folder);
                var bad = new List<string>();
                var watch = Stopwatch.StartNew();
                foreach (string file in files)
                {
                    var root = ContentFiles.ParseObject(file);
                    if (root == null)
                        bad.Add(file);
                    else if (file != "gazetteer_793.json")
                        parsed.Add(new KeyValuePair<string, Dictionary<string, object>>(file, root));
                }
                report.AppendLine($"  {(folder.Length > 0 ? folder : "(root)"),-20} {files.Length - bad.Count} of {files.Length} files, {watch.ElapsedMilliseconds} ms" +
                                  (bad.Count > 0 ? "   DO NOT PARSE: " + string.Join(", ", bad) : ""));
            }

            // ---- B. the story and places files' conditions, costs and effects, through the cards' engine
            report.AppendLine();
            report.AppendLine("B. CONDITIONS, COSTS AND EFFECTS THE ENGINE UNDERSTANDS (story/ and places/)");
            foreach (var kv in parsed)
            {
                if (!kv.Key.StartsWith("story/", StringComparison.Ordinal) && !kv.Key.StartsWith("places/", StringComparison.Ordinal))
                    continue;
                var tally = new Tally();
                Walk(kv.Value, "", kv.Key, tally);
                if (tally.conditions + tally.costs + tally.effects == 0)
                    continue;
                bool faults = tally.conditionFaults + tally.costFaults + tally.effectFaults > 0;
                if (!faults && kv.Key.StartsWith("places/", StringComparison.Ordinal))
                    continue;                                             // (fifty clean lines say nothing)
                report.AppendLine($"  {kv.Key}: conditions {tally.conditions - tally.conditionFaults}/{tally.conditions}, " +
                                  $"costs {tally.costs - tally.costFaults}/{tally.costs}, effect lists {tally.effects - tally.effectFaults}/{tally.effects}");
                foreach (string sample in tally.samples)
                    report.AppendLine("      " + sample);
            }

            // ---- C. the winter's cards beside the others
            report.AppendLine();
            report.AppendLine("C. THE CARDS, WITH THE WINTER'S (story/winter_793.json \"cards\") ADDED");
            try
            {
                var src = new EncounterSources
                {
                    roles = ContentFiles.Read("encounters/npc_roles_793.json"),
                    names = ContentFiles.Read("encounters/names_placeholder.json"),
                    reputation = ContentFiles.Read("reputation_793.json"),
                    knowing = ContentFiles.Read("knowing_793.json"),
                    writing = ContentFiles.Read("writing.json"),
                    goods = ContentFiles.Read("goods_793.json"),
                    ports = ContentFiles.Read("ports_793.json"),
                    traffic = ContentFiles.Read("traffic_793.json"),
                };
                foreach (string file in ContentFiles.List("encounters"))
                {
                    string name = Path.GetFileName(file);
                    if (name != "npc_roles_793.json" && name != "names_placeholder.json")
                        src.cards.Add((file, ContentFiles.Read(file) ?? ""));
                }
                var watch = Stopwatch.StartNew();
                var plain = EncounterLoader.Load(src);
                long plainMs = watch.ElapsedMilliseconds;
                report.AppendLine($"  as the game loads them today: {plain.cards.Count} cards from {src.cards.Count} files, {plain.errors.Count} faults, " +
                                  $"{plain.missingKeys.Count} writing keys missing, {plainMs} ms");
                foreach (string e in plain.errors.Take(6))
                    report.AppendLine("      " + e);
                var chains = new SortedSet<string>(StringComparer.Ordinal);
                foreach (var c in plain.cards)
                {
                    string[] parts = c.id.Split('.');
                    if (parts.Length == 3 && parts[1].StartsWith("chain_", StringComparison.Ordinal))
                        chains.Add(parts[1].Substring(6));
                }
                report.AppendLine($"  chains among them (card ids <stage>.chain_<id>.<step>): {chains.Count}");
                string winter = ContentFiles.Exists("story/winter_793.json") ? ContentFiles.Read("story/winter_793.json") : null;
                if (winter != null)
                {
                    src.cards.Add(("story/winter_793.json", winter));
                    var with = EncounterLoader.Load(src);
                    var added = with.errors.Where(e => !plain.errors.Contains(e)).ToList();
                    report.AppendLine($"  with the winter's cards: {with.cards.Count} cards, {added.Count} new faults");
                    foreach (string e in added.Take(10))
                        report.AppendLine("      " + e);
                }
                else
                    report.AppendLine("  story/winter_793.json is not there");
            }
            catch (Exception e)
            {
                report.AppendLine("  could not be done: " + e.GetType().Name + ": " + e.Message);
            }

            // ---- D. the gazetteer
            report.AppendLine();
            report.AppendLine("D. THE GAZETTEER");
            if (ContentFiles.Exists("gazetteer_793.json"))
            {
                long before = GC.GetTotalMemory(true);
                var watch = Stopwatch.StartNew();
                string text = ContentFiles.Read("gazetteer_793.json");
                long readMs = watch.ElapsedMilliseconds;
                var gaz = ContentFiles.ParseText(text, "gazetteer_793.json");
                long parseMs = watch.ElapsedMilliseconds - readMs;
                int places = ContentJson.Rows(gaz, "places").Count(), stones = ContentJson.Rows(gaz, "stones").Count();
                var tiers = new int[5];
                var kinds = new SortedDictionary<string, int>(StringComparer.Ordinal);
                foreach (var p in ContentJson.Rows(gaz, "places"))
                {
                    tiers[Mathf.Clamp(ContentJson.Int(p, "tier"), 0, 4)]++;
                    string kind = Json.Str(p, "kind");
                    kinds[kind] = kinds.TryGetValue(kind, out int n) ? n + 1 : 1;
                }
                long after = GC.GetTotalMemory(false);
                report.AppendLine($"  {(text != null ? text.Length : 0) / 1024} KB read in {readMs} ms, parsed in {parseMs} ms, about {(after - before) / (1024 * 1024)} MB held while parsed");
                report.AppendLine($"  {places} places (tier 1: {tiers[1]}, tier 2: {tiers[2]}, tier 3: {tiers[3]}, other: {tiers[0] + tiers[4]}) and {stones} stones");
                report.AppendLine("  kinds: " + string.Join(", ", kinds.Select(k => k.Key + " " + k.Value)));
            }
            else
                report.AppendLine("  gazetteer_793.json is not there");
            report.AppendLine("  routes files (routes_*.json in the data folder or story/): " +
                              (ContentFiles.List("", "routes_*.json").Length + (ContentFiles.FolderExists("story") ? ContentFiles.List("story", "routes_*.json").Length : 0)));

            report.AppendLine();
            report.AppendLine($"warnings given while reading: {ContentFiles.Warnings.Count}");

            string all = report.ToString();
            UnityEngine.Debug.Log(all);
            try
            {
                string logs = Path.Combine(Application.dataPath, "..", "Logs");
                Directory.CreateDirectory(logs);
                File.WriteAllText(Path.Combine(logs, "content_report.txt"), all, Encoding.UTF8);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Kapman content: the report could not be written to Logs/content_report.txt: " + e.Message);
            }
            ContentFiles.SetRoot(null);                                   // (forget this run's warnings)
            Assert.Greater(all.Length, 200, "a report was made");
        }

        /// <summary>Every "when", "check", "turned" and "cond" that is text, every "cost", and every list of effects, wherever
        /// in a file they are, asked of the engine as a loader would. (In sayings_793.json "when" is prose for a person and
        /// "cond" is the condition, so that file's count of faults is its prose.)</summary>
        static void Walk(object node, string key, string where, Tally tally)
        {
            if (node is Dictionary<string, object> d)
            {
                foreach (var kv in d)
                {
                    if (kv.Key == "fields" || kv.Key == "units" || kv.Key == "about")
                        continue;                                         // (the files' own notes on their fields)
                    string here = where + (Json.Str(d, "id").Length > 0 ? " " + Json.Str(d, "id") : "");
                    if ((kv.Key == "when" || kv.Key == "check" || kv.Key == "turned" || kv.Key == "cond") && kv.Value is string cond)
                    {
                        tally.conditions++;
                        if (!ContentRules.TryParseWhen(cond, out _))
                        {
                            tally.conditionFaults++;
                            tally.Sample($"{kv.Key}: {cond}");
                        }
                    }
                    else if (kv.Key == "cost" && (kv.Value is string || kv.Value is List<object>))
                    {
                        tally.costs++;
                        if (!ContentRules.ParseCost(kv.Value, here, out _))
                        {
                            tally.costFaults++;
                            tally.Sample("cost: " + (kv.Value as string ?? Json.Write(kv.Value)));
                        }
                    }
                    else if ((kv.Key == "effects" || kv.Key == "success" || kv.Key == "failure" || kv.Key == "does") && kv.Value is List<object> list)
                    {
                        if (list.Count == 0 || !list.All(x => x is string || x is Dictionary<string, object>))
                            continue;
                        tally.effects++;
                        if (!ContentRules.ParseEffects(list, here, out _))
                        {
                            tally.effectFaults++;
                            tally.Sample("effects: " + Json.Write(list));
                        }
                    }
                    else
                        Walk(kv.Value, kv.Key, here, tally);
                }
            }
            else if (node is List<object> items)
                foreach (object item in items)
                    Walk(item, key, where, tally);
        }
    }
}
