// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/ShipCrew.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity; the rule (Decide) and the helmsman's pose were run in Design/ship_view/check.
using UnityEngine;

namespace Kapman.Game.ShipView
{
    public enum CrewMode
    {
        /// <summary>Made fast at a port: sail down, oars in, the men sit.</summary>
        Moored,
        /// <summary>Under oars: sail down.</summary>
        Rowing,
        /// <summary>Under sail: oars in, the men sit, one at the steering oar.</summary>
        Sailing,
        /// <summary>Lying to in a gale, or lying still at sea with no order: sail down, oars in.</summary>
        LyingTo,
    }

    /// <summary>What the crew needs to know, each frame. ShipCrewFeed fills it from the chart game.</summary>
    public struct CrewInputs
    {
        /// <summary>Made fast at a berth (KapmanGame.Current.ship.anchored).</summary>
        public bool moored;
        /// <summary>Lying to for a gale (ChartBody.Sheltering).</summary>
        public bool lyingTo;
        /// <summary>On a river (ChartWorld.OnRiver()).</summary>
        public bool onRiver;
        /// <summary>Close to a berth she is leaving or making for.</summary>
        public bool nearPort;
        /// <summary>Her speed made good, knots (ChartBody.Knots).</summary>
        public float knots;
        /// <summary>The wind, m/s, and where it comes from off her bow, degrees (0 dead ahead, 180 dead astern).</summary>
        public float wind, windOffBow;
        /// <summary>From her class: what the sail would give her now and what the oars give, knots; how close to the wind she lies, degrees.</summary>
        public float sailKnots, oarKnots, closestToWind;
        /// <summary>Degrees a second she is turning (+ to starboard), for the steering oar.</summary>
        public float turning;
    }

    /// <summary>
    /// WHEN SHE ROWS AND WHEN SHE SAILS, and what the men do meanwhile. The rule is the game's own (Travel.Speed: she
    /// rows in a calm and when rowing beats tacking) with two things the eye expects added: she is rowed off her berth
    /// and onto it, and she is rowed when the wind is foul on a river, where there is no room to tack.
    ///   - MOORED: sail furled, oars in, the men sit on their benches.
    ///   - ROWING: sail furled, oars out; 18 strokes a minute near a berth, 22 in a calm, 24 against a foul wind.
    ///   - SAILING: sail set, oars in, the men sit; the helmsman stands at the steering oar and works it as she turns.
    ///   - LYING TO: sail furled, oars in, the men sit low.
    /// A change of mind takes time: a mode is held for some seconds, the oars come out before the sail comes down, and
    /// the stroke builds up and dies away.
    /// </summary>
    [DefaultExecutionOrder(190)]
    public sealed class ShipCrew : MonoBehaviour
    {
        public RowingRig rig;
        [Tooltip("The pack's Ship_Sail_Open and Ship_Sail_Close under the ship.")]
        public GameObject sailSet, sailFurled;
        [Tooltip("The helmsman's root, where he stands (z the way he faces: the bow), and where his hand holds the steering oar.")]
        public Transform helmsman, helmStand, helmGrip;
        [Tooltip("On: helmStand is a seat (a thwart's top) and he sits on it, as in a small boat. Off: he stands there.")]
        public bool helmSits;
        [Tooltip("The pack's ShipSteerOar_1 (its y runs along the shaft): turned about its own length as she steers.")]
        public Transform steerOar;
        [Tooltip("Seconds a mode is kept before the crew will change again.")]
        public float holdSeconds = 6f;
        [Tooltip("Seconds to get the oars out or in, and for the stroke to build or die.")]
        public float oarSeconds = 2.5f, effortSeconds = 3f;

        /// <summary>Set every frame by whoever knows the game (ShipCrewFeed.Fill).</summary>
        [System.NonSerialized] public CrewInputs inputs;

        public CrewMode Mode { get; private set; } = CrewMode.Moored;

        float held = 999f;
        CrewBody helm;
        Quaternion steerRest;
        bool steerRead;
        float rudder;

        /// <summary>The rule. Pure: the same inputs give the same answer.</summary>
        public static CrewMode Decide(in CrewInputs i)
        {
            if (i.moored)
                return CrewMode.Moored;
            if (i.lyingTo)
                return CrewMode.LyingTo;
            if (i.nearPort)
                return CrewMode.Rowing;                                   // off the berth and onto it under oars
            bool canSail = i.sailKnots > 0.05f && i.wind > 1.5f;          // (Travel.Speed: no sailing under 1.5 m/s)
            bool canRow = i.oarKnots > 0.05f;
            if (!canSail)
                return canRow ? CrewMode.Rowing : CrewMode.LyingTo;
            if (canRow && i.onRiver && Mathf.Abs(Mathf.DeltaAngle(0f, i.windOffBow)) < i.closestToWind)
                return CrewMode.Rowing;                                   // a foul wind on a river: no room to tack
            if (canRow && i.oarKnots > i.sailKnots)
                return CrewMode.Rowing;                                   // rowing beats tacking
            return CrewMode.Sailing;
        }

        void OnEnable()
        {
            Build();
        }

