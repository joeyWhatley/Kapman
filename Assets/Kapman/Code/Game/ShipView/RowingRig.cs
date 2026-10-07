// From the draft of 7 Oct 2026 (Design/ship_view/draft/RowingRig.cs), first run in Unity the same day on the stage's
// boat. What was changed after looking at it is in Design/ship_view/BUILD_LOG.md, part 2.
using System;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>Where one stroke stands: each value 0 .. 1.</summary>
    public struct StrokePose
    {
        /// <summary>1 at the catch (the blade toward the bow), 0 at the finish (toward the stern).</summary>
        public float sweep;
        /// <summary>1 with the blade in the water, 0 with it clear.</summary>
        public float dip;
        /// <summary>1 reaching (the back leant toward the stern, the way the rower faces), 0 lying back.</summary>
        public float lean;
        /// <summary>1 with the blade turned flat for the way back, 0 square.</summary>
        public float feather;
    }

    /// <summary>
    /// THE STROKE, as four beats of one cycle (phase 0 .. 1), for a working boat and not a race: at 22 strokes a minute
    /// a cycle is 2.7 s.
    ///   the DRIVE    0.00 - 0.40  the blade is in and sweeps from bow to stern; the back swings first, the arms after;
    ///   the FINISH   0.40 - 0.50  the hands drop, the blade comes out and turns flat;
    ///   the RECOVERY 0.50 - 0.92  the blade goes back toward the bow in the air; hands away first, then the back;
    ///   the CATCH    0.92 - 1.00  the blade turns square and drops in.
    /// </summary>
    public static class Stroke
    {
        public const float Drive = 0.40f, Finish = 0.10f, Catch = 0.08f;

        static float Smooth(float u) => u * u * (3f - 2f * u);

        public static StrokePose At(float phase)
        {
            float p = phase - Mathf.Floor(phase);
            var s = new StrokePose();
            if (p < Drive)
            {
                float u = p / Drive;
                s.sweep = 0.5f + 0.5f * Mathf.Cos(Mathf.PI * u);          // an even pull, eased at both ends
                s.dip = 1f;
                s.lean = (1f - u) * (1f - u);                             // the back opens early
                s.feather = 0f;
            }
            else if (p < Drive + Finish)
            {
                float u = (p - Drive) / Finish;
                s.sweep = 0f;
                s.dip = 1f - Smooth(u);
                s.lean = 0f;
                s.feather = Smooth(u);
            }
            else if (p < 1f - Catch)
            {
                float u = (p - Drive - Finish) / (1f - Catch - Drive - Finish);
                s.sweep = Smooth(u);
                s.dip = 0f;
                s.lean = Smooth(Mathf.Clamp01((u - 0.15f) / 0.85f));      // hands away, then the back follows
                s.feather = 1f - Smooth(Mathf.Clamp01((u - 0.75f) / 0.25f));
            }
            else
            {
                float u = (p - (1f - Catch)) / Catch;
                s.sweep = 1f;
                s.dip = Smooth(u);
                s.lean = 1f;
                s.feather = 0f;
            }
            return s;
        }
    }

    /// <summary>
    /// THE ROWING CREW (the user, 7 Oct 2026: "give animation a shot with those models I have ... just make a simple
    /// animation for them rowing"). The pack has no clips, so the motion is made here, every frame, from ONE number (the
    /// stroke's phase): each oar turns about its thole on the rail (swept fore and aft, dipped and lifted by as much as
    /// the thole's height over the water asks), and each man is rebuilt on his thwart: hips on the seat, feet braced, the
    /// back rocking from the hips, his hands taken to the oars' handles by a two-bone reach, the head kept up.
    /// One man to a pair with a hand on each (a small boat: oar2), or one man to an oar with both hands on it.
    /// No Animator, no avatar, no clips, no package.
    ///
    /// Frames: a THOLE is a child of the ship with x pointing outboard and y up (so its z is toward the bow on the
    /// starboard side and toward the stern on the port side); a SEAT is a child of the ship on the thwart's top with z
    /// the way the man faces (the stern) and y up. PackCrewRig makes both from the stage's boat and steps this.
    /// With the oars put away (oarsOut 0) each lies fore and aft on the thwarts by the rail, its blade toward the bow
    /// (Oar.stowAt), and the men sit with their hands on their thighs.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class RowingRig : MonoBehaviour
    {
        [Serializable]
        public sealed class Oar
        {
            [Tooltip("On the rail where the oar turns: x outboard, y up. A child of the ship.")]
            public Transform thole;
            [Tooltip("A child of the thole that the stroke turns.")]
            public Transform pivot;
            [Tooltip("The oar's mesh under the pivot (the pack's oar: its y runs along the oar, the blade at -y).")]
            public Transform shaft;
            [Tooltip("+1 on the starboard side, -1 on the port side.")]
            public float side = 1f;
            [Tooltip("Metres from the thole out to the middle of the blade.")]
            public float bladeOut = 2.0f;
            [Tooltip("Metres from the thole inboard to the two hands.")]
            public float gripOuter = 0.40f, gripInner = 0.68f;
            [Tooltip("Where the mesh's own origin sits along the pivot's x (0: at the thole, as the pack made it).")]
            public float shaftShift;
            [Tooltip("Where the pivot lies when the oar is put away, in the thole's own axes (the oar then points to the bow).")]
            public Vector3 stowAt;
        }

        [Serializable]
        public sealed class Rower
        {
            [Tooltip("The character's root (an SMV_Viking_* prefab).")]
            public Transform man;
            [Tooltip("On the thwart's top where he sits: z the way he faces (the stern), y up.")]
            public Transform seat;
            [Tooltip("Which oar he pulls (-1: none, he only sits).")]
            public int oar = -1;
            [Tooltip("A second oar on the other side (-1: none): he pulls a pair, one hand to each, as in a small boat.")]
            public int oar2 = -1;
            [Tooltip("Any number: makes him a little unlike the others.")]
            public float seed;
            [NonSerialized] public CrewBody body;
        }

        public Oar[] oars = new Oar[0];
        public Rower[] rowers = new Rower[0];

        [Header("The stroke")]
        [Tooltip("Strokes a minute. A trading boat keeps 18 to 24 for hours; a race is 30 and more.")]
        public float strokesPerMinute = 22f;
        [Range(0f, 1f)] [Tooltip("0: resting on the oars, blades clear. 1: the full stroke.")]
        public float effort = 1f;
        [Range(0f, 1f)] [Tooltip("0: the oars put away along the thwarts. 1: out on the tholes.")]
        public float oarsOut = 1f;
        [Tooltip("Degrees the blade goes toward the bow at the catch, and toward the stern at the finish.")]
        public float catchAngle = 26f, finishAngle = 24f;
        [Tooltip("Degrees the blade turns flat on the way back (0: never).")]
        public float feather = 35f;
        [Tooltip("Metres the middle of the blade goes under the water on the drive, and is held over it on the way back.")]
        public float bladeDepth = 0.12f, bladeClear = 0.30f;
        [Tooltip("Putting an oar away: degrees its blade is tossed up as it is swung in over the rail.")]
        public float stowToss = 20f;

        [Header("The men")]
        [Tooltip("Degrees the back leans toward the stern at the catch, and lies back at the finish.")]
        public float reach = 22f, layBack = 14f;
        [Tooltip("Metres the hip joints sit above the thwart's top.")]
        public float hipAboveSeat = 0.10f;
        [Tooltip("Where the ankles are braced, from the thwart's top: metres ahead (the way he faces), down, and apart from the middle.")]
        public float footAhead = 0.55f, footDown = 0.20f, footApart = 0.13f;
        [Tooltip("How far the knees fall apart (m from the middle), and how high the knees' aim is over the seat.")]
        public float kneeApart = 0.2f, kneeUp = 0.6f;
        [Tooltip("Metres from the wrist to the middle of the grip.")]
        public float palm = 0.07f;
        [Range(0f, 1f)] [Tooltip("0: sitting easy. 1: bent low with the hands on the thwart, as men sit out a gale.")]
        public float huddle;

        [Header("The water")]
        [Tooltip("The height of still water in the ship's own space (m), used when the stage gives no water.")]
        public float waterline = 0f;

        /// <summary>The water's height at a world point, if the stage gives one (the ship then dips her oars in the real
        /// waves); without it the water is level at the ship's own waterline.</summary>
        public Func<Vector3, float> waterHeight;

        /// <summary>The stroke's phase, 0 .. 1 (0: the catch is made, the drive begins).</summary>
        public float Phase { get; set; }

        /// <summary>A blade goes in, or comes out: the oar's index and the middle of its blade in the world (for a splash
        /// and for the sound; both are the stage's to make).</summary>
        public event Action<int, Vector3> BladeIn, BladeOut;

        bool built;
        bool[] manned = new bool[0];

        void OnEnable()
        {
            Build();
        }

        /// <summary>Reads each man's bones and rest pose. Call again after the crew is changed.</summary>
        public void Build()
        {
            foreach (var r in rowers)
                if (r != null && r.man != null && (r.body == null || r.body.root != r.man))
                    r.body = new CrewBody(r.man);
            built = true;
        }

        void LateUpdate()
        {
            Step(Time.deltaTime, Time.time);
        }

        /// <summary>Moves the stroke on by dt seconds and poses the oars and the men (LateUpdate calls it).</summary>
        public void Step(float dt, float time)
        {
            if (!built)
                Build();
            float before = Phase;
            if (effort > 0.001f && oarsOut > 0.5f)
                Phase = Mathf.Repeat(Phase + dt * strokesPerMinute / 60f, 1f);
            Pose(time);
            if (effort > 0.5f && oarsOut > 0.9f && dt > 0f)
            {
                if (Crossed(before, Phase, 1f - Stroke.Catch * 0.5f))
                    Tell(BladeIn);
                if (Crossed(before, Phase, Stroke.Drive + Stroke.Finish * 0.5f))
                    Tell(BladeOut);
            }
        }

        static bool Crossed(float before, float now, float mark) =>
            before <= now ? before < mark && now >= mark : before < mark || now >= mark;

        void Tell(Action<int, Vector3> who)
        {
            if (who == null)
                return;
            for (int i = 0; i < oars.Length; i++)
                if (oars[i] != null && oars[i].pivot != null)
                    who(i, BladeMiddle(i));
        }

        /// <summary>The middle of an oar's blade in the world, as it is posed now.</summary>
        public Vector3 BladeMiddle(int oar) => oars[oar].pivot.TransformPoint(new Vector3(oars[oar].bladeOut, 0f, 0f));

        /// <summary>Poses everything for the phase as it stands (no time passes).</summary>
        public void Pose(float time)
        {
            if (manned.Length != oars.Length)
                manned = new bool[oars.Length];
            for (int i = 0; i < manned.Length; i++)
                manned[i] = false;
            for (int i = 0; i < rowers.Length; i++)
            {
                var r = rowers[i];
                if (r == null || r.body == null || !r.body.Valid || r.seat == null)
                    continue;
                // not every man exactly alike: a hair late, a little more or less swing
                float late = 0.02f * Mathf.PerlinNoise(r.seed * 7.31f, 0.5f);
                float swing = 0.9f + 0.2f * Mathf.PerlinNoise(0.5f, r.seed * 3.17f);
                var s = Stroke.At(Phase - late);
                Oar o = r.oar >= 0 && r.oar < oars.Length ? oars[r.oar] : null;
                if (o != null && o.thole != null && o.pivot != null)
                {
                    PoseOar(o, s);
                    manned[r.oar] = true;
                }
                else
                    o = null;
                Oar o2 = o != null && r.oar2 >= 0 && r.oar2 < oars.Length && r.oar2 != r.oar ? oars[r.oar2] : null;
                if (o2 != null && o2.thole != null && o2.pivot != null)
                {
                    PoseOar(o2, s);
                    manned[r.oar2] = true;
                }
                else
                    o2 = null;
                PoseMan(r, o, o2, s, swing, time);
            }
            var even = Stroke.At(Phase);
            for (int i = 0; i < oars.Length; i++)
                if (!manned[i] && oars[i] != null && oars[i].thole != null && oars[i].pivot != null)
                    PoseOar(oars[i], even);
        }

        float WaterAt(Vector3 world)
        {
            if (waterHeight != null)
                return waterHeight(world);
            return transform.TransformPoint(new Vector3(0f, waterline, 0f)).y;
        }

        void PoseOar(Oar o, StrokePose s)
        {
            // how steeply the oar must slope for its blade to be in the water, and to be clear of it: from the thole's
            // height over the water where the blade is, so a high rail gives a steep oar and a wave lifts the blade
            Vector3 thole = o.thole.position;
            float over = thole.y - WaterAt(thole + o.thole.right * o.bladeOut);
            float len = Mathf.Max(0.5f, o.bladeOut);
            float dipIn = Mathf.Asin(Mathf.Clamp((over + bladeDepth) / len, 0.03f, 0.8f)) * Mathf.Rad2Deg;
            float dipOut = Mathf.Asin(Mathf.Clamp((over - bladeClear) / len, -0.25f, 0.75f)) * Mathf.Rad2Deg;

            float sweep = Mathf.Lerp(-finishAngle, catchAngle, s.sweep) * effort;
            float dip = Mathf.Lerp(dipOut, Mathf.Lerp(dipOut, dipIn, s.dip), effort);
            // put away: swung in over the rail, blade to the bow, and laid along the thwarts
            float stow = 1f - Mathf.Clamp01(oarsOut);
            float lay = stow * stow * (3f - 2f * stow);
            sweep = Mathf.Lerp(sweep, 90f, lay);
            dip = Mathf.Lerp(dip, 0f, lay) - stowToss * Mathf.Sin(Mathf.PI * lay);

            // about the upright first for the sweep (the sign by the side, see the class's note on frames), then the dip
            o.pivot.localPosition = o.stowAt * lay;
            o.pivot.localRotation = Quaternion.AngleAxis(-o.side * sweep, Vector3.up) * Quaternion.AngleAxis(-dip, Vector3.forward);
            if (o.shaft != null)
            {
                // the pack's oar lies along its own y with the blade at -y and the blade's breadth along x: laid along the
                // pivot's x (blade outboard, its breadth upright), then turned about its own length to feather (flat when laid away)
                float turn = Mathf.Lerp(feather * s.feather * effort, 90f, lay);
                o.shaft.localRotation = Quaternion.AngleAxis(turn, Vector3.right) * Quaternion.AngleAxis(90f, Vector3.forward);
                o.shaft.localPosition = new Vector3(o.shaftShift, 0f, 0f);
            }
            if (!o.pivot.gameObject.activeSelf)
                o.pivot.gameObject.SetActive(true);
        }

        void PoseMan(Rower r, Oar o, Oar o2, StrokePose s, float swing, float time)
        {
            var b = r.body;
            b.ResetPose();
            Vector3 up = r.seat.up;
            b.PlaceHips(r.seat.position + up * hipAboveSeat, r.seat.rotation);

            // the feet braced ahead of him on the bottom boards, the knees up and a little apart
            for (int k = 0; k < 2; k++)
            {
                bool left = k == 0;
                float x = left ? -footApart : footApart, kx = left ? -kneeApart : kneeApart;
                Vector3 ankle = r.seat.TransformPoint(new Vector3(x, -footDown, footAhead));
                Vector3 knee = r.seat.TransformPoint(new Vector3(kx, kneeUp, footAhead * 0.5f));
                b.Leg(left, ankle, knee);
            }

            // how much of him is rowing: nothing while the oar is away, all with the hands on it
            float hold = o != null ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 0.95f, oarsOut)) : 0f;
            float work = hold * effort;

            // the back: rocking from the hips with the stroke; sitting easy otherwise, breathing; bent low in a gale
            float idleBreath = Mathf.Sin((time * 0.25f + r.seed) * 2f * Mathf.PI);
            float rowBreath = Mathf.Cos(Phase * 2f * Mathf.PI);
            float rock = Mathf.Lerp(-layBack, reach, s.lean) * swing;
            float easy = Mathf.Lerp(7f + 1.5f * idleBreath, 30f + 1.5f * idleBreath, huddle);
            float lean = Mathf.Lerp(easy, rock, work);
            b.Lean(lean);
            b.Breathe(Mathf.Lerp(idleBreath, rowBreath, work));

            // the hands: on the thighs at rest (on the thwart beside him in a gale), on the oars when they are out
            Vector3 hip = b.hips.position;
            Vector3 restL = Vector3.Lerp(b.From(hip, -0.15f, 0.11f, 0.22f), b.From(hip, -0.27f, -0.06f, 0.06f), huddle);
            Vector3 restR = Vector3.Lerp(b.From(hip, 0.15f, 0.11f, 0.22f), b.From(hip, 0.27f, -0.06f, 0.06f), huddle);
            Vector3 wristL = restL, wristR = restR;
            if (o != null && o2 != null && hold > 0f)
            {
                // a pair: one hand to each oar's handle, the left hand to the oar on his left
                bool firstOnLeft = (b.Shoulder(true) - o.thole.position).sqrMagnitude < (b.Shoulder(false) - o.thole.position).sqrMagnitude;
                Oar onLeft = firstOnLeft ? o : o2, onRight = firstOnLeft ? o2 : o;
                Vector3 gripL = onLeft.pivot.TransformPoint(new Vector3(-onLeft.gripInner, 0f, 0f));
                Vector3 gripR = onRight.pivot.TransformPoint(new Vector3(-onRight.gripInner, 0f, 0f));
                wristL = Vector3.Lerp(restL, Wrist(b.Shoulder(true), gripL), hold);
                wristR = Vector3.Lerp(restR, Wrist(b.Shoulder(false), gripR), hold);
            }
            else if (o != null && hold > 0f)
            {
                Vector3 outer = o.pivot.TransformPoint(new Vector3(-o.gripOuter, 0f, 0f));
                Vector3 inner = o.pivot.TransformPoint(new Vector3(-o.gripInner, 0f, 0f));
                Vector3 th = o.thole.position;
                bool leftInner = (b.Shoulder(true) - th).sqrMagnitude > (b.Shoulder(false) - th).sqrMagnitude;
                wristL = Vector3.Lerp(restL, Wrist(b.Shoulder(true), leftInner ? inner : outer), hold);
                wristR = Vector3.Lerp(restR, Wrist(b.Shoulder(false), leftInner ? outer : inner), hold);
            }
            b.Arm(true, wristL, b.From(b.Shoulder(true), -0.25f, -0.35f, -0.15f));
            b.Arm(false, wristR, b.From(b.Shoulder(false), 0.25f, -0.35f, -0.15f));

            // the head: kept up against the back's lean; looking about when he has nothing to do; down in a gale
            float wander = Mathf.PerlinNoise(r.seed * 1.7f, time * 0.07f) * 2f - 1f;
            b.Look(wander * Mathf.Lerp(40f, 10f, Mathf.Max(work, huddle)), -0.55f * lean + 3f + 22f * huddle * (1f - work));
        }

        Vector3 Wrist(Vector3 shoulder, Vector3 grip)
        {
            Vector3 d = grip - shoulder;
            float m = d.magnitude;
            return m > 1e-4f ? grip - d / m * palm : grip;
        }
    }
}
