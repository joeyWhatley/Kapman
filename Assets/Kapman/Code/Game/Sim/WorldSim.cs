using System;
using System.Collections.Generic;
using Kapman.Weather;
using Kapman.World;

namespace Kapman.Game.Sim
{
    /// <summary>The simulation's numbers to tune (every one a STARTING GUESS; sim_rules.json can come later).</summary>
    public sealed class SimRules
    {
        /// <summary>Days over which a price she saw fades halfway back to what everyone knows.</summary>
        public double beliefHalfLifeDays = 30.0;
        /// <summary>The share of her silver she keeps back.</summary>
        public double keepBack = 0.2;
        /// <summary>The least margin worth carrying, as a share of the price. 0.25 since ruling 30 of 7 Oct 2026 (it
        /// was 0.12): the world's traders stop buying while a quarter is still in the gap, so a laden leg pays the
        /// player too. It goes with the debt of 1,920 d (trade_rules.json) or not at all: from a model
        /// (Design/fixes/money_that_pays.md), to be looked at again once the alpha is played.</summary>
        public float minMargin = 0.25f;
        /// <summary>The most units of one good she buys at one visit, and the share of a market's stock.</summary>
        public int lotCap = 60;
        public float stockShare = 0.5f;
        /// <summary>Units dealt at a time (she looks at the price again after each lot).</summary>
        public int lotStep = 5;
        /// <summary>She sells here if it fetches at least this share of the best she believes she'd get elsewhere.</summary>
        public float sellNearBest = 0.85f;
        /// <summary>Deniers a man's keep costs a day at sea.</summary>
        public double keepPerManDay = 0.3;
        /// <summary>How many of the best ports she weighs, and how much her choice wanders (share of the best score).</summary>
        public int considerTop = 5;
        public double choiceTemperature = 0.25;
        /// <summary>Days she spends in port dealing and taking on stores, at least and at most.</summary>
        public double dwellMin = 0.5, dwellMax = 2.0;
        /// <summary>Real km she'll go from her home: river craft, coasters, sea-going traders.</summary>
        public double riverReachKm = 260, coastReachKm = 450, seaReachKm = 1500;
        /// <summary>Starting silver: deniers' worth, and per tonne of her cargo.</summary>
        public double silverBase = 60, silverPerTonne = 50;
        /// <summary>Days a ship lies up for want of silver before a new partner puts silver into her, as much as a new
        /// ship of her kind starts with (ships changed hands; they didn't rot for want of a master's money).</summary>
        public double newPartnerDays = 90.0;
        /// <summary>Deniers a man costs a day (keep and pay) while the ship is working, and laid up for the winter (STARTING GUESSES).</summary>
        public double wagePerManDay = 0.25, laidUpPerManDay = 0.05;
        /// <summary>At home with nothing worth carrying, laden or in ballast: days her men go home on a lay-up's pay (the
        /// least; up to twice it). 0: she waits in port on full pay, looking again every day or two.</summary>
        public double idleLayUpDays = 3.0;
        /// <summary>Chance per hour in a gale that a ship is lost, times her class's fragility.</summary>
        public double galeLossPerHour = 0.0004;
    }

    /// <summary>
    /// THE WORLD SIMULATION - every other ship of the world, run as data, an hour of the game at a time (Plans/ai_fleets.md,
    /// phase 1). The traders of each home port (traffic_793.json: how many, in which kinds of craft) sail their seasons:
    /// out in the sailing months, laid up at home in winter; in port they sell and buy through the same markets as the
    /// player (so what they carry moves prices) by what they believe prices are (TraderBrain); at sea they go at the speed
    /// the wind where they are gives their kind of craft (Travel), shelter from gales, and the small coasting craft lie up
    /// at night. News of prices travels with them (Knowledge: the ports' waterfronts and the ships swap what they know).
    /// They keep clear of each other as real vessels do (Seamanship): each to her own side of the lane, the give-way ship
    /// slowing or standing out; before an open-sea crossing they wait in port for a fair wind; a full landing makes the
    /// newcomers anchor off and take their turn.
    /// Deterministic: ships act in their order, each with her own dice from the seed, so a run plays the same twice.
    /// </summary>
    public sealed class WorldSim
    {
        public readonly SimData data;
        public readonly SimRules rules;
        public readonly SeamanshipRules sea;
        public readonly List<SimShip> ships = new List<SimShip>();
        public readonly SimStats stats = new SimStats();
        public IRoutes routes;
        public readonly WeatherModel weather;
        public readonly int seed, startYear, startDay;
        /// <summary>Tests: a ship's speed (real m/s) instead of the wind's.</summary>
        public Func<SimShip, double> speedOverride;
        /// <summary>The raiders' table (null: no raiders in this world).</summary>
        public readonly RaiderRules raiders;
        /// <summary>The fairs of the time (Data/fairs_793.json; null: none).</summary>
        public List<FairDef> fairs;

        /// <summary>Where the merchant's ship is and how she's going (real m, real m a game hour), asked at each hour; null:
        /// no player (the headless runs). The world's ships keep clear of her (Seamanship).</summary>
        public Func<(WorldXZ pos, WorldXZ perHour)?> playerNow;
        /// <summary>The merchant's ship as the seamanship pass sees her this hour (null when there's none).</summary>
        public SimShip Player { get; private set; }

        void UpdatePlayer()
        {
            var now = playerNow?.Invoke();
            if (now == null)
            {
                Player = null;
                return;
            }
            if (Player == null)
            {
                data.classes.TryGetValue("gislinge", out var cls);
                Player = new SimShip { index = -1, id = "player", cls = cls ?? new ShipClassDef { id = "player", length = 7.7f, beam = 2.2f },
                                       state = ShipState.Sailing, isPlayer = true, purse = new Purse(), hold = new Hold() };
            }
            var (pos, v) = now.Value;
            // her hour ahead: a straight line from where she is, at her present speed (no side of the lane: Portage keeps 0)
            Player.path.Clear();
            Player.path.Add(pos);
            Player.path.Add(new WorldXZ(pos.x + v.x, pos.z + v.z));
            Player.kinds.Clear();
            Player.kinds.Add(LaneGraph.Portage);
            Player.leg = 0;
            Player.along = 0.0;
            Player.go = Math.Sqrt(v.x * v.x + v.z * v.z);
            Player.total = Player.go + 1e9;
            Player.done = 1e8;
            Player.pos = Player.prevPos = pos;
            Player.heading = Player.go > 1.0 ? Travel.Bearing(pos, Player.path[1]) : Player.heading;
            Player.standOut = 0.0;
        }
        /// <summary>Ships keep clear of each other (Seamanship) and wait for a busy berth to clear. Off, they pass through
        /// each other: the game turns it off for now (KapmanGame.shipsKeepClear); the tests keep it on.</summary>
        public bool keepClear = true;
        /// <summary>The year whose raiding season has been planned.</summary>
        public int raidYear = int.MinValue;
        /// <summary>A port raided (the port, the band); a ship taken at sea (the band, the ship).</summary>
        public event Action<int, SimShip> Raided;
        public event Action<SimShip, SimShip> Taken;

