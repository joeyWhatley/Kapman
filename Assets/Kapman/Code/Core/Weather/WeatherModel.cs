// ported 4 Oct from the first build (Code/Weather/WeatherModel.cs), and the same day moved from Kapman.Game (Code/Game/Sim)
// to Kapman.Core (Code/Core/Weather), namespace kept, so WorldSim, the LocalWeather shim and SimHarness compile unchanged
// (the game plan 2.4.1: the sky is one model). The world's weather model (pressure systems over the whole North Sea,
// sampled at any latitude and longitude and hour) is what the simulation's ships sail in, headless and in the game alike.
// A model made on its own (new WeatherModel(793), as the tests and SimHarness do) is exactly the first build's. The one a
// WeatherEngine owns (WeatherEngine.Shared: the player's level, the world's ships and the shim all read it) is extended in
// place: the engine's circulation chain adds the day's systems to the ones this model makes, and every sample is finished
// by the engine's local layer (the wind fitted to the measured climate of its place, gusts, temperature from the normals
// and the year, the visibility as a range in metres). See WeatherEngine.
using System;
using System.Collections.Generic;

namespace Kapman.Weather
{
    /// <summary>A weather system as the test panel and the reports see it: where it is now and how deep.</summary>
    public struct WeatherSystemInfo
    {
        public bool low;
        /// <summary>Its centre, degrees.</summary>
        public double lat, lon;
        /// <summary>Its depth now against the season's mean field, hPa (negative: a low), its strength over its life included.</summary>
        public double depth;
        public double radiusKm;
        /// <summary>True for a circulation day's system (the engine's chain), false for the model's own.</summary>
        public bool fromChain;
    }

    /// <summary>What the weather is at one place and moment.</summary>
    public struct WeatherSample
    {
        /// <summary>Sea-level pressure, hPa.</summary>
        public float pressure;
        /// <summary>Compass degrees the wind blows from.</summary>
        public float windFrom;
        /// <summary>Wind speed at 10 m, metres a second (real).</summary>
        public float windSpeed;
        /// <summary>0 steady .. 1 very gusty.</summary>
        public float gustiness;
        /// <summary>Cover (0..1) of heaped cloud: fair-weather cumulus, and the cumulus of showery cold air.</summary>
        public float cumulus;
        /// <summary>Cover (0..1) of layered cloud: stratus, altostratus, nimbostratus.</summary>
        public float layered;
        /// <summary>Cover (0..1) of towering shower and thunder cloud (cumulonimbus).</summary>
        public float storm;
        /// <summary>Rain (or snow, when it's cold), mm an hour.</summary>
        public float rain;
        /// <summary>Fog, 0 none .. 1 thick.</summary>
        public float fog;
        /// <summary>Air temperature, °C.</summary>
        public float temperature;
        /// <summary>The gusts (3-second peaks) about the hour's mean wind, metres a second; 0 when not worked out (a model
        /// sampled on its own).</summary>
        public float gust;
        /// <summary>How far one can see: the meteorological optical range, metres (Koschmieder's 2% contrast, WMO); 0 when not
        /// worked out (a model sampled on its own), and then <see cref="VisibilityKm"/> falls back on the fog and rain.</summary>
        public float visibility;

        // the sky, worked out by the engine's local layer (WeatherEngine, weather document 2.3 and 2.5); all zero for a model
        // sampled on its own (HasSky false: the cover fields above then carry the whole sky)

        /// <summary>Dew point, °C.</summary>
        public float dewPoint;
        /// <summary>Cover (0..1) of each layer: low (0-2 km: stratus, stratocumulus, cumulus, and the base of nimbostratus
        /// and cumulonimbus), middle (2-7 km: altostratus, altocumulus), high (5-13 km: cirrus, cirrostratus).</summary>
        public float lowCloud, midCloud, highCloud;
        /// <summary>The kind of the lowest layer that covers a quarter of the sky or more (the high veil when nothing lower does).</summary>
        public CloudType cloudType;
        /// <summary>That layer's base above the ground and its depth, metres.</summary>
        public float cloudBase, cloudDepth;
        /// <summary>The share of the Sun's light the cloud column lets through to the ground, 0..1 (weather document 2.5):
        /// 1 clear, about 0.85 under cirrus, 0.01-0.025 under nimbostratus, 0.002-0.005 under a cumulonimbus. It scales the
        /// Sun's lux.</summary>
        public float cloudTransmittance;
        /// <summary>The nearest front of the low that rules the place: warm, cold or none.</summary>
        public FrontKind front;
        /// <summary>The signed distance to <see cref="front"/>, km: negative ahead of it (it has yet to pass), positive behind.</summary>
        public float frontPhase;
        /// <summary>0..1: how far into a low's warm sector (between its fronts: low grey cloud, drizzle, mild air).</summary>
        public float warmSector;
        /// <summary>Thunder, 0 none .. 1 a thunderstorm overhead.</summary>
        public float thunder;
        /// <summary>The sea breeze's share of the wind, 0..1 (fair spring and summer afternoons on the coast).</summary>
        public float seaBreeze;
        /// <summary>The top of the fog or haze above the ground, metres: radiation fog 30-120, sea fog 200-500, haze 1,000-2,500.</summary>
        public float fogTop;

        /// <summary>Whether the engine worked out the sky (layers, kinds, transmittance, dew point).</summary>
        public bool HasSky => cloudTransmittance > 0f;

        public bool Snow => temperature < 1f && rain > 0.05f;

        /// <summary>Sleet: rain and snow together, the air between 1 and 3 °C (weather document 2.3).</summary>
        public bool Sleet => rain > 0.05f && temperature >= 1f && temperature < 3f;

        /// <summary>The share of what falls that comes down as snow: all of it under 1 °C, none over 3 °C, sleet between.</summary>
        public float SnowShare => temperature <= 1f ? 1f : temperature >= 3f ? 0f : (3f - temperature) * 0.5f;
        public float Cover => Math.Max(cumulus, Math.Max(layered, storm));

