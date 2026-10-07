// The crew half of the draft join (Design/ship_view/draft/ShipViewJoin.cs), first run in Unity on 7 Oct 2026 and
// changed to the stage as it was really built (Design/ship_view/BUILD_LOG.md, part 2).
//
//   ShipStage.Current.Crew = new PackCrewRig(ShipStage.Current.kit.men);
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE CREW ON THE STAGE'S BOAT: three of the pack's men on her thwarts, each pulling a pair of oars (a hand to
    /// each), and a fourth seated at the steering oar. The stage fits her out with thwarts and oars and gives the seats
    /// (StageShip.Seats) and where each oar turns (OarPivots); this takes the oars over (StageShip.OarsDriven), makes
    /// them long enough to row with, stands a thole pin by each, seats the men, and steps the RowingRig (the stroke) and
    /// the ShipCrew (oars or sail, and the helmsman) from the stage's own step, after she has moved.
    /// It reads nothing but the stage's record (ShipStageState), which already says whether she is under oars by the
    /// game's own rule (ChartStageSource), so the men row exactly when the chart moves her by oars.
    /// Where a thwart has one oar only (a boat too wide for pairs) its man pulls that oar with both hands.
    /// </summary>
    public sealed class PackCrewRig : IShipCrewRig, IShipStageSettles
    {
        // the kit's oar (the pack's Oar_1: 0.47 m of loom inboard of its origin, the middle of its blade 1.70 m out, its
        // blade's end 2.18 m out), stretched by half along its length so it lies flat enough and its handle reaches the
        // middle of the boat
        const float OarInboard = 0.47f, OarBlade = 1.70f, OarEnd = 2.18f;
        /// <summary>How much longer than the pack made them the oars are drawn.</summary>
        public float oarStretch = 1.5f;
        /// <summary>The oar turns this far over the rail's top (m), on a thole pin: a working boat's oars do not lie on
        /// the gunwale itself, and from this low rail the handles would otherwise pass through the rowers' thighs.</summary>
        public float tholeRise = 0.08f;
        /// <summary>The pin's height and thickness (m).</summary>
        public float pinHeight = 0.17f, pinThick = 0.045f;

        readonly GameObject[] men;
        ShipStage stage;
        StageShip ship;
        GameObject holder;
        RowingRig rig;
        ShipCrew crew;
        Transform fittedTo, helmGrip;
        Vector3 helmGripOnOar;
        int fittedSeats = -1;
        float lastHeading, turning;
        bool settle = true, haveHeading;

        public PackCrewRig(GameObject[] menPrefabs)
        {
            men = menPrefabs ?? new GameObject[0];
        }

        public RowingRig Rig => rig;
        public ShipCrew Crew => crew;
        /// <summary>How many men are aboard.</summary>
        public int Aboard => (rig != null ? rig.rowers.Length : 0) + (crew != null && crew.helmsman != null ? 1 : 0);
        /// <summary>The stroke stands still while this is set (a test's picture of one moment of it).</summary>
        public bool holdStroke;

        public void Attach(ShipStage onStage, StageShip onShip)
        {
            stage = onStage;
            ship = onShip;
            fittedSeats = -1;
            settle = true;
        }

        public void Detach()
        {
            GiveOarsBack();
            if (crew != null)
                crew.PutSteerOarBack();
            if (holder != null)
                Object.Destroy(holder);
            if (ship != null)
            {
                ship.OarsDriven = false;
                ship.SailDriven = false;
            }
            holder = null;
            rig = null;
            crew = null;
            fittedSeats = -1;
        }

        /// <summary>The next step shows the crew as if the state had long been so (the stage's Settle calls it).</summary>
        public void Settle() => settle = true;

        public void Tick(in ShipStageState state, float dt)
        {
            if (ship == null || ship.Root == null)
                return;
            // the stage fits her out anew when her class or her size changes: seat the crew again
            if (ship.Seats.Count != fittedSeats || (ship.Seats.Count > 0 && ship.Seats[0] != fittedTo))
                Fit();
            if (crew == null || rig == null)
                return;
            if (helmGrip != null && ship.SteerOar != null)
                helmGrip.position = ship.SteerOar.TransformPoint(helmGripOnOar);
            // how fast she is turning, for the steering oar
            if (haveHeading && dt > 1e-4f)
                turning = Mathf.Lerp(turning, Mathf.DeltaAngle(lastHeading, state.heading) / dt, 1f - Mathf.Exp(-dt * 3f));
            lastHeading = state.heading;
            haveHeading = true;
            crew.inputs = From(state, ship.Length, turning);
            if (settle)
            {
                crew.Settle();
                turning = 0f;
                settle = false;
            }
            // the stroke runs on real time, not the game's clock (at 3x the men do not row three times as fast), and
            // stands still while the game is paused
            float step = state.timeFactor > 0f && !holdStroke ? dt : 0f;
            crew.Step(step, Time.unscaledTime);                      // (their breathing and looking about too: the wall clock)
            rig.Step(step, Time.unscaledTime);
            ship.Driven(rig.Phase, rig.oarsOut * (rig.effort > 0.5f ? 1f : 0.5f));
        }

        /// <summary>The crew's inputs from the stage's record.</summary>
        public static CrewInputs From(in ShipStageState s, float shipLength, float turning)
        {
            return new CrewInputs
            {
                moored = s.moored, lyingTo = s.lyingTo, onRiver = s.place == StagePlace.River, nearPort = s.berthing && !s.moored,
                // (rowed in to a berth, the oars are shipped for the last boat's length: her port oars would go through the jetty)
                oarsIn = s.berthing && s.berthDistance < shipLength,
                knots = s.speed / 0.5144f, wind = s.windSpeed, windOffBow = s.WindOffBow,
                closestToWind = s.closestToWind > 1f ? s.closestToWind : 58f,
                // the record already says which (the game's own rule, ChartStageSource): make the crew's rule agree with it
                sailKnots = s.hasSail && !s.underOars && s.UnderWay ? 4f : 0f, oarKnots = s.underOars && s.UnderWay ? 2.5f : 0f,
                turning = turning,
            };
        }

        void GiveOarsBack()
        {
            if (rig == null || ship == null || ship.FitOut == null)
                return;
            foreach (var o in rig.oars)
            {
                if (o == null || o.shaft == null)
                    continue;
                o.shaft.SetParent(ship.FitOut, false);
                var sc = o.shaft.localScale;
                o.shaft.localScale = new Vector3(sc.x, sc.y / oarStretch, sc.z);
            }
        }

        void Fit()
        {
            GiveOarsBack();
            if (holder != null)
                Object.Destroy(holder);
            holder = null;
            rig = null;
            crew = null;
            helmGrip = null;
            fittedSeats = ship.Seats.Count;
            fittedTo = ship.Seats.Count > 0 ? ship.Seats[0] : null;
            if (ship.Root == null || ship.FitOut == null)
                return;

            holder = new GameObject("Crew");
            holder.transform.SetParent(ship.Root, false);
            var origin = stage.kit.origin;
            var oars = new List<RowingRig.Oar>();
            var rowers = new List<RowingRig.Rower>();
            int manIndex = 0;
            for (int i = 0; i < ship.Seats.Count; i++)
            {
                var seat = ship.Seats[i];
                int first = -1, second = -1;
                // port then starboard for each thwart (StageShip.OarPivots, OarsPort, OarsStarboard: null where a thwart has none)
                if (i < ship.OarsPort.Count && 2 * i < ship.OarPivots.Count)
                    first = Oar(oars, ship.OarsPort[i], ship.OarPivots[2 * i], seat, -1f, i);
                if (i < ship.OarsStarboard.Count && 2 * i + 1 < ship.OarPivots.Count)
                    second = Oar(oars, ship.OarsStarboard[i], ship.OarPivots[2 * i + 1], seat, 1f, i);
                var man = Man(manIndex++, "Rower " + (i + 1));
                if (man != null)
                    rowers.Add(new RowingRig.Rower { man = man, seat = seat, oar = first >= 0 ? first : second, oar2 = first >= 0 ? second : -1, seed = 1.3f + 2.1f * i });
            }

            rig = holder.AddComponent<RowingRig>();
            rig.enabled = false;                                              // (stepped from Tick, after the boat has moved)
            rig.oars = oars.ToArray();
            rig.rowers = rowers.ToArray();
            rig.effort = 0f;
            rig.oarsOut = 0f;
            // a small boat: the thwart is a hand's breadth over the boards, so the feet are nearly level with the seat and
            // the legs lie long (knees low: the handles pass over them)
            float seatOverBoards = fittedTo != null ? fittedTo.localPosition.y - ship.Deck : 0.1f;
            rig.footDown = seatOverBoards - 0.126f;                           // (the ankle joint is 0.126 m over the sole)
            rig.footAhead = 0.66f;
            rig.kneeApart = 0.24f;
            rig.kneeUp = 0.5f;
            rig.bladeDepth = 0.10f;
            rig.bladeClear = 0.18f;
            rig.catchAngle = 20f;                                             // (a pair: the handles are long inboard, so less sweep)
            rig.finishAngle = 22f;
            rig.reach = 24f;
            rig.waterHeight = p => origin.y + stage.Height(p.x - origin.x, p.z - origin.z);
            rig.Build();
            ship.OarsDriven = oars.Count > 0;

            crew = holder.AddComponent<ShipCrew>();
            crew.enabled = false;
            crew.rig = rig;
            if (ship.SailOpen != null && ship.SailFurled != null)
            {
                crew.sailSet = ship.SailOpen.gameObject;
                crew.sailFurled = ship.SailFurled.gameObject;
                ship.SailDriven = true;
            }
            if (ship.HelmSeat != null)
            {
                crew.helmsman = Man(manIndex++, "Helmsman");
                crew.helmStand = ship.HelmSeat;
                crew.helmSits = true;
                if (ship.SteerOar != null)
                {
                    crew.steerOar = ship.SteerOar;
                    // his hand on the steering oar's shaft, 0.4 m over his seat, on the side nearest him
                    Vector3 axis = ship.SteerOar.up, at = ship.SteerOar.position;
                    float t = Mathf.Abs(axis.y) > 0.05f ? (ship.HelmSeat.position.y + 0.4f - at.y) / axis.y : 0f;
                    Vector3 onShaft = at + axis * t;
                    Vector3 toHim = ship.HelmSeat.position + Vector3.up * 0.5f - onShaft;
                    // (a point of our own that is kept on the oar each Tick: the oar is the stage's, not ours to hang things on)
                    helmGrip = new GameObject("Helm grip").transform;
                    helmGrip.SetParent(holder.transform, false);
                    helmGrip.position = onShaft + toHim.normalized * (0.11f * Mathf.Max(0.3f, ship.Scale));
                    helmGripOnOar = ship.SteerOar.InverseTransformPoint(helmGrip.position);
                    crew.helmGrip = helmGrip;
                }
            }
            crew.Build();
            settle = true;
            stage.RaiseChanged();                                             // (a look finds the men and the pins)
        }

        int Oar(List<RowingRig.Oar> oars, Transform shaft, Vector3 pivotInFitOut, Transform seat, float side, int thwart)
        {
            if (shaft == null)
                return -1;
            // (the holder and the fit-out both lie at her root, unturned: the fit-out's axes are the holder's)
            Vector3 at = pivotInFitOut + Vector3.up * tholeRise;
            var thole = new GameObject(side > 0f ? "Thole starboard " + (thwart + 1) : "Thole port " + (thwart + 1)).transform;
            thole.SetParent(holder.transform, false);
            thole.localPosition = at;
            thole.localRotation = side > 0f ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);   // x outboard
            // the pin the oar pulls against: on the rail just ahead of the oar (the pull bears toward the bow)
            var pin = stage.Spawn(stage.kit.pile, holder.transform, "Thole pin", PrimitiveType.Cylinder, new Vector3(pinThick, pinHeight * 0.5f, pinThick));
            if (stage.kit.pile != null)
                pin.transform.localScale = Vector3.Scale(pin.transform.localScale, new Vector3(pinThick / stage.kit.pileSize.x, pinHeight / stage.kit.pileSize.y, pinThick / stage.kit.pileSize.x));
            pin.transform.localPosition = new Vector3(pivotInFitOut.x, pivotInFitOut.y - 0.04f + (stage.kit.pile != null ? 0f : pinHeight * 0.5f), pivotInFitOut.z + 0.055f);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(thole, false);
            shaft.SetParent(pivot, false);
            shaft.localScale = new Vector3(shaft.localScale.x, shaft.localScale.y * oarStretch, shaft.localScale.z);
            // the handle's end a hand short of the middle of the boat, where his hand on that side is
            float seatX = ship.FitOut.InverseTransformPoint(seat.position).x;
            float toSeat = Mathf.Abs(seatX - pivotInFitOut.x);
            float grip = Mathf.Max(0.3f, toSeat - 0.22f);
            float shift = OarInboard * oarStretch - (grip + 0.08f);
            // put away, she lies on the thwarts by the rail with her blade to the bow, her middle a little abaft the mast
            float inboard = OarInboard * oarStretch - shift, outboard = OarEnd * oarStretch + shift;
            Vector3 stow = new Vector3(side * (0.40f + 0.075f * thwart), seat.localPosition.y + 0.05f, -0.25f - (outboard - inboard) * 0.5f);
            Vector3 stowLocal = stow - at;
            if (side < 0f)
                stowLocal = new Vector3(-stowLocal.x, stowLocal.y, -stowLocal.z);
            oars.Add(new RowingRig.Oar
            {
                thole = thole, pivot = pivot, shaft = shaft, side = side,
                bladeOut = OarBlade * oarStretch + shift, shaftShift = shift, gripInner = grip, gripOuter = Mathf.Max(0.15f, grip - 0.28f),
                stowAt = stowLocal,
            });
            return oars.Count - 1;
        }

        Transform Man(int index, string name)
        {
            if (men.Length == 0 || men[index % men.Length] == null)
                return null;
            var go = Object.Instantiate(men[index % men.Length], holder.transform);
            go.name = name;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != "hand.L" && t.name != "hand.R")
                    continue;
                for (int i = 0; i < t.childCount; i++)
                    if (t.GetChild(i).GetComponentInChildren<Renderer>(true) != null)
                        t.GetChild(i).gameObject.SetActive(false);            // (an axe, a sword, a shield: not at the oars)
            }
            foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skin.updateWhenOffscreen = true;                               // (posed by code: the bounds of the rest pose are not his)
            stage.Adopt(go);                                                  // (the stage's layer and HDRP materials)
            return go.transform;
        }
    }
}
