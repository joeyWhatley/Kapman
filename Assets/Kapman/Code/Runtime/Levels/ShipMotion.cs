using Kapman.Levels.Water;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Levels
{
    /// <summary>
    /// A SHIP ON THE WATER - rides the level's water: HDRP's real waves (WaterSampler, through the surface's CPU
    /// simulation) under her bow, stern, sides and middle give her height, pitch and roll, tide and flood included; in
    /// the harbour she lies nearly still, at sea she rides the swell you see. Without an HDRP surface she rides the
    /// sine set instead. The player's ship also takes orders, as the design has it: no joystick; click the water to
    /// make for there, and she turns and goes at her natural speed, stopping short of shoals and banks. Moored boats
    /// just ride where they lie. Her own wake is not felt (she'd climb her bow wave); the river's stream pushes her
    /// only if RiverCurrent says so.
    ///
    /// DRIVEN (ship physics 4, 4 Oct 2026): when a body (Kapman.Ships.ShipBody, on the same GameObject) moves her,
    /// <see cref="Driven"/> is on and this component stops integrating: it still takes the clicks (MakeFor, Goal,
    /// Status) and is still what the rest of the level reads (Player, Velocity, Speed, heading, Pitch, Roll, Status,
    /// Ground), but the body writes her pose and publishes her numbers here each frame. While the clock is paused the
    /// body stands still and the five-point follow below rides her on the sea that goes on moving under her, as before.
    /// Moored and distant boats have no body and are never driven.
    /// </summary>
    public class ShipMotion : MonoBehaviour
    {
        [Tooltip("The player's ship (takes orders).")]
        public bool player;
        public float length = 7.7f, beam = 1.5f;
        [Tooltip("How far the model's origin sits above the waterline (m): raise to float her higher.")]
        public float waterline;
        [Tooltip("The water she needs under her (m): she won't go where it's shallower.")]
        public float draught = 0.45f;
        public float turnRate = 14f;
        public float accel = 0.35f;
        [Tooltip("Degrees from north.")]
        public float heading;

        /// <summary>The crew's standing orders (the beats and keys set them; ShipBody carries them out): how much sail
        /// (the crew still reefs further for the wind), and the oars.</summary>
        public enum SailOrder { Full, Reefed, Struck }
        public enum OarsOrder { AsNeeded, All, Shipped }
        public SailOrder sailOrder = SailOrder.Full;
        public OarsOrder oarsOrder = OarsOrder.AsNeeded;

        public Vector3? Goal { get; private set; }
        public float Speed { get; private set; }
        public string Status { get; private set; } = "";

        /// <summary>The crew's bailing (as Kapman.Ships.BailOrder: same names and order) and one-shot orders the body carries
        /// out and clears; the hull's state the body publishes each step (ship physics nights two and three).</summary>
        public enum BailOrder { AsNeeded, AllHands, Stop }
        public BailOrder bailOrder = BailOrder.AsNeeded;
        public bool letGoAnchor, weighAnchor, makeFast, pushOff;
        public float WaterAboard { get; private set; }
        public bool Aground { get; private set; }
        public bool AtAnchor { get; private set; }
        public bool AtBerth { get; private set; }
        public bool Lost { get; private set; }
        public bool Capsized { get; private set; }

        public void PublishHull(float waterAboard, bool aground, bool atAnchor, bool atBerth, bool lost, bool capsized)
        {
            WaterAboard = waterAboard;
            Aground = aground;
            AtAnchor = atAnchor;
            AtBerth = atBerth;
            Lost = lost;
            Capsized = capsized;
        }

        /// <summary>A line from the crew (the body's warnings) shown as her status.</summary>
        public void Say(string status) => Status = status;
        public static ShipMotion Player { get; private set; }
        /// <summary>A click on the water orders her there (the 3D levels). Off on the chart, where she sails her charted
        /// line and every click is on the frame or the map (7 Oct: the chart's ship turns it off while it is at work).</summary>
        public static bool ClickOrders = true;

        /// <summary>Her way over the ground (m/s), the stream included.</summary>
        public Vector3 Velocity { get; private set; }
        /// <summary>Her tilt now (degrees), in Unity's Euler signs as her rotation is built (pitch, heading, roll): bow
        /// down +, and starboard side up + (port down).</summary>
        public float Pitch => pitch;
        public float Roll => roll;
        /// <summary>True while she rides HDRP's water (not the sine fallback).</summary>
        public bool OnHdrpWater { get; private set; }

        /// <summary>True while a body moves her (see the class notes): this component then only takes orders and reports.</summary>
        public bool Driven { get; private set; }

        float pitch, roll;

        /// <summary>The body takes her (on) or gives her back (off). Given back, she goes on from where the body left her.</summary>
        public void Drive(bool on)
        {
            Driven = on;
        }

        /// <summary>
        /// The body's report for this frame, after it has set her pose: her way over the ground (m/s, the stream
        /// included), her speed ahead through the water (m/s), and whether HDRP's water answered. Her heading, pitch and
        /// roll are read back from the pose it set.
        /// </summary>
        public void Publish(Vector3 velocity, float speed, bool onHdrpWater)
        {
            Velocity = velocity;
            Speed = speed;
            OnHdrpWater = onHdrpWater;
            Vector3 e = transform.eulerAngles;
            heading = e.y;
            pitch = Mathf.DeltaAngle(0f, e.x);
            roll = Mathf.DeltaAngle(0f, e.z);
        }

        /// <summary>The order is done or given up: no goal any more, and why (the HUD's line).</summary>
        public void EndOrder(string status)
        {
            Goal = null;
            Status = status;
        }

        void OnEnable()
        {
            heading = transform.eulerAngles.y;
            if (player && ChainRuntime.Allows(this))   // a level preloaded beside the current one leaves the static alone
                Player = this;
        }

        void OnDisable()
        {
            if (Player == this)
                Player = null;
        }

        public void MakeFor(Vector3 point)
        {
            Goal = point;
            Status = "Making for there.";
        }

        void Update()
        {
            var info = LevelInfo.Current;
            float wl = info != null ? info.WaterLevel : 0f;
            float dt = Time.deltaTime * LevelClock.TimeFactor;
            if (player)
                Orders(wl);
            if (Driven && !LevelClock.Paused)
                return;                                                   // the body moves her (paused: the follow below)

            float target = 0f;
            if (Goal.HasValue)
            {
                Vector3 to = Goal.Value - transform.position;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist < 5f)
                {
                    Goal = null;
                    Status = "There.";
                }
                else
                {
                    float want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                    float delta = Mathf.DeltaAngle(heading, want);
                    float turn = turnRate * (0.35f + 0.65f * Mathf.Clamp01(Speed));
                    heading += Mathf.Clamp(delta, -turn * dt, turn * dt);
                    float cruise = info != null ? info.shipSpeed : 1.6f;
                    target = cruise * (Mathf.Abs(delta) > 70f ? 0.35f : 1f) * Mathf.Clamp01(dist / 25f + 0.2f);
                    Vector3 fwd = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
                    float ahead = Ground(transform.position + fwd * (length * 0.5f + 2f + Speed * 3f));
                    if (ahead > wl - draught)
                    {
                        target = 0f;
                        Goal = null;
                        Status = "Too shallow ahead: she stops.";
                    }
                }
            }
            Speed = Mathf.MoveTowards(Speed, target, accel * dt);

            Vector3 f = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
            Vector3 drift = Vector3.zero;
            var river = RiverCurrent.Current;
            if (river != null && river.pushesShips)
                drift = river.Drift(transform.position);
            Velocity = f * Speed + drift;
            Vector3 p = transform.position + Velocity * dt;
            Vector3 r = new Vector3(f.z, 0f, -f.x);

            // the water under five points of her: the heights come back with the water's level in them
            int iterations = player ? 8 : 4;
            bool live = WaterSampler.Sample(p, out float hc, out _, false, iterations);
            float hb = WaterSampler.Height(p + f * length * 0.5f), hs = WaterSampler.Height(p - f * length * 0.5f);
            float hp = WaterSampler.Height(p - r * beam * 0.5f), hsb = WaterSampler.Height(p + r * beam * 0.5f);
            OnHdrpWater = live;
            float y = (hc * 2f + hb + hs + hp + hsb) / 6f + waterline;
            var cam = Camera.main;
            if (!live && info != null && cam != null)
            {
                // the bent world of the sine fallback (HDRP's water doesn't bend: the ship stays on it)
                Vector2 d = new Vector2(p.x - cam.transform.position.x, p.z - cam.transform.position.z);
                y -= info.Drop(d.magnitude);
            }
            float tPitch = Mathf.Atan2(hs - hb, length) * Mathf.Rad2Deg;
            float tRoll = Mathf.Atan2(hsb - hp, beam) * Mathf.Rad2Deg * 0.6f;   // starboard water higher: starboard up (Euler z +)
            float ease = 1f - Mathf.Exp(-Time.deltaTime * 3f);
            pitch = Mathf.Lerp(pitch, tPitch, ease);
            roll = Mathf.Lerp(roll, tRoll, ease);
            transform.SetPositionAndRotation(new Vector3(p.x, y, p.z), Quaternion.Euler(pitch, heading, roll));
        }

        void Orders(float wl)
        {
            var mouse = Mouse.current;
            var cam = Camera.main;
            if (!ClickOrders || mouse == null || cam == null || !mouse.leftButton.wasPressedThisFrame || DevHud.PointerOverPanel)
                return;
            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (ray.direction.y > -0.001f)
                return;
            float t = (wl - ray.origin.y) / ray.direction.y;
            Vector3 hit = ray.origin + ray.direction * t;
            if (Ground(hit) < wl - draught)
                MakeFor(hit);
            else
                Status = "She can't float there.";
        }

        /// <summary>The height of whatever is under a point: ground, a jetty, a house (m).</summary>
        public static float Ground(Vector3 p)
        {
            if (Physics.Raycast(new Vector3(p.x, 500f, p.z), Vector3.down, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            return -100f;
        }
    }
}