        /// <summary>Game hours since the start (the clock's GameHours). Below 0 in the pre-run: the world's traders have
        /// been at work since 1 April, and day 0 stays the clock's start.</summary>
        public double Hours { get; private set; }
        public double Day => Hours / 24.0;
        /// <summary>The hour of the day, 0 to 24 (also before the start, where the hours are below 0).</summary>
        public double HourOfDay => ((Hours % 24.0) + 24.0) % 24.0;
        public GameDate Date => Calendar.At(startYear, startDay, Hours);
        /// <summary>The weather's clock now (game hours since the start of year 0).</summary>
        public double WeatherHours => epochHours + Hours;

        /// <summary>The weather at a place now, from the model the world's ships sail by (the chart game's ship sails by
        /// it too: Kapman.Game.ChartBody).</summary>
        public WeatherSample WeatherAt(WorldXZ p)
        {
            if (snapshot == null)
                snapshot = weather.Snapshot(epochHours + Hours);
            var (lat, lon) = WorldFrame.ToLatLon(p.x, p.z);
            return weather.Sample(snapshot, lat, lon, epochHours + Hours, true);
        }

        /// <summary>A kind of craft from the tables (null if there is none of that id).</summary>
        public ShipClassDef ClassOf(string id) => id != null && data.classes.TryGetValue(id, out var c) ? c : null;

        readonly double epochHours;                               // the weather's clock at hour 0
        readonly Knowledge[] boards;                              // each port's waterfront
        readonly Dictionary<long, double> kmCache = new Dictionary<long, double>();
        readonly Dictionary<int, List<int>> reachCache = new Dictionary<int, List<int>>();
        readonly int[] visitors;                                  // visiting ships at each landing (this hour)
        readonly int[] berths;                                    // and how many it takes
        readonly double[] raidedUntil;                            // a port raided isn't struck again till this day
        int lastDay = int.MinValue;
        object snapshot;

        public WorldSim(SimData data, SimRules rules, IRoutes routes, WeatherModel weather, int seed, int startYear, int startDay, double startHours,
                        SeamanshipRules seamanship = null, RaiderRules raiderRules = null)
        {
            this.data = data;
            this.rules = rules ?? new SimRules();
            sea = seamanship ?? new SeamanshipRules();
            raiders = raiderRules;
            visitors = new int[GameData.Ports.Count];
            berths = new int[GameData.Ports.Count];
            raidedUntil = new double[GameData.Ports.Count];
            for (int i = 0; i < berths.Length; i++)
            {
                float peak = data.berthPeak.TryGetValue(GameData.Ports[i].id, out float p) ? p : 0f;
                berths[i] = Math.Max(sea.berthMin, (int)Math.Round(peak * sea.berthShare));
            }
            this.routes = routes ?? new StraightRoutes();
            this.weather = weather ?? new WeatherModel(793);
            this.seed = seed;
            this.startYear = startYear;
            this.startDay = startDay;
            Hours = startHours;
            epochHours = ((double)startYear * 365.25 + (startDay - 1)) * 24.0;   // as GameClock.HoursSinceEpoch
            boards = new Knowledge[GameData.Ports.Count];
            for (int i = 0; i < boards.Length; i++)
                boards[i] = new Knowledge();
        }

        public Market MarketAt(int port) => Markets.Of(GameData.Ports[port], Date.year, Day);

        /// <summary>Makes the world's fleets: each home port's traders, their kinds of craft drawn by the table's shares.</summary>
        public void Populate()
        {
            var tradeRules = TradeRules.Current;
            int year = Date.year;
            foreach (var fleet in data.fleets)
            {
                var home = GameData.Port(fleet.port);
                if (home == null || !home.Exists(year))
                    continue;
                for (int i = 0; i < fleet.traders; i++)
                {
                    string id = fleet.port + "." + (i + 1);
                    var rng = SimRandom.For(seed, id);
                    var cls = PickClass(fleet, ref rng);
                    if (cls == null)
                        continue;
                    var s = new SimShip
                    {
                        index = ships.Count, id = id, cls = cls, home = home.index, homeRegion = home.region, at = home.index,
                        rng = rng, purse = new Purse(), hold = new Hold { capacity = cls.CargoKg },
                    };
                    double deniers = StartingSilver(cls);
                    s.purse.Add("hacksilver", Math.Round(deniers * 1.7, 1));          // silver by weight: good everywhere, at a small loss
                    s.startWorth = s.purse.Silver(tradeRules);
                    s.readyDay = Day + s.rng.Range(0.0, 3.0);                        // not all on the same day
                    var (x, z) = WorldFrame.ToWorld(home.lat, home.lon);
                    s.pos = new WorldXZ(x, z);
                    ships.Add(s);
                }
            }
        }

        /// <summary>Silver a new ship of this kind starts with (deniers).</summary>
        double StartingSilver(ShipClassDef cls) => rules.silverBase + rules.silverPerTonne * cls.cargoT;

        /// <summary>A new partner puts silver into a ship laid up broke: as much as a new one of her kind starts with
        /// (counted into what she started with, so her gains stay honest).</summary>
        void NewPartner(SimShip s)
        {
            double before = s.purse.Silver(TradeRules.Current);
            s.purse.Add("hacksilver", Math.Round(StartingSilver(s.cls) * 1.7, 1));
            s.startWorth += s.purse.Silver(TradeRules.Current) - before;
            s.owedWages = 0.0;
            s.brokeSince = -1.0;
            stats.newPartners++;
        }

        ShipClassDef PickClass(HomeFleet fleet, ref SimRandom rng)
        {
            double sum = 0.0;
            foreach (var c in fleet.classes)
                sum += c.share;
            double r = rng.Next() * sum;
            foreach (var c in fleet.classes)
            {
                r -= c.share;
                if (r <= 0.0 && data.classes.TryGetValue(c.cls, out var def))
                    return def;
            }
            return data.classes.TryGetValue(fleet.classes[fleet.classes.Count - 1].cls, out var last) ? last : null;
        }

        /// <summary>One game hour of the world.</summary>
        public void Hour()
        {
            Hours += 1.0;
            int day = (int)Math.Floor(Day);
            if (day != lastDay)
            {
                lastDay = day;
                Markets.CatchUpAll(Day);
                PayTheCrews();
                RaiderBrain.Daily(this);
                Fairs.Daily(this);
                stats.Daily(this);
            }
            snapshot = weather.Snapshot(epochHours + Hours);
            CountVisitors();
            UpdatePlayer();
            // each ship decides what she'd do this hour; then those under way keep clear of each other; then they move;
            // then raiders at sea look for prey
            for (int i = 0; i < ships.Count; i++)
                Step(ships[i]);
            if (keepClear)
                stats.closeCalls += Seamanship.Resolve(this);
            foreach (var s in ships)
                if (s.state == ShipState.Sailing && !s.chasing)
                {
                    Advance(s);
                    if (s.holdOut > 0)
                        s.holdOut--;
                    else
                        s.standOut *= 1.0 - sea.standOutFade;             // back to her own side once clear
                }
            if (raiders != null)
                RaiderBrain.Hunt(this);
        }

