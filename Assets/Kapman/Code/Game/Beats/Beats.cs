using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>A landmark the crew steers by (landmarks_&lt;level&gt;.json, written by the level's builder).</summary>
    public sealed class Landmark
    {
        public string id, name, kind;
        public float x, z, height, seenM;
        public int from, to;

        public bool StandsIn(int year) => (from == 0 || from <= year) && (to == 0 || to >= year);
    }

    public static class Landmarks
    {
        public static List<Landmark> Parse(string json)
        {
            var list = new List<Landmark>();
            if (string.IsNullOrEmpty(json))
                return list;
            foreach (var o in Json.Arr(Json.Parse(json), "landmarks") ?? new List<object>())
                list.Add(new Landmark
                {
                    id = Json.Str(o, "id"), name = Json.Str(o, "name"), kind = Json.Str(o, "kind"),
                    x = (float)Json.Num(o, "x"), z = (float)Json.Num(o, "z"), height = (float)Json.Num(o, "height", 1.0),
                    seenM = (float)Json.Num(o, "seen_m", 300.0), from = (int)Json.Num(o, "from"), to = (int)Json.Num(o, "to"),
                });
            return list;
        }
    }

    /// <summary>What the beats see each game minute: where she is and how she goes, the wind, the light, the way out.</summary>
    public struct BeatContext
    {
        public double minute, realSeconds;
        public int year;
        public Vector2 position;
        /// <summary>Degrees from north; metres a second over the water.</summary>
        public float heading, speed;
        /// <summary>The wind the way the air moves (m/s, x east, y north) and how far one sees (m).</summary>
        public Vector2 wind;
        public float visibility;
        /// <summary>The level's way out is in reach, with its prompt.</summary>
        public bool exitNear;
        public string exitPrompt;

        public float WindSpeed => wind.magnitude;
        /// <summary>The compass point the wind comes from (degrees).</summary>
        public float WindFrom => Mathf.Repeat(Mathf.Atan2(-wind.x, -wind.y) * Mathf.Rad2Deg, 360f);
    }

    /// <summary>One moment that asks something of the player: a question, options, what each does.</summary>
    public sealed class Beat
    {
        public string kind, id, question, lesson;
        public string[] options = new string[0], effects = new string[0];
        public bool mandatory;
        public double raisedMinute;
    }

    public interface IBeatRaiser
    {
        void Raise(BeatContext c, List<Beat> into);
    }

    /// <summary>The wording of each beat kind (beats_793.json): placeholder text the user rewrites.</summary>
    public sealed class BeatWords
    {
        public float gapRealMinutes = 4f;
        public float openRealSeconds = 45f;
        readonly Dictionary<string, (string question, string[] options, string[] effects, bool mandatory, string lesson)> kinds =
            new Dictionary<string, (string, string[], string[], bool, string)>();

        public static BeatWords Parse(string json)
        {
            var w = new BeatWords();
            if (string.IsNullOrEmpty(json))
                return w;
            var root = Json.Parse(json);
            w.gapRealMinutes = (float)Json.Num(root, "gap_real_minutes", 4.0);
            w.openRealSeconds = (float)Json.Num(root, "open_real_seconds", 45.0);
            foreach (var kv in Json.Obj(root, "kinds") ?? new Dictionary<string, object>())
            {
                var opts = Json.Arr(kv.Value, "options") ?? new List<object>();
                var o = new string[opts.Count];
                var e = new string[opts.Count];
                for (int i = 0; i < opts.Count; i++)
                {
                    o[i] = Json.Str(opts[i], "text");
                    e[i] = Json.Str(opts[i], "does", "none");
                }
                w.kinds[kv.Key] = (Json.Str(kv.Value, "question"), o, e, Json.Bool(kv.Value, "mandatory"), Json.Str(kv.Value, "lesson"));
            }
            return w;
        }

        /// <summary>A beat of this kind, its {name}, {wind} and {prompt} filled in; null when the kind has no wording.</summary>
        public Beat Make(string kind, string id, double minute, Dictionary<string, string> fill = null)
        {
            if (!kinds.TryGetValue(kind, out var k))
                return null;
            string q = k.question;
            if (fill != null)
                foreach (var f in fill)
                    q = q.Replace("{" + f.Key + "}", f.Value);
            return new Beat { kind = kind, id = id, question = q, options = k.options, effects = k.effects, mandatory = k.mandatory, lesson = k.lesson, raisedMinute = minute };
        }
    }

    /// <summary>
    /// PILOTAGE (playability 3.2): a landmark is noticed once a voyage, when it is within its seen range, the horizon
    /// (3.57 km x (sqrt of the eye's 2.5 m + sqrt of its height)) and the visibility, and lies ahead of the beam
    /// (within 110 degrees of her heading). Boats lying off the dams are one beat in 4 real minutes at most.
    /// </summary>
    public sealed class LandmarkSight : IBeatRaiser
    {
        public const float EyeHeight = 2.5f;
        readonly List<Landmark> marks;
        readonly BeatWords words;
        readonly HashSet<string> noticed = new HashSet<string>();
        readonly Dictionary<string, double> kindLast = new Dictionary<string, double>();
        static readonly Dictionary<string, double> KindCooldown = new Dictionary<string, double> { ["moored"] = 240.0 };

        public LandmarkSight(List<Landmark> marks, BeatWords words)
        {
            this.marks = marks;
            this.words = words;
        }

        public static float Horizon(float height) => 3570f * (Mathf.Sqrt(EyeHeight) + Mathf.Sqrt(Mathf.Max(0f, height)));

        public void Raise(BeatContext c, List<Beat> into)
        {
            foreach (var m in marks)
            {
                if (noticed.Contains(m.id) || !m.StandsIn(c.year))
                    continue;
                var to = new Vector2(m.x, m.z) - c.position;
                float d = to.magnitude;
                if (d > Mathf.Min(m.seenM, Mathf.Min(Horizon(m.height), c.visibility)))
                    continue;
                float bearing = Mathf.Atan2(to.x, to.y) * Mathf.Rad2Deg;
                if (d > 30f && Mathf.Abs(Mathf.DeltaAngle(c.heading, bearing)) > 110f)
                    continue;
                noticed.Add(m.id);
                if (KindCooldown.TryGetValue(m.kind, out double cool) && kindLast.TryGetValue(m.kind, out double last) && c.minute - last < cool)
                    continue;
                kindLast[m.kind] = c.minute;
                var b = words.Make(m.kind, m.id, c.minute, new Dictionary<string, string> { ["name"] = m.name })
                        ?? words.Make("landmark", m.id, c.minute, new Dictionary<string, string> { ["name"] = m.name });
                if (b != null)
                    into.Add(b);
            }
        }
    }

    /// <summary>
    /// THE WIND (playability 3.3): a beat when the wind has gone round 40 degrees or more since the crew last spoke of it
    /// (above 2 m/s), or has crossed a reefing step (12, 15, 20 m/s) up or down; two hours of game time apart at least.
    /// </summary>
    public sealed class WindBeats : IBeatRaiser
    {
        static readonly float[] Steps = { 12f, 15f, 20f };
        readonly BeatWords words;
        float lastFrom = float.NaN;
        int lastStep = -1;
        double lastMinute = -1e9;

        public WindBeats(BeatWords words) => this.words = words;

        static int Step(float u)
        {
            int s = 0;
            while (s < Steps.Length && u >= Steps[s])
                s++;
            return s;
        }

        static string Point(float from)
        {
            string[] names = { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" };
            return names[Mathf.RoundToInt(Mathf.Repeat(from, 360f) / 45f) % 8];
        }

        public void Raise(BeatContext c, List<Beat> into)
        {
            float from = c.WindFrom, u = c.WindSpeed;
            int step = Step(u);
            if (float.IsNaN(lastFrom))
            {
                lastFrom = from;
                lastStep = step;
                return;
            }
            if (c.minute - lastMinute < 120.0)
                return;
            string kind = null;
            if (step > lastStep) kind = "wind_rising";
            else if (step < lastStep) kind = "wind_easing";
            else if (u > 2f && Mathf.Abs(Mathf.DeltaAngle(lastFrom, from)) >= 40f) kind = "wind_shift";
            if (kind == null)
                return;
            lastFrom = from;
            lastStep = step;
            lastMinute = c.minute;
            var b = words.Make(kind, kind + "_" + ((int)c.minute).ToString(CultureInfo.InvariantCulture), c.minute,
                               new Dictionary<string, string> { ["wind"] = Point(from) });
            if (b != null)
                into.Add(b);
        }
    }

    /// <summary>The level's way out (the gateways' prompt), as a beat the log records: once each time it comes in reach.</summary>
    public sealed class ExitBeat : IBeatRaiser
    {
        readonly BeatWords words;
        bool near;

        public ExitBeat(BeatWords words) => this.words = words;

        public void Raise(BeatContext c, List<Beat> into)
        {
            if (c.exitNear && !near)
            {
                var b = words.Make("exit", "exit", c.minute, new Dictionary<string, string> { ["prompt"] = c.exitPrompt ?? "" });
                if (b != null)
                    into.Add(b);
            }
            near = c.exitNear;
        }
    }

    /// <summary>The voyage's first beat: casting off (Ravn's lesson, "make for there").</summary>
    public sealed class CastOff : IBeatRaiser
    {
        readonly BeatWords words;
        bool done;

        public CastOff(BeatWords words) => this.words = words;

        public void Raise(BeatContext c, List<Beat> into)
        {
            if (done)
                return;
            done = true;
            var b = words.Make("cast_off", "cast_off", c.minute);
            if (b != null)
                into.Add(b);
        }
    }

    /// <summary>
    /// THE LESSONS (playability 3.8: the crew teach, there is no tutorial): each offered once per house, whatever the
    /// voyage, saved with the game. Their words are placeholders (lessons_793.json) the user writes.
    /// </summary>
    public static class Lessons
    {
        static readonly Dictionary<string, (string teacher, string text)> all = new Dictionary<string, (string, string)>();
        static readonly HashSet<string> given = new HashSet<string>();

        public static void Load(string json)
        {
            all.Clear();
            if (string.IsNullOrEmpty(json))
                return;
            foreach (var o in Json.Arr(Json.Parse(json), "lessons") ?? new List<object>())
                all[Json.Str(o, "id")] = (Json.Str(o, "teacher"), Json.Str(o, "text"));
        }

        /// <summary>The lesson's words the first time it is asked for in this house; null after (or for an unknown id).</summary>
        public static string Offer(string id)
        {
            if (string.IsNullOrEmpty(id) || !all.TryGetValue(id, out var l) || !given.Add(id))
                return null;
            return $"{l.teacher}: {l.text}";
        }

        public static bool Given(string id) => given.Contains(id);
        public static void Forget() => given.Clear();
        public static List<object> Capture() => new List<object>(given);

        public static void Restore(List<object> data)
        {
            given.Clear();
            foreach (var x in data ?? new List<object>())
                if (x is string s)
                    given.Add(s);
        }
    }

    /// <summary>
    /// THE VOYAGE LOG (playability 4.2): every beat offered and what was chosen, with the game minute and the real
    /// second; CSV for the playtest; its report finds the gaps longer than the rule (4 real minutes) between beats,
    /// from the voyage's start to its end.
    /// </summary>
    public sealed class VoyageLog
    {
        public sealed class Entry
        {
            public double minute, realSeconds;
            public string what, kind, id, text;
        }

        public readonly List<Entry> entries = new List<Entry>();

        public void Note(BeatContext c, string what, Beat b, string text) =>
            entries.Add(new Entry { minute = c.minute, realSeconds = c.realSeconds, what = what, kind = b?.kind ?? "", id = b?.id ?? "", text = text ?? "" });

        public int Offered => entries.FindAll(e => e.what == "offered").Count;

        /// <summary>The stretches without a beat offered longer than this many real seconds, from start to end.</summary>
        public List<(double from, double to)> Gaps(double overSeconds, double startReal, double endReal)
        {
            var gaps = new List<(double, double)>();
            double last = startReal;
            foreach (var e in entries)
                if (e.what == "offered")
                {
                    if (e.realSeconds - last > overSeconds)
                        gaps.Add((last, e.realSeconds));
                    last = e.realSeconds;
                }
            if (endReal - last > overSeconds)
                gaps.Add((last, endReal));
            return gaps;
        }

        public string Csv()
        {
            var sb = new StringBuilder("game_minute,real_seconds,what,kind,id,text\n");
            foreach (var e in entries)
                sb.Append(e.minute.ToString("0.#", CultureInfo.InvariantCulture)).Append(',')
                  .Append(e.realSeconds.ToString("0.#", CultureInfo.InvariantCulture)).Append(',')
                  .Append(e.what).Append(',').Append(e.kind).Append(',').Append(e.id).Append(",\"")
                  .Append(e.text.Replace("\"", "\"\"")).Append("\"\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// THE BEAT SCHEDULER (playability 2.1): once a game minute it asks the raisers for beats, queues them, and offers one
    /// at a time; an optional beat left unanswered for its time lets pass (the log says so), a mandatory one waits. No
    /// gap filler: the gaps are the log's report, a measure of the level, not something papered over.
    /// </summary>
    public sealed class BeatScheduler
    {
        public readonly List<IBeatRaiser> raisers = new List<IBeatRaiser>();
        public readonly VoyageLog log = new VoyageLog();
        readonly BeatWords words;
        readonly Queue<Beat> waiting = new Queue<Beat>();
        readonly List<Beat> fresh = new List<Beat>();
        double openedReal;

        public Beat Open { get; private set; }
        /// <summary>The words of a lesson that came with the open beat (null when none, or given before in this house).</summary>
        public string OpenLesson { get; private set; }

        public BeatScheduler(BeatWords words) => this.words = words;

        public void Tick(BeatContext c)
        {
            fresh.Clear();
            foreach (var r in raisers)
                r.Raise(c, fresh);
            foreach (var b in fresh)
                waiting.Enqueue(b);
            if (Open != null && !Open.mandatory && c.realSeconds - openedReal > words.openRealSeconds)
            {
                log.Note(c, "let_pass", Open, "");
                Open = null;
            }
            if (Open == null && waiting.Count > 0)
            {
                Open = waiting.Dequeue();
                openedReal = c.realSeconds;
                OpenLesson = Lessons.Offer(Open.lesson);
                log.Note(c, "offered", Open, Open.question + (OpenLesson != null ? "  [" + OpenLesson + "]" : ""));
            }
        }

        /// <summary>The player picks an option (0-based): logged; the effect code is returned for the game to carry out.</summary>
        public string Choose(int option, BeatContext c)
        {
            if (Open == null || option < 0 || option >= Open.options.Length)
                return null;
            string does = option < Open.effects.Length ? Open.effects[option] : "none";
            log.Note(c, "chose", Open, Open.options[option]);
            Open = null;
            OpenLesson = null;
            return does;
        }
    }
}
