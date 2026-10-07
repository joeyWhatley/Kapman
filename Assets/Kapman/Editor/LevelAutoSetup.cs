using Kapman.Game;
using Kapman.Levels;
using Kapman.Levels.Water;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE LEVELS KEEP THEMSELVES UP TO DATE (the user, 4 Oct: "why wouldn't it just be set up already"): whenever a level
    /// scene is open in the editor (on load, on opening it, and just before Play) this checks that it carries every part
    /// the game has grown since it was built (the voyage's beats, the encounters, the economy's dev keys, the modelled
    /// soundscape with all its clips, the level's water record) and adds what is missing, then saves the scene. Nothing to
    /// do by hand, and no rebuild.
    /// </summary>
    [InitializeOnLoad]
    public static class LevelAutoSetup
    {
        static LevelAutoSetup()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => EditorApplication.delayCall += () => Check(scene);
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                    Check(SceneManager.GetActiveScene());
            };
            EditorApplication.delayCall += () => Check(SceneManager.GetActiveScene());
        }

        static string LevelOf(Scene scene) =>
            scene.path == DorestadBuilder.ScenePath ? "dorestad" : scene.path == OceanBuilder.ScenePath ? "northsea_south" : null;

        static void Check(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer)
                return;
            string level = LevelOf(scene);
            if (level == null || scene != SceneManager.GetActiveScene())
                return;
            if (Object.FindFirstObjectByType<KapmanGame>() == null)
                return;                                                      // not a built level yet
            bool changed = false;
            if (Object.FindFirstObjectByType<VoyageDirector>() == null || Object.FindFirstObjectByType<EncounterRunner>() == null ||
                Object.FindFirstObjectByType<EconomyDevKeys>() == null)
            {
                GameSetup.AddToScene(false);
                changed = true;
            }
            if (level == "dorestad")
            {
                var sounds = Object.FindFirstObjectByType<LevelSoundscape>();
                if (sounds != null && SoundsOutOfDate(sounds.GetComponent<LevelSoundscape2>()))
                {
                    var old = sounds.GetComponent<LevelSoundscape2>();
                    SoundscapeSetup.AddTo(sounds.gameObject, old != null ? old.shoreMap : null, old != null ? old.shoreSize : 5120f);
                    changed = true;
                }
            }
            if (Object.FindFirstObjectByType<WaterProfileDriver>() == null)
            {
                WaterProfileSetup.AddToOpenScene(level);
                changed = Object.FindFirstObjectByType<WaterProfileDriver>() != null || changed;
            }
            if (!changed)
                return;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Kapman: {scene.name} brought up to date with the game's newest parts (saved).");
        }

        /// <summary>Whether the scene's soundscape lacks a clip soundscape.json names (or is missing).</summary>
        static bool SoundsOutOfDate(LevelSoundscape2 sc)
        {
            if (sc == null || sc.data == null)
                return true;
            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(SoundscapeSetup.DataPath);
            if (file == null)
                return false;
            if (file.text != sc.data.text)
                return true;
            try
            {
                var def = Kapman.Audio.SoundscapeDef.Parse(file.text);
                foreach (var id in def.ClipIds())
                {
                    bool found = false;
                    foreach (var c in sc.clips)
                        if (c.id == id && c.clip != null)
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                        return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Kapman: soundscape.json does not load: " + e.Message);
            }
            return false;
        }
    }
}