        /// <summary>
        /// Steps the world to `toHours` with nobody watching: the pre-run at a new game (the traders have been at work
        /// since the rules' world_from, 1 April). No player's ship to keep clear of; the same hours as play, in the same
        /// order, so one seed gives one world. `daily` is called once for each day stepped into, before that day's hours
        /// (the held shocks stay dear: Shocks.Daily). Returns the hours stepped.
        /// </summary>
        public int RunTo(double toHours, Action<double> daily = null)
        {
            var player = playerNow;
            playerNow = null;
            int n = 0, day = (int)Math.Floor(Day);
            while (Hours + 1.0 <= toHours + 1e-9)
            {
                Hour();
                n++;
                int now = (int)Math.Floor(Day);
                if (now != day)
                {
                    day = now;
                    daily?.Invoke(Day);
                }
            }
            playerNow = player;
            Player = null;
            return n;
        }

        /// <summary>The weather now, for sampling many places this hour.</summary>
        public object WeatherSnapshot => snapshot;
        public double RaidedUntil(int port) => raidedUntil[port];
        public void MarkRaided(int port, double until) => raidedUntil[port] = Math.Max(raidedUntil[port], until);
        public void TellRaided(int port, SimShip band)
        {
            Post("raid", $"Raiders struck {GameData.Ports[port].name}.", port);
            Raided?.Invoke(port, band);
        }

        public void TellTaken(SimShip band, SimShip prey)
        {
            // the ship taken carries the news on to her next port (and her people to their home's waterfront)
            var item = NewNews("taken", $"{Cap(SeaWatch.Label(prey))} was taken by raiders at sea.", -1);
            item.x = prey.pos.x;                                              // (where: the news says roughly where she was)
            item.z = prey.pos.z;
            prey.knows.Hear(item, Day, "her own crew");
            Taken?.Invoke(band, prey);
        }

        /// <summary>A port's waterfront: what's known there of prices and news.</summary>
        public Knowledge Board(int port) => boards[port];

        /// <summary>
        /// What each waterfront knows at the start, as it would have after a season of ships: the prices at the ports
        /// within `reachKm`, as seen some days ago (older the further: ~`kmPerDay` a day, plus a few), and each piece of
        /// news already posted, if it could have come this far by now at that pace. Call after the chronicle's recent
        /// events are posted.
        /// </summary>
        public void SeedKnowledge(double reachKm = 700.0, double kmPerDay = 60.0)
        {
            var rng = SimRandom.For(seed, "boards");
            int month = Date.month;
            var all = new List<NewsItem>();
            foreach (var b in boards)
                foreach (var (item, _, _) in b.News)
                    if (!all.Contains(item))
                        all.Add(item);
            for (int i = 0; i < boards.Length; i++)
            {
                var p = GameData.Ports[i];
                if (!p.Exists(Date.year))
                    continue;
                var (px, pz) = WorldFrame.ToWorld(p.lat, p.lon);
                for (int j = 0; j < boards.Length; j++)
                {
                    var q = GameData.Ports[j];
                    if (i == j || !q.Exists(Date.year))
                        continue;
                    var (qx, qz) = WorldFrame.ToWorld(q.lat, q.lon);
                    double km = Math.Sqrt((qx - px) * (qx - px) + (qz - pz) * (qz - pz)) / 1000.0;
                    if (km > reachKm)
                        continue;
                    var snap = PriceSnapshot.Of(MarketAt(j), month, Day - km / kmPerDay - rng.Range(2.0, 12.0));
                    boards[i].Learn(snap);
                }
                foreach (var item in all)
                {
                    if (item.port < 0 || item.port == i)
                        continue;
                    var q = GameData.Ports[item.port];
                    var (qx, qz) = WorldFrame.ToWorld(q.lat, q.lon);
                    double km = Math.Sqrt((qx - px) * (qx - px) + (qz - pz) * (qz - pz)) / 1000.0;
                    if (Day - item.day >= km / kmPerDay)
                        boards[i].Hear(item, item.day + km / kmPerDay, "ships from " + q.name);
                }
            }
        }

        /// <summary>News of something that happened at a port, known there from today (it spreads with the ships).</summary>
        public NewsItem Post(string kind, string text, int port, double day = double.NaN)
        {
            var item = NewNews(kind, text, port);
            if (!double.IsNaN(day))
                item.day = day;                                              // (it happened before: the chronicle's past)
            if (port >= 0 && port < boards.Length)
                boards[port].Hear(item, Day, "the waterfront at " + GameData.Ports[port].name);
            return item;
        }

        /// <summary>One piece of news known at several ports from a day (a region's famine): one item, not one each.</summary>
        public NewsItem PostAt(string kind, string text, IList<int> ports, double day, string refId = null)
        {
            if (ports == null || ports.Count == 0)
                return null;
            var item = NewNews(kind, text, ports[0]);
            item.day = day;
            item.refId = refId;
            foreach (int p in ports)
                if (p >= 0 && p < boards.Length)
                    boards[p].Hear(item, day, "the waterfront at " + GameData.Ports[p].name);
            return item;
        }

        NewsItem NewNews(string kind, string text, int port) =>
            new NewsItem { id = kind + "." + port + "." + newsCount++, kind = kind, text = text, port = port, day = Day };

        int newsCount;

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);
        /// <summary>Sends a ship off on the passage already in her path (raiders).</summary>
        public void DepartNow(SimShip s) => Depart(s);

        /// <summary>A ship put off her line (raiders after a chase) takes up her passage again from where she is: straight
        /// to the nearest point of the rest of it, then on.</summary>
        public void Rejoin(SimShip s)
        {
            if (s.path.Count < 2)
                return;
            int best = s.path.Count - 1;
            double bestD = double.MaxValue;
            for (int k = Math.Max(1, s.leg + 1); k < s.path.Count; k++)
            {
                double d = WorldXZ.Distance(s.pos, s.path[k]);
                if (d < bestD)
                {
                    bestD = d;
                    best = k;
                }
            }
            var rest = new List<WorldXZ> { s.pos };
            var kinds = new List<byte> { best - 1 < s.kinds.Count ? s.kinds[best - 1] : LaneGraph.Sea };
            for (int k = best; k < s.path.Count; k++)
            {
                rest.Add(s.path[k]);
                if (k + 1 < s.path.Count)
                    kinds.Add(k < s.kinds.Count ? s.kinds[k] : LaneGraph.Sea);
            }
            s.path.Clear();
            s.path.AddRange(rest);
            s.kinds.Clear();
            s.kinds.AddRange(kinds);
            s.leg = 0;
            s.along = 0.0;
            s.done = sea.taperKm * 1000.0;                                  // (already out: her full side of the lane)
            s.total = s.done;
            for (int k = 0; k + 1 < s.path.Count; k++)
                s.total += WorldXZ.Distance(s.path[k], s.path[k + 1]);
            s.prevPos = s.pos;
        }

