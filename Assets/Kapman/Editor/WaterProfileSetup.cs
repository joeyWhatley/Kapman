using Kapman.Levels.Water;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.EditorTools
{
    /// <summary>
    /// Gives a level's water its record from Data/Water/water_793.json (WaterProfileDriver), by the level's water.profile in
    /// chain.json: on the river surface (the one with a RiverCurrent) or the sea surface (the one with an HdrpSea). The
    /// builders call it before saving; Kapman > Levels > Give the levels their water does it on the saved scenes.
    /// </summary>
    public static class WaterProfileSetup
    {
        public const string DataPath = "Assets/Kapman/Data/Water/water_793.json";

        public static void AddToOpenScene(string levelId)
        {
            var level = Kapman.Chain.ChainData.Load(Kapman.Chain.ChainData.ProjectPath).Level(levelId);
            string record = level != null && level.water != null ? level.water.profile : "";
            if (string.IsNullOrEmpty(record))
            {
                Debug.LogWarning($"Kapman water: chain.json gives '{levelId}' no water profile.");
                return;
            }
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
            int n = 0;
            foreach (var surface in Object.FindObjectsByType<WaterSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                bool river = surface.GetComponent<RiverCurrent>() != null, sea = surface.GetComponent<HdrpSea>() != null;
                if (!river && !sea)
                    continue;
                var d = surface.GetComponent<WaterProfileDriver>() ?? surface.gameObject.AddComponent<WaterProfileDriver>();
                d.data = data;
                d.recordId = record;
                EditorUtility.SetDirty(d);
                n++;
            }
            Debug.Log($"Kapman water: '{levelId}' takes its water from '{record}' on {n} surface(s).");
        }

        [MenuItem("Kapman/Levels/Give the levels their water")]
        public static void AddToSavedScenes()
        {
            foreach (var (scene, level) in new[] { (DorestadBuilder.ScenePath, "dorestad"), ("Assets/Kapman/Scenes/L_NorthSea.unity", "northsea_south") })
            {
                var s = EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
                AddToOpenScene(level);
                EditorSceneManager.MarkSceneDirty(s);
                EditorSceneManager.SaveScene(s);
            }
        }
    }
}
