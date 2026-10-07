using System;

namespace Kapman.Weather
{
    /// <summary>
    /// THE HOUR'S SKY - the weather engine's local layer for cloud, rain, fog and the view (weather document 2.2 and 2.3,
    /// night 2). From where a place stands against the fronts (WeatherModel.Fronts), the day's circulation type, the
    /// season and the hour it works out, with nothing switched and nothing set by hand:
    ///   - the frontal curves: ahead of a warm front cirrus from about 1,100 km, cirrostratus and altostratus thickening
    ///     and lowering from 450 km, nimbostratus with steady rain of 1-3 mm/h over the last 200 km; the warm sector's
    ///     stratus at 150-400 m with drizzle and a thick murk (3-8 km); the cold front's narrow line of cumulonimbus with
    ///     5-15 mm/h for an hour or two; behind it clear cold air (20-40 km) with heaped cloud and showers;
    ///   - the rest of the sky: the grey stratocumulus of a winter high or a cold spell, fair-weather cumulus on summer
    ///     afternoons over land, a summer heat storm now and then, the background of broken cloud;
    ///   - the three layers (low, middle, high), the lowest significant layer's kind, base and depth (cumulus base
    ///     125 (T - Td) m), and the column's light (CloudLight's table);
    ///   - the dew point from the air mass's humidity (sea air 85-95%, the warm sector 95%, continental east winds
    ///     50-70%), the radiation fog of still clear nights that thickens to dawn and burns off as the air warms, the sea
    ///     fog of mild moist air over a colder sea, drifting in banks, and the visibility of all of it summed (Visibility);
    ///   - thunder, and the sea breeze that turns the coast's afternoon wind onto the land.
    /// Numbers are the design document's; those marked (guess) are mine. Pure arithmetic and hashed noise (deterministic
    /// from the seed, the place and the hour), no state, no allocation.
    /// </summary>
    internal static class LocalSky
    {
        /// <summary>Everything the hour's sky is worked out from.</summary>
        internal struct Context
        {
            public int seed;
            public double lat, lon, hours, dayOfYear, hour;
            /// <summary>0 mid-January .. 1 mid-July.</summary>
            public double summer;
            public CirculationType type;
            public FrontState front;
            public bool overSea;
            /// <summary>The place class (null: none near).</summary>
            public ClimateNormals.PlaceClass place;
            /// <summary>The sea's surface temperature, C.</summary>
            public double seaTemp;
            /// <summary>The year's offset and the day's air mass, C (for the land's warmth against the sea).</summary>
            public double airOffset;
            /// <summary>The slow cold or mild spell, C (WeatherEngine.Spell): a winter cold spell is dry continental air,
            /// clearer and with less to fall.</summary>
            public double spell;
        }

        /// <summary>The sky's parts, between the cloud pass and the air pass.</summary>
        internal struct Parts
        {
            public double low, mid, high, cb, cumulus, stratiform;
            public CloudType lowType, midType, highType;
            public double lowThick, midThick, highThick;
            public double midBase, highBase, nsBase, nsDepth;
            /// <summary>The cold front's band (0..1), the shower cells behind it, the heat storm's cell, the warm sector.</summary>
            public double band, showers, behind, heat, warm;
            /// <summary>Rain before the heat storms (they need the temperature), mm/h.</summary>
            public double rain;
            /// <summary>How clear the air is, km, before humidity, fog and rain.</summary>
            public double airKm;
            /// <summary>The share of the sky's lowest two layers, for the day's temperature cycle.</summary>
            public double Cover => 1.0 - (1.0 - low) * (1.0 - mid);
        }

        // ------------------------------------------------------------------------------------------- the cloud

