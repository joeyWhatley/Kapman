using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kapman.Levels
{
    /// <summary>
    /// THE LEVELS' DRAW DISTANCES IN PLAY (4 Oct, the user: "the fps is atrocious ... very jagged"): the meadow's grass
    /// was drawn to 260 m at up to 32 plants a 2.5 m cell over 23 layers (hundreds of thousands of cards, and beyond
    /// about 100 m a grass card is smaller than a pixel: it only shimmers), and 400 trees at full detail. Every level
    /// loaded in Play takes these limits; the builders write the same numbers for the next rebuild.
    /// </summary>
    public static class LevelPerformance
    {
        public const float GrassDistance = 110f, GrassDensity = 0.8f;
        public const int TreesAtFullDetail = 150;
        public const float TreeBillboardFrom = 350f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            Apply();
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
        }

        static void Loaded(Scene scene, LoadSceneMode mode) => Apply();

        public static void Apply()
        {
            foreach (var t in Terrain.activeTerrains)
            {
                if (t == null)
                    continue;
                t.detailObjectDistance = Mathf.Min(t.detailObjectDistance, GrassDistance);
                t.detailObjectDensity = Mathf.Min(t.detailObjectDensity, GrassDensity);
                t.treeMaximumFullLODCount = Mathf.Min(t.treeMaximumFullLODCount, TreesAtFullDetail);
                t.treeBillboardDistance = Mathf.Min(t.treeBillboardDistance, TreeBillboardFrom);
            }
        }
    }
}
