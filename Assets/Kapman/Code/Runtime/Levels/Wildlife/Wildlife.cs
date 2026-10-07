using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kapman.Game.Sim;
using Kapman.Nature;
using Kapman.World;
using UnityEngine;

namespace Kapman.Levels
{
    /// <summary>
    /// THE YEAR IN THE ANIMALS (Design/wildlife_and_seasons.md 4.1, first slice) - one per level. It reads the wildlife
    /// tables (Assets/Kapman/Data/Wildlife) and the chronicle's hard winters, finds the level's region (its own override,
    /// else LevelInfo's place and kind), and once a game hour, through the tick ladder so a jumped month is replayed hour
    /// by hour: works out how much of each species is here (its calendar, the year's die, the warm index, hard winters),
    /// sets every flock's presence (the swallows are simply not there in January; a summer goose flock is the resident
    /// greylags alone), tests the wildlife events and raises Fired for each. Birds stay the only visible animals
    /// (decision 15); the rest of the species are numbers here for the sound and the market.
    ///
    /// For the other systems: Present (species id to presence, for the audio pass's voices), Active (that times the
    /// hour and the weather), Context, YearTempOffset (the hard winter's cold, for LevelWeather), HudLines (two lines for
    /// the test panel), CaptureState and RestoreState (the save's "wildlife" block), SetSeed (the world's seed).
    /// In the editor it only loads its tables; ShowNow pushes the calendar into the flocks for the shots.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-50)]
    public class Wildlife : MonoBehaviour, ITickSink
    {
        /// <summary>Where the tables are read from when no TextAsset is assigned (relative to Assets).</summary>
        public const string DataFolder = "Kapman/Data/Wildlife";
        public const string ChronicleFile = "Kapman/Game/Data/chronicle_793.json";

        [Header("The tables (empty: read from Assets/Kapman/Data/Wildlife and the chronicle off the disk)")]
        public TextAsset regionsFile;
        public TextAsset speciesFile;
        public TextAsset farmYearFile;
        public TextAsset eventsFile;
        public TextAsset chronicleFile;

        [Tooltip("The level's wildlife region (regions.json), when its place says it wrongly (the Thames mouth, the Limfjord). " +
                 "Empty: by LevelInfo's latitude, longitude and kind.")]
        public string region = "";
        [Tooltip("The world's seed: the year's dice (hard winters, the mast, early springs). The game sets it from the save.")]
        public int seed = 793;

        public static Wildlife Current { get; private set; }
        /// <summary>The moment as the animals have it (the band of the day, the weather, the year state).</summary>
        public static WildlifeContext Context { get; private set; }
        /// <summary>How much of each species is here today (0..1), species in the region's range only.</summary>
        public static IReadOnlyDictionary<string, float> Present => present;
        /// <summary>Present times the hour's band and the weather: what is seen and heard now.</summary>
        public static IReadOnlyDictionary<string, float> Active => active;
        /// <summary>A wildlife event fired (the test panel, later the crew's line and the chart's calendar listen).</summary>
        public static event Action<WildlifeEvent> Fired;
        /// <summary>Two lines for the test panel: the region and today's events; the flocks' presence and who is away.</summary>
        public static string HudLines { get; private set; } = "";
        /// <summary>The hard winter's cold on the level (degrees C, 0 or below), from December to February: -6 x HW.</summary>
        public static float YearTempOffset { get; private set; }

        static readonly Dictionary<string, float> present = new Dictionary<string, float>();
        static readonly Dictionary<string, float> active = new Dictionary<string, float>();

        WildlifeModel model;
        Region regionNow;
        WildlifeEventState state = new WildlifeEventState();
        readonly List<WildlifeEvent> today = new List<WildlifeEvent>();
        int todayKey = -1;
        readonly List<Flock> flocks = new List<Flock>();
        readonly Dictionary<string, float> flockPresence = new Dictionary<string, float>();
        readonly HashSet<string> warned = new HashSet<string>();
        TickLadder ladder;
        int ladderYear, ladderDay;
        double lastHours = double.NaN;
        bool changed;

        public WildlifeModel Model => model;
        public Region RegionNow => regionNow;
        /// <summary>The events fired today on this level.</summary>
        public IReadOnlyList<WildlifeEvent> Today => today;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            Load();
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Current = null;
            Fired = null;
            present.Clear();
            active.Clear();
            HudLines = "";
            YearTempOffset = 0f;
            Context = default;
        }

        // ------------------------------------------------------------------ the tables

        /// <summary>Reads the tables once; on a missing or broken table it says so and switches itself off.</summary>
        bool Load()
        {
            if (model != null)
                return true;
            try
            {
                model = WildlifeModel.FromJson(
                    Text(regionsFile, DataFolder + "/" + RegionTable.FileName),
                    Text(speciesFile, DataFolder + "/" + SpeciesTable.FileName),
                    Text(farmYearFile, DataFolder + "/" + FarmYear.FileName),
                    Text(eventsFile, DataFolder + "/" + WildlifeEvents.FileName),
                    Text(chronicleFile, ChronicleFile), seed);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Kapman wildlife: the tables could not be read, so the flocks keep no calendar: " + e.Message, this);
                enabled = false;
                return false;
            }
        }

