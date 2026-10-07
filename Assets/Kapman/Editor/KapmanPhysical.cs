using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.ShipView;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// KAPMAN MADE PHYSICAL, STEP BY STEP (Design/KAPMAN_PLAN.md). Each step here changes the project's scenes and
    /// assets once, through the editor, so that what the game is made of stands in the Hierarchy and the Inspector and
    /// not in code that runs at the start. A step can be run again: it leaves alone what is already there.
    ///   Kapman > Make physical > 1. Systems into the scene
    ///   Kapman > Make physical > 2. The frame's layout file
    ///   Kapman > Make physical > 2b. The frame's parts
    /// From a command line: -executeMethod Kapman.EditorTools.KapmanPhysical.Step1 (or .Step2Frame, .Step2Parts)
    /// </summary>
    public static class KapmanPhysical
    {
        public const string GameScene = "Assets/Kapman/Scenes/L_Chart.unity";
        const string SystemsName = "--- SYSTEMS ---";

        /// <summary>The five parts that made themselves when the scene had loaded (the title and the opening, the music,
        /// the notes of F2, the port's picture, the ship's 3D view) become objects of the Game scene under SYSTEMS.</summary>
        [MenuItem("Kapman/Make physical/1. Systems into the scene")]
        public static void Step1()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            GameObject systems = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == SystemsName)
                    systems = root;
            if (systems == null)
                systems = new GameObject(SystemsName);
            int made = 0;
            made += Put<Showtime>(systems.transform, "Showtime (the title and the opening)") ? 1 : 0;
            made += Put<PlayNotes>(systems.transform, "Play notes (F2)") ? 1 : 0;
            made += Put<PortView>(systems.transform, "Port view (the port's picture)") ? 1 : 0;
            if (Put<ShipStageHook>(systems.transform, "Ship view (3D)"))
            {
                made++;
                var hook = Object.FindAnyObjectByType<ShipStageHook>(FindObjectsInactive.Include);
                var kit = ShipStageKit.Load();
                if (hook != null && kit != null)
                {
                    var so = new SerializedObject(hook);
                    so.FindProperty("kit").objectReferenceValue = kit;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            // (the music stays through a new game, so it stands at the scene's root: an object kept across scenes must)
            made += Put<GameMusic>(null, "Game music") ? 1 : 0;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 1: " + made + " parts put into " + GameScene + " (" + SystemsName + ").");
        }

        public const string FrameLayout = "Assets/Kapman/Game/UI/Frame.uxml";

        /// <summary>The frame's UI Document is given its layout file as its Source Asset (Game/UI/Frame.uxml): the frame is
        /// then laid out in UI Builder, and FrameRoot only fills it in.</summary>
        [MenuItem("Kapman/Make physical/2. The frame's layout file")]
        public static void Step2Frame()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var layout = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(FrameLayout);
            if (layout == null)
                throw new System.InvalidOperationException(FrameLayout + " is missing or did not import.");
            var frame = Object.FindAnyObjectByType<FrameRoot>(FindObjectsInactive.Include);
            if (frame == null)
                throw new System.InvalidOperationException(GameScene + " has no FrameRoot.");
            var doc = frame.GetComponent<UnityEngine.UIElements.UIDocument>();
            doc.visualTreeAsset = layout;
            EditorUtility.SetDirty(doc);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 2: the Frame's UI Document has " + FrameLayout + " as its Source Asset.");
        }

        public const string MenuLayout = "Assets/Kapman/Game/UI/MainMenu.uxml";
        public const string ShowLayout = "Assets/Kapman/Game/UI/Showtime.uxml";

        /// <summary>The menu's UI Document is given its layout file as its Source Asset (Game/UI/MainMenu.uxml).</summary>
        [MenuItem("Kapman/Make physical/2c. The menu's layout file")]
        public static void Step2Menu()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var layout = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(MenuLayout);
            if (layout == null)
                throw new System.InvalidOperationException(MenuLayout + " is missing or did not import.");
            var menu = Object.FindAnyObjectByType<MainMenu>(FindObjectsInactive.Include);
            if (menu == null)
                throw new System.InvalidOperationException(GameScene + " has no MainMenu.");
            var doc = menu.GetComponent<UnityEngine.UIElements.UIDocument>();
            doc.visualTreeAsset = layout;
            EditorUtility.SetDirty(doc);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Step2Parts();
            Debug.Log("Kapman, made physical, step 2c: the Main menu's UI Document has " + MenuLayout + " as its Source Asset.");
        }

        public const string PartsFolder = "Assets/Kapman/Game/UI/Parts/";
        public const string PartsAsset = "Assets/Kapman/Game/Resources/FrameParts.asset";

        /// <summary>The asset that names the files of the frame's repeated parts (Resources/FrameParts): made if it is not
        /// there, and each of its fields given its file of Game/UI/Parts.</summary>
        [MenuItem("Kapman/Make physical/2b. The frame's parts")]
        public static void Step2Parts()
        {
            var parts = AssetDatabase.LoadAssetAtPath<FrameParts>(PartsAsset);
            if (parts == null)
            {
                parts = ScriptableObject.CreateInstance<FrameParts>();
                AssetDatabase.CreateAsset(parts, PartsAsset);
            }
            parts.window = Layout("Window");
            parts.floatBox = Layout("Float");
            parts.option = Layout("Option");
            parts.line = Layout("Line");
            parts.lineButton = Layout("LineButton");
            parts.logLine = Layout("LogLine");
            parts.menuOption = Layout("MenuOption");
            parts.showtime = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(ShowLayout);
            if (parts.showtime == null)
                throw new System.InvalidOperationException(ShowLayout + " is missing or did not import.");
            parts.showChoice = Layout("ShowChoice");
            parts.showFace = Layout("ShowFace");
            parts.showName = Layout("ShowName");
            EditorUtility.SetDirty(parts);
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman, made physical, step 2b: " + PartsAsset + " names the files of " + PartsFolder + " and " + ShowLayout + ".");
        }

        static UnityEngine.UIElements.VisualTreeAsset Layout(string file)
        {
            var a = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(PartsFolder + file + ".uxml");
            if (a == null)
                throw new System.InvalidOperationException(PartsFolder + file + ".uxml is missing or did not import.");
            return a;
        }

        static bool Put<T>(Transform parent, string name) where T : Component
        {
            if (Object.FindAnyObjectByType<T>(FindObjectsInactive.Include) != null)
                return false;
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.AddComponent<T>();
            return true;
        }
    }
}
