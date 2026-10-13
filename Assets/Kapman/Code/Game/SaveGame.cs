using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// SAVED GAMES - the whole game as plain data in a file (gzipped JSON, readable once unzipped): the clock, the ship
    /// (where she is, which way she heads, the port she lies at), the course, the crew and their stores, the purse, hold,
    /// ledger and debt, every market, the world's ships and waterfronts (WorldSim.Capture: their dice too, so the world goes
    /// on as it would have), what's been seen of the map, the deeds, what the merchant knows (Known: the news heard, ships
    /// sighted and spoken to, places heard of; the price notes are in the trade). Saved in the player's data
    /// folder (saves/), one file a slot; [F5] saves to "quick", [F9] loads it. Not while something's on (a meeting, a
    /// chase, a search, a panel open): it says so.
    ///
    /// ported 4 Oct: the ship's place is saved in the world frame as before, with the level she was in. Loading in the same
    /// level puts her back there; loading in another level leaves her where she is and says so (the chain's loader, which
    /// opens the saved level first, comes with the level chain). The clock is the level's (LevelClock: its hours are set
    /// straight, there's no Skip), and the world is never shifted (the level's frame is fixed: LevelFrame).
    /// </summary>
    public static class SaveGame
    {
        /// <summary>2: what the merchant knows is kept whole (Known), the price notes keep their history. Version 1 saves
        /// still load (their news heard comes back where the waterfronts still hold it).</summary>
        public const int Version = 2;
        public static string Folder => FolderOverride ?? Path.Combine(Application.persistentDataPath, "saves");
        /// <summary>Another folder for the saved games (a test, so it never touches the player's own). Null: the player's.</summary>
        public static string FolderOverride;

        // (a play never inherits the folder a test named: Play is entered without a domain reload)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics() => FolderOverride = null;

        /// <summary>A game was loaded: the port she lies at (its id), or null at sea. The graphs' bridge sets the shared
        /// variables and opens the port's menu.</summary>
        public static event Action<string> Loaded;

        /// <summary>She has been laid at a port without sailing there (a new game whose people start at their own port):
        /// the graphs are told as a load tells them, so she is made fast there and the port's menu opens.</summary>
        public static void LaidAt(string portId) => Loaded?.Invoke(portId);

        static string PathOf(string slot) => Path.Combine(Folder, slot + ".kapman.json.gz");
        static string InfoOf(string slot) => Path.Combine(Folder, slot + ".kapman.info");

        /// <summary>A new slot's name: "save 1", "save 2"... (the first free).</summary>
        public static string NewSlot()
        {
            for (int i = 1; ; i++)
                if (!File.Exists(PathOf("save " + i)))
                    return "save " + i;
        }

        /// <summary>Why the game can't be saved or loaded just now (null: it can).</summary>
        public static string Busy()
        {
            var game = KapmanGame.Current;
            if (game == null || game.World == null || game.ship == null || !Trade.Started)
                return "The game isn't ready.";
            if (Sim.Encounter.Stage != Sim.EncounterStage.None || Sim.RaiderEncounter.Stage != Sim.RaidStage.None
                || Sim.Patrols.Stage != Sim.PatrolStage.None)
                return "Not now: finish with the ship you've met first.";
            if (Aground.Stage != AgroundStage.None)
                return "Not now: she's being got off the ground.";
            if (TextPanel.Current != null && TextPanel.Current.Open && !LyingInPort(game))
                return "Not now: answer what is in front of you first.";
            return null;
        }

        /// <summary>Why the game can't be SAVED just now (null: it can): all of <see cref="Busy"/>, a lost boat, and a new game on the
        /// chart that has not chosen who it is yet (there is no game to save; loaded, it would ask again half begun).</summary>
        public static string CannotSave()
        {
            string why = Busy();
            if (why != null)
                return why;
            // (a lost boat is not saved: nothing may be written over the saves from before. It is LOADED from: the last
            // page's way back, so this is not in Busy, which a load asks too)
            if (Boat.IsLost)
                return "She is lost: load a saved game.";
            if (ChartWorld.Current != null && !Self.Chosen)
                return "Not yet: the game has not begun.";
            return null;
        }

        static bool LyingInPort(KapmanGame game)
        {
            var p = Port.Nearest(game.ShipPosition);
            return game.ship.anchored && p != null && p.DistanceTo(game.ShipPosition) < p.arrivalRadius;
        }

        /// <summary>Saves the game to a slot; returns the fact (saved, or why not).</summary>
        public static string Save(string slot = "quick")
        {
            string why = CannotSave();
            if (why != null)
                return why;
            var game = KapmanGame.Current;
            var w = FloatingOrigin.ToWorld(game.ship.transform.position);
            var p = Port.Nearest(game.ShipPosition);
            // (7 Oct 2026: saving no longer thins the price notes. The game saves itself at every mooring now, and
            // saving must not change the game: the old notes are thinned as the days turn, in KapmanGame.)
            var data = new Dictionary<string, object>
            {
                ["version"] = Version,
                ["date"] = game.Date,
                ["saved"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                ["hours"] = game.Hours,
                ["level"] = LevelInfo.Current != null ? LevelInfo.Current.levelId : null,   // ported 4 Oct: which level she was in
                ["ship"] = new Dictionary<string, object>
                {
                    ["x"] = w.x, ["z"] = w.z, ["heading"] = (double)game.ship.Heading, ["anchored"] = game.ship.anchored,
                    ["lying_at"] = LyingInPort(game) ? p.id : null,
                },
                ["course"] = Course.Capture(),
                ["voyage"] = Voyage.Capture(),
                ["boat"] = Boat.Capture(),                                    // 7 Oct: her hull, water, sail, order, weariness, shares
                ["trade"] = Trade.Capture(),
                ["markets"] = Markets.Capture(),
                ["shocks"] = Shocks.Capture(),
                ["lessons"] = Lessons.Capture(),
                ["world"] = game.World.Capture(),
                ["seen"] = Exploration.Capture(),
                ["deeds"] = Sim.Deeds.Capture(),
                ["known"] = Known.Capture(),
                ["chest"] = Chest.Capture(),
                ["hoards"] = Hoards.Capture(),
                ["standing"] = Reputation.Capture(),
                ["orders"] = Commissions.Capture(),
                ["encounters"] = EncounterRunner.Capture(),
                ["content"] = ContentSave.Capture(),                          // 7 Oct: what the new content keeps that is not a flag
            };
            try
            {
                Directory.CreateDirectory(Folder);
                var bytes = Encoding.UTF8.GetBytes(Json.Write(data));
                using (var f = File.Create(PathOf(slot) + ".part"))
                using (var z = new GZipStream(f, System.IO.Compression.CompressionLevel.Optimal))
                    z.Write(bytes, 0, bytes.Length);
                if (File.Exists(PathOf(slot)))
                    File.Delete(PathOf(slot));
                File.Move(PathOf(slot) + ".part", PathOf(slot));
                // what the lists show, without unzipping the game
                // (the port's own name, not its object's: on the chart that was "Port traiectum")
                string where = LyingInPort(game) ? "lying at " + (p.Def != null ? p.Def.name : p.id) : "at sea";
                File.WriteAllText(InfoOf(slot), Json.Write(new Dictionary<string, object> { ["date"] = game.Date, ["where"] = where }), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the game couldn't be saved: " + ex.Message);
                return "The game couldn't be saved.";
            }
            return $"Saved: {game.Date}.";
        }

        /// <summary>Loads a slot over the game running now; returns the fact.</summary>
        public static string Load(string slot = "quick")
        {
            string why = Busy();
            if (why != null)
                return why;
            if (!File.Exists(PathOf(slot)))
                return "There's no saved game to load.";
            Dictionary<string, object> data;
            try
            {
                using (var f = File.OpenRead(PathOf(slot)))
                using (var gz = new GZipStream(f, CompressionMode.Decompress))
                using (var r = new StreamReader(gz, Encoding.UTF8))
                    data = Json.Parse(r.ReadToEnd()) as Dictionary<string, object>;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Kapman: the saved game couldn't be read: " + ex.Message);
                return "The saved game couldn't be read.";
            }
            int version = data != null ? (int)Json.Num(data, "version") : 0;
            if (version != Version && version != 1)
                return "The saved game is from another version.";
            var game = KapmanGame.Current;
            // (whatever the graphs were waiting on is of the game that was: given up, not only hidden. Hidden only, the
            // old port's menu went on waiting behind the loaded game's, and a choice was taken twice)
            TextPanel.AbandonWaits();
            if (MapView.Current != null && MapView.Current.IsOpen)
                MapView.Current.Close();
            // the clock first (the world and the markets are reckoned in its hours)
            if (game.clock != null)
                game.clock.GameHours = Math.Max(0.0, Json.Num(data, "hours", game.Hours));   // ported 4 Oct: was clock.Skip(difference)
            Markets.Restore(Json.Arr(data, "markets"), game.Year);
            Shocks.Restore(Json.Arr(data, "shocks"), game.Year, game.Day);
            Lessons.Restore(Json.Arr(data, "lessons"));
            Trade.Restore(Json.Obj(data, "trade"));
            Voyage.Restore(Json.Obj(data, "voyage"));
            Boat.Restore(Json.Obj(data, "boat"));                             // (a save without it: a sound boat on a wage)
            BoatWatch.Loaded();
            game.World.Restore(Json.Obj(data, "world"));
            Exploration.Restore(Json.Obj(data, "seen"));
            Sim.Deeds.Restore(Json.Arr(data, "deeds"));
            Chest.Restore(Json.Obj(data, "chest"));
            Hoards.Restore(Json.Obj(data, "hoards"));
            if (version >= 2)
                Known.Restore(Json.Obj(data, "known"));
            else
            {
                Known.Clear();
                PortLife.RestoreOld(Json.Arr(data, "heard"));
            }
            Reputation.Restore(Json.Obj(data, "standing"));
            Commissions.Restore(Json.Arr(data, "orders"));
            EncounterRunner.Restore(Json.Obj(data, "encounters"));
            ContentSave.Restore(Json.Obj(data, "content"));                   // (after the flags; a save without it: each part afresh)
            Chronicle.StartAt(game.clock != null ? game.clock.Date : Calendar.At(793, 172, 0.0));
            game.ResyncHours();
            // the ship: back where she was, if this is the level she was in
            var ship = Json.Obj(data, "ship");
            double x = Json.Num(ship, "x"), z = Json.Num(ship, "z");
            bool elsewhere = false;
            string savedLevel = Json.Str(data, "level", null), thisLevel = LevelInfo.Current != null ? LevelInfo.Current.levelId : null;
            if (savedLevel == null || savedLevel == thisLevel)
            {
                LevelFrame.Ensure();
                var at = FloatingOrigin.ToUnity(new WorldPos(x, 0.0, z));
                at.y = game.ship.transform.position.y;
                game.ship.Teleport(at, (float)Json.Num(ship, "heading"));
            }
            else
            {
                Debug.LogWarning($"Kapman: the saved game was in the level '{savedLevel}', this is '{thisLevel}': she stays where she is " +
                                 "(the level chain's loader, which opens the saved level first, is still to come).");
                elsewhere = true;
            }
            game.ship.anchored = Json.Bool(ship, "anchored");
            Course.Restore(Json.Obj(data, "course"));
            HandHelm.Clear();                                            // (13 Oct 2026: a game loaded has no hand on the helm)
            Aground.Loaded();                                             // (its timers are of the game that was)
            TideWait.Loaded();
            Autosave.CastOff();                                           // (a mooring's save still held was of the game that was: loading is not mooring)
            GameEvents.Loaded();                                          // (a happening showing or waiting was of the game that was)
            Sim.Encounter.Loaded();
            ChartWorld.SaveLoaded = true;                                 // (this game was begun long ago: no call at home, no "who are you")
            Loaded?.Invoke(Json.Str(ship, "lying_at", null));
            // (said to the player, not only to the console: all of the game is back but her place)
            return $"Loaded: {Json.Str(data, "date")}." + (elsewhere ? " It was saved in another place: she lies where she was, not where she was saved." : "");
        }

        /// <summary>A saved game in the lists: its slot, the game's date in it, where she lay, when it was saved.</summary>
        public struct SlotInfo
        {
            public string slot, date, where;
            public DateTime when;
        }

        /// <summary>The saved games there are, the latest first.</summary>
        public static List<SlotInfo> Slots()
        {
            var list = new List<SlotInfo>();
            if (!Directory.Exists(Folder))
                return list;
            foreach (var f in Directory.GetFiles(Folder, "*.kapman.json.gz"))
            {
                var s = new SlotInfo { slot = Path.GetFileName(f).Replace(".kapman.json.gz", ""), date = "", where = "", when = File.GetLastWriteTime(f) };
                try
                {
                    if (File.Exists(InfoOf(s.slot)))
                    {
                        var info = Json.Parse(File.ReadAllText(InfoOf(s.slot), Encoding.UTF8));
                        s.date = Json.Str(info, "date", "");
                        s.where = Json.Str(info, "where", "");
                    }
                }
                catch (Exception)
                {
                    // (no info: the list shows the slot and when it was saved)
                }
                list.Add(s);
            }
            list.Sort((a, b) => b.when.CompareTo(a.when));
            return list;
        }
    }
}