        /// <summary>Reads the helmsman's bones and the steering oar's rest. Call again after either is changed.</summary>
        public void Build()
        {
            if (helmsman != null && (helm == null || helm.root != helmsman))
                helm = new CrewBody(helmsman);
            if (steerOar != null && !steerRead)
            {
                steerRest = steerOar.localRotation;
                steerRead = true;
            }
        }

        void LateUpdate()
        {
            Step(Time.deltaTime, Time.time);
        }

        /// <summary>One frame: the mode, the oars, the sail, the helmsman (LateUpdate calls it; it runs before the rig).</summary>
        public void Step(float dt, float time)
        {
            if ((helmsman != null && helm == null) || (steerOar != null && !steerRead))
                Build();
            held += dt;
            var want = Decide(inputs);
            if (want != Mode && (held >= holdSeconds || want == CrewMode.Moored || Mode == CrewMode.Moored))
            {
                Mode = want;
                held = 0f;
            }
            bool rowing = Mode == CrewMode.Rowing;
            if (rig != null)
            {
                // the oars stay out until the stroke has died: they are not tossed in mid-pull
                bool oarsWanted = rowing || rig.effort > 0.05f;
                rig.oarsOut = Mathf.MoveTowards(rig.oarsOut, oarsWanted ? 1f : 0f, dt / Mathf.Max(0.1f, oarSeconds));
                // the stroke builds once the oars are out, and dies before they come in
                float effortWanted = rowing && rig.oarsOut > 0.95f ? 1f : 0f;
                rig.effort = Mathf.MoveTowards(rig.effort, effortWanted, dt / Mathf.Max(0.1f, effortSeconds));
                float rate = inputs.nearPort ? 18f : inputs.wind > 1.5f ? 24f : 22f;
                rig.strokesPerMinute = Mathf.MoveTowards(rig.strokesPerMinute, rate, dt * 1.5f);
            }
            // the sail: set only while she sails, and only once the oars are in
            bool set = Mode == CrewMode.Sailing && (rig == null || rig.oarsOut < 0.05f);
            if (sailSet != null && sailSet.activeSelf != set)
                sailSet.SetActive(set);
            if (sailFurled != null && sailFurled.activeSelf == set)
                sailFurled.SetActive(!set);

            PoseHelm(dt, time);
        }

        void PoseHelm(float dt, float time)
        {
            // the steering oar turns about its own length: up to 14 degrees against a hard turn
            float wantRudder = Mode == CrewMode.Moored ? 0f : Mathf.Clamp(inputs.turning * 0.6f, -14f, 14f);
            rudder = Mathf.MoveTowards(rudder, wantRudder + 1.5f * Mathf.Sin(time * 0.7f), dt * 20f);
            if (steerOar != null && steerRead)
                steerOar.localRotation = steerRest * Quaternion.AngleAxis(rudder, Vector3.up);

            if (helm == null || !helm.Valid || helmStand == null)
                return;
            helm.ResetPose();
            if (helmSits)
            {
                // on a thwart, as the rowers sit (the rig's own measures), his feet on the boards ahead of him
                float hipUp = rig != null ? rig.hipAboveSeat : 0.10f, ahead = rig != null ? rig.footAhead : 0.55f;
                float down = rig != null ? rig.footDown : 0.20f, apart = rig != null ? rig.footApart : 0.13f;
                helm.PlaceHips(helmStand.position + helmStand.up * hipUp, helmStand.rotation);
                for (int k = 0; k < 2; k++)
                {
                    float x = k == 0 ? -apart : apart;
                    helm.Leg(k == 0, helmStand.TransformPoint(new Vector3(x, -down, ahead)), helmStand.TransformPoint(new Vector3(x * 1.4f, 0.6f, ahead * 0.6f)));
                }
            }
            else
                helm.root.SetPositionAndRotation(helmStand.position, helmStand.rotation);
            float breath = Mathf.Sin(time * 0.25f * 2f * Mathf.PI);
            float sway = Mathf.PerlinNoise(3.3f, time * 0.11f) * 2f - 1f;
            helm.Lean(3f + breath, 0f, 2f * sway);
            helm.Breathe(breath);
            Vector3 hip = helm.hips.position;
            // one hand on the steering oar (the right: it hangs on the starboard quarter), the other at his belt;
            // at a berth both hang at his sides
            bool steering = Mode != CrewMode.Moored && helmGrip != null;
            Vector3 right = steering ? helmGrip.position : helmSits ? helm.From(hip, 0.15f, 0.11f, 0.22f) : helm.From(hip, 0.24f, -0.12f, 0.06f);
            Vector3 left = helmSits ? helm.From(hip, -0.15f, 0.11f, 0.22f) : helm.From(hip, -0.2f, 0.06f, 0.14f);
            helm.Arm(false, right, helm.From(helm.Shoulder(false), 0.3f, -0.3f, -0.2f));
            helm.Arm(true, left, helm.From(helm.Shoulder(true), -0.3f, -0.3f, -0.2f));
            // he watches the sail and the water ahead; in port he looks about
            float look = Mathf.PerlinNoise(8.1f, time * 0.05f) * 2f - 1f;
            helm.Look(look * (Mode == CrewMode.Moored ? 50f : 25f), Mode == CrewMode.Sailing ? -6f : 0f);
        }
    }
}
