using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// What the Dorestad terrain carries as details (grass, flowers, reeds): each prototype, how it renders, and how many
    /// cells hold it. Headless: Unity.exe -batchmode -nographics -projectPath ... -executeMethod Kapman.EditorTools.DetailCheck.Run -quit
    /// </summary>
    public static class DetailCheck
    {
        [MenuItem("Kapman/Levels/Check Dorestad's details")]
        public static void Run()
        {
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(DorestadBuilder.LevelDir + "/Dorestad_TerrainData.asset");
            if (td == null)
            {
                Debug.LogError("Kapman details: no Dorestad terrain data.");
                return;
            }
            var sb = new StringBuilder();
            var protos = td.detailPrototypes;
            sb.AppendLine($"Kapman details: {protos.Length} prototypes, resolution {td.detailResolution} ({td.detailResolutionPerPatch} a patch), scatter {td.detailScatterMode}");
            for (int i = 0; i < protos.Length; i++)
            {
                var p = protos[i];
                int cells = 0;
                long sum = 0;
                var layer = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, i);
                foreach (int v in layer)
                    if (v > 0) { cells++; sum += v; }
                string what = p.usePrototypeMesh ? (p.prototype != null ? AssetDatabase.GetAssetPath(p.prototype) : "MISSING MESH") : (p.prototypeTexture != null ? p.prototypeTexture.name : "MISSING TEXTURE");
                string mat = "";
                if (p.prototype != null)
                {
                    var r = p.prototype.GetComponentInChildren<MeshRenderer>();
                    var mf = p.prototype.GetComponentInChildren<MeshFilter>();
                    mat = r != null && r.sharedMaterial != null ? $"{r.sharedMaterial.shader.name} (instancing {r.sharedMaterial.enableInstancing})" : "no renderer";
                    mat += mf != null && mf.sharedMesh != null ? $", mesh {mf.sharedMesh.name} {mf.sharedMesh.vertexCount} verts, size {mf.sharedMesh.bounds.size} at scale {mf.transform.lossyScale}" : ", no mesh";
                    mat += $", lod group {p.prototype.GetComponent<LODGroup>() != null}, root renderer {p.prototype.GetComponent<MeshRenderer>() != null}";
                }
                sb.AppendLine($"  [{i}] {what}: {p.renderMode}, instancing {p.useInstancing}, density {p.density:0.###}, h {p.minHeight:0.##}-{p.maxHeight:0.##}, cells {cells}, mean {(cells > 0 ? (double)sum / cells : 0):0.0}; {mat}");
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>The meadow pack's grass on a terrain built before 4 Oct's tuning: the short grass two and a half times as
        /// thick, the flowering grass knee to thigh high.</summary>
        public static void TuneMeadow()
        {
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(DorestadBuilder.LevelDir + "/Dorestad_TerrainData.asset");
            var protos = td.detailPrototypes;
            if (protos.Length < 2 || protos[0].prototype == null || !AssetDatabase.GetAssetPath(protos[0].prototype).Contains("NatureManufacture"))
            {
                Debug.Log("Kapman details: not the meadow pack's grass; nothing to tune.");
                return;
            }
            var grass = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, 0);
            // (4 Oct, second look: the two grasses were swapped and flat; the crossed cards, the sward short and the hay taller)
            var sward = MeadowPack.Prefab("Grass/Prefabs Unity Terrain Grass/prefab_Terrain_grass_meadow_02_cross_1.prefab");
            var hay = MeadowPack.Prefab("Grass/Prefabs Unity Terrain Grass/prefab_Terrain_grass_meadow_01_cross_1.prefab");
            bool widened = protos[0].minWidth >= 1.2f;
            if (sward != null) { protos[0].prototype = sward; protos[0].minHeight = 0.6f; protos[0].maxHeight = 1.0f; protos[0].minWidth = 1.2f; protos[0].maxWidth = 1.6f; }
            if (!widened)
                for (int z = 0; z < grass.GetLength(0); z++)
                    for (int x = 0; x < grass.GetLength(1); x++)
                        grass[z, x] = Mathf.Min(32, grass[z, x] * 3 / 2);   // once: half again as thick
            if (hay != null) { protos[1].prototype = hay; protos[1].minHeight = protos[1].minWidth = 0.45f; protos[1].maxHeight = protos[1].maxWidth = 0.65f; }
            td.detailPrototypes = protos;
            td.SetDetailLayer(0, 0, 0, grass);
            EditorUtility.SetDirty(td);
            AssetDatabase.SaveAssets();
            Run();
        }

        /// <summary>The terrain's layers hold plants per cell: switch the saved terrain to instance counts, keeping every layer's
        /// numbers (a fix for terrains built before 4 Oct, which Unity read as coverage and drew almost bare).</summary>
        [MenuItem("Kapman/Levels/Fix Dorestad's details (counts, not coverage)")]
        public static void UseCounts()
        {
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(DorestadBuilder.LevelDir + "/Dorestad_TerrainData.asset");
            if (td == null || td.detailScatterMode == DetailScatterMode.InstanceCountMode)
            {
                Debug.Log("Kapman details: nothing to change.");
                return;
            }
            int n = td.detailPrototypes.Length;
            var layers = new int[n][,];
            for (int i = 0; i < n; i++)
                layers[i] = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, i);
            td.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
            for (int i = 0; i < n; i++)
                td.SetDetailLayer(0, 0, i, layers[i]);
            EditorUtility.SetDirty(td);
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman details: {n} layers now read as plants per cell.");
            Run();
        }
    }
}
