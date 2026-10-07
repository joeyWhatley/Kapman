using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE FIRST MORNING, KEPT (7 Oct 2026). A new game makes the world as of 1 April and steps every ship of it to the
    /// player's first morning (KapmanGame.MakeWorld: the pre-run of ruling 14). That is thirty days of some hundred and
    /// forty ships, and takes seconds. One seed and one set of tables always give the same first morning, so the result
    /// is written once to the machine's cache folder and read the next time: the markets, the held shocks and the
    /// world's ships, as a saved game holds them.
    /// The key is everything the pre-run is made from: the tables' text, the lanes file, the seed, the start, the sim's
    /// rules and the code itself. Change any of them and the morning is reckoned again. The file is a cache: deleting it
    /// costs a few seconds once and changes nothing.
    /// </summary>
    public static class FirstMorning
    {
        const int Version = 1;
        /// <summary>How many kept mornings stay on disk (the newest).</summary>
        const int KeepFiles = 4;

        /// <summary>Never read or write the kept file (a test of the pre-run itself).</summary>
        public static bool Off;
        /// <summary>Another folder for the kept files (tests).</summary>
        public static string FolderOverride;
        /// <summary>The last TryRestore began to put a kept morning in and failed: the world and the markets are in no
        /// state to be used and must be made again.</summary>
        public static bool Spoiled { get; private set; }

        static string Folder => FolderOverride ?? Path.Combine(Application.temporaryCachePath, "kapman_first_morning");
        static string PathOf(string key) => Path.Combine(Folder, "first_morning_" + key + ".json.gz");

        /// <summary>The key of a game's first morning: a hash of all that the pre-run is made from.</summary>
        public static string Key(KapmanGame game, double toHours, int back)
        {
            var parts = new List<string>
            {
                "v" + Version, "seed " + game.worldSeed, "year " + game.clock.year, "day " + game.clock.startDay,
                "to " + toHours.ToString("R", System.Globalization.CultureInfo.InvariantCulture), "back " + back,
                "clear " + game.shipsKeepClear, "code " + CodeId(),
            };
            foreach (var table in new[] { game.goods, game.ports, game.tradeRules, game.chronicle, game.shocks, game.shipClasses, game.traffic,
                                          game.seamanship, game.raiders, game.fairs, GameFiles.Find("seasons_793") })
                parts.Add(table != null ? table.text : "");
            try
            {
                var lanes = new FileInfo(game.lanesFile ?? "");
                parts.Add(lanes.Exists ? lanes.FullName + " " + lanes.Length + " " + lanes.LastWriteTimeUtc.Ticks : "no lanes");
            }
            catch (Exception)
            {
                parts.Add("no lanes");
            }
            return Key(parts, new SimRules());
        }

        /// <summary>The code the world is stepped by: the sim's own assembly as compiled (a change of the code is a new
        /// number); where that cannot be asked, the build.</summary>
        static string CodeId()
        {
            try
            {
                return typeof(WorldSim).Module.ModuleVersionId.ToString();
            }
            catch (Exception)
            {
                return Application.version + " " + Application.buildGUID;
            }
        }

        /// <summary>A key from its parts and the sim's rules (every number of them).</summary>
        public static string Key(IEnumerable<string> parts, SimRules rules)
        {
            ulong h = 14695981039346656037UL;
            void Mix(string s)
            {
                foreach (char c in s ?? "")
                {
                    h ^= c;
                    h *= 1099511628211UL;
                }
                h ^= 0xFF;
                h *= 1099511628211UL;
            }
            foreach (string part in parts)
                Mix(part);
            foreach (var f in typeof(SimRules).GetFields(BindingFlags.Public | BindingFlags.Instance))
                Mix(f.Name + "=" + Convert.ToString(f.GetValue(rules), System.Globalization.CultureInfo.InvariantCulture));
            return h.ToString("x16");
        }

        /// <summary>
        /// Puts a kept first morning into the markets and the world just made, if there is one for this key and it reads
        /// whole. False: there is none (the caller steps the pre-run). If it began to put one in and failed,
        /// <see cref="Spoiled"/> is set and the caller must make the world again first.
        /// </summary>
        public static bool TryRestore(string key, WorldSim world, int year, double hours)
        {
            Spoiled = false;
            if (Off || string.IsNullOrEmpty(key) || world == null)
                return false;
            string path = PathOf(key);
            Dictionary<string, object> data;
            try
            {
                if (!File.Exists(path))
                    return false;
                using (var f = File.OpenRead(path))
                using (var gz = new GZipStream(f, CompressionMode.Decompress))
                using (var r = new StreamReader(gz, Encoding.UTF8))
                    data = Json.Parse(r.ReadToEnd()) as Dictionary<string, object>;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the kept first morning couldn't be read (" + ex.Message + "): it is reckoned again.");
                return false;
            }
            // (looked at whole before anything of it is put in)
            var markets = data != null ? Json.Arr(data, "markets") : null;
            var ships = data != null ? Json.Arr(Json.Obj(data, "world"), "ships") : null;
            if (data == null || (int)Json.Num(data, "version") != Version || Json.Str(data, "key") != key || markets == null || markets.Count == 0 ||
                ships == null || ships.Count == 0 || Math.Abs(Json.Num(Json.Obj(data, "world"), "hours", double.NaN) - hours) > 1e-6)
                return false;
            try
            {
                Markets.Restore(markets, year);
                Shocks.Restore(Json.Arr(data, "shocks"), year, hours / 24.0);
                world.Restore(Json.Obj(data, "world"));
                return true;
            }
            catch (Exception ex)
            {
                Spoiled = true;
                Debug.LogWarning("Kapman: the kept first morning couldn't be put in (" + ex.Message + "): it is reckoned again.");
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    // (it stays; the next start tries it and fails the same way)
                }
                return false;
            }
        }

        /// <summary>Writes the first morning just reckoned (the markets, the held shocks, the world) for the next game.</summary>
        public static void Keep(string key, WorldSim world)
        {
            if (Off || string.IsNullOrEmpty(key) || world == null)
                return;
            try
            {
                Directory.CreateDirectory(Folder);
                var data = new Dictionary<string, object>
                {
                    ["version"] = Version, ["key"] = key, ["markets"] = Markets.Capture(), ["shocks"] = Shocks.Capture(), ["world"] = world.Capture(),
                };
                var bytes = Encoding.UTF8.GetBytes(Json.Write(data));
                string path = PathOf(key);
                using (var f = File.Create(path + ".part"))
                using (var z = new GZipStream(f, System.IO.Compression.CompressionLevel.Fastest))
                    z.Write(bytes, 0, bytes.Length);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(path + ".part", path);
                // (only the newest few stay: every change of a table or of the code makes a new one)
                var all = new DirectoryInfo(Folder).GetFiles("first_morning_*.json.gz");
                Array.Sort(all, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
                for (int i = KeepFiles; i < all.Length; i++)
                    all[i].Delete();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the first morning couldn't be kept (" + ex.Message + "): the next game reckons it again.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Spoiled = false;
        }
    }
}
