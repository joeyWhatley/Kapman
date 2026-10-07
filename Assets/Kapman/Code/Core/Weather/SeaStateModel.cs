using System;
using System.Collections.Generic;
using Kapman.Game;

namespace Kapman.Weather
{
    /// <summary>A swell train: the long low sea a storm far off sends ahead of itself (weather document 2.4).</summary>
    public struct SwellTrain
    {
        /// <summary>Its significant height when it has fully arrived (m), its peak period (s), and the compass bearing it
        /// comes from.</summary>
        public float hs, tp, from;
        /// <summary>When its front arrives, game hours.</summary>
        public double arrives;
        /// <summary>How far it has come, km.</summary>
        public float km;

        public bool Live => hs > 0f;

        /// <summary>Its height at a moment (m): nothing before its front, rising over six hours, then dying away with an
        /// e-fold of a day (weather 2.4).</summary>
        public float HeightAt(double hours)
        {
            if (hs <= 0f || hours <= arrives)
                return 0f;
            double age = hours - arrives;
            double rise = age < SeaStateModel.SwellRiseHours ? age / SeaStateModel.SwellRiseHours : 1.0;
            double fall = age > SeaStateModel.SwellRiseHours ? Math.Exp(-(age - SeaStateModel.SwellRiseHours) / SeaStateModel.SwellDecayHours) : 1.0;
            return (float)(hs * rise * rise * (3.0 - 2.0 * rise) * fall);
        }
    }

    /// <summary>The sea at a place and moment: the wind sea, the swell, and what a sailor makes of them together.</summary>
    public struct SeaState
    {
        /// <summary>The wind sea: significant height (m), peak period (s), the bearing it comes from, its spread (degrees).</summary>
        public float windSeaHs, windSeaTp, windSeaFrom, spread;
        /// <summary>The largest swell train now: height (m), period (s), the bearing it comes from (zero when none).</summary>
        public float swellHs, swellTp, swellFrom;
        /// <summary>Up to three swell trains, the largest first (by height now).</summary>
        public SwellTrain swell0, swell1, swell2;
        /// <summary>The whole sea: Hs = sqrt(wind sea^2 + swells^2) (m), and the period and bearing of its larger part.</summary>
        public float hs, tp, from;
        /// <summary>The wind sea's steepness, Hs over the deep-water wavelength (0.03-0.05 ordinary, 0.06-0.08 steep).</summary>
        public float steepness;
        /// <summary>The share of the sea surface white with breaking crests (Monahan: 1% at 10 m/s, 10% at 20).</summary>
        public float whitecaps;
        /// <summary>A confused sea, 0..1: the largest swell running across the wind sea.</summary>
        public float confused;
        /// <summary>The expected highest wave met in half an hour, m (Rayleigh: about 1.65-1.7 Hs).</summary>
        public float highestIn30Min;
        /// <summary>The storm surge at the downwind shore of the level's fetch, m: the inverse barometer and the wind's
        /// set-up (provisional: the fetch has one depth until the levels carry a depth profile).</summary>
        public float surge;
        /// <summary>The wind the sea was stepped with, m/s.</summary>
        public float wind;
        /// <summary>The moment, game hours.</summary>
        public double hours;
        /// <summary>Whether the model has worked it out.</summary>
        public bool set;

        /// <summary>One wave in ten exceeds this (m): 1.27 Hs.</summary>
        public float HighestTenth => 1.27f * hs;
    }

    /// <summary>
    /// A level's fetch (weather document 2.4): the open water upwind in each of eight sectors (N, NE ... NW, km) and the
    /// water's depth, read from the level's water record (Assets/Kapman/Data/Water/water_793.json: sea.fetchKm and
    /// sea.depthM.typical; a river's width and channel depth). Read-only once made.
    /// </summary>
    public sealed class FetchTable
    {
        public static readonly string[] Sectors = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        readonly double[] km = new double[8];

