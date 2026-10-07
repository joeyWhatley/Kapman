using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;

namespace Kapman.Weather
{
    /// <summary>
    /// THE WEATHER ENGINE - the one sky (the game plan 2.4.1; Design/weather_and_sea_state.md 2 and 4.1). It owns a
    /// <see cref="WeatherModel"/> (the first build's synoptic model, moved here unchanged and extended in place) and the
    /// layers around it:
    ///   1. the climate: <see cref="ClimateNormals"/>, the measured month by month weather of each place class (KNMI
    ///      1991-2020) and the year's offset (the user's decision 17);
    ///   2. the day: <see cref="CirculationChain"/> draws one circulation type a day and <see cref="SynopticField"/> puts
    ///      that type's highs and lows into the model's own list, so the model's pressure, wind, cloud and fronts follow;
    ///   3. the hour: the model's wind is mapped, month by month, from its own distribution onto the measured one of the
    ///      place's class (a quantile map: the model keeps its timing, its gales come when its lows pass, but the hours of
    ///      each force come out as measured), with the measured day cycle put back and the coast's sea breeze; gusts by the
    ///      measured gust factor, more in showers and at a cold front; the temperature from the normals, the measured day
    ///      cycle (wider on clear still days, up to the design's 9 C inland), the model's warm and cold air, the day type's
    ///      air mass, the slow cold and mild spells, and the year's offset; and (night 2) the sky of LocalSky: the frontal
    ///      curves, the cloud layers with their kind, base, depth and light (CloudLight), rain, sleet and snow, the dew
    ///      point, radiation and sea fog, thunder, and the visibility as a range in metres (Visibility).
    /// The hour of the day is the clock's own (<see cref="HourOfDay"/>), not the game hours modulo 24.
    /// The player's level (LevelWeather), every AI ship (WorldSim, through the same model) and the LocalWeather shim read
    /// <see cref="Shared"/>, so they sail under one sky. Everything is a function of (seed, place, hour): no state but
    /// caches, the same answer whatever was asked before, so a save holds only the seed. Pure C#; thread-safe.
    /// Times are game hours since the start of year 0 (Julian years, as the model's and WorldSim's: see <see cref="HoursAt"/>).
    /// </summary>
    public sealed class WeatherEngine
    {
        public const int DefaultSeed = 793;
        public const string CirculationPath = "Assets/Kapman/Data/Weather/circulation.json";
        /// <summary>The classes over open water (the model samples them as sea).</summary>
        public static readonly string[] SeaClasses = { "north_sea" };
        /// <summary>Where a class's measured climate holds in full (km from its station), and where it has faded out.</summary>
        public const double ClassFullKm = 250.0, ClassFadeKm = 600.0;
        const double HoursPerYear = 365.25 * 24.0;
        /// <summary>The year the wind calibration samples (any year does: the wind's distribution does not change with it).</summary>
        const int CalibrationYear = 1000;

        // ------------------------------------------------------------------------------------------- the shared one

        static readonly object sharedGate = new object();
        static WeatherEngine shared;
        static int sharedSeed = DefaultSeed;
        static string normalsText, circulationText;

        /// <summary>
        /// The world's weather: made on first use from the world seed (<see cref="Reset"/>) and the data, which is the text
        /// given to <see cref="UseData"/> (the runtime passes its TextAssets) or else the project's files
        /// (Assets/Kapman/Data/Weather, read relative to the project folder: the editor, Play in the editor, the tests).
        /// Throws FileNotFoundException naming the file when neither is there.
        /// </summary>
        public static WeatherEngine Shared
        {
            get
            {
                lock (sharedGate)
                {
                    if (shared == null)
                        shared = new WeatherEngine(sharedSeed, ClimateNormals.Parse(normalsText ?? ReadProjectFile(ClimateNormals.ProjectPath)),
                                                   circulationText ?? ReadProjectFile(CirculationPath));
                    return shared;
                }
            }
        }

        /// <summary>Whether the shared engine has been made yet.</summary>
        public static bool HasShared
        {
            get
            {
                lock (sharedGate)
                    return shared != null;
            }
        }

        /// <summary>The seed the shared engine is (or will be) made with.</summary>
        public static int SharedSeed
        {
            get
            {
                lock (sharedGate)
                    return sharedSeed;
            }
        }

