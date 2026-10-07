using System;
using System.Collections.Generic;
using Kapman.Game;

namespace Kapman.Weather
{
    /// <summary>
    /// THE SYNOPTIC FIELD - the weather engine's second layer (weather document 2.2), built on the weather model's own
    /// systems and its climate pressure: each day's circulation type (CirculationChain) places the systems of its template
    /// on the map (the westerly day's low passing north of Scotland, the easterly day's high over Scandinavia ...), each a
    /// little astray from the template, deeper in winter, and moving along its track. A day's systems live two days on a
    /// Hann window centred on the day's noon, so a run of days of one type adds up to one steady pattern and a change of
    /// type is a cross-fade over a day, never a switch. The systems go into the weather model's own list
    /// (WeatherModel.Alive), so the model's pressure, geostrophic wind, cloud and fronts all follow them unchanged (fronts
    /// as the model draws them: the night-1 slice).
    /// </summary>
    public sealed class SynopticField
    {
        /// <summary>A system a type places: where it stands at the middle of its day.</summary>
        public struct Template
        {
            public bool low;
            public double lat, lon;
            /// <summary>hPa against the season's mean field: + a high, - a low.</summary>
            public double depth;
            public double radiusKm, bearing, speedKmh, ridge;
        }

        readonly int seed;
        readonly Template[][] templates = new Template[CirculationChain.Count][];
        readonly double[][] typeTemperature = new double[CirculationChain.Count][];

        public CirculationChain Chain { get; }
        /// <summary>How far (km) and how much (a share of its depth) a day's system may stray from its template (one sigma).</summary>
        public double StrayKm { get; } = 300.0;
        public double StrayDepth { get; } = 0.25;
        /// <summary>How long a day's systems live, game hours (two days: neighbours overlap by half, summing to one).</summary>
        public double KernelHours { get; } = 48.0;
        /// <summary>How much deeper the systems are in midwinter than at the equinoxes (a share).</summary>
        public double WinterDeepening { get; } = 0.25;

        public SynopticField(int seed, CirculationChain chain, object circulationRoot)
        {
            this.seed = seed;
            Chain = chain ?? throw new ArgumentNullException(nameof(chain));
            StrayKm = Json.Num(circulationRoot, "strayKm", StrayKm);
            StrayDepth = Json.Num(circulationRoot, "strayDepth", StrayDepth);
            KernelHours = Math.Max(24.0, Json.Num(circulationRoot, "dayKernelHours", KernelHours));
            WinterDeepening = Json.Num(circulationRoot, "winterDeepening", WinterDeepening);
            var t = Json.Obj(circulationRoot, "templates");
            var temps = Json.Obj(circulationRoot, "typeTemperature");
            for (int i = 0; i < CirculationChain.Count; i++)
            {
                string name = ((CirculationType)i).ToString();
                var list = new List<Template>();
                if (t != null && t.TryGetValue(name, out var o) && o is List<object> items)
                    foreach (var it in items)
                    {
                        string kind = Json.Str(it, "kind");
                        if (kind != "low" && kind != "high")
                            throw new FormatException($"circulation.json: a template of type {name} is neither a low nor a high ('{kind}')");
                        list.Add(new Template
                        {
                            low = kind == "low",
                            lat = Json.Num(it, "lat"),
                            lon = Json.Num(it, "lon"),
                            depth = Json.Num(it, "depth"),
                            radiusKm = Math.Max(200.0, Json.Num(it, "radius", 800.0)),
                            bearing = Json.Num(it, "bearing", 90.0),
                            speedKmh = Math.Max(0.0, Json.Num(it, "speed", 20.0)),
                            ridge = Json.Num(it, "ridge", 0.0),
                        });
                    }
                else
                    throw new FormatException($"circulation.json: no template for type {name}");
                templates[i] = list.ToArray();
                var tt = temps != null ? Json.Arr(temps, name) : null;
                typeTemperature[i] = new[] { tt != null && tt.Count > 0 && tt[0] is double w ? w : 0.0, tt != null && tt.Count > 1 && tt[1] is double s ? s : 0.0 };
            }
        }

        /// <summary>The templates of a type.</summary>
        public IReadOnlyList<Template> TemplatesOf(CirculationType type) => templates[(int)type];

        /// <summary>The type at a moment (game hours since the start of year 0).</summary>
        public CirculationType TypeAt(double hours) => Chain.TypeAt(hours);

