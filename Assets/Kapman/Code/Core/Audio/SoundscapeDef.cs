using System;
using System.Collections.Generic;
using Kapman.Game;
using UnityEngine;

namespace Kapman.Audio
{
    /// <summary>
    /// One condition of a layer: a ramp [a, b] (0 at a, 1 at b; a above b falls instead), or a window [[a, b], [c, d]]
    /// (rising from a to b, full to c, gone by d). A window whose start comes after its end ([[300, 330], [40, 60]])
    /// wraps: it runs over the year's end (or midnight). A ramp that starts beyond the value's range gives a floor:
    /// "night": [-1, 1] is 0.5 by day and 1 at night.
    /// </summary>
    public sealed class SoundCondition
    {
        public string key;
        public float a, b, c, d;
        public bool window;

        /// <summary>True for a window that runs over the end of its range.</summary>
        public bool Wraps => window && a > d;

        public float Evaluate(SoundContext ctx)
        {
            float x = ctx.Get(key);
            if (!window)
                return Ramp(a, b, x);
            if (a > d)
                return Mathf.Max(Ramp(a, b, x), Ramp(d, c, x));
            return Mathf.Min(Ramp(a, b, x), Ramp(d, c, x));
        }

        static float Ramp(float from, float to, float x) => from == to ? (x >= to ? 1f : 0f) : Mathf.InverseLerp(from, to, x);
    }

    /// <summary>
    /// A layer from soundscape.json: a crossfade (clips set at points along its driver, or along "fade_by" when the level
    /// follows another value), a bed (one clip), an event (clips fired by something happening, such as the lightning),
    /// a voice (a pool of one-shots fired at random intervals from a random place round the listener: the wildlife), or a
    /// group (voices of which one at least calls every min_interval_s while the group is on). "mask": "wildlife" silences
    /// a layer in a near gale and thins it in rain; "at": "ship" puts its sources on the player's ship.
    /// </summary>
    public sealed class SoundLayerDef
    {
        public string id, kind, driver, on, fadeBy, mask, at;
        public string[] clips = new string[0];
        public float[] points = new float[0];
        public string[] nearClips = new string[0], farClips = new string[0];
        public float baseDb = -30f, dbPerUnit, capDb = 0f, smoothDbPerS = 6f, spatial;
        public float duckDb, duckHoldS, duckReleaseS, duckWithinM;
        public readonly List<SoundCondition> when = new List<SoundCondition>();

        /// <summary>A voice: its source level (dB SPL at 1 m) and trim (dB); the wait between its tries (s), the distance
        /// round the listener (m, level) and the height above the water (m) it calls from, and how long a stretch of its
        /// recording it plays (s; 0 for the whole clip).</summary>
        public float splDb = 80f, trimDb, intervalMin = 30f, intervalMax = 90f, distMin = 20f, distMax = 100f,
            heightMin = 1f, heightMax = 10f, playMin, playMax;
        /// <summary>A group: its voices, and the longest it may go without one of them calling (s).</summary>
        public string[] members = new string[0];
        public float minIntervalS = 20f;

        bool maskWildlife;

        /// <summary>The value the crossfade's clips are set along: "fade_by", else the driver.</summary>
        public string FadeKey => string.IsNullOrEmpty(fadeBy) ? driver : fadeBy;
        public bool OnShip => at == "ship";

        /// <summary>The product of the conditions and the mask (1 with neither). For a voice it is the chance that it calls
        /// when its wait is up; for a group, whether the group is on.</summary>
        public float When(SoundContext c)
        {
            float g = 1f;
            foreach (var w in when)
                g *= w.Evaluate(c);
            if (maskWildlife && g > 0f)
                g *= SoundscapeModel.WildlifeOpen(c);
            return g;
        }

        internal void SetMask(string m)
        {
            mask = m;
            maskWildlife = m == "wildlife";
        }
    }

    /// <summary>The soundscape's data (Assets/Kapman/Audio/soundscape.json): its layers, checked as they load.</summary>
    public sealed class SoundscapeDef
    {
        public readonly List<SoundLayerDef> layers = new List<SoundLayerDef>();

        /// <summary>The offset from a source level in dB SPL to the game's level on the clip (the design's K, 2.2): the
        /// one knob that moves every voice together.</summary>
        public float splToDb = -76f;