        /// <summary>A new world's weather (a new game, or a save's world seed): the next <see cref="Shared"/> is made with
        /// this seed. The same seed gives the same weather.</summary>
        public static void Reset(int seed)
        {
            lock (sharedGate)
            {
                if (shared != null && shared.Seed == seed)
                    return;
                sharedSeed = seed;
                shared = null;
            }
        }

        /// <summary>The data's text, for a player build that cannot read the project's files: the next <see cref="Shared"/>
        /// is made from it. Null for either keeps the project's file for that one.</summary>
        public static void UseData(string normalsJson, string circulationJson)
        {
            lock (sharedGate)
            {
                if (normalsJson == normalsText && circulationJson == circulationText)
                    return;
                normalsText = normalsJson;
                circulationText = circulationJson;
                shared = null;
            }
        }

        static string ReadProjectFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"The weather engine's data is missing: {path} (read from the project folder; a build " +
                                                "must hand the files over with WeatherEngine.UseData, as LevelWeather does with its TextAssets)", path);
            return File.ReadAllText(path);
        }

        // ------------------------------------------------------------------------------------------- one engine

        public int Seed { get; }
        public WeatherModel Model { get; }
        public ClimateNormals Normals { get; }
        public CirculationChain Chain { get; }
        public SynopticField Field { get; }
        /// <summary>How strong the model's own systems are beside the circulation days' (a share of their depth).</summary>
        public double OwnSystemsWeight { get; }

        readonly int calibrationYears, calibrationPerDay;
        readonly object calibrationGate = new object();
        volatile bool calibrated;
        float[][] seaWinds, landWinds;                         // the model's own winds by month (sorted), for the quantile map

        public WeatherEngine(int seed, ClimateNormals normals, string circulationJson)
        {
            Seed = seed;
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            if (!(Json.Parse(circulationJson) is Dictionary<string, object> root))
                throw new FormatException("circulation.json: the top level is not an object");
            Chain = CirculationChain.Parse(root, seed);
            Field = new SynopticField(seed, Chain, root);
            OwnSystemsWeight = Math.Max(0.0, Json.Num(root, "ownSystemsWeight", 0.6));
            calibrationYears = Math.Max(1, (int)Json.Num(root, "calibrationYears", 3));
            calibrationPerDay = Math.Max(1, (int)Json.Num(root, "calibrationPerDay", 8));
            Model = new WeatherModel(seed) { engine = this };
        }

        /// <summary>Game hours since the start of year 0 for a date and an hour of the day (as WorldSim's weather clock:
        /// (year x 365.25 + day - 1) x 24 + hour).</summary>
        public static double HoursAt(int year, int dayOfYear, double hourOfDay) => ((double)year * 365.25 + (dayOfYear - 1)) * 24.0 + hourOfDay;

        /// <summary>The hour of the day (local, 0-24) at a moment: the hour given to <see cref="HoursAt"/>, which is not the
        /// game hours modulo 24 (a year is 365.25 days, so the year's first hour falls 0, 6, 12 or 18 hours into a day).</summary>
        public static double HourOfDay(double hours)
        {
            double days = hours / 24.0;
            double year = Math.Floor(days / 365.25 + 1e-9);
            double intoYear = days - year * 365.25;                       // whole days since the year began, and the day's share
            double h = (intoYear - Math.Floor(intoYear)) * 24.0;
            return h < 0.0 ? 0.0 : h >= 24.0 ? 0.0 : h;
        }

        /// <summary>The (fractional) year of a moment.</summary>
        public static double YearOf(double hours) => hours / HoursPerYear;

        // ------------------------------------------------------------------------------------------- sampling

        /// <summary>The weather at a place and hour for a place class (coast, wadden, channel_nl, river_land, north_sea):
        /// pressure, wind (from and speed), gusts, temperature, visibility, and the model's cloud, rain and fog.</summary>
        public WeatherSample Sample(double lat, double lon, double hours, string placeClass)
        {
            bool overSea = IsSea(placeClass);
            var snapshot = Model.Snapshot(hours);
            var raw = Model.Raw(snapshot, lat, lon, hours, overSea);
            return Finish(snapshot, raw, lat, lon, hours, overSea, placeClass);
        }

        /// <summary>The weather at a place and hour, the class worked out from the place (<see cref="ClassFor"/>): exactly
        /// what the model gives the world's ships (WorldSim's Sample(snapshot, lat, lon, hours, overSea)).</summary>
        public WeatherSample Sample(double lat, double lon, double hours, bool overSea) => Model.Sample(Model.Snapshot(hours), lat, lon, hours, overSea);

        /// <summary>Hourly samples from one hour to another (both included), for warm starts, the report and the checks.</summary>
        public WeatherSample[] HourlySamples(double lat, double lon, double fromHours, double toHours, string placeClass)
        {
            int n = Math.Max(0, (int)Math.Floor(toHours - fromHours + 1e-9) + 1);
            var a = new WeatherSample[n];
            for (int i = 0; i < n; i++)
                a[i] = Sample(lat, lon, fromHours + i, placeClass);
            return a;
        }

        /// <summary>The day's circulation type at a moment.</summary>
        public CirculationType TypeAt(double hours) => Chain.TypeAt(hours);

        /// <summary>The systems alive at a moment (the model's own and the circulation days').</summary>
        public List<WeatherSystemInfo> Systems(double hours) => Model.Systems(hours);

        /// <summary>Sea-level pressure at a place and moment, hPa.</summary>
        public double PressureAt(double lat, double lon, double hours) => Model.PressureAt(lat, lon, hours);

        public static bool IsSea(string placeClass) => placeClass != null && Array.IndexOf(SeaClasses, placeClass) >= 0;

        /// <summary>The place class whose station is nearest a place, among the sea classes for a place over water and the
        /// land ones otherwise (or any, if the file has none of that kind); null if the file has none.</summary>
        public string ClassFor(double lat, double lon, bool overSea)
        {
            string best = null, any = null;
            double bestKm = double.MaxValue, anyKm = double.MaxValue;
            foreach (var c in Normals.Classes)
            {
                double km = Km(lat, lon, c.lat, c.lon);
                if (km < anyKm)
                {
                    anyKm = km;
                    any = c.id;
                }
                bool sea = IsSea(c.id);
                // the coastal stations stand for the water off them too: the Wadden Sea and the Scheldt mouth
                bool fits = overSea ? sea || c.id == "wadden" || c.id == "channel_nl" : !sea;
                if (fits && km < bestKm)
                {
                    bestKm = km;
                    best = c.id;
                }
            }
            return best ?? any;
        }

        /// <summary>How much a class's measured climate holds at a place: in full within <see cref="ClassFullKm"/> of its
        /// station, fading to none at <see cref="ClassFadeKm"/> (beyond, the model's own weather stands).</summary>
        public static double ClassWeight(ClimateNormals.PlaceClass c, double lat, double lon)
        {
            double km = Km(lat, lon, c.lat, c.lon);
            return Math.Max(0.0, Math.Min(1.0, (ClassFadeKm - km) / (ClassFadeKm - ClassFullKm)));
        }

        /// <summary>
        /// The engine's local layer on the model's own sample (WeatherModel.Sample calls it for an engine's model): the wind
        /// fitted to the place's measured climate and turned by the sea breeze, gusts, the temperature, and the hour's sky
        /// (LocalSky: the frontal curves, the cloud layers and their kinds, the light through them, rain, dew point, fog,
        /// thunder and the visibility). The model's own cloud, rain and fog are replaced; its pressure and fronts lead.
        /// </summary>
        internal WeatherSample Finish(object snapshot, WeatherSample raw, double lat, double lon, double hours, bool overSea, string placeClass)
        {
            var w = raw;
            double doy = WeatherModel.DayOfYear(hours);
            double local = HourOfDay(hours);
            double ut = local - lon / 15.0;                                  // the clock keeps local mean time; KNMI's hours are UT
            var c = placeClass != null && Normals.Has(placeClass) ? Normals.Class(placeClass) : null;
            double weight = c != null ? ClassWeight(c, lat, lon) : 0.0;
            double summer = ClimateNormals.Summerness(doy);
            double offset = Normals.YearOffset(YearOf(hours), summer);
            double airMass = Field.AirMassAt(hours);
            // the spells the model's systems do not carry: runs of cold or mild days (the measured day-to-day memory of the
            // temperature, 0.8, is four or five days), deeper in winter, the cold ones deeper still (continental air)
            double spell = Spell(lat, lon, hours, summer);

            // the hour's cloud and rain first: they damp the day's warming and cooling
            var x = new LocalSky.Context
            {
                seed = Seed,
                lat = lat,
                lon = lon,
                hours = hours,
                dayOfYear = doy,
                hour = local,
                summer = summer,
                type = Chain.TypeAt(hours),
                front = Model.Fronts(snapshot, lat, lon, hours),
                overSea = overSea,
                place = c,
                seaTemp = Normals.SeaTemp(doy) + offset,
                airOffset = offset + airMass,
                spell = spell,
            };
            var sky = LocalSky.Clouds(x);

            // the wind: the model's, mapped onto the measured distribution of the class
            double speed = raw.windSpeed;
            if (weight > 0.0)
                speed += (MapWind(raw.windSpeed, overSea, c, doy, ut) - speed) * weight;
            speed = Math.Max(0.0, Math.Min(speed, Beaufort.Top));

            // the temperature: the normals and their day cycle, the model's warm and cold air, the day's air mass, the year
            double modelAir = raw.temperature - WeatherModel.ClimateTemperature(lat, lon, hours, overSea, local);
            double t2 = raw.temperature + offset;
            double dayMean = t2 - (overSea ? 0.8 : 4.5) * Math.Cos(2.0 * Math.PI * (local - 15.0) / 24.0);
            double landMax = t2;
            if (c != null)
            {
                ClimateNormals.Between(doy, out int m0, out int m1, out double t);
                double meanWind = c.months[m0].windMean + (c.months[m1].windMean - c.months[m0].windMean) * t;
                double usual = (1.0 - 0.6 * 0.65) * (1.0 - 0.4 * Math.Min(1.0, meanWind / 8.0));
                double now = (1.0 - 0.6 * sky.Cover) * (1.0 - 0.4 * Math.Min(1.0, speed / 8.0));
                // the measured cycle is the month's average over dull days and bright; a clear still day and night may swing
                // to the design's range (weather 2.3: 9 C inland, 4 at the coast, 1.5 at sea), however small the average
                double range = DayRange(c.months[m0]) * (1.0 - t) + DayRange(c.months[m1]) * t;
                double most = overSea ? 1.5 : LocalSky.SeaBearing(c, out _) ? 4.0 : 9.0;
                double cap = Math.Max(2.0, most / Math.Max(range, 0.5));
                double cycle = Math.Max(0.2, Math.Min(cap, Math.Pow(now / usual, CycleExponent))) * (overSea ? 0.35 : 1.0);
                double mean = Normals.TempMean(c, doy) + offset + modelAir + airMass;
                double normal = Normals.Temperature(c, doy, ut, cycle) + offset + modelAir + airMass;
                t2 += (normal - t2) * weight;
                dayMean += (mean - dayMean) * weight;
                // the land's afternoon warmth the sea breeze feels: the river land's normal maximum (the coast station sits on
                // the shore itself)
                var land = Normals.Has("river_land") ? Normals.Class("river_land") : c;
                landMax = land.months[m0].tMax + (land.months[m1].tMax - land.months[m0].tMax) * t + offset + airMass;
            }
            t2 += spell;
            dayMean += spell;
            w.temperature = (float)t2;

            // the sea breeze on fair coastal afternoons, and the night's land breeze
            double from = raw.windFrom;
            w.seaBreeze = (float)LocalSky.SeaBreeze(x, sky.low, landMax, ref speed, ref from);
            w.windSpeed = (float)speed;
            w.windFrom = (float)from;

            // gusts: the measured factor (1.52-1.58 on the coast), more in showers (1.75) and at a cold front (1.9)
            double g = 1.55;
            if (c != null)
            {
                ClimateNormals.Between(doy, out int m0, out int m1, out double t);
                g = c.months[m0].gustFactor + (c.months[m1].gustFactor - c.months[m0].gustFactor) * t;
            }
            if (sky.band > 0.5)
                g *= 1.9 / 1.55;
            else if (sky.showers > 0.2 || sky.cb > 0.2 || sky.cumulus > 0.3)
                g *= 1.75 / 1.55;
            w.gust = (float)(speed * g);

            // the rest of the sky now the air's temperature is known
            LocalSky.Air(ref w, x, ref sky, dayMean, w.gust);
            w.front = x.front.kind;
            w.frontPhase = (float)x.front.phase;
            w.warmSector = (float)x.front.warmSector;
            return w;
        }

        // the tuning of the temperature's spread (fitted to the measured frost days in the thirty-year runs)
        internal const double CycleExponent = 1.5, SpellWinter = 3.2, SpellSummer = 1.5, SpellColdSkew = 1.5;

        /// <summary>A slow anomaly of the air's temperature, C: smooth over four or five days and some hundreds of km, its
        /// spread SpellWinter in midwinter and SpellSummer in midsummer, cold spells in winter deeper by SpellColdSkew.</summary>
        double Spell(double lat, double lon, double hours, double summer)
        {
            double n = (LocalSky.Noise(Seed, 41, lat * 0.25, lon * 0.25, hours / 96.0) - 0.5) / 0.17;
            double a = SpellWinter + (SpellSummer - SpellWinter) * summer;
            if (n < 0.0)
                n *= 1.0 + (SpellColdSkew - 1.0) * (1.0 - summer);
            return a * n;
        }

        /// <summary>The measured day's range of a month's temperature (its hourly cycle's highest less its lowest), C.</summary>
        static double DayRange(ClimateNormals.Month m)
        {
            double lo = double.MaxValue, hi = double.MinValue;
            foreach (double v in m.tempByHour)
            {
                lo = Math.Min(lo, v);
                hi = Math.Max(hi, v);
            }
            return hi - lo;
        }

        /// <summary>The model's wind mapped onto the class's measured distribution at a day of the year and UT hour: the
        /// quantile of the model's own wind that month, read off the measured shares (the day's cycle divided out), and the
        /// measured day cycle put back; blended between the two months either side.</summary>
        public double MapWind(double modelWind, bool overSea, ClimateNormals.PlaceClass c, double dayOfYear, double utHour)
        {
            EnsureCalibrated();
            var cal = overSea ? seaWinds : landWinds;
            ClimateNormals.Between(dayOfYear, out int m0, out int m1, out double t);
            double s0 = ClimateNormals.WindAtQuantile(c.months[m0], Quantile(cal[m0], modelWind)) * ClimateNormals.WindByHour(c.months[m0], utHour);
            double s1 = ClimateNormals.WindAtQuantile(c.months[m1], Quantile(cal[m1], modelWind)) * ClimateNormals.WindByHour(c.months[m1], utHour);
            return s0 + (s1 - s0) * t;
        }

        /// <summary>Where a wind falls in the model's own distribution (0..1): the interpolated rank among the sorted
        /// calibration winds, the tails thinning beyond the smallest and largest.</summary>
        static double Quantile(float[] sorted, double x)
        {
            int n = sorted.Length;
            if (n == 0)
                return 0.5;
            if (x <= sorted[0])
                return 0.5 / n * Math.Max(0.0, x / Math.Max(sorted[0], 1e-3));
            if (x >= sorted[n - 1])
                return 1.0 - 0.5 / n * Math.Pow(sorted[n - 1] / x, 4.0);
            int lo = 0, hi = n - 1;                                         // sorted[lo] <= x < sorted[hi]
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (sorted[mid] <= x)
                    lo = mid;
                else
                    hi = mid;
            }
            double span = sorted[hi] - sorted[lo];
            double f = span > 1e-9 ? (x - sorted[lo]) / span : 0.0;
            return (lo + 0.5 + f) / n;
        }

        /// <summary>Samples the model's own wind (over sea and over land, at the North Sea's Dutch side) through a few years,
        /// once, and keeps it sorted by month (each month pooled with its neighbours), for the quantile map.</summary>
        void EnsureCalibrated()
        {
            if (calibrated)
                return;
            lock (calibrationGate)
            {
                if (calibrated)
                    return;
                seaWinds = Measure(52.4, 3.4, true);
                landWinds = Measure(52.05, 5.2, false);
                calibrated = true;
            }
        }

        float[][] Measure(double lat, double lon, bool overSea)
        {
            var lists = new List<float>[12];
            for (int m = 0; m < 12; m++)
                lists[m] = new List<float>();
            double start = CalibrationYear * HoursPerYear, end = start + calibrationYears * HoursPerYear;
            double step = 24.0 / calibrationPerDay;
            for (double h = start; h < end; h += step)
            {
                float v = Model.Raw(Model.Snapshot(h), lat, lon, h, overSea).windSpeed;
                int m = ClimateNormals.MonthOf(WeatherModel.DayOfYear(h));
                lists[m].Add(v);
                lists[(m + 11) % 12].Add(v);
                lists[(m + 1) % 12].Add(v);
            }
            var a = new float[12][];
            for (int m = 0; m < 12; m++)
            {
                a[m] = lists[m].ToArray();
                Array.Sort(a[m]);
            }
            return a;
        }

        static double Km(double lat, double lon, double lat0, double lon0)
        {
            double cosLat = Math.Cos((lat + lat0) * 0.5 * Math.PI / 180.0);
            double east = (lon - lon0) * 111.2 * cosLat, north = (lat - lat0) * 111.2;
            return Math.Sqrt(east * east + north * north);
        }
    }
}
