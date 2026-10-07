using System.Linq;
using Kapman.Game;
using Kapman.Levels;
using Kapman.Sailing;
using Kapman.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE CHART GAME'S SCENE (the user, 4 Oct 2026: "build the game via the 2D map aspect"; picker: "the chart is the
    /// game") - L_Chart: no level, no terrain, no sea. The world frame at 1:60 (the clock's rate: ChartBody), the clock,
    /// a camera that draws nothing (the map is the screen), the merchant's ship as a ShipMotion driven by ChartBody, the
    /// chart's world (ChartWorld makes a Port for every trading place at Play), and the game as GameSetup adds it to any
    /// scene, with the map in chart mode and the parts that only make sense in 3D switched off (the voyage's beats, the
    /// world's ships as models, the event shots).
    /// Menu: Kapman ▸ Levels ▸ Build the chart game (2D).   Batch: -executeMethod Kapman.EditorTools.ChartBuilder.BuildBatch
    /// </summary>
    public static class ChartBuilder
    {
        public const string ScenePath = "Assets/Kapman/Scenes/L_Chart.unity";
        /// <summary>Real metres to a game metre: the clock's game seconds a second, so real m/s are game m/s (ChartBody).</summary>
        public const float Scale = 60f;
        /// <summary>The frame's look.</summary>
        public const string FrameStyle = "Assets/Kapman/Game/UI/Frame.uss";

        [MenuItem("Kapman/Levels/Build the chart game (2D)")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var frame = new GameObject("World frame (1:60)").AddComponent<FloatingOrigin>();
            frame.focus = null;
            frame.shiftDistance = float.MaxValue;                        // the sailing world lies within 30 game km of the origin
            frame.horizontalScale = Scale;
            frame.verticalScale = 1f;

            var clock = new GameObject("Clock").AddComponent<LevelClock>();
            clock.rate = Scale;
            clock.year = 793;
            clock.startDay = 121;                                        // 1 May 793 (7 Oct 2026: the season opens; Lindisfarne is still to come on 8 June, and its news comes by ship)
            clock.startHour = 7f;

            var camGo = new GameObject("Camera", typeof(Camera));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.106f, 0.137f, 0.251f);     // the palette's deep sea
            cam.cullingMask = 0;                                         // nothing to draw: the map is the screen
            var camData = camGo.AddComponent<HDAdditionalCameraData>();
            camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            camData.backgroundColorHDR = cam.backgroundColor;
            camGo.AddComponent<AudioListener>();
            // the chart's sound (7 Oct 2026): the modelled soundscape of the 3D levels, on the listener itself, fed by
            // ChartSound from where she is, the weather and the hour. The clips are given to the scene here (a built game
            // has only those); no shore map: the chart sets the ground outright
            var sound = new GameObject("Sound");
            sound.transform.SetParent(camGo.transform, false);
            var soundscape = SoundscapeSetup.AddTo(sound, null, 0f);
            soundscape.waitForFeed = true;
            soundscape.master = 0f;                                       // (ChartSound gives it its level)
            sound.AddComponent<ChartSound>();

            var ship = new GameObject("Ship");
            var motion = ship.AddComponent<ShipMotion>();
            motion.player = true;
            motion.length = 7.7f;
            motion.beam = 1.5f;
            motion.draught = 0.35f;
            ship.AddComponent<ChartBody>();
            ship.AddComponent<SailingShip>();

            new GameObject("Chart world").AddComponent<ChartWorld>();

            var game = GameSetup.AddToScene(false);
            // GameSetup gives the player's ShipMotion the 3D body (buoyancy on HDRP's sea): there is no sea here
            foreach (var c in ship.GetComponents<Component>())
                if (c != null && (c.GetType().Name == "ShipBody" || c is Rigidbody))
                    Object.DestroyImmediate(c);
            var map = game.GetComponentInChildren<MapView>(true);
            map.chartMode = true;
            map.riseSeconds = 0.05f;
            map.fadeSeconds = 0.05f;
            // the command frame (7 Oct 2026, Game/UI/Frame): the game is run from it and the chart is a picture to read.
            // Its own document, over the map's, the port's text and the market's, under the main menu's
            var frameGo = new GameObject("Frame");
            frameGo.transform.SetParent(game.transform, false);
            var frameDoc = frameGo.AddComponent<UIDocument>();
            frameDoc.panelSettings = map.GetComponent<UIDocument>().panelSettings;
            frameDoc.sortingOrder = 42;
            var commandFrame = frameGo.AddComponent<Kapman.Game.Frame.FrameRoot>();
            commandFrame.style = AssetDatabase.LoadAssetAtPath<StyleSheet>(FrameStyle);
            if (commandFrame.style == null)
                Debug.LogError("Kapman: " + FrameStyle + " is missing: the frame has no look.");
            // the parts that are about the 3D level: off
            foreach (var b in game.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (b == null)
                    continue;
                string n = b.GetType().Name;
                if (n == "VoyageDirector" || n == "WorldShips3D" || n == "HarbourLife" || n == "LocalBoats" || n == "ShotDirector")
                    b.enabled = false;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman: built " + ScenePath + " (the chart is the game).");
        }

        public static void BuildBatch()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
    }
}