        /// <summary>Visibility in real km: the worked-out range when there is one (<see cref="visibility"/>), else 40 in clear
        /// air, a few km in mist, under 1 km in fog (fog over 0.35) and under 200 m in thick fog (over 0.65).</summary>
        public float VisibilityKm
        {
            get
            {
                if (visibility > 0f)
                    return visibility / 1000f;
                // the haze of rain and the droplets of fog each dim the view; their dimming adds (per km)
                double dim = (1.0 + 0.35 * rain) / 40.0 + 16.0 * Math.Pow(Math.Max(fog, 0f), 2.6);
                return (float)Math.Max(0.05, 1.0 / dim);
            }
        }

        /// <summary>A few words for the HUD: "overcast, light rain, mist".</summary>
        public string Describe()
        {
            string sky = Cover < 0.12f ? "clear" : Cover < 0.35f ? (storm > 0.2f ? "showers about" : "fair, a few clouds")
                       : Cover < 0.65f ? (storm > layered && storm > 0.3f ? "showery" : "cloudy") : storm > 0.5f ? "stormy" : "overcast";
            string fall = rain < 0.05f ? "" : (Snow ? (rain < 0.5f ? ", light snow" : rain < 2.5f ? ", snow" : ", heavy snow")
                                                     : (rain < 0.4f ? ", drizzle" : rain < 2f ? ", rain" : rain < 5f ? ", heavy rain" : ", downpour"));
            string mist = fog > 0.65f ? ", thick fog" : fog > 0.35f ? ", fog" : fog > 0.12f ? ", mist" : "";
            return sky + fall + mist;
        }
    }

    /// <summary>Where a place stands against the fronts of the low that rules it (WeatherModel.Fronts; the engine reads it).</summary>
    internal struct FrontState
    {
        /// <summary>The nearer front of the ruling low (None: no front holds here).</summary>
        public FrontKind kind;
        /// <summary>The signed distance to it, km: negative ahead (yet to pass), positive behind.</summary>
        public double phase;
        /// <summary>How much that front holds here, 0..1: the low's vigour and life, faded towards the front's ends and away.</summary>
        public double weight;
        /// <summary>That low's vigour (its depth now over 22 hPa, up to 1.3).</summary>
        public double vigour;
        /// <summary>0..1 inside a warm sector, strongest near the low.</summary>
        public double warmSector;
        /// <summary>0..1 about a low's centre (within 0.6 of its radius): thick layered cloud.</summary>
        public double centre;
        /// <summary>The rain about the lows' centres, mm/h, as the model makes it.</summary>
        public double centreRain;
        /// <summary>How much the highs about clear the sky, 0..1.</summary>
        public double highs;
    }

    /// <summary>
    /// WEATHER MODEL - the weather anywhere on the map at any moment, the same every time for the same place and time
    /// (it's worked out, not stored). A simple "synoptic" model, the way a forecaster reads a chart:
    ///   - the climate: the seasons' mean pressure pattern - the Azores high, the Icelandic low, the Siberian high in
    ///     winter, the heat lows over Persia and the Sahara in summer - which alone gives the westerlies over the
    ///     North Sea, the Mediterranean's summer northerlies and the trades off Africa;
    ///   - weather systems on top: depressions that run north-east along the storm track from the mid-Atlantic (most
    ///     often and deepest about midwinter, deepest of all up by Iceland and Norway, each with a ridge building in
    ///     behind it; in winter a southern branch of shallower lows crosses Biscay and Brittany into France), Genoa lows in the Mediterranean from autumn to spring, and slow highs that bring settled spells
    ///     (most often in spring, over Norway and the northern North Sea: the North Sea's north-easterlies);
    ///   - each depression's fronts: rain ahead of the warm front, drizzle and low cloud in the warm sector, a band of
    ///     downpours on the cold front, then clearer, gustier air with showers.
    /// The wind is the pressure pattern's (geostrophic, slowed and turned towards low pressure by the surface). Fog comes
    /// at sea where warm or settled air drifts over the colder water in light to fresh winds (late winter and spring in
    /// the south, the summer haar in the northern isles), and over land on still autumn and winter nights.
    /// Times are game hours since the start of year 0; lat/lon in degrees.
    /// Thread-safe: it holds no state but the seed.
    /// </summary>
    public sealed class WeatherModel
    {
        readonly int seed;
        /// <summary>The engine this model belongs to (it adds the circulation chain's systems and finishes every sample), or
        /// null for a model on its own, which behaves exactly as the first build's.</summary>
        internal WeatherEngine engine;

        public WeatherModel(int seed) => this.seed = seed;

        /// <summary>The seed the weather systems are drawn from.</summary>
        public int Seed => seed;

        /// <summary>The engine that owns this model (WeatherEngine.Shared's for the game's one sky), or null.</summary>
        public WeatherEngine Engine => engine;

        /// <summary>The hour of the day (local, 0-24) at a moment. A model on its own reads it as the first build did (the
        /// game hours modulo 24); the engine's model reads the clock's own hour (WeatherEngine.HourOfDay), which differs by
        /// 6, 12 or 18 hours in three years out of four because a year is 365.25 days long.</summary>
        internal double LocalHour(double hours) => engine != null ? WeatherEngine.HourOfDay(hours) : (hours % 24.0 + 24.0) % 24.0;

        // ---------------------------------------------------------------------------------------- the climate

        /// <summary>How much of winter there is: 1 in mid-January, -1 in mid-July.</summary>
        public static double Winter(double hours) => Math.Cos(2.0 * Math.PI * (DayOfYear(hours) - 15.0) / 365.25);

        public static double DayOfYear(double hours) => (hours / 24.0) % 365.25;     // (Julian years, as the calendar's)

        /// <summary>How much of the spring blocking season there is: 1 in mid-April, fading out by February and late June.</summary>
        static double Spring(double hours)
        {
            double c = Math.Max(0.0, Math.Cos(2.0 * Math.PI * (DayOfYear(hours) - 105.0) / 365.25));
            return c * c;
        }