        static string Text(TextAsset asset, string underAssets)
        {
            if (asset != null)
                return asset.text;
            string path = Path.Combine(Application.dataPath, underAssets);
            if (!File.Exists(path))
                throw new FileNotFoundException("no " + underAssets + " (assign it on Wildlife, or keep it under Assets)", path);
            return File.ReadAllText(path);
        }

        /// <summary>A new world seed (a new or a loaded game): the year's dice are thrown again and the calendar re-read.</summary>
        public void SetSeed(int worldSeed)
        {
            seed = worldSeed;
            if (model == null)
                return;
            model.Reseed(worldSeed);
            if (Application.isPlaying && LevelClock.Current != null && ResolveRegion())
            {
                Refresh();
                ApplyToFlocks();
            }
        }

        /// <summary>The events' cool-downs as JSON, for the save's "wildlife" block.</summary>
        public string CaptureState() => state.ToJson();

        /// <summary>Puts back the cool-downs from a save (an empty text: none yet).</summary>
        public void RestoreState(string json) => state = WildlifeEventState.FromJson(json);

        bool ResolveRegion()
        {
            if (regionNow != null)
                return true;
            if (model == null)
                return false;
            var info = LevelInfo.Current;
            if (info == null && string.IsNullOrEmpty(region))
            {
                Warn("noinfo", "Kapman wildlife: no LevelInfo and no region set, so the level has no wildlife region.");
                return false;
            }
            try
            {
                regionNow = model.Regions.Resolve(region, info != null ? info.latitude : 0.0, info != null ? info.longitude : 0.0,
                                                  info != null && info.kind == LevelKind.Ocean ? "sea" : "land");
            }
            catch (ArgumentException e)
            {
                Warn("region", "Kapman wildlife: " + e.Message);
                return false;
            }
            return regionNow != null;
        }

        void Warn(string key, string text)
        {
            if (warned.Add(key))
                Debug.LogWarning(text, this);
        }

        // ------------------------------------------------------------------ the hours

        void Update()
        {
            if (!Application.isPlaying || model == null)
                return;
            var clock = LevelClock.Current;
            if (clock == null || !ResolveRegion())
                return;
            double now = clock.GameHours;
            if (ladder == null || ladderYear != clock.year || ladderDay != clock.startDay || double.IsNaN(lastHours))
            {
                // the first look, or the clock was set to another start: no replay, the calendar as it stands
                ladder = new TickLadder(clock.year, clock.startDay);
                ladderYear = clock.year;
                ladderDay = clock.startDay;
                lastHours = now;
                Refresh();
                ApplyToFlocks();
                return;
            }
            if (now == lastHours)
                return;
            changed = false;
            ladder.Advance(lastHours, now, false, this);
            lastHours = now;
            if (changed)
            {
                Refresh();
                ApplyToFlocks();
            }
        }

        void ITickSink.Minute(long minute)
        {
            // (the animals keep the hour: nothing changes by the minute)
        }

        void ITickSink.Hour(long hour)
        {
            var date = Calendar.At(ladderYear, ladderDay, hour);
            var x = HourContext(date.year, date.dayOfYear, (int)Math.Round(date.hour) % 24);
            if (x.DayKey != todayKey)
            {
                todayKey = x.DayKey;
                today.Clear();
            }
            foreach (var e in model.Events.Due(model, regionNow, x, state))
            {
                today.Add(e);
                Fired?.Invoke(e);
            }
            changed = true;
        }

        void ITickSink.Day(int day) => changed = true;

        void ITickSink.Year(int year) => changed = true;

        void ITickSink.Jumped(double fromHours, double toHours) => changed = true;

        void ITickSink.Reversed(double fromHours, double toHours)
        {
            // nothing fires going back; the calendar is simply read again where the clock now stands
            changed = true;
        }

        /// <summary>The context of an hour the ladder raises: the Sun's band by the approximate Sun at the hour's start (the
        /// same for every machine and every frame rate), the weather and the Moon as they are now, the year state.</summary>
        WildlifeContext HourContext(int year, int day, int hour)
        {
            var info = LevelInfo.Current;
            double lat = info != null ? info.latitude : 52.0;
            double alt = Presence.ApproxSunAltitude(lat, day, hour);
            var x = Weathered(new WildlifeContext
            {
                year = year,
                day = day,
                hour = hour,
                band = Presence.Band((float)alt, hour < 12),
                night = Presence.NightFrom(alt),
            });
            return x;
        }

