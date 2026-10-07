using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// SMALL TOOLS FOR THE WATER - HDRP water decals (a patch of foam or a heap of water laid on the surface) and the
    /// little textures they use, shared by the ship's wake, the river's drifting foam and the shore. The textures are
    /// drawn here, not loaded: a patch of foam, a strike's heap of water, a soft drop for spray.
    /// HDRP's "Water Decal Sample" shader, Texture type: the deformation texture's red is the height in metres (times the
    /// decal's amplitude); the foam texture's red is surface foam and green deep foam (times the decal's dimmers).
    /// </summary>
    public static class WaterDecalKit
    {
        public const string DeformationTexture = "_Deformation_Texture";
        public const string FoamTexture = "_Foam_Texture";

        /// <summary>A water decal on the surface: silent (amplitude and foam 0) until its owner turns it up.</summary>
        public static WaterDecal MakeDecal(string name, Transform parent, Material material, Vector2 size, Vector2Int texels)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var decal = go.AddComponent<WaterDecal>();
            decal.material = material;
            decal.regionSize = size;
            decal.resolution = texels;
            decal.updateMode = CustomRenderTextureUpdateMode.OnDemand;
            decal.amplitude = 0f;
            decal.surfaceFoamDimmer = 0f;
            decal.deepFoamDimmer = 0f;
            decal.RequestUpdate();
            return decal;
        }

        /// <summary>Gives a decal its own textures through a property block (the material asset stays as it is).</summary>
        public static void SetTextures(WaterDecal decal, MaterialPropertyBlock block, Texture deformation, Texture foam)
        {
            block.SetTexture(DeformationTexture, deformation != null ? deformation : Texture2D.blackTexture);
            block.SetTexture(FoamTexture, foam != null ? foam : Texture2D.blackTexture);
            decal.SetPropertyBlock(block);
            decal.RequestUpdate();
        }

        static Texture2D foamPatch, softDot;

        /// <summary>A soft patch of foam, broken into streaks and islands along its length; deep foam under it all.</summary>
        public static Texture2D FoamPatch()
        {
            if (foamPatch != null)
                return foamPatch;
            const int n = 64;
            var px = new Color32[n * n];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    float x = (i + 0.5f) / n * 2f - 1f, z = (j + 0.5f) / n * 2f - 1f;
                    float r = Mathf.Sqrt(x * x * 1.1f + z * z * 0.55f);
                    float v = Mathf.Clamp01(1f - r);
                    v = v * v * (3f - 2f * v);
                    float streaks = (float)(0.6 * Noise(i * 0.35, j * 0.12, 5) + 0.4 * Noise(i * 0.8, j * 0.3, 6));
                    float islands = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.7f, streaks));
                    px[j * n + i] = new Color32(B(v * islands), B(Mathf.Clamp01(v * 1.3f)), 0, 255);
                }
            foamPatch = Upload(px, n, n, "Foam patch");
            return foamPatch;
        }

        /// <summary>A strike's heap of water (a smooth mound, 1 m at its middle) and its foam (broken white, deep foam under it).</summary>
        public static void StrikeTextures(out Texture2D heap, out Texture2D froth)
        {
            const int n = 32;
            var heights = new float[n * n];
            var px = new Color32[n * n];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    float x = (i + 0.5f) / n * 2f - 1f, z = (j + 0.5f) / n * 2f - 1f;
                    float r2 = x * x + z * z;
                    heights[j * n + i] = Mathf.Exp(-r2 / 0.18f) * Mathf.Clamp01((1f - r2) * 4f);
                    float v = Mathf.Clamp01(1f - Mathf.Sqrt(r2));
                    v = v * v * (3f - 2f * v);
                    float broken = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 0.65f, (float)Noise(i * 0.45, j * 0.45, 17)));
                    px[j * n + i] = new Color32(B(v * broken * 1.2f), B(v), 0, 255);
                }
            heap = HeightTexture(heights, n, n, "A strike's heap of water");
            froth = Upload(px, n, n, "A strike's foam");
        }

        /// <summary>A soft round dot for spray droplets.</summary>
        public static Texture2D SoftDot()
        {
            if (softDot != null)
                return softDot;
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, true, true) { name = "Spray drop", wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a * 1.6f) * 255f));
                }
            t.SetPixels32(px);
            t.Apply(true, true);
            softDot = t;
            return t;
        }

        /// <summary>Heights in metres as a one-channel float texture (rows south to north).</summary>
        public static Texture2D HeightTexture(float[] heights, int nx, int nz, string name)
        {
            var t = new Texture2D(nx, nz, TextureFormat.RFloat, false, true)
                { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            t.SetPixelData(heights, 0);
            t.Apply(false, true);
            return t;
        }

        /// <summary>Colour bytes as a texture (linear, no mips).</summary>
        public static Texture2D Upload(Color32[] px, int nx, int nz, string name)
        {
            var t = new Texture2D(nx, nz, TextureFormat.RGBA32, false, true)
                { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        public static byte B(float v) => (byte)Mathf.Clamp(Mathf.RoundToInt(v * 255f), 0, 255);

        /// <summary>Smooth value noise, 0-1 (safe on any thread).</summary>
        public static double Noise(double x, double z, int seed)
        {
            long ix = (long)Math.Floor(x), iz = (long)Math.Floor(z);
            double tx = x - ix, tz = z - iz;
            tx = tx * tx * (3.0 - 2.0 * tx);
            tz = tz * tz * (3.0 - 2.0 * tz);
            double a = Hash(ix, iz, seed), b = Hash(ix + 1, iz, seed), c = Hash(ix, iz + 1, seed), d = Hash(ix + 1, iz + 1, seed);
            double top = a + (b - a) * tx, bottom = c + (d - c) * tx;
            return top + (bottom - top) * tz;
        }

        public static double Hash(long x, long z, int seed)
        {
            unchecked
            {
                ulong h = (ulong)(x * 73856093L) ^ (ulong)(z * 19349663L) ^ (ulong)(seed * 83492791L) ^ 0x9E3779B97F4A7C15UL;
                h ^= h >> 33;
                h *= 0xff51afd7ed558ccdUL;
                h ^= h >> 33;
                return (h & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }
    }
}
