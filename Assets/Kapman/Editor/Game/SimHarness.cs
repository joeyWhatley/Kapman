// ported 4 Oct: from the first build's Editor/Game/SimHarness.cs, unchanged in what it does. The world simulation it drives
// (WorldSim, SimData, SimRoutes, the rules) came over byte for byte; WeatherModel now lives with the sim
// (Code/Game/Sim/WeatherModel.cs) under the same namespace, Kapman.Weather, so the usings hold. Nothing here touched the
// first build's land or water (LandStreamer, BakedLand), so nothing was dropped.
using System;
using System.Diagnostics;
using System.IO;
using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Weather;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE TEST RUN - the world's traders sail a year with nobody watching (no scene, no 3D), as fast as the computer goes,
    /// and the numbers are written as spreadsheets: every voyage, every trader's year, a dozen goods' prices at every port
    /// day by day, and a short summary. The user: "Can you test run the simulated ai traders".
    /// Menu: Kapman ▸ Sim ▸ Run a year (headless). Output: G:/kapman_unity/data/work/sim/(date)_seed(seed)/.
    /// </summary>
    public static class SimHarness
    {
        public const string OutRoot = "G:/kapman_unity/data/work/sim";

        [MenuItem("Kapman/Sim/Run a year (headless)", priority = 40)]
        static void RunYearMenu()
        {
            string summary = RunYear(793, 365, out _);
            UnityEngine.Debug.Log(summary);
            EditorUtility.RevealInFinder(OutRoot);
        }

        /// <summary>Ten years from 21 June 793, the prices kept weekly; the report opens in the browser (a few minutes).</summary>
        [MenuItem("Kapman/Sim/Run 10 years (headless)", priority = 41)]
        static void RunTenYearsMenu()
        {
            if (!EditorUtility.DisplayDialog("Run 10 years", "The world runs ten years with nobody watching. It takes a few minutes, " +
                                             "and the editor waits till it's done. The report opens in your browser.", "Run", "Cancel"))
                return;
            string summary = RunYear(793, 3653, out _, true, 1.0, 7);
            UnityEngine.Debug.Log(summary);
            OpenReport(summary);
        }

        /// <summary>The report named in a run's summary, opened in the browser.</summary>
        static void OpenReport(string summary)
        {
            int at = summary.LastIndexOf("Folder: ", StringComparison.Ordinal);
            if (at < 0)
                return;
            string report = Path.Combine(summary.Substring(at + 8).Trim(), "report.html");
            if (File.Exists(report))
                Application.OpenURL("file:///" + report.Replace('\\', '/'));
        }

        /// <summary>Loads the tables, opens every market, runs `days` game days from 21 June 793, writes the numbers.</summary>
        public static string RunYear(int seed, int days, out WorldSim sim, bool write = true) => RunYear(seed, days, out sim, write, 1.0);

        /// <summary>As RunYear, with the raiders' pressure dial (1 = history; tests use 5), and the days between rows of
        /// prices_daily.csv (a long run keeps every 7th).</summary>
        public static string RunYear(int seed, int days, out WorldSim sim, bool write, double raidPressure, int csvEvery = 1)
        {
            var run = MakeWorld(seed, raidPressure);
            run.stats.csvEvery = csvEvery;
            sim = run;
            Action<DealInfo> count = d =>
            {
                if (d.who == "player")
                    return;
                run.stats.deals++;
                run.stats.dealtDeniers += d.deniers;
            };
            var clock = Stopwatch.StartNew();
            Deal.Dealt += count;
            try
            {
                run.Populate();
                for (int h = 0; h < days * 24; h++)
                    run.Hour();
            }
            finally
            {
                Deal.Dealt -= count;
            }
            string summary = "";
            if (write)
            {
                string folder = Path.Combine(OutRoot, DateTime.Now.ToString("yyyy-MM-dd_HHmm") + "_seed" + seed);
                summary = run.stats.Write(run, folder, clock.Elapsed.TotalSeconds);
                SimReport.Write(run, summary, folder);
                summary += "Report: report.html (open it in a browser).\nFolder: " + folder;
            }
            Markets.Clear();
            return summary;
        }

        /// <summary>A world as the game makes it, headless: the tables loaded, every market open (fresh), not yet
        /// populated. Made as KapmanGame.MakeWorld makes the game's: the same constructor and rules, the lanes from
        /// SimRoutes.DefaultLanes (the bake's lanes.bin).</summary>
        public static WorldSim MakeWorld(int seed, double raidPressure = 1.0) => MakeWorldAt(seed, 172, 14.0, raidPressure);

        /// <summary>The same for a game whose day 0 is `startDay` of 793, the world made as of `startHours` (below 0: so
        /// many hours before the clock's start, as the game's pre-run makes it: KapmanGame.MakeWorld). The economy's
        /// calendar is set to that start; a test puts it back (Seasons.SetStart(793, 172)).</summary>
        public static WorldSim MakeWorldAt(int seed, int startDay, double startHours, double raidPressure = 1.0)
        {
            GameData.LoadGoods(Load("goods_793.json"));
            GameData.LoadPorts(Load("ports_793.json"));
            TradeRules.Load(Load("trade_rules.json"));
            var data = SimData.Load(Load("ship_classes_793.json"), Load("traffic_793.json"));
            if (startDay != 172 || Seasons.StartDay != 172 || Seasons.StartYear != 793)
                Seasons.SetStart(793, startDay);
            Markets.Clear();
            Markets.OpenAll(793, startHours < 0.0 ? startHours / 24.0 : 0.0);
            Patrols.Rules = PatrolRules.Load(Load("patrols_793.json"));
            var raiders = RaiderRules.Load(Load("raiders_793.json"));
            raiders.pressureX = raidPressure;
            var run = new WorldSim(data, new SimRules(), SimRoutes.Load(SimRoutes.DefaultLanes), new WeatherModel(793), seed, 793, startDay, startHours,
                                   SeamanshipRules.Load(Load("seamanship_793.json")), raiders);
            run.fairs = Fairs.Load(Load("fairs_793.json"));
            return run;
        }

        static TextAsset Load(string file) => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/" + file);
    }
}