        static double Bump(double lat, double lon, double lat0, double lon0, double rLat, double rLon)
        {
            double a = (lat - lat0) / rLat, b = (lon - lon0) / rLon;
            return Math.Exp(-(a * a + b * b));
        }

        /// <summary>The season's mean sea-level pressure, hPa.</summary>
        public static double ClimatePressure(double lat, double lon, double hours)
        {
            double w = Winter(hours), s = -w, winterOnly = Math.Max(w, 0.0), summerOnly = Math.Max(s, 0.0);
            double p = 1013.0;
            p += (8.0 + 4.0 * s) * Bump(lat, lon, 35.5 + 3.0 * s, -27.0 - 6.0 * s, 12.0, 30.0);        // Azores high
            p += 3.0 * summerOnly * Bump(lat, lon, 46.0, 5.0, 10.0, 22.0);                            // its summer ridge into Europe
            p -= (11.0 + 6.0 * w) * Bump(lat, lon, 62.0, -28.0, 10.0, 26.0);                          // Icelandic low
            // the westerly belt: pressure falling polewards across the mid-latitudes, over the Atlantic and Europe
            // (0.65 hPa a degree in winter, 0.35 in summer, 0.25 in spring, the season of blocking highs and
            // north-easterlies: westerlies between systems, which a strong high can still block)
            double spring = Spring(hours);
            double belt = Math.Exp(-Math.Pow((lat - 55.0) / 13.0, 2.0)) * Math.Exp(-Math.Pow((lon + 5.0) / 55.0, 2.0));
            p -= (0.5 + 0.15 * w - 0.25 * spring) * (lat - 52.0) * belt;
            p += 16.0 * winterOnly * Bump(lat, lon, 52.0, 88.0, 18.0, 40.0);                          // Siberian high
            p += 6.0 * winterOnly * Bump(lat, lon, 72.0, -40.0, 8.0, 20.0);                           // Greenland's cold dome
            p -= 8.0 * summerOnly * Bump(lat, lon, 30.0, 60.0, 12.0, 25.0);                           // Persian heat low
            p -= 5.0 * summerOnly * Bump(lat, lon, 24.0, 2.0, 8.0, 16.0);                             // Saharan heat low
            p -= 3.0 * winterOnly * Bump(lat, lon, 39.0, 16.0, 6.0, 18.0);                            // Mediterranean winter trough
            return p;
        }

        /// <summary>The season's mean air temperature at the surface, °C (before the weather's warm and cold air).</summary>
        public static double ClimateTemperature(double lat, double lon, double hours, bool overSea) =>
            ClimateTemperature(lat, lon, hours, overSea, (hours % 24.0 + 24.0) % 24.0);

        /// <summary>The same at a given hour of the day (local, 0-24): the engine's model passes the clock's true hour.</summary>
        public static double ClimateTemperature(double lat, double lon, double hours, bool overSea, double hourOfDay)
        {
            double w = Winter(hours - 24.0 * 25.0);                                   // the air lags the sun by about a month
            // the further east, the harsher. At sea too: the open Atlantic side (the Channel, Shetland, Norway) swings
            // about 9 °C over the year, the shallow southern North Sea about 13, the Kattegat and Baltic about 15
            double continental = overSea ? Math.Min(0.8, Math.Max(0.45, 0.45 + 0.04 * (lon + 2.0) * Clamp01((lat - 44.0) / 4.0) - 0.05 * Math.Max(lat - 56.0, 0.0)))
                                         : 1.0 + 0.012 * Math.Max(lon, 0.0);
            double t = 14.5 - 0.55 * (lat - 45.0) - 9.5 * w * continental;
            double hour = hourOfDay;
            t += (overSea ? 0.8 : 4.5) * Math.Cos(2.0 * Math.PI * (hour - 15.0) / 24.0);
            return t;
        }

        // ---------------------------------------------------------------------------------------- weather systems

        /// <summary>A depression or a high, over its life.</summary>
        internal struct System
        {
            public double born, life;                 // game hours
            public double lat0, lon0;                 // where it was born
            public double bearing, turn, speed;       // compass degrees, degrees a day, km an hour
            public double depth, radius;              // hPa (negative: a depression), km
            public double deepen;                     // how much deeper it is for each degree north of 57 N (Atlantic lows)
            public double ridge;                      // the ridge that builds in behind it, as a share of its depth
            public bool low;
            /// <summary>A circulation day's system (SynopticField): it grows and fades on a Hann window over its life, so
            /// the systems of a run of days of one type, each a day apart and two days long, add up to a steady one.</summary>
            public bool day;

            /// <summary>Its depth where it is now: Atlantic lows are deepest up by Iceland and Norway, milder further south,
            /// and fill once they run in over Scandinavia and the continent.</summary>
            public double DepthAt(double lat, double lon) => deepen == 0.0 ? depth
                : depth * Math.Min(1.2, Math.Max(0.8, 1.0 + deepen * (lat - 57.0))) * Math.Max(0.5, 1.0 - 0.025 * Math.Max(lon - 5.0, 0.0));

            /// <summary>Where it is and which way it's heading, at an age (hours).</summary>
            public void At(double age, out double lat, out double lon, out double heading)
            {
                heading = bearing + turn * age / 24.0;
                double mid = (bearing + heading) * 0.5 * Math.PI / 180.0;
                double km = speed * age;
                lat = lat0 + km * Math.Cos(mid) / 111.2;
                double cosLat = Math.Cos((lat0 + lat) * 0.5 * Math.PI / 180.0);
                lon = lon0 + km * Math.Sin(mid) / (111.2 * Math.Max(cosLat, 0.2));
            }

            /// <summary>How strong it is at an age: it deepens, then fills.</summary>
            public double Strength(double age) => age <= 0 || age >= life ? 0.0
                : day ? 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * age / life) : Math.Pow(Math.Sin(Math.PI * age / life), 0.7);
        }

        /// <summary>The kinds of system: how often slots come up, and how each is made.</summary>
        enum Kind { AtlanticLow, GenoaLow, High }

