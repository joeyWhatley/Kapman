using Kapman.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>Opening the levels for a look, and bringing the saved scenes up to date without a rebuild.</summary>
    public static class ShowLevel
    {
        [MenuItem("Kapman/Levels/Open Dorestad")]
        public static void Dorestad() => EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);

        [MenuItem("Kapman/Levels/Open the North Sea")]
        public static void NorthSea() => EditorSceneManager.OpenScene(OceanBuilder.ScenePath, OpenSceneMode.Single);

        [MenuItem("Kapman/Levels/Open the 1:40 river (the scale test)")]
        public static void River40() => EditorSceneManager.OpenScene(ScaleTestBuilder.Scene(40), OpenSceneMode.Single);

        /// <summary>
        /// Both saved levels brought up to date (4 Oct): the game's parts (the voyage's beats, the encounters), the
        /// soundscape's clips, each level's water record; then Dorestad is left open for Play.
        /// </summary>
        [MenuItem("Kapman/Levels/Bring both levels up to date")]
        public static void BringUpToDate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Kapman: stop Play mode first.");
                return;
            }
            var n = EditorSceneManager.OpenScene(OceanBuilder.ScenePath, OpenSceneMode.Single);
            GameSetup.AddToScene(false);
            WaterProfileSetup.AddToOpenScene("northsea_south");
            EditorSceneManager.MarkSceneDirty(n);
            EditorSceneManager.SaveScene(n);
            var d = EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);
            GameSetup.AddToScene(false);
            var sounds = Object.FindFirstObjectByType<LevelSoundscape>();
            if (sounds != null)
            {
                var old = sounds.GetComponent<LevelSoundscape2>();
                SoundscapeSetup.AddTo(sounds.gameObject, old != null ? old.shoreMap : null, old != null ? old.shoreSize : 5120f);
            }
            WaterProfileSetup.AddToOpenScene("dorestad");
            EditorSceneManager.MarkSceneDirty(d);
            EditorSceneManager.SaveScene(d);
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman: both levels are up to date; Dorestad is open. Press Play.");
        }
    }
}
