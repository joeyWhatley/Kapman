using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Nature
{
    /// <summary>Multipliers by the Sun's band of the day (dawn, day, dusk, night).</summary>
    public struct HourWeights
    {
        public double dawn, day, dusk, night;

        public static readonly HourWeights One = new HourWeights { dawn = 1.0, day = 1.0, dusk = 1.0, night = 1.0 };

        public double For(HourBand band)
        {
            switch (band)
            {
                case HourBand.Dawn: return dawn;
                case HourBand.Dusk: return dusk;
                case HourBand.Night: return night;
                default: return day;
            }
        }

        public static HourWeights Read(object o, HourWeights fallback)
        {
            if (!(o is Dictionary<string, object>))
                return fallback;
            return new HourWeights
            {
                dawn = Json.Num(o, "dawn", fallback.dawn),
                day = Json.Num(o, "day", fallback.day),
                dusk = Json.Num(o, "dusk", fallback.dusk),
                night = Json.Num(o, "night", fallback.night),
            };
        }
    }

    /// <summary>How weather changes what is seen of a species: the wind it stops flying in (m/s), and the multiplier at
    /// full rain, fog, frost and storm.</summary>
    public struct WeatherWeights
    {
        public double windMax, rain, fog, frost, storm;

        public static readonly WeatherWeights None = new WeatherWeights { windMax = 1000.0, rain = 1.0, fog = 1.0, frost = 1.0, storm = 1.0 };
    }

    /// <summary>A flock's sizes and how it flies.</summary>
    public struct FlockSize
    {
        public int min, max, typical;
        /// <summary>skein, cloud, line, pair, loose, stand, raft.</summary>
        public string formation;
    }

    /// <summary>A sign this species gives ahead of the weather (2.11), right this share of the time.</summary>
    public sealed class Omen
    {
        public string sign = "", predicts = "";
        public double reliability;
    }

    /// <summary>A passage window for skeins over at height: day-of-year from and to, and the bearing.</summary>
    public struct Passage
    {
        public int from, to;
        public string bearing;
    }

    /// <summary>Where and when farm stock is heard (stock records only): a window on the Julian farm calendar, the
    /// builder's anchor, and the hours.</summary>
    public sealed class StockPlace
    {
        public string id = "", anchor = "";
        public int from, to;
        public HourWeights hours = HourWeights.One;
    }

    /// <summary>One species, or a species group the game treats as one (gulls, crows and rooks), as species.json has it
    /// (Design/wildlife_and_seasons.md 2.1).</summary>
    public sealed class Species
    {
        public string id = "", name = "", group = "", kind = "bird", mesh;
        public double wingspan;
        public double[] colour;
        /// <summary>The weight by region id; "*" stands for every region not named.</summary>
        public Dictionary<string, double> range = new Dictionary<string, double>();
        public double perKm2;
        public bool hasPerKm2;
        public Dictionary<string, double> density = new Dictionary<string, double>();
        /// <summary>The presence curve, (Gregorian day of the year, 0..1), sorted; the two ends join.</summary>
        public List<(double x, double y)> presence;
        public double jitterDays, warmShiftDays;
        public HourWeights hours = HourWeights.One;
        public WeatherWeights weather = WeatherWeights.None;
        public FlockSize flock;
        public bool hasFlock;
        public List<string> behaviours = new List<string>();
        public List<string> voice = new List<string>();
        public double voiceRate;
        /// <summary>The words a sound clip's credit line must carry for the clip to be this species' voice.</summary>
        public List<string> creditNames = new List<string>();
        public Omen omen;
        public Passage autumn, spring;
        public double skeinsPerDay;
        /// <summary>The long-term multiplier by year, for every region ("*") and per region where it differs.</summary>
        public Dictionary<string, List<(double x, double y)>> trend = new Dictionary<string, List<(double x, double y)>>();
        public double crash;
        public double recoverYears = 1.0;
        public Dictionary<string, double> goods = new Dictionary<string, double>();
        public Dictionary<string, double> supply = new Dictionary<string, double>();
        public Dictionary<string, double> failSupply = new Dictionary<string, double>();
        /// <summary>The year-state die this species' supply follows ("downs" for the Downs herring), or empty.</summary>
        public string stock = "";
        public readonly List<StockPlace> places = new List<StockPlace>();

        public bool IsStock => kind == "stock";
        public bool IsBird => kind == "bird";

        /// <summary>The weight in a region: its own entry, else "*", else 0.</summary>
        public double Range(string regionId)
        {
            if (regionId != null && range.TryGetValue(regionId, out var v))
                return v;
            return range.TryGetValue("*", out var all) ? all : 0.0;
        }

        /// <summary>The voice clips' file names without any #cut.</summary>
        public IEnumerable<string> VoiceFiles()
        {
            foreach (var v in voice)
            {
                int hash = v.IndexOf('#');
                yield return hash >= 0 ? v.Substring(0, hash) : v;
            }
        }

        /// <summary>The trend multiplier in a year and region (piecewise linear, held flat beyond its ends).</summary>
        public double Trend(int year, string regionId)
        {
            if (!(regionId != null && trend.TryGetValue(regionId, out var curve)) && !trend.TryGetValue("*", out curve))
                return 1.0;
            return Presence.Linear(curve, year);
        }

        public override string ToString() => id;
    }

    /// <summary>THE SPECIES TABLE (species.json): every species by id and by group.</summary>
    public sealed class SpeciesTable
    {
        public const string FileName = "species.json";

        public readonly List<Species> species = new List<Species>();

        public static SpeciesTable Load(string path) => Parse(File.ReadAllText(path));

        public static SpeciesTable Parse(string json)
        {
            var root = WildlifeJson.Root(json, FileName);
            var list = Json.Arr(root, "species") ?? throw new FormatException(FileName + ": 'species' is missing");
            var t = new SpeciesTable();
            foreach (var o in list)
            {
                var s = Read(o);
                if (t.Get(s.id) != null)
                    throw new FormatException(FileName + ": the species id " + s.id + " is used twice");
                t.species.Add(s);
            }
            return t;
        }

        static Species Read(object o)
        {
            string id = WildlifeJson.Need(o, "id", FileName + " species");
            string what = FileName + " species " + id;
            var s = new Species
            {
                id = id,
                name = Json.Str(o, "name", id),
                group = Json.Str(o, "group", id),
                kind = Json.Str(o, "kind", "bird"),
                mesh = Json.Str(o, "mesh", null),
                wingspan = Json.Num(o, "wingspan"),
                range = WildlifeJson.Numbers(o, "range"),
                hasPerKm2 = WildlifeJson.Has(o, "perKm2"),
                perKm2 = Json.Num(o, "perKm2"),
                density = WildlifeJson.Numbers(o, "density"),
                presence = WildlifeJson.Curve(Json.Arr(o, "presence"), what + " presence"),
                jitterDays = Json.Num(o, "jitterDays"),
                warmShiftDays = Json.Num(o, "warmShiftDays"),
                hours = HourWeights.Read(Json.Obj(o, "hours"), HourWeights.One),
                behaviours = WildlifeJson.Strings(o, "behaviours"),
                voice = WildlifeJson.Strings(o, "voice"),
                voiceRate = Json.Num(o, "voiceRate"),
                creditNames = WildlifeJson.Strings(o, "creditNames"),
                goods = WildlifeJson.Numbers(o, "goods"),
                supply = WildlifeJson.Numbers(o, "supply"),
                failSupply = WildlifeJson.Numbers(o, "failSupply"),
                stock = Json.Str(o, "stock"),
            };
            var colour = Json.Arr(o, "colour");
            if (colour != null && colour.Count >= 3)
                s.colour = new[] { (double)colour[0], (double)colour[1], (double)colour[2] };
            var w = Json.Obj(o, "weather");
            if (w != null)
                s.weather = new WeatherWeights
                {
                    windMax = Json.Num(w, "windMax", 1000.0),
                    rain = Json.Num(w, "rain", 1.0),
                    fog = Json.Num(w, "fog", 1.0),
                    frost = Json.Num(w, "frost", 1.0),
                    storm = Json.Num(w, "storm", 1.0),
                };
            var f = Json.Obj(o, "flock");
            if (f != null)
            {
                s.hasFlock = true;
                s.flock = new FlockSize
                {
                    min = (int)Json.Num(f, "min"),
                    max = (int)Json.Num(f, "max"),
                    typical = (int)Json.Num(f, "typical", 1),
                    formation = Json.Str(f, "formation", "loose"),
                };
            }
            var omen = Json.Obj(o, "omen");
            if (omen != null)
                s.omen = new Omen { sign = Json.Str(omen, "sign"), predicts = Json.Str(omen, "predicts"), reliability = Json.Num(omen, "reliability") };
            var m = Json.Obj(o, "migration");
            if (m != null)
            {
                s.autumn = ReadPassage(Json.Arr(m, "autumn"), what + " migration autumn");
                s.spring = ReadPassage(Json.Arr(m, "spring"), what + " migration spring");
                s.skeinsPerDay = Json.Num(m, "skeinsPerDay");
            }
            // the trend: one curve for every region, or an object of curves by region ("*" for the rest)
            if (o is Dictionary<string, object> d && d.TryGetValue("trend", out var tr) && tr != null)
            {
                if (tr is List<object> one)
                    s.trend["*"] = WildlifeJson.Curve(one, what + " trend");
                else if (tr is Dictionary<string, object> byRegion)
                    foreach (var kv in byRegion)
                        s.trend[kv.Key] = WildlifeJson.Curve(kv.Value as List<object>, what + " trend " + kv.Key)
                                          ?? throw new FormatException(what + ": trend " + kv.Key + " is not a curve");
            }
            var hw = Json.Obj(o, "hardWinter");
            if (hw != null)
            {
                s.crash = Json.Num(hw, "crash");
                s.recoverYears = Math.Max(1.0, Json.Num(hw, "recoverYears", 1.0));
            }
            var places = Json.Obj(o, "places");
            if (places != null)
                foreach (var kv in places)
                {
                    var (from, to) = WildlifeJson.Window(kv.Value, "window", what + " place " + kv.Key);
                    s.places.Add(new StockPlace
                    {
                        id = kv.Key,
                        anchor = Json.Str(kv.Value, "anchor"),
                        from = from,
                        to = to,
                        hours = HourWeights.Read(Json.Obj(kv.Value, "hours"), HourWeights.One),
                    });
                }
            return s;
        }

        static Passage ReadPassage(List<object> arr, string what)
        {
            if (arr == null)
                return default;
            if (arr.Count < 3 || !(arr[0] is double a) || !(arr[1] is double b) || !(arr[2] is string bearing))
                throw new FormatException(what + ": [from, to, \"bearing\"] expected");
            return new Passage { from = (int)a, to = (int)b, bearing = bearing };
        }

        /// <summary>A species by id, or null.</summary>
        public Species Get(string id)
        {
            foreach (var s in species)
                if (s.id == id)
                    return s;
            return null;
        }

        /// <summary>The species of a group (empty when there is no such group).</summary>
        public List<Species> Group(string group)
        {
            var list = new List<Species>();
            foreach (var s in species)
                if (s.group == group)
                    list.Add(s);
            return list;
        }

        /// <summary>A flock's species: the species of that id, else the members of the group of that name.</summary>
        public List<Species> Resolve(string speciesOrGroup)
        {
            var one = Get(speciesOrGroup);
            return one != null ? new List<Species> { one } : Group(speciesOrGroup);
        }
    }
}