        static double Interval(Kind k) => k == Kind.AtlanticLow ? 18.0 : k == Kind.GenoaLow ? 48.0 : 72.0;
        const double LongestLife = 170.0;
        /// <summary>Room for the systems alive at one moment: the model's own (about two dozen) and the circulation days'.</summary>
        const int MaxAlive = 64;
        static readonly Kind[] Kinds = { Kind.AtlanticLow, Kind.GenoaLow, Kind.High };

        /// <summary>The systems alive at a moment (at most a few dozen): the model's own, and with an engine the circulation
        /// chain's too (the model's own then weakened by the engine's share, so the day's type leads).</summary>
        int Alive(double hours, System[] into)
        {
            int n = 0;
            double own = engine != null ? engine.OwnSystemsWeight : 1.0;
            foreach (Kind kind in Kinds)
            {
                double interval = Interval(kind);
                long first = (long)Math.Floor((hours - LongestLife) / interval) - 1, last = (long)Math.Floor(hours / interval) + 1;
                for (long slot = first; slot <= last && n < into.Length; slot++)
                    if (Make(kind, slot, out var s) && hours >= s.born && hours < s.born + s.life)
                    {
                        s.depth *= own;
                        into[n++] = s;
                    }
            }
            if (engine != null)
                n = engine.Field.AddSystems(hours, into, n);
            return n;
        }

        /// <summary>The system of a slot, if there is one (its season decides how likely one is).</summary>
        bool Make(Kind kind, long slot, out System s)
        {
            var r = new Random(unchecked((int)Hash(seed, (int)kind, slot)));
            double interval = Interval(kind);
            s = new System { born = slot * interval + (r.NextDouble() - 0.5) * interval * 0.8 };
            double w = Winter(s.born), spring = Spring(s.born);
            double chance;
            switch (kind)
            {
                case Kind.AtlanticLow:
                    // the storm season runs ahead of the cold: stormiest about midwinter, autumn far stormier than spring
                    double gales = Math.Max(-1.0, Math.Min(1.0, 1.3 * Math.Cos(2.0 * Math.PI * (DayOfYear(s.born) + 10.0) / 365.25)));
                    double blocking = Math.Max(0.0, Math.Cos(2.0 * Math.PI * (DayOfYear(s.born) - 120.0) / 365.25));
                    blocking *= blocking;                                                 // April and May
                    chance = 0.56 + 0.12 * gales + 0.15 * Math.Max(gales, 0.0);                                         // one every ~26 h in winter, ~41 h in summer
                    s.low = true;
                    // born in the mid-Atlantic, crossing the North Sea's longitudes mostly at 55-65 N (Scotland to
                    // Norway), spread wide enough that some pass north by Iceland and some south over the Channel;
                    // a more southerly track in summer, and more so under the spring blocking
                    s.lat0 = 51.0 - 3.0 * Math.Max(gales, 0.0) - 2.0 * Math.Max(-gales, 0.0) - 4.0 * blocking + Normal(r) * 4.5;
                    s.lon0 = -42.0 + Normal(r) * 9.0;
                    s.bearing = 68.0 + 2.0 * gales + Normal(r) * 10.0;
                    bool southern = gales > 0.0 && r.NextDouble() < 0.2 * gales;
                    if (southern)
                    {
                        // the southern branch of the winter storm track: lows that cross Biscay and Brittany into France
                        s.lat0 = 41.0 + Normal(r) * 1.5;
                        s.bearing = 82.0 + Normal(r) * 4.0;
                    }
                    s.turn = -2.0 + Normal(r) * 3.5;                                      // curving poleward
                    s.speed = 44.0 + 6.0 * gales + Normal(r) * 10.0;
                    s.life = 100.0 + r.NextDouble() * 60.0;
                    // fairly alike in depth and size (the deep, tight ones made too many storms), deeper in the north
                    s.depth = -(17.5 + 2.5 * Math.Max(gales, 0.0) + 4.5 * Math.Min(gales, 0.0) + 3.0 * blocking) * (0.85 + r.NextDouble() * 0.3);
                    if (southern)
                        s.depth *= 0.85;
                    s.radius = 750.0 + Normal(r) * 100.0;
                    s.deepen = 0.045;
                    s.ridge = 0.45;
                    break;
                case Kind.GenoaLow:
                    chance = Math.Max(0.0, 0.35 + 0.45 * w);                              // autumn to spring
                    s.low = true;
                    s.lat0 = 43.8 + Normal(r) * 1.2;
                    s.lon0 = 9.0 + Normal(r) * 2.5;
                    s.bearing = 112.0 + Normal(r) * 22.0;                                 // down the Adriatic and on to the Aegean
                    s.turn = Normal(r) * 6.0;
                    s.speed = 26.0 + Normal(r) * 7.0;
                    s.life = 48.0 + r.NextDouble() * 36.0;
                    s.depth = -(9.0 + 3.0 * w + Normal(r) * 3.0);
                    s.radius = 600.0 + Normal(r) * 140.0;
                    break;
                default:
                    // settled spells, most and strongest in spring: blocking highs over Norway and the northern North Sea
                    // that bring the southern North Sea its north-easterlies; nearer Scandinavia in winter, the Azores
                    // ridge west of Ireland in summer. Slow, so born a little west of where they stand at mid-life
                    chance = 0.35 + 0.15 * Math.Max(w, 0.0) + 0.5 * spring + 0.15 * Math.Max(-w, 0.0);
                    s.low = false;
                    s.lat0 = 57.0 + 2.0 * Math.Max(w, 0.0) + 6.0 * spring - 4.0 * Math.Max(-w, 0.0) + Normal(r) * 4.5;
                    s.lon0 = 6.0 - 10.0 * Math.Max(w, 0.0) - 10.0 * spring - 24.0 * Math.Max(-w, 0.0) + Normal(r) * 12.0;
                    s.bearing = 95.0 + Normal(r) * 35.0;
                    s.turn = Normal(r) * 10.0;
                    s.speed = 7.0 + Normal(r) * 4.0;
                    s.life = 80.0 + r.NextDouble() * 80.0;
                    s.depth = 12.0 + 6.0 * Math.Max(w, 0.0) + 10.0 * spring + r.NextDouble() * 12.0;
                    s.radius = 1100.0 + Normal(r) * 250.0;
                    break;
            }
            s.speed = Math.Max(s.speed, 5.0);
            s.radius = Math.Max(s.radius, 300.0);
            s.depth = s.low ? Math.Min(s.depth, -4.0) : s.depth;
            return r.NextDouble() < chance;
        }

