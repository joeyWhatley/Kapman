using System;
using System.Collections.Generic;
using Kapman.Encounters;

namespace Kapman.Game
{
    /// <summary>A place a ship lies to in a level: a berth of chain.json, a dam head, a mooring post (the level's metres,
    /// x east and z north), how near counts as there, and the port it belongs to.</summary>
    public struct MooringPoint
    {
        public string port;
        public double x, z, radius;
        /// <summary>A berth of chain.json (the others are dam heads and posts near one).</summary>
        public bool berth;

        public MooringPoint(string port, double x, double z, double radius, bool berth = false)
        {
            this.port = port ?? "";
            this.x = x;
            this.z = z;
            this.radius = radius;
            this.berth = berth;
        }
    }

    /// <summary>
    /// THE WATCH ON THE MOORINGS (npc_text_encounters.md 4.1, MooringTrigger): ShipMotion has no "arrived" event, so the
    /// ship is looked at every frame. She has MOORED when she lies inside a mooring's radius with no order and no way on her
    /// (slower than <see cref="stillSpeed"/> through the water) for <see cref="dwellSeconds"/>. A call needs a passage
    /// first: the watch is armed only once she has been under way, so lying at the berth when the game starts is not an
    /// arrival, and moving along the dam heads during a call is still the same call. The call ends (CAST OFF) when she is
    /// farther than <see cref="leaveRadius"/> from every mooring of the call's port, or the level has none of it (she has
    /// left by a join). Pure: the runner feeds it the player's ship, the tests feed it numbers.
    /// </summary>
    public sealed class MooringWatch
    {
        public enum Change { None, Moored, CastOff }

        public readonly List<MooringPoint> points = new List<MooringPoint>();
        /// <summary>Seconds she must lie still at a mooring before the call begins (the design's 2 s).</summary>
        public double dwellSeconds = 2.0;
        /// <summary>Through the water slower than this (m/s), with no order, she is lying still.</summary>
        public double stillSpeed = 0.3;
        /// <summary>A call ends when she is farther than this (m) from every mooring of the call's port.</summary>
        public double leaveRadius = 150.0;

        /// <summary>She has been under way since the last call began (a call can begin).</summary>
        public bool Armed { get; set; }
        /// <summary>She had an order or way on her at the last look.</summary>
        public bool UnderWay { get; private set; }
        /// <summary>The port of the mooring the last call began at.</summary>
        public string MooredAt { get; private set; } = "";

        double still;

        /// <summary>
        /// One look at her: where she is (level metres), whether she has an order, her speed through the water, the
        /// seconds since the last look, and the port of the call in progress ("" none). Says whether a call begins or ends.
        /// </summary>
        public Change Look(double x, double z, bool hasGoal, double speed, double dt, string callPort)
        {
            UnderWay = hasGoal || Math.Abs(speed) > stillSpeed;
            if (UnderWay)
                Armed = true;
            if (!string.IsNullOrEmpty(callPort))
            {
                still = 0.0;
                return Distance(x, z, callPort) > leaveRadius ? Change.CastOff : Change.None;
            }
            int at = Within(x, z);
            if (at < 0 || UnderWay || !Armed)
            {
                still = 0.0;
                return Change.None;
            }
            still += Math.Max(0.0, dt);
            if (still < dwellSeconds)
                return Change.None;
            still = 0.0;
            Armed = false;
            MooredAt = points[at].port;
            return Change.Moored;
        }

        /// <summary>The nearest mooring she is inside (-1: none).</summary>
        public int Within(double x, double z)
        {
            int best = -1;
            double bestD = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                double d = Dist(points[i], x, z);
                if (d <= points[i].radius && d < bestD)
                {
                    best = i;
                    bestD = d;
                }
            }
            return best;
        }

        /// <summary>Metres to the nearest mooring of a port (infinite when the level has none of it).</summary>
        public double Distance(double x, double z, string port)
        {
            double best = double.PositiveInfinity;
            foreach (var p in points)
                if (p.port == port)
                    best = Math.Min(best, Dist(p, x, z));
            return best;
        }

