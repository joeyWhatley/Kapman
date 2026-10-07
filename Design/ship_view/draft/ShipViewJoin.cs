// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/ShipViewJoin.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity together with the stage's drafts as they stood at 11:10 (not run).
// THE JOIN OF THE TWO HALVES: the stage (ShipStage and its parts) gives two plugs, IShipStageLook and IShipCrewRig
// (ShipStageState.cs); the look and the crew were written as components. These two classes wrap the components in the
// plugs, following the table in Design/ship_view/PLAN.md, part 9. If the stage's drafts change, this is the file to mend.
//
//   ShipStage.Current.Look = new PsxStageLook(PsxStyles.Index("console"));
//   ShipStage.Current.Crew = new PackCrewRig(menPrefabs, readTheChart: true);
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE LOOKS ON THE STAGE. Makes a ShipViewLook and a PsxSeaGrid on the stage, gives the look the stage's camera,
    /// root and water, makes the console twins of the kit's materials, and each frame hands on the sea, her way, the
    /// light of the stage's own sky and how far one can see. While it is on, the look draws the camera (the stage's own
    /// render requests stop: ShipStageView.drawnByLook) and the stage floats her on the grid sea in the styles that draw it.
    /// </summary>
    public sealed class PsxStageLook : IShipStageLook
    {
        readonly int startStyle;
        ShipStage stage;
        ShipViewLook look;
        PsxSeaGrid grid;
        Driver driver;
        readonly List<Material> made = new List<Material>();

        public PsxStageLook(int style = 0)
        {
            startStyle = style;
        }

        public string Name => look != null ? look.Style.name : PsxStyles.All[Mathf.Clamp(startStyle, 0, PsxStyles.All.Length - 1)].name;
        /// <summary>The component, for the style button: Component.Next().</summary>
        public ShipViewLook Component => look;
        public PsxSeaGrid Grid => grid;

        public void Apply(ShipStage onStage)
        {
            stage = onStage;
            var kit = stage.kit;

            var seaGo = new GameObject("PSX sea grid");
            seaGo.transform.SetParent(stage.transform, false);
            seaGo.SetActive(false);                                           // (the look switches it on in the styles that draw it)
            grid = seaGo.AddComponent<PsxSeaGrid>();
            stage.Adopt(seaGo);
            grid.level = kit.origin.y;
            grid.ship = stage.Ship.Root;
            grid.waterline = 0f;                                              // (the stage's ship root rides at the water)
            grid.hullHalfLength = stage.Ship.Length * 0.46f;
            grid.hullHalfBeam = stage.Ship.Beam * 0.5f;
            grid.hullBlend = Mathf.Max(0.6f, stage.Ship.Beam * 0.5f);

            var go = new GameObject("Ship view look");
            go.transform.SetParent(stage.transform, false);
            go.SetActive(false);
            look = go.AddComponent<ShipViewLook>();
            look.stageCamera = stage.View.Camera;
            look.stageRoot = stage.transform;
            look.hdrpSea = stage.Water != null ? stage.Water.Object : null;   // (the stage's own water mesh: off while the grid sea is drawn)
            look.gridSea = grid;
            look.swaps = Swaps(kit);
            look.lightFromClock = false;                                      // (the clock's sky is 52 N 5 E; the stage has her own: SkyNow)
            look.style = startStyle;
            look.OutputChanged += Shown;
            stage.View.drawnByLook = true;
            stage.View.ShownAtChanged += look.Show;
            stage.Changed += look.Recollect;
            driver = go.AddComponent<Driver>();
            driver.owner = this;
            go.SetActive(true);                                               // (its OnEnable applies the style)
            if (stage.View.ShownWidth > 0 && stage.View.ShownHeight > 0)
                look.Show(stage.View.ShownWidth, stage.View.ShownHeight);
            Shown(look.Output);
        }

        public void Remove(ShipStage fromStage)
        {
            if (look != null)
            {
                look.SetStyle(PsxStyles.Index("plain"));                      // (the HDRP materials and the stage's water back)
                look.OutputChanged -= Shown;
                fromStage.View.ShownAtChanged -= look.Show;
                fromStage.Changed -= look.Recollect;
                Object.Destroy(look.gameObject);
            }
            if (grid != null)
                Object.Destroy(grid.gameObject);
            foreach (var m in made)
                Object.Destroy(m);
            made.Clear();
            fromStage.View.drawnByLook = false;
            fromStage.View.shown = null;
            fromStage.HeightAt = null;
            look = null;
            grid = null;
            driver = null;
        }

        void Shown(Texture picture)
        {
            if (stage != null)
                stage.View.shown = picture;
        }

        /// <summary>For each of the kit's HDRP twins, a console twin reading the same palette (made here, at run time).</summary>
        ShipViewLook.Swap[] Swaps(ShipStageKit kit)
        {
            var psx = Shader.Find("Kapman/ShipView/PSX Model");
            if (psx == null)
                Debug.LogWarning("Kapman ship view: the shader 'Kapman/ShipView/PSX Model' is missing; the console styles keep HDRP's materials.");
            var list = new List<ShipViewLook.Swap>();
            foreach (var s in kit.swaps)
            {
                if (s.to == null)
                    continue;
                Material console = null;
                if (psx != null)
                {
                    console = new Material(psx) { name = s.from + " (console)", hideFlags = HideFlags.DontSave };
                    if (s.to.HasProperty("_BaseColorMap"))
                        console.SetTexture("_BaseMap", s.to.GetTexture("_BaseColorMap"));
                    made.Add(console);
                }
                list.Add(new ShipViewLook.Swap { source = s.from, hdrp = s.to, psx = console });
            }
            return list.ToArray();
        }

        /// <summary>The light for the console shaders from where the Sun and the Moon stand on the stage: the level
        /// clock's own recipe (LevelClock.Apply: the sky's colours by the Sun's height and the cloud), into values.</summary>
        public static void Light(in StageSkyNow sky, float overcast, float cloudLight,
                                 out Vector3 toward, out Color light, out Color zenith, out Color horizon)
        {
            float alt = sky.sunAltitude;
            float day = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-8f, 8f, alt));
            float golden = Mathf.Clamp01(1f - Mathf.Abs(alt - 3f) / 9f);
            Color zenNight = new Color(0.008f, 0.012f, 0.024f), zenDay = new Color(0.30f, 0.43f, 0.60f);
            Color horNight = new Color(0.025f, 0.032f, 0.05f), horDay = new Color(0.64f, 0.68f, 0.72f);
            Color horGold = new Color(0.98f, 0.56f, 0.30f), grey = new Color(0.55f, 0.57f, 0.60f);
            zenith = Color.Lerp(zenNight, Color.Lerp(zenDay, grey * 0.85f, overcast), day);
            horizon = Color.Lerp(Color.Lerp(horNight, Color.Lerp(horDay, grey, overcast), day), horGold, golden * 0.7f * (1f - overcast * 0.8f));
            // the Sun: about 2 when full (as LevelClock.SunColour), warm when low, less through cloud and behind the Moon
            float up = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-1f, 6f, alt));
            Color tint = Color.Lerp(new Color(1f, 0.55f, 0.30f), new Color(1f, 0.97f, 0.92f), Mathf.InverseLerp(0f, 25f, alt));
            Color sun = tint * (2f * up * Mathf.Clamp01(cloudLight) * (1f - Mathf.Clamp01(sky.sunCovered)));
            float moon = sky.moonAltitude > 0f ? Mathf.Clamp01(sky.moonLux / 0.3f) * 0.4f * Mathf.Lerp(1f, 0.3f, overcast) : 0f;
            if (sun.maxColorComponent >= moon)
            {
                toward = sky.toSun;
                light = sun;
            }
            else
            {
                toward = sky.toMoon;
                light = new Color(0.78f, 0.85f, 1f) * moon;
                zenith += new Color(0.025f, 0.03f, 0.05f) * (moon / 0.4f);
                horizon += new Color(0.03f, 0.035f, 0.05f) * (moon / 0.4f);
            }
        }

        void Tick(float dt)
        {
            if (stage == null || look == null || grid == null)
                return;
            var s = stage.State;
            var origin = stage.kit.origin;
            bool gridSea = look.Style.sea == PsxSea.Grid;

            // the grid sea: the same sea state, her way running past, her wake; and she rides it while it is drawn
            grid.SetSea(stage.Sea.State);
            float way = s.speed * Mathf.Max(0f, s.timeFactor) * dt, heading = s.heading * Mathf.Deg2Rad;
            grid.flow += new Vector2(Mathf.Sin(heading), Mathf.Cos(heading)) * way;
            grid.wake = s.moored ? 0f : Mathf.Clamp01(s.speed / 3f);
            grid.hullHalfLength = stage.Ship.Length * 0.46f;
            grid.hullHalfBeam = stage.Ship.Beam * 0.5f;
            if (gridSea && stage.HeightAt == null)
                stage.HeightAt = (east, north) => grid.Height(origin.x + east, origin.z + north) - grid.level;
            else if (!gridSea && stage.HeightAt != null)
                stage.HeightAt = null;

            // the light of the stage's own sky, and how far one sees
            Light(stage.SkyNow, s.overcast, s.cloudLight, out var toward, out var light, out var zenith, out var horizon);
            look.lightToward = toward;
            look.lightColour = light * look.sunScale;
            look.skyColour = zenith * look.skyScale;
            look.groundColour = horizon * look.groundScale;
            look.fogColour = horizon;
            look.visibility = s.visibility > 1f ? s.visibility : 20000f;
        }

        /// <summary>Runs the join once a frame, after the stage has stepped (order 0) and before the sea grid (150) and the look (210).</summary>
        [DefaultExecutionOrder(100)]
        sealed class Driver : MonoBehaviour
        {
            [System.NonSerialized] public PsxStageLook owner;

            void LateUpdate()
            {
                owner?.Tick(Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// THE CREW ON THE STAGE'S BOAT. The stage fits her out with thwarts and pairs of full-size oars and gives the
    /// seats (StageShip.Seats: one in the middle of each thwart) and where each oar turns (OarPivots). This seats one
    /// of the pack's men on each thwart pulling its pair (a hand to each oar, as in a small boat: RowingRig.Rower.oar2)
    /// and one more at the helm, and steps the RowingRig and the ShipCrew from the stage's Tick, after she has moved.
    /// The oars and the sail are then the crew's (StageShip.OarsDriven, SailDriven).
    /// The men's prefabs are not in the stage's kit: give them here (the pack's SMV_Viking_Settler_1..4, Karl_1..5).
    /// </summary>
    public sealed class PackCrewRig : IShipCrewRig
    {
        // the kit's oar (the pack's Oar_1: 0.47 m of loom inboard of its origin, the middle of its blade 1.70 m out),
        // stretched by half so it lies flat enough and reaches the middle of the boat
        const float OarInboard = 0.47f, OarBlade = 1.70f, OarStretch = 1.5f;

        readonly GameObject[] men;
        readonly bool readTheChart;
        ShipStage stage;
        StageShip ship;
        GameObject holder;
        RowingRig rig;
        ShipCrew crew;
        Transform fittedTo, helmGrip;
        Vector3 helmGripOnOar;
        int fittedSeats = -1;

        public PackCrewRig(GameObject[] menPrefabs, bool readTheChart)
        {
            men = menPrefabs ?? new GameObject[0];
            this.readTheChart = readTheChart;
        }

        public RowingRig Rig => rig;
        public ShipCrew Crew => crew;

        public void Attach(ShipStage onStage, StageShip onShip)
        {
            stage = onStage;
            ship = onShip;
            Fit();
        }

        public void Detach()
        {
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

        public void Tick(in ShipStageState state, float dt)
        {
            if (ship == null)
                return;
            // the stage fits her out anew when her class changes: seat the crew again
            if (ship.Seats.Count != fittedSeats || (ship.Seats.Count > 0 && ship.Seats[0] != fittedTo))
                Fit();
            if (crew == null || rig == null)
                return;
            if (helmGrip != null && ship.SteerOar != null)
                helmGrip.position = ship.SteerOar.TransformPoint(helmGripOnOar);
            crew.inputs = readTheChart ? ShipCrewFeed.Read(dt) : From(state);
            crew.Step(dt, Time.time);
            rig.Step(dt, Time.time);
        }

        /// <summary>The crew's inputs from the stage's record (a test's fake state; in the chart ShipCrewFeed reads more).</summary>
        public static CrewInputs From(in ShipStageState s)
        {
            return new CrewInputs
            {
                moored = s.moored, lyingTo = s.lyingTo, onRiver = s.place == StagePlace.River, nearPort = s.berthing && !s.moored,
                knots = s.speed / 0.5144f, wind = s.windSpeed, windOffBow = s.WindOffBow,
                closestToWind = s.closestToWind > 1f ? s.closestToWind : 58f,
                // the record already says which: make the rule agree with it
                sailKnots = s.hasSail && !s.underOars ? 4f : 0f, oarKnots = 2.5f,
            };
        }

        void Fit()
        {
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
                // port then starboard for each thwart (StageShip.OarPivots, OarsPort, OarsStarboard)
                if (i < ship.OarsPort.Count && 2 * i < ship.OarPivots.Count)
                    first = Oar(oars, ship.OarsPort[i], ship.OarPivots[2 * i], seat, -1f);
                if (i < ship.OarsStarboard.Count && 2 * i + 1 < ship.OarPivots.Count)
                    second = Oar(oars, ship.OarsStarboard[i], ship.OarPivots[2 * i + 1], seat, 1f);
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
            // a small boat: the thwart is a hand's breadth over the boards, the blade is small
            float seatOverBoards = fittedTo != null ? fittedTo.localPosition.y - ship.Deck : 0.1f;
            rig.footDown = seatOverBoards - 0.126f;                           // (the ankle joint is 0.126 m over the sole)
            rig.footAhead = 0.62f;
            rig.bladeDepth = 0.10f;
            rig.bladeClear = 0.22f;
            rig.catchAngle = 20f;                                             // (a pair: the handles are longer inboard, so less sweep;
            rig.finishAngle = 22f;                                            //  found by running it: Design/ship_view/check)
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
                    // his hand on the steering oar's shaft, 0.4 m over his seat, on the side nearest him
                    // (the stage turns the oar itself: crew.steerOar is left empty)
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
        }

        int Oar(List<RowingRig.Oar> oars, Transform shaft, Vector3 pivotInFitOut, Transform seat, float side)
        {
            if (shaft == null)
                return -1;
            var thole = new GameObject(side > 0f ? "Thole starboard" : "Thole port").transform;
            thole.SetParent(holder.transform, false);
            thole.position = ship.FitOut.TransformPoint(pivotInFitOut);
            thole.rotation = ship.Root.rotation * (side > 0f ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f));   // x outboard
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(thole, false);
            shaft.SetParent(pivot, false);
            shaft.localScale = new Vector3(shaft.localScale.x, shaft.localScale.y * OarStretch, shaft.localScale.z);
            // the handle's end a hand short of the middle of the boat, where his hand on that side is
            float toSeat = Mathf.Abs(ship.FitOut.InverseTransformPoint(seat.position).x - pivotInFitOut.x);
            float grip = Mathf.Max(0.3f, toSeat - 0.22f);
            float shift = OarInboard * OarStretch - (grip + 0.08f);
            oars.Add(new RowingRig.Oar
            {
                thole = thole, pivot = pivot, shaft = shaft, side = side,
                bladeOut = OarBlade * OarStretch + shift, shaftShift = shift, gripInner = grip, gripOuter = Mathf.Max(0.15f, grip - 0.28f),
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
                        t.GetChild(i).gameObject.SetActive(false);            // an axe, a sword, a shield
            }
            foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skin.updateWhenOffscreen = true;
            stage.Adopt(go);                                                  // (the stage's layer and HDRP materials)
            return go.transform;
        }
    }
}