        /// <summary>The cloud and rain of the hour (before the temperature, which the cloud damps).</summary>
        internal static Parts Clouds(in Context x)
        {
            var p = new Parts { lowType = CloudType.Clear, midType = CloudType.Clear, highType = CloudType.Clear, airKm = 45.0 };
            var f = x.front;
            double s = Clamp01(f.weight), v = Clamp01(f.vigour / 1.3);
            double t = x.hours;
            double winter = 1.0 - x.summer;
            double low = 0.0, mid = 0.0, high = 0.0, cb = 0.0, cumulus = 0.0, rain = 0.0;
            double nsLow = 0.0, stLow = 0.0, scLow = 0.0, steady = 0.0;
            // a winter cold spell is dry continental air: clearer, and little falls from it
            double dryCold = Clamp01(-x.spell / 4.0) * winter;
            p.midBase = 3500.0;
            p.highBase = 9000.0;
            p.nsBase = 600.0;
            p.nsDepth = 3500.0;

            // ahead of a warm front: cirrus, then the veil thickening and lowering, then the rain deck
            if (f.kind == FrontKind.Warm && f.phase < 0.0)
            {
                double d = -f.phase;
                // a front that holds at all closes the sky: its decks are whole
                double whole = Smooth(Clamp01(s * 2.5));
                double ci = whole * Smooth(Clamp01((1100.0 - d) / 500.0));
                double asCover = whole * Smooth(Clamp01((450.0 - d) / 300.0));
                double ns = whole * Smooth(Clamp01((220.0 - d) / 170.0));
                high = Math.Max(high, ci);
                mid = Math.Max(mid, asCover);
                nsLow = Math.Max(nsLow, ns);
                steady += s * (0.25 * Smooth(Clamp01((280.0 - d) / 160.0)) + (0.6 + 1.4 * v) * Smooth(Clamp01((200.0 - d) / 200.0)));
                p.highType = d < 600.0 ? CloudType.Cirrostratus : CloudType.Cirrus;
                p.highThick = Clamp01((900.0 - d) / 600.0);
                p.midBase = 5000.0 - 2500.0 * Smooth(Clamp01((450.0 - d) / 350.0));
                p.midThick = Clamp01((400.0 - d) / 300.0);
                p.nsBase = 800.0 - 400.0 * Smooth(Clamp01((150.0 - d) / 150.0));
                p.nsDepth = 3000.0 + 2000.0 * v;
                p.airKm = Math.Min(p.airKm, 15.0 + 10.0 * Clamp01(d / 400.0));
            }

            // the warm sector: low grey stratus, drizzle, murk
            double ws = Smooth(Clamp01((f.warmSector - 0.25) / 0.6));                     // its heart; the edges are mixed air
            p.warm = ws;
            if (ws > 0.0)
            {
                double patch = Noise(x.seed, 21, x.lat * 3.0, x.lon * 3.0, t / 4.0);
                stLow = Math.Max(stLow, ws * (0.82 + 0.25 * patch));
                mid = Math.Max(mid, ws * 0.35);
                steady += ws * Clamp01(patch * 3.0 - 1.65) * (0.1 + 0.4 * v);
                p.airKm = Math.Min(p.airKm, 25.0 - 19.0 * ws);                             // 3-8 km deep in it
            }

            // the cold front's line, and the showery cold air behind it
            if (f.kind == FrontKind.Cold)
            {
                double band = Math.Exp(-Math.Pow((f.phase - 10.0) / 16.0, 2.0));
                p.band = s * band;
                cb = Math.Max(cb, s * band);
                nsLow = Math.Max(nsLow, s * band * 0.9);
                rain += s * band * (2.5 + 8.0 * v);
                if (f.phase > 30.0)
                {
                    double behind = s * Smooth(Clamp01((700.0 - f.phase) / 600.0)) * Smooth(Clamp01((f.phase - 30.0) / 40.0));
                    p.behind = behind;
                    double cell = Clamp01(Noise(x.seed, 23, x.lat * 12.0, x.lon * 12.0, t / 1.2) * 2.4 - 1.35);
                    p.showers = behind * cell;
                    cumulus = Math.Max(cumulus, behind * (0.4 + 0.3 * Noise(x.seed, 24, x.lat * 5.0, x.lon * 5.0, t / 3.0)));
                    cb = Math.Max(cb, behind * cell * 0.85);
                    rain += behind * cell * (2.0 + 6.0 * v);
                    p.airKm = Math.Max(p.airKm, 25.0 + 15.0 * behind);                    // the clean cold air: 20-40 km
                }
            }

            // about a low's centre: thick layered cloud and its rain
            if (f.centre > 0.0)
            {
                nsLow = Math.Max(nsLow, Smooth(Clamp01(f.centre * 1.3)));
                mid = Math.Max(mid, f.centre);
                steady += f.centreRain * 0.7;
            }

            // the rest of the sky: grey decks that come and go over a day or two (a North Sea January is half dull days,
            // a July a quarter), broken cloud between them, thinned by the highs; the inversion stratocumulus of a winter
            // high; fair cumulus on summer afternoons over land
            double clearing = (1.0 - 0.85 * Clamp01(f.highs)) * (1.0 - 0.8 * dryCold) * (1.0 - 0.7 * p.behind);
            double northWest = Clamp01((x.lat - 45.0) / 15.0) * Clamp01((25.0 - x.lon) / 30.0);
            double bgNoise = Noise(x.seed, 25, x.lat * 1.5, x.lon * 1.5, t / 30.0);
            double deckBias = DeckBias + DeckWinter * winter + 0.08 * northWest;
            double deck = Smooth(Clamp01((bgNoise - 0.5 + deckBias) / 0.16)) * clearing;
            double broken = Clamp01(0.8 * Noise(x.seed, 30, x.lat * 3.0, x.lon * 3.0, t / 10.0) - 0.25) * clearing;
            scLow = Math.Max(scLow, Math.Max(0.97 * deck, broken));
            mid = Math.Max(mid, deck * (0.4 + 0.5 * x.summer));
            if (x.type == CirculationType.A)
            {
                // the gloom of a winter high: a lid of stratocumulus under the inversion on some days, clear on others
                double gloom = Clamp01(Noise(x.seed, 26, x.lat * 0.8, x.lon * 0.8, t / 30.0) * 3.0 - 1.65) * winter * (1.0 - dryCold);
                scLow = Math.Max(scLow, 0.95 * gloom);
            }
            else if (x.type == CirculationType.C || x.type == CirculationType.N || x.type == CirculationType.NW)
            {
                // cold, unstable air: heaped cloud and showers all day (over the sea and the coast most)
                double unstable = x.type == CirculationType.C ? 0.8 : 0.55;
                double cell = Clamp01(Noise(x.seed, 27, x.lat * 12.0, x.lon * 12.0, t / 1.2) * 2.6 - 1.65);
                cumulus = Math.Max(cumulus, unstable * (0.45 + 0.2 * winter));
                cb = Math.Max(cb, unstable * cell * 0.75);
                rain += unstable * cell * 2.5;
                p.showers = Math.Max(p.showers, unstable * cell);
            }
            double day = Clamp01(Math.Sin(Math.PI * (x.hour - 9.0) / 10.0));
            double convection = Clamp01(x.summer * 1.3 - 0.3) * day * (x.overSea ? 0.25 : 1.0) * (1.0 - 0.5 * Clamp01(f.highs));
            double texture = Noise(x.seed, 28, x.lat * 4.0, x.lon * 4.0, t / 4.0);
            cumulus = Math.Max(cumulus, convection * Clamp01(texture * 1.4 - 0.35) * (x.type == CirculationType.E ? 0.4 : 1.0));   // dry east winds stay clear

            // steady rain falls from a whole deck; the patchy edges; a cold spell's dry air
            nsLow = Math.Max(nsLow, Smooth(Clamp01(steady / 0.6)));
            rain += steady;
            double breakup = 0.8 + 0.4 * Noise(x.seed, 29, x.lat * 6.0, x.lon * 6.0, t / 2.0);
            rain = Math.Max(0.0, rain * breakup) * Math.Max(0.0, 1.0 + DrySpell * Math.Min(0.0, x.spell) * winter);

            // the layers and their kinds: the low layer is the most of its kinds; the cumulonimbus is part of it
            nsLow = Clamp01(nsLow);
            stLow = Clamp01(stLow);
            scLow = Clamp01(scLow);
            cumulus = Clamp01(cumulus);
            cb = Clamp01(cb);
            low = Math.Max(Math.Max(nsLow, stLow), Math.Max(scLow, Math.Max(cumulus, cb)));
            if (cb >= 0.3 && cb >= 0.6 * low)
                p.lowType = CloudType.Cumulonimbus;
            else if (nsLow >= Math.Max(stLow, scLow) && nsLow >= cumulus && nsLow > 0.0 && rain > 0.4)
                p.lowType = CloudType.Nimbostratus;
            else if (stLow >= scLow && stLow >= cumulus && stLow > 0.0)
                p.lowType = CloudType.Stratus;
            else if (cumulus > scLow)
                p.lowType = CloudType.Cumulus;
            else if (low > 0.0)
                p.lowType = nsLow > scLow ? CloudType.Stratus : CloudType.Stratocumulus;
            p.lowThick = p.lowType == CloudType.Nimbostratus ? Clamp01(0.3 + 0.25 * rain)
                       : p.lowType == CloudType.Cumulonimbus ? Clamp01(cb)
                       : p.lowType == CloudType.Stratocumulus ? Clamp01(low * 1.2 - 0.3) : 0.5;
            if (mid > 0.0)
            {
                bool sheet = f.kind == FrontKind.Warm || f.centre > 0.3 || ws > 0.3;
                p.midType = sheet ? CloudType.Altostratus : CloudType.Altocumulus;
                if (!sheet)
                    p.midThick = 0.4;
            }
            if (high > 0.0 && p.highType == CloudType.Clear)
                p.highType = CloudType.Cirrus;

            p.low = low;
            p.mid = Clamp01(mid);
            p.high = Clamp01(high);
            p.cb = cb;
            p.cumulus = cumulus;
            p.stratiform = Math.Max(Math.Max(nsLow, stLow), scLow);
            p.rain = rain;

            // the air's own clarity by situation (before humidity): polar air clean, a summer high hazy, an east wind dusty
            switch (x.type)
            {
                case CirculationType.N:
                case CirculationType.NW:
                    p.airKm = Math.Max(p.airKm, 35.0);
                    break;
                case CirculationType.A:
                    p.airKm = Math.Min(p.airKm, 20.0 - 10.0 * x.summer);             // 10 km in a summer high's haze, 20 in winter
                    break;
                case CirculationType.E:
                case CirculationType.S:
                    p.airKm = Math.Min(p.airKm, 20.0);
                    break;
            }
            if (x.overSea)
                p.airKm *= 1.25;                                                       // no dust or smoke offshore (guess)
            return p;
        }

