using Kapman.Levels;
using Kapman.Sailing;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE VOYAGE PLANNER - takes the ship where the merchant has chosen (Course): it gives her the next point of the
    /// charted line each frame, has the crew lower the sail and row in for the last stretch to the berth, and clears the
    /// course when she's lying there (the port's graph has moored her by then). With no course she doesn't wander: cast
    /// off without one and she lies to off the berth while the map opens for you to choose where to sail; once you've
    /// chosen, she goes. At sea with no course she lies to as well.
    /// Meeting a ship (Encounter): she makes for the other ship, rowing the last of it, lies to alongside while they
    /// talk, and when they part takes up her course again from where she is.
    ///
    /// ported 4 Oct, THE 3D HAND-OFF (for the level-chain owner): the charted line is real metres in the world frame
    /// (WorldFrame), as the chart and the lanes have it. The first build put the ship in that frame through
    /// FloatingOrigin (one world, 1:32); the rebuild has one level scene at a time, a river place at 1:1 or an ocean
    /// shortened 1:150, each at its own place in the world. Every place this file turns the ship's scene position into
    /// world metres or a point of the line into a scene position goes through FloatingOrigin.ToWorld/ToUnity, and that
    /// works when the level scene carries a FloatingOrigin whose horizontalScale is LevelInfo.realMetresPerUnit and whose
    /// origin stands at the level's world x/z (FloatingOrigin.MoveOriginTo(x, z) of the scene's (0, 0)). Until a level
    /// has one, this planner leaves the ship to the player's click orders and says so once (no course is sailed in 3D;
    /// the chart's course, days and legs still work). The helmsman (ShipAutopilot: sail, tacking, a corridor along the
    /// line) is gone with the first build's sailing model: the ship's own motion (Kapman.Levels.ShipMotion) takes one
    /// order, a point to make for, and goes at the level's speed; "rowing" is the same order (no sail to lower yet).
    /// </summary>
    public class VoyagePlanner : MonoBehaviour
    {
        [Tooltip("Game metres from a point of the line at which she makes for the next (a 3D level; on the chart see chartAheadM).")]
        public float passGameM = 15f;
        [Tooltip("On the chart: real metres ahead of her own place on the charted line that she steers for (Course.Aim), so she keeps to the line round every bend of a river.")]
        public float chartAheadM = 300f;
        [Tooltip("Game metres from the berth at which the sail comes down and they row in.")]
        public float rowInGameM = 60f;
        [Tooltip("Game metres from the berth within which she has arrived (as the ports' reach).")]
        public float arrivedGameM = 30f;

        SailingShip ship;
        ShipMotion helm;
        bool held;                                 // lying to for want of a course (we anchored her, not a port)
        bool wasMoored;
        bool meeting;                              // making for (or lying alongside) a ship met at sea
        Port lastMoored;                           // the port she last lay at (casting off from it: the officers)
        Port searchedAt;                           // the port whose officers have just searched her

        static bool frameWarned;
        static FloatingOrigin frame;

        /// <summary>Does she lie moored in a port with its business open (null: nobody says; the chart's frame sets it)?
        /// While she does, a course that is set waits for the cast off.</summary>
        public static System.Func<bool> Moored;

        /// <summary>Does this level stand in the world frame (a FloatingOrigin placed at its real-world position), so
        /// the charted line can be sailed in it? Warns once a level when not.</summary>
        static bool FrameReady()
        {
            if (frame == null)
                frame = FindAnyObjectByType<FloatingOrigin>();
            if (frame != null)
                return true;
            if (!frameWarned)
            {
                frameWarned = true;
                var info = LevelInfo.Current;
                Debug.LogWarning("Kapman voyage: the level '" + (info != null ? info.levelId : "?") + "' has no FloatingOrigin placing it in the world frame: " +
                                 "the charted course isn't sailed in 3D here (she takes click orders). The level-chain owner: add a FloatingOrigin with " +
                                 "horizontalScale = LevelInfo.realMetresPerUnit and its origin at the level's world x/z (WorldFrame.ToWorld of the scene's (0,0)).");
            }
            return false;
        }

        /// <summary>Gives the ship a point to make for (scene metres). `row`: the first build lowered the sail and rowed;
        /// the ship's motion has no sail yet, so it's the same order, kept for the day it has one.</summary>
        void Steer(Vector3 goal, bool row, string why)
        {
            helm.MakeFor(goal);
        }

        /// <summary>Takes the order back: she makes for where she is, which the ship's motion reads as "there" and stops.
        /// (ShipMotion has no way to clear its goal from outside.)</summary>
        void Hold()
        {
            if (helm.Goal.HasValue)
                helm.MakeFor(helm.transform.position);
        }

        /// <summary>The planner at work (the game's clock steps it: KapmanGame.StepMinute).</summary>
        public static VoyagePlanner Current { get; private set; }
        double stepHours;                          // game hours of the step (or the frame) being run

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            // the stepped clock (the chart) runs the planner once a game minute, from its own step. While that clock
            // stands (paused, a card) the planner still looks each frame, with no time passing, as it always did
            if (GameSpeed.Stepped && !GameSpeed.Paused)
                return;
            var clock = LevelClock.Current;
            stepHours = GameSpeed.Stepped || clock == null || LevelClock.Paused ? 0.0 : Time.deltaTime * clock.rate * LevelClock.Speed / 3600.0;
            Run();
        }

        /// <summary>One step of the game's clock: her orders for the minute to come.</summary>
        public void Step(double gameSeconds)
        {
            stepHours = gameSeconds / 3600.0;
            Run();
        }

        void Run()
        {
            var game = KapmanGame.Current;
            ship = game != null ? game.ship : null;
            if (ship == null)
                return;
            // ported 4 Oct: was ship.GetComponent<ShipAutopilot>(); the player's ship's own motion steers her now
            helm = helm != null ? helm : ShipMotion.Player ?? ship.GetComponent<ShipMotion>() ?? FindAnyObjectByType<ShipMotion>();
            if (helm == null)
                return;
            var clock = LevelClock.Current;                                  // ported 4 Oct: was game.clock
            if (clock == null || !FrameReady())
                return;
            if (ChartBody.Current != null)
                ChartBody.Current.Through = false;                           // (only a point of her course is passed through: set where she is steered along it)
            var w = FloatingOrigin.ToWorld(ship.transform.position);         // 3D HAND-OFF: scene -> world metres
            var here = new Kapman.Game.Sim.WorldXZ(w.x, w.z);
            double scale = FloatingOrigin.HorizontalScale;
            // getting her off the ground stops if something else takes her over (it comes again if she's still fast)
            if (Aground.Stage != AgroundStage.None && (!Course.IsSet || Kapman.Game.Sim.Patrols.Stage != Kapman.Game.Sim.PatrolStage.None
                || Kapman.Game.Sim.RaiderEncounter.Stage != Kapman.Game.Sim.RaidStage.None || Kapman.Game.Sim.Encounter.Stage != Kapman.Game.Sim.EncounterStage.None))
                Aground.Cancel(clock, ship);

            // nothing holds her for ever (7 Oct 2026): on the chart each meeting, chase and search has a time of its own
            if (ChartWorld.Current != null)
                Kapman.Game.Sim.SeaHolds.Tick();

            // the law: casting off from a port, its officers may search her; near a watch station (from 800) a guard boat
            // may signal. She lies to while they're at it
            var lying = ship.anchored ? Port.Nearest(ship.transform.position) : null;
            if (lying != null && lying.DistanceTo(ship.transform.position) < lying.arrivalRadius)
            {
                if (lying != searchedAt)
                    lastMoored = lying;
            }
            else if (!ship.anchored && lastMoored != null)
            {
                var left = lastMoored;
                lastMoored = null;
                Kapman.Game.Sim.Patrols.CastOff(left.Def);
            }
            else if (!ship.anchored)
                searchedAt = null;                                          // (out of that port: its officers again next time)
            Kapman.Game.Sim.Patrols.LookAtSea();
            if (Kapman.Game.Sim.Patrols.Stage != Kapman.Game.Sim.PatrolStage.None)
            {
                searchedAt = Port.Nearest(ship.transform.position);         // (once searched, leaving isn't a new cast-off)
                lastMoored = null;
                ship.anchored = true;
                Hold();
                meeting = true;
                wasMoored = false;
                return;
            }

            // raiders: a band made out comes on when it can; they close on her; running, she sails away from them at her
            // best; caught, she lies to
            Kapman.Game.Sim.RaiderEncounter.Look();
            var raid = Kapman.Game.Sim.RaiderEncounter.Stage;
            if (raid != Kapman.Game.Sim.RaidStage.None)
            {
                // ported 4 Oct: game hours this frame on the level's clock (was clock.held ? 0 : dt * clock.rate / 3600)
                double hours = stepHours;
                Kapman.Game.Sim.RaiderEncounter.Tick(hours, w);
                var band = Kapman.Game.Sim.RaiderEncounter.Band;
                if (raid == Kapman.Game.Sim.RaidStage.Running && band != null)
                {
                    double ax = w.x - band.pos.x, az = w.z - band.pos.z, d = System.Math.Max(1.0, System.Math.Sqrt(ax * ax + az * az));
                    Steer(FloatingOrigin.ToUnity(new WorldPos(w.x + ax / d * 3000.0, 0.0, w.z + az / d * 3000.0)), false, "away from them");
                    ship.anchored = false;
                    held = false;
                    meeting = true;
                    wasMoored = false;
                    return;
                }
                if (raid == Kapman.Game.Sim.RaidStage.Caught)
                {
                    ship.anchored = true;                                   // they're alongside
                    Hold();
                    meeting = true;
                    wasMoored = false;
                    return;
                }
                // (a threat, not yet answered: she carries on as she was. FOR THE PRESSING CARD of rulings 10 and 12: this
                // is where she would be held while the threat waits for its answer: Hold(); meeting = true; return;
                // with its time and its option in Sim/SeaHolds.cs)
            }

            // meeting a ship: make for her (she's hove to), lie to alongside; when they part, the course resumes
            var meet = Kapman.Game.Sim.Encounter.Stage;
            if (meet == Kapman.Game.Sim.EncounterStage.Closing || meet == Kapman.Game.Sim.EncounterStage.Alongside)
            {
                var other = Kapman.Game.Sim.Encounter.Ship;
                double apart = Kapman.Game.Sim.Encounter.DistanceM() / scale;
                if (meet == Kapman.Game.Sim.EncounterStage.Closing && other != null)
                {
                    if (ship.anchored && meeting)
                        ship.anchored = false;
                    if (!meeting)
                    {
                        meeting = true;
                        ship.anchored = false;                              // (lying to for want of a course: off to her)
                        held = false;
                    }
                    Steer(FloatingOrigin.ToUnity(new WorldPos(other.pos.x, 0.0, other.pos.z)), apart < rowInGameM * 2f, "alongside her");
                    if (apart < Kapman.Game.Sim.Encounter.AlongsideGameM)
                    {
                        ship.anchored = true;
                        Hold();
                        Kapman.Game.Sim.Encounter.ComeAlongside();
                    }
                }
                else
                {
                    ship.anchored = true;                                   // alongside: both lie to while they talk
                    Hold();
                }
                wasMoored = false;
                return;
            }
            if (meeting && raid == Kapman.Game.Sim.RaidStage.None)
            {
                meeting = false;                                            // parted: on with the voyage (or lie to again)
                ship.anchored = !Course.IsSet;
                held = !Course.IsSet;
                wasMoored = false;
            }

            if (!Course.IsSet)
            {
                // no course: the planner gives no orders (the player's click orders stand, as in a river place)
                if (!ship.anchored)
                {
                    // no course: lie to. Just cast off: open the map to choose where to sail
                    ship.anchored = true;
                    Hold();
                    held = true;
                    if (wasMoored)
                    {
                        Voyage.Say("Choose where to sail: pick a port on the map.");
                        MapView.Current?.OpenAtSea();
                    }
                    else
                        if (MapView.Current == null || !MapView.Current.chartMode)       // (on the chart her own panel says it: no banner over the map)
                            Voyage.Say("No course set: [M] for the map, then pick a port.");
                }
                wasMoored = ship.anchored && !held;
                return;
            }

            // moored, with the port's own business still open (the frame says so): the course is charted, but she stays
            // made fast until she is cast off. (Without this the order to steer un-moored her at the berth under the
            // held clock: she lay "at sea" in port, and would have sailed the moment the clock ran.)
            if (Moored != null && Moored())
            {
                // (the port has her, whoever laid her to first: on a new game this planner did, a frame before the port's
                // graph, and "held" then let a course take her off the berth with nobody casting off)
                held = false;
                if (!ship.anchored)
                {
                    ship.anchored = true;
                    Hold();
                }
                // (moored where she was bound: she has arrived, as below)
                if (ship.anchored && Kapman.Game.Sim.WorldXZ.Distance(here, Course.Points[Course.Points.Count - 1]) / scale < arrivedGameM)
                {
                    Voyage.Say("Arrived at " + (GameData.Port(Course.Destination)?.name ?? Course.Destination) + ".");
                    Course.Clear();
                    Hold();
                }
                wasMoored = true;
                return;
            }
            if (held)
            {
                ship.anchored = false;                                     // a course: off she goes
                held = false;
            }
            // stuck fast on the way: the choice of how to get her off, and the getting off (Aground); the course waits
            if (Aground.Tick(ship, helm, clock))
            {
                wasMoored = false;
                return;
            }
            // the flats on her way: she lies to and waits for the tide before them (TideWait); the course waits
            if (TideWait.Tick(ship, clock, here))
            {
                Hold();
                wasMoored = false;
                return;
            }
            // ON THE CHART she keeps to her charted line: she steers for a point of the line a little ahead of her own
            // place on it, and passes through it without slowing (ChartBody.Through). In a 3D level she is handed the
            // line's points one at a time, as she always was.
            bool last;
            Kapman.Game.Sim.WorldXZ p;
            var body = ChartBody.Current;
            // (9 Oct 2026: THE SEA MOVES HER. On open water the steersman keeps the headings of her course from where
            // he reckons her to be; she is truly somewhere else by all that has carried her: Voyage/Adrift.cs)
            var by = here;
            if (body != null)
            {
                by = Adrift.SteersBy(here, stepHours, out bool lostReckoning);
                if (lostReckoning)
                {
                    // he has run his distance and his landfall is not in sight: she lies to for the next order
                    string bound = Course.Destination;
                    string lostWords = Adrift.LostWords(Course.ToPlace ? null : Course.BoundName);
                    Course.Clear();
                    Adrift.Lost(bound);
                    ship.anchored = true;
                    Hold();
                    held = true;
                    wasMoored = false;
                    // (a moment of the flow: "On the sea moves her"; the log is told, and the clock drops back)
                    SeaMoments.Raise("lost", lostWords, ClockReason.OffLine);
                    return;
                }
                p = Course.Aim(by, chartAheadM, out last);
                // (the same heading from where she truly is)
                p = new Kapman.Game.Sim.WorldXZ(p.x + here.x - by.x, p.z + here.z - by.z);
                body.Through = !last;
            }
            else
            {
                last = Course.Advance(here, passGameM * scale);
                p = Course.Points[Mathf.Min(Course.Next, Course.Points.Count - 1)];
            }
            int next = Mathf.Min(Course.Next, Course.Points.Count - 1);
            double toBerth = Kapman.Game.Sim.WorldXZ.Distance(here, Course.Points[Course.Points.Count - 1]) / scale;
            // on a river she rows along the channel (no tacking into the banks); and the last stretch to the berth
            bool river = Course.LegIsRiver(next);
            // 3D HAND-OFF: a point of the line (world metres) -> the scene; the beating corridor (helm.lineFrom/hasLine)
            // went with the sailing model
            Steer(FloatingOrigin.ToUnity(new WorldPos(p.x, 0.0, p.z)), river || (last && toBerth < rowInGameM), river ? "down the river" : "in to the berth");
            // A PLACE POINTED AT (9 Oct 2026) has no berth and nobody to take her lines: come to the end of her line, she
            // lies to there, and the clock drops back for the next order.
            // (she comes to it by the steersman's reckoning: on open water that is not always where it is)
            double toPlace = Kapman.Game.Sim.WorldXZ.Distance(by, Course.Points[Course.Points.Count - 1]) / scale;
            if (Course.ToPlace && toPlace < arrivedGameM)
            {
                bool shortOf = Kapman.Game.Sim.WorldXZ.Distance(Course.Points[Course.Points.Count - 1], Course.Place) > 3000.0;
                bool astray = Kapman.Game.Sim.WorldXZ.Distance(by, here) > 2000.0;
                Voyage.Say(astray ? Adrift.PlaceReckonedWords : shortOf ? PlaceShortWords : PlaceThereWords);
                Course.Clear();
                ship.anchored = true;
                Hold();
                held = true;
                wasMoored = false;
                ClockDrops.Raise(ClockReason.AtPlace);
                return;
            }
            // there: moored at her destination by the port's graph
            if (ship.anchored && toBerth < arrivedGameM)
            {
                Voyage.Say("Arrived at " + (GameData.Port(Course.Destination)?.name ?? Course.Destination) + ".");
                Course.Clear();
                Hold();
            }
            wasMoored = ship.anchored && !held;
        }

        /// <summary>What the log says when she comes to a place pointed at, and when the water let her no nearer. PLACEHOLDER
        /// WORDS, the user's to write.</summary>
        public const string PlaceThereWords = "She lies to at the place you pointed at.",
                            PlaceShortWords = "She lies to: this is as near the place you pointed at as the water lets her.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            frameWarned = false;
            frame = null;
            Moored = null;
        }
    }
}
