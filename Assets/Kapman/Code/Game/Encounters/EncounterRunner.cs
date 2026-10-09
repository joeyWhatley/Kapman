using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Encounters;
using Kapman.Levels;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE ENCOUNTERS IN THE SCENE (npc_text_encounters.md 4.1, EncounterRunner and MooringTrigger): on the "--- GAME ---"
    /// object beside KapmanGame. It loads the cards once (the same files the tests read), makes the one EncounterDirector
    /// of the game (static, so it lives on from level to level like the trade), and each frame tells it the time and where
    /// the player's ship is: the director's watch begins a call when she comes to rest at a mooring of the level (the
    /// berths of chain.json, and the town's dam heads and mooring posts near one) and ends it when she leaves. Each game
    /// hour (KapmanGame.HourPassed, which replays every hour of a jump in order) it runs the hourly draw. When no panel
    /// holds the player's attention (UiFocus), the first card in line opens; EncounterPanel shows it and takes the keys.
    /// The effects go to the real game (GameEffects); the toll's ease or waiver is read by the sales through
    /// <see cref="TollFactor"/>.
    ///
    /// ON THE CHART (7 Oct 2026: L_Chart has no level, so no berth of chain.json, and no card could open there): every
    /// port ChartWorld makes is a mooring, and a call begins when the port's graph has laid her to at one (she is
    /// "anchored" inside its reach; the clock is held in port, so the dwell is counted in real seconds). A new game
    /// opens with her lying at home: that call is in progress from the start, quietly (nobody comes down to a boat that
    /// has not arrived). The pace is kept sane for a book of a thousand cards (<see cref="chartSeaGapHours"/>, one drawn
    /// card a call), a card opens over the port's menu (<see cref="Blocked"/> says when it must wait), and a card file
    /// with faults is shut alone: the rest of the book plays.
    /// </summary>
    [DefaultExecutionOrder(10)]
    public class EncounterRunner : MonoBehaviour
    {
        [Tooltip("The card files (empty: every .json in Assets/Kapman/Game/Data/encounters but the roles and the names).")]
        public TextAsset[] cardFiles = new TextAsset[0];
        [Tooltip("Metres from a berth of chain.json within which she is at it.")]
        public float berthRadius = 40f;
        [Tooltip("Metres from a dam head or a mooring post within which she is at it.")]
        public float pointRadius = 20f;
        [Tooltip("Dam heads and mooring posts count as moorings only this near a berth (m): the town's, not a post on a far shore.")]
        public float nearBerth = 400f;
        [Tooltip("A call ends when she is this far (m) from every mooring of its port.")]
        public float leaveRadius = 150f;
        [Tooltip("Seconds of game time (at the game's speed) she must lie still at a mooring before the call begins.")]
        public float dwellSeconds = 2f;
        [Tooltip("Through the water slower than this (m/s), with no order, she is lying still.")]
        public float stillSpeed = 0.3f;
        [Tooltip("Other places she lies to (optional): each belongs to the port of the nearest berth.")]
        public Transform[] extraMoorings = new Transform[0];
        // (36 since the clock has five speeds, ruling 26 of 7 Oct 2026: at the old 4 a card came every two and a half
        // seconds of running clock at speed 4, and the clock would never run. At 36 it is at most two in three days at
        // sea: one every 22 seconds at speed 4, every 90 at speed 3, and one to three on a crossing to Lundenwic.
        // Design/fixes/clock_five_speeds.md 7.1. One number: tune it by play.)
        [Tooltip("On the chart: game hours between one drawn card at sea and the next.")]
        public float chartSeaGapHours = 36f;
        [Tooltip("On the chart: game metres from a port's berth at which a call there ends.")]
        public float chartLeaveRadius = 60f;

        /// <summary>When a waiting card must not open yet (null: while any panel holds the player's attention, UiFocus;
        /// on the chart, while the title, the market, the chest or a talk is up). The frame sets its own.</summary>
        public static Func<bool> Blocked;
        /// <summary>The card files shut for their faults when the book was read (each with its first fault).</summary>
        public static readonly List<string> ShutFiles = new List<string>();
        /// <summary>The card files not read into this game: held back until they are polished (ContentHold, 9 Oct 2026).</summary>
        public static readonly List<string> HeldFiles = new List<string>();

        /// <summary>The game's encounters (null until the cards have loaded without faults).</summary>
        public static EncounterDirector Director { get; private set; }
        /// <summary>The cards as loaded (with their faults, if any).</summary>
        public static EncounterBook Book { get; private set; }
        /// <summary>The runner at work (one, in the level the ship is in).</summary>
        public static EncounterRunner Instance { get; private set; }

        static Dictionary<string, object> pendingRestore;
        static Kapman.Chain.ChainData chain;
        static bool chainFailed, bookReported;

        double lastHours = double.NaN;                                    // a level: the clock's hours at the last look (the card's wait)
        string levelFor;                                                  // the level whose moorings the watch holds
        bool homeLooked;                                                  // the chart: the call at home on a new game is seen to
        float lastLook;                                                   // the chart: the wall clock at the last look

        static bool OnChart => ChartWorld.Current != null;

        // ------------------------------------------------------------------ for the rest of the game

        /// <summary>The share of the zone's toll on the merchant's sales at a port today (1 with no encounters): Deal.Sell
        /// multiplies its toll by it.</summary>
        public static double TollFactor(string port, double day) => Director != null ? Director.TollFactor(port, day) : 1.0;

        /// <summary>A pressing card that holds the ship is open and unanswered (rulings 10 and 12 of 7 Oct 2026): she
        /// keeps her course and her order, and makes no way until it is answered or has answered itself.</summary>
        public static bool ShipHeld => Director != null && Director.HoldsShip;

        /// <summary>The encounter layer for a saved game (null before it has started).</summary>
        public static Dictionary<string, object> Capture() => Director != null ? Director.Capture() : null;

        /// <summary>The encounter layer from a saved game (kept until the director is made, if it isn't yet).</summary>
        public static void Restore(Dictionary<string, object> d)
        {
            if (Director != null)
                Director.Restore(d);
            else
                pendingRestore = d;
            if (Instance != null)
                Instance.lastHours = double.NaN;                          // (the jump of a load is no waiting time)
        }

        /// <summary>The game's own name for a thing a card's rule names, for the line under a choice that cannot be taken
        /// (Cond.NameOf): a writing's title, a good's name, a port's, a role's title, a stat's level; null: none to show.</summary>
        public static string NameOf(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (key.StartsWith("text.", StringComparison.Ordinal))
            {
                var text = ChestData.Text(key.Substring(5));
                return text != null && !string.IsNullOrEmpty(text.title) ? text.title : null;
            }
            if (key.StartsWith("hold.", StringComparison.Ordinal))
            {
                string id = key.Substring(5);
                var good = GameData.Good(id);
                return good != null ? Trade.Named(good) : id == "weapons" || id == "captives" ? id : null;
            }
            if (key.StartsWith("port:", StringComparison.Ordinal))
                return GameData.Port(key.Substring(5))?.name;
            // ("units:frisian_cloth:4": "4 bolts of Frisian cloth")
            if (key.StartsWith("units:", StringComparison.Ordinal))
            {
                var parts = key.Split(':');
                var good = parts.Length == 3 ? GameData.Good(parts[1]) : null;
                return good != null && int.TryParse(parts[2], out int n) && n > 0 ? Trade.UnitsNamed(good, n) : null;
            }
            // (why a choice is shut, where a card asks a thing of the boat by its flag)
            if (key.StartsWith("why:", StringComparison.Ordinal))
                switch (key.Substring(4))
                {
                    case "flag.sea_reefed": return "Only with the sail shortened.";
                    case "flag.sea_lashed": return "Only with the cargo lashed down.";
                    case "flag.sea_spar_aboard": return "Only with a spare spar aboard.";
                    case "flag.crew_on_shares": return "Only when the men sail on shares.";
                    case "flag.crew_own_bundles": return "Only when the men carry bundles of their own.";
                    default: return null;
                }
            if (key.StartsWith("role:", StringComparison.Ordinal))
                return Book != null ? Book.roles.Role(key.Substring(5))?.title : null;
            if (key.StartsWith("level:", StringComparison.Ordinal))
            {
                var parts = key.Split(':');
                var stat = parts.Length == 3 ? SelfBook.Current.Stat(parts[1]) : null;
                return stat != null && int.TryParse(parts[2], out int pips) && stat.Level(pips).Length > 0 ? stat.Level(pips) : null;
            }
            return null;
        }

        /// <summary>A port's realm (ports_793.json) and people (the culture of its words), for the director.</summary>
        public static (string realm, string people) PortInfo(string id)
        {
            var p = GameData.Port(id);
            return p == null ? ("", "") : (p.realm ?? "", Wording.CultureOf(p) ?? "");
        }

        // ------------------------------------------------------------------ the component

        void OnEnable()
        {
            if (Instance == null && ChainRuntime.Allows(this))
                Instance = this;
            KapmanGame.HourPassed -= OnHour;
            KapmanGame.HourPassed += OnHour;
        }

        void OnDisable()
        {
            KapmanGame.HourPassed -= OnHour;
            if (Instance == this)
                Instance = null;
        }

        void Start() => EnsureDirector();

        void OnHour(double hour)
        {
            if (Instance == this && Director != null)
                Director.Tick((long)Math.Floor(hour));
        }

        void Update()
        {
            if (Instance == null && ChainRuntime.Allows(this))
                Instance = this;
            if (Instance != this)
                return;
            EnsureDirector();
            var d = Director;
            if (d == null)
                return;
            var game = KapmanGame.Current;
            if (game != null)
            {
                d.SetTime(game.Day, game.Year, game.Month, (long)Math.Floor(game.Hours));
                // A CARD LEFT UNANSWERED WAITS IN RUNNING GAME HOURS (rulings 10 and 12). On the chart they are counted
                // in the clock's own step (Step, below). In a level they are the clock's hours since the last look:
                // they stand still while the clock is held, and the jump of a load or a new game is not counted.
                double hours = game.Hours;
                double passed = double.IsNaN(lastHours) ? 0.0 : hours - lastHours;
                lastHours = hours;
                if (!GameSpeed.Stepped && passed > 0.0 && passed < 24.0 * 30.0 && !GameSpeed.Paused)
                    d.Wait(passed);
            }
            bool chart = OnChart;
            d.watch.dwellSeconds = dwellSeconds;
            d.watch.stillSpeed = stillSpeed;
            d.watch.leaveRadius = chart ? chartLeaveRadius : leaveRadius;
            d.seaGapHours = chart ? chartSeaGapHours : 0.0;
            d.callDrawMax = chart ? 1 : int.MaxValue;
            Moorings(d.watch);
            var ship = ShipMotion.Player;
            if (ship != null && chart)
            {
                // (real seconds by the wall clock: the game's own seconds stand still while the clock is held)
                float now = Time.realtimeSinceStartup;
                float dt = lastLook > 0f ? Mathf.Clamp(now - lastLook, 0f, 0.25f) : 0f;
                lastLook = now;
                // the stepped clock looks from its own step, in game seconds (Step, below); here only while it stands
                if (!GameSpeed.Stepped || GameSpeed.Paused || !homeLooked)
                    ChartLook(d, game, ship, dt);
            }
            else if (ship != null)
            {
                var p = ship.transform.position;
                Autosaved(d.Look(p.x, p.z, ship.Goal.HasValue, ship.Speed, Time.deltaTime));
            }
            // (a mooring's save that had to wait. It is made after the call has begun and its cards are in line, and
            // before the first of them opens: a load comes back to the moment of arrival, with the cards still to come)
            Autosave.Tick();
            bool blocked = Blocked != null ? Blocked() : chart ? ChartBlocked() : UiFocus.Modal;
            if (d.Current == null && d.Waiting && !blocked)
                d.OpenNext();
        }

        /// <summary>The chart: she has moored when the port's graph has laid her to (lying still is "anchored"); the
        /// dwell is `dt`: real seconds while the clock stands, game seconds from the clock's step.</summary>
        void ChartLook(EncounterDirector d, KapmanGame game, ShipMotion ship, double dt)
        {
            var p = ship.transform.position;
            bool lying = game != null && game.ship != null && game.ship.anchored;
            // (under way: on a charted course, or ordered somewhere else than where she is; an order to where she
            // already is is how she is stopped, and the next point of a river's course is often only metres off)
            bool sailing = !lying && (Course.IsSet || (ship.Goal.HasValue && (ship.Goal.Value - p).sqrMagnitude > 36f));
            HomeCall(d, lying);
            Autosaved(d.Look(p.x, p.z, sailing, sailing ? ship.Speed : 0.0, dt));
        }

        /// <summary>One step of the game's clock on the chart: the mooring watch, in game seconds, so a call begins in
        /// the same game minute at every speed.</summary>
        public void Step(double gameSeconds)
        {
            var d = Director;
            var game = KapmanGame.Current;
            var ship = ShipMotion.Player;
            if (Instance != this || d == null || game == null || ship == null || !OnChart)
                return;
            d.SetTime(game.Day, game.Year, game.Month, (long)Math.Floor(game.Hours));
            ChartLook(d, game, ship, gameSeconds);
            LenderDay(d, game);
            // (the open card's waiting time is counted here, in the clock's own step: a card left unanswered takes its
            // own option in the same game minute at every speed. Nothing steps while the clock is held.)
            d.Wait(gameSeconds / 3600.0);
        }

        /// <summary>The lender's cards of the day: at home, and (a week on) wherever she lies.</summary>
        public const string LenderDayHome = "harbour.wokerers_day.home", LenderDayAway = "harbour.wokerers_day.away";
        int lenderAsked = int.MinValue;
        long lenderLooked = long.MinValue;

        /// <summary>
        /// THE LENDER'S DAY COMES TO HER WHERE SHE LIES. His card is one that meets her as she moors; lying at home from
        /// before Michaelmas she never moors on the day, and nobody came (the play-through of 9 Oct 2026: the day passed
        /// with 1920 d owed and nothing was said). Made fast with the day come and the boat still owed for, his card is
        /// asked for from first light, once a day, and looked for once an hour until it can come.
        /// </summary>
        void LenderDay(EncounterDirector d, KapmanGame game)
        {
            long hourNow = (long)Math.Floor(game.Hours);
            int today = (int)Math.Floor(game.Day);
            if (hourNow == lenderLooked || today == lenderAsked)
                return;
            lenderLooked = hourNow;
            if (!d.InCall || d.Current != null || !Trade.Started || Trade.Debt <= 0.01 || Trade.DebtDaysLeft(game.Day) > 0.0)
                return;
            if (((game.Hours % 24.0) + 24.0) % 24.0 < 6.0)
                return;
            if (d.Offer(LenderDayHome) || d.Offer(LenderDayAway))
                lenderAsked = today;
        }

        /// <summary>She has moored: the game saves itself (held if something is in the way). She has cast off: a save
        /// still held is let go.</summary>
        static void Autosaved(MooringWatch.Change change)
        {
            if (change == MooringWatch.Change.Moored)
                Autosave.Moored();
            else if (change == MooringWatch.Change.CastOff)
                Autosave.CastOff();
        }

        /// <summary>The chart, a new game: she lies at home, so the call there is in progress from the first moment (the
        /// cards read "moored" and the port), but it is not an arrival. Looked at until she has lain to or sailed; a
        /// saved game brings its own call.</summary>
        void HomeCall(EncounterDirector d, bool lying)
        {
            if (homeLooked)
                return;
            if (d.InCall || d.watch.Armed || ChartWorld.SaveLoaded)
            {
                homeLooked = true;
                return;
            }
            string at = lying ? Port.LyingId() : null;
            if (at == null)
                return;
            homeLooked = true;
            d.BeginCallQuietly(at);
        }

        /// <summary>The chart without the frame: a card waits while the title or the pause menu is up, or the market, the
        /// chest, or a talk (the text panel on anything but the port's own menu, which a card may open over).</summary>
        static bool ChartBlocked()
        {
            if (MainMenu.Current != null && (MainMenu.Current.Open || MainMenu.Current.OnTitle))
                return true;
            if (MarketPanel.Current != null && MarketPanel.Current.IsOpen)
                return true;
            if (MapView.Current != null && MapView.Current.ChestOpen)
                return true;
            var text = TextPanel.Current;
            return text != null && text.Open && text.Key != TextPanel.PortMenuKey;
        }

        /// <summary>The cards and the director, once for the game (a fault in the cards is reported, and nothing opens).</summary>
        void EnsureDirector()
        {
            if (Director != null)
                return;
            if (Book == null)
            {
                Book = LoadBook(cardFiles);
                foreach (string shut in ShutFiles)
                    Debug.LogError("Kapman encounters: a card file has faults and is shut (the rest of the book plays): " + shut);
                if (Book != null && (HeldFiles.Count > 0 || Book.heldBack > 0))
                    Debug.Log($"Kapman encounters: held back until polished ({ContentHold.File}): {HeldFiles.Count} card files, and {Book.heldBack} cards of the files that are in. " +
                              $"{Book.cards.Count} cards are in the game.");
                if (Book != null && Book.warnings.Count > 0)
                    Debug.LogWarning("Kapman encounters: the cards load, with things to look at:\n" + string.Join("\n", Book.warnings));
            }
            if (Book == null || !Book.Ok)
            {
                if (!bookReported)
                {
                    bookReported = true;
                    Debug.LogError("Kapman encounters: the cards have faults, so none will open:\n" +
                                   (Book != null ? string.Join("\n", Book.errors) : "no cards"));
                }
                return;
            }
            var game = KapmanGame.Current;
            int seed = game != null ? game.worldSeed : 793;
            var effects = new GameEffects { book = Book };
            var d = new EncounterDirector(Book, seed, effects);
            effects.portNow = () => d.state.callPort;
            d.portInfo = PortInfo;
            d.standingAt = id => Reputation.At(GameData.Port(id));
            // (the port's own toll man, moneyer, count, bishop: the one its page names, not a stranger made for the card)
            d.npcs.known = (port, role, year) =>
            {
                var p = PlaceBook.Current.Holder(port, role, year);
                return p == null ? null : new NpcRegistry.Known { id = p.id, name = p.name, people = p.people, faith = p.faith, born = p.born, to = p.died > 0 ? p.died : p.to };
            };
            EncounterGameVars.Attach(d.vars, () => d.state.callPort);
            // the cards' rules of 7 Oct 2026: how long each waits unanswered, what it then takes, which hold the ship
            d.rules = CardRules.Read(Table("card_rules_793"));
            if (d.rules.Count == 0)
                Debug.LogWarning("Kapman encounters: no card rules (Game/Data/card_rules_793.json): every card waits a week and takes its own usual answer.");
            ClockDrops.PressingCard = () => Director != null && Director.Pressing;
            // (a person's trade is said as the cards say it, wherever a person is named: "Aldgrim the toll man")
            PlaceBook.RoleTitle = id => Book != null ? Book.roles.Role(id)?.title : null;
            Cond.NameOf = NameOf;
            d.CallBegun += port => Debug.Log($"Kapman encounters: moored at {port}; a call begins.");
            d.CallEnded += port => Debug.Log($"Kapman encounters: cast off from {port}.");
            Director = d;
            if (pendingRestore != null)
            {
                d.Restore(pendingRestore);
                pendingRestore = null;
            }
            if (Book.missingKeys.Count > 0)
                Debug.Log($"Kapman encounters: {Book.cards.Count} cards; {Book.missingKeys.Count} writing keys have no entry in writing.json (shown as [write: key]).");
        }

        /// <summary>The cards and their tables, read as KapmanGame reads its tables (GameFiles), and checked. A card file
        /// with faults is shut alone and the book read again without it (<see cref="LoadWithout"/>).</summary>
        /// <summary>The book as a new game reads it, with what is held back left out (a test).</summary>
        public static EncounterBook LoadBookAsTheGameDoes() => LoadBook(null);

        static EncounterBook LoadBook(TextAsset[] files)
        {
            ShutFiles.Clear();
            HeldFiles.Clear();
            var src = new EncounterSources
            {
                roles = Table("encounters/npc_roles_793"),
                names = Table("encounters/names_placeholder"),
                reputation = Table("reputation_793"),
                knowing = Table("knowing_793"),
                writing = Table("writing"),
                goods = Table("goods_793"),
                ports = Table("ports_793"),
                traffic = Table("traffic_793"),
            };
            if (files != null && files.Length > 0)
            {
                foreach (var f in files)
                    if (f != null)
                    {
                        if (ContentHold.FileIn(f.name + ".json"))
                            src.cards.Add(("encounters/" + f.name + ".json", f.text));
                        else
                            HeldFiles.Add(f.name + ".json");
                    }
            }
            else
            {
                // 7 Oct: through ContentFiles, so a built game finds the cards too (StreamingAssets/Data/encounters); the
                // same files in the same order as before in the editor
                foreach (string file in ContentFiles.FolderExists("encounters") ? ContentFiles.List("encounters", "*.json") : new string[0])
                {
                    string name = Path.GetFileName(file);
                    if (name == "npc_roles_793.json" || name == "names_placeholder.json")
                        continue;
                    if (!ContentHold.FileIn(name))
                    {
                        HeldFiles.Add(name);                               // (held back until it is polished: not read at all)
                        continue;
                    }
                    string text = ContentFiles.Read(file);
                    if (text != null)
                        src.cards.Add((file, text));
                }
                if (src.cards.Count == 0)
                {
                    var harbour = GameFiles.Find("encounters/harbour_793");
                    if (harbour != null)
                        src.cards.Add(("encounters/harbour_793.json", harbour.text));
                }
            }
            if (ContentHold.On)
                src.take = ContentHold.CardIn;
            return LoadWithout(src, ShutFiles);
        }

        /// <summary>
        /// Reads the book, and while it has faults that name a card file (every card fault begins with its file's name),
        /// takes those files out and reads again: a card another file's "then" or "schedule" pointed at may have gone with
        /// one, so it is done until the book is sound or no file is to blame (a fault in the roles, the names or the
        /// writing is nobody's file: the book stays faulty, and no card opens, as before). Each file taken out is added
        /// to the list with how many faults it had and the first of them.
        /// </summary>
        public static EncounterBook LoadWithout(EncounterSources src, List<string> shut)
        {
            var book = EncounterLoader.Load(src);
            for (int round = 0; round < 12 && !book.Ok; round++)
            {
                int gone = 0;
                for (int i = src.cards.Count - 1; i >= 0; i--)
                {
                    string name = src.cards[i].name;
                    int n = 0;
                    string first = null;
                    foreach (string e in book.errors)
                        if (e.StartsWith(name + ":", StringComparison.Ordinal) || e.StartsWith(name + " ", StringComparison.Ordinal))
                        {
                            n++;
                            first = first ?? e;
                        }
                    if (n == 0)
                        continue;
                    shut?.Add($"{name} ({n} fault{(n == 1 ? "" : "s")}): {first}");
                    src.cards.RemoveAt(i);
                    gone++;
                }
                if (gone == 0)
                    break;
                book = EncounterLoader.Load(src);
            }
            return book;
        }

        static string Table(string name)
        {
            var t = GameFiles.Find(name);
            return t != null ? t.text : null;
        }

        // ------------------------------------------------------------------ the moorings of the level

        /// <summary>
        /// The level's moorings, made again whenever the level changes: its berths in chain.json, then every dam head and
        /// mooring post of the level within <see cref="nearBerth"/> of a berth (they belong to that berth's port), then the
        /// extra ones set by hand. A level with no berth (the open sea) has none, so no call begins there, and a call in
        /// progress ends (its port has no mooring here).
        /// </summary>
        void Moorings(MooringWatch watch)
        {
            if (OnChart)
            {
                // the chart: a mooring at every port of the world frame (ChartWorld makes them at its Start)
                string key = "chart:" + Port.All.Count;
                if (key == levelFor)
                    return;
                levelFor = key;
                watch.points.Clear();
                foreach (var port in Port.All)
                {
                    if (port == null || string.IsNullOrEmpty(port.id))
                        continue;
                    var at = port.berth != null ? port.berth.position : port.transform.position;
                    watch.points.Add(new MooringPoint(port.id, at.x, at.z, port.arrivalRadius + 5f, true));
                }
                Debug.Log($"Kapman encounters: the chart: {watch.points.Count} moorings, one at every port.");
                return;
            }
            var info = LevelInfo.Current;
            string level = info != null ? info.levelId ?? "" : "";
            if (level == levelFor)
                return;
            levelFor = level;
            watch.points.Clear();
            int berths = watch.AddBerths(ChainLevel(level), berthRadius);
            if (berths == 0)
            {
                Debug.Log($"Kapman encounters: no berth in chain.json for the level '{level}': no port calls here.");
                return;
            }
            int dams = 0, posts = 0;
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (!ChainRuntime.Allows(t))
                    continue;
                Vector3 at;
                if (t.name == "Mooring post")
                    at = t.position;
                else if (!DamHead(t, out at))
                    continue;
                string port = watch.BerthPortNear(at.x, at.z, nearBerth);
                if (port.Length == 0)
                    continue;
                watch.points.Add(new MooringPoint(port, at.x, at.z, pointRadius));
                if (t.name == "Mooring post")
                    posts++;
                else
                    dams++;
            }
            foreach (var t in extraMoorings)
            {
                if (t == null)
                    continue;
                var at = t.position;
                string port = watch.BerthPortNear(at.x, at.z, double.MaxValue);
                if (port.Length > 0)
                    watch.points.Add(new MooringPoint(port, at.x, at.z, pointRadius));
            }
            Debug.Log($"Kapman encounters: {level}: {berths} berth(s), {dams} dam heads and {posts} mooring posts near them, " +
                      $"{watch.points.Count} moorings in all.");
        }

        /// <summary>A dam of the Dorestad builder ("Dam 12", "Dam 12a" under "The harbour: dams on the old riverbed"): it
        /// stands at the dam's middle facing the water, so its head is the far end of its mesh along its forward axis.</summary>
        static bool DamHead(Transform t, out Vector3 head)
        {
            head = Vector3.zero;
            if (!t.name.StartsWith("Dam ", StringComparison.Ordinal) || t.parent == null ||
                !t.parent.name.StartsWith("The harbour", StringComparison.Ordinal))
                return false;
            var mf = t.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return false;
            head = t.TransformPoint(new Vector3(0f, 0f, mf.sharedMesh.bounds.max.z));
            return true;
        }

        /// <summary>A level of chain.json (read once; null if it can't be read or has no such level).</summary>
        static Kapman.Chain.ChainLevel ChainLevel(string id)
        {
            if (chain == null && !chainFailed)
            {
                try
                {
                    chain = Kapman.Chain.ChainData.Load(Path.Combine(Application.dataPath, "Kapman/Data/Chain/chain.json"));
                }
                catch (Exception e)
                {
                    chainFailed = true;
                    Debug.LogWarning("Kapman encounters: chain.json couldn't be read (" + e.Message + "): no berths, so no port calls.");
                }
            }
            return chain != null ? chain.Level(id) : null;
        }

        // Enter Play Mode runs without a domain reload: each play starts with no encounters and reads the cards again
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Director = null;
            Book = null;
            Instance = null;
            pendingRestore = null;
            chain = null;
            chainFailed = false;
            bookReported = false;
            Blocked = null;
            ShutFiles.Clear();
            HeldFiles.Clear();
        }
    }
}