        /// <summary>The water's typical depth, m (0 or less: deep).</summary>
        public double DepthMetres { get; }
        /// <summary>Where the numbers came from (a record id, or "open").</summary>
        public string Source { get; }

        public FetchTable(double[] kmBySector, double depthMetres, string source)
        {
            if (kmBySector == null || kmBySector.Length != 8)
                throw new ArgumentException("a fetch table needs eight sectors, N to NW");
            for (int i = 0; i < 8; i++)
                km[i] = Math.Max(0.0, kmBySector[i]);
            DepthMetres = depthMetres;
            Source = source ?? "";
        }

        /// <summary>The same fetch every way (an open sea or a test).</summary>
        public static FetchTable Open(double kmEveryWay, double depthMetres) =>
            new FetchTable(new[] { kmEveryWay, kmEveryWay, kmEveryWay, kmEveryWay, kmEveryWay, kmEveryWay, kmEveryWay, kmEveryWay }, depthMetres, "open");

        /// <summary>The fetch (km) in sector i (0 N, 1 NE ... 7 NW).</summary>
        public double Km(int sector) => km[((sector % 8) + 8) % 8];

        /// <summary>The fetch (m) for a wind from a compass bearing, between the two nearest sectors.</summary>
        public double FetchMetres(double fromDegrees)
        {
            double s = ((fromDegrees % 360.0) + 360.0) % 360.0 / 45.0;
            int i0 = (int)Math.Floor(s);
            double t = s - i0;
            return 1000.0 * (Km(i0) + (Km(i0 + 1) - Km(i0)) * t);
        }

        /// <summary>A record's fetch (an object of water_793.json's "records"): its sea's fetch by sector and typical
        /// depth, or, for a river with no sea, the river's width every way and its channel depth. Null if it has neither.</summary>
        public static FetchTable FromRecord(object record)
        {
            string id = Json.Str(record, "id");
            var sea = Json.Obj(record, "sea");
            var fetch = sea != null ? Json.Obj(sea, "fetchKm") : null;
            if (fetch != null)
            {
                var k = new double[8];
                for (int i = 0; i < 8; i++)
                    k[i] = Json.Num(fetch, Sectors[i], 0.0);
                return new FetchTable(k, Json.Num(Json.Obj(sea, "depthM"), "typical", 0.0), id);
            }
            var river = Json.Obj(record, "river");
            if (river != null)
            {
                double width = Json.Num(river, "widthM", 0.0) / 1000.0;
                if (width > 0.0)
                    return new FetchTable(new[] { width, width, width, width, width, width, width, width }, Json.Num(river, "channelDepthM", 3.0), id);
            }
            return null;
        }

        /// <summary>The fetch of the record with an id in water_793.json's text, or null (no such record, or no fetch).</summary>
        public static FetchTable FromWaterRecord(string waterJson, string recordId)
        {
            if (string.IsNullOrEmpty(waterJson) || string.IsNullOrEmpty(recordId))
                return null;
            var records = Json.Arr(Json.Parse(waterJson), "records");
            if (records == null)
                return null;
            foreach (var r in records)
                if (Json.Str(r, "id") == recordId)
                    return FromRecord(r);
            return null;
        }
    }

