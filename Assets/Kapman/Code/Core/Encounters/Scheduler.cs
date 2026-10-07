using System;
using System.Collections.Generic;
using Kapman.Game.Sim;

namespace Kapman.Encounters
{
    /// <summary>Where the merchant is and how things stand, for a draw (the runner fills it from the live game).</summary>
    public struct DrawInfo
    {
        /// <summary>The port of the call ("" at sea).</summary>
        public string port;
        public int month;
        /// <summary>Game days since the start.</summary>
        public double day;
        /// <summary>Lying at a berth (a call in progress); under way; lying still away from a berth (at anchor).</summary>
        public bool moored, underWay, anchored;
        /// <summary>Game hours since the ship moored this call.</summary>
        public double mooredHours;
        /// <summary>His standing at the port, -100..100.</summary>
        public double standing;
        /// <summary>m_port: the port's traffic this month (EncounterBook.PortFactor).</summary>
        public double portFactor;
    }

    /// <summary>
    /// THE SCHEDULER (npc_text_encounters.md 2.3). Harbour cards come per port call: on arrival the ARRIVAL SET (at most two
    /// cards, in the file's order: the toll man where there is a toll, the berth when it matters), then ONE VISIT DRAW over
    /// the drawn harbour cards. Sea and crew cards, and the harbour cards once the ship has lain more than a day, come from
    /// the HOURLY DRAW. Each draw is one Poisson process over its pool: with weights w_i (weight x m_standing x m_season x
    /// m_port), lambda = sum w_i, a card comes with p = 1 - exp(-lambda) and is card i with w_i / lambda (card i's chance a call is
    /// (w_i / lambda)(1 - exp(-lambda)): its weight's share of the one process, about its weight while lambda is small). Every draw is seeded by the world's seed and the hour (SimRandom), so a reloaded save draws
    /// the same; an hour already drawn is never drawn again (a backward jump fires nothing until the clock climbs past it),
    /// and a jump of a month is its 720 hours drawn one by one, exactly as if played. A card waits in the queue until no
    /// panel is open; an hourly draw made while one waits is dropped.
    /// </summary>
    public sealed class Scheduler
    {
        public const int ArrivalMax = 2;
        /// <summary>A long stay: past this many hours moored the harbour pool is drawn every hour too, at weight / 72.</summary>
        public const double LongStayHours = 24.0, LongStayDivisor = 72.0;
        /// <summary>A ship lying still at sea (at anchor) meets a third of what she meets under way.</summary>
        public const double AnchoredFactor = 0.3;

        public readonly EncounterBook book;
        public readonly EncounterState state;
        readonly int seed;

        public Scheduler(EncounterBook book, EncounterState state, int seed)
        {
            this.book = book;
            this.state = state;
            this.seed = seed;
        }

        /// <summary>The card first in line to open (null: none).</summary>
        public EncounterDef Next => state.queue.Count > 0 ? book.Card(state.queue[0]) : null;

        /// <summary>Takes the first card in line off it (it is opening).</summary>
        public EncounterDef TakeNext()
        {
            while (state.queue.Count > 0)
            {
                var c = book.Card(state.queue[0]);
                state.queue.RemoveAt(0);
                if (c != null)
                    return c;
            }
            return null;
        }

        /// <summary>A card was shown: its cool-down starts, a "once" card is done.</summary>
        public void Shown(EncounterDef card, double day) => state.Shown(card, day);

        /// <summary>
        /// The ship has moored: the call begins (the port's calls counted), the arrival set goes in line, then the visit
        /// draw's card if one comes. Returns the cards put in line, in order.
        /// </summary>
        public List<EncounterDef> Arrive(IVars vars, DrawInfo info, long hour)
        {
            state.BeginCall(info.port, info.day);
            var all = ArrivalSet(vars, info);
            var visit = Visit(vars, info, hour);
            if (visit != null)
                all.Add(visit);
            foreach (var c in all)
                state.Enqueue(c.id);
            return all;
        }

        /// <summary>The arrival cards that hold now and are ready, in the file's order, at most two.</summary>
        public List<EncounterDef> ArrivalSet(IVars vars, DrawInfo info)
        {
            var set = new List<EncounterDef>();
            foreach (var c in book.cards)
            {
                if (set.Count >= ArrivalMax)
                    break;
                if (c.arrival && c.stage == Stage.Harbour && c.InMonth(info.month) && state.Ready(c, info.day) && c.when.Eval(vars))
                    set.Add(c);
            }
            return set;
        }