        /// <summary>
        /// The air the days' types bring at a moment, C against the season's normal: each type's midwinter and midsummer
        /// values (circulation.json "typeTemperature") blended by the season, and the neighbouring days blended by the same
        /// windows the systems live on, so the air changes over a day with the type.
        /// </summary>
        public double AirMassAt(double hours)
        {
            double summer = ClimateNormals.Summerness(WeatherModel.DayOfYear(hours));
            double sum = 0.0, weights = 0.0;
            long d0 = (long)Math.Floor(hours / 24.0);
            for (long d = d0 - 2; d <= d0 + 2; d++)
            {
                double w = Window(hours, d);
                if (w <= 0.0)
                    continue;
                var tt = typeTemperature[(int)Chain.TypeOn(d)];
                sum += w * (tt[0] + (tt[1] - tt[0]) * summer);
                weights += w;
            }
            return weights > 0.0 ? sum / weights : 0.0;
        }

        /// <summary>How much of day <paramref name="day"/>'s systems is alive at a moment: a Hann window over
        /// <see cref="KernelHours"/> centred on the day's noon.</summary>
        public double Window(double hours, long day)
        {
            double age = hours - Born(day);
            return age <= 0.0 || age >= KernelHours ? 0.0 : 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * age / KernelHours);
        }

        double Born(long day) => (day + 0.5) * 24.0 - KernelHours * 0.5;

        /// <summary>Puts the systems of the days alive at a moment into the weather model's list, after its own; returns the
        /// new count (the list's length is the limit).</summary>
        internal int AddSystems(double hours, WeatherModel.System[] into, int n)
        {
            long d0 = (long)Math.Floor(hours / 24.0);
            int reach = (int)Math.Ceiling(KernelHours / 48.0) + 1;
            for (long d = d0 - reach; d <= d0 + reach && n < into.Length; d++)
            {
                double born = Born(d);
                if (hours <= born || hours >= born + KernelHours)
                    continue;
                var type = Chain.TypeOn(d);
                var list = templates[(int)type];
                double winter = Math.Max(0.0, WeatherModel.Winter(born + KernelHours * 0.5));
                for (int i = 0; i < list.Length && n < into.Length; i++)
                    into[n++] = Place(list[i], d, i, born, winter);
            }
            return n;
        }

        /// <summary>A day's system from its template: strayed, deepened for the season, and started half a life back along
        /// its track so it stands near its template at the day's noon.</summary>
        WeatherModel.System Place(Template t, long day, int index, double born, double winter)
        {
            var r = new Random(unchecked((int)Hash(seed, day, index)));
            double north = Normal(r) * StrayKm, east = Normal(r) * StrayKm;
            double lat = t.lat + north / 111.2;
            double lon = t.lon + east / (111.2 * Math.Max(Math.Cos(lat * Math.PI / 180.0), 0.2));
            double depth = t.depth * (1.0 + WinterDeepening * winter) * Math.Max(0.4, 1.0 + StrayDepth * Normal(r));
            double bearing = t.bearing + Normal(r) * 12.0;
            double speed = Math.Max(0.0, t.speedKmh * (1.0 + 0.2 * Normal(r)));
            // half a life back along the track (flat-earth: a day's travel is short against the pattern)
            double km = speed * KernelHours * 0.5, b = bearing * Math.PI / 180.0;
            double lat0 = lat - km * Math.Cos(b) / 111.2;
            double lon0 = lon - km * Math.Sin(b) / (111.2 * Math.Max(Math.Cos((lat + lat0) * 0.5 * Math.PI / 180.0), 0.2));
            return new WeatherModel.System
            {
                born = born,
                life = KernelHours,
                lat0 = lat0,
                lon0 = lon0,
                bearing = bearing,
                turn = 0.0,
                speed = speed,
                depth = t.low ? Math.Min(depth, -2.0) : Math.Max(depth, 2.0),
                radius = Math.Max(200.0, t.radiusKm * (1.0 + 0.1 * Normal(r))),
                deepen = 0.0,
                ridge = t.low ? t.ridge : 0.0,
                low = t.low,
                day = true,
            };
        }

        static double Normal(Random r)
        {
            double u = 1.0 - r.NextDouble(), v = r.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
        }

        static ulong Hash(int a, long b, int c)
        {
            unchecked
            {
                ulong h = 14695981039346656037UL ^ 0x5157UL;
                h = (h ^ (ulong)a) * 1099511628211UL;
                h = (h ^ (ulong)b) * 1099511628211UL;
                h = (h ^ (ulong)c) * 1099511628211UL;
                h ^= h >> 29;
                h *= 0xBF58476D1CE4E5B9UL;
                h ^= h >> 32;
                return h;
            }
        }
    }
}
