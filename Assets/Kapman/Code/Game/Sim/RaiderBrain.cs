using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>Where a people's raiders come from, what they strike and where they sell (Data/raiders_793.json).</summary>
    public sealed class RaiderOrigin
    {
        public string id, name, home, cls, tongue;
        public int fromYear;
        public float weight = 1f;
        public string[] targets = new string[0], sale = new string[0];
    }

    /// <summary>The raiders' table (Data/raiders_793.json): season, pressure by year, bands, defences, the raid and the
    /// chase. Every number is a starting guess, noted with its basis in the table.</summary>
    public sealed class RaiderRules
    {
        public int outDay = 121, homeDay = 273;
        public readonly SortedDictionary<int, float> pressure = new SortedDictionary<int, float>();
        public double pressureX = 1.0;
        public readonly List<RaiderOrigin> origins = new List<RaiderOrigin>();
        public int shipsMin = 1, shipsMax = 3, menPerShip = 30;
        public double attackRatio = 1.5;
        public readonly Dictionary<string, float> defence = new Dictionary<string, float>();
        public float stockLeft = 0.35f, dearer = 0.45f, lootShare = 0.5f;
        public string[] captivesAt = { "monastery", "village", "market" };
        public int captivesMin = 6, captivesMax = 20;
        public double restDays = 60.0;
        public double sightKm = 12.0, sprintKn = 5.5, sprintHours = 1.5, pullKn = 4.0, giveUpHours = 8.0;
        public double takeShare = 0.6, silverShare = 0.5;
        // the merchant's answers
        public double tributeShare = 0.3, tributeMinD = 60.0, parleyTributeShare = 0.15, submitCargoShare = 1.0, submitSilverShare = 0.6;
        public readonly Dictionary<string, double> parley = new Dictionary<string, double>();
        public int loseFightMenMin = 1, loseFightMenMax = 2, winFightMenMin = 0, winFightMenMax = 1;
        public double statusEveryMinutes = 15.0;

        public static RaiderRules Load(TextAsset json)
        {
            var r = new RaiderRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            var season = Json.Obj(o, "season");
            r.outDay = (int)Json.Num(season, "out_day", r.outDay);
            r.homeDay = (int)Json.Num(season, "home_day", r.homeDay);
            var p = Json.Obj(o, "pressure");
            foreach (var kv in Json.Obj(p, "by_year") ?? new Dictionary<string, object>())
                if (int.TryParse(kv.Key, out int y) && kv.Value is double v)
                    r.pressure[y] = (float)v;
            r.pressureX = Json.Num(p, "pressure_x", r.pressureX);
            foreach (var x in Json.Arr(o, "origins") ?? new List<object>())
                r.origins.Add(new RaiderOrigin
                {
                    id = Json.Str(x, "id"), name = Json.Str(x, "name"), home = Json.Str(x, "home"), cls = Json.Str(x, "class"),
                    tongue = Json.Str(x, "tongue"), fromYear = (int)Json.Num(x, "from_year", 0), weight = (float)Json.Num(x, "weight", 1),
                    targets = Strings(Json.Arr(x, "targets")), sale = Strings(Json.Arr(x, "sale")),
                });
            var band = Json.Obj(o, "band");
            r.shipsMin = (int)Json.Num(band, "ships_min", r.shipsMin);
            r.shipsMax = (int)Json.Num(band, "ships_max", r.shipsMax);
            r.menPerShip = (int)Json.Num(band, "men_per_ship", r.menPerShip);
            r.attackRatio = Json.Num(o, "attack_ratio", r.attackRatio);
            foreach (var kv in Json.Obj(o, "defence") ?? new Dictionary<string, object>())
                if (kv.Value is double d)
                    r.defence[kv.Key] = (float)d;
            var raid = Json.Obj(o, "raid");
            r.stockLeft = (float)Json.Num(raid, "stock_left", r.stockLeft);
            r.dearer = (float)Json.Num(raid, "dearer", r.dearer);
            r.lootShare = (float)Json.Num(raid, "loot_share", r.lootShare);
            var at = Json.Arr(raid, "captives_at");
            if (at != null)
                r.captivesAt = Strings(at);
            var cap = Json.Arr(raid, "captives_per_raid");
            if (cap != null && cap.Count == 2)
            {
                r.captivesMin = (int)(double)cap[0];
                r.captivesMax = (int)(double)cap[1];
            }
            r.restDays = Json.Num(raid, "rest_days", r.restDays);
            var sea = Json.Obj(o, "at_sea");
            r.sightKm = Json.Num(sea, "sight_km", r.sightKm);
            r.sprintKn = Json.Num(sea, "sprint_kn", r.sprintKn);
            r.sprintHours = Json.Num(sea, "sprint_hours", r.sprintHours);
            r.pullKn = Json.Num(sea, "pull_kn", r.pullKn);
            r.giveUpHours = Json.Num(sea, "give_up_hours", r.giveUpHours);
            r.takeShare = Json.Num(sea, "take_share", r.takeShare);
            r.silverShare = Json.Num(sea, "silver_share", r.silverShare);
            var pl = Json.Obj(o, "player");
            r.tributeShare = Json.Num(pl, "tribute_share", r.tributeShare);
            r.tributeMinD = Json.Num(pl, "tribute_min_d", r.tributeMinD);
            r.parleyTributeShare = Json.Num(pl, "parley_tribute_share", r.parleyTributeShare);
            r.submitCargoShare = Json.Num(pl, "submit_cargo_share", r.submitCargoShare);
            r.submitSilverShare = Json.Num(pl, "submit_silver_share", r.submitSilverShare);
            foreach (var kv in Json.Obj(pl, "parley") ?? new Dictionary<string, object>())
                if (kv.Value is double d)
                    r.parley[kv.Key] = d;
            var lose = Json.Arr(pl, "lose_fight_men");
            if (lose != null && lose.Count == 2)
            {
                r.loseFightMenMin = (int)(double)lose[0];
                r.loseFightMenMax = (int)(double)lose[1];
            }
            var win = Json.Arr(pl, "win_fight_men_lost");
            if (win != null && win.Count == 2)
            {
                r.winFightMenMin = (int)(double)win[0];
                r.winFightMenMax = (int)(double)win[1];
            }
            r.statusEveryMinutes = Json.Num(pl, "status_every_game_minutes", r.statusEveryMinutes);
            return r;
        }

        static string[] Strings(List<object> a) => a == null ? new string[0] : a.ConvertAll(x => x as string ?? "").ToArray();

        /// <summary>Bands out in a season of this year (the last year listed holds after it), times the dial.</summary>
        public double BandsIn(int year)
        {
            float n = 0f;
            foreach (var kv in pressure)
                if (kv.Key <= year)
                    n = kv.Value;
            return n * pressureX;
        }

        /// <summary>The men a place can put up at short notice.</summary>
        public double Defence(PortDef p)
        {
            float d = defence.TryGetValue(p.kind ?? "", out float v) ? v : 40f;
            return d * Math.Max(0.5, 1.0 + 0.25 * (p.size - 2));
        }

        public RaiderOrigin Origin(string id) => origins.Find(o => o.id == id);
    }

    /// <summary>A raid on a port, for the spreadsheets: who, where, when, whether they could, what they took.</summary>
    public sealed class RaidRecord
    {
        public double day;
        public string date, band, origin, port, what;
        public bool done;
        public double men, defence;
        public int loot, captives;
    }

    /// <summary>
    /// THE RAIDERS (Plans/ai_fleets.md, phase 4) - bands of longships of the time, simulated as ships of the world: each
    /// season (out about 1 May, home by 30 September) a people's bands go out from home, as many as history has that year
    /// (times the table's dial); each sails for a place it can overwhelm (its men 1.5 times the place's defence), raids it
    /// - the market is stripped and dear for weeks, loot and captives are carried off - then sells it all at a Norse market
    /// and goes home. On the way, a trader within sight that they can outrun may be taken: most of her cargo and half her
    /// silver. Their raids move the same markets as everyone's deals. Deterministic, like the rest of the world.
    /// </summary>
    public static class RaiderBrain
    {
        /// <summary>Once a day: a new year's season is planned; bands whose day has come get ready to go out.</summary>
        public static void Daily(WorldSim w)
        {
            var r = w.raiders;
            if (r == null)
                return;
            var date = w.Date;
            if (date.year != w.raidYear)
            {
                w.raidYear = date.year;
                PlanSeason(w, date.year, date.dayOfYear);
            }
            bool inSeason = date.dayOfYear >= r.outDay && date.dayOfYear < r.homeDay;
            foreach (var s in w.ships)
                if (s.raider && s.state == ShipState.LaidUp && s.at == s.home && s.seasonYear == date.year && inSeason && w.Day >= s.readyDay)
                {
                    s.state = ShipState.InPort;
                    s.readyDay = w.Day;
                }
        }

        /// <summary>The year's bands: this year's number by origin (by weight), reusing last year's crews at home.</summary>
        static void PlanSeason(WorldSim w, int year, int dayOfYear)
        {
            var r = w.raiders;
            var origins = r.origins.Where(o => o.fromYear <= year && GameData.Port(o.home) != null && GameData.Port(o.home).Exists(year)
                                               && w.data.classes.ContainsKey(o.cls)).ToList();
            if (origins.Count == 0)
                return;
            double total = r.BandsIn(year), weights = origins.Sum(o => o.weight);
            var rng = SimRandom.For(w.seed, "raiders." + year);
            double yearStart = w.Day - (dayOfYear - 1);                     // (the sim's day of 1 January this year)
            foreach (var o in origins)
            {
                double want = total * o.weight / Math.Max(0.001, weights);
                int n = (int)Math.Floor(want) + (rng.Next() < want - Math.Floor(want) ? 1 : 0);
                var idle = w.ships.Where(s => s.raider && s.origin == o.id && s.state == ShipState.LaidUp).ToList();
                for (int i = 0; i < n; i++)
                {
                    SimShip band;
                    if (i < idle.Count)
                        band = idle[i];
                    else
                    {
                        var home = GameData.Port(o.home);
                        string id = "raiders." + o.id + "." + (w.ships.Count(s => s.origin == o.id) + 1);
                        band = new SimShip
                        {
                            index = w.ships.Count, id = id, cls = w.data.classes[o.cls], home = home.index, homeRegion = home.region,
                            at = home.index, rng = SimRandom.For(w.seed, id), purse = new Purse(), hold = new Hold(),
                            raider = true, origin = o.id, state = ShipState.LaidUp,
                        };
                        var (x, z) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
                        band.pos = band.prevPos = new WorldXZ(x, z);
                        w.ships.Add(band);
                    }
                    int ships = band.rng.Range(r.shipsMin, r.shipsMax + 1);
                    band.bandShips = ships;
                    band.men = ships * r.menPerShip;
                    band.hold.capacity = band.cls.CargoKg * ships;
                    band.seasonYear = year;
                    band.raidsThisSeason = 0;
                    band.readyDay = yearStart + Math.Max(r.outDay, dayOfYear) - 1 + band.rng.Range(0.0, 45.0);
                }
            }
        }

        /// <summary>A band in port (at home, at a sale market, or at the place raided): what next.</summary>
        public static void InPort(WorldSim w, SimShip s)
        {
            var r = w.raiders;
            var date = w.Date;
            bool inSeason = date.dayOfYear >= r.outDay && date.dayOfYear < r.homeDay && s.seasonYear == date.year;
            var origin = r.Origin(s.origin);
            if (s.at == s.home)
            {
                SellAll(w, s);                                              // (anything left from the season: sold at home)
                if (!inSeason || s.raidsThisSeason >= 2 || origin == null)
                {
                    s.state = ShipState.LaidUp;
                    s.readyDay = double.MaxValue;                           // (till next year's season is planned)
                    return;
                }
                int target = PickTarget(w, s, origin);
                if (target < 0 || !SetOff(w, s, target))
                {
                    s.readyDay = w.Day + 3.0 + s.rng.Range(0.0, 4.0);       // nothing to strike now: wait at home
                    return;
                }
                return;
            }
            if (origin != null && Array.IndexOf(origin.sale, GameData.Ports[s.at].id) >= 0)
                SellAll(w, s);
            // laden, or the season closing: sell, then home; else another place to strike
            int next = -1;
            if (s.hold.Used > s.hold.capacity * 0.4f && !(origin != null && Array.IndexOf(origin.sale, GameData.Ports[s.at].id) >= 0))
                next = SaleMarket(w, s, origin);
            else if (inSeason && s.raidsThisSeason < 2 && origin != null)
                next = PickTarget(w, s, origin);
            if (next < 0)
                next = s.home;
            if (!SetOff(w, s, next) && !SetOff(w, s, s.home))
                s.readyDay = w.Day + 2.0;
        }

        static bool SetOff(WorldSim w, SimShip s, int dest)
        {
            if (dest < 0 || dest == s.at || !w.routes.Passage(s.at, dest, s.cls, s.path, s.kinds, out double km) || s.path.Count < 2)
                return false;
            s.dest = dest;
            s.passageKm = km;
            w.DepartNow(s);
            return true;
        }

        /// <summary>A place the band can overwhelm and hasn't struck lately, picked by its dice (the poorer defended first).</summary>
        static int PickTarget(WorldSim w, SimShip s, RaiderOrigin origin)
        {
            var r = w.raiders;
            int year = w.Date.year;
            var can = new List<(int port, double weight)>();
            foreach (string id in origin.targets)
            {
                var p = GameData.Port(id);
                if (p == null || !p.Exists(year) || p.index == s.at || w.RaidedUntil(p.index) > w.Day)
                    continue;
                double def = r.Defence(p);
                if (s.men < r.attackRatio * def)
                    continue;
                can.Add((p.index, 1.0 + s.men / Math.Max(1.0, def) * 0.1));
            }
            if (can.Count == 0)
                return -1;
            double sum = can.Sum(c => c.weight), pick = s.rng.Next() * sum;
            foreach (var c in can)
            {
                pick -= c.weight;
                if (pick <= 0.0)
                    return c.port;
            }
            return can[can.Count - 1].port;
        }

        static int SaleMarket(WorldSim w, SimShip s, RaiderOrigin origin)
        {
            if (origin == null)
                return -1;
            int year = w.Date.year, best = -1;
            double bestKm = double.MaxValue;
            foreach (string id in origin.sale)
            {
                var p = GameData.Port(id);
                if (p == null || !p.Exists(year))
                    continue;
                double days = w.DaysTo(s, s.at, p.index);
                if (days > 0.0 && days < bestKm)
                {
                    bestKm = days;
                    best = p.index;
                }
            }
            return best;
        }

        static void SellAll(WorldSim w, SimShip s)
        {
            if (s.hold.Lots.Count == 0)
                return;
            var m = w.MarketAt(s.at);
            int month = w.Date.month;
            foreach (var (good, count) in s.hold.Contents.ToList())
                if (m.Get(good.id) != null)
                    Deal.Sell(m, s.purse, s.hold, good.id, count, month, w.Day, s.id);
        }

        /// <summary>The band has come to the place it set out to strike: it raids it if it's strong enough.</summary>
        public static void Raid(WorldSim w, SimShip s, int port)
        {
            var r = w.raiders;
            var p = GameData.Ports[port];
            double def = r.Defence(p);
            var rec = new RaidRecord { day = w.Day, date = w.Date.ToString(), band = s.id, origin = s.origin, port = p.id, men = s.men, defence = def };
            if (w.RaidedUntil(port) > w.Day)
            {
                rec.what = "already stripped: went on";                    // (another band, or the chronicle's raid, came first)
                w.stats.raids.Add(rec);
                return;
            }
            if (s.men < r.attackRatio * def)
            {
                rec.what = "too strong: sheered off";
                w.stats.raids.Add(rec);
                return;
            }
            var m = w.MarketAt(port);
            // the loot: the most worth for its room first, the band's share of what's there
            int month = w.Date.month;
            foreach (var g in m.Goods.OrderByDescending(g => m.Price(g, month) / Math.Max(0.001f, g.good.Space)).ToList())
            {
                int take = Math.Min((int)Math.Floor(g.stock * r.lootShare), s.hold.RoomFor(g.good));
                if (take <= 0)
                    continue;
                g.stock -= take;
                s.hold.Add(g.good, take, 0.0, w.Day);
                rec.loot += take;
            }
            var captives = GameData.Good("captives");
            if (captives != null && Array.IndexOf(r.captivesAt, p.kind) >= 0)
            {
                int n = Math.Min(s.rng.Range(r.captivesMin, r.captivesMax + 1), s.hold.RoomFor(captives));
                if (n > 0)
                {
                    s.hold.Add(captives, n, 0.0, w.Day);
                    rec.captives = n;
                }
            }
            m.Shock(null, r.stockLeft, r.dearer);
            w.MarkRaided(port, w.Day + r.restDays);
            s.raidsThisSeason++;
            rec.done = true;
            rec.what = $"raided: {rec.loot} lots of goods, {rec.captives} captives";
            w.stats.raids.Add(rec);
            w.TellRaided(port, s);
        }

        /// <summary>After the hour's moves: bands at sea look for a trader they can catch, and take her.</summary>
        public static void Hunt(WorldSim w)
        {
            var r = w.raiders;
            if (r == null)
                return;
            double sight = r.sightKm * 1000.0;
            foreach (var band in w.ships)
            {
                if (!band.raider || band.state != ShipState.Sailing || band.chasing || band.hold.Free < band.hold.capacity * 0.2f)
                    continue;
                SimShip prey = null;
                double best = sight;
                foreach (var t in w.ships)
                {
                    if (t.raider || t.state != ShipState.Sailing || t.heaveTo || t.lastTakenDay > w.Day - 3.0)
                        continue;
                    double d = WorldXZ.Distance(band.pos, t.pos);
                    if (d < best)
                    {
                        best = d;
                        prey = t;
                    }
                }
                if (prey == null)
                    continue;
                // a chase over the hour: their best (sail or oars) against hers, in this wind, by daylight
                var (lat, lon) = Kapman.World.WorldFrame.ToLatLon(prey.pos.x, prey.pos.z);
                var ws = w.weather.Sample(w.WeatherSnapshot, lat, lon, w.WeatherHours, true);
                double bearing = Travel.Bearing(band.pos, prey.pos);
                double vb = Math.Max(Travel.Speed(band.cls, bearing, ws.windFrom, ws.windSpeed, out _), r.sprintKn * SimUnits.KnotMs);
                double vt = Travel.Speed(prey.cls, prey.heading, ws.windFrom, ws.windSpeed, out _);
                var date = w.Date;
                double sun = Sight.SunElevation(lat, date.dayOfYear, w.HourOfDay);
                double light = sun > -6.0 ? 1.0 : 0.15;
                double murk = Math.Min(1.0, ws.VisibilityKm * 1000.0 / Math.Max(1.0, best));
                double p = vb > vt ? (vb - vt) / vb * light * murk * 0.5 : 0.0;   // (most sails seen in time are run from)
                if (band.rng.Chance(Math.Min(0.9, p)))
                    Take(w, band, prey);
            }
        }

        /// <summary>A trader taken at sea: most of her cargo (their room allowing) and half her silver.</summary>
        public static void Take(WorldSim w, SimShip band, SimShip prey)
        {
            var r = w.raiders;
            int units = 0;
            foreach (var (good, count) in prey.hold.Contents.ToList())
            {
                int n = Math.Min((int)Math.Ceiling(count * r.takeShare), band.hold.RoomFor(good));
                if (n <= 0)
                    continue;
                prey.hold.Remove(good, n);
                band.hold.Add(good, n, 0.0, w.Day);
                units += n;
            }
            double silver = prey.purse.TakeShare(r.silverShare, TradeRules.Current);
            if (silver > 0.0)
                band.purse.Add("hacksilver", silver * 1.7);                // (1 denier ~ 1.7 g of silver, as their purses start)
            prey.lastTakenDay = w.Day;
            w.stats.shipsTaken++;
            w.stats.raids.Add(new RaidRecord
            {
                day = w.Day, date = w.Date.ToString(), band = band.id, origin = band.origin, port = "at sea: " + prey.id, done = true,
                men = band.men, loot = units, what = $"took a ship: {units} units and {silver:0} d of silver",
            });
            w.TellTaken(band, prey);
        }
    }
}