    /// <summary>
    /// THE SEA STATE - the sea the weather raises at a level (weather document 2.4), at full real height (the user's
    /// decision 4). Stepped with the clock (game hours), it carries:
    ///   - the wind sea, grown by WaveGrowth's laws (the SPM fetch law in the adjusted wind, the CEM duration law, the
    ///     fully developed and depth caps) with the equivalent-duration method: each step asks how long this wind would
    ///     have had to blow to raise the sea already there, and grows it on from that, so a changing wind is handled as
    ///     the coastal manuals do; when the wind can no longer hold the sea it decays with an e-fold of six hours; its
    ///     direction follows the wind's with a lag of an hour and a half;
    ///   - the swell: every six hours each low within 3,000 km whose winds reach 15 m/s sends a train towards the place,
    ///     Hs = 0.3 Hs_storm (1 - d / 3,000 km), at its period, arriving after d / c_g (12 s: 34 km/h), cut down where
    ///     land lies between (the level's fetch towards the storm against half the distance); up to three trains are
    ///     kept, each rising over six hours and dying with an e-fold of a day;
    ///   - the whole sea, its steepness, whitecaps, confusion, the highest wave of half an hour and the surge.
    /// <see cref="WarmStart"/> rebuilds it at any moment from the engine's last 48 hours of wind and 72 of storms (a level
    /// loaded mid-gale starts with its sea already up, and the swell of a storm still a day off is already running). No
    /// allocation in <see cref="Step"/> except every six game hours, when the storms are asked for.
    /// </summary>
    public sealed class SeaStateModel
    {
        public const double ReplayHours = 48.0, SwellReplayHours = 72.0, SwellEveryHours = 6.0;
        public const double SeaDecayHours = 6.0, SwellRiseHours = 6.0, SwellDecayHours = 24.0, DirectionLagHours = 1.5;
        /// <summary>A storm's winds below this send no swell worth the name, m/s (weather 2.4).</summary>
        public const double SwellStormWind = 15.0;
        /// <summary>The farthest storm whose swell is felt, km.</summary>
        public const double SwellReachKm = 3000.0;
        /// <summary>The fetch a storm's own winds blow over, m, and for how long (a guess: half a low's radius, a day; it gives the
        /// North Sea's 10-14 s northerly swell).</summary>
        public const double StormFetchMetres = 300000.0, StormHours = 24.0;

        readonly WeatherEngine engine;
        readonly double lat, lon;
        readonly string placeClass;
        readonly SwellTrain[] trains = new SwellTrain[3];
        double hsSea, tpSea, fromX, fromY;
        long lastEpoch = long.MinValue;
        SeaState state;

        /// <summary>The level's fetch and depth.</summary>
        public FetchTable Fetch { get; set; }
        /// <summary>Whether far storms send swell (off for weather set by hand, whose storms are not the engine's).</summary>
        public bool Swell { get; set; } = true;
        /// <summary>The sea now.</summary>
        public SeaState State => state;
        /// <summary>The moment the model was last stepped to, game hours (NaN before the first step).</summary>
        public double Hours { get; private set; } = double.NaN;

        /// <summary>A sea at a place (lat, lon, the engine's place class) with its fetch. The engine may be null: then only
        /// the wind given to <see cref="Step"/> raises the sea, and there is no swell.</summary>
        public SeaStateModel(WeatherEngine engine, double lat, double lon, string placeClass, FetchTable fetch)
        {
            this.engine = engine;
            this.lat = lat;
            this.lon = lon;
            this.placeClass = placeClass;
            Fetch = fetch ?? FetchTable.Open(300.0, 30.0);
        }

        /// <summary>A flat sea and no swell.</summary>
        public void Reset()
        {
            hsSea = tpSea = 0.0;
            fromX = fromY = 0.0;
            for (int i = 0; i < trains.Length; i++)
                trains[i] = default;
            lastEpoch = long.MinValue;
            state = default;
            Hours = double.NaN;
        }

        /// <summary>Steps the sea on by some game hours to a moment, under a sample of the weather.</summary>
        public SeaState Step(double hours, double dtHours, in WeatherSample weather) =>
            Step(hours, dtHours, weather.windSpeed, weather.windFrom, weather.pressure);

        /// <summary>Steps the sea on by some game hours to a moment, under a wind (m/s, the bearing it blows from) and a
        /// sea-level pressure (hPa).</summary>
        public SeaState Step(double hours, double dtHours, double windSpeed, double windFrom, double pressure)
        {
            GrowWindSea(Math.Max(0.0, dtHours), windSpeed, windFrom);
            if (Swell && engine != null)
            {
                long epoch = (long)Math.Floor(hours / SwellEveryHours);
                if (lastEpoch == long.MinValue || epoch < lastEpoch)
                    lastEpoch = epoch;                                            // the first step: the warm start sends the old storms
                for (long e = lastEpoch + 1; e <= epoch && e - lastEpoch <= 4; e++)
                    Emit(e * SwellEveryHours, hours);
                lastEpoch = epoch;
            }
            Compose(hours, windSpeed, pressure);
            Hours = hours;
            return state;
        }

