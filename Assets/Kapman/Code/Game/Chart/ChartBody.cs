using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.Sailing;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE SHIP ON THE CHART (the user, 4 Oct 2026, picker: "the chart is the game") - in the chart scene there is no sea
    /// to ride: the merchant's ship is a place in the world frame and a heading, and this moves her. It drives the level's
    /// ShipMotion (Drive: the same hand-over the 3D body uses), so the rest of the game (the SailingShip shim, the voyage
    /// planner, the ports' graphs, the world's ships keeping clear, the map's mark) reads her as it always has.
    ///
    /// She goes where her order points (ShipMotion.Goal: the voyage planner gives her the next point of the charted
    /// line), at the speed the world's own ships make in the same wind (Travel.Speed: her class's polar, beating against
    /// the wind, the oars in a calm, lying to in a gale), on the game's clock. The scene's world scale is the clock's rate
    /// (60 real metres to a game metre at 60 game seconds a second), so her real metres a game second are her game
    /// metres a real second: the first build's reaches (a port's 30, the planner's 60 and 15) hold as they are.
    /// </summary>
    [DefaultExecutionOrder(20)]
    [RequireComponent(typeof(ShipMotion))]
    public class ChartBody : MonoBehaviour
    {
        [Tooltip("Her class in Data/ship_classes_793.json.")]
        public string classId = "gislinge";
        [Tooltip("Degrees a second she turns.")]
        public float turnRate = 40f;
        [Tooltip("Game metres from her goal inside which she slows (the last stretch in to a berth).")]
        public float slowWithin = 25f;
        [Tooltip("Knots when the tables can't be read (no world yet).")]
        public float fallbackKnots = 4f;
        [Tooltip("Degrees a second she turns while she follows her charted line (she is steered along it, bend by bend).")]
        public float courseTurnRate = 120f;

        /// <summary>Her goal is a point of her charted course that she passes through (the voyage planner says so each
        /// time it steers her along the line): she does not slow for it nor stop at it. False: a place to come to and
        /// stop at (the berth, a ship met), as it always was.</summary>
        public bool Through { get; set; }

        ShipMotion motion;
        float speed;                                          // real m/s made good (on the game's clock)
        ShipClassDef cls;

        /// <summary>The wind where she is (m/s, and the compass point it blows from), and whether she's lying to for a gale.</summary>
        public float Wind { get; private set; }
        public float WindFrom { get; private set; }
        public bool Sheltering { get; private set; }
        /// <summary>Her speed made good now, knots (real).</summary>
        public float Knots { get; private set; }

        public static ChartBody Current { get; private set; }

        void OnEnable()
        {
            motion = GetComponent<ShipMotion>();
            motion.Drive(true);
            Current = this;
            ShipMotion.ClickOrders = false;                       // (no steering on the chart: a click is on the frame or the map)
        }

        void OnDisable()
        {
            if (motion != null)
                motion.Drive(false);
            if (Current == this)
            {
                Current = null;
                ShipMotion.ClickOrders = true;
            }
        }

        void Update()
        {
            if (!motion.Driven)
                motion.Drive(true);
            if (GameSpeed.Stepped)
                return;                                               // (the game's clock steps her: KapmanGame.StepMinute calls Step)
            var clock = LevelClock.Current;
            float rate = clock != null ? clock.rate : 60f;
            if (HeldNow())
                return;
            Move(LevelClock.Paused ? 0f : Time.deltaTime * rate * LevelClock.Speed, Time.deltaTime * LevelClock.TimeFactor);
        }

        /// <summary>A pressing card holds her where she is (the user's ruling 10 of 7 Oct 2026: it "will just keep them
        /// there until it automatically presses an option"). True while it does.</summary>
        public bool Held { get; private set; }

        /// <summary>Is she held now? Held, she keeps her course and her order and makes no way: the clock, the weather
        /// and the other ships run on, and she goes on by herself when the card is answered or has answered itself.</summary>
        bool HeldNow()
        {
            Held = EncounterRunner.ShipHeld;
            if (!Held)
                return false;
            From = transform.position;                                // (her mark stands still: nothing to glide between)
            FromHeading = motion.heading;
            speed = 0f;
            Knots = 0f;
            motion.Publish(Vector3.zero, 0f, false);
            return true;
        }

        /// <summary>Where she was when the last step began, and her heading then: her mark is drawn between that and
        /// where she is, by KapmanGame.StepFraction, so she glides at speed 1 though she is moved once a second.</summary>
        public Vector3 From { get; private set; }
        public float FromHeading { get; private set; }

        /// <summary>One step of the game's clock: she sails this many game seconds, by the order she has.</summary>
        public void Step(double gameSeconds)
        {
            if (!motion.Driven)
                motion.Drive(true);
            if (HeldNow())
                return;
            Move((float)gameSeconds, (float)(gameSeconds / 60.0));
        }

        /// <summary>She sails `gameSeconds`. `helm` is the same span in real seconds at speed 1 (a game minute is one):
        /// her turning and her gathering way are counted in it, as they always were.</summary>
        void Move(float gameSeconds, float helm)
        {
            float scale = (float)FloatingOrigin.HorizontalScale;
            // game seconds over real metres a game metre: real m/s -> game metres this step
            float step = gameSeconds / Mathf.Max(0.001f, scale);

            Vector3 pos = transform.position;
            From = pos;
            FromHeading = motion.heading;
            float heading = motion.heading;
            float target = 0f;
            var world = KapmanGame.Current != null ? KapmanGame.Current.World : null;
            if (cls == null && world != null)
                cls = world.ClassOf(classId);
            if (world != null)
            {
                var w = FloatingOrigin.ToWorld(pos);
                var s = world.WeatherAt(new WorldXZ(w.x, w.z));
                Wind = s.windSpeed;
                WindFrom = s.windFrom;
            }
            bool rowing = false;                                      // (under the order Out oars, and making way by it)
            var goal = motion.Goal;
            if (goal.HasValue)
            {
                Vector3 to = goal.Value - pos;
                to.y = 0f;
                float dist = to.magnitude;
                bool through = Through;
                if (dist < 5f && !through)
                    motion.EndOrder("There.");
                else if (dist < 0.02f)
                    target = speed;                                       // (on the very point she steers for: as she goes)
                else
                {
                    float want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                    float delta = Mathf.DeltaAngle(heading, want);
                    float turn = through ? Mathf.Max(turnRate, courseTurnRate) : turnRate;
                    heading += Mathf.Clamp(delta, -turn * helm, turn * helm);
                    bool lying = false;
                    float course = Mathf.Repeat(want, 360f);
                    // (THE OARS OUT by the player's order: the sail is down and the men row hard, whatever the wind.
                    // Else as she has always gone: the sail, or the easy stroke where it gives less: Travel.Speed)
                    // (8 Oct: and her speed is the world's ships' own rule through the boat she truly is: her sail, the
                    // water in her, her men, and her order for a gale. Voyage/Boat.cs. Lost, or lying to by a card, she
                    // makes no way under oars either.)
                    double made = cls == null ? fallbackKnots * 0.5144
                                : Oars.Out && !Boat.IsLost && !Boat.LyingTo ? Oars.Speed(cls, course, WindFrom, Wind, out lying) * Boat.OarFactor * Boat.WaterFactor
                                                                            : Boat.Speed(cls, course, WindFrom, Wind, out lying);
                    Sheltering = lying;
                    rowing = Oars.Out && cls != null && !lying;
                    // the last stretch in: she comes in slowly (the ports' graphs take her as arrived under 1.5)
                    target = (float)made * (Mathf.Abs(delta) > 70f ? 0.35f : 1f) * (through ? 1f : Mathf.Clamp01(dist / Mathf.Max(1f, slowWithin) + 0.2f));
                    if (lying)
                        motion.Say(Boat.IsLost ? "She is lost." : Wind > GaleDice.GaleWind ? "A gale: she lies to." : "She lies to.");
                }
            }
            else
                Sheltering = false;
            speed = Mathf.MoveTowards(speed, target, 0.6f * helm);
            Vector3 fwd = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
            pos += fwd * speed * step;
            pos.y = 0f;
            transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, heading, 0f));
            // her way over the ground in game metres a real second at 1x (the world's ships read it an hour ahead)
            float perSecond = 60f / Mathf.Max(0.001f, scale);
            motion.Publish(fwd * speed * perSecond, speed * perSecond, false);
            Knots = speed / 0.5144f;
            // (an hour rowed hard is an hour out of the men; lying still or under sail they rest; made fast the oars are in)
            var game = KapmanGame.Current;
            Oars.Step(gameSeconds / 3600.0, rowing, game != null && game.ship != null && game.ship.anchored, cls);
        }
    }
}