        static double Normal(Random r)
        {
            double u = 1.0 - r.NextDouble(), v = r.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
        }

        static ulong Hash(int a, int b, long c)
        {
            unchecked
            {
                ulong h = 1469598103934665603UL;
                h = (h ^ (ulong)a) * 1099511628211UL;
                h = (h ^ (ulong)b) * 1099511628211UL;
                h = (h ^ (ulong)c) * 1099511628211UL;
                h ^= h >> 29;
                h *= 0xBF58476D1CE4E5B9UL;
                h ^= h >> 32;
                return h;
            }
        }

        // ---------------------------------------------------------------------------------------- the weather

        /// <summary>Sea-level pressure, hPa: the climate and the systems.</summary>
        double Pressure(double lat, double lon, double hours, System[] alive, int n)
        {
            double p = ClimatePressure(lat, lon, hours);
            for (int i = 0; i < n; i++)
            {
                double age = hours - alive[i].born;
                alive[i].At(age, out double clat, out double clon, out _);
                double km = Km(lat, lon, clat, clon, out double east, out double north);
                double R = alive[i].radius, x = km / R, d = alive[i].DepthAt(clat, clon) * alive[i].Strength(age);
                p += d * Math.Exp(-x * x);
                // the ridge behind it, some way west-south-west: between the two, the north-westerly that follows a low
                if (alive[i].ridge > 0.0)
                {
                    double re = (east + 1.5 * R) / R, rn = (north + 0.55 * R) / R;
                    p -= alive[i].ridge * d * Math.Exp(-(re * re + rn * rn));
                }
            }
            return p;
        }

        /// <summary>The weather at a place and moment. overSea: whether it's over water (a cloud map can pass null).</summary>
        public WeatherSample Sample(double lat, double lon, double hours, bool overSea) => Sample(Snapshot(hours), lat, lon, hours, overSea);

        /// <summary>The weather at a place, from a <see cref="Snapshot"/> taken for the same moment: for many places at once
        /// (the world's ships each hour) without working out the weather systems again for each. With an engine the sample
        /// is finished by it for the place's class (WeatherEngine.ClassFor), so the fleets and the level share one sky.</summary>
        public WeatherSample Sample(object snapshot, double lat, double lon, double hours, bool overSea)
        {
            var w = Raw(snapshot, lat, lon, hours, overSea);
            return engine == null ? w : engine.Finish(snapshot, w, lat, lon, hours, overSea, engine.ClassFor(lat, lon, overSea));
        }

        /// <summary>The model's own sample at a place, before an engine finishes it.</summary>
        internal WeatherSample Raw(object snapshot, double lat, double lon, double hours, bool overSea)
        {
            var alive = (System[])snapshot;
            int n = alive.Length;
            var w = Clouds(lat, lon, hours, overSea, alive, n, out double warmth);

            // the wind: the pressure pattern's, slowed and turned in towards low pressure by the sea or land below
            const double step = 0.25;                                                        // degrees
            double cosLat = Math.Cos(lat * Math.PI / 180.0);
            double dpdy = (Pressure(lat + step, lon, hours, alive, n) - Pressure(lat - step, lon, hours, alive, n)) * 100.0 / (2.0 * step * 111200.0);
            double dpdx = (Pressure(lat, lon + step, hours, alive, n) - Pressure(lat, lon - step, hours, alive, n)) * 100.0 / (2.0 * step * 111200.0 * Math.Max(cosLat, 0.2));
            double f = 2.0 * 7.292e-5 * Math.Sin(Math.Max(Math.Abs(lat), 12.0) * Math.PI / 180.0);
            double u = -dpdy / (1.25 * f), v = dpdx / (1.25 * f);                          // geostrophic east, north (m/s)
            double turnIn = (overSea ? 12.0 : 32.0) * Math.PI / 180.0;                       // backed towards low pressure (less over the smooth sea)
            double us = u * Math.Cos(turnIn) - v * Math.Sin(turnIn), vs = u * Math.Sin(turnIn) + v * Math.Cos(turnIn);
            // the surface keeps less of a strong gradient's wind (drag grows with the wind, and the wind curving round a
            // tight low runs slower than its gradient) and a little more of a weak one's
            double slow = Math.Min(1.0, (overSea ? 0.74 : 0.51) * Math.Pow(Math.Max(Math.Sqrt(u * u + v * v), 1.0) / 15.0, -0.3));
            us *= slow;
            vs *= slow;
            double speed = Math.Min(Math.Sqrt(us * us + vs * vs), 33.0);
            // the hour's wind wanders about the pattern's (squalls, showers, small troughs the chart doesn't show)
            speed *= 0.7 + 0.6 * Noise(lat * 2.0, lon * 2.0, hours / 3.0, 13);
            // a light breeze always stirs (sea breezes, eddies): never quite calm for long
            speed = Math.Max(speed, 0.8 + 1.2 * Noise(lat * 1.3, lon * 1.3, hours / 9.0, 11));
            w.windSpeed = (float)speed;
            w.windFrom = (float)((Math.Atan2(-us, -vs) * 180.0 / Math.PI + 360.0) % 360.0);
            w.pressure = (float)Pressure(lat, lon, hours, alive, n);
            w.gustiness = (float)Math.Min(1.0, 0.18 + (overSea ? 0.0 : 0.2) + 0.35 * w.storm + 0.25 * w.cumulus);

            // fog: moist air over something colder. At sea it's warm air drifting over the cold water, which a fresh
            // breeze (3-8 m/s) still carries; over land, still nights
            double doy = DayOfYear(hours), hour = LocalHour(hours);
            double calm = Clamp01((8.0 - speed) / 6.0);
            double drift = Clamp01((9.0 - speed) / 5.0);
            // sea fog's season: while the sea is coldest against the air, February to May in the south and the Baltic,
            // sliding to the summer haar (peak early July) in the northern isles; some all year
            double haar = Clamp01((lat - 55.5) / 4.0) * Clamp01((4.0 - lon) / 5.0);
            double seaFogSeason = 0.5 + 0.25 * (1.0 + Math.Cos(2.0 * Math.PI * (doy - 55.0 - 130.0 * haar) / 365.25));
            double nightFogSeason = Math.Max(0.0, Math.Cos(2.0 * Math.PI * (doy - 330.0) / 365.25));   // autumn and winter
            double night = Clamp01(Math.Cos(2.0 * Math.PI * (hour - 5.0) / 24.0) * 1.6);   // strongest near dawn
            double settled = Clamp01((w.pressure - 1014.0) / 10.0);
            double patches = Noise(lat * 2.2, lon * 2.2, hours / (overSea ? 3.5 : 10.0), 5);   // sea fog comes and goes in banks
            double fog = overSea
                ? drift * seaFogSeason * Clamp01(warmth + settled * 0.4 + 0.2 * haar * Clamp01(-Winter(hours) + 0.2)) * (0.3 + patches)
                : calm * calm * nightFogSeason * night * (0.3 + settled) * (0.3 + 1.0 * patches);
            w.fog = (float)Clamp01(fog * 2.3 - 0.12);

            w.temperature = (float)(ClimateTemperature(lat, lon, hours, overSea, LocalHour(hours)) + 3.5 * warmth);
            return w;
        }

