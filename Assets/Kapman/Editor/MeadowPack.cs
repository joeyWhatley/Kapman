using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE MEADOW PACK - NatureManufacture's "Meadow Environment - Dynamic Nature" (Unity Asset Store, bought by the user
    /// on 4 October 2026; its HDRP 17.3 support pack imported over it). Scanned meadow grass and flowers, grey willow and
    /// field maple bushes, black poplars. It is not in git (Asset Store terms): on another machine, import it from the
    /// Asset Store and then its "HD RP 17.3 Unity 6.3" support pack. The builders fall back to the CC0 stand-ins without it.
    /// </summary>
    public static class MeadowPack
    {
        public const string Root = "Assets/NatureManufacture Assets/Meadow Environment Dynamic Nature/";

        /// <summary>Whether the pack is installed.</summary>
        public static bool Installed => AssetDatabase.IsValidFolder(Root.TrimEnd('/'));

        /// <summary>A prefab of the pack by its path under the pack's folder, or null if it is not installed.</summary>
        public static GameObject Prefab(string relative) => AssetDatabase.LoadAssetAtPath<GameObject>(Root + relative);

        /// <summary>The prefab's height in metres (its tallest mesh, at the prefab's own scale).</summary>
        public static float Height(GameObject prefab)
        {
            float tall = 0f;
            if (prefab == null)
                return 0f;
            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh != null)
                    tall = Mathf.Max(tall, mf.sharedMesh.bounds.max.y * mf.transform.lossyScale.y);
            return tall;
        }
    }
}
