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
    /// From a command line: -executeMethod Kapman.EditorTools.KapmanPhysical.Step1
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