        /// <summary>Just the clouds, rain and warmth (for cloud maps: no wind or fog).</summary>
        public void CloudsAt(double lat, double lon, double hours, bool overSea, out float cumulus, out float layered, out float storm, out float rain)
        {
            var alive = new System[MaxAlive];
            int n = Alive(hours, alive);
            var w = Clouds(lat, lon, hours, overSea, alive, n, out _);
            cumulus = w.cumulus;
            layered = w.layered;
            storm = w.storm;
            rain = w.rain;
        }

        /// <summary>The systems alive at a moment, where they are and how deep (for the test panel and the reports).</summary>
        public List<WeatherSystemInfo> Systems(double hours)
        {
            var alive = new System[MaxAlive];
            int n = Alive(hours, alive);
            var list = new List<WeatherSystemInfo>(n);
            for (int i = 0; i < n; i++)
            {
                var s = alive[i];
                double age = hours - s.born;
                s.At(age, out double lat, out double lon, out _);
                list.Add(new WeatherSystemInfo
                {
                    low = s.low, lat = lat, lon = lon, depth = s.DepthAt(lat, lon) * s.Strength(age), radiusKm = s.radius, fromChain = s.day,
                });
            }
            return list;
        }

        /// <summary>Sea-level pressure at a place and moment, hPa: the season's mean field and every system alive.</summary>
        public double PressureAt(double lat, double lon, double hours)
        {
            var alive = new System[MaxAlive];
            int n = Alive(hours, alive);
            return Pressure(lat, lon, hours, alive, n);
        }

        /// <summary>The systems alive now, reused for many places at once (a cloud map).</summary>
        public object Snapshot(double hours)
        {
            var alive = new System[MaxAlive];
            int n = Alive(hours, alive);
            Array.Resize(ref alive, n);
            return alive;
        }

        /// <summary>Clouds at a place, from a Snapshot taken for the same moment.</summary>
        public void CloudsAt(object snapshot, double lat, double lon, double hours, bool overSea, out float cumulus, out float layered, out float storm, out float rain)
        {
            var alive = (System[])snapshot;
            var w = Clouds(lat, lon, hours, overSea, alive, alive.Length, out _);
            cumulus = w.cumulus;
            layered = w.layered;
            storm = w.storm;
            rain = w.rain;
        }