        // ------------------------------------------------------------------------------------------- the air

        /// <summary>
        /// The rest of the hour once the temperature is known: dew point, heat storms, the kinds' bases and depths, the
        /// column's light, fog, thunder and the visibility, written into the sample.
        /// </summary>
        internal static void Air(ref WeatherSample w, in Context x, ref Parts p, double dayMeanTemp, double gust)
        {
            double t = x.hours, T = w.temperature, U = w.windSpeed;
            double winter = 1.0 - x.summer;

            // the heat storm: a hot humid afternoon over land under a high, an easterly or a southerly (weather 2.3)
            double rhDay = DayHumidity(x, p);
            double tdDay = DewPoint(dayMeanTemp, rhDay);
            if (!x.overSea && T > 22.0 && x.hour >= 13.0 && x.hour <= 22.0
                && (x.type == CirculationType.A || x.type == CirculationType.S || x.type == CirculationType.E || x.type == CirculationType.C))
            {
                double heat = Clamp01((T - 22.0) / 5.0) * Clamp01((tdDay - 12.0) / 4.0) * Clamp01(Math.Sin(Math.PI * (x.hour - 13.0) / 9.0) * 1.5);
                double cell = Clamp01(Noise(x.seed, 31, x.lat * 10.0, x.lon * 10.0, t / 1.0) * 2.6 - 1.55);
                double storm = heat * cell;
                if (storm > 0.0)
                {
                    p.heat = storm;
                    p.cb = Math.Max(p.cb, storm);
                    p.low = Math.Max(p.low, storm);
                    p.rain += storm * 25.0;
                    if (p.cb >= 0.3)
                    {
                        p.lowType = CloudType.Cumulonimbus;
                        p.lowThick = Clamp01(p.cb);
                    }
                }
            }

            // the dew point: the day's humidity holds it while the air cools and warms through the day; rain saturates
            double rh = rhDay;
            if (p.rain > 0.05)
                rh = Math.Max(rh, 0.9 + 0.02 * Math.Min(p.rain, 4.0));
            double td = Math.Min(DewPoint(dayMeanTemp, rh), T);
            if (p.rain > 0.05)
                td = Math.Max(td, T - 1.0);
            w.dewPoint = (float)td;
            double spread = T - DewPoint(dayMeanTemp, rh);                              // can go below 0: the fog to come at night

            // the kinds' bases and depths (weather 2.3, 2.5)
            double lcl = Math.Max(0.0, 125.0 * (T - td));                                // the cumulus condensation level
            double baseLow, depthLow;
            switch (p.lowType)
            {
                case CloudType.Cumulonimbus:
                    baseLow = Clamp(lcl, 500.0, 1200.0);
                    depthLow = 4000.0 + 5500.0 * x.summer;                                // 8-10 km tall in summer, lower in winter
                    break;
                case CloudType.Nimbostratus:
                    baseLow = p.nsBase;
                    depthLow = p.nsDepth;
                    break;
                case CloudType.Stratus:
                    baseLow = Clamp(lcl, 150.0, 400.0);
                    depthLow = 300.0 + 300.0 * p.low;
                    break;
                case CloudType.Cumulus:
                    baseLow = Clamp(lcl, 400.0, 2000.0);
                    depthLow = 1000.0 + 1500.0 * x.summer * Clamp01(p.cumulus * 1.5);
                    break;
                case CloudType.Stratocumulus:
                    baseLow = Clamp(lcl, 500.0, 1500.0);
                    depthLow = 400.0 + 400.0 * p.low;
                    break;
                default:
                    baseLow = 1000.0;
                    depthLow = 1500.0;
                    break;
            }

            // the lowest significant layer (a quarter of the sky or more) names the sky
            if (p.low >= 0.25)
            {
                w.cloudType = p.lowType;
                w.cloudBase = (float)baseLow;
                w.cloudDepth = (float)depthLow;
            }
            else if (p.mid >= 0.25)
            {
                w.cloudType = p.midType;
                w.cloudBase = (float)p.midBase;
                w.cloudDepth = (float)(p.midType == CloudType.Altostratus ? 1000.0 + 1000.0 * p.midThick : 600.0);
            }
            else if (p.high >= 0.1)
            {
                w.cloudType = p.highType;
                w.cloudBase = (float)p.highBase;
                w.cloudDepth = 1500f;
            }
            else
            {
                w.cloudType = p.low > 0.05 ? p.lowType : CloudType.Clear;
                w.cloudBase = (float)baseLow;
                w.cloudDepth = (float)depthLow;
            }
            w.lowCloud = (float)p.low;
            w.midCloud = (float)p.mid;
            w.highCloud = (float)p.high;
            // nimbostratus and cumulonimbus fill the depths: the layers above count only where they are not
            bool deep = p.lowType == CloudType.Nimbostratus || p.lowType == CloudType.Cumulonimbus;
            double midOver = deep ? p.mid * (1.0 - p.low) : p.mid, highOver = deep ? p.high * (1.0 - p.low) : p.high;
            w.cloudTransmittance = (float)Math.Max(1e-4, CloudLight.Column(p.lowType, p.low, p.lowThick, p.midType, midOver, p.midThick,
                                                                        p.highType, highOver, p.highThick));
            // the first build's cover fields, as their readers know them
            w.storm = (float)p.cb;
            w.cumulus = (float)Clamp01(p.lowType == CloudType.Cumulus ? p.low : p.cumulus);
            w.layered = (float)Clamp01(Math.Max(p.stratiform, p.mid * 0.85));
            w.rain = (float)Math.Max(0.0, p.rain);

            // fog: radiation fog on still, clear nights over land (thickest at dawn, gone as the air warms), sea fog where
            // mild moist air drifts over a colder sea (in banks)
            // a long night cools the ground longer: fog is an autumn and winter thing on the river land (the short summer
            // night less often reaches it), and in the long nights it forms under thinner cloud and lies on under the lid
            double nightLength = NightHours(x.lat, x.dayOfYear);
            double longNight = 0.3 + 0.7 * Clamp01((nightLength - 8.5) / 5.0);
            bool coastal = SeaBearing(x.place, out _);
            double calm = Clamp01(((coastal ? 3.5 : 3.0) - U) / 2.0);                // the coast's night land breeze is light
            double clear = 1.0 - Clamp01((p.low + 0.5 * p.mid - 0.25 - 0.35 * Clamp01((nightLength - 12.0) / 4.0)) / 0.5);
            double saturation = Clamp01((FogSpread - spread) / 2.0);
            double radiation = x.overSea ? 0.0 : calm * clear * saturation * longNight * (coastal ? CoastFog : x.place != null && x.place.id == "river_land" ? 1.3 : 1.0);
            double windOk = Clamp01((U - 1.0) / 1.5) * Clamp01((10.0 - U) / 3.0);
            double moist = Clamp01((td - x.seaTemp + SeaFogMargin) / 1.5);
            double bank = Clamp01(Noise(x.seed, 33, x.lat * 25.0, x.lon * 25.0, t / 3.0) * 2.4 - 1.0);
            // on the coast the sea fog comes in on an onshore wind, and drifts off the shallows and inland waters on others
            double seaward = SeaBearing(x.place, out double from) ? 0.3 + 0.7 * Onshore(w.windFrom, from) : (x.overSea ? 1.0 : 0.0);
            double advection = moist * windOk * bank * (x.overSea ? 1.0 : 0.8 * seaward);
            double fogAmount = Clamp01(Math.Max(radiation * 1.15, advection));
            double fogRange = fogAmount > 0.02 ? Visibility.FogRange(fogAmount) : double.PositiveInfinity;
            w.fog = (float)fogAmount;
            w.fogTop = (float)(fogAmount > 0.05 ? (radiation >= advection ? 30.0 + 90.0 * fogAmount : 200.0 + 300.0 * fogAmount)
                                                : 1000.0 + 1500.0 * x.summer * Clamp01(Math.Sin(Math.PI * (x.hour - 8.0) / 12.0)));

            // thunder: the cold front's line and the cold air's showers when the air is unstable (summer over land, autumn
            // and winter over the warmer sea), and the heat storms
            double seaWarmth = (x.overSea || seaward > 0.0) ? Clamp01((x.seaTemp - T - 1.0) / 4.0) : 0.0;
            double unstable = Clamp01(x.summer * 1.4 - 0.45) * (x.overSea ? 0.5 : 1.0) + 0.8 * seaWarmth;
            w.thunder = (float)Clamp01(Math.Max(p.heat * 1.2, Math.Max(p.band, p.showers) * Clamp01(unstable) * 1.2));

            // the view: the air (thicker as it nears saturation), fog, rain, snow and blown spray all dim it
            double rhNow = Humidity(T, td);
            double airM = p.airKm * 1000.0 * (1.0 - 0.55 * Clamp01((rhNow - 0.9) / 0.08));
            double share = w.SnowShare;
            double rainRange = share < 1.0 ? Visibility.RainRange(p.rain * (1.0 - share)) : double.PositiveInfinity;
            double snowRange = share > 0.0 ? Visibility.SnowRange(p.rain * share) : double.PositiveInfinity;
            w.visibility = (float)Visibility.Combine(airM, fogRange, rainRange, snowRange, x.overSea ? Visibility.SprayRange(U) : double.PositiveInfinity);
        }

