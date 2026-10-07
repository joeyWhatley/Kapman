using System.Linq;
using Kapman.Audio;
using Kapman.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// Puts the modelled soundscape (LevelSoundscape2) into a level: its data, its clips (only those soundscape.json names,
    /// from Assets/Kapman/Audio/Sounds) and, for Dorestad, the shore map: the bake's signed distance to the water
    /// (water_sd.f32, 2.5 m cells) taken down to 512 x 512 at 10 m into a half-float texture the game reads (the game never
    /// reads G:\). Called by the Dorestad builder; Kapman > Levels > Add the soundscape to Dorestad does it on the saved scene.
    /// </summary>
    public static class SoundscapeSetup
    {
        public const string DataPath = "Assets/Kapman/Audio/soundscape.json";
        public const int ShoreCells = 512;

        /// <summary>The shore map from the bake (+ metres over water, - inland), saved beside the level's terrain.</summary>
        public static Texture2D BakeShoreMap(DorestadBuilder.Grid g)
        {
            var tex = new Texture2D(ShoreCells, ShoreCells, TextureFormat.RHalf, false, true)
            {
                name = "Dorestad shore distance",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var half = new ushort[ShoreCells * ShoreCells];
            float step = g.size / ShoreCells;
            for (int z = 0; z < ShoreCells; z++)
                for (int x = 0; x < ShoreCells; x++)
                    half[z * ShoreCells + x] = Mathf.FloatToHalf(g.SD(new Vector2((x + 0.5f) * step, (z + 0.5f) * step)));
            tex.SetPixelData(half, 0);
            tex.Apply(false, false);
            string path = DorestadBuilder.LevelDir + "/Dorestad_ShoreDistance.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(tex, path);
            return tex;
        }

        /// <summary>Adds (or refreshes) the modelled soundscape on a level's sound object.</summary>
        public static LevelSoundscape2 AddTo(GameObject sounds, Texture2D shoreMap, float shoreSize)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
            if (data == null)
                throw new System.Exception($"Kapman sound: {DataPath} is missing.");
            var def = SoundscapeDef.Parse(data.text);
            var sc = sounds.GetComponent<LevelSoundscape2>() ?? sounds.AddComponent<LevelSoundscape2>();
            sc.data = data;
            sc.clips = def.ClipIds().Distinct().Select(id =>
            {
                var clip = DorestadBuilder.Clip(id);
                if (clip == null)
                    Debug.LogWarning($"Kapman sound: no clip Assets/Kapman/Audio/Sounds/{id}.mp3");
                return new LevelSoundscape2.ClipRef { id = id, clip = clip };
            }).ToArray();
            sc.shoreMap = shoreMap;
            sc.shoreSize = shoreSize;
            EditorUtility.SetDirty(sc);
            Debug.Log($"Kapman sound: the modelled soundscape on '{sounds.name}': {def.layers.Count} layers, {sc.clips.Length} clips, shore map {(shoreMap != null ? "yes" : "none")}.");
            return sc;
        }

        [MenuItem("Kapman/Levels/Add the soundscape to Dorestad")]
        public static void AddToDorestad()
        {
            var scene = EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);
            var sounds = Object.FindFirstObjectByType<LevelSoundscape>();
            if (sounds == null)
                throw new System.Exception("Kapman sound: L_Dorestad has no LevelSoundscape object.");
            var g = DorestadBuilder.Load();
            AddTo(sounds.gameObject, BakeShoreMap(g), g.size);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
