using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Levels
{
    /// <summary>
    /// THE SOUNDS - public-domain (CC0) recordings only, never made or generated (the rule of 2 October). The wind by its
    /// strength, the river lapping when you're near the water, rain, crickets on summer nights, birds one at a time
    /// somewhere round you (by day, by the water, at night), thunder after the lightning at the speed of sound, and the
    /// ship's oars and water under her keel by her speed. The town's own sounds (the smithy, the hearths) are sources
    /// placed in the scene. M mutes. Every volume here is low on purpose: they're to be tuned by ear.
    /// Where the modelled soundscape (LevelSoundscape2) is in the level, it owns the wind, the rain, the lapping, the
    /// thunder (night one), the birds, the oars and the water under the keel (night two): this then keeps only the M key.
    /// The crickets are retired from river places altogether: field crickets live on dry heath and sing by day and in the
    /// first part of the night, never in the wet Rhine delta (audio design 1.4).
    /// </summary>
    public class LevelSoundscape : MonoBehaviour
    {
        [Range(0f, 1f)] public float master = 0.7f;
        [Header("Loops")]
        public AudioSource wind;
        public AudioSource river;
        public AudioSource rain;
        public AudioSource crickets;
        public AudioSource oars;
        public AudioSource underKeel;
        [Header("One at a time")]
        public AudioSource birdVoice;
        public AudioSource thunderVoice;
        public AudioClip[] dayBirds;
        public AudioClip[] waterBirds;
        public AudioClip[] nightBirds;
        public AudioClip[] thunderNear;
        public AudioClip[] thunderFar;

        float nextBird = 5f;
        bool muted;
        LevelWeather hooked;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.mKey.wasPressedThisFrame && kb.shiftKey.isPressed)       // (Shift+M: M alone opens the map)
            {
                muted = !muted;
                AudioListener.volume = muted ? 0f : 1f;
            }
            var w = LevelWeather.Current;
            // the modelled soundscape (LevelSoundscape2) owns the wind, rain, lapping and thunder (night one) and the
            // birds, oars and water under the keel (night two) when it is here
            bool modelled = LevelSoundscape2.Current != null;
            if (hooked != null && (hooked != w || modelled))
            {
                hooked.Lightning -= OnLightning;
                hooked = null;
            }
            if (w != null && hooked != w && !modelled)
            {
                w.Lightning += OnLightning;
                hooked = w;
            }
            float windSpeed = w != null ? w.Wind : 3f;
            float rainAmount = w != null ? w.Rain : 0f;
            float night = LevelClock.Night;
            var clock = LevelClock.Current;
            int day = clock != null ? clock.DayOfYear : 172;
            var cam = Camera.main;
            var info = LevelInfo.Current;
            float above = cam != null && info != null ? cam.transform.position.y - info.WaterLevel : 20f;
            float near = Mathf.InverseLerp(300f, 25f, above);

            Set(wind, modelled ? 0f : 0.06f + 0.45f * Mathf.InverseLerp(1f, 18f, windSpeed));
            Set(river, modelled ? 0f : 0.30f * near);
            Set(rain, modelled ? 0f : 0.55f * rainAmount);
            float summerNights = Mathf.Clamp01(Mathf.InverseLerp(150f, 175f, day)) * Mathf.Clamp01(Mathf.InverseLerp(250f, 225f, day));
            bool riverPlace = info != null && info.kind == LevelKind.RiverPlace;
            Set(crickets, modelled || riverPlace ? 0f : 0.18f * night * summerNights * (1f - rainAmount) * Mathf.InverseLerp(500f, 50f, above));
            var ship = ShipMotion.Player;
            float rowing = ship != null ? Mathf.Clamp01(ship.Speed / 1.2f) : 0f;
            float shipNear = ship != null && cam != null ? Mathf.InverseLerp(120f, 15f, Vector3.Distance(cam.transform.position, ship.transform.position)) : 0f;
            Set(oars, modelled ? 0f : 0.35f * rowing * shipNear);
            Set(underKeel, modelled ? 0f : 0.3f * rowing * shipNear);

            if (!modelled && Time.time > nextBird && birdVoice != null && cam != null)
            {
                bool dark = night > 0.6f;
                var set = dark ? nightBirds : near > 0.5f && Random.value < 0.5f ? waterBirds : dayBirds;
                if (set != null && set.Length > 0 && rainAmount < 0.5f)
                {
                    var clip = set[Random.Range(0, set.Length)];
                    Vector2 around = Random.insideUnitCircle.normalized * Random.Range(30f, 140f);
                    birdVoice.transform.position = cam.transform.position + new Vector3(around.x, Random.Range(3f, 30f), around.y);
                    birdVoice.PlayOneShot(clip, master * Random.Range(0.25f, 0.5f));
                }
                nextBird = Time.time + Random.Range(8f, 25f) * (rainAmount > 0.3f ? 3f : 1f) * (dark ? 2f : 1f);
            }
        }

        void Set(AudioSource s, float volume)
        {
            if (s == null)
                return;
            s.volume = master * volume;
            if (volume > 0.001f && !s.isPlaying && s.clip != null)
                s.Play();
            else if (volume <= 0.001f && s.isPlaying)
                s.Pause();
        }

        void OnLightning(float distance)
        {
            if (isActiveAndEnabled)
                StartCoroutine(Thunder(distance));
        }

        IEnumerator Thunder(float distance)
        {
            yield return new WaitForSeconds(distance / 340f);
            var set = distance < 1500f ? thunderNear : thunderFar;
            if (thunderVoice != null && set != null && set.Length > 0)
                thunderVoice.PlayOneShot(set[Random.Range(0, set.Length)], master * Mathf.Lerp(0.9f, 0.35f, distance / 6000f));
        }
    }
}