        /// <summary>m_standing: a card that wants a good name comes more where he is well regarded, one that wants a bad
        /// name where he is not; 1 for the rest.</summary>
        public static double StandingFactor(EncounterDef c, double standing)
        {
            if (c.wants == "good_name")
                return Math.Max(0.0, 1.0 + standing / 200.0);
            if (c.wants == "bad_name")
                return Math.Max(0.0, 1.0 - standing / 200.0);
            return 1.0;
        }

        /// <summary>A drawn harbour card's weight this call: weight x m_standing x m_season x m_port (0 if it can't come).</summary>
        public double VisitWeight(EncounterDef c, IVars vars, DrawInfo info)
        {
            if (c.stage != Stage.Harbour || c.arrival || c.weight <= 0.0 || !c.InMonth(info.month) || !state.Ready(c, info.day))
                return 0.0;
            if (!c.when.Eval(vars))
                return 0.0;
            return c.weight * StandingFactor(c, info.standing) * info.portFactor;
        }

        /// <summary>The visit draw: one card or none for this call, seeded by the port and the hour.</summary>
        public EncounterDef Visit(IVars vars, DrawInfo info, long hour)
        {
            var pool = new List<(EncounterDef card, double w)>();
            foreach (var c in book.cards)
            {
                double w = VisitWeight(c, vars, info);
                if (w > 0.0)
                    pool.Add((c, w));
            }
            var rnd = SimRandom.For(seed, $"visit/{info.port}/{hour}");
            return Draw(pool, ref rnd);
        }

        /// <summary>A card's weight in the hourly draw now (0 if it can't come this hour).</summary>
        public double HourlyWeight(EncounterDef c, IVars vars, DrawInfo info)
        {
            if (c.arrival || c.bySim || !c.InMonth(info.month) || !state.Ready(c, info.day))
                return 0.0;
            double w;
            switch (c.stage)
            {
                case Stage.Sea:
                    w = c.rate * (info.underWay ? 1.0 : info.anchored ? AnchoredFactor : 0.0);
                    break;
                case Stage.Crew:
                    w = c.rate;
                    break;
                case Stage.Harbour:
                    w = info.moored && info.mooredHours > LongStayHours ? c.weight / LongStayDivisor * info.portFactor : 0.0;
                    break;
                default:
                    w = 0.0;
                    break;
            }
            if (w <= 0.0)
                return 0.0;
            w *= StandingFactor(c, info.standing);
            return w > 0.0 && c.when.Eval(vars) ? w : 0.0;
        }

        /// <summary>
        /// One game hour (KapmanGame's HourPassed, through the tick ladder): flags that have lapsed go, follow-up cards that
        /// are due (and at their port) go in line, and the hourly draw is made. Returns the card the draw put in line, or
        /// null. An hour at or before the last one drawn does nothing.
        /// </summary>
        public EncounterDef Tick(long hour, IVars vars, DrawInfo info)
        {
            if (hour <= state.lastHour)
                return null;
            state.lastHour = hour;
            state.ExpireFlags(info.day);
            for (int i = 0; i < state.scheduled.Count; i++)
            {
                var s = state.scheduled[i];
                bool here = s.port.Length == 0 || (info.moored && s.port == info.port);
                if (info.day < s.day || !here || book.Card(s.card) == null)
                    continue;
                state.Enqueue(s.card);
                state.scheduled.RemoveAt(i--);
            }
            var pool = new List<(EncounterDef card, double w)>();
            foreach (var c in book.cards)
            {
                double w = HourlyWeight(c, vars, info);
                if (w > 0.0)
                    pool.Add((c, w));
            }
            var rnd = SimRandom.For(seed, "hour/" + hour);
            var drawn = Draw(pool, ref rnd);
            if (drawn == null || state.queue.Count > 0)
                return null;                                  // (a card already waits: this one is dropped)
            state.Enqueue(drawn.id);
            return drawn;
        }

        /// <summary>One Poisson process over a pool: a card comes with 1 - exp(-lambda), and is card i with w_i / lambda.</summary>
        public static EncounterDef Draw(List<(EncounterDef card, double w)> pool, ref SimRandom rnd)
        {
            double lambda = 0.0;
            foreach (var p in pool)
                lambda += p.w;
            double comes = rnd.Next(), which = rnd.Next();
            if (lambda <= 0.0 || comes >= 1.0 - Math.Exp(-lambda))
                return null;
            double at = which * lambda;
            foreach (var p in pool)
            {
                at -= p.w;
                if (at < 0.0)
                    return p.card;
            }
            return pool[pool.Count - 1].card;
        }
    }
}
