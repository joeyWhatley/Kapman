// First run and looked at on 7 Oct 2026 (Design/ship_view/BUILD_LOG.md). The seats, the oars' sweep and the sail's brace
// are worked out from the pack's measures; the sweep and the brace have only been seen in stills.
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE BOAT ON THE STAGE: the pack's longship (SMV_Ship) shown at her class's length, floated on the stage's sea.
    ///   - Model: the pack's hull, mast, sails and steering oar, scaled as one by length / 17.88 (0.43 for the 7.7 m
    ///     Gislinge boat), floated on the pack's own waterline (ShipStageKit.modelWaterline). The longship's shields, oars, benches and deck
    ///     clutter are switched off (ShipStageKit.hide).
    ///   - FitOut: what is ours and at full size: a thwart for each pair of oars, the oars themselves, and the places the
    ///     crew rig seats its men (Seats face aft, HelmSeat by the steering oar on the starboard quarter).
    /// She floats as the 3D levels' ShipMotion does: the water under her bow, stern, sides and middle gives her height,
    /// pitch and roll; under sail she heels a little to leeward and the sail is braced round to the wind.
    /// The oars sweep by <see cref="StrokePhase"/> (0 the catch, 0.4 the release) unless a crew rig drives them.
    /// </summary>
    public sealed class StageShip
    {
        public Transform Root { get; private set; }
        public Transform Model { get; private set; }
        public Transform FitOut { get; private set; }
        public Transform SailOpen { get; private set; }
        public Transform SailFurled { get; private set; }
        public Transform SteerOar { get; private set; }
        public Transform HelmSeat { get; private set; }
        public readonly List<Transform> Seats = new List<Transform>();
        public readonly List<Transform> OarsPort = new List<Transform>(), OarsStarboard = new List<Transform>();
        /// <summary>Where her mooring lines are made fast, on her port side (in Root's axes).</summary>
        public Vector3 BowCleat { get; private set; }
        public Vector3 SternCleat { get; private set; }

        /// <summary>The model's scale (her class's length over the model's).</summary>
        public float Scale { get; private set; } = 1f;
        /// <summary>The length and beam she is drawn at, and the heights of her boards and her gunwale over the water at rest.</summary>
        public float Length { get; private set; } = 7.7f;
        public float Beam { get; private set; } = 2.1f;
        public float Deck { get; private set; }
        public float Gunwale { get; private set; }
        /// <summary>A thwart's height over her boards (m).</summary>
        public float SeatHeight { get; private set; }
        /// <summary>How far out the oars are: 0 shipped along the boards, 1 out and pulling.</summary>
        public float OarsOut => oarsOut;
        /// <summary>An oar's length outboard of its thole, to the blade's end (m).</summary>
        public float BladeOut => kit.oarOutboard;
        /// <summary>The stroke: 0 the catch (blades in, forward), 0.4 the release, back to 1.</summary>
        public float StrokePhase { get; private set; }
        /// <summary>A crew rig moves the oars itself.</summary>
        public bool OarsDriven;
        /// <summary>A crew rig sets and furls the sail itself (SailOpen and SailFurled are its to switch); the stage still
        /// braces the set sail to the wind.</summary>
        public bool SailDriven;
        /// <summary>Where each oar turns on the gunwale (in FitOut's axes), port then starboard for each thwart, as the oars
        /// in OarsPort and OarsStarboard.</summary>
        public IReadOnlyList<Vector3> OarPivots => oarPivots;
        /// <summary>Two more places on each thwart, to port and to starboard of the middle one, for a rig that sits two men
        /// to a thwart with an oar each: port then starboard for each thwart. They face aft as Seats do.</summary>
        public readonly List<Transform> SideSeats = new List<Transform>();
        public float strokesPerMinute = 22f, thwartSpacing = 0.9f;

        ShipStageKit kit;
        ShipStage stage;
        float pitch, roll, height, brace, halfBeam, oarsOut;
        string fittedClass;
        float fittedLength = -1f;
        int fittedOars = -1, fittedCrew = -1;
        readonly List<Vector3> oarPivots = new List<Vector3>();

        public void Build(ShipStage stage, ShipStageKit kit, Transform parent)
        {
            this.stage = stage;
            this.kit = kit;
            Root = new GameObject("Ship").transform;
            Root.SetParent(parent, false);
            // (with no pack: a box of the longship's size, so the scale and the float can still be looked at)
            var model = stage.Spawn(kit.ship, Root, "Model", PrimitiveType.Cube, new Vector3(kit.modelBeam * 0.8f, 1.6f, kit.modelLength));
            Model = model.transform;
            var all = model.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t == Model)
                    continue;
                if (System.Array.IndexOf(kit.hide, t.name) >= 0)
                    t.gameObject.SetActive(false);
                else if (t.name == "Ship_Sail_Open") SailOpen = t;
                else if (t.name == "Ship_Sail_Close") SailFurled = t;
                else if (t.name == "ShipSteerOar_1") SteerOar = t;
            }
            FitOut = new GameObject("Fit-out").transform;
            FitOut.SetParent(Root, false);
            MakeLid();
        }

        GameObject lid;
        Mesh lidMesh;

        /// <summary>
        /// THE LID THAT KEEPS HDRP'S SEA OUT OF HER: a flat sheet just inside her rail and a little under it, drawn with
        /// HDRP's own water exclusion material (it only marks where water is not to be drawn). Without it a wave that
        /// stands higher than her boards is drawn inside the hull. The look lab and the 3D levels do the same
        /// (HdrpWater.AddExcluder). Made in the model's own metres from the rail's measures (ShipRail), under the model,
        /// so it is scaled with her; tapered on to the stems beyond the measured rail, kept low there so it never cuts
        /// the sea away outside her. Only shown while HDRP's water is the sea drawn.
        /// </summary>
        void MakeLid()
        {
            if (kit.waterExclusion == null || kit.ship == null)
                return;
            const float inset = 0.10f, drop = 0.12f;
            var sections = new List<Vector3>();                              // (half-breadth, height, z), stern to bow
            ShipRail.At(-100f, out float endX, out float endY);
            sections.Add(new Vector3(0.25f, endY - drop, -7.1f));
            for (float z = -5.25f; z <= 5.2501f; z += 0.75f)
            {
                ShipRail.At(z, out float x, out float y);
                sections.Add(new Vector3(Mathf.Max(0.05f, x - inset), y - drop, z));
            }
            sections.Add(new Vector3(0.25f, endY - drop, 7.1f));
            var v = new Vector3[sections.Count * 2];
            var tris = new int[(sections.Count - 1) * 6];
            for (int i = 0; i < sections.Count; i++)
            {
                v[i * 2] = new Vector3(-sections[i].x, sections[i].y, sections[i].z);
                v[i * 2 + 1] = new Vector3(sections[i].x, sections[i].y, sections[i].z);
            }
            for (int i = 0, t = 0; i + 1 < sections.Count; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                tris[t++] = a; tris[t++] = c; tris[t++] = b;                 // (clockwise seen from above: HDRP culls the back)
                tris[t++] = b; tris[t++] = c; tris[t++] = d;
            }
            lidMesh = new Mesh { name = "Ship stage hull lid", hideFlags = HideFlags.HideAndDontSave };
            lidMesh.vertices = v;
            lidMesh.triangles = tris;
            lidMesh.RecalculateNormals();
            lidMesh.RecalculateBounds();
            lid = new GameObject("Water excluder (keeps the sea out of the hull)");
            lid.transform.SetParent(Model, false);
            lid.AddComponent<MeshFilter>().sharedMesh = lidMesh;
            var r = lid.AddComponent<MeshRenderer>();
            r.sharedMaterial = kit.waterExclusion;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            lid.SetActive(false);
        }

        /// <summary>Shows the lid (HDRP's water is the sea drawn) or hides it.</summary>
        public void LidOn(bool on)
        {
            if (lid != null)
                lid.SetActive(on);
        }

        public void Destroy()
        {
            if (lidMesh != null)
                Object.Destroy(lidMesh);
            lidMesh = null;
        }

        /// <summary>Sizes her to her class and fits her out again, when the class, its length or its oars change.</summary>
        public void Fit(in ShipStageState s)
        {
            float wantLength = kit.shownLength > 0f ? kit.shownLength : s.length;
            if (s.classId == fittedClass && Mathf.Abs(wantLength - fittedLength) < 0.01f && s.oars == fittedOars && s.crew == fittedCrew)
                return;
            fittedCrew = s.crew;
            fittedClass = s.classId;
            fittedLength = wantLength;
            fittedOars = s.oars;
            // (kit.scaleToClass off: the model at her own size, whatever the class: a longship, not the player's boat)
            Length = kit.scaleToClass ? Mathf.Max(2f, wantLength) : kit.modelLength;
            Scale = Length / kit.modelLength;
            Beam = kit.modelBeam * Scale;
            // she floats on the line the model was made for (the pack's demo: 1.2 m over its origin), scaled with her. That
            // puts 0.69 m of the 7.7 m boat under water where her class draws 0.25 m: not seen, and her rail comes out
            // 0.38 m over the water, where the class's freeboard is 0.34 m
            float originUp = -kit.modelWaterline * Scale;
            Model.localScale = Vector3.one * Scale;
            Model.localPosition = new Vector3(0f, originUp, 0f);
            Deck = kit.deckHeight * Scale + originUp;
            Gunwale = kit.gunwaleHeight * Scale + originUp;
            halfBeam = Beam * 0.5f * 0.93f;                                  // (the gunwale's half-breadth amidships, where the oars are)

            for (int i = FitOut.childCount - 1; i >= 0; i--)
                Object.Destroy(FitOut.GetChild(i).gameObject);
            Seats.Clear();
            SideSeats.Clear();
            OarsPort.Clear();
            OarsStarboard.Clear();
            oarPivots.Clear();
            // (kit.fitOut off: the ship prefab brings her own thwarts, oars and men, as the other builder's seated ship does)
            // THE ROWERS (the lead's ruling, 7 Oct 2026): all her men but the one at the steering oar row, each on a thwart
            // of his own; a man pulls a PAIR of oars where her rail is narrow enough for one man to reach both (kit.pairBeam),
            // else ONE oar with both hands, sitting by his own side as a longship's rowers did, the sides taken in turn.
            int rowers = kit.fitOut ? Mathf.Clamp(Mathf.Min(Mathf.Max(1, s.crew - 1), Mathf.Max(1, s.oars / 2)), 1, Mathf.Max(1, Mathf.FloorToInt(Length * 0.5f / thwartSpacing))) : 0;
            // a thwart is a seat's height over the boards where her side allows it, and never nearer her rail's top than a
            // hand's breadth and a half (the looms must pass over the men's thighs)
            float seatUp = Mathf.Clamp(Gunwale - Deck - 0.14f, 0.08f, 0.27f);
            SeatHeight = seatUp;
            float mastZ = kit.mastAhead * Scale;
            // (pairs or single oars for the whole boat, by her widest rail at any thwart: never some of each)
            bool pair = true;
            for (int i = 0; i < rowers; i++)
            {
                ShipRail.At((mastZ + 0.6f - i * thwartSpacing) / Scale, out float widest, out _);
                pair &= 2f * widest * Scale <= kit.pairBeam;
            }
            for (int i = 0; i < rowers; i++)
            {
                // the first sits just ahead of the mast with his back to the bow, the others abaft it, a stroke's room apart
                float z = mastZ + 0.6f - i * thwartSpacing;
                // (she narrows fore and aft: the thwart is as wide as her rail is there, and the tholes stand on that rail)
                ShipRail.At(z / Scale, out float thwartRailX, out _);
                float halfHere = thwartRailX * Scale;
                int only = pair ? 0 : (i % 2 == 0 ? -1 : 1);                  // (one oar each: port, starboard, port ...)
                var thwart = stage.Spawn(kit.thwart, FitOut, "Thwart " + (i + 1), PrimitiveType.Cube, new Vector3(kit.thwartLength, 0.06f, 0.3f));
                thwart.transform.localPosition = new Vector3(0f, Deck + seatUp - 0.04f, z);
                thwart.transform.localScale = Vector3.Scale(thwart.transform.localScale, new Vector3(2f * halfHere * 0.94f / kit.thwartLength, 1f, 0.7f));
                var seat = new GameObject("Seat " + (i + 1)).transform;
                seat.SetParent(FitOut, false);
                seat.localPosition = new Vector3(pair ? 0f : only * Mathf.Max(0f, halfHere - 0.55f), Deck + seatUp, z);
                seat.localRotation = Quaternion.Euler(0f, 180f, 0f);        // a rower faces aft
                Seats.Add(seat);
                for (int side = -1; side <= 1; side += 2)
                {
                    float tholeZ = z - 0.3f;                                  // the thole lies aft of the thwart
                    ShipRail.At(tholeZ / Scale, out float railX, out float railY);
                    oarPivots.Add(new Vector3(side * (railX - 0.03f) * Scale, (railY - kit.modelWaterline) * Scale + 0.03f, tholeZ));
                    // (the lists keep one place a thwart on each side: null where a thwart has no oar on that side)
                    Transform shaft = null;
                    if (pair || side == only)
                        shaft = stage.Spawn(kit.oar, FitOut, (side < 0 ? "Oar port " : "Oar starboard ") + (i + 1), PrimitiveType.Cylinder, new Vector3(0.06f, kit.oarOutboard * 0.5f, 0.06f)).transform;
                    (side < 0 ? OarsPort : OarsStarboard).Add(shaft);
                    var beside = new GameObject((side < 0 ? "Seat port " : "Seat starboard ") + (i + 1)).transform;
                    beside.SetParent(FitOut, false);
                    beside.localPosition = new Vector3(side * halfHere * 0.45f, Deck + seatUp, z);
                    beside.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    SideSeats.Add(beside);
                }
            }
            // the helmsman: on the starboard quarter by the steering oar, just ahead of her raised after deck, on a low
            // seat of his own (a sea chest's height)
            HelmSeat = new GameObject("Helm seat").transform;
            HelmSeat.SetParent(FitOut, false);
            HelmSeat.localPosition = new Vector3(0.25f, Deck + seatUp, -0.34f * Length);
            var chest = stage.Spawn(kit.crate, FitOut, "Helm seat (a chest)", PrimitiveType.Cube, Vector3.one * 0.7f);
            chest.transform.localPosition = new Vector3(0.25f, Deck, -0.34f * Length - 0.05f);
            chest.transform.localScale = Vector3.Scale(chest.transform.localScale, new Vector3(0.62f, Mathf.Max(0.1f, (seatUp - 0.02f) / 0.71f), 0.5f));
            BowCleat = new Vector3(-halfBeam * 0.5f, Gunwale + 0.1f, 0.4f * Length);
            SternCleat = new Vector3(-halfBeam * 0.6f, Gunwale + 0.1f, -0.38f * Length);
            stage.Adopt(FitOut.gameObject);
        }

        /// <summary>Floats her on the sea of this moment, sets or furls her sail, and sweeps her oars.</summary>
        public void Tick(in ShipStageState s, StageSea sea, float dt, bool snap)
        {
            Fit(s);
            float a = s.heading * Mathf.Deg2Rad;
            float fe = Mathf.Sin(a), fn = Mathf.Cos(a), re = fn, rn = -fe;       // ahead and to starboard, as east and north
            float hl = Length * 0.5f, hb = Beam * 0.5f;
            float bow = stage.Height(fe * hl, fn * hl), stern = stage.Height(-fe * hl, -fn * hl);
            float port = stage.Height(-re * hb, -rn * hb), star = stage.Height(re * hb, rn * hb), mid = stage.Height(0f, 0f);
            float wantY = (mid * 2f + bow + stern + port + star) / 6f;
            float wantPitch = Mathf.Atan2(stern - bow, Length) * Mathf.Rad2Deg;         // bow down +
            float wantRoll = Mathf.Atan2(star - port, Beam) * Mathf.Rad2Deg * 0.6f;     // starboard side up +
            // (a crew rig that works the sail itself is believed: the sail is set when its mesh is shown)
            bool set = SailDriven && SailOpen != null ? SailOpen.gameObject.activeSelf : s.SailSet;
            float off = s.WindOffBow;
            if (set)
                wantRoll += Mathf.Sign(off) * Mathf.Min(10f, 0.05f * s.windSpeed * s.windSpeed * Mathf.Abs(Mathf.Sin(off * Mathf.Deg2Rad)));
            float ease = snap ? 1f : 1f - Mathf.Exp(-dt * 4f);
            height = Mathf.Lerp(height, wantY, ease);
            pitch = Mathf.Lerp(pitch, wantPitch, ease);
            roll = Mathf.Lerp(roll, wantRoll, ease);
            Root.localPosition = new Vector3(0f, height, 0f);
            Root.localRotation = Quaternion.Euler(pitch, s.heading, roll);

            // the sail: set and braced round the mast to the wind, or furled on its yard
            if (SailOpen != null && !SailDriven)
                SailOpen.gameObject.SetActive(set);
            if (SailFurled != null && !SailDriven)
                SailFurled.gameObject.SetActive(!set && s.hasSail);
            if (set && SailOpen != null)
            {
                // (she cannot point higher than her class does: closer than that the chart is beating her to windward)
                float shown = Mathf.Sign(off) * Mathf.Max(Mathf.Abs(off), s.closestToWind);
                float want = Mathf.Clamp(Mathf.DeltaAngle(180f, shown) * 0.6f, -55f, 55f);
                brace = Mathf.Lerp(brace, want, snap ? 1f : 1f - Mathf.Exp(-dt * 0.8f));
                var turn = Quaternion.Euler(0f, brace, 0f);
                var mast = new Vector3(0f, 0f, kit.mastAhead);
                SailOpen.localRotation = turn;
                SailOpen.localPosition = mast - turn * mast;
            }

            // the oars: out and pulling under oars, else shipped along the boards
            // (rowed in to a berth, the oars are shipped for the last boat's length: her port oars would go through the jetty)
            // (a crew rig that drives the oars keeps the stroke itself and says where it stands: Driven)
            if (OarsDriven)
                return;
            bool rowing = s.underOars && s.UnderWay && !(s.berthing && s.berthDistance < Length);
            if (rowing)
                StrokePhase = Mathf.Repeat(StrokePhase + dt * (s.timeFactor > 0f ? 1f : 0f) * strokesPerMinute / 60f, 1f);
            oarsOut = Mathf.MoveTowards(oarsOut, rowing ? 1f : 0f, snap ? 1f : dt * 0.6f);
            // the blade must reach the water from the gunwale: the dip in the pull, ten degrees less on the way back
            float dipIn = Mathf.Asin(Mathf.Clamp((Gunwale + 0.2f) / Mathf.Max(kit.oarOutboard, 0.5f), 0.1f, 0.9f)) * Mathf.Rad2Deg;
            bool pull = StrokePhase < 0.4f;
            float u = pull ? StrokePhase / 0.4f : (StrokePhase - 0.4f) / 0.6f;
            float swing = Mathf.Cos(u * Mathf.PI);                                    // +1 .. -1
            float sweep = 28f * (pull ? swing : -swing);                              // forward at the catch, aft at the release
            float lift = pull ? 0f : 10f * Mathf.Sin(u * Mathf.PI);
            for (int i = 0; i < oarPivots.Count; i++)
            {
                bool starboard = i % 2 == 1;
                var list = starboard ? OarsStarboard : OarsPort;
                var oar = list[i / 2];
                if (oar == null)
                    continue;                                             // (a thwart with one oar: none on this side)
                float side = starboard ? 1f : -1f;
                // shipped: lying fore and aft on the boards beside the keel
                Vector3 stowAt = new Vector3(side * 0.2f, Deck + 0.14f, 0.05f * Length - 0.06f * (i / 2));
                Vector3 stowDir = new Vector3(0f, 0.02f, 1f).normalized;
                float sw = sweep * Mathf.Deg2Rad, dp = (dipIn - lift) * Mathf.Deg2Rad;
                Vector3 outDir = new Vector3(side * Mathf.Cos(sw) * Mathf.Cos(dp), -Mathf.Sin(dp), Mathf.Sin(sw) * Mathf.Cos(dp));
                float t = Mathf.SmoothStep(0f, 1f, oarsOut);
                Pose(oar, Vector3.Lerp(stowAt, oarPivots[i], t), Vector3.Slerp(stowDir, outDir, t));
            }
        }

        /// <summary>A crew rig that drives the oars (OarsDriven) says each step where its stroke stands and how far out
        /// its oars are, for what follows the stroke on the stage (the puddles her blades leave: StageWake).</summary>
        public void Driven(float strokePhase, float oarsAreOut)
        {
            StrokePhase = strokePhase;
            oarsOut = oarsAreOut;
        }

        /// <summary>Lays an oar (the pack's: its origin at the oar port, the blade down its local -y, the blade's breadth
        /// along its local x) with its origin at a point and its blade pointing one way, the blade's breadth upright.</summary>
        static void Pose(Transform oar, Vector3 at, Vector3 bladeTo)
        {
            Vector3 y = -bladeTo.normalized;
            Vector3 x = Vector3.up - Vector3.Dot(Vector3.up, y) * y;
            if (x.sqrMagnitude < 1e-4f)
                x = Vector3.right;
            x.Normalize();
            Vector3 z = Vector3.Cross(x, y);
            oar.localPosition = at;
            oar.localRotation = Quaternion.LookRotation(z, y);
        }
    }
}