        // ------------------------------------------------------------------------------------------- the sea breeze

        /// <summary>
        /// The sea breeze and the night's land breeze on the coast (weather 2.3): on a fair day from April to September with
        /// the land well warmer than the sea and the gradient wind light, a breeze of 3-6 m/s off the sea sets in about
        /// 11-13 h, peaks at 16-17 h and is gone by 21 h; at night a land breeze of 1-2 m/s. It turns the wind (and lifts a
        /// light one); the hour's measured speed already holds the afternoon's rise. Returns the breeze's share, 0..1.
        /// </summary>
        internal static double SeaBreeze(in Context x, double lowCloud, double landMaxTemp, ref double speed, ref double windFrom)
        {
            if (x.overSea || !SeaBearing(x.place, out double from))
                return 0.0;
            double season = Clamp01((x.summer - 0.3) / 0.3);
            double contrast = Clamp01((landMaxTemp - x.seaTemp - 2.0) / 4.0);
            double fair = 1.0 - Clamp01((lowCloud - 0.3) / 0.4);
            double light = Clamp01((7.0 - speed) / 4.0);
            double h = x.hour;
            double afternoon = h < 11.0 || h > 21.0 ? 0.0 : h < 16.5 ? Smooth((h - 11.0) / 5.5) : Smooth((21.0 - h) / 4.5);
            double night = h >= 22.0 || h <= 7.0 ? 1.0 : 0.0;
            double sea = 5.0 * season * contrast * fair * light * afternoon;
            double land = 1.5 * season * fair * light * night;
            if (sea <= 0.0 && land <= 0.0)
                return 0.0;
            // the wind as a vector blowing towards (east, north), the breeze added from the sea (or towards it at night)
            double a = windFrom * Math.PI / 180.0;
            double ux = -Math.Sin(a) * speed, uy = -Math.Cos(a) * speed;
            double b = from * Math.PI / 180.0;
            ux += -Math.Sin(b) * (sea - land);
            uy += -Math.Cos(b) * (sea - land);
            double sum = Math.Sqrt(ux * ux + uy * uy);
            if (sum > 0.05)
                windFrom = (Math.Atan2(-ux, -uy) * 180.0 / Math.PI + 360.0) % 360.0;
            speed = Math.Max(speed, sea);
            return Clamp01(Math.Max(sea, land) / Math.Max(sum, 0.5));
        }

