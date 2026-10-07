using Kapman.Sailing;
using Kapman.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// SHOT DIRECTOR - the event shots of meetings at sea (Plans/ai_fleets.md, phases 3 and 4): a hard cut in, a 1 s blend
    /// back to the ship's camera, [Esc] skips.
    ///   SIGHTING  a long lens from over your boat's shoulder at the ship made out (a trader to meet, raiders bearing
    ///             down), held while you decide;
    ///   TWO-SHOT  the two ships side by side from abeam, both in frame (alongside a trader; caught by raiders);
    ///   CHASE     from ahead of your boat looking back past her at the pursuers, for a few seconds as the run begins.
    /// It goes with the meetings by itself; graphs can call a shot too ("Camera: Event shot", "Camera: Back to the ship").
    /// Only while the ship's camera is on (not flying free, not on the map). The miniature look focuses on the subject.
    /// Ported 4 Oct: the first build's ShipCamera and CameraShot (Kapman.Cameras) are gone. The ship's camera of the
    /// rebuild is the level's OrbitCamera on the main camera; the subject's focus distance for the look is kept here in
    /// <see cref="FocusDistance"/> (0: no shot on) for whatever draws the look to read.
    /// </summary>
    [DefaultExecutionOrder(900)]                 // after the ship's camera has placed the camera; before the map and the look
    public class ShotDirector : MonoBehaviour
    {
        public enum Shot { None, Sighting, TwoShot, Chase }

        [Tooltip("Shots go with the meetings at sea by themselves.")]
        public bool followMeetings = true;
        [Tooltip("Seconds the blend back to the ship's camera takes.")]
        public float blendBack = 1f;
        [Tooltip("Seconds the chase shot holds as the run begins.")]
        public float chaseSeconds = 8f;
        [Tooltip("The long lens: its narrowest and widest field of view, degrees.")]
        public float minFov = 1.5f, maxFov = 32f;
        [Tooltip("How much of the frame's width the ship (or the band) fills in the sighting and chase shots.")]
        [Range(0.1f, 0.9f)] public float fill = 0.3f;
        [Tooltip("Degrees above the horizon of the two-shot.")]
        public float twoShotElevation = 8f;
        [Tooltip("Field of view of the two-shot, degrees.")]
        public float twoShotFov = 24f;
        [Tooltip("How far above the middle of the frame the subject sits (0 = in the middle, 1 = at the top), clear of the panel.")]
        [Range(0f, 0.8f)] public float raise = 0.35f;

        public static ShotDirector Current { get; private set; }
        /// <summary>The shot on now (None while back on the ship, or blending back).</summary>
        public Shot Now { get; private set; }
        /// <summary>Metres from the camera to the shot's subject while a shot is on or blending back (0: none): for the
        /// look's focus (was the first build's CameraShot.FocusDistance).</summary>
        public static float FocusDistance { get; private set; }

        Shot asked;                                                     // a shot a graph called, for so long
        SimShip askedOf;
        float askedFor;
        string stageKey = "";
        bool skipped;
        float since, blend = 1f;
        Vector3 lastPos;
        Quaternion lastRot;
        float lastFov, lastFocus;

        void OnEnable()
        {
            Current = this;
            FloatingOrigin.Shifted += MoveWithTheWorld;
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            FloatingOrigin.Shifted -= MoveWithTheWorld;
            FocusDistance = 0f;
        }

        void MoveWithTheWorld() => lastPos += FloatingOrigin.LastShift;

        /// <summary>A graph's shot of a ship of the world, for so many seconds (or until Back).</summary>
        public void Play(Shot shot, SimShip of, float seconds = 6f)
        {
            asked = of != null ? shot : Shot.None;
            askedOf = of;
            askedFor = seconds;
        }

        /// <summary>Back to the ship: ends the shot on now (a meeting's too, until its next stage).</summary>
        public void Back()
        {
            asked = Shot.None;
            skipped = true;
        }

        /// <summary>Whether the ship's camera is driving the main camera (the level's OrbitCamera, on and enabled).</summary>
        static bool ShipCameraOn(Camera cam)
        {
            var orbit = cam.GetComponent<Kapman.Levels.OrbitCamera>();
            return orbit != null && orbit.isActiveAndEnabled;
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            var game = KapmanGame.Current;
            if (cam == null || !ShipCameraOn(cam) || game == null || game.ship == null || game.World == null
                || (MapView.Current != null && MapView.Current.IsOpen))
            {
                Now = Shot.None;
                blend = 1f;
                FocusDistance = 0f;
                return;
            }
            since += Time.unscaledDeltaTime;
            var (shot, of) = Called();
            if (shot != Shot.None && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
                && !(MarketPanel.Current != null && MarketPanel.Current.IsOpen))
                skipped = true;
            if (shot != Shot.None && !skipped && Frame(shot, of, game.ship, cam, out var pos, out var rot, out float fov, out float focus))
            {
                cam.transform.SetPositionAndRotation(pos, rot);           // (a hard cut in)
                cam.fieldOfView = fov;
                lastPos = pos;
                lastRot = rot;
                lastFov = fov;
                lastFocus = focus;
                FocusDistance = focus;
                Now = shot;
                blend = 0f;
                return;
            }
            Now = Shot.None;
            if (blend >= 1f)
            {
                FocusDistance = 0f;
                return;
            }
            // the blend back: from the shot's last frame to where the ship's camera put the camera this frame
            blend = Mathf.Min(1f, blend + Time.unscaledDeltaTime / Mathf.Max(0.01f, blendBack));
            float e = blend * blend * (3f - 2f * blend);
            float shipFocus = Vector3.Distance(cam.transform.position, game.ShipPosition);
            cam.transform.SetPositionAndRotation(Vector3.Lerp(lastPos, cam.transform.position, e), Quaternion.Slerp(lastRot, cam.transform.rotation, e));
            cam.fieldOfView = Mathf.Lerp(lastFov, cam.fieldOfView, e);
            FocusDistance = blend < 1f ? Mathf.Max(3f, Mathf.Lerp(lastFocus, shipFocus, e)) : 0f;
        }

        /// <summary>The shot called for now: a graph's, else the meeting's stage's.</summary>
        (Shot, SimShip) Called()
        {
            string key = $"{Encounter.Stage}/{RaiderEncounter.Stage}/{asked}/{askedOf?.index}";
            if (key != stageKey)
            {
                stageKey = key;
                skipped = false;
                since = 0f;
            }
            if (asked != Shot.None)
            {
                if (since < askedFor)
                    return (asked, askedOf);
                asked = Shot.None;
            }
            if (!followMeetings)
                return (Shot.None, null);
            switch (RaiderEncounter.Stage)
            {
                case RaidStage.Threat: return (Shot.Sighting, RaiderEncounter.Band);
                case RaidStage.Running: return since < chaseSeconds ? (Shot.Chase, RaiderEncounter.Band) : (Shot.None, null);
                case RaidStage.Caught: return (Shot.TwoShot, RaiderEncounter.Band);
            }
            switch (Encounter.Stage)
            {
                case EncounterStage.Offered: return (Shot.Sighting, Encounter.Ship);
                case EncounterStage.Alongside: return (Shot.TwoShot, Encounter.Ship);
            }
            return (Shot.None, null);
        }

        /// <summary>Where the camera goes for a shot, which way it looks, its lens, and how far its subject is.</summary>
        public bool Frame(Shot shot, SimShip of, SailingShip mine, Camera cam, out Vector3 pos, out Quaternion rot, out float fov, out float focus)
        {
            pos = default;
            rot = Quaternion.identity;
            fov = cam.fieldOfView;
            focus = 0f;
            if (of == null || of.cls == null)
                return false;
            int ranks = (of.raider ? Mathf.Clamp(of.bandShips, 1, 4) : 1) / 2;   // a band's others, in ranks astern of her
            float len = Mathf.Max(6f, of.cls.length);
            // her (or her band's middle: the others lie in echelon astern of her)
            Vector3 her = WorldShips3D.Where(of) + Vector3.up * 2f
                        + Quaternion.Euler(0f, (float)of.heading, 0f) * Vector3.back * (0.8f * len * ranks);
            float spread = len * (1f + 1.3f * ranks);
            Vector3 me = mine.transform.position + Vector3.up * 1.6f;
            Vector3 flat = her - me;
            flat.y = 0f;
            float apart = flat.magnitude;
            if (apart < 0.5f)
                return false;
            Vector3 dir = flat / apart;
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            float aspect = Mathf.Max(0.3f, cam.aspect);
            Vector3 target = her;
            switch (shot)
            {
                case Shot.Sighting:                                        // over her shoulder: astern of you on the line to her
                    pos = me - dir * 14f + side * 4f + Vector3.up * 4f;
                    break;
                case Shot.Chase:                                           // ahead of you, looking back past your boat at them
                    pos = me - dir * 20f + side * 6f + Vector3.up * 3f;
                    break;
                case Shot.TwoShot:
                {
                    Vector3 mid = (me + her) * 0.5f;
                    // from abeam of the pair, on the side the camera was (the line between them isn't crossed)
                    float s = Vector3.Dot(cam.transform.position - mid, side) < 0f ? -1f : 1f;
                    float hfov = 2f * Mathf.Atan(Mathf.Tan(twoShotFov * 0.5f * Mathf.Deg2Rad) * aspect);
                    float dist = Mathf.Max(20f, (apart * 0.5f + spread) / Mathf.Tan(hfov * 0.5f) * 1.15f);
                    float up = twoShotElevation * Mathf.Deg2Rad;
                    pos = mid + side * (s * dist * Mathf.Cos(up)) + Vector3.up * (dist * Mathf.Sin(up));
                    target = mid;
                    fov = twoShotFov;
                    break;
                }
                default:
                    return false;
            }
            float floor = SimStage.WaterHeight(pos) + 1.2f;                 // ported 4 Oct: was WaterSource.Active.Sample
            if (pos.y < floor)
                pos.y = floor;
            focus = Vector3.Distance(pos, target);
            if (shot != Shot.TwoShot)
            {
                // the long lens: her (or the band) filling so much of the frame's width
                float hfov = 2f * Mathf.Atan(spread / Mathf.Max(0.1f, fill) * 0.5f / Mathf.Max(1f, focus));
                fov = Mathf.Clamp(2f * Mathf.Atan(Mathf.Tan(hfov * 0.5f) / aspect) * Mathf.Rad2Deg, minFov, maxFov);
            }
            // looking a little under the subject, so she sits in the upper part of the frame, clear of the panel below
            rot = Quaternion.LookRotation(target - pos, Vector3.up) * Quaternion.Euler(fov * raise * 0.5f, 0f, 0f);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState() => FocusDistance = 0f;
    }
}