        /// <summary>Visiting ships at each landing (in port, or loaded and waiting for a wind there); home craft have
        /// their own places.</summary>
        void CountVisitors()
        {
            Array.Clear(visitors, 0, visitors.Length);
            foreach (var s in ships)
                if ((s.state == ShipState.InPort || s.state == ShipState.Waiting) && s.at != s.home && !s.raider)
                    visitors[s.at]++;
        }

        /// <summary>How many visiting ships a landing takes at once, and how many are there.</summary>
        public int Berths(int port) => berths[port];
        public int Visitors(int port) => visitors[port];

        /// <summary>Each day her men's keep and pay is owed (less in the winter lay-up, when they're home); it's paid out
        /// in port, before she buys. A ship that can't pay what's owed, with nothing aboard to sell, pays her crew off and
        /// lies up a month.</summary>
        void PayTheCrews()
        {
            foreach (var s in ships)
                if (s.state != ShipState.Lost && !s.raider)                    // (raiders are paid in loot)
                    s.owedWages += s.cls.crew * (s.state == ShipState.LaidUp ? rules.laidUpPerManDay : rules.wagePerManDay);
        }

        /// <summary>Pays the crew what's owed, in port; false if she can't.</summary>
        bool PayOff(SimShip s, Market m)
        {
            if (s.owedWages <= 0.0)
                return true;
            double can = Math.Min(s.owedWages, s.purse.Worth(m.zone, TradeRules.Current));
            if (can > 0.0 && s.purse.Pay(can, m.zone, TradeRules.Current, out _))
            {
                s.wages += can;
                s.owedWages -= can;
            }
            return s.owedWages <= 0.01;
        }
        void Step(SimShip s)
        {
            s.prevPos = s.pos;
            switch (s.state)
            {
                case ShipState.Lost:
                    return;
                case ShipState.LaidUp when s.raider:
                    return;                                                  // (RaiderBrain.Daily sends them out in season)
                case ShipState.InPort when s.raider:
                    if (Day >= s.readyDay)
                        RaiderBrain.InPort(this, s);
                    return;
                case ShipState.LaidUp:
                    if (Day >= s.readyDay)
                    {
                        if (s.brokeSince >= 0.0 && Day - s.brokeSince >= rules.newPartnerDays)
                            NewPartner(s);
                        if (s.rng.Chance(data.Season(s.homeRegion, Date.month)))
                            s.state = ShipState.InPort;
                        else
                            s.readyDay = Day + 7.0;
                    }
                    return;
                case ShipState.InPort:
                    if (Day >= s.readyDay)
                        InPort(s);
                    return;
                case ShipState.Waiting:
                    if (Day >= s.readyDay)
                    {
                        if (StartIsBusy(s) && Day - s.waitSince < sea.maxStartWaitDays)
                            return;                                          // (the next hour; after a day she picks her way out)
                        if (!NeedsFairWind(s) || FairWind(s) || Day - s.waitSince >= sea.maxWaitDays)
                            Depart(s);
                        else
                            s.readyDay = Day + sea.checkEveryH / 24.0;
                    }
                    return;
                case ShipState.Anchored:
                    TakeBerth(s);
                    return;
                case ShipState.Sailing:
                    PlanMove(s);
                    return;
            }
        }

        /// <summary>She can't pay her crew: they're paid off with what she had, and she lies up a month (a new partner
        /// may put silver into her in time).</summary>
        void GoBroke(SimShip s)
        {
            stats.broke++;
            s.owedWages = 0.0;
            if (s.brokeSince < 0.0)
                s.brokeSince = Day;
            s.state = ShipState.LaidUp;
            s.readyDay = Day + 30.0;
        }

        void InPort(SimShip s)
        {
            var date = Date;
            var m = MarketAt(s.at);
            // the waterfront's news, and her own
            var seen = PriceSnapshot.Of(m, date.month, Day);
            seen.seenBy = s.id;                                              // (she saw these herself: told on as a witness)
            s.knows.Learn(seen);
            Knowledge.Swap(s.knows, boards[s.at]);
            Knowledge.SwapNews(s.knows, boards[s.at], Day, SeaWatch.Label(s), "the waterfront at " + GameData.Ports[s.at].name);
            TraderBrain.Sell(this, s, m, date.month, date.year);
            bool paid = PayOff(s, m);
            if (!paid && s.hold.Lots.Count > 0)
            {
                TraderBrain.SellToPay(this, s, m, date.month, date.year);   // her men won't wait: she sells what she must
                paid = PayOff(s, m);
            }
            if (s.voyageOpen)
            {
                stats.VoyageEnded(this, s);
                s.voyageOpen = false;
            }
            if (!paid && s.hold.Lots.Count == 0)
            {
                GoBroke(s);
                return;
            }
            // the season: at home she may lay up; away as it closes, she heads home
            float season = data.Season(s.homeRegion, date.month);
            if (s.at == s.home && s.rng.Chance(1.0 - season))
            {
                s.state = ShipState.LaidUp;
                s.readyDay = Day + 7.0;
                return;
            }
            bool goHome = s.at != s.home && season < 0.35f;
            int dest = TraderBrain.Plan(this, s, m, date.month, date.year, goHome);
            if (dest < 0 && s.at != s.home && s.rng.Chance(0.3))
                dest = TraderBrain.Plan(this, s, m, date.month, date.year, true);     // nothing worth carrying: home
            if (dest < 0 && !paid)
            {
                GoBroke(s);                                                  // unpaid, and nowhere worth taking what's aboard
                return;
            }
            if (dest < 0 && s.at == s.home && rules.idleLayUpDays > 0.0)
            {
                // nothing worth doing from home (whatever she has aboard stays there): her men go home on a lay-up's pay
                // for a while
                s.state = ShipState.LaidUp;
                s.readyDay = Day + rules.idleLayUpDays * (1.0 + s.rng.Range(0.0, 1.0));
                return;
            }
            if (dest < 0 || !routes.Passage(s.at, dest, s.cls, s.path, s.kinds, out double km) || s.path.Count < 2)
            {
                s.readyDay = Day + 1.0 + s.rng.Range(0.0, 2.0);
                return;
            }
            s.dest = dest;
            s.passageKm = km;
            // before an open-sea crossing she waits for a wind she can use (loaded, in port)
            if (NeedsFairWind(s) && !FairWind(s))
            {
                s.state = ShipState.Waiting;
                s.windBound = true;
                s.waitSince = Day;
                s.readyDay = Day + sea.checkEveryH / 24.0;
                stats.waitedForWind++;
                return;
            }
            s.state = ShipState.Waiting;                                      // loaded and ready: off when the way is clear
            s.windBound = false;
            s.waitSince = Day;
            s.readyDay = Day;
            if (!keepClear || !StartIsBusy(s))
                Depart(s);
        }

