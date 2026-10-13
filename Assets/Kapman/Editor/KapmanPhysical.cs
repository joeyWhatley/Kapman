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
    ///   Kapman > Make physical > 2d. The chart's layout file
    ///   Kapman > Make physical > 3. The 3D stage into the scene
    ///   Kapman > Make physical > 4. The ports into the scene
    /// From a command line: -executeMethod Kapman.EditorTools.KapmanPhysical.Step1 (or .Step2Frame, .Step2Parts, .Step3Stage, .Step4Ports)
    /// </summary>
    public static class KapmanPhysical
    {
        public const string GameScene = "Assets/Kapman/Scenes/Kapman.unity";
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

        public const string MapLayout = "Assets/Kapman/Game/UI/MapView.uxml";

        /// <summary>The chart's UI Document is given its layout file as its Source Asset (Game/UI/MapView.uxml): the chart
        /// is then laid out in UI Builder, and MapView only fills it in (KAPMAN_PLAN step 2d, 14 Oct 2026).</summary>
        [MenuItem("Kapman/Make physical/2d. The chart's layout file")]
        public static void Step2Map()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var layout = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(MapLayout);
            if (layout == null)
                throw new System.InvalidOperationException(MapLayout + " is missing or did not import.");
            var map = Object.FindAnyObjectByType<MapView>(FindObjectsInactive.Include);
            if (map == null)
                throw new System.InvalidOperationException(GameScene + " has no MapView.");
            var doc = map.GetComponent<UnityEngine.UIElements.UIDocument>();
            doc.visualTreeAsset = layout;
            EditorUtility.SetDirty(doc);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 2d: the Map view's UI Document has " + MapLayout + " as its Source Asset.");
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
            parts.hoverBox = Layout("HoverBox");
            parts.courseKnot = Layout("CourseKnot");
            parts.helmMark = Layout("HelmMark");
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

        public const string StageProfile = "Assets/Kapman/ShipView/Ship stage sky and air.asset";

        /// <summary>
        /// THE 3D STAGE STANDS IN THE SCENE (the user, 14 Oct 2026: "l chart is a bunch of scripts still"; by the picker,
        /// "Yes, both: frame and stage"; KAPMAN_PLAN step 1b). The stage's object is put under SYSTEMS with its kit, and
        /// the stage's own code builds on it, once, what can be saved (ShipStage.MakeSaved): the boat's model as a prefab
        /// instance, the Sun and the Moon, the sky and air's volume with its profile (saved as an asset beside the kit),
        /// the camera with its rain, and HDRP's sea. Seen in the Scene view, tuned in the Inspector; at play ShipStage
        /// takes them up as they are and makes only the rest (ShipStage.Build). Run again it finds all of it there and
        /// makes nothing.
        /// </summary>
        [MenuItem("Kapman/Make physical/3. The 3D stage into the scene")]
        public static void Step3Stage()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var kit = AssetDatabase.LoadAssetAtPath<ShipStageKit>(ShipStageKitBuilder.KitPath);
            if (kit == null)
                throw new System.InvalidOperationException("no kit at " + ShipStageKitBuilder.KitPath + " (Kapman > Ship view > Make the kit).");
            var stage = Object.FindAnyObjectByType<ShipStage>(FindObjectsInactive.Include);
            bool made = stage == null;
            if (made)
            {
                var systems = GameObject.Find(SystemsName);
                if (systems == null)
                    systems = new GameObject(SystemsName);
                var go = new GameObject(ShipStage.ObjectName);
                go.transform.SetParent(systems.transform, false);
                stage = go.AddComponent<ShipStage>();
            }
            stage.kit = kit;
            int before = stage.GetComponentsInChildren<Transform>(true).Length;
            // (the kit's prefabs go on as prefab instances, so the model in the scene is still the pack's prefab)
            ShipStage.PrefabInstantiator = (prefab, parent) => (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            try
            {
                stage.MakeSaved();
            }
            finally
            {
                ShipStage.PrefabInstantiator = null;
            }
            // the sky and air's profile: an asset, with its parts inside it, so the scene can hold it and it can be tuned
            var profile = stage.Volume != null ? stage.Volume.sharedProfile : null;
            if (profile != null && !AssetDatabase.Contains(profile))
            {
                AssetDatabase.CreateAsset(profile, StageProfile);
                foreach (var part in profile.components)
                    if (part != null && !AssetDatabase.Contains(part))
                        AssetDatabase.AddObjectToAsset(part, profile);
                AssetDatabase.SaveAssets();
            }
            int after = stage.GetComponentsInChildren<Transform>(true).Length;
            EditorUtility.SetDirty(stage);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 3: " + ShipStage.ObjectName + (made ? " put into " : " found in ") + GameScene +
                      " with " + (after - before) + " parts made (" + (after - 1) + " in all); the sky and air's profile is " + StageProfile + ".");
        }

        /// <summary>
        /// THE PORTS STAND IN THE SCENE (14 Oct 2026, KAPMAN_PLAN step 1b, after the stage). Every trading place of
        /// Data/ports_793.json is an object "Port id" under "--- PORTS (the chart's) ---", with its Port and its Berth,
        /// placed where the table puts it in the world frame (the frame's scale, the origin at 56 N 5 E). The chart at
        /// play takes them as saved, so a port moved by hand in the Scene view stays moved (ChartWorld.PlacePorts).
        /// Run again it finds them all and makes nothing; a port added to the table is made next time.
        /// </summary>
        [MenuItem("Kapman/Make physical/4. The ports into the scene")]
        public static void Step4Ports()
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            var table = GameFiles.Find("ports_793");
            if (table == null)
                throw new System.InvalidOperationException("no table ports_793.json in " + GameFiles.Folder + ".");
            GameData.LoadPorts(table);
            var frame = Object.FindAnyObjectByType<Kapman.World.FloatingOrigin>(FindObjectsInactive.Include);
            double scale = frame != null ? Mathf.Max(0.001f, frame.horizontalScale) : 60.0;
            var chart = Object.FindAnyObjectByType<ChartWorld>(FindObjectsInactive.Include);
            float reach = chart != null ? chart.arrivalRadius : 30f;
            var root = ChartWorld.PortsRoot();
            int made = ChartWorld.PlacePorts(root, def =>
            {
                var (x, z) = Kapman.World.WorldFrame.ToWorld(def.lat, def.lon);
                return new Vector3((float)(x / scale), 0f, (float)(z / scale));
            }, reach, out int found);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 4: " + (found + made) + " ports under " + ChartWorld.PortsRootName + " in " + GameScene +
                      " (" + found + " there already, " + made + " made) at 1:" + scale + ".");
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
