using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>A check on what was painted: each terrain layer's share, and a picture of the dominant layer per texel.</summary>
    public static class TerrainCheck
    {
        [MenuItem("Kapman/Levels/Check Dorestad's paint")]
        public static void Dorestad() => Check(DorestadBuilder.LevelDir + "/Dorestad_TerrainData.asset", Path.Combine(Shots.OutDir, "check_paint.png"));

        public static void Check(string assetPath, string pngPath)
        {
            var td = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);
            if (td == null)
            {
                Debug.LogError("Kapman check: no terrain data at " + assetPath);
                return;
            }
            int res = td.alphamapResolution;
            var a = td.GetAlphamaps(0, 0, res, res);
            int layers = td.alphamapLayers;
            var sums = new double[layers];
            var colours = new Color32[layers];
            var rnd = new System.Random(5);
            for (int k = 0; k < layers; k++)
                colours[k] = Color.HSVToRGB((k * 0.618f) % 1f, 0.7f, 0.9f);
            int step = Mathf.Max(1, res / 1024);
            var tex = new Texture2D(res / step, res / step, TextureFormat.RGB24, false);
            var px = new Color32[(res / step) * (res / step)];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    int best = 0;
                    float bw = -1f;
                    for (int k = 0; k < layers; k++)
                    {
                        float w = a[z, x, k];
                        sums[k] += w;
                        if (w > bw) { bw = w; best = k; }
                    }
                    if (z % step == 0 && x % step == 0)
                        px[(z / step) * (res / step) + x / step] = colours[best];
                }
            tex.SetPixels32(px);
            tex.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(pngPath));
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            double total = sums.Sum();
            var names = td.terrainLayers.Select(l => l != null ? l.name : "null").ToArray();
            string report = string.Join("\n", Enumerable.Range(0, layers).Select(k => $"  {k,2} {names[k],-28} {100.0 * sums[k] / total,6:0.0}%  colour {ColorUtility.ToHtmlStringRGB(colours[k])}"));
            Debug.Log($"Kapman check: {layers} layers, alphamap {res}:\n{report}\npicture: {pngPath}");
        }
    }

    public static class BatchCheck
    {
        public static void DorestadPaint()
        {
            try { TerrainCheck.Dorestad(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
    }
}
