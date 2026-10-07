using System;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.Sailing;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    public enum AgroundStage { None, Offered, Waiting, Rowing, Pushing }

    /// <summary>
    /// AGROUND - she's stuck fast on the way.
    /// FOR NOW (the user, 2026-10-03: "Just make it a reset back into the water and back on course for now the mechanic is
    /// broken and will need a rework"): the crew try the oars by themselves first; if she's still fast after
    /// voyage_rules.json "aground" offer minutes, she's put back afloat on her charted line (Reset) and sails on.
    /// THE CHOICE, kept for the rework but not offered (the user, 2026-10-02: "If you run aground then you gotta get out
    /// somehow or just wait for the tide"): the clock stops and you choose (the graph "Aground - getting her off"):
    ///   WAIT FOR THE TIDE   only where a tide will float her soon enough: she lies with her sail down and time runs fast
    ///                       till the water lifts her;
    ///   ROW HER OFF         the crew row hard for the deepest water near (she moves only where the water deepens);
    ///   OVER THE SIDE       the crew push and haul her to the nearest water deep enough (wet, cold work: health and
    ///                       heart; it always works if there's such water within reach);
    ///   LEAVE HER FOR NOW   the choice comes again later.
    /// Facts only; your writing goes in writing.json under the keys ("aground", "aground.tide" ...). The voyage planner
    /// hands her to this while it's on, and takes up the course again when she floats.
    ///
    /// ported 4 Oct: the first build's GameClock is the level's clock now (Kapman.Levels.LevelClock: GameHours, rate,
    /// Paused); the helmsman (ShipAutopilot) is the ship's own motion (Kapman.Levels.ShipMotion.MakeFor); the tide comes
    /// from the level's water through VoyageTide (TideWait.cs) instead of TideLevel/Tide; the compass helpers that were
    /// Kapman.Sailing.CompassMath are inlined below; Build mode (which held her while you built) is not in the rebuild.
    /// The ship herself is still SailingShip (the shim over ShipMotion), as the task has it.
    /// </summary>
    public static class Aground
    {
        public static AgroundStage Stage { get; private set; }
        /// <summary>The graphs' "On aground": the choice is to be shown (with the facts of how she lies).</summary>
        public static event Action<string> Due;

        static double stuckMinutes, notBefore, until, rateWas;
        static WorldPos pushTo;
        static float pushLengths;
        static readonly List<(string key, string intention, string what)> options = new List<(string, string, string)>();

        static VoyageRules R => Voyage.Rules;

        /// <summary>Game minutes that passed this frame on the level's clock (0 while it's paused).</summary>
        static double MinutesThisFrame(LevelClock clock) =>
            GameSpeed.Stepped ? KapmanGame.StepSeconds / 60.0 : LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 60.0;

        /// <summary>Each frame (the voyage planner, with a course set): watches for her sticking fast, and does what was
        /// chosen. True while it has her (the choice is open, or she's being got off): the course waits.</summary>
        public static bool Tick(SailingShip ship, ShipMotion helm, LevelClock clock)
        {
            if (ship == null || clock == null)
                return false;
            if (Building())
                return Stage != AgroundStage.None;                          // (Build mode holds her: nothing moves her meanwhile)
            double hours = clock.GameHours;
            switch (Stage)
            {
                case AgroundStage.None:
                    Watch(ship, clock);
                    return Stage != AgroundStage.None;
                case AgroundStage.Offered:
                    ship.anchored = true;                                    // (her sail down while you choose)
                    return true;
                case AgroundStage.Waiting:
                    ship.anchored = true;
                    if (Shortfall(ship, ship.transform.position) <= 0f)
                        Done(ship, clock, "The tide has floated her.");
                    else if (hours > until)
                        Failed(ship, clock, "The tide didn't float her.");
                    return true;
                case AgroundStage.Rowing:
                    ship.anchored = false;
                    // ported 4 Oct: was helm.useGoal/rowToGoal/goal; the ship's motion takes one order, a point to make for.
                    // (ShipMotion stops her when the ground ahead is shallower than her draught: rowing off a bank
                    // this way is for the rework, as the whole choice is.)
                    if (helm != null)
                        helm.MakeFor(FloatingOrigin.ToUnity(pushTo));
                    if (Shortfall(ship, ship.transform.position) <= 0f)
                        Done(ship, clock, "They've rowed her off.");
                    else if (hours > until)
                        Failed(ship, clock, "They couldn't row her off.");
                    return true;
                case AgroundStage.Pushing:
                    ship.anchored = true;
                    var to = FloatingOrigin.ToUnity(pushTo);
                    var p = ship.transform.position;
                    var d = new Vector3(to.x - p.x, 0f, to.z - p.z);
                    // (the game's clock may run fast or be held: they walk her at their own pace, in the game's minutes)
                    float step = (float)(R.pushSpeed * MinutesThisFrame(clock));
                    if (d.magnitude <= step)
                    {
                        ship.transform.position = new Vector3(to.x, p.y, to.z);
                        if (Shortfall(ship, ship.transform.position) <= 0f)
                            Done(ship, clock, "They've pushed her off into deep enough water.");
                        // the tide fell away under her while they pushed: on to the next water deep enough, or give up
                        else if (!FindDeepWater(ship, (float)R.pushReachLengths, (float)R.pushMargin, out pushTo, out pushLengths))
                            Failed(ship, clock, "The tide fell away as they pushed: she's still fast.");
                    }
                    else if (Shortfall(ship, p) <= 0f && d.magnitude < ship.shipClass.length)
                        Done(ship, clock, "They've pushed her off into deep enough water.");
                    else
                        ship.transform.position = p + d.normalized * step;
                    return true;
            }
            return false;
        }

        /// <summary>Counts the game minutes she's stuck fast while sailing her course (nothing else on); the choice
        /// comes after a while.</summary>
        static void Watch(SailingShip ship, LevelClock clock)
        {
            double dtMinutes = MinutesThisFrame(clock);
            // (the map or a panel open doesn't stop it: the reset needs no one)
            bool busy = ship.anchored || Sim.Encounter.Stage != Sim.EncounterStage.None || Sim.RaiderEncounter.Stage != Sim.RaidStage.None
                        || Sim.Patrols.Stage != Sim.PatrolStage.None || GameEvents.Current != null;
            var port = Port.Nearest(ship.transform.position);
            bool inPort = port != null && port.DistanceTo(ship.transform.position) < port.arrivalRadius;
            if (!ship.Aground || inPort)
            {
                stuckMinutes = 0.0;
                return;
            }
            if (busy)
                return;
            stuckMinutes += dtMinutes;
            if (stuckMinutes < R.agroundOfferMinutes || clock.GameHours < notBefore)
                return;
            stuckMinutes = 0.0;
            Reset(ship, clock);
        }

        /// <summary>Lengths round her the reset looks for water when her charted line has none afloat.</summary>
        const float ResetReachLengths = 30f;

        /// <summary>
        /// Stuck fast for a while: she's put back afloat on her charted line, heading along it, and sails on (the user,
        /// 2026-10-03: "Just make it a reset back into the water and back on course for now the mechanic is broken and
        /// will need a rework"). The choice (<see cref="Offer"/>, <see cref="Choose(string)"/>) is kept for that rework,
        /// unused.
        /// </summary>
        static void Reset(SailingShip ship, LevelClock clock)
        {
            if (!AfloatOnCourse(ship, out var to, out float heading))
            {
                if (!FindDeepWater(ship, ResetReachLengths, (float)R.pushMargin, out var deep, out _))
                {
                    notBefore = clock.GameHours + R.agroundReofferMinutes / 60.0;   // (no water near: she tries again later)
                    return;
                }
                to = FloatingOrigin.ToUnity(deep);
                heading = ship.Heading;
            }
            var p = ship.transform.position;
            ship.Teleport(new Vector3(to.x, p.y, to.z), heading);
            ship.anchored = false;
            Voyage.Say("She was stuck fast: she's afloat again, back on her course.");
        }

        /// <summary>The first place along her charted line, from the leg she's on (its point nearest her) onward, where
        /// she'd lie afloat with water to spare at bow, middle and stern, heading along the line; false if there's none.
        /// (The line is real metres in the world frame; FloatingOrigin brings it into the level: the hand-off the
        /// level-chain owner wires, see VoyagePlanner.)</summary>
        static bool AfloatOnCourse(SailingShip ship, out Vector3 at, out float heading)
        {
            at = default;
            heading = ship.Heading;
            var line = Course.Points;
            if (line.Count < 2)
                return false;
            float length = ship.shipClass != null ? ship.shipClass.length : 8f, need = ship.NeedsDepth * (float)R.pushMargin;
            int from = Mathf.Clamp(Course.Next, 1, line.Count - 1);
            for (int i = from; i < line.Count; i++)
            {
                var a = FloatingOrigin.ToUnity(new WorldPos(line[i - 1].x, 0.0, line[i - 1].z));
                var b = FloatingOrigin.ToUnity(new WorldPos(line[i].x, 0.0, line[i].z));
                var d = new Vector3(b.x - a.x, 0f, b.z - a.z);
                float len = d.magnitude;
                if (len < 1e-3f)
                    continue;
                var dir = d / len;
                float t0 = 0f;
                if (i == from)
                {
                    var rel = ship.transform.position - a;
                    t0 = Mathf.Clamp(rel.x * dir.x + rel.z * dir.z, 0f, len);
                }
                for (float t = t0; t <= len; t += length * 0.5f)
                {
                    var c = new Vector3(a.x + dir.x * t, ship.transform.position.y, a.z + dir.z * t);
                    float least = Mathf.Min(ship.Depth(c), Mathf.Min(ship.Depth(c + dir * (length * 0.5f)), ship.Depth(c - dir * (length * 0.5f))));
                    if (least >= need)
                    {
                        at = c;
                        heading = HeadingOf(dir);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>The choice comes: the clock stops, her sail comes down, and the graph is told how she lies.</summary>
        public static void Offer(SailingShip ship, LevelClock clock)
        {
            Stage = AgroundStage.Offered;
            GameSpeed.Hold("aground");                                       // ported 4 Oct: was clock.held (GameSpeed owns the time scale)
            ship.anchored = true;
            string facts = Facts(ship, out bool tide, out float tideHours, out bool push, out float lengths);
            options.Clear();
            if (tide)
                options.Add(("aground.tide", $"Wait for the tide (she floats in about {tideHours:0.#} h)", "tide"));
            options.Add(("aground.row", "Row her off", "row"));
            options.Add(push ? ("aground.push", $"Over the side and push her off ({Lengths(lengths)} to water deep enough)", "push")
                             : ("aground.push", "Over the side and push (no water deep enough within reach)", "push"));
            options.Add(("aground.leave", "Leave her for now", "leave"));
            Due?.Invoke(facts);
        }

        /// <summary>The options for the open choice: (key for your writing, intention).</summary>
        public static IEnumerable<(string key, string intention)> Options()
        {
            foreach (var o in options)
                yield return (o.key, o.intention);
        }

        /// <summary>How she lies, as facts: on what, how far short of the water she needs, the tide.</summary>
        public static string Facts(SailingShip ship, out bool tide, out float tideHours, out bool push, out float lengths)
        {
            var p = ship.transform.position;
            float shortBy = Shortfall(ship, p);
            var b = new System.Text.StringBuilder();
            // ported 4 Oct: the ground kinds (Kapman.Land.Ground) aren't named here, so any enum the shim gives works
            string under = ship.GroundUnderKeel.ToString();
            string on = under == "Water" ? "the bottom" : under.ToLowerInvariant();
            b.Append($"She's fast on {on}, {shortBy:0.0} m short of the water she needs ({ship.NeedsDepth:0.0} m).");
            tide = TideWillFloat(ship, shortBy, out tideHours);
            if (!VoyageTide.Known)
                b.Append(" No tide here to speak of.");
            else
            {
                b.Append($" The tide is {(VoyageTide.Rising ? "rising" : "falling")}, {VoyageTide.Level:+0.0;-0.0} m.");
                b.Append(tide ? $" It will float her in about {tideHours:0.#} h." : $" It won't float her in the next {R.tideLookHours:0} h.");
            }
            push = FindDeepWater(ship, (float)R.pushReachLengths, (float)R.pushMargin, out var to, out lengths);
            b.Append(push ? $" Water deep enough {Lengths(lengths)} off." : " No water deep enough within reach.");
            return b.ToString();
        }

        /// <summary>The option chosen by its number in the panel (from 0): <see cref="Choose(string)"/>.</summary>
        public static string Choose(int choice) =>
            Choose(choice >= 0 && choice < options.Count ? options[choice].what : "leave");

        /// <summary>Your choice ("tide", "row", "push", "leave"): it's begun, and what's happening is told as facts.</summary>
        public static string Choose(string what)
        {
            var game = KapmanGame.Current;
            var ship = game != null ? game.ship : null;
            var clock = LevelClock.Current;                                  // ported 4 Oct: was game.clock
            if (Stage != AgroundStage.Offered || ship == null || clock == null)
                return "";
            GameSpeed.Release("aground");
            bool cold = game.Month >= 11 || game.Month <= 3;
            switch (what)
            {
                case "tide":
                    if (!TideWillFloat(ship, Shortfall(ship, ship.transform.position), out float h))
                        return Again(clock, "The tide won't float her now.");
                    Stage = AgroundStage.Waiting;
                    until = clock.GameHours + h + 1.0;
                    rateWas = clock.rate;
                    clock.rate = (float)R.tideWaitRate;
                    Voyage.Affect(0.0, R.waitMorale);
                    return $"You lie and wait for the tide: it should float her in about {h:0.#} h.";
                case "row":
                    if (!FindDeepWater(ship, 4f, 1f, out pushTo, out _))
                        pushTo = FloatingOrigin.ToWorld(ship.transform.position - ToVector(ship.Heading) * ship.shipClass.length * 3f);
                    Stage = AgroundStage.Rowing;
                    until = clock.GameHours + R.rowOffMinutes / 60.0;
                    ship.anchored = false;
                    Voyage.Affect(R.rowHealth, R.rowMorale);
                    return "The crew take to the oars and row hard for the deepest water near.";
                case "push":
                    if (!FindDeepWater(ship, (float)R.pushReachLengths, (float)R.pushMargin, out pushTo, out pushLengths))
                        return Again(clock, "There's no water deep enough within reach to push her to.");
                    Stage = AgroundStage.Pushing;
                    ship.anchored = true;
                    Voyage.Affect(R.pushHealth + (cold ? R.pushColdHealth : 0.0), R.pushMorale);
                    return $"The crew go over the side and push her towards water deep enough, {Lengths(pushLengths)} off.";
                default:
                    return Again(clock, "You leave her as she lies for now.");
            }
        }

        /// <summary>Something else takes her over (raiders, the king's men, a meeting, the course cleared): whatever was
        /// under way stops, the clock runs as it did; if she's still fast, the choice comes again later.</summary>
        public static void Cancel(LevelClock clock, SailingShip ship)
        {
            if (Stage == AgroundStage.None || Stage == AgroundStage.Offered || clock == null)
                return;                                                      // (an open choice is settled by the choice)
            Restore(clock);
            // waiting or pushing, we had her lie to: let her go, and whatever takes her over lays her to as it needs
            if (ship != null && (Stage == AgroundStage.Waiting || Stage == AgroundStage.Pushing))
                ship.anchored = false;
            Stage = AgroundStage.None;
            stuckMinutes = 0.0;
        }

        /// <summary>A saved game loaded: the clock may have gone back, so the timers start again.</summary>
        public static void Loaded()
        {
            if (Stage == AgroundStage.None)
                stuckMinutes = notBefore = until = 0.0;
        }

        /// <summary>Is Build mode on (it held the ship while you built)? ported 4 Oct: the rebuild has no Build mode yet
        /// (the first build's Kapman.Build.BuildMode); nothing holds her for building, so this is always false.</summary>
        static bool Building() => false;

        /// <summary>Nothing begun: she lies as she is (the crew try the oars now and then) and the choice comes again later.</summary>
        static string Again(LevelClock clock, string fact)
        {
            Stage = AgroundStage.None;
            notBefore = clock.GameHours + R.agroundReofferMinutes / 60.0;
            var ship = KapmanGame.Current != null ? KapmanGame.Current.ship : null;
            if (ship != null && Course.IsSet)
                ship.anchored = false;
            return fact;
        }

        static void Done(SailingShip ship, LevelClock clock, string fact)
        {
            Restore(clock);
            Stage = AgroundStage.None;
            stuckMinutes = 0.0;
            ship.anchored = !Course.IsSet;
            Voyage.Say(fact);
        }

        static void Failed(SailingShip ship, LevelClock clock, string fact)
        {
            Restore(clock);
            Voyage.Say(fact);
            Again(clock, fact);                                              // (the choice again after a while, as after leaving her)
            stuckMinutes = 0.0;
        }

        static void Restore(LevelClock clock)
        {
            if (Stage == AgroundStage.Waiting && rateWas > 0.0)
                clock.rate = (float)rateWas;
            rateWas = 0.0;
        }

        /// <summary>"half a boat length", "a boat length", "2.5 boat lengths".</summary>
        static string Lengths(float n) => n <= 0.5f ? "half a boat length" : n <= 1f ? "a boat length" : $"{n:0.#} boat lengths";

        /// <summary>Real metres of water she's short of at a place (bow, middle, stern; her heading as it is): 0 afloat.</summary>
        public static float Shortfall(SailingShip ship, Vector3 at)
        {
            float half = (ship.shipClass != null ? ship.shipClass.length : 8f) * 0.5f, need = ship.NeedsDepth;
            var fwd = ToVector(ship.Heading);
            return Mathf.Max(0f, Mathf.Max(need - ship.Depth(at), Mathf.Max(need - ship.Depth(at + fwd * half), need - ship.Depth(at - fwd * half))));
        }

        /// <summary>Will the tide float her soon enough (within voyage_rules' look-ahead), and in how many game hours?</summary>
        public static bool TideWillFloat(SailingShip ship, float shortBy, out float hours)
        {
            hours = float.NaN;
            if (!VoyageTide.Known)
                return false;
            var (lat, lon) = FloatingOrigin.ToWorld(ship.transform.position).LatLon;
            // her depth counts the level's tide now: the rise she needs, on the tide where she lies
            float level = VoyageTide.HeightNow(lat, lon) + shortBy + 0.05f;
            return VoyageTide.HoursUntil(lat, lon, level, (float)R.tideLookHours, out hours);
        }

        /// <summary>Real metres the ground may stand higher on the way she's pushed than where she lies (over a bar, yes;
        /// over an island into other water, no).</summary>
        const float PushOverRise = 0.3f;

        /// <summary>
        /// The nearest place, within so many of her lengths, where she'd lie afloat with water to spare (`margin` times
        /// what she needs, at bow, middle and stern on her heading), the deepest of the nearest ring that has one, with
        /// no ground on the way there much higher than where she lies; and how many of her lengths off it is.
        /// </summary>
        public static bool FindDeepWater(SailingShip ship, float reachLengths, float margin, out WorldPos at, out float lengths)
        {
            at = default;
            lengths = 0f;
            float length = ship.shipClass != null ? ship.shipClass.length : 8f, need = ship.NeedsDepth * margin, half = length * 0.5f;
            var p = ship.transform.position;
            var fwd = ToVector(ship.Heading);
            float here = ship.Depth(p);
            for (float r = 0.5f; r <= reachLengths; r += 0.5f)
            {
                float best = float.MinValue;
                Vector3 bestAt = default;
                int around = Mathf.Clamp(Mathf.CeilToInt(r * 12f), 16, 96);
                for (int i = 0; i < around; i++)
                {
                    var dir = ToVector(360f * i / around);
                    var c = p + dir * (r * length);
                    float least = Mathf.Min(ship.Depth(c), Mathf.Min(ship.Depth(c + fwd * half), ship.Depth(c - fwd * half)));
                    if (least < need || least <= best)
                        continue;
                    bool way = true;                                         // the way there, every quarter of a length
                    for (float s = 0.25f; s < r && way; s += 0.25f)
                        way = ship.Depth(p + dir * (s * length)) >= here - PushOverRise;
                    if (way)
                    {
                        best = least;
                        bestAt = c;
                    }
                }
                if (best > float.MinValue)
                {
                    at = FloatingOrigin.ToWorld(bestAt);
                    lengths = r;
                    return true;
                }
            }
            return false;
        }

        // ported 4 Oct: the compass helpers that were Kapman.Sailing.CompassMath (a heading in degrees, 0 north, 90 east)
        static Vector3 ToVector(float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
        }

        static float HeadingOf(Vector3 v) => Mathf.Repeat(Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 360f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            Stage = AgroundStage.None;
            stuckMinutes = notBefore = until = rateWas = 0.0;
            options.Clear();
        }
    }
}
