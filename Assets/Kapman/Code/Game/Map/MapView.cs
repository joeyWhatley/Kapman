using System;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE MAP VIEW - the 2D map, where the merchant's business is done (the user, 2026-10-01: "the nerd side of
    /// things, the 3D is strictly for immersion"). In port, "explore the town" rises from the ship into it:
    ///   1. the camera lifts off the ship and turns to look straight down on the town (eased), the scene blurring
    ///   2. the map fades in over it at the same place and scale, then settles back to show the town and its
    ///      surroundings, with the town's market docked beside it (MarketPanel, docked)
    ///   3. "back to the ship" (or Esc) plays it the other way: down to the town, the map fading, the camera
    ///      settling back behind the ship
    /// At sea, [M] opens it on the ship (no market). The map:
    ///   - a true trace of the 3D world, drawn from the same baked land (MapRenderer): silver sea, light land with
    ///     its relief, coasts and rivers' banks in ink; drag to pan, the wheel to zoom (round the pointer), from a
    ///     town to the whole world
    ///   - the fog of war over what the merchant hasn't seen (Exploration), lifting as she sails
    ///   - the ports, named; the ship, where she is and which way she heads
    ///   - click a port: how far by water and how many days, and "Set course" (the voyage planner then sails her
    ///     there by the sea lanes; Course, VoyagePlanner); the course still to sail drawn as a line
    ///   - picking a good in the market marks where it's made and wanted, and the prices seen there (the ledger)
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [DefaultExecutionOrder(1000)]                                    // after the cameras: it steers the camera in the transition
    public partial class MapView : MonoBehaviour
    {
        public StyleSheet style;
        [Tooltip("Seconds the camera takes to rise off the ship (and to settle back).")]
        public float riseSeconds = 1.6f;
        [Tooltip("Seconds the map takes to fade in over the 3D (and out).")]
        public float fadeSeconds = 0.6f;
        [Tooltip("Real metres per pixel of the map as it takes over from the camera (close).")]
        public float handoverScale = 15f;
        [Tooltip("Real metres per pixel it then settles to, on a town: the town and its surroundings.")]
        public float townScale = 22f;
        [Tooltip("Real metres per pixel at sea ([M]): the waters round her.")]
        public float seaScale = 120f;
        [Tooltip("Real metres per pixel, closest and farthest the map zooms.")]
        public float minScale = 12f, maxScale = 9000f;
        [Tooltip("The world bake the chart is drawn from (world.json and its L0..L7 tiles). Empty: the first build's bake folder.")]
        public string bakedWorldFolder = MapLand.DefaultFolder;         // (ported 4 Oct: the chart reads the bake itself)
        [Header("The chart as the game (the chart scene, 4 Oct 2026)")]
        [Tooltip("On in the chart scene: the map is the game's screen. It is open from the start, follows the ship, takes no camera " +
                 "and holds no focus (the cards and the port's menu come up over it); a town's market docks beside it and gives way to the sea again.")]
        public bool chartMode;
        [Tooltip("Real metres per pixel the chart shows the sea at in chart mode.")]
        public float chartScale = 400f;

        public static MapView Current { get; private set; }
        /// <summary>True from opening till the camera is back behind the ship.</summary>
        public bool IsOpen => state != State.Shut;
        /// <summary>Is the player away from the sea, at the map table, so that what happens at sea must wait for them?
        /// In the 3D levels: while the map is open. On the chart never: the chart is the sea, and what happens there
        /// floats over it (7 Oct 2026: on the chart the map is always open, and raiders, guard boats, traders met and
        /// the events all waited for it to shut, for ever).</summary>
        public static bool AwayFromSea => Current != null && Current.IsOpen && !Current.chartMode;
        /// <summary>The chest is open on the map (the clock waits: PanelClock).</summary>
        public bool ChestOpen => table != null && table.ChestOpen;
        /// <summary>THE FRAME IS UP (7 Oct 2026, Game/UI/Frame): the game is run from the frame and the chart is a picture
        /// to read. The map's own side panels (the title card, the top bar, the outliner, the chronicle), its bottom tool
        /// row and the port's card stand down (they are hidden, not gone), and it leaves [C], [D] and [Esc] to the frame.
        /// The chest ([I]) and the pin and the string ([P], [S]) stay.</summary>
        public bool Framed { get; set; }
        /// <summary>How far the middle of what the frame leaves free lies right of the screen's middle and above it, as
        /// shares of the screen's width and height (the frame of 7 Oct 2026, late: a side on the left, a tall foot).</summary>
        public const double FramedRight = 0.118, FramedUp = 0.09;
        /// <summary>A map tool is in hand, or a measure of the dividers is still shown.</summary>
        public bool ToolInHand => tool != Tool.None || haveMeasure;

        // ---- for the frame's own drawing and tools on the chart (the measure, a course a window wants shown)
        /// <summary>The layer over the chart's picture that moves and zooms with it: an element added here, filling it, draws
        /// in the chart's own place (<see cref="ChartPx"/> gives a place its pixels).</summary>
        public VisualElement ChartOverlays => overlays;
        /// <summary>A place of the world (real metres) in the overlays' pixels, as the chart was last drawn.</summary>
        public Vector2 ChartPx(Kapman.Game.Sim.WorldXZ w) => new Vector2((float)((w.x - drawWest) / mpp), (float)((drawNorth - w.z) / mpp));
        /// <summary>The chart was clicked (pressed and let go without a drag, the left button, no map tool in hand, not on
        /// a port's or a ship's mark): where, in real metres.</summary>
        public event Action<Kapman.Game.Sim.WorldXZ> Clicked;
        /// <summary>With the frame up: a port's mark was clicked (its id). The port's own card stays down.</summary>
        public event Action<string> PortClicked;
        /// <summary>The chart game: scrolled on in past the chart's closest. Whoever shows the ship close takes over (the
        /// ship's own view comes into the middle of the screen: ShipStageHook).</summary>
        public event Action ZoomedPast;

        /// <summary>Real metres to a pixel of the chart as it is drawn now, and the scale it is closing on.</summary>
        public double Scale => mpp;
        public double ScaleWanted => targetMpp;
        /// <summary>Chart mode: the view stays on her.</summary>
        public bool Following => follow;
        /// <summary>The closest and the widest the chart zooms now (real metres to a pixel).</summary>
        public double Closest => ClosestScale;
        public double Widest => WidestFor(Free());
        /// <summary>The part of the chart's layer that shows, in the layer's own pixels: what the frame leaves free when it
        /// is up, else all of it.</summary>
        public Rect FreePart => Free();
        /// <summary>The world that shows in the free part (real metres).</summary>
        public (double west, double north, double east, double south) Showing
        {
            get
            {
                float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
                var free = Free();
                double west = cx + (free.xMin - W * centreFrac) * mpp, north = cz - (free.yMin - H * 0.5) * mpp;
                return (west, north, west + free.width * mpp, north - free.height * mpp);
            }
        }
        /// <summary>The painted chart's own bounds (real metres); false while there is none.</summary>
        public bool Painted(out double west, out double north, out double east, out double south)
        {
            west = art != null ? art.West : 0.0;
            north = art != null ? art.North : 0.0;
            east = art != null ? art.East : 0.0;
            south = art != null ? art.South : 0.0;
            return art != null;
        }

        /// <summary>Chart mode: the view goes back to her and stays on her ([F]), at the chart's own scale.</summary>
        public void Follow()
        {
            if (!chartMode || state != State.Open || town != null)
                return;
            follow = true;
            targetMpp = chartScale;
            wheelZoom = false;
            anchored = false;
        }

        /// <summary>Chart mode: moves and zooms the view so that two places both lie in the chart's free middle (between
        /// the frame's columns when it is up). She is no longer followed, until <see cref="Follow"/> or [F].</summary>
        public void ShowBoth(Kapman.Game.Sim.WorldXZ a, Kapman.Game.Sim.WorldXZ b)
        {
            if (!chartMode || state != State.Open || town != null)
                return;
            float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
            if (float.IsNaN(W) || W < 2f || float.IsNaN(H) || H < 2f)
                return;
            // (both well inside what shows: with the frame up that is what it leaves free, a side on the left and the
            // foot below taken off)
            var free = Free();
            double freeW = free.width * (Framed ? 0.74 : 0.8), freeH = free.height * (Framed ? 0.72 : 0.8);
            double need = Math.Max(Math.Abs(a.x - b.x) / freeW, Math.Abs(a.z - b.z) / freeH);
            follow = false;
            anchored = false;
            wheelZoom = false;
            mpp = targetMpp = Math.Max(ClosestScale, Math.Min(WidestFor(free), Math.Max(need, 60.0)));
            cx = (a.x + b.x) * 0.5 - (free.center.x - W * centreFrac) * mpp;
            cz = (a.z + b.z) * 0.5 + (free.center.y - H * 0.5) * mpp;
        }

        enum State { Shut, Rising, Open, Falling }
        State state;
        float t;
        Port town;                                                   // the town explored (null at sea)
        double cx, cz, mpp = 22.0, targetMpp = 22.0;                 // view centre (real m), metres per pixel
        Camera cam;
        WorldPos topWorld;
        float topHeight;
        Quaternion topRot;
        float camWeight;

        VisualElement root, layer, under, over, overlays, card, shipMark, scaleBar, chroniclePanel, foot;
        bool framedShown;                                             // the chrome stands down for the frame now
        ScrollView chronicleList;
        readonly List<VisualElement> simMarks = new List<VisualElement>();
        VisualElement shipCard;
        Label shipCardText;
        int cardShip = -1;
        // a port's card (click a port): how far by water, and setting the course there
        VisualElement portCard;
        Label portCardText;
        Button portCardSet, portCardClear;
        VisualElement portCardWords;
        string cardPort;
        readonly List<Kapman.Game.Sim.WorldXZ> planLine = new List<Kapman.Game.Sim.WorldXZ>();
        CourseLine courseLine;
        double drawWest, drawNorth;                                   // the view the overlays were last drawn for
        bool showAllShips;                                            // [G]: every ship of the world, through the fog (dev)
        int chronicleShown = -1;                                      // how many entries the list was filled with
        Image fog;
        VisualElement paper;
        /// <summary>The side of one tile of the chart's paper on the screen, in panel pixels (as .map-paper's background-size).</summary>
        public const float PaperTile = 768f;
        /// <summary>A picture of the chart shown smaller than it is by more than this is drawn smoothed (its pixels run
        /// together), not pixel by pixel: seen from far out, every fourth pixel alone made the land a hard speckle.</summary>
        public const double SmoothFrom = 1.3;
        MapTable table;                                               // the top bar, the alerts, the outliner, the chest
        Label title, subtitle, scaleText, hint;
        MapRenderer renderer;
        // the chart's picture: tiles on disk the user can redraw (MapArt), shown over the drawn chart where they reach
        MapArt art;
        bool artLooked;
        Image artOverview;
        readonly Dictionary<int, Image> artImages = new Dictionary<int, Image>();
        readonly HashSet<int> artUsed = new HashSet<int>();
        readonly List<int> artOrder = new List<int>();
        int paletteSeen;
        bool follow = true;                                           // chart mode: the view stays on the ship till it's dragged
        Button backButton;
        readonly Dictionary<MapTile, Image> images = new Dictionary<MapTile, Image>();
        readonly HashSet<MapTile> used = new HashSet<MapTile>();
        readonly Dictionary<string, (VisualElement mark, Label name, Label note)> ports = new Dictionary<string, (VisualElement, Label, Label)>();
        bool dragging;
        float centreFrac = 0.5f;                                     // where across the map its centre point is drawn
        Vector2 dragFrom;
        double dragCx, dragCz;

        void Awake()
        {
            Current = this;
            // scrolled out past the widest view of the water: up through the clouds to the map (the user, 4 Oct pm)
            OrbitCamera.ZoomedPastWidest -= FromZoom;
            OrbitCamera.ZoomedPastWidest += FromZoom;
        }

        void OnDestroy() => OrbitCamera.ZoomedPastWidest -= FromZoom;

        void FromZoom()
        {
            if (state == State.Shut && !UiFocus.Modal)
                OpenAtSea();
        }

        double zoomInPast;

        // ---- THE ZOOM (9 Oct 2026; the user: "Can you fix the map zoom"). It went in jumps of a notch, lurched and
        // slid back under the follow, and ran out past the painted chart into the grey world of the first build's
        // bake. Now the wheel names a scale and the view closes on it within a few frames; the place under the pointer
        // stays under it (or, while she is followed, she stays where she is); and in chart mode the view never
        // leaves the painted chart: its widest is where the chart still fills what shows.
        const double ZoomNotch = 1.25;                                // one notch of the wheel: a quarter wider or closer (it was 1.18: 36 notches from the widest to the closest)
        const float WheelRate = 14f, SettleRate = 2.2f;               // how fast the view closes on its scale: after the wheel; settling on a place
        bool wheelZoom, anchored;
        double anchorWx, anchorWz;
        Vector2 anchorPx;
        VisualElement hole;                                           // the frame's line round the world in the middle

        /// <summary>The part of the layer that shows, in its own pixels: the frame's hole when the frame is up (and the
        /// chart lies behind it: swapped into the ship's window it has the window to itself), else all of it.</summary>
        Rect Free()
        {
            float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
            if (float.IsNaN(W) || float.IsNaN(H) || W < 2f || H < 2f)
                return new Rect(0f, 0f, 1f, 1f);
            var all = new Rect(0f, 0f, W, H);
            if (!Framed)
                return all;
            var frame = Kapman.Game.Frame.FrameRoot.Current;
            if (frame != null && frame.Root != null && (hole == null || hole.panel == null))
                hole = frame.Root.Q<VisualElement>("viewport");
            if (hole == null || hole.panel == null)
                // (no hole to be found: the shares the frame was laid out by)
                return new Rect((float)(W * (0.5 + FramedRight) - W * 0.378), (float)(H * (0.5 - FramedUp) - H * 0.36), W * 0.756f, H * 0.72f);
            var r = hole.worldBound;
            if (float.IsNaN(r.width) || r.width < 8f || r.height < 8f)
                return all;
            Vector2 a = layer.WorldToLocal(r.min), b = layer.WorldToLocal(r.max);
            float x0 = Mathf.Max(0f, a.x), y0 = Mathf.Max(0f, a.y), x1 = Mathf.Min(W, b.x), y1 = Mathf.Min(H, b.y);
            return x1 - x0 < 8f || y1 - y0 < 8f ? all : Rect.MinMaxRect(x0, y0, x1, y1);
        }

        /// <summary>The widest the map zooms (real metres a pixel). In chart mode: where the painted chart still fills what
        /// shows, so nothing of the grey world beyond it is ever seen.</summary>
        double WidestFor(Rect free)
        {
            double widest = maxScale;
            if (chartMode && art != null && free.width > 8f && free.height > 8f)
                widest = Math.Min(widest, Math.Min((art.East - art.West) / free.width, (art.North - art.South) / free.height));
            return Math.Max(widest, ClosestScale * 4.0);
        }

        /// <summary>
        /// One frame of the open map's view: it closes on the scale that was asked for; what stays still meanwhile is
        /// the place under the pointer (the wheel), or the middle of what shows (anything else: so she, followed, keeps
        /// her place on the screen); then the follow; then, in chart mode, the view is kept on the painted chart.
        /// </summary>
        void ZoomAndFollow(float dt)
        {
            float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
            bool sized = !float.IsNaN(W) && !float.IsNaN(H) && W > 2f && H > 2f;
            var free = sized ? Free() : default;
            bool sea = chartMode && town == null;
            // (where the middle of what shows lies from the layer's own centre point: the frame's side is on the left
            // and its foot below)
            double offX = sized && sea ? free.center.x - W * centreFrac : 0.0, offY = sized && sea ? free.center.y - H * 0.5 : 0.0;
            if (sized && sea)
                targetMpp = Math.Max(ClosestScale, Math.Min(WidestFor(free), targetMpp));
            double was = mpp;
            mpp += (targetMpp - mpp) * (1.0 - Math.Exp(-dt * (wheelZoom ? WheelRate : SettleRate)));
            if (Math.Abs(targetMpp - mpp) <= targetMpp * 0.0005)
            {
                mpp = targetMpp;
                wheelZoom = false;
            }
            if (mpp != was && sized)
            {
                if (anchored)
                {
                    cx = anchorWx - (anchorPx.x - W * centreFrac) * mpp;
                    cz = anchorWz + (anchorPx.y - H * 0.5) * mpp;
                }
                else
                {
                    cx += offX * (was - mpp);
                    cz -= offY * (was - mpp);
                }
            }
            if (mpp == targetMpp)
                anchored = false;
            if (sea && follow && !dragging)
            {
                // the chart is the game: the view stays on her till the player drags it away ([F]: back to her); she
                // is kept in the middle of what shows
                var her = ShipWorld();
                double k = 1.0 - Math.Exp(-dt * 4.0);
                cx += (her.x - offX * mpp - cx) * k;
                cz += (her.z + offY * mpp - cz) * k;
            }
            if (sized && sea && art != null)
            {
                // never off the painted chart
                double halfW = free.width * 0.5 * mpp, halfH = free.height * 0.5 * mpp;
                double mx = cx + offX * mpp, mz = cz - offY * mpp;
                double loX = art.West + halfW, hiX = art.East - halfW, loZ = art.South + halfH, hiZ = art.North - halfH;
                mx = loX > hiX ? (art.West + art.East) * 0.5 : Math.Max(loX, Math.Min(hiX, mx));
                mz = loZ > hiZ ? (art.South + art.North) * 0.5 : Math.Max(loZ, Math.Min(hiZ, mz));
                cx = mx - offX * mpp;
                cz = mz + offY * mpp;
            }
        }

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            var docRoot = doc.rootVisualElement;
            root?.RemoveFromHierarchy();
            if (style != null && !docRoot.styleSheets.Contains(style))
                docRoot.styleSheets.Add(style);
            docRoot.style.position = Position.Absolute;
            docRoot.style.left = 0;
            docRoot.style.top = 0;
            docRoot.style.right = 0;
            docRoot.style.bottom = 0;
            docRoot.pickingMode = PickingMode.Ignore;

            root = new VisualElement();
            root.AddToClassList("map-root");
            floorClass = null;                                            // (a new root has no floor class yet)
            layer = new VisualElement();
            layer.AddToClassList("map-layer");
            under = Layer("map-tiles");
            over = Layer("map-tiles");
            // THE PAPER OF THE CHART (the user, 8 Oct 2026: "give the map more texture on the colours so they are not
            // so hard"): one clear tile of specks, patches and fibres over all the colours of the chart, under the fog,
            // the names and the marks (Game/UI/Map/paper.png, made by Bake/art/make_map_textures.py; laid by the style
            // sheet, .map-paper). It goes with the land as the chart is moved: Draw sets where its first tile begins.
            paper = new VisualElement { pickingMode = PickingMode.Ignore };
            paper.AddToClassList("map-paper");
            fog = new Image { scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
            fog.AddToClassList("map-fog");
            overlays = Layer("map-overlays");
            layer.Add(under);
            layer.Add(over);
            layer.Add(paper);
            layer.Add(fog);
            layer.Add(overlays);
            courseLine = new CourseLine(this) { pickingMode = PickingMode.Ignore };
            courseLine.style.position = Position.Absolute;
            courseLine.style.left = courseLine.style.top = courseLine.style.right = courseLine.style.bottom = 0;
            overlays.Add(courseLine);
            shipMark = new VisualElement { pickingMode = PickingMode.Ignore };
            shipMark.AddToClassList("map-ship");
            overlays.Add(shipMark);
            root.Add(layer);

            card = new VisualElement();
            card.AddToClassList("map-card");
            title = new Label();
            title.AddToClassList("map-title");
            subtitle = new Label();
            subtitle.AddToClassList("map-subtitle");
            card.Add(title);
            card.Add(subtitle);
            backButton = new Button(() =>
            {
                if (chartMode && town == null)
                    follow = true;                                         // (the chart is the game: back to her on the chart)
                else
                    Close();
            }) { text = chartMode ? "Follow her  (F)" : "Back to the ship  (Esc)" };
            backButton.AddToClassList("map-button");
            card.Add(backButton);
            root.Add(card);

            foot = new VisualElement();
            foot.AddToClassList("map-foot");
            scaleBar = new VisualElement();
            scaleBar.AddToClassList("map-scalebar");
            scaleText = new Label();
            scaleText.AddToClassList("map-scaletext");
            hint = new Label("drag to move · wheel to zoom");
            hint.AddToClassList("map-hint");
            foot.Add(scaleBar);
            foot.Add(scaleText);
            foot.Add(hint);
            var chronicleButton = new Button(ToggleChronicle) { text = "Chronicle  (C)" };
            chronicleButton.AddToClassList("map-button");
            chronicleButton.AddToClassList("map-foot-button");
            foot.Add(chronicleButton);
            var saveButton = new Button(() => Voyage.Say(SaveGame.Save())) { text = "Save  (F5)" };
            saveButton.AddToClassList("map-button");
            saveButton.AddToClassList("map-foot-button");
            foot.Add(saveButton);
            var loadButton = new Button(() => Voyage.Say(SaveGame.Load())) { text = "Load  (F9)" };
            loadButton.AddToClassList("map-button");
            loadButton.AddToClassList("map-foot-button");
            foot.Add(loadButton);
            BuildTools(foot);
            root.Add(foot);

            // the chronicle: what has happened so far, newest first (Data/chronicle_793.json)
            chroniclePanel = new VisualElement();
            chroniclePanel.AddToClassList("map-chronicle");
            var chronicleTitle = new Label("Chronicle");
            chronicleTitle.AddToClassList("map-title");
            chroniclePanel.Add(chronicleTitle);
            chronicleList = new ScrollView(ScrollViewMode.Vertical);
            chronicleList.AddToClassList("map-chronicle-list");
            chroniclePanel.Add(chronicleList);
            chroniclePanel.style.display = DisplayStyle.None;
            root.Add(chroniclePanel);

            // a ship's facts, when one is clicked
            shipCard = new VisualElement();
            shipCard.AddToClassList("map-ship-card");
            shipCardText = new Label();
            shipCardText.AddToClassList("map-ship-card-text");
            shipCard.Add(shipCardText);
            shipCard.RegisterCallback<ClickEvent>(_ => { cardShip = -1; shipCard.style.display = DisplayStyle.None; });
            shipCard.style.display = DisplayStyle.None;
            root.Add(shipCard);

            // a port's card, when one is clicked: by water how far, and the course
            portCard = new VisualElement();
            portCard.AddToClassList("map-ship-card");
            portCardText = new Label();
            portCardText.AddToClassList("map-ship-card-text");
            portCard.Add(portCardText);
            portCardWords = new VisualElement();                     // the month there, and the word on the goods aboard
            portCardWords.AddToClassList("map-port-words");
            portCard.Add(portCardWords);
            portCardSet = new Button(SetCourseToCard) { text = "Set course" };
            portCardSet.AddToClassList("map-button");
            portCardClear = new Button(() => { Course.Clear(); FillPortCard(); }) { text = "Clear course" };
            portCardClear.AddToClassList("map-button");
            var shut = new Button(() => { cardPort = null; portCard.style.display = DisplayStyle.None; }) { text = "Close" };
            shut.AddToClassList("map-button");
            portCard.Add(portCardSet);
            portCard.Add(portCardClear);
            portCard.Add(shut);
            portCard.style.display = DisplayStyle.None;
            root.Add(portCard);

            table?.Dispose();
            table = new MapTable(root);

            layer.RegisterCallback<PointerDownEvent>(OnDown);
            layer.RegisterCallback<PointerMoveEvent>(OnMove);
            layer.RegisterCallback<PointerUpEvent>(OnUp);
            layer.RegisterCallback<WheelEvent>(OnWheel);

            docRoot.Add(root);
            root.style.display = DisplayStyle.None;
        }

        VisualElement Layer(string cls)
        {
            var v = new VisualElement { pickingMode = PickingMode.Ignore };
            v.AddToClassList(cls);
            return v;
        }

        void OnDisable()
        {
            UiFocus.Release(this);
            table?.Dispose();
            table = null;
        }

        // ------------------------------------------------------------------------------------------ opening and closing

        /// <summary>Rises from the ship into the map over a town, with its market docked beside it.</summary>
        public void ExploreTown(Port port)
        {
            if (chartMode)
            {
                // the chart is already the screen: go to the town on it, and dock its market beside it
                if (port == null || (state != State.Open && !OpenChart()))
                    return;
                town = port;
                follow = false;
                var at = MapWorld.ToWorld(port.berth != null ? port.berth.position : port.transform.position);
                cx = at.x;
                cz = at.z;
                targetMpp = townScale;
                card.style.display = DisplayStyle.None;
                if (MarketPanel.Current != null)
                {
                    MarketPanel.Current.Open(town, docked: true);
                    marketWasOpen = true;
                }
                Labels();
                return;
            }
            if (state != State.Shut || port == null)
                return;
            town = port;
            Begin(port.berth != null ? port.berth.position : port.transform.position, townScale);
        }

        /// <summary>Rises from the ship into the map over her (at sea).</summary>
        public void OpenAtSea()
        {
            if (chartMode)
            {
                if (state == State.Shut)
                    OpenChart();
                follow = true;
                return;
            }
            if (state != State.Shut)
                return;
            town = null;
            Begin(KapmanGame.Current != null ? KapmanGame.Current.ShipPosition : Vector3.zero, seaScale);
        }

        void Begin(Vector3 where, float settleScale)
        {
            if (!Ready())
                return;
            cam = Camera.main;
            var w = MapWorld.ToWorld(where);
            cx = w.x;
            cz = w.z;
            mpp = handoverScale;
            targetMpp = settleScale;
            // the camera's height over the place where its view matches the map's at the handover
            float fov = cam != null ? cam.fieldOfView : 60f;
            float panelH = Mathf.Max(1f, root.panel != null ? root.panel.visualTree.layout.height : 1080f);
            float h = (float)(handoverScale * panelH / (2.0 * Math.Tan(fov * 0.5 * Math.PI / 180.0) * MapWorld.MetresPerUnit));
            topWorld = w;                                              // kept in world terms: the floating origin may move
            topHeight = where.y + Mathf.Max(20f, h);
            topRot = Quaternion.Euler(90f, 0f, 0f);                    // straight down, north up (as the map)
            state = State.Rising;
            t = 0f;
            UiFocus.Take(this, frost: false);
            root.style.display = DisplayStyle.Flex;
            root.style.opacity = 0f;
            card.style.display = town != null ? DisplayStyle.None : DisplayStyle.Flex;   // (in a town the market has its name and the way back)
            Labels();
        }

        /// <summary>Chart mode: the map opens at once on the ship, with no camera's flight (false while the chart's land isn't there).</summary>
        bool OpenChart()
        {
            if (!Ready())
                return false;
            town = null;
            var w = MapWorld.ToWorld(KapmanGame.Current != null ? KapmanGame.Current.ShipPosition : Vector3.zero);
            cx = w.x;
            cz = w.z;
            mpp = targetMpp = chartScale;
            state = State.Open;
            t = 1f;
            camWeight = 0f;
            follow = true;
            root.style.display = DisplayStyle.Flex;
            root.style.opacity = 1f;
            card.style.display = DisplayStyle.Flex;
            if (backButton != null)
                backButton.text = "Follow her  (F)";
            Labels();
            return true;
        }

        /// <summary>Back down to the ship.</summary>
        public void Close()
        {
            if (chartMode)
            {
                // the chart stays: a town left gives way to the sea round her again
                if (town != null)
                {
                    if (MarketPanel.Current != null && MarketPanel.Current.IsOpen)
                        MarketPanel.Current.Close();
                    town = null;
                    marketWasOpen = false;
                    targetMpp = chartScale;
                    follow = true;
                    card.style.display = DisplayStyle.Flex;
                    Labels();
                }
                return;
            }
            if (state != State.Open && state != State.Rising)
                return;
            OrbitCamera.Current?.ToWidest();                               // she comes down to the widest view of the water
            if (MarketPanel.Current != null && MarketPanel.Current.IsOpen)
                MarketPanel.Current.Close();
            // back over the place it rose from, close, then down
            var where = town != null ? (town.berth != null ? town.berth.position : town.transform.position)
                                     : (KapmanGame.Current != null ? KapmanGame.Current.ShipPosition : Vector3.zero);
            var w = MapWorld.ToWorld(where);
            fromCx = cx;
            fromCz = cz;
            fromMpp = mpp;
            toCx = w.x;
            toCz = w.z;
            state = State.Falling;
            t = 0f;
        }

        double fromCx, fromCz, fromMpp, toCx, toCz;

        bool Ready()
        {
            if (renderer != null)
                return true;
            // ported 4 Oct: the first build read the land the 3D world was streaming (LandStreamer); the rebuild's levels
            // are Unity terrains, so the chart opens the world bake itself (MapLand, the same files). No bake: a warning
            // once, and the map doesn't open.
            var land = MapLand.Shared(bakedWorldFolder);
            if (land == null)
                return false;
            renderer = new MapRenderer(land);
            return true;
        }

        // ------------------------------------------------------------------------------------------ the chronicle

        void ToggleChronicle()
        {
            bool show = chroniclePanel.style.display == DisplayStyle.None;
            chroniclePanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            card.style.display = show || town != null ? DisplayStyle.None : DisplayStyle.Flex;   // (the log takes the card's corner)
            if (show)
                FillChronicle();
        }

        /// <summary>The log, as far as the merchant knows it: the backstory (what had happened before the game began, which
        /// every merchant knows) and the entries word has reached them of since, each with when and from whom it was heard
        /// (only what you know: far-off events come weeks late, some never). Newest first; the sources and notes in its
        /// tooltip. Clicking one with a place takes the map there.</summary>
        void FillChronicle()
        {
            int shown = Chronicle.Happened * 100003 + Known.Version;
            if (chronicleShown == shown)
                return;
            chronicleShown = shown;
            chronicleList.Clear();
            var game = KapmanGame.Current;
            int startKey = game != null ? StartKey(game.StartDate) : 0;
            foreach (var e in Chronicle.SoFar())
            {
                var heard = Known.EntryFact(e.id);
                bool backstory = e.Key <= startKey;
                if (heard == null && !backstory)
                    continue;
                var row = new VisualElement();
                row.AddToClassList("map-chronicle-row");
                string how = heard == null ? "" : " · heard " + HeardWhen(heard, game) + (string.IsNullOrEmpty(heard.from) ? "" : ", " + heard.from);
                var head = new Label(e.When + (string.IsNullOrEmpty(e.place) ? "" : " · " + e.place) + how);
                head.AddToClassList("map-chronicle-when");
                var title = new Label(e.title);
                title.AddToClassList("map-chronicle-title");
                row.Add(head);
                row.Add(title);
                row.tooltip = (e.sources.Length > 0 ? "Sources: " + string.Join("; ", e.sources) : "") +
                              (string.IsNullOrEmpty(e.note) ? "" : "\n" + e.note) +
                              (e.certainty == "uncertain" ? "\n(uncertain)" : "");
                var entry = e;
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    if (entry.Where(out double x, out double z))
                    {
                        cx = x;
                        cz = z;
                        targetMpp = Math.Max(targetMpp, 400.0);                // far enough out to see where it is
                    }
                });
                chronicleList.Add(row);
            }
        }

        static int StartKey(GameDate d) => d.year * 400 + d.month * 32 + d.day;

        /// <summary>When word of something came: "today", "3 days ago" (the game days since it was learned).</summary>
        static string HeardWhen(Fact f, KapmanGame game)
        {
            double ago = game != null ? game.Day - f.learnedDay : 0.0;
            return ago < 1.0 ? "today" : ago < 2.0 ? "yesterday" : $"{ago:0} days ago";
        }

        void Labels()
        {
            var game = KapmanGame.Current;
            title.text = town != null ? town.Title : "The waters round her";
            subtitle.text = (game != null ? $"{Cap(game.Season)} {game.Year}" : "") + (Voyage.Started ? "\n" + CrewLine() : "") +
                            "\n" + CourseLine.Status(ShipWorld());
        }

        // ------------------------------------------------------------------------------------------ the course

        /// <summary>Where the ship is (real metres).</summary>
        static Kapman.Game.Sim.WorldXZ ShipWorld()
        {
            var game = KapmanGame.Current;
            var w = MapWorld.ToWorld(game != null ? game.ShipPosition : Vector3.zero);
            return new Kapman.Game.Sim.WorldXZ(w.x, w.z);
        }

        /// <summary>The port she lies at (moored inside its reach), or null.</summary>
        static string LyingAt() => Port.LyingId();

        void OpenPortCard(string id)
        {
            cardPort = id;
            cardShip = -1;
            shipCard.style.display = DisplayStyle.None;
            FillPortCard();
        }

        /// <summary>The port's card: its name, how far by water and how long, and setting (or clearing) the course.</summary>
        void FillPortCard()
        {
            var def = GameData.Port(cardPort);
            if (def == null)
            {
                portCard.style.display = DisplayStyle.None;
                return;
            }
            string text = def.Title + "\n" + StandingLine(def);
            bool bound = Course.IsSet && Course.Destination == def.id;
            bool here = LyingAt() == def.id;
            if (bound)
                text += $"\nYour course: {Course.KmLeft(ShipWorld()):0} km to go, about {Course.Days(Course.KmLeft(ShipWorld())):0.#} days";
            else if (here)
                text += "\nYou are lying here.";
            else if (Course.Plan(def.id, ShipWorld(), LyingAt(), planLine, out double km))
                text += $"\nBy water: {km:0} km, about {Course.Days(km):0.#} days";
            else
                text += "\nNo way there by water yet (its river isn't in the world yet).";
            portCardText.text = text;
            FillPortWords(def);
            portCardSet.style.display = !bound && !here && planLine.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            portCardClear.style.display = bound ? DisplayStyle.Flex : DisplayStyle.None;
            portCard.style.display = DisplayStyle.Flex;
        }

        /// <summary>The most goods a port's card says a word on.</summary>
        const int CardWords = 3;

        /// <summary>
        /// Under the port's card: the month there in one plain line (the trading year every merchant knows, and whether
        /// its water is closed), and the word on the two or three goods you carry or have asked about (the one picked in
        /// the market first, then the hold's): what you know of each at this port (Known.Word).
        /// </summary>
        void FillPortWords(PortDef def)
        {
            portCardWords.Clear();
            var ask = Known.AskNow();
            int monthNow = ask.month >= 1 && ask.month <= 12 ? ask.month : 6;
            var month = TradeYear.Of(def.region, monthNow);
            string season = month == null ? "" : (month.closed + " " + month.line).Trim();
            if (season.Length > 0)
            {
                var line = new Label(season);
                line.AddToClassList("word-season");
                portCardWords.Add(line);
            }
            var goods = new List<GoodDef>();
            var asked = GameData.Good(MarketPanel.Current != null ? MarketPanel.Current.Picked : null);
            if (asked != null)
                goods.Add(asked);
            if (Trade.Started)
                foreach (var (good, _) in Trade.Hold.Contents)
                    if (!goods.Contains(good))
                        goods.Add(good);
            for (int i = 0; i < goods.Count && i < CardWords; i++)
                portCardWords.Add(new WordLine(Known.Word(goods[i], def, ask)));
            portCardWords.style.display = portCardWords.childCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Your standing at a port as you know it: live while you lie there; else as you last found it there (and
        /// how long ago); else not known.</summary>
        static string StandingLine(PortDef def)
        {
            var game = KapmanGame.Current;
            if (LyingAt() == def.id)
                return Reputation.Line(def);
            if (Known.StandingAt(def, out double here, out double region, out double day))
            {
                double ago = game != null ? game.Day - day : 0.0;
                return $"When you last lay here, {(ago < 1.0 ? "today" : $"{ago:0} days ago")}, you were {Reputation.Word(here)}. " +
                       $"In the region: {Reputation.Word(region)}.";
            }
            return "Nobody here knows you.";
        }

        void SetCourseToCard()
        {
            if (cardPort == null)
                return;
            if (Course.SetTo(cardPort, ShipWorld(), LyingAt()))
                Voyage.Say($"Course set for {GameData.Port(cardPort)?.name}: {Course.Km:0} km, about {Course.Days(Course.Km):0.#} days." +
                           (LyingAt() != null ? " Cast off when you're ready." : ""));
            FillPortCard();
            Labels();
        }

        /// <summary>The charted course on the map: the line still to sail, from the ship to the berth.</summary>
        sealed class CourseLine : VisualElement
        {
            readonly MapView map;

            public CourseLine(MapView map)
            {
                this.map = map;
                generateVisualContent += Paint;
            }

            public static string Status(Kapman.Game.Sim.WorldXZ ship)
            {
                if (!Course.IsSet)
                    return "No course set: click a port.";
                double left = Course.KmLeft(ship);
                return $"Bound for {GameData.Port(Course.Destination)?.name}: {left:0} km to go, about {Course.Days(left):0.#} days";
            }

            void Paint(MeshGenerationContext ctx)
            {
                if (!Course.IsSet || Course.Points.Count < 2 || map.mpp <= 0.0)
                    return;
                var p = ctx.painter2D;
                p.strokeColor = new Color(0.08f, 0.08f, 0.1f, 0.9f);
                p.lineWidth = 3f;
                p.lineJoin = LineJoin.Round;
                p.lineCap = LineCap.Butt;
                // THE WAY AHEAD IN SPACED STROKES (the user, 8 Oct 2026: "make the map trail for the ship icon to be spaced
                // lines like -  -  -"). The strokes are counted from the berth back to her, so they lie still on the
                // water and she comes up to them one by one; only what is on the screen is drawn.
                line.Clear();
                line.Add(Px(ShipWorld()));
                for (int i = Mathf.Max(1, Course.Next); i < Course.Points.Count; i++)
                    line.Add(Px(Course.Points[i]));
                const float stroke = 9f, space = 11f, period = stroke + space, edge = 24f;
                float wide = contentRect.width, high = contentRect.height;
                double along = 0.0;                                       // pixels from the berth, along the line
                p.BeginPath();
                for (int i = line.Count - 1; i > 0; i--)
                {
                    Vector2 from = line[i], to = line[i - 1];
                    float len = Vector2.Distance(from, to);
                    if (len < 0.001f || float.IsNaN(len) || float.IsInfinity(len))
                        continue;
                    // the part of the leg that is on the screen (a leg can run far off it when the chart is close in)
                    float t0 = 0f, t1 = len;
                    Vector2 dir = (to - from) / len;
                    if (Clip(from.x, dir.x, -edge, wide + edge, ref t0, ref t1) && Clip(from.y, dir.y, -edge, high + edge, ref t0, ref t1))
                    {
                        // (stroke by stroke by its number from the berth: each step is a whole stroke on, so it always ends)
                        double s0 = along + t0, s1 = along + t1;
                        long k = (long)System.Math.Floor(s0 / period);
                        for (int guard = 0; k * (double)period < s1 && guard < 4000; k++, guard++)
                        {
                            double a0 = System.Math.Max(s0, k * (double)period), a1 = System.Math.Min(s1, k * (double)period + stroke);
                            if (a1 - a0 > 0.25)
                            {
                                p.MoveTo(from + dir * (float)(a0 - along));
                                p.LineTo(from + dir * (float)(a1 - along));
                            }
                        }
                    }
                    along += len;
                }
                p.Stroke();
                // the berth: a ring
                var end = Px(Course.Points[Course.Points.Count - 1]);
                p.BeginPath();
                p.Arc(end, 6f, 0f, 360f);
                p.Stroke();
            }

            readonly List<Vector2> line = new List<Vector2>();

            /// <summary>Narrows the stretch t0..t1 of a line (start + dir * t, along one axis) to where it lies between two
            /// bounds. False when none of it does.</summary>
            static bool Clip(float start, float dir, float low, float high, ref float t0, ref float t1)
            {
                if (Mathf.Abs(dir) < 1e-6f)
                    return start >= low && start <= high;
                float a = (low - start) / dir, b = (high - start) / dir;
                if (a > b)
                {
                    float swap = a;
                    a = b;
                    b = swap;
                }
                t0 = Mathf.Max(t0, a);
                t1 = Mathf.Min(t1, b);
                return t0 < t1;
            }

            Vector2 Px(Kapman.Game.Sim.WorldXZ w) =>
                new Vector2((float)((w.x - map.drawWest) / map.mpp), (float)((map.drawNorth - w.z) / map.mpp));
        }

        /// <summary>The crew and their stores, as facts: men, health, heart, days of food and drink, pay owed.</summary>
        public static string CrewLine() =>
            $"Crew {Voyage.Men} · health {Voyage.Health * 100:0}% · heart {Voyage.Morale * 100:0}% · food {Voyage.FoodDays:0.#} days · drink {Voyage.DrinkDays:0.#} days" +
            (Voyage.PayOwed > 0.01 ? $" · pay owed {Voyage.PayOwed:0.#} d" : "");

        // ------------------------------------------------------------------------------------------ each frame

        string floorClass;                                            // the floor under the text now on the root (TextFloor)

        void Update()
        {
            TextFloor.Keep(root, GetComponent<UIDocument>().panelSettings, ref floorClass);   // (no text under 18 true pixels)
            var kb = Keyboard.current;
            if (kb != null && !UiTyping.Active)
            {
                if (state == State.Shut && !UiFocus.Modal && kb.mKey.wasPressedThisFrame)
                    OpenAtSea();
                else if (state == State.Open && ToolKeys(kb))
                {
                    // ([Esc] put a tool down: the map stays open)
                }
                else if (state == State.Open && ((!Framed && kb.escapeKey.wasPressedThisFrame) || (town == null && kb.mKey.wasPressedThisFrame)))
                    Close();
                else if (state == State.Open && !Framed && kb.cKey.wasPressedThisFrame)
                    ToggleChronicle();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else if (state == State.Open && kb.gKey.wasPressedThisFrame)
                    showAllShips = !showAllShips;                          // (dev: every ship of the world, through the fog)
#endif
            }
            if (chartMode && state == State.Shut)
                OpenChart();
            // the world's colours follow the player's people (MapPalette): when they change, the pictures are read again
            if (art != null && paletteSeen != MapPalette.Version)
            {
                paletteSeen = MapPalette.Version;
                foreach (var img in artImages.Values)
                    img.RemoveFromHierarchy();
                artImages.Clear();
                artOverview?.RemoveFromHierarchy();
                artOverview = null;
                art.Recolour();
            }
            if (chartMode && state == State.Open && kb != null && !UiTyping.Active && kb.fKey.wasPressedThisFrame)
                follow = true;
            // in a town: when the docked market is closed (its own Leave, or Esc) the town is left too
            if (state == State.Open && town != null && MarketPanel.Current != null && !MarketPanel.Current.IsOpen && marketWasOpen)
                Close();
            if (state == State.Shut)
                return;
            if (root.panel != null)
                HoverCards.Attach(root, style, GetComponent<UIDocument>().panelSettings);   // (the game's own hover cards: tooltips that show in play)
            if (state == State.Open)
                table?.Update(MarketPanel.Current != null && MarketPanel.Current.Docked);
            renderer?.Pump();
            Exploration.Apply();
            float dt = Time.unscaledDeltaTime;
            switch (state)
            {
                case State.Rising:
                    t += dt / Mathf.Max(0.05f, riseSeconds);
                    camWeight = Smooth(t);
                    // the map fades in over the last part of the rise
                    float fadeStart = 1f - Mathf.Clamp01(fadeSeconds / Mathf.Max(0.05f, riseSeconds));
                    root.style.opacity = Smooth((t - fadeStart) / Mathf.Max(0.01f, 1f - fadeStart));
                    if (t >= 1f)
                    {
                        state = State.Open;
                        root.style.opacity = 1f;
                        if (town != null && MarketPanel.Current != null)
                        {
                            MarketPanel.Current.Open(town, docked: true);
                            marketWasOpen = true;
                        }
                    }
                    break;
                case State.Open:
                    camWeight = chartMode ? 0f : 1f;
                    ZoomAndFollow(dt);
                    break;
                case State.Falling:
                    t += dt / Mathf.Max(0.05f, fadeSeconds + riseSeconds);
                    float split = fadeSeconds / Mathf.Max(0.05f, fadeSeconds + riseSeconds);
                    if (t < split)
                    {
                        // the map closes in on the place and fades
                        float k = Smooth(t / split);
                        cx = fromCx + (toCx - fromCx) * k;
                        cz = fromCz + (toCz - fromCz) * k;
                        mpp = Math.Exp(Math.Log(fromMpp) + (Math.Log(handoverScale) - Math.Log(fromMpp)) * k);
                        root.style.opacity = 1f - k;
                        camWeight = 1f;
                    }
                    else
                    {
                        root.style.display = DisplayStyle.None;
                        camWeight = 1f - Smooth((t - split) / (1f - split));
                    }
                    if (t >= 1f)
                    {
                        state = State.Shut;
                        camWeight = 0f;
                        marketWasOpen = false;
                        UiFocus.Release(this);
                    }
                    break;
            }
            // the clouds the camera passes through, thickest mid-way (CloudPass draws them)
            CloudPass.Direction = state == State.Falling ? -1f : 1f;
            CloudPass.Amount = state == State.Rising || state == State.Falling ? Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)), 1.5f) : 0f;
            if (state != State.Shut && root.resolvedStyle.display != DisplayStyle.None)
                Draw();
            StandDown();
        }

        /// <summary>With the frame up (<see cref="Framed"/>) the map's own side panels, its tool row and the port's card are
        /// kept out of sight, each frame (their own code still shows and fills them, and none of it is gone); when the
        /// frame goes they come back.</summary>
        void StandDown()
        {
            if (root == null || (!Framed && !framedShown))
                return;
            if (Framed)
            {
                card.style.display = DisplayStyle.None;
                foot.style.display = DisplayStyle.None;
                chroniclePanel.style.display = DisplayStyle.None;
                portCard.style.display = DisplayStyle.None;
            }
            else
            {
                foot.style.display = DisplayStyle.Flex;
                card.style.display = town != null ? DisplayStyle.None : DisplayStyle.Flex;
            }
            if (table != null)
                table.Framed = Framed;
            root.EnableInClassList("framed", Framed);
            framedShown = Framed;
        }

        bool marketWasOpen;

        void LateUpdate()
        {
            // the camera: from where the ship's camera wants it (set this frame, before us) to straight over the place
            if (chartMode || cam == null || camWeight <= 0f)
                return;
            var tr = cam.transform;
            var top = MapWorld.ToUnity(topWorld);
            top.y = topHeight;
            tr.SetPositionAndRotation(Vector3.Lerp(tr.position, top, camWeight), Quaternion.Slerp(tr.rotation, topRot, camWeight));
        }

        // ------------------------------------------------------------------------------------------ drawing

        void Draw()
        {
            float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
            if (float.IsNaN(W) || W < 2f || renderer == null)
                return;
            // with the market docked on the right, the place sits in the middle of the map's free left side
            float wantFrac = state == State.Open && MarketPanel.Current != null && MarketPanel.Current.Docked ? 0.2f : 0.5f;
            centreFrac += (wantFrac - centreFrac) * (1f - Mathf.Exp(-Time.unscaledDeltaTime * 3f));
            double viewWest = cx - W * centreFrac * mpp, viewNorth = cz + H * 0.5 * mpp;
            int top = renderer.Land.LevelCount - 1;
            int level = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log((float)(mpp / renderer.Cell(0)), 2f) + 0.5f), 0, top);
            used.Clear();
            if (!artLooked)
            {
                artLooked = true;
                art = MapArt.Shared();
            }
            if (art != null)
            {
                // the chart's picture where it reaches; beyond it, the drawn chart (coarse: it's far water)
                Tiles(Math.Min(top, level + 2), under, viewWest, viewNorth, W, H);
                ArtTiles(viewWest, viewNorth, W, H);
            }
            else
            {
                Tiles(Math.Min(top, level + 2), under, viewWest, viewNorth, W, H);   // a coarser underlay while the finer draw
                Tiles(level, over, viewWest, viewNorth, W, H);
            }
            // tiles no longer wanted
            var gone = new List<MapTile>();
            foreach (var kv in images)
                if (!used.Contains(kv.Key))
                    gone.Add(kv.Key);
            foreach (var g in gone)
            {
                images[g].RemoveFromHierarchy();
                images.Remove(g);
            }
            // the paper goes with the land (it is laid in screen pixels, so its grain is one size at every zoom)
            if (paper != null)
            {
                float px = Mathf.Round((float)(-(viewWest / mpp) % PaperTile)), py = Mathf.Round((float)((viewNorth / mpp) % PaperTile));
                paper.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, new Length(px));
                paper.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, new Length(py));
            }
            // the fog over the unknown
            if (Exploration.Ready)
            {
                var (fw, fn, fe, fs) = Exploration.Extent;
                fog.image = Exploration.Texture;
                PlaceCut(fog, fw, fn, fe, fs, viewWest, viewNorth, W, H);
            }
            drawWest = viewWest;
            drawNorth = viewNorth;
            courseLine.MarkDirtyRepaint();
            Ports(viewWest, viewNorth, W, H);
            SimShipMarks(viewWest, viewNorth, W, H);
            NotePins(viewWest, viewNorth, W, H);
            HuntLandmarks(viewWest, viewNorth, W, H);
            ToolsDraw();
            Ship(viewWest, viewNorth);
            ScaleBar();
            if (town == null && Time.frameCount % 30 == 0)
                Labels();
            if (chroniclePanel.style.display == DisplayStyle.Flex)
                FillChronicle();                                        // (new entries as their days come)
        }

        void Tiles(int level, VisualElement parent, double viewWest, double viewNorth, float W, float H)
        {
            var lv = renderer.Land.Levels[level];
            double size = MapRenderer.Size * lv.cell;
            int c0 = (int)Math.Floor((viewWest - lv.west) / size), c1 = (int)Math.Floor((viewWest + W * mpp - lv.west) / size);
            int r0 = (int)Math.Floor((lv.north - viewNorth) / size), r1 = (int)Math.Floor((lv.north - (viewNorth - H * mpp)) / size);
            if ((c1 - c0 + 1) * (r1 - r0 + 1) > 400)
                return;                                              // (never: the level follows the zoom)
            for (int r = r0; r <= r1; r++)
                for (int c = c0; c <= c1; c++)
                {
                    if (c < 0 || r < 0 || c >= lv.width / MapRenderer.Size + 1 || r >= lv.height / MapRenderer.Size + 1)
                        continue;
                    var tile = renderer.Get(level, c, r);
                    if (!tile.ready)
                        continue;
                    if (!images.TryGetValue(tile, out var img))
                    {
                        img = new Image { image = tile.texture, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
                        img.AddToClassList("map-tile");
                        parent.Add(img);
                        images[tile] = img;
                    }
                    used.Add(tile);
                    var (w, n, s) = renderer.Bounds(level, c, r);
                    Place(img, (w - viewWest) / mpp, (viewNorth - n) / mpp, s / mpp + 0.6, s / mpp + 0.6);
                }
        }

        /// <summary>The chart's picture (MapArt): its overview under everything it covers, and close in, the tiles themselves.</summary>
        void ArtTiles(double viewWest, double viewNorth, float W, float H)
        {
            art.Pump();
            if (art.Overview != null)
            {
                if (artOverview == null)
                {
                    artOverview = new Image { image = art.Overview, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
                    artOverview.AddToClassList("map-tile");
                    over.Add(artOverview);
                }
                Smooth(art.Overview, mpp > (art.East - art.West) / Mathf.Max(1, art.Overview.width) * SmoothFrom);
                PlaceCut(artOverview, art.West, art.North, art.East, art.South, viewWest, viewNorth, W, H);
            }
            artUsed.Clear();
            // closer than a quarter of the picture's coarsest grain the tiles are shown (a screen then needs at most a few).
            // Where the picture has finer levels (MapArt's pyramid: 60 m, 30 m, and 10 m round the ports) the one that fits
            // the zoom is laid first, and a coarser one only while the finer does not cover the view (still being read,
            // open sea, no port near): in the end only what is seen is kept.
            if (mpp < art.MetresPerPixel * 4.0)
            {
                bool added = false, covered = false;
                for (int level = art.LevelFor(mpp); level >= 0 && !covered; level--)
                    covered = ArtLevel(level, viewWest, viewNorth, W, H, ref added);
                if (added)
                    ArtOrder();
            }
            // tiles off the view, or let go by the picture's store
            List<int> gone = null;
            foreach (var kv in artImages)
                if (!artUsed.Contains(kv.Key))
                    (gone ??= new List<int>()).Add(kv.Key);
            if (gone != null)
                foreach (int key in gone)
                {
                    artImages[key].RemoveFromHierarchy();
                    artImages.Remove(key);
                }
        }

        /// <summary>Lays the tiles of one level of the picture that the view shows and that are read. True when they cover
        /// all of the picture the view shows.</summary>
        bool ArtLevel(int level, double viewWest, double viewNorth, float W, float H, ref bool added)
        {
            var lv = art.LevelInfo(level);
            double size = art.TileMetresAt(level);
            int c0 = (int)Math.Floor((viewWest - art.West) / size), c1 = (int)Math.Floor((viewWest + W * mpp - art.West) / size);
            int r0 = (int)Math.Floor((art.North - viewNorth) / size), r1 = (int)Math.Floor((art.North - (viewNorth - H * mpp)) / size);
            bool covers = true;
            for (int r = Math.Max(0, r0); r <= Math.Min(lv.rows - 1, r1); r++)
                for (int c = Math.Max(0, c0); c <= Math.Min(lv.cols - 1, c1); c++)
                {
                    var tex = art.Tile(level, r, c);
                    if (tex == null)
                    {
                        covers = false;
                        continue;
                    }
                    Smooth(tex, mpp > lv.metresPerPixel * SmoothFrom);
                    int key = MapArt.Key(level, r, c);
                    if (!artImages.TryGetValue(key, out var img) || img.image != tex)
                    {
                        img?.RemoveFromHierarchy();
                        img = new Image { image = tex, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
                        img.AddToClassList("map-tile");
                        over.Add(img);
                        artImages[key] = img;
                        added = true;
                    }
                    artUsed.Add(key);
                    var (w, n) = art.Corner(level, r, c);
                    PlaceCut(img, w, n, w + size, n - size, viewWest, viewNorth, W, H);
                }
            return covers;
        }

        /// <summary>A picture of the chart drawn smoothed (shown smaller than it is: its pixels run together) or pixel by
        /// pixel (shown at its own size or larger: the pixels stay hard-edged, as they are painted).</summary>
        static void Smooth(Texture tex, bool smooth)
        {
            var want = smooth ? FilterMode.Bilinear : FilterMode.Point;
            if (tex != null && tex.filterMode != want)
                tex.filterMode = want;
        }

        /// <summary>Finer tiles lie over coarser ones, whatever order they were read in (a key's high bits are its level).</summary>
        void ArtOrder()
        {
            artOrder.Clear();
            artOrder.AddRange(artImages.Keys);
            artOrder.Sort();
            foreach (int key in artOrder)
                artImages[key].BringToFront();
        }

        /// <summary>The closest the map zooms (real metres a pixel): with the picture's finer levels, to twice its finest.</summary>
        double ClosestScale => art != null && art.LevelCount > 1 ? Math.Min(minScale, art.LevelInfo(art.LevelCount - 1).metresPerPixel * 0.5) : minScale;

        static void Place(VisualElement v, double left, double top, double width, double height)
        {
            v.style.left = (float)left;
            v.style.top = (float)top;
            v.style.width = (float)width;
            v.style.height = (float)height;
        }

        /// <summary>Places a picture of a world rectangle (real m) cut to the view, showing just that part of it: whole, the
        /// fog was a quad a million pixels wide, and UI Toolkit failed an assertion each frame the map faded in or out.</summary>
        void PlaceCut(Image img, double w, double n, double e, double s, double viewWest, double viewNorth, float W, float H)
        {
            double margin = 8.0 * mpp;
            double cw = Math.Max(w, viewWest - margin), ce = Math.Min(e, viewWest + W * mpp + margin);
            double cn = Math.Min(n, viewNorth + margin), cs = Math.Max(s, viewNorth - H * mpp - margin);
            if (ce <= cw || cn <= cs)
            {
                img.style.display = DisplayStyle.None;
                return;
            }
            img.style.display = DisplayStyle.Flex;
            // (uv from the bottom left: the picture's rows run south first)
            var uv = new Rect((float)((cw - w) / (e - w)), (float)((cs - s) / (n - s)), (float)((ce - cw) / (e - w)), (float)((cn - cs) / (n - s)));
            if (img.uv != uv)
            {
                // THE PICTURE MUST BE DRAWN AGAIN FOR ITS NEW PART (9 Oct 2026). An Image does not ask to be drawn again
                // when only its uv changes. A picture larger than the view on every side is cut to the view, so its
                // place on the screen never changes, only the part of it shown: and it stood as it was first drawn.
                // That was the fog of war: it never moved or scaled with the chart ("it's just static"), so the clear
                // round was not on the ship, nor the known waters round home.
                img.uv = uv;
                img.MarkDirtyRepaint();
            }
            Place(img, (cw - viewWest) / mpp, (viewNorth - cn) / mpp, (ce - cw) / mpp, (cn - cs) / mpp);
        }

        void Ports(double viewWest, double viewNorth, float W, float H)
        {
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            string picked = MarketPanel.Current != null ? MarketPanel.Current.Picked : null;
            var good = GameData.Good(picked);
            named.Clear();
            string lyingAt = Port.LyingId(), boundFor = Course.IsSet ? Course.Destination : null;
            string homeAt = ChartWorld.Current != null ? ChartWorld.Current.homePort : null;
            foreach (var def in GameData.Ports)
            {
                if (!def.Exists(year))
                    continue;
                var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
                double px = (x - viewWest) / mpp, py = (viewNorth - z) / mpp;
                bool known = Exploration.Seen(x, z) > 0.35f;
                bool heardOf = !known && Known.IsHeardOf(def);              // (heard of, never seen: faded through the fog)
                bool show = (known || heardOf) && px > -200 && px < W + 200 && py > -60 && py < H + 60;
                if (show)
                    // (whose name is written first where two would lie over one another: where she lies or is bound,
                    // home, the ports she has seen, the ports only heard of; then as the table has them)
                    named.Add((def.id, (float)px, (float)py,
                               def.id == lyingAt || def.id == boundFor || def.id == cardPort ? 0 : def.id == homeAt ? 1 : known ? 2 : 3, named.Count));
                if (!ports.TryGetValue(def.id, out var p))
                {
                    if (!show)
                        continue;
                    var mark = new VisualElement { pickingMode = PickingMode.Position };   // click: the port's card
                    mark.AddToClassList("map-port");
                    string portId = def.id;
                    mark.RegisterCallback<ClickEvent>(e =>
                    {
                        if (Framed)
                            PortClicked?.Invoke(portId);                   // (the frame's Voyage window takes it, or its measure)
                        else
                            OpenPortCard(portId);
                        e.StopPropagation();
                    });
                    var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                    dot.AddToClassList("map-port-dot");
                    var name = new Label(def.name) { pickingMode = PickingMode.Ignore };
                    name.AddToClassList("map-port-name");
                    var noteLabel = new Label { pickingMode = PickingMode.Ignore };
                    noteLabel.AddToClassList("map-port-note");
                    mark.Add(dot);
                    var words = new VisualElement { pickingMode = PickingMode.Ignore };
                    words.Add(name);
                    words.Add(noteLabel);
                    mark.Add(words);
                    overlays.Add(mark);
                    p = (mark, name, noteLabel);
                    ports[def.id] = p;
                }
                p.mark.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (!show)
                    continue;
                p.mark.style.left = (float)px - 5f;
                p.mark.style.top = (float)py - 5f;
                p.mark.EnableInClassList("here", town != null && town.id == def.id);
                p.mark.EnableInClassList("heard", heardOf);
                ModePaint(def, p.name);                                   // (the chart's other readings: MapView.Modes.cs)
                // the good picked in the market: where it's made and wanted, and what it fetched where seen
                string note = "";
                if (good != null)
                {
                    float made = good.MadeIn(def.region), wanted = good.WantedIn(def.region);
                    if (made > 0f) note = made >= 2f ? "made here, plenty" : "made here";
                    if (wanted > 0f) note = (note.Length > 0 ? note + " · " : "") + (wanted >= 2f ? "much wanted" : "wanted");
                    if (Trade.Started)
                        foreach (var seen in Trade.Ledger.Seen(good.id))
                            if (seen.port == def.id)
                            {
                                var zone = TradeRules.Current.ZoneOf(def.region);
                                note = (note.Length > 0 ? note + " · " : "") + $"pays {zone.Show(seen.price.pays)}";
                                break;
                            }
                }
                p.note.text = note;
                p.note.style.display = note.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
            NameWhatFits();
            ModeKeyUpdate();
        }

        readonly List<(string id, float x, float y, int rank, int order)> named = new List<(string, float, float, int, int)>();
        readonly List<Rect> slips = new List<Rect>();
        readonly Dictionary<string, Vector2> slipSize = new Dictionary<string, Vector2>();

        /// <summary>
        /// THE PORTS' NAMES, NONE OVER ANOTHER (9 Oct 2026: far out they lay in a heap nobody could read). Every port
        /// that shows keeps its dot. Its name is written if its slip is clear of the slips already written and of the
        /// other ports' dots: first where she lies and where she is bound, then home (these three whatever dots are
        /// near), then the ports she has seen, then those only heard of. A port left without its name says it when the
        /// pointer rests on its dot.
        /// </summary>
        void NameWhatFits()
        {
            // (the same order every frame, so no two names take turns: by their rank, then as the table has the ports)
            named.Sort((a, b) => a.rank != b.rank ? a.rank.CompareTo(b.rank) : a.order.CompareTo(b.order));
            slips.Clear();
            foreach (var n in named)
                slips.Add(new Rect(n.x - 6f, n.y - 6f, 12f, 12f));          // (every dot's own place is taken)
            int dots = slips.Count;
            for (int i = 0; i < named.Count; i++)
            {
                var n = named[i];
                var p = ports[n.id];
                var words = p.name.parent;
                // the slip's size, as it was last laid out (while it is hidden it is not measured: guessed by its letters)
                float w = p.name.resolvedStyle.width, h = p.name.resolvedStyle.height;
                if (words.resolvedStyle.display != DisplayStyle.None && !float.IsNaN(w) && w > 6f && !float.IsNaN(h) && h > 6f)
                    slipSize[n.id] = new Vector2(w, h);
                else if (slipSize.TryGetValue(n.id, out var kept))
                {
                    w = kept.x;
                    h = kept.y;
                }
                else
                {
                    w = 16f + 9.5f * p.name.text.Length;
                    h = 24f;
                }
                // (the slip stands right of the dot and a little above it: MapView.uss .map-port-name)
                var slip = new Rect(n.x + 9f, n.y - 12f, w + 2f, h + 1f);
                bool clear = true;
                // (where she lies, where she is bound and home are named whatever dots lie under their slips: far out
                // the Rhine's mouths are a dozen dots in a thumb's breadth, and hers is the one name wanted there)
                for (int k = n.rank <= 1 ? dots : 0; k < slips.Count && clear; k++)
                    clear = (k < dots && k == i) || !slips[k].Overlaps(slip);
                words.style.display = clear ? DisplayStyle.Flex : DisplayStyle.None;
                p.mark.tooltip = clear ? null : p.name.text;
                if (clear)
                    slips.Add(slip);
                // (a name written whatever dots are near lies over them, not under: its mark is drawn last)
                if (n.rank <= 1 && clear)
                {
                    if (fronted.Add(n.id))
                        p.mark.BringToFront();
                }
                else
                    fronted.Remove(n.id);
            }
        }

        readonly HashSet<string> fronted = new HashSet<string>();

        /// <summary>
        /// The world's other ships (the simulation): those within sight of yours (or all, with [G]), each a small hull turned
        /// to her heading, moved smoothly between the hourly steps. Click one for her facts.
        /// </summary>
        void SimShipMarks(double viewWest, double viewNorth, float W, float H)
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            int used = 0;
            if (world != null)
            {
                var me = game.ship != null ? MapWorld.ToWorld(game.ship.transform.position) : default;
                // what her lookout sees: to where a sail clears the horizon, no further than the air and the night allow
                var (myLat, myLon) = WorldFrame.ToLatLon(me.x, me.z);
                // ported 4 Oct: the rebuild has one weather for the level (LevelWeather), no fog banks: the lookout's haze
                // is the level's fog density turned into a visibility (Koschmieder: 3.912 / extinction), capped as before
                var weatherNow = LevelWeather.Current;
                double haze = weatherNow != null ? Math.Min(40.0, 3.912 / Math.Max(1e-5, weatherNow.FogDensity) / 1000.0) : 40.0;
                var clock = LevelClock.Current;                            // (the level's clock: the game's reads it)
                double sun = clock != null ? Kapman.Game.Sim.Sight.SunElevation(myLat, clock.DayOfYear, clock.HourOfDay) : 30.0;
                double moon = clock != null ? Kapman.Game.Sim.Sight.MoonLit(clock.JulianDay) : 0.5;
                float t = Mathf.Clamp01((float)(game.Hours - (world.Hours - 1.0)));
                foreach (var s in world.ships)
                {
                    if (s.state != Kapman.Game.Sim.ShipState.Sailing && s.state != Kapman.Game.Sim.ShipState.Anchored)
                        continue;
                    double x = s.prevPos.x + (s.pos.x - s.prevPos.x) * t, z = s.prevPos.z + (s.pos.z - s.prevPos.z) * t;
                    double d2 = (x - me.x) * (x - me.x) + (z - me.z) * (z - me.z);
                    if (!showAllShips)
                    {
                        double sightM = 1000.0 * Kapman.Game.Sim.Sight.RangeKm(1.8, s.cls.SeenHeight, haze, sun, moon);
                        if (d2 > sightM * sightM)
                            continue;
                        // (ported 4 Oct: the first build then checked the fog banks on the line between; the rebuild's
                        // weather is even over the level, so the haze above is the whole of it)
                    }
                    double px = (x - viewWest) / mpp, py = (viewNorth - z) / mpp;
                    if (px < -20 || px > W + 20 || py < -20 || py > H + 20)
                        continue;
                    if (used >= simMarks.Count)
                    {
                        var mark = new VisualElement();
                        mark.AddToClassList("map-sim-ship");
                        mark.RegisterCallback<ClickEvent>(e =>
                        {
                            cardShip = (int)mark.userData;
                            cardPort = null;
                            portCard.style.display = DisplayStyle.None;
                            e.StopPropagation();
                        });
                        overlays.Add(mark);
                        simMarks.Add(mark);
                    }
                    var m = simMarks[used++];
                    m.userData = s.index;
                    m.style.display = DisplayStyle.Flex;
                    m.style.left = (float)px - 7f;
                    m.style.top = (float)py - 3f;
                    m.style.rotate = new Rotate(new Angle((float)s.heading - 90f, AngleUnit.Degree));
                    m.EnableInClassList("picked", s.index == cardShip);
                }
            }
            for (int i = used; i < simMarks.Count; i++)
                simMarks[i].style.display = DisplayStyle.None;
            ShipCard(world);
        }

        /// <summary>The clicked ship's facts, as far as they're known: a sail only sighted is just a sail; made out, her
        /// kind and home and what she's doing; once spoken to, where she's bound and what she carries.</summary>
        void ShipCard(Kapman.Game.Sim.WorldSim world)
        {
            if (world == null || cardShip < 0 || cardShip >= world.ships.Count)
            {
                shipCard.style.display = DisplayStyle.None;
                return;
            }
            var s = world.ships[cardShip];
            var seen = Kapman.Game.Sim.SeaWatch.Current != null ? Kapman.Game.Sim.SeaWatch.Current.Of(s) : Kapman.Game.Sim.SeaWatch.Seen.MadeOut;
            if (!showAllShips && seen != Kapman.Game.Sim.SeaWatch.Seen.MadeOut)
            {
                shipCardText.text = "A sail\nToo far off to make out what she is.";
                shipCard.style.display = DisplayStyle.Flex;
                return;
            }
            if (!showAllShips && !Known.HasSpoken(s.id))
            {
                shipCardText.text = Cap(Kapman.Game.Sim.SeaWatch.Label(s)) +
                                    (s.raider ? "\nRaiders" : s.state == Kapman.Game.Sim.ShipState.Anchored ? "\nAt anchor" : "\nUnder way") +
                                    "\n" + (s.hold.Used > s.hold.capacity * 0.4f ? "She sits deep: laden" : "She rides high: light") +
                                    "\nWhere she's bound and what she carries: not known till you speak to her";
                shipCard.style.display = DisplayStyle.Flex;
                return;
            }
            var home = GameData.Ports[s.home];
            string bound = s.state == Kapman.Game.Sim.ShipState.Sailing && s.dest >= 0 ? "Bound for " + GameData.Ports[s.dest].name
                         : s.state == Kapman.Game.Sim.ShipState.Anchored && s.dest >= 0 ? "At anchor off " + GameData.Ports[s.dest].name + ", waiting for a berth"
                         : s.state == Kapman.Game.Sim.ShipState.Waiting && s.dest >= 0 && s.windBound ? "At " + GameData.Ports[s.at].name + ", waiting for a fair wind for " + GameData.Ports[s.dest].name
                         : s.state == Kapman.Game.Sim.ShipState.Waiting && s.dest >= 0 ? "At " + GameData.Ports[s.at].name + ", casting off for " + GameData.Ports[s.dest].name
                         : s.state == Kapman.Game.Sim.ShipState.Lost ? "Lost" : "In port at " + GameData.Ports[s.at].name;
            shipCardText.text = $"{Kapman.Game.Sim.SeaWatch.ShortName(s.cls)}\nOf {home.name}\n{bound}" +
                                (s.state == Kapman.Game.Sim.ShipState.Sailing ? $", {world.Day - s.departedDay:0.#} days out from {GameData.Ports[s.at].name}" : "") +
                                (string.IsNullOrEmpty(s.cargoNote) ? "\nNo cargo" : "\nCargo: " + s.cargoNote.Replace("_", " ")) +
                                (showAllShips ? $"\n{s.voyages} voyages this year (dev)" : "\nAs she told you when you spoke");
            shipCard.style.display = DisplayStyle.Flex;
        }

        readonly List<VisualElement> notePins = new List<VisualElement>();
        readonly List<(VisualElement mark, Label name)> landmarkMarks = new List<(VisualElement, Label)>();

        /// <summary>The landmarks of the hoard hunts under way, close in (below 600 m a pixel): where the clue's string runs from.</summary>
        void HuntLandmarks(double viewWest, double viewNorth, float W, float H)
        {
            int used = 0;
            if (mpp < 600.0)
                foreach (var h in Hoards.Open)
                {
                    if (Hoards.Lifted(h.id))
                        continue;
                    foreach (var l in h.landmarks)
                    {
                        var (x, z) = WorldFrame.ToWorld(l.lat, l.lon);
                        double px = (x - viewWest) / mpp, py = (viewNorth - z) / mpp;
                        if (px < -40 || px > W + 40 || py < -20 || py > H + 20)
                            continue;
                        if (used >= landmarkMarks.Count)
                        {
                            var mark = new VisualElement { pickingMode = PickingMode.Ignore };
                            mark.AddToClassList("map-landmark");
                            var name = new Label { pickingMode = PickingMode.Ignore };
                            name.AddToClassList("map-landmark-name");
                            overlays.Add(mark);
                            overlays.Add(name);
                            landmarkMarks.Add((mark, name));
                        }
                        var (m, label) = landmarkMarks[used++];
                        m.style.display = DisplayStyle.Flex;
                        m.style.left = (float)px - 4f;
                        m.style.top = (float)py - 4f;
                        label.style.display = DisplayStyle.Flex;
                        label.style.left = (float)px + 7f;
                        label.style.top = (float)py - 8f;
                        label.text = l.name;
                    }
                }
            for (int i = used; i < landmarkMarks.Count; i++)
            {
                landmarkMarks[i].mark.style.display = DisplayStyle.None;
                landmarkMarks[i].name.style.display = DisplayStyle.None;
            }
        }

        /// <summary>The merchant's notes pinned to the map: a small mark each; hover for the note.</summary>
        void NotePins(double viewWest, double viewNorth, float W, float H)
        {
            int used = 0;
            foreach (var n in Chest.Notes)
            {
                if (double.IsNaN(n.x))
                    continue;
                double px = (n.x - viewWest) / mpp, py = (viewNorth - n.z) / mpp;
                if (px < -10 || px > W + 10 || py < -10 || py > H + 10)
                    continue;
                if (used >= notePins.Count)
                {
                    var pin = new VisualElement { pickingMode = PickingMode.Position };
                    pin.AddToClassList("map-note-pin");
                    overlays.Add(pin);
                    notePins.Add(pin);
                }
                var m = notePins[used++];
                m.style.display = DisplayStyle.Flex;
                m.style.left = (float)px - 5f;
                m.style.top = (float)py - 5f;
                m.tooltip = string.IsNullOrEmpty(n.text) ? "(an empty note)" : n.text;
            }
            for (int i = used; i < notePins.Count; i++)
                notePins[i].style.display = DisplayStyle.None;
        }

        void Ship(double viewWest, double viewNorth)
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
            {
                shipMark.style.display = DisplayStyle.None;
                return;
            }
            var w = MapWorld.ToWorld(game.ship.transform.position);
            shipMark.style.display = DisplayStyle.Flex;
            shipMark.style.left = (float)((w.x - viewWest) / mpp) - 9f;
            shipMark.style.top = (float)((viewNorth - w.z) / mpp) - 4f;
            shipMark.style.rotate = new Rotate(new Angle(game.ship.Heading - 90f, AngleUnit.Degree));
        }

        void ScaleBar()
        {
            // a round distance about 120 px long
            double metres = 120.0 * mpp;
            double unit = Math.Pow(10, Math.Floor(Math.Log10(metres)));
            double nice = metres / unit >= 5 ? 5 * unit : metres / unit >= 2 ? 2 * unit : unit;
            scaleBar.style.width = (float)(nice / mpp);
            scaleText.text = nice >= 1000 ? $"{nice / 1000:0} km" : $"{nice:0} m";
        }

        // ------------------------------------------------------------------------------------------ moving the map

        void OnDown(PointerDownEvent e)
        {
            if (state != State.Open)
                return;
            if (ToolDown(e))
                return;
            if (e.button != 0 && !(tool != Tool.None && e.button == 1))
                return;                                                    // (with a tool in hand, the right button moves the map)
            var on = e.target as VisualElement;
            bool onMark = on != null && (on.ClassListContains("map-port") || on.ClassListContains("map-sim-ship") || on.ClassListContains("map-note-pin"));
            if (onMark && e.button == 0 && tool == Tool.None)
            {
                // 7 Oct 2026: a press on a mark with a click of its own (a port, a ship, a pin) is the mark's: the chart
                // does not take the pointer for a drag. It did, and the click then went to the chart's layer and never
                // to the mark: a port clicked on the chart picked nothing (found by the play test's real clicks), and
                // the chart stopped following her besides.
                pressIsClick = false;
                return;
            }
            dragging = true;
            anchored = false;                                          // (the hand has the chart now, not the wheel's place)
            // (a press that is let go where it was made is a click on the chart, unless it was on a mark with its own click)
            pressIsClick = e.button == 0 && tool == Tool.None && !onMark;
            if (!(Framed && pressIsClick))
                follow = false;                                            // (with the frame up she is followed until the chart is really dragged)
            dragFrom = e.position;
            dragCx = cx;
            dragCz = cz;
            layer.CapturePointer(e.pointerId);
        }

        void OnMove(PointerMoveEvent e)
        {
            if (ToolMove(e))
                return;
            if (!dragging)
                return;
            bool still = pressIsClick && ((Vector2)e.position - dragFrom).sqrMagnitude < 25f;
            if (!still)
                pressIsClick = false;
            if (still && Framed)
                return;                                                    // (with the frame up a press is a click until it has moved)
            follow = false;
            cx = dragCx - (e.position.x - dragFrom.x) * mpp;
            cz = dragCz + (e.position.y - dragFrom.y) * mpp;
        }

        bool pressIsClick;

        void OnUp(PointerUpEvent e)
        {
            ToolUp(e);
            if (dragging && pressIsClick && e.button == 0)
            {
                var (x, z) = PxToWorld(layer.WorldToLocal(e.position));
                Clicked?.Invoke(new Kapman.Game.Sim.WorldXZ(x, z));
            }
            pressIsClick = false;
            dragging = false;
            if (layer.HasPointerCapture(e.pointerId))
                layer.ReleasePointer(e.pointerId);
        }

        void OnWheel(WheelEvent e)
        {
            if (state != State.Open)
                return;
            float W = layer.resolvedStyle.width, H = layer.resolvedStyle.height;
            var local = layer.WorldToLocal(e.mousePosition);
            // scrolled on in at the map's closest: back down through the clouds to the ship
            if (e.delta.y < 0 && targetMpp <= ClosestScale * 1.01)
            {
                zoomInPast += -e.delta.y / 3.0;
                if (zoomInPast >= 2.0)
                {
                    zoomInPast = 0.0;
                    if (chartMode)
                        ZoomedPast?.Invoke();                              // (the chart game: on in to the ship herself, on her water)
                    else
                        Close();
                    e.StopPropagation();
                    return;
                }
            }
            else
                zoomInPast = 0.0;
            // (the wheel names the scale; the view closes on it over the next frames: ZoomAndFollow. Notches turned
            // before it is there add up)
            targetMpp = Math.Max(ClosestScale, Math.Min(WidestFor(Free()), targetMpp * Math.Pow(ZoomNotch, e.delta.y / 3.0)));
            wheelZoom = true;
            if (chartMode && follow && town == null)
                anchored = false;                                          // (she is followed: she stays where she is on the screen)
            else
            {
                // the place under the pointer stays under it
                anchored = true;
                anchorPx = local;
                anchorWx = cx + (local.x - W * centreFrac) * mpp;
                anchorWz = cz - (local.y - H * 0.5) * mpp;
            }
            e.StopPropagation();
        }

        static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