        /// <summary>Where the sea breeze comes from at a coastal class (compass degrees): Hoek van Holland's coast faces
        /// north-west, De Kooy's sea lies west and north, Vlissingen's the south-west (guesses from the map). False inland
        /// and for classes without a coast.</summary>
        internal static bool SeaBearing(ClimateNormals.PlaceClass place, out double from)
        {
            from = 0.0;
            if (place == null)
                return false;
            switch (place.id)
            {
                case "coast": from = 295.0; return true;
                case "wadden": from = 320.0; return true;
                case "channel_nl": from = 245.0; return true;
                default: return false;
            }
        }

        /// <summary>How much a wind from a bearing blows onshore at a coast whose sea lies towards another: 1 straight in,
        /// 0 along the shore or off it.</summary>
        static double Onshore(double windFrom, double seaFrom) => Clamp01(Math.Cos((windFrom - seaFrom) * Math.PI / 180.0));

        // ------------------------------------------------------------------------------------------- humidity

        /// <summary>
        /// The day's mean relative humidity (0..1): the month's normal by the place (inland De Bilt about 88% in winter,
        /// 75% in late spring, guesses from memory of KNMI's tables; the coast 2 points more, the sea 5), moved by the air
        /// mass (the warm sector 95%, a continental east wind 50-70%, the clear cold air behind a front drier) and a slow
        /// wander of a few points from day to day.
        /// </summary>
        internal static double DayHumidity(in Context x, in Parts p)
        {
            double m = x.dayOfYear / (365.25 / 12.0) - 0.5;
            int m0 = (((int)Math.Floor(m)) % 12 + 12) % 12, m1 = (m0 + 1) % 12;
            double ft = m - Math.Floor(m);
            double rh = InlandHumidity[m0] + (InlandHumidity[m1] - InlandHumidity[m0]) * ft;
            if (x.overSea)
                rh += 0.05;
            else if (SeaBearing(x.place, out _))
                rh += 0.04;
            switch (x.type)
            {
                case CirculationType.E: rh -= 0.08 + 0.06 * (1.0 - x.summer); break;
                case CirculationType.N: rh -= 0.04; break;
                case CirculationType.A: rh -= 0.03 * x.summer; break;
                case CirculationType.S: rh += 0.02; break;
            }
            rh += (0.95 - rh) * Clamp01(p.warm * 1.2);
            rh -= 0.07 * p.behind;
            rh += 0.10 * (Noise(x.seed, 35, x.lat * 0.7, x.lon * 0.7, x.hours / 30.0) - 0.5);
            return Clamp(rh, 0.4, 0.98);
        }

