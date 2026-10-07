using System.IO;
using Kapman.Levels;
using Kapman.Sailing;
using Kapman.Weather;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE TIDE AHEAD - what the voyage asks of the tide: how high it stands now, whether it's rising, when it will
    /// first reach a height (aground: will it float her?), and the best high water coming (the flats: she goes over on
    /// it). ported 4 Oct: the first build asked TideLevel (Kapman.Weather) for these; the rebuild's level carries its
    /// tide in LevelWater (Kapman.Levels: the EOT20 model at the level's coast point, scaled by tideShare and put back
    /// by tideLagHours), which answers only "now". So the same TideModel is read here from LevelWater's tides file, and
    /// looked ahead with LevelWater's own point, share and lag, so the answers agree with the water you see. A level is
    /// small enough that its tide is one tide: the lat/lon arguments are kept for the call sites, not used.
    /// Without tides (a river place like Dorestad: tideShare 0, or no tides.bin) nothing is Known: no waiting.
    /// </summary>
    public static class VoyageTide
    {
        static TideModel model;
        static string loadedFrom;
        static bool warned;

        static LevelWater Water => LevelWater.Current;

        /// <summary>Is there a tide here to reckon with (the level has one, and its model is loaded)?</summary>
        public static bool Known
        {
            get
            {
                var w = Water;
                if (w == null || !w.tides || w.tideShare <= 0f || LevelClock.Current == null)
                    return false;
                return Model(w) != null;
            }
        }

        /// <summary>The tide's part of the level's water now (real metres about mean level, as LevelWater has it).</summary>
        public static float Level => Water != null ? Water.Tide : 0f;

        /// <summary>The tide is coming in (looked at five game minutes ahead).</summary>
        public static bool Rising => Known && At(5.0 / 60.0) > At(0.0);

        /// <summary>The tide's height (real metres) at a place now: the level's one tide.</summary>
        public static float HeightNow(double lat, double lon) => Known ? At(0.0) : 0f;

        /// <summary>
        /// Game hours from now until the tide first stands at or above a level (real metres), looking ahead in 6-minute
        /// steps up to maxHours; false if it doesn't by then (or there's no tide). Aground: will the tide float her, and when?
        /// </summary>
        public static bool HoursUntil(double lat, double lon, float level, float maxHours, out float hours)
        {
            hours = float.NaN;
            if (!Known)
                return false;
            for (int i = 0; i * 0.1f <= maxHours; i++)
                if (At(i * 0.1) >= level)
                {
                    hours = i * 0.1f;
                    return true;
                }
            return false;
        }

        /// <summary>The best high water within so many game hours (6-minute steps): when (game hours) and how high (real
        /// metres); false without a tide. TideWait: she goes over the flats on it when no tide gives enough.</summary>
        public static bool NextHighWater(double lat, double lon, float maxHours, out float hours, out float height)
        {
            hours = height = float.NaN;
            if (!Known)
                return false;
            float best = float.MinValue;
            int at = -1;
            for (int i = 1; i * 0.1f <= maxHours; i++)
            {
                float h = At(i * 0.1);
                if (h > best)
                {
                    best = h;
                    at = i;
                }
            }
            if (at < 0)
                return false;
            hours = at * 0.1f;
            height = best;
            return true;
        }

        /// <summary>The level's tide so many game hours from now (as LevelWater works it out: UT at the coast point, the
        /// lag taken off, the share applied).</summary>
        static float At(double hoursAhead)
        {
            var w = Water;
            var clock = LevelClock.Current;
            var m = Model(w);
            if (w == null || clock == null || m == null)
                return 0f;
            double jd = Sky.ToUt(clock.JulianDay, w.tideLongitude) - w.tideLagHours / 24.0 + hoursAhead / 24.0;
            float h = m.HeightAt(w.tideLatitude, w.tideLongitude, jd);
            return float.IsNaN(h) ? 0f : h * w.tideShare;
        }

        /// <summary>The tide model from the level's tides file, read once (again if the level names another file).</summary>
        static TideModel Model(LevelWater w)
        {
            if (w == null)
                return null;
            string path = w.tidesFile;
            if (model != null && loadedFrom == path)
                return model;
            if (loadedFrom == path)
                return null;                                                 // (tried and failed: not again every frame)
            loadedFrom = path;
            model = null;
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    model = TideModel.Load(path);
                else if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Kapman voyage: no tides file at '" + path + "' (LevelWater.tidesFile): no waiting for the tide, no tide to float her off.");
                }
            }
            catch (System.Exception e)
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Kapman voyage: the tides file couldn't be read (" + e.Message + ").");
                }
            }
            return model;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            model = null;
            loadedFrom = null;
            warned = false;
        }
    }

    /// <summary>
    /// WAITING FOR THE TIDE - the flats on her way. The charted course keeps off the flats where there's a way (Course,
    /// the user, 2026-10-03: "Courses avoid flats"); some ports (Witla, Lundenwic) have no way in but over them, and the
    /// user's rule for those: "If there's absolutely no way through then the crew will stop completely and just wait or
    /// beach themselves and camp until the tide is back up". So: before a run of shallow lanes on her line (Course.Depths),
    /// if the water over it now isn't enough for her, she lies to at its edge and time runs fast (voyage_rules "aground"
    /// waiting_rate) until the tide makes enough; if no tide in the look-ahead (tide_look_hours) does, she waits for the
    /// best high water in it and goes over on that - and if she sticks, Aground has her. The same if the tide leaves her
    /// short in the middle of a run: she stops where she is (beached, the crew camping) till the water comes back.
    /// Anything that happens meanwhile (a ship, raiders, the king's men, an event, a panel) ends the wait; she waits
    /// again after. Facts only: Voyage.Say. Without tides there's no waiting (she goes on, and Aground has her if she sticks).
    /// ported 4 Oct: the clock is the level's (Kapman.Levels.LevelClock: rate is its game seconds per real second, as
    /// GameClock's was); the tide is asked of VoyageTide above.
    /// </summary>
    public static class TideWait
    {
        public static bool On { get; private set; }
        /// <summary>The most game seconds a real second while she waits (0: no cap). The chart's frame sets it to the
        /// clock's own pace: no hours are skipped there (the user's ruling of 7 Oct 2026).</summary>
        public static float RateCap;
        /// <summary>Real metres along the line ahead within which a shallow run is looked for (about a quarter of an hour's
        /// sailing): she lies to before it, not in it.</summary>
        public const double LookAheadM = 1500.0;
        /// <summary>How many times the water she needs she wants over the flats (the lanes' depths were sampled on 120 m
        /// cells: a bank's crest can be shallower than its lane says).</summary>
        public const float Margin = 1.25f;
        /// <summary>Game hours after going over on a high water that wasn't enough before the same run is waited for again
        /// (else she'd stop at once).</summary>
        const double GoneOverHours = 6.0;

        /// <summary>The first point of the run she's waiting before (for your writing: -1 none).</summary>
        public static int Run { get; private set; } = -1;
        /// <summary>Game hours until she goes over anyway (the tide won't give enough), as last worked out.</summary>
        public static float HoursToGo { get; private set; }

        static double until, goneOverUntil;
        static int goneOver = -1;
        static float rateWas;
        static bool goAnyway;

        static VoyageRules R => Voyage.Rules;

        /// <summary>Each frame (the voyage planner, with a course set, after Aground): True while she lies waiting (the
        /// course waits).</summary>
        public static bool Tick(SailingShip ship, LevelClock clock, Sim.WorldXZ here)
        {
            if (ship == null || clock == null || !Course.IsSet)
            {
                if (On)
                    End(clock, null);
                return false;
            }
            if (Busy())
            {
                if (On)
                    End(clock, null);                                        // (she waits again when it's done)
                return false;
            }
            bool run = ShallowRunAhead(here, out int first, out int last, out float water, out double metresTo);
            if (On)
            {
                if (!run || first != Run)
                    End(clock, null);                                        // (the course changed under her)
                else if (Enough(ship, here, water))
                {
                    End(clock, "The tide has made: there's water enough over the flats, and she goes on.");
                    ClockDrops.Raise(ClockReason.WaitOver);               // (the wait is over: the clock drops back)
                }
                else if (clock.GameHours >= until)
                {
                    goneOver = Run;
                    goneOverUntil = clock.GameHours + GoneOverHours;
                    End(clock, goAnyway ? "High water, and it's as much as this tide gives: she goes over the flats on it."
                                        : "She goes on over the flats.");
                }
                else
                {
                    ship.anchored = true;
                    HoursToGo = (float)(until - clock.GameHours);
                    return true;
                }
                ship.anchored = false;
                return false;
            }
            if (!run || metresTo > LookAheadM || Enough(ship, here, water))
                return false;
            if (first == goneOver && clock.GameHours < goneOverUntil)
                return false;                                                // (she's going over on what the tide gives)
            return Begin(ship, clock, here, first, last, water);
        }

        /// <summary>Anything going on that waiting would skip past (as Rest).</summary>
        static bool Busy() =>
            (TextPanel.Current != null && TextPanel.Current.Open) || Alerts.Open.Count > 0
            || Sim.Encounter.Stage != Sim.EncounterStage.None || Sim.RaiderEncounter.Stage != Sim.RaidStage.None
            || Sim.Patrols.Stage != Sim.PatrolStage.None || Aground.Stage != AgroundStage.None || GameEvents.Current != null
            || (MainMenu.Current != null && MainMenu.Current.Open);

        /// <summary>Real metres of water she wants over the flats.</summary>
        public static float Needs(SailingShip ship) => ship.NeedsDepth * Margin;

        /// <summary>Is there water enough now over a run whose shallowest lane has `water` metres under mean sea level?</summary>
        static bool Enough(SailingShip ship, Sim.WorldXZ here, float water)
        {
            if (!VoyageTide.Known)
                return true;                                                 // (no tides: nothing to wait for)
            var (lat, lon) = new WorldPos(here.x, 0.0, here.z).LatLon;
            return water + VoyageTide.HeightNow(lat, lon) >= Needs(ship);
        }

        static bool Begin(SailingShip ship, LevelClock clock, Sim.WorldXZ here, int first, int last, float water)
        {
            if (!VoyageTide.Known)
                return false;
            var (lat, lon) = new WorldPos(here.x, 0.0, here.z).LatLon;
            float need = Needs(ship), level = need - water + 0.05f;
            double now = clock.GameHours;
            string flats = first == Mathf.Clamp(Course.Next, 1, Course.Points.Count - 1) ? "The flats she's on" : "The flats ahead";
            if (VoyageTide.HoursUntil(lat, lon, level, (float)R.tideLookHours, out float hours))
            {
                goAnyway = false;
                until = now + hours + 0.5;                                   // (half an hour's slack for the water to make)
                Voyage.Say($"{flats} have too little water for her ({Mathf.Max(0f, water + VoyageTide.HeightNow(lat, lon)):0.0} m; she wants {need:0.0}). " +
                           $"She lies to and waits for the tide: about {hours + 0.5f:0.#} h.");
            }
            else if (VoyageTide.NextHighWater(lat, lon, (float)R.tideLookHours, out hours, out float high))
            {
                goAnyway = true;
                until = now + hours;
                Voyage.Say($"{flats} have too little water for her, and no tide in the next {R.tideLookHours:0} h gives enough " +
                           $"({Mathf.Max(0f, water + high):0.0} m at best; she wants {need:0.0}). She waits for high water, about {hours:0.#} h, and takes her over on it.");
            }
            else
                return false;
            Rest.Interrupt(clock);                                           // (resting's pace would be lost under ours)
            On = true;
            Run = first;
            HoursToGo = (float)(until - now);
            rateWas = clock.rate;
            clock.rate = RateCap > 0f ? Mathf.Min((float)R.tideWaitRate, Mathf.Max(rateWas, RateCap)) : (float)R.tideWaitRate;
            ship.anchored = true;
            Voyage.Affect(0.0, R.waitMorale);
            return true;
        }

        static void End(LevelClock clock, string fact)
        {
            On = false;
            Run = -1;
            if (clock != null && rateWas > 0f)
                clock.rate = rateWas;
            rateWas = 0f;
            if (!string.IsNullOrEmpty(fact))
                Voyage.Say(fact);
        }

        /// <summary>
        /// The next run of shallow legs (lane water under Course.KeepOffBelowM) on her line, from the leg she's on: its
        /// first and last points, the shallowest water of its lanes (real metres under mean sea level), and real metres
        /// along the line from her to its start (0: she's on it). Looks no further than LookAheadM. False: none that near.
        /// </summary>
        public static bool ShallowRunAhead(Sim.WorldXZ here, out int first, out int last, out float water, out double metresTo)
        {
            first = last = -1;
            water = float.PositiveInfinity;
            metresTo = 0.0;
            var pts = Course.Points;
            int n = pts.Count;
            if (n < 2 || Course.Depths.Count != n)
                return false;
            int next = Mathf.Clamp(Course.Next, 1, n - 1);
            double dist = 0.0;
            for (int i = next; i < n; i++)
            {
                if (Course.LegWater(i) < Course.KeepOffBelowM)
                {
                    first = i;
                    metresTo = dist;
                    break;
                }
                dist += i == next ? Sim.WorldXZ.Distance(here, pts[i]) : Sim.WorldXZ.Distance(pts[i - 1], pts[i]);
                if (dist > LookAheadM)
                    return false;
            }
            if (first < 0)
                return false;
            last = first;
            water = Course.LegWater(first);
            while (last + 1 < n && Course.LegWater(last + 1) < Course.KeepOffBelowM)
            {
                last++;
                water = Mathf.Min(water, Course.LegWater(last));
            }
            return true;
        }

        /// <summary>A saved game loaded: the wait is over (she waits again if she's still short of water).</summary>
        public static void Loaded()
        {
            if (On)
                End(LevelClock.Current, null);                               // ported 4 Oct: was KapmanGame.Current.clock
            goneOver = -1;
            goneOverUntil = 0.0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            On = false;
            Run = goneOver = -1;
            until = goneOverUntil = 0.0;
            rateWas = HoursToGo = 0f;
            goAnyway = false;
        }
    }
}
