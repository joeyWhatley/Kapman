using System;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.Sailing;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME - the root of the game's own logic, on the "--- GAME ---" object. It holds the tables (loaded once
    /// into <see cref="GameData"/> and <see cref="TradeRules"/>), starts the merchant's trade (<see cref="Trade"/>:
    /// purse, hold, ledger, debt), and finds the clock and the ship for the graphs. What the game DOES lives in the
    /// graphs on this object and its children (Assets/Kapman/Game/Graphs; the "00 Game map" graph shows them all).
    /// It also tells the graphs when a game hour and a game day have passed (On game hour, On game day).
    ///
    /// ported 4 Oct: the clock is the level's (Kapman.Levels.LevelClock), the ship is the level's ShipMotion wrapped in
    /// the SailingShip shim, the world frame is set by LevelFrame, and the tables fill themselves from
    /// Assets/Kapman/Game/Data when nothing is assigned (GameFiles). The rebuild is a chain of level scenes: the game's
    /// state is static and lives on, so a KapmanGame in the next level carries on (the world's ships, the hour count and
    /// the clock's date are carried over; the merchant isn't started again).
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public class KapmanGame : MonoBehaviour
    {
        [Header("Tables (empty: read from Assets/Kapman/Game/Data by name)")]
        [Tooltip("Data/ports_793.json: every trading place.")]
        public TextAsset ports;
        [Tooltip("Data/goods_793.json: every good of the time, where it's made and wanted.")]
        public TextAsset goods;
        [Tooltip("Data/trade_rules.json: the coins and money zones, tolls, laws, how markets answer supply and demand, and what a merchant starts with.")]
        public TextAsset tradeRules;
        [Tooltip("Data/wording_793.json: how each people of 793 names what you do in port and in the market.")]
        public TextAsset wording;
        [Tooltip("Data/chronicle_793.json: real, dated events of the time (raids, wars, laws, deaths, famines...), with their sources.")]
        public TextAsset chronicle;
        [Tooltip("Data/shocks_793.json: what the chronicle's famines, raids and burnings do to the markets.")]
        public TextAsset shocks;
        [Tooltip("Data/ship_classes_793.json: every kind of craft of the time (the world's ships).")]
        public TextAsset shipClasses;
        [Tooltip("Data/traffic_793.json: how many traders each port sends out, in what, and the sailing seasons.")]
        public TextAsset traffic;
        [Tooltip("Data/voyage_rules.json: the crew's keep and pay, their stores, and what the sea does to them.")]
        public TextAsset voyageRules;
        [Tooltip("Data/seamanship_793.json: how the world's ships keep clear of each other, wait for a fair wind and queue for a berth.")]
        public TextAsset seamanship;
        [Tooltip("Data/raiders_793.json: the raiders of the time - who, from where, when, how many, what they strike.")]
        public TextAsset raiders;
        [Tooltip("Data/patrols_793.json: the king's men - who searches ships where, how hard, and from when at sea.")]
        public TextAsset patrols;
        [Tooltip("Data/fairs_793.json: the fairs and seasonal markets of the time.")]
        public TextAsset fairs;
        [Tooltip("Data/events_793.json: happenings at sea and in port, with their options and effects.")]
        public TextAsset events;
        [Tooltip("Data/reputation_793.json: how each people weighs each kind of deed, and what standing does.")]
        public TextAsset reputation;
        [Tooltip("Data/commissions_793.json: the orders each kind of place posts, and what they pay.")]
        public TextAsset commissions;
        [Tooltip("Data/knowing_793.json: how sure each way of learning is (seen, told, talk, rumour), how fast knowledge goes stale, how far told prices can be off.")]
        public TextAsset knowing;
        [Tooltip("Data/texts_793.json: real texts of the time, in their own tongue with a free translation, for the chest.")]
        public TextAsset texts;
        [Tooltip("Data/riddles_793.json: the riddles, the things that answer them, where they're had and what they win.")]
        public TextAsset riddles;
        [Tooltip("Data/runes_793.json: the Younger Futhark and real rune inscriptions.")]
        public TextAsset runes;
        [Tooltip("Data/voyages_793.json: the old texts' sailing directions, leg by leg, laid out on the map with the dividers.")]
        public TextAsset voyages;
        [Tooltip("Data/hoards_793.json: real hoards, their landmarks and their clues as measures, for hoard hunts.")]
        public TextAsset hoards;
        [Tooltip("Data/boat_rules.json: the boat's wear, leaks, bailing, weariness and her odds in a gale (none: the code's own numbers).")]
        public TextAsset boatRules;
        [Tooltip("The world's seed: the same seed plays the same world.")]
        public int worldSeed = 793;
        [Tooltip("The sea lanes (Bake/lanes): the routes on water the world's ships sail by. Missing: straight lines.")]
        public string lanesFile = "G:/kapman_unity/data/baked/world/lanes.bin";
        [Tooltip("The world's ships keep clear of each other and of yours (Seamanship: give way, stand out, wait at a busy " +
                 "berth). Off, they pass through each other (the user, 2026-10-03: \"just make it so the ships phase through " +
                 "each other ATM\").")]
        public bool shipsKeepClear;

        [Header("The world")]
        [Tooltip("The level's clock (found if empty). ported 4 Oct: was the first build's GameClock.")]
        public LevelClock clock;
        [Tooltip("The player's ship (found if empty: the SailingShip shim is put on the level's ShipMotion.Player).")]
        public SailingShip ship;

        public static KapmanGame Current { get; private set; }

        /// <summary>A whole game hour has passed (its count since the start). The graphs' "On game hour".</summary>
        public static event Action<double> HourPassed;
        /// <summary>A whole game day has passed (its count since the start). The graphs' "On game day".</summary>
        public static event Action<int> DayPassed;
        /// <summary>A game minute has passed (its count since the start): the tick ladder's finest step (4 Oct).</summary>
        public static event Action<long> MinutePassed;
        /// <summary>The year has turned (the new year), at the Julian turn.</summary>
        public static event Action<int> YearPassed;
        /// <summary>Time was jumped (the keys, a saved game): raised before the ticks in between are replayed.</summary>
        public static event Action<double, double> Jumped;
        /// <summary>Time went backwards: nothing is ticked; the systems re-read the clock.</summary>
        public static event Action<double, double> Reversed;

        public int Year => clock != null ? clock.Year : 793;
        public double Hours => clock != null ? clock.GameHours : 0.0;
        /// <summary>Game days since the start (with the hours as a fraction).</summary>
        public double Day => Hours / 24.0;
        /// <summary>The month, 1-12 (the Julian calendar's).</summary>
        public int Month => clock != null ? clock.Date.month : 6;
        /// <summary>The season's name for the day of the year (as the first build's clock had it).</summary>
        public string Season => clock == null ? "" : SeasonOf(clock.DayOfYear);
        /// <summary>The date and time as "24 December 793, 14:05".</summary>
        public string Date => clock != null ? clock.Text : "";
        public Vector3 ShipPosition => ship != null ? ship.transform.position : Vector3.zero;

        public static string SeasonOf(int dayOfYear) =>
            dayOfYear < 80 || dayOfYear >= 355 ? "winter" : dayOfYear < 172 ? "spring" : dayOfYear < 266 ? "summer" : "autumn";

        /// <summary>The world's other ships (the traders), simulated; null until the tables are set. The graphs tick it
        /// ("Sim - the world": On game hour, then Sim: Hour). Static underneath: it lives on from level to level.</summary>
        public Kapman.Game.Sim.WorldSim World => world;

        static Kapman.Game.Sim.WorldSim world;
        static double lastHours = -1.0;                            // the tick ladder's cursor (4 Oct: was an hour count)
        static Kapman.Game.Sim.TickLadder ladder;
        static readonly Ticks ticks = new Ticks();

        /// <summary>What each tick does: the ladder raises these in order, exactly once, however the clock moved.</summary>
        sealed class Ticks : Kapman.Game.Sim.ITickSink
        {
            public void Minute(long minute) => MinutePassed?.Invoke(minute);
            public void Hour(long hour)
            {
                HourPassed?.Invoke(hour);
                BoatWatch.Hour(hour);                                      // (after the world's ships have had their hour: the same weather)
                var g = Current;
                if (g == null) return;
                g.NoteStanding();
                Chronicle.Advance(g.clock.Date);
            }
            public void Day(int day)
            {
                DayPassed?.Invoke(day);
                BoatWatch.Day(day);
                GameEvents.Daily(day);
                Commissions.Daily(day);
                Reputation.Daily();                                      // (deeds' news arriving in far regions)
                if (Current != null)
                    Shocks.Daily(Current.Day);                           // held famines stay dear
                // (the old price notes are thinned as the days turn. Saving used to do it, and so changed the game it saved)
                if (Current != null && Trade.Started && Known.Rules != null)
                    Trade.Ledger.Thin(Current.Day, Known.Rules.keepAllDays, Known.Rules.seasonAfterYears);
            }
            public void Year(int year) => YearPassed?.Invoke(year);
            public void Jumped(double from, double to) => KapmanGame.Jumped?.Invoke(from, to);
            public void Reversed(double from, double to) => KapmanGame.Reversed?.Invoke(from, to);
        }
        // carried from one level's clock to the next (the chain's scenes each have a clock of their own)
        static bool begun;
        static int firstYear = 793, firstStartDay = 172;
        static double carriedHours = -1.0;

        void Awake()
        {
            Current = this;
            Kapman.Levels.DevHud.GameLine = MarketLine;
            LevelFrame.Ensure();                                             // the level's place in the world, before anything reads positions
            FillTables();
            GameData.LoadPorts(ports);
            GameData.LoadGoods(goods);
            TradeRules.Load(tradeRules);
            Wording.Load(wording);
            Chronicle.Load(chronicle);
            Shocks.Load(shocks);
            Seasons.Load(GameFiles.Find("seasons_793"));                     // the harvest, vintage and herring curves
            TradeYear.Load(GameFiles.Find("trade_year_793"));                // the trading year by region (the word on the item reads it)
            if (clock != null)
                Seasons.SetStart(clock.year, clock.startDay);                // game day 0 is the clock's start
            Kapman.Game.Sim.Patrols.Rules = Kapman.Game.Sim.PatrolRules.Load(patrols);
            GameEvents.Load(events);
            Reputation.Load(reputation);
            Commissions.Load(commissions);
            Known.Load(knowing);
            ChestData.Load(texts, riddles, runes, voyages);
            Hoards.Load(hoards);
            if (clock == null)
                clock = LevelClock.Current != null ? LevelClock.Current : FindAnyObjectByType<LevelClock>();
            if (clock == null)
                Debug.LogWarning("Kapman: no LevelClock in this scene: the game's hours don't run.");
            FindShip();
            if (begun)
            {
                // the next level of the chain: the same game goes on, on this level's clock set to where the last one was
                if (clock != null && carriedHours >= 0.0)
                {
                    clock.year = firstYear;
                    clock.startDay = firstStartDay;
                    clock.GameHours = carriedHours;
                }
                return;
            }
            begun = true;
            if (clock != null)
            {
                firstYear = clock.year;
                firstStartDay = clock.startDay;
            }
            // a new merchant: the starting purse and debt, an empty hold the size of the ship's
            // the crew and their stores; the water casks take their room in the hold
            var voyage = VoyageRules.Load(voyageRules);
            Voyage.Begin(voyage);
            Boat.Begin(BoatRules.Load(boatRules));                        // (her hull, her sail, the water in her, the men's shares)
            float cargo = ship != null && ship.shipClass != null ? ship.shipClass.cargoKg : 900f;
            float room = Mathf.Max(100f, cargo - (float)(voyage.WaterCapacityL * voyage.kgPerLitre));
            if (clock != null)
                Trade.Begin(TradeRules.Current, room, Day, clock.year, clock.startDay);   // (the debt falls due on a day: Michaelmas)
            else
                Trade.Begin(TradeRules.Current, room, Day);
            // the chronicle up to the start is the backstory; from here on, each event comes on its day
            if (clock != null)
                Chronicle.StartAt(Kapman.World.Calendar.At(clock.year, clock.startDay, clock.startHour));
        }

        /// <summary>Any table left empty on the component is read from Assets/Kapman/Game/Data by name.</summary>
        void FillTables()
        {
            ports = ports != null ? ports : GameFiles.Find("ports_793");
            goods = goods != null ? goods : GameFiles.Find("goods_793");
            tradeRules = tradeRules != null ? tradeRules : GameFiles.Find("trade_rules");
            wording = wording != null ? wording : GameFiles.Find("wording_793");
            chronicle = chronicle != null ? chronicle : GameFiles.Find("chronicle_793");
            shocks = shocks != null ? shocks : GameFiles.Find("shocks_793");
            shipClasses = shipClasses != null ? shipClasses : GameFiles.Find("ship_classes_793");
            traffic = traffic != null ? traffic : GameFiles.Find("traffic_793");
            voyageRules = voyageRules != null ? voyageRules : GameFiles.Find("voyage_rules");
            boatRules = boatRules != null ? boatRules : GameFiles.Find("boat_rules");
            seamanship = seamanship != null ? seamanship : GameFiles.Find("seamanship_793");
            raiders = raiders != null ? raiders : GameFiles.Find("raiders_793");
            patrols = patrols != null ? patrols : GameFiles.Find("patrols_793");
            fairs = fairs != null ? fairs : GameFiles.Find("fairs_793");
            events = events != null ? events : GameFiles.Find("events_793");
            reputation = reputation != null ? reputation : GameFiles.Find("reputation_793");
            commissions = commissions != null ? commissions : GameFiles.Find("commissions_793");
            knowing = knowing != null ? knowing : GameFiles.Find("knowing_793");
            texts = texts != null ? texts : GameFiles.Find("texts_793");
            riddles = riddles != null ? riddles : GameFiles.Find("riddles_793");
            runes = runes != null ? runes : GameFiles.Find("runes_793");
            voyages = voyages != null ? voyages : GameFiles.Find("voyages_793");
            hoards = hoards != null ? hoards : GameFiles.Find("hoards_793");
        }

        /// <summary>The player's ship: a SailingShip in the scene, else the shim put on the level's player ShipMotion.</summary>
        void FindShip()
        {
            if (ship != null)
                return;
            ship = FindAnyObjectByType<SailingShip>();
            if (ship != null)
                return;
            var motion = ShipMotion.Player != null ? ShipMotion.Player : FindAnyObjectByType<ShipMotion>();
            if (motion != null)
                ship = motion.gameObject.AddComponent<SailingShip>();
            else
                Debug.LogWarning("Kapman: no ship in this scene (no ShipMotion): the game runs without her.");
        }

        float nextReveal;

        /// <summary>
        /// WHAT SHE KNOWS OF THE CHART ON THE FIRST MORNING (the user, 9 Oct 2026: "narrow the starting fog area, just
        /// Dorestead and the closest port"): her home port, as far as she sees from it, and the port nearest it by
        /// water. Everything else is fog. (It was 160 km round home: the whole Rhine delta, the Almere and the Frisian
        /// coast.) The waters and landmarks that lie inside what she knows are known to her too, and nothing is said of
        /// them (Discoveries). Called again when a new game's people start at another port: what was lifted round the
        /// first one is put back.
        /// </summary>
        public static void KnowStart(PortDef home)
        {
            if (home == null || !Exploration.Ready)
                return;
            var game = Current;
            int year = game != null ? game.Year : 793;
            Exploration.Forget();
            Discoveries.Forget();
            var (hx, hz) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            Exploration.Reveal(hx, hz, Exploration.Sight);
            var near = NearestPort(home, year);
            if (near != null)
            {
                var (nx, nz) = Kapman.World.WorldFrame.ToWorld(near.lat, near.lon);
                Exploration.Reveal(nx, nz, Exploration.NearSight);
            }
            Exploration.Apply();
            Discoveries.KnowAtStart(year, new Kapman.Game.Sim.WorldXZ(hx, hz));
            Sightings.Forget();
            Sightings.KnowAtStart(year);
            StartKnows = (home.id, near != null ? near.id : "");
            Debug.Log($"Kapman: on the first morning she knows {home.name}" + (near != null ? $" and {near.name}, the port nearest it" : "") +
                      $", and {Discoveries.Count} of the {Discoveries.Total(year)} waters and landmarks.");
        }

        /// <summary>The two ports the first morning's chart was opened round (KnowStart): home, and the nearest to it.</summary>
        public static (string home, string near) StartKnows { get; private set; } = ("", "");

        /// <summary>The port nearest another by water (of the six nearest as the crow flies; where no way can be
        /// reckoned, the nearest as the crow flies). Null if there is no other.</summary>
        public static PortDef NearestPort(PortDef home, int year)
        {
            if (home == null)
                return null;
            var (hx, hz) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            var from = new Kapman.Game.Sim.WorldXZ(hx, hz);
            var near = new List<(double m, PortDef port)>();
            foreach (var def in GameData.Ports)
            {
                if (def.id == home.id || !def.Exists(year))
                    continue;
                var (x, z) = Kapman.World.WorldFrame.ToWorld(def.lat, def.lon);
                near.Add((Kapman.Game.Sim.WorldXZ.Distance(from, new Kapman.Game.Sim.WorldXZ(x, z)), def));
            }
            near.Sort((a, b) => a.m != b.m ? a.m.CompareTo(b.m) : string.CompareOrdinal(a.port.id, b.port.id));
            PortDef best = null;
            double bestKm = double.MaxValue;
            var line = new List<Kapman.Game.Sim.WorldXZ>();
            for (int i = 0; i < near.Count && i < 6; i++)
            {
                bool way;
                double km;
                try
                {
                    way = Course.Plan(near[i].port.id, from, home.id, line, out km);
                }
                catch (Exception)
                {
                    way = false;                                           // (no lanes to reckon by: as the crow flies, below)
                    km = 0.0;
                }
                if (way && km < bestKm)
                {
                    best = near[i].port;
                    bestKm = km;
                }
            }
            return best ?? (near.Count > 0 ? near[0].port : null);
        }

        /// <summary>The fog of war: made once the world's bounds are known (the world bake), the first morning's part of it lifted (KnowStart), then lifted round the ship.</summary>
        void Explore()
        {
            if (!Exploration.Ready)
            {
                // ported 4 Oct: the first build took the bounds off the land it was streaming; the rebuild's levels are
                // terrains, so the bounds come from the world bake the chart reads (MapLand)
                var land = MapLand.Shared();
                if (land == null)
                    return;
                var b = land.Bounds;
                Exploration.Begin(b.west, b.north, b.east, b.south);
                var chart = ChartWorld.Current;
                KnowStart(GameData.Port(chart != null && !string.IsNullOrEmpty(chart.homePort) ? chart.homePort : "dorestad") ?? GameData.Port("dorestad"));
            }
            if (ship != null && Time.unscaledTime >= nextReveal)
            {
                nextReveal = Time.unscaledTime + 1f;
                var w = Kapman.World.FloatingOrigin.ToWorld(ship.transform.position);
                Exploration.See(w.x, w.z, Exploration.Sight);
            }
        }

        /// <summary>Makes the world's ships once the clock and the weather are running (after every Start).</summary>
        void MakeWorld()
        {
            if (world != null)
            {
                Listen();
                return;
            }
            if (shipClasses == null || traffic == null || clock == null)
                return;
            // THE PRE-RUN (ruling 14 of 7 Oct 2026): the world's traders have been at work since the rules' world_from
            // (1 April), so no market is untouched on the player's first morning. The markets and the ships are made as
            // of that day and stepped to the clock's start with nobody watching. Day 0 stays the clock's start: the
            // world's hours and the markets' days run below 0 meanwhile. (The pre-run ends on the clock's whole hour, so
            // one seed gives one first morning whatever the first frame took.)
            int back = preRun ? TradeRules.Current.PreRunDays(clock.year, clock.startDay) : 0;
            double to = back > 0 ? Math.Floor(clock.GameHours) : clock.GameHours;
            double from = to - back * 24.0;
            BuildWorld(from, back > 0);
            // the chronicle's events are the world's news, from where they happened; its raids count as struck
            Listen();
            replaying = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // One seed and one set of tables give one first morning, so it is reckoned once and kept (FirstMorning):
            // stepping thirty days of every ship of the world takes seconds, and a new game should not wait for it
            string key = back > 0 ? FirstMorning.Key(this, to, back) : null;
            bool kept = back > 0 && FirstMorning.TryRestore(key, world, Year, to);
            if (back > 0 && !kept && FirstMorning.Spoiled)
                BuildWorld(from, true);                                  // (a kept morning that broke half way in: start clean)
            if (kept)
                Debug.Log($"Kapman: the world's traders have been at work for {back} days (the first morning as kept, read in {watch.ElapsedMilliseconds} ms); markets {Markets.Hash():X16}");
            else
            {
                // what happened in the weeks before the world's first day is still felt (the graphs are listening by now)
                // (and the states begun before that and not over yet: the great famine of 793, from January to the harvest of 794)
                foreach (var e in Chronicle.Ongoing(world.Date, 60))
                    Chronicle.Replay(e);
                foreach (var e in Chronicle.Recent(world.Date, 60))
                    Chronicle.Replay(e);
                if (back > 0)
                {
                    // the weeks to the clock's start: each day's own events on their day, and the held shocks kept dear
                    int stepped = world.RunTo(to, day =>
                    {
                        Shocks.Daily(day);
                        foreach (var e in Chronicle.Recent(world.Date, 1))
                            Chronicle.Replay(e);
                    });
                    Debug.Log($"Kapman: the world's traders have been at work for {back} days ({stepped} hours stepped in {watch.ElapsedMilliseconds} ms); markets {Markets.Hash():X16}");
                    FirstMorning.Keep(key, world);
                }
            }
            if (back > 0)
                PreRunMs = Math.Max(1, watch.ElapsedMilliseconds);
            replaying = false;
            // and the waterfronts know what they would have by now: prices near them, news that's had time to come
            world.SeedKnowledge();
            KnowHome();
        }

        /// <summary>The markets and the world's ships made as of `from` game hours (below 0: before the clock's start).</summary>
        void BuildWorld(double from, bool fresh)
        {
            var local = Kapman.Weather.LocalWeather.Active;
            var weather = local != null && local.Model != null ? local.Model : new Kapman.Weather.WeatherModel(793);
            if (fresh && Markets.All.Count > 0)
                Markets.Clear();                                         // (a market opened already would stay as of today)
            Markets.OpenAll(Year, from / 24.0);
            Shocks.Clear();
            world = new Kapman.Game.Sim.WorldSim(Kapman.Game.Sim.SimData.Load(shipClasses, traffic), new Kapman.Game.Sim.SimRules(),
                                                 Kapman.Game.Sim.SimRoutes.Load(lanesFile), weather, worldSeed, clock.year, clock.startDay, from,
                                                 Kapman.Game.Sim.SeamanshipRules.Load(seamanship),
                                                 raiders != null ? Kapman.Game.Sim.RaiderRules.Load(raiders) : null);
            world.fairs = Kapman.Game.Sim.Fairs.Load(fairs);
            world.keepClear = shipsKeepClear;
            world.stats.csvEvery = world.stats.sampleEvery = 0;          // (the price logs are for the test runs' reports)
            // the merchant's ship, for the world's ships to keep clear of: where she is, and an hour of her way ahead
            world.playerNow = () =>
            {
                var game = Current;
                if (game == null || game.ship == null)
                    return null;
                var w = Kapman.World.FloatingOrigin.ToWorld(game.ship.transform.position);
                double perHour = 60.0 * Kapman.World.FloatingOrigin.HorizontalScale;     // game m/s -> real m a game hour
                var v = game.ship.anchored ? Vector3.zero : game.ship.GroundVelocity;
                return (new Kapman.Game.Sim.WorldXZ(w.x, w.z), new Kapman.Game.Sim.WorldXZ(v.x * perHour, v.z * perHour));
            };
            world.Populate();
        }

        bool listening;
        // the chronicle is being told as of the world's own day (the pre-run), not the clock's
        bool replaying;

        [Tooltip("The world's traders and markets start on the rules' world_from (1 April) and are stepped to the clock's start before the first frame. Off: they start with the player (every market untouched).")]
        public bool preRun = true;
        /// <summary>How long the last pre-run took, in real milliseconds (0: there was none).</summary>
        public static long PreRunMs { get; private set; }

        static readonly (string id, string name)[] HudGoods = { ("wheat", "wheat"), ("rhenish_wine", "wine"), ("quernstones", "querns"), ("badorf_ware", "Badorf") };

        /// <summary>The H panel's market line: Dorestad's wheat, wine, querns and Badorf ware, asks / pays (stock).</summary>
        string MarketLine()
        {
            var port = GameData.Port("dorestad");
            if (port == null || clock == null || World == null)
                return null;
            var m = Markets.Of(port, Year, Day);
            if (m.Disrupted(Day))
                return "Dorestad market: shut";
            int month = clock.Date.month;
            var sb = new System.Text.StringBuilder("Dorestad, asks/pays d (stock): ");
            foreach (var (id, name) in HudGoods)
            {
                var g = m.Get(id);
                if (g == null)
                    continue;
                sb.Append(name).Append(' ').Append(m.Asks(g, month).ToString("0.#")).Append('/').Append(m.Pays(g, month).ToString("0.#"))
                  .Append(" (").Append(g.stock.ToString("0")).Append(")   ");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>This instance hears the chronicle (once; the last level's instance stopped when it went).</summary>
        void Listen()
        {
            if (listening)
                return;
            listening = true;
            Chronicle.Came -= ChronicleCame;
            Chronicle.Came += ChronicleCame;
        }

        /// <summary>The merchant starts knowing what the home waterfront knows (from last season): its prices and news.</summary>
        void KnowHome()
        {
            var home = GameData.Port("dorestad");
            if (home == null || World == null || !Trade.Started || Trade.Ledger.Count > 0)
                return;
            var board = World.Board(home.index);
            foreach (var snap in board.All)
                if (Trade.Ledger.Hear(snap, "talk on the waterfront at " + home.name, How.Talk, 1, Day, "quay", home.id))
                    Known.HearOf(GameData.Ports[snap.port]);
            foreach (var (item, learned, from) in board.News)
                Known.HearNews(item, Math.Min(Day, learned), from.StartsWith("the waterfront") ? "talk on the waterfront at " + home.name : from,
                               How.Talk, 1, home.index);
            Known.HearOf(home);
            Chest.Begin();                                                // (the riddle the merchant starts with)
        }

        /// <summary>The game's first day (the chronicle's entries up to it are the backstory every merchant knows).</summary>
        public Kapman.World.GameDate StartDate => clock != null ? Kapman.World.Calendar.At(clock.year, clock.startDay, clock.startHour)
                                                                : Kapman.World.Calendar.At(793, 172, 14.0);

        void ChronicleCame(ChronicleEntry e)
        {
            if (World == null)
                return;
            // news, from the ports named (or every port of the regions named), spreading with the ships
            var at = Shocks.PortsOf(e, Year);
            // "now": the clock's day, or the world's own while the weeks before the start are being stepped
            double now = replaying ? World.Day : Day;
            // the game day it happened (a replayed event is in the past)
            double happened = now;
            if (clock != null && e.year > 0)
            {
                var today = replaying ? World.Date : clock.Date;
                int doy = Math.Max(1, e.day);
                for (int m = 1; m < Math.Max(1, e.month); m++)
                    doy += Kapman.World.Calendar.DaysIn(e.year, m);
                double ago = (today.year * 365.0 + today.year / 4 + today.dayOfYear) - (e.year * 365.0 + e.year / 4 + doy);
                happened = now - Math.Max(0.0, ago);
            }
            // (a land with no port in the game, as Iceland and Greenland: its news comes ashore where its ships go, or
            // nobody would ever hear of it. Only the news: the markets there feel nothing of it, below.)
            var told = at.Count > 0 ? at : Chronicle.NewsLandsAt(e, Year);
            World.PostAt(e.kind, e.title.TrimEnd('.') + ".", told.ConvertAll(p => p.index), happened, e.id);
            // the markets feel it: stock lost, prices up, a raided market shut (shocks_793.json)
            Shocks.Apply(e, at.ConvertAll(p => World.MarketAt(p.index)), happened, now);
            if (e.kind != "raid" && e.kind != "burning")
                return;
            double rest = World.raiders != null ? World.raiders.restDays : 60.0;
            foreach (string id in e.ports)
            {
                var p = GameData.Port(id);
                if (p != null)
                    World.MarkRaided(p.index, now + rest);
            }
        }

        /// <summary>Lying at a port, the merchant knows how they're regarded there (and in its region) as of now.</summary>
        void NoteStanding()
        {
            Chest.CheckWakes();                                           // (something learned this hour may wake a riddle)
            if (ship != null)
            {
                var at = Kapman.World.FloatingOrigin.ToWorld(ship.transform.position);
                Chest.CheckVoyages(at.x, at.z);                           // (near a place an old text sails to: found)
            }
            if (ship == null || !ship.anchored)
            {
                if (ship != null)
                    Chest.AtSea();                                        // (things seen at sea are known now)
                return;
            }
            var p = Port.Nearest(ShipPosition);
            if (p != null && p.Def != null && p.DistanceTo(ShipPosition) < p.arrivalRadius)
            {
                Known.NoteStanding(p.Def, Reputation.At(p.Def), Reputation.OfRegion(p.Def.region), Day);
                Chest.AtPort(p.Def);                                      // (answers brought back; riddles found here)
            }
        }

        /// <summary>The game is quit (the editor's stop too): one last autosave, if nothing is in the way. This comes
        /// before anything of the scene is taken down, so the world, the ship and the clock are all still there.</summary>
        void OnApplicationQuit()
        {
            if (Current == this)
                Autosave.Quitting();
        }

        void OnDestroy()
        {
            Chronicle.Came -= ChronicleCame;
            if (clock != null)
                carriedHours = clock.GameHours;                           // (the next level's clock goes on from here)
            if (Current == this)
                Current = null;
        }

        // dev: the game's rate, 1x (a game minute a second), 60x (an hour a second), 1440x (a day a second): [ and ]
        static readonly float[] Rates = { 60f, 3600f, 86400f };
        int rateIndex;

        void FastForwardKeys()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null || clock == null)
                return;
            int was = rateIndex;
            if (kb.rightBracketKey.wasPressedThisFrame) rateIndex = Mathf.Min(Rates.Length - 1, rateIndex + 1);
            if (kb.leftBracketKey.wasPressedThisFrame) rateIndex = Mathf.Max(0, rateIndex - 1);
            if (was != rateIndex)
            {
                clock.rate = Rates[rateIndex];
                Debug.Log($"Kapman: game time x{Rates[rateIndex] / 60f:0} (dev fast-forward)");
            }
#endif
        }

        /// <summary>The game's speed against the normal (1, 60, 1440), for showing.</summary>
        public float Speedup => clock != null ? clock.rate / 60f : 1f;

        /// <summary>After the clock is set (a saved game loaded): the hours between aren't played through.</summary>
        public void ResyncHours()
        {
            lastHours = clock != null ? clock.GameHours : -1.0;
            resync = true;                                               // (the stepped clock takes the hour up at its next frame)
        }

        /// <summary>[F5] saves the game ("quick"), [F9] loads it; what came of it as a notice.</summary>
        void SaveKeys()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null)
                return;
            if (kb.f5Key.wasPressedThisFrame)
                Voyage.Say(SaveGame.Save());
            else if (kb.f9Key.wasPressedThisFrame)
                Voyage.Say(SaveGame.Load());
        }

        void Update()
        {
            Explore();
            MakeWorld();
            FastForwardKeys();
            SaveKeys();
            PanelClock.Tick();                                           // (the clock waits while a card wants an answer)
            GameSpeed.Stepped = ChartWorld.Current != null;              // (the chart: the game steps its own clock, below)
            GameSpeed.Tick();                                            // ([Space] pause, [+] [-] the five speeds)
            Alerts.Tick();
            Rest.Tick(clock);                                            // ([Z] at night: rest till dawn)
            GameEvents.Look();
            if (clock == null)
                return;
            if (GameSpeed.Stepped)
            {
                // (ruling 26: whole game minutes, the same at every speed. The frame's own time: the time scale is 1
                // while the clock runs, so this is the wall clock's; and where frames are counted and not timed, as a
                // play test's are, it is the frame's share of a second and not the instant a frame takes there)
                if (!ClockByHand)
                    RunClock(Time.deltaTime);
                return;
            }
            // the tick ladder: minutes, hours, days and years raised in order, once, however the clock moved (a jump from
            // the keys replays every tick in between in this frame; going back raises Reversed only)
            double hours = clock.GameHours;
            if (lastHours < 0.0)
            {
                lastHours = hours;
                return;
            }
            ladder ??= new Kapman.Game.Sim.TickLadder(clock.year, clock.startDay);
            ladder.Advance(lastHours, hours, false, ticks);
            lastHours = hours;
        }

        // ------------------------------------------------------------------ the stepped clock (the chart; ruling 26)

        static readonly Kapman.Game.Sim.ClockStepper stepper = new Kapman.Game.Sim.ClockStepper();
        static int settle, lastSign, restless;
        static bool restlessTold;
        static bool resync = true;

        /// <summary>Frames the clock stands after a step in which something happened that the frame, a port's graph or
        /// a talk must see before time goes on (they look once a frame, not once a step).</summary>
        public const int SettleFrames = 2;
        /// <summary>The real milliseconds one frame may spend in steps; what is left waits for the next frame (the clock
        /// then runs slower than its speed says; nothing is skipped).</summary>
        public static double StepBudgetMs = 8.0;
        /// <summary>A test runs the clock itself (RunClock with its own seconds): Update leaves it alone.</summary>
        public static bool ClockByHand;
        /// <summary>The steps the last frame ran, and the real milliseconds they took.</summary>
        public static int StepsLastFrame { get; private set; }
        public static double StepMsLastFrame { get; private set; }
        /// <summary>Game seconds of the step being run now (0 outside a step).</summary>
        public static double StepSeconds { get; private set; }
        /// <summary>How far the clock is into the step to come, 0..1 (her mark is drawn between two steps).</summary>
        public static double StepFraction => stepper.Fraction;
        /// <summary>A step was run (the game minute it ended on), after everything of the game has had it.</summary>
        public static event Action<long> Stepped;

        /// <summary>
        /// One frame of the stepped clock: `realSeconds` of the wall clock at the game's speed, as whole game minutes.
        /// Each step is the same at every speed (<see cref="StepMinute"/>). The frame's steps end early when a step
        /// changed something the per-frame side of the game must answer (<see cref="Sign"/>): the clock then stands
        /// <see cref="SettleFrames"/> frames, so a card is up and holding it before another minute passes. Near a port
        /// and in a meeting at sea a frame runs one step at most (<see cref="OneAFrame"/>). A test calls this itself
        /// with its own seconds.
        /// </summary>
        public void RunClock(double realSeconds)
        {
            StepsLastFrame = 0;
            StepMsLastFrame = 0.0;
            if (clock == null)
                return;
            double hours = clock.GameHours;
            ladder ??= new Kapman.Game.Sim.TickLadder(clock.year, clock.startDay);
            if (lastHours < 0.0)
            {
                lastHours = hours;                                       // (the first frame: nothing has passed yet)
                resync = true;
            }
            else if (Math.Abs(hours - lastHours) > 1e-9)
            {
                // the clock was moved from outside (a dev key, a test): the ladder replays what passed, as it always did
                ladder.Advance(lastHours, hours, false, ticks);
                lastHours = hours;
                resync = true;
            }
            if (resync)
            {
                resync = false;
                stepper.SetTo(hours);
                ClockDrops.Rearm();                                      // (what is so now is not news)
                lastSign = Sign();
                settle = 0;
            }
            int sign = Sign();
            if (sign != lastSign)
            {
                lastSign = sign;                                         // (the frame's own side did something: let it settle)
                settle = SettleFrames;
                // (and it may itself be a reason for the clock to drop back: she was laid to at a berth. Asked now, so
                // the drop does not wait for the next minute behind the card of whoever came down to the boat)
                ClockDrops.Step(stepper.Minute);
                // (a guard: if something in the sign changed every frame the clock would never run. After thirty such
                // frames in a row it runs anyway, and says so once)
                if (++restless > 30)
                {
                    settle = 0;
                    if (!restlessTold)
                        Debug.LogWarning("Kapman clock: something the clock watches changes every frame (KapmanGame.Sign): the clock runs on regardless.");
                    restlessTold = true;
                }
            }
            else
                restless = 0;
            if (settle > 0)
            {
                settle--;
                stepper.Stop();
                return;
            }
            int want = stepper.Take(realSeconds, GameSpeed.Rate);
            bool one = want > 1 && OneAFrame();
            if (one)
                want = 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < want; i++)
            {
                StepMinute();
                StepsLastFrame++;
                int now = Sign();
                if (now != lastSign || GameSpeed.Paused)
                {
                    lastSign = now;
                    settle = SettleFrames;
                    stepper.Stop();
                    break;
                }
                if (watch.Elapsed.TotalMilliseconds > StepBudgetMs)
                    break;                                               // (the rest of this frame's minutes wait for the next)
            }
            if (one)
                stepper.Stop();
            StepMsLastFrame = watch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// ONE GAME MINUTE, always in this order: her orders; she sails the minute; the minute is over (the tick ladder:
        /// the minute, then on the hour the world's ships and the cards' draw, then on the day the markets, the bread and
        /// the pay); the lookout; the mooring watch and an open card's waiting time; a wait for light or wind; the fog of
        /// war; and last the reasons for the clock to drop back.
        /// </summary>
        void StepMinute()
        {
            const double seconds = Kapman.Game.Sim.ClockStepper.StepSeconds;
            StepSeconds = seconds;
            long minute = stepper.Step();
            double to = minute / 60.0;
            VoyagePlanner.Current?.Step(seconds);
            ChartBody.Current?.Step(seconds);
            clock.GameHours = to;
            ladder.Advance(lastHours, to, false, ticks);
            lastHours = to;
            Kapman.Game.Sim.SeaWatch.Current?.Step(minute);
            EncounterRunner.Instance?.Step(seconds);
            Kapman.Game.Frame.FrameWait.Step(minute);
            if (ship != null && Exploration.Ready)
            {
                // (each game minute: what is in sight goes with her; it was looked for every half hour)
                var w = Kapman.World.FloatingOrigin.ToWorld(ship.transform.position);
                Exploration.See(w.x, w.z, Exploration.Sight);
                // (and the waters and landmarks she has come to are found, and the harbours that have come in sight:
                // the log says each, once, and the clock drops back for it)
                var at = new Kapman.Game.Sim.WorldXZ(w.x, w.z);
                bool newInSight = false;
                foreach (var found in Discoveries.Step(at, Day, Year))
                {
                    Voyage.Say(Discoveries.Line(found), Discoveries.Short(found), "", Discoveries.Box(found));
                    newInSight = true;
                }
                foreach (var port in Sightings.Step(Year))
                {
                    Voyage.Say(Sightings.Line(port, at), Sightings.Short(port, at), "", Sightings.Box(port, at));
                    newInSight = true;
                }
                if (newInSight)
                    ClockDrops.Raise(ClockReason.Sighted);
            }
            // (9 Oct 2026: the rough water she is in, and how rough it is this minute. After the finds, so that a race
            // found in this same minute is said by its name)
            var body = ChartBody.Current;
            if (ship != null && body != null)
                RoughWaters.Step(ChartWorld.ShipWorld(), body.Wind, body.WindFrom, Kapman.Game.Sim.SimStage.JulianDay,
                                 Port.LyingId() == null && !ChartWorld.OnRiver(), seconds / 3600.0);
            // (9 Oct 2026: the lender's men, when a boat of them is out after her)
            if (ship != null && body != null)
                Collectors.Step(seconds / 3600.0);
            // (9 Oct 2026: Ravn's death and what is done about it, when the lender's men have killed him)
            Feud.Step(seconds / 3600.0);
            ClockDrops.Step(minute);
            StepSeconds = 0.0;
            Stepped?.Invoke(minute);
        }

        /// <summary>A number that changes when something happens that the per-frame side of the game answers: a card in
        /// line or open or answered, a talk, a happening, she is laid to or let go, a meeting's stage, a wait, her
        /// course, the speed, the port she is at. Nothing in it changes merely because time passes.</summary>
        int Sign()
        {
            unchecked
            {
                int h = 17;
                var d = EncounterRunner.Director;
                h = h * 31 + (d == null ? 0 : (d.Current != null ? (d.Current.Chosen ? 2 : 1) : 0) + (d.Waiting ? 4 : 0) + (d.InCall ? 8 : 0));
                var text = TextPanel.Current;
                h = h * 31 + (text != null && text.Open ? 1 + (text.Key ?? "").GetHashCode() : 0);
                h = h * 31 + (GameEvents.Current != null ? 1 : 0) + (GameEvents.Waiting ? 2 : 0);
                h = h * 31 + (ship != null && ship.anchored ? 1 : 0);
                h = h * 31 + (int)Kapman.Game.Sim.SeaHolds.Now;
                h = h * 31 + (int)Aground.Stage;
                h = h * 31 + (Rest.On ? 1 : 0) + (TideWait.On ? 2 : 0) + (Kapman.Game.Frame.FrameWait.On ? 4 : 0);
                h = h * 31 + (Course.IsSet ? 1 + (Course.Destination ?? "").GetHashCode() : 0);
                h = h * 31 + GameSpeed.Level + (GameSpeed.Paused ? 8 : 0) + GameSpeed.Cap * 16;
                var port = ship != null ? Port.Nearest(ShipPosition) : null;
                h = h * 31 + (port != null && port.DistanceTo(ShipPosition) < port.arrivalRadius ? 1 + (port.id ?? "").GetHashCode() : 0);
                return h;
            }
        }

        /// <summary>Where the per-frame side of the game watches something that changes smoothly (the port's graph: is
        /// she near and slow enough to lay to; a talk that counts minutes; a chase), the clock runs one step a frame at
        /// most, so those looks come between every two minutes as they do at speed 1.</summary>
        bool OneAFrame()
        {
            if (Kapman.Game.Sim.SeaHolds.Now != Kapman.Game.Sim.SeaHold.None || Aground.Stage != AgroundStage.None || Rest.On)
                return true;
            var text = TextPanel.Current;
            if (text != null && text.Open && text.Key != TextPanel.PortMenuKey)
                return true;
            if (ship == null || ship.anchored)
                return false;
            var port = Port.Nearest(ShipPosition);
            return port != null && port.DistanceTo(ShipPosition) < port.arrivalRadius * 2f;
        }

        // play mode starts without a domain reload: a new game each play
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            settle = lastSign = restless = 0;
            restlessTold = false;
            ClockByHand = false;
            resync = true;
            StepsLastFrame = 0;
            StepMsLastFrame = StepSeconds = 0.0;
            Stepped = null;
            world = null;
            PreRunMs = 0;
            lastHours = -1.0;
            ladder = null;
            begun = false;
            carriedHours = -1.0;
            HourPassed = null;
            DayPassed = null;
            MinutePassed = null;
            YearPassed = null;
            Jumped = null;
            Reversed = null;
        }
    }
}