        /// <summary>The hours from sunset to sunrise at a latitude on a day of the year (the Sun's declination, no refraction).</summary>
        internal static double NightHours(double lat, double dayOfYear)
        {
            double decl = 23.44 * Math.PI / 180.0 * Math.Sin(2.0 * Math.PI * (dayOfYear - 80.0) / 365.25);
            double c = -Math.Tan(lat * Math.PI / 180.0) * Math.Tan(decl);
            c = Math.Max(-1.0, Math.Min(1.0, c));
            return 24.0 - 2.0 * Math.Acos(c) * 180.0 / Math.PI / 15.0;
        }

        // the tuning, fitted in thirty-year runs against the measured tables (weather document 1.1; WeatherSkyTests):
        // DeckBias and DeckWinter how often the grey decks close (the month's mean cover and its dull days); FogSpread the
        // dew-point spread under which still clear nights fog (fog days, frosty-dawn mist); SeaFogMargin how far above the
        // sea's temperature the dew point may stand short of it and still fog the sea; CoastFog the coast's radiation fog
        // against the river land's (De Kooy's 44 days); DrySpell how much less falls in a winter cold spell, per degree
        internal const double DeckBias = 0.0, DeckWinter = 0.3, FogSpread = 0.45, SeaFogMargin = 0.3, CoastFog = 2.0, DrySpell = 0.3;