        /// <summary>Another ship is still close by her berth, under way or at anchor: she waits for her to get clear
        /// (ships leaving one landing together go one after another).</summary>
        bool StartIsBusy(SimShip s)
        {
            byte kind = s.kinds.Count > 0 ? s.kinds[0] : LaneGraph.Approach;
            foreach (var o in ships)
                if (o != s && (o.state == ShipState.Sailing || o.state == ShipState.Anchored) &&
                    WorldXZ.Distance(o.pos, s.path[0]) < sea.PassM(kind, Seamanship.KindOf(o), s.cls, o.cls))
                    return true;
            return false;
        }

        /// <summary>Sends a ship off now on a given passage (tests, and a dev spawn): from her port `at` to `dest`.</summary>
        public void SetSail(SimShip s, IList<WorldXZ> path, IList<byte> kinds, int dest)
        {
            s.path.Clear();
            s.path.AddRange(path);
            s.kinds.Clear();
            s.kinds.AddRange(kinds);
            s.dest = dest;
            double km = 0.0;
            for (int k = 0; k + 1 < s.path.Count; k++)
                km += WorldXZ.Distance(s.path[k], s.path[k + 1]) / 1000.0;
            s.passageKm = km;
            s.state = ShipState.InPort;
            Depart(s);
        }

        void Depart(SimShip s)
        {
            if (s.state == ShipState.Waiting && s.windBound)
                stats.waitedDays += Day - s.waitSince;
            s.windBound = false;
            // leaving a port with officials, bound out of the realm: she may be searched (Patrols)
            if (!s.raider && s.dest >= 0)
                Patrols.SearchTrader(this, s, GameData.Ports[s.at], GameData.Ports[s.dest]);
            s.leg = 0;
            s.along = 0.0;
            s.done = 0.0;
            s.total = 0.0;
            for (int k = 0; k + 1 < s.path.Count; k++)
                s.total += WorldXZ.Distance(s.path[k], s.path[k + 1]);
            s.standOut = 0.0;
            s.holdOut = 0;
            s.pos = s.path[0];
            s.prevPos = s.pos;
            s.heading = Travel.Bearing(s.path[0], s.path[1]);
            s.departedDay = Day;
            s.voyages++;
            s.voyageOpen = true;
            s.state = ShipState.Sailing;
            stats.VoyageStarted(this, s);
        }

        /// <summary>A passage with this much open sea in it (real km) is a crossing: she waits for a fair wind.</summary>
        bool NeedsFairWind(SimShip s)
        {
            if (!s.cls.hasSail)
                return false;
            double sea_ = 0.0;
            for (int k = 0; k + 1 < s.path.Count; k++)
                if (k < s.kinds.Count && s.kinds[k] == LaneGraph.Sea)
                    sea_ += WorldXZ.Distance(s.path[k], s.path[k + 1]);
            return sea_ >= sea.seaCrossingKm * 1000.0;
        }

        /// <summary>The wind in port now would carry her across: no gale, and she can lie her course over the sea part
        /// (not closer to the wind than her closest plus the table's margin).</summary>
        bool FairWind(SimShip s)
        {
            int first = -1, last = -1;
            for (int k = 0; k < s.kinds.Count && k + 1 < s.path.Count; k++)
                if (s.kinds[k] == LaneGraph.Sea)
                {
                    if (first < 0)
                        first = k;
                    last = k + 1;
                }
            if (first < 0)
                return true;
            var at = s.path[0];
            var (lat, lon) = WorldFrame.ToLatLon(at.x, at.z);
            var w = weather.Sample(snapshot, lat, lon, epochHours + Hours, true);
            if (w.windSpeed > 17.0)
                return false;
            if (w.windSpeed < 1.5)
                return s.cls.oarsKn > 0f;                                   // a calm: only if she can row
            double course = Travel.Bearing(s.path[first], s.path[last]);
            double offWind = Math.Abs(((course - w.windFrom) % 360.0 + 540.0) % 360.0 - 180.0);
            return offWind >= s.cls.closestToWind + sea.foulMarginDeg;
        }

        /// <summary>How far she'd go this hour (real m), before giving way to anyone.</summary>
        void PlanMove(SimShip s)
        {
            var a = s.path[s.leg];
            var b = s.path[s.leg + 1];
            double course = Travel.Bearing(a, b);
            s.heading = course;
            s.go = 0.0;
            // hove to for a hail (Encounter), or a small coasting craft lying up for the night (the long-distance ships
            // sail on)
            double hourOfDay = HourOfDay;
            if (s.chasing)
                return;                                                      // (RaiderEncounter moves them in the chase)
            s.lyingUp = s.heaveTo || (!s.cls.Uses("trade_long") && !s.raider && (hourOfDay < 5.0 || hourOfDay >= 21.0));
            if (s.lyingUp)
                return;
            double speed;
            if (speedOverride != null)
                speed = speedOverride(s);
            else
            {
                var (lat, lon) = WorldFrame.ToLatLon(s.pos.x, s.pos.z);
                var w = weather.Sample(snapshot, lat, lon, epochHours + Hours, true);
                speed = Travel.Speed(s.cls, course, w.windFrom, w.windSpeed, out bool gale);
                if (gale && s.rng.Chance(rules.galeLossPerHour * s.cls.galeFragility))
                {
                    s.state = ShipState.Lost;
                    stats.Lost(this, s);
                    return;
                }
            }
            s.go = SimUnits.RealMetresPerGameHour(speed);
        }

        /// <summary>She goes her (settled) way for the hour - stopping at her place in the anchorage if the landing ahead
        /// is full.</summary>
        void Advance(SimShip s)
        {
            double go = s.go;
            double stop = AnchorDistance(s, out int slot);
            if (stop < s.total && s.done + go >= stop)
            {
                stats.Arrived(this, s);
                s.state = ShipState.Anchored;
                s.anchoredSince = Hours;
                s.anchorSlot = slot;
                s.pos = AnchorPlace(s, slot);
                s.go = 0.0;
                stats.anchored++;
                return;
            }
            while (go > 0.0)
            {
                double leg = WorldXZ.Distance(s.path[s.leg], s.path[s.leg + 1]);
                double left = leg - s.along;
                if (go < left)
                {
                    s.along += go;
                    s.done += go;
                    break;
                }
                go -= left;
                s.done += left;
                s.leg++;
                s.along = 0.0;
                if (s.leg >= s.path.Count - 1)
                {
                    s.leg = s.path.Count - 2;
                    s.along = WorldXZ.Distance(s.path[s.leg], s.path[s.leg + 1]);
                    Arrive(s);
                    return;
                }
            }
            s.pos = Place(s, s.leg, s.along, s.done, s.standOut);
            s.heading = Travel.Bearing(s.path[s.leg], s.path[s.leg + 1]);
        }

