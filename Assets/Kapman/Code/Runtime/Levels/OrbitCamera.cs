using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// THE VOYAGE CAMERA (the user, 4 Oct pm: Design/open_world_isometric.md).
    /// Near land it is ISOMETRIC: a long lens (20 degrees) high over the ship at 50 degrees down, following her; the wheel
    /// zooms between a harbour view (about 150 m across) and a reach view (about 600 m across), Q and E turn it round her
    /// (or right-drag across), WASD or the middle button wander off, F comes back to the ship. A real lens, not an
    /// orthographic one: HDRP's volumetric clouds and fog need perspective.
    /// On the OPEN SEA, more than freeFromLand metres from any land, the free camera unlocks: the old orbit (right-drag
    /// turns and tilts, the wheel zooms from the planks to high over the water); sailing back toward land eases it back to
    /// the isometric view. The change blends over a second or two.
    /// The grass and the trees follow the camera: grass only when zoomed in close enough to see it (beyond that a grass
    /// card is smaller than a pixel and only shimmers), trees to the edge of what the view can show.
    /// </summary>
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        [Header("Free (the open sea)")]
        public float distance = 40f, minDistance = 3f, maxDistance = 1500f;
        public float yaw = 30f, pitch = 14f;
        public float minPitch = -5f, maxPitch = 85f;
        [Header("Isometric (near land)")]
        public bool isometric = true;
        public float isoPitch = 35f, isoFov = 35f;                // view B1 (4 Oct pm), then "slightly higher ... looking down on an angle more"
        [Tooltip("The ground the view spans across at the focus (m): the closest and the widest zoom.")]
        public float isoMinWidth = 25f, isoMaxWidth = 600f;
        public float isoWidth = 35f;
        [Tooltip("Zoomed out, the view rises to this angle (so the inland stays under its fog).")]
        public float isoPitchFar = 48f;
        [Tooltip("The waterfront band each side of the water (m): the widest view is the water plus both bands and a little fog.")]
        public float bandWidth = 60f;
        /// <summary>Scrolled out past the widest view: the map takes over (through the clouds).</summary>
        public static event System.Action ZoomedPastWidest;
        /// <summary>The widest view here (m across): the water's width across the view, both bands and a little of the fog.</summary>
        public float Widest { get; private set; } = 300f;
        [Tooltip("The free camera unlocks this far from any land (m).")]
        public float freeFromLand = 3000f;
        [Tooltip("Grass is drawn while the view spans no more than this (m).")]
        public float grassUpToWidth = 130f;
        [Header("The lens near land")]
        [Tooltip("A slight bulge (the user: \"slightly warping and enlarging the view of the boat\"): HDRP's lens distortion, positive enlarges the middle.")]
        [Range(-0.6f, 0.6f)] public float lensNearLand = 0.25f;
        [Tooltip("A little zoom so the bulge's corners stay filled.")]
        public float lensCrop = 1.08f;
        [Header("Freedom round the ship (near land)")]
        [Tooltip("The box round the ship the view may move in (WASD), as a share of the view's width: bigger zoomed out.")]
        public float boxShare = 0.35f;
        [Tooltip("Seconds the view takes to spring back after a right-drag.")]
        public float rubberSeconds = 0.6f;
        [Tooltip("A little more room zoomed out than the water and its bands.")]
        public float zoomOutRoom = 1.3f;
        Vector3 panOffset, panVelocity;
        [Header("Feeling the ship")]
        [Tooltip("How much of the ship's heel and pitch the camera takes: softly in the isometric view, more at sea.")]
        [Range(0f, 1f)] public float rockIso = 0.25f, rockFree = 0.45f;

        /// <summary>The point the camera looks at (the ship, or where the player wandered to).</summary>
        public Vector3 Focus => focus;
        /// <summary>Whether the free camera is unlocked (the open sea, far from land).</summary>
        public bool FreeNow { get; private set; }
        public static OrbitCamera Current { get; private set; }

        Vector3 focus;
        bool follow = true;
        float smoothDistance, isoBlend = 1f, freeFov = 50f, nextLandCheck;
        Camera cam;

        void OnEnable() => Current = this;

        void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        void Start()
        {
            focus = target != null ? target.position + Vector3.up * 1.5f : transform.position + transform.forward * distance;
            smoothDistance = distance;
            cam = GetComponent<Camera>();
            if (cam != null)
                freeFov = cam.fieldOfView;
            isoBlend = isometric ? 1f : 0f;
            foreach (var v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
                if (v.isGlobal && v.profile != null)
                {
                    var profile = v.profile;                                  // (the runtime copy: the asset is never changed)
                    if (!profile.TryGet(out lens))
                        lens = profile.Add<LensDistortion>(true);
                    break;
                }
        }

        LensDistortion lens;
        float lastLens = float.NaN, pastWidest, isoPitchNow = 35f;

        /// <summary>The view at its widest (coming down from the map).</summary>
        public void ToWidest()
        {
            isoWidth = Widest;
            follow = true;
        }

        /// <summary>The water's width across the view at a point (m): out each way until the ground stands above the water.</summary>
        static float WaterAcross(Vector3 at, Vector3 right)
        {
            var info = LevelInfo.Current;
            float wl = info != null ? info.WaterLevel : 0f;
            right.y = 0f;
            right = right.sqrMagnitude > 1e-4f ? right.normalized : Vector3.right;
            float Side(Vector3 dir)
            {
                for (float r = 5f; r <= 600f; r += 5f)
                    if (ShipMotion.Ground(at + dir * r) > wl)
                        return r;
                return 600f;
            }
            return Side(right) + Side(-right);
        }

        /// <summary>The bulge near land, none at sea (eased with the view).</summary>
        void ApplyLens(float strength)
        {
            if (lens == null || Mathf.Abs(strength - lastLens) < 0.003f)
                return;
            lastLens = strength;
            lens.active = Mathf.Abs(strength) > 0.003f;
            lens.intensity.Override(strength);
            lens.scale.Override(Mathf.Lerp(1f, lensCrop, Mathf.Abs(strength) / Mathf.Max(0.01f, Mathf.Abs(lensNearLand))));
        }

        /// <summary>The camera's distance for a view spanning this width at the focus, with the iso lens.</summary>
        float IsoDistance(float width)
        {
            float aspect = cam != null ? cam.aspect : 16f / 9f;
            float hHalf = Mathf.Atan(Mathf.Tan(isoFov * 0.5f * Mathf.Deg2Rad) * aspect);
            return width / (2f * Mathf.Tan(hHalf));
        }

        /// <summary>Whether there is land (ground above the water) within this distance of a point: twelve bearings at a
        /// few ranges, from the level's ground.</summary>
        static bool LandWithin(Vector3 p, float metres)
        {
            var info = LevelInfo.Current;
            float wl = info != null ? info.WaterLevel : 0f;
            if (ShipMotion.Ground(p) > wl)
                return true;
            for (float r = metres / 4f; r <= metres + 1f; r += metres / 4f)
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI / 6f;
                    if (ShipMotion.Ground(p + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * r) > wl)
                        return true;
                }
            return false;
        }

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            var m = Mouse.current;
            var kb = Keyboard.current;

            // near land or the open sea (checked twice a second)
            if (Time.unscaledTime >= nextLandCheck)
            {
                nextLandCheck = Time.unscaledTime + 0.5f;
                Widest = (WaterAcross(target != null ? target.position : focus, transform.right) + 2.5f * bandWidth) * zoomOutRoom;
                Widest = Mathf.Clamp(Widest, isoMinWidth * 2f, isoMaxWidth);
                Vector3 at = target != null ? target.position : focus;
                var lvl = LevelInfo.Current;
                float scale = lvl != null ? Mathf.Max(1f, lvl.realMetresPerUnit) : 1f;   // (the shortened North Sea: 150 real m a unit)
                FreeNow = !isometric || !LandWithin(at, freeFromLand / scale);
            }
            isoBlend = Mathf.MoveTowards(isoBlend, FreeNow ? 0f : 1f, dt / 1.5f);
            bool iso = isoBlend > 0.5f;

            Vector3 flatFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            bool dragging = false;
            if (m != null && !DevHud.PointerOverPanel)
            {
                var d = m.delta.ReadValue();
                if (m.rightButton.isPressed)
                {
                    if (iso)
                    {
                        // near land: hold the right button and drag the view where you like; let go and it springs
                        // back to the ship (the user, 4 Oct pm)
                        dragging = true;
                        float perPixel = isoWidth / Mathf.Max(1f, Screen.width);
                        panOffset -= (flatRight * d.x + flatFwd * d.y) * perPixel;
                        float far = 1.5f * Widest;
                        if (panOffset.magnitude > far)
                            panOffset = panOffset.normalized * far;
                        panVelocity = Vector3.zero;
                    }
                    else
                    {
                        yaw += d.x * 0.18f;
                        pitch -= d.y * 0.14f;
                    }
                }
                if (m.middleButton.isPressed)
                {
                    if (iso)
                        yaw += d.x * 0.18f;                                   // near land the middle button turns the view
                    else
                    {
                        follow = false;
                        focus -= (flatRight * d.x + flatFwd * d.y) * distance * 0.0015f;
                    }
                }
                float s = m.scroll.ReadValue().y;
                if (Mathf.Abs(s) > 0.01f)
                {
                    float k = Mathf.Pow(0.88f, Mathf.Clamp(s / 120f, -3f, 3f));
                    if (iso)
                    {
                        // scrolled on out at the widest: up through the clouds to the map
                        if (s < 0f && isoWidth >= Widest * 0.97f)
                        {
                            pastWidest += -s / 120f;
                            if (pastWidest >= 1.5f)
                            {
                                pastWidest = 0f;
                                ZoomedPastWidest?.Invoke();
                            }
                        }
                        else
                            pastWidest = 0f;
                        isoWidth *= k;
                    }
                    else
                        distance *= k;
                }
            }
            if (kb != null)
            {
                Vector3 move = Vector3.zero;
                if (kb.wKey.isPressed) move += flatFwd;
                if (kb.sKey.isPressed) move -= flatFwd;
                if (kb.dKey.isPressed) move += flatRight;
                if (kb.aKey.isPressed) move -= flatRight;
                if (move != Vector3.zero)
                {
                    if (iso)
                        panOffset += move.normalized * isoWidth * 0.5f * dt;     // inside the box round the ship (below)
                    else
                    {
                        follow = false;
                        focus += move.normalized * Mathf.Max(5f, distance) * 0.8f * dt;
                    }
                }
                if (kb.qKey.isPressed) yaw -= 60f * dt;
                if (kb.eKey.isPressed) yaw += 60f * dt;
                if (kb.fKey.wasPressedThisFrame)
                {
                    follow = true;
                    panOffset = Vector3.zero;
                }
            }
            if (iso)
            {
                // the box round the ship, bigger as you zoom out: WASD moves the view inside it and it stays; a drag beyond it
                // springs back to the box when the button is let go
                float half = boxShare * isoWidth;
                Vector3 inBox = flatRight * Mathf.Clamp(Vector3.Dot(panOffset, flatRight), -half, half)
                              + flatFwd * Mathf.Clamp(Vector3.Dot(panOffset, flatFwd), -half, half);
                if (!dragging)
                    panOffset = Vector3.SmoothDamp(panOffset, inBox, ref panVelocity, rubberSeconds, Mathf.Infinity, dt);
                if (target != null)
                    focus = Vector3.Lerp(focus, target.position + Vector3.up * 1.5f + panOffset, 1f - Mathf.Exp(-dt * (dragging ? 20f : 6f)));
            }
            else if (follow && target != null)
                focus = Vector3.Lerp(focus, target.position + Vector3.up * 1.5f, 1f - Mathf.Exp(-dt * 6f));
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            isoWidth = Mathf.Clamp(isoWidth, isoMinWidth, Widest);
            // the angle rises as the view widens: low and close at the ship, higher at the widest
            float zoomShare = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(Mathf.Log(60f), Mathf.Log(Mathf.Max(61f, Widest)), Mathf.Log(isoWidth)));
            isoPitchNow = Mathf.Lerp(isoPitch, isoPitchFar, zoomShare);

            // the two views, blended
            float wantDistance = Mathf.Lerp(distance, IsoDistance(isoWidth), isoBlend);
            float usePitch = Mathf.Lerp(pitch, isoPitchNow, isoBlend);
            smoothDistance = Mathf.Lerp(smoothDistance, wantDistance, 1f - Mathf.Exp(-dt * 8f));
            if (cam != null)
                cam.fieldOfView = Mathf.Lerp(freeFov, isoFov, isoBlend);

            Quaternion rot = Quaternion.Euler(usePitch, yaw, 0f);
            Vector3 pos = focus - rot * Vector3.forward * smoothDistance;
            var info = LevelInfo.Current;
            float floor = Mathf.Max(ShipMotion.Ground(pos) + 1.2f, (info != null ? info.WaterLevel : 0f) + 0.8f);
            if (pos.y < floor)
                pos.y = floor;
            var look = Quaternion.LookRotation(focus - pos, Vector3.up);
            // the camera feels the ship (the user, 4 Oct: "bounce and rock softly with the ship"): her heel as seen from
            // where the camera stands tilts the view a little, her pitch nods it, her heave lifts it; softly sprung
            float rockShare = Mathf.Lerp(rockFree, rockIso, isoBlend);
            float wantRoll = 0f, wantNod = 0f, heave = 0f;
            if (target != null && rockShare > 0f)
            {
                Vector3 shipUp = target.up;
                Vector3 camRight = look * Vector3.right, camFwd = Vector3.ProjectOnPlane(look * Vector3.forward, Vector3.up).normalized;
                wantRoll = -Mathf.Asin(Mathf.Clamp(Vector3.Dot(shipUp, camRight), -1f, 1f)) * Mathf.Rad2Deg * rockShare;
                wantNod = Mathf.Asin(Mathf.Clamp(Vector3.Dot(shipUp, camFwd), -1f, 1f)) * Mathf.Rad2Deg * rockShare * 0.5f;
                float y = target.position.y;
                meanY = float.IsNaN(meanY) ? y : Mathf.Lerp(meanY, y, 1f - Mathf.Exp(-dt / 4f));
                heave = (y - meanY) * Mathf.Lerp(0.8f, 0.3f, isoBlend);
            }
            rockRoll = Mathf.SmoothDamp(rockRoll, Mathf.Clamp(wantRoll, -6f, 6f), ref rollSpeed, 0.35f, Mathf.Infinity, dt);
            rockNod = Mathf.SmoothDamp(rockNod, Mathf.Clamp(wantNod, -4f, 4f), ref nodSpeed, 0.35f, Mathf.Infinity, dt);
            rockHeave = Mathf.SmoothDamp(rockHeave, heave, ref heaveSpeed, 0.25f, Mathf.Infinity, dt);
            transform.SetPositionAndRotation(pos + Vector3.up * rockHeave, look * Quaternion.Euler(rockNod, 0f, rockRoll));
            FollowDetail(iso);
            ApplyLens(lensNearLand * isoBlend);
        }

        float lastGrass = -1f, lastTrees = -1f;
        float rockRoll, rockNod, rockHeave, rollSpeed, nodSpeed, heaveSpeed, meanY = float.NaN;

        /// <summary>The grass and trees drawn to what this view can show (changed only when it moves by a tenth): the
        /// low view sees the ground out to where the frame's top edge meets it, and the trees stand up beyond that.</summary>
        void FollowDetail(bool iso)
        {
            float grass, trees;
            if (iso)
            {
                float height = Mathf.Max(2f, transform.position.y - (LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : 0f));
                float topBelow = Mathf.Max(2f, isoPitchNow - (cam != null ? cam.fieldOfView : isoFov) * 0.5f);
                float far = height / Mathf.Tan(topBelow * Mathf.Deg2Rad);
                grass = isoWidth <= grassUpToWidth ? Mathf.Min(far, 140f) : 0f;
                trees = Mathf.Min(far * 2.5f + 100f, 1500f);
            }
            else
            {
                grass = LevelPerformance.GrassDistance;
                trees = 2000f;
            }
            if (Mathf.Abs(grass - lastGrass) < 0.1f * Mathf.Max(lastGrass, 10f) && Mathf.Abs(trees - lastTrees) < 0.1f * Mathf.Max(lastTrees, 10f))
                return;
            lastGrass = grass;
            lastTrees = trees;
            foreach (var t in Terrain.activeTerrains)
            {
                if (t == null)
                    continue;
                t.detailObjectDistance = grass;
                t.treeDistance = trees;
            }
        }

        /// <summary>Puts the camera at a view at once (for the shots: always the free camera's view).</summary>
        public void Place(Vector3 newFocus, float newYaw, float newPitch, float newDistance)
        {
            focus = newFocus;
            yaw = newYaw;
            pitch = newPitch;
            distance = smoothDistance = newDistance;
            follow = false;
            panOffset = panVelocity = Vector3.zero;
            isometric = false;
            FreeNow = true;
            isoBlend = 0f;
            if (cam != null)
                cam.fieldOfView = freeFov;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(focus - rot * Vector3.forward * distance, rot);
        }
    }
}
