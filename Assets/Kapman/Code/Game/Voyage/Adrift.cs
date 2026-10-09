using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>How the steersman knows where she is, this minute.</summary>
    public enum Helm
    {
        /// <summary>Made fast, or held (a meeting, aground, a wait): nothing moves her.</summary>
        Fast,
        /// <summary>On a river: the banks hold her.</summary>
        River,
        /// <summary>In narrow water, in harbour water, or with her landfall in sight: he steers by what he sees, and
        /// she is carried only as far as he lets her be.</summary>
        ByEye,
        /// <summary>Along a coast: he steers by reckoning and finds out a part of his error by the land.</summary>
        Coasting,
        /// <summary>On open water: by reckoning alone.</summary>
        Reckoning,
        /// <summary>Lying to in water too deep to anchor in, or in a gale: she is carried.</summary>
        Drifting,
        /// <summary>Lying to her anchor.</summary>
        Anchored,
    }

    /// <summary>
    /// THE SEA MOVES HER (the user, 9 Oct 2026: "We need an intelligent realistic current and weather system that can
    /// blow and push the ship off course and rough ocean areas"; by the picker: "Always adrift ... She never quite holds
    /// a line anywhere. Every passage wanders and you correct it by pointing again", and for a shore: "Strained and held
    /// off").
    ///
    /// Each game minute the tide's stream, the wind's drift and the top of the sea (SeaPush) carry her, besides her own
    /// way. NOBODY STEERS AGAINST IT ON OPEN WATER: the steersman keeps the headings of the course as it was laid, from
    /// where he RECKONS her to be, and she is truly somewhere else by all that has carried her since (<see cref="Set"/>).
    /// What he knows of it:
    ///   - on a river nothing carries her (the banks);
    ///   - in a sound, over the flats, on the way in to a berth, he steers by eye: she is carried a little and
    ///     brought back, and the stream with her or against her is her speed;
    ///   - along a coast he steers by reckoning but finds out a share of his error each hour by the land (less by
    ///     night, none in thick fog);
    ///   - on the open sea by reckoning alone;
    ///   - when the end of the open water (his landfall: a river's mouth, a sound, the berth) comes in sight, he
    ///     knows where he is and her course is laid again from there;
    ///   - when he has run his distance and the landfall is not in sight, he has lost his reckoning: she lies to and
    ///     the clock drops back.
    /// The player sets her right by pointing again: a course laid from where she truly is.
    /// LYING TO with no course she is carried too, unless the water is shallow enough to anchor in and it does not blow
    /// a gale. SHE IS NEVER PUT ON THE LAND: where the push would set her on a shore she is held off it, and when she
    /// has not the way on to keep clear (lying to, a gale, a calm in a tideway) it works her hull.
    /// </summary>
    public static class Adrift
    {
        /// <summary>How the steersman knows her place this minute.</summary>
        public static Helm Helm { get; private set; } = Helm.Fast;
        /// <summary>East (x) and north (z), real metres: how far she truly is from where the steersman reckons her.</summary>
        public static WorldXZ Set { get; private set; }
        public static double SetM => Math.Sqrt(Set.x * Set.x + Set.z * Set.z);
        /// <summary>What carries her this minute, m/s east and north: all of it, the tide's part, the wind's.</summary>
        public static (double east, double north) Push { get; private set; }
        public static (double east, double north) Tide { get; private set; }
        public static (double east, double north) Blow { get; private set; }
        /// <summary>Real metres she is off her line (0 with no course).</summary>
        public static double OffLine { get; private set; }
        /// <summary>She is held off a shore this minute; and it works her hull.</summary>
        public static bool OnShore { get; private set; }
        public static bool Strained { get; private set; }
        /// <summary>Where she was bound when the steersman lost his reckoning (a port's id, or <see cref="Course.PlaceId"/>;
        /// "" none). The Voyage window offers to lay that course again from where she is.</summary>
        public static string LostBound { get; private set; } = "";
        /// <summary>Counts up when something the windows show of this has changed.</summary>
        public static int Version { get; private set; }

        /// <summary>Real metres of set from which a landfall in sight has her course laid again from where she is
        /// (under it he simply steers for what he sees).</summary>
        public const double RelayM = 500.0;
        /// <summary>Game hours between two layings of her course by the steersman after the land has turned her.</summary>
        public const double RelayEveryHours = 0.5;

        // PLACEHOLDER WORDS, the user's to write
        public const string LeeShoreWords = "She is set down on the shore. The men hold her off it with the oars, and it works her.",
                            LeeShoreGaleWords = "A lee shore in a gale: she is driving down on it, and holding her off works her hard.",
                            DriftWords = "The water is too deep to anchor in: lying to, she goes where wind and tide carry her.";

        static bool steered, wantFix, warned, shoreTold, hooked, relaying, driftTold;
        static double shoreClear, sinceRelay = 1e9;
        static WorldXZ? keptSet;
        static string keptFor;

        static void Hook()
        {
            if (hooked)
                return;
            hooked = true;
            Course.Changed -= OnCourse;
            Course.Changed += OnCourse;
        }

        /// <summary>A course was laid, or given up: laid from where she truly is, the steersman knows his place again.</summary>
        static void OnCourse()
        {
            Set = default;
            warned = false;
            wantFix = false;
            OffLine = 0.0;
            if (Course.IsSet)
            {
                LostBound = "";
                // (a saved game: the set she had on this same course)
                if (keptSet.HasValue && keptFor == Key())
                    Set = keptSet.Value;
            }
            keptSet = null;
            Version++;
        }

        static string Key() => Course.IsSet ? Course.Destination + ":" + Course.Points.Count : "";

        /// <summary>How far he can make out a landfall now, real metres: less by night, and in thick weather no
        /// further than the air lets him see.</summary>
        public static double Sight() => Sight(ChartWorld.ShipWorld());

        static double Sight(WorldXZ here)
        {
            var r = SeaPush.Rules;
            // (the graph "Rule - how far he makes out a landfall")
            bool thick = ChartWorld.TryWeather(out var air) && (air.fog > 0.3f || air.rain > 2f);
            return GameRules.Landfall(r.landfallM, Night(here), r.landfallNight, thick, thick ? air.VisibilityKm * 1000.0 : 0.0);
        }

        /// <summary>
        /// Is it night where she is, by the clock's own minute (the Sun under the line that resting takes for dark)?
        /// Reckoned from the clock and her place, not read from what the frame last drew: the same game ends the
        /// same at every speed of the clock (ChartGameTests holds that, and caught a metre's difference when this
        /// read the frame's own night).
        /// </summary>
        static bool Night(WorldXZ here)
        {
            var (lat, lon) = WorldFrame.ToLatLon(here.x, here.z);
            return Kapman.Game.ShipView.StageSky.At(SimStage.JulianDay, lat, lon).sunAltitude < Rest.DarkBelow;
        }

        /// <summary>
        /// WHERE THE STEERSMAN STEERS BY (the voyage planner asks, under way on a course, before it aims her): her own
        /// place where he knows it, else the place he reckons her at. `hours`: the game hours of the step. `lost`: he
        /// has run his distance and his landfall is not in sight (the planner lays her to).
        /// </summary>
        public static WorldXZ SteersBy(WorldXZ here, double hours, out bool lost)
        {
            Hook();
            lost = false;
            steered = true;
            sinceRelay += Math.Max(0.0, hours);
            var r = SeaPush.Rules;
            int end = Course.Points.Count - 1;
            if (!r.on || !Course.IsSet || end < 1)
            {
                Know(Helm.ByEye);
                return here;
            }
            // (the land turned her last minute: he takes his bearings by it, and her course is laid again from here)
            if (wantFix)
            {
                wantFix = false;
                if (sinceRelay >= RelayEveryHours)
                    Relay(here);
                end = Course.Points.Count - 1;
                if (!Course.IsSet || end < 1)
                    return here;
            }
            int leg = Math.Max(1, Math.Min(end, Course.Next));
            bool reckoning = Helm == Helm.Reckoning || Helm == Helm.Coasting;
            double sight = Sight(here);
            if (!Course.LegIsOpen(leg))
            {
                // by his reckoning she is out of the open water: in the river's mouth, the sound, on the way in
                if (reckoning && SetM > sight)
                {
                    lost = true;
                    return here;
                }
                if (reckoning && SetM > RelayM)
                {
                    Relay(here);                                         // (the mouth is in sight: from where she truly is)
                    Know(Helm.ByEye);
                    return here;
                }
                byte kind = Course.LegKind(leg);
                Know(kind == LaneGraph.River || kind == LaneGraph.Pending ? Helm.River : Helm.ByEye);
                return here;
            }
            int runEnd = Course.OpenRunEnd(leg);
            var mark = Course.Points[runEnd];
            bool noMark = runEnd == end && Course.ToPlace;                // (a place pointed at in open water has nothing to see)
            if (!noMark && WorldXZ.Distance(here, mark) <= sight)
            {
                // his landfall is in sight: he knows where he is
                if (SetM > RelayM)
                    Relay(here);
                Know(Helm.ByEye);
                return here;
            }
            bool coast = Course.LegIsCoastal(leg);
            if (coast && hours > 0.0)
            {
                // along a coast the land tells him a share of his error each hour
                // (how much of it: the graph "Rule - what he keeps of his error along a coast")
                bool thick = ChartWorld.TryWeather(out var air) && air.fog > 0.6f;
                double keep = GameRules.ErrorKept(hours, Night(here), thick, r.coastEye, r.coastEyeNight);
                Set = new WorldXZ(Set.x * keep, Set.z * keep);
            }
            if (Helm != (coast ? Helm.Coasting : Helm.Reckoning))
                Version++;
            Helm = coast ? Helm.Coasting : Helm.Reckoning;
            var by = new WorldXZ(here.x - Set.x, here.z - Set.z);
            if (!noMark && SetM > sight && WorldXZ.Distance(by, mark) < 400.0)
            {
                lost = true;
                return here;
            }
            return by;
        }

        /// <summary>He knows where she is: no set.</summary>
        static void Know(Helm helm)
        {
            if (Helm != helm || Set.x != 0.0 || Set.z != 0.0)
                Version++;
            Helm = helm;
            Set = default;
        }

        /// <summary>Her course laid again from where she truly is, to where she was bound (kept as it was if no way is
        /// found from here).</summary>
        static void Relay(WorldXZ here)
        {
            if (relaying || !Course.IsSet)
                return;
            relaying = true;
            sinceRelay = 0.0;
            try
            {
                Course.SetTo(Course.Destination, here, null);
            }
            finally
            {
                relaying = false;
            }
            Set = default;
        }

        /// <summary>The steersman has lost his reckoning on the way to a port (the planner says so and lays her to):
        /// kept, so the Voyage window can offer that course again from where she is.</summary>
        public static void Lost(string bound)
        {
            LostBound = bound ?? "";
            Helm = Helm.Drifting;
            driftTold = true;
            Version++;
        }

        /// <summary>What the log says then. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string LostWords(string boundName) =>
            "By the steersman's reckoning she should be " + (string.IsNullOrEmpty(boundName) ? "in with the land" : "off " + boundName) +
            " by now, and he cannot make it out. She lies to.";

        /// <summary>What the log says when she comes, by reckoning, to a place pointed at that she is truly well off.</summary>
        public const string PlaceReckonedWords = "She lies to where the steersman reckons the place you pointed at to be.";

        /// <summary>
        /// SHE IS CARRIED (the chart's body asks, with her own way of the step): where she is after it, in the scene.
        /// `own`: what her own way moves her, scene metres. `heading`, `speed` (real m/s through the water), the wind
        /// where she is, whether her sail draws, whether she lies to for a gale.
        /// </summary>
        public static Vector3 Carry(Vector3 pos, Vector3 own, float heading, float speed, float gameSeconds, float wind, float windFrom, bool sailDrawing, bool lyingTo)
        {
            Hook();
            bool reckoned = steered;
            steered = false;
            Push = Tide = Blow = (0.0, 0.0);
            OnShore = Strained = false;
            var r = SeaPush.Rules;
            var game = KapmanGame.Current;
            if (!r.on || game == null || game.ship == null || gameSeconds <= 0f)
                return pos + own;
            // what holds her: a berth, a meeting, the ground, a wait
            var hold = SeaHolds.Now;
            if (Port.LyingId() != null || (hold != SeaHold.None && hold != SeaHold.TraderOffered && hold != SeaHold.RaidersComing)
                || Aground.Stage != AgroundStage.None || TideWait.On || Rest.On || Kapman.Game.Frame.FrameWait.On)
            {
                Know(Helm.Fast);
                return pos + own;
            }
            bool anchored = game.ship.anchored;
            // harbour water is sheltered
            var port = Port.Nearest(pos);
            if (port != null && port.DistanceTo(pos) < port.arrivalRadius)
            {
                Know(Helm.ByEye);
                return pos + own;
            }
            double scale = FloatingOrigin.HorizontalScale;
            var w = FloatingOrigin.ToWorld(pos);
            var at = new WorldXZ(w.x, w.z);
            if (anchored)
            {
                // lying to with no course to be off: carried, unless she can anchor
                if (ChartWorld.OnRiver())
                {
                    Know(Helm.River);
                    return pos + own;
                }
                // (the graph "Rule - can she lie to her anchor")
                if (GameRules.CanAnchor(wind, GaleDice.GaleWind, SeaPush.Depth(at), r.anchorDepthM))
                {
                    Know(Helm.Anchored);
                    return pos + own;
                }
                // (said once each time she is laid to in water she cannot anchor in: the player should know she moves)
                if (Helm != Helm.Drifting && !driftTold)
                {
                    driftTold = true;
                    Voyage.Say(DriftWords);
                }
                Know(Helm.Drifting);
            }
            else
            {
                driftTold = false;
                if (!reckoned)
                    Know(ChartWorld.OnRiver() ? Helm.River : Helm.ByEye);    // (under way with no course: making for a ship, running)
            }
            if (Helm == Helm.River)
                return pos + own;

            // what carries her
            (double east, double north) tide, blow = (0.0, 0.0);
            if (SeaPush.Fixed != null)
                tide = SeaPush.Fixed(at);
            else
            {
                tide = SeaPush.Stream(at, SimStage.JulianDay);
                blow = SeaPush.Wind(heading, speed, windFrom, wind, sailDrawing && !anchored, lyingTo || anchored);
            }
            Tide = tide;
            Blow = blow;
            Push = (tide.east + blow.east, tide.north + blow.north);
            if (Helm == Helm.ByEye)
            {
                // by eye he lets her be carried only along her way: the stream with her or against her is her speed.
                // (In narrow water a stream runs along the channel; and the tide model, a quarter of a degree coarse,
                // does not know which way a channel lies.)
                double h = heading * Math.PI / 180.0, fe = Math.Sin(h), fn = Math.Cos(h);
                double along = Push.east * fe + Push.north * fn;
                Push = (fe * along, fn * along);
            }
            double pe = Push.east * gameSeconds, pn = Push.north * gameSeconds;
            double ownE = own.x * scale, ownN = own.z * scale;
            WorldXZ to;
            bool turned = false;
            if (!Course.KnowsWater)
                to = new WorldXZ(at.x + ownE + pe, at.z + ownN + pn);         // (no bake to ask: nothing to keep her off)
            else if (!Course.IsWater(at.x, at.z))
            {
                // on a lane the bake calls land (a channel too narrow for it): she keeps her line and nothing sets her
                Push = Tide = Blow = (0.0, 0.0);
                to = new WorldXZ(at.x + ownE, at.z + ownN);
            }
            else
            {
                // her own way: trusted on a lane of her course (the lanes are water, whatever the bake makes of a narrow
                // channel). Off her line the land may lie across it; and so it may on the stretch from open water to
                // the lanes, which is no lane. (The first shore test: driven into the old Rhine mouth behind its spit
                // of dunes, her course was laid again from there, and that stretch took her across the spit.)
                var ownTo = new WorldXZ(at.x + ownE, at.z + ownN);
                bool onLane = Course.IsSet && (Course.Next >= 2 || Course.Points.Count < 2
                                               || WorldXZ.Distance(Course.Points[0], Course.Points[1]) <= Course.OwnLaneM);
                bool offLine = (Helm == Helm.Reckoning || Helm == Helm.Coasting) && (SetM > Course.OwnLaneM * 0.6 || !onLane);
                if (offLine && (ownE != 0.0 || ownN != 0.0) && !Clear(at, ownTo))
                {
                    turned = true;
                    ownTo = BearAway(at, ownE, ownN);
                }
                var full = new WorldXZ(ownTo.x + pe, ownTo.z + pn);
                if ((pe == 0.0 && pn == 0.0) || Clear(ownTo, full))
                    to = full;
                else
                {
                    // the push would set her on the shore: she is held off it
                    OnShore = true;
                    to = BearAway(ownTo, pe, pn, 0.7);
                }
            }

            if (anchored)
            {
                // (a boat lying to is not carried into a harbour: lying to inside a port's reach is being made fast
                // there, and nobody has taken her lines)
                var into = FloatingOrigin.ToUnity(new WorldPos(to.x, 0.0, to.z));
                var haven = Port.Nearest(into);
                if (haven != null && haven.DistanceTo(into) < haven.arrivalRadius)
                    to = at;
            }

            // a shore she has not the way on to keep clear of works her hull
            double hours = gameSeconds / 3600.0;
            if (OnShore)
            {
                shoreClear = 0.0;
                double push = Math.Sqrt(Push.east * Push.east + Push.north * Push.north);
                bool gale = wind > GaleDice.GaleWind;
                // (what it costs her hull, or nothing when she keeps clear: the graph "Rule - what a lee shore costs her hull")
                double strain = GameRules.LeeShore(lyingTo || anchored, speed, push, r.clawOff, gale, r.shoreStrainHour, r.shoreStrainGale, hours);
                if (strain > 0.0)
                {
                    Strained = true;
                    Boat.HullBy(-strain);
                    if (!shoreTold)
                    {
                        shoreTold = true;
                        Voyage.Say(gale ? LeeShoreGaleWords : LeeShoreWords);
                        ClockDrops.Raise(ClockReason.LeeShore);
                        Version++;
                    }
                }
            }
            else
            {
                shoreClear += hours;
                if (shoreTold && shoreClear > 0.5)
                {
                    shoreTold = false;
                    Version++;
                }
            }
            if ((turned || OnShore) && reckoned && (Helm == Helm.Reckoning || Helm == Helm.Coasting))
                wantFix = true;

            // what she was carried is what the steersman does not know of
            if (Helm == Helm.Reckoning || Helm == Helm.Coasting)
                Set = new WorldXZ(Set.x + (to.x - at.x) - ownE, Set.z + (to.z - at.z) - ownN);
            else
                Set = default;

            // off her line: said once for each laying of her course, and the clock drops back
            if (Course.IsSet && reckoned)
            {
                OffLine = Course.OffLine(to);
                if (!warned && OffLine > r.offLineWarnM && (Helm == Helm.Reckoning || Helm == Helm.Coasting))
                {
                    warned = true;
                    Voyage.Say(OffLineWords(OffLine, Set));
                    ClockDrops.Raise(ClockReason.OffLine);
                    Version++;
                }
                else if (warned && OffLine < r.offLineWarnM * 0.5)
                    warned = false;
            }
            else
                OffLine = 0.0;
            var u = FloatingOrigin.ToUnity(new WorldPos(to.x, 0.0, to.z));
            return new Vector3(u.x, pos.y + own.y, u.z);
        }

        /// <summary>What the log says when she is well off her line. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string OffLineWords(double metres, WorldXZ set)
        {
            string airt = SeaPush.Sets(set.x, set.z);
            return "Wind and tide have set her " + Math.Round(metres / 1000.0) + " km " + (airt.Length > 0 ? "to the " + airt + " of" : "off") + " her line.";
        }

        /// <summary>Is the straight water between two places water all the way (asked every 60 m, and at its end)?</summary>
        static bool Clear(WorldXZ a, WorldXZ b)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(WorldXZ.Distance(a, b) / 60.0));
            for (int i = 1; i <= steps; i++)
            {
                double k = (double)i / steps;
                if (!Course.IsWater(a.x + (b.x - a.x) * k, a.z + (b.z - a.z) * k))
                    return false;
            }
            return true;
        }

        static readonly double[] Turns = { 25.0, -25.0, 50.0, -50.0, 75.0, -75.0, 100.0, -100.0 };

        /// <summary>
        /// A step that the land lies across, turned along the shore: the nearest turn of it, to one side or the other,
        /// that is water all the way (what is left of the step along that way: its length times `keep` and the cosine
        /// of the turn). Where she stands if there is none.
        /// </summary>
        static WorldXZ BearAway(WorldXZ at, double east, double north, double keep = 1.0)
        {
            foreach (double turn in Turns)
            {
                double a = turn * Math.PI / 180.0, c = Math.Cos(a), s = Math.Sin(a);
                double f = keep * Math.Max(0.15, c);
                var to = new WorldXZ(at.x + (east * c + north * s) * f, at.z + (-east * s + north * c) * f);
                if (Clear(at, to))
                    return to;
            }
            return at;
        }

        // ------------------------------------------------------------------ a saved game

        static object Capture() => new Dictionary<string, object>
        {
            ["east"] = Set.x, ["north"] = Set.z, ["for"] = Key(), ["warned"] = warned, ["lost"] = LostBound,
        };

        static void Restore(object saved)
        {
            Set = default;
            keptSet = null;
            keptFor = null;
            warned = false;
            LostBound = "";
            if (saved != null)
            {
                var set = new WorldXZ(Json.Num(saved, "east"), Json.Num(saved, "north"));
                string key = Json.Str(saved, "for");
                warned = Json.Bool(saved, "warned");
                LostBound = Json.Str(saved, "lost");
                // (her course is restored before or after this part, whichever: the set is hers on that same course)
                if (key.Length > 0 && key == Key())
                    Set = set;
                else if (key.Length > 0)
                {
                    keptSet = set;
                    keptFor = key;
                }
            }
            Version++;
        }

        /// <summary>A test's: as a new game has it.</summary>
        public static void Forget()
        {
            Set = default;
            Helm = Helm.Fast;
            Push = Tide = Blow = (0.0, 0.0);
            OffLine = 0.0;
            OnShore = Strained = false;
            LostBound = "";
            steered = wantFix = warned = shoreTold = relaying = driftTold = false;
            shoreClear = 0.0;
            sinceRelay = 1e9;
            keptSet = null;
            keptFor = null;
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Forget();
            Course.Changed -= OnCourse;
            hooked = false;
            Hook();
            ContentSave.Register("adrift", Capture, Restore);
        }
    }
}