        /// <summary>Where she'd be after going `go` real metres on with this stand-out (for keeping clear; changes
        /// nothing).</summary>
        public WorldXZ Where(SimShip s, double go, double standOut)
        {
            double stop = AnchorDistance(s, out int slot);
            if (stop < s.total && s.done + go >= stop)
                return AnchorPlace(s, slot);
            int leg = s.leg;
            double along = s.along, done = s.done;
            while (go > 0.0)
            {
                double len = WorldXZ.Distance(s.path[leg], s.path[leg + 1]);
                double left = len - along;
                if (go < left)
                {
                    along += go;
                    done += go;
                    break;
                }
                go -= left;
                done += left;
                if (leg + 2 >= s.path.Count)
                    return s.path[s.path.Count - 1];                         // at the berth (or her anchorage)
                leg++;
                along = 0.0;
            }
            return Place(s, leg, along, done, standOut);
        }

        /// <summary>The point of her passage's line this many real metres from its start (on the line itself).</summary>
        public WorldXZ PointAlong(SimShip s, double along)
        {
            for (int k = 0; k + 1 < s.path.Count; k++)
            {
                double len = WorldXZ.Distance(s.path[k], s.path[k + 1]);
                if (along <= len || k + 2 >= s.path.Count)
                {
                    double t = len > 0.0 ? Math.Max(0.0, Math.Min(1.0, along / len)) : 0.0;
                    var a = s.path[k];
                    var b = s.path[k + 1];
                    return new WorldXZ(a.x + (b.x - a.x) * t, a.z + (b.z - a.z) * t);
                }
                along -= len;
            }
            return s.path.Count > 0 ? s.path[s.path.Count - 1] : s.pos;
        }

        /// <summary>A point of her passage: on the lane's line, then to her own side of it (eased in and out near the
        /// berths), plus any stand-out.</summary>
        WorldXZ Place(SimShip s, int leg, double along, double done, double standOut)
        {
            var a = s.path[leg];
            var b = s.path[leg + 1];
            double len = WorldXZ.Distance(a, b);
            double t = len > 0.0 ? along / len : 0.0;
            double x = a.x + (b.x - a.x) * t, z = a.z + (b.z - a.z) * t;
            double taper = sea.taperKm * 1000.0;
            double ease = taper > 0.0 ? Math.Min(1.0, Math.Min(done, Math.Max(0.0, s.total - done)) / taper) : 1.0;
            byte kind = leg < s.kinds.Count ? s.kinds[leg] : LaneGraph.Sea;
            double side = sea.SideM(kind) * ease + standOut;
            if (len <= 0.0 || side == 0.0)
                return new WorldXZ(x, z);
            double ux = (b.x - a.x) / len, uz = (b.z - a.z) / len;
            return new WorldXZ(x + uz * side, z - ux * side);              // starboard of her way: (uz, -ux)
        }

        /// <summary>At the end of her passage, alongside (a full landing has stopped her at its anchorage before this).
        /// Raiders come ashore - and strike, if this is a place they came to raid.</summary>
        void Arrive(SimShip s)
        {
            stats.Arrived(this, s);
            if (s.raider)
            {
                s.pos = s.path[s.path.Count - 1];
                s.at = s.dest;
                s.dest = -1;
                s.state = ShipState.InPort;
                s.readyDay = Day + 0.3;
                var origin = raiders?.Origin(s.origin);
                if (origin != null && s.at != s.home && Array.IndexOf(origin.targets, GameData.Ports[s.at].id) >= 0)
                    RaiderBrain.Raid(this, s, s.at);
                return;
            }
            Berth(s);
        }

        /// <summary>Real metres along her passage where she stops: its end, or - when the landing ahead is full - her
        /// place in its anchorage (the first free one), which she then takes.</summary>
        double AnchorDistance(SimShip s, out int slot)
        {
            slot = -1;
            if (s.dest < 0 || s.dest == s.home || s.raider || visitors[s.dest] < berths[s.dest])
                return s.total;
            slot = FreeAnchorSlot(s.dest);
            return Math.Max(s.done, s.total - AnchorBack(s, slot));          // (already past it: where she is)
        }

        double AnchorBack(SimShip s, int slot) =>
            sea.holdBackKm * 1000.0 + slot * sea.spacingLengths * s.cls.length * SimUnits.Across;

        void Berth(SimShip s)
        {
            if (s.state == ShipState.Anchored)
                stats.anchoredHours += Hours - s.anchoredSince;
            s.pos = s.path[s.path.Count - 1];
            s.at = s.dest;
            s.dest = -1;
            s.anchorSlot = -1;
            s.state = ShipState.InPort;
            s.readyDay = Day + s.rng.Range(rules.dwellMin, rules.dwellMax);
            if (s.at != s.home)
                visitors[s.at]++;
        }

        /// <summary>Anchored off: she goes in when there's a berth and she's first in the queue (who came first, then
        /// the lower number) - or, after days at anchor, anyway, moored alongside another.</summary>
        void TakeBerth(SimShip s)
        {
            s.prevPos = s.pos;
            if (Hours - s.anchoredSince >= sea.maxAnchorDays * 24.0)
            {
                Berth(s);
                return;
            }
            if (visitors[s.dest] >= berths[s.dest])
                return;
            foreach (var o in ships)
                if (o != s && o.state == ShipState.Anchored && o.dest == s.dest &&
                    (o.anchoredSince < s.anchoredSince || (o.anchoredSince == s.anchoredSince && o.index < s.index)))
                    return;
            Berth(s);
        }

        int FreeAnchorSlot(int port)
        {
            for (int slot = 0; ; slot++)
            {
                bool taken = false;
                foreach (var o in ships)
                    if (o.state == ShipState.Anchored && o.dest == port && o.anchorSlot == slot)
                    {
                        taken = true;
                        break;
                    }
                if (!taken)
                    return slot;
            }
        }

        /// <summary>Her place in the anchorage: back along her own way in from the berth, one place per slot, to her
        /// side of the lane (along the bank, on a river).</summary>
        WorldXZ AnchorPlace(SimShip s, int slot)
        {
            double target = Math.Max(s.done, s.total - AnchorBack(s, slot));
            double done = 0.0;
            for (int k = 0; k + 1 < s.path.Count; k++)
            {
                double len = WorldXZ.Distance(s.path[k], s.path[k + 1]);
                if (done + len >= target || k + 2 >= s.path.Count)
                {
                    double along = Math.Min(len, Math.Max(0.0, target - done));
                    var p = Place(s, k, along, s.total * 0.5, 0.0);           // (her full side: not eased)
                    var c = Place(s, k, along, 0.0, 0.0);                     // the line itself
                    return new WorldXZ(c.x + (p.x - c.x) * sea.moorSide, c.z + (p.z - c.z) * sea.moorSide);
                }
                done += len;
            }
            return s.path[s.path.Count - 1];
        }

