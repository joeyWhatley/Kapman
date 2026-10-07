using Kapman.Levels;
using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE PLAYER'S SHIP AS A BODY (ship physics 2.1 and 4). Put beside ShipMotion on the player's ship (GameSetup does it
    /// when the game goes into a level). Each frame it adds real seconds x LevelClock.Speed to its clock and takes that many
    /// 0.02 s steps of ShipSim (the same code the EditMode tests step): the HDRP water under her probe cells lifts and
    /// tilts her, the oars and the sail drive her as ShipMotion's standing orders say, the helmsman steers her on the
    /// steering oar for the course the helm gives him (beating up to a goal inside her closest point to the wind, tacking
    /// or wearing), the water that comes aboard is bailed by the crew, the bottom (SceneBed: the level's colliders under
    /// her, not her own) holds her when she touches, and the anchor and the berth's lines hold her when they are out;
    /// then it draws her between the last two steps (so 50 steps a second look smooth at any frame rate), moves her
    /// kinematic Rigidbody and steps the physics scene with each step, and publishes her speed and velocity to
    /// ShipMotion, which every other part of the level reads.
    ///
    /// The clock: Time.timeScale is never touched (HDRP's water already runs at LevelClock.Speed). Paused: the body
    /// stands still and ShipMotion's own follow rides her on the moving sea; on resuming she is taken up from where the
    /// follow left her. Moved by anything else (a level entry, a loaded game, the shim's Teleport, a salvage) she is taken
    /// up from there, with no way on, her anchor aboard, her lines cast off, and, if she was lost, pumped out and upright.
    /// The physics scene: Physics.simulationMode is set to Script while a body is enabled (nothing else in the levels uses
    /// physics simulation - only raycasts, which Script mode leaves alone) and put back after.
    ///
    /// Her lines (the helm's, the crew's warnings: placeholder wording, the user writes the real ones) go to ShipMotion
    /// by SendMessage("Say", line): ShipMotion shows them once it has a Say(string) (the facade's patch); until then they
    /// are dropped quietly. Her state for the rest of the game (lost, aground, at anchor, water aboard) is read here for
    /// now (Lost, Fate, Aground, AtAnchor, WaterAboard); the facade's patch publishes it on ShipMotion.
    /// </summary>
    [RequireComponent(typeof(ShipMotion))]
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(10)]                                           // after ShipMotion has taken this frame's click
    public class ShipBody : MonoBehaviour
    {
        [Tooltip("The ships as bodies (Assets/Kapman/Data/Ships/ships.json).")]
        public TextAsset ships;
        [Tooltip("Which ship she is (an id in ships.json).")]
        public string classId = "gislinge";
        [Tooltip("The oars while she has an order: 1 = a steady stroke (six men for hours), up to 1.5 a hard pull.")]
        [Range(0f, 1.5f)] public float oarEffort = 1f;
        [Tooltip("Set the sail when the wind will drive her the way she is ordered.")]
        public bool sail = true;
        [Tooltip("The standing bail order: the crew bail as the water rises, all hands to it, or nobody.")]
        public BailOrder bail = BailOrder.AsNeeded;
        [Tooltip("What the bottom is here (until the terrain's layers say): how it grips her keel and holds her anchor.")]
        public GroundKind bottom = GroundKind.Sand;
        [Tooltip("The helmsman's skill and the crew's seamanship (0-1; the crew design will give them).")]
        [Range(0f, 1f)] public float helmSkill = 0.7f, crewSkill = 0.7f;
        [Tooltip("Step the physics scene with each body step (Physics.simulationMode = Script). Off: the Rigidbody " +
                 "just follows her and the physics scene keeps Unity's own stepping.")]
        public bool stepPhysics = true;

        /// <summary>The body, for the HUD and the tests in Play (null until enabled).</summary>
        public ShipSim Sim { get; private set; }
        /// <summary>The helm's judgement (the course under sail, tacks and wears).</summary>
        public Helm Helm => helm;

        /// <summary>Her state for the rest of the game: lost (swamped or capsized: the game decides what that means), fast
        /// aground, riding to her anchor, made fast at a berth, the water in her (m3).</summary>
        public bool Lost => Sim != null && Sim.Lost;
        public ShipFate Fate => Sim != null ? Sim.Fate : ShipFate.Afloat;
        public bool Aground => Sim != null && Sim.Grounding.Aground;
        public bool AtAnchor => Sim != null && Sim.Anchor.Down;
        public bool AtBerth => Sim != null && Sim.Berth.Fast;
        public float WaterAboard => Sim != null ? Sim.Bilge.Water : 0f;

        ShipMotion motion;
        Rigidbody body;
        SceneBed bed;
        readonly Helm helm = new Helm();
        readonly StepClock clock = new StepClock();
        readonly HdrpWaterField water = new HdrpWaterField();
        Vector3 fromPos, toPos, wrotePos;
        Quaternion fromRot = Quaternion.identity, toRot = Quaternion.identity, wroteRot = Quaternion.identity;
        bool seated, firstSeat = true, wasPaused, scripting;
        bool saidLost, wasAground, wasDragging, wasParted, wasBailing, shoal, weighingToGo;
        int touches, cracks;
        float callsAt, awashAt, workingAt, sluggishAt, answerAt, lastSaidAt = -100f;
        string lastSaid = "";

        static int scriptUsers;
        static SimulationMode modeBefore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            scriptUsers = 0;
        }

        void OnEnable()
        {
            motion = GetComponent<ShipMotion>();
            body = GetComponent<Rigidbody>();
            if (ships == null)
            {
                Debug.LogError($"Kapman: {name}'s ShipBody has no ships.json: she is left to ShipMotion (Kapman > Game > Add the game to this scene sets it).", this);
                enabled = false;
                return;
            }
            try
            {
                Sim = new ShipSim(ShipClassData.Parse(ships.text).Get(classId), motion.waterline);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Kapman: {name}'s ShipBody could not be made ({e.Message}): she is left to ShipMotion.", this);
                Sim = null;
                enabled = false;
                return;
            }
            Sim.TurnRateMax = motion.turnRate;
            bed = new SceneBed(transform) { kind = bottom };
            Sim.Bed = bed;
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.None;
            if (stepPhysics)
            {
                if (scriptUsers++ == 0)
                {
                    modeBefore = Physics.simulationMode;
                    Physics.simulationMode = SimulationMode.Script;
                }
                scripting = true;
            }
            water.iterations = motion.player ? 8 : 4;
            seated = false;
            motion.Drive(true);
        }

        void OnDisable()
        {
            if (motion != null)
                motion.Drive(false);
            if (scripting)
            {
                scripting = false;
                if (--scriptUsers == 0)
                    Physics.simulationMode = modeBefore;
            }
        }

        void Update()
        {
            if (Sim == null)
                return;
            if (LevelClock.Paused)
            {
                wasPaused = true;                                         // ShipMotion's follow rides her meanwhile
                return;
            }
            if (!seated || wasPaused || Moved())
                Seat(wasPaused);
            wasPaused = false;

            int steps = clock.Take(Time.deltaTime, LevelClock.Speed);
            Orders(steps * ShipSim.Dt);
            for (int i = 0; i < steps; i++)
            {
                fromPos = toPos;
                fromRot = toRot;
                Sim.Step(water);
                toPos = Sim.Origin;
                toRot = Sim.State.rotation;
                if (scripting)
                {
                    body.MovePosition(toPos);
                    body.MoveRotation(toRot);
                    Physics.Simulate(ShipSim.Dt);
                }
            }
            float a = clock.Alpha;
            wrotePos = Vector3.Lerp(fromPos, toPos, a);
            wroteRot = Quaternion.Slerp(fromRot, toRot, a);
            transform.SetPositionAndRotation(wrotePos, wroteRot);
            if (!scripting)
                body.position = wrotePos;
            motion.Publish(Sim.State.velocity, Mathf.Max(0f, Sim.Forces.AheadSpeed), water.Live);
            motion.PublishHull(Sim.Bilge.Water, Sim.Grounding.Aground, Sim.Anchor.Down, Sim.Berth.Fast, Sim.Lost, Sim.Fate == ShipFate.Capsized);
            Calls();
        }

        /// <summary>Has anything but this body moved her since the last frame?</summary>
        bool Moved() =>
            (transform.position - wrotePos).sqrMagnitude > 0.05f * 0.05f || Quaternion.Angle(transform.rotation, wroteRot) > 0.5f;

        /// <summary>
        /// Takes her up where she lies now. After a pause she keeps her way over the ground (the follow only held her
        /// still in time), and her anchor and lines; after any other move she has no way, her anchor comes aboard, her
        /// lines are cast off and, if she was lost, she is pumped out and set upright (a salvage, or a load). The first
        /// time, she is set at her waterline on the water under her (the level's builder put her at its resting level).
        /// </summary>
        void Seat(bool keepWay)
        {
            Vector3 p = transform.position;
            Quaternion r = transform.rotation;
            bool fresh = !(keepWay && seated);
            if (firstSeat || (fresh && Sim.Lost))
            {
                water.Sample(p, out float h, out _);
                p.y = h + motion.waterline;
                r = Rot.Yaw(transform.eulerAngles.y);
                firstSeat = false;
            }
            if (fresh)
            {
                Sim.Anchor.Stow();
                Sim.Berth.CastOff();
                if (Sim.Lost)
                    Sim.Bilge.Empty();
                helm.Reset();
                saidLost = false;
            }
            Vector3 way = !fresh ? new Vector3(Sim.State.velocity.x, 0f, Sim.State.velocity.z) : water.Current(p);
            Sim.PlaceAt(p, r, way);
            clock.Clear();
            fromPos = toPos = wrotePos = p;
            fromRot = toRot = wroteRot = r;
            transform.SetPositionAndRotation(p, r);
            body.position = p;
            body.rotation = r;
            seated = true;
        }

        /// <summary>How much sail she carries in a wind (m/s at 10 m): full to a fresh breeze, a reef in from 12, two from
        /// 15, struck above 20 (an open boat's master is careful: she is wet and over-pressed long before she capsizes).</summary>
        public static float SailReef(float wind) => wind > 20f ? 0f : wind > 15f ? 0.4f : wind > 12f ? 0.7f : 1f;

        // ----- the master's orders, for the game and the HUD -----

        /// <summary>Lets the anchor go here (3:1 scope or the rope she has). False when there is no bottom for her rope.</summary>
        public bool LetGoAnchor()
        {
            if (Sim == null)
                return false;
            if (!Sim.Anchor.LetGo(Sim, water, Sim.Bed))
            {
                Say(Sim.Anchor.Parted ? "We've no anchor: the cable parted." : "No bottom with the rope we have.");
                return false;
            }
            motion.EndOrder("Anchored.");
            return true;
        }

        public void WeighAnchor()
        {
            if (Sim != null && Sim.Anchor.Down)
            {
                Sim.Anchor.Weigh();
                Say("Weighing anchor.");
            }
        }

        /// <summary>More rope on the anchor (m).</summary>
        public void MoreRope(float metres = 5f)
        {
            if (Sim != null && Sim.Anchor.Down)
                Sim.Anchor.PayOut(metres);
        }

        /// <summary>Makes her fast where she lies to the posts at her stem and stern (a berth, the piles alongside).</summary>
        public void MakeFast()
        {
            if (Sim == null)
                return;
            Sim.Berth.MakeFastHere(Sim);
            motion.EndOrder("Made fast.");
        }

        public void CastOff()
        {
            if (Sim != null && Sim.Berth.Fast)
            {
                Sim.Berth.CastOff();
                Say("Cast off.");
            }
        }

        /// <summary>The crew over the side to push her off the bottom (until she floats, or a minute).</summary>
        public void PushOff()
        {
            if (Sim == null || !Sim.Grounding.Touching)
                return;
            Sim.Grounding.PushOff();
            Say("Over the side and push!");
        }

        /// <summary>Pumped out and set upright where she lies, her way gone (the game's salvage in a river or a port).</summary>
        public void Salvage()
        {
            if (Sim == null)
                return;
            Sim.Bilge.Empty();
            Vector3 p = transform.position;
            water.Sample(p, out float h, out _);
            p.y = h + motion.waterline;
            transform.SetPositionAndRotation(p, Rot.Yaw(transform.eulerAngles.y));
            seated = false;                                               // taken up afresh next frame
            saidLost = false;
        }

        // the orders from the component's menu in Play, until the council panel has them
        [ContextMenu("Anchor: let go")] void MenuLetGo() => LetGoAnchor();
        [ContextMenu("Anchor: weigh")] void MenuWeigh() => WeighAnchor();
        [ContextMenu("Anchor: 5 m more rope")] void MenuMoreRope() => MoreRope();
        [ContextMenu("Berth: make fast here")] void MenuMakeFast() => MakeFast();
        [ContextMenu("Berth: cast off")] void MenuCastOff() => CastOff();
        [ContextMenu("Aground: push her off")] void MenuPushOff() => PushOff();
        [ContextMenu("Lost: salvage her")] void MenuSalvage() => Salvage();

        /// <summary>A line to the panel (the same line not again within 10 s): ShipMotion.Say once the facade has it.</summary>
        void Say(string line)
        {
            if (motion == null || string.IsNullOrEmpty(line) || (line == lastSaid && Time.time - lastSaidAt < 10f))
                return;
            lastSaid = line;
            lastSaidAt = Time.time;
            motion.SendMessage("Say", line, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// The master's orders, from ShipMotion's goal (a click on the water, the voyage planner, getting off a bank) and its
        /// standing orders: the course to the goal; the sail when the wind will drive her (reefed for the wind) and the oars
        /// as ordered, the crew rowing when the sail would make good less than the oars; under sail alone the helm beats up
        /// to a goal inside her closest point. There (within 5 m) the oars stop and she carries her way. Shoal water ahead
        /// (her draught and 0.2 m, a little further than she would carry her way in 6 s): the crew say so and ease her,
        /// the sail comes in, and beating she goes about. She is never stopped short any more: if she runs on, she touches
        /// at a crawl and the order ends ("She's aground."). Anchored or made fast, a new order weighs the anchor (she
        /// waits for it) and casts off. Lost, the crew do nothing.
        /// </summary>
        void Orders(float dt)
        {
            bail = (BailOrder)(int)motion.bailOrder;
            if (motion.letGoAnchor) { motion.letGoAnchor = false; LetGoAnchor(); }
            if (motion.weighAnchor) { motion.weighAnchor = false; WeighAnchor(); }
            if (motion.makeFast) { motion.makeFast = false; MakeFast(); }
            if (motion.pushOff) { motion.pushOff = false; PushOff(); }
            var weather = LevelWeather.Current;
            float windSpeed = weather != null ? weather.Wind : 0f;
            Sim.TrueWind = weather != null ? new Vector3(weather.WindVector.x, 0f, weather.WindVector.y) : Vector3.zero;
            Sim.Rain = weather != null ? weather.Rain : 0f;
            Sim.Bilge.order = bail;
            Sim.Helmsman.skill = helmSkill;
            helm.skill = crewSkill;
            bed.kind = bottom;
            Sim.SailSet = 0f;
            Sim.OarEffort = 0f;
            Sim.OarsMayTurn = true;

            if (Sim.Lost)
            {
                Sim.HasHeadingOrder = false;
                if (!saidLost)
                {
                    saidLost = true;
                    motion.EndOrder(Sim.Fate == ShipFate.Capsized ? "She's capsized." : "She's swamped.");
                }
                return;
            }
            bool aground = Sim.Grounding.Aground;
            if (aground && !wasAground && motion.Goal.HasValue)
                motion.EndOrder("She's aground.");
            wasAground = aground;

            if (!motion.Goal.HasValue)
            {
                Sim.HasHeadingOrder = false;
                helm.Reset();
                weighingToGo = false;
                return;
            }

            // ordered away from an anchorage or a berth: the anchor comes up first, the lines are cast off
            if (Sim.Berth.Fast)
                CastOff();
            if (Sim.Anchor.Down)
            {
                if (!weighingToGo)
                {
                    weighingToGo = true;
                    WeighAnchor();
                }
                Sim.HasHeadingOrder = false;
                return;
            }
            weighingToGo = false;

            Vector3 pos = Sim.Origin;
            Vector3 to = motion.Goal.Value - pos;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 5f)
            {
                motion.EndOrder("There.");
                Sim.HasHeadingOrder = false;
                helm.Reset();
                return;
            }
            float want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
            float heading = Rot.Heading(Sim.State.rotation);
            float speed = Mathf.Max(0f, Sim.Forces.AheadSpeed);
            bool shoalNow = ShoalAhead(pos, speed);
            if (shoalNow && !shoal)
                Say("Shoal water ahead: easing her.");
            shoal = shoalNow;

            // the sail: what it would make good towards the goal on the course the helm could steer under it
            float ordered = motion.sailOrder == ShipMotion.SailOrder.Full ? 1f : motion.sailOrder == ShipMotion.SailOrder.Reefed ? 0.55f : 0f;
            float reef = Mathf.Min(ordered, SailReef(windSpeed));
            float course = SailCourse(want);
            float pullThere = SailPull(course) * reef;
            float madeGood = pullThere * Mathf.Cos(Mathf.DeltaAngle(course, want) * Mathf.Deg2Rad);
            float oarsPull = Oars.Thrust(Sim.Class, oarEffort, Sim.Forces.AheadSpeed);
            bool canSail = sail && dist > 40f && reef > 0f && pullThere > 30f && !shoal;
            bool rowing = motion.oarsOrder == ShipMotion.OarsOrder.All
                          || (motion.oarsOrder == ShipMotion.OarsOrder.AsNeeded && !(canSail && madeGood > 0.8f * oarsPull));
            bool underSail = canSail && !rowing;
            helm.Steer(Sim, want, underSail, dt, shoal);
            if (underSail)
                Sim.SailSet = reef;
            else if (canSail && SailPull(heading) * reef > 30f)
                Sim.SailSet = reef;                                       // rowing, with the sail drawing as she goes
            if (rowing)
            {
                float delta = Mathf.DeltaAngle(heading, want);
                Sim.OarEffort = oarEffort * (Mathf.Abs(delta) > 70f ? 0.35f : 1f) * Mathf.Clamp01(dist / 25f + 0.2f) * (shoal ? 0.3f : 1f);
            }
            if (helm.Said != saidCount)
            {
                saidCount = helm.Said;
                Say(helm.Line);
            }
        }

        int saidCount;

        /// <summary>The course the helm would steer under sail for a course wanted: itself, or close-hauled on the side
        /// nearer it when it lies inside her closest point.</summary>
        float SailCourse(float want)
        {
            Vector3 tw = Sim.TrueWind;
            if (tw.x * tw.x + tw.z * tw.z < 1f)
                return want;
            float wind = Mathf.Atan2(-tw.x, -tw.z) * Mathf.Rad2Deg;
            float theta = Sim.Class.closestToWind + Helm.FullAndBy;
            float g = Mathf.DeltaAngle(wind, want);
            return Mathf.Abs(g) >= theta ? want : wind + (g >= 0f ? theta : -theta);
        }

        /// <summary>The ahead pull of her sail set full at a heading, in this wind at her velocity (N).</summary>
        float SailPull(float heading)
        {
            if (Sim.Class.sailArea <= 0f)
                return 0f;
            Quaternion r = Rot.Yaw(heading);
            Vector3 f = Sail.Force(Sim.TrueWind, Sim.State.velocity, r, Sim.Class.sailArea, Sim.Class.closestToWind, out _, out _);
            return Vector3.Dot(f, r * Vector3.forward);
        }

        /// <summary>Less than her draught and 0.2 m of water a little ahead of her (the gentle guard).</summary>
        bool ShoalAhead(Vector3 pos, float speed)
        {
            Vector3 fwd = Sim.State.rotation * Vector3.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-6f)
                return false;
            Vector3 p = pos + fwd.normalized * (motion.length * 0.5f + 2f + speed * 6f);
            if (!bed.Bed(p, out float bottomAt, out _, out _))
                return false;
            water.Sample(p, out float level, out _);
            return level - bottomAt < Mathf.Max(motion.draught, Sim.Class.draught) + 0.2f;
        }

        /// <summary>The crew's warnings (ship physics 3; placeholder wording), each keyed to a number the body has, said once
        /// as it is crossed and not again for 20 s.</summary>
        void Calls()
        {
            float now = Time.time;
            var b = Sim.Bilge;
            if (b.Bailers > 0 && !wasBailing && now > callsAt)
            {
                callsAt = now + 20f;
                Say("She's taking water.");
            }
            wasBailing = b.Bailers > 0;
            if (Mathf.Abs(Rot.Heel(Sim.State.rotation)) > Sim.Class.DownfloodingDeg - 5f && now > awashAt)
            {
                awashAt = now + 20f;
                Say("The gunwale's awash.");
            }
            if (Sim.Class.gm - b.GMLoss < 0.1f && b.Water > 0f && now > sluggishAt)
            {
                sluggishAt = now + 20f;
                Say("She's getting sluggish.");
            }
            var an = Sim.Anchor;
            if (an.Dragging && !wasDragging)
                Say("The anchor's dragging!");
            else if (an.Down && !an.Dragging && an.Strain > 0.8f && now > workingAt)
            {
                workingAt = now + 20f;
                Say("The anchor's working.");
            }
            wasDragging = an.Dragging;
            if (an.Parted && !wasParted)
                Say("The cable's parted!");
            wasParted = an.Parted;
            var g = Sim.Grounding;
            if (g.Touches != touches)
            {
                touches = g.Touches;
                Say("She's touched the bottom.");
            }
            if (g.PlanksCracked + g.PlanksStove != cracks)
            {
                cracks = g.PlanksCracked + g.PlanksStove;
                Say(g.PlanksStove > 0 ? "A plank's stove in!" : "She's sprung a plank.");
            }
            if (Sim.HasHeadingOrder && Sim.Forces.AheadSpeed > 1f && Sim.Helmsman.Authority < 0.4f && now > answerAt)
            {
                answerAt = now + 20f;
                Say("She's not answering, skipper.");
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Sim == null)
                return;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);
            foreach (var c in Sim.Hull.cells)
                Gizmos.DrawWireSphere(Sim.State.position + Sim.State.rotation * (c.keel - Sim.Mass.cg), 0.08f);
            var g = Sim.Grounding;
            for (int i = 0; i < g.Points; i++)
            {
                Gizmos.color = g.PointTouching(i) ? Color.yellow : new Color(0.6f, 0.4f, 0.2f, 0.8f);
                Gizmos.DrawWireCube(Sim.State.position + Sim.State.rotation * (g.Point(i) - Sim.Mass.cg), Vector3.one * 0.06f);
            }
            if (Sim.Anchor.Down)
            {
                Gizmos.color = Sim.Anchor.Dragging ? Color.red : Color.white;
                Gizmos.DrawLine(Sim.State.position + Sim.State.rotation * (Anchor.BowLocal(Sim) - Sim.Mass.cg), Sim.Anchor.Point);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Sim.State.position, 0.12f);
        }
    }
}