        /// <summary>
        /// Rebuilds the sea at a moment from the engine (weather 2.4): the swell trains of the storms alive over the last 72
        /// hours, and the wind sea replayed hour by hour over the last 48. For a level's entry, a clock jump and a load.
        /// </summary>
        public SeaState WarmStart(double hours)
        {
            if (engine == null)
                throw new InvalidOperationException("a sea with no weather engine cannot warm start from it; use WarmStart(hours, wind, from, pressure)");
            Reset();
            if (Swell)
            {
                long last = (long)Math.Floor(hours / SwellEveryHours);
                long first = (long)Math.Floor((hours - SwellReplayHours) / SwellEveryHours) + 1;
                for (long e = first; e <= last; e++)
                    Emit(e * SwellEveryHours, hours);
                lastEpoch = last;
            }
            WeatherSample s = default;
            for (int i = 0; i <= (int)ReplayHours; i++)
            {
                double h = hours - ReplayHours + i;
                s = engine.Sample(lat, lon, h, placeClass);
                GrowWindSea(i == 0 ? 0.0 : 1.0, s.windSpeed, s.windFrom);
            }
            Compose(hours, s.windSpeed, s.pressure);
            Hours = hours;
            return state;
        }

        /// <summary>A sea that has had a steady wind for the replay's 48 hours (weather set by hand: a shot, the G key).</summary>
        public SeaState WarmStart(double hours, double windSpeed, double windFrom, double pressure)
        {
            Reset();
            lastEpoch = (long)Math.Floor(hours / SwellEveryHours);
            for (int i = 0; i <= (int)ReplayHours; i++)
                GrowWindSea(i == 0 ? 0.0 : 1.0, windSpeed, windFrom);
            Compose(hours, windSpeed, pressure);
            Hours = hours;
            return state;
        }

        // ------------------------------------------------------------------------------------------- the wind sea

        void GrowWindSea(double dt, double u, double windFrom)
        {
            u = Math.Max(0.0, u);
            double fetch = Fetch.FetchMetres(windFrom), depth = Fetch.DepthMetres;
            WaveGrowth.Grow(u, fetch, 1e6, depth, out double hsEq, out double tpEq);   // the most this wind raises here
            if (hsSea < hsEq - 1e-6)
            {
                double already = EquivalentHours(hsSea, u, fetch);
                WaveGrowth.Grow(u, fetch, already + dt, depth, out double hs, out double tp);
                if (hs > hsSea)
                {
                    hsSea = hs;
                    tpSea = Math.Max(tp, Math.Min(tpSea, tpEq));
                }
            }
            else if (dt > 0.0)
            {
                hsSea = hsEq + (hsSea - hsEq) * Math.Exp(-dt / SeaDecayHours);
                tpSea = tpEq + (tpSea - tpEq) * Math.Exp(-dt / (2.0 * SeaDecayHours));   // an old sea keeps its length longer
            }

            // the sea's heading follows the wind's an hour or two behind
            double a = windFrom * Math.PI / 180.0, wx = Math.Sin(a), wy = Math.Cos(a);
            if (fromX * fromX + fromY * fromY < 1e-6 || hsSea < 0.02)
            {
                fromX = wx;
                fromY = wy;
            }
            else if (u > 0.5 && dt > 0.0)
            {
                double k = 1.0 - Math.Exp(-dt / DirectionLagHours);
                fromX += (wx - fromX) * k;
                fromY += (wy - fromY) * k;
                double n = Math.Sqrt(fromX * fromX + fromY * fromY);
                if (n > 1e-6)
                {
                    fromX /= n;
                    fromY /= n;
                }
            }
        }