        /// <summary>The ports she'd go to from where she is: in the year, within her kind's reach of her home.</summary>
        public List<int> Reach(SimShip s)
        {
            int key = s.home * 64 + (s.cls.Uses("trade_long") ? 2 : s.cls.sea || s.cls.coast ? 1 : 0);
            if (reachCache.TryGetValue(key, out var list))
                return list;
            list = new List<int>();
            double reach = s.cls.Uses("trade_long") ? rules.seaReachKm : s.cls.sea || s.cls.coast ? rules.coastReachKm : rules.riverReachKm;
            int year = Date.year;
            var home = GameData.Ports[s.home];
            var (hx, hz) = WorldFrame.ToWorld(home.lat, home.lon);
            foreach (var p in GameData.Ports)
            {
                if (!p.Exists(year))
                    continue;
                var (px, pz) = WorldFrame.ToWorld(p.lat, p.lon);
                if (Math.Sqrt((px - hx) * (px - hx) + (pz - hz) * (pz - hz)) / 1000.0 <= reach)
                    list.Add(p.index);
            }
            reachCache[key] = list;
            return list;
        }

        /// <summary>Game days a passage takes her at an ordinary speed (for planning: spoilage and the crew's keep).</summary>
        public double DaysTo(SimShip s, int from, int to)
        {
            long key = ((long)from * 4096 + to) * 64 + (s.cls.sea ? 1 : 0) + (s.cls.coast ? 2 : 0) + (s.cls.river ? 4 : 0) + (s.cls.length <= 12f ? 8 : 0);
            if (!kmCache.TryGetValue(key, out double km))
            {
                var path = new List<WorldXZ>();
                km = routes.Passage(from, to, s.cls, path, null, out double k) ? k : -1.0;
                kmCache[key] = km;
            }
            if (km <= 0.0)
                return -1.0;
            double knots = s.cls.hasSail ? Math.Max(2.0, s.cls.sailKn * 0.65) : Math.Max(1.0, s.cls.oarsKn * 0.6);
            return km * 1000.0 / (SimUnits.RealMetresPerGameHour(knots * SimUnits.KnotMs) * 24.0);
        }

        // ------------------------------------------------------------------------------------------ saving

        /// <summary>The whole world as plain data (a saved game): the clock, every ship (her dice too, so the world goes
        /// on as it would have), every waterfront's prices and news, the raided ports. Price lists and news are shared
        /// between ships, so each is kept once and referred to by number.</summary>
        public Dictionary<string, object> Capture()
        {
            var snapIds = new Dictionary<PriceSnapshot, int>();
            var snaps = new List<object>();
            var newsIds = new Dictionary<NewsItem, int>();
            var newsList = new List<object>();
            int Snap(PriceSnapshot p)
            {
                if (!snapIds.TryGetValue(p, out int id))
                {
                    id = snaps.Count;
                    snapIds[p] = id;
                    snaps.Add(new List<object> { p.port, p.day, Json.Floats(p.asks), Json.Floats(p.pays), p.seenBy });
                }
                return id;
            }
            int News(NewsItem n)
            {
                if (!newsIds.TryGetValue(n, out int id))
                {
                    id = newsList.Count;
                    newsIds[n] = id;
                    newsList.Add(new List<object> { n.id, n.kind, n.text, n.port, n.day, n.refId,
                                                    double.IsNaN(n.x) ? null : (object)n.x, double.IsNaN(n.z) ? null : (object)n.z });
                }
                return id;
            }
            Dictionary<string, object> Know(Knowledge k)
            {
                var s = new List<object>();
                foreach (var p in k.All)
                    s.Add(Snap(p));
                var n = new List<object>();
                foreach (var (item, learned, from) in k.News)
                    n.Add(new List<object> { News(item), learned, from });
                return new Dictionary<string, object> { ["s"] = s, ["n"] = n };
            }
            var boardData = new List<object>();
            foreach (var b in boards)
                boardData.Add(Know(b));
            var shipData = new List<object>();
            foreach (var s in ships)
            {
                var path = new List<object>();
                foreach (var p in s.path)
                {
                    path.Add(p.x);
                    path.Add(p.z);
                }
                shipData.Add(new Dictionary<string, object>
                {
                    ["id"] = s.id, ["cls"] = s.cls.id, ["home"] = s.home, ["region"] = s.homeRegion, ["state"] = (int)s.state, ["at"] = s.at,
                    ["dest"] = s.dest, ["ready"] = s.readyDay, ["purse"] = s.purse.Capture(), ["hold"] = s.hold.Capture(),
                    ["capacity"] = (double)s.hold.capacity, ["knows"] = Know(s.knows),
                    ["rng"] = new List<object> { s.rng.state.ToString(), s.rng.inc.ToString() },
                    ["path"] = path, ["kinds"] = s.kinds.ConvertAll(k => (object)(int)k), ["leg"] = s.leg, ["along"] = s.along,
                    ["done"] = s.done, ["total"] = s.total, ["pos"] = new List<object> { s.pos.x, s.pos.z },
                    ["prev"] = new List<object> { s.prevPos.x, s.prevPos.z }, ["heading"] = s.heading, ["km"] = s.passageKm,
                    ["departed"] = s.departedDay, ["voyages"] = s.voyages, ["open"] = s.voyageOpen, ["start_worth"] = s.startWorth,
                    ["cost"] = s.voyageCost, ["revenue"] = s.voyageRevenue, ["profit"] = s.profit, ["wages"] = s.wages,
                    ["owed"] = s.owedWages, ["cargo"] = s.cargoNote, ["go"] = s.go, ["stand_out"] = s.standOut, ["hold_out"] = s.holdOut,
                    ["lying_up"] = s.lyingUp, ["wait_since"] = s.waitSince, ["anchored_since"] = s.anchoredSince, ["broke_since"] = s.brokeSince,
                    ["anchor_slot"] = s.anchorSlot, ["wind_bound"] = s.windBound, ["raider"] = s.raider, ["origin"] = s.origin,
                    ["band_ships"] = s.bandShips, ["men"] = s.men, ["season"] = s.seasonYear, ["raids"] = s.raidsThisSeason,
                    ["taken_day"] = s.lastTakenDay,
                });
            }
            var raided = new List<object>();
            foreach (double r in raidedUntil)
                raided.Add(r);
            return new Dictionary<string, object>
            {
                ["hours"] = Hours, ["last_day"] = lastDay, ["raid_year"] = raidYear, ["news_count"] = newsCount, ["raided"] = raided,
                ["snapshots"] = snaps, ["news"] = newsList, ["boards"] = boardData, ["ships"] = shipData,
            };
        }

