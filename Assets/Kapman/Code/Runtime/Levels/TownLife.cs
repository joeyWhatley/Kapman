using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE TOWN'S LIGHTS AND FIRES - hearths glow in the doorways all day and brighter as the light goes, lanterns are lit
    /// at dusk on the jetties and the moored boats, smoke rises from the roofs (thickest at the morning's and the evening's
    /// cooking) and leans with the wind, and bonfires burn on the eves of the feasts that had them (Feasts).
    /// </summary>
    [ExecuteAlways]
    public class TownLife : MonoBehaviour
    {
        public Light[] hearths;
        public Light[] lanterns;
        public ParticleSystem[] smokes;
        public GameObject[] bonfires;
        public Renderer[] flames;        // the fire baskets' and the bonfires' flame meshes: shown at dusk, flickering
        public float hearthStrength = 1.4f;
        public float lanternStrength = 2.4f;

        float[] seeds;
        bool[] tamed;
        float nextFar;
        /// <summary>Roofs farther than this from the camera stop smoking (their haze drifts off); doorway glows and
        /// lanterns farther than LightReach are off (HDRP lights every one on screen otherwise).</summary>
        public const float SmokeReach = 500f, LightReach = 220f;

        /// <summary>A roof's smoke as a thin haze: small, pale, short-lived puffs, a few at a time.</summary>
        static void Tame(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 12f;
            main.startSpeed = 0.45f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            var c = main.startColor.color;
            main.startColor = new Color(c.r, c.g, c.b, 0.28f);
            main.maxParticles = 24;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 3.5f));
        }

        /// <summary>Today's feast, if any (for the HUD).</summary>
        public static string FeastToday { get; private set; }

        void Update()
        {
            var clock = LevelClock.Current;
            float alt = LevelClock.SunAltitude;
            float dusk = Mathf.Clamp01(Mathf.InverseLerp(6f, -5f, alt));
            float hour = clock != null ? clock.HourOfDay : 12f;
            float t = Application.isPlaying ? Time.time : 0f;
            // distances from what the player looks at (the isometric camera stands hundreds of metres off)
            var cam = Camera.main;
            Vector3 eye = OrbitCamera.Current != null ? OrbitCamera.Current.Focus : cam != null ? cam.transform.position : transform.position;
            if (seeds == null || seeds.Length != (hearths != null ? hearths.Length : 0) + (lanterns != null ? lanterns.Length : 0))
            {
                int n = (hearths != null ? hearths.Length : 0) + (lanterns != null ? lanterns.Length : 0);
                seeds = new float[n];
                for (int i = 0; i < n; i++)
                    seeds[i] = i * 7.31f;
            }
            int s = 0;
            if (hearths != null)
                foreach (var h in hearths)
                {
                    float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(t * 3f, seeds[s++]);
                    if (h == null)
                        continue;
                    h.intensity = hearthStrength * (0.3f + 0.7f * dusk) * flicker;
                    h.enabled = !Application.isPlaying || (h.transform.position - eye).sqrMagnitude < LightReach * LightReach;   // a doorway's glow is not seen from afar
                }
            if (lanterns != null)
                foreach (var l in lanterns)
                {
                    float flicker = 0.9f + 0.1f * Mathf.PerlinNoise(t * 4f, seeds[s++]);
                    if (l == null)
                        continue;
                    l.intensity = lanternStrength * dusk * flicker;
                    l.enabled = dusk > 0.02f && (!Application.isPlaying || (l.transform.position - eye).sqrMagnitude < LightReach * LightReach);
                }
            if (flames != null)
                for (int i = 0; i < flames.Length; i++)
                {
                    var r = flames[i];
                    if (r == null)
                        continue;
                    r.enabled = dusk > 0.12f;
                    r.transform.localScale = Vector3.one * (0.85f + 0.15f * Mathf.PerlinNoise(t * 6f, i * 3.17f));
                }

            var weather = LevelWeather.Current;
            Vector2 wind = weather != null ? weather.WindVector : new Vector2(2f, 1f);
            float cooking = 0.35f + 0.65f * Mathf.Max(Bell(hour, 7f, 1.5f), Bell(hour, 18.5f, 2f));
            // (4 Oct, the user: "way too much smoke billowing out from all the buildings" and the fps): a house of 800 had
            // no chimney; the smoke seeped through the thatch as a thin haze over the ridge. Each roof's smoke is made thin,
            // short-lived and small once, and only the roofs near the camera emit; far ones let theirs drift away.
            bool slow = Application.isPlaying && Time.unscaledTime < nextFar;
            if (!slow)
                nextFar = Time.unscaledTime + 0.25f;
            if (smokes != null && !slow)
                for (int i = 0; i < smokes.Length; i++)
                {
                    var ps = smokes[i];
                    if (ps == null)
                        continue;
                    if (tamed == null || tamed.Length != smokes.Length)
                        tamed = new bool[smokes.Length];
                    if (!tamed[i] && Application.isPlaying)                // (in Play only: the editor must not rewrite the scene)
                    {
                        Tame(ps);
                        tamed[i] = true;
                    }
                    var em = ps.emission;
                    float near = !Application.isPlaying || (ps.transform.position - eye).sqrMagnitude < SmokeReach * SmokeReach ? 1f : 0f;
                    // every roof shows a haze at the cooking peaks; between them only every third (a hearth kept in)
                    em.rateOverTime = near * ((cooking > 0.7f || i % 3 == 0) ? 1.2f * cooking : 0f);
                    var force = ps.forceOverLifetime;
                    force.enabled = true;
                    force.x = wind.x * 0.2f;
                    force.z = wind.y * 0.2f;
                }

            FeastToday = null;
            bool fires = false;
            if (clock != null)
            {
                var date = clock.Date;
                var feast = Feasts.On(date.year, date.month, date.day);
                if (feast.HasValue)
                {
                    FeastToday = feast.Value.name;
                    fires = feast.Value.fires && dusk > 0.25f;
                }
            }
            if (bonfires != null)
                foreach (var b in bonfires)
                    if (b != null && b.activeSelf != fires)
                        b.SetActive(fires);
        }

        static float Bell(float x, float c, float w)
        {
            float d = (x - c) / w;
            return Mathf.Exp(-d * d);
        }
    }
}
