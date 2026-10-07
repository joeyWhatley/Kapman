using System.Collections.Generic;
using System.Text;
using Kapman.Audio;
using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE MODELLED SOUNDSCAPE (audio design nights one and two): each frame it reads where the listener is and what the
    /// weather does, asks SoundscapeModel for every layer's level in soundscape.json, and sets its own sources. Night one:
    /// the wind as an equal-power crossfade on the Beaufort force, the rain three ways, the river lapping by the height
    /// above the water and the distance to the shore (from the level's shore map), and the thunder after the flash at the
    /// speed of sound in the air's temperature, scheduled on the audio clock, the far rumbles low-passed by the air's
    /// absorption (ISO 9613-1) and the near cracks ducking the rest. Night two: the wind in the reeds, the leaves or the
    /// bare trees and the dry grass round the listener; the rain on thatch and on water; the player's ship (her bow wave
    /// by her speed, the oars while she is rowed, her hull working as she rolls and pitches), on her; the wildlife of 793
    /// as voices (SoundVoices) by season, hour and weather; the air's humidity from the kind of weather. The surroundings
    /// are guessed from the shore distance until night three's cover map. LevelSoundscape leaves all of this to it, and
    /// keeps only what this does not do yet. CC0 recordings only, never made or generated.
    /// </summary>
    public class LevelSoundscape2 : MonoBehaviour
    {
        [System.Serializable]
        public struct ClipRef
        {
            public string id;
            public AudioClip clip;
        }

        public static LevelSoundscape2 Current { get; private set; }
        /// <summary>The three loudest layers and their dB, for the H panel.</summary>
        public static string Loudest { get; private set; } = "";

        // ---- where there is no 3D level (the chart, 7 Oct 2026): the game feeds what the listener is in, and the level's
        // own weather, clock, shore map and hull are not asked. All null and 1 in a level: nothing there changes.
        /// <summary>Who fills the context in place of Context(at). Null in a level.</summary>
        public static System.Func<SoundContext> Feed;
        /// <summary>The wind the far calls bend in when there is a feed (x east, y north, m/s: where it blows to).</summary>
        public static Vector2 FeedWind;
        /// <summary>The layers that are silent where the listener is now, by id, when there is a feed (the chart shuts the
        /// land's birds on the open sea: several of them ask nothing of the place and would sing there). Null: none.</summary>
        public static System.Func<string, bool> Shut;
        /// <summary>How often the voices try when there is a feed, times their own pace (the chart: half, beside a game
        /// of reading).</summary>
        public static float CallRate = 1f;

        public TextAsset data;
        public ClipRef[] clips = new ClipRef[0];
        [Tooltip("The signed distance to the shore (R, metres: + over water, - inland), covering shoreSize metres square from the level's origin.")]
        public Texture2D shoreMap;
        public float shoreSize = 5120f;
        [Range(0f, 1f)] public float master = 0.7f;
        [Tooltip("The relative humidity when there is no weather to take it from (it follows the kind of weather otherwise).")]
        [Range(0.1f, 1f)] public float humidity = 0.8f;
        [Tooltip("How many wildlife calls can sound at once.")]
        [Range(1, 24)] public int voicePool = 12;
        [Tooltip("Play nothing until something feeds it (the chart: ChartSound), in place of the level's own weather and clock.")]
        public bool waitForFeed;

        sealed class Live
        {
            public SoundLayerDef layer;
            public AudioSource[] sources;
            public float[] weights;
            public float db = -80f;
            public bool started;
        }

        SoundscapeDef def;
        readonly List<Live> live = new List<Live>();
        readonly Dictionary<string, AudioClip> clipById = new Dictionary<string, AudioClip>();
        SoundVoices voices;
        SoundLayerDef thunderLayer;
        AudioSource[] thunder;
        AudioLowPassFilter[] thunderFilter;
        int nextThunder;
        float duckStart = -1f, duckDb, duckHold, duckRelease;
        LevelWeather hooked;
        float lastText;
        float rh = 0.8f;
        WeatherKind? lastKind;
        string kindName = "Fair";
        AnimationCurve flat;
        ShipMotion lastShip;
        float lastRoll, lastPitch, rollRate, pitchRate;
        float lastTemperature = 12f;

        /// <summary>A layer's level now (dB; -80 silent, or not a bed or a crossfade of this soundscape): for tests and tools.</summary>
        public float LevelOf(string id)
        {
            foreach (var l in live)
                if (l.layer.id == id)
                    return l.db;
            return -80f;
        }

        /// <summary>How many wildlife calls sound now, and the voice that called last ("" for none yet).</summary>
        public int Calling => voices != null ? voices.Calling : 0;
        public string LastCalled => voices != null ? voices.LastCalled : "";

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            if (hooked != null)
                hooked.Lightning -= OnLightning;
            hooked = null;
        }

        void Awake()
        {
            if (data == null)
            {
                Debug.LogWarning("Kapman sound: no soundscape.json given; the modelled sounds are off.");
                enabled = false;
                return;
            }
            try
            {
                def = SoundscapeDef.Parse(data.text);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Kapman sound: soundscape.json does not load: " + e.Message);
                enabled = false;
                return;
            }
            rh = humidity;
            flat = AnimationCurve.Constant(0f, 1f, 1f);
            Clips();
            foreach (var layer in def.layers)
            {
                if (layer.kind == "event")
                {
                    if (layer.on == "lightning")
                        thunderLayer = layer;
                    continue;
                }
                if (layer.kind == "voice" || layer.kind == "group")
                    continue;
                var l = new Live { layer = layer, sources = new AudioSource[layer.clips.Length], weights = new float[layer.clips.Length] };
                for (int i = 0; i < layer.clips.Length; i++)
                    l.sources[i] = Source($"{layer.id}: {layer.clips[i]}", Clip(layer.clips[i]), true, layer.spatial);
                live.Add(l);
            }
            voices = new SoundVoices(def, transform, Clip, flat, voicePool);
            if (thunderLayer != null)
            {
                thunder = new AudioSource[3];
                thunderFilter = new AudioLowPassFilter[3];
                for (int i = 0; i < thunder.Length; i++)
                {
                    thunder[i] = Source("thunder " + (i + 1), null, false, 0f);
                    thunderFilter[i] = thunder[i].gameObject.AddComponent<AudioLowPassFilter>();
                    thunderFilter[i].lowpassResonanceQ = 1f;
                }
            }
        }

        /// <summary>The clips by id: those the scene was given (SoundscapeSetup.AddTo), and in the editor any the data names
        /// that the scene does not have yet, straight from Assets/Kapman/Audio/Sounds (run the setup again to keep them).</summary>
        void Clips()
        {
            foreach (var c in clips)
                if (!string.IsNullOrEmpty(c.id) && c.clip != null)
                    clipById[c.id] = c.clip;
            var missing = new List<string>();
            foreach (var id in def.ClipIds())
            {
                if (clipById.ContainsKey(id) || missing.Contains(id))
                    continue;
#if UNITY_EDITOR
                var found = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Kapman/Audio/Sounds/{id}.mp3");
                if (found != null)
                {
                    clipById[id] = found;
                    continue;
                }
#endif
                missing.Add(id);
            }
            if (missing.Count > 0)
                Debug.LogWarning($"Kapman sound: {missing.Count} clips are not given to the soundscape (Kapman > Levels > Add the soundscape to Dorestad): {string.Join(", ", missing)}");
        }

        AudioSource Source(string name, AudioClip clip, bool loop, float spatial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var a = go.AddComponent<AudioSource>();
            a.clip = clip;
            a.loop = loop;
            a.playOnAwake = false;
            a.volume = 0f;
            a.spatialBlend = spatial;
            if (spatial > 0f)
            {
                // placed in the world (the ship's sounds): panned there, but the level is the model's, so no rolloff
                a.dopplerLevel = 0f;
                a.rolloffMode = AudioRolloffMode.Custom;
                a.SetCustomCurve(AudioSourceCurveType.CustomRolloff, flat);
                a.minDistance = 1f;
                a.maxDistance = 20000f;
            }
            return a;
        }

        AudioClip Clip(string id) => id != null && clipById.TryGetValue(id, out var c) ? c : null;

        SoundContext Context(Vector3 at)
        {
            var w = LevelWeather.Current;
            var clock = LevelClock.Current;
            var info = LevelInfo.Current;
            float hour = clock != null ? clock.HourOfDay : 12f;
            float sun = clock != null ? LevelClock.SunAltitude : 45f;
            if (w != null)
            {
                if (lastKind != w.kind)
                {
                    lastKind = w.kind;
                    kindName = w.kind.ToString();
                }
                rh = SoundscapeModel.HumidityFromWeather(kindName, hour, sun);
            }
            else
                rh = humidity;
            var c = new SoundContext
            {
                wind = w != null ? w.Wind : 3f,
                rain = w != null ? w.Rain : 0f,
                storm = w != null ? w.Storm : 0f,
                temperature = w != null ? w.Temperature : 12f,
                humidity = rh,
                cloud = w != null ? w.CloudCover : 0.3f,
                leaf = w != null ? w.Leaf : 1f,
                night = LevelClock.Night,
                day = clock != null ? clock.DayOfYear : 172f,
                hour = hour,
                sunAltitude = sun,
                aboveWater = info != null ? at.y - info.WaterLevel : 20f,
                shore = Shore(at, info),
            };
            SoundscapeModel.GuessSurroundings(ref c);
            Ship(ref c, at, info);
            return c;
        }

        /// <summary>The signed distance to the shore under a point (m), from the shore map; with none, the open sea in an
        /// ocean level and the bank (0) in a river place.</summary>
        float Shore(Vector3 p, LevelInfo info)
        {
            if (shoreMap == null || shoreSize <= 0f)
                return info != null && info.kind == LevelKind.Ocean ? 1000f : 0f;
            return shoreMap.GetPixelBilinear(p.x / shoreSize, p.z / shoreSize).r;
        }

        /// <summary>
        /// The player's ship for her layers (design 2.7): her speed through the water, her roll and pitch rates (by
        /// differencing, smoothed over a third of a second), the rowing (until the storms plan's ship state: rowed in a
        /// river place when she makes way, under sail on the sea), and her distance from the listener.
        /// </summary>
        void Ship(ref SoundContext c, Vector3 at, LevelInfo info)
        {
            var ship = ShipMotion.Player;
            if (ship == null)
            {
                lastShip = null;
                rollRate = pitchRate = 0f;
                c.shipDistance = 1e6f;
                return;
            }
            float dt = Time.deltaTime;
            if (ship == lastShip && dt > 1e-4f)
            {
                float k = 1f - Mathf.Exp(-dt / 0.33f);
                rollRate = Mathf.Lerp(rollRate, Mathf.Abs(Mathf.DeltaAngle(lastRoll, ship.Roll)) / dt, k);
                pitchRate = Mathf.Lerp(pitchRate, Mathf.Abs(Mathf.DeltaAngle(lastPitch, ship.Pitch)) / dt, k);
            }
            lastShip = ship;
            lastRoll = ship.Roll;
            lastPitch = ship.Pitch;
            float speed = LevelClock.Paused ? 0f : Mathf.Abs(ship.Speed);
            bool river = info == null || info.kind == LevelKind.RiverPlace;
            c.shipSpeed = speed;
            c.shipRollRate = rollRate;
            c.shipPitchRate = pitchRate;
            c.shipRowing = river ? Mathf.InverseLerp(0.05f, 0.6f, speed) : 0f;
            c.shipDistance = Vector3.Distance(at, ship.transform.position);
        }

        float DuckNow()
        {
            if (duckStart < 0f)
                return 0f;
            float t = Time.unscaledTime - duckStart;
            if (t < 0f)
                return 0f;
            if (t < duckHold)
                return duckDb;
            if (t < duckHold + duckRelease)
                return Mathf.Lerp(duckDb, 0f, (t - duckHold) / duckRelease);
            duckStart = -1f;
            return 0f;
        }

        void Update()
        {
            var w = LevelWeather.Current;
            if (w != hooked)
            {
                if (hooked != null)
                    hooked.Lightning -= OnLightning;
                if (w != null && thunderLayer != null)
                    w.Lightning += OnLightning;
                hooked = w;
            }
            var cam = Camera.main;
            bool fed = Feed != null;
            // (fed: the listener is aboard, where this object is; the chart's camera looks at nothing)
            Vector3 at = fed || cam == null ? transform.position : cam.transform.position;
            if (!fed && waitForFeed)
            {
                foreach (var l in live)
                    foreach (var s in l.sources)
                    {
                        s.volume = 0f;
                        if (s.isPlaying)
                            s.Pause();
                    }
                return;
            }
            var ctx = fed ? Feed() : Context(at);
            if (fed)
                rh = ctx.humidity;
            lastTemperature = ctx.temperature;
            float duck = DuckNow();
            float dt = Time.unscaledDeltaTime;
            var ship = ShipMotion.Player;
            foreach (var l in live)
            {
                float target = fed && Shut != null && Shut(l.layer.id) ? -80f : SoundscapeModel.LayerDb(l.layer, ctx);
                l.db = !l.started ? target : Mathf.MoveTowards(l.db, target, Mathf.Max(l.layer.smoothDbPerS, 0.1f) * dt * (target < -60f || l.db < -60f ? 20f : 1f));
                l.started = true;
                if (l.layer.kind == "crossfade")
                    SoundscapeModel.Crossfade(ctx.Get(l.layer.FadeKey), l.layer.points, l.weights);
                else
                    l.weights[0] = 1f;
                if (l.layer.OnShip && (ship != null || fed))
                {
                    Vector3 on = fed ? at : ship.transform.position;     // (fed: she is where the ear is)
                    for (int i = 0; i < l.sources.Length; i++)
                        l.sources[i].transform.position = on;
                }
                float gain = master * SoundscapeModel.DbToGain(l.db + duck);
                for (int i = 0; i < l.sources.Length; i++)
                {
                    var s = l.sources[i];
                    float v = gain * l.weights[i];
                    s.volume = v;
                    if (v > 0.0005f && !s.isPlaying && s.clip != null)
                        s.Play();
                    else if (v <= 0.0005f && s.isPlaying)
                        s.Pause();
                }
            }
            var info = LevelInfo.Current;
            Vector2 windWay = w != null ? w.WindVector : fed ? FeedWind : Vector2.zero;
            voices?.Update(ctx, at, info != null ? info.WaterLevel : fed ? at.y - ctx.aboveWater : 0f, windWay, master, duck,
                           fed ? Shut : null, fed ? CallRate : 1f);
            if (Time.unscaledTime - lastText > 0.5f)
            {
                lastText = Time.unscaledTime;
                live.Sort((a, b) => b.db.CompareTo(a.db));
                var sb = new StringBuilder();
                for (int i = 0; i < live.Count && i < 3; i++)
                    if (live[i].db > -79f)
                        sb.Append(sb.Length > 0 ? "  ·  " : "").Append(live[i].layer.id).Append(' ').Append(live[i].db.ToString("0")).Append(" dB");
                string calls = voices != null && voices.LastCalled.Length > 0 ? $", {voices.Calling} calling, last {voices.LastCalled}" : "";
                string masked = SoundscapeModel.WildlifeOpen(ctx) <= 0f ? ", wildlife masked" : "";
                Loudest = $"{sb}  (Beaufort {SoundscapeModel.Beaufort(ctx.wind)}, RH {rh * 100f:0}%{(duck < 0f ? ", ducked" : "")}{masked}{calls})";
            }
        }

        /// <summary>A lightning flash this far off (m), from a scene with no LevelWeather to tell of it (the chart).</summary>
        public void Strike(float metres) => OnLightning(metres);

        // play mode starts without a domain reload: a new play has no feed left over
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearFeed()
        {
            Feed = null;
            Shut = null;
            FeedWind = Vector2.zero;
            CallRate = 1f;
        }

        void OnLightning(float metres)
        {
            if (!isActiveAndEnabled || thunder == null)
                return;
            var kind = SoundscapeModel.ThunderKind(metres);
            if (kind == SoundscapeModel.Thunder.Silent)
                return;
            var set = kind == SoundscapeModel.Thunder.Near ? thunderLayer.nearClips : thunderLayer.farClips;
            if (set.Length == 0)
                return;
            var clip = Clip(set[Random.Range(0, set.Length)]);
            if (clip == null)
                return;
            float t = LevelWeather.Current != null ? LevelWeather.Current.Temperature : lastTemperature;
            float delay = SoundscapeModel.ThunderDelay(metres, t);
            int i = nextThunder++ % thunder.Length;
            var s = thunder[i];
            s.Stop();
            s.clip = clip;
            s.volume = master * SoundscapeModel.DbToGain(SoundscapeModel.ThunderDb(metres));
            thunderFilter[i].cutoffFrequency = SoundscapeModel.AirAbsorptionCutoff(metres, t, rh);
            s.PlayScheduled(AudioSettings.dspTime + delay);
            if (kind == SoundscapeModel.Thunder.Near && metres < thunderLayer.duckWithinM)
            {
                duckStart = Time.unscaledTime + delay;
                duckDb = thunderLayer.duckDb;
                duckHold = thunderLayer.duckHoldS;
                duckRelease = Mathf.Max(0.1f, thunderLayer.duckReleaseS);
            }
        }
    }
}