        /// <summary>How long (hours) a wind must have blown over a fetch to raise a sea of height Hs (the inverse of the
        /// duration-limited growth; the time for the whole fetch if Hs is the fetch's own sea or more).</summary>
        public static double EquivalentHours(double hs, double u10, double fetchMetres)
        {
            if (hs <= 1e-4 || u10 <= 0.3)
                return 0.0;
            double ua = WaveGrowth.AdjustedWind(u10);
            double x = WaveGrowth.G * Math.Pow(hs / (0.0016 * ua), 2.0);
            return WaveGrowth.DurationForFetchHours(Math.Min(x, fetchMetres), u10);
        }

        // ------------------------------------------------------------------------------------------- the swell

        /// <summary>The lows alive at a moment send their swell towards the place (weather 2.4).</summary>
        void Emit(double at, double now)
        {
            var snapshot = engine.Model.Snapshot(at);
            var systems = engine.Model.Systems(at);
            foreach (var sys in systems)
            {
                if (!sys.low || sys.depth > -3.0)
                    continue;
                double d = Km(lat, lon, sys.lat, sys.lon, out double east, out double north);
                if (d > SwellReachKm || d < 0.5 * sys.radiusKm)
                    continue;                                                     // too far, or the place is inside it (its wind sea)
                // the storm's strongest wind: the model's surface wind at four points about it
                double u = 0.0, r = 0.7 * sys.radiusKm;
                for (int k = 0; k < 4; k++)
                {
                    double b = k * Math.PI * 0.5;
                    double plat = sys.lat + r * Math.Cos(b) / 111.2;
                    double plon = sys.lon + r * Math.Sin(b) / (111.2 * Math.Max(Math.Cos(plat * Math.PI / 180.0), 0.2));
                    u = Math.Max(u, engine.Model.Sample(snapshot, plat, plon, at, true).windSpeed);
                }
                if (u < SwellStormWind)
                    continue;
                WaveGrowth.Grow(u, StormFetchMetres, StormHours, 0.0, out double hsStorm, out double tpStorm);
                double bearing = (Math.Atan2(east, north) * 180.0 / Math.PI + 360.0) % 360.0;     // from the place towards the storm
                double open = Math.Min(1.0, Fetch.FetchMetres(bearing) / 1000.0 / (0.5 * d));     // land in the way cuts it down
                double hs = 0.3 * hsStorm * (1.0 - d / SwellReachKm) * open;
                if (hs < 0.2)
                    continue;
                Add(new SwellTrain
                {
                    hs = (float)hs,
                    tp = (float)tpStorm,
                    from = (float)bearing,
                    arrives = at + WaveGrowth.SwellTravelHours(d, tpStorm),
                    km = (float)d,
                }, now);
            }
        }

        /// <summary>Keeps a new train: it replaces one from the same storm (within 30 degrees, arriving within 12 hours) if
        /// larger, or else the weakest of three.</summary>
        void Add(SwellTrain t, double now)
        {
            for (int i = 0; i < trains.Length; i++)
                if (trains[i].Live && Angle(trains[i].from, t.from) < 30.0 && Math.Abs(trains[i].arrives - t.arrives) < 12.0)
                {
                    if (Strength(t, now) > Strength(trains[i], now))
                        trains[i] = t;
                    return;
                }
            int weakest = 0;
            for (int i = 0; i < trains.Length; i++)
            {
                if (!trains[i].Live)
                {
                    weakest = i;
                    break;
                }
                if (Strength(trains[i], now) < Strength(trains[weakest], now))
                    weakest = i;
            }
            if (!trains[weakest].Live || Strength(t, now) > Strength(trains[weakest], now))
                trains[weakest] = t;
        }

        /// <summary>A train's height now, or at its full arrival if that is still to come.</summary>
        static double Strength(in SwellTrain t, double now) => t.HeightAt(Math.Max(now, t.arrives + SwellRiseHours));

