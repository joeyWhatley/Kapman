using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>One task of the farmed year in a region: its window on the Julian farm calendar, what it shows on the
    /// land, what is heard, where, and the change it makes in the supply of goods while it is on.</summary>
    public sealed class FarmTask
    {
        public string id = "", anchor = "";
        public int from, to;
        public List<string> show = new List<string>();
        public List<string> voice = new List<string>();
        public Dictionary<string, double> supply = new Dictionary<string, double>();
        public Dictionary<string, double> failSupply = new Dictionary<string, double>();
        /// <summary>The year-state die the task follows ("mast" for the pannage), or empty.</summary>
        public string stock = "";

        /// <summary>How far on the task is on a Julian day of the year (0..1, the window's shape).</summary>
        public double Strength(int dayOfYear) => Presence.Shape(from, to, dayOfYear);

        public override string ToString() => id;
    }

    /// <summary>
    /// THE FARM YEAR (farm_year.json; Design/wildlife_and_seasons.md 1.5, 2.1): per region, the tasks of the year -
    /// lambing, the cattle let out, the hay, the harvest, the pigs in the woods, the Martinmas slaughter, the eel traps,
    /// the salt fires. The builders' dressing, the audio pass and the economy's season curves (through Supply) read it.
    /// </summary>
    public sealed class FarmYear
    {
        public const string FileName = "farm_year.json";

        /// <summary>The largest seasonal swing of a good's supply either way (decision 16: 20-60%).</summary>
        public double maxSwing = 0.6;
        public readonly Dictionary<string, List<FarmTask>> byRegion = new Dictionary<string, List<FarmTask>>();

        public static FarmYear Load(string path) => Parse(File.ReadAllText(path));

        public static FarmYear Parse(string json)
        {
            var root = WildlifeJson.Root(json, FileName);
            var f = new FarmYear { maxSwing = Json.Num(root, "maxSwing", 0.6) };
            var regions = Json.Arr(root, "regions") ?? throw new FormatException(FileName + ": 'regions' is missing");
            foreach (var r in regions)
            {
                string region = WildlifeJson.Need(r, "region", FileName + " region");
                var tasks = new List<FarmTask>();
                foreach (var o in Json.Arr(r, "tasks") ?? new List<object>())
                {
                    string id = WildlifeJson.Need(o, "id", FileName + " " + region + " task");
                    var (from, to) = WildlifeJson.Window(o, "window", FileName + " " + region + " " + id);
                    tasks.Add(new FarmTask
                    {
                        id = id,
                        from = from,
                        to = to,
                        anchor = Json.Str(o, "anchor"),
                        show = WildlifeJson.Strings(o, "show"),
                        voice = WildlifeJson.Strings(o, "voice"),
                        supply = WildlifeJson.Numbers(o, "supply"),
                        failSupply = WildlifeJson.Numbers(o, "failSupply"),
                        stock = Json.Str(o, "stock"),
                    });
                }
                f.byRegion[region] = tasks;
            }
            return f;
        }

        /// <summary>Every task of a region (empty for a region with no farm year written yet).</summary>
        public List<FarmTask> All(string region) =>
            region != null && byRegion.TryGetValue(region, out var list) ? list : new List<FarmTask>();

        /// <summary>The tasks on in a region on a Julian day of the year, with their strengths (above 0).</summary>
        public List<(FarmTask task, double strength)> Tasks(string region, int dayOfYear)
        {
            var on = new List<(FarmTask, double)>();
            foreach (var t in All(region))
            {
                double s = t.Strength(dayOfYear);
                if (s > 0.0)
                    on.Add((t, s));
            }
            return on;
        }
    }
}