        /// <summary>Inland mean relative humidity by month (De Bilt, guess from memory of KNMI's normals).</summary>
        static readonly double[] InlandHumidity = { 0.88, 0.85, 0.80, 0.75, 0.74, 0.76, 0.77, 0.79, 0.83, 0.86, 0.89, 0.89 };

        /// <summary>The dew point (C) of air at a temperature and relative humidity (Magnus, over water).</summary>
        internal static double DewPoint(double temperature, double relativeHumidity)
        {
            double g = Math.Log(Math.Max(relativeHumidity, 0.01)) + 17.62 * temperature / (243.12 + temperature);
            return 243.12 * g / (17.62 - g);
        }

        /// <summary>The relative humidity (0..1) of air at a temperature with a dew point (Magnus).</summary>
        internal static double Humidity(double temperature, double dewPoint) =>
            Clamp01(Math.Exp(17.62 * dewPoint / (243.12 + dewPoint) - 17.62 * temperature / (243.12 + temperature)));

        // ------------------------------------------------------------------------------------------- helpers

        /// <summary>Smooth value noise in 0..1 over (x, y, t), two octaves, hashed from the seed and a channel.</summary>
        internal static double Noise(int seed, int channel, double x, double y, double t) =>
            0.65 * Layer(seed, channel, x, y, t) + 0.35 * Layer(seed, channel + 101, x * 2.3 + 17.1, y * 2.3 + 5.3, t * 1.7 + 3.1);