        /// <summary>The world as saved (the same tables loaded: classes, ports, goods).</summary>
        public void Restore(Dictionary<string, object> d)
        {
            if (d == null)
                return;
            Hours = Json.Num(d, "hours", Hours);
            lastDay = (int)Json.Num(d, "last_day", lastDay);
            raidYear = (int)Json.Num(d, "raid_year", raidYear);
            newsCount = (int)Json.Num(d, "news_count", newsCount);
            var raided = Json.Arr(d, "raided") ?? new List<object>();
            for (int i = 0; i < raidedUntil.Length && i < raided.Count; i++)
                raidedUntil[i] = raided[i] is double r ? r : 0.0;
            var snaps = new List<PriceSnapshot>();
            foreach (var x in Json.Arr(d, "snapshots") ?? new List<object>())
                if (x is List<object> l && l.Count >= 4)
                    snaps.Add(new PriceSnapshot { port = (int)(double)l[0], day = (double)l[1], asks = Json.Floats(l[2] as string), pays = Json.Floats(l[3] as string),
                                                  seenBy = l.Count > 4 ? l[4] as string : null });
            var items = new List<NewsItem>();
            foreach (var x in Json.Arr(d, "news") ?? new List<object>())
                if (x is List<object> l && l.Count >= 5)
                    items.Add(new NewsItem { id = l[0] as string, kind = l[1] as string, text = l[2] as string, port = (int)(double)l[3], day = (double)l[4],
                                             refId = l.Count > 5 ? l[5] as string : null,
                                             x = l.Count > 6 && l[6] is double nx ? nx : double.NaN, z = l.Count > 7 && l[7] is double nz ? nz : double.NaN });
            void Fill(Knowledge k, object data)
            {
                k.Clear();
                foreach (var x in Json.Arr(data, "s") ?? new List<object>())
                    if (x is double i && i >= 0 && i < snaps.Count)
                        k.Learn(snaps[(int)i]);
                foreach (var x in Json.Arr(data, "n") ?? new List<object>())
                    if (x is List<object> l && l.Count >= 3 && l[0] is double i && i >= 0 && i < items.Count)
                        k.Hear(items[(int)i], (double)l[1], l[2] as string);
            }
            var boardData = Json.Arr(d, "boards") ?? new List<object>();
            for (int i = 0; i < boards.Length; i++)
                Fill(boards[i], i < boardData.Count ? boardData[i] : null);
            ships.Clear();
            foreach (var x in Json.Arr(d, "ships") ?? new List<object>())
            {
                if (!data.classes.TryGetValue(Json.Str(x, "cls"), out var cls))
                    continue;
                var rng = Json.Arr(x, "rng");
                var s = new SimShip
                {
                    index = ships.Count, id = Json.Str(x, "id"), cls = cls, home = (int)Json.Num(x, "home"), homeRegion = Json.Str(x, "region", null),
                    state = (ShipState)(int)Json.Num(x, "state"), at = (int)Json.Num(x, "at"), dest = (int)Json.Num(x, "dest", -1),
                    readyDay = Json.Num(x, "ready"), purse = new Purse(), hold = new Hold { capacity = (float)Json.Num(x, "capacity", cls.CargoKg) },
                    rng = new SimRandom { state = rng != null ? ulong.Parse((string)rng[0]) : 1UL, inc = rng != null ? ulong.Parse((string)rng[1]) : 1UL },
                    leg = (int)Json.Num(x, "leg"), along = Json.Num(x, "along"), done = Json.Num(x, "done"), total = Json.Num(x, "total"),
                    heading = Json.Num(x, "heading"), passageKm = Json.Num(x, "km"), departedDay = Json.Num(x, "departed"),
                    voyages = (int)Json.Num(x, "voyages"), voyageOpen = Json.Bool(x, "open"), startWorth = Json.Num(x, "start_worth"),
                    voyageCost = Json.Num(x, "cost"), voyageRevenue = Json.Num(x, "revenue"), profit = Json.Num(x, "profit"),
                    wages = Json.Num(x, "wages"), owedWages = Json.Num(x, "owed"), cargoNote = Json.Str(x, "cargo"), go = Json.Num(x, "go"),
                    standOut = Json.Num(x, "stand_out"), holdOut = (int)Json.Num(x, "hold_out"), lyingUp = Json.Bool(x, "lying_up"),
                    waitSince = Json.Num(x, "wait_since"), anchoredSince = Json.Num(x, "anchored_since"), brokeSince = Json.Num(x, "broke_since", -1.0), anchorSlot = (int)Json.Num(x, "anchor_slot", -1),
                    windBound = Json.Bool(x, "wind_bound"), raider = Json.Bool(x, "raider"), origin = Json.Str(x, "origin", null),
                    bandShips = (int)Json.Num(x, "band_ships", 1), men = (int)Json.Num(x, "men"), seasonYear = (int)Json.Num(x, "season"),
                    raidsThisSeason = (int)Json.Num(x, "raids"), lastTakenDay = Json.Num(x, "taken_day", double.MinValue),
                };
                s.purse.Restore(Json.Obj(x, "purse"));
                s.hold.Restore(Json.Arr(x, "hold"));
                Fill(s.knows, Json.Obj(x, "knows"));
                var path = Json.Arr(x, "path") ?? new List<object>();
                for (int i = 0; i + 1 < path.Count; i += 2)
                    s.path.Add(new WorldXZ((double)path[i], (double)path[i + 1]));
                foreach (var k in Json.Arr(x, "kinds") ?? new List<object>())
                    s.kinds.Add((byte)(double)k);
                var pos = Json.Arr(x, "pos");
                var prev = Json.Arr(x, "prev");
                if (pos != null && pos.Count >= 2)
                    s.pos = new WorldXZ((double)pos[0], (double)pos[1]);
                s.prevPos = prev != null && prev.Count >= 2 ? new WorldXZ((double)prev[0], (double)prev[1]) : s.pos;
                ships.Add(s);
            }
            snapshot = weather.Snapshot(epochHours + Hours);
        }

        /// <summary>A number for the whole state (to check that a seed plays the same twice).</summary>
        public long StateHash()
        {
            unchecked
            {
                long h = 1469598103934665603L;
                foreach (var s in ships)
                {
                    h = (h ^ (long)Math.Round(s.pos.x)) * 1099511628211L;
                    h = (h ^ (long)Math.Round(s.pos.z)) * 1099511628211L;
                    h = (h ^ s.at) * 1099511628211L;
                    h = (h ^ (long)Math.Round(s.purse.Silver(TradeRules.Current) * 10)) * 1099511628211L;
                }
                return h;
            }
        }
    }
}