        // ------------------------------------------------------------------------------------------- the whole sea

        void Compose(double hours, double u, double pressure)
        {
            // the trains in order of their height now; the dead ones go
            double swell2 = 0.0;
            for (int i = 0; i < trains.Length; i++)
                if (trains[i].Live && hours > trains[i].arrives + SwellRiseHours && trains[i].HeightAt(hours) < 0.05f)
                    trains[i] = default;
            SortTrains(hours);
            for (int i = 0; i < trains.Length; i++)
            {
                double h = trains[i].HeightAt(hours);
                swell2 += h * h;
            }
            var s = new SeaState
            {
                windSeaHs = (float)hsSea,
                windSeaTp = (float)tpSea,
                windSeaFrom = (float)((Math.Atan2(fromX, fromY) * 180.0 / Math.PI + 360.0) % 360.0),
                swell0 = trains[0],
                swell1 = trains[1],
                swell2 = trains[2],
                wind = (float)u,
                hours = hours,
                set = true,
            };
            float big = trains[0].HeightAt(hours);
            if (big > 0f)
            {
                s.swellHs = big;
                s.swellTp = trains[0].tp;
                s.swellFrom = trains[0].from;
            }
            s.hs = (float)Math.Sqrt(hsSea * hsSea + swell2);
            bool swellLeads = s.swellHs > hsSea;
            s.tp = swellLeads ? s.swellTp : (float)tpSea;
            s.from = swellLeads ? s.swellFrom : s.windSeaFrom;
            double fetch = Fetch.FetchMetres(s.windSeaFrom);
            s.spread = (float)(35.0 - 10.0 * Math.Min(1.0, fetch / 300000.0));      // JONSWAP 25-35 degrees, narrower down a long fetch
            double l0 = WaveGrowth.G * tpSea * tpSea / (2.0 * Math.PI);
            s.steepness = l0 > 0.01 ? (float)(hsSea / l0) : 0f;
            s.whitecaps = (float)WaveGrowth.Whitecaps(u);
            s.confused = big > 0f ? (float)(Math.Min(1.0, Angle(s.swellFrom, s.windSeaFrom) / 90.0) * Math.Min(1.0, big / Math.Max(hsSea, 0.3))) : 0f;
            s.highestIn30Min = (float)(s.hs * WaveGrowth.HighestOverHs(1800.0, Math.Max(s.tp, 1.0)));
            // the surge (WaveGrowth.Surge, summed here without a list): the fetch's depth profile until the levels carry their
            // own is four fifths at the typical depth and the last fifth, the shore's approach, at half of it (a guess)
            double depth = Fetch.DepthMetres > 0.0 ? Fetch.DepthMetres : 40.0;
            double along = fetch * 0.8 / depth + fetch * 0.2 / (depth * 0.5);
            s.surge = (float)(WaveGrowth.InverseBarometer(pressure) + WaveGrowth.SetupConstant(u) * u * u * along);
            state = s;
        }

        void SortTrains(double hours)
        {
            for (int i = 0; i < trains.Length; i++)
                for (int j = i + 1; j < trains.Length; j++)
                    if (trains[j].HeightAt(hours) > trains[i].HeightAt(hours) || (!trains[i].Live && trains[j].Live))
                    {
                        var x = trains[i];
                        trains[i] = trains[j];
                        trains[j] = x;
                    }
        }

        static double Angle(double a, double b)
        {
            double d = Math.Abs(((a - b) % 360.0 + 360.0) % 360.0);
            return d > 180.0 ? 360.0 - d : d;
        }

        static double Km(double lat, double lon, double lat0, double lon0, out double east, out double north)
        {
            double cosLat = Math.Cos((lat + lat0) * 0.5 * Math.PI / 180.0);
            east = (lon0 - lon) * 111.2 * cosLat;
            north = (lat0 - lat) * 111.2;
            return Math.Sqrt(east * east + north * north);
        }
    }
}
