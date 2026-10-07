using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>What the events are tested against, once a game hour: the moment, the light, the weather and the year
    /// (Design/wildlife_and_seasons.md 4.1).</summary>
    public struct WildlifeContext
    {
        /// <summary>The year and the Julian day of the year, and the hour of the day (0-23) that has just begun.</summary>
        public int year, day, hour;
        public HourBand band;
        /// <summary>0 by day, 1 at full night (as LevelClock.Night).</summary>
        public double night;
        /// <summary>Wind (m/s); rain, frost, storm (0..1); fog as the weather's density; flood (0..1); the tide (m).</summary>
        public double wind, rain, fogDensity, frost, storm, flood, tide;
        /// <summary>The Moon's light: its lit share times whether it is up (0..1).</summary>
        public double moonLit;
        /// <summary>The year state: the hard-winter index and the mast.</summary>
        public double hardWinter, mast;

        /// <summary>A key for the day, unique across years.</summary>
        public int DayKey => year * 366 + day;
    }

    /// <summary>The conditions an event's hour must meet (all of the set ones; anyOf: at least one of its sets).</summary>
    public sealed class EventCondition
    {
        public List<HourBand> bands;
        public double nightMin = double.NaN, moonLitMax = double.NaN, rainMin = double.NaN, rainMax = double.NaN;
        public double windMax = double.NaN, floodMin = double.NaN, hardWinterMin = double.NaN;
        public List<string> presenceSpecies;
        public double presenceAbove;
        public List<EventCondition> anyOf;

        public static EventCondition Read(object o, string what)
        {
            var c = new EventCondition();
            if (!(o is Dictionary<string, object> d))
                return c;
            var bands = WildlifeJson.Strings(o, "bands");
            if (bands.Count > 0)
            {
                c.bands = new List<HourBand>();
                foreach (var b in bands)
                {
                    if (!Enum.TryParse(b, true, out HourBand band))
                        throw new FormatException(what + ": no band '" + b + "' (dawn, day, dusk, night)");
                    c.bands.Add(band);
                }
            }
            c.nightMin = Opt(o, "nightMin");
            c.moonLitMax = Opt(o, "moonLitMax");
            c.rainMin = Opt(o, "rainMin");
            c.rainMax = Opt(o, "rainMax");
            c.windMax = Opt(o, "windMax");
            c.floodMin = Opt(o, "floodMin");
            c.hardWinterMin = Opt(o, "hardWinterMin");
            var p = Json.Obj(o, "presence");
            if (p != null)
            {
                c.presenceSpecies = WildlifeJson.Strings(p, "species");
                if (c.presenceSpecies.Count == 0)
                    throw new FormatException(what + ": presence needs a species list");
                c.presenceAbove = Json.Num(p, "above");
            }
            if (d.TryGetValue("anyOf", out var any) && any is List<object> list)
            {
                c.anyOf = new List<EventCondition>();
                foreach (var x in list)
                    c.anyOf.Add(Read(x, what + " anyOf"));
            }
            return c;
        }

        static double Opt(object o, string key) => Json.Num(o, key, double.NaN);

        /// <summary>Do the hour's context and the model's presence meet every set condition?</summary>
        public bool Holds(WildlifeContext x, WildlifeModel model, Region region)
        {
            if (bands != null && !bands.Contains(x.band))
                return false;
            if (!double.IsNaN(nightMin) && x.night < nightMin)
                return false;
            if (!double.IsNaN(moonLitMax) && x.moonLit > moonLitMax)
                return false;
            if (!double.IsNaN(rainMin) && x.rain < rainMin)
                return false;
            if (!double.IsNaN(rainMax) && x.rain > rainMax)
                return false;
            if (!double.IsNaN(windMax) && x.wind > windMax)
                return false;
            if (!double.IsNaN(floodMin) && x.flood < floodMin)
                return false;
            if (!double.IsNaN(hardWinterMin) && x.hardWinter < hardWinterMin)
                return false;
            if (presenceSpecies != null)
            {
                bool any = false;
                foreach (var id in presenceSpecies)
                {
                    var s = model.Species.Get(id);
                    if (s != null && s.Range(region != null ? region.id : null) > 0.0 && model.Season(s, x.day, x.year) > presenceAbove)
                    {
                        any = true;
                        break;
                    }
                }
                if (!any)
                    return false;
            }
            if (anyOf != null && anyOf.Count > 0)
            {
                bool one = false;
                foreach (var c in anyOf)
                    if (c.Holds(x, model, region))
                    {
                        one = true;
                        break;
                    }
                if (!one)
                    return false;
            }
            return true;
        }
    }

    /// <summary>How often an event may fire: once a day, once in each pass of its window, once a year.</summary>
    public enum Cooldown { Day, Season, Year }

    /// <summary>One wildlife event as wildlife_events.json defines it.</summary>
    public sealed class WildlifeEventDef
    {
        public string id = "", see = "", crew = "", hook = "";
        public readonly List<string> regions = new List<string>();
        public readonly List<string> except = new List<string>();
        /// <summary>The window, Julian days of the year (from > to runs over the new year).</summary>
        public int from = 1, to = 366;
        public Cooldown cooldown = Cooldown.Day;
        /// <summary>Tested on the day's first hour only (the date-only events).</summary>
        public bool firstHourOnly;
        /// <summary>The chance per day the conditions hold (1 = sure).</summary>
        public double chance = 1.0;
        public EventCondition when = new EventCondition();

        public bool InRegion(string regionId) =>
            regionId != null && (regions.Contains("*") || regions.Contains(regionId)) && !except.Contains(regionId);

        public bool InWindow(int dayOfYear) => Wrap(dayOfYear - from) <= Wrap(to - from);

        /// <summary>The pass of the window a day belongs to: the year the window opened in (one before for the days of a
        /// window that runs over the new year, after it).</summary>
        public int Season(int year, int dayOfYear) => from > to && dayOfYear <= to ? year - 1 : year;

        static int Wrap(int d)
        {
            int w = d % 366;
            return w < 0 ? w + 366 : w;
        }

        public override string ToString() => id;
    }

    /// <summary>An event that fired: what, where and when.</summary>
    public sealed class WildlifeEvent
    {
        public WildlifeEventDef def;
        public string region = "";
        public int year, day, hour;

        public string Id => def != null ? def.id : "";
        public override string ToString() => $"{Id} in {region}, day {day} of {year}, {hour:00}:00";
    }

    /// <summary>The cool-downs: when each event last fired in each region. Small; saved with the game (the save's
    /// "wildlife" block) so a reloaded game does not see the first swallow twice.</summary>
    public sealed class WildlifeEventState
    {
        public readonly Dictionary<string, (int year, int day)> last = new Dictionary<string, (int year, int day)>();

        static string Key(string eventId, string region) => eventId + "|" + region;

        public bool TryLast(string eventId, string region, out int year, out int day)
        {
            if (last.TryGetValue(Key(eventId, region), out var v))
            {
                year = v.year;
                day = v.day;
                return true;
            }
            year = day = 0;
            return false;
        }

        public void Mark(string eventId, string region, int year, int day) => last[Key(eventId, region)] = (year, day);

        /// <summary>{"event|region": [year, day], ...}</summary>
        public string ToJson()
        {
            var d = new Dictionary<string, object>();
            var keys = new List<string>(last.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (var k in keys)
                d[k] = new List<object> { last[k].year, last[k].day };
            return Json.Write(d);
        }

        public static WildlifeEventState FromJson(string json)
        {
            var s = new WildlifeEventState();
            if (string.IsNullOrWhiteSpace(json))
                return s;
            if (!(Json.Parse(json) is Dictionary<string, object> d))
                throw new FormatException("wildlife event state: a JSON object expected");
            foreach (var kv in d)
            {
                var (y, day) = WildlifeJson.Pair(kv.Value, "wildlife event state " + kv.Key);
                s.last[kv.Key] = ((int)y, (int)day);
            }
            return s;
        }
    }

    /// <summary>
    /// THE WILDLIFE EVENTS (wildlife_events.json; Design/wildlife_and_seasons.md 2.10): the things the player witnesses,
    /// tested every game hour. Due gives the events that fire in this hour, in the table's order, and marks them in the
    /// state so their cool-downs hold.
    /// </summary>
    public sealed class WildlifeEvents
    {
        public const string FileName = "wildlife_events.json";

        public readonly List<WildlifeEventDef> events = new List<WildlifeEventDef>();

        public static WildlifeEvents Load(string path) => Parse(File.ReadAllText(path));

        public static WildlifeEvents Parse(string json)
        {
            var root = WildlifeJson.Root(json, FileName);
            var t = new WildlifeEvents();
            foreach (var o in Json.Arr(root, "events") ?? throw new FormatException(FileName + ": 'events' is missing"))
            {
                string id = WildlifeJson.Need(o, "id", FileName + " event");
                string what = FileName + " event " + id;
                var (from, to) = WildlifeJson.Window(o, "window", what);
                var e = new WildlifeEventDef
                {
                    id = id,
                    from = from,
                    to = to,
                    see = Json.Str(o, "see"),
                    crew = Json.Str(o, "crew"),
                    hook = Json.Str(o, "hook"),
                    firstHourOnly = Json.Str(o, "hours", "every") == "first",
                    chance = Json.Num(o, "chance", 1.0),
                    when = EventCondition.Read(Json.Obj(o, "when"), what),
                };
                e.regions.AddRange(WildlifeJson.Strings(o, "regions"));
                e.except.AddRange(WildlifeJson.Strings(o, "except"));
                if (e.regions.Count == 0)
                    throw new FormatException(what + ": 'regions' is missing (\"*\" for every region)");
                string cd = Json.Str(o, "cooldown", "day");
                if (!Enum.TryParse(cd, true, out e.cooldown))
                    throw new FormatException(what + ": no cooldown '" + cd + "' (day, season, year)");
                if (t.Get(id) != null)
                    throw new FormatException(what + ": the id is used twice");
                t.events.Add(e);
            }
            return t;
        }

        public WildlifeEventDef Get(string id)
        {
            foreach (var e in events)
                if (e.id == id)
                    return e;
            return null;
        }

        /// <summary>The events that fire in this hour in the region, marked in the state as fired.</summary>
        public List<WildlifeEvent> Due(WildlifeModel model, Region region, WildlifeContext x, WildlifeEventState state)
        {
            var fired = new List<WildlifeEvent>();
            if (region == null)
                return fired;
            foreach (var e in events)
            {
                if (!e.InRegion(region.id) || !e.InWindow(x.day) || (e.firstHourOnly && x.hour != 0))
                    continue;
                if (Cooling(e, region.id, x, state))
                    continue;
                if (!e.when.Holds(x, model, region))
                    continue;
                if (e.chance < 1.0 && Dice.RollDay(model.Seed, x.year, x.day, e.id) >= e.chance)
                    continue;
                state.Mark(e.id, region.id, x.year, x.day);
                fired.Add(new WildlifeEvent { def = e, region = region.id, year = x.year, day = x.day, hour = x.hour });
            }
            return fired;
        }

        /// <summary>Has the event already fired within its cool-down?</summary>
        static bool Cooling(WildlifeEventDef e, string region, WildlifeContext x, WildlifeEventState state)
        {
            if (!state.TryLast(e.id, region, out int y, out int d))
                return false;
            switch (e.cooldown)
            {
                case Cooldown.Year: return y == x.year;
                case Cooldown.Season: return e.InWindow(d) && e.Season(y, d) == e.Season(x.year, x.day);
                default: return y == x.year && d == x.day;
            }
        }
    }
}