        public SoundLayerDef Layer(string id) => layers.Find(l => l.id == id);

        /// <summary>The layers of a kind ("bed", "crossfade", "event", "voice", "group").</summary>
        public IEnumerable<SoundLayerDef> OfKind(string kind)
        {
            foreach (var l in layers)
                if (l.kind == kind)
                    yield return l;
        }

        /// <summary>Every clip the layers name (to check against the library and to load).</summary>
        public IEnumerable<string> ClipIds()
        {
            foreach (var l in layers)
            {
                foreach (var s in l.clips) yield return s;
                foreach (var s in l.nearClips) yield return s;
                foreach (var s in l.farClips) yield return s;
            }
        }

        public static SoundscapeDef Parse(string json)
        {
            var root = Json.Parse(json);
            var def = new SoundscapeDef { splToDb = (float)Json.Num(root, "spl_to_db", -76.0) };
            var all = Json.Arr(root, "layers") ?? throw new FormatException("soundscape.json: no \"layers\"");
            foreach (var o in all)
            {
                string kind = Json.Str(o, "kind");
                var l = new SoundLayerDef
                {
                    id = Json.Str(o, "id"),
                    kind = kind,
                    driver = Json.Str(o, "driver"),
                    fadeBy = Json.Str(o, "fade_by"),
                    on = Json.Str(o, "on"),
                    at = Json.Str(o, "at"),
                    baseDb = (float)Json.Num(o, "base_db", -30.0),
                    dbPerUnit = (float)Json.Num(o, "db_per_unit", 0.0),
                    capDb = (float)Json.Num(o, "cap_db", 0.0),
                    smoothDbPerS = (float)Json.Num(o, "smooth_db_per_s", 6.0),
                    spatial = (float)Json.Num(o, "spatial", kind == "voice" ? 0.8 : 0.0),
                };
                if (string.IsNullOrEmpty(l.id))
                    throw new FormatException("soundscape.json: a layer has no id");
                if (def.Layer(l.id) != null)
                    throw new FormatException($"soundscape.json: two layers are called '{l.id}'");
                string mask = Json.Str(o, "mask");
                if (mask != "" && mask != "wildlife")
                    throw new FormatException($"soundscape.json: '{l.id}' has an unknown mask \"{mask}\" (only \"wildlife\")");
                l.SetMask(mask);
                if (l.at != "" && l.at != "ship")
                    throw new FormatException($"soundscape.json: '{l.id}' is at an unknown place \"{l.at}\" (only \"ship\")");
                switch (l.kind)
                {
                    case "crossfade":
                        var pairs = Json.Arr(o, "clips") ?? new List<object>();
                        l.clips = new string[pairs.Count];
                        l.points = new float[pairs.Count];
                        for (int i = 0; i < pairs.Count; i++)
                        {
                            var p = (List<object>)pairs[i];
                            l.clips[i] = (string)p[0];
                            l.points[i] = Convert.ToSingle(p[1]);
                            if (i > 0 && l.points[i] <= l.points[i - 1])
                                throw new FormatException($"soundscape.json: '{l.id}' crossfade points must rise");
                        }
                        if (l.clips.Length == 0)
                            throw new FormatException($"soundscape.json: '{l.id}' has no clips");
                        if (string.IsNullOrEmpty(l.FadeKey))
                            throw new FormatException($"soundscape.json: '{l.id}' is a crossfade with nothing to fade by (driver or fade_by)");
                        break;
                    case "bed":
                        l.clips = new[] { Json.Str(o, "clip") };
                        if (string.IsNullOrEmpty(l.clips[0]))
                            throw new FormatException($"soundscape.json: '{l.id}' has no clip");
                        break;
                    case "event":
                        l.nearClips = Strings(Json.Arr(o, "near_clips"));
                        l.farClips = Strings(Json.Arr(o, "far_clips"));
                        var duck = Json.Obj(o, "duck");
                        if (duck != null)
                        {
                            l.duckDb = (float)Json.Num(duck, "db");
                            l.duckHoldS = (float)Json.Num(duck, "hold_s");
                            l.duckReleaseS = (float)Json.Num(duck, "release_s");
                            l.duckWithinM = (float)Json.Num(duck, "within_m");
                        }
                        break;
                    case "voice":
                        l.clips = Strings(Json.Arr(o, "clips"));
                        if (l.clips.Length == 0)
                            throw new FormatException($"soundscape.json: '{l.id}' has no clips");
                        l.splDb = (float)Json.Num(o, "spl_1m", 80.0);
                        l.trimDb = (float)Json.Num(o, "trim_db", 0.0);
                        Range(o, l.id, "interval_s", ref l.intervalMin, ref l.intervalMax);
                        Range(o, l.id, "distance_m", ref l.distMin, ref l.distMax);
                        Range(o, l.id, "height_m", ref l.heightMin, ref l.heightMax);
                        Range(o, l.id, "play_s", ref l.playMin, ref l.playMax);
                        if (l.intervalMin <= 0f)
                            throw new FormatException($"soundscape.json: '{l.id}' interval_s must be above 0");
                        if (l.distMin < 1f)
                            throw new FormatException($"soundscape.json: '{l.id}' distance_m must be at least 1 m");
                        if (l.playMin < 0f)
                            throw new FormatException($"soundscape.json: '{l.id}' play_s must not be below 0");
                        break;
                    case "group":
                        l.members = Strings(Json.Arr(o, "members"));
                        l.minIntervalS = (float)Json.Num(o, "min_interval_s", 20.0);
                        if (l.members.Length == 0)
                            throw new FormatException($"soundscape.json: '{l.id}' is a group with no members");
                        if (l.minIntervalS <= 0f)
                            throw new FormatException($"soundscape.json: '{l.id}' min_interval_s must be above 0");
                        break;
                    default:
                        throw new FormatException($"soundscape.json: '{l.id}' has an unknown kind \"{l.kind}\"");
                }
                var when = Json.Obj(o, "when");
                if (when != null)
                    foreach (var kv in when)
                    {
                        var range = kv.Value as List<object>;
                        var c = new SoundCondition { key = kv.Key };
                        if (range != null && range.Count == 2 && range[0] is List<object> r0 && range[1] is List<object> r1
                            && r0.Count == 2 && r1.Count == 2)
                        {
                            c.window = true;
                            c.a = Convert.ToSingle(r0[0]); c.b = Convert.ToSingle(r0[1]);
                            c.c = Convert.ToSingle(r1[0]); c.d = Convert.ToSingle(r1[1]);
                            if (c.a > c.b || c.c > c.d || (c.b > c.c && c.d >= c.a))
                                throw new FormatException($"soundscape.json: '{l.id}' window \"{kv.Key}\" must be [[rise from, full at], [full to, gone by]]");
                        }
                        else if (range != null && range.Count == 2)
                        {
                            c.a = Convert.ToSingle(range[0]);
                            c.b = Convert.ToSingle(range[1]);
                        }
                        else
                            throw new FormatException($"soundscape.json: '{l.id}' condition \"{kv.Key}\" is not [a, b] or [[a, b], [c, d]]");
                        c.Evaluate(default);                                      // an unknown key throws here, at load
                        l.when.Add(c);
                    }
                if (!string.IsNullOrEmpty(l.driver))
                    default(SoundContext).Get(l.driver);
                if (!string.IsNullOrEmpty(l.fadeBy))
                    default(SoundContext).Get(l.fadeBy);
                def.layers.Add(l);
            }
            foreach (var g in def.OfKind("group"))
                foreach (var m in g.members)
                {
                    var v = def.Layer(m);
                    if (v == null || v.kind != "voice")
                        throw new FormatException($"soundscape.json: group '{g.id}' names '{m}', which is not a voice");
                }
            return def;
        }

        /// <summary>Reads a [min, max] pair into the two values (kept as they are when the field is missing).</summary>
        static void Range(object o, string id, string key, ref float min, ref float max)
        {
            var r = Json.Arr(o, key);
            if (r == null)
                return;
            if (r.Count != 2)
                throw new FormatException($"soundscape.json: '{id}' {key} is not [min, max]");
            min = Convert.ToSingle(r[0]);
            max = Convert.ToSingle(r[1]);
            if (max < min)
                throw new FormatException($"soundscape.json: '{id}' {key} has its max below its min");
        }

        static string[] Strings(List<object> a)
        {
            if (a == null)
                return new string[0];
            var s = new string[a.Count];
            for (int i = 0; i < a.Count; i++)
                s[i] = (string)a[i];
            return s;
        }
    }
}