        WeatherSample Clouds(double lat, double lon, double hours, bool overSea, System[] alive, int n, out double warmth)
        {
            double cumulus = 0, layered = 0, storm = 0, rain = 0, highs = 0;
            warmth = 0;
            for (int i = 0; i < n; i++)
            {
                var s = alive[i];
                double age = hours - s.born, strength = s.Strength(age);
                if (strength <= 0.01)
                    continue;
                s.At(age, out double clat, out double clon, out double heading);
                double dc = Km(lat, lon, clat, clon, out double east, out double north);
                if (!s.low)
                {
                    double x = dc / s.radius;
                    highs = Math.Max(highs, strength * Math.Exp(-x * x) * Math.Min(1.0, s.depth / 14.0));
                    continue;
                }
                // how vigorous it is: the rain goes with its depth, but even a shallow low spreads a wide sheet of cloud
                double vigour = Math.Min(1.3, -s.DepthAt(clat, clon) * strength / 22.0);
                double a = Math.Min(1.0, 0.45 + 0.55 * vigour) * Math.Min(1.0, strength * 2.5);
                double R = s.radius, life = age / s.life;

                // near the centre: thick layered cloud and rain
                double centre = Clamp01(1.0 - dc / (0.6 * R));
                layered = Math.Max(layered, a * Smooth(Clamp01(centre * 1.6)));
                rain += vigour * 2.2 * Clamp01(1.0 - dc / (0.33 * R));

                // the fronts, as rays from the centre: the warm sector lies between them (clockwise from the warm front),
                // and narrows as the cold front catches up (the occlusion)
                double wide = 90.0 * (1.0 - 0.65 * life);
                double warmBearing = heading + 80.0 + 0.4 * (90.0 - wide), coldBearing = warmBearing + wide;
                double warmLen = 0.95 * R * (1.0 - 0.45 * life), coldLen = 1.7 * R;
                Front(east, north, warmBearing, warmLen, out double wAlong, out double wAcross);
                Front(east, north, coldBearing, coldLen, out double cAlong, out double cAcross);

                // ahead of the warm front (its left, the cold side): cloud thickening to rain
                if (wAlong > -100 && wAlong < warmLen + 150 && wAcross < 0)
                {
                    double d = -wAcross, fade = AlongFade(wAlong, warmLen);
                    layered = Math.Max(layered, a * fade * Clamp01(1.15 - Math.Pow(d / 550.0, 1.5)));
                    rain += (0.25 + vigour) * fade * 1.8 * Clamp01(1.0 - d / 230.0);
                }
                // the warm sector: low grey cloud, drizzle, mild
                bool inWarm = Between(Bearing(east, north), warmBearing, coldBearing) && dc < coldLen;
                if (inWarm)
                {
                    double fade = Clamp01(1.0 - dc / coldLen);
                    layered = Math.Max(layered, a * 0.85 * Smooth(Clamp01(fade * 2.5)));
                    rain += vigour * 0.3 * fade;
                    warmth = Math.Max(warmth, vigour * fade);
                }
                // the cold front: a narrow band of towering cloud and downpours
                if (cAlong > -50 && cAlong < coldLen + 100)
                {
                    double d = Math.Abs(cAcross), fade = AlongFade(cAlong, coldLen);
                    storm = Math.Max(storm, a * fade * Clamp01(1.0 - d / 90.0));
                    layered = Math.Max(layered, a * fade * 0.85 * Clamp01(1.0 - d / 160.0));
                    rain += (0.25 + vigour) * fade * 5.0 * Clamp01(1.0 - d / 65.0);
                    // behind it (its right, the cold side): brighter, gusty air with heaped cloud and showers
                    if (cAcross > 40 && !inWarm)
                    {
                        double behind = Clamp01(1.0 - (cAcross - 40.0) / 800.0) * fade;
                        double showers = Noise(lat * 3.0, lon * 3.0, hours / 3.0, 7);
                        cumulus = Math.Max(cumulus, a * 0.65 * behind);
                        storm = Math.Max(storm, a * 0.5 * behind * Clamp01(showers * 1.6 - 0.45));
                        rain += vigour * 1.4 * behind * Clamp01(showers * 2.0 - 1.1);
                        warmth = Math.Min(warmth, -vigour * behind);
                    }
                }
            }

            // settled weather: highs clear the sky, but summer days still build fair-weather cumulus over land, and
            // hot afternoons the odd thunderstorm
            double doy = DayOfYear(hours), hour = LocalHour(hours);
            double summer = Math.Max(0.0, -Winter(hours) * 0.8 + 0.3);
            double day = Clamp01(Math.Sin(Math.PI * (hour - 9.0) / 10.0));               // 9:00 to 19:00, peaking mid-afternoon
            double convection = summer * day * (overSea ? 0.25 : 1.0);
            double texture = Noise(lat * 4.0, lon * 4.0, hours / 4.0, 3);
            cumulus = Math.Max(cumulus, convection * (0.25 + 0.45 * texture));
            storm = Math.Max(storm, convection * Clamp01(texture * 2.2 - 1.55) * 0.8);
            rain += convection * Clamp01(texture * 2.2 - 1.6) * 3.0;
            // a background of cloud everywhere, patchy: more in the cloudy north and west, and a grey winter overcast
            // (a North Sea January is mostly overcast; the Mediterranean summer mostly clear)
            double northWest = Clamp01((lat - 45.0) / 15.0) * Clamp01((25.0 - lon) / 30.0);
            double background = 0.18 + 0.18 * northWest + (0.2 + 0.25 * northWest) * Math.Max(Winter(hours), 0.0)
                              + 0.22 * Noise(lat * 1.5, lon * 1.5, hours / 12.0, 9);
            layered = Math.Max(layered, background * Clamp01((lat - 20.0) / 10.0));     // less in the dry subtropics
            double clear = 1.0 - 0.6 * Clamp01(highs);                                   // highs thin the cloud, rarely clear it
            cumulus *= clear;
            layered *= clear;
            rain *= clear;

            // patchiness: the edges of cloud break up
            double breakup = 0.8 + 0.4 * Noise(lat * 6.0, lon * 6.0, hours / 2.0, 1);
            return new WeatherSample
            {
                cumulus = (float)Clamp01(cumulus * breakup),
                layered = (float)Clamp01(layered * breakup),
                storm = (float)Clamp01(storm),
                rain = (float)Math.Max(0.0, rain * breakup),
            };
        }

