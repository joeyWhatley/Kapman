using System.Collections.Generic;
using Kapman.Audio;
using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE VOICES (audio design 2.3, night two): the wildlife as one-shots, for LevelSoundscape2. Each voice in
    /// soundscape.json tries again after a random wait in its interval (shorter at 2x and 4x, never more than twice as
    /// often) and calls if its chance comes up: the chance is the product of its conditions (season, hour, surroundings,
    /// the wind) and the wildlife mask (silent at Beaufort 7 and over, thinned by rain). It calls from a random bearing,
    /// at a distance and a height above the water in its bands round the listener, as a stretch of its recording (a
    /// random cut of play_s seconds, faded in and out: an edit of the real recording, nothing made). Its level is its
    /// source level less 6 dB a doubling of distance, bent by the wind and a ground inversion over far paths, and its
    /// highs are taken by the air (ISO 9613-1) as the distance and humidity say. A group makes sure one of its members
    /// calls at least every min_interval_s while the group is on. Twelve sources in a pool; when all are busy a call is
    /// skipped. Doppler is off (design 2.6).
    /// </summary>
    sealed class SoundVoices
    {
        sealed class Voice
        {
            public SoundLayerDef layer;
            public float next;
            public readonly List<Group> groups = new List<Group>();
        }

        sealed class Group
        {
            public SoundLayerDef layer;
            public float last;
            public readonly List<Voice> members = new List<Voice>();
        }

        sealed class Slot
        {
            public AudioSource source;
            public AudioLowPassFilter filter;
            public Voice voice;
            public float start, length, fadeIn, fadeOut, nextCutoff;
        }

        const float CutoffEvery = 0.25f;

        readonly List<Voice> voices = new List<Voice>();
        readonly List<Group> groups = new List<Group>();
        readonly Slot[] slots;
        readonly System.Func<string, AudioClip> clipOf;
        readonly float splToDb;

        /// <summary>The voice that called last, and how many are calling now (for the H panel).</summary>
        public string LastCalled { get; private set; } = "";
        public int Calling { get; private set; }
        public int Count => voices.Count;

        public SoundVoices(SoundscapeDef def, Transform parent, System.Func<string, AudioClip> clipOf, AnimationCurve flat, int pool)
        {
            this.clipOf = clipOf;
            splToDb = def.splToDb;
            float now = Time.unscaledTime;
            var byId = new Dictionary<string, Voice>();
            foreach (var l in def.OfKind("voice"))
            {
                // a first try spread over the interval, so they do not all call at once on loading
                var v = new Voice { layer = l, next = now + Random.Range(1f, Mathf.Max(1f, l.intervalMax)) };
                voices.Add(v);
                byId[l.id] = v;
            }
            foreach (var l in def.OfKind("group"))
            {
                var g = new Group { layer = l, last = now };
                foreach (var id in l.members)
                    if (byId.TryGetValue(id, out var v))
                    {
                        g.members.Add(v);
                        v.groups.Add(g);
                    }
                groups.Add(g);
            }
            slots = new Slot[voices.Count > 0 ? pool : 0];
            for (int i = 0; i < slots.Length; i++)
            {
                var go = new GameObject("voice " + (i + 1));
                go.transform.SetParent(parent, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = false;
                s.volume = 0f;
                s.spatialBlend = 0.8f;
                s.dopplerLevel = 0f;
                s.spread = 0f;
                // the distance law is ours (the level is set every frame), so Unity's rolloff is flat
                s.rolloffMode = AudioRolloffMode.Custom;
                s.SetCustomCurve(AudioSourceCurveType.CustomRolloff, flat);
                s.minDistance = 1f;
                s.maxDistance = 20000f;
                var f = go.AddComponent<AudioLowPassFilter>();
                f.lowpassResonanceQ = 1f;
                f.cutoffFrequency = 22000f;
                slots[i] = new Slot { source = s, filter = f };
            }
        }

        /// <summary>
        /// One frame: the voices whose wait is up try to call, the groups keep their chorus going, and the calls under way
        /// are set to their level and low-pass for where the listener is now. `ground` is the water's level (the heights are
        /// above it); `wind` the wind's way (m/s, x and z: where it blows to); `gain` the master gain; `duckDb` the duck.
        /// </summary>
        /// <param name="shut">The voices that are silent where the listener is now, by id (null: none). A shut voice does
        /// not call, alone or for a group.</param>
        /// <param name="callRate">How often the voices try, times their own pace.</param>
        public void Update(SoundContext c, Vector3 listener, float ground, Vector2 wind, float gain, float duckDb,
                           System.Func<string, bool> shut = null, float callRate = 1f)
        {
            if (slots.Length == 0)
                return;
            float now = Time.unscaledTime;
            float rate = Mathf.Clamp(LevelClock.TimeFactor, 1f, 2f) * Mathf.Clamp(callRate, 0.05f, 4f);

            foreach (var v in voices)
            {
                if (now < v.next)
                    continue;
                var l = v.layer;
                v.next = now + Random.Range(l.intervalMin, l.intervalMax) / rate;
                if (shut != null && shut(l.id))
                    continue;
                float chance = l.When(c);
                if (chance > 0f && Random.value < chance)
                    Call(v, listener, ground, now);
            }

            foreach (var g in groups)
            {
                float every = g.layer.minIntervalS / rate;
                if (now - g.last < every)
                    continue;
                g.last = now - every + 2f;                                     // off, or nobody could call: look again in 2 s
                if (g.layer.When(c) <= 0f)
                    continue;
                float total = 0f;
                foreach (var m in g.members)
                    if (shut == null || !shut(m.layer.id))
                        total += m.layer.When(c);
                if (total <= 0f)
                    continue;
                float pick = Random.value * total;
                Voice chosen = null;
                foreach (var m in g.members)
                {
                    float w = shut != null && shut(m.layer.id) ? 0f : m.layer.When(c);
                    if (w <= 0f)
                        continue;
                    chosen = m;
                    pick -= w;
                    if (pick <= 0f)
                        break;
                }
                if (chosen != null)
                    Call(chosen, listener, ground, now);                       // a call sets every group's last of its voice
            }

            int calling = 0;
            foreach (var s in slots)
            {
                if (s.voice == null)
                    continue;
                float t = now - s.start;
                if (t >= s.length || (t > 0.5f && !s.source.isPlaying))
                {
                    s.source.Stop();
                    s.source.volume = 0f;
                    s.voice = null;
                    continue;
                }
                calling++;
                Vector3 toListener = listener - s.source.transform.position;
                float metres = Mathf.Max(1f, toListener.magnitude);
                var level = new Vector2(toListener.x, toListener.z);
                float windToward = level.sqrMagnitude > 1e-4f ? Vector2.Dot(wind, level.normalized) : 0f;
                float bent = SoundscapeModel.RefractionDb(metres, windToward, c, out float cutoffFactor);
                float db = SoundscapeModel.VoiceDb(s.voice.layer, metres, splToDb) + bent + duckDb;
                float envelope = Mathf.Clamp01(t / s.fadeIn) * Mathf.Clamp01((s.length - t) / s.fadeOut);
                s.source.volume = gain * SoundscapeModel.DbToGain(db) * envelope;
                if (now >= s.nextCutoff)
                {
                    s.nextCutoff = now + CutoffEvery;
                    float hz = SoundscapeModel.AirAbsorptionCutoff(metres, c.temperature, c.humidity) * cutoffFactor;
                    s.filter.cutoffFrequency = Mathf.Clamp(hz, 100f, 22000f);
                }
            }
            Calling = calling;
        }

        /// <summary>Starts a call of this voice from a free source; false when none is free or its clip is missing.</summary>
        bool Call(Voice v, Vector3 listener, float ground, float now)
        {
            Slot slot = null;
            foreach (var s in slots)
                if (s.voice == null)
                {
                    slot = s;
                    break;
                }
            if (slot == null)
                return false;
            var l = v.layer;
            var clip = clipOf(l.clips[Random.Range(0, l.clips.Length)]);
            if (clip == null || clip.length <= 0.1f)
                return false;

            float bearing = Random.value * Mathf.PI * 2f;
            float d = Random.Range(l.distMin, l.distMax);
            float h = Random.Range(l.heightMin, l.heightMax);
            slot.source.transform.position = new Vector3(listener.x + Mathf.Sin(bearing) * d, ground + h, listener.z + Mathf.Cos(bearing) * d);

            float length = l.playMax > 0f ? Random.Range(l.playMin, l.playMax) : clip.length;
            length = Mathf.Clamp(length, 0.5f, clip.length);
            float offset = clip.length > length + 0.5f ? Random.Range(0f, clip.length - length) : 0f;

            var src = slot.source;
            src.clip = clip;
            src.spatialBlend = l.spatial;
            src.volume = 0f;
            src.Play();
            if (offset > 0f)
                src.time = Mathf.Min(offset, clip.length - 0.1f);
            slot.voice = v;
            slot.start = now;
            slot.length = length;
            slot.fadeIn = offset > 0f ? 0.4f : 0.02f;                      // a cut from the middle fades in; a clip's start does not need to
            slot.fadeOut = Mathf.Clamp(length * 0.3f, 0.05f, 1.2f);
            slot.nextCutoff = 0f;
            foreach (var g in v.groups)
                g.last = now;
            LastCalled = l.id;
            return true;
        }
    }
}