        /// <summary>The port of the nearest berth within a reach of a point ("" if none): what a dam head or a post near
        /// the town belongs to.</summary>
        public string BerthPortNear(double x, double z, double reach)
        {
            string port = "";
            double best = reach;
            foreach (var p in points)
            {
                if (!p.berth)
                    continue;
                double d = Dist(p, x, z);
                if (d <= best)
                {
                    best = d;
                    port = p.port;
                }
            }
            return port;
        }

        /// <summary>The berths of a level of the chain (chain.json's anchors of kind "berth" that have a pose); returns how many.</summary>
        public int AddBerths(Kapman.Chain.ChainLevel level, double radius)
        {
            int n = 0;
            if (level == null)
                return 0;
            foreach (var a in level.anchors)
            {
                if (!a.IsBerth || !a.hasPose || string.IsNullOrEmpty(a.port))
                    continue;
                points.Add(new MooringPoint(a.port, a.pose.x, a.pose.z, radius, true));
                n++;
            }
            return n;
        }

        static double Dist(MooringPoint p, double x, double z) => Math.Sqrt((p.x - x) * (p.x - x) + (p.z - z) * (p.z - z));
    }

    /// <summary>
    /// THE ENCOUNTERS IN PLAY, without Unity (npc_text_encounters.md 2.3, 4.1): the cards, the encounter layer's state and
    /// the people, the watch on the moorings, and the card on screen. EncounterRunner wraps it in the scene (the live
    /// ship, the clock's hours, the live game's variables and GameEffects); the tests drive it with numbers and a fake game.
    ///   - a call begins (the ship moored): the arrival set and the visit draw go in line (Scheduler.Arrive);
    ///   - a call ends (cast off): the harbour cards still waiting go with it, and a waived toll ends;
    ///   - each game hour: the hourly draw (flags lapse, follow-ups come due);
    ///   - when nothing else is on screen, the first card in line opens (EncounterSession.Open); a choice runs its effects
    ///     (Choose; Enter takes the default on a return), Enter after it closes the card, and the next in line opens.
    /// The port's realm, people, faith and mint are set here from the roles table (and <see cref="portInfo"/>); "moored",
    /// "under_way", "year" and "month" from what the director knows; the rest comes from the vars' live source.
    /// </summary>
    public sealed class EncounterDirector
    {
        public readonly EncounterBook book;
        public readonly EncounterState state;
        public readonly NpcRegistry npcs;
        public readonly Scheduler scheduler;
        public readonly EncounterVars vars = new EncounterVars();
        public readonly MooringWatch watch = new MooringWatch();
        public readonly int seed;
        /// <summary>The live game (GameEffects in play, a fake in the tests).</summary>
        public IGameEffects game;
        public string merchant = "player";
        /// <summary>The dice for a chance (0 inclusive to 1 exclusive). Null: drawn from the world's seed, the card and the
        /// game hour, so a replay from a save rolls the same.</summary>
        public Func<double> roll;
        /// <summary>A port's realm (ports_793.json) and people (its culture), for the forge and the cards; null: none known.</summary>
        public Func<string, (string realm, string people)> portInfo;
        /// <summary>The merchant's standing at a port (-100..100), for the draws' m_standing; null: 0.</summary>
        public Func<string, double> standingAt;

        /// <summary>Game days since the start (with the hours), the year, the month (1-12) and the game hour, as last told.</summary>
        public double Day { get; private set; }
        public int Year { get; private set; } = 793;
        public int Month { get; private set; } = 6;
        public long Hour { get; private set; }

        /// <summary>THE PACE (7 Oct, with about a thousand cards in the book). At sea a drawn card comes no sooner than
        /// this many game hours after the last one (0: every hour may draw, as the scheduler has it).</summary>
        public double seaGapHours;
        /// <summary>The most drawn cards a call may bring, the visit's and the hours' together (the arrival set is not
        /// counted: the toll man always comes).</summary>
        public int callDrawMax = int.MaxValue;
        long lastSeaDraw = long.MinValue;
        int callDrawn;

        /// <summary>The card on screen (null: none).</summary>
        public EncounterSession Current { get; private set; }

        public event Action<string> CallBegun, CallEnded;
        /// <summary>A card opened, a choice was made on it, it closed (for the panel, the sounds and visual scripting).</summary>
        public event Action<EncounterSession> Opened, Closed;
        public event Action<EncounterSession, ChoiceResult> Chose;

        // ------------------------------------------------------------------ a card left unanswered (rulings 10 and 12)

        /// <summary>The cards' rules of 7 Oct 2026 (null: every card ordinary).</summary>
        public CardRules rules;
        /// <summary>Running game hours the card on screen has waited unanswered (0 when none is open).</summary>
        public double WaitedHours { get; private set; }
        /// <summary>A waiting time carried over a save: the card it belongs to, and its hours.</summary>
        string savedWaitCard = "";
        double savedWaitHours;
        double nextTry;                                                   // (when none can be taken: tried again an hour on)
        bool choosing;                                                    // (inside Choose: the effects are running)
        /// <summary>The card on screen took an option by itself and closed: the card, what came of it, and whether it was
        /// because she cast off from its port (false: its time ran out).</summary>
        public event Action<EncounterSession, ChoiceResult, bool> TookItself;
        /// <summary>True while the director itself is choosing (Chose fires too: the log must not write it twice).</summary>
        public bool TakingItself { get; private set; }

        /// <summary>The rule of the card on screen (null: none is open).</summary>
        public CardRule RuleNow => Current == null ? null : rules != null ? rules.For(Current.card.id) : new CardRule { id = Current.card.id };
        /// <summary>Running hours it will still wait (0 when none is open or it is answered).</summary>
        public double HoursLeft => Current == null || Current.Chosen ? 0.0 : Math.Max(0.0, RuleNow.patienceHours - WaitedHours);
        /// <summary>A pressing card is open and unanswered.</summary>
        public bool Pressing => Current != null && !Current.Chosen && RuleNow.pressing;
        /// <summary>A card that holds the ship is open and unanswered: she makes no way.</summary>
        public bool HoldsShip => Current != null && !Current.Chosen && RuleNow.holdsShip;

        /// <summary>The options the card on screen would take by itself, in the order to try them: the rule's, by their
        /// keys; then the card's own usual answer; then the rest, the last first (the way out is written last).</summary>
        IEnumerable<int> OwnOrder(EncounterSession s)
        {
            foreach (string key in RuleNow.take)
                yield return s.options.FindIndex(o => o.def.key == key);
            for (int i = 0; i < s.options.Count; i++)
                if (s.options[i].def.isDefault)
                    yield return i;
            for (int i = s.options.Count - 1; i >= 0; i--)
                yield return i;
        }

        /// <summary>The option the card on screen takes if it is left unanswered, as things stand now (-1: none can be
        /// taken, or no card waits).</summary>
        public int OwnOption
        {
            get
            {
                var s = Current;
                if (s == null || s.Chosen)
                    return -1;
                foreach (int i in OwnOrder(s))
                    if (i >= 0 && i < s.options.Count && s.options[i].enabled)
                        return i;
                return -1;
            }
        }

        /// <summary>
        /// Running game hours have passed with the card open (the runner: the clock's own step, so nothing counts while
        /// the clock is held). When its time is spent the card takes an option by itself and closes; the log says which.
        /// If none can be taken on the day it goes on waiting and is tried again each hour. Returns what came of it when
        /// it took one, else null.
        /// </summary>
        public ChoiceResult Wait(double hours)
        {
            var s = Current;
            if (s == null || s.Chosen || !(hours > 0.0))
                return null;
            WaitedHours += hours;
            if (WaitedHours < RuleNow.patienceHours || WaitedHours < nextTry)
                return null;
            var r = TakeItself(false);
            if (r == null)
                nextTry = WaitedHours + 1.0;
            return r;
        }

        ChoiceResult TakeItself(bool castOff)
        {
            var s = Current;
            if (s == null || s.Chosen || TakingItself || choosing)
                return null;
            TakingItself = true;
            try
            {
                foreach (int i in OwnOrder(s))
                {
                    if (i < 0 || i >= s.options.Count || !s.options[i].enabled)
                        continue;
                    var r = Choose(i);
                    if (r == null || !r.ok)
                        continue;
                    TookItself?.Invoke(s, r, castOff);
                    Continue();                                           // (it closes; the log has said what was taken)
                    return r;
                }
            }
            finally
            {
                TakingItself = false;
            }
            return null;
        }

        public EncounterDirector(EncounterBook book, int seed, IGameEffects game, NpcRegistry npcs = null, EncounterState state = null)
        {
            this.book = book ?? throw new ArgumentNullException(nameof(book));
            this.seed = seed;
            this.game = game;
            this.state = state ?? new EncounterState();
            this.npcs = npcs ?? new NpcRegistry(seed, book.roles, book.names);
            scheduler = new Scheduler(book, this.state, seed);
            vars.state = this.state;
            vars.npcs = this.npcs;
        }

        /// <summary>The time now (the runner, each frame; a test, by hand).</summary>
        public void SetTime(double day, int year, int month, long hour)
        {
            Day = day;
            Year = year;
            Month = Math.Max(1, Math.Min(12, month));
            Hour = hour;
        }

        /// <summary>A call is in progress (the ship lies at a port).</summary>
        public bool InCall => !string.IsNullOrEmpty(state.callPort);

        /// <summary>Cards are waiting to open.</summary>
        public bool Waiting => state.queue.Count > 0;

        // ------------------------------------------------------------------ the moments

        /// <summary>One look at the ship (see MooringWatch.Look): a call begins or ends here when the watch says so.</summary>
        public MooringWatch.Change Look(double x, double z, bool hasGoal, double speed, double dt)
        {
            var change = watch.Look(x, z, hasGoal, speed, dt, state.callPort);
            if (change == MooringWatch.Change.Moored)
                BeginCall(watch.MooredAt);
            else if (change == MooringWatch.Change.CastOff)
                EndCall();
            return change;
        }

        /// <summary>The ship has moored at a port: the call begins, and the arrival set and the visit's card (if one is drawn)
        /// go in line. Returns the cards put in line.</summary>
        public List<EncounterDef> BeginCall(string port)
        {
            if (string.IsNullOrEmpty(port))
                return new List<EncounterDef>();
            if (InCall)
                EndCall();
            Prepare(port, Day, true);
            var info = Info(port, Day, true, 0.0);
            var queued = scheduler.Arrive(vars, info, Hour);
            // the pace: the visit's card counts as the call's drawn one (and goes, where the call may have none)
            callDrawn = 0;
            for (int i = queued.Count - 1; i >= 0; i--)
            {
                if (queued[i].arrival)
                    continue;
                if (callDrawn >= callDrawMax)
                {
                    state.queue.Remove(queued[i].id);
                    queued.RemoveAt(i);
                }
                else
                    callDrawn++;
            }
            CallBegun?.Invoke(port);
            return queued;
        }

        /// <summary>She lies at a port with no arrival behind her (a new game opens at home): the call is in progress, so
        /// the cards read the port and "moored", but nobody comes down to the boat: no arrival set, no visit draw.</summary>
        public void BeginCallQuietly(string port)
        {
            if (string.IsNullOrEmpty(port) || InCall)
                return;
            state.BeginCall(port, Day);
            Prepare(port, Day, true);
            callDrawn = 0;
            watch.Armed = false;
            CallBegun?.Invoke(port);
        }

        /// <summary>The ship has cast off: the harbour cards still waiting go (the toll man does not follow her out), and the
        /// call ends (a waived toll with it).</summary>
        public void EndCall()
        {
            string was = state.callPort;
            if (string.IsNullOrEmpty(was))
                return;
            // (8 Oct 2026: and the harbour card on screen, if it has no answer yet. Whoever stood on the quay does not
            // follow her out either: it goes the way it would have gone by itself, now, while the port is still the
            // port; where none of its options can be taken it is put away.)
            var open = Current;
            if (open != null && !open.Chosen && open.card.stage == Stage.Harbour && !choosing && !TakingItself && TakeItself(true) == null)
            {
                Current = null;
                WaitedHours = 0.0;
                Closed?.Invoke(open);
            }
            state.queue.RemoveAll(id =>
            {
                var c = book.Card(id);
                return c == null || c.stage == Stage.Harbour;
            });
            state.EndCall();
            vars.nums["moored"] = 0.0;
            CallEnded?.Invoke(was);
        }

        /// <summary>A whole game hour (KapmanGame.HourPassed): the hourly draw. Returns the card it put in line, or null.</summary>
        public EncounterDef Tick(long hour)
        {
            double day = hour / 24.0;
            bool moored = InCall;
            Prepare(state.callPort, day, moored);
            double hours = moored ? Math.Max(0.0, (day - state.callStartDay) * 24.0) : 0.0;
            var drawn = scheduler.Tick(hour, vars, Info(state.callPort, day, moored, hours));
            if (drawn == null)
                return null;
            // the pace: one drawn card a call; at sea none within some hours of the last (a card over the pace is let go
            // unshown, so its cool-down is not spent)
            bool comes = moored ? callDrawn < callDrawMax : seaGapHours <= 0.0 || lastSeaDraw == long.MinValue || hour - lastSeaDraw >= seaGapHours;
            if (!comes)
            {
                state.queue.Remove(drawn.id);
                return null;
            }
            if (moored)
                callDrawn++;
            else
                lastSeaDraw = hour;
            return drawn;
        }

        // ------------------------------------------------------------------ the card on screen

        /// <summary>Opens the first card in line, if none is open. Returns it (null: none opened).</summary>
        public EncounterSession OpenNext()
        {
            if (Current != null)
                return null;
            var card = scheduler.TakeNext();
            if (card == null)
                return null;
            string port = state.callPort ?? "";
            Prepare(port, Day, port.Length > 0);
            // (a saved game's open card comes back as it was: the people it had, and the dice of the hour it first opened)
            bool back = keptCard != null && keptCard == card.id;
            openedHour = back ? keptHour : Hour;
            var world = new EncounterWorld
            {
                book = book, state = state, npcs = npcs, game = game, vars = vars, merchant = merchant,
                port = Facts(port), year = Year, day = Day, roll = roll ?? Dice(card.id, openedHour),
            };
            Current = EncounterSession.Open(card, world, back ? keptCast : null);
            keptCard = null;
            keptCast = null;
            // (its waiting time begins; a saved game's open card has the hours it had already waited)
            WaitedHours = card.id == savedWaitCard ? savedWaitHours : 0.0;
            savedWaitCard = "";
            savedWaitHours = 0.0;
            nextTry = 0.0;
            Opened?.Invoke(Current);
            return Current;
        }

        /// <summary>A saved game's open card, waiting to come back as it was: its id, its cast (part to person) and the
        /// game hour it opened at.</summary>
        string keptCard;
        long keptHour;
        Dictionary<string, string> keptCast;
        /// <summary>The game hour the card on screen opened at (its dice are seeded by it).</summary>
        long openedHour;

        /// <summary>A card of a loaded game is waiting to open again as it was (its id; null: none).</summary>
        public string KeptCard => keptCard;

        /// <summary>The merchant chooses option `index` (from 0) on the card on screen. A greyed option does nothing and says
        /// its rule (the result's facts). Null when no card is open or it has been answered.</summary>
        public ChoiceResult Choose(int index)
        {
            var s = Current;
            if (s == null || s.Chosen || choosing)
                return null;
            ChoiceResult r;
            choosing = true;
            try
            {
                r = s.Choose(index);
            }
            finally
            {
                choosing = false;
            }
            if (r.ok)
            {
                StatUsed(s, r);
                Chose?.Invoke(s, r);
            }
            return r;
        }

        /// <summary>What a stat of the player's gains from a check won by it: the stat ("tongue") and the chance as the
        /// card wrote it; true when the win filled a pip. The self's own rule (a stat rises by use, never by points);
        /// a test may put its own in, or null for none.</summary>
        public Func<string, double, bool> statUsed = Self.Used;

        /// <summary>A choice whose chance was "by me.tongue" (or another of the five) and was won: the stat rises a
        /// little, and when a pip fills the card's outcome says so.</summary>
        void StatUsed(EncounterSession s, ChoiceResult r)
        {
            var chance = s.card.Option(r.option)?.Chance;
            if (statUsed == null || chance == null || r.branch != "success" || !chance.by.StartsWith("me.", StringComparison.Ordinal))
                return;
            string stat = chance.by.Substring(3);
            if (!statUsed(stat, chance.chance))
                return;
            string name = SelfBook.Current.Stat(stat)?.name ?? stat;
            r.facts.Add($"Your {name} has grown to {Self.Stat(stat)} of {Self.MaxStat}.");
        }

        /// <summary>Enter on a card seen before: the default option (null when it has none to offer).</summary>
        public ChoiceResult ChooseDefault()
        {
            var s = Current;
            if (s == null || s.Chosen || s.DefaultIndex < 0)
                return null;
            return Choose(s.DefaultIndex);
        }

        /// <summary>After the choice (Enter): the card closes; the next in line opens when the runner next asks.</summary>
        public bool Continue()
        {
            var s = Current;
            if (s == null || !s.Chosen)
                return false;
            Current = null;
            WaitedHours = 0.0;
            Closed?.Invoke(s);
            return true;
        }

        /// <summary>Esc: after a choice it closes the card; before one it is refused on an arrival card (the toll man needs
        /// an answer) and closes any other unanswered.</summary>
        public bool Dismiss()
        {
            var s = Current;
            if (s == null)
                return false;
            if (s.Chosen)
                return Continue();
            if (s.card.arrival)
                return false;
            Current = null;
            WaitedHours = 0.0;
            Closed?.Invoke(s);
            return true;
        }

        /// <summary>The share of the zone's toll taken on the merchant's sales at a port today: 1, 0.5 (eased) or 0 (waived).</summary>
        public double TollFactor(string port, double day) => state.TollFactor(port, day);

        // ------------------------------------------------------------------ outside a card (Content/ContentRules.cs)

        /// <summary>
        /// The variables as they stand now, for a condition asked outside a card (a town act's "when", an unlock's, a
        /// hinge's): set for the port of the call ("" at sea), today. While a card is on screen they are that card's and
        /// are left alone.
        /// </summary>
        public EncounterVars VarsNow()
        {
            if (Current == null)
            {
                string port = state.callPort ?? "";
                Prepare(port, Day, port.Length > 0);
            }
            return vars;
        }

        /// <summary>Could this card open now if the player asked for it (a site's "card:&lt;id&gt;" in a places file): it is
        /// a card of the book, its "when" holds here today, and it is past its cool-down (a "once" card is not done).</summary>
        public bool CanOffer(string cardId)
        {
            var card = book.Card(cardId);
            return card != null && card.InMonth(Month) && state.Ready(card, Day) && card.when.Eval(VarsNow());
        }

        /// <summary>The player asks for a card at a site: it goes first in line and opens when the runner next asks (at
        /// once, when nothing else is on screen). False, and nothing done, when it cannot come now (CanOffer).</summary>
        public bool Offer(string cardId)
        {
            if (!CanOffer(cardId))
                return false;
            state.Enqueue(cardId, true);
            return true;
        }

        /// <summary>
        /// Runs a cost and effects outside a card (a town act done, a job kept, a work paid for), through the same engine
        /// and the same seam to the game as a card's choice. `id` stands where the card's id would (what the people
        /// remember it by, and the seed of its dice). Every role the effects name ("npc reeve feeling +10") is the holder
        /// of that role at the port of the call, the same person a card there would bring; at sea nobody is there, and
        /// such an effect does nothing. Returns what happened as plain facts, in order.
        /// </summary>
        public List<string> RunEffects(string id, List<Effect> cost, List<Effect> effects)
        {
            id = id ?? "";
            string port = state.callPort ?? "";
            var cardActors = vars.actors;
            if (Current == null)
                Prepare(port, Day, port.Length > 0);
            var actors = new Dictionary<string, Npc>();
            if (port.Length > 0)
            {
                var facts = Facts(port);
                foreach (var list in new[] { cost, effects })
                {
                    if (list == null)
                        continue;
                    foreach (var e in list)
                        foreach (var v in e.Verbs())
                        {
                            bool names = v.verb == "npc" || v.verb == "hire" || v.verb == "pay_off" || v.verb == "passenger";
                            if (names && v.a.Length > 0 && !actors.ContainsKey(v.a) && book.roles.Role(v.a) != null)
                                actors[v.a] = npcs.Holder(new RoleSpec { key = v.a, role = v.a }, facts, Year, Day);
                        }
                }
            }
            var ctx = new EffectContext
            {
                game = game, state = state, npcs = npcs, words = book.words, vars = vars, merchant = merchant, port = port,
                day = Day, card = id, option = "", actors = actors, roll = roll ?? Dice(id),
            };
            vars.actors = actors;
            try
            {
                Effects.Apply(cost, ctx);
                Effects.Apply(effects, ctx);
                ctx.CloseFeelings();
            }
            finally
            {
                vars.actors = Current != null ? cardActors : new Dictionary<string, Npc>();
            }
            foreach (var who in actors.Values)
                npcs.MarkMet(who.id, merchant);
            return ctx.facts;
        }

        // ------------------------------------------------------------------ the save

        /// <summary>The encounter layer as plain data (Json.Write): its state and the people. A card open and unanswered goes
        /// back first in line, so it opens again after a load.</summary>
        public Dictionary<string, object> Capture()
        {
            var st = state.Capture();
            var d = new Dictionary<string, object>
            {
                ["version"] = 1, ["merchant"] = merchant, ["armed"] = watch.Armed, ["state"] = st, ["npcs"] = npcs.Capture(),
            };
            // the card on screen, unanswered; or the one a load brought back that has not opened again yet (it is still
            // first in the saved line)
            bool open = Current != null && !Current.Chosen;
            string id = open ? Current.card.id : Current == null ? keptCard : null;
            // (the hours it has waited with the clock running: it comes back with them, and does not start its week again)
            d["wait_card"] = open ? Current.card.id : savedWaitCard;
            d["wait_hours"] = open ? WaitedHours : savedWaitHours;
            if (id == null)
                return d;
            if (open && st.TryGetValue("queue", out var q) && q is List<object> line)
                line.Insert(0, id);
            var cast = new Dictionary<string, object>();
            if (open)
            {
                foreach (var a in Current.actors)
                    if (a.Value != null)
                        cast[a.Key] = a.Value.id;
            }
            else if (keptCast != null)
                foreach (var a in keptCast)
                    cast[a.Key] = a.Value;
            // 7 Oct 2026: with its people and the hour it opened at, so it comes back the same card: the same faces, the
            // same odds. (Before, each load drew its people again: the toll man had another name, and one more forged
            // person was left in the registry at every load.)
            d["open"] = new Dictionary<string, object>
            {
                ["card"] = id, ["hour"] = (double)(open ? openedHour : keptHour), ["cast"] = cast,
            };
            return d;
        }

        /// <summary>The encounter layer as saved (null, an older save without it: as at the start). The card on screen
        /// closes; a card that was open when the game was saved opens again as it was (the runner opens the first in line).</summary>
        public void Restore(Dictionary<string, object> d)
        {
            Current = null;
            state.Restore(Json.Obj(d, "state"));
            npcs.Restore(Json.Obj(d, "npcs"));
            watch.Armed = Json.Bool(d, "armed");
            merchant = Json.Str(d, "merchant", merchant);
            keptCard = null;
            keptCast = null;
            keptHour = 0;
            // (an older save has neither key: its open card starts its wait again)
            WaitedHours = 0.0;
            nextTry = 0.0;
            savedWaitCard = Json.Str(d, "wait_card", "");
            savedWaitHours = Math.Max(0.0, Json.Num(d, "wait_hours", 0.0));
            var open = Json.Obj(d, "open");
            string id = open != null ? Json.Str(open, "card", null) : null;
            // (only if it is still the first in line: it always is in a save this code wrote)
            if (!string.IsNullOrEmpty(id) && state.queue.Count > 0 && state.queue[0] == id)
            {
                keptCard = id;
                keptHour = (long)Json.Num(open, "hour", Hour);
                keptCast = new Dictionary<string, string>();
                if (Json.Obj(open, "cast") is Dictionary<string, object> cast)
                    foreach (var a in cast)
                        if (a.Value is string who)
                            keptCast[a.Key] = who;
            }
        }

        // ------------------------------------------------------------------ the variables and the draws

        /// <summary>The people, faith and realm of a port, as the forge and the cards read them.</summary>
        public PortFacts Facts(string port)
        {
            (string realm, string people) info = ("", "");
            if (portInfo != null && !string.IsNullOrEmpty(port))
                info = portInfo(port);
            return book.roles.Facts(port ?? "", info.realm ?? "", info.people ?? "");
        }

        /// <summary>The variables for a draw or a card at a port: the encounter layer's own and the port's facts.</summary>
        void Prepare(string port, double day, bool moored)
        {
            port = port ?? "";
            var facts = Facts(port);
            vars.state = state;
            vars.npcs = npcs;
            vars.merchant = merchant;
            vars.port = port;
            vars.day = day;
            vars.actors = new Dictionary<string, Npc>();
            vars.nums["moored"] = moored ? 1.0 : 0.0;
            vars.nums["under_way"] = !moored && watch.UnderWay ? 1.0 : 0.0;
            vars.nums["year"] = Year;
            vars.nums["month"] = Month;
            vars.nums["port.has_mint"] = facts.hasMint ? 1.0 : 0.0;
            vars.texts["port.realm"] = facts.realm ?? "";
            vars.texts["port.people"] = facts.People;
            vars.texts["port.faith"] = facts.faith ?? "";
        }

        DrawInfo Info(string port, double day, bool moored, double mooredHours)
        {
            port = port ?? "";
            return new DrawInfo
            {
                port = port, month = Month, day = day, moored = moored,
                underWay = !moored && watch.UnderWay, anchored = !moored && !watch.UnderWay, mooredHours = mooredHours,
                standing = standingAt != null && port.Length > 0 ? standingAt(port) : 0.0,
                portFactor = book.PortFactor(port, Month),
            };
        }

        /// <summary>The dice for a card's chance: the world's seed, the card and the game hour (the same on every replay).</summary>
        Func<double> Dice(string card) => Dice(card, Hour);

        /// <summary>The dice of a card that opened at a given game hour (a loaded game's open card keeps its hour).</summary>
        Func<double> Dice(string card, long hour)
        {
            var rnd = Sim.SimRandom.For(seed, "choice/" + card + "/" + hour);
            return () => rnd.Next();
        }
    }
}
