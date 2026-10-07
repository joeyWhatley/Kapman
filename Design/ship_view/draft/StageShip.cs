// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/StageShip.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run: the seats, the oars' sweep
// and the sail's brace are worked out from the pack's measures and want looking at in the first pictures.
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
        public float strokesPerMinute = 26f, thwartSpacing = 0.9f;

        ShipStageKit kit;
        ShipStage stage;
        float pitch, roll, height, brace, halfBeam, oarsOut;
        string fittedClass;
        float fittedLength = -1f;
        int fittedOars = -1;
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
        }

        /// <summary>Sizes her to her class and fits her out again, when the class, its length or its oars change.</summary>
        public void Fit(in ShipStageState s)
        {
            if (s.classId == fittedClass && Mathf.Abs(s.length - fittedLength) < 0.01f && s.oars == fittedOars)
                return;
            fittedClass = s.classId;
            fittedLength = s.length;
            fittedOars = s.oars;
            // (kit.scaleToClass off: the model at her own size, whatever the class: a longship, not the player's boat)
            Length = kit.scaleToClass ? Mathf.Max(2f, s.length) : kit.modelLength;
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
            int pairs = kit.fitOut ? Mathf.Clamp(s.oars / 2, 1, Mathf.Max(1, Mathf.FloorToInt(Length * 0.5f / thwartSpacing))) : 0;
            for (int i = 0; i < pairs; i++)
            {
                float z = 0.17f * Length - i * thwartSpacing;
                var thwart = stage.Spawn(kit.thwart, FitOut, "Thwart " + (i + 1), PrimitiveType.Cube, new Vector3(kit.thwartLength, 0.06f, 0.3f));
                thwart.transform.localPosition = new Vector3(0f, Deck + 0.06f, z);
                thwart.transform.localScale = Vector3.Scale(thwart.transform.localScale, new Vector3(2f * halfBeam * 0.92f / kit.thwartLength, 1f, 0.7f));
                var seat = new GameObject("Seat " + (i + 1)).transform;
                seat.SetParent(FitOut, false);
                seat.localPosition = new Vector3(0f, Deck + 0.1f, z);
                seat.localRotation = Quaternion.Euler(0f, 180f, 0f);        // a rower faces aft
                Seats.Add(seat);
                for (int side = -1; side <= 1; side += 2)
                {
                    var oar = stage.Spawn(kit.oar, FitOut, (side < 0 ? "Oar port " : "Oar starboard ") + (i + 1), PrimitiveType.Cylinder, new Vector3(0.06f, kit.oarOutboard * 0.5f, 0.06f));
                    (side < 0 ? OarsPort : OarsStarboard).Add(oar.transform);
                    oarPivots.Add(new Vector3(side * halfBeam, Gunwale + 0.03f, z - 0.3f));   // the thole lies aft of the thwart
                    var beside = new GameObject((side < 0 ? "Seat port " : "Seat starboard ") + (i + 1)).transform;
                    beside.SetParent(FitOut, false);
                    beside.localPosition = new Vector3(side * halfBeam * 0.45f, Deck + 0.1f, z);
                    beside.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    SideSeats.Add(beside);
                }
            }
            HelmSeat = new GameObject("Helm seat").transform;
            HelmSeat.SetParent(FitOut, false);
            HelmSeat.localPosition = new Vector3(0.25f, Deck + 0.1f, -0.34f * Length);
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
            bool rowing = s.underOars && s.UnderWay;
            if (rowing)
                StrokePhase = Mathf.Repeat(StrokePhase + dt * Mathf.Max(s.timeFactor, 0f) * strokesPerMinute / 60f, 1f);
            oarsOut = Mathf.MoveTowards(oarsOut, rowing ? 1f : 0f, snap ? 1f : dt * 0.6f);
            if (OarsDriven)
                return;
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