        /// <summary>
        /// Where a place stands against the lows' fronts at a moment (weather document 2.2), from a <see cref="Snapshot"/> for
        /// the same moment. The fronts are drawn exactly as <see cref="Clouds"/> draws them: rays from each low's centre, the
        /// warm front ahead of the cold, the warm sector between them narrowing as the low occludes. Of the lows about, the
        /// one whose front holds most here (its vigour and life, faded towards the front's ends and with the distance) rules,
        /// and the place gets the signed distance to its nearer front: ahead of the warm front (negative), in the warm sector
        /// (behind the warm front, or ahead of the cold one), or behind the cold front (positive).
        /// </summary>
        internal FrontState Fronts(object snapshot, double lat, double lon, double hours)
        {
            var alive = (System[])snapshot;
            var f = new FrontState();
            for (int i = 0; i < alive.Length; i++)
            {
                var s = alive[i];
                double age = hours - s.born, strength = s.Strength(age);
                if (strength <= 0.01)
                    continue;
                s.At(age, out double clat, out double clon, out double heading);
                double dc = Km(lat, lon, clat, clon, out double east, out double north);
                if (!s.low)
                {
                    double x = dc / s.radius;
                    f.highs = Math.Max(f.highs, strength * Math.Exp(-x * x) * Math.Min(1.0, s.depth / 14.0));
                    continue;
                }
                double vigour = Math.Min(1.3, -s.DepthAt(clat, clon) * strength / 22.0);
                double a = Math.Min(1.0, 0.45 + 0.55 * vigour) * Math.Min(1.0, strength * 2.5);
                double R = s.radius, life = age / s.life;
                f.centre = Math.Max(f.centre, a * Smooth(Clamp01((1.0 - dc / (0.6 * R)) * 1.6)));
                f.centreRain += vigour * 2.2 * Clamp01(1.0 - dc / (0.33 * R));

                double wide = 90.0 * (1.0 - 0.65 * life);
                double warmBearing = heading + 80.0 + 0.4 * (90.0 - wide), coldBearing = warmBearing + wide;
                double warmLen = 0.95 * R * (1.0 - 0.45 * life), coldLen = 1.7 * R;
                Front(east, north, warmBearing, warmLen, out double wAlong, out double wAcross);
                Front(east, north, coldBearing, coldLen, out double cAlong, out double cAcross);
                bool inWarm = Between(Bearing(east, north), warmBearing, coldBearing) && dc < coldLen;
                double warmFade = inWarm ? Clamp01(1.0 - dc / coldLen) : 0.0;
                f.warmSector = Math.Max(f.warmSector, a * Smooth(Clamp01(warmFade * 2.5)));

                // this low's nearer front here, and how much it holds
                bool wOk = wAlong > -100.0 && wAlong < warmLen + 150.0, cOk = cAlong > -50.0 && cAlong < coldLen + 100.0;
                double wHold = wOk ? AlongFade(wAlong, warmLen) : 0.0, cHold = cOk ? AlongFade(cAlong, coldLen) : 0.0;
                FrontKind kind = FrontKind.None;
                double phase = 0.0, hold = 0.0;
                if (inWarm)
                {
                    // between the fronts: behind the warm one (+) or ahead of the cold one (-), whichever is nearer
                    bool cold = cOk && (!wOk || -cAcross < Math.Abs(wAcross));
                    kind = cold ? FrontKind.Cold : FrontKind.Warm;
                    phase = cold ? Math.Min(cAcross, 0.0) : Math.Abs(wAcross);
                    hold = a * Math.Max(Smooth(Clamp01(warmFade * 2.5)), cold ? cHold : wHold);
                }
                else
                {
                    bool ahead = wOk && wAcross < 0.0, behind = cOk && cAcross > 0.0;
                    if (ahead && (!behind || -wAcross < cAcross))
                    {
                        kind = FrontKind.Warm;
                        phase = wAcross;
                        hold = a * wHold * Clamp01(1.0 + wAcross / 1200.0);
                    }
                    else if (behind)
                    {
                        kind = FrontKind.Cold;
                        phase = cAcross;
                        hold = a * cHold * Clamp01(1.0 - cAcross / 800.0);
                    }
                }
                if (kind != FrontKind.None && hold > f.weight)
                {
                    f.kind = kind;
                    f.phase = phase;
                    f.weight = hold;
                    f.vigour = vigour;
                }
            }
            f.weight = Clamp01(f.weight);
            return f;
        }

        /// <summary>Where a point lies against a front drawn as a ray from the centre: km along it, and across it (+ right).</summary>
        static void Front(double east, double north, double bearing, double length, out double along, out double across)
        {
            double b = bearing * Math.PI / 180.0;
            double dx = Math.Sin(b), dy = Math.Cos(b);
            along = east * dx + north * dy;
            across = east * dy - north * dx;                                                // right of the ray is positive
        }

        static double AlongFade(double along, double length) => Clamp01(Math.Min((along + 100.0) / 200.0, (length + 150.0 - along) / 300.0));

        static double Bearing(double east, double north) => (Math.Atan2(east, north) * 180.0 / Math.PI + 360.0) % 360.0;

        /// <summary>Whether a bearing lies clockwise between two others.</summary>
        static bool Between(double b, double from, double to)
        {
            double span = ((to - from) % 360.0 + 360.0) % 360.0, off = ((b - from) % 360.0 + 360.0) % 360.0;
            return off <= span;
        }

        /// <summary>Great-circle km between two points (flat-earth close up; fine at these sizes), and the offset east and north.</summary>
        static double Km(double lat, double lon, double lat0, double lon0, out double east, out double north)
        {
            double cosLat = Math.Cos((lat + lat0) * 0.5 * Math.PI / 180.0);
            double dLon = lon - lon0;
            if (dLon > 180.0) dLon -= 360.0;
            if (dLon < -180.0) dLon += 360.0;
            east = dLon * 111.2 * cosLat;
            north = (lat - lat0) * 111.2;
            return Math.Sqrt(east * east + north * north);
        }

        static double Clamp01(double x) => x < 0.0 ? 0.0 : x > 1.0 ? 1.0 : x;
        static double Smooth(double x) => x * x * (3.0 - 2.0 * x);

        /// <summary>Smooth value noise in 0..1 over (x, y, t), for patchiness.</summary>
        double Noise(double x, double y, double t, int channel)
        {
            double Layer(double px, double py, double pt)
            {
                long ix = (long)Math.Floor(px), iy = (long)Math.Floor(py), it = (long)Math.Floor(pt);
                double fx = Smooth(px - ix), fy = Smooth(py - iy), ft = Smooth(pt - it);
                double V(long a, long b, long c) => (Hash(seed + channel * 7919, (int)(a * 73856093 ^ b * 19349663), c) & 0xFFFFFF) / (double)0xFFFFFF;
                double x0 = Lerp(Lerp(V(ix, iy, it), V(ix + 1, iy, it), fx), Lerp(V(ix, iy + 1, it), V(ix + 1, iy + 1, it), fx), fy);
                double x1 = Lerp(Lerp(V(ix, iy, it + 1), V(ix + 1, iy, it + 1), fx), Lerp(V(ix, iy + 1, it + 1), V(ix + 1, iy + 1, it + 1), fx), fy);
                return Lerp(x0, x1, ft);
            }
            return 0.65 * Layer(x, y, t) + 0.35 * Layer(x * 2.3 + 17.1, y * 2.3 + 5.3, t * 1.7 + 3.1);
        }

        static double Lerp(double a, double b, double t) => a + (b - a) * t;
    }
}