        static double Layer(int seed, int channel, double px, double py, double pt)
        {
            double fx0 = Math.Floor(px), fy0 = Math.Floor(py), ft0 = Math.Floor(pt);
            long ix = (long)fx0, iy = (long)fy0, it = (long)ft0;
            double fx = Smooth(px - fx0), fy = Smooth(py - fy0), ft = Smooth(pt - ft0);
            double x0 = Lerp(Lerp(V(seed, channel, ix, iy, it), V(seed, channel, ix + 1, iy, it), fx),
                             Lerp(V(seed, channel, ix, iy + 1, it), V(seed, channel, ix + 1, iy + 1, it), fx), fy);
            double x1 = Lerp(Lerp(V(seed, channel, ix, iy, it + 1), V(seed, channel, ix + 1, iy, it + 1), fx),
                             Lerp(V(seed, channel, ix, iy + 1, it + 1), V(seed, channel, ix + 1, iy + 1, it + 1), fx), fy);
            return Lerp(x0, x1, ft);
        }

        static double V(int seed, int channel, long a, long b, long c)
        {
            unchecked
            {
                ulong h = 14695981039346656037UL ^ 0x51A7UL;
                h = (h ^ (ulong)seed) * 1099511628211UL;
                h = (h ^ (ulong)channel) * 1099511628211UL;
                h = (h ^ (ulong)a) * 1099511628211UL;
                h = (h ^ (ulong)b) * 1099511628211UL;
                h = (h ^ (ulong)c) * 1099511628211UL;
                h ^= h >> 29;
                h *= 0xBF58476D1CE4E5B9UL;
                h ^= h >> 32;
                return (h & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }

        static double Lerp(double a, double b, double t) => a + (b - a) * t;
        internal static double Clamp01(double v) => v < 0.0 ? 0.0 : v > 1.0 ? 1.0 : v;
        static double Clamp(double v, double lo, double hi) => v < lo ? lo : v > hi ? hi : v;
        static double Smooth(double v)
        {
            v = Clamp01(v);
            return v * v * (3.0 - 2.0 * v);
        }
    }
}