        /// <summary>Fills in the weather, the water, the Moon and the year state.</summary>
        WildlifeContext Weathered(WildlifeContext x)
        {
            var w = LevelWeather.Current;
            var info = LevelInfo.Current;
            if (w != null)
            {
                x.wind = w.Wind;
                x.rain = w.Rain;
                x.fogDensity = w.FogDensity;
                x.frost = w.Frost;
                x.storm = w.Storm;
                x.flood = w.flood;
            }
            x.tide = info != null ? info.WaterLevel - info.baseWaterLevel : 0.0;
            x.moonLit = Sky.Known && Sky.Moon.altitude > 0.0 ? Sky.MoonLit : 0.0;
            x.hardWinter = model.Years.HardWinter(x.year, regionNow);
            x.mast = model.Years.Mast(x.year);
            return x;
        }

        // ------------------------------------------------------------------ the calendar into the flocks

        /// <summary>
        /// Reads the calendar where the clock stands now: the context (the live Sun's band), every species' presence and
        /// activity in the region, the hard winter's cold, and the test panel's lines.
        /// </summary>
        public void Refresh()
        {
            var clock = LevelClock.Current;
            if (model == null || clock == null || regionNow == null)
                return;
            var date = clock.Date;
            var x = Weathered(new WildlifeContext
            {
                year = date.year,
                day = date.dayOfYear,
                hour = (int)clock.HourOfDay,
                band = Flock.Band,
                night = LevelClock.Night,
            });
            Context = x;
            present.Clear();
            active.Clear();
            foreach (var s in model.Species.species)
            {
                if (s.Range(regionNow.id) <= 0.0)
                    continue;
                present[s.id] = (float)model.Presence(s, regionNow, x.day, x.year);
                active[s.id] = (float)model.Active(s, regionNow, x);
            }
            // the winter's cold: December belongs to the winter that ends in the next year
            double hw = x.day >= 335 ? model.Years.HardWinter(x.year + 1, regionNow) : x.day <= 60 ? x.hardWinter : 0.0;
            YearTempOffset = (float)(-6.0 * hw);
            HudLines = Hud(x);
        }

        /// <summary>Sets every flock of this level's scene to its species' presence (hiding birds, or the whole flock).</summary>
        public void ApplyToFlocks()
        {
            if (model == null || regionNow == null)
                return;
            if (flocks.Count == 0)
                foreach (var f in FindObjectsByType<Flock>(FindObjectsInactive.Include))
                    if (f.gameObject.scene == gameObject.scene)
                        flocks.Add(f);
            var x = Context;
            flockPresence.Clear();
            foreach (var f in flocks)
            {
                if (f == null)
                    continue;
                string id = f.Species;
                if (!flockPresence.TryGetValue(id, out float p))
                {
                    try
                    {
                        p = (float)model.FlockPresence(id, regionNow, x.day, x.year);
                    }
                    catch (ArgumentException e)
                    {
                        Warn("flock " + id, "Kapman wildlife: the flock " + f.name + " keeps its birds: " + e.Message);
                        continue;
                    }
                    flockPresence[id] = p;
                }
                f.SetPresence(p);
            }
            HudLines = Hud(x);
        }

        /// <summary>For the shots (and the editor): finds the region and pushes the calendar where the clock stands into the
        /// flocks now, without firing anything. False when there is no clock, region or table.</summary>
        public bool ShowNow()
        {
            if (!Load() || LevelClock.Current == null || !ResolveRegion())
                return false;
            flocks.Clear();
            Flock.ForgetBand();
            Refresh();
            ApplyToFlocks();
            return true;
        }

        /// <summary>The flocks' presence by species, as last applied.</summary>
        public IReadOnlyDictionary<string, float> FlockPresence => flockPresence;

        string Hud(WildlifeContext x)
        {
            var sb = new StringBuilder();
            sb.Append("Wildlife: ").Append(regionNow.id).Append(" (").Append(regionNow.name).Append(")  ·  ")
              .Append(x.band.ToString().ToLowerInvariant());
            if (x.hardWinter > 0.0)
                sb.Append("  ·  hard winter ").Append(x.hardWinter.ToString("0.00"));
            sb.Append("  ·  today: ");
            if (today.Count == 0)
                sb.Append("nothing yet");
            else
                for (int i = 0; i < today.Count; i++)
                    sb.Append(i > 0 ? ", " : "").Append(today[i].Id);
            sb.Append('\n');

            // the flocks, most present first (five), then the birds of the region that are away
            var ids = new List<string>(flockPresence.Keys);
            ids.Sort((a, b) => flockPresence[b].CompareTo(flockPresence[a]));
            for (int i = 0; i < ids.Count && i < 5; i++)
                sb.Append(i > 0 ? "  ·  " : "").Append(ids[i]).Append(' ').Append(flockPresence[ids[i]].ToString("0.00"));
            var away = new List<string>();
            foreach (var s in model.Species.species)
                if (s.IsBird && present.TryGetValue(s.id, out float p) && p < Flock.AwayBelow)
                    away.Add(s.id);
            if (away.Count > 0)
                sb.Append(ids.Count > 0 ? "  ·  " : "").Append("away: ").Append(string.Join(", ", away));
            return sb.ToString();
        }
    }
}
