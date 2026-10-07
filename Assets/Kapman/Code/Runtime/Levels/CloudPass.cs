using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THROUGH THE CLOUDS (the user, 4 Oct pm: "a shifting through the clouds animation that brings you to the 2D map
    /// management mode"): while the camera rises from the ship to the chart (and falls back), layers of cloud thicken,
    /// drift past and thin again over the screen. Amount (0 clear, 1 white-out) is set each frame by whoever moves the
    /// camera (the map's rise and fall); this draws it: two soft noise layers, tinted by the sky, sliding apart as they
    /// pass so it reads as going through them, not as a fade.
    /// </summary>
    public class CloudPass : MonoBehaviour
    {
        /// <summary>How thick the cloud is over the screen now (0-1).</summary>
        public static float Amount;
        /// <summary>Which way the camera goes through it: +1 rising (the clouds slide down and outward), -1 falling.</summary>
        public static float Direction = 1f;

        static Texture2D near, far;
        float drift;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Make()
        {
            if (FindFirstObjectByType<CloudPass>() != null)
                return;
            var go = new GameObject("Cloud pass (map transition)") { hideFlags = HideFlags.DontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<CloudPass>();
        }

        /// <summary>A tileable soft cloud texture: summed octaves of Perlin noise, shaped into puffs.</summary>
        static Texture2D Clouds(int size, float scale, int seed)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size, n = 0f, a = 0.5f, f = scale;
                    for (int o = 0; o < 4; o++)
                    {
                        // tileable: blend the noise at the four wrapped corners
                        float s0 = Mathf.PerlinNoise(seed + u * f, seed + v * f), s1 = Mathf.PerlinNoise(seed + (u - 1f) * f, seed + v * f);
                        float s2 = Mathf.PerlinNoise(seed + u * f, seed + (v - 1f) * f), s3 = Mathf.PerlinNoise(seed + (u - 1f) * f, seed + (v - 1f) * f);
                        n += a * Mathf.Lerp(Mathf.Lerp(s0, s1, u), Mathf.Lerp(s2, s3, u), v);
                        a *= 0.5f;
                        f *= 2f;
                    }
                    float puff = Mathf.SmoothStep(0.38f, 0.78f, n);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(255 * puff));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        void OnGUI()
        {
            if (Amount <= 0.002f || Event.current.type != EventType.Repaint)
                return;
            if (near == null)
            {
                near = Clouds(256, 3f, 17);
                far = Clouds(256, 5f, 91);
            }
            drift += Time.unscaledDeltaTime * Direction;
            float a = Mathf.Clamp01(Amount);
            // the cloud's colour: the sky's light, a little warm in the day, blue-grey at night
            float day = 1f - LevelClock.Night;
            var tint = Color.Lerp(new Color(0.42f, 0.46f, 0.55f), new Color(0.93f, 0.92f, 0.89f), day);
            float w = Screen.width, h = Screen.height;
            var old = GUI.color;
            // a soft base so the peak is a full white-out, then the two layers sliding apart
            GUI.color = new Color(tint.r, tint.g, tint.b, Mathf.SmoothStep(0.25f, 1f, a));
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            float grow = 1f + 0.35f * a;
            GUI.color = new Color(tint.r, tint.g, tint.b, a);
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, w, h), far, new Rect(drift * 0.03f, -drift * 0.05f, 1.6f / grow, 0.9f / grow));
            GUI.color = new Color(tint.r * 0.97f, tint.g * 0.97f, tint.b, Mathf.Clamp01(a * 1.2f));
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, w, h), near, new Rect(-drift * 0.06f, drift * 0.11f, 1.1f / grow, 0.62f / grow));
            GUI.color = old;
        }
    }
}
