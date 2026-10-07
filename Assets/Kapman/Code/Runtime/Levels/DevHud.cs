using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE TEST PANEL - a plain panel for trying the level out: where and when, the weather, the water, the look, and the
    /// keys. A stand-in until the real interface (black glass, Gentium) is built for the rebuild.
    /// </summary>
    public class DevHud : MonoBehaviour
    {
        public bool show = true;
        public LevelWater water;

        static Rect panel = new Rect(12, 12, 470, 220);
        /// <summary>A line the game layer adds (the market prices): set by KapmanGame.</summary>
        public static System.Func<string> GameLine;
        public static bool PointerOverPanel { get; private set; }

        GUIStyle box, text, small;
        Texture2D dark;

        void Update()
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null || !show)
            {
                PointerOverPanel = false;
                return;
            }
            Vector2 p = mouse.position.ReadValue();
            p.y = Screen.height - p.y;
            PointerOverPanel = panel.Contains(p);
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.hKey.wasPressedThisFrame)
                show = !show;
        }

        void OnGUI()
        {
            if (!show)
                return;
            if (box == null)
            {
                dark = new Texture2D(1, 1);
                dark.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.55f));
                dark.Apply();
                box = new GUIStyle { normal = { background = dark } };
                text = new GUIStyle { fontSize = 15, normal = { textColor = new Color(0.92f, 0.92f, 0.9f) }, wordWrap = true };
                small = new GUIStyle(text) { fontSize = 12, normal = { textColor = new Color(0.75f, 0.75f, 0.72f) } };
            }
            var info = LevelInfo.Current;
            var clock = LevelClock.Current;
            var weather = LevelWeather.Current;
            var ship = ShipMotion.Player;
            string when = clock != null ? clock.Text : "";
            string speed = LevelClock.Paused ? "paused" : LevelClock.Speed + "x";
            string feast = TownLife.FeastToday != null ? "  ·  " + TownLife.FeastToday : "";
            string sky = weather != null ? weather.Description : "";
            string tide = water != null ? $"  ·  tide {water.Tide:+0.00;-0.00} m" : "";
            string level = info != null ? $"Water {info.WaterLevel:0.00} m{tide}" : "";

            string order = ship != null && !string.IsNullOrEmpty(ship.Status) ? ship.Status : "Click the water to make for there.";

            GUI.Box(panel, GUIContent.none, box);
            GUILayout.BeginArea(new Rect(panel.x + 10, panel.y + 8, panel.width - 20, panel.height - 12));
            GUILayout.Label($"{(info != null ? info.displayName : "")}  ·  {when}  ·  {speed}{feast}", text);
            GUILayout.Label(sky, text);
            GUILayout.Label(level, small);
            var waterNow = Kapman.Levels.Water.WaterProfileDriver.Current;
            if (waterNow != null && waterNow.Profile != null)
                GUILayout.Label(waterNow.Line, small);
            string gameLine = GameLine?.Invoke();
            if (!string.IsNullOrEmpty(gameLine))
                GUILayout.Label(gameLine, small);
            if (!string.IsNullOrEmpty(LevelSoundscape2.Loudest))
                GUILayout.Label("Sound: " + LevelSoundscape2.Loudest, small);

            GUILayout.Label(order, small);
            GUILayout.EndArea();

            var exit = LevelExit.Near;
            if (exit != null)
            {
                var r = new Rect(Screen.width / 2 - 260, Screen.height - 120, 520, 44);
                GUI.Box(r, GUIContent.none, box);
                string line = exit.Built ? $"{exit.prompt}   [Enter]" : $"{exit.prompt}   (not built yet)";
                GUI.Label(new Rect(r.x + 14, r.y + 12, r.width - 28, r.height), line, text);
            }

            var keys = new Rect(12, Screen.height - 46, Screen.width - 24, 36);
            GUI.Box(keys, GUIContent.none, box);
            GUI.Label(new Rect(keys.x + 10, keys.y + 4, keys.width - 20, keys.height), "Click water: make for there   Right-drag: look round (springs back)   Middle-drag or Q/E: turn   WASD: move round her   Wheel: zoom (out past the widest: the map)   M: map   " +
                "WASD: wander   F: back to the ship   Tab: 1x/2x/4x   Space: pause   T/Y/U: an hour/day/month on   " +
                "G: weather   J: aurora   K: flood   F1-F12: looks   I: ink style   Shift+M: sound   X: sail   V: oars   H: this panel", small);
        }
    }
}
