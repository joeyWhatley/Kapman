using System;
using System.IO;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Game
{
    /// <summary>
    /// THE VOYAGE'S BEATS IN PLAY (playability, first slice): once a game minute it builds what the beats see (the
    /// player's ship, the wind, the visibility, the way out) and ticks the BeatScheduler; it draws the open beat as a box
    /// with numbered options (keys 1-4), carries out an option's order, and keeps the crew's standing orders on keys
    /// (X: the sail full, reefed, struck; V: the oars as needed, all, shipped). When the level goes, the voyage log is
    /// written as CSV for the playtest (G:\kapman_unity\data\work\playtests\date\). All wording is placeholder.
    /// </summary>
    public class VoyageDirector : MonoBehaviour
    {
        public static VoyageDirector Current { get; private set; }

        public string levelId = "dorestad";
        public TextAsset beats, lessons, landmarks;
        [Tooltip("Where the playtest logs go (a folder per day).")]
        public string logFolder = @"G:\kapman_unity\data\work\playtests";

        public BeatScheduler Scheduler { get; private set; }
        long lastMinute = long.MinValue;
        GUIStyle box, text, small;
        Texture2D dark;

        void OnEnable() => Current = this;

        void OnDisable()
        {
            WriteLog();
            if (Current == this)
                Current = null;
        }

        void Start()
        {
            beats = beats != null ? beats : GameFiles.Find("beats_793");
            lessons = lessons != null ? lessons : GameFiles.Find("lessons_793");
            string marksJson = landmarks != null ? landmarks.text : ReadLandmarks(levelId);
            var words = BeatWords.Parse(beats != null ? beats.text : null);
            Lessons.Load(lessons != null ? lessons.text : null);
            Scheduler = new BeatScheduler(words);
            Scheduler.raisers.Add(new CastOff(words));
            Scheduler.raisers.Add(new LandmarkSight(Landmarks.Parse(marksJson), words));
            Scheduler.raisers.Add(new WindBeats(words));
            Scheduler.raisers.Add(new ExitBeat(words));
        }

        static string ReadLandmarks(string level)
        {
            try
            {
                string path = Path.Combine(Application.dataPath, "Kapman/Data/Levels", $"landmarks_{level}.json");
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman beats: the landmarks could not be read: " + e.Message);
                return null;
            }
        }

        BeatContext Context()
        {
            var clock = LevelClock.Current;
            var ship = ShipMotion.Player;
            var weather = LevelWeather.Current;
            var exit = LevelExit.Near;
            var c = new BeatContext
            {
                minute = clock != null ? clock.GameHours * 60.0 : Time.time,
                realSeconds = Time.unscaledTime,
                year = clock != null ? clock.year : 793,
                wind = weather != null ? weather.WindVector : Vector2.zero,
                visibility = weather != null ? 3.912f / Mathf.Max(1e-6f, weather.FogDensity) : 20000f,
                exitNear = exit != null,
                exitPrompt = exit != null ? exit.prompt : null,
            };
            if (ship != null)
            {
                var p = ship.transform.position;
                c.position = new Vector2(p.x, p.z);
                c.heading = ship.heading;
                c.speed = ship.Speed;
            }
            return c;
        }

        void Update()
        {
            if (Scheduler == null || ShipMotion.Player == null)
                return;
            var c = Context();
            long minute = (long)Math.Floor(c.minute);
            if (minute != lastMinute)
            {
                lastMinute = minute;
                Scheduler.Tick(c);
                if (Scheduler.Open != null && Scheduler.Open.kind == "exit")
                    Scheduler.Choose(0, c);                                  // the way out has its own prompt (DevHud): logged only
            }
            var kb = Keyboard.current;
            if (kb == null || UiFocus.Modal)
                return;
            if (Scheduler.Open != null)
                for (int i = 0; i < 4 && i < Scheduler.Open.options.Length; i++)
                    if (kb[Key.Digit1 + i].wasPressedThisFrame)
                    {
                        Carry(Scheduler.Choose(i, c));
                        break;
                    }
            var ship = ShipMotion.Player;
            if (kb.xKey.wasPressedThisFrame)
            {
                ship.sailOrder = (ShipMotion.SailOrder)(((int)ship.sailOrder + 1) % 3);
                Scheduler.log.Note(c, "order", null, "sail " + ship.sailOrder);
            }
            if (kb.vKey.wasPressedThisFrame)
            {
                ship.oarsOrder = (ShipMotion.OarsOrder)(((int)ship.oarsOrder + 1) % 3);
                Scheduler.log.Note(c, "order", null, "oars " + ship.oarsOrder);
            }
        }

        /// <summary>What an option does: an order to the crew (beats_793.json's "does").</summary>
        void Carry(string does)
        {
            var ship = ShipMotion.Player;
            if (ship == null || string.IsNullOrEmpty(does))
                return;
            switch (does)
            {
                case "order:full": ship.sailOrder = ShipMotion.SailOrder.Full; break;
                case "order:reef": ship.sailOrder = ShipMotion.SailOrder.Reefed; break;
                case "order:strike": ship.sailOrder = ShipMotion.SailOrder.Struck; break;
                case "order:row_all": ship.oarsOrder = ShipMotion.OarsOrder.All; break;
                case "order:oars_as_needed": ship.oarsOrder = ShipMotion.OarsOrder.AsNeeded; break;
                case "order:anchor": ship.EndOrder("Anchored."); break;
            }
        }

        void WriteLog()
        {
            if (Scheduler == null || Scheduler.log.entries.Count == 0 || string.IsNullOrEmpty(logFolder) || Application.isBatchMode)
                return;                                                      // (not from the headless test runs)
            try
            {
                string dir = Path.Combine(logFolder, DateTime.Now.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, $"voyage_{levelId}_{DateTime.Now:HHmmss}.csv");
                File.WriteAllText(file, Scheduler.log.Csv());
                var gaps = Scheduler.log.Gaps(240.0, Scheduler.log.entries[0].realSeconds, Time.unscaledTime);
                Debug.Log($"Kapman beats: the voyage log ({Scheduler.log.Offered} beats, {gaps.Count} gaps over 4 real minutes) is {file}");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman beats: the voyage log could not be written: " + e.Message);
            }
        }

        void OnGUI()
        {
            var ship = ShipMotion.Player;
            if (Scheduler == null || ship == null)
                return;
            if (box == null)
            {
                dark = new Texture2D(1, 1);
                dark.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.55f));
                dark.Apply();
                box = new GUIStyle { normal = { background = dark } };
                text = new GUIStyle { fontSize = 16, normal = { textColor = new Color(0.94f, 0.94f, 0.92f) }, wordWrap = true };
                small = new GUIStyle(text) { fontSize = 13, normal = { textColor = new Color(0.78f, 0.78f, 0.75f) } };
            }
            var orders = new Rect(Screen.width - 330, Screen.height - 86, 318, 30);
            GUI.Box(orders, GUIContent.none, box);
            GUI.Label(new Rect(orders.x + 10, orders.y + 6, orders.width - 20, 22), $"Sail: {ship.sailOrder} [X]   Oars: {ship.oarsOrder} [V]", small);
            var b = Scheduler.Open;
            if (b == null)
                return;
            float h = 64 + 24 * b.options.Length + (Scheduler.OpenLesson != null ? 44 : 0);
            var r = new Rect(Screen.width / 2 - 300, Screen.height - 180 - h, 600, h);
            GUI.Box(r, GUIContent.none, box);
            GUILayout.BeginArea(new Rect(r.x + 14, r.y + 10, r.width - 28, r.height - 16));
            GUILayout.Label(b.question, text);
            if (Scheduler.OpenLesson != null)
                GUILayout.Label(Scheduler.OpenLesson, small);
            for (int i = 0; i < b.options.Length; i++)
                GUILayout.Label($"{i + 1}   {b.options[i]}", small);
            GUILayout.EndArea();
        }
    }
}
